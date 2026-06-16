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
|  [14]   | [capability-registry](capability-registry.md)             | self-describing op catalog, discovery, command algebra, grant/cost broker, SDK codegen | finalized |
|  [15]   | [mcp-projection](mcp-projection.md)                       | MCP tool projection, dry-run preview, streaming progress, resumable handles | finalized |
|  [16]   | [sandbox-host](sandbox-host.md)                           | WASM/process isolation, grant broker, quota/kill/quarantine, supply-chain gate | finalized |
|  [17]   | [solver-plugin](solver-plugin.md)                         | solver-kind axis, plugin contract, sandboxed hosting, representation negotiation | finalized |
|  [18]   | [live-wire](live-wire.md)                                 | industrial transport axis, reactive binding, edge unit coercion, write-back transaction | finalized |
|  [19]   | [determinism-and-replay](determinism-and-replay.md)       | determinism kernel, content-addressed event log, replay-verify, macro engine, partial recompute | finalized |

## [2]-[WIRE_PAGES]

lifecycle-and-drain · health-and-degradation · support-bundles · runtime-ports · capability-registry · mcp-projection · live-wire · determinism-and-replay (each carries exactly one TS_PROJECTION cluster).

## [3]-[CATALOGUE_PENDING]

App-root-only pins with no restored closure, catalogued at app-root creation: OpenTelemetry.Exporter.OpenTelemetryProtocol, System.CommandLine, Grpc.AspNetCore, Grpc.AspNetCore.Web, Grpc.AspNetCore.HealthChecks, Microsoft.AspNetCore.JsonPatch.SystemTextJson, Serilog.Extensions.Hosting (AddSerilog app-root bridge). Plugin-host and service-app-root pins catalogued at their app-root creation: the WebAssembly component-model host runtime (sandbox-host#ISOLATION_AXIS, the linear-memory boundary and scoped import linkage), the OPC-UA / Modbus / MQTT / serial industrial-protocol client packages (live-wire#TRANSPORT_AXIS, one client per transport row behind its `OutboundHop`), the artifact-signature and SLSA in-toto attestation verification surface (sandbox-host#SUPPLY_CHAIN), and the MCP JSON-RPC transport binding (mcp-projection#METHOD_AXIS, the agent transport over the gRPC server-stream pin).

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
|  [26]   | tenant-identity threading (4th cross-package primitive)           | runtime-ports#PORT_RECORDS                                                                                         |
|  [27]   | W3C distributed trace-context propagation over the control hop    | diagnostics-and-telemetry#CORRELATION_SPINE + companion-sidecar#CONTROL_SERVICE                                    |
|  [28]   | host-side multi-peer session roster (attached/lease-epoch/presence) | companion-sidecar#PROCESS_MODALITY                                                                               |
|  [29]   | fleet-wide health-gated rolling-update wave                       | provisioning-and-update#ROLLOVER_DRAIN                                                                             |
|  [30]   | self-describing op catalog with effect/idempotency/cost/permission | capability-registry#DESCRIPTOR_AXIS + capability-registry#DISCOVERY_FOLD                                          |
|  [31]   | commit-or-rollback intent transaction over the dispatch rail      | capability-registry#COMMAND_ALGEBRA                                                                                |
|  [32]   | scoped grant algebra, consent/elevation, cost metering, dry-run sim | capability-registry#GRANT_BROKER                                                                                  |
|  [33]   | polyglot SDK codegen off one descriptor source                    | capability-registry#SDK_CODEGEN                                                                                    |
|  [34]   | MCP tool projection, dry-run cost preview, streaming, resumable    | mcp-projection#METHOD_AXIS + mcp-projection#TOOL_DISPATCH + mcp-projection#STREAM_PROGRESS                         |
|  [35]   | WASM/process isolation with no-ambient-authority grant broker     | sandbox-host#ISOLATION_AXIS + sandbox-host#GRANT_HANDLE                                                            |
|  [36]   | plugin quota/kill/quarantine + signed/SLSA/semver supply-chain     | sandbox-host#QUOTA_CONTROL + sandbox-host#SUPPLY_CHAIN                                                             |
|  [37]   | solver/mesher/optimizer/CAM/material/codec plugin contract        | solver-plugin#SOLVER_KIND + solver-plugin#PLUGIN_CONTRACT + solver-plugin#SOLVER_HOSTING                          |
|  [38]   | reactive bidirectional external binding + edge unit coercion      | live-wire#TRANSPORT_AXIS + live-wire#BINDING_SPEC + live-wire#WRITE_BACK + live-wire#BINDING_HEALTH                |
|  [39]   | determinism kernel + content-addressed hash-chained event log     | determinism-and-replay#DETERMINISM_KERNEL + determinism-and-replay#EVENT_LOG                                       |
|  [40]   | replay-verify + macro record/replay + partial-recompute graph     | determinism-and-replay#REPLAY_VERIFY + determinism-and-replay#MACRO_ENGINE + determinism-and-replay#RECOMPUTE_GRAPH |
|  [41]   | energy/thermal-aware compute-fidelity scaling                     | host-profiles#POWER_AND_FIDELITY                                                                                   |
|  [42]   | multi-channel delivery fan-out with receipts and dedupe           | outbound-resilience#DELIVERY_FANOUT                                                                                |
|  [43]   | declarative alert rule engine over continuous queries             | health-and-degradation#ALERT_ENGINE                                                                               |

## [5]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail; density means
no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its
owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a
new surface. The owner budget below is the public-surface cap. Key policies, receipts, policy
records, runtime records, and wire shapes ride inside their owner's signature region (ledger rows
AH-01 through AH-13 and AH-04a-d); a public type outside those regions is the named defect.

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
|  [46]   | service host            | `ServiceHost`                    | static surface          | gRPC server registration; `ControlTransport` 1-case union (UDS) | FINALIZED |
|  [47]   | degradation cascade     | `DegradationCascade`             | static write surface    | 1 parent-to-child `Cascade` write; `CascadeReceipt`        | SPIKE     |
|  [48]   | peer admission          | `PeerAdmission`                  | static accept-side read | 2 platform branches (Linux `SO_PEERCRED`, macOS `LOCAL_PEERCRED`); `Ucred`/`Xucred`/`PeerCredential` | FINALIZED |
|  [49]   | tenant context          | `TenantContext`                  | record port             | 4th cross-package primitive (`TenantId` `UInt128` value object, `TenantSlot` `rasm.tenant` GUC/baggage key, `Root` single-tenant default); threads to server-tier RLS + cache-key partition | FINALIZED |
|  [50]   | trace context           | `TraceContext`                   | static fold             | W3C `traceparent`/`tracestate` propagation over the `Correlation` composite (`Inject`/`Extract`/`Continue` gRPC-metadata carrier) | FINALIZED |
|  [51]   | peer roster             | `PeerRoster`                     | record fold             | host-side attached-peer/lease-epoch/presence roster (`Boot`/`Admit`/`Renew`/`Drop`/`Sweep`); `RosterReceipt` per-mutation projection | FINALIZED |
|  [52]   | fleet roll              | `FleetRoll`                      | static surface          | fleet-wide health-gated rolling-update conductor over `PeerRoster.Attached`; `FleetRollReceipt` per-wave projection | FINALIZED |
|  [53]   | power and fidelity      | `FidelityScale`                  | record + static probe   | `PowerState` 3-row `[SmartEnum<string>]`, `ThermalPressure` 4-row `[SmartEnum<int>]`, `PowerCell` MeterListener seat, `PowerProbe` IOKit/SMC native read | SPIKE |
|  [54]   | delivery fan-out        | `DeliveryChannel`                | `[SmartEnum<string>]`   | 4 rows (push, webhook, email, in-app); `DeliveryMessage`/`DeliveryReceipt`, `DeliveryFanout` dedupe-and-fan over `OutboundHop` | FINALIZED |
|  [55]   | alert engine            | `AlertRule`                      | record + static fold    | `AlertSeverity` 4-row, `AlertCondition` 3-case `[Union]` (threshold/anomaly/forecast), `AlertState`, `AlertEngine` evaluate/backtest with hysteresis+escalation | FINALIZED |
|  [56]   | descriptor axis         | `CapabilityDescriptor`           | record + static surface | `EffectClass`/`Idempotency`/`CostUnit` `[SmartEnum<string>]`, `CostModel`/`CostVector`/`PermissionShape`, `DescriptorSurface` fan-in | FINALIZED |
|  [57]   | discovery fold          | `CapabilityRegistry`             | frozen catalog          | `DiscoveryQuery` 5-case `[Union]`, `DiscoveryResult`, alternate-lookup probe over the descriptor fan-in | FINALIZED |
|  [58]   | command algebra         | `CommandAlgebra`                 | static surface          | `CommandTxn` 4-case `[Union]`, `CommandFault` `[Union]`, `CommandReceipt`, `CommandRuntime`; commit-or-rollback over the Compute dispatch rail | FINALIZED |
|  [59]   | grant broker            | `GrantBroker`                    | record + static surface | `GrantScope`, `Consent` 4-case `[Union]`, `GrantFault` `[Union]`, `Budget` live-metering cell; object-set×op-class×ceiling×window | FINALIZED |
|  [60]   | SDK codegen             | `SdkCodegen`                     | static fold             | `SdkTarget` 3-row `[SmartEnum<string>]` (csharp/ts/python), `SdkArtifact`; one descriptor source, three emitters | FINALIZED |
|  [61]   | MCP method axis         | `McpMethod`                      | `[SmartEnum<string>]`   | 8 rows; `ToolProjection`/`McpCatalog`/`McpTool`/`McpResource`/`McpPrompt`; registry-to-MCP fold | FINALIZED |
|  [62]   | MCP dispatch            | `McpDispatch`                    | static surface          | `McpFault` `[Union]`, `CostPreview`, `ToolResult`, `McpRuntime`; dry-run preview + brokered tool call | FINALIZED |
|  [63]   | MCP stream progress     | `StreamProgress`                 | static surface          | `ProgressFrame` 6-case `[Union]`, `ResumeToken`, `AgentSession` lease cell; server-stream fan + resumable handle | SPIKE |
|  [64]   | sandbox isolation       | `SandboxIsolation`               | `[SmartEnum<string>]`   | 2 rows (wasm-component, process); `SandboxRow`/`SandboxRows`, `PluginInstance`, `SandboxFault` `[Union]` | SPIKE |
|  [65]   | grant handle            | `GrantHandle`                    | record + static surface | `BrokeredCall`, `GrantHandleSurface`; capability-brokered no-ambient-authority plugin authority | FINALIZED |
|  [66]   | quota control           | `QuotaShape`                     | record + static surface | `QuotaCell` live-metering capsule, `Quarantine` 4-case `[Union]`, `QuotaControl`; kill/quarantine rail | FINALIZED |
|  [67]   | supply chain            | `SupplyChainGate`                | record + static surface | `PluginArtifact`/`Attestation`, `SemverGate`, `SupplyChainFault` `[Union]`; signature+SLSA+semver fail-closed admission | SPIKE |
|  [68]   | solver kind             | `SolverKind`                     | `[SmartEnum<string>]`   | 7 rows; `KindContract`/`KindContracts`, `SolverFault` `[Union]`; per-category contract shape | FINALIZED |
|  [69]   | plugin contract         | `SolverPluginContract`           | static surface          | `SolverManifest`/`OpDeclaration`; declared-op-to-descriptor projection | FINALIZED |
|  [70]   | solver hosting          | `SolverHosting`                  | static surface          | `HostedSolver`/`Negotiation`/`SolverHostingRuntime`; sandboxed load + registry projection + representation negotiation | SPIKE |
|  [71]   | external transport      | `ExternalTransport`              | `[SmartEnum<string>]`   | 8 rows; `ReadShape` 2-row, `TransportRow`/`TransportRows`, `WireFault` `[Union]`, `ExternalValue` | SPIKE |
|  [72]   | binding spec            | `BindingSpec`                    | record + static engine  | `BindingDirection` flags, `CoercedValue`, `LiveWire` reactive engine; edge unit coercion over `QuantityFamily.Admit` | FINALIZED |
|  [73]   | write-back              | `WriteBack`                      | `[Union]` + static      | 4 cases (ack/reject/rollback/coalesce), `WriteReceipt`, `WriteBackSurface`; transactional outbound write | FINALIZED |
|  [74]   | binding health          | `BindingState`                   | `[SmartEnum<string>]`   | 5 rows; `BindingHealth` aggregating into one `remote`-tagged health contributor | FINALIZED |
|  [75]   | determinism kernel      | `DeterminismContext`             | record + static surface | `FloatMode` 3-row `[SmartEnum<string>]`, `EnvFingerprint`, `DeterminismKernel`; pinned RNG/float/env over `HostFingerprint` | SPIKE |
|  [76]   | event log               | `EventLog`                       | static surface          | `LogEntry`, `ContentHash` `[ValueObject<string>]`, hash-chain append/verify; projects to `OpLogEntry` | FINALIZED |
|  [77]   | replay verify           | `ReplayVerify`                   | static surface          | `ReplayOutcome` 4-case `[Union]`, `ReplayFault` `[Union]`, `ReplayRuntime`; re-execute + content-hash identity proof | FINALIZED |
|  [78]   | macro engine            | `MacroEngine`                    | record + static surface | `Macro`/`MacroParameter`; content-addressed record + parameterized atomic replay over `CommandAlgebra.Batch` | FINALIZED |
|  [79]   | recompute graph         | `RecomputeGraph`                 | static surface          | `RecomputeNode`, content-address dependency walk; minimal downstream partial recompute with unchanged-output prune | FINALIZED |

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
|  [13]   | `Companion.cs`     | process modality, control service, service host, cascade, peer admission | verb folds, UDS control intake, cascade convergence, peer-cred read |
|  [14]   | `CapabilityRegistry.cs` | descriptor axis, discovery fold, command algebra, grant broker, SDK codegen | descriptor totality, discovery shape-dispatch, commit-or-rollback, grant ceiling, schema-identity codegen |
|  [15]   | `Determinism.cs`   | determinism kernel, event log, replay verify, macro engine, recompute graph | seed reproducibility, chain-verify, replay hash identity, macro atomicity, minimal recompute cone |
|  [16]   | `Mcp.cs`           | MCP method axis, tool dispatch, stream progress | tool projection, dry-run-equals-charge preview, resumable-after-bounce |
|  [17]   | `Sandbox.cs`       | isolation axis, grant handle, quota control, supply chain | no-ambient-authority import scope, quota breach kill, fail-closed supply-chain |
|  [18]   | `SolverPlugin.cs`  | solver kind, plugin contract, solver hosting   | kind-contract totality, representation negotiation, sandboxed registry projection |
|  [19]   | `LiveWire.cs`      | transport axis, binding spec, write-back, binding health | edge coercion totality, transactional write-back rollback, feedback-loop guard |

`Provisioning.cs` lands after `Ports.cs` because `UpdateRail` consumes `Lifecycle`,
`DrainConductor`, `ReceiptSinkPort`, and the `AppHostWireContext.UpdateReceipt` row every earlier
file declares; the `UpdateCheck` detect-leg hop stays at `Outbound.cs`, so provisioning is the
post-fetch state machine only. `Companion.cs` lands last because `ControlInbound` folds onto
`DegradationCell` (Health), `OptionsAdmission` (Configuration), `SupportCapture` (Support), and
`ProcessModality` composes `Discovery`/`CompanionChild`/`GrpcChannelPolicy` from `Outbound.cs`
without re-declaring the dial-out mechanics; `DegradationCascade` is a write consumer of
`DegradationCell.Cascade`, never a second degradation owner. The gRPC server-host packages enter
only at service app roots behind the app-root pin and never below a plugin row.

`CapabilityRegistry.cs` lands after `Companion.cs` because `CommandAlgebra` folds onto
`ReceiptSinkPort` and the `AppHostWireContext.CommandReceipt`/`DescriptorReceipt` rows every earlier
file declares, and it composes the Compute `ComputeIntent`/`IntentAdmission`/`SelectionReceipt`
dispatch surface as settled cross-package vocabulary; the descriptor catalog is generated from the
canonical op surfaces (`TensorOpFamily`, `ModelIdentity`, `ComputeEndpoint`, `QuantityFamily`,
`SolverContract`) so it never hand-lists ops. `Determinism.cs` lands after the registry because the
event log records `CommandReceipt` rows and projects each `LogEntry` to one `OpLogEntry` on the
Persistence changefeed, the macro and recompute engines replay through `CommandAlgebra`, and the
determinism kernel composes the Compute `HostFingerprint`. `Mcp.cs`, `Sandbox.cs`,
`SolverPlugin.cs`, and `LiveWire.cs` all land after `CapabilityRegistry.cs` because each is a
projection of or a consumer of the registry: MCP projects the discovery fold onto the agent
transport, the sandbox grants brokered access to the registered descriptors, the solver plugin
projects declared ops into the registry, and live-wire pushes coerced external values as registered
commands. `LiveWire.cs` consumes the Compute `QuantityFamily.Admit`/`Render` unit coercion and
composes `OutboundHop`/`OutboundSurface` for its transport bytes, never re-declaring the dial-out
mechanics. The `Sandbox.cs` process-isolation row reuses `Discovery`/`CompanionPeer`/`PeerAdmission`
from `Outbound.cs` and `Companion.cs` verbatim. The WASM component-model runtime and the
industrial-protocol client packages enter only at service or plugin-host app roots behind the
app-root pin and never below a plugin row.

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
- NEVER a second op-metadata owner beside `CapabilityDescriptor`: a per-op attribute scatter, a
  hand-kept command list, or a second cost table is the named defect; effect, idempotency, cost,
  and permission are descriptor columns.
