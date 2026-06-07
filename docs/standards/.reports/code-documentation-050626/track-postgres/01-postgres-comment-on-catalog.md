# [POSTGRES_01_COMMENT_ON_CATALOG_RESEARCH]

Research date: 2026-06-05.
Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`.
Assignment: POSTGRES 1; PostgreSQL 18.4 `COMMENT ON` and catalog comment semantics: object coverage, visibility, `pg_description`, `pg_shdescription`, shared objects, and security exposure.
Mutation boundary: no active standards edited; this report is the only assigned output file.

## [1][EXECUTIVE_FINDINGS]

[F1][KEEP]: `docs/standards/reference/code-documentation.md` is directionally correct. PostgreSQL 18.4 treats `COMMENT ON` as object metadata, exposes comments through `psql` describe commands and comment-information functions, stores ordinary object comments in `pg_description`, stores shared-object comments in `pg_shdescription`, and warns that comments have no viewing-security mechanism.

[F2][CHANGE]: The current PostgreSQL capsule under-represents `COMMENT ON` object coverage. The official syntax covers access methods, aggregates, casts, collations, conversions, databases, event triggers, foreign data wrappers, languages, large objects, operators, operator classes, operator families, roles, rules, sequences, servers, statistics objects, tablespaces, text-search objects, transforms, and triggers in addition to the objects already named by the capsule.

[F3][CHANGE]: Split database-local and shared-object extraction semantics explicitly. `pg_description` stores descriptions for database objects, including column comments through `objsubid`; `pg_shdescription` is cluster-wide and stores comments for shared objects. Generated catalog routes should choose `obj_description(oid, catalog)`, `col_description(table_oid, column_number)`, or `shobj_description(oid, catalog)` by object kind.

[F4][CHANGE]: Tighten the security warning from "broadly visible" into the exact PostgreSQL exposure model. Any user connected to a database can see comments for objects in that database; for shared objects such as databases, roles, and tablespaces, any user connected to any database in the cluster can see the shared comments. This supports a hard ban on secrets, privileged assumptions, tenant identifiers, security-critical internals, and sensitive operational details in `COMMENT ON`.

[F5][NO_CHANGE]: Do not turn `code-documentation.md` into a full PostgreSQL object encyclopedia. The standard should name the complete coverage shape and extraction rules, then keep semantic guidance grouped by comment job: identity, invariant, security boundary, planner rationale, lifecycle, replication scope, and generated catalog route.

## [2][REPO_EVIDENCE]

[REQUESTED_FILES_READ]:
- `docs/standards/reference/code-documentation.md`: full file read with line numbers.
- `docs/standards/README.md`: full file read with line numbers.
- `docs/standards/AGENTS.md`: full file read with line numbers.
- `docs/standards/information-structure.md`: full file read with line numbers.
- `docs/standards/proof.md`: full file read with line numbers.
- `docs/standards/style-guide.md`: full file read with line numbers.
- `docs/standards/formatting.md`: full file read with line numbers.
- ``: full file read with line numbers.

[GOVERNING_INSTRUCTIONS_READ]:
- `CLAUDE.md`: confirms Markdown routes through `docs/standards`, current source verification for research, and docs-only proof boundaries.
- `AGENTS.md`: confirms docs work routes through `docs/standards/README.md`, nested overlays, full target-file reads, and no C# static, test, or bridge proof claims for docs-only instruction edits.
- `docs/standards/AGENTS.md`: confirms active corpus routing, rule owners, omission of absent fields, and the ban on secrets, nonpublic machine paths, session commentary, and invented tooling claims.

[ACTIVE_STANDARD_SPANS]:
- `docs/standards/reference/code-documentation.md:3-14`: source comments own omitted caller-visible semantics, side effects, security exposure, lifecycle signals, and routes; generated reference pages, lookup facts, entry maps, architecture, support, tasks, tutorials, and runbooks route away.
- `docs/standards/reference/code-documentation.md:71-83`: source truth separates machine shape, semantic contract, generated reference, and routed documentation.
- `docs/standards/reference/code-documentation.md:124-129`: catalog surfaces apply to SQL objects and comments own durable object meaning, security boundary, planner or migration rationale, and catalog extraction route.
- `docs/standards/reference/code-documentation.md:299-324`: the PostgreSQL capsule already distinguishes `COMMENT ON` catalog documentation from SQL source comments, names `pg_description`, `pg_shdescription`, `obj_description`, `col_description`, `shobj_description`, and rejects secrets or sensitive data in `COMMENT ON`.
- `docs/standards/reference/code-documentation.md:383-417`: validation already checks PostgreSQL catalog comments use `COMMENT ON` for durable schema meaning and keeps docs-only work out of executable rails.
- `docs/standards/proof.md:78-85`: maintained upstream sources are the correct proof path for changing tools and platform semantics when repository truth is silent.
- `docs/standards/information-structure.md:176-184`: grouped definition blocks fit independently scannable source notes better than one-row tables.
- `docs/standards/formatting.md:140-149`: bracketed heading idiom and uppercase labels apply to this report.

[WORKTREE_CONTEXT]:
- `git status --short -- .reports/code-documentation-050626 docs/standards/reference/code-documentation.md docs/standards/README.md docs/standards/AGENTS.md` showed pre-existing `M docs/standards/reference/code-documentation.md` and several untracked sibling `.reports/` reports before this report write.
- No active standards were edited by this worker.

## [3][PRIMARY_SOURCES]

[POSTGRESQL_18_4]:
- PostgreSQL 18.4 documentation landing: https://www.postgresql.org/docs/18/index.html
- PostgreSQL 18.4 release notes: https://www.postgresql.org/docs/18/release-18-4.html

[COMMENT_AND_CATALOGS]:
- PostgreSQL 18 `COMMENT`: https://www.postgresql.org/docs/18/sql-comment.html
- PostgreSQL 18 `pg_description`: https://www.postgresql.org/docs/18/catalog-pg-description.html
- PostgreSQL 18 `pg_shdescription`: https://www.postgresql.org/docs/18/catalog-pg-shdescription.html
- PostgreSQL 18 comment-information functions: https://www.postgresql.org/docs/18/functions-info.html#FUNCTIONS-INFO-COMMENT-TABLE
- PostgreSQL 18 system catalog overview: https://www.postgresql.org/docs/18/catalogs-overview.html

## [4][SOURCE_NOTES]

[VERSION_FRESHNESS]:
- The official PostgreSQL 18 documentation landing says `PostgreSQL 18.4 Documentation`.
- The official 18.4 release notes give release date `2026-05-14`.
- The docs page itself is the maintained primary source for current PostgreSQL 18 command and catalog behavior; no local `psql` runtime claim is made by this report.

[COMMENT_COMMAND]:
- `COMMENT` stores, replaces, or removes a comment on a database object.
- One comment string is stored for each object; a new `COMMENT` command replaces the existing comment.
- `NULL` or an empty string removes the comment.
- Comments are automatically dropped when their object is dropped.
- PostgreSQL acquires a `SHARE UPDATE EXCLUSIVE` lock on the object being commented.
- Most objects require object ownership to set the comment; superusers can comment on anything.
- Roles and access methods need special permission treatment because they do not have owners.
- `COMMENT` is not in the SQL standard.

[OBJECT_COVERAGE]:
- The official syntax covers `ACCESS METHOD`, `AGGREGATE`, `CAST`, `COLLATION`, `COLUMN`, table and domain `CONSTRAINT`, `CONVERSION`, `DATABASE`, `DOMAIN`, `EXTENSION`, `EVENT TRIGGER`, `FOREIGN DATA WRAPPER`, `FOREIGN TABLE`, `FUNCTION`, `INDEX`, `LARGE OBJECT`, `MATERIALIZED VIEW`, `OPERATOR`, `OPERATOR CLASS`, `OPERATOR FAMILY`, table `POLICY`, `LANGUAGE`, `PROCEDURE`, `PUBLICATION`, `ROLE`, `ROUTINE`, `RULE`, `SCHEMA`, `SEQUENCE`, `SERVER`, `STATISTICS`, `SUBSCRIPTION`, `TABLE`, `TABLESPACE`, `TEXT SEARCH CONFIGURATION`, `TEXT SEARCH DICTIONARY`, `TEXT SEARCH PARSER`, `TEXT SEARCH TEMPLATE`, `TRANSFORM`, `TRIGGER`, `TYPE`, and `VIEW`.
- Function, procedure, routine, and aggregate identity uses input-relevant argument types; PostgreSQL docs state `COMMENT` does not pay attention to argument names and does not need `OUT` arguments for identity.
- Column comments identify a column through the containing relation and attribute number because table columns do not have their own OIDs.

[PG_DESCRIPTION]:
- `pg_description` stores optional descriptions for each database object.
- Descriptions can be manipulated with `COMMENT` and viewed with `psql` `\d` commands.
- Many built-in system object descriptions are present in initial `pg_description` contents.
- `objoid` is the object OID, `classoid` is the OID of the system catalog containing the object, `objsubid` is the table-column number for column comments and zero for other object types, and `description` is the arbitrary text.

[PG_SHDESCRIPTION]:
- `pg_shdescription` stores optional descriptions for shared database objects.
- It is shared across all databases in a cluster; there is one copy per cluster rather than one per database.
- Its columns are `objoid`, `classoid`, and `description`; unlike `pg_description`, it has no `objsubid` column.
- PostgreSQL comment-information function docs identify shared comment objects as databases, roles, and tablespaces.

[EXTRACTION_VISIBILITY]:
- `psql` describe commands can view comments.
- `obj_description(object_oid, catalog_name)` retrieves database-object comments by OID plus containing catalog name.
- `obj_description(object_oid)` is deprecated because OIDs are not guaranteed unique across system catalogs.
- `col_description(table_oid, column_number)` retrieves table-column comments because columns lack their own OIDs.
- `shobj_description(object_oid, catalog_name)` retrieves comments on shared objects and uses the same OID plus catalog-name shape as `obj_description`.
- `COMMENT` docs state that any user connected to a database can see all comments for objects in that database.
- For shared objects such as databases, roles, and tablespaces, comments are stored globally so any user connected to any database in the cluster can see all comments for shared objects.

[SECURITY_EXPOSURE]:
- PostgreSQL docs explicitly state that there is no security mechanism for viewing comments.
- PostgreSQL docs explicitly warn against security-critical information in comments.
- The current Rasm capsule's rejection of secrets, credentials, privileged assumptions, exploit details, credential routes, tenant IDs, and sensitive operational data is supported and should be kept.

## [5][RECOMMENDATIONS]

[KEEP][CATALOG_SURFACE]:
Keep this rule: PostgreSQL `COMMENT ON` is durable schema and catalog documentation, while SQL source comments are local rationale because PostgreSQL treats SQL comments as whitespace before syntax analysis.

Reason: current official docs make `COMMENT ON` the maintained route for object descriptions, and the current standard already uses the right source/comment split.

[CHANGE][OBJECT_COVERAGE]:
Replace the narrow object examples in the PostgreSQL capsule with one syntax-driven coverage sentence:

```text
Catalog comments apply to every object kind supported by PostgreSQL 18.4 `COMMENT ON`, including relation-like objects, columns, constraints, routines, operators, access methods, casts, collations, conversions, languages, text-search objects, foreign-data objects, replication objects, extensions, large objects, event triggers, databases, roles, and tablespaces.
```

Reason: this captures full object coverage without copying the whole command grammar into the standard.

[CHANGE][SHARED_OBJECTS]:
Add a short extraction rule under `CATALOG_EXTRACTION`:

```text
Use `pg_description` plus `obj_description(oid, catalog)` for database-local object comments, `col_description(table_oid, column_number)` for column comments, and `pg_shdescription` plus `shobj_description(oid, catalog)` for shared-object comments on databases, roles, and tablespaces.
```

Reason: this prevents the common `obj_description(oid)` and column-OID mistakes and makes shared-object visibility explicit.

[CHANGE][SECURITY_VISIBILITY]:
Tighten the rejection text:

```text
Reject secrets, credentials, privileged assumptions, exploit details, credential routes, tenant IDs, sensitive operational data, and security-critical internals in `COMMENT ON`: any connected database user can see comments for objects in that database, and any connected user in the cluster can see shared-object comments for databases, roles, and tablespaces.
```

Reason: PostgreSQL's visibility model is stronger than a generic "public metadata" warning and should be retained at the close of the PostgreSQL capsule.

[CHANGE][FUNCTION_IDENTITY]:
Add one compact identity note for executable objects:

```text
For functions, procedures, routines, and aggregates, comment identity is determined by input-relevant argument types, not argument names; omit `OUT`-only arguments when the object identity does not require them.
```

Reason: generated catalog extractors and migration authors can otherwise create comments that fail to bind to the intended object or appear to document a different overload.

[CHANGE][LOCAL_RATIONALE]:
Keep migration and function-body SQL comments for non-durable rationale, but add that local rationale must not carry the durable schema meaning that belongs in `COMMENT ON`.

Reason: this keeps migration lock/backfill/search-path rationale in source while preventing hand-written SQL comments from becoming an unextractable data dictionary.

[REMOVE][NO_PARALLEL_DICTIONARY]:
Do not add or preserve hand-maintained Markdown data dictionaries for PostgreSQL object comments when catalog extraction can read `pg_description`, `pg_shdescription`, and the object catalogs.

Reason: PostgreSQL already owns the comment storage and extraction route; a parallel Markdown dictionary will drift.

## [6][NO_CHANGE_CONFIRMATIONS]

[NO_CHANGE][CORE_MODEL]:
No change recommended to the opening code-documentation contract. Code documentation should still exist only when public callers need semantics that declarations, schemas, SQL objects, or catalogs cannot express.

[NO_CHANGE][SECURITY_BAN]:
No weakening recommended for the PostgreSQL `COMMENT ON` secret and sensitive-data rejection. The current capsule already bans the right classes of information; the needed change is to anchor the ban in PostgreSQL's exact visibility model.

[NO_CHANGE][CATALOG_EXTRACTION_ROUTE]:
No change recommended to `pg_description`, `pg_shdescription`, `obj_description`, `col_description`, `shobj_description`, and `psql` describe output as generated catalog routes. The active standard names the right routes.

[NO_CHANGE][PROOF_OWNER]:
No proof-field vocabulary should be added to the PostgreSQL capsule. `proof.md` owns `Evidence`, `Generated from`, `Source of truth`, `Last verified`, and `Review trigger`.

[NO_CHANGE][DOCS_ONLY_RAIL]:
No C#, TypeScript, Python, Bash, SQL runtime, static, test, bridge, or generated-reference rail is required for this research report. A future wording-only active standards edit should use docs-only proof unless it changes generated artifacts, repository tooling, or executable source.

## [7][DRAFT_EDIT_MAP]

[PRIMARY_OWNER]:
- `docs/standards/reference/code-documentation.md`: owns PostgreSQL source comments, `COMMENT ON`, catalog comments, generated catalog extraction, security exposure in comments, and anti-patterns for source comments versus durable schema meaning.

[SUPPORTING_OWNER]:
- `docs/standards/proof.md`: owns claim-level evidence and freshness fields for any active edit that cites PostgreSQL 18.4 behavior.
- `docs/standards/information-structure.md`: owns whether a future edit uses grouped lists, records, or tables for object coverage.
- `docs/standards/style-guide.md`: owns final sentence mechanics and source-name exactness.

[DO_NOT_EDIT_FOR_THIS]:
- `docs/standards/README.md` already routes code documentation correctly.
- `docs/standards/AGENTS.md` already carries local read scope, active-corpus boundaries, and safety exclusions.
- `docs/standards/proof.md` already owns proof fields; do not duplicate its vocabulary in the PostgreSQL capsule.

## [8][CONFIDENCE]

[HIGH]:
- PostgreSQL 18.4 is the current PostgreSQL 18 documentation branch verified from the official docs landing and release notes.
- `COMMENT ON` object coverage, one-comment replacement/removal semantics, ownership and special permission notes, and no-SQL-standard compatibility are directly supported by official `COMMENT` docs.
- `pg_description`, `pg_shdescription`, `obj_description`, `col_description`, and `shobj_description` semantics are directly supported by official catalog and function docs.
- The security warning is directly supported by official `COMMENT` docs.

[MEDIUM]:
- The exact placement of the object-coverage sentence inside the active PostgreSQL capsule. The content belongs there, but an active editor should choose the smallest wording that fits the final rewritten capsule.
- Whether to keep a few object-specific semantic examples after broadening coverage. Examples help for policies, functions, indexes, views, and replication objects, but a long object-by-object list would bloat the standard.

[LOW]:
- Any claim about local PostgreSQL runtime behavior in this checkout. This report verified official PostgreSQL documentation and did not run a local PostgreSQL server or `psql` extraction query.

## [9][TRANSCRIPT_SUMMARY]

Commands run from `/Users/bardiasamiee/Documents/99.Github/Rasm`:
- `rg` over `/Users/bardiasamiee/.codex/memories/MEMORY.md` for recent `docs/standards` context.
- `fd` for instruction and standards file discovery.
- `nl -ba` full reads of `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, `docs/standards/reference/code-documentation.md`, `docs/standards/information-structure.md`, `docs/standards/proof.md`, `docs/standards/style-guide.md`, `docs/standards/formatting.md`, and ``.
- `git status --short -- .reports/code-documentation-050626 docs/standards/reference/code-documentation.md docs/standards/README.md docs/standards/AGENTS.md` before writing; pre-existing active-standard modification and sibling untracked reports were observed.
- Web primary-source checks for PostgreSQL 18.4 docs, `COMMENT`, `pg_description`, `pg_shdescription`, comment-information functions, and the system catalog overview.

Write action:
- Created `.reports/code-documentation-050626/track-postgres/01-postgres-comment-on-catalog.md` only.

## [10][CLOSE_CHECK]

- [x] Assigned report file created at `.reports/code-documentation-050626/track-postgres/01-postgres-comment-on-catalog.md`.
- [x] Requested target standard and governing standards were read.
- [x] Current primary sources were used for PostgreSQL 18.4 claims.
- [x] Report includes source notes, findings, recommendations, no-change confirmations, confidence, and proof gaps.
- [x] No active standards were edited.
