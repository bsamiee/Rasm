# [RASM_MATERIALS_API_TRIANGLE]

`Triangle` is the C# port of Jonathan Shewchuk's `Triangle` (the canonical Ruppert-refinement 2D quality mesher) — the `TriangleNet` managed library: a constrained/conforming Delaunay triangulator and quality mesh refiner over a Planar Straight-Line Graph (PSLG of vertices + segments + holes + regions), with selectable triangulation algorithms (`Dwyer` divide-and-conquer, `SweepLine` Fortune, `Incremental`), `RobustPredicates` adaptive-precision incircle/orient, Ruppert/Chew quality refinement under angle/area bounds, Laplacian smoothing, the Voronoi dual, mesh quality measures, and `.poly`/`.node`/`.ele` file I/O. The package has no direct central pin and resolves only as a transitive `Rasm.Materials` package through `VividOrange.InteractionDiagram`. The N-M-M capacity engine Triangle-meshes the concrete+rebar cross-section into the `AnalyticalFace` fibre grid the strain-plane sweep integrates over. The Materials folder mints no direct `Triangle` call; this catalog documents the surface the transitive `VividOrange` mesher drives.

The kernel relationship is COMPOSITIONAL and disjoint from the authored exact mesher: `Rasm`'s `.planning/Meshing/delaunay.md` authors its OWN exact-predicate Bowyer-Watson `Tessellation` (constrained Delaunay over a flat `SimplexStore`, robust via the four-tier `Adaptive.Resolve` ladder, 2D AND 3D). `Triangle` is the FLOAT-domain (`double`) production-grade 2D QUALITY refiner the authored kernel does not own — Ruppert/Chew angle-and-area-bounded refinement with Steiner-point insertion, the de-facto FEA section-meshing path. The two are layered: the authored kernel owns exact-arithmetic robustness and 3D; `Triangle` owns 2D quality refinement at production speed where the input is well-conditioned. Distinct from the in-folder `MIConvexHull` (UNCONSTRAINED point-set hull-lift Delaunay + ND Voronoi-dual, no segments/holes/quality) and the sibling-folder `SharpVoronoiLib` (2D point-site Fortune Voronoi with border clipping + Lloyd): `Triangle` is the only owner of CONSTRAINED + QUALITY-REFINED Delaunay over a PSLG.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Triangle`
- package: `Triangle`
- license: `MIT` (Triangle.NET / wo80; the wrapped Shewchuk core is non-commercial-research, the.NET port is MIT — `licenseUrl` `triangle.codeplex.com/license`)
- assembly: `Triangle`
- alias: none in repo-owned project files; `TriangleNet` is the package namespace, not a configured extern alias
- namespaces: `TriangleNet` (root: `Mesh`, `Configuration`, `RobustPredicates`, `MeshValidator`), `TriangleNet.Geometry` (`Polygon`/`Contour`/`Vertex`/`Point`/`Segment`/`Rectangle`/`RegionPointer`), `TriangleNet.Meshing` (`GenericMesher`/`ConstraintOptions`/`QualityOptions`/`Converter` + algorithm/iterator/interface subns), `TriangleNet.Voronoi` (`StandardVoronoi`/`BoundedVoronoi`), `TriangleNet.Topology` (+ `.DCEL`), `TriangleNet.Smoothing`, `TriangleNet.Tools`, `TriangleNet.IO`, `TriangleNet.Logging`
- asset: pure-managed AnyCPU IL (no native asset, no RID burden, ALC-safe, osx-arm64-safe)
- frameworks: multi-target `net35` + `net45` + `netstandard1.5` (three `lib/` assets); the `net10.0` consumer binds `lib/netstandard1.5/Triangle.dll` by TFM precedence (the highest netstandard available). The `netstandard1.5` asset pulls the `NETStandard.Library` reference-package meta-dependency (`[,)`) — a build-graph floor only, no runtime closure on net10
- owner: transitive `Rasm.Materials` package graph via `VividOrange.InteractionDiagram`
- rail: materials (transitive)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: PSLG input geometry — `TriangleNet.Geometry`
- rail: kernel-substrate

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:-------------- |:----------------- |:------------------------------------------------------------------------------------------- |
| [01] | `Polygon` | PSLG input | the input planar straight-line graph: `List<Vertex> Points`, `List<ISegment> Segments`, `List<Point> Holes`, `List<RegionPointer> Regions`, `HasPointMarkers`/`HasSegmentMarkers` flags; `AddContour(IEnumerable<Vertex>, marker, hole, convex)` is the primary build entry, `Add(Contour, hole)` / `Add(ISegment)` / `Add(Vertex)` the granular ones |
| [02] | `IPolygon` | input contract | the `Polygon` interface: `Points`/`Segments`/`Holes`/`Regions` lists + the marker flags — what `GenericMesher.Triangulate` accepts |
| [03] | `Contour` | boundary loop | one closed vertex loop: `List<Vertex> Points`, `GetSegments()` (loop -> segment list), `FindInteriorPoint(limit, eps)` (a point guaranteed inside the loop — the hole/region marker seed) |
| [04] | `Vertex` | input/mesh vertex | `: Point` with the mesh-attached identity; ctors `(x,y)` / `(x,y,mark)` — the PSLG point and the meshed-vertex carrier |
| [05] | `Point` | 2D point base | `double X`/`Y`, `int ID`/`Label`, `IComparable`/`IEquatable`, `==`/`!=` — the coordinate base of every vertex |
| [06] | `Segment` | constraint edge | `: ISegment` — an input constraint edge the mesher must preserve (a forced edge in the output) |
| [07] | `ISegment` | constraint contract | the segment interface (`: IEdge`) — endpoints by vertex index + label |
| [08] | `Edge` / `IEdge` | output edge | `int P0`/`P1`/`Label` — an output mesh edge by endpoint vertex index |
| [09] | `RegionPointer` | region marker | `(x, y, id)` — a seed point tagging a connected mesh region with an `id`, used with `ConstraintOptions.UseRegions` for per-region area constraints / attributes |
| [10] | `Rectangle` | bounds | the AABB: `Expand(Point)`/`Expand(points)`/`Expand(Rectangle)`, `Contains`/`Intersects`, `Resize` — the mesh bounds and the `StructuredMesh` extent |

[PUBLIC_TYPE_SCOPE]: meshing engine and options — `TriangleNet` / `TriangleNet.Meshing`
- rail: kernel-substrate

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------ |:----------------- |:------------------------------------------------------------------------------------------- |
| [01] | `GenericMesher` | mesher facade | the primary entry: `Triangulate(IList<Vertex>)` (point-set Delaunay), `Triangulate(IPolygon[, ConstraintOptions][, QualityOptions])` (constrained + quality), static `StructuredMesh(w,h,nx,ny)` / `StructuredMesh(Rectangle, nx, ny)` (regular grid); ctors take an `ITriangulator` and/or `Configuration` |
| [02] | `Mesh` | mesh result (`IMesh`) | the meshed result: `Refine(QualityOptions, delaunay)` (post-hoc requality), `Renumber([NodeNumbering])` (Cuthill-McKee bandwidth reduction for FEA assembly), the `IMesh` collections below |
| [03] | `IMesh` | result contract | `ICollection<Vertex> Vertices`, `IEnumerable<Edge> Edges`, `ICollection<SubSegment> Segments`, `ICollection<Triangle> Triangles`, `IList<Point> Holes`, `Rectangle Bounds` — the full output mesh surface the FEA grid reads |
| [04] | `ConstraintOptions` | constraint policy | `ConformingDelaunay` (Steiner points to enforce the empty-circumcircle property on constrained edges), `Convex` (mesh the convex hull, ignore concavities), `UseRegions` (honour `RegionPointer` attributes/area bounds), `SegmentSplitting` (the constrained-segment subdivision policy) |
| [05] | `QualityOptions` | refinement policy | `MinimumAngle` (the Ruppert/Chew lower-angle bound — the headline quality knob), `MaximumAngle`, `MaximumArea` (the global element-area cap), `VariableArea` (per-region area from `RegionPointer`), `SteinerPoints` (the insertion budget cap, -1 = unbounded), `Func<ITriangle,double,bool> UserTest` (a custom per-triangle refine predicate — the FEA gradient-driven sizing hook) |
| [06] | `Converter` | mesh<->DCEL bridge | static `ToMesh(Polygon, IList<ITriangle>)` / `ToMesh(Polygon, ITriangle[])` (rebuild a `Mesh` from a triangle list) and `ToDCEL(Mesh)` (to the half-edge `DcelMesh` the Voronoi consumes) |
| [07] | `MeshValidator` | validation | the static post-mesh topological/Delaunay invariant checker |
| [08] | `Configuration` | DI configuration | the predicate/factory injection point (`IPredicates`, vertex/triangle factories) — defaults to `RobustPredicates`; the seam for swapping the adaptive-precision predicate kernel |

