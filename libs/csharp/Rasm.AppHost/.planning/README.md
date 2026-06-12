# [APPHOST_PLANNING]

Rasm.AppHost has zero consumers; the implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — they are never re-designed downstream. The runtime spine owns host variance, lifecycle, time, configuration, composition, resource lanes, telemetry, health, support capture, outbound resilience, and the cross-package port surface; siblings adapt to its ports and never reverse the dependency.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE] | [OWNS] | [STATE] |
| :-----: | ------ | ------ | :-----: |
| [1] | [host-profiles](host-profiles.md) | The single host-variance axis; profile rows resolve all modality variance | finalized |
| [2] | [lifecycle-and-drain](lifecycle-and-drain.md) | Phase family, transition law, drain conductor, cancellation spine, crash markers | finalized |
| [3] | [time-and-deadlines](time-and-deadlines.md) | Clock seams, deadline taxonomy, schedule port, deterministic test clocks | finalized |
| [4] | [configuration-and-options](configuration-and-options.md) | Source axis, layering, typed binding, validated frozen policy, receipted reload | finalized |
| [5] | [composition-and-modules](composition-and-modules.md) | One composition root per process; module contribution rows | finalized |
| [6] | [resource-lanes](resource-lanes.md) | Hybrid cache lane, object pools, drainable queues | finalized |
| [7] | [diagnostics-and-telemetry](diagnostics-and-telemetry.md) | Correlation spine, signal governance, classification taxonomy | finalized |
| [8] | [health-and-degradation](health-and-degradation.md) | Capability health fold and the usable-failure degradation rail | finalized |
| [9] | [support-bundles](support-bundles.md) | Bounded, correlated, redacted diagnostic capture | finalized |
| [10] | [outbound-resilience](outbound-resilience.md) | Hop axis; exactly one retry owner per remote boundary | finalized |
| [11] | [runtime-ports](runtime-ports.md) | Typed port records, suite wire law, TS tooling map | finalized |

## [2]-[WIRE_PAGES]

lifecycle-and-drain · health-and-degradation · support-bundles · runtime-ports (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

App-root-only pins with no restored closure, catalogued at app-root creation: OpenTelemetry.Exporter.OpenTelemetryProtocol, System.CommandLine, Grpc.AspNetCore, Grpc.AspNetCore.Web, Grpc.AspNetCore.HealthChecks, Microsoft.AspNetCore.JsonPatch.SystemTextJson, Serilog.Extensions.Hosting (AddSerilog app-root bridge).

## [4]-[GAP_LEDGER]

| [INDEX] | [GAP] | [CLOSED_BY] | [STATE] |
| :-----: | ----- | ----------- | :-----: |
| [1] | TelemetryContributorPort seam for sibling ActivitySource/Meter registration | runtime-ports + diagnostics-and-telemetry | CLOSED |
| [2] | Cross-package drain rank-band table with store-dependency column; inbound cessation first | lifecycle-and-drain | CLOSED |
| [3] | DataClassification taxonomy ownership here (Persistence consumes) | diagnostics-and-telemetry | CLOSED |
| [4] | ReceiptSinkPort with HLC envelope as the only cross-process causal primitive | runtime-ports | CLOSED |
| [5] | grpc.health.v1 mapping from the HealthChecks registry via tag predicate | health-and-degradation | CLOSED |
| [6] | Lane homonym resolved: DrainQueue here, WorkLane at Compute | resource-lanes | CLOSED |
| [7] | HybridCache ownership here (port, stampede, tags, entry options); L2 contributed by Persistence | resource-lanes | CLOSED |
| [8] | Options rows carry a reload-class column (frozen, transition) | configuration-and-options | CLOSED |
| [9] | Resource-pressure HealthContributor row consuming ResourceMonitoring snapshots | health-and-degradation | CLOSED |
| [10] | ConfigSource gains the user-secrets row distinct from the OS-keychain secrets-store row | configuration-and-options | CLOSED |
| [11] | OutboundHop gains the update-check case, structurally excluded on plugin rows | outbound-resilience | CLOSED |
| [12] | FaultSource union: unhandled, unobserved-task, posix-signal, host-crash-marker | lifecycle-and-drain | CLOSED |
| [13] | Discovery manifest + LocalIpc hop + single retry owner for the standalone attach | outbound-resilience + runtime-ports | CLOSED |
| [14] | DegradationLevel.LocalOnly designed level for Rhino-absent folding | health-and-degradation | CLOSED |
| [15] | Operator kill-switch as a config row + ControlService set-degradation verb consequence | configuration-and-options + health-and-degradation | CLOSED |
| [16] | Schedule-port record carries CronExpression; lease-handoff distinct from crash-reclaim | time-and-deadlines | CLOSED |
| [17] | JsonTypeInfoResolver.Combine suite contract merge + JsonSchemaExporter emission path | runtime-ports | CLOSED |
| [18] | Crash-recovery choreography: boot probe maps to the existing fault-transition SupportTrigger; plugin rows read host .rhl/.ips markers | lifecycle-and-drain + support-bundles | CLOSED |
| [19] | Service-modality inbound verbs land as ControlService consequence rows | support-bundles + health-and-degradation + configuration-and-options | CLOSED |
| [20] | GC posture per HostProfile row; DATAS knobs claim-gated behind a losing benchmark | host-profiles | CLOSED |

## [5]-[DENSITY_BAR]

Implementation lands at 25-35% of the naive LOC a per-concern implementation spends. The owner
budget below is the public-surface cap: one owner per axis, one entrypoint family per rail; a
new feature is a row or case, never a new surface. Key policies, receipts, policy records,
runtime records, and wire shapes ride inside their owner's signature region (ledger rows AH-01
through AH-11 and AH-04a-d); a public type outside those regions is the named defect.

