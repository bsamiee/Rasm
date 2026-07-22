# [RASM_PERSISTENCE_API_ARA3D_BIMOPENSCHEMA]

`Ara3D.BimOpenSchema` owns the columnar struct-of-arrays BIM analytics schema: a string-interned, typed-index entity/parameter/relation graph (`BimData`) authored through `BimDataBuilder` and projected by `ToDataSet` to the generic `Ara3D.DataTable` `IDataSet`. `Ara3D.BimOpenSchema.IO` owns the codec leg — the `IDataSet` written to and read from a Parquet-zip, a DuckDB file, an Excel workbook, and gzipped JSON. Managed writer and native reader meet at the file format, never the assembly: the folder's own DuckDB, ParquetSharp/Arrow, and MiniExcel rails read what this surface writes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Ara3D.BimOpenSchema`
- package: `Ara3D.BimOpenSchema` (MIT, Ara3D)
- assembly: `Ara3D.BimOpenSchema`
- namespace: `Ara3D.BimOpenSchema`
- asset: pure-managed AnyCPU, no native RID asset; the `net10.0` consumer binds `lib/net8.0`
- depends: `Ara3D.SDK` — the `Ara3D.DataTable` `IDataSet`/`IDataTable`/`DataTableExtensions` columnar abstraction the model projects to
- rail: analytics-exchange (BIM)

