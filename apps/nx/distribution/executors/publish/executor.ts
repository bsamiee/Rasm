import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Option, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { credentialed, Dry, dried, packaged, run, spawned, Words } from '#/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Dry.mapFields(
    Struct.assign({
        publish: Schema.NonEmptyArray(Schema.String).annotate({ description: "The tool's publish argument list, followed by each file of the target's dependentTasksOutputFiles input, none where the target states no such input" }),
        preview: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)).annotate({ description: "Arguments the tool's own dry run adds to publish, run under a dry run in place of publish; absent prints publish and runs nothing" }),
        credential: Schema.String.annotate({ description: "Environment variable holding the destination's credential, where it is unset or empty the run publishes nothing, names the variable, and succeeds" }),
        flag: Schema.optionalKey(Schema.String).annotate({ description: 'Argument the tool takes the credential under, for a tool reading no variable itself' }),
        __unparsed__: Words,
    }),
).annotate({
    title: 'Publish',
    description: "Delivers the files a project's packaging targets wrote to its registry or store through its tool's own publish command where the destination's credential is set, and names the missing variable where it is not; runs through nx release publish",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ publish, preview, credential, flag, __unparsed__: words = [], ...stated }: typeof Publish.Type, context: ExecutorContext) {
    const [dry, files, held] = yield* Effect.all([dried(stated), packaged(context), credentialed(credential)]);
    const command = Array.appendAll(Array.appendAll(publish, files), words);
    const keyed = Option.getOrElse(Option.all([Option.fromUndefinedOr(flag), held]), Array.empty<string>);
    const skipped = dry ? Effect.logInfo(`Would run ${CommandLine.display(command)}`) : Effect.logWarning(`Skipped ${CommandLine.display(command)}: ${credential} is not set`);
    yield* Option.match(dry ? Option.map(Option.fromUndefinedOr(preview), (added) => Array.appendAll(command, added)) : Option.as(held, Array.appendAll(command, keyed)), {
        onNone: () => skipped,
        onSome: (argv) => Effect.asVoid(spawned(argv, { cwd: context.root })),
    });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
