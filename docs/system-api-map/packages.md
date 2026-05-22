# [H1][PACKAGES]
>**Dictum:** *Package state follows concrete consumers and central truth.*

<br>

[IMPORTANT] Central package management lives in `Directory.Packages.props`. Project files declare usage without versions.

---
## [1][STATE]
>**Dictum:** *A package name without state creates false guidance.*

<br>

| [INDEX] | [STATE] | [MEANING] |
| :-----: | ------- | --------- |
| [1] | Active direct | A project references the package now. |
| [2] | Active shared | `Directory.Build.props` references the package for a project class. |
| [3] | Transitive pin | Central version controls a dependency required by an active package. |
| [4] | First-consumer candidate | Approved only when a concrete consumer is added. |
| [5] | Rejected | Creates duplicate paradigm or unsupported host behavior. |

---
## [2][CURRENT]
>**Dictum:** *Current graph truth beats approved intent.*

<br>

| [INDEX] | [PACKAGE] | [STATE] | [OWNER] |
| :-----: | --------- | ------- | ------- |
| [1] | `LanguageExt.Core` | Active shared | Rails, effects, immutable traversal. |
| [2] | `Thinktecture.Runtime.Extensions` | Active shared | Generated value objects, smart enums, unions. |
| [3] | `MathNet.Numerics` | Active direct in `Rasm` | Numerical algorithms. |
| [4] | `MathNet.Symbolics` | Active direct in `Rasm` | Symbolic formulas. |
| [5] | MathNet/F# support closure | Transitive/supporting pins | Symbolics load-context proof, not direct C# API guidance. |
| [6] | `System.Drawing.Common` | Conditioned compile surface | Rhino UI/raster boundary metadata. |

---
## [3][ADOPTION]
>**Dictum:** *New packages need consumer, owner, and validation proof.*

<br>

- Add `PackageVersion` only with a concrete consumer or required transitive pin.
- Keep project `PackageReference` entries versionless.
- Use restore/build proof after graph changes.
- Mark candidate packages as out of graph until consumed.
- Reject packages that duplicate LanguageExt rails, Thinktecture shape, MathNet algorithms, or Rhino/GH semantics.
