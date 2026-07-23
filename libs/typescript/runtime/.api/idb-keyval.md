# [TS_RUNTIME_API_IDB_KEYVAL]

Eleven promise-returning KV ops over one IndexedDB object store each close over an optional `UseStore` transaction-runner, and the synchronous `createStore(dbName, storeName)` factory mints one beside the `promisifyRequest` raw-request bridge — free functions, no class and no singleton handle. Omitting `customStore` targets a lazily-created default `keyval-store`/`keyval` store, so the store roster is a value, never a parallel function family.

`setMany`, `delMany`, and `update` run inside a single IndexedDB transaction and are atomic — an `update` is a read-modify-write in one tx that survives concurrent writers. Values must be structured-cloneable: the `browser/persist.md` Layer encodes domain values through `Schema` to a cloneable shape at the boundary and decodes on read. This is the local-first cache lane, never the record of truth behind `browser/persist.md`'s `EventLog` overlay.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `idb-keyval`
- package: `idb-keyval` (Apache-2.0)
- entry: `.` -> ESM `./dist/index.js`, CJS `./dist/index.cjs`, types `./dist/index.d.ts` (`type: module`)
- variants: `./dist/compat.*` (retired implicit default-store build, re-exports `./`), `./dist/umd.*` (global build)
- asset: browser-only KV over the native `IndexedDB` API; zero dependencies; structured-clone value domain
- runtime: `window`/Worker with `indexedDB`; absent in a bare Node runtime
- rail: persist-kv

## [02]-[KV_OPERATIONS]

[PUBLIC_TYPE_SCOPE]: single-key and batch operations — rail: persist-kv

| [INDEX] | [SYMBOL]  | [ARITY]        | [ATOMICITY]          | [CONSUMER]                  |
| :-----: | :-------- | :------------- | :------------------- | :-------------------------- |
|  [01]   | `get`     | single read    | single tx            | cache hit lookup            |
|  [02]   | `getMany` | batch read     | single tx            | prefetch / hydration        |
|  [03]   | `set`     | single write   | single tx            | cache write                 |
|  [04]   | `setMany` | batch write    | atomic (all-or-none) | bulk hydrate / sync replay  |
|  [05]   | `update`  | read-mod-write | atomic RMW           | counter / queue mutation    |
|  [06]   | `del`     | single delete  | single tx            | eviction                    |
|  [07]   | `delMany` | batch delete   | atomic               | bulk eviction               |
|  [08]   | `clear`   | store wipe     | single tx            | cache reset                 |
|  [09]   | `keys`    | key scan       | single tx            | queue enumeration           |
|  [10]   | `values`  | value scan     | single tx            | background-sync replay body |
|  [11]   | `entries` | pair scan      | single tx            | full-store export           |

[SURFACES]: `get(IDBValidKey,UseStore?) -> Promise<T|undefined>` `getMany(IDBValidKey[],UseStore?) -> Promise<(T|undefined)[]>` `set(IDBValidKey,any,UseStore?) -> Promise<void>` `setMany([IDBValidKey,any][],UseStore?) -> Promise<void>` `update(IDBValidKey,(oldValue:T|undefined)=>T,UseStore?) -> Promise<void>` `del(IDBValidKey,UseStore?) -> Promise<void>` `delMany(IDBValidKey[],UseStore?) -> Promise<void>` `clear(UseStore?) -> Promise<void>` `keys(UseStore?) -> Promise<K[]>` `values(UseStore?) -> Promise<T[]>` `entries(UseStore?) -> Promise<[K,V][]>`

## [03]-[STORE_PARAMETERIZATION]

[PUBLIC_TYPE_SCOPE]: the `UseStore` transaction-runner axis — rail: persist-kv

`createStore` is the whole multi-store mechanism: it opens or upgrades a named database and object store and returns a `UseStore` closure that runs a callback inside a fresh transaction of the requested mode. The Layer resolves the closure once at construction and closes every operation over it, never per call. `promisifyRequest` is the raw-request bridge for the custom cursors and indexes the higher lanes need.

[USE_STORE]: `UseStore = <T>(txMode:IDBTransactionMode,// "readonly"|"readwrite"|…`
[SURFACES]: `createStore(string,string) -> UseStore` `promisifyRequest(IDBRequest<T>|IDBTransaction) -> Promise<T>`

## [04]-[INTEGRATION]

[STACK]: `idb-keyval` -> `effect` rails + `Schema` + `Layer` — rail: persist-kv
- Own the store as one Layer over a single `createStore(dbName, storeName)` `UseStore`; every op becomes an `Effect.tryPromise` that maps the rejection (`DOMException` quota/abort, absent `indexedDB`) into one typed `KvError` rail, so no raw promise escapes the boundary. `update`'s `updater` stays synchronous so the RMW tx is not held open across an await.
- Parse-not-validate at the edge: `set` composes `Schema.encode(V)` to a structured-cloneable encoded value, and `get`/`getMany`/`values`/`entries` compose `Schema.decode(V)` on read, so the Layer's public surface is domain-typed while the stored bytes are canonical — the KV `V` type parameter is the schema `Encoded`, never `any`.
- Collapse the scan/read/write/delete arities behind one polymorphic `kv` entry that discriminates on request shape (one key or many, read or write or mutate) onto the atomic batch ops, rather than exposing eleven method names downstream.

[STACK]: sibling `browser` lanes — rail: persist-kv
- `browser/shell.md` background-sync replay drains a durable outbox: `keys`/`values` enumerate pending frames, `setMany` re-hydrates atomically after a flush, and `delMany` evicts acknowledged frames in one tx.
- `browser/persist.md`'s `Overlay` cluster carries the `EventLog` backings and the sqlite-wasm lane seam; this KV lane is the overlay cache beside them, so divergence is a cache-invalidate, never data loss.
- `browser/boot.md` reads and writes connectivity and last-sync markers here so a cold boot restores session state before the network settles; `promisifyRequest` backs any cursor scan the OPFS lane needs over a shared store.
- `browser/route.md` (the `nuqs` `browser/persist` consumer) persists the last-good serialized query string per route key: `set(routeKey, serialized)` on each `createSerializer` write, and `get(routeKey)` on cold boot so `nuqs`'s `createLoader` re-decodes typed query-state before the Navigation API resolves the entry. The value is already a cloneable `string`, so this lane's `V` is `Schema.String` (identity encode), never a payload.
