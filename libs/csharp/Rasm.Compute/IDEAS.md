# [COMPUTE_IDEAS]

Measured execution's forward pool of higher-order concepts, each grounded in the folder's domain and current platform capability — some deepen a thin owner into a fuller axis, others bind a concrete technique to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[STATS_MODEL_SELECTION]-[QUEUED]: Model-selection surface — information criteria, hyper-parameter paths, and a candidate chooser over the estimator lane's landed `Validate` axis.
- Capability: selection beyond held-out scoring — information criteria over fitted likelihoods, hyper-parameter path evaluation (penalty strength, kernel width, cluster count), and a chooser folding `Validate` scores across candidate `EstimatorPolicy` rows into one ranked verdict.
- Shape: a selection fold on `Stats/estimator` `EstimatorFold` beside `Fit`/`Predict`/`Validate` — candidates as policy rows, never a sibling trainer or a grid-search service; lands in `libs/csharp/Rasm.Compute/.planning/Stats/estimator.md`.
- Unlocks: defensible-by-default classical fits; the C# half of the graduation-evidence contract gains the selection discipline the Python companion carries.
- Anchors: `Stats/estimator#ESTIMATOR_LANE` `Validate` (k-fold and forward-chain scoring landed), `EstimatorPolicy` admitted ranges, the graduation-evidence axis demanding quantified generalization.

[SOLVER_FARM_SHARDS]-[QUEUED]: Sharded distributed solve — one solve partitioned across farm nodes with shard evidence on the receipt rail.
- Capability: a solve partitioned into per-node sub-blocks over the remote-grpc farm, each shard a scheduled job whose count, node, and merge evidence land as receipt fields and one shard instrument row.
- Shape: a shard-partition fold on `Runtime/scheduling` job-graph dispatch feeding per-node `remote-grpc` hops, shard evidence a `Solve`/`Factorization` receipt field the `Runtime/receipts` projection fan folds — never a parallel farm router; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/scheduling.md` and `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: building-scale modal and buckling solves exceeding one node; shard-grain observability for farm capacity planning.
- Anchors: `Runtime/admission#SUBSTRATE_AXIS` warm-affinity and `LoadRank` farm routing, the `Runtime/scheduling` job graph, the `Runtime/receipts` instrument roster absorbing one row per curated aggregate.

[SOLVER_ELEMENT_QUANTIFIED_RULES]-[QUEUED]: Graph-exhaustive rule grounding — `ComplianceRule` templates ground over every `ElementGraph` node-class member with a coverage proof.
- Capability: a node-class selector deriving `RuleGrounding` populations from the concrete graph with a population fold proving every matching member instantiated, verdict/witness/unsat-core keyed per element.
- Shape: a grounding derivation on `Solver/satisfy#RULE_SATISFACTION` consuming the graph the assessment spine already routes — the template quantifies, the selector proves exhaustiveness, caller-supplied rows remain the manual lane; lands in `libs/csharp/Rasm.Compute/.planning/Solver/satisfy.md`.
- Unlocks: satisfy upgrades from caller-assembled populations to a whole-building code audit whose unsat core names the exact failing elements ("every egress door", "each lateral-system member").
- Anchors: `ComplianceRule`/`RuleGrounding` template quantification with name@element tracking literals (landed), assessment-spine per-node fact routing, `Analysis` runners reading the concrete `Rasm.Element` `ElementGraph`.

