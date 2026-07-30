# [RASM_PERSISTENCE_API_NTS_GEOPACKAGE]

`NetTopologySuite.IO.GeoPackage` owns the GeoPackage geometry blob — a GPB header over a WKB body — for SQLite stores on the spatial-values boundary. It decodes to the one NTS `Geometry` currency and binds the same precision-and-SRID configuration the core WKB and WKT codecs carry, so a value crossing the blob, the PostGIS column, and GeoJSON keeps one grid. It serves the `Ingest/geospatial#FEATURE_ROWS` `GeoContainer`/`GeoWire` seam; `Rasm.Bim/.api/api-nts-geopackage.md` is the peer partition serving the `Semantics/geospatial#GEOSPATIAL_SEAM` codec leg, one catalogue per package on both tiers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite.IO.GeoPackage`
- package: `NetTopologySuite.IO.GeoPackage` (BSD-3-Clause)
- assembly: `NetTopologySuite.IO.GeoPackage`
- namespace: `NetTopologySuite.IO`
- depends: `NetTopologySuite`
- abi: netstandard2.0
- rail: spatial-values

- Registers `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): the `Geometry` model this codec decodes to, the `WKBReader`/`WKBWriter` core codecs whose body the blob reuses, and `NtsGeometryServices` with its `PrecisionModel` and `Ordinates` policy all resolve there.

## [02]-[PUBLIC_TYPES]

[GEOPACKAGE_TYPES]: GeoPackage geometry blob codec

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :-------------------- | :------------ | :------------------------------- |
|  [01]   | `GeoPackageGeoReader` | class         | decodes a GPB blob to `Geometry` |
|  [02]   | `GeoPackageGeoWriter` | class         | encodes `Geometry` to a GPB blob |

## [03]-[ENTRYPOINTS]

[GEOPACKAGE_CODEC]: GeoPackage blob codec

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `GeoPackageGeoReader.Read(byte[]\|Stream) -> Geometry` | instance | decodes a GeoPackage blob      |
|  [02]   | `GeoPackageGeoWriter.Write(Geometry) -> byte[]`        | instance | encodes a `Geometry` to a blob |

- Reader/writer policy: `HandleOrdinates` caps ordinates within `AllowedOrdinates`, `HandleSRID` stamps the header SRID, `RepairRings` fixes invalid rings on read.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Blob layout is the GPB header (magic `GP`, version, endianness/envelope-kind/emptiness flags, SRID, envelope, Z/M ranges) followed by a WKB body; `HandleOrdinates` restricts written ordinates within XYZM and selects the header envelope kind, empty points encode as NaN ordinates, and `HandleSRID` stamps the header SRID onto the decoded geometry.
- The body IS the core WKB under a GeoPackage header, so the blob's ordinate set is the intersection of the writer's `HandleOrdinates` and the branch codec's `AllowedOrdinates` factory cap; a store column widened past that cap silently drops the extra dimension.

[STACKING]:
- `api-nts-geojson4stj`(`.api/api-nts-geojson4stj.md`): the sibling satellite on the same rail — both decode to one `Geometry` under one `NtsGeometryServices` precision-and-SRID configuration, so a value surviving a GeoJSON to WKB to GeoPackage round trip keeps one grid.
- `api-nts-ef`(`.api/api-nts-ef.md`), `api-npgsql`(`.api/api-npgsql.md`): a SQLite GeoPackage column round-trips through this reader/writer pair and a PostGIS column through Npgsql's NTS plugin, both producing one NTS `Geometry` — the rail stays provider-agnostic at the `Geometry` boundary and codec-specific only at the wire.
- `api-hashing`(`../../.api/api-hashing.md`): `WKBWriter.Write(Geometry) -> byte[]` keyed by `XxHash128.HashToUInt128` gives a geometry one content-stable identity across the GeoPackage blob, the PostGIS column, and GeoJSON text.

[LOCAL_ADMISSION]:
- GeoPackage geometry columns pass through `GeoPackageGeoReader`/`GeoPackageGeoWriter`.
- Raw WKB and WKT IO takes the branch core codecs directly; a satellite package never re-wraps them.

[RAIL_LAW]:
- Package: `NetTopologySuite.IO.GeoPackage`
- Owns: GeoPackage blob coding for NetTopologySuite spatial values on the `Ingest/geospatial#FEATURE_ROWS` seam
- Accept: NetTopologySuite geometry contracts on the spatial-values rail
- Reject: raw WKB columns standing in for GeoPackage geometry blobs, a folder-local re-tabling of the branch core codecs, and the GeoJSON text shaping the sibling satellite owns
