# [FABRICATION_TASKLOG]

Open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — with `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <present only on a BLOCKED or gated card; the exact observable that flips it actionable — a catalog row landing, a member query returning evidence, a package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart card — cross-folder as `pkg` `[SLUG]` or a same-folder prerequisite `[SLUG]`, prefixed follows/precedes/mirrors when build order is load-bearing>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
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

[HATCH_EDGE_SPELLING]-[QUEUED]: Resolve the nested hatch boundary-edge member spellings blocking hatch admission.
- Capability: exact leaf-member spellings for the `BoundaryPath.Edge` line, circular-arc, elliptic-arc, and polyline edge kinds answer whether each lowers under the OCS frame law with bulge preserved.
- Shape: verified spellings extend `.api/api-acadsharp.md` and unblock the hatch admission rows on the profile page.
- Unlocks: `[HATCH_BOUNDARY_INGRESS]` moves to QUEUED with its landing rows pinned.
- Anchors: `Ingress/profile.md`, `.api/api-acadsharp.md`, IDEAS `[HATCH_BOUNDARY_INGRESS]`.
- Route: `tools.assay api query --key acadsharp` over the `BoundaryPath.Edge` nested types, then the catalog extension.
- Atomic: one catalog extension and one card unblock.

[RECEIPT_PORT_KERNEL_TYPES]-[QUEUED]: Fabrication's receipt seam names the kernel causal-frame owners.
- Capability: receipt emission composes kernel-owned identity, tenancy, envelope, and sink types, so the L2 page names no app-platform declaration.
- Shape: `libs/csharp/Rasm.Fabrication/.planning/Process/telemetry.md` `[03]` — the `ReceiptSinkPort`/`CorrelationId`/`TenantContext`/`ReceiptEnvelope` seam mentions re-anchor to the kernel capsule spellings, and the `[02]` fence's `TelemetrySource.Fabrication.Key` composition re-anchors to the capsule package-identity row it cannot reach at `Rasm.AppHost`.
- Unlocks: the strata inversion at the fabrication receipt seam dissolves; the AppHost seam mirror stays byte-consistent.
- Anchors: `libs/csharp/.planning/RULINGS.md` causal-frame row; kernel `Domain/telemetry.md`.
- Ripple: follows `Rasm` `[CAPSULE_EXTENSION_MINTS]`.
- Atomic: seam-spelling re-anchors.

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

[SCHEDULE_BAG_FOLDS]-[QUEUED]: Pin the schedule derivation folds over the realization bags.
- Capability: bar-bending, weld-map, and stud-layout fold rows, each naming its realization-bag inputs and deliverable shape; the deliverable wire registers at the `Rasm.Materials` `[FABRICATION_SCHEDULE_WIRE]` counterpart in the same change — folds landed with the wire unregistered leave this task open.
- Shape: fold rows on `libs/csharp/Rasm.Fabrication/.planning/Documentation/report.md`; projector-fact reads per `libs/csharp/Rasm.Fabrication/.planning/Process/derivation.md`.
- Unlocks: `[SHOP_SCHEDULE_DERIVATION]` lands deliverable by deliverable.
- Anchors: `DetailSchema.Realization` seam bags, `FabricationProjector`, IDEAS `[SHOP_SCHEDULE_DERIVATION]`.

[ENGINE_SPAN_FOLDS]-[BLOCKED]: Land long-solve span brackets after the diagnostics catalog admits their exact member family.
- Capability: nest, simulation, scanpath, and probe folds share one listener-gated, fault-stamping trace bracket.
- Shape: one package span vocabulary and one AppHost adapter; no kernel-local provider.
- Unlocks: exemplar and profile correlation on the same trace rail.
- Anchors: `Process/telemetry.md#[06]-[SPANS]`, `Process/telemetry.md#[09]-[RESEARCH]`, IDEAS `[FABRICATION_SPAN_SPINE]`.
- Arms: exact diagnostics members are absent from both applicable catalogs; arm when `libs/csharp/.api/api-diagnostics-metrics.md` catalogs them and `libs/csharp/Rasm.AppHost` lands the bracket adapter.
- Atomic: one catalog family, one adapter, and four fold bindings.

