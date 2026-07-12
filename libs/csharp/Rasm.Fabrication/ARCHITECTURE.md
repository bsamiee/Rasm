# [FABRICATION_ARCHITECTURE]

The domain map of `Rasm.Fabrication` — the host-neutral AEC-DOMAIN production-fabrication engine over `{Rasm, Rasm.Element}`. Fifteen folders ↔ fifteen TIER-2 namespaces, BIJECTIVE: the entry vocabulary, rail, and plan orchestrator (`Process`), tool intelligence and wear (`Tooling`), the 2D substrate (`Geometry2D`), geometry ingress (`Ingress`), subtractive CAM (`Toolpath`), motion topology and the fleet registry (`Kinematics`), production 3DP (`Additive`), layout and yield (`Nesting`), keep-out, setup, and assembly planning (`Fixturing`), machine-code emission (`Posting`), program-level truth (`Verify`), production specs (`Spec`), shop documentation (`Documentation`), sheet forming (`Forming`), and weld engineering (`Joining`). The polymorphic owner closes the concern over the 10-case `FabricationPolicy`/`FabricationResult` pair (`Cam` · `HiddenLine` · `Nest` · `Additive` · `Verify` · `Inspect` · `Post` · `Document` · `Derive` · `Form`). Every flagship terminates in a content-keyed machine-consumable artifact: the egress concern collapses onto the entry vocabulary — the thirteen-row `EgressKind` discriminant plus ONE `ContentKey.Of` fold seeding the kernel `ContentHash.Of` — never an `Egress/` folder and never a second hasher. The package depends up on the element seam through the `FabricationProjector : IElementProjection` registration row (realized on `Process/derivation`) and references no AEC peer — alignment travels through seam contracts and the content-keyed wire.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own casing. Treat every node as realized design — all fifty-nine pages are on disk.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Fabrication/
├── Process/            # The entry, the axes, the physics, the rail, the plan orchestrator — 2700 band owner + 2730
│   ├── Owner.cs # owner#atoms vocabulary
│   ├── Family.cs # ProcessKind 11 rows / Machine 14 rows axes
│   ├── Physics.cs # ONE Material identity carrying Map<ProcessModality, ModalityPhysics> → the nine-case RemovalBudget Formed = the form budget
│   ├── Faults.cs       # FabricationFault band-2700 registry: frozen retyped 2701-2710 + folder-grouped growth 2711-2729 + 2730-2746
│   └── Derivation.cs # RunDerive orchestrator
├── Tooling/            # ISO-13399 tool intelligence + machinability + wear — 2712, 2724, 2731
│   ├── Magazine.cs     # MTConnect CuttingToolAsset ToolAssembly + life-split minimal-swap Schedule wired to posting Tnn/M6/G43
│   ├── CuttingData.cs  # Kienzle kc machinability, ISO 513 class seeds, CSV/QIF data ingress, CutterForm.Of(ToolAssembly) projection
│   └── Wear.cs # Taylor VB flank-wear + condition-based RUL over decoded MTConnect telemetry
├── Geometry2D/         # The 2D substrate: line-space + arc-space + parametric-curve lane — 2703
│   ├── Algebra.cs # Clipper2 line-space owner: offset, DeltaCallback64 variable offset, Boolean/open-path clip, signed Area, Minkowski sum/diff
│   ├── Arcs.cs         # CavalierContours arc-space owner — ArcAlgebra kerf/lead/adaptive over kernel Offsetting.Apply region offsets
│   └── Curves.cs # Parametric third owner
├── Ingress/            # Everything entering as geometry — 2711
│   ├── Profile.cs      # DXF/DWG profile arm of the ONE polymorphic Ingress.Admit fold via ACadSharp; hosts the 4-case IngressSource union
│   ├── Solid.cs        # OcctNet STEP/IGES/STL B-rep → Triangulate → kernel MeshSpace admission (dirty STL via kernel HealOp)
│   ├── Steel.cs        # DSTV NC1 read arm → Loop via Riok.Mapperly generated projection; KA blocks feed Forming/sheet
│   └── Element.cs # ElementGraph.Bake arm → AdmittedComponent representation key, composition layers, connection rows, quantity/property bags keyed
├── Toolpath/           # Subtractive CAM — 2705-2707, 2713, 2723, 2732-2734
│   ├── Motion.cs # ProcessModality, CutStrategy generator arms
│   ├── Surface.cs # OpenCAMLib cutter positioning extern "C" shim over SHARED libocl + kernel on-mesh path layout geodesics/extract/flow/segment
│   ├── Partition.cs    # SharpVoronoiLib Fortune/Lloyd region decomposition (extern alias Voronoi)
│   ├── Guard.cs        # Swept tool-plus-holder gouge/collision guard consulted per committed feed move; retract ROUTING re-pointed to Link.Route
│   ├── Skeleton.cs     # Trochoidal constant-engagement WALK over the kernel clearance family (kernel-medial consumer per row 12)
│   ├── Turning.cs # LatheOp union FaceRough/TurnRough/ProfileFinish/Groove/Thread ISO-degression/Part
│   ├── Wire.cs # Wire-EDM cycle union Contour/Taper/FourAxis/NoCorePocket/Collar
│   ├── Link.cs # Rapid-travel minimization
│   └── Bevel.cs # BevelType I/V/A/Y/K/X per-edge coupled condition rows
├── Kinematics/         # Motion topology + the fleet registry — 2702, 2714
│   ├── Cell.cs         # Robots serial-chain cell solve — per-manufacturer posts, extern alias R3 boundary exemplar
│   ├── Machine.cs      # 5-axis rotary-topology inverse + TCP/RTCP over the KinematicClass rotary rows; HOMES the ONE jerk/accel motion-dynamics law
│   └── Fleet.cs # Machine-capability registry
├── Additive/           # Production 3DP — 2708, 2715, 2716, 2725, 2735
│   ├── Slicing.cs # FFF/DED planar slicing kernel Slicing.Apply SliceStack consumer per row 24
│   ├── Implicit.cs     # PicoGK IImplicit→Voxels TPMS/lattice/resin-powder lanes; declares ONCE the ALC-firebreak/sidecar posture
│   ├── Production.cs   # build orientation (kernel Analysis/select) + machine profiles + lib3mf 3MF egress
│   ├── ScanPath.cs # LPBF hatch union Meander/Stripe/Island/Hexagon
│   └── Support.cs # Planar layer-diff overhang census + top-down accumulation + interface carve
├── Nesting/            # Layout + yield + offcut lifecycle + cut linking — 2701, 2709, 2710, 2736
│   ├── Nfp.cs # NFP-feasibility true-shape nesting over Seq<Stock> inventory
│   ├── Stock.cs        # Rectangular cutting-stock yield engine over the RectangleBinPack.CSharp suite
│   ├── Remnant.cs # Offcut lifecycle partial
│   └── Linking.cs # LinkOp union CommonLine/ChainCut/Bridge/SkeletonCutUp
├── Fixturing/          # Keep-out + setup + assembly planning — 2717, 2726, 2727, 2737
│   ├── Workholding.cs  # Clamp/ExclusionZone keep-out family + the Condition fold the Cam conditioning composes
│   ├── Setups.cs       # QuikGraph operation-precedence/datum-lineage setup scheduler OWNING the setup→WCS assignment rows
│   └── Assembly.cs # Join-precedence planning
├── Posting/            # Machine-code emission — 2718, 2719, 2728
│   ├── Program.cs # Dialect-neutral CutProgram AST + cut conditioning incl
│   ├── Dialect.cs # per-dialect Emit generated total Switch over the PostDialect grammar family
│   └── Optimization.cs # MRR-adaptive feedrate + HSM corner smoothing + block-cap compaction over the CutProgram AST via the internal Lookahead
├── Verify/             # Program-level truth — 2720 (+2706 verify-time), 2738-2739
│   ├── Removal.cs # PicoGK BoolSubtract voxel material-removal verify → gouge/uncut/overcut/air-cut receipts, ResidualStock + per-setup
│   ├── Probing.cs      # in-process metrology — G31/G38 rows, kernel ICP datum best-fit, ConformanceMetric verdicts, QIF → capability
│   ├── Simulate.cs # Modal-state execution walk over the parsed CutProgram full RS274 group census as controller state
│   ├── Estimation.cs # Estimate.OfFabricationResult → CostReceipt
│   └── Audit.cs # Additive layer-stack pre-flight
├── Spec/               # Production specs — 2721, 2722, 2729
│   ├── Tolerance.cs # ISO 286 generated fits + ISO 1101/ASME Y14.5 FCF/datums + ISO 1302 finish, QIF-aligned
│   ├── Capability.cs   # Cp/Cpk/Pp/Ppk over kernel Stat.Of + MathNet fit/Monte-Carlo; the plan-time Cpk gate PRODUCING CapabilityVerdict
│   └── Manufacturability.cs # Cross-ModalityClass DfM verdicts on ONE surface
├── Documentation/      # Shop documentation — reserved fault cluster
│   ├── Projection.cs   # HLR emission (kernel DrawingProjection consumer per row 32; preserves the HiddenLineResult receipt AppUi is insulated at)
│   ├── Traveler.cs     # content-keyed setup sheets + shop travelers composed via the Run(Document) case — the widest fan-in node
│   └── Report.cs # FAI/mill-cert/weld-inspection as-built QualityRecord union
├── Forming/            # Sheet forming — 2740-2743
│   ├── Sheet.cs # The ONE unfold owner
│   ├── Brake.cs # BendSequence.Plan best-first over the reach/collision/occlusion feasibility matrix
│   └── Tube.cs # XYZ→YBC/LRA centerline fold + elongation carry + CLR/mandrel admission + analytic cope development into the profile-cut egress
└── Joining/            # Weld engineering — 2744-2746
    ├── Weld.cs # Joint×prep composition over the Materials-owned GrooveGeometry/GroovePrep
    ├── Sequence.cs # Distortion ordering Backstep/SkipWeld/Balanced/Block
    └── Procedure.cs # WPS/PQR essential-variable rows EssentialVariable mints here + the heat-input compliance gate feeding Spec/capability
