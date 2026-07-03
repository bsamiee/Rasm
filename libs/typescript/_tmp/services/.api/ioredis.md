# [API_CATALOGUE] ioredis

`ioredis` supplies `Redis` (standalone), `Cluster` (Redis Cluster), `Pipeline`, `ScanStream`, and their option interfaces for connecting to Redis from Node.js services, with built-in auto-pipelining, Sentinel support, and typed command results.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ioredis`
- package: `ioredis`
- namespace: `ioredis`
- asset: runtime library
- rail: cache / data

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client classes
- rail: cache / data

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [RAIL]                         |
| :-----: | :----------- | :--------------- | :----------------------------- |
|  [01]   | `Redis`      | class            | standalone Redis connection    |
|  [02]   | `Cluster`    | class            | Redis Cluster client           |
|  [03]   | `Pipeline`   | class            | command pipeline / transaction |
|  [04]   | `ScanStream` | class (Readable) | streaming key scan             |

[PUBLIC_TYPE_SCOPE]: connection option types
- rail: cache / data

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                      |
| :-----: | :---------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `RedisOptions`                | interface     | standalone connection config                |
|  [02]   | `CommonRedisOptions`          | interface     | shared timeout, auth, retry config          |
|  [03]   | `StandaloneConnectionOptions` | interface     | TCP/TLS connector options                   |
|  [04]   | `SentinelConnectionOptions`   | interface     | Sentinel connector options                  |
|  [05]   | `ClusterOptions`              | interface     | cluster topology and failover config        |
|  [06]   | `ClusterNode`                 | type          | `string \| number \| { host?, port? }`      |
|  [07]   | `RetryStrategy`               | type          | `(times: number) => number \| void \| null` |
|  [08]   | `ReconnectOnError`            | type          | `(err: Error) => boolean \| 1 \| 2`         |

[PUBLIC_TYPE_SCOPE]: command and streaming types
- rail: cache / data

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :------------------- | :------------ | :-------------------------------- |
|  [01]   | `RedisKey`           | type          | acceptable Redis key type         |
|  [02]   | `RedisValue`         | type          | acceptable Redis value type       |
|  [03]   | `ChainableCommander` | interface     | fluent pipeline command interface |
|  [04]   | `Command`            | class         | raw command envelope              |
|  [05]   | `ScanStreamOptions`  | interface     | `SCAN`/`HSCAN` stream options     |
|  [06]   | `Callback`           | type          | Node.js-style `(err, result)` cb  |
|  [07]   | `NodeRole`           | enum          | cluster node role discriminant    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Redis construction
- rail: cache / data

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `new Redis()`                    | constructor    | connect with defaults (port 6379) |
|  [02]   | `new Redis(port)`                | constructor    | connect to port                   |
|  [03]   | `new Redis(port, host)`          | constructor    | connect to host:port              |
|  [04]   | `new Redis(options)`             | constructor    | connect with `RedisOptions`       |
|  [05]   | `new Redis(port, host, options)` | constructor    | explicit host/port/options        |
|  [06]   | `new Redis(path, options?)`      | constructor    | Unix socket connection            |
|  [07]   | `Redis.createClient(...args)`    | static factory | node-redis compatibility alias    |

[ENTRYPOINT_SCOPE]: Redis lifecycle and mode
- rail: cache / data

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                                                                                 |
| :-----: | :----------------------------- | :------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `redis.connect(callback?)`     | lifecycle      | explicit connect; returns `Promise<void>`                                              |
|  [02]   | `redis.disconnect(reconnect?)` | lifecycle      | immediate close                                                                        |
|  [03]   | `redis.duplicate(override?)`   | factory        | new client with same options                                                           |
|  [04]   | `redis.status`                 | property       | `"wait" \| "reconnecting" \| "connecting" \| "connect" \| "ready" \| "close" \| "end"` |
|  [05]   | `redis.mode`                   | property       | `"normal" \| "subscriber" \| "monitor"`                                                |

[ENTRYPOINT_SCOPE]: Cluster construction
- rail: cache / data

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :------------------------------------ | :------------- | :------------------------ |
|  [01]   | `new Cluster(startupNodes, options?)` | constructor    | multi-node cluster client |

## [04]-[IMPLEMENTATION_LAW]

[IOREDIS_TOPOLOGY]:
- namespace: `ioredis`; `Redis` and `Cluster` are both `Commander` subclasses carrying the same typed command surface
- `Pipeline` accumulates commands and resolves all replies together via `exec()`; `ChainableCommander` is the fluent type overlay
- `ScanStream` wraps the `SCAN`/`HSCAN`/`SSCAN`/`ZSCAN` cursor loop as a Node.js `Readable` stream
- Auto-pipelining is enabled by default; commands issued in a single microtask queue tick are batched automatically

[LOCAL_ADMISSION]:
- `RedisOptions.lazyConnect: true` defers the TCP handshake until the first command; required when wiring inside Effect layers to control connection timing.
- `retryStrategy` must return `null` to stop retrying; returning `undefined` or `void` also stops retries.
- `reconnectOnError` returning `2` reconnects and resends the failing command — use for READONLY errors in sentinel failover scenarios.
- `ScanStream` requires `close()` to abort iteration before `_redisDrained` is set.

[RAIL_LAW]:
- Package: `ioredis`
- Owns: Redis client — standalone, cluster, pipelining, pub/sub, scripting, and scan streaming
- Accept: `RedisOptions`, `ClusterOptions`, `RetryStrategy`, `ReconnectOnError`
- Reject: hand-rolled Redis protocol framing or raw-socket Redis communication
