# [TS_UI_API_APACHE_ARROW]

`apache-arrow` owns the columnar container and type system every engine seam meets, exchanged as a `Table` in memory and an IPC frame on the wire; the viewer's GeoArrow plane binds a geometry `Vector`'s typed arrays straight to the GPU. `ui` carries the viewer-tier type-system and builder depth; `data` owns the OLAP/IPC seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-arrow`
- package: `apache-arrow` (Apache-2.0)
- module: `exports["."]` condition-selects `Arrow.dom` (browser `import`, WHATWG streams), `Arrow.node` (node streams), or the base barrel; `./*` exposes per-concern subpath imports; `sideEffects: false`
- runtime: browser binds `Arrow.dom` with `ReadableStream`/`Blob`; data lanes bind the node/wasm peers; no peer dependency
- rail: columnar interchange every engine seam and viewer layer meets

## [02]-[COLUMNAR_CORE]

Four nested shapes carry the container hierarchy: `Table` batches columns over a `Schema`, `RecordBatch` holds one aligned column set, `Vector` one logical column, `Data` the raw buffer tuple. Factory functions build every container, so buffer layout stays an implementation detail, and `Vector.toArray()` escapes zero-copy to the backing `TypedArray` the GPU binds.

[COLUMNAR_TYPE_SCOPE]: the container hierarchy and its schema

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `Table<T extends TypeMap>`   | interface     | batched columns; `.getChild`/`.select`/`.slice`                            |
|  [02]   | `RecordBatch<T>`             | class         | one aligned column set — IPC unit and layer grain                          |
|  [03]   | `Vector<T extends DataType>` | interface     | one column; `.get`/`.toArray` zero-copy/`.getChild`                        |
|  [04]   | `Data<T>`                    | interface     | raw buffer tuple backing one column                                        |
|  [05]   | `Schema<T>` / `Field<T>`     | class         | column names, logical types, dictionary registry                           |
|  [06]   | `StructRowProxy<T>`          | type          | named-field row view struct reads return; picking layers key on its fields |

[COLUMNAR_ENTRY_SCOPE]: container construction and runtime narrowing

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `makeData(DataProps) -> Data`                                           | factory  | build a `Data` buffer tuple                        |
|  [02]   | `makeVector` / `vectorFromArray`                                        | factory  | build a `Vector` from typed or JS arrays           |
|  [03]   | `makeTable` / `tableFromArrays` / `tableFromJSON`                       | factory  | build a `Table` from column maps or rows           |
|  [04]   | `isArrowTable` / `isArrowRecordBatch` / `isArrowVector` / `isArrowData` | static   | container `x is T` narrowing                       |
|  [05]   | `isArrowSchema` / `isArrowField` / `isArrowDataType`                    | static   | schema-node and `DataType` narrowing               |
|  [06]   | `Table.batches -> RecordBatch[]`                                        | property | zero-copy batch list at the per-batch render grain |

## [03]-[TYPE_SYSTEM]

One `Type` enum discriminates one `DataType` ADT, dispatched by one `Visitor`: every per-type behavior — value get/set, comparison, builder, IPC buffer load — is a `visit<Type>` arm, so the leaf rosters below seed a generated family, never the mechanism. Extending the type algebra adds one `DataType` subclass, one `Type` member, and one visit arm; consumer code branches on `type.typeId` or lets the visitor route.

[TYPE_SYSTEM_TYPE_SCOPE]: the discriminant, the ADT, and its builder

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Type`                  | enum          | the `typeId` discriminant every `DataType` carries    |
|  [02]   | `DataType` + subclasses | interface     | leaf and nested logical types over `Visitor` dispatch |
|  [03]   | `Visitor`               | class         | routes get/set/builder/comparator by `typeId`         |
|  [04]   | `Builder<T>`            | class         | streaming column construction, one arm per type       |

[DATATYPE]: `Null` `Bool` `Int` `Float` `Utf8` `Binary` `Decimal` `Date_` `Time` `Timestamp` `Interval` `Duration` `Utf8View` `BinaryView` `FixedSizeBinary` `LargeBinary` `LargeUtf8` `List` `LargeList` `FixedSizeList` `Struct` `Union` `Map_` `Dictionary` — `*View` backs values inline-or-referenced; `Large*` carries 64-bit offsets past the 2³¹-element ceiling.
[AXES]: `DateUnit` `TimeUnit` `Precision` `IntervalUnit` `UnionMode` `MetadataVersion` `BufferType` — parameter axes on the parameterized types, never a subclass per unit.

[TYPE_SYSTEM_ENTRY_SCOPE]: introspection and builder construction

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Visitor.visit(...) -> any`                              | instance | route a node by `typeId`             |
|  [02]   | `Visitor.getVisitFnByTypeId(Type, boolean?)`             | instance | resolve the visit arm for a `Type`   |
|  [03]   | `makeBuilder(BuilderOptions) -> Builder`                 | factory  | construct the per-`DataType` builder |
|  [04]   | `builderThroughIterable` / `builderThroughAsyncIterable` | factory  | stream values through a `Builder`    |

## [04]-[IPC_AND_STREAM]

`tableFromIPC` decodes an Arrow IPC frame into a `Table`, `tableToIPC` serializes one back, and `RecordBatchReader.from` streams incremental batches. Every reader overloads on source shape — bytes decode synchronously, a `ReadableStream`, `Promise`, or `fetch` `Response` decode asynchronously — so one call covers a `Uint8Array` frame and a streamed body. Compression is a `compressionRegistry` row keyed by `CompressionType`, never a fork.

[IPC_TYPE_SCOPE]: readers, writers, and the byte sources they consume

| [INDEX] | [SYMBOL]                                                                  | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------ | :------------ | :---------------------------------------- |
|  [01]   | `RecordBatchReader` / `RecordBatchFileReader` / `RecordBatchStreamReader` | class         | incremental stream, format auto-detected  |
|  [02]   | `RecordBatchWriter` / `RecordBatchFileWriter` / `RecordBatchStreamWriter` | class         | encode; `.writeAll`/`.toUint8Array`       |
|  [03]   | `ByteStream` / `AsyncByteStream` / `AsyncByteQueue`                       | class         | sync and async byte adapters              |
|  [04]   | `CompressionType` / `compressionRegistry`                                 | enum + const  | pluggable IPC body compression (LZ4/ZSTD) |

[IPC_ENTRY_SCOPE]: the decode and encode boundary

| [INDEX] | [SURFACE]                                              | [SHAPE] | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `tableFromIPC(source) -> Table \| Promise<Table>`      | static  | decode a frame; async when it streams     |
|  [02]   | `tableToIPC(table, type?, compression?) -> Uint8Array` | static  | file/stream encode, optionally compressed |
|  [03]   | `RecordBatchReader.from(source)`                       | factory | open the reader, sync or async by source  |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Arrow carries every analytical result crossing an engine seam — a `Table` in memory, an IPC frame on the wire; JSON or row re-encoding between engines is the named defect.

[STACKING]:
- `geoarrow-deck.gl-geoarrow`(`.api/geoarrow-deck.gl-geoarrow.md`): every `GeoArrow*Layer` binds one `RecordBatch` as `data` with a coordinate-column accessor, reading the geometry `Vector`'s typed arrays into GPU attributes — one layer per `RecordBatch`, zero row materialization.
- `@duckdb/duckdb-wasm`(`../../data/.api/duckdb-duckdb-wasm.md`): the OLAP/IPC seam — `query()` returns an `arrow.Table` and ingest folds through `insertArrowTable` (live `Table`) or `insertArrowFromIPCStream` (IPC bytes), the `isArrowTable`/`isArrowRecordBatch` guards discriminating the inbound shape.
- `@perspective-dev/client`(`.api/perspective-dev-client.md`): `View.to_arrow()` emits an IPC `ArrayBuffer` that `tableFromIPC` decodes for any Arrow consumer, and `Client.table(buf,{format:"arrow"})` / `Table.update(arrowBuf)` ingest `tableToIPC` output — the streaming engine speaks Arrow both directions.
- `turf-turf`(`.api/turf-turf.md`): consumes GeoJSON, so query-scale planar ops materialize the selected coordinate `Vector` at that seam and leave bulk columnar transport on Arrow.
- `wire`: owns the WKB-to-GeoArrow projection; `tableFromIPC` decodes the viewer-bound frame after, so columnar decode stays here and WKB projection stays in `wire`.
- `BrowserWorker`: off-thread decode pools carry heavy `tableFromIPC` frames off the render thread, wrapping malformed-frame throws in the typed `Effect` failure channel before layer binding.

[RAIL_LAW]:
- Package: `apache-arrow`
- Owns: the columnar containers (`Table`/`RecordBatch`/`Vector`/`Data`/`Schema`/`Field`), the `Type`-discriminated `DataType` ADT with `Visitor` dispatch, the `Builder` family, the `isArrow*` guards, and the IPC codec pair (`tableFromIPC`/`tableToIPC`) with its readers, writers, and `compressionRegistry`
- Accept: `tableFromIPC`/`tableToIPC` at engine and viewer seams; a `RecordBatch` handed zero-copy to a `GeoArrow*Layer`; `makeBuilder`/`vectorFromArray` for column construction; `Visitor`/`Type` dispatch for per-type behavior; off-thread decode on the `BrowserWorker` pool wrapped in `Effect`
- Reject: materializing an Arrow column to JS rows before a GeoArrow layer; per-engine result shapes; row-materialized inter-engine transfer; a second columnar vocabulary; a parallel type hierarchy where a `DataType` row and a visit arm suffice; the `arrow2csv` CLI bin as a library surface; WKB decode in `viewer`
