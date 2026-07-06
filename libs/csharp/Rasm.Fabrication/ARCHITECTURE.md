# [FABRICATION_ARCHITECTURE]

The domain map of `Rasm.Fabrication` — the host-neutral AEC-DOMAIN production-fabrication engine over `{Rasm, Rasm.Element}`. Fifteen folders ↔ fifteen TIER-2 namespaces, BIJECTIVE: the entry vocabulary, rail, and plan orchestrator (`Process`), tool intelligence and wear (`Tooling`), the 2D substrate (`Geometry2D`), geometry ingress (`Ingress`), subtractive CAM (`Toolpath`), motion topology and the fleet registry (`Kinematics`), production 3DP (`Additive`), layout and yield (`Nesting`), keep-out, setup, and assembly planning (`Fixturing`), machine-code emission (`Posting`), program-level truth (`Verify`), production specs (`Spec`), shop documentation (`Documentation`), sheet forming (`Forming`), and weld engineering (`Joining`). The polymorphic owner closes the concern over the **10-case** `FabricationPolicy`/`FabricationResult` pair (`Cam` · `HiddenLine` · `Nest` · `Additive` · `Verify` · `Inspect` · `Post` · `Document` · `Derive` · `Form`). Every flagship terminates in a content-keyed machine-consumable artifact: the egress concern collapses onto the entry vocabulary — the thirteen-row `EgressKind` discriminant plus ONE `ContentKey.Of` fold seeding the kernel `ContentHash.Of` — never an `Egress/` folder and never a second hasher. The package depends up on the element seam through the `FabricationProjector : IElementProjection` registration row (realized on `Process/derivation`) and references no AEC peer — alignment travels through seam contracts and the content-keyed wire.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own casing. Treat every realized node as realized code; a `QUEUED` node names its DECISION roster row and lands in its tranche.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Fabrication/
├── Process/            # The entry, the axes, the physics, the rail, the plan orchestrator — 2700 band owner + 2730
│   ├── Owner.cs        # owner#atoms vocabulary (Loop+Bulges/Move/CutterForm/AdmittedComponent/PlannedStep/BendStep/ResidualStock/StockSnapshot/CapabilityVerdict/EgressKind ×13/ContentKey) + 10-case Run Switch (owner#run)
│   ├── Family.cs       # ProcessKind (11 rows) / Machine (14 rows) axes; ProcessModality superset (7 rows) grouped by ModalityClass {removal, additive, formed, joined}; CutStrategy dimensionality; KinematicClass rotary topology; PostDialect grammar-capability table
│   ├── Physics.cs      # ONE Material identity carrying Map<ProcessModality, ModalityPhysics> → the nine-case RemovalBudget (Formed = the form budget; Deposition doubles as the joined-key weld heat-input budget)
│   ├── Faults.cs       # FabricationFault band-2700 registry: frozen retyped 2701-2710 + folder-grouped growth 2711-2729 + 2730-2746
│   └── Derivation.cs   # Run(Derive) orchestrator — manufacturability → routing → fleet → setup/assembly → programs → documentation stage rail; DerivePolicy/DerivationStage; FabricationProjector : IElementProjection registration
├── Tooling/            # ISO-13399 tool intelligence + machinability + wear — 2712, 2724, 2731
│   ├── Magazine.cs     # MTConnect CuttingToolAsset ToolAssembly + life-split minimal-swap Schedule wired to posting Tnn/M6/G43
│   ├── CuttingData.cs  # Kienzle kc machinability, ISO 513 class seeds, CSV/QIF data ingress, CutterForm.Of(ToolAssembly) projection
│   └── Wear.cs         # Taylor VB flank-wear + condition-based RUL over decoded MTConnect telemetry; cross-ModalityClass consumable state; MathNet Fit.Line log-log fit
├── Geometry2D/         # The 2D substrate: line-space + arc-space + parametric-curve lane — 2703
│   ├── Algebra.cs      # Clipper2 line-space owner: offset, DeltaCallback64 variable offset, Boolean/open-path clip, Minkowski sum/diff (carries the arc-space block until Arcs.cs lands)
│   ├── Arcs.cs         # QUEUED (DECISION row 8): CavalierContours arc-space owner — ArcAlgebra kerf/lead/adaptive over kernel Offsetting.Apply region offsets
│   └── Curves.cs       # Parametric third owner — CurveAlgebra.Apply over kernel ParametricOp Offset/Stations/Divide/Refit + Fill + NURBS refit/frames; feeds turning, 2.5D pocket-profile, posting stations, 5-axis smoothing
├── Ingress/            # Everything entering as geometry — 2711
│   ├── Profile.cs      # DXF/DWG profile arm of the ONE polymorphic Ingress.Admit fold via ACadSharp; hosts the 4-case IngressSource union
│   ├── Solid.cs        # QUEUED (row 10): OcctNet STEP/IGES/STL B-rep → Triangulate → kernel MeshSpace admission (dirty STL via kernel HealOp)
│   ├── Steel.cs        # QUEUED (row 11): DSTV NC1 read arm → Loop via Riok.Mapperly generated projection; KA blocks feed Forming/sheet
│   └── Element.cs      # ElementGraph.Bake arm → AdmittedComponent (representation key, composition layers, connection rows, quantity/property bags keyed to the fleet DemandKey contract)
├── Toolpath/           # Subtractive CAM — 2705-2707, 2713, 2723, 2732-2734
│   ├── Motion.cs       # (ProcessModality, CutStrategy) generator arms; the Cam fold executes Guard/Workholding/Magazine conditioning and emits the conditioned Motion; Turn arm composes Turning.Generate; SliceWalk carries seam/comb/monotonic rows
│   ├── Surface.cs      # QUEUED (row 22): OpenCAMLib cutter positioning (extern "C" shim over SHARED libocl) + kernel on-mesh path layout (geodesics/extract/flow/segment)
│   ├── Partition.cs    # QUEUED (row 23): SharpVoronoiLib Fortune/Lloyd region decomposition (extern alias Voronoi)
│   ├── Guard.cs        # Swept tool-plus-holder gouge/collision guard consulted per committed feed move; retract ROUTING re-pointed to Link.Route
│   ├── Skeleton.cs     # Trochoidal constant-engagement WALK over the kernel clearance family (kernel-medial consumer per row 12)
│   ├── Turning.cs      # LatheOp union (FaceRough/TurnRough/ProfileFinish/Groove/Thread ISO-degression/Part); G71/G72/G73 pass folds; 9-quadrant nose comp; Css/G97 spindle modes; K14 revolved envelope
│   ├── Wire.cs         # Wire-EDM cycle union (Contour/Taper/FourAxis/NoCorePocket/Collar); compound skim offsets; guide-plane taper; corner slowdown + wire lag; the Erosion budget's consumer
│   ├── Link.cs         # Rapid-travel minimization — QuikGraph MST/DFS tour + A*/Dijkstra obstacle-routed retracts over guard clearance + kernel SpatialIndex; Link.Route the one linking owner
│   └── Bevel.cs        # BevelType I/V/A/Y/K/X per-edge coupled condition rows; waterjet twin-tilt taper-lag; the ONE THC/Z-law custodian; groove-prep output feeds Joining/weld
├── Kinematics/         # Motion topology + the fleet registry — 2702, 2714
│   ├── Cell.cs         # Robots serial-chain cell solve — per-manufacturer posts, extern alias R3 boundary exemplar
│   ├── Machine.cs      # QUEUED (row 17): 5-axis rotary-topology inverse + TCP/RTCP over the KinematicClass rotary rows; HOMES the ONE jerk/accel motion-dynamics law
│   └── Fleet.cs        # Machine-capability registry — Fleet.Capable(AdmittedComponent, MachineFleet) → Seq<MachineMatch>; envelope/topology/spindle/tool-capacity/material/grade six-check join; DemandKey bag contract
├── Additive/           # Production 3DP — 2708, 2715, 2716, 2725, 2735
│   ├── Slicing.cs      # FFF/DED planar slicing (kernel Slicing.Apply SliceStack consumer per row 24); shells carry the Arachne bead law over the K1 medial RADIUS; density-field infill policy row
│   ├── Implicit.cs     # QUEUED (row 25): PicoGK IImplicit→Voxels TPMS/lattice/resin-powder lanes; declares ONCE the ALC-firebreak/sidecar posture
│   ├── Production.cs   # QUEUED (row 26): build orientation (kernel Analysis/select) + machine profiles + lib3mf 3MF egress
│   ├── ScanPath.cs     # LPBF hatch union (Meander/Stripe/Island/Hexagon); θₙ = (θ₀+n·66.7°) mod 180 recurrence; direction×ordering sorter axes; up/down/in-skin partition; scan-vectors egress off the Powder budget
│   └── Support.cs      # Planar layer-diff overhang census + top-down accumulation + interface carve; TREE influence-area walk with avoidance-state cache; bridge anchor state machine
├── Nesting/            # Layout + yield + offcut lifecycle + cut linking — 2701, 2709, 2710, 2736
│   ├── Nfp.cs          # NFP-feasibility true-shape nesting over Seq<Stock> inventory; MaxRectsBinPack rect-fastpath; mints Remnant (partial — the lifecycle half is Remnant.cs's)
│   ├── Stock.cs        # Rectangular cutting-stock yield engine over the RectangleBinPack.CSharp suite
│   ├── Remnant.cs      # Offcut lifecycle partial — RemnantState transitions, ReusePolicy admission, Claim/Release ledger, Stockable re-mint into the next Inventory
│   └── Linking.cs      # LinkOp union (CommonLine/ChainCut/Bridge/SkeletonCutUp); collision-checked graph edits over the Placement; pierce+rapid objective; one pierce/lead per chain into program
├── Fixturing/          # Keep-out + setup + assembly planning — 2717, 2726, 2727, 2737
│   ├── Workholding.cs  # Clamp/ExclusionZone keep-out family + the Condition fold the Cam conditioning composes
│   ├── Setups.cs       # QUEUED (row 15): QuikGraph operation-precedence/datum-lineage setup scheduler OWNING the setup→WCS assignment rows
│   └── Assembly.cs     # Join-precedence planning — Assembly.Sequence(AdmittedComponent, AssemblyPolicy) → AssemblyPlan; JoinClass prefix classification; QuikGraph DAG gate + topological order + transitive reduction
├── Posting/            # Machine-code emission — 2718, 2719, 2728
│   ├── Program.cs      # Dialect-neutral CutProgram AST + cut conditioning (incl. the cooling Lookahead-sibling pass) + content key; the Run(Post) case body assembles the AST from the atoms Motion; Parse round-trip (NIST modal groups)
│   ├── Dialect.cs      # QUEUED (row 27): per-dialect Emit generated total Switch over the PostDialect grammar family; canned-cycle/macro/subprogram lowering; NC1 emit target
│   └── Optimization.cs # MRR-adaptive feedrate + HSM corner smoothing + block-cap compaction over the CutProgram AST via the internal Lookahead re-sweep; cycle-time truth stays Verify/simulate
├── Verify/             # Program-level truth — 2720 (+2706 verify-time), 2738-2739
│   ├── Removal.cs      # QUEUED (row 28): PicoGK BoolSubtract voxel material-removal verify → gouge/uncut/overcut/air-cut receipts, ResidualStock + per-setup StockSnapshots
│   ├── Probing.cs      # QUEUED (row 31): in-process metrology — G31/G38 rows, kernel ICP datum best-fit, ConformanceMetric verdicts, QIF → capability
│   ├── Simulate.cs     # Modal-state execution walk over the parsed CutProgram (full RS274 group census as controller state); per-block accel-limited time under the machine.md motion-dynamics TYPE contract; envelope/overtravel verdicts; the authoritative cycle-time owner
│   ├── Estimation.cs   # Estimate.Of(FabricationResult) → CostReceipt — simulate clock, nest waste, wear consumables, remnant credit, MachineMatch.Instance.HourlyRate rates; receipts only
│   └── Audit.cs        # Additive layer-stack pre-flight — Span2D connected components per layer; LayerDefect union (Island/ResinTrap/SuctionCup/OverhangArea/TouchingBound); cross-layer lineage; receipts only
├── Spec/               # Production specs — 2721, 2722, 2729
│   ├── Tolerance.cs    # QUEUED (row 20): ISO 286 generated fits + ISO 1101/ASME Y14.5 FCF/datums + ISO 1302 finish, QIF-aligned; Ra→scallop and IT→allowance derivation rows
│   ├── Capability.cs   # QUEUED (row 29): Cp/Cpk/Pp/Ppk over kernel Stat.Of + MathNet fit/Monte-Carlo; the plan-time Cpk gate PRODUCING CapabilityVerdict
│   └── Manufacturability.cs # Cross-ModalityClass DfM verdicts on ONE surface — draft/min-wall/undercut/tool-access/bend-radius/weld-access/printability; DfmReport.Routing feeds derivation; receipts only
├── Documentation/      # Shop documentation — reserved fault cluster
│   ├── Projection.cs   # HLR emission (kernel DrawingProjection consumer per row 32; preserves the HiddenLineResult receipt AppUi is insulated at)
│   ├── Traveler.cs     # QUEUED (row 33): content-keyed setup sheets + shop travelers composed via the Run(Document) case — the widest fan-in node
│   └── Report.cs       # FAI/mill-cert/weld-inspection as-built QualityRecord union — measured features + Cp/Cpk + conformance rows, content-keyed; MODEL only, rendering rides the artifacts seam
├── Forming/            # Sheet forming — 2740-2743
│   ├── Sheet.cs        # The ONE unfold owner — FlatPattern.Unfold as the BA-substitution overlay over kernel Development.Apply; K-factor table/coupon/DIN-6935 sources; relief/hem unions; FormPolicy mints here
│   ├── Brake.cs        # BendSequence.Plan best-first over the reach/collision/occlusion feasibility matrix; V=f·T die rows; F=(C·Rm·S²·L)/(V·1000) tonnage; BendMethod air/bottoming/coining; springback overbend
│   └── Tube.cs         # XYZ→YBC/LRA centerline fold + elongation carry + CLR/mandrel admission + analytic cope development into the profile-cut egress; research-gated row (bender-format breadth precedes the deep interior)
└── Joining/            # Weld engineering — 2744-2746
    ├── Weld.cs         # Joint×prep composition over the Materials-owned GrooveGeometry/GroovePrep; bead-stack fill fold; HI = η·60·V·I/(1000·v) off the joined-key Deposition budget; torch Motion through Cam under the Joined modality
    ├── Sequence.cs     # Distortion ordering (Backstep/SkipWeld/Balanced/Block); tack plan; interpass-temperature scheduling (Deposition.InterpassTemp consumer); permutes only within AssemblyPlan.Precedence
    └── Procedure.cs    # WPS/PQR essential-variable rows (EssentialVariable mints here) + the heat-input compliance gate feeding Spec/capability; QIF-aligned
