# [RUNTIME_CHANNEL]

Framed stream transport is the second half of the branch net plane: where `client` owns request/response egress, this page owns long-lived byte channels — socket duplex under a closed frame vocabulary and the server-sent event feed — all backpressured by construction and typed at the seam. Causal context crosses each channel as data: ingress surfaces the arriving carrier band verbatim, and the consumer extracts the transport's core dialect before the one continuation transformer, so a channel never orphans a producing span and never imports the telemetry plane. A socket is capability: construction rides `Socket.makeWebSocket` against the `Socket.WebSocketConstructor` Tag the runtime binding satisfies, so one framed transport definition serves every runtime lane, and the frame is a row swap under an unchanged schema seam. Its SSE feed owns the full `Sse` codec: `Sse.makeChannel` decodes the `data:`/`event:`/`id:`/`retry:` line protocol as package capability — the `retry:` directive is absorbed in-channel, the parser sleeping the hinted delay in place — the reattach cursor advances as a fold and stamps `last-event-id` on every re-dial, a cleanly completed response reconnects exactly like a faulted one, silence folds through the core degradation ladder to pick probe cadence, and `Sse.encoder` is the mirror the serving edge composes so both directions of the dialect have one codec owner. A raw socket listener, a hand `data:`-line parser, a reconnect that observes only transport faults, and `JSON.stringify` written to a wire are the named defects. Its module is `runtime/src/net/channel.ts`.

## [01]-[INDEX]

- [02]-[FRAME_ROWS]: the duplex frame vocabulary — `ndjson | msgpack` rows fused with one schema seam; `Duplex`.
- [03]-[FEED_SEAM]: the SSE session — codec, cursor fold, Retry-driven reconnect, silence ladder; `Feed`.
- [04]-[MQTT_SEAM]: MQTT v5 scoped clients, per-subscription and per-post policy rows, the consumption descriptor; `Mqtt`.

## [02]-[FRAME_ROWS]

[FRAME_ROWS]:
- Owner: `Duplex.framed` composes `Socket.toChannel` with the selected fused `Ndjson.duplexSchema` or `MsgPack.duplexSchema` row.
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
import { CloudEvent, CONSTANTS, MQTT, type MQTTMessage } from 'cloudevents';
import {
    connectAsync,
    type IClientPublishOptions,
    type IDisconnectPacket,
    type IPublishPacket,
    type ISubscriptionMap,
    type MqttClient,
    type QoS,
} from 'mqtt';
import { Buffer } from 'node:buffer';
import { Carrier, Fault } from '@rasm/ts/core';
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
    Socket.toChannel<Duplex.Fault | ParseResult.ParseError>(socket).pipe(
        _frames[frame]({ inputSchema: protocol.send, outputSchema: protocol.take }),
    );

const Duplex = { framed: _framed } as const;
```

## [03]-[FEED_SEAM]

[FEED_SEAM]:
- Owner: `Feed` — the server-sent-event ingress port every SSE consumer in the branch shares (`proc/flag#GATE_SERVICE` is the standing consumer; the serving edge composes the same codec's `Sse.encoder` for the mirror side). `Feed.open(session)` yields the live event stream from one admitted `Feed.Session` carrier holding the origin, request headers, parser buffer bound, and clean-redial floor: the session dials the `feed` client lane (no total budget), admits status and `text/event-stream`, decodes the response body's bytes to text at the seam (`Stream.decodeText` — the channel consumes text lines), and pipes it through `Sse.makeChannel({ bufferSize })`, so authentication material, media admission, buffering, reconnect posture, and the line protocol remain one session fold and the consumer sees only decoded `Sse.Event` frames.
- Law: the `retry:` directive is package-absorbed pacing — `Sse.makeChannel` never emits `Sse.Retry` downstream: on a `retry:` frame the channel sleeps the hinted `duration` in place and resumes the same response, so server-driven pacing within a live connection is the codec's own behavior and no session cell shadows it.
- Law: `Stream.retry(Fault.Budget.schedule("feed"))` paces every faulted response redial.
- Law: Clean EOF waits `session.redial`; the cursor `Ref` survives both redial paths.
- Law: the reattach cursor is a fold, not a cell convention — every `Sse.Event` carrying an `id` advances the cursor, and every re-dial — clean or faulted — stamps it as the `last-event-id` request header, so an outage backfills by event id; the cursor lives in the session's own `Ref`, invisible to consumers and surviving every re-dial.
- Law: Long-lived consumers project observed silence through `Fault.Degrade.cadence`.
- Law: the feed is transport only — it emits decoded `Sse.Event` frames and owns no payload vocabulary; the consumer's own Schema decodes `event.data` and any admitted carrier at its seam, then continues through `Propagation.ingress`, never inside the feed.
- Boundary: the serving mirror — `Sse.encoder` framing an outbound event stream over an HTTP response — is the edge wave's mount; this page owns the codec direction law so the dialect has one owner.
- Entry: `yield* Feed` then `feed.open(origin)`; `Feed.live` is the shipped Layer over the client lane.
- Packages: `@effect/experimental`, `@effect/platform`, `effect`, `./client.ts`, and `@rasm/ts/core` (`Fault.Budget`).

