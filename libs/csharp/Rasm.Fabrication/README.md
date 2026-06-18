# [RASM_FABRICATION]

`Rasm.Fabrication` is the host-neutral portable-fabrication frontier over the `Rasm` kernel: the polymorphic `Fabrication` owner closes the 3D-to-fabrication concern over a `FrontierPolicy` `[Union]` discriminant folded by one `Run` generated total `Switch` — exact hidden-line projection, CAM toolpath motion plus serial-chain kinematics, 2D true-shape nesting, and a portable cut-program emitter, all over a shared 2D polygon-algebra floor. The professional domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/` mirror the eventual source tree, one page per source file.

- [frontier/owner](.planning/frontier/owner.md): the polymorphic `Fabrication` owner — `FrontierPolicy`/`FrontierResult` unions, the shared `Loop`/`Edge3`/`Move`/`PartTransform` atoms, one `Run` fold.
- [geometry2d/clipper](.planning/geometry2d/clipper.md): the one Clipper2 polygon-algebra substrate — offset/inflate, Boolean clip, Minkowski sum, open-path screen clip.
- [projection/hidden-line](.planning/projection/hidden-line.md): HLR — BSP front-to-back visibility and Clipper2 open-path-Boolean screen clip; world-space edge sets for the AppUi `Viewport2D`.
- [toolpath/motion](.planning/toolpath/motion.md): CAM motion — `ToolpathKind` contour/pocket/drill/trochoidal over the Geometry2D offset.
- [toolpath/skeleton](.planning/toolpath/skeleton.md): the straight-skeleton/medial-axis author-kernel driving trochoidal adaptive clearing.
- [kinematics/serial-chain](.planning/kinematics/serial-chain.md): DH forward kinematics and the one damped-least-squares Jacobian-pseudoinverse IK solver.
- [nesting/nfp](.planning/nesting/nfp.md): 2D true-shape nesting — NFP feasibility and bottom-left/genetic placement.
- [posting/program](.planning/posting/program.md): host-neutral portable cut-program emission — RS-274/ISO-6983 G-code, kerf-comp, lead-in/out, micro-tab, cut-sequencing.
- [faults/faults](.planning/faults/faults.md): the band-2500 `FabricationFault` cases composing the kernel band-2400 `GeometryFault`.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions are centralized in the one C# manifest.

- Rasm
- Clipper2
- Thinktecture.Runtime.Extensions
- LanguageExt.Core
