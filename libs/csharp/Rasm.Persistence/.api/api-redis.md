# [RASM_PERSISTENCE_API_REDIS]

`StackExchange.Redis` mints the multiplexer, database, subscriber, server, and configuration surface for the cache and message-passing store profiles; `Microsoft.Extensions.Caching.StackExchangeRedis` binds the `IDistributedCache`/`IBufferDistributedCache` `RedisCache` over that same multiplexer for DI-wired distributed caching.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `StackExchange.Redis`
- package: `StackExchange.Redis`
- assembly: `StackExchange.Redis`
- namespace: `StackExchange.Redis`, `StackExchange.Redis.Profiling`
- target: `net10.0`
- asset: runtime library
- rail: cache

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.StackExchangeRedis`
- package: `Microsoft.Extensions.Caching.StackExchangeRedis`
- assembly: `Microsoft.Extensions.Caching.StackExchangeRedis`
- namespace: `Microsoft.Extensions.Caching.StackExchangeRedis`, `Microsoft.Extensions.DependencyInjection`
- target: `net10.0`
- asset: runtime library
- rail: cache

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: multiplexer and connection family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [CAPABILITY]                                                                         |
| :-----: | :----------------------- | :------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `ConnectionMultiplexer`  | multiplexer root     | manages all server connections                                                       |
|  [02]   | `IConnectionMultiplexer` | multiplexer contract | shared multiplexer capability                                                        |
|  [03]   | `ConfigurationOptions`   | configuration        | endpoint, auth, timeout, proxy policy                                                |
|  [04]   | `IDatabase`              | database contract    | key-value, hash, list, set, geo, stream ops                                          |
|  [05]   | `ISubscriber`            | pub/sub contract     | subscribe, publish, channel queues                                                   |
|  [06]   | `IServer`                | server contract      | admin, scan, info, config operations                                                 |
|  [07]   | `ChannelMessageQueue`    | message queue        | async-enumerable pub/sub queue                                                       |
|  [08]   | `RedisValue`             | value struct         | polymorphic Redis value carrier                                                      |
|  [09]   | `RedisKey`               | key struct           | Redis key with prefix support                                                        |
|  [10]   | `RedisChannel`           | channel struct       | pub/sub channel with pattern support                                                 |
|  [11]   | `HashEntry`              | hash entry           | field-value pair for hashes                                                          |
|  [12]   | `GeoEntry`               | geo entry            | member with longitude and latitude                                                   |
|  [13]   | `GeoPosition`            | geo position         | longitude and latitude value                                                         |
|  [14]   | `CommandFlags`           | flags enum           | per-command routing and response-wait policy                                         |
|  [15]   | `When`                   | condition enum       | conditional-SET predicate (always/XX/NX)                                             |
|  [16]   | `ExpireWhen`             | TTL condition enum   | conditional-TTL predicate (GT/LT/NX/XX)                                              |
|  [17]   | `SortedSetWhen`          | ZADD condition enum  | conditional-ZADD predicate (`[Flags]`, GT/LT/NX/XX)                                  |
|  [18]   | `RedisProtocol`          | protocol enum        | `Resp2`, `Resp3` wire-protocol selector                                              |
|  [19]   | `LoadedLuaScript`        | prepared script      | SHA1-cached `EVALSHA` server-side script                                             |
|  [20]   | `LuaScript`              | parsed script        | named-parameter Lua with `Prepare`/`Load`                                            |
|  [21]   | `ChannelMessage`         | message value        | one `(Channel, Message)` queue item                                                  |
|  [22]   | `StreamEntry`            | stream entry         | one `(Id, NameValueEntry[])` log record                                              |
|  [23]   | `StreamPosition`         | stream cursor        | `(Key, Position)` multi-stream read cursor; `Beginning`/`NewMessages` seed sentinels |
|  [24]   | `StreamIdempotentId`     | idempotent id        | dedup id for at-most-once `StreamAdd`                                                |
|  [25]   | `StreamTrimMode`         | trim policy enum     | `KeepReferences`/`DeleteReferences`/`Acknowledged`                                   |
|  [26]   | `NameValueEntry`         | stream field pair    | field-value pair for stream records                                                  |
|  [27]   | `BacklogPolicy`          | backlog policy       | command-backlog behavior while disconnected                                          |
|  [28]   | `IReconnectRetryPolicy`  | retry policy         | reconnect backoff strategy contract                                                  |

- `StreamPosition.Beginning` = `0-0`, `NewMessages` = `$` (static sentinels).

[PUBLIC_TYPE_SCOPE]: caching family

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [CAPABILITY]                                                               |
| :-----: | :------------------ | :---------------- | :------------------------------------------------------------------------- |
|  [01]   | `RedisCache`        | distributed cache | `IBufferDistributedCache`/`IDistributedCache`/`IDisposable` over Redis     |
|  [02]   | `RedisCacheOptions` | options           | options bag; `InstanceName` key-prefix + `ProfilingSession` telemetry hook |

- `RedisCache` implements `IBufferDistributedCache` (`ReadOnlySequence<byte>` zero-copy `Set`/`TryGet`, `Microsoft.Extensions.Caching.Distributed`), not only `IDistributedCache`; `RedisCacheOptions.ProfilingSession` (`Func<ProfilingSession>?`) binds cache traffic to the multiplexer profiling span.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: multiplexer lifecycle

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `ConnectionMultiplexer.Connect(configuration)`              | factory  | synchronous connect from options or string            |
|  [02]   | `ConnectionMultiplexer.ConnectAsync(configuration)`         | factory  | async connect from options or string                  |
|  [03]   | `IConnectionMultiplexer.GetDatabase(db?, asyncState?)`      | instance | obtains `IDatabase` for a logical database            |
|  [04]   | `IConnectionMultiplexer.GetSubscriber(asyncState?)`         | instance | obtains `ISubscriber`                                 |
|  [05]   | `IConnectionMultiplexer.GetServer(endpoint)`                | instance | obtains `IServer` for an endpoint                     |
|  [06]   | `IConnectionMultiplexer.GetEndPoints(configuredOnly?)`      | instance | returns all configured endpoints                      |
|  [07]   | `IConnectionMultiplexer.IsConnected`                        | property | true when any server is reachable                     |
|  [08]   | `ConfigurationOptions.Parse(configuration, ignoreUnknown?)` | factory  | parses connection string to options                   |
|  [09]   | `ConnectionMultiplexer.Connect(opts, configure, log?)`      | factory  | connect with a `Action<ConfigurationOptions>` mutator |
|  [10]   | `ConnectionMultiplexer.SentinelConnect(configuration)`      | factory  | Sentinel-managed HA primary discovery                 |

[ENTRYPOINT_SCOPE]: database key-value and data structure operations

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `StringGet(key, flags?)`                         | instance | gets value by key                                  |
|  [02]   | `StringSet(key, value, expiry?, when?, flags?)`  | instance | sets value with optional expiry                    |
|  [03]   | `StringGetSetExpiry(key, expiry)`                | instance | gets value and sets new expiry                     |
|  [04]   | `HashGet(key, field, flags?)`                    | instance | gets one hash field                                |
|  [05]   | `HashGetAll(key, flags?)`                        | instance | gets all `HashEntry[]`                             |
|  [06]   | `HashSet(key, entries, flags?)`                  | instance | sets multiple `HashEntry[]`                        |
|  [07]   | `ListLeftPush(key, values, flags?)`              | instance | prepends values                                    |
|  [08]   | `ListRightPop(key, flags?)`                      | instance | pops from right                                    |
|  [09]   | `SetAdd(key, values, flags?)`                    | instance | adds members                                       |
|  [10]   | `SortedSetAdd(key, entries, flags?)`             | instance | adds scored members                                |
|  [11]   | `KeyExpire(key, expiry, flags?)`                 | instance | sets key TTL                                       |
|  [12]   | `KeyDelete(keys, flags?)`                        | instance | deletes one or many keys                           |
|  [13]   | `KeyExpire(key, expiry, when, flags?)`           | instance | conditional TTL via `ExpireWhen` (GT/LT/NX/XX)     |
|  [14]   | `SortedSetAdd(key, member, score, when, flags?)` | instance | conditional ZADD via `SortedSetWhen` (GT/LT/NX/XX) |
|  [15]   | `Execute(command, args?)`                        | instance | executes arbitrary raw command                     |
|  [16]   | `CreateBatch(asyncState?)`                       | factory  | creates pipelined batch                            |
|  [17]   | `CreateTransaction(asyncState?)`                 | factory  | creates `MULTI/EXEC` transaction                   |

[ENTRYPOINT_SCOPE]: stream (durable append-log) operations

Every op carries a trailing `CommandFlags flags` and an `…Async` twin on `IDatabaseAsync`; the table drops both.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `StreamAdd(key, pairs, …, trimMode)`               | instance | XADD with `StreamTrimMode` capped trim; returns entry id |
|  [02]   | `StreamAdd(key, field, value, StreamIdempotentId)` | instance | at-most-once XADD keyed on `StreamIdempotentId`          |
|  [03]   | `StreamRead(StreamPosition[], countPerStream?)`    | instance | multi-stream XREAD from `(key, position)` cursors        |
|  [04]   | `StreamReadGroup(key, group, consumer, …)`         | instance | XREADGROUP cursor-replay with idle-claim takeover        |
|  [05]   | `StreamAcknowledge(key, group, messageIds)`        | instance | XACK processed entries; advances the group cursor        |
|  [06]   | `StreamCreateConsumerGroup(key, group, position?)` | instance | XGROUP CREATE the replay cursor                          |

[ENTRYPOINT_SCOPE]: pub/sub and subscriber operations

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `ISubscriber.Subscribe(channel, handler, flags?)`       | instance | attaches message handler                 |
|  [02]   | `ISubscriber.SubscribeAsync(channel, handler?, flags?)` | instance | async subscription with optional handler |
|  [03]   | `ISubscriber.Publish(channel, message, flags?)`         | instance | publishes message to channel             |
|  [04]   | `ISubscriber.PublishAsync(channel, message, flags?)`    | instance | async publish                            |
|  [05]   | `ISubscriber.Unsubscribe(channel, handler?, flags?)`    | instance | removes subscription                     |

[ENTRYPOINT_SCOPE]: DI-wired caching

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `RedisCacheOptions.Configuration`                     | property | connection string                             |
|  [02]   | `RedisCacheOptions.ConfigurationOptions`              | property | `ConfigurationOptions` (preferred)            |
|  [03]   | `RedisCacheOptions.ConnectionMultiplexerFactory`      | property | `Func<Task<IConnectionMultiplexer>>`          |
|  [04]   | `RedisCacheOptions.InstanceName`                      | property | key prefix for cache partitioning             |
|  [05]   | `services.AddStackExchangeRedisCache(options => ...)` | static   | registers `RedisCache` as `IDistributedCache` |

[ENTRYPOINT_SCOPE]: RESP3 protocol and server-assisted client-side caching

Raw-command rows carry no typed member and ride `Execute`.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `ConfigurationOptions.Protocol`                    | property | `RedisProtocol?` — selects `Resp3` for the connection   |
|  [02]   | `IServer.Protocol`                                 | property | negotiated `RedisProtocol` after `HELLO` (per-server)   |
|  [03]   | `ISubscriber.Subscribe(RedisChannel.Literal(…))`   | instance | `__redis__:invalidate` RESP3 invalidation push channel  |
|  [04]   | `IDatabase.Execute("CLIENT", "TRACKING", "ON", …)` | instance | enables key-tracking so the server pushes invalidations |
|  [05]   | `IServer.Execute("CLIENT", "TRACKINGINFO")`        | instance | reads the tracking redirect/bcast state                 |

[ENTRYPOINT_SCOPE]: keyspace-notification subscription

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `IServer.ConfigSet("notify-keyspace-events", "KEA")`     | instance | enables `__keyspace@N__`/`__keyevent@N__` emission       |
|  [02]   | `IServer.ConfigGet("notify-keyspace-events")`            | instance | reads the active notification flag set                   |
|  [03]   | `RedisChannel.KeySpacePattern(key, database?)`           | factory  | typed `__keyspace@<db>__:<key>` notification channel     |
|  [04]   | `RedisChannel.KeySpacePrefix(prefix, database?)`         | factory  | `appendStar` prefix `__keyspace@<db>__:<prefix>*` fan-in |
|  [05]   | `RedisChannel.Pattern("__keyevent@*__:*")`               | factory  | no typed factory; rides `Pattern`/`Literal`              |
|  [06]   | `ISubscriber.SubscribeAsync(channel)`                    | instance | returns the channel's `ChannelMessageQueue`              |
|  [07]   | `ChannelMessageQueue : IAsyncEnumerable<ChannelMessage>` | fold     | the async pub/sub stream; drain members ↓                |

- `ChannelMessageQueue` drains via `await foreach (queue.WithCancellation(token))`, `ReadAsync`/`TryRead`/`OnMessage`/`Completion`.

[ENTRYPOINT_SCOPE]: Lua scripting and server functions

`FUNCTION`/`FCALL` rows carry no typed member and ride `Execute`; `ScriptEvaluate`/`ScriptEvaluateReadOnly` take `(script, RedisKey[], RedisValue[], CommandFlags)`, the read-only form accepting a string or SHA1 `byte[]`; `LuaScript.Evaluate`/`EvaluateAsync` bind `@name` tokens from an anonymous-object `ps` and prefix every key through `withKeyPrefix`.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------- | :------- | :---------------------------------------------------------- |
|  [01]   | `LuaScript.Prepare(script)`                    | factory  | parses `@name` parameter tokens into a `LuaScript`          |
|  [02]   | `LuaScript.Load(IServer)`                      | factory  | `SCRIPT LOAD` → SHA1-cached `LoadedLuaScript`               |
|  [03]   | `IDatabase.ScriptEvaluate(LoadedLuaScript, …)` | instance | `EVALSHA` single-flight; one round-trip atomic script       |
|  [04]   | `IDatabase.ScriptEvaluate(script, …)`          | instance | inline `EVAL` over explicit `RedisKey[]`/`RedisValue[]`     |
|  [05]   | `IDatabase.ScriptEvaluateReadOnly(script, …)`  | instance | `EVAL_RO`/`EVALSHA_RO`; replica under `PreferReplica`       |
|  [06]   | `IServer.ScriptLoad(script)`                   | instance | returns the script SHA1 `byte[]` for later `EVALSHA`        |
|  [07]   | `IDatabase.Execute("FUNCTION", "LOAD", code)`  | instance | Redis 7 `FUNCTION LOAD` library register                    |
|  [08]   | `IDatabase.Execute("FCALL", name, …)`          | instance | invokes a loaded function                                   |
|  [09]   | `IDatabase.Execute("FCALL_RO", name, …)`       | instance | read-only function call routed to a replica                 |
|  [10]   | `LuaScript.Evaluate(…)` / `EvaluateAsync(…)`   | instance | `EVAL`/`EVALSHA` off the script; `@name` anon-object params |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ConnectionMultiplexer` is the long-lived shared singleton root, `IDisposable`/`IAsyncDisposable`; `IDatabase` is per-call, lightweight, non-disposable, obtained via `GetDatabase` at the op boundary, never the composition root.
- `ConfigurationOptions.AbortOnConnectFail` defaults `false` when parsed through `RedisCacheOptions`.
- `IConnectionMultiplexer` events: `ConnectionFailed` `ConnectionRestored` `ErrorMessage` `InternalError` `ConfigurationChanged` `ConfigurationChangedBroadcast` `ServerMaintenanceEvent` `HashSlotMoved` — `ServerMaintenanceEvent` surfaces managed-Redis failover windows, `ConfigurationChangedBroadcast` carries cluster-wide config pushes.
- `When` gates conditional SET, `ExpireWhen` conditional TTL, `SortedSetWhen` conditional ZADD (all GT/LT/NX/XX); `CommandFlags.PreferReplica` with `ScriptEvaluateReadOnly`/`FCALL_RO` routes reads to replicas, `FireAndForget` skips the response wait.
- `ConfigurationOptions` carry `Protocol` `HighIntegrity` `HeartbeatConsistencyChecks` `BacklogPolicy` `ReconnectRetryPolicy` `LoggerFactory` (→ AppHost `telemetry` spine) `ChannelPrefix` `SslProtocols` `SslClientAuthenticationOptions` `DefaultDatabase` `ClientName`; `RedisValue` is the stack-allocated polymorphic value struct.
- RESP3: `ConfigurationOptions.Protocol = RedisProtocol.Resp3` negotiates at `HELLO`, read back per-server on `IServer.Protocol`; server-assisted client-side caching rides the broadcast `__redis__:invalidate` push, and `CLIENT TRACKING ON BCAST PREFIX` through `IDatabase.Execute` drops an L1 entry on the source-key change rather than at TTL.
- Keyspace: `notify-keyspace-events` is off by default, `IServer.ConfigSet(…, "KEA")` arms keyspace/keyevent/all-classes as connection-runtime state; typed `RedisChannel.KeySpacePattern`/`KeySpacePrefix` build the `__keyspace@<db>__` channel (KEYEVENT rides `RedisChannel.Pattern`, no typed factory), drained off the returned `ChannelMessageQueue` `IAsyncEnumerable`. Best-effort fire-and-forget — the push REFINES (low-latency hint to) the at-least-once `StreamReadGroup`+`StreamAcknowledge` cursor, never replaces it.
- Lua: `LuaScript.Prepare` parses `@name` tokens, `Load` mints a `LoadedLuaScript` whose `EVALSHA` is a single-round-trip atomic script, so the stampede single-flight and writer-lease fence are one server-side script, never a managed compare-loop; `ScriptEvaluate` takes the `LoadedLuaScript`, inline text, or SHA, and `FUNCTION`/`FCALL` ride `Execute`, so pin `ScriptEvaluate` first and the `Execute` escape only where no typed member exists.

