# [@effect/experimental] — EventLog local-first sync + durable-execution overlay lanes, never the system of record

`@effect/experimental` is the overlay-lane family: `EventLog` local-first event-sourcing with an append-only journal, end-to-end-encrypted WebSocket sync, and a mountable sync server; serializable durable `Machine` actors with snapshot/restore; `KeyValueStore`-backed `Persistence`/`PersistedCache`/`PersistedQueue`; `Reactivity` query-key invalidation; an `Sse` channel/parser/encoder codec; a store-backed `RateLimiter` (fixed-window / token-bucket); a batching/persisted `RequestResolver`; and `DevTools` wiring. Every lane is an OVERLAY over a durable owner: the record of truth is `store/journal` on `@effect/sql`, and the EventLog member surface is the system-of-record boundary law `[R19]`, never a second authority. Each service is one `Context.Tag` + a `layer*` family whose storage is a swappable dependency — memory for specs, IndexedDB/`KeyValueStore` for browsers, SQL for durable nodes — so the same lane rides every runtime by Layer selection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/experimental`
- package: `@effect/experimental`
- version: `0.60.0`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x` (universal-tier substrate; `.api/effect.md`, `.api/effect-platform.md`)
- optional-peer: `ioredis` (Redis `Persistence` backing), `lmdb` (Lmdb backing) — both UNADMITTED; the admitted backings are memory and `@effect/platform` `KeyValueStore`
- catalog-verdict: KEEP `[R19]` — overlay only; the record of truth never depends on experimental
- runtime: dual — `EventLog`/`EventJournal.layerIndexedDb`/`EventLogRemote.layerWebSocketBrowser`/`EventLogEncryption.layerSubtle` are browser-safe (Web Crypto + IndexedDB); `EventLogServer`/`Machine`/`PersistedQueue` are node/bun lanes
- modules: `Event`, `EventGroup`, `EventLog`, `EventJournal`, `EventLogRemote`, `EventLogServer`, `EventLogEncryption`, `Machine`, `Persistence`, `PersistedCache`, `PersistedQueue`, `Reactivity`, `Sse`, `RateLimiter`, `RequestResolver`, `DevTools`, `VariantSchema`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EventLog local-first event-sourcing family
- rail: overlay/local-first
- The client stack: `Event.make` declares a tagged event with a `Schema` payload and primary-key extractor; `EventGroup` collects events into a compaction/reactivity unit; `EventLog` is the append surface + reducer registry; `EventJournal` is the append-only entry store (memory | IndexedDB) with `RemoteId`/`EntryId` HLC-style identity. Events are closed `Schema.TaggedClass` families with app-authored versioning — the same closed-family law as `store/journal`.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                          |
| :-----: | :---------------------------------------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `Event.Event` / `Event.make`                    | tagged event      | `store/journal` overlay events; closed Schema-tagged payload   |
|  [02]   | `Event.EventHandler`                            | reducer contract  | per-event fold handler contract                                |
|  [03]   | `EventGroup.EventGroup` / `EventGroup.empty`    | event group       | `.add(event)` accretes a group; the compaction/reactivity unit |
|  [04]   | `EventLog.EventLog`                             | `Context.Tag`     | the append surface; `store`/`browser` EventLog overlay client   |
|  [05]   | `EventLog.EventLogSchema` / `EventLog.schema`   | schema builder    | `schema(...groups)` freezes the client's event universe        |
|  [06]   | `EventLog.Identity` / `EventLog.Registry`       | identity/registry | client identity + reducer registry Tags                        |
|  [07]   | `EventJournal.EventJournal`                     | `Context.Tag`     | append-only journal store; memory, IndexedDB, or `@effect/sql` `SqlEventJournal` backing |
|  [08]   | `EventJournal.Entry` / `EventJournal.RemoteEntry` | journal row     | local entry / remote-synced entry Schema classes               |
|  [09]   | `EventJournal.RemoteId` / `EventJournal.EntryId` | branded id       | `Uint8ArrayFromSelf` branded ids; `entryIdMillis` time-decode  |
|  [10]   | `EventJournal.EventJournalError`                | tagged error      | journal read/write fault rail                                  |

