import { assert, it } from '@effect/vitest';
import { Scheme, Webhook, WebhookError } from '@rasm/webhooks';
import { TailscaleEvent } from '@rasm/webhooks/tailscale';
import { WebhookTest } from '@rasm/webhooks/testing';
import { Array, Bytes, DateTime, Duration, Effect, Exit, Function, Layer, Option, Queue, Record, Redacted, Ref, Schedule, Schema } from 'effect';
import type { Digest, Mac, Signature } from 'effect/crypto';
import { Headers, HttpClient, HttpClientRequest, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { PersistedQueue } from 'effect/persistence';
import { TestClock } from 'effect/testing';

// --- [CONSTANTS] -----------------------------------------------------------------------

const standardwebhooks = await import('standardwebhooks');
const svix = await import('svix');
const { default: Stripe } = await import('stripe');
const octokit = await import('@octokit/webhooks-methods');
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
const hub = { ...standard, scheme: Scheme.github, late: 'verified', crowded: (signed: string) => `sha256=00ff,${signed},sha256=abcd` } as const;
const vectors = {
    standard: { ...standard, bytes: encoder.encode(standard.body), signed: 'v1,g0hM9SsE+OTPJTGt/tmIKtSyZlE3uFJELVlNIOLJ1OE=' },
    binary: { ...standard, bytes: Uint8Array.from({ length: 256 }, (_, index) => index), signed: 'v1,sEcZ042G8Crqgxbybb3y0eDxQR3TcfYsk11I1XULhSI=', accepted: 'SchemaError' },
    rust: { ...standard, secret: standard.foreign, foreign: standard.secret, id: 'msg_27UH4WbU6Z5A5EzD8u03UvzRbpk', at: 1_649_367_553, bytes: encoder.encode('{"email":"test@example.com","username":"test_user"}'), signed: 'v1,tZ1I4/hDygAJgO5TYxiSd6Sd0kDW6hPenDe+bTa3Kkw=' },
    ed25519: { ...standard, secret: pair.private, foreign: pair.foreign, bytes: encoder.encode(standard.body), signed: 'v1a,fldxM4gAKugP6nnt1hdz3sgGfZ6d99nzrMFnZOELIxbzEHoVmAb2ADpkJK7zgPePmPsle0zV9jSeGlHFG2NVAw==' },
    svix: { ...standard, scheme: Scheme.svix, secret: 'whsec_plJ3nmyCDGBKInavdOK15jsl', id: 'msg_loFOjxBNrRLzqYUf', at: 1_731_705_121, bytes: encoder.encode('{"event_type":"ping","data":{"success":true}}'), signed: 'v1,rAvfW3dJ/X/qxhsaXPOyyCGmRKsaKWcsNccKXlIktD0=' },
    github: { ...hub, secret: "It's a Secret to Everybody", bytes: encoder.encode('Hello, World!'), signed: 'sha256=757107ea0eb2509fc211221cce984b8a37570b6d7586c22c46f4379c8b043e17', accepted: 'SchemaError', late: 'SchemaError' },
    octokit: { ...hub, secret: 'mysecret', foreign: 'foo', bytes: encoder.encode('{"foo":"bar"}'), signed: 'sha256=4864d2759938a15468b5df9ade20bf161da9b4f737ea61794142f3484236bda3' },
    escapes: {
        ...hub,
        secret: 'development',
        bytes: encoder.encode(String.raw`{"foo":"Foo\n\u001B[34mbar: ♥♥♥♥♥♥♥♥\nthis-is-lost\u001B[0m\u001B[2K"}`),
        signed: 'sha256=afecc3caa27548bb90d51a50384cb2868b9a3327b4ad6a01c9bd4ed0f8b0b12c',
    },
    escaped: { ...hub, secret: 'development', bytes: encoder.encode(String.raw`{"foo":"\\u001b"}`), signed: 'sha256=6f8326efbacfbd04e870cea25b5652e635be8c9807f2fd5348ef60753c9e96ed' },
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
const references = {
    standardwebhooks: {
        scheme: Scheme.standard,
        secret: standard.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers, now: DateTime.Utc) {
            const reference = new standardwebhooks.Webhook(standard.secret);
            const accepted = yield* Effect.as(
                Effect.try(() => reference.verify(standard.body, { ...ours })),
                true,
            );
            return { accepted, theirs: { 'webhook-id': standard.id, 'webhook-timestamp': `${DateTime.toEpochSeconds(now)}`, 'webhook-signature': reference.sign(standard.id, DateTime.toDate(now), standard.body) } };
        }),
    },
    svix: {
        scheme: Scheme.svix,
        secret: vectors.svix.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers, now: DateTime.Utc) {
            const reference = new svix.Webhook(vectors.svix.secret);
            const accepted = yield* Effect.as(
                Effect.try(() => reference.verify(standard.body, { ...ours })),
                true,
            );
            return { accepted, theirs: { 'svix-id': standard.id, 'svix-timestamp': `${DateTime.toEpochSeconds(now)}`, 'svix-signature': reference.sign(standard.id, DateTime.toDate(now), standard.body) } };
        }),
    },
    stripe: {
        scheme: Scheme.stripe,
        secret: vectors.stripe.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers, now: DateTime.Utc) {
            const header = yield* Effect.fromOption(Headers.get(ours, Scheme.stripe.signature));
            const accepted = yield* Effect.as(
                Effect.tryPromise(() => Stripe.webhooks.constructEventAsync(standard.body, header, vectors.stripe.secret)),
                true,
            );
            return { accepted, theirs: { [Scheme.stripe.signature]: yield* Effect.promise(() => Stripe.webhooks.generateTestHeaderStringAsync({ payload: standard.body, secret: vectors.stripe.secret, timestamp: DateTime.toEpochSeconds(now) })) } };
        }),
    },
    '@octokit/webhooks-methods': {
        scheme: Scheme.github,
        secret: vectors.octokit.secret,
        exchanged: Effect.fnUntraced(function* (ours: Headers.Headers) {
            const header = yield* Effect.fromOption(Headers.get(ours, Scheme.github.signature));
            const [accepted, signed] = yield* Effect.all([Effect.promise(() => octokit.verify(vectors.octokit.secret, standard.body, header)), Effect.promise(() => octokit.sign(vectors.octokit.secret, standard.body))]);
            return { accepted, theirs: { [Scheme.github.signature]: signed } };
        }),
    },
};

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
    it.effect.each([
        { label: 'no id header', change: Headers.remove('webhook-id'), accepted: false },
        { label: 'no timestamp header', change: Headers.remove('webhook-timestamp'), accepted: false },
        { label: 'a timestamp that is no number', change: Headers.set('webhook-timestamp', 'hello'), accepted: false },
        { label: 'a signature entry holding no value', change: Headers.set('webhook-signature', 'v1,'), accepted: false },
        { label: 'malformed signature entries before a valid one', change: Headers.set('webhook-signature', `v1 v1,not-base64! ${vectors.standard.signed}`), accepted: true },
    ])(
        'standard scheme: a request with $label verifies: $accepted, each refusal a WebhookError',
        Effect.fnUntraced(function* ({ change, accepted }) {
            const { scheme, secret, id, at, bytes } = vectors.standard;
            const webhook = yield* made(scheme, secret);
            yield* TestClock.setTime(Duration.toMillis(Duration.seconds(at)));
            const verified = yield* outcome(webhook.verify(change(yield* webhook.sign(id, bytes)), bytes));
            assert.include(accepted ? ['verified'] : Object.keys(WebhookError.fields.reason.cases), verified);
        }, Effect.provide(crypto)),
    );
    it.effect(
        'standard scheme: refuses a secret holding no key, verifies with a secret written without its whsec_ prefix, and signs the bytes of a view alone',
        Effect.fnUntraced(function* () {
            const { scheme, secret, id, at, bytes } = vectors.binary;
            const [empty, webhook, bare] = yield* Effect.all([Effect.exit(made(scheme, 'whsec_')), made(scheme, secret), Effect.exit(made(scheme, secret.slice('whsec_'.length)))]);
            yield* TestClock.setTime(Duration.toMillis(Duration.seconds(at)));
            const margin = encoder.encode('--');
            const headers = yield* webhook.sign(id, Bytes.concat([margin, bytes, margin]).subarray(margin.length, margin.length + bytes.length));
            const unprefixed = yield* Exit.match(bare, { onFailure: () => Effect.succeed('refused at construction'), onSuccess: (reader) => outcome(reader.verify(headers, bytes)) });
            assert.deepStrictEqual([Exit.isFailure(empty), Headers.get(headers, scheme.signature), unprefixed], [true, Option.some(vectors.binary.signed), vectors.binary.accepted]);
        }, Effect.provide(crypto)),
    );
    it.effect(
        'standard scheme: signs a UTF-8 byte order mark like any other byte, and decodes the JSON after it',
        Effect.fnUntraced(function* () {
            const { scheme, secret, id, bytes } = vectors.standard;
            const webhook = yield* made(scheme, secret);
            const marked = Bytes.concat([encoder.encode('\uFEFF'), bytes]);
            const headers = yield* webhook.sign(id, marked);
            const [event, unmarked] = yield* Effect.all([Effect.match(webhook.verify(headers, marked), { onSuccess: (delivery) => delivery.event, onFailure: ({ _tag }) => _tag }), outcome(webhook.verify(headers, bytes))]);
            assert.deepStrictEqual([event, unmarked], [{ test: 2_432_232_314 }, 'Mismatch']);
        }, Effect.provide(crypto)),
    );
    it.live.each(Record.keys(references))(
        'exchanges signatures with the %s reference library',
        Effect.fnUntraced(function* (name) {
            const { scheme, secret, exchanged } = references[name];
            const [webhook, now, bytes] = [yield* made(scheme, secret), yield* DateTime.now, encoder.encode(standard.body)];
            const { accepted, theirs } = yield* exchanged(yield* webhook.sign(standard.id, bytes), now);
            const { event } = yield* webhook.verify(Headers.fromInput(theirs), bytes);
            assert.deepStrictEqual([accepted, event], [true, { test: 2_432_232_314 }]);
        }, Effect.provide(crypto)),
    );
    it.effect.each([
        { status: 200, delivered: true },
        { status: 204, delivered: true },
        { status: 299, delivered: true },
        { status: 301, delivered: false },
        { status: 404, delivered: false },
        { status: 410, delivered: false },
        { status: 429, delivered: false },
        { status: 500, delivered: false },
        { status: 502, delivered: false },
        { status: 503, delivered: false },
        { status: 504, delivered: false },
    ])(
        'counts a delivery answered $status as delivered: $delivered',
        Effect.fnUntraced(function* ({ status, delivered }) {
            const webhook = yield* made(Scheme.standard, standard.secret);
            const answered = WebhookTest(Effect.succeed(HttpServerResponse.empty({ status, headers: { location: url, 'retry-after': '120' } })));
            const sent = yield* Effect.exit(webhook.deliver(url, { id: standard.id, event: { test: 1 } })).pipe(Effect.provide(answered));
            assert.strictEqual(Exit.isSuccess(sent), delivered);
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
