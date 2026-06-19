# [RASM]

`Rasm` is the geometry and numeric kernel below the C# app strata: it references no sibling and is referenced by every stratum above. It carries three mature sub-domains with co-located source — `Vectors` (the typed operator/spectral vocabulary), `Analysis` (the measure/query/intersect/topology algebra), and `Domain` (Rhino-normalization, context, stats, validation) — plus the greenfield robust-core `Geometry` sub-domain whose design pages live under `.planning/`. The robust-core composes `Rasm.Vectors` as settled vocabulary, never re-mints a primitive, admits no external geometry library (every predicate, index, naming, healing, and constraint kernel is authored from first principles), and runs pure-managed on osx-arm64 with one tier-3 native gate for the boolean/CSG arrangement asset. This README routes the design pages and registers every external package the folder uses.

## [01]-[ROUTER]

- [01]-[PREDICATES](Geometry/.planning/Numerics/predicates.md): Adaptive-precision exact-predicate floor — `Predicate` (Orient2D/Orient3D/InCircle/InSphere + OrientLPI/OrientTPI/InCircleLPI/InSphereTPI implicit-point) over `Expansion` sign-exact arithmetic, the `ErrorBound` filter table, and the `NumericsPolicy` interior-double scope.
- [02]-[INDEX](Geometry/.planning/Spatial/index.md): Polymorphic `SpatialIndex` (SAH-BVH + Morton linear octree) over one `NodeStore`, the `SpatialQuery` nearest/range/ray/overlap fold, `Refit`, and the `ToAcceleration` Compute seam.
- [03]-[NAMING](Geometry/.planning/Spatial/naming.md): Persistent topological naming — one `TopoName` lineage algebra, the `NameTable` registry, and the `Track` re-anchor-by-signature fold.
- [04]-[RECONCILIATION](Geometry/.planning/Spatial/reconciliation.md): Naming-to-hash fence — `CanonicalTopology` canonical-adjacency encoder and the `Reconcile` projection onto the Persistence `GeometryHash`.
- [05]-[REPAIR](Geometry/.planning/Processing/repair.md): Repair rail — the `HealOp` closed algebra (six author-kernels + one native-gate boolean) and the `Heal.Repair` session fold composing the predicate floor.
- [06]-[RECEIPTS](Geometry/.planning/Processing/receipts.md): Typed `RebuildReceipt` family, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold feeding the naming re-anchor.
- [07]-[SOLVER](Geometry/.planning/Processing/solver.md): Author-kernel geometric constraint solver — the closed `Constraint` residual/Jacobian algebra, the `DofAnalysis` verdict, and the Levenberg-Marquardt `Solve` iterate.
- [08]-[DELAUNAY](Geometry/.planning/Meshing/delaunay.md): Author-kernel constrained Delaunay owner — one `Tessellation` (triangulation/tetrahedralization) over a flat `SimplexStore`, Bowyer-Watson insertion driven by the exact `InCircle`/`InSphere` predicate, and predicate-guarded constraint recovery.
- [09]-[OFFSET](Geometry/.planning/Meshing/offset.md): Predicate-exact offsetting owner — one `OffsetOp` (skeleton/medial/minkowski/offset) over a flat `WavefrontStore`, the Aichholzer-Aurenhammer wavefront event queue driven by the exact `Orient2D` turn sign, the medial-axis read-off over the `Tessellation` Voronoi dual, the Minkowski edge-normal convolution, and `Assemble` loop assembly re-routed through the `Arrangement` planar-overlay exact cell complex.
- [10]-[ARRANGEMENT](Geometry/.planning/Meshing/arrangement.md): Fully-managed exact-arithmetic mesh/polygon arrangement owner — one `Arrangement` (mesh-boolean/planar-overlay/cell-complex) over the constrained tetrahedralization, every intersection segment an `Lpi`/`Tpi` implicit point, cells classified inside/outside by `SpatialQuery.Winding` GWN scalar, `BooleanOp`-selected cells welded through `DuplicateWeld`, and the `BooleanReceipt` kept/classified/weld-count carrier; retires the tier-3 native CSG gate.
- [11]-[INTERSECT](Geometry/.planning/Meshing/intersect.md): Predicate-exact curve/surface/mesh intersection lattice — one `IntersectOp` (segment-segment/segment-triangle/triangle-triangle/ray-mesh/mesh-mesh/plane-mesh) over an exact-sign `Crossing` carrier (`Lpi`/`Tpi` implicit point + exact parametric ordering key), the robust counterpart that `Analysis/Intersect.cs` never owns at predicate exactness.
- [12]-[FIT](Geometry/.planning/Processing/fit.md): Robust geometric primitive-fit owner — one `FitOp` (plane/sphere/cylinder/cone/torus/line) over a MLESAC robust-consensus sampler plus geometric-orthogonal-distance LM refine reusing the `ConstraintSolver` iterate, returning a typed `FitReceipt` (inlier mask, residual, fitted primitive, consensus score); the scan-to-BIM segmentation primitive.
- [13]-[FLATTEN](Geometry/.planning/Processing/flatten.md): Robust mesh parameterization/UV-flattening owner — one `ParamOp` (harmonic/LSCM/ARAP/BFF) over the `Vectors` DEC operator substrate composing the `LaplacianCache` Cholesky factor, returning a typed `ChartAtlas` (per-chart UV, distortion receipt, seam set).
- [14]-[VIEW](Geometry/.planning/Drawing/view.md): Predicate-exact hidden-line/silhouette projection owner — one `ViewOp` (silhouette/hidden-line/section/outline) over a Newell-Newell-Sancha BSP visibility kernel, the silhouette locus decided by the exact `Orient3D` view-dot sign, the section cut via one `IntersectOp.Apply`, and the `DrawingProjection` visible/hidden 2D-segment carrier.
- [15]-[DECIMATE](Geometry/.planning/Processing/decimate.md): Predicate-guarded mesh decimation/LOD owner — one `SimplifyOp` (quadric-collapse/progressive-mesh/voxel-remesh/feature-preserve) over a Garland-Heckbert QEM edge-collapse queue gated by the exact `Orient3D` sign, and the `DecimationReceipt` per-LOD budget/Hausdorff bound/reversible vsplit carrier.
- [16]-[PACK](Geometry/.planning/Drawing/pack.md): Canonical geometry-encoding/packing owner — one `PackOp` (point-cloud/mesh-patch/voxel-grid/brep-patch) over the eight-row `EncodingChannel` feature lattice composing the live `Vectors` mesh/cloud/SDF/curvature/geodesic readers, and the `EncodedGeometry` per-channel-payload/lossless-round-trip-witness carrier.
- [17]-[FAULTS](Geometry/.planning/Numerics/faults.md): Consolidated band-2400 `GeometryFault` union every geometry rail routes through (spatial 2401–, topology 2410–, healing 2420–, constraints 2430–, offsetting 2440–, arrangement 2450–, intersection 2460–, fitting 2470–, parameterization 2480–, projection 2490–, decimation 2500–, encoding 2510–), and the `GeometryKeyPolicy` ordinal key accessor.

## [02]-[DOMAIN_PACKAGES]

The numeric solver and geometry host packages this folder consumes outside the C# substrate registry; versions are centralized in the one C# manifest, corroborated by `.api/`.

[NUMERIC_SOLVERS]:
- `MathNet.Numerics`
- `MathNet.Numerics.Providers.MKL`
- `MathNet.Numerics.Providers.OpenBLAS`
- `CSparse`

[PROJECTS]:
- `Rhino.Geometry` / `RhinoCommon`
- `Rasm.Vectors`
- `Rasm.Compute.Solver`

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder consumes; full registry and version ownership live in `libs/csharp/.planning/README.md`, with decompile evidence in the folder `.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `System.Numerics.Tensors`

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `SharpFuzz`
- `Verify.XunitV3`
