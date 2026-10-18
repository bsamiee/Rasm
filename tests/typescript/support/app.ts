import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { test } from '@playwright/test';
import { Effect } from 'effect';
import { createServer } from 'vite';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _targets = {
    cloudflare: (directory: string, worker: number) =>
        Effect.acquireRelease(
            Effect.promise(() => createServer({ root: directory, mode: 'test', cacheDir: `node_modules/.vite/test-${worker}`, server: { host: '127.0.0.1', port: 0, forwardConsole: false }, logLevel: 'warn' }).then((server) => server.listen())),
            (server) => Effect.promise(() => server.close()),
        ).pipe(Effect.flatMap((server) => Effect.fromNullishOr(server.resolvedUrls?.local[0]))),
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const app = (name: string) =>
    test.extend<{ readonly baseURL: string }, { readonly target: keyof typeof _targets; readonly origin: string }>({
        target: ['cloudflare', { scope: 'worker', option: true }],
        origin: [
            ({ target }, use, { parallelIndex, project }) =>
                _targets[target](dirname(createRequire(join(project.testDir, 'package.json')).resolve(`${name}/package.json`)), parallelIndex).pipe(
                    Effect.flatMap((url) => Effect.promise(() => use(url))),
                    Effect.scoped,
                    Effect.runPromise,
                ),
            { scope: 'worker', auto: true },
        ],
        baseURL: ({ origin }, use) => use(origin),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { app };
