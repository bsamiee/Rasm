# [RASM_PERSISTENCE_API_DOTPULSAR]

`DotPulsar` is the official Apache Pulsar `.NET` client speaking the native binary protocol: the `PulsarClient` root (built via `PulsarClient.Builder()`) mints `IProducer<TMessage>`, `IConsumer<TMessage>` (durable subscriptions with `Exclusive`/`Shared`/`Failover`/`KeyShared` types and `Persistent`/`NonPersistent` regex topics), and `IReader<TMessage>` (cursorless replay from a `MessageId`). The send/receive/acknowledge/seek verbs ride small focused interfaces (`ISend`/`IReceive`/`ISeek`/`ISendChannel`) and extension methods (`NewMessage`, `Send`, `Acknowledge`/`AcknowledgeCumulative`, `Process`, `Messages`), schemas span `ISchema<T>` (`Schema.ByteArray`/`String`/`Json<T>`/`Protobuf<T>`/`AvroISpecificRecord<T>`/`AvroGenericRecord<T>`), and the client exposes `IState`/`IStateHolder` reactive state monitoring plus a built-in `ActivitySource`/`Meter` telemetry surface. It backs the `EgressSink.Pulsar` sink and a distinct log-streaming ingress backend (separated compute/storage, tiered storage), distinct from the Kafka (`api-kafka`), NATS JetStream (`api-nats`), and RabbitMQ (`api-rabbitmq`) egress wire protocols; it composes the admitted `Google.Protobuf` (`Schema.Protobuf<T>`) and `Chr.Avro` (`Schema.Avro*`) seams as its payload schemas and folds its `ActivitySource` into the AppHost `telemetry` port.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DotPulsar`
- package: `DotPulsar`
- license: Apache-2.0
- assembly: `DotPulsar`
- namespace: `DotPulsar` (concrete + enums), `DotPulsar.Abstractions` (the `I*` client/builder/message/schema/state contracts), `DotPulsar.Extensions` (the verb surface), `DotPulsar.Schemas` (built-in schema types), `DotPulsar.Exceptions` (the typed failure family); `DotPulsar.Internal.*` (connection pool, `PulsarApi` protobuf wire, compression) is implementation — not a consumer API
- target: multi-target (`net10.0`, `net9.0`, `net8.0`, `netstandard2.1`, `netstandard2.0`); the `net10.0` consumer binds `lib/net10.0` (the bound asset, highest-precedence)
- native: pure-managed (no `runtimes/<rid>/native` payload); the binary protocol rides `System.IO.Pipelines` over a TCP socket. Built-in compression covers `Lz4`/`Zlib`/`Zstd`/`Snappy` (managed codecs)
- transitive (the `net10.0` nuspec dependency group): `Google.Protobuf` (nuspec floor `3.34.1`; the substrate pins `Google.Protobuf@3.35.1` higher and is what resolves — the wire-protocol framing AND the `Schema.Protobuf<T>` payload codec), `Microsoft.Bcl.AsyncInterfaces@10.0.7`, `Microsoft.Bcl.HashCode@6.0.0`, `Microsoft.Extensions.ObjectPool@10.0.7`, `System.Collections.Immutable@10.0.7`, `System.Diagnostics.DiagnosticSource@10.0.7` (the `ActivitySource`/`Meter`), `System.IO.Pipelines@10.0.7`, `System.Text.Json@10.0.7` (the `Schema.Json<T>` codec)
- xml docs: present (`DotPulsar.xml` ships per TFM — member intent is doc-comment-sourced)
- rail: egress-sink

The async surface is `ValueTask`-based throughout; `IPulsarClient`/`IProducer`/`IConsumer`/`IReader` are all `IAsyncDisposable`. Send payloads are `ReadOnlySequence<byte>` (zero-copy) when the schema is raw bytes, or `TMessage` under a typed `ISchema<T>`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, builder, and producer/consumer/reader contracts
- rail: egress-sink

`IPulsarClient` is the connection-pool root (`PulsarClient.Builder()` → `IPulsarClientBuilder.Build()`); it mints the three client kinds from typed options. Each client interface composes the verb mix-ins (`ISend`/`IReceive`/`ISeek`) plus `IStateHolder<TState>` for reactive state.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :----------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `IPulsarClient`                | client root      | `CreateProducer/Consumer/Reader<TMessage>(options)`; `IAsyncDisposable` |
|  [02]   | `IPulsarClientBuilder`         | client builder   | service url, auth, TLS, encryption policy, keep-alive, exception handler |
|  [03]   | `IProducer<TMessage>`          | producer client  | `ISend<TMessage>` + `SendChannel`; `IStateHolder<ProducerState>` |
|  [04]   | `IProducerBuilder<TMessage>`   | producer builder | topic, compression, access mode, router, pending cap, properties |
|  [05]   | `IConsumer<TMessage>`          | consumer client  | `IReceive<IMessage<TMessage>>` + ack/seek/redeliver; `IStateHolder<ConsumerState>` |
|  [06]   | `IConsumerBuilder<TMessage>`   | consumer builder | subscription name/type, initial position, topics/pattern, prefetch, compacted |
|  [07]   | `IReader<TMessage>`            | reader client    | `IReceive<IMessage<TMessage>>` + seek; cursorless replay from `MessageId` |
|  [08]   | `IReaderBuilder<TMessage>`     | reader builder   | topic, start `MessageId`, prefetch, compacted, name |
|  [09]   | `IConsumer` / `IProducer` / `IReader` | base contracts | `ServiceUrl`/`Topic`; consumer base owns `Acknowledge`/`Unsubscribe`/`RedeliverUnacknowledgedMessages` |

[PUBLIC_TYPE_SCOPE]: verb mix-ins, message, and metadata
- rail: egress-sink

The send/receive/seek verbs are small composable interfaces so a producer/consumer/reader exposes exactly the verbs it supports. `IMessage<TValue>` is the received envelope (id, payload, key, event/publish time, properties, redelivery count); `MessageMetadata` is the produce-side carrier.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :----------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `ISend<TMessage>`              | send verb        | `Send(MessageMetadata, TMessage, ct)` → `ValueTask<MessageId>` |
|  [02]   | `ISendChannel<TMessage>`       | send channel     | buffered `Send(..., onMessageSent)`, `Complete()`, `Completion()` |
|  [03]   | `IReceive<TMessage>`           | receive verb     | `Receive(ct)` → `ValueTask<TMessage>`           |
|  [04]   | `ISeek`                        | seek verb        | `Seek(MessageId)` / `Seek(ulong publishTime)`   |
|  [05]   | `IGetLastMessageIds`           | lag probe        | `GetLastMessageIds(ct)` → topic head positions  |
|  [06]   | `IMessageBuilder<TMessage>`    | message builder  | key/orderingKey/eventTime/deliverAt/properties/sequenceId → `Send` |
|  [07]   | `IMessage<TValue>` / `IMessage` | received message | `Value()`, `MessageId`, `Data`, `Key`, `EventTime*`, `Properties`, `RedeliveryCount` |
|  [08]   | `MessageMetadata`              | produce metadata | `Key`/`KeyBytes`/`OrderingKey`/`SequenceId`/`EventTime*`/`DeliverAt*`; property indexer; `SetCompressionInfo` |
|  [09]   | `MessageId`                    | message position | `Earliest`/`Latest`; `LedgerId`/`EntryId`/`Partition`/`BatchIndex`/`Topic`; comparable; `TryParse` |

[PUBLIC_TYPE_SCOPE]: options, schema, routing, auth, and state
- rail: egress-sink

`ProducerOptions<T>`/`ConsumerOptions<T>`/`ReaderOptions<T>` each require an `ISchema<T>`. `Schema` is the static schema factory; `ProcessingOptions` tunes the `Process` auto-ack pump; `IState<T>`/`IStateHolder<T>` expose reactive state with `OnStateChangeTo`/`From`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]    | [RAIL]                                       |
| :-----: | :-------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `ProducerOptions<TMessage>`       | producer options | topic, `Schema`, compression, access mode, router, pending cap |
|  [02]   | `ConsumerOptions<TMessage>`       | consumer options | subscription name/type, `Schema`, topics/pattern, initial position, prefetch |
|  [03]   | `ReaderOptions<TMessage>`         | reader options   | start `MessageId`, topic, `Schema`, prefetch, compacted |
|  [04]   | `ProcessingOptions`               | process pump     | `MaxDegreeOfParallelism`, `MaxMessagesPerTask`, `EnsureOrderedAcknowledgment`, `ShutdownGracePeriod` |
|  [05]   | `ISchema<T>`                      | schema contract  | `Encode(T)`/`Decode(bytes, schemaVersion?)`, `SchemaInfo` |
|  [06]   | `Schema` (static)                 | schema factory   | `ByteArray`/`ByteSequence`/`String`/`Boolean`/`Int8..Int64`/`Float`/`TimeStamp`/`Date`/`Time`; `Json<T>`/`Protobuf<T>`/`AvroISpecificRecord<T>`/`AvroGenericRecord<T>` |
|  [07]   | `SchemaInfo`                      | schema descriptor | name, data bytes, `SchemaType`, properties    |
|  [08]   | `IMessageRouter` / `RoundRobinPartitionRouter` / `SinglePartitionRouter` | partition router | `ChoosePartition(metadata, n)`; MurmurHash3 on key bytes |
|  [09]   | `AuthenticationFactory`           | auth factory     | `Token(string)` / `Token(supplier)` / `Basic(user, pw)` / `Basic(supplier)` |
|  [10]   | `IAuthentication`                 | auth contract    | `AuthenticationMethodName`, `GetAuthenticationData(ct)` |
|  [11]   | `IState<TState>` / `IStateHolder<TState>` | reactive state | `OnStateChangeTo/From(state, ct)`, `IsFinalState` |
|  [12]   | `ConsumerStateChanged`/`ProducerStateChanged`/`ReaderStateChanged` | state event | the client + the state it changed to          |
|  [13]   | `ExceptionContext`                | fault context    | `Exception`, `ExceptionHandled`, `Result` (`FaultAction`) |

[PUBLIC_TYPE_SCOPE]: enums and exceptions
- rail: egress-sink

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]    | [RAIL]                                       |
| :-----: | :-------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `SubscriptionType`                | subscription enum | `Exclusive`/`Shared`/`Failover`/`KeyShared` |
|  [02]   | `SubscriptionInitialPosition`     | position enum    | `Latest`/`Earliest`                          |
|  [03]   | `RegexSubscriptionMode`           | topic-mode enum  | `Persistent`/`NonPersistent`/`All`           |
|  [04]   | `ProducerAccessMode`              | access enum      | `Shared`/`Exclusive`/`WaitForExclusive`/`ExclusiveWithFencing` |
|  [05]   | `CompressionType`                 | codec enum       | `None`/`Lz4`/`Zlib`/`Zstd`/`Snappy`          |
|  [06]   | `SchemaType`                      | schema enum      | `None`/`String`/`Json`/`Protobuf`/`Avro`/primitives/`KeyValue`/`ProtobufNative` |
|  [07]   | `EncryptionPolicy`                | TLS enum         | `EnforceUnencrypted`..`EnforceEncrypted`     |
|  [08]   | `FaultAction`                     | fault enum       | `Rethrow`/`ThrowException`/`Retry`           |
|  [09]   | `ConsumerState`/`ProducerState`/`ReaderState` | state enum | lifecycle states incl. final `Closed`/`Faulted`/`Fenced`/`ReachedEndOfTopic` |
|  [10]   | `DotPulsarException` (+ ~40 subtypes) | failure family | `ProducerFencedException`, `ConsumerFaultedException`, `TooLargeMessageException`, `TransactionConflictException`, ... |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client build and client creation
- rail: egress-sink

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `PulsarClient.Builder()`                                   | factory        | yields `IPulsarClientBuilder`                |
|  [02]   | `.ServiceUrl(uri)` / `.Authentication(IAuthentication)`    | builder        | broker address + auth provider               |
|  [03]   | `.ConnectionSecurity(EncryptionPolicy)` / `.TrustedCertificateAuthority(cert)` / `.AuthenticateUsingClientCertificate(cert)` | builder | TLS posture + mTLS |
|  [04]   | `.KeepAliveInterval(t)` / `.RetryInterval(t)` / `.CloseInactiveConnectionsInterval(t)` | builder | connection timing |
|  [05]   | `.ExceptionHandler(Action<ExceptionContext>)` / `.ExceptionHandler(Func<…, ValueTask>)` | builder | custom fault handler (sets `FaultAction`) |
|  [06]   | `.RemoteCertificateValidation(Func<…, bool>)`              | builder        | custom server-cert validation                |
|  [07]   | `.Build()`                                                 | factory call   | yields `IPulsarClient`                       |
|  [08]   | `CreateProducer<TMessage>(ProducerOptions<TMessage>)`      | client         | `IProducer<TMessage>`                        |
|  [09]   | `CreateConsumer<TMessage>(ConsumerOptions<TMessage>)`      | client         | `IConsumer<TMessage>`                        |
|  [10]   | `CreateReader<TMessage>(ReaderOptions<TMessage>)`          | client         | `IReader<TMessage>`                          |

[ENTRYPOINT_SCOPE]: produce
- rail: egress-sink

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new ProducerOptions<TMessage>(topic, schema)` + `{ CompressionType, ProducerAccessMode, MessageRouter, MaxPendingMessages }` | object init | producer config; default `RoundRobinPartitionRouter` |
|  [02]   | `producer.Send(message, ct)` (extension)                   | produce        | awaits broker ack → `MessageId`              |
|  [03]   | `producer.Send(MessageMetadata, message, ct)`              | produce        | keyed/timed produce → `MessageId`            |
|  [04]   | `sender.Send(byte[] / ReadOnlyMemory<byte>, ct)` (raw-bytes ext) | produce  | `ISend<ReadOnlySequence<byte>>` byte overloads |
|  [05]   | `producer.NewMessage()` → `IMessageBuilder<TMessage>`      | message build  | fluent `.Key(...).EventTime(...).Property(...).Send(message, ct)` |
|  [06]   | `producer.SendChannel`                                     | buffered send  | `ISendChannel`: fire-with-callback, `Complete()`, `Completion()` |
|  [07]   | `AuthenticationFactory.Token(token)` / `.Basic(user, pw)`  | auth           | `IAuthentication` for the client builder      |
|  [08]   | `Schema.Json<T>()` / `Schema.Protobuf<T>()` / `Schema.AvroISpecificRecord<T>()` / `Schema.ByteArray` | schema | the typed `ISchema<T>` for the options |

