# [RUNTIME_PUBSUB]

Fanout and replay are one port with engines as rows: `Fanout` is the broadcast plane the branch composes wherever every subscriber must see every value — live evidence mirrors, presence, cross-process invalidation — with an in-process row over the effect `PubSub` replay window and a `jetstream` row over NATS for genuine cross-process fanout, bounded replay, and dedup. The guarantee ledger is data: at-least-once handler consumption with ack-after-success, exactly-once publish inside a duplicate-detection window keyed by the producer's content-derived `msgID`, double-ack confirmation where a topic row demands it, and replay anchored by sequence or instant — each a row the jetstream engine upholds and the local row honestly degrades, so selecting an engine is a Layer choice at the root and the degradation is readable off one table. The stream is NEVER the system of record: retention bounds every topic, replay is a warm-up and recovery window, and a consumer needing full history reads the data wave's journal. The NATS server's durability posture — fsync interval hardening, replica quorum — is a deployment fact the deploy plane owns. The module ships on the `./server` subpath for the jetstream row; the local row is runtime-neutral. The module is `runtime/src/net/pubsub.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                        | [PUBLIC]                |
| :-----: | :-------------- | :-------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | `TOPIC_ROWS`    | the topic policy vocabulary: subject, retention, replay, ack posture, dedup      | `Fanout`                |
|  [02]   | `PORT_SHAPE`    | the engine-neutral port — publish, subscribe, consume, replay — and its faults   | `Fanout`, `FanoutFault` |
|  [03]   | `LOCAL_ROW`     | the in-process engine over `PubSub` replay                                       | `Engines.local`         |
|  [04]   | `JETSTREAM_ROW` | the NATS engine: connection capability, stream ensure, dedup, double-ack, replay | `Engines.jetstream`     |

## [2]-[TOPIC_ROWS]

[TOPIC_ROWS]:
- Owner: the topic policy row — `subject` (the wire address), `retention` (the stream age bound — the never-system-of-record ceiling), `replay` (the warm-up window a late subscriber receives), `ack` (`"fire" | "double"` — whether consumption confirms the acknowledgement itself), and `dedup` reading `Setting.fanout.dedup` (the duplicate-detection window the publish identity rides); an app declares its closed topic table in this shape and hands it to the engine constructor, so topic policy is root data and the engine reads rows, never knobs.
- Law: the publish identity is content-derived — the producer mints the `key` from the kernel content-key mint or the envelope's `Hlc` stamp, so a replayed publish inside the dedup window is recognized by value identity in every language; the engine never invents identity.
- Law: the envelope is transport-only — opaque octets plus the identity key; the consumer's own Schema decodes the body at its seam (event stamps ride the payload vocabulary), so one plane serves every dialect and no payload shape accretes here.
- Growth: a new fanout concern is one topic row; a new guarantee axis is one row column both engines answer.
- Packages: `effect` (`Schema`, `Duration`), `../proc/config.ts` (`Setting`).

```typescript
import { Context, Data, Duration, Effect, Layer, Option, PubSub, Record, Schema, Stream } from "effect"
import { type NatsConnection, wsconnect } from "@nats-io/nats-core"
import { type JsMsg, jetstream, jetstreamManager } from "@nats-io/jetstream"
import type { FaultClass } from "@rasm/ts/core"
import { Setting } from "../proc/config.ts"

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
  }
  type Topics = Readonly<Record<string, Topic>>
  type Anchor = Data.TaggedEnum<{
    Window: {}
    Sequence: { readonly seq: number }
    Instant: { readonly at: Date }
  }>
  type Receipt = { readonly seq: number; readonly duplicate: boolean }
}

const _Anchor = Data.taggedEnum<Fanout.Anchor>()
```

## [3]-[PORT_SHAPE]

