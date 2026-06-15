# [SUITE_TASKLOG]

Every remaining open item across all ledgers and ROADMAP gates — the suite-level index of work not closed by the planning campaign. Per-folder detail lives in each `ROADMAP.md`; this log aggregates and adds the cross-folder and infrastructure rows no single folder owns. Closed items do not appear.

## [1]-[NATIVE_AND_SERVER_PROBES]

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | vec0 sourcing decision (package payload vs vendored tarball) + live-load proof | Persistence ROADMAP |
| [2] | SQLCipher provider route with external dylib on osx-arm64 | Persistence ROADMAP |
| [3] | sqlean per-RID vendoring as build content | Persistence ROADMAP |
| [4] | MergeWithOutput RETURNING old/new emission on the pg provider | Persistence ROADMAP |
| [5] | Live-PG18 probes: publish_generated_columns, idle slot timeout, subscription conflict stats, pgaudit categories | Persistence ROADMAP |
| [6] | Two-process first-open race (racing MigrateAsync + busy_timeout, one WAL file) | Persistence ROADMAP |
| [7] | DuckDB sqlite_scanner ATTACH snapshot visibility under a concurrent WAL writer | Persistence ROADMAP |
| [8] | uuidv7 double-generation precedence (client CreateVersion7 vs pg column default) | Persistence ROADMAP |
| [9] | APFS rename durability without directory fsync; migration-lock holder evidence | Persistence ROADMAP |
| [10] | Deploy assets at first server app root: postgresql.conf preload fragments, pg_hba fragments, role grants; PostGIS 3.6.1+ server-side enablement | Persistence ROADMAP + first app root |
| [11] | SIGHUP delivery under launchd/systemd; standalone crash-marker schema; macOS keychain secrets-store route | AppHost ROADMAP |
| [12] | UDS peer-credential read (LOCAL_PEERCRED) admission probe | AppHost ROADMAP |
| [13] | CoreML option value domains (MLComputeUnits, SpecializationStrategy); Terminate-latch latency cadence | Compute ROADMAP |
| [14] | GH2 async result readback ceiling (solver idle-loop) | Compute ROADMAP |

## [2]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | Zero-alloc stream-pool eleven-event fold (Check.Faster); drop-callback allocation profile | Compute |
| [2] | Progress no-regress race (CsCheck SampleParallel over concurrent Advance) | Compute |
| [3] | ComputeWireContext + suite Strict resolver-merge round-trip over Thinktecture key scalars | Compute |
| [4] | DockSerializer round-trip preserving dockable identity; render-hash lanes; AvaloniaFact-under-MTP dispatch spike | AppUi |
| [5] | Compile-options receipt + Batteries_V2 round-trip under the bundle-line override; snapshot bracket preconditions | Persistence |
| [6] | NodaTime converter precedence over combined source-gen metadata in the Strict merge | AppHost |
| [7] | LiveCharts net8-asset render fidelity on Avalonia 12; heat-land geo payload | AppUi |
| [8] | UnitsNet next-major QuantityInfo reshape staged-restore check | Compute |

## [3]-[ADMISSION_DECISIONS_PENDING]