```typescript signature
const _feedFamily = Fault.Class.family(['transport', 'media'] as const, {
    transport: { class: 'unavailable' },
    media: { class: 'unavailable' },
});

class FeedFault extends Data.TaggedError('FeedFault')<{
    readonly origin: string;
    readonly reason: (typeof _feedFamily.reasons)[number];
    readonly status: Option.Option<number>;
    readonly cursor: Option.Option<string>;
}> {
    get class(): Fault.Class.Kind {
        return _feedFamily.classOf(this.reason);
    }
    override get message(): string {
        return `<feed:${this.reason}> ${this.origin}`;
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
                Stream.retry(Fault.Budget.schedule('feed')),
            ),
        ),
    );
```

## [04]-[MQTT_SEAM]

[MQTT_SEAM]:
- Owner: `Mqtt` — the MQTT v5 channel. `Mqtt.Broker` carries origin, default delivery grade, default retain posture, and keepalive; `Mqtt.live(broker)` brackets one publisher client, while `open(topics)` brackets its own subscription client and listener. `consume(topics, handler)` is the admitted handling ingress. No client or emitter crosses an app boundary.
- Law: MQTT v5 User Properties carry the `Carrier` frame on publish and consume.
- Law: `CloudEvent` bodies select `MQTT.binary`; `Mqtt.event` applies `MQTT.toEvent` and strict validation.
- Law: `open` preserves the ordered raw-frame lane.
- Law: a subscription is a POLICY ROW, never a filter under one broker grade — `Mqtt.Topic` carries the per-subscription v5 axes the protocol decides (grade, no-local, retain-as-published, retain-handling) and `_mqttSubscription` folds every selector modality into one `ISubscriptionMap`; a bare filter still admits and takes the broker row's grade, so `local` is expressible per topic and one client publishing and consuming one filter no longer re-reads its own posts.
- Law: a post is a POLICY ROW on the same law — v5 decides grade, retain, message expiry, and the response/correlation pair PER PUBLISH PACKET, so `Mqtt.Post` carries them and `_mqttPublish` folds the row against the broker defaults into one `IClientPublishOptions`; a bare topic string still admits, and `dup` stays foreclosed because the client raises it on its own redelivery and a caller setting it forges a replay marker.
- Law: `_MQTT_GRADES` states each delivery grade's forfeit — v5 carries no broker-side dedup at any grade, so QoS 1 duplicates on redelivery and only QoS 2 removes them at its four-packet cost; a caller reads the row rather than inferring a guarantee from a number.
- Law: `_MQTT_ROW` is the seam's one consumption descriptor and every coordinate reads off it — selection (`fits`, `admit`, `tenancy`, `lifetime`), guarantee (`deliver`, `order`, `settle`), recovery (`replay`, `bound`, `refuse`), and the residual `degrade` no column expresses; a caller comparing this seam against `pubsub#PORT_SHAPE` reads the same column names, and a coordinate restated as prose beside the row forks it.
- Law: `serves` closes the member roster this engine answers, so a caller wanting replay, a positional cursor, or a consumer census reads `pubsub#PORT_SHAPE` rather than bending a topic into one.
- Law: subscription admission is evidence — every `subscribeAsync` grant is inspected, any `qos: 128` refusal fails the typed `grant` rail before a message stream escapes, and the refusal NAMES the filters the broker rejected rather than reporting that some filter failed.
- Law: terminal events carry unequal evidence and `_MQTT_TERMINALS` keeps it — only `error` holds a cause and only `disconnect` holds a v5 reason code, so one nullary handler across all four discards the sole diagnosis the seam receives; `offline` names a client still retrying beneath an ended stream, never a dead transport. Failed subscription or grant admission ends the minted client before the fault escapes; successful acquisition transfers that client to the stream scope. Message and lifecycle listeners share the stream scope; `close`, `error`, `disconnect`, and `offline` terminate the stream once, and release ends the client before detaching the complete listener row.
- Law: Raw frames keep opaque bytes; CloudEvent callers cross only through the MQTT binding projection.
- Packages: `mqtt`, `cloudevents`, `effect`, `node:buffer`, `@rasm/ts/core`, and `../otel/emit.ts`.

