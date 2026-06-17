# [FABRICATION_ARCHITECTURE]

`Rasm.Fabrication` is one fabrication frontier: every concern is a `FrontierKind` case, every result a `FrontierResult` case, every toolpath a `ToolpathKind` row — never a per-concern projector/post/packer class. Mechanics live in the `.planning/` pages; this page is the atlas — the implementation source tree (the build order), the owner registry (the one owner-state surface), dependency direction, cross-package seams, the boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The fabrication-frontier layout IS the build order: the shared frontier owner and the hidden-line kernel first (the frontier owner the CAM and nesting kernels compose), then the CAM motion kernel, then the nesting kernel, the consolidated fault family last. Each leaf is annotated with the owning `<SubDomain>/<page>#CLUSTER`; sub-folders group the flat file set by concern axis.

```text codemap
Rasm.Fabrication/
├── Projection/
│   └── Fabrication.cs            # FrontierKind, FrontierPolicy/FrontierResult unions, Loop/Edge3/Move atoms, FabricationKeyPolicy, one Run table-fold; Hlr BSP-visibility + Weiler-Atherton clip — hidden-line#FABRICATION_OWNER, #PROJECTION_HIDDEN_LINE
├── Cam/
│   └── Toolpath.cs               # DhJoint, IkPolicy, PartTransform, Cam ToolpathKind generators, Fk forward kinematics, Ik damped-pseudoinverse solver — toolpath#CAM_MOTION
├── Nesting/
│   └── Nesting.cs                # SheetBounds, NestPolicy, NoFitPolygon author-kernel, Nest bottom-left/genetic placement fold — nesting#NESTING
└── Faults.cs                     # FabricationFault union (band 2400), FabricationKeyPolicy ordinal accessor — consolidated last
```

`Projection/Fabrication.cs` lands first because the shared `FrontierKind`/`FrontierPolicy`/`FrontierResult`/`Loop`/`Move` vocabulary and the `Run` dispatch are read by every kernel — `Cam/Toolpath.cs` composes the `FrontierPolicy.Cam`/`FrontierResult.Motion` shapes and `Nesting/Nesting.cs` the `FrontierPolicy.Nest`/`FrontierResult.Placement` shapes plus the `PartTransform` the CAM page owns. `Hlr` composes the kernel predicates and the index. `Faults.cs` consolidates the one fabrication fault band-2400 family and the one `FabricationKeyPolicy` last.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the package. Implementation collapses to one owner per axis and one entrypoint family per rail; a new frontier is a `FrontierKind` row + `FrontierPolicy`/`FrontierResult` case, a new toolpath a `ToolpathKind` row, a new placement heuristic a `NestPolicy` column — never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual native/cross-page probe named in the page RESEARCH cluster; a SPIKE owner is fully shaped now, never a deferred surface. This is the ONLY place owner state lives — FEATURES, TASKLOG, and README route here.

| [INDEX] | [AXIS/RAIL]              | [OWNER]               | [KIND]                                                                                                | [CASES]                          | [PAGE#CLUSTER]                       |  [STATE]  |
| :-----: | :---------------------- | :-------------------- | :--------------------------------------------------------------------------------------------------- | :------------------------------- | :----------------------------------- | :-------: |
|   [1]   | fabrication frontier    | `Fabrication`         | static surface + `FrontierKind` SmartEnum + `FrontierPolicy`/`FrontierResult` unions + one `Run` fold | 3 (Project/Toolpath/Place)       | hidden-line#FABRICATION_OWNER        | FINALIZED |
|   [2]   | hidden-line / projection | `Hlr`                | BSP visibility solver + Weiler-Atherton edge clip over `Predicate.Orient2D`                          | visible/hidden/silhouette        | hidden-line#PROJECTION_HIDDEN_LINE   | FINALIZED |
|   [3]   | CAM / motion            | `Cam`/`Fk`/`Ik`       | `ToolpathKind` offset/spiral/drill + DH forward kinematics + damped-pseudoinverse IK                 | 3 toolpath rows                  | toolpath#CAM_MOTION                  | FINALIZED |
|   [4]   | nesting                 | `Nest`/`NoFitPolygon` | author-kernel no-fit-polygon + bottom-left/genetic placement fold                                    | 2 (bottom-left/genetic)          | nesting#NESTING                      | FINALIZED |
|   [5]   | fault family            | `FabricationFault`    | `[Union]` fault, band 2400                                                                           | consolidated                     | (Faults.cs)                          | FINALIZED |
|   [6]   | key policy              | `FabricationKeyPolicy` | ordinal comparer accessor                                                                           | ordinal                          | (Faults.cs)                          | FINALIZED |

One rail per entrypoint, named in the return type: a `Fin<FrontierResult>` where a band-2400 `FabricationFault` can route (degenerate input, open loop, no-fit), the result union where the verdict is total. The interior coordinate doubles inside every kernel are the sanctioned native-scalar scope ([5]-[BOUNDARIES]).

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PROJECT]          | [MAY_REFERENCE_FABRICATION] | [FABRICATION_MAY_REFERENCE] | [BOUNDARY]                                          |
| :-----: | :----------------- | :-------------------------: | :-------------------------: | :------------------------------------------------- |
|   [1]   | `Rasm`             |            no               |        yes (compose)        | composes the geometry kernel — `Predicate.Orient2D`, `SpatialIndex`, `Matrix`/`Point3d`/`Vector3d` |
|   [2]   | `Rasm.AppUi`       |            yes              |        yes (seam)           | `Viewport2D` consumes the hidden-line edge sets BELOW its painter sort |
|   [3]   | host packages      |       library compose       |            no               | RhinoCommon geometry composed through the `Rasm`/Vectors substrate |

`Rasm.Fabrication` is an AEC-domain package over the kernel: it references `Rasm` and adds the fabrication ALGORITHM atop the kernel's exact-geometry and acceleration substance, never a thinner face over it. The AppUi `Viewport2D` is the planned consumer of the hidden-line edge sets — a consumption seam where AppUi reads and Fabrication produces, the projection-to-sheet owned by AppUi.

## [4]-[SEAMS]

Every two-package fact splits by altitude: mechanics live at the named `Fabrication` cluster, consequences land at the consumer. Intra-language seams ride `pkg/page#CLUSTER`; the cross-language consequences ride the Tier-0 `region-map/seam-splits.md`.

| [INDEX] | [SEAM]                | [MECHANICS_AT]                    | [CONSEQUENCE_AT]                                                              |
| :-----: | :-------------------- | :-------------------------------- | :--------------------------------------------------------------------------- |
|   [1]   | hidden-line substance | hidden-line#PROJECTION_HIDDEN_LINE | exact silhouette/occlusion consumed by `csharp:AppUi/drafting-sheets#PROJECTION` `Viewport2D` |

## [5]-[BOUNDARIES]

- `Rasm.Fabrication` is a portable fabrication-algorithm package: not a geometry kernel, runtime spine, UI package, or host-boundary package. It owns the fabrication frontier algorithm; the kernel owns the predicate floor and the acceleration substance, AppUi owns the projection-to-sheet.
- The fabrication kernels admit NO external HLR/CAM/nesting library (no GPL/native HLR/CAM, no UNesting, no ManifoldNET); every kernel is authored from first principles. The geometry-kernel predicate floor and the spatial index are composed, never re-minted.
- The fabrication frontier mints NO second `Viewport2D`/`ProjectionBasis`/painter hidden-line, NO second acceleration structure, and NO re-minted predicate; the `Predicate.Orient2D` exact sign is the one orientation floor, the `SpatialIndex` the one broad-phase.
- RATIFIED NATIVE-SCALAR SCOPE (settled law): the interior coordinate doubles inside every fabrication kernel are the sanctioned native-scalar posture — a coordinate is the domain's native scalar, not a unit-bearing quantity. A unit-bearing quantity in a kernel signature is the named seam violation.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER a public type outside the OWNER_REGISTRY owner regions; a new capability is a `FrontierKind` row, a `FrontierPolicy`/`FrontierResult` case, a `ToolpathKind` row, or a `NestPolicy` column, never a sibling surface.
- NEVER an external HLR/CAM/nesting library admission; the hidden-line, CAM-motion, and nesting kernels are author-kernel.
- NEVER a per-concern `HlrProjector`/`CamPost`/`NestPacker` sibling-class triple; the three concerns differ only in their kernel fold, never in their entrypoint — `Run` dispatches by `FrontierPolicy` case over the `Builders` table.
- NEVER a re-minted predicate, acceleration structure, `Viewport2D`, `ProjectionBasis`, or painter hidden-line; the kernel deepens the substance app packages project and consume.
- NEVER a hand-rolled 4×4 matrix re-mint or a `double` cross-product sign at a call site where `Rasm`/Vectors `Matrix` and the exact `Predicate.Orient2D` own the operation.
- NEVER a per-arm analytic IK family beside the one damped-least-squares pseudoinverse fold; an analytic closed form is a fast-path row, never a parallel solver.
- NEVER an interior `double` escaping the sanctioned native-scalar kernel scope as a unit-bearing quantity in a public signature.
- NEVER exception-style control flow in domain logic; faults route through the one `FabricationFault` union (band 2400) and `Fin`/`Validation`/`Eff` rails.
- NEVER a generic `IReceipt`/ledger abstraction; `FrontierResult` (`HiddenLineResult`/`Motion`/`Placement`) stays typed per kind.
- Analyzer diagnostics are architecture pressure: fix the shape, refine the rule on a false positive, never suppress.