| [INDEX] | [ITEM] | [GATE] |
| :-----: | ------ | ------ |
| [1] | Velopack (vpk) — UpdatePort vehicle | standalone notarization spike |
| [2] | SharpGLTF / Xbim interchange | named interchange need (Rasm.Rhino owns host formats today) |
| [3] | Avalonia.Win32.Interoperability (Win32 embed route) | Windows panel-host spike |
| [4] | Dock.Serializer.* package naming | first app-root layout persistence |
| [5] | Microsoft.Extensions.Caching.StackExchangeRedis (web L2) | first web app root |
| [6] | Microsoft.AspNetCore.TestHost (InProcess transport row helper) | folded to Compute remote-lane RESEARCH [TRANSPORTS]; lands as test-only PackageReference in the spec project at the matched ASP.NET Core servicing line |
| [7] | Microsoft.Extensions.Options.ConfigurationExtensions central-pin policy (directly-invoked lock-pinned transitive) | repo policy decision |
| [10] | Microsoft.ML.OnnxRuntime.Gpu / .DirectML (Cuda/DirectMl EP registration members) | Windows-profile implementation; folded to Compute model-lane RESEARCH [EP_OPTIONS] as designed-only rows |
| [11] | Microsoft.Extensions.Compliance.Redaction registration surface (AddRedaction/SetHmacRedactor/SetRedactor/SetFallbackRedactor) | AppHost diagnostics; EXTEXP0002 experimental gate; folded to RESEARCH [REDACTION_REGISTRATION] |
| [8] | dotnet-counters/trace/gcdump tool-manifest admission (9.0.661903 line, re-verify) | AppHost implementation start |
| [9] | TimescaleDB / pg_partman / pg_repack / HypoPG / pgaudit operator provisioning | self-provisioned server era |

## [4]-[ASSAY_AND_INFRA_INTEGRATION]

Assay changes are their own work, not page edits.

| [INDEX] | [ITEM] | [DISPOSITION] |
| :-----: | ------ | ------------- |
| [1] | `test run --target` rows for the four new test projects | CLOSE — RESOLVED-not-authored: the `_select` `(_, Path() as target, _)` arm already pins any one via `--target <path>`; no `catalog.py` row minted. The only open sub-question (`--all` baselining the four) is a `settings.test_target` policy decision, not a wiring gap. |
| [2] | CrashDump/HangDump argv on the assay test rail (packages already referenced, dormant) | CLOSE — RESOLVED-not-authored: the dump packages are already referenced in the `Directory.Build.props` Test Runner SDK group; the argv home is the assay test rail (`test.py` `_dispatch_all`), not `Build.targets`. |
| [3] | BuildCheck `-check` on `static full` + `.editorconfig` build_check severities (BC bypasses TreatWarningsAsErrors) | KEEP-OPEN(SPIKE) — the assay test/build rail is 5b-owned; the BuildCheck severity-matrix landing is probe-gated on that rail's spike. |
| [4] | Per-verb binlogs in `splice_command` (`msbuild-{verb}.binlog`) | KEEP-OPEN(SPIKE) — assay-rail plumbing on the 5b-owned assay surface; probe-gated on the `splice_command` binlog-emit spike. |
| [5] | Register transitive packages in the assay api source map: Grpc.Core.Api, NetTopologySuite core, FluentIcons.Common, TextMateSharp (restores direct decompile routes) | OUT-OF-SCOPE→Phase 7 — `.api` source-map / catalogue deepening is Phase-7 catalogue-authoring; production-transitive registration, not a test pin. |
| [6] | Benchmark Release-config closure on the assay rail, then remove the interim `Optimize=true` from Rasm.Benchmarks.csproj | KEEP-OPEN(SPIKE) — 5b NEEDS the BenchmarkDotNet rail (Compute + Persistence); Release-config closure on `tests/csharp/_benchmarks` is the rail's realization probe; the `Optimize=true` removal lands when the benchmark rail is first-classed. |
| [7] | api catalogue deepening: api-dynamicdata.md operator rows (TransformToTree/Group/combinators); EmbeddableControlRoot family rows in api-avalonia.md; svg hit/animation rows in api-svg-skia.md | OUT-OF-SCOPE→Phase 7 — `.api` catalogue authoring is Phase-7 `.api` ownership. |
| [8] | DOTNET_* env triple replication into CI when a CI executor lands; lock-law note already in NuGet.config/global.json | OUT-OF-SCOPE→out-of-campaign — gated on a CI executor landing; no 5b test-rail gate. |
| [9] | coverlet.MTP NuGet listing stability watch at next admission sweep | CLOSE — the watch executed: `coverlet.MTP` holds at its pinned listed stable; no bump. |

