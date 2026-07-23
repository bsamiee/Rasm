# [PY_DATA_API_SQLGLOT]

`sqlglot` is the pure-Python SQL tokenizer, parser, transpiler, and optimizer for the data rail's query-IR, SQL-gate, and lineage concerns, with no database connection. `parse_one` builds an `Expr` syntax tree, `Expr.sql`/`transpile` regenerate it under any dialect, `optimizer.optimize` canonicalizes against a schema, `lineage.lineage` traces a column to its sources, and `diff` computes the AST edit set between two trees. `Dialect.get_or_raise` gates every dialect over the closed `Dialects` vocabulary, and every failure descends from `SqlglotError`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sqlglot`
- package: `sqlglot`
- module: `sqlglot`
- owner: `data`
- rail: query-ir

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: AST node, dialect gate, engine triad, scope, lineage, diff, and the error rail

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Expr`                          | AST node      | syntax-tree root (`sqlglot.expressions.Expression`); `exp` is the subtype namespace |
|  [02]   | `Dialect`                       | dialect       | resolved dialect instance backing parse/generate                                    |
|  [03]   | `Dialects`                      | enum          | `str` enum of the supported dialect names plus the empty default                    |
|  [04]   | `ErrorLevel`                    | enum          | `IGNORE`/`WARN`/`RAISE`/`IMMEDIATE` parser error policy                             |
|  [05]   | `Schema`                        | schema        | abstract column/type schema (`abc.ABC`)                                             |
|  [06]   | `MappingSchema`                 | schema        | mapping-backed `Schema` (`AbstractMappingSchema`)                                   |
|  [07]   | `Parser`                        | engine        | dialect parser producing `Expr` collections                                         |
|  [08]   | `Generator`                     | engine        | `Expr`-to-SQL string generator                                                      |
|  [09]   | `Tokenizer`/`Token`/`TokenType` | engine        | source-to-`Token` lexer, the lexeme, and the token-kind vocabulary                  |
|  [10]   | `optimizer.scope.Scope`         | scope         | resolved name scope from `build_scope`; `find_all_in_scope` walks it                |
|  [11]   | `lineage.Node`                  | lineage node  | column-lineage tree node (`name`/`source`/`downstream`); root from `lineage()`      |
|  [12]   | `diff.Edit`                     | diff op       | one AST edit `Insert`/`Remove`/`Move`/`Update`/`Keep` from `diff(source, target)`   |
|  [13]   | `exp.*` clause nodes            | AST subtype   | `exp.Where`/`exp.Having`/`exp.Qualify`/`exp.Join`/`exp.Column` clause/column nodes  |

[Error rail]: `SqlglotError` (root) `ParseError` (`errors` detail list) `TokenError` `UnsupportedError` `OptimizeError` `SchemaError`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, transpile, build, and dialect gate

| [INDEX] | [SURFACE]                                       | [SHAPE] | [CAPABILITY]                                             |
| :-----: | :---------------------------------------------- | :------ | :------------------------------------------------------- |
|  [01]   | `parse(sql, read, dialect) -> list[Expr\|None]` | static  | parse multi-statement SQL into trees                     |
|  [02]   | `parse_one(sql, read, dialect, into) -> Expr`   | static  | parse one statement, optionally `into` a subtype         |
|  [03]   | `maybe_parse(sql_or_expr, *, into, dialect)`    | static  | parse a string, pass an existing `Expr` through          |
|  [04]   | `transpile(sql, read, write, identity)`         | static  | rewrite SQL from `read` to `write` dialect               |
|  [05]   | `tokenize(sql, read, dialect) -> list[Token]`   | static  | lex without parsing                                      |
|  [06]   | `find_tables(expr) -> set[Table]`               | static  | collect non-CTE tables referenced                        |
|  [07]   | `Dialect.get_or_raise(dialect) -> Dialect`      | factory | resolve a name, settings string, or instance, else raise |

[Builder DSL]: construct an `Expr` tree programmatically (`select` returns `Select`), never f-string concatenation — `select` `from_` `column` `func` `cast` `case` `condition` `and_` `or_` `not_` `alias` `table` `subquery` `insert` `delete` `merge` `union` `intersect` `except_` `to_column` `to_table` `to_identifier`

[ENTRYPOINT_SCOPE]: `Expr` generate, search, rewrite, optimize, scope, lineage, diff

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `Expr.sql(dialect, copy) -> str`                           | instance | regenerate SQL for this tree under `dialect`              |
|  [02]   | `Expr.find(*types, bfs)` / `find_all(*types) -> Iterator`  | instance | first / all nodes of the given subtypes                   |
|  [03]   | `Expr.walk(bfs, prune)` / `find_ancestor(*types)`          | instance | visit every node (prunable) / climb to an ancestor        |
|  [04]   | `Expr.transform(fn, copy) -> Expr` / `node.replace(other)` | instance | bottom-up rewrite / swap a node in its parent             |
|  [05]   | `optimizer.optimize(expr, schema, dialect, rules) -> Expr` | static   | run the `RULES` pipeline to schema-qualified form         |
|  [06]   | `optimizer.qualify(expr, schema, dialect) -> Expr`         | static   | normalize and qualify tables/columns; mandatory pre-step  |
|  [07]   | `optimizer.scope.build_scope(expr) -> Scope\|None`         | static   | resolve and walk name scopes (source and alias maps)      |
|  [08]   | `lineage.lineage(column, sql, schema, sources) -> Node`    | static   | column-level lineage tree back to source columns          |
|  [09]   | `diff(source, target, matchings, delta_only) -> list[Edit]`| static   | AST edit set between two trees                            |
|  [10]   | `Select.selects -> list[Expr]`                             | property | projection list; each `alias_or_name` is an output column |

[OPTIMIZER_RULES]: `optimizer.RULES` is the ordered pipeline `optimize` runs, each rule dispatched by introspected parameter name and a sliced `rules=` tuple running a subset — `qualify` (mandatory first) `pushdown_projections` `normalize` `unnest_subqueries` `pushdown_predicates` `optimize_joins` `eliminate_subqueries` `merge_subqueries` `eliminate_joins` `eliminate_ctes` `quote_identifiers` `annotate_types` `canonicalize` `simplify`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `import sqlglot` at module scope; the pure-Python parse/transpile plane is a module-top substrate corpus-wide.
- `parse_one` builds single-statement IR and `parse` multi-statement; `read`/`dialect` selects the dialect as a `DialectType` argument, and `maybe_parse` passes an already-built `Expr` through without re-parsing.
- New SQL constructs through the typed builder DSL returning an `Expr`; `Expr.transform`/`replace` rewrite an existing tree in place; `tokenize` is the lexeme-level row.
- `Expr.sql(dialect=...)` is the single generation surface, cross-dialect output a `dialect` argument; `transpile` is the parse-then-generate composite keyed by `read`/`write`.
- `Dialect.get_or_raise` is the single admit/reject gate; a target is a `Dialects` member, a name string with optional `k = v` settings, or a `Dialect` instance, and an unknown name raises.
- `find`/`find_all`/`walk`/`find_ancestor` own AST search by node type and BFS/DFS flag; `find_tables` collects tables; `optimizer.scope.build_scope`/`traverse_scope`/`find_all_in_scope` own source and alias resolution.
- `optimize` runs the `RULES` pipeline with `qualify` first; `schema` is a `{table:{col:type}}`/`{db:...}`/`{catalog:...}` mapping or a `Schema`, and a subset run slices `rules=`.
- `lineage.lineage(column, sql, schema=...)` produces the column-lineage `Node` tree; `diff(source, target)` produces the `Edit` list for query-evolution and migration analysis.
- Every failure descends from `SqlglotError` over the `ParseError`/`TokenError`/`UnsupportedError`/`OptimizeError`/`SchemaError` rows; `ErrorLevel` selects the parser policy; the boundary maps these onto the data rail's typed result.
- Each op emits a query-IR receipt: parse keys dialect and statement/node counts, transpile the `read`/`write` dialects, optimize the applied rule names and schema presence, lineage the target column and source-table set, diff the edit-op counts.

[STACKING]:
- `ibis-framework`(`.api/ibis-framework.md`): compiles its expression graph to SQL through sqlglot, so centralizing sqlglot here pins the `ibis` transitive SQL backend.
- `duckdb`(`.api/duckdb.md`), `datafusion`(`.api/datafusion.md`): receive `transpile`-output SQL keyed by the engine's `Dialects` member, transpiled to the engine dialect rather than hand-written.
- within-lib: the `Expr` AST feeds the data rail's query owner directly (gated through `Dialect.get_or_raise` before generate); `lineage.lineage` maps a result column back to its physical source columns for the provenance owner, and `diff` drives query-version change detection over a stored `Expr` baseline for the migration owner.

[LOCAL_ADMISSION]:
- Build new SQL through the builder DSL; parse and rewrite through `parse_one`/`Expr.transform`; gate every dialect through `Dialect.get_or_raise` over `Dialects`; trace provenance through `lineage.lineage` and query change through `diff`.

[RAIL_LAW]:
- Package: `sqlglot`
- Owns: dialect-aware SQL parsing into a typed `Expr` AST; AST traversal, search, and in-place rewrite; the typed builder DSL; schema-driven optimization across the `RULES` pipeline; scope analysis; column-level lineage; AST diffing; cross-dialect transpilation; the `Dialects` gate; the `SqlglotError` rail
- Accept: parse/transpile/optimize/qualify/lineage/diff and dialect admission feeding the query-IR, SQL-gate, and provenance owners; the builder DSL for programmatic construction; scope analysis for source resolution
- Reject: wrapper-renames of `parse_one`/`transpile`/`optimize`/`lineage`/`diff`; a hand-rolled tokenizer, parser, generator, scope-resolver, or lineage tracer; regex SQL rewriting where the AST owns structure; f-string SQL construction where the builder DSL applies; a free-string dialect bypassing `Dialect.get_or_raise`; a bare `Exception` rail in place of the `SqlglotError` descendants
