# [RASM_APPHOST_ROADMAP]

The `.planning/` corpus is finalized; implementation transcribes pages in the charter
BUILD_ORDER ([planning charter](.planning/README.md)). Every task exits against named page
clusters and proves through the charter PROOF_GATES (G1 restore, G2 catalogue, G3 static,
G4 specs, G5 bridge, G6 mermaid).

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                                      |
| :-----: | :---------------- | :------------------------------------------- |
|   [1]   | planning corpus   | 11 pages finalized; charter complete         |
|   [2]   | package graph     | runtime closure admitted and lock-tracked    |
|   [3]   | production source | absent                                       |
|   [4]   | test project      | `Rasm.AppHost.Tests` node present, empty     |
|   [5]   | API catalogues    | 30 pages current; 2 admissions pending pages |

## [2]-[START_GATES]

Implementation-start gates from the campaign binding: bridge-proofed spikes and
research-row resolution probes that need a live host or scratch process. Decompile-grade
research rows (`assay api query`) resolve inline inside the owning task and are not listed.

| [INDEX] | [GATE]                                                             | [PROBE]                                                                             | [UNBLOCKS]                              |
| :-----: | :----------------------------------------------------------------- | :---------------------------------------------------------------------------------- | :-------------------------------------- |
|   [1]   | Generic Host boot and unload in plugin ALC                         | `uv run python -m tools.assay bridge verify --pattern host_boot_drain`              | host-profiles#LIFETIME_ADAPTERS         |
|   [2]   | Kestrel/ASP.NET shared framework in RhinoWIP plugin ALC            | bridge scenario at the Rhino app root serving grpc.health over UDS                  | outbound-resilience#DISCOVERY_ATTACH    |
|   [3]   | drain ForceFlush latency inside the cooperative allotment          | `libs/csharp/Rasm.AppHost/scenarios/drain-deadlines.verify.csx` under live RhinoWIP | time-and-deadlines#DEADLINE_TAXONOMY    |
|   [4]   | SIGHUP delivery under launchd and systemd                          | `dotnet run` scratch headless host; `kill -HUP` asserts one reload, zero drains     | lifecycle-and-drain#FAULT_SPINE         |
|   [5]   | secrets-store keychain provider route                              | `dotnet run` keychain spike: P/Invoke versus `/usr/bin/security` child              | configuration-and-options#SOURCE_AXIS   |
|   [6]   | binder generator interception of `Get<T>` with fail-closed options | `dotnet build` probe project + binlog inspection of emitted interceptors            | configuration-and-options#TYPED_BINDING |
|   [7]   | host-document transition reload over the in-memory mount           | `dotnet run` ConfigurationManager probe asserting one monitor invocation            | configuration-and-options#SOURCE_AXIS   |
|   [8]   | UDS peer-credential raw-option read on macOS and Linux             | `dotnet run` UDS accept spike asserting LOCAL_PEERCRED                              | outbound-resilience#DISCOVERY_ATTACH    |
|   [9]   | dump and gcdump tool admission for the process-dump row            | `dotnet package search dotnet-gcdump --exact-match`                                 | support-bundles#CAPTURE_PIPELINE        |
|  [10]   | web app-root static-asset compile under the shared framework       | `dotnet build` scratch web root compiling `UseStaticFiles` + `MapFallbackToFile`    | host-profiles#PROFILE_AXIS              |
|  [11]   | assay test rail row for the package test project                   | `uv run python -m tools.assay test run --target Rasm.AppHost.Tests`                 | every G4 proof below                    |

## [3]-[IMPLEMENTATION_TASKS]

Ordered by the charter BUILD_ORDER; each task transcribes its clusters verbatim, resolves the
page RESEARCH rows gated on those clusters, and exits on the named proof.

| [INDEX] | [TASK]             | [EXITS_AGAINST]                                                                                                            | [PROOF]                                                                                      |
| :-----: | :----------------- | :------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------- |
|   [1]   | `Time.cs`          | time-and-deadlines#CLOCK_SPLIT, #DEADLINE_TAXONOMY, #SCHEDULE_PORT                                                         | G3 + G4: deadline receipts and cron occurrences under `FakeClock`/`FakeTimeProvider`         |
|   [2]   | `Profiles.cs`      | host-profiles#PROFILE_AXIS, #LIFETIME_ADAPTERS, #RESOURCE_IDENTITY                                                         | G3 + G4: Resolve/Roots law matrix; G5: gate [1]                                              |
|   [3]   | `Lifecycle.cs`     | lifecycle-and-drain#PHASE_FAMILY, #FAULT_SPINE, #DRAIN_CONDUCTOR, #CANCEL_SPINE                                            | G3 + G4: total 8x10 transition matrix, marker round-trip, straggler outcomes                 |
|   [4]   | `Configuration.cs` | configuration-and-options#SOURCE_AXIS, #TYPED_BINDING, #POLICY_VALUES, #KILL_SWITCH                                        | G3 + G4: rank fold, fail-closed bind, reload-class gating                                    |
|   [5]   | `Composition.cs`   | composition-and-modules#MODULE_TABLE, #SCAN_AND_DECORATE, #BOUNDARY_ACTIVATION                                             | G3 + G4: compose under `ValidateOnBuild` + `ValidateScopes`                                  |
|   [6]   | `ResourceLanes.cs` | resource-lanes#CACHE_PORT, #OBJECT_POOLS, #DRAIN_QUEUES                                                                    | G3 + G4: tag-cut semantics, reset law, unreceipted-loss rejection                            |
|   [7]   | `Diagnostics.cs`   | diagnostics-and-telemetry#TELEMETRY_IDENTITY, #CORRELATION_SPINE, #LOG_PROJECTION, #SIGNAL_GOVERNANCE, #REDACTION_TAXONOMY | G3 + G4: `FakeLogCollector` + `MetricCollector<T>` assertions                                |
|   [8]   | `Health.cs`        | health-and-degradation#HEALTH_FOLD, #DEGRADATION_RAIL, #WIRE_HEALTH                                                        | G3 + G4: escalation-immediate, recovery-hysteresis, force-beats-derived                      |
|   [9]   | `Support.cs`       | support-bundles#TRIGGER_UNION, #CAPTURE_PIPELINE, #MANIFEST_RECEIPT                                                        | G3 + G4: coalesce gate, caps with truncation receipts, eviction sweep                        |
|  [10]   | `Outbound.cs`      | outbound-resilience#HOP_AXIS, #HTTP_PIPELINES, #KEYED_PIPELINES, #DISCOVERY_ATTACH, #OWNERSHIP_LAW                         | G3 + G4: admission fold, owner-conflict evidence, breaker enforcement; G5: gates [2] and [8] |
|  [11]   | `Ports.cs`         | runtime-ports#PORT_RECORDS, #WIRE_LAW                                                                                      | G3 + G4: HLC advance law, wire round-trip incl. NodaTime converter precedence                |

TS_PROJECTION clusters (lifecycle-and-drain, health-and-degradation, support-bundles,
runtime-ports) transcribe into the TS workspace at web app-root creation; they carry no C#
build row. G1 and G2 run once before task [1] and again on any manifest or catalogue change;
G6 runs on any page-diagram edit.
