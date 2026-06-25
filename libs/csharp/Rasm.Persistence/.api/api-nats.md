# [RASM_PERSISTENCE_API_NATS]

`NATS.Net` is the full NATS protocol owner backing the `Sync/egress#EGRESS_SINK` `EgressSink.Nats` row (`SinkBinding.Subject`, `ContentMode.Binary`): Core pub/sub + request/reply for fire-and-forget and RPC, and JetStream durable streams/consumers/acks/replay for the `DeliveryGuarantee.Settle` ceremony — `JetStreamContext.PublishAsync` → `PubAckResponse` is the at-least-once durable ack the `EgressSink` `DeliveryAck.FromResult` analogue folds, `PubAckResponse.Duplicate` is the message-dedup-window flag the exactly-once posture rides, and `INatsJSConsumer.ConsumeAsync` + `INatsJSMsg.AckAsync`/`NakAsync`/`AckTerminateAsync` is the cursor-replay drain the redrive leg consumes. Beyond messaging it adds two distinct store-backend capabilities the embedded/relational tiers lack: JetStream **KeyValue** (revisioned CAS KV with `WatchAsync`/`HistoryAsync` — the distributed counterpart to the `LightningDB`/`rocksdb` embedded engines and a `Sync/coordination` fenced-CAS substrate) and JetStream **Object Store** (chunked object storage with `SealAsync`/`AddLinkAsync`). The serializer registry (`INatsSerializerRegistry`) is the codec seam where the `MessagePack`/`System.Text.Json` snapshot of a `[ValueObject]`/`[SmartEnum]` owner crosses the wire — NATS frames the bytes, the codec owns the shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NATS.Net` (meta-package)
- package: `NATS.Net`
- version: `2.8.2`
- assembly: `NATS.Net` (a ~5.6 KB facade exposing the `NATS.Net.NatsClient` simplified entry; the real surface lives in the sub-client assemblies it fans out to)
- license: Apache-2.0 (`<license type="expression">Apache-2.0</license>`)
- target framework: `net8.0` asset on the `net10.0` floor (package ships `net8.0`/`net6.0`/`netstandard2.1`/`netstandard2.0`; net8.0 wins NuGet precedence — there is no `net10.0` asset)
- asset: runtime library, pure-managed AnyCPU (TCP/WebSocket NATS protocol; no native)
- fan-out (the eight 2.8.2 sub-clients the meta-package transitively pins, each a real assembly — pin the meta-package, consume the sub-client namespaces): `NATS.Client.Core`, `NATS.Client.JetStream`, `NATS.Client.KeyValueStore`, `NATS.Client.ObjectStore`, `NATS.Client.Services`, `NATS.Client.Hosting`, `NATS.Client.Simplified`, `NATS.Client.Serializers.Json` (plus `NATS.Client.Abstractions` and `NATS.NKeys` 1.0.1 transitives)
- rail: messaging-protocol (NATS) + jetstream-store-backend

[SUB_CLIENT_MAP]: which namespace owns which concern
- rail: messaging-protocol

| [INDEX] | [ASSEMBLY]                       | [NAMESPACE]                  | [OWNS]                                                       |
| :-----: | :------------------------------- | :--------------------------- | :----------------------------------------------------------- |
|  [01]   | `NATS.Client.Core`               | `NATS.Client.Core`           | connection, pub/sub, request/reply, serializers, TLS, auth   |
|  [02]   | `NATS.Client.JetStream`          | `NATS.Client.JetStream`(`.Models`) | durable streams, consumers, publish-ack, consume/fetch  |
|  [03]   | `NATS.Client.KeyValueStore`      | `NATS.Client.KeyValueStore`  | JetStream KV — revisioned CAS, watch, history                |
|  [04]   | `NATS.Client.ObjectStore`        | `NATS.Client.ObjectStore`    | JetStream Object Store — chunked objects, links, seal        |
|  [05]   | `NATS.Client.Services`           | `NATS.Client.Services`       | the NATS micro-services protocol (discovery, stats, ping)    |
|  [06]   | `NATS.Client.Hosting`            | `Microsoft.Extensions.DependencyInjection` | `AddNats` DI registration + connection pool    |
|  [07]   | `NATS.Client.Simplified`         | `NATS.Net`                   | `NatsClient` one-line entry + `NatsClientDefaultSerializerRegistry` (the `CreateXContext` extensions live in their own KV/Object sub-clients, not here) |
|  [08]   | `NATS.Client.Serializers.Json`   | `NATS.Client.Serializers.Json` | the System.Text.Json serializer registry leg               |

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: the client, connection, options, and message — `NATS.Client.Core` / `NATS.Net`
- rail: messaging-protocol

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `NATS.Net.NatsClient : INatsClient`                  | simplified entry | one-line client (`NatsClient(url, name, credsFile)`); `.Connection` exposes the `INatsConnection` |
|  [02]   | `INatsConnection : INatsClient, IAsyncDisposable`    | connection      | the full connection — pub/sub/request + lifecycle events    |
|  [03]   | `NatsConnection : INatsConnection`                   | connection      | the concrete connection (`new NatsConnection(NatsOpts)`)    |
|  [04]   | `INatsConnectionPool` / `NatsConnectionPool`         | pool            | multi-connection pool for throughput fan-out                |
|  [05]   | `NatsOpts` (sealed record)                           | options         | url, name, serializer registry, TLS, auth, ping interval, pending-buffer policy |
|  [06]   | `NatsMsg<T>` (readonly record struct, `: INatsMsg<T>`) | message       | `Subject`/`Data`/`Headers`/`ReplyTo`/`Connection`; `ReplyAsync` for request/reply |
|  [07]   | `NatsHeaders : IDictionary<string, StringValues>`    | headers         | message headers — the `traceparent`/`Nats-Msg-Id` carrier   |
|  [08]   | `NatsResult` / `NatsResult<T>` (readonly struct)     | result rail     | the `Try*` non-throwing outcome — `Success`/`Error`, the native ROP rail |
|  [09]   | `NatsAuthOpts` / `NatsAuthCred` / `NatsTlsOpts` / `NatsWebSocketOpts` | auth/transport | creds/NKey/JWT, TLS mode, WebSocket transport |
|  [10]   | `NatsStats` (readonly record struct)                 | telemetry       | `SentBytes`/`ReceivedBytes`/`PendingMessages`/`SubscriptionCount` |
|  [11]   | `NatsInstrumentationOptions` / `NatsInstrumentationContext` | telemetry | the OpenTelemetry span context (subject/headers/connection/`ParentContext`) |
|  [12]   | `Nuid`                                               | id              | NATS unique id generator (subject/inbox tokens)             |

[SERIALIZER_TYPES]: the codec registry seam — `NATS.Client.Core` / `NATS.Client.Serializers.Json`
- rail: messaging-protocol

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]    | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `INatsSerializerRegistry`                                 | registry         | the per-`NatsOpts` codec selector — `GetSerializer<T>`/`GetDeserializer<T>` |
|  [02]   | `INatsSerialize<T>` / `INatsDeserialize<T>` / `INatsSerializer<T>` | codec    | the per-type serialize/deserialize contract                 |
|  [03]   | `NatsRawSerializer<T>`                                    | codec            | raw `byte[]`/`ReadOnlyMemory<byte>` passthrough — the snapshot-bytes carrier |
|  [04]   | `NatsUtf8PrimitivesSerializer<T>`                         | codec            | UTF-8 primitive serializer (string/number)                  |
|  [05]   | `NatsJsonContextSerializer<T>` / `NatsJsonContextSerializerRegistry` | codec  | source-generated `JsonSerializerContext` codec (AOT-safe)   |
|  [06]   | `NatsJsonSerializer<T>` / `NatsJsonSerializerRegistry` (`.Serializers.Json`) | codec | reflection `System.Text.Json` codec + options form |
|  [07]   | `NatsSerializerBuilder<T>`                                | builder          | chains a fallback serializer pipeline                       |

[FAULT_TYPES]: the `NatsException` family — `NATS.Client.Core`
- rail: messaging-protocol

`NatsException` base with closed sub-cases: `NatsNoRespondersException` (request to a subject with no responder — the RPC-target-absent), `NatsNoReplyException` (request timed out awaiting reply), `NatsServerException` (a `-ERR` from the server), `NatsPayloadTooLargeException` (over `max_payload`), `NatsConnectionFailedException`, `NatsProtocolViolationException`, `NatsTimeoutException`, `NatsDeserializeException`/`NatsHeaderParseException` (codec/header decode), `NatsSubException` (subscription fault carrying `NatsSubEndReason`). `NatsServerErrorKind` enumerates the server `-ERR` classes; `NatsMsgFlags` is the per-message bit (`Empty`/`NoResponders`).

## [03]-[ENTRYPOINTS]

[CONNECTION]: connect and the lifecycle events — `INatsClient` / `INatsConnection`
- rail: messaging-protocol

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `new NatsClient(url, name, credsFile?)` / `new NatsConnection(NatsOpts)` | construct | the connection (creds-file or `NatsOpts` form) |
|  [02]   | `ConnectAsync()` / `ReconnectAsync()` → `ValueTask`        | lifecycle      | establish / force-reconnect                           |
|  [03]   | `PingAsync(ct)` → `ValueTask<TimeSpan>`                    | health         | round-trip latency probe                              |
|  [04]   | `INatsConnection` events: `ConnectionOpened`/`ConnectionDisconnected`/`ReconnectFailed`/`MessageDropped`/`SlowConsumerDetected`/`LameDuckModeActivated`/`ServerError` | events | connection-health stream into the telemetry/health spine; `LameDuckModeActivated` is the graceful-server-drain signal, `SlowConsumerDetected` the back-pressure signal |
|  [05]   | `services.AddNats(poolSize, configureOpts, configureConnection, key)` | DI | `NATS.Client.Hosting` pooled registration |

[CORE_PUBSUB]: pub / sub / request-reply — the `EgressSink.Nats` Core leg
- rail: messaging-protocol

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `PublishAsync<T>(subject, data, headers?, replyTo?, serializer?, opts?, ct)` → `ValueTask` | publish | fire-and-forget publish — the at-most-once `Settle` leg |
|  [02]   | `PublishAsync<T>(in NatsMsg<T> msg, serializer?, opts?, ct)`           | publish        | publish a pre-built message (carries headers/reply)   |
|  [03]   | `SubscribeAsync<T>(subject, queueGroup?, serializer?, opts?, ct)` → `IAsyncEnumerable<NatsMsg<T>>` | subscribe | core subscription as an async stream; `queueGroup` = load-balanced consumer group |
|  [04]   | `SubscribeCoreAsync<T>(…)` → `INatsSub<T>`                             | subscribe      | the lower-level subscription handle (manual drain)    |
|  [05]   | `RequestAsync<TRequest, TReply>(subject, data, …)` → `ValueTask<NatsMsg<TReply>>` | request | request/reply RPC (`NatsNoRespondersException` if none) |
|  [06]   | `RequestManyAsync<TRequest, TReply>(…)` → `IAsyncEnumerable<NatsMsg<TReply>>` | scatter-gather | one request, many replies as a stream             |

[JETSTREAM]: durable streams, publish-ack, consume — the `DeliveryGuarantee.Settle` ceremony (`NATS.Client.JetStream`)
- rail: jetstream-store-backend

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `new NatsJSContext(INatsConnection connection)` / `new NatsJSContext(connection, NatsJSOpts opts)` | context factory | constructs the `INatsJSContext` view — JS-context construction is constructor-only (there is no `CreateJetStreamContext` extension; KV/Object DO have `CreateXContext` extensions) |
|  [02]   | `INatsJSContext.PublishAsync<T>(subject, data, serializer?, opts?, headers?, ct)` → `ValueTask<PubAckResponse>` | durable publish | at-least-once: awaits the broker `PubAckResponse` (Stream/Seq/Duplicate) — the `DeliveryAck.Persisted` analogue |
|  [03]   | `INatsJSContext.TryPublishAsync<T>(…)` → `ValueTask<NatsResult<PubAckResponse>>` | durable publish | the ROP-rail publish (no throw on `-ERR`)              |
|  [04]   | `INatsJSContext.PublishConcurrentAsync<T>(…)` → `ValueTask<NatsJSPublishConcurrentFuture>` | pipelined publish | decoupled publish + deferred-ack for high-throughput batches |
|  [05]   | `INatsJSContext.CreateStreamAsync(StreamConfig, ct)` / `CreateOrUpdateStreamAsync` → `INatsJSStream` | stream admin | provision a durable stream (Subjects/Retention/Storage/MaxAge/MaxMsgs/Discard) |
|  [06]   | `INatsJSContext.CreateOrUpdateConsumerAsync(stream, ConsumerConfig, ct)` → `INatsJSConsumer` | consumer admin | durable consumer (DurableName/AckPolicy/DeliverPolicy/FilterSubject(s)/MaxDeliver/AckWait) |
|  [07]   | `INatsJSContext.CreateOrderedConsumerAsync(stream, opts?, ct)`         | consumer admin | ephemeral ordered consumer (replay in stream order)          |
|  [08]   | `INatsJSConsumer.ConsumeAsync<T>(serializer?, opts?, ct)` → `IAsyncEnumerable<INatsJSMsg<T>>` | drain | continuous push-style cursor drain — the redrive/replay source |
|  [09]   | `INatsJSConsumer.FetchAsync<T>(NatsJSFetchOpts, …)` / `FetchNoWaitAsync<T>` → `IAsyncEnumerable<INatsJSMsg<T>>` | pull batch | bounded pull-batch fetch                            |
|  [10]   | `INatsJSConsumer.NextAsync<T>(serializer?, opts?, ct)` → `ValueTask<INatsJSMsg<T>?>` | pull one | single-message pull                                   |
|  [11]   | `INatsJSMsg<T>.AckAsync(opts?, ct)` / `NakAsync(opts?, ct)` / `AckProgressAsync(opts?, ct)` / `AckTerminateAsync(opts?, ct)` | ack | commit / negative-ack-redeliver / in-progress-extend / terminate-no-redeliver — the at-least-once cursor commit |
|  [12]   | `INatsJSStream.GetDirectAsync<T>(StreamMsgGetRequest, …)` → `ValueTask<NatsMsg<T>>` / `PurgeAsync(StreamPurgeRequest)` | stream read/purge | direct message get (replay) / retention purge |

[JETSTREAM_KV]: revisioned CAS key-value store backend — `NATS.Client.KeyValueStore`
- rail: jetstream-store-backend

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `connection.CreateKeyValueStoreContext()` → `INatsKVContext`; `.CreateStoreAsync(NatsKVConfig)` / `.CreateStoreAsync(string bucket)` → `INatsKVStore` | context | KV bucket provisioning (`NatsKVConfig`: `History`/`MaxAge`/`Storage`/`NumberOfReplicas`/`MaxBytes`/`MaxValueSize`/`Compression`/`Republish`) |
|  [02]   | `INatsKVStore.PutAsync<T>(key, value, serializer?, ct)` → `ValueTask<ulong>` | write | upsert; returns the new revision                              |
|  [03]   | `INatsKVStore.CreateAsync<T>(key, value, ttl?, …)` → `ValueTask<ulong>` | write-once | create-if-absent (`NatsKVCreateException` on conflict) — the fenced-CAS create |
|  [04]   | `INatsKVStore.UpdateAsync<T>(key, value, ulong revision, …)` → `ValueTask<ulong>` | CAS write | optimistic concurrency — `NatsKVWrongLastRevisionException` if revision moved |
|  [05]   | `INatsKVStore.GetEntryAsync<T>(key, revision = 0, …)` → `ValueTask<NatsKVEntry<T>>` | read | revisioned read (`Value`/`Revision`/`Operation`/`Created`)    |
|  [06]   | `INatsKVStore.WatchAsync<T>(key|keys?, opts?, ct)` → `IAsyncEnumerable<NatsKVEntry<T>>` | watch | live change feed over keys — the distributed changefeed       |
|  [07]   | `INatsKVStore.HistoryAsync<T>(key, …)` → `IAsyncEnumerable<NatsKVEntry<T>>` | history | the revision history — the AS-OF replay at the KV tier         |
|  [08]   | `INatsKVStore.DeleteAsync(key, opts?)` / `PurgeAsync(key, ttl?, opts?)` / `GetKeysAsync(filters?)` | mutate/scan | tombstone delete / purge history / key enumeration |
|  [09]   | `Try*` mirrors (`TryPutAsync`/`TryCreateAsync`/`TryUpdateAsync`/`TryGetEntryAsync`/`TryDeleteAsync`/`TryPurgeAsync`) → `ValueTask<NatsResult<…>>` | ROP | the non-throwing rail for every mutation |

[JETSTREAM_OBJECT]: chunked object store backend — `NATS.Client.ObjectStore`
- rail: jetstream-store-backend

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `connection.CreateObjectStoreContext()` → `INatsObjContext`; `.CreateObjectStoreAsync(NatsObjConfig)` / `.CreateObjectStoreAsync(string bucket)` → `INatsObjStore` | context | object bucket provisioning |
|  [02]   | `INatsObjStore.PutAsync(string key, Stream, leaveOpen?, ct)` / `PutAsync(key, byte[])` / `PutAsync(ObjectMetadata, Stream)` → `ValueTask<ObjectMetadata>` | write | chunked object upload returning `ObjectMetadata` (`NATS.Client.ObjectStore.Models`: `Name`/`Bucket`/`Nuid`/`Size`/`Chunks`/`Digest`/`Headers`/`Deleted`) |
|  [03]   | `INatsObjStore.GetAsync(key, Stream, leaveOpen?, ct)` → `ValueTask<ObjectMetadata>` / `GetBytesAsync(key)` → `ValueTask<byte[]>` | read | chunked download to a stream / to bytes               |
|  [04]   | `INatsObjStore.GetInfoAsync(key, showDeleted?)` → `ValueTask<ObjectMetadata>` / `ListAsync(opts?)` → `IAsyncEnumerable<ObjectMetadata>` | head/list | object metadata / enumeration |
|  [05]   | `INatsObjStore.AddLinkAsync(link, target)` / `AddBucketLinkAsync(link, INatsObjStore)` → `ValueTask<ObjectMetadata>` | link | object/bucket alias links                             |
|  [06]   | `INatsObjStore.SealAsync(ct)` / `DeleteAsync(key)` / `WatchAsync(opts?)` → `IAsyncEnumerable<ObjectMetadata>` | lifecycle | make read-only / delete / watch object mutations      |

## [04]-[IMPLEMENTATION_LAW]

[CONNECTION_TOPOLOGY]:
- `NatsConnection`/`NatsClient` is the long-lived shared root (`IAsyncDisposable`); it is thread-safe and multiplexes all subjects over one TCP/WebSocket connection. The JetStream/KV/Object contexts are lightweight views over it — the JS view via the `new NatsJSContext(connection)` constructor, the KV/Object views via the `connection.CreateKeyValueStoreContext()`/`CreateObjectStoreContext()` extensions (which themselves take an `INatsConnection`/`INatsClient`/`INatsJSContext`) — never one connection per publish, mirroring the `StackExchange.Redis` multiplexer-singleton law in `api-redis`.
- `NatsConnectionPool` fans publishes across N connections for raw throughput; `AddNats(poolSize: N)` registers the pool. Default `poolSize: 1` is the single-multiplexer norm.
- the lifecycle events (`ConnectionDisconnected`/`ReconnectFailed`/`SlowConsumerDetected`/`LameDuckModeActivated`/`ServerError`) are the health/back-pressure stream the `Store ← Rasm.AppHost/Observability # [HEALTH_PROBE]` reachability probe and the `Sync/egress` shed signal read — `SlowConsumerDetected` is the broker-side back-pressure trip, `LameDuckModeActivated` the graceful server-drain.