```typescript signature
const _mqttFamily = Fault.Class.family(['dial', 'grant', 'event', 'publish'] as const, {
    dial: { class: 'unavailable' },
    grant: { class: 'malformed' },
    event: { class: 'malformed' },
    publish: { class: 'unavailable' },
});

class MqttFault extends Data.TaggedError('MqttFault')<{
    readonly origin: string;
    readonly reason: (typeof _mqttFamily.reasons)[number];
    // `reason` bands the fault and `detail` proves it: four bands cover fourteen mint sites, so without the operand
    // that decided one, a refused grant, a server disconnect, and a binding rejection all read as one string.
    readonly detail: string;
}> {
    get class(): Fault.Class.Kind {
        return _mqttFamily.classOf(this.reason);
    }
    override get message(): string {
        return `<mqtt:${this.reason}> ${this.origin}: ${this.detail}`;
    }
}

// MQTT v5 decides delivery grade, no-local, retain-as-published, and retain-handling PER SUBSCRIPTION, so a topic is a
// policy row and never a bare filter under one broker-wide grade. `local` is structural rather than cosmetic: one
// client publishing and consuming one filter re-reads its own posts without it, and no broker-level knob says so per
// topic. Bare filters still admit and take the broker row's grade, so the simple selector costs a caller nothing.
class _MqttTopic extends Schema.Class<_MqttTopic>('Mqtt/Topic')({
    filter: Schema.NonEmptyString,
    qos: Schema.optionalWith(Schema.Literal(0, 1, 2), { as: 'Option' }),
    local: Schema.optionalWith(Schema.Boolean, { default: () => true }),
    asPublished: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    retained: Schema.optionalWith(Schema.Literal(0, 1, 2), { default: () => 0 }),
}) {}

// Posts carry the v5 axes the protocol decides per PUBLISH packet, exactly as `_MqttTopic` carries the per-subscription
// four. Bare topic strings still admit and take the broker row's grade and retain, so the simple call costs nothing.
// `dup` stays off this row: the client raises that flag on its OWN redelivery, and a caller setting it forges a replay.
class _MqttPost extends Schema.Class<_MqttPost>('Mqtt/Post')({
    topic: Schema.NonEmptyString,
    qos: Schema.optionalWith(Schema.Literal(0, 1, 2), { as: 'Option' }),
    retain: Schema.optionalWith(Schema.Boolean, { as: 'Option' }),
    expiry: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { as: 'Option' }), // seconds; the broker drops an undeliverable message at its own edge
    respond: Schema.optionalWith(Schema.NonEmptyString, { as: 'Option' }),
    correlate: Schema.optionalWith(Schema.Uint8ArrayFromSelf, { as: 'Option' }),
}) {}

// One consumption-descriptor row over this seam's own column set: selection, guarantee, recovery, and the residual
// forfeit. Cells carry MQTT's own vocabulary, so a reader crossing to `pubsub#PORT_SHAPE` compares column against
// column rather than prose against prose, and `serves` states the member roster instead of a capability discovered by
// fault. Every coordinate below is v5's; a `V311` broker silently drops half of them, which the `degrade` cell owns.
const _MQTT_ROW = {
    fits: '<constrained-sensor-gateway-or-edge-peer:long-lived-session,small-frames,carrier-frame,per-subscription-policy>',
    admit: 'publish',
    tenancy: '<topic-filter-scope>',
    // `Mqtt.Post.expiry` makes this package the owner: without that cell the broker alone decided the bound.
    lifetime: { until: '<retained-until-a-publisher-overwrites;live-until-messageExpiryInterval-elapses>', owner: 'package' },
    serves: { consume: true, event: true, open: true, publish: true },
    deliver: '<qos-0|1|2-per-subscription-and-per-post,no-broker-dedup-at-any-grade;retry-owner:the-client-session>',
    order: '<per-topic;the-topic-string-IS-the-key-member,so-per-entity-order-costs-one-topic-per-entity>',
    settle: '<publishAsync-resolves-on-PUBACK-at-qos-1-and-PUBCOMP-at-qos-2,on-write-at-qos-0>',
    replay: '<none:a-re-drive-resumes-nowhere;cleanStart-false-under-a-session-expiry-restores-in-flight-qos-1-state-alone>',
    bound: '<none-published:the-caller-bounds-its-own-in-flight-work>',
    refuse: '<value+event:grants-carry-qos-128,publishAsync-rejects,and-close/error/disconnect/offline-carry-the-rest>',
    degrade: '<no-partition-key,no-origin-coordinate,no-flow-control-member;every-v5-field-drops-silently-under-V311-while-the-publish-still-succeeds>',
} as const satisfies Mqtt.Row;

