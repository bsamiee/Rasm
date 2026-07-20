# [TS_DATA_API_PARQUET_WASM]

`parquet-wasm` is the durable lake codec the Arrow wire lacks: Rust `parquet`+`arrow` compiled to WebAssembly, reading and writing Parquet bytes round-tripping `apache-arrow` `Table` values with no analytical engine instantiated. Parquet is the format at rest, Arrow IPC the in-memory wire, and this package the seam; its own wasm-backed `Table`/`Schema`/`RecordBatch` meet `apache-arrow`'s only across the IPC stream buffer or the Arrow C Data Interface, never by shared class identity.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `parquet-wasm`
- package: `parquet-wasm`
- license: `MIT OR Apache-2.0`
- module: `exports["."]` condition-selects the `node` build (wasm inlined, synchronous) vs the default `esm` build (async `initWasm()`); `./node`, `./esm`, `./bundler` name the builds explicitly, and `./<build>/parquet_wasm_bg.wasm` exposes the raw asset for a hosted-URL init
- peers: none; dependencies: none — the wasm binary is self-contained
- init: default export `initWasm(input?)` instantiates the module for the `esm`/`bundler` builds and MUST resolve before any entry runs; the `node` build needs no init
- runtime: both lanes — the `node` build rides the `@duckdb/node-api` server lane, the `esm`/`bundler` build the `@duckdb/duckdb-wasm` browser lane beside the worker engine
- rail: `lane/olap` `[06]-[ARROW_WIRE]` at the parquet-at-rest edge — every read decodes to Arrow IPC, every write accepts it

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the wasm-backed container and its writer-policy vocabulary
- rail: lane/olap
- `Table` batches columns behind the wasm heap; `fromIPCStream(buf)` ingests an `apache-arrow` IPC stream buffer and `intoIPCStream()` emits one, the sole shared-identity crossing; `toFFI()`/`intoFFI()` export the Arrow C Data Interface (`FFIStream`) for a zero-copy handoff that skips IPC serialization. `Table`, `ParquetFile`, `Schema`, `RecordBatch`, and every metadata class are `!Send` wasm resources holding heap memory — each carries `free()` and `[Symbol.dispose]`, so each rides an `Effect` scope.

