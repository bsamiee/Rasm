# [PY_RUNTIME_API_TREE_SITTER_PYTHON]

`tree-sitter-python` supplies the compiled Python grammar for the tree-sitter runtime: a `language()` factory returning the grammar pointer plus bundled highlight and tags query sources. It is a grammar row consumed by the `tree-sitter` parser; it owns no parsing logic of its own.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter-python`
- package: `tree-sitter-python`
- import: `tree_sitter_python`
- owner: `runtime`
- rail: parsing
- namespaces: `tree_sitter_python`
- capability: compiled Python grammar pointer, bundled highlight/tags query sources

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grammar query-source family
- rail: parsing

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :----------------- | :------------ | :-------------------------- |
|   [1]   | `HIGHLIGHTS_QUERY` | query source  | syntax-highlight query text |
|   [2]   | `TAGS_QUERY`       | query source  | symbol-tags query text      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar operations
- rail: parsing

| [INDEX] | [SURFACE]  | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :--------- | :------------- | :----------------------------------------- |
|   [1]   | `language` | grammar        | grammar pointer for `tree_sitter.Language` |

## [4]-[IMPLEMENTATION_LAW]

[PARSING_TOPOLOGY]:
- grammar law: the Python grammar is admitted as one `Language(tree_sitter_python.language())` row in the parser's grammar set; the runtime constructs it once and reuses it.
- query law: highlight/tags extraction compiles `HIGHLIGHTS_QUERY`/`TAGS_QUERY` through the `tree-sitter` `Query` surface; the runtime does not author parallel query text where the bundled source suffices.

[LOCAL_ADMISSION]:
- This package supplies a grammar pointer only; all parser, tree, and query mechanics arrive settled from `.api/tree-sitter.md`.
- The grammar is a parsing row, never a standalone owner.

[RAIL_LAW]:
- Package: `tree-sitter-python`
- Owns: the compiled Python grammar row and its bundled query sources
- Accept: one reused `Language` from `language()`, the bundled `HIGHLIGHTS_QUERY`/`TAGS_QUERY` sources
- Reject: per-parse grammar construction, hand-authored query text duplicating the bundled sources
