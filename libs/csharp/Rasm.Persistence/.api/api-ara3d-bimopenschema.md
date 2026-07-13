# [RASM_PERSISTENCE_API_ARA3D_BIMOPENSCHEMA]

`Ara3D.BimOpenSchema` supplies the columnar struct-of-arrays BIM analytics schema — a string-interned, typed-index entity/parameter/relation graph (`BimData`) authored through one interning `BimDataBuilder`, projected to a generic columnar `Ara3D.DataTable` `IDataSet` (eleven named tables) and to the denormalized `ExpandedBIMData` join view. `Ara3D.BimOpenSchema.IO` supplies the codec leg: read/write the `BimData` to a Parquet-zip (`Parquet.Net`, one `.parquet` per table, Brotli), a DuckDB file (`DuckDB.NET.Data.Full`, bulk-appended tables), an Excel workbook (`ClosedXML`, one sheet per table), and a gzipped JSON (`System.Text.Json`). The eleven columnar tables ARE the tabular BIM analytics frames the `Query/columnar` lane exposes: the folder's own `DuckDB.NET.Data.Full` (`api-duckdb.md`) opens the written `.duckdb` and SQL-joins the entity/parameter/relation tables, `ParquetSharp` (`api-parquetsharp.md`) + `Apache.Arrow` (`api-arrow.md`) read the same standard-format `.parquet` files into Arrow record batches, and `MiniExcel` (`api-miniexcel.md`) streams the `.xlsx` back into `IDataReader` rows via `GetReader` — the managed Parquet.Net writer and the native ParquetSharp reader interoperate at the file format, not the assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Ara3D.BimOpenSchema`
- package: `Ara3D.BimOpenSchema`
- license: MIT (`licenses.nuget.org/MIT` — Ara3D; `github.com/ara3d/bim-open-schema`)
- assembly: `Ara3D.BimOpenSchema`
- namespace: `Ara3D.BimOpenSchema`
- dependency: `Ara3D.SDK` `1.4.2` — supplies the `Ara3D.DataTable` `IDataSet`/`IDataTable`/`DataTableExtensions` columnar abstraction the model projects to
- target frameworks: `net8.0`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. The `net10.0` consumer binds `lib/net8.0`.
- rail: analytics-exchange (BIM)
- ABI floor: the bound `1.0.1` assembly is a DEBUG build (`AssemblyConfiguration("Debug")`, `DisableOptimizations | EnableEditAndContinue`) — JIT optimizations are disabled in the shipped IL, so a hot ingest loop pays the un-optimized penalty; the schema/model surface is otherwise stable. The `1.6.1` feed release rebuilt both assemblies `AssemblyConfiguration("Release")` (DEBUG-IL retired there), but `Ara3D.BimOpenSchema.IO` `1.6.1` regressed its asset TFM to `net8.0-windows7.0` (Windows-only), incompatible with the `net10.0` osx-arm64 target, so the central pin is held at `1.0.1` and the DEBUG-IL penalty is LOCKED until upstream ships a Release-built cross-platform IO.

