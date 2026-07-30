# [TS_RUNTIME_API_CONFLUENTINC_KAFKA_JAVASCRIPT]

`@confluentinc/kafka-javascript` binds librdkafka from one native addon behind two surfaces: a KafkaJS-compatible promise API whose `Kafka` mints `producer`/`consumer`/`admin` clients, and the native `RdKafka` surface for the throughput lanes.

It carries the transactional exactly-once producer and manual-commit at-least-once consumer the `net/pubsub` Kafka engine row folds through `Effect.tryPromise`; the envelope stays transport-only octets, so no blob store rides it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@confluentinc/kafka-javascript`
- package: `@confluentinc/kafka-javascript` (MIT)
- module: CJS; `KafkaJS` and `RdKafka` namespaces re-exported beside the flat `RdKafka` surface
- runtime: node and bun over a prebuilt `node-pre-gyp` native binary; no browser lane
- abi: bundled librdkafka, the same broker core the C# `Confluent.Kafka` binds — idempotence, compression, SASL/TLS are one wire vocabulary across both hosts
- plane: an Apache Kafka or Confluent Platform broker on the bootstrap port; partitions, replication, and retention are deploy-plane facts
- rail: Kafka engine row of the `net/pubsub` broker plane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: KafkaJS promise types the engine row composes; native `RdKafka` types back the throughput lanes.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `Kafka`              | factory       | one client factory — `producer`/`consumer`/`admin` over a shared bootstrap        |
|  [02]   | `Producer`           | producer      | `Client &` send surface; transactional members are the exactly-once lane          |
|  [03]   | `Consumer`           | consumer      | `Client &` subscribe/run surface; manual offset commit is the at-least-once lane  |
|  [04]   | `Admin`              | admin         | topic/group lifecycle — create, offsets, metadata the engine reconciles at boot   |
|  [05]   | `ProducerRecord`     | message       | `topic` with `Message[]` — `key`/`value`/`partition?`/`headers?`/`timestamp?`     |
|  [06]   | `RecordMetadata`     | receipt       | per-message `topicName`, `partition`, `offset?`, `timestamp?` — publish evidence  |
|  [07]   | `EachMessagePayload` | delivery      | `message`, `partition`, `heartbeat`, `pause` — the `eachMessage` handler argument |
|  [08]   | `EachBatchPayload`   | delivery      | `batch`, `resolveOffset`, `commitOffsetsIfNecessary` — the `eachBatch` argument   |
|  [09]   | `ConsumerRunConfig`  | run config    | `eachMessage`/`eachBatch`, `partitionsConsumedConcurrently`, auto-resolve toggle  |
|  [10]   | `CompressionTypes`   | codec enum    | `None`/`GZIP`/`Snappy`/`LZ4`/`ZSTD` — per-topic compression a `Setting` names     |
|  [11]   | `KafkaJSError`       | fault         | error family — `KafkaJSProtocolError`, `KafkaJSConnectionError`, aggregate        |

- [05]-[MESSAGE]: `key?` selects a stable partition and carries correlation identity, never deduplicating equal keys.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and the promise lifecycle; native `RdKafka` `Producer`/`KafkaConsumer`/`AdminClient` and `createReadStream`/`createWriteStream` back the stream lanes.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------------------------ |
|  [01]   | `new Kafka(config?)`                                     | ctor     | root client over `brokers`/`ssl`/`sasl` from `Setting` rows   |
|  [02]   | `producer(config?)` / `producer.send(record)`            | factory  | scoped `connect`/`disconnect`; `send` yields `RecordMetadata` |
|  [03]   | `producer.transaction()` / `commit()` / `sendOffsets()`  | instance | transactional — atomic publish with consumed-offset handoff   |
|  [04]   | `consumer(config).subscribe(subscription)`               | instance | topic or `RegExp` fanout attach before `run`                  |
|  [05]   | `consumer.run({ eachMessage })` / `commitOffsets()`      | instance | at-least-once lane — commit after success, never auto         |
|  [06]   | `consumer.seek(topicPartitionOffset)`                    | instance | offset anchor within retention — warm-up and recovery         |
|  [07]   | `admin().createTopics()` / `fetchTopicMetadata()`        | instance | boot-time topic/partition convergence, transport-blind        |
|  [08]   | `new RdKafka.KafkaConsumer(conf, topicConf?)`            | ctor     | group-less native consumer — the bounded-replay client        |
|  [09]   | `consumer.assign(assignments)` / `unassign()`            | instance | explicit partition attach carrying no group membership        |
|  [10]   | `consumer.seek(toppar, timeout, cb)`                     | instance | native three-arg seek over an assigned partition              |
|  [11]   | `consumer.consume(count, cb)`                            | instance | the one bounded read — a finite batch, never a lane           |
|  [12]   | `queryWatermarkOffsets(topic, partition, timeout?, cb?)` | instance | replay bounds; `offsetsForTimes` the time-keyed anchor        |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `effect` (`.api/effect.md`): the `Kafka` clients are `Effect.acquireRelease` acquisitions over `connect`/`disconnect`, every promise member converts through `Effect.tryPromise` at the engine seam, and the `run` handler loop lifts each delivery callback into the effect runtime.
- `proc/config` `Setting`: bootstrap brokers, SASL/TLS posture, compression, and idempotence are config rows — no broker literal, credential, or `acks` knob lives in the engine.
- `net/pubsub` `Fanout`: the Kafka engine row is one `Fanout` member set — `publish` over `Producer.send`, `consume` over the manual-commit `run` lane under the unpositioned `Window` anchor, exactly-once over the lane's transactional producer — carrying no `stash`/`haul` blob surface. `replay`, positional consume anchors, warm subscription, and the durable-consumer census answer `horizon`: the port's `Fanout.Anchor` is engine-neutral and carries no partition coordinate, and a consumer group is not a server-held consumer object, so the ordered-replay ledger column reads honestly empty rather than approximating a sequence the broker never held.

[LOCAL_ADMISSION]:
- Acquire one `Kafka` per process in the engine Layer and derive `producer`/`consumer`/`admin` from it; a second bootstrap beside the engine client is the named defect.
- Release through `disconnect()` on scope close; a leaked client holds broker connections and consumer-group membership past the process.
- A bounded positional read is native-surface-only — `RdKafka.KafkaConsumer` `assign` the partition, `seek(toppar, timeout, cb)` it, `consume(count, cb)` against a `queryWatermarkOffsets` ceiling, then `unassign` — because the promise surface's `seek(topicPartitionOffset)` returns `void`, repositions the durable group cursor, and publishes no bounded read. The `Fanout` port declines that triple: a partition-and-offset pair is not spellable in an engine-neutral anchor, so ordered replay answers `horizon` here and the native surface stays reserved for operator tooling outside the port.
- Spell `offset` per surface: `TopicPartitionOffset.offset` is a `string` on the KafkaJS-compatible promise surface and a `number` on the native `RdKafka` surface, so one coordinate crossing both carries an explicit conversion, never a shared alias.
- Commit offsets after handler success — the at-least-once guarantee is ack-after-success, and an `autoCommit` before the handler completes silently drops on crash.
- Exactly-once rides the `Transaction` from `producer.transaction()`: produced records and consumed offsets pass through it, `sendOffsets()` binds the offset handoff, and `commit()` publishes both atomically.
- Envelope `value` is opaque transport octets the consumer's own `Schema` decodes at its seam; the engine never inspects or re-addresses it, and `ContentKey` stays the one addressing vocabulary.

[RAIL_LAW]:
- Package: `@confluentinc/kafka-javascript`
- Owns: the Kafka broker client, the promise producer/consumer/admin surface, the transactional exactly-once lane, the native throughput streams
- Accept: one scoped `Kafka` per process, offsets committed after success, exactly-once through `transaction()` with `sendOffsets()` and `commit()`, config from `Setting` rows
- Reject: per-call bootstraps, `autoCommit` where at-least-once is named, per-call transactions, hardcoded brokers or credentials, a second content-addressing vocabulary over the `value` octets