New finding rows carried forward (Phase-1c-continuous + Phase-5 ownership):

| [INDEX] | [FINDING] | [VERDICT] |
| :-----: | --------- | --------- |
| [10] | Classification-flag latent blocker | All four new app-package test projects are `IsRhinoCommonAwareProject==FALSE` (host-neutral), including AppUi: `IsAppUiProject` keys on the `Rasm.AppUi/` production root, which the `tests/csharp/libs/Rasm.AppUi/` test project does not prefix-match. `IsComputeProject`/`IsPersistenceProject`/`IsAppHostProject`/`IsAppUiProject` are referenced only in their own self-definitions — no consumer block keys on any of them. Any conditional test-package block keys on the test-project path (a new `TestsRasm<Pkg>Root` prefix) or a new test-classification flag, or stays scaffold-local (`PrivateAssets="all"`, as AppHost / AppUi already do). Never author an `IsComputeProject`-gated test-`PackageReference` block — it would silently never fire. |
| [11] | Lock-skew obligation (unconditional) | The four scaffold `packages.lock.json` + the `_testkit` lock predate the current central manifest; regeneration (`dotnet restore --force-evaluate`; `--locked-mode` clean) is obligated for all four scaffolds + `_testkit` regardless of whether 5b adds a pin to that specific project. |
| [12] | Assay WIRED-ALREADY / NEEDS-ROW verdict | The four new test projects are WIRED-ALREADY via `--target` (the `_select` `(_, Path() as target, _)` arm). The `(_, None, True)` arm unions `settings.test_target` (single `Rasm.Tests.csproj`) with the changed-closure projects, so the four are not reached by `--all` absent a source change. The single authorized candidate edit is widening `settings.test_target` only if `--all` must baseline the four — a policy decision, not a wiring gap. No `catalog.py` row is needed. |
| [13] | `SharpFuzz` assay verdict | NEEDS-ADMISSION. Fuzz is NEEDED on Compute (protobuf / corridor-frame decode) + Persistence (snapshot-codec / restore-ladder decode) per `specialized-rails.md [3]`. Pinned to the Test Stack at its newest stable; the dedicated `tests/csharp/_fuzz` project and `PackageReference` wiring land with the fuzz rail's first-classing. No `dotnet-tools` instrument-CLI entry needed this phase. |
| [14] | `Microsoft.AspNetCore.TestHost` assay verdict | NEEDS-ADMISSION at the matched ASP.NET Core servicing line (the `Microsoft.Extensions` 10.0.9 line). The Compute `RemoteTransport.InProcess` / `TestServer.CreateHandler` seam needs `TestServer` / `CreateHandler` / `CreateClient`. `WebApplicationFactory` is a separate package (`Microsoft.AspNetCore.Mvc.Testing`) — RESEARCH/deferred. |
| [15] | `NodaTime.Testing 3.3.2` doc-truth-drift | PIN-ADDITION (preferred over charter-correction). The AppHost charter row + the `api-testing-seams.md` `FakeClock` / `FromUtc` seam exist; the central pin and lock entry did not. The seam is an authored consuming surface; the deterministic-clock seam exercises it. The Test Stack pin lands and the AppHost lock regenerates. |
| [16] | `NodaTime.Serialization.Protobuf` | RESEARCH/deferred — not a 5b admission. The Compute `[CATALOGUE_PENDING]` row gates it on the `Google.Api.CommonProtos` transitive check — a production wire-codec pin, outside 5b test/tool scope. |
| [17] | `dotnet-tools.json` NO-GAP | Proven NO-GAP. Mutation (`dotnet-stryker`), EF round-trip (`dotnet-ef`), diagnostic-dump (`dotnet-gcdump` / `dotnet-counters`), decompile (`ilspycmd`), outdated audit (`dotnet-outdated-tool`) all present; coverage / benchmark / fuzz are PackageReferences, not CLI tools. No `NEEDS-TOOL`. |