```

The `Process` owner evaluates as TWO ledger nodes — `owner#atoms` upstream-only (every plane reads it; it reads only family leaf axes + kernel) and `owner#run` terminal (the 10-arm dispatch, composed by nothing) — the one ratified acyclicity exception; the page never splits physically. The WIRED-PIPELINE law: `owner#run → Cam`; `Cam → {guard.Check per feed move, workholding.Condition, magazine.Schedule consult}` producing the conditioned `Motion`; `owner#run → Derivation.Plan` (Derive) and the stage rail `Derivation.Plan → {Manufacturability.Assess → DfmReport.Routing, Fleet.Capable, Assembly.Sequence}` with the quote lane `→ Estimate.Of` (a receipt read, never an intra-run gate); the `Run(Form)` case body composes `FlatPattern.Unfold → BendSequence.Plan → FormedResult`; `Run(Post{Motion, PostDialect}) → {program AST-assembly + condition + dialect.Emit, magazine.Schedule, workholding conditioning, setups WCS}`; `Run(Document{results}) → traveler → {projection, program, magazine, setups, tolerance, capability, report}`; `workholding.Condition → algebra.NestingOrder` (the re-home that breaks the workholding→posting cycle); `guard → {skeleton.ClearanceAt, magazine.HolderEnvelope, workholding.ExclusionZone}` with retract ROUTING on `Link.Route`; `motion.Turn → Turning.Generate`; `turning/wire → curves.CurveAlgebra` (the parametric substrate); `weld → Cam(Joined · boundary-pass) + cell posts`, `sequence → assembly.Precedence + Deposition.InterpassTemp`, `procedure.Gate → capability (QUEUED)`; `optimization → program.Lookahead (internal re-sweep)` with cycle-time truth on `Simulate.Execute`; `estimation ← {SimulationReceipt, WearState, NestYield/Placement/remnant rows, MachineMatch.Instance.HourlyRate}`; `linking → program Pierce/Lead chain rows`; `audit ← {kernel SliceStack, support's one overhang census}`; `element → AdmittedComponent` (atoms) with the bag keys pinned to `fleet.DemandKey`; `removal → implicit` (the declared voxel seam); `probing → program` (G38 rows); `capability.Gate → owner#atoms(CapabilityVerdict)`. FIVE cycle-breaks hold the DAG: `ProjectionDir` minted on atoms, `ResidualStock` input-carry, `CutterForm` on atoms, `NestingOrder` re-homed to `Geometry2D/algebra`, `CapabilityVerdict` input-carry; `AdmittedComponent` rides the `Derive` POLICY case (never a new input field) and the result-payload discipline (no plane-internal type on a `FabricationResult` case) is the sixth latent-cycle guard.

