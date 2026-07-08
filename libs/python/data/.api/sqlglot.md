# [PY_DATA_API_SQLGLOT]

`sqlglot` supplies the pure-Python SQL tokenizer/parser/transpiler/optimizer, the dialect registry, and the column-level lineage and AST-diff engines for the data rail's `QUERY_IR_AND_SQLGATE` and lineage concerns: `parse`/`parse_one` build an `Expr` syntax tree, `transpile` rewrites between dialects, `optimizer.optimize` canonicalizes the tree against a schema across the 14-rule pipeline, `lineage.lineage` traces a column to its source columns, `diff` computes the AST edit set between two trees, and `Dialect.get_or_raise` is the dialect gate that admits or rejects a target. The package owner composes `parse_one`, `Expr.sql`, `transpile`, `optimize`, `lineage`, `diff`, and the `Dialects` enum into the query-IR/SQL-gate/lineage path; it removes ad-hoc string concatenation and regex SQL rewriting because the AST owns structure, builds new SQL through the typed builder DSL (`select`/`column`/`func`/`condition`) rather than f-strings, and never re-implements the dialect tokenizer, parser, generator, scope-resolver, or lineage tracer sqlglot already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sqlglot`
- package: `sqlglot`
- version: `30.12.0`
- license: MIT (permissive)
- module: `import sqlglot`
- owner: `data`
- rail: query-ir
- entry points: console script `sqlglot` (CLI); library use is import-only
- capability: dialect-aware SQL tokenization, parsing into a typed `Expr` AST, AST traversal/search/in-place rewrite (`find`/`find_all`/`walk`/`transform`/`replace`), the typed builder DSL (`select`/`column`/`func`/`condition`/`cast`/`case`/`insert`/`delete`/`merge`/`union`), schema-driven optimization across the 14-rule `RULES` pipeline, scope analysis (`build_scope`/`traverse_scope`/`find_all_in_scope`), column-level lineage (`lineage.lineage`), AST diffing into an `Edit` list (`diff`), cross-dialect transpilation, the 31-member `Dialects` registry as a parse/generate gate, JSONPath parsing, and a typed error rail rooted at `SqlglotError`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: AST, dialect gate, scope, lineage, diff, and error rail
- rail: query-ir

`parse`/`parse_one` return `Expr` nodes; `Dialect.get_or_raise` resolves a `Dialects` member (or settings string) into a `Dialect` and raises on an unknown target; the parser/generator/tokenizer triad backs every parse and generate; `Scope`/`Node`/`Edit` are the scope-analysis, lineage, and diff carriers; the error rail descends from `SqlglotError`.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| --- | --- | --- | --- |
| [01] | `Expr` | AST node | syntax-tree root (`sqlglot.expressions.Expression`); `exp` is the subtype namespace |
| [02] | `Dialect` | dialect | resolved dialect instance backing parse/generate |
| [03] | `Dialects` | enum | `str` enum of 31 supported dialect names + the empty default |
| [04] | `ErrorLevel` | enum | `IGNORE`/`WARN`/`RAISE`/`IMMEDIATE` parser error policy |
| [05] | `Schema` | schema | abstract column/type schema (`abc.ABC`) |
| [06] | `MappingSchema` | schema | mapping-backed `Schema` (`AbstractMappingSchema`) |
| [07] | `Parser` | engine | dialect parser producing `Expr` collections |
| [08] | `Generator` | engine | `Expr`-to-SQL string generator |
| [09] | `Tokenizer` / `Token` / `TokenType` | engine | source-to-`Token` lexer, the lexeme, and the token-kind vocabulary |
| [10] | `optimizer.scope.Scope` | scope | resolved name scope from `build_scope`; `find_all_in_scope` walks it |
| [11] | `lineage.Node` | lineage node | a column-lineage tree node (`name`/`source`/`downstream`); root from `lineage()` |
| [12] | `diff.Edit` (`Insert`/`Remove`/`Move`/`Update`/`Keep`) | diff op | one AST edit from `diff(source, target)` |
| [13] | `exp.Where` / `exp.Having` / `exp.Qualify` / `exp.Join` / `exp.Column` | AST subtype | `exp.Expression` subtypes in the `sqlglot.expressions` namespace; the predicate/clause nodes a `parse_one(text).find_all(exp.Where, exp.Having, exp.Qualify, exp.Join)` predicate-count fold scans, and `exp.Column` the column reference |
| [14] | `SqlglotError` | error (root) | base exception for all rail failures |
| [15] | `ParseError` | error | parse failure carrying an `errors` detail list |
| [16] | `TokenError` | error | tokenization failure |
| [17] | `UnsupportedError` | error | dialect cannot represent the construct |
| [18] | `OptimizeError` / `SchemaError` | error | optimizer-rule failure / schema resolution failure |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, transpile, build, and dialect gate
- rail: query-ir

`read`/`dialect` carry a `DialectType` (a `Dialects` member, a name string with optional `k = v` settings, or a `Dialect` instance); `error_level` selects the `ErrorLevel` policy. `transpile` parses under `read` and regenerates under `write`, defaulting `write` to `read` when `identity` is set. The builder functions (`select`/`column`/`func`/...) construct an `Expr` tree directly without parsing a string.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `parse` | `parse(sql, read=None, dialect=None, **opts) -> list[Expr \| None]` | parse multi-statement SQL into a list of trees |
| [02] | `parse_one` | `parse_one(sql, read=None, dialect=None, into=None, **opts) -> Expr` | parse one statement (optionally `into` a target subtype) |
| [03] | `maybe_parse` | `maybe_parse(sql_or_expression, *, into=None, dialect=None, prefix=None, copy=False, **opts) -> Expr` | accept a string or existing `Expr`, parsing only the string |
| [04] | `transpile` | `transpile(sql, read=None, write=None, identity=True, error_level=None, **opts) -> list[str]` | rewrite SQL from `read` dialect to `write` dialect |
| [05] | `tokenize` | `tokenize(sql, read=None, dialect=None) -> list[Token]` | lex without parsing (lexeme-level inspection) |
| [06] | `find_tables` | `find_tables(expression) -> set[Table]` | collect non-CTE tables referenced in a query |
| [07] | `Dialect.get_or_raise` | `Dialect.get_or_raise(dialect) -> Dialect` (classmethod) | dialect gate: resolve name/settings/instance or raise |
| [08] | builder DSL | `select(*expr, dialect=None, **opts) -> Select`, `from_`, `condition`, `and_`/`or_`/`not_`, `column(col, table=None, db=None, catalog=None, *, quoted=None)`, `func(name, *args, dialect=None)`, `cast(expression, to, dialect=None)`, `case(...)`, `alias`, `table`, `subquery`, `insert`, `delete`, `merge`, `union`/`intersect`/`except_`, `to_column`/`to_table`/`to_identifier` | construct an `Expr` tree programmatically instead of string concatenation |

[ENTRYPOINT_SCOPE]: `Expr` generate, search, rewrite, optimize, scope, lineage, diff
- rail: query-ir

`Expr.sql` regenerates the tree under any dialect; `find`/`find_all`/`walk`/`iter_expressions` traverse by node type; `transform`/`replace` rewrite in place; `optimize`/`qualify` rewrite the tree against a `schema` mapping (`{table: {col: type}}`, `{db: ...}`, or `{catalog: ...}`); `build_scope`/`traverse_scope` resolve name scopes; `lineage` traces a column to its sources; `diff` reports the edit set between two trees. `qualify` is the mandatory first optimizer rule.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| --- | --- | --- | --- |
| [01] | `Expr.sql` | `sql(dialect=None, copy=True, **opts) -> str` | generate the SQL string for this tree under `dialect` |
| [02] | `Expr.find` / `find_all` | `find(*expression_types, bfs=True) -> E \| None`; `find_all(*expression_types, bfs=True) -> Iterator[E]` | first / all nodes matching any given `Expr` subtype |
| [03] | `Expr.walk` / `find_ancestor` | `walk(bfs=True, prune=None) -> Iterator[Expr]`; `find_ancestor(*expression_types) -> Expr \| None` | visit every node (prunable) / climb to a matching ancestor |
| [04] | `Expr.transform` / `replace` | `transform(fn, *args, copy=True) -> Expr`; `node.replace(other)` | recursive bottom-up rewrite / swap a node in its parent |
| [05] | `optimizer.optimize` | `optimize(expression, schema=None, db=None, catalog=None, dialect=None, rules=RULES, **kwargs) -> Expr` | run the 14-rule pipeline into optimized, schema-qualified form |
| [06] | `optimizer.qualify` | `qualify(expression, dialect=None, db=None, catalog=None, schema=None, expand_alias_refs=True, expand_stars=True, infer_schema=None, isolate_tables=False, qualify_columns=True, allow_partial_qualification=False, validate_qualify_columns=True, quote_identifiers=True, identify=True, ...) -> Expr` | normalize and qualify tables/columns; mandatory pre-step |
| [07] | `optimizer.scope.build_scope` | `build_scope(expression) -> Scope \| None`; `traverse_scope(expression) -> list[Scope]`; `find_all_in_scope(scope, expression_types)` | resolve and walk name scopes (source resolution, alias maps) |
| [08] | `lineage.lineage` | `lineage(column, sql, schema=None, sources=None, dialect=None, trim_selects=True, on_node=None, **kwargs) -> Node` | column-level lineage tree from a target column to its sources |
| [09] | `diff` | `diff(source, target, matchings=None, delta_only=False, **kwargs) -> list[Edit]` | AST edit set (`Insert`/`Remove`/`Move`/`Update`/`Keep`) between two trees |
| [10] | `Select.selects` | `selects -> list[Expr]` (property) | the projection list of a `Select` tree; each entry exposes `alias_or_name`, the qualified output-column names the lineage fold iterates |

[OPTIMIZER_RULES]: `optimizer.optimize.RULES` is the ordered 14-rule pipeline run by `optimize`: `qualify` -> `pushdown_projections` -> `normalize` -> `unnest_subqueries` -> `pushdown_predicates` -> `optimize_joins` -> `eliminate_subqueries` -> `merge_subqueries` -> `eliminate_joins` -> `eliminate_ctes` -> `quote_identifiers` -> `annotate_types` -> `canonicalize` -> `simplify`. Each rule is dispatched by introspected parameter name; pass a sliced `rules=` tuple to run a subset.

