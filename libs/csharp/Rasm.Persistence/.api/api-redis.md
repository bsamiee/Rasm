# [RASM_PERSISTENCE_API_REDIS]

`StackExchange.Redis` supplies the Redis multiplexer, database, subscriber, server, and
configuration surfaces for cache and message-passing store profiles. `Microsoft.Extensions.Caching.StackExchangeRedis`
provides the `IDistributedCache`-backed `RedisCache` and its `RedisCacheOptions` for DI-wired
distributed caching over the same multiplexer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `StackExchange.Redis`
- package: `StackExchange.Redis`
- version: `3.0.7`
- assembly: `StackExchange.Redis`
- namespace: `StackExchange.Redis`, `StackExchange.Redis.Profiling`
- target framework: `net10.0` asset on the `net10.0` floor (package also ships `net8.0`/`net6.0`/`netstandard2.0`/`net472`/`net461`)
- asset: runtime library
- rail: cache

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.StackExchangeRedis`
- package: `Microsoft.Extensions.Caching.StackExchangeRedis`
- version: `10.0.9`
- assembly: `Microsoft.Extensions.Caching.StackExchangeRedis`
- namespace: `Microsoft.Extensions.Caching.StackExchangeRedis`, `Microsoft.Extensions.DependencyInjection`
- target framework: `net10.0` asset on the `net10.0` floor (package also ships `net462`/`netstandard2.0`)
- asset: runtime library
- rail: cache

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: multiplexer and connection family
- rail: cache

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [CAPABILITY]                                |
| :-----: | :----------------------- | :------------------- | :------------------------------------------ |
|  [01]   | `ConnectionMultiplexer`  | multiplexer root     | manages all server connections              |
|  [02]   | `IConnectionMultiplexer` | multiplexer contract | shared multiplexer capability               |
|  [03]   | `ConfigurationOptions`   | configuration        | endpoint, auth, timeout, proxy policy       |
|  [04]   | `IDatabase`              | database contract    | key-value, hash, list, set, geo, stream ops |
|  [05]   | `ISubscriber`            | pub/sub contract     | subscribe, publish, channel queues          |
|  [06]   | `IServer`                | server contract      | admin, scan, info, config operations        |
|  [07]   | `ChannelMessageQueue`    | message queue        | async-enumerable pub/sub queue              |
|  [08]   | `RedisValue`             | value struct         | polymorphic Redis value carrier             |
|  [09]   | `RedisKey`               | key struct           | Redis key with prefix support               |
|  [10]   | `RedisChannel`           | channel struct       | pub/sub channel with pattern support        |
|  [11]   | `HashEntry`              | hash entry           | field-value pair for hashes                 |
|  [12]   | `GeoEntry`               | geo entry            | member with longitude and latitude          |
|  [13]   | `GeoPosition`            | geo position         | longitude and latitude value                |
|  [14]   | `CommandFlags`           | flags enum           | fire-and-forget, prefer-replica, etc.       |
|  [15]   | `When`                   | condition enum       | `Always`, `Exists`, `NotExists`             |
|  [16]   | `ExpireWhen`             | TTL condition enum   | `Always`/`GreaterThanCurrentExpiry`/`LessThanCurrentExpiry`/`HasExpiry`/`HasNoExpiry` (GT/LT/NX/XX) |
|  [17]   | `SortedSetWhen`          | ZADD condition enum  | `Always`/`Exists`/`NotExists`/`GreaterThan`/`LessThan` (GT/LT/NX/XX `[Flags]`) |
|  [18]   | `RedisProtocol`          | protocol enum        | `Resp2`, `Resp3` wire-protocol selector     |
|  [19]   | `LoadedLuaScript`        | prepared script      | SHA1-cached `EVALSHA` server-side script    |
|  [20]   | `LuaScript`              | parsed script        | named-parameter Lua with `Prepare`/`Load`   |
|  [21]   | `ChannelMessage`         | message value        | one `(Channel, Message)` queue item         |
|  [22]   | `StreamEntry`            | stream entry         | one `(Id, NameValueEntry[])` log record     |
|  [23]   | `StreamPosition`         | stream cursor        | `(Key, Position)` multi-stream read cursor; static `Beginning` (`0-0`) / `NewMessages` (`$`) sentinels seed `StreamReadGroup`/`StreamCreateConsumerGroup` |
|  [24]   | `StreamIdempotentId`     | idempotent id        | dedup id for at-most-once `StreamAdd`       |
|  [25]   | `StreamTrimMode`         | trim policy enum     | `KeepReferences`/`DeleteReferences`/`Acknowledged` |
|  [26]   | `NameValueEntry`         | stream field pair    | field-value pair for stream records         |
|  [27]   | `BacklogPolicy`          | backlog policy       | command-backlog behavior while disconnected |
|  [28]   | `IReconnectRetryPolicy`  | retry policy         | reconnect backoff strategy contract         |

[PUBLIC_TYPE_SCOPE]: caching family
- rail: cache

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [CAPABILITY]                                                                  |
| :-----: | :------------------ | :---------------- | :--------------------------------------------------------------------------- |
|  [01]   | `RedisCache`        | distributed cache | `IBufferDistributedCache, IDistributedCache, IDisposable` over Redis          |
|  [02]   | `RedisCacheOptions` | options           | `Configuration`/`ConfigurationOptions`/`ConnectionMultiplexerFactory`/`InstanceName`/`ProfilingSession` |

`RedisCache` implements `IBufferDistributedCache` (the `ReadOnlySequence<byte>` zero-copy `Set`/`TryGet` surface, namespace `Microsoft.Extensions.Caching.Distributed`), NOT only `IDistributedCache` — this is the exact L2 contract that `DefaultHybridCache` (`api-hybrid-cache`) sniffs for to stack an in-process L1 over the Redis L2 without a double-buffer copy. `RedisCacheOptions.ProfilingSession` (`Func<ProfilingSession>?`) wires the `StackExchange.Redis.Profiling` session so cache traffic rides the same profiling/telemetry span as the raw multiplexer.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: multiplexer lifecycle
- rail: cache

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `ConnectionMultiplexer.Connect(configuration)`         | static factory | synchronous connect from options or string |
|  [02]   | `ConnectionMultiplexer.ConnectAsync(configuration)`    | async factory  | async connect from options or string       |
|  [03]   | `IConnectionMultiplexer.GetDatabase(db?, asyncState?)` | access         | obtains `IDatabase` for a logical database |
|  [04]   | `IConnectionMultiplexer.GetSubscriber(asyncState?)`    | access         | obtains `ISubscriber`                      |
|  [05]   | `IConnectionMultiplexer.GetServer(endpoint)`           | access         | obtains `IServer` for an endpoint          |
|  [06]   | `IConnectionMultiplexer.GetEndPoints(configuredOnly?)` | discovery      | returns all configured endpoints           |
|  [07]   | `IConnectionMultiplexer.IsConnected`                   | property       | true when any server is reachable          |
|  [08]   | `ConfigurationOptions.Parse(configuration, ignoreUnknown?)` | factory   | parses connection string to options        |
|  [09]   | `ConnectionMultiplexer.Connect(opts, configure, log?)` | static factory | connect with a `Action<ConfigurationOptions>` mutator |
|  [10]   | `ConnectionMultiplexer.SentinelConnect(configuration)` | static factory | Sentinel-managed HA primary discovery      |

[ENTRYPOINT_SCOPE]: database key-value and data structure operations
- rail: cache

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]      | [CAPABILITY]                     |
| :-----: | :---------------------------------------------- | :------------------ | :------------------------------- |
|  [01]   | `StringGet(key, flags?)`                        | string read         | gets value by key                |
|  [02]   | `StringSet(key, value, expiry?, when?, flags?)` | string write        | sets value with optional expiry  |
|  [03]   | `StringGetSetExpiry(key, expiry)`               | atomic update       | gets value and sets new expiry   |
|  [04]   | `HashGet(key, field, flags?)`                   | hash read           | gets one hash field              |
|  [05]   | `HashGetAll(key, flags?)`                       | hash read           | gets all `HashEntry[]`           |
|  [06]   | `HashSet(key, entries, flags?)`                 | hash write          | sets multiple `HashEntry[]`      |
|  [07]   | `ListLeftPush(key, values, flags?)`             | list write          | prepends values                  |
|  [08]   | `ListRightPop(key, flags?)`                     | list read           | pops from right                  |
|  [09]   | `SetAdd(key, values, flags?)`                   | set write           | adds members                     |
|  [10]   | `SortedSetAdd(key, entries, flags?)`            | sorted set write    | adds scored members              |
|  [11]   | `KeyExpire(key, expiry, flags?)`                | key policy          | sets key TTL                     |
|  [12]   | `KeyDelete(keys, flags?)`                       | key removal         | deletes one or many keys         |
|  [13]   | `KeyExpire(key, expiry, when, flags?)`          | key policy           | conditional TTL via `ExpireWhen` (GT/LT/NX/XX) |
|  [14]   | `SortedSetAdd(key, member, score, when, flags?)` | sorted set write     | conditional ZADD via `SortedSetWhen` (GT/LT/NX/XX) |
|  [15]   | `Execute(command, args?)`                       | raw command         | executes arbitrary raw command   |
|  [16]   | `CreateBatch(asyncState?)`                      | batch factory       | creates pipelined batch          |
|  [17]   | `CreateTransaction(asyncState?)`                | transaction factory | creates `MULTI/EXEC` transaction |

[ENTRYPOINT_SCOPE]: stream (durable append-log) operations
- rail: cache

The Redis Stream is the at-least-once durable log that COMPLEMENTS the best-effort fire-and-forget keyspace/pub-sub push: a consumer group replays from a committed cursor, where pub/sub drops on disconnect. `StreamReadGroup` plus `StreamAcknowledge` is the cursor-replayable drain that the keyspace-notification path REFINES (see `KEYSPACE_NOTIFICATION`).

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]   | [CAPABILITY]                                                            |
| :-----: | :--------------------------------------------------------------------- | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `StreamAdd(key, pairs, messageId?, maxLength?, …, trimMode, flags?)`   | stream write     | XADD with `StreamTrimMode` capped trim; returns the assigned entry id  |
|  [02]   | `StreamAdd(key, field, value, StreamIdempotentId, …)`                  | idempotent write | at-most-once XADD keyed on `StreamIdempotentId`                         |
|  [03]   | `StreamRead(StreamPosition[], countPerStream?, flags?)`               | fan-in read      | multi-stream XREAD from explicit `(key, position)` cursors             |
|  [04]   | `StreamReadGroup(key, group, consumer, position?, count?, noAck?, claimMinIdleTime?, flags?)` | group drain | XREADGROUP cursor-replay with idle-claim takeover                      |
|  [05]   | `StreamAcknowledge(key, group, messageIds, flags?)`                   | commit           | XACK the processed entries so the group cursor advances                |
|  [06]   | `StreamCreateConsumerGroup(key, group, position?, …)`                | group setup      | XGROUP CREATE the replay cursor                                        |
|  [07]   | `StreamReadGroupAsync(key, group, consumer, position?, count?, [noAck,] flags?)` / `StreamAcknowledgeAsync(key, group, messageId\|messageIds[], flags?)` / `StreamAddAsync(…)` | async twin | every `IDatabase` stream op has an `…Async` twin on `IDatabaseAsync` — the form the cache-fabric drain loop awaits |

[ENTRYPOINT_SCOPE]: pub/sub and subscriber operations
- rail: cache

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------ | :-------------- | :--------------------------------------- |
|  [01]   | `ISubscriber.Subscribe(channel, handler, flags?)`       | subscribe       | attaches message handler                 |
|  [02]   | `ISubscriber.SubscribeAsync(channel, handler?, flags?)` | async subscribe | async subscription with optional handler |
|  [03]   | `ISubscriber.Publish(channel, message, flags?)`         | publish         | publishes message to channel             |
|  [04]   | `ISubscriber.PublishAsync(channel, message, flags?)`    | async publish   | async publish                            |
|  [05]   | `ISubscriber.Unsubscribe(channel, handler?, flags?)`    | unsubscribe     | removes subscription                     |

[ENTRYPOINT_SCOPE]: DI-wired caching
- rail: cache

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `RedisCacheOptions.Configuration`                     | options property | connection string                             |
|  [02]   | `RedisCacheOptions.ConfigurationOptions`              | options property | `ConfigurationOptions` (preferred)            |
|  [03]   | `RedisCacheOptions.ConnectionMultiplexerFactory`      | options property | `Func<Task<IConnectionMultiplexer>>`          |
|  [04]   | `RedisCacheOptions.InstanceName`                      | options property | key prefix for cache partitioning             |
|  [05]   | `services.AddStackExchangeRedisCache(options => ...)` | DI extension     | registers `RedisCache` as `IDistributedCache` |

[ENTRYPOINT_SCOPE]: RESP3 protocol and server-assisted client-side caching
- rail: cache

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]   | [CAPABILITY]                                                                                             |
| :-----: | :----------------------------------------------------------------------- | :--------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `ConfigurationOptions.Protocol`                                          | options property | `RedisProtocol?` — selects `Resp3` for the connection                                                    |
|  [02]   | `IServer.Protocol`                                                       | property         | the negotiated `RedisProtocol` after `HELLO` (per-server; `ConnectionMultiplexer` exposes no `Protocol`) |
|  [03]   | `ISubscriber.Subscribe(RedisChannel.Literal("__redis__:invalidate"), …)` | subscribe        | the RESP3 server-assisted client-side-caching invalidation push channel                                  |
|  [04]   | `IDatabase.Execute("CLIENT", "TRACKING", "ON", …)`                       | raw command      | enables key-tracking so the server pushes invalidations (no typed member; rides `Execute`)               |
|  [05]   | `IServer.Execute("CLIENT", "TRACKINGINFO")`                              | raw command      | reads the tracking redirect/bcast state (no typed member; rides `Execute`)                               |

[ENTRYPOINT_SCOPE]: keyspace-notification subscription
- rail: cache

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [CAPABILITY]                                                                                                                                            |
| :-----: | :--------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `IServer.ConfigSet("notify-keyspace-events", "KEA")`                   | config write    | enables `__keyspace@N__`/`__keyevent@N__` event emission                                                                                                |
|  [02]   | `IServer.ConfigGet("notify-keyspace-events")`                          | config read     | reads the active notification flag set                                                                                                                  |
|  [03]   | `RedisChannel.KeySpacePattern(key, database?)`                         | typed factory   | builds the `__keyspace@<db>__:<key>` notification channel — the typed replacement for a hand-built `RedisChannel.Pattern("__keyspace@*__:*")` string     |
|  [04]   | `RedisChannel.KeySpacePrefix(prefix, database?)`                       | typed factory   | the prefix form (`appendStar`) for `__keyspace@<db>__:<prefix>*` fan-in                                                                                  |
|  [05]   | `RedisChannel.Pattern("__keyevent@*__:*")`                             | pattern factory | the keyEVENT channel has no dedicated typed factory — it rides `RedisChannel.Pattern`/`Literal`                                                          |
|  [06]   | `ISubscriber.SubscribeAsync(channel)`                                  | async subscribe | returns the `ChannelMessageQueue` of the subscribed keyspace/keyevent channel                                                                            |
|  [07]   | `ChannelMessageQueue : IAsyncEnumerable<ChannelMessage>`               | async enumerate | the queue IS the async stream — `await foreach (var m in queue.WithCancellation(token))`; also `ReadAsync(token)`/`TryRead(out m)`/`OnMessage(handler)`/`Completion` |

[ENTRYPOINT_SCOPE]: Lua scripting and server functions
- rail: cache

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `LuaScript.Prepare(script)`                              | static factory | parses `@name` parameter tokens into a `LuaScript`                          |
|  [02]   | `LuaScript.Load(IServer)`                                | server load    | `SCRIPT LOAD` returning a SHA1-cached `LoadedLuaScript`                     |
|  [03]   | `IDatabase.ScriptEvaluate(LoadedLuaScript, parameters)`  | atomic eval    | `EVALSHA` single-flight; one round-trip atomic script                       |
|  [04]   | `IDatabase.ScriptEvaluate(script, keys, values, flags?)` | atomic eval    | inline `EVAL` over explicit `RedisKey[]`/`RedisValue[]`                     |
|  [05]   | `IDatabase.ScriptEvaluateReadOnly(script, keys, values, flags?)` | replica eval | `EVAL_RO`/`EVALSHA_RO` (string or SHA1 `byte[]` overloads) — read-only scripts route to a replica under `PreferReplica` |
|  [06]   | `IServer.ScriptLoad(script)`                             | server load    | returns the script SHA1 `byte[]` for later `EVALSHA`                        |
|  [07]   | `IDatabase.Execute("FUNCTION", "LOAD", code)`            | raw command    | Redis 7 `FUNCTION LOAD` library register (no typed member; rides `Execute`) |
|  [08]   | `IDatabase.Execute("FCALL", name, …)`                    | raw command    | invokes a loaded function (no typed member; rides `Execute`)                |
|  [09]   | `IDatabase.Execute("FCALL_RO", name, …)`                 | raw command    | read-only function call routed to a replica (no typed member; rides `Execute`) |
|  [10]   | `LuaScript.Evaluate(IDatabase db, object? ps = null, RedisKey? withKeyPrefix = null, CommandFlags = None)` / `EvaluateAsync(IDatabaseAsync db, object? ps = null, RedisKey? = null, CommandFlags = None)` (mirror on `LoadedLuaScript`) | script eval | `EVAL`/`EVALSHA` directly off the parsed/loaded script with `@name` params bound from an anonymous object — the single-flight lease drain form, beside `IDatabase.ScriptEvaluate` |

## [04]-[IMPLEMENTATION_LAW]

[REDIS_TOPOLOGY]:
- `ConnectionMultiplexer` is the long-lived shared root; it is `IDisposable` and `IAsyncDisposable`
- `IDatabase` is obtained per-call via `GetDatabase`; it is lightweight and not independently disposable
- `ConfigurationOptions.AbortOnConnectFail = false` is the default when parsed via `RedisCacheOptions`
- `IConnectionMultiplexer` events: `ConnectionFailed`, `ConnectionRestored`, `ErrorMessage`, `InternalError`, `ConfigurationChanged`, `ConfigurationChangedBroadcast`, `ServerMaintenanceEvent`, `HashSlotMoved` — `ServerMaintenanceEvent` surfaces managed-Redis (Azure) failover windows, `ConfigurationChangedBroadcast` carries cluster-wide config pushes
- `When` enum: `Always`, `Exists`, `NotExists` for conditional SET; `ExpireWhen` (`GreaterThanCurrentExpiry`/`LessThanCurrentExpiry`/`HasExpiry`/`HasNoExpiry`) gates conditional TTL (GT/LT/NX/XX); `SortedSetWhen` (`GreaterThan`/`LessThan`/`Exists`/`NotExists`, `[Flags]`) gates conditional ZADD
- `CommandFlags.FireAndForget` skips response waiting; `CommandFlags.PreferReplica` routes reads to replicas; `ScriptEvaluateReadOnly`/`FCALL_RO` are the read-only eval forms eligible for that replica routing
- `RedisValue` is a stack-allocated struct covering `null`, `int64`, `uint64`, `double`, `string`, `byte[]`, and `ReadOnlyMemory<byte>`
- `ConfigurationOptions`: `Protocol` (`RedisProtocol?`, RESP2/RESP3), `HighIntegrity` (per-command integrity checksums), `HeartbeatConsistencyChecks`, `BacklogPolicy` (queued-command behavior while disconnected), `ReconnectRetryPolicy` (`IReconnectRetryPolicy` backoff), `LoggerFactory` (`ILoggerFactory?` — routes multiplexer diagnostics into the AppHost `telemetry` logging spine), `ChannelPrefix`, `SslProtocols`, `SslClientAuthenticationOptions`, `DefaultDatabase`, `ClientName`

[RESP3_CLIENT_SIDE_CACHING]:
- `ConfigurationOptions.Protocol = RedisProtocol.Resp3` negotiates RESP3 at `HELLO`; the negotiated value reads back per-server on `IServer.Protocol` (`ConnectionMultiplexer` carries no `Protocol` member — obtain a server via `GetServer` to read the active protocol).
- Server-assisted client-side caching is the broadcast `__redis__:invalidate` push channel — subscribe `ISubscriber` to `RedisChannel.Literal("__redis__:invalidate")` and the server pushes the invalidated key (or `RedisValue.Null` for a flush) so an L1 entry drops on the source key change rather than at TTL.
- `CLIENT TRACKING ON BCAST PREFIX <p>` (issued through `IDatabase.Execute` — no typed member) arms the tracking; the broadcast mode keys on a prefix set so the connection receives one invalidation per changed key under those prefixes, never a per-key opt-in round trip.

[KEYSPACE_NOTIFICATION]:
- `notify-keyspace-events` is OFF by default; `IServer.ConfigSet("notify-keyspace-events", "KEA")` arms keyspace (`K`), keyevent (`E`), and all classes (`A`); the flag is connection-instance runtime state, never persisted by a Rasm process.
- `__keyevent@<db>__:<event>` carries the affected key as the message and `__keyspace@<db>__:<key>` carries the event name; build the KEYSPACE channel with the TYPED `RedisChannel.KeySpacePattern(key, db)` / `RedisChannel.KeySpacePrefix(prefix, db)` factories (not a hand-spelled `RedisChannel.Pattern("__keyspace@*__:*")` string), while the KEYEVENT channel still rides `RedisChannel.Pattern("__keyevent@*__:*")` (no dedicated typed factory); subscribe through `ISubscriber.SubscribeAsync` and drain the returned `ChannelMessageQueue` — the queue IS `IAsyncEnumerable<ChannelMessage>`, so `await foreach (var m in queue.WithCancellation(token))` is the backpressure-safe drain, never a non-existent `ReadAllAsync` member.
- Keyspace notifications are best-effort fire-and-forget — a disconnected subscriber misses events, so the push path REFINES (feeds a low-latency hint to) the Redis Stream consumer-group replay (`StreamReadGroup` + `StreamAcknowledge`), which is the at-least-once cursor of record; the notification never replaces the stream cursor.

[LUA_AND_FUNCTIONS]:
- `LuaScript.Prepare(text)` parses `@name` tokens; `LuaScript.Load(IServer)` returns a `LoadedLuaScript` whose `EVALSHA` is a single round-trip atomic script — the stampede single-flight and the writer-lease fence are one atomic server-side script, never a managed compare-loop.
- `IServer.ScriptLoad` returns the raw SHA1 for an explicit `EVALSHA`; `IDatabase.ScriptEvaluate` accepts either the `LoadedLuaScript`, the inline text, or the SHA with explicit `RedisKey[]`/`RedisValue[]`.
- Redis 7 `FUNCTION LOAD`/`FCALL` have no typed member; they ride `IDatabase.Execute("FUNCTION", "LOAD", code)` and `Execute("FCALL", name, …)`. Pin the typed `ScriptEvaluate` spelling first and the `Execute` escape hatch only where a typed member is absent.

[STACKING]:
- L1+L2 cache: `RedisCache : IBufferDistributedCache` is the zero-copy distributed L2 that `DefaultHybridCache` (`api-hybrid-cache`) detects and layers an in-process L1 over — register `AddStackExchangeRedisCache` and `AddHybridCache` together and a `HybridCache.GetOrCreateAsync` read serves L1, falls to the Redis L2 over `ReadOnlySequence<byte>`, and mints once; the `__redis__:invalidate` RESP3 push drops the L1 entry on the source-key change so the two tiers stay coherent without per-read TTL races.
- snapshot codec at the wire: the L2 byte payload is the `api-thinktecture-serialization`/`api-messagepack` snapshot of a `[ValueObject]`/`[SmartEnum]` owner via `IHybridCacheSerializer<T>` — Redis stores the bytes, the codec owns the shape; no JSON is hand-spelled at the cache boundary.
- telemetry: `ConfigurationOptions.LoggerFactory` and `RedisCacheOptions.ProfilingSession` route multiplexer and cache traffic into the AppHost `telemetry` spine, so connection faults and cache latency ride the same span as store-profile operations.

[LOCAL_ADMISSION]:
- The multiplexer is a singleton; call `GetDatabase` at the operation boundary, not at composition root.
- `RedisCacheOptions.ConfigurationOptions` takes precedence over `RedisCacheOptions.Configuration` string.
- `InstanceName` prefixes all cache keys; omitting it shares the keyspace across all consumers.
- Pub/sub uses `ChannelMessageQueue` (returned by `SubscribeAsync`) for backpressure-safe async enumeration.
- The Lua scripts, the keyspace subscription, the Stream consumer group, and the RESP3 tracking arming are connection-instance state on the multiplexer; absent Redis the TTL-bounded baseline is bit-identical, so the live-fabric path is additive capability, never a new dependency.

[RAIL_LAW]:
- Packages: `StackExchange.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`
- Owns: Redis multiplexed transport and distributed cache layer
- Accept: `ConnectionMultiplexer` singleton, `IDatabase` per-operation, `RedisCacheOptions` for DI path
- Reject: per-operation multiplexer construction, raw TCP Redis protocol without the multiplexer