[PACKAGE_SURFACE]: `Ara3D.BimOpenSchema.IO`
- package: `Ara3D.BimOpenSchema.IO`
- license: MIT (`licenses.nuget.org/MIT` — Ara3D)
- assembly: `Ara3D.BimOpenSchema.IO`
- namespace: `Ara3D.BimOpenSchema.IO`
- dependencies: `Ara3D.BimOpenSchema` `1.0.1`; `Ara3D.SDK` `1.4.2`; `ClosedXML` `0.105.0` (the Excel codec); `DuckDB.NET.Data.Full` `1.5.3` (the DuckDB codec + native dylib); `Parquet.Net` `6.0.3` (the managed Parquet codec)
- target frameworks: `net8.0`
- asset: runtime library, pure-managed AnyCPU at the IO layer; the native floor rides the transitives — `DuckDB.NET.Data.Full` carries the `duckdb` osx-arm64 dylib (`DuckDB.NET.Bindings.Full`), `Parquet.Net`/`ClosedXML` are pure-managed
- rail: analytics-exchange (BIM)
- ABI floor: also a DEBUG build (`AssemblyConfiguration("Debug")`, `DisableOptimizations | EnableEditAndContinue`) — the same un-optimized-IL caveat as the model assembly, so a hot codec loop pays the un-JIT-optimized penalty. Its Parquet/DuckDB codec calls (`ParquetReader.CreateAsync`/`ParquetWriter.CreateAsync`, `new DataField(...)`, `new ParquetSchema(IEnumerable<Field>)`, `DuckDBConnection.CreateAppender(string)`/`IDuckDBAppenderRow.CreateRow()`) bind the centrally-pinned `Parquet.Net` `6.0.3` and `DuckDB.NET.Data.Full` `1.5.3`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Ara3D.BimOpenSchema` columnar model
- rail: analytics-exchange

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CAPABILITY]                                                                          |
| :-----: | :----------------- | :---------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `BimData`          | columnar root     | SoA container of eleven parallel `List<T>` columns ([COLUMNS] below)                  |
|  [02]   | `BimDataBuilder`   | interning builder | interns entities/documents/points/descriptors/strings → typed indices                 |
|  [03]   | `BIMDataExtension` | static accessors  | polymorphic `Get`, index enumerators, `ToInt`, `ToDataSet`/`ToBimData` bridge         |
|  [04]   | `ExpandedBIMData`  | join view         | resolves strings; builds `RelationsFrom`/`RelationsTo`/`ParametersByEntity`/`…ByName` |

[COLUMNS]: the eleven `BimData` `List<T>` columns — `Entities`, `Documents`, `Strings`, `Points`, `Descriptors`, `Relations`, `IntegerParameters`, `DoubleParameters`, `StringParameters`, `EntityParameters`, `PointParameters`.

[RECORDS]: dimension + fact records; every `Parameter*` value row is `(EntityIndex Entity, DescriptorIndex Descriptor, <T> Value)`.

| [INDEX] | [SYMBOL]              | [SHAPE]                                                                                           |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `Entity`              | `(long LocalId, string GlobalId, DocumentIndex Document, StringIndex Name, StringIndex Category)` |
|  [02]   | `Document`            | `(StringIndex Title, StringIndex Path)`                                                           |
|  [03]   | `Point`               | `(double X, double Y, double Z)`                                                                  |
|  [04]   | `ParameterDescriptor` | `(StringIndex Name, StringIndex Units, StringIndex Group, ParameterType Type)`                    |
|  [05]   | `ParameterInt`        | `<T>` = `int`                                                                                     |
|  [06]   | `ParameterDouble`     | `<T>` = `double`                                                                                  |
|  [07]   | `ParameterString`     | `<T>` = `StringIndex`                                                                             |
|  [08]   | `ParameterEntity`     | `<T>` = `EntityIndex`                                                                             |
|  [09]   | `ParameterPoint`      | `<T>` = `PointIndex`                                                                              |
|  [10]   | `EntityRelation`      | `(EntityIndex EntityA, EntityIndex EntityB, RelationType RelationType)` — the typed graph edge    |

[ENUMS_AND_INDICES]:
- `ParameterType`: `Int`/`Double`/`Entity`/`String`/`Point` — the descriptor's value-column discriminant.
- `RelationType`: `PartOf`/`ElementOf`/`ContainedIn`/`InstanceOf`/`HostedBy`/`ChildOf`/`HasLayer`/`HasMaterial`/`ConnectsTo` (`HasConnector` aliases `ConnectsTo`=`8`).
- row-index enums (`: long` FKs, uncomparable across columns): `EntityIndex`/`DocumentIndex`/`DescriptorIndex`/`StringIndex`/`PointIndex`/`RelationIndex`.
- `ExpandedEntity`/`ExpandedParameter`/`ExpandedEntityRelation`: flat string-valued export classes; `ExpandedEntity` adds derived `Level`/`Type`/`Material`/`Room`.

[PUBLIC_TYPE_SCOPE]: `Ara3D.BimOpenSchema.IO` codecs
- rail: analytics-exchange

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [ROLE]                                                                 |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `BimDataSerializer`                 | static (top)  | `BimData` read/write over JSON / Parquet-zip / DuckDB / Excel          |
|  [02]   | `DuckDbUtils`                       | static        | `IDataSet`↔DuckDB codec ([01] members)                                 |
|  [03]   | `ParquetUtils`                      | static        | `IDataTable`/`IDataSet`↔Parquet, `Parquet.Net` ([02])                  |
|  [04]   | `ParquetUtils.ParquetColumnAdpater` | adapter       | `: IDataColumn` wrapping a Parquet `DataColumn` on read [sic spelling] |
|  [05]   | `ExcelUtils`                        | static        | `IDataSet`/`IDataTable`/ADO.NET `DataSet`→Excel, `ClosedXML` ([03])    |

- [01]-[DUCKDB]: `WriteToDuckDB`/`WriteTable`/`ReadTable`/`ReadColumnValues<T>`/`GetTableNames`/`ToDataSet<T>`.
- [02]-[PARQUET]: `WriteParquetAsync`/`WriteParquetToZipAsync`/`ReadParquetAsync`/`ReadParquetFromZipAsync`.
- [03]-[EXCEL]: `WriteToExcel` (one worksheet per table).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: author a `BimData` — `BimDataBuilder` (string interning, typed-index return)
- rail: analytics-exchange
- composition law: every member is on `BimDataBuilder`; every `Add*` interns its value into the matching column via a backing `Dictionary` (de-dup) and returns the typed index. `AddParameter` has 6 value overloads (`double`/`int`/`string`/`EntityIndex`/`PointIndex`/`Point`) — `PointIndex` binds a pre-interned point, `Point` interns inline; the descriptor is either a pre-interned `DescriptorIndex` or a `(name, units, group)` triple that interns inline.

| [INDEX] | [MEMBER]                 | [SIGNATURE]                                                                                           |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `AddEntity`              | `EntityIndex AddEntity(long localId, string globalId, DocumentIndex d, string name, string category)` |
|  [02]   | `AddDocument`            | `DocumentIndex AddDocument(string title, string pathName)`                                            |
|  [03]   | `AddDescriptor`          | `DescriptorIndex AddDescriptor(string name, string units, string group, ParameterType pt)`            |
|  [04]   | `AddString`/`AddPoint`   | `StringIndex AddString(string)` / `PointIndex AddPoint(Point)`                                        |
|  [05]   | `AddParameter` (value)   | `void AddParameter(EntityIndex e, <T> val, DescriptorIndex d)` — `<T>` per the lead's 6-overload set  |
|  [06]   | `AddParameter` (+intern) | `void AddParameter(EntityIndex e, <T> val, string name, string units, string group)`                  |
|  [07]   | `AddRelation`            | `void AddRelation(EntityIndex a, EntityIndex b, RelationType rt)`                                     |
|  [08]   | `Data`                   | field `BimData` — the accumulated columnar model                                                      |

[ENTRYPOINT_SCOPE]: read + project a `BimData` — `BIMDataExtension`
- rail: analytics-exchange
- `Get` is polymorphic — one overload per row-index enum (`StringIndex`/`EntityIndex`/`DocumentIndex`/`PointIndex`/`RelationIndex`/`DescriptorIndex`), each returning that column's row; the `…Indices` enumerators cover the `Entity`/`Document`/`Descriptor`/`String`/`Point` columns.

| [INDEX] | [SURFACE]                       | [SIGNATURE]                                                                                  |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `data.Get`                      | `Get(this BimData, <Index>)` — resolves any typed row index to its row                       |
|  [02]   | `data.EntityIndices`/`…Indices` | `IEnumerable<EntityIndex> EntityIndices(this BimData)` — per-column index enumerators        |
|  [03]   | `data.ToDataSet`                | `IDataSet ToDataSet(this BimData)` — the eleven columns → `Ara3D.DataTable` analytics bridge |
|  [04]   | `set.ToBimData`                 | `BimData ToBimData(this IDataSet)` — round-trip back                                         |
|  [05]   | `new ExpandedBIMData(data)`     | constructor — denormalized join view (resolves strings, builds relation/parameter lookups)   |
|  [06]   | `index.ToInt`                   | `int ToInt(this <Index>)` — unwrap a typed index to its `int` ordinal                        |

[ENTRYPOINT_SCOPE]: serialize a `BimData` — `BimDataSerializer`
- rail: analytics-exchange
- composition law: `FilePath` is `Ara3D.Utils.FilePath` (implicit `string` conversion). The Parquet/DuckDB/Excel paths route through `BimData.ToDataSet()`; default Parquet compression is `(CompressionMethod)4` = Brotli. `WriteToParquetZip` emits one `.parquet` per table in a Brotli zip; `WriteDuckDB` a `.duckdb` with the eleven appended tables; `WriteToExcel` an `.xlsx`, one worksheet per table; `WriteToJson` System.Text.Json, optionally gzipped.

| [INDEX] | [SURFACE]                                 | [SIGNATURE]                                                                         |
| :-----: | :---------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `data.WriteToParquetZip` / `…Async`       | `void WriteToParquetZip(this BimData, FilePath)` / `Task WriteToParquetZipAsync(…)` |
|  [02]   | `data.WriteDuckDB`                        | `void WriteDuckDB(this BimData, FilePath)`                                          |
|  [03]   | `data.WriteToExcel`                       | `void WriteToExcel(this BimData, FilePath)`                                         |
|  [04]   | `data.WriteToJson`                        | `void WriteToJson(this BimData, FilePath, bool withIndenting, bool withZip)`        |
|  [05]   | `fp.ReadBimDataFromParquetZip` / `…Async` | `BimData ReadBimDataFromParquetZip(this FilePath)` / `Task<BimData> …Async`         |
|  [06]   | `fp.ReadBimDataFromJson[Zip]` / `…Async`  | `BimData ReadBimDataFromJson(this FilePath\|Stream)` / `…Zip` / `…Async`            |

[ENTRYPOINT_SCOPE]: columnar IO over `Ara3D.DataTable` — `DuckDbUtils` / `ParquetUtils` / `ExcelUtils`
- rail: analytics-exchange
- `WriteToDuckDB` writes each `IDataTable` to a DuckDB table named `<Name>_<n>` via `DuckDBAppender` bulk append; `WriteToExcel` sanitizes + de-dups sheet names. The Parquet writers default `CompressionLevel = Optimal`, `CompressionMethod = Brotli`.

| [INDEX] | [SURFACE]                    | [SIGNATURE]                                                                                       |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `set.WriteToDuckDB`          | `void WriteToDuckDB(this IDataSet, FilePath)`                                                     |
|  [02]   | `conn.ReadTable`             | `IDataTable ReadTable(this DuckDBConnection, string)`                                             |
|  [03]   | `conn.ToDataSet<T>`          | `IDataSet ToDataSet<T>(this DuckDBConnection)`                                                    |
|  [04]   | `conn.ReadColumnValues<T>`   | `T[] ReadColumnValues<T>(this DuckDBConnection, string table, string column)`                     |
|  [05]   | `conn.GetTableNames`         | `IReadOnlyList<string> GetTableNames(this DuckDBConnection, bool includeViews = false)`           |
|  [06]   | `table.WriteParquetAsync`    | `Task WriteParquetAsync(this IDataTable, FilePath\|Stream, CompressionLevel, CompressionMethod)`  |
|  [07]   | `set.WriteParquetToZipAsync` | `Task WriteParquetToZipAsync(this IDataSet, string zipPath, CompressionMethod, CompressionLevel)` |
|  [08]   | `fp.ReadParquetAsync`        | `Task<IDataTable> ReadParquetAsync(this FilePath\|Stream, string? name)`                          |
|  [09]   | `fp.ReadParquetFromZipAsync` | `Task<IDataSet> ReadParquetFromZipAsync(this FilePath)`                                           |
|  [10]   | `set.WriteToExcel`           | `void WriteToExcel(this IDataSet\|IDataTable, FilePath)`                                          |

## [04]-[IMPLEMENTATION_LAW]

[COLUMNAR_MODEL]:
- `BimData` is a struct-of-arrays: eleven parallel `List<T>` columns, every cross-reference a typed `long`-backed index enum (`EntityIndex`/`DescriptorIndex`/`StringIndex`/`PointIndex`/`DocumentIndex`/`RelationIndex`), never an object pointer. Strings are interned once into `Strings` and referenced by `StringIndex` — the model is a normalized star schema (entity fact + descriptor dimension + string/point dimensions + EAV parameter columns split by value type).
- The EAV split is by value TYPE, not one polymorphic value column: `IntegerParameters`/`DoubleParameters`/`StringParameters`/`EntityParameters`/`PointParameters` each hold a `(EntityIndex, DescriptorIndex, <T>)` row, so a column is homogeneously typed and Parquet/DuckDB store it natively without a variant box. The `ParameterDescriptor.Type` (`ParameterType`) is the discriminant that says which column a descriptor's values live in.
- `ExpandedBIMData` is the denormalized read model: it resolves every `StringIndex` to its `string` and builds `RelationsFrom`/`RelationsTo` (the adjacency lists), `ParametersByEntity`, and `ParametersByName` so a downstream query reads `entity -> parameters` and `entity -> relations` without re-scanning the columns.

[DATASET_BRIDGE]:
- `ToDataSet()` is the single projection from the domain-specific `BimData` to the generic `Ara3D.DataTable` `IDataSet`. The eleven `IDataTable`s are emitted in a FIXED order that IS the DuckDB ordinal suffix — `Points`(0)/`Strings`(1)/`Descriptors`(2)/`Documents`(3)/`Entities`(4)/`Relations`(5)/`DoubleParameters`(6)/`IntegerParameters`(7)/`StringParameters`(8)/`EntityParameters`(9)/`PointParameters`(10). Every IO codec consumes the `IDataSet`, NOT the `BimData` directly — so JSON is the only codec that reads `BimData` structurally; Parquet/DuckDB/Excel are generic columnar codecs over the `IDataSet`.
- The DuckDB writer suffixes each table name with that projection ordinal (`Points_0`, `Strings_1`, `Descriptors_2`, …, `Entities_4`, `DoubleParameters_6`, …, `PointParameters_10`) and round-trips by `GetTableNames` -> `ReadTable`; a consumer querying the `.duckdb` directly must reference the suffixed names by this exact order.
- Consumed generic traversal surface (`Ara3D.DataTable`, decompile-verified on the restored assembly — the in-corpus DEBUG-IL absorption made these the load-bearing members `Query/columnar#WriteFrames` binds to synthesize the eleven `CREATE OR REPLACE TABLE` + `DuckDBAppender` writes):

