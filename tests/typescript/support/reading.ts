import { createRequire } from 'node:module';
import { join } from 'node:path';
import { cwd } from 'node:process';
import { pathToFileURL } from 'node:url';
import { runMain } from '@effect/platform-node/NodeRuntime';
import type { ProjectGraph } from '@nx/devkit';
import { Array, Cause, Console, Effect, Formatter, Record, Result, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Reading = Schema.fromJsonString(
    Schema.Struct({
        nodes: Schema.Record(Schema.String, Schema.Any),
        dependencies: Schema.Record(Schema.String, Schema.Any),
        externalNodes: Schema.Record(Schema.String, Schema.Any),
        sourceMaps: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Tuple([Schema.NullOr(Schema.String), Schema.String]))),
        hashes: Schema.Record(Schema.String, Schema.Struct({ value: Schema.String, inputs: Schema.Record(Schema.String, Schema.String) })),
        failures: Schema.Array(Schema.Struct({ name: Schema.String, message: Schema.String, files: Schema.Array(Schema.Struct({ file: Schema.NullOr(Schema.String), message: Schema.String })) })),
    }),
).annotate({
    description:
        'One process reading of the workspace in the working directory through the nx it installs: the project graph, or the partial graph where a plugin failed with each failure and the file and message of each file it names, the file and plugin Nx records for each project property, and the hash Nx gives every task of every target with the hash of each input',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const installed = createRequire(join(cwd(), 'package.json'));

const loaded = <A>(specifier: string) => Effect.promise((): Promise<A> => import(pathToFileURL(installed.resolve(specifier)).href));

const read = Effect.gen(function* () {
    const [internals, plugins, errors, settings, hasher, planner, environment] = yield* Effect.all([
        loaded<typeof import('nx/src/devkit-internals')>('nx/src/devkit-internals'),
        loaded<typeof import('nx/src/project-graph/plugins/get-plugins')>('nx/src/project-graph/plugins/get-plugins'),
        loaded<typeof import('nx/src/project-graph/error-types')>('nx/src/project-graph/error-types'),
        loaded<typeof import('nx/src/config/nx-json')>('nx/src/config/nx-json'),
        loaded<typeof import('nx/src/hasher/create-task-hasher')>('nx/src/hasher/create-task-hasher'),
        loaded<typeof import('nx/src/tasks-runner/create-task-graph')>('nx/src/tasks-runner/create-task-graph'),
        loaded<typeof import('nx/src/tasks-runner/task-env')>('nx/src/tasks-runner/task-env'),
    ]);
    const { projectGraph, sourceMaps, failures } = yield* Effect.tryPromise({
        try: () => internals.createProjectGraphAndSourceMapsAsync({ exitOnError: false }),
        catch: (cause) => (cause instanceof errors.ProjectGraphError ? cause : new Cause.UnknownError(cause, 'Nx refused the workspace before reading a plugin result')),
    }).pipe(
        Effect.map(({ projectGraph: graph, sourceMaps: maps }): { readonly projectGraph: ProjectGraph; readonly sourceMaps: object; readonly failures: (typeof Reading.Type)['failures'] } => ({ projectGraph: graph, sourceMaps: maps, failures: [] })),
        Effect.catchIf(
            (cause): cause is InstanceType<typeof errors.ProjectGraphError> => cause instanceof errors.ProjectGraphError,
            (refused) =>
                Effect.succeed({
                    projectGraph: refused.getPartialProjectGraph(),
                    sourceMaps: refused.getPartialSourcemaps(),
                    failures: refused.getErrors().map((error) => ({ name: error.name, message: error.message, files: errors.isAggregateCreateNodesError(error) ? error.errors.map(([file, { message }]) => ({ file, message })) : [] })),
                }),
        ),
        Effect.orDie,
        Effect.ensuring(Effect.sync(plugins.cleanupPlugins)),
    );
    const nodes = Object.values(projectGraph.nodes);
    const planned = planner.createTaskGraph(
        projectGraph,
        {},
        nodes.map((node) => node.name),
        Array.dedupe(nodes.flatMap((node) => Record.keys(node.data.targets ?? {}))),
        undefined,
        {},
    );
    const tasks = Object.values(planned.tasks);
    const hashed = yield* Effect.result(Effect.tryPromise(() => hasher.createTaskHasher(projectGraph, settings.readNxJson()).hashTasks(tasks, planned, Record.fromIterableWith(tasks, (task) => [task.id, environment.getTaskSpecificEnv(task, projectGraph)]))));
    const taken = Result.match(hashed, {
        onSuccess: (hashes) => ({ hashes: Record.fromIterableWith(Array.zip(tasks, hashes), ([task, hash]) => [task.id, { value: hash.value, inputs: hash.details.nodes }]), failures }),
        onFailure: ({ cause }) => ({ hashes: {}, failures: [...failures, { name: 'TaskHasher', message: Formatter.format(cause), files: [] }] }),
    });
    return { nodes: projectGraph.nodes, dependencies: projectGraph.dependencies, externalNodes: projectGraph.externalNodes ?? {}, sourceMaps, ...taken };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runMain(Effect.flatMap(Effect.flatMap(read, Schema.encodeUnknownEffect(Reading)), Console.log));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Reading };
