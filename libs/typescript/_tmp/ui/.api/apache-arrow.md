# [API_CATALOGUE] apache-arrow

`apache-arrow` is the JS reference implementation of the Arrow columnar format and the columnar substrate the GeoArrow deck.gl layers consume zero-copy. The container hierarchy is four nested shapes generic over a `TypeMap`/`DataType`: `Table` (batched columns) → `RecordBatch` (one aligned column set, the IPC unit and the layer `data` grain) → `Vector` (one logical column, chunked over `Data[]`) → `Data` (the raw buffer tuple — validity/offset/value/child), with `Schema`/`Field` carrying names + logical types + metadata. The type system is the APPROACH-collapse spine: ONE `Type`-enum-discriminated `DataType` ADT dispatched by ONE `Visitor` — get/set/builder/comparator/IPC-load all route by `type.typeId`, so a new logical type is a `DataType` subclass + a `Type` member + a visit arm, never a parallel class hierarchy. In Rasm it is `scope:viewer` project-local (admitted only by the `ui/viewer` Nx project, compile-time excluded from the non-spatial core): a declared PEER of `@geoarrow/deck.gl-geoarrow` (`geoarrow.md`) whose `GeoArrow*Layer` roster binds an Arrow geometry column's flat typed-array buffers to the GPU with no per-row copy. The decode is wire-owned — the `interchange` `GeometryRail` re-exports the decoded `RecordBatch`; `ui` imports the apache-arrow TYPE VOCABULARY for zero-copy column binding + `StructRowProxy` picking and never re-mints the IPC/WKB decode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-arrow`
- package / version: `apache-arrow` @ `21.1.0`
- license: `Apache-2.0`
- module: `type: commonjs`; THREE build targets condition-selected by `exports["."]` — `Arrow.dom` (browser `import`, WHATWG streams), `Arrow.node` (node streams), `Arrow.d.ts` (base); `./*` exposes per-concern subpath imports; `sideEffects: false`
- asset: TSDECL barrel `Arrow.dom.d.ts` over per-concern `.d.ts` — `table`/`recordbatch`/`vector`/`data`/`schema`/`type`/`enum`/`factories`/`builder`/`visitor`/`interfaces`/`ipc/*` (`assay api resolve apache-arrow` → `21.1.0`, restored)
- deps: `flatbuffers` (the IPC metadata wire), `tslib`, `@swc/helpers`, `json-bignum`; `command-line-args`/`command-line-usage` back the `arrow2csv` bin — a CLI, NOT part of the library surface. No peer.
- runtime: browser lane binds `Arrow.dom` (WHATWG `ReadableStream`/`Blob`); `viewer` uses the DOM target. `Vector.toArray()` returns the backing `TypedArray` view, not a row copy
- plane: `plane:ui` / `scope:viewer` — project-local to the `ui/viewer` Nx project; the core `ui` project never imports it
- rail: viewer/columnar — the `[VIEWER_GEO]` group (`maplibre-gl`, `@deck.gl/*`, `@geoarrow/deck.gl-geoarrow`, `apache-arrow`)
- role: the columnar container + `Type`-discriminated type vocabulary the GeoArrow deck.gl layers consume zero-copy; the IPC codec primitive whose decode is wire-owned (`interchange` `GeometryRail`), never re-run in `ui`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the columnar containers — `Table` → `RecordBatch` → `Vector` → `Data` over `Schema`/`Field`
- rail: viewer/columnar
- Four nested shapes, each generic over a `TypeMap`/`DataType`. Construction is factory functions (`makeData`/`makeVector`/`makeTable`), not constructors, so the buffer layout stays an implementation detail. `RecordBatch` is the layer's `data` grain; `Vector.toArray()`/`Data.values` are the zero-copy escapes the GPU binds.

