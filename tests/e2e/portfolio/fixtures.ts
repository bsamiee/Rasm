import { type Page, test } from '@playwright/test';
import { resource } from '@rasm/testing/playwright';
import { Effect } from 'effect';
import { content } from '#/rendering';
import { OWNER, type Published, platform, published, served } from '#/served';

// --- [COMPOSITION] ---------------------------------------------------------------------

const site = test.extend<{ readonly owner: Page }, { readonly site: { readonly origin: string; readonly publish: (shown: Published) => Promise<void>; readonly reset: () => Promise<void> } }>({
    site: [resource(Effect.map(Effect.provide(served, platform), ({ origin, reset }) => ({ origin, publish: (shown: Published) => Effect.runPromise(Effect.provide(published(origin, shown), platform)), reset: () => Effect.runPromise(reset) }))), { scope: 'worker' }],
    baseURL: ({ site: { origin } }, use) => use(origin),
    owner: ({ browser, baseURL }, use) =>
        Effect.acquireDisposable(Effect.promise(() => browser.newContext({ extraHTTPHeaders: OWNER, ...(baseURL !== undefined && { baseURL }) }))).pipe(
            Effect.flatMap((context) => Effect.promise(() => context.newPage().then(use))),
            Effect.scoped,
            Effect.runPromise,
        ),
});

const blank = site.extend<{ readonly blank: void }>({ blank: [({ site: { reset } }, use) => reset().then(use), { auto: true }] });

const shown = site.extend<object, { readonly published: void }>({
    published: [
        ({ site: { reset, publish } }, use) =>
            reset()
                .then(() => publish(content))
                .then(use),
        { scope: 'worker', auto: true },
    ],
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { blank, shown };
