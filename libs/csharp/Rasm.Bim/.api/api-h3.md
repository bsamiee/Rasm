# [RASM_BIM_API_H3]

`pocketken.H3` owns the managed Uber-H3 v4 discrete-global-grid keyer for the Bim geospatial seam: a georeferenced site footprint resolves to a resolution-tagged 64-bit `H3Index` cell beside the continuous `NetTopologySuite` planar algebra. Every cell construction and boundary crosses the same NTS `Point`/`Polygon`/`Geometry` model the seam already carries, so a footprint and its H3 cover are two projections of one coordinate sequence. `H3Index` reduces to the durable `ulong` site-context bucket a coarse DGGS spatial join keys on beside the `STRtree` envelope broad-phase.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pocketken.H3`
- package: `pocketken.H3` (Apache-2.0, managed Uber-H3 v4 port)
- assembly: `pocketken.H3` — pure-managed AnyCPU, no native dylib
- namespace: `H3`, `H3.Model`, `H3.Extensions`, `H3.Algorithms`
- depends: `NetTopologySuite` — `Point`/`Polygon`/`MultiPolygon`/`Geometry`/`LineString`/`Coordinate` bridge the cell boundary
- rail: `Semantics/geospatial#GEOSPATIAL_SEAM` (the DGGS site-context keyer arm)

## [02]-[PUBLIC_TYPES]

[CELL_TYPES]: the H3 cell index and its decoded bit layout

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                                                       |
| :-----: | :----------------------------- | :-------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `H3Index`                      | struct          | 64-bit cell id — mutable value `struct` over a `ulong`, `IComparable`/`IEquatable` |
|  [02]   | `H3Index.Invalid`              | sentinel        | `static readonly` zero cell (`== default(H3Index)`) → `Option<H3Index>` boundary   |
|  [03]   | `H3.Model.Mode`                | enum            | `Unknown`(0)/`Cell`(1)/`UniEdge`(2)/`Vertex`(4) index-mode (`H3_MODE_OFFSET` 59)   |
|  [04]   | `H3.Model.Direction`           | enum            | IJK hex direction `Center`/`K`/`J`/`JK`/`I`/`IK`/`IJ`/`Invalid`                    |
|  [05]   | `H3.Model.LatLng`              | struct          | radian lat/lng value `struct` with degree projections and great-circle metrics     |
|  [06]   | `H3.Model.CoordIJ`             | value           | local IJ coordinate for grid-distance / line algebra                               |
|  [07]   | `H3.Model.BaseCell`            | value           | one of the 122 base cells; `H3Index.BaseCell`/`BaseCellNumber` decode it           |
|  [08]   | `H3.Constants`                 | constants       | resolution caps, `EARTH_RADIUS_KM`, hexagon geometry literals                      |
|  [09]   | `H3.H3IndexJsonConverter`      | STJ converter   | `JsonConverter<H3Index>` — STJ hex-string round-trip for a site-context column     |
|  [10]   | `H3.Algorithms.RingCell`       | readonly struct | `(H3Index index, int distance)` — k-ring member carrying its grid distance         |
|  [11]   | `H3.Algorithms.VertexTestMode` | enum            | `Center`/`Any`/`All` polyfill cell-acceptance predicate (`Fill` defaults `Center`) |

`[H3Index decode]`: `Resolution` `Mode` `BaseCell` `BaseCellNumber` `Direction` `IsValidCell` `IsValidIndex` `IsPentagon` `LeadingNonZeroDirection` `MaximumFaceCount` `ReservedBits` `HighBit` — read-only projections of the `ulong` (`IsValidCell` gates cell mode, `IsValidIndex` any mode).

`H3Index` is a mutable value `struct`: `RotateClockwise`/`SetDirectionForResolution` rewrite only the local copy, `null` is not a valid cell (`default(H3Index)` equals `Invalid`), and the durable form is `(ulong)index`. Two implicit conversions bridge `H3Index` ↔ `ulong` with `==`/`!=` against a bare `ulong`: a cell flows as a zero-ceremony `ulong` key, `new H3Index(id)` rehydrates a stored cell and `(ulong)index` persists it.

