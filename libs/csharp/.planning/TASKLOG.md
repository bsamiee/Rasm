# [CSHARP_BRANCH_TASKLOG]

C#-internal cross-folder and infrastructure open work no single package owns; per-package open work lives in each package `TASKLOG.md`; closed items do not appear. Cross-language amendments (the `CrdtOpWire` wire-vocabulary change, the `CapabilityDescriptor` SDK-codegen consumption) and the sibling-branch seams live in Tier 0 (`libs/.planning/TASKLOG.md`) and are referenced as Tier-0 seams, never restated here.

## [1]-[NATIVE_AND_SERVER_PROBES]

| [INDEX] | [ITEM]                                                                                                                                                                              | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------- | :------- |
|   [1]   | Deploy assets at first server app root: postgresql.conf preload fragments, pg_hba fragments, role grants; PostGIS 3.6.1+ server-side enablement                                     | Persistence/server-tier#CLUSTER_CONFIG  | SPIKE    |
|   [2]   | Disposable tier-2 server probes via `assay spike up\|verify\|env\|down`; generated compose assets under `.artifacts/spikes/provisioning`, public Timescale PG18 + ParadeDB PG18 images, only labelled spike containers torn down | Persistence/server-tier#TIMESCALE_PROVISIONING | SPIKE    |

## [2]-[ADMISSION_DECISIONS_PENDING]

