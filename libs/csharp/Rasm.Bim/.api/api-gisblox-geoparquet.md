# [RASM_BIM_API_GISBLOX_GEOPARQUET]

`GISBlox.IO.GeoParquet` mints the managed OGC-GeoParquet columnar codec: a `.parquet` whose geometry column carries WKB/WKT payloads and whose file-level `geo` metadata names the primary column, its encoding, and its bbox, read and written over a `System.Data.DataTable` carrying `GeoFileMetadata`/`GeoColumnMetadata` — never an NTS `IFeature` collection, so the geometry-algebra bridge stays the consumer's. It feeds the `Semantics/geospatial#VECTOR_INGEST` fold as the managed columnar arm, the no-new-native-runtime counterpart of the row-oriented FGB/shapefile codecs and the GDAL OGR driver.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GISBlox.IO.GeoParquet`
- package: `GISBlox.IO.GeoParquet` (MIT, GISBlox)
- assembly: `GISBlox.IO.GeoParquet` — pure-managed AnyCPU IL, sole `lib/net10.0` TFM
- namespace: `GISBlox.IO.GeoParquet` (`GeoParquetReader`/`GeoParquetWriter`), `.Common` (the `geo` metadata model, `GeometryFormat`/`Edges`), `.Extensions` (the `DataTable`/`DataColumn` geo-schema surface)
- depends: `NetTopologySuite`, `ParquetSharp`
- abi: the `ParquetSharp` transitive carries the `runtimes/osx-arm64/native/libparquet` dylib, so the codec inherits ParquetSharp's RID-keyed native runtime
- rail: `Semantics/geospatial#VECTOR_INGEST` managed columnar-geo arm

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec roots (`GISBlox.IO.GeoParquet`) — both static, path- or stream-based

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                                                                                    |
| :-----: | :----------------- | :-------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `GeoParquetReader` | columnar reader | projects a `.parquet` onto a `DataTable` decoded to a `GeometryFormat`, plus the metadata reads |
|  [02]   | `GeoParquetWriter` | columnar writer | writes a `DataTable`, names the geo column(s), embeds the `geo` metadata                        |

[PUBLIC_TYPE_SCOPE]: the `geo` file-metadata model (`.Common`) — the OGC-GeoParquet header the reader surfaces and the writer derives from the geo-column tags

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [CAPABILITY]                                                |
| :-----: | :-------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `GeoFileMetadata`     | file metadata     | the `geo` header — `Version`, `Primary_column`, `Columns`   |
|  [02]   | `GeoColumnMetadata`   | column metadata   | `(columnName, encoding)` ctor; per-column OGC descriptor    |
|  [03]   | `BBox`                | bounding box      | geo-column extent and the file-level dataset bbox           |
|  [04]   | `Covering`            | covering geometry | the bbox-covering hint for index acceleration               |
|  [05]   | `Edges`               | enum              | `Planar` vs `Spherical` edge interpretation                 |
|  [06]   | `GeometryFormat`      | enum              | `WKB` vs `WKT` payload discriminant on both legs            |
|  [07]   | `ParquetFileMetadata` | parquet metadata  | underlying Parquet schema / row-group metadata              |
|  [08]   | `GeometryException`   | codec failure     | malformed geo column / metadata fault trapped onto `Fin<T>` |

[GeoColumnMetadata]: `Encoding` `Bbox` `Covering` `Edges` `Epoch` `GeometryTypes` `Orientation` `AdditionalProperties`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read a GeoParquet to a `DataTable` — every static `GeoParquetReader` projection carries a `(…, GeometryFormat format, int batchSize = 65536)` tail; `ReadColumn(s)` pushes the column projection down, decoding only the geometry and the requested attribute columns of a wide dataset

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------ | :-------------------------------------- |
|  [01]   | `ReadAll(string) -> DataTable`                          | full-table columnar read                |
|  [02]   | `ReadColumn(string, int) -> DataTable`                  | single-column projection by index       |
|  [03]   | `ReadColumn(string, string) -> DataTable`               | single-column projection by name        |
|  [04]   | `ReadColumns(string, ICollection<int>) -> DataTable`    | multi-column projection by index        |
|  [05]   | `ReadColumns(string, ICollection<string>) -> DataTable` | multi-column projection by name         |
|  [06]   | `ReadGeoMetadata(string) -> GeoFileMetadata?`           | the `geo` header without reading rows   |
|  [07]   | `ReadFileMetadata(string) -> ParquetFileMetadata`       | the Parquet schema / row-group metadata |

[ENTRYPOINT_SCOPE]: write a `DataTable` to GeoParquet — every static `GeoParquetWriter.Write` embeds the `geo` metadata naming the primary column; a `Stream` target is the in-memory emit for the object-store transport, geo columns pre-tagged through the `.Extensions` surface

| [INDEX] | [SURFACE]                                        | [CAPABILITY]                               |
| :-----: | :----------------------------------------------- | :----------------------------------------- |
|  [01]   | `Write(string, DataTable, string)`               | single-geo-column file write               |
|  [02]   | `Write(string, DataTable, List<string>, string)` | multi-geo-column file write, primary named |
|  [03]   | `Write(Stream, DataTable, string)`               | in-memory single-column emit               |
|  [04]   | `Write(Stream, DataTable, List<string>, string)` | in-memory multi-column emit                |

[ENTRYPOINT_SCOPE]: tag a `DataTable` geo schema (`.Extensions`) — instance extension methods that tag a hand-built table before a write and inspect the geo schema after a read

| [INDEX] | [SURFACE]                                                             | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `DataTable.AddGeoColumn(string, int, GeometryFormat) -> DataColumn`   | add a typed geometry column at an ordinal       |
|  [02]   | `DataColumn.SetGeoFormat(GeometryFormat)`                             | mark a column's geometry encoding               |
|  [03]   | `DataColumn.GetGeoFormat() -> GeometryFormat`                         | read a column's geometry encoding               |
|  [04]   | `DataTable.PeekGeoColumnFormat(DataColumn) -> GeometryFormat`         | read a column's encoding via the table          |
|  [05]   | `DataColumn.SetAsPrimaryGeoColumn()`                                  | mark the primary geometry column                |
|  [06]   | `DataColumn.IsPrimaryGeoColumn() -> bool`                             | test the primary geometry column                |
|  [07]   | `DataTable.GetPrimaryGeoColumn() -> DataColumn?`                      | resolve the primary geo column                  |
|  [08]   | `DataTable.GetPrimaryGeoColumnName() -> string`                       | resolve the primary geo column name             |
|  [09]   | `DataTable.GetGeoColumnsByFormat(GeometryFormat) -> List<DataColumn>` | every geo column of an encoding                 |
|  [10]   | `DataTable.AddGeoProcessingMetadata(List<string>, string)`            | bind `geo` metadata from columns + primary name |
|  [11]   | `DataTable.AddGeoProcessingMetadata(GeoFileMetadata?)`                | attach a prebuilt `geo` metadata object         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- exchange unit is a `System.Data.DataTable`: the geometry column holds WKB `byte[]` or WKT `string` per `GeometryFormat`, attribute columns hold Parquet scalars, and `GeoFileMetadata` names the primary geometry column with per-column encoding/bbox/covering/edges — the OGC-GeoParquet shape, never an NTS `IFeature` collection
- `ReadColumn(s)` pushes the column projection into the native engine so only the geometry and the requested attribute columns decode; `ReadGeoMetadata` reads the `geo` header without materializing rows, resolving bbox and extent before a windowed read
- `GeometryFormat` is the WKB/WKT discriminant on read and write, WKB the dense default the `WKBReader`/`WKBWriter` round-trip; `Edges` records planar-vs-spherical interpretation the consumer honors when reprojecting

[STACKING]:
- `ParquetSharp`(`.api/api-parquetsharp`, `Rasm.Persistence`): a GeoParquet file IS a ParquetSharp file — this codec composes the native `libparquet` columnar engine that lane already admits, inheriting its `runtimes/osx-arm64/native/libparquet` asset, and adds only the OGC `geo` metadata + geometry-encoding leg on top
- `NetTopologySuite`(`.api/api-nettopologysuite`): the geo-column WKB cell bridges to the canonical `GeoFeature` `Geometry` through `WKBReader.Read(byte[])` and `WKBWriter.Write(Geometry) -> byte[]` seeded from `GeoServices.Factory` — the same WKB bridge the GDAL OGR arm crosses, so a `DataTable` geo column and an OGR feature land the identical NTS geometry, and `GeoFeature.Attributes` maps to/from the non-geo columns
- `Semantics/geospatial#VECTOR_INGEST`: this fold composes GeoParquet as the managed columnar `GeoVectorSource` arm — `ReadAll`/`ReadColumns` over the `ObjectStore.Fetch` bytes yields a `DataTable`, each geo column WKB-decodes into a `GeoFeature`, and the write leg projects `Seq<GeoFeature>` back to a `DataTable`; `ReadColumns(filePath, names)` is the attribute push-down for a wide web dataset, the columnar analog of the FGB bbox row filter

[LOCAL_ADMISSION]:
- GeoParquet read/write enters through `GeoParquetReader`/`GeoParquetWriter` over a `DataTable`; the geo column WKB-bridges to the canonical `GeoFeature` at the seam and the `GISBlox.*` types stay inside the `GeoVector` fold per the boundary-mapping law
- a known column subset enters through `ReadColumns` for server-side projection, dataset extent reads through `ReadGeoMetadata` before a windowed read, and the geometry algebra stays `NetTopologySuite`

[RAIL_LAW]:
- Package: `GISBlox.IO.GeoParquet`
- Owns: the managed OGC-GeoParquet columnar codec — `DataTable`↔`.parquet` read/write with WKB/WKT geometry columns, the `geo` file-metadata model, batched column projection, and the geo-column schema tagging
- Accept: a `Semantics/geospatial#VECTOR_INGEST` managed columnar arm reading/writing a `DataTable` whose geo column WKB-bridges to the canonical `GeoFeature` via the NTS `WKBReader`/`WKBWriter`, with `ReadColumns` column push-down and `ReadGeoMetadata` metadata-first reads, composing the `ParquetSharp` native engine
- Reject: routing GeoParquet through the GDAL OGR `"Parquet"` driver where this managed codec reads it; a second Parquet engine beside `ParquetSharp`; treating the `DataTable` geo column as canonical geometry instead of bridging it to the NTS `GeoFeature`; a whole-table `ReadAll` where a known column subset projects via `ReadColumns`; a boolean op inside the codec where `NetTopologySuite` owns the planar algebra