```

The `Process` owner evaluates as TWO ledger nodes — `owner#atoms` upstream-only (every plane reads it; it reads only family leaf axes + kernel) and `owner#run` terminal (the 10-arm dispatch, composed by nothing) — the one ratified acyclicity exception; the page never splits physically. The WIRED-PIPELINE law: `owner#run → Cam`; `Cam → {guard.Check per feed move, workholding.Condition, magazine.Schedule consult}` producing the conditioned `Motion`; `owner#run → Derivation.Plan` (Derive) and the stage rail `Derivation.Plan → {Manufacturability.Assess → DfmReport.Routing, Fleet.Capable, Assembly.Sequence}` with the quote lane `→ Estimate.Of` (a receipt read, never an intra-run gate); the `Run(Form)` case body composes `FlatPattern.Unfold → BendSequence.Plan → FormedResult`; `Run(Post{Motion, PostDialect}) → {program AST-assembly + condition + dialect.Emit, magazine.Schedule, workholding conditioning, setups WCS}`; `Run(Document{results}) → traveler → {projection, program, magazine, setups, tolerance, capability, report}`; `workholding.Condition → algebra.NestingOrder` (the re-home that breaks the workholding→posting cycle); `guard → {skeleton.ClearanceAt, magazine.HolderEnvelope, workholding.ExclusionZone}` with retract ROUTING on `Link.Route`; `motion.Turn → Turning.Generate`; `turning/wire → curves.CurveAlgebra` (the parametric substrate); `weld → Cam(Joined · boundary-pass) + cell posts`, `sequence → assembly.Precedence + Deposition.InterpassTemp`, `procedure.Gate → capability`; `optimization → program.Lookahead (internal re-sweep)` with cycle-time truth on `Simulate.Execute`; `estimation ← {SimulationReceipt, WearState, NestYield/Placement/remnant rows, MachineMatch.Instance.HourlyRate}`; `linking → program Pierce/Lead chain rows`; `audit ← {kernel SliceStack, support's one overhang census}`; `element → AdmittedComponent` (atoms) with the bag keys pinned to `fleet.DemandKey`; `removal → implicit` (the declared voxel seam); `probing → program` (G38 rows); `capability.Gate → owner#atoms(CapabilityVerdict)`. FIVE cycle-breaks hold the DAG: `ProjectionDir` minted on atoms, `ResidualStock` input-carry, `CutterForm` on atoms, `NestingOrder` re-homed to `Geometry2D/algebra`, `CapabilityVerdict` input-carry; `AdmittedComponent` rides the `Derive` POLICY case (never a new input field) and the result-payload discipline (no plane-internal type on a `FabricationResult` case) is the sixth latent-cycle guard.

