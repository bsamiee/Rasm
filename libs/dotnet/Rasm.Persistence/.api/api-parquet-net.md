# [RASM_PERSISTENCE_API_PARQUET_NET]

`Parquet.Net` is the pure-managed Parquet codec: a schema model, a row-group reader and writer over any `Stream`, and a reflection-driven object serializer, with no native library and no Arrow dependency. It reaches this folder as the codec floor beneath `Ara3D.BimOpenSchema.IO`'s Parquet-zip leg rather than as a directly-referenced writer, and the folder's own Parquet lane stays the native `ParquetSharp.Arrow` codec — the two meet at the file format, never the assembly.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file actors and their options

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `ParquetReader`         | class         | footer-first reader over a file path or `Stream`            |
|  [02]   | `ParquetWriter`         | sealed class  | row-group writer over a `Stream`, append-capable            |
|  [03]   | `ParquetRowGroupReader` | class         | one row group's typed and raw column reads                  |
|  [04]   | `ParquetRowGroupWriter` | class         | one row group's per-field writes                            |
|  [05]   | `ParquetOptions`        | class         | codec, encoding, pooling, and type-mapping policy           |
|  [06]   | `CompressionMethod`     | enum          | `None` `Snappy` `Gzip` `Lzo` `Brotli` `LZ4` `Zstd` `Lz4Raw` |
|  [07]   | `EncodingHint`          | enum          | per-column encoding steer over `ColumnEncodingHints`        |
|  [08]   | `ParquetException`      | exception     | the codec's own fault channel                               |

[PUBLIC_TYPE_SCOPE]: schema model

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `ParquetSchema`                                                 | class         | ordered `Field` list, dotted-path lookup, equality |
|  [02]   | `Field`                                                         | abstract      | the base every schema node derives from            |
|  [03]   | `DataField`                                                     | class         | one leaf column: name, CLR type, nullability       |
|  [04]   | `StructField` / `ListField` / `MapField`                        | class         | the three nested shapes                            |
|  [05]   | `DecimalDataField` / `BigDecimalDataField`                      | class         | precision and scale carriers over decimal columns  |
|  [06]   | `DateTimeDataField` / `TimeOnlyDataField` / `TimeSpanDataField` | class         | temporal columns with their unit and format        |
|  [07]   | `FieldPath`                                                     | value         | the dotted path a nested lookup resolves           |
|  [08]   | `SchemaType`                                                    | enum          | the node discriminant                              |

[PUBLIC_TYPE_SCOPE]: serialization and column payloads

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `ParquetSerializer`         | static class  | reflection-driven object and untyped-dictionary codec |
|  [02]   | `DeserializationResult<T>`  | result        | the decoded rows beside the schema they came from     |
|  [03]   | `LazyDeserialisationResult` | result        | streamed rows under `MaxRowGroups` and read callbacks |
|  [04]   | `ProgressCallbacks`         | seats         | `SchemaRead`, `DataFieldReadStarted`, `…ReadFinished` |
|  [05]   | `RawColumnData`             | value         | values beside definition and repetition levels        |
|  [06]   | `DataColumnStatistics`      | value         | per-column min, max, null and distinct counts         |
|  [07]   | `BigDecimal` / `NanoTime`   | value         | the two payload types no BCL primitive carries        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: open, write, and drain a file — every factory also takes an optional `ParquetOptions` and a trailing `CancellationToken`, and every reader and writer is `IAsyncDisposable`

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `ParquetReader.CreateAsync(string)`                              | factory  | open a path                           |
|  [02]   | `ParquetReader.CreateAsync(Stream, bool leaveStreamOpen = true)` | factory  | open a stream, lifetime caller-owned  |
|  [03]   | `ParquetReader.ReadSchemaAsync(string \| Stream)`                | static   | footer schema with no row read        |
|  [04]   | `ParquetReader.OpenRowGroupReader(int)`                          | instance | one row group by index                |
|  [05]   | `ParquetReader.RowGroups` `.RowGroupCount` `.Schema`             | property | the row-group and schema projections  |
|  [06]   | `ParquetReader.CustomMetadata` `.Metadata`                       | property | the footer key-values and thrift meta |
|  [07]   | `ParquetWriter.CreateAsync(ParquetSchema, Stream, bool append)`  | factory  | open a writer, optionally appending   |
|  [08]   | `ParquetWriter.CreateRowGroup()`                                 | instance | one row-group writer                  |
|  [09]   | `ParquetWriter.CustomMetadata`                                   | property | the key-value footer block            |

