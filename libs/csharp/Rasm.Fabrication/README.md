# [RASM_FABRICATION]

`Rasm.Fabrication` is the host-neutral AEC-DOMAIN production-fabrication engine over the `Rasm` kernel — fifteen folders ↔ fifteen namespaces, bijective. The polymorphic `Fabrication` owner closes the whole concern over the 10-case `FabricationPolicy` `[Union]` folded by one `Run` generated total `Switch` (Cam · HiddenLine · Nest · Additive · Verify · Inspect · Post · Document · Derive · Form); every flagship terminates in a content-keyed machine-consumable artifact through the thirteen-row `EgressKind` + `ContentKey.Of` egress spine. The domain map, seam ledger, and fault registry live in `ARCHITECTURE.md`; forward work in `IDEAS.md` and `TASKLOG.md`. Rows [01]-[33] mirror the DECISION roster; rows [34]-[59] are the widened-map pages; all fifty-nine pages are on disk.

## [01]-[ROUTER]

- [01]-[OWNER](.planning/Process/owner.md): owner#atoms vocabulary (`Loop`+`Bulges`/`Edge3`/`Move`/`PartTransform`/`ProjectionDir`/`CutterForm`/`AdmittedComponent`/`PlannedStep`/`BendStep`/`ResidualStock`/`StockSnapshot`/`CapabilityVerdict`/`EgressKind`/`ContentKey`) + the 10-case `Run` entry (owner#run).
- [02]-[FAMILY](.planning/Process/family.md): `ProcessKind` (11) / `Machine` (14) axes, the `ProcessModality` superset (7 rows under `ModalityClass` {removal, additive, formed, joined}), `CutStrategy` + dimensionality, `KinematicClass` rotary topology, and the `PostDialect` grammar-capability table.
- [03]-[PHYSICS](.planning/Process/physics.md): ONE `Material` identity carrying `Map<ProcessModality, ModalityPhysics>` → the nine-case `RemovalBudget` (subtractive/thermal/abrasive/FFF/DED/erosion/resin/powder/formed; `Deposition` doubles as the joined-key weld heat-input budget).
- [04]-[FAULTS](.planning/Process/faults.md): the band-2700 registry — frozen retyped 2701-2710 + folder-grouped growth 2711-2729 + 2730-2746; next free 2747.
- [05]-[MAGAZINE](.planning/Tooling/magazine.md): MTConnect ISO-13399 `ToolAssembly` + the life-split minimal-swap `Schedule` wired to the posting `Tnn`/`M6`/`G43` blocks.
- [06]-[CUTTINGDATA](.planning/Tooling/cuttingdata.md): Kienzle `kc` machinability — ISO 513 class seeds, CSV/QIF data ingress, `CutterForm.Of(ToolAssembly)` projection.
- [07]-[ALGEBRA](.planning/Geometry2D/algebra.md): Clipper2 line-space substrate — offset, variable-delta offset, Boolean/open-path clip, signed `Area`, Minkowski sum/diff, containment-depth `NestingOrder` (the arc-space owner is `arcs`).
- [08]-[ARCS](.planning/Geometry2D/arcs.md): CavalierContours arc-space owner — `ArcAlgebra` kerf/lead/adaptive arc rails over kernel K1 region offsets; `BoolKind` set-op axis; the bulged-`Loop`↔`Polyline<double>` boundary seam.
- [09]-[PROFILE](.planning/Ingress/profile.md): the DXF/DWG profile arm of the ONE polymorphic `Ingress.Admit` fold via `ACadSharp`; hosts the 4-case `IngressSource` union.
- [10]-[SOLID](.planning/Ingress/solid.md): OcctNet STEP/IGES/STL B-rep ingress → triangulate → kernel `MeshSpace` admission (dirty STL through `HealOp`); the `libTKHLR`/`libTKXCAF` scope-limit demand row stands.
- [11]-[STEEL](.planning/Ingress/steel.md): DSTV NC1 steel-exchange read arm — `SteelImport.Read` record tree → `Loop` via Mapperly projection; `KA` bend seeds feed `Forming/sheet`; NC1 EMIT stays `Posting/dialect`.
- [12]-[MOTION](.planning/Toolpath/motion.md): `(ProcessModality, CutStrategy)` generator arms; the Cam fold emits the conditioned `Motion`; the Turn arm composes `Turning.Generate`; SliceWalk carries seam/comb/monotonic rows.
- [13]-[SURFACE](.planning/Toolpath/surface.md): OpenCAMLib cutter positioning (`extern "C"` shim + `[LibraryImport]` over shared `libocl`) + kernel K10-K13 on-mesh path layout — waterline/scallop/pencil/rest/3+2/swarf rows; `SurfacePath.Sample`.
- [14]-[PARTITION](.planning/Toolpath/partition.md): SharpVoronoiLib Fortune/Lloyd region-decomposition CAM lane — `Partition.Seed` pocket/stipple/engrave/pen-plot strategy rows, point-site only.
- [15]-[GUARD](.planning/Toolpath/guard.md): swept tool-plus-holder gouge/collision guard consulted per committed feed move; retract routing re-pointed to `Link.Route`.
- [16]-[SKELETON](.planning/Toolpath/skeleton.md): trochoidal constant-engagement WALK over the kernel clearance family.
- [17]-[CELL](.planning/Kinematics/cell.md): `Robots` serial-chain cell solve — per-manufacturer posts, `extern alias R3` boundary.
- [18]-[MACHINE](.planning/Kinematics/machine.md): 5-axis rotary-topology inverse + TCP/RTCP admission over `KinematicClass`; `MachineTool.Solve`; homes the ONE `MotionDynamics` law cell and posting `Lookahead` read.
- [19]-[SLICING](.planning/Additive/slicing.md): FFF/DED planar slicing — `InfillPattern` + shells with the Arachne bead law over the K1 medial RADIUS and the density-field infill policy row (kernel `SliceStack` consumer).
- [20]-[IMPLICIT](.planning/Additive/implicit.md): PicoGK implicit/voxel lanes — `Implicit.Voxelize` TPMS/gyroid/cellular conformal infill, `Lattice` scaffolds, grayscale/`.cli` egress; declares ONCE the ALC-firebreak + mesh↔`Voxels` wire.
- [21]-[PRODUCTION](.planning/Additive/production.md): `Production.Plan` build orientation on the K20 face-decomposition objective + machine-profile rows + lib3mf 3MF core/production/beam-lattice egress.
- [22]-[NFP](.planning/Nesting/nfp.md): NFP-feasibility true-shape nesting over the `Seq<Stock>` inventory, `MaxRectsBinPack` rect-fastpath, kerf-difference `Remnant` lineage (the mint half — the lifecycle half is `remnant`).
- [23]-[STOCK](.planning/Nesting/stock.md): rectangular cutting-stock yield — one `StockNest.Pack` fold over the `NestStrategy` `[Union]`, the `NestPlan`/`NestYield` receipt `Nest.Honor` consumes.
- [24]-[WORKHOLDING](.planning/Fixturing/workholding.md): `Clamp`/`ExclusionZone` keep-out family + the `Condition` fold the Cam conditioning composes.
- [25]-[SETUPS](.planning/Fixturing/setups.md): QuikGraph multi-fixture setup scheduler — `Setup.Schedule` precedence/datum-lineage graph; OWNS the setup→WCS assignment rows posting renders; op-N admission against the op-N-1 `StockSnapshot`.
- [26]-[PROGRAM](.planning/Posting/program.md): dialect-neutral `CutProgram` AST + cut conditioning (incl. the cooling Lookahead-sibling pass) + content key; the `Run(Post)` case assembles the AST from the atoms `Motion`; `Parse` round-trip.
- [27]-[DIALECT](.planning/Posting/dialect.md): per-dialect `Dialect.Emit` generated total `Switch` over the `PostDialect` grammar family — canned-cycle/macro/subprogram lowering, code-override resolution, the NC1 emit target.
- [28]-[REMOVAL](.planning/Verify/removal.md): `Removal.Verify` PicoGK voxel material-removal — gouge/uncut/overcut/air-cut receipts; PRODUCES `ResidualStock` + content-keyed per-setup `StockSnapshot`s.
- [29]-[PROBING](.planning/Verify/probing.md): `Probe.Inspect` in-process metrology — G31/G38 cycle rows on the posting AST, kernel K16 ICP datum best-fit + K26 substitute datums, `ConformanceMetric` verdicts, QIF → capability.
- [30]-[TOLERANCE](.planning/Spec/tolerance.md): typed GD&T vocabulary — ISO 286 generated fit rows, ISO 1101/ASME Y14.5 frames + datums, ISO 1302 texture; `ScallopStep`/`Allowance` spec-drives-process derivation rows.
- [31]-[CAPABILITY](.planning/Spec/capability.md): `Capability.Assess` Cp/Cpk/Pp/Ppk over kernel `Stat.Of` + MathNet fit/Monte-Carlo stackup; SPC rows; the plan-time `Capability.Gate` producing `CapabilityVerdict`.
- [32]-[PROJECTION](.planning/Documentation/projection.md): HLR emission (kernel `DrawingProjection` consumer) — the `HiddenLineResult` receipt AppUi is insulated at.
- [33]-[TRAVELER](.planning/Documentation/traveler.md): content-keyed setup sheets + shop travelers composed in the `Run(Document)` case body — the widest fan-in receipt validator; MODEL only.
- [34]-[DERIVATION](.planning/Process/derivation.md): the `Run(Derive)` orchestrator — `Derivation.Plan` stage rail (manufacturability → routing → fleet → setup/assembly → programs → documentation), `DerivePolicy`/`DerivationStage`, the realized `FabricationProjector : IElementProjection` registration.
- [35]-[WEAR](.planning/Tooling/wear.md): Taylor VB flank-wear + condition-based RUL over decoded MTConnect telemetry; cross-`ModalityClass` consumable state; `ToolWear.Assess` → `WearState` feeding `magazine.Schedule` + estimation.
- [36]-[CURVES](.planning/Geometry2D/curves.md): the parametric third owner — `CurveAlgebra.Apply` over kernel exact offsets/stations/divide/refit + `Fill` + NURBS refit/frames; feeds turning, 2.5D pocket-profile, posting stations, 5-axis smoothing.
- [37]-[ELEMENT](.planning/Ingress/element.md): the `ElementGraph.Bake` arm of `Ingress.Admit` → `AdmittedComponent` (representation key, composition layers, connection rows, `DemandKey`-pinned quantity/property bags).
- [38]-[TURNING](.planning/Toolpath/turning.md): `LatheOp` union + `Turning.Generate` — G71/G72/G73 pass folds, ISO thread degression, 9-quadrant nose comp, Css/G97 spindle modes, K14 revolved envelope.
- [39]-[WIRE](.planning/Toolpath/wire.md): wire-EDM cycle union — compound skim offsets, guide-plane taper + corner modes, corner slowdown + wire lag, slug/tab retention; the `Erosion` budget's consumer.
- [40]-[LINK](.planning/Toolpath/link.md): rapid-travel minimization — `Link.Route` QuikGraph tour + A*/Dijkstra obstacle-routed retracts over guard clearance + kernel `SpatialIndex`.
- [41]-[BEVEL](.planning/Toolpath/bevel.md): `BevelType` I/V/A/Y/K/X per-edge coupled condition rows; waterjet twin-tilt taper-lag; the ONE THC/Z-law custodian; groove-prep feeds `Joining/weld`.
- [42]-[FLEET](.planning/Kinematics/fleet.md): the machine-capability registry — `Fleet.Capable(AdmittedComponent, MachineFleet)` six-check join → ranked `Seq<MachineMatch>`; instance DATA over the family `Machine` axis.
- [43]-[SCANPATH](.planning/Additive/scanpath.md): LPBF hatch union + 66.7° rotation recurrence + direction×ordering sorter axes + up/down/in-skin partition; `scan-vectors` egress off the `Powder` budget.
- [44]-[SUPPORT](.planning/Additive/support.md): planar layer-diff overhang census + interface carve; TREE influence-area walk with avoidance-state cache; bridge anchor state machine; voxel/lattice lane stays `implicit`.
- [45]-[REMNANT](.planning/Nesting/remnant.md): the offcut lifecycle partial — `Remnant.Reconcile(Seq<Remnant>, RemnantInventory) → RemnantPlan` batch seam + the Claim/Release/Sweep op overload, `RemnantState` transitions, `ReusePolicy` admission, `Stockable` re-mint into the next `Inventory`.
- [46]-[LINKING](.planning/Nesting/linking.md): `LinkOp` union (CommonLine/ChainCut/Bridge/SkeletonCutUp) — collision-checked graph edits over the `Placement`; one pierce/lead per chain into program.
- [47]-[ASSEMBLY](.planning/Fixturing/assembly.md): join-precedence planning — `Assembly.Sequence` → `AssemblyPlan`; `JoinClass` classification; QuikGraph DAG gate + topological order + transitive reduction.
- [48]-[OPTIMIZATION](.planning/Posting/optimization.md): MRR-adaptive feedrate + HSM corner smoothing + block-cap compaction over the `CutProgram` AST via the internal `Lookahead` re-sweep.
- [49]-[ESTIMATION](.planning/Verify/estimation.md): `Estimate.Of(FabricationResult)` → `CostReceipt` — simulate clock, nest waste, wear consumables, remnant credit, fleet `HourlyRate` rates; receipts only.
- [50]-[AUDIT](.planning/Verify/audit.md): additive layer-stack pre-flight — `Span2D` connected components, `LayerDefect` union, cross-layer lineage; receipts only.
- [51]-[SIMULATE](.planning/Verify/simulate.md): `Simulate.Execute` — the modal-state execution walk over the parsed `CutProgram`, per-block accel-limited time, envelope/overtravel verdicts; the authoritative cycle-time owner.
- [52]-[MANUFACTURABILITY](.planning/Spec/manufacturability.md): cross-`ModalityClass` DfM verdicts on ONE surface — `Manufacturability.Assess` → `DfmReport` (with the ranked `Routing` derivation feed); receipts only.
- [53]-[REPORT](.planning/Documentation/report.md): FAI/mill-cert/weld-inspection as-built `QualityRecord` union — measured features + Cp/Cpk + conformance rows, content-keyed; MODEL only.
- [54]-[SHEET](.planning/Forming/sheet.md): the ONE unfold owner — `FlatPattern.Unfold` as the BA-substitution overlay over kernel `Development.Apply`; K-factor table/coupon/DIN-6935; relief/hem unions; `FormPolicy` mints here.
- [55]-[BRAKE](.planning/Forming/brake.md): `BendSequence.Plan` best-first over the reach/collision/occlusion matrix; V=f·T die rows; air-bend tonnage; `BendMethod` air/bottoming/coining; springback overbend into `BendStep`.
- [56]-[TUBE](.planning/Forming/tube.md): XYZ→YBC/LRA centerline fold + elongation carry + CLR/mandrel admission + analytic cope development; research-gated row (bender-format breadth precedes the deep interior).
- [57]-[WELD](.planning/Joining/weld.md): joint×prep over the Materials-owned groove vocabulary; bead-stack fill fold; `HI = η·60·V·I/(1000·v)` off the joined-key `Deposition` budget; torch `Motion` through Cam.
- [58]-[SEQUENCE](.planning/Joining/sequence.md): distortion ordering (Backstep/SkipWeld/Balanced/Block) + tack plan + interpass scheduling; permutes only within `AssemblyPlan.Precedence`.
- [59]-[PROCEDURE](.planning/Joining/procedure.md): WPS/PQR essential-variable rows (`EssentialVariable` mints here) + the heat-input compliance gate; QIF-aligned.

