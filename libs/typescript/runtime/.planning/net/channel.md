# [RUNTIME_CHANNEL]

Framed stream transport is the second half of the branch net plane: where `client` owns request/response egress, this page owns long-lived byte channels — socket duplex under a closed frame vocabulary and the server-sent event feed — all backpressured by construction and typed at the seam. Causal context crosses each channel as data: ingress surfaces the arriving carrier band verbatim, and the consumer extracts the transport's core dialect before the one continuation transformer, so a channel never orphans a producing span and never imports the telemetry plane. Sockets are capability: construction rides `Socket.makeWebSocket` against the `Socket.WebSocketConstructor` Tag the runtime binding satisfies, so one framed transport definition serves every runtime lane, and the frame is a row swap under an unchanged schema seam. Its SSE feed owns the full `Sse` codec: `Sse.makeChannel` decodes the `data:`/`event:`/`id:`/`retry:` line protocol as package capability — the `retry:` directive is absorbed in-channel, the parser sleeping the hinted delay in place — the reattach cursor advances as a fold and stamps `last-event-id` on every re-dial, a cleanly completed response reconnects exactly like a faulted one, silence folds through the core degradation ladder to pick probe cadence, and `Sse.encoder` is the mirror the serving edge composes so both directions of the dialect have one codec owner. Named defects: a raw socket listener, a hand `data:`-line parser, a reconnect observing only transport faults, and `JSON.stringify` written to a wire. Its module is `runtime/src/net/channel.ts`.

## [01]-[INDEX]

- [02]-[FRAME_ROWS]: `Duplex` — the `ndjson | msgpack` frame vocabulary fused with one schema seam.
- [03]-[FEED_SEAM]: `Feed` — the SSE session: codec, cursor fold, Retry-driven reconnect, silence ladder.
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
import {
    Array,
    type Channel,
    type Chunk,
    Context,
    Duration,
    Effect,
    Either,
    Layer,
    Option,
    ParseResult,
    Record,
    Ref,
    Schema,
    type Scope,
    Stream,
    pipe,
} from 'effect';
import { CloudEvent, CONSTANTS, HTTP, MQTT, MQTTMessageFactory, type CloudEventV1, type MQTTMessage } from 'cloudevents';
import { Type, type schema } from 'avsc';
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
import { Carrier, Fault, Format } from '@rasm/ts/core';
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
// Only a GRADED response carries a status, so that column rides the `media` row alone: an `Option<number>` on the
// shared carrier let a transport refusal declare a slot it could never fill, and every reader paid an arm for a case
// that never arrived. The cursor rides both, because a reattach position is what either refusal resumes from.
const _feedFamily = Fault.Class.family(['transport', 'media'] as const, {
    transport: Fault.Class.row({
        class: 'unavailable',
        leg: 'feed',
        detail: Schema.Struct({ origin: Schema.String, cursor: Schema.OptionFromSelf(Schema.String) }),
        render: ({ origin, cursor }) =>
            `${origin} feed dial failed at ${Option.getOrElse(cursor, () => '<stream-head>')}`,
    }),
    media: Fault.Class.row({
        class: 'unavailable',
        leg: 'feed',
        detail: Schema.Struct({
            origin: Schema.String,
            status: Schema.Int,
            cursor: Schema.OptionFromSelf(Schema.String),
        }),
        render: ({ origin, status, cursor }) =>
            `${origin} answered ${status} without text/event-stream at ${Option.getOrElse(cursor, () => '<stream-head>')}`,
    }),
});

