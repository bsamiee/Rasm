# [RUNTIME_PUBSUB]

Fanout and replay are one port with engines as rows: `Fanout` is the broadcast plane the branch composes wherever every subscriber must see every value — live evidence mirrors, presence, cross-process invalidation, large-binary handoff — with an in-process row over the effect `PubSub` replay window, a browser cross-tab row bridging the same cells over `BroadcastChannel`, and a `jetstream` row over NATS for genuine cross-process fanout, bounded replay, dedup, and chunked blob passing. The guarantee ledger is data: at-least-once handler consumption on a durable explicit-ack consumer with ack-after-success, poison termination, and long-handler heartbeats; exactly-once publish inside a duplicate-detection window keyed by the producer's content-derived `msgID`; double-ack confirmation where a topic row demands it; replay anchored by sequence or instant on an ordered consumer that carries no ack surface at all — each a row the jetstream engine upholds and the lighter rows honestly degrade, so selecting an engine is a Layer choice at the root and the degradation is readable off one table. The stream is NEVER the system of record: retention bounds every topic, replay is a warm-up and recovery window, and a consumer needing full history reads the data wave's journal. The NATS server's durability posture — fsync interval hardening, replica quorum — is a deployment fact the deploy plane owns. The module ships on the `./server` subpath for the jetstream row; the local row is runtime-neutral and the tab row is the browser condition. The module is `runtime/src/net/pubsub.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                              | [PUBLIC]                     |
| :-----: | :-------------- | :---------------------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `TOPIC_ROWS`    | the topic policy vocabulary: subject, retention, replay, ack posture, redelivery    | `Fanout`                     |
|  [02]   | `PORT_SHAPE`    | the engine-neutral port — publish, subscribe, consume, replay, stash, haul — faults | `Fanout`, `FanoutFault`      |
|  [03]   | `LOCAL_ROW`     | the in-process engine over `PubSub` replay plus the in-process blob shelf           | `Fanout.local`               |
|  [04]   | `TAB_ROW`       | the browser cross-tab engine — `BroadcastChannel` bridge decorating the local cells | `Fanout.tab`                 |
|  [05]   | `JETSTREAM_ROW` | the NATS engine: ordered vs durable lanes, dedup, double-ack, heartbeat, blob store | `Fanout.jetstream`, `Broker` |

## [02]-[TOPIC_ROWS]

[TOPIC_ROWS]:

- Owner: `Fanout.Topic` — the `Schema.Class` policy authority carrying `subject` (the wire address), `retention` (the stream age bound — the never-system-of-record ceiling), `replay` (the non-negative warm-up window a late subscriber receives), `ack` (`"fire" | "double"` — whether consumption confirms the acknowledgement itself), `wait` (the ack deadline the durable lane declares as `ack_wait` and halves into the long-handler heartbeat cadence), and positive `attempts` (the redelivery ceiling declared as `max_deliver` — beyond it the server parks the message); the dedup window reads `Setting.fanout.dedup` and the blob chunk size `Setting.fanout.chunk`, so topic policy decodes once as root data and the engine reads admitted rows, never knobs.
- Law: the publish identity is content-derived — the producer mints the `key` from the kernel content-key mint or the envelope's `Hlc` stamp, so a replayed publish inside the dedup window is recognized by value identity in every language; the engine never invents identity.
- Law: the envelope is transport-only — opaque octets, the identity key, and an optional closed optimistic expectation (`LastMessage | Stream | LastSequence | LastSubjectSequence | SubjectSequence`); the consumer's own Schema decodes the body at its seam (event stamps ride the payload vocabulary), while the engine projects the expectation into its publish primitive and the `Fanout.Receipt` / `Fanout.Stowed` `Schema.Class` authorities bind returned sequence, duplicate, extent, and digest evidence without parallel DTOs.
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
import { type NatsConnection, wsconnect } from '@nats-io/nats-core';
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
import type { FaultClass } from '@rasm/ts/core';
import { Setting } from '../proc/config.ts';
import { Breaker } from './client.ts';

