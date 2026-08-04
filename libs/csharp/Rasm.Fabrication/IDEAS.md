# [FABRICATION_IDEAS]

Forward concept pool for `Rasm.Fabrication`. Each idea is a card — a bracketed slug leader with a few bullets stating the capability, what it unlocks, and the gap or technique it draws on. Ideas drive one or more `TASKLOG.md` tasks; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[FABRICATION_FINITE_CAPACITY]-[ACTIVE]: Time-phase lot derivation binds routing instants to fleet availability, maintenance, and load-factor windows.
- Capability: `LotOf` consumes routing and availability per assigned step, drives every schedule advance, and emits reservations beside lead, critical-path, and slack evidence.
- Shape: Multi-lot load leveling and quote lead-time receipts remain on the lap-phased lot fold under operation-topology order.
- Unlocks: Capacity-aware routing, promise intervals, and shop-floor dispatch order.
- Anchors: QuikGraph `SourceFirstBidirectionalTopologicalSort`, `Kinematics/fleet.md`, `Process/derivation.md`, and `Verify/estimation.md`.
- Tension: `PlannedStep` carries machine kind without a `MachineInstance` identity, so instance contention requires widening the step owner.

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
- Capability: one closed `DistortionSource` family produces the residual field that stability and tolerance consumers require.
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
- Shape: each failed characteristic names the feature variation dominating its closure.
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

[DIMENSIONAL_ADMISSION_ATOM]-[QUEUED]: Centralize dimensional quantity admission on the Process atoms vocabulary.
- Capability: One caller-fault-parameterized arrow converts unit-bearing text into canonical machining scalars.
- Shape: Folder-local length parsers collapse onto the atom owner.
- Unlocks: Shared unit policy and additional quantity families without wrapper multiplication.
- Anchors: `Process/owner.md`, `Toolpath/wire.md`, `Toolpath/link.md`, and `Toolpath/bevel.md`.

[SPINDLE_SPEED_OWNER]-[QUEUED]: One owner spells the surface-speed-to-spindle law the package currently states five times.
- Capability: the cutting-speed/rpm correspondence lives on one cutting-physics owner as a forward/inverse pair over the canonical mm and m-per-minute carriers, so a diameter convention or unit change lands once.
- Shape: the pair seats on `Process/physics.md`'s cutting-physics owner; `Process/physics.md` (three sites), `Kinematics/fleet.md`, and the inverse at `Tooling/cuttingdata.md` compose it, each losing its own `Math.PI`/`1000.0` transcription.
- Unlocks: the five-copy duplication and its five conversion literals delete as a consequence; the R2 unit rule closes on the last rpm sites.
- Anchors: the five censused sites (`physics.md` three, `fleet.md:1200`, `cuttingdata.md:464`); the landed UnitsNet typed-product precedent at `Fixturing/workholding.md`.

[UNMEASURED_SUPPORT_LAYER]-[QUEUED]: The audit's unsupported-mass reading distinguishes a measured zero from a measurement never taken.
- Capability: a support-free modality reports ABSENCE on the unsupported-mass axis instead of a fabricated zero, so the recoater-strike score stops silently understating by the whole trend term on runs that never measured support.
- Shape: `Verify/audit.md` splits the collapsed guard — `layer == 0` stays a measured structural zero with its reason stated; the risk-off case carries absent measures on `LayerMetric.UnsupportedAreaMm2`/`UnsupportedMassKg`/`UnsupportedMassTrendKg` with the recoater composition reading the absence arm.
- Unlocks: honest recoater-strike likelihood on support-free modalities; the `[FORGED_ZERO]` scar closes at its one proven live leak.
- Anchors: `LayerMetric` is confined to `audit.md` (single-file consumer census, eight sites), so the receipt reshape has no cross-page ripple; `AuditDefect.RecoaterStrike` emits independently of the Support risk.

[NEST_COST_REFERENCE]-[QUEUED]: The nest objective's cost term ranks scale-free across price bases.
- Capability: every `NestObjective` term weighs a pure number, so `Balanced`'s equal weights compare honestly whatever the caller's currency basis — the cost term normalizes against a declared cost reference instead of tracking the raw price density.
- Shape: a cost-reference column on the scoring input beside the frozen `NestEvidence` record — never on the CanonicalWriter-digested carrier itself; `Nesting/nfp.md` `NestObjective.Score` divides through it.
- Unlocks: cross-run and cross-supplier nest ranking; the stated structural divergence at `NestEvidence` retires.
- Anchors: the digest-freeze ruling at `[04]-[STRUCTURE]` (the reference rides scoring input, not the frozen record); the `Yield`/`Cut`/`Remnant` terms already pure.

