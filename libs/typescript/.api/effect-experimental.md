# [TS_BRANCH_API_EFFECT_EXPERIMENTAL]

`@effect/experimental` owns the overlay-lane family: every service is one `Context.Tag` with a `layer*` whose storage is a swappable Layer dependency — memory for specs, IndexedDB/`KeyValueStore` for browsers, SQL for durable nodes — so one lane rides every runtime by Layer selection. Each lane overlays a durable owner; `store/journal` on `@effect/sql` holds the record of truth, and no overlay journal or persisted queue is ever a second authority `[OVERLAY_BOUNDARY_RULING]`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/experimental`
- package: `@effect/experimental` (MIT)
- effect-peer: `effect catalog`, `@effect/platform catalog` (universal-tier substrate; `.api/effect.md`, `.api/effect-platform.md`)
- optional-peer: `ioredis` (Redis `Persistence` backing), `lmdb` (Lmdb backing) — both UNADMITTED
- runtime: dual — browser-safe client/journal/sync/encryption lanes; node/bun server/actor/queue lanes

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EventLog local-first event-sourcing family
- rail: overlay/local-first
- `Event.make` payloads are closed `Schema.TaggedClass` families with app-authored versioning, the same closed-family law as `store/journal`; `EventJournal` carries `RemoteId`/`EntryId` HLC-style identity.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------ | :---------------- | :------------------------------------------------------------- |
|  [01]   | `Event.Event` / `Event.make`                      | tagged event      | `store/journal` overlay events; closed Schema-tagged payload   |
|  [02]   | `Event.EventHandler`                              | reducer contract  | per-event fold handler contract                                |
|  [03]   | `EventGroup.EventGroup` / `EventGroup.empty`      | event group       | `.add(event)` accretes a group; the compaction/reactivity unit |
|  [04]   | `EventLog.EventLog`                               | `Context.Tag`     | the append surface; `store`/`browser` EventLog overlay client  |
|  [05]   | `EventLog.EventLogSchema` / `EventLog.schema`     | schema builder    | `schema(...groups)` freezes the client's event universe        |
|  [06]   | `EventLog.Identity` / `EventLog.Registry`         | identity/registry | client identity + reducer registry Tags                        |
|  [07]   | `EventJournal.EventJournal`                       | `Context.Tag`     | memory / IndexedDB / `SqlEventJournal` backing                 |
|  [08]   | `EventJournal.Entry` / `EventJournal.RemoteEntry` | journal row       | local entry / remote-synced entry Schema classes               |
|  [09]   | `EventJournal.RemoteId` / `EventJournal.EntryId`  | branded id        | `Uint8ArrayFromSelf` branded ids; `entryIdMillis` time-decode  |
|  [10]   | `EventJournal.EventJournalError`                  | tagged error      | journal read/write fault rail                                  |

[PUBLIC_TYPE_SCOPE]: EventLog sync transport + server + encryption
- rail: overlay/local-first
- `EventLogServer` mounts the server as a raw `Socket` handler or an `HttpApp` at the `edge/live` protocol-handler port; `EventLogEncryption` makes it zero-knowledge — client-side Web Crypto E2E, ciphertext entries at rest.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `EventLogRemote.EventLogRemote`                              | sync client   | client-side change push/pull driver                     |
|  [02]   | `EventLogRemote.ProtocolRequest` / `ProtocolResponse`        | Schema union  | WriteEntries/RequestChanges/Changes/Ping/Pong           |
|  [03]   | `EventLogRemote.ProtocolRequestMsgPack` / `…ResponseMsgPack` | MsgPack codec | request/response wire codec                             |
|  [04]   | `EventLogServer.Storage`                                     | `Context.Tag` | `layerStorageMemory` / SQL `[SQL_OVERLAY_BACKING]`      |
|  [05]   | `EventLogServer.PersistedEntry`                              | Schema class  | server-side stored entry row                            |
|  [06]   | `EventLogEncryption.EventLogEncryption`                      | `Context.Tag` | `security/secret` composes E2E keys                     |
|  [07]   | `EventLogEncryption.EncryptedEntry` / `EncryptedRemoteEntry` | Schema        | ciphertext-at-rest entry shapes (zero-knowledge server) |

- `EventLogRemote` MsgPack codec: `decodeRequest`/`encodeRequest`/`decodeResponse`/`encodeResponse`.

[PUBLIC_TYPE_SCOPE]: durable execution — Machine actors, persisted queue/cache
- rail: durable-execution
- `Machine` is a serializable durable actor — state with tagged public/private request procedures, booted to an `Actor` (`Subscribable` of state), snapshot/restore across process restarts; `PersistedQueue`/`PersistedCache` overlay a `KeyValueStore`.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Machine.Machine` / `Machine.SerializableMachine`  | actor def     | `work/flow/durable` durable-actor definitions                  |
|  [02]   | `Machine.Actor` / `Machine.SerializableActor`      | live actor    | booted actor; `Subscribable` of state for `state`/`ui` binding |
|  [03]   | `Machine.MachineContext` / `Machine.MachineDefect` | context/fault | in-actor context + defect rail                                 |
|  [04]   | `PersistedQueue.PersistedQueue` / `…Factory`       | durable queue | `work/queue/job` durable job families over a store             |
|  [05]   | `PersistedQueue.PersistedQueueStore`               | `Context.Tag` | `layerStoreMemory` / `SqlPersistedQueue.layerStore` backing    |
|  [06]   | `PersistedCache.PersistedCache`                    | durable cache | `work` idempotency/result cache keyed by `Persistence` key     |

