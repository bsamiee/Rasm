# [RASM_PERSISTENCE_API_KAFKA]

`Confluent.Kafka` supplies the librdkafka-backed Kafka client surface: the `IProducer<TKey, TValue>` and `IConsumer<TKey, TValue>` clients, their `ProducerBuilder`/`ConsumerBuilder` construction surfaces, the `Message<TKey, TValue>` envelope with `Headers`, `DeliveryReport`/`DeliveryResult` acknowledgement records, the `TopicPartitionOffset` position family, strongly-typed `ProducerConfig`/`ConsumerConfig`, and the `ISerializer`/`IDeserializer`/`IAsyncSerializer`/`IAsyncDeserializer` codec contracts with the `SyncOverAsync` adapter. Transitive `librdkafka.redist` supplies the `osx-arm64` native runtime. Kafka is the at-least-once, dead-letter-capable transport for the Persistence op-log changefeed CDC egress; the `CloudNative.CloudEvents.Kafka` binding rides on top.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Confluent.Kafka`
- package: `Confluent.Kafka`
- license: Apache-2.0
- assembly: `Confluent.Kafka`
- namespace: `Confluent.Kafka`, `Confluent.Kafka.SyncOverAsync`
- target: multi-target (`net462`, `net6.0`, `net8.0`, `netstandard2.0`); the `net10.0` consumer binds `lib/net8.0`
- native: transitive `librdkafka.redist` `2.14.2` (librdkafka LICENSES.txt) ships `runtimes/osx-arm64/native/librdkafka.dylib` (~8.6 MiB) plus `osx-x64`/`linux-*`/`win-*`; pulled by `Confluent.Kafka` `include="All"`, P/Invoke-loaded by the client handle
- rail: cdc-egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and builder family
- rail: cdc-egress

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [RAIL]                                              |
| :-----: | :------------------------------ | :--------------- | :-------------------------------------------------- |
|  [01]   | `IProducer<TKey, TValue>`       | producer client  | typed produce and transaction                       |
|  [02]   | `IConsumer<TKey, TValue>`       | consumer client  | typed consume and offset commit                     |
|  [03]   | `ProducerBuilder<TKey, TValue>` | producer builder | config, serializer, handler wiring                  |
|  [04]   | `ConsumerBuilder<TKey, TValue>` | consumer builder | config, deserializer, rebalance                     |
|  [05]   | `IClient`                       | client root      | `Handle`/`Name`, `AddBrokers`, `SetSaslCredentials` |
|  [06]   | `Handle`                        | native handle    | librdkafka handle accessor                          |
|  [07]   | `IConsumerGroupMetadata`        | group metadata   | transactional offset commit token                   |
|  [08]   | `PartitionerDelegate`           | partition hook   | per-message partition selection delegate            |
|  [09]   | `LogMessage`                    | log record       | librdkafka facility/level/message                   |

[PUBLIC_TYPE_SCOPE]: message and acknowledgement family
- rail: cdc-egress

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                         |
| :-----: | :----------------------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `Message<TKey, TValue>`        | message envelope | key, value, timestamp, headers                 |
|  [02]   | `MessageMetadata`              | metadata base    | timestamp and headers carrier                  |
|  [03]   | `Headers`                      | header list      | ordered multi-valued header set                |
|  [04]   | `Header`                       | header value     | one key plus byte payload                      |
|  [05]   | `IHeader`                      | header contract  | key plus value-bytes accessor                  |
|  [06]   | `DeliveryReport<TKey, TValue>` | delivery ack     | per-message produce outcome                    |
|  [07]   | `DeliveryResult<TKey, TValue>` | delivery result  | awaited produce outcome                        |
|  [08]   | `PersistenceStatus`            | persistence enum | `NotPersisted`/`PossiblyPersisted`/`Persisted` |
|  [09]   | `ConsumeResult<TKey, TValue>`  | consume result   | fetched message plus position                  |

[PUBLIC_TYPE_SCOPE]: position and timestamp family
- rail: cdc-egress

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [RAIL]                          |
| :-----: | :------------------------ | :-------------- | :------------------------------ |
|  [01]   | `TopicPartition`          | position value  | topic plus partition            |
|  [02]   | `TopicPartitionOffset`    | position value  | topic, partition, offset, epoch |
|  [03]   | `TopicPartitionTimestamp` | position value  | topic, partition, timestamp     |
|  [04]   | `Offset`                  | offset value    | numeric offset plus sentinels   |
|  [05]   | `Partition`               | partition value | partition id plus `Any`         |
|  [06]   | `Timestamp`               | timestamp value | unix-ms plus `TimestampType`    |
|  [07]   | `TimestampType`           | timestamp enum  | create-time vs. log-append      |
|  [08]   | `WatermarkOffsets`        | offset range    | low and high watermark          |
|  [09]   | `TopicCollection`         | topic set       | named topic collection          |

[PUBLIC_TYPE_SCOPE]: configuration family
- rail: cdc-egress

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [RAIL]                           |
| :-----: | :----------------- | :-------------- | :------------------------------- |
|  [01]   | `ClientConfig`     | config base     | shared bootstrap, security, SASL |
|  [02]   | `ProducerConfig`   | producer config | idempotence, linger, compression |
|  [03]   | `ConsumerConfig`   | consumer config | group, offset reset, auto-commit |
|  [04]   | `Acks`             | ack enum        | `None`/`Leader`/`All`            |
|  [05]   | `AutoOffsetReset`  | reset enum      | `Latest`/`Earliest`/`Error`      |
|  [06]   | `CompressionType`  | codec enum      | `Gzip`/`Snappy`/`Lz4`/`Zstd`     |
|  [07]   | `SecurityProtocol` | security enum   | plaintext/SSL/SASL selector      |
|  [08]   | `SaslMechanism`    | auth enum       | SASL mechanism selector          |
|  [09]   | `IsolationLevel`   | isolation enum  | read-committed vs. uncommitted   |

[PUBLIC_TYPE_SCOPE]: serialization family
- rail: cdc-egress

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]      | [RAIL]                           |
| :-----: | :---------------------- | :----------------- | :------------------------------- |
|  [01]   | `ISerializer<T>`        | sync codec         | span-free synchronous serialize  |
|  [02]   | `IDeserializer<T>`      | sync codec         | span synchronous deserialize     |
|  [03]   | `IAsyncSerializer<T>`   | async codec        | memory-based async serialize     |
|  [04]   | `IAsyncDeserializer<T>` | async codec        | memory-based async deserialize   |
|  [05]   | `Serializers`           | serializer catalog | built-in primitive serializers   |
|  [06]   | `Deserializers`         | codec catalog      | built-in primitive deserializers |
|  [07]   | `SerializationContext`  | codec context      | component, topic, headers        |

[PUBLIC_TYPE_SCOPE]: error and exception family
- rail: cdc-egress

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [RAIL]                           |
| :-----: | :------------------------------- | :-------------- | :------------------------------- |
|  [01]   | `Error`                          | error value     | code, reason, fatal flag         |
|  [02]   | `ErrorCode`                      | error enum      | librdkafka and broker codes      |
|  [03]   | `KafkaException`                 | base failure    | typed error wrapper              |
|  [04]   | `KafkaRetriableException`        | retriable fault | transient transaction error      |
|  [05]   | `KafkaTxnRequiresAbortException` | txn fault       | abort-required transaction error |
|  [06]   | `ProduceException<TKey, TValue>` | produce failure | failed delivery with message     |
|  [07]   | `ConsumeException`               | consume failure | failed fetch with result         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: producer build and produce
- rail: cdc-egress
- note: rows [01]-[10] configure the builder; [11]-[17] act on the built `IProducer<TKey, TValue>`

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `new ProducerBuilder<TKey, TValue>(config)`  | ctor           | builds from `ProducerConfig` pairs          |
|  [02]   | `SetKeySerializer(serializer)`               | builder        | sets sync or async key codec                |
|  [03]   | `SetValueSerializer(serializer)`             | builder        | sets sync or async value codec              |
|  [04]   | `SetDefaultPartitioner(partitioner)`         | builder        | sets fallback partitioner delegate          |
|  [05]   | `SetPartitioner(topic, partitioner)`         | builder        | sets per-topic partitioner                  |
|  [06]   | `SetErrorHandler(handler)`                   | builder        | wires async error callback                  |
|  [07]   | `SetLogHandler(handler)`                     | builder        | wires librdkafka log callback               |
|  [08]   | `SetStatisticsHandler(handler)`              | builder        | wires JSON statistics callback              |
|  [09]   | `SetOAuthBearerTokenRefreshHandler(handler)` | builder        | wires SASL/OAUTHBEARER token refresh (OIDC) |
|  [10]   | `Build()`                                    | factory call   | yields `IProducer<TKey, TValue>`            |
|  [11]   | `ProduceAsync(topic, message, ct)`           | async produce  | awaits `DeliveryResult`                     |
|  [12]   | `ProduceAsync(topicPartition, message, ct)`  | async produce  | awaits delivery to fixed partition          |
|  [13]   | `Produce(topic, message, deliveryHandler)`   | fire-and-poll  | enqueues with delivery callback             |
|  [14]   | `Poll(timeout)`                              | drive          | serves delivery report callbacks            |
|  [15]   | `Flush(timeout)` / `Flush(ct)`               | drain          | blocks until queue is empty                 |
|  [16]   | `SetSaslCredentials(user, pw)`               | runtime        | rotates SASL credentials without rebuild    |
|  [17]   | `AddBrokers(brokers)`                        | runtime        | adds bootstrap brokers without rebuild      |

[ENTRYPOINT_SCOPE]: producer transaction
- rail: cdc-egress

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `InitTransactions(timeout)`                    | txn init       | registers transactional producer  |
|  [02]   | `BeginTransaction()`                           | txn begin      | opens a transaction window        |
|  [03]   | `SendOffsetsToTransaction(offsets, group, t)`  | txn offset     | binds consumed offsets to txn     |
|  [04]   | `CommitTransaction()` / `CommitTransaction(t)` | txn commit     | atomically commits produced batch |
|  [05]   | `AbortTransaction()` / `AbortTransaction(t)`   | txn abort      | discards produced batch           |

[ENTRYPOINT_SCOPE]: consumer build and consume
- rail: cdc-egress

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `new ConsumerBuilder<TKey, TValue>(config)`                           | ctor           | builds from `ConsumerConfig` pairs              |
|  [02]   | `SetKeyDeserializer(deserializer)`                                    | builder        | sets key codec                                  |
|  [03]   | `SetValueDeserializer(deserializer)`                                  | builder        | sets value codec                                |
|  [04]   | `SetPartitionsAssignedHandler(handler)`                               | builder        | wires rebalance assign callback                 |
|  [05]   | `SetPartitionsRevokedHandler(handler)`                                | builder        | wires rebalance revoke callback                 |
|  [06]   | `SetPartitionsLostHandler(handler)`                                   | builder        | wires partition-loss callback                   |
|  [07]   | `SetOffsetsCommittedHandler(handler)`                                 | builder        | wires commit-result callback                    |
|  [08]   | `Build()`                                                             | factory call   | yields `IConsumer<TKey, TValue>`                |
|  [09]   | `Subscribe(topics)` / `Subscribe(topic)` / `Unsubscribe()`            | subscribe      | joins or leaves the group subscription          |
|  [10]   | `Assign(...)` / `IncrementalAssign(...)` / `IncrementalUnassign(...)` | assign         | manual, incremental add, or incremental remove  |
|  [11]   | `Consume(ct)` / `Consume(timeout)` / `Consume(millisecondsTimeout)`   | fetch          | one `ConsumeResult`; `IsPartitionEOF` flags end |
|  [12]   | `Seek(topicPartitionOffset)`                                          | reposition     | repositions a fetch cursor                      |
|  [13]   | `Pause(partitions)` / `Resume(partitions)`                            | flow control   | halts or resumes fetch                          |
|  [14]   | `GetWatermarkOffsets(tp)` / `QueryWatermarkOffsets(tp, t)`            | lag probe      | cached or broker-queried low/high watermarks    |
|  [15]   | `Assignment` (property)                                               | state          | the current `List<TopicPartition>` assignment   |
|  [16]   | `Close()`                                                             | leave          | leaves group and commits final state            |

[ENTRYPOINT_SCOPE]: consumer offset commit
- rail: cdc-egress

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :---------------------------------- | :------------- | :------------------------------- |
|  [01]   | `StoreOffset(consumeResult)`        | store          | marks result offset for commit   |
|  [02]   | `StoreOffset(topicPartitionOffset)` | store          | marks explicit offset for commit |
|  [03]   | `Commit()`                          | commit         | commits all stored offsets       |
|  [04]   | `Commit(offsets)`                   | commit         | commits explicit offset list     |
|  [05]   | `Commit(consumeResult)`             | commit         | commits one result offset        |
|  [06]   | `Committed(partitions, timeout)`    | query          | fetches committed offsets        |
|  [07]   | `Position(topicPartition)`          | query          | returns current consume position |
|  [08]   | `ConsumerGroupMetadata`             | property       | `IConsumerGroupMetadata` for txn |

[ENTRYPOINT_SCOPE]: message, header, and codec construction
- rail: cdc-egress
- note: `AsSyncOverAsync()` mounts the adapter type `SyncOverAsyncSerializer<T>` on the sync codec slot

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new Message<TKey, TValue> { Key, Value }`                             | object init    | sets key, value, timestamp, headers          |
|  [02]   | `MessageMetadata.Timestamp` / `.Headers`                               | property       | per-message metadata carriers                |
|  [03]   | `new Headers()` / `Add(key, valueBytes)`                               | header build   | appends byte-valued header                   |
|  [04]   | `Headers.TryGetLastBytes(key, out value)`                              | header read    | reads latest value for a key                 |
|  [05]   | `new Header(key, valueBytes)`                                          | ctor           | constructs one header                        |
|  [06]   | `ISerializer<T>.Serialize(value, context)`                             | codec call     | synchronous serialize                        |
|  [07]   | `IAsyncSerializer<T>.SerializeAsync(value, ctx)`                       | codec call     | async serialize                              |
|  [08]   | `Serializers.{Utf8,Null,Int32,Int64,Single,Double,ByteArray}`          | static codec   | primitive serializers; `Null` = tombstone    |
|  [09]   | `Deserializers.{Utf8,Null,Ignore,Int32,Int64,Single,Double,ByteArray}` | static codec   | primitive deserializers; `Ignore` skips it   |
|  [10]   | `asyncSerializer.AsSyncOverAsync()`                                    | adapter        | mounts an async codec on the sync codec slot |
|  [11]   | `new TopicPartitionOffset(tp, offset, epoch?)`                         | ctor           | constructs a commit position                 |
|  [12]   | `Offset.Beginning` / `Offset.End` / `Offset.Stored` / `Offset.Unset`   | sentinel       | named offset starting points                 |

