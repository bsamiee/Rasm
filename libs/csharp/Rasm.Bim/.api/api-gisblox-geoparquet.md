# [RASM_BIM_API_GISBLOX_GEOPARQUET]

`GISBlox.IO.GeoParquet` is the managed GeoParquet columnar codec backing the
`Semantics/geospatial#VECTOR_INGEST` web-scale columnar-geo leg: it reads and writes an
OGC-GeoParquet `.parquet` (a Parquet file whose geometry column is WKB/WKT-encoded and whose
file-level `geo` key-value metadata names the primary geometry column, its encoding, its CRS,
its bbox, and its covering) over the native `ParquetSharp` (`libparquet-cpp`) engine and the
`NetTopologySuite` geometry algebra. Its exchange unit is a `System.Data.DataTable` whose geo
columns carry WKB/WKT byte/text payloads and whose `geo` file metadata is the
`GeoFileMetadata`/`GeoColumnMetadata` model — NOT an NTS `IFeature` collection. It therefore
stacks on TWO admitted owners: `ParquetSharp` (`api-parquetsharp` in `Rasm.Persistence`) for
the native columnar file engine, and `NetTopologySuite` (`api-nettopologysuite`) for the
`WKBReader`/`WKBWriter` that bridges a geo-column cell to/from the canonical `GeoFeature`
`Geometry`. The `Semantics/geospatial#VECTOR_INGEST` fold composes it as a MANAGED columnar
arm (a `DataTable`↔`GeoFeature` bridge at the seam), distinct from the GDAL OGR universal
driver and from the row-oriented FGB/shapefile codecs — the no-new-native-runtime columnar
leg for a web-published parcel/building dataset.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GISBlox.IO.GeoParquet`
- package: `GISBlox.IO.GeoParquet`
- license: MIT (`license type="expression"`, `GISBlox/GISBlox.IO.GeoParquet`)
- assembly: `GISBlox.IO.GeoParquet` → the `net10.0` consumer binds `lib/net10.0/GISBlox.IO.GeoParquet.dll` (the sole `lib/` TFM == the consumer floor; pure-managed AnyCPU IL)
- namespace: `GISBlox.IO.GeoParquet` (`GeoParquetReader`/`GeoParquetWriter`), `GISBlox.IO.GeoParquet.Common` (the `geo` metadata model + `GeometryFormat`/`Edges`), `GISBlox.IO.GeoParquet.Extensions` (the `DataTable`/`DataColumn` geo-metadata extension surface), `GISBlox.IO.GeoParquet.Utils`
- dependency: `NetTopologySuite` (the central pin, exact match) + `ParquetSharp` (the native columnar engine, exact match) — the package itself is managed but its `ParquetSharp` transitive ships the `runtimes/osx-arm64/native/libparquet` native dylib, so the codec inherits ParquetSharp's RID-keyed native runtime
- asset: the geometry payload is WKB or WKT (selected by `GeometryFormat`) in a `DataTable` column; the package does NOT materialize NTS `IFeature` — the NTS bridge is the consumer's `WKBReader`/`WKBWriter`
- scope: OGC-GeoParquet `.parquet` read/write over a `DataTable`, the `geo` file-metadata model, batched column projection, and the geo-column schema extension surface
- rail: `Semantics/geospatial#VECTOR_INGEST` (the managed web-scale columnar-geo arm)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec roots
- namespace: `GISBlox.IO.GeoParquet`
- rail: geospatial
- note: both are static — `GeoParquetReader` projects a `.parquet` (path-based) onto a `DataTable` with the geometry decoded to the chosen `GeometryFormat`; `GeoParquetWriter` writes a `DataTable` (path- or stream-based) naming the geo column(s) and embedding the `geo` metadata.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [RAIL]                                                                                    |
| :-----: | :----------------- | :-------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `GeoParquetReader` | columnar reader | static `ReadAll`/`ReadColumn(s)` → `DataTable`, plus `ReadFileMetadata`/`ReadGeoMetadata` |
|  [02]   | `GeoParquetWriter` | columnar writer | static `Write(path/stream, DataTable, geoColumn(s), batchSize)`                           |