| [INDEX] | [ITEM]                                                                                                              | [PAGE#CLUSTER]                       | [STATUS] |
| :-----: | :---------------------------------------------------------------------------------------------------------------- | :----------------------------------- | :------- |
|   [1]   | SharpGLTF / Xbim interchange admission — named interchange need (Rasm.Rhino owns host formats today)               | Compute/interchange#FORMAT_AXIS      | BLOCKED  |
|   [2]   | `Dock.Serializer.*` package naming — first app-root layout persistence (AppUi DockSerializer round-trip spec)      | AppUi/shell-navigation#DOCK_LAYOUTS  | BLOCKED  |
|   [3]   | `Microsoft.Extensions.Options.ConfigurationExtensions` central-pin policy (directly-invoked lock-pinned transitive) | AppHost/configuration-and-options#TYPED_BINDING | BLOCKED  |

## [3]-[ASSAY_AND_INFRA_INTEGRATION]

| [INDEX] | [ITEM]                                                                                                                                                          | [PAGE#CLUSTER]                              | [STATUS] |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------ | :------- |
|   [1]   | BuildCheck `-check` on `static full` + `.editorconfig` build_check severities (BC bypasses TreatWarningsAsErrors); severity-matrix landing probe-gated on the assay test/build rail spike | AppHost/runtime-ports#WIRE_LAW             | SPIKE    |
|   [2]   | Per-verb binlogs in `splice_command` (`msbuild-{verb}.binlog`) — assay-rail plumbing probe-gated on the `splice_command` binlog-emit spike                       | AppHost/runtime-ports#WIRE_LAW             | SPIKE    |
|   [3]   | Benchmark Release-config closure on the assay rail, then remove the interim `Optimize=true` from `Rasm.Benchmarks.csproj`; lands when the BenchmarkDotNet rail (Compute + Persistence) is first-classed | Compute/receipts-and-benchmarks#BENCHMARK_CLAIMS | SPIKE |
|   [4]   | Classification-flag latent blocker: the four app-package test projects are `IsRhinoCommonAwareProject==FALSE`; a conditional test-package block keys on the test-project path prefix or stays scaffold-local — never an `IsComputeProject`-gated test-`PackageReference` block (it never fires) | Persistence/store-profiles#PROFILE_AXIS    | QUEUED   |
|   [5]   | Lock-skew obligation (unconditional): the four scaffold `packages.lock.json` + the `_testkit` lock predate the current central manifest; regeneration (`dotnet restore --force-evaluate`; `--locked-mode` clean) obligated for all four scaffolds + `_testkit` | AppHost/runtime-ports#WIRE_LAW             | QUEUED   |
|   [6]   | Assay WIRED-ALREADY / NEEDS-ROW verdict: the four test projects are WIRED via `--target`; the `--all` arm unions `settings.test_target` with the changed-closure set; the single candidate edit is widening `settings.test_target` if `--all` must baseline the four — no `catalog.py` row | AppHost/runtime-ports#WIRE_LAW             | QUEUED   |
|   [7]   | `SharpFuzz` admission: NEEDED on Compute (protobuf/corridor-frame decode) + Persistence (snapshot-codec/restore-ladder decode); dedicated `tests/csharp/_fuzz` project + `PackageReference` land with the fuzz rail's first-classing; no `dotnet-tools` instrument entry | Compute/remote-lane#PROTO_VOCABULARY       | QUEUED   |
|   [8]   | `Microsoft.AspNetCore.TestHost` admission at the matched ASP.NET Core servicing line: the Compute `RemoteTransport.InProcess`/`TestServer.CreateHandler` seam needs `TestServer`/`CreateHandler`/`CreateClient`; `WebApplicationFactory` (`Microsoft.AspNetCore.Mvc.Testing`) is deferred | Compute/remote-lane#TRANSPORT_AXIS         | QUEUED   |
|   [9]   | `NodaTime.Testing` pin-addition: the AppHost `FakeClock`/`FromUtc` deterministic-clock seam exists but the central pin + lock entry did not; the Test Stack pin lands and the AppHost lock regenerates | AppHost/time-and-deadlines#CLOCK_SPLIT     | QUEUED   |
|  [10]   | `NodaTime.Serialization.Protobuf` deferred — gated on the `Google.Api.CommonProtos` transitive check, a production wire-codec pin outside test/tool scope                                              | Compute/receipts-and-benchmarks#WIRE_STAMPS | BLOCKED  |

## [4]-[CROSS_FOLDER_CONSEQUENCES]

| [INDEX] | [ITEM]                                                                                                                              | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------- | :------- |
|   [1]   | Rasm.Rhino HostAttachPort registration publishes exactly: `HostUtils.RunningInDarkMode`, `Rhino.UI.ThemeSettings.ThemeChanged`, `AppearanceSettings.LanguageIdentifier` | AppUi/surface-hosts#HOST_AXIS           | SPIKE    |
|   [2]   | DocumentTransaction typed receipt ↔ DocumentService proto response field-for-field parity verification                              | Compute/remote-lane#PROTO_VOCABULARY    | SPIKE    |
|   [3]   | DATAS GC knobs stay claim-gated behind a losing benchmark                                                                          | AppHost/host-profiles#PROFILE_AXIS      | BLOCKED  |
|   [4]   | Package atlas anchor-column header harmonization on `[ANCHORS]` (cosmetic)                                                          | AppHost/runtime-ports#PORT_RECORDS      | QUEUED   |

## [5]-[CLOSE_OUT_SPIKES]

The cross-folder boot/drain/wire choreography no single folder owns, closed against the running integrated host as the final implementation-start gate. Each per-folder tier-3 host-bridge probe is named on its own package `TASKLOG.md`; these run only after every other section is done.

| [INDEX] | [ITEM]                                                                                                                                                                                                                                                              | [PAGE#CLUSTER]                          | [STATUS] |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------- | :------- |
|   [1]   | In-host Generic Host boot + cooperative-then-forced drain across all four packages in one live RhinoWIP/GH2 plugin ALC without process exit: AppHost `ProfileBoot`/`FAULT_SPINE` conducts, Persistence store lease + migration gate, Compute ORT dylib load + pool teardown, AppUi `SurfaceHost` NSView embed + host-shared `GRContext` lease through `EMBED_CAPSULE` all boot and drain in one eviction cycle | AppHost/lifecycle-and-drain#DRAIN_CONDUCTOR | SPIKE    |
|   [2]   | Cross-process wire choreography live: `ReceiptSinkPort` HLC fan-in + `TenantContext` threading + W3C `TraceContext` traceparent over the UDS/HTTP2 hop carry one correlation across AppHost (mint) → Compute (solve/generate rpc) → Persistence (op-log + RLS) → AppUi (evidence join), correlation + tenant preserved end-to-end | AppHost/diagnostics-and-telemetry#CORRELATION_SPINE | SPIKE    |
|   [3]   | DocumentTransaction typed receipt ↔ DocumentService proto response field-for-field parity confirmed live, the Rasm.Rhino host-seam sibling committing a Compute solve result into the live document at the grid edge | AppUi/tables-hierarchy#GRID_COMMIT      | SPIKE    |
