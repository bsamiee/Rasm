# [GEOMETRYCORE_PLANNING]

Rasm.GeometryCore has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns the geometry SUBSTANCE the suite does not yet possess: an exact-predicate-grounded robust geometry kernel that Rhino/GH2 and the Compute mesh layer build ON rather than re-derive. It owns the adaptive-precision predicate floor (orientation/incircle/insphere through filter-then-exact expansion arithmetic), the interior robust numerics that floor depends on, the canonical spatial acceleration index (BVH + linear octree over Morton order), persistent topological naming with stable entity references across rebuilds, the heal/rebuild rail with typed receipts, the geometric constraint solver, and the fabrication frontier — true hidden-line projection substance, CAM toolpath motion, and sheet nesting. It composes `Rasm`/`Vectors` vector, matrix, and mesh primitives as SETTLED vocabulary (read public shapes, compose, NEVER re-mint), emits hash-friendly immutable records the Persistence `GeometryHash` content-addresses, and feeds the AppUi `Viewport2D` hidden-line consumer and the Compute `ClashScale` acceleration consumer at the seam. There are NO admitted external geometry libraries: every kernel is authored from first principles.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                | [OWNS]                                                                                                                          |
| :-----: | :------------------------------------ | :---------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [geometry-kernel](geometry-kernel.md) | Adaptive-precision exact predicates (orientation/incircle/insphere) over expansion arithmetic; the interior robust-numerics floor (filter-then-exact, two-product/two-sum, sign-exact expansions) those predicates ride |
|   [2]   | [spatial-index](spatial-index.md)     | One polymorphic spatial acceleration owner — SAH-BVH + linear (Morton) octree over a unified node store; query union (nearest/range/ray/overlap) and refit/rebuild for the Compute `ClashScale` consumer |
|   [3]   | [topology](topology.md)               | Persistent topological naming — stable brep/mesh entity references (face/edge/vertex lineage) surviving rebuilds; the naming↔hash reconciliation fence mapping naming refs onto Persistence content hashes |
|   [4]   | [healing](healing.md)                 | Heal + rebuild rail — gap/overlap/sliver/non-manifold repair, tolerance-driven welding, degenerate collapse; typed `RebuildReceipt` evidence per heal operation, never a generic ledger |
|   [5]   | [constraints](constraints.md)         | Geometric constraint solver — DOF analysis, decomposition (graph-constructive + Newton fallback), and convergence over coincidence/distance/angle/tangency/parallel/perpendicular constraint rows |
|   [6]   | [fabrication](fabrication.md)         | One polymorphic `Fabrication` frontier owner — a `FrontierKind` SmartEnum (`Project`/`Toolpath`/`Place`) dispatching exact hidden-line projection substance (silhouette + occlusion) feeding AppUi `Viewport2D`, CAM toolpath motion (contour/pocket/drill, feed/retract), and 2D sheet nesting (no-fit-polygon placement) through one `Run` fold; no sibling fabrication surfaces |

## [2]-[WIRE_PAGES]

None. GeometryCore is a pure interior-substance kernel: it carries no TS_PROJECTION cluster, mints no wire vocabulary, and crosses no transport. Its outputs reach other packages only as hash-friendly immutable records consumed at the in-process seam (`COUPLES_TO`).

## [3]-[CATALOGUE_PENDING]

None. The package is AUTHOR-KERNEL with zero admitted external geometry dependency; `Thinktecture.Runtime.Extensions` and `LanguageExt.Core` arrive settled through the stack atlas, and `Rasm`/Vectors arrives as operator-owned source vocabulary composed at the seam, never catalogued here.

## [4]-[GAP_LEDGER]

Every adversarial-verifier gap finding for the package; a row is present only when closed by its `[CLOSED_BY (page#cluster)]` page, so every row is CLOSED.

