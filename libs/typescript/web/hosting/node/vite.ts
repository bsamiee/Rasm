import { join } from 'node:path';
import { Cause, Effect } from 'effect';
import type { Plugin } from 'vite';

// --- [CONSTANTS] -----------------------------------------------------------------------

const layout = { environment: 'node', entry: 'index', assets: 'assets', port: 'PORT' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const node = (entry: string): Plugin => ({
    name: '@rasm/hosting/node/vite',
    config: () => ({ environments: { [layout.environment]: { consumer: 'server', resolve: { noExternal: true }, build: { outDir: join('dist', layout.environment), assetsDir: 'chunks', emptyOutDir: true, copyPublicDir: false, rolldownOptions: { input: { [layout.entry]: entry }, platform: 'node' } } } } }),
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[layout.environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: (cause) => new Cause.UnknownError(cause, `${layout.environment} environment build of ${entry} failed`) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { layout, node };
