# [RASM_PERSISTENCE_API_PG_GRAPHQL]

`pg_graphql` supplies an in-database GraphQL resolver — it reflects the SQL schema (tables, columns,
foreign keys, comments) into a Relay-conformant GraphQL schema at runtime and resolves a GraphQL query
document to `jsonb` through the one `graphql.resolve(...)` function, with no out-of-process GraphQL
gateway. It carries no managed assembly: every surface is server-side SQL the
`Schema/ddl#EXTENSION_DDL` `SchemaDdl.Extension("pg_graphql")` row installs and a read-API egress
consumer drives through raw `Npgsql`/`FromSql`/`SqlQuery` against the `jsonb` result, so the
`Query/federation#ENTITY_GRAPH` tables a GraphQL client reads are reflected once from the live schema
rather than hand-mapped. The extension is NOT preload-gated — it is a `pgrx` (Rust) extension exposing
SQL functions plus DDL event triggers, installed through `CREATE EXTENSION pg_graphql`, never a
`shared_preload_libraries` row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_graphql`
- package: server-side PostgreSQL extension (Rust/`pgrx`, not a NuGet package); repo `supabase/pg_graphql`, version `1.6.1` (built against `pgrx 0.16.x`, default target `pg18`)
- namespace: SQL `graphql` schema (the `resolve` resolver plus private reflection/config objects); `relocatable = false` — the `graphql` schema name is fixed, never relocated
- license: Apache-2.0 — the in-DB deployment is the license boundary, no managed linkage
- registration: `CREATE EXTENSION pg_graphql`, preload-free — absent from the `Store/provisioning#CLUSTER_CONFIG` `shared_preload_libraries` row by design; the `SchemaDdl.Extension("pg_graphql", PreloadGated: false)` row carries its install. The control file declares `superuser = true`, so the `CREATE EXTENSION` step runs under a superuser/owner role at provision time (a non-superuser session still resolves queries through `graphql.resolve` once installed)
- consumed by: a read-API egress over the `Query/federation#ENTITY_GRAPH` reflected tables, driven through raw `Npgsql` against the `jsonb` resolver result
- reflection: tables → object types, columns → fields, foreign keys → relationship/connection fields, comments → `@graphql` directives — recomputed lazily and auto-invalidated by DDL event triggers
- rail: graphql-provisioning, read-api-egress

## [02]-[RESOLVER]

One function resolves an entire GraphQL operation. The public `graphql.resolve` is a thin `plpgsql`
wrapper that delegates to the private Rust `graphql._internal_resolve` inside a
`begin ... exception when others` block, returning a GraphQL-shaped error envelope on failure rather
than raising.

```
graphql.resolve(
    "query" text,
    "variables" jsonb default '{}',
    "operationName" text default null,
    "extensions" jsonb default null
) RETURNS jsonb
```

The return is the GraphQL response envelope `{ "data": <jsonb>, "errors": [...] }`. `operationName`
and `variables` carry the GraphQL operation name and the bound variable map; the consumer passes the
query document and `variables` as `Npgsql` parameters and reads the `jsonb` result through
`FromSql`/`SqlQuery`, never an EF-translated member.

## [03]-[SCHEMA_REFLECTION]

The GraphQL schema is reflected from the live SQL schema — no schema-definition file. A table without
a primary key (or a directive-supplied surrogate) is not exposed. Inflection is OFF by default: SQL
names pass through literally; opting in maps `snake_case` → `PascalCase` (types) / `camelCase`
(fields).

| [INDEX] | [SQL_SHAPE]                       | [GRAPHQL_SHAPE]                                                              | [SEMANTICS]                              |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | table                             | object type + `Query.<table>Collection`                                      | each table is a type with a paginated collection entrypoint |
|  [02]   | column                            | field (typed by the column type)                                            | columns become object fields             |
|  [03]   | primary key                       | `nodeId: ID!` (base64 `["schema","table",pk...]`) + `Query.node(nodeId)`     | Relay global object identity; PK is required for exposure |
|  [04]   | foreign key                       | to-one field on the referencing side + `<child>Collection` on the referenced side | FKs become relationship/connection fields |
|  [05]   | collection                        | `<Table>Connection { edges { cursor, node }, pageInfo, totalCount?, aggregate? }` | the Relay connection envelope            |
|  [06]   | collection args                   | `first`/`last`/`before`/`after`/`offset`, `filter`, `orderBy`               | cursor pagination, filtering, ordering   |

Filtering is `input <Table>Filter { <col>: <Type>Filter, nodeId, and, or, not }`; per-scalar filters
expose `eq`/`neq`/`gt`/`gte`/`lt`/`lte`/`in`/`is` (string adds `startsWith`/`like`/`ilike`/`regex`/
`iregex`; list adds `contains`/`containedBy`/`overlaps`). Ordering is `[<Table>OrderBy!]` over
`enum OrderByDirection { AscNullsFirst, AscNullsLast, DescNullsFirst, DescNullsLast }`. Mutations
appear as `insertInto<Table>Collection` / `update<Table>Collection` / `deleteFrom<Table>Collection`
returning `{ affectedCount, records }`.

## [04]-[COMMENT_DIRECTIVES]

Schema shape is tuned by `@graphql` JSON directives carried in SQL `COMMENT` text, parsed by the
`graphql.comment_directive` helper. The escape-string form `e'@graphql(<json>)'` is used because the
payload commonly contains quotes.

