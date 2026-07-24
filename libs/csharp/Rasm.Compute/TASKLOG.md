# [COMPUTE_TASKLOG]

Open and closed work for measured execution, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup and the residual live-host probes whose owner shape is complete.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
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

[SELECTION_FOLD]-[QUEUED]: Land the model-selection fold on the estimator lane — information criteria, hyper-parameter paths, and the ranked candidate verdict.
- Capability: information criteria over fitted likelihoods, candidate `EstimatorPolicy` path evaluation (penalty strength, kernel width, cluster count), and a chooser folding `Validate` scores into one ranked verdict with per-candidate evidence.
- Shape: a selection member on `EstimatorFold` beside `Fit`/`Predict`/`Validate` in `libs/csharp/Rasm.Compute/.planning/Stats/estimator.md`; candidates are policy rows.
- Unlocks: `[STATS_MODEL_SELECTION]` realized; the C# half of the graduation-evidence selection discipline.
- Anchors: `EstimatorFold.Validate` k-fold and forward-chain scoring (landed), `EstimatorPolicy` admitted ranges, `ValidationReport` carrier.

[SELECTION_EXACT_ACCUMULATION]-[QUEUED]: Accumulate large-n log-likelihood sums for information criteria in extended precision.
- Capability: criterion sums over building-scale sample counts accumulate through the `PeterO.Numbers` `EFloat`/`EContext` carrier so double cancellation never flips a candidate ranking.
- Shape: an accumulation detail inside the selection fold in `libs/csharp/Rasm.Compute/.planning/Stats/estimator.md`.
- Unlocks: defensible criterion deltas at screening scale; the admitted `EFloat` surface gains its first consumer.
- Anchors: folder `.api` `api-petero-numbers.md` `EFloat`/`EContext`/`ERounding` rows.
- Atomic: one accumulation detail on the selection fold.

[QUANTILE_SEAM_AWARENESS]-[QUEUED]: Monitor and kernel quantile owners name each other's charter across the seam.
- Capability: the operational P² owner and the kernel's batch quantile owner each state the split, so neither page reads as the other's gap.
- Shape: one seam clause on `libs/csharp/Rasm.Compute/.planning/Stats/monitor.md` `[02]` citing the kernel exact small-sample `Distribution.Of` as the distinct batch owner.
- Unlocks: the branch three-formed quantile refusal holds with zero mutually-unaware prose.
- Anchors: `libs/csharp/.planning/RULINGS.md` streaming-quantile row; `libs/csharp/Rasm/.planning/Domain/stats.md` policy-row line.
- Ripple: mirrors `Rasm` `[QUANTILE_SEAM_AWARENESS]`.
- Atomic: one clause.

[SHARD_PARTITION_FOLD]-[QUEUED]: Partition one solve into per-node sub-blocks over the remote-grpc farm through the job graph.
- Capability: a shard-partition fold deriving per-node jobs from the factorization block structure, scheduling each shard through `JobGraph` dependency dispatch onto `remote-grpc` hops, and merging shard results under the existing deadline budget.
- Shape: a partition fold on `libs/csharp/Rasm.Compute/.planning/Runtime/scheduling.md` job-graph dispatch; `NodeSelection` ranks shard placement.
- Unlocks: `[SOLVER_FARM_SHARDS]` execution half; modal and buckling solves exceeding one node.
- Anchors: `JobGraph` content-digest node keys, `NodeSelection.Select` rotation/load/warm tiers, `Runtime/admission#SUBSTRATE_AXIS` warm-affinity routing.

[SHARD_EVIDENCE_ROWS]-[QUEUED]: Stamp shard count, node placement, and merge evidence onto the solve receipt rail.
- Capability: shard evidence as `Solve`/`Factorization` receipt fields and one shard instrument row the projection fan folds.
- Shape: receipt field additions and one `InstrumentRow` in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: shard-grain observability for farm capacity planning.
- Anchors: `ComputeInstrumentFan.Project` compile-broken switch, curated-aggregate boundary law.
- Atomic: receipt fields and one instrument row.

[RULE_POPULATION_DERIVATION]-[QUEUED]: Derive `RuleGrounding` populations from the concrete `ElementGraph` node classes.
- Capability: a node-class selector instantiating `ComplianceRule` templates over every matching graph member, verdict/witness/unsat-core keyed per element.
- Shape: a grounding derivation on `libs/csharp/Rasm.Compute/.planning/Solver/satisfy.md` consuming the graph the assessment spine routes; caller-supplied rows stay the manual lane.
- Unlocks: `[SOLVER_ELEMENT_QUANTIFIED_RULES]` derivation half; whole-building code audits naming exact failing elements.
- Anchors: `ComplianceRule`/`RuleGrounding` name@element tracking literals (landed), assessment-spine per-node fact routing.

