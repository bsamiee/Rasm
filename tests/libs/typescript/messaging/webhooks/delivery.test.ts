import { layer as crypto } from '@effect/platform-node/NodeCrypto';
import { assert, it } from '@effect/vitest';
import { transport } from '@rasm/testing/http';
import { Scheme, Webhook } from '@rasm/webhooks';
import { TailscaleEvent } from '@rasm/webhooks/tailscale';
import { Array, Effect, Exit, Fiber, Function, Layer, Option, Queue, Record, Redacted, Ref, Schedule, Schema } from 'effect';
import { Headers, HttpClient, HttpClientRequest, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { PersistedQueue } from 'effect/persistence';
import { TestClock } from 'effect/testing';
import { encoder, made, standard, tolerance, vectors } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const url = 'https://hooks.test/sender';
const digest = /^[0-9a-f]{64}$/u;
const node = { nodeID: 'nFJw3SRKTM59', deviceName: 'alice-workstation1.yak-bebop.ts.net', managedBy: 'alice@example.com', actor: 'alice@example.com', url: 'https://console.tailscale.com/admin/machines/100.12.345.67' };
const envelope = { timestamp: '2022-09-21T13:59:02.949217-04:00', version: 1, tailnet: 'example.com', message: 'Documented event' };
const documented = {
    Other: [
        { ...envelope, type: 'test', data: null },
        { ...envelope, type: 'webhookUpdated' },
        { ...envelope, type: 'nodeCreated', data: { nodeID: node.nodeID } },
    ],
    Node: [
        { ...envelope, type: 'nodeCreated', data: node },
        { ...envelope, type: 'nodeDeleted', data: node },
    ],
    KeyExpiry: [{ ...envelope, type: 'nodeKeyExpired', data: { ...node, actor: 'expiring-node-key-marker', expiration: '2022-11-08T18:44:46.979292Z' } }],
    Policy: [{ ...envelope, type: 'policyUpdate', data: { newPolicy: '{}', oldPolicy: '{}', url: 'https://console.tailscale.com/admin/acls', actor: 'admin@example.com' } }],
    Role: [{ ...envelope, type: 'userRoleUpdated', data: { user: 'alice@example.com', url: 'https://console.tailscale.com/admin/users', actor: 'admin@example.com', oldRoles: ['Member'], newRoles: ['Member', 'IT admin'] } }],
};

// --- [MODELS] --------------------------------------------------------------------------

const Counted = Schema.Struct({ count: Schema.Int });

// --- [OPERATIONS] ----------------------------------------------------------------------

const queues = Effect.provide(Layer.merge(crypto, Layer.provide(PersistedQueue.layer, PersistedQueue.layerStoreMemory)));
const posted = (headers: Headers.Headers, body: Uint8Array) =>
    HttpClientRequest.post(url, { headers }).pipe(
        HttpClientRequest.bodyUint8Array(body, 'application/json'),
        HttpClient.execute,
        Effect.map(({ status }) => status),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect.each([
    { status: 299, attempts: 1, delivered: true },
    { status: 300, attempts: 1, delivered: false },
    { status: 404, attempts: 1, delivered: false },
    { status: 429, attempts: 3, delivered: false },
    { status: 503, attempts: 3, delivered: false },
])(
    'sends a delivery answered $status $attempts times after its retry-after delays, delivered: $delivered',
    Effect.fnUntraced(function* ({ status, attempts, delivered }) {
        const [webhook, answered] = yield* Effect.all([made(Scheme.standard, standard.secret), Ref.make(0)]);
        const endpoint = Ref.update(answered, (count) => count + 1).pipe(Effect.as(HttpServerResponse.empty({ status, headers: { location: url, 'retry-after': '120' } })));
        const sending = yield* Effect.forkChild(Effect.exit(webhook.deliver(url, { id: standard.id, event: { test: 1 } }, Schedule.recurs(2))).pipe(Effect.provide(transport(endpoint))));
        yield* TestClock.adjust('239 seconds');
        const early = yield* Ref.get(answered);
        yield* TestClock.adjust('1 second');
        assert.deepStrictEqual([Exit.isSuccess(yield* Fiber.join(sending)), early, yield* Ref.get(answered)], [delivered, Math.min(attempts, 2), attempts]);
    }, Effect.provide(crypto)),
);
it.effect(
    'decodes documented Tailscale events to their cases, delivers a batch to a receiver, and answers refused requests by status',
    Effect.fnUntraced(function* () {
        const webhook = yield* TailscaleEvent.webhook([Redacted.make(vectors.tailscale.secret)]);
        const events = yield* Effect.all(Record.map(documented, (rows) => Schema.decodeEffect(TailscaleEvent.Batch)(rows)));
        const batch = Object.values(events).flat();
        const deliveries = yield* Queue.unbounded<Effect.Success<ReturnType<typeof webhook.verify>>>();
        const text = encoder.encode('not JSON');
        const statuses = yield* Effect.gen(function* () {
            yield* webhook.deliver(url, { id: 'derived', event: batch }, Schedule.recurs(0));
            const [signed, unparsed] = yield* Effect.all([webhook.sign('derived', vectors.tailscale.bytes), webhook.sign('derived', text)]);
            const unbounded = Headers.fromInput({ [Scheme.tailscale.signature]: 't=99999999999999999999,v1=00' });
            return yield* Effect.all({ accepted: posted(signed, vectors.tailscale.bytes), forged: posted(signed, text), unsigned: posted(Headers.empty, text), unbounded: posted(unbounded, text), unparsed: posted(unparsed, text) });
        }).pipe(Effect.provide(transport(webhook.receive((incoming) => Queue.offer(deliveries, incoming)))));
        const received = yield* Queue.takeAll(deliveries);
        assert.deepStrictEqual(
            [Record.map(events, (rows) => Array.map(rows, ({ _tag }) => _tag)), statuses, Array.map(received, ({ id, event }) => [digest.test(id), event.length])],
            [
                Record.map(documented, (rows, tag) => Array.map(rows, Function.constant(tag))),
                { accepted: 204, forged: 401, unsigned: 401, unbounded: 401, unparsed: 400 },
                [
                    [true, batch.length],
                    [true, 1],
                ],
            ],
        );
        assert.deepStrictEqual(received[0].event, batch);
    }, Effect.provide(crypto)),
);
it.effect(
    'works a repeated delivery once, and retries a refused delivery under its id with a new timestamp until the attempt limit',
    Effect.fnUntraced(function* () {
        const webhook = yield* Webhook({ scheme: Scheme.standard, tolerance, payload: Counted, secrets: [Redacted.make(standard.secret)] });
        const [worked, stamps] = yield* Effect.all([Ref.make(Array.empty<string>()), Ref.make(Array.empty<Option.Option<readonly [string, string]>>())]);
        const accept = yield* webhook.queued({ name: 'inbox' }, ({ id, event }, attempts) => Ref.update(worked, Array.append(`${id}:${event.count}:${attempts}`)));
        const endpoint = Effect.gen(function* () {
            const { headers } = yield* HttpServerRequest.HttpServerRequest;
            const seen = yield* Ref.updateAndGet(stamps, Array.append(Option.all([Headers.get(headers, 'webhook-id'), Headers.get(headers, 'webhook-timestamp')])));
            return seen.length === 1 || Option.contains(Headers.get(headers, 'webhook-id'), 'msg_refused') ? HttpServerResponse.empty({ status: 503 }) : yield* webhook.receive(accept);
        });
        const client = yield* Layer.build(transport(endpoint));
        const offer = yield* webhook.queued({ name: 'outbox', maxAttempts: 3, retrySchedule: Schedule.spaced('1 minute') }, (queued) => webhook.deliver(url, queued, Schedule.recurs(0))).pipe(Effect.provide(client));
        yield* Effect.all([offer({ id: 'msg_repeated', event: { count: 1 } }), offer({ id: 'msg_repeated', event: { count: 2 } })]);
        yield* TestClock.adjust('1 minute');
        yield* Effect.provide(webhook.deliver(url, { id: 'msg_repeated', event: { count: 1 } }, Schedule.recurs(0)), client);
        yield* offer({ id: 'msg_refused', event: { count: 3 } });
        yield* Effect.repeat(TestClock.adjust('1 minute'), { times: 4 });
        assert.deepStrictEqual(
            [yield* Ref.get(worked), yield* Ref.get(stamps)],
            [
                ['msg_repeated:1:1'],
                Array.map(
                    [
                        ['msg_repeated', '0'],
                        ['msg_repeated', '60'],
                        ['msg_repeated', '60'],
                        ['msg_refused', '60'],
                        ['msg_refused', '120'],
                        ['msg_refused', '180'],
                    ],
                    Option.some,
                ),
            ],
        );
    }, queues),
);
it.effect(
    'keeps working deliveries after work dies',
    Effect.fnUntraced(function* () {
        const webhook = yield* Webhook({ scheme: Scheme.standard, tolerance, payload: Counted, secrets: [Redacted.make(standard.secret)] });
        const worked = yield* Ref.make(Array.empty<string>());
        const accept = yield* webhook.queued({ name: 'defects', maxAttempts: 2, retrySchedule: Schedule.spaced('1 minute') }, ({ id, event }, attempts) => (event.count === 0 ? Effect.die(id) : Ref.update(worked, Array.append(`${id}:${attempts}`))));
        yield* Effect.all([accept({ id: 'dies', event: { count: 0 } }), accept({ id: 'works', event: { count: 1 } })]);
        yield* Effect.repeat(TestClock.adjust('1 minute'), { times: 2 });
        assert.deepStrictEqual(yield* Ref.get(worked), ['works:1']);
    }, queues),
);
