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

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]       | [CAPABILITY]    |
| :-----: | :-------------- | :------------------ | :-------------- |
|  [01]   | `Polygon`       | PSLG input          | graph builder   |
|  [02]   | `IPolygon`      | input contract      | mesher input    |
|  [03]   | `Contour`       | boundary loop       | loop geometry   |
|  [04]   | `Vertex`        | input/mesh vertex   | mesh vertex     |
|  [05]   | `Point`         | 2D point base       | coordinates     |
|  [06]   | `Segment`       | constraint edge     | forced edge     |
|  [07]   | `ISegment`      | constraint contract | segment access  |
|  [08]   | `Edge`          | output edge         | concrete edge   |
|  [09]   | `IEdge`         | output contract     | indexed edge    |
|  [10]   | `RegionPointer` | region marker       | regional sizing |
|  [11]   | `Rectangle`     | bounds              | mesh extent     |

[Polygon]:

- Shape: the planar straight-line graph carries `List<Vertex> Points`, `List<ISegment> Segments`, `List<Point> Holes`, `List<RegionPointer> Regions`, and the `HasPointMarkers`/`HasSegmentMarkers` flags.
- Build: `AddContour(IEnumerable<Vertex>, marker, hole, convex)` is the primary entry; `Add(Contour, hole)`, `Add(ISegment)`, and `Add(Vertex)` are the granular entries.

[IPolygon]: the `Polygon` contract exposes the point, segment, hole, and region lists plus the marker flags accepted by `GenericMesher.Triangulate`.

[Contour]:

- Shape: `List<Vertex> Points` carries one closed vertex loop, and `GetSegments()` projects its segment list.
- Interior: `FindInteriorPoint(limit, eps)` returns a point inside the loop for a hole or region marker seed.

[Vertex]: derives from `Point`, carries mesh-attached identity, and exposes `(x, y)` and `(x, y, mark)` constructors for PSLG and meshed vertices.

[Point]: `double X`/`Y`, `int ID`/`Label`, `IComparable`/`IEquatable`, and `==`/`!=` form the coordinate base of every vertex.

[Segment]: implements `ISegment` as an input constraint edge preserved as a forced output edge.

[ISegment]: derives from `IEdge` and exposes endpoints by vertex index plus label.

[Edge]: carries a concrete output edge.

[IEdge]: `int P0`/`P1`/`Label` identify an output mesh edge by endpoint vertex index.

[RegionPointer]: `(x, y, id)` seeds a connected mesh region; `ConstraintOptions.UseRegions` applies the `id` to per-region attributes and area constraints.

[Rectangle]: `Expand(Point)`, `Expand(points)`, `Expand(Rectangle)`, `Contains`, `Intersects`, and `Resize` own the mesh AABB and `StructuredMesh` extent.

[PUBLIC_TYPE_SCOPE]: meshing engine and options — `TriangleNet` / `TriangleNet.Meshing`

- rail: kernel-substrate

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [CAPABILITY]        |
| :-----: | :------------------ | :---------------- | :------------------ |
|  [01]   | `GenericMesher`     | mesher facade     | triangulation entry |
|  [02]   | `Mesh`              | mesh result       | postprocessing      |
|  [03]   | `IMesh`             | result contract   | mesh collections    |
|  [04]   | `ConstraintOptions` | constraint policy | constraint controls |
|  [05]   | `QualityOptions`    | refinement policy | quality controls    |
|  [06]   | `Converter`         | mesh/DCEL bridge  | topology conversion |
|  [07]   | `MeshValidator`     | validation        | invariant checker   |
|  [08]   | `Configuration`     | DI configuration  | predicate factories |

[GenericMesher]:

- Triangulate: `Triangulate(IList<Vertex>)` owns point-set Delaunay, and `Triangulate(IPolygon[, ConstraintOptions][, QualityOptions])` owns constrained quality meshing.
- Structured: static `StructuredMesh(w, h, nx, ny)` and `StructuredMesh(Rectangle, nx, ny)` create regular grids.
- Construction: constructors accept an `ITriangulator`, a `Configuration`, or both.

