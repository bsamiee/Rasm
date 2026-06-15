# [APPHOST_FEATURES]

Isolation-concept atlas for the runtime spine. Every concept is a row or case on a budgeted
DENSITY_BAR owner, never a new surface; mechanics live at the `.planning/` page#cluster anchors
named on each row. The `[STATE]` column mirrors the charter `GAP_LEDGER` model: `FINALIZED` when
the concept is realized as a transcription-complete `csharp` fence in an owning page, `PARTIAL`
when the fence exists but a column/case/route is gated on a RESEARCH row, `MISSING` when no fence
owns it yet, `SPIKE` when it is gated on a live-host bridge, native, or server-provisioning probe.

## [1]-[ISOLATION_SPINE]

The spine concepts this folder owns and no sibling reverses — the host-variance, lifecycle, time,
configuration, composition, resource, telemetry, health, support, and outbound axes that boot,
degrade, drain, and report every modality with zero app-side ceremony.

| [INDEX] | [CONCEPT]                                                                     | [SPINE]                                                                  | [OWNER]                          | [PAGE#CLUSTER]                                                              |  [STATE]  |
| :-----: | :---------------------------------------------------------------------------- | :---------------------------------------------------------------------- | :------------------------------- | :-------------------------------------------------------------------------- | :-------: |
|   [1]   | Eight-modality boot from one variance axis                                    | profile row → `Resolve` fold → `ResolvedProfile` → `Boot`               | `HostProfile`/`ProfileBoot`      | host-profiles#PROFILE_AXIS, host-profiles#LIFETIME_ADAPTERS                  | FINALIZED |
|   [2]   | Per-user roots + telemetry resource identity from the resolved record         | `ProfileRoots` fold → `ResourceAttributes` projection                   | `ProfileIdentity`                | host-profiles#RESOURCE_IDENTITY                                              | FINALIZED |
|   [3]   | sd_notify service-state lifecycle mirror (Ready/Stopping, watchdog keep-alive)| committed `PhaseReceipt` → `ServiceNotify` table → `ISystemdNotifier`    | `ProfileBoot`                    | host-profiles#LIFETIME_ADAPTERS                                             |  PARTIAL  |
|   [4]   | Total phase machine with CAS-committed receipts and LIFO subscriptions        | `RuntimePhase` × `PhaseTrigger` → `Next` law → `PhaseReceipt`           | `Lifecycle`                      | lifecycle-and-drain#PHASE_FAMILY                                            | FINALIZED |
|   [5]   | Crash-recovery + upgrade boot probe (own marker + host `.rhl`/`.ips` markers) | `BootMarker` write/clear/probe → `HostCrashMarker` → support trigger     | `FaultSpine`                     | lifecycle-and-drain#FAULT_SPINE                                            |  PARTIAL  |
|   [6]   | Unhandled/unobserved/POSIX fault traps with SIGHUP reload + SIGTERM drain     | `ArmTraps` LIFO detacher → `FaultSource` commit → phase transition       | `FaultSpine`                     | lifecycle-and-drain#FAULT_SPINE                                            | FINALIZED |
|   [7]   | Rank-band cooperative→forced drain with per-step straggler receipts           | `DrainBand` order fold → `DrainStep` → `DrainReceipt`                    | `DrainConductor`                 | lifecycle-and-drain#DRAIN_CONDUCTOR                                        | FINALIZED |
|   [8]   | One cancellation root; every token derives with provenance + deadline         | `CancelScope.Root` → `Derive` linked tokens                              | `CancelScope`                    | lifecycle-and-drain#CANCEL_SPINE                                          | FINALIZED |
|   [9]   | One injected clock pair: elapsed vs semantic time, sentinel admission         | `ClockPolicy(TimeProvider, IClock)` → `Admit`/`Persisted`/`Exported`     | `ClockPolicy`                    | time-and-deadlines#CLOCK_SPLIT                                            | FINALIZED |
|  [10]   | Nine-row deadline taxonomy; every suite duration literal traces to a row      | `DeadlineClass` rows → `Escalation` arc → `DeadlineReceipt`              | `DeadlineClass`                  | time-and-deadlines#DEADLINE_TAXONOMY                                      | FINALIZED |
|  [11]   | One scheduler: cron + period rows, lease split, missed-occurrence sweep       | `ScheduleEntry` × `OccurrenceSpec` → `Run`/`Missed`/`Window`             | `SchedulePort`                   | time-and-deadlines#SCHEDULE_PORT                                          | FINALIZED |
|  [12]   | Watchdog heartbeat as a schedule row folding a not-met receipt to a trigger   | `Heartbeat` fold → `SupportTrigger.WatchdogTimeout`                      | `SchedulePort`                   | time-and-deadlines#SCHEDULE_PORT, support-bundles#TRIGGER_UNION           | FINALIZED |
|  [13]   | Eight ranked config sources mounted onto one chain, rank-fold precedence      | `ConfigSource` rows → `Compose` rank fold → `ConfigurationManager`       | `ConfigSource`                   | configuration-and-options#SOURCE_AXIS                                     | FINALIZED |
|  [14]   | Fail-closed source-gen binding into validated immutable policy records        | `Bind<T>` → `Validation<ConfigError,T>` accumulate                        | `PolicyBinding`/`ConfigError`    | configuration-and-options#TYPED_BINDING                                   | FINALIZED |
|  [15]   | Validate-once frozen publish with reload-class-gated receipted transitions    | `Admit<T>` → `ReloadClass` switch → `ReloadOutcome` → `ReloadReceipt`     | `OptionsAdmission`               | configuration-and-options#POLICY_VALUES                                   | FINALIZED |
|  [16]   | Operator kill-switch as one transition-class row forcing the degradation fold | `KillSwitchConfig` → `OperatorOverride` → `DegradationCell.Force`         | `OperatorOverride`               | configuration-and-options#KILL_SWITCH, health-and-degradation#DEGRADATION_RAIL | FINALIZED |
|  [17]   | One composition root: frozen module table folds + freezes the service graph   | `ModuleContribution` rows → `Compose` one-pass fold → `MakeReadOnly`     | `CompositionSurface`             | composition-and-modules#MODULE_TABLE, composition-and-modules#SCAN_AND_DECORATE | FINALIZED |
|  [18]   | Keyed decoration of contributor ports, profile-conditional via `TryDecorate`  | `DecorationRow` column → `Decorate`/`TryDecorate` → `ContributionReceipt`| `BoundaryActivation`             | composition-and-modules#BOUNDARY_ACTIVATION                              | FINALIZED |
|  [19]   | Admission-edge activation: availability probe, async drain scope, validators  | `Activate`/`Available`/`Scoped` → cached `ObjectFactory` plan            | `BoundaryActivation`             | composition-and-modules#BOUNDARY_ACTIVATION                              | FINALIZED |

## [2]-[RESOURCE_AND_OUTBOUND]

Bounded runtime resource lanes and the single outbound boundary — the cache, pool, queue, and hop
axes a downstream app would otherwise hand-roll per call site.

| [INDEX] | [CONCEPT]                                                                     | [SPINE]                                                                  | [OWNER]                          | [PAGE#CLUSTER]                                                              |  [STATE]  |
| :-----: | :---------------------------------------------------------------------------- | :---------------------------------------------------------------------- | :------------------------------- | :-------------------------------------------------------------------------- | :-------: |
|  [20]   | Lane-keyed hybrid cache: stampede single-flight, tag cut, per-lane keyed L2   | `CacheLane` rows → `Read`/`Invalidate` → `AddKeyedHybridCache`           | `CacheLane`/`CacheSurface`       | resource-lanes#CACHE_PORT                                                   | FINALIZED |
|  [21]   | Delegate-row object pools: reset law, sanity predicate, leak-tracked test row | `PoolPolicy<T>` → `Get`/`Recycle` → `ObjectPool.Create`                  | `PoolPolicy<T>`/`Pools`          | resource-lanes#OBJECT_POOLS                                                 | FINALIZED |
|  [22]   | Bounded drainable queues with receipted loss; pipe vs network split          | `DrainSpec` rows → `DrainQueue<T>` union → `Drained` under conductor      | `DrainQueue<T>`/`DrainSurface`   | resource-lanes#DRAIN_QUEUES                                                 | FINALIZED |
|  [23]   | Seven outbound hop cases bound to frozen policy rows with total dispatch      | `OutboundHop` union → `HopPolicy` row → `Policy` switch                  | `OutboundHop`/`HopRows`          | outbound-resilience#HOP_AXIS                                                | FINALIZED |
|  [24]   | Standard + hedging HTTP pipelines; weighted multi-region routing as a subrow  | `HttpLane.Wire` → idempotency dispatch → `AddStandard`/`AddHedging`       | `HttpLane`                       | outbound-resilience#HTTP_PIPELINES                                         |  PARTIAL  |
|  [25]   | One keyed Polly registry per non-HTTP hop; breaker read/write split; chaos    | `KeyedLane.Register` fold → retry/breaker/fallback/timeout strategies     | `KeyedLane`/`GrpcChannelPolicy`  | outbound-resilience#KEYED_PIPELINES                                        |  PARTIAL  |
|  [26]   | Companion discovery manifest + UDS attach + checksum gate + child lifecycle   | `DiscoveryManifest` → `Publish`/`Read`/`Compatible`/`Connect`/`Spawn`     | `Discovery`/`CompanionChild`     | outbound-resilience#DISCOVERY_ATTACH                                       |  PARTIAL  |
|  [27]   | One retry owner per hop: claim cell, conflict receipt, kill-switch enforce    | `OutboundSurface.Claim`/`Run`/`Guarded`/`Enforce` → `HopReceipt`         | `OutboundSurface`/`OutboundRuntime` | outbound-resilience#OWNERSHIP_LAW                                       | FINALIZED |

## [3]-[OBSERVABILITY_AND_HARDENING]

The unified observability and hardening posture — traces/metrics/logs through minted identities,
the HLC causal primitive, redaction at every egress, and bounded redacted support capture. One
posture, owned here, never re-derived by an app.

| [INDEX] | [CONCEPT]                                                                     | [SPINE]                                                                  | [OWNER]                          | [PAGE#CLUSTER]                                                              |  [STATE]  |
| :-----: | :---------------------------------------------------------------------------- | :---------------------------------------------------------------------- | :------------------------------- | :-------------------------------------------------------------------------- | :-------: |
|  [28]   | Minted source + meter identity with a source-gen instrument registry          | `TelemetrySource` rows → `Mint` → `HostMetrics` attribute partials       | `TelemetrySource`/`TelemetryIdentity` | diagnostics-and-telemetry#TELEMETRY_IDENTITY                          |  PARTIAL  |
|  [29]   | One boot-minted correlation id stamped across every signal and hop            | `Correlation.Stamp` → `Baggage`/`LogContext`/`TextMapPropagator`         | `Correlation`                    | diagnostics-and-telemetry#CORRELATION_SPINE                                | FINALIZED |
|  [30]   | Per-profile log pipeline arbitration: serilog-projection vs otel-export       | `LogPipeline.Owner(profile)` → `SerilogProjectionPolicy.Shape`          | `LogPipeline`                    | diagnostics-and-telemetry#LOG_PROJECTION                                   |  PARTIAL  |
|  [31]   | Source-gen lib-level log delegates with sink-loss listener fold               | `SpineLog` `[LoggerMessage]` → `SpineLossFold : ILoggingFailureListener` | `SpineLog`/`SpineLossFold`       | diagnostics-and-telemetry#LOG_PROJECTION                                   |  PARTIAL  |
|  [32]   | Per-signal governance: sampling ratio, buffering, exemplars, metric views     | `TelemetrySignal` rows + `GOVERNANCE_VALUES` → `SignalGovernance.Govern`  | `TelemetrySignal`/`SignalGovernance` | diagnostics-and-telemetry#SIGNAL_GOVERNANCE                            |  PARTIAL  |
|  [33]   | Three-phase latency-context recorder (drain/hop/capture) drained at the band  | `LatencyCheckpoint` rows → `LatencySpine.Mark`/`Seal` → `LatencyData`     | `LatencySpine`/`LatencyCheckpoint` | diagnostics-and-telemetry#SIGNAL_GOVERNANCE                              |  PARTIAL  |
|  [34]   | OTLP export backend-parameterized by env (Prometheus/Tempo/Loki/Alloy)        | `OtlpExport` row → `UseOtlpExporter` after three signals → `OTEL_EXPORTER_OTLP_*` | `SignalGovernance`         | diagnostics-and-telemetry#SIGNAL_GOVERNANCE, runtime-ports#WIRE_LAW       |   SPIKE   |
|  [35]   | Seven-row classification taxonomy; redaction bound at every exporter seam      | `DataClassification` rows × `RedactorKind` → `RedactionRegistration.Bind`| `DataClassification`             | diagnostics-and-telemetry#REDACTION_TAXONOMY                              |  PARTIAL  |
|  [36]   | Resource-pressure health fold with container-limit (cgroup-v2) grading         | `HealthContributorRow` rows + `PressurePolicy.Grade` → `HealthSnapshot`  | `HealthContributorRow`/`PressurePolicy` | health-and-degradation#HEALTH_FOLD                                  |  PARTIAL  |
|  [37]   | Five-level usable-failure degradation rail: retained capability sets, hysteresis| `DegradationLevel` rows → `DegradationPolicy.Derive` → `DegradationState` | `DegradationLevel`/`DegradationCell` | health-and-degradation#DEGRADATION_RAIL                              | FINALIZED |
|  [38]   | Wire health: tag-predicate registry → `grpc.health.v1`; set-degradation route | `WireHealthRow` → `Evaluate` → serving/not-serving projection            | `WireHealthRow`/`WireHealth`     | health-and-degradation#WIRE_HEALTH                                        |   SPIKE   |
|  [39]   | Six-cause support trigger union auto-arming on fault/health/watchdog/schedule  | `SupportTrigger` union → `Facts` total dispatch                          | `SupportTrigger`                 | support-bundles#TRIGGER_UNION                                             | FINALIZED |
|  [40]   | Bounded redacted capture: window freeze, ordered fan-in, caps, coalesce        | `SupportCapture.Capture` → redact-before-write → `Capped` → zip bundle    | `SupportCapture`/`SupportLedger` | support-bundles#CAPTURE_PIPELINE, support-bundles#MANIFEST_RECEIPT        |  PARTIAL  |
|  [41]   | Retention sweep: max-bundle + max-age eviction as a schedule row               | `SupportLedger.Sweep`/`Evict` → `SupportReceipt.Evicted`                  | `SupportLedger`                  | support-bundles#MANIFEST_RECEIPT                                          | FINALIZED |
|  [42]   | Process-dump artifact row (dump + gcdump) for support capture                  | `SupportArtifact` row → `Produce` over the dump tool                      | `SupportCapture`                 | support-bundles#CAPTURE_PIPELINE                                          |   SPIKE   |

## [4]-[CROSS_PACKAGE_PORTS]

The seven inward port records — the only cross-package seam — plus the suite wire law and TS map.
Siblings adapt to these; the dependency never reverses.

| [INDEX] | [CONCEPT]                                                                     | [SPINE]                                                                  | [OWNER]                          | [PAGE#CLUSTER]                                                              |  [STATE]  |
| :-----: | :---------------------------------------------------------------------------- | :---------------------------------------------------------------------- | :------------------------------- | :-------------------------------------------------------------------------- | :-------: |
|  [43]   | HLC-stamped receipt envelope as the sole cross-process causal primitive       | `ReceiptSinkPort.Send` → `Advance` HLC → `ReceiptEnvelope` w/ `SkewBound`| `ReceiptSinkPort`                | runtime-ports#PORT_RECORDS                                                  | FINALIZED |
|  [44]   | Seven typed inward ports; an eighth is the named defect                       | `TelemetryContributorPort` + six siblings → `TryAddEnumerable` rows      | `ReceiptSinkPort` + six siblings | runtime-ports#PORT_RECORDS                                                  | FINALIZED |
|  [45]   | One Strict JSON wire law per package; app roots merge resolvers, emit schemas | `AppHostWireContext` → `SuiteContracts.Wire`/`Schema` → `NodaPatterns`   | `AppHostWireContext`/`SuiteContracts` | runtime-ports#WIRE_LAW                                                 | FINALIZED |
|  [46]   | TS dashboard contract: one envelope, schema-derived types, codec residence    | `ReceiptEnvelopeWire<TPayload>` + per-cluster `TS_PROJECTION` wire shapes | `RasmPackage`/wire records       | runtime-ports#TS_PROJECTION, lifecycle/health/support#TS_PROJECTION         | FINALIZED |

## [5]-[STATE_LEDGER]

`PARTIAL`/`SPIKE` rows above are gated on the page RESEARCH rows and the charter
`CATALOGUE_PENDING` app-root pins; the gate is the only barrier to `FINALIZED` and never a missing
owner. Phase 3 absorbs each derived row; Phase 5 reconciles state; Phase 7 confirms no unaccounted
`MISSING` survives. No concept in this atlas is `MISSING` — every isolation concept already rides a
budgeted owner with a fence; the open states are catalogue-grounding or probe gates.

| [STATE]   | [ROWS]                                              | [GATE]                                                                                          |
| :-------- | :-------------------------------------------------- | :--------------------------------------------------------------------------------------------- |
| FINALIZED | 1,2,4,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,27,29,37,39,41,43,44,45,46 | transcription-complete fence in the owning page                          |
| PARTIAL   | 3,5,24,25,26,28,30,31,32,33,35,36,40                | one RESEARCH row gates a column/route — host-profiles#WATCHDOG_PING; lifecycle FAULT host-marker path; outbound DEPENDENCY_METADATA/FALLBACK_ACTION/PEER_CREDENTIAL; diagnostics METRIC_SOURCE_GEN/LOGGING_FAILURE_LISTENER/LATENCY_TOKEN_RESOLUTION/REDACTION_REGISTRATION/APP_ROOT_BRIDGE/BUFFER_RULE_WIRING; health CONTAINER_LIMIT_READ; support process-dump produce |
| SPIKE     | 34,38,42                                             | OTLP exporter app-root catalogue (CATALOGUE_PENDING `OpenTelemetry.Exporter.OpenTelemetryProtocol`); `grpc.health.v1` wire registration (CATALOGUE_PENDING `Grpc.AspNetCore.HealthChecks`); dump/gcdump tool admission |

## [6]-[ROUTING]

- Concept mechanics: `.planning/<page>.md` at the cluster anchor on each row.
- Owner budgets and the public-surface cap: `.planning/README.md#DENSITY_BAR` (39 rows, AH-01..AH-11 + AH-04a-d).
- Gap closure and version ledger: `.planning/README.md#GAP_LEDGER`, `#ADMISSIONS_RECORD`, `#CATALOGUE_PENDING`.
- Package API truths grounding each fence: `.api/*.md` (routed by `.api/README.md`).
- Cross-package and multi-process topologies consuming these owners: `../../.planning/FEATURES.md`.
