# [RASM_API_GEOMETRY3SHARP]

`geometry3Sharp` (gradientspace, ns `g3`) is the dense index-based triangle-mesh substrate and
its full operator ecosystem: `DMesh3` is a compact reference-counted vertex/edge/triangle mesh
with O(1) local connectivity (one-rings, edge/triangle adjacency) and in-place Euler edits
(`SplitEdge`/`CollapseEdge`/`FlipEdge`/`PokeTriangle`), and around it g3 ships a complete
processing stack: `DMeshAABBTree3` spatial queries with the Barill-et-al. fast-winding-number
inside/outside test, `MeshBoolean`/`MeshMeshCut`/`MeshPlaneCut` surface CSG, `Remesher`
(isotropic flip/split/collapse/smooth) and `Reducer` (Garland-Heckbert QEM decimation), a full
implicit/SDF kernel (`MeshSignedDistanceGrid` + `MarchingCubes` + the `Implicit*Union/Difference
/Intersection/Smooth*` bounded-function algebra), Laplacian deformation/parameterization
(`LaplacianMeshDeformer`/`MeshLocalParam`), primitive fits (`OrthogonalPlaneFit3`/`GaussPointsFit3`/`QuadraticFit2`/`Sphere3` fit), a mesh-generator catalog, and the `StandardMeshReader`/`StandardMeshWriter` OBJ/STL/OFF/g3-binary/SVG I/O. This is the kernel's dense-mesh container and its
mesh-domain algorithm reservoir; the predicate-EXACT robust kernels (`Meshing/arrangement`,
`Meshing/delaunay`, `Meshing/intersect`) are AUTHORED against the four-tier predicate floor and
do NOT delegate to g3's float-domain ops — g3 is the substrate and the import/biarc/SDF leg, the
exact CSG/intersection stays author-owned.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geometry3Sharp`
- package: `geometry3Sharp`
- version: `1.0.324`
- license: `BSL-1.0` (Boost Software License 1.0 — gradientspace `LICENSE` via the nuspec `licenseUrl`, no bundled file or SPDX expression; `requireLicenseAcceptance=false`, `© Ryan Schmidt 2016`)
- assembly: `geometry3Sharp` (multi-target `netstandard2.0` + `net45`; the `lib/netstandard2.0/geometry3Sharp.dll` is the asset the `net10.0` consumer binds — `net45` is the legacy fallback, never bound forward)
- namespace: `g3`
- deps: none (pure-managed AnyCPU, osx-arm64-clean, ALC/IL-only)
- owners: `libs/csharp/Rasm/Rasm.csproj` (KERNEL owner — `[COMPUTATIONAL_GEOMETRY]`), `libs/csharp/Rasm.Bim/Rasm.Bim.csproj` (mesh-text import), `libs/csharp/Rasm.Fabrication/Rasm.Fabrication.csproj` (`BiArcFit2`) — all over the one central pin
- rail: dense-mesh substrate / mesh-processing / implicit-SDF / mesh-IO

## [02]-[MESH_CONTAINER]

[MESH_CONTAINER_SCOPE]: the `DMesh3` index-based mesh and its query/edit/spatial surface
- rail: mesh

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `DMesh3`            | `class : IMesh, IDeformableMesh` | compact ref-counted V/E/T mesh; vertex normals/colors/UVs/groups opt-in via `MeshComponents` |
|  [02]   | `NTMesh3`           | `class`       | non-manifold mesh variant (>2 triangles per edge) for intermediate states |
|  [03]   | `DMeshAABBTree3`    | `class : ISpatial` | top-down AABB tree: nearest tri/vtx, ray hit, mesh-mesh intersection, winding number |
|  [04]   | `DMesh3Builder`     | `class : IMeshBuilder` | streaming append-builder the I/O readers drive (`AddTriangleFailBehaviors`) |
|  [05]   | `Index2i`/`Index3i`/`Index4i` | `struct`   | integer index tuples — a triangle is an `Index3i` of vertex ids          |
|  [06]   | `MeshComponents`/`MeshHints` | `enum`   | the opt-in attribute mask + builder hints (`IsCompact`/manifold)         |
|  [07]   | `MeshResult`/`MeshConstraints` | `enum`/`class` | edit result status + the smoothing/remesh constraint set (`MeshConstraintUtil`) |

`DMesh3` is index-addressed (`int` vertex/triangle ids, NOT pointers): `GetVertex(vID)→Vector3d`,
`GetTriangle(tID)→Index3i`, `AppendVertex(Vector3d)→int`, `AppendTriangle(v0,v1,v2,gid=-1)→int`,
`VertexCount`/`TriangleCount`, `IsClosed()`, `GetBounds()→AxisAlignedBox3d`, `EnableVertexNormals`/`GetVertexNormal`/`GetVertexUV`/`GetVertexColor`. Local edits return typed info
structs (`EdgeCollapseInfo`/`EdgeSplitInfo`/`EdgeFlipInfo`/`PokeTriangleInfo`). The `Vectors`
typed vector vocabulary meets g3 at `Vector3d`/`Vector3f`/`Index3i` — these are g3's own structs,
so the kernel boundary-maps once at the seam rather than threading a foreign mesh type internally.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the mesh-domain operations the kernel composes, grouped by concern
- rail: mesh-processing / IO

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `StandardMeshReader.ReadMesh(string|Stream,ext)` → `DMesh3`                            | IO read        | format-dispatched OBJ/STL/OFF/g3-binary read; `OBJReader`/`STLReader`/`OFFReader`/`BinaryG3Reader` per format |
|  [02]   | `StandardMeshWriter.WriteMesh(file, IMesh, WriteOptions)` / `WriteMeshes(file, List<DMesh3>, opts)` | IO write       | OBJ/STL/OFF/g3-binary write; `SVGWriter` for 2D vector emit              |
|  [03]   | `new MeshBoolean { Target, Tool }.Compute()` → `Result` (`DMesh3`)                     | surface CSG    | mesh-mesh union/difference/intersection via cut+resolve (`VertexSnapTol`) |
|  [04]   | `new MeshMeshCut { … }.Compute()` / `MeshPlaneCut(mesh, origin, normal)`               | cut            | imprint one mesh's intersection curve onto another / split by a plane     |
|  [05]   | `new Remesher(DMesh3){ MinEdgeLength, MaxEdgeLength, SmoothType }.BasicRemeshPass()`    | remesh         | isotropic flip/split/collapse/smooth to a target edge length; `SetProjectionTarget` keeps it on the surface |
|  [06]   | `new Reducer(DMesh3).ReduceToTriangleCount(n)` / `ReduceToVertexCount(n)` / `ReduceToEdgeLength(L)` | decimate (QEM) | Garland-Heckbert quadric-error edge-collapse LOD; `PreserveBoundaryShape` |
|  [07]   | `new MeshSignedDistanceGrid(mesh, cellSize).Compute()` + `MarchingCubes{ Implicit }.Generate()` | implicit/SDF   | narrow-band signed distance field → iso-surface re-mesh; the voxel CSG path |
|  [08]   | `BiArcFit2(Vector2d p1, Vector2d t1, Vector2d p2, Vector2d t2)` → `Arc1`/`Arc2`/`Segment1`/`Segment2` | curve fit (2D) | tangent-continuous biarc fit of a curve span into two `Arc2d` (+ segments) |
|  [09]   | `OrthogonalPlaneFit3(points)` / `GaussPointsFit3` / `QuadraticFit2`                    | primitive fit  | least-squares plane/sphere/quadric fit — the analytic seed for robust fits |

## [04]-[IMPLEMENTATION_LAW]

[SPATIAL_AND_WINDING]:
- `DMeshAABBTree3(mesh, autoBuild:true)` then `Build(BuildStrategy, ClusterPolicy)` gives `FindNearestTriangle(p, out distSqr)`, `FindNearestVertex(p)`, `FindNearestHitTriangle(Ray3d)`, `FindAllHitTriangles(ray, list)`, and `TestIntersection(otherTree)` / `FindAllIntersections(otherTree)` → `IntersectionsQueryResult { Points, Segments }`
- the winding-number leg is the headline robustness primitive: `IsInside(Vector3d)`, `WindingNumber(p)` (exact summed solid angle), and `FastWindingNumber(p)` (the Barill-et-al. tree-approximated GWN, tuned by `FWNBeta = 2.0` / `FWNApproxOrder = 2`) classify a point against an arbitrary (even open/non-watertight) mesh — this is g3's robust inside/outside, distinct from but technique-aligned with the kernel's own `SpatialQuery.Winding` GWN in `Meshing/arrangement`
- `MeshProjectionTarget.Auto(mesh)` wraps the tree as an `IProjectionTarget` so `Remesher`/`Reducer` re-project moved vertices back onto the original surface during refinement

[SURFACE_CSG]:
- `MeshBoolean { Target, Tool, VertexSnapTol = 1e-5 }.Compute()` writes `Result`: it cuts each input by the other's intersection curve (`MeshMeshCut`), classifies the resulting patches by winding number, and welds — this is the float-domain SURFACE boolean; it does NOT carry the exact-arithmetic guarantee of `Meshing/arrangement`, so the kernel routes EXACT mesh boolean to the authored arrangement and reserves `MeshBoolean` for tolerant/preview CSG and the import/repair leg
- `MeshPlaneCut(mesh, origin, normal)` and `MeshMeshCut` imprint a cut curve; `MeshBoundaryLoops`/`MeshConnectedComponents`/`MeshDecomposition` extract loops/shells; `MeshEditor` (with `DuplicateTriBehavior`), `MeshExtrudeMesh`/`MeshExtrudeFaces`/`MeshExtrudeLoop`, and `MeshInsertPolygon`/`MeshInsertUVPolyCurve` do topological surgery

[REMESH_DECIMATE]:
- `Remesher : MeshRefinerBase` runs `BasicRemeshPass()` under `EnableFlips`/`EnableCollapses`/`EnableSplits`/`EnableSmoothing` toward `[MinEdgeLength, MaxEdgeLength]`; `SmoothType` ∈ `{Uniform, Cotan, MeanValue}`, `CustomSmoothF`/`VertexControlF` are per-vertex hooks, `EnableParallelProjection`/`EnableParallelSmooth` parallelize, `PreventNormalFlips` guards inversion, `SetProjectionTarget` keeps the surface
- `Reducer : MeshRefinerBase` is Garland-Heckbert QEM: `ReduceToTriangleCount(n)`/`ReduceToVertexCount(n)`/`ReduceToEdgeLength(L)` and `FastCollapsePass`, with `MinimizeQuadricPositionError`/`PreserveBoundaryShape`; the quadric accumulation is g3's own — the kernel's `Processing/decimate` author-kernel uses the same QEM math under `ddouble` 106-bit accumulation and `Orient3D`-guarded flips, so g3's `Reducer` is the substrate-level/preview decimator, the predicate-guarded one is authored
- `RegionRemesher`/`EdgeLoopRemesher` scope the operation to a submesh region

[IMPLICIT_SDF]:
- `MeshSignedDistanceGrid(mesh, cellSize, spatial?)` with `ComputeMode` ∈ `{FullGrid, NarrowBandOnly, NarrowBand_SpatialFloodFill}`, `InsideMode` ∈ `{CrossingCount, ParityCount}`, `ComputeSigns`/`NarrowBandMaxDistance` → `Compute()` builds a `DenseGrid3f` SDF (`Grid`/`GridOrigin`/`Dimensions`/`CellSize`, indexed `this[i,j,k]` and `this[Vector3i]`)
- `MarchingCubes { Implicit (ImplicitFunction3d), IsoValue, Bounds, CubeSize, RootMode, RootModeSteps, ParallelCompute }.Generate()` extracts the iso-surface into its `Mesh` (`DMesh3`) field; the implicit algebra is a full bounded-function (`BoundedImplicitFunction3d : ImplicitFunction3d`, adds `Bounds()`) CSG tree — `ImplicitSphere3d`/`ImplicitBox3d`/`ImplicitHalfSpace3d`/`ImplicitLine3d` leaves, `ImplicitUnion3d`/`ImplicitIntersection3d`/`ImplicitDifference3d` + `ImplicitNaryUnion3d`/`…Intersection3d`/`…Difference3d` boolean nodes, `ImplicitSmoothUnion3d`/`ImplicitSmoothDifference3d`/`ImplicitSmoothIntersection3d` blends, `ImplicitOffset3d`/`ImplicitShell3d`/`ImplicitBlend3d` modifiers, and `CachingMeshSDFImplicit`/`DenseGridTrilinearImplicit` to lift a mesh into the field — this is the voxel-domain CSG that complements both the planar `Clipper2` boolean and the exact `Meshing/arrangement`
- `MeshExtrudeMesh`, `VoxelSurfaceGenerator`, and the SDF round-trip give a remesh-by-resample repair path the `Processing/repair` `HealOp` can compose

[DEFORM_PARAM]:
- `LaplacianMeshDeformer`/`LaplacianMeshSmoother` solve a cotangent-Laplacian system with `SoftConstraintV` handles for as-rigid-as-possible deformation; `MeshLocalParam` (`UVModes`, exponential-map local parameterization) and `DenseUVMesh`/`MeshInsertUVPolyCurve` give UV charts — technique-adjacent to `Processing/flatten`'s authored harmonic/LSCM/ARAP/BFF over the `Vectors` DEC substrate, which the kernel owns; g3's are the substrate-level smoothers
- `MeshNormals.QuickCompute(mesh)` (area-weighted one-ring), `MeshTransforms` (`Translate`/`Rotate`/`Scale`/`ToFrame`/`ConvertZUpToYUp`/`VertexNormalOffset`/`PerVertexTransform`), `MeshMeasurements` (volume/area/genus/centroid) round out the in-place toolbox

[GENERATORS_FIT]:
- the mesh-generator catalog (`TrivialBox3Generator`/`GridBox3Generator`/`Sphere3Generator_NormalizedCube`/`OpenCylinderGenerator`/`CappedCylinderGenerator`/`ConeGenerator`/`TubeGenerator`/`Curve3Axis3RevolveGenerator`/`TriangulatedPolygonGenerator`/`RoundRectGenerator`/`PuncturedDiscGenerator`/`VerticalGeneralizedCylinderGenerator`) builds parametric primitives with UV modes
- `OrthogonalPlaneFit3` (total-least-squares plane + eigen frame, `ResultValid`), `GaussPointsFit3`, `QuadraticFit2`, and the sphere/circle fits supply the analytic seed `Processing/fit`'s MLESAC+LM robust consensus refines; `BiArcFit2(p1,t1,p2,t2[,d1])` fits a tangent-continuous biarc (`Arc1`/`Arc2` as `Arc2d`, or `Segment1`/`Segment2`, with `FitD1`/`FitD2` + `Distance`/`NearestPoint`) — the curve→arc primitive `Rasm.Fabrication` toolpath uses to convert line-densified offsets back to G-code arcs

[INTEGRATION_STACK]:
- `Rasm.Bim` mesh-text import: `StandardMeshReader.ReadMesh(path)` lands a `DMesh3` for the OBJ/STL/OFF `MeshText InterchangeCodec` leg; PLY/3MF stay codec-pending (no g3 reader) and route to `Ply.Net`/`AssimpNetter` in Bim — g3 owns OBJ/STL/OFF only
- `Rasm.Fabrication` toolpath: `g3.BiArcFit2` is the arc-fit the kerf/clearing passes use; the dense-mesh AM path stacks g3's `MeshSignedDistanceGrid`+`MarchingCubes` beside Fabrication's `PicoGK` voxel kernel (g3 = managed SDF, PicoGK = native OpenVDB), each a distinct fidelity tier
- kernel `Processing/repair`/`#receipts`: `MeshValidation`/`MeshConnectedComponents`/`MeshBoundaryLoops`/`Remesher`/`Reducer`/`MeshBoolean` are the substrate ops the `HealOp` closed algebra composes BELOW its predicate-exact author-kernels; the `RebuildReceipt`/`ManifoldStatus` carriers read g3's component/loop/validity output
- the `Vectors` vector/matrix vocabulary boundary-maps to g3's `Vector3d`/`Matrix3d`/`Frame3f`/`Quaterniond`/`AxisAlignedBox3d` at the mesh seam; `DMeshAABBTree3` is the dense-mesh spatial leaf beside the kernel's own SAH-BVH+octree (`Spatial/index`) and `Supercluster.KDTree.Net` point kd-tree — three spatial structures, three distinct workloads (triangle queries / primitive broad-phase / point k-NN)

