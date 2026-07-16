# [RASM_PERSISTENCE_API_NATS]

`NATS.Net` is the full NATS protocol owner backing the `Version/egress#EGRESS_SINK` `EgressSink.Nats(Bind, Subject)` row (`ContentMode.Binary`): Core pub/sub + request/reply for fire-and-forget and RPC, and JetStream durable streams/consumers/acks/replay for the awaited-broker-ack leg — `INatsJSContext.PublishAsync` → `PubAckResponse` is the durable Settle-ack the sink leg folds to `DeliveryAck` (`Persisted`, `Persisted(Duplicate: true)` on the dedup-window replay, `Indeterminate` on a server `-ERR`/timeout, `Refused` only on a fatal protocol fault), `PubAckResponse.Duplicate` is the `Nats-Msg-Id` dedup-window flag the content-key replay-absorption stance rides, and `INatsJSConsumer.ConsumeAsync` + `INatsJSMsg.AckAsync`/`NakAsync`/`AckTerminateAsync` is the cursor-replay drain a downstream consumer owns independently of the PostgreSQL outbox cursor. Beyond messaging it adds two distinct store-backend capabilities the embedded/relational tiers lack: JetStream KeyValue (revisioned CAS KV with `WatchAsync`/`HistoryAsync` — the distributed counterpart to the `LightningDB`/`rocksdb` embedded engines and a `Store/coordination` fenced-CAS substrate) and JetStream Object Store (chunked object storage with `SealAsync`/`AddLinkAsync`). The serializer registry (`INatsSerializerRegistry`) is the codec seam where the `MessagePack`/`System.Text.Json` snapshot of a `[ValueObject]`/`[SmartEnum]` owner crosses the wire — NATS frames the bytes, the codec owns the shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NATS.Net` (meta-package)
- package: `NATS.Net`
- assembly: `NATS.Net` (a ~5.6 KB facade exposing the `NATS.Net.NatsClient` simplified entry; the real surface lives in the sub-client assemblies it fans out to)
- license: Apache-2.0 (`<license type="expression">Apache-2.0</license>`)
- target framework: `net8.0` asset on the `net10.0` floor (package ships `net8.0`/`net6.0`/`netstandard2.1`/`netstandard2.0`; net8.0 wins NuGet precedence — there is no `net10.0` asset)
- asset: runtime library, pure-managed AnyCPU (TCP/WebSocket NATS protocol; no native)
- fan-out (the eight sub-clients the meta-package transitively pins, each a real assembly — pin the meta-package, consume the sub-client namespaces): `NATS.Client.Core`, `NATS.Client.JetStream`, `NATS.Client.KeyValueStore`, `NATS.Client.ObjectStore`, `NATS.Client.Services`, `NATS.Client.Hosting`, `NATS.Client.Simplified`, `NATS.Client.Serializers.Json` (plus `NATS.Client.Abstractions` and `NATS.NKeys` transitives)
- rail: messaging-protocol (NATS) + jetstream-store-backend

[SUB_CLIENT_MAP]: which namespace owns which concern
- rail: messaging-protocol

| [INDEX] | [ASSEMBLY]                     | [NAMESPACE]                                | [OWNS]                                                     |
| :-----: | :----------------------------- | :----------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `NATS.Client.Core`             | `NATS.Client.Core`                         | connection, pub/sub, request/reply, serializers, TLS, auth |
|  [02]   | `NATS.Client.JetStream`        | `NATS.Client.JetStream`(`.Models`)         | durable streams, consumers, publish-ack, consume/fetch     |
|  [03]   | `NATS.Client.KeyValueStore`    | `NATS.Client.KeyValueStore`                | JetStream KV — revisioned CAS, watch, history              |
|  [04]   | `NATS.Client.ObjectStore`      | `NATS.Client.ObjectStore`                  | JetStream Object Store — chunked objects, links, seal      |
|  [05]   | `NATS.Client.Services`         | `NATS.Client.Services`                     | the NATS micro-services protocol (discovery, stats, ping)  |
|  [06]   | `NATS.Client.Hosting`          | `Microsoft.Extensions.DependencyInjection` | `AddNats` DI registration + connection pool                |
|  [07]   | `NATS.Client.Simplified`       | `NATS.Net`                                 | `NatsClient` entry + `NatsClientDefaultSerializerRegistry` |
|  [08]   | `NATS.Client.Serializers.Json` | `NATS.Client.Serializers.Json`             | the System.Text.Json serializer registry leg               |

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: the client, connection, options, and message — `NATS.Client.Core` / `NATS.Net`; the message type is `NatsMsg<T> : INatsMsg<T>`
- rail: messaging-protocol

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]    | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------------ | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `NATS.Net.NatsClient : INatsClient`               | simplified entry | one-line client; `.Connection` exposes `INatsConnection`          |
|  [02]   | `INatsConnection : INatsClient, IAsyncDisposable` | connection       | the full connection — pub/sub/request + lifecycle events          |
|  [03]   | `NatsConnection : INatsConnection`                | connection       | the concrete connection (`new NatsConnection(NatsOpts)`)          |
|  [04]   | `INatsConnectionPool` / `NatsConnectionPool`      | pool             | multi-connection pool for throughput fan-out                      |
|  [05]   | `NatsOpts` (sealed record)                        | options          | url, name, serializer registry, TLS, auth, ping, buffer           |
|  [06]   | `NatsMsg<T>` (readonly record struct)             | message          | `Subject`/`Data`/`Headers`/`ReplyTo`/`Connection`; `ReplyAsync`   |
|  [07]   | `NatsHeaders : IDictionary<string, StringValues>` | headers          | the `traceparent`/`Nats-Msg-Id` carrier                           |
|  [08]   | `NatsResult` / `NatsResult<T>` (readonly struct)  | result rail      | the `Try*` non-throwing `Success`/`Error` — native ROP            |
|  [09]   | `NatsAuthOpts`                                    | auth             | creds/NKey/JWT auth options                                       |
|  [10]   | `NatsAuthCred`                                    | auth             | a single credential (creds/NKey/JWT)                              |
|  [11]   | `NatsTlsOpts`                                     | transport        | TLS mode                                                          |
|  [12]   | `NatsWebSocketOpts`                               | transport        | WebSocket transport                                               |
|  [13]   | `NatsStats` (readonly record struct)              | telemetry        | `SentBytes`/`ReceivedBytes`/`PendingMessages`/`SubscriptionCount` |
|  [14]   | `NatsInstrumentationOptions`                      | telemetry        | the OpenTelemetry span options                                    |
|  [15]   | `NatsInstrumentationContext`                      | telemetry        | the span context (subject/headers/connection/`ParentContext`)     |
|  [16]   | `Nuid`                                            | id               | NATS unique id generator (subject/inbox tokens)                   |

[SERIALIZER_TYPES]: the codec registry seam — `NATS.Client.Core` / `NATS.Client.Serializers.Json`
- rail: messaging-protocol

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :---------------------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `INatsSerializerRegistry`           | registry      | the per-`NatsOpts` codec selector — `GetSerializer<T>`/`GetDeserializer<T>` |
|  [02]   | `INatsSerialize<T>`                 | codec         | the per-type serialize contract                                             |
|  [03]   | `INatsDeserialize<T>`               | codec         | the per-type deserialize contract                                           |
|  [04]   | `INatsSerializer<T>`                | codec         | the combined serialize/deserialize contract                                 |
|  [05]   | `NatsRawSerializer<T>`              | codec         | raw `byte[]`/`ReadOnlyMemory<byte>` passthrough — snapshot-bytes carrier    |
|  [06]   | `NatsUtf8PrimitivesSerializer<T>`   | codec         | UTF-8 primitive serializer (string/number)                                  |
|  [07]   | `NatsJsonContextSerializer<T>`      | codec         | source-generated `JsonSerializerContext` codec (AOT-safe)                   |
|  [08]   | `NatsJsonContextSerializerRegistry` | registry      | the source-generated JSON registry                                          |
|  [09]   | `NatsJsonSerializer<T>`             | codec         | reflection `System.Text.Json` codec + options form                          |
|  [10]   | `NatsJsonSerializerRegistry`        | registry      | the reflection JSON registry (`.Serializers.Json`)                          |
|  [11]   | `NatsSerializerBuilder<T>`          | builder       | chains a fallback serializer pipeline                                       |

[FAULT_TYPES]: the `NatsException` family — `NATS.Client.Core`
- rail: messaging-protocol

`NatsException` base with closed sub-cases: `NatsNoRespondersException` (request to a subject with no responder — the RPC-target-absent), `NatsNoReplyException` (request timed out awaiting reply), `NatsServerException` (a `-ERR` from the server), `NatsPayloadTooLargeException` (over `max_payload`), `NatsConnectionFailedException`, `NatsProtocolViolationException`, `NatsTimeoutException`, `NatsDeserializeException`/`NatsHeaderParseException` (codec/header decode), `NatsSubException` (subscription fault carrying `NatsSubEndReason`). `NatsServerErrorKind` enumerates the server `-ERR` classes; `NatsMsgFlags` is the per-message bit (`Empty`/`NoResponders`).

## [03]-[ENTRYPOINTS]

[CONNECTION]: connect and the lifecycle events — `INatsClient` / `INatsConnection`
- rail: messaging-protocol

| [INDEX] | [SURFACE]                                                             | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `new NatsClient(url, name, credsFile?)`                               | construct      | the simplified client (creds-file form)         |
|  [02]   | `new NatsConnection(NatsOpts)`                                        | construct      | the connection from `NatsOpts`                  |
|  [03]   | `ConnectAsync()` / `ReconnectAsync()` → `ValueTask`                   | lifecycle      | establish / force-reconnect                     |
|  [04]   | `PingAsync(ct)` → `ValueTask<TimeSpan>`                               | health         | round-trip latency probe                        |
|  [05]   | `INatsConnection` events                                              | events         | connection-health stream (events in `[EVENTS]`) |
|  [06]   | `services.AddNats(poolSize, configureOpts, configureConnection, key)` | DI             | `NATS.Client.Hosting` pooled registration       |

[EVENTS]: the `INatsConnection` lifecycle events feeding the telemetry/health spine.
- events: `ConnectionOpened`/`ConnectionDisconnected`/`ReconnectFailed`/`MessageDropped`/`SlowConsumerDetected`/`LameDuckModeActivated`/`ServerError`
- signals: `LameDuckModeActivated` = graceful-server-drain, `SlowConsumerDetected` = back-pressure

[CORE_PUBSUB]: pub / sub / request-reply — the `EgressSink.Nats` Core leg; each op is generic `<T>` and takes a trailing `serializer?, opts?, CancellationToken`
- rail: messaging-protocol

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :--------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `PublishAsync<T>(subject, data, headers?, replyTo?)` | publish        | → `ValueTask`; fire-and-forget at-most-once `Settle`         |
|  [02]   | `PublishAsync<T>(in NatsMsg<T> msg)`                 | publish        | → `ValueTask`; publish a pre-built message (headers/reply)   |
|  [03]   | `SubscribeAsync<T>(subject, queueGroup?)`            | subscribe      | → `IAsyncEnumerable<NatsMsg<T>>`; `queueGroup` load-balances |
|  [04]   | `SubscribeCoreAsync<T>(…)`                           | subscribe      | → `INatsSub<T>`; lower-level handle (manual drain)           |
|  [05]   | `RequestAsync<TRequest, TReply>(subject, data)`      | request        | → `ValueTask<NatsMsg<TReply>>`; request/reply RPC            |
|  [06]   | `RequestManyAsync<TRequest, TReply>(…)`              | scatter-gather | → `IAsyncEnumerable<NatsMsg<TReply>>`; many replies streamed |

[JETSTREAM]: durable streams, publish-ack, consume — the awaited Settle-ack ceremony (`NATS.Client.JetStream`); receivers `INatsJSContext` (publish/admin), `INatsJSConsumer` (drain), `INatsJSMsg<T>` (ack), `INatsJSStream` (read/purge); async, trailing `serializer?/opts?/ct`; drain/fetch ops yield `IAsyncEnumerable<INatsJSMsg<T>>`
- rail: jetstream-store-backend

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]  | [CAPABILITY]                                             |
| :-----: | :--------------------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `new NatsJSContext(connection)`                            | context factory | → `INatsJSContext`; the base JS-context view             |
|  [02]   | `new NatsJSContext(connection, NatsJSOpts opts)`           | context factory | → `INatsJSContext`; constructor-only construction        |
|  [03]   | `.PublishAsync<T>(subject, data, headers?)`                | durable publish | → `ValueTask<PubAckResponse>`; at-least-once ack         |
|  [04]   | `.TryPublishAsync<T>(…)`                                   | durable publish | → `ValueTask<NatsResult<PubAckResponse>>`; ROP publish   |
|  [05]   | `.PublishConcurrentAsync<T>(…)`                            | pipelined       | → `NatsJSPublishConcurrentFuture`; deferred-ack batches  |
|  [06]   | `.CreateStreamAsync(StreamConfig)`                         | stream admin    | → `INatsJSStream`; provision a stream (`StreamConfig`)   |
|  [07]   | `.CreateOrUpdateStreamAsync(StreamConfig)`                 | stream admin    | → `INatsJSStream`; provision-or-update a stream          |
|  [08]   | `.CreateOrUpdateConsumerAsync(…)`                          | consumer admin  | → `INatsJSConsumer`; durable consumer (`ConsumerConfig`) |
|  [09]   | `.CreateOrderedConsumerAsync(stream, opts?)`               | consumer admin  | ephemeral ordered consumer (stream-order replay)         |
|  [10]   | `.ConsumeAsync<T>(…)`                                      | drain           | continuous cursor drain — the redrive/replay source      |
|  [11]   | `.FetchAsync<T>(NatsJSFetchOpts)` / `.FetchNoWaitAsync<T>` | pull batch      | bounded pull-batch fetch                                 |
|  [12]   | `.NextAsync<T>(…)`                                         | pull one        | → `ValueTask<INatsJSMsg<T>?>`; single-message pull       |
|  [13]   | `.AckAsync`                                                | ack             | commit the consumer cursor — at-least-once               |
|  [14]   | `.NakAsync`                                                | ack             | negative-ack, trigger redelivery                         |
|  [15]   | `.AckProgressAsync`                                        | ack             | extend the in-progress ack window                        |
|  [16]   | `.AckTerminateAsync`                                       | ack             | terminate, no redelivery (dead-letter)                   |
|  [17]   | `.GetDirectAsync<T>(StreamMsgGetRequest)`                  | stream read     | → `ValueTask<NatsMsg<T>>`; direct message get (replay)   |
|  [18]   | `.PurgeAsync(StreamPurgeRequest)`                          | stream purge    | retention purge                                          |

[JS_CONFIG]: the admin config surfaces.
- [01]-[STREAMCONFIG]: `Subjects`/`Retention`/`Storage`/`MaxAge`/`MaxMsgs`/`Discard`
- [02]-[CONSUMERCONFIG]: `DurableName`/`AckPolicy`/`DeliverPolicy`/`FilterSubject(s)`/`MaxDeliver`/`AckWait`

[JETSTREAM_KV]: revisioned CAS key-value store backend — `NATS.Client.KeyValueStore`; store ops hang off `INatsKVStore`, generic `<T>`, trailing `serializer?/opts?/ct`; reads yield `NatsKVEntry<T>` (`Value`/`Revision`/`Operation`/`Created`)
- rail: jetstream-store-backend

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `connection.CreateKeyValueStoreContext()`     | context        | → `INatsKVContext`; the KV context view                      |
|  [02]   | `.CreateStoreAsync(NatsKVConfig)`             | context        | → `INatsKVStore`; bucket provision (config in `[KV_DETAIL]`) |
|  [03]   | `.CreateStoreAsync(string bucket)`            | context        | → `INatsKVStore`; provision by bucket name                   |
|  [04]   | `.PutAsync<T>(key, value)`                    | write          | → `ValueTask<ulong>`; upsert, returns the new revision       |
|  [05]   | `.CreateAsync<T>(key, value, ttl?)`           | write-once     | → `ValueTask<ulong>`; create-if-absent, fenced-CAS create    |
|  [06]   | `.UpdateAsync<T>(key, value, ulong revision)` | CAS write      | → `ValueTask<ulong>`; optimistic concurrency                 |
|  [07]   | `.GetEntryAsync<T>(key, revision = 0)`        | read           | → `ValueTask<NatsKVEntry<T>>`; revisioned read               |
|  [08]   | `.WatchAsync<T>(key\|keys?, opts?)`           | watch          | → `IAsyncEnumerable<NatsKVEntry<T>>`; distributed changefeed |
|  [09]   | `.HistoryAsync<T>(key)`                       | history        | → `IAsyncEnumerable<NatsKVEntry<T>>`; AS-OF KV replay        |
|  [10]   | `.DeleteAsync(key)`                           | mutate         | tombstone delete                                             |
|  [11]   | `.PurgeAsync(key, ttl?)`                      | mutate         | purge history                                                |
|  [12]   | `.GetKeysAsync(filters?)`                     | scan           | key enumeration                                              |
|  [13]   | `Try*` mirrors                                | ROP            | → `ValueTask<NatsResult<…>>`; the non-throwing rail          |

[KV_DETAIL]: bucket config and the ROP mirror roster; the fenced-CAS faults are `[KV_AS_FENCED_CAS]`'s.
- [01]-[NATSKVCONFIG]: `History`/`MaxAge`/`Storage`/`NumberOfReplicas`/`MaxBytes`/`MaxValueSize`/`Compression`/`Republish`
- [02]-[TRY_MIRRORS]: `TryPutAsync`/`TryCreateAsync`/`TryUpdateAsync`/`TryGetEntryAsync`/`TryDeleteAsync`/`TryPurgeAsync`

[JETSTREAM_OBJECT]: chunked object store backend — `NATS.Client.ObjectStore`; ops hang off `INatsObjStore`, trailing `leaveOpen?/ct`; writes/reads return `ObjectMetadata` (fields in `[OBJ_META]`)
- rail: jetstream-store-backend

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------ | :------------- | :----------------------------------------------------------- |
|  [01]   | `connection.CreateObjectStoreContext()`     | context        | → `INatsObjContext`; the object context view                 |
|  [02]   | `.CreateObjectStoreAsync(NatsObjConfig)`    | context        | → `INatsObjStore`; provision by config                       |
|  [03]   | `.CreateObjectStoreAsync(string bucket)`    | context        | → `INatsObjStore`; provision by bucket name                  |
|  [04]   | `.PutAsync(string key, Stream, leaveOpen?)` | write          | → `ObjectMetadata`; chunked stream upload                    |
|  [05]   | `.PutAsync(key, byte[])`                    | write          | → `ObjectMetadata`; chunked byte upload                      |
|  [06]   | `.PutAsync(ObjectMetadata, Stream)`         | write          | → `ObjectMetadata`; metadata-driven upload                   |
|  [07]   | `.GetAsync(key, Stream, leaveOpen?)`        | read           | → `ObjectMetadata`; chunked download to a stream             |
|  [08]   | `.GetBytesAsync(key)`                       | read           | → `ValueTask<byte[]>`; chunked download to bytes             |
|  [09]   | `.GetInfoAsync(key, showDeleted?)`          | head           | → `ObjectMetadata`; object metadata                          |
|  [10]   | `.ListAsync(opts?)`                         | list           | → `IAsyncEnumerable<ObjectMetadata>`; object enumeration     |
|  [11]   | `.AddLinkAsync(link, target)`               | link           | → `ObjectMetadata`; object alias link                        |
|  [12]   | `.AddBucketLinkAsync(link, INatsObjStore)`  | link           | → `ObjectMetadata`; bucket alias link                        |
|  [13]   | `.SealAsync(ct)`                            | lifecycle      | make the bucket read-only                                    |
|  [14]   | `.DeleteAsync(key)`                         | lifecycle      | delete an object                                             |
|  [15]   | `.WatchAsync(opts?)`                        | lifecycle      | → `IAsyncEnumerable<ObjectMetadata>`; watch object mutations |

[OBJ_META]: `ObjectMetadata` (`NATS.Client.ObjectStore.Models`): `Name`/`Bucket`/`Nuid`/`Size`/`Chunks`/`Digest`/`Headers`/`Deleted`.

## [04]-[IMPLEMENTATION_LAW]

[CONNECTION_TOPOLOGY]:
- `NatsConnection`/`NatsClient` is the long-lived shared root (`IAsyncDisposable`); it is thread-safe and multiplexes all subjects over one TCP/WebSocket connection. The JetStream/KV/Object contexts are lightweight views over it — the JS view via the `new NatsJSContext(connection)` constructor, the KV/Object views via the `connection.CreateKeyValueStoreContext()`/`CreateObjectStoreContext()` extensions (which themselves take an `INatsConnection`/`INatsClient`/`INatsJSContext`) — never one connection per publish, mirroring the `StackExchange.Redis` multiplexer-singleton law in `api-redis`.
- `NatsConnectionPool` fans publishes across N connections for raw throughput; `AddNats(poolSize: N)` registers the pool. Default `poolSize: 1` is the single-multiplexer norm.
- the lifecycle events (`ConnectionDisconnected`/`ReconnectFailed`/`SlowConsumerDetected`/`LameDuckModeActivated`/`ServerError`) are the health/back-pressure stream the `Store ← Rasm.AppHost/Observability # [HEALTH_PROBE]` reachability probe and the `Version/egress` shed signal read — `SlowConsumerDetected` is the broker-side back-pressure trip, `LameDuckModeActivated` the graceful server-drain.

