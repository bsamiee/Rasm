# [TS_DATA_API_APACHE_ARROW]

`apache-arrow` owns the columnar interchange the analytical lane meets at: the container values every engine row emits or accepts, the IPC codec pair, and the streaming reader `lane/olap` lifts onto `Effect`/`Stream`. `ui`'s catalog of the same package owns the viewer-tier type-system and builder depth.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-arrow`
- package: `apache-arrow` (Apache-2.0)
- module: `exports["."]` condition-selects `Arrow.node` under the `node` condition, `Arrow.dom` by default; `sideEffects: false`
- runtime: isomorphic — the wasm row rides the dom build, the node row the node build; no peer dependency
- rail: `lane/olap` `[06]-[ARROW_WIRE]`, the wire every engine row joins by emitting or accepting IPC

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the columnar containers the lane seams exchange

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :---------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `Table<T extends TypeMap>`                | interface     | batched columns over a `Schema<T>` — the decode result  |
|  [02]   | `RecordBatch<T>`                          | class         | one aligned column set — the IPC unit and stream grain  |
|  [03]   | `Vector<T extends DataType>`              | interface     | one logical column; `.toArray()` a zero-copy view       |
|  [04]   | `Schema<T>` / `Field<T>`                  | class         | names and logical types on every decoded frame          |
|  [05]   | `CompressionType` / `compressionRegistry` | enum + const  | IPC body compression (`LZ4_FRAME`/`ZSTD`); its registry |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IPC decode, encode, and the streaming reader

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `tableFromIPC(bytes) -> Table`                         | static   | sync decode; a stream/promise source returns `Promise` |
|  [02]   | `tableToIPC(table, type?, compression?) -> Uint8Array` | static   | file/stream IPC encode with optional body compression  |
|  [03]   | `RecordBatchReader.from(source)`                       | factory  | opens the incremental reader, sync or async by source  |
|  [04]   | `reader[Symbol.asyncIterator]()`                       | instance | batch pull; no whole-`Table` materialization           |
|  [05]   | `isArrowTable(x)` / `isArrowRecordBatch(x)`            | static   | narrowing guards the ingest discriminant folds through |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every engine row joins the analytical lane by emitting or accepting Arrow IPC; the `Table` is the one columnar value crossing every seam, never a per-engine row shape.

[STACKING]:
- `@duckdb/duckdb-wasm`(`.api/duckdb-duckdb-wasm.md`): `query()` returns an `arrow.Table`; ingest discriminates on `isArrowTable` — a live `Table` rides `insertArrowTable`, IPC bytes ride `insertArrowFromIPCStream`.
- `@duckdb/node-api`(`.api/duckdb-node-api.md`): result IPC egress folds through `tableFromIPC`, outbound serialization through `tableToIPC`.
- `parquet-wasm`(`.api/parquet-wasm.md`): `fromIPCStream(buf)` ingests and `intoIPCStream()` emits the shared IPC stream buffer; `toFFI()`/`intoFFI()` cross the Arrow C Data Interface zero-copy.
- `@effect/sql-clickhouse`(`.api/effect-sql-clickhouse.md`): Arrow IPC carries interchange from the at-scale row back to the embedded rows and the viewer.
- `@qualithm/arrow-flight-client`(`.api/qualithm-arrow-flight-client.md`): `decodeFlightDataToTable` lands `FlightData` on Arrow columns, `encodeRecordBatchesToFlightData` re-encodes for `doPut`.
- `lane/olap`: `Olap.wire.decode`/`.encode` fold `tableFromIPC`/`tableToIPC` through `Effect.try` into `OlapFault` (`reason: "wire"`), `Olap.wire.batches` lifts `RecordBatchReader` iteration through `Stream.fromAsyncIterable` with the same fault mint, and decoded `Table` values reach `ui`'s geoarrow plane without row materialization.

[LOCAL_ADMISSION]:
- Large interchange rides `RecordBatchReader` batch iteration; `tableFromIPC` whole-frame decode admits only where the frame is provably bounded.
- Row-shaped egress exists only at the final consumer projection, never between engine seams.

[RAIL_LAW]:
- Package: `apache-arrow`
- Owns: the columnar containers (`Table`/`RecordBatch`/`Vector`/`Schema`/`Field`) and the IPC codec pair with its streaming reader at the data-lane seams
- Accept: IPC decode/encode at engine seams, `RecordBatchReader` streaming for bounded memory, zero-copy `Table` handoff to wasm ingest and viewer
- Reject: JSON or row-object re-encoding between engines, per-engine result shapes, whole-`Table` materialization of unbounded interchange, a second columnar vocabulary
