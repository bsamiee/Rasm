# [RASM_PERSISTENCE_API_APACHE_AGE]

`apache-age` (repo `apache/age`; installed extension `age`) mints an in-PostgreSQL openCypher graph store — labelled vertices and edges in per-graph backing relations under the `ag_catalog` schema, queried through the one `cypher(graph, $$ … $$, params)` set-returning function whose rows are the `agtype` graph value type. It ships no managed assembly: every surface is server-side SQL. `Query/cypher#GRAPH_QUERY`, the optional self-hosted graph lane, drives it through raw `Npgsql`, demoted beneath the default in-process QuikGraph `Query/topology` view.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-age` / extension `age`
- package: server-side PostgreSQL extension (C, not a NuGet package); repo `apache/age`, installed name `age`
- namespace: SQL `ag_catalog` schema — functions, catalog tables, the `agtype` type and its operator/cast set
- license: Apache-2.0 — the in-DB deployment is the license boundary, no managed linkage
- registration: `CREATE EXTENSION age` installs the SQL objects; the per-session `LOAD 'age'`/`search_path` is a connection-init concern (`[02]`); install rides the `age` `ServerExtension` row (`Store/provisioning#SERVER_EXTENSIONS`)
- consumed by: `Query/cypher#GRAPH_SESSION` (enablement gate, `GraphDdl` lifecycle, `agtype` decode) and `Query/cypher#GRAPH_QUERY` (the openCypher verb surface), driven through raw `Npgsql` against the `agtype` result type
- rail: graph-provisioning, graph-lane

## [02]-[SESSION_SETUP]

`age` loads per session and needs no `shared_preload_libraries`: `CREATE EXTENSION age` succeeds without preload, and every connection running Cypher issues `LOAD 'age'` and puts `ag_catalog` on `search_path` so the bare `cypher`/`agtype` symbols resolve — preloading `age` is an optional convenience that only skips the per-session `LOAD`.

| [INDEX] | [STATEMENT]                                     | [SCOPE]           | [SEMANTICS]                                                    |
| :-----: | :---------------------------------------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `CREATE EXTENSION IF NOT EXISTS age`            | once per database | install the SQL objects into `ag_catalog` (the `CreateSql`)    |
|  [02]   | `LOAD 'age'`                                    | once per session  | load the shared library into the backend (before any Cypher)   |
|  [03]   | `SET search_path = ag_catalog, "$user", public` | per session/txn   | resolve unqualified `cypher`/`agtype` (or `SET LOCAL` per txn) |

A PL/pgSQL function running Cypher repeats `LOAD 'age'; SET search_path TO ag_catalog;` in its own body — the caller's session settings do not cross the function boundary.

## [03]-[CATALOG_SCHEMA]

`ag_catalog` holds the graph/label registry as ordinary tables keyed by the label-id/kind domains; `pg_extension_config_dump` marks them so they survive `pg_dump`.

| [INDEX] | [OBJECT]     | [SHAPE]                                                                                  | [SEMANTICS]             |
| :-----: | :----------- | :--------------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | `ag_graph`   | `(graphid oid, name name, namespace regnamespace)`                                       | one row per graph       |
|  [02]   | `ag_label`   | `(name name, graph oid, id label_id, kind label_kind, relation regclass, seq_name name)` | one row per label       |
|  [03]   | `label_id`   | `DOMAIN int CHECK (VALUE > 0 AND VALUE <= 65535)`                                        | label id `1`–`65535`    |
|  [04]   | `label_kind` | `DOMAIN "char" CHECK (VALUE = 'v' OR VALUE = 'e')`                                       | `'v'` vertex/`'e'` edge |
|  [05]   | `graphid`    | 8-byte pass-by-value id packing `label_id` + entry-id                                    | vertex/edge identity    |

## [04]-[GRAPH_LABEL_LIFECYCLE]

Graph and label DDL are `ag_catalog` `SELECT`-function calls returning `void`. `create_graph` materialises the namespace and the default `_ag_label_vertex`/`_ag_label_edge` base labels; a label's backing relation lands on first use or explicitly through `create_vlabel`/`create_elabel`.

