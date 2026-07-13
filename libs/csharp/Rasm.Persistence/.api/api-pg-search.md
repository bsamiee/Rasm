# [RASM_PERSISTENCE_API_PG_SEARCH]

`pg_search` (ParadeDB) supplies the `bm25` index access method and the `pdb` query-builder schema —
a Tantivy-backed BM25 full-text engine providing high-power lexical relevance over a PostgreSQL
table beside the always-present native `tsvector`/`ts_rank` baseline. It carries no managed assembly:
every surface is server-side SQL the `Store/provisioning#SERVER_EXTENSIONS` `Bm25Predicate`/`IndexSpec.Bm25`
fold emits and the `Query/retrieval#FUSION_AND_REUSE` `FusionRank.Fuse` BM25 branch matches through. Only
the `pdb.*` builders and the bare column operators are emitted; `paradedb.*` is absent. The extension is
preload-gated (it rides the `ClusterConfig` `shared_preload_libraries` row), runs in-process inside the
PG18 server tier under its AGPL boundary at the DB deployment, and is never linked into managed code.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_search`
- package: `pg_search` (ParadeDB) — server-side PostgreSQL extension, not a NuGet package
- version line: `0.24.x` (v2 `pdb.*` API; `paradedb.*` removed)
- license: AGPL-3.0 — confined to the PG server tier; never linked into managed code
- access method: `bm25` (`USING bm25`)
- abi / runtime: PG18 in-process extension, `shared_preload_libraries`-gated; built on `pgrx`/Tantivy
- namespace: SQL `pdb` schema; `@@@` match operator; `|||`/`&&&`/`===`/`###` bare column operators; `##`/`##>` proximity operators inside `@@@`
- asset: server extension, preload-gated
- rail: search-provisioning, search-lanes, hybrid-fusion

## [02]-[INDEX_DDL]

`CREATE INDEX <name> ON <table> USING bm25 (<key_field>, <text-cols...>) WITH (key_field = '<col>')`.
The `key_field` column (a) carries a `UNIQUE` constraint — usually the table `PRIMARY KEY`, (b) is
listed first in the column list, and (c) is untokenized if it is a text field. Exactly one BM25 index
per table is the constraint. Index every column that appears in search, sort, group, filter, or
aggregation — most PG types are admitted (text, json, numeric, timestamp, range, boolean, array). The
`key_field` is the join anchor every `pdb.score`/`pdb.snippet` projection references.

## [03]-[MATCH_OPERATORS]

The `@@@` operator matches a column (or `key_field`, for document-level builders) against a `pdb.*`
builder on its right. The bare column operators match a literal directly without `@@@`. The proximity
operators compose inside an `@@@` expression.

| [INDEX] | [SURFACE]            | [FORM]                        | [SEMANTICS]                               |
| :-----: | :------------------- | :---------------------------- | :---------------------------------------- |
|  [01]   | `@@@` + builder      | `col @@@ pdb.parse('q')`      | match column/key against a `pdb` builder  |
|  [02]   | `\|\|\|` (any-token) | `col \|\|\| 'a b'`            | any of the tokens (disjunction)           |
|  [03]   | `&&&` (all-token)    | `col &&& 'a b'`               | all of the tokens (conjunction)           |
|  [04]   | `===` (exact-term)   | `col === 'term'`              | exact un-analyzed token match             |
|  [05]   | `###` (phrase)       | `col ### 'a b'`               | ordered phrase (term presence + position) |
|  [06]   | `##` (proximity)     | `col @@@ ('a' ## 2 ## 'b')`   | terms within N tokens, any order          |
|  [07]   | `##>` (ordered prox) | `col @@@ ('a' ##> 2 ##> 'b')` | terms within N tokens, left term first    |

## [04]-[PDB_BUILDERS]

The `pdb.*` query builders the `Bm25Predicate` union projects to the right of `@@@`; each `[SIGNATURE]`
drops the leading builder name carried in `[BUILDER]`. The cast-wrapper modifiers (`::pdb.*`) compose
over any inner predicate and stack in cast order (`'shose'::pdb.fuzzy(2)::pdb.boost(2)` applies typo
tolerance then a score multiplier); `::pdb.fuzzy` allows `distance` up to 2 with `prefix`/
`transposition_cost_one` defaulting `f`.

