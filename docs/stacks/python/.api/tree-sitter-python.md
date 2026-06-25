# [PY_RUNTIME_API_TREE_SITTER_PYTHON]

`tree-sitter-python` supplies the compiled Python grammar for the tree-sitter runtime: a `language()` factory returning the grammar pointer plus bundled highlight and tags query sources. It is a grammar row consumed by the `tree-sitter` parser; it owns no parsing logic of its own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter-python`
- package: `tree-sitter-python`
- version: `0.25.0`
- license: MIT
- wheel: native CPython extension (`_binding.abi3.so`) shipped as a forward-compatible `abi3` wheel (`cp310-abi3-macosx_11_0_arm64`, `Root-Is-Purelib: false`) — one build runs on cp310 through cp315+ (unlike the runtime `tree-sitter` package, which ships per-version non-abi3 wheels). Admitted and resolved in the cp315 default venv.
- import: `tree_sitter_python`
- owner: `runtime`
- rail: parsing
- namespaces: `tree_sitter_python`
- capability: compiled Python grammar pointer (`language()`), bundled highlight (`HIGHLIGHTS_QUERY`) and tags (`TAGS_QUERY`) query sources lazily loaded from `queries/*.scm`
- query-loading: `HIGHLIGHTS_QUERY`/`TAGS_QUERY` are NOT eager module attributes — module `__getattr__` reads `queries/highlights.scm`/`queries/tags.scm` via `importlib.resources` on first access and caches the text in module globals. They resolve to `Final[str]` query source, never compiled `Query` objects.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grammar query-source family
- rail: parsing
- Both are `Final[str]` raw `.scm` query SOURCE (not compiled queries), lazily loaded on first attribute access. The Python grammar ships highlights and tags only — there is no `LOCALS_QUERY` (the TypeScript grammar does ship one).

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :----------------- | :------------ | :---------------------------------------------- |
|  [01]   | `HIGHLIGHTS_QUERY` | query source  | `Final[str]` syntax-highlight `.scm` source text |
|  [02]   | `TAGS_QUERY`       | query source  | `Final[str]` symbol-tags `.scm` source text      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar operations
- rail: parsing

- `language()` returns an opaque grammar capsule (`-> object`, a `TSLanguage *` PyCapsule) that is wrapped exactly once via `tree_sitter.Language(tree_sitter_python.language())` into a reusable grammar row; the `int`-pointer constructor form is deprecated, so the capsule is passed as the object it is.

| [INDEX] | [SURFACE]    | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------- | :------------- | :---------------------------------------------- |
|  [01]   | `language()` | grammar        | grammar capsule for `tree_sitter.Language(...)` |

## [04]-[IMPLEMENTATION_LAW]

[PARSING_TOPOLOGY]:
- grammar law: the Python grammar is admitted as one `Language(tree_sitter_python.language())` row in the parser's grammar set; the runtime constructs it once and reuses it.
- query law: highlight/tags extraction compiles `HIGHLIGHTS_QUERY`/`TAGS_QUERY` through the `tree-sitter` `Query` surface; the runtime does not author parallel query text where the bundled source suffices.

[LOCAL_ADMISSION]:
- This package supplies a grammar capsule and bundled query sources only; all parser, tree, and query mechanics arrive settled from `.api/tree-sitter.md`.
- The grammar is a parsing row, never a standalone owner; the `abi3` wheel runs unchanged on cp315.

[RAIL_LAW]:
- Package: `tree-sitter-python`
- Owns: the compiled Python grammar row and its bundled `HIGHLIGHTS_QUERY`/`TAGS_QUERY` sources
- Accept: one reused `Language` from `language()`, the bundled `HIGHLIGHTS_QUERY`/`TAGS_QUERY` `Final[str]` sources compiled through `tree-sitter` `Query`
- Reject: per-parse grammar construction, hand-authored query text duplicating the bundled sources
