# [RUNTIME_PUBSUB]

Fanout and replay are one port with engines as rows. `Fanout` broadcasts evidence mirrors, presence, cross-process invalidation, and large-binary handoff through an in-process `PubSub` row, a browser `BroadcastChannel` row, or a NATS `jetstream` row. Every replay row is a `Fanout.Replayed` pair carrying the announcement and its engine-minted resume coordinate.

Guarantee rows declare at-least-once handler consumption with ack-after-success, poison termination, and heartbeats; deduplicated publish under an address-derived `msgID`; optional double-ack confirmation; and sequence- or instant-anchored replay without an ack surface.

Retention makes replay a warm-up and recovery window, never the system of record; consumers needing full history read the data journal. Every broker connection authenticates as the security plane's machine principal projected at its own dial coordinate — `ConnectionOptions.authenticator` on NATS, the SASL/OAUTHBEARER provider on Kafka — so a fleet worker carries no static token and the `present` column states what a rotation costs each row. Deployment owns NATS fsync and replica quorum. JetStream ships on `./server`, local stays runtime-neutral, and tab stays browser-bound. Module: `runtime/src/net/pubsub.ts`.

## [01]-[INDEX]

- [02]-[TOPIC_ROWS]: `Fanout.Topic` — subject, retention, replay, ack posture, redelivery, both in-flight bounds; the announcement admission.
- [03]-[PORT_SHAPE]: `Fanout` — publish, atomic, subscribe, consume, consumers, replay, stash, alias, haul, pulse; `FanoutFault`.
- [04]-[LOCAL_ROW]: `Fanout.local` — the in-process engine over `PubSub` replay and the in-process blob shelf.
- [05]-[TAB_ROW]: `Fanout.tab` — a `BroadcastChannel` bridge decorating the local cells for cross-tab delivery.
- [06]-[JETSTREAM_ROW]: `Fanout.jetstream` and `Broker` — ordered vs durable lanes, dedup, atomic stage, two status surfaces, blob store.
- [07]-[KAFKA_ROW]: `Fanout.kafka` — librdkafka client, manual-commit lane, explicit degradation, reconcile.

## [02]-[TOPIC_ROWS]

[TOPIC_ROWS]:
- Owner: `Fanout.Topic` — the `Schema.Class` policy authority carrying `subject` (the wire address), `retention` (the stream age bound — the never-system-of-record ceiling, reaching only a row whose `_ENGINES` lifetime owner is `package`), `replay` (the non-negative warm-up window a late subscriber receives), `ack` (`"fire" | "double"` — whether consumption confirms the acknowledgement itself), `wait` (the ack deadline the durable lane declares as `ack_wait` and halves into the long-handler heartbeat cadence), positive `attempts` (the redelivery ceiling declared as `max_deliver` — beyond it the server parks the message), positive `pending` (the unacked in-flight ceiling declared as `max_ack_pending`, the descriptor `bound` coordinate a server-redelivering engine answers), positive `prefetch` (the CLIENT-side pull buffer declared as `max_messages`, the second half of that same `bound` coordinate), and `budget` (the `core/value/fault#RETRY_BUDGET` ledger row an engine with no server-side redelivery compiles its in-process pulse from, defaulting `lease`); the dedup window reads `Setting.fanout.dedup` and the blob chunk size `Setting.fanout.chunk`, so topic policy decodes once as root data and the engine reads admitted rows, never knobs.
- Law: Broker redelivery uses row `wait` and `attempts`; process redelivery uses `Fault.Budget.schedule(row.budget)`.
- Law: the in-flight window is TWO bounds and the row states both — `pending` caps what the server holds unacknowledged, `prefetch` caps what the client already pulled into this process. Naming only the server half leaves the pull buffer at the client's own default, and every message sitting in it burns `ack_wait` behind the one the handler heartbeats, so redelivery arrives for work already in hand and reads as broker instability.
- Law: publish identity DERIVES from the announcement and the engine invents none. JetStream consumes core `Event.address`, the one length-framed digest of the typed `(source,id)` pair, as its bounded `msgID`; payload-content identity never substitutes, because two peers writing identical data are two facts rather than one replay.
- Law: this port carries ONE envelope and mints none — the announced fact IS `interchange/carrier`'s message envelope, so the binding derives transport key and header band while the selected serde frames payload bytes under `Fanout.Contract`; `dataschema` remains the event's payload-schema URI and never aliases registry identity. Any second envelope class beside it is the drift defect this collapse deletes, and optimistic expectation rides `Fanout.Post` where publish policy belongs.
- Law: two traces ship and neither folds onto the other — the CREATION-time context rides the roster extensions the mint sealed, while the HOP context rides each transport frame through its exact core dialect. Kafka's `ce_` attribute band and NATS's structured JSON body stay disjoint from the bare carrier keys beside them; `fanout` names the cross-tab hop row, and no dialect masquerades as another.
- Growth: a new fanout concern is one topic row; a new guarantee axis is one row column every engine answers.
- Packages: `effect`, `@rasm/core` (`Fault.Budget`), and `../proc/config.ts`.

```typescript signature
import {
    Array,
    Cause,
    Chunk,
    Context,
    Data,
    DateTime,
    Duration,
    Effect,
    Exit,
    HashMap,
    HashSet,
    Layer,
    Match,
    Option,
    type ParseResult,
    Predicate,
    PubSub,
    Queue,
    Record,
    MutableRef,
    Redacted,
    Ref,
    Schedule,
    Schema,
    type Scope,
    Stream,
} from 'effect';
import {
    type Authenticator,
    type ConnectionOptions,
    type MsgHdrs,
    type NatsConnection,
    type Status,
    headers as natsHeaders,
    tokenAuthenticator,
    wsconnect,
} from '@nats-io/nats-core';
import { KafkaJS } from '@confluentinc/kafka-javascript';
import { type CloudEvent, CONSTANTS, Kafka, V1 } from 'cloudevents';
import { Buffer } from 'node:buffer';
import { Propagation } from '../otel/emit.ts';
import {
    AckPolicy,
    type Consumer,
    type ConsumerMessages,
    type ConsumerNotification,
    DeliverPolicy,
    type JsMsg,
    JetStreamApiCodes,
    JetStreamApiError,
    jetstream,
    jetstreamManager,
    type StreamInfo,
} from '@nats-io/jetstream';
import { Objm, type ObjectStore } from '@nats-io/obj';
import {
    AvroDeserializer,
    AvroSerializer,
    Compatibility,
    JsonDeserializer,
    JsonSerializer,
    ProtobufDeserializer,
    ProtobufSerializer,
    RuleRegistry,
    SchemaId,
    SchemaRegistryClient,
    SerdeType,
    type Deserializer,
    type DeserializerConfig,
    type SchemaInfo,
    type Serializer,
    type SerializerConfig,
} from '@confluentinc/schemaregistry';
import { Carrier, Event, Fault, Format } from '@rasm/core';
import type { Backend } from '@rasm/data';
import type { MachinePrincipal } from '@rasm/security';
import { Setting } from '../proc/config.ts';
import { Breaker, Machine } from './client.ts';

// One sample type feeds the derived arbitrary below: generation needs a grammar-lawful `type`, and the roster's own
// pattern is what makes an invented literal fail the very admission the generator exists to exercise.
const _SAMPLE_TYPE = 'rasm.fanout.probe.sampled.v1';

// The public port adds generation and rendering metadata to the core semantic schema; it owns no second predicate or
// transform over the CloudEvents envelope.
const _Announced = Event.schema.annotations({
    identifier: 'Fanout/CloudEvent',
    arbitrary: () => (fc) =>
        fc
            .record({
                id: fc.uuid(),
                source: fc.webUrl(),
                time: fc.date({ noInvalidDate: true }).map((at) => at.toISOString()),
            })
            .map((addressed) => Effect.runSync(Event.mint({
                ...addressed,
                specversion: V1,
                type: _SAMPLE_TYPE,
                data: null,
            }))),
    pretty: () => (event) => event.toString(),
});

// Publish-time expectations are POLICY on the CALL, never a field of the announced fact: one fact republished
// under a different expectation stays one fact, so this row rides beside the envelope exactly as `Mqtt.Post` rides
// beside a body, and an engine reads it where it reads its own topic row.
class _Post extends Schema.Class<_Post>('Fanout/Post')({
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

// Transport identity DERIVES from the announcement and no engine invents one beside it: `partitionkey` is the member
// declared by the roster as what a transport partitions on, falling back to the operation identity where none was
// none, and `(source, id)` is the specification's OWN uniqueness composite — which is why a dedup window keys on the
// pair rather than on a content digest, where two peers writing identical payloads are two facts, not a replay.
const _key = (event: CloudEvent<unknown>): Effect.Effect<string, Event.Refusal> =>
    Effect.map(Event.rasm.at(event, 'partitionkey'), (partition) => Option.getOrElse(partition, () => event.id));

class _Topic extends Schema.Class<_Topic>('Fanout/Topic')({
    subject: Schema.NonEmptyString,
    retention: Schema.Duration,
    replay: Schema.NonNegativeInt,
    ack: Schema.Literal('fire', 'double'),
    wait: Schema.Duration,
    attempts: Schema.Int.pipe(Schema.positive()),
    // unacked ceiling: without it the server default decides the in-flight window and the row's `bound` cell names a number nobody set
    pending: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 1_000 }),
    // CLIENT-side pull buffer, the second in-flight bound beside `pending`: a puller asks the server for this many
    // messages at a time, every one of them starts its `ack_wait` clock at DELIVERY, and a sequential handler
    // heartbeats the head alone — so `prefetch * handler-span` must stay under `wait` or the tail redelivers work
    // already in hand. Unset, the client picks 100 and the row's `bound` cell names a number nobody set.
    prefetch: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 8 }),
    // ledger row an engine re-driving in process compiles its pulse from; server-redelivering engines read `wait`/`attempts` instead
    budget: Schema.optionalWith(Schema.Literal(...Fault.Budget.kinds), { default: () => 'lease' as const }),
}) {}

const _ReceiptPosition = Schema.Union(
    Schema.TaggedStruct('Sequence', { seq: Schema.NonNegativeInt }),
    Schema.TaggedStruct('PartitionOffset', { partition: Schema.NonNegativeInt, offset: Schema.NonEmptyString }),
);

class _Replayed extends Schema.Class<_Replayed>('Fanout/Replayed')({
    event: _Announced,
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
    key: Schema.NonEmptyString, // the store key the stash minted: an alias links off this receipt, never a re-derived join
    size: Schema.NonNegativeInt,
    digest: Schema.optionalWith(Schema.NonEmptyString, { as: 'Option' }),
}) {}

class _Consumer extends Schema.Class<_Consumer>('Fanout/Consumer')({
    name: Schema.NonEmptyString,
    created: Schema.DateTimeUtc,
    delivered: Schema.NonNegativeInt,
    pending: Schema.NonNegativeInt,
    unacked: Schema.NonNegativeInt,
    redelivered: Schema.NonNegativeInt,
}) {}

class _LocalPolicy extends Schema.Class<_LocalPolicy>('Fanout/LocalPolicy')({
    capacity: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 256 }),
    shelf: Schema.optionalWith(Schema.Int.pipe(Schema.positive()), { default: () => 33_554_432 }),
}) {}

declare namespace Fanout {
    type Ack = 'fire' | 'double';
    type Topic = _Topic;
    type Topics = Readonly<Record<string, Topic>>;
    type Announced = CloudEvent<unknown>;
    type Post = _Post;
    type Anchor = Data.TaggedEnum<{
        Window: {};
        Sequence: { readonly seq: number };
        Instant: { readonly at: DateTime.Utc };
    }>;
    type ReceiptPosition = typeof _ReceiptPosition.Type;
    type Replayed = _Replayed;
    type Receipt = _Receipt;
    type Stowed = _Stowed;
    type Consumer = _Consumer;
    type LocalPolicy = _LocalPolicy;
}

const _Anchor = Data.taggedEnum<Fanout.Anchor>();
const _blobKey = Schema.encodeSync(Schema.parseJson(Schema.Tuple(Schema.String, Schema.String)));

type _KafkaPair = readonly [
    new (client: SchemaRegistryClient, role: SerdeType, config: SerializerConfig, rules?: RuleRegistry) => Serializer,
    new (client: SchemaRegistryClient, role: SerdeType, config: DeserializerConfig, rules?: RuleRegistry) => Deserializer,
];

const _KAFKA_CODECS = {
    // Three registry families share one ctor arity, so a contract family is a lookup keyed by its own `schemaType`
    AVRO: [AvroSerializer, AvroDeserializer],
    JSON: [JsonSerializer, JsonDeserializer],
    PROTOBUF: [ProtobufSerializer, ProtobufDeserializer],
} as const satisfies Record<string, _KafkaPair>;

type _KafkaFamily = keyof typeof _KAFKA_CODECS;

type _KafkaContract = {
    readonly artifact: Option.Option<string>;
    readonly subject: string;
    readonly id: number;
    readonly version: number;
    readonly compatibility: Compatibility;
    readonly schema: SchemaInfo & { readonly schemaType: _KafkaFamily };
    readonly rules: () => RuleRegistry;
    readonly actions: ReadonlyArray<string>;
};

type _KafkaLane = {
    readonly consumer: KafkaJS.Consumer;
    readonly producer: KafkaJS.Producer;
    readonly position: Option.Option<KafkaJS.TopicOffsets>;
};

type _KafkaCodec = {
    readonly lower: (
        topic: string,
        event: Fanout.Announced,
    ) => Effect.Effect<{ readonly key: string; readonly value: Buffer; readonly band: KafkaJS.IHeaders }, FanoutFault>;
    readonly raise: (
        topic: string,
        key: Buffer | null,
        payload: Buffer,
        headers: KafkaJS.IHeaders,
    ) => Effect.Effect<Fanout.Announced, FanoutFault>;
    readonly close: () => void;
};

// Compat clients publish NO emitter — `connect`, `disconnect`, `logger`, `setSaslCredentialProvider`, and
// `dependentAdmin` are the whole surface — while the wrapper binds `error` and `event.error` internally and routes both
// into its Logger. That Logger is therefore the ONLY seam an async transport fault can reach, so this rail supplies one
// whose error level lands on `pulse` and whose `namespace` returns itself, keeping one cell across every minted client.
const _kafkaLogger = (emit: (detail: string) => void): KafkaJS.Logger => {
    const rail: KafkaJS.Logger = {
        info: () => {},
        warn: () => {},
        debug: () => {},
        error: (message, extra) => emit(`<client-error:${message}${extra === undefined ? '' : `:${JSON.stringify(extra)}`}>`),
        namespace: () => rail,
        setLogLevel: () => {},
    };
    return rail;
};

const _kafkaNamed = <A>(
    rows: Readonly<Record<string, A>>,
    topic: string,
): Effect.Effect<A, FanoutFault> =>
    Option.match(Option.fromNullable(rows[topic]), {
        onNone: () => Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: '<no-contract-row>' } })),
        onSome: Effect.succeed,
    });

const _named = (topics: Fanout.Topics, topic: string): Effect.Effect<Fanout.Topic, FanoutFault> =>
    Option.match(Option.fromNullable(topics[topic]), {
        onNone: () => Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: '<undeclared-topic>' } })),
        onSome: Effect.succeed,
    });
```

## [03]-[PORT_SHAPE]

