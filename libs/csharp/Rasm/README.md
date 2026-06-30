# [RASM]

`Rasm` is the geometry and numeric kernel below the C# app strata: it references no sibling and is referenced by every stratum above. It carries three mature sub-domains with co-located source — `Vectors` (the typed operator/spectral vocabulary), `Analysis` (the measure/query/intersect/topology algebra), and `Domain` (Rhino-normalization, context, stats, validation, and the seed-zero `ContentHash` content-identity entry the federation composes) — plus the greenfield robust-core `Geometry` sub-domain whose design pages live under `.planning/`. The robust-core composes `Rasm.Vectors` as settled vocabulary, never re-mints a primitive, authors every exact predicate/index/naming/healing/constraint kernel from first principles, and additively composes a focused set of host-neutral managed geometry owners — a pure-managed NURBS evaluation contract (`GShark`), a dense point-cloud kd-tree (`Supercluster.KDTree.Net`), and a winding-rule polygon tessellator (`LibTessDotNet`) — for the parametric, point-neighbour, and messy-winding-fill concerns a production ecosystem package owns better than a hand-roll. It runs pure-managed on osx-arm64 with one tier-3 native gate for the boolean/CSG arrangement asset. This README routes the design pages and registers every external package the folder uses.

## [01]-[ROUTER]

- [01]-[PREDICATES](Geometry/.planning/Numerics/predicates.md): Adaptive-precision exact-predicate floor — `Predicate` (Orient2D/Orient3D/InCircle/InSphere + OrientLPI/OrientTPI/InCircleLPI/InSphereTPI implicit-point) over the `PrecisionTier` four-tier `Adaptive.Resolve` ladder (`double` filter → `ddouble` 106-bit refine → `Expansion` sign-exact → `Fraction` rational oracle), the per-tier `ErrorBound` filter/refine table, the `RationalOracle` exact-rational adjudicator, and the `NumericsPolicy` interior-double scope.
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
- [17]-[FAULTS](Geometry/.planning/Numerics/faults.md): Consolidated band-2400 `GeometryFault` union every geometry rail routes through, the whole family inside the geometry century 2400–2449 (strictly below the AEC `MaterialFault` 2450 boundary) on a 4-wide per-cluster stride — spatial 2400–, topology 2404–, healing 2408–, constraints 2412–, offsetting 2416–, arrangement 2420–, intersection 2424–, fitting 2428–, parameterization 2432–, projection 2436–, simplification 2440–, encoding 2444–.

## [02]-[DOMAIN_PACKAGES]

The numeric solver and geometry host packages this folder consumes outside the C# substrate registry; versions are centralized in the one C# manifest, corroborated by `.api/`.

[NUMERIC_SOLVERS]:
- `MathNet.Numerics`
- `MathNet.Numerics.Providers.MKL`
- `MathNet.Numerics.Providers.OpenBLAS`
- `CSparse`

[EXACT_PRECISION]:

The predicate floor's precision ladder: `TYoshimura.DoubleDouble` supplies the middle-precision tier (106-bit hi/lo double-double, FMA `TwoProduct` + Knuth `TwoSum` transforms matching the kernel) that resolves near-degenerate determinant signs as a fast second stage below the `Expansion` exact branch, and ALSO the 106-bit accumulation tier for the cancellation-prone non-predicate reduces — the Garland-Heckbert QEM `Quadric` plane-sum/cost-evaluate (`Processing/decimate`) and the Levenberg-Marquardt orthogonal-distance objective `Σ d²` (`Processing/fit`) — narrowing to `double` only at the readout; `ExtendedNumerics.BigRational` supplies the exact-rational oracle (BigInteger numerator/denominator with exact `Sign`/`NormalizeSign`) that the Orient2D/Orient3D/InCircle/InSphere and the four implicit-point predicates (OrientLPI/OrientTPI/InCircleLPI/InSphereTPI) are proved against and the exact parametric/cell ordering key the `Meshing/intersect` `t`-chain and the `Meshing/arrangement` crossing-endpoint sort totally order on (`Compare`). `PeterO.Numbers` adds the arbitrary-precision BINARY floating-point tier (`EFloat`) between the fixed 106-bit `ddouble` and the rational oracle, serving as an INDEPENDENT external exact adjudicator in a different representation than `BigRational` (`EFloat` under an unlimited `EContext` is exact for the polynomial orient/incircle/insphere determinants over dyadic-double coordinates) that cross-validates the hand-rolled `Expansion`, plus `ERounding.Floor`/`Ceiling` software directed-rounding brackets for an interval-style filter. All three are pure-managed AnyCPU on osx-arm64.
- `TYoshimura.DoubleDouble`
- `ExtendedNumerics.BigRational`
- `PeterO.Numbers`

[GEOMETRY_KERNEL]:

Host-neutral managed geometry owners the kernel composes additively beside its first-principles authored kernels, for the concerns a production ecosystem package owns better than a hand-roll. `GShark` is the pure-managed NURBS engine (`NurbsCurve`/`NurbsSurface`/`Bezier` point+derivative `Evaluate`, interpolation/approximation/least-squares `Fitting`, curve-curve and curve-surface `Intersection`, adaptive `Sampling`, curvature/length/closest-point `Analyze`, `Modify`/`Optimization` Newton refine) — the host-neutral parametric contract `Rasm.Vectors`/`Geometry` exposes so a non-Rhino runtime evaluates curves/surfaces without RhinoCommon, retiring the NURBS-Book hand-roll. `Supercluster.KDTree.Net` is the array-backed flat 3D kd-tree exact k-NN (`NearestNeighbors`) + radius search (`RadialSearch`) feeding the `Processing/fit` MLESAC primitive-fit + normal-estimation and registration/ICP lanes — the low-dimensional point-NN leaf the SAH-BVH + Morton octree broad-phase (`Spatial/index`) serves poorly, additive to (not a replacement for) the BVH/octree primitive broad-phase. `LibTessDotNet` is the de-facto C# GLU/libtess2 winding-rule polygon tessellator (arbitrary self-intersecting/holey contours combined under EvenOdd/NonZero/Positive/Negative/AbsGeqTwo into triangle `Polygons`/`ConnectedPolygons` with a `VertexCombine` callback) — the messy-winding polygon-FILL triangulation the clean-PSLG Bowyer-Watson Delaunay (`Meshing/delaunay`) cannot own, backing the `Drawing/view` + `Drawing/pack` fill leg. All three are pure-managed AnyCPU on osx-arm64.
- `GShark`
- `Supercluster.KDTree.Net`
- `LibTessDotNet`

[COMPUTATIONAL_GEOMETRY]:

Computational-geometry leaf libraries the kernel composes for convex-hull/Delaunay/Voronoi/offset/boolean/triangulation/mesh concerns. All pure-managed AnyCPU on osx-arm64.
- `MIConvexHull` — 2D/3D incremental convex hull + Delaunay/Voronoi diagram construction
- `Clipper2` — robust integer-exact 2D/3D polygon boolean (union/intersection/difference/XOR) and parallel offset
- `CavalierContours` — arc-aware (bulge) polyline parallel offset + closed-polyline boolean, float-domain
- `SharpVoronoiLib` — Fortune's-algorithm point-site Voronoi with edge clipping, border closure, Delaunay dual, and Lloyd relaxation
- `Triangle` — Triangle.NET constrained/conforming Delaunay triangulation and refinement, float-domain
- `geometry3Sharp` — `DMesh3` dense half-edge mesh substrate (OBJ/STL/OFF I/O, boolean, remesh, repair)

[PROJECTS]:
- `Rhino.Geometry` / `RhinoCommon`
- `Rasm.Vectors`
- `Rasm.Compute.Solver`

## [03]-[SUBSTRATE_PACKAGES]

The C# substrate registry cards this folder consumes; full registry and substrate contracts live in `libs/csharp/.planning/README.md`, with shared API evidence in `libs/csharp/.api/`.

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
- `Verify.XunitV3`
