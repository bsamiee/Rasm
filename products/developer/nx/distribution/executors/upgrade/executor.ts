import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Option, Path, Record, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { readPath, resolveTokens, type TokenScope } from 'nx/src/tasks-runner/tokens';
import { Dry, dried, run } from '#/executor.ts';
import { spawned } from '#/inference.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Dry.mapFields(
    Struct.assign({
        commands: Schema.NonEmptyArray(
            Schema.Struct({
                apply: Schema.NonEmptyArray(Schema.String).annotate({ description: "The tool's argument list moving requirements and locks" }),
                preview: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)).annotate({ description: "The tool's own dry run argument list, run under a dry run in place of apply; absent prints apply and runs nothing" }),
                env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: 'Variables this command alone runs with over the inherited environment, each value resolving Nx tokens over the executor context at run, {root} naming the workspace root, so the target holds no machine path' }),
            }),
        ).annotate({ description: 'Commands run in order, stopping at the first failure' }),
        cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    }),
).annotate({ title: 'Upgrade', description: "Runs a tool's upgrade commands in order, and under a dry run each command's own dry run in its place, printing a command without one" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ commands, cwd, ...stated }: typeof Upgrade.Type, context: ExecutorContext) {
    const [dry, path] = yield* Effect.all([dried(stated), Path.Path]);
    const scope: TokenScope = { isToken: (name) => Object.hasOwn(context, name), read: (name, at) => readPath(context, [name, ...Array.fromNullishOr(at)].join('.')), noValue: (token) => token };
    yield* Effect.forEach(
        commands,
        ({ apply, preview, env = {} }) =>
            Option.match(dry ? Option.fromUndefinedOr(preview) : Option.some(apply), {
                onNone: () => Effect.logInfo(`Would run ${CommandLine.display(apply)}`),
                onSome: (argv) => Effect.asVoid(spawned(argv, { cwd: path.join(context.root, cwd), env: Record.map(env, (value) => resolveTokens(value, scope)), stdout: 'inherit', stderr: 'inherit' })),
            }),
        { discard: true },
    );
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Upgrade };
