# [FABRICATION_IDEAS]

Forward concept pool for `Rasm.Fabrication`. Each idea is a card — a bracketed slug leader with a few bullets stating the capability, what it unlocks, and the gap or technique it draws on. An idea drives one or more `TASKLOG.md` tasks; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[FABRICATION_FINITE_CAPACITY]-[ACTIVE]: Time-phase lot derivation binds routing instants to fleet availability, maintenance, and load-factor windows.
- Capability: `LotOf` consumes routing and availability per assigned step, drives every schedule advance, and emits reservations beside lead, critical-path, and slack evidence.
- Shape: Multi-lot load leveling and quote lead-time receipts remain on the lap-phased lot fold under operation-topology order.
- Unlocks: Capacity-aware routing, promise intervals, and shop-floor dispatch order.
- Anchors: QuikGraph `SourceFirstBidirectionalTopologicalSort`, `Kinematics/fleet.md`, `Process/derivation.md`, and `Verify/estimation.md`.
- Tension: `PlannedStep` carries machine kind without a `MachineInstance` identity, so instance contention requires widening the step owner.

[FABRICATION_MEASURED_FLEET]-[ACTIVE]: Fresh measured performance displaces nameplate cost, power, reliability, and utilization.
- Capability: `MachinePerformance` retains a `PerformanceHorizon` and observed operating evidence.
- Shape: Decoded telemetry admission and estimation projection close the feedback seam.
- Unlocks: Evidence-based routing, costing, and reliability decisions.
- Anchors: `Kinematics/fleet.md`, `Tooling/wear.md`, `Verify/estimation.md`, and `Process/telemetry.md`.

[ENGINE_COUNTER_FACTS]-[QUEUED]: Solver-internal counters join the fact rail as engine evidence.
- Capability: Nest branch-and-bound nodes, ICP iterations, engagement-walk steps, and bend-search expansions land as one engine fact case with solver and phase dimensions.
- Shape: One `FabricationFact` case over per-solver evidence rows the owning kernels already accumulate; the fan gains one arm and one `rasm.fabrication.engine.*` instrument row.
- Unlocks: Solver-regression detection and per-engine cost attribution without a profiler attach.
- Anchors: `Process/telemetry.md`, `Nesting/nfp.md`, `Verify/probing.md`, `Toolpath/skeleton.md`, and `Fixturing/setups.md`.
- Tension: Kernel folds are allocation-free hot paths, so counter accumulation must ride existing receipt evidence rather than new per-iteration writes.

[HATCH_BOUNDARY_INGRESS]-[BLOCKED]: Admit ACadSharp hatch boundary paths as profile contours.
- Capability: Each boundary path lowers edge-discriminated lines, circular arcs, elliptic arcs, and polylines into one loop under the OCS frame law.
- Shape: Arc edges preserve bulge geometry through `Arc.CreateFromBulge` rather than tessellating the filled region away.
- Unlocks: Hatched cut regions enter as geometry instead of unsupported entities.
- Anchors: `Ingress/profile.md` and the ACadSharp catalog's hatch and frame surfaces.
- Tension: Nested `BoundaryPath.Edge` leaf-member spelling remains unverified.

[PROFILE_ANNOTATION_MARKS]-[QUEUED]: Carry profile annotation content through ingress.
- Capability: `ProfileMarking` admits insert attributes, text, and multiline-text content.
- Shape: `Insert.HasAttributes` discriminates attribute capture while marking entities retain their source identity.
- Unlocks: Part marks, heat numbers, and shop tags reach traveler and posting consumers.
- Anchors: `Ingress/profile.md`, `Documentation/traveler.md`, and `Posting/program.md`.

[MAGAZINE_CHANGE_TIME_TO_ESTIMATION]-[QUEUED]: Price magazine traverse time as typed estimate evidence.
- Capability: `ToolChange.Elapsed` derives from layout index steps and arm swing instead of a flat dwell.
- Shape: Estimation consumes one per-change evidence row, and simulation advances its modal clock by the same elapsed value.
- Unlocks: Magazine-aware quoting and cycle simulation.
- Anchors: `Tooling/magazine.md`, `Verify/estimation.md`, and `Verify/simulate.md`.

