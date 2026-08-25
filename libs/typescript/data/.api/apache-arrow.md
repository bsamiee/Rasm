# [TS_DATA_API_APACHE_ARROW]

`apache-arrow` owns the columnar interchange the analytical lane meets at: the container values every engine row emits or accepts, the column-construction surface a schema-declared roster lands rows through, the IPC codec pair, and the streaming reader `lane/olap` lifts onto `Effect`/`Stream`. `ui`'s catalog of the same package (`libs/typescript/ui/.api/apache-arrow.md`) owns the viewer-tier `Visitor` dispatch and the per-`DataType` builder subclasses.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the columnar containers the lane seams exchange

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `Table<T extends TypeMap>`                | interface     | batched columns over a `Schema<T>` — the decode result    |
|  [02]   | `RecordBatch<T>`                          | class         | one aligned column set — the IPC unit and stream grain    |
|  [03]   | `Vector<T extends DataType>`              | interface     | one logical column; `.toArray()` a zero-copy view         |
|  [04]   | `Schema<T>` / `Field<T>`                  | class         | names and logical types on every frame, declaration-order |
|  [05]   | `CompressionType` / `compressionRegistry` | enum + const  | IPC body compression (`LZ4_FRAME`/`ZSTD`); its registry   |
|  [06]   | `DataType` + `Type`                       | ADT + enum    | the logical-type algebra a `Field` and a `Builder` take   |
|  [07]   | `Builder<T extends DataType>`             | class         | streaming column construction, one subclass per type      |

- `Schema` and `Field` construct: `new Field(name, type, nullable?, metadata?)` and `new Schema(fields, metadata?, dictionaries?)`, so a column roster declared in order mints its schema with no builder chain.
- `Table` construction overloads on its argument shape — `new Table(batches)`, `new Table(columnRecord)`, and `new Table(schema, columnRecord)` where the record maps each field name to its `Vector`; the schema-carrying form is the one that fixes field order off a declaration rather than off object key insertion.
- `DataType` leaves construct bare (`new Utf8()`, `new Float64()`, `new Uint64()`, `new Bool()`, `new TimestampNanosecond(timezone?)`) while the nested ones take child fields — `new List(new Field("item", child, true))` and `new Map_(new Field("entries", new Struct([keyField, valueField]), false), keysSorted?)`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: IPC decode, encode, the streaming reader, and column construction

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------- | :------- | :-------------------------------------------------------------- |
|  [01]   | `tableFromIPC(bytes) -> Table`                         | static   | sync decode; a stream/promise source returns `Promise`          |
|  [02]   | `tableToIPC(table, type?, compression?) -> Uint8Array` | static   | file/stream IPC encode with optional body compression           |
|  [03]   | `RecordBatchReader.from(source)`                       | factory  | opens the incremental reader, sync or async by source           |
|  [04]   | `reader[Symbol.asyncIterator]()`                       | instance | batch pull; no whole-`Table` materialization                    |
|  [05]   | `isArrowTable(x)` / `isArrowRecordBatch(x)`            | static   | narrowing guards the ingest discriminant folds through          |
|  [06]   | `Table.numRows` / `Table.schema.fields`                | instance | row arity and the ordered field roster a projection walks       |
|  [07]   | `Table.getChild(name) -> Vector \| null`               | instance | one column BY NAME; a null answer means the frame lacks it      |
|  [08]   | `Vector.get(index) -> value \| null`                   | instance | one cell, so a bounded frame projects to rows without `toArray` |
|  [09]   | `makeBuilder({ type, nullValues? }) -> Builder<T>`     | factory  | the per-`DataType` builder a declared column bank mints         |
|  [10]   | `builder.append(value) -> this`                        | instance | one cell appended; `set(index, value)` is the random-access arm |
|  [11]   | `builder.finish().toVector() -> Vector<T>`             | instance | seal the builder and take its column; `flush()` yields `Data`   |
|  [12]   | `makeVector(data)` / `vectorFromArray(values, type?)`  | factory  | one `Vector` from a typed array or from JS values               |
|  [13]   | `makeTable(input)` / `tableFromArrays(input)`          | factory  | a whole `Table` from a typed-array or JS-array column map       |