[RUNTIME_HOOK_POINTS]-[QUEUED]: Typed compute hook roster on the AppHost hook rail — veto/observe/replay points over admission, dispatch, solve iteration, assessment writeback, and twin control.
- Capability: package-qualified `rasm.compute.<domain>.<point>` hook points, each one closed payload type and one modality row — admission veto (policy transform-or-reject before `Plan`), dispatch observe (substrate-keyed tap beside the `ComputeTraces` span), solve-iteration observe (cadence-gated convergence marks), assessment-writeback veto (gate before the caller applies the `GraphDelta`), twin-control veto (gate before the control suggestion crosses to the AppHost write-back as `ExternalValue`).
- Shape: a hook-point roster registered once at composition through the AppHost registry, domain code firing evidence and observability subscribing — no emit-call scatter; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/admission.md` and `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: app-composable policy gates (a tenant plugin vetoing a solve, a UI replaying convergence marks) with subscriber-fault isolation; two apps compose disjoint hook sets without collision because ids are registry-enforced unique.
- Anchors: AppHost `Observability/hooks` `HookId.Validate` four-segment grammar, `HookModality` Veto/Observe/Replay rows, registry structural uniqueness and `FaultBand.Hook`; `ComputeTraces.Traced` dispatch spine; `Analysis/assessment` content-keyed `GraphDelta`; `Solver/clash#CLASH_AND_TWIN` receipted control crossing.

[ADAPTIVE_RESOURCE_GOVERNOR]-[QUEUED]: Utilization-adaptive scheduling — live `IResourceMonitor` snapshots govern `CpuBudget`, reader fan-out, spill, and backpressure instead of a boot-frozen processor count.
- Capability: a governor fold reading `ResourceUtilization` (CPU/memory percentages, container-granted `SystemResources`) at collection cadence into lane reader ceilings, job-graph fair-share weights, spill-to-store pressure, and backpressure thresholds, every adjustment a receipted fact; in-node parallel evaluators — GeneticSharp `TplPopulation`, ORT intra-op threads, partition routes — bind under the same governed budget.
- Shape: a utilization column on the `Runtime/scheduling` `CpuBudget`/`JobGraph` owners fed by the AppHost-composed monitor, never a second scheduler; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/scheduling.md`.
- Unlocks: farm and desktop hosts degrade gracefully under co-tenant load; container CPU limits stop overcommitting readers; screening sweeps self-throttle instead of starving the interactive lane.
- Anchors: branch `.api` `api-resourcemonitoring.md` `IResourceMonitor.GetUtilization`/`ResourceUtilization`/`SystemResources`, `Runtime/scheduling#CPU_BUDGET` `CpuBudget` as the one budget record, `WorkLane.Readers(budget)` row projection, `HostFingerprint.Current` already reading `CpuBudget.Total`.

[TENANT_COST_LEDGER]-[QUEUED]: Per-tenant compute cost attribution — grant-cost vectors joined to the envelope tenant partition yield a chargeback ledger from the standing receipt stream.
- Capability: substrate-rated cost vectors (elapsed×substrate rate, generated tokens, staged bytes, remote node-seconds) folded per tenant from the identical fact stream dashboards consume, one `rasm.compute.cost.units` instrument row, and a content-keyed chargeback dataset egress — journal rows stay billing truth, the metric stays the lossy channel.
- Shape: cost-policy rows and tenant-partitioned fold members on `Runtime/receipts` `ReceiptFolds` reading the envelope `Tenant` field `Send` already threads, never a second fact stream; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: per-tenant cost and usage boards for multi-tenant hosts; farm capacity pricing; the compute leg of the estate cost fabric.
- Anchors: `ReceiptSurface.Emit` ambient `TenantContext.Current` envelope partition, `ReceiptScope.Execution` lane/substrate/elapsed evidence, `Generate.Tokens`/`Allocation` byte evidence, AppHost tenant cardinality cap on tag fans.
- Ripple: `libs` `[COST_ATTRIBUTION_BAGGAGE]`.

