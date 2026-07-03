# DOSSIER — corpus-b — libs/csharp/Rasm/Geometry/.planning

Lane: corpus-b. Deep-read scope = second half of alphabetically-sorted subfolders: **Parametric, Processing, Spatial** (10 pages, 3,531 LOC). First half (Drawing, Meshing, Numerics) skimmed only at crossed seams (arrangement, tessellation, predicates, faults). Stance: hostile; disk beats prose.

Governing law read: `libs/.planning/architecture.md` (strata + dependency direction), `libs/csharp/Rasm/ARCHITECTURE.md` (Geometry codemap `[01]`, seams `[02]`, namespace law `[03]`), `RASM-GENERATION-SPEC.md` (`[04]` KERNEL GATE + `[06]` exposure), `RASM-COMPONENT-PARADIGM-DECISION.md` (SectionProfile/SpineRef/Placement seams). Member verification via `libs/csharp/Rasm/.api/` (kdtree/gshark/doubledouble) + `libs/csharp/.api/`.

---

## VERIFICATION LEDGER (external members = REAL; Vectors seams = UNVERIFIED)

External-package member discipline is **excellent** — every external member corpus-b composes resolves against the `.api` catalogs:
- `api-kdtree.md`: pkg `Supercluster.KDTree.Net`, **namespace `SuperClusterKDTree`** (short name — NOT `Supercluster.KDTree.Net`), `KDTree.Create(points,nodes,DistanceMetrics)`, `NearestNeighbors`, `RadialSearch`, `DistanceMetrics.EuclideanDistance` = **squared-L2**. `Spatial/index.md` uses `using SuperClusterKDTree;`, squares the radius before `RadialSearch` (index.md:380), and notes the squared metric (index.md:375) → VERIFIED CORRECT.
- `api-gshark.md`: v`2.3.1`, `netstandard2.0`, ns `GShark.Geometry`/`Fitting`/`Intersection`; `NurbsBase.PerpendicularFrameAt`/`Divide`/`DivideByChordLength`, `Fitting.Curve.Interpolated`/`Approximate`/`InterpolateBezier`, `Intersection.Intersect`, `NurbsSurface.FromLoft`/`FromExtrusion`/`FromSweep`/`Ruled`/`Revolved`/`IsoCurve` → `Parametric/curve.md` VERIFIED CORRECT.
- `api-doubledouble.md`: pkg `TYoshimura.DoubleDouble` v`5.0.8`, ns `DoubleDouble`, `ddouble` with full `System.Numerics` generic-math → `decimate.md`/`fit.md` `using DoubleDouble;` + `ddouble`/`ddouble.Zero`/`ddouble.Sqrt` VERIFIED CORRECT.

**Verification EXPOSURE (campaign must close via `assay api` over `Rasm.Vectors`):** the most sophisticated corpus-b pages rest on UNVERIFIED mature-`Rasm.Vectors` members, not external `.api`:
- `flatten.md`: `MeshAdjointSnapshot.Of`, `DiscreteCalculus.{D0,D1,Star1}` with `D0.RowPtr`/`D0.ColInd`/`D0.Cols`/`Star1[e]` (CSR-shape assumption), `CholeskySparse.Of`, `SparseMatrix.FromTriplets`/`Solve`, `Matrix.DecomposeSvd`/`Dimension.Create`. If the real DEC surface is not CSR-with-`RowPtr`/`ColInd`, `MeshDec.StiffnessTriplets` (flatten.md:349) is wrong.
- `decimate.md`: `VectorCloudMetric.PrincipalCurvature`, `CloudCurvatureSample.{K1,K2}`, `ScalarField.SignedDistanceFromMesh`/`IsoSurfaceDetailed`, `SdfMeshPolicy.GeneralizedWinding`, `VectorCloud.Cluster`, `VectorIntent.Cloud`.
- `fit.md`: `VectorCloudMetric.OrientedNormals`. `receipts.md`/`repair.md`: `VectorIntent.Topology(space).Project<(int Euler,int Genus,int BoundaryComponents)>` (repair.md:399-400 — the pages themselves flag `MeshKernel.TopologyDetailed` as `internal` and `IsManifold`/`NonManifoldEdges` as non-projectable, so `ManifoldStatus` is deliberately the 3-tuple only).

---

## PER-PAGE

### Parametric/curve.md — VERDICT 5/10
Charter (as-is): host-neutral parametric curve/surface owner — ONE `Parametric [Union]`(Curve/Surface) over GShark NURBS; `ParametricOp [Union]`(Evaluate/Measure/Divide/Split/Reconstruct/Intersect) via one `Apply`; `SurfaceFactory [Union]`(7); `EvalResult [Union]`(5). GShark usage is member-verified and the boundary-marshal discipline (map to `Rasm.Vectors` at the seam, escalate degeneracy to `Numerics/predicates`) is correct.

Defects:
- **KERNEL-GATE UNDER-COVERAGE (critical, on the gated critical path).** RASM-GENERATION-SPEC `[04]` (line 179) demands stations/frames on curves ✓ (`Divide`+`PerpendicularFrameAt`), surface isolines ✓ (`IsoCurve`), but **geodesics (ABSENT), surface offset (ABSENT), region subdivision/Catmull-Clark-Loop (ABSENT), developable/unroll (ABSENT), panelization (ABSENT), pattern-to-surface mapping (ABSENT)**. Curve `Offset` (`NurbsBase.Offset`) is demoted to Growth (curve.md:19) though it is a KERNEL-GATE + Fabrication toolpath item. GShark's surface algebra (loft/extrude/sweep/ruled/revolved/isocurve/pointat) structurally CANNOT supply geodesic/offset/unroll/panelize — no owner exists anywhere in the corpus for ~half the gate.
- **ONE-FILE FOLDER.** `Parametric/` holds only `curve.md` (156 LOC) — loose one-file-one-folder combo, the exact folder-architecture rot the brief grades.
- **TITLE/CHARTER MISMATCH.** `curve.md` owns Curve AND Surface; either rename to `parametric.md` or split the folder into the curve/surface operational tier that carries the gate (see verdict candidate V2).
- **DEPTH OUTLIER.** Signature-only: bodies elided with `;` (curve.md:116-138, `CurveApply`/`SurfaceApply`/`ToPolyline`/`ToMesh` are declarations) while every Processing/Spatial sibling is full-body. Least-realized page carrying the most downstream weight.

Owner charter (as it SHOULD be): the Parametric folder becomes the **curve/surface operational tier** — `curve.md` (GShark eval/measure/divide/split/reconstruct/intersect), `surface.md` (NURBS surface eval + the surface-ops the gate needs: isolines/geodesics/offsets), `develop.md` (developable/unroll/panelize/pattern-to-surface), `sample.md` (arc-length stations+frames as a first-class owner the Generation `SpineRef` consumes). Curve offset promoted to a first-class `ParametricOp` case NOW.

Split/merge/move: SPLIT the folder into the multi-owner tier above; PROMOTE offset; ADD the gate owners.

### Spatial/index.md — VERDICT 8/10
Charter (as-is): ONE `SpatialIndex [Union]` (Bvh/LinearOctree/PointCloud) over one SoA `NodeStore` + `SpatialQuery [Union]`(Range/Ray/Nearest/Overlap/Winding); builders SAH-BVH/Morton-octree/agglomerative/kd-tree; degradation-keyed `Refit`; `ToAcceleration` Compute seam with pinned `CLASH_GOLDEN`. Dense, full-body, member-verified, genuinely rich (Barill-Jacobson GWN as a query case, PLOC agglomerative, byte-pinned clash wire).

