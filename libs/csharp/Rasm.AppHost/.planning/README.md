# [APPHOST_PLANNING]

Rasm.AppHost has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — they are never re-designed downstream. The runtime spine owns host variance, lifecycle, time, configuration, composition, resource lanes, telemetry, health, support capture, outbound resilience, and the cross-package port surface; siblings adapt to its ports and never reverse the dependency.

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
|  [10]   | [outbound-resilience](outbound-resilience.md)             | hop axis and single retry owner                  | finalized |
|  [11]   | [runtime-ports](runtime-ports.md)                         | port records, suite wire law, TS map             | finalized |

## [2]-[WIRE_PAGES]

lifecycle-and-drain · health-and-degradation · support-bundles · runtime-ports (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

App-root-only pins with no restored closure, catalogued at app-root creation: OpenTelemetry.Exporter.OpenTelemetryProtocol, System.CommandLine, Grpc.AspNetCore, Grpc.AspNetCore.Web, Grpc.AspNetCore.HealthChecks, Microsoft.AspNetCore.JsonPatch.SystemTextJson, Serilog.Extensions.Hosting (AddSerilog app-root bridge).

## [4]-[GAP_LEDGER]

The red-team pass requires every row `CLOSED`; `[CLOSED_BY]` names the page#cluster that absorbed the gap.

| [INDEX] | [GAP]                                            | [CLOSED_BY (page#cluster)]                                                                                         | [STATE] |
| :-----: | :----------------------------------------------- | :----------------------------------------------------------------------------------------------------------------- | :-----: |
|   [1]   | sibling telemetry registration seam              | runtime-ports#PORT_RECORDS + diagnostics-and-telemetry#TELEMETRY_IDENTITY                                          | CLOSED  |
|   [2]   | cross-package drain rank bands                   | lifecycle-and-drain#DRAIN_CONDUCTOR                                                                                | CLOSED  |
|   [3]   | shared data-classification taxonomy              | diagnostics-and-telemetry#REDACTION_TAXONOMY                                                                       | CLOSED  |
|   [4]   | HLC receipt envelope as causal primitive         | runtime-ports#PORT_RECORDS                                                                                         | CLOSED  |
|   [5]   | `grpc.health.v1` registry projection             | health-and-degradation#WIRE_HEALTH                                                                                 | CLOSED  |
|   [6]   | `DrainQueue` / `WorkLane` name split             | resource-lanes#DRAIN_QUEUES                                                                                        | CLOSED  |
|   [7]   | `HybridCache` port, stampede, tags, and L2 seam  | resource-lanes#CACHE_PORT                                                                                          | CLOSED  |
|   [8]   | options reload-class column                      | configuration-and-options#POLICY_VALUES                                                                            | CLOSED  |
|   [9]   | resource-pressure health contributor             | health-and-degradation#HEALTH_FOLD                                                                                 | CLOSED  |
|  [10]   | user-secrets versus OS-keychain config rows      | configuration-and-options#SOURCE_AXIS                                                                              | CLOSED  |
|  [11]   | update-check outbound hop                        | outbound-resilience#HOP_AXIS                                                                                       | CLOSED  |
|  [12]   | unhandled, task, POSIX, and crash-marker faults  | lifecycle-and-drain#FAULT_SPINE                                                                                    | CLOSED  |
|  [13]   | discovery manifest and local IPC attach          | outbound-resilience#DISCOVERY_ATTACH + runtime-ports#PORT_RECORDS                                                  | CLOSED  |
|  [14]   | `LocalOnly` degradation level                    | health-and-degradation#DEGRADATION_RAIL                                                                            | CLOSED  |
|  [15]   | operator kill-switch and degradation consequence | configuration-and-options#KILL_SWITCH + health-and-degradation#DEGRADATION_RAIL                                    | CLOSED  |
|  [16]   | cron schedule port and lease/crash split         | time-and-deadlines#SCHEDULE_PORT                                                                                   | CLOSED  |
|  [17]   | merged JSON contracts and schema emission        | runtime-ports#WIRE_LAW                                                                                             | CLOSED  |
|  [18]   | crash probe, support trigger, and host markers   | lifecycle-and-drain#FAULT_SPINE + support-bundles#TRIGGER_UNION                                                    | CLOSED  |
|  [19]   | service-mode control verbs                       | support-bundles#CAPTURE_PIPELINE + health-and-degradation#DEGRADATION_RAIL + configuration-and-options#KILL_SWITCH | CLOSED  |
|  [20]   | GC posture and DATAS benchmark gate              | host-profiles#PROFILE_AXIS                                                                                         | CLOSED  |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of the naive LOC a per-concern implementation spends. The owner
budget below is the public-surface cap: one owner per axis, one entrypoint family per rail; a
new feature is a row or case, never a new surface. Key policies, receipts, policy records,
runtime records, and wire shapes ride inside their owner's signature region (ledger rows AH-01
through AH-11 and AH-04a-d); a public type outside those regions is the named defect.

| [INDEX] | [AXIS/CONCERN]          | [OWNER]                          | [KIND]                  | [CASES]                                                     |
| :-----: | :---------------------- | :------------------------------- | :---------------------- | :---------------------------------------------------------- |
|   [1]   | host variance           | `HostProfile`                    | `[SmartEnum<string>]`   | 8 rows                                                      |
|   [2]   | lifetime adapters       | `ProfileBoot`                    | static fold             | 6 delegate targets                                          |
|   [3]   | resource identity       | `ProfileIdentity`                | static fold             | 5 attribute rows                                            |
|   [4]   | phase family            | `RuntimePhase`                   | `[SmartEnum<string>]`   | 8 rows                                                      |
|   [5]   | trigger vocabulary      | `PhaseTrigger`                   | `[Union]`               | 10 cases                                                    |
|   [6]   | transition cell         | `Lifecycle`                      | boundary capsule        | 1 CAS entry                                                 |
|   [7]   | fault spine             | `FaultSource`                    | `[Union]`               | 4 cases                                                     |
|   [8]   | drain bands             | `DrainBand`                      | `[SmartEnum<int>]`      | 4 rows                                                      |
|   [9]   | cancellation spine      | `CancelScope`                    | record capsule          | 1 root                                                      |
|  [10]   | clock seam              | `ClockPolicy`                    | record                  | 2 admissions                                                |
|  [11]   | deadline taxonomy       | `DeadlineClass`                  | `[SmartEnum<string>]`   | 9 rows                                                      |
|  [12]   | schedule port           | `ScheduleEntry`                  | record port             | 2 occurrence cases                                          |
|  [13]   | config sources          | `ConfigSource`                   | `[SmartEnum<string>]`   | 8 rows                                                      |
|  [14]   | binding faults          | `ConfigError`                    | `[Union]` fault         | 7 cases                                                     |
|  [15]   | reload rail             | `ReloadOutcome`                  | `[Union]`               | 4 cases                                                     |
|  [16]   | kill switch             | `OperatorOverride`               | `[Union]`               | 2 cases                                                     |
|  [17]   | module table            | `ModuleContribution`             | record row              | 1 row per package; `DecorationRow` carrier column           |
|  [18]   | composition fold        | `CompositionSurface`             | static fold             | 1 receipted entry                                           |
|  [19]   | boundary activation     | `BoundaryActivation`             | static surface          | 1 entry family                                              |
|  [20]   | cache lanes             | `CacheLane`                      | `[SmartEnum<string>]`   | 3 rows                                                      |
|  [21]   | object pools            | `PoolPolicy<T>`                  | policy row              | 1 row per type; `Pools` registration sub-surface            |
|  [22]   | drain queues            | `DrainQueue<T>`                  | `[Union]`               | 2 cases                                                     |
|  [23]   | telemetry identity      | `TelemetrySource`                | `[SmartEnum<string>]`   | 6 rows                                                      |
|  [24]   | correlation spine       | `Correlation`                    | static surface          | 1 boot mint                                                 |
|  [25]   | log arbitration         | `LogPipeline`                    | `[SmartEnum<string>]`   | 2 rows; `SpineLossFold` listener sub-surface                |
|  [26]   | signal governance       | `TelemetrySignal`                | `[SmartEnum<string>]`   | 3 rows; `LatencyCheckpoint`/`LatencySpine` carrier          |
|  [27]   | classification taxonomy | `DataClassification`             | `[SmartEnum<string>]`   | 7 rows; `RedactorKind` column, `RedactionRegistration` fold |
|  [28]   | health fold             | `HealthContributorRow`           | record row + probe      | 4 tag families                                              |
|  [29]   | capability vocabulary   | `Capability`                     | `[SmartEnum<string>]`   | 6 rows                                                      |
|  [30]   | degradation rail        | `DegradationLevel`               | `[SmartEnum<string>]`   | 5 rows                                                      |
|  [31]   | wire health             | `WireHealthRow`                  | record row              | 1 row per service                                           |
|  [32]   | support triggers        | `SupportTrigger`                 | `[Union]`               | 6 cases                                                     |
|  [33]   | support receipts        | `SupportReceipt`                 | `[Union]`               | 3 cases                                                     |
|  [34]   | hop axis                | `OutboundHop`                    | `[Union]`               | 7 cases                                                     |
|  [35]   | hop faults              | `HopFault`                       | `[Union]` fault         | 7 cases                                                     |
|  [36]   | hop outcomes            | `HopOutcome`                     | `[Union]`               | 3 cases                                                     |
|  [37]   | discovery attach        | `DiscoveryManifest`              | record + static surface | 1 manifest law                                              |
|  [38]   | runtime ports           | `ReceiptSinkPort` + six siblings | sealed records          | 7 ports                                                     |
|  [39]   | wire law                | `AppHostWireContext`             | JsonSerializerContext   | 9 contract rows; `NodaPatterns` pattern sub-surface         |

## [6]-[BUILD_ORDER]

Vocabulary owners land before their consumers; shapes before rails, rails before dispatch,
boundaries before composition. `Ports.cs` lands last because `AppHostWireContext` rows reference
receipts every earlier file declares. `CorrelationId` is owned at
lifecycle-and-drain#PHASE_FAMILY; `Diagnostics.cs` consumes it and never re-declares.
Drain band literals live only on `DrainBand`; sibling registrations arrive as
`DrainParticipantPort` rows and never copy them. `DrainQueue` is the AppHost name;
`WorkLane` stays at Compute. Comparer accessors stay package-local, one per axis owner.

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

The executed admissions ledger — the only planning location where versions are written. PAGE
names the primary consuming page; CATALOGUE is the `.api` file.

| [PACKAGE]                                               | [VERSION] | [PAGE]                    | [CATALOGUE]                   |
| :------------------------------------------------------ | :-------- | :------------------------ | :---------------------------- |
| Cronos                                                  | 0.13.0    | time-and-deadlines        | api-cronos.md                 |
| FluentValidation                                        | 12.1.1    | configuration-and-options | api-validation.md             |
| FluentValidation.DependencyInjectionExtensions          | 12.1.1    | composition-and-modules   | api-validation-di.md          |
| Microsoft.Extensions.Caching.Hybrid                     | 10.7.0    | resource-lanes            | api-hybrid-cache.md           |
| Microsoft.Extensions.Compliance.Redaction               | 10.7.0    | diagnostics-and-telemetry | api-telemetry.md              |
| Microsoft.Extensions.Configuration                      | 10.0.9    | configuration-and-options | api-config.md                 |
| Microsoft.Extensions.Configuration.Binder               | 10.0.9    | configuration-and-options | api-binder.md                 |
| Microsoft.Extensions.Configuration.CommandLine          | 10.0.9    | configuration-and-options | api-config-providers.md       |
| Microsoft.Extensions.Configuration.EnvironmentVariables | 10.0.9    | configuration-and-options | api-config-providers.md       |
| Microsoft.Extensions.Configuration.Json                 | 10.0.9    | configuration-and-options | api-config-providers.md       |
| Microsoft.Extensions.Configuration.UserSecrets          | 10.0.9    | configuration-and-options | api-config-providers.md       |
| Microsoft.Extensions.DependencyInjection                | 10.0.9    | composition-and-modules   | api-di.md                     |
| Microsoft.Extensions.Diagnostics.HealthChecks           | 10.0.9    | health-and-degradation    | api-health.md                 |
| Microsoft.Extensions.Diagnostics.ResourceMonitoring     | 10.7.0    | health-and-degradation    | api-resource-monitoring.md    |
| Microsoft.Extensions.Hosting                            | 10.0.9    | host-profiles             | api-hosting.md                |
| Microsoft.Extensions.Hosting.Systemd                    | 10.0.9    | host-profiles             | api-hosting-lifetimes.md      |
| Microsoft.Extensions.Hosting.WindowsServices            | 10.0.9    | host-profiles             | api-hosting-lifetimes.md      |
| Microsoft.Extensions.Http.Resilience                    | 10.7.0    | outbound-resilience       | api-resilience.md             |
| Microsoft.Extensions.Logging.Abstractions               | 10.0.9    | diagnostics-and-telemetry | api-logging.md                |
| Microsoft.Extensions.ObjectPool                         | 10.0.9    | resource-lanes            | api-objectpool.md             |
| Microsoft.Extensions.Options                            | 10.0.9    | configuration-and-options | api-options.md                |
| Microsoft.Extensions.Telemetry                          | 10.7.0    | diagnostics-and-telemetry | api-telemetry.md              |
| Microsoft.Extensions.Telemetry.Abstractions             | 10.7.0    | diagnostics-and-telemetry | api-telemetry-abstractions.md |
| NodaTime                                                | 3.3.2     | time-and-deadlines        | api-nodatime.md               |
| OpenTelemetry                                           | 1.16.0    | diagnostics-and-telemetry | api-otel.md                   |
| OpenTelemetry.Extensions.Hosting                        | 1.16.0    | diagnostics-and-telemetry | api-otel-hosting.md           |
| OpenTelemetry.Instrumentation.Http                      | 1.15.1    | diagnostics-and-telemetry | api-otel-instrumentation.md   |
| OpenTelemetry.Instrumentation.Runtime                   | 1.15.1    | diagnostics-and-telemetry | api-otel-instrumentation.md   |
| Polly.Core                                              | 8.7.0     | outbound-resilience       | api-polly-core.md             |
| Polly.Extensions                                        | 8.7.0     | outbound-resilience       | api-polly-extensions.md       |
| Polly.RateLimiting                                      | 8.7.0     | outbound-resilience       | api-polly-ratelimiting.md     |
| Scrutor                                                 | 7.0.0     | composition-and-modules   | api-scrutor.md                |
| Serilog                                                 | 4.3.1     | diagnostics-and-telemetry | api-serilog.md                |
| System.Threading.Tasks.Dataflow                         | 10.0.9    | resource-lanes            | api-dataflow.md               |
| Thinktecture.Runtime.Extensions.Json                    | 10.2.0    | runtime-ports             | api-thinktecture-json.md      |

Substrate, pending, and test-only admissions:

| [PACKAGE]                                 | [VERSION]     | [PAGE]                    | [CATALOGUE]                                            |
| :---------------------------------------- | :------------ | :------------------------ | :----------------------------------------------------- |
| LanguageExt.Core                          | 5.0.0-beta-77 | every page                | stack doctrine (`docs/stacks/csharp`)                  |
| Thinktecture.Runtime.Extensions           | 10.2.0        | every page                | stack doctrine (`docs/stacks/csharp`)                  |
| Grpc.Net.Client                           | 2.80.0        | outbound-resilience       | catalogue pending; csproj row lands with `Outbound.cs` |
| NodaTime.Serialization.SystemTextJson     | 1.4.0         | runtime-ports             | catalogue pending; csproj row lands with `Ports.cs`    |
| Microsoft.Extensions.TimeProvider.Testing | 10.7.0        | time-and-deadlines        | api-testing-seams.md (tests-only)                      |
| NodaTime.Testing                          | 3.3.2         | time-and-deadlines        | api-testing-seams.md (tests-only)                      |
| Microsoft.Extensions.Diagnostics.Testing  | 10.7.0        | diagnostics-and-telemetry | api-testing-seams.md (tests-only)                      |

## [11]-[REFINEMENT_HORIZON]

Folder-specific deepening targets beyond the closed corpus: the operational control surface (ControlService consequence rows) exercised against every service modality; watchdog/heartbeat receipts composed with the support pipeline end-to-end; the discovery/attach choreography rehearsed against the paired and companion topologies in `../../.planning/FEATURES.md`; keychain secrets-store and SIGHUP routes resolved from their probes into settled rows. The bar: any host modality boots, degrades, drains, and reports through this spine with zero app-side ceremony.
