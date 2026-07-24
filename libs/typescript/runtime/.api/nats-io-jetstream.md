# [TS_RUNTIME_API_NATS_IO_JETSTREAM]

`@nats-io/jetstream` layers JetStream over a `@nats-io/nats-core` connection: `jetstreamManager` administers streams, `jetstream` mints the client whose `publish` arms `msgID` dedup and `expect` optimistic concurrency, whose consumers deliver `JsMsg` values carrying the full ack algebra, and whose `DeliverPolicy` anchors bounded replay. It is the fanout/replay engine of `net/pubsub` — at-least-once with server-clocked redelivery, exactly-once publish inside the dedup window — never the system of record: retention bounds every stream, and the data journal owns full history.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@nats-io/jetstream`
- package: `@nats-io/jetstream` (Apache-2.0)
- module: ESM + CJS dual; modular sibling of `@nats-io/nats-core`
- runtime: node, bun, or browser-over-websockets, wherever the core connection runs; peer `@nats-io/nats-core` supplies the `NatsConnection`
- rail: fanout/replay engine row of `net/pubsub`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: streams, consumers, messages

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

[STREAMCONFIG]: `name` `subjects` `max_age` `duplicate_window` `retention` `storage` `num_replicas`
[CONSUMERCONFIG]: `ack_policy` `deliver_policy` `durable_name` `opt_start_seq` `opt_start_time` `replay_policy` `idle_heartbeat` `flow_control`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: publish, consume, administer

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

[PUBLISH_OPTS]: `{ msgID, expect?: { lastMsgID, lastSequence, lastSubjectSequence, lastSubjectSequenceSubject, streamName } }` — `expect` arms optimistic concurrency; `lastSubjectSequenceSubject` redirects the `lastSubjectSequence` constraint onto a wildcardable subject.

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the connection and `Nats-Msg-Id` carriage — the `msgID` publish option writes the header the consumer reads back as identity.
- `effect` (`.api/effect.md`): every promise member converts through `Effect.tryPromise`; `ConsumerMessages` lifts through `Stream.fromAsyncIterable` under `Effect.acquireRelease` releasing `close()`; ack members are sync-void except `ackAck()`, awaited as `Effect.promise`.
- `core/value/contentKey` + `core/value/clock`: the publish `msgID` is content-derived — kernel mint or `Hlc` stamp — so dedup identity is cross-language by construction.
- data journal: the stream is a bounded window; the journal owns full history.

[LOCAL_ADMISSION]:
- Ensure streams at engine Layer build from topic rows; stream shape never lives beside a call site.
- Every publish carries a content-derived `msgID`; a keyless publish forfeits dedup and is rejected.
- Ack after handler success; `nak` on failure, `working()` heartbeats a handler outliving half its `ack_wait`, `term` only for poison the handler proves unprocessable.
- Ordered and durable consumers never share a mint: a nameless `consumers.get(stream, options)` is fixed to `AckPolicy.None` and its every ack member is a no-op, while at-least-once consumption declares a durable through `jsm.consumers.add` with `AckPolicy.Explicit` and binds it by name.
- Anchored replay validates against retention; an anchor beyond `max_age` answers the typed horizon fault, never an empty read reported as success.
- Server durability deploys rather than assumes: file-store fsync defaults to a 2-minute interval, so the engine row holds no system-of-record data and the deploy plane owns `sync_interval` and replica quorum.

[RAIL_LAW]:
- Package: `@nats-io/jetstream`
- Owns: stream administration, dedup-windowed publish with OCC arms, ordered/durable consumers with anchored starts, the `JsMsg` ack algebra, bounded replay
- Accept: Layer-build stream ensure, content-derived `msgID` on every publish, ack-after-success with row-selected `ackAck`, `Stream.fromAsyncIterable` over a scoped `consume()`
- Reject: keyless publishes, pre-ack consumption, the stream as system of record, hand pull loops beside the lifted iterator
