# [RUNTIME_PUBSUB]

Fanout and replay are one port with engines as rows: `Fanout` is the broadcast plane the branch composes wherever every subscriber must see every value — live evidence mirrors, presence, cross-process invalidation, large-binary handoff — with an in-process row over the effect `PubSub` replay window, a browser cross-tab row bridging the same cells over `BroadcastChannel`, and a `jetstream` row over NATS for genuine cross-process fanout, bounded replay, dedup, and chunked blob passing. The guarantee ledger is data: at-least-once handler consumption on a durable explicit-ack consumer with ack-after-success, poison termination, and long-handler heartbeats; exactly-once publish inside a duplicate-detection window keyed by the producer's content-derived `msgID`; double-ack confirmation where a topic row demands it; replay anchored by sequence or instant on an ordered consumer that carries no ack surface at all — each a row the jetstream engine upholds and the lighter rows honestly degrade, so selecting an engine is a Layer choice at the root and the degradation is readable off one table. The stream is NEVER the system of record: retention bounds every topic, replay is a warm-up and recovery window, and a consumer needing full history reads the data wave's journal. The NATS server's durability posture — fsync interval hardening, replica quorum — is a deployment fact the deploy plane owns. The module ships on the `./server` subpath for the jetstream row; the local row is runtime-neutral and the tab row is the browser condition. The module is `runtime/src/net/pubsub.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                              | [PUBLIC]                |
| :-----: | :-------------- | :---------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | `TOPIC_ROWS`    | the topic policy vocabulary: subject, retention, replay, ack posture, redelivery    | `Fanout`                |
|  [02]   | `PORT_SHAPE`    | the engine-neutral port — publish, subscribe, consume, replay, stash, haul — faults | `Fanout`, `FanoutFault` |
|  [03]   | `LOCAL_ROW`     | the in-process engine over `PubSub` replay plus the in-process blob shelf           | `Fanout.local`          |
|  [04]   | `TAB_ROW`       | the browser cross-tab engine — `BroadcastChannel` bridge decorating the local cells | `Fanout.tab`            |
|  [05]   | `JETSTREAM_ROW` | the NATS engine: ordered vs durable lanes, dedup, double-ack, heartbeat, blob store | `Fanout.jetstream`, `Broker` |

## [2]-[TOPIC_ROWS]

[TOPIC_ROWS]:
- Owner: the topic policy row — `subject` (the wire address), `retention` (the stream age bound — the never-system-of-record ceiling), `replay` (the warm-up window a late subscriber receives), `ack` (`"fire" | "double"` — whether consumption confirms the acknowledgement itself), `wait` (the ack deadline the durable lane declares as `ack_wait` and halves into the long-handler heartbeat cadence), and `attempts` (the redelivery ceiling declared as `max_deliver` — beyond it the server parks the message); the dedup window reads `Setting.fanout.dedup` and the blob chunk size `Setting.fanout.chunk`, so topic policy is root data and the engine reads rows, never knobs.
- Law: the publish identity is content-derived — the producer mints the `key` from the kernel content-key mint or the envelope's `Hlc` stamp, so a replayed publish inside the dedup window is recognized by value identity in every language; the engine never invents identity.
- Law: the envelope is transport-only — opaque octets plus the identity key; the consumer's own Schema decodes the body at its seam (event stamps ride the payload vocabulary), so one plane serves every dialect and no payload shape accretes here.
- Growth: a new fanout concern is one topic row; a new guarantee axis is one row column every engine answers.
- Packages: `effect` (`Schema`, `Duration`), `../proc/config.ts` (`Setting`).

