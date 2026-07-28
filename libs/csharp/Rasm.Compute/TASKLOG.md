# [COMPUTE_TASKLOG]

Open and closed work for measured execution, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup and the residual live-host probes whose owner shape is complete.

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

[STAGE_PARTITION_EVIDENCE]-[BLOCKED]: Pin the chrome-trace node schema so the graph-partition count publishes as measured evidence.
- Capability: the partition count a stage result carries reads from a real trace parse rather than a key spelling assumed at authoring time, closing the last unmeasured column on the photo-to-PBR execution wire.
- Shape: the trace-reading warm pulse the app root injects into `Model/sessions#SESSION_CAPSULE` `Warmup`, filling `WarmEvidence.Partitions` for the bucket it ran, with the resolved row deleted from `libs/csharp/Rasm.Compute/.planning/Model/inference.md` `[06]-[RESEARCH]`.
- Unlocks: `StageResult.PartitionCount` publishes a measured value on every provider, so the specifying end's partition-bound refusal grades against observation instead of refusing every non-floor run for want of evidence.
- Anchors: `RunOps.Profile` already mints the `ChromeTrace` artifact from `EndProfiling`; `WarmEvidence` already carries the column as `Option<int>` and the stage fold already refuses an unmeasured bucket; `libs/csharp/.api/api-system-text-json.md` `JsonDocument.Parse`/`TryGetProperty`/`EnumerateArray` rows.
- Arms: one profiled `InferenceSession` run under the ORT host emitting a trace whose first node event names its category, argument bag, and provider keys verbatim.
- Route: `tools.assay bridge` opens a session with `SessionPolicy.Profiling` set, runs one pulse, calls `EndProfiling`, and reads the emitted file's leading node event.
- Ripple: mirrors `Rasm.Materials` `Appearance/neural` `StageResult.Admit` partition-bound guard.

[WEBGPU_PROVIDER_ROW]-[BLOCKED]: Admit the WebGPU execution-provider row once the pinned runtime proves it ships.
- Capability: the provider axis carries the successor accelerator for the platform whose CoreML lane upstream no longer develops, with its own option roster rather than an assumed one.
- Shape: one `ExecutionProvider` row in `libs/csharp/Rasm.Compute/.planning/Model/providers.md` `[02]-[EP_AXIS]` beside the existing accelerator rows, retiring the research row it answers.
- Unlocks: a stage request naming the `webGpu` wire key resolves to a real row instead of degrading to the floor, and the frozen stage wire's third provider value becomes reachable from one row column.
- Anchors: `ExecutionProvider.Resolve`/`Available` already answer absence without a surface move; `Accelerator` folds a row from name, affinity, gate, register delegate, and its optional `WireKey`, so admitting the row to the stage wire is one argument.
- Arms: `GetAvailableProviders()` naming a WebGPU provider on the pinned native package for `osx-arm64`, with its `OrtEpDevice.EpOptions` roster read back.
- Route: restore the pinned runtime, enumerate providers and EP devices through `uv run python -m tools.assay api`, and read the option roster off the returned device.
- Tension: the stable package may carry the EP for no platform this estate targets, in which case the row lands nowhere and the research row retires as refuted rather than answered.

[PARITY_CADENCE]-[QUEUED]: Make floor-provider parity a declared cadence rather than an every-run obligation.
- Capability: the residual an accelerated run reports comes from a stated sampling posture — every run, first run per bucket, or a fraction of runs — so a pipeline chaining several accelerated stages pays for the evidence it wants instead of a second floor session and two extra inferences on every stage it executes.
- Shape: one policy row consumed by the residual measurement in `libs/csharp/Rasm.Compute/.planning/Model/inference.md` `[04]-[STAGE_EXECUTION]`, with the unsampled result carrying an absent residual rather than a zero.
- Unlocks: an accelerated multi-stage plan runs at accelerated cost while the parity obligation still holds by measurement, and the specifying end reads a residual it can tell apart from an unmeasured one.
- Anchors: `StageRun` already brackets each lease inside the bind that took it and already treats a floor run answering itself as an identity; `Model/providers#EP_AXIS` states the measurement obligation and leaves tolerance to the consumer.
- Tension: the frozen `goldenDelta` column is a plain double, so an absent measurement needs the wire's own optionality settled at the specifying end before a sampled cadence can report honestly — a zero there reads to the parity gate as a perfect match.
- Ripple: mirrors `Rasm.Materials` `Appearance/neural` stage-result admission, which owns the residual ceiling.

[SELECTION_FOLD]-[QUEUED]: Land the model-selection fold on the estimator lane — information criteria, hyper-parameter paths, and the ranked candidate verdict.
- Capability: information criteria over fitted likelihoods, candidate `EstimatorPolicy` path evaluation (penalty strength, kernel width, cluster count), and a chooser folding `Validate` scores into one ranked verdict with per-candidate evidence.
- Shape: a selection member on `EstimatorFold` beside `Fit`/`Predict`/`Validate` in `libs/csharp/Rasm.Compute/.planning/Stats/estimator.md`; candidates are policy rows.
- Unlocks: `[STATS_MODEL_SELECTION]` realized; the C# graduation binding carries the selection discipline.
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
- Shape: receipt field additions and one `InstrumentSpec` row in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
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
- Anchors: folder `.api` `api-ortools#ENTRYPOINTS` routing rail, `OptimizerKind` row law, `MaxFlow`/`MinCostFlow` disposal precedent.

[ROUTING_SEARCH_POLICY]-[QUEUED]: Pin the routing search-policy rows.
- Capability: `RoutingSearchParameters` first-solution and metaheuristic choices as policy rows with time-limit columns, never call-site knobs.
- Shape: policy rows beside the routing row in `libs/csharp/Rasm.Compute/.planning/Solver/optimizer.md`.
- Unlocks: reproducible routing solves; policy participates in receipt evidence.
- Anchors: `FirstSolutionStrategy.Types.Value` catalog row, policy-row precedent across the optimizer lane.
- Atomic: policy rows on one owner.

[BASIS_VERDICT_COSIGN]-[QUEUED]: Co-sign the `DesignBasis` re-cut of the design-check verdict vocabulary the structural consumer reads.
- Capability: the Materials basis axis renames the `SectionCapacity` case family; the `(DesignCode, LimitState)` capacity table and the `MemberCheck` carriers re-read the re-cut vocabulary in the same pass, so consumer and owner never hold two spellings.
- Shape: verdict-vocabulary alignment rows on `Analysis/structural#DESIGN_CHECK`; the re-cut itself lands Materials-side.
- Unlocks: EC3/EN 1994/EN 1996 basis rows flow through the standing capacity table without a parallel verdict family.
- Anchors: `DesignCode`/`LimitState` SmartEnum rows, `SectionCapacity`/`MemberCheck` carriers, the Materials capacity rail.
- Ripple: `Rasm.Materials` `[DESIGN_BASIS_AXIS]`.
- Atomic: verdict-vocabulary alignment on one section.

[ENERGY_RESULTS_WIRE]-[QUEUED]: Mint the typed energy-results receipt wire — zone and space result rows keyed by the `EnergyArtifact` content key.
- Capability: the `SqlFile` result read emits a typed receipt — annual and peak loads, comfort hours, EUI per zone and space — keyed by the `EnergyArtifact` content key, the record the Bim results-admission fold consumes; `SqlFile` decode stays Compute's per the standing simulation ruling.
- Shape: one receipt record and emit row on `Analysis/energy#SIMULATION_RUN` beside `ReadResults`.
- Unlocks: results survive the run directory — Bim annotates zones and re-exports Psets from the receipt.
- Anchors: `EnergySimulation.Run` fact stream, the `SqlFile` readers, `AssessmentSink` content-keyed landing.
- Ripple: `Rasm.Bim` `[ENERGY_RESULTS_ANNOTATION]`.

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

