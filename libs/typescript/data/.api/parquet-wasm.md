# [TS_DATA_API_PARQUET_WASM]

`parquet-wasm` owns engine-free Parquet decode and encode at the lake-at-rest edge, round-tripping `apache-arrow` `Table` values through Parquet bytes with no analytical engine booted. Parquet is the format at rest, Arrow IPC the in-memory wire, and this codec the seam on `lane/olap`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `parquet-wasm`
- package: `parquet-wasm` (MIT)
- module: `exports["."]` condition-selects the synchronous `node` build (wasm inlined) against the async `esm`/`bundler` default; `./node`, `./esm`, `./bundler` name each build explicitly
- runtime: both lanes — the `node` build rides the `@duckdb/node-api` server lane, the async build the `@duckdb/duckdb-wasm` browser lane beside the worker engine
- rail: `lane/olap` `[06]-[ARROW_WIRE]` at the parquet-at-rest edge — every read decodes to Arrow IPC, every write accepts it

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the wasm-backed containers and the writer-policy vocabulary

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]  | [CAPABILITY]                      |
| :-----: | :------------------------------------ | :------------- | :-------------------------------- |
|  [01]   | `Table`                               | wasm container | the parquet↔Arrow codec value     |
|  [02]   | `ParquetFile`                         | async reader   | lazy row-group/column projection  |
|  [03]   | `Schema` / `RecordBatch`              | frame shape    | decoded shape; per-batch grain    |
|  [04]   | `WriterProperties(Builder)`           | write policy   | write-policy tuning surface       |
|  [05]   | `ParquetMetaData` family              | footer census  | footer stats without a full read  |
|  [06]   | `FFIStream` / `FFIData` / `FFISchema` | C Data export  | zero-copy Arrow handoff           |
|  [07]   | `ReaderOptions`                       | read shape     | batch/projection/limit read knobs |

- `Table`, `ParquetFile`, `Schema`, `RecordBatch`, and each metadata class are `!Send` heap resources carrying `free()` and `[Symbol.dispose]`; `ParquetMetaData` spans `FileMetaData`, `RowGroupMetaData`, `ColumnChunkMetaData`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parquet decode, encode, and the streaming reader/writer

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------ | :------ | :--------------------------------- |
|  [01]   | `readParquet(bytes, options?) -> Table`           | static  | decode a whole buffer to a Table   |
|  [02]   | `writeParquet(table, props?) -> Uint8Array`       | static  | encode a Table to parquet bytes    |
|  [03]   | `readSchema(bytes) -> Schema`                     | static  | schema without column decode       |
|  [04]   | `ParquetFile.fromUrl(url)` / `.fromFile(blob)`    | factory | range-request or `Blob` reader     |
|  [05]   | `readParquetStream(url)` / `ParquetFile.stream()` | static  | bounded-memory `RecordBatch` pull  |
|  [06]   | `transformParquetStream(stream, props?)`          | static  | encode a `RecordBatch` stream out  |
|  [07]   | `Table.fromIPCStream(buf)` / `.intoIPCStream()`   | factory | the `apache-arrow` round-trip seam |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Parquet's own `Table` never leaves the codec as a value — the IPC `Uint8Array` or the `FFIStream` is the only egress, so a wasm-backed container meets `apache-arrow`'s across the stream buffer or the Arrow C Data Interface, never by shared class identity.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): read folds `readParquet(bytes).intoIPCStream()` into `tableFromIPC`; write folds `tableToIPC(table, "stream")` into `Table.fromIPCStream(bytes)` then `writeParquet`; `toFFI()`/`intoFFI()` cross the Arrow C Data Interface zero-copy, `toFFI` keeping the source alive for its `free()` and `intoFFI` consuming it.
- `@duckdb/duckdb-wasm`(`.api/duckdb-duckdb-wasm.md`): lake parquet bytes are content-addressed objects — a read pulls the object then `readParquet`s it, a write hashes the `writeParquet` output into the store, and large objects ride `ParquetFile.fromUrl` range requests with `stream()` so the browser never materializes a whole file.
- `lane/olap`: writer policy is one `WriterPropertiesBuilder` chain — `setCompression(Compression.ZSTD)`, `setStatisticsEnabled(EnabledStatistics.Page)`, `setMaxRowGroupSize(n)`, `build()` — policy on one owner, never per-call flags; `Olap.lake.read`/`.write`/`.batches`/`.sink` bind `readParquet`/`writeParquet`/`stream`/`transformParquetStream`.

[LOCAL_ADMISSION]:
- Every wasm handle acquires through `Effect.acquireRelease` releasing `free()`, or `Effect.addFinalizer` over `[Symbol.dispose]`; a `Table` escaping its scope is rejected.
- Each async build resolves `initWasm()` once at `Layer` construction, proven before any entry; the `node` build skips it, so the lane owner selects the build by runtime, never at the call site.
- Unbounded lake objects ride `ParquetFile.stream`/`readParquetStream` lifted through `Stream.fromReadableStream`; `readParquet` whole-buffer decode admits only where the object is provably bounded.

[RAIL_LAW]:
- Package: `parquet-wasm`
- Owns: engine-free Parquet decode, encode, and the streaming reader/writer at the lake-at-rest edge, round-tripping `apache-arrow` Tables over IPC or the Arrow C Data Interface
- Accept: the parquet↔Arrow-IPC round-trip, `WriterPropertiesBuilder` policy, `ParquetFile` range-request and stream reads, `FFIStream` zero-copy handoff, scoped `free()` on every handle
- Reject: a DuckDB or Flight engine booted only to transcode parquet, parquet's own `Table` leaking as a value, an unscoped wasm handle, a second columnar codec, row-object re-encoding between lake and wire
