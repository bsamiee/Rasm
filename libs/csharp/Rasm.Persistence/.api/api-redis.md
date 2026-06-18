# [RASM_PERSISTENCE_API_REDIS]

`StackExchange.Redis` supplies the Redis multiplexer, database, subscriber, server, and
configuration surfaces for cache and message-passing store profiles. `Microsoft.Extensions.Caching.StackExchangeRedis`
provides the `IDistributedCache`-backed `RedisCache` and its `RedisCacheOptions` for DI-wired
distributed caching over the same multiplexer.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: multiplexer and connection family
- rail: cache

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]        | [CAPABILITY]                                |
| :-----: | :----------------------- | :------------------- | :------------------------------------------ |
|   [1]   | `ConnectionMultiplexer`  | multiplexer root     | manages all server connections              |
|   [2]   | `IConnectionMultiplexer` | multiplexer contract | shared multiplexer capability               |
|   [3]   | `ConfigurationOptions`   | configuration        | endpoint, auth, timeout, proxy policy       |
|   [4]   | `IDatabase`              | database contract    | key-value, hash, list, set, geo, stream ops |
|   [5]   | `ISubscriber`            | pub/sub contract     | subscribe, publish, channel queues          |
|   [6]   | `IServer`                | server contract      | admin, scan, info, config operations        |
|   [7]   | `ChannelMessageQueue`    | message queue        | async-enumerable pub/sub queue              |
|   [8]   | `RedisValue`             | value struct         | polymorphic Redis value carrier             |
|   [9]   | `RedisKey`               | key struct           | Redis key with prefix support               |
|  [10]   | `RedisChannel`           | channel struct       | pub/sub channel with pattern support        |
|  [11]   | `HashEntry`              | hash entry           | field-value pair for hashes                 |
|  [12]   | `GeoEntry`               | geo entry            | member with longitude and latitude          |
|  [13]   | `GeoPosition`            | geo position         | longitude and latitude value                |
|  [14]   | `CommandFlags`           | flags enum           | fire-and-forget, prefer-replica, etc.       |
|  [15]   | `When`                   | condition enum       | `Always`, `Exists`, `NotExists`             |

[PUBLIC_TYPE_SCOPE]: caching family
- rail: cache

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [CAPABILITY]                                |
| :-----: | :------------------ | :---------------- | :------------------------------------------ |
|   [1]   | `RedisCache`        | distributed cache | `IDistributedCache` over Redis              |
|   [2]   | `RedisCacheOptions` | options           | connection string, options, prefix, factory |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: multiplexer lifecycle
- rail: cache

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `ConnectionMultiplexer.Connect(configuration)`         | static factory | synchronous connect from options or string |
|   [2]   | `ConnectionMultiplexer.ConnectAsync(configuration)`    | async factory  | async connect from options or string       |
|   [3]   | `IConnectionMultiplexer.GetDatabase(db?, asyncState?)` | access         | obtains `IDatabase` for a logical database |
|   [4]   | `IConnectionMultiplexer.GetSubscriber(asyncState?)`    | access         | obtains `ISubscriber`                      |
|   [5]   | `IConnectionMultiplexer.GetServer(endpoint)`           | access         | obtains `IServer` for an endpoint          |
|   [6]   | `IConnectionMultiplexer.GetEndPoints(configuredOnly?)` | discovery      | returns all configured endpoints           |
|   [7]   | `IConnectionMultiplexer.IsConnected`                   | property       | true when any server is reachable          |
|   [8]   | `ConfigurationOptions.Parse(configurationString)`      | factory        | parses connection string to options        |

