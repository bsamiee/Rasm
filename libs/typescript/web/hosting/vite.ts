import { join } from 'node:path';
import { Effect } from 'effect';
import type { Plugin } from 'vite';
import { HostingError } from '#/error';
import { nodeBundle } from '#/node-bundle';

// --- [COMPOSITION] ---------------------------------------------------------------------

const nodeServer = ({ entry }: { readonly entry: string }): Plugin => ({
    name: '@rasm/hosting/vite',
    config: ({ build, environments }) => ({
        environments: {
            [nodeBundle.environment]: {
                consumer: 'server',
                resolve: { noExternal: true },
                build: {
                    outDir: environments?.[nodeBundle.environment]?.build?.outDir ?? join(build?.outDir ?? 'dist', nodeBundle.environment),
                    assetsDir: 'chunks',
                    emptyOutDir: true,
                    copyPublicDir: false,
                    rolldownOptions: { input: { [nodeBundle.entry]: entry }, platform: 'node' },
                },
            },
        },
    }),
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[nodeBundle.environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: HostingError.build(nodeBundle.environment, entry) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeServer };
