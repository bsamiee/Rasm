# [RUNTIME_PUBSUB]

Fanout and replay are one port with engines as rows. `Fanout` broadcasts evidence mirrors, presence, cross-process invalidation, and large-binary handoff through an in-process `PubSub` row, a browser `BroadcastChannel` row, or a NATS `jetstream` row. Every replay row is a `Fanout.Replayed` pair carrying the envelope and its engine-minted resume coordinate.

Guarantee rows declare at-least-once handler consumption with ack-after-success, poison termination, and heartbeats; deduplicated publish under a content-derived `msgID`; optional double-ack confirmation; and sequence- or instant-anchored replay without an ack surface. JetStream upholds the full ledger, lighter engines expose their degradation, and root Layer selection chooses the engine.

Retention makes replay a warm-up and recovery window, never the system of record; consumers needing full history read the data journal. Deployment owns NATS fsync and replica quorum. JetStream ships on `./server`, local stays runtime-neutral, and tab stays browser-bound. Module: `runtime/src/net/pubsub.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                               | [PUBLIC]                     |
| :-----: | :-------------- | :----------------------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `TOPIC_ROWS`    | the topic policy vocabulary: subject, retention, replay, ack posture, redelivery     | `Fanout`                     |
|  [02]   | `PORT_SHAPE`    | the engine-neutral port — publish, subscribe, consume, replay, stash, haul — faults  | `Fanout`, `FanoutFault`      |
|  [03]   | `LOCAL_ROW`     | the in-process engine over `PubSub` replay and the in-process blob shelf            | `Fanout.local`               |
|  [04]   | `TAB_ROW`       | the browser cross-tab engine — `BroadcastChannel` bridge decorating the local cells  | `Fanout.tab`                 |
|  [05]   | `JETSTREAM_ROW` | the NATS engine: ordered vs durable lanes, dedup, double-ack, heartbeat, blob store  | `Fanout.jetstream`, `Broker` |
|  [06]   | `KAFKA_ROW`     | Kafka engine: librdkafka client, manual-commit lane, explicit degradation, reconcile | `Fanout.kafka`               |

## [02]-[TOPIC_ROWS]

[TOPIC_ROWS]:
- Owner: `Fanout.Topic` — the `Schema.Class` policy authority carrying `subject` (the wire address), `retention` (the stream age bound — the never-system-of-record ceiling), `replay` (the non-negative warm-up window a late subscriber receives), `ack` (`"fire" | "double"` — whether consumption confirms the acknowledgement itself), `wait` (the ack deadline the durable lane declares as `ack_wait` and halves into the long-handler heartbeat cadence), and positive `attempts` (the redelivery ceiling declared as `max_deliver` — beyond it the server parks the message); the dedup window reads `Setting.fanout.dedup` and the blob chunk size `Setting.fanout.chunk`, so topic policy decodes once as root data and the engine reads admitted rows, never knobs.
- Law: the publish identity is content-derived — the producer mints the `key` from the kernel content-key mint or the envelope's `Hlc` stamp, so a replayed publish inside the dedup window is recognized by value identity in every language; the engine never invents identity.
- Law: the envelope is transport-only — opaque octets, the identity key, an optional closed optimistic expectation (`LastMessage | Stream | LastSequence | LastSubjectSequence | SubjectSequence`), and the `band` header record, defaulted empty, that every engine projects into its native message headers (the transport frame widened, the payload shape untouched); the consumer's own Schema decodes the body at its seam (event stamps ride the payload vocabulary), while the engine projects the expectation into its publish primitive and the `Fanout.Receipt` / `Fanout.Stowed` `Schema.Class` authorities bind returned position, duplicate, extent, and digest evidence without parallel DTOs.
- Law: the W3C carrier rides the band, never the payload — each publish seam reads `Propagation.current` and injects its exact core dialect, each handler-backed consume lane extracts the matching row and continues through `Propagation.ingress`, and ordered lanes surface the band for the consumer's handling fold. `fanout` names the transport-neutral in-memory envelope row; NATS and Kafka never masquerade as it or each other.
- Growth: a new fanout concern is one topic row; a new guarantee axis is one row column every engine answers.
- Packages: `effect` (`Schema`, `Duration`), `../proc/config.ts` (`Setting`).

```typescript signature
import {
    Chunk,
    Context,
    Data,
    DateTime,
    Duration,
    Effect,
    HashMap,
    Layer,
    Match,
    Option,
    PubSub,
    Record,
    Ref,
    Schedule,
    Schema,
    type Scope,
    Stream,
} from 'effect';
import { type ConnectionOptions, type MsgHdrs, type NatsConnection, headers as natsHeaders, wsconnect } from '@nats-io/nats-core';
import { KafkaJS } from '@confluentinc/kafka-javascript';
import { Propagation } from '../otel/emit.ts';
import {
    AckPolicy,
    type Consumer,
    type ConsumerMessages,
    DeliverPolicy,
    type JsMsg,
    JetStreamApiCodes,
    JetStreamApiError,
    jetstream,
    jetstreamManager,
    type StreamInfo,
} from '@nats-io/jetstream';
import { Objm, type ObjectStore } from '@nats-io/obj';
import { Carrier, type FaultClass } from '@rasm/ts/core';
import { Setting } from '../proc/config.ts';
import { Breaker } from './client.ts';

class Envelope extends Schema.Class<Envelope>('Envelope')({
    key: Schema.NonEmptyString,
    body: Schema.Uint8ArrayFromSelf,
    band: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { default: () => ({}) }),
    expect: Schema.optionalWith(
        Schema.Union(
            Schema.TaggedStruct('LastMessage', { id: Schema.NonEmptyString }),
            Schema.TaggedStruct('Stream', { name: Schema.NonEmptyString }),
            Schema.TaggedStruct('LastSequence', { sequence: Schema.NonNegativeInt }),
            Schema.TaggedStruct('LastSubjectSequence', { sequence: Schema.NonNegativeInt }),
            Schema.TaggedStruct('SubjectSequence', { subject: Schema.NonEmptyString, sequence: Schema.NonNegativeInt }),
        ),
        { as: 'Option' },
    ),
}) {}

class _Topic extends Schema.Class<_Topic>('Fanout/Topic')({
    subject: Schema.NonEmptyString,
    retention: Schema.Duration,
    replay: Schema.NonNegativeInt,
    ack: Schema.Literal('fire', 'double'),
    wait: Schema.Duration,
    attempts: Schema.Int.pipe(Schema.positive()),
}) {}

const _ReceiptPosition = Schema.Union(
    Schema.TaggedStruct('Sequence', { seq: Schema.NonNegativeInt }),
    Schema.TaggedStruct('PartitionOffset', { partition: Schema.NonNegativeInt, offset: Schema.NonEmptyString }),
);

class _Replayed extends Schema.Class<_Replayed>('Fanout/Replayed')({
    envelope: Envelope,
    coordinate: _ReceiptPosition,
}) {}

class _Receipt extends Schema.Class<_Receipt>('Fanout/Receipt')({
    topic: Schema.NonEmptyString,
    subject: Schema.NonEmptyString,
    key: Schema.NonEmptyString,
    position: _ReceiptPosition,
    duplicate: Schema.Boolean,
}) {}

class _Stowed extends Schema.Class<_Stowed>('Fanout/Stowed')({
    size: Schema.NonNegativeInt,
    digest: Schema.optionalWith(Schema.NonEmptyString, { as: 'Option' }),
}) {}

class _LocalPolicy extends Schema.Class<_LocalPolicy>('Fanout/LocalPolicy')({
    capacity: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 256 }),
    shelf: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 33_554_432 }),
}) {}

declare namespace Fanout {
    type Ack = 'fire' | 'double';
    type Topic = _Topic;
    type Topics = Readonly<Record<string, Topic>>;
    type Anchor = Data.TaggedEnum<{
        Window: {};
        Sequence: { readonly seq: number };
        Instant: { readonly at: DateTime.Utc };
    }>;
    type ReceiptPosition = typeof _ReceiptPosition.Type;
    type Replayed = _Replayed;
    type Receipt = _Receipt;
    type Stowed = _Stowed;
    type LocalPolicy = _LocalPolicy;
}

const _Anchor = Data.taggedEnum<Fanout.Anchor>();
const _blobKey = Schema.encodeSync(Schema.parseJson(Schema.Tuple(Schema.String, Schema.String)));

type _KafkaHeader = Uint8Array | string | ReadonlyArray<Uint8Array | string> | undefined;

const _kafkaFrame = (headers: Readonly<Record<string, _KafkaHeader>>): Carrier.Frame['kafka'] =>
    Record.fromEntries(
        Object.entries(headers).flatMap(([key, value]) => {
            const head = Array.isArray(value) ? value[0] : value;
            return head === undefined
                ? []
                : [[key, typeof head === 'string' ? new TextEncoder().encode(head) : new Uint8Array(head)] as const];
        }),
    );

const _kafkaBand = (band: Readonly<Record<string, string>>): Readonly<Record<string, Buffer>> =>
    Record.map(band, (value) => Buffer.from(value));

const _fanoutEnvelope = (envelope: Envelope): Effect.Effect<Envelope> =>
    Effect.map(
        Propagation.current,
        (context) => new Envelope({ ...envelope, band: Carrier.inject('fanout', context, envelope.band) }),
    );

const _named = (topics: Fanout.Topics, topic: string): Effect.Effect<Fanout.Topic, FanoutFault> =>
    Option.match(Option.fromNullable(topics[topic]), {
        onNone: () => Effect.fail(new FanoutFault({ reason: 'horizon', topic })),
        onSome: Effect.succeed,
    });
```

## [03]-[PORT_SHAPE]

[PORT_SHAPE]:
- Owner: the `Fanout` Tag — six members over the topic key. This envelope lane: `publish(topic, envelope)` yields the evidence receipt whose position is either a stream sequence or a partition-offset coordinate; `subscribe(topic)` is the live fanout stream with the topic's replay window warming a late attach; `consume(topic, consumer, anchor, handler)` is the at-least-once lane whose explicit consumer identity derives a distinct durable name, preventing independent logical subscribers from accidentally load-balancing one durable consumer; `replay(topic, anchor)` re-reads within retention as `Fanout.Replayed`, pairing each envelope with its engine-minted coordinate. This blob lane streams transient large-binary handoff through `stash` / `haul`, never a second content-addressing vocabulary or durable store.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `horizon` (the anchor precedes the engine's window, the topic is undeclared, or the blob is absent — class `absent`), `publish` (an unacknowledged publish or a rejected stash, class `unavailable`), `poison` (the handler proves an envelope unprocessable, class `malformed` — the consume lane's terminate signal) — so the core budget gate re-drives the transient rows and a horizon miss routes as the terminal evidence it is.
- Law: delivery semantics are the row's, not the call site's — a consumer never re-states ack posture, replay depth, retention, or redelivery ceiling; it names the topic and the engine answers the row; an unknown topic answers `horizon` identically on every engine and every member.
- Law: the port is engine-blind — no member names NATS, and swapping any row for another edits the root merge and nothing else; the engine roster law is the services doctrine's, instantiated here.
- Entry: `yield* Fanout` then the six members; engines land as `Fanout.local(topics)` / `Fanout.tab(topics)` / `Fanout.jetstream(topics)` / `Fanout.kafka(topics)` root Layers.
- Packages: `effect` (`Context`, `Data`, `Stream`).

```typescript signature
class FanoutFault extends Data.TaggedError('FanoutFault')<{
    readonly reason: 'dial' | 'horizon' | 'publish' | 'poison';
    readonly topic: string;
}> {
    get class(): FaultClass.Kind {
        return this.reason === 'horizon' ? 'absent' : this.reason === 'poison' ? 'malformed' : 'unavailable';
    }
}

class Fanout extends Context.Tag('runtime/Fanout')<
    Fanout,
    {
        readonly publish: (topic: string, envelope: Envelope) => Effect.Effect<Fanout.Receipt, FanoutFault>;
        readonly subscribe: (topic: string) => Stream.Stream<Envelope, FanoutFault>;
        readonly consume: (
            topic: string,
            consumer: string,
            anchor: Fanout.Anchor,
            handler: (envelope: Envelope) => Effect.Effect<void, FanoutFault>,
        ) => Effect.Effect<void, FanoutFault>;
        readonly replay: (topic: string, anchor: Fanout.Anchor) => Stream.Stream<Fanout.Replayed, FanoutFault>;
        readonly stash: (topic: string, name: string, body: Stream.Stream<Uint8Array, FanoutFault>) => Effect.Effect<Fanout.Stowed, FanoutFault>;
        readonly haul: (topic: string, name: string) => Stream.Stream<Uint8Array, FanoutFault>;
    }
>() {
    static readonly Anchor = _Anchor;
    static readonly Envelope = Envelope;
    static readonly Topic = _Topic;
    static readonly ReceiptPosition = _ReceiptPosition;
    static readonly Replayed = _Replayed;
    static readonly Receipt = _Receipt;
    static readonly Stowed = _Stowed;
    static readonly LocalPolicy = _LocalPolicy;
    static readonly local = (topics: Fanout.Topics, policy: Fanout.LocalPolicy = new _LocalPolicy({})): Layer.Layer<Fanout> => _local(topics, policy);
    static readonly tab = (topics: Fanout.Topics, policy: Fanout.LocalPolicy = new _LocalPolicy({})): Layer.Layer<Fanout> => _tab(topics, policy);
    static readonly jetstream = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting | Broker> => _jetstream(topics);
    static readonly kafka = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting> => _kafka(topics);
}
```

## [04]-[LOCAL_ROW]

[LOCAL_ROW]:
- Owner: `Fanout.local(topics, policy)` — one scoped `PubSub.bounded<Fanout.Replayed>({ capacity: policy.capacity, replay: row.replay })` per topic row and one `Ref`-held aggregate blob shelf; `publish` mints the topic sequence before offering the envelope/coordinate pair and returns `publish` fault when `PubSub.publish` rejects it (a local publish is never a duplicate — the dedup window is a server guarantee the local row honestly lacks), `subscribe` projects envelopes from the scoped replay rows, `consume` folds the same envelope projection through the handler with the ack posture vacuous (in-process delivery has no redelivery to confirm or terminate), `replay` snapshots the current sequence count and returns exactly the retained warm-up pairs for the `Window` anchor, while a `Sequence` or `Instant` anchor folds to `horizon`; the blob lane folds into the shelf under `policy.shelf`, replaces a held key without double-counting its prior body, and streams out keyed `topic/name`, digest honestly absent.
- Law: the degradation is the table read aloud — replay-is-a-bounded warm-up, no cross-process reach, no dedup window, no durable redelivery, no digest evidence, and the blob shelf is process memory under the admitted `Fanout.LocalPolicy.shelf` aggregate byte ceiling: a stash crossing it refuses with the `publish` fault, never exhausts memory, so the general large-binary contract is the jetstream row's; a proof or a single-process deployment selects this row deliberately, and promoting a workload that needs the missing rows is a root Layer swap, never a local re-implementation.
- Law: capacity backpressures — the bounded construction suspends a producer ahead of the slowest subscriber's window; a sliding local topic is a row decision, never a default.
- Packages: `effect` (`PubSub`, `Stream`, `Layer`, `Record`, `Ref`, `HashMap`, `Chunk`).

```typescript signature
type _Port = Context.Tag.Service<Fanout>;
type _LocalPort = _Port & { readonly offer: _Port['publish'] };

const _minted = (topics: Fanout.Topics, policy: Fanout.LocalPolicy): Effect.Effect<_LocalPort, never, Scope.Scope> =>
    Effect.gen(function* () {
        const cells = yield* Effect.all(
            Record.map(topics, (row) => PubSub.bounded<Fanout.Replayed>({ capacity: policy.capacity, replay: row.replay })),
            { concurrency: 'inherit' },
        );
        const shelf = yield* Ref.make({
            size: 0,
            bodies: HashMap.empty<string, { readonly size: number; readonly chunks: Chunk.Chunk<Uint8Array> }>(),
        });
        const seqs = yield* Ref.make(HashMap.empty<string, number>());
        const held = (topic: string): Effect.Effect<PubSub.PubSub<Fanout.Replayed>, FanoutFault> =>
            Option.match(Option.fromNullable(cells[topic]), {
                onNone: () => Effect.fail(new FanoutFault({ reason: 'horizon', topic })),
                onSome: Effect.succeed,
            });
        const offer: _Port['publish'] = (topic, envelope) =>
            Effect.flatMap(held(topic), (cell) =>
                Effect.flatMap(
                    Ref.modify(seqs, (counts) => {
                        const next = Option.getOrElse(HashMap.get(counts, topic), () => 0) + 1;
                        return [next, HashMap.set(counts, topic, next)] as const;
                    }),
                    (seq) =>
                        Effect.flatMap(
                            PubSub.publish(
                                cell,
                                new _Replayed({ envelope, coordinate: { _tag: 'Sequence', seq } }),
                            ),
                            (delivered) =>
                                delivered
                                    ? Effect.succeed(
                                          new _Receipt({
                                              topic,
                                              subject: topics[topic]?.subject ?? topic,
                                              key: envelope.key,
                                              position: { _tag: 'Sequence', seq },
                                              duplicate: false,
                                          }),
                                      )
                                    : Effect.fail(new FanoutFault({ reason: 'publish', topic })),
                        ),
                ),
            );
        return {
            offer,
            publish: (topic, envelope) => Effect.flatMap(_fanoutEnvelope(envelope), (stamped) => offer(topic, stamped)),
            subscribe: (topic) =>
                Stream.unwrap(Effect.map(held(topic), (cell) => Stream.map(Stream.fromPubSub(cell), (row) => row.envelope))),
            consume: (topic, _consumer, anchor, handler) =>
                _Anchor.$is('Window')(anchor)
                    ? Stream.runForEach(
                          Stream.unwrap(Effect.map(held(topic), (cell) => Stream.map(Stream.fromPubSub(cell), (row) => row.envelope))),
                          (envelope) => Propagation.ingress(handler(envelope), Carrier.extract('fanout', envelope.band)),
                      )
                    : Effect.fail(new FanoutFault({ reason: 'horizon', topic })),
            replay: (topic, anchor) =>
                _Anchor.$is('Window')(anchor)
                    ? Stream.unwrap(
                          Effect.all({ cell: held(topic), counts: Ref.get(seqs) }).pipe(
                              Effect.map(({ cell, counts }) =>
                                  Stream.take(
                                      Stream.fromPubSub(cell),
                                      Math.min(
                                          topics[topic]?.replay ?? 0,
                                          Option.getOrElse(HashMap.get(counts, topic), () => 0),
                                      ),
                                  ),
                              ),
                          ),
                      )
                    : Stream.fail(new FanoutFault({ reason: 'horizon', topic })),
            stash: (topic, name, body) =>
                Effect.zipRight(
                    held(topic),
                    Effect.flatMap(
                        Stream.runFoldEffect(body, { size: 0, chunks: Chunk.empty<Uint8Array>() }, (acc, part) =>
                            acc.size + part.byteLength > policy.shelf
                                ? Effect.fail(new FanoutFault({ reason: 'publish', topic })) // the ceiling is typed evidence: an over-bound stash refuses instead of exhausting memory
                                : Effect.succeed({ size: acc.size + part.byteLength, chunks: Chunk.append(acc.chunks, part) }),
                        ),
                        (folded) =>
                            Effect.flatMap(
                                Ref.modify(shelf, (held) => {
                                    const key = _blobKey([topic, name]);
                                    const prior = Option.getOrElse(HashMap.get(held.bodies, key), () => ({
                                        size: 0,
                                        chunks: Chunk.empty<Uint8Array>(),
                                    }));
                                    const size = held.size - prior.size + folded.size;
                                    return size > policy.shelf
                                        ? ([Option.none<Fanout.Stowed>(), held] as const)
                                        : ([
                                              Option.some(new _Stowed({ size: folded.size, digest: Option.none<string>() })),
                                              { size, bodies: HashMap.set(held.bodies, key, folded) },
                                          ] as const);
                                }),
                                Option.match({
                                    onNone: () => Effect.fail(new FanoutFault({ reason: 'publish', topic })),
                                    onSome: Effect.succeed,
                                }),
                            ),
                    ),
                ),
            haul: (topic, name) =>
                Stream.unwrap(
                    Effect.map(Ref.get(shelf), (kept) =>
                        Option.match(HashMap.get(kept.bodies, _blobKey([topic, name])), {
                            onNone: () => Stream.fail(new FanoutFault({ reason: 'horizon', topic })),
                            onSome: (body) => Stream.fromChunk(body.chunks),
                        }),
                    ),
                ),
        };
    });

const _local = (topics: Fanout.Topics, policy: Fanout.LocalPolicy): Layer.Layer<Fanout> => Layer.scoped(Fanout, _minted(topics, policy));
```

## [05]-[TAB_ROW]

[TAB_ROW]:
- Owner: `Fanout.tab(topics)` — the browser cross-tab engine: the local cells decorated with one `BroadcastChannel` per topic row keyed by the row's `subject`. `publish` offers locally then posts the encoded envelope, an arriving post decodes through the `Envelope` schema and offers into the same cells, and every other member is the local row's — so same-tab and cross-tab subscribers read one replay window and the engine is one decoration, never a second implementation.
- Law: the channel loop is structural — `BroadcastChannel` never delivers a post to the posting context, so re-offering an arrival cannot echo; a malformed arrival drops at the decode seam, never poisons a cell.
- Law: the degradation extends the local row's — no cross-process reach beyond the origin's tabs, no dedup, no durable redelivery, and the blob shelf stays per-tab (a cross-tab `haul` of another tab's stash answers `horizon`); a browser workload needing the durable rows dials the jetstream row over websockets instead.
- Boundary: the session plane's own `BroadcastChannel` (`browser/route` `Vault`) is a distinct, single-purpose channel — session continuity is not fanout, and neither surface composes the other.
- Packages: `effect` (`Stream`, `Schema`, `Record`), the host `BroadcastChannel` Web API at the sanctioned FFI seam.

```typescript signature
const _tab = (topics: Fanout.Topics, policy: Fanout.LocalPolicy): Layer.Layer<Fanout> =>
    Layer.scoped(
        Fanout,
        Effect.gen(function* () {
            const inner = yield* _minted(topics, policy);
            const posts = yield* Effect.all(
                Record.map(topics, (row) =>
                    Effect.acquireRelease(
                        Effect.sync(() => new BroadcastChannel(row.subject)),
                        (channel) => Effect.sync(() => channel.close()),
                    ),
                ),
                { concurrency: 'inherit' },
            );
            yield* Effect.forEach(
                Record.toEntries(posts),
                ([topic, channel]) =>
                    Stream.runForEach(
                        Stream.asyncPush<unknown>((emit) =>
                            Effect.acquireRelease(
                                Effect.sync(() => {
                                    const listener = (event: MessageEvent) => emit.single(event.data);
                                    channel.addEventListener('message', listener);
                                    return listener;
                                }),
                                (listener) => Effect.sync(() => channel.removeEventListener('message', listener)),
                            ),
                        ),
                        (data) => Effect.ignore(Effect.flatMap(Schema.decodeUnknown(Envelope)(data), (envelope) => inner.offer(topic, envelope))),
                    ).pipe(Effect.forkScoped),
                { concurrency: 'inherit', discard: true },
            );
            return {
                ...inner,
                publish: (topic, envelope) =>
                    Effect.flatMap(_fanoutEnvelope(envelope), (stamped) =>
                        Effect.tap(inner.offer(topic, stamped), () =>
                            Effect.sync(() => posts[topic]?.postMessage(Schema.encodeSync(Envelope)(stamped)))),
                    ),
            };
        }),
    );
