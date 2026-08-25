# [RUNTIME_CHANNEL]

Framed stream transport is the second half of the branch net plane: where `client` owns request/response egress, this page owns long-lived byte channels — socket duplex under a closed frame vocabulary and the server-sent event feed — all backpressured by construction and typed at the seam. Causal context crosses each channel as data: ingress surfaces the arriving carrier band verbatim, and the consumer extracts the transport's core dialect before the one continuation transformer, so a channel never orphans a producing span and never imports the telemetry plane. Sockets are capability: construction rides `Socket.makeWebSocket` against the `Socket.WebSocketConstructor` Tag the runtime binding satisfies, so one framed transport definition serves every runtime lane, and the frame is a row swap under an unchanged schema seam. Its SSE feed owns the full `Sse` codec: `Sse.makeChannel` decodes the `data:`/`event:`/`id:`/`retry:` line protocol as package capability — the `retry:` directive is absorbed in-channel, the parser sleeping the hinted delay in place — the reattach cursor advances as a fold and stamps `last-event-id` on every re-dial, a cleanly completed response reconnects exactly like a faulted one, silence folds through the core degradation ladder to pick probe cadence, and `Sse.encoder` is the mirror the serving edge composes so both directions of the dialect have one codec owner. Named defects: a raw socket listener, a hand `data:`-line parser, a reconnect observing only transport faults, and `JSON.stringify` written to a wire. Its module is `runtime/src/net/channel.ts`.

## [01]-[INDEX]

- [02]-[FRAME_ROWS]: `Duplex` — the `ndjson | msgpack | proto` frame vocabulary fused with one schema seam.
- [03]-[FEED_SEAM]: `Feed` — the SSE session: codec, cursor fold, Retry-driven reconnect, silence ladder.
- [04]-[MQTT_SEAM]: MQTT v5 scoped clients, per-subscription and per-post policy rows, the consumption descriptor; `Mqtt`.

## [02]-[FRAME_ROWS]

[FRAME_ROWS]:
- Owner: `Duplex.framed` composes `Socket.toChannel` with the selected fused `Ndjson.duplexSchema`, `MsgPack.duplexSchema`, or size-delimited proto row.
- Law: the frame is a row swap under an unchanged schema seam — the `_frames` table keys each dialect to its fused combinator, dispatch is one keyed lookup, and `Duplex.Kind` derives from the table; moving a peer from ndjson to msgpack edits one argument and no consumer, and a new frame dialect is one row, zero arms.
- Law: the frame is a VALUE carrying what its row needs — `{ kind: "proto", descriptor }` names the message the size-delimited row decodes, the two self-describing rows carry the kind alone — so the descriptor is recoverable from the argument and no caller spells a dialect twice.
- Law: the proto row is `Format.proto.framed` — `sizeDelimitedEncode` out, a `sizeDelimitedPeek` fold in under `Shape.Ingress.floor.bytes` per frame. `ArtifactService.Fetch` is the generated server-streaming retrieval seam and travels through `Invoke.Dial.sdk`; this page claims no artifact socket consumer beside that RPC.
- Law: the protocol pair is send/take symmetric evidence — `send` types the outbound seam, `take` the inbound seam, both usually one closed `Schema.Union` of tagged messages; an untyped frame crossing the channel is unspellable because the fused combinator is the only construction.
- Law: fault families arrive typed and stay separate — the frame's own error, `Socket.SocketError`, and `ParseError` each route on their own tag; none is re-wrapped.
- Law: causal context rides the protocol, never the frame — a duplex peer whose messages carry `traceparent`/`tracestate`/`baggage` declares those fields on its `take` schema, and the consumer extracts its admitted dialect through core `Carrier` before `Propagation.ingress` at the handling seam; the frame rows stay context-blind and this floor module composes no telemetry import.
- Boundary: socket construction is capability — `Socket.makeWebSocket(url)` demands the `WebSocketConstructor` Tag, satisfied by the runtime binding at the root; session lifetime, reconnect, and the pipeline geometry above the channel are the consumer's, composed from `Stream` law.
- Entry: `Duplex.framed(socket, { kind }, { send, take })`; the proto row takes `{ kind: "proto", descriptor }`.
- Packages: `@effect/platform` (`Socket`, `Ndjson`, `MsgPack`, `ChannelSchema`), `@bufbuild/protobuf` (`DescMessage`), `@rasm/core` (`Format.proto.framed`), `effect` (`Channel`, `Chunk`, `Schema`).

```typescript
import { Sse } from '@effect/experimental';
import type { DescMessage } from '@bufbuild/protobuf';
import { ChannelSchema, type HttpClient, HttpClientRequest, MsgPack, Ndjson, Socket } from '@effect/platform';
import {
    Array,
    type Channel,
    type Chunk,
    Context,
    Data,
    Duration,
    Effect,
    Either,
    Layer,
    Option,
    ParseResult,
    Predicate,
    Record,
    Redacted,
    Ref,
    Schema,
    type Scope,
    Stream,
    pipe,
} from 'effect';
import { type CloudEvent, CONSTANTS, MQTT, MQTTMessageFactory, type CloudEventV1, type MQTTMessage } from 'cloudevents';
import { CloudEventsAvro } from '@rasm\/contracts/io/cloudevents/v1/cloudevents_avro';
import { Type, type schema } from 'avsc';
import {
    connectAsync,
    type IClientOptions,
    type IClientPublishOptions,
    type IDisconnectPacket,
    type IPublishPacket,
    type ISubscriptionMap,
    type MqttClient,
    type QoS,
} from 'mqtt';
import { Buffer } from 'node:buffer';
import { Carrier, Event, Fault, Format } from '@rasm/core';
import type { MachinePrincipal } from '@rasm/security';
import { Client, Machine } from './client.ts';

type _Frame = { readonly kind: 'msgpack' } | { readonly kind: 'ndjson' } | { readonly kind: 'proto'; readonly descriptor: DescMessage };
const _frames = {
    msgpack: (seam: Duplex.Seam, _frame: _Frame) => MsgPack.duplexSchema(seam),
    ndjson: (seam: Duplex.Seam, _frame: _Frame) => Ndjson.duplexSchema(seam),
    proto: (seam: Duplex.Seam, frame: Extract<_Frame, { readonly kind: 'proto' }>) =>
        <R, IE, OE, OutDone, InDone>(
            self: Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<Uint8Array>, OE, IE, OutDone, InDone, R>,
        ) => ChannelSchema.duplexUnknown(seam)(Format.proto.framed(frame.descriptor)(self)),
} as const;

declare namespace Duplex {
    type Kind = keyof typeof _frames;
    type Frame = _Frame;
    type Fault = MsgPack.MsgPackError | Ndjson.NdjsonError | ParseResult.ParseError;
    type Protocol<Send, SendI, Take, TakeI> = {
        readonly send: Schema.Schema<Send, SendI>;
        readonly take: Schema.Schema<Take, TakeI>;
    };
    type Seam = { readonly inputSchema: Schema.Schema.AnyNoContext; readonly outputSchema: Schema.Schema.AnyNoContext };
    type _Rows<T extends { readonly [K in Frame['kind']]: unknown } = typeof _frames> = T;
}

const _framed = <Send, SendI, Take, TakeI>(
    socket: Socket.Socket,
    frame: Duplex.Frame,
    protocol: Duplex.Protocol<Send, SendI, Take, TakeI>,
): Channel.Channel<
    Chunk.Chunk<Take>,
    Chunk.Chunk<Send>,
    Duplex.Fault | Socket.SocketError,
    ParseResult.ParseError,
    void,
    unknown
> =>
    Socket.toChannel<Duplex.Fault>(socket).pipe(
        frame.kind === 'proto'
            ? _frames.proto({ inputSchema: protocol.send, outputSchema: protocol.take }, frame)
            : _frames[frame.kind]({ inputSchema: protocol.send, outputSchema: protocol.take }, frame),
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
- Packages: `@effect/experimental`, `@effect/platform`, `effect`, `./client.ts`, and `@rasm/core` (`Fault.Budget`).

```typescript
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
                Stream.concat(Stream.drain(Stream.fromEffect(Effect.sleep(session.redial)))),
                Stream.forever,
                Stream.retry(Fault.Budget.schedule('feed')),
            ),
        ),
    );
