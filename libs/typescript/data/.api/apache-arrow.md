# [TS_DATA_API_APACHE_ARROW]

`apache-arrow` is the one columnar interchange the analytical lane meets at: DuckDB wasm returns and ingests Arrow natively, the node row and ClickHouse emit Arrow IPC, and the viewer's geoarrow plane consumes the same `Table` values downstream. This folder catalog carries the OLAP-seam grain — the container values, the IPC codec pair, and the streaming reader `lane/olap` lifts onto `Effect`/`Stream`; the viewer-tier type-system and builder depth stays on `ui`'s catalog of the same package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-arrow`
- package: `apache-arrow` (Apache-2.0)
- module: `exports["."]` condition-selects `Arrow.dom` (browser) vs `Arrow.node` (node) builds; `sideEffects: false`
- peers: none; `@duckdb/duckdb-wasm` and `@duckdb/node-api` meet it at the lane seam as producers/consumers
- runtime: both lanes — the wasm row rides the dom build beside the worker engine, the node row the node build
- rail: `lane/olap` `[06]-[ARROW_WIRE]` — the one wire every engine row joins by emitting or accepting IPC

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the columnar containers the lane seams exchange
- rail: lane/olap
- `Table<T>` is batched columns over a `Schema<T>`; `RecordBatch<T>` one aligned column set — the IPC unit and the streaming grain; `Vector<T>` one logical column whose `.toArray()` is a zero-copy `TypedArray` view; readers iterate as sync AND async iterables of `RecordBatch`.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CONSUMER]                                               |
| :-----: | :---------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Table<T extends TypeMap>`                | container     | wasm `query()` results; `_wire.decode` output            |
|  [02]   | `RecordBatch<T>`                          | batch grain   | the `Stream` element of every bounded interchange        |
|  [03]   | `Vector<T extends DataType>`              | column        | consumer projections; zero-copy `.toArray()` egress      |
|  [04]   | `Schema<T>` / `Field<T>`                  | column shape  | names + logical types riding every decoded frame         |
|  [05]   | `CompressionType` / `compressionRegistry` | codec table   | IPC body compression rows (LZ4_FRAME/ZSTD), never a fork |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IPC decode, encode, and the streaming reader
- rail: lane/olap
- `tableFromIPC` is overloaded on source shape — bytes decode synchronously, a stream/promise/`Response` asynchronously; `tableToIPC(table, type?, compressionType?)` serializes file or stream format; `RecordBatchReader.from(source)` opens the incremental lane and the reader itself is the `AsyncIterable<RecordBatch>` a `Stream.fromAsyncIterable` lift consumes; `isArrowTable`/`isArrowRecordBatch` are the narrowing guards the ingest discriminant folds through.

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CONSUMER]                                            |
| :-----: | :------------------------------------------------------ | :------------- | :---------------------------------------------------- |
|  [01]   | `tableFromIPC(bytes)` → `Table`                         | decode         | `Olap.wire.decode` — engine IPC egress into one Table |
|  [02]   | `tableToIPC(table, type?, compression?)` → `Uint8Array` | encode         | `Olap.wire.encode` — the outbound engine seam         |
|  [03]   | `RecordBatchReader.from(source)`                        | stream reader  | `Olap.wire.batches` — bounded-memory interchange      |
|  [04]   | `reader[Symbol.asyncIterator]()`                        | batch pull     | the `Stream` lift; no whole-Table materialization     |
|  [05]   | `isArrowTable(x)` / `isArrowRecordBatch(x)`             | guard          | ingest discriminant — narrow a live container at seam |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- Boundary-kernel wrap: `tableFromIPC`/`tableToIPC` throws fold through `Effect.try` into `OlapFault` (`reason: "wire"`) at the lane owner; `RecordBatchReader` iteration lifts through `Stream.fromAsyncIterable` with the same fault mint — no Arrow throw crosses the data seam raw.
- DuckDB wasm interop (`.api/duckdb-duckdb-wasm.md`): `conn.send(sql)` pulls record batches lazily so a browser result larger than memory never materializes; ingest is ONE entry discriminating on the source value through `isArrowTable` — a live `Table` rides `insertArrowTable`, IPC bytes ride `insertArrowFromIPCStream`.
- ClickHouse and the node row emit Arrow IPC at the result seam; every engine joins the wire by emitting or accepting IPC, never a per-engine result shape.
- Viewer handoff: decoded `Table` values flow to `ui`'s geoarrow plane without row materialization; the type-system, builder, and visitor depth those layers exploit lives on `ui`'s catalog.

[LOCAL_ADMISSION]:
- Streams stay bounded — large interchange rides `RecordBatchReader` batch iteration; `tableFromIPC` whole-frame decode is admitted only where the frame is provably bounded.
- Row-shaped egress exists only at the final consumer projection; no intermediate row materialization exists between engine seams.

[RAIL_LAW]:
- Package: `apache-arrow`
- Owns: the columnar containers (`Table`/`RecordBatch`/`Vector`/`Schema`/`Field`) and the IPC codec pair with its streaming reader at the data lane seams
- Accept: IPC decode/encode at engine seams, `RecordBatchReader` streaming for bounded memory, zero-copy `Table` handoff to wasm ingest and viewer consumers
- Reject: JSON or row-object re-encoding between engines, per-engine result shapes, whole-Table materialization of unbounded interchange, a second columnar vocabulary
