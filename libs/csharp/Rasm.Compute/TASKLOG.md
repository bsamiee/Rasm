# [COMPUTE_TASKLOG]

Open and closed work for measured execution, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup and the residual live-host probes whose owner shape is complete.

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

[HOOK_POINT_ROSTER]-[QUEUED]: Pin the compute hook-point roster — exact ids, payload types, and modality rows per point.
- Capability: the five-point roster — `rasm.compute.runtime.admit` (Veto), `rasm.compute.runtime.dispatch` (Observe), `rasm.compute.solve.iteration` (Observe), `rasm.compute.assessment.writeback` (Veto), `rasm.compute.twin.control` (Veto) — each bound to one closed payload type and registered once at composition.
- Shape: roster rows in `libs/csharp/Rasm.Compute/.planning/Runtime/admission.md` (admit/dispatch) and `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md` (iteration/writeback/control).
- Unlocks: `[RUNTIME_HOOK_POINTS]` roster half; slice-implement discovery gets exact grep-able point names.
- Anchors: AppHost `HookId.Validate` grammar, `HookModality` rows, registry uniqueness law.

[HOOK_FAULT_ISOLATION]-[QUEUED]: Wire subscriber-fault isolation and the telemetry-as-tap law across the compute hook points.
- Capability: subscriber failures land on the AppHost hook fault band without breaking the emitting rail; replay points size bounded buffers; observability subscribes to hook evidence rather than adding emit calls in domain code.
- Shape: isolation and buffer law on the roster rows in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: `[RUNTIME_HOOK_POINTS]` delivery half; a faulting UI subscriber can never fail a solve.
- Anchors: AppHost `FaultBand.Hook`, `ReceiptSinkPort` envelope tap, bounded-channel precedent on `WorkLane`.

[GOVERNOR_UTILIZATION_FOLD]-[QUEUED]: Fold live resource utilization into budget, reader, spill, and backpressure decisions.
- Capability: `IResourceMonitor.GetUtilization` snapshots project onto `CpuBudget` ceilings, `WorkLane.Readers` fan-out, job-graph fair-share weights, and spill pressure at collection cadence, every adjustment receipted.
- Shape: a governor column on the `CpuBudget`/`JobGraph` owners in `libs/csharp/Rasm.Compute/.planning/Runtime/scheduling.md`; the monitor composes at AppHost.
- Unlocks: `[ADAPTIVE_RESOURCE_GOVERNOR]` core; container-aware degradation under co-tenant load.
- Anchors: branch `.api` `api-resourcemonitoring.md` `ResourceUtilization`/`SystemResources`, `CpuBudget` one-record law, `WorkLane.Readers(budget)`.

[PARALLEL_BUDGET_BINDINGS]-[QUEUED]: Bind in-node parallel evaluators under the governed budget.
- Capability: GeneticSharp `TplPopulation` population parallelism, ORT intra-op thread counts, and tensor partition routes read the governed `CpuBudget`, never ambient processor count.
- Shape: binding rows on `libs/csharp/Rasm.Compute/.planning/Solver/optimizer.md` and `libs/csharp/Rasm.Compute/.planning/Runtime/scheduling.md`.
- Unlocks: the admitted `TplPopulation` surface gains its consumer; parallel fitness evaluation stops oversubscribing lanes.
- Anchors: folder `.api` `api-geneticsharp.md` `TplPopulation` row, `CpuBudget.Total` fingerprint precedent.
- Atomic: budget bindings on two owners.

[COST_VECTOR_ROWS]-[QUEUED]: Land substrate-rated cost-policy rows and the cost instrument.
- Capability: per-substrate rate rows pricing elapsed seconds, generated tokens, staged bytes, and remote node-seconds into cost units; one `rasm.compute.cost.units` instrument row on the roster.
- Shape: policy rows beside `ReceiptSurface.Instruments` in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: `[TENANT_COST_LEDGER]` pricing half.
- Anchors: `ReceiptScope.Execution` substrate/elapsed evidence, `Generate.Tokens`, `Allocation` byte fields, curated-aggregate instrument law.

[TENANT_PARTITIONED_FOLDS]-[QUEUED]: Partition the operational folds by envelope tenant and project the chargeback dataset.
- Capability: tenant-keyed fold members joining envelope `Tenant` to cost vectors over the fact stream, with a content-keyed chargeback dataset egress.
- Shape: fold members on `ReceiptFolds` in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: `[TENANT_COST_LEDGER]` ledger half; per-tenant boards from the identical stream.
- Anchors: `ReceiptSurface.Emit` threading `TenantContext.Current`, `Provenance` fold precedent, AppHost tenant cardinality cap.

[DESCRIPTOR_PROJECTION]-[QUEUED]: Project the instrument roster into the typed dashboard descriptor.
- Capability: panel rows deriving name, unit, description, and bucket-advice thresholds from `ReceiptSurface.Instruments` so descriptor truth can never drift from the mounted roster.
- Shape: a descriptor projection beside the roster in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: `[DASHBOARD_ALERT_DESCRIPTOR]` panel half.
- Anchors: `ReceiptSurface.Kinds` generated-projection precedent, `ComputeBuckets` advice arrays, suite schema-hash gating.

[SLO_ALERT_ROWS]-[QUEUED]: Derive the SLO burn-rate alert rows from the operational folds.
- Capability: alert rows for solve non-convergence rate, remote-call failure rate, backpressure drop rate, and twin anomaly rate with burn windows and severities as data.
- Shape: alert rows on the descriptor in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: `[DASHBOARD_ALERT_DESCRIPTOR]` alert half; the ts-iac counterpart compiles rule groups from data.
- Anchors: `ReceiptFolds.Diverged`/`Anomalies`, `Backpressure`/`Drain` cases, core SLO burn-rate algebra at the estate tier.