[PACKAGE_SURFACE]: `Ara3D.BimOpenSchema.IO`
- package: `Ara3D.BimOpenSchema.IO` (MIT, Ara3D)
- assembly: `Ara3D.BimOpenSchema.IO`
- namespace: `Ara3D.BimOpenSchema.IO`
- asset: pure-managed AnyCPU; the native floor rides `DuckDB.NET.Data.Full` (osx-arm64 `duckdb` dylib), `Parquet.Net`/`ClosedXML` pure-managed
- depends: `Ara3D.BimOpenSchema`, `Ara3D.SDK`, `ClosedXML` (Excel codec), `DuckDB.NET.Data.Full` (DuckDB codec + dylib), `Parquet.Net` (managed Parquet codec)
- rail: analytics-exchange (BIM)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Ara3D.BimOpenSchema` columnar model

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CAPABILITY]                                                                          |
| :-----: | :----------------- | :---------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `BimData`          | columnar root     | SoA container of eleven parallel `List<T>` columns ([COLUMNS] below)                  |
|  [02]   | `BimDataBuilder`   | interning builder | interns entities/documents/points/descriptors/strings → typed indices                 |
|  [03]   | `BIMDataExtension` | static accessors  | polymorphic `Get`, index enumerators, `ToInt`, `ToDataSet`/`ToBimData` bridge         |
|  [04]   | `ExpandedBIMData`  | join view         | resolves strings; builds `RelationsFrom`/`RelationsTo`/`ParametersByEntity`/`…ByName` |

[COLUMNS]: the eleven `BimData` `List<T>` columns — `Entities`, `Documents`, `Strings`, `Points`, `Descriptors`, `Relations`, `IntegerParameters`, `DoubleParameters`, `StringParameters`, `EntityParameters`, `PointParameters`.

[RECORDS]: dimension + fact records; every `Parameter*` value row is `(EntityIndex Entity, DescriptorIndex Descriptor, <T> Value)`, where `<T>` is `int`/`double`/`StringIndex`/`EntityIndex`/`PointIndex` for `ParameterInt`/`Double`/`String`/`Entity`/`Point`.

| [INDEX] | [SYMBOL]              | [SHAPE]                                                                                           |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `Entity`              | `(long LocalId, string GlobalId, DocumentIndex Document, StringIndex Name, StringIndex Category)` |
|  [02]   | `Document`            | `(StringIndex Title, StringIndex Path)`                                                           |
|  [03]   | `Point`               | `(double X, double Y, double Z)`                                                                  |
|  [04]   | `ParameterDescriptor` | `(StringIndex Name, StringIndex Units, StringIndex Group, ParameterType Type)`                    |
|  [05]   | `EntityRelation`      | `(EntityIndex EntityA, EntityIndex EntityB, RelationType RelationType)` — the typed graph edge    |

[ENUMS_AND_INDICES]:
- `ParameterType`: `Int`/`Double`/`Entity`/`String`/`Point` — the descriptor's value-column discriminant.
- `RelationType`: `PartOf`/`ElementOf`/`ContainedIn`/`InstanceOf`/`HostedBy`/`ChildOf`/`HasLayer`/`HasMaterial`/`ConnectsTo` (`HasConnector` aliases `ConnectsTo`=`8`).
- row-index enums (`: long` FKs, uncomparable across columns): `EntityIndex`/`DocumentIndex`/`DescriptorIndex`/`StringIndex`/`PointIndex`/`RelationIndex`.
- `ExpandedEntity`/`ExpandedParameter`/`ExpandedEntityRelation`: flat string-valued export classes; `ExpandedEntity` adds derived `Level`/`Type`/`Material`/`Room`.

[PUBLIC_TYPE_SCOPE]: `Ara3D.BimOpenSchema.IO` codecs

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `BimDataSerializer`                 | static (top)  | `BimData` read/write over JSON / Parquet-zip / DuckDB / Excel    |
|  [02]   | `DuckDbUtils`                       | static        | `IDataSet`↔DuckDB codec                                          |
|  [03]   | `ParquetUtils`                      | static        | `IDataTable`/`IDataSet`↔Parquet over `Parquet.Net`               |
|  [04]   | `ParquetUtils.ParquetColumnAdpater` | adapter       | `: IDataColumn` wrapping a Parquet `DataColumn` on read [sic]    |
|  [05]   | `ExcelUtils`                        | static        | `IDataSet`/`IDataTable`/ADO.NET `DataSet`→Excel over `ClosedXML` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: author a `BimData` — `BimDataBuilder` (string interning, typed-index return)
- every `Add*` interns its value into the matching column via a backing `Dictionary` (de-dup) and returns the typed index. `AddParameter` carries 6 value overloads (`double`/`int`/`string`/`EntityIndex`/`PointIndex`/`Point`) × 2 descriptor forms — a pre-interned `DescriptorIndex`, or a `(name, units, group)` triple interned inline; `PointIndex` binds a pre-interned point, `Point` interns inline.

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
- `FilePath` is `Ara3D.Utils.FilePath` (implicit `string` conversion). Parquet/DuckDB/Excel route through `BimData.ToDataSet()`; default Parquet compression is `(CompressionMethod)4` = Brotli. `WriteToParquetZip` emits one `.parquet` per table in a Brotli zip; `WriteDuckDB` a `.duckdb` with the eleven appended tables; `WriteToExcel` an `.xlsx`, one worksheet per table; `WriteToJson` System.Text.Json, optionally gzipped.

| [INDEX] | [SURFACE]                                 | [SIGNATURE]                                                                         |
| :-----: | :---------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `data.WriteToParquetZip` / `…Async`       | `void WriteToParquetZip(this BimData, FilePath)` / `Task WriteToParquetZipAsync(…)` |
|  [02]   | `data.WriteDuckDB`                        | `void WriteDuckDB(this BimData, FilePath)`                                          |
|  [03]   | `data.WriteToExcel`                       | `void WriteToExcel(this BimData, FilePath)`                                         |
|  [04]   | `data.WriteToJson`                        | `void WriteToJson(this BimData, FilePath, bool withIndenting, bool withZip)`        |
|  [05]   | `fp.ReadBimDataFromParquetZip` / `…Async` | `BimData ReadBimDataFromParquetZip(this FilePath)` / `Task<BimData> …Async`         |
|  [06]   | `fp.ReadBimDataFromJson[Zip]` / `…Async`  | `BimData ReadBimDataFromJson(this FilePath\|Stream)` / `…Zip` / `…Async`            |

[ENTRYPOINT_SCOPE]: columnar IO over `Ara3D.DataTable` — `DuckDbUtils` / `ParquetUtils` / `ExcelUtils`
- `WriteToDuckDB` writes each `IDataTable` to a DuckDB table named `<Name>_<n>` via `DuckDBAppender` bulk append; `WriteToExcel` sanitizes + de-dups sheet names; the Parquet writers default `CompressionLevel = Optimal`, `CompressionMethod = Brotli`.

| [INDEX] | [SURFACE]                    | [SIGNATURE]                                                                                       |
| :-----: | :--------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `set.WriteToDuckDB`          | `void WriteToDuckDB(this IDataSet, FilePath)`                                                     |
|  [02]   | `conn.WriteTable`            | `void WriteTable(this DuckDBConnection, IDataTable, string)`                                      |
|  [03]   | `conn.ReadTable`             | `IDataTable ReadTable(this DuckDBConnection, string)`                                             |
|  [04]   | `conn.ToDataSet<T>`          | `IDataSet ToDataSet<T>(this DuckDBConnection)`                                                    |
|  [05]   | `conn.ReadColumnValues<T>`   | `T[] ReadColumnValues<T>(this DuckDBConnection, string table, string column)`                     |
|  [06]   | `conn.GetTableNames`         | `IReadOnlyList<string> GetTableNames(this DuckDBConnection, bool includeViews = false)`           |
|  [07]   | `table.WriteParquetAsync`    | `Task WriteParquetAsync(this IDataTable, FilePath\|Stream, CompressionLevel, CompressionMethod)`  |
|  [08]   | `set.WriteParquetToZipAsync` | `Task WriteParquetToZipAsync(this IDataSet, string zipPath, CompressionMethod, CompressionLevel)` |
|  [09]   | `fp.ReadParquetAsync`        | `Task<IDataTable> ReadParquetAsync(this FilePath\|Stream, string? name)`                          |
|  [10]   | `fp.ReadParquetFromZipAsync` | `Task<IDataSet> ReadParquetFromZipAsync(this FilePath)`                                           |
|  [11]   | `set.WriteToExcel`           | `void WriteToExcel(this IDataSet\|IDataTable, FilePath)`                                          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `BimData` is a struct-of-arrays star schema: eleven parallel `List<T>` columns, every cross-reference a typed `long`-backed index enum (never an object pointer); strings intern once into `Strings` and resolve by `StringIndex`.
- Parameters are EAV split by value TYPE — `IntegerParameters`/`DoubleParameters`/`StringParameters`/`EntityParameters`/`PointParameters`, each a homogeneous `(EntityIndex, DescriptorIndex, <T>)` row Parquet/DuckDB store natively without a variant box; `ParameterDescriptor.Type` is the discriminant selecting the column.
- `ToDataSet` is the sole projection to the generic `IDataSet`; every codec but JSON consumes the `IDataSet`, not `BimData` structurally. Tables emit in a fixed projection order that IS the DuckDB name suffix (`Points_0`…`Descriptors_2`…`Entities_4`…`DoubleParameters_6`…`PointParameters_10`), so a direct SQL consumer references the `<Name>_<n>` names by that exact order.
- `ExpandedBIMData` is the denormalized read model: resolves every `StringIndex`, builds `RelationsFrom`/`RelationsTo` adjacency and `ParametersByEntity`/`ParametersByName`, so a query reads `entity -> parameters`/`entity -> relations` without re-scanning columns.

[STACKING]:
- `api-duckdb.md` (`DuckDB.NET.Data.Full`): `data.WriteDuckDB(fp)` writes a `.duckdb` the folder's own DuckDB opens and SQL-joins in-process (`Entities_4 e JOIN DoubleParameters_6 p ON p."Entity" = …`), reusing the one pinned runtime and its osx-arm64 dylib.
- `api-parquetsharp.md` (`ParquetSharp`) + `api-arrow.md` (`Apache.Arrow`/`Apache.Arrow.Adbc`): `data.WriteToParquetZip(fp)` writes standard `.parquet` (managed `Parquet.Net`); the folder's native ParquetSharp and Arrow read the SAME files into record batches, so BIM frames flow into the Arrow/ADBC rail without re-encoding.
- `api-miniexcel.md` (`MiniExcel`): `data.WriteToExcel(fp)` writes an `.xlsx` (`ClosedXML`); the folder's streaming MiniExcel reads it back as `IDataReader` rows via `GetReader`.
- within-lib: the DuckDB-table synthesis binds the consumed generic `Ara3D.DataTable` traversal surface (`Ara3D.SDK`) at operator depth — walk `Tables` by fixed ordinal, fold `Columns` into the `CREATE OR REPLACE TABLE`, bound the appender loop by `Rows.Count`, and dispatch each cell by its column `Descriptor.Type` CLR→DuckDB map below:

| [INDEX] | [MEMBER]                               | [SHAPE]                      | [SEAM]                                    |
| :-----: | :------------------------------------- | :--------------------------- | :---------------------------------------- |
|  [01]   | `IDataSet.Tables`                      | `IReadOnlyList<IDataTable>`  | the fixed-ordinal table walk              |
|  [02]   | `IDataTable.Name`                      | `string`                     | the `<Name>_<n>` trust-gated identifier   |
|  [03]   | `IDataTable.Rows`                      | `IReadOnlyList<IDataRow>`    | `Rows.Count` bounds the appender loop     |
|  [04]   | `IDataTable.Columns`                   | `IReadOnlyList<IDataColumn>` | the CREATE-TABLE fold + per-row cell walk |
|  [05]   | `IDataTable.this[int column, int row]` | `object`                     | the typed-`Cell`-dispatch cell read       |
|  [06]   | `IDataColumn.ColumnIndex`              | `int`                        | the indexer's column ordinal              |
|  [07]   | `IDataColumn.Descriptor`               | `IDataDescriptor`            | the column's typing authority             |
|  [08]   | `IDataDescriptor.Name` / `Type`        | `string` / `System.Type`     | column name + `DuckType` CLR→DuckDB map   |

[LOCAL_ADMISSION]:
- This page carries `Ara3D.BimOpenSchema[.IO]` API facts only; the `Query/columnar` case algebra, content-key projection, and columnar query rail are the design pages'.
- Codecs meet only at the file boundary, never substitute at the API: `Parquet.Net` (managed write) and `ParquetSharp` (native read) are distinct engines over one `.parquet`; `ClosedXML` writes and `MiniExcel` reads one `.xlsx`; the DuckDB writer and the folder query rail share the one pinned `DuckDB.NET.Data.Full`, and the `<Name>_<n>` suffix is a serializer fact a direct SQL consumer honors.

[RAIL_LAW]:
- Package: `Ara3D.BimOpenSchema` + `Ara3D.BimOpenSchema.IO` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`)
- Owns: the columnar SoA BIM schema (`BimData` + typed indices + EAV parameter columns + `EntityRelation` graph), the interning `BimDataBuilder`, the `ExpandedBIMData` join view, the `IDataSet` bridge, and the JSON/Parquet-zip/DuckDB/Excel codecs over `Ara3D.DataTable`
- Accept: a BIM model authored through `BimDataBuilder` and exported via `ToDataSet()` to the analytics codecs; a `.duckdb`/`.parquet` queried by the folder's own DuckDB/ParquetSharp/Arrow rails at the file boundary
- Reject: a hand-rolled BIM-to-tabular flattener where `BimData`/`ExpandedBIMData` + `ToDataSet()` model it; a second Parquet/DuckDB runtime where the folder pins own the engine; referencing the written DuckDB tables by bare name where the `<Name>_<n>` projection-ordinal suffix is the real identity
