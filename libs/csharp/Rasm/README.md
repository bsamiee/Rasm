# [RASM]

`Rasm` is the geometry and numeric kernel below the app packages: it owns the typed vector geometry layer (`Vectors`), the shared Rhino-normalization and statistics domain (`Domain`), the analysis algebra (`Analysis`), and the robust-geometry domain under `Geometry/` — exact-predicate-grounded geometry the rest of the suite builds ON rather than re-derives. The robust-geometry pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The domain owns the adaptive-precision predicate floor, the interior robust numerics it rides, the canonical spatial acceleration index, persistent topological naming reconciled to the Persistence content hash, the heal/rebuild rail with typed receipts, and the geometric constraint solver; it composes `Rasm.Vectors` as settled operator vocabulary and never re-mints a primitive. The portable fabrication frontier (hidden-line substance, CAM motion, sheet nesting) lives in the AEC-domain `Rasm.Fabrication` package that composes this kernel. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                  | [OWNS]                                                                                                                                              |
| :-----: | :----------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [geometry-kernel](Geometry/.planning/geometry-kernel.md) | Adaptive-precision exact predicates (orientation/incircle/insphere) over expansion arithmetic; the filter-then-exact interior robust-numerics floor |
|   [2]   | [topology](Geometry/.planning/topology.md)               | Persistent topological naming — stable face/edge/vertex lineage across rebuilds; the naming↔content-hash reconciliation fence                       |
|   [3]   | [spatial-index](Geometry/.planning/spatial-index.md)     | One polymorphic spatial acceleration owner — SAH-BVH + Morton linear octree over one node store; query union, refit, Compute seam                   |
|   [4]   | [healing](Geometry/.planning/healing.md)                 | Heal + rebuild rail — gap/overlap/sliver/non-manifold repair, tolerance weld, degenerate collapse; typed per-op `RebuildReceipt`                    |
|   [5]   | [constraints](Geometry/.planning/constraints.md)         | Geometric constraint solver — DOF analysis, analytic residual/Jacobian algebra, author-kernel Levenberg-Marquardt convergence                       |

## [2]-[ADMISSIONS_RECORD]

The admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. The robust-geometry domain admits NO external geometry library — every kernel is author-kernel; the numeric-lane packages below back the constraint solver's dense linear algebra and the existing `Vectors`/`Analysis` substrate. `[STATUS]` is `catalogue-pending` until the `.api` catalogue lands in the tooling pass; the `[CATALOGUE]` cell holds `—` until then.

| [INDEX] | [PACKAGE]               | [PAGE]          | [CATALOGUE] | [STATUS]          |
| :-----: | :---------------------- | :-------------- | :---------: | :---------------- |
|   [1]   | CSparse                 | constraints     |      —      | catalogue-pending |
|   [2]   | MathNet.Numerics        | constraints     |      —      | catalogue-pending |
|   [3]   | MathNet.Symbolics       | constraints     |      —      | catalogue-pending |
|   [4]   | System.Numerics.Tensors | geometry-kernel |      —      | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]                        | [EVIDENCE]                                                                                             |
| :-----: | :-------------------- | :---------------------------- | :----------------------------------------------------------------------------------------------------- |
|  [G1]   | locked restore        | Assay restore rail            | clean closure; unchanged `packages.lock.json`                                                          |
|  [G2]   | API catalogue resolve | `assay api` doctor/resolve    | fence members resolve in `.api` or doctrine pages                                                      |
|  [G3]   | static plan + build   | Assay static rail             | routed closure against `Vectors` source vocabulary, zero `': error '` lines                            |
|  [G4]   | spec law-matrix       | Assay test rail (Rasm target) | predicate sign-exactness, index invariants, naming lineage, heal idempotence, LM convergence laws pass |
|  [G5]   | page diagram render   | local mermaid-cli             | page diagrams render through the local renderer                                                        |