[Mesh]: implements `IMesh`; `Refine(QualityOptions, delaunay)` refines mesh quality, and `Renumber([NodeNumbering])` applies Cuthill-McKee bandwidth reduction for FEA assembly.

[IMesh]: `ICollection<Vertex> Vertices`, `IEnumerable<Edge> Edges`, `ICollection<SubSegment> Segments`, `ICollection<Triangle> Triangles`, `IList<Point> Holes`, and `Rectangle Bounds` form the FEA grid output.

[ConstraintOptions]:

- `ConformingDelaunay`: inserts Steiner points to enforce empty circumcircles across constrained edges.
- `Convex`: meshes the convex hull and ignores concavities.
- `UseRegions`: honors `RegionPointer` attributes and area bounds.
- `SegmentSplitting`: selects constrained-segment subdivision.

[QualityOptions]:

- Angles: `MinimumAngle` carries the Ruppert/Chew lower bound, and `MaximumAngle` carries the upper bound.
- Area: `MaximumArea` sets the global cap, and `VariableArea` reads per-region area from `RegionPointer`.
- Insertion: `SteinerPoints` caps insertion; `-1` is unbounded.
- Predicate: `Func<ITriangle, double, bool> UserTest` supplies per-triangle refinement for FEA gradient-driven sizing.

[Converter]: static `ToMesh(Polygon, IList<ITriangle>)` and `ToMesh(Polygon, ITriangle[])` rebuild a `Mesh`; `ToDCEL(Mesh)` produces the half-edge `DcelMesh` consumed by Voronoi surfaces.

[MeshValidator]: statically checks post-mesh topology and Delaunay invariants.

[Configuration]: injects `IPredicates` plus vertex and triangle factories, defaults predicates to `RobustPredicates`, and admits replacement of the adaptive-precision kernel.

[PUBLIC_TYPE_SCOPE]: triangulation algorithms, topology, predicates — `TriangleNet.Meshing.Algorithm` / `.Topology`

- rail: kernel-substrate

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]      | [CAPABILITY]          |
| :-----: | :----------------- | :----------------- | :-------------------- |
|  [01]   | `ITriangulator`    | algorithm contract | strategy injection    |
|  [02]   | `Dwyer`            | algorithm          | divide-and-conquer    |
|  [03]   | `SweepLine`        | algorithm          | sweepline             |
|  [04]   | `Incremental`      | algorithm          | incremental insertion |
|  [05]   | `RobustPredicates` | predicate impl     | adaptive precision    |
|  [06]   | `IPredicates`      | predicate contract | orientation tests     |
|  [07]   | `Triangle`         | output triangle    | mesh connectivity     |
|  [08]   | `ITriangle`        | triangle contract  | triangle access       |
|  [09]   | `VertexType`       | vertex kind        | vertex classification |
|  [10]   | `NodeNumbering`    | renumber policy    | bandwidth ordering    |

[ITriangulator]: supplies the initial-triangulation strategy accepted by `GenericMesher`.

[Dwyer]: is the default divide-and-conquer alternating-cut triangulator and the fastest strategy for general point sets.

[SweepLine]: implements Fortune's sweepline triangulation.

[Incremental]: implements the simplest insertion strategy for staged or online point addition.

[RobustPredicates]: implements `IPredicates` with Shewchuk adaptive-precision exact `InCircle`, `CounterClockwise` (`orient2d`), and `Orient3d`; `Statistic.InCircleCount` and `InCircleAdaptCount` expose its counters within the `double`-domain robustness floor.

[IPredicates]: carries the orientation and incircle contract replaceable through `Configuration`.

[Triangle]: `TriangleNet.Topology.Triangle` implements `ITriangle`; `int ID`/`Label`, `double Area`, `GetVertex(i)`, `GetVertexID(i)`, `GetNeighbor(i)`, `GetNeighborID(i)`, and `GetSegment(i)` expose connectivity to FEA assembly.

