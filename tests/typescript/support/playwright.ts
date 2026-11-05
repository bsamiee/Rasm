import { join } from 'node:path';
import AxeBuilder from '@axe-core/playwright';
import { layer } from '@effect/platform-node/NodeFileSystem';
import { type Page, test } from '@playwright/test';
import { Cause, Effect, FileSystem, Function, Schema } from 'effect';
import { createServer } from 'vite';
import { e2eMode, located } from '../../../vite.shared.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Report = Schema.fromJsonString(Schema.Unknown, { space: 2 });

// --- [COMPOSITION] ---------------------------------------------------------------------

const testApp = (name: string) =>
    test.extend<{ readonly baseURL: string }, { readonly origin: string }>({
        origin: [
            // biome-ignore lint/correctness/noEmptyPattern: Playwright reads fixture dependencies from the first parameter's object pattern
            ({}, use, { parallelIndex, project }) =>
                Effect.gen(function* () {
                    const root = yield* FileSystem.FileSystem.use((fs) => fs.realPath(join(project.testDir, 'node_modules', name)));
                    const server = yield* Effect.acquireRelease(
                        Effect.tryPromise(() => createServer({ root, mode: e2eMode, cacheDir: located(join('.cache', 'vite', String(parallelIndex)), root), server: { host: '127.0.0.1', port: 0, forwardConsole: false }, logLevel: 'warn' })),
                        (vite) => Effect.promise(vite.close.bind(vite)),
                    );
                    const { resolvedUrls } = yield* Effect.tryPromise(() => server.listen());
                    const origin = yield* Effect.fromNullishOr(resolvedUrls?.local[0]);
                    yield* Effect.promise(() => use(origin));
                }).pipe(
                    Effect.scoped,
                    Effect.mapError((cause) => new Cause.UnknownError(cause, `${name} dev server failed in ${project.name}`)),
                    Effect.provide(layer),
                    Effect.runPromise,
                ),
            { scope: 'worker' },
        ],
        baseURL: ({ origin }, use) => use(origin),
    });
const expect = test.expect.extend({
    toPassAxe: Function.flow(
        Effect.fnUntraced(function* (page: Page, select: (builder: AxeBuilder) => AxeBuilder = Function.identity) {
            const { violations } = yield* Effect.promise(() => select(new AxeBuilder({ page })).analyze());
            const report = yield* Schema.encodeEffect(Report)(violations);
            yield* Effect.promise(() => test.info().attach('axe', { body: report, contentType: 'application/json' }));
            const pass = violations.length === 0;
            return { pass, message: () => (pass ? `${page.url()} passes axe rules` : `${page.url()} violates axe rules ${violations.map(({ id }) => id).join(', ')}\n${report}`) };
        }),
        Effect.runPromise,
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { expect, testApp };
