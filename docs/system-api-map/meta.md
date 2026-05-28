# [H1][CSHARP_META]
>**Dictum:** *C# meta files are build contracts, not incidental configuration.*

<br>

[IMPORTANT] Read this file before changing language version, package graph, analyzer posture, host references, generated assembly metadata, or global usings.

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

`TargetFramework`, `LangVersion`, `AnalysisLevel`, warnings-as-errors, compiler toolset, and analyzer packages must stay centralized. Prefer explicit language version over floating `latest`. Treat analyzer failures as architecture pressure unless the owning policy explicitly permits an exception.

---
## [3][HOST_REFERENCES]
>**Dictum:** *RhinoWIP assemblies are host references, not generic packages.*

<br>

`Directory.Build.props` owns RhinoWIP app paths, `RhinoCommon`, `Rhino.UI`, `Grasshopper2`, `GrasshopperIO`, `Eto`, RhinoCode references, `System.Drawing.Common` host assembly, and conditioned compile package metadata. Verify host assemblies with `uv run python -m tools.quality api doctor` before changing docs or references.

---
## [4][GENERATED_METADATA]
>**Dictum:** *MSBuild emits assembly metadata where possible.*

<br>

Use centralized generated `InternalsVisibleTo`, plugin description attributes, platform metadata, and GUID attributes instead of scattered `AssemblyInfo.cs` files. Keep project files focused on real project identity and references.

---
## [5][PACKAGE_GRAPH]
>**Dictum:** *Central pins need consumers or transitive proof.*

<br>

Add central versions only with active consumer proof, transitive pin need, or a conditioned compile surface. For graph changes, validate restore/build state and update lockfiles intentionally.