| [INDEX] | [AXIS/CONCERN]          | [OWNER]              | [KIND]                  | [CASES]            |
| :-----: | :---------------------- | :------------------- | :---------------------- | :----------------- |
|   [1]   | host variance           | `HostProfile`        | `[SmartEnum<string>]`   | 8 rows             |
|   [2]   | lifetime adapters       | `ProfileBoot`        | static fold             | 5 delegate targets |
|   [3]   | resource identity       | `ProfileIdentity`    | static fold             | 5 attribute rows   |
|   [4]   | phase family            | `RuntimePhase`       | `[SmartEnum<string>]`   | 8 rows             |
|   [5]   | trigger vocabulary      | `PhaseTrigger`       | `[Union]`               | 10 cases           |
|   [6]   | transition cell         | `Lifecycle`          | boundary capsule        | 1 CAS entry        |
|   [7]   | fault spine             | `FaultSource`        | `[Union]`               | 4 cases            |
|   [8]   | drain bands             | `DrainBand`          | `[SmartEnum<int>]`      | 4 rows             |
|   [9]   | cancellation spine      | `CancelScope`        | record capsule          | 1 root             |
|  [10]   | clock seam              | `ClockPolicy`        | record                  | 2 admissions       |
|  [11]   | deadline taxonomy       | `DeadlineClass`      | `[SmartEnum<string>]`   | 9 rows             |
|  [12]   | schedule port           | `ScheduleEntry`      | record port             | 2 occurrence cases |
|  [13]   | config sources          | `ConfigSource`       | `[SmartEnum<string>]`   | 8 rows             |
|  [14]   | binding faults          | `ConfigError`        | `[Union]` fault         | 7 cases            |
|  [15]   | reload rail             | `ReloadOutcome`      | `[Union]`               | 4 cases            |
|  [16]   | kill switch             | `OperatorOverride`   | `[Union]`               | 2 cases            |
|  [17]   | module table            | `ModuleContribution` | record row              | 1 row per package  |
|  [18]   | composition fold        | `CompositionSurface` | static fold             | 1 receipted entry  |
|  [19]   | boundary activation     | `BoundaryActivation` | static surface          | 1 entry family     |
|  [20]   | cache lanes             | `CacheLane`          | `[SmartEnum<string>]`   | 3 rows             |
|  [21]   | object pools            | `PoolPolicy<T>`      | policy row              | 1 row per type     |
|  [22]   | drain queues            | `DrainQueue<T>`      | `[Union]`               | 2 cases            |
|  [23]   | telemetry identity      | `TelemetrySource`    | `[SmartEnum<string>]`   | 6 rows             |
|  [24]   | correlation spine       | `Correlation`        | static surface          | 1 boot mint        |
|  [25]   | log arbitration         | `LogPipeline`        | `[SmartEnum<string>]`   | 2 rows             |
|  [26]   | signal governance       | `TelemetrySignal`    | `[SmartEnum<string>]`   | 3 rows             |
|  [27]   | classification taxonomy | `DataClassification` | `[SmartEnum<string>]`   | 7 rows             |
|  [28]   | health fold             | `HealthContributorRow` | record row + probe    | 4 tag families     |
|  [29]   | capability vocabulary   | `Capability`         | `[SmartEnum<string>]`   | 6 rows             |
|  [30]   | degradation rail        | `DegradationLevel`   | `[SmartEnum<string>]`   | 5 rows             |
|  [31]   | wire health             | `WireHealthRow`      | record row              | 1 row per service  |
|  [32]   | support triggers        | `SupportTrigger`     | `[Union]`               | 6 cases            |
|  [33]   | support receipts        | `SupportReceipt`     | `[Union]`               | 3 cases            |
|  [34]   | hop axis                | `OutboundHop`        | `[Union]`               | 7 cases            |
|  [35]   | hop faults              | `HopFault`           | `[Union]` fault         | 7 cases            |
|  [36]   | hop outcomes            | `HopOutcome`         | `[Union]`               | 3 cases            |
|  [37]   | discovery attach        | `DiscoveryManifest`  | record + static surface | 1 manifest law     |
|  [38]   | runtime ports           | `ReceiptSinkPort` + six siblings | sealed records | 7 ports          |
|  [39]   | wire law                | `AppHostWireContext` | JsonSerializerContext   | 9 contract rows    |

