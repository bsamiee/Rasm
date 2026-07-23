# [PY_RUNTIME_API_TREE_SITTER_PYTHON]

`tree-sitter-python` supplies the compiled Python grammar for the tree-sitter runtime: a `language()` factory returning the grammar pointer with bundled highlight and tags query sources. It is a grammar row consumed by the `tree-sitter` parser; it owns no parsing logic of its own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter-python`
- package: `tree-sitter-python` (MIT)
- module: `tree_sitter_python`
- namespaces: `tree_sitter_python`
- rail: parsing
- query-loading: `HIGHLIGHTS_QUERY`/`TAGS_QUERY` load lazily — module `__getattr__` reads `queries/highlights.scm`/`queries/tags.scm` via `importlib.resources` on first access, caching the `Final[str]` source in module globals; the values are query SOURCE, never compiled `Query` objects.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grammar query-source family
- Python grammar ships highlights and tags only — no `LOCALS_QUERY`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------- | :------------ | :----------------------------------------------- |
|  [01]   | `HIGHLIGHTS_QUERY` | query source  | `Final[str]` syntax-highlight `.scm` source text |
|  [02]   | `TAGS_QUERY`       | query source  | `Final[str]` symbol-tags `.scm` source text      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar operations
- `language()` returns an opaque grammar capsule (`-> object`, a `TSLanguage *` PyCapsule), wrapped exactly once via `tree_sitter.Language(tree_sitter_python.language())` into a reusable grammar row.

| [INDEX] | [SURFACE]    | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :----------- | :------ | :---------------------------------------------- |
|  [01]   | `language()` | grammar | grammar capsule for `tree_sitter.Language(...)` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Python grammar admits as one reused `Language` row; the runtime constructs it once from `language()`, never per parse.

[STACKING]:
- `tree-sitter`(`.api/tree-sitter.md`): `Language(tree_sitter_python.language())` wraps the capsule once into a reused `Parser`; the bundled `HIGHLIGHTS_QUERY`/`TAGS_QUERY` (`Final[str]`) compile through `Query(language, source)` and run under a `QueryCursor` for highlight/tags extraction.
- runtime grammar registry: the capsule enters the keyed grammar registry as one reused `Language`, with capture-name and node-kind strings resolved to ids against `Language` introspection once at build, never per parse.

[LOCAL_ADMISSION]:
- this package supplies a grammar capsule and bundled query sources only; all parser, tree, and query mechanics arrive settled from `.api/tree-sitter.md`.

[RAIL_LAW]:
- Package: `tree-sitter-python`
- Owns: the compiled Python grammar row and its bundled `HIGHLIGHTS_QUERY`/`TAGS_QUERY` sources
- Accept: one reused `Language` from `language()`, the bundled `Final[str]` sources compiled through `tree-sitter` `Query`
- Reject: per-parse grammar construction, hand-authored query text duplicating the bundled sources