## [04]-[IMPLEMENTATION_LAW]

[QUERY_IR_SQLGATE]:
- import: `import sqlglot` at module scope; the pure-Python parse/transpile plane is a module-top substrate corpus-wide.
- parse axis: one `parse_one` owns single-statement IR construction and `parse` owns multi-statement; `read`/`dialect` is a `DialectType` argument row, never a per-dialect parser type; `maybe_parse` accepts an already-built `Expr` without re-parsing; `tokenize` is the lexeme-level row.
- build axis: new SQL is constructed through the typed builder DSL (`select`/`from_`/`column`/`func`/`cast`/`condition`/`insert`/`delete`/`merge`/`union`) returning an `Expr`, never f-string concatenation; `Expr.transform`/`replace` rewrite an existing tree in place.
- generate axis: `Expr.sql(dialect=...)` is the single generation surface; cross-dialect output is a `dialect` argument, not a parallel generator wrapper; `transpile` is the parse-then-generate composite row keyed by `read`/`write`.
- dialect-gate axis: `Dialect.get_or_raise` is the single admit/reject gate; a target is a `Dialects` member, a name string with optional `k = v` settings, or a `Dialect` instance, and an unknown name fails through `suggest_closest_match_and_fail`; the `Dialects` enum is the closed vocabulary, never a free string.
- traversal axis: `find`/`find_all`/`walk`/`find_ancestor` own AST search by node type and BFS/DFS flag; `find_tables` is the table-collection row; `optimizer.scope.build_scope`/`traverse_scope`/`find_all_in_scope` own source/alias resolution. Structural inspection routes through these, never regex over generated SQL.
- optimize axis: `optimize` runs the 14-rule `RULES` pipeline; `qualify` is the mandatory first rule and stays in the sequence; `schema` is a `{table:{col:type}}`/`{db:...}`/`{catalog:...}` mapping or a `Schema`, never an inline string. A subset run slices `rules=`.
- lineage axis: `lineage.lineage(column, sql, schema=...)` produces the column-lineage `Node` tree (each node carries `name`/`source`/`downstream`); this is the lineage gate the header names — never re-derive column provenance by string scanning. `diff(source, target)` is the AST change-detection row producing the `Edit` list for query-evolution and migration analysis.
- error axis: every failure descends from `SqlglotError`; `ParseError`/`TokenError`/`UnsupportedError`/`OptimizeError`/`SchemaError` are the typed rows; `ErrorLevel.IGNORE`/`WARN`/`RAISE`/`IMMEDIATE` selects the parser policy; the boundary maps these onto the data rail's typed result, never bare `Exception`.
- evidence: each parse captures dialect, statement count, node count, and error level; each transpile captures `read`/`write` dialect and output statement count; each optimize captures applied rule names and schema presence; each lineage run captures the target column and source-table set; each diff captures the edit-op counts — all as query-IR receipts.

[SIBLING_STACK]:
- boundary: sqlglot owns SQL structure (tokenize, parse, generate, optimize, transpile, lineage, diff) with no database connection; execution and connection management stay outside this package. The `Expr` AST feeds the data rail's query owner directly; dialect selection is gated through `Dialect.get_or_raise` before any generate.
- `ibis` (sibling catalog) compiles its expression graph to SQL through sqlglot; centralizing sqlglot here pins the `ibis` transitive SQL backend. `duckdb`/`datafusion` (sibling execution engines) receive sqlglot-transpiled SQL keyed by the `duckdb`/`datafusion` `Dialects` member — transpile to the engine dialect, never hand-write engine-specific SQL.
- `lineage.lineage` + `diff` feed the data rail's provenance/migration owners: lineage maps a result column back to its physical source columns (schema-driven), and diff drives query-version change detection over a stored `Expr` baseline.

[RAIL_LAW]:
- Package: `sqlglot`
- Owns: dialect-aware SQL parsing into a typed `Expr` AST; AST traversal/search/in-place rewrite; the typed builder DSL; schema-driven optimization across the 14-rule pipeline; scope analysis; column-level lineage; AST diffing; cross-dialect transpilation; the `Dialects` registry gate; the `SqlglotError` rail
- Accept: parse/transpile/optimize/qualify/lineage/diff and dialect admission feeding the data rail's query-IR, SQL-gate, and provenance owners; the builder DSL for programmatic SQL construction; scope analysis for source resolution
- Reject: wrapper-renames of `parse_one`/`transpile`/`optimize`/`lineage`/`diff`; a hand-rolled tokenizer, parser, generator, scope-resolver, or lineage tracer; regex SQL rewriting where the AST owns structure; f-string SQL construction where the builder DSL applies; a free-string dialect bypassing `Dialect.get_or_raise`/`Dialects`; a bare `Exception` rail in place of the typed `SqlglotError` descendants; a parallel per-dialect parser or generator type
