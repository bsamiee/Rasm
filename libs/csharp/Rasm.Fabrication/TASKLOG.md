# [FABRICATION_TASKLOG]

Open and closed work for `Rasm.Fabrication`, distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` open, `[COMPLETE]`/`[DROPPED]` closed — with `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[WELD_DWELL_MS_EGRESS]-[QUEUED]: The weave dwell's second-to-millisecond crossing carries an adjudicated unit posture at its controller-word egress.
- Capability: the controller-bound dwell word converts through a declared route — typed `UnitsNet.Duration` egress, or a stated frozen-wire bare multiply — instead of an unadjudicated `* 1000.0`.
- Shape: one verdict at `Joining/weld.md:1503` (`(int)Math.Round(band.Weave.EdgeDwellS * 1000.0)`); the page already imports UnitsNet, so the typed form costs one expression if the integer-millisecond field is not a frozen controller wire.
- Unlocks: the last undispositioned F2/F3 boundary site in the Joining family closes.
- Anchors: the digest/wire freeze ruling at `[04]-[STRUCTURE]`; the controller word's own field declaration decides frozen-versus-typed.
- Atomic: one site, one verdict.

[CUTTING_DEPTH_RATIO_UNITS]-[QUEUED]: The cutting-data depth ratio proves its numerator unit before any typed conversion lands.
- Capability: `depth * 1000.0 / TargetDepth.Millimeters` at `Tooling/cuttingdata.md:895`/`:905` either becomes a typed `Length` ratio or records the structural negative — the numerator's unit is proven at `Depth(...)`'s own definition, never assumed from the call sites.
- Shape: prove the producer's output unit, then land the typed egress or the recorded negative at both sites.
- Unlocks: the two remaining unproven scale literals in Tooling close under the R2 rule.
- Anchors: the `Mass.FromGrams` precedent at `Tooling/magazine.md:762`; the proven-reading requirement (`ToolMeasure.Weight` grams proof pattern).
- Atomic: one proof, two sites.

[IMPLICIT_PROGRESS_THREAD]-[QUEUED]: The run fold's progress sink reaches the PicoGK egress legs through the slicing chain.
- Capability: a fabrication run's `FabricationRuntime.Progress` sink reports the additive stack's longest legs — vectorize, CLI write, VDB convert — instead of going dark between the `Dispatched` and `Sealed` stage boundaries.
- Shape: `Additive/slicing.md` `Layers` gains a defaulted `Option<IProgress<double>>` threaded through the `@implicit` arm's `Voxel` into the landed `Implicit.Cli(op, progress)` parameter; the dispatch chain from `Process/owner.md` passes `runtime.Progress` at its slicing call.
- Unlocks: the three PicoGK `IProgress?` provider parameters already composed at `Additive/implicit.md` receive a live sink end to end.
- Anchors: `FabricationRuntime.Progress` (`Process/owner.md:1508`); `Implicit.Cli(ImplicitOp, Option<IProgress<double>>)` (`Additive/implicit.md:784`); the `RunStage` band whose Dispatched-to-Sealed gap this fills.
- Atomic: one defaulted parameter threaded through one chain.

[PLINESEG_INTERSECTION_ADOPT]-[QUEUED]: Compose the newly catalogued arc-aware segment-intersection family where the toolpath pages hand-classify crossings.
- Capability: segment crossings classify through the provider's own verdict vocabulary — overlapping arcs included — instead of a page-local classification.
- Shape: the `Rasm.Fabrication/.planning/Toolpath` pages whose folds test segment crossings compose the `PlineSegIntersection.Intersect` family and its `PlineSegIntrKind` verdicts per the widened `api-cavaliercontours.md` rows.
- Unlocks: the `OverlappingArcs` verdict a bool cannot spell reaches every crossing consumer, and a new crossing class is a provider verdict row, never a page predicate.
- Anchors: `libs/csharp/Rasm.Fabrication/.api/api-cavaliercontours.md` intersection primitives with their producing facades and verdict enums (landed this pass); `Toolpath/link.md` the settled `PlineSegIntersection.Intersect` fence.

