# M2 — KERNEL BUILD PLANES: LANDED-TRUTH INVENTORY

Scope: `libs/csharp/Rasm/.planning/{Meshing,Processing,Parametric,Solving,Drawing}` — 33 pages read in full.
All owners are `Fin<T>`-railed over the band-2400 `GeometryFault` union with `Op? key` threading; no exception crosses a public surface. Fault codes cited inline.

---

## [01]-[MESHING] — 10 pages

### mesh.md — mesh substrate owner
- `MeshSpace.Of(Mesh native, Context context, MeshAssemblyPolicy? assembly = null, Op? key = null) -> Fin<MeshSpace>` — validated defensive-snapshot admission; one `DuplicateMesh`; assembly policy fixed for snapshot lifetime.
- `space.Laplacian(MeshLaplacian kind, Op? key = null) -> Fin<SparseLaplacian>`; `MeshLaplacian` SmartEnum 3 rows (`Cotangent`/`IntrinsicDelaunay`/`TuftedIntrinsic`) with `Select`/`Snapshot` delegates.
- `MeshAdjointSnapshot.Of(MeshSpace, Op?) -> Fin<MeshAdjointSnapshot>` — THE public cross-package adjoint seam (projects cached `DiscreteCalculus`); `IntrinsicMesh` stays internal.
- `MeshKernel.TopologyDetailed(MeshSpace) -> Fin<TopologyReceipt>` — total 19-field witness, never fails; `(Euler,Genus,BoundaryComponents)` projection.
- `MeshKernel.RestrictedPowerCells(MeshSpace, Seq<Point3d> sites, Option<Arr<double>> weights, Option<ScalarField> density, Op) -> Fin<RestrictedPowerDiagram>` — sole consumer: sample.md `PowerCcvtRun`.
- Laws: `LaplacianCache` keyed by `ConditionalWeakTable` on snapshot `Mesh` reference identity (cache dies with snapshot); success-only memos (failed solve never cached); `Cotangent` (`OfLengths`/`OfEdges`/`AngleOfLengths`) is THE one cotangent primitive; `Memoized<TKey,T>` anti-aliasing law — one key-record type per solver family.
- Handroll bait: inline `(a·b)/2A` cotangent; keyed-dictionary cache (leaks + aliases); caching failed solves; per-solver named cache accessors.

### edit.md — mutable-arena tier (`MeshEdit`) + ARENA_LAW anchor
- `MeshEdit.Of(MeshSpace space, ArenaPolicy? policy = null)` AND `MeshEdit.Of(ReadOnlySpan<Point3d> vertices, ReadOnlySpan<(int A,int B,int C)> faces, ArenaPolicy? policy = null)` — ONE polymorphic `Of`, TOTAL (no rail); argument type discriminates.
- `edit.ToSpace(Context, Op?) -> Fin<MeshSpace>` — the ONE publish/freeze seam. Mutation verbs: `AddVertex`/`AddFace`/`SetPosition`/`SetFace`/`KillFace`/`Touch`. `Parallel<TAction>(int extent, in TAction) where TAction : struct, IAction`. `IDisposable` (pooled).
- `Kernels.WeldDuplicates(MeshEdit) -> MeshEdit` (idempotent tolerance-grid union-find); `Kernels.QuadDiagonal(Point3d a,b,c,d) -> bool` (exact `Orient2D` quad-split gate).
- `ArenaPolicy(int Capacity, double WeldTolerance, int ParallelFloor)`, `.Canonical` (1024, 1e-6, 4096). WeldTolerance lives HERE — no healing-policy reach-through.
- **ARENA_LAW** (corpus-wide contract, this page owns it): single-writer, no lock/CAS; concurrency only via frozen projection or partition-disjoint spans; publish-by-freeze; `XxHash128` binds ONLY frozen projections; amortized doubling (input-derived `2n` guess rejected); mutation total, faults surface at freeze. Governs sibling arenas: `SimplexStore` (delaunay), `CrossingStore` (intersect), `PatchStore` (arrangement), `WavefrontStore` (offset), `QuadricStore` (decimate), spatial `NodeStore`.
- Two-carrier seam: `MeshSpace` (immutable snapshot) ⟂ `MeshEdit` (build arena) are the ONLY mesh carriers; a third carrier is the named defect. Soup adapter lives here ONCE.
- Handroll bait: an immutable-record edit state with `with`-copy mutation (third carrier); per-consumer `Soup(MeshSpace)` `DuplicateNative` copy; raw `Parallel.For` beside the budgeted surface; `2n` fixed sizing; local welder.

### intersect.md — predicate-exact crossing lattice (E7 collapse: THE one crossing kernel)
- `Intersection.Apply(IntersectOp op, Op? key = null) -> Fin<IntersectResult>`.
- `IntersectOp` [Union] 7: `SegmentSegment(Line,Line,Axis,IntersectPolicy)` · `SegmentTriangle` · `TriangleTriangle` · `RayMesh(Ray3d,double MaxT,MeshSpace,…)` · `MeshMesh` · `SelfMesh` · `PlaneMesh(Plane,MeshSpace,…)`. `IntersectResult` [Union] 3: `Points(Seq<Point3d>)` · `Segments(Seq<Line>)` · `Chains(Seq<Chain>, CrossLattice)`.
- **Re-founded constructions**: `CrossKey(int Side,int EdgeU,int EdgeV,int Face,int OtherU=-1,int OtherV=-1)`; `Crossing(Implicit Point, CrossKey Key)`; `CrossLattice(Crossing[] Rows, segments, coplanar)` with memoized `OnFace`/`CoplanarOnFace`. Same physical crossing from two adjacent face pairs interns to ONE row by integer equality — the cross-face merge no float weld expresses. Existence by `Orient3D`/`Orient2D`; point as `Implicit` (`Lpi`/`Ssi`); `Round()` at emission only. Dead prior form: `Site`+`OrderKey`+`RationalKey` + `ExtendedNumerics.Fraction`.
- Manifold-inheritance law: interior section endpoint has one incoming + one outgoing material-oriented segment; forward walk closes loops outer-CCW/holes-CW; second in/out edge = typed non-manifold fault.
- Faults: `DegenerateInput` 2400; `IntersectionFault(Kind.A, Kind.B)` 2424. `PrimitiveKind` (5) MINTED here.
- Handroll bait: sibling `SegmentIntersector`/`MeshIntersector`/`PlaneSectioner` family; rounded `Point3d` at birth beside exact key; proximity-keyed endpoint merge; kind-keyed concat of unoriented fragments; local all-pairs scan or second accel structure; nearest-box single re-test for `RayMesh` (false-miss class).
- Recorded growth (NOT landed): plane-slab `SpatialQuery` case for the slice consumer. Altitude boundary: `Analysis/relations.md` owns host NURBS/Brep parametric intersection.