[DASHBOARD_ALERT_DESCRIPTOR]-[QUEUED]: Compute dashboard-and-alert contribution as data — the instrument roster projects into a typed descriptor the IaC compile leg turns into provisioned dashboards and rule groups.
- Capability: a descriptor family deriving panels from the `rasm.compute.*` `InstrumentRow` roster (unit, description, bucket advice as thresholds) and SLO burn-rate alert rows from the operational folds — solve non-convergence rate, remote-call failure rate, backpressure drop rate, twin anomaly rate — so dashboard truth derives from the same roster the meter mounts and can never drift from it.
- Shape: a descriptor projection beside `ReceiptSurface.Instruments` emitting data rows, never rendered JSON; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md`.
- Unlocks: zero-drift compute observability boards; alert coverage for every curated aggregate; the folder's contribution to the estate deploy plane without owning any IaC surface.
- Anchors: `ReceiptSurface.Instruments` roster with `ComputeBuckets` advice, `ReceiptFolds` operational views, ts-iac Foundation-SDK compile leg consuming descriptor data, suite schema-hash gating on generated projections.

[PROFILE_EVIDENCE_AXIS]-[QUEUED]: One typed profile-artifact family — ORT chrome-trace, BenchmarkDotNet exports, and span-correlated continuous profiles unify as content-keyed evidence on receipts and claims.
- Capability: a `ProfileArtifact` union covering the chrome-trace `EndProfiling` output, BenchmarkDotNet exporter artifacts, and EP-context caches, each content-keyed and indexed on the blob lane; dispatch spans stamp the correlation the AppHost-composed `PyroscopeSpanProcessor` links to `pyroscope.profile.id`, so a flame graph joins its receipt chain by span identity with zero Compute OTel reference.
- Shape: a typed artifact family replacing the loose `ProfileArtifact`/`Artifacts` string columns across `ModelRun` receipts and `BenchmarkClaim` rows; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/receipts.md` and `libs/csharp/Rasm.Compute/.planning/Model/inference.md`.
- Unlocks: profile-to-receipt navigation in the evidence view; support bundles carry provable profile provenance; claim artifacts stop being untyped path strings.
- Anchors: branch `.api` `api-pyroscope-opentelemetry.md` root-span `pyroscope.profile.id` stamping, `ComputeTraces.Started` span tags, `ModelRun.ProfileArtifact` and `BenchmarkClaim.Artifacts` columns, Persistence `ArtifactIndexRow` custody.

