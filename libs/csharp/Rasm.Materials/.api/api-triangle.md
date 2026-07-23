# [RASM_MATERIALS_API_TRIANGLE]

`Triangle` owns constrained and conforming 2D Delaunay triangulation with Ruppert/Chew quality refinement over a Planar Straight-Line Graph of vertices, forced segments, carved holes, and per-region sizing, computed in the `double` domain over `RobustPredicates` adaptive-precision orientation and incircle tests. `Rasm.Materials` reaches it only transitively through `VividOrange.InteractionDiagram`, whose N-M-M capacity engine meshes the concrete-plus-rebar section into the `AnalyticalFace` fibre grid the strain-plane sweep integrates over.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Triangle`
- package: `Triangle` (MIT)
- assembly: `Triangle`
- namespaces: `TriangleNet`, `.Geometry`, `.Meshing` (`.Algorithm`, `.Iterators`, `.Data`), `.Voronoi`, `.Topology` (`.DCEL`), `.Smoothing`, `.Tools`, `.IO`, `.Logging`
- asset: pure-managed AnyCPU IL, no native RID asset, ALC-safe and osx-arm64-safe; the consumer binds the netstandard asset by TFM precedence
- rail: materials, transitive through `VividOrange.InteractionDiagram`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: PSLG input geometry — `TriangleNet.Geometry`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]           |
| :-----: | :-------------- | :------------ | :--------------------- |
|  [01]   | `Polygon`       | class         | PSLG graph builder     |
|  [02]   | `IPolygon`      | interface     | mesher input contract  |
|  [03]   | `Contour`       | class         | closed boundary loop   |
|  [04]   | `Vertex`        | class         | input/mesh vertex      |
|  [05]   | `Point`         | class         | 2D coordinate base     |
|  [06]   | `Segment`       | class         | forced constraint edge |
|  [07]   | `ISegment`      | interface     | constraint contract    |
|  [08]   | `Edge`          | class         | output mesh edge       |
|  [09]   | `IEdge`         | interface     | output edge contract   |
|  [10]   | `RegionPointer` | class         | region marker + sizing |
|  [11]   | `Rectangle`     | class         | mesh AABB extent       |

- `Polygon`: `Points`/`Segments`/`Holes`/`Regions` lists and the `HasPointMarkers`/`HasSegmentMarkers` flags feed `GenericMesher.Triangulate`.
- `RegionPointer(x, y, id)`: `ConstraintOptions.UseRegions` applies its `id` to per-region attributes and area constraints.

[PUBLIC_TYPE_SCOPE]: meshing engine and options — `TriangleNet` / `TriangleNet.Meshing`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :------------------ | :------------ | :------------------------ |
|  [01]   | `GenericMesher`     | class         | triangulation facade      |
|  [02]   | `Mesh`              | class         | mesh result + postprocess |
|  [03]   | `IMesh`             | interface     | mesh collection contract  |
|  [04]   | `ConstraintOptions` | class         | constraint policy         |
|  [05]   | `QualityOptions`    | class         | refinement policy         |
|  [06]   | `Converter`         | static class  | mesh/DCEL bridge          |
|  [07]   | `MeshValidator`     | static class  | invariant checker         |
|  [08]   | `Configuration`     | class         | predicate/factory DI      |

- `IMesh`: `Vertices`, `Edges`, `Segments`, `Triangles`, `Holes`, and `Bounds` form the FEA grid output.
- `Converter.ToDCEL(Mesh) -> DcelMesh`: builds the half-edge dual the Voronoi surfaces consume.
- `Configuration`: defaults `IPredicates` to `RobustPredicates` and admits replacement of the adaptive-precision kernel.

[PUBLIC_TYPE_SCOPE]: triangulation algorithms, topology, predicates — `TriangleNet.Meshing.Algorithm` / `.Topology`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :----------------- | :------------ | :---------------------------- |
|  [01]   | `ITriangulator`    | interface     | strategy contract             |
|  [02]   | `Dwyer`            | class         | divide-and-conquer, default   |
|  [03]   | `SweepLine`        | class         | Fortune sweepline             |
|  [04]   | `Incremental`      | class         | incremental insertion         |
|  [05]   | `RobustPredicates` | class         | adaptive-precision predicates |
|  [06]   | `IPredicates`      | interface     | orientation/incircle contract |
|  [07]   | `Triangle`         | class         | output triangle connectivity  |
|  [08]   | `ITriangle`        | interface     | triangle access contract      |
|  [09]   | `VertexType`       | enum          | vertex classification         |
|  [10]   | `NodeNumbering`    | enum          | renumber ordering             |

- `RobustPredicates`: exact `InCircle`, `CounterClockwise` (orient2d), `NonRegular`, and `FindCircumcenter` in the `double` domain; `Statistic` exposes its call counters.
- `Triangle.GetVertex(int)`/`GetNeighbor(int)`/`GetSegment(int)` with `ID`/`Label`/`Area` expose connectivity to FEA assembly.
- `[VertexType]`: `InputVertex` `SegmentVertex` `FreeVertex` `DeadVertex` `UndeadVertex`.
- `[NodeNumbering]`: `None` `Linear` `CuthillMcKee`.

[PUBLIC_TYPE_SCOPE]: Voronoi dual, smoothing, quality tools — `TriangleNet.Voronoi` / `.Smoothing` / `.Tools`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [CAPABILITY]            |
| :-----: | :------------------- | :------------- | :---------------------- |
|  [01]   | `StandardVoronoi`    | class          | unbounded Voronoi dual  |
|  [02]   | `BoundedVoronoi`     | class          | hull-clipped dual       |
|  [03]   | `VoronoiBase`        | abstract class | dual construction base  |
|  [04]   | `DcelMesh`           | class          | half-edge topology      |
|  [05]   | `SimpleSmoother`     | class          | vertex relaxation       |
|  [06]   | `QualityMeasure`     | class          | FEA quality metrics     |
|  [07]   | `Statistic`          | static class   | predicate-call counters |
|  [08]   | `IntersectionHelper` | static class   | segment/box clipping    |
|  [09]   | `Interpolation`      | static class   | scalar field sampling   |
|  [10]   | `TriangleQuadTree`   | class          | point-location index    |
|  [11]   | `AdjacencyMatrix`    | class          | sparse mesh adjacency   |
|  [12]   | `CuthillMcKee`       | class          | bandwidth reduction     |

- `QualityMeasure.Update(Mesh)`: computes `area_min`/`area_max`/`area_total` and `alpha_min`/`alpha_max`/`alpha_ave` minimum-angle statistics for the FEA quality gate.
- `IntersectionHelper.IntersectSegments`/`LiangBarsky`/`BoxRayIntersection`: segment intersection and Liang-Barsky box clipping.
- `[Statistic]`: `InCircleCount` `InCircleAdaptCount` `CounterClockwiseCount` `Orient3dCount` `CircumcenterCount` `RelocationCount`.

[PUBLIC_TYPE_SCOPE]: file I/O — `TriangleNet.IO`

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]             |
| :-----: | :--------------- | :------------ | :----------------------- |
|  [01]   | `FileProcessor`  | static class  | format dispatch          |
|  [02]   | `TriangleReader` | class         | PSLG reader              |
|  [03]   | `TriangleWriter` | class         | mesh writer              |
|  [04]   | `TriangleFormat` | class         | `.poly`/`.ele` binding   |
|  [05]   | `InputTriangle`  | class         | decoded triangle carrier |

- `TriangleReader`/`TriangleWriter`: read and write native `.poly`, `.node`, and `.ele` Triangle files through the `FileProcessor` format registry.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the mesh pipeline — build a `Polygon` (`AddContour` outer, holes, regions), drive `GenericMesher.Triangulate`, read the `IMesh` collections; the constraint and quality overloads layer the optional policies onto the same call.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Polygon.AddContour(IEnumerable<Vertex>, int, bool, bool)`               | instance | build outer/hole/region loops     |
|  [02]   | `GenericMesher.Triangulate(IList<Vertex>)`                               | instance | unconstrained point-set Delaunay  |
|  [03]   | `GenericMesher.Triangulate(IPolygon)`                                    | instance | constrained PSLG Delaunay         |
|  [04]   | `GenericMesher.Triangulate(IPolygon, ConstraintOptions)`                 | instance | conforming/convex/region policy   |
|  [05]   | `GenericMesher.Triangulate(IPolygon, QualityOptions)`                    | instance | Ruppert/Chew quality refinement   |
|  [06]   | `GenericMesher.Triangulate(IPolygon, ConstraintOptions, QualityOptions)` | instance | full FEA section mesh             |
|  [07]   | `GenericMesher.StructuredMesh(double, double, int, int)`                 | static   | regular nx-by-ny grid             |
|  [08]   | `Mesh.Refine(QualityOptions, bool)`                                      | instance | re-refine after error estimate    |
|  [09]   | `Mesh.Renumber(NodeNumbering)`                                           | instance | Cuthill-McKee bandwidth reduction |
|  [10]   | `StandardVoronoi(Mesh, Rectangle)`                                       | ctor     | unbounded/clipped Voronoi dual    |
|  [11]   | `BoundedVoronoi(Mesh)`                                                   | ctor     | hull-clipped Voronoi dual         |
|  [12]   | `SimpleSmoother.Smooth(IMesh, int)`                                      | instance | Laplacian/CVT relaxation          |