[PORT_SHAPE]:
- Owner: the `Fanout` Tag — ten members over the topic key. This envelope lane: `publish(topic, envelope)` yields the evidence receipt whose position is either a stream sequence or a partition-offset coordinate; `atomic(topic, consumer, envelopes)` publishes a batch and the naming consume lane's held position as one indivisible unit — the read-process-write member; `subscribe(topic)` is the live fanout stream with the topic's replay window warming a late attach; `consume(topic, consumer, anchor, handler)` is the at-least-once lane whose explicit consumer identity derives a distinct durable name, preventing independent logical subscribers from accidentally load-balancing one durable consumer; `consumers(topic, retire?)` is the durable-consumer doctor read, the optional predicate turning the census into a reap that answers the survivors; `replay(topic, anchor)` re-reads within retention as `Fanout.Replayed`, pairing each envelope with its engine-minted coordinate. This blob lane streams transient large-binary handoff through `stash` / `alias` / `haul`, never a second content-addressing vocabulary or durable store.
- Law: `atomic` is the one indivisible-publish spelling and it is a topic-row capability, never a call-site protocol — an engine whose `serves.atomic` cell reads false answers `horizon` through `_absent` rather than pretending the sequence held. Kafka reads the consumer argument as its OFFSET SOURCE, because there the unit a crash may neither duplicate nor drop spans the read and the write, so that engine binds the handoff off a live consume lane; the JetStream stage binds no read half and names the argument without reading it.
- Law: `_ENGINES` is the one capability authority and `_absent` mints every refusal from it — a binding site never hand-spells a marker, and `Fanout.engine` publishes the same rows a caller reads before it calls.
- Law: `serves` is a map over the PORT MEMBERS and never a second vocabulary beside them, and `anchors` is the admitted anchor set every positional gate reads through `_admits`, so an engine widening either edits one cell and every binding follows it.
- Law: `expects` grades the publish ARGUMENT the member roster cannot — `Fanout.Post` carries an optimistic expectation only a stream engine evaluates, so `_expects` refuses it at the three engines that hold no last-sequence to compare against. Dropping the expectation instead leaves a caller believing a conditional write landed conditionally when it landed unconditionally, which is the one outcome an unguarded write never produces on its own.
- Law: `pulse` is the out-of-band rail — five of these transports learn a delivery or connection failure on a surface no member's await reaches, so each engine projects that surface onto the same `FanoutFault` family and an engine standing behind none answers `Stream.empty`; a row folding only what its call returned reports success for a loss the transport already told someone else about.
- Law: every row answers the four selection columns before a selector binds it, and `lifetime` names its OWNER beside its `until` — `Fanout.Topic.retention` reaches only a row whose lifetime owner is `package`, so a broker-owned or host-owned retention is read from the row rather than presumed from the field.
- Law: the guarantee columns are what the engine alone decides and the recovery columns are what a re-drive stands on, so `degrade` carries only the residual none of them expresses and a coordinate an engine forfeits is stated where a selector sees it.
- Law: an engine row is SELECTED BY the consumption profile and mints no axis vocabulary — `tenancy` states the isolation this engine realizes (a subject namespace, a topic prefix, an origin) while `proc/config#ADMISSION_ROWS` `Profile` owns the closed axis every root states, so a second roster never stands beside it.
- Law: `present` states WHERE this engine's credential lives and what a rotation costs it, spelling the same cell name the egress lane table at `net/client#LANE_ROWS` carries, so one credential vocabulary crosses both planes. The column separates rather than repeats: the in-process rows authenticate nothing at all, JetStream authenticates at HANDSHAKE off `ConnectionOptions`, so a rotated principal reaches a live connection only by replacing it, and Kafka's SASL/OAUTHBEARER provider re-runs on the broker client's own token cadence, so a rotation lands in place and holds the connection open. A message header is never the credential on any row — headers are app metadata no server authenticates on, and a row presenting one would be authenticating the payload rather than the peer.
- Law: `alias` is byte-free aliasing, never a re-stash — a derivative fanned to a second topic or a payload re-published after a replay answers off the target receipt's own store key, so a large binary is paid for once; a `haul`-then-`stash` round trip re-streams every chunk and re-digests an object the store already holds, and it is the named defect this member deletes.
- Law: `consumers` closes the one substrate object a caller mints at runtime — every distinct logical identity ever passed to `consume` leaves a durable consumer holding an ack-pending window and a server cursor, so the census reads them with their pending, unacked, and redelivered depth and the retire predicate retracts a renamed service; Layer-build reconciliation converges streams and stores, and this member is its consumer half.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `horizon` (the anchor precedes the engine's window, the topic is undeclared, or the blob is absent — class `absent`), `publish` (an unacknowledged publish or a rejected stash, class `unavailable`), `poison` (the handler proves an envelope unprocessable, class `malformed` — the consume lane's terminate signal) — so the core budget gate re-drives the transient rows and a horizon miss routes as the terminal evidence it is.
- Law: `reason` bands the fault and `detail` proves it — every mint carries the evidence its own site holds: a caught cause stringified, or an angle-bracketed marker naming the structural refusal and the operand that decided it (the missed window bound, the shelf ceiling, the drifted contract axes, the unbound axis). Each re-minted reason therefore keeps one band while its site stays diagnosable, so a `dial` names its transport failure instead of reporting that dialing failed and nothing more.
- Law: delivery semantics are the row's, not the call site's — a consumer never re-states ack posture, replay depth, retention, or redelivery ceiling; it names the topic and the engine answers the row; an unknown topic answers `horizon` identically on every engine and every member.
- Law: the port is engine-blind — no member names NATS, and swapping any row for another edits the root merge and nothing else; the engine roster law is the services doctrine's, instantiated here.
- Boundary: the `@effect/experimental` EventLog overlay is a PROJECTION of the journal onto a local-first client, never a second carriage lane beside this port. This package claims no native op-log consumer: no real application binding currently maps that envelope into a journal intent, and inventing one here would fork the record of truth this port was built never to become.
- Entry: engines land through one `Fanout` layer; Kafka receives its generated contract projection.
- Packages: `effect` (`Context`, `Data`, `Predicate`, `Schema`, `Stream`); `@rasm/core` (`Fault.Class`).

```typescript signature
// Every refusal on this port names the TOPIC it was raised against and what actually failed — a caught cause
// stringified, or an angle-bracketed marker naming the structural refusal no cause accompanies — so the four rows
// share one subject and differ in what they SAY about it: a band alone left a dead engine and an undeclared topic
// reading identically at the call site.
const _FanoutSubject = Schema.Struct({ topic: Schema.String, detail: Schema.String });

const _family = Fault.Class.family(['dial', 'horizon', 'publish', 'poison'] as const, {
    dial: Fault.Class.row({
        class: 'unavailable',
        leg: 'fanout',
        detail: _FanoutSubject,
        render: ({ topic, detail }) => `${topic} engine transport failed: ${detail}`,
    }),
    horizon: Fault.Class.row({
        class: 'absent',
        leg: 'fanout',
        detail: _FanoutSubject,
        render: ({ topic, detail }) => `${topic} names nothing this port holds: ${detail}`,
    }),
    publish: Fault.Class.row({
        class: 'unavailable',
        leg: 'fanout',
        detail: _FanoutSubject,
        render: ({ topic, detail }) => `${topic} refused the offer: ${detail}`,
    }),
    poison: Fault.Class.row({
        class: 'malformed',
        leg: 'fanout',
        detail: _FanoutSubject,
        render: ({ topic, detail }) => `${topic} carried a frame no codec admits: ${detail}`,
    }),
});

class FanoutFault extends Schema.TaggedError<FanoutFault>()('FanoutFault', {
    case: _family.payload,
}) {
    get class(): Fault.Class.Kind {
        return _family.classOf(this.case.reason);
    }
    override get message(): string {
        return _family.render(this.case);
    }
}

const _announced = (topic: string, event: unknown): Effect.Effect<Fanout.Announced, FanoutFault> =>
    Schema.decodeUnknown(_Announced)(event).pipe(
        Effect.mapError((issue) => new FanoutFault({ case: { reason: 'publish', topic, detail: issue.message } })),
    );

const _partitionKey = (
    topic: string,
    event: Fanout.Announced,
    reason: 'publish' | 'poison' = 'publish',
): Effect.Effect<string, FanoutFault> =>
    Effect.mapError(
        _key(event),
        (refusal) => new FanoutFault({ case: { reason, topic, detail: refusal.message } }),
    );

// Port members ARE the capability roster, so `serves` maps over THEM and a caller reads a refusal off the member it
// was about to call; a capability vocabulary standing beside the members it shadows names one fact twice.
const _MEMBERS = ['publish', 'atomic', 'subscribe', 'consume', 'consumers', 'replay', 'stash', 'alias', 'haul', 'pulse'] as const;

// One descriptor row per engine over four column groups. SELECTION (`fits`, `admit`, `tenancy`, `present`, `lifetime`)
// answers what a composition root binds on — `present` naming where the credential lives and what a rotation costs,
// under the same cell name the egress lane table carries; GUARANTEE (`deliver`, `order`, `settle`) answers what the
// engine itself decides, and one
// value repeating across engines that genuinely differ is a row that stopped reading its engine; RECOVERY (`replay`,
// `bound`, `refuse`) answers where a re-drive resumes, what caps in-flight work, and the SHAPE a refusal arrives in,
// which is the rail trap `pulse` closes; `degrade` carries only the residual no column above already expresses.
// `lifetime.until` is retention and the `bound` column is the in-flight window: two coordinates one name once merged.
// No row carries a retry SCHEDULE — each `deliver` cell names its retry owner, since one owner holds every curve.
// `expects` sits beside `serves` because it grades an ARGUMENT rather than a member: `publish` lands on every engine
// while the optimistic expectation riding beside it lands on one, and a cell is what keeps the other three refusing.
const _ENGINES = {
    local: {
        fits: '<one-process:proof-or-single-node-deployment>',
        admit: 'publish',
        tenancy: '<process-memory>',
        present: '<none:a cell offer crosses no peer,so there is nothing to authenticate against>',
        lifetime: { until: '<bounded-cell-then-scope-close>', owner: 'package' },
        serves: { publish: true, atomic: false, subscribe: true, consume: true, consumers: true, replay: true, stash: true, alias: true, haul: true, pulse: false },
        anchors: ['Window'],
        expects: false,
        deliver: '<at-most-once:an-unattached-subscriber-misses-and-a-handler-fault-ends-the-fold;retry-owner:the-caller>',
        order: '<one-cell-per-topic-in-offer-order;NO-key-member>',
        settle: '<the-PubSub-offer-boolean:a-refused-offer-IS-the-publish-fault>',
        replay: '<Window-only,off-the-cell-own-bounded-replay-buffer;no-positional-coordinate-exists-to-resume-from>',
        bound: '<LocalPolicy.capacity-suspends-the-producer-at-the-slowest-subscriber;LocalPolicy.shelf-caps-the-blob-aggregate>',
        refuse: '<value-only:every-refusal-is-a-typed-fault-on-the-calling-rail>',
        degrade: '<a-shelved-body-carries-no-digest-and-dies-with-the-process>',
    },
    tab: {
        fits: '<one-browser-origin:cross-tab-mirror-and-invalidation>',
        admit: 'publish',
        tenancy: '<browser-origin>',
        present: '<none:the same-origin rule IS the boundary,and a post carries no credential the receiving tab could check>',
        lifetime: { until: '<tab-close>', owner: 'host' },
        serves: { publish: true, atomic: false, subscribe: true, consume: true, consumers: true, replay: true, stash: true, alias: true, haul: true, pulse: false },
        anchors: ['Window'],
        expects: false,
        deliver: '<at-most-once,and-a-post-reaches-only-tabs-live-at-post-time;retry-owner:the-caller>',
        order: '<per-subject-channel-FIFO;NO-key-member-and-no-order-across-subjects>',
        settle: '<the-local-cell-offer-boolean-alone:postMessage-answers-nothing,so-a-cross-tab-drop-settles-nowhere>',
        replay: '<Window-only-and-LOCAL-only:a-post-carries-no-replay,so-a-tab-opened-after-it-never-sees-it>',
        bound: '<the-local-capacity-per-tab;the-channel-itself-bounds-nothing>',
        refuse: '<value-only:a-foreign-post-drops-at-the-decode-seam-and-an-offer-refusal-logs>',
        degrade: '<only-envelopes-cross-the-channel,so-a-haul-or-alias-naming-another-tab-stash-answers-absent>',
    },
    jetstream: {
        fits: '<cluster-durable:full-ack-ledger,dedup-window,positional-replay,object-store>',
        admit: 'publish',
        tenancy: '<nats-account-and-subject-namespace>',
        present: '<at-handshake:ConnectionOptions.authenticator rebuilds the CONNECT frame on every dial,so a rotation REPLACES the connection and every in-flight request with it>',
        // Construction reconciles `max_age` from the topic row, so this package owns the bound it declares.
        lifetime: { until: '<stream-max-age-from-row-retention>', owner: 'package' },
        serves: { publish: true, atomic: true, subscribe: true, consume: true, consumers: true, replay: true, stash: true, alias: true, haul: true, pulse: true },
        anchors: ['Window', 'Sequence', 'Instant'],
        expects: true,
        deliver: '<at-least-once-plus-the-duplicate_window-on-the-address-derived-msgID;retry-owner:the-server,row-wait-and-attempts>',
        order: '<per-subject-stream-order;the-SUBJECT-is-the-key-member,so-per-entity-order-costs-one-subject-per-entity>',
        settle: '<PubAck-carrying-seq-and-duplicate;ackAck-confirms-the-acknowledgement-itself-on-double-rows;BatchAck-settles-a-whole-atomic-stage>',
        replay: '<DeliverPolicy-StartSequence-or-StartTime-bounded-by-StreamInfo-first_seq-and-first_ts>',
        bound: '<TWO-windows:max_ack_pending-from-row-pending-caps-what-the-server-holds-unacked,row-prefetch-caps-max_messages-the-client-pulls-into-this-process;the-replay-fetch-reads-the-same-prefetch>',
        refuse: '<value+throw+TWO-event-surfaces:nc.status-carries-the-CONNECTION-losses-and-ConsumerMessages.status-the-CONSUMER-notifications,disjoint-families-neither-of-which-any-await-reaches>',
        degrade: '<atomic-stages-at-most-1000-messages-under-50-live-batches-and-abandon-after-10s-idle;a-staged-member-carries-no-msgID,so-the-dedup-grain-is-the-batch-not-the-message>',
    },
    kafka: {
        fits: '<cluster-partitioned:transactional-read-process-write-over-a-registry-contract>',
        admit: 'publish',
        tenancy: '<topic-prefix>',
        present: '<on-refresh:the SASL/OAUTHBEARER provider re-runs on librdkafka own token cadence and sets the token in place,so a rotation costs no connection>',
        // Broker-side topic retention this package never sets: `Fanout.Topic.retention` does not reach this row.
        lifetime: { until: '<broker-topic-retention>', owner: 'deploy' },
        serves: { publish: true, atomic: true, subscribe: false, consume: true, consumers: false, replay: false, stash: false, alias: false, haul: false, pulse: true },
        anchors: ['Window'],
        expects: false,
        deliver: '<at-least-once-on-the-manual-commit-lane,exactly-once-on-atomic-alone;keys-partition-and-never-dedup;retry-owner:the-in-process-budget-ledger-intersected-with-row-attempts>',
        order: '<partition-by-record-key;the-envelope-KEY-is-that-member>',
        settle: '<ONE-RecordMetadata-per-topic-partition-carrying-baseOffset,never-one-per-message-and-never-an-offset-field>',
        replay: '<none:Fanout.Anchor-carries-no-partition-coordinate,so-the-data-journal-is-the-positioned-re-read>',
        bound: '<eachBatch-under-partitionsConsumedConcurrently-1-with-auto-resolve-off-holds-one-record-in-flight;the-produce-queue-is-librdkafka-own>',
        refuse: '<value+logger:the-compat-clients-expose-no-emitter,so-every-async-transport-error-lands-on-the-supplied-Logger-alone>',
        degrade: '<consumer-groups-READ-fine-here;the-census-SHAPE-does-not:a-group-carries-committed-offsets-and-members,never-an-ack-pending-window,so-consumers-refuses-rather-than-forging-the-four-columns-it-cannot-measure>',
    },
} as const satisfies Record<string, Fanout.Row>;

// One generator mints every capability refusal from the row it read, so a marker cannot drift from the cell deciding
// it and an engine growing a capability flips one cell while its refusal disappears with no binding edited.
const _absent = (engine: Fanout.Engine, member: Fanout.Member, topic: string, operand = ''): FanoutFault =>
    new FanoutFault({ case: { reason: 'horizon', topic, detail: `<${engine}-no-${member}${operand}>` } });

// One gate over the row's admitted anchor set replaces the `Window`-only ternary each engine hand-spelled: an engine
// widening its anchors edits one cell and every member follows it.
const _admits = (engine: Fanout.Engine, anchor: Fanout.Anchor): boolean =>
    (_ENGINES[engine].anchors as ReadonlyArray<Fanout.Anchor['_tag']>).includes(anchor._tag);

// Publish-time expectations are OPTIMISTIC CONCURRENCY, so an engine that cannot evaluate one refuses instead of
// dropping it: a caller whose guard silently vanished believes the write was conditional and it was unconditional,
// which is the one refusal shape worse than no expectation at all. `expects` is the cell, this is its only reader.
const _expects = (engine: Fanout.Engine, topic: string, post: Fanout.Post): Effect.Effect<void, FanoutFault> =>
    _ENGINES[engine].expects || Option.isNone(post.expect)
        ? Effect.void
        : Effect.fail(_absent(engine, 'publish', topic, ':expect'));

declare namespace Fanout {
    type Member = (typeof _MEMBERS)[number];
    type Engine = keyof typeof _ENGINES;
    type Row = {
        readonly fits: string;
        readonly admit: Member;
        readonly tenancy: string;
        readonly present: string;
        readonly lifetime: { readonly until: string; readonly owner: 'package' | 'host' | 'deploy' };
        readonly serves: { readonly [M in Member]: boolean };
        readonly anchors: ReadonlyArray<Anchor['_tag']>;
        readonly expects: boolean;
        readonly deliver: string;
        readonly order: string;
        readonly settle: string;
        readonly replay: string;
        readonly bound: string;
        readonly refuse: string;
        readonly degrade: string;
    };
    type _Engines<T extends Record<Engine, Row> = typeof _ENGINES> = T;
}

class Fanout extends Context.Tag('runtime/Fanout')<
    Fanout,
    {
        readonly publish: (
            topic: string,
            event: Fanout.Announced,
            post?: Fanout.Post,
        ) => Effect.Effect<Fanout.Receipt, FanoutFault>;
        readonly atomic: (
            topic: string,
            consumer: string,
            events: ReadonlyArray<Fanout.Announced>,
        ) => Effect.Effect<ReadonlyArray<Fanout.Receipt>, FanoutFault>;
        readonly subscribe: (topic: string) => Stream.Stream<Fanout.Announced, FanoutFault>;
        readonly consume: (
            topic: string,
            consumer: string,
            anchor: Fanout.Anchor,
            handler: (event: Fanout.Announced) => Effect.Effect<void, FanoutFault>,
        ) => Effect.Effect<void, FanoutFault>;
        readonly consumers: (
            topic: string,
            retire?: Predicate.Predicate<Fanout.Consumer>,
        ) => Effect.Effect<ReadonlyArray<Fanout.Consumer>, FanoutFault>;
        readonly replay: (topic: string, anchor: Fanout.Anchor) => Stream.Stream<Fanout.Replayed, FanoutFault>;
        readonly stash: (topic: string, name: string, body: Stream.Stream<Uint8Array, FanoutFault>) => Effect.Effect<Fanout.Stowed, FanoutFault>;
        readonly alias: (topic: string, name: string, target: Fanout.Stowed) => Effect.Effect<Fanout.Stowed, FanoutFault>;
        readonly haul: (topic: string, name: string) => Stream.Stream<Uint8Array, FanoutFault>;
        // faults the engine learned OUT OF BAND: this stream never fails, so a row standing behind no such surface is `Stream.empty`
        readonly pulse: Stream.Stream<FanoutFault>;
    }
>() {
    static readonly Anchor = _Anchor;
    static readonly Announced = _Announced;
    static readonly Post = _Post;
    static readonly Topic = _Topic;
    static readonly ReceiptPosition = _ReceiptPosition;
    static readonly Replayed = _Replayed;
    static readonly Receipt = _Receipt;
    static readonly Stowed = _Stowed;
    static readonly Consumer = _Consumer;
    static readonly LocalPolicy = _LocalPolicy;
    static readonly members = _MEMBERS;
    // Callers read the row instead of discovering a capability by fault: a scheduler choosing a topic's engine asks
    // this cell up front, so a refusal it could have foreseen never rides the request path as evidence.
    static readonly engine = _ENGINES;
    static readonly local = (topics: Fanout.Topics, policy: Fanout.LocalPolicy = new _LocalPolicy({})): Layer.Layer<Fanout> => _local(topics, policy);
    static readonly tab = (topics: Fanout.Topics, policy: Fanout.LocalPolicy = new _LocalPolicy({})): Layer.Layer<Fanout, FanoutFault> => _tab(topics, policy);
    static readonly jetstream = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting | Broker> => _jetstream(topics);
    static readonly kafka = (
        topics: Fanout.Topics,
        contracts: Readonly<Record<string, _KafkaContract>>,
        // Composition roots bind this backend-generation port as an axis value: unbound, a contract naming an
        // artifact refuses at admission by axis name; bound, it proves that artifact declared and observed.
        generation: Option.Option<Backend.Generation> = Option.none(),
    ): Layer.Layer<Fanout, FanoutFault, Setting> => _kafka(topics, contracts, generation);
}
```