[PUBLIC_TYPE_SCOPE]: EventLog sync transport + server + encryption
- rail: overlay/local-first
- The sync stack: `EventLogRemote` is the client-side WebSocket sync protocol (MsgPack request/response union, chunking, ping/pong); `EventLogServer` mounts the server side as a socket handler or an `HttpApp` (the `edge/live` protocol-handler mount port — the store EventLog sync server is the standing example); `EventLogEncryption` makes the server zero-knowledge (Web Crypto SubtleCrypto client-side E2E, encrypted entries at rest).

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                            |
| :-----: | :-------------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `EventLogRemote.EventLogRemote`                     | sync client     | client-side change push/pull driver                              |
|  [02]   | `EventLogRemote.ProtocolRequest` / `ProtocolResponse` | Schema union | closed WriteEntries/RequestChanges/Changes/Ping/Pong wire union  |
|  [03]   | `EventLogRemote.ProtocolRequestMsgPack` / `…ResponseMsgPack` | MsgPack codec | `decodeRequest`/`encodeRequest`/`decodeResponse`/`encodeResponse` |
|  [04]   | `EventLogServer.Storage`                            | `Context.Tag`   | server persisted-entry store; `layerStorageMemory` or `@effect/sql` `SqlEventLogServer.layerStorage` `[R4]` |
|  [05]   | `EventLogServer.PersistedEntry`                     | Schema class    | server-side stored entry row                                     |
|  [06]   | `EventLogEncryption.EventLogEncryption`             | `Context.Tag`   | E2E encrypt/decrypt boundary; `security/secret` composes keys    |
|  [07]   | `EventLogEncryption.EncryptedEntry` / `EncryptedRemoteEntry` | Schema | ciphertext-at-rest entry shapes (zero-knowledge server)          |

[PUBLIC_TYPE_SCOPE]: durable execution — Machine actors, persisted queue/cache
- rail: durable-execution
- `Machine` is a serializable durable actor: a state + tagged public/private request procedures, booted to an `Actor` (a `Subscribable` of state), snapshot/restore across process restarts. `PersistedQueue`/`PersistedCache` are durable queue/cache overlays over a `KeyValueStore`. `work/engine` and `work/flow` compose these as the in-process durable-actor altitude (deployment topology is `iac`).

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                          |
| :-----: | :------------------------------------------------ | :-------------- | :------------------------------------------------------------- |
|  [01]   | `Machine.Machine` / `Machine.SerializableMachine` | actor def       | `work/flow/durable` durable-actor definitions                 |
|  [02]   | `Machine.Actor` / `Machine.SerializableActor`     | live actor      | booted actor; `Subscribable` of state for `state`/`ui` binding |
|  [03]   | `Machine.MachineContext` / `Machine.MachineDefect` | context/fault  | in-actor context + defect rail                                 |
|  [04]   | `PersistedQueue.PersistedQueue` / `…Factory`      | durable queue   | `work/queue/job` durable job families over a store            |
|  [05]   | `PersistedQueue.PersistedQueueStore`              | `Context.Tag`   | queue backing store; `layerStoreMemory` or `@effect/sql` `SqlPersistedQueue.layerStore` |
|  [06]   | `PersistedCache.PersistedCache`                   | durable cache   | `work` idempotency/result cache keyed by `Persistence` key    |

