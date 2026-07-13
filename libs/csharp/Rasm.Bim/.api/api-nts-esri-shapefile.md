# [RASM_BIM_API_NTS_ESRI_SHAPEFILE]

`NetTopologySuite.IO.Esri.Shapefile` is a pure-managed Esri shapefile codec reading and
writing the `.shp`/`.shx`/`.dbf`/`.prj` quartet directly to/from `NetTopologySuite`
`Feature`/`Geometry`. It owns the geometry-record reader/writer per shape type, the
streaming forward-only `IEnumerable<Feature>` reader with an MBR (envelope) push-down
filter, the dBASE III/IV attribute table with the full typed `DbfField` family, and the
`.prj` WKT projection sidecar. It is the shapefile leg of the
`Semantics/georeference#GEOSPATIAL_SEAM`, materializing the same `Feature`/`AttributesTable`
shape `NetTopologySuite` defines and `MaxRev.Gdal.Core` OGR also produces.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.Esri.Shapefile`
- package: `NetTopologySuite.IO.Esri.Shapefile`
- license: BSD-3-Clause
- assembly: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri` (`Shapefile` static facade, `ShapeType`, `ShapefileException`)
- namespace: `NetTopologySuite.IO.Esri.Shapefiles.Readers` (`ShapefileReader` + `ShapefileReaderOptions` + per-type readers)
- namespace: `NetTopologySuite.IO.Esri.Shapefiles.Writers` (`ShapefileWriter` + `ShapefileWriterOptions` + per-type writers)
- namespace: `NetTopologySuite.IO.Esri.Dbf` (`Dbf`, `DbfReader`, `DbfWriter`, `DbfEncoding`)
- namespace: `NetTopologySuite.IO.Esri.Dbf.Fields` (`DbfField` base + concrete field types + `DbfFieldCollection`)
- asset: netstandard2.0 ONLY (single-target); the net10.0 consumer binds the `lib/netstandard2.0` asset
- asset: IL-only AnyCPU managed assembly; no P/Invoke, no native binaries
- dependency: `NetTopologySuite` (the `Geometry`/`GeometryFactory` algebra) and `NetTopologySuite.Features` (the `Feature`/`IFeature`/`AttributesTable` shape it materializes)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: facade and shape kind
- package: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri`
- rail: geometry

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                                                                                |
| :-----: | :------------------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `Shapefile`          | abstract static facade; one-call `OpenRead`/`OpenWrite`/`ReadAllFeatures`/`WriteAllFeatures`/`GetShapeType` |
|  [02]   | `ShapeType`          | enum (values in the fence below); one shape type per file, heterogeneous rejected at write                  |
|  [03]   | `ShapefileException` | typed codec failure (corrupt header, shape/dbf count mismatch, unsupported type) → `BimFault.CodecReject`   |

```csharp signature
enum ShapeType =                               // one shape type per file; heterogeneous geometry rejected at write
    NullShape=0 | Point=1 | PolyLine=3 | Polygon=5 | MultiPoint=8
    | PointZM=11 | PolyLineZM=13 | PolygonZM=15 | MultiPointZM | MultiPatch;   // ESRI "Z" forms named *ZM here
```

[PUBLIC_TYPE_SCOPE]: readers
- package: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri.Shapefiles.Readers`
- rail: geometry

| [INDEX] | [SYMBOL]                                             | [CAPABILITY]                                                     |
| :-----: | :--------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `ShapefileReader`                                    | abstract forward-only reader (members in the fence below)        |
|  [02]   | `Shapefile{Point,PolyLine,Polygon,MultiPoint}Reader` | the per-type concrete readers `Shapefile.OpenRead` dispatches to |
|  [03]   | `ShapefileReaderOptions`                             | reader options (fields in the fence below)                       |

```csharp signature
abstract class ShapefileReader : IEnumerable<Feature> {   // forward-only; foreach yields Feature
    bool Read(out bool deleted, out Feature); bool Read();  // cursor
    Geometry; Attributes;                                  // current record
    string Projection;                                     // .prj WKT
    Envelope BoundingBox;                                  // .shp header MBR
    DbfFieldCollection Fields; void Restart();
}
class ShapefileReaderOptions {
    GeometryFactory Factory;                               // seed from NtsGeometryServices.Instance
    Encoding;                                              // dBASE codepage override
    GeometryBuilderMode;                                   // Strict | IgnoreInvalidShapes | FixInvalidShapes | QuickFixInvalidShapes
    Envelope MbrFilter; MbrFilterOption;                   // spatial pre-filter + inclusion mode
}
```

[PUBLIC_TYPE_SCOPE]: writers
- package: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri.Shapefiles.Writers`
- rail: geometry

| [INDEX] | [SYMBOL]                                             | [CAPABILITY]                                                      |
| :-----: | :--------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `ShapefileWriter`                                    | abstract forward-only writer (members in the fence below)         |
|  [02]   | `Shapefile{Point,PolyLine,Polygon,MultiPoint}Writer` | the per-type concrete writers `Shapefile.OpenWrite` dispatches to |
|  [03]   | `ShapefileWriterOptions`                             | writer options + fluent schema builders (in the fence below)      |

```csharp signature
abstract class ShapefileWriter : IDisposable {             // forward-only; flush-on-dispose; writes .shp/.shx/.dbf/.prj in lockstep
    void Write(IFeature); void Write(IEnumerable<IFeature>);
    Geometry; Fields; void Write();                        // or set Geometry + Fields, then parameterless Write()
}
class ShapefileWriterOptions {
    ShapeType;                                             // fixed output kind
    List<DbfField> Fields; Encoding; string Projection;    // .prj WKT
    ShapefileWriterOptions(ShapeType, params DbfField[]);
    ShapefileWriterOptions(ShapefileReader reader);        // copy source schema
    AddField(name, Type); AddField<T>(name);
    AddCharacterField; AddDateField; AddFloatField; AddLogicalField; AddNumericInt32Field; AddNumericInt64Field;
}
```

[PUBLIC_TYPE_SCOPE]: dBASE attribute table and field family
- package: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri.Dbf`, `NetTopologySuite.IO.Esri.Dbf.Fields`
- rail: geometry

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                                                                                 |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `DbfReader` / `DbfWriter` | standalone dBASE table codec (`.dbf` without geometry); `Dbf` is the static facade           |
|  [02]   | `DbfEncoding`             | dBASE codepage ↔ `Encoding` resolution (the `.dbf` language-driver byte)                     |
|  [03]   | `DbfField`                | abstract column: `Name`/`FieldType`(`DbfType`)/`Length`/`NumericScale`/`Value`/`StringValue` |
|  [04]   | `DbfFieldCollection`      | ordered field schema; indexable by name/ordinal; the `Fields` carrier                        |
|  [05]   | `DbfCharacterField`       | text column; `DbfCharacterField(name, length = 254)` — dBASE 254-char cap                    |
|  [06]   | `DbfNumericInt32Field`    | 32-bit integer column                                                                        |
|  [07]   | `DbfNumericInt64Field`    | 64-bit integer column (fixed-width numeric)                                                  |
|  [08]   | `DbfNumericDoubleField`   | floating column; `DbfNumericDoubleField(name, length, precision)` = width + scale            |
|  [09]   | `DbfFloatField`           | single-precision float column                                                                |
|  [10]   | `DbfNumericDecimalField`  | fixed-scale decimal (quantity/cost; no binary-float rounding)                                |
|  [11]   | `DbfDateField`            | dBASE date column (8-char `YYYYMMDD`); the carrier a NodaTime `LocalDate` maps onto          |
|  [12]   | `DbfLogicalField`         | boolean column (`T`/`F`)                                                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read shapefiles
- package: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri`
- rail: geometry

| [INDEX] | [SURFACE]                                                                                        | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `Shapefile.OpenRead(string shpPath, ShapefileReaderOptions? = null)` → `ShapefileReader`         | streaming reader (large files)       |
|  [02]   | `Shapefile.OpenRead(Stream shp, Stream dbf, ShapefileReaderOptions? = null)` → `ShapefileReader` | stream overload (in-memory ingest)   |
|  [03]   | `Shapefile.ReadAllFeatures(string shpPath, ShapefileReaderOptions? = null)` → `Feature[]`        | eager materialization (small files)  |
|  [04]   | `Shapefile.ReadAllGeometries(string shpPath, ShapefileReaderOptions? = null)` → `Geometry[]`     | geometry-only (skips `.dbf`)         |
|  [05]   | `Shapefile.GetShapeType(string shpPath)` / `(Geometry)` → `ShapeType`                            | sniff header type / map NTS geometry |

[ENTRYPOINT_SCOPE]: write shapefiles
- package: `NetTopologySuite.IO.Esri.Shapefile`
- namespace: `NetTopologySuite.IO.Esri`
- rail: geometry
- note: `AddField<T>` constrains `T: struct, IComparable, IConvertible, IFormattable`.

| [INDEX] | [SURFACE]                                                                                           | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------------------------------------------------------- | :--------------------------- |
|  [01]   | `Shapefile.OpenWrite(string shpPath, ShapefileWriterOptions)` → `ShapefileWriter`                   | streaming writer             |
|  [02]   | `Shapefile.OpenWrite(Stream shp, shx, dbf, prj, ShapefileWriterOptions)` → `ShapefileWriter`        | four-stream overload         |
|  [03]   | `Shapefile.WriteAllFeatures(IEnumerable<IFeature>, string shpPath, string? projection, Encoding?)`  | eager write; schema inferred |
|  [04]   | `Shapefile.WriteAllFeatures(IEnumerable<IFeature>, Stream shp, shx, dbf, prj?, string?, Encoding?)` | all-streams eager write      |
|  [05]   | `ShapefileWriterOptions.AddField<T>(string name)` → `DbfField`                                      | typed schema builder         |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- single-target netstandard2.0 managed assembly; no P/Invoke, no native binaries. This is the pure-managed AnyCPU shapefile leg — distinct from the `MaxRev.Gdal.Core` OGR `ESRI Shapefile` driver which reads the same format through native `libgdal`. The managed codec is the admitted default for shapefile I/O (no native dependency); the OGR driver is reserved for the formats only GDAL covers.
- the codec writes the four shapefile components in lockstep: `.shp` (geometry records), `.shx` (the record-offset index), `.dbf` (the dBASE attribute table), and `.prj` (the WKT projection sidecar). The stream overloads expose all four so a non-filesystem store can route each component.

[STREAMING_AND_FILTER]:
- `Shapefile.OpenRead` returns a forward-only `IEnumerable<Feature>` — foreach iteration reads one record at a time and never materializes the full set, the form for large vector files. `ReadAllFeatures` is the eager convenience for small inputs only.
- `ShapefileReaderOptions.MbrFilter` is an `Envelope` spatial push-down: the reader uses the `.shp` header MBR and per-record bounding boxes to skip records outside the filter envelope before decoding their geometry — the cheap window the `georeference` site/context ingest applies to clip a large basemap to the project extent. `MbrFilterOption` selects the inclusion semantics (overlap vs containment).

[FEATURE_SHAPE]:
- the reader yields `NetTopologySuite.Features.Feature` directly: `feature.Geometry` is the NTS `Geometry`, `feature.Attributes` is the `IAttributesTable` keyed by `DbfField.Name`. There is NO intermediate shapefile-specific record type that the design must translate — the codec output IS the canonical NTS feature shape (the `api-nettopologysuite` `Feature`/`AttributesTable` types).
- `ShapefileReaderOptions.Factory` seeds the `GeometryFactory` parsed geometry is built with: set it from `NtsGeometryServices.Instance` so shapefile geometry carries the canonical `PrecisionModel`/`SRID` and composes directly with the rest of the planar algebra without a precision rebuild.

[DBF_SCHEMA]:
- the writer schema is declared up front through `ShapefileWriterOptions`: either pass `params DbfField[]` to the ctor or build fluently with `AddCharacterField`/`AddNumericInt32Field`/`AddNumericDoubleField`/`AddDateField`/`AddLogicalField`. `AddField<T>(name)` infers the field type from `T` (a `BimElement` property value type maps to its `DbfField` subclass).
- the dBASE format constrains the schema: field names are ≤10 chars, character fields ≤254 chars, and one shape type per file. A `BimElement` `PropertySet` projected to a shapefile is lowered to this constrained schema — the rejected form is assuming arbitrary-width string columns.
- `DbfReader`/`DbfWriter` is the standalone table codec when only the `.dbf` is needed (an attribute-only export with no geometry), and `DbfEncoding` resolves the codepage byte the `Encoding` option overrides.

[STACK_INTEGRATION]:
- NTS seam: the codec consumes and produces `NetTopologySuite` `Geometry` and `NetTopologySuite.Features` `Feature` — a shapefile feature flows straight into the planar predicate/overlay/index algebra (`STRtree.Insert(feature.Geometry.EnvelopeInternal, feature)`), and a `BimElement` footprint flows out through `Shapefile.WriteAllFeatures`.
- reprojection seam: the `.prj` WKT string is read into `ShapefileReader.Projection` and written from `ShapefileWriterOptions.Projection`. `ProjNET` parses that WKT (`CoordinateSystemWktReader`) or `MaxRev.Gdal.Core` OSR (`ImportFromESRI`/`ImportFromWkt`) does, and the `MathTransform`/OSR `CoordinateTransformation` reprojects the geometry ordinates to the project CRS before the features enter the model.
- format-table seam: shapefile read/write is one `Detect` row in the `format#INTERCHANGE` table; the `format` page routes `.shp` ingest to `Shapefile.OpenRead` with the MBR filter, and shapefile export to `Shapefile.OpenWrite` with the schema-from-`PropertySet` projection.

[LOCAL_ADMISSION]:
- shapefile read enters through `Shapefile.OpenRead` (streaming, with `MbrFilter` for large files) or `ReadAllFeatures` (eager, small files); the reader's `Factory` option is seeded from `NtsGeometryServices.Instance`.
- shapefile write enters through `Shapefile.OpenWrite` with a `ShapefileWriterOptions` carrying the fixed `ShapeType`, the `DbfField` schema, and the `.prj` projection.
- the rejected form is hand-rolling the `.shp`/`.shx`/`.dbf` binary records or treating the codec output as anything other than the canonical NTS `Feature`.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.Esri.Shapefile`
- Owns: the Esri shapefile (`.shp`/`.shx`/`.dbf`/`.prj`) read/write codec over NTS `Feature`/`Geometry`, the streaming MBR-filtered reader, and the typed dBASE attribute-table schema
- Accept: shapefile ingest/export, attribute-table (`.dbf`) read/write, spatial-windowed shapefile streaming
- Reject: the geometry algebra itself (`NetTopologySuite` owns it), datum/projection transformation (`ProjNET`/OSR own it), raster and non-shapefile vector formats (`MaxRev.Gdal.Core` owns them), GeoPackage/GeoJSON (their own NTS IO codec packages own them)