## [03]-[ENTRYPOINTS]

[CONSTRUCTION]: lat/lng or geometry to cell, cell to geometry

| [INDEX] | [SURFACE]                      | [SHAPE]   | [CAPABILITY]                                             |
| :-----: | :----------------------------- | :-------- | :------------------------------------------------------- |
|  [01]   | `FromLatLng(LatLng, int)`                    | factory   | radian lat/lng → cell (v4 `latLngToCell`)                 |
|  [02]   | `FromPoint(Point, int)`                      | factory   | NTS `Point` (SRID 4326) → cell — the `GeoFeature` bridge  |
|  [03]   | `Create(int, int, Direction)`                | factory   | base-cell number + lead direction                         |
|  [04]   | `Create(int, int, IReadOnlyList<Direction>)` | factory   | base-cell number + per-res digit list (v4 `constructCell`) |
|  [05]   | `ToLatLng()`                                 | instance  | cell → centroid radian lat/lng                            |
|  [06]   | `ToPoint(GeometryFactory?)`                  | instance  | cell → NTS centroid `Point` (SRID 4326)                   |
|  [07]   | `Coordinate.ToH3Index(int)`                  | extension | NTS `Coordinate` → cell (`H3GeometryExtensions`)          |
|  [08]   | `ToCoordinate(Coordinate?)`                  | extension | cell → NTS `Coordinate` (`H3GeometryExtensions`)          |

[HIERARCHY]: parent / child / neighbour traversal — `H3HierarchyExtensions` on `this H3Index`

| [INDEX] | [SURFACE]                                           | [SHAPE]   | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------- | :-------- | :--------------------------------------------------------- |
|  [01]   | `GetParentForResolution(int)`                       | extension | coarser-resolution ancestor cell                           |
|  [02]   | `GetChildCenterForResolution(int)`                  | extension | center child at finer resolution                           |
|  [03]   | `GetChildrenForResolution(int)`                     | extension | `IEnumerable<H3Index>` of all finer children               |
|  [04]   | `GetDirectChild(Direction)`                         | extension | one immediate child by direction                           |
|  [05]   | `CellToChildPos(int)`                               | extension | child ordinal within its coarser parent (v4 `cellToChildPos`) |
|  [06]   | `ChildPosToCell(long, int)`                         | extension | child ordinal + resolution → cell (v4 `childPosToCell`)    |
|  [07]   | `CellToChildrenSize(int)`                           | extension | child count at a finer resolution (buffer sizing)          |
|  [08]   | `Contains(H3Index)` / `ContainedBy`                 | extension | hierarchical containment predicate (range-query prefilter) |
|  [09]   | `GetNeighbours()` / `GetDirectNeighbour(Direction, int)` | extension | same-resolution neighbours / one by direction + rotation |
|  [10]   | `IsNeighbour(H3Index)` / `DirectionForNeighbour(…)` | extension | adjacency predicate / the connecting `Direction`           |

[DISK_AND_PATH]: k-ring disk, grid distance, line — `H3.Algorithms.Rings` / `Lines` on `this H3Index`

| [INDEX] | [SURFACE]                  | [SHAPE]   | [CAPABILITY]                                                                          |
| :-----: | :------------------------- | :-------- | :------------------------------------------------------------------------------------ |
|  [01]   | `GridRing(int k)`          | extension | pentagon-safe hollow ring at distance k (v4 `gridRing`)                               |
|  [02]   | `GridRingUnsafe(int k)`    | extension | fast hollow ring; throws `HexRing*Exception` near a pentagon (v4 `gridRingUnsafe`)    |
|  [03]   | `GridDiskDistances(int k)` | extension | filled disk → `IEnumerable<RingCell>` with per-cell distance (v4 `gridDiskDistances`) |
|  [04]   | `GridDiskDistancesSafe`    | extension | pentagon-safe filled disk                                                             |
|  [05]   | `GridDiskDistancesUnsafe`  | extension | fast-path filled disk (throws on pentagon)                                            |
|  [06]   | `GridDistance(H3Index)`    | extension | integer grid distance between two same-resolution cells                               |
|  [07]   | `GridPathCells(H3Index)`   | extension | `IEnumerable<H3Index>` straight-line cell path                                        |
|  [08]   | `CellToLocalIj(H3Index)`   | extension | cell → `CoordIJ` in origin's local frame (`H3LocalIJExtensions`)                      |
|  [09]   | `LocalIjToCell(CoordIJ)`   | extension | `CoordIJ` → cell in origin's local frame (`H3LocalIJExtensions`)                      |