## [6]-[BUILD_ORDER]

Vocabulary owners land before their consumers; shapes before rails, rails before dispatch,
boundaries before composition. `Ports.cs` lands last because `AppHostWireContext` rows reference
receipts every earlier file declares — the AppHost instance of the ledger's
vocabulary-owners-first law (the fingerprint-slot seam note). `CorrelationId` is the relocated
owner at lifecycle-and-drain#PHASE_FAMILY; `Diagnostics.cs` consumes it and never re-declares.
Drain band literals live only on `DrainBand`; sibling registrations arrive as
`DrainParticipantPort` rows and never copy them. `DrainQueue` is the AppHost name;
`WorkLane` stays at Compute. Comparer accessors stay package-local, one per axis owner.

| [INDEX] | [FILE]             | [TRANSCRIBES]                                                                                   | [GATE]                                          |
| :-----: | :----------------- | :----------------------------------------------------------------------------------------------- | :----------------------------------------------- |
|   [1]   | `Time.cs`          | time-and-deadlines#CLOCK_SPLIT + DEADLINE_TAXONOMY + SCHEDULE_PORT                                | G3 + G4 fake-clock deadline and occurrence specs |
|   [2]   | `Profiles.cs`      | host-profiles#PROFILE_AXIS + LIFETIME_ADAPTERS + RESOURCE_IDENTITY                                | G3 + G4 Resolve/Roots matrix; G5 `host_boot_drain` |
|   [3]   | `Lifecycle.cs`     | lifecycle-and-drain#PHASE_FAMILY + FAULT_SPINE + DRAIN_CONDUCTOR + CANCEL_SPINE                   | G3 + G4 total transition matrix, marker round-trip |
|   [4]   | `Configuration.cs` | configuration-and-options#SOURCE_AXIS + TYPED_BINDING + POLICY_VALUES + KILL_SWITCH               | G3 + G4 rank fold, fail-closed bind, reload gating |
|   [5]   | `Composition.cs`   | composition-and-modules#MODULE_TABLE + SCAN_AND_DECORATE + BOUNDARY_ACTIVATION                    | G3 + G4 compose under `ValidateOnBuild`          |
|   [6]   | `ResourceLanes.cs` | resource-lanes#CACHE_PORT + OBJECT_POOLS + DRAIN_QUEUES                                           | G3 + G4 tag-cut, reset, unreceipted-loss specs   |
|   [7]   | `Diagnostics.cs`   | diagnostics-and-telemetry#TELEMETRY_IDENTITY + CORRELATION_SPINE + LOG_PROJECTION + SIGNAL_GOVERNANCE + REDACTION_TAXONOMY | G3 + G4 `FakeLogCollector` + `MetricCollector<T>` |
|   [8]   | `Health.cs`        | health-and-degradation#HEALTH_FOLD + DEGRADATION_RAIL + WIRE_HEALTH                               | G3 + G4 hysteresis and force-beats-derived specs |
|   [9]   | `Support.cs`       | support-bundles#TRIGGER_UNION + CAPTURE_PIPELINE + MANIFEST_RECEIPT                               | G3 + G4 coalesce, cap, eviction specs            |
|  [10]   | `Outbound.cs`      | outbound-resilience#HOP_AXIS + HTTP_PIPELINES + KEYED_PIPELINES + DISCOVERY_ATTACH + OWNERSHIP_LAW | G3 + G4 admission and owner-conflict specs; G5 UDS attach |
|  [11]   | `Ports.cs`         | runtime-ports#PORT_RECORDS + WIRE_LAW                                                             | G3 + G4 HLC advance and wire round-trip specs    |

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