[RULE_COVERAGE_PROOF]-[QUEUED]: Prove grounding exhaustiveness — every matching node-class member instantiated or the audit refuses.
- Capability: a population fold comparing derived groundings against the node-class census and landing a typed coverage fact.
- Shape: a coverage proof beside the derivation in `libs/csharp/Rasm.Compute/.planning/Solver/satisfy.md`.
- Unlocks: audit verdicts carry a completeness guarantee, not a best-effort sample.
- Anchors: `ElementGraph` node-class census, typed unknown-verdict precedent on the satisfy owner.
- Atomic: one coverage fold and fact.

[ROUTING_ROW_SHAPE]-[QUEUED]: Land the routing row — typed problem shape, solve, and status mapping.
- Capability: `RoutingModel`/`RoutingIndexManager` behind one `OptimizerKind` row over typed nodes, arcs, capacity/time-window `RoutingDimension` columns, and vehicles; `RoutingSearchStatus.Types.Value` maps onto the fault rail; `Optimization` receipt evidence.
- Shape: one exact-lane row in `libs/csharp/Rasm.Compute/.planning/Solver/optimizer.md` under the disposal law the circulation natives set.
- Unlocks: `[OPTIMIZER_ROUTING_LANE]` core.
- Anchors: folder `.api` `api-ortools.md` solver#ROUTING rail, `OptimizerKind` row law, `MaxFlow`/`MinCostFlow` disposal precedent.

[ROUTING_SEARCH_POLICY]-[QUEUED]: Pin the routing search-policy rows.
- Capability: `RoutingSearchParameters` first-solution and metaheuristic choices as policy rows with time-limit columns, never call-site knobs.
- Shape: policy rows beside the routing row in `libs/csharp/Rasm.Compute/.planning/Solver/optimizer.md`.
- Unlocks: reproducible routing solves; policy participates in receipt evidence.
- Anchors: `FirstSolutionStrategy.Types.Value` catalog row, policy-row precedent across the optimizer lane.
- Atomic: policy rows on one owner.

[BASIS_VERDICT_COSIGN]-[QUEUED]: Co-sign the `DesignBasis` re-cut of the design-check verdict vocabulary the structural consumer reads.
- Capability: the Materials basis axis renames the `SectionCapacity` case family; the `(DesignCode, LimitState)` capacity table and the `MemberCheck` carriers re-read the re-cut vocabulary in the same pass, so consumer and owner never hold two spellings.
- Shape: verdict-vocabulary alignment rows on `libs/csharp/Rasm.Compute/.planning/Analysis/structural.md#DESIGN_CHECK`; the re-cut itself lands Materials-side.
- Unlocks: EC3/EN 1994/EN 1996 basis rows flow through the standing capacity table without a parallel verdict family.
- Anchors: `DesignCode`/`LimitState` SmartEnum rows, `SectionCapacity`/`MemberCheck` carriers, the Materials capacity rail.
- Ripple: `Rasm.Materials` `[DESIGN_BASIS_AXIS]`.
- Atomic: verdict-vocabulary alignment on one section.

[ENERGY_RESULTS_WIRE]-[QUEUED]: Mint the typed energy-results receipt wire — zone and space result rows keyed by the `EnergyArtifact` content key.
- Capability: the `SqlFile` result read emits a typed receipt — annual and peak loads, comfort hours, EUI per zone and space — keyed by the `EnergyArtifact` content key, the record the Bim results-admission fold consumes; `SqlFile` decode stays Compute's per the standing simulation ruling.
- Shape: one receipt record and emit row on `libs/csharp/Rasm.Compute/.planning/Analysis/energy.md#SIMULATION_RUN` beside `ReadResults`.
- Unlocks: results survive the run directory — Bim annotates zones and re-exports Psets from the receipt.
- Anchors: `EnergySimulation.Run` fact stream, the `SqlFile` readers, `AssessmentSink` content-keyed landing.
- Ripple: `Rasm.Bim` `[ENERGY_RESULTS_ANNOTATION]`.

[PARALLEL_BUDGET_BINDINGS]-[BLOCKED]: Optimizer population parallelism binds to the sealed CPU budget.
- Capability: `Optimizer.Optimize` — already sealing `ParallelTaskExecutor.MaxThreads` from `CpuBudget.Workers` — runs over the verified parallel population carrier.
- Shape: the `TplPopulation` carrier replacing the verified `Population` carrier at the optimizer owner.
- Unlocks: budget-governed parallel genetic search over a verified carrier.
- Anchors: `api-geneticsharp.md`; the `CpuBudget.Workers` seal.
- Arms: `api-geneticsharp.md` gains the complete `TplPopulation` constructor declaration.

