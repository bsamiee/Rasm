# [POSTGRES_02_MIGRATION_COMMENTS]

PostgreSQL migration comments should preserve local execution rationale for lock, rewrite, backfill, validation, rollback, and proof-query choices. Durable schema meaning belongs in `COMMENT ON` catalog comments; SQL source comments are whitespace to PostgreSQL and should not carry public data-dictionary truth.

## [1][SCOPE]

Research date: 2026-06-05.
Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`.
Assignment: `POSTGRES 2`; SQL source comments versus catalog comments, migration comments, lock, rewrite, backfill, validation, rollback, and proof query guidance.
Assigned output: `docs/standards/_reports/code-documentation-050626/track-postgres/02-postgres-migrations.md`.
Mutation boundary: no active standards edited; this report is the only assigned output file.

Active standards read fully:
- `docs/standards/reference/code-documentation.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/proof.md`
- `docs/standards/information-structure.md`
- `docs/standards/style-guide.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/formatting.md`

Repo instruction context checked:
- `CLAUDE.md`
- root `AGENTS.md`
- `~/.codex/memories/MEMORY.md` lines for current `docs/standards` routing, `_reports/` status, proof ownership, and docs-only validation.

Current workspace note: `docs/standards/reference/code-documentation.md` was already modified before this report. This worker leaves that active standard and all sibling reports untouched.

## [2][ACTIVE_STANDARD_SNAPSHOT]

`code-documentation.md` already contains the correct PostgreSQL split:
- `COMMENT ON` is the durable schema and catalog-documentation surface.
- SQL source comments are local rationale because PostgreSQL removes comments before syntax analysis.
- Catalog comments own object meaning, invariants, security boundaries, planner purpose, data exposure, and extraction routes.
- Migration source comments own lock level, rewrite behavior, backfill shape, validation phase, privilege window, rollout gate, irreversibility, rollback boundary, extension gate, and proof query.
- Generated catalog references extract from `pg_description` or `pg_shdescription` and use description functions plus `psql` describe output.
- The rejection list already bans secrets and sensitive operational data in `COMMENT ON`, rejects source comments for durable schema meaning, and rejects `obj_description(oid)` without catalog identity.

Main refinement opportunity: the migration-comment bullet is directionally right but under-specified for agents. A later standards edit should keep the catalog/source boundary, then add a compact migration-risk record that tells authors when a SQL source comment needs lock, rewrite, backfill, validation, rollback, and proof-query fields.

## [3][SOURCE_BASIS]

[LOCAL_REPO]:
- `docs/standards/reference/code-documentation.md:3-14`: source comments own omitted caller-visible semantics and route generated references, architecture, tasks, support, and operations away.
- `docs/standards/reference/code-documentation.md:28-42`: comments apply when declarations omit transaction state, lock state, SQL role, tenant scope, side effects, security behavior, or data exposure.
- `docs/standards/reference/code-documentation.md:73-83`: machine shape, semantic contract, generated reference, and routed documentation stay separate.
- `docs/standards/reference/code-documentation.md:124-127`: catalog surfaces include SQL schemas, tables, columns, constraints, indexes, functions, routines, policies, views, publications, subscriptions, and extensions.
- `docs/standards/reference/code-documentation.md:299-324`: PostgreSQL capsule defines `COMMENT ON`, SQL source comments, migration-comment fields, catalog extraction, and rejection rules.
- `docs/standards/reference/code-documentation.md:377-380`: proof for source comments, generated reference output, and catalog output belongs to `proof.md`; source comments carry no proof details unless a generator consumes them.
- `docs/standards/proof.md`: proof fields are claim-level, close to the claim, and exact enough to refresh.

[PRIMARY_POSTGRESQL]:
- PostgreSQL 18 current documentation landing: `https://www.postgresql.org/docs/`; current manual is PostgreSQL 18, with 18.4 released 2026-05-14, while PostgreSQL 19 is beta/development.
- PostgreSQL 18 lexical structure: `https://www.postgresql.org/docs/current/sql-syntax-lexical.html`; SQL comments are not tokens and are removed before further syntax analysis.
- PostgreSQL 18 `COMMENT`: `https://www.postgresql.org/docs/current/sql-comment.html`; comments are set by object owners, viewed through `psql` describe commands and description functions, and have no viewing security mechanism.
- PostgreSQL 18 `pg_description`: `https://www.postgresql.org/docs/current/catalog-pg-description.html`; object comments are stored as optional descriptions with object OID, catalog OID, optional column number, and description text.
- PostgreSQL 18 system information functions: `https://www.postgresql.org/docs/current/functions-info.html`; `obj_description`, `col_description`, and `shobj_description` are the description-function route.
- PostgreSQL 18 `ALTER TABLE`: `https://www.postgresql.org/docs/current/sql-altertable.html`; lock level defaults to `ACCESS EXCLUSIVE` unless noted, nonvolatile defaults avoid rewrite, volatile defaults and many type changes rewrite, `NOT VALID` plus `VALIDATE CONSTRAINT` reduces concurrent-update impact, and validation uses `SHARE UPDATE EXCLUSIVE`.
- PostgreSQL 18 explicit locking: `https://www.postgresql.org/docs/current/explicit-locking.html`; `ACCESS EXCLUSIVE` conflicts with all lock modes and is the only table lock that blocks plain `SELECT`.
- PostgreSQL 18 `pg_locks`: `https://www.postgresql.org/docs/current/view-pg-locks.html`; lock proof can inspect active locks and should use `pg_blocking_pids()` for blockers rather than hand-encoding conflict logic.
- PostgreSQL 18 `CREATE INDEX`: `https://www.postgresql.org/docs/current/sql-createindex.html`; `CREATE INDEX CONCURRENTLY` avoids write-blocking locks, performs extra scans and waits, leaves invalid indexes on failure, cannot run inside a transaction block, and partitioned indexes need per-partition concurrent builds followed by a metadata parent index.
- PostgreSQL 18 progress reporting: `https://www.postgresql.org/docs/current/progress-reporting.html`; `pg_stat_progress_create_index` reports command, phase, lockers, block progress, tuple progress, and partition progress for index creation.
- PostgreSQL 18 `pg_constraint`: `https://www.postgresql.org/docs/current/catalog-pg-constraint.html`; `convalidated` records whether a constraint has been validated, and `pg_get_constraintdef()` is recommended for check-expression extraction.
- PostgreSQL 18 `pg_index`: `https://www.postgresql.org/docs/current/catalog-pg-index.html`; `indisvalid`, `indisready`, and related flags record whether an index is usable for queries and ready for writes.
- PostgreSQL 18 administration functions: `https://www.postgresql.org/docs/current/functions-admin.html`; size and filenode functions such as `pg_relation_size`, `pg_table_size`, `pg_total_relation_size`, and `pg_relation_filenode` support rewrite and disk-impact proof.
- PostgreSQL 18 client defaults: `https://www.postgresql.org/docs/current/runtime-config-client.html`; `lock_timeout` aborts statements waiting too long for locks, and `statement_timeout` aborts statements running too long.