- `Builder` is the streaming form and `vectorFromArray` the whole-array form of one construction — a bank of builders fed row by row is what lets a row-shaped producer land N columns in one pass, where the array form re-materializes each column first.
- TRAP: every `Timestamp` builder takes a JS millisecond NUMBER whatever unit its `DataType` declares, and a `bigint` throws `Cannot mix BigInt and other types` at `append`; a nanosecond-declared column therefore crosses this construction seam at millisecond grain, so a producer holding nanos states that loss where it converts rather than at each call site.
- TRAP: `MapBuilder` declares its input as the read-side `MapRow` proxy this side never constructs, while its runtime accepts the JS `Map` a producer holds — so the declared type and the accepted value disagree and the crossing is stated as a boundary adapter at the one bank, never as a per-column cast.
- `Uint64`/`Int64` builders take `bigint`, `Utf8` takes `string`, `Bool` takes `boolean`, `Float64` takes `number`, and `List` takes the JS array of its child cells; each `Vector` seals through `finish().toVector()` and the record of sealed vectors is what `new Table(schema, columns)` binds.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every engine row joins the analytical lane by emitting or accepting Arrow IPC; the `Table` is the one columnar value crossing every seam, never a per-engine row shape.

[STACKING]:
- `@duckdb/duckdb-wasm`(`.api/duckdb-duckdb-wasm.md`): `query()` returns an `arrow.Table`; ingest discriminates on `isArrowTable` — a live `Table` rides `insertArrowTable`, IPC bytes ride `insertArrowFromIPCStream`.
- `@duckdb/node-api`(`.api/duckdb-node-api.md`): result IPC egress folds through `tableFromIPC`, outbound serialization through `tableToIPC`.
- `parquet-wasm`(`.api/parquet-wasm.md`): `fromIPCStream(buf)` ingests and `intoIPCStream()` emits the shared IPC stream buffer; `toFFI()`/`intoFFI()` cross the Arrow C Data Interface zero-copy.
- `@effect/sql-clickhouse`(`.api/effect-sql-clickhouse.md`): Arrow IPC carries interchange from the at-scale row back to the embedded rows and the viewer.
- `@qualithm/arrow-flight-client`(`.api/qualithm-arrow-flight-client.md`): `decodeFlightDataToTable` lands `FlightData` on Arrow columns, `encodeRecordBatchesToFlightData` re-encodes for `doPut`.
- `lane/olap`: `_ROWED` projects a worker-answered `Table` to row records through `schema.fields`, `getChild`, and `Vector.get`, so a bounded diagnosis frame reads BY NAME and the two driver grains meet at one normalization.
- `lane/olap`: `Olap.wire.roster(columns)` is the ONE schema-declared landing — one `[name, token]` column declaration mints the `Field` roster, the `Schema`, the `makeBuilder` bank its `table` fold appends rows through, and the DuckDB scan residency, so field order, builder set, reader ordinals, and the row type all derive from that declaration and no producer pairs a column name with a vector by hand.
- `lane/olap`: `Olap.wire.decode`/`.encode` fold `tableFromIPC`/`tableToIPC` through `Effect.try` into `OlapFault` (`reason: "wire"`), `Olap.wire.batches` lifts `RecordBatchReader` iteration through `Stream.fromAsyncIterable` with the same fault mint, and decoded `Table` values reach `ui`'s geoarrow plane without row materialization.

[LOCAL_ADMISSION]:
- Large interchange rides `RecordBatchReader` batch iteration; `tableFromIPC` whole-frame decode admits only where the frame is provably bounded.
- Row-shaped egress exists only at the final consumer projection, never between engine seams.
