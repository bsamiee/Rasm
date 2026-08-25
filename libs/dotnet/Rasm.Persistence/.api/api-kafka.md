# [RASM_PERSISTENCE_API_KAFKA]

`Confluent.Kafka` owns the librdkafka-backed Kafka client rail: builder-constructed typed producers and consumers, the message envelope over ordered headers, per-message delivery acknowledgement, the offset position algebra, and the codec slots every payload crosses. It carries the at-least-once, dead-letter-capable leg of the op-log changefeed egress — message-envelope shape rides the CloudEvents binding and payload shape the registry codecs, so produce, acknowledgement, commit, and native lifetime are this package's whole concern.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, builder, and native handle

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `IProducer<TKey, TValue>`       | interface     | typed produce, drive, and transaction           |
|  [02]   | `IConsumer<TKey, TValue>`       | interface     | typed consume, assignment, and offset commit    |
|  [03]   | `IClient`                       | interface     | handle, name, broker and SASL mutation          |
|  [04]   | `ProducerBuilder<TKey, TValue>` | class         | config, serializer, partitioner, handler wiring |
|  [05]   | `ConsumerBuilder<TKey, TValue>` | class         | config, deserializer, rebalance-handler wiring  |
|  [06]   | `Handle`                        | class         | opaque librdkafka client reference              |
|  [07]   | `Library`                       | class         | native load state and linked-version probe      |
|  [08]   | `IConsumerGroupMetadata`        | interface     | transactional offset-commit token               |
|  [09]   | `PartitionerDelegate`           | delegate      | per-message partition selection                 |
|  [10]   | `LogMessage`                    | class         | librdkafka facility, level, and text            |

[PUBLIC_TYPE_SCOPE]: message envelope and delivery acknowledgement

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :----------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `Message<TKey, TValue>`        | class         | key and value over the metadata base           |
|  [02]   | `MessageMetadata`              | class         | timestamp and headers carrier                  |
|  [03]   | `Headers`                      | class         | ordered multi-valued header list               |
|  [04]   | `Header`                       | class         | one key over a byte payload                    |
|  [05]   | `IHeader`                      | interface     | key and value-bytes accessor                   |
|  [06]   | `DeliveryResult<TKey, TValue>` | class         | awaited produce outcome with position          |
|  [07]   | `DeliveryReport<TKey, TValue>` | class         | delivery outcome carrying `Error`              |
|  [08]   | `PersistenceStatus`            | enum          | `NotPersisted`/`PossiblyPersisted`/`Persisted` |
|  [09]   | `ConsumeResult<TKey, TValue>`  | class         | fetched message, position, and EOF flag        |

[PUBLIC_TYPE_SCOPE]: position and timestamp algebra

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------ | :------------ | :------------------------------------------ |
|  [01]   | `TopicPartition`          | class         | topic and partition                         |
|  [02]   | `TopicPartitionOffset`    | class         | topic, partition, offset, leader epoch      |
|  [03]   | `TopicPartitionTimestamp` | class         | topic, partition, timestamp                 |
|  [04]   | `Offset`                  | struct        | ordered offset value carrying its sentinels |
|  [05]   | `Partition`               | struct        | partition id with the `Any` sentinel        |
|  [06]   | `Timestamp`               | struct        | unix-ms value with its `TimestampType`      |
|  [07]   | `TimestampType`           | enum          | `NotAvailable`/`CreateTime`/`LogAppendTime` |
|  [08]   | `WatermarkOffsets`        | class         | low and high watermark pair                 |
|  [09]   | `TopicCollection`         | class         | named topic set for describe-side calls     |

[PUBLIC_TYPE_SCOPE]: configuration — each config is an `IEnumerable<KeyValuePair<string, string>>` the matching builder consumes.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :---------------- | :------------ | :----------------------------------------------- |
|  [01]   | `ClientConfig`    | class         | bootstrap, security, SASL, SSL rows              |
|  [02]   | `ProducerConfig`  | class         | idempotence, transaction, buffering, compression |
|  [03]   | `ConsumerConfig`  | class         | group, offset reset, auto-commit, poll interval  |
|  [04]   | `Acks`            | enum          | `None`/`Leader`/`All`                            |
|  [05]   | `AutoOffsetReset` | enum          | `Latest`/`Earliest`/`Error`                      |
|  [06]   | `CompressionType` | enum          | `None`/`Gzip`/`Snappy`/`Lz4`/`Zstd`              |
|  [07]   | `IsolationLevel`  | enum          | `ReadUncommitted`/`ReadCommitted`                |
|  [08]   | `Partitioner`     | enum          | `Random`/`Consistent`/`ConsistentRandom`/murmur  |
|  [09]   | `GroupProtocol`   | enum          | `Classic`/`Consumer` rebalance protocol          |

