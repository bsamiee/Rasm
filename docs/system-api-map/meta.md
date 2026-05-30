# [H1][CSHARP_META]
>**Dictum:** *C# meta files are build contracts, not incidental configuration.*

<br>

[IMPORTANT] Scope: language version, central package graph, analyzer posture, RhinoWIP/GH2 host references, generated assembly metadata, global usings. BCL surfaces: `bcl.md`. Package state: `packages.md`. Language catalog: `../external-libs/csharp/language.md`.

---
## [1][CONTROL_PLANES]
>**Dictum:** *Each root file owns one build decision layer.*

<br>

| [INDEX] | [FILE] | [OWNS] |
| :-----: | ------ | ------ |
| [1] | `Directory.Build.props` | Target framework, language version, analyzer posture, global usings, project classification, RhinoWIP/GH2 host references, generated assembly metadata. |
| [2] | `Directory.Packages.props` | Central package versions and transitive pins. |
| [3] | `Directory.Build.targets` | Late build behavior only. |
| [4] | `.editorconfig` | Analyzer/style severity and code formatting policy. |
| [5] | `Workspace.slnx` | Broad solution graph. |
| [6] | `packages.lock.json` | Locked restore graph per project. |

---
## [2][LANGUAGE_AND_ANALYZERS]
>**Dictum:** *Deterministic language and analyzer settings protect agent output.*

<br>

`TargetFramework=net10.0`, `LangVersion=14.0`, `AnalysisLevel=latest-all`, warnings-as-errors, and analyzer packages stay centralized. Prefer explicit language version over floating `latest`.

| [INDEX] | [LAYER] | [PACKAGE / SURFACE] | [SCOPE] |
| :-----: | ------- | ------------------- | ------- |
| [1] | Compile toolset | `Microsoft.Net.Compilers.Toolset` 5.3.0 | All projects |
| [2] | Third-party analyzers | `Meziantou.Analyzer`, `Microsoft.VisualStudio.Threading.Analyzers` | All projects except local analyzer project |
| [3] | Local analyzer | `tools/cs-analyzer/CsAnalyzer.csproj` | Project analyzer ref; CSP#### catalog |
| [4] | Roslyn authoring | `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers` | `CsAnalyzer` and tests only |

C# 14 feature catalog: `../external-libs/csharp/language.md`. Compiler pin state: `packages.md` §3. Treat analyzer failures as architecture pressure unless the owning policy explicitly permits an exception.

---
## [3][HOST_REFERENCES]
>**Dictum:** *RhinoWIP assemblies are host references, not generic packages.*

<br>

`Directory.Build.props` owns RhinoWIP app paths, `RhinoCommon`, `Rhino.UI`, `Grasshopper2`, `GrasshopperIO`, `Eto`, RhinoCode references, `System.Drawing.Common` host assembly, and conditioned compile package metadata. Verify host assemblies with `uv run python -m tools.quality api doctor` before changing references or host docs.

---
## [4][GENERATED_METADATA]
>**Dictum:** *MSBuild emits assembly metadata where possible.*

<br>

Use centralized generated `InternalsVisibleTo`, plugin description attributes, platform metadata, and GUID attributes instead of scattered `AssemblyInfo.cs` files. Keep project files focused on real project identity and references.

---
## [5][PACKAGE_GRAPH]
>**Dictum:** *Central pins need consumers or transitive proof.*

<br>

Add central versions only with active consumer proof, transitive pin need, or a conditioned compile surface. For graph changes, validate restore/build state and update lockfiles intentionally. Build/analyzer packages: `packages.md` §3. Local analyzer refs are not central-package entries.

---
## [6][GLOBAL_USINGS]
>**Dictum:** *Usings stack in layers; BCL owners in `bcl.md` §11 need explicit import unless a layer below covers them.*

<br>

| [INDEX] | [LAYER] | [INJECTED] | [CONDITION] |
| :-----: | ------- | ---------- | ----------- |
| [1] | SDK implicit | `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Net.Http`, `System.Threading`, `System.Threading.Tasks` | `ImplicitUsings=enable` on all projects |
| [2] | Workspace globals | `LanguageExt`, `LanguageExt.Common`, `LanguageExt.Traits`, `LanguageExt.Effects`, `LanguageExt.Pretty`, `LanguageExt.Traits.Domain`, `static LanguageExt.Prelude`, `Thinktecture` | `UseWorkspaceLibraries=true` and not local analyzer project |
| [3] | Grasshopper-aware | `Eto.Drawing`, `Grasshopper2.Types.Numeric`, `Grasshopper2.UI`, `Rasm.Analysis`, `Rasm.Domain`, `Rhino.Geometry`, `System.Runtime.InteropServices` | GH plugin or GH-aware project |
| [4] | Test | `Xunit`, `CsCheck`; vector aliases `Dim`, `VectorMatrix` on Rhino-aware tests/testkit | Test or testkit project plus `IsRhinoCommonAwareProject` |
| [5] | Per-project | e.g. `System.Collections.Immutable`, `System.Collections.Frozen`, extra `Grasshopper2.*`, Rhino command namespaces | Declared in owning `.csproj` |

**Not workspace-global:** `System.Collections.Frozen`, `System.Collections.Immutable`, `System.Text.RegularExpressions`, `System.Buffers`, `System.Threading.Channels`, `System.Text.Json`, `System.Numerics`, `System.Diagnostics.Metrics`, `Microsoft.Extensions.Logging`, ISA intrinsics subnamespaces.

**GH-aware injection:** `System.Runtime.InteropServices` and `Rasm.Analysis` are global on GH trees but not in the workspace LanguageExt block.

**Non-transitive:** Global usings from a referenced project assembly do not flow to referencers — each project declares its own `<Using>` or file-scoped imports.

`LanguageExt.UnitsOfMeasure` is never global; import explicitly when needed.

**Bridge scenarios:** compile-time globals differ from runtime scenario usings staged by `tools/rhino-bridge` — see `tools/rhino-bridge/AGENTS.md`.

---
## [7][USE_WORKSPACE_LIBRARIES]
>**Dictum:** *Plugin and bridge trees opt out of workspace library injection by default.*

<br>

| [INDEX] | [DEFAULT] | [WHEN_FALSE] | [SUPPRESSED] | [STILL_ACTIVE] |
| :-----: | --------- | ------------ | ------------ | -------------- |
| [1] | `true` | Rhino plugin, Grasshopper plugin, rhino-bridge projects | Auto `LanguageExt`/`Thinktecture` packages and prelude globals | Host references, compiler toolset, third-party analyzers, local CsAnalyzer, Grasshopper-aware usings block |

Opt-in per `.csproj` under gated trees when a tool or plugin boundary still needs rails. Example: `tools/rhino-bridge/client` declares LanguageExt/Thinktecture directly when gated.

---
## [8][LOCAL_ANALYZER]
>**Dictum:** *The analyzer project is an isolated build island.*

<br>

- Root: `tools/cs-analyzer`; referenced as `OutputItemType="Analyzer"` from non-exempt projects.
- Skipped on: analyzer project itself, `CsAnalyzer.Tests` (local analyzer ref only — tests still receive workspace globals and third-party analyzers).
- Analyzer project: no workspace LanguageExt/Thinktecture globals, no third-party analyzer packages, targets `netstandard2.0`.
- `tools/cs-analyzer/Contracts/BoundaryContracts.cs` compiled into non-exempt projects for CSP boundary exemptions.
- Boundary-exempt project classes: plugins, GH-aware, Rhino UI-aware, tests, testkit, rhino-bridge.
