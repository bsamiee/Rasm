# [RASM_PERSISTENCE_API_PG_SEARCH]

`pg_search` mints the `bm25` index access method and the `pdb` query schema over a PostgreSQL table: a Tantivy-backed BM25 engine whose builders, bare operators, and cast modifiers lower to server SQL, and whose score, snippet, and aggregate projections anchor on the index `key_field`. Every surface is server-side SQL carrying no managed assembly. It is the branch's lexical-relevance owner — the search-provisioning rail emits its index DDL and the hybrid-fusion CTE ranks its BM25 branch beside the pgvector dense branch.

## [01]-[PUBLIC_TYPES]

[PDB_TYPES]: the access method and the cast types a predicate or an indexed column composes through, stacking in cast order.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                   |
| :-----: | :--------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `bm25`                       | access method  | index kind a `CREATE INDEX ... USING` declares |
|  [02]   | `pdb.fuzzy(int, bool, bool)` | cast type      | edit distance, prefix mode, transposition cost |
|  [03]   | `pdb.boost(float)`           | cast type      | relevance-weight multiplier                    |
|  [04]   | `pdb.const(float)`           | cast type      | constant score for the wrapped predicate       |
|  [05]   | `pdb.slop(int)`              | cast type      | phrase-position slack                          |
|  [06]   | `pdb.unicode_words`          | tokenizer cast | default word-boundary split, lowercased        |
|  [07]   | `pdb.ngram(int, int)`        | tokenizer cast | gram windows for partial matching              |
|  [08]   | `pdb.icu`                    | tokenizer cast | Unicode segmentation across mixed languages    |
|  [09]   | `pdb.whitespace`             | tokenizer cast | whitespace split                               |

- `pdb.fuzzy`: second and third arguments default `f`, so `pdb.fuzzy(1)` is `pdb.fuzzy(1, f, f)`; highlighting does not apply to a fuzzy match.

## [02]-[ENTRYPOINTS]

`CREATE INDEX <name> ON <table> USING bm25 (<key_field>, <col>, …) WITH (key_field = '<col>')` mints the one BM25 index a table carries: `key_field` leads the column list, carries a `UNIQUE` constraint, and stays untokenized when it is a text field. Index every column reached by search, sort, group, filter, or aggregation, and select a per-column tokenizer as a cast on the indexed expression — `(description::pdb.icu)`.

[MATCH_SURFACE]: `@@@` takes a column or the `key_field` on its left and a `pdb` builder on its right; a bare operator takes a literal, a text array of finalized tokens, or a cast-wrapped literal.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `col @@@ <builder>`                                        | operator | route a column or key to a `pdb` builder       |
|  [02]   | `col \|\|\| 'a b'`                                         | operator | any tokenized term — disjunction               |
|  [03]   | `col &&& 'a b'`                                            | operator | all tokenized terms — conjunction              |
|  [04]   | `col === 'term'`                                           | operator | one finalized token, un-analyzed               |
|  [05]   | `col === ARRAY['a', 'b']`                                  | operator | term set — any one finalized token             |
|  [06]   | `col ### 'a b'`                                            | operator | ordered phrase over token positions            |
|  [07]   | `col @@@ ('a' ## n ## 'b')`                                | operator | terms within n tokens, either order            |
|  [08]   | `col @@@ ('a' ##> n ##> 'b')`                              | operator | terms within n tokens, left term first         |
|  [09]   | `pdb.parse('q', lenient, conjunction_mode)`                | builder  | Tantivy query-string parse over user text      |
|  [10]   | `pdb.regex('pattern')`                                     | builder  | regex term match                               |
|  [11]   | `pdb.regex_phrase(ARRAY['re'], slop, max_expansions)`      | builder  | ordered sequence of regex terms                |
|  [12]   | `pdb.phrase_prefix(ARRAY['a', 'pre'], max_expansions)`     | builder  | phrase with a prefix-expanded last term        |
|  [13]   | `pdb.more_like_this(<key>, ARRAY['col'], max_query_terms)` | builder  | similar-document retrieval off a key value     |
|  [14]   | `pdb.range_term(<value>)`                                  | builder  | ranges containing the value                    |
|  [15]   | `pdb.range_term(<range>, '<relation>')`                    | builder  | range relation against the query range         |
|  [16]   | `pdb.all()`                                                | builder  | force a Top-K or aggregate plan onto the index |
|  [17]   | `pdb.prox_regex('pattern', max_expansions)`                | operand  | regex token inside a proximity expression      |
|  [18]   | `pdb.prox_array('a', 'b')`                                 | operand  | token alternatives inside a proximity form     |