[PORT_SHAPE]:
- Owner: the `Fanout` Tag — four members over the topic key: `publish(topic, envelope)` yields the `Receipt` whose `duplicate` flag is the idempotent-noop evidence; `subscribe(topic)` is the live fanout stream — every subscriber sees every value from attach, the topic's `replay` window warming a late one; `consume(topic, anchor, handler)` is the at-least-once lane — each envelope acknowledges only after the handler succeeds, a handler fault re-delivers, and the topic's `ack: "double"` row confirms the acknowledgement itself; `replay(topic, anchor)` re-reads from a sequence or instant anchor within retention.
- Law: the fault family is one reason-discriminated class — `dial` (the engine's transport is unreachable, class `unavailable`), `horizon` (the anchor precedes the engine's window, class `absent`), `publish` (an unacknowledged publish, class `unavailable`) — so the core budget gate re-drives the transient rows and a horizon miss routes as the terminal evidence it is.
- Law: delivery semantics are the row's, not the call site's — a consumer never re-states ack posture, replay depth, or retention; it names the topic and the engine answers the row.
- Law: the port is engine-blind — no member names NATS, and swapping the local row for the jetstream row edits the root merge and nothing else; the engine roster law is the services doctrine's, instantiated here.
- Entry: `yield* Fanout` then the four members; engines land as `Engines.local(topics)` / `Engines.jetstream(topics)` root Layers.
- Packages: `effect` (`Context`, `Data`, `Stream`).

```typescript
class FanoutFault extends Data.TaggedError("FanoutFault")<{
  readonly reason: "dial" | "horizon" | "publish"
  readonly topic: string
}> {
  get class(): FaultClass.Kind {
    return this.reason === "horizon" ? "absent" : "unavailable"
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
}>() {
  static readonly Anchor = _Anchor
  static readonly Envelope = Envelope
}
```

## [4]-[LOCAL_ROW]

[LOCAL_ROW]:
- Owner: `Engines.local(topics)` — one scoped `PubSub.bounded<Envelope>({ capacity, replay: row.replay })` per topic row; `publish` offers (a local publish is never a duplicate — the dedup window is a server guarantee the local row honestly lacks), `subscribe` is the scoped `Stream.fromPubSub` whose late attach receives the replay window, `consume` folds the same stream through the handler with the ack posture vacuous (in-process delivery has no redelivery to confirm), and `replay` answers only the `Window` anchor — a `Sequence` or `Instant` anchor folds to the `horizon` fault because the local tier holds no log.
- Law: the degradation is the table read aloud — replay-is-a-warm-up, no cross-process reach, no dedup window, no durable redelivery; a proof or a single-process deployment selects this row deliberately, and promoting a workload that needs the missing rows is a root Layer swap, never a local re-implementation.
- Law: capacity backpressures — the bounded construction suspends a producer ahead of the slowest subscriber's window; a sliding local topic is a row decision, never a default.
- Packages: `effect` (`PubSub`, `Stream`, `Layer`, `Record`).

```typescript
const _LOCAL = { capacity: 256 } as const

const _local = (topics: Fanout.Topics): Layer.Layer<Fanout> =>
  Layer.scoped(
    Fanout,
    Effect.gen(function* () {
      const cells = yield* Effect.all(
        Record.map(topics, (row) => PubSub.bounded<Envelope>({ capacity: _LOCAL.capacity, replay: row.replay })),
      )
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
      }
    }),
  )
```

## [5]-[JETSTREAM_ROW]

