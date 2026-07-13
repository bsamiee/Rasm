# [TS_RUNTIME_API_NATS_IO_JETSTREAM]

`@nats-io/jetstream` layers NATS JetStream over a `@nats-io/nats-core` connection: `jetstreamManager(nc)` administers streams (subjects, retention `max_age`, `duplicate_window`, storage, replicas) and `jetstream(nc)` is the client — `publish` with `msgID` dedup identity and `expect` optimistic-concurrency arms returning a `PubAck` whose `duplicate` flag is idempotency evidence, and `consumers.get(stream, nameOrOptions?)` minting ordered or durable consumers whose `consume()`/`fetch()`/`next()` deliver `JsMsg` values carrying the full ack algebra — `ack()`, `ackAck()` (the double-ack that confirms the acknowledgement itself), `nak(millis?)` redelivery, `term(reason?)` poison rejection. Replay is anchored: `DeliverPolicy` start rows (`opt_start_seq`, `opt_start_time`) re-read a bounded window by sequence or instant. This is the fanout/replay ENGINE row of `net/pubsub` — at-least-once with server-clocked redelivery, exactly-once publish inside the dedup window — and never the system of record: retention bounds every stream, and full history is the data journal's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/jetstream`
- package: `@nats-io/jetstream` (Apache-2.0, Synadia/nats.io)
- module format: ESM + CJS dual; catalog-bound modular sibling of `@nats-io/nats-core`
- runtime target: wherever the core connection runs — node, bun, browser over websockets
- peer: `@nats-io/nats-core` (the `NatsConnection` input)
- server caveat: JetStream file-store fsync defaults to a 2-minute interval — committed-write loss under coordinated power failure is the documented risk; the deploy plane hardens `sync_interval` and replica quorum, and the engine row never carries system-of-record data
- rail: fanout/replay engine (`net/pubsub#JETSTREAM_ROW`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: streams, consumers, messages; the `StreamConfig`/`ConsumerConfig` field rosters are keyed below the table
- rail: boundaries

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CONSUMER]                                                                               |
| :-----: | :----------------- | :------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `JetStreamClient`  | client         | `publish`, `consumers`; the engine surface `jetstream(nc)` mints                         |
|  [02]   | `JetStreamManager` | admin          | `streams.add/update/info/delete`, `consumers.*`; stream ensure at Layer build            |
|  [03]   | `StreamConfig`     | stream shape   | topic rows compiled to streams; durations in nanoseconds; fields keyed below             |
|  [04]   | `ConsumerConfig`   | consumer shape | anchored replay + durable consumption; fields keyed below                                |
|  [05]   | `PubAck`           | receipt        | `stream`, `seq`, `duplicate` — `Fanout.Receipt`; `duplicate` is idempotency evidence     |
|  [06]   | `JsMsg`            | message        | `subject`, `seq`, `data`, `headers?`; the ack algebra the consume lane folds             |
|  [07]   | `ConsumerMessages` | delivery       | async iterable + `close()`; lifted through `Stream.fromAsyncIterable`                    |
|  [08]   | `DeliverPolicy`    | anchor rows    | `All`/`Last`/`New`/`LastPerSubject`/`StartSequence`/`StartTime` — `Fanout.Anchor` target |
|  [09]   | `AckPolicy`        | ack rows       | `None`/`All`/`Explicit`; `Explicit` durable, ordered consumers fixed to `None`           |
|  [10]   | `ReplayPolicy`     | replay pacing  | `Instant`/`Original`; original-timing replay is a growth row on the ordered lane         |

- [03]-[STREAMCONFIG]: `name`, `subjects`, `max_age`, `duplicate_window`, `retention`, `storage`, `num_replicas`.
- [04]-[CONSUMERCONFIG]: `ack_policy`, `deliver_policy`, `durable_name`, `opt_start_seq`, `opt_start_time`, `replay_policy`, `idle_heartbeat`, `flow_control`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: publish, consume, administer; the `js.publish` options shape is keyed below the table
- rail: boundaries

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CONSUMER]                                                          |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------------------------ |
|  [01]   | `jetstream(nc)` / `jetstreamManager(nc)`        | mint           | engine Layer build over the core connection                         |
|  [02]   | `js.publish(subject, payload?, opts?)`          | publish        | dedup-windowed exactly-once publish; `expect` rows are the OCC arms |
|  [03]   | `jsm.streams.add(config)`                       | ensure         | idempotent stream provisioning per topic row                        |
|  [04]   | `jsm.consumers.add(stream, config)`             | ensure         | durable consumer: `durable_name`, `ack_wait`, `max_deliver`, anchor |
|  [05]   | `js.consumers.get(stream, nameOrOptions?)`      | consumer       | ordered (nameless) start-anchor, or durable bind by name            |
|  [06]   | `consumer.consume(opts?)` (`{ max_messages? }`) | delivery       | the long-lived pull loop the engine lifts to a `Stream`             |
|  [07]   | `consumer.fetch(opts)` / `consumer.next(opts?)` | delivery       | bounded-batch and single-shot pulls — growth rows                   |
|  [08]   | `msg.ack()` / `msg.ackAck()`                    | ack            | ack-after-success; `ackAck` double-ack confirms the ack             |
|  [09]   | `msg.nak(millis?)` / `msg.working()`            | redelivery     | redelivery request; `working()` heartbeats ack-wait                 |
|  [10]   | `msg.term(reason?)`                             | poison         | terminal reject for unprocessable poison                            |

- [02]-[PUBLISH_OPTS]: `{ msgID, expect?: { lastMsgID, lastSequence, lastSubjectSequence, lastSubjectSequenceSubject, streamName } }` — `lastSubjectSequenceSubject` redirects the `lastSubjectSequence` constraint onto an alternative (wildcardable) subject; the typed `lastSubjectSequenceValue` member maps to no publish header and is dead surface.

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
