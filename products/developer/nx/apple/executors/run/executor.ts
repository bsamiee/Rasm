import { basename, dirname, join, resolve, sep } from 'node:path';
import { env } from 'node:process';
import { installed } from '@rasm/nx/executor';
import { executor, spawned, Words } from '@rasm/nx/inference';
import { Array, Config, Effect, FileSystem, Function, Option, Record, Result, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

class Run extends Schema.Opaque<Run>()(
    Schema.Struct({
        runs: Schema.String.pipe(Schema.NonEmptyArray, Schema.Array).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
        applications: Schema.String.pipe(Schema.Array, Schema.optionalKey).annotate({
            description:
                "Bundle names of the applications the invocations archive, each invocation taking -archivePath <n>.xcarchive in a temporary folder, once every invocation succeeds each bundle under an archive's Products copied beside its path below / and swapped with the bundle installed there in one renamex_np, so a failed invocation leaves the installed bundle untouched, then each one holding Contents/Library/LaunchAgents launched once more with open -n -g -j -W, behind the user's windows, so its SMAppService registration replaces the agents still running the build it swapped out, and one whose Contents/MacOS holds the executable a running process launched from named in the last line as needing a relaunch",
        }),
        results: Schema.String.pipe(Schema.Array, Schema.optionalKey).annotate({
            description:
                "Result bundle paths, each removed before the first invocation runs, as xcodebuild refuses a result bundle path that exists, the invocation at a path's index writing its bundle through -resultBundlePath into a temporary folder that every bundle leaves for its path once the last invocation ends, as xcodebuild resolves the package graph again at each write under the root of a package it tests and invalidates the test bundles it runs",
        }),
        dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each invocation and runs none' }),
        __unparsed__: Words,
        outputs: Schema.String.pipe(Schema.Array, Schema.optionalKey).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
    }).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed and git status.branch off after the caller's GIT_CONFIG_<n> entries, stopping at the first failure" }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = Config.map(Config.withDefault(Config.Int('GIT_CONFIG_COUNT'), 0), (count) => ({ GIT_CONFIG_COUNT: `${count + 1}`, [`GIT_CONFIG_KEY_${count}`]: 'status.branch', [`GIT_CONFIG_VALUE_${count}`]: 'false' }));

const commands = Effect.fnUntraced(function* ({ runs, applications = [], results = [], dryRun: dry = false, __unparsed__: words = [] }: Run, root: string) {
    const [fs, spawner, stated] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, configured]);
    const cleared = Record.map(
        Record.filter(env, (_, name) => name.startsWith('NX_') || name === 'LERNA_PACKAGE_NAME'),
        Function.constUndefined,
    );
    const stage = yield* (applications.length === 0 && results.length === 0) || dry ? Effect.succeedNone : Effect.asSome(fs.makeTempDirectoryScoped());
    const archives = Option.filter(stage, () => applications.length > 0);
    const bundles = Option.match(stage, { onNone: () => [], onSome: (at) => results.map((kept, index) => ({ written: join(at, `${index}.xcresult`), kept: resolve(root, kept) })) });
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
                              [...Array.tailNonEmpty(invocation), ...words, ...Option.match(archives, { onNone: () => [], onSome: (at) => ['-archivePath', join(at, `${index}.xcarchive`)] }), ...Option.match(Array.get(bundles, index), { onNone: () => [], onSome: ({ written }) => ['-resultBundlePath', written] })],
                              { cwd: root, env: { ...cleared, ...stated }, extendEnv: true, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' },
                          ),
                      ),
                      (code) => code === 0,
                  )
                : Effect.as(dry ? Effect.logInfo(`Would run ${CommandLine.display(invocation)}`) : Effect.void, passed),
    );
    const ended = yield* Effect.filter(bundles, ({ written }) => fs.exists(written));
    yield* Effect.forEach(ended, ({ written, kept }) => Effect.andThen(fs.makeDirectory(dirname(kept), { recursive: true }), fs.rename(written, kept)), { discard: true });
    const staged = yield* Effect.forEach(success ? Option.toArray(archives) : [], (at) =>
        Effect.map(
            fs.readDirectory(at, { recursive: true }),
            Array.filterMap((entry) => (applications.includes(basename(entry)) ? Result.succeed([join(at, entry), join(sep, ...entry.split(sep).slice(2))] as const) : Result.failVoid)),
        ),
    );
    const running = yield* installed(staged.flat());
    const registering = yield* Effect.filter(staged.flat(), ([, to]) => fs.exists(join(to, 'Contents', 'Library', 'LaunchAgents')));
    yield* Effect.forEach(registering, ([, to]) => spawned(['open', '-n', '-g', '-j', '-W', to], { cwd: root }), { discard: true });
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
