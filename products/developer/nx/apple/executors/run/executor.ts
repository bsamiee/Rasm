import { basename, join, sep } from 'node:path';
import { env } from 'node:process';
import { executor, spawned, Words } from '@rasm/nx';
import { Array, Config, Effect, Function, Record, Schema } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

class Run extends Schema.Opaque<Run, unknown>()(
    Schema.Struct({
        runs: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
        applications: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Bundle names of the applications the invocations install, a running executable inside one named in the last line as needing a relaunch' }),
        dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each invocation and runs none' }),
        __unparsed__: Words,
        outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
    }).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed and git status.branch off after the caller's GIT_CONFIG_<n> entries, stopping at the first failure" }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const configured = Config.map(Config.withDefault(Config.Int('GIT_CONFIG_COUNT'), 0), (count) => ({ GIT_CONFIG_COUNT: `${count + 1}`, [`GIT_CONFIG_KEY_${count}`]: 'status.branch', [`GIT_CONFIG_VALUE_${count}`]: 'false' }));

const commands = Effect.fnUntraced(function* ({ runs, applications = [], dryRun: dry = false, __unparsed__: words = [] }: Run, root: string) {
    const [spawner, stated] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, configured]);
    const cleared = Record.map(
        Record.filter(env, (_, name) => name.startsWith('NX_') || name === 'LERNA_PACKAGE_NAME'),
        Function.constUndefined,
    );
    const success = yield* Effect.reduce(
        runs,
        (): boolean => true,
        (passed, invocation) =>
            passed && !dry
                ? Effect.map(spawner.exitCode(ChildProcess.make(Array.headNonEmpty(invocation), Array.appendAll(Array.tailNonEmpty(invocation), words), { cwd: root, env: { ...cleared, ...stated }, extendEnv: true, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' })), (code) => code === 0)
                : Effect.as(dry ? Effect.logInfo(`Would run ${CommandLine.display(invocation)}`) : Effect.void, passed),
    );
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