[TWIN_SENSOR_INGEST]-[QUEUED]: Broker-carried sensor ingest for the digital twin — MQTT and NATS subscription rows feed measured signals into the twin loop under CloudEvents envelopes with W3C trace continuity.
- Capability: ingest transport rows subscribing sensor streams (MQTT v5 UserProperties, NATS headers — both manual W3C composite carriers by estate law), decoding CloudEvents payloads into typed measured signals, admitting them onto the `capture-ingest` lane, and closing the loop through `DigitalTwin.Score`/`Update` so site telemetry drives anomaly verdicts and FE model updating continuously.
- Shape: ingest rows beside the gRPC axis under the same receipt and deadline law — a subscription is a channel row, never a second transport spine; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/transport.md` and `libs/csharp/Rasm.Compute/.planning/Solver/clash.md`.
- Unlocks: live-building digital twins without a bespoke ingest service; trace-continuous sensor evidence joining the OTel rail; the AEC IoT edge lands on the standing twin machinery.
- Anchors: `MQTTnet`, `NATS.Net`, `CloudNative.CloudEvents` admitted in the central manifest; `WorkLane.CaptureIngest` DropOldest row; `Twin` receipt case; `Stats/signal` `Transform.Modal` measured end; estate transport law (no OTel broker instrumentation exists — manual carriers are design, not gap).
- Tension: `CloudNative.CloudEvents.Mqtt` is unadmitted — the card assumes the admission lane lands it; until then the MQTT row decodes the structured-mode JSON envelope by hand.

[STATS_STREAM_MONITORS]-[QUEUED]: Streaming statistical monitors — online control charts, quantile sketches, and drift verdicts over receipt and sensor streams as a third Stats page.
- Capability: stateful monitor capsules composing the landed detector rows (CUSUM, Bayesian-online run-length, correlated-residual) as streaming folds beside online EWMA control limits and P²-class quantile sketches, consuming `Seq<ComputeReceipt>` operational streams (solve residual drift, remote-latency drift, backpressure cadence) and twin signal windows, each verdict a typed fact.
- Shape: one new page `libs/csharp/Rasm.Compute/.planning/Stats/monitor.md` owning the monitor capsule family — batch detectors stay `Stats/estimator` rows the monitors compose, never re-derived.
- Unlocks: operational drift detection over the folder's own telemetry; the twin anomaly detector gains control-chart discipline; Stats deepens past its two-file stub.
- Anchors: `Stats/estimator` `TimeSeriesModel` cusum/bayesian-online/correlated-residual rows and `EstimatorModel.Detector` carrier, `ReceiptFolds` stream views, `Solver/clash` injected Stats detector seam, MathNet distributions for control-limit quantiles.

[OPTIMIZER_ROUTING_LANE]-[QUEUED]: Route and sequence optimization row — OR-Tools routing joins the exact lane for tour-shaped AEC design problems.
- Capability: `RoutingModel`/`RoutingIndexManager`/`RoutingDimension` behind one `OptimizerKind` row solving crane-pick sequencing, hoist tours, MEP run ordering, and site-logistics routes with capacity and time-window dimensions, typed status mapping onto the fault rail, and `Optimization` receipt evidence.
- Shape: one routing row on the `Solver/optimizer` exact lane beside CP-SAT/MILP — the problem shape is typed nodes, arcs, dimensions, and vehicles, never a hand-built CP encoding; lands in `libs/csharp/Rasm.Compute/.planning/Solver/optimizer.md`.
- Unlocks: sequence-shaped design search the CP-SAT row prices poorly; the admitted routing surface stops being dead catalog weight.
- Anchors: folder `.api` `api-ortools.md` solver#ROUTING rail (`RoutingModel`, `RoutingIndexManager`, `RoutingSearchParameters`, `RoutingDimension`, `RoutingSearchStatus.Types.Value`), `OptimizerKind` row law, `Analysis/circulation` OR-Tools graph natives as the disposal-pattern precedent.

[DOE_LAKE_EGRESS]-[QUEUED]: Arrow-native sweep and receipt egress — screening corpora land lake-queryable as record batches with a Flight push seam instead of dying JSON-only.
- Capability: an Arrow schema projection for `DoeDataset` (columnar factors, objectives, front membership) and receipt archives, content key preserved as batch metadata, pushed through the Persistence-owned Flight SQL seam so DuckDB, polars, and the Python companion query screening corpora directly.
- Shape: an Arrow codec arm on the `Runtime/codecs` field/result codec family with a `Dataset` egress column on `Solver/sweep` — Compute owns the batch shape, Persistence owns the client and landing; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/codecs.md` and `libs/csharp/Rasm.Compute/.planning/Solver/sweep.md`.
- Unlocks: thousand-variant screening history becomes an analyzable lake asset; graduation training corpora stream without bespoke decode; receipt archives join the estate analytics plane.
- Anchors: `SweepLane.Dataset` graduation egress, `Runtime/codecs` GeoArrow IPC precedent, `Apache.Arrow.Flight.Sql` admitted with Persistence `.api` custody, `XxHash128` content keys on `DoeDataset`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[SOLVER_GEOTECHNICAL_CONSTITUTIVE]-[COMPLETE]: pressure-dependent frictional soil laws landed collapsed — `Solver/constitutive` `PlasticPotential` parameterizes `DruckerPrager`/`SmoothedMohrCoulomb`/`ModifiedCamClay` as seed data over one invariant generator, and `MaterialState` carries volumetric hardening, preconsolidation pressure, and pore pressure.

[SOLVER_ARC_LENGTH_CONTINUATION]-[COMPLETE]: `Solver/contract` `SolveMethod.ArcLength` and `ArcLengthPolicy` enforce the Crisfield displacement/load constraint through predictor-corrector iterations across limit points on the landed Newton internal-force machinery.

[ST-FDD]-[COMPLETE]: `Stats/signal` `Transform.Modal` runs the N-channel frequency-domain decomposition over Welch cross-PSD matrices, returning `ModalEstimate`/`MeasuredMode` with the full singular spectrum; `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end.