[LOCAL_ADMISSION]:
- `DMesh3` is the kernel's dense-mesh CONTAINER and g3's float-domain ops are the SUBSTRATE/import/preview tier; EXACT mesh boolean, intersection, Delaunay, and offset stay AUTHORED against the predicate floor (`Meshing/arrangement`/`intersect`/`delaunay`/`offset`) and never delegate to `MeshBoolean`/`MeshMeshCut`
- inside/outside uses `FastWindingNumber` for tolerant classification; the exact arrangement classifier is the authored GWN — the two are technique-aligned, not the same code
- import lands `DMesh3` via `StandardMeshReader`; do NOT hand-roll an OBJ/STL/OFF parser; do NOT add a second dense-mesh container beside `DMesh3`
- QEM decimation, isotropic remesh, and Laplacian smoothing route to g3's `Reducer`/`Remesher`/`LaplacianMeshDeformer` for substrate/preview work and to the authored predicate-guarded kernels for the exact-tolerance deliverable

[RAIL_LAW]:
- Package: `geometry3Sharp`
- Owns: the `DMesh3` dense index-based mesh container + edits, `DMeshAABBTree3` spatial+fast-winding queries, float-domain surface CSG/cut, QEM decimation + isotropic remesh, the implicit/SDF + marching-cubes kernel, Laplacian deformation/parameterization, primitive + biarc fits, OBJ/STL/OFF/g3-binary/SVG mesh IO
- Accept: a `DMesh3` (built directly, via `DMesh3Builder`, or read by `StandardMeshReader`) plus the operation's parameter struct
- Reject: a second dense-mesh container; a hand-rolled OBJ/STL/OFF reader; using `MeshBoolean`/`MeshMeshCut` where exact-arithmetic mesh boolean/intersection is required (use `Meshing/arrangement`/`intersect`); re-implementing QEM/remesh/Laplacian primitives the kernel already authors for its exact tier; a PLY/3MF read through g3 (use `Ply.Net`/`AssimpNetter`)