[PUBLIC_TYPE_SCOPE]: persistence backing + reactive/streaming + governance
- rail: overlay/resource
- `Persistence` splits `BackingPersistence` (raw byte KV store) from `ResultPersistence` (schema-typed `Persistable` results); both back `PersistedCache`/`PersistedQueue`/`RequestResolver.persisted`. `Reactivity` invalidates query keys after mutations (the `store/project` read-your-writes signal). `Sse` is the SSE codec every SSE seam shares. `RateLimiter` is the distributed limiter for `edge/api/middleware`.

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]   | [CONSUMER / BOUNDARY]                                         |
| :-----: | :--------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `Persistence.BackingPersistence` / `…Store`          | `Context.Tag`   | raw-byte KV backing; `layerMemory` / `layerKeyValueStore`    |
|  [02]   | `Persistence.ResultPersistence` / `…Store`           | `Context.Tag`   | schema-typed result store over a backing                     |
|  [03]   | `Persistence.Persistable`                            | schema mixin    | `WithResult` schema keyed for persistence                    |
|  [04]   | `Persistence.PersistenceParseError` / `…BackingError` | tagged error   | persistence fault rail                                       |
|  [05]   | `Reactivity.Reactivity`                              | `Context.Tag`   | `store/project` query-key invalidation signal                |
|  [06]   | `Sse.Event` / `Sse.EventEncoded` / `Sse.Parser` / `Sse.Encoder` | codec | `net/client/channel`, `edge/live`, `proc/flag` SSE seam        |
|  [07]   | `Sse.Retry`                                          | tagged control  | SSE reconnection `retry:` directive                          |
|  [08]   | `RateLimiter.RateLimiter` / `RateLimiter.RateLimiterStore` | `Context.Tag` | `edge/api/middleware` rate/quota; store-backed for distributed |
|  [09]   | `RateLimiter.RateLimitExceeded` / `RateLimitStoreError` | tagged error  | `RateLimiterError` union — 429/Retry-After mapping           |
|  [10]   | `RequestResolver.PersistedRequest`                  | request mixin   | schema-typed persisted request for `dataLoader`/`persisted`  |
|  [11]   | `DevTools.*` / `VariantSchema.*`                     | dev / schema    | `telemetry ./dev` DevTools wiring; multi-variant schema build |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: EventLog client assembly
- rail: overlay/local-first
- One `schema(...groups)` freezes the event universe; `layer(schema)` provides the `EventLog` service (requiring the group services + `EventJournal` + `Identity` + `Reactivity`); `makeClient(schema)` yields the typed command dispatcher. Storage and identity are swappable dependencies — `layerIndexedDb` + `layerIdentityKvs` for browsers, `layerMemory` for specs.

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `EventLog.schema(...groups: EventGroup.Any[]): EventLogSchema`                                              | schema         | freeze the client event universe                         |
|  [02]   | `EventLog.group(group, (handlers) => handlers)` / `groupCompaction(group, effect)` / `groupReactivity(group, keys)` | handlers | register reducers, compaction, and reactivity keys       |
|  [03]   | `EventLog.layer(schema): Layer<EventLog, never, EventGroup.ToService<…> \| …>`                              | layer          | the composed EventLog service                            |
|  [04]   | `EventLog.makeClient(schema): Effect<(<Tag>(payload) => Effect<…>), …>`                                     | client         | typed per-event command dispatcher                       |
|  [05]   | `EventLog.layerIdentityKvs({ key }): Layer<Identity, …, KeyValueStore>`                                     | identity       | client identity over a `@effect/platform` `KeyValueStore` |
|  [06]   | `EventJournal.layerIndexedDb({ database? })` / `layerMemory`                                                  | journal        | browser IndexedDB journal / memory spec journal          |

[ENTRYPOINT_SCOPE]: EventLog sync transport + mountable server
- rail: overlay/local-first
- Client sync is one Layer: `layerWebSocketBrowser(url)` is self-contained (needs only `EventLog`); `layerWebSocket(url)` needs a `Socket.WebSocketConstructor` (from `BrowserSocket`/`BunSocket`) plus `EventLogEncryption`. The server is `makeHandlerHttp` mounted as an `HttpApp` at the `edge/live` protocol-handler port, or `makeHandler` over a raw `Socket`.