Host-neutral testkit-split (future implementation session, no `.cs` authored this phase):

| [INDEX] | [ITEM] |
| :-----: | ------ |
| [18] | `[TESTKIT_HOST_NEUTRAL_SPLIT]` — a host-neutral spec / PBT / Verify compilation home. Transferable (host-neutral-by-signature, must move): from `Spec.cs` — `Spec.ForAll`, `Spec.Metamorphic`, `Spec.Regression`, `Spec.ModelBased`, `Spec.MetamorphicOps` plus the generic LanguageExt-rail assertions and smart-enum / value-object law rails; from `Approx.cs` — the `Tolerance` value type and the `double` / `Complex` / `Arr<double>` / `Seq<double>` `Approx.Equal` overloads; from `Gens.cs` — the generic collection / scalar / rail generators; from `Numeric.cs` — the `VectorMatrix` / `Arr<double>` numeric oracles. `Tolerance.Default` / `Tolerance.FromContext` re-base off `RhinoMath` / `Context` onto a host-neutral default. Rhino-only (must NOT transfer; floors on the RhinoCommon HintPath): `HostBundle`, `GeometrySerializer`, `ContextFixture`, the `RhinoScenarioAttribute` / `ScenarioContext` / `DocumentScope` bridge rail, `Spec.MetamorphicTranslation`, and the `Point3d` / `Vector3d` / `Plane` / `Transform` overloads co-located inside `Spec.cs` / `Gens.cs` / `Numeric.cs` / `Approx.cs`. `Directory.Build.props` change (future session): the new host-neutral home classifies so its directory does not match the `TestKitRoot` arm of `IsRhinoCommonAwareProject` (so the `RhinoWIP Libraries` `<Reference Include="RhinoCommon">` group does not fire) and is excluded from the `NeedsSystemDrawingCompilePackage` `IsTestKitProject` clause. No condition change is required on the four consuming test projects — they are already `IsRhinoCommonAwareProject==FALSE`. Paired Phase-7 row: author the `.api` testing-seam pages for AppUi / Compute / Persistence (the seam pages the charter sub-tables point to as `catalogue pending`). |

## [5]-[CROSS_FOLDER_CONSEQUENCES]

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | Rasm.Rhino HostAttachPort registration publishes exactly: HostUtils.RunningInDarkMode, Rhino.UI.ThemeSettings.ThemeChanged, AppearanceSettings.LanguageIdentifier | Rasm.Rhino (consequence side of the host-attach seam) |
| [2] | DocumentTransaction typed receipt ↔ DocumentService proto response field-for-field parity verification | Rasm.Rhino + Compute remote-lane at proto authoring |
| [3] | DATAS GC knobs stay claim-gated behind a losing benchmark | AppHost host-profiles at benchmark lane |
| [4] | Package atlas anchor-column header harmonization on [ANCHORS] (cosmetic) | suite root |

## [6]-[TS_CAMPAIGN]

Entry: `libs/typescript/.planning/README.md`.

| [INDEX] | [ITEM] |
| :-----: | ------ |
| [1] | Stage A: root TS infra finalization (catalog refresh to newest stable incl. TS 7, dependency-usage truth, lock law, engine pins) |
| [2] | Stage B: lib scaffolding as a real workspace package, Effect rails, wire-contract-only integration |
| [3] | Stage C: TS dependency catalogue extraction (the .api equivalent) |
| [4] | Stage D: planning corpus completion to the suite review-law bar with a TS region ledger; register the five TS service owners (WireClients, SnapshotFeed, RuntimeFeed, CommandGateway, EvidenceFeed) |
| [5] | Pin @bufbuild/buf + remaining unpinned rows at catalogue truth; the connect peer set moves in one resolve |
| [6] | Phase-6 wire-drift reconcile from the Phase-1 deepening (all additive-tolerable per `wire-consumption.md` §4, none breaking): (a) Compute remote-lane `RemoteTransport` widened to six rows incl. designed-only `NamedPipe`/`TcpLoopback` — `TransportCapabilityWire` enumerates more transport rows but the browser surface stays grpc-web unary + server-stream only, both byte paths structurally absent in the browser; (b) the Compute receipts `Selection` case payload gained a warm-affinity flag — one additive JSON member on the Selection case interface; (c) AppHost `HealthEntryWire`/`DegradationWire` unaffected by the new `PressurePolicy` container-limit columns (internal policy, the limit grade folds into the existing status field); (d) AppUi telemetry contribution (`AppUiTelemetry`) crosses no wire per the inventory — confirm at closeout. No TS authored here. |