```

## [04]-[MQTT_SEAM]

[MQTT_SEAM]:
- Owner: `Mqtt` — the MQTT v5 channel. `Mqtt.Broker` carries origin, admitted Rasm classifications, default delivery grade, default retain posture, keepalive, the broker-side in-flight window, and the session-expiry span a withheld acknowledgement re-offers under; `Mqtt.live(broker)` brackets one publisher client, while `open(topics)` brackets its own subscription client. `consume(topics, handler)` is the admitted handling ingress, handing each frame beside its extracted `mqtt` carrier and settling the grade's acknowledgement on the handler's own outcome. No client or emitter crosses an app boundary.
- Law: causal context crosses this seam as DATA, holding the page lead's telemetry-blind law on this cluster too — `consume` extracts the `mqtt` dialect through core `Carrier` and hands the whole `Carrier.Extraction` beside the frame, its parse-drop census included, so the consuming seam continues through `otel/emit#CONTINUATION`'s one transformer at its own stratum; a publisher reads its live context at its own seam and hands it into `publish`, which injects it through the exact `mqtt` dialect before the binding band lands — this floor module composes no telemetry import.
- Law: MQTT v5 User Properties carry the `Carrier` frame on publish and consume, and the BINDING owns that namespace whole for an envelope publish — MQTT alone spreads attribute names unprefixed, so the creation-time trace the roster extensions carry and the hop trace the carrier writes collide on three keys, and the binding writing last is what keeps the sealed attribute rather than the hop overwriting it.
- Law: publish shape is a closed carrier, not an overloaded host union — `Mqtt.Body.Raw` carries octets plus optional media, while `Mqtt.Body.Event` carries one admitted envelope and exactly one supported singular format (`binary | json | protobuf | avro`). The format row uses `MQTT.binary` for transport binary, the SDK-backed core JSON event codec for structured JSON, and the generated/publisher codecs for Protobuf/Avro; no batch constructor exists and no unknown body is stringified into wire data.
- Law: `Mqtt.event` reads the frame before decoding — `Format.event.framed` recovers format and arity from exact parsed media identity, `MQTTMessageFactory` mints the singular frame the binding reads, and every batch frame refuses because the MQTT binding defines no batch mode.
- Law: binary evidence and structured media are mutually exclusive; a packet carrying both refuses before either decoder runs. Hop propagation is extracted from the packet band and stripped from binary event reconstruction, so transport trace and creation-time extensions cannot overwrite each other.
- Law: the broker's one classification row governs Rasm-profile ingress and egress. Outbound admission recognizes the profile through `Event.rasm.Fact`, requires its admitted class before encoding, and leaves generic CloudEvents generic; inbound `Mqtt.EventPolicy` adds source trust before return. Generated `dataref` refuses because this binding owns no residence or resolver.
- Law: MQTT authenticates no tenant inverse. Ingress removes `rasm.tenant` from hop and creation baggage while retaining all other admitted members; a transport claim can never establish ambient tenancy.
- Law: inbound settlement is the HANDLER's verdict, never delivery — mqtt.js emits `message` and then calls `client.handleMessage(packet, done)`, and that callback is what releases the PUBACK at QoS 1 and the PUBCOMP at QoS 2, both grades routing through the one member. Event listeners therefore read a frame the broker already counts acknowledged, so a handler fault, a scope teardown, or a crash between the two loses it permanently while the row advertises at-least-once. `consume` overrides `handleMessage` and settles on the handler's own outcome; a refusal withholds the acknowledgement so the broker re-offers on SESSION RESUME, which is why `clean: false` beside the row's session expiry is a declared coordinate rather than a client default.
- Law: `open` preserves the ordered raw-frame lane and settles on ARRIVAL, stated rather than inherited — a caller folding that stream holds no settlement handle, so the acknowledgement rides delivery exactly as an unsettled listener does and `serves` points a caller wanting handler-gated delivery at `consume`.
- Law: the credential rides the CONNECT frame's own `username`/`password` pair and never a User Property — v5 defines no bearer band, and a property carrying a token authenticates a payload nobody checks while the session stays anonymous. `password` takes the BARE `MachinePrincipal.token`, because `credential` prefixes the HTTP scheme its issuer chose and no broker parses that, while `username` takes the `clientId` the broker authorizes against; a source holding nothing for this origin dials unauthenticated rather than presenting an empty pair every broker refuses.
- Law: rotation costs a re-dial and this seam forks no supervisor for it — the CONNECT packet rebuilds from `client.options` on every dial, so a refreshed credential written into that record lands on the next handshake, while `reconnect()` REPLACES both packet stores and discards every queued QoS>0 message. Forking a rotation fiber here trades credential freshness for silent publish loss, so `present` states the cost and the composition root owns the swap.
- Law: the in-flight window is a declared pair — `receiveMaximum` from the row's `inflight` caps what the broker holds unacknowledged toward this client, and the emit mailbox declares its own capacity because `Stream.asyncScoped` takes a sixteen-slot bounded queue when handed nothing. Suspending is the only strategy that composes with a handler-gated acknowledgement: dropping or sliding discards a frame this seam already told the broker to hold. What stays genuinely unbounded is the client's own offline queue and packet stores, and `bound` names that rather than claiming a ceiling nobody set.
- Law: teardown FLUSHES under a deadline because the package's graceful arm carries none — `end(false)` parks on `outgoingEmpty` until every unacknowledged QoS>0 publish is matched, so an unresponsive broker parks a closing scope forever. Graceful teardown runs first and the forced arm follows past the window, since a scope that cannot close is worse than a DISCONNECT the broker never reads; the offline queue holding QoS-0 publishes and control packets drains under neither arm, so a publish that must survive teardown rides QoS 1.
- Law: a subscription is a POLICY ROW, never a filter under one broker grade — `Mqtt.Topic` carries the per-subscription v5 axes the protocol decides (grade, no-local, retain-as-published, retain-handling) and `_mqttSubscription` folds every selector modality into one `ISubscriptionMap`; a bare filter still admits and takes the broker row's grade, so `local` is expressible per topic and one client publishing and consuming one filter no longer re-reads its own posts.
- Law: a post is a POLICY ROW on the same law — v5 decides grade, retain, message expiry, and the response/correlation pair PER PUBLISH PACKET, so `Mqtt.Post` carries them and `_mqttPublish` folds the row against the broker defaults into one `IClientPublishOptions`; a bare topic string still admits, and `dup` stays foreclosed because the client raises it on its own redelivery and a caller setting it forges a replay marker.
- Law: `_MQTT_GRADES` states each delivery grade's forfeit — v5 carries no broker-side dedup at any grade, so QoS 1 duplicates on redelivery and only QoS 2 removes them at its four-packet cost; a caller reads the row rather than inferring a guarantee from a number.
- Law: `_MQTT_ROW` is the seam's one consumption descriptor and every coordinate reads off it — selection (`fits`, `admit`, `tenancy`, `lifetime`), guarantee (`deliver`, `order`, `settle`), recovery (`replay`, `bound`, `refuse`), and the residual `degrade` no column expresses; a caller comparing this seam against `pubsub#PORT_SHAPE` reads the same column names, and a coordinate restated as prose beside the row forks it.
- Law: `serves` closes the member roster this engine answers, so a caller wanting replay, a positional cursor, or a consumer census reads `pubsub#PORT_SHAPE` rather than bending a topic into one.
- Law: subscription admission is evidence — every `subscribeAsync` grant is inspected, any `qos: 128` refusal fails the typed `grant` rail before a message stream escapes, and the refusal NAMES the filters the broker rejected rather than reporting that some filter failed.
- Law: terminal events carry unequal evidence and `_MQTT_TERMINALS` keeps it — only `error` holds a cause and only `disconnect` holds a v5 reason code, so one nullary handler across all four discards the sole diagnosis the seam receives; `offline` names a client still retrying beneath an ended stream, never a dead transport. Failed subscription or grant admission ends the minted client before the fault escapes; successful acquisition transfers that client to the stream scope. Message and lifecycle listeners share the stream scope; `close`, `error`, `disconnect`, and `offline` terminate the stream once, and release ends the client before detaching the complete listener row.
- Law: Raw frames keep opaque bytes; event callers cross through the selected official/exact codec row, and a raw or structured publish keeps the hop carrier whole because no binary binding claims its User Properties.
- Tests: MQTT JSON/binary SDK singles, Avro asset singles, and generated Protobuf singles encode and decode through their one row under strict admission; Avro covers recursive object/record-array data, absent-data-to-null normalization, `data_base64` exclusion, and opaque-host-object refusal; every framed batch refuses, and cross-format assertions compare event semantics rather than bytes.
- Packages: `mqtt` (`connectAsync`, `handleMessage`, `IClientOptions`, `endAsync`), `cloudevents`, `avsc` (`Type`, `schema` — the host-bound engine), `effect`, `node:buffer`, `@rasm\/contracts` (`CloudEventsAvro`), `@rasm/security` (`MachinePrincipal`), `./client.ts` (`Machine`), and `@rasm/core` (`Carrier`, `Event`, `Fault`, `Format`).

