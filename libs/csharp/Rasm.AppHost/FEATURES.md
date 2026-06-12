# [APPHOST_FEATURES]

Feature atlas for the runtime spine. Every concept rides HostProfile, RuntimePhase, ConfigSource,
DeadlineClass, DegradationLevel, OutboundHop, SupportTrigger, CacheLane, DrainQueue,
TelemetrySignal, and FaultSource rows — a new concept is a row, never a surface. Mechanics live
in the `.planning/` pages at the cluster anchors named below; this atlas tracks the
product-level capabilities the spine owns.

## [1]-[CONCEPTS]

| [INDEX] | [CONCEPT] | [MODALITIES] | [PAGE_CLUSTERS] |
| :-----: | --------- | ------------ | ---------------- |
| [1] | Support-bundle capture console | all | support-bundles#CAPTURE_PIPELINE, support-bundles#MANIFEST_RECEIPT, diagnostics-and-telemetry#SIGNAL_GOVERNANCE |
| [2] | Live degradation dashboard + operator kill-switch | all | health-and-degradation#DEGRADATION_RAIL, configuration-and-options#KILL_SWITCH |
| [3] | Crash-recovery boot probe (host markers + own markers) | plugin, standalone | lifecycle-and-drain#FAULT_SPINE, support-bundles#TRIGGER_UNION |
| [4] | Upgrade detection at boot via marker version stamp | plugin, standalone | lifecycle-and-drain#PHASE_FAMILY, lifecycle-and-drain#FAULT_SPINE |
| [5] | Options-as-policy inspector with receipted reload | all | configuration-and-options#POLICY_VALUES, configuration-and-options#TYPED_BINDING |
| [6] | Operational reload verbs (SIGHUP, ControlService reload-options) | service, companion | lifecycle-and-drain#FAULT_SPINE, configuration-and-options#POLICY_VALUES |
| [7] | Outbound-hop resilience telemetry (incl. chaos lanes) | service, companion | outbound-resilience#KEYED_PIPELINES, outbound-resilience#OWNERSHIP_LAW, diagnostics-and-telemetry#TELEMETRY_IDENTITY |
| [8] | Resource-pressure health gauges + per-profile GC posture | all | health-and-degradation#HEALTH_FOLD, host-profiles#PROFILE_AXIS |
| [9] | Scheduled maintenance lane (cron rows on the schedule port) | service, standalone, companion | time-and-deadlines#SCHEDULE_PORT |
| [10] | Watchdog heartbeat rows with deadline receipts | service, companion | time-and-deadlines#SCHEDULE_PORT, support-bundles#TRIGGER_UNION |
| [11] | Companion spawn + discovery manifest + UDS attach | standalone, companion | outbound-resilience#DISCOVERY_ATTACH, outbound-resilience#HOP_AXIS |
| [12] | OTLP telemetry export with runtime + HTTP instrumentation | service app-roots | diagnostics-and-telemetry#SIGNAL_GOVERNANCE, runtime-ports#WIRE_LAW |
| [13] | Receipted module composition with frozen-graph proof | all | composition-and-modules#MODULE_TABLE, composition-and-modules#SCAN_AND_DECORATE |
| [14] | HLC-correlated cross-process receipt stream | all | runtime-ports#PORT_RECORDS, lifecycle-and-drain#TS_PROJECTION |
| [15] | Suite wire law: contract merge, schema emission, TS tooling map | app roots | runtime-ports#WIRE_LAW, runtime-ports#TS_PROJECTION |
| [16] | Bounded drainable queues with receipted loss | all | resource-lanes#DRAIN_QUEUES, lifecycle-and-drain#DRAIN_CONDUCTOR |

## [2]-[CAPABILITY_ROWS]

- Profiles: eight HostProfile rows resolve every modality variance; lifetime-adapter delegate
  rows and per-user root + telemetry identity folds — host-profiles#PROFILE_AXIS,
  host-profiles#LIFETIME_ADAPTERS, host-profiles#RESOURCE_IDENTITY.
- Lifecycle: 8-phase transition table with 10 triggers; drain conductor over frozen rank bands
  with inbound-admission cessation first; FaultSource union incl. posix-signal and
  host-crash-marker; one cancellation spine — lifecycle-and-drain#PHASE_FAMILY,
  lifecycle-and-drain#DRAIN_CONDUCTOR, lifecycle-and-drain#CANCEL_SPINE.