[ENTRYPOINT_SCOPE]: consume, acknowledge, and process
- rail: egress-sink

`Process` is the auto-acknowledge pump: it receives, runs the processor concurrently up to `ProcessingOptions.MaxDegreeOfParallelism`, and acks on success (ordered if `EnsureOrderedAcknowledgment`). For manual control, `Receive`/`Messages` plus explicit `Acknowledge`/`AcknowledgeCumulative` give at-least-once with cumulative or individual ack.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new ConsumerOptions<TMessage>(subscriptionName, topic, schema)` (+ `IEnumerable<string>` / `Regex` topic overloads) | object init | subscription config |
|  [02]   | `consumer.Receive(ct)`                                     | consume        | one `IMessage<TMessage>`                     |
|  [03]   | `consumer.Messages(ct)` (extension)                        | consume stream | `IAsyncEnumerable<IMessage<TMessage>>`       |
|  [04]   | `consumer.TryReceive(out message)` (extension)             | consume probe  | non-blocking buffered receive                |
|  [05]   | `consumer.Acknowledge(message / MessageId, ct)`            | ack            | individual ack                               |
|  [06]   | `consumer.AcknowledgeCumulative(message / MessageId, ct)`  | ack            | cumulative ack up to and including           |
|  [07]   | `consumer.RedeliverUnacknowledgedMessages(ids?, ct)`       | nack           | request redelivery                           |
|  [08]   | `consumer.Process(Func<IMessage<TMessage>, CancellationToken, ValueTask>, ProcessingOptions?, ct)` | auto-ack pump | concurrent process + auto-acknowledge |
|  [09]   | `consumer.Unsubscribe(ct)`                                 | teardown       | leaves and deletes the subscription          |

[ENTRYPOINT_SCOPE]: read, seek, and reactive state
- rail: egress-sink

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new ReaderOptions<TMessage>(MessageId.Earliest, topic, schema)` | object init | cursorless replay from a position           |
|  [02]   | `reader.Messages(ct)` / `reader.Receive(ct)`               | read           | replay stream / one message                  |
|  [03]   | `seeker.Seek(MessageId)` / `Seek(ulong publishTime)` / `Seek(DateTime/Offset)` | seek | reposition consumer or reader cursor         |
|  [04]   | `consumer.GetLastMessageIds(ct)` / `reader.GetLastMessageIds(ct)` | lag probe | topic head positions for lag computation     |
|  [05]   | `client.State.OnStateChangeTo(state, ct)` / `.OnStateChangeFrom(...)` | state watch | await a `ProducerState`/`ConsumerState`/`ReaderState` |
|  [06]   | `producer.StateChangedTo(ProducerState, ct)` (extension)   | state watch    | typed state-change await                     |
|  [07]   | `entity.DelayedStateMonitor(state, delay, onLeft, onReached, ct)` | state monitor | reconnect-aware state callback loop          |

