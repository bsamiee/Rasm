# [@duckdb/duckdb-wasm] — the browser OLAP engine: Web-Worker DuckDB with Arrow-native results behind the `lane/olap` wasm row

`@duckdb/duckdb-wasm` runs the full DuckDB engine in a Web Worker — the client-side analytics row that pushes compute to the browser over HTTP-range-read remote Parquet instead of shipping rows through a service. Results are Arrow-native (`query()` returns an `arrow.Table`), ingestion accepts Arrow tables/IPC streams, CSV, JSON, and registered file handles, and OPFS backs durable tables. Self-hosted bundles are the deployment law (the strict CSP forbids CDN loads); the worker split, single-threaded default, and CORS-bound range reads are the row's degradation coordinates.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@duckdb/duckdb-wasm`
- package: `@duckdb/duckdb-wasm`
- version: `1.33.1-dev57.0`
- license: `MIT`
- peer: `apache-arrow` (`.api/apache-arrow.md`) — the result and ingest wire
- runtime: browser Web Worker (`AsyncDuckDB` protocol over `postMessage`); the node row is `@duckdb/node-api` (`.api/duckdb-node-api.md`)
- rail: `lane/olap` wasm row — no Effect peer; boundary-kernel wrap is the lane's

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                        |
| :-----: | :-------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `AsyncDuckDB`                                                   | engine handle   | main-thread proxy over the worker-resident engine            |
|  [02]   | `DuckDBBundles` / bundle (`mainModule`, `mainWorker`, `pthreadWorker`) | bundle roster | self-hosted `mvp`/`eh` artifact coordinates             |
|  [03]   | `ConsoleLogger`                                                 | logger          | engine log sink handed to the constructor                    |
|  [04]   | connection (from `db.connect()`)                                | session handle  | `query`/`send`/insert members; closed to release memory      |
|  [05]   | `DuckDBDataProtocol` (`HTTP`, `BROWSER_FILEREADER`)             | file protocol   | `registerFileHandle`/`registerFileURL` residency discriminant |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                          |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `selectBundle(bundles)` → `new Worker(bundle.mainWorker)` → `new AsyncDuckDB(logger, worker)` → `db.instantiate(bundle.mainModule, bundle.pthreadWorker)` | engine acquire | the lane's scoped acquire; self-hosted bundles only |
|  [02]   | `db.connect()` → connection; `conn.close()` / `db.terminate()`                              | session lease  | scoped connection; release arms                  |
|  [03]   | `conn.query<T>(sql)` → `arrow.Table<T>`                                                     | materialize    | Arrow-native result — zero-copy into the viewer  |
|  [04]   | `for await (const batch of await conn.send<T>(sql))`                                        | stream read    | lazy record-batch pull — the lane's `Stream` lift |
|  [05]   | `conn.insertArrowTable(table, { name })` / `conn.insertArrowFromIPCStream(bytes, { name })` | arrow ingest   | the ONE columnar wire inbound                    |
|  [06]   | `db.registerFileURL(name, url, DuckDBDataProtocol.HTTP, direct)` / `registerFileHandle(name, file, DuckDBDataProtocol.BROWSER_FILEREADER, direct)` / `registerFileText` / `registerFileBuffer` | file registry | remote Parquet range reads; picked local files |
|  [07]   | `conn.insertCSVFromPath(path, options)` / `conn.insertJSONFromPath(path, options)`          | typed ingest   | schema-typed CSV/JSON admission                  |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Boundary-kernel wrap: instantiation and connections ride `Effect.acquireRelease` under `Scope`; queries lift through `Effect.tryPromise`; `send` batches lift to `Stream` at the lane seam.
- Arrow is the wire (`.api/apache-arrow.md`): `query()` results land as `arrow.Table` and flow to the viewer's geoarrow plane without row materialization; IPC-stream ingest mirrors it inbound.
- Deployment law: bundles self-host beside the app shell — `selectBundle` over owned artifact URLs; the CDN pattern is rejected by CSP.

[LOCAL_ADMISSION]:
- Single-threaded by default; threads demand cross-origin isolation — a deployment fact, not a code branch.
- HTTP-range Parquet reads are CORS-bound; the object plane's presigned grants are the sanctioned remote source.
- Browser analytics is an accelerator over server-minted data, never a record of truth.

[RAIL_LAW]:
- Package: `@duckdb/duckdb-wasm`
- Owns: the browser analytical engine — worker instantiation, Arrow-native query/ingest, file registry, streamed batches
- Accept: self-hosted bundles, scoped lifecycle wrap, Arrow interchange, range-read remote Parquet
- Reject: CDN bundle loads, main-thread engine residency, row-materialized interchange, browser analytics as authority
