# [RASM_FABRICATION]

`Rasm.Fabrication` is the host-neutral portable-fabrication frontier over the `Rasm` kernel: the polymorphic `Fabrication` owner closes the 3D-to-fabrication concern over a `FrontierPolicy` `[Union]` discriminant folded by one `Run` generated total `Switch` — exact hidden-line projection, CAM toolpath motion plus serial-chain kinematics, 2D true-shape nesting, and a portable cut-program emitter, all over a shared 2D polygon-algebra floor. The professional domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/` mirror the eventual source tree, one page per source file.

- [frontier/owner](.planning/frontier/owner.md): the polymorphic `Fabrication` owner — `FrontierPolicy`/`FrontierResult` unions, the shared `Loop`/`Edge3`/`Move`/`PartTransform` atoms, one `Run` fold.
- [process/family](.planning/process/family.md): the `Process`/`Machine` axis pair — the removal-modality/kinematic-class/post-dialect discriminant every fabrication owner reads (mill/turn/route/laser/plasma/waterjet/additive/oxyfuel/edm-wire).
- [geometry2d/clipper](.planning/geometry2d/clipper.md): the one Clipper2 polygon-algebra substrate — offset/inflate, Boolean clip, Minkowski sum, open-path screen clip.
- [geometry2d/profile-import](.planning/geometry2d/profile-import.md): the portable 2D profile-ingress boundary — DXF/DWG closed-polyline and arc entities tessellated into the canonical `Loop` through the `ACadSharp` reader, host-neutral and coexisting with Rhino-native I/O.
- [projection/hidden-line](.planning/projection/hidden-line.md): HLR — BSP front-to-back visibility and Clipper2 open-path-Boolean screen clip; world-space edge sets for the AppUi `Viewport2D`.
- [toolpath/motion](.planning/toolpath/motion.md): CAM motion — the `(Process, ToolpathKind)` move family (milling contour/pocket/drill/trochoidal, lathe turn/face/groove/thread, thermal contour, additive slice-layer) over the Geometry2D offset.
- [toolpath/skeleton](.planning/toolpath/skeleton.md): the straight-skeleton/medial-axis author-kernel driving trochoidal adaptive clearing.
- [additive/slicing](.planning/additive/slicing.md): the FFF/DED slicing author-kernel — planar-section layer contours, perimeter shells, and hatch-clip infill over the Geometry2D substrate.
- [kinematics/serial-chain](.planning/kinematics/serial-chain.md): DH forward kinematics and the one damped-least-squares Jacobian-pseudoinverse IK solver.
- [nesting/nfp](.planning/nesting/nfp.md): 2D true-shape nesting — NFP feasibility, bottom-left/genetic placement, and the `Stock` union (sheet/plate/bar/tube/billet/filament/remnant) content-keyed feasibility set.
- [posting/program](.planning/posting/program.md): host-neutral portable cut-program emission — the dialect-neutral G-code AST plus the `PostDialect` family (linuxcnc/grbl/fanuc/marlin/hypertherm), kerf-comp, lead-in/out, pierce, micro-tab, cut-sequencing.
- [fixturing/workholding](.planning/fixturing/workholding.md): the `Workholder` keep-out family (clamp/vise/chuck/vacuum-table/magnet/sacrificial-bed) conditioning the toolpath and the cut sequence against fixture geometry.
- [process-physics/cut-parameter](.planning/process-physics/cut-parameter.md): the removal-physics table projecting the `process × material × tool × operation` row to the modality-discriminated `RemovalBudget` (subtractive/thermal/abrasive/additive) the toolpath generators read.
- [faults/faults](.planning/faults/faults.md): the band-2500 `FabricationFault` cases composing the kernel band-2400 `GeometryFault`.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions are centralized in the one C# manifest.

- Rasm
- Clipper2
- ACadSharp
- Thinktecture.Runtime.Extensions
- LanguageExt.Core
- System.IO.Hashing
