import { runMain } from '@effect/platform-node/NodeRuntime';
import type { ProjectGraph } from '@nx/devkit';
import { Array, Cause, Console, Effect, Formatter, Record, Result, Schema } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { createProjectGraphAndSourceMapsAsync } from 'nx/src/devkit-internals';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { isAggregateCreateNodesError, ProjectGraphError } from 'nx/src/project-graph/error-types';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { getTaskSpecificEnv } from 'nx/src/tasks-runner/task-env';

// --- [MODELS] --------------------------------------------------------------------------

const Reading = Schema.fromJsonString(
    Schema.Struct({
        nodes: Schema.Record(Schema.String, Schema.Any),
        dependencies: Schema.Record(Schema.String, Schema.Any),
        externalNodes: Schema.Record(Schema.String, Schema.Any),
        sourceMaps: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Tuple([Schema.NullOr(Schema.String), Schema.String]))),
        hashes: Schema.Record(Schema.String, Schema.Struct({ value: Schema.String, inputs: Schema.Record(Schema.String, Schema.String) })),
        failures: Schema.Array(Schema.Struct({ name: Schema.String, message: Schema.String, files: Schema.Array(Schema.NullOr(Schema.String)) })),
    }),
).annotate({
    description:
        'One process reading of the workspace in the working directory: the project graph Nx builds, or the partial graph where a plugin failed with each failure and the files it names, the file and plugin Nx records for each project property, and the hash Nx gives every task of every target with the hash of each input',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const graphed = Effect.tryPromise({
    try: () => createProjectGraphAndSourceMapsAsync({ exitOnError: false }),
    catch: (cause) => (cause instanceof ProjectGraphError ? cause : new Cause.UnknownError(cause, 'Nx refused the workspace before reading a plugin result')),
}).pipe(
    Effect.map(({ projectGraph, sourceMaps }): { readonly projectGraph: ProjectGraph; readonly sourceMaps: object; readonly failures: (typeof Reading.Type)['failures'] } => ({ projectGraph, sourceMaps, failures: [] })),
    Effect.catchIf(
        (cause): cause is ProjectGraphError => cause instanceof ProjectGraphError,
        (refused) =>
            Effect.succeed({
                projectGraph: refused.getPartialProjectGraph(),
                sourceMaps: refused.getPartialSourcemaps(),
                failures: refused.getErrors().map((error) => ({ name: error.name, message: error.message, files: isAggregateCreateNodesError(error) ? error.errors.map(([file]) => file) : [] })),
            }),
    ),
);

const hashed = Effect.fnUntraced(function* (projectGraph: ProjectGraph) {
    const nodes = Object.values(projectGraph.nodes);
    const planned = createTaskGraph(
        projectGraph,
        {},
        nodes.map((node) => node.name),
        Array.dedupe(nodes.flatMap((node) => Record.keys(node.data.targets ?? {}))),
        undefined,
        {},
    );
    const tasks = Object.values(planned.tasks);
    const environments = Record.fromIterableWith(tasks, (task) => [task.id, getTaskSpecificEnv(task, projectGraph)]);
    const hashes = yield* Effect.tryPromise(() => createTaskHasher(projectGraph, readNxJson()).hashTasks(tasks, planned, environments));
    return Record.fromIterableWith(Array.zip(tasks, hashes), ([task, hash]) => [task.id, { value: hash.value, inputs: hash.details.nodes }]);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runMain(
        Effect.gen(function* () {
            const { projectGraph, sourceMaps, failures } = yield* graphed;
            const taken = Result.match(yield* Effect.result(hashed(projectGraph)), {
                onSuccess: (hashes) => ({ hashes, failures }),
                onFailure: ({ cause }) => ({ hashes: {}, failures: [...failures, { name: 'TaskHasher', message: Formatter.format(cause), files: [] }] }),
            });
            yield* Effect.flatMap(Schema.encodeUnknownEffect(Reading)({ nodes: projectGraph.nodes, dependencies: projectGraph.dependencies, externalNodes: projectGraph.externalNodes ?? {}, sourceMaps, ...taken }), Console.log);
        }),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Reading };
