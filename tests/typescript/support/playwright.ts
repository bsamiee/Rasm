import { realpath } from 'node:fs/promises';
import { join } from 'node:path';
import AxeBuilder from '@axe-core/playwright';
import { expect as base, type Page, test } from '@playwright/test';
import { Effect, Function, Schema } from 'effect';
import { createServer } from 'vite';
import { located, testMode } from '../../../vite.shared.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = (name: string) =>
    test.extend<{ readonly baseURL: string }, { readonly origin: string }>({
        origin: [
            // biome-ignore lint/correctness/noEmptyPattern: Playwright reads fixture dependencies from the first parameter's object pattern
            ({}, use, { parallelIndex, project }) =>
                Effect.gen(function* () {
                    const root = yield* Effect.tryPromise(() => realpath(join(project.testDir, 'node_modules', name)));
                    const server = yield* Effect.acquireRelease(
                        Effect.tryPromise(() => createServer({ root, mode: testMode, cacheDir: located(join('.cache', 'vite', String(parallelIndex)), root), server: { host: '127.0.0.1', port: 0, forwardConsole: false }, logLevel: 'warn' })),
                        (vite) => Effect.promise(() => vite.close()),
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
    toPassAxe: (page: Page, select: (builder: AxeBuilder) => AxeBuilder = Function.identity) =>
        Effect.gen(function* () {
            const { violations } = yield* Effect.promise(() => select(new AxeBuilder({ page })).analyze());
            const report = yield* Schema.encodeEffect(Schema.fromJsonString(Schema.Unknown, { space: 2 }))(violations).pipe(Effect.orDie);
            yield* Effect.promise(() => test.info().attach('axe', { body: report, contentType: 'application/json' }));
            return { name: 'toPassAxe', pass: violations.length === 0, message: () => `${violations.length} axe violations at ${page.url()}\n${report}` };
        }).pipe(Effect.runPromise),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { app, expect };
