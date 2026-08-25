# [TS_BRANCH_API_EFFECT_EXPERIMENTAL]

`@effect/experimental` owns local-first overlays, storage-backed services, and host-neutral `Machine` actors. Storage services select memory, IndexedDB, key-value, or SQL Layers; `Machine` holds in-process state only. The SQL journal remains the durable authority.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EventLog local-first event-sourcing family
- rail: overlay/local-first
- `Event.make` payloads are closed `Schema.TaggedClass` families with app-authored versioning, the same closed-family law as `journal/append`; `EventJournal` carries `RemoteId`/`EntryId` HLC-style identity.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]     | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------ | :---------------- | :------------------------------------------------------------- |
|  [01]   | `Event.Event` / `Event.make`                      | tagged event      | `journal/append` overlay events; closed Schema-tagged payload  |
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
- `EventLogServer` mounts the server as a raw `Socket` handler or an `HttpApp` at the `serve/live` protocol-handler port; `EventLogEncryption` makes it zero-knowledge — client-side Web Crypto E2E, ciphertext entries at rest.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `EventLogRemote.EventLogRemote`                              | sync client   | client-side change push/pull driver                     |
|  [02]   | `EventLogRemote.ProtocolRequest` / `ProtocolResponse`        | Schema union  | WriteEntries/RequestChanges/Changes/Ping/Pong           |
|  [03]   | `EventLogRemote.ProtocolRequestMsgPack` / `…ResponseMsgPack` | MsgPack codec | request/response wire codec                             |
|  [04]   | `EventLogServer.Storage`                                     | `Context.Tag` | `layerStorageMemory` / SQL `[SQL_OVERLAY_BACKING]`      |
|  [05]   | `EventLogServer.PersistedEntry`                              | Schema class  | server-side stored entry row                            |
|  [06]   | `EventLogEncryption.EventLogEncryption`                      | `Context.Tag` | `crypt/secret` composes E2E keys                        |
|  [07]   | `EventLogEncryption.EncryptedEntry` / `EncryptedRemoteEntry` | Schema        | ciphertext-at-rest entry shapes (zero-knowledge server) |

- `EventLogRemote` MsgPack codec: `decodeRequest`/`encodeRequest`/`decodeResponse`/`encodeResponse`.

[PUBLIC_TYPE_SCOPE]: actor execution and persisted queue/cache
- rail: actor-execution/persistence
- `Machine` is a host-neutral in-process serializable actor. `snapshot` and `restore` encode and consume state; they do not store it.
- `PersistedQueue` and `PersistedCache` are separate storage-backed services over `KeyValueStore`/`Persistence`.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Machine.Machine` / `Machine.SerializableMachine`  | actor def     | host-neutral in-process actor definitions                      |
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
|  [05]   | `Reactivity.Reactivity`                                         | `Context.Tag`  | `read/live` query-key invalidation signal          |
|  [06]   | `Sse.Event` / `Sse.EventEncoded` / `Sse.Parser` / `Sse.Encoder` | codec          | `net/channel` / `serve/live` SSE seam              |
|  [07]   | `Sse.Retry`                                                     | tagged control | SSE reconnection `retry:` directive                |
|  [08]   | `RateLimiter.RateLimiter` / `RateLimiter.RateLimiterStore`      | `Context.Tag`  | `serve` + `work` + `security` quota store port     |
|  [09]   | `RateLimiter.RateLimitExceeded` / `RateLimitStoreError`         | tagged error   | shared `_tag`, split on `reason` — 429/Retry-After |
|  [10]   | `RequestResolver.PersistedRequest`                              | request mixin  | persisted request for `dataLoader`/`persisted`     |
|  [11]   | `DevTools.*` / `VariantSchema.*`                                | dev / schema   | DevTools wiring + multi-variant schema build       |

- `VariantSchema.make({ variants, defaultVariant })` mints the whole builder set from one declaration — `Struct`, `Field`, `FieldOnly(...keys)`, `FieldExcept(...keys)`, `fieldEvolve`, `fieldFromKey`, `Class<Self>(identifier)(fields, annotations?)`, `Union(...members)`, and `extract(variant)` — each `Function.dual` where it takes a subject, and `Struct.Validate` is what refuses a field naming a variant the set never declared.
- `Override(value)` brands a value the `Overrideable(from, to, {...})` property signature admits, so a variant supplies its own computed field without a second schema declaration.
- `@effect/sql` `Model` IS this constructor applied to the relational variant set — `select`, `insert`, `update`, `json`, `jsonCreate`, `jsonUpdate` — so a relation family derives every projection through `Model.Class` and a bespoke `VariantSchema.make` earns its seat only where the variant axis is not the relational one.

## [02]-[ENTRYPOINTS]

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
- `layerWebSocketBrowser(url)` is self-contained (needs only `EventLog`); `layerWebSocket(url)` needs a `Socket.WebSocketConstructor` (from `BrowserSocket`/`BunSocket`) and `EventLogEncryption`. `makeHandlerHttp` mounts the server as an `HttpApp` at the `serve/live` port; `makeHandler` serves it over a raw `Socket`.

| [INDEX] | [SURFACE]                                                                 | [SHAPE]        | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `EventLogRemote.layerWebSocketBrowser(url)`                               | client sync    | `browser/persist#Overlay` browser sync      |
|  [02]   | `EventLogRemote.layerWebSocket(url, opts)`                                | client sync    | node/bun sync via `BunSocket`               |
|  [03]   | `EventLogRemote.fromSocket(opts)` / `fromWebSocket(url, opts)`            | client sync    | over existing `Socket` / raw WS             |
|  [04]   | `EventLogServer.makeHandlerHttp`                                          | server         | `serve/live` `HttpApp` upgrade handler      |
|  [05]   | `EventLogServer.makeHandler`                                              | server         | raw-socket server handler                   |
|  [06]   | `EventLogServer.layerStorageMemory`                                       | server storage | memory storage; SQL `[SQL_OVERLAY_BACKING]` |
|  [07]   | `EventLogEncryption.layerSubtle` / `makeEncryptionSubtle(crypto: Crypto)` | encryption     | Web Crypto E2E; zero-knowledge server       |