Defects:
- **PointCloud breaks the union's core invariant.** The whole page is `NodeStore`-centric, but `PointCloud` (kd-tree, no NodeStore) forces exception escapes: `Store` getter throws (index.md:130), `ToAcceleration` throws (index.md:574,579), and `Query` for Ray/Overlap/Winding over a PointCloud returns `new QueryResult.Hits(Seq<int>())` — a **silent empty** (index.md:358) while the prose claims "a Ray/Overlap/Winding over a PointCloud is a kind-mismatch, never a silent empty" (index.md:351). Exception-style control flow + prose-vs-fence, contradicting both the ROP mandate and the page's own boundary law.
- **Non-conservative float32 bounds.** `NodeStore.Write` narrows `double`→`float` via round-to-nearest `Single` (index.md:79). Internal-node bounds can shrink below the true child union → the descent `if (!Intersects(bound, ...)) continue` (index.md:453,474,518) can prune a subtree holding a real hit → range/ray/overlap FALSE NEGATIVE. Broad-phase bounds must round min DOWN / max UP.
- **Stale Cases counts after the PointCloud graft.** Cases prose (index.md:14) says "SpatialKind rows bvh·octree·agglomerative (3)" and "SpatialIndex cases Bvh·LinearOctree (2)" but the fence adds `PointCloud` (4th kind, 3rd union case, index.md:44,118); DENSITY_BAR (index.md:707) mislabels row [02] the same way.
- **`ToAcceleration` returns bare `AccelerationStructure`** (index.md:572) so it cannot rail; the PointCloud path throws instead of `Fin`.

Owner charter: keep the BVH/octree/agglomerative union NodeStore-pure; **lift PointCloud to a sibling owner** (`cloud-index.md`) or a distinct union so the kd-tree stops forcing throws; make `ToAcceleration` return `Fin<AccelerationStructure>`; round bounds outward.

Split/merge/move: MOVE PointCloud out of `SpatialIndex` union.

### Spatial/naming.md — VERDICT 6/10
Charter (as-is): persistent topological naming — `TopoName [ValueObject<UInt128>]` lineage ref over one `EntityKind`(Vertex/Edge/Face); `TopoSignature` position-free fingerprint; `NameTable` registry + signature index + `VertexNames` row; `Track` re-anchor fold (Survived/Migrated/Born) over the `Fin` rail. Dense, thoughtful, one-fold-serves-all-modalities.

Defects:
- **Vertex-index mis-keying (CONFIRMED logic bug, cross-page).** `Track` registers `VertexNames` via `state.Table.With(bound.Entry, entity.IncidentVertices[0])` (naming.md:113). For a Vertex, `RebuiltEntity.IncidentVertices = star[v]` (reconciliation.md:62, the sorted set of self+neighbours), so `[0]` is the MINIMUM index in the star, **not v itself**. Any vertex that is not the smallest among its neighbours registers its name under a neighbour's index → `ResolveBoundary` (naming.md:97) returns wrong/empty names → edge/face `TopoSignature`s corrupt. Root cause: `RebuiltEntity` carries no self-index field.
- **`Subsumes` direction likely wrong for splits.** `TopoSignature.Subsumes` (naming.md:62-66) requires prior boundary ⊂ rebuilt boundary (strict superset). A split face's child holds FEWER of the parent's original boundary vertices, not more → migration mis-fires exactly on the split case it is meant to detect.
- **Self-flagged OPEN residual** (naming.md:161): Survive×Migrate injectivity unproven.
- **No namespace declared** in the fence (index.md declares `Rasm.Geometry.Spatial`; naming.md declares nothing) → feeds the namespace triad (see V1).

Owner charter: add an explicit `Self` index to `RebuiltEntity`; key `VertexNames` by the true vertex index; fix `Subsumes` to the correct split/merge subset relation; declare the namespace.

### Spatial/reconciliation.md — VERDICT 8/10
Charter (as-is): the naming↔hash fence — `CanonicalTopology.OfMesh` canonical adjacency; `Encode` (frozen byte layout Persistence `GeometryHash` reads); `Reconcile` (TopoName→content-hash, `Fin`); hosts the `ONE_WIRE_FIXTURE_CORPUS` index. Owns the one real host-derived byte fixture. Strong: keeps reference vs content identity orthogonal, one hasher (`Domain.ContentHash`), honest DESIGN-PIN vs REAL fixture states.

Defects:
- **Originates the vertex-star mis-key** (reconciliation.md:62 conflates the vertex self-index into `star[v]`), which naming.md then mis-consumes.
- **`0x9462A71A5DD13DCFA3B1D6D225FCBE70` presented as REAL/frozen** (reconciliation.md:126,147) — the one place a fabricated byte/digest could hide; must be harness-confirmed as the actual `XxHash128` of the 52-byte stream, not asserted.
- **No namespace declared** in the fence (same as naming.md).
- Minor: `star[v]` includes self (reconciliation.md:56) → vertex kind-histogram degree is +1 (deterministic, semantically odd).

