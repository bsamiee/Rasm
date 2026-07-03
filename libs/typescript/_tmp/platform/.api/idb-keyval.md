# [API_CATALOGUE] idb-keyval

`idb-keyval` supplies a lightweight `Promise`-based key-value store over IndexedDB: single-entry ops (`get`/`set`/`del`/`update`), atomic bulk ops (`getMany`/`setMany`/`delMany`), cursor reads (`keys`/`values`/`entries`) plus `clear`, the `createStore(dbName, storeName)` named-store factory returning a `UseStore` transaction-accessor, and the `promisifyRequest` raw-`IDBRequest`/`IDBTransaction` escape hatch. Every op takes an optional trailing `customStore?: UseStore`, so store targeting is one parameter across the whole surface rather than a per-store API. In `platform` it is the SOLE sanctioned IndexedDB site, owned by `Session/store.md` `LocalPersistence`: a frozen `Record<StoreDomain, UseStore>` maps each domain to its own named store, the last-good snapshot rides a `@effect/platform` `KeyValueStore.make` adapter over the `snapshot` store, and the offline-queue drain is one atomic `entries`-then-`clear` transaction.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `idb-keyval`
- package: `idb-keyval`
- version: `6.2.6`
- license: `Apache-2.0`
- runtime: `browser` — requires the DOM `indexedDB` global; not isomorphic (a Node fake-indexeddb shim is a test-only concern, never a platform runtime)
- module: `idb-keyval` -> `exports["."]` = `dist/index.js` (the tree-shakeable ESM barrel a modern bundler binds); the legacy `main`/`module` (`dist/compat.{cjs,js}`) is the bundled all-in-one build platform never imports
- type: `module`; side-effects: `false` (tree-shakeable — unused ops drop from the bundle)
- peer: none; deps: none — a zero-dependency IndexedDB wrapper
- catalog-verdict: KEEP; the one IndexedDB durability owner under `@effect/platform` `KeyValueStore`, admitted only at `Session/store.md` `LocalPersistence`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store accessor type
- rail: persistence
- `UseStore` is the single customization seam: a transaction-accessor closure that opens one `IDBObjectStore` in the requested mode and runs a callback inside it. `createStore` produces one; every op's `customStore?` parameter accepts one; the raw callback shape is the escape hatch for an op the wrapper does not cover (a compound index, a cursor range).

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [SIGNATURE / BOUNDARY]                                                              |
| :-----: | :----------- | :------------ | :--------------------------------------------------------------------------------- |
|  [01]   | `UseStore`   | type alias    | `<T>(txMode: IDBTransactionMode, callback: (store: IDBObjectStore) => T \| PromiseLike<T>) => Promise<T>` |
| ` `     | `IDBValidKey`| lib.dom       | `string \| number \| Date \| BufferSource \| IDBValidKey[]` — the key union every op accepts (string keys are conventional) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: store construction and raw-IDB bridge
- rail: persistence

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                              |
| :-----: | :--------------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `createStore(dbName: string, storeName: string): UseStore` | store factory | one named IDB database+object-store; the per-domain store handle   |
|  [02]   | `promisifyRequest<T = undefined>(request: IDBRequest<T> \| IDBTransaction): Promise<T>` | IDB bridge | resolve on `success`/`complete`, reject on `error`; the raw-IDB escape hatch inside a `UseStore` callback |

[ENTRYPOINT_SCOPE]: single-entry operations
- rail: persistence

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                  |
| :-----: | :----------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `get<T = any>(key: IDBValidKey, customStore?): Promise<T \| undefined>` | read | value by key; `undefined` when absent (indistinguishable from a stored `undefined`) |
|  [02]   | `set(key: IDBValidKey, value: any, customStore?): Promise<void>` | write | one `readwrite` key-value write                       |
|  [03]   | `del(key: IDBValidKey, customStore?): Promise<void>`         | delete         | remove a single key                                   |
|  [04]   | `update<T = any>(key, updater: (old: T \| undefined) => T, customStore?): Promise<void>` | atomic RMW | read-modify-write in ONE `readwrite` transaction — the race-free enqueue/increment primitive |

[ENTRYPOINT_SCOPE]: bulk operations (single transaction each)
- rail: persistence

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                  |
| :-----: | :----------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `getMany<T = any>(keys: IDBValidKey[], customStore?): Promise<(T \| undefined)[]>` | bulk read | ordered by input keys; a missing key yields `undefined` in position |
|  [02]   | `setMany(entries: [IDBValidKey, any][], customStore?): Promise<void>` | bulk write | ATOMIC — all pairs write or none; the reject for a `set` loop |
|  [03]   | `delMany(keys: IDBValidKey[], customStore?): Promise<void>` | bulk delete    | remove multiple keys in one transaction               |