## [04]-[LOCAL_ROW]

[LOCAL_ROW]:
- Owner: `Fanout.local(topics, policy)` — one scoped `PubSub.bounded<Fanout.Replayed>({ capacity: policy.capacity, replay: row.replay })` per topic row and one `Ref`-held aggregate blob shelf; `publish` mints the topic sequence before offering the envelope/coordinate pair and returns `publish` fault when `PubSub.publish` rejects it (a local publish is never a duplicate — the dedup window is a server guarantee the local row honestly lacks), `subscribe` projects envelopes from the scoped replay rows, `consume` folds the same envelope projection through the handler with the ack posture vacuous (in-process delivery has no redelivery to confirm or terminate), `replay` snapshots the current sequence count and returns exactly the retained warm-up pairs for the `Window` anchor, while a `Sequence` or `Instant` anchor folds to `horizon`; the blob lane folds into the shelf under `policy.shelf`, replaces a held key without double-counting its prior body, aliases a second key onto a held body at zero charge, and streams out keyed `topic/name` through the same alias resolution, digest honestly absent.
- Law: `_ENGINES.kafka` refuses `consumers` on a SHAPE mismatch, never a client limitation — `KafkaJS.Admin` publishes `listGroups`, `describeGroups`, `deleteGroups`, and `fetchOffsets`, so a group census reads perfectly well; what a group cannot answer is `Fanout.Consumer` itself, whose `created`, `delivered`, `unacked`, and `redelivered` columns measure a server-held ack-pending window that the broker's committed-offset model never keeps. Filling those four with zeros publishes a measurement no producer took, so the row refuses whole and a caller needing group state dials the admin surface it names. Recording the refusal as a client incapacity instead is the inverted form, since it reads as a gap a package upgrade closes rather than the durable modelling difference it is.
- Law: `_ENGINES.local` carries this row's forfeits and `_absent` speaks them — a single fiber's publish sequence is not a broker's atomic unit, `serves.consumers` reads true because an empty roster IS the honest census where none is ever minted, and the shelf is process memory under the admitted `Fanout.LocalPolicy.shelf` aggregate byte ceiling where a crossing stash refuses with the `publish` fault instead of exhausting memory, so the general large-binary contract is the jetstream row's; a proof or a single-process deployment selects this row deliberately, and promoting a workload that needs the missing cells is a root Layer swap, never a local re-implementation.
- Law: this row stands behind no out-of-band surface, so `pulse` is `Stream.empty` and `serves.pulse` states it rather than leaving an empty stream indistinguishable from an unwritten one.
- Law: capacity backpressures — the bounded construction suspends a producer ahead of the slowest subscriber's window; a sliding local topic is a row decision, never a default.
- Packages: `effect` (`PubSub`, `Stream`, `Layer`, `Record`, `Ref`, `HashMap`, `Chunk`).