[PROFILE_ARTIFACT_UNION]-[QUEUED]: Replace loose artifact strings with the typed `ProfileArtifact` union.
- Capability: chrome-trace, BenchmarkDotNet export, and EP-context artifact cases, each content-keyed and blob-indexed, unifying `ModelRun.ProfileArtifact` and `BenchmarkClaim.Artifacts`.
- Shape: a union family in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md` with the inference column repointed in `libs/csharp/Rasm.Compute/.planning/Model/inference.md`.
- Unlocks: `[PROFILE_EVIDENCE_AXIS]` typing half; claim artifacts stop being untyped paths.
- Anchors: `ArtifactIndexRow` custody, `EndProfiling` chrome-trace output, claim `Artifacts` column.

[SPAN_PROFILE_CORRELATION]-[QUEUED]: Join dispatch spans to continuous profiles by span identity.
- Capability: dispatch spans carry the identity the AppHost-composed `PyroscopeSpanProcessor` stamps as `pyroscope.profile.id`, so flame graphs join receipt chains with zero Compute OTel reference.
- Shape: a correlation law note on `ComputeTraces` in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: `[PROFILE_EVIDENCE_AXIS]` correlation half.
- Anchors: branch `.api` `api-pyroscope-opentelemetry.md` root-span stamping, `ComputeTraces.Started` tag set.
- Atomic: one correlation law on the trace spine.

[BROKER_INGEST_ROWS]-[QUEUED]: Land the MQTT and NATS ingest rows with CloudEvents decode and W3C continuity.
- Capability: subscription rows decoding CloudEvents envelopes into typed measured signals, trace context read manually from MQTT v5 UserProperties and NATS headers, receipted under the existing transport evidence law.
- Shape: ingest rows beside the gRPC axis in `libs/csharp/Rasm.Compute/.planning/Runtime/transport.md`.
- Unlocks: `[TWIN_SENSOR_INGEST]` transport half.
- Anchors: `MQTTnet`/`NATS.Net`/`CloudNative.CloudEvents` central-manifest rows, `WireTransition` receipt precedent, estate manual-carrier law.

[SIGNAL_ADMISSION_LOOP]-[QUEUED]: Admit ingested signals onto the capture lane and close the twin loop.
- Capability: measured-signal admission (finite, windowed, identified) enqueues onto `WorkLane.CaptureIngest`, feeds `DigitalTwin.Score` continuously, and batches windows into `Update` FE recalibration.
- Shape: an admission-and-loop seam in `libs/csharp/Rasm.Compute/.planning/Solver/clash.md`.
- Unlocks: `[TWIN_SENSOR_INGEST]` loop half; live anomaly verdicts and receipted control suggestions from site telemetry.
- Anchors: `CaptureIngest` DropOldest row, `DigitalTwin.Score` signal faulting, `Transform.Modal` measured end.

[MONITOR_PAGE_SPINE]-[QUEUED]: Author the streaming-monitor page — capsule family, composed detector rows, and sketch families.
- Capability: stateful monitor capsules composing estimator detector rows as streaming folds beside EWMA control limits and P²-class quantile sketches over receipt and signal streams.
- Shape: one new page `libs/csharp/Rasm.Compute/.planning/Stats/monitor.md`; batch detectors stay `Stats/estimator` rows the monitors compose.
- Unlocks: `[STATS_STREAM_MONITORS]` page half; Stats deepens past its stub.
- Anchors: `TimeSeriesModel` cusum/bayesian-online/correlated-residual rows, `EstimatorModel.Detector`, `ReceiptFolds` stream views.

[MONITOR_VERDICT_SEAM]-[QUEUED]: Route monitor verdicts onto the receipt rail and the twin detector seam.
- Capability: drift and control-limit verdicts land as typed facts the projection fan folds, and the `Solver/clash` injected detector slot accepts a monitor capsule.
- Shape: a verdict seam in `libs/csharp/Rasm.Compute/.planning/Stats/monitor.md` naming its receipt case row.
- Unlocks: `[STATS_STREAM_MONITORS]` evidence half; operational drift joins the dashboards.
- Anchors: `Twin` case precedent, `ComputeInstrumentFan` compile-broken switch, injected-detector boundary on the clash page.

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

[ARROW_DATASET_SCHEMA]-[QUEUED]: Project `DoeDataset` and receipt archives onto Arrow record-batch schemas.
- Capability: columnar factor/objective/front-membership batches and receipt-archive batches, content key preserved as batch metadata, schema stated as data beside the codec.
- Shape: an Arrow codec arm on the field/result codec family in `libs/csharp/Rasm.Compute/.planning/Runtime/codecs.md` with the `Dataset` egress column in `libs/csharp/Rasm.Compute/.planning/Solver/sweep.md`.
- Unlocks: `[DOE_LAKE_EGRESS]` schema half; DuckDB/polars query screening corpora directly.
- Anchors: `SweepLane.Dataset` projection, GeoArrow IPC codec precedent, `XxHash128` content keys.

[FLIGHT_PUSH_SEAM]-[QUEUED]: Name the Flight push seam — Compute batch shape, Persistence client and landing.
- Capability: pushed batches cross a named seam where Persistence owns the Flight SQL client, index rows, and lake landing while Compute owns only the batch shape and content key.
- Shape: a seam declaration on `libs/csharp/Rasm.Compute/.planning/Runtime/codecs.md` mirroring the counterpart custody.
- Unlocks: `[DOE_LAKE_EGRESS]` transport half without a Compute store client.
- Anchors: `Apache.Arrow.Flight.Sql` Persistence `.api` custody, `ArtifactIndexRow` landing precedent, tessellation two-hop seam law.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