- Time: NodaTime clock seam paired with TimeProvider elapsed seam in one ClockPolicy record;
  nine DeadlineClass rows; Cronos CronExpression on the schedule-port record; fake pairs on the
  test row — time-and-deadlines#CLOCK_SPLIT, time-and-deadlines#DEADLINE_TAXONOMY,
  time-and-deadlines#SCHEDULE_PORT.
- Configuration: 8 ranked ConfigSource rows with reload-class column (frozen, transition);
  source-generated fail-closed binder; generated options validators + FluentValidation
  accumulation; validate-once frozen publish — configuration-and-options#SOURCE_AXIS,
  configuration-and-options#TYPED_BINDING, configuration-and-options#POLICY_VALUES.
- Composition: frozen ModuleContribution rows, one receipted compose fold, Scrutor scan and
  decoration, admission-edge activation with cached constructor plans —
  composition-and-modules#MODULE_TABLE, composition-and-modules#BOUNDARY_ACTIVATION.
- Caching: HybridCache port with stampede single-flight, tag invalidation, entry options;
  per-domain CacheLane rows; L2 contribution arrives from Persistence —
  resource-lanes#CACHE_PORT.
- Pools and queues: delegate-row pool policy with reset law and test-row leak tracking;
  pipe-versus-network drain queues completing under the conductor —
  resource-lanes#OBJECT_POOLS, resource-lanes#DRAIN_QUEUES.
- Telemetry: W3C TraceContext + Baggage spine; IMeterFactory minting; LoggerMessage source-gen
  delegates; per-profile log-pipeline arbitration; per-signal governance rows; DataClassification
  taxonomy owned here — diagnostics-and-telemetry#CORRELATION_SPINE,
  diagnostics-and-telemetry#LOG_PROJECTION, diagnostics-and-telemetry#SIGNAL_GOVERNANCE,
  diagnostics-and-telemetry#REDACTION_TAXONOMY.
- Health: contributor row fold into one wire-neutral snapshot; five-level degradation rail with
  retained-capability sets, hysteresis, and forced override; grpc.health.v1 tag-predicate
  mapping — health-and-degradation#HEALTH_FOLD, health-and-degradation#DEGRADATION_RAIL,
  health-and-degradation#WIRE_HEALTH.
- Resilience: one keyed Polly pipeline per OutboundHop; standard and hedging HTTP handlers;
  manual circuit-breaker enforcement from the degradation level; one retry owner per hop with
  conflict evidence; database retry excluded (Persistence execution strategy owns it) —
  outbound-resilience#HOP_AXIS, outbound-resilience#HTTP_PIPELINES,
  outbound-resilience#OWNERSHIP_LAW.
- Wire: STJ Strict source-gen records; JsonTypeInfoResolver.Combine suite contract merge;
  JsonSchemaExporter schema emission; seven port records with the HLC receipt envelope —
  runtime-ports#PORT_RECORDS, runtime-ports#WIRE_LAW.

## [3]-[GAPS_TRACKED]

- Inbound operational verbs for service modalities close through ControlService wire rows
  (capture-support, set-degradation, reload-options) — support-bundles#TRIGGER_UNION,
  health-and-degradation#WIRE_HEALTH, configuration-and-options#POLICY_VALUES.
- Service reload trigger under launchd/systemd and the standalone crash-marker schema are
  research rows — lifecycle-and-drain#FAULT_SPINE.
- DATAS GC tuning stays claim-gated behind a losing benchmark — host-profiles#PROFILE_AXIS.
- The process-dump artifact row is designed, gated on the diagnostics-tool admission —
  support-bundles#CAPTURE_PIPELINE.
- The secrets-store keychain provider route is research-gated —
  configuration-and-options#SOURCE_AXIS.
- The UDS peer-credential read is an admission-probe row —
  outbound-resilience#DISCOVERY_ATTACH.
- The update vehicle lands as one UpdatePort row on a later admission; ReleaseIdentity ships
  vehicle-free now — outbound-resilience#HOP_AXIS.
