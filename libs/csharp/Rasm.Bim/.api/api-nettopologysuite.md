# [RASM_BIM_API_NETTOPOLOGYSUITE]

`NetTopologySuite` is the pure-managed JTS port supplying the OGC Simple-Features
planar geometry algebra: the `Geometry` type hierarchy, the binary-predicate (DE-9IM)
relate matrix, the robust overlay/boolean engine (`OverlayNG`), buffer/convex-hull/
simplify/densify operations, the `STRtree` bulk-loaded R-tree spatial index with k-NN,
`PreparedGeometry` for repeated-query acceleration, `GeometryFixer` validity repair, and
the WKT/WKB readers/writers. It is the planar topology owner of the
`Semantics/georeference#GEOSPATIAL_SEAM`: the shapefile/GeoPackage/GeoJSON codecs and the
`MaxRev.Gdal.Core` OGR ingest all materialize and emit `Geometry`/`Feature` through this
core, and `ProjNET` reprojects its ordinates between EPSG-keyed coordinate systems.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NetTopologySuite`
- package: `NetTopologySuite`
- version: `2.6.0`
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
- asset: netstandard2.1, netstandard2.0; the net10.0 consumer binds the `lib/netstandard2.1` asset (same managed surface as the ns2.0 build)
- asset: IL-only AnyCPU managed assembly; no P/Invoke, no native GEOS dependency (the topology engine is pure C# — distinct from the `MaxRev.Gdal.Core` runtime's bundled `libgeos`)
- dependency: `NetTopologySuite.Features` >= 2.2.0 (transitive; supplies `Feature`/`IFeature`/`AttributesTable`/`IAttributesTable` — the attributed-geometry shape the shapefile/GeoJSON codecs read and write)
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry hierarchy
- package: `NetTopologySuite`
- namespace: `NetTopologySuite.Geometries`
- rail: geometry

| [INDEX] | [SYMBOL]             | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Geometry`           | geometry | abstract root; `OgcGeometryType`, `Dimension`, `SRID`, `EnvelopeInternal`, `Coordinates`, `UserData`, `IsValid`/`IsSimple`/`IsEmpty`/`IsRectangle`, `Area`/`Length`, `Centroid`/`InteriorPoint`. Owns the predicate, boolean, and serialization methods enumerated in [03]/[04] |
|  [02]   | `Point`              | geometry | 0-dim; `X`/`Y`/`Z`/`M`, `Coordinate`, `CoordinateSequence`                                               |
|  [03]   | `LineString`         | geometry | 1-dim curve; `IsClosed`, `IsRing`, `StartPoint`/`EndPoint`, `GetPointN`, `CoordinateSequence`            |
|  [04]   | `LinearRing`         | geometry | closed `LineString`; ring shell/hole component of `Polygon`                                              |
|  [05]   | `Polygon`            | geometry | 2-dim; `ExteriorRing`, `InteriorRings`/`NumInteriorRings`, `Shell`/`Holes`                               |
|  [06]   | `MultiPoint` / `MultiLineString` / `MultiPolygon` | geometry | homogeneous collections; indexed `GetGeometryN`, `NumGeometries`                                         |
|  [07]   | `GeometryCollection` | geometry | heterogeneous collection; the `BuildGeometry` fallback when members are mixed-dimension                  |
|  [08]   | `Coordinate`         | geometry | `(X, Y)` value; `CoordinateZ`/`CoordinateM`/`CoordinateZM` carry the optional Z/M ordinates              |
|  [09]   | `CoordinateSequence` | geometry | packed ordinate store; `Count`, `Ordinates`/`HasZ`/`HasM`, `GetOrdinate`/`SetOrdinate` — the buffer a kernel mesh/curve maps onto without per-point `Coordinate` boxing |
|  [10]   | `Envelope`           | geometry | axis-aligned 2D bbox; `MinX`/`MaxX`/`MinY`/`MaxY`, `Intersects`, `Contains`, `ExpandToInclude`, `Centre`. The `STRtree`/`Quadtree` key and the GDAL `Dataset.GetExtent` carrier |
|  [11]   | `OgcGeometryType`    | geometry | enum: `Point`, `LineString`, `Polygon`, `MultiPoint`, `MultiLineString`, `MultiPolygon`, `GeometryCollection`, plus the curve/surface OGC members — the discriminant a `[SmartEnum]`/match folds onto a `BimElement` footprint kind |
|  [12]   | `PrecisionModel`     | geometry | coordinate rounding policy; `PrecisionModels.Floating`/`FloatingSingle`/`Fixed`, `Scale`, `MakePrecise` — the robustness knob `OverlayNG` and `GeometryFixer` honor |
|  [13]   | `Dimension`          | geometry | enum: `Point`(0)/`Curve`(1)/`Surface`(2)/`False`/`True`/`DontCare` — the DE-9IM matrix cell vocabulary |

[PUBLIC_TYPE_SCOPE]: factory and global services
- package: `NetTopologySuite`
- namespace: `NetTopologySuite`, `NetTopologySuite.Geometries`
- rail: geometry

| [INDEX] | [SYMBOL]               | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :--------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `NtsGeometryServices`  | geometry | the global factory/precision/SRID cache; static `Instance` is the canonical singleton owner (set once at startup with a `GeometryOverlay`/`GeometryRelate`/`PrecisionModel`/`CoordinateSequenceFactory`). `CreateGeometryFactory(srid)` returns a cached factory — the rejected form is a per-call `new GeometryFactory()` |
|  [02]   | `GeometryFactory`      | geometry | builds geometries under a fixed `PrecisionModel`/`SRID`/`CoordinateSequenceFactory`; `CreatePoint`/`CreateLineString`/`CreateLinearRing`/`CreatePolygon`/`CreateMultiPolygon`/`CreateGeometryCollection`, `BuildGeometry(IEnumerable<Geometry>)` (dimension-folding constructor), `ToGeometry(Envelope)`, `CreateEmpty(Dimension)`. Static `Default`/`Floating`/`FloatingSingle`/`Fixed` presets |
|  [03]   | `GeometryOverlay`      | geometry | the boolean-engine strategy `NtsGeometryServices` injects; `NG` selects the robust `OverlayNG` engine, `Legacy` the old overlay — `NG` is the admitted default |
|  [04]   | `CoordinateSequenceFactory` | geometry | the ordinate-store strategy; `PackedCoordinateSequenceFactory` (struct-of-arrays `double[]`/`float[]`, the dense interop-friendly layout) vs `CoordinateArraySequenceFactory` (`Coordinate[]`) |

[PUBLIC_TYPE_SCOPE]: prepared geometry, spatial index, validity repair
- package: `NetTopologySuite`
- namespace: `NetTopologySuite.Geometries.Prepared`, `NetTopologySuite.Index.Strtree`, `NetTopologySuite.Geometries.Utilities`
- rail: geometry

| [INDEX] | [SYMBOL]                   | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :------------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `PreparedGeometryFactory`  | geometry | builds an indexed query geometry; static `Prepare(Geometry)` or instance `Create(Geometry)`              |
|  [02]   | `IPreparedGeometry`        | geometry | thread-safe pre-indexed predicate target; `Contains`/`ContainsProperly`/`CoveredBy`/`Covers`/`Crosses`/`Disjoint`/`Intersects`/`Overlaps`/`Touches`, `Geometry`. The form to use when ONE geometry is tested against many (segment tree built once) |
|  [03]   | `STRtree<TItem>`           | geometry | bulk-loaded Sort-Tile-Recursive R-tree; `Insert(Envelope, item)`, `Query(Envelope)` / `Query(Envelope, IItemVisitor)`, `Remove`, `NearestNeighbour(IItemDistance)` / `NearestNeighbour(env, item, dist, k)` (k-NN), `IsWithinDistance(tree, dist, maxDistance)`. Read-only after first query — the broad-phase index for clash/spatial-join candidate sets |
|  [04]   | `Quadtree<TItem>`          | geometry | incrementally mutable region quadtree; the index to prefer over `STRtree` when items are added after queries begin |
|  [05]   | `GeometryFixer`            | geometry | makes an invalid geometry OGC-valid (self-intersection/ring-order repair); static `Fix(Geometry)` / `Fix(geom, isKeepMulti)`, instance `KeepCollapsed`/`KeepMulti` + `GetResult()` — the repair leg before an overlay or shapefile write rejects a degenerate ring |
|  [06]   | `AffineTransformation`     | geometry | 2D affine matrix over geometry; `Translate`/`Scale`/`Rotate`/`Shear` composition, `Transform(Geometry)` — the planar transform distinct from the `ProjNET` geodetic leg |

[PUBLIC_TYPE_SCOPE]: attributed-feature shape (from `NetTopologySuite.Features` 2.2.0)
- package: `NetTopologySuite.Features`
- namespace: `NetTopologySuite.Features`
- rail: geometry

| [INDEX] | [SYMBOL]            | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :------------------ | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `IFeature`          | geometry | geometry + attribute contract; `Geometry`, `Attributes`, `BoundingBox` — the row shape every vector codec exchanges |
|  [02]   | `Feature`           | geometry | sealed `IFeature`; `Feature(Geometry, IAttributesTable)`, `Geometry`/`Attributes` setters, `BoundingBox` |
|  [03]   | `IAttributesTable`  | geometry | string-keyed attribute bag contract; `Count`, `this[name]`, `GetNames`/`GetValues`, `Exists`             |
|  [04]   | `AttributesTable`   | geometry | dictionary-backed `IAttributesTable`; ctors from `Dictionary<string,object>`/`IEnumerable<KeyValuePair>`, `Add`/`DeleteAttribute`/`MergeWith`, `GetOptionalValue` — the carrier mapped to/from a `BimElement` `PropertySet` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binary predicates (DE-9IM relate)
- package: `NetTopologySuite`
- namespace: `NetTopologySuite.Geometries`
- rail: geometry

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                  | [CAPABILITY]                                                  |
| :-----: | :------------------------- | :-------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Geometry.Intersects`      | `(Geometry g)` → `bool`                       | any shared point — the broad-phase narrow-confirm predicate  |
|  [02]   | `Geometry.Contains`        | `(Geometry g)` → `bool`                       | every point of `g` inside, interiors meet                    |
|  [03]   | `Geometry.Within`          | `(Geometry g)` → `bool`                       | the inverse of `Contains`                                    |
|  [04]   | `Geometry.Covers` / `CoveredBy` | `(Geometry g)` → `bool`                  | containment without the interior-intersection requirement    |
|  [05]   | `Geometry.Crosses` / `Overlaps` / `Touches` / `Disjoint` | `(Geometry g)` → `bool` | the remaining OGC spatial relationships                       |
|  [06]   | `Geometry.Relate`          | `(Geometry g)` → `IntersectionMatrix`         | the full DE-9IM matrix; `Relate(g, pattern)` tests a 9-char mask |
|  [07]   | `Geometry.Distance`        | `(Geometry g)` → `double`                     | minimum Euclidean separation                                 |
|  [08]   | `Geometry.IsWithinDistance` | `(Geometry g, double d)` → `bool`            | early-out proximity test (cheaper than `Distance < d`)       |

[ENTRYPOINT_SCOPE]: boolean and constructive operations
- package: `NetTopologySuite`
- namespace: `NetTopologySuite.Geometries`, `NetTopologySuite.Operation.OverlayNG`, `NetTopologySuite.Operation.Union`
- rail: geometry

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                            | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :----------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Geometry.Intersection`         | `(Geometry other)` → `Geometry`                        | overlay AND (routes through the `OverlayNG` engine)          |
|  [02]   | `Geometry.Union`                | `(Geometry other)` → `Geometry` / `()` → `Geometry`   | overlay OR; the no-arg form self-unions a collection         |
|  [03]   | `Geometry.Difference` / `SymmetricDifference` | `(Geometry other)` → `Geometry`           | overlay MINUS / XOR                                          |
|  [04]   | `Geometry.Buffer`               | `(double d)` / `(d, int quadrantSegments)` / `(d, BufferParameters)` / `(d, EndCapStyle)` → `Geometry` | offset polygon; `BufferParameters` controls cap/join/mitre and single-sided |
|  [05]   | `Geometry.ConvexHull`           | `()` → `Geometry`                                      | the minimal enclosing convex polygon                         |
|  [06]   | `OverlayNGRobust.Overlay`       | `(Geometry a, Geometry b, SpatialFunction op)` (static) | the robust overlay engine directly; `op` ∈ Intersection/Union/Difference/SymDifference |
|  [07]   | `OverlayNGRobust.Union`         | `(IEnumerable<Geometry> geoms[, GeometryFactory])` (static) | robust noded union of many geometries — the parcel/footprint dissolve |
|  [08]   | `UnaryUnionOp.Union`            | `(IEnumerable<Geometry> geoms)` (static)               | cascaded-union of a heterogeneous set (faster than pairwise) |

