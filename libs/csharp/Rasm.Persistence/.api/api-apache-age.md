# [RASM_PERSISTENCE_API_APACHE_AGE]

`apache-age` (the project/repo name; the installed extension is `age`) supplies an openCypher graph
engine inside PostgreSQL — labelled vertices and edges stored in per-graph backing relations under the
`ag_catalog` schema, queried through the one `cypher(graph, $$ ... $$)` set-returning function whose
rows are the `agtype` graph value type. It carries no managed assembly: every surface is server-side
SQL the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension("age")` row installs and the `Query/cypher`
graph lane drives through raw `Npgsql` against the `agtype` result — `GraphSession` owns the enablement
gate, DDL lifecycle, and `agtype` decode, and the `GraphQuery` `[Union]` (`Match`/`Mutate`/`Reach`) is
the ONE verb surface an in-PG openCypher query enters through, all inside the one `PostgresServer`
residence (the in-process QuikGraph `Query/topology` view is the default sibling; this lane is the
optional self-hosted escalation). The extension is NOT preload-gated — it installs through `CREATE EXTENSION age` and requires
a per-session `LOAD 'age'` plus a `search_path` set, never a `shared_preload_libraries` row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-age` / extension `age`
- package: server-side PostgreSQL extension (C, not a NuGet package); repo `apache/age`, installed name `age`, `default_version = '1.7.0'`
- namespace: SQL `ag_catalog` schema (functions, catalog tables, the `agtype` type and its operator/cast set)
- license: Apache-2.0 — the in-DB deployment is the license boundary, no managed linkage
- registration: `CREATE EXTENSION age`, preload-free — absent from the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries` row by design; the `ServerExtension("age", PreloadGated: false)` row carries its install, the per-session `LOAD 'age'`/`search_path` is a connection-init concern (`[06]`)
- consumed by: `Query/cypher#GRAPH_SESSION` (enablement gate, `GraphDdl` lifecycle, `agtype` decode) + `Query/cypher#GRAPH_QUERY` (the `GraphQuery.Match`/`Mutate`/`Reach` openCypher cases), driven through raw `Npgsql` against the `agtype` result type
- rail: graph-provisioning, graph-lane

## [02]-[SESSION_SETUP]

`age` is a per-session-loaded extension: `CREATE EXTENSION` registers the SQL objects once per
database, but every connection that runs Cypher must `LOAD 'age'` and put `ag_catalog` on the
`search_path` so the bare `cypher`/`agtype` symbols resolve. This is a per-connection init step, not a
one-shot migration, and is NOT encoded by the `Extension` row's `CreateSql`.

| [INDEX] | [STATEMENT]                                     | [SCOPE]           | [SEMANTICS]                                                    |
| :-----: | :---------------------------------------------- | :---------------- | :------------------------------------------------------------- |
|  [01]   | `CREATE EXTENSION IF NOT EXISTS age`            | once per database | install the SQL objects into `ag_catalog` (the `CreateSql`)    |
|  [02]   | `LOAD 'age'`                                    | once per session  | load the shared library into the backend (before any Cypher)   |
|  [03]   | `SET search_path = ag_catalog, "$user", public` | per session/txn   | resolve unqualified `cypher`/`agtype` (or `SET LOCAL` per txn) |

A PL/pgSQL function that runs Cypher repeats `LOAD 'age'; SET search_path TO ag_catalog;` inside its
own body — the session settings of the caller do not cross the function boundary.

## [03]-[CATALOG_SCHEMA]

The `ag_catalog` schema holds the graph/label registry as ordinary tables (dumped via
`pg_extension_config_dump`, so they survive `pg_dump`), keyed by the label-id/kind domains.

| [INDEX] | [OBJECT]     | [SHAPE]                                                                                  | [SEMANTICS]             |
| :-----: | :----------- | :--------------------------------------------------------------------------------------- | :---------------------- |
|  [01]   | `ag_graph`   | `(graphid oid, name name, namespace regnamespace)`                                       | one row per graph       |
|  [02]   | `ag_label`   | `(name name, graph oid, id label_id, kind label_kind, relation regclass, seq_name name)` | one row per label       |
|  [03]   | `label_id`   | `DOMAIN int CHECK (VALUE > 0 AND VALUE <= 65535)`                                        | label id `1`–`65535`    |
|  [04]   | `label_kind` | `DOMAIN "char" CHECK (VALUE = 'v' OR VALUE = 'e')`                                       | `'v'` vertex/`'e'` edge |
|  [05]   | `graphid`    | 8-byte pass-by-value id packing `label_id` + entry-id                                    | vertex/edge identity    |

