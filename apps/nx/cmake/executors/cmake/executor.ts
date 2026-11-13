import { dirname, resolve } from 'node:path';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { Array, Effect, FileSystem, Schema, type Scope } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Invocation = Schema.NonEmptyArray(Schema.String);
const Cmake = Schema.Struct({
    cwd: Schema.String.annotate({ description: 'Folder holding CMakePresets.json, relative to the workspace root, every invocation runs in it' }),
    configure: Invocation.annotate({ description: 'The cmake invocation that configures the build tree' }),
    commands: Schema.Array(Invocation).annotate({ description: 'Invocations run in order in the configured tree, stopping at the first failure' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to the last invocation' }),
    env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: "Environment variables every invocation runs with, over the caller's" }),
    outputs: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: "Files the invocations write, which the target's outputs name through {options.outputs.<n>}, each folder created before the run" }),
}).annotate({ title: 'CMake', description: 'Configures a CMake build tree with its configure invocation, then runs the cmake, ctest, and clang tool invocations of the target in order, stopping at the first failure' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const sequenced = (cwd: string, invocations: readonly (typeof Invocation.Type)[], env: Readonly<Record<string, string>> = {}) =>
    Effect.forEach(invocations, ([command, ...args]) => Completed.output(ChildProcess.make(command, args, { cwd, env, extendEnv: true, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' })), { discard: true });

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => Effect.runPromise(Effect.provide(Effect.scoped(effect), layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ cwd, configure, commands, args = [], env, outputs = [] }: typeof Cmake.Type, context: ExecutorContext) {
    const fs = yield* FileSystem.FileSystem;
    yield* Effect.forEach(outputs, (file) => fs.makeDirectory(dirname(resolve(context.root, file)), { recursive: true }), { discard: true });
    yield* sequenced(resolve(context.root, cwd), [configure, ...Array.matchRight(commands, { onEmpty: () => commands, onNonEmpty: (leading, last) => [...leading, Array.appendAll(last, args)] })], env);
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Cmake, execute, run, sequenced };