| [INDEX] | [SYMBOL]                                                                              | [KIND]        | [CAPABILITY / BOUNDARY]                                                                                                   |
| :-----: | :------------------------------------------------------------------------------------ | :------------ | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Table<T extends TypeMap>`                                                             | class         | batched columns; `.schema`/`.batches: RecordBatch<T>[]`/`.numRows`/`.numCols`/`.data: Data<Struct<T>>[]`/`.getChild(name)`/`.select(names)`/`.slice(begin?, end?)` |
|  [02]   | `RecordBatch<T>`                                                                       | class         | one aligned column set — the IPC unit and the layer `data` grain; `.schema`/`.get(i): StructRowProxy \| null`/`.at(i)`/`[Symbol.iterator]`/`.toArray(): StructRowProxy[]`/`.getChild(name)`/`.slice` |
|  [03]   | `Vector<T extends DataType>`                                                           | class         | one logical column (chunked over `Data[]`); `.type`/`.data: ReadonlyArray<Data<T>>`/`.length`/`.stride`/`.nullCount`; `.get(i)`/`.at(i)`/`.isValid(i)`/`.toArray(): T['TArray']`/`.getChild(name)`/`.slice`/`.concat`/`.memoize` |
|  [04]   | `Data<T>` / `makeData<T>(props: DataProps<T>)`                                          | class/factory | the raw buffer tuple — `.values`/`.valueOffsets`/`.nullBitmap`/`.typeIds` + `.children`/`.offset`/`.length`/`.stride`; `makeData` is overloaded per `DataType` (the visitor dispatches) |
|  [05]   | `Schema<T>` / `Field<T>(name, type, nullable?, metadata?)`                              | class         | column names + logical types + `Map` metadata + `.dictionaries` registry; `Schema.names`/`.fields`; `Field(name: string, type: T, nullable?: boolean, metadata?: Map<string,string> \| null)` |
|  [06]   | `makeVector` / `vectorFromArray` / `makeTable` / `tableFromArrays` / `tableFromJSON`   | factory       | build a `Vector`/`Table` from typed arrays, JS arrays, or column maps — column construction without touching the buffer layout |

[PUBLIC_TYPE_SCOPE]: the `Type`-discriminated `DataType` ADT + `Visitor` dispatch + `Builder` family — ONE enum, ONE ADT, ONE dispatcher
- rail: viewer/columnar
- Not 40 unrelated type/builder classes but ONE `Type` enum discriminating ONE `DataType` ADT, dispatched by ONE `Visitor`. Every per-type behavior (value get/set, comparison, builder, IPC buffer load) is a `visit<Type>` arm, so the `DataType` rows are SEED members of a generated family, not the mechanism. A new logical type is a `DataType` subclass + a `Type` member + a visit arm; consumer code branches on `type.typeId` or lets the visitor route. GeoArrow geometry columns ARE Arrow nested types — a point is `FixedSizeList<Float64>[2|3]` (the `Vector.stride` is the fixed width), a line/ring a `List` of them.

| [INDEX] | [SYMBOL]                                                                                          | [KIND]        | [CAPABILITY / BOUNDARY]                                                                                                   |
| :-----: | :------------------------------------------------------------------------------------------------ | :------------ | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Type` (enum)                                                                                     | discriminant  | `Null=1`/`Int=2`/`Float=3`/`Binary=4`/`Utf8=5`/`Bool=6`/`Decimal=7`/`Date=8`/`Time=9`/`Timestamp=10`/`List=12`/`Struct=13`/`Union=14`/`FixedSizeBinary=15`/`FixedSizeList=16`/`Map=17`/`Dictionary=-1` — the `type.typeId` |
|  [02]   | `DataType` + leaf subclasses                                                                      | ADT           | `Null`/`Bool`/`Int8..64`/`Uint8..64`/`Float`/`Float16/32/64`/`Utf8`/`Binary`/`Decimal`/`Date_`/`Time`/`Timestamp`/`Interval`/`Duration` — the leaf logical types the scalar/color/normal accessors bind (`Float`, `Uint8`, `Float32`) |
|  [03]   | `List` / `FixedSizeList` / `Struct` / `Union_` / `Map_` / `Dictionary`                            | nested type   | the composite types GeoArrow geometry columns are built from — `FixedSizeList<Float64>` coordinate, `List<FixedSizeList>` ring, `List<List<FixedSizeList>>` polygon |
|  [04]   | `Visitor`                                                                                          | dispatcher    | `visit(node)` / `getVisitFn(node, throwIfNotFound?)` / `getVisitFnByTypeId(typeId: Type, throwIfNotFound?)` — routes by `typeId`; the get/set/builder/comparator base |
|  [05]   | `Builder<T>` / `makeBuilder` / `builderThroughIterable` / `builderThroughAsyncIterable`           | factory       | streaming column construction; one `<Type>Builder` per `DataType` via the visitor, driven by a `BuilderOptions<T>` |
|  [06]   | enums `DateUnit`/`TimeUnit`/`Precision`/`IntervalUnit`/`UnionMode`/`MetadataVersion`/`BufferType`/`CompressionType` | axis | the parameter axes on the parameterized types (never subclass-per-unit) — a `Timestamp` carries a `TimeUnit`, not a `Timestamp<Millisecond>` subclass |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the IPC codec primitive — decode/encode at the wire boundary (wire-owned in Rasm)
- rail: viewer/columnar
- `tableFromIPC` reads an Arrow IPC frame (file or stream format) into a `Table`, overloaded on source shape — bytes decode synchronously, a `ReadableStream`/`Promise`/`fetch` `Response` decode asynchronously — so one call covers a `Uint8Array` frame and a streamed body. In Rasm this decode is owned at the wire boundary by the `interchange` `GeometryRail`; `ui` consumes the re-exported `RecordBatch`, never re-running the codec.