[ENTRYPOINT_SCOPE]: actor execution + persistence + governance
- rail: actor-execution/resource
- `Machine.boot` launches an in-process actor; `snapshot`/`restore` cross an encoded state boundary without persistence.
- `Persistence.layerResultKeyValueStore` mounts the separate persistence tree onto a `KeyValueStore`.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]       | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `Machine.procedures.make(state)` / `.add<Req>()(tag, handler)`               | procedures    | build the request-handler list     |
|  [02]   | `Machine.procedures.addPrivate<Req>()(tag, handler)`                         | procedures    | Private-union twin; type-curried   |
|  [03]   | `Machine.serializable.add`/`.addPrivate(schema, handler)`                    | procedures    | schema-first dual; wire-lane twin  |
|  [04]   | `Machine.make` / `makeWith<State, Input>()` / `makeSerializable`             | actor def     | host-neutral in-process actors     |
|  [05]   | `Machine.retry(policy)` / `withTracingEnabled(bool)`                         | actor def     | re-drive failed init; span toggle  |
|  [06]   | `Machine.boot` / `snapshot` / `restore`                                      | actor         | run one; encode and restore state  |
|  [07]   | `Persistence.layerResult` / `layerResultMemory` / `layerResultKeyValueStore` | result store  | schema-typed result tier           |
|  [08]   | `Persistence.layerMemory` / `layerKeyValueStore`                             | backing store | raw-byte backing tier              |
|  [09]   | `PersistedQueue.make` / `makeFactory` / `layer` / `layerStoreMemory`         | durable queue | `work/queue/job` durable jobs      |
|  [10]   | `PersistedCache.make(...)`                                                   | durable cache | `work` idempotency cache           |
|  [11]   | `Reactivity.mutation` / `query` / `stream` / `invalidate(keys)` / `layer`    | reactive      | `read/live` read-your-writes       |
|  [12]   | `Sse.makeChannel(...)` / `makeParser(...)` / `encoder`                       | SSE codec     | `net/channel` + `serve/live` codec |
|  [13]   | `RateLimiter.make` / `layer` / `layerStoreMemory`                            | rate limit    | `crypt/verify` + `lane/olap` quota |
|  [14]   | `RateLimiter.makeWithRateLimiter` / `makeSleep`                              | rate limit    | `serve` refuse, `work` delay       |
|  [15]   | `RequestResolver.dataLoader(...)` / `persisted(...)`                         | batching      | curried batch/persist combinators  |
|  [16]   | `DevTools.layer(url?)` / `layerWebSocket(url?)`                              | dev           | `telemetry ./dev` DevTools export  |