`[BUFFER_FILL]`: zero-allocation `Span<T>`-filling overloads returning the count written — `GridDisk` `GridDiskDistances` `GridRingUnsafe` `GridPathCells` `GetChildrenForResolution` `CompactCells` `UncompactCells` `GetCellBoundaryVertices`; presized by `MaxGridDiskSize(k)` `MaxGridRingSize(k)` `GridPathCellsSize(dest)` `CellToChildrenSize(res)` `UncompactCellsSize(cells, res)`. Streaming `IEnumerable` forms stand unchanged and yield identical cells.

[REGION_AND_SET]: polyfill, cell boundary, compaction — `H3.Algorithms.Polyfill` / `H3GeometryExtensions` / `H3SetExtensions`

`GetCellBoundaries`/`CellsToMultiPolygon`/`CompactCells`/`UncompactCells`/`CanonicalizeCells`/`AreOfSameResolution` extend `IEnumerable<H3Index>`, `IsCanonicalCells`/`CanonicalCellsContain` extend `IReadOnlyList<H3Index>`; the rest extend their prefixed receiver.

| [INDEX] | [SURFACE]                                        | [SHAPE]   | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------- | :-------- | :---------------------------------------------------------------- |
|  [01]   | `Geometry.Fill(int, VertexTestMode = Center)`    | extension | NTS polygon/line/point/collection → cell cover (v4 `polygonToCells`) |
|  [02]   | `Geometry.Fill(int, Func<H3Index, bool>)`        | extension | flood fill under a caller containment predicate (non-4326 CRS)     |
|  [03]   | `Geometry.ParallelFill(int, VertexTestMode, int?)` | extension | envelope-sharded concurrent fill; same set as `Fill`, opt-in large |
|  [04]   | `LineString.Fill(int)`                           | extension | NTS polyline → traversed cells                                     |
|  [05]   | `Geometry.IsTransMeridian()`                     | extension | antimeridian-crossing detection before fill                       |
|  [06]   | `H3Index.GetCellBoundary(GeometryFactory?)`      | extension | cell → NTS `Polygon` hex boundary                                 |
|  [07]   | `GetCellBoundaries(GeometryFactory?)`            | extension | cell set → NTS `MultiPolygon`, one hex `Polygon` per cell          |
|  [08]   | `CellsToMultiPolygon(GeometryFactory?)`          | extension | cell set → dissolved outline `MultiPolygon` (v4 `cellsToMultiPolygon`) |
|  [09]   | `CompactCells()`                                 | extension | minimal mixed-resolution covering set (deterministic order)       |
|  [10]   | `UncompactCells(int)`                            | extension | expand a compacted set back to uniform resolution                 |
|  [11]   | `CanonicalizeCells()`                            | extension | cell set → canonical sorted, compacted order (v4 `canonicalizeCells`) |
|  [12]   | `CanonicalCellsContain(H3Index)`                 | extension | binary-search coverage test on a canonical set, no uncompact       |
|  [13]   | `IsCanonicalCells()` / `AreOfSameResolution()`   | extension | canonical-order / uniform-resolution preconditions for set algebra |

- `Geometry.Fill`: seeds from every component of a `MultiPolygon`/`GeometryCollection` and traces the boundary before flooding, so thin/holed/antimeridian and disjoint shapes cover exactly per libh3 `polygonToCells`.
- `GetCellBoundaries`: the caller's `GeometryFactory` flows to each per-cell `Polygon`, not only the outer `MultiPolygon`.
- `CompactCells`: output is deterministically ordered (sorted within each resolution, coarsest last); the sole guarantee remains a minimal covering set.

[EDGE]: directed-edge index algebra — `H3DirectedEdgeExtensions` on `this H3Index` (origin or edge)