## [02]-[DOMAIN_PACKAGES]

Domain libraries owned outside the C# substrate registry. NuGet versions are centralized in the one C# manifest and corroborated by this folder's `.api/`; `lib3mf` and `OpenCAMLib` are vendored — their pins and RID-keyed natives ride this folder (`vendor/runtimes/**`, their `.api` catalogs), outside NuGet restore.

[POLYGON_ALGEBRA]:
- `Clipper2`

[CAD_IMPORT]:
- `ACadSharp` — DWG/DXF 2D-profile read projection over the shared `CadDocument` model (Bim projects the mesh-bearing entities); DXF/DWG WRITE is the AppUi drafting leg, never a Fabrication arm.

[STEEL_FABRICATION_EXCHANGE]:
- `DSTV.Net` — DSTV/NC1 steel fabrication exchange for profile-cut programs, saw/drill/punch data, and shop-machine handoff beside the neutral G-code AST; `KA` bend blocks feed `Forming/sheet`.

[RECT_PACKING]:
- `RectangleBinPack.CSharp` — cutting-stock suite: five packers behind `Nesting/stock`, and the `Nesting/nfp` rectangle fast-path via `MaxRectsBinPack`.

[ARC_FIT]:
- `geometry3Sharp` — the SOLE biarc/curve-fit owner, scoped to `g3.BiArcFit2`/`Arc2d`/`Segment2d`/`Vector2d`; the mesh-boolean half stays firewalled.

