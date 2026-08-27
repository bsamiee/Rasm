# [RASM_API_NATS]

`NATS.Net` owns the NATS protocol for this branch: Core pub/sub and request-reply, JetStream durable streams under an awaited broker publish-ack, and the JetStream KeyValue and Object Store backends. An awaited `PubAckResponse` is the only durable delivery evidence the protocol produces, so a Core publish never backs a durable row. Payload shape belongs to the serializer registry — NATS frames opaque bytes and never inspects them. Two folders bind disjoint legs of one meta-package: `Rasm.Persistence` owns the JetStream durable-stream, publish-ack, KV, and Object-Store legs feeding the changefeed egress and distributed store-backend rows, and `Rasm.Compute` binds the `NATS.Client.Core` subscription ingest boundary — one `NatsMsg<byte[]>` per sensor sample onto `WorkLane.CaptureIngest` — beside its `RequestAsync`/`ReplyAsync` remote-compute RPC leg.

## [01]-[PUBLIC_TYPES]

[CONNECTION_TYPES]: client, connection, options, message, and telemetry carriers.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :--------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `NatsClient`                 | class         | the one-line entry; `.Connection` exposes the connection    |
|  [02]   | `INatsClient`                | interface     | connect, ping, publish, subscribe, request                  |
|  [03]   | `INatsConnection`            | interface     | the full connection — events, core subs, inbox mint         |
|  [04]   | `NatsConnection`             | class         | the concrete `IAsyncDisposable` connection root             |
|  [05]   | `INatsConnectionPool`        | interface     | round-robin connection fan-out                              |
|  [06]   | `NatsConnectionPool`         | class         | the pool `AddNats` registers                                |
|  [07]   | `NatsOpts`                   | record        | the whole connection policy — `[CONNECTION_POLICY]`         |
|  [08]   | `NatsPubOpts`                | record        | per-publish wait-until-sent and error handler               |
|  [09]   | `NatsSubOpts`                | record        | the per-subscription override — `[SUBSCRIPTION_POLICY]`     |
|  [10]   | `NatsSubChannelOpts`         | record        | bounded subscription channel capacity and full-mode         |
|  [11]   | `NatsMsg<T>`                 | struct        | `Subject`/`Data`/`Headers`/`ReplyTo`/`Flags`; `ReplyAsync`  |
|  [12]   | `NatsMsgBuilder<T>`          | class         | mutable message build with its own serializer slot          |
|  [13]   | `NatsHeaders`                | class         | the `IDictionary<string, StringValues>` header carrier      |
|  [14]   | `INatsSub<T> : INatsSub`     | interface     | manual-drain handle; `Msgs` channel, `DrainAsync` fences    |
|  [15]   | `NatsMsgFlags` (byte enum)   | enum          | `None`/`Empty`/`NoResponders` — `IsEmpty`/`HasNoResponders` |
|  [16]   | `NatsResult`                 | struct        | the non-throwing `Success`/`Error` result                   |
|  [17]   | `NatsResult<T>`              | struct        | the same result carrying `Value`                            |
|  [18]   | `NatsAuthOpts`               | class         | creds, NKey, JWT, token; `AuthCredCallback` rotation hook   |
|  [19]   | `NatsAuthCred`               | struct        | one resolved credential                                     |
|  [20]   | `NatsTlsOpts`                | class         | TLS mode, client certificate, CA bundle                     |
|  [21]   | `NatsWebSocketOpts`          | class         | WebSocket transport options                                 |
|  [22]   | `NatsStats`                  | struct        | sent and received bytes and messages, pending, subs         |
|  [23]   | `NatsInstrumentationOptions` | class         | `Filter` and `Enrich` on the `Default` static               |
|  [24]   | `NatsInstrumentationContext` | struct        | subject, headers, connection, `ParentContext`               |
|  [25]   | `NatsServerErrorEventArgs`   | class         | `Error` text with the parsed `NatsServerErrorKind`          |
|  [26]   | `Nuid`                       | class         | the id generator behind inbox and object keys               |

- `NatsConnection.GetStats()`: internal, so `NatsStats` reads only through a wrapper the assembly itself composes; process telemetry comes off the `NATS.Net` `ActivitySource`.
- `NatsOpts.SocketConnectionFactory` (`INatsSocketConnectionFactory`) swaps the transport for a custom `INatsSocketConnection`/`INatsTlsUpgradeableSocketConnection`; the default socket stands unless one is supplied.

