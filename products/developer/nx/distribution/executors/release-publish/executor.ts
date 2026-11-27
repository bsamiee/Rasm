import { credentialed, Dry, forwarded, released } from '@rasm/nx/executor';
import { dried, executor, Words } from '@rasm/nx/inference';
import { Array, Cause, Effect, Option, Schema, Struct } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { located } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Dry.mapFields(Struct.assign({ credential: Schema.String.annotate({ description: "Environment variable holding the store's credential, where it is unset or empty the run publishes nothing, names the variable, and succeeds" }), __unparsed__: Words })).annotate({
    title: 'Release publish',
    description:
        "Publishes each host package the target's dependency targets wrote to its host's store through the store's own upload, where the credential variable is set, the store lacks that version, and the checked-out commit holds the project's tag nx release made from release.releaseTag.pattern, and names the missing variable, tag, or the first upload a store takes by hand alone, where it is not; runs through nx release publish, a dry run previewing every project",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Publish, function* ({ credential, dryRun, __unparsed__: words = [] }, context) {
    const [dry, token, { project, target, outputs, packages }] = yield* Effect.all([dried(dryRun), credentialed(credential), located(context)]);
    const stored = packages.flatMap((found) => Array.fromNullishOr(found.host.publish).map((store) => ({ found, ...store })));
    const pushed = Effect.fnUntraced(function* ({ found, store, command, held: listed, push }: (typeof stored)[number]) {
        const kept = Option.isSome(token) && (yield* listed(found, store, token.value));
        const skipped = yield* Option.match(
            Option.filter(token, () => !kept),
            {
                onNone: () => Effect.succeedSome(kept ? 'the store holds that version' : `${credential} is not set`),
                onSome: (held) => push(found, store, held, forwarded(words)),
            },
        );
        return yield* Option.match(skipped, { onNone: () => Effect.logInfo(`Published ${found.at} to ${store} with ${command}`), onSome: (reason) => Effect.logWarning(`Skipped ${found.at} for ${store}: ${reason}`) });
    }, Effect.provide(FetchHttpClient.layer));
    const delivered = Array.match(stored, {
        onEmpty: () => new Cause.UnknownError(outputs, `${project}: no host package sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first`),
        onNonEmpty: Effect.forEach((row) => (dry ? Effect.logInfo(`Would publish ${row.found.at} to ${row.store} through ${row.command}`) : pushed(row)), { discard: true }),
    });
    yield* dry ? delivered : Effect.when(delivered, Effect.map(released(context), Option.isSome));
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
