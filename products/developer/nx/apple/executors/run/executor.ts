import { glob } from 'node:fs/promises';
import { basename, dirname, join, resolve, sep } from 'node:path';
import { env } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Config, Effect, FileSystem, Function, HashSet, Option, Predicate, Record, Schema, Stream } from 'effect';
import { CommandLine } from 'effect/process';
import { ran, run } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({
    profiles: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Patterns of the profiles the invocations write, each match removed before the invocations' }),
    objects: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Instrumented binaries llvm-cov reads the coverage mapping of' }),
    file: Schema.String.annotate({ description: 'File the lcov report is written to' }),
}).annotate({ description: 'lcov report written once every invocation passes: the profiles matching profiles merge with xcrun llvm-profdata merge, then export over objects with xcrun llvm-cov export' });
const Run = Schema.Struct({
    runs: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
    applications: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Application bundles the invocations install, a running one named in the last line as needing a relaunch' }),
    report: Schema.optionalKey(Report),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each invocation and runs none' }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
}).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed and git status.branch off after the caller's GIT_CONFIG_<n> entries, stopping at the first failure, then writes the lcov report a coverage configuration states" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = Config.map(Config.withDefault(Config.Int('GIT_CONFIG_COUNT'), 0), (count) => ({ GIT_CONFIG_COUNT: `${count + 1}`, [`GIT_CONFIG_KEY_${count}`]: 'status.branch', [`GIT_CONFIG_VALUE_${count}`]: 'false' }));

const matched = (patterns: readonly string[], root: string) => Effect.promise(() => globalThis.Array.fromAsync(glob(patterns, { cwd: root }), (at) => resolve(root, at)));

const lcov = Effect.fnUntraced(function* ({ profiles, objects }: typeof Report.Type, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const written = yield* Effect.filterOrFail(matched(profiles, root), Array.isReadonlyArrayNonEmpty, () => new Cause.UnknownError(profiles, `No profile matches ${profiles.join(' or ')} after the invocations`));
    const merged = join(yield* fs.makeTempDirectoryScoped(), 'merged.profdata');
    yield* spawned(['xcrun', 'llvm-profdata', 'merge', '--sparse', `--output=${merged}`, ...written], { cwd: root });
    return yield* spawned(['xcrun', 'llvm-cov', 'export', '--format=lcov', `--instr-profile=${merged}`, ...objects.flatMap((object) => ['--object', object])], { cwd: root });
}, Effect.scoped);

const commands = Effect.fnUntraced(function* ({ runs, applications = [], report, dryRun: dry = false, __unparsed__: words = [] }: typeof Run.Type, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const stated = yield* configured;
    const reported = Option.filter(Option.fromUndefinedOr(report), () => !dry);
    const cleared = Record.map(
        Record.filter(env, (_, name) => name.startsWith('NX_') || name === 'LERNA_PACKAGE_NAME'),
        Function.constUndefined,
    );
    const previous = yield* Option.match(reported, { onNone: () => Effect.succeed([]), onSome: ({ profiles }) => matched(profiles, root) });
    yield* Effect.forEach(previous, (at) => fs.remove(at, { force: true }), { discard: true });
    const success = yield* Effect.reduce(
        runs,
        (): boolean => true,
        (passed, invocation) => (passed && !dry ? Effect.map(ran(Array.appendAll(invocation, words), { cwd: root, env: { ...cleared, ...stated } }), (code) => code === 0) : Effect.as(dry ? Effect.logInfo(`Would run ${CommandLine.display(invocation)}`) : Effect.void, passed)),
    );
    yield* Effect.transposeOption(
        Option.map(
            Option.filter(reported, () => success),
            Effect.fnUntraced(function* (stating) {
                const at = resolve(root, stating.file);
                yield* fs.makeDirectory(dirname(at), { recursive: true });
                yield* fs.writeFileString(at, yield* lcov(stating, root));
            }),
        ),
    );
    const running = !success || dry || applications.length === 0 ? [] : (yield* spawned(['ps', '-Ao', 'comm='], { cwd: root })).split('\n');
    const stale = applications.filter((application) => running.some((executable) => executable.startsWith(join(application, 'Contents', 'MacOS', sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => basename(application, '.app')).join(', ')} runs the build it launched from, relaunch it to run this one`);
    return { success };
});

const ordered = (graph: TaskGraph, inputs: Readonly<Record<string, typeof Run.Type>>, root: string) =>
    Stream.paginate({ pending: Record.keys(inputs), failed: HashSet.empty<string>() }, ({ pending, failed }) => {
        const waits = (id: string) => (graph.dependencies[id] ?? []).some((edge) => pending.includes(edge.id) && edge.id !== id);
        return Option.match(
            Option.flatMap(Array.findFirst(pending, Predicate.not(waits)), (id) => Option.map(Option.fromUndefinedOr(inputs[id]), (options) => [id, options] as const)),
            {
                onNone: () => Effect.succeed([[], Option.none()] as const),
                onSome: ([task, options]) => {
                    const rest = pending.filter((id) => id !== task);
                    const blocked = (graph.dependencies[task] ?? []).some((edge) => HashSet.has(failed, edge.id));
                    const outcome = blocked ? Effect.succeed<TaskResult>({ success: false, status: 'skipped', terminalOutput: '' }) : Effect.map(commands(options, root), ({ success }): TaskResult => ({ success, terminalOutput: '' }));
                    return Effect.map(outcome, (result) => [[{ task, result }], Option.some({ pending: rest, failed: result.success ? failed : HashSet.add(failed, task) })] as const);
                },
            },
        );
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: typeof Run.Type, context: ExecutorContext) => run(commands(options, context.root));

const batch = (graph: TaskGraph, inputs: Readonly<Record<string, typeof Run.Type>>, _: unknown, context: ExecutorContext) => Stream.toReadableStream(Stream.provide(ordered(graph, inputs, context.root), layer)).values();

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, commands, configured, execute, Run };
