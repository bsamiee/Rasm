# [SUITE_TASKLOG]

Cross-folder and infrastructure open work no single folder owns; per-folder open work lives in each package TASKLOG.md; closed items do not appear.

## [1]-[NATIVE_AND_SERVER_PROBES]

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | Deploy assets at first server app root: postgresql.conf preload fragments, pg_hba fragments, role grants; PostGIS 3.6.1+ server-side enablement | Persistence ROADMAP + first app root |

## [2]-[ADMISSION_DECISIONS_PENDING]

| [INDEX] | [ITEM] | [GATE] |
| :-----: | ------ | ------ |
| [1] | SharpGLTF / Xbim interchange | named interchange need (Rasm.Rhino owns host formats today) |
| [2] | Avalonia.Win32.Interoperability (Win32 embed route) | Windows panel-host spike; consumed by AppUi surface-hosts#WIN32_ROUTE |
| [3] | Dock.Serializer.* package naming | first app-root layout persistence (AppUi DockSerializer round-trip spec) |
| [4] | Microsoft.Extensions.Options.ConfigurationExtensions central-pin policy (directly-invoked lock-pinned transitive) | repo policy decision |

## [3]-[ASSAY_AND_INFRA_INTEGRATION]

| [INDEX] | [ITEM] | [DISPOSITION] |
| :-----: | ------ | ------------- |
| [1] | BuildCheck `-check` on `static full` + `.editorconfig` build_check severities (BC bypasses TreatWarningsAsErrors) | KEEP-OPEN(SPIKE) — the assay test/build rail is 5b-owned; the BuildCheck severity-matrix landing is probe-gated on that rail's spike. |
| [2] | Per-verb binlogs in `splice_command` (`msbuild-{verb}.binlog`) | KEEP-OPEN(SPIKE) — assay-rail plumbing on the 5b-owned assay surface; probe-gated on the `splice_command` binlog-emit spike. |
| [3] | Benchmark Release-config closure on the assay rail, then remove the interim `Optimize=true` from Rasm.Benchmarks.csproj | KEEP-OPEN(SPIKE) — 5b NEEDS the BenchmarkDotNet rail (Compute + Persistence); Release-config closure on `tests/csharp/_benchmarks` is the rail's realization probe; the `Optimize=true` removal lands when the benchmark rail is first-classed. |

New finding rows carried forward (Phase-1c-continuous + Phase-5 ownership):

| [INDEX] | [FINDING] | [VERDICT] |
| :-----: | --------- | --------- |
| [4] | Classification-flag latent blocker | All four new app-package test projects are `IsRhinoCommonAwareProject==FALSE` (host-neutral), including AppUi: `IsAppUiProject` keys on the `Rasm.AppUi/` production root, which the `tests/csharp/libs/Rasm.AppUi/` test project does not prefix-match. `IsComputeProject`/`IsPersistenceProject`/`IsAppHostProject`/`IsAppUiProject` are referenced only in their own self-definitions — no consumer block keys on any of them. Any conditional test-package block keys on the test-project path (a new `TestsRasm<Pkg>Root` prefix) or a new test-classification flag, or stays scaffold-local (`PrivateAssets="all"`, as AppHost / AppUi already do). Never author an `IsComputeProject`-gated test-`PackageReference` block — it would silently never fire. |
| [5] | Lock-skew obligation (unconditional) | The four scaffold `packages.lock.json` + the `_testkit` lock predate the current central manifest; regeneration (`dotnet restore --force-evaluate`; `--locked-mode` clean) is obligated for all four scaffolds + `_testkit` regardless of whether 5b adds a pin to that specific project. |
| [6] | Assay WIRED-ALREADY / NEEDS-ROW verdict | The four new test projects are WIRED-ALREADY via `--target` (the `_select` `(_, Path() as target, _)` arm). The `(_, None, True)` arm unions `settings.test_target` (single `Rasm.Tests.csproj`) with the changed-closure projects, so the four are not reached by `--all` absent a source change. The single authorized candidate edit is widening `settings.test_target` only if `--all` must baseline the four — a policy decision, not a wiring gap. No `catalog.py` row is needed. |
| [7] | `SharpFuzz` assay verdict | NEEDS-ADMISSION. Fuzz is NEEDED on Compute (protobuf / corridor-frame decode) + Persistence (snapshot-codec / restore-ladder decode) per `specialized-rails.md [3]`. Pinned to the Test Stack at its newest stable; the dedicated `tests/csharp/_fuzz` project and `PackageReference` wiring land with the fuzz rail's first-classing. No `dotnet-tools` instrument-CLI entry needed this phase. |
| [8] | `Microsoft.AspNetCore.TestHost` assay verdict | NEEDS-ADMISSION at the matched ASP.NET Core servicing line (the `Microsoft.Extensions` servicing line). The Compute `RemoteTransport.InProcess` / `TestServer.CreateHandler` seam needs `TestServer` / `CreateHandler` / `CreateClient`. `WebApplicationFactory` is a separate package (`Microsoft.AspNetCore.Mvc.Testing`) — RESEARCH/deferred. |
| [9] | `NodaTime.Testing` doc-truth-drift | PIN-ADDITION (preferred over charter-correction). The AppHost charter row + the `api-testing-seams.md` `FakeClock` / `FromUtc` seam exist; the central pin and lock entry did not. The seam is an authored consuming surface; the deterministic-clock seam exercises it. The Test Stack pin lands and the AppHost lock regenerates. |
| [10] | `NodaTime.Serialization.Protobuf` | RESEARCH/deferred — not a 5b admission. The Compute `[CATALOGUE_PENDING]` row gates it on the `Google.Api.CommonProtos` transitive check — a production wire-codec pin, outside 5b test/tool scope. |
| [11] | `dotnet-tools.json` NO-GAP | Proven NO-GAP. Mutation (`dotnet-stryker`), EF round-trip (`dotnet-ef`), diagnostic-dump (`dotnet-gcdump` / `dotnet-counters`), decompile (`ilspycmd`), outdated audit (`dotnet-outdated-tool`) all present; coverage / benchmark / fuzz are PackageReferences, not CLI tools. No `NEEDS-TOOL`. |