- `[selector vocabularies]`: `SecurityProtocol` `SaslMechanism` `SaslOauthbearerMethod` `SslEndpointIdentificationAlgorithm` `PartitionAssignmentStrategy` `BrokerAddressFamily` `ClientDnsLookup` `MetadataRecoveryStrategy`.

[PUBLIC_TYPE_SCOPE]: `ProducerConfig` declares twenty properties over `ClientConfig`, every one nullable but `DeliveryReportFields`; each maps to the librdkafka key below and unset means the documented default beside it, so a claim about buffering, ordering, or ack semantics holds only where its key is written or that default is the intent. Producer-side queue depth is the `[BOUND]` coordinate a durable egress leg reads and the retry pair is its `[RETRY_OWNER]`.

| [INDEX] | [PROPERTY]                            | [LIBRDKAFKA_KEY]                          | [CLR_TYPE] | [DEFAULT]           |
| :-----: | :------------------------------------ | :---------------------------------------- | :--------- | :------------------ |
|  [01]   | `QueueBufferingMaxMessages`           | `queue.buffering.max.messages`            | `int?`     | `100000`            |
|  [02]   | `QueueBufferingMaxKbytes`             | `queue.buffering.max.kbytes`              | `int?`     | `1048576`           |
|  [03]   | `QueueBufferingBackpressureThreshold` | `queue.buffering.backpressure.threshold`  | `int?`     | `1`                 |
|  [04]   | `LingerMs`                            | `linger.ms`                               | `double?`  | `5`                 |
|  [05]   | `BatchNumMessages`                    | `batch.num.messages`                      | `int?`     | `10000`             |
|  [06]   | `BatchSize`                           | `batch.size`                              | `int?`     | `1000000`           |
|  [07]   | `StickyPartitioningLingerMs`          | `sticky.partitioning.linger.ms`           | `int?`     | `10`                |
|  [08]   | `MessageSendMaxRetries`               | `message.send.max.retries`                | `int?`     | `2147483647`        |
|  [09]   | `MessageTimeoutMs`                    | `message.timeout.ms`                      | `int?`     | `300000`            |
|  [10]   | `RequestTimeoutMs`                    | `request.timeout.ms`                      | `int?`     | `30000`             |
|  [11]   | `EnableIdempotence`                   | `enable.idempotence`                      | `bool?`    | `false`             |
|  [12]   | `EnableGaplessGuarantee`              | `enable.gapless.guarantee`                | `bool?`    | `false`             |
|  [13]   | `TransactionalId`                     | `transactional.id`                        | `string?`  | empty               |
|  [14]   | `TransactionTimeoutMs`                | `transaction.timeout.ms`                  | `int?`     | `60000`             |
|  [15]   | `CompressionType`                     | `compression.type`                        | enum?      | `none`              |
|  [16]   | `CompressionLevel`                    | `compression.level`                       | `int?`     | `-1`                |
|  [17]   | `Partitioner`                         | `partitioner`                             | enum?      | `consistent_random` |
|  [18]   | `EnableBackgroundPoll`                | `dotnet.producer.enable.background.poll`  | `bool?`    | `true`              |
|  [19]   | `EnableDeliveryReports`               | `dotnet.producer.enable.delivery.reports` | `bool?`    | `true`              |
|  [20]   | `DeliveryReportFields`                | `dotnet.producer.delivery.report.fields`  | `string`   | `all`               |

