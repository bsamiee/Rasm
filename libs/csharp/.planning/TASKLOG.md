# [SUITE_TASKLOG]

Every remaining open item across all ledgers, agent findings, and ROADMAP gates — the suite-level index of work not closed by the planning campaign. Per-folder detail lives in each `ROADMAP.md` (START_GATES / RESEARCH_PROBES tables); this log aggregates and adds the cross-folder and infrastructure rows no single folder owns. Closed items do not appear.

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
| [6] | Microsoft.AspNetCore.TestHost (InProcess transport row helper) | Compute spec lane need |
| [7] | Microsoft.Extensions.Options.ConfigurationExtensions central-pin policy (directly-invoked lock-pinned transitive) | repo policy decision |
| [8] | dotnet-counters/trace/gcdump tool-manifest admission (9.0.661903 line, re-verify) | AppHost implementation start |
| [9] | TimescaleDB / pg_partman / pg_repack / HypoPG / pgaudit operator provisioning | self-provisioned server era |

## [4]-[ASSAY_AND_INFRA_INTEGRATION] (assay changes are their own work, not page edits)

| [INDEX] | [ITEM] |
| :-----: | ------ |
| [1] | `test run --target` rows for the four new test projects |
| [2] | CrashDump/HangDump argv on the assay test rail (packages already referenced, dormant) |
| [3] | BuildCheck `-check` on `static full` + `.editorconfig` build_check severities (BC bypasses TreatWarningsAsErrors) |
| [4] | Per-verb binlogs in `splice_command` (`msbuild-{verb}.binlog`) |
| [5] | Register transitive packages in the assay api source map: Grpc.Core.Api, NetTopologySuite core, FluentIcons.Common, TextMateSharp (restores direct decompile routes) |
| [6] | Benchmark Release-config closure on the assay rail, then remove the interim `Optimize=true` from Rasm.Benchmarks.csproj |
| [7] | api catalogue deepening: api-dynamicdata.md operator rows (TransformToTree/Group/combinators); EmbeddableControlRoot family rows in api-avalonia.md; svg hit/animation rows in api-svg-skia.md |
| [8] | DOTNET_* env triple replication into CI when a CI executor lands; lock-law note already in NuGet.config/global.json |
| [9] | coverlet.MTP NuGet listing stability watch at next admission sweep |

## [5]-[CROSS_FOLDER_CONSEQUENCES]

| [INDEX] | [ITEM] | [OWNER] |
| :-----: | ------ | ------- |
| [1] | Rasm.Rhino HostAttachPort registration publishes exactly: HostUtils.RunningInDarkMode, Rhino.UI.ThemeSettings.ThemeChanged, AppearanceSettings.LanguageIdentifier | Rasm.Rhino (consequence side of the host-attach seam) |
| [2] | DocumentTransaction typed receipt ↔ DocumentService proto response field-for-field parity verification | Rasm.Rhino + Compute remote-lane at proto authoring |
| [3] | DATAS GC knobs stay claim-gated behind a losing benchmark | AppHost host-profiles at benchmark lane |
| [4] | Package atlas anchor-column header harmonization on [ANCHORS] (cosmetic) | suite root |

## [6]-[TS_CAMPAIGN] (entry: `libs/typescript/.planning/README.md`)

| [INDEX] | [ITEM] |
| :-----: | ------ |
| [1] | Stage A: root TS infra finalization (catalog refresh to newest stable incl. TS 7, knip truth, lock law, engine pins) |
| [2] | Stage B: lib scaffolding as a real workspace package, Effect rails, wire-contract-only integration |
| [3] | Stage C: TS dependency catalogue extraction (the .api equivalent) |
| [4] | Stage D: planning corpus completion to the suite review-law bar with a TS region ledger; register the five TS service owners (WireClients, SnapshotFeed, RuntimeFeed, CommandGateway, EvidenceFeed) |
| [5] | Pin @bufbuild/buf + remaining unpinned rows at catalogue truth; the connect peer set moves in one resolve |

## [7]-[PLANNING_CLOSE_OUT_SPIKES]

The total planning-phase close-out: these bridge-proofed spikes run ONLY after every other TASKLOG section is done — including rows added by later sessions — as the final gate before implementation. They are not per-folder work and never run incrementally while planning is still moving.

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