[PUBLIC_TYPE_SCOPE]: persistence backing + reactive/streaming + governance
- rail: overlay/resource
- `Persistence` splits `BackingPersistence` (raw-byte KV) from `ResultPersistence` (schema-typed `Persistable`); both back `PersistedCache`/`PersistedQueue`/`RequestResolver.persisted`.

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `Persistence.BackingPersistence` / `…Store`                     | `Context.Tag`  | `layerMemory` / `layerKeyValueStore` backing       |
|  [02]   | `Persistence.ResultPersistence` / `…Store`                      | `Context.Tag`  | schema-typed result store over a backing           |
|  [03]   | `Persistence.Persistable`                                       | schema mixin   | `WithResult` schema keyed for persistence          |
|  [04]   | `Persistence.PersistenceParseError` / `…BackingError`           | tagged error   | persistence fault rail                             |
|  [05]   | `Reactivity.Reactivity`                                         | `Context.Tag`  | `store/project` query-key invalidation signal      |
|  [06]   | `Sse.Event` / `Sse.EventEncoded` / `Sse.Parser` / `Sse.Encoder` | codec          | `net/client` / `edge/live` SSE seam                |
|  [07]   | `Sse.Retry`                                                     | tagged control | SSE reconnection `retry:` directive                |
|  [08]   | `RateLimiter.RateLimiter` / `RateLimiter.RateLimiterStore`      | `Context.Tag`  | `edge/api/middleware` rate/quota limiter           |
|  [09]   | `RateLimiter.RateLimitExceeded` / `RateLimitStoreError`         | tagged error   | `RateLimiterError` union — 429/Retry-After mapping |
|  [10]   | `RequestResolver.PersistedRequest`                              | request mixin  | persisted request for `dataLoader`/`persisted`     |
|  [11]   | `DevTools.*` / `VariantSchema.*`                                | dev / schema   | DevTools wiring + multi-variant schema build       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: EventLog client assembly
- rail: overlay/local-first
- Assembly runs `schema(...groups)` → `layer(schema)` → `makeClient(schema)`: `schema` freezes the event universe, `layer` mounts `EventLog` over the group services + `EventJournal` + `Identity` + `Reactivity`, `makeClient` yields the typed command dispatcher; storage and identity are swappable Layers — `layerIndexedDb` + `layerIdentityKvs` browser, `layerMemory` spec.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `EventLog.schema(...)`                                                  | schema   | freeze the client event universe           |
|  [02]   | `EventLog.group(...)` / `groupCompaction(...)` / `groupReactivity(...)` | handlers | reducer/compaction/reactivity registration |
|  [03]   | `EventLog.layer(schema)`                                                | layer    | the composed EventLog service              |
|  [04]   | `EventLog.makeClient(schema)`                                           | client   | typed per-event command dispatcher         |
|  [05]   | `EventLog.layerIdentityKvs({ key })`                                    | identity | client identity over `KeyValueStore`       |
|  [06]   | `EventJournal.layerIndexedDb({ database? })` / `layerMemory`            | journal  | IndexedDB / memory journal                 |

