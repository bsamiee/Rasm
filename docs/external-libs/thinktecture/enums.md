# [THINKTECTURE_ENUMS]

[IMPORTANT] Use smart enums where a bounded vocabulary carries lookup, metadata, and behavior.

## [1][REGISTRIES]

| [INDEX] | [SURFACE]                    | [USE]                                        |
| :-----: | ---------------------------- | -------------------------------------------- |
|   [1]   | `Items`                      | Complete registry for derived maps and docs. |
|   [2]   | `Get`, `TryGet`              | Key admission at boundary edges.             |
|   [3]   | Key comparers                | Protocol-specific string or scalar policy.   |
|   [4]   | Generated JSON/parse support | Only when an adopted boundary requires it.   |

## [2][BEHAVIOR]

Use delegate-backed constructor members when each item owns behavior such as metric projection, port policy, field blend, display mode, or host capability gate. Prefer generated `Switch`/`Map` or item delegates over external dictionaries and repeated switch arms.

`[UseDelegateFromConstructor]` on SmartEnum items injects constructor-selected delegates (e.g. `MatrixNormKind`, `MeshLaplacian`, wire traversal policy). Prefer `static` factory helpers + initializer expressions for dense case tables — see `objects.md` §5.2.

## [3][RASM_POLICY]

- Use smart enums for stable bounded protocol vocabularies.
- Keep Rhino/GH native names at boundary mapping only.
- Derive lookup tables from generated `Items`.
- Do not mirror a smart enum with parallel constants or dictionaries.