## [04]-[IMPLEMENTATION_LAW]

[DOTPULSAR_TOPOLOGY]:
- protocol: native Pulsar binary protocol over `System.IO.Pipelines`; the wire framing is `Google.Protobuf` (`DotPulsar.Internal.PulsarApi`). One `PulsarClient` owns a connection pool shared across producers/consumers/readers; it is `IAsyncDisposable` and disposes all minted children.
- subscription model: durable subscriptions are named (`ConsumerOptions.SubscriptionName`) with a `SubscriptionType` — `Exclusive`/`Failover` (ordered, single active consumer) vs `Shared` (round-robin) vs `KeyShared` (per-key affinity). Pulsar's separated compute/storage means a `Reader` replays from any `MessageId` without a server cursor; a `Consumer` commits acks server-side.
- delivery: at-least-once via explicit `Acknowledge`/`AcknowledgeCumulative`; the `Process` extension auto-acks after a successful processor invocation (`EnsureOrderedAcknowledgment` preserves order under `MaxDegreeOfParallelism`). `RedeliverUnacknowledgedMessages` requests redelivery (negative ack).
- routing: a partitioned-topic producer routes via `IMessageRouter` — `RoundRobinPartitionRouter` (default) or `SinglePartitionRouter`, both hashing on `MessageMetadata.KeyBytes` (MurmurHash3) when a key is set so same-key messages land on the same partition.
- compression: managed `Lz4`/`Zlib`/`Zstd`/`Snappy` selected via `ProducerOptions.CompressionType`; no native codec.
- state: every client is an `IStateHolder<TState>` — `OnStateChangeTo`/`From` and `StateChangedHandler` give reactive lifecycle (final states `Closed`/`Faulted`/`Fenced`/`ReachedEndOfTopic` terminate the await). A `WaitForExclusive` producer access mode is the Pulsar leader-election primitive (the exclusive producer is the WAL leader).

