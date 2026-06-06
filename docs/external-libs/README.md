# [EXTERNAL_LIBS]

Scope: approved non-standard-library APIs. Package state, cross-stack owner order, and proof live in the project-declared owners.

## [1][LIBRARIES]

| [INDEX] | [LIBRARY]           | [OWNER]                                                                                                          |
| :-----: | ------------------- | ---------------------------------------------------------------------------------------------------------------- |
|   [1]   | LanguageExt         | Rails, effects, immutable collections, effect-polymorphic algorithms.                                            |
|   [2]   | Thinktecture        | Generated value objects, smart enums, unions, dispatch.                                                          |
|   [3]   | MathNet             | Numerical algorithms, linear algebra, symbolic expressions; sparse hybrid includes CSparse direct factorization. |
|   [4]   | C# language         | C# 14.0 language features for the active compiler/toolchain.                                                      |

## [2][FILES]

```text
docs/external-libs/
├── csharp/
│   └── language.md
├── languageext/
│   ├── api.md
│   ├── collections.md
│   ├── combinators.md
│   ├── effects.md
│   ├── operators.md
│   ├── prelude.md
│   ├── rasm.md
│   └── traits.md
├── mathnet/
│   ├── api.md
│   ├── linear.md
│   ├── rasm.md
│   ├── sparse.md
│   └── symbolics.md
└── thinktecture/
    ├── api.md
    ├── enums.md
    ├── objects.md
    ├── rasm.md
    ├── sourcegen.md
    ├── union-attributes.md
    └── unions.md
```

## [3][RULES]

- Use approved library APIs directly; do not wrap, rename, or mirror them.
- Keep package graph state in the project package map; keep repo posture in `*/rasm.md`.
- Universal advanced surfaces live in leaf reference files — not in `rasm.md`.
- Mark unadopted packages as not in graph; never describe them as active.
- Route cross-stack usage through the project usage owner, not duplicated leaf prose.
- Document advanced API claims through local XML or the generated API rail.