- `LingerMs` alone reads through `GetDouble`, so an `int?` assumption on that one property mistypes the only fractional-millisecond knob the producer carries.
- `QueueBufferingMaxMessages` and `QueueBufferingMaxKbytes` cap the native produce queue and `QueueBufferingBackpressureThreshold` throttles the internal producer threads ahead of that cap; a full queue REFUSES the produce rather than dropping it, so the durable bound is a refusal a leg folds and never silent loss.
- `MessageSendMaxRetries` is a producer-side RETRY OWNER at `INT32_MAX`, and `MessageTimeoutMs` is its only real bound — "the maximum time librdkafka may use to deliver a message (including retries)", where delivery fails when EITHER the retry count or that timeout is exceeded, and a zero timeout is infinite. Leaving both unset declines nothing — it inherits an effectively unbounded retry behind a five-minute wall.
- `EnableIdempotence` is not one knob but four: enabling it adjusts `max.in.flight.requests.per.connection=5`, `retries=INT32_MAX`, `acks=all`, and `queuing.strategy=fifo` "if not modified by the user", and "Producer instantation will fail if user-supplied configuration is incompatible" — so those four stay foreclosed rather than tunable beside it, and a settings roster carrying any of them carries a value that either restates the forced one or refuses to construct.
- `EnableGaplessGuarantee` ships subject to change or removal by its own doc, requires `enable.idempotence=true`, raises a FATAL `ERR__GAPLESS_GUARANTEE` that stops the producer, and covers no `message.timeout.ms` expiry — so it converts one class of gap into a dead producer and leaves the timeout class uncovered.
- `Acks` and `MaxInFlight` are NOT `ProducerConfig` members: both are inherited from `ClientConfig`, and `MaxInFlight` maps to `max.in.flight` — not `max.in.flight.requests.per.connection`, which no property spells at all.
- `DeliveryReportFields` is the one non-nullable property and its setter calls `value.ToString()` unguarded, so assigning null raises rather than clearing the key.
- `ClientConfig.ThrowIfContainsNonUserConfigurable()` returns `this` and validates nothing, so it never screens a misplaced key.

[PUBLIC_TYPE_SCOPE]: serialization

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `ISerializer<T>`             | interface     | synchronous serialize to `byte[]`              |
|  [02]   | `IDeserializer<T>`           | interface     | span deserialize with an explicit null flag    |
|  [03]   | `IAsyncSerializer<T>`        | interface     | `Task<byte[]>` serialize                       |
|  [04]   | `IAsyncDeserializer<T>`      | interface     | `ReadOnlyMemory<byte>` async deserialize       |
|  [05]   | `Serializers`                | class         | primitive serializer statics                   |
|  [06]   | `Deserializers`              | class         | primitive deserializer statics                 |
|  [07]   | `SerializationContext`       | struct        | component slot, topic, and headers             |
|  [08]   | `MessageComponentType`       | enum          | `Key`/`Value` codec slot discriminant          |
|  [09]   | `SyncOverAsyncSerializer<T>` | class         | async codec mounted on the sync slot           |
|  [10]   | `Null`                       | class         | null-payload marker for a tombstone key        |
|  [11]   | `Ignore`                     | class         | skipped-payload marker for an unread component |

[PUBLIC_TYPE_SCOPE]: error and fault discrimination

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `Error`                          | class         | code, reason, fatal and broker-origin flags |
|  [02]   | `ErrorCode`                      | enum          | librdkafka and broker code vocabulary       |
|  [03]   | `KafkaException`                 | class         | typed error wrapper                         |
|  [04]   | `KafkaRetriableException`        | class         | transient transaction fault                 |
|  [05]   | `KafkaTxnRequiresAbortException` | class         | abort-required transaction fault            |
|  [06]   | `ProduceException<TKey, TValue>` | class         | failed delivery carrying its message        |
|  [07]   | `ConsumeException`               | class         | failed fetch carrying its result            |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction — every builder setter returns the builder, so wiring chains to one `Build()`, and each `Set*Handler` takes `Action<TClient, T>` over the payload type its cell names.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `new ProducerBuilder<TKey, TValue>(config)`                   | ctor     | builder over `ProducerConfig` rows |
|  [02]   | `new ConsumerBuilder<TKey, TValue>(config)`                   | ctor     | builder over `ConsumerConfig` rows |
|  [03]   | `ProducerBuilder.SetKeySerializer(ISerializer<TKey>)`         | instance | key codec slot, sync or async      |
|  [04]   | `ProducerBuilder.SetValueSerializer(ISerializer<TValue>)`     | instance | value codec slot, sync or async    |
|  [05]   | `ConsumerBuilder.SetKeyDeserializer(IDeserializer<TKey>)`     | instance | key codec slot                     |
|  [06]   | `ConsumerBuilder.SetValueDeserializer(IDeserializer<TValue>)` | instance | value codec slot                   |
|  [07]   | `ProducerBuilder.SetDefaultPartitioner(PartitionerDelegate)`  | instance | fallback partition selection       |
|  [08]   | `ProducerBuilder.SetPartitioner(string, PartitionerDelegate)` | instance | per-topic partition selection      |
|  [09]   | `ProducerBuilder.Build()` / `ConsumerBuilder.Build()`         | factory  | mints the client, codec slots set  |

