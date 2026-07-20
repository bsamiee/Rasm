# [FABRICATION_TASKLOG]

Open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — with `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[SPECIALIZED_ENVELOPE_ADMISSION]-[QUEUED]: Enforce `SpecializedToolpathEnvelope` admission at its owner and route every consumer through the admitted rail.
- Capability: `SpecializedToolpathEnvelope` mints only through an admission factory that folds payload validity, so an invalid envelope is unrepresentable and the public primary constructor retires.
- Shape: a private primary constructor with a static `Fin<SpecializedToolpathEnvelope>` admission factory on the owner; Toolpath `wire`, `bevel`, `link`, `motion`, Posting `program`, `dialect`, and Verify `simulate` construct through the factory and drop their local `payload.IsValid` revalidation branches.
- Unlocks: one admission seam for every specialized-toolpath consumer, collapsing six duplicated advisory revalidations into the owner's single fold.
- Anchors: the value-object admission pattern the corpus already carries; the `SpecializedToolpathEnvelope` owner in `Process/owner.md`; the consumer pages holding direct `new SpecializedToolpathEnvelope(...)` construction; IDEAS `[SPECIALIZED_TOOLPATH_EGRESS]`.

[LOT_INSTANCE_CONTENTION]-[QUEUED]: Widen `PlannedStep` with `MachineInstance` identity so lot scheduling reserves real stations.
- Capability: `PlannedStep` carries its assigned `MachineInstance`, and the `LotOf` fold emits per-instance reservations beside lead, critical-path, and slack evidence.
- Shape: derivation consumes fleet availability windows per assigned instance; contention resolves inside the lap-phased fold, never a post-pass.
- Unlocks: instance-true capacity routing, promise intervals, and dispatch order.
- Anchors: `Process/derivation.md`, `Kinematics/fleet.md`, IDEAS `[FABRICATION_FINITE_CAPACITY]`.

[FLEET_CALENDAR_RECURRENCE]-[QUEUED]: Pin fleet shift and maintenance recurrence on the NodaTime civil-calendar types.
- Capability: `ShiftCalendar` recurrence and maintenance-exception rows carry `AnnualDate` yearly recurrences and `YearMonth` capacity horizons instead of raw month and day integers.
- Shape: generated calendars and availability windows derive from the typed rows, and capacity-horizon evidence states its month identity.
- Unlocks: unambiguous recurring windows feeding finite-capacity reservations.
- Anchors: `Kinematics/fleet.md`, `libs/csharp/.api/api-nodatime.md` `YearMonth` and `AnnualDate`, IDEAS `[FABRICATION_FINITE_CAPACITY]`.
- Atomic: two typed calendar rows on the fleet page.

[PERFORMANCE_HORIZON_REFRESH]-[QUEUED]: Close the measured-performance refresh from the decoded observation slice.
- Capability: `MachinePerformance` refreshes from admitted `MachineObservation` evidence under its `PerformanceHorizon` freshness fold, displacing nameplate rows.
- Shape: fleet ranking and estimation projection read the measured row when fresh and fall back beside the stale-match count.
- Unlocks: evidence-based routing, costing, and reliability decisions.
- Anchors: `Kinematics/fleet.md`, `Verify/estimation.md`, `Kinematics/observation.md`, IDEAS `[FABRICATION_MEASURED_FLEET]`.

[ENGINE_FACT_CASE]-[QUEUED]: Land the engine fact case over counters the solver receipts already hold.
- Capability: one `FabricationFact` engine case with solver and phase dimensions covering nest branch-and-bound nodes, ICP iterations, engagement-walk steps, and bend-search expansions.
- Shape: one case row, one `Of` projection, one `rasm.fabrication.engine.*` instrument row, one fan arm; counters ride existing receipt evidence, never per-iteration writes.
- Unlocks: solver-regression detection and per-engine cost attribution without a profiler attach.
- Anchors: `Process/telemetry.md`, `Nesting/nfp.md`, `Verify/probing.md`, `Toolpath/skeleton.md`, `Fixturing/setups.md`, IDEAS `[ENGINE_COUNTER_FACTS]`.

[HATCH_EDGE_SPELLING]-[BLOCKED]: Resolve the nested hatch boundary-edge member spellings blocking hatch admission.
- Capability: exact leaf-member spellings for the `BoundaryPath.Edge` line, circular-arc, elliptic-arc, and polyline edge kinds answer whether each lowers under the OCS frame law with bulge preserved.
- Shape: verified spellings extend `.api/api-acadsharp.md` and unblock the hatch admission rows on the profile page.
- Unlocks: `[HATCH_BOUNDARY_INGRESS]` moves to QUEUED with its landing rows pinned.
- Anchors: `Ingress/profile.md`, `.api/api-acadsharp.md`, IDEAS `[HATCH_BOUNDARY_INGRESS]`.
- Tension: blocker question — which concrete edge types nest under `BoundaryPath.Edge` and what are their member names; route — `tools.assay api query` over the installed ACadSharp assembly.
- Atomic: one catalog extension and one card unblock.

[MARKING_ENTITY_ROWS]-[QUEUED]: Pin the profile-marking admission rows and their transform lowering.
- Capability: `ProfileMarking` rows admit insert attributes through `Insert.HasAttributes`, text, and multiline-text content; array and nested instances lower through `Insert.ApplyTransform(Transform)`, and arc marks center through `Arc.GetCenter`.
- Shape: marking entities retain source identity through census and lowering, and traveler and posting consumers read the typed marks.
- Unlocks: part marks, heat numbers, and shop tags on travelers and posted programs.
- Anchors: `Ingress/profile.md`, `.api/api-acadsharp.md`, `Documentation/traveler.md`, IDEAS `[PROFILE_ANNOTATION_MARKS]`.

[SPLINE_NATIVE_SAMPLING]-[QUEUED]: Sample splines through the owner's parametric evaluator, never a hand-rolled de Boor.
- Capability: profile spline lowering walks `Spline.TryPointOnSpline(double t, out XYZ)` at the chord-precision knob, and the witnessed lowering row records the sampler.
- Shape: one sampler row on the profile curve lowering; evaluator failure at a parameter lands as repair evidence.
- Unlocks: faithful spline contours with owner-verified sampling.
- Anchors: `Ingress/profile.md`, `.api/api-acadsharp.md` `Spline.TryPointOnSpline`, IDEAS `[HATCH_BOUNDARY_INGRESS]`.
- Atomic: one sampler row on the profile page.

[TOOLCHANGE_ELAPSED_ROW]-[QUEUED]: Price magazine traverse as a typed per-change evidence row.
- Capability: `ToolChange.Elapsed` derives from slot index distance and arm swing; estimation consumes the row and simulation advances its modal clock by the same value.
- Shape: one derivation on the magazine owner, one estimation evidence row, one simulate clock consumption.
- Unlocks: magazine-aware quoting and cycle truth.
- Anchors: `Tooling/magazine.md`, `Verify/estimation.md`, `Verify/simulate.md`, IDEAS `[MAGAZINE_CHANGE_TIME_TO_ESTIMATION]`.
- Atomic: one evidence row with two consumers.

[STABILITY_WORD_INTERSECTION]-[QUEUED]: Intersect the chatter recommendation with controller and power limits at posting.
- Capability: `StabilityReceipt.Recommend` selects the highest-margin stable spindle point; posting intersects it with dialect and power limits while motion carries it on `CutStrategy`.
- Shape: one recommendation consumption row in motion and one intersection row in optimization, with refusal evidence when no stable point survives the limits.
- Unlocks: stable emitted spindle words and physics-backed feed optimization.
- Anchors: `Tooling/cuttingdata.md`, `Toolpath/motion.md`, `Posting/optimization.md`, IDEAS `[STABILITY_SPEED_SELECTION_IN_POSTING]`.

[SHARED_EDGE_MEASURE]-[QUEUED]: Publish the collinear-overlap measure and score it during placement.
- Capability: linking publishes its collinear-overlap measure as a reusable owner, and `NestObjective` gains one shared-edge weight and evidence field consuming it.
- Shape: measure mints once in linking; placement scoring reads it without coupling owners.
- Unlocks: lower pierce count and cut length at equal packing yield.
- Anchors: `Nesting/linking.md`, `Nesting/nfp.md`, IDEAS `[COMMON_LINE_AFFINITY]`.

[DISTORTION_SOURCE_FAMILY]-[QUEUED]: Close the distortion-source family behind one displacement receipt.
- Capability: joining thermal load, preload, and fixture release fold as `DistortionSource` cases into a per-member displacement field receipt.
- Shape: assembly tolerance chains and setup datum-transfer budgets consume the one receipt.
- Unlocks: post-weld position error and fixture-release planning.
- Anchors: `Joining/sequence.md`, `Fixturing/assembly.md`, `Fixturing/setups.md`, IDEAS `[FIXTURING_DISTORTION]`.

[WELD_ARC_FIT_GATE]-[QUEUED]: Ledger the Joining-to-Geometry2D edge and land the circular-emission gate.
- Capability: transported torch frames satisfying an arc-fit gate emit one circular move with rotation sense, and non-circular runs keep the linear chain.
- Shape: ARCHITECTURE strata ledger gains the legal Joining-to-Geometry2D consumption edge before the arc-fit owner is consumed.
- Unlocks: faithful circumferential deposits without chord-error dependence.
- Anchors: `Joining/weld.md`, `Geometry2D/arcs.md`, `ARCHITECTURE.md` strata ledger, IDEAS `[ORBITAL_ARC_DEPOSIT_PATH]`.

[CHAIN_CONTRIBUTION_ROWS]-[QUEUED]: Carry ranked stackup contribution onto failed characteristics.
- Capability: quality evidence retains stack method and ranked `ChainReceipt` contribution rows, so a failed characteristic names its dominating feature variation.
- Shape: one evidence widening on the quality record, no second vocabulary.
- Unlocks: corrective-action routing and targeted capability studies.
- Anchors: `Spec/tolerance.md`, `Documentation/report.md`, IDEAS `[STACKUP_CONTRIBUTION_ON_QUALITY_RECORDS]`.
- Atomic: one evidence widening.

[FRAME_SYMBOL_SEAM]-[BLOCKED]: Resolve the app-stratum drafting seam name carrying the frame symbols.
- Capability: `FeatureFrameReceipt.Annotation` symbol rows feed drawings, travelers, exchange, and reports through one layout-free law once the receiving seam owner is named.
- Shape: projection consumes the symbol rows while the render tier keeps layout, and the seam ledger names one APP-stratum owner.
- Unlocks: `[GDT_ANNOTATION_AS_SPEC_EVIDENCE]` lands with its consumer seam pinned.
- Anchors: `Spec/tolerance.md`, `Documentation/projection.md`, IDEAS `[GDT_ANNOTATION_AS_SPEC_EVIDENCE]`.
- Tension: blocker question — which APP-stratum owner receives `ProjectionReceipt` and the drafting symbols, `Rasm.App` or `Rasm.AppUi`; route — the AppUi drafting seam page `libs/csharp/Rasm.AppUi/.planning/Render/drafting.md` and this folder's ARCHITECTURE seam ledger.

[MOVE_ORIENTATION_PAYLOAD]-[QUEUED]: Widen the motion atom with continuous tool-frame and contact payload.
- Capability: `Move` gains continuous orientation and contact; indexed 3+2 stays on `SurfaceFrame`, and the consumer census spans surface swarf, machine solve, posting, and swept guard.
- Shape: one atom widening with per-consumer lowering rows; the dropped swept-solid guard ruling re-opens on landing with typed refusal for unsupported axes.
- Unlocks: typed multi-axis motion through the CAM-to-post chain.
- Anchors: `Process/owner.md`, `Toolpath/surface.md`, `Toolpath/guard.md`, IDEAS `[TOOLPATH_ORIENTED_MOTION_ATOM]`.

[CUTTER_FORM_COMPOUND_ROWS]-[QUEUED]: Make compound cutter lowering total over explicit form evidence.
- Capability: `CutterForm` carries compound family, major length, and secondary angle, and `OpenCamCutterKind` dispatches every catalogued constructor without inferring form from coincident dimensions.
- Shape: one form widening and one dispatch-table completion.
- Unlocks: faithful BullCone and compound cutter lowering.
- Anchors: `Process/owner.md`, `Toolpath/surface.md`, IDEAS `[OPENCAM_COMPOUND_CUTTER_EVIDENCE]`.
- Atomic: one form widening and dispatch completion.

[DIRECTIVE_ATOM_LOWERING]-[QUEUED]: Admit dwell, oriented stop, and spindle sync as motion, never typed failure.
- Capability: one directive payload on the motion atom lowers through turning and posting without a parallel command family.
- Shape: turning directive rows become admitted motion, and dialect owns executable spelling or annotation.
- Unlocks: controller-neutral directive generation.
- Anchors: `Process/owner.md`, `Toolpath/turning.md`, `Posting/dialect.md`, IDEAS `[FABRICATION_MOTION_DIRECTIVE_ATOM]`.

[PACK_ARC_ATOM_SEAM]-[QUEUED]: Preserve analytic arcs across geometry publication.
- Capability: `Post.Publish` emits analytic circular segments once the kernel `PackOp.Toolpath` carrier accepts an arc-bearing path atom; matching arc events fail typed until it lands.
- Shape: one typed-failure guard now and one emission arm on kernel acceptance, with center and sense staying digest-bearing channels.
- Unlocks: published geometry retains controller arc centers and senses.
- Anchors: `Posting/program.md`, IDEAS `[ANALYTIC_ARC_TOOLPATH_PACKING]`.
- Tension: the kernel counterpart lands the arc-bearing atom in `Rasm`; this task guards until it does.

[EROSION_WIRE_ROUTING]-[QUEUED]: Route erosion boundary passes through the wire owner.
- Capability: `EngagementPolicy` carries `WirePolicy`, and `Cam.Generate` sends erosion boundary passes into `WireEdm.Generate`, replacing cutter-radius compensation with spark-gap, overburn, taper-guide, and retention law.
- Shape: one policy row and one dispatch rerouting with wire-specific refusal evidence.
- Unlocks: total erosion routing.
- Anchors: `Toolpath/motion.md`, `Toolpath/wire.md`, IDEAS `[EROSION_CONTOUR_ROUTES_WIRE_OWNER]`.
- Atomic: one policy row and one dispatch arm.

[PARTITION_DENSITY_MAP]-[QUEUED]: Derive partition policy from target areal density.
- Capability: boundary area maps density to pitch, relaxation, and separation on `PartitionStrategy`, and retained cell areas with Lloyd residuals close the inverse.
- Shape: one derivation fold replacing preset constants.
- Unlocks: parameterized stipple and engrave generation.
- Anchors: `Toolpath/partition.md`, IDEAS `[PARTITION_DENSITY_CLOSURE]`.
- Atomic: one derivation fold on the partition page.

[MEASURED_LOAD_CEILING]-[QUEUED]: Rebind engagement ceilings from decoded spindle load.
- Capability: `EngagementLimit` gains a measured-load row consuming the observation slice, and `EngagementSolution.Binding` still names the governing bound.
- Shape: one limit row with no receipt change.
- Unlocks: adaptive clearing justified by observed load.
- Anchors: `Toolpath/skeleton.md`, `Process/physics.md`, `Kinematics/observation.md`, IDEAS `[ENGAGEMENT_FEEDBACK]`.
- Atomic: one limit row.

[QUANTITY_ARROW_COLLAPSE]-[QUEUED]: Collapse folder-local length parsers onto the atoms quantity arrow.
- Capability: one caller-fault-parameterized arrow converts unit-bearing text to canonical machining scalars, and the wire, link, and bevel parser sites collapse onto it.
- Shape: one atom owner and three call-site collapses.
- Unlocks: shared unit policy and new quantity families without wrappers.
- Anchors: `Process/owner.md`, `Toolpath/wire.md`, `Toolpath/link.md`, `Toolpath/bevel.md`, IDEAS `[DIMENSIONAL_ADMISSION_ATOM]`.
- Atomic: one arrow and three call-site collapses.

[PRECEDENCE_SAFE_TWO_OPT]-[QUEUED]: Refine linked tours against routed transition cost.
- Capability: a bounded two-opt or Or-opt stage reorders only swaps whose graph in-degrees stay satisfied, and `LinkReceipt` carries the improvement delta after re-routing swapped pairs.
- Shape: one refinement stage after routing under a bounded swap budget.
- Unlocks: tours optimized against routed geometry instead of Euclidean proxy cost.
- Anchors: `Toolpath/link.md`, IDEAS `[LINK_TOUR_REFINEMENT]`.

[HOLD_POINT_RELEASE]-[QUEUED]: Gate traveler advance on satisfied hold-point evidence.
- Capability: a `HoldPoint` family over inspection stages carries release attestations for hold, witness, review, and surveillance points.
- Shape: traveler step release consumes satisfied hold evidence, never rendered plan text.
- Unlocks: customer and notified-body release gating before material advances.
- Anchors: `Documentation/report.md`, `Documentation/traveler.md`, `Joining/procedure.md`, IDEAS `[INSPECTION_TEST_PLAN]`.

[ENGINE_SPAN_FOLDS]-[QUEUED]: Wrap the four long-solve entries in scope-bound spans.
- Capability: an `ActivitySource` family under `TelemetrySource.Fabrication` wraps nest solve, simulation run, scanpath derivation, and probe fit; phase transitions ride span events with `HasListeners()` gating tag cost.
- Shape: one span per solve at the folds already minting facts; the AppHost root registers the source and the branch `PyroscopeSpanProcessor` joins flame graphs to the same traces.
- Unlocks: exemplar click-through from a histogram bucket to the exact solve trace.
- Anchors: `Process/telemetry.md`, `libs/csharp/.api/api-pyroscope-opentelemetry.md`, IDEAS `[FABRICATION_SPAN_SPINE]`.

[BENCH_CASE_ROSTER]-[QUEUED]: Enumerate the solver benchmark cases and close the measured-claim loop.
- Capability: gated bench cases for NFP placement, ICP probe fit, skeleton offset, and bend search, each folding one `BenchmarkReceipt` judged against the durable claim index, so `ProbeRoute.Measured` resolves its `BenchmarkKey` against minted claims.
- Shape: cases ride the branch benchmark project tier, and this package's csproj never references the bench harness.
- Unlocks: regression detection with host-evidence honesty and benchmark-authorized parallel probing.
- Anchors: `Toolpath/guard.md` `ProbeRoute.Measured`, IDEAS `[SOLVER_BENCHMARK_CORPUS]`.

[HOOK_POINT_ROSTER]-[QUEUED]: Enumerate the fabrication hook points with modality columns.
- Capability: `rasm.fabrication.<domain>.<point>` roster rows — admission, stage advance, egress mint, verify verdict, delivery — each declaring veto, observe, and replay modality and its payload receipt.
- Shape: roster beside the tap on the telemetry page, registered through the runtime instance at composition.
- Unlocks: `[FABRICATION_HOOK_RAIL]` lands with its rows pinned.
- Anchors: `Process/telemetry.md`, `Process/owner.md`, IDEAS `[FABRICATION_HOOK_RAIL]`.

[OBSERVATION_PAGE_MINT]-[QUEUED]: Mint the machine-observation page and rebind its three consumers.
- Capability: `Kinematics/observation.md` owns the `MachineObservation` vocabulary over verified MTConnect observation-model members, and wear observations, fleet refresh, and measured-load rows rebind onto it.
- Shape: one new page as the fourth Kinematics file; member spellings verify via `tools.assay api query` over MTConnect.NET-Common and extend `.api/api-mtconnect-net-common.md`.
- Unlocks: `[MACHINE_TELEMETRY_DECODE]` lands with one decode truth.
- Anchors: `Kinematics/fleet.md`, `Tooling/wear.md`, `.api/api-mtconnect-net-common.md`, IDEAS `[MACHINE_TELEMETRY_DECODE]`.

[STORE_SLOT_ROWS]-[QUEUED]: Pin the durable shop-state slot rows and their owning receipts.
- Capability: `store.fabrication.<domain>.<verb>` rows for remnant inventory, fleet horizons, magazine state, and capability history, each naming its read and write receipt.
- Shape: one seam row per owning page, with the Persistence counterpart registering the slots.
- Unlocks: `[SHOP_STATE_SLOTS]` lands with slot custody stated at both ends.
- Anchors: `Nesting/remnant.md`, `Kinematics/fleet.md`, `Tooling/magazine.md`, `Spec/capability.md`, IDEAS `[SHOP_STATE_SLOTS]`.

[SLO_ROW_FAMILY]-[QUEUED]: Derive the SLO rows from the instrument roster.
- Capability: SLO rows naming instrument, objective, window, and burn-rate policy for wear-critical rate, violation budget, gouge budget, stale-match ratio, and cycle envelope.
- Shape: data rows beside `FabricationInstruments`, consumed by the alert rail and the dashboard compile leg from the same source.
- Unlocks: `[FABRICATION_SLO_PACK]` lands as one derivation.
- Anchors: `Process/telemetry.md`, IDEAS `[FABRICATION_SLO_PACK]`.

[DELIVERY_FACT_CASE]-[QUEUED]: Land the delivery receipt and its fact case.
- Capability: a delivery receipt binding `PostImage` content key, transfer digest, controller acknowledgment, and operator attestation, with the Robots remote channel carrying the robot arm.
- Shape: one boundary row in the cell owner, one delivery demand row at dialect egress, one fact case with roster row and fan arm.
- Unlocks: `[PROGRAM_DELIVERY_RECEIPTS]` lands with chain-of-custody evidence.
- Anchors: `Kinematics/cell.md`, `Posting/dialect.md`, `Process/telemetry.md`, `.api/api-robots.md`, IDEAS `[PROGRAM_DELIVERY_RECEIPTS]`.

[NFP_MEMO_KEYS]-[QUEUED]: Content-key the NFP pair memo and count its hits.
- Capability: NFP pair polygons memoize under keys folded from exact pair geometry and separation policy, and hit and miss counts join the engine evidence.
- Shape: per-runtime memo tier over the branch hybrid-cache surface, with the durable tier federating at the Persistence cache seam.
- Unlocks: `[SOLVER_MEMO_CACHE]` lands on its hottest lane.
- Anchors: `Nesting/nfp.md`, `libs/csharp/.api/api-hybrid-cache.md`, IDEAS `[SOLVER_MEMO_CACHE]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[FABRICATION_FACT_RAIL]-[COMPLETE]: `Process/telemetry.md` landed the fact union, instrument roster, contributor port, projection fan, and classification rows; `FabricationRuntime` carries the `FabricationTap` port and the AppHost seam is mirrored at `[03]-[SEAMS]`.