[JETSTREAM_DELIVERY_ACK]:
- Core `PublishAsync` is fire-and-forget (no broker confirmation) — the `DeliveryGuarantee.AtMostOnce`/`DeliveryAck.Lossy` leg. JetStream `INatsJSContext.PublishAsync` awaits the broker `PubAckResponse` (carrying `Stream`, `Seq`, `Duplicate`, and `Error` as `ApiError?`) — this is the durable at-least-once confirmation the `DeliveryGuarantee.AtLeastOnce` `Settle` arm advances the `SyncCursor` on, folding a `PubAckResponse` with a null `Error` to the advancing case and a non-null `Error` to the retriable/refused dead-letter exactly as `DeliveryAck.FromResult` folds the Kafka `DeliveryResult.Status`.
- `PubAckResponse.Duplicate` is the JetStream message-dedup-window flag (set when the broker recognizes a previously-seen `Nats-Msg-Id` header within the stream's `Duplicates` window) — the at-most-once-effective idempotency the `DeliveryGuarantee.ExactlyOnce` posture rides at the NATS tier (NATS has no Kafka-style producer transaction; the dedup window + idempotent publish IS the exactly-once-effective primitive, set the `Nats-Msg-Id` header on `NatsMsg.Headers` from the entity content key).
- on the consume side, `INatsJSConsumer.ConsumeAsync`/`FetchAsync` is the cursor-replayable drain (`ConsumerConfig.DeliverPolicy`/`AckPolicy.Explicit`/`AckWait`/`MaxDeliver`), and `INatsJSMsg.AckAsync` commits the consumer cursor, `NakAsync` triggers redelivery, `AckTerminateAsync` dead-letters without redelivery — the at-least-once cursor the `Sync/egress` redrive leg consumes (a `NakAsync`/`MaxDeliver`-exhausted message is the retriable dead-letter row).

[SERIALIZER_REGISTRY]:
- `NatsOpts.SerializerRegistry` (an `INatsSerializerRegistry`) selects the per-type codec; `NatsRawSerializer<T>` is the `byte[]`/`ReadOnlyMemory<byte>` passthrough that carries the already-encoded snapshot bytes, `NatsJsonContextSerializer<T>` the AOT-safe source-generated JSON codec. The egress payload is the settled `MessagePack`/`api-thinktecture-serialization`/`System.Formats.Cbor` snapshot of a `[ValueObject]`/`[SmartEnum]` owner handed to `NatsRawSerializer` — NATS frames the bytes, the codec owns the shape, no JSON hand-spelled at the subject boundary.
- `NatsResult`/`NatsResult<T>` (the `Try*` return) is the native ROP rail — pin the `TryPublishAsync`/`TryGetEntryAsync` form in domain logic and lift `NatsResult.Error` into the `StoreFault` rail at one site, never the throwing overload.

[KV_AS_FENCED_CAS]:
- the JetStream KV `CreateAsync` (create-if-absent, `NatsKVCreateException` on conflict) + `UpdateAsync(key, value, revision)` (optimistic CAS, `NatsKVWrongLastRevisionException` on a moved revision) is a distributed compare-and-swap cell — the distributed counterpart to the `Sync/coordination#OUTBOX_TABLE` fenced-CAS `CoordCell` and the embedded `LightningDB` `NoOverwrite`, never a second coordination vocabulary. `WatchAsync` is the distributed changefeed and `HistoryAsync` the KV-tier AS-OF replay.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- the `EgressSink.Nats` sink: NATS is the `SinkBinding.Subject` / `ContentMode.Binary` row on the `Sync/egress#EGRESS_SINK` axis — `DeliveryGuarantee.AtMostOnce` dials Core `PublishAsync`, `AtLeastOnce` dials JetStream `INatsJSContext.PublishAsync` and advances the `SyncCursor` on the `PubAckResponse`, and the broker-outcome fold is the NATS analogue of `DeliveryAck.FromResult` (`PubAckResponse.Error == null` → `Persisted`, a server `-ERR`/timeout → `Indeterminate` retriable, a fatal protocol fault → `Refused`), so the egress drains the one op-log CloudEvents envelope to a NATS subject with no second pump.
- snapshot codec at the subject: the CloudEvents-projected `CdcEnvelope.Payload` crosses as `NatsRawSerializer` bytes carrying the `traceparent`/`redactor`/`classification` in `NatsHeaders` (the CloudEvents extension attributes ride headers in binary mode), the same codec-owns-the-shape law `api-redis#STACKING` and `Sync/egress` state.
- JetStream as the durable-stream backend: a `StreamConfig` (`Retention`/`MaxAge`/`Storage.File`) provisioned stream is the durable log the `DeliveryGuarantee.Settle` ceremony writes through and the redrive/`Replay` leg re-consumes via `ConsumeAsync` + `AckAsync` — distinct from the Kafka backbone (`api-kafka`), one of the named messaging protocols (`NATS.Net`/`RabbitMQ.Client`/`DotPulsar`) each a sink row.
- JetStream KV + Object Store as store backends: the revisioned-CAS KV (`CreateAsync`/`UpdateAsync(revision)`) is a `Sync/coordination` distributed fenced-CAS substrate and the chunked Object Store (`PutAsync(Stream)`/`SealAsync`) a distributed blob tier — two distinct store-backend capabilities beyond the embedded `LightningDB`/`rocksdb` and the cloud `ObjectStore`, selected as `Store/profiles` rows, never a public type naming the package.
- DI + telemetry: `AddNats` wires the pooled connection; `NatsInstrumentationOptions`/`NatsInstrumentationContext` (the `ParentContext` `ActivityContext`) route NATS spans into the AppHost `telemetry` spine, and the connection-health events fold into the `HealthContributorRow` probe.

[RAIL_LAW]:
- Packages: `NATS.Net` (meta) → `NATS.Client.Core`/`.JetStream`/`.KeyValueStore`/`.ObjectStore`/`.Services`/`.Hosting`/`.Simplified`/`.Serializers.Json`
- Owns: the NATS protocol — Core pub/sub + request/reply, JetStream durable streams/consumers/publish-ack/consume, JetStream KV (distributed CAS) and Object Store (distributed chunked blob)
- Accept: one long-lived `NatsConnection`/`NatsClient` (singleton via `AddNats`), the lightweight `CreateXContext` views, `INatsJSContext.PublishAsync` → `PubAckResponse` as the durable ack, `NatsRawSerializer` carrying the settled snapshot bytes, the `NatsResult`/`Try*` ROP rail, `Nats-Msg-Id` header from the entity content key for dedup
- Reject: per-publish connection construction, a hand-rolled NATS protocol/framing loop the sub-clients own, Core `PublishAsync` on the at-least/exactly-once `Settle` rows (those demand the JetStream `PubAckResponse`), a JSON shape hand-spelled at the subject (the codec registry owns it), a second coordination vocabulary beside the KV CAS / `CoordCell` owners, a second retry owner where the AppHost `OutboundHop` owns hop retry and JetStream `AckWait`/`MaxDeliver` owns redelivery
