# [H1][EXTERNAL_LIBS]
>**Dictum:** *External libraries are direct implementation surfaces with verified ownership.*

<br>

[IMPORTANT] Use this folder for approved non-`System.*` library APIs. Use `../system-api-map/README.md` for BCL, shared-framework, package, build, and host assembly policy.

---
## [1][SOURCE_ORDER]
>**Dictum:** *Pinned local truth outranks package marketing.*

<br>

| [INDEX] | [SOURCE] | [OWNS] |
| :-----: | -------- | ------ |
| [1] | `Directory.Packages.props` | Pinned versions and central package state. |
| [2] | `Directory.Build.props` | Package references, global usings, RhinoWIP/GH2 host references. |
| [3] | Local NuGet XML/nuspec/DLL | Exact API surface for pinned assets. |
| [4] | RhinoWIP XML/decompile | RhinoCommon, GH2, Rhino.UI, Eto compile truth. |
| [5] | Official docs | Current context after local proof. |

---
## [2][LIBRARIES]
>**Dictum:** *Each library owns one class of capability.*

<br>

| [INDEX] | [LIBRARY] | [OWNER] | [FILES] |
| :-----: | --------- | ------- | ------- |
| [1] | LanguageExt | Rails, effects, immutable collections, effect-polymorphic algorithms. | `languageext/api.md`, `effects.md`, `collections.md`, `combinators.md`, `operators.md`, `prelude.md`, `traits.md`, `rasm.md` |
| [2] | Thinktecture | Generated value objects, smart enums, unions, dispatch. | `thinktecture/api.md`, `objects.md`, `enums.md`, `unions.md`, `union-attributes.md`, `sourcegen.md`, `rasm.md` |
| [3] | MathNet | Numerical algorithms, linear algebra, symbolic expressions. | `mathnet/api.md`, `linear.md`, `rasm.md`, `symbolics.md`, `rhino.md`, `gh2.md` |

---
## [3][RULES]
>**Dictum:** *Library power removes local ceremony.*

<br>

- Use approved library APIs directly; do not wrap, rename, or mirror them.
- Keep package/version truth in each library `api.md`; keep repo posture in `rasm.md`.
- Mark unadopted packages as not in graph; never describe them as active.
- Route cross-stack usage through `../usage.md`, not duplicated leaf prose.
- Verify advanced claims against pinned XML or local RhinoWIP evidence before writing them.
