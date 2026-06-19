# [RASM_PERSISTENCE_API_PG_SEARCH]

`pg_search` (ParadeDB) supplies the `bm25` index access method and the `pdb` query-builder schema —
a Tantivy-backed BM25 full-text engine providing high-power lexical relevance over a PostgreSQL
table beside the always-present native `tsvector`/`ts_rank` baseline. It carries no managed assembly:
every surface is server-side SQL the `provisioning#SEARCH_PROVISIONING` `Bm25Predicate`/`IndexSpec.Bm25`
fold emits and the `data-lanes#SEARCH_LANES` `HybridRetrieve.Fuse` BM25 branch matches through. The
0.24.0 line removed the legacy `paradedb.*` namespace — only `pdb.*` builders and the bare column
operators are emitted, and `paradedb.*` is asserted absent. The extension is preload-gated (it rides
the `ClusterConfig` `shared_preload_libraries` row), runs in-process inside the PG18 server tier under
its AGPL boundary at the DB deployment, and is never linked into managed code.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_search`
- package: `pg_search` (ParadeDB) (server-side PostgreSQL extension, not a NuGet package)
- access method: `bm25`
- namespace: SQL (`pdb` schema; `@@@` operator; `|||`/`&&&`/`===`/`###` column operators)
- asset: server extension, preload-gated (`shared_preload_libraries`)
- rail: search-provisioning, search-lanes, hybrid-fusion

## [2]-[INDEX_DDL]

`CREATE INDEX <name> ON <table> USING bm25 (<key_field>, <text-cols...>) WITH (key_field = '<col>')`.
The `key_field` column is `UNIQUE`/primary-key and listed first. One BM25 index per table is the
constraint. The `key_field` is the join anchor `pdb.score`/`pdb.snippet` reference.

## [3]-[MATCH_OPERATORS]

The `@@@` operator matches a column against a `pdb.*` builder on its right. The bare column operators
match without `@@@` and take a literal on the right.

| [INDEX] | [SURFACE]            | [FORM]                   | [SEMANTICS]                          |
| :-----: | :------------------- | :----------------------- | :----------------------------------- |
|   [1]   | `@@@` + builder      | `col @@@ pdb.parse('q')` | match column against a `pdb` builder |
|   [2]   | `\|\|\|` (any-token) | `col \|\|\| 'a b'`       | any of the tokens                    |
|   [3]   | `&&&` (all-token)    | `col &&& 'a b'`          | all of the tokens                    |
|   [4]   | `===` (exact-term)   | `col === 'term'`         | exact un-analyzed term               |
|   [5]   | `###` (phrase)       | `col ### 'a b'`          | ordered phrase                       |

## [4]-[PDB_BUILDERS]

The `pdb.*` query builders the `Bm25Predicate` union projects to the right of `@@@`. The cast-wrapper
modifiers (`::pdb.*`) compose over any inner predicate.

| [INDEX] | [BUILDER]            | [SIGNATURE]                                                                | [SEMANTICS]                           |
| :-----: | :------------------- | :------------------------------------------------------------------------- | :------------------------------------ |
|   [1]   | `pdb.parse`          | `pdb.parse('q', lenient => bool, conjunction_mode => bool)`                | free-text query-string parse          |
|   [2]   | `pdb.range_term`     | `pdb.range_term('v', relation => 'r', range_type => 't')`                  | range-membership term                 |
|   [3]   | `pdb.phrase_prefix`  | `pdb.phrase_prefix(ARRAY['a','b'], max_expansions => n)`                   | phrase with prefix-expanded last term |
|   [4]   | `pdb.more_like_this` | `pdb.more_like_this('doc_id', fields => ARRAY[...], max_query_terms => n)` | similar-document retrieval            |
|   [5]   | `pdb.regex`          | `pdb.regex('pattern')`                                                     | regex term match                      |
|   [6]   | `pdb.all`            | `pdb.all()`                                                                | match-all                             |
|   [7]   | `::pdb.fuzzy`        | `<inner>::pdb.fuzzy(distance, prefix, transposition_cost_one)`             | fuzzy edit-distance modifier          |
|   [8]   | `::pdb.boost`        | `<inner>::pdb.boost(factor)`                                               | relevance-weight modifier             |
|   [9]   | `::pdb.const`        | `<inner>::pdb.const(score)`                                                | constant-score modifier               |
|  [10]   | `::pdb.slop`         | `<inner>::pdb.slop(distance)`                                              | phrase-proximity slack modifier       |

## [5]-[SCORE_SNIPPET]

The relevance and highlight projections, anchored on the index `key_field` column. Each is raw SQL
through `FromSql`/`SqlQuery`, never an EF-translated member.

| [INDEX] | [FUNCTION]              | [SIGNATURE]                                                                        | [RETURNS]                |
| :-----: | :---------------------- | :--------------------------------------------------------------------------------- | :----------------------- |
|   [1]   | `pdb.score`             | `pdb.score(<key_col>)`                                                             | BM25 relevance score     |
|   [2]   | `pdb.snippet`           | `pdb.snippet(col, start_tag, end_tag, max_num_chars)`                              | one highlighted fragment |
|   [3]   | `pdb.snippets`          | `pdb.snippets(col, max_num_chars, "limit", "offset", sort_by, start_tag, end_tag)` | ranked fragment set      |
|   [4]   | `pdb.snippet_positions` | `pdb.snippet_positions(col)`                                                       | match-position offsets   |
|   [5]   | `pdb.agg`               | `pdb.agg('<es_json>') OVER ()`                                                     | Elasticsearch-style aggs |