| [INDEX] | [DIRECTIVE]           | [APPLIES_TO]                       | [FORM]                                                                 |
| :-----: | :-------------------- | :--------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `inflect_names`       | schema                             | `comment on schema s is e'@graphql({"inflect_names": true})'`         |
|  [02]   | `max_rows`            | schema / table / view              | `e'@graphql({"max_rows": 100})'` (default 30; cascades to parent)      |
|  [03]   | `introspection`       | schema                             | `e'@graphql({"introspection": true})'` (default off since 1.6.0)      |
|  [04]   | `name`                | table / column / function          | `e'@graphql({"name": "AccountHolder"})'` — rename the type/field      |
|  [05]   | `description`         | table / column / function          | `e'@graphql({"description": "..."})'`                                  |
|  [06]   | `totalCount`          | table                              | `e'@graphql({"totalCount": {"enabled": true}})'`                       |
|  [07]   | `aggregate`           | table                              | `e'@graphql({"aggregate": {"enabled": true}})'`                        |
|  [08]   | `local_name` / `foreign_name` | foreign-key constraint     | `comment on constraint fk on t is e'@graphql({"local_name": "posts", "foreign_name": "author"})'` |
|  [09]   | `primary_key_columns` | view / matview / foreign table     | `e'@graphql({"primary_key_columns": ["id"]})'` — surrogate PK for exposure |
|  [10]   | `foreign_keys`        | view / matview / foreign table     | array of `{"local_columns":[...], "foreign_schema":"...", "foreign_table":"...", "foreign_columns":[...], "local_name"?, "foreign_name"?}` |
|  [11]   | `mappings`            | enum type                          | `e'@graphql({"mappings": {"aead-ietf": "AEAD_IETF"}})'` — remap non-conforming enum variants |

## [05]-[CONFIGURATION]

Schema reflection is cached and invalidated by version, not rebuilt by hand. Two event triggers bump
the schema version on any DDL, so the next `resolve` sees the new shape — there is no manual
`rebuild_schema` entrypoint.

| [INDEX] | [SURFACE]                            | [SIGNATURE]                                  | [SEMANTICS]                                  |
| :-----: | :----------------------------------- | :------------------------------------------- | :------------------------------------------- |
|  [01]   | `graphql.comment_directive`          | `graphql.comment_directive(comment_ text)` → `jsonb` | parse the `@graphql(...)` JSON out of a comment string (`{}` if none) |
|  [02]   | `graphql.get_schema_version`         | `graphql.get_schema_version()` → `int`       | read the current reflected-schema version    |
|  [03]   | `graphql.increment_schema_version`   | `graphql.increment_schema_version()` → `event_trigger` | bump the schema version (driven by the DDL triggers) |
|  [04]   | `graphql_watch_ddl` / `graphql_watch_drop` | event triggers (`ddl_command_end` / `sql_drop`) | auto-invalidate the cache on schema change |

## [06]-[IMPLEMENTATION_LAW]

[GRAPHQL_TOPOLOGY]:
- Preload-free, function + event-trigger registered: `pg_graphql` registers no `shared_preload_libraries` row (it is a pgrx extension exposing SQL functions plus DDL event triggers, no background worker, no planner hook), so it is correctly absent from the `Store/provisioning#CLUSTER_CONFIG` preload value; install is `SchemaDdl.Extension("pg_graphql", PreloadGated: false)` whose `CreateSql` emits `CREATE EXTENSION IF NOT EXISTS pg_graphql` through `Schema/ddl#EXTENSION_DDL` `Declare`.
- No managed assembly, no EF translator: the whole GraphQL operation resolves through one `graphql.resolve(query, variables, operationName, extensions)` returning `jsonb`, ridden by raw `Npgsql`/`FromSql`/`SqlQuery`; the query document and `variables` arrive as `Npgsql` parameters, never a runtime-concatenated GraphQL string. The resolver never raises — it returns `{ "data": ..., "errors": [...] }` — so the egress consumer reads the `errors` array from the `jsonb` rather than catching an exception.
- Reflection is data, not hand-mapped schema: tables/columns/FKs/comments reflect into the GraphQL schema automatically — a table needs a primary key (or a `primary_key_columns`/`foreign_keys` directive for a view/matview/foreign table) to be exposed, type/field/relationship names tune through `@graphql` comment directives, and the schema version invalidates on DDL through the two event triggers, so a parallel hand-written GraphQL schema beside the reflected one is the rejected form. A platform `graphql_public.graphql(...)` wrapper is a deployment-host artifact, not part of this extension.

[RAIL_LAW]:
- Package: `pg_graphql` (server-side, in the deploy-image PG18)
- Owns: the in-PG GraphQL resolver — live schema reflection (tables/columns/FKs/comments → Relay GraphQL types) and the `graphql.resolve(...)` → `jsonb` query resolution
- Accept: `CREATE EXTENSION pg_graphql` install via `SchemaDdl.Extension("pg_graphql")`, `graphql.resolve(query, variables, operationName, extensions)` driven through `FromSql`/`SqlQuery` with bound parameters, `@graphql` comment directives for schema tuning, the Relay connection/filter/order args, the GraphQL `{data,errors}` envelope read from the `jsonb` result
- Reject: linking the extension into managed code, a runtime-concatenated GraphQL document, a hand-written GraphQL schema beside the reflected one, placing `pg_graphql` on the `shared_preload_libraries` row (it is preload-free), expecting `resolve` to raise on a query error (it returns an `errors` envelope), exposing a primary-key-less table without a surrogate-PK directive