```typescript
import { Chunk, Context, Data, DateTime, Duration, Effect, HashMap, Layer, Option, PubSub, Record, Ref, Schedule, Schema, type Scope, Stream } from "effect"
import { type NatsConnection, wsconnect } from "@nats-io/nats-core"
import { AckPolicy, DeliverPolicy, type JsMsg, jetstream, jetstreamManager } from "@nats-io/jetstream"
import { Objm, type ObjectStore } from "@nats-io/obj"
import type { FaultClass } from "@rasm/ts/core"
import { Setting } from "../proc/config.ts"
import { Breaker } from "./client.ts"

class Envelope extends Schema.Class<Envelope>("Envelope")({
  key: Schema.NonEmptyString,
  body: Schema.Uint8ArrayFromSelf,
}) {}

declare namespace Fanout {
  type Ack = "fire" | "double"
  type Topic = {
    readonly subject: string
    readonly retention: Duration.Duration
    readonly replay: number
    readonly ack: Ack
    readonly wait: Duration.Duration
    readonly attempts: number
  }
  type Topics = Readonly<Record<string, Topic>>
  type Anchor = Data.TaggedEnum<{
    Window: {}
    Sequence: { readonly seq: number }
    Instant: { readonly at: DateTime.Utc }
  }>
  type Receipt = { readonly seq: number; readonly duplicate: boolean }
  type Stowed = { readonly size: number; readonly digest: Option.Option<string> }
}

const _Anchor = Data.taggedEnum<Fanout.Anchor>()
```

## [3]-[PORT_SHAPE]

[PORT_SHAPE]:
- Owner: the `Fanout` Tag — six members over the topic key. The envelope lane: `publish(topic, envelope)` yields the `Receipt` whose `duplicate` flag is the idempotent-noop evidence; `subscribe(topic)` is the live fanout stream — every subscriber sees every value from attach, the topic's `replay` window warming a late one; `consume(topic, anchor, handler)` is the at-least-once lane — each envelope acknowledges only after the handler succeeds, a handler fault re-delivers, a `poison`-classed handler fault terminates the message, and the topic's `ack: "double"` row confirms the acknowledgement itself; `replay(topic, anchor)` re-reads from a sequence or instant anchor within retention. The blob lane: `stash(topic, name, body)` streams chunked large binary onto the plane and yields the `Stowed` receipt, `haul(topic, name)` streams it back — transient handoff for payloads the bounded envelope cannot carry (tiles, mesh bands, media), announced over the envelope lane and passed here, never a second content-addressing vocabulary and never durable storage.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `horizon` (the anchor precedes the engine's window, the topic is undeclared, or the blob is absent — class `absent`), `publish` (an unacknowledged publish or a rejected stash, class `unavailable`), `poison` (the handler proves an envelope unprocessable, class `malformed` — the consume lane's terminate signal) — so the core budget gate re-drives the transient rows and a horizon miss routes as the terminal evidence it is.
- Law: delivery semantics are the row's, not the call site's — a consumer never re-states ack posture, replay depth, retention, or redelivery ceiling; it names the topic and the engine answers the row; an unknown topic answers `horizon` identically on every engine and every member.
- Law: the port is engine-blind — no member names NATS, and swapping any row for another edits the root merge and nothing else; the engine roster law is the services doctrine's, instantiated here.
- Entry: `yield* Fanout` then the six members; engines land as `Fanout.local(topics)` / `Fanout.tab(topics)` / `Fanout.jetstream(topics)` root Layers.
- Packages: `effect` (`Context`, `Data`, `Stream`).