## [4][FINDINGS]

### [4.1][SOURCE_COMMENTS_ARE_NOT_CATALOG_TRUTH]

SQL source comments are a migration-maintainer surface, not database metadata. PostgreSQL treats them as whitespace before syntax analysis, so they are not queryable through catalogs, description functions, `psql` describe output, generated data dictionaries, or runtime introspection.

Standards consequence:
- Keep durable object meaning, tenant boundaries, data exposure, planner purpose, and public schema semantics in `COMMENT ON`.
- Keep migration execution rationale in SQL source comments beside the statement or block that needs it.
- Reject migration prose that explains a permanent schema meaning only in a source comment.
- Reject catalog comments that include rollout details, private operational notes, secret-handling routes, incident context, or temporary proof queries.

Confidence: high. PostgreSQL lexical docs and `COMMENT` docs make the source/catalog boundary explicit, and the active standard already encodes this split.

### [4.2][CATALOG_COMMENTS_ARE_BROADLY_VISIBLE]

`COMMENT ON` is not a private maintainer note. PostgreSQL states that any connected database user can see comments for objects in that database, and shared-object comments are visible cluster-wide to connected users.

Standards consequence:
- Keep the existing ban on secrets, credentials, tenant IDs, privileged assumptions, exploit details, credential routes, and sensitive operational data in `COMMENT ON`.
- Add "migration status" and "rollback plan" to the rejected catalog-comment examples unless the status is durable external support truth routed through support or runbook documentation.
- Favor schema-qualified object names in comments and extraction queries so generated dictionaries do not depend on `search_path`.

Confidence: high. PostgreSQL `COMMENT` docs explicitly state the visibility rule and recommend description functions for retrieval.

### [4.3][MIGRATION_COMMENTS_SHOULD_NAME_LOCK_BEHAVIOR]

