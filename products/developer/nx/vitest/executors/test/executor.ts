import { dirname, relative, resolve } from 'node:path';
import { chdir, cwd, env } from 'node:process';
import { normalizePath } from '@nx/devkit';
import { type BatchExecutorTaskResult, findProjectForPath, hashObject } from '@nx/devkit/internal';
import { batched, executor } from '@rasm/nx-distribution/inference';
import { Array, Cause, Clock, Effect, Exit, Formatter, Function, Option, Predicate, Record, Result, Schema, Stream, Struct } from 'effect';
import type { CliOptions, parseCLI, TestModule, TestsNotFoundError, Vitest } from 'vitest/node';
import { loaded, MODE } from '#/plugin';

// --- [TYPES] ---------------------------------------------------------------------------

interface Planned {
    readonly task: string;
    readonly options: Test;
    readonly home: string;
}

// --- [MODELS] --------------------------------------------------------------------------

class Test extends Schema.Opaque<Test, unknown>()(
    Schema.Struct({
        config: Schema.String.annotate({ description: 'Vitest configuration file the run loads, relative to the workspace root, whose folder the run starts from' }),
        projects: Schema.Array(Schema.String).annotate({ description: 'Vitest projects the task runs, passed as --project, every project the configuration resolves where empty' }),
        attachmentsDir: Schema.optionalKey(Schema.String).annotate({ description: "Folder the task's projects write attachments to, relative to the workspace root, the run's attachmentsDir where the task runs alone" }),
        coverage: Schema.optionalKey(Schema.Struct({ enabled: Schema.Boolean, reporter: Schema.String, reportsDirectory: Schema.optionalKey(Schema.String) })).annotate({
            description: 'Coverage options of the run, reportsDirectory relative to the workspace root, which the coverage configuration of the target sets',
        }),
        __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, parsed as vitest run arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
    }).annotate({
        title: 'Test',
        description: "Runs vitest run once over the projects of every batched task sharing a configuration and its options, one Vite server and one worker pool the configuration's maxWorkers sizes, and decides each task from the test modules of its own projects",
    }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const testMode = Effect.acquireRelease(
    Effect.sync(() => {
        const before = { ...env };
        Object.assign(env, { ...MODE, NODE_ENV: before['NODE_ENV'] ?? MODE.NODE_ENV });
        return before;
    }),
    (before) =>
        Effect.sync(() => {
            Record.keys(MODE).forEach((name) => {
                Reflect.deleteProperty(env, name);
            });
            Object.assign(env, before);
        }),
);

const reported = (module: TestModule) =>
    [...module.children.allTests('failed')].flatMap((test) => {
        const result = test.result();
        return [`FAIL ${module.project.name} ${module.relativeModuleId} > ${test.fullName}`, ...(result.state === 'failed' ? result.errors.map(({ stack, message }) => stack ?? message) : [])];
    });

const tested = Effect.fnUntraced(function* (root: string, group: Array.NonEmptyReadonlyArray<Planned>) {
    const [
        {
            options: { config, coverage, __unparsed__: words = [] },
        },
    ] = group;
    const entry = resolve(root, config);
    const node = yield* loaded<{ readonly createVitest: (options: CliOptions) => Promise<Vitest>; readonly parseCLI: typeof parseCLI; readonly TestsNotFoundError: typeof TestsNotFoundError }>(config, entry, 'vitest/node');
    const alone = Option.flatMap(
        Option.liftPredicate(group, (all) => all.length === 1),
        ([{ options }]) => Option.fromUndefinedOr(options.attachmentsDir),
    );
    const stated: CliOptions = {
        ...node.parseCLI(['vitest', 'run', ...words]).options,
        config: entry,
        watch: false,
        project: group.some(({ options }) => options.projects.length === 0) ? [] : Array.dedupe(group.flatMap(({ options }) => options.projects)),
        ...Option.getOrUndefined(Option.map(alone, (at) => ({ attachmentsDir: resolve(root, at) }))),
        ...(coverage === undefined ? {} : { coverage: { ...coverage, ...(coverage.reportsDirectory === undefined ? {} : { reportsDirectory: resolve(root, coverage.reportsDirectory) }) } }),
    };
    yield* testMode;
    yield* Effect.acquireRelease(
        Effect.sync(() => {
            const before = cwd();
            chdir(dirname(entry));
            return before;
        }),
        (before) => Effect.sync(() => chdir(before)),
    );
    const vitest = yield* Effect.acquireRelease(Effect.tryPromise({ try: () => node.createVitest(stated), catch: (cause) => new Cause.UnknownError(cause, `${config} does not start Vitest`) }), (started) => Effect.promise(() => started.close()));
    const homes = new Map(group.map(({ home, task }) => [home, task] as const));
    const owner = (at: string) => Option.fromNullOr(findProjectForPath(normalizePath(relative(root, at)), homes));
    const attachments = Record.fromIterableWith(group, ({ task, options }) => [task, options.attachmentsDir] as const);
    vitest.projects
        .flatMap((project) =>
            owner(project.config.root).pipe(
                Option.flatMap((task) => Option.fromUndefinedOr(attachments[task])),
                Option.map((at) => [project, resolve(root, at)] as const),
                Option.toArray,
            ),
        )
        .forEach(([project, at]) => {
            Object.assign(project.config, { attachmentsDir: at });
        });
    yield* Effect.acquireRelease(
        Effect.sync(() => {
            const before = process.exitCode;
            process.exitCode = undefined;
            return before;
        }),
        (before) => Effect.sync(() => Object.assign(process, { exitCode: before })),
    );
    const started = yield* Effect.exit(Effect.tryPromise({ try: () => vitest.start(), catch: (cause) => new Cause.UnknownError(cause, `vitest run over ${config} stops before it reports`) }));
    const modules = vitest.state.getTestModules();
    const unhandled = vitest.state.getUnhandledErrors().map((error) => `unhandled error: ${Formatter.format(error)}`);
    const signalled = Option.filter(Option.fromNullishOr(process.exitCode), (code) => Number(code) !== 0 && Exit.isSuccess(started) && unhandled.length === 0 && modules.every((module) => module.ok()));
    const broken = [
        ...unhandled,
        ...(Exit.isFailure(started) && !Option.exists(Cause.findErrorOption(started.cause), (error) => error.cause instanceof node.TestsNotFoundError) ? [`vitest run over ${config} stops before it reports: ${Cause.pretty(started.cause)}`] : []),
        ...Option.toArray(Option.map(signalled, (code) => `vitest run over ${config} sets exit code ${code} with every test module passing, as a run-level check such as a coverage threshold or the import duration limit failed`)),
    ];
    const files = new Map(vitest.state.getFiles().map((file) => [file.id, file] as const));
    const held = Array.groupBy(
        modules.flatMap((module) => {
            const { prepareDuration, environmentSetupDuration, setupDuration, collectDuration, duration } = module.diagnostic();
            const span = Option.map(Option.fromUndefinedOr(files.get(module.id)?.result?.startTime), (begun) => [Math.round(begun - prepareDuration - environmentSetupDuration - setupDuration - collectDuration), Math.round(begun + duration)] as const);
            return Option.toArray(Option.map(owner(module.project.config.root), (task) => ({ task, module, span })));
        }),
        Struct.get('task'),
    );
    return Array.map(group, ({ task }): BatchExecutorTaskResult => {
        const own = (held[task] ?? []).map(Struct.get('module'));
        const spans = (held[task] ?? []).flatMap(({ span }) => Option.toArray(span));
        const said = [...own.flatMap(reported), ...(own.length === 0 ? [`no test module of ${task} ran`] : []), ...broken];
        const passed = own.length > 0 ? own.every((module) => module.ok()) : Predicate.isTruthy(vitest.config.passWithNoTests);
        const timed = Array.match(spans, { onEmpty: () => ({}), onNonEmpty: (ran) => ({ startTime: Math.min(...ran.map(([from]) => from)), endTime: Math.max(...ran.map(([, to]) => to)) }) });
        return { task, result: { success: passed && broken.length === 0, terminalOutput: said.join('\n'), ...timed } };
    });
}, Effect.scoped);

const settled = (root: string, group: Array.NonEmptyReadonlyArray<Planned>) =>
    Effect.map(Effect.all([Clock.currentTimeMillis, Effect.result(tested(root, group)), Clock.currentTimeMillis]), ([startTime, outcome, endTime]) =>
        Result.match(outcome, {
            onFailure: (error) => Array.map(group, ({ task }): BatchExecutorTaskResult => ({ task, result: { success: false, terminalOutput: error.message, startTime, endTime } })),
            onSuccess: Array.map(({ task, result }): BatchExecutorTaskResult => ({ task, result: { startTime, endTime, ...result } })),
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Test, function* (options, context) {
    const task = yield* Effect.fromNullishOr(context.projectName);
    const [{ result }] = yield* settled(context.root, [{ task, options, home: context.projectGraph.nodes[task]?.data.root ?? '.' }]);
    return { success: result.success };
});

const batch = batched(Test, (graph, inputs, context) =>
    Function.pipe(
        Record.collect(inputs, (task, options): Planned => ({ task, options, home: graph.tasks[task]?.projectRoot ?? '.' })),
        Array.groupBy(({ options }) => hashObject(Struct.omit(options, ['projects', 'attachmentsDir']))),
        Record.values,
        Stream.fromIterable,
        Stream.flatMap((group) => Stream.fromIterableEffect(settled(context.root, group))),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute, Test };
