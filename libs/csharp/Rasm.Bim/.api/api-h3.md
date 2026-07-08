# [RASM_BIM_API_H3]

`pocketken.H3` is the managed Uber-H3 v4 hexagonal hierarchical geospatial indexer — the Bim-scoped DGGS site-context keyer that hands the georeferenced-BIM geospatial seam a discrete, resolution-tagged spatial bucket beside the continuous `NetTopologySuite` Simple-Features planar algebra. The whole surface composes the Bim-owned `NetTopologySuite` geo stack: `H3Index.FromPoint`/`ToPoint`, `Geometry.Fill`, and `H3Index.GetCellBoundary` cross at the NTS `Point`/`Polygon`/`Geometry` boundary the `Semantics/geospatial#GEOSPATIAL_SEAM` `GeoFeature` row already carries, so a site footprint and its covering H3 cell set are two projections of one coordinate sequence, the cell boundary rides the same `GeometryFactory` the `GeoServices` `NtsGeometryServices.Instance` root mints, and reprojection stays the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg. The `H3Index` ulong is the durable, resolution-tagged site-context key a `GeoModel` feature buckets on for a coarse DGGS spatial join beside the `STRtree` envelope broad-phase. This is the in-process counterpart to the Persistence `h3-pg` server extension (`Rasm.Persistence/.api/api-h3-pg.md`): the same dual central pin computes the identical 64-bit cell id in-process here and in-database there, so a site context indexed in Bim and persisted through `Rasm.Persistence` agree bit-for-bit.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pocketken.H3`
- package: `pocketken.H3`
- assembly: `pocketken.H3`
- namespace: `H3`, `H3.Model`, `H3.Extensions`, `H3.Algorithms`
- license: Apache-2.0 (`<license type="file">LICENSE</license>`; the H3.net managed port carries the Uber-H3 upstream Apache-2.0)
- target framework: `net6.0` asset on the `net10.0` floor (package ships `net6.0`/`netstandard2.1`/`netstandard2.0`; net6.0 wins NuGet precedence — there is no `net10.0` asset, so the consumer binds the `lib/net6.0` build)
- dependency: `NetTopologySuite` (nuspec floor; the workspace central pin binds — the same admitted Bim NTS stack `Semantics/geospatial#GEOSPATIAL_SEAM` composes); the geometry types `Point`/`Polygon`/`MultiPolygon`/`Geometry`/`LineString`/`Coordinate`/`GeometryFactory` flow through. The `net6.0` asset pulls only NTS (the `netstandard2.0`/ assets additionally pull `System.Text.Json`/`Microsoft.Bcl.HashCode`, never bound here)
- asset: runtime library, pure-managed AnyCPU (no native dylib — a managed C# port, distinct from the `MaxRev.Gdal.Core` native OGR engine)
- rail: `Semantics/geospatial#GEOSPATIAL_SEAM` (the DGGS site-context keyer arm)

## [02]-[PUBLIC_TYPES]

[CELL_TYPES]: the H3 cell index and its decoded bit layout
- rail: geospatial-index

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------ |:----------------- |:--------------------------------------------------------------------------- |
| [01] | `H3Index: IComparable<H3Index>` | cell index | the 64-bit cell id; `sealed class` wrapping a `ulong`, `IComparable` for keyset order |
| [02] | `H3Index.Invalid` | sentinel | `new H3Index(0uL)` — the absent-cell value projecting to `Option<H3Index>` at the boundary |
| [03] | `H3.Model.Mode` | enum | `Unknown`(0)/`Cell`(1)/`UniEdge`(2)/`Vertex`(4) index-mode discriminant on the high bits (`H3_MODE_OFFSET` 59) |
| [04] | `H3.Model.Direction` | enum | the IJK hex direction (`Center`/`K`/`J`/`JK`/`I`/`IK`/`IJ`/`Invalid`) for child/neighbour traversal |
| [05] | `H3.Model.LatLng` | value | radian lat/lng pair (`H3.Model.GeoCoord: LatLng` is the alias) |
| [06] | `H3.Model.CoordIJ` | value | local IJ coordinate for grid-distance / line algebra |
| [07] | `H3.Model.BaseCell` | value | one of the 122 base cells; `H3Index.BaseCell`/`BaseCellNumber` decode it |
| [08] | `H3.Constants` | constants | resolution caps, `EARTH_RADIUS_KM`, hexagon geometry literals |
| [09] | `H3.H3IndexJsonConverter: JsonConverter<H3Index>` | STJ converter | System.Text.Json hex-string round-trip for an `H3Index` site-context column |
| [10] | `H3.Algorithms.RingCell(H3Index index, int distance)` | readonly struct | a k-ring member carrying its grid distance from origin |
| [11] | `H3.Algorithms.VertexTestMode` | enum | `Center`/`Any`/`All` polyfill cell-acceptance predicate (`Fill` defaults `Center`) |

`H3Index` decode properties (read-only projections of the ulong): `Resolution` (int 0-15), `Mode` (`Mode`), `BaseCell`/`BaseCellNumber`, `Direction`, `IsValidCell` (`IsValid` aliases it), `IsPentagon`, `LeadingNonZeroDirection`, `MaximumFaceCount`, `ReservedBits`/`HighBit`. The class is mutable (`RotateClockwise`/`SetDirectionForResolution` mutate in place) — treat a stored `H3Index` as the immutable ulong (`(ulong)index` / `index.ToString` hex) and never share a live instance across a fold. Two implicit conversions bridge `H3Index`↔`ulong` (`implicit operator ulong(H3Index)` / `implicit operator H3Index(ulong)`) plus `==`/`!=` overloads against a bare `ulong`, so the cell flows as a `ulong` key with zero cast ceremony — `new H3Index(id)` rehydrates a stored cell, `(ulong)index` is the durable form.

## [03]-[ENTRYPOINTS]

[CONSTRUCTION]: lat/lng or geometry to cell, cell to geometry
- rail: geospatial-index

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------ |:------------- |:------------------------------------------------------ |
| [01] | `H3Index.FromLatLng(LatLng latLng, int resolution)` | static factory | radian lat/lng → cell at resolution (v4 `latLngToCell`) |
| [02] | `H3Index.FromPoint(Point point, int resolution)` | static factory | NTS `Point` (SRID 4326) → cell — the `GeoFeature` geometry bridge |
| [03] | `H3Index.FromGeoCoord(GeoCoord latLng, int resolution)` | static factory | alias of `FromLatLng` |
| [04] | `H3Index.Create(int resolution, int baseCell, Direction dir)` | static factory | constructs from explicit base-cell + direction |
| [05] | `H3Index.ToLatLng()` / `ToGeoCoord()` | instance | cell → centroid radian lat/lng |
| [06] | `H3Index.ToPoint(GeometryFactory? factory = null)` | instance | cell → NTS centroid `Point` (defaults `Utils.DefaultGeometryFactory`, SRID 4326) |
| [07] | `Coordinate.ToH3Index(int resolution, …)` (`H3GeometryExtensions`) | extension | NTS `Coordinate` → cell |
| [08] | `H3Index.ToCoordinate(…)` (`H3GeometryExtensions`) | extension | cell → NTS `Coordinate` |

[HIERARCHY]: parent / child / neighbour traversal — `H3HierarchyExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:--------------------------------------------------------- |:------------- |:------------------------------------------------- |
| [01] | `GetParentForResolution(this H3Index, int parentRes)` | hierarchy up | coarser-resolution ancestor cell |
| [02] | `GetChildCenterForResolution(this H3Index, int childRes)` | hierarchy down | center child at finer resolution |
| [03] | `GetChildrenForResolution(this H3Index, int childRes)` | hierarchy down | `IEnumerable<H3Index>` of all finer children |
| [04] | `GetDirectChild(this H3Index, Direction)` | hierarchy down | one immediate child by direction |
| [05] | `Contains(this H3Index parent, H3Index child)` / `ContainedBy` | containment | hierarchical containment predicate (range-query prefilter) |
| [06] | `GetNeighbours(this H3Index)` / `GetDirectNeighbour(…)` | adjacency | same-resolution neighbour cells / one by direction |
| [07] | `IsNeighbour(this H3Index, H3Index)` / `DirectionForNeighbour(…)` | adjacency | adjacency predicate / the connecting `Direction` |

[DISK_AND_PATH]: k-ring disk, grid distance, line — `H3.Algorithms.Rings` / `Lines`
- rail: geospatial-index

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:----------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `GridRing(this H3Index, int k)` (= `GetHexRing`) | disk | the hollow ring at distance k (v4 `gridRingUnsafe`; there is no plain `GridDisk` — the filled disk is `GridDiskDistances`) |
| [02] | `GridDiskDistances(this H3Index, int k)` (= `GetKRing`) | disk | filled disk → `IEnumerable<RingCell>` carrying each cell's distance (v4 `gridDiskDistances`) |
| [03] | `GridDiskDistancesSafe`/`Unsafe` (= `GetKRingSlow`/`Fast`) | disk | pentagon-safe vs. fast-path filled disk (Unsafe throws on pentagon) |
| [04] | `GridDistance(this H3Index, H3Index)` (= `DistanceTo`) | metric | integer grid distance between two same-resolution cells |
| [05] | `GridPathCells(this H3Index, H3Index)` (= `LineTo`) | path | `IEnumerable<H3Index>` straight-line cell path |
| [06] | `CellToLocalIj(this H3Index origin, H3Index)` (`H3LocalIJExtensions`, = `ToLocalIJ`) | local coords | cell → `CoordIJ` in origin's local frame |
| [07] | `LocalIjToCell(this H3Index origin, CoordIJ)` (= `FromLocalIJ`) | local coords | `CoordIJ` → cell in origin's local frame |

[REGION_AND_SET]: polyfill, cell boundary, compaction — `H3.Algorithms.Polyfill` / `H3GeometryExtensions` / `H3SetExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:--------------------------------------------------------------------- |:------------- |:----------------------------------------------------------------- |
| [01] | `Geometry.Fill(this Geometry, int res, VertexTestMode = Center)` | polyfill | NTS polygon → covering `IEnumerable<H3Index>` (v4 `polygonToCells`) — a site footprint to its cell cover |
| [02] | `LineString.Fill(this LineString, int res)` | polyfill | NTS polyline → traversed cells |
| [03] | `Geometry.IsTransMeridian(this Geometry)` | polyfill guard | antimeridian-crossing detection before fill |
| [04] | `H3Index.GetCellBoundary(this H3Index, GeometryFactory? = null)` | boundary | cell → NTS `Polygon` hex boundary |
| [05] | `IEnumerable<H3Index>.GetCellBoundaries(this …, GeometryFactory? = null)` | boundary | cell set → NTS `MultiPolygon` (one GeoJSON feature collection) |
| [06] | `IEnumerable<H3Index>.CompactCells(this …)` (= `Compact`) | set | minimal mixed-resolution covering set (storage-dense region key) |
| [07] | `IEnumerable<H3Index>.UncompactCells(this …, int res)` (= `UncompactToResolution`) | set | expand a compacted set back to uniform resolution |
| [08] | `IEnumerable<H3Index>.AreOfSameResolution(this …)` | set guard | uniform-resolution precondition for set algebra |

[EDGE]: directed-edge index algebra — `H3DirectedEdgeExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `ToDirectedEdge(this H3Index origin, H3Index dest)` (= `GetUnidirectionalEdge`) | edge | the directed-edge index between two adjacent cells |
| [02] | `OriginToDirectedEdges(this H3Index origin)` (= `GetUnidirectionalEdges`) | edge | the (up to six) directed edges leaving a cell |
| [03] | `DirectedEdgeToCells(this H3Index edge)` (= `GetIndexesFromUnidirectionalEdge`) | edge | `(origin, destination)` tuple from an edge index |
| [04] | `GetDirectedEdgeOrigin` / `GetDirectedEdgeDestination(this H3Index edge)` | edge | one endpoint cell of a directed edge |
| [05] | `IsValidDirectedEdge(this H3Index edge)` (= `IsUnidirectionalEdgeValid`) | edge guard | directed-edge validity test |
| [06] | `EdgeLengthMeters/Kilometers/Radians(this H3Index edge)` (`GetExactEdgeLengthInRadians` exact) | metric | great-circle edge length in the chosen unit |
| [07] | `GetDirectedEdgeBoundaryVertices(this H3Index edge)` (= `GetUnidirectionalEdgeBoundaryVertices`) | boundary | the edge's `IEnumerable<LatLng>` boundary |

[AREA_AND_METRIC]: cell metrics — `H3GeometryExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `CellAreaInMSquared/KmSquared/RadiansSquared(this H3Index)` | metric | exact spherical cell area in the chosen unit |
| [02] | `GetRadiusInKm(this H3Index)` / `GetFaces(this H3Index)` | metric | cell circumradius / icosahedron faces a cell touches |
| [03] | `GetCellBoundaryVertices(this H3Index)` | boundary | raw `IEnumerable<LatLng>` boundary (`GetCellBoundary` wraps it into an NTS `Polygon`) |

[VERTEX]: topological vertex-mode cells — `H3VertexExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------- |:------------- |:-------------------------------------------------------- |
| [01] | `CellToVertex(this H3Index cell, int vertexNum)` (= `GetVertexIndex`) | vertex | the `Mode.Vertex` index of one of a cell's topological vertices (0..5) |
| [02] | `CellToVertexes(this H3Index cell)` (= `GetVertexIndicies`) | vertex | `IEnumerable<H3Index>` of all vertex-mode indices of a cell |
| [03] | `VertexToLatLng(this H3Index vertex)` (= `VertexToGeoCoord`) | vertex | a vertex-mode index → its `LatLng` coordinate (the shared corner where cells meet) |
| [04] | `IsValidVertex(this H3Index vertex)` | vertex guard | vertex-mode validity test |
| [05] | `GetVertexNumberForDirection` / `GetDirectionForVertexNumber(this H3Index, …)` | vertex | the vertex-number ↔ `Direction` mapping for a cell |

## [04]-[IMPLEMENTATION_LAW]

[V4_NAME_ALIASING]:
- pocketken.H3 ships BOTH the v4-canonical names (`GridRing`, `GridDiskDistances`, `GridDistance`, `GridPathCells`, `CompactCells`, `UncompactCells`, `CellToLocalIj`, `LocalIjToCell`, `ToDirectedEdge`, `OriginToDirectedEdges`, `DirectedEdgeToCells`, `GetDirectedEdgeOrigin`/`Destination`, `IsValidDirectedEdge`, `CellToVertex`, `CellToVertexes`, `VertexToLatLng`) AND the pre-v4 names (`GetHexRing`, `GetKRing`, `DistanceTo`, `LineTo`, `Compact`, `UncompactToResolution`, `ToLocalIJ`, `FromLocalIJ`, `GetUnidirectionalEdge`, `GetUnidirectionalEdges`, `GetIndexesFromUnidirectionalEdge`, `GetOriginFromUnidirectionalEdge`/`GetDestinationFromUnidirectionalEdge`, `IsUnidirectionalEdgeValid`, `GetVertexIndex`, `GetVertexIndicies`, `VertexToGeoCoord`) as distinct method pairs over the same body. Pin the v4-canonical spelling so a Bim site-context cell id matches the `h3-pg` SQL function name (`h3_grid_ring_unsafe`, `h3_grid_disk_distances`, `h3_grid_distance`, `h3_compact_cells`, `h3_cell_to_vertex`) `Rasm.Persistence` persists against one-to-one. There is no managed method literally named `GridDisk`: the hollow ring is `GridRing`, the filled disk-with-distances is `GridDiskDistances`.
- `GetKRingFast`/`GridDiskDistancesUnsafe` skip the pentagon-distortion check and throw `HexRingPentagonException` near a pentagon; `GridDiskDistancesSafe`/`GetKRingSlow` are the pentagon-tolerant path. Default to the safe path on site-region input; the unsafe path is for known-interior bulk fills.

[NTS_GEOMETRY_BRIDGE]:
- The geometry overloads consume and produce `NetTopologySuite.Geometries` types (`Point`/`Polygon`/`MultiPolygon`/`Geometry`/`LineString`/`Coordinate`), so an H3 cell and a `GeoFeature.Geometry` round-trip through the SAME NTS object the `Semantics/geospatial#GEOSPATIAL_SEAM` already carries — no second coordinate model beside the Bim NTS stack.
- `H3.Utils.DefaultGeometryFactory` is `new GeometryFactory(new PrecisionModel(1e16), 4326)` — SRID 4326 (WGS84) with a fixed high-precision model. The geometry overloads default to it; pass the `GeoServices`-resolved `GeometryFactory` to match the project precision/SRID, and never hand-roll a `Coordinate` the factory must mint.
- `GetCellBoundaries` returns one `MultiPolygon` for a cell set — the GeoJSON `FeatureCollection` egress shape the `NetTopologySuite.IO.GeoJSON4STJ` writer the seam already owns serializes, so a cell-set boundary crosses the wire as one NTS-native GeoJSON document.

[CELL_AS_SITE_KEY]:
- The `H3Index` ulong (`(ulong)index`) is the stable, resolution-tagged site-context bucket a `GeoModel` can key a coarse DGGS spatial join on beside the `STRtree` envelope broad-phase — a regional context query lowers to `GridDiskDistances`/`Fill` → a `FrozenSet<ulong>` cell set → a cell-membership test, never a per-feature scan.
- `H3Index.Invalid` (the zero ulong) is the only sentinel; an out-of-range coordinate or an invalid decode yields it, and it projects to `Option<H3Index>.None` at the boundary per the `NEVER propagate sentinels` law — never a stored `0` cell.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- geospatial-seam keyer: `H3Index.FromPoint(feature.Geometry.Centroid, res)` / `Geometry.Fill(footprint, res)` derive the DGGS cell (or cover) of a `GeoFeature` the `Semantics/geospatial#GEOSPATIAL_SEAM` materializes, so a site context carries a discrete hex key beside its continuous NTS geometry and a coarse regional bucket sits beside the `STRtree` envelope index.
- persistence parity with `h3-pg`: the same dual central pin computes the identical cell id in-process here and in-database in `Rasm.Persistence` (`h3_latlng_to_cell` over `h3-pg`, the v4-canonical spelling), so a site context keyed in Bim and persisted server-side agree bit-for-bit — the dual admission exists to make the in-process and in-database site index one vocabulary. The `Query/lanes#GEO_LANES` `H3Cell`/`H3CellOps` owner is the realized consumer: `H3Cell.Of(point, res)` wraps `H3Index.FromPoint`, and `H3CellOps.Cover`/`Disk`/`Compact` lower `Geometry.Fill`/`GridDiskDistances`/`CompactCells` to the `FrozenSet<ulong>` the `h3_cell = ANY(@cells)` prefilter tests.
- geometry codec at the wire: the `Point`/`Polygon`/`MultiPolygon` results ride the admitted `NetTopologySuite.IO.GeoJSON4STJ` (GeoJSON) and `NetTopologySuite.IO.GeoPackage` writers the seam already composes, so an H3 region boundary serializes through the one Bim NTS geo codec — no hand-spelled GeoJSON.
- reprojection seam: cell construction takes SRID-4326 lat/lng, so a project-CRS `GeoFeature` reprojects through the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg into 4326 before `FromPoint`/`Fill`, and the cell boundary reprojects back — the `ProjNET` `MathTransform` owns the datum bridge, never a hand-rolled great-circle conversion.

[RAIL_LAW]:
- Packages: `pocketken.H3` (composes the Bim `NetTopologySuite` stack; reprojects through `ProjNET`)
- Owns: managed Uber-H3 v4 cell indexing, hierarchy/disk/path/region algebra, the directed-edge and vertex-mode topology, the cell-area/edge-length metrics, the NTS-geometry ↔ cell bridge, and the in-process DGGS site-context key
- Accept: the `H3Index` ulong as the durable site key (the two-way implicit `ulong` conversion is the zero-ceremony path), the v4-canonical method spelling, NTS `Point`/`Geometry` at the boundary, the `GeoServices`-resolved `GeometryFactory`
- Reject: storing a live mutable `H3Index` instance, the `H3Index.Invalid` sentinel crossing the boundary unwrapped, a second coordinate model beside the Bim NTS stack, the pre-v4 method names where the call must match `h3-pg`, hand-rolled great-circle/area math the `H3GeometryExtensions` metrics own

[CONSUMER]:
- `Semantics/geospatial#GEOSPATIAL_SEAM` is the realized consumer: `GeoFeature.Cell` mints the v4-canonical `H3Index.FromPoint` cell over the Wgs84-reprojected centroid (`(ulong)` durable, `Invalid` → `None`), `GeoModel.Bucket` keys the coarse DGGS join map beside the `STRtree`, and `GeoModel.Cover` lowers a probe region through `Geometry.Fill` → `GridDiskDistances` ring expansion → `CompactCells` into the `FrozenSet<ulong>` region key the `Rasm.Persistence` `h3-pg` `h3_cell = ANY(@cells)` prefilter tests bit-for-bit.
