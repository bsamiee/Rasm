# [RASM_BIM_API_NTS_GEOPACKAGE]

`NetTopologySuite.IO.GeoPackage` is the pure-managed codec for the OGC GeoPackage *StandardGeoPackageBinary* geometry encoding — the per-feature BLOB stored in a `.gpkg` geometry column. Its `GeoPackageGeoReader`/`GeoPackageGeoWriter` pair round-trips that BLOB to a `NetTopologySuite` `Geometry` with `Ordinates` (XY/XYZ/XYM/XYZM) and SRID handling; the `.gpkg` SQLite container stays out of scope. It is the GeoPackage leg of the `Semantics/georeference#GEOSPATIAL_SEAM`: the container layer yields the geometry-column bytes, this codec materializes the planar algebra `NetTopologySuite` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoPackage`
- package: `NetTopologySuite.IO.GeoPackage` (BSD-3-Clause)
- assembly: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO` (`GeoPackageGeoReader`, `GeoPackageGeoWriter`)
- asset: netstandard2.0 single TFM, IL-only AnyCPU managed, no P/Invoke, no native binaries; the net10.0 consumer binds the `lib/netstandard2.0` asset
- dependency: `NetTopologySuite` (the `Geometry`/`Ordinates`/`CoordinateSequenceFactory`/`PrecisionModel` algebra); no `NetTopologySuite.Features` dependency — this codec is geometry-only, attributes ride the SQLite row columns
- rail: geospatial

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: BLOB geometry reader/writer

`GeoPackageBinaryHeader` and its `GeoPackageBinaryType` enum are `internal`; the pair owns the `GP`-magic wire shape end-to-end and no header type crosses the public surface.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :-------------------- | :------------ | :------------------------------------------ |
|  [01]   | `GeoPackageGeoReader` | class         | decode a geometry BLOB to an NTS `Geometry` |
|  [02]   | `GeoPackageGeoWriter` | class         | encode an NTS `Geometry` to a geometry BLOB |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decode a GeoPackage geometry BLOB

Each reader seeds `DefaultCoordinateSequenceFactory`/`DefaultPrecisionModel` from `NtsGeometryServices.Instance` unless a factory/precision pair is passed, and defaults `HandleOrdinates` to XYZM; `Read(byte[])` wraps a `MemoryStream` over the stream overload. Set `RepairRings`, `HandleSRID`, and `HandleOrdinates` on the reader before `Read`.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `new GeoPackageGeoReader()`                                                     | ctor     | all-default reader                    |
|  [02]   | `new GeoPackageGeoReader(CoordinateSequenceFactory, PrecisionModel)`            | ctor     | seed the packed factory and precision |
|  [03]   | `new GeoPackageGeoReader(CoordinateSequenceFactory, PrecisionModel, Ordinates)` | ctor     | add the materialized `Ordinates`      |
|  [04]   | `GeoPackageGeoReader.Read(byte[]) -> Geometry`                                  | instance | decode a BLOB from a SQLite row       |
|  [05]   | `GeoPackageGeoReader.Read(Stream) -> Geometry`                                  | instance | decode from a stream                  |

[ENTRYPOINT_SCOPE]: encode a GeoPackage geometry BLOB

`GeoPackageGeoWriter`, constructed parameterless, defaults `HandleOrdinates` to the static XYZM ceiling; set it to drop Z/M on write.

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `GeoPackageGeoWriter.Write(Geometry) -> byte[]`       | instance | encode a geometry to a BLOB for a SQLite insert |
|  [02]   | `GeoPackageGeoWriter.Write(Geometry, Stream) -> void` | instance | encode directly into a stream                   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Envelope: the codec owns the OGC StandardGeoPackageBinary envelope — a `GP` magic, a version byte, a flags byte (envelope-contents, byte-order, and empty bits), a 4-byte `srs_id`, the optional per-ordinate envelope, then the OGC WKB body. `GeoPackageGeoWriter` writes the header little-endian with the envelope from `Geometry.EnvelopeInternal` and delegates the body to the NTS `WKBWriter`; `GeoPackageGeoReader` parses the header, honors the empty bit, and reads the WKB body with the seeded factory.
- Dimensionality: `HandleOrdinates` is the shared read/write knob expressed as NTS `Ordinates` flags (XY=3, XYZ=7, XYM=65539, XYZM=65543); the setter keeps the XY bits and intersects the request with `AllowedOrdinates`, whose reader ceiling is the seeded `CoordinateSequenceFactory` capacity and whose writer ceiling is the static XYZM value.
- Identity: `HandleSRID` stamps the header `srs_id` onto `Geometry.SRID` on decode and reads `Geometry.SRID` back into the header on encode; SRID is an identity tag, so datum or projection change is the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg. `RepairRings` repairs ring order on decode, and a heavier pre-overlay repair runs `GeometryFixer.Fix` from the NTS core.
- Factory seed: seed the reader from `NtsGeometryServices.Instance` — `DefaultCoordinateSequenceFactory` is the packed factory, `DefaultPrecisionModel` the canonical precision — so decoded geometry carries the packed ordinate layout and maps onto a kernel buffer without per-point boxing; a per-call `new GeometryFactory()` is the rejected form.

[STACKING]:
- `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): the codec consumes and produces `Geometry` directly and reuses the NTS `WKBWriter`/`WKBReader` for the body, so a decoded geometry flows straight into the planar predicate/overlay/`STRtree` algebra and a `BimElement` footprint flows out through `Write`.
- `MaxRev.Gdal.Core`(`api-maxrev-gdal`): the GeoPackage rail stacks this codec onto a SQLite reader — `Microsoft.Data.Sqlite` selects the geometry-column `byte[]` and attribute columns, `Read(blob)` materializes the geometry, and the attribute columns pair into an NTS `Feature`; the OGR `GPKG` driver is the all-in-one container path when the full GDAL matrix is already loaded.
- `format#INTERCHANGE`: GeoPackage read/write is one row beside the shapefile (`api-nts-esri-shapefile`), GeoJSON (`api-nts-geojson4stj`), GeoParquet (`api-gisblox-geoparquet`), and FlatGeobuf (`api-flatgeobuf`) codecs, all exchanging one NTS `Geometry` and differing only in container and envelope.
- within-lib: the Persistence owner round-trips a geometry BLOB into a SQLite/GeoPackage store while the Bim owner ingests site and context GeoPackage data, two rails over one `Geometry` shape.

[LOCAL_ADMISSION]:
- BLOB decode enters through a `GeoPackageGeoReader` seeded from `NtsGeometryServices.Instance` with `HandleSRID`/`HandleOrdinates` set for the dataset; `Read(byte[])` is the per-row call over a SQLite cursor.
- BLOB encode enters through `GeoPackageGeoWriter.Write(geometry)` with `HandleOrdinates` set to the stored dimensionality; hand-rolling the `GP` header or treating the codec as a `.gpkg` container reader is the rejected form.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.GeoPackage`
- Owns: the OGC GeoPackage StandardGeoPackageBinary geometry-BLOB read/write codec over NTS `Geometry`, with `Ordinates` dimensionality and `srs_id`/SRID handling
- Accept: geometry-column BLOB decode and encode, dimensionality (XY/XYZ/XYM/XYZM) control, SRID stamping, on-read ring repair
- Reject: the `.gpkg` SQLite container (`Microsoft.Data.Sqlite` and the OGR `GPKG` driver in `MaxRev.Gdal.Core` own it), the geometry algebra (`NetTopologySuite` owns it), datum/projection transformation (`ProjNET`/OSR own it), attribute storage (the SQLite row columns carry it)