class Envelope extends Schema.Class<Envelope>('Envelope')({
    key: Schema.NonEmptyString,
    body: Schema.Uint8ArrayFromSelf,
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

class _Receipt extends Schema.Class<_Receipt>('Fanout/Receipt')({
    topic: Schema.NonEmptyString,
    subject: Schema.NonEmptyString,
    key: Schema.NonEmptyString,
    seq: Schema.NonNegativeInt,
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
    type Receipt = _Receipt;
    type Stowed = _Stowed;
    type LocalPolicy = _LocalPolicy;
}

const _Anchor = Data.taggedEnum<Fanout.Anchor>();
const _blobKey = Schema.encodeSync(Schema.parseJson(Schema.Tuple(Schema.String, Schema.String)));
```

## [03]-[PORT_SHAPE]

[PORT_SHAPE]:

- Owner: the `Fanout` Tag — six members over the topic key. The envelope lane: `publish(topic, envelope)` yields the evidence receipt; `subscribe(topic)` is the live fanout stream with the topic's replay window warming a late attach; `consume(topic, consumer, anchor, handler)` is the at-least-once lane whose explicit consumer identity derives a distinct durable name, preventing independent logical subscribers from accidentally load-balancing one durable consumer; `replay(topic, anchor)` re-reads within retention. The blob lane streams transient large-binary handoff through `stash` / `haul`, never a second content-addressing vocabulary or durable store.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `horizon` (the anchor precedes the engine's window, the topic is undeclared, or the blob is absent — class `absent`), `publish` (an unacknowledged publish or a rejected stash, class `unavailable`), `poison` (the handler proves an envelope unprocessable, class `malformed` — the consume lane's terminate signal) — so the core budget gate re-drives the transient rows and a horizon miss routes as the terminal evidence it is.
- Law: delivery semantics are the row's, not the call site's — a consumer never re-states ack posture, replay depth, retention, or redelivery ceiling; it names the topic and the engine answers the row; an unknown topic answers `horizon` identically on every engine and every member.
- Law: the port is engine-blind — no member names NATS, and swapping any row for another edits the root merge and nothing else; the engine roster law is the services doctrine's, instantiated here.
- Entry: `yield* Fanout` then the six members; engines land as `Fanout.local(topics)` / `Fanout.tab(topics)` / `Fanout.jetstream(topics)` root Layers.
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
        readonly replay: (topic: string, anchor: Fanout.Anchor) => Stream.Stream<Envelope, FanoutFault>;
        readonly stash: (topic: string, name: string, body: Stream.Stream<Uint8Array, FanoutFault>) => Effect.Effect<Fanout.Stowed, FanoutFault>;
        readonly haul: (topic: string, name: string) => Stream.Stream<Uint8Array, FanoutFault>;
    }
>() {
    static readonly Anchor = _Anchor;
    static readonly Envelope = Envelope;
    static readonly Topic = _Topic;
    static readonly Receipt = _Receipt;
    static readonly Stowed = _Stowed;
    static readonly LocalPolicy = _LocalPolicy;
    static readonly local = (topics: Fanout.Topics, policy: Fanout.LocalPolicy = new _LocalPolicy({})): Layer.Layer<Fanout> => _local(topics, policy);
    static readonly tab = (topics: Fanout.Topics, policy: Fanout.LocalPolicy = new _LocalPolicy({})): Layer.Layer<Fanout> => _tab(topics, policy);
    static readonly jetstream = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting | Broker> => _jetstream(topics);
}
```

## [04]-[LOCAL_ROW]

[LOCAL_ROW]:

- Owner: `Fanout.local(topics, policy)` — one scoped `PubSub.bounded<Envelope>({ capacity: policy.capacity, replay: row.replay })` per topic row plus one `Ref`-held aggregate blob shelf; `publish` offers and advances the topic's own sequence counter (a local publish is never a duplicate — the dedup window is a server guarantee the local row honestly lacks), `subscribe` is the scoped `Stream.fromPubSub` whose late attach receives the replay window, `consume` folds the same stream through the handler with the ack posture vacuous (in-process delivery has no redelivery to confirm or terminate), `replay` snapshots the current sequence count and takes exactly the retained warm-up prefix for the `Window` anchor, while a `Sequence` or `Instant` anchor folds to `horizon`; the blob lane folds into the shelf under `policy.shelf`, replaces a held key without double-counting its prior body, and streams out keyed `topic/name`, digest honestly absent.
- Law: the degradation is the table read aloud — replay-is-a-bounded warm-up, no cross-process reach, no dedup window, no durable redelivery, no digest evidence, and the blob shelf is process memory under the admitted `Fanout.LocalPolicy.shelf` aggregate byte ceiling: a stash crossing it refuses with the `publish` fault, never exhausts memory, so the general large-binary contract is the jetstream row's; a proof or a single-process deployment selects this row deliberately, and promoting a workload that needs the missing rows is a root Layer swap, never a local re-implementation.
- Law: capacity backpressures — the bounded construction suspends a producer ahead of the slowest subscriber's window; a sliding local topic is a row decision, never a default.
- Packages: `effect` (`PubSub`, `Stream`, `Layer`, `Record`, `Ref`, `HashMap`, `Chunk`).

```typescript signature
type _Port = Context.Tag.Service<Fanout>;

const _minted = (topics: Fanout.Topics, policy: Fanout.LocalPolicy): Effect.Effect<_Port, never, Scope.Scope> =>
    Effect.gen(function* () {
        const cells = yield* Effect.all(
            Record.map(topics, (row) => PubSub.bounded<Envelope>({ capacity: policy.capacity, replay: row.replay })),
            { concurrency: 'inherit' },
        );
        const shelf = yield* Ref.make({
            size: 0,
            bodies: HashMap.empty<string, { readonly size: number; readonly chunks: Chunk.Chunk<Uint8Array> }>(),
        });
        const seqs = yield* Ref.make(HashMap.empty<string, number>());
        const held = (topic: string): Effect.Effect<PubSub.PubSub<Envelope>, FanoutFault> =>
            Option.match(Option.fromNullable(cells[topic]), {
                onNone: () => Effect.fail(new FanoutFault({ reason: 'horizon', topic })),
                onSome: Effect.succeed,
            });
        return {
            publish: (topic, envelope) =>
                Effect.flatMap(held(topic), (cell) =>
                    Effect.zipRight(
                        PubSub.publish(cell, envelope),
                        Ref.modify(seqs, (counts) => {
                            const next = Option.getOrElse(HashMap.get(counts, topic), () => 0) + 1;
                            return [
                                new _Receipt({ topic, subject: topics[topic]?.subject ?? topic, key: envelope.key, seq: next, duplicate: false }),
                                HashMap.set(counts, topic, next),
                            ] as const;
                        }),
                    ),
                ),
            subscribe: (topic) => Stream.unwrap(Effect.map(held(topic), (cell) => Stream.fromPubSub(cell))),
            consume: (topic, _consumer, anchor, handler) =>
                _Anchor.$is('Window')(anchor)
                    ? Stream.runForEach(Stream.unwrap(Effect.map(held(topic), (cell) => Stream.fromPubSub(cell))), handler)
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
                        (data) => Effect.ignore(Effect.flatMap(Schema.decodeUnknown(Envelope)(data), (envelope) => inner.publish(topic, envelope))),
                    ).pipe(Effect.forkScoped),
                { concurrency: 'inherit', discard: true },
            );
            return {
                ...inner,
                publish: (topic, envelope) =>
                    Effect.tap(inner.publish(topic, envelope), () =>
                        Effect.sync(() => posts[topic]?.postMessage(Schema.encodeSync(Envelope)(envelope))),
                    ),
            };
        }),
    );
```

## [06]-[JETSTREAM_ROW]

[JETSTREAM_ROW]:

- Owner: `Fanout.jetstream(topics)` — the NATS engine. The connection is capability: the exported `Broker` Tag holds the one scoped `wsconnect({ servers })` against `Setting.fanout.origin`, drained on scope close, and the one connection fans into the stream lanes, the object store, and the sibling coordination engine (`coordinate#KV_ROW`) — a second dial beside `Broker.live` is the named defect. Construction reconciles the substrate: `jetstreamManager(nc)` inspects each topic stream, adds an absent stream, and updates a present stream's mutable retention, subject, and dedup policy; `Objm.create` creates or opens the blob store. Restart is therefore convergence, never a duplicate-create failure, while the server's own durability posture (fsync interval, replicas) stays a deployment fact.
- Law: the consumer lanes are split by ack capability — the ordered lane (`subscribe`, `replay`) mints a nameless ordered consumer fixed to `AckPolicy.None`; the durable lane (`consume`) derives `durable_name` from topic plus the caller's logical consumer identity, declares explicit ack posture, and binds that same name. Independent consumers therefore receive independent durable streams, while replicas sharing one identity intentionally load-balance.
- Law: exactly-once publish is the dedup window under the circuit — `js.publish` carries the content-derived `msgID` and the envelope's optional expectation projected through `_expected`; the server recognizes a replay inside `duplicate_window`, enforces every `StreamExpectations` arm, and returns a `PubAck` whose sequence and `duplicate` flag ride a receipt bound to the addressed topic, subject, and key; the whole publish rides `Breaker.guard` so a dead broker sheds fast instead of hammering.
- Law: at-least-once is the full ack algebra — the handler runs under a heartbeat race that stamps `msg.working()` every half `ack_wait` so a long handler never triggers spurious redelivery; success acks — `ack()` on `"fire"` rows, `ackAck()` awaited on `"double"` rows so the acknowledgement itself is confirmed; a `poison` handler fault terminates through `msg.term(reason)`, every other handler fault `nak()`s, and only that ruled handler-failure branch returns `Effect.void`; an `ackAck()` rejection remains a `dial` fault on the consume rail and cannot be mistaken for handled work.
- Law: replay is bounded honesty — `replay(topic, anchor)` snapshots `StreamInfo.state`, rejects a `Sequence` before `first_seq` or an `Instant` before `first_ts`, computes a bounded retained-message count instead of using the absolute head sequence as `max_messages`, opens the ordered lane at the admitted anchor (`Window` → the row's replay-depth sequence, `Sequence(seq)` → `opt_start_seq` under `DeliverPolicy.StartSequence`, `Instant(at)` → `opt_start_time` under `DeliverPolicy.StartTime`), and uses `Consumer.fetch` plus `Stream.takeUntil` to terminate exactly at that head; `subscribe(topic)` warms from the same replay-depth sequence and then tails through `Consumer.consume`. The durable `consume` anchor is creation policy: an existing named consumer resumes its server-held position and never silently re-anchors.
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
    static readonly live: Layer.Layer<Broker, FanoutFault, Setting> = Layer.scoped(
        Broker,
        Effect.flatMap(Setting, (setting) =>
            Effect.acquireRelease(
                Effect.tryPromise({
                    try: () => wsconnect({ servers: setting.fanout.origin.href }),
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

            const named = (topic: string): Effect.Effect<Fanout.Topic, FanoutFault> =>
                Option.match(Option.fromNullable(topics[topic]), {
                    onNone: () => Effect.fail(new FanoutFault({ reason: 'horizon', topic })),
                    onSome: Effect.succeed,
                });

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
            ): Stream.Stream<Envelope, FanoutFault> => {
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
                    ([envelope]) => envelope,
                );
            };

            const subscribed = (topic: string): Stream.Stream<Envelope, FanoutFault> =>
                Stream.unwrap(
                    Effect.flatMap(named(topic), (row) =>
                        Effect.map(
                            Effect.tryPromise({
                                try: () => jsm.streams.info(topic),
                                catch: () => new FanoutFault({ reason: 'dial', topic }),
                            }),
                            (info) => ordered(topic, _Anchor.Sequence({ seq: Math.max(1, info.state.last_seq - row.replay + 1) })),
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

            const replayed = (topic: string, anchor: Fanout.Anchor): Stream.Stream<Envelope, FanoutFault> =>
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
                    Effect.flatMap(named(topic), (row) =>
                        Breaker.guard(
                            'fanout:publish',
                            _CIRCUIT,
                        )(
                            Effect.tryPromise({
                                try: () => js.publish(row.subject, envelope.body, { msgID: envelope.key, ..._expected(envelope.expect) }),
                                catch: () => new FanoutFault({ reason: 'publish', topic }),
                            }),
                        ).pipe(
                            Effect.mapError((fault) => (fault._tag === 'Lapse' ? new FanoutFault({ reason: 'publish', topic }) : fault)),
                            Effect.map(
                                (ack) => new _Receipt({ topic, subject: row.subject, key: envelope.key, seq: ack.seq, duplicate: ack.duplicate }),
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
                                    handler(envelope),
                                    Effect.repeat(
                                        Effect.sync(() => msg.working()),
                                        Schedule.spaced(Duration.times(row.wait, 0.5)),
                                    ),
                                ),
                                {
                                    onFailure: (fault) => Effect.sync(() => (fault.reason === 'poison' ? msg.term(fault.reason) : msg.nak())),
                                    onSuccess: () =>
                                        row.ack === 'double'
                                            ? Effect.asVoid(
                                                  Effect.tryPromise({
                                                      try: () => msg.ackAck(),
                                                      catch: () => new FanoutFault({ reason: 'dial', topic }),
                                                  }),
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { Broker, Envelope, Fanout, FanoutFault };
```
