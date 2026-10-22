import { join } from 'node:path';
import { Effect } from 'effect';
import type { Plugin } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const output = { environment: 'node', entry: 'index', assets: 'assets' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const node = (entry: string): Plugin => ({
    name: '@rasm/hosting/node/vite',
    config: () => ({ environments: { [output.environment]: { consumer: 'server', resolve: { noExternal: true }, build: { outDir: join('dist', output.environment), assetsDir: 'chunks', emptyOutDir: true, copyPublicDir: false, rolldownOptions: { input: { [output.entry]: entry }, platform: 'node' } } } } }),
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[output.environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise(() => builder.build(target))),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { node, output };