[PUBLIC_TYPE_SCOPE]: triangulation algorithms, topology, predicates — `TriangleNet.Meshing.Algorithm` / `.Topology`
- rail: kernel-substrate

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:-------------------- |:----------------- |:------------------------------------------------------------------------------------------- |
| [01] | `ITriangulator` | algorithm contract | the pluggable initial-triangulation strategy `GenericMesher` accepts |
| [02] | `Dwyer` | algorithm | the divide-and-conquer alternating-cut triangulator — the default, fastest for general point sets |
| [03] | `SweepLine` | algorithm | Fortune's sweepline triangulator |
| [04] | `Incremental` | algorithm | incremental insertion — the simplest, useful for staged/online point addition |
| [05] | `RobustPredicates` | predicate impl | `: IPredicates` — Shewchuk's adaptive-precision exact `InCircle`/`CounterClockwise`(orient2d)/`Orient3d` (cf. `Statistic.InCircleCount`/`InCircleAdaptCount` counters); the `double`-domain robustness floor of the mesher |
| [06] | `IPredicates` | predicate contract | the orient/incircle predicate interface — swappable via `Configuration` |
| [07] | `Triangle` | output triangle | `TriangleNet.Topology.Triangle: ITriangle` — `int ID`/`Label`, `double Area`, `GetVertex(i)`/`GetVertexID(i)`/`GetNeighbor(i)`/`GetNeighborID(i)`/`GetSegment(i)` (the mesh-connectivity accessors the FEA assembly walks) |
| [08] | `ITriangle` | triangle contract | `int ID`/`Label`, `double Area`, `GetVertexID(i)`/`GetNeighborID(i)` — the output-triangle interface |
| [09] | `VertexType` | vertex kind | `InputVertex`, `SegmentVertex`, `FreeVertex`, `DeadVertex`, `UndeadVertex` — the mesh-vertex classification |
| [10] | `NodeNumbering` | renumber policy | `None`, `Linear`, `CuthillMcKee` — the `Mesh.Renumber` bandwidth-reduction ordering |

[PUBLIC_TYPE_SCOPE]: Voronoi dual, smoothing, quality tools — `TriangleNet.Voronoi` / `.Smoothing` / `.Tools`
- rail: kernel-substrate

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:--------------------- |:----------------- |:------------------------------------------------------------------------------------------- |
| [01] | `StandardVoronoi` | voronoi (`VoronoiBase`) | the unbounded Voronoi dual: `StandardVoronoi(Mesh)` / `(Mesh, Rectangle)` / `(Mesh, Rectangle, IVoronoiFactory, IPredicates)` — the half-edge `DcelMesh` dual of the Delaunay mesh, optionally clipped to a box |
| [02] | `BoundedVoronoi` | voronoi (`VoronoiBase`) | the bounded Voronoi: `BoundedVoronoi(Mesh)` / `(Mesh, IVoronoiFactory, IPredicates)` — clipped to the input mesh convex hull |
| [03] | `VoronoiBase` / `DcelMesh` | dual base / DCEL | the abstract Voronoi base and the doubly-connected-edge-list mesh (`Face`/`HalfEdge`/`Vertex`) the dual is expressed in |
| [04] | `SimpleSmoother` | mesh smoother | `: ISmoother` Laplacian/CVT smoothing: `Smooth(IMesh)` / `Smooth(IMesh, limit)` — the post-mesh quality-improving vertex relaxation (uses an internal Voronoi factory) |
| [05] | `QualityMeasure` | quality metrics | `Update(Mesh, sampleDegrees)` then read per-element `Area` min/max/total and `alpha` (min-angle quality) statistics — the mesh-quality acceptance metric the FEA gate reads |
| [06] | `Statistic` | algorithm counters | static `InCircleCount`/`InCircleAdaptCount`/`CounterClockwiseCount`/`Orient3dCount`/`CircumcenterCount`/`RelocationCount` — the predicate-call and adaptive-refine counters (the robustness/perf receipt) |
| [07] | `IntersectionHelper` | segment geometry | static `IntersectSegments(p0,p1,q0,q1,ref c0)`, `LiangBarsky(rect, p0, p1, ref c0, ref c1)` (segment-rect clip), `BoxRayIntersection` — the segment/box clipping utilities |
| [08] | `Interpolation` | scalar interp | static barycentric/natural-neighbour scalar interpolation over the mesh — the field-sampling helper |
| [09] | `TriangleQuadTree` / `AdjacencyMatrix` / `CuthillMcKee` | spatial / sparse | the point-location quadtree, the sparse adjacency matrix, and the Cuthill-McKee reordering engine `Mesh.Renumber` drives |

[PUBLIC_TYPE_SCOPE]: file I/O — `TriangleNet.IO`
- rail: kernel-substrate

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:-------------------------------- |:----------------- |:------------------------------------------------------------------------- |
| [01] | `FileProcessor` | I/O facade | the static read/write dispatch over the registered `IFileFormat`s |
| [02] | `TriangleReader` / `TriangleWriter` | `.poly`/`.node`/`.ele` | the native Triangle file-format reader/writer (PSLG in, mesh out) |
| [03] | `TriangleFormat` | format | `: IPolygonFormat, IMeshFormat` — the `.poly`/`.ele` format binding |
| [04] | `InputTriangle` | I/O triangle | `: ITriangle` — the deserialized triangle carrier |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the canonical mesh pipeline — `GenericMesher.Triangulate`
- rail: kernel-substrate
- note: the pipeline is BUILD `Polygon` (`AddContour` outer + holes + regions) -> `GenericMesher.Triangulate(polygon, constraintOptions, qualityOptions)` -> read the `IMesh` collections. The four `Triangulate(IPolygon,...)` overloads layer the optional constraint and quality policies; the `Triangulate(IList<Vertex>)` overload is the unconstrained point-set Delaunay (no PSLG).

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------------------------------- |:------------- |:--------------------------------------------------------------------------- |
| [01] | `GenericMesher.Triangulate(IList<Vertex> points)` | point Delaunay | the unconstrained Delaunay of a point set (no segments) — the bare hull-fill |
| [02] | `GenericMesher.Triangulate(IPolygon polygon)` | constrained | the constrained Delaunay of the PSLG (segments preserved, holes carved), no quality refinement |
| [03] | `GenericMesher.Triangulate(IPolygon polygon, ConstraintOptions options)` | constrained+ | constrained Delaunay with conforming/convex/region policy |
| [04] | `GenericMesher.Triangulate(IPolygon polygon, QualityOptions quality)` | refined | constrained + Ruppert/Chew quality refinement (min-angle / max-area Steiner insertion) |
| [05] | `GenericMesher.Triangulate(IPolygon polygon, ConstraintOptions options, QualityOptions quality)` | full | the full pipeline: constrained + conforming + region-aware + quality-refined — the FEA section-mesh entry |
| [06] | `GenericMesher.StructuredMesh(double w, double h, int nx, int ny)` / `StructuredMesh(Rectangle, nx, ny)` | structured | a regular nx-by-ny grid mesh (no PSLG) — the quick rectangular-domain mesh |
| [07] | `Mesh.Refine(QualityOptions quality, bool delaunay = false)` | re-refine | post-hoc requality of an existing mesh (e.g. adaptive FEA refinement after an error estimate) |
| [08] | `Mesh.Renumber([NodeNumbering num])` | renumber | Cuthill-McKee node renumbering for FEA stiffness-matrix bandwidth reduction |