| [GATE] | [COMMAND]                                                          | [EVIDENCE]                                                       |
| :----: | :------------------------------------------------------------------ | :----------------------------------------------------------------- |
|  [G1]  | `dotnet restore --locked-mode`                                      | closure restores clean; `packages.lock.json` unchanged             |
|  [G2]  | `uv run python -m tools.assay api doctor` + `api resolve <key>`     | every fence member resolves in `.reports/api` or doctrine pages    |
|  [G3]  | `uv run python -m tools.assay static plan` + `static build`         | routing proven; zero `': error '` lines on the touched closure     |
|  [G4]  | `uv run python -m tools.assay test run --target Rasm.AppHost.Tests` | law-matrix specs green per `testing-cs`                            |
|  [G5]  | `uv run python -m tools.assay bridge verify --pattern <scenario>`   | host-seam scenarios pass under live RhinoWIP                       |
|  [G6]  | `pnpm exec mmdc -i <page> -o /tmp/<page>.svg`                       | every page diagram renders through the local mermaid-cli route     |
|  [G7]  | spec compile on the G4 rail                                         | `Grpc.Core.Api` transitive members prove by compiling specs until the assay source map registers the key |

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
names the primary consuming page; CATALOGUE is the `.reports/api` file.

| [PACKAGE]                                        | [VERSION] | [PAGE]                     | [CATALOGUE]                  |
| :----------------------------------------------- | :-------- | :------------------------- | :--------------------------- |
| Cronos                                           | 0.13.0    | time-and-deadlines         | api-cronos.md                |
| FluentValidation                                 | 12.1.1    | configuration-and-options  | api-validation.md            |
| FluentValidation.DependencyInjectionExtensions   | 12.1.1    | composition-and-modules    | api-validation-di.md         |
| Microsoft.Extensions.Caching.Hybrid              | 10.7.0    | resource-lanes             | api-hybrid-cache.md          |
| Microsoft.Extensions.Compliance.Redaction        | 10.7.0    | diagnostics-and-telemetry  | api-telemetry.md             |
| Microsoft.Extensions.Configuration               | 10.0.9    | configuration-and-options  | api-config.md                |
| Microsoft.Extensions.Configuration.Binder        | 10.0.9    | configuration-and-options  | api-binder.md                |
| Microsoft.Extensions.Configuration.CommandLine   | 10.0.9    | configuration-and-options  | api-config-providers.md      |
| Microsoft.Extensions.Configuration.EnvironmentVariables | 10.0.9 | configuration-and-options | api-config-providers.md      |
| Microsoft.Extensions.Configuration.Json          | 10.0.9    | configuration-and-options  | api-config-providers.md      |
| Microsoft.Extensions.Configuration.UserSecrets   | 10.0.9    | configuration-and-options  | api-config-providers.md      |
| Microsoft.Extensions.DependencyInjection         | 10.0.9    | composition-and-modules    | api-di.md                    |
| Microsoft.Extensions.Diagnostics.HealthChecks    | 10.0.9    | health-and-degradation     | api-health.md                |
| Microsoft.Extensions.Diagnostics.ResourceMonitoring | 10.7.0 | health-and-degradation     | api-resource-monitoring.md   |
| Microsoft.Extensions.Hosting                     | 10.0.9    | host-profiles              | api-hosting.md               |
| Microsoft.Extensions.Hosting.Systemd             | 10.0.9    | host-profiles              | api-hosting-lifetimes.md     |
| Microsoft.Extensions.Hosting.WindowsServices     | 10.0.9    | host-profiles              | api-hosting-lifetimes.md     |
| Microsoft.Extensions.Http.Resilience             | 10.7.0    | outbound-resilience        | api-resilience.md            |
| Microsoft.Extensions.Logging.Abstractions        | 10.0.9    | diagnostics-and-telemetry  | api-logging.md               |
| Microsoft.Extensions.ObjectPool                  | 10.0.9    | resource-lanes             | api-objectpool.md            |
| Microsoft.Extensions.Options                     | 10.0.9    | configuration-and-options  | api-options.md               |
| Microsoft.Extensions.Telemetry                   | 10.7.0    | diagnostics-and-telemetry  | api-telemetry.md             |
| Microsoft.Extensions.Telemetry.Abstractions      | 10.7.0    | diagnostics-and-telemetry  | api-telemetry-abstractions.md |
| NodaTime                                         | 3.3.2     | time-and-deadlines         | api-nodatime.md              |
| OpenTelemetry                                    | 1.16.0    | diagnostics-and-telemetry  | api-otel.md                  |
| OpenTelemetry.Extensions.Hosting                 | 1.16.0    | diagnostics-and-telemetry  | api-otel-hosting.md          |
| OpenTelemetry.Instrumentation.Http               | 1.15.0    | diagnostics-and-telemetry  | api-otel-instrumentation.md  |
| OpenTelemetry.Instrumentation.Runtime            | 1.15.1    | diagnostics-and-telemetry  | api-otel-instrumentation.md  |
| Polly.Core                                       | 8.7.0     | outbound-resilience        | api-polly-core.md            |
| Polly.Extensions                                 | 8.7.0     | outbound-resilience        | api-polly-extensions.md      |
| Polly.RateLimiting                               | 8.7.0     | outbound-resilience        | api-polly-ratelimiting.md    |
| Scrutor                                          | 7.0.0     | composition-and-modules    | api-scrutor.md               |
| Serilog                                          | 4.3.1     | diagnostics-and-telemetry  | api-serilog.md               |
| System.Threading.Tasks.Dataflow                  | 10.0.9    | resource-lanes             | api-dataflow.md              |
| Thinktecture.Runtime.Extensions.Json             | 10.2.0    | runtime-ports              | api-thinktecture-json.md     |

Substrate, pending, and test-only admissions:

| [PACKAGE]                                | [VERSION]      | [PAGE]                | [CATALOGUE]                          |
| :--------------------------------------- | :------------- | :-------------------- | :----------------------------------- |
| LanguageExt.Core                          | 5.0.0-beta-77  | every page            | stack doctrine (`docs/stacks/csharp`) |
| Thinktecture.Runtime.Extensions           | 10.2.0         | every page            | stack doctrine (`docs/stacks/csharp`) |
| Grpc.Net.Client                           | 2.80.0         | outbound-resilience   | catalogue pending; csproj row lands with `Outbound.cs` |
| NodaTime.Serialization.SystemTextJson     | 1.4.0          | runtime-ports         | catalogue pending; csproj row lands with `Ports.cs` |
| Microsoft.Extensions.TimeProvider.Testing | 10.7.0         | time-and-deadlines    | api-testing-seams.md (tests-only)    |
| NodaTime.Testing                          | 3.3.2          | time-and-deadlines    | api-testing-seams.md (tests-only)    |
| Microsoft.Extensions.Diagnostics.Testing  | 10.7.0         | diagnostics-and-telemetry | api-testing-seams.md (tests-only) |
