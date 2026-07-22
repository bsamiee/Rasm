# [RUNTIME_CHANNEL]

Framed stream transport is the second half of the branch net plane: where `client` owns request/response egress, this page owns long-lived byte channels — socket duplex under a closed frame vocabulary and the server-sent event feed — all backpressured by construction and typed at the seam. Causal context crosses each channel as data: ingress surfaces the arriving carrier band verbatim, and the consumer extracts the transport's core dialect before the one continuation transformer, so a channel never orphans a producing span and never imports the telemetry plane. A socket is capability: construction rides `Socket.makeWebSocket` against the `Socket.WebSocketConstructor` Tag the runtime binding satisfies, so one framed transport definition serves every runtime lane, and the frame is a row swap under an unchanged schema seam. Its SSE feed owns the full `Sse` codec: `Sse.makeChannel` decodes the `data:`/`event:`/`id:`/`retry:` line protocol as package capability — the `retry:` directive is absorbed in-channel, the parser sleeping the hinted delay in place — the reattach cursor advances as a fold and stamps `last-event-id` on every re-dial, a cleanly completed response reconnects exactly like a faulted one, silence folds through the core degradation ladder to pick probe cadence, and `Sse.encoder` is the mirror the serving edge composes so both directions of the dialect have one codec owner. A raw socket listener, a hand `data:`-line parser, a reconnect that observes only transport faults, and `JSON.stringify` written to a wire are the named defects. Its module is `runtime/src/net/channel.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                            | [PUBLIC] |
| :-----: | :----------- | :-------------------------------------------------------------------------------- | :------- |
|  [01]   | `FRAME_ROWS` | the duplex frame vocabulary — `ndjson \| msgpack` rows fused with one schema seam | `Duplex` |
|  [02]   | `FEED_SEAM`  | the SSE session — codec, cursor fold, Retry-driven reconnect, silence ladder      | `Feed`   |
|  [03]   | `MQTT_SEAM`  | MQTT v5 scoped clients, carrier properties, typed refusal                         | `Mqtt`   |

## [02]-[FRAME_ROWS]

[FRAME_ROWS]:
- Owner: `Duplex` — the framed-socket owner. `Duplex.framed(socket, frame, protocol)` lifts a `Socket.Socket` to a typed message channel: `Socket.toChannelWith<E>()` turns the socket into a byte `Channel`, and the frame row's fused combinator — `Ndjson.duplexSchema` (newline-delimited, `NdjsonError`) or `MsgPack.duplexSchema` (length-delimited binary, `MsgPackError`) — owns chunk reassembly and the schema seam in one step, so both directions are decoded values and backpressure is channel-native.
- Law: the frame is a row swap under an unchanged schema seam — the `_frames` table keys each dialect to its fused combinator, dispatch is one keyed lookup, and `Duplex.Kind` derives from the table; moving a peer from ndjson to msgpack edits one argument and no consumer, and a new frame dialect is one row, zero arms.
- Law: the protocol pair is send/take symmetric evidence — `send` types the outbound seam, `take` the inbound seam, both usually one closed `Schema.Union` of tagged messages; an untyped frame crossing the channel is unspellable because the fused combinator is the only construction.
- Law: fault families arrive typed and stay separate — the frame's own error, `Socket.SocketError`, and `ParseError` each route on their own tag; none is re-wrapped.
- Law: causal context rides the protocol, never the frame — a duplex peer whose messages carry `traceparent`/`tracestate`/`baggage` declares those fields on its `take` schema, and the consumer extracts its admitted dialect through core `Carrier` before `Propagation.ingress` at the handling seam; the frame rows stay context-blind and this floor module composes no telemetry import.
- Boundary: socket construction is capability — `Socket.makeWebSocket(url)` demands the `WebSocketConstructor` Tag, satisfied by the runtime binding at the root; session lifetime, reconnect, and the pipeline geometry above the channel are the consumer's, composed from `Stream` law.
- Entry: `Duplex.framed(socket, frame, { send, take })`.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `MsgPack`), `effect` (`Channel`, `Chunk`, `Schema`).

