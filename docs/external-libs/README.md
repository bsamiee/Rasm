# [H1][EXTERNAL_LIBS]
>**Dictum:** *External libraries are direct implementation surfaces with verified ownership.*

<br>

[IMPORTANT] Scope: approved non-`System.*` library APIs. BCL, packages, host policy: `../system-api-map/README.md`. Cross-stack owner order and proof: `../usage.md` §1 and §5.

---
## [1][LIBRARIES]
>**Dictum:** *Each library owns one class of capability.*

<br>

| [INDEX] | [LIBRARY] | [OWNER] | [FILES] |
| :-----: | --------- | ------- | ------- |
| [1] | LanguageExt | Rails, effects, immutable collections, effect-polymorphic algorithms. | `languageext/api.md`, `effects.md`, `collections.md`, `combinators.md`, `operators.md`, `prelude.md`, `traits.md`, `rasm.md` |
| [2] | Thinktecture | Generated value objects, smart enums, unions, dispatch. | `thinktecture/api.md`, `objects.md`, `enums.md`, `unions.md`, `union-attributes.md`, `sourcegen.md`, `rasm.md` |
| [3] | MathNet | Numerical algorithms, linear algebra, symbolic expressions; sparse hybrid includes CSparse direct factorization. | `mathnet/api.md`, `linear.md`, `sparse.md`, `symbolics.md`, `rasm.md`, `rhino.md`, `gh2.md` |
| [4] | C# language | C# 14.0 language features at pinned compiler/toolchain. | `csharp/language.md` |

---
## [2][RULES]
>**Dictum:** *Library power removes local ceremony.*

<br>

- Use approved library APIs directly; do not wrap, rename, or mirror them.
- Keep package/version truth in each library `api.md`; keep repo posture in `rasm.md` when present.
- Universal advanced surfaces live in leaf reference files — not in `rasm.md`.
- Mark unadopted packages as not in graph; never describe them as active.
- Route cross-stack usage through `../usage.md`, not duplicated leaf prose.
- Verify advanced claims against pinned XML or local RhinoWIP evidence before writing them.
