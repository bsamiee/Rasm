# [H1][MATHNET_PACKAGE_REGISTRY]
>**Dictum:** *MathNet package posture follows the active numerical dependency, not the whole ecosystem.*

<br>

[IMPORTANT] Rasm pins `6.0.0-beta2` to keep Numerics and Numerics.FSharp unified while satisfying `MathNet.Symbolics` `0.25.0`. Treat `Directory.Packages.props` as the package truth.

---
## [1][ACTIVE_BASELINE]
>**Dictum:** *One MathNet stack owns numerical and symbolic execution.*

<br>

| [INDEX] | [PACKAGE] | [VERSION] | [POSTURE] | [ROLE] |
| :-----: | --------- | --------: | :-------: | ---- |
| **[1]** | `MathNet.Numerics` | `6.0.0-beta2` | Use | Managed numerical foundation for matrices, vectors, decomposition, statistics, interpolation, integration, fitting, distributions, transforms, and optimization. |
| **[2]** | `MathNet.Symbolics` | `0.25.0` | Use | Symbolic expression parsing, formatting, algebra, calculus, polynomial, rational, trigonometric, exponential, substitution, evaluation, and compile surfaces. |
| **[3]** | `MathNet.Numerics.FSharp` | `6.0.0-beta2` | Support | Required by Symbolics; pin centrally to block hidden transitive drift. |
| **[4]** | `FSharp.Core` | `8.0.100` | Support | Required F# runtime surface for Symbolics and Numerics.FSharp. |
| **[5]** | `FParsec` | `2.0.0-beta2` | Support | Parser runtime used by Symbolics infix and expression parsing. |

[CRITICAL] Keep the stack universal and explicit. `FParsec` declares `FSharp.Core` `4.3.4`; Rasm pins `FSharp.Core` `8.0.100`. Do not allow Symbolics, FParsec, or Numerics.FSharp to select runtime assemblies outside `Directory.Packages.props`.

---
## [2][CURRENT_CANDIDATES]
>**Dictum:** *Sibling packages are capability evidence, not defaults.*

<br>

| [INDEX] | [PACKAGE] | [ACTIVE_LINE] | [POSTURE] | [ACTIVATION_TRIGGER] |
| :-----: | --------- | ------------: | :-------: | -------------------- |
| **[1]** | `MathNet.Numerics` | `6.0.0-beta2` | Use | Required by active Symbolics posture. |
| **[2]** | `MathNet.Numerics.FSharp` | `6.0.0-beta2` | Support | Required by Symbolics; do not use as primary C# API. |
| **[3]** | `MathNet.Numerics.Data.Text` | External candidate | Add On Demand | Matrix/vector text import or export becomes a product feature; verify current NuGet before use. |
| **[4]** | `MathNet.Numerics.Data.Matlab` | External candidate | Add On Demand | MATLAB interchange becomes a product feature; verify current NuGet before use. |
| **[5]** | `MathNet.Numerics.Providers.OpenBLAS` | External candidate | Add On Proof | Requires package add plus RhinoWIP macOS arm64 native-library-path load and benchmark proof. |
| **[6]** | `MathNet.Numerics.Providers.MKL` | External candidate | Add On Proof | Requires package add plus RhinoWIP macOS arm64 native-library-path load and benchmark proof. |
| **[7]** | `MathNet.Numerics.Providers.CUDA` | External candidate | Out Of Scope | CUDA does not match the macOS-only RhinoWIP target. |

---
## [3][ADJACENT_PACKAGES]
>**Dictum:** *Adjacent MathNet packages must not displace RhinoCommon ownership.*

<br>

| [INDEX] | [PACKAGE] | [VERSION] | [POSTURE] | [REASON] |
| :-----: | --------- | --------: | :-------: | ------ |
| **[1]** | `MathNet.Spatial` | `0.6.0` | Ignore | RhinoCommon owns geometry, transforms, planes, curves, and tolerance semantics. |
| **[2]** | `MathNet.Filtering` | `0.7.0` | Add On Demand | Signal filtering enters current vector and geometry rails. |
| **[3]** | `MathNet.Filtering.Kalman` | `0.7.0` | Add On Demand | Kalman filtering enters an explicit tracking or estimation feature. |
| **[4]** | `MathNet.Symbolics` | `0.25.0` | Use | Active future-facing symbolic expression substrate. |
| **[5]** | `.Signed` package variants | varies | Ignore | Strong-named alternatives duplicate package surfaces without Rasm value. |
| **[6]** | Windows or Linux native provider packages | varies | Out Of Scope | Native assets do not match macOS-only RhinoWIP deployment. |

---
## [4][EVIDENCE]
>**Dictum:** *Pinned local artifacts outrank broad package discovery.*

<br>

| [INDEX] | [ANCHOR] | [USE] |
| :-----: | -------- | ----- |
| **[1]** | `Directory.Packages.props` | Pins active MathNet, F#, and parser package versions. |
| **[2]** | `libs/csharp/Rasm/Rasm.csproj` | Owns direct `MathNet.Numerics` and `MathNet.Symbolics` references. |
| **[3]** | `.cache/nuget/packages/mathnet.numerics/6.0.0-beta2/mathnet.numerics.nuspec` | Local Numerics package metadata and target assets. |
| **[4]** | `.cache/nuget/packages/mathnet.numerics/6.0.0-beta2/lib/net8.0/MathNet.Numerics.xml` | Local Numerics API member surface selected by `net10.0`. |
| **[5]** | `.cache/nuget/packages/mathnet.symbolics/0.25.0/mathnet.symbolics.nuspec` | Symbolics dependency closure and target assets. |
| **[6]** | `.cache/nuget/packages/mathnet.symbolics/0.25.0/lib/net8.0/MathNet.Symbolics.xml` | Local Symbolics API member surface selected by `net10.0`. |
| **[7]** | `libs/csharp/Rasm/Vectors/Matrix.cs` | Active Numerics integration surface. |
| **[8]** | `.cache/nuget/packages/fparsec/2.0.0-beta2/fparsec.nuspec` | Parser dependency declares the lower `FSharp.Core` floor; central pin overrides. |
| **[9]** | `scripts/rhino.sh package <package> <version>` | Static packaging stage copies plugin outputs and dependency assemblies while excluding host assemblies. |
| **[10]** | `https://www.nuget.org/packages/MathNet.Numerics/6.0.0-beta2` | Active Numerics NuGet prerelease metadata. |
| **[11]** | `https://www.nuget.org/packages/MathNet.Symbolics/0.25.0` | Active Symbolics NuGet metadata. |
| **[12]** | `https://developer.rhino3d.com/en/guides/rhinocommon/moving-to-dotnet-core/` | McNeel .NET Core runtime and package loading guidance. |

---
## [5][RULES]
>**Dictum:** *Numerical dependency expansion must preserve Rasm rails.*

<br>

- Keep `MathNet.Numerics` and `MathNet.Symbolics` active only in `Rasm`.
- Keep Symbolics support assemblies centrally pinned.
- Add data packages only for explicit import/export capability.
- Add native provider packages only after RhinoWIP macOS arm64 native-library-path load and benchmark proof.
- Prefer RhinoCommon for geometry and tolerance semantics.
- Verify package output includes `MathNet.Numerics.dll`, `MathNet.Symbolics.dll`, `MathNet.Numerics.FSharp.dll`, `FSharp.Core.dll`, `FParsec.dll`, and `FParsecCS.dll`.
- Treat static package-output inspection as separate from RhinoWIP load-smoke; load-smoke proves runtime support, but package closure can be reviewed without launching Rhino.
