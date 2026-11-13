import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { readProjectsConfigurationFromProjectGraph, type Task, type TaskGraph } from '@nx/devkit';
import { Array, Effect, Option, Record } from 'effect';
import { createTaskGraph } from 'nx/src/tasks-runner/create-task-graph';
import { assertTaskGraphDoesNotContainInvalidTargets, findCycle } from 'nx/src/tasks-runner/task-graph-utils';
import { type Batch, TasksSchedule } from 'nx/src/tasks-runner/tasks-schedule';
import { getExecutorForTask } from 'nx/src/tasks-runner/utils';
import { declared, read, verbs, type Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface Slot {
    readonly batches: readonly Batch[];
    readonly alone: readonly Task[];
    readonly ran: readonly Task[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    cycle: 'nrwl/nx packages/nx/src/tasks-runner/task-graph-utils.spec.ts findCycle: should return null when no cycle',
    invalid: 'nrwl/nx packages/nx/src/tasks-runner/task-graph-utils.spec.ts assertTaskGraphDoesNotContainInvalidTargets: should throw if a task has parallelism set to false and has continuous dependencies, or a depended-on continuous task has it',
    ready: "nrwl/nx e2e/nx/src/run.test.ts: should wait for a continuous dependency to be ready when the edge asks for it; README.md TASKS: `serve` runs a project's development servers until the run ends, where its config holds the ready signal",
    exclusive: 'nrwl/nx packages/nx/src/tasks-runner/tasks-schedule.spec.ts tasks with parallelism false: should schedule root tasks first, one at a time; should not schedule batches of tasks by different executors if task has parallelism false',
    batched: 'nrwl/nx packages/nx/src/tasks-runner/tasks-schedule.spec.ts: should schedule batches of tasks by different executors; preferBatch: should batch tasks when executor has preferBatch: true and --batch not specified',
    drained: 'nrwl/nx packages/nx/src/tasks-runner/tasks-schedule.spec.ts: should run out of tasks when they are all complete',
    packaged: 'README.md TASKS: Attach and publish targets depend on the targets that package their files (`pack--dotnet` for NuGet)',
    previewed: 'README.md TASKS: `preview` builds a project, then serves its build output as `serve` does',
} as const;
const CHECKED = ['check', 'test', 'build'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const planned = (workspace: Workspace, targets: readonly string[]) =>
    createTaskGraph(
        workspace,
        {},
        Object.values(workspace.nodes)
            .filter(({ data }) => targets.some((target) => target in (data.targets ?? {})))
            .map(({ name }) => name),
        [...targets],
        undefined,
        {},
    );

const cyclic = (workspace: Workspace, stated: readonly string[]) => stated.flatMap((verb) => Array.fromNullishOr(findCycle(planned(workspace, [verb]))).map((cycle) => [verb, ...cycle]));

const unscheduled = ({ tasks }: TaskGraph, slots: readonly Slot[]) => [...new Set(Record.keys(tasks)).difference(new Set(slots.flatMap(({ ran }) => ran.map((task) => task.id))))];

const every = (workspace: Workspace) => planned(workspace, Array.dedupe(declared(workspace).map(({ name }) => name)));

const reached = ({ dependencies, continuousDependencies }: Pick<TaskGraph, 'dependencies' | 'continuousDependencies'>, from: string, seen: ReadonlySet<string> = new Set()): ReadonlySet<string> =>
    [...(dependencies[from] ?? []), ...(continuousDependencies[from] ?? [])].reduce((held, { id }) => (held.has(id) ? held : reached({ dependencies, continuousDependencies }, id, held.union(new Set([id])))), seen);

const lacking = (tasks: TaskGraph, first: (task: Task) => boolean, needed: (task: Task, id: string) => boolean) =>
    Object.values(tasks.tasks)
        .filter((task) => first(task) && ![...reached(tasks, task.id)].some((id) => needed(task, id)))
        .map(({ id }) => id);

const unsignalled = (workspace: Workspace, { tasks, continuousDependencies }: TaskGraph) => {
    const configured = (task: string) => Array.fromNullishOr(tasks[task]?.target).flatMap(({ project, target }) => Array.fromNullishOr(workspace.nodes[project]?.data.targets?.[target]));
    return Record.collect(continuousDependencies, (dependent, edges) =>
        edges.filter(({ id, waitFor }) => configured(id).some(({ readyWhen }) => (readyWhen ?? undefined) === undefined) || (waitFor !== 'ready' && configured(dependent).some(({ executor }) => executor !== 'nx:noop'))).map(({ id, waitFor = 'started' }) => `${dependent} waits for ${id} to be ${waitFor}`),
    ).flat();
};

const awaited = ({ continuousDependencies }: TaskGraph, incomplete: readonly Task[]) => new Set(incomplete.flatMap((task) => (continuousDependencies[task.id] ?? []).map(({ id }) => id)));

const scheduled = Effect.fnUntraced(function* (workspace: Workspace, tasks: TaskGraph) {
    const schedule = new TasksSchedule(workspace, readProjectsConfigurationFromProjectGraph(workspace).projects, tasks, { lifeCycle: { scheduleTask: () => undefined, startTask: () => undefined, endTask: () => undefined } });
    const drained = <A>(next: () => A | null): readonly A[] => Array.unfold(next(), (item) => Option.map(Option.fromNullOr(item), (held) => [held, next()] as const));
    const slotted = Effect.fnUntraced(function* (slots: readonly Slot[], serving: readonly string[]): Effect.fn.Return<readonly Slot[]> {
        yield* Effect.promise(() => schedule.scheduleNextTasks());
        const [batches, alone] = [drained(() => schedule.nextBatch()), drained(() => schedule.nextTask())];
        const ran = [...batches.flatMap((batch) => Object.values(batch.taskGraph.tasks)), ...alone];
        const started = ran.filter((task) => task.continuous === true).map((task) => task.id);
        started.forEach((id) => {
            schedule.markContinuousTaskStarted(id);
            schedule.markReady(id);
        });
        schedule.complete(ran.filter((task) => task.continuous !== true).map((task) => task.id));
        const needed = awaited(tasks, schedule.getIncompleteTasks());
        const stopped = [...serving, ...started].filter((id) => !needed.has(id));
        schedule.complete(stopped);
        return (Array.isReadonlyArrayNonEmpty(ran) || Array.isReadonlyArrayNonEmpty(stopped)) && schedule.hasTasks() ? yield* slotted([...slots, { batches, alone, ran }], Array.difference([...serving, ...started], stopped)) : [...slots, { batches, alone, ran }];
    });
    yield* Effect.promise(() => schedule.init());
    return yield* slotted([], []);
});

const beside = (slots: readonly Slot[]) => slots.filter(({ ran }) => ran.length > 1).flatMap(({ ran }) => ran.filter((task) => task.parallelism === false).map((task) => `${task.id} beside ${ran.length - 1} tasks`));

const mixed = (workspace: Workspace, slots: readonly Slot[]) => {
    const { projects } = readProjectsConfigurationFromProjectGraph(workspace);
    return slots.flatMap(({ batches }) =>
        batches.flatMap((batch) =>
            Object.values(batch.taskGraph.tasks)
                .filter((task) => workspace.nodes[task.target.project]?.data.targets?.[task.target.target]?.executor !== batch.executorName || getExecutorForTask(task, projects).batchImplementationFactory === undefined)
                .map((task) => `${task.id} in ${batch.id}`),
        ),
    );
};

const preferring = (workspace: Workspace, tasks: readonly Task[]) => {
    const { projects } = readProjectsConfigurationFromProjectGraph(workspace);
    return tasks.filter((task) => task.parallelism !== false && task.continuous !== true && getExecutorForTask(task, projects).preferBatch === true).map((task) => task.id);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('task graph', (suite) => {
    suite.effect(
        'plans check, test, and build over every project without a cycle',
        Effect.fnUntraced(function* () {
            assert.isNull(findCycle(planned(yield* read('fresh'), CHECKED)), SOURCES.cycle);
        }),
    );

    suite.effect(
        'plans every README verb over every project without a cycle',
        Effect.fnUntraced(function* () {
            const [workspace, stated] = yield* Effect.all([read('fresh'), verbs]);
            assert.deepStrictEqual(cyclic(workspace, stated), [], SOURCES.cycle);
        }),
    );

    suite.effect(
        'gives no continuous task to a task that runs alone, and runs no depended-on continuous task alone',
        Effect.fnUntraced(function* () {
            const tasks = every(yield* read('fresh'));
            assert.doesNotThrow(() => assertTaskGraphDoesNotContainInvalidTargets(tasks), undefined, SOURCES.invalid);
        }),
    );

    suite.effect(
        'makes every command that depends on a continuous task wait until that task signals ready',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.deepStrictEqual(unsignalled(workspace, every(workspace)), [], SOURCES.ready);
        }),
    );

    suite.effect(
        'schedules every task of check, test, and build',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const tasks = planned(workspace, CHECKED);
            const slots = yield* scheduled(workspace, tasks);
            assert.deepStrictEqual(unscheduled(tasks, slots), [], SOURCES.drained);
        }),
    );

    suite.effect(
        'schedules a task that runs alone beside no other task of check, test, and build',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.deepStrictEqual(beside(yield* scheduled(workspace, planned(workspace, CHECKED))), [], SOURCES.exclusive);
        }),
    );

    suite.effect(
        'batches tasks of one executor that declares a batch implementation',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.deepStrictEqual(mixed(workspace, yield* scheduled(workspace, planned(workspace, CHECKED))), [], SOURCES.batched);
        }),
    );

    suite.effect(
        'batches every task of an executor that prefers batches',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const tasks = planned(workspace, CHECKED);
            const slots = yield* scheduled(workspace, tasks);
            assert.isNotEmpty(preferring(workspace, Object.values(tasks.tasks)), SOURCES.batched);
            assert.deepStrictEqual(
                preferring(
                    workspace,
                    slots.flatMap(({ alone }) => alone),
                ),
                [],
                SOURCES.batched,
            );
        }),
    );

    suite.effect(
        'runs a pack task of the same project before every attach and publish task of a project that packs',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const packing = new Set(
                declared(workspace)
                    .filter(({ name }) => name.startsWith('pack--'))
                    .map(({ project }) => project),
            );
            assert.deepStrictEqual(
                lacking(
                    planned(workspace, ['attach', 'nx-release-publish']),
                    ({ target }) => packing.has(target.project) && (target.target.startsWith('attach--') || target.target === 'nx-release-publish'),
                    ({ target }, id) => id.startsWith(`${target.project}:pack--`),
                ),
                [],
                SOURCES.packaged,
            );
        }),
    );

    suite.effect(
        'runs a build task of the same project before every preview task',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(
                lacking(
                    planned(yield* read('fresh'), ['preview']),
                    ({ target }) => target.target.startsWith('preview--'),
                    ({ target }, id) => id.startsWith(`${target.project}:build--`),
                ),
                [],
                SOURCES.previewed,
            );
        }),
    );
});