[FIXTURE_FAULT_CASE_HOMING]-[QUEUED]: Seat the fixture-admission fault case on its union owner so the `[Union]` dispatch reaches it and its band code stays disjoint.
- Capability: one `FabricationFault` type carries every case, so the generated `Switch`/`Map` is exhaustive over the whole family and each case's band offset is unique across the package.
- Shape: `Process/faults.md` `[ERRORS]` gains the `FixtureInadmissible(FixturingWitness Witness)` case at the free offset 54, its prelude gains the witness namespace, and `Fixturing/workholding.md` drops the second `public abstract partial record FabricationFault` block whole; the witness-homing question — witnesses beside their fault owner, as `EquipmentWitness` and `DeriveWitness` already sit — settles in the same pass.
- Unlocks: every `FabricationFault` consumer dispatches one exhaustive family, and the Fixturing pages' roughly forty-five `FabricationFault.FixtureInadmissible` construction sites bind a case that compiles.
- Anchors: the partial seated at `Rasm.Fabrication.Fixturing` against the union's `Rasm.Fabrication.Process` home, so the two declarations are distinct types and the nested case's three-argument base call resolves against a constructor its enclosing type never declares; offset 53 already spent by `bend-search-budget-exceeded`, making 54 the free slot on a 54-case roster; `docs/laws/topology.md` `[01]` row [13] binds band disjointness; branch `RULINGS.md` `[02]` package-receipt-union row binds one kind vocabulary per union.
- Atomic: one case relocation with its prelude row and one deleted block.

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
- Arms: arm when one APP-stratum owner claims the annotation rows — the landed `HiddenLineResult` seam onto `Rasm.AppUi`, or an app shell outside `libs/csharp` the seam ledger then names.
- Route: `libs/csharp/Rasm.AppUi/.planning/Render/drafting.md` for the drafting seam's declared consumption, then this folder's `ARCHITECTURE.md` seam ledger for the edge that claim obliges.

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