A migration source comment should state lock behavior when the SQL statement can block writes, reads, schema changes, validation, concurrent index work, or deployment progress. PostgreSQL `ALTER TABLE` defaults to `ACCESS EXCLUSIVE` unless a subform says otherwise, and when several subcommands run in one `ALTER TABLE`, the strictest lock applies.

Migration-comment field:
- `Lock`: exact expected lock mode, relation scope, whether plain reads proceed, whether writes block, whether schema changes block, and whether timeout settings are intentionally used.

Proof-query route:
- Use `pg_locks` joined to `pg_stat_activity` for active lock observation.
- Use `pg_blocking_pids(pid)` to identify blockers instead of manually encoding the lock conflict matrix.
- Use `lock_timeout` and `statement_timeout` as statement behavior controls only when the migration runner or SQL block actually sets them.

Confidence: high. PostgreSQL locking docs define lock modes, `ALTER TABLE` docs define default lock behavior, and `pg_locks` docs define the inspection surface.

### [4.4][REWRITE_BEHAVIOR_NEEDS_SEPARATE_COMMENT_FIELD]

Lock level and table rewrite are different risks. A migration can lock heavily without rewriting, or rewrite and therefore require extra disk, MVCC visibility caution, index rebuilds, and rollback planning.

Migration-comment field:
- `Rewrite`: `none`, `metadata-only`, `scan-only`, `table-rewrite`, `index-rebuild`, or `unknown`; include the reason when PostgreSQL docs make the case explicit.

Source-backed examples:
- Adding a column with no constraints or with a nonvolatile default avoids a table rewrite.
- Adding a volatile default, stored generated column, identity column, or constrained domain column rewrites the table and indexes.
- Changing column type normally rewrites the table and indexes unless the documented binary-coercible or unconstrained-domain exception applies.
- `ALTER TABLE` rewrites can require significant time and temporary disk space and are not MVCC-safe for old snapshots.

Proof-query route:
- Capture `pg_relation_filenode(<table>::regclass)` before and after when a rewrite/no-rewrite claim matters.
- Capture `pg_table_size`, `pg_indexes_size`, or `pg_total_relation_size` before and after when disk-impact proof matters.
- Avoid claiming `sqlfluff` or formatting proves rewrite behavior; it cannot.

Confidence: high for the field and documented rewrite classes; medium for exact project proof-query shape because the repo may later wrap these checks in a migration tool.

### [4.5][BACKFILL_COMMENTS_SHOULD_DESCRIBE_DATA_SHAPE]

Backfill comments should explain the data-mutation plan that SQL syntax cannot express: batch predicate, ordering, idempotence, write amplification, retry shape, and completion predicate. They should not duplicate the `UPDATE` statement or narrate each assignment.

Migration-comment field:
- `Backfill`: target row set, monotonic predicate, batch size or external runner owner, ordering key, idempotence rule, retry boundary, and expected remaining-row proof.

Proof-query route:
- Use a predicate count such as rows still missing the new value, rows carrying old enum/domain values, or rows whose derived value disagrees with source truth.
- Use a sample reconciliation query only when the row count alone can miss semantic drift.
- Use `pg_stat_progress_copy` only for `COPY`-based backfills, because PostgreSQL progress reporting is command-specific.

Confidence: medium-high. The need follows from active standard fields and PostgreSQL progress/catalog surfaces; exact predicate queries are application-specific and should stay as templates or per-migration comments.

### [4.6][VALIDATION_COMMENTS_SHOULD_TRACK_CONSTRAINT_STATE]

Validation comments need a different route than catalog comments. `NOT VALID` changes rollout behavior and catalog state; the durable object comment should describe the invariant, while the migration source comment describes why validation is staged and how completion is proved.

Migration-comment field:
- `Validation`: `NOT VALID` admission reason, validation statement, expected `pg_constraint.convalidated` or `pg_index.indisvalid` state, and post-validation proof query.

Source-backed examples:
- `ALTER TABLE ADD CONSTRAINT ... NOT VALID` skips the initial table scan and can commit immediately.
- `VALIDATE CONSTRAINT` checks existing rows later while concurrent transactions enforce new rows, and it uses only `SHARE UPDATE EXCLUSIVE` on the altered table.
- Failed `CREATE INDEX CONCURRENTLY` can leave an invalid index that consumes update overhead and may still enforce uniqueness after some failures.

Proof-query route:
- Query `pg_constraint.convalidated` for staged constraints.
- Query `pg_index.indisvalid` and `pg_index.indisready` for concurrently built or attached indexes.
- Use `pg_get_constraintdef()` for constraint-expression display instead of relying on internal `conbin`.