- NEVER a second permission-and-cost owner beside `GrantBroker`: a per-op permission check, an
  ambient role flag, and a second cost meter are the deleted forms; the descriptor declares the
  permission and the broker meters one budget per tenant.
- NEVER an in-process third-party plugin: a direct `Assembly.LoadFrom`, a plugin `AppDomain`, or a
  shared managed-heap plugin reference is the deleted form; a plugin always crosses the WASM or
  process isolation boundary and reaches the host only through the brokered `GrantHandle`.
- NEVER a plugin-private geometry representation or a second op execution path: a plugin speaks the
  Compute canonical `EncodedTensor` encoding and its ops dispatch through the command algebra and
  the Compute substrate selection, never a privileged solver execution.
- NEVER a second RNG, clock-seeded entropy source, or non-chained event log: `DeterminismContext`
  owns the seed and float mode, `EventLog` is the single hash-chained content-addressed command
  log riding the durable `OpLog`, and a macro or recompute is a slice of that one log.
- NEVER a second notification sender or external-binding poller: `DeliveryFanout` fans every
  channel over its `OutboundHop` under one dedupe, and `ExternalTransport`/`LiveWire` own every
  industrial edge over one reactive binding contract with mandatory edge unit coercion.
- NEVER a second alerting owner or power monitor: `AlertEngine` evaluates over the existing health
  snapshot stream, `FidelityScale` reads the existing power-state probe, and neither forces a
  degradation level — alerting and energy-awareness are read consumers of the health and power
  signals, never parallel state machines.
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
|   [5]   | Microsoft.Extensions.Compliance.Redaction               | diagnostics-and-telemetry | api-redaction.md                      | admitted          |
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
|  [17]   | Microsoft.Extensions.Http.Resilience                    | outbound-resilience       | api-resilience.md                     | admitted          |
|  [18]   | Microsoft.Extensions.Logging.Abstractions               | diagnostics-and-telemetry | api-logging.md                        | admitted          |
|  [19]   | Microsoft.Extensions.ObjectPool                         | resource-lanes            | api-objectpool.md                     | admitted          |
|  [20]   | Microsoft.Extensions.Options                            | configuration-and-options | api-options.md                        | admitted          |
|  [21]   | Microsoft.Extensions.Telemetry                          | diagnostics-and-telemetry | api-telemetry.md                      | admitted          |
|  [22]   | Microsoft.Extensions.Telemetry.Abstractions             | diagnostics-and-telemetry | api-telemetry-abstractions.md         | admitted          |
|  [23]   | NodaTime                                                | time-and-deadlines        | api-nodatime.md                       | admitted          |
|  [24]   | OpenTelemetry                                           | diagnostics-and-telemetry | api-otel.md                           | admitted          |
|  [25]   | OpenTelemetry.Extensions.Hosting                        | diagnostics-and-telemetry | api-otel-hosting.md                   | admitted          |
|  [26]   | OpenTelemetry.Instrumentation.Http                      | diagnostics-and-telemetry | api-otel-instrumentation.md           | admitted          |
|  [27]   | OpenTelemetry.Instrumentation.Runtime                   | diagnostics-and-telemetry | api-otel-instrumentation.md           | admitted          |
|  [28]   | Polly.Core                                              | outbound-resilience       | api-polly-core.md                     | admitted          |
|  [29]   | Polly.Extensions                                        | outbound-resilience       | api-polly-extensions.md               | admitted          |
|  [30]   | Polly.RateLimiting                                      | outbound-resilience       | api-polly-ratelimiting.md             | admitted          |
|  [31]   | Scrutor                                                 | composition-and-modules   | api-scrutor.md                        | admitted          |
|  [32]   | Serilog                                                 | diagnostics-and-telemetry | api-serilog.md                        | admitted          |
|  [33]   | System.Threading.Tasks.Dataflow                         | resource-lanes            | api-dataflow.md                       | admitted          |
|  [34]   | Thinktecture.Runtime.Extensions.Json                    | runtime-ports             | api-thinktecture-json.md              | admitted          |
|  [35]   | LanguageExt.Core                                        | every page                | stack doctrine (`docs/stacks/csharp`) | admitted          |
|  [36]   | Thinktecture.Runtime.Extensions                         | every page                | stack doctrine (`docs/stacks/csharp`) | admitted          |
|  [37]   | Velopack                                                | provisioning-and-update   | api-velopack.md                       | admitted          |
|  [38]   | Grpc.Net.Client                                         | outbound-resilience       | api-grpc-client.md                    | admitted          |
|  [39]   | Grpc.AspNetCore                                          | companion-sidecar         | —                                     | app-root-pending  |
|  [40]   | Grpc.AspNetCore.HealthChecks                             | companion-sidecar         | —                                     | app-root-pending  |
|  [41]   | NodaTime.Serialization.SystemTextJson                   | runtime-ports             | api-nodatime-stj.md                   | admitted          |
|  [42]   | Microsoft.Extensions.TimeProvider.Testing               | time-and-deadlines        | api-testing-seams.md                  | tests-only        |
|  [43]   | NodaTime.Testing                                        | time-and-deadlines        | api-testing-seams.md                  | tests-only        |
|  [44]   | Microsoft.Extensions.Diagnostics.Testing                | diagnostics-and-telemetry | api-testing-seams.md                  | tests-only        |
|  [45]   | Microsoft.Extensions.Options.ConfigurationExtensions    | configuration-and-options | api-config-providers.md               | admitted          |

