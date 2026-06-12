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

Sections [DENSITY_BAR], [BUILD_ORDER], [FILE_PROCESS], [PROOF_GATES], [PROHIBITIONS], [ADMISSIONS_RECORD] complete after page finalization.
