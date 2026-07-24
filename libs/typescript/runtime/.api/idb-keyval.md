# [TS_RUNTIME_API_IDB_KEYVAL]

Free-function key-value over one IndexedDB object store: each op closes over an optional `UseStore` runner `createStore` mints, `promisifyRequest` lifts a raw request or transaction to a promise, and an omitted store resolves the lazy `keyval-store`/`keyval` default.

`setMany`, `delMany`, and `update` run atomically in one transaction, `update` a read-modify-write surviving concurrent writers. This is the cache lane behind `browser/persist.md`'s `EventLog` overlay — values structured-cloneable, never the record of truth.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `idb-keyval`
- package: `idb-keyval` (Apache-2.0)
- module: `.` ESM `./dist/index.js` / CJS `./dist/index.cjs`, types `./dist/index.d.ts` (`type: module`)
- runtime: browser or Worker with `indexedDB`; absent in a bare Node runtime; zero dependencies
- rail: persist-kv

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transaction-runner every op threads

[USE_STORE]: `UseStore = <T>(IDBTransactionMode, (IDBObjectStore) => T | PromiseLike<T>) -> Promise<T>` — runs one callback inside a fresh transaction of the requested mode.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: each KV op is a module-level async function closing over an optional `UseStore`; `createStore(string, string) -> UseStore` opens or upgrades a named database and object store and returns the runner, and `promisifyRequest(IDBRequest<T> | IDBTransaction) -> Promise<T>` bridges the raw requests the custom cursors and indexes higher lanes need.

| [INDEX] | [SURFACE]                                                                | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------------- | :----------------------- |
|  [01]   | `get(IDBValidKey, UseStore?) -> Promise<T \| undefined>`                 | single read              |
|  [02]   | `getMany(IDBValidKey[], UseStore?) -> Promise<(T \| undefined)[]>`       | batch read               |
|  [03]   | `set(IDBValidKey, any, UseStore?) -> Promise<void>`                      | single write             |
|  [04]   | `setMany([IDBValidKey, any][], UseStore?) -> Promise<void>`              | atomic batch write       |
|  [05]   | `update(IDBValidKey, (T \| undefined) => T, UseStore?) -> Promise<void>` | atomic read-modify-write |
|  [06]   | `del(IDBValidKey, UseStore?) -> Promise<void>`                           | single delete            |
|  [07]   | `delMany(IDBValidKey[], UseStore?) -> Promise<void>`                     | atomic batch delete      |
|  [08]   | `clear(UseStore?) -> Promise<void>`                                      | store wipe               |
|  [09]   | `keys(UseStore?) -> Promise<K[]>`                                        | key scan                 |
|  [10]   | `values(UseStore?) -> Promise<T[]>`                                      | value scan               |
|  [11]   | `entries(UseStore?) -> Promise<[K, V][]>`                                | pair scan                |

- `update`: reads and writes in one open transaction, so its updater stays synchronous — an await lets the tx close.

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `effect`(`.api/effect.md`): own the store as one `Layer` over a single `createStore` `UseStore` resolved once at construction; every op becomes an `Effect.tryPromise` mapping the rejection (`DOMException` quota/abort, absent `indexedDB`) onto one typed `KvError`, and `set` composes `Schema.encode(V)` while `get`/`getMany`/`values`/`entries` compose `Schema.decode(V)`, so `V` is the schema `Encoded` and no raw promise or `any` escapes the boundary.
- `nuqs`(`.api/nuqs.md`): `set(routeKey, serialized)` on each `createSerializer` write and `get(routeKey)` on cold boot persist the last-good query string per route key, so `createLoader` re-decodes typed query-state before the Navigation API resolves the entry — the value is a cloneable `string`, identity-encoded.
- `browser/*`: `browser/shell.md`'s background-sync outbox enumerates pending frames with `keys`/`values`, re-hydrates atomically with `setMany`, and evicts acknowledged frames with `delMany` in one tx; `browser/boot.md` reads and writes connectivity and last-sync markers, `promisifyRequest` backing the OPFS cursor scans; this lane is the overlay cache beside `browser/persist.md`'s `EventLog`, so divergence is a cache-invalidate.

[LOCAL_ADMISSION]:
- Collapse the arities behind one polymorphic `kv` entry discriminating on request shape — one key or many, read or write or mutate — onto the atomic batch ops.

[RAIL_LAW]:
- Package: `idb-keyval`
- Owns: browser IndexedDB key-value cache — single and batch reads, writes, and deletes, atomic batch and read-modify-write mutation, and key/value/entry scans over one object store.
- Accept: one `Layer` over a single `createStore` `UseStore`, every op an `Effect.tryPromise` folding rejection to a typed `KvError`, values `Schema`-encoded at the boundary, arities collapsed behind one polymorphic `kv` entry.
- Reject: a raw promise or `any`-typed value escaping the boundary, per-op method names downstream, this lane standing as the record of truth behind the `EventLog` overlay.
