# [RASM_PERSISTENCE_API_REDIS]

`StackExchange.Redis` supplies the Redis multiplexer, database, subscriber, server, and
configuration surfaces for cache and message-passing store profiles. `Microsoft.Extensions.Caching.StackExchangeRedis`
provides the `IDistributedCache`-backed `RedisCache` and its `RedisCacheOptions` for DI-wired
distributed caching over the same multiplexer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `StackExchange.Redis`
- package: `StackExchange.Redis`
- assembly: `StackExchange.Redis`
- namespace: `StackExchange.Redis`
- asset: runtime library
- rail: cache

[PACKAGE_SURFACE]: `Microsoft.Extensions.Caching.StackExchangeRedis`
- package: `Microsoft.Extensions.Caching.StackExchangeRedis`
- assembly: `Microsoft.Extensions.Caching.StackExchangeRedis`
- namespace: `Microsoft.Extensions.Caching.StackExchangeRedis`
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
|  [16]   | `RedisProtocol`          | protocol enum        | `Resp2`, `Resp3` wire-protocol selector     |
|  [17]   | `LoadedLuaScript`        | prepared script      | SHA1-cached `EVALSHA` server-side script    |
|  [18]   | `LuaScript`              | parsed script        | named-parameter Lua with `Prepare`/`Load`   |
|  [19]   | `ChannelMessage`         | message value        | one `(Channel, Message)` queue item         |

[PUBLIC_TYPE_SCOPE]: caching family
- rail: cache

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [CAPABILITY]                                |
| :-----: | :------------------ | :---------------- | :------------------------------------------ |
|  [01]   | `RedisCache`        | distributed cache | `IDistributedCache` over Redis              |
|  [02]   | `RedisCacheOptions` | options           | connection string, options, prefix, factory |

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
|  [08]   | `ConfigurationOptions.Parse(configurationString)`      | factory        | parses connection string to options        |

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
|  [13]   | `Execute(command, args?)`                       | raw command         | executes arbitrary raw command   |
|  [14]   | `CreateBatch(asyncState?)`                      | batch factory       | creates pipelined batch          |
|  [15]   | `CreateTransaction(asyncState?)`                | transaction factory | creates `MULTI/EXEC` transaction |

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
|  [03]   | `ISubscriber.SubscribeAsync(RedisChannel.Pattern("__keyevent@*__:*"))` | async subscribe | returns the `ChannelMessageQueue` of keyevent transitions                                                                                               |
|  [04]   | `ISubscriber.SubscribeAsync(RedisChannel.Pattern("__keyspace@*__:*"))` | async subscribe | the per-key keyspace-transition queue                                                                                                                   |
|  [05]   | `ChannelMessageQueue : IAsyncEnumerable<ChannelMessage>`               | async enumerate | the queue IS the async stream — `await foreach (var m in queue.WithCancellation(token))`; also `ReadAsync(token)`/`TryRead(out m)`/`OnMessage(handler)` |

[ENTRYPOINT_SCOPE]: Lua scripting and server functions
- rail: cache

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `LuaScript.Prepare(script)`                              | static factory | parses `@name` parameter tokens into a `LuaScript`                          |
|  [02]   | `LuaScript.Load(IServer)`                                | server load    | `SCRIPT LOAD` returning a SHA1-cached `LoadedLuaScript`                     |
|  [03]   | `IDatabase.ScriptEvaluate(LoadedLuaScript, parameters)`  | atomic eval    | `EVALSHA` single-flight; one round-trip atomic script                       |
|  [04]   | `IDatabase.ScriptEvaluate(script, keys, values, flags?)` | atomic eval    | inline `EVAL` over explicit `RedisKey[]`/`RedisValue[]`                     |
|  [05]   | `IServer.ScriptLoad(script)`                             | server load    | returns the script SHA1 `byte[]` for later `EVALSHA`                        |
|  [06]   | `IDatabase.Execute("FUNCTION", "LOAD", code)`            | raw command    | Redis 7 `FUNCTION LOAD` library register (no typed member; rides `Execute`) |
|  [07]   | `IDatabase.Execute("FCALL", name, …)`                    | raw command    | invokes a loaded function (no typed member; rides `Execute`)                |

## [04]-[IMPLEMENTATION_LAW]