[ITriangle]: exposes `int ID`/`Label`, `double Area`, `GetVertexID(i)`, and `GetNeighborID(i)` for output triangles.

[VertexType]: `InputVertex`, `SegmentVertex`, `FreeVertex`, `DeadVertex`, and `UndeadVertex` classify mesh vertices.

[NodeNumbering]: `None`, `Linear`, and `CuthillMcKee` select the `Mesh.Renumber` bandwidth-reduction ordering.

[PUBLIC_TYPE_SCOPE]: Voronoi dual, smoothing, quality tools — `TriangleNet.Voronoi` / `.Smoothing` / `.Tools`

- rail: kernel-substrate

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]        | [CAPABILITY]        |
| :-----: | :------------------- | :------------------- | :------------------ |
|  [01]   | `StandardVoronoi`    | Voronoi base         | unbounded dual      |
|  [02]   | `BoundedVoronoi`     | Voronoi base         | bounded dual        |
|  [03]   | `VoronoiBase`        | dual base            | dual construction   |
|  [04]   | `DcelMesh`           | DCEL mesh            | half-edge topology  |
|  [05]   | `SimpleSmoother`     | mesh smoother        | vertex relaxation   |
|  [06]   | `QualityMeasure`     | quality metrics      | FEA acceptance      |
|  [07]   | `Statistic`          | algorithm counters   | performance receipt |
|  [08]   | `IntersectionHelper` | segment geometry     | clipping utilities  |
|  [09]   | `Interpolation`      | scalar interpolation | field sampling      |
|  [10]   | `TriangleQuadTree`   | spatial index        | point location      |
|  [11]   | `AdjacencyMatrix`    | sparse topology      | mesh adjacency      |
|  [12]   | `CuthillMcKee`       | reorder engine       | bandwidth reduction |

[StandardVoronoi]: `StandardVoronoi(Mesh)`, `(Mesh, Rectangle)`, and `(Mesh, Rectangle, IVoronoiFactory, IPredicates)` produce the half-edge `DcelMesh` dual, optionally clipped to a box.

[BoundedVoronoi]: `BoundedVoronoi(Mesh)` and `(Mesh, IVoronoiFactory, IPredicates)` clip the dual to the input mesh convex hull.

[VoronoiBase]: is the abstract Voronoi base.

[DcelMesh]: represents the dual as a doubly-connected edge list of `Face`, `HalfEdge`, and `Vertex` elements.

[SimpleSmoother]: implements `ISmoother`; `Smooth(IMesh)` and `Smooth(IMesh, limit)` apply Laplacian/CVT vertex relaxation through an internal Voronoi factory.

[QualityMeasure]: `Update(Mesh, sampleDegrees)` calculates per-element `Area` minimum, maximum, and total plus `alpha` minimum-angle statistics consumed by the FEA quality gate.

[Statistic]: static `InCircleCount`, `InCircleAdaptCount`, `CounterClockwiseCount`, `Orient3dCount`, `CircumcenterCount`, and `RelocationCount` expose predicate-call and adaptive-refinement performance.

[IntersectionHelper]: static `IntersectSegments(p0, p1, q0, q1, ref c0)`, `LiangBarsky(rect, p0, p1, ref c0, ref c1)`, and `BoxRayIntersection` own segment intersection and box clipping.

[Interpolation]: statically applies barycentric and natural-neighbor scalar interpolation over the mesh.

[TriangleQuadTree]: indexes triangles for point location.

[AdjacencyMatrix]: carries sparse mesh adjacency.

[CuthillMcKee]: reorders the adjacency graph for `Mesh.Renumber`.

[PUBLIC_TYPE_SCOPE]: file I/O — `TriangleNet.IO`

- rail: kernel-substrate

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]    |
| :-----: | :--------------- | :------------ | :-------------- |
|  [01]   | `FileProcessor`  | I/O facade    | format dispatch |
|  [02]   | `TriangleReader` | format reader | PSLG input      |
|  [03]   | `TriangleWriter` | format writer | mesh output     |
|  [04]   | `TriangleFormat` | format        | format binding  |
|  [05]   | `InputTriangle`  | I/O triangle  | decoded carrier |

