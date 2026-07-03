# [RASM_FABRICATION]

`Rasm.Fabrication` is the host-neutral portable-fabrication owner over the `Rasm` kernel. The polymorphic `Fabrication` owner closes the 3D-to-fabrication concern over a `FabricationPolicy` `[Union]` discriminant folded by one `Run` generated total `Switch`, spanning exact hidden-line projection, CAM toolpath motion with serial-chain kinematics, 2D true-shape nesting, and a portable cut-program emitter — all over a shared 2D polygon-algebra floor. The domain map and forward work live in `ARCHITECTURE.md`, `IDEAS.md`, and `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[OWNER](.planning/Process/owner.md): polymorphic `Fabrication` owner — `FabricationPolicy`/`FabricationResult` unions, shared `Loop`/`Edge3`/`Move`/`PartTransform` atoms, one `Run` fold.
- [02]-[FAMILY](.planning/Process/family.md): `Process`/`Machine` axis pair — removal-modality/kinematic-class/post-dialect discriminant covering mill/turn/route/laser/plasma/waterjet/additive/oxyfuel/edm-wire.
- [03]-[CLIPPER](.planning/Polygon/clipper.md): `Clipper2` polygon-algebra substrate — offset/inflate, Boolean clip, Minkowski sum, open-path screen clip.
- [04]-[IMPORT](.planning/Polygon/import.md): portable 2D profile-ingress boundary — DXF/DWG closed-polyline and arc entities tessellated into the canonical `Loop` via `ACadSharp`, host-neutral and coexisting with Rhino-native I/O.
- [05]-[PROJECTION](.planning/Posting/projection.md): HLR — BSP front-to-back visibility and `Clipper2` open-path-Boolean screen clip; world-space edge sets for the `AppUi` `Viewport2D`.
- [06]-[MOTION](.planning/Toolpath/motion.md): CAM motion — `(RemovalModality, CutStrategy)` cross-product move family (boundary-pass/pocket-clear/peck/adaptive/radial-sweep/plunge-dwell/helical/layer-walk, the modality enveloping each strategy) over the `Geometry2D` offset, the arc-capable `Move` carrying its `ArcCenter` for the posting biarc refit.
- [07]-[SKELETON](.planning/Toolpath/skeleton.md): straight-skeleton/medial-axis author-kernel driving trochoidal adaptive clearing.
- [08]-[SLICING](.planning/Toolpath/slicing.md): FFF/DED slicing author-kernel — planar-section layer contours, perimeter shells, and hatch-clip infill over the `Geometry2D` substrate.
- [09]-[KINEMATICS](.planning/Toolpath/kinematics.md): articulated-robot-cell solver — `RobotProgram` drives a serial-chain `Move` stream through the admitted `Robots` cell (FK/IK, joint-limit/singularity/reach validation, cell-dialect post), superseding the hand-rolled DH/Jacobian-IK sketch.
- [10]-[NFP](.planning/Nesting/nfp.md): 2D true-shape nesting — NFP feasibility plus the inner-fit-polygon dual, bottom-left/genetic/`RectpackSharp` rect-fastpath placement over a `Seq<Stock>` multi-sheet inventory, and the kerf-inflated Boolean-difference `Remnant` lineage producer.
- [11]-[PROGRAM](.planning/Posting/program.md): host-neutral cut-program emission — dialect-neutral G-code AST plus the `Process/family` `PostDialect` family with the independent dialect-override seam, kerf-comp, lead-in/out, pierce, micro-tab, `geometry3Sharp` `g3.BiArcFit2` biarc arc-fit, jerk-limited look-ahead feedrate, cut-sequencing.
- [12]-[WORKHOLDING](.planning/Nesting/workholding.md): `Workholder` keep-out family (clamp/vise/chuck/vacuum-table/magnet/sacrificial-bed) conditioning the toolpath and cut sequence against fixture geometry.
- [13]-[PHYSICS](.planning/Process/physics.md): removal-physics table projecting `process × material × tool × operation` to the modality-discriminated `RemovalBudget` (subtractive/thermal/abrasive/additive) the toolpath generators read.
- [14]-[FAULTS](.planning/Process/faults.md): `FabricationFault` cases on the registry band `FaultBand.Fabrication` (2700, offset-derived codes) composing the kernel band-2400 `GeometryFault`.
- [15]-[GUARD](.planning/Toolpath/guard.md): swept tool-plus-holder collision/gouge guard — `SweptVolume` over the `Geometry2D` Minkowski substrate, the `Verdict` union (clear/gouge/collision/clearance), and the collision-aware safe-Z lift `Cam.Solve` consults per feed move.
- [16]-[MAGAZINE](.planning/Process/magazine.md): tool-magazine — `Magazine` carousel/turret/manual slot map, per-slot `ToolAssembly` holder geometry, and the minimal-swap tool-change `Schedule` the posting `G43`/`M6` emits.
- [17]-[STOCK](.planning/Nesting/stock.md): rectangular cutting-stock yield engine — one `StockNest.Pack` fold over the `NestStrategy` `[Union]` collapsing the five `RectangleBinPack.CSharp` packers (max-rects/skyline/guillotine/shelf/mass-cut plus the heuristic sweep) into the `NestPlan`/`NestYield` per-sheet sheet-yield receipt the `Nest.Honor` fold consumes on `FabricationInput.Plan`.

## [02]-[DOMAIN_PACKAGES]

Domain libraries owned outside the C# substrate registry. Versions are centralized in the one C# manifest and corroborated by this folder's `.api/`.

[POLYGON_ALGEBRA]:
- `Clipper2`

[CAD_IMPORT]:
- `ACadSharp` — consumes the host-neutral DWG/DXF read codec (DXF + DWG AC1014-AC1032, block traversal, the native `Spline`/`Arc`/`Circle` sampler) at the `Rasm.Bim`-owned interchange seam: `Rasm.Bim` is the AEC-DOMAIN CAD-interchange owner of the `CadDocument`/`DxfDocument` read surface, and Fabrication references the same central pin directly for 2D profile ingress (the Fabrication<-Bim ACadSharp read seam in `ARCHITECTURE [02]-[SEAMS]`). DXF/DWG WRITE is the distinct `Rasm.AppUi`/Render drafting leg over `netDxf`, not a Fabrication rail; this folder admits the read surface only.

[STEEL_FABRICATION_EXCHANGE]:
- `DSTV.Net` — DSTV/NC1 steel fabrication exchange for profile-cut programs, saw/drill/punch data, and portable shop-machine handoff beside the neutral G-code AST.

[RECT_PACKING]:
- `RectpackSharp`
- `RectangleBinPack.CSharp` — the academic axis-aligned cutting-stock suite (`MaxRectsBinPack`/`SkylineBinPack`/`GuillotineBinPack`/`ShelfBinPack`/`SingleBinPack`, each over its own heuristic enum) the `Nesting/stock` `StockNest.Pack` yield engine drives — the material-planning YIELD concern (minimum sheets, per-sheet offcuts, waste evidence), DISJOINT from `RectpackSharp` (the one-shot `FindBest` fast-path arm of the true-shape CAM nest); neither re-packs the other. Pure-managed AnyCPU single-TFM netstandard2.0, MIT, zero dependencies; assembly id `RectangleBinPacking`.

[ARC_FIT]:
- `geometry3Sharp` — the SOLE biarc/curve-fit owner, the SAME gradientspace package the `Rasm.Bim` mesh-text importer already admits centrally; this folder reuses it scoped to the `g3.BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d` curve surface, the `DMesh3`/mesh-boolean half firewalled. No second `geometry4Sharp` fork is admitted beside it.

[ROBOT_KINEMATICS]:
- `Robots` (visose) — the SOLE host-neutral serial-chain robot-kinematics owner, superseding the hand-rolled DH-FK + damped-least-squares Jacobian IK in `Toolpath/kinematics.md`. Owns per-mechanism DH/Modified-DH forward kinematics, closed-form ANALYTIC IK (`SphericalWristKinematics`/`OffsetWristKinematics`) discriminated by `RobotConfigurations`, a `NumericalKinematics` iterative fallback, explicit joint-limit/singularity/reach-envelope validation stamped into a `KinematicSolution`, multi-mechanism groups (`MechanicalGroupKinematics`) plus external-axis (`TrackKinematics`/`PositionerKinematics`) kinematics, and the ABB/KUKA/UR/Staubli/Franka/Doosan/Fanuc/Igus/Jaka cell models. The `Robots` core binds `Rhino3dm` (its own `Rhino.Geometry.*` identity); plan-cs boundary-maps at the kinematics seam — consume via `Transform`/`double[]` joint vectors and `KinematicSolution`, never pass a RhinoCommon geometry type into the `Rhino3dm` API. Transitive floors `Rhino3dm` (osx-arm64 native `librhino3dm_native.dylib`), `SSH.NET` (the robot-program SFTP upload path), and `BouncyCastle.Cryptography` are centrally floor-pinned.

[ARC_OFFSET]:
- `CavalierContours` (oberbichler) — the arc-native (bulge) 2D polyline offset/Boolean owner: parallel offset, closed-polyline union/intersection/difference/XOR, containment/winding, and a `StaticAABB2DIndex` spatial index over open, closed, AND self-intersecting polylines whose vertices carry `bulge = tan(theta/4)`. The arc-space substrate the lead-arc and morphed-spiral adaptive-clearing rails consume to generate kerf/lead arcs directly, orthogonal to the line-only `Clipper2` Boolean/offset substrate (not a dup — `Clipper2` cannot express constant-radius arc segments natively). The posting `g3.BiArcFit2` biarc-refit stays the distinct chord-stream arc-RECOVERY-at-emission owner; the two concerns (arc-native offsetting versus chord-run arc recovery) do not overlap. Pure-managed AnyCPU, ISC.

[VORONOI_TESSELLATION]:
- `SharpVoronoiLib` (RudyTheDev) — the dedicated 2D Fortune's-algorithm Voronoi owner with edge clipping, border closure, and Lloyd's relaxation (plus the Delaunay dual): the CAM tessellation primitive behind Voronoi-based toolpath region partitioning, spiral-pocket seed centroids, even-spacing region decomposition, and Lloyd-relaxed stipple/engrave/pen-plot paths. Point-site only — it does NOT compute the polygon medial axis / segment-Voronoi, so the straight-skeleton concern stays in the in-folder `Toolpath/skeleton` author-kernel. Supersedes the stale `VoronoiLib` 0.1.0 predecessor. Pure-managed AnyCPU, MIT.

[IMPLICIT_VOXEL]:
- `PicoGK` (LEAP71) — the implicit/SDF/voxel geometry kernel for additive manufacturing: `IImplicit.fSignedDistance` -> `Voxels(IImplicit, BBox3)` TPMS/gyroid/cellular infill the rectilinear/honeycomb quartet cannot express, `Lattice.AddBeam`/`AddSphere` -> `Voxels(Lattice)` overhang-conditioned support scaffolds, `GetVoxelSlice`/`GetInterpolatedVoxelSlice` + `CliIo`/`Vdb2Cli` SLA/DLP/MSLA grayscale layer-stack rasterization (the resin/powder path the planar-only FFF `Section` never reaches), 3D SDF Boolean (`BoolAdd`/`BoolSubtract`/`BoolIntersect`) + `Offset`/`DoubleOffset`/`OverOffset`/`Fillet`, and `OpenVdbFile` I/O. Composes — does NOT replace — the planar mesh-section (`geometry3Sharp` `MeshPlaneCut`) and leaves the `Clipper2` 2D perimeter offsetting untouched. COMPANION / OUTSIDE-RHINO: `lib/net9.0`-only plus the bundled `runtimes/osx-arm64/native/picogk.dylib` (OpenVDB/boost/TBB-backed) firebreaks it out of any net48 in-Rhino plugin ALC; its sole dependency `SkiaSharp` rides the existing `App UI` 3.119.4 row. Apache-2.0.

[TOOL_DATA_MODEL]:
- `MTConnect.NET-Common` (TrakHound) — the ISO-13399-aligned MTConnect cutting-tool asset model (`MTConnect.Assets.CuttingTools`: `CuttingToolAsset`/`CuttingToolLifeCycle`/`CuttingItem`/`Measurement` with the ISO-13399-derived cutting-diameter/corner-radius/edge-angle/flute-length/tool-life subtypes, `ToolLife`, `ProgramToolNumber`/`ProgramToolGroup`, `CutterStatus`, `ReconditionCount`) backing the tool-data MODEL half of the `Magazine`/`ToolAssembly` catalogue. The lean model-only sub-package (no HTTP/MQTT/SHDR/Agent machinery). The feeds/speeds numeric cutting-DATA half stays a recorded gap — no free-full feeds/speeds dataset is published on NuGet. Pure-managed AnyCPU net9.0, MIT; the single managed transitive floor `YamlDotNet` >=13.7.1 is centrally floor-pinned.

[SOLID_INGRESS]:
- `OcctNet.Wrapper` — the first managed 3D solid-CAD / B-rep manufacturing-geometry ingress in Fabrication beyond the 2D `ACadSharp` DXF/DWG path: `ImportStep` (AP203/AP214/AP242) / `ImportIges` -> an `OcctShape` B-rep (`OcctFace`/`OcctEdge`/`OcctWire`/`OuterWire` topology) tessellated through `OcctMesh`, with `ExportStep`/`ExportIges`/`ExportStl`, primitive modeling (box/cylinder/sphere/edge/face/wire), boolean fuse/cut/common, extrude/revolve, translate, and bounding-box over OCCT 7.9.3. Ships `lib/net10.0` plus a real `runtimes/osx-arm64/native` OCCT 7.9.3 build (dynamic-link, LGPL-2.1-with-OCCT-exception-1.0 clean); wrapper MIT. SCOPE: single-shape import only — it does NOT surface TKHLR hidden-line-removal (the `.dylib` ships unbound) nor TKXCAF assembly/color/PMI, so the HLR projection stays in `Posting/projection`. plan-cs boundary-maps at the `OcctShape`/`OcctMesh` seam, never passing a kernel geometry type into the OCCT ABI.

[REJECTED]:
- `netDxf` — present in the central manifest as an `Rasm.AppUi` DXF-write dependency, NOT a Fabrication rail. Rejected as a second DXF reader: DXF-only (no DWG, no AC1014-AC1032 spread), no managed `Spline`/bulge sampler parity with `ACadSharp`. No sibling kernel opens a `netDxf` reader beside `ProfileImport`.
- `MaxRect`, `BinPack.NET` — rejected rectangle packers: AABB-only, cannot express true-shape NFP feasibility, superseded by `RectpackSharp` as the axis-aligned fast-path arm.
- `geometry4Sharp` — the NewWheelTech fork of gradientspace `geometry3Sharp`. Rejected as a SECOND mesh/curve package: the central manifest already pins `geometry3Sharp` (the `Rasm.Bim` mesh-text importer owner), and `g3.BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d` exist identically in the already-admitted `geometry3Sharp`, so the biarc rail composes the pinned package, never a duplicate heavy fork. The `g3` namespace and the entire biarc surface are shared between fork and base.

## [03]-[SUBSTRATE_PACKAGES]

Substrate cards this folder consumes from the registry. Full substrate law and package charters live in `libs/csharp/.planning/README.md`; shared API evidence lives in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `System.IO.Hashing`

[NUMERIC_SUBSTRATE]:
- `UnitsNet`

[TEST_SUBSTRATE]:
- `xunit.v3.core`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `Verify.XunitV3`