- `Contour.FindInteriorPoint(int, double) -> Point`: seeds a point inside the loop for a hole or region marker.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Coordinates are `double`; `RobustPredicates` gives adaptive-precision orientation and incircle robust to rounding, never exact-rational — the mesher owns the well-conditioned 2D quality path, never exact-arithmetic robustness or 3D.
- `ConstraintOptions.ConformingDelaunay` inserts Steiner points so every circumcircle is empty across constrained edges (true Delaunay); without it edges force but circumcircles are not guaranteed empty. `Convex` meshes the convex hull ignoring concavities; `UseRegions` honors `RegionPointer` ids for per-region attributes and area.
- `QualityOptions.MinimumAngle` sets the Ruppert/Chew lower angle bound: termination is guaranteed below roughly 20.7 degrees and holds in practice near 34 degrees, above which tight input angles risk non-termination, so `SteinerPoints` caps the insertion budget (`-1` unbounded). `MaximumArea` is the global element-size cap; `VariableArea` with `RegionPointer` gives per-region sizing; `UserTest` (`Func<ITriangle, double, bool>`) is the FEA gradient-driven refine predicate, returning true to split.
- Holes carve from a `Point` seed inside the loop that `Contour.FindInteriorPoint` produces: the mesher floods the unbounded region from each seed and deletes those triangles, and regions tag the same way through `RegionPointer`.

