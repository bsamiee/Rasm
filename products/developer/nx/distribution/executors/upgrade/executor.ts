import type { ExecutorContext } from '@nx/devkit';
import { Effect, Option, Path, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { Dry, dried, run } from '#/executor';
import { spawned } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Dry.mapFields(
    Struct.assign({
        commands: Schema.NonEmptyArray(
            Schema.Struct({
                apply: Schema.NonEmptyArray(Schema.String).annotate({ description: "The tool's argument list moving requirements and locks" }),
                preview: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)).annotate({ description: "The tool's own dry run argument list, run under a dry run in place of apply; absent prints apply and runs nothing" }),
            }),
        ).annotate({ description: 'Commands run in order, stopping at the first failure' }),
        cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    }),
).annotate({ title: 'Upgrade', description: "Runs a tool's upgrade commands in order, and under a dry run each command's own dry run in its place, printing a command without one" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ commands, cwd, ...stated }: typeof Upgrade.Type, context: ExecutorContext) {
    const [dry, path] = yield* Effect.all([dried(stated), Path.Path]);
    yield* Effect.forEach(
        commands,
        ({ apply, preview }) =>
            Option.match(dry ? Option.fromUndefinedOr(preview) : Option.some(apply), {
                onNone: () => Effect.logInfo(`Would run ${CommandLine.display(apply)}`),
                onSome: (argv) => Effect.asVoid(spawned(argv, { cwd: path.join(context.root, cwd), stdout: 'inherit', stderr: 'inherit' })),
            }),
        { discard: true },
    );
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Upgrade };