Owner charter: emit an explicit self-index in `BuildEntities`; declare the namespace; keep the fixture-corpus index (good design — it is the cross-language parity spine).

### Processing/receipts.md — VERDICT 7/10
Charter (as-is): typed rebuild evidence — `RebuildReceipt [Union]`(7, one per HealOp) + `ManifoldStatus` projection + `HealSession`/`RebuildLog` fold feeding naming `Track`. Clean typed-per-kind receipts, no generic ledger.

Defects:
- **`Converged` inverted for booleans (CONFIRMED, cross-page).** `HealSession.Converged` requires `b.AssetGated == true` for a boolean receipt to count converged (receipts.md:99), but `RebuildReceipt.Of` hardcodes `AssetGated: false` at mint (receipts.md:65) and repair's managed `BooleanArrangement` succeeds without ever setting it true → **any heal session containing a boolean op reports Converged = false**. The predicate is semantically inverted (a successful boolean has `AssetGated=false`, which should be the converged case).

Owner charter: `Converged` for a boolean should test success (asset present / result manifold), not the gate flag; or `AssetGated` semantics must flip and be set at the arrangement seam.

### Processing/repair.md — VERDICT 7/10
Charter (as-is): heal rail — `HealOp [Union]`(6 author-kernel + 1 managed-arrangement boolean) folded by `Heal.Repair`, exact-`Predicate` gated weld/collapse/self-intersect; `MeshEdit` working set; arrangement-boolean seam member-verified (repair.md:290 matches arrangement.md:15 `Apply` signature). Genuinely rich first-principles kernels.

Defects:
- **`BooleanOp` strata inversion (cross-layer cycle).** `BooleanOp` is defined here in Processing/Healing (repair.md:57) but CONSUMED by `Meshing/arrangement.md` (arrangement.md:5,18,124) — Meshing is BELOW Processing in the floor-first order (ARCHITECTURE.md:39). Lower layer depends on higher layer's type; and Healing→Arrangement (for the boolean body) + Arrangement→Healing (for `BooleanOp`) is a circular type dependency. Compounded by TWO distinct `BooleanReceipt` records sharing the name (receipts.md + arrangement.md:124).
- **namespace `Rasm.Geometry.Healing`** ≠ ARCHITECTURE's mandated `Rasm.Geometry.Processing` (see V1).
- **Double topology recompute per op.** `Heal.Repair` computes `before`+`after` via `ToSpace`+`VectorIntent.Topology.Project` for EVERY op (repair.md:382-385) → 2N full mesh rebuilds for N ops; `before[n]` == recomputed `after[n-1]`. Efficiency, not correctness.
- Brittle hard cast `((QueryResult.Hits)hits)` (repair.md:243) relies on the query-kind→result-kind contract totally.

Owner charter: move `BooleanOp` to `Meshing/arrangement` (the boolean owner) or a Numerics-floor shared vocabulary; Healing composes it upward. Thread topology status forward (after[n-1] → before[n]).

### Processing/solver.md — VERDICT 9/10 (best page in corpus-b)
Charter (as-is): ONE author-kernel geometric constraint solver — closed `Constraint [Union]`(9) with one `Residual` fold + analytic `Partials` per arm, `WitnessAnalyze` numeric-rank DOF refinement (adds RedundantConsistent), Levenberg-Marquardt `Solve` over MathNet Cholesky, `Fin` rail. Award-grade: analytic Jacobians throughout, NaN-trial rejection, witness DOF, honest reject-budget analysis (solver.md:391).

Defects:
- **namespace `Rasm.Geometry.Constraints`** ≠ `Rasm.Geometry.Processing` (V1).
- **Stale namespace reference.** Comment cites "the sibling `Rasm.Geometry.Topology` namespace" (solver.md:38) which ARCHITECTURE.md:64 explicitly forbids ("the robust-core mints no `Topology` namespace") and no page declares.
- **Un-simplified derivative in the product fence.** `TangentRow` writes `(dy * 1.0 - cy * 0.0 + (-cy))` = `dy - cy` (solver.md:204) — correct but sloppy code-gen residue; hygiene defect in a page whose fences ARE the product.

Owner charter: this LM iterate is the ONE the whole geometry domain should compose (see V3). Generalize `Solve` to accept a residual+Jacobian functor + `SolvePolicy` so `fit.md` instantiates it rather than re-rolling it.

### Processing/decimate.md — VERDICT 7/10
Charter (as-is): predicate-gated QEM decimation — `SimplifyOp [Union]`(QuadricCollapse/ProgressiveMesh/VoxelRemesh/FeaturePreserve) over one `QuadricStore`; 106-bit `ddouble` quadrics; exact-`Orient3D` collapse gate + link condition; BVH-`Nearest` one-sided Hausdorff; reversible vsplit; SDF voxel remesh. Member-verified `ddouble`. Rich and largely correct at the algebra level.

Defects:
- **O(F²) collapse loop (fundamental-approach + prose-vs-fence).** Prose claims "half-edge face-fan adjacency so a vertex's incident faces are O(1)" (decimate.md:16), but NO face adjacency is ever built — `FanFaces` (decimate.md:318-322), `CollapsedFaces` (342-347), and `NoAdmissibleCollapse` (351-360) all linear-scan the full immutable `edit.Faces` per collapse. `Drain` pops O(F) edges × O(F) scan = O(F²). Faces are never compacted until `Emit`. Decimating 25% of a 1M-face mesh ≈ 7.5e11 face touches.
- **Boundary edges never collapse.** `LinkCondition` hardcodes `shared == 2` (decimate.md:312) — the interior-edge link condition. A boundary edge needs `shared == 1`, so boundary collapse is always rejected → boundary frozen → `TargetFraction` unreachable on open meshes → spurious `DecimationFault(budget, achieved)`.
- **namespace `Rasm.Geometry.Simplification`** ≠ Processing (V1).
- Local `Solve`/Cholesky wrapper (decimate.md:374-379) duplicates solver.md + fit.md (V3).

Owner charter: build real vertex→incident-face adjacency in `QuadricStore.Seed` and maintain it across collapses; handle boundary-edge (`shared==1`) link condition; compose the shared Cholesky-solve rail.

### Processing/flatten.md — VERDICT 5/10
Charter (as-is): UV flattening — `ParamOp [Union]`(Harmonic/Lscm/Arap/Bff) composing the Vectors DEC substrate, cached `CholeskySparse` factor, exact-`Orient2D` flip guard, `ChartAtlas`/`DistortionReceipt`. The most-detailed page — and the one whose depth hides the most real defects.

Defects:
- **Dirichlet pin is mathematically broken (CONFIRMED).** `StiffnessTriplets` adds `Policy.MassShift` (1e-9) to EVERY diagonal (flatten.md:359, not just pinned rows), and `PinnedRhs` sets boundary rhs = `boundaryValues[k] / Policy.MassShift` = value·1e9 (flatten.md:364). A correct penalty method needs a LARGE penalty on PINNED rows only + rhs = penalty·value. As written, the boundary-row equation `(L_row + 1e-9)·x = value·1e9` forces garbage ~1e9 into boundary-adjacent solves; the interior harmonic/BFF result is corrupted (Scatter overwrites boundary post-hoc at flatten.md:370, but the interior was already solved against the corrupted rhs).
- **LSCM built DENSE (fundamental-approach + prose-vs-fence).** Page prose: "smallest-singular-vector of a **sparse** least-squares operator" (flatten.md:4). Fence: `ConformalOperator` allocates `new double[(2n)*(2n)]` (flatten.md:377) and `FlattenLscm` runs full `DecomposeSvd` on it (flatten.md:173) — O(n²) memory, O(n³) SVD. Will not scale past tiny charts; defeats the entire point of Lévy-2002 sparsity.
- **O(F²) ARAP via gratuitous re-lookup.** `AccumulateRotated` calls `IndexOf(i,j,k)` (flatten.md:474) — an O(FaceCount) linear scan to recover a face index it was ALREADY given (`RotatedGradient` has `f` at flatten.md:395). O(F²) per ARAP iteration × `MaxIterations`(64). `IndexOf` (flatten.md:482-485) also fragile on rotated/duplicate triples (returns 0 on miss).
- **Misleading carrier.** `FactorNonZeros => Calculus.D1.NonZeros` (flatten.md:330) reports the curl operator nnz as "the cached Cholesky factor fill" (receipt semantics, flatten.md:17) — wrong operator.
- **Dead carrier.** `ChartStore.Dead`/`FreeList`/`Kill` (flatten.md:81) never used.
- **namespace `Rasm.Geometry.Parameterization`** ≠ Processing (V1).

Owner charter: correct penalty-method Dirichlet (large penalty on pinned rows, rhs=penalty·value) or the eliminate-boundary-rows formulation; assemble LSCM as the SPARSE conformal-energy least-squares / smallest-eigenpair (not dense SVD); precompute per-face cotangent scatter so ARAP global assembly is O(F); wire or delete `Dead`/`FreeList`; report the actual factor fill.

### Processing/fit.md — VERDICT 8/10
Charter (as-is): efficient-RANSAC primitive fit — `FitOp [Union]`(Plane/Sphere/Cylinder/Cone/Torus/Line); `FitPrimitive` with one `Distance` + analytic `Gradient` per kind; MLESAC/PROSAC truncated-cost consensus + adaptive budget; geometric-orthogonal-distance LM refine; 106-bit `ddouble` objective; seeded/reproducible. Member-verified. Strong, coherent, the analytic ODR Jacobians are real.

Defects:
- **Re-implements the LM solver.md owns (CONFIRMED duplication + prose-vs-fence).** Prose: "reusing the `ConstraintSolver` iterate", "a **separate refine loop beside the constraint solver is the named double-owner defect**" (fit.md:16,731). Fence: fit.md locally re-implements the entire LM — `Iterate`/`Step`/`Linearize`/`SolveLinear` (fit.md:568-611) — duplicating solver.md:295-347. It commits exactly the double-owner defect it names.
- **Damping not adaptive.** `Iterate` resets `lambda: 1e-3` every OUTER iteration (fit.md:576); `Step`'s accept branch never carries λ down (no LambdaDown), so the "accept → toward Gauss-Newton" claim (fit.md:16) is unrealized. LM constants hardcoded (1e-3, ×10) rather than `FitPolicy`-driven, unlike solver.md's parameterized `SolvePolicy`.
- **Dead carrier.** `FitStore` is documented as "the flat inlier/residual SoA the consensus accumulates" (fit.md:3,13,248) but NEVER used — consensus accumulates into `Candidate` (fit.md:344); `FitStore.Empty` is never called.
- **`Line` parameterization degenerate.** `FreeParameters=4` fixes anchor Z (`Unpack` line, fit.md:645) → biased for near-vertical lines; `MinimalSamples=3` but `MinimalLine` uses 2 (fit.md:481).
- **namespace `Rasm.Geometry.Fitting`** ≠ Processing (V1).

Owner charter: compose solver.md's generalized LM (residual+Jacobian functor); thread λ through the refine state with a policy-driven up/down ladder; delete or wire `FitStore`; use a Plücker/2-point line chart that is not Z-degenerate.

---

## CROSS-CUTTING

**A. Namespace scheme unreconciled (three-way).** ARCHITECTURE.md `[03]` + codemap mandate one-namespace-per-FOLDER (Numerics/Spatial/Meshing/Processing/Drawing) and explicitly "no `Topology` namespace; naming lives under `Rasm.Geometry.Spatial`" (ARCHITECTURE.md:64). The 18 pages realize one-namespace-per-CONCEPT: `Encoding`(pack), `Projection`(view), `Arrangement`/`Tessellation`/`Intersection`/`Offsetting`(Meshing), `Simplification`(decimate), `Fitting`(fit), `Parameterization`(flatten), `Healing`(repair), `Constraints`(solver), `Spatial`(index). `faults.md:3` cluster taxonomy AND `solver.md:38` reference a `topology` namespace the law forbids; `naming.md`/`reconciliation.md` declare NONE. The cross-page `using`s are self-consistent with the per-concept scheme, so the concept scheme is likely the winner — but then ARCHITECTURE's law + codemap comments are stale and must be rewritten, naming/reconciliation must declare a namespace, and "topology" vs "spatial" for those two must be chosen once.

**B. LM iterate + Cholesky-solve re-implemented 3×.** solver.md owns damped-normal-equations LM (`Iterate`/`Step`/`SolveCholesky`, solver.md:295-347). fit.md re-implements the whole LM (fit.md:568-611) + a `ddouble` `Norm`. decimate.md carries its own `Solve` Cholesky wrapper (decimate.md:374). The doctrine and fit.md's own prose say a fit "instantiates that established λ-ladder rather than minting a second"; the fences mint 2-3 copies. Single biggest collapse opportunity: one parameterized LM engine (functor + policy) + one Cholesky-solve rail the others compose.

**C. Cross-layer type ownership inversion.** `BooleanOp` owned by Processing/Healing (repair.md:57) but consumed by Meshing/Arrangement (arrangement.md:5,18) — a lower strata layer depending on a higher one, plus a Healing↔Arrangement type cycle. Two distinct `BooleanReceipt` records share the name (receipts.md + arrangement.md:124).

**D. Prose-vs-fence optimism (systemic).** Several pages assert a property the fence does not deliver: decimate "O(1) incident-face fans" (re-scans all faces), flatten "sparse least-squares operator" (dense 2n² SVD) + correct Dirichlet pin (broken penalty), fit "reuses the ConstraintSolver iterate" (re-implements it), index "never a silent empty" (returns silent empty). The confident Boundary/RESEARCH "the deleted form"/"the named defect" prose needs a mechanical diff against each fence.

**E. Dead / misleading typed carriers.** fit.md `FitStore` (unused), flatten.md `ChartStore.{Dead,FreeList,Kill}` (unused) + `FactorNonZeros=D1.NonZeros` (wrong operator), index.md stale Cases counts. Declared+documented as central, never wired.

**F. Fence-tag taxonomy carries no meaning.** `csharp signature` vs `csharp contract` does NOT track signatures-only vs full-body: curve.md(signature) is elided; index/solver/naming/reconciliation(signature) are full-body; decimate/fit/flatten/pack(contract) are full-body. Either curve.md is under-realized (should be full-body `contract` like siblings) or the taxonomy needs a real, enforced meaning.

**G. Folder architecture.** Parametric/ is a one-file folder (rot). Processing/ is a healthy 6-page cluster but fragments into 6 namespaces (A) and re-implements the LM 2× within itself (B). Spatial/ is coherent (index+naming+reconciliation tightly coupled) but PointCloud is bolted onto the wrong union (index verdict) and naming/reconciliation drop their namespace. Numerics/faults.md is a clean single fault owner — all corpus-b fault references resolve (DegenerateInput/IndexMismatch/NameCollision/HashMismatch/UnrepairableMesh/NativeAssetMissing/OverConstrained/SingularSystem/FitFault/ParameterizationFault/DecimationFault), payload shapes match call sites, sub-banded 2400-2447, count 17 correct.

