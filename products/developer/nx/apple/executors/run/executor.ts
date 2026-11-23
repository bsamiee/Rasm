import { glob } from 'node:fs/promises';
import { basename, dirname, join, resolve, sep } from 'node:path';
import { env } from 'node:process';
import type { ExecutorContext, TaskGraph } from '@nx/devkit';
import type { TaskResult } from '@nx/devkit/internal';
import { batched, executor, spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, Config, Effect, FileSystem, Function, HashSet, Option, Predicate, Record, Schema, Stream } from 'effect';
import { CommandLine } from 'effect/process';
import { ran, run } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.Struct({
    profiles: Schema.NonEmptyArray(Schema.String).annotate({ description: 'Patterns of the profiles the invocations write, each match removed before the invocations' }),
    objects: Schema.Array(Schema.String)
        .annotate({ description: 'Instrumented binaries llvm-cov reads the coverage mapping of' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    results: Schema.Array(Schema.String)
        .annotate({ description: 'Patterns of the result bundles the invocations write, each match removed before the invocations, the build product of each target xcrun xccov reports in one joining objects' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    file: Schema.String.annotate({ description: 'File the lcov report is written to' }),
}).annotate({ description: 'lcov report written once every invocation passes: the profiles matching profiles merge with xcrun llvm-profdata merge, then export over objects and the targets of each matching result bundle with xcrun llvm-cov export' });
const Covered = Schema.fromJsonString(Schema.Array(Schema.Struct({ buildProductPath: Schema.String })));
class Run extends Schema.Opaque<Run, unknown>()(
    Schema.Struct({
        runs: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
        applications: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Bundle names of the applications the invocations install, a running executable inside one named in the last line as needing a relaunch' }),
        report: Schema.optionalKey(Report),
        dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each invocation and runs none' }),
        __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
        outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
    }).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed and git status.branch off after the caller's GIT_CONFIG_<n> entries, stopping at the first failure, then writes the lcov report a coverage configuration states" }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = Config.map(Config.withDefault(Config.Int('GIT_CONFIG_COUNT'), 0), (count) => ({ GIT_CONFIG_COUNT: `${count + 1}`, [`GIT_CONFIG_KEY_${count}`]: 'status.branch', [`GIT_CONFIG_VALUE_${count}`]: 'false' }));

const matched = (patterns: readonly string[], root: string) => Effect.promise(() => globalThis.Array.fromAsync(glob(patterns, { cwd: root }), (at) => resolve(root, at)));

const lcov = Effect.fnUntraced(function* ({ profiles, objects, results }: NonNullable<Run['report']>, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const written = yield* Effect.filterOrFail(matched(profiles, root), Array.isReadonlyArrayNonEmpty, () => new Cause.UnknownError(profiles, `No profile matches ${profiles.join(' or ')} after the invocations`));
    const reported = Effect.flatMap(matched(results, root), (bundles) => Effect.forEach(bundles, (bundle) => Effect.flatMap(spawned(['xcrun', 'xccov', 'view', '--report', '--only-targets', '--json', bundle], { cwd: root }), Schema.decodeEffect(Covered))));
    const instrumented = yield* Effect.filterOrFail(
        Effect.map(reported, (targets) => Array.dedupe([...objects, ...targets.flat().map(({ buildProductPath }) => buildProductPath)])),
        Array.isReadonlyArrayNonEmpty,
        () => new Cause.UnknownError(results, `No instrumented binary is stated or reported by a result bundle matching ${results.join(' or ')} after the invocations`),
    );
    const merged = join(yield* fs.makeTempDirectoryScoped(), 'merged.profdata');
    yield* spawned(['xcrun', 'llvm-profdata', 'merge', '--sparse', `--output=${merged}`, ...written], { cwd: root });
    return yield* spawned(['xcrun', 'llvm-cov', 'export', '--format=lcov', `--instr-profile=${merged}`, ...instrumented.flatMap((object) => ['--object', object])], { cwd: root });
}, Effect.scoped);

const commands = Effect.fnUntraced(function* ({ runs, applications = [], report, dryRun: dry = false, __unparsed__: words = [] }: Run, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const stated = yield* configured;
    const reported = Option.filter(Option.fromUndefinedOr(report), () => !dry);
    const cleared = Record.map(
        Record.filter(env, (_, name) => name.startsWith('NX_') || name === 'LERNA_PACKAGE_NAME'),
        Function.constUndefined,
    );
    const previous = yield* Option.match(reported, { onNone: () => Effect.succeed([]), onSome: ({ profiles, results }) => matched([...profiles, ...results], root) });
    yield* Effect.forEach(previous, (at) => fs.remove(at, { force: true, recursive: true }), { discard: true });
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
    const stale = applications.filter((application) => running.some((executable) => executable.includes(join(sep, application, 'Contents', 'MacOS', sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => basename(application, '.app')).join(', ')} runs the build it launched from, relaunch it to run this one`);
    return { success };
});

const ordered = (graph: TaskGraph, inputs: Readonly<Record<string, Run>>, { root }: ExecutorContext) =>
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

const execute = executor(Run, run, function* (stated, context) {
    return yield* commands(stated, context.root);
});

const batch = batched(Run, ordered);

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, commands, configured, execute, Run };