```typescript
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

class _MqttTopic extends Schema.Class<_MqttTopic>('Mqtt/Topic')({
    filter: Schema.NonEmptyString,
    qos: Schema.optionalWith(Schema.Literal(0, 1, 2), { as: 'Option' }),
    local: Schema.optionalWith(Schema.Boolean, { default: () => true }),
    asPublished: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    retained: Schema.optionalWith(Schema.Literal(0, 1, 2), { default: () => 0 }),
}) {}

class _MqttPost extends Schema.Class<_MqttPost>('Mqtt/Post')({
    topic: Schema.NonEmptyString,
    qos: Schema.optionalWith(Schema.Literal(0, 1, 2), { as: 'Option' }),
    retain: Schema.optionalWith(Schema.Boolean, { as: 'Option' }),
    expiry: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { as: 'Option' }),
    respond: Schema.optionalWith(Schema.NonEmptyString, { as: 'Option' }),
    correlate: Schema.optionalWith(Schema.Uint8ArrayFromSelf, { as: 'Option' }),
}) {}

const _MQTT_ROW = {
    fits: '<constrained-sensor-gateway-or-edge-peer:long-lived-session,small-frames,carrier-frame,per-subscription-policy>',
    admit: 'publish',
    tenancy: '<topic-filter-scope>',
    present: '<at-handshake:the CONNECT packet rebuilds from client.options on every dial and carries the principal as username plus a BARE token password,so a rotation costs a re-dial;reconnect() replaces both packet stores,which is why no supervisor forks here and the composition root owns the swap>',
    lifetime: { until: '<retained-until-a-publisher-overwrites;live-until-messageExpiryInterval-elapses>', owner: 'package' },
    serves: { consume: true, event: true, open: true, publish: true },
    deliver: '<qos-0|1|2-per-subscription-and-per-post,no-broker-dedup-at-any-grade;consume-gates-the-ack-on-the-handler-while-open-settles-on-arrival;retry-owner:the-client-session>',
    order: '<per-topic;the-topic-string-IS-the-key-member,so-per-entity-order-costs-one-topic-per-entity>',
    settle: '<OUT:publishAsync-resolves-on-PUBACK-at-qos-1-and-PUBCOMP-at-qos-2,on-write-at-qos-0;IN:handleMessage-callback-releases-both-inbound-acks,so-a-withheld-one-re-offers-on-session-resume>',
    replay: '<none:a-re-drive-resumes-nowhere;clean-false-under-the-row-session-expiry-restores-unacknowledged-qos-1-and-2-state-alone>',
    bound: '<TWO:receiveMaximum-from-row-inflight-caps-what-the-broker-holds-unacknowledged-toward-this-client,and-the-emit-mailbox-suspends-at-its-declared-capacity;the-client-offline-queue-and-packet-stores-stay-genuinely-unbounded>',
    refuse: '<value+event:grants-carry-qos-128,publishAsync-rejects,and-close/error/disconnect/offline-carry-the-rest>',
    degrade: '<no-partition-key,no-origin-coordinate;every-v5-field-drops-silently-under-V311-while-the-publish-still-succeeds;binary-event-publish-forfeits-the-hop-carrier-keys-the-binding-seals,while-structured-rows-preserve-them;batch-frames-refuse;teardown-flushes-in-flight-acknowledgements-under-a-deadline-and-abandons-the-offline-queue-whatever-the-window>',
} as const satisfies Mqtt.Row;

const _MQTT_GRADES = {
    0: { guarantee: 'at-most-once', degrade: '<no-ack,no-redelivery,loss-on-disconnect>' },
    1: { guarantee: 'at-least-once', degrade: '<duplicates-on-redelivery,no-broker-dedup>' },
    2: { guarantee: 'exactly-once', degrade: '<four-packet-handshake-latency>' },
} as const satisfies Record<QoS, { readonly guarantee: string; readonly degrade: string }>;

class _MqttBroker extends Schema.Class<_MqttBroker>('Mqtt/Broker')({
    origin: Schema.URLFromSelf,
    classes: Schema.NonEmptyArray(Event.rasm.classes.schema.pipe(
        Schema.filter((classification) =>
            Event.rasm.classes.at(classification).broker || '<dataclassification-forbidden-on-broker>'),
    )),
    qos: Schema.optionalWith(Schema.Literal(0, 1, 2), { default: () => 1 as QoS }),
    retain: Schema.optionalWith(Schema.Boolean, { default: () => false }),
    keepalive: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 60 }),
    inflight: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 32 }),
    session: Schema.optionalWith(Schema.Duration, { default: () => Duration.hours(1) }),
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
        readonly present: string;
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
    type Selector = string | ReadonlyArray<string | Topic>;
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
    type EventFormat = 'binary' | Format.Event;
    type EventPolicy = {
        readonly trust: (claim: {
            readonly topic: string;
            readonly fact: Schema.Schema.Type<typeof Event.rasm.Fact>;
            readonly classification: Event.Class;
        }) => Effect.Effect<void, MqttFault>;
    };
    type Body = Data.TaggedEnum<{
        Raw: { readonly octets: Uint8Array; readonly media: Option.Option<string> };
        Event: { readonly envelope: CloudEvent<unknown>; readonly format: EventFormat };
    }>;
}

const _MqttBody = Data.taggedEnum<Mqtt.Body>();

const _mqttBand = (packet: IPublishPacket): Mqtt.Band => packet.properties?.userProperties ?? {};
const _mqttUtf8 = { read: new TextDecoder(), write: new TextEncoder() } as const;

const _mqttPublishBand = (band: Carrier.Frame['mqtt']): Mqtt.PublishBand =>
    Record.map(band, (value) => typeof value === 'string' ? value : [...value]);

const _mqttBindingBand = (message: MQTTMessage): Carrier.Frame['mqtt'] =>
    Record.fromEntries(
        Object.entries(message.headers).flatMap(([key, value]) =>
            value === undefined ? [] : [[key, typeof value === 'string' ? value : [...value]] as const]),
    );

const _mqttWithoutHop = (band: Carrier.Frame['mqtt']): Carrier.Frame['mqtt'] =>
    Record.fromEntries(Array.filter(
        Object.entries(band),
        ([name]) => !Array.contains(Carrier.keys, name as (typeof Carrier.keys)[number]),
    ));

const _mqttOctets = (message: MQTTMessage, origin: string): Effect.Effect<Uint8Array, MqttFault> =>
    typeof message.body === 'string'
        ? Effect.succeed(_mqttUtf8.write.encode(message.body))
        : message.body instanceof Uint8Array
          ? Effect.succeed(new Uint8Array(message.body))
          : message.body === undefined
            ? Effect.succeed(new Uint8Array())
            : Effect.fail(new MqttFault({ case: { reason: 'publish', origin, detail: '<binding-body-is-not-octets>' } }));

const _avroType = Type.forSchema(CloudEventsAvro as schema.AvroSchema, { wrapUnions: 'never' });

const _avroEngine = {
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

const _AvroTree = Schema.Struct({
    attribute: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
    data: Schema.Unknown,
});

const _AvroWire = Schema.transformOrFail(Schema.Uint8ArrayFromSelf, _AvroTree, {
    strict: true,
    decode: (octets, _, ast) =>
        Either.match(_avroEngine.read(octets), {
            onLeft: (issue) => ParseResult.fail(new ParseResult.Type(ast, octets, issue)),
            onRight: ParseResult.succeed,
        }),
    encode: (tree, _, ast) =>
        Either.match(_avroEngine.write(tree), {
            onLeft: (issue) => ParseResult.fail(new ParseResult.Type(ast, tree, issue)),
            onRight: ParseResult.succeed,
        }),
});

const _avroObject = (input: unknown): input is Readonly<Record<string, unknown>> =>
    typeof input === 'object'
    && input !== null
    && !globalThis.Array.isArray(input)
    && !(input instanceof Uint8Array)
    && (Object.getPrototypeOf(input) === Object.prototype || Object.getPrototypeOf(input) === null);

const _avroTraverse = <A, B>(
    values: ReadonlyArray<A>,
    project: (value: A) => Either.Either<B, string>,
): Either.Either<ReadonlyArray<B>, string> =>
    Array.reduce(values, Either.right<ReadonlyArray<B>, string>([]), (held, value) =>
        Either.flatMap(held, (rows) => Either.map(project(value), (row) => [...rows, row])));

function _avroRecordEncode(input: unknown): Either.Either<Readonly<Record<string, unknown>>, string> {
    if (!_avroObject(input)) return Either.left('<avro-json-object-required>');
    return Either.map(
        _avroTraverse(Object.entries(input), ([key, value]) =>
            Either.map(_avroNestedEncode(value), (encoded) => [key, encoded] as const)),
        Object.fromEntries,
    );
}

function _avroNestedEncode(input: unknown): Either.Either<unknown, string> {
    if (input === null || typeof input === 'boolean' || typeof input === 'string') return Either.right(input);
    if (typeof input === 'number' && Number.isFinite(input)) return Either.right(input);
    if (_avroObject(input)) return Either.map(_avroRecordEncode(input), (value) => ({ value }));
    if (globalThis.Array.isArray(input)) {
        return _avroTraverse(input, (item) => Either.map(_avroRecordEncode(item), (value) => ({ value })));
    }
    return Either.left('<avro-json-nested-value-unsupported>');
}

function _avroRecordDecode(input: unknown): Either.Either<Readonly<Record<string, unknown>>, string> {
    if (!_avroObject(input)) return Either.left('<avro-json-record-required>');
    return Either.map(
        _avroTraverse(Object.entries(input), ([key, value]) =>
            Either.map(_avroNestedDecode(value), (decoded) => [key, decoded] as const)),
        Object.fromEntries,
    );
}

function _avroNestedDecode(input: unknown): Either.Either<unknown, string> {
    if (globalThis.Array.isArray(input)) {
        return _avroTraverse(input, (item) =>
            !_avroObject(item) || !_avroObject(item.value)
                ? Either.left('<avro-json-array-record-required>')
                : _avroRecordDecode(item.value));
    }
    if (_avroObject(input)) {
        return !_avroObject(input.value)
            ? Either.left('<avro-json-value-record-required>')
            : _avroRecordDecode(input.value);
    }
    return Either.right(input);
}

const _avroDataEncode = (input: unknown): Either.Either<unknown, string> => {
    if (input === undefined) return Either.right(null);
    if (input instanceof Uint8Array || input === null || typeof input === 'boolean' || typeof input === 'string') {
        return Either.right(input);
    }
    if (typeof input === 'number' && Number.isFinite(input)) return Either.right(input);
    if (_avroObject(input)) return _avroRecordEncode(input);
    if (globalThis.Array.isArray(input)) {
        return _avroTraverse(input, (item) => Either.map(_avroRecordEncode(item), (value) => ({ value })));
    }
    return Either.left('<avro-data-unsupported>');
};

const _avroDataDecode = (input: unknown): Either.Either<unknown, string> =>
    globalThis.Array.isArray(input)
        ? _avroTraverse(input, (item) =>
              !_avroObject(item) || !_avroObject(item.value)
                  ? Either.left('<avro-data-array-record-required>')
                  : _avroRecordDecode(item.value))
        : _avroObject(input)
          ? _avroRecordDecode(input)
          : Either.right(input);

const _avroAttribute = (name: string, input: unknown): Either.Either<unknown, string> => {
    if (input === null || typeof input === 'boolean' || typeof input === 'string' || input instanceof Uint8Array) {
        return Either.right(input);
    }
    if (input instanceof Date) return Either.right(input.toISOString());
    if (input instanceof URL) return Either.right(input.href);
    if (typeof input === 'number' && Number.isInteger(input) && input >= -2_147_483_648 && input <= 2_147_483_647) {
        return Either.right(input);
    }
    return Either.left(`<avro-attribute-unsupported:${name}>`);
};

const _avroLower = (envelope: CloudEvent<unknown>): Either.Either<typeof _AvroTree.Encoded, string> =>
    Either.flatMap(
        _avroTraverse(
            Object.entries(envelope).filter(
                ([name, value]) => name !== 'data' && name !== 'data_base64' && value !== undefined,
            ),
            ([name, value]) => Either.map(_avroAttribute(name, value), (encoded) => [name, encoded] as const),
        ),
        (attributes) => Either.map(_avroDataEncode(envelope.data), (data) => ({
            attribute: Object.fromEntries(attributes),
            data,
        })),
    );

const _AvroLift: Schema.Schema<CloudEvent<unknown>, typeof _AvroTree.Encoded> = Schema.transformOrFail(
    _AvroTree,
    Event.schema,
    {
        strict: true,
        decode: ({ attribute, data }, _, ast) =>
            Effect.fromEither(_avroDataDecode(data)).pipe(
                Effect.map((decoded) => ({ ...attribute, data: decoded })),
                Effect.flatMap(Event.admit),
                Effect.mapError((issue) => new ParseResult.Type(ast, { attribute, data }, String(issue))),
            ),
        encode: (envelope, _, ast) =>
            Event.admit(envelope).pipe(
                Effect.flatMap((admitted) => Effect.fromEither(_avroLower(admitted))),
                Effect.mapError((issue) => new ParseResult.Type(ast, envelope, String(issue))),
            ),
    },
);

const _AvroEnvelope = _AvroWire.pipe(Schema.compose(_AvroLift, { strict: false }));
const Avro = Format.event.avro.bind(_AvroEnvelope);

const _eventFault = (origin: string, detail: string): MqttFault =>
    new MqttFault({ case: { reason: 'event', origin, detail } });

const _avroDecoded = (
    frame: Mqtt.Frame,
    origin: string,
): Effect.Effect<Array.NonEmptyReadonlyArray<CloudEvent<unknown>>, MqttFault> =>
    Schema.decodeUnknown(Avro.single)(frame.body).pipe(
        Effect.mapError((issue) => _eventFault(origin, `<avro-rejected:${issue.message}>`)),
        Effect.map(Array.of),
    );

const _mqttMessage = (frame: Mqtt.Frame, structured: boolean): MQTTMessage =>
    MQTTMessageFactory(
        frame.media ?? CONSTANTS.MIME_OCTET_STREAM,
        structured ? frame.band : _mqttWithoutHop(frame.band),
        structured ? _mqttUtf8.read.decode(frame.body) : Buffer.from(frame.body),
    );

const _mqttFraming = (
    frame: Mqtt.Frame,
    origin: string,
): Effect.Effect<Option.Option<Format.Event.Frame>, MqttFault> => {
    const binary = Record.has(frame.band, CONSTANTS.CE_ATTRIBUTES.SPEC_VERSION);
    const structured = Option.flatMap(Option.fromNullable(frame.media), Format.event.framed);
    if (binary && Option.isSome(structured)) {
        return Effect.fail(_eventFault(origin, '<conflicting-binary-and-structured-event-evidence>'));
    }
    if (binary) return Effect.succeedNone;
    return Option.match(structured, {
        onNone: () => Effect.fail(_eventFault(origin, '<not-a-cloudevent-message>')),
        onSome: Effect.succeedSome,
    });
};

const _mqttDecoded = (
    message: MQTTMessage,
    origin: string,
    decode: (message: MQTTMessage) => CloudEventV1<unknown> | ReadonlyArray<CloudEventV1<unknown>>,
): Effect.Effect<Array.NonEmptyReadonlyArray<CloudEvent<unknown>>, MqttFault> =>
    Effect.flatMap(
        Effect.try({
            try: () => decode(message),
            catch: (cause) => _eventFault(origin, `<toevent-rejected:${String(cause)}>`),
        }),
        (decoded) =>
            globalThis.Array.isArray(decoded)
                ? Effect.fail(_eventFault(origin, '<batch-unsupported:mqtt>'))
                : Event.admit(decoded).pipe(
                      Effect.mapError((refusal) => _eventFault(origin, refusal.message)),
                      Effect.map(Array.of),
                  ),
    );

type _MqttProjected = {
    readonly payload: Uint8Array;
    readonly band: Carrier.Frame['mqtt'];
    readonly media: string | undefined;
};

const _mqttBound = (
    bind: (event: CloudEventV1<unknown>) => MQTTMessage,
    envelope: CloudEvent<unknown>,
    origin: string,
): Effect.Effect<_MqttProjected, MqttFault> =>
    Effect.flatMap(
        Effect.try({
            try: () => bind(envelope),
            catch: (cause) =>
                new MqttFault({ case: { reason: 'publish', origin, detail: `<binding-rejected:${String(cause)}>` } }),
        }),
        (message) =>
            Effect.map(_mqttOctets(message, origin), (payload) => ({
                payload,
                band: _mqttBindingBand(message),
                media: message.PUBLISH?.['Content Type'],
            })),
    );

const _mqttEventFrames = {
    binary: (envelope: CloudEvent<unknown>, origin: string) => _mqttBound(MQTT.binary, envelope, origin),
    json: (envelope: CloudEvent<unknown>, origin: string) =>
        Schema.encode(Event.format.json.single)(envelope).pipe(
            Effect.mapError((issue) => new MqttFault({ case: { reason: 'publish', origin, detail: issue.message } })),
            Effect.map((payload): _MqttProjected => ({ payload, band: {}, media: Event.format.json.media })),
        ),
    protobuf: (envelope: CloudEvent<unknown>, origin: string) =>
        Schema.encode(Event.format.protobuf.single)(envelope).pipe(
            Effect.mapError((issue) => new MqttFault({ case: { reason: 'publish', origin, detail: issue.message } })),
            Effect.map((payload): _MqttProjected => ({ payload, band: {}, media: Event.format.protobuf.media })),
        ),
    avro: (envelope: CloudEvent<unknown>, origin: string) =>
        Schema.encode(Avro.single)(envelope).pipe(
            Effect.mapError((issue) => new MqttFault({ case: { reason: 'publish', origin, detail: issue.message } })),
            Effect.map((payload): _MqttProjected => ({ payload, band: {}, media: Avro.media })),
        ),
} as const satisfies Record<Mqtt.EventFormat, (envelope: CloudEvent<unknown>, origin: string) => Effect.Effect<_MqttProjected, MqttFault>>;

const _mqttProjected = (body: Mqtt.Body, broker: Mqtt.Broker): Effect.Effect<_MqttProjected, MqttFault> =>
    _MqttBody.$match(body, {
        Raw: ({ octets, media }) => Effect.succeed({
            payload: new Uint8Array(octets),
            band: {},
            media: Option.match(media, { onNone: () => undefined, onSome: (value) => value }),
        }),
        Event: ({ envelope, format }) => Effect.gen(function* () {
            const origin = broker.origin.href;
            const admitted = yield* Event.admit(envelope).pipe(
                Effect.mapError((refusal) => _eventFault(origin, refusal.message)),
            );
            const { roster } = yield* Event.rasm.read(admitted).pipe(
                Effect.mapError((refusal) => _eventFault(origin, refusal.message)),
            );
            if (roster.dataref !== undefined) {
                return yield* Effect.fail(_eventFault(origin, '<dataref-unsupported:mqtt>'));
            }
            if (Option.isSome(Schema.decodeUnknownOption(Event.rasm.Fact)(admitted))) {
                const classification = yield* Option.match(Option.fromNullable(roster.dataclassification), {
                    onNone: () => Effect.fail(_eventFault(origin, '<dataclassification-required:mqtt>')),
                    onSome: Effect.succeed,
                });
                if (!Array.contains(broker.classes, classification)) {
                    return yield* Effect.fail(_eventFault(origin, `<dataclassification-refused:${classification}>`));
                }
            }
            return yield* _mqttEventFrames[format](admitted, origin);
        }),
    });

const _mqttAdmitted = (
    frame: Mqtt.Frame,
    envelope: CloudEvent<unknown>,
    broker: Mqtt.Broker,
    policy: Mqtt.EventPolicy,
): Effect.Effect<CloudEvent<unknown>, MqttFault> =>
    Effect.gen(function* () {
        const origin = broker.origin.href;
        const fact = yield* Schema.decodeUnknown(Event.rasm.Fact)(envelope, { errors: 'all' }).pipe(
            Effect.mapError((issue) => _eventFault(origin, `<profile-rejected:${issue.message}>`)),
        );
        const { roster } = yield* Event.rasm.read(envelope).pipe(
            Effect.mapError((refusal) => _eventFault(origin, refusal.message)),
        );
        if (roster.dataref !== undefined) yield* Effect.fail(_eventFault(origin, '<dataref-unsupported:mqtt>'));
        const classification = yield* Option.match(Option.fromNullable(roster.dataclassification), {
            onNone: () => Effect.fail(_eventFault(origin, '<dataclassification-required:mqtt>')),
            onSome: Effect.succeed,
        });
        if (!Array.contains(broker.classes, classification)) {
            yield* Effect.fail(_eventFault(origin, `<dataclassification-refused:${classification}>`));
        }
        yield* policy.trust({ topic: frame.topic, fact, classification });
        const carried = Carrier.withoutTenant(Carrier.extract('cloudevents', envelope).context);
        const baggage = Carrier.print.baggage(carried.baggage);
        return yield* Event.clone(
            envelope,
            baggage.length === 0 ? {} : { baggage },
            baggage.length === 0 ? ['baggage'] : [],
        ).pipe(Effect.mapError((refusal) => _eventFault(origin, refusal.message)));
    });

const _mqttEvent = (
    frame: Mqtt.Frame,
    broker: Mqtt.Broker,
    policy: Mqtt.EventPolicy,
): Effect.Effect<Array.NonEmptyReadonlyArray<CloudEvent<unknown>>, MqttFault> =>
    Effect.flatMap(_mqttFraming(frame, broker.origin.href), (framed) =>
        Effect.flatMap(
            Option.match(framed, {
                onNone: () => _mqttDecoded(_mqttMessage(frame, false), broker.origin.href, MQTT.toEvent<unknown>),
                onSome: (framing) =>
                    framing._tag === 'Batch'
                        ? Effect.fail(_eventFault(broker.origin.href, `<batch-unsupported:mqtt:${framing.format}>`))
                        : framing.format === 'avro'
                          ? _avroDecoded(frame, broker.origin.href)
                          : framing.format === 'protobuf'
                            ? Schema.decodeUnknown(Event.format.protobuf.single)(frame.body).pipe(
                                  Effect.mapError((issue) =>
                                      _eventFault(broker.origin.href, `<protobuf-rejected:${issue.message}>`)),
                                  Effect.map(Array.of),
                              )
                            : _mqttDecoded(_mqttMessage(frame, true), broker.origin.href, MQTT.toEvent<unknown>),
            }),
            (events) => Effect.forEach(events, (event) => _mqttAdmitted(frame, event, broker, policy)),
        ));

const _mqttCredential = (broker: Mqtt.Broker): Effect.Effect<Partial<IClientOptions>, MqttFault> =>
    Effect.map(
        Effect.mapError(
            Machine.at('mqtt:dial', broker.origin.origin),
            (lapse) => new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: `<credential-${lapse.case.reason}>` } }),
        ),
        Option.match({
            onNone: () => ({}),
            onSome: (principal: MachinePrincipal) => ({
                username: principal.clientId,
                password: Redacted.value(principal.token),
            }),
        }),
    );

const _mqttConnect = (broker: Mqtt.Broker): Effect.Effect<MqttClient, MqttFault> =>
    Effect.flatMap(_mqttCredential(broker), (credential) =>
        Effect.tryPromise({
            try: () =>
                connectAsync(broker.origin.href, {
                    protocolVersion: 5,
                    keepalive: broker.keepalive,
                    clean: false,
                    properties: {
                        sessionExpiryInterval: Math.floor(Duration.toSeconds(broker.session)),
                        receiveMaximum: broker.inflight,
                    },
                    resubscribe: true,
                    ...credential,
                }),
            catch: (cause) => new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
        }));

const _MQTT_TERMINALS = {
    close: { detail: () => '<broker-closed>' },
    error: { detail: (cause: unknown) => `<broker-error:${String(cause)}>` },
    disconnect: { detail: (packet: unknown) => `<broker-disconnect:${String((packet as IDisconnectPacket)?.reasonCode)}>` },
    offline: { detail: () => '<broker-offline:client-retrying>' },
} as const;

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

const _MQTT_FLUSH = Duration.seconds(5);

const _mqttEnded = (broker: Mqtt.Broker, client: MqttClient): Effect.Effect<void, MqttFault> =>
    Effect.tryPromise({
        try: () => client.endAsync(),
        catch: (cause) => new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
    }).pipe(
        Effect.timeoutTo({
            duration: _MQTT_FLUSH,
            onSuccess: () => Effect.void,
            onTimeout: () =>
                Effect.ignoreLogged(Effect.tryPromise({
                    try: () => client.endAsync(true),
                    catch: (cause) => new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
                })),
        }),
        Effect.flatten,
    );

type _MqttDelivery = {
    readonly frame: Mqtt.Frame;
    readonly settle: (refusal: Option.Option<MqttFault>) => Effect.Effect<void>;
};

const _MQTT_MAILBOX = { bufferSize: 64, strategy: 'suspend' } as const;

const _mqttDelivered = (broker: Mqtt.Broker, topics: Mqtt.Selector): Stream.Stream<_MqttDelivery, MqttFault> =>
    Stream.asyncScoped<_MqttDelivery, MqttFault>((emit) =>
        Effect.acquireRelease(
            Effect.gen(function* () {
                const client = yield* _mqttConnect(broker);
                return yield* Effect.gen(function* () {
                    const grants = yield* Effect.tryPromise({
                        try: () => client.subscribeAsync(_mqttSubscription(broker, topics)),
                        catch: (cause) =>
                            new MqttFault({ case: { reason: 'dial', origin: broker.origin.href, cause: String(cause) } }),
                    });
                    const refused = grants.filter((grant) => grant.qos === 128).map((grant) => grant.topic);
                    yield* Array.match(refused, {
                        onEmpty: () => Effect.void,
                        onNonEmpty: (filters) =>
                            Effect.fail(new MqttFault({ case: { reason: 'grant', origin: broker.origin.href, filters } })),
                    });
                    let terminated = false;
                    client.handleMessage = (packet: IPublishPacket, done: (error?: Error) => void) => {
                        if (terminated) return done();
                        let settled = false;
                        void emit.single({
                            frame: {
                                topic: packet.topic.toString(),
                                body: new Uint8Array(packet.payload),
                                band: _mqttBand(packet),
                                media: packet.properties?.contentType,
                            },
                            settle: (refusal) =>
                                Effect.sync(() => {
                                    if (settled) return;
                                    settled = true;
                                    done(Option.getOrUndefined(refusal));
                                }),
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
                    for (const [event, listener] of Object.entries(terminals)) client.on(event as Mqtt.Terminal, listener);
                    return { client, terminals };
                }).pipe(
                    Effect.tapError(() => Effect.orDie(_mqttEnded(broker, client))),
                );
            }),
            (live) =>
                Effect.orDie(
                    Effect.ensuring(
                        _mqttEnded(broker, live.client),
                        Effect.sync(() => {
                            for (const [event, listener] of Object.entries(live.terminals)) {
                                live.client.off(event as Mqtt.Terminal, listener);
                            }
                        }),
                    ),
                ),
        ),
        _MQTT_MAILBOX,
    );

class Mqtt extends Context.Tag('runtime/Mqtt')<
    Mqtt,
    {
        readonly consume: (
            topics: Mqtt.Selector,
            handler: (frame: Mqtt.Frame, carrier: Carrier.Extraction) => Effect.Effect<void, MqttFault>,
        ) => Effect.Effect<void, MqttFault>;
        readonly event: (
            frame: Mqtt.Frame,
            policy: Mqtt.EventPolicy,
        ) => Effect.Effect<Array.NonEmptyReadonlyArray<CloudEvent<unknown>>, MqttFault>;
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
    static readonly Body = _MqttBody;
    static readonly Post = _MqttPost;
    static readonly Topic = _MqttTopic;
    static readonly grades = _MQTT_GRADES;
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
                        Stream.runForEach(_mqttDelivered(broker, topics), ({ frame, settle }) =>
                            Effect.flatMap(
                                Effect.either(pipe(Carrier.extract('mqtt', frame.band), (extracted) => handler(frame, {
                                    ...extracted,
                                    context: Carrier.withoutTenant(extracted.context),
                                }))),
                                Either.match({
                                    onLeft: (refusal) => Effect.zipRight(settle(Option.some(refusal)), Effect.fail(refusal)),
                                    onRight: () => settle(Option.none()),
                                }),
                            ),
                        ),
                    event: (frame, policy) => _mqttEvent(frame, broker, policy),
                    open: (topics) =>
                        Stream.mapEffect(_mqttDelivered(broker, topics), ({ frame, settle }) =>
                            Effect.as(settle(Option.none()), frame)),
                    publish: (target, body, band = {}, context = Option.none()) =>
                        Effect.flatMap(
                            _mqttProjected(body, broker),
                            ({ payload, band: bindingBand, media }) => {
                                const hopBand = Option.match(context, {
                                    onNone: () => band,
                                    onSome: (hop) => Carrier.inject('mqtt', hop, band),
                                })
                                const collision = Array.findFirst(Carrier.keys, (name) =>
                                    Record.has(bindingBand, name) && Record.has(hopBand, name))
                                if (Option.isSome(collision)) {
                                    return Effect.fail(new MqttFault({
                                        case: {
                                            reason: 'publish',
                                            origin: broker.origin.href,
                                            detail: `<binary-event-collides-with-hop:${collision.value}>`,
                                        },
                                    }))
                                }
                                return pipe(
                                    _mqttPublish(
                                        broker,
                                        target,
                                        media,
                                        _mqttPublishBand({ ...bindingBand, ...hopBand }),
                                    ),
                                    ({ topic, options }) =>
                                        Effect.tryPromise({
                                            try: () => publisher.publishAsync(topic, Buffer.from(payload), options),
                                            catch: (cause) =>
                                                new MqttFault({
                                                    case: { reason: 'publish', origin: broker.origin.href, detail: String(cause) },
                                                }),
                                        }),
                                ).pipe(Effect.asVoid)
                            },
                        ),
                }),
            ),
        );
}
```

```typescript
// --- [EXPORTS] -------------------------------------------------------------------------

export { Avro, Duplex, Feed, FeedFault, Mqtt, MqttFault };
```

## [05]-[RESEARCH]

(none)
