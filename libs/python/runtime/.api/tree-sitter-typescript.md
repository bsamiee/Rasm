# [PY_RUNTIME_API_TREE_SITTER_TYPESCRIPT]

`tree-sitter-typescript` supplies the compiled TypeScript and TSX grammars for the tree-sitter runtime: `language_typescript()` and `language_tsx()` factories returning grammar capsules, plus bundled highlights/locals/tags query sources. It is a grammar row consumed by the `tree-sitter` parser; it owns no parsing logic of its own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter-typescript`
- package: `tree-sitter-typescript`
- version: `0.23.2`
- license: MIT
- import: `tree_sitter_typescript`
- owner: `runtime`
- rail: parsing
- namespaces: `tree_sitter_typescript`
- capability: compiled TypeScript and TSX grammar capsules (`language_typescript()`/`language_tsx()`), plus bundled highlights (`HIGHLIGHTS_QUERY`), locals (`LOCALS_QUERY`), and tags (`TAGS_QUERY`) query sources
- query-loading: `HIGHLIGHTS_QUERY`/`LOCALS_QUERY`/`TAGS_QUERY` are NOT eager module attributes — module `__getattr__` reads `queries/highlights.scm`/`queries/locals.scm`/`queries/tags.scm` via `importlib.resources` on first access and caches the text in module globals. They resolve to `Final[str]` query source, never compiled `Query` objects. The two factory capsules wrap once via `tree_sitter.Language(...)`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grammar query-source family
- rail: parsing
- The package surface is two grammar factories AND three `Final[str]` query-source constants. The TypeScript grammar additionally ships a `LOCALS_QUERY` (scope/binding resolution source), which the Python grammar does not. All three are raw `.scm` source loaded lazily, not compiled queries; the two dialects share one query-source set.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :----------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `HIGHLIGHTS_QUERY` | query source  | `Final[str]` syntax-highlight `.scm` source text         |
|  [02]   | `LOCALS_QUERY`     | query source  | `Final[str]` scope/binding-resolution `.scm` source text |
|  [03]   | `TAGS_QUERY`       | query source  | `Final[str]` symbol-tags `.scm` source text              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar operations
- rail: parsing
- Both factories return an opaque grammar capsule (`-> object`, a `TSLanguage *` PyCapsule) wrapped exactly once via `tree_sitter.Language(tree_sitter_typescript.language_typescript())` / `Language(tree_sitter_typescript.language_tsx())` into a reusable grammar row.

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :---------------------- | :------------- | :------------------------- |
|  [01]   | `language_typescript()` | grammar        | TypeScript grammar capsule |
|  [02]   | `language_tsx()`        | grammar        | TSX grammar capsule        |

## [04]-[IMPLEMENTATION_LAW]

[PARSING_TOPOLOGY]:
- grammar law: TypeScript and TSX are two grammar rows — `Language(tree_sitter_typescript.language_typescript())` and `Language(tree_sitter_typescript.language_tsx())` — constructed once into the grammar registry and reused; the dialect is a grammar row, never a parser branch.
- selection law: source dialect selects the grammar row; the parser is rebound to the chosen `Language`, never duplicated per dialect.
- query law: highlight/locals/tags extraction compiles `HIGHLIGHTS_QUERY`/`LOCALS_QUERY`/`TAGS_QUERY` through the `tree-sitter` `Query` surface against the chosen dialect `Language`; the runtime does not author parallel query text where the bundled source suffices.

[LOCAL_ADMISSION]:
- This package supplies grammar capsules and bundled query sources only; all parser, tree, and query mechanics arrive settled from `.api/tree-sitter.md`.

[RAIL_LAW]:
- Package: `tree-sitter-typescript`
- Owns: the compiled TypeScript and TSX grammar rows and their bundled `HIGHLIGHTS_QUERY`/`LOCALS_QUERY`/`TAGS_QUERY` sources
- Accept: two reused `Language` objects from `language_typescript()`/`language_tsx()`, dialect-by-row selection, the bundled query sources compiled through `tree-sitter` `Query`
- Reject: per-parse grammar construction, a parser branch per dialect, hand-authored query text duplicating the bundled sources
