import { Array, Effect, Option, pipe, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { credentialed, Dry, forwarded, packaged, released } from '#/executor';
import { dried, executor, spawned, Words } from '#/inference';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ALTERNATIVES = new Intl.ListFormat('en', { type: 'disjunction' });

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Dry.mapFields(
    Struct.assign({
        publish: Schema.NonEmptyArray(Schema.String).annotate({ description: "The tool's publish argument list, followed by each file of the target's dependentTasksOutputFiles input, none where the target states no such input" }),
        preview: Schema.optionalKey(Schema.NonEmptyArray(Schema.String)).annotate({ description: "Arguments the tool's own dry run adds to publish, run under a dry run in place of publish; absent prints publish and runs nothing" }),
        credential: Schema.Union([Schema.String, Schema.NonEmptyArray(Schema.String)]).annotate({
            description: "Environment variable holding the destination's credential, or variables in order of preference any one of which grants it (an OIDC token request beside a registry token), where every one is unset or empty the run publishes nothing, names them, and succeeds",
        }),
        flag: Schema.optionalKey(Schema.String).annotate({ description: 'Argument the tool takes the credential under, for a tool reading no variable itself' }),
        __unparsed__: Words,
    }),
).annotate({
    title: 'Publish',
    description:
        "Delivers the files a project's packaging targets wrote to its registry or store through its tool's own publish command where the destination's credential is set and the checked-out commit holds the project's tag nx release made from release.releaseTag.pattern, and names the missing variable or tag where it is not; runs through nx release publish, a dry run previewing every project",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Publish, function* ({ publish, preview, credential, flag, dryRun, __unparsed__: words = [] }, context) {
    const [dry, { project, files }, held] = yield* Effect.all([dried(dryRun), packaged(context), credentialed(credential)]);
    const command = pipe(publish, Array.appendAll(files), Array.appendAll(forwarded(words)));
    const keyed = Option.getOrElse(Option.all([Option.fromUndefinedOr(flag), held]), Array.empty<string>);
    const skipped = dry ? Effect.logInfo(`Would run ${CommandLine.display(command)}`) : Effect.logWarning(`Skipped ${CommandLine.display(command)}: ${ALTERNATIVES.format(Array.ensure(credential))} is not set`);
    const delivered = Option.match(dry ? Option.map(Option.fromUndefinedOr(preview), Array.prependAll(command)) : Option.as(held, Array.appendAll(command, keyed)), {
        onNone: () => skipped,
        onSome: (argv) => spawned(argv, { cwd: context.root, stdout: 'inherit', stderr: 'inherit' }),
    });
    yield* dry ? delivered : Effect.flatMap(released(context), Option.match({ onNone: () => Effect.logInfo(`Skipped ${CommandLine.display(command)}: HEAD holds no tag of ${project}, so nx release released no version of it at this commit`), onSome: () => delivered }));
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