[STABILITY_SPEED_SELECTION_IN_POSTING]-[QUEUED]: Carry chatter-aware spindle selection into motion and posting.
- Capability: `StabilityReceipt.Recommend` selects the highest-margin stable spindle point at the requested depth.
- Shape: Posting intersects the recommendation with controller and power limits, while motion carries it on `CutStrategy`.
- Unlocks: Stable emitted spindle words and physics-backed feed optimization.
- Anchors: `Tooling/cuttingdata.md`, `Posting/optimization.md`, and `Toolpath/motion.md`.

[COMMON_LINE_AFFINITY]-[QUEUED]: Placements whose accepted edges become shared cuts rank first.
- Capability: `NestObjective` scores shared-edge length during placement rather than discovering it after layout.
- Shape: One shared-edge weight and evidence field consume a reusable collinear-overlap measure from linking.
- Unlocks: Lower pierce count and cut length without trading away packing yield.
- Anchors: `Nesting/nfp.md` candidate contact, cut-length evidence, and `Nesting/linking.md` shared-cut editing.
- Tension: Linking must publish the overlap measure before the placement scorer can consume it without coupling owners.

[FIXTURING_DISTORTION]-[QUEUED]: Fold joining thermal load, preload, and fixture release into per-member displacement evidence.
- Capability: A closed `DistortionSource` family produces the residual field that stability and tolerance consumers require.
- Shape: Assembly tolerance chains and setup datum-transfer budgets consume one displacement receipt.
- Unlocks: Post-weld position error and fixture-release distortion planning.
- Anchors: `Joining/sequence.md`, `Fixturing/assembly.md`, and `Fixturing/setups.md`.

[ORBITAL_ARC_DEPOSIT_PATH]-[QUEUED]: Emit true circular motion for co-circular weld seam spans.
- Capability: `Weld.Pass` emits a circular move with rotation sense when transported torch frames satisfy an arc-fit gate.
- Shape: Non-circular runs retain the linear chain, and weld-pass admission accepts the circular arm.
- Unlocks: Faithful circumferential deposits for pipe positions without chord-error dependence.
- Anchors: `Process/owner.md`, `Joining/weld.md`, and `Geometry2D/arcs.md`.
- Tension: Architecture must ledger the legal Joining-to-Geometry2D consumption before the arc-fit owner is consumed.

[STACKUP_CONTRIBUTION_ON_QUALITY_RECORDS]-[QUEUED]: Publish dominant tolerance-chain contribution on as-built quality records.
- Capability: Quality evidence retains stack method and ranked contribution rows from `ChainReceipt`.
- Shape: A failed characteristic names the feature variation dominating its closure.
- Unlocks: Corrective-action routing and targeted capability studies.
- Anchors: `Spec/tolerance.md` and `Documentation/report.md`.

[GDT_ANNOTATION_AS_SPEC_EVIDENCE]-[QUEUED]: Draft feature-control frames from specification-owned symbols.
- Capability: Projection consumes `FeatureFrameReceipt.Annotation` and row symbols while retaining layout in the render tier.
- Shape: One layout-free symbol law feeds drawings, travelers, exchange, and reports.
- Unlocks: New characteristic and modifier rows reach every rendered surface without a second vocabulary.
- Anchors: `Spec/tolerance.md` and the app drafting receipt seam.

[TOOLPATH_ORIENTED_MOTION_ATOM]-[QUEUED]: Extend `Move` with a continuous tool-frame and contact payload.
- Capability: Surface swarf, contact propagation, machine solve, posting, and swept-solid guard consume one oriented atom.
- Shape: Indexed 3+2 remains on `SurfaceFrame`; only continuous orientation widens `Move`.
- Unlocks: Typed multi-axis motion through the full CAM-to-post chain.
- Anchors: `Process/owner.md`, `Toolpath/surface.md`, and `Toolpath/guard.md`.

