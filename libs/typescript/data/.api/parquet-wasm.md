# [TS_DATA_API_PARQUET_WASM]

`parquet-wasm` owns engine-free Parquet decode and encode at the lake-at-rest edge, round-tripping `apache-arrow` `Table` values through Parquet bytes with no analytical engine booted. Parquet is the format at rest, Arrow IPC the in-memory wire, and this codec the adapter on `lane/olap`.

## [01]-[PUBLIC_TYPES]

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
- `ReaderOptions` carries `batchSize`, `rowGroups`, `limit`, `offset`, `columns`, and `concurrency` — the read-policy row every lazy entry takes, so a scan states its batch grain and its request fan rather than inheriting silent defaults.
- Members prefixed `into` consume their receiver and members prefixed `to` keep it alive for a later `free()`; `writeParquet` consumes BOTH its table and its writer properties, so the write path leaves nothing to release.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parquet decode, encode, and the streaming reader/writer

| [INDEX] | [SURFACE]                                         | [SHAPE] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------ | :------ | :-------------------------------- |
|  [01]   | `readParquet(bytes, options?) -> Table`           | static  | decode a whole buffer to a Table  |
|  [02]   | `writeParquet(table, props?) -> Uint8Array`       | static  | encode a Table to parquet bytes   |
|  [03]   | `readSchema(bytes) -> Schema`                     | static  | schema without column decode      |
|  [04]   | `ParquetFile.fromUrl(url)` / `.fromFile(blob)`    | factory | range-request or `Blob` reader    |
|  [05]   | `readParquetStream(url)` / `ParquetFile.stream()` | static  | bounded-memory `RecordBatch` pull |
|  [06]   | `transformParquetStream(stream, props?)`          | static  | encode a `RecordBatch` stream out |
|  [07]   | `Table.fromIPCStream(buf)` / `.intoIPCStream()`   | factory | the `apache-arrow` round-trip     |

- `ParquetFile.fromUrl(url)` and `.stream(options?)` both answer a `Promise` — `stream` resolves a web `ReadableStream` of this package's own `RecordBatch`, so the lifted form awaits both and never treats either as synchronous.
- `ParquetFile.fromUrl` reads by RANGE on both builds: opening costs three bounded requests — a suffix range for the footer length, a suffix range for the footer metadata, and one bounded span for the page index — and the stream then draws exactly ONE range request per row group, so a scan pays metadata and the groups it reads, never a whole-object GET.
- `ReaderOptions.concurrency` bounds range requests in flight at precisely its value on the node build, and `batchSize` splits each row group into ceil(rowGroupRows / batchSize) emitted batches, so both knobs are live on the server lane rather than browser-only.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Parquet's own `Table` never leaves the codec as a value — the IPC `Uint8Array` or the `FFIStream` is the only egress, so a wasm-backed container meets `apache-arrow`'s across the stream buffer or the Arrow C Data Interface, never by shared class identity.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): read folds `readParquet(bytes).intoIPCStream()` into `tableFromIPC`; write folds `tableToIPC(table, "stream")` into `Table.fromIPCStream(bytes)` then `writeParquet`; `toFFI()`/`intoFFI()` cross the Arrow C Data Interface zero-copy, `toFFI` keeping the source alive for its `free()` and `intoFFI` consuming it.
- `@duckdb/duckdb-wasm`(`.api/duckdb-duckdb-wasm.md`): lake parquet bytes are content-addressed objects — a read pulls the object then `readParquet`s it, a write hashes the `writeParquet` output into the store, and large objects ride `ParquetFile.fromUrl` range requests with `stream()` so the browser never materializes a whole file.
- `lane/olap`: `Olap.lake` binds this codec at `[08]-[ARROW_WIRE]` — `read`/`schema` fold `readParquet`/`readSchema` through `intoIPCStream` onto `tableFromIPC`, `batches` streams `ParquetFile.stream` range reads, `write` folds `Table.fromIPCStream` into `writeParquet`, and `sink` weights an Arrow batch feed by rows into one object per row group; writer policy is one `_PARQUET` row driving the `WriterPropertiesBuilder` chain, never per-call flags.
- `lane/olap`: `transformParquetStream` is DECLINED at that owner — its input grain is this package's own `RecordBatch`, reachable from an Arrow batch only through a per-batch round trip whose intermediate container the consumer cannot free, so the weighted-window write buys the same bounded egress with no orphaned handle.

[LOCAL_ADMISSION]:
- Ownership is per member, never a blanket bracket: `intoIPCStream`, `writeParquet`, and `WriterPropertiesBuilder.build` each call `__destroy_into_raw` and CONSUME their handle, so a release arm around one frees a pointer the call already took; `ParquetFile` outlives its mint and is the container that acquires under `Effect.acquireRelease` releasing `free()`.
- No container leaves the expression that minted it — bytes and `apache-arrow` values are the only egress, so no linear-memory view crosses a lane boundary.
- Each async build resolves its default initializer once at construction, proven before any entry; the `node` build inlines its wasm and resolves nothing, so the lane owner takes the initializer as a coordinate and no call site branches on runtime.
- Unbounded lake objects ride `ParquetFile.stream`/`readParquetStream` lifted through `Stream.fromReadableStream`; `readParquet` whole-buffer decode admits only where the object is provably bounded.