| [INDEX] | [MEMBER]                               | [SHAPE]                      | [CONSUMER_BINDING]                                          |
| :-----: | :------------------------------------- | :--------------------------- | :---------------------------------------------------------- |
|  [01]   | `IDataSet.Tables`                      | `IReadOnlyList<IDataTable>`  | the fixed-ordinal table walk (`Select((held, index) => …)`) |
|  [02]   | `IDataTable.Name`                      | `string`                     | the `<Name>_<n>` trust-gated table identifier               |
|  [03]   | `IDataTable.Rows`                      | `IReadOnlyList<IDataRow>`    | `Rows.Count` bounds the appender row loop                   |
|  [04]   | `IDataTable.Columns`                   | `IReadOnlyList<IDataColumn>` | the CREATE-TABLE column fold + per-row cell walk            |
|  [05]   | `IDataTable.this[int column, int row]` | `object`                     | the cell read the typed `Cell` dispatch consumes            |
|  [06]   | `IDataColumn.ColumnIndex`              | `int`                        | the indexer's column ordinal                                |
|  [07]   | `IDataColumn.Descriptor`               | `IDataDescriptor`            | the column's typing authority                               |
|  [08]   | `IDataDescriptor.Name` / `Type`        | `string` / `System.Type`     | column name (trust-gated) + the `DuckType` CLR→DuckDB map   |

