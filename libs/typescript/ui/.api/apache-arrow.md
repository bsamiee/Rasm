# [apache-arrow] — the columnar substrate the GeoArrow deck.gl layers consume zero-copy; `scope:viewer` project-local

`apache-arrow` is the JS reference implementation of the Arrow columnar format: a `Table` is a set of `RecordBatch`es over a `Schema`, each column a `Vector<T extends DataType>` backed by `Data<T>` (validity/offset/value/child buffers). The type system is ONE `Type`-enum-discriminated `DataType` ADT dispatched by ONE `Visitor` — get/set/builder/comparator/IPC-load all route through the visitor keyed by `type.typeId`, so a new logical type is a `DataType` row + a visit arm, never a parallel class hierarchy. Inside Rasm it is `scope:viewer` project-local (admitted only by the `ui/viewer` Nx project, compile-time excluded from the non-spatial core): it is a declared PEER of `@geoarrow/deck.gl-geoarrow`, whose `GeoArrow*Layer` roster reads an Arrow geometry `Vector` (GeoArrow-encoded `FixedSizeList`/`List` coordinate columns) and uploads its flat typed-array buffers to the GPU with no per-row copy. `viewer/geo/layers` decodes an Arrow IPC frame with `tableFromIPC`, hands the geometry column to a GeoArrow layer, and never re-materializes rows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-arrow`
- package: `apache-arrow` · version `21.1.0` · license `Apache-2.0`
- module: `type: commonjs`; THREE build targets condition-selected by `exports["."]` — `Arrow.dom` (browser `import`, DOM streams), `Arrow.node` (`node`, node streams), `Arrow.d.ts` (base); `./*` exposes per-concern subpath imports. `sideEffects: false`.
- asset: TSDECL `Arrow.dom.d.ts` (browser barrel over `Arrow.js`); per-concern `.d.ts` — `table`/`recordbatch`/`vector`/`data`/`schema`/`type`/`enum`/`factories`/`builder`/`visitor`/`ipc/*` (`assay api resolve apache-arrow` → `21.1.0`, restored).
- deps: `flatbuffers` (the IPC metadata wire), `tslib`, `@swc/helpers`; `command-line-args`/`command-line-usage` back the `arrow2csv` bin — a CLI, NOT part of the library surface. No peer.
- runtime: browser lane binds `Arrow.dom` (WHATWG streams, `ReadableStream`/`Blob`); `viewer` uses the DOM target. Zero-copy `Vector.toArray()` returns the backing `TypedArray` view, not a row copy.
- plane: `plane:ui` / `scope:viewer` — project-local to the `ui/viewer` Nx project; the core `ui` project never imports it.
- rail: viewer/columnar; the `[VIEWER_GEO]` group (`maplibre-gl`, `@deck.gl/*`, `@geoarrow/deck.gl-geoarrow`, `apache-arrow`, `@turf/turf`).
- role: the columnar container + type system the GeoArrow deck.gl layers consume; the IPC decode/encode at the `viewer` data boundary.

## [02]-[COLUMNAR_CORE]

The container hierarchy is four nested shapes, each generic over a `TypeMap`/`DataType`: `Table` (batched columns) → `RecordBatch` (one aligned column set) → `Vector` (one logical column, possibly chunked) → `Data` (the raw buffer tuple). `Schema`/`Field` carry the column names + logical types + metadata. `Vector.toArray()` is the zero-copy escape to the backing `TypedArray` the GPU binds; `getChild` walks nested (`Struct`/`List`) columns. Construction is factory functions, not constructors, so the buffer layout stays an implementation detail.

| [INDEX] | [SYMBOL]                                                   | [KIND]        | [CAPABILITY / BOUNDARY]                                             |
| :-----: | :--------------------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Table<T extends TypeMap>`                                  | class         | batched columns; `.schema`/`.numRows`/`.numCols`/`.getChild(name)`/`.select`/`.slice`/`.batches` |
|  [02]   | `RecordBatch<T>`                                            | class         | one aligned column set — the IPC unit and the layer's `data` grain |
|  [03]   | `Vector<T extends DataType>`                               | class         | one column; `.get(i)`/`.toArray()` (zero-copy `T['TArray']`)/`.getChild`/`.nullCount`/`.data` |
|  [04]   | `Data<T>` / `makeData<T>(props)`                            | class/factory | the raw buffer tuple (validity/offsets/values/children)            |
|  [05]   | `Schema<T>` / `Field<T>(name, type, nullable?, metadata?)` | class         | column names + logical types + `Map` metadata + dictionary registry|
|  [06]   | `makeVector` / `vectorFromArray` / `makeTable` / `tableFromArrays` / `tableFromJSON` | factory | build a `Vector`/`Table` from typed arrays, JS arrays, or column maps |

```ts contract
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
```

## [03]-[TYPE_SYSTEM]

The APPROACH-collapse spine: not 40 unrelated type/builder classes but ONE `Type` enum discriminating ONE `DataType` ADT, dispatched by ONE `Visitor`. Every per-type behavior — value get/set, comparison, builder, IPC buffer load — is a `visit<Type>` arm, so the `DataType` rows below are SEED members of a generated family, not the mechanism. A new logical type is a `DataType` subclass + a `Type` enum member + a visit arm; consumer code branches on `type.typeId` (the `Type` enum) or lets the visitor route.

| [INDEX] | [SYMBOL]                                                              | [KIND]        | [CAPABILITY / BOUNDARY]                                             |
| :-----: | :------------------------------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Type` (enum)                                                        | discriminant  | `Null=1`/`Int=2`/`Float=3`/`Utf8=5`/`Bool=6`/`List=12`/`Struct=13`/`FixedSizeList=16`/`Map=17`/`Dictionary=-1`/… — `type.typeId` |
|  [02]   | `DataType` + subclasses                                              | ADT           | `Null`/`Bool`/`Int(8..64)`/`Float(16/32/64)`/`Utf8`/`Binary`/`Decimal`/`Date_`/`Time`/`Timestamp`/`Interval`/`Duration` — leaf logical types |
|  [03]   | `List`/`FixedSizeList`/`Struct`/`Union`/`Map_`/`Dictionary`          | nested type   | the composite types GeoArrow geometry columns are built from       |
|  [04]   | `Visitor`                                                            | dispatcher    | `visit(node)` routes by `typeId` — get/set/builder/comparator base |
|  [05]   | `Builder<T>` / `makeBuilder` / `builderThroughIterable` / `builderThroughAsyncIterable` | factory | streaming column construction; one `<Type>Builder` per DataType via the visitor |
|  [06]   | enums `DateUnit`/`TimeUnit`/`Precision`/`IntervalUnit`/`UnionMode`/`MetadataVersion`/`BufferType` | axis | the parameter axes on the parameterized types (never subclass-per-unit) |

```ts contract
declare enum Type { Null = 1, Int = 2, Float = 3, Binary = 4, Utf8 = 5, Bool = 6, Decimal = 7, Date = 8, Time = 9, Timestamp = 10, List = 12, Struct = 13, Union = 14, FixedSizeBinary = 15, FixedSizeList = 16, Map = 17, Dictionary = -1 /* + Duration/Interval/… */ }
// GeoArrow geometry columns ARE Arrow nested types — a point is a fixed 2/3-wide coordinate, a line/ring a variable list of them:
//   Point     : FixedSizeList<Float64>[2|3]
//   LineString: List<FixedSizeList<Float64>>
//   Polygon   : List<List<FixedSizeList<Float64>>>          (+ another List for Multi* variants)
declare abstract class Visitor { visit(...args: any[]): any; getVisitFnByTypeId(typeId: Type, throwIfNotFound?: boolean): any }   // one dispatcher, routed by Type.typeId
declare function makeBuilder<T extends DataType>(options: BuilderOptions<T>): Builder<T>        // one factory, per-type builder via the visitor
```

## [04]-[IPC_AND_STREAM]

The decode/encode boundary: `tableFromIPC` reads an Arrow IPC frame (file or stream format) into a `Table`, `tableToIPC` serializes one back, and `RecordBatchReader.from` is the streaming lane for incremental batches. All are overloaded on source shape — bytes decode synchronously, a `ReadableStream`/`Promise`/`fetch` `Response` decode asynchronously — so the same call covers a `Uint8Array` frame and a streamed body. Compression is a pluggable `compressionRegistry` keyed by `CompressionType` (LZ4/ZSTD), not a fork.

| [INDEX] | [SYMBOL]                                                                                | [KIND]        | [CAPABILITY / BOUNDARY]                                             |
| :-----: | :-------------------------------------------------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `tableFromIPC<T>(source): Table<T> \| Promise<Table<T>>`                                 | decode        | bytes → sync `Table`; stream/promise/`Response` → `Promise<Table>` |
|  [02]   | `tableToIPC<T>(table, type?: 'file' \| 'stream', compressionType?: CompressionType \| null): Uint8Array` | encode | serialize a `Table` to an IPC frame, optionally compressed         |
|  [03]   | `RecordBatchReader.from(source)` / `RecordBatchFileReader` / `RecordBatchStreamReader` / `Async*` | reader | incremental `RecordBatch` stream; the file-vs-stream format is auto-detected |
|  [04]   | `RecordBatchWriter` / `RecordBatchFileWriter` / `RecordBatchStreamWriter` / `RecordBatchJSONWriter` | writer | streaming encode; `.writeAll(table)` / `.toUint8Array()`           |
|  [05]   | `ByteStream` / `AsyncByteStream` / `AsyncByteQueue`                                      | byte source   | the sync/async byte adapters IPC readers consume                   |
|  [06]   | `compressionRegistry` / `CompressionType`                                                | codec table   | pluggable IPC body compression (LZ4_FRAME/ZSTD) — a registry row   |

## [05]-[STACKING]

- [STACK: `@geoarrow/deck.gl-geoarrow` (the primary consumer, `apache-arrow` is its PEER)] — the full 14-mirror roster reads an Arrow column directly: coordinate mirrors `GeoArrowScatterplotLayer`/`GeoArrowPointCloudLayer` (point `FixedSizeList<Float64>`), `GeoArrowPathLayer`/`GeoArrowTripsLayer` (line `List<FixedSizeList>`), `GeoArrowPolygonLayer`/`GeoArrowSolidPolygonLayer` (polygon `List<List<FixedSizeList>>`), `GeoArrowArcLayer`/`GeoArrowColumnLayer`/`_GeoArrowTextLayer`; DGGS cell mirrors `GeoArrowH3HexagonLayer`/`GeoArrowS2Layer`/`GeoArrowGeohashLayer`/`GeoArrowA5Layer` (a cell-id `Utf8`/`Int` column, not a coordinate list); and `GeoArrowHeatmapLayer` (a `getWeight` `Float` column, pending the `@deck.gl/aggregation-layers` peer — next bullet). The layer's `data` prop is ONE `RecordBatch` (v0.4 grain, not a `Table`) + a `getPosition`/`getPath`/`getPolygon`/`getHexagon` accessor bound to the target column's `Data<…>` buffer, uploaded to the GPU with no per-row copy; the caller iterates `Table.batches` and mounts one layer per batch. This is the reason `viewer/geo/layers` keeps everything as an Arrow `RecordBatch`/`Vector`, never a JS array of coordinates.
- [STACK: `@deck.gl/core` + `@deck.gl/layers` + `@deck.gl/geo-layers` + `@geoarrow/geoarrow-js`] — the deck.gl `Layer`/`Deck` host the GeoArrow layers extend; `@geoarrow/geoarrow-js` (transitive) owns the GeoArrow encoding/algorithms (earcut tessellation via `initEarcutPool`). Boundary fact: `@geoarrow/deck.gl-geoarrow` peer-requires `@deck.gl/aggregation-layers` + `@math.gl/polygon` (verified peers), neither in the `[VIEWER_GEO]` roster — the heatmap family (`GeoArrowHeatmapLayer`, whose deck.gl base is in `aggregation-layers`) and the polygon tessellation (`@math.gl/polygon`) are unresolved until those peers are admitted; the `Scatterplot`/`Path`/`Polygon`/`SolidPolygon`/`Arc`/`Column` set resolves over the admitted `core`/`layers`/`geo-layers`.
- [STACK: `tableFromIPC` at the `viewer` data boundary + `wire`] — the viewer receives Arrow IPC frames (WKB geometry decoded to columnar upstream — `viewer/geo/layers` notes "WKB decode stays in `wire`"); `tableFromIPC(bytes)` decodes the frame to a `Table`, and the geometry `Vector` is handed straight to the GeoArrow layer. `apache-arrow` owns the columnar decode; `wire` owns the WKB→GeoArrow projection.
- [STACK: `.api/effect-platform-browser.md` `BrowserWorker` decode pool] — Arrow decode is a pure transform; heavy `tableFromIPC` frames decode off-main-thread on the `BrowserWorker.layer(spawn)` pool (`runtime browser/fetch`), the `Table` (or its transferable buffers) posted back to the main thread for layer binding. The decode wraps in `Effect.try`/`Effect.sync` on the worker side — Arrow throws on malformed frames, so the boundary is a typed `Effect` failure, never a bare exception in layer code.
- [STACK: `@turf/turf` planar ops] — `turf` consumes GeoJSON, not Arrow; where a planar op (buffer/intersect) is needed, the coordinate `Vector` materializes to GeoJSON at that one seam and back, so the Arrow columnar path stays the transport and `turf` the analysis leaf.

## [06]-[RAIL_LAW]

- Owns: the Arrow columnar containers (`Table`/`RecordBatch`/`Vector`/`Data`/`Schema`/`Field`), the `Type`-discriminated `DataType` ADT + `Visitor` dispatch + `Builder` family, and the IPC decode/encode (`tableFromIPC`/`tableToIPC`, the `RecordBatch` readers/writers, `compressionRegistry`).
- Accept: `tableFromIPC` to decode a frame at the `viewer` boundary; a `RecordBatch` (from `Table.batches`) handed zero-copy to a `GeoArrow*Layer` via `data: RecordBatch` + a coordinate-column accessor; `makeBuilder`/`vectorFromArray` for column construction; the `Visitor`/`Type` enum for per-type dispatch; off-thread decode on the `BrowserWorker` pool wrapped in `Effect`.
- Reject: materializing an Arrow column to a JS array of rows before a GeoArrow layer (defeats the zero-copy path — bind the `Vector`); a parallel type hierarchy where a `DataType` row + visit arm suffices; importing `apache-arrow` outside `scope:viewer`; the `arrow2csv` bin as a library surface; re-implementing IPC decode where `tableFromIPC` covers the source shape; WKB decode in `viewer` (it stays in `wire`).
- Boundary: browser binds the `Arrow.dom` build target (WHATWG streams). `apache-arrow` is the columnar transport + type system; `@geoarrow/geoarrow-js` owns the GeoArrow encoding, deck.gl the GPU render, `turf` the planar analysis. The aggregation-family GeoArrow layers need `@deck.gl/aggregation-layers` + `@math.gl/polygon` peers not currently in the `[VIEWER_GEO]` roster.