## [04]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:
- Coordinates are `double` (`Point.X`/`Y`); `Triangle` is a FLOATING-POINT mesher with `RobustPredicates` adaptive-precision orient/incircle — robust to ROUNDING but NOT an exact-rational kernel. For exact-arithmetic robustness (degenerate cocircular input, exact constrained-edge recovery) the consumer routes through the `Rasm` kernel's `.planning/Meshing/delaunay.md` `Adaptive.Resolve` four-tier Bowyer-Watson; `Triangle` is the production 2D quality path where the input is well-conditioned.
- `QualityOptions.MinimumAngle` is the Ruppert termination bound: angles above ~ deg are guaranteed to terminate; pushing it higher risks non-termination on tight input angles — bound `SteinerPoints` to cap the insertion budget. `MaximumArea` is the global element-size cap; `VariableArea` + `RegionPointer` give per-region sizing; `UserTest` (`Func<ITriangle,double,bool>`) is the custom refine predicate (return true to split — the FEA gradient/error-driven sizing hook).
- `ConstraintOptions.ConformingDelaunay` inserts Steiner points so every triangle's circumcircle is empty even across constrained edges (the true Delaunay property); without it the result is a CONSTRAINED Delaunay (edges forced, circumcircles not guaranteed empty). `Convex` meshes the convex hull ignoring concavities; `UseRegions` honours `RegionPointer` ids for per-region attributes/area.
- Holes are carved by a `Point` seed INSIDE the hole loop (`Contour.FindInteriorPoint` produces one); the mesher floods the unbounded region from each hole seed and deletes those triangles. Regions are tagged the same way via `RegionPointer`.

[STACKING_LAW]:
- VividOrange.InteractionDiagram (the Materials transitive consumer): the N-M-M capacity engine builds a `Polygon` from the concrete outer perimeter (outer `Contour`) + the rebar exclusion/void contours, runs `GenericMesher.Triangulate(polygon, qualityOptions)` to produce the `AnalyticalFace` fibre grid, then sweeps strain planes and integrates fibre stress over the meshed faces to emit the capacity surface (`MIConvexHull` then welds the resulting force-moment-moment cloud into the 3D onion). `Triangle` and `MIConvexHull` thus STACK inside the one capacity engine: `Triangle` meshes the SECTION (constrained PSLG -> fibre grid), `MIConvexHull` hulls the RESULT (N-M-M points -> capacity onion). The `Rasm.Materials` design composes the welded `IForceMomentMesh`, never either primitive directly.
- direct-use status: no repo-owned project directly references `Triangle`; exact-robustness and 3D concerns stay on the authored Bowyer-Watson + `Arrangement`.
- Disjoint from `MIConvexHull` and `SharpVoronoiLib`: `MIConvexHull` owns the UNCONSTRAINED point-set hull-lift Delaunay + ND Voronoi-dual (no segments/holes/quality refinement). `SharpVoronoiLib` owns the 2D point-site Fortune Voronoi with border clipping + Lloyd relaxation as a STANDALONE diagram. `Triangle` owns CONSTRAINED + QUALITY-REFINED Delaunay over a PSLG AND its own `StandardVoronoi`/`BoundedVoronoi` dual computed FROM the Delaunay mesh — a consumer needing the Voronoi dual OF a constrained mesh uses `Triangle`'s, one needing a standalone clipped point-site diagram uses `SharpVoronoiLib`.

[RAIL_LAW]:
- Package: `Triangle` (assembly `Triangle`, alias `TriangleNet`)
- Owns: constrained/conforming 2D Delaunay triangulation and Ruppert/Chew quality refinement over a PSLG (`Polygon` of vertices/segments/holes/regions), with pluggable `Dwyer`/`SweepLine`/`Incremental` initial triangulation, `RobustPredicates` adaptive-precision orient/incircle, Laplacian `SimpleSmoother`, the `StandardVoronoi`/`BoundedVoronoi` dual, `QualityMeasure`/`Statistic` metrics, Cuthill-McKee renumbering, and `.poly`/`.node`/`.ele` I/O — all in the `double` domain
- Accept: the transitive `VividOrange.InteractionDiagram` section fibre-grid mesh
- Reject: direct `Triangle` calls minted in `Rasm.Materials`; direct repo-owned references without a central package admission row; using `Triangle` where exact-rational robustness on degenerate input is required (route to the kernel `Adaptive.Resolve` Bowyer-Watson) or where 3D tetrahedralization is needed (`Triangle` is 2D-only — the kernel owns 3D); duplicating the unconstrained point-set Delaunay/ND-Voronoi (`MIConvexHull` owns it) or a standalone clipped point-site Voronoi (`SharpVoronoiLib` owns it)
