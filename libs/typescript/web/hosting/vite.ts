import { join } from 'node:path';
import { Effect } from 'effect';
import type { Plugin } from 'vite';
import { App } from '#/app';
import { HostingError } from '#/error';

// --- [COMPOSITION] ---------------------------------------------------------------------

const nodeServer = ({ entry }: { readonly entry: string }): Plugin => ({
    name: '@rasm/hosting/vite',
    config: ({ build, environments }) => ({
        environments: {
            [App.bundle.environment]: {
                consumer: 'server',
                resolve: { noExternal: true },
                build: {
                    outDir: environments?.[App.bundle.environment]?.build?.outDir ?? join(build?.outDir ?? 'dist', App.bundle.environment),
                    assetsDir: 'chunks',
                    emptyOutDir: true,
                    copyPublicDir: false,
                    rolldownOptions: { input: { [App.bundle.entry]: entry }, platform: 'node' },
                },
            },
        },
    }),
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[App.bundle.environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: HostingError.build(App.bundle.environment, entry) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeServer };
