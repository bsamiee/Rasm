import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, Option } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { credentialed, Dry, dried, refused, run } from '#/executor';
import { located } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Dry.annotate({
    title: 'Release publish',
    description: "Publishes each host package the target's dependency targets wrote to its host's store through the store's own upload, where the store's credential variable is set and the store lacks that version, and names the missing variable where it is not; runs through nx release publish",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(
    function* (stated: typeof Publish.Type, context: ExecutorContext) {
        const [dry, { project, target, outputs, packages }] = yield* Effect.all([dried(stated), located(context)]);
        const stored = packages.flatMap((found) => Array.fromNullishOr(found.host.publish).map((store) => ({ found, ...store })));
        const pushed = Effect.fnUntraced(function* ({ found, store, command, credential, held: listed, push }: (typeof stored)[number]) {
            const token = yield* credentialed(credential);
            const kept = Option.isSome(token) && (yield* listed(found, store));
            const reason = kept ? 'the store holds that version' : `${credential} is not set`;
            return yield* Option.match(
                Option.filter(token, () => !kept),
                {
                    onNone: () => Effect.logWarning(`Skipped ${found.at} for ${store}: ${reason}`),
                    onSome: (held) => Effect.andThen(push(found, store, held), Effect.logInfo(`Published ${found.at} to ${store} through ${command}`)),
                },
            );
        });
        yield* Array.match(stored, {
            onEmpty: () => Effect.fail(refused(project, `no host package sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first`)()),
            onNonEmpty: Effect.forEach((row) => (dry ? Effect.logInfo(`Would publish ${row.found.at} to ${row.store} through ${row.command}`) : pushed(row)), { discard: true }),
        });
        return { success: true };
    },
    Effect.provide(FetchHttpClient.layer),
    run,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
