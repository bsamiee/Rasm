import { assert, describe, it } from '@effect/vitest';
import { Client, Server } from '@rasm/mcp';
import { Array, Deferred, Duration, Effect, Layer, MutableRef, Option, Queue, Schema, SubscriptionRef } from 'effect';
import { McpClient, McpProtocol, McpTest } from 'effect/ai';
import { FetchHttpClient } from 'effect/http';
import { ENDPOINT, MANIFEST, opened, STATEFUL } from '#/conformance';
import { app, Probe } from '#/fixtures';

// --- [OPERATIONS] ----------------------------------------------------------------------

const connected = Effect.fnUntraced(function* (protocol: (typeof STATEFUL)[number]) {
    const [probe, scheme] = yield* Effect.all([Probe.make, Schema.decodeEffect(Server.Scheme)(MANIFEST.name)]);
    const client = yield* McpTest.makeClient(Layer.provide(app(probe), Server.layer({ scheme, retention: Duration.minutes(1) })), { protocols: [protocol] });
    yield* Effect.addFinalizer(() => Deferred.succeed(probe.release, undefined));
    const updated = Queue.take(client.notifications).pipe(
        Effect.repeat({ until: ({ method }) => method === 'notifications/resources/updated' }),
        Effect.map(({ payload }) => payload),
        Effect.timeoutOption('2 seconds'),
    );
    return { probe, client, updated };
});

const listed = (mcp: McpClient.McpClient) =>
    Effect.map(
        mcp.listTools(),
        Array.some(({ name }) => name === 'test_simple_text'),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe.each(STATEFUL)('$protocolVersion server', (protocol) => {
    it.live(
        'notifies a subscriber of a resource with its URI when the value of the resource changes',
        Effect.fnUntraced(function* () {
            const { probe, client, updated } = yield* connected(protocol);
            yield* client.subscribe({ uri: 'test://watched-resource' });
            yield* SubscriptionRef.set(probe.watched, 'moved');
            assert.deepStrictEqual(yield* updated, Option.some({ uri: 'test://watched-resource' }));
        }, Effect.scoped),
    );
});

describe('client', () => {
    it.live(
        'opens a new session and answers the call when the server it was connected to restarted and no longer holds its session',
        Effect.fnUntraced(function* () {
            const protocol = McpProtocol.v2025_11_25;
            const serving = MutableRef.make((yield* opened(protocol)).handler);
            const fetch: typeof globalThis.fetch = (input, init) => MutableRef.get(serving)(new Request(input, init));
            const connection = McpClient.layerHttp({ name: 'reconnecting', version: '1.0.0', url: ENDPOINT, protocol }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.Fetch, fetch)));
            const listing = Client.use(({ run }) => run(listed));
            const restarted = Effect.gen(function* () {
                const before = yield* listing;
                MutableRef.set(serving, (yield* opened(protocol)).handler);
                return { before, after: yield* listing };
            });
            assert.deepStrictEqual(yield* Effect.provide(restarted, Client.layer(connection)), { before: true, after: true });
        }, Effect.scoped),
    );
});
