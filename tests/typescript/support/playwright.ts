import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import AxeBuilder from '@axe-core/playwright';
import { expect as base, type Page, test } from '@playwright/test';
import { Effect, Function } from 'effect';
import { createServer } from 'vite';
import { testMode } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = (name: string) =>
    test.extend<{ readonly baseURL: string }, { readonly origin: string }>({
        origin: [
            // biome-ignore lint/correctness/noEmptyPattern: Playwright reads fixture dependencies from the first parameter's object pattern
            ({}, use, { parallelIndex, project }) =>
                Effect.gen(function* () {
                    const root = yield* Effect.try(() => dirname(createRequire(join(project.testDir, 'package.json')).resolve(`${name}/package.json`)));
                    const server = yield* Effect.acquireRelease(
                        Effect.tryPromise(() => createServer({ root, mode: testMode, cacheDir: `node_modules/.vite/${testMode}-${parallelIndex}`, server: { host: '127.0.0.1', port: 0, forwardConsole: false }, logLevel: 'warn' })),
                        (opened) => Effect.orDie(Effect.tryPromise(() => opened.close())),
                    );
                    const { resolvedUrls } = yield* Effect.tryPromise(() => server.listen());
                    const origin = yield* Effect.fromNullishOr(resolvedUrls?.local[0]);
                    yield* Effect.promise(() => use(origin));
                }).pipe(
                    Effect.scoped,
                    Effect.mapError((cause) => new Error(`${name} dev server failed in ${project.name}`, { cause })),
                    Effect.runPromise,
                ),
            { scope: 'worker' },
        ],
        baseURL: ({ origin }, use) => use(origin),
    });
const expect = base.extend({
    async toPassAxe(page: Page, select: (builder: AxeBuilder) => AxeBuilder = Function.identity) {
        const { violations } = await select(new AxeBuilder({ page })).analyze();
        const report = JSON.stringify(violations, null, 2);
        await test.info().attach('axe', { body: report, contentType: 'application/json' });
        return { name: 'toPassAxe', pass: violations.length === 0, message: () => `${violations.length} axe violations at ${page.url()}\n${report}` };
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { app, expect };
