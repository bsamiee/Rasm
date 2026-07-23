# [RASM_API_NETTOPOLOGYSUITE]

`NetTopologySuite` owns the OGC Simple Features algebra on the double-precision production plane: geometry representation, DE-9IM topology, robust overlay, spatial indexing, validity repair, and the text and binary geometry codecs every geospatial, persistence, and circulation boundary crosses. Kernel `Rasm` keeps exact-predicate geometry; this rail carries everything downstream of that conversion. `NetTopologySuite.Features` couples one geometry to its attribute table as the vector-codec row shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite`
- package: `NetTopologySuite` (BSD-3-Clause)
- assembly: `NetTopologySuite`
- namespace: `NetTopologySuite`, `.Geometries`, `.Geometries.Prepared`, `.Geometries.Implementation`, `.Geometries.Utilities`, `.Algorithm`, `.Algorithm.Hull`, `.Operation.OverlayNG`, `.Operation.Union`, `.Operation.Buffer`, `.Operation.Distance`, `.Operation.Valid`, `.Operation.Polygonize`, `.Operation.Linemerge`, `.Triangulate`, `.LinearReferencing`, `.Coverage`, `.Dissolve`, `.Precision`, `.Simplify`, `.Densify`, `.Index`, `.Index.Strtree`, `.Index.Quadtree`, `.Index.HPRtree`, `.IO`
- asset: IL-only AnyCPU managed JTS port; no P/Invoke and no native GEOS binding
- rail: geometry

[PACKAGE_SURFACE]: `NetTopologySuite.Features`
- package: `NetTopologySuite.Features` (BSD-3-Clause)
- assembly: `NetTopologySuite.Features`
- namespace: `NetTopologySuite.Features`
- asset: IL-only AnyCPU managed assembly over the core geometry algebra
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry hierarchy and coordinate model

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CAPABILITY]                |
| :-----: | :-------------------------------- | :------------- | :-------------------------- |
|  [01]   | `Geometry`                        | abstract class | planar geometry algebra     |
|  [02]   | `Point`                           | class          | point position              |
|  [03]   | `LineString`                      | class          | linear curve                |
|  [04]   | `LinearRing`                      | class          | closed ring, shell or hole  |
|  [05]   | `Polygon`                         | class          | areal boundary              |
|  [06]   | `MultiPoint`                      | class          | homogeneous points          |
|  [07]   | `MultiLineString`                 | class          | homogeneous curves          |
|  [08]   | `MultiPolygon`                    | class          | homogeneous polygons        |
|  [09]   | `GeometryCollection`              | class          | mixed-dimension members     |
|  [10]   | `Coordinate`                      | class          | XY ordinate tuple           |
|  [11]   | `CoordinateZ`                     | class          | XYZ ordinate tuple          |
|  [12]   | `CoordinateM`                     | class          | XYM ordinate tuple          |
|  [13]   | `CoordinateZM`                    | class          | XYZM ordinate tuple         |
|  [14]   | `CoordinateSequence`              | abstract class | packed ordinate store       |
|  [15]   | `Envelope`                        | class          | axis-aligned 2D extent      |
|  [16]   | `IntersectionMatrix`              | class          | DE-9IM cell matrix          |
|  [17]   | `OgcGeometryType`                 | enum           | geometry discriminant       |
|  [18]   | `Dimension`                       | enum           | DE-9IM cell vocabulary      |
|  [19]   | `Ordinates`                       | flags enum     | ordinate-set mask           |
|  [20]   | `ICoordinateSequenceFilter`       | interface      | per-vertex rewrite walk     |
|  [21]   | `IEntireCoordinateSequenceFilter` | interface      | whole-sequence rewrite walk |

[`Geometry`]: `Factory` `SRID` `UserData` `GeometryType` `OgcGeometryType` `Dimension` `BoundaryDimension` `Boundary` `Envelope` `EnvelopeInternal` `Coordinate` `Coordinates` `NumPoints` `NumGeometries` `GetGeometryN` `GetOrdinates` `Area` `Length` `Centroid` `InteriorPoint` `PointOnSurface` `IsValid` `IsSimple` `IsEmpty` `IsRectangle` `HasDimension` `Copy` `Reverse` `Normalized`
[`Point`]: `X` `Y` `Z` `M` `Coordinate` `CoordinateSequence`
[`LineString`]: `StartPoint` `EndPoint` `GetPointN` `GetCoordinateN` `Count` `IsClosed` `IsRing` `CoordinateSequence`
[`Polygon`]: `Shell` `Holes` `ExteriorRing` `InteriorRings` `GetInteriorRingN` `NumInteriorRings`
[`GeometryCollection`]: `Geometries` `IsHomogeneous` `GetGeometryN` `NumGeometries`
[`CoordinateSequence`]: `Count` `Dimension` `Measures` `Ordinates` `HasZ` `HasM` `GetX` `GetY` `GetZ` `GetM` `GetOrdinate` `SetOrdinate` `SetX` `SetY` `SetZ` `GetCoordinate` `GetCoordinateCopy` `ToCoordinateArray` `ExpandEnvelope` `Reversed` `Copy`
[`Envelope`]: `MinX` `MaxX` `MinY` `MaxY` `Width` `Height` `Area` `Diameter` `Centre` `IsNull` `Intersects` `Contains` `Covers` `Disjoint` `Distance` `Intersection` `ExpandToInclude` `ExpandBy` `ExpandedBy` `Translate` `Copy`
[`IntersectionMatrix`]: `Get` `Set` `SetAtLeast` `SetAll` `Add` `Matches` `Transpose` `IsDisjoint` `IsIntersects` `IsWithin` `IsContains` `IsCovers` `IsCoveredBy` `IsTouches` `IsCrosses` `IsOverlaps` `IsEquals`
[`ICoordinateSequenceFilter`]: `Filter` `Done` `GeometryChanged`

- `Geometry.Coordinates`: materializes a fresh array per call and never aliases the sequence store, so a representation-independent ordinate rewrite goes through `Geometry.Apply` instead.

[PUBLIC_TYPE_SCOPE]: construction policy and ordinate storage

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :-------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `NtsGeometryServices`             | class          | process-wide engine and factory policy |
|  [02]   | `GeometryFactory`                 | class          | geometry construction                  |
|  [03]   | `GeometryOverlay`                 | abstract class | overlay engine selector                |
|  [04]   | `GeometryRelate`                  | abstract class | relate engine selector                 |
|  [05]   | `PrecisionModel`                  | class          | coordinate rounding policy             |
|  [06]   | `PrecisionModels`                 | enum           | precision mode vocabulary              |
|  [07]   | `CoordinateSequenceFactory`       | abstract class | ordinate-store strategy                |
|  [08]   | `CoordinateArraySequenceFactory`  | class          | `Coordinate[]` store                   |
|  [09]   | `PackedCoordinateSequenceFactory` | class          | interleaved double or float store      |
|  [10]   | `PackedDoubleCoordinateSequence`  | class          | raw `double[]` interleaved store       |
|  [11]   | `RawCoordinateSequenceFactory`    | sealed class   | caller-owned column store              |
|  [12]   | `RawCoordinateSequence`           | sealed class   | strided `Memory<double>` columns       |

[`NtsGeometryServices`]: `GeometryOverlay` `GeometryRelate` `CoordinateEqualityComparer` `DefaultSRID` `DefaultPrecisionModel` `DefaultCoordinateSequenceFactory`
[`GeometryFactory`]: `PrecisionModel` `SRID` `CoordinateSequenceFactory` `GeometryServices` `ToGeometryArray` `ToPolygonArray` `ToLineStringArray` `ToPointArray`
[`PrecisionModel`]: `Floating` `FloatingSingle` `Fixed` `Scale` `GridSize` `IsFloating` `MaximumSignificantDigits` `PrecisionModelType` `MakePrecise` `MostPrecise`
[`PrecisionModels`]: `Floating` `FloatingSingle` `Fixed`
[`PackedCoordinateSequenceFactory`]: `DoubleFactory` `FloatFactory` `Type`

- `PrecisionModel.Floating`, `.FloatingSingle`, and `.Fixed` are `Lazy<PrecisionModel>` statics; `.Value` unwraps each.

[PUBLIC_TYPE_SCOPE]: acceleration, repair, transform, and operation policy

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------- | :------------ | :---------------------------------- |
|  [01]   | `PreparedGeometryFactory`  | class         | predicate preparation               |
|  [02]   | `IPreparedGeometry`        | interface     | segment-indexed predicates          |
|  [03]   | `STRtree<TItem>`           | class         | bulk-loaded R-tree                  |
|  [04]   | `Quadtree<TItem>`          | class         | mutation-tolerant quadtree          |
|  [05]   | `HPRtree<TItem>`           | class         | Hilbert-packed static R-tree        |
|  [06]   | `IItemVisitor<TItem>`      | interface     | streaming query sink                |
|  [07]   | `IItemDistance`            | interface     | nearest-neighbour metric            |
|  [08]   | `GeometryItemDistance`     | class         | envelope-to-geometry metric         |
|  [09]   | `GeometryFixer`            | class         | validity repair                     |
|  [10]   | `IsValidOp`                | class         | validity adjudication               |
|  [11]   | `TopologyValidationError`  | class         | validity failure detail             |
|  [12]   | `TopologyValidationErrors` | enum          | validity failure vocabulary         |
|  [13]   | `AffineTransformation`     | class         | planar affine map and vertex filter |
|  [14]   | `GeometryEditor`           | class         | structure-preserving edit           |
|  [15]   | `BufferParameters`         | class         | offset join, cap, and side policy   |
|  [16]   | `EndCapStyle`              | enum          | line end cap                        |
|  [17]   | `JoinStyle`                | enum          | corner join                         |
|  [18]   | `SpatialFunction`          | enum          | overlay operation selector          |
|  [19]   | `ByteOrder`                | enum          | WKB endianness                      |

[`IPreparedGeometry`]: `Geometry` `Intersects` `Disjoint` `Contains` `ContainsProperly` `Within` `Covers` `CoveredBy` `Crosses` `Overlaps` `Touches`
[`AffineTransformation`]: `TranslationInstance` `RotationInstance` `ScaleInstance` `ShearInstance` `ReflectionInstance` `Translate` `Rotate` `Scale` `Shear` `Reflect` `Compose` `ComposeBefore` `GetInverse` `Determinant` `MatrixEntries` `IsIdentity`
[`GeometryFixer`]: `KeepCollapsed` `KeepMulti` `GetResult`
[`IsValidOp`]: `IsValid` `ValidationError` `IsSelfTouchingRingFormingHoleValid`
[`BufferParameters`]: `QuadrantSegments` `EndCapStyle` `JoinStyle` `MitreLimit` `IsSingleSided` `SimplifyFactor` `Copy`
[`EndCapStyle`]: `Round` `Flat` `Square`
[`JoinStyle`]: `Round` `Mitre` `Bevel`
[`SpatialFunction`]: `Intersection` `Union` `Difference` `SymDifference`

- `AffineTransformation` implements `ICoordinateSequenceFilter`, so one instance either maps a whole geometry or rides a filter walk over a caller-owned sequence.

[PUBLIC_TYPE_SCOPE]: attributed-feature shape

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :------------------ | :------------ | :------------------ |
|  [01]   | `IFeature`          | interface     | attributed geometry |
|  [02]   | `Feature`           | sealed class  | mutable feature     |
|  [03]   | `FeatureCollection` | sealed class  | ordered feature set |
|  [04]   | `IAttributesTable`  | interface     | attribute contract  |
|  [05]   | `AttributesTable`   | sealed class  | attribute storage   |

[`IFeature`]: `Geometry` `Attributes` `BoundingBox`
[`AttributesTable`]: `Count` `this[string]` `GetNames` `GetValues` `GetType` `Exists` `Add` `DeleteAttribute` `MergeWith` `GetOptionalValue`

- `Feature.BoundingBox` is independently assignable and never recomputed from `Geometry` on read.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: predicates, relate, and distance

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `Geometry.Relate(Geometry) -> IntersectionMatrix`                | instance | full DE-9IM matrix               |
|  [02]   | `Geometry.Relate(Geometry, string) -> bool`                      | instance | DE-9IM mask test                 |
|  [03]   | `Geometry.EqualsTopologically(Geometry) -> bool`                 | instance | topological equality             |
|  [04]   | `Geometry.EqualsExact(Geometry, double) -> bool`                 | instance | vertexwise equality in tolerance |
|  [05]   | `Geometry.Distance(Geometry) -> double`                          | instance | minimum separation               |
|  [06]   | `Geometry.IsWithinDistance(Geometry, double) -> bool`            | instance | bounded proximity, early exit    |
|  [07]   | `DistanceOp.NearestPoints(Geometry, Geometry) -> Coordinate[]`   | static   | clash-gap witness pair           |
|  [08]   | `DistanceOp.NearestLocations() -> GeometryLocation[]`            | instance | witness with component index     |
|  [09]   | `PreparedGeometryFactory.Prepare(Geometry) -> IPreparedGeometry` | static   | build the segment index          |

- Predicate family: `Geometry.Intersects` `Disjoint` `Contains` `Within` `Covers` `CoveredBy` `Crosses` `Overlaps` `Touches` each take `(Geometry)` and return `bool`; `IPreparedGeometry` mirrors the set and adds `ContainsProperly`.

[ENTRYPOINT_SCOPE]: construction and factory policy

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `NtsGeometryServices.Instance`                                   | property | process-wide policy holder          |
|  [02]   | `NtsGeometryServices.CreateGeometryFactory(int)`                 | instance | cached factory for one SRID         |
|  [03]   | `NtsGeometryServices.CreatePrecisionModel(double)`               | instance | fixed model at one scale            |
|  [04]   | `GeometryFactory.CreatePolygon(LinearRing, LinearRing[])`        | instance | shell with holes                    |
|  [05]   | `GeometryFactory.CreateMultiPointFromCoords(Coordinate[])`       | instance | point set from ordinates            |
|  [06]   | `GeometryFactory.BuildGeometry(IEnumerable<Geometry>)`           | instance | narrowest type covering the set     |
|  [07]   | `GeometryFactory.ToGeometry(Envelope) -> Geometry`               | instance | extent to polygon                   |
|  [08]   | `GeometryFactory.CreateEmpty(Dimension)`                         | instance | empty geometry of one dimension     |
|  [09]   | `GeometryFactory.WithSRID(int) -> GeometryFactory`               | instance | same policy under another SRID      |
|  [10]   | `PackedCoordinateSequenceFactory.Create(double[], int, int)`     | instance | sequence over an interleaved buffer |
|  [11]   | `PackedDoubleCoordinateSequence.GetRawCoordinates() -> double[]` | instance | the interleaved backing store       |
|  [12]   | `RawCoordinateSequenceFactory.CreateXYZ(Memory<double>)`         | static   | sequence over a caller-owned buffer |
|  [13]   | `RawCoordinateSequence.GetRawCoordinatesAndStride(int)`          | instance | one ordinate column with its stride |

- Create family: `GeometryFactory.CreatePoint(Coordinate)` `CreateLineString(Coordinate[])` `CreateLinearRing(Coordinate[])` `CreateMultiPoint(Point[])` `CreateMultiLineString(LineString[])` `CreateMultiPolygon(Polygon[])` `CreateGeometryCollection(Geometry[])` each return their named type.

[ENTRYPOINT_SCOPE]: overlay, buffer, and hull

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `Geometry.Intersection(Geometry) -> Geometry`                             | instance | overlay intersection                  |
|  [02]   | `Geometry.Union(Geometry) -> Geometry`                                    | instance | overlay union                         |
|  [03]   | `Geometry.Union() -> Geometry`                                            | instance | collection self-union                 |
|  [04]   | `Geometry.Difference(Geometry) -> Geometry`                               | instance | overlay difference                    |
|  [05]   | `Geometry.SymmetricDifference(Geometry) -> Geometry`                      | instance | symmetric difference                  |
|  [06]   | `Geometry.Buffer(double, int, EndCapStyle) -> Geometry`                   | instance | offset at segment and cap policy      |
|  [07]   | `Geometry.Buffer(double, BufferParameters) -> Geometry`                   | instance | offset under full policy              |
|  [08]   | `Geometry.ConvexHull() -> Geometry`                                       | instance | convex enclosure                      |
|  [09]   | `OverlayNGRobust.Overlay(Geometry, Geometry, SpatialFunction)`            | static   | robust binary overlay                 |
|  [10]   | `OverlayNGRobust.OverlaySR(Geometry, Geometry, SpatialFunction)`          | static   | snap-rounded attempt, null on failure |
|  [11]   | `OverlayNGRobust.Union(IEnumerable<Geometry>, GeometryFactory)`           | static   | robust bulk union                     |
|  [12]   | `UnaryUnionOp.Union(Geometry) -> Geometry`                                | static   | cascaded self-union                   |
|  [13]   | `CoverageUnion.Union(Geometry[]) -> Geometry`                             | static   | edge-matched coverage union           |
|  [14]   | `OffsetCurve.GetCurve(Geometry, double, int, JoinStyle, double)`          | static   | single-sided offset curve             |
|  [15]   | `OffsetCurve.GetCurveJoined(Geometry, double) -> Geometry`                | static   | joined two-sided offset curve         |
|  [16]   | `ConcaveHull.ConcaveHullByLength(Geometry, double, bool)`                 | static   | concave hull at a max edge length     |
|  [17]   | `ConcaveHull.ConcaveHullByLengthRatio(Geometry, double, bool)`            | static   | scale-free concave hull               |
|  [18]   | `ConcaveHull.AlphaShape(Geometry, double, bool)`                          | static   | alpha shape                           |
|  [19]   | `ConcaveHullOfPolygons.ConcaveHullByLength(Geometry, double, bool, bool)` | static   | hull over a polygon set               |
|  [20]   | `ConcaveHullOfPolygons.ConcaveFillByLength(Geometry, double)`             | static   | gap fill between polygons             |
|  [21]   | `MinimumDiameter.GetMinimumRectangle(Geometry) -> Geometry`               | static   | minimum-area oriented rectangle       |
|  [22]   | `MinimumDiameter.GetMinimumDiameter(Geometry) -> Geometry`                | static   | minimum width segment                 |
|  [23]   | `MinimumBoundingCircle.GetCircle() -> Geometry`                           | instance | smallest enclosing circle             |

[`MinimumBoundingCircle`]: `GetCentre` `GetRadius` `GetDiameter` `GetMaximumDiameter` `GetFarthestPoints` `GetExtremalPoints`
[`ConcaveHull`]: `MaximumEdgeLength` `MaximumEdgeLengthRatio` `Alpha` `HolesAllowed` `GetHull` `UniformGridEdgeLength`

[ENTRYPOINT_SCOPE]: simplify, reduce, repair, and coverage

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `DouglasPeuckerSimplifier.Simplify(Geometry, double)`                    | static  | vertex reduction, rings may cross |
|  [02]   | `TopologyPreservingSimplifier.Simplify(Geometry, double)`                | static  | topology-safe reduction           |
|  [03]   | `PolygonHullSimplifier.Hull(Geometry, bool, double)`                     | static  | inner or outer vertex-count hull  |
|  [04]   | `PolygonHullSimplifier.HullByAreaDelta(Geometry, bool, double)`          | static  | hull at an area-delta budget      |
|  [05]   | `Densifier.Densify(Geometry, double) -> Geometry`                        | static  | bounded segment length            |
|  [06]   | `GeometryPrecisionReducer.Reduce(Geometry, PrecisionModel)`              | static  | snap ordinates and re-node        |
|  [07]   | `GeometryPrecisionReducer.ReduceKeepCollapsed(Geometry, PrecisionModel)` | static  | reduce, retaining collapses       |
|  [08]   | `GeometryPrecisionReducer.ReducePointwise(Geometry, PrecisionModel)`     | static  | snap ordinates without re-noding  |
|  [09]   | `GeometryFixer.Fix(Geometry, bool) -> Geometry`                          | static  | repair invalid geometry           |
|  [10]   | `IsValidOp.CheckValid(Geometry) -> bool`                                 | static  | validity verdict                  |
|  [11]   | `CoverageValidator.Validate(Geometry[], double) -> Geometry[]`           | static  | per-polygon invalid-edge report   |
|  [12]   | `CoverageSimplifier.Simplify(Geometry[], double) -> Geometry[]`          | static  | edge-matched coverage simplify    |
|  [13]   | `CoverageSimplifier.SimplifyInner(Geometry[], double)`                   | static  | inner edges only, outline fixed   |
|  [14]   | `LineDissolver.Dissolve(Geometry) -> Geometry`                           | static  | collapse duplicate linework       |

[ENTRYPOINT_SCOPE]: topology assembly, triangulation, and linear referencing

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Polygonizer.Add(Geometry)`                                            | instance | contribute linework                 |
|  [02]   | `Polygonizer.GetPolygons() -> ICollection<Geometry>`                   | instance | polygons formed from noded linework |
|  [03]   | `LineMerger.GetMergedLineStrings() -> IList<Geometry>`                 | instance | merge lines across degree-2 nodes   |
|  [04]   | `LineSequencer.GetSequencedLineStrings() -> Geometry`                  | instance | order lines into one traversal      |
|  [05]   | `DelaunayTriangulationBuilder.GetTriangles(GeometryFactory)`           | instance | Delaunay triangle collection        |
|  [06]   | `DelaunayTriangulationBuilder.GetEdges(GeometryFactory)`               | instance | triangulation edges                 |
|  [07]   | `VoronoiDiagramBuilder.GetDiagram(GeometryFactory)`                    | instance | Voronoi cells, medial-axis input    |
|  [08]   | `ConformingDelaunayTriangulationBuilder.GetTriangles(GeometryFactory)` | instance | constraint-conforming triangles     |
|  [09]   | `PolygonTriangulator.Triangulate(Geometry) -> Geometry`                | static   | ear-clipped polygon triangles       |
|  [10]   | `LengthIndexedLine.ExtractPoint(double, double) -> Coordinate`         | instance | station and offset to a point       |
|  [11]   | `LengthIndexedLine.ExtractLine(double, double) -> Geometry`            | instance | sub-line between two stations       |
|  [12]   | `LengthIndexedLine.IndexOf(Coordinate) -> double`                      | instance | station of a point on the line      |
|  [13]   | `LengthIndexedLine.Project(Coordinate) -> double`                      | instance | station nearest an off-line point   |
|  [14]   | `LocationIndexedLine.ExtractLine(LinearLocation, LinearLocation)`      | instance | sub-line by component and fraction  |

