import { join } from 'node:path';
import { Effect } from 'effect';
import type { Plugin } from 'vite';
import { HostingError } from '#/hosting-error';

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodeBundle = (entry: string): Plugin => ({
    name: '@rasm/hosting/vite',
    config: () => ({
        environments: {
            [nodeBundle.layout.environment]: { consumer: 'server', resolve: { noExternal: true }, build: { outDir: join('dist', nodeBundle.layout.environment), assetsDir: 'chunks', emptyOutDir: true, copyPublicDir: false, rolldownOptions: { input: { [nodeBundle.layout.entry]: entry }, platform: 'node' } } },
        },
    }),
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[nodeBundle.layout.environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: HostingError.build(nodeBundle.layout.environment, entry) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});
nodeBundle.layout = { environment: 'node', entry: 'index', assets: 'assets', portVariable: 'PORT' } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeBundle };
