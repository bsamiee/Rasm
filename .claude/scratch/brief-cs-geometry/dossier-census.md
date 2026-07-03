# [DOSSIER_CENSUS] — libs/csharp/Rasm/Geometry/.planning

Read-only census of the whole Geometry robust-core planning tree (18 pages, 6 folders, ~6,900 LOC of fenced C#). All 18 pages deep-read in full; both `.api` tiers enumerated; every cited external member (kd-tree, GShark, ddouble/Fraction/EFloat/ERational, MathNet) verified against `libs/csharp/Rasm/.api/`; the owning `libs/csharp/Rasm/ARCHITECTURE.md` seam ledger diffed both directions against the real page graph and the counterpart Fabrication/Compute/Persistence/Element ledgers.

Corpus quality is HIGH in isolation — the exact-predicate floor and most owners are genuinely world-class (adaptive 4-tier predicates with 4 independent oracles, Cherchi-Attene managed exact boolean, DEC-based LSCM/ARAP/BFF, Garland-Heckbert QEM with exact-Orient3D gate). The defects are STRUCTURAL and CROSS-CUTTING: a pervasive folder↔namespace↔fault-cluster tri-way incoherence, an undeclared 6th sub-domain that breaks a stated invariant, a declared-but-unrealized crossing-owner collapse (4 inline copies), and two concerns owned twice across the kernel↔Fabrication strata. The isolation-grade quality masks systemic architecture rot.

---

## [PART_1] — PER-PAGE VERDICTS

Verdict = 1-10 quality against the docs/stacks/csharp bar AND the two gated consumers (Generation, Fabrication). All namespaces below are the fence-declared `namespace` line; "≠folder" flags the NAMESPACE_LAW violation (see VC1).

### Numerics/predicates.md — VERDICT 9/10 (namespace `Rasm.Geometry.Numerics`, matches folder)
- The strongest page. Closed `Predicate` family (`Orient2D`/`Orient3D`/`InCircle`/`InSphere` + `OrientLPI`/`OrientTPI`/`InCircleLPI`/`InSphereTPI`) over a 4-tier `PrecisionTier` ladder (double filter → ddouble 106-bit refine → Expansion sign-exact → Fraction rational oracle), inline allocation-free escalation (predicates.md:63-65). FOUR independent exact oracles for the law-matrix differential — `Expansion`, `TYoshimura.DoubleDouble`, `ExtendedNumerics.BigRational` `Fraction`, `PeterO.Numbers` `EFloat`/`ERational` (predicates.md:360-366, :371) — every member VERIFIED real in api-doubledouble/api-bigrational/api-peteronumbers.
- Defects: none material. Minor — the implicit in-circum members escalate straight to `RationalOracle.InCircum` skipping the ddouble middle tier (predicates.md:247-248); defensible (lambda² homogeneous degree). `NumericsPolicy` sanctions the interior-double scope cleanly (:378).
- Charter as-is: correct. This is the reference floor; do not touch beyond namespace-law reconciliation.

### Numerics/faults.md — VERDICT 8/10 (namespace `Rasm.Geometry`, root — defensible for the shared family)
- Clean consolidated `GeometryFault` `[Union]` band 2400, 17 cases, `ToError()` lowering, `ComparerAccessors.StringOrdinal` (faults.md:33-101).
- Defects: (a) the 12 sibling sub-band CLUSTER NAMES (`healing`, `topology`, `constraints`, `offsetting`, `arrangement`, `intersection`, `fitting`, `parameterization`, `projection`, `simplification`, `encoding`, `spatial` — faults.md:14) are the TRUE concept vocabulary, and they DIVERGE from the 5 folder names (`Processing`, `Spatial`, `Meshing`) — this is the seed of the namespace schism (VC1). (b) NO `parametric` cluster exists — curve.md borrows flatten's `ParameterizationFault` 2432 (curve.md:191), so the orphan Parametric sub-domain has no fault home (VC2). (c) The band is nearly full: 2400-2447 allocated, only 2448-2449 headroom, and faults.md:19 explicitly warns a 13th cluster forces "an outright federation re-plan" — yet Parametric is a live undeclared 6th sub-domain needing a cluster.
- Charter as-is: the union is right; the sub-band cluster names must become the canonical concept/namespace names, and a parametric sub-band + a band-widening plan are required.

### Spatial/index.md — VERDICT 9/10 (namespace `Rasm.Geometry.Spatial`, matches folder)
- One polymorphic `SpatialIndex` `[Union]` (Bvh/LinearOctree/PointCloud) over one SoA `NodeStore`, `Build`/`Query`/`Refit`/`ToAcceleration`, `SpatialQuery` (Nearest/Range/Ray/Overlap/Winding), pinned `CLASH_GOLDEN` 160-byte fixture (index.md:16-20, 572-578). kd-tree usage VERIFIED real: `KDTree.Create`/`NearestNeighbors`/`RadialSearch`/`DistanceMetrics.EuclideanDistance` all exist in api-kdtree.md (namespace `SuperClusterKDTree`, SQUARED-L2 metric correctly handled at index.md:377-380).
- Defects: (a) index.md GREW `SpatialKind.PointCloud` kd-tree (index.md:44,118) and the `Winding` GWN query (index.md:101,383-412) BEYOND the owning codemap charter, which says only "SAH-BVH/Morton-octree SpatialIndex" (ARCHITECTURE.md:16) and "Spatial groups the BVH/octree acceleration index" (:39) — codemap is STALE vs the richer page. (b) The `Winding` GWN (Barnes-Hut solid-angle tree-code, index.md:383-444) DUPLICATES the concern Vectors already owns ("triangle solid-angle winding SDF", Vectors _ARCHITECTURE:99) — two winding-number owners in the kernel (arrangement.md:444 composes THIS one; Vectors owns the exact per-triangle). Reconcile: kernel Spatial owns the BVH-accelerated GWN, Vectors' should compose it or be demoted.
- Charter as-is: the richer page IS the truth; the codemap must be updated to record kd-tree + GWN, and the GWN/Vectors-winding double-owner reconciled.

### Spatial/naming.md — VERDICT 8.5/10 (namespace ambiguous: folder Spatial, but consumers import `Rasm.Geometry.Topology`)
- Strong `TopoName` `[ValueObject<UInt128>]` lineage over one `EntityKind` discriminant, `NameTable` registry, `Track` re-anchor fold (Survived/Migrated/Born), position-free `TopoSignature` (naming.md:13-20, 108-144).
- Defects: (a) namespace incoherence — the folder is `Spatial/` and index.md declares `Rasm.Geometry.Spatial`, but solver.md:13 and pack.md:33 import the naming/reconciliation types as `Rasm.Geometry.Topology`; the fault cluster is `topology` (faults.md:14). So a single folder splits across two namespaces. (b) An OPEN injectivity residual is honestly recorded (naming.md:161): the Survive×Migrate cross-case is unproven, held for the property harness — a genuinely-deferred verification, not a fence gap.
- Charter as-is: right owner; the Spatial-vs-Topology namespace split must resolve (VC1).

### Spatial/reconciliation.md — VERDICT 9/10 (namespace: folder Spatial, imported as `Rasm.Geometry.Topology`)
- Owns the FROZEN 52-byte canonical-adjacency byte layout + `XxHash128` digest (reconciliation.md:147), the `Reconcile` TopoName→GeometryHash fence, and the `ONE_WIRE_FIXTURE_CORPUS` index — the single cross-language golden-fixture parent every parity harness reads (reconciliation.md:114-134). Excellent parity discipline; CANONICAL_BYTE_IDENTITY + CLASH_GOLDEN are REAL/pinned, the other 5 fixtures honestly marked DESIGN-PIN on named cross-folder producers.
- Defects: SEAM-ANCHOR DRIFT — the reconciliation→Persistence seam is recorded THREE inconsistent ways: reconciliation.md:2,16 says the counterpart is `Persistence/version-control#STRUCTURAL_DIFF` (Version/merge); the owning `ARCHITECTURE.md:45` says `Rasm.Persistence/Query`; and Persistence's own `ARCHITECTURE.md:55` says `Query/topology ← Rasm/Geometry/Spatial`. Three ledgers, three anchors, one seam (VC8).
- Charter as-is: correct owner; the seam anchor must be reconciled across all three ledgers.

### Meshing/delaunay.md — VERDICT 7/10 (namespace `Rasm.Geometry.Tessellation` ≠ folder Meshing)
- Design is strong (Bowyer-Watson CDT/CDTet over one SimplexStore, exact InCircle/InSphere cavity, LPI/TPI Steiner recovery — delaunay.md:16). But it is the LEAST transcription-complete page: the genuinely-hard bodies — `Seed`, `Retriangulate`, `RecoverOne`, `StripSuper`, `LastLive`, `TouchesSuper`, `AddSimplexFaces` — are SIGNATURE-ONLY stubs (delaunay.md:192-198), with the algorithm deferred to [RESEARCH] prose (:251-252). This is the substrate that arrangement, offset, AND flatten all ride, so the unfenced core is load-bearing.
- Defects: (a) prose-vs-fence split on the cavity re-triangulation + constraint-recovery flips (the hardest CDT logic); (b) namespace ≠ folder.
- Charter as-is: right owner, but the deferred bodies must be fenced to the density bar — this is the highest-priority transcription completion in the tree.

### Meshing/arrangement.md — VERDICT 9/10 (namespace `Rasm.Geometry.Arrangement` ≠ folder Meshing)
- World-class managed exact Cherchi-Attene mesh/polygon arrangement (MeshBoolean/PlanarOverlay/CellComplex), composing exact Orient3D crossings, GWN classification, in-plane CDT re-triangulation, WeldDuplicates (arrangement.md:16). Retires the native CSG gate for common cases (ScaleCeiling int.MaxValue, :60-65).
- Defects: (a) DUPLICATE crossing — re-implements the exact triangle-triangle straddle INLINE (`EdgeCrossings`/`SegmentOf`/`PlaneCrossPoint`/`InTriangle`, arrangement.md:242-262) instead of composing intersect.md's `Intersection.Apply(TriangleTriangle)`; intersect.md:388 names arrangement as a collapse target but the fence still inlines it (VC3). (b) DUPLICATE `BooleanReceipt` — arrangement.md:115 declares `BooleanReceipt(int Classified,int Kept,int Welded,bool AssetGated)` while receipts.md:44 declares a DIFFERENT `RebuildReceipt.BooleanReceipt(BooleanOp,bool,ManifoldStatus,ManifoldStatus,Set<int>)` — same name, different shape, different namespaces (VC5). (c) page CYCLE with repair (composes `Rasm.Geometry.Healing` BooleanOp/WeldDuplicates/MeshEdit, arrangement.md:18; repair.md:288 composes back). (d) namespace ≠ folder.
- Charter as-is: the boolean/overlay/cell-complex owner is right; collapse the inline crossing onto intersect, unify BooleanReceipt, break the repair cycle.

### Meshing/intersect.md — VERDICT 9/10 (namespace `Rasm.Geometry.Intersection` ≠ folder Meshing)
- The exact-crossing lattice (SegmentSegment/SegmentTriangle/TriangleTriangle/RayMesh/MeshMesh/PlaneMesh), Guigue-Devillers exact tri-tri, ordered crossing-chain with exact Expansion+Fraction OrderKey (intersect.md:16, 181-216). PlaneMesh IS the kernel section primitive the Fabrication slicer + Drawing/view section consume.
- Defects: the collapse it DECLARES is UNREALIZED — intersect.md:388 [EXACT_CROSSING_COLLAPSE] names repair/offset/arrangement as consumers that must compose `Intersection.Apply`/`Crossing`, but all three still inline their own straddle (VC3). The page is correct; the consumers haven't aligned inward. namespace ≠ folder.
- Charter as-is: correct sole crossing owner; force the three consumers to compose it.

### Meshing/offset.md — VERDICT 8.5/10 (namespace `Rasm.Geometry.Offsetting` ≠ folder Meshing)
- Aichholzer-Aurenhammer wavefront skeleton + weighted/mitered skeleton + medial + Minkowski + offset over one WavefrontStore (offset.md:16). Good toolpath-grade breadth.
- Defects: (a) PROSE-VS-FENCE — `MedialFrom` claims "reconciles against the constrained-Delaunay Voronoi dual so the medial axis is the exact bisector locus" (offset.md:478) but the fence BUILDS the tessellation then DISCARDS it (`Tessellation.Build(...).Map(_ => new MedialAxis(...))`, offset.md:325-326) — the medial is actually just the trimmed straight skeleton, which the prose itself admits diverges from the true medial at reflex vertices. (b) THIRD inline copy of the exact straddle (`SegmentsCross`, offset.md:440-444) — the offset instance intersect.md:388 names (VC3). (c) NO corner-strategy vocabulary — the `Offset` case is `(Ring, Distance, Policy)` (offset.md:124), straight-skeleton offset is MITER-only; the CAM `JoinType` (round/miter/square/bevel) + `EndType` vocabulary Fabrication toolpaths need is ABSENT (VC10). (d) DUPLICATES Fabrication/Toolpath/skeleton (Fabrication ARCHITECTURE.md:24 "Straight-skeleton/medial-axis trochoidal primitive") — offset.md:480 expects Fabrication to consume `Offsetting.Apply`, but Fabrication declares its own (VC4). (e) namespace ≠ folder.
- Charter as-is: the wavefront owner is right and toolpath-critical; realize the Voronoi-dual medial, add JoinType/EndType corner strategies, and make it the sole skeleton owner Fabrication consumes.

### Processing/repair.md — VERDICT 8/10 (namespace `Rasm.Geometry.Healing` ≠ folder Processing)
- Closed HealOp algebra (6 author-kernels + Boolean-via-arrangement), `Heal.Repair` session, exact-predicate weld/collapse/orient (repair.md:16, 378-402). Boolean now composes `Arrangement.Apply` for common cases (repair.md:288-292).
- Defects: (a) `SelfIntersectResolve` is NAIVE — appends ONE crossing point and fans the offending face into a fixed 3-fan `(gf.A,gf.B,p),(gf.B,gf.C,p),(gf.C,gf.A,p)` patching only face `g` not `f` (repair.md:244-250); delaunay.md:253 confirms this is a "local fan today" pending the CDT substrate (naivety-APPROACH). (b) FOURTH inline crossing copy (`TriangleCrossPoint`/`EdgesCrossTriangle`/`PlaneCrossPoint`/`InTriangle`, repair.md:346-372) (VC3). (c) namespace `Rasm.Geometry.Healing` vs folder `Processing` vs codemap `Repair.cs` vs Fabrication seam `Rasm/Geometry/Healing` — 4-way naming fracture (VC1). (d) page CYCLE with arrangement (VC5). (e) owns `MeshEdit` (repair.md:76-102), the de-facto shared mesh substrate 5+ pages consume (VC6).
- Charter as-is: the heal algebra is right; rename to the concept/folder-aligned owner, extract MeshEdit, break the arrangement cycle, and make SelfIntersectResolve a real CDT re-mesh.

### Processing/receipts.md — VERDICT 8/10 (namespace `Rasm.Geometry.Healing` ≠ folder Processing)
- Clean typed `RebuildReceipt` `[Union]` (7 per-op cases), `ManifoldStatus` projection, `HealSession`/`RebuildLog` (receipts.md:34-106).
- Defects: (a) DUPLICATE `BooleanReceipt` type-name collision with arrangement.md (receipts.md:44 vs arrangement.md:115) (VC5). (b) `ManifoldStatus` depends on `VectorIntent.Topology(space).Project<(int,int,int)>` and honestly records that `IsManifold`/`NonManifoldEdges` are NOT projectable and `MeshKernel.TopologyDetailed` is internal to Vectors (receipts.md:441) — an unwired Vectors-seam limitation (VC8).
- Charter as-is: right; unify BooleanReceipt naming, and the Vectors topology-projection seam needs a public row.

### Processing/solver.md — VERDICT 9/10 (namespace `Rasm.Geometry.Constraints` ≠ folder Processing)
- Excellent closed `Constraint` `[Union]` (9 relations) with co-located analytic Residual+Partials, structural + witness DOF analysis, author-kernel LM composing MathNet Cholesky directly at the kernel tier (solver.md:16, 283-360). RESOLVES the Compute-LM question: the kernel solver composes MathNet independently (solver.md:18, 26-27), NOT Compute.
- Defects: (a) the Compute ARCHITECTURE.md:120 seam CLAIMS "Geometry/Processing/solver consumes the Compute LM/constraint Solver rail" — a DOWNWARD kernel→app-platform edge that would VIOLATE the acyclic strata; solver.md proves it spurious (composes MathNet, references no Compute) (VC8). (b) references a PHANTOM `Rasm.Geometry.Topology` namespace (solver.md:13 "the sibling `Rasm.Geometry.Topology` namespace") — no such namespace; the topology naming is folder Spatial (VC1). (c) LM λ-ladder (`Step`/`Iterate`, solver.md:295-331) is PRIVATE and Constraint-coupled (`Linearize` folds `Constraint.Residual`), so fit.md cannot reuse it → in-kernel LM duplication (VC7). (d) namespace ≠ folder.
- Charter as-is: right; extract a generic damped-Gauss-Newton owner, delete the spurious Compute seam, fix the phantom namespace reference.

### Processing/fit.md — VERDICT 9/10 (namespace `Rasm.Geometry.Fitting` ≠ folder Processing)
- MLESAC/PROSAC efficient-RANSAC + geometric-orthogonal-distance LM refine with analytic per-primitive Jacobian (fit.md:16, `FitPrimitive.Gradient` :121-200), deterministic seeded sampler, 106-bit ddouble objective. Reuses Spatial BVH + the solver-grade LM.
- Defects: (a) `Refine` (fit.md:557+) RE-IMPLEMENTS the LM ladder solver.md owns privately (RefineState/Norm, its own damped step) — the prose claims "the SAME LM the sketch solver composes" (fit.md:16) but mechanically the private Constraint-coupled `Step`/`Iterate` cannot be shared → duplicate ladder (VC7). (b) namespace ≠ folder.
- Charter as-is: right; the refine must compose a shared generic damped-GN owner, not re-author the ladder.

### Processing/flatten.md — VERDICT 9/10 (namespace `Rasm.Geometry.Parameterization` ≠ folder Processing)
- World-class Harmonic/LSCM/ARAP/BFF over the Vectors DEC substrate (`MeshAdjointSnapshot.Of` public handle), one cached CholeskySparse factor reused across every ARAP global step (flatten.md:16). Exactly the panelization/unroll capability the Generation gate + Fabrication nesting need. The `MeshAdjointSnapshot` seam is a SETTLED contract (Vectors _ARCHITECTURE:151-158).
- Defects: (a) ALGORITHMIC NAIVETY — ARAP `AccumulateRotated` calls `IndexOf(i,j,k)` (a LINEAR O(F) face scan, flatten.md:482-485) per face inside `RotatedGradient`'s per-face loop (flatten.md:393-401,474), where the face index `f` is already in scope → O(F²) per ARAP iteration (VC9). (b) receipt bug — `MeshDec.FactorNonZeros => Calculus.D1.NonZeros` (flatten.md:330) reports the CURL operator nnz, but the cached factor is the Cholesky of the D0-based stiffness `L=D0ᵀ·diag(Star1)·D0` — wrong nnz reported. (c) namespace ≠ folder.
- Charter as-is: right and Generation-critical; fix the O(F²) ARAP scan (pass `f`), correct FactorNonZeros.

### Processing/decimate.md — VERDICT 8.5/10 (namespace `Rasm.Geometry.Simplification` ≠ folder Processing)
- Garland-Heckbert QEM (QuadricCollapse/ProgressiveMesh/VoxelRemesh/FeaturePreserve), exact-Orient3D collapse gate + manifold link-condition, 106-bit ddouble quadric, SDF voxel remesh, one-sided Hausdorff, reversible vsplit (decimate.md:16). LOD for the Compute tile-pyramid.
- Defects: (a) ALGORITHMIC NAIVETY — `FanFaces` (decimate.md:318-322) and `CollapsedFaces` (:342-347) SCAN THE ENTIRE static `edit.Faces` array per collapse (filtered only by `store.Valid`), and `Drain` calls both per popped edge → O(F²) overall LOD loop, where a half-edge/face-incidence structure gives O(F log F). The vertex adjacency IS maintained incrementally (decimate.md:330-334) but the face-level ops regressed to full scans (VC9). (b) namespace ≠ folder. (c) consumes the Healing-owned `MeshEdit` (decimate.md:35) (VC6).
- Charter as-is: right; replace the face-array scans with the incidence structure the page half-maintains.

### Drawing/view.md — VERDICT 9/10 (namespace `Rasm.Geometry.Projection` ≠ folder Drawing)
- Newell-Newell-Sancha BSP painter + Appel quantitative-invisibility HLR, exact-Orient3D silhouette locus, Section delegated to `IntersectOp.PlaneMesh` (view.md:16). `DrawingProjection` visible/hidden carrier.
- Defects: (a) HLR OWNED TWICE across strata — Fabrication/Posting/projection independently authors "BSP front-to-back HLR projection" (Fabrication ARCHITECTURE.md:32) and its seam SUPERSEDES the AppUi painter (:45); the owning ledger says view→Fabrication/Posting consumes (ARCHITECTURE.md:51), and view.md:437 [PROJECTION_CONSUMERS] asserts Fabrication is a consumer — but Fabrication declares its own. BOTH feed AppUi/Render (ARCHITECTURE.md:52 + Fabrication:45) (VC4). (b) namespace ≠ folder. (c) `using Rasm.Geometry.Healing` (view.md:30) for the Healing-owned MeshEdit (VC6).
- Charter as-is: correct sole predicate-exact HLR owner; Fabrication must consume DrawingProjection, not re-implement BSP HLR.

### Drawing/pack.md — VERDICT 8.5/10 (namespace `Rasm.Geometry.Encoding` ≠ folder Drawing)
- 8-channel `EncodingChannel` feature lattice, `PackOp` (PointCloud/MeshPatch/VoxelGrid/BrepPatch), contiguous SoA `ReadOnlyMemory<float>` payload, round-trip witness keyed by CANONICAL_BYTE_IDENTITY, Compute EncodedTensor + AppHost GeometryPacking residency seam (pack.md:16). EncodedGeometry is the kernel producer AppHost (ARCHITECTURE.md:48) + Compute (Compute ARCHITECTURE.md:125) consume.
- Defects: (a) depends on `ScalarField.SampleDetailed` which pack.md:459 admits is "the public analogue of the landed SampleSdfDetailed, the settled-contract seam the Vectors source-pass exposes" — i.e. an UNEXPOSED Vectors member (unwired seam, VC8). (b) references the phantom `Rasm.Geometry.Topology` namespace (pack.md:33) for CanonicalTopology/NamingHashOps (VC1). (c) namespace ≠ folder.
- Charter as-is: right; the Vectors `ScalarField.SampleDetailed` seam must be exposed, and the Topology namespace resolved.

### Parametric/curve.md — VERDICT 8/10 (namespace `Rasm.Geometry.Parametric` — ORPHAN, folder undeclared)
- Host-neutral GShark NURBS parametric owner (Curve/Surface, evaluate/measure/divide/split/reconstruct/intersect), all cited members VERIFIED in api-gshark.md (`PerpendicularFrameAt`, `Fitting.Curve.Interpolated/Approximate/InterpolateBezier`, `Intersection.Intersect.CurveCurve/CurveLine/CurvePlane`, `NurbsSurface.FromLoft/IsoCurve/FromExtrusion/FromSweep/Ruled/Revolved`). This is the Generation KERNEL-GATE answer for curve stations/frames/isolines.
- Defects: (a) ORPHAN — the `Parametric/` folder + `Rasm.Geometry.Parametric` namespace are ABSENT from the owning ARCHITECTURE.md codemap (which lists only Numerics/Spatial/Meshing/Processing/Drawing, :15-36), the NAMESPACE_LAW (:64), and the faults band (VC2). (b) ADMITS GShark (curve.md:3 "GShark pure-managed NURBS engine"), directly CONTRADICTING ARCHITECTURE.md:3,14 "the Rasm.Geometry.* kernel that admits no external geometry library" — the README roster DOES admit GShark (README.md:45), so this is an internal ARCHITECTURE-vs-README contradiction (VC2). (c) `Apply`/`CurveApply`/`SurfaceApply`/`ToMesh`/`ToPolyline`/`CurveFrom`/`SurfaceFrom` are SIGNATURE-ONLY (curve.md:116-137), transcription-deferred like delaunay.
- Charter as-is: essential for Generation; must be DECLARED as a sanctioned sub-domain, the no-external-lib invariant amended, and the deferred bodies fenced.

---

## [PART_2] — CROSS-CUTTING FINDINGS

### Folder / namespace / fault-cluster tri-way incoherence (the defining rot)
The owning ARCHITECTURE.md:64 NAMESPACE_LAW is explicit: "Rasm.Geometry.Numerics/Spatial/Meshing/Processing/Drawing, one namespace per sub-domain ... Meshing the Delaunay/arrangement/intersection/offset owners, Processing the heal/receipt/decimate/flatten/fit/solver kernels." The fences CONTRADICT this on ~15 of 18 pages:

| Folder | Fence namespace(s) | Fault cluster | Matches law? |
|---|---|---|---|
| Numerics | `Rasm.Geometry.Numerics` (predicates), `Rasm.Geometry` (faults) | spatial/topology/… (all) | mostly |
| Spatial | `Rasm.Geometry.Spatial` (index) + `Rasm.Geometry.Topology` (naming/recon per imports) | spatial + topology | SPLIT |
| Meshing | `.Tessellation` + `.Arrangement` + `.Intersection` + `.Offsetting` | tessellation? + arrangement + intersection + offsetting | NO (4 ns) |
| Processing | `.Healing` + `.Constraints` + `.Fitting` + `.Parameterization` + `.Simplification` | healing + constraints + fitting + parameterization + simplification | NO (5 ns) |
| Drawing | `.Projection` + `.Encoding` | projection + encoding | NO (2 ns) |
| Parametric | `.Parametric` | (none) | UNDECLARED |

The fence namespaces + the 12 fault clusters ARE the true concept structure (~14 deep owners). The 5-folder codemap is the naive coarse bucket. This is a binary campaign ruling (see VC1).

### Duplicate mechanisms (concern owned twice)
1. Exact crossing: intersect.md (owner) vs inline copies in arrangement.md:242-262, offset.md:440-444, repair.md:346-372 (4 copies of `PlaneCrossPoint`/`InTriangle`/`DominantAxis`/straddle). VC3.
2. HLR: Drawing/view.md vs Fabrication/Posting/projection — both BSP hidden-line, both feed AppUi/Render. VC4.
3. Straight-skeleton/medial: Meshing/offset.md vs Fabrication/Toolpath/skeleton. VC4.
4. GWN winding: Spatial/index.md `Winding` (Barnes-Hut tree-code) vs Vectors "triangle solid-angle winding SDF" (Vectors _ARCHITECTURE:99).
5. LM damped-Gauss-Newton iterate: solver.md `Step`/`Iterate` (private, Constraint-coupled) vs fit.md `Refine` vs decimate.md `OptimalPosition` Cholesky solve — three MathNet-Cholesky normal-equation solves, no shared owner. VC7.
6. `BooleanReceipt` type-name: arrangement.md:115 vs receipts.md:44 — same name, different shape/namespace. VC5.

### Concern mixing / mis-homed owners
- `MeshEdit` (the kernel mesh working-set) is owned by repair.md (namespace `Healing`, repair.md:76) but consumed by intersect, arrangement, view, pack, decimate — 5 non-healing pages import `Rasm.Geometry.Healing` for the core mesh soup. It is a shared substrate mis-homed inside the heal owner. VC6.
- `BooleanOp` `[SmartEnum<int>]` discriminant lives in repair.md:57 (Healing) but the boolean ALGORITHM is arrangement.md and the weld is repair.md — boolean is split across the repair⇄arrangement cycle. VC5.

### Page-level cycles
- repair.md (Healing) ⇄ arrangement.md: repair.Boolean composes `Arrangement.Apply` (repair.md:288); arrangement composes `Healing.BooleanOp`/`WeldDuplicates`/`MeshEdit` (arrangement.md:18). Mutual dependency.
- MeshEdit hub: repair → {intersect, arrangement, view, pack, decimate} all depend on the Healing namespace for MeshEdit (a hub, not a cycle, but a coupling concentrator).

### Hardcoding-vs-generator (naivety-APPROACH)
Largely CLEAN — every owner is a `[Union]`/`[SmartEnum]` + FrozenDictionary data-table dispatch with growth-by-row. The rosters (predicate kinds, heal ops, fit primitives, param kinds, pack channels, view kinds) are correctly seed DATA feeding one polymorphic Apply. No enumerated-instance-where-a-generator-belongs defects found. The naivety here is COVERAGE (missing gate capabilities, VC10) and the O(F²) hot-loop scans (VC9), not hardcoded rosters.

### Dead / mis-reported typed carriers
- flatten.md:330 `FactorNonZeros => Calculus.D1.NonZeros` reports the wrong operator's nnz (factor is the D0-stiffness Cholesky).
- arrangement.md `ArrangementStore.FaceOrigin`/`FaceInside` are set EMPTY on the welded boolean path (arrangement.md:109) — honestly documented as "consumed" not ghost, defensible.

### Unwired seams (declared-need, unrealized counterpart)
- pack.md → Vectors `ScalarField.SampleDetailed` (unexposed public Vectors member, pack.md:459,291).
- receipts.md/repair.md → Vectors public `TopologyReceipt` projection for IsManifold/NonManifoldEdges (receipts.md:441 — internal only).
- flatten.md/pack.md/curve.md/fit.md/decimate.md all lean on the Vectors public surface (`MeshAdjointSnapshot.Of`, `DiscreteCalculus`, `CholeskySparse`, `Matrix.DecomposeSvd`, `VectorCloudMetric.*`, `ScalarField.*`, `FeatureEdge`) — mostly SETTLED per Vectors _ARCHITECTURE, but `SampleDetailed` and the topology-projection row are gaps.

### Unmined admitted capability
- MathNet: doctrine scopes MathNet to linear algebra ("dense factorization, spectral, Fourier, quadrature", algorithms.md:5); `MathNet.Numerics.Optimization.LevenbergMarquardtMinimizer` is ABSENT from api-mathnet-numerics.md — the hand-rolled LM (solver/fit/decimate) is doctrine-sanctioned as an owned-build, BUT it is duplicated 3x with no shared owner (VC7). Decide whether to admit MathNet.Optimization (collapses all 3) or author one shared kernel damped-GN owner.
- SharpVoronoiLib ships a bundled public kd-tree (api-kdtree.md:104) alongside the admitted `Supercluster.KDTree.Net` — potential package-level kd-tree duplication; confirm one categorical owner.

### Folder-architecture verdicts
- The 5-folder codemap is NOT conducive to growth: each folder is a coarse bucket over 1-6 unrelated deep owners whose namespaces/fault-clusters already diverge. Adding a concept means either violating the namespace law (current practice) or cramming into a coarse namespace.
- Parametric/curve.md is a loose one-file-one-folder combo (156 LOC, 1 page) that is ALSO undeclared — the worst of both (orphan + singleton folder).
- Recommendation: the coherent structure is folders = concepts = namespaces = fault clusters (~14 sub-folders: Numerics, Spatial, Topology, Tessellation, Arrangement, Intersection, Offsetting, Healing, Constraints, Fitting, Parameterization, Simplification, Projection, Encoding, Parametric), each a genuine 300-700 LOC deep owner. This matches the "real higher-order domain folders, source-mirroring" law (libs/.planning/architecture.md:77) and the fault-band sub-clustering already in place.

### Seam-ledger diff vs owning ARCHITECTURE.md [02]-[SEAMS] (both directions)
WIRED-UNDECLARED (real in pages/counterparts, absent from owner ledger):
- pack → Compute/Tensor/residency (EncodedGeometry→EncodedTensor, Compute ARCHITECTURE.md:125) — owner only has pack→AppHost (:48).
- pack → AppHost/Sandbox/solver (PackKind rows, Compute ARCHITECTURE.md:126) — owner says pack→AppHost/Runtime (anchor drift Runtime vs Sandbox).
- repair(Healing) → Fabrication/Posting/projection (BooleanOp, Fabrication ARCHITECTURE.md:43, names phantom `Rasm/Geometry/Healing`) — owner has no such seam.
- Parametric/curve → {view, pack, fit, Fabrication, Bim} (curve.md:191) — entire sub-domain undeclared.
- flatten/decimate/pack → Vectors DEC/ScalarField/VectorCloudMetric (intra-kernel, not in ledger; SampleDetailed unexposed).

DECLARED-MIS-STATED (in a ledger, but the real relationship differs):
- solver ⇄ Compute (Compute ARCHITECTURE.md:120 claims Geometry consumes Compute LM — SPURIOUS downward strata violation; solver.md composes MathNet).
- Spatial/index ⇄ Compute (owner :58 says ⇄; Compute :121 is one-directional consume — ⇄ overstates).
- view → Fabrication/Posting (owner :51; Fabrication re-implements BSP HLR instead of consuming).

ANCHOR DRIFT (same seam, inconsistent anchors across ledgers):
- reconciliation→Persistence: `Persistence/Query` (owner :45) vs `Persistence/Version/StructuralMerge` (reconciliation.md:2) vs `Query/topology` (Persistence :55).
- intersect section→Fabrication: `Fabrication/Posting` (owner :49) vs `Toolpath/slicing` (Fabrication :46) vs silhouette/NFP (intersect.md:389).

### Consumer-gate coverage (the two gated campaigns)
GENERATION KERNEL GATE (RASM-GENERATION-SPEC.md:179) — ~70% met:
- MET: arc-length stations + frames on curves (curve.md Divide/PerpendicularFrameAt/ParameterAtLength), surface isolines (curve.md NurbsSurface.IsoCurve), region tessellation (delaunay + curve ToMesh), developable/panelization + UV (flatten LSCM/ARAP/BFF).
- GAP: subdivision surfaces (Catmull-Clark/Loop) — NO kernel owner; NURBS-surface geodesics — absent (only mesh geodesics in Vectors ScalarField.Geodesic); explicit pattern-to-surface mapping — only implicit via flatten UV inverse.
FABRICATION TOOLPATH-GRADE (brief mandate) — ~75% met:
- MET: mesh booleans/repair at tolerance (arrangement + repair), medial axis (offset.Medial), distance fields (Vectors SDF + pack occupancy), unroll (flatten), single planar section (intersect PlaneMesh).
- GAP: offset corner strategies (JoinType round/miter/square/bevel + EndType) — absent from offset.Offset; slice STACK (parallel section planes for FFF/DED) — intersect is single-plane, Fabrication stacks; adaptive-clearing region — kernel has the medial/skeleton substrate but Fabrication duplicates it.

---

## [PART_3] — VERDICT CANDIDATES (campaign-defining structural rulings, most-severe first)

Delivered as structured verdict_candidates in the tool return. Summary evidence anchors:
1. Folder↔namespace↔fault-cluster tri-way incoherence — ARCHITECTURE.md:64 vs 15 fence namespaces.
2. Parametric orphan + no-external-geometry-library contradiction — curve.md:3 vs ARCHITECTURE.md:3,14 vs README.md:45.
3. Exact-crossing collapse declared-unrealized (4 inline copies) — intersect.md:388 vs arrangement:242, offset:440, repair:346.
4. HLR + straight-skeleton owned twice across kernel↔Fabrication — view vs Fabrication:32; offset vs Fabrication:24.
5. Boolean fractured across repair⇄arrangement cycle + duplicate BooleanReceipt — repair:57/288, arrangement:18/115, receipts:44.
6. Seam-ledger drift both directions — Compute:120/125, reconciliation anchor 3-way, view→Fabrication unhonored.
7. In-kernel LM iterate duplicated 3x, no shared damped-GN owner — solver:295, fit:557, decimate:363.
8. MeshEdit mis-homed inside the Healing owner (5 consumers) — repair:76.
9. O(F²) hot loops (naivety-APPROACH) — flatten IndexOf:482, decimate FanFaces:318/CollapsedFaces:342.
10. Consumer-gate coverage gaps — subdivision/surface-geodesic/pattern-map (Generation); JoinType/slice-stack (Fabrication).
