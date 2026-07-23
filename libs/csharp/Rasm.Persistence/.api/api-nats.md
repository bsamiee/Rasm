# [RASM_PERSISTENCE_API_NATS]

`NATS.Net` owns the NATS protocol for this branch: Core pub/sub and request-reply, JetStream durable streams under an awaited broker publish-ack, and the JetStream KeyValue and Object Store backends. An awaited `PubAckResponse` is the only durable delivery evidence the protocol produces, so a Core publish never backs a durable row. Payload shape belongs to the serializer registry — NATS frames opaque bytes and never inspects them. Core and JetStream feed the changefeed egress rail; KV and Object Store feed the distributed store-backend rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NATS.Net`
- package: `NATS.Net` (Apache-2.0)
- assembly: `NATS.Net` declares no types; every member ships in the sub-client assemblies the meta-package pins
- namespace: `NATS.Net`, `NATS.Client.Core`, `NATS.Client.JetStream`(`.Models`), `NATS.Client.KeyValueStore`, `NATS.Client.ObjectStore`, `NATS.Client.Services`, `NATS.Client.Hosting`, `NATS.Client.Serializers.Json`
- target: `net10.0`
- asset: pure-managed AnyCPU over the TCP and WebSocket NATS transports
- rail: messaging-protocol for the changefeed egress leg, jetstream-store-backend for the KV and Object tiers

[SUB_CLIENT_MAP]: assembly to owned concern; `NATS.Client.Abstractions` carries the serializer contracts and the socket-connection interfaces (`INatsSocketConnection`/`INatsTlsUpgradeableSocketConnection`, namespace `NATS.Client.Core`), and `NATS.NKeys` the credential primitives.

| [INDEX] | [ASSEMBLY]                     | [NAMESPACE]                        | [OWNS]                                                     |
| :-----: | :----------------------------- | :--------------------------------- | :--------------------------------------------------------- |
|  [01]   | `NATS.Client.Core`             | `NATS.Client.Core`                 | connection, pub/sub, request-reply, serializers, TLS, auth |
|  [02]   | `NATS.Client.JetStream`        | `NATS.Client.JetStream`(`.Models`) | durable streams, consumers, publish-ack, consume, fetch    |
|  [03]   | `NATS.Client.KeyValueStore`    | `NATS.Client.KeyValueStore`        | JetStream KV — revisioned CAS, watch, history              |
|  [04]   | `NATS.Client.ObjectStore`      | `NATS.Client.ObjectStore`          | JetStream Object Store — chunked blobs, links, seal        |
|  [05]   | `NATS.Client.Services`         | `NATS.Client.Services`             | the micro-services protocol — discovery, stats, ping       |
|  [06]   | `NATS.Client.Hosting`          | `NATS.Client.Hosting`              | `AddNats` pooled DI registration                           |
|  [07]   | `NATS.Client.Simplified`       | `NATS.Net`                         | `NatsClient` entry, `NatsClientDefaultSerializerRegistry`  |
|  [08]   | `NATS.Client.Serializers.Json` | `NATS.Client.Serializers.Json`     | the reflection `System.Text.Json` registry leg             |

## [02]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: client, connection, options, message, and telemetry carriers.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `NatsClient`                 | class         | the one-line entry; `.Connection` exposes the connection   |
|  [02]   | `INatsClient`                | interface     | connect, ping, publish, subscribe, request                 |
|  [03]   | `INatsConnection`            | interface     | the full connection — events, core subs, inbox mint        |
|  [04]   | `NatsConnection`             | class         | the concrete `IAsyncDisposable` connection root            |
|  [05]   | `INatsConnectionPool`        | interface     | round-robin connection fan-out                             |
|  [06]   | `NatsConnectionPool`         | class         | the pool `AddNats` registers                               |
|  [07]   | `NatsOpts`                   | class         | url, name, registry, TLS, auth, ping, buffers, reply mode  |
|  [08]   | `NatsPubOpts`                | class         | per-publish wait-until-sent and error handler              |
|  [09]   | `NatsSubOpts`                | class         | per-subscription max-msgs, idle and start-up timeouts      |
|  [10]   | `NatsSubChannelOpts`         | class         | bounded subscription channel capacity and full-mode        |
|  [11]   | `NatsMsg<T>`                 | struct        | `Subject`/`Data`/`Headers`/`ReplyTo`/`Flags`; `ReplyAsync` |
|  [12]   | `NatsMsgBuilder<T>`          | class         | mutable message build with its own serializer slot         |
|  [13]   | `NatsHeaders`                | class         | the `IDictionary<string, StringValues>` header carrier     |
|  [14]   | `NatsResult`                 | struct        | the non-throwing `Success`/`Error` rail                    |
|  [15]   | `NatsResult<T>`              | struct        | the same rail carrying `Value`                             |
|  [16]   | `NatsAuthOpts`               | class         | creds, NKey, JWT, token; `AuthCredCallback` rotation hook  |
|  [17]   | `NatsAuthCred`               | struct        | one resolved credential                                    |
|  [18]   | `NatsTlsOpts`                | class         | TLS mode, client certificate, CA bundle                    |
|  [19]   | `NatsWebSocketOpts`          | class         | WebSocket transport options                                |
|  [20]   | `NatsStats`                  | struct        | sent and received bytes and messages, pending, subs        |
|  [21]   | `NatsInstrumentationOptions` | class         | `Filter` and `Enrich` on the `Default` static              |
|  [22]   | `NatsInstrumentationContext` | struct        | subject, headers, connection, `ParentContext`              |
|  [23]   | `NatsServerErrorEventArgs`   | class         | `Error` text with the parsed `NatsServerErrorKind`         |
|  [24]   | `Nuid`                       | class         | the id generator behind inbox and object keys              |

- `NatsConnection.GetStats()`: internal, so `NatsStats` reads only through a wrapper the assembly itself composes; process telemetry comes off the `NATS.Net` `ActivitySource`.
- `NatsOpts.SocketConnectionFactory` (`INatsSocketConnectionFactory`) swaps the transport for a custom `INatsSocketConnection`/`INatsTlsUpgradeableSocketConnection`; the default socket stands unless one is supplied.

[SERIALIZER_TYPES]: the codec registry seam — `NATS.Client.Core` contracts with the `NATS.Client.Serializers.Json` reflection leg.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `INatsSerializerRegistry`             | interface     | the per-`NatsOpts` codec selector                        |
|  [02]   | `INatsSerialize<T>`                   | interface     | the per-type serialize contract                          |
|  [03]   | `INatsDeserialize<T>`                 | interface     | the per-type deserialize contract                        |
|  [04]   | `INatsSerializer<T>`                  | interface     | the combined codec contract                              |
|  [05]   | `NatsRawSerializer<T>`                | class         | `byte[]` and `ReadOnlyMemory<byte>` passthrough          |
|  [06]   | `NatsUtf8PrimitivesSerializer<T>`     | class         | UTF-8 string and number codec                            |
|  [07]   | `NatsJsonContextSerializer<T>`        | class         | source-generated `JsonSerializerContext` codec, AOT-safe |
|  [08]   | `NatsJsonContextSerializerRegistry`   | class         | the source-generated JSON registry                       |
|  [09]   | `NatsJsonSerializer<T>`               | class         | reflection `System.Text.Json` codec                      |
|  [10]   | `NatsJsonSerializerRegistry`          | class         | the reflection JSON registry                             |
|  [11]   | `NatsJsonOptionsSerializer<T>`        | class         | reflection codec over supplied `JsonSerializerOptions`   |
|  [12]   | `NatsSerializerBuilder<T>`            | class         | chains a fallback codec pipeline                         |
|  [13]   | `NatsDefaultSerializerRegistry`       | class         | the `NatsOpts` default chain                             |
|  [14]   | `NatsClientDefaultSerializerRegistry` | class         | the `NatsClient` default chain                           |

- `[CONTEXT_CODECS]` (opt-in): `INatsSerializeWithContext<T>` `INatsDeserializeWithContext<T>` `INatsSerializerWithContext<T>` receive a `NatsMsgContext` (`Subject`, `ReplyTo`, `Headers`) during (de)serialization; a plain `INatsSerialize<T>`/`INatsDeserialize<T>` runs unchanged.

[FAULT_TYPES]: `NatsException` roots the hierarchy and `NatsJSException` its JetStream branch; the rows below carry the discrimination a rail cannot read off the name, and the roster line closes the set.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :--------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `NatsNoRespondersException`        | class         | request to a subject with no responder               |
|  [02]   | `NatsNoReplyException`             | class         | request timed out awaiting a reply                   |
|  [03]   | `NatsServerException`              | class         | a server `-ERR`; `IsAuthError` splits auth faults    |
|  [04]   | `NatsSubException`                 | class         | subscription fault carrying payload and header bytes |
|  [05]   | `NatsJSApiException`               | class         | a JetStream API reply carrying `ApiError`            |
|  [06]   | `NatsJSDuplicateMessageException`  | class         | duplicate-sequence rejection carrying `Sequence`     |
|  [07]   | `NatsJSTimeoutException`           | class         | JetStream request timeout carrying `Type`            |
|  [08]   | `NatsKVCreateException`            | class         | create-if-absent lost to an existing key             |
|  [09]   | `NatsKVWrongLastRevisionException` | class         | CAS revision moved under the writer                  |
|  [10]   | `NatsKVKeyDeletedException`        | class         | tombstoned key, distinct from an absent one          |

- `[SELF_NAMING_FAULTS]`: `NatsPayloadTooLargeException` `NatsConnectionFailedException` `NatsProtocolViolationException` `NatsTimeoutException` `NatsDeserializeException` `NatsHeaderParseException` `NatsJSPublishNoResponseException` `NatsJSProtocolException` `NatsKVKeyNotFoundException` `NatsObjNotFoundException`

[VOCABULARIES]: `NatsServerErrorKind` classifies a server `-ERR` into auth-expiry, permission, and limit cases; `NatsSubEndReason` states why a subscription closed; `NatsKVOperation` tags a KV entry `Put`, `Del`, or `Purge`.
- `[NATS_ENUMS]`: `NatsServerErrorKind` `NatsSubEndReason` `NatsMsgFlags` `NatsConnectionState` `NatsAuthType` `TlsMode` `NatsRequestReplyMode` `NatsKVOperation` `NatsKVStorageType` `NatsObjStorageType`

## [03]-[ENTRYPOINTS]

Every op is async under a trailing `CancellationToken`, and `-> T` names the awaited payload rather than its `ValueTask` or `IAsyncEnumerable` wrapper. A generic `<T>` op carries optional trailing `serializer` and `opts` arguments.

[CONNECTION]: construction, lifecycle, socket hooks, and the `NatsHostingExtensions` pooled registration.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `NatsClient(string, string, string)`                                    | ctor     | url, client name, creds-file path     |
|  [02]   | `NatsClient(NatsOpts, BoundedChannelFullMode)`                          | ctor     | full options and pending-channel mode |
|  [03]   | `NatsConnection(NatsOpts)`                                              | ctor     | the root every context views          |
|  [04]   | `INatsClient.ConnectAsync()`                                            | instance | open the connection eagerly           |
|  [05]   | `INatsClient.ReconnectAsync()`                                          | instance | force a reconnect                     |
|  [06]   | `INatsClient.PingAsync() -> TimeSpan`                                   | instance | round-trip latency probe              |
|  [07]   | `INatsConnection.OnConnectingAsync`                                     | property | rewrite host and port per attempt     |
|  [08]   | `INatsConnection.OnSocketAvailableAsync`                                | property | wrap the socket before protocol start |
|  [09]   | `AddNats(int, Func<NatsOpts,NatsOpts>, Action<NatsConnection>, object)` | static   | pooled keyed DI registration          |

[CONNECTION_EVENTS]: `INatsConnection` raises each as an `AsyncEventHandler<T>` feeding the health fold — `SlowConsumerDetected` is the subscription back-pressure trip, `LameDuckModeActivated` the graceful server drain, and `ServerError` carries the classified `NatsServerErrorKind`.
- `[INatsConnection]`: `ConnectionOpened` `ConnectionDisconnected` `ReconnectFailed` `MessageDropped` `SlowConsumerDetected` `LameDuckModeActivated` `ServerError`

[CORE_PUBSUB]: Core publish, subscribe, and request-reply — the unconfirmed leg.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `INatsClient.PublishAsync<T>(string, T, NatsHeaders, string)`         | instance | fire-and-forget publish, no broker ack   |
|  [02]   | `INatsConnection.PublishAsync<T>(in NatsMsg<T>)`                      | instance | publish a pre-built message              |
|  [03]   | `INatsClient.SubscribeAsync<T>(string, string) -> NatsMsg<T>`         | instance | subject drain; queue group load-balances |
|  [04]   | `INatsConnection.SubscribeCoreAsync<T>(string, string)`               | instance | manual-drain `INatsSub<T>` handle        |
|  [05]   | `INatsClient.RequestAsync<TReq, TRep>(string, TReq) -> NatsMsg<TRep>` | instance | one-reply RPC                            |
|  [06]   | `INatsConnection.RequestManyAsync<TReq, TRep>(string, TReq)`          | instance | scatter-gather, replies streamed         |
|  [07]   | `INatsConnection.NewInbox() -> string`                                | instance | mint a reply-inbox subject               |
|  [08]   | `INatsSub<T>.DrainAsync()`                                            | instance | drain one sub, connection stays open     |

[JETSTREAM_PUBLISH]: `INatsJSContext` publish and admin — the durable leg.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `INatsClient.CreateJetStreamContext(NatsJSOpts) -> INatsJSContext`         | factory  | context view over the connection        |
|  [02]   | `NatsJSContext(INatsConnection, NatsJSOpts)`                               | ctor     | the concrete context                    |
|  [03]   | `PublishAsync<T>(string, T, NatsHeaders) -> PubAckResponse`                | instance | durable publish awaiting the broker ack |
|  [04]   | `TryPublishAsync<T>(string, T, NatsHeaders) -> NatsResult<PubAckResponse>` | instance | the non-throwing publish rail           |
|  [05]   | `PublishConcurrentAsync<T>(string, T) -> NatsJSPublishConcurrentFuture`    | instance | deferred ack for pipelined batches      |
|  [06]   | `CreateOrUpdateStreamAsync(StreamConfig) -> INatsJSStream`                 | instance | provision or reconfigure a stream       |
|  [07]   | `CreateOrUpdateConsumerAsync(string, ConsumerConfig) -> INatsJSConsumer`   | instance | durable consumer                        |
|  [08]   | `CreateOrderedConsumerAsync(string, NatsJSOrderedConsumerOpts)`            | instance | ephemeral stream-order replay           |
|  [09]   | `GetAccountInfoAsync() -> AccountInfoResponse`                             | instance | JetStream account limits and usage      |

[JETSTREAM_DRAIN]: the consumer cursor, the per-message ack verbs, and the direct stream read.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `INatsJSConsumer.ConsumeAsync<T>(NatsJSConsumeOpts)`                 | instance | continuous cursor drain           |
|  [02]   | `INatsJSConsumer.FetchAsync<T>(NatsJSFetchOpts)`                     | instance | bounded pull batch                |
|  [03]   | `INatsJSConsumer.FetchNoWaitAsync<T>(NatsJSFetchOpts)`               | instance | pull batch without the wait       |
|  [04]   | `INatsJSConsumer.NextAsync<T>(NatsJSNextOpts) -> INatsJSMsg<T>`      | instance | single-message pull               |
|  [05]   | `INatsJSMsg<T>.AckAsync(AckOpts)`                                    | instance | commit the consumer cursor        |
|  [06]   | `INatsJSMsg<T>.NakAsync(AckOpts)`                                    | instance | negative-ack, trigger redelivery  |
|  [07]   | `INatsJSMsg<T>.AckProgressAsync(AckOpts)`                            | instance | extend the in-progress ack window |
|  [08]   | `INatsJSMsg<T>.AckTerminateAsync(AckOpts)`                           | instance | terminate with no redelivery      |
|  [09]   | `INatsJSStream.GetDirectAsync<T>(StreamMsgGetRequest) -> NatsMsg<T>` | instance | direct stream read for replay     |
|  [10]   | `INatsJSStream.PurgeAsync(StreamPurgeRequest)`                       | instance | retention purge                   |

[JS_CONFIG]: `StreamConfig.DuplicateWindow` bounds message dedup and `ConsumerConfig.MaxDeliver` with `Backoff` bounds redelivery.
- `[StreamConfig]`: `Subjects` `Retention` `Storage` `MaxAge` `MaxMsgs` `MaxBytes` `Discard` `DuplicateWindow` `AllowDirect` `Republish` `Placement` `Mirror` `Sources`
- `[ConsumerConfig]`: `DurableName` `AckPolicy` `DeliverPolicy` `ReplayPolicy` `FilterSubject` `FilterSubjects` `AckWait` `MaxDeliver` `MaxAckPending` `Backoff` `InactiveThreshold`

[JETSTREAM_KV]: revisioned compare-and-swap key-value; reads yield `NatsKVEntry<T>` carrying `Value`, `Revision`, `Delta`, `Operation`, `Created`.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `INatsClient.CreateKeyValueStoreContext(NatsKVOpts)`            | factory  | the KV context view                   |
|  [02]   | `INatsKVContext.CreateStoreAsync(NatsKVConfig) -> INatsKVStore` | instance | bucket provision                      |
|  [03]   | `INatsKVContext.CreateOrUpdateStoreAsync(NatsKVConfig)`         | instance | provision or reconfigure a bucket     |
|  [04]   | `INatsKVStore.PutAsync<T>(string, T) -> ulong`                  | instance | upsert returning the new revision     |
|  [05]   | `INatsKVStore.CreateAsync<T>(string, T, TimeSpan) -> ulong`     | instance | create-if-absent with a per-key TTL   |
|  [06]   | `INatsKVStore.UpdateAsync<T>(string, T, ulong) -> ulong`        | instance | revision-guarded compare-and-swap     |
|  [07]   | `INatsKVStore.GetEntryAsync<T>(string, ulong)`                  | instance | read at a revision                    |
|  [08]   | `INatsKVStore.WatchAsync<T>(IEnumerable<string>)`               | instance | distributed changefeed                |
|  [09]   | `INatsKVStore.HistoryAsync<T>(string)`                          | instance | revision replay for one key           |
|  [10]   | `INatsKVStore.DeleteAsync(string, NatsKVDeleteOpts)`            | instance | tombstone delete                      |
|  [11]   | `INatsKVStore.PurgeAsync(string, TimeSpan)`                     | instance | purge history behind a TTL marker     |
|  [12]   | `INatsKVStore.PurgeDeletesAsync(NatsKVPurgeOpts)`               | instance | reclaim every tombstone in the bucket |
|  [13]   | `INatsKVStore.GetKeysAsync(IEnumerable<string>)`                | instance | filtered key enumeration              |
|  [14]   | `INatsKVStore.GetStatusAsync() -> NatsKVStatus`                 | instance | bucket status over its `StreamInfo`   |

- `[TRY_MIRRORS]`: `TryPutAsync` `TryCreateAsync` `TryUpdateAsync` `TryGetEntryAsync` `TryDeleteAsync` `TryPurgeAsync`
- `[NatsKVConfig]`: `History` `MaxAge` `Storage` `NumberOfReplicas` `MaxBytes` `MaxValueSize` `Compression` `Republish` `Placement` `Mirror` `Sources` `LimitMarkerTTL`

[JETSTREAM_OBJECT]: chunked blob storage; every write and read returns `ObjectMetadata`.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `INatsClient.CreateObjectStoreContext()`                         | factory  | the object context view             |
|  [02]   | `INatsObjContext.CreateObjectStoreAsync(NatsObjConfig)`          | instance | bucket provision                    |
|  [03]   | `INatsObjStore.PutAsync(string, Stream, bool) -> ObjectMetadata` | instance | chunked stream upload               |
|  [04]   | `INatsObjStore.PutAsync(string, byte[]) -> ObjectMetadata`       | instance | chunked byte upload                 |
|  [05]   | `INatsObjStore.PutAsync(ObjectMetadata, Stream, bool)`           | instance | metadata-driven upload              |
|  [06]   | `INatsObjStore.GetAsync(string, Stream, bool) -> ObjectMetadata` | instance | chunked download to a stream        |
|  [07]   | `INatsObjStore.GetBytesAsync(string) -> byte[]`                  | instance | chunked download to bytes           |
|  [08]   | `INatsObjStore.GetInfoAsync(string, bool) -> ObjectMetadata`     | instance | metadata head, deleted included     |
|  [09]   | `INatsObjStore.UpdateMetaAsync(string, ObjectMetadata)`          | instance | rename and re-describe in place     |
|  [10]   | `INatsObjStore.ListAsync(NatsObjListOpts)`                       | instance | object enumeration                  |
|  [11]   | `INatsObjStore.WatchAsync(NatsObjWatchOpts)`                     | instance | object mutation changefeed          |
|  [12]   | `INatsObjStore.AddLinkAsync(string, ObjectMetadata)`             | instance | object alias link                   |
|  [13]   | `INatsObjStore.AddBucketLinkAsync(string, INatsObjStore)`        | instance | bucket alias link                   |
|  [14]   | `INatsObjStore.SealAsync()`                                      | instance | make the bucket read-only           |
|  [15]   | `INatsObjStore.DeleteAsync(string)`                              | instance | delete an object                    |
|  [16]   | `INatsObjStore.GetStatusAsync() -> NatsObjStatus`                | instance | bucket status over its `StreamInfo` |

- `[ObjectMetadata]`: `Name` `Description` `Bucket` `Nuid` `Size` `Chunks` `Digest` `MTime` `Headers` `Metadata` `Deleted` `Options`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NatsConnection` is the long-lived thread-safe root multiplexing every subject over one socket and disposed as `IAsyncDisposable`; `CreateJetStreamContext`, `CreateKeyValueStoreContext`, and `CreateObjectStoreContext` mint lightweight views over it, and `NatsConnectionPool` fans publishes across N connections only where raw throughput demands it.
- Every entry point (`NatsConnection`, `NatsClient`, the `AddNats` DI builder) shares one `NatsOpts` subscription default — a 16384-slot pending channel with `BoundedChannelFullMode.DropNewest` surfacing overflow through `MessageDropped` rather than stalling the socket read loop into a slow-consumer disconnect.
- `NatsOpts.RequestReplyMode` defaults to `NatsRequestReplyMode.Direct`: a `RequestAsync` reply correlates on the connection's existing inbox subscription with the per-reply muxer skipped, `NatsNoRespondersException` still thrown at the no-responder reply; `SharedInbox` restores the per-request subscription-and-channel mechanism.
- Core `PublishAsync` returns once the frame is written, so the awaited `INatsJSContext.PublishAsync` and its `PubAckResponse` are the protocol's only durable delivery evidence; `TryPublishAsync` carries the same ack on the `NatsResult<T>` rail instead of throwing.
- `PubAckResponse.Duplicate` reports the broker recognizing a prior `Nats-Msg-Id` inside the stream's `StreamConfig.DuplicateWindow`, which makes idempotent publish the exactly-once-effective primitive; `NatsJSDuplicateMessageException` is the distinct duplicate-sequence rejection, never the benign window hit.
- `ConsumerConfig` owns redelivery through `AckPolicy`, `AckWait`, `MaxDeliver`, and `Backoff`, and `INatsJSMsg<T>` closes each message with `AckAsync`, `NakAsync`, `AckProgressAsync`, or `AckTerminateAsync`; each consumer group tracks its own cursor independent of any store cursor a reader keeps; `NatsJSConsumeOpts.DrainOnCancel` opts a consume loop into delivering buffered messages after cancellation so handlers still ack, the default stopping immediately, and `INatsSub<T>.DrainAsync` is the Core-subscription counterpart fencing in-flight deliveries without tearing the connection.
- `NatsOpts.SerializerRegistry` fixes the per-type codec at construction, so `NatsRawSerializer<T>` carries already-encoded bytes and `NatsJsonContextSerializer<T>` the AOT-safe source-generated JSON form.
- `INatsKVStore.CreateAsync` and `UpdateAsync(key, value, revision)` are the create-if-absent and revision-CAS pair, refusing through `NatsKVCreateException` and `NatsKVWrongLastRevisionException`; `WatchAsync` is the changefeed and `HistoryAsync` the revision replay.
- `NatsInstrumentationOptions.Default` is process-static, so `Filter` and `Enrich` bind once for the whole process; spans emit on the `NATS.Net` `ActivitySource` under `messaging.system = nats`.