```typescript
class FanoutFault extends Data.TaggedError("FanoutFault")<{
  readonly reason: "dial" | "horizon" | "publish" | "poison"
  readonly topic: string
}> {
  get class(): FaultClass.Kind {
    return this.reason === "horizon" ? "absent" : this.reason === "poison" ? "malformed" : "unavailable"
  }
}

class Fanout extends Context.Tag("runtime/Fanout")<Fanout, {
  readonly publish: (topic: string, envelope: Envelope) => Effect.Effect<Fanout.Receipt, FanoutFault>
  readonly subscribe: (topic: string) => Stream.Stream<Envelope, FanoutFault>
  readonly consume: (
    topic: string,
    anchor: Fanout.Anchor,
    handler: (envelope: Envelope) => Effect.Effect<void, FanoutFault>,
  ) => Effect.Effect<void, FanoutFault>
  readonly replay: (topic: string, anchor: Fanout.Anchor) => Stream.Stream<Envelope, FanoutFault>
  readonly stash: (topic: string, name: string, body: Stream.Stream<Uint8Array, FanoutFault>) => Effect.Effect<Fanout.Stowed, FanoutFault>
  readonly haul: (topic: string, name: string) => Stream.Stream<Uint8Array, FanoutFault>
}>() {
  static readonly Anchor = _Anchor
  static readonly Envelope = Envelope
  static readonly local = (topics: Fanout.Topics): Layer.Layer<Fanout> => _local(topics)
  static readonly tab = (topics: Fanout.Topics): Layer.Layer<Fanout> => _tab(topics)
  static readonly jetstream = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting | Broker> => _jetstream(topics)
}
```

## [4]-[LOCAL_ROW]

[LOCAL_ROW]:
- Owner: `Fanout.local(topics)` — one scoped `PubSub.bounded<Envelope>({ capacity, replay: row.replay })` per topic row plus one `Ref`-held blob shelf; `publish` offers (a local publish is never a duplicate — the dedup window is a server guarantee the local row honestly lacks), `subscribe` is the scoped `Stream.fromPubSub` whose late attach receives the replay window, `consume` folds the same stream through the handler with the ack posture vacuous (in-process delivery has no redelivery to confirm or terminate), `replay` answers only the `Window` anchor — a `Sequence` or `Instant` anchor folds to the `horizon` fault because the local tier holds no log — and the blob lane collects into and streams out of the shelf keyed `topic/name`, digest honestly absent.
- Law: the degradation is the table read aloud — replay-is-a-warm-up, no cross-process reach, no dedup window, no durable redelivery, no digest evidence; a proof or a single-process deployment selects this row deliberately, and promoting a workload that needs the missing rows is a root Layer swap, never a local re-implementation.
- Law: capacity backpressures — the bounded construction suspends a producer ahead of the slowest subscriber's window; a sliding local topic is a row decision, never a default.
- Packages: `effect` (`PubSub`, `Stream`, `Layer`, `Record`, `Ref`, `HashMap`, `Chunk`).

```typescript
const _LOCAL = { capacity: 256 } as const

type _Port = Context.Tag.Service<Fanout>

const _minted = (topics: Fanout.Topics): Effect.Effect<_Port, never, Scope.Scope> =>
  Effect.gen(function* () {
    const cells = yield* Effect.all(
      Record.map(topics, (row) => PubSub.bounded<Envelope>({ capacity: _LOCAL.capacity, replay: row.replay })),
    )
    const shelf = yield* Ref.make(HashMap.empty<string, Chunk.Chunk<Uint8Array>>())
    const held = (topic: string): Effect.Effect<PubSub.PubSub<Envelope>, FanoutFault> =>
      Option.match(Option.fromNullable(cells[topic]), {
        onNone: () => Effect.fail(new FanoutFault({ reason: "horizon", topic })),
        onSome: Effect.succeed,
      })
    return {
      publish: (topic, envelope) =>
        Effect.map(Effect.tap(held(topic), (cell) => PubSub.publish(cell, envelope)), () => ({ seq: 0, duplicate: false })),
      subscribe: (topic) => Stream.unwrap(Effect.map(held(topic), (cell) => Stream.fromPubSub(cell))),
      consume: (topic, anchor, handler) =>
        _Anchor.$is("Window")(anchor)
          ? Stream.runForEach(Stream.unwrap(Effect.map(held(topic), (cell) => Stream.fromPubSub(cell))), handler)
          : Effect.fail(new FanoutFault({ reason: "horizon", topic })),
      replay: (topic, anchor) =>
        _Anchor.$is("Window")(anchor)
          ? Stream.unwrap(Effect.map(held(topic), (cell) => Stream.fromPubSub(cell)))
          : Stream.fail(new FanoutFault({ reason: "horizon", topic })),
      stash: (topic, name, body) =>
        Effect.flatMap(Effect.zipRight(held(topic), Stream.runCollect(body)), (chunks) =>
          Effect.as(Ref.update(shelf, HashMap.set(`${topic}/${name}`, chunks)), {
            size: Chunk.reduce(chunks, 0, (sum, part) => sum + part.byteLength),
            digest: Option.none(),
          })),
      haul: (topic, name) =>
        Stream.unwrap(
          Effect.map(Ref.get(shelf), (kept) =>
            Option.match(HashMap.get(kept, `${topic}/${name}`), {
              onNone: () => Stream.fail(new FanoutFault({ reason: "horizon", topic })),
              onSome: Stream.fromChunk,
            })),
        ),
    }
  })

const _local = (topics: Fanout.Topics): Layer.Layer<Fanout> => Layer.scoped(Fanout, _minted(topics))
```

