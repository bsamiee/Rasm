# [PY_DATA_API_SQLGLOT]

`sqlglot` supplies the pure-Python SQL parser, transpiler, optimizer, and dialect registry for the data rail's `QUERY_IR_AND_SQLGATE` concern: `parse`/`parse_one` build an `Expr` syntax tree, `transpile` rewrites between dialects, `optimizer.optimize` canonicalizes the tree against a schema, and `Dialect.get_or_raise` is the dialect gate that admits or rejects a target. The package owner composes `parse_one`, `Expr.sql`, `transpile`, `optimize`, and the `Dialects` enum into the query-IR and SQL-gate path; it removes ad-hoc string concatenation and regex SQL rewriting because the AST owns structure, and it never re-implements the dialect tokenizer, parser, or generator sqlglot already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sqlglot`
- package: `sqlglot`
- import: `sqlglot`
- owner: `data`
- rail: query-ir
- installed: `30.11.0` reflected via assay api on cp315
- entry points: console script `sqlglot` (CLI); library use is import-only
- capability: dialect-aware SQL tokenization, parsing into a typed `Expr` AST, AST traversal/search (`find`/`find_all`/`walk`), schema-driven optimization and qualification, cross-dialect transpilation, the 30-member `Dialects` registry as a parse/generate gate, and a typed error rail (`SqlglotError` root)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: AST, dialect gate, and error rail
- rail: query-ir