[CONNECTION_POLICY]: `NatsOpts` is a record whose every column carries a shipped default, so a composition that names none still runs a fully-decided policy — reconnect curve, ping liveness, subscription bound, and drain posture all resolve from this table whether or not a page declares them.

| [INDEX] | [MEMBER]                        | [TYPE]                   | [DEFAULT]               | [DECIDES]                                    |
| :-----: | :------------------------------ | :----------------------- | :---------------------- | :------------------------------------------- |
|  [01]   | `Url`                           | `string`                 | `nats://localhost:4222` | seed server list, comma-separated            |
|  [02]   | `Name`                          | `string`                 | `NATS .NET Client`      | the client name every server view reports    |
|  [03]   | `ConnectTimeout`                | `TimeSpan`               | `2s`                    | one dial attempt's ceiling                   |
|  [04]   | `RetryOnInitialConnect`         | `bool`                   | `false`                 | whether a failed FIRST dial enters reconnect |
|  [05]   | `ReconnectWaitMin`              | `TimeSpan`               | `2s`                    | the reconnect curve's floor                  |
|  [06]   | `ReconnectWaitMax`              | `TimeSpan`               | `5s`                    | the reconnect curve's ceiling                |
|  [07]   | `ReconnectJitter`               | `TimeSpan`               | `100ms`                 | the per-attempt spread                       |
|  [08]   | `MaxReconnectRetry`             | `int`                    | `-1`                    | attempt bound; negative is unbounded         |
|  [09]   | `NoRandomize`                   | `bool`                   | `false`                 | whether the server list keeps declared order |
|  [10]   | `IgnoreAuthErrorAbort`          | `bool`                   | `false`                 | whether a repeated auth error stops retrying |
|  [11]   | `PingInterval`                  | `TimeSpan`               | `2m`                    | liveness cadence                             |
|  [12]   | `MaxPingOut`                    | `int`                    | `2`                     | unanswered pings before the socket drops     |
|  [13]   | `RequestTimeout`                | `TimeSpan`               | `5s`                    | the request-reply await ceiling              |
|  [14]   | `CommandTimeout`                | `TimeSpan`               | `5s`                    | the write-side enqueue ceiling               |
|  [15]   | `SubPendingChannelCapacity`     | `int`                    | `16384`                 | every subscription's pending-channel bound   |
|  [16]   | `SubPendingChannelFullMode`     | `BoundedChannelFullMode` | `DropNewest`            | what an overrun subscription does            |
|  [17]   | `SubscriptionCleanUpInterval`   | `TimeSpan`               | `5m`                    | the reaper cadence for ended subscriptions   |
|  [18]   | `DrainSubscriptionsOnDispose`   | `bool`                   | `false`                 | whether dispose drains or abandons buffers   |
|  [19]   | `ConsumerDrainOnDisposeTimeout` | `TimeSpan?`              | absent                  | the dispose-drain budget, drain-gated        |
|  [20]   | `DrainPingTimeout`              | `TimeSpan`               | `5s`                    | the per-subscription drain PING/PONG fence   |
|  [21]   | `SuppressSlowConsumerWarnings`  | `bool`                   | `false`                 | whether a slow consumer logs once            |
|  [22]   | `WriterBufferSize`              | `int`                    | `65536`                 | socket write buffer                          |
|  [23]   | `ReaderBufferSize`              | `int`                    | `65536`                 | socket read buffer                           |
|  [24]   | `MaxPayloadHardCap`             | `int`                    | `67108864`              | the local ceiling a server INFO cannot raise |
|  [25]   | `WaitUntilSent`                 | `bool`                   | `false`                 | whether a publish awaits the socket write    |
|  [26]   | `PublishTimeoutOnDisconnected`  | `bool`                   | `false`                 | whether a disconnected publish times out     |
|  [27]   | `ObjectPoolSize`                | `int`                    | `256`                   | command-object pool depth                    |
|  [28]   | `InboxPrefix`                   | `string`                 | `_INBOX`                | the reply-inbox subject root                 |
|  [29]   | `HeaderEncoding`                | `Encoding`               | `ASCII`                 | header byte encoding                         |
|  [30]   | `SubjectEncoding`               | `Encoding`               | `UTF8`                  | subject byte encoding                        |
|  [31]   | `SkipSubjectValidation`         | `bool`                   | `false`                 | whether subjects are checked before send     |
|  [32]   | `Echo`                          | `bool`                   | `true`                  | whether a connection receives its own sends  |
|  [33]   | `Headers`                       | `bool`                   | `true`                  | whether the CONNECT advertises header use    |
|  [34]   | `Verbose`                       | `bool`                   | `false`                 | whether the server ACKs every protocol line  |
|  [35]   | `UseThreadPoolCallback`         | `bool`                   | `false`                 | whether callbacks leave the read loop        |