[JETSTREAM_DELIVERY_ACK]:
- Core `PublishAsync` is fire-and-forget (no broker confirmation) — never a durable egress leg. JetStream `INatsJSContext.PublishAsync` awaits the broker `PubAckResponse` (carrying `Stream`, `Seq`, `Duplicate`, and `Error` as `ApiError?`) — the durable Settle-ack the sink leg folds to `DeliveryAck.Persisted` (advancing the PostgreSQL outbox cursor past the contiguous `Persisted` prefix), a server `-ERR`/timeout to `Indeterminate` (the held cursor re-drives into the dedup window), and only a fatal protocol fault to `Refused` — the NATS analogue of the `api-kafka` `DeliveryAck.FromResult(PersistenceStatus, detail)` fold.
- `PubAckResponse.Duplicate` is the JetStream message-dedup-window flag (set when the broker recognizes a previously-seen `Nats-Msg-Id` header within the stream's `Duplicates` window) — the replay-absorption stance the `EGRESS_SINK` dedup column names (NATS has no Kafka-style producer transaction; the dedup window + idempotent publish IS the exactly-once-effective primitive, `NatsHeaders["Nats-Msg-Id"]` = the entity content key in lower-hex), folded as `Persisted(Duplicate: true)`.
- on the consume side, `INatsJSConsumer.ConsumeAsync`/`FetchAsync` is the cursor-replayable drain (`ConsumerConfig.DeliverPolicy`/`AckPolicy.Explicit`/`AckWait`/`MaxDeliver`), and `INatsJSMsg.AckAsync` commits the consumer cursor, `NakAsync` triggers redelivery, `AckTerminateAsync` dead-letters without redelivery — the at-least-once cursor the `Version/egress` redrive leg consumes (a `NakAsync`/`MaxDeliver`-exhausted message is the retriable dead-letter row).

[SERIALIZER_REGISTRY]:
- `NatsOpts.SerializerRegistry` (an `INatsSerializerRegistry`) selects the per-type codec; `NatsRawSerializer<T>` is the `byte[]`/`ReadOnlyMemory<byte>` passthrough that carries the already-encoded snapshot bytes, `NatsJsonContextSerializer<T>` the AOT-safe source-generated JSON codec. The egress payload is the settled `MessagePack`/`api-thinktecture-serialization`/`System.Formats.Cbor` snapshot of a `[ValueObject]`/`[SmartEnum]` owner handed to `NatsRawSerializer` — NATS frames the bytes, the codec owns the shape, no JSON hand-spelled at the subject boundary.
- `NatsResult`/`NatsResult<T>` (the `Try*` return) is the native ROP rail — pin the `TryPublishAsync`/`TryGetEntryAsync` form in domain logic and lift `NatsResult.Error` into the `StoreFault` rail at one site, never the throwing overload.

[KV_AS_FENCED_CAS]:
- the JetStream KV `CreateAsync` (create-if-absent, `NatsKVCreateException` on conflict) + `UpdateAsync(key, value, revision)` (optimistic CAS, `NatsKVWrongLastRevisionException` on a moved revision) is a distributed compare-and-swap cell — the distributed counterpart to the `Store/coordination#OUTBOX_TABLE` fenced-CAS `CoordCell` and the embedded `LightningDB` `NoOverwrite`, never a second coordination vocabulary. `WatchAsync` is the distributed changefeed and `HistoryAsync` the KV-tier AS-OF replay.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- the `EgressSink.Nats(Bind, Subject)` sink: NATS is the `ContentMode.Binary` row on the `Version/egress#EGRESS_SINK` axis — the bound `SinkBinding.Leg` dials JetStream `INatsJSContext.TryPublishAsync` with `NatsHeaders["Nats-Msg-Id"]` = the content key, folds the `PubAckResponse` to `DeliveryAck` at the leg boundary (`Error == null` → `Persisted`, `Duplicate` → `Persisted(Duplicate: true)`, a server `-ERR`/timeout → `Indeterminate` retriable, a fatal protocol fault → `Refused`), and only the contiguous `Persisted` prefix advances the PostgreSQL outbox cursor — the egress drains the one op-log CloudEvents envelope to a NATS subject with no second pump.
- snapshot codec at the subject: the CloudEvents-projected `CdcEnvelope.Payload` crosses as `NatsRawSerializer` bytes carrying the `traceparent`/`redactor`/`classification` in `NatsHeaders` (the CloudEvents extension attributes ride headers in binary mode), the same codec-owns-the-shape law `api-redis#STACKING` and `Version/egress` state.
- JetStream as the durable-stream backend: a `StreamConfig` (`Retention`/`MaxAge`/`Storage.File`) provisioned stream is the durable log the awaited Settle-ack publish writes through and a downstream consumer re-consumes via `ConsumeAsync` + `AckAsync` on its own independent group cursor — distinct from the Kafka backbone (`api-kafka`), one of the named messaging protocols (`NATS.Net`/`RabbitMQ.Client`/`DotPulsar`) each a sink row.
- JetStream KV + Object Store as store backends: the revisioned-CAS KV (`CreateAsync`/`UpdateAsync(revision)`) is a `Store/coordination` distributed fenced-CAS substrate and the chunked Object Store (`PutAsync(Stream)`/`SealAsync`) a distributed blob tier — two distinct store-backend capabilities beyond the embedded `LightningDB`/`rocksdb` and the cloud `ObjectStore`, selected as `Store/provisioning` rows, never a public type naming the package.
- DI + telemetry: `AddNats` wires the pooled connection; `NatsInstrumentationOptions`/`NatsInstrumentationContext` (the `ParentContext` `ActivityContext`) route NATS spans into the AppHost `telemetry` spine, and the connection-health events fold into the `HealthContributorRow` probe.

[RAIL_LAW]:
- Packages: `NATS.Net` (meta) → `NATS.Client.Core`/`.JetStream`/`.KeyValueStore`/`.ObjectStore`/`.Services`/`.Hosting`/`.Simplified`/`.Serializers.Json`
- Owns: the NATS protocol — Core pub/sub + request/reply, JetStream durable streams/consumers/publish-ack/consume, JetStream KV (distributed CAS) and Object Store (distributed chunked blob)
- Accept: one long-lived `NatsConnection`/`NatsClient` (singleton via `AddNats`), the lightweight `CreateXContext` views, `INatsJSContext.PublishAsync` → `PubAckResponse` as the durable ack, `NatsRawSerializer` carrying the settled snapshot bytes, the `NatsResult`/`Try*` ROP rail, `Nats-Msg-Id` header from the entity content key for dedup
- Reject: per-publish connection construction, a hand-rolled NATS protocol/framing loop the sub-clients own, Core `PublishAsync` on the at-least/exactly-once `Settle` rows (those demand the JetStream `PubAckResponse`), a JSON shape hand-spelled at the subject (the codec registry owns it), a second coordination vocabulary beside the KV CAS / `CoordCell` owners, a second retry owner where the AppHost `OutboundHop` owns hop retry and JetStream `AckWait`/`MaxDeliver` owns redelivery