Host-neutral testkit-split (future implementation session, no `.cs` authored this phase):

| [INDEX] | [ITEM] |
| :-----: | ------ |
| [12] | `[TESTKIT_HOST_NEUTRAL_SPLIT]` — a host-neutral spec / PBT / Verify compilation home. Transferable (host-neutral-by-signature, must move): from `Spec.cs` — `Spec.ForAll`, `Spec.Metamorphic`, `Spec.Regression`, `Spec.ModelBased`, `Spec.MetamorphicOps` plus the generic LanguageExt-rail assertions and smart-enum / value-object law rails; from `Approx.cs` — the `Tolerance` value type and the `double` / `Complex` / `Arr<double>` / `Seq<double>` `Approx.Equal` overloads; from `Gens.cs` — the generic collection / scalar / rail generators; from `Numeric.cs` — the `VectorMatrix` / `Arr<double>` numeric oracles. `Tolerance.Default` / `Tolerance.FromContext` re-base off `RhinoMath` / `Context` onto a host-neutral default. Rhino-only (must NOT transfer; floors on the RhinoCommon HintPath): `HostBundle`, `GeometrySerializer`, `ContextFixture`, the `RhinoScenarioAttribute` / `ScenarioContext` / `DocumentScope` bridge rail, `Spec.MetamorphicTranslation`, and the `Point3d` / `Vector3d` / `Plane` / `Transform` overloads co-located inside `Spec.cs` / `Gens.cs` / `Numeric.cs` / `Approx.cs`. `Directory.Build.props` change (future session): the new host-neutral home classifies so its directory does not match the `TestKitRoot` arm of `IsRhinoCommonAwareProject` (so the `RhinoWIP Libraries` `<Reference Include="RhinoCommon">` group does not fire) and is excluded from the `NeedsSystemDrawingCompilePackage` `IsTestKitProject` clause. No condition change is required on the four consuming test projects — they are already `IsRhinoCommonAwareProject==FALSE`. Paired Phase-7 row: author the `.api` testing-seam pages for AppUi / Compute / Persistence (the seam pages the charter sub-tables point to as `catalogue pending`). |

## [4]-[CROSS_FOLDER_CONSEQUENCES]

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | Rasm.Rhino HostAttachPort registration publishes exactly: HostUtils.RunningInDarkMode, Rhino.UI.ThemeSettings.ThemeChanged, AppearanceSettings.LanguageIdentifier | Rasm.Rhino (consequence side of the host-attach seam) |
| [2] | DocumentTransaction typed receipt ↔ DocumentService proto response field-for-field parity verification | Rasm.Rhino + Compute remote-lane at proto authoring |
| [3] | DATAS GC knobs stay claim-gated behind a losing benchmark | AppHost host-profiles at benchmark lane |
| [4] | Package atlas anchor-column header harmonization on [ANCHORS] (cosmetic) | suite root |

## [5]-[TS_CAMPAIGN]

Entry: `libs/typescript/.planning/README.md`.