[ENTRYPOINT_SCOPE]: EventLog sync transport + mountable server
- rail: overlay/local-first
- `layerWebSocketBrowser(url)` is self-contained (needs only `EventLog`); `layerWebSocket(url)` needs a `Socket.WebSocketConstructor` (from `BrowserSocket`/`BunSocket`) and `EventLogEncryption`. `makeHandlerHttp` mounts the server as an `HttpApp` at the `edge/live` port; `makeHandler` serves it over a raw `Socket`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]        | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `EventLogRemote.layerWebSocketBrowser(url)`                               | client sync    | `browser`/`store` browser sync              |
|  [02]   | `EventLogRemote.layerWebSocket(url, opts)`                                | client sync    | node/bun sync via `BunSocket`               |
|  [03]   | `EventLogRemote.fromSocket(opts)` / `fromWebSocket(url, opts)`            | client sync    | over existing `Socket` / raw WS             |
|  [04]   | `EventLogServer.makeHandlerHttp`                                          | server         | `edge/live` `HttpApp` upgrade handler       |
|  [05]   | `EventLogServer.makeHandler`                                              | server         | raw-socket server handler                   |
|  [06]   | `EventLogServer.layerStorageMemory`                                       | server storage | memory storage; SQL `[SQL_OVERLAY_BACKING]` |
|  [07]   | `EventLogEncryption.layerSubtle` / `makeEncryptionSubtle(crypto: Crypto)` | encryption     | Web Crypto E2E; zero-knowledge server       |

[ENTRYPOINT_SCOPE]: durable execution + persistence + governance
- rail: durable-execution/resource
- `Machine.boot` launches a serializable actor, `snapshot`/`restore` crossing process restarts; `Persistence.layerResultKeyValueStore` backs the persistence tree onto a `KeyValueStore`. `RateLimiter.makeWithRateLimiter` wraps an effect with `algorithm` (`fixed-window` | `token-bucket`) and `onExceeded` (`delay` | `fail`) as policy values, `makeSleep` the bare exceeded-sleep form.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]       | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `Machine.make` / `makeSerializable` / `boot` / `snapshot` / `restore`        | actor         | `work/flow/durable` durable actors |
|  [02]   | `Persistence.layerResult` / `layerResultMemory` / `layerResultKeyValueStore` | result store  | schema-typed result tier           |
|  [03]   | `Persistence.layerMemory` / `layerKeyValueStore`                             | backing store | raw-byte backing tier              |
|  [04]   | `PersistedQueue.make` / `makeFactory` / `layer` / `layerStoreMemory`         | durable queue | `work/queue/job` durable jobs      |
|  [05]   | `PersistedCache.make(...)`                                                   | durable cache | `work` idempotency cache           |
|  [06]   | `Reactivity.mutation` / `query` / `stream` / `invalidate(keys)` / `layer`    | reactive      | `store/project` read-your-writes   |
|  [07]   | `Sse.makeChannel(...)` / `makeParser(...)` / `encoder`                       | SSE codec     | `edge/live` SSE codec              |
|  [08]   | `RateLimiter.make` / `layer` / `layerStoreMemory`                            | rate limit    | `edge/api/middleware` limiter      |
|  [09]   | `RateLimiter.makeWithRateLimiter` / `makeSleep`                              | rate limit    | `algorithm`/`onExceeded` policy    |
|  [10]   | `RequestResolver.dataLoader(...)` / `persisted(...)`                         | batching      | curried batch/persist combinators  |
|  [11]   | `DevTools.layer(url?)` / `layerWebSocket(url?)`                              | dev           | `telemetry ./dev` DevTools export  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `[OVERLAY_BOUNDARY_RULING]` system-of-record boundary: `store/journal` on `@effect/sql` is the durable authority and the overlay lanes only accelerate local-first reads and offline queues; a record whose loss corrupts state is projected from, or mirrored to, the SQL journal, never held only in an overlay.
- one service, swappable storage: each lane is a `Context.Tag` whose backing is a Layer the app root selects, never named in lane code. `EventJournal` selects `layerMemory`, `layerIndexedDb`, or `@effect/sql` `SqlEventJournal.layer` `[SQL_OVERLAY_BACKING]`; `Persistence` selects `layerMemory` or `layerKeyValueStore`; `RateLimiterStore` selects `layerStoreMemory`.
- durable store backings ship in `@effect/sql`: `PersistedQueueStore` binds `SqlPersistedQueue.layerStore`, `EventLogServer.Storage` binds `SqlEventLogServer.layerStorage` (`data/.api/effect-sql.md`).
- closed event families: `Event.make` payloads are `Schema.TaggedClass` closed families with app-authored versioning; read-time upcasting is a `store/journal/upcast` total fold, never a journal rewrite.

