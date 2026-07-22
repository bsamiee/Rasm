# [RASM_BIM_API_NTS_ESRI_SHAPEFILE]

`NetTopologySuite.IO.Esri.Shapefile` owns the pure-managed Esri shapefile codec: it reads and writes the `.shp`/`.shx`/`.dbf`/`.prj` quartet directly to and from `NetTopologySuite` `Feature`/`Geometry`, streaming forward-only under an MBR push-down filter over a typed dBASE attribute schema. It holds the managed shapefile leg of the federation's geospatial exchange seam, materializing the canonical NTS feature shape with no native `libgdal` dependency; the geometry algebra, the datum transform, and the raster and non-shapefile vector formats stay with their own owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.Esri.Shapefile`
- package: `NetTopologySuite.IO.Esri.Shapefile` (BSD-3-Clause)
- assembly: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri` (facade `Shapefile`, `ShapeType`, `ShapefileException`)
- namespace: `NetTopologySuite.IO.Esri.Shapefiles.Readers` / `.Writers` (per-type readers/writers + options)
- namespace: `NetTopologySuite.IO.Esri.Dbf` / `.Dbf.Fields` (standalone dBASE codec + the `DbfField` family)
- asset: netstandard2.0 single TFM; the net10 consumer binds `lib/netstandard2.0` — IL-only AnyCPU, no P/Invoke or native binaries
- depends: `NetTopologySuite` (`Geometry`/`GeometryFactory` algebra) + `NetTopologySuite.Features` (`Feature`/`IFeature`/`AttributesTable` shape it materializes)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: facade and shape vocabulary

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Shapefile`          | class         | abstract facade over readers, writers, and type sniff    |
|  [02]   | `ShapeType`          | enum          | one shape type per file; heterogeneous rejected at write |
|  [03]   | `ShapefileException` | class         | typed codec failure -> `BimFault.CodecReject`            |

[SHAPE_TYPE]: `NullShape` `Point` `PolyLine` `Polygon` `MultiPoint` `PointZM` `PolyLineZM` `PolygonZM` `MultiPointZM` `PointM` `PolyLineM` `PolygonM` `MultiPointM` `MultiPatch`

`ShapeType` names the ESRI `Z` forms `*ZM` (measured 3D) and the measure-only forms `*M`; the facade dispatches concrete point/polyline/polygon/multipoint readers and writers.

[PUBLIC_TYPE_SCOPE]: forward-only readers

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `ShapefileReader`                                    | class         | abstract `IEnumerable<Feature>` cursor    |
|  [02]   | `Shapefile{Point,PolyLine,Polygon,MultiPoint}Reader` | class         | per-type readers `OpenRead` dispatches to |
|  [03]   | `ShapefileReaderOptions`                             | class         | reader options carrier                    |

`ShapefileReader` exposes the current record through `Geometry`, `Fields`, `Projection`, `BoundingBox`, `Encoding`, `RecordCount`, walking it with `Read`/`Restart`; `ShapefileReaderOptions` carries `Factory`, `Encoding`, `GeometryBuilderMode`, `MbrFilter`, `MbrFilterOption`.

[PUBLIC_TYPE_SCOPE]: forward-only writers

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `ShapefileWriter`                                    | class         | abstract, flush-on-dispose cursor          |
|  [02]   | `Shapefile{Point,PolyLine,Polygon,MultiPoint}Writer` | class         | per-type writers `OpenWrite` dispatches to |
|  [03]   | `ShapefileWriterOptions`                             | class         | writer options + schema builders           |

`ShapefileWriter.Write` accepts one `IFeature`, an `IEnumerable<IFeature>`, or a set `Geometry`+`Fields` through the parameterless overload; `ShapefileWriterOptions` carries `ShapeType`, `Fields`, `Encoding`, `Projection` and the fluent `AddField`/`AddCharacterField`/`AddNumericInt32Field`/`AddDateField`/`AddLogicalField` builders.

[PUBLIC_TYPE_SCOPE]: dBASE attribute table and field family

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `Dbf`                                           | class         | static dBASE facade (`DefaultEncoding`)                   |
|  [02]   | `DbfReader` / `DbfWriter`                       | class         | standalone `.dbf` codec — attributes, no geometry         |
|  [03]   | `DbfFieldCollection`                            | class         | ordered `IReadOnlyList<DbfField>`, name/ordinal indexable |
|  [04]   | `DbfField`                                      | class         | abstract column base                                      |
|  [05]   | `DbfCharacterField`                             | class         | text column, 254-char cap                                 |
|  [06]   | `DbfNumericInt32Field` / `DbfNumericInt64Field` | class         | fixed-width integer columns over `int`/`long`             |
|  [07]   | `DbfNumericDoubleField`                         | class         | width-plus-scale floating column                          |
|  [08]   | `DbfNumericDecimalField`                        | class         | fixed-scale decimal, no binary-float rounding             |
|  [09]   | `DbfFloatField`                                 | class         | dBASE `F` float column                                    |
|  [10]   | `DbfDateField`                                  | class         | dBASE date (`YYYYMMDD`); NodaTime `LocalDate` carrier     |
|  [11]   | `DbfLogicalField`                               | class         | boolean column (`T`/`F`)                                  |

`DbfField` carries `Name`, `FieldType` (`DbfType`), `Length`, `NumericScale`, `Value`, `IsNull`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read shapefiles

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `Shapefile.OpenRead(string, ShapefileReaderOptions?) -> ShapefileReader`     | static  | streaming reader (large files)      |
|  [02]   | `Shapefile.OpenRead(Stream shp, Stream dbf, ShapefileReaderOptions?)`        | static  | stream overload (in-memory ingest)  |
|  [03]   | `Shapefile.ReadAllFeatures(string, ShapefileReaderOptions?) -> Feature[]`    | static  | eager materialization (small files) |
|  [04]   | `Shapefile.ReadAllFeatures(Stream shp, Stream dbf, ShapefileReaderOptions?)` | static  | eager stream overload               |
|  [05]   | `Shapefile.ReadAllGeometries(string, ShapefileReaderOptions?) -> Geometry[]` | static  | geometry-only (skips `.dbf`)        |
|  [06]   | `Shapefile.GetShapeType(string)` / `(Geometry) -> ShapeType`                 | static  | sniff header / map NTS geometry     |

[ENTRYPOINT_SCOPE]: write shapefiles

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Shapefile.OpenWrite(string, ShapefileWriterOptions) -> ShapefileWriter`    | static   | streaming writer                       |
|  [02]   | `Shapefile.OpenWrite(Stream shp/shx/dbf/prj, ShapefileWriterOptions)`       | static   | four-stream writer                     |
|  [03]   | `Shapefile.WriteAllFeatures(IEnumerable<IFeature>, string)`                 | static   | eager write; schema from first feature |
|  [04]   | `Shapefile.WriteAllFeatures(IEnumerable<IFeature>, Stream shp/shx/dbf/prj)` | static   | all-streams eager write                |
|  [05]   | `ShapefileWriterOptions.AddField<T>(string) -> DbfField`                    | instance | typed schema builder                   |

`WriteAllFeatures` takes optional trailing `projection` (WKT) and `Encoding`; `AddField<T>` constrains `T : struct, IComparable, IConvertible, IFormattable`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Shapefile` writes the four components in lockstep — `.shp` geometry records, `.shx` record-offset index, `.dbf` attribute table, `.prj` WKT projection sidecar; the stream overloads expose all four so a non-filesystem store routes each component.
- `Shapefile.OpenRead` returns a forward-only `IEnumerable<Feature>` reading one record at a time and never materializing the full set; `ReadAllFeatures` is the eager convenience for small inputs.
- `ShapefileReaderOptions.MbrFilter` is an `Envelope` push-down: the reader uses the `.shp` header MBR and per-record bounding boxes to skip records outside the window before decoding geometry, and `MbrFilterOption` selects overlap versus containment.
- `ShapefileReader` yields `NetTopologySuite.Features.Feature` directly with no intermediate shapefile record type, so the codec output is the canonical NTS feature; `ShapefileReaderOptions.Factory` seeds the `GeometryFactory` from `NtsGeometryServices.Instance` so geometry carries the canonical `PrecisionModel`/`SRID` and composes with the planar algebra without a precision rebuild.
- `ShapefileWriterOptions` declares the writer schema up front — `params DbfField[]` ctor, the fluent `Add*Field` builders, or the copy-from-`ShapefileReader` ctor; dBASE caps field names at 10 chars, character fields at 254, one shape type per file.

[STACKING]:
- `api-nettopologysuite` NTS seam: the codec consumes and produces `Geometry` and `Feature`, so a decoded feature flows into the planar predicate/overlay/index algebra (`STRtree.Insert(feature.Geometry.EnvelopeInternal, feature)`) and a `BimElement` footprint flows out through `Shapefile.WriteAllFeatures`.
- `api-projnet` / `api-maxrev-gdal` reproject seam: `.prj` WKT reads into `ShapefileReader.Projection` and writes from `ShapefileWriterOptions.Projection`; `ProjNET` `CoordinateSystemWktReader` or OSR `ImportFromESRI`/`ImportFromWkt` parses it, and the `MathTransform`/`CoordinateTransformation` reprojects the ordinates to the project CRS before features enter the model.
- `Exchange/format#INTERCHANGE`: shapefile read/write is one `Detect` row — `.shp` ingest routes to `Shapefile.OpenRead` under the MBR filter, export to `Shapefile.OpenWrite` under the schema-from-`PropertySet` projection.

[LOCAL_ADMISSION]:
- shapefile read enters through `Shapefile.OpenRead` (streaming, `MbrFilter` for large files) or `ReadAllFeatures` (eager, small files), the `Factory` seeded from `NtsGeometryServices.Instance`.
- shapefile write enters through `Shapefile.OpenWrite` with a `ShapefileWriterOptions` carrying the fixed `ShapeType`, the `DbfField` schema, and the `.prj` projection.
- `DbfReader`/`DbfWriter` is the standalone `.dbf` codec for an attribute-only export with no geometry.
- this managed codec is the admitted default for shapefile I/O; the `MaxRev.Gdal.Core` OGR `ESRI Shapefile` driver is reserved for the formats only GDAL covers.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.Esri.Shapefile`
- Owns: the Esri shapefile (`.shp`/`.shx`/`.dbf`/`.prj`) read/write codec over NTS `Feature`/`Geometry`, the streaming MBR-filtered reader, and the typed dBASE attribute schema
- Accept: shapefile ingest/export, attribute-table (`.dbf`) read/write, spatial-windowed shapefile streaming
- Reject: the geometry algebra (`NetTopologySuite` owns it), datum/projection transformation (`ProjNET`/OSR own it), raster and non-shapefile vector formats (`MaxRev.Gdal.Core` owns them), GeoPackage/GeoJSON (their own NTS IO codecs own them), a hand-rolled `.shp`/`.shx`/`.dbf` binary parse