[OBSERVATION_PAGE_MINT]-[BLOCKED]: Complete the provider decode adapter against cataloged MTConnect observation members.
- Capability: provider rows map exhaustively into `MachineObservationIngress`; wear, fleet, and engagement consumers remain bound to `MachineObservations`.
- Shape: one AppHost adapter and one provider-neutral package ingress.
- Unlocks: measured consumers receive verified machine state without transport coupling.
- Anchors: `Kinematics/observation.md#[02]-[MACHINE_OBSERVATION]`, `Kinematics/observation.md#[03]-[RESEARCH]`, IDEAS `[MACHINE_TELEMETRY_DECODE]`.
- Arms: observation-model members are absent from both applicable catalogs; arm when `libs/csharp/Rasm.Fabrication/.api/api-mtconnect-net-common.md` catalogs them and `libs/csharp/Rasm.AppHost` lands the adapter.
- Atomic: one catalog family and one adapter.

[BENCH_CASE_ROSTER]-[BLOCKED]: Mint accepted receipts for every solver benchmark row before a measured route consumes one.
- Capability: branch cases produce durable receipts, AppHost judges them, and the accepted projection enters `ProbeRoute.Measured`.
- Shape: `AcceptedBenchmarkClaim` closes the package boundary over the roster and durable receipt key.
- Unlocks: measured clearance claims carry evidence instead of claim-key possibility.
- Anchors: `Toolpath/guard.md#[02]-[GUARD]`, `Toolpath/guard.md#[03]-[RESEARCH]`, IDEAS `[SOLVER_BENCHMARK_CORPUS]`.
- Arms: case producers and claim projection are absent; arm when `tests/csharp/_benchmarks` mints every case and `libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md` projects accepted receipts.
- Atomic: one case family and one claim adapter.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[ENGINE_FACT_CASE]-[COMPLETE]: `FabricationFact.Engine` case with solver/phase dimensions fans nest, skeleton, setup, scan, probe (`AlignmentReceipt.Iterations`), and bend-search (`BendSequenceReceipt`) rows onto `rasm.fabrication.engine.steps`; counters ride settled receipt evidence.
[HOOK_POINT_ROSTER]-[COMPLETE]: `FabricationHooks` roster with modality columns landed at `Process/telemetry.md#[07]-[HOOK_ROSTER]`; the run spine fires every point per `Process/owner.md#[03]-[RUN_FOLD]`.
[SLO_ROW_FAMILY]-[COMPLETE]: `FabricationSlos` five rows landed at `Process/telemetry.md#[08]-[SLO_ROWS]`; the fleet-stale row's `measured=false` selector matches its emitted tag.
[PERFORMANCE_HORIZON_REFRESH]-[COMPLETE]: `MachinePerformance.Of(MachineObservations, ratedPowerKw, declared, prior)` refreshes measured rows under `PerformanceHorizon`; `PerformanceBaseline` supplies honest cold-start performance and quality ratios, and matching falls back to declared OEE.
[MEASURED_LOAD_CEILING]-[COMPLETE]: one `EngagementLimit.MeasuredLoad` row over the demand-carried `LoadWindow`; `EngagementSolution.Binding` unchanged.
[STORE_SLOT_ROWS]-[COMPLETE]: `store.fabrication.<domain>.<verb>` rows landed as `RemnantSlots`, `FleetSlots`, `MagazineSlots`, and `CapabilitySlots` on their owning pages; the Persistence registry's contributed span already reserves the family at the counterpart.
[DELIVERY_FACT_CASE]-[COMPLETE]: `ProgramDelivery` carries upload custody, `TravelerAmendment.Released` requires its verified receipt, and `Delivery.ProgramKind` avoids the polymorphic `kind` discriminator while its fan arm projects the roster row.
[NFP_MEMO_KEYS]-[COMPLETE]: `PairMemo` memoizes pair polygons under `PairTable.Key` content identities through the runtime-carried `HybridCache`; hit and miss counts settle on `NestEvidence` and fan as engine rows.
[FABRICATION_FACT_RAIL]-[COMPLETE]: `Process/telemetry.md` landed the fact union, instrument roster, contributor port, projection fan, and classification rows; `FabricationRuntime` carries the `FabricationTap` port and the AppHost seam is mirrored at `[03]-[SEAMS]`.
