import { createRequire } from 'node:module';
import { dirname, relative, resolve } from 'node:path';
import { chdir, cwd } from 'node:process';
import { pathToFileURL } from 'node:url';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { run } from '@rasm/nx-distribution/inference';
import { Array, Cause, Effect, Equal, Exit, Formatter, Function, Option, Order, Record, Schema, Stream, Struct } from 'effect';
import type { CliOptions, parseCLI, TestModule, TestsNotFoundError, Vitest } from 'vitest/node';
import { testMode } from '#/plugin';

// --- [TYPES] ---------------------------------------------------------------------------

interface Planned {
    readonly task: string;
    readonly options: typeof Test.Type;
    readonly home: string;
}
interface Settled {
    readonly task: string;
    readonly result: TaskResult;
}

// --- [MODELS] --------------------------------------------------------------------------

const Test = Schema.Struct({
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
});

// --- [OPERATIONS] ----------------------------------------------------------------------

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
    const node = yield* Effect.tryPromise({
        try: (): Promise<{ readonly createVitest: (options: CliOptions) => Promise<Vitest>; readonly parseCLI: typeof parseCLI; readonly TestsNotFoundError: typeof TestsNotFoundError }> => import(pathToFileURL(createRequire(entry).resolve('vitest/node')).href),
        catch: (cause) => new Cause.UnknownError(cause, `${config} resolves no vitest/node`),
    });
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
    const ranked = Array.sort(
        group,
        Order.mapInput(Order.Number, ({ home }: Planned) => -home.length),
    );
    const owner = (at: string) => Array.findFirst(ranked, ({ home }) => !relative(resolve(root, home), at).startsWith('..'));
    const placed = Option.isNone(alone)
        ? Array.getSomes(
              vitest.projects.map((project) =>
                  Option.map(
                      Option.flatMap(owner(project.config.root), ({ options }) => Option.fromUndefinedOr(options.attachmentsDir)),
                      (at) => [project, resolve(root, at)] as const,
                  ),
              ),
          )
        : [];
    placed.forEach(([project, at]) => {
        Object.assign(project.config, { attachmentsDir: at });
    });
    const started = yield* Effect.exit(Effect.tryPromise({ try: () => vitest.start(), catch: (cause) => new Cause.UnknownError(cause, `vitest run over ${config} stops before it reports`) }));
    const broken = [
        ...vitest.state.getUnhandledErrors().map((error) => `unhandled error: ${Formatter.format(error)}`),
        ...(Exit.isFailure(started) && !Option.exists(Cause.findErrorOption(started.cause), (error) => error instanceof node.TestsNotFoundError) ? [`vitest run over ${config} stops before it reports: ${Cause.pretty(started.cause)}`] : []),
    ];
    const held = Array.groupBy(
        vitest.state.getTestModules().flatMap((module) => Option.toArray(Option.map(owner(module.project.config.root), ({ task }) => ({ task, module })))),
        Struct.get('task'),
    );
    return Array.map(group, ({ task }): Settled => {
        const own = (held[task] ?? []).map(Struct.get('module'));
        const said = [...own.flatMap(reported), ...(own.length === 0 ? [`no test module of ${task} ran`] : []), ...broken];
        const passed = own.length > 0 ? own.every((module) => module.ok()) : vitest.config.passWithNoTests;
        return { task, result: { success: passed && broken.length === 0, terminalOutput: said.join('\n') } };
    });
}, Effect.scoped);

const settled = (root: string, group: Array.NonEmptyReadonlyArray<Planned>) => Effect.match(tested(root, group), { onFailure: (error) => Array.map(group, ({ task }): Settled => ({ task, result: { success: false, terminalOutput: error.message } })), onSuccess: Function.identity });

const grouped = (planned: readonly Planned[]) => {
    const keyed = ({ options }: Planned) => Struct.omit(options, ['projects', 'attachmentsDir']);
    return Array.dedupe(planned.map(keyed)).flatMap((key) =>
        Array.match(
            planned.filter((each) => Equal.equals(keyed(each), key)),
            { onEmpty: () => [], onNonEmpty: (group) => [group] },
        ),
    );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* (options: typeof Test.Type, context: ExecutorContext) {
    const task = yield* Effect.fromNullishOr(context.projectName);
    const [{ result }] = yield* settled(context.root, [{ task, options, home: context.projectGraph.nodes[task]?.data.root ?? '.' }]);
    return { success: result.success };
}, run);

const batch = (graph: TaskGraph, inputs: Readonly<Record<string, typeof Test.Type>>, _: unknown, context: ExecutorContext) =>
    Stream.toReadableStream(Stream.flatMap(Stream.fromIterable(grouped(Record.collect(inputs, (task, options): Planned => ({ task, options, home: context.projectGraph.nodes[graph.tasks[task]?.target.project ?? '']?.data.root ?? '.' })))), (group) => Stream.fromIterableEffect(settled(context.root, group)))).values();

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute, Test };