[SCHEMA_VOCABULARY]:
- `ISchema<T>` is the payload codec on both producer and consumer options (`Encode`/`Decode` over `ReadOnlySequence<byte>` with an optional schema version). `Schema` is the factory: raw (`ByteArray`/`ByteSequence`), primitives (`String`/`Boolean`/`Int8..Int64`/`Float`/`TimeStamp`/`Date`/`Time`), and the structured `Json<T>` (System.Text.Json), `Protobuf<T> where T : IMessage<T>` (Google.Protobuf), `AvroISpecificRecord<T>`/`AvroGenericRecord<T>` (Apache Avro).
- the raw-bytes path (`ISend<ReadOnlySequence<byte>>`) is the zero-copy egress codec when the op payload is already framed — the byte overloads of `Send`/`IMessageBuilder.Send` avoid a per-message serializer.

[LOCAL_ADMISSION]:
- DotPulsar enters behind the `Version/egress#EGRESS_SINK` vocabulary as the `EgressSink.Pulsar` sink and a distinct log-streaming ingress backend, orthogonal to the Kafka/NATS/RabbitMQ wire protocols.
- the client lifecycle (connection pool, producer/consumer/reader handles) is egress-profile ceremony — `IAsyncDisposable` resources bracketed by the sink, not ambient singletons.
- subscription name, type, initial position, and ack policy are sink policy declared on the egress profile, not chosen per-message.

