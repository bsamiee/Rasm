# [POSTGRES_FUNCTION_SECURITY_RESEARCH]

Research date: 2026-06-05.
Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`.
Assignment: `POSTGRES 3`; functions, procedures, and routines documentation for volatility, strictness, leakproof, parallel safety, cost, rows, SQLSTATE, `search_path`, and security definer or invoker behavior.
Mutation boundary: no active standards edited; this report is the only assigned output file.

## [1][EXECUTIVE_FINDINGS]

[F1][KEEP]: `docs/standards/reference/code-documentation.md` already routes PostgreSQL durable schema meaning to `COMMENT ON`, local implementation rationale to SQL source comments, and generated catalog extraction to `pg_description`, `pg_shdescription`, description functions, and `psql` describe output.

[F2][CHANGE]: Split the PostgreSQL capsule's phrase `Functions, procedures, and routines` into attribute-scoped guidance. PostgreSQL function attributes include volatility, strictness, leakproof, parallel safety, cost, rows, security mode, and function-local `SET`; procedures expose security mode and procedure-local `SET` through `CREATE PROCEDURE` and `ALTER PROCEDURE`, but not volatility, strictness, leakproof, parallel safety, cost, or rows through the procedure command surface.

[F3][CHANGE]: Treat volatility, strictness, leakproof, parallel safety, cost, and rows as machine shape first. The `CREATE FUNCTION` command and `pg_proc` catalog already carry the attribute value; `COMMENT ON FUNCTION` should add the caller-visible promise, hazard, or rationale only when the attribute's consequence is not obvious from the attribute itself.

[F4][CHANGE]: Strengthen the `SECURITY DEFINER` and `search_path` wording. Official PostgreSQL docs make `search_path` hardening a security requirement for privileged functions and extension-provided functions, not optional descriptive prose; comments should state the trusted schema expectation or caller-visible privilege boundary without exposing privileged routes, secrets, exploit detail, or tenant identifiers.

[F5][CHANGE]: Document SQLSTATE only when exposed as caller behavior. PostgreSQL assigns stable five-character SQLSTATE codes to server messages, and PL/pgSQL `RAISE` can set condition names or SQLSTATE codes; catalog comments should name only the supported observable SQLSTATE contract, not every possible internal error.

[F6][NO_CHANGE]: Keep the strong warning that `COMMENT ON` content is broadly visible. PostgreSQL explicitly states that comments have no viewing security mechanism for connected users, so routine comments must avoid security-critical information even when the routine itself implements security behavior.

## [2][REPO_EVIDENCE]

[REQUESTED_FILES_READ]:
- `docs/standards/reference/code-documentation.md`: full target standard read.
- `docs/standards/README.md`: source precedence, reader-need routing, shared standards, placement, and lifecycle read.
- `docs/standards/AGENTS.md`: standards-folder read scope, rule owners, artifact contract, edit invariants, forbidden patterns, and close check read.
- `docs/standards/agentic-documentation.md`: salience, artifact separation, generated mirrors, provider claims, and safety validation read.
- `docs/standards/information-structure.md`: tables, records, code blocks, heading form, and checklist form read.
- `docs/standards/style-guide.md`: prose, terminology, code-safe Markdown, links, examples, and final proofing pass read.
- `docs/standards/proof.md`: evidence hierarchy, source research, proof fields, and docs-only gate ladder read.
- `docs/standards/formatting.md`: heading idiom, table styling, markers, and whitespace read.

[GOVERNING_INSTRUCTIONS_READ]:
- `CLAUDE.md`: confirms Markdown routes through `docs/standards`, research uses new sources, and docs-only work should not claim static, test, bridge, SQL runtime, or generated-reference rails.
- Root `AGENTS.md`: confirms docs work routes through `docs/standards/README.md`; root instructions reject active-code proof claims for docs-only instruction edits.
- `~/.codex/memories/MEMORY.md`: confirms `_reports/` reports are source material only and active standards outrank prior notes.

[ACTIVE_STANDARD_SPANS]:
- `docs/standards/reference/code-documentation.md:3-14`: comments carry caller-visible semantics omitted by source, including SQLSTATE, security exposure, planner rationale, and route-away obligations.
- `docs/standards/reference/code-documentation.md:75-79`: PostgreSQL catalog objects are machine shape, catalog comments and description functions are generated-reference surfaces, and routed docs consume only routed facts.
- `docs/standards/reference/code-documentation.md:124-129`: catalog surfaces own durable object meaning, security boundary, planner or migration rationale, catalog extraction route, and SQLSTATE where exposed.
- `docs/standards/reference/code-documentation.md:139-154`: boundary contracts include row locks, catalog changes, security definer behavior, leakproof behavior, public catalog visibility, SQLSTATE exposure, transaction, lock, and SQL error conversion.
- `docs/standards/reference/code-documentation.md:299-324`: PostgreSQL 18.4 capsule already names `COMMENT ON`, SQL source comments, function/procedure/routine fields, catalog extraction routes, and sensitive-comment rejections.
- `docs/standards/reference/code-documentation.md:344-350`: PostgreSQL references should use schema-qualified object names in `COMMENT ON` and generated catalog routes through `psql` describe output or description functions.
- `docs/standards/reference/code-documentation.md:393-417`: validation already requires PostgreSQL catalog surfaces to use `COMMENT ON` for durable schema meaning and keeps executable rails out of docs-only work.

[WORKTREE_CONTEXT]:
- `git status --short -- docs/standards` showed `docs/standards/reference/code-documentation.md` already modified before this report.
- Existing `docs/standards/_reports/code-documentation-050626` reports are untracked sibling research artifacts.
- This worker created only `docs/standards/_reports/code-documentation-050626/track-postgres/03-postgres-functions-security.md`.

## [3][CURRENT_PRIMARY_SOURCES]

Source of truth: PostgreSQL 18 official documentation, selected through Context7 library `/websites/postgresql_18` and verified directly against `postgresql.org` pages.
Last verified: 2026-06-05.
Review trigger: PostgreSQL current major version changes, PostgreSQL 18 point-release docs change the command pages below, or Rasm changes the PostgreSQL code-documentation capsule.

Primary source set:
- PostgreSQL 18 `CREATE FUNCTION`: <https://www.postgresql.org/docs/18/sql-createfunction.html>
- PostgreSQL 18 `CREATE PROCEDURE`: <https://www.postgresql.org/docs/18/sql-createprocedure.html>
- PostgreSQL 18 `ALTER FUNCTION`: <https://www.postgresql.org/docs/18/sql-alterfunction.html>
- PostgreSQL 18 `ALTER PROCEDURE`: <https://www.postgresql.org/docs/18/sql-alterprocedure.html>
- PostgreSQL 18 `ALTER ROUTINE`: <https://www.postgresql.org/docs/18/sql-alterroutine.html>
- PostgreSQL 18 `COMMENT`: <https://www.postgresql.org/docs/18/sql-comment.html>
- PostgreSQL 18 `pg_proc`: <https://www.postgresql.org/docs/18/catalog-pg-proc.html>
- PostgreSQL 18 function volatility categories: <https://www.postgresql.org/docs/18/xfunc-volatility.html>
- PostgreSQL 18 parallel safety: <https://www.postgresql.org/docs/18/parallel-safety.html>
- PostgreSQL 18 function security: <https://www.postgresql.org/docs/18/perm-functions.html>
- PostgreSQL 18 extension security considerations: <https://www.postgresql.org/docs/18/extend-extensions.html>
- PostgreSQL 18 PL/pgSQL errors and messages: <https://www.postgresql.org/docs/18/plpgsql-errors-and-messages.html>
- PostgreSQL 18 error codes appendix: <https://www.postgresql.org/docs/18/errcodes-appendix.html>
- PostgreSQL 18 system information and catalog reconstruction functions: <https://www.postgresql.org/docs/18/functions-info.html>

## [4][SOURCE_NOTES]

[COMMENT_ON]:
- `COMMENT ON` accepts `FUNCTION`, `PROCEDURE`, and `ROUTINE` targets and stores one string per object.
- `COMMENT ON` ignores routine argument names and `OUT` arguments for identity; input argument data types identify the function, procedure, or aggregate.
- Comments can be viewed through `psql` describe commands and description functions.
- PostgreSQL states that there is no security mechanism for viewing comments; any connected user can see object comments in the database, and shared-object comments are cluster-visible.

[FUNCTION_ATTRIBUTES]:
- `CREATE FUNCTION` accepts `IMMUTABLE`, `STABLE`, or `VOLATILE`; `VOLATILE` is the default.
- `CREATE FUNCTION` accepts `[ NOT ] LEAKPROOF`; only superusers can set `LEAKPROOF`.
- `CREATE FUNCTION` accepts `CALLED ON NULL INPUT`, `RETURNS NULL ON NULL INPUT`, or `STRICT`; `CALLED ON NULL INPUT` is the default.
- `CREATE FUNCTION` accepts `SECURITY INVOKER` or `SECURITY DEFINER`; invoker is the default.
- `CREATE FUNCTION` accepts `PARALLEL UNSAFE`, `PARALLEL RESTRICTED`, or `PARALLEL SAFE`; unsafe is the default.
- `CREATE FUNCTION` accepts `COST` and `ROWS`; `ROWS` is only allowed for set-returning functions.
- `CREATE FUNCTION` accepts a function-local `SET` clause that applies at function entry and restores the prior setting at exit, subject to the documented `SET LOCAL` and ordinary `SET` behavior.

[PROCEDURE_ATTRIBUTES]:
- `CREATE PROCEDURE` accepts language, transform, `SECURITY INVOKER` or `SECURITY DEFINER`, procedure-local `SET`, and body clauses.
- `CREATE PROCEDURE` does not expose volatility, strictness, leakproof, parallel safety, cost, or rows in its synopsis.
- `ALTER PROCEDURE` exposes `SECURITY INVOKER` or `SECURITY DEFINER` and procedure-local `SET`, `RESET`, and `RESET ALL`.
- `ALTER PROCEDURE` states that the SQL standard allows more properties and specifically notes volatility as a standard property not changed by PostgreSQL `ALTER PROCEDURE`.
- `ALTER ROUTINE` is a generic routine command, but procedure-specific documentation is narrower; future standards wording should not let the generic routine term imply that every listed function attribute applies to procedures.

[PG_PROC_MACHINE_SHAPE]:
- `pg_proc` stores functions, procedures, aggregate functions, and window functions, collectively also known as routines.
- `pg_proc.prokind` distinguishes normal functions, procedures, aggregates, and window functions.
- `pg_proc.procost` and `pg_proc.prorows` carry planner estimates; `prorows` is zero when the function is not set-returning.
- `pg_proc.prosecdef`, `proleakproof`, `proisstrict`, `provolatile`, `proparallel`, and `proconfig` carry security, leakproof, strictness, volatility, parallel safety, and local setting facts.
- `pg_get_functiondef`, `pg_get_function_arguments`, `pg_get_function_identity_arguments`, and `pg_get_function_result` reconstruct routine definitions and arguments; `pg_get_function_result` returns `NULL` for a procedure.

[VOLATILITY]:
- Volatility is a promise to the optimizer.
- `VOLATILE` may change on repeated calls with the same arguments, can do anything including database modification, and is re-evaluated wherever needed.
- `STABLE` cannot modify the database and is stable for the same arguments within one statement.
- `IMMUTABLE` cannot modify the database and is stable forever for the same arguments.
- The docs warn that mislabeling non-immutable behavior as immutable can produce stale reused values, especially with prepared statements or cached plans.
- PostgreSQL requires `STABLE` and `IMMUTABLE` functions to contain no SQL commands other than `SELECT`, but the docs warn this is not a complete protection when they call volatile functions.

[STRICTNESS]:
- `CALLED ON NULL INPUT` means the function is called when arguments are null, and the author must handle nulls.
- `RETURNS NULL ON NULL INPUT` and `STRICT` mean PostgreSQL does not execute the function when any argument is null and assumes a null result.
- For a `STRICT` function with a `VARIADIC` argument, the strictness check tests the variadic array as a whole; null elements inside the array still allow the function to execute.

[LEAKPROOF]:
- `LEAKPROOF` means no side effects and no information about arguments except through the return value.
- A function that throws argument-dependent errors or includes argument values in error messages is not leakproof.
- Leakproof functions may be executed ahead of security policy and security barrier view conditions; non-leakproof functions are delayed to prevent data exposure.
- Row-level security docs repeat that leakproof functions are the exception to policy-expression-before-user-condition ordering.

[PARALLEL_SAFETY]:
- PostgreSQL cannot automatically infer user-defined function parallel safety.
- User-defined functions default to parallel unsafe unless marked otherwise.
- `PARALLEL UNSAFE` disables parallel query for any query containing the function.
- `PARALLEL RESTRICTED` allows parallel mode but only in the parallel group leader.
- `PARALLEL SAFE` allows execution in parallel workers.
- Functions must be unsafe if they write database state, change transaction state, access sequences, or persistently change settings.
- Functions must be restricted if they access temporary tables, client connection state, cursors, prepared statements, or backend-local state that workers cannot synchronize.
- Mislabeling can produce errors, wrong answers, or undefined behavior for C-language functions.

[COST_ROWS]:
- `COST` is a positive estimated execution cost in `cpu_operator_cost` units; for set-returning functions it is cost per returned row.
- Default cost is 1 for C-language and internal functions and 100 for all other languages.
- Larger cost values make the planner avoid unnecessary evaluation where possible.
- `ROWS` is a positive estimated row count for set-returning functions only; the default is 1000.
- `pg_proc.procost` and `pg_proc.prorows` expose these estimates to catalog extraction.

[SQLSTATE]:
- PostgreSQL server messages have five-character SQLSTATE codes.
- Applications should test error codes rather than localized human-readable message text.
- PL/pgSQL `RAISE` can specify a condition name or SQLSTATE code and can set `ERRCODE` through `USING`.
- `RAISE EXCEPTION` defaults to `raise_exception` (`P0001`) if no condition or SQLSTATE is supplied.
- Custom SQLSTATE codes can be any five digits or uppercase ASCII letters except `00000`, but the docs recommend avoiding codes ending in three zeroes because those are category codes.

[SEARCH_PATH_SECURITY]:
- Function security docs warn that functions, triggers, and RLS policies let users insert backend code that others may execute unintentionally; they recommend controlling object-definition privileges and removing untrusted-writable schemas from `search_path`.
- `SECURITY DEFINER` functions run with owner privileges, so PostgreSQL advises setting `search_path` to trusted schemas and `pg_temp` last.
- Without a safe `SET search_path`, a malicious temporary object can mask an intended object.
- PostgreSQL notes default execute privilege on new functions is granted to `PUBLIC`; security-definer functions often need revocation and selective grants in one transaction to avoid an exposure window.
- Extension docs advise applying security-definer search-path techniques to extension functions generally because high-privilege users may call them.
- Extension docs warn that SQL-language and PL-language extension functions are exposed to search-path-based attacks at execution time and that unqualified names and operators can resolve to hostile objects.

## [5][FUNCTION_COMMENT_RULES]

Routine catalog comments should subtract machine shape before adding prose. If `pg_proc`, `CREATE FUNCTION`, or generated catalog reconstruction already exposes the attribute value, `COMMENT ON FUNCTION` should document only the caller-visible consequence, security boundary, planner rationale, or failure contract that the attribute alone cannot express.

[DOCUMENT_WHEN]:
- Volatility: the function has a non-obvious snapshot, configuration, plan-cache, table-lookup, time, sequence, or side-effect relationship that callers or reviewers could misclassify.
- Strictness: null input suppresses execution in a way that affects side effects, audit writes, validation, domain absence, or variadic-array behavior.
- Leakproof: the function is intentionally trusted for RLS or `security_barrier` ordering, and the comment can state the no-leak promise without exposing sensitive predicate detail.
- Parallel safety: restricted or safe labeling depends on temporary tables, backend-local state, locks, sequences, transaction state, connection state, or worker-visible behavior that callers must preserve.
- Cost or rows: planner estimates are intentionally non-default because they protect a plan shape, set-returning cardinality expectation, or evaluation frequency.
- SQLSTATE: callers trap a documented condition name or code, or the function converts internal errors into a stable SQLSTATE contract.
- `search_path`: a privileged function, extension function, dynamic SQL path, operator lookup, or unqualified-name choice depends on a trusted schema list.
- Security mode: the function or procedure intentionally crosses an invoker/owner privilege boundary, changes effective user behavior, or needs grant/revoke sequencing.

[OMIT_WHEN]:
- The comment only restates `IMMUTABLE`, `STABLE`, `VOLATILE`, `STRICT`, `LEAKPROOF`, `PARALLEL SAFE`, `SECURITY DEFINER`, `COST`, `ROWS`, or the SQL return type.
- The SQL object declaration and catalog extraction already carry the complete caller-visible fact.
- The proposed comment copies migration rationale, proof queries, generated dictionary text, or active task notes into a durable catalog comment.
- The proposed comment describes procedure attributes that PostgreSQL procedures do not expose through `CREATE PROCEDURE` or `ALTER PROCEDURE`.

## [6][ATTRIBUTE_MATRIX]

Use this matrix as a later edit input, not as text to paste wholesale into the active standard.

| [INDEX] | [ATTRIBUTE]     | [MACHINE_SOURCE]                                            | [COMMENT_DELTA]                                                       |
| :-----: | :-------------- | :---------------------------------------------------------- | :-------------------------------------------------------------------- |
|   [1]   | volatility      | `CREATE FUNCTION`, `pg_proc.provolatile`                    | snapshot, plan-cache, side-effect, or configuration hazard            |
|   [2]   | strictness      | `CREATE FUNCTION`, `pg_proc.proisstrict`                    | null-suppressed execution consequence or variadic-array caveat        |
|   [3]   | leakproof       | `CREATE FUNCTION`, `pg_proc.proleakproof`                   | RLS or barrier ordering promise without sensitive details             |
|   [4]   | parallel safety | `CREATE FUNCTION`, `pg_proc.proparallel`                    | worker, leader, lock, sequence, state, or wrong-answer hazard         |
|   [5]   | cost            | `CREATE FUNCTION`, `pg_proc.procost`                        | non-default planner rationale or evaluation-frequency intent          |
|   [6]   | rows            | `CREATE FUNCTION`, `pg_proc.prorows`                        | set-returning cardinality expectation when caller-visible             |
|   [7]   | SQLSTATE        | PL/pgSQL `RAISE`, Appendix A                                | stable caller-trapped condition or conversion boundary                |
|   [8]   | `search_path`   | function/procedure `SET`, `pg_proc.proconfig`               | trusted schema expectation or unqualified-name security boundary      |
|   [9]   | security mode   | `SECURITY INVOKER`, `SECURITY DEFINER`, `pg_proc.prosecdef` | effective privilege boundary, grant window, or owner-risk consequence |

The table cells are intentionally compact. A future active-standard edit should express the rule as grouped bullets or a smaller matrix only if it remains scannable beside the existing PostgreSQL capsule.

## [7][CHANGE_RECOMMENDATIONS]

[CHANGE][ROUTINE_SCOPE]:
Replace the broad PostgreSQL capsule line that treats functions, procedures, and routines as one field set with attribute-scoped wording.

Suggested direction:

```text
Functions: contract, volatility, strictness, null behavior, set-returning cardinality, parallel safety, cost/rows when caller-visible, security mode, `search_path` expectation, leakproof promise, and SQLSTATE exposure.
Procedures: contract, security mode, transaction-control limitation where caller-visible, procedure-local `SET` behavior, `search_path` expectation, privilege boundary, and SQLSTATE exposure from the procedure body.
Routines: use only as the catalog identity umbrella when a generated route or `COMMENT ON ROUTINE` target really spans functions and procedures.
```

Reason: PostgreSQL `CREATE PROCEDURE` and `ALTER PROCEDURE` do not expose several function-only attributes; `ALTER ROUTINE` is generic and should not flatten the procedure-specific command surface.

[CHANGE][MACHINE_SHAPE_FIRST]:
Add a PostgreSQL-specific subtraction rule near `CATALOG_COMMENTS`:

```text
For routine attributes, let `CREATE FUNCTION`, `CREATE PROCEDURE`, `ALTER FUNCTION`, `ALTER PROCEDURE`, `pg_proc`, and catalog reconstruction functions carry machine shape. `COMMENT ON` adds only the contract, security boundary, planner rationale, SQLSTATE meaning, or caller obligation that the attribute value cannot express.
```

Reason: this applies the active standard's universal machine-shape rule to PostgreSQL's rich catalog surface and prevents attribute echo.

[CHANGE][LEAKPROOF_SECURITY]:
Refine the leakproof phrase from a plain field into a promise rule:

```text
Leakproof comments state the trusted no-leak promise only when the function is intentionally usable before RLS or security-barrier conditions. Do not mark or document a function as leakproof if argument-dependent errors, messages, timing, notices, or side effects can reveal input information.
```

Reason: official docs define leakproof as a security promise, not just an optimizer hint.

[CHANGE][SEARCH_PATH_SECURITY]:
Add a compact `search_path` hardening rule:

```text
For `SECURITY DEFINER`, extension-provided, dynamic SQL, or unqualified-name functions, state the trusted schema expectation or the reason a function-local `SET search_path` exists. Keep `pg_temp` last when documenting the safe pattern, and never publish exploit steps, credential routes, tenant IDs, or privileged object names that do not need to be public.
```

Reason: PostgreSQL docs make secure `search_path` part of safe privileged function design.

[CHANGE][SQLSTATE_FIELD]:
Add a SQLSTATE field under PostgreSQL comment-owned semantics:

```text
SQLSTATE exposure: document condition names or five-character codes only when callers trap them or the routine intentionally converts internal errors into a stable SQLSTATE contract. Prefer condition names where PL/pgSQL supports them; avoid documenting category codes ending in `000` as if they were specific failures.
```

Reason: SQLSTATE is more stable than message text, and PL/pgSQL gives authors explicit error-code controls.

[CHANGE][COST_ROWS]:
Keep `cost/rows when caller-visible`, but add the omission test:

```text
Document `COST` or `ROWS` only when a non-default estimate encodes a planner contract a maintainer must preserve. Omit comments that merely restate the numeric value stored in `pg_proc`.
```

Reason: cost and rows are machine-readable estimates; prose should explain only the non-obvious preservation reason.

[CHANGE][VALIDATION_ITEM]:
Add a PostgreSQL validation check only if active standards are later edited:

```text
- [ ] PostgreSQL routine comments distinguish function-only planner/nullability attributes from procedure-local security and `SET` attributes.
```

Reason: this makes the main source of overgeneralization scoreable without adding a SQL runtime gate.

## [8][NO_CHANGE_CONFIRMATIONS]

[NO_CHANGE][COMMENT_ON_OWNER]:
Keep `COMMENT ON` as the durable catalog-documentation owner. PostgreSQL source comments are local rationale only because SQL comments are syntax whitespace before analysis, and catalog comments are retrievable through catalog routes.

[NO_CHANGE][VISIBILITY_WARNING]:
Keep the rejection of secrets, credentials, privileged assumptions, exploit details, credential routes, tenant IDs, and sensitive operational data in `COMMENT ON`. PostgreSQL's own docs make object comments broadly visible metadata.

[NO_CHANGE][CATALOG_EXTRACTION]:
Keep `pg_description`, `pg_shdescription`, `col_description`, `obj_description(oid, catalog)`, `shobj_description`, and `psql` describe output as generated-reference routes. Add `pg_proc` and `pg_get_functiondef` family references only if a later edit needs to name routine attribute extraction more precisely.

[NO_CHANGE][SQLFLUFF_BOUNDARY]:
Keep the current statement that `sqlfluff --dialect postgres` is formatting and linting proof only. It does not prove volatility, leakproof, parallel safety, cost, rows, SQLSTATE, or privilege behavior.

[NO_CHANGE][DOCS_ONLY_RAIL]:
Do not run PostgreSQL, SQL, static, test, bridge, generated-reference, or application rails for this report. The report changes one Markdown research artifact and does not alter executable source, migration files, active standards, generated catalogs, or configured docs output.

## [9][DRAFT_EDIT_MAP]

[PRIMARY_OWNER]:
- `docs/standards/reference/code-documentation.md`: PostgreSQL source-comment and catalog-comment semantics, language capsule, anti-patterns, and validation.

[SUPPORTING_OWNER]:
- `docs/standards/proof.md`: evidence field labels, source hierarchy, and docs-only gate ladder; no PostgreSQL-specific proof schema needed.
- `docs/standards/information-structure.md`: any future matrix or grouped record form used to keep the PostgreSQL capsule compact.
- `docs/standards/agentic-documentation.md`: generated catalog and mirror boundaries; no PostgreSQL-specific duplication needed.

[DO_NOT_EDIT_FOR_THIS]:
- `docs/standards/README.md`: already routes code documentation and source comments correctly.
- `docs/standards/AGENTS.md`: already bans secrets, nonpublic machine paths, session state, and invented tooling claims.
- `docs/standards/reference/reference.md`: do not move routine attribute truth into a hand-maintained lookup leaf unless there is a separate generated catalog consumer.

## [10][CONFIDENCE]

[HIGH]:
- PostgreSQL function attributes and defaults are current official docs: volatility, strictness, leakproof, security mode, parallel safety, cost, rows, and `SET`.
- Procedure-specific command docs are narrower than function docs: procedures expose security mode and local settings, not function-only planner/nullability attributes.
- `COMMENT ON` content is broadly visible and cannot carry security-critical information.
- SQLSTATE documentation should prefer stable codes or condition names over message text when caller behavior depends on failure handling.
- `SECURITY DEFINER` and extension functions need secure `search_path` handling.

[MEDIUM]:
- A future active edit should mention `pg_proc` directly. It is the precise catalog source for routine attributes, but the current capsule may intentionally stay at the higher catalog-comment level.
- A future active edit should include a compact attribute matrix. The matrix is useful for overgeneralization, but it may be too table-heavy for the capsule unless compressed.

[LOW]:
- Any claim about this checkout's local PostgreSQL runtime or generated catalog output. No local database, `psql` smoke route, generated dictionary, or migration corpus was run for this docs-only research task.

## [11][CLOSE_CHECK]

- [x] Assigned report file created at `docs/standards/_reports/code-documentation-050626/track-postgres/03-postgres-functions-security.md`.
- [x] Requested target standard and governing standards were read.
- [x] Current primary PostgreSQL sources were used, with Context7 plus direct official documentation verification.
- [x] Findings cover volatility, strictness, leakproof, parallel safety, cost, rows, SQLSTATE, `search_path`, and security definer or invoker behavior.
- [x] Recommendations distinguish functions, procedures, and routines.
- [x] No active standards were edited.