[JETSTREAM_ROW]:
- Owner: `Engines.jetstream(topics)` — the NATS engine. The connection is capability: one scoped `wsconnect({ servers })` against `Setting.fanout.origin`, drained on scope close, held by the interior `Nats` Tag so the engine Layer composes it and nothing else reaches the socket. Construction ensures the streams: `jetstreamManager(nc)` then one `jsm.streams.add({ name, subjects, max_age, duplicate_window })` per topic row — retention and the dedup window are the row's durations in nanoseconds — so stream shape is declared where topic policy lives, and the server's own durability posture (fsync interval, replicas) stays a deployment fact.
- Law: exactly-once publish is the dedup window — `js.publish(subject, body, { msgID: envelope.key })` carries the content-derived identity, the server recognizes a replay inside `duplicate_window`, and the `PubAck`'s `duplicate` flag rides the `Receipt` so an idempotent re-publish is evidence, never an error; the `expect` rows (`lastMsgID`, `lastSequence`, `lastSubjectSequence`) are the optimistic-concurrency arms a coordinating producer composes.
- Law: at-least-once is ack-after-success — `consume` reads through `js.consumers.get(stream)` with the anchor compiled to the consumer's start (`Window` → new deliveries, `Sequence(seq)` → `opt_start_seq` under `DeliverPolicy.StartSequence`, `Instant(at)` → `opt_start_time` under `DeliverPolicy.StartTime`), each `JsMsg` folds to the envelope, the handler runs, success acks — `ack()` on `"fire"` rows, `ackAck()` awaited on `"double"` rows so the acknowledgement itself is confirmed — and a handler fault `nak(delay)`s for redelivery; an unacknowledged message re-delivers by the server's own clock, so no hand redelivery ledger exists.
- Law: replay is bounded honesty — `replay(topic, anchor)` opens an ordered consumer at the anchor and streams to the head; an anchor older than `max_age` retention answers the `horizon` fault, because the stream is a window, and history beyond it is the journal's.
- Law: the iterator seam is the platform-forced boundary — `consume()` yields an async iterable the engine lifts through `Stream.fromAsyncIterable` under a scoped acquisition whose release closes the consumer, so teardown rides the `Scope` and a leaked pull loop is unspellable.
- Boundary: NATS server deployment — the websocket listener, fsync `sync_interval` hardening the durability caveat demands, replica quorum — is the deploy plane's; the data journal remains the system of record, and a projection rebuilt from fanout evidence is the named defect.
- Packages: `@nats-io/nats-core` (`wsconnect`, `NatsConnection`), `@nats-io/jetstream` (`jetstream`, `jetstreamManager`, `DeliverPolicy`), `effect` (`Effect`, `Layer`, `Stream`, `Duration`), `../proc/config.ts` (`Setting`).

```typescript
const _nanos = (span: Duration.Duration): number => Duration.toMillis(span) * 1_000_000

const _start = (anchor: Fanout.Anchor): { readonly deliver_policy?: string; readonly opt_start_seq?: number; readonly opt_start_time?: string } =>
  _Anchor.$match(anchor, {
    Window: () => ({}),
    Sequence: ({ seq }) => ({ deliver_policy: "by_start_sequence", opt_start_seq: seq }),
    Instant: ({ at }) => ({ deliver_policy: "by_start_time", opt_start_time: at.toISOString() }),
  })

const _jetstream = (topics: Fanout.Topics): Layer.Layer<Fanout, FanoutFault, Setting> =>
  Layer.scoped(
    Fanout,
    Effect.gen(function* () {
      const setting = yield* Setting
      const nc: NatsConnection = yield* Effect.acquireRelease(
        Effect.tryPromise({
          try: () => wsconnect({ servers: setting.fanout.origin.href }),
          catch: () => new FanoutFault({ reason: "dial", topic: "*" }),
        }),
        (live) => Effect.promise(() => live.drain()),
      )
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

      const drained = (topic: string, anchor: Fanout.Anchor): Stream.Stream<readonly [Envelope, JsMsg], FanoutFault> =>
        Stream.unwrapScoped(
          Effect.gen(function* () {
            const consumer = yield* Effect.tryPromise({
              try: () => js.consumers.get(topic, _start(anchor)),
              catch: () => new FanoutFault({ reason: "horizon", topic }),
            })
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

      return {
        publish: (topic, envelope) =>
          Effect.map(
            Effect.tryPromise({
              try: () => js.publish(topics[topic]?.subject ?? topic, envelope.body, { msgID: envelope.key }),
              catch: () => new FanoutFault({ reason: "publish", topic }),
            }),
            (ack) => ({ seq: ack.seq, duplicate: ack.duplicate }),
          ),
        subscribe: (topic) =>
          Stream.mapEffect(drained(topic, _Anchor.Window()), ([envelope, msg]) =>
            Effect.as(Effect.sync(() => msg.ack()), envelope)),
        consume: (topic, anchor, handler) =>
          Stream.runForEach(drained(topic, anchor), ([envelope, msg]) =>
            handler(envelope).pipe(
              Effect.tap(() =>
                topics[topic]?.ack === "double"
                  ? Effect.asVoid(Effect.promise(() => msg.ackAck()))
                  : Effect.sync(() => msg.ack())),
              Effect.tapError(() => Effect.sync(() => msg.nak())),
            )),
        replay: (topic, anchor) =>
          Stream.mapEffect(drained(topic, anchor), ([envelope, msg]) =>
            Effect.as(Effect.sync(() => msg.ack()), envelope)),
      }
    }),
  )

const Engines = {
  local: _local,
  jetstream: _jetstream,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Engines, Envelope, Fanout, FanoutFault }
```