### delaunay.md — exact CDT/CDTet owner
- `Tessellation.Build(TessellationOp op, Op? key = null) -> Fin<Tessellation>`; `TessellationOp` [Union] 3: `Points(TessellationKind, Implicit[], Seq<Constraint>, TessellationPolicy, Axis, Option<(Point3d,Point3d,Point3d)> Support)` · `Insert(Tessellation, Implicit)` · `Recover(Tessellation, Seq<Constraint>)`.
- Projections on `Tessellation`: `ToMesh(Context, Op?) -> Fin<MeshSpace>` (via `MeshEdit`/`ToSpace` — the one path); `Triangles(Op?) -> Fin<(Point3d,Point3d,Point3d)[]>`; `VoronoiDual(Op?) -> Fin<DualGraph>`; `LowerHull(Context, Op?) -> Fin<MeshSpace>` ([V11] envelope, tier-split with `Spatial/cloud` hull).
- `Constraint` [Union] 3: `Segment(int,int)` · `Facet(int[])` · `Crossing(int A,int B, Point3d P,Q,R)` (carries other operand's face plane so recovery splits re-anchor over original points — depth-1 seal, depth-2 implicit-over-implicit forbidden).
- Faults: 2400; `ConstraintUnrecoverable(constraint, budget)` 2421; `DegenerateTessellation` 2422. Index law: `Triangles()[i]` ≡ `VoronoiDual` node `i`.
- Recorded growth (NOT landed, typed-fault-gated): multi-implicit in-circum `{IIEE..IIII}`, 3D multi-implicit orientation, exact circumcenter side-of.
- Handroll bait: `DelaunayTriangulator`/`Tetrahedralizer` sibling classes; rounded-at-birth `IsImplicit` carrier; epsilon in-circle; Triangle.NET/TetGen admission; second hull owner.

### arrangement.md — boolean/arrangement owner
- `Arrangement.Apply(ArrangementOp op, Op? key = null) -> Fin<ArrangementResult>`; `ArrangementOp` [Union] 3: `MeshBoolean(MeshSpace A, MeshSpace B, BooleanOp, ArrangementPolicy)` · `PlanarOverlay(Seq<Polyline> A, Seq<Polyline> B, BooleanOp, Axis, ArrangementPolicy)` · `CellComplex(MeshSpace, MeshSpace, ArrangementPolicy)`. Results: `Boolean(MeshSpace Solid, BooleanReceipt)` · `Overlay(Seq<Chain>, BooleanReceipt)` · `Complex(CellSet, BooleanReceipt)`.
- `BooleanOp` SmartEnum 3 RE-HOMED here (`Union`/`Difference`/`Intersection`), each `int Native` + delegate `bool Region(bool inA, bool inB)`; `Keep`/`Flip` DERIVED over `Region` — one classification, never three keep bodies. `BooleanReceipt(int Classified, int Kept, int Welded, BooleanRoute Route)` owned here, composed by Processing/receipts.
- Scale gate: managed exact arrangement is THE correctness rail; `manifoldc` native route ONLY when faces > `ScaleCeiling` (1_000_000) AND RID asset resolves, else `NativeAssetMissing("manifoldc",…)` 2423 — never silent degrade. `DegenerateArrangement` 2420.
- Consumers: repair `HealOp.Boolean` delegates here; offset loop-resolution + Drawing `Fill` + Parametric `Fill` route `PlanarOverlay`; `Rasm.Bim` solid classifier reads un-welded `CellSet`; Fabrication NFP robust tier.
- Handroll bait: sibling `MeshBooleanOp`/`PolygonOverlay`/`CellComplexBuilder` family; three enumerated keep bodies; in-page crossing/triangulation kernel; bit-equality `Dictionary<Point3d,int>` interning; per-patch GWN loop instead of ONE batched `SpatialQuery.Winding` per operand.

### dec.md — mesh-bound DEC assembly (internal tier; `csharp contract` fence, private bodies signature-pinned by design)
- `DecAssembly.Build(MeshSpace, [MeshLaplacian,] Op) -> Fin<DiscreteCalculus>`; `BuildCrouzeixRaviartHeatSystemDetailed`; `DistributeHolonomy`; `HodgeDecomposeDetailed`/`HodgeSolutionOf`/`HodgeVectorAt` (memoized via mesh cache); `WhitneyVectorAt`; `ComputeSpectralBasisDetailed -> Fin<SpectralBasisBundle>`.
- Owns `HodgeDecompositionReceipt` (18 fields) + `HodgeDecomposition`; populates settled `Numerics/spectral` carriers (`DiscreteCalculus`/`SpectralBasis`) — re-declaring them is the named collapse violation (Compute adjoint binds those spellings BY NAME).
- Laws: `∂∂ = 0` residual gate at `SqrtEpsilon`-band; harmonic dimension `2g + max(0, b−1)` admits ZERO (sphere succeeds with `η ≡ 0`); co-exact by orthogonality (no indefinite hot-path solve); one-discretization law (operators + mass on one `MeshLaplacian.Snapshot` row).
- Handroll bait: local cotangent expression; standalone harmonic basis beside `calculus.Harmonic`; per-vertex potential-gradient `HodgeProjectedAt` approximation instead of the genuine Whitney lift; re-running assembly instead of reading cache memos.

### offset.md — exact wavefront offsetting; MINTS the clearance family
- `Offsetting.Apply(OffsetOp op, Op? key = null) -> Fin<OffsetResult>`; `OffsetOp` [Union] 6: `Skeleton(Polyline,OffsetPolicy)` · `Weighted` · `Offset(Polyline,double Distance,JoinType,EndType,OffsetPolicy)` · `Medial` · `Minkowski(Polyline Ring, Polyline Element,…)` · `Clearance(Polyline, Point3d Probe,…)`. `OffsetResult` [Union] 4: `Graph(SkeletonGraph)` · `Axis(SkeletonGraph)` · `Curves(OffsetCurves)` · `Probe(ClearanceNode)`.
- **Clearance family (THE kernel vocabulary, minted ONCE here)**: `ClearanceNode(Point3d At, double Radius, int NearestEdge)`; `SkeletonArc(int From, int To, int OriginEdge)`; `SkeletonGraph(Seq<ClearanceNode> Nodes, Seq<SkeletonArc> Arcs)`; `OffsetCurves(Seq<Chain> Loops, double Distance)`.
- `JoinType`/`EndType` SmartEnums (4 rows each) carry `Corner`/`Cap` delegates. One `Propagate` wavefront serves Skeleton/Weighted/Offset (Weighted = same queue at non-unit `EdgeSpeed`). Medial is REAL against delaunay `VoronoiDual`. Minkowski = one support-vertex walk (element gated convex by exact turn signs). Loop resolution routes `Arrangement.Apply(PlanarOverlay, Union, nonzero)`.
- Faults: `DegenerateOffset` 2416; `SkeletonStalled` 2417; `CollapseStalled` 2418.
- Handroll bait: `StraightSkeleton`/`MedialAxis`/`MinkowskiSum`/`PolygonOffset` sibling family; radius-less medial node or per-consumer clearance record; local four-sign straddle copy; `Mesh.CreateFromClosedPolyline` loop round-trip; two-pointer normal merge for Minkowski (unsound non-convex); exact-zero event guard on float trajectory positions.

### skeleton.md — 3D curve-skeleton (Au-2008 MCF) [concurrent redteam edit — read complete/consistent; treat provisional]
- `Skeletonize.Apply(SkeletonOp op, Op? key = null) -> Fin<CurveSkeleton>`; `SkeletonOp(MeshSpace Mesh, SkeletonPolicy Policy)`. Probe rides the result — no `Contract`/`ExtractSkeleton`/`ProbeClearance` siblings.
- `CurveSkeleton(double[] NodeX, NodeY, NodeZ, double[] Radius, int[] Witness, int[] ArcFrom, ArcTo, ArcOrigin, Component)` — SoA wire; projections `SkeletonGraph Graph` and `ClearanceNode Clearance(Point3d probe)`. Mints ZERO new clearance types — composes offset.md's family verbatim.
- Watertight gate via `TopologyDetailed` conjunction. Contraction owns its own assembly loop (composes `Cotangent.OfEdges` + `SparseMatrix.FromTriplets`/`CholeskySparse`) — deliberate split from the quality-gated substrate row. Surgery: cost-ordered collapse over FACE-BEARING edges only; union-find provenance recovers `Radius`/`Witness`. QuikGraph Kruskal MST transient in-computation only. geodesics.md's MCF arm is the SCALAR-FIELD owner — two MCF forms, one anchor each, no interior meet.
- Faults: 2400/2417/2418 (shared offsetting cluster).

### slice.md — slice-stack owner [concurrent redteam edit — read complete/consistent; treat provisional]
- `Slicing.Apply(SliceOp op, Op? key = null) -> Fin<SliceStack>`; `SliceOp(MeshSpace Mesh, Plane Datum, LayerPlan Plan, SlicePolicy Policy)`.
- `LayerPlan` [Union] 5 height-law rows over ONE `March`: `Uniform(double Height)` · `Adaptive(double CuspHeight, MinHeight, MaxHeight)` · `BySlope(Arr<(double SlopeCeiling,double Height)>)` · `SupportInterface(double BaseHeight, InterfaceHeight, int InterfaceLayers, double OverhangCosine)` · **`AtElevations(Arr<double> Elevations)`**; folded by `Elevations(SliceFrame, SlicePolicy) -> Fin<Arr<double>>`.
- `SliceStack(double[] Elevations, int[] LayerPtr, ContourPtr, double[] X,Y,Z, int[] Parent, ChildPtr, Children, Open)` — five-channel SoA forest wire; `ContourAt(int)->Chain`, `LayerAt(int)->Seq<Chain>`, `RootsOf`, `Depth`, `IsOpen`.
- Composes `Intersection.Apply(PlaneMesh)` over a parallel-plane family — never a 7th IntersectOp case, never a second crossing kernel. Contours arrive ORIENTED from intersect (no slice-side re-orientation). Nesting = exact-parity containment (`Predicate.Compare` half-open ray + `Orient2D` signs); QuikGraph `ComputeTransitiveReduction` projects `Parent`; laminar DAG gate. Non-watertight → typed `Chain(Closed:false)` rows or `SectionFault` 2425 under `RequireWatertight`.
- Handroll bait: slice-local plane sweep/chain walker; contour re-orientation pass; float point-in-polygon; O(C²) immediate-parent scan; `Seq<Seq<Chain>>` dual carriage beside the channels.

### reconstruct.md — implicit reconstruction
- `Reconstruction.Reconstruct(Seq<MlsSample> samples, ReconstructionPolicy policy, Context, Op?) -> Fin<ReconstructionResult>`; `ReconstructionPolicy` [Union] 5 (`Rbf`/`Mls`/`Levin`/`Apss`/`Poisson` factories) — the policy case IS the mode selection.
- `SignedHeatSpine.Solve(SignedHeatDiscretization, Op?) -> Fin<SignedHeatOutcome>` — ONE four-stage law (heat → unit-gradient divergence → gauge-fixed Poisson → sign calibration) × 3 discretization rows (`TetFem`/`BoundarySource`/`ClosedVolumeGrid`).
- `MeshSdf.SignedDistanceDetailed(MeshSpace, SdfMeshPolicy, Point3d, Op?) -> Fin<SdfMeshSample>`; `MeshSdf.Prewarm(…) -> Fin<SdfMeshReceipt>`. `IsoSurface.Detailed(ScalarField, BoundingBox, int resolution, IsoSurfacePolicy, Context, Op?) -> Fin<IsoSurfaceResult>`.
- `MlsSample(Point3d Position, Vector3d Normal, double Value)` — the ONE sample carrier. SignConvention/Method stay OUTSIDE memo keys (one cached solve serves both). `PoissonReceipt` conservation law `Contribution+Rejected+Clamped == SampleCount`.
- Consumers: `Spatial/fields` frozen `ScalarField` cases delegate sampling here; Processing/extract consumes `IsoSurface.Detailed`.
- Handroll bait: a discretization implementing its own heat→Poisson pipeline outside the spine rows; page-local GWN walker (must route `SpatialQuery.Winding`); raw MathNet/CSparse solve; clamp-to-edge trilinear sampler; re-admitting the dropped octree knobs.

---

## [02]-[PROCESSING] — 12 pages

### receipts.md — shared healing receipt vocabulary (carriers only, no entry)
- `ManifoldStatus(int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus)` + derived `GenusClosed` — a re-read projection of `TopologyReceipt`, never a second manifold computation.
- `RebuildReceipt` [Union] 7 typed per-op cases (`DegenerateReceipt`/`GapReceipt`/`WeldReceipt`/`ManifoldReceipt`/`SelfIntersectReceipt`/`OrientReceipt`/`MergeReceipt(BooleanOp, BooleanReceipt,…)`) — boolean payload IS arrangement's `BooleanReceipt`. Mint: `RebuildReceipt.Of(HealOp, RepairPolicy, before, after, MeshEdit, Option<BooleanReceipt>)`.
- `RebuildLog(Set<int> Vertices, Edges, Faces, Seq<HealStage> Ops)`; `HealSession(MeshSpace Input, MeshSpace Healed, Seq<RebuildReceipt> Receipts)`; `ToLog()` gated by `HealStage.RebuildsTopology`.
- Consumers: `Spatial/naming` `Track` (re-anchor seed from `RebuildLog`), `Spatial/reconciliation` `Encode` hashes `HealSession.Healed`. Mints NO hash. Boundary-count movement is evidence, never law.

### repair.md — predicate-gated heal fold
- `Heal.Repair(HealPlan plan, Op? key = null) -> Fin<HealSession>`; `Heal.Standard : Seq<HealOp>` DERIVED off `HealStage.Mint` roster (weld→degenerate→gap→manifold→orient→self-intersect).
- `HealOp` [Union] 7: 6 stateless + `Boolean(BooleanOp Op, MeshSpace Tool)` — delegates to `Arrangement.Apply`. `HealStage` SmartEnum 7 is the `UnrepairableMesh(HealStage,int,int)` **2408** payload + receipt discriminant.
- `RepairPolicy(PositiveMagnitude GapMaxSpan, double SliverAreaFloor, Dimension MaxManifoldPasses, ArenaPolicy Arena, IntersectPolicy Intersect, TessellationPolicy Retile, ArrangementPolicy Arrangement)`.
- Laws: forward topology threading `before[n]=after[n-1]`; N ops = N freeze+projection pairs; `Incidence` carry (mutating kernel drops it — stale fold unrepresentable); exact `Orient2D` sliver signs; crossing/CDT/boolean delegate to siblings.
- Handroll bait: per-defect `Welder`/`GapCloser`/`Orienter` classes; epsilon cross-product sign; local crossing kernels; repair-local kd/grid; second CSG kernel.

### decimate.md — decimation/LOD [concurrent edit — read complete]
- `Simplify.Apply(SimplifyOp op, Op? key = null) -> Fin<DecimationResult>`; `SimplifyOp` [Union] 4: `QuadricCollapse` · `ProgressiveMesh` · `VoxelRemesh` · `FeaturePreserve` (each `(MeshSpace Mesh, SimplifyPolicy Policy)`). Tolerance = `op.Mesh.Tolerance` — no second tolerance param.
- `SimplifyKind` SmartEnum rows carry `Weigh` delegate; `DecimationResult(MeshSpace Mesh, int Vertices, Faces, RequestedFaces, double Hausdorff, Seq<FeatureEdge> Features, Seq<VertexSplit> Splits)`; `VertexSplit` = reversible record.
- Faults: `DecimationFault(FaceBudget, Achieved)` **2440**; 2400; Hausdorff-over-ceiling via `key.InvalidResult`.
- Laws: exact-plane collapse gate (`Orient3D` of moved fan vs ORIGINAL supporting plane; `Zero` refuses); boundary-admitting link condition (open meshes decimate along rims); 106-bit `ddouble` quadrics; directed one-sided Hausdorff rides ONE `Spatial.Apply` Nearest + `TensorPrimitives.Max`.
- Compute consumers: tile pyramid (`Mesh`+`Hausdorff`), multigrid coarse seed, meshlet residency (`Faces`), continuous-LOD (`Splits` replay). `VoxelRemesh` (defective-input SDF resample) DISTINCT from remesh.md (valid-mesh re-tessellation). `Alimer.Bindings.MeshOptimizer` is Compute's residency binding, never this kernel.
- Handroll bait: sibling decimator classes; vacuous before/after flip test; O(F²) face-table scans; dead phantom `field.IsoSurfaceDetailed` instance spelling (real: static `IsoSurface.Detailed`).

### remesh.md — Botsch-Kobbelt rewrite [concurrent edit — read complete/coherent]
- `Remeshing.Apply(RemeshOp op, Op? key = null) -> Fin<RewriteResult>`; `RemeshOp` [Union] 2: `Isotropic(MeshSpace, double TargetLength, RemeshPolicy)` · `QuadField(MeshSpace, double TargetLength, int Symmetry, RemeshPolicy)`.
- `RewriteResult(MeshSpace Mesh, RemeshTrace Trace, Option<QuadProvenance> Quads)`; **`QuadProvenance(Arr<int> Corners, Arr<int> PatchOf, Arr<double> U, Arr<double> V, Arr<int> SingularFaces)`** — the panelize substrate wire.
- Faults: 2400; `RemeshStalled(targetLength, achieved, iterations)` **2441**.
- Laws: split→collapse→flip→relax→project over ONE `MeshEdit` arena; flip gate = TWO composed `Kernels.QuadDiagonal` probes (float dihedral deleted); relax = area-weighted equalizer deliberately NOT cotangent; projection targets ORIGINAL surface (BVH once at pass-zero). `QuadField` composes segment.md `CrossFieldAt`/`StripeAt`.
- Three remesh tiers coexist by charter: segment.md [06] host-capture (`ApplyRemeshDetailed` native QuadRemesh/Reduce) — remesh.md author-kernel — decimate.md `VoxelRemesh`/`QuadricCollapse`. Compute [V12]/[V7] consumes `Isotropic` as volumetric boundary-conditioning pre-step through the frozen `MeshSpace` wire.

### flatten.md — UV parameterization
- `Flatten.Apply(ParamOp op, Op? key = null) -> Fin<ChartAtlas>`; `ParamOp` [Union] 4: `Harmonic(MeshSpace, Option<Polyline> Boundary, ParamPolicy)` · `Lscm` · `Arap` · `Bff(MeshSpace, Option<Arr<double>> TargetCurvature, ParamPolicy)`.
- **`ChartAtlas(MeshSpace Source, Seq<UvIsland> Islands, Seq<FeatureEdge> Seams, DistortionReceipt Receipt)`** + `ToMesh`/`ToTextureMesh`; `UvIsland(ChartId Chart, Arr<int> Vertices, Arr<(int,int,int)> Faces, Arr<Point2d> Uv)`; `ChartId` [ValueObject<int>] minted here — the `ParameterizationFault(ChartId,double)` **2432** payload.
- `DistortionReceipt(double MaxConformal, MeanConformal, MaxArea, MeanArea, MaxQuasiConformal, int Iterations, double Residual, int FactorNonZeros, bool FlipFreeBijective)`.
- Laws: eliminate-boundary-rows reduced SPD system, factored ONCE (`CholeskySparse`, pin-set-keyed memo) — no penalty/diagonal-shift; `Lscm` = sparse spectral conformal (LOBPCG k=3); `Arap` seeds from Lscm; `Bff` boundary-first curvature prescription; exact `Orient2D` flip bit — a flip ALWAYS refuses; islands via QuikGraph `WeaklyConnectedComponents` over face-dual, wedge-duplicated seam vertices.
- Consumers: Fabrication unroll/nesting (`DistortionReceipt.MaxArea`/`MaxConformal` strain bound + `UvIsland` layout); AppUi texture lane; Parametric/develop CONTRASTS against it (flatten = low-distortion charts, develop = guaranteed-isometric strips — one anchor each).
- Handroll bait: raw CSparse/`Matrix<double>.Svd()`; penalty pinning; Geometry-side cotangent re-assembly; float signed-area flip band; vertex-labeled cut dropping seam-straddling faces.

### geodesics.md — on-mesh distance and transport (`GeodesicKernel`, internal; surfaced via frozen field cases + intent)
- Heat: `HeatGeodesicAt(MeshSpace, Seq<int> sources, Point3d, Op) -> Fin<double>`; `GeodesicTangentAt`; `MeanCurvatureMagnitudeAt(MeshSpace, double timeStep, int iterations, Point3d, Op)`.
- Exact: `PropagateWindows(IntrinsicMesh, int source, WindowPropagationPolicy) -> WindowPropagation` (total, MMP windows); `TraceStraightestGeodesic`; `BacktraceGeodesicToSource -> Option<BvpTrace>`; ONE `WalkChart` unfold serves IVP exp + BVP log + `EdgeOverlay`.
- Transport: `VectorHeatAt(MeshSpace, Seq<(int Vertex, Vector3d Direction)>, double time, Point3d, Op)`; `TangentLogMapAt(MeshSpace, int source, Point3d, double time, TangentLogMapAlgorithm, GeodesicTracePolicy, WindowPropagationPolicy, Op) -> Fin<TangentLogMapResult>` (ONE surface, 3-arm SmartEnum `VectorHeatApproximate`/`ExactStraightestExp`/`ExactWindowPropagation`); `ExactExpMapAt`.
- Laws: `t=h²` scale-derived (never a knob); pinned singular Poisson `GaugePolicy.Pinned + GaugeShift.MinZero` → nonnegative distances; flipped IDT → `Unsupported` never silent-extrinsic; LOG_EXP_WITNESS independence (`TracedLength` vs `FieldDistance` disjoint, agreement `PathRelativeResidual ≤ SqrtEpsilon` gates direction recovery); unreachable keeps `+∞` (no fabricated values); segment law `SegmentCount = EdgeCrossingCount + VertexPassCount + 1` total on every stop kind; `FrameBundle` = THE per-mesh tangent-frame owner.
- Handroll bait: third cotangent path; second closest-face interpolation owner (six per-solver helpers → `MeshProbe`); second tangent-frame derivation; `HeatTime` knob on distance entry; zero-substitution for unreachable; endpoint-interpolation window distance.

### sample.md — point sampling
- `SampleKind` [Union] 12 cases (`Explicit`/`PoissonDisk`/`Farthest`/`Optimize`/`Lloyd`/`Capacity`/`Weighted`/`ScalarDensity`/`Adaptive`/`SampleElimination`/`DworkVariableDensity`/`PowerCcvt`), one `Fin<SampleKind>` factory each; entry `kind.Project<TOut>(ExtractionDomain domain, Context, Op?)` (`TOut` ∈ `Seq<Point3d>`/`VectorCloud`/`PointCloud`/`SampleReceipt`); kernel `SampleKernel.Sample(SampleKind, ExtractionDomain, Context, Op) -> Fin<SampleResult>` — ONE dispatch over support/mesh/cloud/candidate domains.
- `PowerCcvtPolicy` grouped family (22-knob ctor dead): nested `CapacityPolicy`/`MotionPolicy`/`ArmijoPolicy`/`RegularityPolicy` + `PowerCcvtGauge`; BNOT composes `MeshKernel.RestrictedPowerCells` + `SparseMatrix.SingularSolveDetailed` — never re-implements. Receipts: `SampleReceipt` → `Option<SampleAlgorithmReceipt>` (one evidence stream) → `Option<DworkReceipt>`/`Option<PowerCcvtReceipt>` (~34 fields)/`Option<MeshSamplingSpectrumReceipt>`.
- Determinism through `Domain/identity` splitmix64 (`Deterministic.OrderKey`/`UnitInterval`) — private hash twin dead. ONE `Spacing` fold.
- Handroll bait: per-algorithm sampler class; parallel per-algorithm receipt; page-local splitmix; raw-knob ctor; local `RestrictedPowerDiagram`; discrete-Sinkhorn reuse for continuous OT.

### segment.md — spectral shape analysis + restructure (ONE `SegmentKernel` owner, 6 sub-domains)
- Descriptors: `DescribeShape<TOut>(MeshSpace, MeshDescriptor, int eigenpairs, Op)`; `SpectralDistanceAt`; `ValidateSamplingSpectrum(space, SampleResult, Op, double lowFrequencyCeiling=0.5) -> Fin<SampleResult>` (blue-noise gate `low/total ≤ 0.5` → `MeshSamplingSpectrumReceipt` stamped into sample receipts).
- Features: `DetectFeatureEdgesDetailed(space, MeshFeaturePolicy, key) -> Fin<FeatureReceipt>`; `FeatureEdge(int A, int B, MeshFeatureKind Kind, Option<double> DihedralRadians, SignedDihedralRadians, CurvatureSignal)`; `MeshFeatureKind` SmartEnum 8. Consumed by decimate/flatten/view.
- Segmentation: `Segment<TOut>(MeshSpace, MeshSegmentation, Op)`; `MeshSegmentation` [Union] 6 frozen cases (`ScalarThreshold`/`ScalarBands`/`SeededRegionGrow`/`DescriptorClusters`/`Watershed`/`NormalizedCut`), one adjacency/scalar-derivation/component-split shared; NaN = mask, censused.
- Direction fields: `CrossFieldAt(MeshSpace, int symmetry, Option<constraints>, Option<cones>, Point3d, Op) -> Fin<Vector3d>`; `StripeAt(space, crossField, double frequency, Point3d, key) -> Fin<double>` — the Knöppel owners remesh.md `QuadField` composes; symmetry ∈ {1,2,4,6}.
- Restructure (HOST tier by charter): `ApplyRemeshDetailed(RemeshKind, MeshSpace, Op) -> Fin<RemeshResult>` (native QuadRemesh/Reduce); `ParameterizeFlattenDetailed(MeshSpace, Op) -> Fin<FlattenResult>` (native LSCM) — coexists with robust decimate/flatten at different altitude, never a re-derivation. intent.md dispatches Flatten/Remesh through THESE (host tier), not through `Flatten.Apply`/`Remeshing.Apply` — legitimate tier routing that reads like split-brain; brief-writers must name both tiers.

### register.md — rigid registration/ICP
- `AlignKind` SmartEnum 6 (`Point`/`Plane`/`Symmetric`/`Robust`/`NormalWeightedPointToPlane`/`Generalized`) each carrying `SolveStep` delegate; entry `AlignDetailed(VectorCloud source, VectorCloud target, [AlignmentPolicy,] Op?) -> Fin<AlignmentReceipt>`; consumer rail `VectorIntent.Align`.
- `AlignmentReceipt(Transform, AlignKind, AlignmentStopKind, int Iterations, double FinalDelta, Option<AlignmentRobustReceipt>, CloudCorrespondenceSet, Option<SolveReceipt>, Option<AlignmentOptimizerReceipt>)`; `Project<TOut>` gates `Transform` on `Stop==Converged`.
- Laws: ONE outer fold, variants differ ONLY in `SolveStep`; ONE kNN substrate (`NeighborKernel.GraphOf`, k=1); GICP precision inverse = ONE spectral route; Armijo-ascending fused-metric objective; Welsch weights floored above underflow.
- Handroll bait: per-variant `AlignPoint`/`AlignPlane` solver family; page-local `RTree` reach; raw `DenseMatrix` normal-equation build.

### extract.md — extraction/projection rail
- `Extraction` [Union] 4 (`Probe`/`Contour`/`IsoSurface`/`Sampled` factories → `Fin<Extraction>`); egress `Project<TOut>(Context, Op)`; `ExtractionDomain` [Union] 3 (`Support`/`Mesh`/`Cloud`) with polymorphic `Of(object?, Context, Op?)` runtime-shape admission; `ContourPolicy` [Union] 4; `ExtractionProbe` [Union] 3; `SampledExtraction` [Union] 3 (`Glyph`/`Grid`/`StreamBundle`).
- `ExtractionReceipt(ExtractionRoute, int Attempted, Emitted, ExtractionTolerance, bool ParallelCallback, Option<IsoSurfaceReceipt>, Option<ScalarIsolineReceipt>, Option<SampleReceipt>, Option<int> ItemFailures)` — `Rejected`/`Complete` DERIVED from counts.
- Native-first sectioning; local marching-triangles PL kernel exists ONLY for the RhinoCommon per-vertex-scalar-contour hole. `AtomProjection.Rows` typed-row dispatch is the ONLY output gate (`typeof(TOut)` dead).

### flow.md — streamline/trace (`FlowKernel`)
- `FlowKernel.Trace<TOut>(VectorField, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator, Termination, Context, Op, Option<TracePolicy>)`; `ProjectTrace<TOut>(StreamlineTrace, Op)`; `Termination` [Union] 6 (`Steps`/`ArcLength`/`Magnitude`/`CrossSurface`/`RegionThreshold`/`LoopDetected`).
- `StreamlineTrace` receipt (17 fields incl. `StopKind`, dense-output error band, `Option<TraceEvent>`); three-tier event localization (endpoint touch → bracketed bisection via `DenseOutputSpan.PointAt` → none). `SpatialIntegration.Module` = THE one `IntegrationModule<Point3d,Vector3d>` in the corpus. Budget exhaust = typed terminal, never `Fin.Fail`.

### intent.md — kernel consumer rail (`VectorIntent`)
- ONE [Union] `VectorIntent`, 33 sealed cases, exactly ONE factory per case (41-factory spam dead); frozen egress `Project<TOut>(Context context, Op? key = null)` — bound by `Rasm.Rhino` Camera. Dispatch COMPOSES, carries ZERO domain math; construction internalizes admission (an existing `VectorIntent` is proof); generated `Switch` = exhaustiveness proof, no `_` arm.
- Routes: `Slerp`→`MotionInterpolation.Slerp.Rotate` (THE one slerp site); `Flatten`→`SegmentKernel.ParameterizeFlattenDetailed` (host tier); `Remesh`→`SegmentKernel.ApplyRemeshDetailed` (host tier); `Topology`→`MeshKernel.TopologyDetailed`; `DiscreteCalculus`→`DecAssembly.Build` (the Compute adjoint seam); `Align`→`AlignKind.AlignDetailed`; `Sample`→`SampleKind.Project`; `Features`→`SegmentKernel.DetectFeatureEdgesDetailed`.

---

## [03]-[PARAMETRIC] — 9 pages (fault bands: `ParametricFault(ParametricStage,…)` **2448** evaluation tier; `DevelopmentFault(DevelopmentStage,…)` **2449** fabrication tier)

### nurbs.md — THE vendored NURBS engine [mid-edit; kernel bodies signature-pinned]
- **`Nurbs.Of(NurbsWire wire, Op? key = null) -> Fin<NurbsForm>`** — ONE polymorphic admission over `NurbsWire` [Union] 4: `Curve(int Degree, Arr<double> Knots, Arr<Point3d> Points, Arr<double> Weights, KnotForm Origin)` · `Surface(int DegreeU, DegreeV, Arr<double> KnotsU, KnotsV, int CountU, Arr<Point3d> Grid, Arr<double> Weights, KnotForm Origin)` · `CurveThrough(Arr<Point3d> Samples, FitPolicy)` · `SurfaceThrough(int CountU, Arr<Point3d> Samples, FitPolicy)`.
- `NurbsForm` [Union] 2 — **`NurbsForm.Surface` IS the vendored NURBS surface** (homogeneous SoA columns WX/WY/WZ/W). Vendored from `GSharker/G-Shark@57bfbfb` (MIT, dormant); fixed-defect roster #234 (SKL deriv order), #337 (periodic), #373 (RMF closure), #391 (allocation). One-engine law: NO NURBS package survives beside the owned source; carriers are native `Point3d`/`Vector3d`/`Plane` (marshal dead).
- Surface members: `PointAt(u,v)`; `RationalDerivatives(u,v,order) -> Vector3d[][]` (`SKL[k][l] = ∂^{k+l}S/∂u^k∂v^l`); `NormalAt`; `FundamentalForms -> (E,F,G,L,M,N)`; `CurvatureAt -> (K1,K2,Dir1,Dir2,Gaussian,Mean)`; `IsoCurve`; `ClosestParameter(probe, policy, seed)`; `SplitAt`/`Refine`. Curve members: `PointAt`/`TangentAt`/`CurvatureAt`/`Length`/`ParameterAtLength`/`ParameterAtChordLength`/`ClosestParameter`/`PerpendicularFrames(ReadOnlySpan<double>)`/`SplitAt`/`SubCurve`/`Refine`/`ElevateDegree`/`DecomposeIntoBeziers`. Shared: `ToEncodeForm(Op?)` identity projection.
- Knots normalize to `[0,1]`; `KnotVector.Of` admits full-clamped AND Rhino-trimmed spellings — one content key per curve. Weights strictly positive at admission. Numerics COMPOSE MathNet + `matrix.md`.
- Handroll bait: evaluating via `Rhino.Geometry.NurbsSurface`/`NurbsCurve` instead of `Nurbs.Of`+`NurbsForm`; re-minting basis/De Boor/insertion; private point vocabulary + marshal layer; local Gauss-Legendre table or Newton loop.

### curve.md — host-neutral curve op algebra [mid-edit; kernel stubs signature-pinned]
- `Parametric.Apply(ParametricOp op, Op? key = null) -> Fin<ParametricResult>`; `Parametric.Fill(Arr<NurbsForm.Curve> loops, Axis plane, ArrangementPolicy?, Op?) -> Fin<ArrangementResult>` (delegates PlanarOverlay).
- `ParametricOp` [Union] 8: `Evaluate` · `Measure` · `Divide` · **`Stations(NurbsForm.Curve, StationPlan)`** · `Split` · `Reconstruct` · `Offset(curve, Plane Frame, double Distance, RefinePolicy)` · `Intersect2D`. `ParametricResult` [Union] 8; the Generation row: **`StationField(Arr<double> Arcs, Arr<double> Parameters, Arr<Point3d> Points, Arr<Plane> Frames, double FrameDefect)`** — SoA columns bound directly by Generation `PathRow`/`Placement`.
- ONE `Stationize` kernel serves Divide+Stations (spline table above `TableFloor=16`); `Stations` = `SubCurve` first, ONE `PerpendicularFrames` batch RMF sweep (Wang-2008), affine remap to parent domain. Offset = fit seed → bounded deviation-refinement → exact `SegmentSegment` self-intersection trim. `RefinePolicy` genuinely shared with surface.md `NormalOffset`.
- Handroll bait: local Brent chord inversion; N single-frame calls (loses RMF coherence); float-heuristic offset trim; row-object re-pack of the SoA `StationField`.

### surface.md — host-neutral surface op algebra; UV-PROVENANCE ORIGIN [untracked new; kernel stubs pinned]
- `Surfaces.Apply(SurfaceOp op, Op? key = null) -> Fin<SurfaceResult>`; `SurfaceOp` [Union] 6: `Tessellate(NurbsForm.Surface, TessellateRule, Context)` · `Isolines` · `Geodesics(SurfaceResult.UvTessellation, GeodesicPlan)` · `NormalOffset(surface, double Distance, FitPolicy Refit, RefinePolicy Refine)` · `CurvatureSample` · `Pullback(surface, Arr<Point3d> Probes, PullbackPolicy)`.
- **`SurfaceResult.UvTessellation(NurbsForm.Surface Source, MeshSpace Mesh, Arr<Point2d> Uv)`** — THE tier seam: tessellation kills no arena faces so vertex order survives `ToSpace`; `Uv[i]` parameterizes vertex `i` 1:1 — the invariant all pullbacks ride. The ONLY admissible surface ingress to develop/panelize/patternmap (provenance-by-type).
- Other results: `GeodesicField` (with `GeodesicGrade` heat/exact honesty marker), `Offsets(NurbsForm.Surface, RefineReceipt)` (REAL NURBS refit, Greville abscissae → `SurfaceThrough` G5), `CurvatureField` (K1/K2/Gaussian/Mean/Dirs/AreaElement SoA), `Pulled`.
- Handroll bait: a `MeshSpace` handed downstream WITHOUT its UV column + surface binding; `ClosestParameter` re-projection of tessellation-born points; tessellated offset mesh standing in for the real refit; second `k·d⊗d` shape-operator assembly.

### develop.md — exact-isometry developable strips [untracked new; kernel stubs pinned]
- `Development.Apply(DevelopOp op, Op? key = null) -> Fin<DevelopmentResult>`; `DevelopOp` [Union] 2: `Decompose(UvTessellation, DevelopPolicy)` · `Unroll(UvTessellation, DevelopPolicy)`. Results: `Strips(StripField)` · `Unrolled(ChartAtlas Atlas, StripField Field, DevelopmentReceipt Receipt)`.
- `DevelopmentReceipt(int Strips, Rulings, double MaxIsometry, MeanIsometry, MaxTorsal, int Components)`; acceptance `MaxIsometry ≤ IsometryBudget`. Rails PINNED `GeodesicGrade.Exact` (heat rail = named drift defect). **Isometry witness = `Σ(‖e‖₃D−‖e‖₂D)²` in 106-bit `ddouble`, narrowed only at readout** — the guarantee. Unroll = rigid two-circle placement, no relaxation. Emits Processing's `ChartAtlas` unchanged (Fabrication/AppUi bind the seam type).
- Handroll bait: a conformal/ARAP/spring solve here (flatten.md's tier); claiming isometric without the `ddouble` witness; mesh-normal approximation instead of `NormalAt`.

### panelize.md — cross-field-guided panelization [landed]
- `Panelization.Apply(PanelOp op, Op? key = null) -> Fin<PanelResult>`; `PanelOp` [Union] 2: `Map(UvTessellation, PanelFamily, PanelPolicy)` · `Planarize(PanelResult Prior, PanelPolicy)`. `PanelFamily` [Union] 2: `Lattice(int Symmetry, double TargetLength)` · `Seeded(SampleKind Seeds, int Symmetry)`.
- `PanelResult(PanelField, PanelReceipt)`; **`PanelField`** = per-panel PLACEMENT FRAMES SoA (`CornerOffsets/Corners/Vertices/Uv/Origin/XAxis/ZAxis/Planarity/PatchOf/AdjacencyOffsets/Adjacent/Component`); `PanelReceipt(…, double MaxPlanarity, MeanPlanarity, int SingularFaces, Rounds)`; acceptance `MaxPlanarity ≤ PlanarityBudget`.
- `Lattice` CONSUMES remesh.md `QuadProvenance` (never re-runs the field solve); lattice UV restored via ONE batch `SurfaceOp.Pullback`. `Seeded` = geodesic-Voronoi over cached `EnsureGeodesicDistances`. `Planarize` = bounded proximal rounds.
- Handroll bait: a `CrossFieldAt`/`StripeAt` extraction loop beside `Remeshing.Apply`; per-vertex `ClosestParameter` loop; Euclidean nearest-seed labels on a curved sheet; conformal pass inside `Planarize`.

### patternmap.md — pattern-to-surface instancing [landed]
- `Patterning.Apply(PatternOp op, Op? key = null) -> Fin<InstanceStream>`; `PatternOp` [Union] 2: `Orbit(PatternPlan)` · `Map(UvTessellation, PatternPlan, PatternPolicy)`.
- **`InstanceStream`** [Union] 2: `Planar(Site/Spin/Mirrored/Anchor/Seat)` · `Mapped(Arr<Point3d> Origin, Arr<Point2d> Uv, Arr<Vector3d> XAxis, ZAxis, Arr<double> Spin, Arr<bool> Mirrored, Arr<int> Anchor, Seat, Face, Arr<double> Radius, PatternReceipt Receipt)`; acceptance `MaxFrameDefect ≤ FrameBudget`.
- `WallpaperGroup` SmartEnum — **17 theorem-closed rows** (census can never grow); `PatternLattice` 5 rows with `Admits` delegates. Surface mapping = piecewise-linear INVERSION of one per-vertex `TangentLogMapAt` sweep (never per-site geodesic shoot); cut-locus honesty (`Flipped`/`Clipped` counted); frames parallel-transported via `VectorHeatAt`. Rhino block/instance materialization is host altitude at the wire.

### subdivide.md — subdivision surfaces [untracked new; kernel stubs pinned]
- `Subdivision.Apply(SubdivideOp op, Op? key = null) -> Fin<SubdivisionResult>`; `SubdivideOp` [Union] 2: `Refine(MeshSpace, SubdivisionScheme, int Levels, SubdividePolicy, Context)` · `Limit(MeshSpace, SubdivisionScheme, Arr<(int Face,double U,double V)> Samples, SubdividePolicy)`.
- `SubdivisionScheme` SmartEnum 2 (CatmullClark arity-4 / Loop arity-3) with stencil/limit/tangent/Eigenbasis delegate columns — scheme is DATA, fold is ONE. Results: `Refined(MeshSpace, Arr<Point3d> LimitPositions, Arr<Vector3d> LimitNormals, SubdivisionReceipt)` (limit columns ALWAYS ride) · `LimitField`. Each level EMITS the operator as a sparse matrix (3× SpMV); Stam eigenbasis memo per `(scheme,valence)`; crease/boundary = row masks; quads PRESERVED; region closure via red-green seal (`RegionClosures` audits). OpenSubdiv REJECTED (recorded gate).

### locate.md + projections.md — Rhino-runtime siblings (NOT the Generation fan)
- locate.md: routes ONLY through `AnalysisQuery.Location(Location)` under `Eff<Env, Seq<TOut>>` (Analysis runtime, not Fin-only). `Locator` [Union] 6 × `LocationValue` [Union] 8 case-owned matrix (31/48 supported). Delegates curve frame/tangent/curvature to `VectorIntent.Curve(…, CurveProjection.…)`.
- projections.md: four SmartEnum selectors gating into `AtomProjection.Raw` — `CurveProjection` (9 rows), `SurfaceProjection` (13 rows; **`ShapeOperator` = THE one second-fundamental-form owner**), `ConeProjection` (4), `MotionInterpolation` (2; **THE one quaternion-interpolation site** — `Rasm.Rhino` Camera binds `VectorIntent.Pose(…, MotionInterpolation.Slerp, …)`); `SurfaceSpace.Of(Surface, Context, Op?)` boundary capsule. This is the AppUi/analysis projection-display substrate.
- Runtime-split law: division/closest/arc-length live in BOTH runtimes by decision (host-neutral curve.md/surface.md vs Rhino locate.md/projections.md) — meet at the wire, never double-owners.

---

## [04]-[SOLVING] — 2 pages (committed clean; no Processing-provenance markers by prose law)

### solver.md — the ONE λ-ladder + constraint solver
- **`Lm.Minimize(ILmModel model, SolvePolicy policy, Op? key = null) -> Fin<LmResult>`** — the corpus's ONE damped Gauss-Newton iterate; `const double LambdaCeiling = 1e12`; `Lm.PackedIndex(int n,int i,int j)` mirrors `SymmetricMatrix` FlatIndex.
- **`ILmModel { int Dof; double[] Seed; ddouble Norm(ReadOnlySpan<double>); (double[] PackedNormal, double[] Gradient) Linearize(ReadOnlySpan<double>); }`** — `Norm` returns `ddouble` BY CONTRACT (106-bit). Implementors: `ConstraintModel` (here), `FitModel` (fit.md). A second `Iterate`/`Step` loop anywhere in the corpus is the named double-owner defect.
- `ConstraintSolver.Solve(ConstraintSystem, SolvePolicy, Op?) -> Fin<Solution>`; `Analyze -> DofAnalysis`; `StructuralAnalyze -> DofReport` (per-island König matching); `WitnessAnalyze` (numeric SVD rank); `ConstraintSystem.Build(Seq<(SketchEntityKind,double[])>, Seq<Constraint>, Op?)`.
- `Constraint` [Union] **15 cases** (`Distance`/`Angle`/`Coincident`/`Concentric`/`Parallel`/`Perpendicular`/`Tangent`/`PointOnLine`/`Midpoint`/`Axis`/`Equal`/`Symmetric`/`Ground`/`Radius`/`OnCircle`) with `Residual`/`Touches`/`WellFormed` generated folds. `SketchEntityKind` 3 rows (Point 2 / Line 4 / Circle 3 arity).
- Receipts: `LmResult(double[] Parameters, double Norm, int Iterations, double Lambda, SolveStatus)`; `Solution(double[] Parameters, ConstraintSolveReceipt)`; `ConstraintSolveReceipt(SolveStatus, DofAnalysis, double ResidualNorm, int Iterations, double TerminalLambda, int ResidualRows, Islands)`; `DofReport(verdict, StructuralRank, MatchingDeficiency, per-island rows)`.
- Faults: `OverConstrained(RedundantRows, norm)` 2412 (only when witness over-determined AND residual past tolerance); `SingularSystem(rank, dof)` 2413; 2400.
- Laws: accept carries λ DOWN; reject re-solves without re-linearizing (unbounded only by ceiling); zero-diagonal floor damps residual-untouched DOF at seed (under-constrained-manifold behavior); island decomposition via QuikGraph `ConnectedComponents`, solve per `dof_island²`; squared residuals for `Distance`/`Tangent`/`OnCircle` (C¹ at degenerate); GPL solvers rejected — authored from first principles.
- FLAG: no concrete downstream consumer of `Solution` named ("in-process seam" abstract); registration/parameterization `ILmModel` conformers anticipated, unrealized.

### fit.md — robust primitive fit (MLESAC + LM)
- **`Fit.Apply(FitOp op, Context tolerance, Op? key = null) -> Fin<FitReceipt>`**; `FitOp(Seq<FitKind> Kinds, Point3d[] Cloud, Option<Vector3d[]> Normals, FitPolicy Policy)` — single-vs-multi-kind is `Kinds` arity, not sibling requests.
- `FitKind` SmartEnum 6 (`plane`/`sphere`/`cylinder`/`cone`/`torus`/`line`) with `MinimalSamples`(3·4·6·7·8·2), `FreeParameters`, `NeedsNormals`(cone/torus), `Carrier` columns + `Minimal`/`Unpack` delegates. `FitPrimitive` [Union] 6 with six generated-Switch folds (`Distance`/`Gradient`/`Pack`/`Support`/`Agreement`).
- `FitReceipt(FitPrimitive Primitive, BitArray Inliers, double Residual, double Consensus, int Trials, int Iterations)`. Faults: `FitFault(achievedFraction, floor)` 2428; 2400.
- Laws: kind axis is DATA — one sampler/scorer/refine; TWO-GATE inlier (distance AND `Agreement ≥ NormalBand`); exact bounded-support prefilter (sphere/torus only); refine owns ZERO iterate code (entire ladder on `Lm.Minimize`); seeded determinism; ODR not algebraic least-squares.
- **Caller obligation (formerly Processing-inline)**: `Normals` must arrive PRE-COMPUTED via `Rasm.Spatial` `VectorCloudMetric.OrientedNormals` — absence gated at admission via `NeedsNormals`.
- Consumers: `Rasm.Bim` reality-capture (wall=plane, column=cylinder, dome=sphere/torus) — EXPLICITLY a future wire; full-cloud multi-primitive extraction = consumer fold (detect→mask→re-apply), never a second sampler.

---

## [05]-[DRAWING] — 2 pages

### pack.md — encoding/packing arena (transcription-complete)
- **`Encode.Apply(PackOp op, Op? key = null) -> Fin<EncodedGeometry>`**; `PackOp` [Union] 6: `PointCloud(VectorCloud.ClusterCase, PackPolicy)` · `MeshPatch(MeshSpace,…)` · `VoxelGrid(MeshSpace, PackGrid,…)` · `BrepPatch(MeshSpace,…)` · `Field(MeshSpace, ScalarField,…)` · `Toolpath(VectorCloud.PolylineCase,…)`.
- **`PackKind`** SmartEnum 6 (`point-cloud`/`mesh-patch`/`voxel-grid`/`brep-patch`/`field`/`toolpath`) column `Seq<EncodingChannel> Channels`. `EncodingChannel` SmartEnum 8 (`position` 3×F32 / `normal` 3×F32 / `color-rgba` 4×Unorm8 / `curvature`,`geodesic`,`intensity`,`occupancy`,`weight` 1×F16). `ChannelDtype` SmartEnum 3 (`Float32` w4 tol0 / `Float16` w2 tol9.77e-4 / `Unorm8` w1 tol1/255) with `Pack`/`Unpack` arms.
- **dtype-strided arena**: `EncodedStore.Reserve` sums `count·arity·width` per channel into ONE contiguous `byte[]`; each channel tiled at its `ByteOffset` behind `EncodingChannelDescriptor(Channel, Count, ByteOffset, Dtype)`; `Float16` stores real 2 bytes/scalar via `TensorPrimitives.ConvertToHalf` (widened-back 4-byte store dead).
- `EncodedGeometry(Seq<EncodingChannelDescriptor> Descriptors, ReadOnlyMemory<byte> Payload, int Count, RoundTripWitness Witness)` — `Channel(EncodingChannel) -> ReadOnlyMemory<byte>`; `View<T>(EncodingChannel) -> ReadOnlyTensorSpan<T>` `[Count × Arity]` descriptor-dispatched. `IsValid` = descriptor-tiling claim (contiguous offsets, Σ Bytes == Payload.Length).
- **Round-trip witness law**: digest keyed by reconciliation content digest ONLY (`EncodeForm.Of` → `Reconciliation.Apply(ReconcileOp.Encode)` → `GeometryHash`) — this owner mints NO second hash; per-channel scale-relative max error ≤ `Dtype.Tolerance` or `EncodingFault(channel, dtype, …)` **2444** — no silent lossy pack.
- Consumers: **Compute tensor lane** wraps `EncodedTensor` from `Payload`+`Descriptors`+`Witness` as `ReadOnlyTensorSpan<float|Half>` on the descriptor Dtype row — a residency view, never a re-pack; content hash IS the kernel `CANONICAL_BYTE_IDENTITY` digest. **AppHost `GeometryPacking` capsule**: 6 `EncodingKind` rows signature-locked one-to-one on `PackKind` keys (`Field`/`Toolpath` lock the rows this rebuild landed) — the lock is a wire on the consuming task, never a coupling edit here.
- **SEAM GAP FLAG**: `Rasm.Persistence` is NEVER named in pack.md — no stored-encoded-artifact seam row exists on the producer side. If Persistence stores encoded artifacts it would key on `GeometryHash` + `(Descriptors, Payload)`; that counterpart is unrealized.
- Handroll bait: raw `byte[]` / jagged `float[][]` packing (the re-pack tax); false-`Half` 4-byte store; per-kind encoder classes; channel switch cascade instead of the `Readers` FrozenDictionary; page-local content digest; `Lossless=true` without the witness.

### view.md — `DrawingProjection`, the analytic Appel HLR engine [Delta/Emit/seed kernels signature-pinned by declared design]
- `View.Apply(ViewOp op, Op? key = null) -> Fin<DrawingProjection>`; `ViewOp` [Union] 4: `Silhouette(MeshSpace, Camera, ViewPolicy)` · `HiddenLine` · `Section(MeshSpace, Plane Cut, Camera, ViewPolicy)` · `Outline`. `ViewKind` SmartEnum 4 with `EmitsHidden`/`ResolvesVisibility` columns (`NeedsBsp` dead).
- **`DrawingProjection(Seq<ProjectedSegment> Visible, Seq<ProjectedSegment> Hidden, EdgeHistogram Histogram)`** + `ToPolylines()` (successor-linked PER SET — never a `GroupBy(kind)` concat) + `ToSegments()` + `Fill(BooleanOp, ArrangementPolicy, Op?) -> Fin<ArrangementResult>` (routes PlanarOverlay). `ProjectedSegment(ScreenA, ScreenB, EdgeKind Edge, int Invisibility, Next, SourceA, SourceB)`. `EdgeKind` SmartEnum 4 (`Silhouette`/`Crease`/`Boundary`/`Intersection`) — the `ProjectionFault(EdgeKind,int)` **2436** payload, load-bearing corpus-wide.
- **Exact-analytic invariant**: NO sampling, NO ordering structure. Silhouette = exact `Orient3D` sign-change set (`FacesOppose`: both sides non-Zero and opposed — a grazing edge never flickers); crossings = exact `SegmentSegment(Axis.Z)`; Appel ±1 deltas = exact `Orient3D`/`Orient2D` sign pairs; seeds = ONE batched `SpatialQuery.Winding` cull + exact `SegmentTriangle` stab battery; round-ONCE at `Emit`. Dead forms enumerated: `SampleStep` marcher, per-sample BVH ray + `OcclusionBias`, Newell-Newell-Sancha BSP, `PaintBackToFront`.
- Consumers: **Fabrication Posting** reads visible/hidden runs + `EdgeKind` (retired Posting BSP HLR dies for this seam; `HiddenLineResult` a thin projection); **AppUi drafting** reads the same carrier; fill consumers route `Fill`. `Analysis/select` host capture (`Silhouette.Compute`) stands BESIDE under the capture law — altitude split.
- Handroll bait: z-buffer/painter raster HLR; recursive BSP minting rounded vertices; uniform-step marcher; epsilon dot-test silhouette; view-local crossing kernel (a 4th copy); host `Make2D` round-trip for Section; local ear-clipper for fill.

---

## [06]-[DOWNSTREAM_CONSUMER_MAP]

| Consumer | Contract consumed | Exact ingress/egress shape |
|---|---|---|
| Fabrication FAB:22 | `Offsetting.Apply` + `Skeletonize.Apply` | `SkeletonGraph`/`ClearanceNode` family; `CurveSkeleton.Graph` + `Clearance(probe)`; [V5] weighted rows ride `OffsetOp.Weighted` + `EdgeSpeed` |
| Fabrication FAB:48/FAB:23 | `Slicing.Apply` | five-channel `SliceStack`; toolpath ordering outermost-first via `RootsOf`/`Depth` |
| Fabrication Posting | `View.Apply` | `DrawingProjection` visible/hidden runs + `EdgeKind`; retired BSP HLR dies |
| Fabrication nesting/unroll | `Flatten.Apply`; `Development.Apply`; `Panelization.Apply` | `ChartAtlas` (`DistortionReceipt.MaxArea`/`MaxConformal` strain bound, `UvIsland` layout); `PanelField` adjacency columns; NFP robust tier reads `PlanarOverlay` |
| Fabrication stock/clearing | `Tessellation.LowerHull` [V11] | predicate-gated exact envelope `MeshSpace` |
| Compute [V12]a circulation | `Slicing.Apply` | story contours through `LayerPlan.AtElevations` + `LayerAt` |
| Compute tensor lane | `Encode.Apply` | `EncodedTensor` residency view over `Payload`+`Descriptors`+`Witness`; `PackKind`/dtype row lock; hash = `CANONICAL_BYTE_IDENTITY` digest, no second key |
| Compute LOD/residency | `Simplify.Apply` | tile pyramid (`Mesh`+`Hausdorff`), multigrid coarse seed, meshlet `Faces`, continuous-LOD `Splits` replay |
| Compute [V7] boundary conditioning | `Remeshing.Apply(Isotropic)` | frozen `MeshSpace` wire, never a Compute-side remesher |
| Compute adjoint (GeometryTape) | `MeshAdjointSnapshot.Of` | public `DiscreteCalculus` bound BY NAME (`Numerics/spectral` spellings); never internal `IntrinsicMesh` |
| AppHost GeometryPacking | `Encode.Apply` | 6 `EncodingKind` rows signature-locked 1:1 on `PackKind` keys (incl. new `Field`/`Toolpath`) |
| AppUi drafting/display | `View.Apply`; projections.md/locate.md | `DrawingProjection`; `AnalysisQuery.Location` + selector rows (`SurfaceProjection`, `MotionInterpolation.Slerp` via `VectorIntent.Pose`) |
| AppUi texture lane | `Flatten.Apply` | `ChartAtlas.ToMesh`/`ToTextureMesh` + `UvIsland` |
| Generation fan row 1 | curve.md | in: `ParametricOp.Stations(NurbsForm.Curve, StationPlan)` — out: `ParametricResult.StationField` SoA → `PathRow`/`Placement` |
| Generation fan row 2 | surface.md | in: `SurfaceOp.Tessellate(NurbsForm.Surface, TessellateRule, Context)` — out: `SurfaceResult.UvTessellation` |
| Generation fan row 3 | develop.md | in: `DevelopOp.Unroll(UvTessellation, DevelopPolicy)` — out: `DevelopmentResult.Unrolled(ChartAtlas, StripField, DevelopmentReceipt)`; gate `MaxIsometry ≤ IsometryBudget` |
| Generation fan row 4 | panelize.md | in: `PanelOp.Map(UvTessellation, PanelFamily, PanelPolicy)` — out: `PanelResult(PanelField, PanelReceipt)`; gate `MaxPlanarity ≤ PlanarityBudget` |
| Generation fan row 5 | patternmap.md | in: `PatternOp.Map(UvTessellation, PatternPlan, PatternPolicy)` — out: `InstanceStream.Mapped`; gate `MaxFrameDefect ≤ FrameBudget` |
| Generation fan row 6 | subdivide.md | in: `SubdivideOp.Refine(MeshSpace, SubdivisionScheme, Levels, SubdividePolicy, Context)` — out: `SubdivisionResult.Refined` (`RegionClosures` audits T-seal) |
| Generation fan row 7 | nurbs.md | in: `Nurbs.Of(NurbsWire)` (4 wire shapes; SpineRef key→wire→form round-trip) — out: `NurbsForm`; identity via `ToEncodeForm` |
| Persistence | reconciliation chain | content-addresses frozen carriers (`MeshSpace`, `ChartAtlas`, `DecimationResult`, `FitReceipt`) through `Spatial/reconciliation` `Encode`/`GeometryHash`; **pack.md names NO Persistence seam — encoded-artifact storage counterpart unrealized on the producer side** |
| Rasm.Bim | `Arrangement.Apply(CellComplex)`; `Fit.Apply` | un-welded `CellSet` solid classifier; `FitReceipt.Primitive`+`Inliers` → `ReconstructionPrimitive`/`ElementPredicate` (explicit FUTURE wire) |

Rows 3–5 of the Generation fan share `SurfaceResult.UvTessellation` as their ONLY admissible surface ingress (provenance-by-type). locate.md/projections.md are the Rhino-runtime analysis siblings, NOT fan rows.

---

## [07]-[HANDROLL_BAIT_REGISTER] (per contract, the naive re-derivation briefs must forbid)

| Contract | Bait |
|---|---|
| `MeshEdit.Of`/ARENA_LAW | third mesh carrier (immutable `with`-copy edit record); per-consumer soup `DuplicateNative`; raw `Parallel.For`; fixed-`2n` store; local welder; hashing a mid-build arena |
| `Intersection.Apply`/`CrossLattice` | sibling intersector family; rounded `Point3d` at birth beside exact keys; proximity weld of endpoints; unoriented fragment concat; local all-pairs broad phase; a 4th crossing kernel anywhere |
| `Tessellation.Build` | ear-clipping/epsilon in-circle triangulator; Triangle.NET/TetGen admission; `IsImplicit` rounded carrier; page-local `Mesh`+`RebuildNormals` path instead of `ToMesh` |
| `Arrangement.Apply` | three enumerated keep bodies vs the one `Region` column; per-patch GWN loops; bit-equality point interning; silent native/managed degrade |
| `Offsetting.Apply`/clearance family | `StraightSkeleton`/`MedialAxis`/`MinkowskiSum` siblings; radius-less medial node or per-consumer clearance record; local Voronoi for medial; `Mesh.CreateFromClosedPolyline` round-trip |
| `Slicing.Apply`/`SliceStack` | slice-local plane sweep; contour re-orientation; float point-in-polygon nesting; O(C²) parent scan; nested `Seq<Seq<Chain>>` beside the channels |
| `Skeletonize.Apply` | skeleton-local node/arc/graph shapes; second probe semantics; stored QuikGraph fields; reaching into geodesics' MCF arm |
| `Heal.Repair`/`Simplify.Apply` | per-defect healer classes; epsilon sliver signs; local crossing kernels; vacuous flip test; O(F²) rescans; second-tolerance params |
| `Remeshing.Apply`/`QuadProvenance` | float-dihedral flip; cotangent relax; unprojected relax; native QuadRemesh here (tier violation); panelize re-running the field solve |
| `Flatten.Apply`/`ChartAtlas` | raw CSparse/SVD reach; penalty pinning; float flip band; local dihedral seam detector; dense `(2n)²` conformal |
| `GeodesicKernel` | third cotangent path; second closest-face/tangent-frame owner; `HeatTime` knob; zero-for-unreachable; per-site geodesic shoots (patternmap) |
| `Nurbs.Of`/`NurbsForm` | evaluating via `Rhino.Geometry.NurbsCurve/Surface`; re-minted De Boor/basis/insertion kernels; private point vocabulary + marshal; local Gauss-Legendre/Newton |
| `Stations`/`StationField` | Brent chord inversion loops; N single-frame calls (RMF incoherence); AoS re-pack of the SoA columns |
| `UvTessellation` carrier | unbound `MeshSpace` handed downstream; `ClosestParameter` re-projection of tessellation-born points; parallel projector beside seeded `ClosestParameter` |
| `Lm.Minimize`/`ILmModel` | private LM `Iterate`/λ loops in fit/registration/parameterization pages; own triangular indexing; `double`-narrowed objectives; FD Jacobians; GPL solver admission |
| `Fit.Apply` | fitter-class family per kind; inline covariance/eigen; count-threshold RANSAC; algebraic LS; distance-only inlier gate; a `Detect` sibling |
| `Encode.Apply`/`PackKind` | raw `byte[]`/jagged packing; false-`Half` 4-byte store; per-kind encoder classes; page-local content digest; silent lossy pack |
| `View.Apply`/`DrawingProjection` | z-buffer/painter/BSP HLR; sampled marcher + occlusion bias; epsilon silhouette; host `Make2D` round-trip; visible+hidden merged concat |
| receipts vocabulary | generic `IReceipt`/`HealLedger` erasure; local manifold/genus recompute; standalone `Converged` bools |
| `VectorIntent` | arms that compute instead of compose; sibling factories/bool knobs; `_` catch-all switch arms |

---

## [08]-[SEAM_FLAGS] (missing counterparts + mid-edit state)

1. **Persistence × pack.md**: no producer-side seam row for stored encoded artifacts (only the reconciliation `GeometryHash` chain exists). Root docs claiming "Persistence stores the encoded-artifact shapes" overstate the landed page.
2. **solver.md `Solution`**: no named downstream consumer ("in-process seam" abstract); anticipated registration/parameterization `ILmModel` conformers unrealized. `DofReport` reader is "a sketch UI" — unnamed.
3. **fit.md → Rasm.Bim**: `ReconstructionPrimitive`/`ElementPredicate` wire is EXPLICITLY future, not landed.
4. **Namespace attribution split (corpus-wide, git-queued fix)**: Meshing prose attributes `Tessellation`/`Constraint`/`Predicate` to `Rasm.Numerics` while the fences declare `namespace Rasm.Meshing` (Predicate/Implicit genuinely from `Rasm.Numerics`). Treat `Rasm.Numerics` delaunay attributions as pending reconciliation.
5. **Recorded growth, not landed**: multi-implicit in-circum + 3D multi-implicit orientation + exact circumcenter side-of (delaunay/predicates); plane-slab `SpatialQuery` case (intersect→slice consumer); barrier stop rows (geodesics×segment).
6. **intent.md tier routing that reads as split-brain**: `Flatten`/`Remesh` cases dispatch to `SegmentKernel.ParameterizeFlattenDetailed`/`ApplyRemeshDetailed` (HOST tier) while `Flatten.Apply`/`Remeshing.Apply` are the robust kernel owners — legitimate by the three-tier charter, but any brief touching these must name both tiers explicitly.
7. **Mid-edit census**: skeleton.md, slice.md, remesh.md, decimate.md, curve.md, nurbs.md are git-modified (concurrent redteam) — all read structurally complete and internally consistent; treat as provisional until the redteam lands. develop.md, subdivide.md, surface.md are untracked-new. Signature-pinned-stub pages BY DECLARED DESIGN (not truncation): dec.md private bodies, view.md `Delta`/`Emit`/seed kernels, nurbs.md `NurbsKernel`, curve/surface/develop/subdivide kernel internals.
8. **Least-anchored spots to re-verify post-redteam**: slice.md `SectionFault.openChains` payload overloaded across three defect classes; skeleton.md `SkeletonPolicy` lacks a `CollapseAreaRatio`×`StallBand` cross-claim.

---

## [09]-[FAULT_BAND_LEDGER] (band-2400 `GeometryFault`, codes landed in these folders)

2400 `DegenerateInput` · 2408 `UnrepairableMesh(HealStage,…)` · 2412 `OverConstrained` · 2413 `SingularSystem` · 2416 `DegenerateOffset` · 2417 `SkeletonStalled` · 2418 `CollapseStalled` · 2420 `DegenerateArrangement` · 2421 `ConstraintUnrecoverable` · 2422 `DegenerateTessellation` · 2423 `NativeAssetMissing` · 2424 `IntersectionFault(PrimitiveKind,PrimitiveKind)` · 2425 `SectionFault` · 2428 `FitFault` · 2432 `ParameterizationFault(ChartId,…)` · 2436 `ProjectionFault(EdgeKind,…)` · 2440 `DecimationFault` · 2441 `RemeshStalled` · 2444 `EncodingFault(EncodingChannel,ChannelDtype,…)` · 2448 `ParametricFault(ParametricStage,…)` · 2449 `DevelopmentFault(DevelopmentStage,…)`.