[STACKING]:
- egress payload codec: the CDC-egress op payload (already a redacted, framed byte buffer) sends through the raw-bytes `ISend<ReadOnlySequence<byte>>.Send(byte[], ct)` with the CloudEvents attributes carried as `MessageMetadata` properties (the property indexer) so a broker filters on metadata without decoding `Data` — Pulsar owns only the produce/ack/seek, never the envelope shape. A typed sink instead binds `Schema.Protobuf<T>()` (`api-schemaregistry-serdes-protobuf` / `Google.Protobuf`) or `Schema.Avro*` (`api-chr-avro`) so the payload schema rides the message and consumers evolve safely.
- exactly-once egress: `ProducerAccessMode.WaitForExclusive` + the reactive `IState` await elects one WAL-leader producer per partition; the awaited `Send` `MessageId` (`LedgerId:EntryId:Partition:BatchIndex`) confirms the durable offset advance for the op-log, and a `ProducerFencedException` on the loser triggers re-election.
- ingress replay: `IReader<TMessage>` from a stored `MessageId` (or `Seek(publishTime)`) replays the Pulsar topic deterministically into the `Version/ledger` ingress rail — Pulsar's tiered storage makes the replay window long-lived, distinct from a cursor-bound consumer.
- back-pressure: `GetLastMessageIds` against the consumer position derives subscription lag for the egress back-pressure shed, the same lag-probe shape as the Kafka watermark seam (`api-kafka`).
- telemetry: DotPulsar's built-in `ActivitySource`/`Meter` (`DotPulsar.Internal.DotPulsarActivitySource`/`DotPulsarMeter`, via `System.Diagnostics.DiagnosticSource`) is registered with the AppHost OpenTelemetry tracer (`telemetry` port) — produce/consume spans and client-created/disposed meters ride the shared provider, not a bespoke logger.
- fault rail: the `DotPulsarException` family (`ProducerFencedException`, `ConsumerFaultedException`, `TooLargeMessageException`, `TransactionConflictException`, ...) lifts at the sink edge into the egress failure rail; `ExceptionContext.Result` (`FaultAction.Retry`/`Rethrow`/`ThrowException`) is the registered handler's verdict, never a blanket catch.

[RAIL_LAW]:
- Package: `DotPulsar`
- Owns: native Pulsar binary-protocol produce/consume/read, durable subscriptions, acks, cursorless replay, schema-typed payloads, reactive state, and built-in `ActivitySource`/`Meter` telemetry
- Accept: `PulsarClient.Builder()` construction, typed `ProducerOptions/ConsumerOptions/ReaderOptions` with an explicit `ISchema<T>`, awaited `Send`/`Acknowledge` for at-least-once egress, `Process` auto-ack with bounded parallelism, `WaitForExclusive` leader-election for exactly-once, and `ActivitySource` telemetry through the AppHost tracer
- Reject: hand-rolled Pulsar wire framing, treating `DotPulsar.Internal.*` as a consumer API, fire-and-forget `SendChannel` without `Completion()` on the at-least-once path, a missing `ISchema<T>` (it is required on every options object), and surfacing the `DotPulsarException` family as a single collapsed error rail
