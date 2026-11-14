import { spawnSync } from 'node:child_process';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Option, Schema, Struct, UndefinedOr } from 'effect';
import { CommandLine } from 'effect/process';
import { credentialed, Dry, dried, packaged, run } from '#/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Command = Schema.NonEmptyArray(Schema.String);
const Publish = Dry.mapFields(
    Struct.assign({
        publish: Command.annotate({ description: "The tool's publish argument list, followed by each file the target's dependency targets declare as outputs" }),
        preview: Schema.optionalKey(Command).annotate({ description: "The tool's own dry-run form, run in place of publish under a dry run, absent prints publish and runs nothing" }),
        credential: Schema.String.annotate({ description: "Environment variable holding the destination's credential, where it is unset or empty the run publishes nothing, names the variable, and succeeds" }),
        flag: Schema.optionalKey(Schema.String).annotate({ description: 'Argument the tool takes the credential under, for a tool reading no variable itself' }),
    }),
).annotate({
    title: 'Publish',
    description: "Delivers the files a project's packaging targets wrote to its registry or store through its tool's own publish command where the destination's credential is set, and names the missing variable where it is not; runs through nx release publish",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ publish, preview, credential, flag, ...stated }: typeof Publish.Type, context: ExecutorContext) {
    const [dry, files, held] = yield* Effect.all([dried(stated), packaged(context), credentialed(credential)]);
    const keyed = Option.getOrElse(Option.all([Option.fromUndefinedOr(flag), held]), Array.empty<string>);
    const noted = dry ? Effect.logInfo(`Would run ${CommandLine.display([...publish, ...files])}`) : Effect.logWarning(`Skipped ${CommandLine.display([...publish, ...files])}: ${credential} is not set`);
    return yield* UndefinedOr.match(dry ? preview : Option.getOrUndefined(Option.as(held, publish)), {
        onUndefined: () => Effect.as(noted, { success: true }),
        onDefined: ([command, ...rest]) => Effect.sync(() => ({ success: spawnSync(command, [...rest, ...files, ...keyed], { cwd: context.root, stdio: 'inherit' }).status === 0 })),
    });
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