[ROBOT_KINEMATICS]:
- `Robots` — visose serial-chain kinematics owner: DH FK, analytic+numerical IK, `KinematicSolution`, external axes; never RhinoCommon into `Rhino3dm`; the weld-torch cell posts ride the same rows.
- `Rhino3dm` — the `extern alias R3` boundary assembly the `Robots` seam copies through (binary-distinct `Rhino.Geometry.*` from the kernel RhinoCommon); Mapperly owns the R3 seam copyists.

[ARC_OFFSET]:
- `CavalierContours` — arc-native bulge polyline offset/Boolean owner with `StaticAABB2DIndex`; orthogonal to line-only `Clipper2` and the `g3.BiArcFit2` refit.

[VORONOI_TESSELLATION]:
- `SharpVoronoiLib` — 2D Fortune Voronoi: clipping, border closure, Lloyd relaxation, Delaunay dual; point-site only — medial axis stays `Toolpath/skeleton`.

[SURFACE_ENGINE]:
- `OpenCAMLib` — 3-axis cutter-location engine: drop/push-cutter + waterline loops; `extern "C"` shim over SHARED `libocl` (LGPL-2.1).

[IMPLICIT_VOXEL]:
- `PicoGK` — LEAP71 implicit/SDF/voxel kernel: TPMS/lattice infill, SLA/DLP layer rasterization, SDF Booleans, `OpenVdbFile`; companion-only, never in-Rhino.

[ADDITIVE_3MF]:
- `lib3mf` — 3MF reader/WRITER: core/production/beam-lattice/slice egress, PicoGK `Lattice` maps onto `CBeamLattice`. Vendored ACT binding, RID native (BSD-2).

[TOOL_DATA_MODEL]:
- `MTConnect.NET-Common` — TrakHound ISO-13399 cutting-tool model behind the `Magazine`/`ToolAssembly` catalogue; feeds/speeds live on `Tooling/cuttingdata` (Kienzle `kc` + data ingress); `Tooling/wear` reads the decoded condition/telemetry slice.

[SOLID_INGRESS]:
- `OcctNet.Wrapper` — managed STEP/IGES B-rep ingress to `OcctShape`/`OcctMesh`; single-shape import only, no TKHLR/TKXCAF — HLR stays `Documentation/projection`.

[REJECTED]:
- `netDxf` — rejected second reader: DXF-only, no `Spline`/bulge parity with `ACadSharp`; the WRITE leg is the AppUi ACadSharp leg.
- `RectpackSharp` — removed as redundant: a strict subset of `RectangleBinPack.CSharp`; the `Nesting/nfp` fast-path re-homes to `MaxRectsBinPack`.
- `MaxRect`, `BinPack.NET` — rejected rectangle packers: AABB-only, no true-shape NFP feasibility; superseded by `MaxRectsBinPack`.
- `geometry4Sharp` — rejected second mesh/curve fork: the pinned `geometry3Sharp` carries the identical `g3` biarc surface; the rail composes the pin.