[STACKING]:
- `VividOrange.InteractionDiagram`(`.api/api-vividorange-interactiondiagram.md`): the `GenericMesher.Triangulate(polygon, qualityOptions)` `IMesh` becomes the `AnalyticalFace` fibre grid the N-M-M strain-plane sweep integrates fibre stress over.
- capacity pipeline: the concrete outer `Contour`, rebar exclusions, and void contours form the `Polygon`, the constrained mesh feeds the strain-plane sweep into force-moment-moment points, and `MIConvexHull` welds the cloud into the 3D capacity onion the design composes as the welded `IForceMomentMesh`.

[LOCAL_ADMISSION]:
- `Rasm.Materials` reaches `Triangle` only transitively through `VividOrange.InteractionDiagram`, minting no direct call and carrying no central admission row.
- A constrained-mesh dual routes to `Triangle`; unconstrained point-set hull-Delaunay and ND Voronoi route to `MIConvexHull`, and a standalone clipped point-site diagram routes to `SharpVoronoiLib`.

[RAIL_LAW]:
- Package: `Triangle` (assembly `Triangle`, namespace root `TriangleNet`)
- Owns: constrained and conforming 2D Delaunay with Ruppert/Chew quality refinement over a PSLG in the `double` domain — pluggable initial triangulation, adaptive-precision predicates, Laplacian smoothing, the Voronoi dual, quality metrics, Cuthill-McKee renumbering, and native `.poly`/`.node`/`.ele` I/O.
- Accept: the transitive `VividOrange.InteractionDiagram` section fibre-grid mesh.
- Reject: a direct `Triangle` call minted in `Rasm.Materials`; exact-rational robustness on degenerate input or 3D tetrahedralization, which route to the kernel `Adaptive.Resolve` Bowyer-Watson; unconstrained point-set Delaunay or ND Voronoi, which `MIConvexHull` owns; a standalone clipped point-site Voronoi, which `SharpVoronoiLib` owns.