| [INDEX] | [SYMBOL]                                                                                                 | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                                                 |
| :-----: | :------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `tableFromIPC<T>(source): Table<T> \| Promise<Table<T>>`                                                  | decode         | bytes → sync `Table`; stream/promise/`Response` → `Promise<Table>` — WIRE-owned (`interchange`), never re-run in `ui` |
|  [02]   | `tableToIPC<T>(table, type?: 'file' \| 'stream', compressionType?: CompressionType \| null): Uint8Array`  | encode         | serialize a `Table` to an IPC frame, optionally LZ4/ZSTD-compressed                    |
|  [03]   | `RecordBatchReader.from(source)` / `RecordBatchFileReader` / `RecordBatchStreamReader` / `Async*Reader`   | reader         | incremental `RecordBatch` stream; the file-vs-stream format auto-detects              |
|  [04]   | `RecordBatchWriter` / `RecordBatchFileWriter` / `RecordBatchStreamWriter` / `RecordBatchJSONWriter`       | writer         | streaming encode; `.writeAll(table)` / `.toUint8Array()`                               |
|  [05]   | `ByteStream` / `AsyncByteStream` / `AsyncByteQueue`                                                       | byte source    | the sync/async byte adapters IPC readers consume                                      |
|  [06]   | `compressionRegistry` / `CompressionType`                                                                 | codec table    | pluggable IPC body compression (LZ4_FRAME/ZSTD) — a registry row keyed by `CompressionType`, never a fork |

