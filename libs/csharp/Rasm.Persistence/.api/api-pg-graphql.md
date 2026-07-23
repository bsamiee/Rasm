# [RASM_PERSISTENCE_API_PG_GRAPHQL]

`pg_graphql` owns the in-database GraphQL resolver: it reflects the live SQL schema — tables, columns, foreign keys, comments — into a Relay-conformant GraphQL schema and resolves a query document to `jsonb` through the one `graphql.resolve` function, with no out-of-process gateway. Every surface is server-side SQL under the `graphql` schema, carrying no managed assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pg_graphql`
- package: `pg_graphql` (Apache-2.0)
- namespace: SQL `graphql` schema; `relocatable = false` fixes the schema name
- target: PG18, function and DDL-event-trigger registered, `shared_preload_libraries`-free
- registration: `CREATE EXTENSION pg_graphql`; control-file `superuser = true` runs install under a superuser role, query resolution under any role
- rail: graphql-provisioning, read-api-egress

## [02]-[RESOLVER]

One function resolves an entire GraphQL operation: the public plpgsql `graphql.resolve` delegates to the private Rust resolver inside a `begin ... exception when others` block, returning the response envelope `{ "data": <jsonb>, "errors": [...] }` rather than raising.

`graphql.resolve(query text, variables jsonb, "operationName" text, extensions jsonb) -> jsonb`

## [03]-[SCHEMA_REFLECTION]

Reflection derives the GraphQL schema from the live SQL schema with no schema-definition file: a table without a primary key or a directive-supplied surrogate is not exposed, and inflection is off by default so SQL names pass through unchanged — opting in maps `snake_case` to `PascalCase` types and `camelCase` fields.

| [INDEX] | [SQL_SHAPE]     | [GRAPHQL_SHAPE]                                                      | [SEMANTICS]                        |
| :-----: | :-------------- | :------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | table           | object type + `Query.<table>Collection`                              | paginated collection entrypoint    |
|  [02]   | column          | field (typed by the column type)                                     | typed object field                 |
|  [03]   | primary key     | `nodeId: ID!` (base64 `[schema,table,pk...]`) + `Query.node(nodeId)` | Relay global identity; PK required |
|  [04]   | foreign key     | to-one field (referencing) + `<child>Collection` (referenced)        | relationship / connection field    |
|  [05]   | collection      | `<Table>Connection { edges, pageInfo, totalCount?, aggregate? }`     | Relay connection envelope          |
|  [06]   | collection args | `first`/`last`/`before`/`after`/`offset`, `filter`, `orderBy`        | cursor pagination, filter, order   |

Collection results carry `<Table>Connection { edges { cursor, node }, pageInfo, totalCount?, aggregate? }`. `<Table>Filter` carries `nodeId`, `and`/`or`/`not`, and a per-column `<Type>Filter` exposing `eq`/`neq`/`gt`/`gte`/`lt`/`lte`/`in`/`is`; string adds `startsWith`/`like`/`ilike`/`regex`/`iregex`, list adds `contains`/`containedBy`/`overlaps`. Ordering is `[<Table>OrderBy!]` over `OrderByDirection { AscNullsFirst, AscNullsLast, DescNullsFirst, DescNullsLast }`. Mutations are `insertInto`/`update`/`deleteFrom<Table>Collection` returning `{ affectedCount, records }`.

## [04]-[COMMENT_DIRECTIVES]

`@graphql` JSON directives in SQL `COMMENT` text tune the schema shape, parsed by `graphql.comment_directive`; each `[FORM]` is the `<json>` body, and the wire form is `comment on <object> is e'@graphql(<json>)'`, escape-string because the payload commonly carries quotes.

| [INDEX] | [DIRECTIVE]                   | [APPLIES_TO]                   | [FORM]                                                           |
| :-----: | :---------------------------- | :----------------------------- | :--------------------------------------------------------------- |
|  [01]   | `inflect_names`               | schema                         | `{"inflect_names": true}`                                        |
|  [02]   | `max_rows`                    | schema / table / view          | `{"max_rows": 100}` (default 30; cascades to parent)             |
|  [03]   | `introspection`               | schema                         | `{"introspection": true}` (default off)                          |
|  [04]   | `name`                        | table / column / function      | `{"name": "AccountHolder"}` — rename the type/field              |
|  [05]   | `description`                 | table / column / function      | `{"description": "..."}`                                         |
|  [06]   | `totalCount`                  | table                          | `{"totalCount": {"enabled": true}}`                              |
|  [07]   | `aggregate`                   | table                          | `{"aggregate": {"enabled": true}}`                               |
|  [08]   | `local_name` / `foreign_name` | foreign-key constraint         | `{"local_name": "posts", "foreign_name": "author"}` (on the FK)  |
|  [09]   | `primary_key_columns`         | view / matview / foreign table | `{"primary_key_columns": ["id"]}` — surrogate PK for exposure    |
|  [10]   | `foreign_keys`                | view / matview / foreign table | virtual FK on a view/matview/foreign table (shape below)         |
|  [11]   | `mappings`                    | enum type                      | `{"mappings": {"aead-ietf": "AEAD_IETF"}}` — remap enum variants |

- [10]-[FOREIGN_KEYS]: `{"local_columns":[...], "foreign_schema":"...", "foreign_table":"...", "foreign_columns":[...], "local_name"?, "foreign_name"?}` synthesizes a virtual FK on a view/matview/foreign table.

## [05]-[CONFIGURATION]

Schema reflection caches and invalidates by version: two event triggers bump the schema version on any DDL, so the next `resolve` sees the new shape with no manual `rebuild_schema` entrypoint.

| [INDEX] | [SIGNATURE]                                                                 | [SEMANTICS]                                              |
| :-----: | :-------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `graphql.comment_directive(comment_ text)` → `jsonb`                        | parse `@graphql(...)` JSON from a comment (`{}` if none) |
|  [02]   | `graphql.get_schema_version()` → `int`                                      | read the current reflected-schema version                |
|  [03]   | `graphql.increment_schema_version()` → `event_trigger`                      | bump the schema version (DDL-trigger driven)             |
|  [04]   | `graphql_watch_ddl` / `graphql_watch_drop` (`ddl_command_end` / `sql_drop`) | auto-invalidate the cache on DDL change                  |

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Preload-free registration: `pg_graphql` registers no `shared_preload_libraries` row — a `pgrx` extension exposing SQL functions and DDL event triggers, no background worker, no planner hook — installed as `ServerExtension("pg_graphql", PreloadGated: false)`.
- Reflection is data, never a hand-mapped schema: tables/columns/foreign keys/comments reflect into the GraphQL schema; a table earns exposure through a primary key or a `primary_key_columns`/`foreign_keys` directive for a view/matview/foreign table, type/field/relationship names tune through `@graphql` directives, and the two event triggers invalidate the cache on DDL.

[STACKING]:
- `npgsql`(`.api/api-npgsql.md`): the `jsonb` resolver result rides raw `Npgsql` through `FromSql`/`SqlQuery`, the query document and `variables` bound as parameters — never an EF-translated member, never a runtime-concatenated GraphQL string.
- `Store/provisioning#SERVER_EXTENSIONS`: `ServerExtension("pg_graphql", PreloadGated: false)` emits `CREATE EXTENSION IF NOT EXISTS pg_graphql` through `Declare`, its row absent from the preload value.
- `Element/identity#ELEMENT_IDENTITY`: read-API egress reads the reflected `element_identity`/`node_cell` tables, mapped once from the live schema.

[LOCAL_ADMISSION]:
- `pg_graphql` is the sole in-DB GraphQL resolver, installed through the provisioning `ServerExtension` row; the platform `graphql_public.graphql(...)` convenience wrapper is a deployment-host artifact outside this extension.

[RAIL_LAW]:
- Package: `pg_graphql` (Apache-2.0)
- Owns: the in-PG GraphQL resolver — live schema reflection (tables/columns/FKs/comments → Relay GraphQL types) and `graphql.resolve(...)` → `jsonb` resolution
- Accept: `CREATE EXTENSION pg_graphql` via `ServerExtension("pg_graphql")`, `graphql.resolve` driven through `FromSql`/`SqlQuery` with bound parameters, `@graphql` comment directives, the Relay connection/filter/order args, the `{data,errors}` envelope read from the `jsonb`
- Reject: linking the extension into managed code, a runtime-concatenated GraphQL document, a hand-written GraphQL schema beside the reflected one, a `shared_preload_libraries` row, expecting `resolve` to raise on a query error, exposing a primary-key-less table without a surrogate-PK directive
