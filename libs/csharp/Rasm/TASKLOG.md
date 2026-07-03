# [RASM_TASKLOG]

The kernel's open and closed work is distilled from ideas and design-page RESEARCH residuals. Each task is a card whose leader carries `[ID]-[STATUS]: thesis`, followed by `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` bullets.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[HOST_BOUNDARY_REENTRY]-[BLOCKED]: the host-boundary packages re-enter the solution build surface when kernel realization lands.
- Capability: Restore `Rasm.Rhino`, `Rasm.Grasshopper`, and their test shells to `Workspace.slnx` once compiled `Rasm.Domain`/`Rasm.Vectors`/`Rasm.Analysis` types exist again.
- Shape: Four slnx folder/project rows return — `/libs/csharp/Rasm.Rhino/`, `/libs/csharp/Rasm.Grasshopper/`, `/tests/csharp/libs/Rasm.Rhino/`, `/tests/csharp/libs/Rasm.Grasshopper/`; the frozen host source binds the frozen-name contracts (`VectorIntent`/`VectorFrame`/`MotionInterpolation`, `Analyze`/`AnalysisQuery`/`Env`, `Op` + the `[Union]` generator emit).
- Unlocks: Host-boundary compile, the rhino-bridge runtime scenarios over kernel capability, and future app-root composition.
- Anchors: `Processing/intent#DISPATCH`, `Analysis/query#OPERATION_RUNTIME`, `Domain/rails#GENERATOR_CONTRACTS`, the frozen-name law in the kernel index docs, `tools/cs-analyzer` `UnionOpsGenerator` emit.
- Tension: Blocked on realization, never on planning — no design page waits on this card.

[T-BOOLEAN-NATIVE-ASSET]-[BLOCKED]: native arrangement asset remains a scale-only gate after the managed exact boolean path lands.
- Capability: Admit a robust exact-arithmetic mesh-arrangement native asset only for boolean workloads beyond the managed arrangement scale ceiling.
- Shape: `HealOp.Boolean` composes `Arrangement.Apply(ArrangementKind.MeshBoolean, ...)` for common managed cases; `NativeAssetMissing` stays reserved for the future tier-3 scale route.
- Unlocks: Large boolean and CSG repair workloads can gain a chartered native acceleration path without blocking managed correctness, planar overlay, or ordinary mesh-boolean repair.
- Anchors: `Processing/repair#BOOLEAN_NATIVE_ASSET`, `Meshing/arrangement#ARRANGEMENT`, `Arrangement.Apply`, exact `Predicate` crossings, constrained `Tessellation`, `SpatialQuery.Winding`, and `DuplicateWeld`.
- Tension: Native admission is still an external dependency with RID burden and post-condition proof; it is a charter amendment for scale, not a second correctness owner.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[ARRANGEMENT_REALIZED_OWNER_LANDING]-[COMPLETE]: `Meshing/arrangement` page authored and its `[04]-[CROSS_PAGE_SEAMS]` records the kernel-side consume-when-realized contract — `Apply(ArrangementKind.MeshBoolean, …)` welds the kept patches into a manifold solid and `ToMesh` re-emits the `MeshSpace` watertight outline the Fabrication `Hlr.Solve` reads through the `MeshSpace`/`Arrangement` union-value seams, never coupling into `ArrangementStore`; ARCHITECTURE `[02]-[SEAMS]` carries the `Meshing/Arrangement.cs → csharp:Rasm.Fabrication/Posting/projection` wire; Ripple: `Rasm.Fabrication` `[CSG_WATERTIGHT_SILHOUETTE]`.

[KERNEL_PLANNING_CONVERSION]-[COMPLETE]: the kernel converted to an ordinary planning-scoped package — one `.planning/` root, eight sub-domain folders, 52 pages (18 settled Geometry pages re-homed path-only, 34 new pages re-expressing the retired `Vectors`/`Domain`/`Analysis` source ground-up under the frozen `Rasm.Domain`/`Rasm.Vectors`/`Rasm.Analysis` contract namespaces, zero capability loss); retired source archived at `.archive/Rasm`; roster delta landed (`Triangle` pin dropped repo-wide, `geometry3Sharp` + `MathNet.Numerics.Providers.MKL` kernel references dropped with pins retained for sibling consumers); the host-boundary slnx rows left the build surface per `[HOST_BOUNDARY_REENTRY]`; index docs rebuilt for the new root.