```typescript signature
import { Sse } from '@effect/experimental';
import { type HttpClient, HttpClientRequest, MsgPack, Ndjson, Socket } from '@effect/platform';
import { type Channel, type Chunk, Context, Data, Duration, Effect, Layer, Option, type ParseResult, Record, Ref, Schema, type Scope, Stream, pipe } from 'effect';
import { connectAsync, type IPublishPacket, type MqttClient, type QoS } from 'mqtt';
import { Budget, Carrier, type FaultClass } from '@rasm/ts/core';
import { Propagation } from '../otel/emit.ts';
import { Client } from './client.ts';

const _frames = { msgpack: MsgPack.duplexSchema, ndjson: Ndjson.duplexSchema } as const;

declare namespace Duplex {
    type Kind = keyof typeof _frames;
    type Fault = MsgPack.MsgPackError | Ndjson.NdjsonError;
    type Protocol<Send, SendI, Take, TakeI> = {
        readonly send: Schema.Schema<Send, SendI>;
        readonly take: Schema.Schema<Take, TakeI>;
    };
}

const _framed = <Send, SendI, Take, TakeI>(
    socket: Socket.Socket,
    frame: Duplex.Kind,
    protocol: Duplex.Protocol<Send, SendI, Take, TakeI>,
): Channel.Channel<
    Chunk.Chunk<Take>,
    Chunk.Chunk<Send>,
    Duplex.Fault | ParseResult.ParseError | Socket.SocketError,
    ParseResult.ParseError,
    void,
    unknown
> =>
    Socket.toChannelWith<Duplex.Fault | ParseResult.ParseError>()(socket).pipe(
        _frames[frame]({ inputSchema: protocol.send, outputSchema: protocol.take }),
    );

const Duplex = { framed: _framed } as const;
```

## [03]-[FEED_SEAM]

