# [API_CATALOGUE] ioredis

`ioredis` is the Redis driver for the node interior — `Redis` (standalone/Sentinel), `Cluster`, `Pipeline`, `ScanStream`, `Command`, `Script`, and the `Commander`→`RedisCommander<Context>` command surface whose every method returns the context-discriminated `Result<Reply, Context>` (a `Promise` in default mode, a `ChainableCommander` in pipeline mode). It carries the full command vocabulary, auto-pipelining, pub/sub, Lua scripting (`defineCommand`), scan streaming, transactions, Sentinel failover, and Cluster topology. In `services` it is never a bare cache client: it is the raw Redis transport that a thin `Effect.acquireRelease` layer turns into a `Context.Tag`, then composes as the backing store for the Effect substrate rails — `@effect/experimental` `BackingPersistence`/`RateLimiter`/`Reactivity`, a `@effect/platform` `KeyValueStore`, and (available, not the wired default) a `@effect/cluster` `MessageStorage.makeEncoded` custom driver — with `ScanStream` and subscriber events bridged into Effect `Stream`s. Raw socket framing or a hand-rolled RESP protocol is the named defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ioredis`
- package: `ioredis` (5.11.1, MIT, © Zihua Li); reflected from `node_modules/.pnpm/ioredis@5.11.1/.../built/*.d.ts`
- namespace: `ioredis`; default export `Redis` (`import Redis from "ioredis"` or `import { Redis, Cluster } from "ioredis"`); `Redis.Cluster`/`Redis.Command` also hang off the default
- asset: runtime library; node-tier — `node:net`/`node:tls` sockets, `events` `EventEmitter`, `denque` queue
- rail: messaging / redis-driver (the Redis transport beneath the `execution/backplane` + `@effect/experimental` substrate; README `[RPC_MESSAGING]`)
- tier: `node` (`engines.node >=12.22.0`); pinned first-class in the workspace catalog at `5.11.1` (`# store`), the direct-dependency `catalog:` row every owner references before importing
- ABI: a subscriber-mode connection cannot issue normal commands — a dedicated `duplicate()` connection carries pub/sub; auto-pipelining batches every command issued in one microtask tick

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client + command classes
- rail: messaging

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]      | [CAPABILITY]                                                     |
| :-----: | :----------- | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `Redis`      | class              | standalone/Sentinel connection — `extends Commander implements DataHandledable`, also `EventEmitter` + `Transaction` |
|  [02]   | `Cluster`    | class              | Redis Cluster client — `extends Commander`, slot-routed, `nodes(role)` fan-out |
|  [03]   | `Commander`  | class (`<Context>`) | the shared command base both `Redis` and `Cluster` extend; `extends RedisCommander<Context>` |
|  [04]   | `Pipeline`   | class              | `extends Commander<{ type: "pipeline" }>` — accumulate then `exec()` all replies together; `.length` |
|  [05]   | `ScanStream` | class (`Readable`) | `SCAN`/`SSCAN`/`HSCAN`/`ZSCAN` cursor loop as a Node `Readable`; `.close()` |
|  [06]   | `Command`    | class              | raw command envelope (`Redis.Command`) for `sendCommand` |
|  [07]   | `Script`     | class              | cached Lua script (`lua`, `numberOfKeys`, `readOnly`) behind `defineCommand` |

[PUBLIC_TYPE_SCOPE]: the context-discriminated command algebra — the polymorphic core
- rail: messaging

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `RedisCommander<Context>`   | interface     | the generated command surface — 200+ typed methods (`get`/`set`/`hset`/`xadd`/`zadd`/`eval`/`fcall`/…), each returning `Result<Reply, Context>`; one interface owns the entire command space |
|  [02]   | `Result<Reply, Context>`    | conditional type | the discriminant — `Context.type extends "pipeline"` → `ChainableCommander` (fluent chain); else → `Promise<Reply>`. One command spelling serves both the async and the pipeline modality |
|  [03]   | `ClientContext`             | type          | `{ type: "default" \| "pipeline" \| … }` — the mode tag `Result` branches on |
|  [04]   | `ChainableCommander`        | interface     | `extends RedisCommander<{ type: "pipeline" }>` + `exec()`/`length` — the fluent pipeline/multi return |
|  [05]   | `RedisKey` / `RedisValue`   | type          | `string \| Buffer` / `string \| Buffer \| number \| (string\|Buffer\|number)[]` — the accepted key/arg domain |
|  [06]   | `Callback<T>`               | type          | `(err: Error \| null, result?: T) => void` — the node-style callback every command optionally accepts (prefer the Promise/`Effect` return) |

[PUBLIC_TYPE_SCOPE]: connection + topology options
- rail: messaging

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `RedisOptions`                  | interface     | standalone config — `lazyConnect`/`retryStrategy`/`reconnectOnError`/`maxRetriesPerRequest`/`enableAutoPipelining`/`enableOfflineQueue`/`enableReadyCheck`/`commandTimeout`/`connectTimeout`/`keepAlive`/`keyPrefix`/`db`/`username`/`password`/`tls`/`connectionName`/`autoResubscribe`/`autoResendUnfulfilledCommands`/`stringNumbers`/`noDelay` |
|  [02]   | `CommonRedisOptions`            | interface     | the shared retry/auth/timeout base of `RedisOptions` and the Sentinel/standalone connector options |
|  [03]   | `StandaloneConnectionOptions` / `SentinelConnectionOptions` | interface | TCP/TLS connector vs Sentinel connector (`sentinels`, `name`, `role`, failover) options |
|  [04]   | `ClusterOptions`                | interface     | cluster topology — `scaleReads` (`NodeRole \| fn`)/`natMap`/`maxRedirections`/`retryDelayOnFailover`/`clusterRetryStrategy`/`dnsLookup`/`slotsRefreshTimeout`/`redisOptions`/`enableReadyCheck`/`enableOfflineQueue`/`enableAutoPipelining`/`lazyConnect` |
|  [05]   | `ClusterNode`                   | type          | `string \| number \| { host?: string; port?: number }` — a startup-node spec |
|  [06]   | `RetryStrategy`                 | type          | `(times: number) => number \| void \| null` — reconnect backoff; `null`/`void` stop retrying |
|  [07]   | `ReconnectOnError`              | type          | `(err: Error) => boolean \| 1 \| 2` — `2` reconnects *and* resends the failing command |
|  [08]   | `NodeRole`                      | enum          | `"master" \| "slave"` — cluster node role + `scaleReads` discriminant |
|  [09]   | `SentinelAddress` / `NatMap` / `DNSLookupFunction` / `DNSResolveSrvFunction` | type | Sentinel address + NAT/DNS remap hooks for containerized topologies |
|  [10]   | `ScanStreamOptions`            | interface     | `{ match?; count?; type?; noValues? }` — cursor stream tuning |

[PUBLIC_TYPE_SCOPE]: tracing + module exports
- rail: messaging

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `CommandTraceContext` / `BatchOperationContext` / `ConnectTraceContext` | type | the built-in tracing seams (command / pipeline batch / connect) to bridge onto `Effect.withSpan` |
|  [02]   | `ReplyError`                    | value         | the error class Redis command failures reject with (the `catch` target for `Effect.tryPromise`) |
|  [03]   | `print`                         | function      | `(err, reply?) => void` debug reply printer |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction — the argument-shape axis
- rail: messaging

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `new Redis()` / `(port)` / `(port, host)` / `(port, host, options)` / `(path)` / `(path, options)` / `(options)` | constructor | one client, seven overloads discriminating on arg shape (port/host/unix-path/options) — prefer `new Redis(RedisOptions)` for a layer |
|  [02]   | `Redis.createClient(...args)`               | static factory | node-redis-compatible alias over the same constructor   |
|  [03]   | `new Cluster(startupNodes, options?)`       | constructor    | multi-node cluster client from `ClusterNode[]`          |
|  [04]   | `redis.duplicate(override?)`                | factory        | a second client with the same options — the sanctioned subscriber/blocking connection |

[ENTRYPOINT_SCOPE]: lifecycle, mode, events
- rail: messaging

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `redis.connect(cb?)` → `Promise<void>`      | lifecycle      | explicit connect (needed with `lazyConnect: true`); resolves on `ready` |
|  [02]   | `redis.quit(cb?)` → `Promise<"OK">` / `redis.disconnect(reconnect?)` / `redis.end()` | lifecycle | graceful drain-then-close vs immediate close; `end` deprecated |
|  [03]   | `redis.status`                              | property       | `"wait" \| "connecting" \| "connect" \| "ready" \| "reconnecting" \| "close" \| "end"` |
|  [04]   | `redis.mode`                                | property       | `"normal" \| "subscriber" \| "monitor"` — command eligibility gate |
|  [05]   | `redis.monitor(cb?)` → `Promise<Redis>`     | diagnostic     | a new connection emitting a `monitor` event per server command |
|  [06]   | `redis.on(event, cb)`                       | events         | lifecycle (`ready`/`connect`/`close`/`reconnecting`/`end`/`error`) + pub/sub (`message`/`messageBuffer`/`pmessage`/`pmessageBuffer`) |

[ENTRYPOINT_SCOPE]: the command surface, pipeline, transaction, scripting
- rail: messaging

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `redis.<command>(...args)` (e.g. `get`/`set`/`hset`/`zadd`/`xadd`/`eval`) | command | the typed `RedisCommander` surface; returns `Promise<Reply>` in default mode |
|  [02]   | `redis.call(command, args?, cb?)` / `redis.callBuffer(...)` | escape hatch | send an arbitrary command name (new/unsupported commands) with string vs `Buffer` replies |
|  [03]   | `redis.pipeline(commands?)` → `ChainableCommander`; `.exec()` | pipeline | queue commands, one round-trip, ordered `[err, reply][]` |
|  [04]   | `redis.multi()` → `ChainableCommander`; `redis.multi({ pipeline: false })` → `Promise<"OK">` | transaction | `MULTI`/`EXEC` atomic block; `{ pipeline: false }` selects the non-pipelined form |
|  [05]   | `redis.defineCommand(name, { lua, numberOfKeys?, readOnly? })` | scripting | register a Lua script as a typed method on the client (module-augment `RedisCommander<Context>` for the type) |
|  [06]   | `redis.getBuiltinCommands()` / `createBuiltinCommand(name)` / `addBuiltinCommand(name)` | scripting | enumerate/synthesize builtin command methods |
|  [07]   | `redis.sendCommand(command: Command, stream?)` | raw | the lowest-level command dispatch (internal; prefer `call`) |

[ENTRYPOINT_SCOPE]: scan streaming + pub/sub — the two Stream sources
- rail: messaging

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `redis.scanStream(opts?)` / `scanBufferStream` / `sscanStream(key,…)` / `sscanBufferStream` / `hscanStream(key,…)` / `hscanBufferStream` / `zscanStream(key,…)` / `zscanBufferStream` | scan | one `ScanStream` (`Readable`) per `(command × key-scope × string/buffer)` cell — the cursor loop as a stream, never a manual `SCAN` loop |
|  [02]   | `redis.subscribe(...channels, cb?)` / `psubscribe(...patterns)` / `ssubscribe(...shardChannels)` | pub/sub in | subscribe on a subscriber-mode (`duplicate()`) connection; replies via `message`/`pmessage` events |
|  [03]   | `redis.unsubscribe(...)` / `punsubscribe(...)` / `sunsubscribe(...)` | pub/sub out | mirror unsubscribe family |
|  [04]   | `redis.publish(channel, message)` / `spublish(...)` | pub/sub out | publish on a normal-mode connection |

[ENTRYPOINT_SCOPE]: Cluster surface
- rail: messaging

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `cluster.nodes(role?)` → `Redis[]`          | topology       | every master/slave node connection (fan a command across the cluster) |
|  [02]   | `cluster.connect()` / `disconnect(reconnect?)` / `quit(cb?)` / `duplicate(nodes?, opts?)` | lifecycle | cluster-wide lifecycle mirror |
|  [03]   | `cluster.refreshSlotsCache(cb?)`            | topology       | force a slot-map refresh (post-resharding) |
|  [04]   | `cluster.scanStream/sscanStream/hscanStream/zscanStream(...)` | scan | slot-aware scan streams |

## [04]-[IMPLEMENTATION_LAW]

[IOREDIS_TOPOLOGY]:
- `Redis` and `Cluster` both `extend Commander`, which `extends RedisCommander<Context>` — they share one typed command surface; `Pipeline` is `Commander<{ type: "pipeline" }>`, so the same command methods on it return `ChainableCommander` for chaining.
- `Result<Reply, Context>` is the sole modality discriminant: default context → `Promise<Reply>`, pipeline context → `ChainableCommander`. There is no `getMany`/`getBuffer` proliferation — a `Buffer` reply is the `*Buffer` command variant, a pipelined command is the same spelling under pipeline context.
- the scan family is a `(command, key-scope, encoding)` matrix over one `ScanStream` (`Readable`): `scan`/`sscan`/`hscan`/`zscan` × keyed/unkeyed × string/`Buffer`. `ScanStream.close()` aborts before `_redisDrained`.
- auto-pipelining (`enableAutoPipelining`, default on) batches every command issued in one microtask tick; a subscriber-mode connection rejects normal commands, so pub/sub lives on a `duplicate()` connection.
- `defineCommand` compiles a Lua script into a cached `Script` and installs it as a first-class method — one round-trip, server-atomic; typed by module-augmenting `RedisCommander<Context>`.

[STACKS_WITH]:
- `effect` (`../../.api/effect.md`): the connection is a scoped resource — `Effect.acquireRelease(Effect.sync(() => new Redis({ ...opts, lazyConnect: true })).pipe(Effect.tap((r) => Effect.tryPromise(() => r.connect()))), (r) => Effect.promise(() => r.quit()))` exposed as a `Context.Tag<RedisClient>`/`Effect.Service`. Every command is `Effect.tryPromise({ try: () => redis.get(k), catch: (e) => new RedisFault({ cause: e as ReplyError }) })`; retries ride `Effect.retry(Schedule)`, spans ride `Effect.withSpan` — never the driver's own `retryStrategy` for domain-level retry.
- `effect/Stream` + `@effect/platform-node` `NodeStream` (`effect-platform-node.md`): `ScanStream` (a Node `Readable`) becomes `NodeStream.fromReadable(() => redis.scanStream({ match, count }))` → a backpressured `Stream<Buffer>` for key iteration; the subscriber `message`/`pmessage` events become `Stream.async`/`Stream.asyncPush` emitters, so pub/sub is an Effect `Stream` consumers fold, not an event handler.
- `@effect/experimental` `Persistence` (`effect-experimental.md`): a Redis `BackingPersistence` is the canonical Effect integration — implement `BackingPersistence.make: (storeId) => Effect<BackingPersistenceStore, never, Scope>` over `redis.get`/`set`/`del`/`hgetall` and provide it as a `Layer`; that one Tag then backs `@effect/ai` chat persistence, `@effect/rpc` request dedup, and any `ResultPersistence` cache with Redis TTL semantics `KeyValueStore` cannot express.
- `@effect/experimental` `RateLimiter` + `Reactivity` (`effect-experimental.md`): the distributed `RateLimiter` store (`messaging/quota#QUOTA`) is a `defineCommand` token-bucket Lua over Redis — one atomic round-trip per acquire, shared across every runner; `Reactivity` cross-node invalidation fans through Redis pub/sub (a `publish` on mutate, a `psubscribe` `Stream` driving the key-scoped re-run).
- `@effect/platform` `KeyValueStore` (`../../.api/effect-platform.md`): a Redis `KeyValueStore.KeyValueStore` layer over `redis.get`/`set`/`del` gives every owner the ephemeral cross-node cache lane beside `layerMemory`/`layerFileSystem`, composable with `.prefix(k)` and `.layerSchema(schema)`.
- `@effect/cluster` `MessageStorage`/`RunnerStorage` (`effect-cluster.md`): the catalog names `redis` as a `MessageStorage.makeEncoded(encoded)` custom-driver path — a Redis Streams-backed message store is available for the cluster backplane. The wired `execution/backplane` default is SQL (`messageStore: Layer<MessageStorage, …, SqlClient>`); the Redis driver is the alternative when a lighter, non-durable transport is acceptable, never a silent swap of the durable SQL default.

[LOCAL_ADMISSION]:
- Always `lazyConnect: true` inside an Effect `Layer` so the TCP handshake is the acquire step, not a constructor side effect — connection timing belongs to the scope.
- `retryStrategy` returning `null`/`void` stops driver reconnects; keep it for transport-level reconnect only and let `Effect.retry(Schedule)` own domain retry so the policy is one composable value.
- `reconnectOnError` returning `2` reconnects and resends the failing command — the READONLY-on-failover recovery for Sentinel/Cluster; `1` reconnects without resend.
- Use `duplicate()` for every subscriber and blocking (`BLPOP`/`XREAD BLOCK`) connection; a subscriber-mode connection cannot run normal commands and a long block starves the shared client.
- Prefer `quit()` (drain pending replies) over `disconnect()` (immediate) on graceful shutdown; the `acquireRelease` finalizer owns it.
- Express any read-modify-write invariant (rate-limit token, idempotency claim, outbox lock) as one `defineCommand` Lua script — server-atomic, one round-trip — never a `get`-then-`set` race across the network.

[RAIL_LAW]:
- Package: `ioredis`
- Owns: the Redis transport — standalone/Sentinel/Cluster connection, the context-discriminated command surface, pipelining/transactions, pub/sub, Lua scripting, and scan streaming — behind a scoped `Context.Tag`, as the backing driver for the Effect `Persistence`/`RateLimiter`/`Reactivity`/`KeyValueStore` rails
- Accept: `RedisOptions`/`ClusterOptions` (with `lazyConnect`), `RetryStrategy`/`ReconnectOnError` for transport reconnect, `defineCommand` Lua for atomic ops, `RedisKey`/`RedisValue` command args
- Reject: a raw `new Redis()` in domain code outside the layer, a bare command `Promise` un-lifted from `Effect.tryPromise`, a manual `SCAN` cursor loop where `scanStream` is the `Stream` source, `get`-then-`set` where a `defineCommand` script is atomic, hand-rolled RESP/socket framing, and a silent Redis-for-SQL swap of the wired durable cluster storage