[LINK_TOUR_REFINEMENT]-[QUEUED]: Refine linked tours against realized obstacle-aware transition cost.
- Capability: one bounded precedence-safe two-opt or Or-opt stage reorders only swaps whose graph in-degrees remain satisfied.
- Shape: `LinkReceipt` carries improvement delta after re-entering transition routing for swapped pairs.
- Unlocks: Tours optimized against routed geometry instead of Euclidean proxy cost.
- Anchors: `Toolpath/link.md`.

[INSPECTION_TEST_PLAN]-[QUEUED]: Own the ordered inspection hold-point plan.
- Capability: one `HoldPoint` family over inspection stages carries release attestations for hold, witness, review, and surveillance points.
- Shape: Traveler step release consumes satisfied hold evidence rather than rendered plan text.
- Unlocks: Customer and notified-body release gating before material advances.
- Anchors: `Documentation/report.md`, `Joining/procedure.md`, and `Documentation/traveler.md`.

[SHOP_SCHEDULE_DERIVATION]-[QUEUED]: Shop-deliverable schedules derive from realized detail bags — bar bending schedules, weld maps, and stud layouts off the projected realization vocabulary.
- Capability: the projected `DetailSchema.Realization` bags over the registered `FabricationProjector : IElementProjection` row carry the realized Materials scalar vocabularies — schedule derivation folds them into shop deliverables without re-resolving materials.
- Shape: derivation folds on `libs/csharp/Rasm.Fabrication/.planning/Documentation/report.md` reading the projector facts of `libs/csharp/Rasm.Fabrication/.planning/Process/derivation.md`; each schedule kind one fold row.
- Unlocks: shop paperwork generates from the seam graph; the Materials realization vocabularies gain their consuming end.
- Anchors: the `FabricationProjector` registration row, the seam `DetailSchema.Realization` bags, the Documentation traveler and report owners.
- Ripple: `Rasm.Materials` `[FABRICATION_SCHEDULE_WIRE]`.

