import { fork } from 'node:child_process';
import { once } from 'node:events';
import { availableParallelism } from 'node:os';
import { dirname, relative, resolve } from 'node:path';
import process, { env } from 'node:process';
import { normalizePath } from '@nx/devkit';
import { type BatchExecutorTaskResult, findProjectForPath, hashObject } from '@nx/devkit/internal';
import { batched, executor, NxError, request } from '@rasm/nx/inference';
import { Array, Cause, Clock, Effect, Exit, Formatter, Option, Predicate, Record, Result, Schema, Stream, Struct } from 'effect';
import type { CliOptions, parseCLI, TestModule, TestsNotFoundError, Vitest } from 'vitest/node';
import { loaded, MODE } from '#/plugin';

// --- [TYPES] ---------------------------------------------------------------------------

type Planned = (typeof Grouped.Type)['group'][number];

// --- [MODELS] --------------------------------------------------------------------------

class Test extends Schema.Opaque<Test>()(
    Schema.Struct({
        config: Schema.String.annotate({ description: 'Vitest configuration file the run loads, relative to the workspace root, whose folder the run starts from' }),
        projects: Schema.Array(Schema.String).annotate({ description: 'Vitest projects the task runs, passed as --project, every project the configuration resolves where empty' }),
        attachmentsDir: Schema.optionalKey(Schema.String).annotate({ description: "Folder the task's projects write attachments to, relative to the workspace root, the run's attachmentsDir where the task runs alone" }),
        __unparsed__: Schema.String.pipe(Schema.Array, Schema.optionalKey).annotate({ description: 'Words after -- of the run, parsed as vitest run arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
    }).annotate({
        title: 'Test',
        description:
            "Runs vitest run in a child process of its own once over the projects of every batched task sharing a configuration and its options, one Vite server and one worker pool the configuration's maxWorkers sizes, side by side with the runs of other option groups, and decides each task from the test modules of its own projects",
    }),
) {}
const Traced = Schema.Struct({ VITEST_TEST_PATH: Schema.String }).annotate({ description: 'The test file path Vitest stamps on an unhandled error a test module raised' });
const Grouped = Schema.Struct({ root: Schema.String, group: Schema.NonEmptyArray(Schema.Struct({ task: Schema.String, options: Test, home: Schema.String })) }).annotate({
    description: 'The workspace root and the tasks one vitest run decides, each with its options and the root of its project, which the executor sends the child process running them',
});
const Settled = Schema.Array(Schema.Struct({ task: Schema.String, result: Schema.Struct({ success: Schema.Boolean, terminalOutput: Schema.String, startTime: Schema.optionalKey(Schema.Finite), endTime: Schema.optionalKey(Schema.Finite) }) })).annotate({
    description: 'The result of each task of one vitest run, which the child process running it sends back in the shape Nx reads from a batch',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const reported = (module: TestModule) =>
    [...module.children.allTests('failed')].flatMap((test) => {
        const result = test.result();
        return [`FAIL ${module.project.name} ${module.relativeModuleId} > ${test.fullName}`, ...(result.state === 'failed' ? result.errors.map(({ stack, message }) => stack ?? message) : [])];
    });

const tested = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const { config, attachmentsDir, __unparsed__: words = [] } = Array.headNonEmpty(group).options;
    const entry = resolve(root, config);
    const node = yield* loaded<{ readonly createVitest: (options: CliOptions) => Promise<Vitest>; readonly parseCLI: typeof parseCLI; readonly TestsNotFoundError: typeof TestsNotFoundError }>(config, entry, 'vitest/node');
    const stated: CliOptions = {
        allowOnly: false,
        update: 'none',
        ...node.parseCLI(['vitest', 'run', ...words]).options,
        config: entry,
        watch: false,
        project: group.some(({ options }) => options.projects.length === 0) ? [] : Array.dedupe(group.flatMap(({ options }) => options.projects)),
        ...(attachmentsDir !== undefined && group.length === 1 && { attachmentsDir: resolve(root, attachmentsDir) }),
    };
    const vitest = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => node.createVitest(stated), catch: (cause) => new Cause.UnknownError(cause, `${config} does not start Vitest`) }), (started) => Effect.promise(() => started.close()));
    const homes = new Map(group.map(({ home, task }) => [home, task] as const));
    const owner = (at: string) => Option.fromNullOr(findProjectForPath(normalizePath(relative(root, at)), homes));
    const attachments = Record.fromIterableWith(group, ({ task, options }) => [task, options.attachmentsDir] as const);
    vitest.projects.forEach((project) => {
        const held = Option.flatMapNullishOr(owner(project.config.root), (task) => attachments[task]);
        Object.assign(project.config, Option.getOrUndefined(Option.map(held, (at) => ({ attachmentsDir: resolve(root, at) }))));
    });
    const started = yield* Effect.exit(Effect.tryPromise({ try: () => vitest.start(), catch: (cause) => new Cause.UnknownError(cause, `vitest run over ${config} stops before it reports`) }));
    const modules = vitest.state.getTestModules();
    const unhandled = vitest.state.getUnhandledErrors().map((error) => ({ said: `unhandled error: ${Formatter.format(error)}`, by: Option.flatMap(Schema.decodeUnknownOption(Traced)(error), ({ VITEST_TEST_PATH }) => owner(VITEST_TEST_PATH)) }));
    const [traced, stray] = Array.partition(unhandled, ({ said, by }) => Option.match(by, { onNone: () => Result.fail(said), onSome: (task) => Result.succeed({ task, said }) }));
    const faulted = Array.groupBy(traced, Struct.get('task'));
    const signalled = Option.filter(Option.fromNullishOr(process.exitCode), (code) => Number(code) !== 0 && Exit.isSuccess(started) && unhandled.length === 0 && modules.every((module) => module.ok()));
    const broken = [
        ...stray,
        ...(Exit.isFailure(started) && !Option.exists(Cause.findErrorOption(started.cause), (error) => error.cause instanceof node.TestsNotFoundError) ? [`vitest run over ${config} stops before it reports: ${Cause.pretty(started.cause)}`] : []),
        ...Option.toArray(Option.map(signalled, (code) => `vitest run over ${config} sets exit code ${code} with every test module passing, as a run-level check such as the import duration limit failed`)),
    ];
    const held = Array.groupBy(
        modules.flatMap((module) => {
            const { prepareDuration, environmentSetupDuration, setupDuration, collectDuration, duration } = module.diagnostic();
            const span = Option.map(Option.fromUndefinedOr(vitest.state.idMap.get(module.id)?.result?.startTime), (begun) => [Math.round(begun - prepareDuration - environmentSetupDuration - setupDuration - collectDuration), Math.round(begun + duration)] as const);
            return Option.toArray(Option.map(owner(module.project.config.root), (task) => ({ task, module, span })));
        }),
        Struct.get('task'),
    );
    return Array.map(group, ({ task }): BatchExecutorTaskResult => {
        const own = (held[task] ?? []).map(Struct.get('module'));
        const spans = (held[task] ?? []).flatMap(({ span }) => Option.toArray(span));
        const faults = [...(faulted[task] ?? []).map(Struct.get('said')), ...broken];
        const said = [...own.flatMap(reported), ...(own.length === 0 ? [`no test module of ${task} ran`] : []), ...faults];
        const passed = own.length > 0 ? own.every((module) => module.ok()) : Predicate.isTruthy(vitest.config.passWithNoTests);
        const timed = Array.match(spans, { onEmpty: () => ({}), onNonEmpty: (ran) => ({ startTime: Math.min(...ran.map(([from]) => from)), endTime: Math.max(...ran.map(([, to]) => to)) }) });
        return { task, result: { success: passed && faults.length === 0, terminalOutput: said.join('\n'), ...timed } };
    });
}, Effect.scoped);