[STACKING]:
- `CloudNative.CloudEvents`(`.api/api-cloudevents.md`): no NATS protocol binding ships, so the egress leg maps the envelope onto `NatsHeaders` itself — `Nats-Msg-Id` from the content key beside the `traceparent` rows — and hands `NatsRawSerializer<T>` the formatter's bytes.
- `Confluent.Kafka`(`.api/api-kafka.md`), `RabbitMQ.Client`(`.api/api-rabbitmq.md`), `DotPulsar`(`.api/api-dotpulsar.md`): peer `EgressSink` rows over one op-log envelope, each folding its provider outcome to `DeliveryAck` at its own leg boundary.
- `StackExchange.Redis`(`.api/api-redis.md`): the same multiplexer-singleton topology and the same codec-owns-the-shape boundary; a Redis stream and a JetStream stream are peer sink rows whose group cursors never merge.
- `LightningDB`(`.api/api-lightningdb.md`), `ObjectStore`(`.api/api-objectstore.md`): embedded and cloud counterparts to the JetStream KV and Object tiers, all selected as `Store/provisioning` backend rows.
- `AspNetCore.HealthChecks.Nats`(`libs/csharp/Rasm.AppHost/.api/api-healthchecks-nats.md`): probes the pooled `INatsConnection` as the `DriverProbe.Nats` contributor row, and the connection events feed that same health fold.
- Within the package one connection carries every leg: `TryPublishAsync` publishes on the ROP rail, `PublishConcurrentAsync` defers its ack through `NatsJSPublishConcurrentFuture.GetResponseAsync` for pipelined batches, `NatsMsg<T>.StartActivity` continues the consume-side trace, `NatsAuthOpts.AuthCredCallback` rotates credentials per connect, and `NatsKVEntry<T>.Delta` bounds a watch catch-up.

