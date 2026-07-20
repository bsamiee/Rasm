# [TS_RUNTIME_API_CONFLUENTINC_KAFKA_JAVASCRIPT]

`@confluentinc/kafka-javascript` is Confluent's node binding over librdkafka — the log-broker client backing `net/pubsub`'s Kafka engine row, the TypeScript counterpart of the C# host's `Confluent.Kafka` on the shared broker plane. It carries two surfaces from one native addon: a KafkaJS-compatible promise API (`KafkaJS` namespace) whose `Kafka` mints `producer`/`consumer`/`admin` clients over `connect`/`disconnect` lifecycles, and the native `RdKafka` surface (`Producer`, `KafkaConsumer`, `AdminClient`, stream factories) for throughput lanes the promise API abstracts.

Its promise surface is the engine-facing one: `send` returns `RecordMetadata`, `run` drives `eachMessage`/`eachBatch` handlers under manual `commitOffsets`, and `transaction`/`commit`/`sendOffsets` carry the exactly-once producer — every member a `Promise` the engine seam lifts through `Effect.tryPromise`, exactly as the jetstream row composes `NatsConnection`. Kafka answers the same guarantee ledger `net/pubsub` reads as data — at-least-once, exactly-once, replay — yet carries no blob store, so the envelope stays transport-only octets and the object-store lane is the jetstream row's alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@confluentinc/kafka-javascript`
- package: `@confluentinc/kafka-javascript` (MIT, Confluent Inc.)
- module format: CJS (`main: lib/index.js`, `types: index.d.ts`); native addon over librdkafka, `KafkaJS` and `RdKafka` namespaces re-exported beside the flat `RdKafka` surface
- runtime target: node and bun — a prebuilt native binary per `node-pre-gyp` platform tuple; no browser lane, the broker is a server-plane transport
- bundled: librdkafka the broker protocol core — the same C library the C# `Confluent.Kafka` binds, so wire posture (idempotence, compression, SASL/TLS) is one broker vocabulary across both hosts
- server: an Apache Kafka or Confluent Platform broker reachable on the bootstrap port; partition count, replication factor, and retention are cluster facts the deploy plane provisions
- rail: fanout transport capability, Kafka engine row (`net/pubsub` broker plane)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the KafkaJS promise vocabulary the engine row composes; `Kafka` mints the clients keyed in [03], and the native `RdKafka` types back the throughput lanes
- rail: boundaries

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CONSUMER]                                                                        |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `Kafka`               | factory       | the one client factory — `producer`/`consumer`/`admin` over a shared bootstrap    |
|  [02]   | `Producer`            | producer      | `Client &` send surface; the transactional members are the exactly-once lane      |
|  [03]   | `Consumer`            | consumer      | `Client &` subscribe/run surface; manual offset commit is the at-least-once lane  |
|  [04]   | `Admin`               | admin         | topic/group lifecycle — create, offsets, metadata the engine reconciles at boot   |
|  [05]   | `ProducerRecord`      | message       | `topic` plus `Message[]` (`key`, `value`, `partition?`, `headers?`, `timestamp?`) |
|  [06]   | `RecordMetadata`      | receipt       | per-message `topicName`, `partition`, `offset?`, `timestamp?` — publish evidence  |
|  [07]   | `EachMessagePayload`  | delivery      | `message`, `partition`, `heartbeat`, `pause` — the `eachMessage` handler argument |
|  [08]   | `EachBatchPayload`    | delivery      | `batch`, `resolveOffset`, `commitOffsetsIfNecessary` — the `eachBatch` argument    |
|  [09]   | `ConsumerRunConfig`   | run config    | `eachMessage`/`eachBatch`, `partitionsConsumedConcurrently`, auto-resolve toggle  |
|  [10]   | `CompressionTypes`    | codec enum    | `None`/`GZIP`/`Snappy`/`LZ4`/`ZSTD` — the per-topic compression a `Setting` names  |
|  [11]   | `KafkaJSError`        | fault         | the error family (`KafkaJSProtocolError`, `KafkaJSConnectionError`, aggregate)     |

- [05]-[MESSAGE]: `key?: Buffer | string | null` selects a stable partition and carries correlation identity; Kafka never deduplicates equal keys. `value` is the opaque envelope octets the consumer's own Schema decodes.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and the promise lifecycle; the native `RdKafka.Producer`/`KafkaConsumer`/`AdminClient` and `createReadStream`/`createWriteStream` back the stream lanes below the promise seam
- rail: system-apis

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `new Kafka(config?)`                                  | factory        | root client over `brokers`/`ssl`/`sasl` from `Setting` rows  |
|  [02]   | `producer(config?)` / `producer.send(record)`        | publish        | scoped `connect`/`disconnect`; `send` yields `RecordMetadata` |
|  [03]   | `producer.transaction()` / `commit()` / `sendOffsets()` | exactly-once | transactional producer — atomic publish plus consumed-offset |
|  [04]   | `consumer(config).subscribe(subscription)`           | subscribe      | topic or `RegExp` fanout attach before `run`                 |
|  [05]   | `consumer.run({ eachMessage })` / `commitOffsets()`  | consume        | at-least-once lane; commit after success, never auto         |
|  [06]   | `consumer.seek(topicPartitionOffset)`                | replay         | offset anchor within retention — warm-up and recovery window |
|  [07]   | `admin().createTopics()` / `fetchTopicMetadata()`    | reconcile      | boot-time topic/partition convergence, transport-blind       |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `effect` (`.api/effect.md`): the `Kafka` clients are scoped acquisitions (`Effect.acquireRelease` over `connect`/`disconnect`); every promise member converts through `Effect.tryPromise` at the engine seam, and the `run` handler loop lifts a delivery callback into the effect runtime, never a raw `await` in domain code.
- `proc/config` `Setting`: the bootstrap brokers, SASL/TLS posture, compression, and idempotence are described config rows; no broker literal, credential, or `acks` knob exists in the engine.
- `net/pubsub` `Fanout`: the Kafka engine row is one `Fanout` member set — `publish` over `Producer.send`, `consume` over the manual-commit `run` lane, `replay` over `seek`; it degrades the port honestly, carrying no `stash`/`haul` blob surface the jetstream row owns.

[LOCAL_ADMISSION]:
- Acquire exactly one `Kafka` per process inside the engine Layer and derive `producer`/`consumer`/`admin` from it; a second bootstrap beside the engine client is the named defect.
- Release through `disconnect()` on scope close; a leaked client holds broker connections and consumer-group membership past the process.
- Commit offsets after handler success, never `autoCommit` — the at-least-once guarantee is ack-after-success, and an auto-commit before the handler completes silently drops on crash.
- Exactly-once rides the `Transaction` returned by `producer.transaction()`: produced records and consumed offsets pass through that transaction, `sendOffsets()` binds the offset handoff, and `commit()` publishes both atomically. Message keys select partitions; they never deduplicate delivery.
- Envelope `value` is opaque transport octets; the consumer's own Schema decodes the body at its seam, and the engine never inspects or re-addresses it — `ContentKey` stays the one addressing vocabulary.

[RAIL_LAW]:
- Package: `@confluentinc/kafka-javascript`
- Owns: the Kafka broker client, the promise producer/consumer/admin surface, the transactional exactly-once lane, the native throughput streams
- Accept: one scoped `Kafka` per process, offsets committed after success, exactly-once through `transaction()` plus `sendOffsets()` and `commit()`, config from `Setting` rows
- Reject: per-call bootstraps, `autoCommit` where at-least-once is named, per-call transactions, hardcoded brokers or credentials, a second content-addressing vocabulary over the `value` octets