- `pdb.range_term`: relation values are `Within`, `Intersects`, and `Contains`; the query range casts to the column's range type.
- `pdb.more_like_this`: takes the source row's `key_field` value first, matches every indexed field by default, and ignores JSON fields.

[PROJECTIONS]: relevance, highlight, and facet surfaces over the matched set, each anchored on the index `key_field`. `pdb.snippet` and `pdb.snippets` default `start_tag => '<b>'`, `end_tag => '</b>'`, and `max_num_chars => 150`.

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `pdb.score(<key_col>)`                                                             | function | BM25 relevance score                 |
|  [02]   | `pdb.snippet(col, start_tag, end_tag, max_num_chars)`                              | function | single best highlighted fragment     |
|  [03]   | `pdb.snippets(col, start_tag, end_tag, max_num_chars, "limit", "offset", sort_by)` | function | ranked array of fragments            |
|  [04]   | `pdb.snippet_positions(col)`                                                       | function | `[start, end)` byte-offset pairs     |
|  [05]   | `pdb.agg('<es-json>')`                                                             | function | Elasticsearch-shaped facet aggregate |

- `pdb.agg`: reads the index's columnar side and composes with `GROUP BY`; several aggregates ride one target list as sibling projections.
- Highlighting costs a per-row extraction, so a snippet query carries a `LIMIT` that bounds the fragment count.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every match lowers to one expression whose left side is an admitted column or the declared `key_field` and whose right side is a builder, a literal, or a cast chain; each cast wraps its inner predicate and appends its own cast, so composition is structural rather than string concatenation at the call site.
- Score and snippet projections read the posting lists the match already consumed, so relevance and highlighting are projections over the matched set rather than a second scan.

[STACKING]:
- `api-pgvector-ef`(`.api/api-pgvector-ef.md`): the BM25 branch orders by `pdb.score(<key_col>)` while the dense branch orders by `VectorDbFunctionsExtensions.CosineDistance`; one reciprocal-rank-fusion CTE sums `1.0 / (k + rank)` across both, fusing lexical relevance and vector similarity into one top-k with no learned reranker.
- `api-pgvectorscale`(`.api/api-pgvectorscale.md`): `diskann` and `bm25` are peer access methods over the same rows, and the `key_field` value is the identity both branches project, so the fusion re-queries the row store once instead of materializing two candidate payloads.
- `api-npgsql-ef`(`.api/api-npgsql-ef.md`): index DDL lands as raw SQL on the migration rail and every `pdb` projection rides `RelationalDatabaseFacadeExtensions.SqlQuery<T>(FormattableString)`, so the lexical lane reaches the caller as a composable `IQueryable<T>`.
- `api-timescaledb`(`.api/api-timescaledb.md`): `pg_search` rides one `shared_preload_libraries` row beside the other preload-gated extensions, never a self-provisioned `CREATE EXTENSION` annotation.
- Within-lib: the search-lane owner closes this whole surface as one union whose `Sql(column)` switch emits the exact expression — a new builder, operator, or cast is one case, and the tokenizer and modifier axes compose inside the case rather than as sibling methods; identifiers admit through the `Identifier` trust gate and every free-text payload crosses the one quote-doubling literal seam.

[LOCAL_ADMISSION]:
- `pg_search` runs in-process inside the PostgreSQL server tier, so the AGPL boundary stops at the database deployment and the folder composes the extension as server SQL alone.
- Lexical relevance selects the BM25 arm wherever the server profile preloads the extension; a profile without it selects the native `ts_rank` arm inside the same fusion CTE, so the fused result stays correct at reduced lexical power and the arm taken is branch-lineage evidence.