| [INDEX] | [FUNCTION]      | [SIGNATURE]                                                            | [SEMANTICS]                               |
| :-----: | :-------------- | :--------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `create_graph`  | `create_graph('graph_name'::name)`                                     | graph namespace + base labels             |
|  [02]   | `drop_graph`    | `drop_graph('graph_name'::name, cascade => true)`                      | drop graph; `cascade` drops label tables  |
|  [03]   | `create_vlabel` | `create_vlabel('graph_name', 'label')` (args `cstring`)                | declare a vertex label / relation         |
|  [04]   | `create_elabel` | `create_elabel('graph_name', 'label')` (args `cstring`)                | declare an edge label / relation          |
|  [05]   | `drop_label`    | `drop_label('graph_name'::name, 'label'::name, force => false)`        | drop a label; `force` ignores refs        |
|  [06]   | `alter_graph`   | `alter_graph('graph_name'::name, 'RENAME'::cstring, 'new_name'::name)` | rename the graph (`operation`=`'RENAME'`) |

## [05]-[CYPHER_QUERY]

One polymorphic set-returning function runs every Cypher statement; the third `params agtype` argument carries the parameterized values, referenced inside the query body as `$name`.

```sql signature
ag_catalog.cypher(graph_name name = NULL, query_string cstring = NULL, params agtype = NULL) RETURNS SETOF record
```

`cypher` is declared `RETURNS SETOF record`, so PostgreSQL requires the caller's column-definition list — no anonymous-record default. Every projected column is typed `agtype`, and the list arity and names must match the Cypher `RETURN` clause.

| [INDEX] | [FORM]                                                                                       | [SEMANTICS]                               |
| :-----: | :------------------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `SELECT * FROM cypher('g', $$ MATCH (v {name:'A'}) RETURN v $$) AS (v agtype)`               | read; one `agtype` col per `RETURN` term  |
|  [02]   | `SELECT * FROM cypher('g', $$ CREATE (:Label {k:'v'}) $$) AS (r agtype)`                     | write; still needs a column list          |
|  [03]   | `SELECT * FROM cypher('g', $$ MATCH (n) WHERE n.id = $target RETURN n $$, $1) AS (n agtype)` | `$target` binds from `params agtype` `$1` |

AGE rejects a single Cypher statement that mutates and returns — split `CREATE`/`SET` from the trailing `MATCH … RETURN`. `$$ … $$` dollar-quoting isolates the Cypher body from SQL string escaping.

## [06]-[AGTYPE]

`agtype` is AGE's single value type — a superset of `jsonb`'s binary format extended with the exact-number kinds (`integer`/`float`/`numeric`) and the graph entities `vertex`/`edge`/`path`. A vertex or edge renders `{id, label, properties}::vertex|edge`; a path is the alternating `[vertex, edge, …]::path`.

| [INDEX] | [OPERATOR]    | [SIGNATURE]                               | [SEMANTICS]                              |
| :-----: | :------------ | :---------------------------------------- | :--------------------------------------- |
|  [01]   | `->`          | `agtype -> text\|int4\|agtype` → `agtype` | object field / array element             |
|  [02]   | `->>`         | `agtype ->> text\|int4\|agtype` → `text`  | object field / array element as text     |
|  [03]   | `#>`          | `agtype #> agtype` → `agtype`             | extract value at path                    |
|  [04]   | `#>>`         | `agtype #>> agtype` → `text`              | extract value at path as text            |
|  [05]   | `@>` / `<@`   | `agtype @> agtype` → `boolean`            | containment / contained-by (commutators) |
|  [06]   | `@>>` / `<<@` | `agtype @>> agtype` → `boolean`           | top-level-only containment               |

Casts compose at two layers: in-Cypher `expr::int|float|numeric|bool|vertex|edge|path` (agtype → agtype), and SQL-level casts against native types — `agtype::text`/`::boolean`/`::float8`/`::bigint`/`::int`/`::smallint`/`::int[]`/`::json`/`::jsonb` and their reverses — so `(cypher(…)).col::int` extracts a typed scalar from a returned `agtype` row.

## [07]-[GRAPH_ALGORITHMS]

`age` ships no `age_shortest_path` SQL routine: variable-length traversal is the `age_vle` engine the planner invokes behind Cypher `*` range patterns, so a shortest-path query is a bounded `*`-range `MATCH`, never a dedicated SQL function. Bulk loaders, the complete-graph generator, and `age_vle` all live under `ag_catalog`, returning `agtype` rows or `void`.

| [INDEX] | [FUNCTION]              | [SEMANTICS]                                                             |
| :-----: | :---------------------- | :---------------------------------------------------------------------- |
|  [01]   | `load_labels_from_file` | bulk-load vertices from a server-side CSV                               |
|  [02]   | `load_edges_from_file`  | bulk-load edges from a server-side CSV                                  |
|  [03]   | `create_complete_graph` | generate a complete graph                                               |
|  [04]   | `age_vle`               | the VLE engine the planner invokes for Cypher `*` ranges; rarely direct |
|  [05]   | `get_cypher_keywords`   | the reserved-Cypher-keyword roster (tooling/escaping aid)               |

- [01]: `load_labels_from_file(graph name, label name, file_path text, id_field_exists bool => true, load_as_agtype bool => false)` → `void`.
- [02]: `load_edges_from_file(graph name, label name, file_path text, load_as_agtype bool => false)` → `void`.
- [03]: `create_complete_graph(graph name, nodes int, edge_label name, node_label name => NULL)` → `void`.
- [04]: `age_vle(agtype, agtype, agtype, agtype, agtype, agtype, agtype, agtype, OUT edges agtype, OUT start_id graphid, OUT end_id graphid)` → `SETOF record`.
- [05]: `get_cypher_keywords(OUT word text, OUT catcode "char", OUT catdesc text)` → `SETOF record`.

## [08]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Session-load obligation: `CREATE EXTENSION age` installs the `ag_catalog` SQL objects once per database, and the one-shot `CreateSql` cannot encode the per-connection load — a Cypher-running connection issues `LOAD 'age'` + `SET search_path = ag_catalog, "$user", public` at the connection seam before any `cypher`/`agtype` symbol resolves.
- Install ownership guard: `CREATE EXTENSION age` refuses a pre-existing `ag_catalog` owned by a different role — ownership is compared exactly, even for a superuser — so a re-creating provisioning profile owns `ag_catalog` or drops it first, and the install row is idempotent only against an `ag_catalog` the installing role owns.
- VLE cache coherence: the `age_invalidate_graph_cache()` trigger on each label's backing relation bumps the graph version on any SQL-level `INSERT`/`UPDATE`/`DELETE`/`TRUNCATE` that bypasses Cypher (a bulk loader), keeping direct backing-relation writes coherent with later `*`-range traversals.
- Column-definition list: `cypher(…)`'s `RETURNS SETOF record` contract requires the caller's `AS (col agtype, …)` list — an anonymous-record call without it is the faulted spelling — and `agtype` columns extract to typed scalars through the registered SQL casts (`::int`/`::text`/`::jsonb`), never a hand-parsed text decode.

[STACKING]:
- `api-npgsql`(`.api/api-npgsql.md`): AGE ships no managed or EF driver, so every Cypher statement rides raw `Npgsql` — `NpgsqlDataSource`/`NpgsqlCommand` execute `cypher(…)`, `agtype` columns read through `FromSql`/`SqlQuery`, and `NpgsqlDataSourceBuilder.UsePhysicalConnectionInitializer` installs the per-connection `LOAD 'age'`+`search_path` hook once per physical connection.
- `api-pgrouting`(`.api/api-pgrouting.md`): AGE owns the openCypher node space (`Match`/`Mutate`/`Reach`) while `pgrouting` owns the weighted routing cases over H3-cell vertex ids — both halves of the one `Query/cypher#GRAPH_QUERY` union, driven raw against the `SETOF record` result.
- `Query/cypher#GRAPH_SESSION`: `GraphSession` binds graph name, Cypher body, and `params agtype` through `Npgsql` parameters and decodes `agtype`/path via the registered `->>`/`::jsonb` casts; `Query/cypher#GRAPH_QUERY` lowers each openCypher case to its server-side SQL, escalating beyond the default in-process QuikGraph `Query/topology` view.

[LOCAL_ADMISSION]:
- `age` carries no managed linkage: install rides the `age` `ServerExtension` row as a `Standalone` admission (`Store/provisioning#SERVER_EXTENSIONS`, `CREATE EXTENSION IF NOT EXISTS age`, no `shared_preload_libraries` gate), and the lane admits only under `Query/cypher` `CypherEnablement.SelfHosted`, disabled by default beneath the in-process QuikGraph topology.

[RAIL_LAW]:
- Package: `apache-age` / extension `age` (server-side, in-DB)
- Owns: the in-PG openCypher graph store — labelled vertex/edge relations under `ag_catalog`, the `cypher(graph, $$..$$, params)` query function, and the `agtype` value type with its operator/cast set
- Accept: `CREATE EXTENSION age` install via the `age` `ServerExtension` row, the per-session `LOAD 'age'`/`search_path` connection-init, `create_graph`/`create_vlabel`/`create_elabel` lifecycle, parameterized `cypher(…)` with the mandatory `agtype` column-definition list, `agtype` operator/cast extraction through `FromSql`/`SqlQuery`
- Reject: linking the extension into managed code, an anonymous-record `cypher(…)` call without the column-definition list, a runtime-concatenated Cypher body, omitting the per-session `LOAD 'age'`, treating `apache-age` as the installed extension name (it is `age`)