[ENTRYPOINT_SCOPE]: database key-value and data structure operations
- rail: cache

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]      | [CAPABILITY]                     |
| :-----: | :---------------------------------------------- | :------------------ | :------------------------------- |
|   [1]   | `StringGet(key, flags?)`                        | string read         | gets value by key                |
|   [2]   | `StringSet(key, value, expiry?, when?, flags?)` | string write        | sets value with optional expiry  |
|   [3]   | `StringGetSetExpiry(key, expiry)`               | atomic update       | gets value and sets new expiry   |
|   [4]   | `HashGet(key, field, flags?)`                   | hash read           | gets one hash field              |
|   [5]   | `HashGetAll(key, flags?)`                       | hash read           | gets all `HashEntry[]`           |
|   [6]   | `HashSet(key, entries, flags?)`                 | hash write          | sets multiple `HashEntry[]`      |
|   [7]   | `ListLeftPush(key, values, flags?)`             | list write          | prepends values                  |
|   [8]   | `ListRightPop(key, flags?)`                     | list read           | pops from right                  |
|   [9]   | `SetAdd(key, values, flags?)`                   | set write           | adds members                     |
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
|   [1]   | `ISubscriber.Subscribe(channel, handler, flags?)`       | subscribe       | attaches message handler                 |
|   [2]   | `ISubscriber.SubscribeAsync(channel, handler?, flags?)` | async subscribe | async subscription with optional handler |
|   [3]   | `ISubscriber.Publish(channel, message, flags?)`         | publish         | publishes message to channel             |
|   [4]   | `ISubscriber.PublishAsync(channel, message, flags?)`    | async publish   | async publish                            |
|   [5]   | `ISubscriber.Unsubscribe(channel, handler?, flags?)`    | unsubscribe     | removes subscription                     |

[ENTRYPOINT_SCOPE]: DI-wired caching
- rail: cache

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------- | :--------------- | :-------------------------------------------- |
|   [1]   | `RedisCacheOptions.Configuration`                     | options property | connection string                             |
|   [2]   | `RedisCacheOptions.ConfigurationOptions`              | options property | `ConfigurationOptions` (preferred)            |
|   [3]   | `RedisCacheOptions.ConnectionMultiplexerFactory`      | options property | `Func<Task<IConnectionMultiplexer>>`          |
|   [4]   | `RedisCacheOptions.InstanceName`                      | options property | key prefix for cache partitioning             |
|   [5]   | `services.AddStackExchangeRedisCache(options => ...)` | DI extension     | registers `RedisCache` as `IDistributedCache` |

## [4]-[IMPLEMENTATION_LAW]

[REDIS_TOPOLOGY]:
- `ConnectionMultiplexer` is the long-lived shared root; it is `IDisposable` and `IAsyncDisposable`
- `IDatabase` is obtained per-call via `GetDatabase`; it is lightweight and not independently disposable
- `ConfigurationOptions.AbortOnConnectFail = false` is the default when parsed via `RedisCacheOptions`
- `IConnectionMultiplexer` events: `ConnectionFailed`, `ConnectionRestored`, `ErrorMessage`, `ConfigurationChanged`, `HashSlotMoved`
- `When` enum: `Always`, `Exists`, `NotExists` used for conditional SET operations
- `CommandFlags.FireAndForget` skips response waiting; `CommandFlags.PreferReplica` routes reads to replicas
- `RedisValue` is a stack-allocated struct covering `null`, `int64`, `uint64`, `double`, `string`, `byte[]`, and `ReadOnlyMemory<byte>`

[LOCAL_ADMISSION]:
- The multiplexer is a singleton; call `GetDatabase` at the operation boundary, not at composition root.
- `RedisCacheOptions.ConfigurationOptions` takes precedence over `RedisCacheOptions.Configuration` string.
- `InstanceName` prefixes all cache keys; omitting it shares the keyspace across all consumers.
- Pub/sub uses `ChannelMessageQueue` (returned by `SubscribeAsync`) for backpressure-safe async enumeration.

[RAIL_LAW]:
- Packages: `StackExchange.Redis`, `Microsoft.Extensions.Caching.StackExchangeRedis`
- Owns: Redis multiplexed transport and distributed cache layer
- Accept: `ConnectionMultiplexer` singleton, `IDatabase` per-operation, `RedisCacheOptions` for DI path
- Reject: per-operation multiplexer construction, raw TCP Redis protocol without the multiplexer
