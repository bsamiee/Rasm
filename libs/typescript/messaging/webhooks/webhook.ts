import { Array, DateTime, Duration, Effect, Option, pipe, type Redacted, Schema } from 'effect';
import { Digest } from 'effect/crypto';
import { type Headers, HttpClient, HttpClientRequest, HttpClientResponse, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { PersistedQueue } from 'effect/persistence';
import { Scheme, Stamp, WebhookError } from '#/scheme';

// --- [TYPES] ---------------------------------------------------------------------------

interface Delivery<A> {
    readonly id: string;
    readonly event: A;
}
type Webhook<S extends Schema.Top> = Effect.Success<ReturnType<typeof Webhook<S>>>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const [encoder, decoder] = [new TextEncoder(), new TextDecoder()];

const Webhook = Effect.fn('Webhook')(function* <S extends Schema.Top>({ scheme, payload, ...contract }: { readonly scheme: Scheme; readonly payload: S; readonly tolerance: Duration.Input; readonly secrets: Array.NonEmptyReadonlyArray<Redacted.Redacted> }) {
    const [keys, digest] = yield* Effect.all([Effect.forEach(contract.secrets, (secret) => Scheme.key(scheme, secret)), Digest.Digest]);
    const [tolerance, text] = [Duration.fromInputUnsafe(contract.tolerance), Schema.fromJsonString(Schema.toCodecJson(payload))];
    const refused = (reason: WebhookError['reason']) => WebhookError.of(reason, scheme.signature)();
    const verify = Effect.fn('verify')(function* (headers: Headers.Headers, body: Uint8Array) {
        const [{ id, timestamp, content, signatures }, now] = yield* Effect.all([Scheme.read(scheme, headers, body), DateTime.now]);
        const matches = yield* Effect.forEach(Array.cartesian(keys, signatures), ([key, { label, bytes }]) => (key.label === label ? key.verify(bytes, content) : Effect.succeed(false)));
        yield* Effect.filterOrFail(Effect.succeed(matches), Array.contains(true), () => refused({ _tag: 'Mismatch' }));
        yield* pipe(
            timestamp,
            Option.map((signed) => Duration.abs(DateTime.distance(now, signed))),
            Option.filter(Duration.isGreaterThan(tolerance)),
            Option.map((skew) => Effect.fail(refused({ _tag: 'Stale', skew }))),
            Effect.transposeOption,
        );
        return yield* Effect.all({ id: Option.match(id, { onSome: Effect.succeed, onNone: () => digest.digestHex(Digest.sha256, body) }), event: Schema.decodeEffect(text)(decoder.decode(body)) });
    });
    const sign = Effect.fnUntraced(function* (id: string, body: Uint8Array) {
        const values = { id, t: yield* Effect.orDie(Effect.flatMap(DateTime.now, Schema.encodeEffect(Stamp))) };
        const content = scheme.content(values, body);
        return yield* scheme.write(values, yield* Effect.forEach(keys, (key) => key.sign(content)));
    });
    const request = Effect.fnUntraced(function* (url: string | URL, { id, event }: Delivery<S['Type']>) {
        const body = encoder.encode(yield* Schema.encodeEffect(text)(event));
        return HttpClientRequest.post(url, { headers: yield* sign(id, body) }).pipe(HttpClientRequest.bodyUint8Array(body, 'application/json'));
    });
    return {
        scheme,
        payload,
        verify,
        receive: Effect.fnUntraced(function* <E, R>(accept: (delivery: Delivery<S['Type']>) => Effect.Effect<unknown, E, R>) {
            const incoming = yield* HttpServerRequest.HttpServerRequest;
            yield* accept(yield* verify(incoming.headers, new Uint8Array(yield* incoming.arrayBuffer)));
            return HttpServerResponse.empty();
        }),
        sign,
        request,
        deliver: (url: string | URL, delivery: Delivery<S['Type']>) => request(url, delivery).pipe(Effect.flatMap(HttpClient.execute), Effect.flatMap(HttpClientResponse.filterStatusOk)),
        queued: Effect.fnUntraced(function* <E, R>(options: Omit<Parameters<typeof PersistedQueue.make>[0], 'schema'>, work: (delivery: Delivery<S['Type']>, attempts: number) => Effect.Effect<unknown, E, R>) {
            const queue = yield* PersistedQueue.make({ ...options, schema: payload });
            yield* queue.take((event, { id, attempts }) => work({ id, event }, attempts)).pipe(Effect.ignoreCause({ log: 'Warn', message: `Webhook queue ${options.name} work failed` }), Effect.forever, Effect.forkScoped);
            return ({ id, event }: Delivery<S['Type']>) => queue.offer(event, { id });
        }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Delivery, Webhook };
