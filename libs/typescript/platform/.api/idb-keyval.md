# [API_CATALOGUE] idb-keyval

`idb-keyval` supplies a lightweight `Promise`-based key-value store over IndexedDB, with single-entry operations (`get`, `set`, `del`, `update`), bulk operations (`getMany`, `setMany`, `delMany`), cursor operations (`keys`, `values`, `entries`), store management (`createStore`), and a `promisifyRequest` helper for wrapping raw `IDBRequest`/`IDBTransaction` handles.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `idb-keyval`
- package: `idb-keyval`
- module: `idb-keyval`
- asset: runtime library
- rail: persistence

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: store type
- rail: persistence

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :--------- | :------------ | :---------------------------------------------------------- |
|   [1]   | `UseStore` | type alias    | `<T>(txMode, callback) => Promise<T>` custom store accessor |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: store construction
- rail: persistence

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :------------------------------- | :------------- | :----------------------------------------- |
|   [1]   | `createStore(dbName, storeName)` | store factory  | returns `UseStore` for a named IDB store   |
|   [2]   | `promisifyRequest<T>(request)`   | IDB bridge     | promisify `IDBRequest` or `IDBTransaction` |

[ENTRYPOINT_SCOPE]: single-entry operations
- rail: persistence

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `get<T>(key, customStore?)`             | read           | `Promise<T \| undefined>` by key       |
|   [2]   | `set(key, value, customStore?)`         | write          | `Promise<void>` atomic key-value write |
|   [3]   | `del(key, customStore?)`                | delete         | `Promise<void>` remove single key      |
|   [4]   | `update<T>(key, updater, customStore?)` | atomic update  | read-modify-write in one transaction   |

[ENTRYPOINT_SCOPE]: bulk operations
- rail: persistence

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `getMany<T>(keys, customStore?)` | bulk read      | `Promise<T[]>` ordered by input key array |
|   [2]   | `setMany(entries, customStore?)` | bulk write     | atomic multi-key write; all-or-nothing    |
|   [3]   | `delMany(keys, customStore?)`    | bulk delete    | `Promise<void>` remove multiple keys      |

[ENTRYPOINT_SCOPE]: cursor operations
- rail: persistence

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------------ | :------------- | :------------------------------------------ |
|   [1]   | `keys<KeyType extends IDBValidKey>(customStore?)` | cursor read    | `Promise<KeyType[]>` all store keys         |
|   [2]   | `values<T>(customStore?)`                         | cursor read    | `Promise<T[]>` all store values             |
|   [3]   | `entries<KeyType, ValueType>(customStore?)`       | cursor read    | `Promise<[KeyType, ValueType][]>` all pairs |
|   [4]   | `clear(customStore?)`                             | cursor delete  | `Promise<void>` remove all entries          |

## [4]-[IMPLEMENTATION_LAW]

[IDB_KEYVAL_TOPOLOGY]:
- All operations use the default store (database `keyval-store`, object store `keyval`) when `customStore` is omitted; pass a `UseStore` from `createStore` to target a named database/store
- `setMany` is atomic: if any key-value pair fails to write, none are written; use it instead of looping `set` when batch atomicity is required
- `update` performs read and write inside a single `readwrite` transaction, making it safe for concurrent increment/decrement patterns
- `promisifyRequest` wraps a raw `IDBRequest<T>` or `IDBTransaction` and resolves on `success`/`complete`, rejects on `error`

[LOCAL_ADMISSION]:
- `UseStore` is the customization seam; create one store per logical domain area (distinct `dbName` + `storeName` pairs) to prevent key collisions and allow independent `clear` calls
- All key parameters accept the full `IDBValidKey` union (`string | number | Date | ArrayBufferView | ArrayBuffer | IDBValidKey[]`); string keys are the conventional usage
- `get<T>` returns `T | undefined`; callers must handle the undefined case — the store does not distinguish between a missing key and a key explicitly set to `undefined`

[RAIL_LAW]:
- Package: `idb-keyval`
- Owns: IndexedDB key-value persistence with Promise-based API
- Accept: `createStore` for named stores; `setMany` for atomic bulk writes; `update` for read-modify-write
- Reject: direct `IDBDatabase`/`IDBObjectStore` access when `idb-keyval` operations cover the case; separate `set` loop in place of `setMany` when atomicity is required