Confidence: high. PostgreSQL `ALTER TABLE`, `CREATE INDEX`, `pg_constraint`, and `pg_index` docs directly support the recommended fields.

### [4.7][ROLLBACK_COMMENTS_SHOULD_STATE_THE_BOUNDARY]

Rollback guidance belongs in migration source comments only when the source block has an irreversible, lossy, non-transactional, concurrent, or staged effect. Generic "rollback by reverting migration" prose is noise unless it changes a maintainer action.

Migration-comment field:
- `Rollback`: transactional rollback path, irreversible step, lossy operation, invalid-index cleanup, staged validation reversal, data restore source, or "forward-only after <gate>" condition.

Source-backed examples:
- `CREATE INDEX CONCURRENTLY` cannot run inside a transaction block and can leave invalid indexes that must be dropped or rebuilt concurrently.
- `DROP COLUMN` is quick but hides the column rather than immediately reclaiming disk; later rewrites reclaim space.
- A table rewrite is not MVCC-safe for transactions with older snapshots, which affects rollback and deploy-window reasoning.

Proof-query route:
- For failed concurrent indexes, query `pg_index` state and drop or rebuild the invalid index as the recovery path.
- For lossy data changes, use a pre-migration snapshot/export/checksum route owned by the migration system or runbook; do not put sensitive backup paths in comments.
- For staged validation rollback, query the constraint or index catalog state before deciding whether the rollback removes an unvalidated object, a validated object, or only a source migration marker.

Confidence: high for concurrent-index and rewrite caveats; medium for broader rollback procedure because project tooling determines exact backup and deploy gates.

### [4.8][PROOF_QUERIES_BELONG_IN_SOURCE_COMMENTS_ONLY_AS_REFRESHABLE_ROUTES]

Proof queries are useful in migration comments when they are the smallest reproducible route to close a risky migration step. They should not become permanent catalog comments, generated dictionaries, or broad runbooks.

Recommended proof-query classes:
- `Lock proof`: current blockers and lock mode through `pg_locks`, `pg_stat_activity`, and `pg_blocking_pids()`.
- `Rewrite proof`: `pg_relation_filenode` and relation size functions before and after.
- `Backfill proof`: remaining-row predicate count and targeted reconciliation sample.
- `Validation proof`: `pg_constraint.convalidated`, `pg_index.indisvalid`, `pg_index.indisready`, and `psql` describe output when human smoke proof is useful.
- `Comment proof`: `obj_description(oid, catalog)`, `col_description(oid, attnum)`, `shobj_description(oid, catalog)`, and `pg_description` or `pg_shdescription` extraction.

Standards consequence:
- A migration source comment may name a proof query or route, but it should not paste unbounded output.
- A generated data dictionary should include catalog facts beside comments and should not copy migration proof prose.
- A standards page should provide proof-query classes and small templates, not project-specific table predicates.

Confidence: high. This follows from `proof.md`, active PostgreSQL capsule language, and PostgreSQL catalog/progress/lock documentation.

## [5][RECOMMENDATIONS]

[KEEP][POSTGRES_SPLIT]:
Keep the current `COMMENT ON` versus SQL source-comment split. This is the highest-value rule in the PostgreSQL capsule and should remain near the top of the capsule.

[CHANGE][MIGRATION_COMMENT_RECORD]:
Replace or expand the single migration-comment bullet with a compact field record:

```text template
Migration source comment: use beside a statement or block whose execution risk is not obvious from SQL syntax.
Lock: lock mode, relation scope, read/write/schema blocking, timeout owner, or omit when ordinary.
Rewrite: none, metadata-only, scan-only, table-rewrite, index-rebuild, or unknown; include the documented reason.
Backfill: target row set, batch predicate, idempotence, retry boundary, and remaining-row proof.
Validation: staged constraint or index state, validation command, catalog state, and proof query.
Rollback: transactional rollback, forward-only gate, invalid-object cleanup, lossy boundary, or restore route.
Proof query: smallest query or generated route that proves the step; omit output transcripts.
```

Reason: the current bullet lists the right concerns but does not tell agents how to decide which fields a migration source comment should carry.

[CHANGE][CATALOG_COMMENT_REJECTS]:
Tighten the PostgreSQL rejection line:
- Reject migration status, rollout windows, rollback plans, backfill predicates, proof-query transcripts, private backup routes, tenant IDs, privileged operational assumptions, and sensitive incident context in `COMMENT ON`.
- Keep permanent object meaning, invariant, unit, planner/access-path purpose, data exposure, policy semantics, and generated-catalog extraction in `COMMENT ON`.

