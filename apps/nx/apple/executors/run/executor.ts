import { execFileSync, spawnSync } from 'node:child_process';
import { basename, join, sep } from 'node:path';
import { env } from 'node:process';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Effect, Record, Schema, type Scope } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({
    runs: Schema.Array(Schema.NonEmptyArray(Schema.String)).annotate({ description: 'Invocations the plugin infers, each a program and its arguments' }),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: "Environment variables every invocation runs with, over the caller's" }),
    applications: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Application bundles the invocations install, a running one named in the last line as needing a relaunch' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to every invocation' }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the configuration writes, which the target's outputs name through {options.outputs.<n>}" }),
}).annotate({ title: 'Run', description: "Runs swift and xcodebuild invocations in order with Nx's per-task environment removed, stopping at the first failure" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const commands = Effect.fnUntraced(function* ({ runs, env: stated = {}, applications = [], args = [] }: typeof Run.Type, root: string) {
    const kept = Record.filter(env, (_, name) => !(name.startsWith('NX_') || name === 'LERNA_PACKAGE_NAME'));
    const success = yield* Effect.sync(() => runs.every(([program, ...given]) => spawnSync(program, [...given, ...args], { cwd: root, env: { ...kept, ...stated }, stdio: 'inherit' }).status === 0));
    const running = !success || applications.length === 0 ? [] : (yield* Effect.try(() => execFileSync('ps', ['-Ao', 'comm='], { encoding: 'utf8' }))).split('\n');
    const stale = applications.filter((application) => running.some((executable) => executable.startsWith(join(application, 'Contents', 'MacOS', sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => basename(application, '.app')).join(', ')} runs the build it launched from, relaunch it to run this one`);
    return { success };
});

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => effect.pipe(Effect.tapCause(Effect.logError), Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: typeof Run.Type, context: ExecutorContext) => run(commands(options, context.root));

// --- [EXPORTS] -------------------------------------------------------------------------

export { commands, execute, Run, run };
