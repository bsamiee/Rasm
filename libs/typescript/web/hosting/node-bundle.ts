// --- [CONSTANTS] -----------------------------------------------------------------------

const nodeBundle = {
    layout: { environment: 'node', entry: 'index', assets: 'assets', portVariable: 'PORT', readiness: '/readyz' } as const,
    services: {
        store: { image: 'rustfs/rustfs:latest' },
        identity: { image: 'authelia/authelia:latest' },
        proxy: { image: 'caddy:latest' },
        database: { image: 'postgres:19rc1', environment: { POSTGRES_INITDB_ARGS: '--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST' }, command: ['postgres', '-c', 'shared_preload_libraries=pg_stat_statements'] },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeBundle };
