# [RASM]

`Rasm` is the geometry and numeric kernel below the C# app strata: a planning-scoped package that references no sibling and is referenced by every stratum above. Its design corpus lives under one `.planning/` root in nine sub-domain folders — `Domain` (the ROP/tolerance/identity/validation/normalization/evaluation/statistics substrate floor), `Numerics` (the exact-predicate floor plus the host-neutral-shaped dense/sparse/spectral/integration/analytic-calculus algebra), `Spatial` (proximity, clouds, neighborhoods, optimal transport, implicit fields, acceleration index, persistent naming), `Parametric` (the vendored in-kernel NURBS engine, the host-neutral curve/surface op rails, subdivision surfaces, isometric development, panelization, and pattern instancing, beside the Rhino evaluation selectors and the location algebra), `Meshing` (the mesh substrate, DEC assembly, reconstruction, the predicate-exact construction lattice, the slice-stack owner, and the MCF curve-skeleton), `Processing` (the algorithm suites: sampling, extraction, tracing, registration, geodesics, segmentation, repair, decimation, remeshing, flattening), `Solving` (the nonlinear least-squares owners: geometric constraint solving and robust primitive fitting), `Drawing` (hidden-line projection and geometry encoding), and `Analysis` (the measured-query public entry). Folder is domain, namespace is contract: the fences declare `Rasm.Domain`, `Rasm.Vectors`, `Rasm.Analysis`, and `Rasm.Geometry.*` per the frozen namespace matrix in `ARCHITECTURE.md`. The kernel is RhinoCommon-aware end to end; the pure-numeric floor is host-neutral-shaped (`double.IsFinite` + owned epsilon policies, never `RhinoMath`) without minting a host-free assembly, and the two doc-facing adapters (`Context.Of(RhinoDoc)`, `Analyze.From(RhinoDoc)`) are the only doc-coupled entries. This README routes the design pages and registers every external package the folder uses.

## [01]-[ROUTER]

[DOMAIN]:
- [01]-[RAILS](.planning/Domain/rails.md): The kernel ROP substrate — `Op` caller-member-name operation key, the `Expected`/`Fault` union, `Catch`/`Side` boundary-exception rail, `Lease<T>` resource rail, `IValidityEvidence` + the corpus-wide validity fold, the `GenerateUnionOps` generator contracts, and the one Op-threading law.
- [02]-[CONTEXT](.planning/Domain/context.md): The tolerance/units substrate — tolerance value objects and the immutable `Context` bundle with validated builders; `Of(RhinoDoc)` is the one doc-coupled boundary adapter.
- [03]-[IDENTITY](.planning/Domain/identity.md): The determinism owner — `ContentHash.Of` seed-zero XxHash128 federation content key plus the one splitmix64 deterministic-derivation owner.
- [04]-[VALIDATION](.planning/Domain/validation.md): The one acceptance/readiness oracle — `Requirement`/`Check` readiness algebra, `OpAcceptance.ValidityOf` extended by `IValidityEvidence` registration, and the canonical admission vocabulary.
- [05]-[NORMALIZATION](.planning/Domain/normalization.md): The Rhino-kind taxonomy and coercion owner — `Topology`/`Kind` smart enums with the capability web, `CurveForm`, the polymorphic coercion lattice, and the one disposable projection carrier.
- [06]-[EVALUATION](.planning/Domain/evaluation.md): The closest-point/evaluation lattice — the `ClosestHit` receipt, `ClosestOf` polymorphic evaluation, frames, surface sampling, and signed distance.
- [07]-[STATS](.planning/Domain/stats.md): The statistics substrate — `ScalarMetric` vocabulary, Welford `Stat`, tolerance-banded `Extrema`, `Distribution` quantiles, and the `SampleMoment` weighted mean/covariance owner.

[NUMERICS]:
- [08]-[PREDICATES](.planning/Numerics/predicates.md): Adaptive-precision exact-predicate floor — `Predicate` (Orient2D/Orient3D/InCircle/InSphere + the LPI/TPI implicit-point family) over the `PrecisionTier` four-tier `Adaptive.Resolve` ladder, the per-tier `ErrorBound` table, and the `RationalOracle` exact-rational adjudicator.
- [09]-[FAULTS](.planning/Numerics/faults.md): Consolidated band-2400 `GeometryFault` union every geometry rail routes through, on a 4-wide per-cluster stride inside the 2400–2449 century.
- [10]-[ATOMS](.planning/Numerics/atoms.md): The typed vector-algebra primitive floor — dimension/magnitude value objects, `Direction`/`VectorSpan`/`VectorFrame`/`VectorCone`, and the promoted `AtomProjection`/`ProjectionRow` raw-to-typed projection dispatch every `.Project<TOut>` routes through.
- [11]-[MATRIX](.planning/Numerics/matrix.md): The dense/sparse/complex linear-algebra owner — `Matrix`/`SymmetricMatrix`/`SparseMatrix`/`SparseHermitian` facades over MathNet+CSparse, `CholeskySparse`, `GaugePolicy`, and the `MatrixKernel` solve family with LOBPCG real+Hermitian eigen.
- [12]-[INTEGRATE](.planning/Numerics/integrate.md): The ODE/RK integration floor — the `IntegratorKind` 9-tableau data-driven vocabulary, `ButcherTableau` moment validation, dense-output coefficient families, and the `FieldIntegrator` adaptive stepper.
- [13]-[SPECTRAL](.planning/Numerics/spectral.md): The mesh-free spectral algebra — the `DiscreteCalculus` DEC operator bundle, `SpectralBasis`/`SpectralFilter` transfer-function algebra, and the `SpectralDescriptor` normalize/rank/distance algebra.
- [14]-[CALCULUS](.planning/Numerics/calculus.md): The sample-anywhere analytic math floor — the six-axis central-difference stencil (gradient/curl/divergence/Laplacian), `FieldNoise` procedural lattices, and the weight-kernel/falloff profile math.

[SPATIAL]:
- [15]-[INDEX](.planning/Spatial/index.md): Polymorphic `SpatialIndex` (SAH-BVH + Morton linear octree + agglomerative PLOC) over one frozen `NodeStore` behind ONE `Spatial.Apply(SpatialOp, Op?)` entry — the `SpatialQuery` range/ray/nearest/overlap/winding fold, persistent degradation-keyed refit, and the `Wire` node-array Compute seam.
- [16]-[NAMING](.planning/Spatial/naming.md): Persistent topological naming behind ONE `Naming.Apply(NamingOp, Op?)` entry — one `TopoName` lineage algebra, the `NameTable` registry, and the `Track`/`Resolve` re-anchor-by-signature fold with overlap-fraction migration.
- [17]-[RECONCILIATION](.planning/Spatial/reconciliation.md): Naming-to-hash fence behind ONE `Reconciliation.Apply(ReconcileOp, Op?)` entry — the `EncodeForm` {Mesh · Cloud · Parametric} frozen canonical byte streams, the `GeometryHash` content axis, the accumulating `Reconcile` traverse onto the `NamingHash` receipt Persistence consumes, and the `ONE_WIRE_FIXTURE_CORPUS` index.
- [18]-[SUPPORT](.planning/Spatial/support.md): The proximity boundary adapter — the `SupportProjection` 14-case capability-gated projection SmartEnum with the single `Project<TOut>` gate over the `SupportSpace` boundary adapter.
- [19]-[CLOUD](.planning/Spatial/cloud.md): The point-cloud owner — the `VectorCloud` union with lazy indexed admission, the `VectorCloudMetric` 30-case metric surface, the `CloudKernel` covariance/PCA owner, and the realized convex/concave/alpha hull rail.
- [20]-[NEIGHBORS](.planning/Spatial/neighbors.md): The one neighborhood substrate — kNN/radius/graph queries over Rhino `RTree` + `Supercluster.KDTree.Net`, Hoppe-MST normal orientation, quadric-fit curvature, the one rotation-minimizing-frame owner, and the absorbed spatial-probe query modalities.
- [21]-[TRANSPORT](.planning/Spatial/transport.md): The optimal-transport owner — log-domain stabilized Sinkhorn (balanced/unbalanced/debiased) with typed `SinkhornPlan` projections and cloud correspondences.
- [22]-[FIELDS](.planning/Spatial/fields.md): The implicit-field algebra — the `ScalarField`/`VectorField`/`TensorField` unions, the typed 12-primitive `SdfKind` family, the `BlendKind` smin algebra, and the status-tagged `SampleDetailed` sampling seam.

[PARAMETRIC]:
- [23]-[NURBS](.planning/Parametric/nurbs.md): THE vendored in-kernel NURBS engine — `Nurbs.Of(NurbsWire)` one polymorphic admission over four wire shapes, the normalized-clamped `KnotVector` algebra, De Boor evaluation with the fixed-convention `RationalDerivatives`, the Bezier-decomposed Gauss-Legendre arc-length engine, Wang-2008 RMF with closure correction, Piegl-Tiller curve AND surface fits, fundamental forms/curvature projections, and the `ToEncodeForm` identity projection.
- [24]-[CURVE](.planning/Parametric/curve.md): The host-neutral curve op rail — one `ParametricOp` eight-case `[Union]` (`Evaluate`·`Measure`·`Divide`·`Stations`·`Split`·`Reconstruct`·`Offset`·`Intersect2D`) folded by `Parametric.Apply` over the vendored `NurbsForm.Curve`; the `StationField` SoA producer for the Generation SpineRef window, the promoted deviation-refined `Offset` with exact-lattice trim, and the `Fill` overlay delegation.
- [25]-[SURFACE](.planning/Parametric/surface.md): The host-neutral surface op rail — one `SurfaceOp` six-case `[Union]` (`Tessellate`·`Isolines`·`Geodesics`·`NormalOffset`·`CurvatureSample`·`Pullback`) folded by `Surfaces.Apply`; `UvTessellation` the tier's UV-provenance carrier, real NURBS normal offsets through the Piegl-Tiller refit, and the kd-tree-seeded batch pullback.
- [26]-[SUBDIVIDE](.planning/Parametric/subdivide.md): Subdivision surfaces as stencil rows — Catmull-Clark/Loop as `SubdivisionScheme` data rows over ONE sparse-operator refinement fold, semi-sharp creases and region closure as policy data, and Stam eigenbasis limit evaluation with per-vertex limit columns on every refinement.
- [27]-[DEVELOP](.planning/Parametric/develop.md): Guaranteed-isometric developable strips — MMP-exact geodesic rails, the Brent/Broyden torsal ruling solve, exact rigid unroll, the `ddouble` per-strip isometry witness, and `ChartAtlas` emission with the sharper `DevelopmentReceipt` beside it.
- [28]-[PANELIZE](.planning/Parametric/panelize.md): Cross-field-guided panelization — `PanelFamily` data rows (`Lattice` consuming the remesh `QuadProvenance` substrate · `Seeded` over the sample suite and geodesic-Voronoi cells), per-panel placement frames, planarity acceptance with the bounded `Planarize` pass, and the `PanelField` SoA wire.
- [29]-[PATTERNMAP](.planning/Parametric/patternmap.md): Pattern-to-surface instancing — the theorem-closed 17 wallpaper-group rows over ONE Seitz orbit fold, the piecewise-linear log-map inversion with honest flip/clip censuses, and vector-heat-transported per-instance frames on the `InstanceStream`.
- [30]-[PROJECTIONS](.planning/Parametric/projections.md): The Rhino-native parametric evaluation selectors — `CurveProjection`/`SurfaceProjection` SmartEnums with the one shape-operator owner, `ConeProjection`, the one quaternion pose-slerp site, and the re-homed `SurfaceSpace` adapter.
- [31]-[LOCATE](.planning/Parametric/locate.md): The curve/surface location algebra — `Locator`/`LocationValue`/`Division` with curvature sampling, orientation, containment, and perpendicular frames, routed by the analysis query.

[MESHING]:
- [32]-[DELAUNAY](.planning/Meshing/delaunay.md): Author-kernel constrained Delaunay owner — one `Tessellation` over a flat `SimplexStore`, Bowyer-Watson insertion on the exact `InCircle`/`InSphere` predicates, and predicate-guarded constraint recovery.
- [33]-[ARRANGEMENT](.planning/Meshing/arrangement.md): Fully-managed exact-arithmetic mesh/polygon arrangement — implicit-point crossings, GWN inside/outside classification, `BooleanOp` cell welds, and the `BooleanReceipt`; retires the tier-3 native CSG gate.
- [34]-[INTERSECT](.planning/Meshing/intersect.md): Predicate-exact intersection lattice — one `IntersectOp` over an exact-sign `Crossing` carrier with exact parametric ordering.
- [35]-[SLICE](.planning/Meshing/slice.md): The slice-stack owner — one `Slicing.Apply` section fold composing `IntersectOp.PlaneMesh` over a `LayerPlan`-generated plane family (five height-law seed rows over ONE `March` integrator), oriented contours with typed open chains, exact-parity nesting into the transitive-reduced forest, and the `SliceStack` five-channel SoA wire.
- [36]-[OFFSET](.planning/Meshing/offset.md): Predicate-exact offsetting — one `OffsetOp` (skeleton/medial/minkowski/offset) over the Aichholzer-Aurenhammer wavefront with loop assembly through the arrangement.
- [37]-[SKELETON](.planning/Meshing/skeleton.md): The 3D MCF curve-skeleton owner — Au-2008 implicit contraction over the `MeshEdit` arena, cost-ordered collapse to 1D, Kruskal tree/branch extraction, and the `CurveSkeleton` SoA wire composing offset's clearance family (`ClearanceNode` radius + the `Clearance(probe)` query).
- [38]-[MESH](.planning/Meshing/mesh.md): The mesh substrate owner — the `MeshSpace` validated snapshot handle, the `LaplacianCache` memoization service, the frozen `IntrinsicMesh` intrinsic triangulation with `MeshAdjointSnapshot`, the one cotangent primitive, and the restricted power diagram.
- [39]-[EDIT](.planning/Meshing/edit.md): The mutable-arena tier — `MeshEdit`, the single-writer pooled SoA build arena with one polymorphic `Of` (space | soup), dirty-bitset tracking, partition-disjoint parallel folds, and the `ToSpace` publish-by-freeze seam; sites the weld kernel and the corpus-wide arena law.
- [40]-[DEC](.planning/Meshing/dec.md): The mesh-bound DEC assembly owner — `AssembleDecOperators`, Crouzeix-Raviart connection heat, CDS trivial-connection holonomy, the harmonic 1-form basis + Hodge decomposition family, and mesh-side spectral basis computation.
- [41]-[RECONSTRUCT](.planning/Meshing/reconstruct.md): The implicit-reconstruction owner — RBF/MLS/Levin/APSS/screened-Poisson kernels, the unified signed-heat spine across three discretizations, the three mesh-SDF methods, and marching-cubes iso-extraction.

[PROCESSING]:
- [42]-[REPAIR](.planning/Processing/repair.md): Repair rail — `Heal.Repair(HealPlan, Op?)` folds the `HealOp` closed algebra (six author-kernels + the managed arrangement-boolean delegation) over the `MeshEdit` arena with Genus-tolerant topology threading and the typed `RebuildReceipt` chain.
- [43]-[RECEIPTS](.planning/Processing/receipts.md): Typed `RebuildReceipt` family, the `ManifoldStatus` projection, and the `HealSession`/`RebuildLog` fold feeding the naming re-anchor.
- [44]-[DECIMATE](.planning/Processing/decimate.md): Predicate-guarded decimation/LOD — one `SimplifyOp` over a Garland-Heckbert QEM collapse queue and the `DecimationResult` carrier.
- [45]-[REMESH](.planning/Processing/remesh.md): The remesh substrate — one `RemeshOp` two-row `[Union]` (`Isotropic` Botsch-Kobbelt split/collapse/flip/relax/project · `QuadField` cross-field-guided quad extraction over the landed Knöppel machinery) folded by `Remeshing.Apply`, exact projected-convexity flip gates, the `RemeshTrace` receipt, and `QuadProvenance` the panelize substrate; the author-kernel robust tier beside `segment.md`'s host-capture tier.
- [46]-[FLATTEN](.planning/Processing/flatten.md): Robust parameterization/UV-flattening — one `ParamOp` (harmonic/LSCM/ARAP/BFF) over the DEC substrate returning the typed `ChartAtlas`.
- [47]-[INTENT](.planning/Processing/intent.md): The kernel consumer rail — the `VectorIntent` union with `Project<TOut>(Context, Op?)` preserved verbatim, case-family constructors with internalized admission, and dispatch that composes the owning pages.
- [48]-[SAMPLE](.planning/Processing/sample.md): The point-sampling owner — the `SampleKind` union (Bridson/farthest/Lloyd/capacity/elimination/Dwork/BNOT power-CCVT) with preset policy records and the `SampleKernel` dispatch over support/mesh/cloud domains.
- [49]-[EXTRACT](.planning/Processing/extract.md): The extraction/projection rail — polymorphic `ExtractionDomain` ingress, native-first `ContourPolicy` sectioning plus the marching-triangles isoline kernel, and the `Extraction` union re-dispatched through typed projection rows.
- [50]-[FLOW](.planning/Processing/flow.md): The streamline/trace owner — the `Termination` 6-stop union with dense-output event localization and `FlowKernel.Trace<TOut>` over any vector field.
- [51]-[REGISTER](.planning/Processing/register.md): The registration owner — the `AlignKind` 6-variant ICP dispatcher (Umeyama/Chen-Medioni/symmetric/robust-IRLS/normal-weighted/GICP) behind one `AlignmentPolicy` record.
- [52]-[GEODESICS](.planning/Processing/geodesics.md): The on-mesh distance/transport suite — heat-method and MMP exact geodesics, tangent log maps, exp-map and BVP backtrace, vector-heat parallel transport, and implicit mean-curvature flow.
- [53]-[SEGMENT](.planning/Processing/segment.md): The spectral shape-analysis and restructure owner — HKS/WKS descriptors, feature-edge detection, the `MeshSegmentation` 6-algorithm union, Knöppel cross-fields + stripes, and the host-native QuadRemesh/Reduce/unwrap capture tier.

[SOLVING]:
- [54]-[SOLVER](.planning/Solving/solver.md): The one nonlinear least-squares owner — `Lm.Minimize(ILmModel, SolvePolicy, Op?)` the corpus's ONE damped Gauss-Newton λ-ladder, plus the geometric constraint solver: the closed 15-case `Constraint` residual/Jacobian algebra, QuikGraph island decomposition with the König-matching `DofReport`, the witness-rank `DofAnalysis`, and the island-folded `ConstraintSolver.Solve`.
- [55]-[FIT](.planning/Solving/fit.md): Robust primitive-fit — one `Fit.Apply` over a kind-as-data `FitOp` (single or multi-kind detection), the MLESAC truncated-cost sampler with PROSAC/NAPSAC draw rows over the `Spatial/neighbors` kd-tree lane, and the orthogonal-distance refine instantiating `Lm.Minimize`, returning the typed `FitReceipt`.

[DRAWING]:
- [56]-[VIEW](.planning/Drawing/view.md): Exact hidden-line/silhouette projection — one `ViewOp` over an exact analytic quantitative-invisibility kernel returning the `DrawingProjection` visible/hidden segment carrier.
- [57]-[PACK](.planning/Drawing/pack.md): Canonical geometry-encoding owner — one `PackOp` over the six-row `PackKind` modality axis (including the `field`/`toolpath` rows) and the eight-row `EncodingChannel` lattice into the dtype-strided byte arena, returning `EncodedGeometry` with a lossless round-trip witness.

[ANALYSIS]:
- [58]-[QUERY](.planning/Analysis/query.md): The measured-query runtime and the kernel's public analysis entry — the one `AnalysisQuery` request algebra (absorbing the geometry-request band), the `Operation<TGeometry,TOut>` Build/Reject/Service algebra, the `Env` reader, and the `Analyze` facade.
- [59]-[MEASURE](.planning/Analysis/measure.md): The metrology owner — the `Measure` mass-property union, the `Bounds` union with Ritter enclosing fits and principal-frame OBB, and `ConformanceMetric` residual sampling.
- [60]-[INSPECT](.planning/Analysis/inspect.md): The diagnostics owner — the `Topologies` union with genus/Euler folds and the `Meshes` union over the full defect/quality capture.
- [61]-[SELECT](.planning/Analysis/select.md): The selection/extraction owner — the `Curves`/`Faces`/`Points` unions over the edge taxonomy, silhouette/draft capture, and PCA spread.
- [62]-[RELATIONS](.planning/Analysis/relations.md): The pairwise-relation owner — the 25-row RhinoCommon intersection lattice as table rows, `IntersectionHit`/`RayQuery`, deviation, self-intersection, and classification; the Rhino-parametric altitude beside the predicate-exact `Meshing/intersect`.

## [02]-[DOMAIN_PACKAGES]

The numeric solver and geometry host packages this folder consumes outside the C# substrate registry; versions are centralized in the one C# manifest, corroborated by `.api/`.

[NUMERIC_SOLVERS]:
- `MathNet.Numerics`
- `CSparse`

[EXACT_PRECISION]:

The predicate floor's precision ladder: `TYoshimura.DoubleDouble` supplies the middle-precision tier (106-bit hi/lo double-double, FMA `TwoProduct` + Knuth `TwoSum` transforms matching the kernel) that resolves near-degenerate determinant signs as a fast second stage below the `Expansion` exact branch, and ALSO the 106-bit accumulation tier for the cancellation-prone non-predicate reduces — the Garland-Heckbert QEM `Quadric` plane-sum/cost-evaluate (`Processing/decimate`) and the Levenberg-Marquardt objective `Σ r²` of the ONE `Solving/solver` `Lm.Minimize` λ-ladder (`ILmModel.Norm` returns `ddouble` by contract, so the constraint solve and the `Solving/fit` orthogonal-distance instantiation both keep the accept/reject digits), and the `Parametric/develop` per-strip isometry witness `Σ(‖e‖₃D − ‖e‖₂D)²` — narrowing to `double` only at the readout; `ExtendedNumerics.BigRational` supplies the exact-rational oracle (BigInteger numerator/denominator with exact `Sign`/`NormalizeSign`) that the Orient2D/Orient3D/InCircle/InSphere and the four implicit-point predicates (OrientLPI/OrientTPI/InCircleLPI/InSphereTPI) are proved against and the exact parametric/cell ordering key the `Meshing/intersect` `t`-chain and the `Meshing/arrangement` crossing-endpoint sort totally order on (`Compare`). `PeterO.Numbers` adds the arbitrary-precision BINARY floating-point tier (`EFloat`) between the fixed 106-bit `ddouble` and the rational oracle, serving as an INDEPENDENT external exact adjudicator in a different representation than `BigRational` (`EFloat` under an unlimited `EContext` is exact for the polynomial orient/incircle/insphere determinants over dyadic-double coordinates) that cross-validates the hand-rolled `Expansion`, plus `ERounding.Floor`/`Ceiling` software directed-rounding brackets for an interval-style filter. All three are pure-managed AnyCPU on osx-arm64.
- `TYoshimura.DoubleDouble`
- `ExtendedNumerics.BigRational`
- `PeterO.Numbers`

[GEOMETRY_KERNEL]:

The NURBS engine is VENDORED AND OWNED in-kernel at `Parametric/nurbs` — the whole MIT algorithm set re-based onto the kernel's own carriers (`Point3d`/`Vector3d`/`Plane`, homogeneous SoA control columns), with public arbitrary-knot construction for both carriers, fixed-convention raw surface derivatives, fundamental forms, Piegl-Tiller curve and surface fitting, and parameterized projection knobs — so NO NURBS package survives beside the owned source (the one-engine law) and the only geometry-owner package here is the kd-tree leaf. `Supercluster.KDTree.Net` is the array-backed flat 3D kd-tree exact k-NN (`NearestNeighbors`) + radius search (`RadialSearch`) backing the `Spatial/neighbors` static-point-set lane that feeds the `Solving/fit` MLESAC primitive-fit, normal estimation, the `Processing/register` ICP correspondences, and the `Parametric/surface` dense-pullback seed grid — the low-dimensional point-NN leaf the SAH-BVH + Morton octree broad-phase (`Spatial/index`) serves poorly, additive to (not a replacement for) the BVH/octree primitive broad-phase. Pure-managed AnyCPU on osx-arm64.
- `Supercluster.KDTree.Net`

[COMPUTATIONAL_GEOMETRY]:

The one computational-geometry leaf library the kernel composes; pure-managed AnyCPU on osx-arm64. The float-domain polygon boolean/offset/Voronoi/fill packages (`Clipper2`, `CavalierContours`, `SharpVoronoiLib`, `LibTessDotNet`) are NOT admitted here — the corpus owns those concerns exactly: `Meshing/arrangement` `PlanarOverlay` is the exact 2D polygon boolean and messy-winding fill (GWN cell classification over the CDT), `Meshing/offset` is the exact wavefront offset/skeleton, `Meshing/delaunay` owns the constrained Voronoi dual, and `Meshing/mesh` owns the restricted power diagram; the `Clipper2`/`CavalierContours`/`SharpVoronoiLib` float lanes live in `Rasm.Fabrication`, never in the robust core, and `LibTessDotNet` is retired outright — `Meshing/delaunay` subsumes its polygon-fill concern.
- `MIConvexHull` — 2D/3D incremental convex hull + Delaunay complex; realizes the `Spatial/cloud` convex/concave-outline/alpha-shape hull rail over `Triangulation.CreateDelaunay`
- `manifoldc` (in-house P/Invoke over `elalish/manifold`, Apache-2.0 — NO NuGet pin; the `Manifold` NuGet ID is an unrelated homonym) — the tier-3 guaranteed-manifold boolean SCALE companion behind `ArrangementPolicy.ScaleCeiling`, gated on RID-asset resolution + the golden-boolean fixture; the managed exact arrangement stays the ONE correctness rail

[PROJECTS]:
- `Rhino.Geometry` / `RhinoCommon` — the host compile surface; the kernel reads value structs and Mesh/Curve/Brep reference geometry, never `RhinoDoc`/`RhinoApp`/UI

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
- `CommunityToolkit.HighPerformance` — `Span2D`/`MemoryOwner<T>`/`ParallelHelper` on the SoA build arenas, frozen or partition-disjoint spans only

[GRAPH_ALGORITHM]:
- `QuikGraph` — the `Spatial/neighbors` Prim-MST normal-orientation lane (`MinimumSpanningTreePrim` over the kNN graph), the `Solving/solver` constraint-graph decomposition (`ConnectedComponents` islands + `MaximumBipartiteMatchingAlgorithm` König structural rank behind the `DofReport`), and the robust-core in-computation lanes under the bounded-lane law — `Meshing/slice` nesting DAG + transitive-reduction forest, `Meshing/skeleton` Kruskal tree/branch extraction, `Processing/remesh` patch labelling, `Parametric/develop` strip layout MST, `Parametric/panelize` panel adjacency — every graph RESULT leaving as a kernel-owned SoA wire, never a stored graph field; the `Processing/segment` normalized cut is spectral through the `Numerics/matrix` owner, not a combinatorial cut

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `Verify.XunitV3`
