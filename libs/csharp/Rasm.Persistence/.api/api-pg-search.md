# [RASM_PERSISTENCE_API_PG_SEARCH]

`pg_search` (ParadeDB) is a PostgreSQL extension that adds the `bm25` index access
method for full-text BM25 relevance search, column-level match operators, query-builder
functions (`pdb.*`), relevance scoring, and snippet highlighting.

## [1]-[EXTENSION_SURFACE]

[EXTENSION_SURFACE]: `pg_search`
- extension: `pg_search`
- access method: `bm25`
- version: `0.24.0`
- prerequisite: `shared_preload_libraries = 'pg_search'` in `postgresql.conf` or `ALTER SYSTEM`
- asset: PostgreSQL extension (not a .NET assembly; not decompilable)
- rail: search-provisioning

[DDL_ADMISSION]:

| [INDEX] | [DDL]                         | [CAPABILITY]       |
| :-----: | :---------------------------- | :----------------- |
|   [1]   | `CREATE EXTENSION pg_search;` | installs extension |

## [2]-[INDEX_DDL]

[INDEX_ACCESS_METHOD]: `bm25`
- access method: `bm25`
- constraint: one BM25 index per table; `key_field` column must be `UNIQUE` or primary key and listed first

[CREATE_INDEX_SHAPE]:

```sql
CREATE INDEX <name> ON <table>
USING bm25 (<key_col>, <col1>, <col2>, ...)
WITH (key_field = '<key_col>');
```

[INDEX_OPTIONS]:
- used by: `server-tier#SEARCH_PROVISIONING`
- evidence: `docs.paradedb.com` and GitHub `pg_search`

| [INDEX] | [OPTION]    | [VALUE_SHAPE]    | [CAPABILITY]                |
| :-----: | :---------- | :--------------- | :-------------------------- |
|   [1]   | `key_field` | `text`, required | names the unique key column |

## [3]-[SEARCH_OPERATORS]

[COLUMN_OPERATORS]: left-hand column, right-hand query value; use without `@@@`
- used by: `data-lanes#SEARCH_LANES`
- evidence: `docs.paradedb.com`

| [INDEX] | [OPERATOR] | [QUERY_SHAPE]                   | [CAPABILITY]           |
| :-----: | :--------- | :------------------------------ | :--------------------- |
|   [1]   | `\|\|\|`   | `<col> \|\|\| <query>`          | any-token match        |
|   [2]   | `&&&`      | `<col> &&& <query>`             | all-token match        |
|   [3]   | `===`      | `<col> === <term>`              | exact token match      |
|   [4]   | `###`      | `<col> ### <phrase>`            | ordered phrase match   |
|   [5]   | `@@@`      | `<col> @@@ pdb.<function>(...)` | query-builder dispatch |

Array query values are admitted for `|||`, `&&&`, and `===`; `###` also admits `::pdb.slop(<n>)`.

[FUZZY_CAST]: applied as a cast modifier on the query string value
- used by: `data-lanes#SEARCH_LANES`
- evidence: `docs.paradedb.com`

| [INDEX] | [CAST]        | [ARGUMENTS]                                                  | [CAPABILITY]     |
| :-----: | :------------ | :----------------------------------------------------------- | :--------------- |
|   [1]   | `::pdb.fuzzy` | `distance int`, `prefix bool`, `transposition_cost_one bool` | fuzzy term match |

`::pdb.fuzzy` applies to a query string value and admits edit distance `0` through `2`.

## [4]-[QUERY_BUILDERS]

[QUERY_BUILDER_SURFACE]: `pdb.*` functions used on the right-hand side of `@@@`
- used by: `data-lanes#SEARCH_LANES`
- evidence: `docs.paradedb.com`

| [INDEX] | [FUNCTION]           | [ARGUMENT_SCOPE]                | [CAPABILITY]            |
| :-----: | :------------------- | :------------------------------ | :---------------------- |
|   [1]   | `pdb.all()`          | none                            | unfiltered index scan   |
|   [2]   | `pdb.regex`          | pattern                         | regex field match       |
|   [3]   | `pdb.parse`          | query string and parse mode     | raw-query parser        |
|   [4]   | `pdb.phrase_prefix`  | token array and expansion cap   | phrase-prefix match     |
|   [5]   | `pdb.range_term`     | value, relation, and range type | range predicate         |
|   [6]   | `pdb.more_like_this` | document-similarity controls    | similar-document search |

[QUERY_BUILDER_CONTRACTS]:

```sql
pdb.all()
pdb.regex(pattern text)
pdb.parse(query_string text, lenient boolean DEFAULT false, conjunction_mode boolean DEFAULT false)
pdb.phrase_prefix(tokens text[], max_expansions integer DEFAULT 50)
pdb.range_term(value, relation text DEFAULT 'Contains', range_type text DEFAULT NULL)
pdb.more_like_this(
    document_id,
    fields text[] DEFAULT NULL,
    min_term_frequency int DEFAULT NULL,
    max_term_frequency int DEFAULT NULL,
    min_doc_frequency int DEFAULT NULL,
    max_doc_frequency int DEFAULT NULL,
    max_query_terms int DEFAULT NULL,
    min_word_length int DEFAULT NULL,
    max_word_length int DEFAULT NULL,
    stopwords text[] DEFAULT NULL
)
```