## [02]-[SEAMS]

```text seams
*                        →  csharp:Rasm.Element/Projection # [PROJECTION]: FabricationProjector : IElementProjection
*                        ←  csharp:Rasm # [SHAPE]: Predicate.Orient2D/Orient3D BOTH locked
Documentation/projection ←  csharp:Rasm/Drawing/view # [WIRE]: View.Apply → Fin<DrawingProjection> exact analytic HLR K6
Documentation/projection ←  csharp:Rasm/Meshing/arrangement # [WIRE]: Arrangement.Apply watertight
Documentation/projection →  csharp:Rasm.AppUi/Render # [RECEIPT]: HiddenLineResult Viewport2D edge sets
Documentation/report     ←  csharp:Rasm/Analysis/measure # [WIRE]: ConformanceMetric over ResidualSample K17
Additive/slicing         ←  csharp:Rasm/Meshing/slice # [WIRE]: Slicing.Apply → Fin<SliceStack>, five LayerPlan rows + 5-channel SoA forest K3
Additive/{scanpath,support} ← csharp:Rasm/Meshing/slice # [WIRE]: the same SliceStack owner
Verify/audit             ←  csharp:Rasm/Meshing/slice         # [WIRE]: SliceStack layer occupancy — the pre-flight census walks kernel layers (K3)
Toolpath/skeleton        ←  csharp:Rasm/Meshing/offset # [SHAPE]: ONE 2D/3D clearance family
Toolpath/skeleton        ←  csharp:Rasm/Meshing/skeleton # [WIRE]: Skeletonize.Apply → Fin<CurveSkeleton> + ClearancePoint3d probe
Toolpath/surface         ←  csharp:Rasm/Processing # [SHAPE]: geodesics/extract/flow/segment on-mesh path LAYOUT K10-K13
Toolpath/surface         ←  csharp:Rasm/Parametric/surface # [SHAPE-DECLARED]: SurfaceOp.{NormalOffsetFitPolicy Refit, CurvatureSample, Isolines} →
Toolpath/{motion,turning} ← csharp:Rasm/Analysis/measure # [SHAPE]: Bounds.EnclosingCylinderCaseVector3d Axis measure.md:343,:362
Toolpath/guard           ←  csharp:Rasm/Spatial/index # [SHAPE]: SpatialIndex BVH broad-phase keep-out prune
Geometry2D/curves        ←  csharp:Rasm/Parametric/{curve,nurbs} # [WIRE]: ParametricOp.Offset → OffsetsCurves, Receipt, TrimmedCrossings
{Geometry2D/curves, Forming/tube} ← csharp:Rasm/Numerics/atoms # [WIRE]: VectorFrame.ChainSeq<Point3d>, Direction, bool, Context, Op? atoms.md:232
Forming/sheet            ←  csharp:Rasm/Parametric/develop # [WIRE]: Development.Apply
Forming/tube             ←  csharp:Rasm/{Meshing/intersect, Parametric/develop} # [WIRE]: Intersection.Apply section + Development.Apply unroll
Joining/weld             ←  csharp:Rasm/Processing/intent # [WIRE]: VectorIntent.PoseCase
Ingress/solid            ←  csharp:Rasm/Meshing+Processing # [WIRE]: MeshSpace admission + predicate-gated HealOp for dirty STL K15
Ingress/profile          ←  csharp:Rasm.Bim/Exchange          # [SHAPE]: ACadSharp READ pin — Bim meshes, here 2D profiles; WRITE is AppUi's
Ingress/element          ←  csharp:Rasm.Element/Graph # [WIRE]: ElementGraph.Bake plus RepresentationContentHash and element properties
Ingress/element          ←  csharp:Rasm.Element/{Relations,Properties} # [SHAPE]: ConnectFrom, To, SubKind, and Realizing rows
Ingress/element          ←  csharp:Rasm.Materials/Projection # [SHAPE]: LayerSet/CompositionOf component.md:337,:439
{Fixturing/assembly, Joining/*, Forming/sheet} ← csharp:Rasm.Materials/Component # [SHAPE]: GrooveGeometry and GroovePrep carriers
Spec/manufacturability   ←  csharp:Rasm/Analysis+Spatial # [SHAPE]: Faces.Top/Bottom/At + Curves.Draft select.md:311+, K20 · Meshes census
{Spec/manufacturability, Verify/estimation} ← csharp:Rasm/Analysis/measure # [SHAPE]: conformance and estimation measures
Kinematics/fleet         ←  csharp:Rasm/Analysis/query # [WIRE]: Analyze.Run bounds query over MeshSpace
Verify/probing           ←  csharp:Rasm/Processing/register # [WIRE]: AlignKind ICP dispatcher under one AlignmentPolicy
Verify/probing           ←  csharp:Rasm/{Solving/fit, Processing/sample, Spatial/neighbors} # [WIRE-DECLARED, rides row 31]: Fin<FitReceipt>
Verify/probing           ←  csharp:Rasm/Analysis/measure      # [WIRE]: ConformanceMetric over ResidualSample — measured-vs-nominal verdicts (K17)
Posting/{program, optimization} ← csharp:Rasm/Domain/normalization # [SHAPE-DECLARED]: public CurveForm union normalization.md:158
Spec/capability          ←  csharp:Rasm/Domain/stats # [WIRE]: Stat.Of Welford public quantile surface + StatContext.Tolerance
Kinematics/machine       ←  csharp:Rasm/Parametric # [WIRE]: MotionInterpolation one-slerp owner
Additive/production      ←  csharp:Rasm/Analysis/select # [SHAPE]: Faces/Curves.Draft axis-ranked face decomposition
Nesting/nfp              ←  csharp:Rasm/Processing/flatten # [PROJECTION]: ChartAtlas unrolled UV islands + DistortionReceipt
Nesting/nfp              ←  csharp:Rasm/Parametric/develop # [PROJECTION]: DevelopmentReceipt isometric strips
Process/physics          ←  csharp:Rasm.Materials/Properties # [WIRE]: conductivity/specific-heat/density as raw doubles
Posting/program          →  csharp:Rasm/Drawing/pack # [WIRE]: toolpath PackKind committed-motion residency K7
```