**POSITIVES (record, do not disturb):** external member discipline is award-grade (all `.api`-verified); solver.md is a reference-quality author-kernel; the fault family is a clean single owner with typed payloads; `CLASH_GOLDEN` (index) + `CANONICAL_BYTE_IDENTITY` (reconciliation) establish a real cross-language fixture spine; `Fin`/ROP discipline is near-universal (index.md's PointCloud throws the notable breach); the `Numerics/predicates` exact floor is composed correctly by repair/decimate/flatten (Orient2D/Orient3D/Sign, member-checked at the seam).

---

## VERDICT CANDIDATES (strongest, campaign-defining)

**V1 — Ratify ONE namespace scheme and reconcile ARCHITECTURE + every page + the fault-cluster taxonomy.** The realized per-concept scheme (Encoding/Projection/Arrangement/Tessellation/Intersection/Offsetting/Spatial/Simplification/Fitting/Parameterization/Healing/Constraints) contradicts ARCHITECTURE.md's `[03]` per-folder law and its "no `Topology` namespace" clause; `faults.md:3` + `solver.md:38` reference a forbidden `topology` namespace; `naming.md`/`reconciliation.md` declare none. Evidence: 14 `namespace` decls surveyed vs ARCHITECTURE.md:64. Pick per-concept (finer bounded-concept granularity; `using`s already consistent), rewrite ARCHITECTURE's law+codemap, and give naming/reconciliation an explicit namespace.

**V2 — The KERNEL GATE is under-covered; Parametric must grow into the curve/surface operational tier.** RASM-GENERATION-SPEC `[04]` (line 179) + Fabrication toolpath-grade needs demand geodesics, surface offset, subdivision, developable/unroll, panelization, pattern-to-surface, adaptive-clearing, slicing/section-stacks — NONE has an owner; `Parametric/curve.md` (156 LOC, one-file folder, signature-only) covers only stations/frames/isolines. Both gated campaigns (Generation stand-up, Fabrication scope expansion) block on this. Grow Parametric/ into a multi-owner tier (curve/surface/develop/sample) and promote curve offset to a first-class op NOW.

**V3 — Collapse the LM iterate + Cholesky-solve to ONE owner.** solver.md owns the damped-normal-equations LM; fit.md re-implements it wholesale (fit.md:568-611 vs solver.md:295-347) and decimate.md re-rolls the Cholesky wrapper — the exact "double-owner defect" fit.md's own prose names. Generalize `ConstraintSolver.Solve` to a residual+Jacobian functor + policy; fit.md instantiates it (its Distance/Gradient are already the functor); one Cholesky-solve rail.

**V4 — Move `BooleanOp` down out of Processing/Healing to resolve the strata inversion + type cycle.** `BooleanOp` (repair.md:57, Processing) is consumed by Meshing/arrangement (arrangement.md:5,18) — a lower layer depending upward, with a Healing↔Arrangement cycle and a duplicated `BooleanReceipt` name. Relocate `BooleanOp` to Meshing/arrangement or a Numerics-floor shared vocabulary; Healing composes it upward.

**V5 — Fix the confirmed math/logic bugs before any build leg.** (a) flatten.md Dirichlet penalty is inverted/mis-scaled (flatten.md:359,364) → corrupted harmonic/BFF interior; (b) flatten.md LSCM is dense O(n³) SVD despite "sparse" prose (flatten.md:173,377) → non-scaling; (c) naming.md/reconciliation.md conflate the vertex self-index into the star (naming.md:113 + reconciliation.md:62) → mis-keyed `VertexNames`; (d) receipts.md `Converged` inverted for booleans (receipts.md:65,99); (e) decimate.md O(F²) collapse loop + frozen boundary (decimate.md:312,318-322). Each is a fence-level defect in a page presented as transcription-complete.

**V6 — Excise dead/misleading typed carriers and re-sync stale counts.** fit.md `FitStore` (unused), flatten.md `ChartStore.{Dead,FreeList,Kill}` (unused) + `FactorNonZeros=D1.NonZeros` (wrong operator, flatten.md:330), index.md stale Cases counts (index.md:14,707). Dead carriers documented as central mislead every downstream reader.

**V7 — Establish a real fence-depth taxonomy.** `signature` vs `contract` is noise (curve.md alone is elided; all other "signature" pages are full-body). Either mandate full-body `contract` everywhere and rebuild curve.md to that depth, or define+enforce a genuine signature-tier meaning. Tied to V2 (curve.md is the sole under-realized page).

**V8 — Close the Vectors-seam verification gap and confirm the frozen fixtures.** The sophisticated pages rest on unverified `Rasm.Vectors` members (`MeshAdjointSnapshot`/`DiscreteCalculus` CSR shape, `VectorCloudMetric.{PrincipalCurvature,OrientedNormals}`, `ScalarField.{SignedDistanceFromMesh,IsoSurfaceDetailed}`, `VectorIntent.Topology.Project<(int,int,int)>`); external members are `.api`-verified but these are not. Run `assay api` over Rasm.Vectors; if the DEC surface is not CSR-with-`RowPtr`/`ColInd`, flatten.md:349 is wrong. Confirm the REAL digest `0x9462A71A…` (reconciliation.md:147) and the `CLASH_GOLDEN` 160-byte stream (index.md) against actual `XxHash128`/`NodeLinkProjection`.
