import { Effect } from 'effect';
import type { Plugin } from 'vite';
import { HostingError } from '#/error.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodeBundle = (entry: string, outDir: string): Plugin => ({
    name: '@rasm/hosting/node-bundle',
    config: () => ({
        environments: {
            [nodeBundle.layout.environment]: { consumer: 'server', resolve: { noExternal: true }, build: { outDir, assetsDir: 'chunks', emptyOutDir: true, copyPublicDir: false, rolldownOptions: { input: { [nodeBundle.layout.entry]: entry }, platform: 'node' } } },
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
nodeBundle.layout = { environment: 'node', entry: 'index', assets: 'assets', portVariable: 'PORT', readiness: '/readyz' } as const;
nodeBundle.services = {
    store: { image: 'rustfs/rustfs:latest' },
    identity: { image: 'authelia/authelia:latest' },
    proxy: { image: 'caddy:latest' },
    database: { image: 'postgres:19rc1', environment: { POSTGRES_INITDB_ARGS: '--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST' }, command: ['postgres', '-c', 'shared_preload_libraries=pg_stat_statements'] },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeBundle };
