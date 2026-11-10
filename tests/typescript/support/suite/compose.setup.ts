import { fileURLToPath } from 'node:url';
import { Effect } from 'effect';
import { Wait } from 'testcontainers';
import { environment } from '#/compose';
import { provided } from '#/vitest';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly cache: { readonly host: string; readonly port: number };
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const served = 6379;

// --- [COMPOSITION] ---------------------------------------------------------------------

const setup = provided(
    'cache',
    Effect.map(
        environment(fileURLToPath(new URL('../../../../infra/compose.yaml', import.meta.url)), ['cache'], (compose) => compose.withEnvironment({ CACHE_PORT: '0' }).withDefaultWaitStrategy(Wait.forListeningPorts())),
        (services) => ({ host: services.getContainer('cache-1').getHost(), port: services.getContainer('cache-1').getMappedPort(served) }),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { setup };
