# [TS_RUNTIME_API_NATS_IO_JETSTREAM]

`@nats-io/jetstream` layers JetStream over a `@nats-io/nats-core` connection: `jetstreamManager` administers streams, `jetstream` mints the client whose `publish` arms `msgID` dedup and `expect` optimistic concurrency, whose `startBatch` stages indivisibly, whose consumers deliver `JsMsg` beside their own status rail, and whose `DeliverPolicy` anchors bounded replay. It is the fanout/replay engine of `net/pubsub` — at-least-once redelivery, exactly-once publish inside the dedup window — never the system of record: retention bounds every stream and the journal owns full history.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: streams, consumers, messages

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CONSUMER]                                                                               |
| :-----: | :---------------------- | :-------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `JetStreamClient`       | client          | `publish`, `consumers`; the engine surface `jetstream(nc)` mints                         |
|  [02]   | `JetStreamManager`      | admin           | `streams.add/update/info/delete`, `consumers.*`; stream ensure at Layer build            |
|  [03]   | `StreamConfig`          | stream shape    | topic rows compiled to streams; durations in nanoseconds; fields keyed below             |
|  [04]   | `ConsumerConfig`        | consumer shape  | anchored replay + durable consumption; fields keyed below                                |
|  [05]   | `PubAck`                | publish ack     | `stream`, `seq`, `duplicate` — `Fanout.Landing`; `duplicate` is idempotency evidence     |
|  [06]   | `JsMsg`                 | message         | `subject`, `seq`, `data`, `headers?`; the ack algebra the consume lane folds             |
|  [07]   | `ConsumerMessages`      | delivery        | async iterable + `close()`; lifted through `Stream.fromAsyncIterable`                    |
|  [08]   | `DeliverPolicy`         | anchor rows     | `All`/`Last`/`New`/`LastPerSubject`/`StartSequence`/`StartTime` — `Fanout.Anchor` target |
|  [09]   | `AckPolicy`             | ack rows        | `None`/`All`/`Explicit`; `Explicit` durable, ordered consumers fixed to `None`           |
|  [10]   | `ReplayPolicy`          | replay pacing   | `Instant`/`Original`; original-timing replay is a growth row on the ordered lane         |
|  [11]   | `JetStreamApiError`     | fault           | server API rejection carrying `.code`; the ensure and refuse arms discriminate on it     |
|  [12]   | `JetStreamApiCodes`     | fault codes     | the const code roster keyed below                                                        |
|  [13]   | `JetStreamError`        | fault           | client-side base beneath the API error                                                   |
|  [14]   | `JetStreamStatusError`  | fault           | status family; a non-API rejection folds to the engine's `dial` reason                   |
|  [15]   | `ConsumerNotFoundError` | fault           | typed subclass narrowing the consumer-absent arm                                         |
|  [16]   | `StreamNotFoundError`   | fault           | typed subclass narrowing the stream-absent arm                                           |
|  [17]   | `StreamInfo`            | census fact     | the bounded-replay horizon `replay` validates against; fields keyed below                |
|  [18]   | `ConsumerInfo`          | census          | the durable-consumer doctor read; fields keyed below                                     |
|  [19]   | `Lister`                | census          | pages a census one turn at a time                                                        |
|  [20]   | `ConsumerNotification`  | consumer status | the 15-arm union `ConsumerMessages.status()` yields; keyed below                         |
|  [21]   | `ConsumeOptions`        | pull options    | the long-lived pull loop's own bounds; keyed below                                       |
|  [22]   | `Batch` / `BatchAck`    | atomic stage    | `startBatch` handle and its one settled `BatchAck`; keyed below                          |

[STREAMCONFIG]: `name` `subjects` `max_age` `duplicate_window` `retention` `storage` `num_replicas` `allow_atomic` `allow_batched`
[CONSUMERCONFIG]: `ack_policy` `deliver_policy` `durable_name` `opt_start_seq` `opt_start_time` `replay_policy` `idle_heartbeat` `flow_control`
[APICODES]: `StreamWrongLastSequence`/`StreamWrongLastSequenceUnknown` are the lost-race pair; `StreamNotFound`/`ConsumerNotFound` are the ensure arms' converge probes
[STREAMINFO]: `state.messages` `state.first_seq` `state.last_seq` `state.first_ts`
[CONSUMERINFO]: `created` `delivered` `num_pending` `num_ack_pending` `num_redelivered`
[CONSUMERNOTIFICATION]: NINE arms carry a loss — `heartbeats_missed`(`count`), `consumer_not_found`(`name`,`stream`,`count`), `stream_not_found`(`name`), `consumer_deleted`(`code`,`description`), `ordered_consumer_recreated`(`name`), `exceeded_limits`(`code`,`description`), `no_responders`(`code`), `discard`(`messagesLeft`,`bytesLeft`), `reset`(`name`); SIX carry progress — `heartbeat`, `flow_control`, `next`, `debug`, `consumer_pinned`, `consumer_unpinned`
[CONSUMEOPTIONS]: `max_messages` (default 100) or `max_bytes`, `threshold_messages`/`threshold_bytes` (default 75% of the ceiling), `expires` (default 30_000, minimum 1_000), `idle_heartbeat`, `abort_on_missing_resource`, `bind`, `callback`; `bind` and `abort_on_missing_resource` are mutually exclusive and `bind` is invalid on ordered consumers
[BATCH]: `id` `count` `add(subj, payload?, opts?)` `commit(subj, payload?, opts?)`; `BatchAck` widens `PubAck` with `batch` and `count`
[BATCHLIMITS]: at most 1000 messages per stage counting the opening and committing ones, at most 50 live stages per stream, and a stage idle past 10 seconds is abandoned server-side

