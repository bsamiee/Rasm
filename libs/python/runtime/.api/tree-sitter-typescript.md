# [PY_RUNTIME_API_TREE_SITTER_TYPESCRIPT]

`tree-sitter-typescript` mints two compiled grammar rows — TypeScript and TSX — for the `tree-sitter` parser, each behind a factory capsule, alongside the bundled `.scm` highlight, locals, and tags query sources. It owns grammar and query text alone; every parse, tree, and query mechanic settles in `.api/tree-sitter.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter-typescript`
- package: `tree-sitter-typescript` (MIT)
- module: `tree_sitter_typescript`
- rail: parsing
- namespaces: `tree_sitter_typescript`
- query-loading: the three query constants load lazily on first attribute access through module `__getattr__`, resolving to `Final[str]` `.scm` source text, never eager attributes and never a compiled `Query`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grammar query-source family

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `HIGHLIGHTS_QUERY` | query source  | `Final[str]` syntax-highlight `.scm` source text         |
|  [02]   | `LOCALS_QUERY`     | query source  | `Final[str]` scope/binding-resolution `.scm` source text |
|  [03]   | `TAGS_QUERY`       | query source  | `Final[str]` symbol-tags `.scm` source text              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: grammar factories

| [INDEX] | [SURFACE]               | [SHAPE] | [CAPABILITY]               |
| :-----: | :---------------------- | :------ | :------------------------- |
|  [01]   | `language_typescript()` | factory | TypeScript grammar capsule |
|  [02]   | `language_tsx()`        | factory | TSX grammar capsule        |

- both factories return an opaque `TSLanguage *` PyCapsule (`-> object`), never a live `Language`; the parser wraps it once.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- grammar law: TypeScript and TSX are two `Language` rows — `Language(language_typescript())` / `Language(language_tsx())` — constructed once and reused; source dialect selects the row and rebinds `Parser.language`, never a parser branch or a per-dialect duplicate.
- query law: highlight/locals/tags extraction compiles `HIGHLIGHTS_QUERY`/`LOCALS_QUERY`/`TAGS_QUERY` through the `tree-sitter` `Query` surface against the chosen `Language`; the bundled source is the query text.

[STACKING]:
- `tree-sitter`(`.api/tree-sitter.md`): each capsule wraps once via `Language(language_typescript())` / `Language(language_tsx())` into one reused `Parser`, dialect-by-row, and the three query sources compile through `Query(language, source)` run under a `QueryCursor`.
- runtime grammar registry: the two `Language` rows and their resolved capture/node-kind ids land once in the keyed registry `.api/tree-sitter.md` builds, reused across parses.

[LOCAL_ADMISSION]:
- This package supplies grammar capsules and bundled query sources only; every parser, tree, and query mechanic arrives settled from `.api/tree-sitter.md`.

[RAIL_LAW]:
- Package: `tree-sitter-typescript`
- Owns: the compiled TypeScript and TSX grammar rows and their bundled `HIGHLIGHTS_QUERY`/`LOCALS_QUERY`/`TAGS_QUERY` sources
- Accept: two reused `Language` rows from the factories, dialect-by-row selection, the bundled sources compiled through `tree-sitter` `Query`
- Reject: per-parse grammar construction, a parser branch per dialect, hand-authored query text duplicating the bundled sources