[STACKING]:
- `@effect/platform-browser`(`.api/effect-platform-browser.md`) / `@effect/platform-bun`(`.api/effect-platform-bun.md`): `EventLog.layerIdentityKvs({ key })` binds a `KeyValueStore` satisfied by `BrowserKeyValueStore.layerLocalStorage` or `BunKeyValueStore.layerFileSystem`, and `EventLogRemote.layerWebSocket` binds a `Socket.WebSocketConstructor` from `BrowserSocket.layerWebSocketConstructor` / `BunSocket.layerWebSocketConstructor`; each overlay declares the `@effect/platform` Tag and the platform binding satisfies it.
- `@effect/platform`(`.api/effect-platform.md`): `EventLogServer.makeHandlerHttp` mounts at the `edge/live/socket` `HttpApp` port a `BunHttpServer`/`NodeHttpServer` serve row hosts; `EventLogEncryption.layerSubtle` composes `security/secret` key material for the zero-knowledge server.
- `store`/`state`: EventLog reducers fold into `state` vocabulary; `Reactivity.invalidate` is the read-your-writes signal `store/project/inline` emits after an OCC append; `Machine` actors persist through `PersistedQueue`/`Persistence` onto the store-owned `KeyValueStore` driver.
- `net/client` + `core/value/fault`: sync and `Sse.Retry` reconnection budgets ride `core/value/fault` degradation, never a hand-rolled loop; `edge/api/middleware` maps `RateLimiter` `RateLimitExceeded` to a 429/Retry-After problem detail.

[LOCAL_ADMISSION]:
- EventLog client and journal are browser-safe (`layerIndexedDb`, `layerWebSocketBrowser`, `layerSubtle` over Web Crypto); `EventLogServer`/`Machine`/`PersistedQueue` durable lanes are node/bun only.
- every lane bounds its backing at the composition root; a lane imported with no Layer-provided store is the defect.

[RAIL_LAW]:
- Package: `@effect/experimental`
- Owns: EventLog local-first event-sourcing (event/group/journal/log), E2E-encrypted WebSocket sync + mountable server, serializable durable `Machine` actors, `KeyValueStore`-backed `Persistence`/`PersistedCache`/`PersistedQueue`, `Reactivity` invalidation, `Sse` codec, distributed `RateLimiter`, batching `RequestResolver`, `DevTools`
- Accept: EventLog as the local-first OVERLAY client (`schema`→`layer`→`makeClient`), `EventLogServer` mounted at the `edge/live` HttpApp port, swappable storage Layers per lane, `RateLimiter.makeWithRateLimiter`/`makeSleep` with `algorithm`/`onExceeded` as policy values, `Sse` as the one SSE codec, durable `Machine`/`PersistedQueue` for `work`
- Reject: EventLog or a persisted queue as the system of record (the SQL journal owns truth `[OVERLAY_BOUNDARY_RULING]`), a hand-rolled SSE parser or retry loop, a second local-first or durable-actor implementation, `ioredis`/`lmdb` backings, storage hardcoded inside a lane instead of Layer-injected