Reason: PostgreSQL comments are broadly visible, while migration operations often contain private deploy and recovery information.

[ADD][PROOF_QUERY_CLASSES]:
Add a short proof-query class list to the PostgreSQL capsule or validation section:
- catalog comment: `obj_description`, `col_description`, `shobj_description`, `pg_description`, `pg_shdescription`, or `psql \d`.
- constraint validation: `pg_constraint.convalidated` plus `pg_get_constraintdef()`.
- concurrent index: `pg_index.indisvalid`, `pg_index.indisready`, and `pg_stat_progress_create_index` while running.
- lock observation: `pg_locks`, `pg_stat_activity`, and `pg_blocking_pids()`.
- rewrite and disk impact: `pg_relation_filenode`, `pg_table_size`, `pg_indexes_size`, or `pg_total_relation_size`.
- backfill completion: project-specific predicate count and reconciliation query.

Reason: proof classes make comments refreshable without turning the standards file into a migration cookbook.

[ADD][LOCK_REWRITE_BOUNDARY]:
Add one explicit distinction:
- Lock level describes concurrency blocking.
- Rewrite behavior describes physical table/index replacement, disk impact, and MVCC visibility.
- Backfill describes data mutation.
- Validation describes catalog state and invariant admission.
- Rollback describes recovery boundary.

Reason: agents often conflate these concerns and overclaim safety after checking only one of them.

[NO_CHANGE][NO_RUNTIME_GATE]:
Do not add a required PostgreSQL runtime gate to `code-documentation.md` for wording-only standards edits. A future active edit should run `git diff --check -- docs/standards` and link/anchor checks if links or anchors change; SQL runtime proof belongs to migrations or generated catalog work, not this report.

## [6][DRAFT_SNIPPETS]

The snippets below are candidate wording inputs for a later standards pass, not active edits.

Accepted:
`-- Lock: VALIDATE CONSTRAINT uses SHARE UPDATE EXCLUSIVE; writes may continue while existing rows are scanned. Proof: pg_constraint.convalidated for app.order_total_positive.`
Rejected:
`-- Validate constraint.`
Reason: the accepted comment carries lock and proof semantics the SQL syntax omits; the rejected comment narrates the next statement.

Accepted:
`COMMENT ON CONSTRAINT order_total_positive ON app.orders IS 'Orders must carry a nonnegative total after discounts.';`
Rejected:
`COMMENT ON CONSTRAINT order_total_positive ON app.orders IS 'Added NOT VALID in deploy 2026-06-05; rollback by dropping before gate G2.';`
Reason: the accepted catalog comment carries durable invariant meaning; the rejected catalog comment publishes migration status and rollback procedure.

Accepted:
`-- Rewrite: adding a stored generated column rewrites app.orders and indexes; capture pg_relation_filenode and pg_total_relation_size before and after.`
Rejected:
`-- Add generated column.`
Reason: the accepted comment names physical risk and refreshable proof; the rejected comment repeats syntax.

## [7][NO_CHANGE_CONFIRMATIONS]

[NO_CHANGE][CATALOG_SURFACE]:
No change recommended to treating schemas, tables, columns, domains, constraints, indexes, functions, routines, policies, views, publications, subscriptions, and extensions as catalog surfaces. The list matches PostgreSQL `COMMENT` object coverage well enough for standards purposes.

[NO_CHANGE][DESCRIPTION_FUNCTIONS]:
No change recommended to requiring `obj_description(oid, catalog)` with catalog identity. PostgreSQL object descriptions use both object OID and catalog OID, and the active standard correctly rejects ambiguous `obj_description(oid)`.

[NO_CHANGE][SQLFLUFF_LIMIT]:
No change recommended to the rule that `sqlfluff --dialect postgres` is formatting and lint proof only. It cannot prove lock levels, table rewrites, validation state, backfill completion, or catalog extraction.

[NO_CHANGE][SECURITY_BAN]:
No change recommended to the PostgreSQL comment security ban except adding migration-specific examples. PostgreSQL official docs directly support the broad visibility concern.

## [8][VALIDATION]

- [x] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, the full target `docs/standards/reference/code-documentation.md`, and shared governing standards.
- [x] Used current primary PostgreSQL 18.4 official documentation; PostgreSQL 19 was treated only as beta/development.
- [x] Edited only `docs/standards/_reports/code-documentation-050626/track-postgres/02-postgres-migrations.md`.
- [x] Left active standards untouched.
- [x] SQL runtime proof intentionally not run; this is a docs research report, and no PostgreSQL database target was assigned.