- `NatsOpts` owns the reconnect curve alone, so a page composing it declares no schedule of its own; `ReconnectAsync()` forces one attempt and `ReconnectFailed` reports each refusal.
- `MaxReconnectRetry` at its `-1` default retries forever, so a composition wanting a bounded reconnect states the bound rather than wrapping the connection in a retry it does not own.

[SUBSCRIPTION_POLICY]: `NatsSubOpts` is the per-subscription override over `[CONNECTION_POLICY]`; every slot is nullable and an unset slot inherits the connection value rather than a second default.

| [INDEX] | [MEMBER]                          | [TYPE]                    | [DECIDES]                                             |
| :-----: | :-------------------------------- | :------------------------ | :---------------------------------------------------- |
|  [01]   | `NatsSubOpts.ChannelOpts`         | `NatsSubChannelOpts?`     | this subscription's own pending-channel bound         |
|  [02]   | `NatsSubOpts.MaxMsgs`             | `int?`                    | auto-unsubscribe after N messages                     |
|  [03]   | `NatsSubOpts.Timeout`             | `TimeSpan?`               | auto-unsubscribe after a total span                   |
|  [04]   | `NatsSubOpts.StartUpTimeout`      | `TimeSpan?`               | auto-unsubscribe when no first message arrives        |
|  [05]   | `NatsSubOpts.IdleTimeout`         | `TimeSpan?`               | auto-unsubscribe on a gap between messages            |
|  [06]   | `NatsSubOpts.StopOnEmptyMsg`      | `bool?`                   | whether an empty status frame ends the subscription   |
|  [07]   | `NatsSubOpts.ThrowIfNoResponders` | `bool?`                   | whether a no-responders frame raises                  |
|  [08]   | `NatsSubOpts.Events`              | `NatsSubEvents?`          | `OnSubscribed`, the established-subscription callback |
|  [09]   | `NatsSubChannelOpts.Capacity`     | `int?`                    | the bound, `1000` where the record is supplied unset  |
|  [10]   | `NatsSubChannelOpts.FullMode`     | `BoundedChannelFullMode?` | the overrun verb, `Wait` where supplied unset         |

- `MaxMsgs` and the three timeout slots END the subscription rather than refusing one message, so a composition reading them as per-message deadlines loses the stream at the first gap.
- Supplying `NatsSubChannelOpts` at all switches the two unset slots onto ITS defaults (`1000`/`Wait`), not the connection's (`16384`/`DropNewest`), so a page overriding one slot states both.
- `NatsSubEvents.OnSubscribed` fires once when the SUB reaches the send queue and never again across reconnects, so it is an establishment fence, never a reconnect signal.

[SERIALIZER_TYPES]: the codec registry interface — `NATS.Client.Core` contracts with the `NATS.Client.Serializers.Json` reflection leg.

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

[FAULT_TYPES]: `NatsException` roots the hierarchy and `NatsJSException` its JetStream branch; the rows below carry the discrimination a handler cannot read off the name, and the roster line closes the set.

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

## [02]-[ENTRYPOINTS]

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

[CONNECTION_EVENTS]: `INatsConnection` raises each as an `AsyncEventHandler<T>` feeding the health fold — `SlowConsumerDetected` is the subscription back-pressure trip, `MessageDropped` the loss that trip produces, `LameDuckModeActivated` the graceful server drain, and `ServerError` carries the classified `NatsServerErrorKind`. Each is connection-wide, so a per-subscription reader discriminates on the args' own `Subscription`/`Subject`.
- `[INatsConnection]`: `ConnectionOpened` `ConnectionDisconnected` `ReconnectFailed` `MessageDropped` `SlowConsumerDetected` `LameDuckModeActivated` `ServerError`
- `[NatsMessageDroppedEventArgs]`: `Subscription` (`NatsSubBase`) `Pending` `Subject` `ReplyTo` `Headers` `Data`; `[NatsSlowConsumerEventArgs]`: `Subscription`

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
|  [09]   | `msg.Headers?.TryGetValue(name, out StringValues v)`                  | instance | non-throwing W3C trace-pair extract      |
|  [10]   | `msg.ReplyAsync<TReply>(data, headers?, …)`                           | instance | reply to a request-subject message       |

