import { assert, it } from '@effect/vitest';
import { Scheme, Webhook } from '@rasm/webhooks';
import { TailscaleEvent } from '@rasm/webhooks/tailscale';
import { WebhookTest } from '@rasm/webhooks/testing';
import { Array, DateTime, Duration, Effect, Function, Layer, Option, Queue, Record, Redacted, Ref, Schedule, Schema } from 'effect';
import type { Digest, Mac, Signature } from 'effect/crypto';
import { Headers, HttpClient, HttpClientRequest, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { PersistedQueue } from 'effect/persistence';
import { TestClock } from 'effect/testing';

// --- [CONSTANTS] -----------------------------------------------------------------------

const encoder = new TextEncoder();
const tolerance = Duration.fromInputUnsafe('5 minutes');
const second = Duration.toMillis('1 second');
const digest = /^[0-9a-f]{64}$/u;
const url = 'https://hooks.test/sender';
const label = /\bv1a?\b|sha256/u;
const tail = /.{4}$/u;
const keyed = (signed: string) => `${signed.replace(',', ',v1=00ff,v0=abcd,')},v1=abcd`;
const standard = {
    scheme: Scheme.standard,
    secret: 'whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw',
    foreign: 'whsec_C2FVsBQIhrscChlQIMV+b5sSYspob7oD',
    id: 'msg_p5jXN8AQM9LWM0D4loKWxJek',
    at: 1_614_265_330,
    body: '{"test": 2432232314}',
    accepted: 'verified',
    late: 'Stale',
    crowded: (signed: string) => `v1,AAAA v9,BBBB ${signed} v1,CCCC`,
} as const;
const pair = { private: 'whsk_nWGxne/9WmC6hEr0kuwsxERJxWl7MmkZcDusAxyuf2A=', public: 'whpk_11qYAYKxCrfVS/7TyWQHOg7hcvPapiMlrwIaaPcHURo=', foreign: 'whsk_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=' } as const;
const vectors = {
    standard: { ...standard, bytes: encoder.encode(standard.body), signed: 'v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=' },
    binary: { ...standard, bytes: Uint8Array.from({ length: 256 }, (_, index) => index), signed: 'v1,sEcZ042G8Crqgxbybb3y0eDxQR3TcfYsk11I1XULhSI=', accepted: 'SchemaError' },
    rust: { ...standard, secret: standard.foreign, foreign: standard.secret, id: 'msg_27UH4WbU6Z5A5EzD8u03UvzRbpk', at: 1_649_367_553, bytes: encoder.encode('{"email":"test@example.com","username":"test_user"}'), signed: 'v1,tZ1I4/hDygAJgO5TYxiSd6Sd0kDW6hPenDe+bTa3Kkw=' },
    ed25519: { ...standard, secret: pair.private, foreign: pair.foreign, bytes: encoder.encode(standard.body), signed: 'v1a,fldxM4gAKugP6nnt1hdz3sgGfZ6d99nzrMFnZOELIxbzEHoVmAb2ADpkJK7zgPePmPsle0zV9jSeGlHFG2NVAw==' },
    svix: { ...standard, scheme: Scheme.svix, secret: 'whsec_plJ3nmyCDGBKInavdOK15jsl', id: 'msg_loFOjxBNrRLzqYUf', at: 1_731_705_121, bytes: encoder.encode('{"event_type":"ping","data":{"success":true}}'), signed: 'v1,rAvfW3dJ/X/qxhsaXPOyyCGmRKsaKWcsNccKXlIktD0=' },
    github: {
        ...standard,
        scheme: Scheme.github,
        secret: "It's a Secret to Everybody",
        bytes: encoder.encode('Hello, World!'),
        signed: 'sha256=757107ea0eb2509fc211221cce984b8a37570b6d7586c22c46f4379c8b043e17',
        accepted: 'SchemaError',
        late: 'SchemaError',
        crowded: (signed: string) => `sha256=00ff,${signed},sha256=abcd`,
    },
    tailscale: {
        ...standard,
        scheme: Scheme.tailscale,
        secret: 'tskey-webhook-kExample1CNTRL-0123456789abcdef',
        at: 1_663_781_880,
        bytes: encoder.encode('[{"timestamp":"2022-09-21T13:37:51.658918-04:00","version":1,"type":"test","tailnet":"example.com","message":"This is a test event","data":null}]'),
        signed: 't=1663781880,v1=1d3b2cdcad5c8c64af84284e332b174a087ebf35d68a417c46371ec97574d3f5',
        crowded: keyed,
    },
    stripe: { ...standard, scheme: Scheme.stripe, secret: 'whsec_stripeexamplesecret', at: 1_492_774_577, bytes: encoder.encode('{"id":"evt_1","object":"event"}'), signed: 't=1492774577,v1=3c9d0dd9cf25b86ddd4ce4a4d6adbf58ea11e924b829fd7d28481310f7411b9e', crowded: keyed },
} as const;
const clocks = { early: -Duration.toMillis(tolerance) - second, soon: -Duration.toMillis(tolerance), edge: Duration.toMillis(tolerance), late: Duration.toMillis(tolerance) + second };
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const Counted = Schema.Struct({ count: Schema.Int });
const outcome = <A, R>(effect: Effect.Effect<A, Effect.Error<ReturnType<Webhook<typeof Schema.Json>['verify']>>, R>) => effect.pipe(Effect.unwrapReason('WebhookError'), Effect.match({ onSuccess: Function.constant('verified'), onFailure: ({ _tag }) => _tag }));
const made = (scheme: Scheme, ...secrets: Array.NonEmptyReadonlyArray<string>) => Webhook({ scheme, tolerance, payload: Schema.Json, secrets: Array.map(secrets, (secret) => Redacted.make(secret)) });
const queues = (crypto: Layer.Layer<Mac.Mac | Signature.Signature | Digest.Digest>) => Effect.provide(Layer.merge(crypto, Layer.provide(PersistedQueue.layer, PersistedQueue.layerStoreMemory)));
const posted = (headers: Headers.Headers, body: Uint8Array) =>
    HttpClientRequest.post(url, { headers }).pipe(
        HttpClientRequest.bodyUint8Array(body, 'application/json'),
        HttpClient.execute,
        Effect.map(({ status }) => status),
    );

const cases = (crypto: Layer.Layer<Mac.Mac | Signature.Signature | Digest.Digest>) => {
    it.effect(
        'signs the known-answer vector of each scheme row, verifies it among other signatures, and refuses another body, a cut signature, another label, another key, and no signature',
        Effect.fnUntraced(function* () {
            const results = yield* Effect.all(
                Record.map(
                    vectors,
                    Effect.fnUntraced(function* ({ scheme, secret, foreign, id, at, bytes, signed, crowded }) {
                        const [webhook, stranger] = yield* Effect.all([made(scheme, secret), made(scheme, foreign)]);
                        yield* TestClock.setTime(Duration.toMillis(Duration.seconds(at)));
                        const [headers, forged] = yield* Effect.all([webhook.sign(id, bytes), stranger.sign(id, bytes)]);
                        const rewritten = Record.map({ crowded: crowded(signed), cut: crowded(signed.replace(tail, '')), relabelled: signed.replace(label, 'v9') }, (text) => Headers.set(headers, scheme.signature, text));
                        const presented = yield* Effect.all(Record.map({ signed: headers, ...rewritten, forged, unsigned: Headers.remove(headers, scheme.signature) }, (changed) => outcome(webhook.verify(changed, bytes))));
                        return { header: Headers.get(headers, scheme.signature), body: yield* outcome(webhook.verify(headers, bytes.subarray(1))), ...presented };
                    }),
                ),
            );
            assert.deepStrictEqual(
                results,
                Record.map(vectors, ({ signed, accepted }) => ({ header: Option.some(signed), body: 'Mismatch', signed: accepted, crowded: accepted, cut: 'Mismatch', relabelled: 'Mismatch', forged: 'Mismatch', unsigned: 'Unsigned' })),
            );
        }, Effect.provide(crypto)),
    );
    it.effect(
        'accepts the timestamp of each scheme row at the tolerance in either direction and refuses one a second past it',
        Effect.fnUntraced(function* () {
            const results = yield* Effect.all(
                Record.map(
                    vectors,
                    Effect.fnUntraced(function* ({ scheme, secret, id, at, bytes }) {
                        const [webhook, signedAt] = [yield* made(scheme, secret), Duration.toMillis(Duration.seconds(at))];
                        const headers = yield* Effect.andThen(TestClock.setTime(signedAt), webhook.sign(id, bytes));
                        return yield* Effect.all(Record.map(clocks, (offset) => Effect.andThen(TestClock.setTime(signedAt + offset), outcome(webhook.verify(headers, bytes)))));
                    }),
                ),
            );
            assert.deepStrictEqual(
                results,
                Record.map(vectors, ({ accepted, late }) => ({ early: late, soon: accepted, edge: accepted, late })),
            );
        }, Effect.provide(crypto)),
    );
    it.effect(
        'accepts either secret of a sender signing with two, in each header syntax, and verifies with a public key that cannot sign',
        Effect.fnUntraced(function* () {
            const { id, bytes, secret, foreign } = vectors.standard;
            const rotated = yield* Effect.all(
                Record.map(
                    { spaced: Scheme.standard, keyed: Scheme.stripe },
                    Effect.fnUntraced(function* (scheme) {
                        const [both, former, latter, mixed, stranger] = yield* Effect.all([made(scheme, secret, foreign), made(scheme, secret), made(scheme, foreign), made(scheme, vectors.svix.secret, foreign), made(scheme, vectors.svix.secret)]);
                        const [doubled, single] = yield* Effect.all([both.sign(id, bytes), former.sign(id, bytes)]);
                        return yield* Effect.forEach([former.verify(doubled, bytes), latter.verify(doubled, bytes), mixed.verify(doubled, bytes), both.verify(single, bytes), stranger.verify(doubled, bytes), latter.verify(single, bytes)], outcome);
                    }),
                ),
            );
            const [signer, verifier, shared] = yield* Effect.all([made(Scheme.standard, pair.private), made(Scheme.standard, pair.public), made(Scheme.standard, secret)]);
            const signed = yield* signer.sign(id, bytes);
            const asymmetric = yield* Effect.all([outcome(verifier.verify(signed, bytes)), Effect.match(verifier.sign(id, bytes), { onSuccess: Function.constant('signed'), onFailure: ({ _tag }) => _tag }), outcome(shared.verify(signed, bytes))]);
            const accepted = ['verified', 'verified', 'verified', 'verified', 'Mismatch', 'Mismatch'];
            assert.deepStrictEqual([rotated, asymmetric], [{ spaced: accepted, keyed: accepted }, ['verified', 'IllegalArgumentError', 'Mismatch']]);
        }, Effect.provide(crypto)),
    );
    it.effect(
        'keys a delivery by the id its signature covers, and by its body where the signature covers none',
        Effect.fnUntraced(function* () {
            const { id, bytes, secret } = vectors.standard;
            const identified = Effect.fnUntraced(function* (scheme: Scheme, change: (headers: Headers.Headers) => Headers.Headers) {
                const webhook = yield* made(scheme, secret);
                const [initial, repeated] = yield* Effect.all([webhook.sign(id, bytes), Effect.andThen(TestClock.adjust('1 minute'), webhook.sign(id, bytes))]);
                return yield* Effect.forEach([initial, repeated, change(initial)], (headers) => Effect.match(webhook.verify(headers, bytes), { onSuccess: (delivery) => delivery.id, onFailure: ({ _tag }) => _tag }));
            });
            const [covered, uncovered, unidentified] = yield* Effect.all([identified(Scheme.standard, Headers.set('webhook-id', 'replayed')), identified(Scheme.github, Headers.set('x-github-delivery', 'replayed')), identified(Scheme.tailscale, Function.identity)]);
            assert.deepStrictEqual([covered, Array.dedupe(uncovered).map((key) => digest.test(key)), Array.dedupe(unidentified).map((key) => digest.test(key))], [[id, id, 'WebhookError'], [true], [true]]);
        }, Effect.provide(crypto)),
    );
    it.live(
        'exchanges signatures with the standardwebhooks reference library',
        Effect.fnUntraced(function* () {
            const { scheme, secret, id, body } = standard;
            const [webhook, now, standardwebhooks] = yield* Effect.all([made(scheme, secret), DateTime.now, Effect.promise(() => import('standardwebhooks'))]);
            const reference = new standardwebhooks.Webhook(secret);
            const ours = yield* webhook.sign(id, encoder.encode(body));
            const theirs = Headers.fromInput({ 'webhook-id': id, 'webhook-timestamp': `${DateTime.toEpochSeconds(now)}`, [scheme.signature]: reference.sign(id, DateTime.toDate(now), body) });
            const accepted = yield* Effect.try(() => reference.verify(body, { ...ours }));
            assert.deepStrictEqual([accepted, yield* webhook.verify(theirs, encoder.encode(body))], [{ test: 2_432_232_314 }, { id, event: { test: 2_432_232_314 } }]);
        }, Effect.provide(crypto)),
    );
    it.effect(
        'decodes each documented Tailscale event to its case, lists the event types of its cases, delivers a batch to a receiver, and answers refused requests by status before reading their body',
        Effect.fnUntraced(function* () {
            const webhook = yield* TailscaleEvent.webhook([Redacted.make(vectors.tailscale.secret)]);
            const events = yield* Effect.all(Record.map(documented, (rows) => Schema.decodeEffect(TailscaleEvent.Batch)(rows)));
            const batch = Object.values(events).flat();
            const { deliveries, client } = yield* WebhookTest.capture(webhook);
            const text = encoder.encode('not JSON');
            const statuses = yield* Effect.gen(function* () {
                yield* webhook.deliver(url, { id: 'derived', event: batch });
                const [signed, unparsed] = yield* Effect.all([webhook.sign('derived', vectors.tailscale.bytes), webhook.sign('derived', text)]);
                const unbounded = Headers.fromInput({ [Scheme.tailscale.signature]: 't=99999999999999999999,v1=00' });
                return yield* Effect.all({ accepted: posted(signed, vectors.tailscale.bytes), forged: posted(signed, text), unsigned: posted(Headers.empty, text), unbounded: posted(unbounded, text), unparsed: posted(unparsed, text) });
            }).pipe(Effect.provide(client));
            const received = yield* Queue.takeAll(deliveries);
            assert.deepStrictEqual(
                [
                    Record.map(
                        events,
                        Array.map(({ _tag }) => _tag),
                    ),
                    statuses,
                    Array.map(received, ({ id, event }) => [digest.test(id), event.length]),
                ],
                [
                    Record.map(documented, (rows, tag) => Array.map(rows, Function.constant(tag))),
                    { accepted: 204, forged: 401, unsigned: 401, unbounded: 401, unparsed: 400 },
                    [
                        [true, batch.length],
                        [true, 1],
                    ],
                ],
            );
            assert.deepStrictEqual([received[0]?.event, TailscaleEvent.Kind.literals], [batch, ['nodeCreated', 'nodeNeedsApproval', 'nodeApproved', 'nodeDeleted', 'nodeKeyExpiringInOneDay', 'nodeKeyExpired', 'policyUpdate', 'userRoleUpdated']]);
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
            const transport = yield* Layer.build(WebhookTest(endpoint));
            const offer = yield* webhook.queued({ name: 'outbox', maxAttempts: 3, retrySchedule: Schedule.spaced('1 minute') }, (queued) => webhook.deliver(url, queued)).pipe(Effect.provide(transport));
            yield* Effect.all([offer({ id: 'msg_repeated', event: { count: 1 } }), offer({ id: 'msg_repeated', event: { count: 2 } })]);
            yield* TestClock.adjust('1 minute');
            yield* Effect.provide(webhook.deliver(url, { id: 'msg_repeated', event: { count: 1 } }), transport);
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
        }, queues(crypto)),
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
        }, queues(crypto)),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { cases };