- `Machine.procedures.make(initialState, { identifier? })` seeds an empty `ProcedureList`; `procedures.add<Req>()(list, tag, handler)` curries the request type first and then runs `Function.dual`, so each call widens the list's Public union by `Req` and its `R` by the handler's requirements, and `procedures.addPrivate` lands the same handler on the Private union instead.
- `Machine.serializable` is the differently-shaped twin a `makeSerializable` definition builds through — `serializable.add(schema, handler)` and `serializable.addPrivate(schema, handler)` are `Function.dual` over a `Schema.TaggedRequest` carrying its own `_tag`, never a type application over a tag string, and each widens `R` by `Schema.SerializableWithResult.Context<Req>` on top of the handler's. The two namespaces never substitute: a Private request added through `serializable.addPrivate` is the one that never reaches `sendUnknown`.
- `Machine.withTracingEnabled(effect, enabled)` is `Function.dual` over the `currentTracingEnabled` `FiberRef`, so a booted actor's request spans switch off as a scoped policy value rather than a definition-time flag.
- `Machine.makeWith<State, Input>()` pins the state and input types up front and then takes the same initializer `make` does, which is what admits a recursive or self-referencing state a bare `make` cannot infer.
- `Machine.retry(self, policy)` wraps the DEFINITION rather than a booted actor: the `Schedule` reads `Machine.InitError<M> | MachineDefect` as its input and the result widens the machine's context by the schedule's `R`, so a boot whose initialization fails re-drives under the policy.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `[OVERLAY_BOUNDARY_RULING]` system-of-record boundary: `journal/append` on `@effect/sql` is the durable authority and the overlay lanes only accelerate local-first reads and offline queues; a record whose loss corrupts state is projected from, or mirrored to, the SQL journal, never held only in an overlay.
- one service, swappable storage: each lane is a `Context.Tag` whose backing is a Layer the app root selects, never named in lane code. `EventJournal` selects `layerMemory`, `layerIndexedDb`, or `@effect/sql` `SqlEventJournal.layer` `[SQL_OVERLAY_BACKING]`; `Persistence` selects `layerMemory` or `layerKeyValueStore`; `RateLimiterStore` selects `layerStoreMemory` on a single node and a shared store-backed Layer across a fleet, which is the one selection deciding whether a quota bucket is per-process or estate-wide.
- durable store backings ship in `@effect/sql`: `PersistedQueueStore` binds `SqlPersistedQueue.layerStore`, `EventLogServer.Storage` binds `SqlEventLogServer.layerStorage` (`data/.api/effect-sql.md`).
- closed event families: `Event.make` payloads are `Schema.TaggedClass` closed families the overlay holds in one shape; a shape change re-mints the durable log whole at `journal/evolve`, never a per-entry read path.

[STACKING]:
- `@effect/platform-browser`(`.api/effect-platform-browser.md`) / `@effect/platform-bun`(`.api/effect-platform-bun.md`): `EventLog.layerIdentityKvs({ key })` binds a `KeyValueStore` satisfied by `BrowserKeyValueStore.layerLocalStorage` or `BunKeyValueStore.layerFileSystem`, and `EventLogRemote.layerWebSocket` binds a `Socket.WebSocketConstructor` from `BrowserSocket.layerWebSocketConstructor` / `BunSocket.layerWebSocketConstructor`; each overlay declares the `@effect/platform` Tag and the platform binding satisfies it.
- `@effect/platform`(`.api/effect-platform.md`): `EventLogServer.makeHandlerHttp` mounts at the `serve/live` socket `HttpApp` port a `BunHttpServer`/`NodeHttpServer` serve row hosts; `EventLogEncryption.layerSubtle` composes `crypt/secret` key material for the zero-knowledge server.
- `data`/`state`: EventLog reducers fold into core state, and `Reactivity.invalidate` signals read-your-writes publication.
- `Machine` remains host-neutral; runtime workflow/cluster/storage owners compose durable replay or persistence around it.
- `net/client` + `core/value/fault`: sync and `Sse.Retry` reconnection budgets ride `core/value/fault` degradation, never a hand-rolled loop; `authn/session` and `crypt/verify` fold `RateLimitExceeded` to their own `throttled` reason, and `serve/problem`'s `exhausted` row renders the 429/Retry-After problem detail.
- one accessor, postures per site: `makeWithRateLimiter` carries `onExceeded` as the whole difference — `serve/api`'s `Gate.fenced` takes `"fail"` so `serve/route`'s edge quota answers a Problem, `work/queue`'s `Throttle.pace` takes `"delay"` beside the `@effect/workflow` `DurableRateLimiter` arm that survives replay, and every row on both sides carries the same four columns (`window`, `limit`, `key`, `cost`). The store namespaces nothing, so each site joins its own scope into `key` or two rows share one bucket.

[LOCAL_ADMISSION]:
- EventLog client, journal, and Machine are host-neutral or browser-capable; server and persisted-queue bindings select host Layers.
- every lane bounds its backing at the composition root; a lane imported with no Layer-provided store is the defect.
