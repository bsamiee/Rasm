# [PY_RUNTIME_API_TREE_SITTER_TYPESCRIPT]

`tree-sitter-typescript` supplies the compiled TypeScript and TSX grammars for the tree-sitter runtime: `language_typescript()` and `language_tsx()` factories returning grammar pointers. It is a grammar row consumed by the `tree-sitter` parser; it owns no parsing logic of its own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter-typescript`
- package: `tree-sitter-typescript`
- import: `tree_sitter_typescript`
- owner: `runtime`
- rail: parsing
- namespaces: `tree_sitter_typescript`
- capability: compiled TypeScript and TSX grammar pointers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grammar family
- rail: parsing
- No named public types; the package surface is two factory functions only.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar operations
- rail: parsing

| [INDEX] | [SURFACE]             | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :-------------------- | :------------- | :------------------------- |
|  [01]   | `language_typescript` | grammar        | TypeScript grammar pointer |
|  [02]   | `language_tsx`        | grammar        | TSX grammar pointer        |

## [04]-[IMPLEMENTATION_LAW]

[PARSING_TOPOLOGY]:
- grammar law: TypeScript and TSX are two grammar rows — `Language(tree_sitter_typescript.language_typescript())` and `Language(tree_sitter_typescript.language_tsx())` — constructed once and reused; the dialect is a grammar row, never a parser branch.
- selection law: source dialect selects the grammar row; the parser is rebound to the chosen `Language`, never duplicated per dialect.

[LOCAL_ADMISSION]:
- This package supplies grammar pointers only; all parser, tree, and query mechanics arrive settled from `.api/tree-sitter.md`.
- The two grammars are parsing rows, never standalone owners.

[RAIL_LAW]:
- Package: `tree-sitter-typescript`
- Owns: the compiled TypeScript and TSX grammar rows
- Accept: two reused `Language` objects from `language_typescript()`/`language_tsx()`, dialect-by-row selection
- Reject: per-parse grammar construction, a parser branch per dialect