// Grades forfeit different things and this row states which — v5 carries no broker-side dedup at any grade, so QoS 1
// duplicates on redelivery and only the QoS 2 four-packet handshake removes them at a latency the caller pays for.
const _MQTT_GRADES = {
    0: { guarantee: 'at-most-once', degrade: '<no-ack,no-redelivery,loss-on-disconnect>' },
    1: { guarantee: 'at-least-once', degrade: '<duplicates-on-redelivery,no-broker-dedup>' },
    2: { guarantee: 'exactly-once', degrade: '<four-packet-handshake-latency>' },
} as const satisfies Record<QoS, { readonly guarantee: string; readonly degrade: string }>;

class _MqttBroker extends Schema.Class<_MqttBroker>('Mqtt/Broker')({
    origin: Schema.URLFromSelf,
    qos: Schema.optionalWith(Schema.Literal(0, 1, 2), { default: () => 1 as QoS }),
    retain: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    keepalive: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 60 }),
}) {}

declare namespace Mqtt {
    type PublishBand = NonNullable<NonNullable<IClientPublishOptions['properties']>['userProperties']>;
    type PacketBand = NonNullable<NonNullable<IPublishPacket['properties']>['userProperties']>;
    type Band = Readonly<PublishBand & PacketBand>;
    type Broker = _MqttBroker;
    type Topic = _MqttTopic;
    type Post = _MqttPost;
    type Member = keyof Row['serves'];
    type Row = {
        readonly fits: string;
        readonly admit: 'publish';
        readonly tenancy: string;
        readonly lifetime: { readonly until: string; readonly owner: 'package' | 'host' | 'deploy' };
        readonly serves: { readonly consume: boolean; readonly event: boolean; readonly open: boolean; readonly publish: boolean };
        readonly deliver: string;
        readonly order: string;
        readonly settle: string;
        readonly replay: string;
        readonly bound: string;
        readonly refuse: string;
        readonly degrade: string;
    };
    // One selector owns every subscription modality: a filter, a filter roster, or rows carrying their own v5 axes.
    type Selector = string | ReadonlyArray<string | Topic>;
    // One target owns every publish modality on the same law: a bare topic, or a row carrying its own v5 post axes.
    type Target = string | Post;
    type Grade = (typeof _MQTT_GRADES)[QoS];
    type Terminal = keyof typeof _MQTT_TERMINALS;
    type _Grades<T extends Record<QoS, { readonly guarantee: string; readonly degrade: string }> = typeof _MQTT_GRADES> = T;
    type Frame = {
        readonly topic: string;
        readonly body: Uint8Array;
        readonly band: Band;
        readonly media: NonNullable<IPublishPacket['properties']>['contentType'];
    };
    type Body = Uint8Array | CloudEvent<unknown>;
}

