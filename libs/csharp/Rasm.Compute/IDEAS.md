# [COMPUTE_IDEAS]

Measured execution's forward pool of higher-order concepts, each grounded in the folder's domain and current platform capability — some deepen a thin owner into a fuller axis, others bind a concrete technique to a settled abstract surface. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[STATS_MODEL_SELECTION]-[QUEUED]: Model-selection surface — information criteria, hyper-parameter paths, and a candidate chooser over the estimator lane's landed `Validate` axis.
- Capability: selection beyond held-out scoring — information criteria over fitted likelihoods, hyper-parameter path evaluation (penalty strength, kernel width, cluster count), and a chooser folding `Validate` scores across candidate `EstimatorPolicy` rows into one ranked verdict.
- Shape: a selection fold on `Stats/estimator` `EstimatorFold` beside `Fit`/`Predict`/`Validate` — candidates as policy rows, never a sibling trainer or a grid-search service; lands in `libs/csharp/Rasm.Compute/.planning/Stats/estimator.md`.
- Unlocks: defensible-by-default classical fits; the C# graduation binding gains the selection discipline Python compute carries.
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
- Anchors: folder `.api` `api-ortools#ENTRYPOINTS` routing rail (`RoutingModel`, `RoutingIndexManager`, `RoutingSearchParameters`, `RoutingDimension`, `RoutingSearchStatus.Types.Value`), `OptimizerKind` row law, `Analysis/circulation` OR-Tools graph natives as the disposal-pattern precedent.