[`Polygonizer`]: `GetGeometry` `GetDangles` `GetCutEdges` `GetInvalidRingLines` `IsCheckingRingsValid`
[`DelaunayTriangulationBuilder`]: `SetSites` `Tolerance` `GetSubdivision` `ExtractUniqueCoordinates`
[`VoronoiDiagramBuilder`]: `SetSites` `ClipEnvelope` `Tolerance` `GetSubdivision`
[`LengthIndexedLine`]: `StartIndex` `EndIndex` `IndicesOf` `IndexOfAfter` `IsValidIndex` `ClampIndex`

- `LinearLocation` addresses a component index and a segment fraction, so `LocationIndexedLine` survives a vertex-count change that shifts every length index.

[ENTRYPOINT_SCOPE]: spatial index and nearest neighbour

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `STRtree<T>.Insert(Envelope, T)`                                 | instance | register a candidate              |
|  [02]   | `STRtree<T>.Build()`                                             | instance | pack the tree, closing insertion  |
|  [03]   | `STRtree<T>.Query(Envelope) -> IList<T>`                         | instance | envelope broad phase              |
|  [04]   | `STRtree<T>.Query(Envelope, IItemVisitor<T>)`                    | instance | streaming broad phase             |
|  [05]   | `STRtree<T>.Remove(Envelope, T) -> bool`                         | instance | drop one candidate                |
|  [06]   | `STRtree<T>.NearestNeighbour(IItemDistance) -> T[]`              | instance | closest pair inside the tree      |
|  [07]   | `STRtree<T>.NearestNeighbour(STRtree<T>, IItemDistance) -> T[]`  | instance | closest cross-tree pair           |
|  [08]   | `STRtree<T>.NearestNeighbour(Envelope, T, IItemDistance, int)`   | instance | k nearest to one item             |
|  [09]   | `STRtree<T>.IsWithinDistance(STRtree<T>, IItemDistance, double)` | instance | bounded cross-tree proximity      |
|  [10]   | `Quadtree<T>.Insert(Envelope, T)`                                | instance | insert after querying begins      |
|  [11]   | `Quadtree<T>.Query(Envelope) -> IList<T>`                        | instance | query a mutating candidate set    |
|  [12]   | `Quadtree<T>.Remove(Envelope, T) -> bool`                        | instance | drop one candidate                |
|  [13]   | `HPRtree<T>.Build()`                                             | instance | Hilbert-pack a static set         |
|  [14]   | `HPRtree<T>.Query(Envelope, IItemVisitor<T>)`                    | instance | streaming query on the packed set |