const _mqttBand = (packet: IPublishPacket): Mqtt.Band => packet.properties?.userProperties ?? {};
const _mqttUtf8 = { read: new TextDecoder(), write: new TextEncoder() } as const;

const _mqttPublishBand = (band: Carrier.Frame['mqtt']): Mqtt.PublishBand =>
    Record.map(band, (value) => typeof value === 'string' ? value : [...value]);

const _mqttBindingBand = (message: MQTTMessage): Carrier.Frame['mqtt'] =>
    Record.fromEntries(
        Object.entries(message.headers).flatMap(([key, value]) =>
            value === undefined ? [] : [[key, typeof value === 'string' ? value : [...value]] as const]),
    );

const _mqttBody = (message: MQTTMessage, origin: string): Effect.Effect<Uint8Array, MqttFault> =>
    typeof message.body === 'string'
        ? Effect.succeed(_mqttUtf8.write.encode(message.body))
        : message.body instanceof Uint8Array
          ? Effect.succeed(new Uint8Array(message.body))
          : message.body === undefined
            ? Effect.succeed(new Uint8Array())
            : Effect.flatMap(
                  Effect.try({
                      try: () => JSON.stringify(message.body),
                      catch: (cause) => new MqttFault({ origin, reason: 'publish', detail: `<unserializable-body:${String(cause)}>` }),
                  }),
                  (json) =>
                      Option.match(Option.fromNullable(json), {
                          onNone: () => Effect.fail(new MqttFault({ origin, reason: 'publish', detail: '<body-stringify-undefined>' })),
                          onSome: (held) => Effect.succeed(_mqttUtf8.write.encode(held)),
                      }),
              );

const _mqttEvent = (frame: Mqtt.Frame, origin: string): Effect.Effect<CloudEvent<unknown>, MqttFault> =>
    pipe(
        frame.media?.startsWith(CONSTANTS.MIME_CE_JSON) ?? false
            ? _mqttUtf8.read.decode(frame.body)
            : Buffer.from(frame.body),
        (payload): MQTTMessage => ({
            PUBLISH: { 'Content Type': frame.media },
            body: payload,
            headers: frame.band,
            payload,
            'User Properties': frame.band,
        }),
        Option.liftPredicate((message) => MQTT.isEvent(message)),
        Option.match({
            onNone: () => Effect.fail(new MqttFault({ origin, reason: 'event', detail: '<not-a-cloudevent-message>' })),
            onSome: (message) =>
                Effect.flatMap(
                    Effect.try({
                        try: () => MQTT.toEvent<unknown>(message),
                        catch: (cause) => new MqttFault({ origin, reason: 'event', detail: `<toevent-rejected:${String(cause)}>` }),
                    }),
                    (decoded) =>
                        pipe(
                            globalThis.Array.isArray(decoded) ? decoded : [decoded],
                            Option.liftPredicate((events) => events.length === 1),
                            Option.flatMap(Array.head),
                            Option.match({
                                onNone: () => Effect.fail(new MqttFault({ origin, reason: 'event', detail: '<batch-not-single>' })),
                                onSome: (event) =>
                                    Effect.try({
                                        try: () => new CloudEvent(event),
                                        catch: (cause) => new MqttFault({ origin, reason: 'event', detail: `<cloudevent-invalid:${String(cause)}>` }),
                                    }),
                            }),
                        ),
                ),
        }),
    );

const _mqttConnect = (broker: Mqtt.Broker): Effect.Effect<MqttClient, MqttFault> =>
    Effect.tryPromise({
        try: () => connectAsync(broker.origin.href, { protocolVersion: 5, keepalive: broker.keepalive }),
        catch: (cause) => new MqttFault({ origin: broker.origin.href, reason: 'dial', detail: String(cause) }),
    });

// Terminal rows carry what each event actually knows: only `error` holds a cause and only `disconnect` holds a v5
// reason code, so one nullary handler across all four discards the sole evidence the seam ever receives.
const _MQTT_TERMINALS = {
    close: { detail: () => '<broker-closed>' },
    error: { detail: (cause: unknown) => `<broker-error:${String(cause)}>` },
    disconnect: { detail: (packet: unknown) => `<broker-disconnect:${String((packet as IDisconnectPacket)?.reasonCode)}>` },
    // Clients reconnect on their own after `offline`, so this row names a stream the seam ends while the socket
    // beneath it keeps retrying — a caller reading a bare transport fault would re-dial a client already dialing.
    offline: { detail: () => '<broker-offline:client-retrying>' },
} as const;