## [11]-[REFINEMENT_HORIZON]

Folder-specific deepening targets beyond the closed corpus, each carrying its open probe. The Velopack `VelopackHook` delegate-signature is closed by tier-1 decompile: `api-velopack.md` carries the exact `public delegate void VelopackHook(SemanticVersion version)` and the `OnFirstRun`/`OnRestarted`/`OnAfter*FastCallback` registrations at the `VelopackApp.Build()...Run()` app-root bootstrap, so provisioning-and-update#UPDATE_RAIL transcribes a settled member shape with no open residual. The `ServiceMappingCollection.Map` versus `MapService` behavioral distinction on `GrpcHealthChecksOptions.Services` settles the by-service-name versus predicate-routing wire-health registration row at companion-sidecar#CONTROL_SERVICE. The cross-process degradation-cascade convergence bridge scenario — a companion observes the parent level over the control hop, lands it as a `DegradationCell.Cascade` floor, and re-derives on release — runs against the paired and companion topologies inside the running integrated host (companion-sidecar#DEGRADATION_CASCADE `[CASCADE_CONVERGENCE]`, the legitimate tier-3 live-host residual). The macOS keychain secrets-store route (configuration-and-options#SOURCE_ROUTES) and the launchd/systemd SIGHUP delivery (lifecycle-and-drain#FAULT_PROBES) carry settled member shapes, but their live delivery stays a tier-3 residual: the keychain leg is a HARD BAN on unattended `SecItem*` reads (a live read prompts a macOS dialog) and the SIGHUP leg needs a live service-manager to deliver the signal, so both pages correctly keep their RESEARCH rows OPEN until the integrated-host probe lands. The bar: any host modality boots, updates, degrades, drains, serves its control plane, and reports through this spine with zero app-side ceremony.

Testing-infrastructure horizon: the deterministic-clock seam pairs `FakeClock` (`NodaTime.Testing`) with `FakeTimeProvider` over the dual clock authority, and the telemetry-seam assertions ride `MetricCollector<T>`/`FakeLogCollector` against the diagnostics signal-governance fold.