[STACK]:
- duckdb seam: `data.WriteDuckDB(fp)` writes a DuckDB database the folder's OWN `DuckDB.NET.Data.Full` (`api-duckdb.md`) opens — a Persistence analytics query SQL-joins `Entities`/`DoubleParameters`/`Relations` in-process (`SELECT … FROM Entities_4 e JOIN DoubleParameters_6 p ON p."Entity" = …`), reusing the one centrally pinned `DuckDB.NET.Data.Full` `1.5.3` and its osx-arm64 dylib. No second DuckDB runtime.
- parquet seam: `data.WriteToParquetZip(fp)` writes standard-format `.parquet` files (managed `Parquet.Net` `6.0.3`); the folder's native `ParquetSharp` (`api-parquetsharp.md`) and `Apache.Arrow`/`Apache.Arrow.Adbc` (`api-arrow.md`) read the SAME files into Arrow record batches for the columnar query rail — writer and reader meet at the Parquet file format, so the BIM frames flow into the Arrow/ADBC analytics pipeline without re-encoding.
- excel seam: `data.WriteToExcel(fp)` writes an `.xlsx` (`ClosedXML`); the folder's streaming `MiniExcel` (`api-miniexcel.md`) reads it back as `IDataReader` rows via `GetReader` — the spreadsheet egress the BIM model needs, distinct codec from the columnar lane.
- analytics-owner seam: the eleven columns are the tabular BIM frames the `Query/columnar` lane exposes; the typed-index star schema maps onto that lane's columnar/ADBC query surface, and `ExpandedBIMData` is the join view a report/QTO consumer reads directly.

