# [API_CATALOGUE] ioredis

`ioredis` supplies `Redis` (standalone), `Cluster` (Redis Cluster), `Pipeline`, `ScanStream`, and their option interfaces for connecting to Redis from Node.js services, with built-in auto-pipelining, Sentinel support, and typed command results.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ioredis`
- package: `ioredis`
- namespace: `ioredis`
- asset: runtime library
- rail: cache / data

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client classes
- rail: cache / data

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [RAIL]                         |
| :-----: | :----------- | :--------------- | :----------------------------- |
|   [1]   | `Redis`      | class            | standalone Redis connection    |
|   [2]   | `Cluster`    | class            | Redis Cluster client           |
|   [3]   | `Pipeline`   | class            | command pipeline / transaction |
|   [4]   | `ScanStream` | class (Readable) | streaming key scan             |

[PUBLIC_TYPE_SCOPE]: connection option types
- rail: cache / data

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                      |
| :-----: | :---------------------------- | :------------ | :------------------------------------------ |
|   [1]   | `RedisOptions`                | interface     | standalone connection config                |
|   [2]   | `CommonRedisOptions`          | interface     | shared timeout, auth, retry config          |
|   [3]   | `StandaloneConnectionOptions` | interface     | TCP/TLS connector options                   |
|   [4]   | `SentinelConnectionOptions`   | interface     | Sentinel connector options                  |
|   [5]   | `ClusterOptions`              | interface     | cluster topology and failover config        |
|   [6]   | `ClusterNode`                 | type          | `string \| number \| { host?, port? }`      |
|   [7]   | `RetryStrategy`               | type          | `(times: number) => number \| void \| null` |
|   [8]   | `ReconnectOnError`            | type          | `(err: Error) => boolean \| 1 \| 2`         |

[PUBLIC_TYPE_SCOPE]: command and streaming types
- rail: cache / data

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                            |
| :-----: | :------------------- | :------------ | :-------------------------------- |
|   [1]   | `RedisKey`           | type          | acceptable Redis key type         |
|   [2]   | `RedisValue`         | type          | acceptable Redis value type       |
|   [3]   | `ChainableCommander` | interface     | fluent pipeline command interface |
|   [4]   | `Command`            | class         | raw command envelope              |
|   [5]   | `ScanStreamOptions`  | interface     | `SCAN`/`HSCAN` stream options     |
|   [6]   | `Callback`           | type          | Node.js-style `(err, result)` cb  |
|   [7]   | `NodeRole`           | enum          | cluster node role discriminant    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Redis construction
- rail: cache / data

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `new Redis()`                    | constructor    | connect with defaults (port 6379) |
|   [2]   | `new Redis(port)`                | constructor    | connect to port                   |
|   [3]   | `new Redis(port, host)`          | constructor    | connect to host:port              |
|   [4]   | `new Redis(options)`             | constructor    | connect with `RedisOptions`       |
|   [5]   | `new Redis(port, host, options)` | constructor    | explicit host/port/options        |
|   [6]   | `new Redis(path, options?)`      | constructor    | Unix socket connection            |
|   [7]   | `Redis.createClient(...args)`    | static factory | node-redis compatibility alias    |

[ENTRYPOINT_SCOPE]: Redis lifecycle and mode
- rail: cache / data

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                                                                                 |
| :-----: | :----------------------------- | :------------- | :------------------------------------------------------------------------------------- |
|   [1]   | `redis.connect(callback?)`     | lifecycle      | explicit connect; returns `Promise<void>`                                              |
|   [2]   | `redis.disconnect(reconnect?)` | lifecycle      | immediate close                                                                        |
|   [3]   | `redis.duplicate(override?)`   | factory        | new client with same options                                                           |
|   [4]   | `redis.status`                 | property       | `"wait" \| "reconnecting" \| "connecting" \| "connect" \| "ready" \| "close" \| "end"` |
|   [5]   | `redis.mode`                   | property       | `"normal" \| "subscriber" \| "monitor"`                                                |

[ENTRYPOINT_SCOPE]: Cluster construction
- rail: cache / data

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY] | [RAIL]                    |
| :-----: | :------------------------------------ | :------------- | :------------------------ |
|   [1]   | `new Cluster(startupNodes, options?)` | constructor    | multi-node cluster client |

## [4]-[IMPLEMENTATION_LAW]

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