class FeedFault extends Schema.TaggedError<FeedFault>()('FeedFault', {
    case: _feedFamily.payload,
}) {
    get class(): Fault.Class.Kind {
        return _feedFamily.classOf(this.case.reason);
    }
    override get message(): string {
        return _feedFamily.render(this.case);
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
                : Effect.fail(
                      new FeedFault({ case: { reason: 'media', origin: session.origin.href, status: response.status, cursor } }),
                  ),
        ),
    ).pipe(
        Stream.mapError((fault) =>
            fault instanceof FeedFault
                ? fault
                : new FeedFault({ case: { reason: 'transport', origin: session.origin.href, cursor } }),
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
- Owner: `Mqtt` — the MQTT v5 channel. `Mqtt.Broker` carries origin, default delivery grade, default retain posture, and keepalive; `Mqtt.live(broker)` brackets one publisher client, while `open(topics)` brackets its own subscription client and listener. `consume(topics, handler)` is the admitted handling ingress, handing each frame beside its extracted `mqtt` carrier. No client or emitter crosses an app boundary.
- Law: causal context crosses this seam as DATA, holding the page lead's telemetry-blind law on this cluster too — `consume` extracts the `mqtt` dialect through core `Carrier` and hands the whole `Carrier.Extraction` beside the frame, its parse-drop census included, so the consuming seam continues through `otel/emit#CONTINUATION`'s one transformer at its own stratum; a publisher reads its live context at its own seam and hands it into `publish`, which injects it through the exact `mqtt` dialect before the binding band lands — this floor module composes no telemetry import.
- Law: MQTT v5 User Properties carry the `Carrier` frame on publish and consume, and the BINDING owns that namespace whole for an envelope publish — MQTT alone spreads attribute names unprefixed, so the creation-time trace the roster extensions carry and the hop trace the carrier writes collide on three keys, and the binding writing last is what keeps the sealed attribute rather than the hop overwriting it.
- Law: message-envelope bodies select `MQTT.binary` and `Mqtt.event` reads the FRAME before decoding — `Format.event.framed` recovers format and arity in one prefix comparison, `MQTTMessageFactory` mints the frame the binding reads, and one entry answers both arities because the media type already decided which arrived.
- Law: the Avro event format decodes on this lane alone — `Avro` mints the ONE `Type` from the frozen `io.cloudevents.AvroCloudEvent` schema, byte-pinned to `tests/contracts/cloudevents.avsc`, and `Avro.event` is the `Lane`-seat admission the core avro row's empty arm declares, so this seam and the HTTP intake read one codec and no second `Type` mints anywhere.
- Law: `open` preserves the ordered raw-frame lane.
- Law: a subscription is a POLICY ROW, never a filter under one broker grade — `Mqtt.Topic` carries the per-subscription v5 axes the protocol decides (grade, no-local, retain-as-published, retain-handling) and `_mqttSubscription` folds every selector modality into one `ISubscriptionMap`; a bare filter still admits and takes the broker row's grade, so `local` is expressible per topic and one client publishing and consuming one filter no longer re-reads its own posts.
- Law: a post is a POLICY ROW on the same law — v5 decides grade, retain, message expiry, and the response/correlation pair PER PUBLISH PACKET, so `Mqtt.Post` carries them and `_mqttPublish` folds the row against the broker defaults into one `IClientPublishOptions`; a bare topic string still admits, and `dup` stays foreclosed because the client raises it on its own redelivery and a caller setting it forges a replay marker.
- Law: `_MQTT_GRADES` states each delivery grade's forfeit — v5 carries no broker-side dedup at any grade, so QoS 1 duplicates on redelivery and only QoS 2 removes them at its four-packet cost; a caller reads the row rather than inferring a guarantee from a number.
- Law: `_MQTT_ROW` is the seam's one consumption descriptor and every coordinate reads off it — selection (`fits`, `admit`, `tenancy`, `lifetime`), guarantee (`deliver`, `order`, `settle`), recovery (`replay`, `bound`, `refuse`), and the residual `degrade` no column expresses; a caller comparing this seam against `pubsub#PORT_SHAPE` reads the same column names, and a coordinate restated as prose beside the row forks it.
- Law: `serves` closes the member roster this engine answers, so a caller wanting replay, a positional cursor, or a consumer census reads `pubsub#PORT_SHAPE` rather than bending a topic into one.
- Law: subscription admission is evidence — every `subscribeAsync` grant is inspected, any `qos: 128` refusal fails the typed `grant` rail before a message stream escapes, and the refusal NAMES the filters the broker rejected rather than reporting that some filter failed.
- Law: terminal events carry unequal evidence and `_MQTT_TERMINALS` keeps it — only `error` holds a cause and only `disconnect` holds a v5 reason code, so one nullary handler across all four discards the sole diagnosis the seam receives; `offline` names a client still retrying beneath an ended stream, never a dead transport. Failed subscription or grant admission ends the minted client before the fault escapes; successful acquisition transfers that client to the stream scope. Message and lifecycle listeners share the stream scope; `close`, `error`, `disconnect`, and `offline` terminate the stream once, and release ends the client before detaching the complete listener row.
- Law: Raw frames keep opaque bytes; message-envelope callers cross only through the MQTT binding projection, and a raw publish keeps the hop carrier whole because no binding claims its User Properties.
- Packages: `mqtt`, `cloudevents`, `avsc` (`Type`, `schema` — the host-bound engine riding the `Lane` seat), `effect`, `node:buffer`, and `@rasm/ts/core` (`Carrier`, `Fault`, `Format`).

```typescript signature
// `reason` bands the fault and the row's own subject proves it: four bands cover fourteen mint sites, so without the
// operand that decided one, a refused grant, a server disconnect, and a binding rejection all read as one string.
// `grant` is the row that earns a shaped subject — the refused FILTERS themselves, in an array the render joins —
// because that is the fact a caller repairs from, and a joined string forced every reader to re-split it.
const _MqttOrigin = Schema.Struct({ origin: Schema.String, detail: Schema.String });

const _mqttFamily = Fault.Class.family(['dial', 'grant', 'event', 'publish'] as const, {
    dial: Fault.Class.row({
        class: 'unavailable',
        leg: 'mqtt',
        detail: Schema.Struct({ origin: Schema.String, cause: Schema.String }),
        render: ({ origin, cause }) => `${origin} mqtt transport failed: ${cause}`,
    }),
    grant: Fault.Class.row({
        class: 'malformed',
        leg: 'mqtt',
        detail: Schema.Struct({ origin: Schema.String, filters: Schema.NonEmptyArray(Schema.String) }),
        render: ({ origin, filters }) => `${origin} broker refused filters ${Array.join(filters, '|')}`,
    }),
    event: Fault.Class.row({
        class: 'malformed',
        leg: 'mqtt',
        detail: _MqttOrigin,
        render: ({ origin, detail }) => `${origin} refused the event frame: ${detail}`,
    }),
    publish: Fault.Class.row({
        class: 'unavailable',
        leg: 'mqtt',
        detail: _MqttOrigin,
        render: ({ origin, detail }) => `${origin} refused the publish: ${detail}`,
    }),
});

class MqttFault extends Schema.TaggedError<MqttFault>()('MqttFault', {
    case: _mqttFamily.payload,
}) {
    get class(): Fault.Class.Kind {
        return _mqttFamily.classOf(this.case.reason);
    }
    override get message(): string {
        return _mqttFamily.render(this.case);
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
    degrade: '<no-partition-key,no-origin-coordinate,no-flow-control-member;every-v5-field-drops-silently-under-V311-while-the-publish-still-succeeds;an-envelope-publish-forfeits-the-hop-carrier-because-the-binding-owns-the-unprefixed-user-property-namespace;a-batch-frame-decodes-json-alone>',
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
                      catch: (cause) =>
                          new MqttFault({ case: { reason: 'publish', origin, detail: `<unserializable-body:${String(cause)}>` } }),
                  }),
                  (json) =>
                      Option.match(Option.fromNullable(json), {
                          onNone: () =>
                              Effect.fail(new MqttFault({ case: { reason: 'publish', origin, detail: '<body-stringify-undefined>' } })),
                          onSome: (held) => Effect.succeed(_mqttUtf8.write.encode(held)),
                      }),
              );

// `MQTTMessageFactory` is the package's OWN frame mint: it seats the `PUBLISH`/`payload`/`User Properties` aliases the
// binding reads off one body, so a hand-built literal beside it can only drift from the shape that binding expects.
// Transcribed VERBATIM from the frozen contract asset `tests/contracts/cloudevents.avsc`; the proof estate diffs this
// literal against that fixture, so a drift on either side fails a test rather than skewing the wire.
const _AVRO_SCHEMA = {
    namespace: 'io.cloudevents',
    type: 'record',
    name: 'AvroCloudEvent',
    version: '1.0',
    doc: 'Avro Event Format for CloudEvents',
    fields: [
        { name: 'attribute', type: { type: 'map', values: ['null', 'boolean', 'int', 'string', 'bytes'] } },
        {
            name: 'data',
            type: [
                'bytes',
                'null',
                'boolean',
                {
                    type: 'map',
                    values: [
                        'null',
                        'boolean',
                        {
                            type: 'record',
                            name: 'AvroCloudEventData',
                            doc: 'Representation of a JSON Value',
                            fields: [
                                {
                                    name: 'value',
                                    type: {
                                        type: 'map',
                                        values: [
                                            'null',
                                            'boolean',
                                            { type: 'map', values: 'AvroCloudEventData' },
                                            { type: 'array', items: 'AvroCloudEventData' },
                                            'double',
                                            'string',
                                        ],
                                    },
                                },
                            ],
                        },
                        'double',
                        'string',
                    ],
                },
                { type: 'array', items: 'AvroCloudEventData' },
                'double',
                'string',
            ],
        },
    ],
};

// `_avroType` is the ONE mint, compiled at module initialization — the core format contract owns the codec identity
// and no second `Type` constructs anywhere. Every union the contract declares is bucket-disjoint, so `wrapUnions`
// lands unwrapped either way; STATING it pins the posture whose change respells every encoded payload. Bundled
// typings declare themselves incomplete, so the schema value crosses their seam on one marked pin.
const _avroType = Type.forSchema(_AVRO_SCHEMA as schema.AvroSchema, { wrapUnions: 'never' });

// Buffer crosses at this seam alone: egress passes the returned view straight because a Buffer IS a Uint8Array, and
// ingress wraps because the reader indexes Buffer-only slice methods. Both engine members convert the codec's throw
// onto the Either rail here, so no exception channel reaches the core composition.
const _avroEngine: Format.Event.Engine = {
    read: (octets) =>
        Either.try({
            try: (): unknown => _avroType.fromBuffer(Buffer.from(octets)),
            catch: (cause) => `<avro-decode-rejected:${String(cause)}>`,
        }),
    write: (value) =>
        Either.try({
            try: () => new Uint8Array(_avroType.toBuffer(value)),
            catch: (cause) => `<avro-encode-rejected:${String(cause)}>`,
        }),
};

const _AvroAttribute = Schema.Union(Schema.Null, Schema.Boolean, Schema.Number, Schema.String, Schema.Uint8ArrayFromSelf);
const _isAttributeCell = Schema.is(_AvroAttribute);
const _AvroTree = Schema.Struct({
    attribute: Schema.Record({ key: Schema.String, value: _AvroAttribute }),
    data: Schema.Unknown,
});
const _AVRO_REQUIRED = ['id', 'source', 'specversion', 'type'] as const;
const _isAvroEnvelope = (input: unknown): input is CloudEventV1<unknown> =>
    typeof input === 'object' && input !== null
    && _AVRO_REQUIRED.every((key) => typeof (input as Record<string, unknown>)[key] === 'string');

// CloudEvents' Avro schema seats context attributes in ONE string-keyed map beside the payload, so the lift
// spreads the map whole and the four REQUIRED attributes gate here — a tree missing `id` refuses at the seam,
// never downstream as a half-shaped envelope. Egress inverts the lift: every member except `data` is a context
// attribute, and a cell outside the map's five value types refuses here as admission evidence, never as an
// engine throw at the wire.
const _AvroEnvelope: Schema.Schema<CloudEventV1<unknown>, typeof _AvroTree.Encoded> = Schema.transformOrFail(
    _AvroTree,
    Schema.declare(_isAvroEnvelope),
    {
        strict: true,
        decode: ({ attribute, data }, _, ast) => {
            const lifted: unknown = { ...attribute, data };
            return _isAvroEnvelope(lifted)
                ? ParseResult.succeed(lifted)
                : ParseResult.fail(new ParseResult.Type(ast, lifted, '<missing-required-context-attribute>'));
        },
        encode: (envelope, _, ast) => {
            const context = Record.remove(envelope as unknown as Record<string, unknown>, 'data');
            return Record.values(context).every(_isAttributeCell)
                ? ParseResult.succeed({ attribute: context as Record<string, typeof _AvroAttribute.Type>, data: envelope.data })
                : ParseResult.fail(new ParseResult.Type(ast, envelope, '<attribute-outside-avro-value-union>'));
        },
    },
);

const Avro = {
    engine: _avroEngine,
    // The demand is the codec pair alone — this lane decodes one envelope at a time and MQTT publishes no batch arm —
    // and the engine enters on a `Lane` seat because core law holds the avro row's arm empty. A refusal here means
    // the row gained a core engine and this lane's mint became the second codec the seam denies, so it throws the
    // named refusal rather than degrading to a decoder that never learned the media type.
    event: Either.getOrThrowWith(
        Format.event.admitted('avro', Format.event.demand(), _AvroEnvelope, Format.event.seat.Lane({ engine: _avroEngine })),
        (missing) => missing,
    ),
} as const;

// `cloudevents` decodes JSON alone, so the avro framing routes through the lane-seat admission rather than a
// decoder that refuses a media type it never learned.
const _avroDecoded = (frame: Mqtt.Frame, origin: string): Effect.Effect<Array.NonEmptyReadonlyArray<CloudEventV1<unknown>>, MqttFault> =>
    Effect.mapBoth(Schema.decodeUnknown(Avro.event)(frame.body), {
        onFailure: (issue) => new MqttFault({ case: { reason: 'event', origin, detail: `<avro-rejected:${issue.message}>` } }),
        onSuccess: (envelope) => Array.of(envelope),
    });

// Structured frames carry text and binary frames carry opaque data bytes, so the framing read selects the body.
const _mqttMessage = (frame: Mqtt.Frame, structured: boolean): MQTTMessage =>
    MQTTMessageFactory(
        frame.media ?? CONSTANTS.MIME_OCTET_STREAM,
        frame.band,
        structured ? _mqttUtf8.read.decode(frame.body) : Buffer.from(frame.body),
    );

// Detection reads the FRAME: `Format.event.framed` recovers format and arity from one media-type prefix comparison,
// and a binary frame declares `specversion` among the User Properties MQTT spreads UNPREFIXED — so the package's own
// `isEvent`, which runs a full deserialize inside `try`/`catch`, never parses a frame the decode below parses again.
const _mqttFraming = (frame: Mqtt.Frame): Option.Option<Option.Option<Format.Event.Framing>> =>
    pipe(
        Option.flatMap(Option.fromNullable(frame.media), Format.event.framed),
        (framed) =>
            Option.isSome(framed) || Record.has(frame.band, CONSTANTS.CE_ATTRIBUTES.SPEC_VERSION)
                ? Option.some(framed)
                : Option.none(),
    );

const _mqttDecoded = (
    message: MQTTMessage,
    origin: string,
    decode: (message: MQTTMessage) => CloudEventV1<unknown> | ReadonlyArray<CloudEventV1<unknown>>,
): Effect.Effect<Array.NonEmptyReadonlyArray<CloudEventV1<unknown>>, MqttFault> =>
    Effect.flatMap(
        Effect.try({
            try: () => decode(message),
            catch: (cause) => new MqttFault({ case: { reason: 'event', origin, detail: `<toevent-rejected:${String(cause)}>` } }),
        }),
        (decoded) =>
            pipe(
                globalThis.Array.isArray(decoded) ? decoded : [decoded],
                Option.liftPredicate(Array.isNonEmptyReadonlyArray),
                Option.match({
                    onNone: () => Effect.fail(new MqttFault({ case: { reason: 'event', origin, detail: '<empty-event-frame>' } })),
                    onSome: Effect.succeed,
                }),
            ),
    );

// One entry answers both arities, since the frame already decided which one arrived. MQTT publishes NO batch arm at
// any binding, so this seam owns that half, and the ONE batch decode the package ships is the HTTP binding's JSON
// envelope — whose media type is exactly the `json` row's own batch spelling. A batch frame in another format
// therefore refuses BY FORMAT, naming the codec no binding here decodes, rather than by an arity the frame proved.
const _mqttEvent = (frame: Mqtt.Frame, origin: string): Effect.Effect<Array.NonEmptyReadonlyArray<CloudEventV1<unknown>>, MqttFault> =>
    Option.match(_mqttFraming(frame), {
        onNone: () => Effect.fail(new MqttFault({ case: { reason: 'event', origin, detail: '<not-a-cloudevent-message>' } })),
        onSome: (framed) =>
            Option.match(Option.filter(framed, (framing) => framing.batch), {
                onNone: () =>
                    Option.exists(framed, (framing) => framing.format === 'avro')
                        ? _avroDecoded(frame, origin)
                        : _mqttDecoded(_mqttMessage(frame, Option.isSome(framed)), origin, MQTT.toEvent<unknown>),
                onSome: (framing) =>
                    framing.format === 'json'
                        ? _mqttDecoded(
                              MQTTMessageFactory(
                                  CONSTANTS.MIME_CE_BATCH,
                                  { [CONSTANTS.HEADER_CONTENT_TYPE]: CONSTANTS.MIME_CE_BATCH },
                                  _mqttUtf8.read.decode(frame.body),
                              ),
                              origin,
                              HTTP.toEvent<unknown>,
                          )
                        : Effect.fail(
                              new MqttFault({ case: { reason: 'event', origin, detail: `<no-batch-decoder:${framing.format}>` } }),
                          ),
            }),
    });

const _mqttConnect = (broker: Mqtt.Broker): Effect.Effect<MqttClient, MqttFault> =>
    Effect.tryPromise({
        try: () => connectAsync(broker.origin.href, { protocolVersion: 5, keepalive: broker.keepalive }),
        catch: (cause) => new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
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
                        catch: (cause) =>
                            new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
                    });
                    // Refused filters name THEMSELVES: a bare boolean leaves a caller re-subscribing every filter to
                    // find which one the broker's ACL rejected, and a partial grant is the common shape, never the rare one.
                    // The match is what PROVES the roster non-empty, so the refusal's own array column cannot carry a
                    // vacuous grant and no length test guards a shape the type already closes.
                    const refused = grants.filter((grant) => grant.qos === 128).map((grant) => grant.topic);
                    yield* Array.match(refused, {
                        onEmpty: () => Effect.void,
                        onNonEmpty: (filters) =>
                            Effect.fail(new MqttFault({ case: { reason: 'grant', origin: broker.origin.href, filters } })),
                    });
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
                            new MqttFault({
                                case: { reason: 'dial', origin: broker.origin.href, cause: _MQTT_TERMINALS[event].detail(evidence) },
                            }),
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
                                catch: (cause) =>
                                    new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
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
                        catch: (cause) =>
                            new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
                    }),
                ),
        ),
    );

class Mqtt extends Context.Tag('runtime/Mqtt')<
    Mqtt,
    {
        readonly consume: (
            topics: Mqtt.Selector,
            handler: (frame: Mqtt.Frame, carrier: Carrier.Extraction) => Effect.Effect<void, MqttFault>,
        ) => Effect.Effect<void, MqttFault>;
        readonly event: (frame: Mqtt.Frame) => Effect.Effect<Array.NonEmptyReadonlyArray<CloudEventV1<unknown>>, MqttFault>;
        readonly open: (topics: Mqtt.Selector) => Stream.Stream<Mqtt.Frame, MqttFault>;
        readonly publish: (
            target: Mqtt.Target,
            body: Mqtt.Body,
            band?: Mqtt.Band,
            context?: Option.Option<Carrier.Context>,
        ) => Effect.Effect<void, MqttFault>;
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
                            catch: (cause) =>
                                new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
                        }),
                    )),
                (publisher) => ({
                    consume: (topics, handler) =>
                        Stream.runForEach(_mqttOpen(broker, topics), (frame) =>
                            // extraction is the core dialect read this floor may perform, and it crosses WHOLE: the
                            // handler's own stratum continues the hop through the one ingress transformer, which is
                            // what publishes the parse-drop census this floor measures but never reads
                            handler(frame, Carrier.extract('mqtt', frame.band)),
                        ),
                    event: (frame) => _mqttEvent(frame, broker.origin.href),
                    open: (topics) => _mqttOpen(broker, topics),
                    publish: (target, body, band = {}, context = Option.none()) =>
                        Effect.flatMap(
                            body instanceof CloudEvent
                                ? Effect.map(
                                      Effect.try({
                                          try: () => MQTT.binary(body),
                                          catch: (cause) =>
                                              new MqttFault({
                                                  case: {
                                                      reason: 'publish',
                                                      origin: broker.origin.href,
                                                      detail: `<binary-binding-rejected:${String(cause)}>`,
                                                  },
                                              }),
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
                                        // MQTT spreads attribute names UNPREFIXED, so an envelope publish's User Properties ARE the
                                        // binding's namespace: the caller-handed hop context writes FIRST and the binding band lands
                                        // last, so a hop `traceparent` can never overwrite the creation-time attribute the mint sealed
                                        // and the tenant baggage the authenticated inverse reads survives this publish intact.
                                        _mqttPublishBand({
                                            ...Option.match(context, {
                                                onNone: () => band,
                                                onSome: (hop) => Carrier.inject('mqtt', hop, band),
                                            }),
                                            ...bindingBand,
                                        }),
                                    ),
                                    ({ topic, options }) =>
                                        Effect.tryPromise({
                                            try: () => publisher.publishAsync(topic, Buffer.from(payload), options),
                                            catch: (cause) =>
                                                new MqttFault({
                                                    case: { reason: 'publish', origin: broker.origin.href, detail: String(cause) },
                                                }),
                                        }),
                                ).pipe(Effect.asVoid),
                        ),
                }),
            ),
        );
}
```

```typescript signature
// --- [EXPORTS] --------------------------------------------------------------------------

export { Avro, Duplex, Feed, FeedFault, Mqtt, MqttFault };
```

## [05]-[RESEARCH]

(none)
