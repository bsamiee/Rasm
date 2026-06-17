# [FABRICATION]

`Rasm.Fabrication` is the suite's portable fabrication frontier: the polymorphic `Fabrication` owner that closes the 3D-to-fabrication concern over a `FrontierKind` discriminant folded by one `Run` data-table dispatch — exact hidden-line projection, CAM toolpath motion, and 2D true-shape nesting, each an author-kernel because no admitted library carries a CAM/HLR/nesting robustness or license guarantee. The `.planning/` pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns the BSP-visibility + Weiler-Atherton hidden-line removal producing world-space edge sets the AppUi `Viewport2D` consumes, the `ToolpathKind` offset/spiral/drill motion over Denavit-Hartenberg forward kinematics and damped Jacobian-pseudoinverse IK, and the no-fit-polygon nesting with bottom-left + genetic placement; it composes the `Rasm` geometry kernel's `Predicate.Orient2D` exact-orientation floor and `SpatialIndex` broad-phase as settled vocabulary and never re-mints a predicate, an acceleration structure, or a `Viewport2D`. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                            | [OWNS]                                                                                                                          |
| :-----: | :------------------------------------------------ | :---------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | [hidden-line](Projection/.planning/hidden-line.md) | One polymorphic `Fabrication` owner (`FrontierKind`/`FrontierPolicy`/`FrontierResult` + `Run` fold); BSP-visibility + Weiler-Atherton hidden-line edge sets for AppUi `Viewport2D` |
|   [2]   | [toolpath](Cam/.planning/toolpath.md)             | `ToolpathKind` contour-offset/pocket-spiral/drill motion; Denavit-Hartenberg forward kinematics; damped Jacobian-pseudoinverse IK |
|   [3]   | [nesting](Nesting/.planning/nesting.md)           | Author-kernel no-fit-polygon; bottom-left + genetic placement fold over `Predicate.Orient2D` feasibility |

## [2]-[ADMISSIONS_RECORD]

The admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. The fabrication kernels admit NO external HLR/CAM/nesting library — every kernel is author-kernel; the `Rasm` project reference carries the geometry-kernel predicate floor, the spatial index, and the `Matrix`/`Point3d`/`Vector3d` substrate. `[STATUS]` is `catalogue-pending` until the `.api` catalogue lands; the `[CATALOGUE]` cell holds `—` until then; author-kernel concerns carry no package row.

| [INDEX] | [PACKAGE] | [PAGE]      | [CATALOGUE] | [STATUS]          |
| :-----: | :-------- | :---------- | :---------: | :---------------- |
|   [1]   | Rasm      | hidden-line |      —      | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]                                | [EVIDENCE]                                                                                  |
| :-----: | :-------------------- | :------------------------------------ | :----------------------------------------------------------------------------------------- |
|  [G1]   | locked restore        | Assay restore rail                    | clean closure; unchanged `packages.lock.json`                                              |
|  [G2]   | API catalogue resolve | `assay api` doctor/resolve            | fence members resolve in `.api` or doctrine pages                                          |
|  [G3]   | static plan + build   | Assay static rail                     | routed closure against the `Rasm` kernel vocabulary, zero `': error '` lines               |
|  [G4]   | spec law-matrix       | Assay test rail (Fabrication target)  | hidden-line clip exactness, IK convergence through singularities, NFP placement laws pass  |
|  [G5]   | page diagram render   | local mermaid-cli                     | page diagrams render through the local renderer                                            |
