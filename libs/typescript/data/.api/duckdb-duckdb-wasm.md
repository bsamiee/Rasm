# [TS_DATA_API_DUCKDB_DUCKDB_WASM]

`@duckdb/duckdb-wasm` runs the full DuckDB engine in a Web Worker — the browser-side analytical row that pushes compute to the client over HTTP-range reads of remote Parquet instead of shipping rows through a service. Results and ingest are Arrow-native, OPFS backs durable tables, and self-hosted bundles are the deployment law the strict CSP demands.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the worker-resident engine, its self-hosted bundle coordinates, and the file-residency protocol

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]  | [CONSUMER]                                                    |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `AsyncDuckDB`                                 | engine handle  | main-thread proxy over the worker-resident engine             |
|  [02]   | `DuckDBBundles` / `DuckDBBundle`              | bundle roster  | self-hosted `mvp`/`eh` artifact coordinates                   |
|  [03]   | `ConsoleLogger`                               | logger         | engine log sink handed to the constructor                     |
|  [04]   | `AsyncDuckDBConnection` (from `db.connect()`) | session handle | `bindings` reaches its engine; `close()` releases memory      |
|  [05]   | `AsyncPreparedStatement<T>`                   | bind surface   | the ONE parameterized path — `query`/`send` take `...params`  |
|  [06]   | `DuckDBDataProtocol`                          | file protocol  | `registerFileHandle`/`registerFileURL` residency discriminant |

- `DuckDBDataProtocol` carries six arms — `BUFFER`, `NODE_FS`, `BROWSER_FILEREADER`, `BROWSER_FSACCESS`, `HTTP`, `S3`; `lane/olap` admits `HTTP` for a presigned grant and `BROWSER_FILEREADER` for a picked file, and the remaining four stay unrostered because no browser session holds a node filesystem, an S3 credential, or a file-system access handle.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scoped engine acquire, query, and ingest
- Acquire runs `selectBundle(bundles)` → `new Worker(bundle.mainWorker)` → `new AsyncDuckDB(logger, worker)` → `db.instantiate(bundle.mainModule, bundle.pthreadWorker)`; every read and ingest below is a `conn` member.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CONSUMER]                                          |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `selectBundle` → `AsyncDuckDB` → `db.instantiate`              | engine acquire | the lane's scoped acquire; self-hosted bundles only |
|  [02]   | `db.connect()` → connection; `conn.close()` / `db.terminate()` | session lease  | scoped connection; release arms                     |
|  [03]   | `query<T>(sql)` → `arrow.Table<T>`                             | materialize    | Arrow-native result — zero-copy into the viewer     |
|  [04]   | `for await (const batch of await send<T>(sql))`                | stream read    | lazy record-batch pull — the lane's `Stream` lift   |
|  [05]   | `prepare<T>(sql)` → `AsyncPreparedStatement<T>`                | bind seam      | binds cross here alone; `close()` on every exit     |
|  [06]   | `stmt.query(...params)` / `stmt.send(...params)`               | bound read     | the prepared twins of rows [03] and [04]            |
|  [07]   | `conn.bindings` → `AsyncDuckDB`                                | engine reach   | the file registry off a leased session              |
|  [08]   | `insertArrowTable(table, { name })`                            | arrow ingest   | the ONE columnar wire inbound                       |
|  [09]   | `insertArrowFromIPCStream(bytes, { name })`                    | arrow ingest   | IPC-stream columnar ingest                          |
|  [10]   | `registerFileHandle` / `registerFileURL`                       | file registry  | remote Parquet range reads; picked local files      |
|  [11]   | `registerFileBuffer` / `registerEmptyFileBuffer`               | file registry  | bytes in hand; a named sink an export writes into   |
|  [12]   | `registerFileText` / `registerOPFSFileName`                    | file registry  | inline text residency; OPFS-backed durable file     |
|  [13]   | `dropFile(name)` / `dropFiles()`                               | file release   | the scoped drop a per-view registration owes        |
|  [14]   | `globFiles(pattern)`                                           | file listing   | the registry's own roster read                      |
|  [15]   | `copyFileToBuffer` / `copyFileToPath` `(name, …)`              | file egress    | reading a registered or engine-written file back    |
|  [16]   | `insertCSVFromPath` / `insertJSONFromPath` `(path, options)`   | typed ingest   | schema-typed CSV/JSON admission                     |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Engine resides in the worker; `AsyncDuckDB` proxies it over `postMessage`, so every member returns a promise the lane lifts.
- Self-hosted bundles are the sole load path — `selectBundle` resolves owned artifact URLs; CSP forecloses the CDN load.
- This build registers FILES and COLUMNS, never FUNCTIONS: no table-function surface is exported at all, and `createScalarFunction` reaches only the unexported synchronous bindings, so a source that becomes a relation through a registered scan on the node row becomes one here through the file registry or an Arrow insert.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): `query<T>()` returns `arrow.Table<T>` and `send<T>()` yields an `arrow.AsyncRecordBatchStreamReader<T>` that lifts through `Stream.fromAsyncIterable`; inbound, a live `arrow.Table` rides `insertArrowTable` and IPC bytes ride `insertArrowFromIPCStream`.
- `lane/olap`: instantiation and connection ride `Effect.acquireRelease` under `Scope`, `query` lifts through `Effect.tryPromise`, and `send` batches lift to `Stream` at the lane seam.
- `lane/olap`: this engine mints the SAME leased-session handle the node row mints, so the browser lane answers one bulkhead, one budget, one replay rule, and one engine-tagged meter fan rather than a private read path.

[LOCAL_ADMISSION]:
- `query` and `send` take TEXT alone, so every parameterized read crosses `prepare` and its statement closes on each exit; splicing a value into the SQL is the injection surface the bind seam exists to delete, and admission text stays bind-free because a multi-statement `INSTALL`/`LOAD` prepares nowhere.
- Single-threaded by default; threads demand cross-origin isolation, a deployment fact rather than a code branch.
- HTTP-range Parquet reads are CORS-bound; presigned object-plane grants are the sanctioned remote source.
- Browser analytics accelerates server-minted data, never records truth.
