import { join } from 'node:path';
import { Effect } from 'effect';
import type { Plugin } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const environment = 'node';

// --- [OPERATIONS] ----------------------------------------------------------------------

const node = (entry: string): Plugin => ({
    name: '@rasm/hosting/node',
    config: () => ({ environments: { [environment]: { consumer: 'server', resolve: { noExternal: true }, build: { outDir: join('dist', environment), assetsDir: 'chunks', emptyOutDir: true, copyPublicDir: false, rolldownOptions: { input: { index: entry }, platform: 'node' } } } } }),
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise(() => builder.build(target))),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { environment, node };