- `ConsumerNotFoundError` and `StreamNotFoundError`: `instanceof` narrows both, so a fence never re-spells the code roster to discriminate.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: publish, consume, administer

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [CONSUMER]                                                           |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `jetstream(nc)` / `jetstreamManager(nc)`        | mint            | engine Layer build over the core connection                          |
|  [02]   | `js.publish(subject, payload?, opts?)`          | publish         | dedup-windowed exactly-once publish; `expect` rows are the OCC arms  |
|  [03]   | `jsm.streams.add(config)`                       | ensure          | idempotent stream provisioning per topic row                         |
|  [04]   | `jsm.consumers.add(stream, config)`             | ensure          | durable consumer: `durable_name`, `ack_wait`, `max_deliver`, anchor  |
|  [05]   | `js.consumers.get(stream, nameOrOptions?)`      | consumer        | ordered (nameless) start-anchor, or durable bind by name             |
|  [06]   | `consumer.consume(opts?: ConsumeOptions)`       | delivery        | the long-lived pull loop the engine lifts to a `Stream`              |
|  [07]   | `consumer.fetch(opts)` / `consumer.next(opts?)` | delivery        | bounded-batch and single-shot pulls                                  |
|  [08]   | `msg.ack()` / `msg.ackAck()`                    | ack             | ack-after-success; `ackAck` double-ack confirms the ack              |
|  [09]   | `msg.nak(millis?)` / `msg.working()`            | redelivery      | redelivery request; `working()` heartbeats ack-wait                  |
|  [10]   | `msg.term(reason?)`                             | poison          | terminal reject for unprocessable poison                             |
|  [11]   | `messages.status()`                             | consumer status | `AsyncIterable<ConsumerNotification>` — the loop's own evidence rail |
|  [12]   | `js.startBatch(subj, payload?, opts?)`          | atomic stage    | → `Promise<Batch>`; the opening message carries the stage's `msgID`  |

[BATCH_OPTS]: `BatchMessageOptions` omits `msgID` and `lastMsgID`, so the OPENING message alone carries a dedup key and the stage's dedup grain is the batch; `commit` takes `RequestOptions`, carrying headers but no publish options. `Batch` publishes NO abort member — the server's idle window is the only reclamation.

[PUBLISH_OPTS]: `{ msgID, expect?: { lastMsgID, lastSequence, lastSubjectSequence, lastSubjectSequenceSubject, streamName } }` — `expect` arms optimistic concurrency; `lastSubjectSequenceSubject` redirects the `lastSubjectSequence` constraint onto a wildcardable subject.

## [03]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@nats-io/nats-core` (`.api/nats-io-nats-core.md`): the connection and `Nats-Msg-Id` carriage — the `msgID` publish option writes the header the consumer reads back as identity.
- `effect` (`.api/effect.md`): every promise member converts through `Effect.tryPromise`; `ConsumerMessages` lifts through `Stream.fromAsyncIterable` under `Effect.acquireRelease` releasing `close()`; ack members are sync-void except `ackAck()`, awaited as `Effect.promise`; `status()` lifts through the same seam on a fiber forked into the pull loop's own scope.
- `@nats-io/nats-core` `status()`: the CONNECTION union and `ConsumerNotification` are DISJOINT families answering different questions — one reports what the socket did, the other what the pull loop did — so a rail folding only the first reports a healthy socket under a dead consumer.
- `core/value/contentKey` + `core/value/clock`: the publish `msgID` is content-derived — kernel mint or `Hlc` stamp — so dedup identity is cross-language by construction.
- data journal: the stream is a bounded window; the journal owns full history.

[LOCAL_ADMISSION]:
- Ensure streams at engine Layer build from topic rows; stream shape never lives beside a call site.
- Every publish carries a content-derived `msgID`; a keyless publish forfeits dedup and is rejected.
- Ack after handler success; `nak` on failure, `working()` heartbeats a handler outliving half its `ack_wait`, `term` only for poison the handler proves unprocessable.
- Read `status()` on every `consume()`: the client keeps re-requesting from a consumer it once reached, so a deleted durable, a purged stream, or a heartbeat gap yields nothing, fails nothing, and reads exactly like an idle topic. Ignoring that iterator is the named silent-stall defect.
- Declare `max_messages` on every `consume()`: a bare call takes 100, every buffered message starts its `ack_wait` clock at DELIVERY, and `working()` covers the one message a sequential handler holds — so an undeclared buffer redelivers work already in the process. `threshold_messages` refills it, while `expires` and `idle_heartbeat` stay unset because the package documents both as its own to pick and a low value stresses the server.
- Arm `abort_on_missing_resource` on a DURABLE lane, where an absent consumer is terminal and a supervisor re-ensures it; leave it off the ordered lane, whose consumer the client recreates.
- Stage atomicity through `startBatch` and arm `allow_atomic` at stream ensure — the capability is stream shape, so an unarmed stream refuses the stage at the server. Stages settle ONCE, so N messages answer one `BatchAck` and no per-message sequence exists to hand back.
- Ordered and durable consumers never share a mint: a nameless `consumers.get(stream, options)` is fixed to `AckPolicy.None` and its every ack member is a no-op, while at-least-once consumption declares a durable through `jsm.consumers.add` with `AckPolicy.Explicit` and binds it by name.
- Anchored replay validates against retention; an anchor beyond `max_age` answers the typed horizon fault, never an empty read reported as success.
- Server durability deploys rather than assumes: file-store fsync defaults to a 2-minute interval, so the engine row holds no system-of-record data and the deploy plane owns `sync_interval` and replica quorum.
