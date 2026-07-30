# [RASM_BIM_API_H3]

`Rasm.Persistence` owns the managed Uber-H3 cell algebra for this branch at `libs/csharp/Rasm.Persistence/.api/api-h3.md` — the `H3Index` bit layout, hierarchy, disk, path, region fill, cover algebra, directed-edge and vertex topology, the `LatLng` bridge and its spherical metrics — so Bim registers that surface rather than re-tabling it. This partition holds the `Semantics/geospatial#GEOSPATIAL_SEAM` DGGS keyer arm alone: a georeferenced site footprint resolved to its resolution-tagged cell beside the continuous `NetTopologySuite` planar algebra, and the coarse `ulong` bucket a `GeoModel` joins on beside its `STRtree` envelope broad-phase.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: Bim geospatial-seam partition of `pocketken.H3`
- package: `pocketken.H3` (Apache-2.0, direct `PackageReference`)
- assembly: `pocketken.H3` — pure-managed AnyCPU, no native dylib
- namespace: `H3`, `H3.Model`, `H3.Extensions`, `H3.Algorithms`
- depends: `NetTopologySuite` — `Point`/`Polygon`/`MultiPolygon`/`Geometry`/`LineString`/`Coordinate` bridge the cell boundary
- rail: `Semantics/geospatial#GEOSPATIAL_SEAM` (the DGGS site-context keyer arm)

- Registers the cell algebra(`libs/csharp/Rasm.Persistence/.api/api-h3.md`): `H3Index` with its decode roster and key bridge, `H3.Model.Mode`/`Direction`/`LatLng`/`CoordIJ`/`BaseCell`, `H3.Constants`, `H3.Utils`, `H3IndexJsonConverter`, `RingCell`, `VertexTestMode`, the `HexRingException` family, and every hierarchy, disk, path, fill, cover, edge, vertex, and metric member resolve there under their extension hosts — a member verified against that catalogue is verified for this seam, and re-tabling one here forks the branch's cell truth.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Cell construction and boundary consume and produce `NetTopologySuite.Geometries` types, so an H3 cell and a `GeoFeature.Geometry` round-trip through one NTS object — never a second coordinate model; the `GeoServices`-resolved `GeometryFactory` passes explicitly wherever project precision or SRID differs from the package default.
- `(ulong)index` is the durable, resolution-tagged bucket a `GeoModel` keys a coarse DGGS join on beside the `STRtree`: a regional query lowers `Geometry.Fill`/`GridDiskDistances` → a `FrozenSet<ulong>` cover → a membership test, never a per-feature scan. `H3Index.Invalid` (the zero `ulong`) is the sole sentinel — an out-of-range coordinate decodes to it and projects to `Option<H3Index>.None` at the boundary, never a stored `0`.
- Site-region input takes the pentagon-tolerant disk walk; the throwing fast path serves known-interior bulk fills alone.

[STACKING]:
- `h3-pg`(`libs/csharp/Rasm.Persistence/.api/api-h3-pg.md`): the same dual central pin computes the identical 64-bit cell in-process here and server-side there (`h3_latlng_to_cell`), so a site context keyed in Bim and persisted through `Rasm.Persistence` agree bit-for-bit; the `Rasm.Persistence/Element/identity#ELEMENT_IDENTITY` `H3Cell.Of(point, res)` and `H3CellOps.Cover`/`Disk`/`Compact` lower `Geometry.Fill`/`GridDiskDistances`/`CompactCells` to the `FrozenSet<ulong>` the `h3_cell = ANY(@cells)` prefilter tests.
- `NetTopologySuite.IO.GeoJSON4STJ`(`.api/api-nts-geojson4stj.md`) / `NetTopologySuite.IO.GeoPackage`(`.api/api-nts-geopackage.md`): a `MultiPolygon` cell-set boundary from `GetCellBoundaries` serializes through the one Bim geo codec — never a hand-spelled GeoJSON document.
- `ProjNET`(`.api/api-projnet.md`): cell construction takes SRID-4326 lat/lng, so a project-CRS `GeoFeature` reprojects through the `Semantics/georeference#GEODETIC_TRANSFORM` `MathTransform` leg into 4326 before `FromPoint`/`Fill` and the boundary reprojects back — the datum bridge is never a hand-rolled great-circle conversion.
- geospatial seam (within-lib): `GeoFeature.Cell` mints `H3Index.FromPoint` over the Wgs84 centroid, `GeoModel.Bucket` keys the DGGS join beside the `STRtree`, and `GeoModel.Cover` lowers a probe region `Geometry.Fill` → `GridDiskDistances` → `CompactCells` into the `FrozenSet<ulong>` region key.

[LOCAL_ADMISSION]:
- `pocketken.H3` admits at the geospatial-seam DGGS keyer arm; a site-context cell enters the repo as the `(ulong)` durable id, never a live `H3Index` instance.

[RAIL_LAW]:
- Package: `pocketken.H3`
- Owns: the in-process DGGS site-context key and the NTS-geometry to cell bridge at the geospatial seam
- Accept: the `H3Index` `ulong` durable key (two-way implicit conversion), NTS `Point`/`Geometry` at the boundary, the `GeoServices`-resolved `GeometryFactory`
- Reject: a member roster for the package here, storing a mutable `H3Index` value instead of its `(ulong)` id, the `Invalid` sentinel crossing the boundary unwrapped, a second coordinate model beside the Bim NTS stack, and hand-rolled great-circle or area math the registered cell metrics own
