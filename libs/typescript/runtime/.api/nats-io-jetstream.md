# [@nats-io/jetstream] — the JetStream persistence surface: streams, dedup-windowed publish, anchored consumers, ack algebra

`@nats-io/jetstream` layers NATS JetStream over a `@nats-io/nats-core` connection: `jetstreamManager(nc)` administers streams (subjects, retention `max_age`, `duplicate_window`, storage, replicas) and `jetstream(nc)` is the client — `publish` with `msgID` dedup identity and `expect` optimistic-concurrency arms returning a `PubAck` whose `duplicate` flag is idempotency evidence, and `consumers.get(stream, nameOrOptions?)` minting ordered or durable consumers whose `consume()`/`fetch()`/`next()` deliver `JsMsg` values carrying the full ack algebra — `ack()`, `ackAck()` (the double-ack that confirms the acknowledgement itself), `nak(millis?)` redelivery, `term(reason?)` poison rejection. Replay is anchored: `DeliverPolicy` start rows (`opt_start_seq`, `opt_start_time`) re-read a bounded window by sequence or instant. This is the fanout/replay ENGINE row of `net/pubsub` — at-least-once with server-clocked redelivery, exactly-once publish inside the dedup window — and never the system of record: retention bounds every stream, and full history is the data journal's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/jetstream`
- package: `@nats-io/jetstream` (3.4.0, Apache-2.0, Synadia/nats.io)
- module format: ESM + CJS dual; v3 modular sibling of `@nats-io/nats-core`
- runtime target: wherever the core connection runs — node, bun, browser over websockets
- peer: `@nats-io/nats-core` (the `NatsConnection` input)
- server caveat: JetStream file-store fsync defaults to a 2-minute interval — committed-write loss under coordinated power failure is the documented risk; the deploy plane hardens `sync_interval` and replica quorum, and the engine row never carries system-of-record data
- rail: fanout/replay engine (`net/pubsub#JETSTREAM_ROW`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: streams, consumers, messages
- rail: boundaries

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY] | [CONSUMER]                                                     |
| :-----: | :--------------------------------------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `JetStreamClient` (`publish`, `consumers`)                              | client        | the engine surface `jetstream(nc)` mints                          |
|  [02]   | `JetStreamManager` (`streams.add/update/info/delete`, `consumers.*`)    | admin         | stream ensure at engine Layer build                               |
|  [03]   | `StreamConfig` (`name`, `subjects`, `max_age`, `duplicate_window`, `retention`, `storage`, `num_replicas`) | stream shape | topic rows compiled to streams; durations in nanoseconds |
|  [04]   | `ConsumerConfig` (`ack_policy`, `deliver_policy`, `durable_name`, `opt_start_seq`, `opt_start_time`, `replay_policy`, `idle_heartbeat`, `flow_control`) | consumer shape | anchored replay + durable consumption |
|  [05]   | `PubAck` (`stream`, `seq`, `duplicate`)                                 | receipt       | `Fanout.Receipt` projection — `duplicate` is idempotency evidence |
|  [06]   | `JsMsg` (`subject`, `seq`, `data`, `headers?`, `ack()`, `ackAck()`, `nak(millis?)`, `term(reason?)`) | message | the ack algebra the consume lane folds |
|  [07]   | `ConsumerMessages` (async iterable, `close()`)                          | delivery      | lifted through `Stream.fromAsyncIterable` under a scoped bracket  |
|  [08]   | `DeliverPolicy` (`All`, `Last`, `New`, `LastPerSubject`, `StartSequence`, `StartTime`) | anchor rows | the `Fanout.Anchor` compilation target                |
|  [09]   | `AckPolicy` (`None`, `All`, `Explicit`)                                 | ack rows      | `Explicit` on the durable consume lane; ordered consumers are fixed to `None` |
|  [10]   | `ReplayPolicy` (`Instant`, `Original`)                                  | replay pacing | original-timing replay is a growth row on the ordered lane        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: publish, consume, administer
- rail: boundaries

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [CONSUMER]                                                  |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `jetstream(nc)` / `jetstreamManager(nc)`                                             | mint           | engine Layer build over the core connection                    |
|  [02]   | `js.publish(subject, payload?, { msgID, expect?: { lastMsgID, lastSequence, lastSubjectSequence, streamName } })` | publish | dedup-windowed exactly-once publish; `expect` rows are the OCC arms |
|  [03]   | `jsm.streams.add(config)`                                                            | ensure         | idempotent stream provisioning per topic row                   |
|  [04]   | `jsm.consumers.add(stream, config)`                                                  | ensure         | durable consumer declaration — `durable_name`, `ack_policy`, `ack_wait`, `max_deliver`, start anchor |
|  [05]   | `js.consumers.get(stream, nameOrOptions?)`                                           | consumer       | ordered consumer (nameless) with start-anchor options; durable bind by name |
|  [06]   | `consumer.consume(opts?)` (`{ max_messages? }`)                                      | delivery       | the long-lived pull loop the engine lifts to a `Stream`        |
|  [07]   | `consumer.fetch(opts)` / `consumer.next(opts?)`                                      | delivery       | bounded-batch and single-shot pulls — growth rows              |
|  [08]   | `msg.ack()` / `msg.ackAck()` / `msg.nak(millis?)` / `msg.working()` / `msg.term(reason?)` | ack algebra | ack-after-success, double-ack, redelivery, ack-wait heartbeat, poison |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the connection and the `Nats-Msg-Id` header carriage; the `msgID` publish option writes the same header the consumer reads back as identity.
- `effect` (`.api/effect.md`): every promise member converts through `Effect.tryPromise` at the engine seam; `ConsumerMessages` lifts through `Stream.fromAsyncIterable` under `Effect.acquireRelease` whose release is `close()`; ack members are sync-void except `ackAck()`, which awaits server confirmation as `Effect.promise`.
- `core/value/contentKey` + `core/value/clock`: the publish `msgID` is content-derived — the kernel mint or the `Hlc` stamp — so dedup identity is cross-language by construction.
- data journal: the stream is a bounded window; a projection rebuilt from fanout instead of the journal is the named defect.

[LOCAL_ADMISSION]:
- Ensure streams at engine Layer build from the topic rows; stream shape never lives beside a call site.
- Publish always carries `msgID`; a keyless publish forfeits the dedup guarantee row silently and is rejected.
- Ack after handler success only; a pre-ack consume lane converts at-least-once into at-most-once and is the named defect; `nak` on handler failure, `working()` heartbeats a handler outliving half its `ack_wait`, `term` only for poison the handler proves unprocessable.
- Ordered consumers cannot ack — a nameless `consumers.get(stream, options)` mint is fixed to `AckPolicy.None`, so every ack member on its messages is a no-op; at-least-once consumption declares a durable through `jsm.consumers.add` with `AckPolicy.Explicit` and binds it by name — the two lanes never share a mint.
- Anchored replay validates against retention — an anchor beyond `max_age` answers the typed horizon fault, never an empty stream read as success.

[RAIL_LAW]:
- Package: `@nats-io/jetstream`
- Owns: stream administration, dedup-windowed publish with OCC arms, ordered/durable consumers with anchored starts, the `JsMsg` ack algebra, bounded replay
- Accept: Layer-build stream ensure, content-derived `msgID` on every publish, ack-after-success with row-selected `ackAck`, `Stream.fromAsyncIterable` over a scoped `consume()`
- Reject: keyless publishes, pre-ack consumption, the stream as system of record, hand pull loops beside the lifted iterator, server durability posture assumed instead of deployed
