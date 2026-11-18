import { createRequire } from 'node:module';
import { join } from 'node:path';
import { cwd } from 'node:process';
import { pathToFileURL } from 'node:url';
import { runMain } from '@effect/platform-node/NodeRuntime';
import type { ProjectGraph } from '@nx/devkit';
import { Array, Cause, Console, Effect, Formatter, Record, Result, Schema } from 'effect';
import type { readNxJson } from 'nx/src/config/nx-json';
import type { buildProjectGraphAndSourceMapsWithoutDaemon, createProjectGraphAndSourceMapsAsync } from 'nx/src/devkit-internals';
import type { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import type { isAggregateCreateNodesError, ProjectGraphError } from 'nx/src/project-graph/error-types';
import type { cleanupPlugins } from 'nx/src/project-graph/plugins/get-plugins';
import type { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import type { getTaskSpecificEnv } from 'nx/src/tasks-runner/task-env';

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
        loaded<{ readonly createProjectGraphAndSourceMapsAsync: (options: Parameters<typeof createProjectGraphAndSourceMapsAsync>[0]) => ReturnType<typeof buildProjectGraphAndSourceMapsWithoutDaemon> }>('nx/src/devkit-internals'),
        loaded<{ readonly cleanupPlugins: typeof cleanupPlugins }>('nx/src/project-graph/plugins/get-plugins'),
        loaded<{ readonly ProjectGraphError: typeof ProjectGraphError; readonly isAggregateCreateNodesError: typeof isAggregateCreateNodesError }>('nx/src/project-graph/error-types'),
        loaded<{ readonly readNxJson: typeof readNxJson }>('nx/src/config/nx-json'),
        loaded<{ readonly createTaskHasher: typeof createTaskHasher }>('nx/src/hasher/create-task-hasher'),
        loaded<{ readonly createTaskGraph: typeof createTaskGraph }>('nx/src/tasks-runner/create-task-graph'),
        loaded<{ readonly getTaskSpecificEnv: typeof getTaskSpecificEnv }>('nx/src/tasks-runner/task-env'),
    ]);
    const { projectGraph, sourceMaps, refusals } = yield* Effect.tryPromise({
        try: () => internals.createProjectGraphAndSourceMapsAsync({ exitOnError: false }),
        catch: (cause) => (cause instanceof errors.ProjectGraphError ? cause : new Cause.UnknownError(cause, 'Nx refused the workspace before reading a plugin result')),
    }).pipe(
        Effect.map(({ projectGraph: graph, sourceMaps: maps }): { readonly projectGraph: ProjectGraph; readonly sourceMaps: object; readonly refusals: ReturnType<ProjectGraphError['getErrors']> } => ({ projectGraph: graph, sourceMaps: maps, refusals: [] })),
        Effect.catchIf(
            (cause): cause is ProjectGraphError => cause instanceof errors.ProjectGraphError,
            (refused) =>
                Effect.succeed({
                    projectGraph: refused.getPartialProjectGraph(),
                    sourceMaps: refused.getPartialSourcemaps(),
                    refusals: refused.getErrors(),
                }),
        ),
        Effect.orDie,
        Effect.ensuring(Effect.sync(plugins.cleanupPlugins)),
    );
    const failures = refusals.map((error) => ({ name: error.name, message: error.message, files: errors.isAggregateCreateNodesError(error) ? error.errors.map(([file, { message }]) => ({ file, message })) : [] }));
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
    const hashed = yield* Effect.result(
        Effect.tryPromise({
            try: () =>
                hasher.createTaskHasher(projectGraph, settings.readNxJson()).hashTasks(
                    tasks,
                    planned,
                    Record.fromIterableWith(tasks, (task) => [task.id, environment.getTaskSpecificEnv(task, projectGraph)]),
                ),
            catch: (cause) => new Cause.UnknownError(cause, 'Nx task hasher refused the task graph'),
        }),
    );
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