## [04]-[GRAPH_LABEL_LIFECYCLE]

Graph and label DDL are `ag_catalog`-schema `SELECT`-function calls returning `void`. `create_graph`
materialises the namespace plus the default `_ag_label_vertex`/`_ag_label_edge` base labels; a label's
backing relation is created on first use or explicitly by `create_vlabel`/`create_elabel`.

| [INDEX] | [FUNCTION]      | [SIGNATURE]                                                            | [SEMANTICS]                               |
| :-----: | :-------------- | :--------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `create_graph`  | `create_graph('graph_name'::name)`                                     | graph namespace + base labels             |
|  [02]   | `drop_graph`    | `drop_graph('graph_name'::name, cascade => true)`                      | drop graph; `cascade` drops label tables  |
|  [03]   | `create_vlabel` | `create_vlabel('graph_name', 'label')` (args `cstring`)                | declare a vertex label / relation         |
|  [04]   | `create_elabel` | `create_elabel('graph_name', 'label')` (args `cstring`)                | declare an edge label / relation          |
|  [05]   | `drop_label`    | `drop_label('graph_name'::name, 'label'::name, force => false)`        | drop a label; `force` ignores refs        |
|  [06]   | `alter_graph`   | `alter_graph('graph_name'::name, 'RENAME'::cstring, 'new_name'::name)` | rename the graph (`operation`=`'RENAME'`) |

## [05]-[CYPHER_QUERY]

One polymorphic set-returning function runs every Cypher statement; the third `params agtype`
argument is the parameterized form (referenced inside the query body as `$name`).

```sql signature
ag_catalog.cypher(graph_name name = NULL, query_string cstring = NULL, params agtype = NULL) RETURNS SETOF record
```

Because `cypher` is declared `RETURNS SETOF record`, PostgreSQL itself requires the caller to supply a
column-definition list — there is no anonymous-record default. Every projected column is typed
`agtype`, and the list arity/names must match the Cypher `RETURN` clause.

| [INDEX] | [FORM]                                                                                       | [SEMANTICS]                               |
| :-----: | :------------------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `SELECT * FROM cypher('g', $$ MATCH (v {name:'A'}) RETURN v $$) AS (v agtype)`               | read; one `agtype` col per `RETURN` term  |
|  [02]   | `SELECT * FROM cypher('g', $$ CREATE (:Label {k:'v'}) $$) AS (r agtype)`                     | write; still needs a column list          |
|  [03]   | `SELECT * FROM cypher('g', $$ MATCH (n) WHERE n.id = $target RETURN n $$, $1) AS (n agtype)` | `$target` binds from `params agtype` `$1` |

A clause that mutates AND returns in one Cypher statement is rejected by AGE (split `CREATE`/`SET` from
the trailing `MATCH ... RETURN`); the `$$ ... $$` dollar-quote isolates the Cypher body from SQL
string escaping. The `Query/cypher` `GraphSession` binds graph name, query body, and the `params agtype`
through `Npgsql` parameters (server-side `format('%L')` composition) and decodes the `agtype` columns
itself, never an EF-translated member.

## [06]-[AGTYPE]

`agtype` is AGE's single value type — a superset of `jsonb`'s binary format extended with exact-number
kinds (`integer`/`float`/`numeric`) and the graph entities `vertex`/`edge`/`path`. A vertex/edge
renders `{id, label, properties}::vertex|edge`; a path is the alternating `[vertex, edge, ...]::path`.

| [INDEX] | [OPERATOR]    | [SIGNATURE]                               | [SEMANTICS]                              |
| :-----: | :------------ | :---------------------------------------- | :--------------------------------------- |
|  [01]   | `->`          | `agtype -> text\|int4\|agtype` → `agtype` | object field / array element             |
|  [02]   | `->>`         | `agtype ->> text\|int4\|agtype` → `text`  | object field / array element as text     |
|  [03]   | `#>`          | `agtype #> agtype` → `agtype`             | extract value at path                    |
|  [04]   | `#>>`         | `agtype #>> agtype` → `text`              | extract value at path as text            |
|  [05]   | `@>` / `<@`   | `agtype @> agtype` → `boolean`            | containment / contained-by (commutators) |
|  [06]   | `@>>` / `<<@` | `agtype @>> agtype` → `boolean`           | top-level-only containment               |

