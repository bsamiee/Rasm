# [POSTGRES_04_RLS_VIEWS_RESEARCH]

Research date: 2026-06-05.
Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`.
Assignment: POSTGRES 4; RLS, policies, views, materialized views, security barriers, security invoker, leakproof, covert channels, and verification routes for `docs/standards/reference/code-documentation.md`.
Mutation boundary: no active standards edited; this report is the only assigned output file.

## [1][EXECUTIVE_FINDINGS]

[F1][KEEP]: The PostgreSQL capsule's core split is correct. `COMMENT ON` is durable catalog documentation, while SQL source comments are local rationale because PostgreSQL treats SQL comments as syntax whitespace before analysis.

[F2][CHANGE]: Add a compact verification-route record for catalog security comments. RLS, policy, view security mode, materialized-view freshness, and leakproof claims are all catalog-checkable through `pg_class`, `pg_policy`, `pg_policies`, `pg_proc`, `pg_roles`, `pg_get_viewdef`, description functions, and `psql` describe commands.

[F3][CHANGE]: Make view comments distinguish `security_barrier` and `security_invoker`. `security_barrier` constrains planner ordering to avoid passing hidden rows to unsafe functions; `security_invoker` changes whose privileges and RLS policies are used for underlying base relations. They are separate contracts.

[F4][CHANGE]: Materialized-view comments need a freshness and refresh contract, not ordinary view-security language. PostgreSQL returns stored data directly from a materialized view, refreshes it on demand, can leave it unscannable with `WITH NO DATA`, and has specific `CONCURRENTLY` requirements.

[F5][CHANGE]: Leakproof and covert-channel wording should be narrow. `LEAKPROOF` is a superuser-set function promise that affects security barrier views and RLS planner behavior, but security barriers do not protect against every inference channel such as `EXPLAIN`, runtime measurement, optimizer statistics, or plan choice.

## [2][REPO_EVIDENCE]

[REQUESTED_FILES_READ]:
- `CLAUDE.md`: Markdown routes through `docs/standards`; current-source verification is required for research.
- `AGENTS.md`: docs work routes through `docs/standards/README.md`; no static, test, or bridge proof claims for docs-only instruction edits.
- `docs/standards/README.md`: source precedence, reader-need routing, placement, lifecycle, and active standards boundaries.
- `docs/standards/AGENTS.md`: local overlay for `docs/standards/**`; `.reports/` is source material, not active standard text.
- ``: salience, artifact separation, generated mirrors, and access-boundary rules.
- `docs/standards/information-structure.md`: report containers, definition blocks, tables, code fences, and heading shape.
- `docs/standards/proof.md`: evidence hierarchy, freshness, proof-field labels, docs-only gate selection.
- `docs/standards/style-guide.md`: current-source terminology, code-safe Markdown, and prose mechanics.
- `docs/standards/formatting.md`: bracketed headings, table styling, whitespace, and status markers.
- `docs/standards/reference/code-documentation.md`: full target standard, especially `[6.5][POSTGRESQL_18_4]`.

[ACTIVE_STANDARD_SPANS]:
- `docs/standards/reference/code-documentation.md` says source comments carry caller-visible semantics omitted by signatures, annotations, schemas, shell declarations, SQL objects, and catalogs.
- The current PostgreSQL capsule already names policy access invariant, command scope, role scope, `USING` and `WITH CHECK`, permissive or restrictive combination, tenant predicate, bypass assumption, owner behavior, race, and covert-channel reasoning as catalog-comment content.
- The capsule already names view and materialized-view security mode, freshness, refresh contract, barrier or invoker behavior, projection scope, and data-exposure rule.
- The capsule rejects secrets, credentials, privileged assumptions, exploit details, credential routes, tenant IDs, and sensitive operational data in `COMMENT ON`.

[WORKTREE_CONTEXT]:
- `git status --short -- .reports/code-documentation-050626 docs/standards/reference/code-documentation.md` showed a pre-existing modified `docs/standards/reference/code-documentation.md` and many untracked sibling `.reports/` reports.
- `psql --version` failed with `zsh:1: command not found: psql`, so no local PostgreSQL runtime verification was executed.

## [3][PRIMARY_SOURCES]

Official PostgreSQL sources opened on 2026-06-05:

| [INDEX] | [SOURCE]                                                                                              | [USE]                                                                                                         |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------ |
|   [1]   | [PostgreSQL documentation landing](https://www.postgresql.org/docs/)                                  | `18` is current supported documentation; May 14, 2026 release signal names PostgreSQL 18.4                    |
|   [2]   | [Row Security Policies](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)                 | RLS default-deny, owner and `BYPASSRLS` bypass, leakproof ordering exception, policy creation and combination |
|   [3]   | [CREATE POLICY](https://www.postgresql.org/docs/current/sql-createpolicy.html)                        | `USING`, `WITH CHECK`, command and role scope, permissive and restrictive combination, trigger timing         |
|   [4]   | [ALTER TABLE](https://www.postgresql.org/docs/current/sql-altertable.html)                            | `ENABLE ROW LEVEL SECURITY`, `FORCE ROW LEVEL SECURITY`, default deny when no policy exists                   |
|   [5]   | [CREATE VIEW](https://www.postgresql.org/docs/current/sql-createview.html)                            | `security_barrier`, `security_invoker`, check option, RLS behavior through views, updatable views             |
|   [6]   | [Rules and Privileges](https://www.postgresql.org/docs/current/rules-privileges.html)                 | security barrier rationale, leakproof pushdown, performance tradeoff, covert-channel limitation               |
|   [7]   | [CREATE MATERIALIZED VIEW](https://www.postgresql.org/docs/current/sql-creatematerializedview.html)   | materialized query storage, security-restricted operation, `search_path`, `WITH NO DATA`                      |
|   [8]   | [REFRESH MATERIALIZED VIEW](https://www.postgresql.org/docs/current/sql-refreshmaterializedview.html) | refresh semantics, `MAINTAIN`, `CONCURRENTLY`, unique-index requirement, one refresh per materialized view    |
|   [9]   | [Materialized Views](https://www.postgresql.org/docs/current/rules-materializedviews.html)            | stored table-like data, system-catalog relation identity, non-current data, refresh route                     |
|  [10]   | [COMMENT](https://www.postgresql.org/docs/current/sql-comment.html)                                   | supported object comments, `POLICY`, `VIEW`, `MATERIALIZED VIEW`, visibility warning, description functions   |
|  [11]   | [pg_description](https://www.postgresql.org/docs/current/catalog-pg-description.html)                 | durable comment storage and extraction through `COMMENT` and `psql` describe commands                         |
|  [12]   | [pg_policy](https://www.postgresql.org/docs/current/catalog-pg-policy.html)                           | policy catalog fields and `relrowsecurity` dependency                                                         |
|  [13]   | [pg_policies](https://www.postgresql.org/docs/current/view-pg-policies.html)                          | human-readable policy view for roles, command, `qual`, and `with_check`                                       |
|  [14]   | [pg_class](https://www.postgresql.org/docs/current/catalog-pg-class.html)                             | relation kind, RLS flags, materialized-view populated flag, relation options                                  |
|  [15]   | [pg_proc](https://www.postgresql.org/docs/current/catalog-pg-proc.html)                               | `proleakproof`, `prosecdef`, volatility, strictness, parallel safety, function settings                       |
|  [16]   | [CREATE FUNCTION](https://www.postgresql.org/docs/current/sql-createfunction.html)                    | leakproof meaning, superuser-only option, `SECURITY DEFINER`, safe `search_path` guidance                     |
|  [17]   | [System Information Functions](https://www.postgresql.org/docs/current/functions-info.html)           | `row_security_active`, privilege inquiry functions, `pg_get_viewdef`, comment functions                       |
|  [18]   | [pg_roles](https://www.postgresql.org/docs/current/view-pg-roles.html)                                | `rolbypassrls` verification route                                                                             |
|  [19]   | [pg_views](https://www.postgresql.org/docs/current/view-pg-views.html)                                | view lookup surface and information-schema limitation context                                                 |
|  [20]   | [pg_matviews](https://www.postgresql.org/docs/current/view-pg-matviews.html)                          | materialized-view lookup surface                                                                              |
|  [21]   | [psql](https://www.postgresql.org/docs/current/app-psql.html)                                         | describe commands, object descriptions, leakproof operator-family inspection path                             |

## [4][SOURCE_NOTES]

[RLS_POLICIES]:
- RLS is off by default. When enabled, normal table access must be allowed by policy; without a policy, PostgreSQL applies a default-deny policy.
- Policies can apply by command and role. They can use `USING` for visible or modifiable existing rows and `WITH CHECK` for new row contents on `INSERT`, `UPDATE`, and `MERGE`.
- `WITH CHECK` runs after `BEFORE ROW` triggers and before actual data modification or other constraints, so a documentation comment may need trigger-timing context when a trigger mutates policy-checked values.
- Multiple applicable permissive policies combine with `OR`; restrictive policies combine with `AND`, and at least one permissive policy must grant access before restrictive policies usefully narrow it.
- Superusers, roles with `BYPASSRLS`, and table owners by default bypass RLS; `FORCE ROW LEVEL SECURITY` makes enabled RLS apply to the table owner.

[VIEWS]:
- A regular view runs its query each time it is referenced and normally checks base relations through the view owner.
- A `security_invoker` view checks underlying base relations with the privileges of the querying user. If base relations have RLS, `security_invoker` also makes policies and permissions apply as if the base relations were referenced directly by the invoking user.
- A `security_barrier` view should be used when a view is intended to provide row-level security; it prevents unsafe user-supplied functions and operators from receiving hidden row values before the view filter has done its work.
- `security_barrier` can reduce planner freedom and performance. The standard should not let a comment imply that barrier views are equivalent to complete information-flow control.
- Updatable views have `CHECK OPTION` semantics independent from RLS. Documentation should state whether an updatable security view relies on `LOCAL`, `CASCADED`, or no check option.

[MATERIALIZED_VIEWS]:
- A materialized view stores the query result in table-like form and returns stored data directly when queried.
- The stored data may not be current. A useful materialized-view comment should state freshness expectation, refresh owner, refresh cadence or event, and whether stale rows are acceptable.
- `CREATE MATERIALIZED VIEW` runs its query in a security-restricted operation and temporarily sets `search_path` to `pg_catalog, pg_temp`.
- `REFRESH MATERIALIZED VIEW` replaces the contents, requires `MAINTAIN`, can leave the view unscannable with `WITH NO DATA`, cannot combine `CONCURRENTLY` with `WITH NO DATA`, and allows only one refresh at a time per materialized view.
- `CONCURRENTLY` requires at least one unique index using only column names and including all rows. The comment should not claim nonblocking refresh unless that index condition is verified.

[LEAKPROOF_COVERT]:
- A leakproof function has no side effects and reveals no information about arguments except through its return value. Functions that may throw argument-dependent errors are not leakproof.
- `LEAKPROOF` may be set only by a superuser. A source comment should never make a leakproof promise for an unverified function or operator.
- For RLS tables and security barrier views, PostgreSQL enforces security conditions before user-supplied conditions that contain non-leakproof functions. Leakproof functions and operators may be evaluated earlier.
- `security_barrier` protects only the limited channel where invisible tuple values are passed to unsafe functions. PostgreSQL docs explicitly warn that users may still infer information through `EXPLAIN`, query runtime, statistics, or plan choice.

[CATALOG_VISIBILITY]:
- PostgreSQL comments can be viewed through `psql` describe commands and description functions, and there is no security mechanism for viewing comments in a connected database.
- `COMMENT ON POLICY`, `COMMENT ON VIEW`, and `COMMENT ON MATERIALIZED VIEW` are supported SQL surfaces. They are the durable route for policy and relation security semantics.

## [5][VERIFICATION_ROUTES]

Use catalog checks as proof routes when a PostgreSQL comment claims RLS, policy, view security mode, materialized-view freshness, or leakproof behavior. These routes are templates; they were not run in this workspace because `psql` is unavailable.

[RLS_TABLE_STATE]:

```sql template
SELECT
    n.nspname AS schema_name,
    c.relname AS relation_name,
    c.relkind,
    c.relrowsecurity,
    c.relforcerowsecurity,
    row_security_active(c.oid) AS active_for_current_user,
    obj_description(c.oid, 'pg_class') AS relation_comment
FROM pg_class AS c
JOIN pg_namespace AS n ON n.oid = c.relnamespace
WHERE n.nspname = '<schema_name>'
  AND c.relname = '<table_name>';
```

[POLICY_SHAPE]:

```sql template
SELECT
    schemaname,
    tablename,
    policyname,
    permissive,
    roles,
    cmd,
    qual,
    with_check
FROM pg_policies
WHERE schemaname = '<schema_name>'
  AND tablename = '<table_name>'
ORDER BY policyname;
```

[POLICY_COMMENT]:

```sql template
SELECT
    n.nspname AS schema_name,
    c.relname AS table_name,
    p.polname AS policy_name,
    obj_description(p.oid, 'pg_policy') AS policy_comment
FROM pg_policy AS p
JOIN pg_class AS c ON c.oid = p.polrelid
JOIN pg_namespace AS n ON n.oid = c.relnamespace
WHERE n.nspname = '<schema_name>'
  AND c.relname = '<table_name>'
  AND p.polname = '<policy_name>';
```

[BYPASS_ROLE]:

```sql template
SELECT rolname, rolsuper, rolbypassrls
FROM pg_roles
WHERE rolname = ANY (ARRAY['<role_name>', '<owner_role>']);
```

[VIEW_SECURITY]:

```sql template
SELECT
    n.nspname AS schema_name,
    c.relname AS view_name,
    c.relkind,
    c.reloptions,
    pg_get_viewdef(c.oid, false) AS view_definition,
    obj_description(c.oid, 'pg_class') AS view_comment
FROM pg_class AS c
JOIN pg_namespace AS n ON n.oid = c.relnamespace
WHERE n.nspname = '<schema_name>'
  AND c.relname = '<view_name>'
  AND c.relkind IN ('v', 'm');
```

[MATERIALIZED_VIEW_REFRESH]:

```sql template
SELECT
    n.nspname AS schema_name,
    c.relname AS matview_name,
    c.relispopulated,
    pg_get_viewdef(c.oid, false) AS stored_query,
    obj_description(c.oid, 'pg_class') AS matview_comment
FROM pg_class AS c
JOIN pg_namespace AS n ON n.oid = c.relnamespace
WHERE n.nspname = '<schema_name>'
  AND c.relname = '<matview_name>'
  AND c.relkind = 'm';
```

[CONCURRENT_REFRESH_INDEX]:

```sql template
SELECT
    i.indexrelid::regclass AS index_name,
    i.indisunique,
    i.indpred IS NULL AS includes_all_rows,
    i.indexprs IS NULL AS column_only_index
FROM pg_index AS i
WHERE i.indrelid = '<schema_name>.<matview_name>'::regclass
  AND i.indisunique;
```

[FUNCTION_SECURITY]:

```sql template
SELECT
    n.nspname AS schema_name,
    p.proname AS function_name,
    p.prosecdef,
    p.proleakproof,
    p.provolatile,
    p.proisstrict,
    p.proparallel,
    p.proconfig,
    obj_description(p.oid, 'pg_proc') AS function_comment
FROM pg_proc AS p
JOIN pg_namespace AS n ON n.oid = p.pronamespace
WHERE n.nspname = '<schema_name>'
  AND p.proname = '<function_name>';
```

[PSQL_SMOKE]:
- `\d+ <schema>.<table>`: table columns, policies, privileges, and comments where `psql` exposes them.
- `\d+ <schema>.<view>`: view definition, options, privileges, and comments.
- `\dm+ <schema>.<matview>`: materialized-view listing and comments.
- `\dd <pattern>`: descriptions for supported object classes that do not have a more specific describe command.
- `\dAo+ <pattern>`: operator-family inspection route for leakproof operator support.

## [6][RECOMMENDATIONS]

[ADD][CATALOG_SECURITY_PROOF]:
Add one PostgreSQL verification-route rule near `[CATALOG_EXTRACTION]`:

```text
Security comments on policies, RLS tables, views, materialized views, and functions must have a catalog proof route: `pg_policies` or `pg_policy` for policy shape, `pg_class.relrowsecurity` and `relforcerowsecurity` for table state, `pg_roles.rolbypassrls` for bypass roles, `pg_class.reloptions` and `pg_get_viewdef` for view mode and definition, `pg_class.relispopulated` plus refresh/index checks for materialized views, and `pg_proc.proleakproof` or `prosecdef` for function security claims.
```

Reason: the current capsule says what comments own, but future agents need exact verification routes before accepting drift-prone security prose.

[CHANGE][VIEW_FIELDS]:
Tighten the view bullet from "security mode, freshness, refresh contract, barrier or invoker behavior" into two rows or clauses:
- Views: projection scope, owner privilege mode, `security_invoker` behavior, `security_barrier` behavior, `CHECK OPTION`, RLS policy user, function execution mode, and data-exposure rule.
- Materialized views: stored projection, freshness, `relispopulated` state, refresh owner or event, `CONCURRENTLY` eligibility, unique-index proof, stale-data tolerance, and data-exposure rule.

Reason: views and materialized views share catalog relation identity but have different security and freshness contracts.

[CHANGE][LEAKPROOF_BOUNDARY]:
Add a boundary sentence:

```text
Leakproof comments name a verified `pg_proc.proleakproof` or operator-family fact and the caller-visible planner consequence; they do not assert full data noninterference or describe exploit steps.
```

Reason: official PostgreSQL docs limit the protection and explicitly warn about covert channels.

[CHANGE][POLICY_COMMENTS]:
Keep policy comments on `COMMENT ON POLICY`, but require the comment to name only non-secret semantics:
- access invariant;
- command scope;
- role scope;
- `USING` and `WITH CHECK` distinction;
- permissive or restrictive combination;
- table-owner and `BYPASSRLS` assumptions;
- trigger timing when `BEFORE ROW` triggers affect `WITH CHECK`;
- race or covert-channel rationale when the policy expression cannot carry it.

Reason: this matches the active capsule and makes the policy comment scoreable against `pg_policies`.

[ADD][MATERIALIZED_VIEW_ANTI_PATTERN]:
Add a PostgreSQL anti-pattern:

```text
Rejected: a materialized-view comment that says "secure reporting view" without freshness, refresh route, stored-data exposure, and `CONCURRENTLY` eligibility proof.
Reason: materialized views return stored rows directly and can be stale or unscannable; ordinary view-security wording hides the real caller contract.
```

Reason: materialized-view misuse is the likely drift point in this topic.

[NO_CHANGE][COMMENT_VISIBILITY]:
Keep the current warning that `COMMENT ON` must not contain secrets, credentials, privileged assumptions, exploit details, tenant IDs, or sensitive operational data.

Reason: official PostgreSQL docs say comments have no security mechanism for connected users.

## [7][DRAFT_EDIT_MAP]

[PRIMARY_OWNER]:
- `docs/standards/reference/code-documentation.md` owns PostgreSQL source comments, catalog comments, catalog extraction, generated-reference handoffs, and language capsule wording.

[SUPPORTING_OWNER]:
- `docs/standards/proof.md` owns the proof labels and freshness field meanings. Do not copy a proof schema into the PostgreSQL capsule beyond route-specific examples.

[ROUTE_AWAY]:
- Generated SQL dictionaries or catalog extracts belong in `reference.md` or `api.md` routes when configured.
- Operational recovery for failed refreshes, lock contention, or access incidents belongs in runbook routes, not source comments.
- How to implement RLS policies or refresh jobs belongs in how-to routes, not code-documentation standards.

## [8][CONFIDENCE]

[HIGH]:
- RLS default-deny, policy command and role scope, `USING` and `WITH CHECK`, permissive and restrictive combination, owner and `BYPASSRLS` bypass, and `FORCE ROW LEVEL SECURITY` behavior are supported by current PostgreSQL 18 docs.
- `security_barrier` and `security_invoker` are distinct view options with different security consequences.
- `COMMENT ON` supports policies, views, and materialized views, and comments are broadly visible to connected database users.
- `pg_policy`, `pg_policies`, `pg_class`, `pg_proc`, `pg_roles`, and system information functions provide verification routes for the claims this report covers.

[MEDIUM]:
- `pg_class.reloptions` as the easiest direct query for `security_barrier` and `security_invoker` options. It is a practical catalog route, but future active edits should verify exact option rendering against a live PostgreSQL 18.4 instance or `psql \d+`.

[LOW]:
- Any local Rasm PostgreSQL runtime behavior. `psql` is not installed in this workspace, and no database was available.

## [9][PROOF_GAPS]

- No PostgreSQL runtime was available; all SQL verification routes are source-backed templates.
- No active standards were edited, so no full active-corpus link or anchor validation was required.
- PostgreSQL 19 beta was released on 2026-06-04, but the official documentation landing still marks `18` as current. This report uses PostgreSQL 18.4/current documentation, not beta behavior.

## [10][TRANSCRIPT]

1. Searched memory for current `docs/standards` guidance and confirmed `.reports/` is source material only.
2. Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/README.md`, `docs/standards/AGENTS.md`, `information-structure.md`, `proof.md`, `style-guide.md`, `formatting.md`, and the full target `reference/code-documentation.md`.
3. Read sibling research reports to match local `.reports/` report shape.
4. Queried current official PostgreSQL documentation for RLS, policies, views, materialized views, comments, catalogs, functions, roles, system information functions, and `psql` describe routes.
5. Checked `psql --version`; local command failed because `psql` is unavailable.
6. Checked worktree status for the assigned report directory and target active standard; active standard changes were pre-existing and untouched.
7. Wrote only `.reports/code-documentation-050626/track-postgres/04-postgres-rls-views.md`.

## [11][CLOSE_CHECK]

- [x] Assigned report file exists.
- [x] No active standards were edited by this worker.
- [x] Current primary PostgreSQL sources are listed.
- [x] Verification routes are marked as templates because local `psql` was unavailable.
- [x] `git diff --check -- .reports/code-documentation-050626/track-postgres/04-postgres-rls-views.md` passes.