[SIGNAL_CAPSULE_COMPOSE]-[QUEUED]: Receipt surface composes the kernel signal capsule — spec, burn algebra, causal frame — and the alert-route vocabulary un-collides.
- Capability: instrument specs, burn windows, and receipt-seam types arrive kernel-owned; the folder keeps only its IaC-descriptor and receipt-projection specifics, and the page/ticket routing token stops sharing the platform severity ladder's name.
- Shape: `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md` — the folder spec record and the `CanonicalBurn` constants re-cut over the kernel owners; `AlertSeverity` renames to `AlertRoute` with its rows.
- Unlocks: one alerting algebra across AppUi tiles, Compute IaC rows, and AppHost health rules; the severity-ladder name collision dissolves.
- Anchors: `libs/csharp/.planning/RULINGS.md` instrument-spec and burn-rate rows; kernel `Domain/telemetry.md`.
- Ripple: follows `Rasm` `[CAPSULE_EXTENSION_MINTS]`; mirrors `Rasm.AppUi` `[SIGNAL_CAPSULE_COMPOSE]`.

[STRUCTURAL_ROW_STATICS]-[QUEUED]: Structural attribute-bag reads compose the Element-declared row statics.
- Capability: the analysis reader keys the seam bag through owner-declared names, so a spelling divergence between writer and reader becomes uncompilable.
- Shape: `libs/csharp/Rasm.Compute/.planning/Analysis/structural.md` — the folder `StructuralAnalysis` `PropertyName` statics retarget to the Element-declared rows.
- Unlocks: `Rasm.Element` `[DETAIL_SCHEMA_READER_PROVISION]` — reader adoption completes the single-owner key space.
- Anchors: `libs/csharp/.planning/RULINGS.md` seam-bag custody row; `Rasm.Element` `Properties/property.md` `PropertyName` owner.
- Ripple: follows `Rasm.Element` `[DETAIL_SCHEMA_READER_PROVISION]`; mirrors `Rasm.Bim` `[READER_ROWS_RECONCILE]`.
- Atomic: one statics retarget.

[PROTO_PACKAGE_SPELLING]-[QUEUED]: Spell the proto package declaration at the wire owner so cross-runtime ends project the fully-qualified service names.
- Capability: the one wire-contract owner carries the complete service identity — package segment and service names — so a peer runtime projects the fully-qualified name from the mint instead of hard-coding it.
- Shape: the proto `package` line on `libs/csharp/Rasm.Compute/.planning/Runtime/wire.md` `[02]-[PROTO_VOCABULARY]` beside the service roster.
- Unlocks: the python geometry serve end projects its service-name rows from the C# mint under the single-writer law.
- Anchors: the `[PROTO_VOCABULARY]` service roster; the estate single-writer measure-authority row.
- Atomic: one declaration line.

[BROKER_INGEST_ROWS]-[BLOCKED]: Broker subscription, NATS decode, and W3C activity restoration join the settled MQTT decode.
- Capability: the ingest half of the sensor loop verifies over required-tier member rows; drives from IDEAS `[TWIN_SENSOR_INGEST]`.
- Shape: required-tier member rows on the broker ingest owners beside the settled typed MQTT message decode.
- Unlocks: IDEAS.md [TWIN_SENSOR_INGEST] — verified broker crossings on every ingest arm.
- Anchors: the MQTTnet, NATS.Net, and diagnostics catalogs.
- Arms: complete MQTTnet, NATS.Net, and diagnostics catalogs and direct broker project references.

[SIGNAL_ADMISSION_LOOP]-[BLOCKED]: A verified broker pump feeds `WorkLane.CaptureIngest`.
- Capability: `TwinLoop` — typed envelope admission, serialized window updates, once-consumed recalibration edges, all settled — closes its admission loop over a verified pump; drives from IDEAS `[TWIN_SENSOR_INGEST]`.
- Shape: the admit-sink binding from the settled broker pump onto `WorkLane.CaptureIngest`.
- Unlocks: IDEAS.md [TWIN_SENSOR_INGEST] — the admission loop closes end to end.
- Anchors: `TwinLoop`; `WorkLane.CaptureIngest`; the `[MQTT_SUBSCRIPTION]` and `[BROKER_NATS]` research rows.
- Arms: `[MQTT_SUBSCRIPTION]` or `[BROKER_NATS]` settles and its admit sink binds.

[ARROW_DATASET_SCHEMA]-[BLOCKED]: `DoeDataset` admits a `RecordBatch` projection over verified Arrow members.
- Capability: the batch half of the lake egress verifies against required-tier Arrow member rows; drives from IDEAS `[DOE_LAKE_EGRESS]`.
- Shape: required-tier Apache.Arrow member rows and the direct project reference beside the settled `OnFront` carrier.
- Unlocks: IDEAS.md [DOE_LAKE_EGRESS] — DOE batches project typed onto the Arrow wire.
- Anchors: `DoeDataset.OnFront`; the Apache.Arrow catalog tiers.
- Arms: the Arrow catalog completes and the direct `Apache.Arrow` reference lands.