// Twin of the subscription fold on the publish side: a bare topic takes the broker row's grade and retain, a row names
// its own v5 post axes. Absent cells never emit their property, so a v5 default is the broker's rather than this fold's.
const _mqttPublish = (
    broker: Mqtt.Broker,
    target: Mqtt.Target,
    media: string | undefined,
    band: Mqtt.PublishBand,
): { readonly topic: string; readonly options: IClientPublishOptions } => {
    const post = typeof target === 'string' ? new _MqttPost({ topic: target }) : target;
    return {
        topic: post.topic,
        options: {
            qos: Option.getOrElse(post.qos, () => broker.qos),
            retain: Option.getOrElse(post.retain, () => broker.retain),
            properties: {
                ...(media === undefined ? {} : { contentType: media }),
                ...Option.match(post.expiry, { onNone: () => ({}), onSome: (seconds) => ({ messageExpiryInterval: seconds }) }),
                ...Option.match(post.respond, { onNone: () => ({}), onSome: (topic) => ({ responseTopic: topic }) }),
                ...Option.match(post.correlate, { onNone: () => ({}), onSome: (bytes) => ({ correlationData: Buffer.from(bytes) }) }),
                userProperties: band,
            },
        },
    };
};

// One fold owns every subscription modality: a bare filter takes the broker row's grade, a row names its own v5 axes.
const _mqttSubscription = (broker: Mqtt.Broker, topics: Mqtt.Selector): ISubscriptionMap =>
    Record.fromEntries(
        (typeof topics === 'string' ? [topics] : topics).map((entry) =>
            typeof entry === 'string'
                ? ([entry, { qos: broker.qos }] as const)
                : ([entry.filter, {
                      qos: Option.getOrElse(entry.qos, () => broker.qos),
                      nl: !entry.local,
                      rap: entry.asPublished,
                      rh: entry.retained,
                  }] as const)),
    );