[FileProcessor]: statically dispatches reads and writes across registered `IFileFormat` implementations.

[TriangleReader]: reads native `.poly`, `.node`, and `.ele` Triangle files.

[TriangleWriter]: writes a mesh through the native `.poly`, `.node`, and `.ele` Triangle formats.

[TriangleFormat]: implements `IPolygonFormat` and `IMeshFormat` for `.poly` and `.ele` bindings.

[InputTriangle]: implements `ITriangle` as the deserialized triangle carrier.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the canonical mesh pipeline — `GenericMesher.Triangulate`

- rail: kernel-substrate
- note: the pipeline is BUILD `Polygon` (`AddContour` outer + holes + regions) -> `GenericMesher.Triangulate(polygon, constraintOptions, qualityOptions)` -> read the `IMesh` collections. The four `Triangulate(IPolygon,...)` overloads layer the optional constraint and quality policies; the `Triangulate(IList<Vertex>)` overload is the unconstrained point-set Delaunay (no PSLG).

| [INDEX] | [VARIANT]  | [ENTRY_FAMILY] | [CAPABILITY]        |
| :-----: | :--------- | :------------- | :------------------ |
|  [01]   | point set  | point Delaunay | unconstrained mesh  |
|  [02]   | PSLG       | constrained    | PSLG mesh           |
|  [03]   | constraint | constrained    | constraint policy   |
|  [04]   | quality    | refined        | quality policy      |
|  [05]   | full       | full           | FEA section mesh    |
|  [06]   | structured | structured     | rectangular grid    |
|  [07]   | refine     | re-refine      | adaptive refinement |
|  [08]   | renumber   | renumber       | bandwidth reduction |

[POINT_DELAUNAY]:

- Surface: `GenericMesher.Triangulate(IList<Vertex> points)`.
- Capability: Unconstrained point-set Delaunay without segments.

[CONSTRAINED]:

- Surface: `GenericMesher.Triangulate(IPolygon polygon)`.
- Capability: Constrained PSLG Delaunay preserves segments and carves holes without quality refinement.

[CONSTRAINT_POLICY]:

- Surface: `GenericMesher.Triangulate(IPolygon polygon, ConstraintOptions options)`.
- Capability: Constrained Delaunay applies conforming, convex, and region policy.

[QUALITY_POLICY]:

- Surface: `GenericMesher.Triangulate(IPolygon polygon, QualityOptions quality)`.
- Capability: Constrained Ruppert/Chew refinement inserts Steiner points under minimum-angle and maximum-area bounds.

[FULL_PIPELINE]:

- Surface: `GenericMesher.Triangulate(IPolygon polygon, ConstraintOptions options, QualityOptions quality)`.
- Capability: The FEA section-mesh entry combines constrained, conforming, region-aware, and quality-refined meshing.

[STRUCTURED]:

- Surface: `GenericMesher.StructuredMesh(double w, double h, int nx, int ny)` and `StructuredMesh(Rectangle, nx, ny)`.
- Capability: Produces a regular `nx`-by-`ny` rectangular grid without a PSLG.

[REFINE]:

- Surface: `Mesh.Refine(QualityOptions quality, bool delaunay = false)`.
- Capability: Requalifies an existing mesh after an adaptive FEA error estimate.

[RENUMBER]:

- Surface: `Mesh.Renumber([NodeNumbering num])`.
- Capability: Applies Cuthill-McKee numbering to reduce FEA stiffness-matrix bandwidth.

## [04]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:

- Coordinates are `double` (`Point.X`/`Y`); `Triangle` is a FLOATING-POINT mesher with `RobustPredicates` adaptive-precision orient/incircle — robust to ROUNDING but NOT an exact-rational kernel. For exact-arithmetic robustness (degenerate cocircular input, exact constrained-edge recovery) the consumer routes through the `Rasm` kernel's `.planning/Meshing/delaunay.md` `Adaptive.Resolve` four-tier Bowyer-Watson; `Triangle` is the production 2D quality path where the input is well-conditioned.
- `QualityOptions.MinimumAngle` is the Ruppert termination bound: angles above ~ deg are guaranteed to terminate; pushing it higher risks non-termination on tight input angles — bound `SteinerPoints` to cap the insertion budget. `MaximumArea` is the global element-size cap; `VariableArea` + `RegionPointer` give per-region sizing; `UserTest` (`Func<ITriangle,double,bool>`) is the custom refine predicate (return true to split — the FEA gradient/error-driven sizing hook).
- `ConstraintOptions.ConformingDelaunay` inserts Steiner points so every triangle's circumcircle is empty even across constrained edges (the true Delaunay property); without it the result is a CONSTRAINED Delaunay (edges forced, circumcircles not guaranteed empty). `Convex` meshes the convex hull ignoring concavities; `UseRegions` honours `RegionPointer` ids for per-region attributes/area.
- Holes are carved by a `Point` seed INSIDE the hole loop (`Contour.FindInteriorPoint` produces one); the mesher floods the unbounded region from each hole seed and deletes those triangles. Regions are tagged the same way via `RegionPointer`.

[STACKING_LAW]:

`VividOrange.InteractionDiagram` composes `Triangle` and `MIConvexHull` inside one N-M-M capacity engine.

[CAPACITY_PIPELINE]:

- Input: the concrete outer `Contour` plus rebar exclusion and void contours form a `Polygon`.
- Mesh: `GenericMesher.Triangulate(polygon, qualityOptions)` produces the constrained `AnalyticalFace` fibre grid.
- Integrate: the strain-plane sweep integrates fibre stress over meshed faces into force-moment-moment points.
- Hull: `MIConvexHull` welds the point cloud into the 3D capacity onion.
- Output: the design composes the welded `IForceMomentMesh` without calling either primitive directly.

[DIRECT_USE]: no repo-owned project directly references `Triangle`; the authored Bowyer-Watson and `Arrangement` owners retain exact-robustness and 3D concerns.

[OWNER_BOUNDARIES]:

- `Triangle`: constrained and quality-refined Delaunay over a PSLG plus `StandardVoronoi`/`BoundedVoronoi` duals derived from that mesh.
- `MIConvexHull`: unconstrained point-set hull-lift Delaunay and ND Voronoi dual without segments, holes, or quality refinement.
- `SharpVoronoiLib`: standalone 2D point-site Fortune Voronoi with border clipping and Lloyd relaxation.
- Routing: a constrained-mesh dual uses `Triangle`, while a standalone clipped point-site diagram uses `SharpVoronoiLib`.

[RAIL_LAW]:

- Package: `Triangle` (assembly `Triangle`, alias `TriangleNet`)
- Owns: constrained/conforming 2D Delaunay triangulation and Ruppert/Chew quality refinement over a PSLG (`Polygon` of vertices/segments/holes/regions), with pluggable `Dwyer`/`SweepLine`/`Incremental` initial triangulation, `RobustPredicates` adaptive-precision orient/incircle, Laplacian `SimpleSmoother`, the `StandardVoronoi`/`BoundedVoronoi` dual, `QualityMeasure`/`Statistic` metrics, Cuthill-McKee renumbering, and `.poly`/`.node`/`.ele` I/O — all in the `double` domain
- Accept: the transitive `VividOrange.InteractionDiagram` section fibre-grid mesh
- Reject: direct `Triangle` calls minted in `Rasm.Materials`; direct repo-owned references without a central package admission row; using `Triangle` where exact-rational robustness on degenerate input is required (route to the kernel `Adaptive.Resolve` Bowyer-Watson) or where 3D tetrahedralization is needed (`Triangle` is 2D-only — the kernel owns 3D); duplicating the unconstrained point-set Delaunay/ND-Voronoi (`MIConvexHull` owns it) or a standalone clipped point-site Voronoi (`SharpVoronoiLib` owns it)