- `[builder diagnostics]` on either builder: `SetErrorHandler(Error)` `SetLogHandler(LogMessage)` `SetStatisticsHandler(string)` `SetOAuthBearerTokenRefreshHandler(string)`. Every one is SINGLE-SHOT, raising `InvalidOperationException` on a second set, while `SetPartitioner`/`SetDefaultPartitioner` raise `ArgumentException` instead; a statistics-handler exception routes to the error handler, and error- and log-handler exceptions are swallowed outright, so neither reaches a rail.
- `[rebalance hooks]` on `ConsumerBuilder`: `SetPartitionsAssignedHandler` `SetPartitionsRevokedHandler` `SetPartitionsLostHandler` `SetOffsetsCommittedHandler` — each takes the `Func<…, IEnumerable<TopicPartitionOffset>>` start-offset override or the `Action<…>` observer form.
- `SetPartitionsAssignedHandler(Func<IConsumer<TKey, TValue>, List<TopicPartition>, IEnumerable<TopicPartitionOffset>>)` returns the START offsets replacing the committed position; the `Action<IConsumer<TKey, TValue>, List<TopicPartition>>` overload adapts onto that same backing slot, so setting either form twice raises `InvalidOperationException` and one builder binds exactly one assigned handler.
- `SetPartitionsRevokedHandler`'s `Func` variant sets `RevokedOrLostHandlerIsFunc`, which is incompatible with incremental (cooperative) rebalancing; a cooperative consumer takes the `Action` form and reaches `IncrementalAssign(IEnumerable<TopicPartitionOffset>)`/`IncrementalAssign(IEnumerable<TopicPartition>)` beside the four whole-assignment `Assign` overloads.
- Assigned and revoked handlers run on the POLL thread inside `Consume`, so their wall time is charged against `ConsumerConfig.MaxPollIntervalMs` (`public int? MaxPollIntervalMs` → `max.poll.interval.ms`), and an exception thrown from one re-raises out of `Consume` rather than failing the rebalance alone.
- `OffsetsForTimes(IEnumerable<TopicPartitionTimestamp>, TimeSpan)` resolves the whole enumerable in one batched broker exchange, so its cost past a fixed round trip grows only marginally with partition count while an unreachable leader consumes the passed `TimeSpan` in full and then raises `KafkaException` carrying `ErrorCode.Local_TimedOut` — that timeout, never the assignment width, is what a caller budgets against the poll interval, and splitting one call into N multiplies both the round trip and that timeout.

[ENTRYPOINT_SCOPE]: produce, drive, and transaction on `IProducer<TKey, TValue>`

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `ProduceAsync(string, Message<TKey, TValue>, ct)`                     | instance | awaited broker acknowledgement   |
|  [02]   | `Produce(string, Message<TKey, TValue>, handler)`                     | instance | enqueue with a delivery callback |
|  [03]   | `Poll(TimeSpan) -> int`                                               | instance | serves queued delivery callbacks |
|  [04]   | `Flush(TimeSpan) -> int` / `Flush(ct)`                                | instance | drains the produce queue         |
|  [05]   | `InitTransactions(TimeSpan)`                                          | instance | registers the producer           |
|  [06]   | `BeginTransaction()`                                                  | instance | opens a transaction window       |
|  [07]   | `SendOffsetsToTransaction(offsets, IConsumerGroupMetadata, TimeSpan)` | instance | binds consumed offsets to it     |
|  [08]   | `CommitTransaction()` / `CommitTransaction(TimeSpan)`                 | instance | commits the produced batch       |
|  [09]   | `AbortTransaction()` / `AbortTransaction(TimeSpan)`                   | instance | discards the produced batch      |

