# [APPHOST_PLANNING]

Rasm.AppHost has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — they are never re-designed downstream. The runtime spine owns host variance, lifecycle, time, configuration, composition, resource lanes, telemetry, health, support capture, outbound resilience, post-fetch provisioning and update, the inbound control-service host with its process-modality and degradation-cascade legs, and the cross-package port surface; siblings adapt to its ports and never reverse the dependency.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                    | [OWNS]                                           |  [STATE]  |
| :-----: | --------------------------------------------------------- | :----------------------------------------------- | :-------: |
|   [1]   | [host-profiles](host-profiles.md)                         | host-variance axis; modality rows                | finalized |
|   [2]   | [lifecycle-and-drain](lifecycle-and-drain.md)             | phase, transition, drain, cancellation, markers  | finalized |
|   [3]   | [time-and-deadlines](time-and-deadlines.md)               | clocks, deadlines, schedule port, test clocks    | finalized |
|   [4]   | [configuration-and-options](configuration-and-options.md) | source layering, binding, frozen policy, reloads | finalized |
|   [5]   | [composition-and-modules](composition-and-modules.md)     | composition root and module contribution rows    | finalized |
|   [6]   | [resource-lanes](resource-lanes.md)                       | cache lane, object pools, drainable queues       | finalized |
|   [7]   | [diagnostics-and-telemetry](diagnostics-and-telemetry.md) | correlation, signals, classification taxonomy    | finalized |
|   [8]   | [health-and-degradation](health-and-degradation.md)       | health fold and degradation rail                 | finalized |
|   [9]   | [support-bundles](support-bundles.md)                     | bounded redacted diagnostic capture              | finalized |
|  [10]   | [outbound-resilience](outbound-resilience.md)             | hop axis, single retry owner, discovery manifest, UDS attach, companion spawn | finalized |
|  [11]   | [runtime-ports](runtime-ports.md)                         | port records, suite wire law, TS map             | finalized |
|  [12]   | [provisioning-and-update](provisioning-and-update.md)     | post-fetch update rail, channel axis, rollover drain | finalized |
|  [13]   | [companion-sidecar](companion-sidecar.md)                 | process modality, control-service host, degradation cascade, peer admission | finalized |

## [2]-[WIRE_PAGES]