`parse`/`parse_one` return `Expr` nodes; `Dialect.get_or_raise` resolves a `Dialects` member (or settings string) into a `Dialect` and raises on an unknown target; the parser/generator/tokenizer triad backs every parse and generate; the error rail descends from `SqlglotError`.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Expr`             | AST node      | syntax-tree root (`sqlglot.expressions.Expression`)          |
|  [02]   | `Dialect`          | dialect       | resolved dialect instance backing parse/generate             |
|  [03]   | `Dialects`         | enum          | `str` enum of the 30 supported dialect names + empty default |
|  [04]   | `ErrorLevel`       | enum          | `IGNORE`/`WARN`/`RAISE`/`IMMEDIATE` parser error policy      |
|  [05]   | `Schema`           | schema        | abstract column/type schema (`abc.ABC`)                      |
|  [06]   | `MappingSchema`    | schema        | mapping-backed `Schema` (`AbstractMappingSchema`, `Schema`)  |
|  [07]   | `Parser`           | engine        | dialect parser producing `Expr` collections                  |
|  [08]   | `Generator`        | engine        | `Expr`-to-SQL string generator                               |
|  [09]   | `Tokenizer`        | engine        | source-to-`Token` lexer                                      |
|  [10]   | `Token`            | lexeme        | one lexed token                                              |
|  [11]   | `TokenType`        | enum          | token kind vocabulary                                        |
|  [12]   | `SqlglotError`     | error (root)  | base exception for all rail failures                         |
|  [13]   | `ParseError`       | error         | parse failure carrying `errors` detail list                  |
|  [14]   | `TokenError`       | error         | tokenization failure                                         |
|  [15]   | `UnsupportedError` | error         | dialect cannot represent the construct                       |
|  [16]   | `OptimizeError`    | error         | optimizer-rule failure                                       |
|  [17]   | `SchemaError`      | error         | schema resolution failure                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, transpile, and dialect gate
- rail: query-ir

`read`/`dialect` carry a `DialectType` (a `Dialects` member, name string with optional `k = v` settings, or `Dialect` instance); `error_level` selects the `ErrorLevel` policy. `transpile` parses under `read` and regenerates under `write`, defaulting `write` to `read` when `identity` is set.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                                                                          | [CAPABILITY]                                                |
| :-----: | :--------------------- | :---------------------------------------------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `parse`                | `parse(sql, read=None, dialect=None, **opts) -> list[Expr                                             | None]`                                                      | parse multi-statement SQL into a list of trees |
|  [02]   | `parse_one`            | `parse_one(sql, read=None, dialect=None, into=None, **opts) -> Expr`                                  | parse one statement (or a `Block`) into a single tree       |
|  [03]   | `maybe_parse`          | `maybe_parse(sql_or_expression, *, into=None, dialect=None, prefix=None, copy=False, **opts) -> Expr` | accept a string or existing `Expr`, parsing only the string |
|  [04]   | `transpile`            | `transpile(sql, read=None, write=None, identity=True, error_level=None, **opts) -> list[str]`         | rewrite SQL from `read` dialect to `write` dialect          |
|  [05]   | `find_tables`          | `find_tables(expression) -> set[Table]`                                                               | collect non-CTE tables referenced in a query                |
|  [06]   | `Dialect.get_or_raise` | `Dialect.get_or_raise(dialect) -> Dialect` (classmethod)                                              | dialect gate: resolve name/settings/instance or raise       |

[ENTRYPOINT_SCOPE]: `Expr` generate, search, and optimize
- rail: query-ir

`Expr.sql` regenerates the tree under any dialect; `find`/`find_all`/`walk` traverse by node type; `optimize`/`qualify` rewrite the tree against a `schema` mapping (`{table: {col: type}}`, `{db: ...}`, or `{catalog: ...}`). `qualify` is the mandatory first optimizer rule — `optimize` keeps it in `RULES` and runs each rule by introspected parameter name.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                                                                                                                                                                                                                                                                                                 | [CAPABILITY]                                             |
| :-----: | :------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `Expr.sql`           | `sql(dialect=None, copy=True, **opts) -> str`                                                                                                                                                                                                                                                                                                                | generate the SQL string for this tree under `dialect`    |
|  [02]   | `Expr.find`          | `find(*expression_types, bfs=True) -> E                                                                                                                                                                                                                                                                                                                      | None`                                                    | first node matching any given `Expr` subtype |
|  [03]   | `Expr.find_all`      | `find_all(*expression_types, bfs=True) -> Iterator[E]`                                                                                                                                                                                                                                                                                                       | all nodes matching any given `Expr` subtype              |
|  [04]   | `Expr.walk`          | `walk(bfs=True, prune=None) -> Iterator[Expr]`                                                                                                                                                                                                                                                                                                               | visit every node, pruning branches via `prune`           |
|  [05]   | `optimizer.optimize` | `optimize(expression, schema=None, db=None, catalog=None, dialect=None, rules=RULES, sql=None, **kwargs) -> Expr`                                                                                                                                                                                                                                            | rewrite the tree into optimized, schema-qualified form   |
|  [06]   | `optimizer.qualify`  | `qualify(expression, dialect=None, db=None, catalog=None, schema=None, expand_alias_refs=True, expand_stars=True, infer_schema=None, isolate_tables=False, qualify_columns=True, allow_partial_qualification=False, validate_qualify_columns=True, quote_identifiers=True, identify=True, canonicalize_table_aliases=False, on_qualify=None, sql=None) -> E` | normalize and qualify tables/columns; mandatory pre-step |

## [04]-[IMPLEMENTATION_LAW]

[QUERY_IR_SQLGATE]:
- import: `import sqlglot` at boundary scope only; module-level import is banned by the manifest import policy.
- parse axis: one `parse_one` owns single-statement IR construction and `parse` owns multi-statement; `read`/`dialect` is a `DialectType` argument row, never a per-dialect parser type; `maybe_parse` is the row that accepts an already-built `Expr` without re-parsing.
- generate axis: `Expr.sql(dialect=...)` is the single generation surface; cross-dialect output is a `dialect` argument, not a parallel generator wrapper; `transpile` is the parse-then-generate composite row keyed by `read`/`write`.
- dialect-gate axis: `Dialect.get_or_raise` is the single admit/reject gate; a target is a `Dialects` member, a name string with optional `k = v` settings, or a `Dialect` instance, and an unknown name fails through `suggest_closest_match_and_fail`; the `Dialects` enum is the closed vocabulary, never a free string.
- traversal axis: `find`/`find_all`/`walk` own AST search by node type and BFS/DFS flag; `find_tables` is the table-collection row over `traverse_scope`; structural inspection routes through these, never through regex over generated SQL.
- optimize axis: `optimize` owns schema-driven rewriting over the `RULES` sequence; `qualify` is the mandatory first rule and stays in the sequence; `schema` is a `{table:{col:type}}`/`{db:...}`/`{catalog:...}` mapping or a `Schema`, never an inline string.
- error axis: every failure descends from `SqlglotError`; `ParseError`/`TokenError`/`UnsupportedError`/`OptimizeError`/`SchemaError` are the typed rows; `ErrorLevel.IGNORE`/`WARN`/`RAISE`/`IMMEDIATE` selects the parser policy; the boundary maps these onto the data rail's typed result, never bare `Exception`.
- evidence: each parse captures dialect, statement count, node count, and error level; each transpile captures `read`/`write` dialect and output statement count; each optimize captures applied rule names and schema presence as a query-IR receipt.
- boundary: sqlglot owns SQL structure (tokenize, parse, generate, optimize, transpile) with no database connection; execution and connection management stay outside this package; the `Expr` AST feeds the data rail's query owner directly; dialect selection is gated through `Dialect.get_or_raise` before any generate.

[RAIL_LAW]:
- Package: `sqlglot`
- Owns: dialect-aware SQL parsing into a typed `Expr` AST, AST traversal/search, schema-driven optimization and qualification, cross-dialect transpilation, the `Dialects` registry gate, and the `SqlglotError` rail
- Accept: parse/transpile/optimize/validate and dialect admission feeding the data rail's query-IR and SQL-gate owners
- Reject: wrapper-renames of `parse_one`/`transpile`/`optimize`; a hand-rolled tokenizer, parser, or generator; regex SQL rewriting where the AST owns structure; a free-string dialect bypassing `Dialect.get_or_raise`/`Dialects`; a bare `Exception` rail in place of the typed `SqlglotError` descendants; a parallel per-dialect parser or generator type