[JETSTREAM_PUBLISH]: `INatsJSContext` publish and admin — the durable leg.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `INatsClient.CreateJetStreamContext(NatsJSOpts) -> INatsJSContext`         | factory  | context view over the connection        |
|  [02]   | `NatsJSContext(INatsConnection, NatsJSOpts)`                               | ctor     | the concrete context                    |
|  [03]   | `PublishAsync<T>(string, T, NatsHeaders, NatsJSPubOpts) -> PubAckResponse` | instance | durable publish awaiting the broker ack |
|  [04]   | `TryPublishAsync<T>(string, T, NatsHeaders, NatsJSPubOpts)`                | instance | the non-throwing publish result         |
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

[JS_PUBLISH_OPTS]: `NatsJSPubOpts : NatsPubOpts` carries the publish-side identity and optimistic-concurrency guards; `NatsJSPubOpts.Default` is the instance a publish taking no options runs under.

| [INDEX] | [MEMBER]                             | [TYPE]     | [DEFAULT] | [DECIDES]                                       |
| :-----: | :----------------------------------- | :--------- | :-------- | :---------------------------------------------- |
|  [01]   | `MsgId`                              | `string`   | absent    | the dedup identity, written as `Nats-Msg-Id`    |
|  [02]   | `ExpectedStream`                     | `string`   | absent    | refuse where the subject binds another stream   |
|  [03]   | `ExpectedLastMsgId`                  | `string`   | absent    | refuse unless the named id is the stream's last |
|  [04]   | `ExpectedLastSequence`               | `ulong?`   | absent    | stream-wide optimistic-concurrency fence        |
|  [05]   | `ExpectedLastSubjectSequence`        | `ulong?`   | absent    | per-subject optimistic-concurrency fence        |
|  [06]   | `ExpectedLastSubjectSequenceSubject` | `string`   | absent    | the subject that per-subject fence reads        |
|  [07]   | `RetryAttempts`                      | `int`      | `1`       | extra publish attempts on a no-responders reply |
|  [08]   | `RetryWaitBetweenAttempts`           | `TimeSpan` | `250ms`   | the spacing between those attempts              |