[PUBLIC_TYPE_NOTE]: the `ParquetMetaData` family spans `FileMetaData`, `RowGroupMetaData`, and `ColumnChunkMetaData`.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]  | [CONSUMER]                                               |
| :-----: | :------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `Table`                               | wasm container | `readParquet` output; `writeParquet` input; IPC crossing |
|  [02]   | `ParquetFile`                         | async reader   | URL/`Blob`-backed lazy row-group and column projection   |
|  [03]   | `Schema` / `RecordBatch`              | frame shape    | `readSchema` egress; per-batch streaming grain           |
|  [04]   | `WriterProperties(Builder)`           | write policy   | compression, encoding, row-group, statistics tuning      |
|  [05]   | `ParquetMetaData` family              | footer census  | row counts, sizes, per-column stats without a full read  |
|  [06]   | `FFIStream` / `FFIData` / `FFISchema` | C Data export  | zero-copy Arrow handoff bypassing IPC serialization      |
|  [07]   | `ReaderOptions`                       | read shape     | batch/row-group/limit/offset/column/concurrency knobs    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parquet decode, encode, and the streaming reader/writer
- rail: lane/olap
- `readParquet(bytes, options?)` decodes a whole buffer to a wasm `Table`; `writeParquet(table, writerProperties?)` encodes one back to parquet `Uint8Array`; `readSchema(bytes)` pulls the schema alone. Streaming rides web `ReadableStream`: `readParquetStream(url, contentLength?)` and `ParquetFile.stream(options?)` yield `RecordBatch` incrementally, and `transformParquetStream(stream, writerProperties?)` is the stream-in/stream-out encoder for bounded-memory writes.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [CONSUMER]                                              |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `readParquet(bytes, options?)` → `Table`             | decode         | `Olap.lake.read` — parquet buffer into a wasm Table     |
|  [02]   | `writeParquet(table, props?)` → `Uint8Array`         | encode         | `Olap.lake.write` — wasm Table into parquet at rest     |
|  [03]   | `readSchema(bytes)` → `Schema`                       | schema peek    | column shape without decoding column data               |
|  [04]   | `ParquetFile.fromUrl(url)` / `.fromFile(blob)`       | async open     | range-request or `Blob` reader over a remote object     |
|  [05]   | `readParquetStream(url)` / `ParquetFile.stream()`    | stream reader  | `Olap.lake.batches` — bounded-memory `RecordBatch` pull |
|  [06]   | `transformParquetStream(stream, props?)`             | stream writer  | `Olap.lake.sink` — encode a `RecordBatch` stream out    |
|  [07]   | `Table.fromIPCStream(buf)` / `table.intoIPCStream()` | IPC crossing   | the `apache-arrow` round-trip seam                      |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Arrow round-trip is the canonical seam (`.api/apache-arrow.md`): read folds `readParquet(bytes).intoIPCStream()` into `apache-arrow`'s `tableFromIPC`; write folds `apache-arrow`'s `tableToIPC(table, "stream")` into `Table.fromIPCStream(bytes)` then `writeParquet`. Parquet's own `Table` never leaves the codec owner as a value — the IPC `Uint8Array` or the `FFIStream` is the only egress.
- Zero-copy path: `toFFI()`/`intoFFI()` hand an Arrow C Data Interface stream to an engine that reads it directly (DuckDB wasm, an FFI-aware consumer), bypassing IPC byte copies; `toFFI` keeps the source `Table` alive so its `free()` still runs, `intoFFI` consumes it.
- Object-plane join (`.api/duckdb-duckdb-wasm.md`, object rail): parquet bytes are content-addressed objects — a lake read pulls the object then `readParquet`s it, a write hashes the `writeParquet` output into the store; large objects ride `ParquetFile.fromUrl` range requests and `stream()` so a browser never materializes a file beyond memory.
- Writer policy is one builder chain: `new WriterPropertiesBuilder().setCompression(Compression.ZSTD).setStatisticsEnabled(EnabledStatistics.Page).setMaxRowGroupSize(n).build()` — compression (`ZSTD`/`SNAPPY`/`GZIP`/`BROTLI`/`LZ4_RAW`), encoding, row-group sizing, and statistics are policy values on one owner, never per-call flags.

[LOCAL_ADMISSION]:
- Every wasm handle is scoped — `Table`, `ParquetFile`, and metadata classes acquire through `Effect.acquireRelease` releasing `free()` (or `Effect.addFinalizer` over `[Symbol.dispose]`), so no wasm heap leaks past the lane; a bare `readParquet` whose Table escapes its scope is rejected.
- `initWasm()` for the `esm`/`bundler` build runs once at `Layer` construction and is proven before any entry; the `node` build skips it, so the lane owner selects the build by runtime, never at the call site.
- Streams stay bounded — unbounded lake objects ride `ParquetFile.stream`/`readParquetStream` lifted through `Stream.fromReadableStream`; `readParquet` whole-buffer decode is admitted only where the object is provably bounded.

[RAIL_LAW]:
- Package: `parquet-wasm`
- Owns: engine-free Parquet decode/encode and the streaming reader/writer at the lake-at-rest edge, round-tripping `apache-arrow` Tables over IPC or the Arrow C Data Interface
- Accept: parquet↔Arrow-IPC round-trip at the codec seam, `WriterPropertiesBuilder` policy for compression/encoding/statistics, `ParquetFile` range-request and stream reads for bounded memory, `FFIStream` zero-copy handoff, scoped `free()` on every wasm handle
- Reject: a DuckDB or Flight engine instantiated only to transcode parquet, parquet's own `Table` leaking as a value across the data seam, an unscoped wasm handle, a second parquet or columnar codec, row-object re-encoding between the lake and the Arrow wire