```

## [06]-[JETSTREAM_ROW]

[JETSTREAM_ROW]:
- Owner: `Fanout.jetstream(topics)` — the NATS engine. This connection is capability: the exported `Broker` Tag holds the one scoped dial against `Setting.fanout.origin` — the runtime row's `nats` TCP/TLS binding (`exec#RUNTIME_ROWS`) on the server lanes, the `wsconnect` default on the browser lane — drained on scope close, and the one connection fans into the stream lanes, the object store, and the sibling coordination engine (`coordinate#KV_ROW`) — a second dial beside `Broker.live` is the named defect. Construction reconciles the substrate: `jetstreamManager(nc)` inspects each topic stream, adds an absent stream, and updates a present stream's mutable retention, subject, and dedup policy; `Objm.create` creates or opens the blob store. Restart is therefore convergence, never a duplicate-create failure, while the server's own durability posture (fsync interval, replicas) stays a deployment fact.
- Law: the consumer lanes are split by ack capability — the ordered lane (`subscribe`, `replay`) mints a nameless ordered consumer fixed to `AckPolicy.None`; the durable lane (`consume`) derives `durable_name` from topic and the caller's logical consumer identity, declares explicit ack posture, and binds that same name. Independent consumers therefore receive independent durable streams, while replicas sharing one identity intentionally load-balance.
- Law: exactly-once publish is the dedup window under the circuit — `js.publish` carries the content-derived `msgID` and the envelope's optional expectation projected through `_expected`; the server recognizes a replay inside `duplicate_window`, enforces every `StreamExpectations` arm, and returns a `PubAck` whose sequence and `duplicate` flag ride a receipt bound to the addressed topic, subject, and key; the whole publish rides `Breaker.guard` so a dead broker sheds fast instead of hammering.
- Law: at-least-once is the full ack algebra — the handler runs under a heartbeat race that stamps `msg.working()` every half `ack_wait` so a long handler never triggers spurious redelivery; success acks — `ack()` on `"fire"` rows, `ackAck()` awaited on `"double"` rows so the acknowledgement itself is confirmed; a `poison` handler fault terminates through `msg.term(reason)`, every other handler fault `nak()`s, and only that ruled handler-failure branch returns `Effect.void`; an `ackAck()` rejection remains a `dial` fault on the consume rail and cannot be mistaken for handled work.
- Law: replay is bounded honesty — `replay(topic, anchor)` snapshots `StreamInfo.state`, rejects a `Sequence` before `first_seq` or an `Instant` before `first_ts`, computes a bounded retained-message count instead of using the absolute head sequence as `max_messages`, opens the ordered lane at the admitted anchor (`Window` → the row's replay-depth sequence, `Sequence(seq)` → `opt_start_seq` under `DeliverPolicy.StartSequence`, `Instant(at)` → `opt_start_time` under `DeliverPolicy.StartTime`), and returns each envelope with its `JsMsg.seq` coordinate before `Consumer.fetch` and `Stream.takeUntil` terminate exactly at that head; `subscribe(topic)` projects envelopes from the same ordered rows, warms from the replay-depth sequence, and then tails through `Consumer.consume`. This durable `consume` anchor is creation policy: an existing named consumer resumes its server-held position and never silently re-anchors.
- Law: the blob lane is the object store — `stash` bridges the effect stream through `Stream.toReadableStream` into `os.put({ name, options: { max_chunk_size } }, body)` with the chunk size from `Setting.fanout.chunk`, the `ObjectInfo` digest riding the `Stowed` receipt as wire-integrity evidence (never a second content identity — `ContentKey` remains the one addressing vocabulary); `haul` lifts `os.get(name)` back through `Stream.fromReadableStream` with the deferred `error` promise joined into the failure channel, so a mid-stream chunk fault is a typed failure, never a silent truncation.
- Law: the iterator seam is the platform-forced boundary — `consume()` yields an async iterable the engine lifts through `Stream.fromAsyncIterable` under a scoped acquisition whose release closes the consumer, so teardown rides the `Scope` and a leaked pull loop is unspellable.
- Boundary: NATS server deployment — the websocket listener, fsync `sync_interval` hardening, replica quorum — is the deploy plane's; the data journal remains the system of record, and a projection rebuilt from fanout evidence is the named defect.
- Packages: `@nats-io/nats-core` (`wsconnect`, `NatsConnection`), `@nats-io/jetstream` (`jetstream`, `jetstreamManager`, `AckPolicy`, `DeliverPolicy`), `@nats-io/obj` (`Objm`, `ObjectStore`), `effect` (`Effect`, `Layer`, `Stream`, `Duration`, `Schedule`), `../proc/config.ts` (`Setting`), `./client.ts` (`Breaker`).

```typescript signature
const _nanos = (span: Duration.Duration): number => Duration.toMillis(span) * 1_000_000;

const _BLOB = { store: 'fanout' } as const;
const _CIRCUIT = { trip: 8, cool: Duration.seconds(20), probes: 1 } as const;

const _expected = (expect: Envelope['expect']) =>
    Option.match(expect, {
        onNone: () => ({}),
        onSome: Match.valueTags({
            LastMessage: ({ id }) => ({ expect: { lastMsgID: id } }),
            Stream: ({ name }) => ({ expect: { streamName: name } }),
            LastSequence: ({ sequence }) => ({ expect: { lastSequence: sequence } }),
            LastSubjectSequence: ({ sequence }) => ({ expect: { lastSubjectSequence: sequence } }),
            SubjectSequence: ({ subject, sequence }) => ({ expect: { lastSubjectSequence: sequence, lastSubjectSequenceSubject: subject } }),
        }),
    });

const _hdrs = (band: Readonly<Record<string, string>>): MsgHdrs => {
    // BOUNDARY ADAPTER: MsgHdrs mint over the NATS header FFI — the draft detaches at the return
    const minted = natsHeaders();
    for (const [key, value] of Object.entries(band)) {
        minted.set(key, value);
    }
    return minted;
};

const _unband = (hdrs: MsgHdrs | undefined): Readonly<Record<string, string>> => {
    // BOUNDARY ADAPTER: MsgHdrs read — get answers '' for an absent key, so only inhabited pairs survive
    if (hdrs === undefined) return {};
    const band: Record<string, string> = {};
    for (const key of hdrs.keys()) {
        const value = hdrs.get(key);
        if (value !== '') band[key] = value;
    }
    return band;
};

const _start = (
    anchor: Fanout.Anchor,
): { readonly deliver_policy: DeliverPolicy; readonly opt_start_seq?: number; readonly opt_start_time?: string } =>
    _Anchor.$match(anchor, {
        Window: () => ({ deliver_policy: DeliverPolicy.New }),
        Sequence: ({ seq }) => ({ deliver_policy: DeliverPolicy.StartSequence, opt_start_seq: seq }),
        Instant: ({ at }) => ({ deliver_policy: DeliverPolicy.StartTime, opt_start_time: DateTime.formatIso(at) }),
    });

const _within = (topic: string, anchor: Fanout.Anchor, info: StreamInfo): Effect.Effect<void, FanoutFault> =>
    info.state.messages === 0
        ? Effect.void
        : _Anchor.$match(anchor, {
              Window: () => Effect.void,
              Sequence: ({ seq }) => (seq < info.state.first_seq ? Effect.fail(new FanoutFault({ reason: 'horizon', topic })) : Effect.void),
              Instant: ({ at }) =>
                  Option.match(DateTime.make(info.state.first_ts), {
                      onNone: () => Effect.fail(new FanoutFault({ reason: 'dial', topic })),
                      onSome: (first) => (DateTime.lessThan(at, first) ? Effect.fail(new FanoutFault({ reason: 'horizon', topic })) : Effect.void),
                  }),
          });

const _remaining = (anchor: Fanout.Anchor, info: StreamInfo, replay: number): number =>
    _Anchor.$match(anchor, {
        Window: () => Math.min(replay, info.state.messages),
        Sequence: ({ seq }) => Math.max(0, info.state.last_seq - Math.max(seq, info.state.first_seq) + 1),
        Instant: () => info.state.messages,
    });

class Broker extends Context.Tag('runtime/Broker')<Broker, NatsConnection>() {
    // This dial is the runtime row's binding: the node/bun root passes Runtime.<row>.nats, the browser root keeps the wsconnect default
    static readonly live = (dial: (opts?: ConnectionOptions) => Promise<NatsConnection> = wsconnect): Layer.Layer<Broker, FanoutFault, Setting> =>
        Layer.scoped(
            Broker,
            Effect.flatMap(Setting, (setting) =>
                Effect.acquireRelease(
                    Effect.tryPromise({
                        try: () => dial({ servers: setting.fanout.origin.href }),
                        catch: () => new FanoutFault({ reason: 'dial', topic: '*' }),
                    }),
                    (live) => Effect.orDie(Effect.tryPromise(() => live.drain())),
                ),
            ),
        );
}

const _jetstream = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting | Broker> =>
    Layer.scoped(
        Fanout,
        Effect.gen(function* () {
            const setting = yield* Setting;
            const nc = yield* Broker;
            const js = jetstream(nc);
            const jsm = yield* Effect.tryPromise({
                try: () => jetstreamManager(nc),
                catch: () => new FanoutFault({ reason: 'dial', topic: '*' }),
            });
            yield* Effect.forEach(
                Record.toEntries(topics),
                ([name, row]) =>
                    Effect.gen(function* () {
                        const config = {
                            subjects: [row.subject],
                            max_age: _nanos(row.retention),
                            duplicate_window: _nanos(setting.fanout.dedup),
                        };
                        const current = yield* Effect.tryPromise({
                            try: () => jsm.streams.info(name),
                            catch: (cause) => cause,
                        }).pipe(
                            Effect.matchEffect({
                                onFailure: (cause) =>
                                    cause instanceof JetStreamApiError && cause.code === JetStreamApiCodes.StreamNotFound
                                        ? Effect.succeed(Option.none())
                                        : Effect.fail(new FanoutFault({ reason: 'dial', topic: name })),
                                onSuccess: (info) => Effect.succeed(Option.some(info)),
                            }),
                        );
                        yield* Option.match(current, {
                            onNone: () =>
                                Effect.tryPromise({
                                    try: () => jsm.streams.add({ name, ...config }),
                                    catch: () => new FanoutFault({ reason: 'dial', topic: name }),
                                }),
                            onSome: () =>
                                Effect.tryPromise({
                                    try: () => jsm.streams.update(name, config),
                                    catch: () => new FanoutFault({ reason: 'dial', topic: name }),
                                }),
                        });
                    }),
                { concurrency: 'inherit', discard: true },
            );
            const store: ObjectStore = yield* Effect.tryPromise({
                try: () => new Objm(nc).create(_BLOB.store),
                catch: () => new FanoutFault({ reason: 'dial', topic: _BLOB.store }),
            });

            const named = (topic: string): Effect.Effect<Fanout.Topic, FanoutFault> => _named(topics, topic);

            const pulled = (
                topic: string,
                minted: Effect.Effect<Consumer, FanoutFault>,
                pull: (consumer: Consumer) => Promise<ConsumerMessages> = (consumer) => consumer.consume(),
            ): Stream.Stream<readonly [Envelope, JsMsg], FanoutFault> =>
                Stream.unwrapScoped(
                    Effect.gen(function* () {
                        const consumer = yield* minted;
                        const messages = yield* Effect.acquireRelease(
                            Effect.tryPromise({
                                try: () => pull(consumer),
                                catch: () => new FanoutFault({ reason: 'dial', topic }),
                            }),
                            (live) => Effect.orDie(Effect.tryPromise(() => live.close())),
                        );
                        return Stream.fromAsyncIterable(messages, () => new FanoutFault({ reason: 'dial', topic })).pipe(
                            Stream.map(
                                (msg: JsMsg) =>
                                    [
                                        new Envelope({
                                            key: msg.headers?.get('Nats-Msg-Id') ?? String(msg.seq),
                                            body: msg.data,
                                            band: _unband(msg.headers), // the arriving header band: the consume lane's carrier, the ordered lanes' surfaced continuation material
                                        }),
                                        msg,
                                    ] as const,
                            ),
                        );
                    }),
                );

            const ordered = (
                topic: string,
                anchor: Fanout.Anchor,
                bound: Option.Option<{ readonly head: number; readonly limit: number; readonly wait: Duration.Duration }> = Option.none(),
            ): Stream.Stream<Fanout.Replayed, FanoutFault> => {
                const source = pulled(
                    topic,
                    Effect.zipRight(
                        named(topic),
                        Effect.tryPromise({
                            try: () => js.consumers.get(topic, _start(anchor)),
                            catch: () => new FanoutFault({ reason: 'horizon', topic }),
                        }),
                    ),
                    Option.match(bound, {
                        onNone: () => (consumer: Consumer) => consumer.consume(),
                        onSome:
                            ({ limit, wait }) =>
                            (consumer: Consumer) =>
                                consumer.fetch({ max_messages: Math.max(1, limit), expires: Math.max(1_000, Duration.toMillis(wait)) }),
                    }),
                );
                return Stream.map(
                    Option.match(bound, {
                        onNone: () => source,
                        onSome: ({ head, limit }) => (limit === 0 ? Stream.empty : Stream.takeUntil(source, ([, msg]) => msg.seq >= head)),
                    }),
                    ([envelope, msg]) =>
                        new _Replayed({
                            envelope,
                            coordinate: { _tag: 'Sequence', seq: msg.seq },
                        }),
                );
            };

            const subscribed = (topic: string): Stream.Stream<Envelope, FanoutFault> =>
                Stream.unwrap(
                    Effect.flatMap(named(topic), (row) =>
                        Effect.flatMap(
                            Effect.tryPromise({
                                try: () => jsm.streams.info(topic),
                                catch: () => new FanoutFault({ reason: 'dial', topic }),
                            }),
                            (info) =>
                                Stream.map(
                                    ordered(topic, _Anchor.Sequence({ seq: Math.max(1, info.state.last_seq - row.replay + 1) })),
                                    (replayed) => replayed.envelope,
                                ),
                        ),
                    ),
                );

            const durable = (topic: string, consumer: string, row: Fanout.Topic, anchor: Fanout.Anchor) =>
                Effect.gen(function* () {
                    const durable = `${topic}:${consumer}`;
                    const current = yield* Effect.tryPromise({
                        try: () => jsm.consumers.info(topic, durable),
                        catch: (cause) => cause,
                    }).pipe(
                        Effect.matchEffect({
                            onFailure: (cause) =>
                                cause instanceof JetStreamApiError && cause.code === JetStreamApiCodes.ConsumerNotFound
                                    ? Effect.succeed(Option.none())
                                    : Effect.fail(new FanoutFault({ reason: 'dial', topic })),
                            onSuccess: (info) => Effect.succeed(Option.some(info)),
                        }),
                    );
                    yield* Option.match(current, {
                        onNone: () =>
                            Effect.tryPromise({
                                try: () =>
                                    jsm.consumers.add(topic, {
                                        durable_name: durable,
                                        ack_policy: AckPolicy.Explicit,
                                        ack_wait: _nanos(row.wait),
                                        max_deliver: row.attempts,
                                        ..._start(anchor),
                                    }),
                                catch: () => new FanoutFault({ reason: 'dial', topic }),
                            }),
                        onSome: () =>
                            Effect.tryPromise({
                                try: () => jsm.consumers.update(topic, durable, { ack_wait: _nanos(row.wait), max_deliver: row.attempts }),
                                catch: () => new FanoutFault({ reason: 'dial', topic }),
                            }),
                    });
                    return yield* Effect.tryPromise({
                        try: () => js.consumers.get(topic, durable),
                        catch: () => new FanoutFault({ reason: 'dial', topic }),
                    });
                });

            const replayed = (topic: string, anchor: Fanout.Anchor): Stream.Stream<Fanout.Replayed, FanoutFault> =>
                Stream.unwrap(
                    Effect.flatMap(named(topic), (row) =>
                        Effect.flatMap(
                            Effect.tryPromise({
                                try: () => jsm.streams.info(topic),
                                catch: () => new FanoutFault({ reason: 'dial', topic }),
                            }),
                            (info) =>
                                Effect.as(
                                    _within(topic, anchor, info),
                                    ordered(
                                        topic,
                                        _Anchor.$is('Window')(anchor)
                                            ? _Anchor.Sequence({ seq: Math.max(info.state.first_seq, info.state.last_seq - row.replay + 1) })
                                            : anchor,
                                        Option.some({ head: info.state.last_seq, limit: _remaining(anchor, info, row.replay), wait: row.wait }),
                                    ),
                                ),
                        ),
                    ),
                );

            return {
                publish: (topic, envelope) =>
                    Effect.flatMap(Effect.all({ context: Propagation.current, row: named(topic) }), ({ context, row }) =>
                        Breaker.guard(
                            'fanout:publish',
                            _CIRCUIT,
                        )(
                            Effect.tryPromise({
                                try: () =>
                                    js.publish(row.subject, envelope.body, {
                                        msgID: envelope.key,
                                        headers: _hdrs(Carrier.inject('nats', context, envelope.band)),
                                        ..._expected(envelope.expect),
                                    }),
                                catch: () => new FanoutFault({ reason: 'publish', topic }),
                            }),
                        ).pipe(
                            Effect.mapError((fault) => (fault._tag === 'Lapse' ? new FanoutFault({ reason: 'publish', topic }) : fault)),
                            Effect.map(
                                (ack) =>
                                    new _Receipt({
                                        topic,
                                        subject: row.subject,
                                        key: envelope.key,
                                        position: { _tag: 'Sequence', seq: ack.seq },
                                        duplicate: ack.duplicate,
                                    }),
                            ),
                        ),
                    ),
                subscribe: subscribed,
                consume: (topic, consumer, anchor, handler) =>
                    Effect.flatMap(named(topic), (row) =>
                        Stream.runForEach(pulled(topic, durable(topic, consumer, row, anchor)), ([envelope, msg]) =>
                            Effect.matchEffect(
                                Effect.raceFirst(
                                    // first COMPLETION wins: the heartbeat never settles, so the handler's success or failure always decides and the beat dies with it
                                    Propagation.ingress(handler(envelope), Carrier.extract('nats', envelope.band)), // the handler continues the publisher's parent: the NATS row decodes once, the shared scrub included
                                    Effect.repeat(
                                        Effect.sync(() => msg.working()),
                                        Schedule.spaced(Duration.times(row.wait, 0.5)),
                                    ),
                                ),
                                {
                                    onFailure: (fault) => Effect.sync(() => (fault.reason === 'poison' ? msg.term(fault.reason) : msg.nak())),
                                    onSuccess: () =>
                                        row.ack === 'double'
                                            ? Effect.flatMap(
                                                  Effect.tryPromise({
                                                      try: () => msg.ackAck(),
                                                      catch: () => new FanoutFault({ reason: 'dial', topic }),
                                                  }),
                                                  (confirmed) =>
                                                      confirmed
                                                          ? Effect.void
                                                          : Effect.fail(new FanoutFault({ reason: 'dial', topic })),
                                              )
                                            : Effect.sync(() => msg.ack()),
                                },
                            ),
                        ),
                    ),
                replay: replayed,
                stash: (topic, name, body) =>
                    Effect.zipRight(
                        named(topic),
                        Effect.map(
                            Effect.tryPromise({
                                try: () =>
                                    store.put(
                                        { name: _blobKey([topic, name]), options: { max_chunk_size: setting.fanout.chunk } },
                                        Stream.toReadableStream(body),
                                    ),
                                catch: () => new FanoutFault({ reason: 'publish', topic }),
                            }),
                            (info) => new _Stowed({ size: info.size, digest: Option.some(info.digest) }),
                        ),
                    ),
                haul: (topic, name) =>
                    Stream.unwrap(
                        Effect.map(
                            Effect.tryPromise({
                                try: () => store.get(_blobKey([topic, name])),
                                catch: () => new FanoutFault({ reason: 'dial', topic }),
                            }),
                            (result) =>
                                result === null
                                    ? Stream.fail(new FanoutFault({ reason: 'horizon', topic }))
                                    : Stream.fromReadableStream({
                                          evaluate: () => result.data,
                                          onError: () => new FanoutFault({ reason: 'dial', topic }),
                                      }).pipe(
                                          Stream.concat(
                                              Stream.drain(
                                                  Stream.fromEffect(
                                                      Effect.flatMap(
                                                          Effect.tryPromise({
                                                              try: () => result.error,
                                                              catch: () => new FanoutFault({ reason: 'dial', topic }),
                                                          }),
                                                          (fault) =>
                                                              fault === null ? Effect.void : Effect.fail(new FanoutFault({ reason: 'dial', topic })),
                                                      ),
                                                  ),
                                              ),
                                          ),
                                      ),
                        ),
                    ),
            };
        }),
    );

```

## [07]-[KAFKA_ROW]

[KAFKA_ROW]:
- Owner: `Fanout.kafka(topics)` — the Kafka engine over the librdkafka promise surface, the TypeScript counterpart of the C# host's `Confluent.Kafka` on the shared broker plane. This client is one scoped `new KafkaJS.Kafka(config)` from `Setting.fanout.brokers` — an empty broker list is the config row's unarmed contract, so construction refuses `dial` before any client mints — its `producer()`/`admin()` acquired at construction under `connect`/`disconnect` brackets; boot reconciles the substrate through `admin.createTopics` exactly as the jetstream row reconciles streams, so restart is convergence.
- Law: the guarantee ledger reads honestly per column — at-least-once holds on the sequential manual-commit lane (`consumer.run({ eachBatch })` with auto-resolve disabled): each record retries under the topic row, resolution and commit follow handler success, and exhaustion stops the run before a higher offset. Kafka keys select partitions, never deduplicate, so every receipt answers `duplicate: false` with its exact partition-offset position; positional consume anchors, ordered replay, warm subscription, and blob carriage answer `horizon` because `Fanout.Anchor` carries no partition coordinate and this row carries no object store. `Window` alone selects the unpositioned consumer-group flow.
- Law: the consumer identity derives exactly as the durable lane's — `groupId` is `${topic}:${consumer}`, so independent logical subscribers hold independent groups and replicas sharing one identity load-balance; `subscribe({ topic: row.subject })` precedes `run`, and the handler continues any caller-supplied parent through `Carrier.extract('kafka', ...)` and `Propagation.ingress`.
- Law: the header band is the message's `headers` record both directions — publish projects caller-supplied string values to broker bytes, ingress normalizes repeated broker values once and extracts through the Kafka row, and no NATS dialect impersonates Kafka; causal injection remains blocked in `[08]-[RESEARCH]`.
- Boundary: broker deployment — partitions, replication, retention, SASL/TLS posture — is the deploy plane's; the bootstrap roster and security rows are `Setting` rows, and no broker literal exists in the engine.
- Packages: `@confluentinc/kafka-javascript` (`KafkaJS.Kafka`, producer/consumer/admin promise surface), `effect` (`Effect`, `Layer`, `Stream`), `../proc/config.ts` (`Setting`).

```typescript signature
const _kafka = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting> =>
    Layer.scoped(
        Fanout,
        Effect.gen(function* () {
            const setting = yield* Setting;
            // empty brokers is the unarmed row (Setting.fanout.brokers contract): selection refuses at
            // layer construction, before any client mints or dials
            yield* setting.fanout.brokers.length === 0
                ? Effect.fail(new FanoutFault({ reason: 'dial', topic: '*' }))
                : Effect.void;
            const kafka = new KafkaJS.Kafka({ kafkaJS: { brokers: [...setting.fanout.brokers] } });
            const producer = yield* Effect.acquireRelease(
                Effect.tryPromise({
                    try: async () => {
                        const minted = kafka.producer();
                        await minted.connect();
                        return minted;
                    },
                    catch: () => new FanoutFault({ reason: 'dial', topic: '*' }),
                }),
                (live) => Effect.orDie(Effect.tryPromise(() => live.disconnect())),
            );
            const admin = yield* Effect.acquireRelease(
                Effect.tryPromise({
                    try: async () => {
                        const minted = kafka.admin();
                        await minted.connect();
                        return minted;
                    },
                    catch: () => new FanoutFault({ reason: 'dial', topic: '*' }),
                }),
                (live) => Effect.orDie(Effect.tryPromise(() => live.disconnect())),
            );
            yield* Effect.tryPromise({
                // reconcile: topic convergence at boot, the jetstream stream-add law on the Kafka substrate
                try: () => admin.createTopics({ topics: Record.values(topics).map((row) => ({ topic: row.subject })) }),
                catch: () => new FanoutFault({ reason: 'dial', topic: '*' }),
            });

            const named = (topic: string): Effect.Effect<Fanout.Topic, FanoutFault> => _named(topics, topic);

            const consumed = (
                topic: string,
                group: string,
                handler: (envelope: Envelope) => Effect.Effect<void, FanoutFault>,
            ): Effect.Effect<void, FanoutFault> =>
                Effect.flatMap(named(topic), (row) =>
                    Effect.acquireUseRelease(
                        Effect.tryPromise({
                            try: async () => {
                                const minted = kafka.consumer({ kafkaJS: { groupId: group, autoCommit: false } });
                                await minted.connect();
                                await minted.subscribe({ topic: row.subject });
                                return minted;
                            },
                            catch: () => new FanoutFault({ reason: 'dial', topic }),
                        }),
                        (consumer) =>
                            Effect.async<void, FanoutFault>((resume) => {
                                void consumer
                                    .run({
                                        eachBatchAutoResolve: false,
                                        partitionsConsumedConcurrently: 1,
                                        eachBatch: async ({ batch, heartbeat, isRunning, isStale, resolveOffset }) => {
                                            for (const message of batch.messages) {
                                                if (!isRunning() || isStale()) return;
                                                const frame = _kafkaFrame(message.headers ?? {});
                                                const envelope = new Envelope({
                                                    key: message.key?.toString() ?? `${batch.partition}:${message.offset}`,
                                                    body: new Uint8Array(message.value ?? new Uint8Array()),
                                                    band: Record.map(frame, (value) => new TextDecoder().decode(value)),
                                                });
                                                await Effect.runPromise(
                                                    Propagation.ingress(handler(envelope), Carrier.extract('kafka', frame)).pipe(
                                                        Effect.retry(
                                                            Schedule.intersect(
                                                                Schedule.recurs(row.attempts - 1),
                                                                Schedule.spaced(row.wait),
                                                            ),
                                                        ),
                                                    ),
                                                );
                                                resolveOffset(message.offset);
                                                await consumer.commitOffsets([
                                                    {
                                                        topic: batch.topic,
                                                        partition: batch.partition,
                                                        offset: (BigInt(message.offset) + 1n).toString(),
                                                    },
                                                ]);
                                                await heartbeat();
                                            }
                                        },
                                    })
                                    .catch(() => resume(Effect.fail(new FanoutFault({ reason: 'dial', topic }))));
                            }),
                        (consumer) => Effect.orDie(Effect.tryPromise(() => consumer.disconnect())),
                    ),
                );

            return {
                publish: (topic, envelope) =>
                    Effect.flatMap(Effect.all({ context: Propagation.current, row: named(topic) }), ({ context, row }) =>
                        Effect.flatMap(
                            Effect.tryPromise({
                                try: () =>
                                    producer.send({
                                        topic: row.subject,
                                        messages: [{
                                            key: envelope.key,
                                            value: Buffer.from(envelope.body),
                                            headers: Carrier.inject('kafka', context, _kafkaBand(envelope.band)),
                                        }],
                                    }),
                                catch: () => new FanoutFault({ reason: 'publish', topic }),
                            }),
                            (metadata) => {
                                const landed = metadata[0];
                                return landed?.offset === undefined
                                    ? Effect.fail(new FanoutFault({ reason: 'publish', topic }))
                                    : Effect.succeed(
                                          new _Receipt({
                                              topic,
                                              subject: row.subject,
                                              key: envelope.key,
                                              position: {
                                                  _tag: 'PartitionOffset',
                                                  partition: landed.partition,
                                                  offset: landed.offset,
                                              },
                                              duplicate: false, // honest column: Kafka keys route partitions, never deduplicate
                                          }),
                                      );
                            },
                        ),
                    ),
                subscribe: (topic) => Stream.fail(new FanoutFault({ reason: 'horizon', topic })), // the ordered warm-up lane is the jetstream row's; Kafka fanout attaches through consume groups
                consume: (topic, consumer, anchor, handler) =>
                    _Anchor.$is('Window')(anchor)
                        ? consumed(topic, `${topic}:${consumer}`, handler)
                        : Effect.fail(new FanoutFault({ reason: 'horizon', topic })),
                replay: (topic) => Stream.fail(new FanoutFault({ reason: 'horizon', topic })),
                stash: (topic) => Effect.fail(new FanoutFault({ reason: 'horizon', topic })), // no object store on this substrate: the blob contract is the jetstream row's
                haul: (topic) => Stream.fail(new FanoutFault({ reason: 'horizon', topic })),
            };
        }),
    );
```

```typescript signature
// --- [EXPORTS] --------------------------------------------------------------------------

export { Broker, Envelope, Fanout, FanoutFault };
```

## [08]-[RESEARCH]

- [KAFKA_PARTITION_REPLAY]-[BLOCKED]: which partition-bearing anchor extends `Fanout.Anchor` without inventing a broker-local sequence and how `consumer.seek(topicPartitionOffset)` yields a bounded replay stream rather than mutating a durable consume group; route through `libs/typescript/runtime/.api/confluentinc-kafka-javascript.md` `[03]-[ENTRYPOINTS]` row `[06]`; arm when the port owns a partition coordinate and both `consume` and `replay` can honor it.
