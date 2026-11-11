import { fileURLToPath } from 'node:url';
import { assert, inject, it } from '@effect/vitest';
import { Effect } from 'effect';
import { createClient } from 'redis';
import { Wait } from 'testcontainers';
import { environment } from '#/compose';

// --- [CONSTANTS] -----------------------------------------------------------------------

const served = 6379;
const started = 120_000;

// --- [OPERATIONS] ----------------------------------------------------------------------

const opened = (host: string, port: number) => createClient({ socket: { host, port } });

const asked = <A>(host: string, port: number, ask: (client: ReturnType<typeof opened>) => Promise<A>) =>
    Effect.acquireUseRelease(
        Effect.tryPromise(() => {
            const client = opened(host, port);
            return client.connect().then(() => client);
        }),
        (client) => Effect.tryPromise(() => ask(client)),
        (client) => Effect.sync(() => client.destroy()),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it.live(
    'answers a client through the address global setup provides',
    () =>
        Effect.map(
            asked(inject('cache').host, inject('cache').port, (client) => client.ping()),
            (answer) => assert.strictEqual(answer, 'PONG'),
        ),
    started,
);

it.live(
    'loses every key when the cache service of infra/compose.yaml restarts under a project of its own',
    Effect.fnUntraced(function* () {
        const services = yield* environment(fileURLToPath(new URL('../../../../infra/compose.yaml', import.meta.url)), ['cache'], (compose) => compose.withEnvironment({ CACHE_PORT: '0' }).withDefaultWaitStrategy(Wait.forListeningPorts()));
        const cache = services.getContainer('cache-1');
        const written = yield* asked(cache.getHost(), cache.getMappedPort(served), (client) => client.set('sheet', 'plan').then(() => client.get('sheet')));
        yield* Effect.tryPromise(() => cache.restart());
        const kept = yield* asked(cache.getHost(), cache.getMappedPort(served), (client) => client.get('sheet'));
        assert.deepStrictEqual([written, kept], ['plan', null]);
    }),
    started,
);