[OPENCAM_COMPOUND_CUTTER_EVIDENCE]-[QUEUED]: Make compound cutter construction total over explicit form evidence.
- Capability: `CutterForm` carries compound family, major length, and secondary angle.
- Shape: `OpenCamCutterKind` dispatches every catalogued constructor without inferring compound form from coincident dimensions.
- Unlocks: Faithful BullCone and compound cutter lowering.
- Anchors: `Process/owner.md` and `Toolpath/surface.md`.

[FABRICATION_MOTION_DIRECTIVE_ATOM]-[QUEUED]: Carry dwell, oriented stop, and spindle synchronization on the motion atom.
- Capability: One directive payload lowers through turning and posting without a parallel command family.
- Shape: Fine-bore and turning directives become admitted motion rather than typed failures.
- Unlocks: Controller-neutral directive generation.
- Anchors: `Process/owner.md`, `Toolpath/motion.md`, `Toolpath/turning.md`, and `Posting/dialect.md`.

[SPECIALIZED_TOOLPATH_EGRESS]-[QUEUED]: Admit specialized programs without sequential-motion flattening.
- Capability: One typed envelope preserves wire guide simultaneity, bevel axis/pivot/THC/inspection rows, and link segment metrics through posting, simulation, and estimation.
- Shape: `PostSource` and evidence-consumer registries consume S0 wire payloads that preserve specialized evidence without upward Process dependencies.
- Unlocks: Authoritative specialized receipts cross canonical consumer seams.
- Anchors: `Toolpath/wire.md`, `Toolpath/bevel.md`, `Toolpath/link.md`, `Posting/program.md`, `Verify/simulate.md`, and `Verify/estimation.md`.

[ANALYTIC_ARC_TOOLPATH_PACKING]-[QUEUED]: Preserve interpreted arcs through geometry publication.
- Capability: `Post.Publish` emits analytic circular segments instead of chord-only point clouds.
- Shape: the kernel `PackOp.Toolpath` carrier gains an arc-bearing path atom; Fabrication fails typed on matching arc events while the kernel accepts only `VectorCloud.PolylineCase`.
- Unlocks: Published geometry retains controller arc centers and senses.
- Anchors: `Posting/program.md` and `Rasm/.planning/Drawing/pack.md`.

[EROSION_CONTOUR_ROUTES_WIRE_OWNER]-[QUEUED]: Route erosion boundary passes through the wire-EDM owner.
- Capability: `EngagementPolicy` carries `WirePolicy`, so spark gap, overburn, taper-guide, and retention law replace cutter-radius compensation.
- Shape: `Cam.Generate` sends erosion boundary passes into `WireEdm.Generate`.
- Unlocks: Total erosion routing with wire-specific refusal evidence.
- Anchors: `Toolpath/motion.md` and `Toolpath/wire.md`.

[PARTITION_DENSITY_CLOSURE]-[QUEUED]: Derive partition policy from target areal density.
- Capability: Boundary area maps density to pitch, relaxation, and separation policy.
- Shape: Retained cell areas and Lloyd residuals close the inverse derivation on `PartitionStrategy`.
- Unlocks: Parameterized stipple and engrave generation without preset constants.
- Anchors: `Toolpath/partition.md`.

[ENGAGEMENT_FEEDBACK]-[QUEUED]: Rebind engagement ceilings from decoded measured spindle load.
- Capability: `EngagementLimit` gains a measured-load row consuming telemetry through the existing input.
- Shape: `EngagementSolution.Binding` continues to name the governing physics bound without a receipt change.
- Unlocks: Adaptive clearing justified by observed load.
- Anchors: `Toolpath/skeleton.md`, `Process/physics.md`, and the decoded-telemetry seam.

[DIMENSIONAL_ADMISSION_ATOM]-[QUEUED]: Centralize dimensional quantity admission on the Process atoms vocabulary.
- Capability: One caller-fault-parameterized arrow converts unit-bearing text into canonical machining scalars.
- Shape: Folder-local length parsers collapse onto the atom owner.
- Unlocks: Shared unit policy and additional quantity families without wrapper multiplication.
- Anchors: `Process/owner.md`, `Toolpath/wire.md`, `Toolpath/link.md`, and `Toolpath/bevel.md`.