## [04]-[IMPLEMENTATION_LAW]

[KAFKA_TOPOLOGY]:
- core namespace: `Confluent.Kafka` — clients, builders, message, position, config, codecs, errors
- sync-over-async namespace: `Confluent.Kafka.SyncOverAsync` — `AsSyncOverAsync` adapter for async codecs on the sync codec slot
- native runtime: the transitive `librdkafka.redist` `2.14.2` dependency ships `runtimes/osx-arm64/native/librdkafka.dylib`, P/Invoke-loaded by the client handle; `Library.Version`/`Library.VersionString` report the linked native version
- `ProducerConfig` and `ConsumerConfig` derive from `ClientConfig`; each is an `IEnumerable<KeyValuePair<string, string>>` consumed by the matching builder
- `Message<TKey, TValue>` derives from `MessageMetadata`; `Timestamp` and `Headers` live on the metadata base
- `DeliveryReport<TKey, TValue>` extends `DeliveryResult<TKey, TValue>` with `Error` and `TopicPartitionOffsetError`; `DeliveryResult` carries `Status` of type `PersistenceStatus`
- `TopicPartitionOffset` carries an optional `LeaderEpoch` alongside topic, partition, and offset
- `Offset` exposes `Beginning`, `End`, `Stored`, and `Unset` sentinels; `Partition` exposes `Any`
- `ISerializer<T>` consumes `ReadOnlySpan<byte>` on the read side via `IDeserializer<T>`; async codecs use `ReadOnlyMemory<byte>` and `ValueTask`