[FEED_SEAM]:
- Owner: `Feed` — the server-sent-event ingress port every SSE consumer in the branch shares (`flag#GATE_SERVICE` is the standing consumer; the serving edge composes the same codec's `Sse.encoder` for the mirror side). `Feed.open(session)` yields the live event stream from one admitted `Feed.Session` carrier holding the origin, request headers, parser buffer bound, and clean-redial floor: the session dials the `feed` client lane (no total budget), admits status and `text/event-stream`, decodes the response body's bytes to text at the seam (`Stream.decodeText` — the channel consumes text lines), and pipes it through `Sse.makeChannel({ bufferSize })`, so authentication material, media admission, buffering, reconnect posture, and the line protocol remain one session fold and the consumer sees only decoded `Sse.Event` frames.
- Law: the `retry:` directive is package-absorbed pacing — `Sse.makeChannel` never emits `Sse.Retry` downstream: on a `retry:` frame the channel sleeps the hinted `duration` in place and resumes the same response, so server-driven pacing within a live connection is the codec's own behavior and no session cell shadows it.
- Law: reconnection covers every ending — the session is one `Stream.forever` over the dial, so a cleanly completed response re-dials after `session.redial` (a server that closes immediately cannot hot-loop), and a faulted response re-dials under `Budget.schedule("feed")` — the ledger owner's gate-modal compile, attempts, reset, jitter, and elapsed window intact, its `FaultClass.retryable` default gate passing because every retry-channel error is `FeedFault`, classed `unavailable` — with exhaustion minting `FeedFault` carrying the origin as evidence, the one fault consumers see.
- Law: the reattach cursor is a fold, not a cell convention — every `Sse.Event` carrying an `id` advances the cursor, and every re-dial — clean or faulted — stamps it as the `last-event-id` request header, so an outage backfills by event id; the cursor lives in the session's own `Ref`, invisible to consumers and surviving every re-dial.
- Law: silence is laddered, never polled — a long-lived consumer folds its observed quiet span through the core `Degrade.cadence` ladder to pick the rung's probe cadence; the ladder folds the span it is handed, the consumer owns the measurement, and no consumer indexes a degradation row to reconstruct cadence.
- Law: the feed is transport only — it emits decoded `Sse.Event` frames and owns no payload vocabulary; the consumer's own Schema decodes `event.data` and any admitted carrier at its seam, then continues through `Propagation.ingress`, never inside the feed.
- Boundary: the serving mirror — `Sse.encoder` framing an outbound event stream over an HTTP response — is the edge wave's mount; this page owns the codec direction law so the dialect has one owner.
- Entry: `yield* Feed` then `feed.open(origin)`; `Feed.live` is the shipped Layer over the client lane.
- Packages: `@effect/experimental` (`Sse`), `@effect/platform` (`HttpClientRequest`), `effect` (`Context`, `Data`, `Duration`, `Option`, `Ref`, `Stream`), `./client.ts` (`Client`), `@rasm/ts/core` (`Budget`).

```typescript signature
class FeedFault extends Data.TaggedError('FeedFault')<{
    readonly origin: string;
    readonly reason: 'transport' | 'media';
    readonly status: Option.Option<number>;
    readonly cursor: Option.Option<string>;
}> {
    get class(): FaultClass.Kind {
        return 'unavailable';
    }
}

class _Session extends Schema.Class<_Session>('Feed/Session')({
    origin: Schema.URLFromSelf,
    headers: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { default: () => ({}) }),
    buffer: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 64 }),
    redial: Schema.optionalWith(Schema.Duration, { default: () => Duration.millis(500) }),
}) {}

class Feed extends Context.Tag('runtime/Feed')<
    Feed,
    {
        readonly open: (session: URL | Feed.Session) => Stream.Stream<Sse.Event, FeedFault>;
    }
>() {
    static readonly live: Layer.Layer<Feed, never, HttpClient.HttpClient> = Layer.effect(
        Feed,
        Effect.map(Effect.context<HttpClient.HttpClient>(), (context) => ({
            open: (session) => Stream.provideContext(_session(session instanceof URL ? new _Session({ origin: session }) : session), context),
        })),
    );
    static readonly Session = _Session;
}

declare namespace Feed {
    type Cursor = Option.Option<string>;
    type Session = _Session;
}

const _reattached = (session: Feed.Session, cursor: Feed.Cursor): HttpClientRequest.HttpClientRequest =>
    pipe(
        HttpClientRequest.get(session.origin.href).pipe(
            HttpClientRequest.setHeaders(session.headers),
            HttpClientRequest.setHeader('accept', 'text/event-stream'),
        ),
        (base) =>
            Option.match(cursor, {
                onNone: () => base,
                onSome: (id) => base.pipe(HttpClientRequest.setHeader('last-event-id', id)),
            }),
    );

const _pulled = (
    session: Feed.Session,
    request: HttpClientRequest.HttpClientRequest,
    cursor: Feed.Cursor,
): Stream.Stream<Sse.Event, FeedFault, HttpClient.HttpClient> =>
    Stream.unwrapScoped(
        Effect.flatMap(Client.dial('feed', request), (response) =>
            response.headers['content-type']?.split(';', 1)[0]?.trim().toLowerCase() === 'text/event-stream'
                ? Effect.succeed(
                      response.stream.pipe(Stream.decodeText(), Stream.pipeThroughChannel(Sse.makeChannel({ bufferSize: session.buffer }))),
                  )
                : Effect.fail(new FeedFault({ origin: session.origin.href, reason: 'media', status: Option.some(response.status), cursor })),
        ),
    ).pipe(
        Stream.mapError((fault) =>
            fault instanceof FeedFault ? fault : new FeedFault({ origin: session.origin.href, reason: 'transport', status: Option.none(), cursor }),
        ),
    );

const _session = (session: Feed.Session): Stream.Stream<Sse.Event, FeedFault, HttpClient.HttpClient> =>
    Stream.unwrap(
        Effect.map(Ref.make<Feed.Cursor>(Option.none()), (cursor) =>
            Stream.unwrap(Effect.map(Ref.get(cursor), (held) => _pulled(session, _reattached(session, held), held))).pipe(
                Stream.tap((event) => Ref.update(cursor, (held) => Option.orElse(Option.fromNullable(event.id), () => held))),
                Stream.concat(Stream.drain(Stream.fromEffect(Effect.sleep(session.redial)))), // the clean-EOF floor paces the next dial
                Stream.forever, // every ending re-dials: the cursor Ref outlives each response, so reattach rides last-event-id
                Stream.retry(Budget.schedule('feed')),
            ),
        ),
    );
```

## [04]-[MQTT_SEAM]

[MQTT_SEAM]:

- Owner: `Mqtt` — the MQTT v5 channel. `Mqtt.Broker` carries origin, delivery grade, retain posture, and keepalive; `Mqtt.live(broker)` brackets one publisher client, while `open(topics)` brackets its own subscription client and listener. `consume(topics, handler)` is the admitted handling ingress. No client or emitter crosses an app boundary.
- Law: v5 `userProperties` is the carrier frame — publish injects `Propagation.current` through core `Carrier.inject("mqtt", ...)`, transport preserves the resulting string-or-string-array map, and `consume` extracts that exact row before the one `Propagation.ingress` transformer. `open` remains the ordered raw-frame lane for callers that compose continuation with a larger stream fold.
- Law: subscription admission is evidence — every `subscribeAsync` grant is inspected, and any `qos: 128` refusal fails the typed `grant` rail before a message stream escapes. Failed subscription or grant admission ends the minted client before the fault escapes; successful acquisition transfers that client to the stream scope. Message and lifecycle listeners share the stream scope; `close`, `error`, `disconnect`, and `offline` terminate the stream once, and release ends the client before detaching the complete listener row.
- Law: payload stays opaque — the channel exposes `Uint8Array`; the consuming owner decodes once through its own Schema. Retain and QoS are broker policy values, never payload conventions.
- Packages: `mqtt` (`connectAsync`, `MqttClient.subscribeAsync`, `publishAsync`, `endAsync`, `IPublishPacket`); `effect` (`Context`, `Effect`, `Layer`, `Stream`).

```typescript signature
class MqttFault extends Data.TaggedError('MqttFault')<{
    readonly origin: string;
    readonly reason: 'dial' | 'grant' | 'publish';
}> {
    get class(): FaultClass.Kind {
        return this.reason === 'grant' ? 'malformed' : 'unavailable';
    }
}

class _MqttBroker extends Schema.Class<_MqttBroker>('Mqtt/Broker')({
    origin: Schema.URLFromSelf,
    qos: Schema.optionalWith(Schema.Literal(0, 1, 2), { default: () => 1 as QoS }),
    retain: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    keepalive: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 60 }),
}) {}

declare namespace Mqtt {
    type Band = Carrier.Frame['mqtt'];
    type Broker = _MqttBroker;
    type Frame = { readonly topic: string; readonly body: Uint8Array; readonly band: Band };
}

const _mqttBand = (packet: IPublishPacket): Mqtt.Band => packet.properties?.userProperties ?? {};

const _mqttConnect = (broker: Mqtt.Broker): Effect.Effect<MqttClient, MqttFault> =>
    Effect.tryPromise({
        try: () => connectAsync(broker.origin.href, { protocolVersion: 5, keepalive: broker.keepalive }),
        catch: () => new MqttFault({ origin: broker.origin.href, reason: 'dial' }),
    });

const _MQTT_TERMINALS = ['close', 'error', 'disconnect', 'offline'] as const;

const _mqttOpen = (broker: Mqtt.Broker, topics: string | ReadonlyArray<string>): Stream.Stream<Mqtt.Frame, MqttFault> =>
    Stream.asyncScoped<Mqtt.Frame, MqttFault>((emit) =>
        Effect.acquireRelease(
            Effect.gen(function* () {
                const client = yield* _mqttConnect(broker);
                return yield* Effect.gen(function* () {
                    const grants = yield* Effect.tryPromise({
                        try: () => client.subscribeAsync(typeof topics === 'string' ? [topics] : [...topics], { qos: broker.qos }),
                        catch: () => new MqttFault({ origin: broker.origin.href, reason: 'dial' }),
                    });
                    if (grants.some((grant) => grant.qos === 128)) {
                        return yield* Effect.fail(new MqttFault({ origin: broker.origin.href, reason: 'grant' }));
                    }
                    let terminated = false;
                    const message = (topic: string, body: Uint8Array, packet: IPublishPacket) => {
                        if (terminated) return;
                        void emit.single({ topic, body: new Uint8Array(body), band: _mqttBand(packet) });
                    };
                    const terminate = () => {
                        if (terminated) return;
                        terminated = true;
                        void emit.fail(new MqttFault({ origin: broker.origin.href, reason: 'dial' }));
                    };
                    client.on('message', message);
                    for (const event of _MQTT_TERMINALS) client.on(event, terminate);
                    return { client, message, terminate };
                }).pipe(
                    Effect.tapError(() =>
                        Effect.orDie(
                            Effect.tryPromise({
                                try: () => client.endAsync(),
                                catch: () => new MqttFault({ origin: broker.origin.href, reason: 'dial' }),
                            }),
                        ),
                    ),
                );
            }),
            (live) =>
                Effect.orDie(
                    Effect.tryPromise({
                        try: async () => {
                            try {
                                await live.client.endAsync();
                            } finally {
                                live.client.off('message', live.message);
                                for (const event of _MQTT_TERMINALS) live.client.off(event, live.terminate);
                            }
                        },
                        catch: () => new MqttFault({ origin: broker.origin.href, reason: 'dial' }),
                    }),
                ),
        ),
    );

class Mqtt extends Context.Tag('runtime/Mqtt')<
    Mqtt,
    {
        readonly consume: (
            topics: string | ReadonlyArray<string>,
            handler: (frame: Mqtt.Frame) => Effect.Effect<void, MqttFault>,
        ) => Effect.Effect<void, MqttFault>;
        readonly open: (topics: string | ReadonlyArray<string>) => Stream.Stream<Mqtt.Frame, MqttFault>;
        readonly publish: (topic: string, body: Uint8Array, band?: Mqtt.Band) => Effect.Effect<void, MqttFault>;
    }
>() {
    static readonly Broker = _MqttBroker;
    static readonly live = (broker: Mqtt.Broker): Layer.Layer<Mqtt, MqttFault> =>
        Layer.scoped(
            Mqtt,
            Effect.map(
                Effect.acquireRelease(_mqttConnect(broker), (client) => Effect.promise(() => client.endAsync())),
                (publisher) => ({
                    consume: (topics, handler) =>
                        Stream.runForEach(_mqttOpen(broker, topics), (frame) =>
                            Propagation.ingress(handler(frame), Carrier.extract('mqtt', frame.band)),
                        ),
                    open: (topics) => _mqttOpen(broker, topics),
                    publish: (topic, body, band = {}) =>
                        Effect.flatMap(Propagation.current, (context) =>
                            Effect.tryPromise({
                                try: () => publisher.publishAsync(topic, Buffer.from(body), {
                                    qos: broker.qos,
                                    retain: broker.retain,
                                    properties: { userProperties: Carrier.inject('mqtt', context, band) },
                                }),
                                catch: () => new MqttFault({ origin: broker.origin.href, reason: 'publish' }),
                            }).pipe(Effect.asVoid),
                        ),
                }),
            ),
        );
}
```

```typescript signature
// --- [EXPORTS] --------------------------------------------------------------------------

export { Duplex, Feed, FeedFault, Mqtt, MqttFault };
```

## [05]-[RESEARCH]

(none)