[ENTRYPOINT_SCOPE]: cursor operations
- rail: persistence

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                          |
| :-----: | :---------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `keys<KeyType extends IDBValidKey>(customStore?): Promise<KeyType[]>` | cursor read | all store keys                                 |
|  [02]   | `values<T = any>(customStore?): Promise<T[]>`                     | cursor read    | all store values                               |
|  [03]   | `entries<KeyType extends IDBValidKey, ValueType = any>(customStore?): Promise<[KeyType, ValueType][]>` | cursor read | all `[key, value]` pairs — the full ordered drain read |
|  [04]   | `clear(customStore?): Promise<void>`                             | cursor delete  | remove all entries                             |

## [04]-[IMPLEMENTATION_LAW]

[IDB_KEYVAL_TOPOLOGY]:
- omitting `customStore` targets the default store (database `keyval-store`, object store `keyval`); pass a `UseStore` from `createStore` to target a named database/store, so store targeting is one parameter, never a per-store op family.
- `setMany` is atomic: if any pair fails, none write — use it over a `set` loop whenever batch atomicity is required.
- `update` performs read AND write inside one `readwrite` transaction, making it the race-free primitive for concurrent increment/append (`prior ?? []` -> `[...prior, item]`); the updater is synchronous (`(old) => next`).
- `promisifyRequest` wraps a raw `IDBRequest<T>`/`IDBTransaction`, resolving on `success`/`complete` and rejecting on `error` — the escape hatch for a compound index or cursor range the wrapper ops do not cover, run inside a `UseStore` callback so it shares the store's transaction.

[INTEGRATION_LAW]:
- one owner, per-domain named stores: `Session/store.md` `LocalPersistence` owns the ONE sanctioned `idb-keyval` site — a frozen `Record<StoreDomain, UseStore>` maps each `StoreDomain` (`snapshot`/`offline-queue`/`auth-flow`/`viewpoint-cache`) to its own `createStore(\`rasm-${domain}\`, domain)` named store, so a `snapshot` `clear` never evicts the `offline-queue` and IndexedDB transaction isolation holds per domain. A per-domain `createStore` scattered at a consumer, a key-prefix convention in one flat `keyval` store, or a hand-rolled `idb-keyval` call outside the `StoreDomain` table is the named flat-store defect.
- stack under `@effect/platform` `KeyValueStore`: the Schema-encoded `lastGood` snapshot composes a custom `KeyValueStore.make({ get, set, remove, clear, size })` adapter over the `snapshot` `UseStore` (each method wrapping the `idb-keyval` op in `Effect.promise`, `get` mapping `T | undefined` through `Option.fromNullable`), so the `@effect/platform` KV abstraction survives over IndexedDB and `Schema.encode`/`Schema.decodeUnknown` round-trips the value. The `@effect/platform-browser` `BrowserKeyValueStore` `layerLocalStorage`/`layerSessionStorage` surface backs NO domain here — it is `localStorage`/`sessionStorage`-backed with NO IndexedDB layer, so it never backs a durable domain.
- atomic drain, race-free enqueue: the offline-queue drain is one atomic `entries`-then-`clear` over the `offline-queue` store (`entries` cursor-reads the full ordered queue, `clear` empties it in the same logical drain), never a per-element `get`/`del` loop a mid-drain crash can half-apply; `enqueue` rides `update` (read-modify-write in one `readwrite` transaction), never a get-then-set race. The drained resolved-intent pairs replay verbatim into the `interchange` `CommandGateway` (`Shell/sync.md` `BackgroundSyncReplay`).
- keys are the full `IDBValidKey` union (`string | number | Date | BufferSource | IDBValidKey[]`); string keys are conventional. `get<T>` returns `T | undefined` and the store cannot distinguish a missing key from a key set to `undefined`, so callers handle the `Option.none` projection at the adapter boundary.

[LOCAL_ADMISSION]:
- create one store per logical domain area (distinct `dbName` + `storeName`) to prevent key collisions and allow independent `clear` calls; a new persisted domain is one `StoreDomain` row and one `Record` entry, never a new store binding scattered at a consumer.
- import the tree-shakeable ESM barrel (`exports["."]` -> `dist/index.js`); never reach for the `compat` bundle.

[RAIL_LAW]:
- Package: `idb-keyval`
- Owns: IndexedDB key-value persistence with a `Promise`-based, `UseStore`-parameterized API
- Accept: `createStore` for per-domain named stores; `setMany` for atomic bulk writes; `update` for race-free read-modify-write; `entries`+`clear` for the atomic drain; `promisifyRequest` as the raw-IDB escape hatch — all behind `LocalPersistence` under a `KeyValueStore.make` adapter
- Reject: direct `IDBDatabase`/`IDBObjectStore`/`localStorage` access when `idb-keyval` covers the case; a `set` loop where `setMany` atomicity is required; a per-element `get`/`del` drain loop; a key-prefix convention in one flat store; any `idb-keyval` call outside the `LocalPersistence` `StoreDomain` table
