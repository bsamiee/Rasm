import { basename, join, sep } from 'node:path';
import { env } from 'node:process';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { Effect, Record, Schema, type Scope } from 'effect';
import { ChildProcess } from 'effect/process';

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
    const cleared = Record.fromIterableWith([...Object.keys(env).filter((name) => name.startsWith('NX_')), 'LERNA_PACKAGE_NAME'], (name) => [name, undefined]);
    yield* Effect.forEach(runs, ([program, ...given]) => Completed.output(ChildProcess.make(program, [...given, ...args], { cwd: root, env: { ...cleared, ...stated }, extendEnv: true, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' })), { discard: true });
    const running = applications.length === 0 ? [] : (yield* Completed.output(ChildProcess.make('ps', ['-Ao', 'comm='], { cwd: root, stdin: 'ignore' }))).split('\n');
    const stale = applications.filter((application) => running.some((executable) => executable.startsWith(join(application, 'Contents', 'MacOS', sep))));
    yield* stale.length === 0 ? Effect.void : Effect.logInfo(`${stale.map((application) => basename(application, '.app')).join(', ')} runs the build it launched from, relaunch it to run this one`);
    return { success: true };
});

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => Effect.runPromise(Effect.provide(Effect.scoped(effect), layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: typeof Run.Type, context: ExecutorContext) => run(commands(options, context.root));

// --- [EXPORTS] -------------------------------------------------------------------------

export { commands, execute, Run, run };