| [INDEX] | [SURFACE]                                                                                           | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                        |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `EventLogRemote.layerWebSocketBrowser(url, { disablePing? }): Layer<never, never, EventLog>`         | client sync    | `browser`/`store` self-contained browser sync                |
|  [02]   | `EventLogRemote.layerWebSocket(url, opts): Layer<never, never, WebSocketConstructor \| EventLog \| EventLogEncryption>` | client sync | node/bun sync; stacks `BunSocket.layerWebSocketConstructor`  |
|  [03]   | `EventLogRemote.fromSocket(opts)` / `fromWebSocket(url, opts)`                                       | client sync    | run sync over an existing `Socket` / raw WS                  |
|  [04]   | `EventLogServer.makeHandlerHttp: Effect<Effect<HttpServerResponse, …>, never, Storage>`             | server         | `edge/live` mount as an `HttpApp` upgrade handler            |
|  [05]   | `EventLogServer.makeHandler: Effect<(socket) => Effect<void, …>, never, Storage>`                   | server         | raw-socket server handler                                    |
|  [06]   | `EventLogServer.layerStorageMemory: Layer<Storage>`                                                  | server storage | memory storage; SQL storage ships in `@effect/sql` (`SqlEventLogServer.layerStorage`/`layerStorageSubtle` — `data/.api/effect-sql.md`) `[R4]` |
|  [07]   | `EventLogEncryption.layerSubtle` / `makeEncryptionSubtle(crypto: Crypto)`                            | encryption     | Web Crypto E2E; zero-knowledge server                        |

[ENTRYPOINT_SCOPE]: durable execution + persistence + governance
- rail: durable-execution/resource
- `Machine.boot` launches a serializable actor; `snapshot`/`restore` cross process restarts. `Persistence.layerResultKeyValueStore` backs the whole persistence tree onto a `KeyValueStore`. `RateLimiter.makeWithRateLimiter` yields the effect-wrapping limiter (`algorithm`: `fixed-window` | `token-bucket`; `onExceeded`: `delay` | `fail`); `makeSleep` is the bare exceeded-sleep form — one limiter, strategies as policy values.

| [INDEX] | [SURFACE]                                                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `Machine.make(…)` / `makeSerializable(…)` / `boot(machine, input)` / `snapshot(actor)` / `restore(machine, snapshot)` | actor | `work/flow/durable` durable actors                       |
|  [02]   | `Persistence.layerResult` / `layerResultMemory` / `layerResultKeyValueStore` / `layerMemory` / `layerKeyValueStore` | persistence | `ResultPersistence` rows back `PersistedCache`/`RequestResolver.persisted`; `layerMemory`/`layerKeyValueStore` are the raw `BackingPersistence` tier beneath `layerResult` |
|  [03]   | `PersistedQueue.make(opts)` / `makeFactory` / `layer` / `layerStoreMemory`                     | durable queue  | `work/queue/job` durable jobs                            |
|  [04]   | `PersistedCache.make({ storeId, lookup, timeToLive, … })`                                      | durable cache  | `work` idempotency / memoized results                    |
|  [05]   | `Reactivity.mutation` / `query` / `stream` / `invalidate(keys)` / `layer`                      | reactive       | `store/project` read-your-writes invalidation            |
|  [06]   | `Sse.makeChannel({ bufferSize? })` / `makeParser(onParse)` / `encoder`                         | SSE codec      | `net/client/channel`, `edge/live`, `proc/flag` SSE         |
|  [07]   | `RateLimiter.make` / `layer` / `layerStoreMemory`; `makeWithRateLimiter` → `({ algorithm?, onExceeded?, window, limit, key, tokens? })(effect)`; `makeSleep({ algorithm?, window, limit, key, tokens? })` | rate limit | `edge/api/middleware` distributed limiter            |
|  [08]   | `RequestResolver.dataLoader({ window, maxBatchSize? })(resolver)` / `persisted({ storeId, … })(resolver)` | batching | batched/persisted request resolution (curried combinators) |
|  [09]   | `DevTools.layer(url?)` / `layerWebSocket(url?)`                                                | dev            | `telemetry ./dev` fenced DevTools export                 |

## [04]-[IMPLEMENTATION_LAW]

