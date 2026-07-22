# [RASM_PERSISTENCE_API_H3]

`pocketken.H3` owns managed Uber-H3 hexagonal hierarchical indexing: one 64-bit `H3Index` computed in process, bit-identical to the cell `h3-pg` computes server-side, so ingest and database indexing share one cell vocabulary. That ulong is the resolution-tagged spatial key `Element/identity` stores and every spatial predicate keysets on. Geometry crosses at `NetTopologySuite`, the same currency the PostGIS binding already speaks, so a cell column and a geometry column project one coordinate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pocketken.H3`
- package: `pocketken.H3` (Apache-2.0, pocketken/H3.net)
- assembly: `pocketken.H3`
- namespace: `H3`, `H3.Model`, `H3.Extensions`, `H3.Algorithms`
- asset: pure-managed AnyCPU IL — a C# port of the Uber-H3 algorithm carrying no native binary
- depends: `NetTopologySuite` alone; every geometry overload crosses at that object model
- rail: geospatial-index

## [02]-[PUBLIC_TYPES]

[CELL_TYPE_SCOPE]: the cell index, its decoded bit layout, and the kernels the algorithms fold through

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `H3Index : IComparable<H3Index>`                   | class         | 64-bit cell id over a mutable `ulong`; `Invalid` is the zero |
|  [02]   | `H3.Model.Mode`                                    | enum          | `Unknown` `Cell` `UniEdge` `Vertex` kind in the high bits    |
|  [03]   | `H3.Model.Direction`                               | enum          | IJK step `Center` `K` `J` `JK` `I` `IK` `IJ` `Invalid`       |
|  [04]   | `H3.Model.LatLng`                                  | class         | radian pair with degree projections and great-circle metrics |
|  [05]   | `H3.Model.CoordIJ`                                 | class         | local IJ coordinate for grid-distance and line algebra       |
|  [06]   | `H3.Model.BaseCell`                                | class         | one of 122 base cells; home face, pentagon and rotation data |
|  [07]   | `H3.Constants`                                     | class         | resolution caps, `EARTH_RADIUS_KM`, hexagon geometry values  |
|  [08]   | `H3.Utils`                                         | class         | `DefaultGeometryFactory` and the radian trigonometry kernel  |
|  [09]   | `H3.H3IndexJsonConverter : JsonConverter<H3Index>` | class         | hex-string round-trip, attribute-bound on `H3Index`          |
|  [10]   | `H3.Algorithms.RingCell`                           | struct        | disk member carrying `Index` and `Distance`                  |
|  [11]   | `H3.Algorithms.VertexTestMode`                     | enum          | `Center` `Any` `All` polyfill cell-acceptance predicate      |

[DISK_FAULT]: `H3.Algorithms.HexRingPentagonException` `H3.Algorithms.HexRingKSequenceException`

[H3INDEX_DECODE]: `Resolution` `Mode` `BaseCell` `BaseCellNumber` `Direction` `IsValidCell` `IsPentagon` `LeadingNonZeroDirection` `MaximumFaceCount` `ReservedBits` `HighBit`

## [03]-[ENTRYPOINTS]

[COORDINATE_ENTRY_SCOPE]: coordinate and stored-key admission with the inverse projections

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `H3Index.FromLatLng(LatLng, int)`            | factory  | radian pair admitted at a resolution           |
|  [02]   | `H3Index.FromPoint(Point, int)`              | factory  | NTS `Point` at SRID 4326 admitted as a cell    |
|  [03]   | `Coordinate.ToH3Index(int)`                  | instance | NTS `Coordinate` admitted as a cell            |
|  [04]   | `H3Index.Create(int, int, Direction)`        | factory  | explicit resolution, base cell, lead direction |
|  [05]   | `H3Index.ToLatLng() -> LatLng`               | instance | cell centroid as a radian pair                 |
|  [06]   | `H3Index.ToPoint(GeometryFactory?) -> Point` | instance | cell centroid as an NTS `Point`                |
|  [07]   | `H3Index.ToCoordinate() -> Coordinate`       | instance | cell centroid as an NTS `Coordinate`           |

[CELL_KEY_BRIDGE]: `new H3Index(ulong)` `new H3Index(string)` `H3Index.ToString()` `implicit operator ulong(H3Index)` `implicit operator H3Index(ulong)` `H3Index.CompareTo(H3Index?)`

[LATLNG_BRIDGE]: `LatLng.FromPoint` `LatLng.FromCoordinate` `LatLng.ToPoint` `LatLng.ToCoordinate` `LatitudeDegrees` `LongitudeDegrees` `implicit operator LatLng((double, double))`

[LATLNG_METRIC]: `GetGreatCircleDistanceInRadians` `GetGreatCircleDistanceInKm` `GetGreatCircleDistanceInMeters` `GetAzimuthInRadians` `ForAzimuthDistanceInRadians` `GetTriangleArea` `LineHexEstimate` `AlmostEquals` `AlmostEqualsThreshold`

[TRAVERSAL_ENTRY_SCOPE]: hierarchy, adjacency, disk, path, and the origin-local IJ frame, every surface extending `H3Index`

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `GetParentForResolution(int)`                           | instance | coarser ancestor cell                       |
|  [02]   | `GetChildCenterForResolution(int)`                      | instance | center child at a finer resolution          |
|  [03]   | `GetChildrenForResolution(int) -> IEnumerable<H3Index>` | instance | every finer child                           |
|  [04]   | `GetDirectChild(Direction)`                             | instance | one immediate child by direction            |
|  [05]   | `Contains(H3Index) -> bool`                             | instance | hierarchical containment prefilter          |
|  [06]   | `ContainedBy(H3Index) -> bool`                          | instance | inverse containment test                    |
|  [07]   | `GetNeighbours() -> IEnumerable<H3Index>`               | instance | same-resolution neighbour cells             |
|  [08]   | `GetDirectNeighbour(Direction, int) -> (H3Index, int)`  | instance | one neighbour with its rotation count       |
|  [09]   | `IsNeighbour(H3Index) -> bool`                          | instance | adjacency predicate                         |
|  [10]   | `DirectionForNeighbour(H3Index) -> Direction`           | instance | connecting direction between neighbours     |
|  [11]   | `GridRing(int) -> IEnumerable<H3Index>`                 | instance | hollow ring at exactly k                    |
|  [12]   | `GridDiskDistances(int) -> IEnumerable<RingCell>`       | instance | filled disk carrying per-cell distance      |
|  [13]   | `GridDiskDistancesSafe(int)`                            | instance | pentagon-tolerant filled disk               |
|  [14]   | `GridDiskDistancesUnsafe(int)`                          | instance | fast filled disk over known interior        |
|  [15]   | `GridDistance(H3Index) -> int`                          | instance | integer grid distance at one resolution     |
|  [16]   | `GridPathCells(H3Index) -> IEnumerable<H3Index>`        | instance | straight-line cell path                     |
|  [17]   | `CellToLocalIj(H3Index) -> CoordIJ`                     | instance | cell read in the origin's local IJ frame    |
|  [18]   | `LocalIjToCell(CoordIJ) -> H3Index`                     | instance | local IJ coordinate resolved back to a cell |

[REGION_ENTRY_SCOPE]: polygon and polyline fill, cell-boundary geometry, and the mixed-resolution cover algebra

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `Geometry.Fill(int, VertexTestMode)`                       | instance | polygon covered under the cell-acceptance mode  |
|  [02]   | `LineString.Fill(int)`                                     | instance | polyline traversed as cells                     |
|  [03]   | `Coordinate[].TraceCoordinates(int)`                       | instance | raw coordinate run traced as cells              |
|  [04]   | `Geometry.IsTransMeridian() -> bool`                       | instance | antimeridian crossing detected ahead of a fill  |
|  [05]   | `H3Index.GetCellBoundary(GeometryFactory?)`                | instance | cell boundary as an NTS `Polygon`               |
|  [06]   | `H3Index.GetCellBoundaryVertices()`                        | instance | boundary corners as `IEnumerable<LatLng>`       |
|  [07]   | `IEnumerable<H3Index>.CompactCells()`                      | fold     | minimal mixed-resolution cover                  |
|  [08]   | `IEnumerable<H3Index>.UncompactCells(int)`                 | fold     | cover expanded to a uniform resolution          |
|  [09]   | `IEnumerable<H3Index>.UncompactCellsToHighestResolution()` | fold     | cover expanded to its finest member resolution  |
|  [10]   | `IEnumerable<H3Index>.AreOfSameResolution() -> bool`       | fold     | uniform-resolution precondition for set algebra |
|  [11]   | `IEnumerable<H3Index>.GetCellBoundaries(GeometryFactory?)` | fold     | cover as one NTS `MultiPolygon`                 |

[TOPOLOGY_ENTRY_SCOPE]: directed-edge and vertex-mode indices, every surface extending `H3Index`

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------------- |
|  [01]   | `ToDirectedEdge(H3Index) -> H3Index`              | instance | directed edge between adjacent cells         |
|  [02]   | `OriginToDirectedEdges() -> IEnumerable<H3Index>` | instance | up to six edges leaving a cell               |
|  [03]   | `DirectedEdgeToCells() -> (H3Index, H3Index)`     | instance | origin and destination of an edge            |
|  [04]   | `GetDirectedEdgeOrigin() -> H3Index`              | instance | origin endpoint of an edge                   |
|  [05]   | `GetDirectedEdgeDestination() -> H3Index`         | instance | destination endpoint of an edge              |
|  [06]   | `IsValidDirectedEdge() -> bool`                   | instance | directed-edge validity test                  |
|  [07]   | `GetDirectedEdgeBoundaryVertices()`               | instance | edge corners as `IEnumerable<LatLng>`        |
|  [08]   | `CellToVertex(int) -> H3Index`                    | instance | `Mode.Vertex` index of one cell corner       |
|  [09]   | `CellToVertexes() -> IEnumerable<H3Index>`        | instance | every vertex-mode index of a cell            |
|  [10]   | `VertexToLatLng() -> LatLng`                      | instance | vertex index as its shared corner coordinate |
|  [11]   | `IsValidVertex() -> bool`                         | instance | vertex-mode validity test                    |

[VERTEX_DIRECTION]: `GetVertexNumberForDirection(Direction) -> int` `GetDirectionForVertexNumber(int) -> Direction`

[CELL_METRIC]: `CellAreaInMSquared` `CellAreaInKmSquared` `CellAreaInRadiansSquared` `EdgeLengthMeters` `EdgeLengthKilometers` `EdgeLengthRadians` `GetRadiusInKm` `GetFaces`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `H3Index` wraps a mutable `ulong` — `RotateClockwise` and `SetDirectionForResolution` rewrite the receiver — so `(ulong)index` or its hex `ToString()` is the durable form and every store, fold, and cross-thread hand-off carries that value.
- Each traversal, set, edge, and vertex member spells its `h3_*` SQL counterpart verbatim minus prefix and casing, so a managed call site and its server-side twin read as one name.
- `GridDiskDistances` runs the unsafe walk and falls back to the pentagon-tolerant walk on throw, making it the disk entry every caller takes; `GridDiskDistancesUnsafe` raises `HexRingPentagonException`, `GridRing` raises that and `HexRingKSequenceException`, and both serve known-interior bulk fills.
- `H3.Utils.DefaultGeometryFactory` fixes `PrecisionModel(1e16)` at SRID 4326 on every geometry overload; an explicit `GeometryFactory` enters only to match a different SRID, and the factory mints every `Coordinate`.
- `H3Index` carries `[JsonConverter(typeof(H3IndexJsonConverter))]`, so System.Text.Json binds the hex round-trip with no registration; its read accepts a 15-character hex string, and a mode-2 edge or mode-4 vertex index (16 hex characters) crosses JSON as its `ulong`.
- `H3Index.Invalid` is the sole absent-cell sentinel: an unresolvable coordinate yields the zero cell, `Element/identity` lifts it to a typed `IdentityFault.CellUnresolvable`, and the stored column carries admitted cells alone.

[STACKING]:
- `api-h3-pg`(`.api/api-h3-pg.md`): `H3Index.FromPoint(point, res)` at ingest and `h3_latlng_to_cell` server-side return the identical 64-bit value, so the `h3_cell` generated column and the managed key agree bit-for-bit; `CompactCells` collapses a managed cover before it lands as the `h3_cell = ANY(@cells)` btree/brin membership test.
- `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite.md`): `Point`, `Polygon`, `MultiPolygon`, `LineString`, `Coordinate`, and `GeometryFactory` are the whole geometry vocabulary of this surface, so a cell and a PostGIS geometry column round-trip through one object model with no second coordinate type.
- `api-npgsql-nts`(`.api/api-npgsql-nts.md`): the codec `UseNetTopologySuite` admits binds the same `Point` instance `H3Index.FromPoint` consumes, so a row's geometry parameter and its cell derive from one materialized geometry.
- `api-nts-io`(`.api/api-nts-io.md`): `GetCellBoundaries` returns one `MultiPolygon` per cover, the shape the `GeoJSON4STJ` writer serializes as a single GeoJSON document and the GeoPackage writer stores as one blob.
- `api-pgrouting`(`.api/api-pgrouting.md`): `GridPathCells` and `GridDistance` mint cell-id nodes and edge weights over the same id space `pgr_dijkstra` traverses in `Query/cypher`, so in-process and in-database routing share one node vocabulary.
- within-lib: one pipeline folds `Fill` or `GridDiskDistancesSafe` through `CompactCells` into an `H3Cell[]` cover — `Element/identity` wraps each result as `H3Cell.Of(index)` and widens a radius through `Nearby`, and the `Query/lane` `SetPredicate.Spatial` leg runs its `Geometry` refine only over rows the cell-set membership test survived.

[LOCAL_ADMISSION]:
- Cell computation enters at `Ingest/geospatial` and `Element/identity`; a spatial predicate lowers to a cell set ahead of any geometry test, and the persisted column carries the `H3Cell` ulong the identity Key axis owns.

[RAIL_LAW]:
- Package: `pocketken.H3`
- Owns: managed cell indexing over the NTS coordinate bridge — hierarchy, disk, path, region fill, cover algebra, directed-edge and vertex topology, and the spherical area and length metrics
- Accept: the cell ulong or its hex text as the durable key, NTS geometry at every boundary, `Utils.DefaultGeometryFactory` at SRID 4326, `GridDiskDistances` as the disk walk
- Reject: a live `H3Index` instance held across a store or a fold, a second coordinate model beside NTS, hand-rolled great-circle distance, azimuth, or spherical area the `LatLng` and cell metrics own, a per-row `ST_DWithin` scan where a cell-set membership test serves