[GRADUATION_EVIDENCE_OWNER]-[QUEUED]: Land the reverse graduation-evidence envelope the python companion already decodes.
- Capability: the C# half of the graduation crossing gains a real owner — the evidence envelope carrying owner descriptors and their leaf field shapes — so the seam is a two-ended contract rather than one branch-architecture sentence.
- Shape: a `GraduationEvidence` cluster on a `libs/csharp/Rasm.Compute/.planning/Model/` page beside the landed `GraduationEnvelope`, spelling the descriptor and leaf-field roster the companion's stub projector reads.
- Unlocks: the branch seam ledger's `[GRADUATION]` edge gains its C# endpoint, and the companion's decode target stops naming an owner no page holds.
- Anchors: the seam sentence at `libs/csharp/.planning/ARCHITECTURE.md` `[03]-[SEAMS]`; the landed `GraduationEnvelope` on `libs/csharp/Rasm.Compute/.planning/Model/identity.md`.
- Tension: the field roster is the companion's assumption until this owner fixes it, so landing it may contradict a shape the peer already transcribed.
- Ripple: mirrors `python:compute` `[GRADUATION_EVIDENCE_COUNTERPART]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[PARALLEL_BUDGET_BINDINGS]-[COMPLETE]: `TplPopulation(int minSize, int maxSize, IChromosome adamChromosome)` is public and takes the `Population` constructor whole, so `Solver/optimizer#OPTIMIZER_LANE` `GeneticEngine.Evolve` carries it beside the budget-sealed `ParallelTaskExecutor` and `TplOperatorsStrategy`; the recorded blocker was false — the ctor existed at the admitted pin all along.
[BROKER_CARRIER_PARENT]-[COMPLETE]: `[BROKER_TRACE_CONTEXT]` was false — `TraceCarrier.Parent` owns the branch's one `ActivityContext.TryParse` and `SpanEdge.Under(carrier, kind = Consumer)` its ingress bracket over the collapsed `(Kind, Parent, Links)` carriage, so the ingest leg hands the carrier outward with no kernel edit owed and every 3-arg `Traced` call site stays byte-identical under `default(SpanEdge)`.
[MQTT_INGEST_LAND]-[COMPLETE]: `Runtime/transport#BROKER_INGEST` gained the MQTT subscribe pump — a bounded channel bridging `ApplicationMessageReceivedAsync` onto the same `IAsyncEnumerable<Fin<SensorEnvelope<T>>>` the NATS pump yields, `AutoAcknowledge` false with the ack riding the successful enqueue so a shed QoS 1/2 delivery redelivers, and the W3C pair extracted from `MqttApplicationMessage.UserProperties` through `ReadValueAsString` rather than the `[Obsolete]` `Value`.
[GOVERNOR_SERIES_ROSTER]-[COMPLETE]: `Runtime/scheduling#CPU_BUDGET` lands `UtilizationSeries` — the meter name, `process.cpu.utilization`, `dotnet.process.memory.virtual.utilization`, the four container ratios, and `container.memory.usage` — spelled as literals because `ResourceUtilizationInstruments` is `internal`, and the const identifier differs from the dotted wire name on the process-memory row.
[SIGNAL_CAPSULE_COMPOSE]-[COMPLETE]: the folder `PanelKind`, `InstrumentSpec`, `AlertSeverity`, `BurnWindow`/`CanonicalBurn`, and `FactMatch` twins deleted against the kernel instrument mechanism and SLO algebra; the `AlertRoute` rename is refuted — one kernel `AlertSeverity` carries `page` and `ticket` with rank, hold, tone, and urgency columns, leaving no second routing token. `ComputeObjective` keeps the typed fact sampler, `ComputeTraces` composes the kernel `SpanBand`, and `ComputeDescriptors` composes `PanelSpec`/`BoardPack`, each panel breaking on its declaring row's `Dimensions`.
[HOOK_POINT_ROSTER]-[COMPLETE]: five-point roster landed in `.planning/Runtime/receipts#HOOK_POINTS` (`ComputeHookRail.Live`, `HookId.Create` four-segment ids, one payload type and modality per point) with the admit/dispatch thread on `.planning/Runtime/admission#SUBSTRATE_AXIS`; `rasm.compute.solve.iteration` is Replay depth 256, not Observe.
[HOOK_FAULT_ISOLATION]-[COMPLETE]: `.planning/Runtime/receipts#HOOK_POINTS` boundary law composes the kernel capsule fork-shield — subscriber faults park as `IsolatedFault` rows on the roster evidence cell — with the bounded replay buffer under the progress cadence gate and telemetry-as-tap.
[GOVERNOR_UTILIZATION_FOLD]-[COMPLETE]: `.planning/Runtime/scheduling#CPU_BUDGET` `ResourceGovernor.Steer` folds `UtilizationSample` values into the reserve-and-memory-scale re-resolve, returns no fact for a steady sample, and emits one `Governor` fact for each budget or spill transition; `JobGraph` seals the current effective memory limit onto each `JobRun` per invocation.
[COST_VECTOR_ROWS]-[COMPLETE]: `.planning/Runtime/receipts#COST_LEDGER` `CostPolicy.Admit` rate rows price elapsed, token, byte, and remote axes into the `CostVector` monoid with the `rasm.compute.cost.units` roster row.
[TENANT_PARTITIONED_FOLDS]-[COMPLETE]: `.planning/Runtime/receipts#FOLD_PROJECTIONS` `Journal` joins the envelope tenant; `TenantCosts`/`TenantRouteCosts`/`TenantFacts` and `ChargebackDataset.Of` fold the partitioned ledger.
[DESCRIPTOR_PROJECTION]-[COMPLETE]: `.planning/Runtime/receipts#DASHBOARD_DESCRIPTOR` derives one kernel `BoardPack` from the primary `ReceiptSurface.Specs` roster — each `PanelSpec` titled by its row description and broken on that row's declared `Dimensions` — and `ComputeDescriptors.Panels` projects the pack outward with widget, unit, and bucket edges read from the same row.
[SLO_ALERT_ROWS]-[COMPLETE]: `.planning/Runtime/receipts#DASHBOARD_DESCRIPTOR` lands solve-convergence, remote-call, backpressure, and twin-anomaly `ComputeObjective` rows whose specs, windows, factors, and severities derive from the kernel burn table.
[PROFILE_ARTIFACT_UNION]-[COMPLETE]: `.planning/Runtime/receipts#BENCHMARK_CLAIMS` `ProfileArtifact` (ChromeTrace/BenchmarkExport/EpContext) replaces the loose strings on `ModelRun.Profile` and `BenchmarkClaim.Artifacts`; `.planning/Model/inference.md` `RunOps.Profile` mints `ChromeTrace` from the admitted row's `ContentAddress` and `ProfilingStartTimeNs`.
[SPAN_PROFILE_CORRELATION]-[COMPLETE]: `.planning/Runtime/receipts#TELEMETRY_PROJECTION` boundary states the shared-`TraceId` join to the root-span `pyroscope.profile.id` stamp with zero Compute OTel reference.
[MONITOR_PAGE_SPINE]-[COMPLETE]: `.planning/Stats/monitor.md` lands `StreamMonitor` EWMA/P²/`FittedModel` detector capsules, `MonitorChannel` extraction rows, and `MonitorLane` advance/observe folds; detector verdicts call the fitted estimator instead of fabricating window-only results.
[SIGNAL_ADMISSION_LOOP]-[COMPLETE]: the sensor loop closes at `.planning/Runtime/transport#BROKER_INGEST` — `CaptureAdmission` seats `WorkLane.CaptureIngest` itself and `BrokerChannels.Capture` folds each pump delivery through `AdmittedIntent.Admit` onto `LaneRuntime.Enqueue`, the new `ComputeIntent.SensorAdmit` case carrying the typed envelope with its own `Measured`/`Derived` arms; the recorded `[BROKER_NATS]` blocker was already settled, the NATS pump cataloged and landed.
[BROKER_CARRIER_COLLAPSE]-[COMPLETE]: the folder `W3cCarrier` record and the `SensorEnvelope` two-field trace flattening collapsed onto the kernel `TraceCarrier`, so the branch carries one causal-pair spelling; `libs/csharp/.api/api-diagnostics-activity.md` already rows `ActivityContext.TryParse` and the kernel owns the one parse behind `TraceCarrier.Parent`, so no folder spelling of it survives.
[GEOMETRY_ARENA_COLUMNS]-[COMPLETE]: `Runtime/codecs#ARROW_BATCH` gained `GeometryDataset`, the `Lanes`/`ArenaLane` quantization-row wrap, `ArrowBatch.Geometry`, and the `LandingArm.Geometry` `Landing` overload; every channel column borrows the kernel arena through `ArrowBuffer(ReadOnlyMemory<byte>)` at its stored width, so a landing allocates only the `source`/`ordinal` join pair. `Tensor/residency#GEOMETRY_ENCODING` was repaired in the same pass to hold `EncodedGeometry` whole rather than a float-typed payload the dtype-strided arena never carried.
[FLIGHT_PUSH_SEAM]-[COMPLETE]: DOE and chargeback batches land through `.planning/Runtime/codecs#ARROW_BATCH` `ArrowBatch.Landing` — one projection overloaded on dataset shape emitting the `FlatTableEgress.Land` triple over `LandingArm.Doe`/`LandingArm.Cost` and a `Schema.FieldsList`-derived schema key; the "Flight port" the card named is refuted — `#FLIGHT_RESULT_PLANE` is the read end and `Query/columnar#FLAT_TABLE_EGRESS` is the one landing custodian.
[ARROW_DATASET_SCHEMA]-[COMPLETE]: `ArrowBatch.Doe`/`ArrowBatch.Chargeback` already realized the batch projection against a landed `Apache.Arrow` reference and the folder catalog tier, so the card's arming condition had long since resolved.
[MONITOR_VERDICT_SEAM]-[COMPLETE]: `MonitorVerdict.Receipt` mints the `Drift` receipt case the `ComputeInstrumentFan` counts onto `rasm.compute.monitor.breaches`, and `MonitorLane.AsDetector` satisfies the `Solver/clash` injected-detector slot.
