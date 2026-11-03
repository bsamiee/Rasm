import { globSync } from 'node:fs';
import { createRequire } from 'node:module';
import { basename, dirname, join, relative } from 'node:path';
import AxeBuilder from '@axe-core/playwright';
import { expect as base, defineConfig, type Page, test } from '@playwright/test';
import { Effect, Function } from 'effect';
import { createServer, searchForWorkspaceRoot } from 'vite';
import { testMode } from '../../../vite.shared.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ROOT = searchForWorkspaceRoot(import.meta.dirname);
const _SUITES = dirname(import.meta.dirname);

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = (name: string) =>
    test.extend<{ readonly baseURL: string }, { readonly origin: string }>({
        origin: [
            // biome-ignore lint/correctness/noEmptyPattern: Playwright reads fixture dependencies from the first parameter's object pattern
            ({}, use, { parallelIndex, project }) =>
                Effect.gen(function* () {
                    const root = dirname(yield* Effect.try({ try: () => createRequire(join(project.testDir, 'package.json')).resolve(`${name}/package.json`), catch: Function.identity }));
                    yield* Effect.acquireUseRelease(
                        Effect.tryPromise({ try: () => createServer({ root, mode: testMode, cacheDir: `node_modules/.vite/test-${parallelIndex}`, server: { host: '127.0.0.1', port: 0, forwardConsole: false }, logLevel: 'warn' }), catch: Function.identity }),
                        (server) =>
                            Effect.tryPromise({ try: () => server.listen(), catch: Function.identity }).pipe(
                                Effect.flatMap(({ resolvedUrls }) => Effect.fromNullishOr(resolvedUrls?.local[0])),
                                Effect.flatMap((origin) => Effect.promise(() => use(origin))),
                            ),
                        (server) => Effect.tryPromise({ try: () => server.close(), catch: Function.identity }),
                    );
                }).pipe(
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
const config = (directory: string) => {
    const artifacts = join(_ROOT, '.artifacts', 'typescript', 'playwright', basename(directory));
    return defineConfig({
        outputDir: join(artifacts, 'results'),
        updateSnapshots: 'none',
        reporter: [['list'], ['html', { outputFolder: join(artifacts, 'report'), open: 'never' }]],
        use: { trace: { mode: 'retain-on-failure', snapshots: { dom: true, aria: true, screen: true } }, locale: 'en-US', timezoneId: 'UTC', reducedMotion: 'reduce' },
        projects: globSync('e2e-*', { cwd: _SUITES })
            .filter((name) => !relative(directory, join(_SUITES, name)).startsWith('..'))
            .map((name) => ({ name, testDir: join(_SUITES, name) })),
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app, config, expect };
