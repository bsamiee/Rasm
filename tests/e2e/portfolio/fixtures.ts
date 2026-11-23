import { type Page, test } from '@playwright/test';
import { resource } from '@rasm/testing/playwright';
import { Effect } from 'effect';
import { OWNER, type Published, platform, published, served } from '#/served';

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test.extend<{ readonly served: { readonly origin: string; readonly publish: (content: Published) => Promise<void> }; readonly owner: Page }>({
    served: resource(
        Effect.provide(
            Effect.map(served, ({ origin }) => ({ origin, publish: (content: Published) => Effect.runPromise(Effect.provide(published(origin, content), platform)) })),
            platform,
        ),
    ),
    baseURL: ({ served: { origin } }, use) => use(origin),
    owner: ({ browser, baseURL }, use) =>
        Effect.runPromise(
            Effect.scoped(
                Effect.flatMap(
                    Effect.acquireRelease(
                        Effect.promise(() => browser.newContext({ extraHTTPHeaders: OWNER, ...(baseURL !== undefined && { baseURL }) })),
                        (context) => Effect.promise(() => context.close()),
                    ),
                    (context) => Effect.promise(() => context.newPage().then(use)),
                ),
            ),
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { it };