## [03]-[SUBSTRATE_PACKAGES]

Substrate cards this folder consumes from the registry. Full substrate law and package charters live in `libs/csharp/.planning/README.md`; shared API evidence lives in `libs/csharp/.api/`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `System.IO.Hashing` — reached ONLY through the kernel `ContentHash.Of` single mint; every egress content key seeds there.
- `NodaTime` — `Instant` stamps on traveler documents, quality-record models, probing receipts, tool-life schedules, and SPC control points.

[NUMERIC_SUBSTRATE]:
- `UnitsNet` — cut-parameter boundary + `Spec/tolerance` quantities (Speed/Length/RotationalSpeed/Pressure + Force/Power/Temperature/Angle/Torque overlay).
- `MathNet.Numerics` — `Spec/capability` distribution-fit + Monte-Carlo tolerance-stackup; `Tooling/wear` Taylor log-log `Fit.Line`; the streaming-moment half stays kernel `Domain/stats`.

[SIMD_SPANS]:
- `System.Numerics.Tensors` — SIMD-lower the hot sampling folds across `Toolpath/motion`, `Nesting/nfp`, `Spec/capability`.
- `CommunityToolkit.HighPerformance` — `Span2D`/`Memory2D` 2D grids for grayscale/uncut/engagement rasters and the `Verify/audit` layer census.

[MAPPING]:
- `Riok.Mapperly` — zero-reflection source-generated projections: DSTV-record→`Loop`, MTConnect asset rows, and the `extern alias R3` Rhino3dm boundary seam.

[GRAPH]:
- `QuikGraph` — the `Fixturing/setups` operation-precedence + datum-lineage graph, the `Fixturing/assembly` join-precedence DAG, and the `Toolpath/link` tour/routing (the magazine-eviction greedy stays hand-rolled).

[TEST_SUBSTRATE]:
- `xunit.v3.assert`
- `xunit.v3.common`
- `xunit.v3.extensibility.core`
- `xunit.v3.mtp-v2`
- `CsCheck`
- `coverlet.MTP`
- `BenchmarkDotNet`
- `Verify.XunitV3`
