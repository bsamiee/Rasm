# [TS_DATA_API_DUCKDB_DUCKDB_WASM]

`@duckdb/duckdb-wasm` runs the full DuckDB engine in a Web Worker — the browser-side analytical row that pushes compute to the client over HTTP-range reads of remote Parquet instead of shipping rows through a service. Results and ingest are Arrow-native, OPFS backs durable tables, and self-hosted bundles are the deployment law the strict CSP demands.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@duckdb/duckdb-wasm`
- package: `@duckdb/duckdb-wasm` (MIT)
- peer: `apache-arrow` (`.api/apache-arrow.md`) — the result and ingest wire
- runtime: browser Web Worker (`AsyncDuckDB` protocol over `postMessage`); the node row is `@duckdb/node-api` (`.api/duckdb-node-api.md`)
- rail: `lane/olap` wasm row — no Effect peer; boundary-kernel wrap is the lane's

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the worker-resident engine, its self-hosted bundle coordinates, and the file-residency protocol

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]  | [CONSUMER]                                                    |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `AsyncDuckDB`                                       | engine handle  | main-thread proxy over the worker-resident engine             |
|  [02]   | `DuckDBBundles` / `DuckDBBundle`                    | bundle roster  | self-hosted `mvp`/`eh` artifact coordinates                   |
|  [03]   | `ConsoleLogger`                                     | logger         | engine log sink handed to the constructor                     |
|  [04]   | connection (from `db.connect()`)                    | session handle | closed to release memory                                      |
|  [05]   | `DuckDBDataProtocol` (`HTTP`, `BROWSER_FILEREADER`) | file protocol  | `registerFileHandle`/`registerFileURL` residency discriminant |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scoped engine acquire, query, and ingest
- Acquire runs `selectBundle(bundles)` → `new Worker(bundle.mainWorker)` → `new AsyncDuckDB(logger, worker)` → `db.instantiate(bundle.mainModule, bundle.pthreadWorker)`; every read and ingest below is a `conn` member.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CONSUMER]                                          |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `selectBundle` → `AsyncDuckDB` → `db.instantiate`              | engine acquire | the lane's scoped acquire; self-hosted bundles only |
|  [02]   | `db.connect()` → connection; `conn.close()` / `db.terminate()` | session lease  | scoped connection; release arms                     |
|  [03]   | `query<T>(sql)` → `arrow.Table<T>`                             | materialize    | Arrow-native result — zero-copy into the viewer     |
|  [04]   | `for await (const batch of await send<T>(sql))`                | stream read    | lazy record-batch pull — the lane's `Stream` lift   |
|  [05]   | `insertArrowTable(table, { name })`                            | arrow ingest   | the ONE columnar wire inbound                       |
|  [06]   | `insertArrowFromIPCStream(bytes, { name })`                    | arrow ingest   | IPC-stream columnar ingest                          |
|  [07]   | `registerFileHandle` / `registerFileURL`                       | file registry  | remote Parquet range reads; picked local files      |
|  [08]   | `insertCSVFromPath` / `insertJSONFromPath` `(path, options)`   | typed ingest   | schema-typed CSV/JSON admission                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Engine resides in the worker; `AsyncDuckDB` proxies it over `postMessage`, so every member returns a promise the lane lifts.
- Self-hosted bundles are the sole load path — `selectBundle` resolves owned artifact URLs; CSP forecloses the CDN load.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): `query<T>()` returns `arrow.Table<T>` and `send<T>()` yields an `arrow.AsyncRecordBatchStreamReader<T>` that lifts through `Stream.fromAsyncIterable`; inbound, a live `arrow.Table` rides `insertArrowTable` and IPC bytes ride `insertArrowFromIPCStream`.
- `lane/olap`: instantiation and connection ride `Effect.acquireRelease` under `Scope`, `query` lifts through `Effect.tryPromise`, and `send` batches lift to `Stream` at the lane seam.

[LOCAL_ADMISSION]:
- Single-threaded by default; threads demand cross-origin isolation, a deployment fact rather than a code branch.
- HTTP-range Parquet reads are CORS-bound; presigned object-plane grants are the sanctioned remote source.
- Browser analytics accelerates server-minted data, never records truth.

[RAIL_LAW]:
- Package: `@duckdb/duckdb-wasm`
- Owns: the browser analytical engine — worker instantiation, Arrow-native query/ingest, file registry, streamed batches
- Accept: self-hosted bundles, scoped lifecycle wrap, Arrow interchange, range-read remote Parquet
- Reject: CDN bundle loads, main-thread engine residency, row-materialized interchange, browser analytics as authority