## [02]-[SEAMS]

```text seams
*                        →  csharp:Rasm.Element/Projection    # [PROJECTION]: FabricationProjector : IElementProjection — Project(ProjectionContext) → Fin<GraphDelta> (projection.md:54); REALIZED on Process/derivation as the one app-wired Seq<IElementProjection> registration row (projection.md:113)
*                        ←  csharp:Rasm                       # [SHAPE]: Predicate.Orient2D/Orient3D (BOTH locked — the Posting wire) · MeshSpace · Point3d/Vector3d · Matrix · ContentHash.Of (K9) · Op/Eff<Env> + OpAcceptance + AtomProjection rails (K21-K23)
Documentation/projection ←  csharp:Rasm/Drawing/view          # [WIRE]: View.Apply → Fin<DrawingProjection> exact analytic HLR (K6) — the ONE visibility solve; the in-folder BSP dies with row 32
Documentation/projection ←  csharp:Rasm/Meshing/arrangement   # [WIRE]: Arrangement.Apply watertight — the arm reads ArrangementResult.Boolean.Solid (K5); healing re-points to Rasm/Processing/repair
Documentation/projection →  csharp:Rasm.AppUi/Render          # [RECEIPT]: HiddenLineResult Viewport2D edge sets — supersession insulated AT the receipt (UIBRIEF [V7](b))
Documentation/report     ←  csharp:Rasm/Analysis/measure      # [WIRE]: ConformanceMetric over ResidualSample (K17) — as-built conformance verdicts (probing receipts arrive input-carried)
Additive/slicing         ←  csharp:Rasm/Meshing/slice         # [WIRE]: Slicing.Apply → Fin<SliceStack>, five LayerPlan rows + 5-channel SoA forest (K3); adaptive layer HEIGHT stays kernel LayerPlan — sealed
Additive/{scanpath,support} ← csharp:Rasm/Meshing/slice       # [WIRE]: the same SliceStack owner — hatch/skin region algebra and the overhang layer-diff read kernel-emitted contours (K3)
Verify/audit             ←  csharp:Rasm/Meshing/slice         # [WIRE]: SliceStack layer occupancy — the pre-flight census walks kernel layers (K3)
Toolpath/skeleton        ←  csharp:Rasm/Meshing/offset        # [SHAPE]: ONE 2D/3D clearance family — Offsetting.Apply Medial + per-point clearance RADIUS + OffsetOp.Clearance probe case (K1); slicing's Arachne bead law and manufacturability's min-wall read the SAME radius
Toolpath/skeleton        ←  csharp:Rasm/Meshing/skeleton      # [WIRE]: Skeletonize.Apply → Fin<CurveSkeleton> + Clearance(Point3d probe) — the 3D half of the one clearance family (K2)
Toolpath/surface         ←  csharp:Rasm/Processing            # [SHAPE]: geodesics/extract/flow/segment on-mesh path LAYOUT (K10-K13); Spatial/fields SDF the recorded OCL fallback (K8); Simplify.Apply → DecimationResult + Remeshing.Apply(RemeshOp.Isotropic) → RewriteResult LOD/conditioning pre-pass (K36 — declared, rides row 22)
Toolpath/surface         ←  csharp:Rasm/Parametric/surface    # [SHAPE-DECLARED]: SurfaceOp.{NormalOffset(FitPolicy Refit), CurvatureSample, Isolines} → SurfaceResult.{Offsets(RefineReceipt), CurvatureField(K1/K2/Gaussian/Mean/Dir1/Dir2), Isolines} — CL-offset/curvature/isoline suite (K25; rides row 22)
Toolpath/{motion,turning} ← csharp:Rasm/Analysis/measure      # [SHAPE]: Bounds.EnclosingCylinderCase(Vector3d Axis) (measure.md:343,:362) — the Turn/lathe revolved stock envelope (K14, realized on turning)
Toolpath/guard           ←  csharp:Rasm/Spatial/index         # [SHAPE]: SpatialIndex BVH broad-phase keep-out prune; Link.Route rides the same index for retract channels
Geometry2D/curves        ←  csharp:Rasm/Parametric/{curve,nurbs} # [WIRE]: ParametricOp.Offset → Offsets(Curves, Receipt, TrimmedCrossings, KeptSegments) · Stations → StationField · Divide → Division · Reconstruct → Refit (curve.md:102-124) · Fill → ArrangementResult.Overlay · NurbsWire.CurveThrough(FitPolicy) (nurbs.md:154) · PerpendicularFrames (:215) — K24
{Geometry2D/curves, Forming/tube} ← csharp:Rasm/Numerics/atoms # [WIRE]: VectorFrame.Chain(Seq<Point3d>, Direction, bool, Context, Op?) (atoms.md:232) — the RMF PUBLIC front (BishopChain is kernel-internal, never crossed); VectorCone.Of/Contains (:254) orientation cones (K31/K32)
Forming/sheet            ←  csharp:Rasm/Parametric/develop    # [WIRE]: Development.Apply — DevelopOp.Decompose(SurfaceResult.UvTessellation, DevelopPolicy) (develop.md:74) → StripField with LayoutParent MST columns (:61-65) — the ONE unroll engine; sheet is the neutral-fiber BA-substitution OVERLAY
Forming/tube             ←  csharp:Rasm/{Meshing/intersect, Parametric/develop} # [WIRE]: Intersection.Apply section + Development.Apply unroll — cope/fishmouth saddle development into the profile-cut egress
Joining/weld             ←  csharp:Rasm/Processing/intent     # [WIRE]: VectorIntent.PoseCase — the PUBLIC orientation-interpolation front for torch frames (MotionInterpolation.Interpolate is kernel-internal, never crossed; K19 corrected)
Ingress/solid            ←  csharp:Rasm/Meshing+Processing    # [WIRE]: MeshSpace admission + predicate-gated HealOp for dirty STL (K15) — downstream planes consume admitted kernel geometry
Ingress/profile          ←  csharp:Rasm.Bim/Exchange          # [SHAPE]: shared ACadSharp READ pin — Bim owns the read codec, the two-format WRITE leg is AppUi's
Ingress/element          ←  csharp:Rasm.Element/Graph         # [WIRE]: ElementGraph.Bake (element.md:531) + RepresentationContentHash (:122) + the Element property bags (:380-393) — the baked ingress seam producing AdmittedComponent
Ingress/element          ←  csharp:Rasm.Element/{Relations,Properties} # [SHAPE]: Connect(From, To, SubKind, Realizing) (relation.md:177,:189) + DetailSchema.Realization (property.md:262) — connection rows admitted as ComponentConnection string keys; Rasm.Bim Semantics/connection.md:13-16 names this detailer the consumer
Ingress/element          ←  csharp:Rasm.Materials/Projection  # [SHAPE]: LayerSet/CompositionOf (component.md:337,:439; Ply/Layered :442-443) — composition read as ComponentLayer boundary scalars, never Materials types
{Fixturing/assembly, Joining/*, Forming/sheet} ← csharp:Rasm.Materials/Component # [SHAPE]: GrooveGeometry (joint.md:97) · GroovePrep (:239) · WeldRow.MinimumFilletLegMm (:313) · ConnectorPlate (connector.md:193) · PlateStock (:185) — Materials OWNS the joining vocabulary; Fabrication consumes at the string-key boundary, never re-mints
Spec/manufacturability   ←  csharp:Rasm/Analysis+Spatial      # [SHAPE]: Faces.Top/Bottom/At + Curves.Draft (select.md:311+, K20) · Meshes census Validity/Counts/Defects/Quality/NakedEdges (inspect.md:257-266, K34) · RayQuery(Ray3d, MaxReflections) (relations.md:57) + SpatialQuery.Ray/.Winding GWN (index.md:101,:104, K29/K30) · the K1 medial clearance RADIUS min-wall probe
{Spec/manufacturability, Verify/estimation} ← csharp:Rasm/Analysis/measure # [SHAPE]: Measure.Volume/Centroid(MassKind)/Inertia(MassKind)/PrincipalAxes(MassKind) (measure.md:49-55) + Bounds.Oriented/Principal/EnclosingSphere/EnclosingCircle (:349-361) — mass/envelope metrology (K27/K28)
Kinematics/fleet         ←  csharp:Rasm/Analysis/query        # [WIRE]: Analyze.Run<MeshSpace, BoundingBox>(AnalysisQuery.Bounds(Bounds.AxisAligned), …) — the component envelope bound (Analysis/query.md:3)
Verify/probing           ←  csharp:Rasm/Processing/register   # [WIRE]: AlignKind ICP dispatcher under one AlignmentPolicy — probed points → nominal (K16)
Verify/probing           ←  csharp:Rasm/{Solving/fit, Processing/sample, Spatial/neighbors} # [WIRE-DECLARED, rides row 31]: Fin<FitReceipt> Fit.Apply(FitOp, Context, Op?) — FitKind plane/sphere/cylinder/cone/torus/line, FitReceipt(Primitive, Inliers, Residual, Consensus, Trials, Iterations) (K26) · 12-case SampleKind inspection sampling (K35) · NeighborIndex.Of(NeighborSource, Op?) kNN scan front (K37)
Verify/probing           ←  csharp:Rasm/Analysis/measure      # [WIRE]: ConformanceMetric over ResidualSample — measured-vs-nominal verdicts (K17)
Posting/{program, optimization} ← csharp:Rasm/Domain/normalization # [SHAPE-DECLARED]: public CurveForm union (normalization.md:158) — native arc/line classification for compaction arc-fit (K33)
Spec/capability          ←  csharp:Rasm/Domain/stats          # [WIRE]: Stat.Of Welford public quantile surface + StatContext.Tolerance; Distribution.Of stays kernel-internal (K18)
Kinematics/machine       ←  csharp:Rasm/Parametric            # [WIRE]: MotionInterpolation one-slerp owner — never a second slerp site (K19; the PUBLIC front is Processing/intent VectorIntent.PoseCase)
Additive/production      ←  csharp:Rasm/Analysis/select       # [SHAPE]: Faces/Curves.Draft axis-ranked face decomposition — the orientation objective (K20)
Nesting/nfp              ←  csharp:Rasm/Processing/flatten    # [PROJECTION]: ChartAtlas unrolled UV islands + DistortionReceipt — inbound true-shape part feed
Nesting/nfp              ←  csharp:Rasm/Parametric/develop    # [PROJECTION]: DevelopmentReceipt isometric strips — the SECOND true-shape part feed (Rasm/ARCHITECTURE.md:114); Forming/sheet flat patterns are the THIRD
Process/physics          ←  csharp:Rasm.Materials/Properties  # [WIRE]: conductivity/specific-heat/density as raw doubles — AEC-peer boundary (Properties/properties#MATERIAL_PROPERTY_CATALOGUE)
Posting/program          →  csharp:Rasm/Drawing/pack          # [WIRE]: toolpath PackKind committed-motion residency (K7) — a ledger row, never a Fabrication encoder
```