[ENTRYPOINT_SCOPE]: column-grain reads and writes inside one row group — `Reader` is `ParquetRowGroupReader`, `Writer` is `ParquetRowGroupWriter`, and every async leg takes a trailing `CancellationToken`

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Reader.ReadAsync<T>(DataField, Memory<T>, Memory<int>?)`                  | instance | typed read into a caller buffer         |
|  [02]   | `Reader.ReadRawAsync<T>(DataField, Memory<T>, Memory<int>?, Memory<int>?)` | instance | levels alongside values                 |
|  [03]   | `Reader.ReadRawColumnDataBaseAsync(DataField)`                             | instance | one `RawColumnData` payload             |
|  [04]   | `Reader.ColumnExists(DataField)` `.RowCount` `.RowGroup`                   | instance | presence and cardinality                |
|  [05]   | `Writer.WriteAsync<T>(DataField, ReadOnlyMemory<T>, …)`                    | instance | a value-type column                     |
|  [06]   | `Writer.WriteAsync<T>(DataField, ReadOnlyMemory<T?>, …)`                   | instance | its nullable twin                       |
|  [07]   | `Writer.WriteAsync(DataField, IReadOnlyCollection<string?>, …)`            | instance | the string column                       |
|  [08]   | `Writer.WriteAsync(DataField, IReadOnlyCollection<byte[]?>, …)`            | instance | the byte-array column                   |
|  [09]   | `Writer.CompleteValidate()`                                                | instance | refuses a group missing a schema column |

- `ReadAsync` takes repetition levels alone while `ReadRawAsync` takes definition levels THEN repetition levels, so the two argument lists are not interchangeable at the same arity.
- Every `Writer.WriteAsync` overload trails an optional `ReadOnlyMemory<int>?` repetition-level buffer; the two `ReadOnlyMemory<T>` forms trail a `Dictionary<string, string>?` column-metadata slot beyond it.

[ENTRYPOINT_SCOPE]: object serialization over the same file actors — every verb is static on `ParquetSerializer`, takes an optional `ParquetOptions` and a trailing `CancellationToken`, and spells untyped rows `IReadOnlyCollection<IDictionary<string, object?>>`

| [INDEX] | [SURFACE]                                             | [SHAPE] | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `SerializeAsync<T>(IEnumerable<T>, Stream \| string)` | static  | POCO rows out, returning the derived schema |
|  [02]   | `SerializeUntypedAsync(rows, ParquetSchema, Stream)`  | static  | dictionary rows against a supplied schema   |
|  [03]   | `DeserializeAsync<T>(Stream \| string, int?)`         | static  | typed rows, one row group or all            |
|  [04]   | `DeserializeUntypedAsync(Stream, int?)`               | static  | dictionary rows                             |

- Both serialize verbs also take an `IReadOnlyDictionary<string, string>?` custom-metadata slot, and both deserialize verbs read the whole file when their `int? rowGroupIndex` is null.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ParquetOptions.CompressionMethod` defaults to `Snappy` and `CompressionLevel` to `SmallestSize`, so an extract written without an explicit policy carries the fast codec at the slow level — the two knobs move together or the pairing is unintentional.
- `ParquetOptions.RowGroupSize` is nullable over a `DefaultRowGroupSize` of one million rows, and every row group buffers whole before flush, so group size IS the writer's peak-memory knob.
- `UseHardwareAcceleration` and `PreferUntypedString` are STATIC, so they are process-wide policy no per-file `ParquetOptions` instance can override — one composition sets them, never a per-extract caller.
- `TreatBigIntegersAsDates` defaults TRUE, so a large integer column round-trips as a date unless the option is cleared; `UseDateOnlyTypeForDates`, `UseTimeOnlyTypeForTimeMillis`, and `UseTimeOnlyTypeForTimeMicros` each default FALSE, so temporal columns decode to `DateTime` unless a reader opts into the narrower BCL types.
- `ParquetReader.CreateAsync(Stream, …)` defaults `leaveStreamOpen` TRUE, the opposite of the BCL convention, so a caller disposing the reader alone leaks the stream unless it owns the lifetime deliberately.
- `ParquetRowGroupWriter.CompleteValidate` refuses a group that skipped a schema column, so a partially written group faults at close rather than emitting a short file.
- Reads and writes take caller-owned `Memory<T>`/`ReadOnlyMemory<T>` buffers over a `DataField`, so column traffic is buffer-grain and a row-shaped consumer transposes on its own side.

[STACKING]:
- `Ara3D.BimOpenSchema[.IO]`(`api-ara3d-bimopenschema.md`): the only composed consumer — `WriteToParquetZip`/`ReadBimDataFromParquetZip` drive this codec at Brotli over an `IDataSet`, and the central pin exists so the solution versions that codec rather than inheriting whatever the IO package resolves.
- `ParquetSharp`(`api-parquetsharp.md`): the folder's own Parquet lane, native and Arrow-bridged, reading and writing the SAME file format with a disjoint object model — a file crosses between them, never a type.
- `api-arrow`: absent by construction here; this codec exposes no `RecordBatch` and no C-Data bridge, which is exactly why the folder's record-batch lane rides `ParquetSharp.Arrow` instead.

[LOCAL_ADMISSION]:
- Transitive reach is the design: `CentralPackageTransitivePinningEnabled` surfaces the `Ara3D.BimOpenSchema.IO` dependency as a central row so the solution governs the version, and a direct `PackageReference` mints a second Parquet runtime beside `ParquetSharp` in one folder.
- Folder fences writing Parquet compose `ParquetSharp.Arrow`; this surface enters only where the payload is already a BimOpenSchema `IDataSet` crossing the Parquet-zip leg.
- Encryption is unreachable here — this codec ships no Parquet Modular Encryption, so a sensitive extract writes through the `ParquetSharp` PME lane, never this one.