[REDIS_TOPOLOGY]:
- `ConnectionMultiplexer` is the long-lived shared root; it is `IDisposable` and `IAsyncDisposable`
- `IDatabase` is obtained per-call via `GetDatabase`; it is lightweight and not independently disposable
- `ConfigurationOptions.AbortOnConnectFail = false` is the default when parsed via `RedisCacheOptions`
- `IConnectionMultiplexer` events: `ConnectionFailed`, `ConnectionRestored`, `ErrorMessage`, `ConfigurationChanged`, `HashSlotMoved`
- `When` enum: `Always`, `Exists`, `NotExists` used for conditional SET operations
- `CommandFlags.FireAndForget` skips response waiting; `CommandFlags.PreferReplica` routes reads to replicas
- `RedisValue` is a stack-allocated struct covering `null`, `int64`, `uint64`, `double`, `string`, `byte[]`, and `ReadOnlyMemory<byte>`

[RESP3_CLIENT_SIDE_CACHING]:
- `ConfigurationOptions.Protocol = RedisProtocol.Resp3` negotiates RESP3 at `HELLO`; the negotiated value reads back per-server on `IServer.Protocol` (`ConnectionMultiplexer` carries no `Protocol` member — obtain a server via `GetServer` to read the active protocol).
- Server-assisted client-side caching is the broadcast `__redis__:invalidate` push channel — subscribe `ISubscriber` to `RedisChannel.Literal("__redis__:invalidate")` and the server pushes the invalidated key (or `RedisValue.Null` for a flush) so an L1 entry drops on the source key change rather than at TTL.
- `CLIENT TRACKING ON BCAST PREFIX <p>` (issued through `IDatabase.Execute` — no typed member) arms the tracking; the broadcast mode keys on a prefix set so the connection receives one invalidation per changed key under those prefixes, never a per-key opt-in round trip.

[KEYSPACE_NOTIFICATION]:
- `notify-keyspace-events` is OFF by default; `IServer.ConfigSet("notify-keyspace-events", "KEA")` arms keyspace (`K`), keyevent (`E`), and all classes (`A`); the flag is connection-instance runtime state, never persisted by a Rasm process.
- `__keyevent@<db>__:<event>` carries the affected key as the message and `__keyspace@<db>__:<key>` carries the event name; subscribe the pattern form through `ISubscriber.SubscribeAsync` and drain the returned `ChannelMessageQueue` — the queue IS `IAsyncEnumerable<ChannelMessage>`, so `await foreach (var m in queue.WithCancellation(token))` is the backpressure-safe drain, never a non-existent `ReadAllAsync` member.
- Keyspace notifications are best-effort fire-and-forget — a disconnected subscriber misses events, so the stream REFINES (feeds a push path to) a durable cursor replay, never replaces it.

[LUA_AND_FUNCTIONS]:
- `LuaScript.Prepare(text)` parses `@name` tokens; `LuaScript.Load(IServer)` returns a `LoadedLuaScript` whose `EVALSHA` is a single round-trip atomic script — the stampede single-flight and the writer-lease fence are one atomic server-side script, never a managed compare-loop.
- `IServer.ScriptLoad` returns the raw SHA1 for an explicit `EVALSHA`; `IDatabase.ScriptEvaluate` accepts either the `LoadedLuaScript`, the inline text, or the SHA with explicit `RedisKey[]`/`RedisValue[]`.
- Redis 7 `FUNCTION LOAD`/`FCALL` have no typed member; they ride `IDatabase.Execute("FUNCTION", "LOAD", code)` and `Execute("FCALL", name, …)`. Pin the typed `ScriptEvaluate` spelling first and the `Execute` escape hatch only where a typed member is absent.

[LOCAL_ADMISSION]:
- The multiplexer is a singleton; call `GetDatabase` at the operation boundary, not at composition root.
- `RedisCacheOptions.ConfigurationOptions` takes precedence over `RedisCacheOptions.Configuration` string.
- `InstanceName` prefixes all cache keys; omitting it shares the keyspace across all consumers.
- Pub/sub uses `ChannelMessageQueue` (returned by `SubscribeAsync`) for backpressure-safe async enumeration.
- The Lua scripts, the keyspace subscription, and the RESP3 tracking arming are connection-instance state on the multiplexer; absent Redis the TTL-bounded baseline is bit-identical, so the live-fabric path is additive capability, never a new dependency.

[RAIL_LAW]:
- Packages: `StackExchange.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`
- Owns: Redis multiplexed transport and distributed cache layer
- Accept: `ConnectionMultiplexer` singleton, `IDatabase` per-operation, `RedisCacheOptions` for DI path
- Reject: per-operation multiplexer construction, raw TCP Redis protocol without the multiplexer