[OVERLAY_TOPOLOGY]:
- `[R19]` system-of-record boundary: `store/journal` on `@effect/sql` is the durable authority; EventLog, PersistedQueue, and PersistedCache are OVERLAYS that accelerate local-first reads and offline queues. A record whose loss corrupts state never lives only in an EventLog journal or a persisted queue — it is projected from, or mirrored to, the SQL journal. `store` catalogues `@effect/experimental` as EventLog-overlay-only; `browser` as the EventLog client; `work` as the durable-execution substrate.
- one service, swappable storage: each lane is a `Context.Tag` whose backing is a Layer dependency. `EventJournal` = `layerMemory` (spec) | `layerIndexedDb` (browser) | `@effect/sql` `SqlEventJournal.layer` (durable) `[R4]`. `Persistence` = `layerMemory` | `layerKeyValueStore`. The durable `PersistedQueueStore`/`EventLogServer.Storage` backings ship in `@effect/sql` (`SqlPersistedQueue.layerStore`/`SqlEventLogServer.layerStorage` — `data/.api/effect-sql.md`); `RateLimiterStore` = `layerStoreMemory`. The lane code never names its storage; the app root selects it.
- closed event families: `Event.make` payloads are `Schema.TaggedClass` closed families with app-authored versioning; read-time upcasting is a `store/journal/upcast` total fold, never a journal rewrite — the same law the SQL journal holds.

[INTEGRATION_LAW]:
- Stack with `@effect/platform-browser` / `@effect/platform-bun`: EventLog client identity rides `EventLog.layerIdentityKvs({ key })` over a `KeyValueStore` satisfied by `BrowserKeyValueStore.layerLocalStorage` (browser) or `BunKeyValueStore.layerFileSystem` (node/bun). WebSocket sync rides `EventLogRemote.layerWebSocket` over a `Socket.WebSocketConstructor` from `BrowserSocket.layerWebSocketConstructor` / `BunSocket.layerWebSocketConstructor`. The overlay declares the `@effect/platform` Tag; the platform binding satisfies it.
- Stack with `@effect/platform` HttpApp: `EventLogServer.makeHandlerHttp` mounts at `edge/live/socket` as the protocol-handler mount port (an `HttpApp` port Tag); the `BunHttpServer`/`NodeHttpServer` serve row hosts it. `EventLogEncryption.layerSubtle` composes `security/secret` key material so the server stays zero-knowledge.
- Stack with `store`/`state`: EventLog reducers fold into `state` vocabulary; `Reactivity.invalidate` is the read-your-writes signal `store/project/inline` emits after an OCC append; `Machine` actors persist through `PersistedQueue`/`Persistence` onto the store-owned `KeyValueStore` driver.
- Stack with `stamina`-equivalent retry / `net/client`: sync reconnection and `Sse.Retry` reconnection budgets ride `core/value/fault` degradation budgets, not a hand-rolled loop; `edge/api/middleware` reads `RateLimiter` `RateLimitExceeded` into a 429/Retry-After problem detail.

[LOCAL_ADMISSION]:
- EventLog client and journal are browser-safe (`layerIndexedDb`, `layerWebSocketBrowser`, `layerSubtle` over Web Crypto); `EventLogServer`/`Machine`/`PersistedQueue` durable lanes are node/bun only.
- optional peers `ioredis`/`lmdb` back `Persistence` for Redis/Lmdb; both are UNADMITTED — the admitted backings are memory and `@effect/platform` `KeyValueStore`.
- every lane bounds its backing at the composition root; a lane imported with no Layer-provided store is the defect.

[RAIL_LAW]:
- Package: `@effect/experimental`
- Owns: EventLog local-first event-sourcing (event/group/journal/log), E2E-encrypted WebSocket sync + mountable server, serializable durable `Machine` actors, `KeyValueStore`-backed `Persistence`/`PersistedCache`/`PersistedQueue`, `Reactivity` invalidation, `Sse` codec, distributed `RateLimiter`, batching `RequestResolver`, `DevTools`
- Accept: EventLog as the local-first OVERLAY client (`schema`→`layer`→`makeClient`), `EventLogServer` mounted at the `edge/live` HttpApp port, swappable storage Layers per lane, `RateLimiter.makeWithRateLimiter`/`makeSleep` with `algorithm`/`onExceeded` as policy values, `Sse` as the one SSE codec, durable `Machine`/`PersistedQueue` for `work`
- Reject: EventLog or a persisted queue as the system of record (the SQL journal owns truth `[R19]`), a hand-rolled SSE parser or retry loop, a second local-first or durable-actor implementation, `ioredis`/`lmdb` backings, storage hardcoded inside a lane instead of Layer-injected