| [INDEX] | [SURFACE]                                              | [SHAPE]   | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------------- | :-------- | :------------------------------------------------------------- |
|  [01]   | `ToDirectedEdge(H3Index dest)`                         | extension | directed-edge index between two adjacent cells                 |
|  [02]   | `OriginToDirectedEdges()` / `DestinationToDirectedEdges()` | extension | the (up to six) edges leaving / arriving at a cell          |
|  [03]   | `ReverseDirectedEdge()`                                | extension | the opposite-direction edge (v4 `reverseDirectedEdge`)         |
|  [04]   | `DirectedEdgeToCells()`                                | extension | `(origin, destination)` tuple from an edge index               |
|  [05]   | `GetDirectedEdgeOrigin` / `GetDirectedEdgeDestination` | extension | one endpoint cell of a directed edge                           |
|  [06]   | `IsValidDirectedEdge()`                                | extension | directed-edge validity test                                    |
|  [07]   | `EdgeLengthMeters` / `Kilometers` / `Radians`          | extension | great-circle edge length (`GetExactEdgeLengthInRadians` exact) |
|  [08]   | `GetDirectedEdgeBoundaryVertices()`                    | extension | the edge's `IEnumerable<LatLng>` boundary                      |

- `EdgeLengthMeters`/`Kilometers`/`Radians` and `GetDirectedEdgeBoundaryVertices` throw `ArgumentException` when the index is not a valid directed edge (upstream `E_DIR_EDGE_INVALID`), never silently returning 0 or an empty set.

[AREA_AND_METRIC]: per-cell metrics (`H3GeometryExtensions` on `this H3Index`) and static resolution grid statistics (`H3Index.*`)

| [INDEX] | [SURFACE]                                             | [SHAPE]   | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `CellAreaInMSquared` / `KmSquared` / `RadiansSquared` | extension | exact spherical cell area in the chosen unit          |
|  [02]   | `GetRadiusInKm()` / `GetFaces()`                      | extension | cell circumradius / icosahedron faces a cell touches  |
|  [03]   | `GetCellBoundaryVertices()`                           | extension | raw `IEnumerable<LatLng>` boundary                    |
|  [04]   | `GetNumberOfCells(int)`                               | static    | total cell count at a resolution (v4 `getNumCells`)   |
|  [05]   | `GetRes0Cells()` / `GetPentagons(int)`                | static    | the res-0 cell set / the pentagon cells at a res      |
|  [06]   | `GetHexagonAreaAverageInKmSquared(int)` / `InMSquared` | static   | mean hexagon area at a resolution                     |
|  [07]   | `GetHexagonEdgeLengthAverageInKm(int)` / `InM`        | static    | mean hexagon edge length at a resolution              |

[VERTEX]: topological vertex-mode cells — `H3VertexExtensions` on `this H3Index`

