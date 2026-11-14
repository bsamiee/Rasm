import { basename, join, sep } from 'node:path';
import { env } from 'node:process';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Record, Schema } from 'effect';
import { printed, ran, run } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({
    runs: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: "Environment variables every invocation runs with, over the caller's" }),
    applications: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Application bundles the invocations install, a running one named in the last line as needing a relaunch' }),
    __unparsed__: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Words after -- of the run, appended after the configured arguments', $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
}).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed, stopping at the first failure" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const commands = Effect.fnUntraced(function* ({ runs, env: stated = {}, applications = [], __unparsed__: words = [] }: typeof Run.Type, root: string) {
    const cleared = Record.map(
        Record.filter(env, (_, name) => name.startsWith('NX_') || name === 'LERNA_PACKAGE_NAME'),
        () => undefined,
    );
    const success = yield* Effect.reduce(
        runs,
        (): boolean => true,
        (passed, invocation) => (passed ? Effect.map(ran(Array.appendAll(invocation, words), { cwd: root, env: { ...cleared, ...stated } }), (code) => code === 0) : Effect.succeed(false)),
    );
    const running = !success || applications.length === 0 ? [] : (yield* printed(['ps', '-Ao', 'comm='], { cwd: root })).split('\n');
    const stale = applications.filter((application) => running.some((executable) => executable.startsWith(join(application, 'Contents', 'MacOS', sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => basename(application, '.app')).join(', ')} runs the build it launched from, relaunch it to run this one`);
    return { success };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: typeof Run.Type, context: ExecutorContext) => run(commands(options, context.root));

// --- [EXPORTS] -------------------------------------------------------------------------

export { commands, execute, Run };