[FLIGHT_PUSH_SEAM]-[BLOCKED]: DOE batches push over a verified Persistence Flight port.
- Capability: the push half of the lake egress verifies against the reciprocal port and Flight SQL member rows; drives from IDEAS `[DOE_LAKE_EGRESS]`.
- Shape: the push seam binding the DOE batch onto the reciprocal Persistence port.
- Unlocks: IDEAS.md [DOE_LAKE_EGRESS] — DOE truth lands on the durable columnar plane.
- Anchors: `Rasm.Persistence` `[PERS_L1]` Flight SQL serving; the `Apache.Arrow.Flight.Sql` catalog tiers.
- Arms: the reciprocal Persistence port lands and both required catalog tiers complete.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[HOOK_POINT_ROSTER]-[COMPLETE]: five-point roster landed in `.planning/Runtime/receipts.md#HOOK_POINTS` (`ComputeHookRail.Live`, `HookId.Create` four-segment ids, one payload type and modality per point) with the admit/dispatch thread on `.planning/Runtime/admission.md#SUBSTRATE_AXIS`; `rasm.compute.solve.iteration` is Replay depth 256, not Observe.
[HOOK_FAULT_ISOLATION]-[COMPLETE]: `.planning/Runtime/receipts.md#HOOK_POINTS` boundary law composes the kernel capsule fork-shield — subscriber faults park as `IsolatedFault` rows on the roster evidence cell — with the bounded replay buffer under the progress cadence gate and telemetry-as-tap.
[GOVERNOR_UTILIZATION_FOLD]-[COMPLETE]: `.planning/Runtime/scheduling.md#CPU_BUDGET` `ResourceGovernor.Steer` folds `UtilizationSample` values into the reserve-and-memory-scale re-resolve, returns no fact for a steady sample, and emits one `Governor` fact for each budget or spill transition; `JobGraph` seals the current effective memory limit onto each `JobRun` per invocation.
[COST_VECTOR_ROWS]-[COMPLETE]: `.planning/Runtime/receipts.md#COST_LEDGER` `CostPolicy.Admit` rate rows price elapsed, token, byte, and remote axes into the `CostVector` monoid with the `rasm.compute.cost.units` roster row.
[TENANT_PARTITIONED_FOLDS]-[COMPLETE]: `.planning/Runtime/receipts.md#FOLD_PROJECTIONS` `Journal` joins the envelope tenant; `TenantCosts`/`TenantRouteCosts`/`TenantFacts` and `ChargebackDataset.Of` fold the partitioned ledger.
[DESCRIPTOR_PROJECTION]-[COMPLETE]: `.planning/Runtime/receipts.md#DASHBOARD_DESCRIPTOR` `ComputeDescriptors.Panels` derives from the primary `ReceiptSurface.Specs` roster (not the mounted `Instruments` table), thresholds from the explicit spec column.
[SLO_ALERT_ROWS]-[COMPLETE]: `.planning/Runtime/receipts.md#DASHBOARD_DESCRIPTOR` lands solve-nonconvergence, remote-call-failure, backpressure-drop, and twin-anomaly `SloAlertRow` values over `CanonicalBurn` multiwindow pairs.
[PROFILE_ARTIFACT_UNION]-[COMPLETE]: `.planning/Runtime/receipts.md#BENCHMARK_CLAIMS` `ProfileArtifact` (ChromeTrace/BenchmarkExport/EpContext) replaces the loose strings on `ModelRun.Profile` and `BenchmarkClaim.Artifacts`; `.planning/Model/inference.md` `RunOps.Profile` mints `ChromeTrace` from the admitted row's `ContentAddress` and `ProfilingStartTimeNs`.
[SPAN_PROFILE_CORRELATION]-[COMPLETE]: `.planning/Runtime/receipts.md#TELEMETRY_PROJECTION` boundary states the shared-`TraceId` join to the root-span `pyroscope.profile.id` stamp with zero Compute OTel reference.
[MONITOR_PAGE_SPINE]-[COMPLETE]: `.planning/Stats/monitor.md` lands `StreamMonitor` EWMA/P²/`FittedModel` detector capsules, `MonitorChannel` extraction rows, and `MonitorLane` advance/observe folds; detector verdicts call the fitted estimator instead of fabricating window-only results.
[MONITOR_VERDICT_SEAM]-[COMPLETE]: `MonitorVerdict.Receipt` mints the `Drift` receipt case the `ComputeInstrumentFan` counts onto `rasm.compute.monitor.breaches`, and `MonitorLane.AsDetector` satisfies the `Solver/clash` injected-detector slot.