- `ProduceAsync` and `Produce` each carry a `TopicPartition` overload pinning the partition; the `Produce` callback takes `Action<DeliveryReport<TKey, TValue>>`, so the report carries `Error` where the awaited path throws `ProduceException<TKey, TValue>`.
- `Error` lives on `DeliveryReport` alone. `ProduceException.DeliveryResult` is typed `DeliveryResult<TKey, TValue>` and NOT the report, so a caught produce fault reads its `Error` off the base `KafkaException`, and the result's `Key`/`Value`/`Timestamp`/`Headers` proxies dereference `Message` unguarded and raise on the null one a refusal leaves.
- `Flush` is two members with two return types — `int Flush(TimeSpan)` answering the remaining out-queue length and `void Flush(CancellationToken)` raising `OperationCanceledException`; neither is async and no `FlushAsync` exists.
- `InitTransactions` and `SendOffsetsToTransaction` carry ONLY their `TimeSpan` overloads, and `AddBrokers` on `IClient` is the broker-mutation member — no `AddBrokerAddresses` spelling exists.

[ENTRYPOINT_SCOPE]: subscribe, fetch, and commit on `IConsumer<TKey, TValue>`

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Subscribe(string)` / `Subscribe(IEnumerable<string>)` | instance | joins the group subscription            |
|  [02]   | `Unsubscribe()`                                        | instance | leaves the subscription                 |
|  [03]   | `Assign(TopicPartition)` / `Assign(offsets)`           | instance | replaces the whole assignment           |
|  [04]   | `Unassign()`                                           | instance | drops the whole assignment              |
|  [05]   | `IncrementalAssign(offsets)`                           | instance | cooperative partition add               |
|  [06]   | `IncrementalUnassign(IEnumerable<TopicPartition>)`     | instance | cooperative partition remove            |
|  [07]   | `Consume(ct)` / `Consume(TimeSpan)` / `Consume(int)`   | instance | one `ConsumeResult` or a blocked wait   |
|  [08]   | `Seek(TopicPartitionOffset)`                           | instance | repositions the fetch cursor            |
|  [09]   | `Pause(IEnumerable<TopicPartition>)`                   | instance | halts fetch, assignment held            |
|  [10]   | `Resume(IEnumerable<TopicPartition>)`                  | instance | resumes fetch                           |
|  [11]   | `GetWatermarkOffsets(TopicPartition)`                  | instance | cached low/high watermark               |
|  [12]   | `QueryWatermarkOffsets(TopicPartition, TimeSpan)`      | instance | broker-queried low/high watermark       |
|  [13]   | `OffsetsForTimes(timestamps, TimeSpan)`                | instance | first offset at or after each timestamp |
|  [14]   | `StoreOffset(ConsumeResult<TKey, TValue>)`             | instance | marks the result offset for commit      |
|  [15]   | `StoreOffset(TopicPartitionOffset)`                    | instance | marks an explicit offset for commit     |
|  [16]   | `Commit() -> List<TopicPartitionOffset>`               | instance | commits every stored offset             |
|  [17]   | `Commit(IEnumerable<TopicPartitionOffset>)`            | instance | commits an explicit offset list         |
|  [18]   | `Commit(ConsumeResult<TKey, TValue>)`                  | instance | commits one result offset               |
|  [19]   | `Committed(TimeSpan)` / `Committed(partitions, t)`     | instance | committed offsets, assigned or explicit |
|  [20]   | `Position(TopicPartition) -> Offset`                   | instance | current consume position                |
|  [21]   | `PositionTopicPartitionOffset(TopicPartition)`         | static   | position as a commit-ready triple       |
|  [22]   | `Assignment` / `Subscription` / `MemberId`             | property | live assignment, topics, broker id      |
|  [23]   | `ConsumerGroupMetadata`                                | property | token the producer transaction takes    |
|  [24]   | `Close()`                                              | instance | leaves the group, commits final state   |

- `ConsumeResult.IsPartitionEOF` marks a poll that reached the live edge with no record, so the result carries a position and no `Message`. `StoreOffset(ConsumeResult)` and `Commit(ConsumeResult)` both raise `InvalidOperationException` on such a result, and both derive the committed position as `Offset + 1`.
- `Close()` is the ONE teardown: `Dispose` alone releases the handle "without committing offsets and without alerting the group coordinator that the consumer is exiting the group", leaving the group to rebalance only after `session.timeout.ms`. `Close` also RE-RAISES a statistics- or log-handler exception the client stashed earlier, so a teardown crossing carries a throw unrelated to the close itself.
- `Headers.GetLastBytes` raises `KeyNotFoundException` where the `Try` form answers false, `Remove(string)` drops EVERY value under the key, and `Header` publishes no `Value` property — the bytes reach a reader through `GetValueBytes()` alone.
- `IConsumerExtensions.PositionTopicPartitionOffset` extends `IConsumer<TKey, TValue>` and rethrows a per-partition fault as `KafkaException`.

[ENTRYPOINT_SCOPE]: runtime mutation on a live `IClient` — `ClientExtensions` carries the OAUTHBEARER hand-off as extension methods.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `AddBrokers(string) -> int`                              | instance | adds bootstrap brokers, no rebuild       |
|  [02]   | `SetSaslCredentials(string, string)`                     | instance | rotates SASL credentials in place        |
|  [03]   | `OAuthBearerSetToken(string, long, string, IDictionary)` | static   | hands a minted token to the refresh hook |
|  [04]   | `OAuthBearerSetTokenFailure(string)`                     | static   | signals a token-mint failure             |

- `[IClient]`: `Handle` `Name`. `[Library]`: `Load(string)` `IsLoaded` `HandleCount` `VersionString` `Version` `DebugContexts`.

[ENTRYPOINT_SCOPE]: message, header, and position construction

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `new Message<TKey, TValue> { Key, Value }`                        | ctor     | key and value over the metadata base  |
|  [02]   | `new Headers()` / `Headers.Add(string, byte[])`                   | instance | appends a byte-valued header          |
|  [03]   | `Headers.GetLastBytes(string)` / `.TryGetLastBytes(string, out)`  | instance | latest value; `Try` form never throws |
|  [04]   | `Headers.Remove(string)` / `.BackingList` / `.Count`              | instance | drops a key; ordered read access      |
|  [05]   | `new Header(string, byte[])` / `IHeader.GetValueBytes()`          | ctor     | one header and its bytes              |
|  [06]   | `asyncSerializer.AsSyncOverAsync()`                               | static   | mounts an async codec on a sync slot  |
|  [07]   | `new SerializationContext(MessageComponentType, string, Headers)` | ctor     | codec context for one slot            |
|  [08]   | `new TopicPartitionOffset(TopicPartition, Offset, int?)`          | ctor     | commit position with a leader epoch   |

- `[Serializers]`: `Utf8` `Null` `Int32` `Int64` `Single` `Double` `ByteArray`.
- `[Deserializers]`: `Utf8` `Null` `Ignore` `Int32` `Int64` `Single` `Double` `ByteArray`.
- `[Offset]`: `Beginning` `End` `Stored` `Unset`; `Partition.Any` defers placement to the partitioner.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One handle owns one client: `ProducerConfig`/`ConsumerConfig` derive from `ClientConfig` as string key-value rows, the builder folds them with the codec slots into a native librdkafka handle, and every runtime mutation (`SetSaslCredentials`, the OAUTHBEARER hand-off, `AddBrokers`) acts on that live handle rather than minting a second one.
- Codec slots are symmetric and set-once: serialize takes the value and a `SerializationContext` returning `byte[]` or `Task<byte[]>`, deserialize takes `ReadOnlySpan<byte>`/`ReadOnlyMemory<byte>` with an explicit `isNull` flag, and `AsSyncOverAsync()` is the one adapter mounting an async codec on a sync slot.
- Outcome types nest rather than branch: `Message<TKey, TValue>` derives from `MessageMetadata`, `DeliveryReport` extends `DeliveryResult` with `Error` and `TopicPartitionOffsetError`, and `ConsumeResult` carries the same position triple, so one position algebra spans produce, consume, and commit.

[STACKING]:
- `CloudNative.CloudEvents.Kafka`(`.api/api-cloudevents-kafka.md`): `cloudEvent.ToKafkaMessage(ContentMode.Binary, formatter)` mints the exact `Message<string?, byte[]>` `ProduceAsync` takes and `message.ToCloudEvent(formatter, extensions)` inverts it at the consume leg, so message-envelope attributes ride `ce_*` `Headers` and this package never reads `Data`.
- `Confluent.SchemaRegistry.Serdes.*`(`.api/api-schemaregistry-serdes-json.md`): registry codecs realize `IAsyncSerializer<T>`/`IAsyncDeserializer<T>` and mount straight on the async codec slots, `AsSyncOverAsync()` mounting them on a sync slot; the registry frames the schema id into the payload bytes this package treats as opaque.
- `OpenTelemetry.Instrumentation.ConfluentKafka`(`libs/dotnet/.api/api-otel-instrumentation-confluentkafka.md`): `AsInstrumentedProducerBuilder`/`AsInstrumentedConsumerBuilder` lift a fully configured builder, so `Build()` mints a span- and meter-emitting client with no call-site change and the drain trace continues to the broker ack.
- `NATS.Net`/`RabbitMQ.Client`/`DotPulsar`(`libs/dotnet/.api/api-nats.md`, `.api/api-rabbitmq.md`, `.api/api-dotpulsar.md`): peer binding rows over the one op-log message envelope, each folding its own provider outcome to the shared `DeliveryAck` vocabulary at its own adapter boundary.
- Within the package, one built client carries the whole egress leg: the awaited `ProduceAsync` supersedes the `Produce` and `Poll` callback drive, `Flush` bounds shutdown, `InitTransactions`/`BeginTransaction`/`CommitTransaction` bracket a read-committed batch, and `SendOffsetsToTransaction` closes the consume-transform-produce loop when the source position is Kafka consumer-group metadata.

[LOCAL_ADMISSION]:
- Op-log changefeed egress builds one `IProducer<TKey, TValue>` per stream through `ProducerBuilder` with explicit `ISerializer<T>` codecs; the serializer slot is fixed at build, never per-call, and CDC egress binds `Serializers.ByteArray` for the already-framed redacted payload.
- Delivery enters through awaited `ProduceAsync`, and `KafkaAck.FromResult(DeliveryResult.Status, detail)` maps `Persisted` to `Persisted(Duplicate: false)` and either ambiguous status to retriable `Indeterminate`; a caught fatal `ProduceException` maps to `Refused` through `KafkaAck.FromError` at the same boundary owner, and raw `DeliveryResult`, `PersistenceStatus`, and `Error` values stop there — the shared `DeliveryAck` union itself names no type from this package.
- `Error.IsRetriable` and `Error.TxnRequiresAbort` are `internal`, so adapter discrimination reads `Error.Code`, `IsFatal`, and `IsBrokerError`, and `KafkaRetriableException` versus `KafkaTxnRequiresAbortException` splits a transient retry from a mandatory `AbortTransaction`.
- `ProducerConfig.EnableIdempotence` deduplicates broker-side retries and the transaction bracket gives `ReadCommitted` consumers atomic visibility, both scoped to broker records alone; the PostgreSQL outbox cursor advances outside that scope, so content-key consumer dedupe closes the crash window between broker persistence and cursor advance. Egress states `MessageTimeoutMs` explicitly because idempotence forces the retry count to `INT32_MAX` and that timeout becomes the leg's whole attempt bound; it states none of the four properties idempotence forces, since supplying any of them either restates the forced value or refuses to construct the producer.
- Kafka consumers disable `EnableAutoCommit` and commit through `StoreOffset` with an explicit `Commit`, so a committed offset never outruns durable downstream apply; `GetWatermarkOffsets`/`QueryWatermarkOffsets` derive lag and `OffsetsForTimes` resolves a replay cursor from a wall-clock `TopicPartitionTimestamp`.
- `SetOAuthBearerTokenRefreshHandler` wires a SASL/OAUTHBEARER cluster so each librdkafka refresh callback mints a token from the admitted `OpenIddict.Client` and hands it back through `OAuthBearerSetToken`, or reports a mint failure through `OAuthBearerSetTokenFailure`; `SetSaslCredentials` rotates a PLAIN or SCRAM credential without rebuilding the client.
