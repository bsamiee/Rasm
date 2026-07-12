# [RASM_API_NETTOPOLOGYSUITE]

`NetTopologySuite` owns the managed planar OGC Simple Features algebra from geometry representation through topology, spatial acceleration, repair, and serialization. The `Rasm` kernel retains exact geometry, while NTS carries floating-point production geometry across geospatial, persistence, and circulation boundaries. Coordinate-system transformation, universal format drivers, raster I/O, and 3D geometry remain outside this rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite`
- package: `NetTopologySuite`
- license: BSD-3-Clause
- assembly: `NetTopologySuite`
- namespace: `NetTopologySuite` (`NtsGeometryServices` global factory cache)
- namespace: `NetTopologySuite.Geometries` (geometry hierarchy, `GeometryFactory`, `PrecisionModel`, `Envelope`, `Coordinate`/`CoordinateSequence`)
- namespace: `NetTopologySuite.Geometries.Prepared` (`PreparedGeometryFactory`, `IPreparedGeometry`)
- namespace: `NetTopologySuite.Geometries.Utilities` (`GeometryFixer`, `AffineTransformation`, `GeometryEditor`)
- namespace: `NetTopologySuite.Operation.OverlayNG` (`OverlayNGRobust`), `NetTopologySuite.Operation.Union` (`UnaryUnionOp`), `NetTopologySuite.Operation.Distance` (`DistanceOp`)
- namespace: `NetTopologySuite.Simplify` (`DouglasPeuckerSimplifier`, `TopologyPreservingSimplifier`), `NetTopologySuite.Densify` (`Densifier`)
- namespace: `NetTopologySuite.Index.Strtree` (`STRtree<T>`), `NetTopologySuite.Index.Quadtree` (`Quadtree<T>`)
- namespace: `NetTopologySuite.IO` (`WKTReader`/`WKTWriter`, `WKBReader`/`WKBWriter`)
- asset: IL-only AnyCPU managed assembly; no P/Invoke or native GEOS dependency
- companion: `NetTopologySuite.Features` (attributed geometry and attribute-table contracts)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry hierarchy
- namespace: `NetTopologySuite.Geometries`
- rail: geometry

`NetTopologySuite.Geometries` partitions the planar model by representation kind.

| [INDEX] | [SYMBOL]                    | [KIND]         | [CAPABILITY]            |
| :-----: | :-------------------------- | :------------- | :---------------------- |
|  [01]   | `Geometry`                  | abstract class | planar geometry algebra |
|  [02]   | `Point`                     | class          | point position          |
|  [03]   | `LineString`                | class          | linear curve            |
|  [04]   | `LinearRing`                | class          | polygon ring            |
|  [05]   | `Polygon`                   | class          | areal boundary          |
|  [06]   | `MultiPoint`                | class          | homogeneous points      |
|  [07]   | `MultiLineString`           | class          | homogeneous curves      |
|  [08]   | `MultiPolygon`              | class          | homogeneous polygons    |
|  [09]   | `GeometryCollection`        | class          | mixed geometries        |
|  [10]   | `Coordinate`                | class          | ordinate tuple          |
|  [11]   | `CoordinateSequence`        | abstract class | packed ordinates        |
|  [12]   | `ICoordinateSequenceFilter` | interface      | coordinate rewrite      |
|  [13]   | `Envelope`                  | class          | axis-aligned 2D extent  |
|  [14]   | `OgcGeometryType`           | enum           | geometry discriminant   |
|  [15]   | `PrecisionModel`            | class          | coordinate rounding     |
|  [16]   | `Dimension`                 | enum           | DE-9IM cell vocabulary  |

[MEMBER_SURFACES]:
- `Geometry`: `OgcGeometryType`, `Dimension`, `SRID`, `EnvelopeInternal`, `Coordinates`, `UserData`, `IsValid`, `IsSimple`, `IsEmpty`, `IsRectangle`, `Area`, `Length`, `Centroid`, `InteriorPoint`, and `Apply(ICoordinateSequenceFilter)`. `Coordinates` returns an array whose elements cannot be assumed to alias internal storage; representation-independent ordinate rewrites use the filter, whose `GeometryChanged` result invalidates cached geometry state.
- `Point`: `X`, `Y`, `Z`, `M`, `Coordinate`, and `CoordinateSequence`.
- `LineString`: `IsClosed`, `IsRing`, `StartPoint`, `EndPoint`, `GetPointN`, and `CoordinateSequence`.
- `LinearRing`: a closed `LineString` that serves as a `Polygon` shell or hole.
- `Polygon`: `ExteriorRing`, `InteriorRings`, `NumInteriorRings`, `Shell`, and `Holes`.
- `MultiPoint`, `MultiLineString`, and `MultiPolygon`: `GetGeometryN` and `NumGeometries` index homogeneous members.
- `GeometryCollection`: the heterogeneous collection preserves mixed-dimension members produced by `GeometryFactory.BuildGeometry`.
- `Coordinate`: `(X, Y)` with `CoordinateZ`, `CoordinateM`, and `CoordinateZM` for optional ordinates.
- `CoordinateSequence`: `Count`, `Ordinates`, `HasZ`, `HasM`, `GetOrdinate`, `SetOrdinate`, `SetX`, `SetY`, and `SetZ` operate over the packed store.
- `ICoordinateSequenceFilter`: `Filter(CoordinateSequence, int)`, `Done`, and `GeometryChanged` define the in-place rewrite walk driven by `Geometry.Apply`.
- `Envelope`: `MinX`, `MaxX`, `MinY`, `MaxY`, `Intersects`, `Contains`, `ExpandToInclude`, and `Centre` define the `STRtree` and `Quadtree` key.
- `OgcGeometryType`: `Point`, `LineString`, `Polygon`, `MultiPoint`, `MultiLineString`, `MultiPolygon`, `GeometryCollection`, and the curve and surface members form the domain-footprint discriminant.
- `PrecisionModel`: `PrecisionModels.Floating`, `FloatingSingle`, and `Fixed`, plus `Scale` and `MakePrecise`, define overlay and repair precision.
- `Dimension`: `Point`, `Curve`, `Surface`, `False`, `True`, and `Dontcare` encode DE-9IM cells.

[PUBLIC_TYPE_SCOPE]: factory and global services
- namespace: `NetTopologySuite`, `NetTopologySuite.Geometries`
- rail: geometry

`NtsGeometryServices` centralizes factory policy, and its peer types bind creation, overlay, and ordinate storage.

| [INDEX] | [SYMBOL]                    | [KIND]         | [CAPABILITY]          |
| :-----: | :-------------------------- | :------------- | :-------------------- |
|  [01]   | `NtsGeometryServices`       | class          | factory policy cache  |
|  [02]   | `GeometryFactory`           | class          | geometry construction |
|  [03]   | `GeometryOverlay`           | abstract class | boolean engine        |
|  [04]   | `CoordinateSequenceFactory` | abstract class | ordinate storage      |

[MEMBER_SURFACES]:
- `NtsGeometryServices.Instance` owns global overlay, relate, precision, SRID, and coordinate-sequence policy; `CreateGeometryFactory(int)` returns a cached factory.
- `GeometryFactory` constructs under one `PrecisionModel`, `SRID`, and `CoordinateSequenceFactory`; `CreatePoint`, `CreateLineString`, `CreateLinearRing`, `CreatePolygon`, `CreateMultiPoint`, `CreateMultiPointFromCoords`, `CreateMultiPolygon`, `CreateGeometryCollection`, `BuildGeometry`, `ToGeometry`, and `CreateEmpty` span the geometry family.
- `GeometryOverlay.NG` selects the `OverlayNG` constructive-operation engine.
- `CoordinateSequenceFactory` admits `PackedCoordinateSequenceFactory.DoubleFactory`, `PackedCoordinateSequenceFactory.FloatFactory`, or `CoordinateArraySequenceFactory` as the ordinate-store strategy.

[PUBLIC_TYPE_SCOPE]: prepared geometry, spatial index, validity repair
- namespace: `NetTopologySuite.Geometries.Prepared`, `NetTopologySuite.Index.Strtree`, `NetTopologySuite.Geometries.Utilities`
- rail: geometry

Prepared geometries and spatial indexes separate repeated-query acceleration from repair and transformation.

| [INDEX] | [SYMBOL]                  | [KIND]    | [CAPABILITY]          |
| :-----: | :------------------------ | :-------- | :-------------------- |
|  [01]   | `PreparedGeometryFactory` | class     | predicate preparation |
|  [02]   | `IPreparedGeometry`       | interface | indexed predicates    |
|  [03]   | `STRtree<TItem>`          | class     | bulk-loaded queries   |
|  [04]   | `Quadtree<TItem>`         | class     | incremental mutation  |
|  [05]   | `GeometryFixer`           | class     | geometry validity     |
|  [06]   | `AffineTransformation`    | class     | planar affine mapping |

[MEMBER_SURFACES]:
- `PreparedGeometryFactory.Prepare(Geometry)` and `Create(Geometry)` build the indexed predicate target.
- `IPreparedGeometry` owns `Contains`, `ContainsProperly`, `CoveredBy`, `Covers`, `Crosses`, `Disjoint`, `Intersects`, `Overlaps`, `Touches`, `Within`, and its source `Geometry`; one prepared target amortizes its segment index across candidates.
- `STRtree<TItem>` owns `Insert`, list and visitor `Query`, `Remove`, `NearestNeighbour`, and `IsWithinDistance`. Nearest-neighbor evaluation compares the tree with itself, another tree, one item, or the item's `k` results; its first query builds the tree and closes further insertion, while query and removal remain legal.
- `Quadtree<TItem>` retains mutation after queries begin.
- `GeometryFixer.Fix` repairs invalid geometry, and the instance path carries `KeepCollapsed`, `KeepMulti`, and `GetResult()` policy.
- `AffineTransformation` composes `Translate`, `Scale`, `Rotate`, and `Shear` before `Transform(Geometry)` applies the planar map.

[PUBLIC_TYPE_SCOPE]: attributed-feature shape (from `NetTopologySuite.Features`)
- namespace: `NetTopologySuite.Features`
- rail: geometry

The feature contract couples one geometry with its attribute table.

| [INDEX] | [SYMBOL]           | [KIND]    | [CAPABILITY]        |
| :-----: | :----------------- | :-------- | :------------------ |
|  [01]   | `IFeature`         | interface | attributed geometry |
|  [02]   | `Feature`          | class     | mutable feature     |
|  [03]   | `IAttributesTable` | interface | attribute contract  |
|  [04]   | `AttributesTable`  | class     | attribute storage   |

[MEMBER_SURFACES]:
- `IFeature` exposes `Geometry`, `Attributes`, and `BoundingBox` as the vector-codec row shape.
- `Feature(Geometry, IAttributesTable)` constructs the sealed mutable `Feature` with geometry and attributes; `BoundingBox` remains independently assignable.
- `IAttributesTable` owns `Count`, `this[string]`, `GetNames`, `GetValues`, and `Exists`.
- `AttributesTable` constructs from a dictionary or key-value sequence and owns `Add`, `DeleteAttribute`, `MergeWith`, and `GetOptionalValue`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binary predicates (DE-9IM relate)
- namespace: `NetTopologySuite.Geometries`
- rail: geometry

`Geometry` owns binary predicates over another geometry.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                        | [CAPABILITY]         |
| :-----: | :-------------------------- | :-------------------------------------------------- | :------------------- |
|  [01]   | `Geometry.Intersects`       | `(Geometry g)` → `bool`                             | shared-point test    |
|  [02]   | `Geometry.Contains`         | `(Geometry g)` → `bool`                             | interior containment |
|  [03]   | `Geometry.Within`           | `(Geometry g)` → `bool`                             | inverse containment  |
|  [04]   | `Geometry.Covers`           | `(Geometry g)` → `bool`                             | boundary containment |
|  [05]   | `Geometry.CoveredBy`        | `(Geometry g)` → `bool`                             | inverse coverage     |
|  [06]   | `Geometry.Crosses`          | `(Geometry g)` → `bool`                             | crossing relation    |
|  [07]   | `Geometry.Overlaps`         | `(Geometry g)` → `bool`                             | overlap relation     |
|  [08]   | `Geometry.Touches`          | `(Geometry g)` → `bool`                             | boundary contact     |
|  [09]   | `Geometry.Disjoint`         | `(Geometry g)` → `bool`                             | disjointness         |
|  [10]   | `Geometry.Relate`           | `(Geometry g)` → `IntersectionMatrix`               | DE-9IM matrix        |
|  [11]   | `Geometry.Relate`           | `(Geometry g, string intersectionPattern)` → `bool` | DE-9IM mask test     |
|  [12]   | `Geometry.Distance`         | `(Geometry g)` → `double`                           | minimum separation   |
|  [13]   | `Geometry.IsWithinDistance` | `(Geometry geom, double distance)` → `bool`         | bounded proximity    |

`Intersects` confirms candidates after envelope screening, and `IsWithinDistance` exits before computing an unneeded full distance.

[ENTRYPOINT_SCOPE]: boolean and constructive operations
- namespace: `NetTopologySuite.Geometries`, `NetTopologySuite.Operation.OverlayNG`, `NetTopologySuite.Operation.Union`
- rail: geometry

Constructive operations return geometry and never mutate their operands.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                                                    | [CAPABILITY]          |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------ | :-------------------- |
|  [01]   | `Geometry.Intersection`        | `(Geometry other)` → `Geometry`                                                 | overlay intersection  |
|  [02]   | `Geometry.Union`               | `(Geometry other)` → `Geometry`                                                 | overlay union         |
|  [03]   | `Geometry.Union`               | `()` → `Geometry`                                                               | collection self-union |
|  [04]   | `Geometry.Difference`          | `(Geometry other)` → `Geometry`                                                 | overlay difference    |
|  [05]   | `Geometry.SymmetricDifference` | `(Geometry other)` → `Geometry`                                                 | symmetric difference  |
|  [06]   | `Geometry.Buffer`              | `(double distance)` → `Geometry`                                                | default offset        |
|  [07]   | `Geometry.Buffer`              | `(double distance, int quadrantSegments)` → `Geometry`                          | segmented offset      |
|  [08]   | `Geometry.Buffer`              | `(double distance, BufferParameters bufferParameters)` → `Geometry`             | policy offset         |
|  [09]   | `Geometry.Buffer`              | `(double distance, EndCapStyle endCapStyle)` → `Geometry`                       | capped offset         |
|  [10]   | `Geometry.Buffer`              | `(double distance, int quadrantSegments, EndCapStyle endCapStyle)` → `Geometry` | segmented cap         |
|  [11]   | `Geometry.ConvexHull`          | `()` → `Geometry`                                                               | convex enclosure      |
|  [12]   | `OverlayNGRobust.Overlay`      | `(Geometry geom0, Geometry geom1, SpatialFunction opCode)` → `Geometry`         | robust binary overlay |
|  [13]   | `OverlayNGRobust.OverlaySR`    | `(Geometry geom0, Geometry geom1, SpatialFunction opCode)` → `Geometry?`        | snap-rounded attempt  |
|  [14]   | `OverlayNGRobust.Union`        | `(Geometry geom)` → `Geometry`                                                  | robust unary union    |
|  [15]   | `OverlayNGRobust.Union`        | `(IEnumerable<Geometry> geoms)` → `Geometry`                                    | robust bulk union     |
|  [16]   | `OverlayNGRobust.Union`        | `(IEnumerable<Geometry> geoms, GeometryFactory geomFact)` → `Geometry`          | factory-bound union   |
|  [17]   | `UnaryUnionOp.Union`           | `(IEnumerable<Geometry> geoms)` → `Geometry`                                    | cascaded union        |

`Geometry` constructive methods route through the configured overlay engine. `BufferParameters` owns join, cap, mitre, and single-sided policy; `SpatialFunction` selects the overlay operation.

[ENTRYPOINT_SCOPE]: simplify, densify, nearest-point
- namespace: `NetTopologySuite.Simplify`, `NetTopologySuite.Densify`, `NetTopologySuite.Operation.Distance`
- rail: geometry

Transformation functions return new geometry under an explicit tolerance or operand pair.

| [INDEX] | [SURFACE]                               | [CALL_SHAPE]                                             | [CAPABILITY]            |
| :-----: | :-------------------------------------- | :------------------------------------------------------- | :---------------------- |
|  [01]   | `DouglasPeuckerSimplifier.Simplify`     | `(Geometry geom, double distanceTolerance)` → `Geometry` | vertex reduction        |
|  [02]   | `TopologyPreservingSimplifier.Simplify` | `(Geometry geom, double distanceTolerance)` → `Geometry` | topology-safe reduction |
|  [03]   | `Densifier.Densify`                     | `(Geometry geom, double distanceTolerance)` → `Geometry` | segment densification   |
|  [04]   | `DistanceOp.NearestPoints`              | `(Geometry g0, Geometry g1)` → `Coordinate[]`            | nearest-point witness   |
|  [05]   | `DistanceOp.IsWithinDistance`           | `(Geometry g0, Geometry g1, double distance)` → `bool`   | bounded proximity       |

`DouglasPeuckerSimplifier` may break inter-ring topology, while `TopologyPreservingSimplifier` preserves it. `Densifier` bounds segment length before nonlinear reprojection, and `NearestPoints` returns the clash-gap witness.

[ENTRYPOINT_SCOPE]: WKT/WKB serialization
- namespace: `NetTopologySuite.IO`, `NetTopologySuite.Geometries`
- rail: geometry

Text and binary codecs admit or emit geometry through explicit boundary shapes.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                        | [CAPABILITY]      |
| :-----: | :--------------------- | :------------------------------------------------------------------ | :---------------- |
|  [01]   | `Geometry.ToText`      | `()` → `string`                                                     | WKT emission      |
|  [02]   | `Geometry.AsText`      | `()` → `string`                                                     | WKT emission      |
|  [03]   | `Geometry.ToBinary`    | `()` → `byte[]`                                                     | WKB emission      |
|  [04]   | `Geometry.AsBinary`    | `()` → `byte[]`                                                     | WKB emission      |
|  [05]   | `WKTReader.Read`       | `(string wellKnownText)` → `Geometry`                               | WKT parsing       |
|  [06]   | `WKTReader.Read`       | `(TextReader reader)` → `Geometry`                                  | WKT stream parse  |
|  [07]   | `WKTReader.Read`       | `(Stream stream)` → `Geometry`                                      | WKT byte parse    |
|  [08]   | `WKBReader.Read`       | `(byte[] data)` → `Geometry`                                        | WKB parsing       |
|  [09]   | `WKBReader.Read`       | `(Stream stream)` → `Geometry`                                      | WKB stream parse  |
|  [10]   | `WKBReader.HexToBytes` | `(string hex)` → `byte[]`                                           | hex decoding      |
|  [11]   | `WKBWriter`            | `()`                                                                | default policy    |
|  [12]   | `WKBWriter`            | `(ByteOrder encodingType)`                                          | byte-order policy |
|  [13]   | `WKBWriter`            | `(ByteOrder encodingType, bool handleSRID)`                         | SRID policy       |
|  [14]   | `WKBWriter`            | `(ByteOrder encodingType, bool handleSRID, bool emitZ)`             | Z policy          |
|  [15]   | `WKBWriter`            | `(ByteOrder encodingType, bool handleSRID, bool emitZ, bool emitM)` | Z/M policy        |
|  [16]   | `WKBWriter.Write`      | `(Geometry geometry)` → `byte[]`                                    | WKB emission      |
|  [17]   | `WKBWriter.Write`      | `(Geometry geometry, Stream stream)` → `void`                       | WKB stream emit   |
|  [18]   | `WKBWriter.ToHex`      | `(byte[] bytes)` → `string`                                         | hex encoding      |

`WKTReader` and `WKBReader` consume `NtsGeometryServices` so parsed geometry inherits its SRID, precision, and coordinate-sequence policy. `WKTReader` carries strictness, structure repair, and coordinate-syntax policy; `WKBReader` carries `HandleSRID`, `HandleOrdinates`, `IsStrict`, and `RepairRings`. `WKBWriter` fixes byte order, SRID handling, and Z/M emission at construction.

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- `NetTopologySuite.dll` is a managed JTS port with no P/Invoke or native GEOS boundary; OGR keeps its native topology engine inside the GDAL boundary.
- Constructive operations return new geometries, while metadata setters and coordinate filters can mutate an instance; `GeometryChanged()` invalidates derived caches after coordinate edits.

[GLOBAL_SERVICES]:
- `NtsGeometryServices.Instance` is configured during startup with `GeometryOverlay.NG`, the admitted precision and SRID policy, and a `PackedCoordinateSequenceFactory`; readers consume that service, and `CreateGeometryFactory` resolves cached factories from it.
- `PackedCoordinateSequenceFactory` stores interleaved ordinates in one `double[]` or `float[]`, avoiding a `Coordinate` object per point at the geometry boundary.

[PREDICATE_ACCELERATION]:
- Repeated predicate queries against one fixed geometry build its segment index through `PreparedGeometryFactory.Prepare`, then evaluate candidates through `IPreparedGeometry` predicates.
- `STRtree<TItem>` bulk-inserts candidates before its first `Query`; subsequent queries and removals operate on the built tree, while `Quadtree<TItem>` admits interleaved insertion.
- The spatial-join rail composes `STRtree.Query` candidate envelopes with `IPreparedGeometry.Intersects` exact confirmation.

[ROBUST_OVERLAY]:
- `GeometryOverlay.NG` configures constructive operations for noding and precision-model snapping; `OverlayNGRobust.Overlay` and `OverlayNGRobust.Union` expose direct robust binary and bulk operations.
- `GeometryFixer.Fix` repairs invalid ingress before an overlay or write.

[STACK_INTEGRATION]:
- `Rasm.Bim` geospatial: `NetTopologySuite.IO.Esri.Shapefile`, the GeoJSON/GeoPackage codecs, and the `MaxRev.Gdal.Core` OGR ingest all materialize and emit `Geometry`/`Feature` through this core; `ProjNET` reprojects its ordinates between EPSG-keyed systems (the `Densifier.Densify` pre-step tracks the datum), with `MaxRev.Gdal.Core` OSR the exotic-datum escalation. NTS holds the planar algebra on both sides; it never reprojects itself.
- `Rasm.Persistence` spatial: the PostGIS spatial column binds `Npgsql.NetTopologySuite`, exchanging `Geometry` over WKB; the `STRtree` broad-phase and `PreparedGeometry` narrow-phase are the in-process spatial-join lane beside the SQL side.
- `Rasm.Compute` circulation resolves space boundaries through the content-keyed `GeometrySource` port and uses NTS for visibility, offset, and area operations; graph packages retain flow and topology ownership.
- wire seam: `Geometry.ToBinary()` and `WKBWriter` encode the domain-footprint wire form. Python decodes it through `shapely`, while TypeScript routes it through `WkbParser` before planar GeoJSON operations.

[LOCAL_ADMISSION]:
- geometry construction enters through a `NtsGeometryServices.Instance`-resolved `GeometryFactory`; the global singleton is configured once at startup.
- predicates and boolean ops enter through the `Geometry` instance methods; the prepared/indexed forms (`PreparedGeometry`, `STRtree`) are used whenever one geometry faces many.
- validity repair enters through `GeometryFixer.Fix` before raw vector geometry reaches an overlay.
- WKT/WKB I/O enters through the `WKTReader`/`WKBReader`/`WKBWriter` family seeded with the canonical `NtsGeometryServices` instance.

[RAIL_LAW]:
- Package: `NetTopologySuite` with the `NetTopologySuite.Features` companion
- Owns: the OGC Simple-Features planar geometry algebra — type hierarchy, DE-9IM predicates, robust overlay/boolean, buffer/hull/simplify/densify, R-tree spatial index, prepared geometry, validity repair, WKT/WKB I/O, the attributed `Feature` shape
- Accept: planar topology, spatial predicates and joins, 2D boolean operations, footprint dissolve, geospatial broad/narrow-phase indexing, the circulation planar side
- Reject: coordinate-system transformation, universal vector and raster I/O, byte-format codecs, 3D geometry and meshing, and exact predicates