[MESHLET_CUT_EVIDENCE]-[QUEUED]: Publish the duplicated-vertex cut a cluster build paid, so a build-strategy choice is measured rather than assumed.
- Capability: the encode receipt carries the realized boundary-vertex count per level, so the greedy native builds and the cut-minimizing bisection are comparable on the one figure that decides stream cost, and a policy row is chosen from evidence instead of taste.
- Shape: one measured column on the payload receipt projection beside the cluster count, filled in the meshlet-cluster encode arm; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/payload.md` `[RESIDENCY]`.
- Unlocks: the `ClusterBuild` axis becomes selectable by measurement — a corpus-level answer to which build a given geometry class wants.
- Anchors: `ClusterBuild.Bisect` and its `FaceAdjacency` cut weight already compute the shared-vertex count this receipt publishes; the `StreamSegment` slot already carries the cluster count beside the blob length.

[MESHLET_CURVATURE_COLUMN]-[QUEUED]: Each meshlet cluster carries its own curvature bound, so a ray-cone consumer widens its footprint by measured shape instead of guessing planar.
- Capability: the per-cluster descriptor grows a curvature evidence column measured at encode from the cluster's own triangles, so every downstream footprint, LOD, and filtering decision reads geometry truth the payload already visited once.
- Shape: one curvature column on `ResidencyMeshlet` beside the cluster-LOD chain columns, filled in the meshlet-cluster encode arm and decoded through `Runs`; lands in `libs/csharp/Rasm.Compute/.planning/Runtime/payload.md` `[RESIDENCY]`.
- Unlocks: `csharp:Rasm.AppUi` `[RAY_CONE_CURVATURE]` — the declared ray-cone growth leg arms the moment this column ships.
- Anchors: `ResidencyMeshlet` already carries `Level`/`Parent`/`Shell`/`Error`/`ParentError` as per-cluster evidence, so a further measured column is a widening, not a shape change; the encode arm already walks every cluster triangle for cone and sphere bounds, so the curvature estimate reads visited data.
- Ripple: `csharp:Rasm.AppUi` `[RAY_CONE_CURVATURE]` consumes; this card is the producer end.

[SOLVER_DIRECTIONAL_PARTICIPATION]-[QUEUED]: Modal participation becomes directional, so a seismic mass floor is checked per excitation axis the way every code writes it.
- Capability: the modal result carries one participation factor per excitation direction rather than one over an all-ones influence vector, so the effective-mass floor gates per axis, a torsional mode stops contributing to a translational demand, and the spectral demand scales by the direction the spectrum row was written for.
- Shape: the participation column widens from a per-mode scalar to a per-mode direction triple on the solve result, the modal folds project it, and the seismic gate and spectral demand read the direction the request names; lands in `libs/csharp/Rasm.Compute/.planning/Solver/contract.md` `[02]-[SOLVE_CONTRACT]` and `libs/csharp/Rasm.Compute/.planning/Analysis/structural.md` `[05]-[SEISMIC_ROUTE]`.
- Unlocks: `IDEAS.md [SOLVER_DIRECTIONAL_PARTICIPATION]` — the seismic route's 90% floor becomes the code-faithful per-direction check instead of a single aggregate that sums translational and rotational contributions into one number.
- Anchors: the modal arms already fold the participation factor off the lumped inertia and the full recovered mode, so a direction is a projection of data in hand; the condensed route returns full-length modes, so a direction triple reads the same rows the whole-operator route does; the spectrum rows carry behaviour factor and damping per code.
- Tension: aggregate and per-direction floors disagree on real models, so this changes seismic verdicts on already-content-keyed assessments — the solver-version token is the re-key lever, and one deliberate re-key beats a silent parallel column.

[SOLVER_SCALAR_CAPACITY]-[QUEUED]: Thermal and network transients march on real capacity, so a first-order step is a physical time constant rather than a geometric share.
- Capability: the scalar material forms carry volumetric heat capacity the way the elastic forms now carry density, so the first-order capacity march reads `ρ·c_p·V` per cell and a thermal time constant is a material fact rather than an element volume standing in for one.
- Shape: a capacity column on the scalar material assignment cases with the capacity fold reading it per cell, beside the payload-supplied network capacity that arrives measured; lands in `libs/csharp/Rasm.Compute/.planning/Solver/contract.md` `[02]-[SOLVE_CONTRACT]`.
- Unlocks: transient thermal results compare against measured or code-tabulated time constants, and payload-borne network capacity stops being the lane's only honest capacity.
- Anchors: the elastic cases carry density off the seam `Mechanical` case, so the seam precedent and the accessor shape both exist; the seam `Thermal` case carries specific heat, so the read has a source; the energy-network payload supplies a measured capacity vector the fold reads where present.
- Arms: the scalar assignment cases serve conductance-only physics whose consumers read no capacity, so the widening arms when a transient thermal or network route lands a consumer reading the column.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[TWIN_SENSOR_INGEST]-[COMPLETE]: realized in `.planning/Runtime/transport#BROKER_INGEST` — `BrokerChannels.Mqtt` bridges `IMqttClient.ApplicationMessageReceivedAsync` through one bounded channel onto the identical `IAsyncEnumerable<Fin<SensorEnvelope<T>>>` the NATS pump yields, so `Capture` consumes either dialect with no arm; `AutoAcknowledge` stays false with the ack riding the successful enqueue so a shed QoS 1/2 delivery redelivers, and the W3C pair extracts from `MqttApplicationMessage.UserProperties` through `ReadValueAsString` rather than the `[Obsolete]` `Value`. Parent adoption needed no row: the kernel `TraceCarrier.Parent` and `SpanEdge.Under` already own the ingress bracket, so the leg hands the carrier outward rather than re-minting it.
[GEOMETRY_LAKE_EGRESS]-[COMPLETE]: realized in `.planning/Runtime/codecs#ARROW_BATCH` — `GeometryDataset` carries one `PackKind` corpus, `ArrowBatch.Geometry` wraps each kernel channel arena as a `FixedSizeList` column with zero gather, and `ArrowBatch.Landing` fills the custodian's `LandingArm.Geometry` row keying its tree on the kernel `PackSchema.SchemaId`; the arm arrived here because S0 reaches neither the landing coordinate nor `Apache.Arrow` and holds no geometry corpus. Category is LANDING-TIME: a content-addressed encode carries no observation clock, so the dataset declares no time column and no wall-clock metadata, which keeps a retry byte-identical.
[DOE_LAKE_EGRESS]-[COMPLETE]: realized in `.planning/Runtime/codecs#ARROW_BATCH` — `ArrowBatch.Doe`/`Chargeback` seal the batches and `ArrowBatch.Landing` projects each onto the `LakeGeneration` coordinate `Rasm.Persistence` `Query/columnar#FLAT_TABLE_EGRESS` `Land` writes; the card's Flight push is refuted — the branch's one columnar custodian owns the landing door and Flight serves reads alone, so byte framing arises only where the `topology` axis splits the processes and the composition root frames it.
[RUNTIME_HOOK_POINTS]-[COMPLETE]: realized in `.planning/Runtime/receipts#HOOK_POINTS` — `ComputeHookRail.Live` mints the five-point roster as kernel `HookPoint<TFact>` values, capsule fork-shield isolation parking `IsolatedFault` rows on the roster evidence cell; `rasm.compute.solve.iteration` landed as Replay depth 256, correcting the card's Observe claim.
[ADAPTIVE_RESOURCE_GOVERNOR]-[COMPLETE]: realized in `.planning/Runtime/scheduling#CPU_BUDGET` — `ResourceGovernor.Steer` folds typed `UtilizationSample` values into the reserve-and-memory-scale re-resolve, `JobGraph` seals the effective memory limit onto every `JobRun`, and only a budget or spill transition returns a `Governor` fact; the `IResourceMonitor`/`ResourceUtilization` snapshot API is obsolete `EXTOBS0001`, so AppHost sources samples from the package's observable instruments.
[TENANT_COST_LEDGER]-[COMPLETE]: realized in `.planning/Runtime/receipts#COST_LEDGER` — `CostPolicy`/`CostVector`/`ChargebackDataset` over the envelope-joined `ReceiptFolds.Journal` with the `rasm.compute.cost.units` lossy channel; the `libs` `[COST_ATTRIBUTION_BAGGAGE]` ripple end stays the estate tier's.
[DASHBOARD_ALERT_DESCRIPTOR]-[COMPLETE]: realized in `.planning/Runtime/receipts#DASHBOARD_DESCRIPTOR` — one kernel `BoardPack` derives its panels from `ReceiptSurface.Specs`, four `ComputeObjective` rows derive their specs from the kernel burn table, and the pack rides the contributor port so the composing root admits it whole.
[PROFILE_EVIDENCE_AXIS]-[COMPLETE]: realized in `.planning/Runtime/receipts#BENCHMARK_CLAIMS` (`ProfileArtifact` union) with `.planning/Model/inference.md` `RunOps.Profile` minting `ChromeTrace` from the admitted `ArtifactIndexRow` and the `#TELEMETRY_PROJECTION` span-identity correlation law.
[STATS_STREAM_MONITORS]-[COMPLETE]: realized as `.planning/Stats/monitor.md` — `StreamMonitor` capsules (EWMA, P², `FittedModel` detector), `MonitorChannel` receipt extraction, the `Drift` receipt case, and the `MonitorLane.AsDetector` twin projection.
[SOLVER_GEOTECHNICAL_CONSTITUTIVE]-[COMPLETE]: pressure-dependent frictional soil laws landed collapsed — `Solver/constitutive` `PlasticPotential` parameterizes `DruckerPrager`/`SmoothedMohrCoulomb`/`ModifiedCamClay` as seed data over one invariant generator, and `MaterialState` carries volumetric hardening, preconsolidation pressure, and pore pressure.
[SOLVER_ARC_LENGTH_CONTINUATION]-[COMPLETE]: `Solver/contract` `SolveMethod.ArcLength` and `ArcLengthPolicy` enforce the Crisfield displacement/load constraint through predictor-corrector iterations across limit points on the landed Newton internal-force machinery.
[ST_FDD]-[COMPLETE]: `Stats/signal` `Transform.Modal` runs the N-channel frequency-domain decomposition over Welch cross-PSD matrices, returning `ModalEstimate`/`MeasuredMode` with the full singular spectrum; `MeasuredMode` crosses to `Solver/clash#CLASH_AND_TWIN` as the FE-updating measured end.
[COMPUTE_PACK_PROVENANCE]-[COMPLETE]: `ComputeDescriptors.Board` carries `compute.receipt` as the pack's own first column and the iac `_PACKS` tuple admits that key, so one spelling serves both ends; the card's `compute.descriptor` wording is refuted — the tuple never seated it, and the receipt surface names the subject this pack projects.