| [INDEX] | [GAP]                                                       | [CLOSED_BY (page#cluster)]              |
| :-----: | :--------------------------------------------------------- | :-------------------------------------- |
|   [1]   | adaptive-precision orientation/incircle predicate floor    | geometry-kernel#ROBUST_PREDICATES       |
|   [2]   | sign-exact expansion arithmetic (two-product/two-sum) base | geometry-kernel#INTERIOR_NUMERICS       |
|   [3]   | filter-then-exact static/dynamic error-bound stage         | geometry-kernel#INTERIOR_NUMERICS       |
|   [4]   | unified BVH+octree spatial acceleration owner              | spatial-index#SPATIAL_INDEX             |
|   [5]   | Morton-order linear-octree build + SAH-BVH build           | spatial-index#SPATIAL_INDEX             |
|   [6]   | polymorphic query union (nearest/range/ray/overlap)        | spatial-index#SPATIAL_INDEX             |
|   [7]   | refit/rebuild for incremental clash acceleration           | spatial-index#SPATIAL_INDEX             |
|   [8]   | persistent topological naming (stable face/edge refs)      | topology#TOPO_NAMING                    |
|   [9]   | naming-ref lineage survival across rebuild                 | topology#TOPO_NAMING                    |
|  [10]   | naming↔content-hash reconciliation (one identity)          | topology#NAMING_HASH                    |
|  [11]   | canonical hash-friendly immutable topology encoding        | topology#NAMING_HASH                    |
|  [12]   | gap/overlap/sliver/non-manifold heal rail                  | healing#HEALING                         |
|  [13]   | tolerance-driven weld + degenerate collapse                | healing#HEALING                         |
|  [14]   | typed per-operation rebuild receipt (not generic ledger)   | healing#REBUILD_RECEIPTS                |
|  [15]   | geometric constraint solver DOF + decomposition            | constraints#CONSTRAINT_SOLVER           |
|  [16]   | constructive + Newton-fallback convergence                 | constraints#CONSTRAINT_SOLVER           |
|  [17]   | exact hidden-line silhouette + occlusion substance         | fabrication#FRONTIER                    |
|  [18]   | CAM toolpath motion (contour/pocket/drill, feed/retract)   | fabrication#FRONTIER                    |
|  [19]   | 2D sheet nesting (no-fit-polygon placement)                | fabrication#FRONTIER                    |
|  [20]   | one consolidated fault family (band 2400, no exception flow)| faults#FAULT_BAND                       |
|  [21]   | ordinal key-policy comparer per string-keyed axis          | faults#KEY_POLICY                       |

## [5]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface. Dispatch runs over row data through generated total Switches and frozen tables. The package carries one fault family (`GeometryFault`, band 2400) and one key-policy accessor per string-keyed axis (`GeometryKeyPolicy`, ordinal). `[STATE]` is `{PLANNED, FINALIZED, SPIKE}`: `PLANNED` is the pre-authoring charter state every owner carries here (this charter is authored before its pages, so the budget is fixed but the fences are not yet transcribed); `FINALIZED` where the owner is a transcription-complete fence with no open gate; `SPIKE` where the owner is fence-complete but its proof carries a residual numeric-determinism or RID probe named in the page's RESEARCH cluster — a SPIKE owner is fully shaped, never a deferred surface. The pages flip each `PLANNED` cell to `FINALIZED` or `SPIKE` as authored.

The `[RAIL]` cell names the one return rail each owner's entrypoint family exposes — pure verdicts where the result is total, `Fin`/`Validation`/`Eff` where a `GeometryFault` (band 2400) can route, never a parallel error surface.

| [INDEX] | [AXIS/CONCERN]            | [OWNER]               | [KIND]                                                                 | [RAIL]                                       | [CASES] |  [STATE]  |
| :-----: | :------------------------ | :-------------------- | :-------------------------------------------------------------------- | :------------------------------------------- | :-----: | :-------: |
|   [1]   | Exact predicates          | `Predicate`           | static surface + `Orient2D`/`Orient3D`/`InCircle`/`InSphere` arms      | `Predicate.Orient2D → Sign` (pure, total)    |    4    |  PLANNED  |
|   [1a]  | Expansion arithmetic      | `Expansion`           | readonly struct + `TwoProduct`/`TwoSum`/`Grow`/`Estimate`/`Sign` fold  | `Expansion.Estimate → double` (pure)         |    5    |  PLANNED  |
|   [1b]  | Filter stage              | `ErrorBound`          | record rows (static/dynamic permanence) + `Stage` fold                 | `ErrorBound.Stage → Stage` (pure)            |    3    |  PLANNED  |
|   [2]   | Spatial index             | `SpatialIndex`        | [Union] (`Bvh`/`LinearOctree`) + unified node store + `Query` fold     | `SpatialIndex.Build → Fin<SpatialIndex>`     |    2    |  PLANNED  |
|   [2a]  | Spatial query             | `SpatialQuery`        | [Union] (`Nearest`/`Range`/`Ray`/`Overlap`)                            | `SpatialIndex.Query → QueryResult` (pure)    |    4    |  PLANNED  |
|   [3]   | Topological naming         | `TopoName`            | [ValueObject] lineage ref + `NameTable` registry + `Track` fold        | `TopoName.Track → Fin<NameTable>`            |    —    |  PLANNED  |
|   [3a]  | Naming↔hash bridge        | `NamingHash`          | static surface + `Reconcile`/`Encode` (emits `UInt128`-friendly bytes) | `NamingHash.Reconcile → Fin<NamingHash>`     |    2    |  PLANNED  |
|   [4]   | Healing rail              | `Heal`                | static surface + `HealKind` SmartEnum + `RepairPolicy`                 | `Heal.Repair → Fin<RebuildReceipt>`          |    7    |  PLANNED  |
|   [4a]  | Rebuild receipt           | `RebuildReceipt`      | [Union] typed receipt (per heal-kind evidence)                        | carrier (returned in `Heal.Repair` rail)     |    7    |  PLANNED  |
|   [5]   | Constraint solver         | `ConstraintSolver`    | static surface + `Constraint` [Union] + `DofAnalysis`/`Solve` fold     | `ConstraintSolver.Solve → Fin<Solution>`     |    8    |  PLANNED  |
|   [6]   | Fabrication frontier      | `Fabrication`         | static surface + `FrontierKind` SmartEnum (`Project`/`Toolpath`/`Place`) + per-kind `FrontierPolicy` [Union] (`HiddenLine` silhouette/occlude · CAM `ToolpathKind` contour/pocket/drill · `NoFitPolygon` nest) | `Fabrication.Run → Fin<FrontierResult>` ([Union]: `HiddenLineResult`/`Motion`/`Placement`) |    3    |  PLANNED  |
|   [7]   | Fault family              | `GeometryFault`       | [Union] fault, band 2400                                               | the band-2400 carrier every `Fin`/`Validation`/`Eff` routes |    9    |  PLANNED  |
|   [8]   | Key policy                | `GeometryKeyPolicy`   | comparer accessor                                                      | `GeometryKeyPolicy.For → IEqualityComparer` (pure) | ordinal |  PLANNED  |

## [6]-[BUILD_ORDER]

Vocabulary owners first, then the numeric floor the predicates ride, then predicates, the spatial index, naming, healing, constraints, fabrication, faults last consolidated. The non-flat implementation tree, one leaf per file:

```
Rasm.GeometryCore/
├── Numerics/
│   ├── Expansion.cs            # [1a] sign-exact expansion arithmetic (TwoProduct/TwoSum/Grow/Estimate/Sign)
│   └── Predicate.cs            # [1][1b] ErrorBound filter rows; Orient2D/Orient3D/InCircle/InSphere
├── Spatial/
│   └── SpatialIndex.cs         # [2][2a] Bvh/LinearOctree union, node store, SpatialQuery union, Build/Refit/Query
├── Topology/
│   ├── TopoName.cs             # [3] TopoName lineage ref, NameTable registry, Track across rebuild
│   └── NamingHash.cs           # [3a] Reconcile (naming refs ↔ content-hash) + Encode (canonical hash-friendly adjacency encoding); one identity
├── Healing/
│   └── Heal.cs                 # [4][4a] HealKind, RepairPolicy, Heal fold, RebuildReceipt union
├── Constraints/
│   └── ConstraintSolver.cs     # [5] Constraint union, DofAnalysis, constructive+Newton Solve
├── Fabrication/
│   └── Fabrication.cs          # [6] FrontierKind SmartEnum + FrontierPolicy union: HiddenLine silhouette/occlude (Viewport2D substance), CAM contour/pocket/drill motion, NoFitPolygon nest; one Run fold
└── Faults.cs                   # [7][8] GeometryFault union (band 2400), GeometryKeyPolicy
```

## [7]-[RATIFICATIONS]

Two cross-package contradictions are ratified here, decision-complete; each is the sanctioned exception, not an open question.

[R1]-[INTERIOR_NUMERICS_EXCEPTION] — The Compute `units-boundary#QUANTITY_ADMISSION` law (`QuantityFamily.Admit` converts once at admission, forbidding interior `double` arithmetic crossing a signature) gets a NAMED, scoped exception in GeometryCore. The adaptive-precision predicate and healing kernels (`Numerics/Expansion.cs`, `Numerics/Predicate.cs`, and the tolerance-weld interior of `Healing/Heal.cs`) operate on RAW `double` internally — filter-then-exact evaluation, two-product/two-sum splitting, and arbitrary-length floating-point expansions (Shewchuk-style) are the only correct robust formulation and are mathematically defined over IEEE-754 doubles, not unit-carrying quantities. The exception is bounded: raw-double interior numerics live ONLY inside the predicate/expansion/weld kernels; every value crossing a GeometryCore PUBLIC signature is either a sign verdict, a `Rasm`/Vectors typed primitive, or a double that is itself the canonical geometric coordinate at the seam (coordinates are not unit-bearing quantities — they are the domain's native scalar). `Expansion`, `ErrorBound`, and the weld inner loop are the sanctioned interior-double owners; introducing interior doubles anywhere else is the named defect. This exception is the geometry kernel's analogue of a BLAS lane: the exactness substance the rest of the suite consumes through typed seams.

[R2]-[NAMING_HASH_RECONCILIATION] — Persistent topological naming (`Topology/TopoName.cs`) owns STABLE ENTITY REFERENCES across rebuilds (a face/edge/vertex keeps its lineage identity when the brep is re-solved). The Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` (`UInt128` via `XxHash128` over canonical brep face-edge-vertex / mesh half-edge adjacency) stays the ONE content-address node-identity in the Persistence federation; GeometryCore mints NO second hash and NO second identity. The reconciliation is explicit: `TopoName` is a *reference* identity (which entity, lineage-stable), `GeometryHash` is a *content* identity (what shape, change-sensitive), and `Topology/NamingHash.cs` `Reconcile` is the fence mapping naming refs onto content hashes — `Encode` emits the EXACT canonical adjacency bytes the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` content-addresses (so a morph re-hashes, a topology break re-hashes distinctly, a moved-but-unchanged face keeps its `TopoName` and re-derives the same `GeometryHash`). The Compute `interchange#CONTENT_ADDRESSING` `InterchangeIdentity` (`XxHash128` over artifact format + vertices + tolerance policy) is a distinct ARTIFACT-bytes key, NOT an adjacency hash; GeometryCore asserts byte-identity ONLY against the Persistence canonical-adjacency `GeometryHash`, never against the interchange artifact key. GeometryCore emits hash-friendly immutable records; Persistence computes the hash. A second hash function, a parallel node-identity, or a GeometryCore-local content address is the deleted form — the naming ref and the content hash are two orthogonal axes reconciled by one fence, never two competing identities.

## [8]-[COUPLES_TO]

GeometryCore composes at the seam and never re-mints across it.

- `Rasm`/Vectors (SETTLED vocabulary — compose, NEVER modify or re-mint; operator territory): the vector/matrix/mesh primitives (`Matrix`, `VectorFrame`, `Direction`, `MeshSpace`, `IntrinsicMesh`, `SparseMatrix`, and the half-edge/adjacency carriers) are the geometric substrate every GeometryCore owner reads. Predicates take `Rasm`/Vectors coordinates; the spatial index stores `Rasm`/Vectors AABBs; healing and naming operate over `Rasm`/Vectors mesh adjacency. GeometryCore adds a higher-order capability layer (exactness, naming, healing, fabrication) FROM these primitives — never a thinner face over them, never a re-minted vector type.
- Rasm.Compute `solver-and-optimization#CLASH_AND_TWIN` (acceleration consumer): `SpatialIndex` is the canonical BVH/octree the Compute `ClashScale` clash fold consumes for broad-phase clash and clearance acceleration. GeometryCore owns the index substance; the Compute cluster consumes it through the in-process seam and mints no second acceleration structure.
- Rasm.AppUi `drafting-sheets#PROJECTION` (hidden-line consumer): AppUi already owns the 2D painter-depth-sort `HiddenLine`/`ProjectionBasis`/`Viewport2D` drafting frame. GeometryCore owns the exact hidden-line SUBSTANCE — true silhouette extraction over the brep/mesh and analytic occlusion — that `Viewport2D` consumes when it needs CAD-grade hidden-line beyond the painter sort. GeometryCore mints NO second `Viewport2D`, no second sheet frame; it deepens the substance AppUi projects.
- Rasm.Persistence `version-control#STRUCTURAL_DIFF` (the ONE content-address node-identity): GeometryCore emits the canonical hash-friendly immutable adjacency records the Persistence `GeometryHash` content-addresses, and `NamingHash.Reconcile` ([R2]) maps naming refs to those content hashes. GeometryCore computes NO hash and mints NO second identity.

## [9]-[PROOF_GATES]

Assay rows use `uv run python -m tools.assay`; proof runs at the planned phase gate, not after each edit. The package is pure-managed AUTHOR-KERNEL — no native, no server, no wire — so the gate matrix is static + spec + numeric-determinism only.

| [GATE] | [RAIL]                          | [EVIDENCE]                                                          |
| :----: | :------------------------------ | :----------------------------------------------------------------- |
|  [G1]  | `dotnet restore` GeometryCore   | lockfile unchanged; zero NU1004; no geometry-package admission      |
|  [G2]  | `static plan` + `static build`  | routed closure compiles against `Rasm`/Vectors source vocabulary    |
|  [G3]  | `test run` GeometryCore target  | predicate sign-exactness laws hold (perturbed near-degenerate input)|
|  [G4]  | G3 spec rail                    | spatial-index query/refit invariants; naming lineage survives rebuild; heal idempotence; `NamingHash.Encode` byte-identity vs Persistence canonical adjacency proved against a FROZEN golden-bytes fixture both packages assert (the `topology#NAMING_HASH` cluster owns the canonical-adjacency byte-order law; Persistence `StructuralMerge` composes the same encoder, never a parallel serializer) |
|  [G5]  | `docs check` GeometryCore       | Mermaid blocks render through local `mmdc`                          |

## [10]-[PROHIBITIONS]

- [NEVER] add a public surface beside the budgeted owners; a new capability is a row or case on an existing axis.
- [NEVER] admit an external geometry library — ManifoldNET, SpatialMapping.Core, or any GPL/native constraint/CAM/HLR library; the kernels are authored from first principles ([11]).
- [NEVER] re-mint a `Rasm`/Vectors primitive: no GeometryCore-local vector, matrix, frame, or mesh type; compose the operator-owned source and modify it never.
- [NEVER] mint a second content hash or node-identity; the Persistence `version-control#STRUCTURAL_DIFF` `GeometryHash` is the one identity, `NamingHash.Reconcile` is the one bridge ([R2]).
- [NEVER] mint a second `Viewport2D`, sheet frame, painter hidden-line, or `ProjectionBasis`; GeometryCore owns hidden-line SUBSTANCE, AppUi owns the drafting frame ([8]).
- [NEVER] mint a second acceleration structure beside `SpatialIndex`; `ClashScale` consumes it.
- [NEVER] let interior `double` arithmetic escape the [R1] sanctioned kernels (`Expansion`/`Predicate`/weld inner loop); a public-signature interior double outside that scope is the named defect.
- [NEVER] loosen a predicate to pass a near-degenerate case — fix the expansion/error-bound stage; a sign verdict is exact or it is a defect.
- [NEVER] use exception-style control flow in domain logic; faults route through the one `GeometryFault` union (band 2400) and `Fin`/`Validation`/`Eff` rails.
- [NEVER] introduce a generic `IReceipt`/ledger abstraction; `RebuildReceipt` cases stay typed per heal-kind.
- [NEVER] proliferate predicate/query/heal/constraint surfaces into sibling factory methods; one polymorphic owner discriminates by input value or case.

## [11]-[ADMISSIONS_RECORD]

ALL AUTHOR-KERNEL. The package admits NO external geometry dependency; the executed evaluation rejected every candidate and the kernels are authored from first principles. `[STATUS]` in {rejected, author-kernel}.

| [INDEX] | [PACKAGE/CANDIDATE]                     | [CONCERN]                          | [VERDICT]    | [STATUS]      |
| :-----: | :-------------------------------------- | :--------------------------------- | :----------- | :------------ |
|   [1]   | ManifoldNET                             | boolean/mesh manifold ops          | alpha-only — unstable API, no robustness guarantee | rejected |
|   [2]   | SpatialMapping.Core                     | spatial acceleration index         | pre-1.0 — unstable, half-built query surface        | rejected |
|   [3]   | GPL constraint solvers (PlaneGCS class) | geometric constraint solving       | GPL license — incompatible with suite distribution  | rejected |
|   [4]   | GPL/native CAM libraries                | toolpath motion generation         | GPL or native-only — license + RID burden           | rejected |
|   [5]   | GPL/native HLR libraries                | hidden-line removal                | GPL or native-only — license + RID burden           | rejected |
|   [6]   | Shewchuk adaptive predicates (authored) | exact orientation/incircle/insphere | first-principles author — the only correct robust floor | author-kernel |
|   [7]   | SAH-BVH + Morton linear octree (authored) | spatial acceleration              | first-principles author — one polymorphic index owner    | author-kernel |
|   [8]   | Persistent topological naming (authored)  | stable entity refs across rebuild | first-principles author — reconciled to `GeometryHash`   | author-kernel |

`Thinktecture.Runtime.Extensions` (`[Union]`/`[SmartEnum]`/`[ValueObject]`) and `LanguageExt.Core` (`Fin`/`Validation`/`Eff`) arrive settled through the stack atlas; `Rasm`/Vectors arrives as operator-owned source vocabulary. Versions live in `Directory.Packages.props`; this table carries no pin.

## [12]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/csharp/.planning/campaign-method.md` then `TASKLOG.md` then this charter. Folder-specific deepening targets: the predicate floor widened from the four base predicates (`Orient2D`/`Orient3D`/`InCircle`/`InSphere`) to the full robust-CGAL-class set (`ParallelOrder`, `CompareDistance`, segment-segment intersection sign, Delaunay-flip predicate) as fold arms on `Predicate`, each riding the same `Expansion`/`ErrorBound` stage; the spatial index deepened from broad-phase queries to a self-balancing refit cadence and a GPU-friendly node layout for the `ClashScale` hot path; persistent naming widened to carry full brep-history lineage (split/merge/fillet provenance) so a downstream rebuild re-binds references through topological operations, not just survival; healing widened to a full B-rep sewing + face-rebuild rail; the constraint solver deepened from the eight base constraint rows to the full 2D/3D parametric-sketch constraint set with witness-configuration disambiguation; the fabrication frontier deepened — hidden-line to analytic curved-surface silhouette (not just facet), CAM to 5-axis motion + collision-aware retract, nesting to true irregular-shape no-fit-polygon with rotation search. The bar: Rhino/GH2 and the Compute mesh layer build every geometry operation ON this kernel's exact substance, with naming refs and content hashes reconciled at every rebuild and no second identity, acceleration, or hidden-line owner anywhere in the suite.
