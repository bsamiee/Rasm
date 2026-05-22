# [H1][SYSTEM_API_MAP]
>**Dictum:** *System APIs are explicit capability choices, not incidental imports.*

<br>

[IMPORTANT] Reference map for BCL, shared-framework, NuGet, and RhinoWIP-hosted assemblies in Rasm. Use before adding package references, global usings, or local code that duplicates .NET capability.

---
## [1][SCOPE]
>**Dictum:** *A namespace import and a package dependency are different decisions.*

<br>

| [INDEX] | [SURFACE] | [OWNERSHIP_RULE] |
| :-----: | --------- | ---------------- |
| [1] | Shared-framework `System.*` namespace | Use directly; no package. |
| [2] | `System.*` package outside shared framework | Add with concrete consumer and central version. |
| [3] | RhinoWIP app-bundle assembly | Reference through `Directory.Build.props`; require host parity before NuGet substitution. |
| [4] | Approved external library | Use direct library APIs when the library owns the concern. |
| [5] | Candidate package with no consumer | Keep out of the package graph. |

---
## [2][REFERENCE_POLICY]
>**Dictum:** *Universal dependencies must be universal in behavior, not just convenient in syntax.*

<br>

| [INDEX] | [REFERENCE_KIND] | [RULE] | [RASM_EXAMPLE] |
| :-----: | ---------------- | ------ | -------------- |
| [1] | Shared framework | Use local namespace imports; centralize only universal usings. | `System.Text.RegularExpressions`, `System.Buffers`, `System.Numerics`. |
| [2] | Central package | Pin once in `Directory.Packages.props`; project usage carries no version. | `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`, `MathNet.Numerics`. |
| [3] | Host assembly | Resolve by app-bundle `HintPath`; keep `Private=false`. | `RhinoCommon`, `Rhino.UI`, `Grasshopper2`, `Eto`, `System.Drawing.Common`. |
| [4] | Tool package | Keep out of production libraries unless compile analysis uses it. | Roslyn, analyzers, BenchmarkDotNet. |

---
## [3][SYSTEM_DRAWING_COMMON]
>**Dictum:** *`System.Drawing.Common` is a Rhino UI boundary dependency in Rasm.*

<br>

`System.Drawing.Common` is not a universal package or global project dependency. Rasm targets macOS RhinoWIP and obtains runtime drawing assemblies from the RhinoWIP app bundle for Rhino UI-aware projects.

| [INDEX] | [DECISION] | [REASON] |
| :-----: | ---------- | -------- |
| [1] | `Directory.Build.props` host `Reference`. | `IsRhinoUiAwareProject` resolves RhinoWIP app-bundle runtime assembly. |
| [2] | `Directory.Build.props` compile `PackageReference`. | `ExcludeAssets="runtime"` supplies forwarded `System.Drawing.Imaging` type metadata. |
| [3] | `Directory.Packages.props` central version. | Version exists only while the conditioned compile package exists. |
| [4] | No global drawing usings. | Drawing APIs stay inside Rhino UI/raster boundaries. |

| [INDEX] | [ANCHOR] | [PROOF] |
| :-----: | -------- | ------- |
| [1] | `Directory.Build.props` | `SystemDrawingCommonReferencePath`, host `Reference`, and conditioned package usage. |
| [2] | `Directory.Packages.props` | Central `System.Drawing.Common` compile-surface version. |
| [3] | `libs/csharp/Rasm.Rhino` | Raster publish code owns `System.Drawing.Imaging` usage. |
| [4] | RhinoWIP app bundle | Runtime assembly source on macOS. |

---
## [4][FILES]
>**Dictum:** *Each file answers one implementation question.*

<br>

| [INDEX] | [FILE] | [PURPOSE] |
| :-----: | ------ | --------- |
| [1] | `api.md` | System namespace and API family registry. |
| [2] | `replacement-taxonomy.md` | Code-smell to approved replacement map. |
| [3] | `package-adoption.md` | Package adoption, documentation, and rejection decisions. |

---
## [5][SOURCES]
>**Dictum:** *Package truth comes from package metadata; API truth comes from primary docs.*

<br>

| [INDEX] | [SOURCE] | [USE] |
| :-----: | -------- | ----- |
| [1] | `Directory.Packages.props` | Repo package truth. |
| [2] | NuGet V3 flat-container metadata | Stable and prerelease package versions. |
| [3] | Microsoft Learn .NET docs | BCL, C# 14, `System.Drawing.Common`, regex, and span guidance. |
| [4] | Local RhinoWIP app bundle | RhinoCommon, Rhino.UI, GH2, Eto, and host System.Drawing assembly truth. |
| [5] | Local package XML under `.cache/nuget/packages` | Pinned API member surfaces. |
