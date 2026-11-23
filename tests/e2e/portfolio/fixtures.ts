import { it as base } from '@rasm/testing/playwright';
import { Effect } from 'effect';
import { type Published, platform, published, served } from '#/served';

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = base.extend<{ readonly served: { readonly origin: string; readonly publish: (content: Published) => Promise<void> } }>({
    served: ({}, use) =>
        Effect.runPromise(
            Effect.provide(
                Effect.scoped(Effect.flatMap(served, ({ origin }) => Effect.promise(() => use({ origin, publish: (content) => Effect.runPromise(Effect.provide(published(origin, content), platform)) })))),
                platform,
            ),
        ),
    baseURL: ({ served: { origin } }, use) => use(origin),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { it };
