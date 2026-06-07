# [POSTGRES_05_EXTRACTION_GENERATED_RESEARCH]

PostgreSQL catalog comments are durable schema documentation only when generated dictionaries extract them from PostgreSQL catalog identity and catalog facts. `sqlfluff --dialect postgres` can prove parseability and style for SQL text, but it does not connect to PostgreSQL, inspect catalogs, verify object existence, prove privileges, or prove semantic documentation.

## [1][SCOPE]

Assignment: POSTGRES 5, generated dictionaries and extraction, with focus on `psql` describe routes, `col_description`, `obj_description`, `shobj_description`, SQLFluff limits, schema-qualified references, and extension, publication, and subscription documentation.

Mutation boundary: no active standards were edited. This report is the only assigned output file.

Active standard read:
- `docs/standards/reference/code-documentation.md`

Governing standards read:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/agents-md.md`
- `docs/standards/information-structure.md`
- `docs/standards/proof.md`
- `docs/standards/style-guide.md`
- `docs/standards/formatting.md`

Sibling `.reports/` inputs read for consistency:
- `.reports/code-documentation-050626/track-general/02-general-generation.md`
- `.reports/code-documentation-050626/track-general/03-general-security-inline.md`

Local checks:
- `git status --short -- docs/standards .reports/code-documentation-050626/track-postgres/05-postgres-extraction-generated.md` showed pre-existing `M docs/standards/reference/code-documentation.md` and sibling untracked `.reports/` reports before this file was written.
- `sqlfluff --version` returned `sqlfluff, version 4.2.1`.
- `sqlfluff dialects` listed `postgres` as a PostgreSQL dialect inheriting from `ansi`.
- `sqlfluff parse --dialect postgres -` accepted `COMMENT ON EXTENSION`, `COMMENT ON PUBLICATION`, `COMMENT ON SUBSCRIPTION`, `obj_description`, `col_description`, and `shobj_description` sample statements.
- `psql --version` failed with `zsh:1: command not found: psql`; no local `psql` describe output was available.

## [2][CURRENT_SOURCES]

Primary sources checked on 2026-06-05:
- PostgreSQL 18.4 docs landing: https://www.postgresql.org/docs/current/
- PostgreSQL 18 `COMMENT`: https://www.postgresql.org/docs/current/sql-comment.htm
- PostgreSQL 18 object and comment information functions: https://www.postgresql.org/docs/current/functions-info.html
- PostgreSQL 18 `pg_description`: https://www.postgresql.org/docs/current/catalog-pg-description.html
- PostgreSQL 18 `pg_shdescription`: https://www.postgresql.org/docs/current/catalog-pg-shdescription.html
- PostgreSQL 18 `psql`: https://www.postgresql.org/docs/current/app-psql.html
- PostgreSQL 18 information schema `columns`: https://www.postgresql.org/docs/current/infoschema-columns.html
- PostgreSQL 18 `CREATE EXTENSION`: https://www.postgresql.org/docs/current/sql-createextension.html
- PostgreSQL 18 `pg_extension`: https://www.postgresql.org/docs/current/catalog-pg-extension.html
- PostgreSQL 18 `CREATE PUBLICATION`: https://www.postgresql.org/docs/current/sql-createpublication.html
- PostgreSQL 18 `pg_publication`: https://www.postgresql.org/docs/current/catalog-pg-publication.html
- PostgreSQL 18 `pg_publication_tables`: https://www.postgresql.org/docs/current/view-pg-publication-tables.html
- PostgreSQL 18 `CREATE SUBSCRIPTION`: https://www.postgresql.org/docs/current/sql-createsubscription.html
- PostgreSQL 18 `pg_subscription`: https://www.postgresql.org/docs/current/catalog-pg-subscription.html
- PostgreSQL source `comment.c`: https://doxygen.postgresql.org/comment_8c_source.html
- PostgreSQL source `pg_subscription.h`: https://doxygen.postgresql.org/pg__subscription_8h_source.html
- SQLFluff home: https://www.sqlfluff.com/
- SQLFluff dialects reference: https://docs.sqlfluff.com/en/stable/reference/dialects.html
- SQLFluff CLI reference: https://docs.sqlfluff.com/en/stable/reference/cli.html
- SQLFluff GitHub repository and release metadata: https://github.com/sqlfluff/sqlfluff

PostgreSQL `current` documentation is PostgreSQL 18.4 on the checked date. PostgreSQL 19 Beta 1 was announced on 2026-06-04, but it is a development version and should not become the standards baseline unless the repo intentionally targets PostgreSQL 19 pre-release behavior.

## [3][FINDINGS]

[F1][KEEP_CATALOG_BOUNDARY]:
The active standard's PostgreSQL split is correct: `COMMENT ON` is the durable catalog-comment surface, while SQL source comments are local rationale. PostgreSQL documents SQL comments as not the durable schema-description path, and `COMMENT` stores, replaces, or removes object comments attached to database objects.

[F2][COMMENTS_ARE_PUBLIC_METADATA]:
The PostgreSQL `COMMENT` page explicitly warns that there is no security mechanism for viewing comments. Any user connected to a database can see object comments in that database, and comments on shared objects are visible cluster-wide. The active standard's rejection of secrets, credentials, privileged assumptions, exploit detail, tenant IDs, and sensitive operational data in `COMMENT ON` is strongly supported.

[F3][DESCRIPTION_STORAGE_SPLIT]:
`pg_description` stores comments for database objects, including column comments via `objsubid`, and `pg_shdescription` stores comments for shared database objects. Current PostgreSQL source narrows the shared-comment write path to databases, tablespaces, and roles; comments on all other object types go through `pg_description`. That means a generated dictionary should not blindly route every shared catalog relation through `shobj_description`.

[F4][DESCRIPTION_FUNCTIONS_NEED_CATALOG_IDENTITY]:
Generated extraction should use `col_description(table_oid, column_number)` for columns, `obj_description(object_oid, catalog_name)` for ordinary objects, and `shobj_description(object_oid, catalog_name)` only for shared-object comments. The one-argument `obj_description(object_oid)` is deprecated because OIDs are not unique across catalogs, so it is not a valid generated-dictionary route.

[F5][OBJECT_IDENTITY_IS_A_DICTIONARY_FIELD]:
`pg_identify_object` returns machine-readable object type, schema, name, and identity for catalog OID, object OID, and sub-object ID. Generated dictionaries should keep object address fields or generated identity beside comments instead of using hand-built display names. `pg_describe_object` is human-readable and may be translated, so it is useful for display or diagnostics but weaker for stable generated keys.

[F6][PSQL_DESCRIBE_IS_A_SMOKE_ROUTE]:
`psql` describe commands are good human smoke checks because many `\d` commands show descriptions with `+`, and `\dd` lists descriptions for object families not covered by their own describe command. They are not the generator source. Use catalog queries and description functions for generated dictionaries, then use `psql` describe output only to spot-check human-visible surfacing.

[F7][SCHEMA_QUALIFICATION_IS_CONDITIONAL]:
`COMMENT ON` says names of objects that live in schemas can be schema-qualified. That rule applies to tables, views, functions, types, indexes, policies through table names, and similar schema-contained objects. It does not make extension names schema-qualified: PostgreSQL states the extension object itself is not considered within any schema, though extension-owned objects can be in schemas. Publication and subscription names are also database-level object names in their respective commands, not schema-contained names.

[F8][EXTENSION_DICTIONARIES_NEED_CATALOG_FACTS]:
Extension comments should describe installed purpose, version gate, operational boundary, and provider assumption only when those are durable. Generated dictionaries should extract `pg_extension` facts such as extension name, owner, exported-object schema, relocatable flag, version, config tables, and filter conditions beside the comment. The active standard's "extensions" bullet is directionally correct, but a future edit should avoid implying that an extension itself has a schema-qualified name.

[F9][PUBLICATION_DICTIONARIES_NEED_EXPANDED_SCOPE]:
Publication comments should describe replication scope, generated-column policy, row-filter or column-list meaning, and operational boundary at a high level. Generated dictionaries should include `pg_publication` flags and use `pg_publication_tables` when readers need the expanded mapping from publications to eligible tables, including all-tables and schema-level publications.

[F10][SUBSCRIPTION_DICTIONARIES_NEED_REDACTION]:
Subscription comments should describe subscriber intent and operational boundary, not connection strings, credentials, or privileged routes. PostgreSQL documents `pg_subscription` as a shared catalog and says `subconninfo` access is revoked from normal users because it could contain plain-text passwords. Generated dictionaries should exclude or redact `subconninfo` and should preserve only nonsecret catalog facts such as target database, enabled state, slot name, sync settings, subscribed publication names, origin policy, and failure behavior.

[F11][SQLFLUFF_LIMIT_IS_SEMANTIC]:
SQLFluff official docs describe parsing without needing database access, and the CLI docs expose parse, lint, fix, and format commands. Its PostgreSQL dialect is current and local `sqlfluff 4.2.1` accepted the relevant sample statements. That proves syntax support for the checked text in the local install, not object existence, catalog extraction, comment visibility, privilege safety, RLS meaning, replication behavior, or dictionary correctness.

## [4][EXTRACTION_MODEL]

Generated dictionaries should be database-derived records. The dictionary key should come from catalog address or generated object identity, the comment should come from a description function, and the surrounding fields should come from the owning catalog or stable system view.

[RECOMMENDED_RECORD_FIELDS]:
- `classid`: system catalog OID for the object family.
- `objid`: object OID inside that catalog.
- `objsubid`: column number for column comments, otherwise `0`.
- `object_type`: value from `pg_identify_object`.
- `schema`: schema from `pg_identify_object` when present.
- `name`: object name from `pg_identify_object` when present.
- `identity`: generated identity from `pg_identify_object`.
- `source_catalog`: catalog or view used for the row, such as `pg_class`, `pg_attribute`, `pg_extension`, `pg_publication`, `pg_publication_tables`, or `pg_subscription`.
- `description_route`: `col_description`, `obj_description`, or `shobj_description`.
- `description`: extracted comment text.
- `catalog_facts`: compact source-owned facts required to interpret the object.
- `visibility`: database-visible or shared-object-visible, with redaction notes where needed.
- `review_trigger`: object rename, type change, comment change, catalog query change, PostgreSQL version target change, or generated dictionary schema change.

Column extraction should join `pg_class`, `pg_namespace`, and `pg_attribute`, then call `col_description(pg_class.oid, pg_attribute.attnum)`. Information schema `columns` is stable and portable for many shape facts, but it omits PostgreSQL-specific features and shows only columns the current user can access; generated dictionaries that need PostgreSQL-specific comments or catalog identity should use PostgreSQL catalogs.

Ordinary object extraction should use the object's source catalog and `obj_description(object_oid, '<catalog_name>')`. Use two-argument `obj_description` only. The generated row should include enough catalog context to distinguish objects whose OIDs collide across catalogs.

Shared object extraction should use `shobj_description(object_oid, '<catalog_name>')` for objects whose comments are actually stored in `pg_shdescription`. Current source identifies databases, tablespaces, and roles for that path. If a future PostgreSQL release changes this split, the extraction route should be refreshed from source or local server behavior.

## [5][RECOMMENDATIONS]

[ADD]:
- Add a generated-dictionary extraction rule: dictionary rows come from catalog address, `pg_identify_object`, owning catalog facts, and description functions; they do not copy migration prose or hand-maintained Markdown table rows.
- Add a `psql` proof nuance: `\d+`, `\dd`, `\dx+`, `\dRp+`, and `\dRs+` are human smoke routes for visible descriptions and replication objects, not generated-dictionary sources.
- Add a SQLFluff proof limit: `sqlfluff --dialect postgres` proves parse, lint, or formatting behavior for SQL text only; it cannot prove catalog comments, object identity, privileges, data exposure, or replication semantics.
- Add an object identity rule: generated dictionary display should prefer `pg_identify_object` output or catalog-derived schema/name fields rather than ad hoc string concatenation.
- Add a subscription redaction rule: generated subscription dictionaries must not expose `subconninfo` or connection credentials, even when catalog access allows extraction.

[CHANGE]:
- Change any broad "schema-qualified object names in `COMMENT ON`" wording to "schema-qualify schema-contained objects; keep extension, publication, and subscription identity in the form PostgreSQL commands define."
- Change extraction guidance from generic `obj_description(oid)` to explicit `obj_description(oid, catalog)` and mark the one-argument form rejected.
- Change extension wording to distinguish the extension object from extension-owned schema objects. `pg_extension.extnamespace` identifies the schema containing exported objects; it does not mean the extension itself belongs to that schema.
- Change publication and subscription guidance to require catalog facts beside comments: publications need publish flags and expanded table mapping where relevant; subscriptions need nonsecret replication settings and subscribed publication names.

[REMOVE]:
- Remove any path that lets a generated dictionary be maintained by hand when catalog extraction is possible.
- Remove source SQL comments as a durable schema-meaning route. Keep migration and function-body comments only for local rationale that cannot live in `COMMENT ON`.
- Remove any implied claim that SQLFluff proves semantic documentation, replication behavior, or catalog extraction.
- Remove examples that put connection strings, tenant IDs, credential routes, or privileged operational assumptions inside `COMMENT ON`.

## [6][NO_CHANGE_CONFIRMATIONS]

- Keep `COMMENT ON` as the PostgreSQL durable catalog documentation surface.
- Keep SQL source comments as local rationale only.
- Keep `pg_description` and `pg_shdescription` as storage/extraction concepts.
- Keep `col_description`, `obj_description(oid, catalog)`, and `shobj_description` as generated-reference routes.
- Keep `psql` describe output as a human smoke route.
- Keep "generated dictionaries must include catalog facts beside comments instead of copying migration prose."
- Keep "sqlfluff --dialect postgres is formatting and linting proof only."
- Keep the PostgreSQL `COMMENT ON` safety warning.

## [7][DRAFT_EDIT_MAP]

Primary owner:
- `docs/standards/reference/code-documentation.md`

Likely insertion points:
- `[6.5][POSTGRESQL_18_4]` under `[CATALOG_EXTRACTION]` for generated-dictionary extraction and SQLFluff limits.
- `[7][LIFECYCLE_REFERENCES]` cross-reference rule for schema-qualified PostgreSQL references, if active editors want a broader reference-routing sentence.
- `[10][VALIDATION]` under `[ROUTES_GENERATION]` or `[COMMENTS_BOUNDARY]` for the no-hand-maintained-dictionary and no-connection-string checks.

Do not edit for this report:
- `docs/standards/README.md`; routing is already sufficient.
- `docs/standards/proof.md`; it already owns evidence labels and docs-as-code gates.
- `docs/standards/reference/api.md`; generated API records are adjacent only if active generated catalog pages are adopted.

## [8][CONFIDENCE]

[HIGH]:
- `COMMENT ON` is the durable PostgreSQL object-comment mechanism.
- PostgreSQL comments are broadly visible and must not contain secrets or sensitive operational data.
- `col_description`, two-argument `obj_description`, and `shobj_description` are the documented comment extraction functions.
- One-argument `obj_description` is deprecated and should be rejected for generated dictionaries.
- SQLFluff cannot prove catalog or semantic documentation because it parses SQL without database access.

[MEDIUM]:
- Subscription comment extraction should use `obj_description(oid, 'pg_subscription')` rather than `shobj_description`, based on current PostgreSQL source `comment.c` saying only databases, tablespaces, and roles use `pg_shdescription`. Official user-facing docs prove `pg_subscription` is shared and prove `pg_shdescription` exists, but they do not spell out the `COMMENT ON SUBSCRIPTION` storage path. Verify against the target PostgreSQL server before encoding a concrete subscription extraction SQL snippet.
- `pg_identify_object` should be the preferred generated identity source for all dictionary rows. The function is documented for machine-readable identity, but exact row shape should be validated against the target PostgreSQL version before freezing a generated dictionary schema.

[LOW]:
- Any claim about local PostgreSQL runtime behavior in this checkout. `psql` is unavailable, and no database-backed extraction query was run.

## [9][PROOF_GAPS]

- No `psql` describe command ran because `psql` is not installed or not on `PATH`.
- No live PostgreSQL catalog query ran, so extraction SQL snippets remain design guidance, not verified local output.
- SQLFluff parse smoke used local `sqlfluff 4.2.1`; future active edits should re-check the installed version or repository tool route before claiming local parser support.
- PostgreSQL source checks used current online source because user-facing docs do not state every comment storage branch. If the active standard needs exact storage behavior for PostgreSQL 18.4 only, verify against PostgreSQL 18 source or a PostgreSQL 18.4 server.

## [10][CLOSE_CHECK]

- [x] Assigned report file created at `.reports/code-documentation-050626/track-postgres/05-postgres-extraction-generated.md`.
- [x] Requested target standard and governing standards were read.
- [x] Current primary sources were used for drift-prone PostgreSQL and SQLFluff claims.
- [x] Active standards were not edited.
- [x] `psql` unavailability is recorded as a proof gap.