[LOCAL_ADMISSION]:
- Changefeed egress dials `INatsJSContext.TryPublishAsync` on its `Nats` sink row with `NatsHeaders["Nats-Msg-Id"]` set to the entity content key in lower-hex, folding a null `Error` to `Persisted`, `Duplicate` to `Persisted(Duplicate: true)`, a server `-ERR` or timeout to `Indeterminate`, and a fatal protocol fault to `Refused`; only the contiguous `Persisted` prefix advances the outbox cursor.
- Each publish builds a fresh `NatsHeaders`: publish leaves an instance mutable, so one instance never serves concurrent publishes.
- A durable stream provisions through `CreateOrUpdateStreamAsync(StreamConfig)` on file storage with a `DuplicateWindow` wide enough to absorb a held-cursor re-drive, and a downstream reader consumes it on its own `ConsumerConfig` cursor.
- PostgreSQL owns coordination: the `Store/coordination` fenced compare-and-swap under `LeaseToken` is the one lease and CAS vocabulary, and the JetStream KV enters as a distributed store-backend row on `Store/provisioning`.
- An Object Store bucket carries chunked blobs through `PutAsync(string, Stream, bool)` and closes with `SealAsync`, a distributed tier beside the embedded and cloud blob rows.
- Domain code binds the `Try*` form and lifts `NatsResult.Error` onto the store fault rail at one site.

[RAIL_LAW]:
- Package: `NATS.Net`
- Owns: the NATS protocol — Core pub/sub and request-reply, JetStream durable streams with publish-ack and drain, JetStream KV revisioned CAS, and JetStream Object Store chunked blobs.
- Accept: one long-lived connection with context views over it, an awaited `PubAckResponse` as the durable ack, `Nats-Msg-Id` from the content key, `NatsRawSerializer<T>` carrying settled snapshot bytes, and the `NatsResult` rail in domain logic.
- Reject: per-publish connection construction, hand-rolled NATS framing, a Core publish backing a durable row, a JSON shape spelled at the subject boundary, a lease or CAS vocabulary beside the PostgreSQL fenced store, and a retry owner beside `AckWait` and `MaxDeliver`.