lifecycle-and-drain · health-and-degradation · support-bundles · runtime-ports (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

App-root-only pins with no restored closure, catalogued at app-root creation: OpenTelemetry.Exporter.OpenTelemetryProtocol, System.CommandLine, Grpc.AspNetCore, Grpc.AspNetCore.Web, Grpc.AspNetCore.HealthChecks, Microsoft.AspNetCore.JsonPatch.SystemTextJson, Serilog.Extensions.Hosting (AddSerilog app-root bridge).

## [4]-[GAP_LEDGER]

Every row is CLOSED; `[CLOSED_BY]` names the page#cluster that absorbed the gap.

| [INDEX] | [GAP]                                            | [CLOSED_BY (page#cluster)]                                                                                         |
| :-----: | :----------------------------------------------- | :----------------------------------------------------------------------------------------------------------------- |
|   [1]   | sibling telemetry registration seam              | runtime-ports#PORT_RECORDS + diagnostics-and-telemetry#TELEMETRY_IDENTITY                                          |
|   [2]   | cross-package drain rank bands                   | lifecycle-and-drain#DRAIN_CONDUCTOR                                                                                |
|   [3]   | shared data-classification taxonomy              | diagnostics-and-telemetry#REDACTION_TAXONOMY                                                                       |
|   [4]   | HLC receipt envelope as causal primitive         | runtime-ports#PORT_RECORDS                                                                                         |
|   [5]   | `grpc.health.v1` registry projection             | health-and-degradation#WIRE_HEALTH                                                                                 |
|   [6]   | `DrainQueue` / `WorkLane` name split             | resource-lanes#DRAIN_QUEUES                                                                                        |
|   [7]   | `HybridCache` port, stampede, tags, and L2 seam  | resource-lanes#CACHE_PORT                                                                                          |
|   [8]   | options reload-class column                      | configuration-and-options#POLICY_VALUES                                                                            |
|   [9]   | resource-pressure health contributor             | health-and-degradation#HEALTH_FOLD                                                                                 |
|  [10]   | user-secrets versus OS-keychain config rows      | configuration-and-options#SOURCE_AXIS                                                                              |
|  [11]   | update-check outbound hop                        | outbound-resilience#HOP_AXIS                                                                                       |
|  [12]   | unhandled, task, POSIX, and crash-marker faults  | lifecycle-and-drain#FAULT_SPINE                                                                                    |
|  [13]   | discovery manifest and local IPC attach          | outbound-resilience#DISCOVERY_ATTACH + runtime-ports#PORT_RECORDS                                                  |
|  [14]   | `LocalOnly` degradation level                    | health-and-degradation#DEGRADATION_RAIL                                                                            |
|  [15]   | operator kill-switch and degradation consequence | configuration-and-options#KILL_SWITCH + health-and-degradation#DEGRADATION_RAIL                                    |
|  [16]   | cron schedule port and lease/crash split         | time-and-deadlines#SCHEDULE_PORT                                                                                   |
|  [17]   | merged JSON contracts and schema emission        | runtime-ports#WIRE_LAW                                                                                             |
|  [18]   | crash probe, support trigger, and host markers   | lifecycle-and-drain#FAULT_SPINE + support-bundles#TRIGGER_UNION                                                    |
|  [19]   | service-mode control verbs                       | support-bundles#CAPTURE_PIPELINE + health-and-degradation#DEGRADATION_RAIL + configuration-and-options#KILL_SWITCH |
|  [20]   | GC posture and DATAS benchmark gate              | host-profiles#PROFILE_AXIS                                                                                         |
|  [21]   | post-fetch update orchestration (download/stage/rollover/rollback) | provisioning-and-update#UPDATE_RAIL + provisioning-and-update#CHANNEL_AXIS + provisioning-and-update#ROLLOVER_DRAIN |
|  [22]   | inbound gRPC ControlService host (serving side of every control verb) | companion-sidecar#CONTROL_SERVICE + companion-sidecar#SERVICE_HOST                                  |
|  [23]   | process-modality lifecycle (companion/sidecar/paired spawn-attach-discovery) | companion-sidecar#PROCESS_MODALITY                                                          |
|  [24]   | cross-process degradation cascade (parent floor to child cell)    | companion-sidecar#DEGRADATION_CASCADE + health-and-degradation#DEGRADATION_RAIL                                    |
|  [25]   | accept-side UDS peer-credential read                              | companion-sidecar#PEER_ADMISSION                                                                                   |

## [5]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail; density means
no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its
owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a
new surface. The owner budget below is the public-surface cap. Key policies, receipts, policy
records, runtime records, and wire shapes ride inside their owner's signature region (ledger rows
AH-01 through AH-11 and AH-04a-d); a public type outside those regions is the named defect.

The `[STATE]` column carries `FINALIZED` where the owner is a transcription-complete fence with
no open gate and `SPIKE` where the owner is fence-complete but its proof carries a residual
native, bridge, or live-server probe named in the page RESEARCH cluster; a SPIKE owner is fully
shaped now, never a deferred surface.

| [INDEX] | [AXIS/CONCERN]          | [OWNER]                          | [KIND]                  | [CASES]                                                     |  [STATE]  |
| :-----: | :---------------------- | :------------------------------- | :---------------------- | :---------------------------------------------------------- | :-------: |
|   [1]   | host variance           | `HostProfile`                    | `[SmartEnum<string>]`   | 8 rows                                                      | FINALIZED |
|   [2]   | lifetime adapters       | `ProfileBoot`                    | static fold             | 6 delegate targets; `ServiceNotify`/`MirrorService`/`Emit` mirror sub-surface | SPIKE |
|   [3]   | resource identity       | `ProfileIdentity`                | static fold             | 5 attribute rows; `HostResourceDetector` detector sub-surface | FINALIZED |
|   [4]   | phase family            | `RuntimePhase`                   | `[SmartEnum<string>]`   | 8 rows                                                      | FINALIZED |
|   [5]   | trigger vocabulary      | `PhaseTrigger`                   | `[Union]`               | 10 cases                                                    | FINALIZED |
|   [6]   | transition cell         | `Lifecycle`                      | boundary capsule        | 1 CAS entry                                                 | FINALIZED |
|   [7]   | fault spine             | `FaultSource`                    | `[Union]`               | 4 cases                                                     | FINALIZED |
|   [8]   | drain bands             | `DrainBand`                      | `[SmartEnum<int>]`      | 4 rows                                                      | FINALIZED |
|   [9]   | cancellation spine      | `CancelScope`                    | record capsule          | 1 root                                                      | FINALIZED |
|  [10]   | clock seam              | `ClockPolicy`                    | record                  | 2 admissions                                                | FINALIZED |
|  [11]   | deadline taxonomy       | `DeadlineClass`                  | `[SmartEnum<string>]`   | 9 rows                                                      | FINALIZED |
|  [12]   | schedule port           | `ScheduleEntry`                  | record port             | 4 `OccurrenceSpec` cases (cron, every, annual, fleet); `CronCadence` jitter-template axis, `Spread` schedule-key seed | FINALIZED |
|  [13]   | config sources          | `ConfigSource`                   | `[SmartEnum<string>]`   | 8 rows                                                      | FINALIZED |
|  [14]   | binding faults          | `ConfigError`                    | `[Union]` fault         | 7 cases                                                     | FINALIZED |
|  [15]   | reload rail             | `ReloadOutcome`                  | `[Union]`               | 4 cases                                                     | FINALIZED |
|  [16]   | kill switch             | `OperatorOverride`               | `[Union]`               | 2 cases                                                     | FINALIZED |
|  [17]   | module table            | `ModuleContribution`             | record row              | 1 row per package; `DecorationRow` carrier column           | FINALIZED |
|  [18]   | composition fold        | `CompositionSurface`             | static fold             | 1 receipted entry                                           | FINALIZED |
|  [19]   | boundary activation     | `BoundaryActivation`             | static surface          | 1 entry family                                              | FINALIZED |
|  [20]   | cache lanes             | `CacheLane`                      | `[SmartEnum<string>]`   | 3 rows                                                      | FINALIZED |
|  [21]   | object pools            | `PoolPolicy<T>`                  | policy row              | 1 row per type; `Pools` registration sub-surface            | FINALIZED |
|  [22]   | drain queues            | `DrainQueue<T>`                  | `[Union]`               | 2 cases; `DrainKind` 5-row topology (pipe, network, fan-out, correlated-join, dual-coalesce), `DrainSurface` block builders | FINALIZED |
|  [23]   | telemetry identity      | `TelemetrySource`                | `[SmartEnum<string>]`   | 6 rows                                                      | FINALIZED |
|  [24]   | correlation spine       | `Correlation`                    | static surface          | 1 boot mint; `RootEnricher`/`CausalEnricher` cost-class seats | FINALIZED |
|  [25]   | log arbitration         | `LogPipeline`                    | `[SmartEnum<string>]`   | 2 rows; `SpineLossFold` listener, `HostTags` tag-provider   | FINALIZED |
|  [26]   | signal governance       | `TelemetrySignal`                | `[SmartEnum<string>]`   | 3 rows; `LatencyCheckpoint`/`LatencySpine` carrier          | FINALIZED |
|  [27]   | classification taxonomy | `DataClassification`             | `[SmartEnum<string>]`   | 7 rows; `RedactorKind` column, `RedactionRegistration` fold | FINALIZED |
|  [28]   | health fold             | `HealthContributorRow`           | record row + probe      | 4 tag families; `PressurePolicy` `ResourceQuota` grade, `UtilizationCell` MeterListener seat over the ResourceMonitoring observable instruments | FINALIZED |
|  [29]   | capability vocabulary   | `Capability`                     | `[SmartEnum<string>]`   | 6 rows                                                      | FINALIZED |
|  [30]   | degradation rail        | `DegradationLevel`               | `[SmartEnum<string>]`   | 5 rows                                                      | FINALIZED |
|  [31]   | wire health             | `WireHealthRow`                  | record row              | 1 row per service                                           | FINALIZED |
|  [32]   | support triggers        | `SupportTrigger`                 | `[Union]`               | 6 cases                                                     | FINALIZED |
|  [33]   | support receipts        | `SupportReceipt`                 | `[Union]`               | 3 cases                                                     | FINALIZED |
|  [34]   | hop axis                | `OutboundHop`                    | `[Union]`               | 7 cases                                                     | FINALIZED |
|  [35]   | hop faults              | `HopFault`                       | `[Union]` fault         | 7 cases                                                     | FINALIZED |
|  [36]   | hop outcomes            | `HopOutcome`                     | `[Union]`               | 3 cases                                                     | FINALIZED |
|  [37]   | discovery attach        | `DiscoveryManifest`              | record + static surface | 1 manifest law                                              | SPIKE     |
|  [38]   | runtime ports           | `ReceiptSinkPort` + six siblings | sealed records          | 7 ports                                                     | FINALIZED |
|  [39]   | wire law                | `AppHostWireContext`             | JsonSerializerContext   | 9 contract rows; `NodaPatterns` pattern sub-surface         | FINALIZED |
|  [40]   | update phases           | `UpdatePhase`                    | `[SmartEnum<string>]`   | 5 rows; `UpdateOutcome`/`UpdateFault` unions, `UpdateMetrics` source-gen instrument partial | FINALIZED |
|  [41]   | update rail             | `UpdateRail`                     | boundary capsule        | 1 `UpdateManager` handle; `Stage`/`Rollover`/`Resume` rails, staged-pending probe | FINALIZED |
|  [42]   | update channels         | `UpdateChannel`                  | `[SmartEnum<string>]`   | 3 rows (stable, beta, canary); feed/explicit-channel/downgrade columns | FINALIZED |
|  [43]   | rollover drain          | `RolloverDrain`                  | static surface          | 1 drain-before-swap fold over `DrainConductor`              | FINALIZED |
|  [44]   | process modality        | `ProcessModality`                | `[SmartEnum<string>]`   | 3 rows (companion, sidecar, paired-peer); `ModalityRow` columns, `CompanionPeer` capsule | FINALIZED |
|  [45]   | control service host    | `ControlInbound`                 | static handler          | 3 verbs folding onto degradation/options/support owners; `ControlRuntime`/`VerbReceipt` | FINALIZED |
|  [46]   | service host            | `ServiceHost`                    | static surface          | gRPC server registration; `ControlTransport` 2-case union (UDS, named-pipe), `PipeHardening` | FINALIZED |
|  [47]   | degradation cascade     | `DegradationCascade`             | static write surface    | 1 parent-to-child `Cascade` write; `CascadeReceipt`        | SPIKE     |
|  [48]   | peer admission          | `PeerAdmission`                  | static accept-side read | 2 platform branches (Linux `SO_PEERCRED`, macOS `LOCAL_PEERCRED`); `Ucred`/`Xucred`/`PeerCredential` | FINALIZED |

## [6]-[BUILD_ORDER]

Vocabulary owners land before their consumers; shapes before rails, rails before dispatch,
boundaries before composition. `Ports.cs` lands last because `AppHostWireContext` rows reference
receipts every earlier file declares. `CorrelationId` is owned at
lifecycle-and-drain#PHASE_FAMILY; `Diagnostics.cs` consumes it and never re-declares.
Drain band literals live only on `DrainBand`; sibling registrations arrive as
`DrainParticipantPort` rows and never copy them. `DrainQueue` is the AppHost name;
`WorkLane` stays at Compute. Comparer accessors stay package-local, one per axis owner.
`Outbound.cs` transcribes `outbound-resilience.md` including its DISCOVERY_ATTACH cluster, where
the `LocalIpc` hop case carries the `DiscoveryManifest` payload and `Discovery.Connect` consumes
`GrpcChannelPolicy`, so discovery is never a second file.

Cluster names are page-local anchors; gates name the extra proof beyond the standard static build row.

| [INDEX] | [FILE]             | [CLUSTERS]                                     | [PROOF]                                     |
| :-----: | :----------------- | :--------------------------------------------- | :------------------------------------------ |
|   [1]   | `Time.cs`          | clock split, deadlines, schedule port          | fake-clock deadlines and occurrences        |
|   [2]   | `Profiles.cs`      | profile axis, lifetime adapters, resource ids  | Resolve/Roots matrix; `host_boot_drain`     |
|   [3]   | `Lifecycle.cs`     | phase, fault, drain, cancellation              | transition matrix and marker round-trip     |
|   [4]   | `Configuration.cs` | sources, binding, policy, kill switch          | rank fold, fail-closed bind, reload gating  |
|   [5]   | `Composition.cs`   | module table, scan/decorate, activation        | `ValidateOnBuild` composition               |
|   [6]   | `ResourceLanes.cs` | cache port, object pools, drain queues         | tag cut, reset, and loss receipts           |
|   [7]   | `Diagnostics.cs`   | telemetry identity, correlation, logs, signals | `FakeLogCollector` and `MetricCollector<T>` |
|   [8]   | `Health.cs`        | health fold, degradation, wire health          | hysteresis and forced state precedence      |
|   [9]   | `Support.cs`       | trigger union, capture pipeline, manifest      | coalescing, caps, and eviction              |
|  [10]   | `Outbound.cs`      | hop axis, pipelines, discovery, ownership      | admission, owner conflicts, UDS attach      |
|  [11]   | `Ports.cs`         | port records and wire law                      | HLC advance and wire round-trip             |
|  [12]   | `Provisioning.cs`  | update rail, channel axis, rollover drain      | downgrade foreclosure, drain-before-swap, staged-pending resume |
|  [13]   | `Companion.cs`     | process modality, control service, service host, cascade, peer admission | verb folds, UDS/named-pipe intake, cascade convergence, peer-cred read |

`Provisioning.cs` lands after `Ports.cs` because `UpdateRail` consumes `Lifecycle`,
`DrainConductor`, `ReceiptSinkPort`, and the `AppHostWireContext.UpdateReceipt` row every earlier
file declares; the `UpdateCheck` detect-leg hop stays at `Outbound.cs`, so provisioning is the
post-fetch state machine only. `Companion.cs` lands last because `ControlInbound` folds onto
`DegradationCell` (Health), `OptionsAdmission` (Configuration), `SupportCapture` (Support), and
`ProcessModality` composes `Discovery`/`CompanionChild`/`GrpcChannelPolicy` from `Outbound.cs`
without re-declaring the dial-out mechanics; `DegradationCascade` is a write consumer of
`DegradationCell.Cascade`, never a second degradation owner. The gRPC server-host packages enter
only at service app roots behind the app-root pin and never below a plugin row.

TS_PROJECTION clusters carry no C# build row; they transcribe into the TS workspace at web
app-root creation.

## [7]-[FILE_PROCESS]

1. Read this charter end to end.
2. Read every page in the file's TRANSCRIBES cell end to end before the first edit.
3. Transcribe signature fences verbatim; add only file-organization scaffolding (section
   separators, usings, file-scoped namespace). Page boundary lines name every statement
   carve-out; everything else stays expression-shaped.
4. Run the collapse scan on every edit: 3+ parallel types, 3+ sibling factories, 3+ repeated
   switch arms, or 3+ single-call helpers triggers in-place polymorphic collapse.
5. Run `uv run python -m tools.assay static fix`, then `static build` on the touched closure;
   zero `': error '` lines before proceeding.
6. Author specs per the `testing-cs` skill against the page law: axis totality, receipt fields,
   fault codes, policy-table values; resolve the page RESEARCH rows gated on the file's clusters.
7. Host-seam files add `*.verify.csx` bridge scenarios grouped per file; G5 gates the row.

## [8]-[PROOF_GATES]

Assay rows use `uv run python -m tools.assay`; proof runs at the planned phase gate, not after each edit.

| [GATE] | [RAIL]                         | [EVIDENCE]                                                            |
| :----: | :----------------------------- | :-------------------------------------------------------------------- |
|  [G1]  | `dotnet restore --locked-mode` | clean closure; unchanged `packages.lock.json`                         |
|  [G2]  | `api doctor` + `api resolve`   | fence members resolve in `.api` or doctrine pages                     |
|  [G3]  | `static plan` + `static build` | routed closure, zero `': error '` lines                               |
|  [G4]  | `test run` AppHost target      | `testing-cs` law-matrix specs pass                                    |
|  [G5]  | `bridge verify` scenario       | host-seam scenarios pass under live RhinoWIP                          |
|  [G6]  | `mmdc` page render             | page diagrams render through local mermaid-cli                        |
|  [G7]  | G4 spec compile                | `Grpc.Core.Api` members compile until assay source-map coverage lands |

## [9]-[PROHIBITIONS]

- NEVER a public type outside the DENSITY_BAR owner regions; an eighth port record is the named
  defect.
- NEVER wrappers, rename adapters, helper or utility files, or thin forwarding surfaces over
  admitted packages.
- NEVER a generic receipt, ledger, or reported-value abstraction; every receipt stays its typed
  record.
- NEVER a second state machine, shutdown flag, or sibling phase enum beside `Lifecycle`.
- NEVER a free-floating `CancellationTokenSource` below the `CancelScope` spine.
- NEVER `DateTime.UtcNow`, `DateTime.Now`, or direct `Stopwatch` call sites; `ClockPolicy` owns
  both clocks, and sentinels project to `Option<T>` at the admission seam.
- NEVER a bare duration literal; every bound traces to a `DeadlineClass` row or a page policy
  table.
- NEVER a second scheduler: per-package timer loops, host idle hooks, and external job
  frameworks are the deleted patterns; `SchedulePort` owns every scheduled concern.
- NEVER a second cache owner: no hand-rolled double-checked caches, no cache-service wrappers;
  `CacheSurface` over `HybridCache` is the only entry.
- NEVER a second retry owner on one seam: no gRPC `ServiceConfig` retry, no stacked
  `Schedule`-plus-pipeline loops; database retry stays at the Persistence execution strategy.
- NEVER ambient `IConfiguration` reads past bootstrap or interior `IOptions` handles; interiors
  read frozen policy records published at ready.
- NEVER `AddSingleton`/`AddScoped`/`AddTransient`/`AddKeyed*` descriptor spellings or
  closure-walking scans; `Describe`/`DescribeKeyed` rows and `FromAssemblies` only.
- NEVER a process-static `Meter` or `ActivitySource` outliving its provider; never Serilog types
  below composition roots; never OTLP exporter pins below service app roots.
- NEVER a hand-written STJ converter beside the generated Thinktecture and NodaTime converters.
- NEVER an unredacted classified value at an exporter or bundle seam.
- NEVER posix traps or single-instance enforcement on plugin rows; host-attach injection drives
  phases there.
- CSP analyzer diagnostics are architecture pressure: fix the shape, refine the rule on a false
  positive, never suppress.

## [10]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and
admission status. Versions live in `Directory.Packages.props`; this table never carries a pin.
`[PAGE]` names the primary consuming page; `[CATALOGUE]` is the `.api` file; `[STATUS]` is one of
`catalogue-pending`, `app-root-pending`, `tests-only`, `admitted`.

| [INDEX] | [PACKAGE]                                               | [PAGE]                    | [CATALOGUE]                           | [STATUS]          |
| :-----: | :----------------------------------------------------- | :------------------------ | :------------------------------------ | :---------------- |
|   [1]   | Cronos                                                  | time-and-deadlines        | api-cronos.md                         | admitted          |
|   [2]   | FluentValidation                                        | configuration-and-options | api-validation.md                     | admitted          |
|   [3]   | FluentValidation.DependencyInjectionExtensions          | composition-and-modules   | api-validation-di.md                  | admitted          |
|   [4]   | Microsoft.Extensions.Caching.Hybrid                     | resource-lanes            | api-hybrid-cache.md                   | admitted          |
|   [5]   | Microsoft.Extensions.Compliance.Redaction               | diagnostics-and-telemetry | api-telemetry.md                      | admitted          |
|   [6]   | Microsoft.Extensions.Configuration                      | configuration-and-options | api-config.md                         | admitted          |
|   [7]   | Microsoft.Extensions.Configuration.Binder               | configuration-and-options | api-binder.md                         | admitted          |
|   [8]   | Microsoft.Extensions.Configuration.CommandLine          | configuration-and-options | api-config-providers.md               | admitted          |
|   [9]   | Microsoft.Extensions.Configuration.EnvironmentVariables | configuration-and-options | api-config-providers.md               | admitted          |
|  [10]   | Microsoft.Extensions.Configuration.Json                 | configuration-and-options | api-config-providers.md               | admitted          |
|  [11]   | Microsoft.Extensions.Configuration.UserSecrets          | configuration-and-options | api-config-providers.md               | admitted          |
|  [12]   | Microsoft.Extensions.DependencyInjection                | composition-and-modules   | api-di.md                             | admitted          |
|  [13]   | Microsoft.Extensions.Diagnostics.HealthChecks           | health-and-degradation    | api-health.md                         | admitted          |
|  [14]   | Microsoft.Extensions.Diagnostics.ResourceMonitoring     | health-and-degradation    | api-resource-monitoring.md            | admitted          |
|  [15]   | Microsoft.Extensions.Hosting                            | host-profiles             | api-hosting.md                        | admitted          |
|  [16]   | Microsoft.Extensions.Hosting.Systemd                    | host-profiles             | api-hosting-lifetimes.md              | admitted          |
|  [17]   | Microsoft.Extensions.Hosting.WindowsServices            | host-profiles             | api-hosting-lifetimes.md              | admitted          |
|  [18]   | Microsoft.Extensions.Http.Resilience                    | outbound-resilience       | api-resilience.md                     | admitted          |
|  [19]   | Microsoft.Extensions.Logging.Abstractions               | diagnostics-and-telemetry | api-logging.md                        | admitted          |
|  [20]   | Microsoft.Extensions.ObjectPool                         | resource-lanes            | api-objectpool.md                     | admitted          |
|  [21]   | Microsoft.Extensions.Options                            | configuration-and-options | api-options.md                        | admitted          |
|  [22]   | Microsoft.Extensions.Telemetry                          | diagnostics-and-telemetry | api-telemetry.md                      | admitted          |
|  [23]   | Microsoft.Extensions.Telemetry.Abstractions             | diagnostics-and-telemetry | api-telemetry-abstractions.md         | admitted          |
|  [24]   | NodaTime                                                | time-and-deadlines        | api-nodatime.md                       | admitted          |
|  [25]   | OpenTelemetry                                           | diagnostics-and-telemetry | api-otel.md                           | admitted          |
|  [26]   | OpenTelemetry.Extensions.Hosting                        | diagnostics-and-telemetry | api-otel-hosting.md                   | admitted          |
|  [27]   | OpenTelemetry.Instrumentation.Http                      | diagnostics-and-telemetry | api-otel-instrumentation.md           | admitted          |
|  [28]   | OpenTelemetry.Instrumentation.Runtime                   | diagnostics-and-telemetry | api-otel-instrumentation.md           | admitted          |
|  [29]   | Polly.Core                                              | outbound-resilience       | api-polly-core.md                     | admitted          |
|  [30]   | Polly.Extensions                                        | outbound-resilience       | api-polly-extensions.md               | admitted          |
|  [31]   | Polly.RateLimiting                                      | outbound-resilience       | api-polly-ratelimiting.md             | admitted          |
|  [32]   | Scrutor                                                 | composition-and-modules   | api-scrutor.md                        | admitted          |
|  [33]   | Serilog                                                 | diagnostics-and-telemetry | api-serilog.md                        | admitted          |
|  [34]   | System.Threading.Tasks.Dataflow                         | resource-lanes            | api-dataflow.md                       | admitted          |
|  [35]   | Thinktecture.Runtime.Extensions.Json                    | runtime-ports             | api-thinktecture-json.md              | admitted          |
|  [36]   | LanguageExt.Core                                        | every page                | stack doctrine (`docs/stacks/csharp`) | admitted          |
|  [37]   | Thinktecture.Runtime.Extensions                         | every page                | stack doctrine (`docs/stacks/csharp`) | admitted          |
|  [38]   | Velopack                                                | provisioning-and-update   | api-velopack.md                       | admitted          |
|  [39]   | Grpc.Net.Client                                         | outbound-resilience       | —                                     | catalogue-pending |
|  [40]   | Grpc.AspNetCore                                          | companion-sidecar         | —                                     | app-root-pending  |
|  [41]   | Grpc.AspNetCore.HealthChecks                             | companion-sidecar         | —                                     | app-root-pending  |
|  [42]   | NodaTime.Serialization.SystemTextJson                   | runtime-ports             | —                                     | catalogue-pending |
|  [43]   | Microsoft.Extensions.TimeProvider.Testing               | time-and-deadlines        | api-testing-seams.md                  | tests-only        |
|  [44]   | NodaTime.Testing                                        | time-and-deadlines        | api-testing-seams.md                  | tests-only        |
|  [45]   | Microsoft.Extensions.Diagnostics.Testing                | diagnostics-and-telemetry | api-testing-seams.md                  | tests-only        |

## [11]-[REFINEMENT_HORIZON]

Folder-specific deepening targets beyond the closed corpus, each carrying its open probe. The Velopack `VelopackHook` delegate-signature probe resolves the exact parameter shape of the `OnFirstRun`/`OnRestarted`/`OnAfter*FastCallback` registrations at the `VelopackApp.Build()...Run()` app-root bootstrap (the only residual after provisioning-and-update lands). The `ServiceMappingCollection.Map` versus `MapService` behavioral distinction on `GrpcHealthChecksOptions.Services` settles the by-service-name versus predicate-routing wire-health registration row at companion-sidecar#CONTROL_SERVICE. The cross-process degradation-cascade convergence bridge scenario — a companion observes the parent level over the control hop, lands it as a `DegradationCell.Cascade` floor, and re-derives on release — runs against the paired and companion topologies inside the running integrated host (companion-sidecar#DEGRADATION_CASCADE `[CASCADE_CONVERGENCE]`, the legitimate tier-3 live-host residual). The keychain secrets-store route (configuration-and-options#SOURCE_ROUTES) and SIGHUP delivery (lifecycle-and-drain#FAULT_PROBES) resolved from their probes into settled rows. The bar: any host modality boots, updates, degrades, drains, serves its control plane, and reports through this spine with zero app-side ceremony.

Testing-infrastructure horizon: the deterministic-clock seam pairs `FakeClock` (`NodaTime.Testing`) with `FakeTimeProvider` over the dual clock authority, and the telemetry-seam assertions ride `MetricCollector<T>`/`FakeLogCollector` against the diagnostics signal-governance fold.