[LINK_TOUR_REFINEMENT]-[QUEUED]: Refine linked tours against realized obstacle-aware transition cost.
- Capability: A bounded precedence-safe two-opt or Or-opt stage reorders only swaps whose graph in-degrees remain satisfied.
- Shape: `LinkReceipt` carries improvement delta after re-entering transition routing for swapped pairs.
- Unlocks: Tours optimized against routed geometry instead of Euclidean proxy cost.
- Anchors: `Toolpath/link.md`.

[INSPECTION_TEST_PLAN]-[QUEUED]: Own the ordered inspection hold-point plan.
- Capability: A `HoldPoint` family over inspection stages carries release attestations for hold, witness, review, and surveillance points.
- Shape: Traveler step release consumes satisfied hold evidence rather than rendered plan text.
- Unlocks: Customer and notified-body release gating before material advances.
- Anchors: `Documentation/report.md`, `Joining/procedure.md`, and `Documentation/traveler.md`.

[FABRICATION_SPAN_SPINE]-[QUEUED]: Engine spans make fabrication histograms exemplar-bearing — long solves trace, and every measurement inside them links to its trace.
- Capability: an `ActivitySource` span family under the minted `TelemetrySource.Fabrication` scope identity wraps the long-running engine boundaries — nest solve, simulation run, scanpath derivation, probe fit — so TraceBased exemplars attach trace ids to the cycle-time, wear, and SPC histograms automatically.
- Shape: one span per solve at the engine entry folds that already mint `FabricationFact` evidence, phase transitions as span events, `HasListeners()` gating tag cost; per-iteration spans stay the deleted form.
- Unlocks: click-through from a latency histogram bucket to the exact solve trace; span-profile correlation joins the flame graph to the regressing solve.
- Anchors: `Process/telemetry.md` fact union and instrument roster, the AppHost exemplar law (`SetExemplarFilter(TraceBased)`), the scope-name law binding tracer and meter to one package identity.
- Tension: facts are receipts by design — spans complement the fact rail, and admission stays at the AppHost root's source roster, never a folder-local provider.

[SOLVER_BENCHMARK_CORPUS]-[QUEUED]: Solver kernels join the corpus benchmark gate — the claims `ProbeRoute.Measured` consumes finally gain a producer.
- Capability: gated benchmark cases for the NFP placement, ICP probe-fit, skeleton-offset, and bend-search kernels, each folding to one `BenchmarkReceipt` judged under `BenchmarkGate.Judge` against the durable claim the Persistence benchmark index holds.
- Shape: bench cases ride the branch benchmark project tier per the Test Stack law — BenchmarkDotNet never enters this package's csproj; `Toolpath/guard.md` `ProbeRoute.Measured` resolves its `BenchmarkKey` against the minted claims, closing the loop the route today references blind.
- Unlocks: solver-regression detection with host-evidence honesty; benchmark-authorized parallel probing backed by a real claim instead of an unmintable key.
- Anchors: AppHost `Observability/benchmarks.md` gate fold and `BenchmarkRun.Traced`, Persistence `Query/cache.md#BENCHMARK_INDEX` `BenchmarkRow` and `store.cache.benchmark`, `[ENGINE_COUNTER_FACTS]` counter evidence as the paired live signal.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[FABRICATION_FACT_RAIL]-[COMPLETE]: `Process/telemetry.md` owns the `FabricationFact` union, the `rasm.fabrication.*` instrument roster with its contributor port, the envelope projection fan, and the suite-taxonomy classification rows; emitting pages carry their kind anchors and classified members their attributes.

[KINEMATICS_CELL_PLACEMENT]-[COMPLETE]: `RobotProgram.Place` ranks batch-solved base placements over one loaded robot system with feasibility, travel, posture, peak-step, and peak-joint evidence.

[TILTED_AXIS_SWEPT_SOLID_GUARD]-[DROPPED]: Current `Move` cases carry no tool axis, so planar sweep is exact for every admitted move; reopen only when an oriented move atom lands and require typed refusal for unsupported axes.
