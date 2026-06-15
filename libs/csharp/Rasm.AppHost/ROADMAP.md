# [RASM_APPHOST_ROADMAP]

The `.planning/` corpus is finalized; implementation transcribes pages in the charter
BUILD_ORDER ([planning charter](.planning/README.md)). Every task exits against named page
clusters and proves through the charter PROOF_GATES.

## [1]-[CURRENT_POSITION]

| [INDEX] | [SURFACE]         | [STATE]                                      |
| :-----: | :---------------- | :------------------------------------------- |
|   [1]   | planning corpus   | 11 pages finalized; charter complete         |
|   [2]   | package graph     | runtime closure admitted and lock-tracked    |
|   [3]   | production source | absent                                       |
|   [4]   | test project      | `Rasm.AppHost.Tests` node present, empty     |
|   [5]   | API catalogues    | 30 pages current; 2 admissions pending pages |

## [2]-[START_GATES]

Implementation-start gates: bridge-proofed spikes and research-resolution probes that need a
live host or scratch process. Decompile-grade research items (`assay api query`) resolve
inline inside the owning task and are not listed.

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
RESEARCH items its pages carry, and exits on the named proof.

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
|  [10]   | `Outbound.cs`      | outbound-resilience#HOP_AXIS, #HTTP_PIPELINES, #KEYED_PIPELINES, #OWNERSHIP_LAW, #DISCOVERY_ATTACH                         | G3 + G4: admission fold, owner-conflict evidence, breaker enforcement; G5: gates [2] and [8] |
|  [11]   | `Ports.cs`         | runtime-ports#PORT_RECORDS, #WIRE_LAW                                                                                      | G3 + G4: HLC advance law, wire round-trip incl. NodaTime converter precedence                |

G1 and G2 run once before task [1] and again on any manifest or catalogue change; G6 runs on
any page-diagram edit.

## [4]-[TESTING_APPROACH]

Universal rails carry one shared legend (owner + resolved member identical across the four packages); versions live in `Directory.Packages.props` `ItemGroup Label="Test Stack"` and `.config/dotnet-tools.json`, never restated here.

| [RAIL]                      | [OWNER]                  | [RESOLVED MEMBER / TOKEN]                                                                                       |
| :-------------------------- | :----------------------- | :------------------------------------------------------------------------------------------------------------- |
| xUnit v3 managed law        | `managed-laws.md [1]`    | `[Fact]`, `[Theory]`, `TheoryData<…>`, `[AssemblyFixture]`, `IClassFixture<T>`, `Assert.Throws*` (boundary-only) |
| CsCheck PBT                 | `managed-laws.md [3]`    | `Spec.ForAll`, `Spec.Metamorphic`, `Spec.ModelBased`, `Spec.MetamorphicOps`; `Check.Sample`, `Check.SampleParallel`, `Check.Faster` |
| coverlet.MTP coverage       | `evidence-rails.md [1]`  | `--coverlet`, `--coverlet-output-format`, `--coverlet-exclude-by-attribute`                                     |
| dotnet-stryker mutation     | `evidence-rails.md [2]`  | `dotnet-stryker` modes `off` / `changed` / `full`                                                              |
| Verify.XunitV3 snapshot     | `evidence-rails.md [3]`  | `Verify.XunitV3`                                                                                               |
| ArchUnitNET architecture    | `specialized-rails.md [1]` | `TngTech.ArchUnitNET.xUnitV3`; rail `tests/csharp/_architecture`                                               |

Universal-rail concept differentiator:

| [RAIL]                   | [CONCEPT PROVEN]                                                                                                                          |
| :----------------------- | :--------------------------------------------------------------------------------------------------------------------------------------- |
| xUnit v3 managed law     | boot-fold descriptor receipt (lifetime / dormant / decorated counts), schema and rank folds, options-admission validator law; `Assert.Throws*` only at the post-seal `HostApplicationBuilderSettings` boundary |
| CsCheck PBT              | stamp-algebra causal-monotone order (`Advance` / `Receive`), degradation-rank FSM with hysteresis, `Atom` / `Ref` cell races on the phase spine |
| coverlet.MTP coverage    | managed reachability of fold / projection / validator surfaces; runtime-owned native host paths classified out                            |
| dotnet-stryker mutation  | killing oracle over rank-advance gradient, drain-band ordering, options relational `Validate` rows, schema-verdict switch                  |
| Verify.XunitV3 snapshot  | redaction-projection reload-receipt JSON + composition-receipt JSON as normalized evidence JSON; scrub clocks and paths                   |
| ArchUnitNET architecture | module / registration dependency direction; testing seams never enter production composition; no ambient-`TimeProvider`-now admission     |

Package-specific rails:

| [RAIL]                                      | [OWNER]                  | [CONCEPT PROVEN]                                                                                  | [RESOLVED MEMBER / TOKEN]                                                                              |
| :------------------------------------------ | :----------------------- | :----------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------- |
| telemetry-seam law (rides xUnit + runtime)  | `api-testing-seams.md`   | counter / log assertions over diagnostics signal governance — instrument tap + record sink       | `MetricCollector<T>`, `FakeLogCollector`, `FakeLogCollectorOptions`, `AddFakeLogging`, `GetSnapshot`, `GetMeasurementSnapshot` |
| host/runtime scenarios                      | `testing/README.md [3]`  | live host boot, lifetime tokens, drain-band native settle; the deterministic-clock seam pairs both clock authorities | `FakeTimeProvider`, `FakeClock`, `SetUtcNow`, `Advance`, `FromUtc`                                     |

N/A rails: BenchmarkDotNet — AppHost owns no durable managed numeric kernel (`specialized-rails.md [2]` reject names host / runtime surfaces). SharpFuzz — AppHost admits no parser / decoder / grammar surface; config binding is generator-on-generator (`specialized-rails.md [3]`).

## [5]-[EXIT]

The package exits implementation when every BUILD_ORDER file is transcribed `Time.cs` through
`Ports.cs`, every PROOF_GATES row is green (G1 restore, G2 `api doctor`/`resolve`, G3 `static
build`, G4 `test run`, G5 `bridge verify`, G6 `mmdc` render, G7 `Grpc.Core.Api` spec compile),
the GAP_LEDGER stays fully CLOSED, every START_GATES probe discharges under live RhinoWIP or its
scratch host and lands as a settled fence row rather than a re-opened gate, and the charter
`spec` gate passes on the full suite.