| [INDEX] | [SURFACE]                     | [SHAPE]   | [CAPABILITY]                                                |
| :-----: | :---------------------------- | :-------- | :---------------------------------------------------------- |
|  [01]   | `CellToVertex(int)`           | extension | the `Mode.Vertex` index of a cell's vertex (0..5)           |
|  [02]   | `CellToVertexes()`            | extension | `IEnumerable<H3Index>` of all vertex-mode indices of a cell |
|  [03]   | `VertexToLatLng()`            | extension | vertex-mode index → its `LatLng` corner                     |
|  [04]   | `IsValidVertex()`             | extension | vertex-mode validity test                                   |
|  [05]   | `GetVertexNumberForDirection` | extension | `Direction` → vertex number for a cell                      |
|  [06]   | `GetDirectionForVertexNumber` | extension | vertex number → `Direction` for a cell                      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every surface spells the v4-canonical name: it matches the `h3-pg` SQL function names `Rasm.Persistence` persists against, so a Bim cell id and its server-side counterpart share one function vocabulary. `GridRing` is the pentagon-safe hollow ring (v4 `gridRing`), `GridRingUnsafe` its throwing fast path, `GridDiskDistances` the filled disk-with-distances; `GridDisk` exists only as the `Span<H3Index>` buffer-fill overload.
- `H3Index` is a mutable value `struct`: a `RotateClockwise`/`SetDirectionForResolution` op rewrites only the local copy, `null` is not a cell, and `default(H3Index)` equals `Invalid`, so the durable, hand-off, and cross-thread form is always `(ulong)index` or its hex `ToString()`, never a shared instance.
- `GridDiskDistancesSafe` tolerates pentagon distortion; `GridDiskDistancesUnsafe` throws near a pentagon. Site-region input takes the safe path; the unsafe path serves known-interior bulk fills.
- Cell construction and boundary consume and produce `NetTopologySuite.Geometries` types, so an H3 cell and a `GeoFeature.Geometry` round-trip through one NTS object — never a second coordinate model. `H3.Utils.DefaultGeometryFactory` (SRID 4326, fixed high-precision `PrecisionModel`) backs the `GeometryFactory?`-defaulting overloads; pass the `GeoServices`-resolved factory to match project precision and SRID.
- `(ulong)index` is the durable, resolution-tagged bucket a `GeoModel` keys a coarse DGGS join on beside the `STRtree`: a regional query lowers `Geometry.Fill`/`GridDiskDistances` → a `FrozenSet<ulong>` cover → a membership test, never a per-feature scan. `H3Index.Invalid` (the zero `ulong`) is the sole sentinel — an out-of-range coordinate decodes to it and projects to `Option<H3Index>.None` at the boundary, never a stored `0`.

[STACKING]:
- `h3-pg`(`Rasm.Persistence/.api/api-h3-pg.md`): the same dual central pin computes the identical 64-bit cell in-process here and server-side there (`h3_latlng_to_cell`), so a site context keyed in Bim and persisted through `Rasm.Persistence` agree bit-for-bit; the `Query/lanes#GEO_LANES` `H3Cell.Of(point, res)` and `H3CellOps.Cover`/`Disk`/`Compact` lower `Geometry.Fill`/`GridDiskDistances`/`CompactCells` to the `FrozenSet<ulong>` the `h3_cell = ANY(@cells)` prefilter tests.
- `NetTopologySuite.IO.GeoJSON4STJ`(`.api/api-nts-geojson4stj.md`) / `NetTopologySuite.IO.GeoPackage`(`.api/api-nts-geopackage.md`): a `MultiPolygon` cell-set boundary from `GetCellBoundaries` serializes through the one Bim geo codec — never a hand-spelled GeoJSON document.
- `ProjNET`(`.api/api-projnet.md`): cell construction takes SRID-4326 lat/lng, so a project-CRS `GeoFeature` reprojects through the `Semantics/georeference#GEODETIC_TRANSFORM` `MathTransform` leg into 4326 before `FromPoint`/`Fill` and the boundary reprojects back — the datum bridge is never a hand-rolled great-circle conversion.
- geospatial seam (within-lib): `GeoFeature.Cell` mints `H3Index.FromPoint` over the Wgs84 centroid, `GeoModel.Bucket` keys the DGGS join beside the `STRtree`, and `GeoModel.Cover` lowers a probe region `Geometry.Fill` → `GridDiskDistances` → `CompactCells` into the `FrozenSet<ulong>` region key.

[LOCAL_ADMISSION]:
- `pocketken.H3` admits at the geospatial-seam DGGS keyer arm; a site-context cell enters the repo as the `(ulong)` durable id, never a live `H3Index` instance.

[RAIL_LAW]:
- Package: `pocketken.H3`
- Owns: managed Uber-H3 v4 cell indexing — hierarchy/disk/path/region algebra, directed-edge and vertex-mode topology, cell-area/edge-length metrics, the NTS-geometry ↔ cell bridge, and the in-process DGGS site-context key
- Accept: the `H3Index` `ulong` durable key (two-way implicit conversion), the v4-canonical spelling, NTS `Point`/`Geometry` at the boundary, the `GeoServices`-resolved `GeometryFactory`
- Reject: storing a mutable `H3Index` value instead of its `(ulong)` id, the `Invalid` sentinel crossing the boundary unwrapped, a second coordinate model beside the Bim NTS stack, hand-rolled great-circle/area math the `H3GeometryExtensions` metrics own