[RAIL_LAW]:
- Packages: `Ara3D.BimOpenSchema` + `Ara3D.BimOpenSchema.IO` `1.0.1` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, DEBUG-built IL)
- Owns: the columnar SoA BIM schema (`BimData` + typed indices + EAV parameter columns + `EntityRelation` graph), the interning `BimDataBuilder`, the `ExpandedBIMData` join view, the `IDataSet` bridge, and the JSON/Parquet-zip/DuckDB/Excel codecs over `Ara3D.DataTable`
- Accept: a BIM model authored through `BimDataBuilder` and exported via `ToDataSet()` to the analytics codecs; a `.duckdb`/`.parquet` queried by the folder's own DuckDB/ParquetSharp/Arrow rails at the file boundary
- Reject: a hand-rolled BIM-to-tabular flattener where `BimData`/`ExpandedBIMData` + `ToDataSet()` model it; a second Parquet/DuckDB runtime where the folder pins own the engine; treating the DEBUG-built hot path as production-optimized without measuring; referencing the written DuckDB tables by bare name where the `<Name>_<n>` projection-ordinal suffix ([DATASET_BRIDGE]) is the real identity

## [05]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- This page carries `Ara3D.BimOpenSchema[.IO]` API facts only; the `Query/columnar` case algebra, the content-key projection, and the columnar query rail are owned by the design pages.
- Parquet lane separation: BimOpenSchema.IO uses MANAGED `Parquet.Net` (`6.0.3`) for its own read/write; the folder's `ParquetSharp` (native libparquet-cpp) is a DISTINCT engine that reads the same files. The two never substitute at the API — they meet only at the `.parquet` file format.
- Excel lane separation: BimOpenSchema.IO writes via `ClosedXML`; the folder's `MiniExcel` is the streaming READER. Distinct codecs, one file boundary.
- DuckDB shared: the writer and the folder's query rail use the ONE centrally pinned `DuckDB.NET.Data.Full`; the suffixed table-name convention (`<Name>_<n>`, projection order in [DATASET_BRIDGE]) is a serializer fact a direct SQL consumer honors.
