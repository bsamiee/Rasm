import { join } from 'node:path';
import { Effect, Function } from 'effect';
import type { Plugin } from 'vite';
import { App } from '#/app/app';
import { HostingError } from '#/app/error';

// --- [CONSTANTS] -----------------------------------------------------------------------

const START = `\0@rasm/hosting/vite/${App.bundle.entry}`;
const STARTED = { id: new RegExp(`^${RegExp.escape(START)}$`, 'u') };

// --- [COMPOSITION] ---------------------------------------------------------------------

const nodeServer = ({ entry }: { readonly entry: string }): Plugin => ({
    name: '@rasm/hosting/vite',
    config: ({ build, environments }) => ({
        builder: {},
        environments: {
            [App.bundle.environment]: {
                consumer: 'server',
                resolve: { noExternal: true },
                build: {
                    outDir: environments?.[App.bundle.environment]?.build?.outDir ?? join(build?.outDir ?? 'dist', App.bundle.environment),
                    assetsDir: 'chunks',
                    emptyOutDir: true,
                    copyPublicDir: false,
                    sourcemap: true,
                    rolldownOptions: { input: { [App.bundle.entry]: START }, platform: 'node' },
                },
            },
        },
    }),
    resolveId: { filter: STARTED, handler: Function.identity },
    load: {
        filter: STARTED,
        handler: () => `import { setSourceMapsSupport } from 'node:module';\nsetSourceMapsSupport(true);\nawait import(${JSON.stringify(entry)});`,
    },
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Effect.fromNullishOr(builder.environments[App.bundle.environment]).pipe(
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: HostingError.of('Build', `${App.bundle.environment} environment of ${entry}`) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeServer };
