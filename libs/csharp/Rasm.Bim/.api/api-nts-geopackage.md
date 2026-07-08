# [RASM_BIM_API_NTS_GEOPACKAGE]

`NetTopologySuite.IO.GeoPackage` is the pure-managed codec for the OGC GeoPackage
*StandardGeoPackageBinary* geometry encoding — the per-feature BLOB stored in a `.gpkg`
feature table's geometry column. It is NOT a SQLite container reader: it owns only the
`GeoPackageGeoReader`/`GeoPackageGeoWriter` pair that converts that BLOB (the `GP` magic +
header + WKB body) to and from a `NetTopologySuite` `Geometry`, with `Ordinates` (XY/XYZ/
XYM/XYZM) and SRID handling. It is the GeoPackage leg of the
`Semantics/georeference#GEOSPATIAL_SEAM`: the SQLite container layer (`Microsoft.Data.Sqlite`,
or the GDAL OGR `GPKG` driver in `MaxRev.Gdal.Core`) yields the geometry-column bytes, this
codec materializes the planar algebra `NetTopologySuite` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoPackage`
- package: `NetTopologySuite.IO.GeoPackage`
- license: BSD-3-Clause (`requireLicenseAcceptance=false`)
- assembly: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO` (`GeoPackageGeoReader`, `GeoPackageGeoWriter`)
- asset: netstandard2.0 ONLY (single TFM); the net10.0 consumer binds the `lib/netstandard2.0` asset
- asset: IL-only AnyCPU managed assembly; no P/Invoke, no native binaries — the BLOB body is emitted through the NTS `WKBWriter`
- dependency: `NetTopologySuite` `[,)` only (the `Geometry`/`Ordinates`/`CoordinateSequenceFactory`/`PrecisionModel` algebra — the core is pinned); NO `NetTopologySuite.Features` dependency (this codec is geometry-only, attributes live in the SQLite row columns)
- consumer: dual-owner central pin — `libs/csharp/Rasm.Bim` (site/context GeoPackage ingest) + `libs/csharp/Rasm.Persistence` (geometry-BLOB column round-trip in a SQLite/GeoPackage store)
- rail: geospatial

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: BLOB geometry reader/writer
- package: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO`
- rail: geospatial

The `GeoPackageBinaryHeader` (the `GP`-magic header struct) and its `GeoPackageBinaryType`
enum are `internal` — they are NOT a public surface; the header format is described in
`[04]` as the wire shape the reader/writer own end-to-end.

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:--------------------- |:--------- |:------------------------------------------------------------------------------------------------------- |
| [01] | `GeoPackageGeoReader` | geospatial | decodes a StandardGeoPackageBinary BLOB to an NTS `Geometry`. `RepairRings` (bool — fix invalid ring order on read), `HandleSRID` (bool — stamp the header `srs_id` onto `Geometry.SRID`), `AllowedOrdinates` (the `Ordinates` the seeded `CoordinateSequenceFactory` can carry), `HandleOrdinates` (the `Ordinates` mask actually materialized; setter clamps to `AllowedOrdinates`, always keeps XY) |
| [02] | `GeoPackageGeoWriter` | geospatial | encodes an NTS `Geometry` to a StandardGeoPackageBinary BLOB. `static readonly AllowedOrdinates` (`(Ordinates)65543` = XYZM), `HandleOrdinates` (the `Ordinates` written; setter clamps to `AllowedOrdinates`, always keeps XY). Emits the `GP` header (little-endian, envelope from `Geometry.EnvelopeInternal`) then the WKB body via the NTS `WKBWriter` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decode a GeoPackage geometry BLOB
- package: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO`
- rail: geospatial

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------- |:--------------------------------------------------------------------------------------- |:----------------------------------------------------------- |
| [01] | `new GeoPackageGeoReader` | `()` | default reader: seeds `DefaultCoordinateSequenceFactory`/`DefaultPrecisionModel` from `NtsGeometryServices.Instance`, `HandleOrdinates` = XYZM (`(Ordinates)65543`) |
| [02] | `new GeoPackageGeoReader` | `(CoordinateSequenceFactory, PrecisionModel)` / `(CoordinateSequenceFactory, PrecisionModel, Ordinates handleOrdinates)` | reader seeded with the canonical `PackedCoordinateSequenceFactory` + precision so decoded geometry composes with the rest of the planar algebra without a precision rebuild |
| [03] | `GeoPackageGeoReader.Read` | `(byte[] blob)` → `Geometry` | decode the geometry-column BLOB read from a SQLite row |
| [04] | `GeoPackageGeoReader.Read` | `(Stream)` → `Geometry` | decode from a stream (the `byte[]` overload wraps a `MemoryStream`) |

[ENTRYPOINT_SCOPE]: encode a GeoPackage geometry BLOB
- package: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO`
- rail: geospatial

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------- |:-------------------------------------------- |:----------------------------------------------------------- |
| [01] | `GeoPackageGeoWriter.Write` | `(Geometry geom)` → `byte[]` | encode a geometry to a GeoPackage BLOB ready for a SQLite geometry-column insert |
| [02] | `GeoPackageGeoWriter.Write` | `(Geometry geom, Stream stream)` → `void` | encode directly into a stream (the `byte[]` form wraps a `MemoryStream`) |

## [04]-[IMPLEMENTATION_LAW]

[BLOB_FORMAT]:
- the codec owns the OGC GeoPackage *StandardGeoPackageBinary* envelope: a 2-byte `GP` magic, a version byte, a flags byte (envelope-contents bits, byte-order bit, empty bit), the 4-byte `srs_id`, the optional envelope (min/max per ordinate driven by the flags), then the OGC WKB geometry body. `GeoPackageGeoWriter` writes the header little-endian and delegates the body to the NTS `WKBWriter`; `GeoPackageGeoReader` parses the header, honors the empty bit, and reads the WKB body with the seeded factory.
- this is the per-feature BLOB ONLY. The `.gpkg` SQLite container — `gpkg_contents`, `gpkg_geometry_columns`, `gpkg_spatial_ref_sys`, the feature/tile tables, the RTree spatial index — is OUT of scope; the container is read through `Microsoft.Data.Sqlite` (the SQLite owner) or the GDAL OGR `GPKG` driver (`MaxRev.Gdal.Core`, `api-maxrev-gdal`). The rejected form is expecting this package to open a `.gpkg` file.

[ORDINATES_AND_SRID]:
- `HandleOrdinates` is the dimensionality knob on both reader and writer, expressed as the NTS `Ordinates` flags (XY=3, XYZ=7, XYM=65539, XYZM=65543). The setter always keeps the XY bits and intersects the request with `AllowedOrdinates` — the reader's `AllowedOrdinates` is what the seeded `CoordinateSequenceFactory` can store, the writer's is the static XYZM ceiling. Set it to drop Z/M on write or to force planar-only decode.
- `GeoPackageGeoReader.HandleSRID = true` stamps the header `srs_id` onto the decoded `Geometry.SRID`; the writer reads `Geometry.SRID` into the header `srs_id`. SRID is an identity tag, not a transform — reprojection is the separate `Semantics/georeference#GEODETIC_TRANSFORM` leg (`ProjNET`, `api-projnet`).
- `RepairRings = true` repairs invalid ring order during decode; for a heavier repair before an overlay, run `GeometryFixer.Fix` from the NTS core (`api-nettopologysuite`).

[FACTORY_SEED]:
- seed the reader from `NtsGeometryServices.Instance` (`DefaultCoordinateSequenceFactory` = the admitted `PackedCoordinateSequenceFactory`, `DefaultPrecisionModel`) so decoded geometry carries the canonical packed ordinate layout and precision and maps onto a kernel buffer without per-point boxing — the same global-singleton discipline the shapefile and GeoJSON codecs follow. The rejected form is a per-call `new GeometryFactory()`.

[STACK_INTEGRATION]:
- NTS seam: the codec consumes and produces `NetTopologySuite.Geometries.Geometry` directly (`api-nettopologysuite`) and reuses the NTS `WKBWriter` for the body — a decoded GeoPackage geometry flows straight into the planar predicate/overlay/`STRtree` algebra, and a `BimElement` footprint flows out through `GeoPackageGeoWriter.Write`.
- SQLite-container seam: the canonical GeoPackage rail STACKS this codec onto a SQLite reader — `Microsoft.Data.Sqlite` (Persistence) selects the geometry-column `byte[]` and the attribute columns from a feature table, `GeoPackageGeoReader.Read(blob)` materializes the geometry, and the attribute columns become the `AttributesTable` that pairs with it into an NTS `Feature`. The OGR `GPKG` driver (`MaxRev.Gdal.Core`) is the alternative all-in-one container path when the full driver matrix is already in play.
- format-table seam: GeoPackage read/write is one row in the `format#INTERCHANGE` table beside the shapefile (`api-nts-esri-shapefile`), GeoJSON (`api-nts-geojson4stj`), GeoParquet (`api-gisblox-geoparquet`), and FlatGeobuf (`api-flatgeobuf`) codecs — all exchanging the same NTS `Geometry`, differing only in container/envelope.
- dual-consumer seam: the Persistence owner uses this codec to round-trip a geometry BLOB into a SQLite/GeoPackage store; the Bim owner uses it to ingest site/context GeoPackage data. One central pin, two rails, one canonical `Geometry` shape.

[LOCAL_ADMISSION]:
- BLOB decode enters through a `GeoPackageGeoReader` seeded from `NtsGeometryServices.Instance` with `HandleSRID`/`HandleOrdinates` set for the dataset; `Read(byte[])` is the per-row call over a SQLite cursor.
- BLOB encode enters through `GeoPackageGeoWriter.Write(geometry)` with `HandleOrdinates` set to the stored dimensionality; the rejected form is hand-rolling the `GP` header or treating the codec as a `.gpkg` container reader.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.GeoPackage`
- Owns: the OGC GeoPackage StandardGeoPackageBinary geometry-BLOB read/write codec over NTS `Geometry`, with `Ordinates` dimensionality and `srs_id`/SRID handling
- Accept: geometry-column BLOB decode/encode, dimensionality (XY/XYZ/XYM/XYZM) control, SRID stamping, on-read ring repair
- Reject: the `.gpkg` SQLite container itself (`Microsoft.Data.Sqlite` / OGR `GPKG` in `MaxRev.Gdal.Core` own it), the geometry algebra (`NetTopologySuite` owns it), datum/projection transformation (`ProjNET`/OSR own it), attribute storage (the SQLite row columns carry it — this codec is geometry-only)
