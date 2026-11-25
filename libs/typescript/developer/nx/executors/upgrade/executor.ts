import { Effect, Option, Path, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { Dry } from '#/executor';
import { dried, executor, spawned } from '#/inference';

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Dry.mapFields(
    Struct.assign({
        commands: Schema.NonEmptyArray(
            Schema.Struct({
                apply: Schema.NonEmptyArray(Schema.String).annotate({ description: "The tool's argument list moving requirements and locks" }),
                preview: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)).annotate({ description: "The tool's own dry run argument list, run under a dry run in place of apply; absent prints apply and runs nothing" }),
                env: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)).annotate({ description: 'Variables this command alone runs with over the inherited environment' }),
            }),
        ).annotate({ description: 'Commands run in order, stopping at the first failure' }),
        cwd: Schema.String.annotate({ description: 'Folder the commands run in, relative to the workspace root', default: '.' }),
    }),
).annotate({ title: 'Upgrade', description: "Runs a tool's upgrade commands in order, and under a dry run each command's own dry run in its place, printing a command without one" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Upgrade, function* ({ commands, cwd, dryRun }, context) {
    const [dry, path] = yield* Effect.all([dried(dryRun), Path.Path]);
    yield* Effect.forEach(
        commands,
        ({ apply, preview, env = {} }) =>
            Option.match(dry ? Option.fromUndefinedOr(preview) : Option.some(apply), {
                onNone: () => Effect.logInfo(`Would run ${CommandLine.display(apply)}`),
                onSome: (argv) => spawned(argv, { cwd: path.join(context.root, cwd), env, stdout: 'inherit', stderr: 'inherit' }),
            }),
        { discard: true },
    );
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Upgrade };