[ENTRYPOINT_SCOPE]: WKT and WKB codecs

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `Geometry.ToText() -> string`                  | instance | WKT emission                   |
|  [02]   | `Geometry.ToBinary() -> byte[]`                | instance | WKB emission                   |
|  [03]   | `WKTReader.Read(string) -> Geometry`           | instance | WKT parse                      |
|  [04]   | `WKTReader.Read(TextReader) -> Geometry`       | instance | WKT reader parse               |
|  [05]   | `WKTReader.Read(Stream) -> Geometry`           | instance | WKT byte parse                 |
|  [06]   | `WKTWriter.Write(Geometry) -> string`          | instance | WKT under the output ordinates |
|  [07]   | `WKTWriter.WriteFormatted(Geometry) -> string` | instance | indented WKT                   |
|  [08]   | `WKBReader.Read(byte[]) -> Geometry`           | instance | WKB parse                      |
|  [09]   | `WKBReader.Read(Stream) -> Geometry`           | instance | WKB stream parse               |
|  [10]   | `WKBReader.HexToBytes(string) -> byte[]`       | static   | hex decode                     |
|  [11]   | `WKBWriter(ByteOrder, bool, bool, bool)`       | ctor     | fixes order, SRID, Z, and M    |
|  [12]   | `WKBWriter.Write(Geometry) -> byte[]`          | instance | WKB emission                   |
|  [13]   | `WKBWriter.Write(Geometry, Stream)`            | instance | WKB stream emission            |
|  [14]   | `WKBWriter.ToHex(byte[]) -> string`            | static   | hex encode                     |

