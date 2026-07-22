# [RASM_PERSISTENCE_API_DOTPULSAR]

`DotPulsar` speaks the native Apache Pulsar binary protocol as a pure-managed client, backing `EgressSink.Pulsar` and a distinct log-streaming ingress backend (`Version/egress#EGRESS_SINK`). Separated compute/storage makes `IReader` replay cursorless from any `MessageId` — the trait distinguishing Pulsar from the Kafka (`api-kafka`), NATS JetStream (`api-nats`), and RabbitMQ (`api-rabbitmq`) egress protocols. `Google.Protobuf` (`Schema.Protobuf<T>`) and `Chr.Avro` (`Schema.Avro*`) are the typed payload codecs, and the built-in `ActivitySource`/`Meter` folds into the AppHost `telemetry` port.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DotPulsar`
- package: `DotPulsar` (Apache-2.0)
- assembly: `DotPulsar`
- namespace: `DotPulsar` (concrete + enums), `DotPulsar.Abstractions` (client/builder/message/schema/state contracts), `DotPulsar.Extensions` (verb surface), `DotPulsar.Schemas` (built-in schemas), `DotPulsar.Exceptions` (failure family); `DotPulsar.Internal.*` is implementation, never a consumer API
- target: multi-target; the `net10.0` consumer binds `lib/net10.0`
- native: pure-managed, no `runtimes/<rid>/native` payload; the binary protocol rides `System.IO.Pipelines` over a TCP socket, compression managed
- depends: `Google.Protobuf` (wire framing + `Schema.Protobuf<T>`), `System.Diagnostics.DiagnosticSource` (`ActivitySource`/`Meter`), `System.Text.Json` (`Schema.Json<T>`)
- rail: egress-sink

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, builder, and producer/consumer/reader contracts

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]    | [CAPABILITY]                                                             |
| :-----: | :------------------------------------ | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `IPulsarClient`                       | client root      | `CreateProducer/Consumer/Reader<TMessage>(options)`; `IAsyncDisposable`  |
|  [02]   | `IPulsarClientBuilder`                | client builder   | service url, auth, TLS, encryption, keep-alive, fault handler            |
|  [03]   | `IProducer<TMessage>`                 | producer client  | `ISend<TMessage>` + `SendChannel`; `IStateHolder<ProducerState>`         |
|  [04]   | `IProducerBuilder<TMessage>`          | producer builder | topic, compression, access mode, router, pending cap, properties         |
|  [05]   | `IConsumer<TMessage>`                 | consumer client  | `IReceive<IMessage<TMessage>>` + ack/seek; `IStateHolder<ConsumerState>` |
|  [06]   | `IConsumerBuilder<TMessage>`          | consumer builder | subscription name/type, initial position, topics/pattern, prefetch       |
|  [07]   | `IReader<TMessage>`                   | reader client    | `IReceive<IMessage<TMessage>>` + seek; cursorless `MessageId` replay     |
|  [08]   | `IReaderBuilder<TMessage>`            | reader builder   | topic, start `MessageId`, prefetch, compacted, name                      |
|  [09]   | `IConsumer` / `IProducer` / `IReader` | base contracts   | `ServiceUrl`/`Topic`; consumer base owns ack/unsubscribe/redeliver       |

[PUBLIC_TYPE_SCOPE]: verb mix-ins, message, and metadata

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :-------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `ISend<TMessage>`           | send verb       | `Send(MessageMetadata, TMessage, ct)` → `ValueTask<MessageId>`     |
|  [02]   | `ISendChannel<TMessage>`    | send channel    | buffered `Send(..., onMessageSent)`, `Complete()`, `Completion()`  |
|  [03]   | `IReceive<TMessage>`        | receive verb    | `Receive(ct)` → `ValueTask<TMessage>`                              |
|  [04]   | `ISeek`                     | seek verb       | `Seek(MessageId)` / `Seek(ulong publishTime)`                      |
|  [05]   | `IGetLastMessageIds`        | lag probe       | `GetLastMessageIds(ct)` → topic head positions                     |
|  [06]   | `IMessageBuilder<TMessage>` | message builder | key/orderingKey/eventTime/deliverAt/properties/sequenceId → `Send` |

- `IMessage<TValue>`/`IMessage` received envelope: `Value()` `MessageId` `Data` `Key` `EventTime*` `Properties` `RedeliveryCount`
- `MessageMetadata` produce carrier: `Key` `KeyBytes` `OrderingKey` `SequenceId` `EventTime*` `DeliverAt*`, property indexer, `SetCompressionInfo`
- `MessageId` position: `LedgerId` `EntryId` `Partition` `BatchIndex` `Topic`, static `Earliest`/`Latest`, `TryParse`, comparable

[PUBLIC_TYPE_SCOPE]: options and schema

Each `ProducerOptions<T>`/`ConsumerOptions<T>`/`ReaderOptions<T>` requires an `ISchema<T>`; `ProcessingOptions` tunes the `Process` auto-ack pump.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [CAPABILITY]                                                                 |
| :-----: | :-------------------------- | :---------------- | :--------------------------------------------------------------------------- |
|  [01]   | `ProducerOptions<TMessage>` | producer options  | topic, `Schema`, compression, access mode, router, pending cap               |
|  [02]   | `ConsumerOptions<TMessage>` | consumer options  | subscription name/type, `Schema`, topics/pattern, initial position, prefetch |
|  [03]   | `ReaderOptions<TMessage>`   | reader options    | start `MessageId`, topic, `Schema`, prefetch, compacted                      |
|  [04]   | `ProcessingOptions`         | process pump      | `Process` auto-ack pump tuning (roster below)                                |
|  [05]   | `ISchema<T>`                | schema contract   | `Encode(T)`/`Decode(bytes, schemaVersion?)`, `SchemaInfo`                    |
|  [06]   | `Schema` (static)           | schema factory    | raw, primitive, and structured `ISchema<T>` factories (roster below)         |
|  [07]   | `SchemaInfo`                | schema descriptor | name, data bytes, `SchemaType`, properties                                   |

[SCHEMA_FACTORY]: `Schema.ByteArray` `ByteSequence` `String` `Boolean` `Int8` `Int16` `Int32` `Int64` `Float` `TimeStamp` `Date` `Time` `Json<T>` `Protobuf<T>` `AvroISpecificRecord<T>` `AvroGenericRecord<T>`
[PROCESSING_OPTIONS]: `MaxDegreeOfParallelism` `MaxMessagesPerTask` `EnsureOrderedAcknowledgment` `ShutdownGracePeriod`

[PUBLIC_TYPE_SCOPE]: routing, auth, and reactive state

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]    | [CAPABILITY]                                              |
| :-----: | :---------------------------------------- | :--------------- | :-------------------------------------------------------- |
|  [01]   | `IMessageRouter`                          | partition router | `ChoosePartition(metadata, n)` contract                   |
|  [02]   | `RoundRobinPartitionRouter`               | partition router | default; MurmurHash3 on key bytes                         |
|  [03]   | `SinglePartitionRouter`                   | partition router | pins every message to one partition                       |
|  [04]   | `AuthenticationFactory`                   | auth factory     | `Token(...)` / `Basic(...)` in string and supplier forms  |
|  [05]   | `IAuthentication`                         | auth contract    | `AuthenticationMethodName`, `GetAuthenticationData(ct)`   |
|  [06]   | `IState<TState>` / `IStateHolder<TState>` | reactive state   | `OnStateChangeTo/From(state, ct)`, `IsFinalState`         |
|  [07]   | `ConsumerStateChanged`                    | state event      | the client + the state it changed to                      |
|  [08]   | `ProducerStateChanged`                    | state event      | the client + the state it changed to                      |
|  [09]   | `ReaderStateChanged`                      | state event      | the client + the state it changed to                      |
|  [10]   | `ExceptionContext`                        | fault context    | `Exception`, `ExceptionHandled`, `Result` (`FaultAction`) |

[PUBLIC_TYPE_SCOPE]: enums and exceptions

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]     | [CAPABILITY]                                                                    |
| :-----: | :---------------------------- | :---------------- | :------------------------------------------------------------------------------ |
|  [01]   | `SubscriptionType`            | subscription enum | `Exclusive`/`Shared`/`Failover`/`KeyShared`                                     |
|  [02]   | `SubscriptionInitialPosition` | position enum     | `Latest`/`Earliest`                                                             |
|  [03]   | `RegexSubscriptionMode`       | topic-mode enum   | `Persistent`/`NonPersistent`/`All`                                              |
|  [04]   | `ProducerAccessMode`          | access enum       | `Shared`/`Exclusive`/`WaitForExclusive`/`ExclusiveWithFencing`                  |
|  [05]   | `CompressionType`             | codec enum        | `None`/`Lz4`/`Zlib`/`Zstd`/`Snappy`                                             |
|  [06]   | `SchemaType`                  | schema enum       | `None`/`String`/`Json`/`Protobuf`/`Avro`/primitives/`KeyValue`/`ProtobufNative` |
|  [07]   | `EncryptionPolicy`            | TLS enum          | `EnforceUnencrypted`..`EnforceEncrypted`                                        |
|  [08]   | `FaultAction`                 | fault enum        | `Rethrow`/`ThrowException`/`Retry`                                              |
|  [09]   | `ConsumerState`               | state enum        | lifecycle; finals `Closed`/`Faulted`/`Fenced`/`ReachedEndOfTopic`               |
|  [10]   | `ProducerState`               | state enum        | lifecycle; finals `Closed`/`Faulted`/`Fenced`/`ReachedEndOfTopic`               |
|  [11]   | `ReaderState`                 | state enum        | lifecycle; finals `Closed`/`Faulted`/`Fenced`/`ReachedEndOfTopic`               |
|  [12]   | `DotPulsarException`          | failure family    | lifts onto the egress fault rail                                                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client build and client creation

| [INDEX] | [SURFACE]                                             | [SHAPE]      | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------- | :----------- | :---------------------------------------- |
|  [01]   | `PulsarClient.Builder()`                              | factory      | yields `IPulsarClientBuilder`             |
|  [02]   | `.ServiceUrl(uri)`                                    | builder      | broker address                            |
|  [03]   | `.Authentication(IAuthentication)`                    | builder      | auth provider                             |
|  [04]   | `.ConnectionSecurity(EncryptionPolicy)`               | builder      | TLS posture                               |
|  [05]   | `.TrustedCertificateAuthority(cert)`                  | builder      | trusted CA for TLS                        |
|  [06]   | `.AuthenticateUsingClientCertificate(cert)`           | builder      | mTLS client certificate                   |
|  [07]   | `.KeepAliveInterval(t)`                               | builder      | keep-alive timing                         |
|  [08]   | `.RetryInterval(t)`                                   | builder      | reconnect retry timing                    |
|  [09]   | `.CloseInactiveConnectionsInterval(t)`                | builder      | idle-connection reaping                   |
|  [10]   | `.ExceptionHandler(Action<ExceptionContext>)`         | builder      | custom fault handler (sets `FaultAction`) |
|  [11]   | `.ExceptionHandler(Func<…, ValueTask>)`               | builder      | async fault handler                       |
|  [12]   | `.RemoteCertificateValidation(Func<…, bool>)`         | builder      | custom server-cert validation             |
|  [13]   | `.Build()`                                            | factory call | yields `IPulsarClient`                    |
|  [14]   | `CreateProducer<TMessage>(ProducerOptions<TMessage>)` | client       | `IProducer<TMessage>`                     |
|  [15]   | `CreateConsumer<TMessage>(ConsumerOptions<TMessage>)` | client       | `IConsumer<TMessage>`                     |
|  [16]   | `CreateReader<TMessage>(ReaderOptions<TMessage>)`     | client       | `IReader<TMessage>`                       |

[ENTRYPOINT_SCOPE]: produce

| [INDEX] | [SURFACE]                                                 | [SHAPE]       | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `new ProducerOptions<TMessage>(topic, schema)`            | object init   | producer config; default `RoundRobinPartitionRouter`       |
|  [02]   | `producer.Send(message, ct)` (extension)                  | produce       | awaits broker ack → `MessageId`                            |
|  [03]   | `producer.Send(MessageMetadata, message, ct)`             | produce       | keyed/timed produce → `MessageId`                          |
|  [04]   | `sender.Send(byte[] / ReadOnlyMemory<byte>, ct)`          | produce       | raw-bytes `ISend<ReadOnlySequence<byte>>` overloads        |
|  [05]   | `producer.NewMessage()` → `IMessageBuilder<TMessage>`     | message build | fluent `.Key(...).EventTime(...).Property(...).Send(...)`  |
|  [06]   | `producer.SendChannel`                                    | buffered send | `ISendChannel`: fire-callback, `Complete()`/`Completion()` |
|  [07]   | `AuthenticationFactory.Token(token)` / `.Basic(user, pw)` | auth          | `IAuthentication` for the client builder                   |
|  [08]   | `Schema.Json<T>()`                                        | schema        | typed JSON `ISchema<T>` (System.Text.Json)                 |
|  [09]   | `Schema.Protobuf<T>()`                                    | schema        | typed Protobuf `ISchema<T>` (Google.Protobuf)              |
|  [10]   | `Schema.AvroISpecificRecord<T>()`                         | schema        | typed Avro `ISchema<T>` (specific record)                  |
|  [11]   | `Schema.ByteArray`                                        | schema        | raw-bytes `ISchema<byte[]>`                                |

[ENTRYPOINT_SCOPE]: consume, acknowledge, and process

`ConsumerOptions` takes a single `topic`, an `IEnumerable<string>`, or a `Regex` topic overload.

| [INDEX] | [SURFACE]                                                        | [SHAPE]        | [CAPABILITY]                           |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `new ConsumerOptions<TMessage>(subscriptionName, topic, schema)` | object init    | subscription config                    |
|  [02]   | `consumer.Receive(ct)`                                           | consume        | one `IMessage<TMessage>`               |
|  [03]   | `consumer.Messages(ct)` (extension)                              | consume stream | `IAsyncEnumerable<IMessage<TMessage>>` |
|  [04]   | `consumer.TryReceive(out message)` (extension)                   | consume probe  | non-blocking buffered receive          |
|  [05]   | `consumer.Acknowledge(message / MessageId, ct)`                  | ack            | individual ack                         |
|  [06]   | `consumer.AcknowledgeCumulative(message / MessageId, ct)`        | ack            | cumulative ack up to and including     |
|  [07]   | `consumer.RedeliverUnacknowledgedMessages(ids?, ct)`             | nack           | request redelivery                     |
|  [08]   | `consumer.Process(processor, ProcessingOptions?, ct)`            | auto-ack pump  | concurrent process + auto-acknowledge  |
|  [09]   | `consumer.Unsubscribe(ct)`                                       | teardown       | leaves and deletes the subscription    |

[ENTRYPOINT_SCOPE]: read, seek, and reactive state

| [INDEX] | [SURFACE]                                                             | [SHAPE]       | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `new ReaderOptions<TMessage>(MessageId.Earliest, topic, schema)`      | object init   | cursorless replay from a position        |
|  [02]   | `reader.Messages(ct)` / `reader.Receive(ct)`                          | read          | replay stream / one message              |
|  [03]   | `seeker.Seek(MessageId)`                                              | seek          | reposition cursor at a `MessageId`       |
|  [04]   | `seeker.Seek(ulong publishTime)`                                      | seek          | reposition cursor at a publish time      |
|  [05]   | `seeker.Seek(DateTime/Offset)`                                        | seek          | reposition cursor at a timestamp         |
|  [06]   | `consumer.GetLastMessageIds(ct)` / `reader.GetLastMessageIds(ct)`     | lag probe     | topic head positions for lag computation |
|  [07]   | `client.State.OnStateChangeTo(state, ct)` / `.OnStateChangeFrom(...)` | state watch   | await the client state change            |
|  [08]   | `producer.StateChangedTo(ProducerState, ct)` (extension)              | state watch   | typed state-change await                 |
|  [09]   | `entity.DelayedStateMonitor(state, delay, onLeft, onReached, ct)`     | state monitor | reconnect-aware state callback loop      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- protocol: native Pulsar binary protocol over `System.IO.Pipelines` on a TCP socket; `Google.Protobuf` frames the wire (`DotPulsar.Internal.PulsarApi`). One `PulsarClient` owns the shared connection pool and, as `IAsyncDisposable`, disposes every producer/consumer/reader it minted; the async surface is `ValueTask`-based throughout.
- surface shape: send/receive/seek verbs ride small composable interfaces (`ISend`/`IReceive`/`ISeek`/`ISendChannel`) so a client composes only the verbs it implements, and `IProducer`/`IConsumer`/`IReader` are each `IAsyncDisposable`.
- subscription model: named durable subscriptions carry a `SubscriptionType` — `Exclusive`/`Failover` (ordered, single active consumer), `Shared` (round-robin), `KeyShared` (per-key affinity). A `Reader` replays from any `MessageId` with no server cursor; a `Consumer` commits acks server-side.
- delivery: at-least-once via explicit `Acknowledge`/`AcknowledgeCumulative`; `Process` auto-acks after a successful processor invocation, `EnsureOrderedAcknowledgment` preserving order under `MaxDegreeOfParallelism`, and `RedeliverUnacknowledgedMessages` is the negative ack.
- routing: a partitioned-topic producer routes via `IMessageRouter` — `RoundRobinPartitionRouter` (default) or `SinglePartitionRouter`, both MurmurHash3 over `MessageMetadata.KeyBytes` so same-key messages land on one partition.
- payload codec: `ISchema<T>` encodes/decodes over `ReadOnlySequence<byte>` with an optional schema version; the raw-bytes path (`ISend<ReadOnlySequence<byte>>`) is the zero-copy egress codec for an already-framed payload, avoiding a per-message serializer.
- state: every client is an `IStateHolder<TState>` whose `OnStateChangeTo`/`From` drive reactive lifecycle, the final states `Closed`/`Faulted`/`Fenced`/`ReachedEndOfTopic` terminating the await; `WaitForExclusive` producer access is the leader-election primitive, the exclusive producer being the WAL leader.
- compression: managed `Lz4`/`Zlib`/`Zstd`/`Snappy` via `ProducerOptions.CompressionType`, no native codec.

[STACKING]:
- egress payload codec: the CDC-egress op payload (already a redacted, framed byte buffer) sends through the raw-bytes `ISend<ReadOnlySequence<byte>>.Send(byte[], ct)` with CloudEvents attributes carried as `MessageMetadata` properties, so a broker filters on metadata without decoding `Data`. A typed sink instead binds `Schema.Protobuf<T>()` (`api-schemaregistry-serdes-protobuf`, `Google.Protobuf`) or `Schema.Avro*` (`api-chr-avro`) so the schema rides the message.
- exactly-once egress: `ProducerAccessMode.WaitForExclusive` with the reactive `IState` await elects one WAL-leader producer per partition; the awaited `Send` `MessageId` (`LedgerId:EntryId:Partition:BatchIndex`) confirms the durable offset advance, and a `ProducerFencedException` on the loser triggers re-election.
- ingress replay: `IReader<TMessage>` from a stored `MessageId` (or `Seek(publishTime)`) replays the topic deterministically into the `Version/ledger` ingress rail; tiered storage makes the replay window long-lived, distinct from a cursor-bound consumer.
- back-pressure: `GetLastMessageIds` against the consumer position derives subscription lag for the egress back-pressure shed — the same lag-probe shape as the Kafka watermark seam (`api-kafka`).
- telemetry: the built-in `ActivitySource`/`Meter` registers with the AppHost OpenTelemetry tracer (`telemetry` port) so produce/consume spans and client-created/disposed meters ride the shared provider, never a bespoke logger.
- fault rail: the `DotPulsarException` family (`ProducerFencedException`, `ConsumerFaultedException`, `TooLargeMessageException`, `TransactionConflictException`) lifts at the sink edge onto the egress failure rail; `ExceptionContext.Result` (`FaultAction.Retry`/`Rethrow`/`ThrowException`) is the registered handler's verdict.

[LOCAL_ADMISSION]:
- DotPulsar enters behind the `Version/egress#EGRESS_SINK` vocabulary as `EgressSink.Pulsar` and a distinct log-streaming ingress backend, orthogonal to the Kafka/NATS/RabbitMQ wire protocols.
- client lifecycle (connection pool, producer/consumer/reader handles) is egress-profile ceremony — `IAsyncDisposable` resources bracketed by the sink, never ambient singletons.
- subscription name, type, initial position, and ack policy are sink policy declared on the egress profile, never chosen per-message.

[RAIL_LAW]:
- Package: `DotPulsar`
- Owns: native Pulsar binary-protocol produce/consume/read, durable subscriptions, acks, cursorless replay, schema-typed payloads, reactive state, and built-in `ActivitySource`/`Meter` telemetry
- Accept: `PulsarClient.Builder()` construction, typed `ProducerOptions`/`ConsumerOptions`/`ReaderOptions` with an explicit `ISchema<T>`, awaited `Send`/`Acknowledge` for at-least-once egress, `Process` auto-ack under bounded parallelism, `WaitForExclusive` leader-election for exactly-once, and `ActivitySource` telemetry through the AppHost tracer
- Reject: hand-rolled Pulsar wire framing, `DotPulsar.Internal.*` as a consumer API, fire-and-forget `SendChannel` without `Completion()` on the at-least-once path, a missing `ISchema<T>`, and the `DotPulsarException` family collapsed onto one error rail
