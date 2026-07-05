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
- `ACadSharp` — DWG/DXF read at the `Rasm.Bim`-owned interchange seam, referenced directly for 2D profile ingress; DXF/DWG WRITE is the AppUi two-format ACadSharp drafting leg, never a Fabrication arm.

[STEEL_FABRICATION_EXCHANGE]:
- `DSTV.Net` — DSTV/NC1 steel fabrication exchange for profile-cut programs, saw/drill/punch data, and shop-machine handoff beside the neutral G-code AST.

[RECT_PACKING]:
- `RectangleBinPack.CSharp` — axis-aligned cutting-stock suite behind `Nesting/stock` `StockNest.Pack` (the five packers max-rects/skyline/guillotine/shelf/mass-cut plus the heuristic sweep); ALSO owns the `Nesting/nfp` rectangle fast-path via `MaxRectsBinPack`, absorbing the retired `RectpackSharp` try-every-heuristic sweep as one `MaxRectsBinPack.Insert` fold.

[ARC_FIT]:
- `geometry3Sharp` — the SOLE biarc/curve-fit owner, scoped to `g3.BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d`; the mesh-boolean half stays firewalled.

[ROBOT_KINEMATICS]:
- `Robots` — visose serial-chain kinematics owner: DH FK, analytic+numerical IK, `KinematicSolution`, external axes; never RhinoCommon into `Rhino3dm`.

[ARC_OFFSET]:
- `CavalierContours` — arc-native bulge polyline offset/Boolean owner with `StaticAABB2DIndex`; orthogonal to line-only `Clipper2` and the `g3.BiArcFit2` refit.

[VORONOI_TESSELLATION]:
- `SharpVoronoiLib` — 2D Fortune Voronoi: clipping, border closure, Lloyd relaxation, Delaunay dual; point-site only — medial axis stays `Toolpath/skeleton`.

[SURFACE_ENGINE]:
- `OpenCAMLib` — analytic 3-axis cutter-location engine behind `Toolpath/surface`: drop-cutter Z-sampling, push-cutter fibers, waterline Z-level loops over arbitrary `MillingCutter` forms; consumed through an `extern "C"` C-shim over the SHARED `libocl` (LGPL-2.1 dynamic-link), path layout stays kernel on-mesh.

[IMPLICIT_VOXEL]:
- `PicoGK` — LEAP71 implicit/SDF/voxel kernel: TPMS/lattice infill, SLA/DLP layer rasterization, SDF Booleans, `OpenVdbFile`; companion-only, never in-Rhino.

[ADDITIVE_3MF]:
- `lib3mf` — 3MF Consortium reference reader/WRITER behind `Additive/production`: core/production/beam-lattice/slice egress; the PicoGK `Lattice` beam/node set maps directly onto `CBeamLattice`. Vendored ACT-generated binding + RID-keyed native (BSD-2), no NuGet.

[TOOL_DATA_MODEL]:
- `MTConnect.NET-Common` — TrakHound ISO-13399 cutting-tool model behind the `Magazine`/`ToolAssembly` catalogue; feeds/speeds data stays a recorded gap.

[SOLID_INGRESS]:
- `OcctNet.Wrapper` — managed STEP/IGES B-rep ingress to `OcctShape`/`OcctMesh`; single-shape import only, no TKHLR/TKXCAF — HLR stays `Posting/projection`.

[REJECTED]:
- `netDxf` — an `Rasm.AppUi` DXF-write dependency, not a Fabrication rail; rejected as a second reader (DXF-only, no `Spline`/bulge parity with `ACadSharp`) and purged repo-wide — the drafting WRITE leg is the AppUi two-format ACadSharp leg.
- `RectpackSharp` — removed as redundant: a strict subset of `RectangleBinPack.CSharp` (`PackingHints.FindBest` = the try-every-heuristic sweep over one `MaxRectsBinPack.Insert` fold); the `Nesting/nfp` fast-path re-homes to `RectangleBinPack.CSharp` `MaxRectsBinPack`.
- `MaxRect`, `BinPack.NET` — rejected rectangle packers: AABB-only, no true-shape NFP feasibility; superseded by the `RectangleBinPack.CSharp` `MaxRectsBinPack` fast-path arm.
- `geometry4Sharp` — rejected second mesh/curve fork: the pinned `geometry3Sharp` carries the identical `g3` biarc surface; the rail composes the pin.

## [03]-[SUBSTRATE_PACKAGES]

Substrate cards this folder consumes from the registry. Full substrate law and package charters live in `libs/csharp/.planning/README.md`; shared API evidence lives in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `System.IO.Hashing` — reached ONLY through the kernel `ContentHash.Of` single mint; every egress content key seeds there.
- `NodaTime` — `Instant` stamps on traveler documents, probing receipts, tool-life schedules, and SPC control points.

[NUMERIC_SUBSTRATE]:
- `UnitsNet` — cut-parameter boundary + `Spec/tolerance` quantities (Speed/Length/RotationalSpeed/Pressure + Force/Power/Temperature/Angle/Torque overlay).
- `MathNet.Numerics` — `Spec/capability` distribution-fit + Monte-Carlo tolerance-stackup (first Fabrication consumer; the streaming-moment half stays kernel `Domain/stats`).

[SIMD_SPANS]:
- `System.Numerics.Tensors` — SIMD-lower the hot sampling folds (engagement, scallop, NFP-sweep, capability-batch) across `Toolpath/surface`+`motion`, `Nesting/nfp`, `Spec/capability`.
- `CommunityToolkit.HighPerformance` — `Span2D`/`Memory2D` 2D grids for grayscale/uncut/engagement rasters across `Additive/implicit`, `Verify/removal`, `Toolpath/surface`.

[MAPPING]:
- `Riok.Mapperly` — zero-reflection source-generated projections: DSTV-record→`Loop`, MTConnect asset rows, and the `extern alias R3` Rhino3dm boundary seam (`Kinematics/cell`, `Tooling/magazine`, `Verify/probing`, `Ingress/steel`).

[GRAPH]:
- `QuikGraph` — the `Fixturing/setups` operation-precedence + datum-lineage graph (the magazine-eviction greedy stays hand-rolled).

[TEST_SUBSTRATE]:
- `xunit.v3.core`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `Verify.XunitV3`