[PUBLIC_TYPE_SCOPE]: the `geo` file-metadata model
- namespace: `GISBlox.IO.GeoParquet.Common`
- rail: geospatial
- note: this is the OGC-GeoParquet `geo` key-value metadata the file header carries — the primary geometry column name, and per-column the encoding/CRS/bbox/covering/geometry-types. The reader surfaces it through `ReadGeoMetadata`; the writer derives it from the `DataTable` geo-column tags.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]      | [RAIL]                                                                                                                                       |
| :-----: | :-------------------- | :----------------- | :------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `GeoFileMetadata`     | file metadata      | `Version` (required), `Primary_column`, `Columns` (`IDictionary<string, GeoColumnMetadata>`) — the `geo` header                              |
|  [02]   | `GeoColumnMetadata`   | column metadata    | `(columnName, encoding)` ctor; `Encoding`/`Bbox`/`Covering`/`Edges`/`Epoch`/`GeometryTypes`/`Orientation`/`AdditionalProperties` (+ the CRS) |
|  [03]   | `BBox`                | bounding box       | the geo-column extent the metadata records (and the file-level dataset bbox)                                                                 |
|  [04]   | `Covering`            | covering geometry  | the bbox-covering hint the OGC spec carries for index acceleration                                                                           |
|  [05]   | `Edges`               | edge interpolation | `enum` — `planar` vs `spherical` edge interpretation of the geometry                                                                         |
|  [06]   | `GeometryFormat`      | geometry encoding  | `enum` — WKB vs WKT, the read/write geometry-column payload discriminant                                                                     |
|  [07]   | `ParquetFileMetadata` | parquet metadata   | the underlying Parquet schema/row-group metadata (`ReadFileMetadata`) the columnar projection reads                                          |
|  [08]   | `GeometryException`   | codec failure      | thrown on a malformed geo column / metadata; the boundary fault the rail traps onto `Fin<T>`                                                 |

[PUBLIC_TYPE_SCOPE]: the `DataTable`/`DataColumn` geo-schema extension surface
- namespace: `GISBlox.IO.GeoParquet.Extensions`
- rail: geospatial
- note: the geometry column carries its format and primary-flag as `DataColumn` metadata; these extensions tag a hand-built `DataTable` before a write and inspect the geo schema after a read — the bridge a `GeoFeature`↔`DataTable` projection composes.

| [INDEX] | [SURFACE]                                                                                                                             | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `DataTable.AddGeoColumn(string columnName, int ordinalPosition, GeometryFormat format)`                                               | schema build   | add a typed geometry column (WKB/WKT) at a position      |
|  [02]   | `DataColumn.SetGeoFormat(GeometryFormat)` / `GetGeoFormat()`                                                                          | column tag     | mark / read a column's geometry encoding                 |
|  [03]   | `DataColumn.SetAsPrimaryGeoColumn()` / `IsPrimaryGeoColumn()`                                                                         | primary tag    | mark / test the primary geometry column                  |
|  [04]   | `DataTable.GetPrimaryGeoColumn()` / `GetPrimaryGeoColumnName()`                                                                       | primary lookup | resolve the primary geo column / its name                |
|  [05]   | `DataTable.GetGeoColumnsByFormat(GeometryFormat)`                                                                                     | column query   | every geo column of a given encoding                     |
|  [06]   | `DataTable.AddGeoProcessingMetadata(List<string> geoColumns, string primaryGeoColumn)` / `AddGeoProcessingMetadata(GeoFileMetadata?)` | metadata bind  | attach the `geo` file metadata to the table before write |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read a GeoParquet to a DataTable
- namespace: `GISBlox.IO.GeoParquet`
- rail: geospatial
- note: `ReadAll` projects every column; `ReadColumn(s)` projects a subset by index or name — the columnar push-down that reads only the geometry + needed attribute columns of a wide web dataset. `format` selects the geometry-column decode (WKB → `byte[]` cells the `WKBReader` materializes); `batchSize` is the Arrow/Parquet row-group batch.

| [INDEX] | [SURFACE]                                                                                                                                                                                          | [CALL_SHAPE]            | [RAIL]                                                                      |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `GeoParquetReader.ReadAll(string filePath, GeometryFormat format, int batchSize = 65536)`                                                                                                          | → `DataTable`           | full-table columnar read                                                    |
|  [02]   | `GeoParquetReader.ReadColumn(string filePath, int columnIndex, GeometryFormat format, int batchSize = 65536)` / `ReadColumn(string filePath, string columnName, …)`                                | → `DataTable`           | single-column projection (by index or name)                                 |
|  [03]   | `GeoParquetReader.ReadColumns(string filePath, ICollection<int> columnIndexes, GeometryFormat format, int batchSize = 65536)` / `ReadColumns(string filePath, ICollection<string> columnNames, …)` | → `DataTable`           | multi-column projection push-down                                           |
|  [04]   | `GeoParquetReader.ReadGeoMetadata(string filePath)`                                                                                                                                                | → `GeoFileMetadata?`    | the `geo` header (primary column, encoding, CRS, bbox) without reading rows |
|  [05]   | `GeoParquetReader.ReadFileMetadata(string filePath)`                                                                                                                                               | → `ParquetFileMetadata` | the Parquet schema / row-group metadata                                     |

[ENTRYPOINT_SCOPE]: write a DataTable to GeoParquet
- namespace: `GISBlox.IO.GeoParquet`
- rail: geospatial
- note: the geo column(s) hold WKB/WKT (tagged via the extension surface); `Write` embeds the OGC `geo` file metadata naming the primary column and per-column encoding/CRS/bbox. The stream overload is the `/vsimem`-equivalent in-memory emit for the object-store transport.

| [INDEX] | [SURFACE]                                                                                                                                                                                                | [CALL_SHAPE] | [RAIL]                                                 |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :----------------------------------------------------- |
|  [01]   | `GeoParquetWriter.Write(string filePath, DataTable dataTable, string geoColumn, int batchSize = 65536)`                                                                                                  | → `void`     | single-geo-column file write                           |
|  [02]   | `GeoParquetWriter.Write(string filePath, DataTable dataTable, List<string> geoColumns, string primaryGeoColumn, int batchSize = 65536)`                                                                  | → `void`     | multi-geo-column write naming the primary column       |
|  [03]   | `GeoParquetWriter.Write(Stream stream, DataTable dataTable, string geoColumn, int batchSize = 65536)` / `Write(Stream stream, DataTable dataTable, List<string> geoColumns, string primaryGeoColumn, …)` | → `void`     | in-memory stream emit (the object-store transport leg) |

## [04]-[IMPLEMENTATION_LAW]

[CODEC_TOPOLOGY]:
- the exchange unit is a `System.Data.DataTable`: the geometry column holds WKB `byte[]` (or WKT `string`) cells per the `GeometryFormat`, the attribute columns hold the Parquet scalar values, and the `geo` file metadata (`GeoFileMetadata`) names the primary geometry column plus per-column encoding/CRS/bbox/covering/edges — this is the OGC-GeoParquet shape, NOT an NTS `IFeature` collection
- `GeoParquetReader` runs the native `ParquetSharp` columnar engine: `ReadColumn(s)` is the projection push-down (only the geometry + requested attribute columns are decoded from the row groups), and `ReadGeoMetadata` reads the `geo` header without materializing rows — the metadata-first posture for resolving the CRS/bbox before a windowed read
- `GeoParquetWriter` writes the `DataTable` to a Parquet file naming the geo column(s) and embedding the `geo` metadata; the geo columns are pre-tagged through the `Extensions` surface (`SetGeoFormat`/`SetAsPrimaryGeoColumn`/`AddGeoProcessingMetadata`)
- `GeometryFormat` is the WKB/WKT discriminant on both legs; WKB is the dense default the `WKBReader`/`WKBWriter` round-trips; `Edges` records planar-vs-spherical edge interpretation the consumer honors when reprojecting

[INTEGRATION_STACK]:
- with `ParquetSharp` (`api-parquetsharp`, `Rasm.Persistence`): GeoParquet IS a ParquetSharp file — this codec composes the native `libparquet-cpp` columnar engine the `csharp:Rasm.Persistence` Parquet lane already admits, inheriting its `runtimes/osx-arm64/native/libparquet` RID asset; the GeoParquet codec adds the OGC `geo` metadata + geometry-encoding leg ON TOP of the columnar engine, never a second Parquet reader
- with `NetTopologySuite` (`api-nettopologysuite`): the geo-column WKB cell bridges to the canonical `GeoFeature` `Geometry` through `WKBReader.Read(byte[])` (and `WKBWriter.Write(Geometry) → byte[]` on the write leg) seeded from `GeoServices.Factory` — the SAME WKB bridge the GDAL OGR universal arm crosses (`OSGeo.OGR.Geometry.ExportToWkb` → `WKBReader.Read`), so a `DataTable` geo column and an OGR feature land the identical NTS geometry; the `GeoFeature.Attributes` `IAttributesTable` maps to/from the `DataTable` non-geo columns
- `Semantics/geospatial#VECTOR_INGEST` seam: the fold composes GeoParquet as a MANAGED columnar `GeoVectorSource` arm — `GeoParquetReader.ReadAll`/`ReadColumns` over the `ObjectStore.Fetch` bytes (a temp-path or stream) yields a `DataTable`, the geo column WKB-decodes each row into a `GeoFeature`, and the non-geo columns fold onto the `AttributesTable`; the write leg projects `Seq<GeoFeature>` to a `DataTable` (geometry → WKB cell, attributes → columns) and `GeoParquetWriter.Write(stream, …)` emits the columnar payload — the columnar counterpart of the row-oriented FGB/shapefile arms, with NO new native runtime beyond ParquetSharp's
- column-projection seam: `ReadColumns(filePath, names, …)` is the attribute push-down a wide web dataset uses — read only the geometry + the priced/queried attribute columns, the columnar analog of the FGB bbox push-down (FGB filters by ROW/bbox, GeoParquet projects by COLUMN); a whole-table `ReadAll` followed by an in-memory column drop is the rejected form when the needed columns are known

[LOCAL_ADMISSION]:
- GeoParquet read/write enters through `GeoParquetReader`/`GeoParquetWriter` producing/consuming a `DataTable`; the `DataTable` geo column WKB-bridges to the canonical `GeoFeature` through the NTS `WKBReader`/`WKBWriter` at the seam, and the `GISBlox.*` types never leak past the `GeoVector` fold per the boundary-mapping law
- a column subset enters through `ReadColumns` so the columnar engine projects server-side; a full `ReadAll` + in-memory drop is the rejected form when the needed columns are known
- the `geo` metadata (primary column, CRS, bbox) reads through `ReadGeoMetadata` before a windowed read; re-deriving the dataset extent by scanning rows is the rejected form
- GeoParquet is the MANAGED columnar arm — admitting the GDAL OGR `"Parquet"` driver for a file this managed codec reads is the rejected form, and the geometry algebra stays `NetTopologySuite` (a boolean op inside this codec is the deleted form)

[RAIL_LAW]:
- Package: `GISBlox.IO.GeoParquet` (, MIT, pure-managed `lib/net10.0` AnyCPU IL; deps `NetTopologySuite` + `ParquetSharp`, the ParquetSharp transitive carrying the osx-arm64 native `libparquet`)
- Owns: the managed OGC-GeoParquet columnar codec — `DataTable`↔`.parquet` read/write with WKB/WKT geometry columns, the `geo` file-metadata model, batched column projection, and the geo-column schema tagging
- Accept: a `Semantics/geospatial#VECTOR_INGEST` managed columnar arm reading/writing a `DataTable` whose geo column WKB-bridges to the canonical `GeoFeature` via the NTS `WKBReader`/`WKBWriter`, with `ReadColumns` column push-down and `ReadGeoMetadata` metadata-first reads, composing the `ParquetSharp` native engine
- Reject: routing GeoParquet through the GDAL OGR `"Parquet"` driver where this managed codec reads it; a second Parquet engine beside the admitted `ParquetSharp`; treating the `DataTable` geo column as the canonical geometry instead of bridging it to the NTS `GeoFeature` via WKB; a whole-table `ReadAll` where a known column subset projects via `ReadColumns`; a boolean op inside the codec where `NetTopologySuite` owns the planar algebra