## [03]-[FAULT_REGISTRY]

`FabricationFault` is one `[Union]` on `FaultBand.Fabrication = 2700` (the `Rasm.Element` registry row). Layer 1 (2701-2710) preserves wire-code decode and retypes bare detail payloads. Layer 2 (2711-2729) allocates folder-grouped across two tiers. Layer 3 (2730-2746) allocates folder-grouped under the 15-cluster partition. `OpenLoop` 2704 is the one cross-cutting arm (`FabConcern` carries the `form` row). The next free offset is 2747.

| [INDEX] | [FOLDER]         | [BLOCKS]                             | [ARMS]                                                                                                                                                         |
| :-----: | :--------------- | :----------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Process/`       | 2700 owner + cross-cut · 2730        | `OpenLoop` 2704 · `RoutingInfeasible` 2730 `(UInt128, DerivationStage)`                                                                                        |
|  [02]   | `Nesting/`       | 2701, 2709, 2710 (frozen) · 2736     | `NoFit` · `StockOverflow` · `Nest` · `RemnantStale` 2736 `(ContentKey)`                                                                                        |
|  [03]   | `Kinematics/`    | 2702, 2714                           | `Unreachable` · `AxisSingularity` (payload retyped onto the shared `MachineAxis`)                                                                              |
|  [04]   | `Geometry2D/`    | 2703                                 | `KerfCollision` (the kerf owner's; curves routes kernel `DegenerateInput`)                                                                                     |
|  [05]   | `Toolpath/`      | 2705-2707, 2713, 2723 · 2732-2734    | `InadmissiblePair` · `Gouge` · `Collision` · `SampleStalled` · `PartitionDegenerate` · `WireTaperExceeded` 2732 · `LinkBlocked` 2733 · `BevelUnsupported` 2734 |
|  [06]   | `Additive/`      | 2708, 2715, 2716, 2725 · 2735        | `NonManifoldSlice` · `VoxelFault` · `OrientationInfeasible` · `Unsupported3mfExtension` · `SupportUnbuildable` 2735                                            |
|  [07]   | `Ingress/`       | 2711                                 | `IngressTranslation` (one polymorphic arm; `SourceLocus` now four loci incl. `ElementNode`)                                                                    |
|  [08]   | `Tooling/`       | 2712, 2724 · 2731                    | `MachinabilityUnknown` · `NoToolForOp` · `WearEstimateUnfit` 2731                                                                                              |
|  [09]   | `Fixturing/`     | 2717, 2726, 2727 · 2737              | `SetupInfeasible` · `DatumLineageBroken` · `ClampOnMachinedFace` · `AssemblyPrecedenceCyclic` 2737                                                             |
|  [10]   | `Posting/`       | 2718, 2719, 2728                     | `DialectUnsupported` · `ProgramParse` · `BlockCapExceeded` (optimization compaction routes 2728; no tier-3 arm)                                                |
|  [11]   | `Verify/`        | 2720 (+2706 verify-time) · 2738-2739 | `ProbeOvertravel` · `EnvelopeExceeded` 2738 · `SimulatedOvertravel` 2739 (both on `MachineAxis`); estimation/audit are receipts-only                           |
|  [12]   | `Spec/`          | 2721, 2722, 2729                     | `ToleranceUnsatisfiable` · `CapabilityShortfall` · `StackupExceeded` (manufacturability is receipts-only)                                                      |
|  [13]   | `Documentation/` | reserved (empty)                     | projection routes kernel `GeometryFault.ProjectionFault` 2436-2439; traveler/report are content-keyed assembly with no fault producer                          |
|  [14]   | `Forming/`       | 2740-2743                            | `UnfoldInfeasible` 2740 · `BendSequenceInfeasible` 2741 · `TonnageExceeded` 2742 · `MinBendRadiusViolated` 2743                                                |
|  [15]   | `Joining/`       | 2744-2746                            | `WeldAccessBlocked` 2744 · `HeatInputExceeded` 2745 · `WpsUnqualified` 2746 `(EssentialVariable, double)`                                                      |

## [04]-[CROSS_PACKAGE]

Every machine-consumable egress artifact mints its content key through the CONFIRMED `ContentHash.Of` seed-zero entry (the landed kernel law); the Fabrication egress discriminant is the LOCAL thirteen-row `EgressKind` axis, FEDERATED to the Persistence `ArtifactKind` rows at the content-key boundary — never a strata-crossing type reference and never a second same-named mint.

| [INDEX] | [SEAM_FABRICATION_SIDE]                                                                                  | [DIRECTION]                  | [COUNTERPART]                                                                                                                                                                                                                                                                                           |
| :-----: | :------------------------------------------------------------------------------------------------------- | :--------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Nesting/stock → Rasm.Compute`                                                                           | produces — TWO-SIDED, landed | `NestYield.WasteAreaMm2` — Compute decodes `ElementQuantity.WasteAreaM2`/`NestWasteM2` under the frozen `NestWasteArea` key (SI m², `Rasm.Compute/ARCHITECTURE.md:90`); `Nesting/remnant` reuse REDUCES the same wire value (recorded-only); the quantity-bag lowering rides the `FabricationProjector` |
|  [02]   | `Nesting/stock ← Rasm.Element`                                                                           | consumes                     | `MaterialId` — the seam material a cut part is cut from                                                                                                                                                                                                                                                 |
|  [03]   | `Nesting/nfp ← Rasm.Compute`                                                                             | gated consume (Compute-side) | DRL nest-policy score as an injected `Func<NoFitPolygon, PartTransform, double>` delegate column — strata-correct; the gate closes only when Compute lands its side                                                                                                                                     |
|  [04]   | `Posting/program → Rasm.Persistence`                                                                     | durable-row ENROLLMENT       | `cutprogram` `ArtifactKind` Growth-law row on the LANDED `Query/cache#ARTIFACT_BLOB_INDEX` owner — lands with the program page                                                                                                                                                                          |
|  [05]   | `Nesting/{nfp, remnant} → Rasm.Persistence`                                                              | durable-row ENROLLMENT       | `placement`/`remnant` `ArtifactKind` rows — nfp owns the MINT, remnant the LIFECYCLE; one `ContentKey`, no second enrollment                                                                                                                                                                            |
|  [06]   | `{Verify/removal, Documentation/traveler, Additive/production} → Rasm.Persistence`                       | durable-row ENROLLMENT       | `cli`/`threemf`/`nc1`/`stock-snapshot`/`traveler` `ArtifactKind` rows via `ContentHash.Of` — each lands with its egress page                                                                                                                                                                            |
|  [07]   | `{Forming/sheet, Forming/brake, Joining/weld, Additive/scanpath, Process/derivation} → Rasm.Persistence` | durable-row ENROLLMENT       | `flat-pattern`/`bend-program`/`weld-plan`/`scan-vectors`/`plan` `ArtifactKind` rows — each rides its landed egress page                                                                                                                                                                                 |
|  [08]   | `{Tooling/magazine, Tooling/wear, Verify/probing} ← Rasm.AppHost`                                        | decoded telemetry            | MTConnect `-Common` model slice — the LANDED AppHost owners (`Wire/livewire.md` `MtconnectLane`→`ExternalValue` decode); magazine tool-life FIRST consumer, wear condition-RUL SECOND, probing measured-feature THIRD; transport never crosses                                                          |
|  [09]   | `Documentation/projection → Rasm.AppUi/Render`                                                           | produces                     | `HiddenLineResult` receipt — supersession insulated AT the receipt                                                                                                                                                                                                                                      |
|  [10]   | `Ingress/profile ← Rasm.Bim/Exchange`                                                                    | shared read pin              | ACadSharp read — Bim meshes, here 2D profiles; WRITE is the AppUi leg                                                                                                                                                                                                                                   |
|  [11]   | `Ingress/element ← {Rasm.Element, Rasm.Materials, Rasm.Bim}`                                             | consumes                     | `ElementGraph.Bake` + composition/connection/detail reads at the string-key boundary; `Rasm.Bim` `Semantics/connection.md:13-16` names this detailer the consumer                                                                                                                                       |
|  [12]   | `Fabrication → Rasm.Element/Projection`                                                                  | REALIZED contract            | `FabricationProjector : IElementProjection` — the one app-wired `Seq<IElementProjection>` registration row (`Process/derivation`); quantity-bag lowering carries the frozen `NestWasteArea` key                                                                                                         |
