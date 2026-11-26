import { basename, dirname, join, resolve, sep } from 'node:path';
import { env } from 'node:process';
import { getSystemErrorMessage } from 'node:util';
import { executor, spawned, Words } from '@rasm/nx/inference';
import { Array, Config, Effect, FileSystem, Function, Option, PlatformError, Record, Result, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RENAME_SWAP = 0x2;

// --- [MODELS] --------------------------------------------------------------------------

class Run extends Schema.Opaque<Run>()(
    Schema.Struct({
        runs: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
        applications: Schema.optionalKey(Schema.Array(Schema.String)).annotate({
            description:
                "Bundle names of the applications the invocations archive, each invocation taking -archivePath <n>.xcarchive in a temporary folder, once every invocation succeeds each bundle under an archive's Products taking its path below / in one renamex_np, swapped with the bundle installed there, so a failed invocation leaves the installed bundle untouched, then each one holding Contents/Library/LaunchAgents launched once more with open -n -g -j -W, behind the user's windows, so its SMAppService registration replaces the agents still running the build it swapped out, and a running executable inside one named in the last line as needing a relaunch",
        }),
        results: Schema.optionalKey(Schema.Array(Schema.String)).annotate({
            description: "Result bundle paths, the one at an invocation's index passed to it as -resultBundlePath, each removed before the first invocation runs, as xcodebuild refuses a result bundle path that exists",
        }),
        dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each invocation and runs none' }),
        __unparsed__: Words,
        outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
    }).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed and git status.branch off after the caller's GIT_CONFIG_<n> entries, stopping at the first failure" }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const install = Effect.fnUntraced(function* (from: string, to: string) {
    const fs = yield* FileSystem.FileSystem;
    const [held, { dlopen, getInt32 }] = yield* Effect.all([fs.exists(to), Effect.promise(() => import('node:ffi'))]);
    const { renamex_np: rename, __error: errno } = (yield* Effect.acquireDisposable(Effect.sync(() => dlopen(null, { renamex_np: { arguments: ['string', 'string', 'u32'], return: 'i32' }, __error: { return: 'pointer' } })))).functions;
    yield* fs.makeDirectory(dirname(to), { recursive: true });
    return yield* rename(from, to, held ? RENAME_SWAP : 0) === 0 ? Effect.void : PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'install', syscall: 'renamex_np', pathOrDescriptor: to, description: getSystemErrorMessage(-getInt32(errno())) });
}, Effect.scoped);

const configured = Config.map(Config.withDefault(Config.Int('GIT_CONFIG_COUNT'), 0), (count) => ({ GIT_CONFIG_COUNT: `${count + 1}`, [`GIT_CONFIG_KEY_${count}`]: 'status.branch', [`GIT_CONFIG_VALUE_${count}`]: 'false' }));

const commands = Effect.fnUntraced(function* ({ runs, applications = [], results = [], dryRun: dry = false, __unparsed__: words = [] }: Run, root: string) {
    const [fs, spawner, stated] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, configured]);
    const cleared = Record.map(
        Record.filter(env, (_, name) => name.startsWith('NX_') || name === 'LERNA_PACKAGE_NAME'),
        Function.constUndefined,
    );
    const stage = yield* applications.length === 0 || dry ? Effect.succeedNone : Effect.asSome(fs.makeTempDirectoryScoped());
    yield* Effect.forEach(dry ? [] : results, (at) => fs.remove(resolve(root, at), { recursive: true, force: true }), { discard: true });
    const success = yield* Effect.reduce(
        runs,
        (): boolean => true,
        (passed, invocation, index) =>
            passed && !dry
                ? Effect.map(
                      spawner.exitCode(
                          ChildProcess.make(
                              Array.headNonEmpty(invocation),
                              [...Array.tailNonEmpty(invocation), ...words, ...Option.match(stage, { onNone: () => [], onSome: (at) => ['-archivePath', join(at, `${index}.xcarchive`)] }), ...Option.match(Array.get(results, index), { onNone: () => [], onSome: (at) => ['-resultBundlePath', at] })],
                              { cwd: root, env: { ...cleared, ...stated }, extendEnv: true, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' },
                          ),
                      ),
                      (code) => code === 0,
                  )
                : Effect.as(dry ? Effect.logInfo(`Would run ${CommandLine.display(invocation)}`) : Effect.void, passed),
    );
    const staged = yield* Effect.forEach(success ? Option.toArray(stage) : [], (at) =>
        Effect.map(
            fs.readDirectory(at, { recursive: true }),
            Array.filterMap((entry) => (applications.includes(basename(entry)) ? Result.succeed([join(at, entry), join(sep, ...entry.split(sep).slice(2))] as const) : Result.failVoid)),
        ),
    );
    yield* Effect.forEach(staged.flat(), ([from, to]) => install(from, to));
    const registering = yield* Effect.filter(staged.flat(), ([, to]) => fs.exists(join(to, 'Contents', 'Library', 'LaunchAgents')));
    yield* Effect.forEach(registering, ([, to]) => spawned(['open', '-n', '-g', '-j', '-W', to], { cwd: root }), { discard: true });
    const running = !success || dry || applications.length === 0 ? [] : (yield* spawned(['ps', '-Ao', 'comm='], { cwd: root })).split('\n');
    const stale = applications.filter((application) => running.some((executable) => executable.includes(join(sep, application, 'Contents', 'MacOS', sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => basename(application, '.app')).join(', ')} runs the build it launched from, relaunch it to run this one`);
    return { success };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Run, function* (stated, context) {
    return yield* commands(stated, context.root);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { commands, configured, execute, Run };
