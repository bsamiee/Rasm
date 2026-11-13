import { executor } from '@rasm/nx-devkit/executor';
import { ran, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Path, Schema } from 'effect';

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
    Effect.forEach(
        invocations,
        ([command, ...args]) =>
            Effect.filterOrFail(
                ran(command, args, { cwd, env, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' }),
                ({ code }) => code === 0,
                ({ code }) => ToolError.make({ subject: command, detail: `exited ${code}` }),
            ),
        { discard: true },
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Cmake,
    Effect.fnUntraced(function* ({ cwd, configure, commands, args = [], env, outputs = [] }, context) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        yield* Effect.forEach(outputs, (file) => fs.makeDirectory(path.dirname(path.resolve(context.root, file)), { recursive: true }), { discard: true });
        yield* sequenced(path.resolve(context.root, cwd), [configure, ...Array.matchRight(commands, { onEmpty: () => commands, onNonEmpty: (leading, last) => [...leading, Array.appendAll(last, args)] })], env);
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Cmake, execute, sequenced };
