# [API_CATALOGUE] idb-keyval

`idb-keyval` supplies a lightweight `Promise`-based key-value store over IndexedDB, with single-entry operations (`get`, `set`, `del`, `update`), bulk operations (`getMany`, `setMany`, `delMany`), cursor operations (`keys`, `values`, `entries`), store management (`createStore`), and a `promisifyRequest` helper for wrapping raw `IDBRequest`/`IDBTransaction` handles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `idb-keyval`
- package: `idb-keyval`
- module: `idb-keyval`
- asset: runtime library
- rail: persistence

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store type
- rail: persistence

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :--------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `UseStore` | type alias    | `<T>(txMode, callback) => Promise<T>` custom store accessor |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: store construction
- rail: persistence

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `createStore(dbName, storeName)` | store factory  | returns `UseStore` for a named IDB store   |
|  [02]   | `promisifyRequest<T>(request)`   | IDB bridge     | promisify `IDBRequest` or `IDBTransaction` |

[ENTRYPOINT_SCOPE]: single-entry operations
- rail: persistence

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `get<T>(key, customStore?)`             | read           | `Promise<T \| undefined>` by key       |
|  [02]   | `set(key, value, customStore?)`         | write          | `Promise<void>` atomic key-value write |
|  [03]   | `del(key, customStore?)`                | delete         | `Promise<void>` remove single key      |
|  [04]   | `update<T>(key, updater, customStore?)` | atomic update  | read-modify-write in one transaction   |

[ENTRYPOINT_SCOPE]: bulk operations
- rail: persistence

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `getMany<T>(keys, customStore?)` | bulk read      | `Promise<T[]>` ordered by input key array |
|  [02]   | `setMany(entries, customStore?)` | bulk write     | atomic multi-key write; all-or-nothing    |
|  [03]   | `delMany(keys, customStore?)`    | bulk delete    | `Promise<void>` remove multiple keys      |

[ENTRYPOINT_SCOPE]: cursor operations
- rail: persistence

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `keys<KeyType extends IDBValidKey>(customStore?)` | cursor read    | `Promise<KeyType[]>` all store keys         |
|  [02]   | `values<T>(customStore?)`                         | cursor read    | `Promise<T[]>` all store values             |
|  [03]   | `entries<KeyType, ValueType>(customStore?)`       | cursor read    | `Promise<[KeyType, ValueType][]>` all pairs |
|  [04]   | `clear(customStore?)`                             | cursor delete  | `Promise<void>` remove all entries          |

## [04]-[IMPLEMENTATION_LAW]

[IDB_KEYVAL_TOPOLOGY]:
- All operations use the default store (database `keyval-store`, object store `keyval`) when `customStore` is omitted; pass a `UseStore` from `createStore` to target a named database/store
- `setMany` is atomic: if any key-value pair fails to write, none are written; use it instead of looping `set` when batch atomicity is required
- `update` performs read and write inside a single `readwrite` transaction, making it safe for concurrent increment/decrement patterns
- `promisifyRequest` wraps a raw `IDBRequest<T>` or `IDBTransaction` and resolves on `success`/`complete`, rejects on `error`

[LOCAL_ADMISSION]:
- `UseStore` is the customization seam; create one store per logical domain area (distinct `dbName` + `storeName` pairs) to prevent key collisions and allow independent `clear` calls
- `LocalPersistence` owns the one sanctioned `idb-keyval` site: a frozen `Record<StoreDomain, UseStore>` table maps each `StoreDomain` (`snapshot`/`offline-queue`/`auth-flow`/`viewpoint-cache`) to its own `createStore(\`rasm-${domain}\`, domain)` named store, so a `snapshot` `clear` never evicts the `offline-queue` and IndexedDB transaction isolation holds per domain. A per-domain `createStore` call scattered at a consumer, a key-prefix convention in one flat `keyval` object store, or a hand-rolled `idb-keyval` call outside the `StoreDomain` table is the named flat-store defect.
- The offline-queue drain is one atomic `entries`-then-`clear` over the `offline-queue` store (the full ordered queue cursor-read, then the store emptied), never a per-element `get`/`del` loop a mid-drain crash can half-apply; `enqueue` rides `update` (read-modify-write in one `readwrite` transaction), never a get-then-set race.
- The Schema-encoded `lastGood` snapshot composes a custom `KeyValueStore.make` adapter over the `snapshot` `UseStore` so the `@effect/platform` `KeyValueStore` abstraction survives over IndexedDB; the `@effect/platform-browser` `BrowserKeyValueStore` `layerLocalStorage`/`layerSessionStorage` surface backs NO domain here — it exposes no IndexedDB layer, so it is never the backing store for a durable domain.
- All key parameters accept the full `IDBValidKey` union (`string | number | Date | ArrayBufferView | ArrayBuffer | IDBValidKey[]`); string keys are the conventional usage
- `get<T>` returns `T | undefined`; callers must handle the undefined case — the store does not distinguish between a missing key and a key explicitly set to `undefined`

[RAIL_LAW]:
- Package: `idb-keyval`
- Owns: IndexedDB key-value persistence with Promise-based API
- Accept: `createStore` for named stores; `setMany` for atomic bulk writes; `update` for read-modify-write
- Reject: direct `IDBDatabase`/`IDBObjectStore` access when `idb-keyval` operations cover the case; separate `set` loop in place of `setMany` when atomicity is required
