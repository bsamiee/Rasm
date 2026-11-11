import { runMain } from '@effect/platform-node/NodeRuntime';
import { createProjectGraphAsync, type TaskGraph } from '@nx/devkit';
import { Array, Console, Effect, Predicate, Record, Schema } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { getTaskRawInputs } from 'nx/src/hasher/check-task-files';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { getTaskSpecificEnv } from 'nx/src/tasks-runner/task-env';
import { isCacheableTask } from 'nx/src/tasks-runner/utils';

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const waits = (edges: TaskGraph['continuousDependencies'][string] = []) => edges.filter((edge) => Predicate.isString(edge) || edge.inputs !== false).map((edge) => (Predicate.isString(edge) ? edge : edge.id));

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runMain(
        Effect.gen(function* () {
            const projectGraph = yield* Effect.tryPromise(() => createProjectGraphAsync({ exitOnError: true }));
            const nxJson = readNxJson();
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
            const hashes = yield* Effect.tryPromise(() => createTaskHasher(projectGraph, nxJson).hashTasks(tasks, planned, environments));
            const inputs = yield* Effect.forEach(tasks, (task) => Effect.tryPromise(() => getTaskRawInputs(task.id, { projectGraph, nxJson })));
            const read = Record.fromIterableWith(Array.zip(Array.zip(tasks, hashes), inputs), ([[task, hash], held]) => [task.id, { ...task.target, hash: hash.value, cache: isCacheableTask(task), waits: waits(planned.continuousDependencies[task.id]), ...held }]);
            yield* Effect.flatMap(Schema.encodeUnknownEffect(Json)(read), Console.log);
        }),
    );
}