[STACKING]:
- `api-hybrid-cache`(`libs/csharp/.api/api-hybrid-cache.md`): `RedisCache : IBufferDistributedCache` is the zero-copy L2 that `DefaultHybridCache` layers an in-process L1 over — register `AddStackExchangeRedisCache`+`AddHybridCache`, and a `HybridCache.GetOrCreateAsync` read serves L1, falls to the L2 over `ReadOnlySequence<byte>`, mints once, and drops the L1 entry on the `__redis__:invalidate` RESP3 push so the tiers stay coherent without per-read TTL races.
- `api-thinktecture-serialization`/`api-messagepack`(`.api/`): the L2 byte payload is the snapshot of a `[ValueObject]`/`[SmartEnum]` owner via `IHybridCacheSerializer<T>` — Redis stores the bytes, the codec owns the shape.
- within-lib: `ConfigurationOptions.LoggerFactory` and `RedisCacheOptions.ProfilingSession` route multiplexer and cache traffic onto the AppHost `telemetry` spine, so connection faults and cache latency ride the store-profile span.

[LOCAL_ADMISSION]:
- `RedisCacheOptions.ConfigurationOptions` outranks the `Configuration` string.
- `InstanceName` prefixes every cache key; omitting it shares the keyspace across consumers.
- Live-fabric state — Lua scripts, keyspace subscription, Stream consumer group, RESP3 tracking — is connection-instance on the multiplexer; absent Redis the TTL-bounded baseline is bit-identical, so the live path is additive capability, never a new dependency.

[RAIL_LAW]:
- Package: `StackExchange.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`
- Owns: Redis multiplexed transport and the distributed cache layer
- Accept: `ConnectionMultiplexer` singleton, `IDatabase` per-operation, `RedisCacheOptions` on the DI path
- Reject: per-operation multiplexer construction, raw TCP Redis protocol without the multiplexer