## [7]-[PLANNING_CLOSE_OUT_SPIKES]

The total planning-phase close-out: these bridge-proofed spikes run ONLY after every other TASKLOG section is done — including rows added later — as the final gate before implementation. They are not per-folder work and never run incrementally while planning is still moving.

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | Generic Host boot + unload inside the RhinoWIP plugin ALC without process exit | AppHost ROADMAP |
| [2] | Kestrel + Grpc.AspNetCore hosting inside the plugin ALC (gRPC loopback from the Rhino host) | AppHost ROADMAP |
| [3] | Avalonia-12-in-Rhino NSView embedding (pump coexistence, resize sync, render-backend contention scenarios) | AppUi ROADMAP |
| [4] | ONNX dylib load inside the RhinoWIP ALC; libortextensions versioned-RID resolution | Compute ROADMAP |
| [5] | Hardened-runtime dlopen of extension dylibs inside the signed Rhino host (extension-load.verify.csx) | Persistence ROADMAP |
| [6] | Embedded-TopLevel service resolution (toasts, storage pickers) inside the rhino-panel root | AppUi ROADMAP |
| [7] | Host-object drag across the NSView boundary; VoiceOver reach across the embedding | AppUi ROADMAP |
| [8] | Drain-deadline conformance scenario under live plugin unload | AppHost ROADMAP |

## [8]-[DEEPENING_FINDINGS]

Cross-folder findings the Phase-1 deepening surfaced. Page-local RESEARCH tokens stay on the owning page's RESEARCH cluster and each folder ROADMAP; this section carries only the suite-level reconciliations and the one grounding regression that crosses folder ownership. Every owned foldable item from the deepening landed as a transcription-complete fence; the residue below is genuine probe-gated or suite-policy work, not unfolded page law.

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | AppUi input-interaction net-lost-grounding: the deepening softened the previously-fenced gesture-trigger and clipboard member spellings (`TappedEventTrigger`, `DoubleTappedEventTrigger`, `RightTappedEventTrigger`, `IPointer.Capture`, `PointerCaptureLostEventTrigger`, `ExportState`, the multi-format clipboard write) to prose and split them into three new RESEARCH rows (`[GESTURE_TRIGGERS]`, `[POINTER_CAPTURE]`, `[CLIPBOARD_WRITE]`); re-ground each against the installed Avalonia.Xaml.Behaviors and Avalonia clipboard surfaces at implementation so the routed-event-trigger family and structured clipboard write carry exact member spellings rather than prose. Surfaced as AppUi ROADMAP `[1]-START_GATES` gate [8] (`input-interaction#POINTER_GESTURES`/`#DRAG_CLIPBOARD`) — a grounding-regression frontier re-grounded at implementation, not folded page law. | AppUi ROADMAP (input-interaction) |
| [2] | Optional `NodaTime.Serialization.Protobuf` direct `<PackageVersion>` admission gated on whether `Google.Api.CommonProtos` calendar surface resolves transitively through the admitted NodaTime + Grpc graph (Compute receipts-and-benchmarks RESEARCH `[CALENDAR_BRIDGE]`; surfaced via Compute GAP_LEDGER [20] + CATALOGUE_PENDING). | Compute ROADMAP |