[ENTRYPOINT_SCOPE]: simplify, densify, nearest-point
- package: `NetTopologySuite`
- namespace: `NetTopologySuite.Simplify`, `NetTopologySuite.Densify`, `NetTopologySuite.Operation.Distance`
- rail: geometry

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                  | [CAPABILITY]                                                  |
| :-----: | :------------------------------------- | :-------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `DouglasPeuckerSimplifier.Simplify`    | `(Geometry, double tol)` (static) → `Geometry` | vertex-reduction LOD (may break topology between rings)      |
|  [02]   | `TopologyPreservingSimplifier.Simplify` | `(Geometry, double tol)` (static) → `Geometry` | topology-safe simplification (no ring self-cross)            |
|  [03]   | `Densifier.Densify`                    | `(Geometry, double tol)` (static) → `Geometry` | inserts vertices so no segment exceeds `tol` — the pre-reprojection step so a long edge curves correctly under a non-linear datum transform |
|  [04]   | `DistanceOp.NearestPoints`             | `(Geometry g0, Geometry g1)` (static) → `Coordinate[2]` | the closest pair of points across two geometries (the clash-gap witness) |
|  [05]   | `DistanceOp.IsWithinDistance`          | `(Geometry g0, Geometry g1, double d)` (static) → `bool` | proximity early-out                                          |

[ENTRYPOINT_SCOPE]: WKT/WKB serialization
- package: `NetTopologySuite`
- namespace: `NetTopologySuite.IO`, `NetTopologySuite.Geometries`
- rail: geometry

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                  | [CAPABILITY]                                                  |
| :-----: | :--------------------- | :-------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `Geometry.ToText` / `AsText` | `()` → `string`                          | OGC WKT emit                                                 |
|  [02]   | `Geometry.ToBinary` / `AsBinary` | `()` → `byte[]`                       | OGC WKB emit                                                 |
|  [03]   | `WKTReader.Read`       | `(string)` / `(TextReader)` / `(Stream)` → `Geometry` | WKT parse; ctor takes a `GeometryFactory`/`NtsGeometryServices` so parsed geometry carries the canonical SRID/precision |
|  [04]   | `WKBReader.Read`       | `(byte[])` / `(Stream)` → `Geometry`          | WKB parse; `HandleSRID`, `HandleOrdinates`, `RepairRings` knobs. `HexToBytes(string)` decodes a hex WKB column — the bridge from a PostGIS/`MaxRev.Gdal.Core` OGR `ExportToWkb` buffer |
|  [05]   | `WKBWriter`            | `new WKBWriter(ByteOrder, handleSRID, Ordinates)`; `Write(Geometry)` → `byte[]` | WKB emit with explicit byte-order and Z/M handling |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- ships a single managed `NetTopologySuite.dll` (netstandard2.1/2.0 only); no P/Invoke, no native GEOS. The topology engine is a pure C# JTS port — this is the seam distinction from `MaxRev.Gdal.Core`, whose OGR side carries a bundled native `libgeos`. Two GEOS-equivalent engines exist in the closure; NTS owns the managed planar algebra and OGR's GEOS stays inside the GDAL native boundary.
- the net10.0 consumer binds the `lib/netstandard2.1` asset (identical managed surface to the ns2.0 build).
- `Geometry` is immutable by contract: predicates and boolean ops return new geometries; `GeometryChanged()` is the only mutation hook (for in-place coordinate edits via a filter).

[GLOBAL_SERVICES]:
- `NtsGeometryServices.Instance` is the single global factory/precision/SRID owner — set it ONCE at startup with the admitted `GeometryOverlay.NG` engine and a `PackedCoordinateSequenceFactory`, then resolve cached factories through it. Every WKT/WKB reader, the shapefile codec, and the OGR ingest read this singleton for the canonical `PrecisionModel`/`SRID`, so a per-call `new GeometryFactory()` is the rejected form that fragments precision.
- `PackedCoordinateSequenceFactory` is the dense layout: it stores ordinates in a `double[]`/`float[]` struct-of-arrays rather than `Coordinate[]`, so a kernel mesh/curve buffer maps onto a `CoordinateSequence` without per-point boxing — the interop-friendly choice when geometry crosses the `Rasm` kernel seam.

[PREDICATE_ACCELERATION]:
- repeated predicate queries against one fixed geometry build the segment intersection tree once through `PreparedGeometryFactory.Prepare(geom)`, then call `IPreparedGeometry.Intersects`/`Contains`/... per candidate. This is the form for a point-in-polygon classification of many elements against one zone, or a clash narrow-phase after the broad-phase index.
- `STRtree<T>` is the broad-phase: bulk-`Insert(envelope, item)` every candidate, then `Query(searchEnvelope)` returns the overlap candidate set, or `NearestNeighbour(env, item, dist, k)` returns the k nearest. It is read-only after the first query (the tree builds lazily on first `Query`); a workflow that interleaves inserts and queries uses `Quadtree<T>` instead.
- the canonical spatial-join rail STACKS both: `STRtree.Query` for candidate envelopes, then `PreparedGeometry.Intersects` for the exact predicate — the same two-tier broad-then-narrow pattern the `SwiftCollections.Lean` 3D BVH runs for clash, projected to 2D for the geospatial footprint.

[ROBUST_OVERLAY]:
- `GeometryOverlay.NG` (the `OverlayNG` engine) is the robust default: it nodes the input, snaps to the `PrecisionModel`, and avoids the topology exceptions the legacy overlay throws on near-degenerate input. `OverlayNGRobust.Union(IEnumerable<Geometry>)` is the parcel/footprint dissolve; `OverlayNGRobust.Overlay(a, b, op)` is the direct two-geometry boolean.
- a geometry that fails `IsValid` (self-intersecting ring, wrong ring order) is repaired through `GeometryFixer.Fix(geom)` BEFORE an overlay or a shapefile write — the validity gate the `format#GEOSPATIAL` codec runs so a malformed OGR/shapefile ring does not poison the boolean engine.

[STACK_INTEGRATION]:
- vector codec seam: `NetTopologySuite.IO.Esri.Shapefile` reads/writes `Feature[]`/`IFeature` directly (the `api-nts-esri-shapefile` catalog) — NTS is the geometry algebra those features carry. `bertt.CityJSON` and the GeoJSON/GeoPackage codecs all exchange the same `Geometry`/`Feature` shape.
- GDAL/OGR seam: `MaxRev.Gdal.Core` OGR `Geometry.ExportToWkb()` produces a WKB buffer that `WKBReader.Read(byte[])` materializes into an NTS `Geometry`, and `WKBWriter.Write` produces the buffer `OGR.Geometry.CreateFromWkb` consumes — the canonical bidirectional bridge between the GDAL universal-driver ingest and the managed planar algebra.
- reprojection seam: NTS holds the topology; `ProjNET` holds the default managed geodetic transform, with `MaxRev.Gdal.Core` OSR as the escalation counterpart for the exotic datum-grid/dynamic-datum transforms `ProjNET` cannot express. The `georeference#GEODETIC_TRANSFORM` leg reads a geometry's `CoordinateSequence`, batches the ordinates through `MathTransform` (or, on escalation, a one-shot OSR `CoordinateTransformation` over the same ordinate columns), and the SRID stamped on the `GeometryFactory` records the target CRS. A long edge is `Densifier.Densify`-ed first so the reprojected curve tracks the datum. NTS never reprojects itself — it owns the planar algebra on both sides of the transform.
- wire seam: `Geometry.ToBinary()`/`WKBWriter` is the compact form the `wire#BIM_WIRE` projection carries for a `BimElement` planar footprint; the Python/TypeScript peers decode WKB through their own NTS-equivalent (`shapely`/`turf`).

[LOCAL_ADMISSION]:
- geometry construction enters through a `NtsGeometryServices.Instance`-resolved `GeometryFactory`; the global singleton is configured once at startup.
- predicates and boolean ops enter through the `Geometry` instance methods; the prepared/indexed forms (`PreparedGeometry`, `STRtree`) are used whenever one geometry faces many.
- validity repair enters through `GeometryFixer.Fix`; the rejected form is trusting raw OGR/shapefile rings into an overlay.
- WKT/WKB I/O enters through the `WKTReader`/`WKBReader`/`WKBWriter` family seeded with the canonical factory.

[RAIL_LAW]:
- Package: `NetTopologySuite` (+ transitive `NetTopologySuite.Features`)
- Owns: the OGC Simple-Features planar geometry algebra — type hierarchy, DE-9IM predicates, robust overlay/boolean, buffer/hull/simplify/densify, R-tree spatial index, prepared geometry, validity repair, WKT/WKB I/O, the attributed `Feature` shape
- Accept: planar topology, spatial predicates and joins, 2D boolean operations, footprint dissolve, geospatial broad/narrow-phase indexing
- Reject: geodetic datum/projection transformation (`ProjNET` owns it as default, `MaxRev.Gdal.Core` OSR as the exotic datum-grid escalation), raster I/O and universal vector drivers (`MaxRev.Gdal.Core` owns them), the shapefile/GeoPackage/GeoJSON byte format (the IO codec packages own them), 3D solid geometry and meshing (the kernel `Rasm` owns them)
