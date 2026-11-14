import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { Dry, dried, refused, run } from '#/executor';
import { located } from '#/host-packages';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Dry.annotate({ title: 'Release publish', description: "Publishes each host package the target's dependency targets wrote to its host's store through the store's own upload; runs through nx release publish" });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(
    function* (stated: typeof Publish.Type, context: ExecutorContext) {
        const [dry, { project, target, outputs, packages }] = yield* Effect.all([dried(stated), located(context)]);
        const stored = packages.flatMap((found) => Array.fromNullishOr(found.host.publish).map((store) => ({ found, ...store })));
        yield* Array.match(stored, {
            onEmpty: () => Effect.fail(refused(project, `no host package sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first`)()),
            onNonEmpty: Effect.forEach(({ found, store, command, push }) => (dry ? Effect.logInfo(`Would publish ${found.at} to ${store} through ${command}`) : Effect.andThen(push(found, store), Effect.logInfo(`Published ${found.at} to ${store} through ${command}`))), { discard: true }),
        });
        return { success: true };
    },
    Effect.provide(FetchHttpClient.layer),
    run,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Publish };