## [5]-[TAB_ROW]

[TAB_ROW]:
- Owner: `Fanout.tab(topics)` — the browser cross-tab engine: the local cells decorated with one `BroadcastChannel` per topic row keyed by the row's `subject`. `publish` offers locally then posts the encoded envelope, an arriving post decodes through the `Envelope` schema and offers into the same cells, and every other member is the local row's — so same-tab and cross-tab subscribers read one replay window and the engine is one decoration, never a second implementation.
- Law: the channel loop is structural — `BroadcastChannel` never delivers a post to the posting context, so re-offering an arrival cannot echo; a malformed arrival drops at the decode seam, never poisons a cell.
- Law: the degradation extends the local row's — no cross-process reach beyond the origin's tabs, no dedup, no durable redelivery, and the blob shelf stays per-tab (a cross-tab `haul` of another tab's stash answers `horizon`); a browser workload needing the durable rows dials the jetstream row over websockets instead.
- Boundary: the session plane's own `BroadcastChannel` (`browser/route` `Vault`) is a distinct, single-purpose channel — session continuity is not fanout, and neither surface composes the other.
- Packages: `effect` (`Stream`, `Schema`, `Record`), the host `BroadcastChannel` Web API at the sanctioned FFI seam.

```typescript
const _tab = (topics: Fanout.Topics): Layer.Layer<Fanout> =>
  Layer.scoped(
    Fanout,
    Effect.gen(function* () {
      const inner = yield* _minted(topics)
      const posts = yield* Effect.all(
        Record.map(topics, (row) =>
          Effect.acquireRelease(
            Effect.sync(() => new BroadcastChannel(row.subject)),
            (channel) => Effect.sync(() => channel.close()),
          )),
      )
      yield* Effect.forEach(
        Record.toEntries(posts),
        ([topic, channel]) =>
          Stream.runForEach(
            Stream.asyncPush<unknown>((emit) =>
              Effect.sync(() => {
                channel.addEventListener("message", (event: MessageEvent) => emit.single(event.data))
                return channel
              })),
            (data) =>
              Effect.ignore(
                Effect.flatMap(Schema.decodeUnknown(Envelope)(data), (envelope) => inner.publish(topic, envelope)),
              ),
          ).pipe(Effect.forkScoped),
        { discard: true },
      )
      return {
        ...inner,
        publish: (topic, envelope) =>
          Effect.tap(inner.publish(topic, envelope), () =>
            Effect.sync(() => posts[topic]?.postMessage(Schema.encodeSync(Envelope)(envelope)))),
      }
    }),
  )
```

## [6]-[JETSTREAM_ROW]

[JETSTREAM_ROW]:
- Owner: `Fanout.jetstream(topics)` — the NATS engine. The connection is capability: the exported `Broker` Tag holds the one scoped `wsconnect({ servers })` against `Setting.fanout.origin`, drained on scope close, and the one connection fans into the stream lanes, the object store, and the sibling coordination engine (`coordinate#KV_ROW`) — a second dial beside `Broker.live` is the named defect. Construction ensures the substrate: `jetstreamManager(nc)` then one `jsm.streams.add({ name, subjects, max_age, duplicate_window })` per topic row — retention and the dedup window are the row's durations in nanoseconds — and one `Objm(nc)` store for the blob lane, so stream and store shape are declared where topic policy lives and the server's own durability posture (fsync interval, replicas) stays a deployment fact.
- Law: the consumer lanes are split by ack capability — the ordered lane (`subscribe`, `replay`) mints a nameless ordered consumer via `js.consumers.get(stream, startOptions)`, which the server fixes to `AckPolicy.None`: it replays and tails but CANNOT acknowledge, so no ack call exists on it and the lane is read-only by construction; the durable lane (`consume`) declares `jsm.consumers.add(stream, { durable_name, ack_policy: AckPolicy.Explicit, ack_wait, max_deliver, ...anchor })` then binds `js.consumers.get(stream, durable_name)`, and only there does the ack algebra exist. An ack against an ordered consumer is unspellable because the lanes never share a mint.
- Law: exactly-once publish is the dedup window under the circuit — `js.publish(subject, body, { msgID: envelope.key })` carries the content-derived identity, the server recognizes a replay inside `duplicate_window`, the `PubAck`'s `duplicate` flag rides the `Receipt`, and the whole publish rides `Breaker.guard` so a dead broker sheds fast instead of hammering; the `expect` rows (`lastMsgID`, `lastSequence`, `lastSubjectSequence`) are the optimistic-concurrency arms a coordinating producer composes.
- Law: at-least-once is the full ack algebra — the handler runs under a heartbeat race that stamps `msg.working()` every half `ack_wait` so a long handler never triggers spurious redelivery; success acks — `ack()` on `"fire"` rows, `ackAck()` awaited on `"double"` rows so the acknowledgement itself is confirmed; a `poison` handler fault terminates through `msg.term(reason)` so an unprocessable envelope never redelivers; every other handler fault `nak()`s and the server's own clock redelivers up to the row's `max_deliver` — no hand redelivery ledger exists.
- Law: replay is bounded honesty — `replay(topic, anchor)` opens the ordered lane at the anchor (`Window` → `DeliverPolicy.New`, `Sequence(seq)` → `opt_start_seq` under `DeliverPolicy.StartSequence`, `Instant(at)` → `opt_start_time` under `DeliverPolicy.StartTime`) and streams to the head; `subscribe(topic)` IS the `Window` replay — one lane body, two members; an anchor older than `max_age` retention answers the `horizon` fault, because the stream is a window and history beyond it is the journal's.
- Law: the blob lane is the object store — `stash` bridges the effect stream through `Stream.toReadableStream` into `os.put({ name, options: { max_chunk_size } }, body)` with the chunk size from `Setting.fanout.chunk`, the `ObjectInfo` digest riding the `Stowed` receipt as wire-integrity evidence (never a second content identity — `ContentKey` remains the one addressing vocabulary); `haul` lifts `os.get(name)` back through `Stream.fromReadableStream` with the deferred `error` promise joined into the failure channel, so a mid-stream chunk fault is a typed failure, never a silent truncation.
- Law: the iterator seam is the platform-forced boundary — `consume()` yields an async iterable the engine lifts through `Stream.fromAsyncIterable` under a scoped acquisition whose release closes the consumer, so teardown rides the `Scope` and a leaked pull loop is unspellable.
- Boundary: NATS server deployment — the websocket listener, fsync `sync_interval` hardening, replica quorum — is the deploy plane's; the data journal remains the system of record, and a projection rebuilt from fanout evidence is the named defect.
- Packages: `@nats-io/nats-core` (`wsconnect`, `NatsConnection`), `@nats-io/jetstream` (`jetstream`, `jetstreamManager`, `AckPolicy`, `DeliverPolicy`), `@nats-io/obj` (`Objm`, `ObjectStore`), `effect` (`Effect`, `Layer`, `Stream`, `Duration`, `Schedule`), `../proc/config.ts` (`Setting`), `./client.ts` (`Breaker`).

```typescript
const _nanos = (span: Duration.Duration): number => Duration.toMillis(span) * 1_000_000

const _BLOB = { store: "fanout" } as const
const _CIRCUIT = { trip: 8, cool: Duration.seconds(20), probes: 1 } as const

const _start = (anchor: Fanout.Anchor): { readonly deliver_policy: DeliverPolicy; readonly opt_start_seq?: number; readonly opt_start_time?: string } =>
  _Anchor.$match(anchor, {
    Window: () => ({ deliver_policy: DeliverPolicy.New }),
    Sequence: ({ seq }) => ({ deliver_policy: DeliverPolicy.StartSequence, opt_start_seq: seq }),
    Instant: ({ at }) => ({ deliver_policy: DeliverPolicy.StartTime, opt_start_time: DateTime.formatIso(at) }),
  })

class Broker extends Context.Tag("runtime/Broker")<Broker, NatsConnection>() {
  static readonly live: Layer.Layer<Broker, FanoutFault, Setting> = Layer.scoped(
    Broker,
    Effect.flatMap(Setting, (setting) =>
      Effect.acquireRelease(
        Effect.tryPromise({
          try: () => wsconnect({ servers: setting.fanout.origin.href }),
          catch: () => new FanoutFault({ reason: "dial", topic: "*" }),
        }),
        (live) => Effect.promise(() => live.drain()),
      )),
  )
}

const _jetstream = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting | Broker> =>
  Layer.scoped(
    Fanout,
    Effect.gen(function* () {
      const setting = yield* Setting
      const nc = yield* Broker
      const js = jetstream(nc)
      const jsm = yield* Effect.promise(() => jetstreamManager(nc))
      yield* Effect.forEach(
        Record.toEntries(topics),
        ([name, row]) =>
          Effect.tryPromise({
            try: () =>
              jsm.streams.add({
                name,
                subjects: [row.subject],
                max_age: _nanos(row.retention),
                duplicate_window: _nanos(setting.fanout.dedup),
              }),
            catch: () => new FanoutFault({ reason: "dial", topic: name }),
          }),
        { discard: true },
      )
      const store: ObjectStore = yield* Effect.tryPromise({
        try: () => new Objm(nc).create(_BLOB.store),
        catch: () => new FanoutFault({ reason: "dial", topic: _BLOB.store }),
      })

      const named = (topic: string): Effect.Effect<Fanout.Topic, FanoutFault> =>
        Option.match(Option.fromNullable(topics[topic]), {
          onNone: () => Effect.fail(new FanoutFault({ reason: "horizon", topic })),
          onSome: Effect.succeed,
        })

      const pulled = (
        topic: string,
        minted: Effect.Effect<Awaited<ReturnType<typeof js.consumers.get>>, FanoutFault>,
      ): Stream.Stream<readonly [Envelope, JsMsg], FanoutFault> =>
        Stream.unwrapScoped(
          Effect.gen(function* () {
            const consumer = yield* minted
            const messages = yield* Effect.acquireRelease(
              Effect.tryPromise({
                try: () => consumer.consume(),
                catch: () => new FanoutFault({ reason: "dial", topic }),
              }),
              (live) => Effect.promise(() => live.close()),
            )
            return Stream.fromAsyncIterable(messages, () => new FanoutFault({ reason: "dial", topic })).pipe(
              Stream.map((msg: JsMsg) =>
                [
                  new Envelope({
                    key: msg.headers?.get("Nats-Msg-Id") ?? String(msg.seq),
                    body: msg.data,
                  }),
                  msg,
                ] as const),
            )
          }),
        )

      const ordered = (topic: string, anchor: Fanout.Anchor): Stream.Stream<Envelope, FanoutFault> =>
        Stream.map(
          pulled(
            topic,
            Effect.zipRight(
              named(topic),
              Effect.tryPromise({
                try: () => js.consumers.get(topic, _start(anchor)),
                catch: () => new FanoutFault({ reason: "horizon", topic }),
              }),
            ),
          ),
          ([envelope]) => envelope,
        )

      const durable = (topic: string, row: Fanout.Topic, anchor: Fanout.Anchor) =>
        Effect.zipRight(
          Effect.tryPromise({
            try: () =>
              jsm.consumers.add(topic, {
                durable_name: topic,
                ack_policy: AckPolicy.Explicit,
                ack_wait: _nanos(row.wait),
                max_deliver: row.attempts,
                ..._start(anchor),
              }),
            catch: () => new FanoutFault({ reason: "dial", topic }),
          }),
          Effect.tryPromise({
            try: () => js.consumers.get(topic, topic),
            catch: () => new FanoutFault({ reason: "dial", topic }),
          }),
        )

      return {
        publish: (topic, envelope) =>
          Effect.flatMap(named(topic), (row) =>
            Breaker.guard("fanout:publish", _CIRCUIT)(
              Effect.tryPromise({
                try: () => js.publish(row.subject, envelope.body, { msgID: envelope.key }),
                catch: () => new FanoutFault({ reason: "publish", topic }),
              }),
            ).pipe(
              Effect.mapError((fault) => (fault._tag === "Lapse" ? new FanoutFault({ reason: "publish", topic }) : fault)),
              Effect.map((ack) => ({ seq: ack.seq, duplicate: ack.duplicate })),
            )),
        subscribe: (topic) => ordered(topic, _Anchor.Window()),
        consume: (topic, anchor, handler) =>
          Effect.flatMap(named(topic), (row) =>
            Stream.runForEach(pulled(topic, durable(topic, row, anchor)), ([envelope, msg]) =>
              Effect.race(
                handler(envelope),
                Effect.repeat(Effect.sync(() => msg.working()), Schedule.spaced(Duration.times(row.wait, 0.5))),
              ).pipe(
                Effect.tap(() =>
                  row.ack === "double"
                    ? Effect.asVoid(Effect.promise(() => msg.ackAck()))
                    : Effect.sync(() => msg.ack())),
                Effect.tapError((fault) =>
                  Effect.sync(() => (fault.reason === "poison" ? msg.term(fault.reason) : msg.nak()))),
              ))),
        replay: ordered,
        stash: (topic, name, body) =>
          Effect.zipRight(
            named(topic),
            Effect.map(
              Effect.tryPromise({
                try: () =>
                  store.put(
                    { name: `${topic}/${name}`, options: { max_chunk_size: setting.fanout.chunk } },
                    Stream.toReadableStream(body),
                  ),
                catch: () => new FanoutFault({ reason: "publish", topic }),
              }),
              (info) => ({ size: info.size, digest: Option.some(info.digest) }),
            ),
          ),
        haul: (topic, name) =>
          Stream.unwrap(
            Effect.map(
              Effect.tryPromise({
                try: () => store.get(`${topic}/${name}`),
                catch: () => new FanoutFault({ reason: "dial", topic }),
              }),
              (result) =>
                result === null
                  ? Stream.fail(new FanoutFault({ reason: "horizon", topic }))
                  : Stream.fromReadableStream({
                      evaluate: () => result.data,
                      onError: () => new FanoutFault({ reason: "dial", topic }),
                    }).pipe(
                      Stream.concat(
                        Stream.drain(
                          Stream.fromEffect(
                            Effect.flatMap(Effect.promise(() => result.error), (fault) =>
                              fault === null ? Effect.void : Effect.fail(new FanoutFault({ reason: "dial", topic }))),
                          ),
                        ),
                      ),
                    ),
            ),
          ),
      }
    }),
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { Broker, Envelope, Fanout, FanoutFault }
```
