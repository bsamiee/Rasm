# [RASM_FABRICATION]

`Rasm.Fabrication` is the host-neutral portable-fabrication owner over the `Rasm` kernel. The polymorphic `Fabrication` owner closes the 3D-to-fabrication concern over a `FabricationPolicy` `[Union]` discriminant folded by one `Run` generated total `Switch`, spanning exact hidden-line projection, CAM toolpath motion with serial-chain kinematics, 2D true-shape nesting, and a portable cut-program emitter — all over a shared 2D polygon-algebra floor. The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[OWNER](.planning/Process/owner.md): polymorphic `Fabrication` owner — `FabricationPolicy`/`FabricationResult` unions, shared `Loop`/`Edge3`/`Move`/`PartTransform` atoms, one `Run` fold.
- [02]-[FAMILY](.planning/Process/family.md): `Process`/`Machine` axis pair — removal-modality/kinematic-class/post-dialect discriminant covering mill/turn/route/laser/plasma/waterjet/additive/oxyfuel/edm-wire.
- [03]-[CLIPPER](.planning/Polygon/clipper.md): `Clipper2` polygon-algebra substrate — offset/inflate, Boolean clip, Minkowski sum, open-path screen clip.
- [04]-[IMPORT](.planning/Polygon/import.md): portable 2D profile-ingress boundary — DXF/DWG closed-polyline and arc entities tessellated into the canonical `Loop` via `ACadSharp`, host-neutral and coexisting with Rhino-native I/O.
- [05]-[PROJECTION](.planning/Posting/projection.md): HLR — BSP front-to-back visibility and `Clipper2` open-path-Boolean screen clip; world-space edge sets for the `AppUi` `Viewport2D`.
- [06]-[MOTION](.planning/Toolpath/motion.md): CAM motion — `(Process, ToolpathKind)` move family (milling contour/pocket/drill/trochoidal, lathe turn/face/groove/thread, thermal contour, additive slice-layer) over the `Geometry2D` offset.
- [07]-[SKELETON](.planning/Toolpath/skeleton.md): straight-skeleton/medial-axis author-kernel driving trochoidal adaptive clearing.
- [08]-[SLICING](.planning/Toolpath/slicing.md): FFF/DED slicing author-kernel — planar-section layer contours, perimeter shells, and hatch-clip infill over the `Geometry2D` substrate.
- [09]-[KINEMATICS](.planning/Toolpath/kinematics.md): DH forward kinematics and the damped-least-squares Jacobian-pseudoinverse IK solver.
- [10]-[NFP](.planning/Nesting/nfp.md): 2D true-shape nesting — NFP feasibility, bottom-left/genetic placement, and the `Stock` union (sheet/plate/bar/tube/billet/filament/remnant) content-keyed feasibility set.
- [11]-[PROGRAM](.planning/Posting/program.md): host-neutral cut-program emission — dialect-neutral G-code AST plus `PostDialect` family (linuxcnc/grbl/fanuc/marlin/hypertherm), kerf-comp, lead-in/out, pierce, micro-tab, cut-sequencing.
- [12]-[WORKHOLDING](.planning/Nesting/workholding.md): `Workholder` keep-out family (clamp/vise/chuck/vacuum-table/magnet/sacrificial-bed) conditioning the toolpath and cut sequence against fixture geometry.
- [13]-[PHYSICS](.planning/Process/physics.md): removal-physics table projecting `process × material × tool × operation` to the modality-discriminated `RemovalBudget` (subtractive/thermal/abrasive/additive) the toolpath generators read.
- [14]-[FAULTS](.planning/Process/faults.md): band-2500 `FabricationFault` cases composing the kernel band-2400 `GeometryFault`.

## [02]-[DOMAIN_PACKAGES]

Domain libraries owned outside the C# substrate registry. Versions are centralized in the one C# manifest and corroborated by this folder's `.api/`.

[POLYGON_ALGEBRA]:
- `Clipper2`

[CAD_IMPORT]:
- `ACadSharp` — the SOLE read-side CAD owner (DXF + DWG AC1014-AC1032, block traversal, the native `Spline`/`Arc`/`Circle` sampler). DXF/DWG WRITE is an `Rasm.AppUi`/Render drafting concern, not a Fabrication rail; this folder admits the read surface only.

[RECT_PACKING]:
- `RectpackSharp`

[ARC_FIT]:
- `geometry4Sharp`

[QUANTITY_INGRESS]:
- `UnitsNet`

[CONTENT_IDENTITY]:
- `System.IO.Hashing` — the Nesting content-identity owner: the `Remnant`/`Stock` `XxHash128` content address and the `NoFitPolygon.PairKey` precompute-memo digest. No cross-kernel `NAMING_HASH` owner consumes it; the federation hash is wired wholly in-folder.

[REJECTED]:
- `netDxf` — present in the central manifest as an `Rasm.AppUi` DXF-write dependency, NOT a Fabrication rail. Rejected as a second DXF reader: DXF-only (no DWG, no AC1014-AC1032 spread), no managed `Spline`/bulge sampler parity with `ACadSharp`. No sibling kernel opens a `netDxf` reader beside `ProfileImport`.
- `MaxRect`, `BinPack.NET` — rejected rectangle packers: AABB-only, cannot express true-shape NFP feasibility, superseded by `RectpackSharp` as the axis-aligned fast-path arm.

## [03]-[SUBSTRATE_PACKAGES]

Substrate cards this folder consumes from the registry. Full substrate law and package charters live in `libs/csharp/.planning/README.md`; decompile evidence for domain packages lives in this folder's `.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[TEST_SUBSTRATE]:
- `xunit.v3.core`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `SharpFuzz`
- `Verify.XunitV3`
