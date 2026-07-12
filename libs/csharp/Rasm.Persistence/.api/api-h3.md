# [RASM_PERSISTENCE_API_H3]

`pocketken.H3` is the managed Uber-H3 v4 hexagonal hierarchical geospatial indexer — the in-process counterpart to the `h3-pg` server extension (`api-pg-search` cluster sibling) so the same 64-bit cell id is computed identically at `Ingest/geospatial` and inside PostgreSQL via `h3_latlng_to_cell`, and the `H3Index` ulong is the durable content-key dimension `Element/identity` keysets on. The whole surface composes the transitive core `NetTopologySuite` (`Point`/`Polygon`/`MultiPolygon`/`Geometry`/`LineString`/`Coordinate` — H3's SOLE package dependency): `H3Index.FromPoint`/`ToPoint`, `Polyfill.Fill(Geometry)`, and `H3GeometryExtensions.GetCellBoundary` cross at that NTS boundary, so a PostGIS `geometry` column and an H3 cell column are two projections of one coordinate. The admitted `NetTopologySuite.IO.GeoJSON4STJ`/`GeoPackage` writers are SEPARATE NTS-IO packages that serialize those NTS `Point`/`Polygon` results at the wire — never a dependency `pocketken.H3` pulls.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pocketken.H3`
- package: `pocketken.H3`
- assembly: `pocketken.H3`
- namespace: `H3`, `H3.Model`, `H3.Extensions`, `H3.Algorithms`
- license: Apache-2.0 (`<license type="file">LICENSE</license>`; the H3.net managed port carries the Uber-H3 upstream Apache-2.0)
- target framework: `net6.0` asset on the `net10.0` floor (package ships `net6.0`/`netstandard2.1`/`netstandard2.0`; net6.0 wins NuGet precedence — there is no `net10.0` asset, so the consumer binds the `net6.0` lib)
- dependency: `NetTopologySuite` (nuspec floor `2.5.0`; the workspace central pin binds `2.6.0` — the admitted Persistence NTS stack the `Npgsql.NetTopologySuite` plugin and the `NetTopologySuite.IO.GeoJSON4STJ`/`GeoPackage` writers compose); the geometry types `Point`/`Polygon`/`MultiPolygon`/`Geometry`/`LineString`/`Coordinate`/`GeometryFactory` flow through. The `net6.0` asset pulls only NTS (the `netstandard2.0`/`2.1` assets additionally pull `System.Text.Json`/`Microsoft.Bcl.HashCode`, never bound here)
- asset: runtime library, pure-managed AnyCPU (no native dylib — the algorithm is a managed C# port, distinct from `LightningDB`/`rocksdb` native engines)
- rail: geospatial-index

## [02]-[PUBLIC_TYPES]

[CELL_TYPES]: the H3 cell index and its decoded bit layout
- rail: geospatial-index

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [CAPABILITY]                                                                                        |
| :-----: | :---------------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `H3Index : IComparable<H3Index>`                      | cell index      | the 64-bit cell id; mutable class wrapping a `ulong`, `IComparable` for keyset order                |
|  [02]   | `H3Index.Invalid`                                     | sentinel        | `new H3Index(0uL)` — the absent-cell value projecting to `Option<H3Index>` at the boundary          |
|  [03]   | `H3.Model.Mode`                                       | enum            | `Unknown`(0)/`Cell`(1)/`UniEdge`(2)/`Vertex`(4) index-mode discriminant on the high bits            |
|  [04]   | `H3.Model.Direction`                                  | enum            | the IJK hex direction (`Center`/`K`/`J`/`JK`/`I`/`IK`/`IJ`/`Invalid`) for child/neighbour traversal |
|  [05]   | `H3.Model.LatLng`                                     | value           | radian lat/lng pair (`H3.Model.GeoCoord : LatLng` is the legacy alias)                              |
|  [06]   | `H3.Model.CoordIJ`                                    | value           | local IJ coordinate for grid-distance / line algebra                                                |
|  [07]   | `H3.Model.BaseCell`                                   | value           | one of the 122 base cells; `H3Index.BaseCell`/`BaseCellNumber` decode it                            |
|  [08]   | `H3.Constants`                                        | constants       | resolution caps, `EARTH_RADIUS_KM`, hexagon geometry literals                                       |
|  [09]   | `H3.H3IndexJsonConverter : JsonConverter<H3Index>`    | STJ converter   | System.Text.Json hex-string round-trip — register on the `Schema/converters` STJ mount              |
|  [10]   | `H3.Algorithms.RingCell(H3Index index, int distance)` | readonly struct | a k-ring member carrying its grid distance from origin                                              |
|  [11]   | `H3.Algorithms.VertexTestMode`                        | enum            | `Center`/`Any`/`All` polyfill cell-acceptance predicate (`Fill` defaults `Center`)                  |

`H3Index` decode properties (read-only projections of the ulong): `Resolution` (int 0-15), `Mode` (`Mode`), `BaseCell`/`BaseCellNumber`, `Direction`, `IsValidCell` (`IsValid` aliases it), `IsPentagon`, `LeadingNonZeroDirection`, `MaximumFaceCount`, `ReservedBits`/`HighBit`. The class is mutable (`RotateClockwise`/`SetDirectionForResolution` mutate in place) — treat a stored `H3Index` as the immutable ulong (`(ulong)index` / `index.ToString()` hex) and never share a live instance across a fold. Two implicit conversions bridge `H3Index`↔`ulong` (`implicit operator ulong(H3Index)` / `implicit operator H3Index(ulong)`) plus `==`/`!=` overloads against a bare `ulong`, so the cell flows as a `ulong` key with zero cast ceremony — `new H3Index(id)` rehydrates a stored cell, `(ulong)index` is the durable form.

## [03]-[ENTRYPOINTS]

[CONSTRUCTION]: lat/lng or geometry to cell, cell to geometry
- rail: geospatial-index

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                                         |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `H3Index.FromLatLng(LatLng latLng, int resolution)`                | static factory | radian lat/lng → cell at resolution (v4 `latLngToCell`)                              |
|  [02]   | `H3Index.FromPoint(Point point, int resolution)`                   | static factory | **NTS** `Point` (SRID 4326) → cell — the PostGIS-geometry bridge                     |
|  [03]   | `H3Index.FromGeoCoord(GeoCoord latLng, int resolution)`            | static factory | legacy alias of `FromLatLng`                                                         |
|  [04]   | `H3Index.Create(int resolution, int baseCell, Direction dir)`      | static factory | constructs from explicit base-cell + direction                                       |
|  [05]   | `H3Index.ToLatLng()` / `ToGeoCoord()`                              | instance       | cell → centroid radian lat/lng                                                       |
|  [06]   | `H3Index.ToPoint(GeometryFactory? factory = null)`                 | instance       | cell → **NTS** centroid `Point` (defaults `Utils.DefaultGeometryFactory`, SRID 4326) |
|  [07]   | `Coordinate.ToH3Index(int resolution, …)` (`H3GeometryExtensions`) | extension      | **NTS** `Coordinate` → cell                                                          |
|  [08]   | `H3Index.ToCoordinate(…)` (`H3GeometryExtensions`)                 | extension      | cell → **NTS** `Coordinate`                                                          |

[HIERARCHY]: parent / child / neighbour traversal — `H3HierarchyExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `GetParentForResolution(this H3Index, int parentRes)`             | hierarchy up   | coarser-resolution ancestor cell                           |
|  [02]   | `GetChildCenterForResolution(this H3Index, int childRes)`         | hierarchy down | center child at finer resolution                           |
|  [03]   | `GetChildrenForResolution(this H3Index, int childRes)`            | hierarchy down | `IEnumerable<H3Index>` of all finer children               |
|  [04]   | `GetDirectChild(this H3Index, Direction)`                         | hierarchy down | one immediate child by direction                           |
|  [05]   | `Contains(this H3Index parent, H3Index child)` / `ContainedBy`    | containment    | hierarchical containment predicate (range-query prefilter) |
|  [06]   | `GetNeighbours(this H3Index)` / `GetDirectNeighbour(…)`           | adjacency      | same-resolution neighbour cells / one by direction         |
|  [07]   | `IsNeighbour(this H3Index, H3Index)` / `DirectionForNeighbour(…)` | adjacency      | adjacency predicate / the connecting `Direction`           |

[DISK_AND_PATH]: k-ring disk, grid distance, line — `H3.Algorithms.Rings` / `Lines`
- rail: geospatial-index

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                               |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `GridRing(this H3Index, int k)` (= `GetHexRing`)                                     | disk           | the hollow ring at distance k (v4 `gridRingUnsafe`; there is no plain `GridDisk` — the filled disk is `GridDiskDistances`) |
|  [02]   | `GridDiskDistances(this H3Index, int k)` (= `GetKRing`)                              | disk           | filled disk → `IEnumerable<RingCell>` carrying each cell's distance (v4 `gridDiskDistances`)                               |
|  [03]   | `GridDiskDistancesSafe`/`Unsafe` (= `GetKRingSlow`/`Fast`)                           | disk           | pentagon-safe vs. fast-path filled disk (Unsafe throws on pentagon)                                                        |
|  [04]   | `GridDistance(this H3Index, H3Index)` (= `DistanceTo`)                               | metric         | integer grid distance between two same-resolution cells                                                                    |
|  [05]   | `GridPathCells(this H3Index, H3Index)` (= `LineTo`)                                  | path           | `IEnumerable<H3Index>` straight-line cell path                                                                             |
|  [06]   | `CellToLocalIj(this H3Index origin, H3Index)` (`H3LocalIJExtensions`, = `ToLocalIJ`) | local coords   | cell → `CoordIJ` in origin's local frame                                                                                   |
|  [07]   | `LocalIjToCell(this H3Index origin, CoordIJ)` (= `FromLocalIJ`)                      | local coords   | `CoordIJ` → cell in origin's local frame                                                                                   |

[REGION_AND_SET]: polyfill, cell boundary, compaction — `H3.Algorithms.Polyfill` / `H3GeometryExtensions` / `H3SetExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `Geometry.Fill(this Geometry, int res, VertexTestMode = Center)`                   | polyfill       | **NTS** polygon → covering `IEnumerable<H3Index>` (v4 `polygonToCells`) |
|  [02]   | `LineString.Fill(this LineString, int res)`                                        | polyfill       | **NTS** polyline → traversed cells                                      |
|  [03]   | `Geometry.IsTransMeridian(this Geometry)`                                          | polyfill guard | antimeridian-crossing detection before fill                             |
|  [04]   | `H3Index.GetCellBoundary(this H3Index, GeometryFactory? = null)`                   | boundary       | cell → **NTS** `Polygon` hex boundary                                   |
|  [05]   | `IEnumerable<H3Index>.GetCellBoundaries(this …, GeometryFactory? = null)`          | boundary       | cell set → **NTS** `MultiPolygon` (one GeoJSON feature collection)      |
|  [06]   | `IEnumerable<H3Index>.CompactCells(this …)` (= `Compact`)                          | set            | minimal mixed-resolution covering set (storage-dense region key)        |
|  [07]   | `IEnumerable<H3Index>.UncompactCells(this …, int res)` (= `UncompactToResolution`) | set            | expand a compacted set back to uniform resolution                       |
|  [08]   | `IEnumerable<H3Index>.AreOfSameResolution(this …)`                                 | set guard      | uniform-resolution precondition for set algebra                         |

[EDGE_AND_AREA]: directed edges, cell metrics — `H3DirectedEdgeExtensions` / `H3GeometryExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE]                                                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `ToDirectedEdge(this H3Index origin, H3Index dest)` (= `GetUnidirectionalEdge`)                  | edge           | the directed-edge index between adjacent cells       |
|  [02]   | `OriginToDirectedEdges(this H3Index origin)` (= `GetUnidirectionalEdges`)                        | edge           | the (up to six) directed edges leaving a cell        |
|  [03]   | `DirectedEdgeToCells(this H3Index edge)` (= `GetIndexesFromUnidirectionalEdge`)                  | edge           | `(origin, destination)` tuple from an edge index     |
|  [04]   | `GetDirectedEdgeOrigin` / `GetDirectedEdgeDestination(this H3Index edge)`                        | edge           | one endpoint cell of a directed edge                 |
|  [05]   | `IsValidDirectedEdge(this H3Index edge)` (= `IsUnidirectionalEdgeValid`)                         | edge guard     | directed-edge validity test                          |
|  [06]   | `GetDirectedEdgeBoundaryVertices(this H3Index edge)` (= `GetUnidirectionalEdgeBoundaryVertices`) | boundary       | the edge's `IEnumerable<LatLng>` boundary            |
|  [07]   | `EdgeLengthMeters/Kilometers/Radians(this H3Index edge)`                                         | metric         | great-circle edge length in the chosen unit          |
|  [08]   | `CellAreaInMSquared/KmSquared/RadiansSquared(this H3Index)`                                      | metric         | exact spherical cell area                            |
|  [09]   | `GetRadiusInKm(this H3Index)` / `GetFaces(this H3Index)`                                         | metric         | cell circumradius / icosahedron faces a cell touches |

[VERTEX]: topological vertex-mode cells — `H3VertexExtensions`
- rail: geospatial-index

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                                                       |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `CellToVertex(this H3Index cell, int vertexNum)` (= `GetVertexIndex`)          | vertex         | the `Mode.Vertex` index of one of a cell's topological vertices (0..5)             |
|  [02]   | `CellToVertexes(this H3Index cell)` (= `GetVertexIndicies`)                    | vertex         | `IEnumerable<H3Index>` of all vertex-mode indices of a cell                        |
|  [03]   | `VertexToLatLng(this H3Index vertex)` (= `VertexToGeoCoord`)                   | vertex         | a vertex-mode index → its `LatLng` coordinate (the shared corner where cells meet) |
|  [04]   | `IsValidVertex(this H3Index vertex)`                                           | vertex guard   | vertex-mode validity test                                                          |
|  [05]   | `GetVertexNumberForDirection` / `GetDirectionForVertexNumber(this H3Index, …)` | vertex         | the vertex-number ↔ `Direction` mapping for a cell                                 |

## [04]-[IMPLEMENTATION_LAW]

[V4_NAME_ALIASING]:
- pocketken.H3 ships BOTH the v4-canonical names (`GridRing`, `GridDiskDistances`, `GridDistance`, `GridPathCells`, `CompactCells`, `UncompactCells`, `CellToLocalIj`, `LocalIjToCell`, `ToDirectedEdge`, `OriginToDirectedEdges`, `DirectedEdgeToCells`, `GetDirectedEdgeOrigin`/`Destination`, `IsValidDirectedEdge`, `CellToVertex`, `CellToVertexes`, `VertexToLatLng`) AND the pre-v4 legacy names (`GetHexRing`, `GetKRing`, `DistanceTo`, `LineTo`, `Compact`, `UncompactToResolution`, `ToLocalIJ`, `FromLocalIJ`, `GetUnidirectionalEdge`, `GetUnidirectionalEdges`, `GetIndexesFromUnidirectionalEdge`, `GetOriginFromUnidirectionalEdge`/`GetDestinationFromUnidirectionalEdge`, `IsUnidirectionalEdgeValid`, `GetVertexIndex`, `GetVertexIndicies`, `VertexToGeoCoord`) as distinct method pairs over the same body. Pin the v4-canonical spelling so the managed call site matches the `h3-pg` SQL function name (`h3_grid_ring_unsafe`, `h3_grid_disk_distances`, `h3_grid_distance`, `h3_compact_cells`) one-to-one — the parity is the whole point of the dual admission. There is no managed method literally named `GridDisk`: the hollow ring is `GridRing`, the filled disk-with-distances is `GridDiskDistances`.
- `GetKRingFast`/`GridDiskDistancesUnsafe` skip the pentagon-distortion check and throw `HexRingPentagonException` near a pentagon; `GridDiskDistancesSafe`/`GetKRingSlow` are the pentagon-tolerant path. Default to the safe path on user-region input; the unsafe path is for known-interior bulk fills.

[NTS_GEOMETRY_BRIDGE]:
- The geometry overloads consume and produce `NetTopologySuite.Geometries` types (`Point`/`Polygon`/`MultiPolygon`/`Geometry`/`LineString`/`Coordinate`), so an H3 cell and a PostGIS `geometry` column round-trip through the SAME NTS object the `Npgsql.NetTopologySuite` plugin already binds — no second coordinate model.
- `H3.Utils.DefaultGeometryFactory` is `new GeometryFactory(new PrecisionModel(1e16), 4326)` — SRID 4326 (WGS84) with a fixed high-precision model. The geometry overloads default to it; an explicit `GeometryFactory` enters only to match a non-4326 SRID, and callers never hand-roll a `Coordinate` the factory mints.
- `GetCellBoundaries` returns one `MultiPolygon` for a cell set — the GeoJSON `FeatureCollection` egress shape the `NetTopologySuite.IO.GeoJSON4STJ` writer serializes, so a cell-set boundary crosses the wire as one NTS-native GeoJSON document.

[CELL_AS_CONTENT_KEY]:
- The `H3Index` ulong (`(ulong)index`) is the stable, resolution-tagged spatial bucket the `Element/identity` Key axis carries and the `Query/lane` `StoreOp` keysets on — a spatial range query lowers to `GridDiskDistances`/`Fill` → a `FrozenSet<ulong>` cell set → an `IN (…)`/`= ANY` predicate, never a per-row `ST_DWithin` scan.
- `H3Index.Invalid` (the zero ulong) is the only sentinel; an out-of-range coordinate or an invalid decode yields it, and it projects to `Option<H3Index>.None` at the boundary per the `NEVER propagate sentinels` law — never a stored `0` cell.

## [05]-[STACKING_AND_RAIL]

[STACKING]:
- ingest parity with `h3-pg`: `H3Index.FromPoint(point, res)` at ingest computes the identical cell id `h3-pg`'s `h3_latlng_to_cell((lng, lat), res)` computes server-side, so a row's `h3_cell` generated column (DDL on the EF migration rail, `Element/identity#SCHEMA_VERDICT`, over the `Store/provisioning#SERVER_EXTENSIONS` `h3`/`h3_postgis` `ServerExtension` rows) and the managed cell agree bit-for-bit — the dual admission exists to make in-process and in-database indexing one vocabulary.
- spatial lane prefilter: `Polyfill.Fill(region, res)` / `GridDiskDistances(center, k)` produce the candidate cell set the `Query/lane#ELEMENT_SET_ALGEBRA` `SetPredicate.Spatial` PostGIS leg intersects, turning a radius/region query into a hierarchical cell-set membership test the H3 GiST/BRIN index on the `h3_cell` column answers, with `CompactCells` collapsing the set to the densest mixed-resolution key before the predicate.
- geometry codec at the wire: the `Point`/`Polygon`/`MultiPolygon` results ride the admitted `NetTopologySuite.IO.GeoJSON4STJ` (GeoJSON) and `NetTopologySuite.IO.GeoPackage` writers, so an H3 region boundary serializes through the one NTS geo codec the `Schema/converters` STJ mount already owns — no hand-spelled GeoJSON.
- routing seam: `GridPathCells`/`GridDistance` feed the `Query/cypher#GRAPH_QUERY` pgrouting routing cases (`Store/provisioning` server-extension row) with H3-cell graph nodes, so the in-process path and the in-database `pgr_dijkstra` share the cell-id node space.
- STJ converter: `H3IndexJsonConverter` registers on the `Schema/converters` System.Text.Json mount so an `H3Index` serializes as its hex string in a JSON projection without a hand-written converter.

[RAIL_LAW]:
- Packages: `pocketken.H3` (composes `NetTopologySuite`)
- Owns: managed Uber-H3 v4 cell indexing, hierarchy/disk/path/region algebra, the directed-edge and vertex-mode topology, the cell-area/edge-length metrics, and the NTS-geometry ↔ cell bridge
- Accept: the `H3Index` ulong as the durable spatial key, the v4-canonical method spelling, NTS `Point`/`Geometry` at the boundary, `Utils.DefaultGeometryFactory` SRID 4326
- Reject: storing a live mutable `H3Index` instance, the `H3Index.Invalid` sentinel crossing the boundary unwrapped, a second coordinate model beside NTS, the legacy method names at a call site that must match `h3-pg`, hand-rolled great-circle/area math the `H3GeometryExtensions` metrics own
