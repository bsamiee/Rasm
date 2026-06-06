# [EXTERNAL_LIBS]

[IMPORTANT] Scope: approved non-standard-library APIs. Package state, host policy, cross-stack owner order, and proof live in the project-declared owners.

## [1][LIBRARIES]

| [INDEX] | [LIBRARY]           | [OWNER]                                                                                                          |
| :-----: | ------------------- | ---------------------------------------------------------------------------------------------------------------- |
|   [1]   | LanguageExt         | Rails, effects, immutable collections, effect-polymorphic algorithms.                                            |
|   [2]   | Thinktecture        | Generated value objects, smart enums, unions, dispatch.                                                          |
|   [3]   | MathNet             | Numerical algorithms, linear algebra, symbolic expressions; sparse hybrid includes CSparse direct factorization. |
|   [4]   | Host SDK boundaries | Project usage owner, maintained host metadata, and nested host instruction owners.                               |
|   [5]   | Host composition    | Bootstrap packages (Scrutor, EF, OTel…) — not-in-graph until consumer.                                           |
|   [6]   | C# language         | C# 14.0 language features for the active compiler/toolchain.                                                      |

[FILES]
- [1] `languageext/api.md`, `effects.md`, `collections.md`, `combinators.md`, `operators.md`, `prelude.md`, `traits.md`, `rasm.md`
- [2] `thinktecture/api.md`, `objects.md`, `enums.md`, `unions.md`, `union-attributes.md`, `sourcegen.md`, `rasm.md`
- [3] `mathnet/api.md`, `linear.md`, `sparse.md`, `symbolics.md`, `rasm.md`
- [4] project usage owner; nested host instruction owners
- [5] `../host-libraries.md`
- [6] `csharp/language.md`

## [2][RULES]

- Use approved library APIs directly; do not wrap, rename, or mirror them.
- Keep package graph state in the project package map; keep repo posture in `*/rasm.md` and host boundaries in the project usage owner.
- Universal advanced surfaces live in leaf reference files — not in `rasm.md`.
- Mark unadopted packages as not in graph; never describe them as active.
- Route cross-stack usage through the project usage owner, not duplicated leaf prose.
- Document advanced API claims through local XML, generated API rail, or host evidence.