```typescript signature
type _Port = Context.Tag.Service<Fanout>;
type _LocalPort = _Port & { readonly offer: _Port['publish'] };

// Two rows share this kernel, so the engine NAME rides in as a parameter: a tab-row refusal minting
// `<local-no-atomic>` names an engine the caller never bound, and `_absent` exists precisely so a marker cannot
// drift from its row.
const _minted = (
    topics: Fanout.Topics,
    policy: Fanout.LocalPolicy,
    engine: 'local' | 'tab',
): Effect.Effect<_LocalPort, never, Scope.Scope> =>
    Effect.gen(function* () {
        const cells = yield* Effect.all(
            Record.map(topics, (row) => PubSub.bounded<Fanout.Replayed>({ capacity: policy.capacity, replay: row.replay })),
            { concurrency: 'inherit' },
        );
        const shelf = yield* Ref.make({
            size: 0,
            bodies: HashMap.empty<string, { readonly size: number; readonly chunks: Chunk.Chunk<Uint8Array> }>(),
            aliases: HashMap.empty<string, string>(), // an alias is a key pointing at a held body: it charges the shelf nothing, exactly as the object store's link pays no bytes
        });
        const seqs = yield* Ref.make(HashMap.empty<string, number>());
        const held = (topic: string): Effect.Effect<PubSub.PubSub<Fanout.Replayed>, FanoutFault> =>
            Option.match(Option.fromNullable(cells[topic]), {
                onNone: () => Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: '<undeclared-topic>' } })),
                onSome: Effect.succeed,
            });
        const offer: _Port['publish'] = (topic, event, post = new _Post({})) =>
            // Cells hold no last sequence, so an optimistic expectation is unevaluable here and refuses rather than
            // dropping: silently ignoring it hands a caller a write it believes was conditional and was not.
            Effect.zipRight(_expects(engine, topic, post), Effect.flatMap(_announced(topic, event), (admitted) => Effect.flatMap(held(topic), (cell) =>
                Effect.flatMap(
                    Ref.modify(seqs, (counts) => {
                        const next = Option.getOrElse(HashMap.get(counts, topic), () => 0) + 1;
                        return [next, HashMap.set(counts, topic, next)] as const;
                    }),
                    (seq) =>
                        Effect.flatMap(
                            PubSub.publish(
                                cell,
                                new _Replayed({ event: admitted, coordinate: { _tag: 'Sequence', seq } }),
                            ),
                            (delivered) =>
                                delivered
                                    ? Effect.map(
                                          _partitionKey(topic, admitted),
                                          (key) => new _Receipt({
                                              topic,
                                              subject: topics[topic]?.subject ?? topic,
                                              key,
                                              position: { _tag: 'Sequence', seq },
                                              duplicate: false,
                                          }),
                                      )
                                    : Effect.fail(new FanoutFault({ case: { reason: 'publish', topic, detail: '<pubsub-offer-rejected>' } })),
                        ),
                ),
            )));
        return {
            offer,
            // In-process delivery crosses no hop, so the announcement carries whole and the creation-time context its
            // own extensions already seal is the only causal identity there is to continue.
            publish: offer,
            atomic: (topic) => Effect.fail(_absent(engine, 'atomic', topic)),
            consumers: (topic) => Effect.as(held(topic), [] as ReadonlyArray<Fanout.Consumer>), // in-process delivery mints no durable consumer: an empty roster is the honest census, and an undeclared topic still answers horizon
            subscribe: (topic) =>
                Stream.unwrap(Effect.map(held(topic), (cell) => Stream.map(Stream.fromPubSub(cell), (row) => row.event))),
            pulse: Stream.empty,
            consume: (topic, _consumer, anchor, handler) =>
                _admits(engine, anchor)
                    ? Stream.runForEach(
                          Stream.unwrap(Effect.map(held(topic), (cell) => Stream.map(Stream.fromPubSub(cell), (row) => row.event))),
                          (event) => Propagation.ingress(handler(event), Carrier.extract('cloudevents', event)),
                      )
                    : Effect.fail(_absent(engine, 'consume', topic, `:${anchor._tag}`)),
            replay: (topic, anchor) =>
                _admits(engine, anchor)
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
                    : Stream.fail(_absent(engine, 'replay', topic, `:${anchor._tag}`)),
            stash: (topic, name, body) =>
                Effect.zipRight(
                    held(topic),
                    Effect.flatMap(
                        Stream.runFoldEffect(body, { size: 0, chunks: Chunk.empty<Uint8Array>() }, (acc, part) =>
                            // `policy.shelf` is typed evidence: an over-bound stash refuses instead of exhausting memory
                            acc.size + part.byteLength > policy.shelf
                                ? Effect.fail(new FanoutFault({ case: { reason: 'publish', topic, detail: `<shelf-ceiling:${policy.shelf}>` } }))
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
                                              Option.some(new _Stowed({ key, size: folded.size, digest: Option.none<string>() })),
                                              {
                                                  size,
                                                  bodies: HashMap.set(held.bodies, key, folded),
                                                  aliases: HashMap.remove(held.aliases, key), // a stash at an aliased key takes the key back as a body
                                              },
                                          ] as const);
                                }),
                                Option.match({
                                    onNone: () =>
                                        Effect.fail(new FanoutFault({ case: { reason: 'publish', topic, detail: `<shelf-ceiling:${policy.shelf}>` } })),
                                    onSome: Effect.succeed,
                                }),
                            ),
                    ),
                ),
            alias: (topic, name, target) =>
                Effect.zipRight(
                    held(topic),
                    Effect.flatMap(
                        Ref.modify(shelf, (kept) => {
                            const source = Option.getOrElse(HashMap.get(kept.aliases, target.key), () => target.key);
                            return Option.match(HashMap.get(kept.bodies, source), {
                                // an alias of an alias resolves to the one held body, so the chain can never outlive it
                                onNone: () => [Option.none<Fanout.Stowed>(), kept] as const,
                                onSome: (body) =>
                                    [
                                        Option.some(new _Stowed({ key: _blobKey([topic, name]), size: body.size, digest: Option.none<string>() })),
                                        { ...kept, aliases: HashMap.set(kept.aliases, _blobKey([topic, name]), source) },
                                    ] as const,
                            });
                        }),
                        Option.match({
                            onNone: () =>
                                Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<no-shelved-blob:${target.key}>` } })),
                            onSome: Effect.succeed,
                        }),
                    ),
                ),
            haul: (topic, name) =>
                Stream.unwrap(
                    Effect.map(Ref.get(shelf), (kept) => {
                        const key = _blobKey([topic, name]);
                        return Option.match(HashMap.get(kept.bodies, Option.getOrElse(HashMap.get(kept.aliases, key), () => key)), {
                            onNone: () => Stream.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<no-shelved-blob:${name}>` } })),
                            onSome: (body) => Stream.fromChunk(body.chunks),
                        });
                    }),
                ),
        };
    });

const _local = (topics: Fanout.Topics, policy: Fanout.LocalPolicy): Layer.Layer<Fanout> => Layer.scoped(Fanout, _minted(topics, policy, 'local'));
```

## [05]-[TAB_ROW]

[TAB_ROW]:
- Owner: `Fanout.tab(topics)` — the browser cross-tab engine: the local cells decorated with one `BroadcastChannel` per topic row keyed by the row's `subject`. `publish` offers locally then posts the announcement in the ONE structured JSON spelling `Format.event` names beside the hop band, an arriving post decodes back through that same format and offers into the same cells, and every other member is the local row's — so same-tab and cross-tab subscribers read one replay window and the engine is one decoration, never a second implementation.
- Law: the channel loop is structural — `BroadcastChannel` never delivers a post to the posting context, so re-offering an arrival cannot echo; a malformed arrival drops at the decode seam, never poisons a cell.
- Law: the bridge's two failure arms stay distinct — a decode miss drops silently because nothing actionable arrived, while an offer refusal is the cell's own typed fault and logs, so one blanket discard never hides a shelf ceiling or an undeclared topic behind a foreign post.
- Law: `_ENGINES.tab` shares the local row's `serves` and `anchors` and diverges on the four cells the channel moves — `fits`, `tenancy`, `lifetime`, and the `settle`/`replay` pair, because `postMessage` answers nothing and carries no warm window, so a cross-tab drop settles nowhere and a tab opened after a post never sees it; the shelf stays per-tab since only envelopes cross, so a `haul` or `alias` naming another tab's stash answers the local row's absent-blob evidence, and a browser workload needing the durable cells dials the jetstream row over websockets instead.
- Law: both host FFI calls fold onto the typed rail — the `BroadcastChannel` mint refuses in a context without the constructor, and the post frames through the package's own structured serializer inside `Effect.try` beside a `postMessage` that raises on an unclonable body, so neither reaches the fiber as an untyped defect.
- Law: a cross-tab post is a real HOP, so it alone carries the `fanout` carrier row beside the announcement — the sealed creation trace rides the envelope's own extensions and the posting tab's current context rides the frame, so a receiving tab continues the hop it observed.
- Tests: structured JSON round-trips one strictly admitted event through `Event.format.json.single`; a batch-shaped or malformed post drops before the local cell offer.
- Boundary: the session plane's own `BroadcastChannel` (`browser/route` `Vault`) is a distinct, single-purpose channel — session continuity is not fanout, and neither surface composes the other.
- Packages: `effect` (`Stream`, `Schema`, `Record`), `@rasm/core` (`Carrier`, `Event`, `Format`), the host `BroadcastChannel` Web API at the sanctioned FFI seam.

```typescript signature
// Cross-tab posts carry exact JSON-format octets beside the posting tab's hop band. `Event.format` owns both codec
// directions and strict admission, so the local cell cannot observe an unadmitted event.
const _TAB_POST = Schema.Struct({
    structured: Schema.Uint8ArrayFromSelf,
    band: Schema.Record({ key: Schema.String, value: Schema.String }),
});

const _tabLower = (topic: string, event: Fanout.Announced): Effect.Effect<Uint8Array, FanoutFault> =>
    Effect.flatMap(_announced(topic, event), (admitted) =>
        Schema.encode(Event.format.json.single)(admitted).pipe(
            Effect.mapError((issue) => new FanoutFault({ case: { reason: 'publish', topic, detail: issue.message } })),
        ));

const _tabRaise = (
    topic: string,
    post: unknown,
): Effect.Effect<{ readonly event: Fanout.Announced; readonly carrier: Carrier.Extraction }, FanoutFault | ParseResult.ParseError> =>
    Effect.flatMap(Schema.decodeUnknown(_TAB_POST)(post), (framed) =>
        Schema.decodeUnknown(Event.format.json.single)(framed.structured).pipe(
            Effect.mapError((issue) => new FanoutFault({ case: { reason: 'publish', topic, detail: issue.message } })),
            Effect.map((event) => ({ event, carrier: Carrier.extract('fanout', framed.band) })),
        ));

const _tab = (topics: Fanout.Topics, policy: Fanout.LocalPolicy): Layer.Layer<Fanout, FanoutFault> =>
    Layer.scoped(
        Fanout,
        Effect.gen(function* () {
            const inner = yield* _minted(topics, policy, 'tab');
            const posts = yield* Effect.all(
                Record.map(topics, (row) =>
                    Effect.acquireRelease(
                        // BOUNDARY ADAPTER: the constructor is absent outside a browser context, so the mint refuses by fault rather than dying
                        Effect.try({
                            try: () => new BroadcastChannel(row.subject),
                            catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: row.subject, detail: String(cause) } }),
                        }),
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
                        (post) =>
                            _tabRaise(topic, post).pipe(
                                Effect.matchEffect({
                                    // Arrivals that do not decode drop by law: a foreign post on the origin's channel
                                    // never poisons a cell, and its shape is nothing this row can act on.
                                    onFailure: () => Effect.void,
                                    // Offer refusals are the CELL's own typed evidence and log rather than vanish —
                                    // discarding them beside the decode drop leaves cross-tab delivery silently
                                    // one-way with a shelf ceiling or an undeclared topic recorded nowhere.
                                    // Each arriving band is the HOP this tab observed, so the offer runs under it and
                                    // its sealed creation trace stays inside the announcement itself.
                                    onSuccess: ({ carrier, event }) =>
                                        Propagation.ingress(Effect.ignoreLogged(inner.offer(topic, event)), carrier),
                                }),
                            ),
                    ).pipe(Effect.forkScoped),
                { concurrency: 'inherit', discard: true },
            );
            return {
                ...inner,
                publish: (topic, event, post) =>
                    // Decoration never shadows its decorated row: the local cell's own `_expects` gate decides
                    // this expectation, so the bridge forwards it whole instead of minting a second refusal
                    Effect.tap(inner.offer(topic, event, post), () =>
                        Effect.flatMap(
                            Effect.all({ context: Propagation.current, framed: _tabLower(topic, event) }),
                            ({ context, framed }) =>
                                Effect.try({
                                    // Structured clone raises on a body no algorithm can copy: that refusal is the publish fault, never a defect
                                    try: () =>
                                        posts[topic]?.postMessage({
                                            structured: framed,
                                            band: Carrier.inject('fanout', context, {}),
                                        }),
                                    catch: (cause) => new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                                }),
                        )),
            };
        }),
    );
```

## [06]-[JETSTREAM_ROW]

[JETSTREAM_ROW]:
- Owner: `Fanout.jetstream(topics)` — the NATS engine. This connection is capability: the exported `Broker` Tag holds the one scoped dial against `Setting.fanout.origin` — the runtime row's `nats` TCP/TLS binding (`proc/exec#RUNTIME_ROWS`) on the server lanes, the `wsconnect` default on the browser lane — drained on scope close, and the one connection fans into the stream lanes, the object store, and the sibling coordination engine (`coordinate#KV_ROW`) — a second dial beside `Broker.live` is the named defect. Because the credential is a DIAL coordinate here, `Broker` owns the machine principal's residency too: the cell the authenticator reads, the first read that fills it, and the supervisor that re-reads it. Construction reconciles the substrate: `jetstreamManager(nc)` inspects each topic stream, adds an absent stream, and updates a present stream's mutable retention, subject, and dedup policy; `Objm.create` creates or opens the blob store. Restart is therefore convergence, never a duplicate-create failure, while the server's own durability posture (fsync interval, replicas) stays a deployment fact.
- Law: credentials live on `ConnectionOptions` at dial and never in a message header — the CONNECT frame is what the server authenticates, so the `authenticator` this row supplies is what admits the handshake AND every reconnect handshake after it, while `MsgHdrs` stay app metadata the branch binding fills with `ce-` attributes and W3C keys. A publish-time header carrying a token authenticates a payload nobody checks and leaves the connection itself anonymous.
- Law: the authenticator is the rotation rail because it is re-invoked per handshake, not per process — the client resets its info gate on every dial and rebuilds `Connect` from `options.authenticator(nonce)` when the server's INFO lands, so a THUNK-form authenticator re-reads its source at each handshake and a literal freezes the process's first credential forever. `MachinePrincipal.token` is what the frame carries BARE: `credential` prefixes the HTTP scheme its issuer chose, and `auth_token` is an opaque string an auth-callout service reads, so the prefixed form hands the callout a credential that matches nothing.
- Law: the callback is SYNCHRONOUS by the client's own interface, so no grant resolves inside it — the authenticator reads a mutable cell and a scoped supervisor is what keeps that cell fresh; a source read attempted inside the callback would either block the handshake or hand back a stale value with no way to say so.
- Law: rotation REPLACES the connection and the supervisor states that cost — the authenticator runs at handshake alone, so a refreshed principal reaches a live connection only through `nc.reconnect()`, which drops in-flight requests by the client's own documented caveat; the supervisor therefore sleeps the principal's OWN remaining window rather than a lane cadence, and an estate binding no credential source forks no supervisor at all. `reconnect()` rejects against a closed or draining client, which is scope teardown racing the loop, so that arm logs rather than escaping.
- Law: the unbounded reconnect budget and the client's auth-abort default are ONE interlock — rotation must outlive every re-dial this process makes, so `maxReconnectAttempts` is `-1`, and what keeps that from becoming a hot loop against a dead credential is the client's own rule that two identical authentication refusals in a row abort reconnect regardless of policy; `ignoreAuthErrorAbort` is therefore left unset, and setting it true would trade a fast, loud credential failure for an endless quiet one.
- Law: a sender-constrained principal never reaches this dial — the shared `Machine.at` gate refuses a `dpop`-scheme principal for every wire, because RFC 9449 binds the token to a proof over an HTTP method and URL that no CONNECT frame has; the refusal re-spells as a `dial` fault carrying the audience, so a misbound source is a boot refusal rather than a broker rejecting an unbound token.
- Growth: `credsAuthenticator` is the same thunk shape over operator-issued NATS creds and lands as a second row the day a `Setting.fanout` creds row exists; this row projects the machine principal alone, because a creds file is a deploy-plane credential with no principal behind it and `proc/config` owns that row.
- Law: the consumer lanes are split by ack capability — the ordered lane (`subscribe`, `replay`) mints a nameless ordered consumer fixed to `AckPolicy.None`; the durable lane (`consume`) derives `durable_name` from topic and the caller's logical consumer identity, declares explicit ack posture, binds `max_ack_pending` from the row's `pending` ceiling so the descriptor `bound` cell names a number this package set, and binds that same name. Independent consumers therefore receive independent durable streams, while replicas sharing one identity intentionally load-balance.
- Law: `pulse` merges TWO disjoint out-of-band surfaces and neither substitutes for the other. `nc.status()` through `_NATS_PULSE` reports what the SOCKET did — a server error, a slow consumer, a stale connection, a disconnect, a close — while a reconnect or a cluster update carries no loss and emits nothing. `ConsumerMessages.status()` through `_JS_PULSE` reports what a PULL LOOP did, and its nine loss arms name a consumer this connection never lost: missed heartbeats, an absent consumer or stream, a deleted or reset consumer, a recreated ordered consumer, a pull limit, absent responders, and a server-truncated request stating the messages it left unsatisfied. Six further arms — a received heartbeat, a flow-control frame, a pull request, an ignored server status, and the two pin transitions — are the loop working and stay silent. Each leg folds its own read failure into one emitted fault, so the stream never fails and a supervisor reads one family.
- Law: a consume loop reading only the connection stalls SILENTLY, which is why the consumer surface is not optional — the client's own rule is to keep re-requesting from a consumer it once reached, so a deleted durable or a purged stream delivers nothing, fails nothing, and reads exactly like an idle topic while the stream lag grows. Forking that status read publishes the evidence, `abort_on_missing_resource` on the durable lane ends the iterator, and the pair turns an invisible stall into a typed failure a supervisor can re-ensure against.
- Law: the in-flight window is TWO numbers this package sets and neither is the client's default — `max_ack_pending` from the row's `pending` caps what the server holds unacknowledged, and `max_messages` from the row's `prefetch` caps what this process already pulled. Bare `consume()` takes a hundred-message buffer, every one of those messages starts its `ack_wait` clock at DELIVERY, and the heartbeat covers the one message the handler holds — so the tail of an undeclared buffer redelivers work already in hand and reads as broker instability. `threshold_messages` refills at half the buffer, while `expires` and `idle_heartbeat` stay UNSET on the long-lived tail because the client documents both as its own to pick and a low value stresses the server; the bounded replay fetch is the one place `expires` earns a value, and its `max_messages` is the computed retained count rather than the prefetch, since clamping a replay to the ack-pressure bound truncates the window silently.
- Law: the ack algebra folds onto the rail, never past it — `working`, `ack`, `nak`, and `term` each publish on a connection a drain may already have closed, so the confirming calls mint `dial` evidence and the redelivery-bound calls log, and no arm reaches the fiber as a defect.
- Law: NATS carries singular structured JSON through `Event.format.json`, whose codec admits both directions; every batch or non-JSON CloudEvents format refuses before delivery.
- Law: exactly-once publish is the dedup window under the circuit — `js.publish` carries the digest of a tagged, length-framed `(source,id)` address and the envelope's optional expectation projected through `_expected`.
- Law: at-least-once is the full ack algebra — the handler runs under a heartbeat race that stamps `msg.working()` every half `ack_wait` so a long handler never triggers spurious redelivery; success acks — `ack()` on `"fire"` rows, `ackAck()` awaited on `"double"` rows so the acknowledgement itself is confirmed; a `poison` handler fault terminates through `msg.term(reason)`, every other handler fault `nak()`s, and only that ruled handler-failure branch returns `Effect.void`; an `ackAck()` rejection remains a `dial` fault on the consume rail and cannot be mistaken for handled work.
- Law: replay is bounded honesty — `replay(topic, anchor)` snapshots `StreamInfo.state`, rejects a `Sequence` before `first_seq` or an `Instant` before `first_ts`, computes a bounded retained-message count instead of using the absolute head sequence as `max_messages`, opens the ordered lane at the admitted anchor (`Window` → the row's replay-depth sequence, `Sequence(seq)` → `opt_start_seq` under `DeliverPolicy.StartSequence`, `Instant(at)` → `opt_start_time` under `DeliverPolicy.StartTime`), and returns each envelope with its `JsMsg.seq` coordinate before `Consumer.fetch` and `Stream.takeUntil` terminate exactly at that head; `subscribe(topic)` projects envelopes from the same ordered rows, warms from the replay-depth sequence, and then tails through `Consumer.consume`. This durable `consume` anchor is creation policy: an existing named consumer resumes its server-held position and never silently re-anchors.
- Law: Blob aliases retain `Digest.Key` as the sole content address.
- Law: `stash` records object-store digest evidence; `alias` links without copying bytes.
- Law: `haul` joins deferred object-store errors into the stream failure channel.
- Law: `atomic` is the server's own STAGE, never a republish loop — `js.startBatch` opens it, `add` stages the middle, and `commit` publishes the closing message and settles the whole run, so nothing lands until the commit and a crash mid-stage leaves the stream untouched instead of half-written. `allow_atomic` arms the stream at ensure, since the capability is stream shape rather than a publish flag, and the three server ceilings are named rows this row refuses against: a stage carries at most a thousand messages counting the opening and closing ones, at most fifty stages stay live per stream, and a stage untouched past its idle window is abandoned server-side.
- Law: the stage states what it LOSES — `BatchMessageOptions` omits `msgID`, so the opening message alone carries a dedup key and the stage's dedup grain is the batch rather than the message. Sequential republishing bought per-message dedup by never being atomic at all, which is the trade named rather than hidden. `Batch` publishes no abort member, so an interrupted stage is reclaimed by that idle window and nothing here pretends to roll it back.
- Law: `atomic` answers the receipts the engine's own settlement produced and never one per envelope — a stage settles ONCE, so N envelopes answer one receipt carrying the `BatchAck` sequence beside the opening address, while Kafka's transactional lane settles per record and answers N. Indexing either answer by envelope reads a correspondence neither protocol publishes, which is why `settle` states the grain per row. One envelope routes to `publish`, because a single message is already atomic and that form keeps its own `msgID`.
- Tests: NATS structured JSON single round-trips with semantic event equality; JSON batch, Protobuf, and Avro media refuse before handler delivery.
- Law: the durable-consumer census is the reconciliation's missing half — `jsm.consumers.list(topic)` is a `Lister`, so it lifts through the same `Stream.fromAsyncIterable` seam the message lanes ride and pages one turn at a time rather than materializing a roster, each `ConsumerInfo` projecting into the `Fanout.Consumer` fact (created instant, delivered sequence, pending, unacked, redelivered depth); the retire predicate turns the same read into a reap over `jsm.consumers.delete`, answering the survivors so a doctor verb reads the post-reap truth in one call.
- Law: the iterator seam is the platform-forced boundary — `consume()` yields an async iterable the engine lifts through `Stream.fromAsyncIterable` under a scoped acquisition whose release closes the consumer, so teardown rides the `Scope` and a leaked pull loop is unspellable.
- Boundary: NATS server deployment — the websocket listener, fsync `sync_interval` hardening, replica quorum — is the deploy plane's; the data journal remains the system of record, and a projection rebuilt from fanout evidence is the named defect.
- Packages: `@nats-io/nats-core` (`wsconnect`, `tokenAuthenticator`, `Authenticator`, `ConnectionOptions`, `NatsConnection`, `Status`), `@nats-io/jetstream` (`jetstream`, `jetstreamManager`, `AckPolicy`, `DeliverPolicy`, `ConsumerNotification`, `startBatch`), `@nats-io/obj` (`Objm`, `ObjectStore`), `effect` (`DateTime`, `Duration`, `Effect`, `Layer`, `Match`, `MutableRef`, `Predicate`, `PubSub`, `Redacted`, `Schedule`, `Stream`), `@rasm/core` (`Carrier`, `Event`, `Format`), `@rasm/security` (`MachinePrincipal`), `../proc/config.ts` (`Setting`), `./client.ts` (`Breaker`, `Machine`).

```typescript signature
const _nanos = (span: Duration.Duration): number => Duration.toMillis(span) * 1_000_000;

const _BLOB = { store: 'fanout' } as const;
const _CIRCUIT = { trip: 8, cool: Duration.seconds(20), probes: 1 } as const;

// `name` is what the server's monitoring pages call this client, so an operator reading a connection list sees the
// engine rather than an anonymous socket. `attempts` is `-1` — never give up — because a rotation replaces the
// connection and a bounded budget would retire the engine after enough rotations; the client's own rule that two
// identical auth refusals in a row abort reconnect is what still terminates a genuinely dead credential, which is
// why `ignoreAuthErrorAbort` stays unset.
const _NATS = { name: 'rasm-fanout', attempts: -1 } as const;

// The CONNECT frame carries the token BARE: `MachinePrincipal.credential` prefixes the HTTP scheme its issuer chose,
// and `auth_token` is an opaque string an auth-callout service reads, so the prefixed form matches nothing there.
// The thunk form is the whole rotation rail — the client rebuilds `Connect` from `options.authenticator(nonce)` on
// every handshake, so whatever the cell holds at that instant is what authenticates the reconnect.
const _authenticated = (held: MutableRef.MutableRef<Option.Option<MachinePrincipal>>): Authenticator =>
    tokenAuthenticator(() =>
        Option.match(MutableRef.get(held), { onNone: () => '', onSome: (principal) => Redacted.value(principal.token) }));

// `DateTime.distanceDuration` is ABSOLUTE, so a principal five minutes dead reads as a five-minute window and this
// supervisor sleeps past the credential it exists to replace; the SIGNED `distance` is the one member that separates
// them, and the lease row's total is the floor that paces a re-read instead of letting a lapsed principal spin.
const _remainder = (principal: MachinePrincipal): Effect.Effect<Duration.Duration> =>
    Effect.map(DateTime.now, (now) =>
        Duration.max(Fault.Budget.at('lease').total, Duration.millis(Math.max(0, DateTime.distance(now, principal.expiresAt)))));

// Status rows carry unequal evidence and only five of the eleven mean a loss: a reconnect, a cluster update, a ping,
// and a forced reconnect are progress this rail stays silent on, while the five below reach no publish or consume await.
const _NATS_PULSE = Match.type<Status>().pipe(
    Match.discriminators('type')({
        error: (status) => Option.some(`<server-error:${status.error.message}>`),
        slowConsumer: (status) => Option.some(`<slow-consumer:${status.pending}>`),
        staleConnection: () => Option.some('<stale-connection>'),
        disconnect: (status) => Option.some(`<disconnected:${status.server}>`),
        close: () => Option.some('<connection-closed>'),
    }),
    Match.orElse(() => Option.none<string>()),
);

// Consumer notifications are the SECOND out-of-band surface and share nothing with the first: `nc.status()` reports
// what the SOCKET did, `ConsumerMessages.status()` what the PULL LOOP did, and the two unions are disjoint. A loop
// reading only the socket hangs silently on every consumer-scoped failure, because the client's own rule is to keep
// re-requesting from a consumer it once reached — a deleted durable, a purged stream, or a heartbeat gap therefore
// delivers nothing, fails nothing, and reads exactly like an idle topic until someone asks why the lag grows.
// Nine arms carry a loss and six carry progress: a received heartbeat, a flow-control frame, a pull request, an
// ignored server status, and the two pin transitions are the loop working, so this rail stays silent on them.
const _JS_PULSE = Match.type<ConsumerNotification>().pipe(
    Match.discriminators('type')({
        heartbeats_missed: ({ count }) => Option.some(`<heartbeats-missed:${count}>`),
        consumer_not_found: ({ stream, name, count }) => Option.some(`<consumer-absent:${stream}/${name}:${count}>`),
        stream_not_found: ({ name }) => Option.some(`<stream-absent:${name}>`),
        consumer_deleted: ({ code, description }) => Option.some(`<consumer-deleted:${code}:${description}>`),
        ordered_consumer_recreated: ({ name }) => Option.some(`<ordered-consumer-recreated:${name}>`),
        exceeded_limits: ({ code, description }) => Option.some(`<pull-limit:${code}:${description}>`),
        no_responders: ({ code }) => Option.some(`<no-responders:${code}>`),
        // server-truncated pull request: exactly the messages this loop asked for and did NOT receive
        discard: ({ messagesLeft, bytesLeft }) => Option.some(`<request-discarded:${messagesLeft}msg:${bytesLeft}b>`),
        reset: ({ name }) => Option.some(`<consumer-reset:${name}>`),
    }),
    Match.orElse(() => Option.none<string>()),
);

// Server-owned atomic-stage ceilings this row refuses against rather than discovering by rejection: one stage
// carries at most this many messages counting the opening and committing ones, at most this many stages stay
// live per stream, and one untouched past this window is abandoned server-side. `Batch` publishes no abort
// member, so that idle window IS the abort an interrupted stage falls back on.
const _BATCH = { members: 1_000, live: 50, idle: Duration.seconds(10) } as const;

const _expected = (expect: Fanout.Post['expect']) =>
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

// No official SDK binding exists for NATS. The transport therefore carries the exact structured JSON single codec;
// it does not fabricate HTTP binary headers or restate CloudEvents attributes.
const _natsLower = (
    topic: string,
    event: Fanout.Announced,
): Effect.Effect<{
    readonly event: Fanout.Announced;
    readonly band: Readonly<Record<string, string>>;
    readonly body: Uint8Array;
}, FanoutFault> =>
    Effect.flatMap(_announced(topic, event), (admitted) =>
        Schema.encode(Event.format.json.single)(admitted).pipe(
            Effect.mapError((issue) => new FanoutFault({ case: { reason: 'publish', topic, detail: issue.message } })),
            Effect.map((body) => ({
                event: admitted,
                band: { [CONSTANTS.HEADER_CONTENT_TYPE]: Event.format.json.media },
                body,
            })),
        ));

const _natsRaise = (topic: string, msg: JsMsg): Effect.Effect<Fanout.Announced, FanoutFault> =>
    Option.match(
        Option.flatMap(
            Option.fromNullable(_unband(msg.headers)[CONSTANTS.HEADER_CONTENT_TYPE]),
            Format.event.framed,
        ),
        {
            onNone: () => Effect.fail(new FanoutFault({ case: { reason: 'poison', topic, detail: '<event-format-absent>' } })),
            onSome: ({ format, batch }) =>
                batch
                    ? Effect.fail(new FanoutFault({ case: { reason: 'poison', topic, detail: '<batch-unsupported:nats>' } }))
                    : format !== 'json'
                      ? Effect.fail(new FanoutFault({ case: { reason: 'poison', topic, detail: `<format-unsupported:nats:${format}>` } }))
                      : Schema.decodeUnknown(Event.format.json.single)(msg.data).pipe(
                            Effect.mapError((issue) =>
                                new FanoutFault({ case: { reason: 'poison', topic, detail: issue.message } })),
                        ),
        },
    );

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
              Sequence: ({ seq }) =>
                  seq < info.state.first_seq
                      ? Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<before-first-seq:${info.state.first_seq}>` } }))
                      : Effect.void,
              Instant: ({ at }) =>
                  Option.match(DateTime.make(info.state.first_ts), {
                      onNone: () => Effect.fail(new FanoutFault({ case: { reason: 'dial', topic, detail: `<unreadable-first-ts:${info.state.first_ts}>` } })),
                      onSome: (first) =>
                          DateTime.lessThan(at, first)
                              ? Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<before-first-ts:${info.state.first_ts}>` } }))
                              : Effect.void,
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
            Effect.gen(function* () {
                const setting = yield* Setting;
                // The dial origin IS the audience: a source holding a principal for another service answers none here
                // rather than presenting a token this broker was never the audience for.
                const audience = setting.fanout.origin.origin;
                const held = MutableRef.make(Option.none<MachinePrincipal>());
                const refreshed = Effect.tap(
                    Effect.mapError(
                        Machine.at('fanout:dial', audience),
                        (lapse) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: `<credential-${lapse.reason}:${audience}>` } }),
                    ),
                    (principal) => Effect.sync(() => MutableRef.set(held, principal)),
                );
                const armed = yield* refreshed; // the first read fills the cell BEFORE the dial builds its CONNECT frame
                const nc = yield* Effect.acquireRelease(
                    Effect.tryPromise({
                        try: () =>
                            dial({
                                servers: setting.fanout.origin.href,
                                name: _NATS.name,
                                authenticator: _authenticated(held),
                                maxReconnectAttempts: _NATS.attempts,
                            }),
                        catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: String(cause) } }),
                    }),
                    (live) => Effect.orDie(Effect.tryPromise(() => live.drain())),
                );
                // Refresh rides the PRINCIPAL's own window, never a lane cadence, and the forced reconnect is what
                // makes the refreshed cell reach the wire — the authenticator runs at handshake alone. An estate that
                // binds no source armed nothing, so it forks nothing.
                yield* Option.match(armed, {
                    onNone: () => Effect.void,
                    onSome: () =>
                        Effect.forkScoped(
                            Effect.forever(
                                Effect.flatMap(
                                    Option.match(MutableRef.get(held), {
                                        onNone: () => Effect.succeed(Fault.Budget.at('lease').total),
                                        onSome: _remainder,
                                    }),
                                    (window) =>
                                        Effect.zipRight(
                                            Effect.sleep(window),
                                            Effect.zipRight(
                                                Effect.ignoreLogged(refreshed),
                                                // rejects against a closed or draining client, which is scope teardown racing this loop
                                                Effect.ignoreLogged(Effect.tryPromise(() => nc.reconnect())),
                                            ),
                                        ),
                                ),
                            ),
                        ),
                });
                return nc;
            }),
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
                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: String(cause) } }),
            });
            yield* Effect.forEach(
                Record.toEntries(topics),
                ([name, row]) =>
                    Effect.gen(function* () {
                        const config = {
                            subjects: [row.subject],
                            max_age: _nanos(row.retention),
                            duplicate_window: _nanos(setting.fanout.dedup),
                            // Stream shape arms the atomic stage, never a publish flag: unset, `startBatch`
                            // refuses at the server and the row's `serves.atomic` cell claims what it cannot do
                            allow_atomic: true,
                        };
                        const current = yield* Effect.tryPromise({
                            try: () => jsm.streams.info(name),
                            catch: (cause) => cause,
                        }).pipe(
                            Effect.matchEffect({
                                onFailure: (cause) =>
                                    cause instanceof JetStreamApiError && cause.code === JetStreamApiCodes.StreamNotFound
                                        ? Effect.succeed(Option.none())
                                        : Effect.fail(new FanoutFault({ case: { reason: 'dial', topic: name, detail: String(cause) } })),
                                onSuccess: (info) => Effect.succeed(Option.some(info)),
                            }),
                        );
                        yield* Option.match(current, {
                            onNone: () =>
                                Effect.tryPromise({
                                    try: () => jsm.streams.add({ name, ...config }),
                                    catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: name, detail: String(cause) } }),
                                }),
                            onSome: () =>
                                Effect.tryPromise({
                                    try: () => jsm.streams.update(name, config),
                                    catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: name, detail: String(cause) } }),
                                }),
                        });
                    }),
                { concurrency: 'inherit', discard: true },
            );
            const store: ObjectStore = yield* Effect.tryPromise({
                try: () => new Objm(nc).create(_BLOB.store),
                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: _BLOB.store, detail: String(cause) } }),
            });
            // One cell every consumer's own status iterator writes into, merged with the connection projection at
            // `pulse`: the two surfaces answer different questions and a supervisor reads one family for both.
            const beat = yield* Effect.acquireRelease(PubSub.unbounded<FanoutFault>(), (live) => PubSub.shutdown(live));

            const named = (topic: string): Effect.Effect<Fanout.Topic, FanoutFault> => _named(topics, topic);

            // `pull` carries NO default: the client picks `max_messages: 100` for a bare `consume()`, and every one of
            // those hundred starts its `ack_wait` clock at delivery while the sequential handler heartbeats the head
            // alone — so the tail redelivers work already in this process and the row's `bound` cell names a number
            // nobody set. Both call sites therefore spell the buffer off the topic row.
            const pulled = (
                topic: string,
                minted: Effect.Effect<Consumer, FanoutFault>,
                pull: (consumer: Consumer) => Promise<ConsumerMessages>,
            ): Stream.Stream<readonly [Fanout.Announced, JsMsg], FanoutFault> =>
                Stream.unwrapScoped(
                    Effect.gen(function* () {
                        const consumer = yield* minted;
                        const messages = yield* Effect.acquireRelease(
                            Effect.tryPromise({
                                try: () => pull(consumer),
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                            (live) => Effect.orDie(Effect.tryPromise(() => live.close())),
                        );
                        // Status rides THIS loop's scope, so the iterator dies with the pull it describes and a
                        // stalled consumer's evidence reaches `pulse` while the message iterator yields nothing.
                        yield* Effect.forkScoped(
                            Effect.ignoreLogged(Stream.runForEach(
                                Stream.fromAsyncIterable(
                                    messages.status(),
                                    (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                ),
                                (notice) =>
                                    Option.match(_JS_PULSE(notice), {
                                        onNone: () => Effect.void,
                                        onSome: (detail) =>
                                            Effect.asVoid(PubSub.publish(beat, new FanoutFault({ case: { reason: 'dial', topic, detail } }))),
                                    }),
                            )),
                        );
                        return Stream.fromAsyncIterable(messages, (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } })).pipe(
                            Stream.mapEffect((msg: JsMsg) =>
                                Effect.map(_natsRaise(topic, msg), (event) => [event, msg] as const)),
                        );
                    }),
                );

            const ordered = (
                topic: string,
                anchor: Fanout.Anchor,
                prefetch: number,
                bound: Option.Option<{ readonly head: number; readonly limit: number; readonly wait: Duration.Duration }> = Option.none(),
            ): Stream.Stream<Fanout.Replayed, FanoutFault> => {
                const source = pulled(
                    topic,
                    Effect.zipRight(
                        named(topic),
                        Effect.tryPromise({
                            try: () => js.consumers.get(topic, _start(anchor)),
                            catch: (cause) => new FanoutFault({ case: { reason: 'horizon', topic, detail: String(cause) } }),
                        }),
                    ),
                    Option.match(bound, {
                        // Ordered delivery acks nothing, so this buffer costs no redelivery — it is still the row's,
                        // because an undeclared client default is a bound the descriptor cannot name. `expires` and
                        // `idle_heartbeat` stay UNSET on this long-lived tail: the client documents both as its own to
                        // pick, and a low value stresses the server while the status rail already reports the gaps.
                        onNone: () => (consumer: Consumer) =>
                            consumer.consume({ max_messages: prefetch, threshold_messages: Math.max(1, Math.floor(prefetch / 2)) }),
                        // Bounded replay is the one place `expires` earns a value: the fetch must end when the
                        // retained window runs out rather than park for the client's thirty-second default. Its
                        // `max_messages` is the computed retained COUNT and never the row's prefetch — clamping a
                        // replay to the ack-pressure bound ends the fetch early and truncates the window silently.
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
                    ([event, msg]) =>
                        new _Replayed({
                            event,
                            coordinate: { _tag: 'Sequence', seq: msg.seq },
                        }),
                );
            };

            const subscribed = (topic: string): Stream.Stream<Fanout.Announced, FanoutFault> =>
                Stream.unwrap(
                    Effect.flatMap(named(topic), (row) =>
                        Effect.flatMap(
                            Effect.tryPromise({
                                try: () => jsm.streams.info(topic),
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                            (info) =>
                                Stream.map(
                                    ordered(topic, _Anchor.Sequence({ seq: Math.max(1, info.state.last_seq - row.replay + 1) }), row.prefetch),
                                    (replayed) => replayed.event,
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
                                    : Effect.fail(new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } })),
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
                                        max_ack_pending: row.pending, // the row's in-flight ceiling: unset, the server default decides a bound no descriptor cell could name
                                        ..._start(anchor),
                                    }),
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                        onSome: () =>
                            Effect.tryPromise({
                                try: () =>
                                    jsm.consumers.update(topic, durable, {
                                        ack_wait: _nanos(row.wait),
                                        max_deliver: row.attempts,
                                        max_ack_pending: row.pending,
                                    }),
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                    });
                    return yield* Effect.tryPromise({
                        try: () => js.consumers.get(topic, durable),
                        catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                    });
                });

            const censused = (topic: string, retire?: Predicate.Predicate<Fanout.Consumer>): Effect.Effect<ReadonlyArray<Fanout.Consumer>, FanoutFault> =>
                Effect.flatMap(named(topic), () =>
                    Stream.runCollect(
                        Stream.mapEffect(
                            Stream.fromAsyncIterable(
                                jsm.consumers.list(topic), // the Lister IS the async iterable: one page pull per turn, never a materialized roster
                                (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            ),
                            (info) =>
                                Effect.map(
                                    Option.match(DateTime.make(info.created), {
                                        onNone: () =>
                                            Effect.fail(new FanoutFault({ case: { reason: 'dial', topic, detail: `<unreadable-created:${info.created}>` } })),
                                        onSome: Effect.succeed,
                                    }),
                                    (created) =>
                                        new _Consumer({
                                            name: info.name,
                                            created,
                                            delivered: info.delivered.stream_seq,
                                            pending: info.num_pending,
                                            unacked: info.num_ack_pending,
                                            redelivered: info.num_redelivered,
                                        }),
                                ),
                        ),
                    ).pipe(
                        Effect.map(Chunk.toReadonlyArray),
                        Effect.flatMap((roster) =>
                            retire === undefined
                                ? Effect.succeed(roster)
                                : // the reap is the census with its predicate applied: one round trip per retired name, the survivors answered
                                  Effect.as(
                                      Effect.forEach(
                                          roster.filter(retire),
                                          (row) =>
                                              Effect.tryPromise({
                                                  try: () => jsm.consumers.delete(topic, row.name),
                                                  catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                              }),
                                          { concurrency: 'inherit', discard: true },
                                      ),
                                      roster.filter((row) => !retire(row)),
                                  ),
                        ),
                    ),
                );

            const replayed = (topic: string, anchor: Fanout.Anchor): Stream.Stream<Fanout.Replayed, FanoutFault> =>
                Stream.unwrap(
                    Effect.flatMap(named(topic), (row) =>
                        Effect.flatMap(
                            Effect.tryPromise({
                                try: () => jsm.streams.info(topic),
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                            (info) =>
                                Effect.as(
                                    _within(topic, anchor, info),
                                    ordered(
                                        topic,
                                        _Anchor.$is('Window')(anchor)
                                            ? _Anchor.Sequence({ seq: Math.max(info.state.first_seq, info.state.last_seq - row.replay + 1) })
                                            : anchor,
                                        row.prefetch,
                                        Option.some({ head: info.state.last_seq, limit: _remaining(anchor, info, row.replay), wait: row.wait }),
                                    ),
                                ),
                        ),
                    ),
                );

            const published = (
                topic: string,
                event: Fanout.Announced,
                post: Fanout.Post = new _Post({}),
            ): Effect.Effect<Fanout.Receipt, FanoutFault> =>
                Effect.flatMap(
                    Effect.all({ context: Propagation.current, lowered: _natsLower(topic, event), row: named(topic) }),
                    ({ context, lowered, row }) =>
                        Effect.flatMap(Event.address(lowered.event), (msgID) => Breaker.guard(
                            'fanout:publish',
                            _CIRCUIT,
                        )(
                            Effect.tryPromise({
                                try: () =>
                                    js.publish(row.subject, lowered.body, {
                                        // The typed length frame makes the `(source,id)` address injective before its
                                        // digest becomes the broker-sized replay coordinate.
                                        msgID,
                                        // Binding names prefix `ce-` and the hop carrier writes bare W3C keys, so the two
                                        // sets are disjoint and the sealed creation trace survives this injection intact.
                                        headers: _hdrs({ ...Carrier.inject('nats', context, {}), ...lowered.band }),
                                        ..._expected(post.expect),
                                    }),
                                catch: (cause) => new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                            }),
                        )).pipe(
                            Effect.mapError((fault) =>
                                fault._tag === 'Lapse' ? new FanoutFault({ case: { reason: 'publish', topic, detail: '<breaker-open>' } }) : fault,
                            ),
                            Effect.flatMap((ack) =>
                                Effect.map(_partitionKey(topic, lowered.event), (key) =>
                                    new _Receipt({
                                        topic,
                                        subject: row.subject,
                                        key,
                                        position: { _tag: 'Sequence', seq: ack.seq },
                                        duplicate: ack.duplicate,
                                    }))),
                        ),
                );

            // Server-staged atomicity, never a republish loop: `startBatch` opens the stage, `add` fills the
            // middle, and `commit` publishes the closing message and settles the whole run — nothing lands until
            // that commit, so a crash mid-stage leaves the stream untouched instead of half-written.
            //
            // What the collapse LOSES, stated where a caller reads it: `BatchMessageOptions` omits `msgID`, so the
            // OPENING message alone carries a dedup key. Dedup grain becomes the stage rather than the message — a
            // replayed stage answers duplicate on the whole run — and sequential republishing bought per-message
            // dedup at the price of never being atomic at all.
            //
            // Arity changes with it: one stage settles ONCE, so N envelopes answer ONE receipt carrying the
            // `BatchAck` sequence beside the opening address. Kafka's transactional lane settles per record and
            // answers N; indexing either answer by envelope reads a correspondence neither protocol publishes,
            // which is why `settle` states the grain per row.
            const staged = (
                topic: string,
                _consumer: string,
                events: ReadonlyArray<Fanout.Announced>,
            ): Effect.Effect<ReadonlyArray<Fanout.Receipt>, FanoutFault> =>
                events.length <= 1
                    ? // one message is already atomic, and `publish` is the form that keeps its own `msgID`
                      Effect.forEach(events, (event) => published(topic, event))
                    : events.length > _BATCH.members
                      ? Effect.fail(new FanoutFault({
                            case: { reason: 'publish', topic, detail: `<stage-over-ceiling:${events.length}/${_BATCH.members}>` },
                        }))
                      : Effect.gen(function* () {
                            const context = yield* Propagation.current;
                            const row = yield* named(topic);
                            const lowered = yield* Effect.forEach(events, (event) => _natsLower(topic, event));
                            const opening = lowered[0]!;
                            const closing = lowered[lowered.length - 1]!;
                            const band = (framed: (typeof lowered)[number]) =>
                                _hdrs({ ...Carrier.inject('nats', context, {}), ...framed.band });
                            const key = yield* Event.address(opening.event);
                            const ack = yield* Effect.tryPromise({
                                try: async () => {
                                    // `Batch` publishes no abort member: an interrupted stage is reclaimed by the
                                    // server's own idle-abandon window, which is why that window is a named ceiling
                                    // rather than a comment, and why nothing here pretends to roll the stage back.
                                    const batch = await js.startBatch(row.subject, opening.body, {
                                        msgID: key,
                                        headers: band(opening),
                                    });
                                    for (const framed of lowered.slice(1, -1)) {
                                        batch.add(row.subject, framed.body, { headers: band(framed) });
                                    }
                                    return await batch.commit(row.subject, closing.body, { headers: band(closing) });
                                },
                                catch: (cause) => new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                            });
                            return [
                                new _Receipt({
                                    topic,
                                    subject: row.subject,
                                    key,
                                    position: { _tag: 'Sequence', seq: ack.seq },
                                    duplicate: ack.duplicate,
                                }),
                            ];
                        });

            return {
                publish: published,
                pulse: Stream.merge(
                    Stream.filterMap(
                        Stream.fromAsyncIterable(nc.status(), (cause) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: String(cause) } })),
                        (status) => Option.map(_NATS_PULSE(status), (detail) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail } })),
                    ).pipe(Stream.catchAll(Stream.succeed)), // the iterator's own failure IS a transport fact, so it rides the stream rather than ending it
                    // Consumer half: every live pull loop's own status iterator writes here, so a stalled consume
                    // and a dropped socket reach one supervisor as one family instead of one arriving and one not
                    Stream.fromPubSub(beat),
                ),
                atomic: staged,
                consumers: censused,
                subscribe: subscribed,
                consume: (topic, consumer, anchor, handler) =>
                    Effect.flatMap(named(topic), (row) =>
                        Stream.runForEach(
                            pulled(topic, durable(topic, consumer, row, anchor), (bound) =>
                                bound.consume({
                                    max_messages: row.prefetch,
                                    threshold_messages: Math.max(1, Math.floor(row.prefetch / 2)),
                                    // Durables this Layer ensured and the server then dropped are TERMINAL, so the
                                    // iterator ends and the fault escapes to a supervisor that can re-ensure it.
                                    // Otherwise the client keeps re-requesting from a consumer it once reached, which
                                    // is exactly the shape that reads as an idle topic while the stream lag grows.
                                    abort_on_missing_resource: true,
                                })),
                            ([event, msg]) =>
                            Effect.matchEffect(
                                Effect.raceFirst(
                                    // first COMPLETION wins: the heartbeat never settles, so the handler's success or failure always decides and the beat dies with it
                                    // Handlers continue the HOP the publisher opened, read off the frame's own bare W3C keys;
                                    // its sealed creation trace stays inside the announcement for a consumer joining on causality.
                                    Propagation.ingress(handler(event), Carrier.extract('nats', _unband(msg.headers))),
                                    Effect.repeat(
                                        Effect.try({
                                            try: () => msg.working(),
                                            catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: `<heartbeat-refused:${String(cause)}>` } }),
                                        }),
                                        Schedule.spaced(Duration.times(row.wait, 0.5)),
                                    ),
                                ),
                                {
                                    // handler failure already decided this arm and `ack_wait` owns redelivery either way, so a refused signal logs rather than masking that evidence
                                    onFailure: (fault) =>
                                        Effect.ignoreLogged(
                                            Effect.try({
                                                try: () => (fault.reason === 'poison' ? msg.term(fault.reason) : msg.nak()),
                                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                            }),
                                        ),
                                    onSuccess: () =>
                                        row.ack === 'double'
                                            ? Effect.flatMap(
                                                  Effect.tryPromise({
                                                      try: () => msg.ackAck(),
                                                      catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                                  }),
                                                  (confirmed) =>
                                                      confirmed
                                                          ? Effect.void
                                                          : Effect.fail(new FanoutFault({ case: { reason: 'dial', topic, detail: '<ack-unconfirmed>' } })),
                                              )
                                            : Effect.try({
                                                  try: () => msg.ack(),
                                                  catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                              }),
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
                                catch: (cause) => new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                            }),
                            (info) => new _Stowed({ key: info.name, size: info.size, digest: Option.some(info.digest) }),
                        ),
                    ),
                alias: (topic, name, target) =>
                    Effect.zipRight(
                        named(topic),
                        Effect.flatMap(
                            Effect.tryPromise({
                                try: () => store.info(target.key),
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                            (info) =>
                                info === null
                                    ? Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<no-stored-object:${target.key}>` } }))
                                    : Effect.map(
                                          Effect.tryPromise({
                                              // Links carry no chunks: the second name resolves to the same stored entries, digest included
                                              try: () => store.link(_blobKey([topic, name]), info),
                                              catch: (cause) => new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                                          }),
                                          (linked) => new _Stowed({ key: linked.name, size: info.size, digest: Option.some(info.digest) }),
                                      ),
                        ),
                    ),
                haul: (topic, name) =>
                    Stream.unwrap(
                        Effect.map(
                            Effect.tryPromise({
                                try: () => store.get(_blobKey([topic, name])),
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                            (result) =>
                                result === null
                                    ? Stream.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<no-stored-object:${name}>` } }))
                                    : Stream.fromReadableStream({
                                          evaluate: () => result.data,
                                          onError: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                      }).pipe(
                                          Stream.concat(
                                              Stream.drain(
                                                  Stream.fromEffect(
                                                      Effect.flatMap(
                                                          Effect.tryPromise({
                                                              try: () => result.error,
                                                              catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                                          }),
                                                          (fault) =>
                                                              fault === null
                                                                  ? Effect.void
                                                                  : Effect.fail(new FanoutFault({ case: { reason: 'dial', topic, detail: String(fault) } })),
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
- Owner: `Fanout.kafka(topics, contracts, generation)` — one scoped client pair over the topic-contract roster, with the backend-generation port arriving as an axis value the composition root binds and defaulting unbound.
- Law: boot proves a total topic-contract map, an inhabited broker roster, exact subject identity, compatibility, writer schema, and rule coverage; a contract naming its backend artifact demands the bound generation port too and refuses by axis name when the root leaves it unbound, while a contract naming none admits and claims no attestation.
- Law: every admission refusal reports the axes that decided it — the roster drift names both key sets, the identity sweep folds one row per proven axis into a `<contract-drift:…>` list, and the artifact read names the unproven artifact.
- Law: the codec family is the contract's own `schema.schemaType` read through one `as const` pair table — `AVRO`, `JSON`, `PROTOBUF` all mint on the registry's shared `(client, SerdeType, config, rules)` arity, so a family is a lookup and a new one is a table row, never an arm ladder or a second engine; `SchemaId` carries the same discriminant, and the Protobuf frame's message indexes therefore read off the existing `fromBytes` with no second framing path.
- Law: one matched pair per topic carries exact `useSchemaId`, explicit subject selection, validation, and a local `RuleRegistry`; `validate` rides the JSON arm's own config extension and the other two families ignore the key.
- Law: the guarantee ledger reads honestly per column — at-least-once holds on the sequential manual-commit lane (`consumer.run({ eachBatch })` with auto-resolve disabled): each record retries under the topic row's ledger budget, resolution and commit follow handler success, and exhaustion stops the run before a higher offset. Exactly-once holds on `atomic` alone, over the lane's own transactional producer. Kafka keys select partitions, never deduplicate, so every `publish` receipt answers `duplicate: false` with its exact partition-offset position; positional consume anchors, ordered replay, warm subscription, the durable-consumer census, and blob carriage answer `horizon` because `Fanout.Anchor` carries no partition coordinate, a consumer group is not a server-held consumer object, and this row carries no object store. `Window` alone selects the unpositioned consumer-group flow.
- Law: the broker ack is the delivery report and the report fills `baseOffset` ALONE — a delivery failure rejects the awaiting record's own promise, so `send` never resolves over a refused write, but the compat metadata declares an `offset` field the wrapper never writes and folds its rows to ONE per topic-partition at the minimum offset; a receipt reading `offset` refuses every landed record and one indexed by envelope over a multi-message send asserts a position that record never held, which is why `atomic` sends one envelope per call in offer order.
- Law: `pulse` is the Logger seam — the compat clients publish no emitter at all, and the wrapper binds `error` and `event.error` itself and routes both into whatever Logger its config carried, so the engine supplies one whose error level writes onto a scoped `PubSub` every minted client shares; leaving the default Logger bound sends every async transport fault to a console and nothing else.
- Law: this is the credential-projection family's third row and the only one whose rotation costs no connection — `sasl.mechanism` is `OAUTHBEARER` and `oauthBearerProvider` is an ASYNC callback librdkafka re-runs on its own token-refresh cadence, handing the answer to `setOAuthBearerToken` on the live client, so a rotated principal lands in place while the NATS row must re-dial and the HTTP lanes re-stamp per call. Whether the mechanism is armed at all is a BOOT decision off one source read: a source holding no principal for this cluster leaves `sasl` absent and the engine dials exactly as it does unauthenticated today, because a `mechanism` set with no token to carry refuses every handshake instead of degrading.
- Law: the three fields the provider answers are each a different projection and each has a way to be silently wrong — `value` is the BARE `MachinePrincipal.token`, because RFC 7628 frames `auth=Bearer <token>` itself and the HTTP-shaped `credential` double-prefixes; `principal` is the `clientId`, the Kafka principal name the broker authorizes against; and `lifetime` is ABSOLUTE epoch milliseconds off `expiresAt`, not a remaining span, because librdkafka documents it as when the token expires since the epoch — handing it a duration dates every token to 1970 and the broker treats each one as already dead.
- Law: a credential refusal reaches the same rail every other async fault does — a rejected provider promise makes the wrapper call `setOAuthBearerTokenFailure` and emit on `error`, which the supplied Logger routes onto the `pulse` cell, so a source that empties mid-life surfaces as one transport fault on the stream a supervisor already reads rather than as a silent authentication stall.
- Law: the audience is the SORTED bootstrap roster, because that roster is the cluster's identity and its declaration order is not — keying on the raw array would mint a second audience for the same brokers listed differently and strand the principal a source holds for the first spelling.
- Law: `atomic` is the transactional lane and it binds to a live `consume` identity — the consume acquisition mints a second producer whose `transactionalId` IS the group name, registers the consumer handle beside it, and stamps the next-offset position before every handler call, so `atomic` opens `producer.transaction()`, sends the batch, hands that exact position to `sendOffsets({ consumer, topics })`, and `commit()`s both as one unit; a failed or interrupted unit aborts, so no half-published batch survives. Naming an identity with no live lane, or a lane that has not yet processed a record, answers `horizon` by that name — the offset half has no source, and inventing one forges the guarantee.
- Law: two independent coordinates stay with their owners — the package's `ce_` binding owns the header band and record key projected from `partitionkey`, while `Fanout.Contract` alone pins registry subject, version, id, compatibility, and schema; `dataschema` describes the event payload schema URI and is never repurposed as a registry coordinate.
- Law: `autoRegisterSchemas: false` keeps registration out of the producer path; exact identity admits before `Fanout` exists.
- Law: the consumer identity derives exactly as the durable lane's — `groupId` is `${topic}:${consumer}`, so independent logical subscribers hold independent groups and replicas sharing one identity load-balance; `subscribe({ topic: row.subject })` precedes `run`, and the handler continues any caller-supplied parent through `Carrier.extract('kafka', ...)` and `Propagation.ingress`.
- Law: `Carrier.record.read` normalizes Kafka headers into the canonical immutable byte frame.
- Law: `Carrier.inject("kafka", ...)` writes causal context before `Carrier.record.write` projects producer headers.
- Law: the registry frame is the sole PAYLOAD framing and `datacontenttype` states it as row data off that arrow, so a consumer reads opaque octets under a declared media rather than a literal asserting a shape the serde chose.
- Tests: each registry family round-trips a strictly admitted single event; `Kafka.toEvent` arrays refuse as unsupported batch, clone results re-admit, and registry subject/version remain independent of absent or unrelated `dataschema`.
- Law: replay stays engine-neutral, so this row refuses it — `Fanout.Anchor` gains no partition coordinate because a partition-and-offset pair is broker-local and leaks this engine's shape onto every row, and a fan of per-partition reads merged into one stream answers an ORDER no partition holds; `Fanout.Replayed`'s coordinate is a per-record fact and stays honest where the merged stream's sequence does not. Callers needing a positioned re-read read the data journal, which is the system of record the retention window was never allowed to replace.
- Packages: `@confluentinc/kafka-javascript` (`KafkaJS.Kafka`, `KafkaJS.Logger`, `KafkaJS.RecordMetadata`, `KafkaJS.SASLOptions`, `KafkaJS.OauthbearerProviderResponse`), `@confluentinc/schemaregistry`, `cloudevents` (`Kafka`, `CloudEvent`, `CONSTANTS`), `effect` (`DateTime`, `PubSub`, `Queue`, `Redacted`, `Stream`), `@rasm/core` (`Carrier`, `Event`), `@rasm/security` (`MachinePrincipal`), `./client.ts` (`Machine`), and `../proc/config.ts`.
- Boundary: broker deployment — partitions, replication, retention, TLS posture — is the deploy plane's; the bootstrap roster is a `Setting` row and no broker literal exists in the engine. The SASL identity is not a deploy row here: it is the machine principal this engine projects, so a static `sasl.username`/`password` pair beside the provider would be the hand-carried credential the projection deletes. `ssl` stays unset until a `Setting.fanout` row carries the transport posture `proc/config` owns.

```typescript signature
const _kafka = (
    topics: Fanout.Topics,
    contracts: Readonly<Record<string, _KafkaContract>>,
    generation: Option.Option<Backend.Generation>,
): Layer.Layer<Fanout, FanoutFault, Setting> =>
    Layer.scoped(
        Fanout,
        Effect.gen(function* () {
            const setting = yield* Setting;
            const registryOrigin = yield* Option.match(setting.fanout.registry, {
                onNone: () => Effect.fail(new FanoutFault({ case: { reason: 'dial', topic: '*', detail: '<no-schema-registry-origin>' } })),
                onSome: Effect.succeed,
            });
            const topicKeys = Object.keys(topics).sort();
            const contractKeys = Object.keys(contracts).sort();
            yield* setting.fanout.brokers.length === 0
                ? Effect.fail(new FanoutFault({ case: { reason: 'dial', topic: '*', detail: '<empty-broker-roster>' } }))
                : topicKeys.length !== contractKeys.length || topicKeys.some((topic, index) => topic !== contractKeys[index])
                ? Effect.fail(
                      new FanoutFault({
                          case: {
                              reason: 'dial',
                              topic: '*',
                              detail: `<topic-contract-roster-drift:${topicKeys.join('|')}!=${contractKeys.join('|')}>`,
                          },
                      }),
                  )
                : Effect.void;
            // one unbounded cell every minted client's Logger writes into: the `pulse` stream drains it, so a producer's,
            // a consumer's, and the admin's transport faults arrive on one rail instead of vanishing into a default logger
            const beat = yield* Effect.acquireRelease(
                PubSub.unbounded<FanoutFault>(),
                (live) => PubSub.shutdown(live),
            );
            const logger = _kafkaLogger((detail) => {
                Queue.unsafeOffer(beat, new FanoutFault({ case: { reason: 'dial', topic: '*', detail } }));
            });
            // The sorted roster IS the cluster identity; declaration order is not, and keying on the raw array would
            // mint a second audience for the same brokers listed differently.
            const audience = [...setting.fanout.brokers].sort().join(',');
            const _principal = Effect.mapError(
                Machine.at('fanout:kafka', audience),
                (lapse) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: `<credential-${lapse.reason}:${audience}>` } }),
            );
            // One boot read decides whether the mechanism is armed at all: a `mechanism` set with no token to carry
            // refuses every handshake, where an absent `sasl` dials exactly as an unauthenticated estate does today.
            // From then on the provider is the rotation rail — librdkafka re-runs it on its OWN refresh cadence and
            // sets the token on the live client, so this row rotates without replacing a connection.
            const armed: { readonly sasl?: KafkaJS.SASLOptions } = Option.match(yield* _principal, {
                onNone: () => ({}),
                onSome: () => ({
                    sasl: {
                        mechanism: 'oauthbearer',
                        oauthBearerProvider: () =>
                            Effect.runPromise(
                                Effect.flatMap(
                                    _principal,
                                    Option.match({
                                        // a source that empties mid-life rejects here, which is what makes the wrapper call
                                        // `setOAuthBearerTokenFailure` and emit on `error` — so the refusal lands on `pulse`
                                        onNone: () =>
                                            Effect.fail(
                                                new FanoutFault({ case: { reason: 'dial', topic: '*', detail: `<credential-withdrawn:${audience}>` } }),
                                            ),
                                        onSome: (principal: MachinePrincipal) =>
                                            Effect.succeed({
                                                // BARE: RFC 7628 frames `auth=Bearer <token>` itself, so `credential` double-prefixes
                                                value: Redacted.value(principal.token),
                                                principal: principal.clientId,
                                                // ABSOLUTE epoch millis, never a remaining span: librdkafka reads `md_lifetime_ms`
                                                // as when the token expires since the epoch, so a duration dates it to 1970
                                                lifetime: DateTime.toEpochMillis(principal.expiresAt),
                                                extensions: {},
                                            } satisfies KafkaJS.OauthbearerProviderResponse),
                                    }),
                                ),
                            ),
                    },
                }),
            });
            const kafka = new KafkaJS.Kafka({ kafkaJS: { brokers: [...setting.fanout.brokers], logger, ...armed } });
            const producer = yield* Effect.acquireRelease(
                Effect.tryPromise({
                    try: async () => {
                        const minted = kafka.producer({ kafkaJS: { logger } });
                        await minted.connect();
                        return minted;
                    },
                    catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: String(cause) } }),
                }),
                (live) => Effect.orDie(Effect.tryPromise(() => live.disconnect())),
            );
            const admin = yield* Effect.acquireRelease(
                Effect.tryPromise({
                    try: async () => {
                        const minted = kafka.admin({ kafkaJS: { logger } });
                        await minted.connect();
                        return minted;
                    },
                    catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: String(cause) } }),
                }),
                (live) => Effect.orDie(Effect.tryPromise(() => live.disconnect())),
            );
            const registry = yield* Effect.acquireRelease(
                Effect.try({
                    try: () => new SchemaRegistryClient({ baseURLs: [registryOrigin.href] }),
                    catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: String(cause) } }),
                }),
                (live) => Effect.sync(() => live.close()),
            );
            const admitted = yield* Effect.forEach(
                Object.entries(contracts),
                ([topic, contract]) =>
                    Effect.gen(function* () {
                        // Every contract naming an artifact demands the bound generation port on the providers axis, so
                        // an unbound port refuses by axis name and a bound one proves declared and observed both carry it.
                        yield* Option.match(contract.artifact, {
                            onNone: () => Effect.void,
                            onSome: (artifact) =>
                                Option.match(generation, {
                                    onNone: () =>
                                        Effect.fail(
                                            new FanoutFault({
                                                case: {
                                                    reason: 'dial',
                                                    topic,
                                                    detail: `<providers-axis-unbound:backend-generation-for-${artifact}>`,
                                                },
                                            }),
                                        ),
                                    onSome: (proved) =>
                                        HashSet.has(proved.artifacts, artifact) && HashSet.has(proved.observed.artifacts, artifact)
                                            ? Effect.void
                                            : Effect.fail(
                                                  new FanoutFault({ case: { reason: 'dial', topic, detail: `<artifact-unobserved:${artifact}>` } }),
                                              ),
                                }),
                        });
                        const [metadata, compatibility, compatible, id, writer] = yield* Effect.tryPromise({
                            try: () => Promise.all([
                                registry.getSchemaMetadata(contract.subject, contract.version, false),
                                registry.getCompatibility(contract.subject),
                                registry.testSubjectCompatibility(contract.subject, contract.schema),
                                registry.getId(contract.subject, contract.schema, true),
                                registry.getBySubjectAndId(contract.subject, contract.id),
                            ]),
                            catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                        });
                        const rules = yield* Effect.acquireRelease(
                            Effect.try({
                                try: contract.rules,
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                            (live) => Effect.sync(() => live.clear()),
                        );
                        const declaredRules = [
                            ...(metadata.ruleSet?.migrationRules ?? []),
                            ...(metadata.ruleSet?.domainRules ?? []),
                        ];
                        // one identity roster replaces the boolean ladder: each row names the axis it proves, so a
                        // refusal reports exactly which axes drifted rather than one undifferentiated tag.
                        const drift = (
                            [
                                ['metadata-id', metadata.id === contract.id],
                                ['metadata-version', metadata.version === contract.version],
                                ['compatibility', compatibility === contract.compatibility],
                                ['subject-compatible', compatible],
                                ['registered-id', id === contract.id],
                                ['metadata-schema', metadata.schema === contract.schema.schema],
                                ['metadata-schema-type', metadata.schemaType === contract.schema.schemaType],
                                ['writer-schema', writer.schema === contract.schema.schema],
                                ['writer-schema-type', writer.schemaType === contract.schema.schemaType],
                                ['rule-executors', declaredRules.every((rule) => rules.getExecutor(rule.type) !== undefined)],
                                ['rule-actions', contract.actions.every((action) => rules.getAction(action) !== undefined)],
                            ] as const
                        ).flatMap(([axis, holds]) => (holds ? [] : [axis]));
                        yield* drift.length === 0
                            ? Effect.void
                            : Effect.fail(new FanoutFault({ case: { reason: 'dial', topic, detail: `<contract-drift:${drift.join(',')}>` } }));
                        const codec = yield* Effect.acquireRelease(
                            Effect.try({
                                try: (): _KafkaCodec => {
                                    const wireTopic = topics[topic]!.subject;
                                    const subjectNameStrategy = () => contract.subject;
                                    // one lookup over the family table: `validate` rides the JSON arm's own config extension and the other two ignore it
                                    const [Encoder, Decoder] = _KAFKA_CODECS[contract.schema.schemaType];
                                    const writing = {
                                        autoRegisterSchemas: false,
                                        useSchemaId: contract.id,
                                        normalizeSchemas: true,
                                        subjectNameStrategy,
                                        validate: true,
                                    };
                                    const reading = { subjectNameStrategy, validate: true };
                                    const serializer = new Encoder(registry, SerdeType.VALUE, writing, rules);
                                    const deserializer = new Decoder(registry, SerdeType.VALUE, reading, rules);
                                    return {
                                        // Registry coordinates belong to the contract row, payload bytes to the serde, and the
                                        // CloudEvents header band plus record key to the official Kafka binding.
                                        lower: (logical, event) =>
                                            Effect.gen(function* () {
                                                const admitted = yield* _announced(logical, event);
                                                const framed = yield* Effect.tryPromise({
                                                    try: () => serializer.serialize(wireTopic, admitted.data),
                                                    catch: (cause) =>
                                                        new FanoutFault({ case: { reason: 'publish', topic: logical, detail: String(cause) } }),
                                                });
                                                const sealed = yield* Event.clone(admitted, {
                                                    data: framed,
                                                    datacontenttype: CONSTANTS.MIME_OCTET_STREAM,
                                                }).pipe(
                                                    Effect.mapError((refusal) =>
                                                        new FanoutFault({ case: { reason: 'publish', topic: logical, detail: refusal.message } })),
                                                );
                                                const message = yield* Effect.try({
                                                    try: () => Kafka.binary<unknown>(sealed),
                                                    catch: (cause) =>
                                                        new FanoutFault({ case: { reason: 'publish', topic: logical, detail: String(cause) } }),
                                                });
                                                const partition = yield* _partitionKey(logical, admitted);
                                                return {
                                                    key: typeof message.key === 'string' ? message.key : partition,
                                                    value: Buffer.from(framed),
                                                    band: Carrier.record.write(Carrier.record.read(message.headers)),
                                                };
                                            }),
                                        raise: (logical, key, payload, headers) =>
                                            Effect.gen(function* () {
                                                const schemaId = yield* Effect.try({
                                                    try: () => {
                                                        const frame = new SchemaId(contract.schema.schemaType);
                                                        frame.fromBytes(payload);
                                                        return frame.id;
                                                    },
                                                    catch: (cause) =>
                                                        new FanoutFault({ case: { reason: 'poison', topic: logical, detail: String(cause) } }),
                                                });
                                                yield* schemaId === contract.id
                                                    ? Effect.void
                                                    : Effect.fail(
                                                          new FanoutFault({
                                                              case: {
                                                                  reason: 'poison',
                                                                  topic: logical,
                                                                  detail: `<schema-id-drift:${schemaId}!=${contract.id}>`,
                                                              },
                                                          }),
                                                      );
                                                const data = yield* Effect.tryPromise({
                                                    try: () => deserializer.deserialize(wireTopic, payload, headers),
                                                    catch: (cause) =>
                                                        new FanoutFault({ case: { reason: 'poison', topic: logical, detail: String(cause) } }),
                                                });
                                                const envelope = yield* Effect.try({
                                                    try: () => Kafka.toEvent<unknown>({ key, value: payload, headers }),
                                                    catch: (cause) =>
                                                        new FanoutFault({ case: { reason: 'poison', topic: logical, detail: String(cause) } }),
                                                });
                                                const one = yield* (globalThis.Array.isArray(envelope)
                                                    ? Effect.fail(
                                                          new FanoutFault({
                                                              case: { reason: 'poison', topic: logical, detail: '<batch-unsupported:kafka>' },
                                                          }),
                                                      )
                                                    : Event.admit(envelope).pipe(
                                                          Effect.mapError((refusal) =>
                                                              new FanoutFault({
                                                                  case: { reason: 'poison', topic: logical, detail: refusal.message },
                                                              })),
                                                      ));
                                                const expectedKey = yield* _partitionKey(logical, one, 'poison');
                                                yield* key === null || key.toString('utf8') === expectedKey
                                                    ? Effect.void
                                                    : Effect.fail(
                                                          new FanoutFault({
                                                              case: {
                                                                  reason: 'poison',
                                                                  topic: logical,
                                                                  detail: `<record-key-drift:${key.toString('utf8')}>`,
                                                              },
                                                          }),
                                                      );
                                                return yield* Event.clone(one, { data }).pipe(
                                                    Effect.mapError((refusal) =>
                                                        new FanoutFault({ case: { reason: 'poison', topic: logical, detail: refusal.message } })),
                                                );
                                            }),
                                        close: () => {
                                            serializer.close();
                                            deserializer.close();
                                        },
                                    };
                                },
                                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                            }),
                            (live) => Effect.sync(() => live.close()),
                        );
                        return [topic, codec] as const;
                    }),
                { concurrency: 'unbounded' },
            );
            const codecs: Readonly<Record<string, _KafkaCodec>> = Record.fromEntries(admitted);
            yield* Effect.tryPromise({
                try: () => admin.createTopics({ topics: Record.values(topics).map((row) => ({ topic: row.subject })) }),
                catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic: '*', detail: String(cause) } }),
            });

            const named = (topic: string): Effect.Effect<Fanout.Topic, FanoutFault> => _named(topics, topic);

            // Read-process-write registry: a live consume lane publishes its consumer handle and the position it is currently
            // processing, which is exactly the pair `sendOffsets` binds — nothing else can produce a KIP-447 offset handoff
            const lanes = yield* Ref.make(HashMap.empty<string, _KafkaLane>());

            const receipted = (
                topic: string,
                subject: string,
                key: string,
                landed: KafkaJS.RecordMetadata | undefined,
            ): Effect.Effect<Fanout.Receipt, FanoutFault> =>
                // delivery reports fill `baseOffset` and NEVER `offset`, though the compat type declares both optional:
                // reading `offset` answers undefined for every landed record and turns each success into a phantom refusal
                landed?.baseOffset === undefined
                    ? Effect.fail(new FanoutFault({ case: { reason: 'publish', topic, detail: '<no-broker-ack-metadata>' } }))
                    : Effect.succeed(
                          new _Receipt({
                              topic,
                              subject,
                              key,
                              position: { _tag: 'PartitionOffset', partition: landed.partition, offset: landed.baseOffset },
                              duplicate: false,
                          }),
                      );

            const run = async <A>(
                effect: Effect.Effect<A, FanoutFault>,
                topic: string,
            ): Promise<A> => {
                const exit = await Effect.runPromiseExit(effect);
                return Exit.match(exit, {
                    onFailure: (cause) => {
                        throw Option.getOrElse(
                            Cause.failureOption(cause),
                            () => new FanoutFault({ case: { reason: 'dial', topic, detail: '<defect-or-interrupt>' } }),
                        );
                    },
                    onSuccess: (value) => value,
                });
            };

            const consumed = (
                topic: string,
                group: string,
                handler: (event: Fanout.Announced) => Effect.Effect<void, FanoutFault>,
            ): Effect.Effect<void, FanoutFault> =>
                Effect.flatMap(
                    Effect.all({ codec: _kafkaNamed(codecs, topic), row: named(topic) }),
                    ({ codec, row }) =>
                        Effect.acquireUseRelease(
                            Effect.tap(
                                Effect.tryPromise({
                                    try: async () => {
                                        const minted = kafka.consumer({ kafkaJS: { groupId: group, autoCommit: false, logger } });
                                        await minted.connect();
                                        await minted.subscribe({ topic: row.subject });
                                        // Transactional id IS the lane identity, so a restarted replica fences its own zombie predecessor
                                        const writer = kafka.producer({ kafkaJS: { transactionalId: group, idempotent: true, logger } });
                                        await writer.connect();
                                        return { consumer: minted, producer: writer };
                                    },
                                    catch: (cause) => new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                }),
                                ({ consumer, producer }) =>
                                    Ref.update(lanes, (held) => HashMap.set(held, group, { consumer, producer, position: Option.none() })),
                            ),
                            ({ consumer }) =>
                                Effect.async<void, FanoutFault>((resume) => {
                                    // `run` throws SYNCHRONOUSLY on a state refusal before any promise exists, so the guard
                                    // is what keeps that arm on the rail instead of tearing the fiber down as a defect
                                    try {
                                        void consumer
                                            .run({
                                                eachBatchAutoResolve: false,
                                                partitionsConsumedConcurrently: 1,
                                                eachBatch: async ({ batch, heartbeat, isRunning, isStale, resolveOffset }) => {
                                                    for (const message of batch.messages) {
                                                        if (!isRunning() || isStale()) return;
                                                        const frame = Carrier.record.read(message.headers ?? {});
                                                        const event = await run(
                                                            codec.raise(
                                                                topic,
                                                                message.key,
                                                                Buffer.from(message.value ?? new Uint8Array()),
                                                                message.headers ?? {},
                                                            ),
                                                            topic,
                                                        );
                                                        await run(
                                                            Ref.update(lanes, (live) =>
                                                                HashMap.modify(live, group, (lane) => ({
                                                                    ...lane,
                                                                    // Whatever `sendOffsets` binds is the NEXT offset, exactly the manual commit below
                                                                    position: Option.some({
                                                                        topic: batch.topic,
                                                                        partitions: [
                                                                            {
                                                                                partition: batch.partition,
                                                                                offset: (BigInt(message.offset) + 1n).toString(),
                                                                            },
                                                                        ],
                                                                    }),
                                                                })),
                                                            ),
                                                            topic,
                                                        );
                                                        await run(
                                                            Propagation.ingress(handler(event), Carrier.extract('kafka', frame)).pipe(
                                                                // Ledger compilation carries jitter, reset, and the elapsed window; the topic's own
                                                                // redelivery ceiling intersects it, and the class gate refuses to re-drive a poison record
                                                                Effect.retry(
                                                                    Schedule.intersect(
                                                                        Fault.Budget.schedule(row.budget),
                                                                        Schedule.recurs(row.attempts - 1),
                                                                    ),
                                                                ),
                                                            ),
                                                            topic,
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
                                            .catch((cause) =>
                                                resume(
                                                    Effect.fail(
                                                        cause instanceof FanoutFault
                                                            ? cause
                                                            : new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } }),
                                                    ),
                                                ));
                                    } catch (cause) {
                                        resume(Effect.fail(new FanoutFault({ case: { reason: 'dial', topic, detail: String(cause) } })));
                                    }
                                }),
                            ({ consumer, producer }) =>
                                Effect.zipRight(
                                    Ref.update(lanes, (held) => HashMap.remove(held, group)), // the lane leaves the registry before its handles close, so atomic can never reach a dead producer
                                    Effect.orDie(Effect.tryPromise(() => Promise.all([consumer.disconnect(), producer.disconnect()]))),
                                ),
                        ),
                );

            return {
                publish: (topic, event, post = new _Post({})) =>
                    Effect.flatMap(
                        Effect.all({
                            // Kafka keys partition and never dedup, so there is no last-sequence to compare an
                            // optimistic expectation against; `_expects` refuses it off the row rather than letting
                            // a caller's guard vanish into a send that was never conditional.
                            _guard: _expects('kafka', topic, post),
                            codec: _kafkaNamed(codecs, topic),
                            context: Propagation.current,
                            row: named(topic),
                        }),
                        ({ codec, context, row }) =>
                            Effect.flatMap(
                                codec.lower(topic, event),
                                (lowered) =>
                                    Effect.tryPromise({
                                        try: () => producer.send({
                                            topic: row.subject,
                                            messages: [{
                                                key: lowered.key,
                                                value: lowered.value,
                                                // Binding names prefix `ce_` and the hop carrier writes bare W3C keys, so the
                                                // injection lands beside the attribute band rather than over it.
                                                headers: Carrier.record.write(
                                                    Carrier.inject('kafka', context, Carrier.record.read(lowered.band)),
                                                ),
                                            }],
                                        }),
                                        catch: (cause) => new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                                    }),
                            ).pipe(
                                Effect.flatMap((metadata) =>
                                    Effect.flatMap(_partitionKey(topic, event), (key) =>
                                        receipted(topic, row.subject, key, metadata[0]))),
                            ),
                    ),
                atomic: (topic, consumer, events) =>
                    Effect.flatMap(
                        Effect.all({
                            codec: _kafkaNamed(codecs, topic),
                            context: Propagation.current,
                            row: named(topic),
                            live: Ref.get(lanes),
                        }),
                        ({ codec, context, row, live }) =>
                            Option.match(HashMap.get(live, `${topic}:${consumer}`), {
                                // atomic is meaningless outside a live read-process-write lane: the offset half has no source without one
                                onNone: () =>
                                    Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<no-live-consume-lane:${consumer}>` } })),
                                onSome: (lane) =>
                                    Option.match(lane.position, {
                                        onNone: () =>
                                            Effect.fail(new FanoutFault({ case: { reason: 'horizon', topic, detail: `<lane-holds-no-offset:${consumer}>` } })),
                                        onSome: (position) =>
                                            Effect.flatMap(
                                                Effect.forEach(events, (event) => codec.lower(topic, event), { concurrency: 'inherit' }),
                                                (lowered) =>
                                                    Effect.acquireUseRelease(
                                                        Effect.tryPromise({
                                                            try: () => lane.producer.transaction(),
                                                            catch: (cause) =>
                                                                new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                                                        }),
                                                        (txn) =>
                                                            Effect.tryPromise({
                                                                try: async () => {
                                                                    // ONE send per envelope, in offer order: a send carrying N messages folds
                                                                    // its reports to one row PER TOPIC-PARTITION at the minimum baseOffset, so
                                                                    // indexing that array by envelope pairs a receipt with a position it never held
                                                                    const landed: KafkaJS.RecordMetadata[] = [];
                                                                    for (const framed of lowered) {
                                                                        const [metadata] = await txn.send({
                                                                            topic: row.subject,
                                                                            messages: [{
                                                                                key: framed.key,
                                                                                value: framed.value,
                                                                                headers: Carrier.record.write(
                                                                                    Carrier.inject(
                                                                                        'kafka',
                                                                                        context,
                                                                                        Carrier.record.read(framed.band),
                                                                                    ),
                                                                                ),
                                                                            }],
                                                                        });
                                                                        landed.push(metadata!);
                                                                    }
                                                                    // Offset handoff joins the produced records: commit publishes both or neither
                                                                    await txn.sendOffsets({ consumer: lane.consumer, topics: [position] });
                                                                    await txn.commit();
                                                                    return landed;
                                                                },
                                                                catch: (cause) =>
                                                                    new FanoutFault({ case: { reason: 'publish', topic, detail: String(cause) } }),
                                                            }),
                                                        (txn, exit) =>
                                                            Exit.isSuccess(exit)
                                                                ? Effect.void
                                                                : Effect.orDie(Effect.tryPromise(() => txn.abort())), // a failed or interrupted unit aborts: no half-published batch survives
                                                    ).pipe(
                                                        Effect.flatMap((landed) =>
                                                            Effect.forEach(events, (event, index) =>
                                                                Effect.flatMap(_partitionKey(topic, event), (key) =>
                                                                    receipted(topic, row.subject, key, landed[index])),
                                                            ),
                                                        ),
                                                    ),
                                            ),
                                    }),
                            }),
                    ),
                // `serves.consumers` reads false where the local row's reads true on an empty roster: a consumer group
                // IS held by the substrate here, so reporting no consumers would forge a census this client cannot take.
                consumers: (topic) => Effect.fail(_absent('kafka', 'consumers', topic)),
                pulse: Stream.fromPubSub(beat),
                alias: (topic) => Effect.fail(_absent('kafka', 'alias', topic)),
                subscribe: (topic) => Stream.fail(_absent('kafka', 'subscribe', topic)),
                consume: (topic, consumer, anchor, handler) =>
                    _admits('kafka', anchor)
                        ? consumed(topic, `${topic}:${consumer}`, handler)
                        : Effect.fail(_absent('kafka', 'consume', topic, `:${anchor._tag}`)),
                replay: (topic) => Stream.fail(_absent('kafka', 'replay', topic)),
                stash: (topic) => Effect.fail(_absent('kafka', 'stash', topic)),
                haul: (topic) => Stream.fail(_absent('kafka', 'haul', topic)),
            };
        }),
    );
```

```typescript signature
// --- [EXPORTS] --------------------------------------------------------------------------

export { Broker, Fanout, FanoutFault };
```

## [08]-[RESEARCH]

(none)