[LOCAL_ADMISSION]:
- Op-log changefeed egress builds one `IProducer<TKey, TValue>` per stream via `ProducerBuilder` with explicit `ISerializer<T>` codecs; the serializer slot is fixed at build, never per-call.
- At-least-once delivery enters through awaited `ProduceAsync`; `DeliveryResult.Status.Persisted` confirms broker persistence, and `NotPersisted` or `PossiblyPersisted` holds the PostgreSQL outbox cursor for retry.
- Broker-ack fold (`Version/egress#EGRESS_SINK`): `DeliveryAck.FromResult(DeliveryResult.Status, detail)` maps `PersistenceStatus.Persisted` to `Persisted(Duplicate: false)` and either ambiguous status to retriable `Indeterminate(detail)`. A caught fatal `ProduceException` maps directly to `Refused(detail)` at the adapter boundary. Raw `DeliveryResult`, `PersistenceStatus`, and broker `Error` values never cross into the pump.
- Dead-letter routing reads `DeliveryReport.Error` and `Error.IsFatal`; fatal errors map to `DeliveryAck.Refused`, while broker ambiguity maps to `DeliveryAck.Indeterminate`. `Error.IsRetriable` is `internal`; public `Error.Code`/`IsFatal`/`IsBrokerError` supply the adapter discrimination.
- Idempotent egress sets `ProducerConfig.EnableIdempotence`. `InitTransactions`, `BeginTransaction`, and `CommitTransaction` provide atomic visibility to `read_committed` Kafka consumers, but they cannot commit a PostgreSQL outbox cursor. `SendOffsetsToTransaction` applies only when the source position is Kafka consumer-group metadata. Content-key consumer dedup closes the crash window between broker persistence and PostgreSQL cursor advance.
- Kafka consumers disable `EnableAutoCommit` and commit through `StoreOffset` plus explicit `Commit`, so the committed offset never outruns durable downstream apply; `ConsumeResult.IsPartitionEOF` marks the live edge and `GetWatermarkOffsets`/`QueryWatermarkOffsets` derive consumer lag.
- `KafkaRetriableException` and `KafkaTxnRequiresAbortException` discriminate transient retry from mandatory `AbortTransaction`; the catch path never collapses both into one rail.
- CloudEvents binding (`Version/egress#EGRESS_SINK`): `cloudEvent.ToKafkaMessage(ContentMode.Binary, JsonEventFormatter)` builds the Kafka `Message<TKey, TValue>`, so CloudEvents attributes (`traceparent`, `redacted`, `sequence`) ride `Headers` and broker filters avoid parsing `Data`; `Confluent.Kafka` owns produce, acknowledgement, and commit, never the envelope shape. CDC egress uses `Serializers.ByteArray` for the already-framed redacted payload; a per-call serializer is rejected.
- OIDC credential stacking (control-plane): a SASL/OAUTHBEARER cluster wires `SetOAuthBearerTokenRefreshHandler` to mint a fresh token from the admitted `OpenIddict.Client` on each librdkafka refresh callback, and `IProducer.SetSaslCredentials` rotates the credential in place without rebuilding the producer; the OAuth handler is the load-bearing seam binding the broker security to the runtime token authority.

[RAIL_LAW]:
- Package: `Confluent.Kafka`
- Owns: librdkafka-backed produce and consume, delivery acknowledgement, offset commit, and transactional CDC egress; one of the named `Version/egress#EGRESS_SINK` messaging-protocol sinks (`Confluent.Kafka`/`NATS.Net`/`RabbitMQ.Client`/`DotPulsar`), each a sink row over the one op-log CloudEvents envelope, never a parallel pump
- Accept: `ProducerBuilder`/`ConsumerBuilder` construction, awaited `ProduceAsync`, `DeliveryResult.Status` folded through `DeliveryAck.FromResult(status, detail)`, explicit `StoreOffset`/`Commit` for Kafka-sourced consumers, `Error`-driven refusal on `Error.Code`/`IsFatal`/`IsBrokerError`, and producer transactions scoped to broker-record visibility
- Reject: hand-rolled Kafka wire framing, fire-and-forget `Produce`, auto-commit on a durable Kafka consumer, raw provider outcomes past the `DeliveryAck` adapter, and a per-protocol broker-ack vocabulary distinct from `Persisted`/`Indeterminate`/`Refused`
