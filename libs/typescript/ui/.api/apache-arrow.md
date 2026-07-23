# [TS_UI_API_APACHE_ARROW]

`apache-arrow` is the JS reference implementation of Arrow and the columnar interchange every engine seam meets: DuckDB node/wasm, ClickHouse, and the pg result seam speak it; the viewer's GeoArrow plane consumes the same tables row-copy-free. A `Table` is a set of `RecordBatch`es over a `Schema`, each column a `Vector` backed by `Data` buffers. One `Type`-discriminated `DataType` ADT dispatches through one `Visitor`, so a new logical type is a `DataType` row and a visit arm, never a parallel class hierarchy. `GeoArrow*Layer` binds a geometry `Vector`'s typed arrays straight to the GPU.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-arrow`
- package: `apache-arrow` · license `Apache-2.0`
- module: `type: commonjs`; THREE build targets condition-selected by `exports["."]` — `Arrow.dom` (browser `import`, DOM streams), `Arrow.node` (`node`, node streams), `Arrow.d.ts` (base); `./*` exposes per-concern subpath imports. `sideEffects: false`.
- asset: TSDECL `Arrow.dom.d.ts` (browser barrel over `Arrow.js`); per-concern `.d.ts` — `table`/`recordbatch`/`vector`/`data`/`schema`/`type`/`enum`/`factories`/`builder`/`visitor`/`ipc/*` (restored).
- deps: `flatbuffers` (the IPC metadata wire), `tslib`, `@swc/helpers`; `command-line-args`/`command-line-usage` back the `arrow catalogcsv` bin — a CLI, NOT part of the library surface. No peer.
- runtime: browser lane binds `Arrow.dom` (WHATWG streams, `ReadableStream`/`Blob`); data lanes bind node/wasm row peers. Zero-copy `Vector.toArray()` returns the backing `TypedArray` view, not a row copy.
- plane: viewer-tier catalog owned in `ui/.api` — the type-system and builder depth the strictest consumer demands; data owns its own folder catalog for the OLAP/IPC seam.
- rail: columnar interchange; the data `[ANALYTICAL]` group and ui `[GRID_CHARTS]`/`[SPATIAL]` groups meet on this owner.
- role: the columnar container + type system, IPC decode/encode, and RecordBatch stream surface shared by engine seams and viewer layers.

## [02]-[COLUMNAR_CORE]

Four nested shapes carry the container hierarchy, each generic over a `TypeMap`/`DataType`: `Table` (batched columns) → `RecordBatch` (one aligned column set) → `Vector` (one logical column, possibly chunked) → `Data` (the raw buffer tuple). `Schema`/`Field` carry the column names + logical types + metadata. `Vector.toArray()` is the zero-copy escape to the backing `TypedArray` the GPU binds; `getChild` walks nested (`Struct`/`List`) columns. Construction is factory functions, not constructors, so the buffer layout stays an implementation detail.

| [INDEX] | [SYMBOL]                                                                             | [KIND]        |
| :-----: | :----------------------------------------------------------------------------------- | :------------ |
|  [01]   | `Table<T extends TypeMap>`                                                           | class         |
|  [02]   | `RecordBatch<T>`                                                                     | class         |
|  [03]   | `Vector<T extends DataType>`                                                         | class         |
|  [04]   | `Data<T>` / `makeData<T>(props)`                                                     | class/factory |
|  [05]   | `Schema<T>` / `Field<T>(name, type, nullable?, metadata?)`                           | class         |
|  [06]   | `makeVector` / `vectorFromArray` / `makeTable` / `tableFromArrays` / `tableFromJSON` | factory       |
|  [07]   | `isArrowTable` / `isArrowRecordBatch` / `isArrowVector` / `isArrowData`              | guard         |
|  [08]   | `isArrowSchema` / `isArrowField` / `isArrowDataType`                                 | guard         |

[CAPABILITY] per member:
- [01]-[TABLE]: batched columns; `.schema`/`.numRows`/`.numCols`/`.getChild(name)`/`.select`/`.slice`/`.batches`.
- [02]-[RECORD_BATCH]: one aligned column set — the IPC unit and the layer's `data` grain.
- [03]-[VECTOR]: one column; `.get(i)`/`.toArray()` (zero-copy `T['TArray']`)/`.getChild`/`.nullCount`/`.data`.
- [04]-[DATA]: raw buffer tuple (validity/offsets/values/children).
- [05]-[SCHEMA]: column names + logical types + `Map` metadata + dictionary registry.
- [06]-[FACTORIES]: build a `Vector`/`Table` from typed arrays, JS arrays, or column maps.
- [07]-[CONTAINER_GUARDS]: runtime `x is T` narrowing over live containers — the family the data lane's ingest discriminant folds through.
- [08]-[TYPE_GUARDS]: `isArrowSchema`/`isArrowField` narrow schema nodes; `isArrowDataType` narrows the `[03]` `DataType` ADT.

```ts signature
declare class Table<T extends TypeMap = any> {
  constructor(schema: Schema<T>, data?: RecordBatch<T> | RecordBatch<T>[])
  readonly schema: Schema<T>; readonly batches: RecordBatch<T>[]
  get numRows(): number; get numCols(): number
  getChild<P extends keyof T>(name: P): Vector<T[P]> | null       // a column, by name
  select<K extends keyof T>(names: K[]): Table<Pick<T, K>>; slice(begin?: number, end?: number): Table<T>
}
declare class Vector<T extends DataType = any> {
  get length(): number; get nullCount(): number; get data(): Data<T>[]; get type(): T
  get(index: number): T['TValue'] | null
  toArray(): T['TArray']                                          // zero-copy view onto the backing TypedArray — what deck.gl binds
  getChild<R extends keyof T['TChildren']>(name: R): Vector<any> | null   // nested Struct/List access
}
declare function makeData<T extends DataType>(props: DataProps<T>): Data<T>   // overloaded per DataType — one factory, the visitor dispatches
declare class Field<T extends DataType = any> { constructor(name: string, type: T, nullable?: boolean, metadata?: Map<string, string> | null) }
declare function isArrowTable(x: any): x is Table; declare function isArrowRecordBatch(x: any): x is RecordBatch
declare function isArrowVector(x: any): x is Vector; declare function isArrowData(x: any): x is Data<any>
declare function isArrowSchema(x: any): x is Schema; declare function isArrowField(x: any): x is Field; declare function isArrowDataType(x: any): x is DataType   // runtime narrowing over live containers + type nodes
```

## [03]-[TYPE_SYSTEM]

One `Type` enum discriminates one `DataType` ADT, dispatched by one `Visitor` — the APPROACH-collapse spine, never 40 unrelated type/builder classes. Every per-type behavior — value get/set, comparison, builder, IPC buffer load — is a `visit<Type>` arm, so the `DataType` rows below are SEED members of a generated family, not the mechanism. A new logical type is a `DataType` subclass + a `Type` enum member + a visit arm; consumer code branches on `type.typeId` (the `Type` enum) or lets the visitor route.

| [INDEX] | [SYMBOL]                                                                                          | [KIND]       |
| :-----: | :------------------------------------------------------------------------------------------------ | :----------- |
|  [01]   | `Type` (enum)                                                                                     | discriminant |
|  [02]   | `DataType` + subclasses                                                                           | ADT          |
|  [03]   | `List`/`LargeList`/`FixedSizeList`/`Struct`/`Union`/`Map_`/`Dictionary`                            | nested type  |
|  [04]   | `Visitor`                                                                                         | dispatcher   |
|  [05]   | `Builder<T>` / `makeBuilder` / `builderThroughIterable` / `builderThroughAsyncIterable`           | factory      |
|  [06]   | enums `DateUnit`/`TimeUnit`/`Precision`/`IntervalUnit`/`UnionMode`/`MetadataVersion`/`BufferType` | axis         |

[CAPABILITY] per member:
- [01]-[TYPE_ENUM]: `Null=1`/`Int=2`/`Float=3`/`Utf8=5`/`Bool=6`/`List=12`/`Struct=13`/`FixedSizeList=16`/`Map=17`/`Duration=18`/`LargeList=21`/`BinaryView=23`/`Utf8View=24`/`Dictionary=-1`/… — `type.typeId`.
- [02]-[DATATYPE]: `Null`/`Bool`/`Int(8..64)`/`Float(16/32/64)`/`Utf8`/`Binary`/`Decimal`/`Date_`/`Time`/`Timestamp`/`Interval`/`Duration`/`Utf8View`/`BinaryView` — leaf logical types; `*View` backs each value by an inline-or-referenced view rather than a contiguous offset run.
- [03]-[NESTED]: composite types GeoArrow geometry columns build from; `LargeList` is the 64-bit-offset list for columns past the 2³¹-element ceiling.
- [04]-[VISITOR]: `visit(node)` routes by `typeId` — get/set/builder/comparator base.
- [05]-[BUILDER]: streaming column construction; one `<Type>Builder` per DataType via the visitor — `Utf8ViewBuilder`/`BinaryViewBuilder`/`LargeListBuilder` are the view/large-offset arms.
- [06]-[AXES]: parameter axes on the parameterized types (never subclass-per-unit).

```ts signature
declare enum Type { Null = 1, Int = 2, Float = 3, Binary = 4, Utf8 = 5, Bool = 6, Decimal = 7, Date = 8, Time = 9, Timestamp = 10, Interval = 11, List = 12, Struct = 13, Union = 14, FixedSizeBinary = 15, FixedSizeList = 16, Map = 17, Duration = 18, LargeBinary = 19, LargeUtf8 = 20, LargeList = 21, BinaryView = 23, Utf8View = 24, Dictionary = -1 /* + negative physical Int8/Float32/… ids */ }
// GeoArrow geometry columns ARE Arrow nested types — a point is a fixed 2/3-wide coordinate, a line/ring a variable list of them:
//   Point     : FixedSizeList<Float64>[2|3]
//   LineString: List<FixedSizeList<Float64>>
//   Polygon   : List<List<FixedSizeList<Float64>>>          (+ another List for Multi* variants)
declare abstract class Visitor { visit(...args: any[]): any; getVisitFnByTypeId(typeId: Type, throwIfNotFound?: boolean): any }   // one dispatcher, routed by Type.typeId
declare function makeBuilder<T extends DataType>(options: BuilderOptions<T>): Builder<T>        // one factory, per-type builder via the visitor
declare class Utf8View extends DataType<Type.Utf8View> { constructor() }        // inline-or-referenced UTF8 views; static INLINE_CAPACITY = 12
declare class BinaryView extends DataType<Type.BinaryView> { constructor() }    // inline-or-referenced byte views; static ELEMENT_WIDTH = 16
declare class LargeList<T extends DataType> extends DataType<Type.LargeList> { constructor(child: Field<T>) }   // 64-bit offsets; throws TypeError past the 2³¹ index ceiling
```

## [04]-[IPC_AND_STREAM]

`tableFromIPC` reads an Arrow IPC frame (file or stream format) into a `Table`, `tableToIPC` serializes one back, and `RecordBatchReader.from` streams incremental batches — the decode/encode boundary. All overload on source shape — bytes decode synchronously, a `ReadableStream`/`Promise`/`fetch` `Response` decode asynchronously — so the same call covers a `Uint8Array` frame and a streamed body. Compression is a pluggable `compressionRegistry` keyed by `CompressionType` (LZ4/ZSTD), not a fork.

| [INDEX] | [SYMBOL]                                                                                                 | [KIND]      |
| :-----: | :------------------------------------------------------------------------------------------------------- | :---------- |
|  [01]   | `tableFromIPC<T>(source): Table<T> \| Promise<Table<T>>`                                                 | decode      |
|  [02]   | `tableToIPC<T>(table, type?: 'file' \| 'stream', compressionType?: CompressionType \| null): Uint8Array` | encode      |
|  [03]   | `RecordBatchReader.from(source)` / `RecordBatchFileReader` / `RecordBatchStreamReader` / `Async*`        | reader      |
|  [04]   | `RecordBatchWriter` / `RecordBatchFileWriter` / `RecordBatchStreamWriter` / `RecordBatchJSONWriter`      | writer      |
|  [05]   | `ByteStream` / `AsyncByteStream` / `AsyncByteQueue`                                                      | byte source |
|  [06]   | `compressionRegistry` / `CompressionType`                                                                | codec table |

[CAPABILITY] per member:
- [01]-[DECODE]: bytes → sync `Table`; stream/promise/`Response` → `Promise<Table>`.
- [02]-[ENCODE]: serialize a `Table` to an IPC frame, optionally compressed.
- [03]-[READER]: incremental `RecordBatch` stream; the file-vs-stream format is auto-detected.
- [04]-[WRITER]: streaming encode; `.writeAll(table)` / `.toUint8Array()`.
- [05]-[BYTE_SOURCE]: sync/async byte adapters IPC readers consume.
- [06]-[COMPRESSION]: pluggable IPC body compression (LZ4_FRAME/ZSTD) — a registry row.

## [05]-[STACKING]

- GeoArrow deck.gl consumes Arrow columns directly: coordinate layers bind nested point, line, and polygon vectors, while DGGS layers bind cell-id columns. Callers mount one layer per `RecordBatch`, so viewer code keeps `RecordBatch`/`Vector` values instead of JS coordinate arrays.
- deck.gl core, layers, geo-layers, and `@geoarrow/geoarrow-js` host the GeoArrow layers and encoding algorithms.
- Data OLAP, pg, and viewer interchange meet on Arrow: DuckDB wasm returns `arrow.Table` and ingests through `insertArrowTable`/`insertArrowFromIPCStream` — the live-`Table`-vs-IPC-bytes discriminant folding through the `isArrowTable`/`isArrowRecordBatch` narrowing guards `data`(`.api/apache-arrow.md`) carries as its ingest fold — while the node row and ClickHouse meet the same wire at the lane seam. JSON or row re-encoding between engines is the named defect.
- `tableFromIPC` decodes viewer-bound Arrow IPC frames after upstream WKB-to-columnar projection. `apache-arrow` owns the columnar decode; `wire` owns the WKB-to-GeoArrow projection.
- `BrowserWorker` decode pools carry heavy `tableFromIPC` frames off the render thread, wrapping malformed-frame throws in the typed Effect failure channel before layer binding.
- `@turf/turf` consumes GeoJSON, not Arrow. Query-scale planar ops materialize the selected coordinate vector at that seam, leaving bulk columnar transport on Arrow.

## [06]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- One wire law: an analytical result crossing any engine seam travels as Arrow (`Table` in memory, IPC on the wire); JSON or row re-encoding between engines is the named defect.
- DuckDB's wasm row returns `arrow.Table` natively and ingests via `insertArrowTable`/`insertArrowFromIPCStream`; the node row, ClickHouse, pg result seam, and viewer meet the same wire at the lane boundary.

[RAIL_LAW]:
- Package: `apache-arrow`
- Owns: the Arrow columnar containers (`Table`/`RecordBatch`/`Vector`/`Data`/`Schema`/`Field`), the `Type`-discriminated `DataType` ADT (view + large-offset types included) + `Visitor` dispatch + `Builder` family + the `isArrow*` narrowing guards, and the IPC decode/encode (`tableFromIPC`/`tableToIPC`, the `RecordBatch` readers/writers, `compressionRegistry`).
- Accept: `tableFromIPC`/`tableToIPC` at engine and viewer seams; `Table`/IPC/record-batch interchange between OLAP rows, pg result lanes, and viewer layers; a `RecordBatch` handed zero-copy to a `GeoArrow*Layer` via `data: RecordBatch` + a coordinate-column accessor; `makeBuilder`/`vectorFromArray` for column construction; the `Visitor`/`Type` enum for per-type dispatch; off-thread decode on the `BrowserWorker` pool wrapped in `Effect`.
- Reject: materializing an Arrow column to a JS array of rows before a GeoArrow layer; per-engine bespoke result shapes; row-materialized inter-engine transfer; a second columnar vocabulary; a parallel type hierarchy where a `DataType` row + visit arm suffices; the `arrow2csv` bin as a library surface; re-implementing IPC decode where `tableFromIPC` covers the source shape; WKB decode in `viewer` (it stays in `wire`).
- Boundary: browser binds the `Arrow.dom` build target, and data lanes bind node/wasm row peers. `apache-arrow` is the columnar transport and type system; `@geoarrow/geoarrow-js` owns GeoArrow encoding, deck.gl owns GPU render, and `turf` owns planar analysis.
- Peer gap: aggregation-family GeoArrow layers need `@deck.gl/aggregation-layers` and `@math.gl/polygon`, neither in the `[VIEWER_GEO]` roster.
