// --- [CONSTANTS] -----------------------------------------------------------------------

const containers = {
    postgres: 'postgres:19.0-alpine3.24',
    clickhouse: 'clickhouse/clickhouse-server:26.10.10.3-alpine',
    rustfs: 'rustfs/rustfs:1.0.7',
    ryuk: 'testcontainers/ryuk:0.14.0',
    ssh: { image: 'rasm/test-ssh', context: new URL('../../libs/typescript/network/ssh/', import.meta.url) },
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { containers };
