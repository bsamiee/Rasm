import { join } from 'node:path';
import { Cause, Effect } from 'effect';
import type { Plugin } from 'vite';

// --- [OPERATIONS] ----------------------------------------------------------------------

const vite = (entry: string): Plugin => ({
    name: '@rasm/hosting/node/vite',
    config: () => ({
        environments: { [vite.layout.environment]: { consumer: 'server', resolve: { noExternal: true }, build: { outDir: join('dist', vite.layout.environment), assetsDir: 'chunks', emptyOutDir: true, copyPublicDir: false, rolldownOptions: { input: { [vite.layout.entry]: entry }, platform: 'node' } } } },
    }),
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[vite.layout.environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: (cause) => new Cause.UnknownError(cause, `${vite.layout.environment} environment build of ${entry} failed`) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});
vite.layout = { environment: 'node', entry: 'index', assets: 'assets', portVariable: 'PORT' } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { vite };