[HATCH_EDGE_SPELLING]-[COMPLETE]: the route resolved on the assay rail — `Hatch.BoundaryPath` publishes `Edges : ObservableCollection<Edge>` over `Line`(`Start`/`End : XY`), `Arc`(`Center`/`Radius`/`StartAngle`/`EndAngle`/`CounterClockWise`), `Ellipse`(`MajorAxisEndPoint`/`RadiusRatio`), `Polyline`(`Vertices : List<XYZ>` carrying bulge in Z), and `Spline`, each answering `EdgeType` with `ToEntity()`/`PolygonalVertexes(int)`; the catalogue carries every row and the lowering arms already compose them.
[OBSERVATION_PAGE_MINT]-[COMPLETE]: the arming condition landed at the branch tier — `libs/csharp/.api/api-mtconnect.md` carries both namespace partitions with `Rasm.AppHost` bound to CONNECTIVITY and this folder to the ISO-13399 CUTTING-TOOL slice, the dual-homed folder catalogue is deleted, and the README row moved to `## [03]-[SUBSTRATE_PACKAGES]` under `[MACHINE_CONNECTIVITY]`; AppHost already carries the decode adapter, its `MachineObservationWire` crossing, and the `machine.observations` instrument, so `Kinematics/observation.md` stays provider-neutral by design rather than by omission and the `[LOCAL_ADMISSION]` partition law forecloses this folder reaching the connectivity half.
[DERIVATION_ROW_KEY_CUSTODY]-[COMPLETE]: `Process/derivation.md` mints every bag key through `Row`, its own one-line composition of the seam owner's `PropertyCategory.Fabrication.Row`, so the three call-site `PropertyName.Create(row.Key)` spellings are gone and this package's vocabulary lands inside a partition `Rasm.Element` blesses rather than in the shared key space; the Boundary states a bare `PropertyName.Create` at any write site as the deleted form.
[MARKING_CONSUMER_BINDING]-[COMPLETE]: the binding was unreachable, not merely unwritten — `FabricationInput` dropped markings at admission, so both consumers gained their read only once the input carried `Markings` beside `Profiles` with `Tags` deriving through the ingress owner's newly-public `ProfileImport.TagsOf`; `TravelerMarks` lands a `TravelerSection.Marks` carrying keyed rows, free-text runs, and every keyed row contradicting the declared `TravelerIdentity`, and posting's `Prologue` opens every program on a sorted verbatim comment block so an operator verifies material against the sheet the program was posted from.
[MARKING_ENTITY_ROWS]-[COMPLETE]: the admission rows and transform lowering landed at `Ingress/profile.md` over verified `Insert.Attributes -> SeqendCollection<AttributeEntity>`, `Insert.ApplyTransform(Transform)`, and `Arc.GetCenter`; `Insert.HasAttributes` catalogued but deliberately unbranched, since concatenating an empty collection is already total and a guard carries no decision; `[MARKING_CONSUMER_BINDING]` re-cuts the consumer half, since neither traveler nor posting named a marking read.
[SPLINE_NATIVE_SAMPLING]-[COMPLETE]: `SplineLoop` walks `Spline.TryPointOnSpline(double, out XYZ)` first and falls back to the bulk tessellator then the fit-point rebuild, with `SplineSampler` and `ProfileRepair.Sampled` recording which won on the new `ProfileLowered.Repairs` channel; the evaluator assigns `XYZ.NaN` and swallows every exception, so the bool is the only safe gate and `t == 1.0` nudges down one epsilon.
[ENGINE_FACT_CASE]-[COMPLETE]: `FabricationFact.Engine` fans nest, skeleton, setup, scan, probe (`AlignmentReceipt.Iterations`), and bend-search (`BendSequenceReceipt`) rows onto `rasm.fabrication.engine.steps`; the `EnginePhase` roster carries each phase's owning `FabricationEngine` row, so the fact stores one discriminant, the arm derives the solver dimension through the generated keyed admission, and every solver and phase literal is a vocabulary row.
[ENGINE_SPAN_FOLDS]-[COMPLETE]: `FabricationTrace.Scopes` landed at `Process/telemetry#SPANS` — `libs/csharp/.api/api-diagnostics-activity.md` already met the arming condition, and the kernel `SpanBand` owns the listener gate, the `using` open, and the fail-leg status, so the assumed AppHost adapter never mints. Probing, simulate, and scanpath drop the `EngineSpan` they composed: the bracket is `band.Traced(FabricationEngine.<lane>, key, body)` over a nullable band, and phase events are `FabricationTrace.Mark(span, EnginePhase.<row>)`.
[HOOK_POINT_ROSTER]-[COMPLETE]: `FabricationHooks` roster with modality columns landed at `Process/telemetry.md#[07]-[HOOK_ROSTER]`; the run spine fires every point per `Process/owner.md#[03]-[RUN_FOLD]`.
[SLO_ROW_FAMILY]-[COMPLETE]: `FabricationDescriptors` landed at `Process/telemetry#BOARD_PACK` — `Objective` rows over `Sli.Partition`, `Sli.Latency`, and the `FleetLoad` `Sli.Saturation` headroom row beside the panel roster, the pack carrying its `Admit` proof; stringly breach selectors, the folder-local `SliKind`/`BurnLane` twins, and every good-half counter are deleted, so each share partitions the population its arm stamps a verdict on and the wear share reads `MaintenanceDisposition.ServiceableKeys`.
[PERFORMANCE_HORIZON_REFRESH]-[COMPLETE]: `MachinePerformance.Of(MachineObservations, ratedPowerKw, declared, prior)` refreshes measured rows under `PerformanceHorizon`; `PerformanceBaseline` supplies honest cold-start performance and quality ratios, and matching falls back to declared OEE.
[MEASURED_LOAD_CEILING]-[COMPLETE]: one `EngagementLimit.MeasuredLoad` row over the demand-carried `LoadWindow`; `EngagementSolution.Binding` unchanged.
[STORE_SLOT_ROWS]-[COMPLETE]: `store.fabrication.<domain>.<verb>` rows landed as `RemnantSlots`, `FleetSlots`, `MagazineSlots`, and `CapabilitySlots` on their owning pages; the Persistence registry's contributed span already reserves the family at the counterpart.
[DELIVERY_FACT_CASE]-[COMPLETE]: `ProgramDelivery` carries upload custody, `TravelerAmendment.Released` requires its verified receipt, and `Delivery.ProgramKind` avoids the polymorphic `kind` discriminator while its fan arm projects the roster row.
[NFP_MEMO_KEYS]-[COMPLETE]: `PairMemo` memoizes pair polygons under `PairTable.Key` content identities through the runtime-carried `HybridCache`; hit and miss counts settle on `NestEvidence` and fan as engine rows.
[FABRICATION_FACT_RAIL]-[COMPLETE]: `Process/telemetry.md` landed the fact union, instrument roster, contributor port, projection fan, and classification rows; `FabricationRuntime` carries the `FabricationTap` port and the AppHost seam is mirrored at `[03]-[SEAMS]`.
