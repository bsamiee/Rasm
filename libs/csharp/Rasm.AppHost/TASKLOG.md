# [APPHOST_TASKLOG]

The open and closed work for the runtime spine, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[ADMIT_OPENIDDICT_CLIENT]-[COMPLETE]: `OpenIddict.Client` 7.5.0 pinned + referenced; README `[IDENTITY_AUTHZ]` group and `.api/api-openiddict-client.md` authored.
[ADMIT_ASPNETCORE_AUTHORIZATION]-[COMPLETE]: `Microsoft.AspNetCore.Authorization` 10.0.9 pinned + referenced (host-neutral ABAC evaluation core, no HTTP-pipeline coupling); README `[IDENTITY_AUTHZ]` group and `.api/api-authorization.md` authored; the ABAC evaluation over the principal rides `RUNTIME_IDENTITY_AUTHZ_PLANE`.
[ADMIT_SERVICEDISCOVERY]-[COMPLETE]: `Microsoft.Extensions.ServiceDiscovery` 10.7.0 pinned + referenced; README `[OUTBOUND]` group and `.api/api-service-discovery.md` authored.
[ADMIT_OPENFEATURE]-[COMPLETE]: `OpenFeature` 2.13.0 pinned + referenced (in-memory provider ships in the core SDK); README `[FEATURE_FLAGS]` group and `.api/api-openfeature.md` authored.
[DEEPEN_MODEL_ROUTING_GUARDRAILS]-[COMPLETE]: `Agent/reasoning.md#MODEL_GOVERNANCE` folds the `GoverningChatClient : DelegatingChatClient` with `UseModelSelection` (reading `ModelRoute.From(FlagVerdict)` and rewriting `ChatOptions.ModelId`) and `UseContentFilter` (over the one `RedactionRegistration` `Redactor.Redact`) into the one `ChatClientBuilder` fold both front doors share; metering rides `Charge -> GrantBroker.Admit` `CostUnit.ModelTokens` unchanged. The `Runtime/features#FLAG_VERDICT` seam is design-complete with a policy-default `ModelRoute` fallback when the features rail is absent.
[DEEPEN_FLEET_ROLL_STRATEGY_AXIS]-[COMPLETE]: `Sandbox/provisioning.md#ROLLOVER_DRAIN` carries the `RollStrategy` `[SmartEnum<string>]` axis (`Canary`/`BlueGreen`/`LinearWave`) with per-strategy `Plan`/`Advances` arms and `FleetRoll.Roll` walking `PeerRoster.Attached` in health-gated cohorts halting on the first `NotServing` node; the `ScheduleEntry.Spread` seed stays the wave cadence and the `Runtime/features#FLAG_VERDICT` strategy-select seam is design-complete with the features rail forward.
[DEEPEN_DISTRIBUTED_QUOTA_STORE_SEAM]-[COMPLETE]: `Agent/capability.md#GRANT_BROKER` carries the `DistributedBudget` seam — the live ceiling check executes INSIDE one `FencingToken.Admits`-fenced compare-and-set so two nodes presenting fresh tokens cannot both overshoot (TOCTOU foreclosed), with the per-process `Cell` the no-seam fallback; Ripple: `csharp:Rasm.Persistence` `[ONE_FENCED_LEASE_STORE]` (seam landed).
[RESTRUCTURE_AGENT_COMMAND_DISPATCH_PAGE]-[COMPLETE]: new `Agent/runtime.md` declares the one `CommandDispatch.Run(CommandIntent) -> CommandReceipt` front door over `CommandAlgebra.Run`, chaining each receipt into the one `EventLog` and serving the three callers (operator/agent/plugin) plus the orchestration step and bus subscription through one transaction — no second dispatcher, resolving the spine's dangling reference.
[COLLAPSE_FAULT_SOURCE_SUPPORT_TRIGGER]-[COMPLETE]: `Runtime/lifecycle.md#FAULT_SPINE` folds every `FaultSource` case through `FaultRecord.From` into one `SupportTrigger.FaultTransition(correlation, FaultRecord)` emitted before `PhaseTrigger.FaultCommitted`; `ProbeMarkers` host-crash-marker boot evidence rides the identical trigger case so live faults and crash markers share one fact stream the `Runtime/orchestration#CRASH_RESUME` reads.
[COLLAPSE_HEALTH_DEGRADATION_CELL]-[COMPLETE]: `Observability/health.md#DEGRADATION_RAIL` collapses the snapshot read and `Derive` into one `Atom<DegradationReading>` whose `PublishAsync` snapshots `HealthReport` and folds `Derive` in the SAME swap; the cross-process cascade seam-split (READ here, WRITE at `Wire/companion#DEGRADATION_CASCADE`) and the hysteresis/cascade-floor fields are preserved, giving the `Runtime/laneguard#LANE_GUARD` governor a race-free `(snapshot, level)` read.
[SEAM_OUTBOX_AND_WORKFLOW_PERSISTENCE_TABLE]-[COMPLETE]: the AppHost half lands — `Runtime/orchestration.md#STEP_STATE_SEAM` and `Wire/outbox.md#OUTBOX_FABRIC` both write through one tenant-scoped transaction boundary so the workflow step-state CAS row and the outbox enqueue commit atomically; Ripple: `csharp:Rasm.Persistence` `[ONE_OUTBOX_EGRESS_SPINE]` + `[ONE_FENCED_LEASE_STORE]` (both seams landed).
[DEEPEN_EVENTBUS_DATAFLOW_TOPOLOGY]-[COMPLETE]: new `Wire/topics.md` builds the in-process bus as `DrainSurface` builders over the one `DrainKind` union — `BroadcastBlock` topic fan, bounded `BufferBlock`/`ActionBlock` subscriptions with `BoundedCapacity` back-pressure, `JoinBlock`/`BatchedJoinBlock` correlate/coalesce — draining under the `DrainConductor` band; the `DeliveryFanout` dedupe cell is reused, Dataflow stays the transitive floor; Ripple: `csharp:Rasm.Persistence` `[ONE_OUTBOX_EGRESS_SPINE]` (seam landed).
[DEEPEN_LANEGUARD_RESILIENCE_GOVERNOR]-[COMPLETE]: new `Runtime/laneguard.md` registers one keyed Polly `ResiliencePipeline` per `WorkLane` (bulkhead/breaker/timeout/hedge + test-host Simmy chaos) mirroring `KeyedLane.Register`, with the `AdaptiveConcurrency` arm reading `ResourceMonitoring` and the `LoadShed` arm reading the atomic `DegradationReading`; the per-lane `ShedVerdict` is minted once; Ripple: `csharp:Rasm.Compute` `[ONE_DEGRADATION_SHED_VERDICT]` (seam landed).
[STRUCTURE_SANDBOX_GRANT_MEDIATION]-[COMPLETE]: `Sandbox/isolation.md#GRANT_HANDLE` unifies operator/agent/plugin admission into one `GrantHandleSurface.Mediate` fold where `CallerModality` is a `BrokeredCall` discriminant, running one `Scope.Covers` gate and one `GrantBroker.Admit` charge; the no-ambient-authority law is preserved (plugin path carries only scope + dispatch-closure) and the `RuntimePolicy` ABAC gate hooks before the scope check; Ripple: `csharp:Rasm.Persistence` `[ONE_IDENTITY_STORE]` + `[ONE_FENCED_LEASE_STORE]` (seams landed).