Casts compose at two layers: in-Cypher `expr::int|float|numeric|bool|vertex|edge|path` (agtype →
agtype), and SQL-level CASTs registered against native types — `agtype::text`, `::boolean`,
`::float8`, `::bigint`/`::int`/`::smallint`, `::int[]`, `::json`, `::jsonb` (and the reverse
`text`/`boolean`/`float8`/`int8`/`int4`/`jsonb` → `agtype`) — so `(cypher(...)).col::int` /
`::text` / `::jsonb` extracts a typed scalar from a returned `agtype` row.

## [07]-[GRAPH_ALGORITHMS]

Bulk loaders, the complete-graph generator, and the variable-length-edge engine — all under
`ag_catalog`, returning `agtype` rows or `void`. `age` exposes NO `age_shortest_path`/
`age_all_shortest_paths` SQL functions: variable-length traversal is the `age_vle` engine behind
Cypher's `*` range patterns, and any shortest-path need is written as a bounded `*`-range Cypher `MATCH`,
never a dedicated SQL routine — weighted/large-fan path work is the `pgrouting` half of the SAME
`Query/cypher#GRAPH_QUERY` union (`Path`/`Via`/`Kth`/…), and the in-process QuikGraph `Query/topology`
view is the default synchronous counterpart.

All functions live under `ag_catalog`; each row's exact signature is keyed `[01]`–`[05]` below the table.

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

[AGE_TOPOLOGY]:
- Preload-free, session-loaded: `age` registers no `shared_preload_libraries` row, so it is correctly absent from the `Store/provisioning#SERVER_EXTENSIONS` preload value; install is `ServerExtension("age", PreloadGated: false)` whose `CreateSql` emits `CREATE EXTENSION IF NOT EXISTS age` through `Store/provisioning#SERVER_EXTENSIONS` `Declare`/`Migrate`. The per-session `LOAD 'age'` + `SET search_path = ag_catalog, "$user", public` is a connection-init obligation an `Npgsql` open-hook (or per-connection `SET`) issues — the one-shot `Extension` `CreateSql` cannot encode it, so a profile that runs Cypher must carry the session-load step, and a PL/pgSQL Cypher wrapper repeats `LOAD`/`search_path` in its own body.
- Install ownership guard (`age`): `CREATE EXTENSION age` refuses to install into a pre-existing `ag_catalog` schema owned by a different role (ownership is compared directly, exact even for a superuser) — a provisioning profile that re-creates the extension must own `ag_catalog` or drop it first, so the install row is idempotent only against an `ag_catalog` the installing role owns.
- VLE cache coherence: `age` installs the `age_invalidate_graph_cache()` trigger on each label's backing relation, so an SQL-level `INSERT`/`UPDATE`/`DELETE`/`TRUNCATE` against a label table (bypassing Cypher, e.g. a bulk loader) bumps the graph version and invalidates the `age_vle` caches — direct backing-relation writes stay coherent with subsequent `*`-range traversals.
- No managed assembly, no EF translator: every Cypher statement rides raw `Npgsql`/`FromSql`/`SqlQuery` mapping `agtype` columns, and the mandatory `AS (col agtype, ...)` column-definition list is required by PostgreSQL's `RETURNS SETOF record` contract, never optional — an anonymous-record call without the list is the faulted spelling. The `agtype` columns are extracted to typed scalars through the registered SQL-level casts (`::int`/`::text`/`::jsonb`), never a hand-parsed text decode.
- In-residence graph engine: `age` lives inside the one `PostgresServer` residence — a within-PG openCypher capability the OPTIONAL self-hosted `Query/cypher` lane gates behind `CypherEnablement`, demoted beneath the default in-process QuikGraph `Query/topology` view, never a cross-store query federator. Graph name, Cypher body, and `params agtype` arrive bound through the `GraphSession` `format('%L')` server-side composition, never a runtime-concatenated Cypher string.

[RAIL_LAW]:
- Package: `apache-age` / extension `age` (server-side, in the deploy-image PG18)
- Owns: the in-PG openCypher graph store — labelled vertex/edge relations under `ag_catalog`, the `cypher(graph, $$..$$, params)` query function, and the `agtype` value type with its operator/cast set
- Accept: `CREATE EXTENSION age` install via `ServerExtension("age")`, the per-session `LOAD 'age'`/`search_path` connection-init, `create_graph`/`create_vlabel`/`create_elabel` lifecycle, parameterized `cypher(...)` with the mandatory `agtype` column-definition list, `agtype` operator/cast extraction through `FromSql`/`SqlQuery`
- Reject: linking the extension into managed code, an anonymous-record `cypher(...)` call without the column-definition list, a runtime-concatenated Cypher body, placing `age` on the `shared_preload_libraries` row (it is preload-free), omitting the per-session `LOAD 'age'`, treating `apache-age` as the installed extension name (it is `age`)
