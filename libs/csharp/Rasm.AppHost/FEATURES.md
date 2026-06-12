# [APPHOST_FEATURES]

Feature atlas for the runtime spine. Every concept rides HostProfile, RuntimePhase, ConfigSource, DeadlineClass, DegradationLevel, OutboundHop, SupportTrigger, CacheLane, DrainQueue, TelemetrySignal, and FaultSource rows — a new concept is a row, never a surface. Mechanics live in `.planning/` pages; this atlas tracks the product-level capabilities the spine owns.

## [1]-[CONCEPTS]

| [INDEX] | [CONCEPT] | [MODALITIES] | [PAGES] |
| :-----: | --------- | ------------ | ------- |
| [1] | Support-bundle capture console | all | support-bundles, diagnostics-and-telemetry |
| [2] | Live degradation dashboard + operator kill-switch | all | health-and-degradation, configuration-and-options |
| [3] | Crash-recovery boot probe (host markers + own markers) | plugin, standalone | lifecycle-and-drain, support-bundles |
| [4] | Options-as-policy inspector with receipted reload | all | configuration-and-options |
| [5] | Outbound-hop resilience telemetry (incl. chaos lanes) | service, companion | outbound-resilience, diagnostics-and-telemetry |
| [6] | Resource-pressure health gauges + per-profile GC posture | all | resource-lanes, health-and-degradation, host-profiles |
| [7] | Scheduled maintenance lane (cron expressions on the schedule port) | service, standalone, long-lived companion | time-and-deadlines |
| [8] | Companion spawn + discovery manifest | standalone, companion | outbound-resilience, runtime-ports |
| [9] | OTLP telemetry export with runtime + HTTP instrumentation | service app-roots | diagnostics-and-telemetry, runtime-ports |

## [2]-[CAPABILITY_ROWS]

- Time: NodaTime clock seam + TimeProvider elapsed seam; Cronos CronExpression carried by the schedule-port record; fake pairs on the test-host row.
- Caching: HybridCache port with stampede protection, tag invalidation, entry options; per-domain CacheLane rows; L2 contribution arrives from Persistence.
- Resilience: one keyed Polly pipeline registry per OutboundHop; hedging and manual circuit-breaker control are named pipeline rows; database retry is excluded (Persistence execution strategy owns it).
- Telemetry: W3C TraceContext + Baggage spine; IMeterFactory minting; LoggerMessage source-gen delegates; stable Runtime + Http instrumentation; contributor and receipt-sink ports for sibling packages; DataClassification taxonomy owned here.
- Configuration: 8 ConfigSource rows with rank order, reload-class column (frozen | transition), source-generated binder, generated options validators + FluentValidation accumulation.
- Lifecycle: 8-phase RuntimePhase transition table; drain conductor over frozen rank bands with inbound-admission cessation first; crash markers; FaultSource union incl. posix-signal and host-crash-marker.
- Wire: STJ source-gen wire-neutral records; JsonTypeInfoResolver.Combine suite contract merge; JsonSchemaExporter schema emission; suite wire-law + TS tooling map carried by runtime-ports.

## [3]-[GAPS_TRACKED]

- Inbound operational verbs for service modalities close through the ControlService wire rows (capture-support, set-degradation, reload-options).
- Service reload trigger (SIGHUP/file-watch) and standalone crash-flag schema are research rows on their owning pages.
- DATAS GC tuning stays claim-gated behind a losing benchmark.