| [INDEX] | [ITEM] |
| :-----: | ------ |
| [1] | Stage A: root TS infra finalization (catalog refresh to newest stable incl. TS 7, dependency-usage truth, lock law, engine pins) |
| [2] | Stage B: lib scaffolding as a real workspace package, Effect rails, wire-contract-only integration |
| [3] | Stage C: TS dependency catalogue extraction (the .api equivalent) |
| [4] | Stage D: planning corpus completion to the suite review-law bar with a TS region ledger; register the five TS service owners (WireClients, SnapshotFeed, RuntimeFeed, CommandGateway, EvidenceFeed) |
| [5] | Pin @bufbuild/buf + remaining unpinned rows at catalogue truth; the connect peer set moves in one resolve |
| [6] | Phase-6 wire-drift reconcile from the Phase-1 deepening (all additive-tolerable per `wire-consumption.md` §4, none breaking): (a) Compute remote-lane `RemoteTransport` widened to six rows incl. designed-only `NamedPipe`/`TcpLoopback` — `TransportCapabilityWire` enumerates more transport rows but the browser surface stays grpc-web unary + server-stream only, both byte paths structurally absent in the browser; (b) the Compute receipts `Selection` case payload gained a warm-affinity flag — one additive JSON member on the Selection case interface; (c) AppHost `HealthEntryWire`/`DegradationWire` unaffected by the new `PressurePolicy` container-limit columns (internal policy, the limit grade folds into the existing status field); (d) AppUi telemetry contribution (`AppUiTelemetry`) crosses no wire per the inventory — confirm at closeout. No TS authored here. |
| [7] | TS-consumption absorption of this turn's C# wire surfaces (post-Gate-3 transcription track): the four new C# pages (numeric-lane, remote-stores, server-tier, custom-visuals) each declared their wire posture this turn — all four are host-local and author no `#TS_PROJECTION`, so the browser-dialable surface this turn mints stays exactly the existing remote-lane `Solve`/`Generate`/`GraphDiff`/`SubtreeFetch` `ComputeServiceShape` MethodShapes plus the `TenantContext`→`TenantContextWire` and W3C `TraceContext` primitives owned at `AppHost/runtime-ports#TS_PROJECTION` and `AppHost/diagnostics-and-telemetry#CORRELATION_SPINE`. `libs/typescript/.planning/wire-consumption.md` `[1]-[CONTRACT_INVENTORY]` + `wire-contracts.md` decode rails absorb the four MethodShapes + `TenantContextWire` (tenant-scoped read partition) + `TraceContext` (traceparent metadata read); no new TS contract surface beyond these. Confirm at closeout that the four host-local pages remain absent from any WIRE_PAGES set. |

## [6]-[PLANNING_CLOSE_OUT_SPIKES]

The total planning-phase close-out: these bridge-proofed spikes run ONLY after every other TASKLOG section is done — including rows added later — as the final gate before implementation. They are not per-folder work and never run incrementally while planning is still moving. Each per-folder member of the residual tier-3 host-bridge set is named on its own folder TASKLOG; this section records only the cross-folder boot/drain/wire choreography that no single folder owns and that closes against the running integrated host as the final implementation-start gate.

| [INDEX] | [ITEM] | [SPANS] | [CLOSURE_ENV] |
| :-----: | ------ | ------- | ------------- |
| [1] | In-host Generic Host boot + cooperative-then-forced drain choreography across all four packages in one live RhinoWIP/GH2 plugin ALC without process exit: AppHost `ProfileBoot`/`FAULT_SPINE` conducts, Persistence store lease + migration gate, Compute ORT dylib load + pool teardown, AppUi `SurfaceHost` NSView embed all boot and drain in the one load-context eviction cycle | AppHost + Persistence + Compute + AppUi | live RhinoWIP/GH2 plugin ALC; `bridge verify --pattern host_boot_drain` after every per-folder bridge spike passes |
| [2] | Cross-process wire choreography rehearsed live: the `ReceiptSinkPort` HLC fan-in + `TenantContext` threading + W3C `TraceContext` traceparent propagation over the UDS/HTTP2 hop carry one correlation across AppHost (mint) → Compute (solve/generate rpc) → Persistence (op-log + RLS) → AppUi (evidence join) with the boot-minted correlation id and tenant identity preserved end-to-end | AppHost + Compute + Persistence + AppUi | paired + companion topologies inside the running integrated host; correlation/tenant drill-down join confirmed against multi-process receipts |
| [3] | DocumentTransaction typed receipt ↔ DocumentService proto response field-for-field parity confirmed live, with the Rasm.Rhino host-seam sibling committing a Compute solve result into the live document at the grid edge | Compute + Rasm.Rhino + AppUi | live Rhino host + UDS attach; proto round-trip parity verified at the live grid commit |