[SOLVER_BENCHMARK_CORPUS]-[BLOCKED]: Produce accepted benchmark claims before measured probe routes consume them.
- Capability: each solver case emits one durable benchmark receipt whose accepted projection authorizes the matching measured route.
- Shape: `AcceptedBenchmarkClaim` is the package boundary; AppHost owns claim projection and the branch benchmark tier owns case production.
- Unlocks: `ProbeRoute.Measured` becomes evidence-backed instead of roster-backed.
- Anchors: `Toolpath/guard.md#[02]-[GUARD]` and `Toolpath/guard.md#[03]-[RESEARCH]`.
- Arms: claim-family projection and case producers are absent; arm when `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md` maps accepted receipts and `tests/csharp/_benchmarks` mints every roster case.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[MACHINE_TELEMETRY_DECODE]-[COMPLETE]: the decode never belonged in this folder — `libs/csharp/.api/api-mtconnect.md` splits the package into CONNECTIVITY and CUTTING-TOOL partitions under the `[LOCAL_ADMISSION]` law, `Rasm.AppHost` owns provider conversion and the `MachineObservationWire` crossing, and `Kinematics/observation.md` keeps its neutral `MachineObservationIngress` with no MTConnect type crossing the package boundary.
[HATCH_BOUNDARY_INGRESS]-[COMPLETE]: the blocker was false and the capability already landed — `Ingress/profile.md` `HatchContours`/`HatchEdge` lowers all five `BoundaryPath` leaves under the hatch's own `Normal`/`Elevation` OCS frame, with `HatchArcSpans` emitting exact bulges signed by the extrusion direction and the ellipse and spline leaves riding the package's own `PolygonalVertexes(int)`; `.api/api-acadsharp.md` now carries the whole verified `Edge` roster it was blocked on.
[PROFILE_ANNOTATION_MARKS]-[COMPLETE]: `ProfileMarking` widened at `Ingress/profile.md` onto a `MarkingContent` `[Union]` — glyph, text, paragraph, and tag over one `Seq<string> Lines`, so arity is the only difference a consumer reads — with `AttributeEntity`/`CadText`/`MText` lowering arms and a `Tag` projection keying the attribute read; `Insert.Explode()` yields `Block.Entities` alone and never `Insert.Attributes`, so `Insertion` concatenates both in one ordinal space and `AttributeEntity` precedes its `TextEntity` base.
[ENGINE_COUNTER_FACTS]-[COMPLETE]: `FabricationFact.Engine` fans nest (candidates/evaluated/rejected/memo-hits/memo-misses), skeleton, setup, scan, probe ICP-iteration, and bend-search rows over settled receipts onto `rasm.fabrication.engine.steps`; brake gained `BendSequenceReceipt` so the counters ride receipt evidence.
[FABRICATION_HOOK_RAIL]-[COMPLETE]: `FabricationHooks` five-point roster on `Process/telemetry.md` fires from the `Process/owner.md` run spine — admission veto, stage advance, egress mint veto, verify verdict replay, delivery hand-off — scoped per `FabricationRuntime` instance.
[FABRICATION_SLO_PACK]-[COMPLETE]: `Process/telemetry#BOARD_PACK` carries `FabricationDescriptors` as one kernel `BoardPack` value — five `Objective` rows and the panel roster under the pack's own `Admit` proof — so window, factor, severity, budget share, and the canonical panel widget all derive from the kernel and the shop decides only target, indicator, and a deliberate reading.
[FABRICATION_SPAN_SPINE]-[COMPLETE]: `FabricationTrace.Scopes` projects one `TraceScope` per `FabricationEngine` row into the composing root's kernel `SpanBand`, so the listener gate, the typed fail-leg status, and source lifetime are the capsule's and the solve bracket is one `band.Traced` call with no folder-local source or AppHost adapter.
[PROGRAM_DELIVERY_RECEIPTS]-[COMPLETE]: `ProgramDelivery` binds image and transfer keys, controller acknowledgment, records, and classified operator attestation; `TravelerAmendment.Released` requires its verified receipt, and the collision-free `Delivery.ProgramKind` fact projects onto `rasm.fabrication.delivery.programs`.
[FABRICATION_MEASURED_FLEET]-[COMPLETE]: `MachinePerformance.Of` folds admitted observations into availability, utilization, failure spacing, and load-scaled power; unobserved performance and quality carry from the prior row or admitted `PerformanceBaseline`, and stale matching falls back to declared OEE.
[ENGAGEMENT_FEEDBACK]-[COMPLETE]: `EngagementLimit.MeasuredLoad` row scales the radial ceiling from the `LoadWindow` observed spindle-load fraction; `EngagementSolution.Binding` names the governing bound unchanged.
[SHOP_STATE_SLOTS]-[COMPLETE]: `RemnantSlots`, `FleetSlots`, `MagazineSlots`, and `CapabilitySlots` name the `store.fabrication.<domain>.<verb>` streams on their owning pages as value federation; the Persistence slot registry's contributed span mounts them at composition.
[SOLVER_MEMO_CACHE]-[COMPLETE]: `PairMemo` content-keys the NFP pair matrix on `PairTable.Key` through the runtime-carried `HybridCache` with hit/miss engine facts; a further memo lane is one content key and one `GetOrBuild` wrap on the same owner, and the durable L2 federates at the Persistence cache seam.
[FABRICATION_FACT_RAIL]-[COMPLETE]: `Process/telemetry.md` owns the `FabricationFact` union, the `rasm.fabrication.*` instrument roster with its contributor port, the envelope projection fan, and the suite-taxonomy classification rows; emitting pages carry their kind anchors and classified members their attributes.
[KINEMATICS_CELL_PLACEMENT]-[COMPLETE]: `RobotProgram.Place` ranks batch-solved base placements over one loaded robot system with feasibility, travel, posture, peak-step, and peak-joint evidence.
[TILTED_AXIS_SWEPT_SOLID_GUARD]-[DROPPED]: Current `Move` cases carry no tool axis, so planar sweep is exact for every admitted move; reopen only when an oriented move atom lands and require typed refusal for unsupported axes.