## [03]-[FAULT_REGISTRY]

`FabricationFault` is ONE `[Union]` on `FaultBand.Fabrication = 2700` (the `Rasm.Element` registry row). Layer 1 (2701-2710) is FROZEN: wire codes retained per the build-ON law and the landed artifact-index persisted-decode constraint; every legacy arm retypes its bare `string Detail` to a typed payload; the non-contiguous per-folder spread (Nesting 2701/2709/2710) is the recorded frozen exception. Layer 2 (2711-2729) allocates folder-grouped across two growth tiers (2711-2722 · 2723-2729). Layer 3 (2730-2746) allocates folder-grouped under the 15-cluster partition — Forming and Joining mint their clusters here; a receipts-only page (estimation, audit, report, manufacturability) mints NO arm. Folder = fault-cluster is mechanically checkable within each tier. `OpenLoop` 2704 is the one cross-cutting arm (`FabConcern` carries the `form` row). The next free offset is 2747; a new concern is one arm on its folder's block, never a new band.

| Folder | Blocks | Arms |
|---|---|---|
| `Process/` | 2700 owner + cross-cut · 2730 | `OpenLoop` 2704 · `RoutingInfeasible` 2730 `(UInt128, DerivationStage)` |
| `Nesting/` | 2701, 2709, 2710 (frozen) · 2736 | `NoFit` · `StockOverflow` · `Nest` · `RemnantStale` 2736 `(ContentKey)` |
| `Kinematics/` | 2702, 2714 | `Unreachable` · `AxisSingularity` (payload retyped onto the shared `MachineAxis`) |
| `Geometry2D/` | 2703 | `KerfCollision` (the kerf owner's; curves routes kernel `DegenerateInput`) |
| `Toolpath/` | 2705-2707, 2713, 2723 · 2732-2734 | `InadmissiblePair` · `Gouge` · `Collision` · `SampleStalled` · `PartitionDegenerate` · `WireTaperExceeded` 2732 · `LinkBlocked` 2733 · `BevelUnsupported` 2734 |
| `Additive/` | 2708, 2715, 2716, 2725 · 2735 | `NonManifoldSlice` · `VoxelFault` · `OrientationInfeasible` · `Unsupported3mfExtension` · `SupportUnbuildable` 2735 |
| `Ingress/` | 2711 | `IngressTranslation` (one polymorphic arm; `SourceLocus` now four loci incl. `ElementNode`) |
| `Tooling/` | 2712, 2724 · 2731 | `MachinabilityUnknown` · `NoToolForOp` · `WearEstimateUnfit` 2731 |
| `Fixturing/` | 2717, 2726, 2727 · 2737 | `SetupInfeasible` · `DatumLineageBroken` · `ClampOnMachinedFace` · `AssemblyPrecedenceCyclic` 2737 |
| `Posting/` | 2718, 2719, 2728 | `DialectUnsupported` · `ProgramParse` · `BlockCapExceeded` (optimization compaction routes 2728; no tier-3 arm) |
| `Verify/` | 2720 (+2706 verify-time) · 2738-2739 | `ProbeOvertravel` · `EnvelopeExceeded` 2738 · `SimulatedOvertravel` 2739 (both on `MachineAxis`); estimation/audit are receipts-only |
| `Spec/` | 2721, 2722, 2729 | `ToleranceUnsatisfiable` · `CapabilityShortfall` · `StackupExceeded` (all QUEUED-page arms; manufacturability is receipts-only) |
| `Documentation/` | reserved (empty) | projection routes kernel `GeometryFault.ProjectionFault` 2436-2439; traveler/report are content-keyed assembly with no fault producer |
| `Forming/` | 2740-2743 | `UnfoldInfeasible` 2740 · `BendSequenceInfeasible` 2741 · `TonnageExceeded` 2742 · `MinBendRadiusViolated` 2743 |
| `Joining/` | 2744-2746 | `WeldAccessBlocked` 2744 · `HeatInputExceeded` 2745 · `WpsUnqualified` 2746 `(EssentialVariable, double)` |

## [04]-[CROSS_PACKAGE]

Every machine-consumable egress artifact mints its content key through the CONFIRMED `ContentHash.Of` seed-zero entry (the landed kernel law); the Fabrication egress discriminant is the LOCAL thirteen-row `EgressKind` axis, FEDERATED to the Persistence `ArtifactKind` rows at the content-key boundary — never a strata-crossing type reference and never a second same-named mint.

| Seam (Fabrication side) | Direction | Counterpart |
|---|---|---|
| `Nesting/stock → Rasm.Compute` | produces — TWO-SIDED, landed | `NestYield.WasteAreaMm2` — Compute decodes `ElementQuantity.WasteAreaM2`/`NestWasteM2` under the frozen `NestWasteArea` key (SI m², `Rasm.Compute/ARCHITECTURE.md:90`); `Nesting/remnant` reuse REDUCES the same wire value (recorded-only); the quantity-bag lowering rides the `FabricationProjector` |
| `Nesting/stock ← Rasm.Element` | consumes | `MaterialId` — the seam material a cut part is cut from |
| `Nesting/nfp ← Rasm.Compute` | gated consume (Compute-side) | DRL nest-policy score as an injected `Func<NoFitPolygon, PartTransform, double>` delegate column — strata-correct; the gate closes only when Compute lands its side |
| `Posting/program → Rasm.Persistence` | durable-row ENROLLMENT | `cutprogram` `ArtifactKind` Growth-law row on the LANDED `Query/cache#ARTIFACT_BLOB_INDEX` owner — lands with the program page |
| `Nesting/{nfp, remnant} → Rasm.Persistence` | durable-row ENROLLMENT | `placement`/`remnant` `ArtifactKind` rows — nfp owns the MINT, remnant the LIFECYCLE; one `ContentKey`, no second enrollment |
| `{Verify/removal, Documentation/traveler, Additive/production} → Rasm.Persistence` | durable-row ENROLLMENT | `cli`/`threemf`/`nc1`/`stock-snapshot`/`traveler` `ArtifactKind` rows via `ContentHash.Of` — each lands with its egress page |
| `{Forming/sheet, Forming/brake, Joining/weld, Additive/scanpath, Process/derivation} → Rasm.Persistence` | durable-row ENROLLMENT | `flat-pattern`/`bend-program`/`weld-plan`/`scan-vectors`/`plan` `ArtifactKind` rows — each rides its landed egress page |
| `{Tooling/magazine, Tooling/wear, Verify/probing} ← Rasm.AppHost` | decoded telemetry | MTConnect `-Common` model slice — the LANDED AppHost owners (`Wire/livewire.md` `MtconnectLane`→`ExternalValue` decode); magazine tool-life FIRST consumer, wear condition-RUL SECOND, probing measured-feature THIRD; transport never crosses |
| `Documentation/projection → Rasm.AppUi/Render` | produces | `HiddenLineResult` receipt — supersession insulated AT the receipt |
| `Ingress/profile ← Rasm.Bim/Exchange` | shared read pin | ACadSharp read codec; the two-format WRITE leg is the AppUi ACadSharp leg |
| `Ingress/element ← {Rasm.Element, Rasm.Materials, Rasm.Bim}` | consumes | `ElementGraph.Bake` + composition/connection/detail reads at the string-key boundary; `Rasm.Bim` `Semantics/connection.md:13-16` names this detailer the consumer |
| `Fabrication → Rasm.Element/Projection` | REALIZED contract | `FabricationProjector : IElementProjection` — the one app-wired `Seq<IElementProjection>` registration row (`Process/derivation`); quantity-bag lowering carries the frozen `NestWasteArea` key |
