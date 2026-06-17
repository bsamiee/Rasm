# [CSHARP_BRANCH]

The C# library branch is the Rhino9(WIP)/GH2-aware AEC platform organized into strata: the `Rasm` geometry/numeric kernel, the host-neutral AEC-domain packages `Rasm.Materials`/`Rasm.Bim`/`Rasm.Fabrication`, and the app-platform packages `Rasm.AppHost`/`Rasm.Compute`/`Rasm.Persistence`/`Rasm.AppUi`. The host-boundary packages `Rasm.Rhino` and `Rasm.Grasshopper` are source-only: they floor on the RhinoCommon HintPath and stay outside the planning standardization, entering only at the future app roots that compose a live host. The full hierarchy law is `libs/.planning/architecture.md`. This charter carries the C#-internal cross-package topology and the one branch-wide test policy; cross-language facts route to the Tier-0 seam ledger.

## [1]-[PACKAGE_TOPOLOGY]

| [INDEX] | [STRATUM]     | [PACKAGE]          | [ROLE]                                                                                     |
| :-----: | :------------ | :----------------- | :---------------------------------------------------------------------------------------- |
|   [1]   | KERNEL        | `Rasm`             | RhinoCommon-aware geometry/numeric kernel: Vectors/Analysis/Domain + Geometry/ robust-core (predicates, spatial index, topology, healing, constraints) |
|   [2]   | AEC-DOMAIN    | `Rasm.Materials`   | host-neutral profiles + appearance + construction: `Profiles/` families, `Appearance/` BSDF/spectral/photometric, `Construction/` elements→assemblies→layout |
|   [3]   | AEC-DOMAIN    | `Rasm.Bim`         | host-neutral BIM: object model + IFC/glTF/STEP exchange codec + element-set/classification/assembly |
|   [4]   | AEC-DOMAIN    | `Rasm.Fabrication` | host-neutral fabrication: portable HLR/CAM/nesting frontier composing the kernel           |
|   [5]   | APP-PLATFORM  | `Rasm.AppHost`     | runtime spine: host profiles, lifecycle/drain, ports, telemetry, outbound, capability      |
|   [6]   | APP-PLATFORM  | `Rasm.Compute`     | measured execution: tensors, numeric solve, model lane, remote-lane wire, tessellation/identity |
|   [7]   | APP-PLATFORM  | `Rasm.Persistence` | durable stores: profiles, query rail, snapshots, sync, cache indexes, server tier          |
|   [8]   | APP-PLATFORM  | `Rasm.AppUi`       | Avalonia product UI: surfaces, shell, screens, commands, charts, visuals, diagnostics       |

`Rasm` is RhinoCommon-aware (the `Rhino.Geometry` global usings on the kernel) and is referenced by every higher stratum. The host split is sharp: `Rasm.Rhino`/`Rasm.Grasshopper` own the live-host native APIs, reference only `Rasm`, and are admitted only at the future app roots — never as an interior dependency of a host-neutral package, and no longer referenced by `Rasm.AppUi`. The AEC-domain packages compose the kernel and are host-neutral; the app-platform consumes AEC-domain capability at minimal one-directional boundaries. `Rasm.Bim` consumes the shared content-identity seed and the tessellation rail at the Compute seam as settled vocabulary, never through a project reference (strata-upward).

## [2]-[TEST_POLICY]

The universal rails are identical across the four app packages — same owner, same resolved member — and are carried once here. Each package then adds its package-specific rails. Versions live in `Directory.Packages.props` and `.config/dotnet-tools.json`; this policy names owners and members, never pins.

[UNIVERSAL_RAILS] (carried once, applied to AppHost/AppUi/Compute/Persistence):

| [INDEX] | [RAIL]              | [OWNER]                                  | [ROUTE]                  |
| :-----: | :------------------ | :--------------------------------------- | :----------------------- |
|   [1]   | managed law         | xUnit v3                                 | `managed-laws.md [1]`    |
|   [2]   | property-based test | CsCheck                                  | `managed-laws.md [3]`    |
|   [3]   | coverage            | coverlet.MTP                             | `evidence-rails.md [1]`  |
|   [4]   | mutation            | dotnet-stryker                           | `evidence-rails.md [2]`  |
|   [5]   | snapshot            | Verify.XunitV3                           | `evidence-rails.md [3]`  |
|   [6]   | architecture        | ArchUnitNET                              | `specialized-rails.md [1]`|

[PACKAGE_RAILS]:

| [INDEX] | [PACKAGE]          | [RAILS]                                                                                                                                              | [N/A]                       |
| :-----: | :----------------- | :------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------- |
|   [1]   | `Rasm.AppHost`     | telemetry-seam law (`MetricCollector<T>`/`FakeLogCollector`); host/runtime deterministic-clock scenarios (`FakeTimeProvider`/`FakeClock`)            | BenchmarkDotNet, SharpFuzz  |
|   [2]   | `Rasm.AppUi`       | xUnit headless (`[AvaloniaFact]`/`[AvaloniaTheory]`); headless render-hash scenarios (`HeadlessUnitTestSession`/`CaptureRenderedFrame`/`SetRenderScaling`) | BenchmarkDotNet, SharpFuzz  |
|   [3]   | `Rasm.Compute`     | BenchmarkDotNet (TensorPrimitives kernels, `MemoryDiagnoser`); SharpFuzz (protobuf decode `Fuzzer.OutOfProcess.Run`); host/runtime (`TestServer` InProcess via `Microsoft.AspNetCore.TestHost`) | —                           |
|   [4]   | `Rasm.Persistence` | BenchmarkDotNet (bulk-write/codec/DuckDB); SharpFuzz (snapshot-codec decode `WithSecurity(UntrustedData)`); host/runtime (`StoreProfile.SqliteMemory` + DuckDB live engine) | —                           |

The deterministic test-host concert binds the four rails together: the test-host profile, `SqliteMemory` placement, `InProcess` transport, headless surface, and fake-clock pair boot one in-process suite. `Rasm`, `Rasm.Materials`, `Rasm.Bim`, and `Rasm.Fabrication` carry the universal managed/PBT/coverage rails per their own package proof gates and add no host/runtime or fuzz rail.

## [3]-[ENTRY]

The campaign method and the authoring standard for the entire planning corpus live in Tier 0: `libs/.planning/campaign-method.md` (the spike-resolution machinery, tiered probe ladder) and `libs/.planning/README.md` (doc-set tiers, column schemas, page#cluster notation, signature law, language law, ledger protocol). The branch ledgers are `region-map/seam-splits.md` (intra-branch cross-folder seams) and `region-map/signature-regions.md` (the C# cross-folder signature-key region ledger); the per-package owner registries live in each package `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Catalogue governance is `api-catalogues.md`.