| [INDEX] | [BUILDER]            | [SIGNATURE]                                                      | [SEMANTICS]                               |
| :-----: | :------------------- | :--------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `pdb.parse`          | `('q', lenient => bool, conjunction_mode => bool)`               | free-text Tantivy query-string parse      |
|  [02]   | `pdb.match`          | `('q', distance => n, prefix => bool, conjunction_mode => bool)` | analyzed per-field fuzzy match            |
|  [03]   | `pdb.range_term`     | `('v', relation => 'r', range_type => 't')`                      | range-membership term                     |
|  [04]   | `pdb.phrase_prefix`  | `(ARRAY['a','b'], max_expansions => n)`                          | phrase with prefix-expanded last term     |
|  [05]   | `pdb.more_like_this` | `('doc_id', fields => ARRAY[...], max_query_terms => n)`         | similar-document retrieval (key-anchored) |
|  [06]   | `pdb.regex`          | `('pattern')`                                                    | regex term match                          |
|  [07]   | `pdb.all`            | `()`                                                             | match-all                                 |
|  [08]   | `::pdb.fuzzy`        | `<inner>::pdb.fuzzy(distance, prefix, transposition_cost_one)`   | fuzzy edit-distance modifier              |
|  [09]   | `::pdb.boost`        | `<inner>::pdb.boost(factor)`                                     | relevance-weight modifier                 |
|  [10]   | `::pdb.const`        | `<inner>::pdb.const(score)`                                      | constant-score modifier                   |
|  [11]   | `::pdb.slop`         | `<inner>::pdb.slop(distance)`                                    | phrase-proximity slack modifier           |

Analyzed (tokenized) matching has two forms: the per-field `pdb.match` builder of row `[02]` (carrying
its own fuzzy `distance`/`prefix`, the `Bm25Predicate.Match` case) on the right of `@@@`, and the bare
`|||`/`&&&` column operators of section `[03]` (optionally with a tokenizer cast, e.g.
`'running shoes'::pdb.whitespace`) the `Bm25Predicate` `AnyToken`/`AllToken` cases own.

## [05]-[SCORE_SNIPPET]

The relevance and highlight projections, anchored on the index `key_field` column. Each is raw SQL
through `FromSql`/`SqlQuery`, never an EF-translated member; each `[SIGNATURE]` drops the leading
function name carried in `[FUNCTION]`. The snippet functions default `start_tag => '<b>'`,
`end_tag => '</b>'`, `max_num_chars => 150`, and `pdb.snippets` adds `sort_by => 'score'`.

| [INDEX] | [FUNCTION]              | [SIGNATURE]                                                            | [RETURNS]                         |
| :-----: | :---------------------- | :--------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `pdb.score`             | `(<key_col>)`                                                          | BM25 relevance score              |
|  [02]   | `pdb.snippet`           | `(col, start_tag, end_tag, max_num_chars)`                             | one highlighted fragment          |
|  [03]   | `pdb.snippets`          | `(col, start_tag, end_tag, max_num_chars, "limit", "offset", sort_by)` | ranked fragment set               |
|  [04]   | `pdb.snippet_positions` | `(col)`                                                                | match-position offsets            |
|  [05]   | `pdb.agg`               | `('<es_json>') OVER ()`                                                | Elasticsearch-style aggs / facets |

## [06]-[STACKING]

- The `Query/retrieval#LEXICAL_ALGEBRA` `Bm25Predicate` `[Union]` is the C# projection of section
  `[04]` — one union case per builder/operator/cast, `Bm25Predicate.Sql()` switching to the exact SQL
  string; the `SearchProjection` static surface is the C# projection of section `[05]`
  (`Score`/`Snippet`/`Snippets`/`SnippetPositions`/`Agg`). A new builder, operator, or cast is one
  union case, never a sibling method, so the catalog's member set is the union's case roster.
- The `Query/retrieval#FUSION_AND_REUSE` `FusionRank.Fuse` composes the BM25 route ONTO the pgvector dense
  route inside one reciprocal-rank-fusion CTE: the BM25 branch matches `corpus @@@ pdb.parse($terms)`
  and orders by `pdb.score(<key_col>)` (the index's declared `key_field` anchor), the vector branch
  orders by the `EmbeddingArity` distance operator, and `1.0 / (rrfConstant + rank)` is summed across
  branches — so BM25 lexical relevance and pgvector semantic similarity fuse into one top-k without a
  learned reranker, and a profile without `pg_search` preloaded degrades the BM25 branch to the native
  `ts_rank` baseline inside the same CTE (the fused result stays correct at reduced lexical power).
- BM25 carries no EF translator, so the index DDL lands via raw `MigrationBuilder.Sql` on the EF
  migration rail (`Element/identity#SCHEMA_VERDICT`) and every query projection rides `FromSql`/`SqlQuery` on the same boundary as the
  native `websearch_to_tsquery`/`ts_rank`/`ts_headline` baseline; the `key_field` join anchor is the
  content key the fusion re-queries the row store by, so the fusion projects identities rather than
  re-materializing both candidate payloads.
- Query values arrive pre-escaped from the search-lane binder (the `Bm25Predicate` constructors carry
  already-bound string columns/terms), never raw runtime input; the AGPL boundary stays the DB
  deployment because `pg_search` runs in-process inside the PG server and is never linked into managed
  code.