[`WKTReader`]: `Factory` `DefaultSRID` `IsStrict` `FixStructure`
[`WKBReader`]: `HandleSRID` `HandleOrdinates` `AllowedOrdinates` `IsStrict` `RepairRings`
[`WKBWriter`]: `EncodingType` `HandleSRID` `HandleOrdinates` `Strict`

- `WKTReader` and `WKBReader` construct from `NtsGeometryServices`, so parsed geometry inherits its SRID, precision model, and coordinate-sequence policy.

[ENTRYPOINT_SCOPE]: coordinate rewrite and affine mapping

| [INDEX] | [SURFACE]                                                                                      | [SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `Geometry.Apply(ICoordinateSequenceFilter)`                                                    | instance | per-vertex in-place rewrite |
|  [02]   | `Geometry.Apply(IEntireCoordinateSequenceFilter)`                                              | instance | whole-sequence rewrite      |
|  [03]   | `Geometry.GeometryChanged()`                                                                   | instance | clear derived caches        |
|  [04]   | `AffineTransformation(Coordinate, Coordinate, Coordinate, Coordinate, Coordinate, Coordinate)` | ctor     | fit from three point pairs  |
|  [05]   | `AffineTransformation.Compose(AffineTransformation)`                                           | instance | this map then the argument  |
|  [06]   | `AffineTransformation.GetInverse() -> AffineTransformation`                                    | instance | inverse map                 |
|  [07]   | `AffineTransformation.Transform(Geometry) -> Geometry`                                         | instance | mapped copy                 |
|  [08]   | `GeometryEditor.Edit(Geometry, IGeometryEditorOperation)`                                      | instance | structure-preserving edit   |

- `ICoordinateSequenceFilter.GeometryChanged` returning true drives the host geometry's cache invalidation at the end of the walk; a filter that mutates ordinates and returns false leaves a stale envelope behind.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `NtsGeometryServices.Instance` fixes the overlay engine, relate engine, precision model, SRID, and coordinate-sequence factory for every factory, reader, and operation the process constructs; `CreateGeometryFactory` hands back cached factories off that one policy.
- Constructive operations return new geometry, while coordinate filters and metadata setters mutate in place; `Geometry.GeometryChanged()` clears envelope and derived state after a rewrite.
- `PreparedGeometryFactory.Prepare` amortizes one fixed geometry's segment index across every candidate, so one-against-many is its only shape.
- `STRtree<T>` and `HPRtree<T>` pack on `Build()` or on the first `Query`, closing further insertion while query and `Remove` stay legal; `Quadtree<T>` admits interleaved insertion for a candidate set that keeps moving.
- Robustness rides `GeometryOverlay.NG` noding with `PrecisionModel` snapping.
- `GeometryFixer.Fix` and `GeometryPrecisionReducer.Reduce` condition raw ingress before an overlay or a write.
- `PackedDoubleCoordinateSequence` and `RawCoordinateSequence` expose their backing `double[]` and `Memory<double>` stores, so an ordinate batch rewrites in place with no `Coordinate` object per vertex.

[STACKING]:
- `Npgsql.NetTopologySuite`(`Rasm.Persistence/.api/api-npgsql-nts.md`): `NpgsqlNetTopologySuiteExtensions.UseNetTopologySuite` binds this geometry model as the PostGIS `geometry`/`geography` codec, so a spatial column round-trips the same `Geometry` the algebra operates on.
- `ProjNET`(`Rasm.Bim/.api/api-projnet.md`): `Densifier.Densify` bounds segment length first, then `MathTransform.Transform(Span<double>, Span<double>, Span<double>, int, int, int)` batches strided over `RawCoordinateSequence.GetRawCoordinatesAndStride` or `PackedDoubleCoordinateSequence.GetRawCoordinates` in place, and `Geometry.GeometryChanged()` closes the walk.
- `NetTopologySuite.IO.Esri.Shapefile`(`Rasm.Bim/.api/api-nts-esri-shapefile.md`): `ShapefileReader` streams `Feature`/`AttributesTable` values of this model, and the GeoPackage and GeoJSON codecs decode to the identical `Geometry` object.
- `Clipper2`(`libs/csharp/.api/api-clipper2.md`): integer-exact winding takes the toolpath and nesting plane; this rail holds the double-plane topology on both sides of that conversion.
- within-lib: one spatial-join fold composes `STRtree<T>.Query(Envelope)` broad phase with `PreparedGeometryFactory.Prepare` narrow phase and `OverlayNGRobust.Union` dissolve, while `Polygonizer`, `LineMerger`, and `VoronoiDiagramBuilder` assemble corridor, boundary, and medial-axis geometry off that same factory policy.
- wire seam: `Geometry.ToBinary()` and `WKBWriter.Write` emit the domain-footprint wire form; `shapely` decodes it on the Python side and the `WkbParser` port on the TypeScript side.

[LOCAL_ADMISSION]:
- Geometry construction enters through a `NtsGeometryServices.Instance`-resolved `GeometryFactory` configured once at startup.
- Repeated predicates against one fixed geometry enter through `PreparedGeometryFactory.Prepare`; a many-against-many join adds the `STRtree<T>` broad phase in front of it.
- Raw vector ingress passes `GeometryFixer.Fix` before an overlay or a write.
- WKT and WKB I/O enters through readers and writers constructed from the canonical services instance.

[RAIL_LAW]:
- Package: `NetTopologySuite` with `NetTopologySuite.Features`
- Owns: the OGC Simple Features algebra on the double-precision plane — every planar predicate, overlay, index, repair, assembly, and geometry codec the branch calls
- Accept: geometry whose ordinates are production-plane doubles under one declared precision model and SRID
- Reject: coordinate-system transformation, raster and universal vector drivers, 3D meshing, and the exact-predicate geometry kernel `Rasm` owns