[ENTRYPOINT_SCOPE]: zero-copy column access + construction — what `ui` binds
- rail: viewer/columnar
- The `ui` side: fan a `Table` to per-batch layers, pick the geometry/scalar column, bind its backing `TypedArray` to the GPU, read a picked feature as a `StructRowProxy`. Never materialize rows.

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]  | [CONSUMER / BOUNDARY]                                                                 |
| :-----: | :--------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Table.batches: RecordBatch<T>[]`                                                   | batch fan-out   | `geoarrow.md` — iterate to mount ONE `GeoArrow*Layer` per `RecordBatch` (the v0.4 grain); a chunked `Table` becomes a deck `LayersList` |
|  [02]   | `RecordBatch.getChild(name)` / `Vector.getChild(name)` / `.getChildAt(i)`           | column pick     | the geometry/scalar column handed to a GeoArrow accessor; `getChild` walks nested `Struct`/`List` columns |
|  [03]   | `Vector.toArray(): T['TArray']` / `Data.values`                                     | zero-copy view  | the backing `TypedArray` deck.gl uploads as a `BinaryAttribute` — no per-row copy; `Data.stride` is the fixed coordinate width |
|  [04]   | `RecordBatch.get(i): StructRowProxy \| null` / `.at(i)` / `[Symbol.iterator]`        | zero-copy row   | a picked feature as an Arrow row proxy (`GeoArrowPickingInfo.object`), never a materialized GeoJSON `Feature`; a selection reads the proxy's key column directly |
|  [05]   | `makeBuilder` / `vectorFromArray` / `makeData`                                      | construct       | `ui`-side column synthesis from typed arrays or per-type builders when a column is minted locally rather than decoded upstream |

## [04]-[IMPLEMENTATION_LAW]

[COLUMNAR_TOPOLOGY]:
- four nested shapes: `Table` (batched columns) → `RecordBatch` (one aligned column set, the IPC unit and the layer `data` grain) → `Vector` (one logical column, chunked over `Data[]`) → `Data` (the raw buffer tuple: `.values`/`.valueOffsets`/`.nullBitmap`/`.typeIds` + `.children`). `Schema`/`Field` carry names + logical types + `.dictionaries`.
- GeoArrow geometry columns ARE Arrow nested types: a point is `FixedSizeList<Float64>[2|3]` (the fixed width is `Vector.stride`), a line/ring `List<FixedSizeList<Float64>>`, a polygon `List<List<FixedSizeList<Float64>>>` (+ another `List` for `Multi*` variants). A color column is `FixedSizeList<Uint8>[4]` (RGBA), a scalar is `Float`, an animated-path timestamp is `List<Float>`.
- ONE `Type` enum discriminates ONE `DataType` ADT dispatched by ONE `Visitor`: value get/set, comparison, builder, and IPC buffer load all route by `type.typeId`. A new logical type is a `DataType` subclass + a `Type` member + a visit arm — never a parallel class hierarchy; consumer code branches on `type.typeId` or lets the visitor route.
- `Vector.toArray()` is the zero-copy escape to the backing `TypedArray`; `Data.values`/`.valueOffsets` are the flat buffers the GPU binds directly — the whole point of the columnar path is that no JS row object is ever built between decode and upload.

[STACKING_LAW]:
- `@geoarrow/deck.gl-geoarrow` (`geoarrow.md`) is the sole `ui` consumer and apache-arrow is its PEER: every `GeoArrow*Layer` binds an Arrow column directly — the coordinate mirrors (point `FixedSizeList<Float64>`, line `List<FixedSizeList>`, polygon `List<List<FixedSizeList>>`), the DGGS cell mirrors (a cell-id `Utf8`/`Int` column), and the accessor family (`FloatAccessor` over `Data<Float>`, `ColorAccessor` over `Data<FixedSizeList<Uint8>>`, `NormalAccessor` over `Data<FixedSizeList<Float32>>`, `TimestampAccessor` over `Data<List<Float>>`). The layer `data` prop is ONE `RecordBatch` (the v0.4 grain, not a `Table`); the caller iterates `Table.batches` and mounts one layer per batch. A picked feature returns as a `StructRowProxy` (`GeoArrowPickingInfo.object`) — Arrow's zero-copy row view, no JS object built.
- `interchange` `GeometryRail` (`@rasm/ts/interchange`) is the wire-owned decode source: the decoded `RecordBatch` VALUE arrives from the rail, which re-exports `RecordBatch` (`import type { RecordBatch } from "@rasm/ts/interchange"`, the design-page `"n"` alias). The WKB→GeoArrow→Arrow-IPC decode (`tableFromIPC`/`RecordBatchReader`) is wire-owned and runs ONCE at the `interchange` boundary; `ui` imports only the apache-arrow TYPE VOCABULARY (`Data`/`Vector`/`FixedSizeList`/`List`/`Struct`/`StructRowProxy`/`Float`/`Uint8`) for zero-copy column binding + picking, and NEVER re-mints the decode. `render/geo.md` `GeoSeriesLayer` carries the `RecordBatch` as its `geoarrow`/`cell-index`-arm `data`; the `cell-index` arm aggregates the SAME batch with no second decode.
- `deck.gl-core.md` / `deck.gl-layers.md` / `deck.gl-geo-layers.md`: each GeoArrow layer subclasses a deck base `CompositeLayer` and uploads the column's flat `Data` buffers as a GPU `BinaryAttribute`; the aggregation family (`GeoArrowHeatmapLayer`, `@deck.gl/aggregation-layers`) + the JS-polygon tessellation (`@math.gl/polygon`) peers stay unresolved until admitted — the vector/cell/temporal set resolves over the admitted `core`/`layers`/`geo-layers`.
- universal `libs/typescript/.api/effect.md`: the `RecordBatch` is a `Schema`-decoded wire projection at the `interchange` boundary; `render/geo.md` folds the batch to a `LayersList` under `Match`/`Effect.forEach`, and `GeoSeriesLayer` is a `Data.TaggedEnum` dispatched by `$match`. Arrow throws on a malformed frame, so the `interchange` decode wraps in `Effect.try` — a typed failure, never a bare exception reaching `ui` layer code.
- universal `libs/typescript/.api/effect-platform-browser.md`: heavy `tableFromIPC`/`RecordBatchReader` decode runs off-main-thread on the `BrowserWorker.layer` decode pool `ui` declares as a port, the `Table` (or its transferable buffers) posted back for layer binding. That decode is the `interchange` wire boundary's, not `ui`'s — `ui` only binds the posted-back `RecordBatch`; the earcut `threads` pool (`geoarrow.md` `initEarcutPool`) is a separate vendored worker path for triangulation.

[LOCAL_ADMISSION]:
- import apache-arrow for its TYPE VOCABULARY (`Data`/`Vector`/`FixedSizeList`/`List`/`Struct`/`StructRowProxy`/`Float`/`Uint8`) to bind columns + pick rows; take the decoded `RecordBatch` VALUE from `@rasm/ts/interchange`, never re-run `tableFromIPC`/the WKB decode in `ui`.
- bind a column's `Data<…>` buffer straight to a GeoArrow accessor and read a picked feature as a `StructRowProxy`; never materialize `RecordBatch → Array<object>` for a per-row accessor — that discards the whole point of GeoArrow.
- iterate `Table.batches` and mount one `GeoArrow*Layer` per `RecordBatch` (the v0.4 grain); a chunked `Table` is a `LayersList`.
- branch on `type.typeId` or let the `Visitor` route; never author a parallel type hierarchy where a `DataType` row + visit arm suffices.
- import apache-arrow only under `scope:viewer`; the core `ui` project excludes it. Treat the `arrow2csv` bin as a CLI, never a library surface.

[RAIL_LAW]:
- package: `apache-arrow`
- owns: the Arrow columnar containers (`Table`/`RecordBatch`/`Vector`/`Data`/`Schema`/`Field`), the `Type`-discriminated `DataType` ADT + `Visitor` dispatch + `Builder` family, and the IPC codec primitive (`tableFromIPC`/`tableToIPC`, the `RecordBatch` readers/writers, `compressionRegistry`)
- accept: the `RecordBatch` (from `@rasm/ts/interchange`, iterated off `Table.batches`) handed zero-copy to a `GeoArrow*Layer` via `data: RecordBatch` + a column accessor; `Vector.toArray()`/`Data.values` bound to a GPU `BinaryAttribute`; `RecordBatch.get(i)` → `StructRowProxy` for picking; `makeBuilder`/`vectorFromArray` for `ui`-side column synthesis; the `Visitor`/`Type` enum for per-type dispatch
- reject: re-running the IPC/WKB decode in `ui` (wire-owned in `interchange`); materializing an Arrow column to a JS array of rows before a GeoArrow layer (bind the `Data` buffer); a parallel type hierarchy where a `DataType` row + visit arm suffices; importing apache-arrow outside `scope:viewer`; the `arrow2csv` bin as a library surface
- boundary: browser binds the `Arrow.dom` build target (WHATWG streams). apache-arrow is the columnar transport + type vocabulary; the `interchange` `GeometryRail` owns the decode, `@geoarrow/geoarrow-js` the GeoArrow encoding, deck.gl the GPU render
