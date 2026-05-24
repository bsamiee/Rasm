# [H1][SYSTEM_API_MAP]
>**Dictum:** *System APIs are capability choices before package choices.*

<br>

[IMPORTANT] Use this folder before adding `System.*` packages, global usings, build metadata, BCL replacements, or host assembly references. Use `../external-libs/README.md` for product library APIs and `../testing-libs` for test-tool APIs.

---
## [1][FILES]
>**Dictum:** *Each file answers one implementation question.*

<br>

| [INDEX] | [FILE] | [QUESTION] |
| :-----: | ------ | ---------- |
| [1] | `bcl.md` | Which BCL/shared-framework API owns a primitive concern? |
| [2] | `packages.md` | Which package state permits adoption or rejection? |
| [3] | `replacements.md` | Which owner replaces repeated local machinery? |
| [4] | `meta.md` | Which C# build/meta file owns language, analyzer, and host wiring? |

---
## [2][PRECEDENCE]
>**Dictum:** *Owner selection precedes API selection.*

<br>

| [INDEX] | [OWNER] | [CAPABILITY] |
| :-----: | ------- | ------------ |
| [1] | RhinoCommon | Geometry validity, tolerances, transforms, topology, model units. |
| [2] | GH2 | Trees, access, coverage, diagnostics, user-facing numeric boundaries. |
| [3] | MathNet | Linear algebra, solvers, fitting, statistics, symbolic formulas. |
| [4] | BCL | Spans, regex generation, frozen lookups, generic math, time, diagnostics. |
| [5] | LanguageExt | Rails, effects, immutable traversal, runtime records. |
| [6] | Thinktecture | Generated value objects, smart enums, unions, dispatch. |

---
## [3][SOURCE_ORDER]
>**Dictum:** *Runtime claims require local build and host proof.*

<br>

- Read `Directory.Build.props` before changing references, usings, analyzer posture, or RhinoWIP host assemblies.
- Read `Directory.Packages.props` before naming package state.
- Verify .NET and C# features against Microsoft Learn plus local `TargetFramework` and `LangVersion`.
- Verify RhinoWIP/GH2 APIs with local XML/decompile before public docs.
- Keep unadopted packages out of active guidance.
