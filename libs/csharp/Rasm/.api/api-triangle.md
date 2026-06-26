# [RASM_API_TRIANGLE]

`Triangle` (Triangle.NET — the C# port of Shewchuk's Triangle) is a 2D quality constrained
Delaunay triangulator and mesh generator. It takes a point set OR a planar straight-line graph
(PSLG — a `Polygon` of `Contour` boundaries with holes and regions) and produces an `IMesh` of
triangles satisfying optional quality constraints (minimum angle, maximum area, conforming
Delaunay), through ONE polymorphic entry — `GenericMesher.Triangulate` / the `IPolygon.Triangulate`
extension — that discriminates on input shape (points vs polygon) and option presence. The
triangulation algorithm is a swappable `ITriangulator` (Dwyer divide-and-conquer / sweep-line /
incremental), the geometric predicates a swappable `IPredicates` (the bundled adaptive-exact
`RobustPredicates` by default), both injected through a `Configuration` — a genuine DI seam that
lets the Rasm exact predicate floor replace the bundled predicates. On top it ships Ruppert/Chew
quality refinement (`Mesh.Refine`), Lloyd smoothing (`SimpleSmoother`), the Voronoi dual as a
DCEL half-edge mesh (`StandardVoronoi`/`BoundedVoronoi`), point-location (`TriangleQuadTree`),
PSLG and mesh validity oracles (`PolygonValidator`/`MeshValidator`), and mesh-quality statistics
(`QualityMeasure`). It is `double`-domain (NOT predicate-exact at the Rasm `Expansion`/`Fraction`
tier) and the package id `Triangle` differs from its `TriangleNet` namespace — the Rasm reference
binds it under `Aliases="TriangleNet"`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Triangle`
- package: `Triangle`
- version: `0.0.6-Beta3`
- license: MIT-style (Triangle.NET, Christian Woltering; the `triangle.codeplex.com/license` terms — the original Shewchuk Triangle is research-licensed, the .NET port is permissive)
- assembly: `Triangle.dll`; root namespace `TriangleNet` (the PACKAGE id `Triangle` differs from the NAMESPACE `TriangleNet` — the Rasm `<PackageReference Include="Triangle" Aliases="TriangleNet" />` binds the assembly under the `TriangleNet` extern alias to disambiguate a bare `Triangle` type and pin the using-root)
- namespace: `TriangleNet` (`Mesh`/`Configuration`/`Behavior`/`RobustPredicates`/`IPredicates`), `TriangleNet.Geometry` (`Polygon`/`Contour`/`Vertex`/`Point`/`ISegment`/`IPolygon`/`ITriangle`/`Rectangle`/`RegionPointer`), `TriangleNet.Meshing` (`GenericMesher`/`ITriangulator`/`IMesh`/`QualityOptions`/`ConstraintOptions`), `TriangleNet.Meshing.Algorithm` (`Dwyer`/`Incremental`/`SweepLine`), `TriangleNet.Smoothing` (`SimpleSmoother`/`ISmoother`), `TriangleNet.Voronoi` (`StandardVoronoi`/`BoundedVoronoi`/`VoronoiBase`), `TriangleNet.Tools` (`TriangleQuadTree`/`QualityMeasure`/`Interpolation`/`PolygonValidator`/`Statistic`), `TriangleNet.Topology.DCEL` (`DcelMesh`/`HalfEdge`/`Face`), `TriangleNet.IO` (the .node/.poly/.ele readers/writers)
- target: multi-target (`lib/net35`, `lib/net45`, `lib/netstandard1.5`); the `net10.0` consumer binds `lib/netstandard1.5` — the highest available is `netstandard1.5`, so the asset carries the `NETStandard.Library 1.5.0` framework reference and uses `List<T>`/`ICollection<T>` BCL collections (NO `Span<T>`, no modern numeric interfaces); it is the dated-but-stable netstandard1.5 ABI on every modern runtime
- asset: pure-managed runtime library, AnyCPU, no native runtime; `osx-arm64`-safe
- abi: plain reference types — `Vertex : Point : IComparable<Point>/IEquatable<Point>`; the mesh/options/mesher are `class`; NO `System.Numerics` / generic-math contract; double-precision coordinates throughout
- rail: 2D quality constrained Delaunay triangulation + meshing (`double`-domain)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the mesher entry + the PSLG input + the mesh output (namespace `TriangleNet.*`)
- rail: 2D constrained Delaunay meshing

The flow is: build a `Polygon` PSLG (or a `Vertex` list) -> `Triangulate` through `GenericMesher`
with optional `ConstraintOptions`/`QualityOptions` -> get an `IMesh` -> optionally `Refine`,
`Smooth`, or derive the Voronoi dual. The triangulation `ITriangulator` and the geometric
`IPredicates` are injected through `Configuration`.

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]                      | [CAPABILITY]                                                                                          |
| :-----: | :------------------ | :---------------------------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `GenericMesher`     | the polymorphic mesher entry        | `class`; `Triangulate(IList<Vertex>)` / `Triangulate(IPolygon[, ConstraintOptions][, QualityOptions])` — one entry discriminating on point-set vs PSLG vs option presence; `StructuredMesh` for a grid |
|  [02]   | `Polygon`           | the PSLG input (`: IPolygon`)       | `class`; `Add(Contour, hole)` / `Add(ISegment)` / `Add(Vertex)`; `Points`/`Segments`/`Holes`/`Regions` — the constrained-boundary input with holes and region markers |
|  [03]   | `Contour`           | a closed boundary loop              | `class`; `new Contour(IEnumerable<Vertex>[, marker][, convex])`; `GetSegments()`, `FindInteriorPoint()` (auto hole-seed point) |
|  [04]   | `Vertex` / `Point`  | a mesh node / a 2D coordinate       | `Vertex : Point`; `Point` carries `X`/`Y`/`ID`/`Label`; `Vertex` adds `Type` (`VertexType`) and a boundary marker |
|  [05]   | `IMesh`             | the triangulation result            | `interface`; `Triangles`/`Vertices`/`Edges`/`Segments`/`Holes`/`Bounds` + `Renumber()` + `Refine(QualityOptions, delaunay)` — the mesh the consumer reads |
|  [06]   | `ITriangle`         | a mesh cell                         | `interface`; `GetVertex(i)`/`GetVertexID(i)`/`GetNeighbor(i)`/`GetSegment(i)`/`Area`/`Label`/`ID` — the per-triangle topology readout |
|  [07]   | `QualityOptions`    | refinement quality constraints      | `class`; `MinimumAngle`/`MaximumAngle`/`MaximumArea`/`VariableArea`/`SteinerPoints`/`UserTest` — the Ruppert/Chew refinement target |
|  [08]   | `ConstraintOptions` | segment/region constraints          | `class`; `ConformingDelaunay`/`Convex`/`UseRegions`/`SegmentSplitting` — the constrained-Delaunay behavior |
|  [09]   | `ITriangulator`     | the triangulation-algorithm seam    | `interface Triangulate(IList<Vertex>, Configuration)`; impls `Dwyer` (divide-and-conquer, default), `SweepLine` (Fortune), `Incremental` — pluggable into `GenericMesher` |
|  [10]   | `IPredicates` / `RobustPredicates` | the geometric-predicate seam | `interface`; `CounterClockwise(a,b,c)`/`InCircle(a,b,c,p)`/`FindCircumcenter(...)`; `RobustPredicates.Default` is the bundled adaptive-exact (Shewchuk) impl — replaceable via `Configuration` |
|  [11]   | `Configuration`     | the mesher DI container             | `class`; `Func<IPredicates> Predicates` + `Func<TrianglePool> TrianglePool` — injects the predicate impl and the triangle allocator |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `GenericMesher` / `IPolygon.Triangulate` — the polymorphic triangulation entry
- rail: 2D constrained Delaunay meshing

One entry discriminates: a `Vertex` list yields an unconstrained Delaunay triangulation; an
`IPolygon` yields a constrained Delaunay respecting the boundary segments; adding
`ConstraintOptions` selects conforming/convex behavior; adding `QualityOptions` runs quality
refinement during meshing. The `IPolygon.Triangulate` extension is the fluent form (and the only
one taking an explicit `ITriangulator`).

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `new GenericMesher()` / `(ITriangulator)` / `(Configuration)` / `(ITriangulator, Configuration)` | constructor | the mesher, optionally with a chosen algorithm and/or DI configuration |
|  [02]   | `GenericMesher.Triangulate(IList<Vertex> points)`                          | instance       | unconstrained Delaunay triangulation of a point set |
|  [03]   | `GenericMesher.Triangulate(IPolygon polygon[, ConstraintOptions][, QualityOptions])` | instance | constrained Delaunay of a PSLG; with `QualityOptions` runs Ruppert/Chew refinement inline |
|  [04]   | `polygon.Triangulate([ConstraintOptions][, QualityOptions][, ITriangulator])` (extension) | instance (fluent) | the `IPolygon` extension form — the 5-arg overload injects the triangulation algorithm |
|  [05]   | `GenericMesher.StructuredMesh(double width, height, int nx, ny)` / `(Rectangle bounds, int nx, ny)` | static | a regular structured triangle grid (no Delaunay step) |

[ENTRYPOINT_SCOPE]: `Polygon` / `Contour` — the PSLG input construction
- rail: 2D constrained Delaunay meshing

A `Polygon` accumulates `Contour` boundary loops (outer boundary + holes), free `ISegment`
constraints, hole seed points, and `RegionPointer` region-attribute markers. `Contour.FindInteriorPoint`
computes a point strictly inside a loop — the seed a hole requires. Markers on vertices/segments
survive into the mesh for boundary-condition tagging.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `new Polygon([int capacity][, bool markers])`                              | constructor    | the PSLG accumulator (with marker tracking) |
|  [02]   | `Polygon.Add(Contour contour, bool hole = false)` / `Add(Contour, Point hole)` | instance | add a boundary loop as outer boundary or a hole (with an explicit hole seed) |
|  [03]   | `Polygon.Add(Vertex)` / `Add(ISegment[, bool insert])` / `Add(ISegment, int index)` | instance | add a free vertex / a constraint segment |
|  [04]   | `Polygon.Points` / `Segments` / `Holes` / `Regions` / `Bounds()`           | instance prop  | the accumulated vertices / segments / hole seeds / region markers / bounding box |
|  [05]   | `new Contour(IEnumerable<Vertex>[, int marker][, bool convex])`            | constructor    | a closed boundary loop from a vertex ring (marker tags its segments) |
|  [06]   | `Contour.GetSegments()` / `FindInteriorPoint(int limit = 5, double eps = 2E-05)` | instance | the loop's segments / an interior seed point (for hole marking) |
|  [07]   | `new Vertex(double x, double y[, int mark])` / `new Point(double x, double y[, int label])` | constructor | a mesh node / a labeled coordinate |
|  [08]   | `new RegionPointer(double x, double y, int id)`                            | constructor    | a region-attribute marker (tags the triangles of the enclosing region with `id`) |

[ENTRYPOINT_SCOPE]: `IMesh` / `Mesh.Refine` — the result + quality refinement
- rail: 2D constrained Delaunay meshing

The `IMesh` is the triangulation; `Refine` runs a second Ruppert/Chew pass to enforce tighter
quality bounds, inserting Steiner points. `QualityOptions.UserTest` is a per-triangle predicate
the refinement consults — the hook for a custom size field. `Renumber` compacts the vertex IDs.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `IMesh.Triangles` / `Vertices` / `Edges` / `Segments` / `Holes` / `Bounds` | instance prop  | the result triangles / nodes / edges / constraint segments / hole seeds / bounding box |
|  [02]   | `IMesh.Refine(QualityOptions quality, bool delaunay)` / `Mesh.Refine(QualityOptions, bool delaunay = false)` | instance | enforce tighter quality bounds (insert Steiner points); `delaunay=true` keeps the result conforming-Delaunay |
|  [03]   | `IMesh.Renumber()` / `Mesh.Renumber(NodeNumbering)`                        | instance       | compact / Cuthill-McKee-reorder the vertex numbering (bandwidth reduction for a downstream solver) |
|  [04]   | `QualityOptions.MinimumAngle` / `MaximumAngle` / `MaximumArea` / `VariableArea` / `SteinerPoints` | prop | the refinement targets (min/max angle, max triangle area, per-region variable area, Steiner-point cap) |
|  [05]   | `QualityOptions.UserTest` (`Func<ITriangle, double, bool>`)               | prop (delegate) | a per-triangle "needs refinement" predicate — the custom size-field / grading hook |
|  [06]   | `ConstraintOptions.ConformingDelaunay` / `Convex` / `UseRegions` / `SegmentSplitting` | prop | conforming-Delaunay enforcement / convex-hull fill / region-attribute propagation / segment-split policy |
|  [07]   | `ITriangle.GetVertex(int i)` / `GetVertexID(int i)` / `GetNeighbor(int i)` / `GetSegment(int i)` / `Area` / `Label` | instance | per-triangle vertices, adjacency, constraint segments, area, and region label |

[ENTRYPOINT_SCOPE]: the algorithm, predicate, smoothing, and analysis surface
- rail: 2D constrained Delaunay meshing

The triangulation algorithm and the geometric predicates are injected; `SimpleSmoother` is Lloyd
relaxation over the mesh; the Voronoi dual is a DCEL; `TriangleQuadTree` is point-location;
`PolygonValidator`/`MeshValidator` are the input/output validity oracles; `QualityMeasure` is the
mesh-quality statistic.

| [INDEX] | [SURFACE]                                                                  | [CALL_SHAPE]   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `new Dwyer()` / `new SweepLine()` / `new Incremental()` (`: ITriangulator`) | constructor   | the three triangulation strategies — Dwyer divide-and-conquer (default), Fortune sweep-line, incremental insertion |
|  [02]   | `RobustPredicates.Default` / `new RobustPredicates()` / `.CounterClockwise(a,b,c)` / `.InCircle(a,b,c,p)` / `.NonRegular(a,b,c,p)` / `.FindCircumcenter(org,dest,apex, ref xi, ref eta[, offconstant])` | static/instance | the bundled Shewchuk adaptive-exact orientation / in-circle / weighted in-circle / circumcenter predicates |
|  [03]   | `new Configuration([Func<IPredicates>][, Func<TrianglePool>])` / `.Predicates` / `.TrianglePool` | constructor/prop | inject a custom `IPredicates` (e.g. the Rasm exact predicate floor) and the triangle allocator |
|  [04]   | `new SimpleSmoother([IVoronoiFactory][, Configuration])` (`: ISmoother`) / `.Smooth(IMesh[, int limit])` | constructor/instance | Lloyd relaxation — move interior vertices to cell centroids for a better-conditioned mesh |
|  [05]   | `new StandardVoronoi(Mesh[, Rectangle box][, IVoronoiFactory, IPredicates])` / `new BoundedVoronoi(Mesh)` (`: VoronoiBase : DcelMesh`) | constructor | the VORONOI DUAL of the Delaunay mesh as a DCEL half-edge mesh (`Vertices`/`HalfEdges`/`Faces`) |
|  [06]   | `new TriangleQuadTree(Mesh[, int maxDepth = 10][, int sizeBound = 10])` / `.FindTriangles(Point)` | constructor/instance | quadtree point-location — which triangle(s) contain a query point |
|  [07]   | `PolygonValidator.IsConsistent(IPolygon)` / `HasDuplicateVertices(IPolygon)` / `HasBadAngles(IPolygon, double threshold)` | static (`bool`) | PSLG-input validity oracles (run BEFORE triangulating) |
|  [08]   | `MeshValidator.IsConsistent(Mesh)` / `IsDelaunay(Mesh)` / `IsConstrainedDelaunay(Mesh)` | static (`bool`) | mesh-output validity oracles — the Delaunay/constrained-Delaunay property check (a differential cross-check) |
|  [09]   | `new QualityMeasure()` / `.Measure(Point a, b, c)` + the `alpha_min/max/ave` + `q_min/max` shape-ratio fields | instance | per-triangle and mesh-wide quality statistics (angle ratio α, radius ratio q) |
|  [10]   | `Statistic.InCircleCount` / `CounterClockwiseCount` / `Orient3dCount` / `CircumcenterCount` (+ `*AdaptCount`) | static field | the global predicate-call counters (adaptive vs exact-branch invocation counts — a robustness/perf probe) |
|  [11]   | `ITriangle.Contains(Point)` / `Contains(double x, double y)` (extension)    | instance       | point-in-triangle test (the `TriangleNet.Geometry.ExtensionMethods` companion to `IPolygon.Triangulate`) |

## [04]-[IMPLEMENTATION_LAW]

[MESHER_PROFILE]:
- single polymorphic entry: `GenericMesher.Triangulate` (and the `IPolygon.Triangulate` extension) is the one entry — input shape (point set vs PSLG) and option presence (`ConstraintOptions`/`QualityOptions`) discriminate behavior, never a `TriangulatePoints`/`TriangulatePolygon`/`TriangulateWithQuality` family. The two injectable seams (`ITriangulator`, `IPredicates`) ride `Configuration`, not a per-variant entry.
- domain: `double` coordinates with the bundled `RobustPredicates` (Shewchuk adaptive-exact orientation/in-circle). The predicates are adaptive-EXACT for the SIGN (so the combinatorial Delaunay structure is robust), but the COORDINATES (circumcenters, Steiner points) are `double` — this is NOT the Rasm `Expansion`/`Fraction` exact-coordinate tier. For a triangulation whose vertex coordinates must be exact rationals, the in-house `Meshing/delaunay` over the predicate floor owns it; Triangle.NET is the production `double`-domain quality mesher.
- predicate injection: `Configuration(Func<IPredicates>)` lets a consumer SWAP `RobustPredicates` for a custom `IPredicates` — the seam through which the Rasm exact predicate floor (`Predicate.Orient2D`/`InCircle`) can drive the sign decisions while Triangle.NET owns the mesh combinatorics, marrying the exact floor to the mature mesher.
- quality model: `QualityOptions` drives Ruppert's refinement (minimum-angle bound via Steiner-point insertion) and Chew's second algorithm; `UserTest` is the per-triangle refinement predicate (the custom size-field hook); `MaximumArea`/`VariableArea` bound triangle size globally or per region.
- Voronoi dual: `StandardVoronoi`/`BoundedVoronoi` derive the Voronoi as a DCEL half-edge mesh from the Delaunay triangulation — a DIFFERENT representation than `SharpVoronoiLib`'s site/edge/cell model (`api-sharpvoronoilib`): TriangleNet's dual is a topological DCEL keyed off the triangulation, SharpVoronoiLib's is a Fortune diagram with box-clipped closed cells.

[LOCAL_ADMISSION]:
- `Triangle` (Triangle.NET) is the constrained/conforming Delaunay triangulation + quality-meshing substrate PRIMARILY OWNED by the Rasm geometry kernel (per the README roster). It discretizes a 2D PSLG into a quality triangle mesh — the production `double`-domain path the README cites as the capacity-surface section mesher (TriangleNet-meshes a concrete+rebar section into `AnalyticalFaces`) and the clean-PSLG meshing path additive beside the in-house exact `Meshing/delaunay`.
- the `Aliases="TriangleNet"` extern alias on the Rasm reference is load-bearing: the package id `Triangle` differs from the namespace `TriangleNet`, and the alias both pins the using-root and forecloses any collision with another `Triangle`-named type on the graph — internal code references `extern alias TriangleNet; using TriangleNet.Meshing;`. (This is a GENUINE extern-alias case, unlike the `SharpVoronoiLib`/`api-kdtree` kd-tree pair, whose bundled `Supercluster.KDTree.KDTree` and standalone `SuperClusterKDTree.KDTree` are distinct FQNs needing no alias.)
- the PSLG path is the primary use: build a `Polygon` of `Contour` loops (outer + holes via `Contour.FindInteriorPoint` seeds), constrain with `ConstraintOptions.ConformingDelaunay`, refine with `QualityOptions.MinimumAngle`/`MaximumArea`, read the `IMesh.Triangles` with per-cell `ITriangle.Label` region tags.
- this is `double`-domain meshing — NOT the predicate-exact-COORDINATE tier; the exact-sign `RobustPredicates` keep the combinatorics robust, but a design that needs exact-RATIONAL vertex coordinates stays on the in-house `Meshing/delaunay`.

[STACKING_LAW]:
- vs the in-house exact Delaunay (`Meshing/delaunay`): the in-house Bowyer-Watson runs on the Rasm `Expansion`/`Fraction` exact-coordinate floor; Triangle.NET runs `double` coordinates with exact-SIGN predicates. The two are precision tiers — Triangle.NET for the production quality mesh where `double` coordinates suffice, the in-house path where vertex coordinates must be exact. Better still, `Configuration(Func<IPredicates>)` lets the Rasm exact predicates DRIVE Triangle.NET's sign decisions, composing the exact floor with the mature mesher rather than choosing one.
- vs SharpVoronoiLib (`api-sharpvoronoilib`): both produce a Voronoi diagram, in DIFFERENT representations — TriangleNet's `StandardVoronoi` is the DCEL dual of its Delaunay triangulation (topological, half-edge), SharpVoronoiLib's is a Fortune diagram with box-clipped closed cells and site/edge adjacency. TriangleNet's strength is the triangulation (it gives the Voronoi as a by-product); SharpVoronoiLib's is the Voronoi (closed cells + Lloyd). Use TriangleNet when the triangle mesh is the goal, SharpVoronoiLib when the closed Voronoi cells are.
- vs MIConvexHull (`api-miconvexhull`): `MIConvexHull` is the N-dimensional incremental hull/Delaunay/Voronoi over a paraboloid lift (no constraints, no quality, no PSLG); Triangle.NET is 2D-only but adds CONSTRAINED boundaries, hole/region markers, quality refinement, and smoothing. Complementary: `MIConvexHull` for N-D unconstrained Delaunay, Triangle.NET for 2D constrained quality meshing.
- vs LibTessDotNet (`api-libtess`): `LibTessDotNet` is a winding-rule polygon TESSELLATOR (fill arbitrary self-intersecting/holey contours into triangles, no quality guarantee) — the messy-input fill leg; Triangle.NET is a quality MESHER on a clean PSLG (angle/area bounds, conforming Delaunay) — the FEM/analysis-grade mesh leg. A self-intersecting input goes to `LibTessDotNet`; a clean PSLG needing a quality mesh goes to Triangle.NET.
- vs Clipper2's ear-clip (`api-clipper2`): Clipper2 ships `Clipper.Triangulate(Paths64, out Paths64, useDelaunay=true)` → `TriangulateResult` — an integer-exact EAR-CLIP with an optional Delaunay edge-FLIP on an ALREADY-SIMPLE `Paths64` (it rejects a self-intersecting input as `pathsIntersect`, never refines). Triangle.NET is the constrained-DELAUNAY quality mesher: it accepts boundary/hole CONSTRAINTS and angle/area QUALITY targets and inserts Steiner points; Clipper's ear-clip does neither. These two plus `LibTessDotNet` (winding-rule fill) are the three distinct polygon-triangulation owners — integer ear-clip (`Clipper.Triangulate`) for a clean boolean result already in the `Point64` lattice, winding-rule fill (`LibTessDotNet`) for arbitrary self-intersecting input, constrained-quality mesh (Triangle.NET) for an FEM-grade Delaunay; a concern lands on exactly one.
- validity cross-check: `MeshValidator.IsDelaunay`/`IsConstrainedDelaunay` is an independent oracle that VERIFIES a mesh satisfies the (constrained-)Delaunay property — a differential check the in-house mesher's law-matrix can run TriangleNet against (does the exact mesher's output pass TriangleNet's Delaunay test, and vice versa), and `Statistic.InCircleCount`/`*AdaptCount` exposes how often the adaptive predicate escalated to the exact branch.

[RAIL_LAW]:
- Package: `Triangle` (Triangle.NET; namespace `TriangleNet`, bound `Aliases="TriangleNet"`)
- Owns: 2D quality constrained Delaunay triangulation + meshing — the polymorphic `GenericMesher.Triangulate` / `IPolygon.Triangulate` entry, the `Polygon`/`Contour`/`Vertex` PSLG input with holes and region markers, the `IMesh`/`ITriangle` result, `ConstraintOptions` (conforming/convex/regions) + `QualityOptions` (angle/area bounds + `UserTest` size field) refinement, the `Dwyer`/`SweepLine`/`Incremental` triangulator + `RobustPredicates`/`IPredicates` predicate seams injected via `Configuration`, `SimpleSmoother` Lloyd smoothing, the `StandardVoronoi`/`BoundedVoronoi` DCEL Voronoi dual, `TriangleQuadTree` point-location, `PolygonValidator`/`MeshValidator` validity oracles, and `QualityMeasure`/`Statistic` quality + predicate-count statistics
- Accept: a `double`-domain constrained Delaunay quality mesh of a clean PSLG (`Polygon` of `Contour` loops + holes + region markers); inline Ruppert/Chew refinement via `QualityOptions`; a custom size field via `QualityOptions.UserTest`; the Rasm exact predicate floor injected via `Configuration(Func<IPredicates>)`; the Voronoi dual via `StandardVoronoi`; point-location via `TriangleQuadTree`; a differential Delaunay-property cross-check via `MeshValidator`
- Reject: an exact-RATIONAL-coordinate triangulation (this is `double` coordinates with exact-SIGN predicates — the in-house `Meshing/delaunay` owns exact coordinates); a self-intersecting/messy-winding fill (that is `api-libtess` `LibTessDotNet`, not a clean-PSLG quality mesher); an integer-exact ear-clip of an already-simple `Paths64` with no quality/constraint need (that is `Clipper.Triangulate`, `api-clipper2` — Triangle.NET is the constrained-quality leg of the three-owner polygon-triangulation seam); referencing the assembly WITHOUT the `TriangleNet` extern alias (the package id `Triangle` ≠ the namespace `TriangleNet`, and the alias forecloses a `Triangle`-type collision); a `Triangulate{Points,Polygon,WithQuality}` operation family (one polymorphic entry discriminates on input shape + option presence); a `Span<T>`/modern-numeric-interface expectation (netstandard1.5 asset — `List<T>`/`IComparable<Point>` only); confusing TriangleNet's DCEL `StandardVoronoi` dual with the SharpVoronoiLib closed-cell Fortune diagram (`api-sharpvoronoilib`)
