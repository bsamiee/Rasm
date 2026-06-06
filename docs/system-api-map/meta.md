# [CSHARP_META]

Scope: language version, central package graph, analyzer posture, RhinoWIP/GH2 host references, generated assembly metadata, global usings. BCL surfaces: `bcl.md`. Package state: `packages.md`. Language catalog: `../external-libs/csharp/language.md`.

## [1][CONTROL_PLANES]

| [INDEX] | [FILE]                     |
| :-----: | -------------------------- |
|   [1]   | `Directory.Build.props`    |
|   [2]   | `Directory.Packages.props` |
|   [3]   | `Directory.Build.targets`  |
|   [4]   | `.editorconfig`            |
|   [5]   | `Workspace.slnx`           |
|   [6]   | `packages.lock.json`       |
|   [7]   | `global.json`              |

[OWNS]
- [1] Target framework, language version, analyzer posture, global usings, project classification, RhinoWIP/GH2 host references, generated assembly metadata.
- [2] Central package versions and transitive package state.
- [3] Late build behavior only.
- [4] Analyzer/style severity and code formatting policy.
- [5] Broad solution graph.
- [6] Locked restore graph per project.
- [7] .NET CLI runner selection for MTP tests; no SDK pin.

## [2][LANGUAGE_AND_ANALYZERS]

`TargetFramework=net10.0`, `LangVersion=14.0`, `AnalysisLevel=latest-all`, warnings-as-errors, and analyzer packages stay centralized. Prefer explicit language version over floating `latest`.

| [INDEX] | [LAYER]               | [PACKAGE / SURFACE]                                                 | [SCOPE]                               |
| :-----: | --------------------- | ------------------------------------------------------------------- | ------------------------------------- |
|   [1]   | Compile toolset       | .NET 10 SDK compiler                                                | All projects                          |
|   [2]   | Third-party analyzers | `Meziantou.Analyzer`, `Microsoft.VisualStudio.Threading.Analyzers`  | All projects except analyzer island   |
|   [3]   | Local analyzer        | `tools/cs-analyzer/CsAnalyzer.csproj`                               | Project analyzer ref; CSP#### catalog |
|   [4]   | Roslyn authoring      | `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers` | `CsAnalyzer` and tests only           |

C# 14 feature catalog: `../external-libs/csharp/language.md`. Compiler package state: `packages.md` §3. Treat analyzer failures as architecture pressure unless the owning policy explicitly permits an exception.

## [3][HOST_REFERENCES]

`Directory.Build.props` owns RhinoWIP app paths, `RhinoCommon`, `Rhino.UI`, `Grasshopper2`, `GrasshopperIO`, `Eto`, RhinoCode references, `System.Drawing.Common` host assembly, and conditioned compile package metadata. Host-reference edits use `uv run python -m tools.quality api doctor`.

## [4][GENERATED_METADATA]

Use centralized generated `InternalsVisibleTo`, plugin description attributes, platform metadata, and GUID attributes instead of scattered `AssemblyInfo.cs` files. Keep project files focused on real project identity and references.

## [5][PACKAGE_GRAPH]

Add central versions only with active consumer proof, transitive package need, or a conditioned compile surface. For graph changes, validate restore/build state and update lockfiles intentionally. Build/analyzer packages: `packages.md` §3. Local analyzer refs are not central-package entries.

## [6][GLOBAL_USINGS]

| [INDEX] | [LAYER]   | [INJECTED]                                                                                                          |
| :-----: | --------- | ------------------------------------------------------------------------------------------------------------------- |
|   [1]   | SDK       | `System`, `Collections.Generic`, `IO`, `Linq`, `Net.Http`, `Threading`, `Threading.Tasks`                           |
|   [2]   | Workspace | `LanguageExt` (+Common, Traits, Effects, Pretty, Traits.Domain); `static Prelude`; `Thinktecture`                   |
|   [3]   | GH-aware  | `Eto.Drawing`; `GH2.Types.Numeric`; `GH2.UI`; `Rasm.{Analysis,Domain}`; `Rhino.Geometry`; `Runtime.InteropServices` |
|   [4]   | Test      | `Xunit`; `CsCheck`; aliases `Dim`/`VectorMatrix` (Rhino-aware test+testkit)                                         |
|   [5]   | Project   | e.g. `Immutable`, `Frozen`, `Grasshopper2.*`, Rhino command NS                                                      |

[CONDITION]
- [1] `ImplicitUsings=enable`; row [1] omits repeated `System.` prefix on SDK implicit NS.
- [2] `UseWorkspaceLibraries=true` and not local analyzer project.
- [3] GH plugin or GH-aware project; `Runtime.InteropServices` is `System.Runtime.InteropServices`.
- [4] Test or testkit project plus `IsRhinoCommonAwareProject`.
- [5] Declared in owning `.csproj`.

**Not workspace-global:** `System.Collections.Frozen`, `System.Collections.Immutable`, `System.Text.RegularExpressions`, `System.Buffers`, `System.Threading.Channels`, `System.Text.Json`, `System.Numerics`, `System.Diagnostics.Metrics`, `Microsoft.Extensions.Logging`, ISA intrinsics subnamespaces.

**GH-aware injection:** `System.Runtime.InteropServices` and `Rasm.Analysis` are global on GH trees but not in the workspace LanguageExt block.

**Non-transitive:** Global usings from a referenced project assembly do not flow to referencers — each project declares its own `<Using>` or file-scoped imports.

`LanguageExt.UnitsOfMeasure` is never global; import explicitly when needed.

**Bridge scenarios:** compile-time globals differ from runtime scenario usings staged by `tools/rhino-bridge` — see `tools/rhino-bridge/README.md`.

## [7][USE_WORKSPACE_LIBRARIES]

| [INDEX] | [DEFAULT] | [WHEN_FALSE]                          | [SUPPRESSED]                                        |
| :-----: | --------- | ------------------------------------- | --------------------------------------------------- |
|   [1]   | `true`    | Rhino plugin, GH plugin, rhino-bridge | LanguageExt/Thinktecture packages + prelude globals |

When `UseWorkspaceLibraries=false`, host references, compiler toolset, third-party analyzers, local CsAnalyzer, and Grasshopper-aware usings block remain active.

Opt-in per `.csproj` under gated trees when a tool or plugin boundary still needs rails. Example: `tools/rhino-bridge/client` declares LanguageExt/Thinktecture directly when gated.

## [8][LOCAL_ANALYZER]

- Root: `tools/cs-analyzer`; referenced as `OutputItemType="Analyzer"` from non-exempt projects.
- Skipped on: analyzer project itself, `CsAnalyzer.Tests` (local analyzer ref only — tests still receive workspace globals and third-party analyzers).
- Analyzer project: no workspace LanguageExt/Thinktecture globals, no third-party analyzer packages, targets `netstandard2.0`.
- `tools/cs-analyzer/Contracts/BoundaryContracts.cs` compiled into non-exempt projects for CSP boundary exemptions.
- Boundary-exempt project classes: plugins, GH-aware, Rhino UI-aware, tests, testkit, rhino-bridge.