const _mqttOpen = (broker: Mqtt.Broker, topics: Mqtt.Selector): Stream.Stream<Mqtt.Frame, MqttFault> =>
    Stream.asyncScoped<Mqtt.Frame, MqttFault>((emit) =>
        Effect.acquireRelease(
            Effect.gen(function* () {
                const client = yield* _mqttConnect(broker);
                return yield* Effect.gen(function* () {
                    const grants = yield* Effect.tryPromise({
                        try: () => client.subscribeAsync(_mqttSubscription(broker, topics)),
                        catch: (cause) => new MqttFault({ origin: broker.origin.href, reason: 'dial', detail: String(cause) }),
                    });
                    // Refused filters name THEMSELVES: a bare boolean leaves a caller re-subscribing every filter to
                    // find which one the broker's ACL rejected, and a partial grant is the common shape, never the rare one.
                    const refused = grants.filter((grant) => grant.qos === 128).map((grant) => grant.topic);
                    if (refused.length > 0) {
                        return yield* Effect.fail(
                            new MqttFault({ origin: broker.origin.href, reason: 'grant', detail: `<refused:${refused.join('|')}>` }),
                        );
                    }
                    let terminated = false;
                    const message = (topic: string, body: Uint8Array, packet: IPublishPacket) => {
                        if (terminated) return;
                        void emit.single({
                            topic,
                            body: new Uint8Array(body),
                            band: _mqttBand(packet),
                            media: packet.properties?.contentType,
                        });
                    };
                    const terminate = (event: Mqtt.Terminal) => (evidence?: unknown) => {
                        if (terminated) return;
                        terminated = true;
                        void emit.fail(
                            new MqttFault({ origin: broker.origin.href, reason: 'dial', detail: _MQTT_TERMINALS[event].detail(evidence) }),
                        );
                    };
                    const terminals = Record.map(_MQTT_TERMINALS, (_row, event) => terminate(event as Mqtt.Terminal));
                    client.on('message', message);
                    for (const [event, listener] of Object.entries(terminals)) client.on(event as Mqtt.Terminal, listener);
                    return { client, message, terminals };
                }).pipe(
                    Effect.tapError(() =>
                        Effect.orDie(
                            Effect.tryPromise({
                                try: () => client.endAsync(),
                                catch: (cause) => new MqttFault({ origin: broker.origin.href, reason: 'dial', detail: String(cause) }),
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
                                for (const [event, listener] of Object.entries(live.terminals)) {
                                    live.client.off(event as Mqtt.Terminal, listener);
                                }
                            }
                        },
                        catch: (cause) => new MqttFault({ origin: broker.origin.href, reason: 'dial', detail: String(cause) }),
                    }),
                ),
        ),
    );

class Mqtt extends Context.Tag('runtime/Mqtt')<
    Mqtt,
    {
        readonly consume: (
            topics: Mqtt.Selector,
            handler: (frame: Mqtt.Frame) => Effect.Effect<void, MqttFault>,
        ) => Effect.Effect<void, MqttFault>;
        readonly event: (frame: Mqtt.Frame) => Effect.Effect<CloudEvent<unknown>, MqttFault>;
        readonly open: (topics: Mqtt.Selector) => Stream.Stream<Mqtt.Frame, MqttFault>;
        readonly publish: (target: Mqtt.Target, body: Mqtt.Body, band?: Mqtt.Band) => Effect.Effect<void, MqttFault>;
    }
>() {
    static readonly Broker = _MqttBroker;
    static readonly Post = _MqttPost;
    static readonly Topic = _MqttTopic;
    static readonly grades = _MQTT_GRADES;
    // Callers read the descriptor instead of discovering a coordinate by fault, exactly as `Fanout.engine` publishes its rows.
    static readonly row = _MQTT_ROW;
    static readonly live = (broker: Mqtt.Broker): Layer.Layer<Mqtt, MqttFault> =>
        Layer.scoped(
            Mqtt,
            Effect.map(
                Effect.acquireRelease(_mqttConnect(broker), (client) =>
                    Effect.orDie(
                        Effect.tryPromise({
                            try: () => client.endAsync(),
                            catch: (cause) => new MqttFault({ origin: broker.origin.href, reason: 'dial', detail: String(cause) }),
                        }),
                    )),
                (publisher) => ({
                    consume: (topics, handler) =>
                        Stream.runForEach(_mqttOpen(broker, topics), (frame) =>
                            Propagation.ingress(handler(frame), Carrier.extract('mqtt', frame.band)),
                        ),
                    event: (frame) => _mqttEvent(frame, broker.origin.href),
                    open: (topics) => _mqttOpen(broker, topics),
                    publish: (target, body, band = {}) =>
                        Effect.flatMap(Propagation.current, (context) =>
                            Effect.flatMap(
                                body instanceof CloudEvent
                                    ? Effect.map(
                                          Effect.try({
                                              try: () => MQTT.binary(body),
                                              catch: (cause) =>
                                                  new MqttFault({ origin: broker.origin.href, reason: 'publish', detail: `<binary-binding-rejected:${String(cause)}>` }),
                                          }),
                                          (message) => ({
                                              message,
                                              band: _mqttBindingBand(message),
                                              media: message.PUBLISH?.['Content Type'],
                                          }),
                                      ).pipe(
                                          Effect.flatMap(({ message, band: bindingBand, media }) =>
                                              Effect.map(_mqttBody(message, broker.origin.href), (payload) => ({
                                                  payload,
                                                  band: bindingBand,
                                                  media,
                                              }))),
                                      )
                                    : Effect.succeed({ payload: new Uint8Array(body), band: {}, media: undefined }),
                                ({ payload, band: bindingBand, media }) =>
                                    pipe(
                                        _mqttPublish(
                                            broker,
                                            target,
                                            media,
                                            _mqttPublishBand(Carrier.inject('mqtt', context, { ...band, ...bindingBand })),
                                        ),
                                        ({ topic, options }) =>
                                            Effect.tryPromise({
                                                try: () => publisher.publishAsync(topic, Buffer.from(payload), options),
                                                catch: (cause) =>
                                                    new MqttFault({ origin: broker.origin.href, reason: 'publish', detail: String(cause) }),
                                            }),
                                    ).pipe(Effect.asVoid),
                            ),
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