const settled = Effect.fnUntraced(function* (group: Array.NonEmptyReadonlyArray<Planned>, run: Effect.Effect<readonly BatchExecutorTaskResult[], Error>) {
    const [startTime, outcome, endTime] = yield* Effect.all([Clock.currentTimeMillis, Effect.result(run), Clock.currentTimeMillis]);
    return Result.match(outcome, {
        onFailure: (error) => Array.map(group, ({ task }): BatchExecutorTaskResult => ({ task, result: { success: false, terminalOutput: error.message, startTime, endTime } })),
        onSuccess: Array.map(({ task, result }): BatchExecutorTaskResult => ({ task, result: { startTime, endTime, ...result } })),
    });
});

const forked = (root: string, group: Array.NonEmptyReadonlyArray<Planned>) => {
    const { config } = Array.headNonEmpty(group).options;
    const started = Effect.tap(
        Effect.sync(() => fork(import.meta.filename, { cwd: dirname(resolve(root, config)), env: { ...env, ...MODE, NODE_ENV: env['NODE_ENV'] ?? MODE.NODE_ENV }, serialization: 'advanced' })),
        (child) => Effect.sync(() => child.send({ root, group })),
    );
    return settled(group, request(started, Settled, NxError.of({ _tag: 'Failed', step: 'vitest run' }, config)));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Test, function* (options, context) {
    const task = yield* Effect.fromNullishOr(context.projectName);
    const [{ result }] = yield* forked(context.root, [{ task, options, home: context.projectGraph.nodes[task]?.data.root ?? '.' }]);
    return { success: result.success };
});

const batch = batched(Test, (graph, inputs, context) =>
    Stream.fromIterable(
        Record.values(
            Array.groupBy(
                Record.toEntries(inputs).map(([task, options]) => ({ task, options, home: graph.tasks[task]?.projectRoot ?? '.' })),
                ({ options }) => hashObject(Struct.omit(options, ['projects', 'attachmentsDir'])),
            ),
        ),
    ).pipe(
        Stream.mapEffect((group) => forked(context.root, group), { concurrency: Math.max(availableParallelism() - 1, 1), unordered: true }),
        Stream.flatMap(Stream.fromIterable),
    ),
);

if (import.meta.main) {
    Effect.runFork(
        Effect.promise((): Promise<readonly unknown[]> => once(process, 'message')).pipe(
            Effect.flatMap(([message]) => Schema.decodeUnknownEffect(Grouped)(message)),
            Effect.flatMap(({ root, group }) => settled(group, tested(root, group))),
            Effect.flatMap((results) => Effect.sync(() => process.send?.(results, () => process.disconnect?.()))),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute, Test };
