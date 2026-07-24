# [COMPUTE_IDEAS]

Measured execution's forward pool of higher-order concepts, each grounded in the folder's domain and current platform capability — some deepen a thin owner into a fuller axis, others bind a concrete technique to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
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

[OPTIMIZER_ROUTING_LANE]-[QUEUED]: Route and sequence optimization row — OR-Tools routing joins the exact lane for tour-shaped AEC design problems.
- Capability: `RoutingModel`/`RoutingIndexManager`/`RoutingDimension` behind one `OptimizerKind` row solving crane-pick sequencing, hoist tours, MEP run ordering, and site-logistics routes with capacity and time-window dimensions, typed status mapping onto the fault rail, and `Optimization` receipt evidence.
- Shape: one routing row on the `Solver/optimizer` exact lane beside CP-SAT/MILP — the problem shape is typed nodes, arcs, dimensions, and vehicles, never a hand-built CP encoding; lands in `libs/csharp/Rasm.Compute/.planning/Solver/optimizer.md`.
- Unlocks: sequence-shaped design search the CP-SAT row prices poorly; the admitted routing surface stops being dead catalog weight.
- Anchors: folder `.api` `api-ortools.md` solver#ROUTING rail (`RoutingModel`, `RoutingIndexManager`, `RoutingSearchParameters`, `RoutingDimension`, `RoutingSearchStatus.Types.Value`), `OptimizerKind` row law, `Analysis/circulation` OR-Tools graph natives as the disposal-pattern precedent.

[TWIN_SENSOR_INGEST]-[BLOCKED]: Live sensor telemetry decodes into the serialized twin loop over verified broker members.
- Capability: broker subscription, NATS decode, and W3C activity restoration join the settled typed MQTT CloudEvents decode and serialized twin loop, closing the sensor-to-twin path.
- Shape: required-tier member rows beside the settled decode and twin-loop owners in `libs/csharp/Rasm.Compute/.planning/Runtime/`.
- Unlocks: live sensor streams reach the digital-twin loop with typed, verified broker crossings.
- Anchors: the settled MQTT CloudEvents decode; `TwinLoop`; the MQTTnet, NATS.Net, and diagnostics catalogs.
- Arms: complete MQTTnet, NATS.Net, and diagnostics catalog declarations and direct broker project references.

[DOE_LAKE_EGRESS]-[BLOCKED]: DOE datasets egress to the analytical lake as Arrow batches over the Persistence Flight port.
- Capability: `DoeDataset` — front membership and stable content key settled — gains a verified `RecordBatch` projection and a push seam onto the durable columnar plane.
- Shape: Arrow batch rows and the Flight push seam beside the settled `DoeDataset` owner in `libs/csharp/Rasm.Compute/.planning/`.
- Unlocks: DOE truth lands on the columnar lake plane for cross-runtime analytics.
- Anchors: `DoeDataset` `OnFront` and content key; the Apache.Arrow catalog tiers; `Rasm.Persistence` `[PERS_L1]` Flight SQL serving.
- Arms: complete Arrow catalogs, a direct `Apache.Arrow` project reference, and the reciprocal Persistence port.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[RUNTIME_HOOK_POINTS]-[COMPLETE]: realized in `.planning/Runtime/receipts.md#HOOK_POINTS` — `ComputeHookRail.Live` mints the five-point roster as kernel `HookPoint<TFact>` values, capsule fork-shield isolation parking `IsolatedFault` rows on the roster evidence cell; `rasm.compute.solve.iteration` landed as Replay depth 256, correcting the card's Observe claim.
[ADAPTIVE_RESOURCE_GOVERNOR]-[COMPLETE]: realized in `.planning/Runtime/scheduling.md#CPU_BUDGET` — `ResourceGovernor.Steer` folds typed `UtilizationSample` values into the reserve-and-memory-scale re-resolve, `JobGraph` seals the effective memory limit onto every `JobRun`, and only a budget or spill transition returns a `Governor` fact; the `IResourceMonitor`/`ResourceUtilization` snapshot API is obsolete `EXTOBS0001`, so AppHost sources samples from the package's observable instruments.
[TENANT_COST_LEDGER]-[COMPLETE]: realized in `.planning/Runtime/receipts.md#COST_LEDGER` — `CostPolicy`/`CostVector`/`ChargebackDataset` over the envelope-joined `ReceiptFolds.Journal` with the `rasm.compute.cost.units` lossy channel; the `libs` `[COST_ATTRIBUTION_BAGGAGE]` ripple end stays the estate tier's.
[DASHBOARD_ALERT_DESCRIPTOR]-[COMPLETE]: realized in `.planning/Runtime/receipts.md#DASHBOARD_DESCRIPTOR` — `ComputeDescriptors.Panels` derives from `ReceiptSurface.Specs`, four `SloAlertRow` values ride `CanonicalBurn`, and `Probe` refuses drift at boot.
[PROFILE_EVIDENCE_AXIS]-[COMPLETE]: realized in `.planning/Runtime/receipts.md#BENCHMARK_CLAIMS` (`ProfileArtifact` union) with `.planning/Model/inference.md` `RunOps.Profile` minting `ChromeTrace` from the admitted `ArtifactIndexRow` and the `#TELEMETRY_PROJECTION` span-identity correlation law.
[STATS_STREAM_MONITORS]-[COMPLETE]: realized as `.planning/Stats/monitor.md` — `StreamMonitor` capsules (EWMA, P², `FittedModel` detector), `MonitorChannel` receipt extraction, the `Drift` receipt case, and the `MonitorLane.AsDetector` twin projection.
[SOLVER_GEOTECHNICAL_CONSTITUTIVE]-[COMPLETE]: pressure-dependent frictional soil laws landed collapsed — `Solver/constitutive` `PlasticPotential` parameterizes `DruckerPrager`/`SmoothedMohrCoulomb`/`ModifiedCamClay` as seed data over one invariant generator, and `MaterialState` carries volumetric hardening, preconsolidation pressure, and pore pressure.
[SOLVER_ARC_LENGTH_CONTINUATION]-[COMPLETE]: `Solver/contract` `SolveMethod.ArcLength` and `ArcLengthPolicy` enforce the Crisfield displacement/load constraint through predictor-corrector iterations across limit points on the landed Newton internal-force machinery.
[ST_FDD]-[COMPLETE]: `Stats/signal` `Transform.Modal` runs the N-channel frequency-domain decomposition over Welch cross-PSD matrices, returning `ModalEstimate`/`MeasuredMode` with the full singular spectrum; `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end.