- `MsgId` is the SDK's own dedup carriage and the ONE spelling a durable publish takes; hand-writing `Nats-Msg-Id` onto `NatsHeaders` re-implements a member the option owns and forks the key the peer branches already set through their own option (`@nats-io/jetstream` `msgID`).
- `RetryAttempts` is a retry owner INSIDE the publish, so a hop already holding one declares this slot rather than inheriting it — an undeclared default silently doubles the attempt count the hop's own owner accounts.
- `ExpectedLastSequence` and `ExpectedLastSubjectSequence` carry the broker-side refusal a resumed relay reads instead of re-publishing blind; the per-subject fence needs `ExpectedLastSubjectSequenceSubject` to name the subject it measures.

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NatsConnection` is the long-lived thread-safe root multiplexing every subject over one socket and disposed as `IAsyncDisposable`; `CreateJetStreamContext`, `CreateKeyValueStoreContext`, and `CreateObjectStoreContext` mint lightweight views over it, and `NatsConnectionPool` fans publishes across N connections only where raw throughput demands it. Compute owns its capture subscriber independent of the Persistence egress connection — constructed once and shared across the capture lane, never per-subscription and never a process-global static.
- Every entry point (`NatsConnection`, `NatsClient`, the `AddNats` DI builder) shares one `NatsOpts` subscription default — a 16384-slot pending channel with `BoundedChannelFullMode.DropNewest` surfacing overflow through `MessageDropped` rather than stalling the socket read loop into a slow-consumer disconnect.
- Every Core subscription therefore rides a BOUNDED lossy channel whether or not a page declares one: `NatsSub<T>` constructs `Channel.CreateBounded` over `[CONNECTION_POLICY]` rows `[15]`/`[16]` and routes each discarded message into `OnMessageDropped`, so a subscriber that declares no `NatsSubOpts.ChannelOpts` has still chosen a drop policy and owes it the same declaration a hand-built bounded channel owes.
- `MessageDropped` is that loss's one evidence surface — `NatsMessageDroppedEventArgs` carries `Subscription` (`NatsSubBase`, exposing `Subject`), `Pending`, `Subject`, `ReplyTo`, `Headers`, and `Data` — and `SlowConsumerDetected` (`NatsSlowConsumerEventArgs.Subscription`) marks entry into that condition once per recovery cycle unless `SuppressSlowConsumerWarnings` is set. Core NATS carries no redelivery, so a dropped Core message is permanently lost and a subscription reading neither event accounts none of it.
- Teardown splits three ways and one alone preserves the buffer: cancelling the `SubscribeAsync<T>` enumerator abandons every pending message, `INatsSub<T>.UnsubscribeAsync()` completes the channel and drops what the socket still holds, and `INatsSub<T>.DrainAsync(ct)` sends UNSUB, fences on a PING/PONG round trip bounded by `DrainPingTimeout`, then completes `Msgs`.
- Flushing closes therefore hold the `SubscribeCoreAsync<T>` handle and read `Msgs` to completion; `DrainSubscriptionsOnDispose` with `ConsumerDrainOnDisposeTimeout` is the connection-wide counterpart.
- `NatsOpts.RequestReplyMode` defaults to `NatsRequestReplyMode.Direct`: a `RequestAsync` reply correlates on the connection's existing inbox subscription with the per-reply muxer skipped, `NatsNoRespondersException` still thrown at the no-responder reply; `SharedInbox` restores the per-request subscription-and-channel mechanism.
- Core `PublishAsync` returns once the frame is written, so the awaited `INatsJSContext.PublishAsync` and its `PubAckResponse` are the protocol's only durable delivery evidence; `TryPublishAsync` carries the same ack in the `NatsResult<T>` carrier instead of throwing.
- `PubAckResponse.Duplicate` reports the broker recognizing a prior `Nats-Msg-Id` inside the stream's `StreamConfig.DuplicateWindow`, which makes idempotent publish the exactly-once-effective primitive; `NatsJSDuplicateMessageException` is the distinct duplicate-sequence rejection, never the benign window hit.
- `ConsumerConfig` owns redelivery through `AckPolicy`, `AckWait`, `MaxDeliver`, and `Backoff`, and `INatsJSMsg<T>` closes each message with `AckAsync`, `NakAsync`, `AckProgressAsync`, or `AckTerminateAsync`; each consumer group tracks its own cursor independent of any store cursor a reader keeps; `NatsJSConsumeOpts.DrainOnCancel` opts a consume loop into delivering buffered messages after cancellation so handlers still ack, the default stopping immediately, and `INatsSub<T>.DrainAsync` is the Core-subscription counterpart fencing in-flight deliveries without tearing the connection — the graceful stop for one capture subscriber.
- `SubscribeAsync<byte[]>(subject, queueGroup, serializer, opts, ct)` drains through `await foreach` until `ct` trips or the connection drops, one `NatsMsg<byte[]>` per iteration; `queueGroup` load-balances the sensor subject across N capture subscribers, null for one, and `opts` is where a subscriber states its `[SUBSCRIPTION_POLICY]` bound. `SubscribeCoreAsync<T>(subject, queueGroup, serializer, opts, ct)` returns the `INatsSub<T>` handle instead, which is what a drain-closing subscriber holds.
- `NatsOpts.SerializerRegistry` fixes the per-type codec at construction, so `NatsRawSerializer<T>` carries already-encoded bytes and `NatsJsonContextSerializer<T>` the AOT-safe source-generated JSON form.
- `INatsKVStore.CreateAsync` and `UpdateAsync(key, value, revision)` are the create-if-absent and revision-CAS pair, refusing through `NatsKVCreateException` and `NatsKVWrongLastRevisionException`; `WatchAsync` is the changefeed and `HistoryAsync` the revision replay.
- `NatsInstrumentationOptions.Default` is process-static, so `Filter` and `Enrich` bind once for the whole process; spans emit on the `NATS.Net` `ActivitySource` under `messaging.system = nats`.

[STACKING]:
- `CloudNative.CloudEvents`(`api-cloudevents.md`): no NATS protocol binding ships, so the egress leg maps the message envelope onto `NatsHeaders` itself — the `ce-`-prefixed attribute rows beside the `traceparent` pair — while the dedup identity rides `NatsJSPubOpts.MsgId` from the content key rather than a hand-written header, and hands `NatsRawSerializer<T>` the formatter's bytes; on the ingest side `NatsMsg<byte[]>.Data` decodes through `Rasm/Domain/event` `EventEnvelope.Decode` when the framing names an admitted event format and through the branch-owned `ce-`-prefixed header binding otherwise, landing the typed `SensorReading<T>`, the W3C pair over `NatsMsg.Headers` symmetric to the MQTT `UserProperties` carrier.
- `Confluent.Kafka`(`Rasm.Persistence/.api/api-kafka.md`), `RabbitMQ.Client`(`Rasm.Persistence/.api/api-rabbitmq.md`), `DotPulsar`(`Rasm.Persistence/.api/api-dotpulsar.md`): peer egress `Binding` rows over one op-log message envelope, each folding its provider outcome to `DeliveryAck` at its own leg boundary.
- `StackExchange.Redis`(`Rasm.Persistence/.api/api-redis.md`): the same multiplexer-singleton topology and the same codec-owns-the-shape boundary; a Redis stream and a JetStream stream are peer sink rows whose group cursors never merge.
- `LightningDB`(`Rasm.Persistence/.api/api-lightningdb.md`), `ObjectStore`(`Rasm.Persistence/.api/api-objectstore.md`): embedded and cloud counterparts to the JetStream KV and Object tiers, all selected as `Store/provisioning` backend rows.
- `AspNetCore.HealthChecks.Nats`(`Rasm.AppHost/.api/api-healthchecks-nats.md`): probes the pooled `INatsConnection` as the `DriverProbe.Nats` contributor row, and the connection events feed that same health fold.
- Compute consumer anchor: a `NatsClient` subscription surfaces one `NatsMsg<byte[]>` per sample → `msg.Data` decodes through the kernel decode pair → `SensorReading<T>` admits onto `WorkLane.CaptureIngest` DropOldest (shedding oldest geometry state at the lane rather than blocking the drain) → `Stats/signal` folds the measured end (`Transform.Modal`) → `TwinLoop.Ingest`/`Update` closes into anomaly verdicts — the identical pipeline the MQTT adapter drives from `MqttApplicationMessage`; the NATS and MQTT bindings are two `BrokerBinding` rows under ONE `BrokerChannels.Decode<T>` and ONE `BrokerChannels.Pump<T>`, each row's `Reader` column the only per-dialect variance, so no `Nats<T>`/`Mqtt<T>` overload pair exists. `INatsConnection.RequestAsync<TReq, TReply>` dispatches a compute call awaiting `NatsMsg<TReply>` and `msg.ReplyAsync<TReply>` answers a request-subject message — the request/reply half beside the gRPC `CallSpine`, distinct from the fire-and-forget sensor subscription.
- Within the package one connection carries every leg: `TryPublishAsync` publishes on the ROP carrier, `PublishConcurrentAsync` defers its ack through `NatsJSPublishConcurrentFuture.GetResponseAsync` for pipelined batches, `NatsMsg<T>.StartActivity` continues the consume-side trace, `NatsAuthOpts.AuthCredCallback` rotates credentials per connect, and `NatsKVEntry<T>.Delta` bounds a watch catch-up.

[LOCAL_ADMISSION]:
- Persistence changefeed egress dials `INatsJSContext.TryPublishAsync` on its `Nats` sink row under a `NatsJSPubOpts` whose `MsgId` is the entity content key in lower-hex, folding a null `Error` to `Persisted`, `Duplicate` to `Persisted(Duplicate: true)`, a server `-ERR` or timeout to `Indeterminate`, and a fatal protocol fault to `Refused`; only the contiguous `Persisted` prefix advances the outbox cursor. Each publish builds a fresh `NatsHeaders`: publish leaves an instance mutable, so one instance never serves concurrent publishes.
- A durable stream provisions through `CreateOrUpdateStreamAsync(StreamConfig)` on file storage with a `DuplicateWindow` wide enough to absorb a held-cursor re-drive, and a downstream reader consumes it on its own `ConsumerConfig` cursor.
- PostgreSQL owns coordination: the `Store/coordination` fenced compare-and-swap under `LeaseToken` is the one lease and CAS vocabulary, and the JetStream KV enters as a distributed store-backend row on `Store/provisioning`; an Object Store bucket carries chunked blobs through `PutAsync(string, Stream, bool)` and closes with `SealAsync`, a distributed tier beside the embedded and cloud blob rows; domain code binds the `Try*` form and lifts `NatsResult.Error` onto the store fault carrier at one site.
- Compute ingest obtains declarations and whole-message admission from one kernel `EventExtensionContract<event.Extensions>` for structured bodies and `ce-` binary headers. `TraceContext.Continue` owns current-hop adoption while admitted generated extensions carry creation-time trace.
- Compute ingest states its own pending-channel bound on `NatsSubOpts.ChannelOpts` and subscribes to `MessageDropped` for the subscription's life, so the capture lane's loss is accounted on the same result its expiry drops ride rather than as the connection default's silent discard.