[RELEVANCE_CAST_MODIFIERS]: composable cast modifiers on any query value
- used by: `data-lanes#SEARCH_LANES`
- evidence: `docs.paradedb.com`

| [INDEX] | [CAST]        | [ARGUMENT]     | [CAPABILITY]         |
| :-----: | :------------ | :------------- | :------------------- |
|   [1]   | `::pdb.boost` | `factor float` | score multiplier     |
|   [2]   | `::pdb.const` | `score float`  | constant match score |

`::pdb.boost` accepts factors from `-2048` through `2048`.

## [5]-[SCORING_AND_HIGHLIGHTING]

[SCORE_SURFACE]: relevance score
- used by: `data-lanes#SEARCH_LANES`
- evidence: `docs.paradedb.com`

| [INDEX] | [FUNCTION]  | [RETURN] | [CAPABILITY]         |
| :-----: | :---------- | :------- | :------------------- |
|   [1]   | `pdb.score` | `float4` | BM25 relevance score |

`pdb.score(<key_col>)` is composable across joins.

[HIGHLIGHT_SURFACE]: snippet and position functions
- used by: `data-lanes#SEARCH_LANES`
- evidence: `docs.paradedb.com`

| [INDEX] | [FUNCTION]              | [RETURN]  | [CAPABILITY]             |
| :-----: | :---------------------- | :-------- | :----------------------- |
|   [1]   | `pdb.snippet`           | `text`    | best highlighted snippet |
|   [2]   | `pdb.snippets`          | `text[]`  | ranked snippet set       |
|   [3]   | `pdb.snippet_positions` | `int[][]` | match-span byte offsets  |

[HIGHLIGHT_CONTRACTS]:

```sql
pdb.snippet(<col>, start_tag text DEFAULT '<b>', end_tag text DEFAULT '</b>', max_num_chars integer DEFAULT 150)
pdb.snippets(<col>, max_num_chars integer DEFAULT 150, "limit" integer DEFAULT 5, "offset" integer DEFAULT 0, sort_by text DEFAULT 'score', start_tag text DEFAULT '<b>', end_tag text DEFAULT '</b>')
pdb.snippet_positions(<col>)
```

Note: highlighting is not supported for fuzzy queries.

## [6]-[AGGREGATION]

[AGGREGATION_SURFACE]: Elasticsearch-compatible window aggregation
- used by: `data-lanes#SEARCH_LANES`
- evidence: `docs.paradedb.com`

| [INDEX] | [FUNCTION] | [SQL_SHAPE]                       | [CAPABILITY]                   |
| :-----: | :--------- | :-------------------------------- | :----------------------------- |
|   [1]   | `pdb.agg`  | `pdb.agg(<es_json text>) OVER ()` | ES-compatible window aggregate |

## [7]-[DEPRECATED_SURFACE]

[PHANTOM_GUARD]: the following names appear in older documentation and training data but do NOT exist in `pg_search` 0.24.0:

| [INDEX] | [PHANTOM]               | [REPLACED_BY]                            |
| :-----: | :---------------------- | :--------------------------------------- |
|   [1]   | `paradedb.match()`      | column `\|\|\|` / `&&&` operators        |
|   [2]   | `paradedb.term()`       | column `===` operator                    |
|   [3]   | `paradedb.phrase()`     | column `###` operator                    |
|   [4]   | `paradedb.boolean()`    | `pdb.parse()` with boolean query string  |
|   [5]   | `paradedb.fuzzy_term()` | `'<term>'::pdb.fuzzy(...)` cast modifier |
|   [6]   | `paradedb.range()`      | `pdb.range_term()`                       |
|   [7]   | `paradedb.score()`      | `pdb.score()`                            |
|   [8]   | `paradedb.snippet()`    | `pdb.snippet()`                          |

## [8]-[IMPLEMENTATION_LAW]

[SEARCH_PROVISIONING]:
- extension: `pg_search`
- index root: `bm25` access method; one index per table
- `key_field` is the join anchor for `pdb.score()` and `pdb.snippet()` projections
- column operators (`|||`, `&&&`, `===`, `###`) execute without `@@@`; use `@@@` only with `pdb.*` query builders
- `pdb.score()` is composable across joins: `pdb.score(t1.id) + pdb.score(t2.id)` is valid
- `pdb.agg()` accepts Elasticsearch-compatible aggregation DSL as a window function

[RAIL_LAW]:
- Extension: `pg_search`
- Owns: BM25 full-text search index and relevance scoring
- Accept: profile-declared BM25 indexes and query-scoped search predicates
- Reject: `paradedb.*` namespace (removed); extension DDL outside store-profile migrations
