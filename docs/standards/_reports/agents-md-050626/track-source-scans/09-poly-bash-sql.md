# [POLY_04_BASH_SQL]

## [TRANSCRIPT]

Scope: context research report for advanced polymorphism in Bash 5.3 and PostgreSQL 18.4 `AGENTS.md` guidance. No active repository files were edited.

Local standards read:
- `CLAUDE.md`: universal polymorphic posture, required skills for `.sh`, `.bash`, `.sql`, and `.md`, current-source research, branch-collapse rules, and no imperative domain branching.
- `AGENTS.md`: root router, nested overlay discovery, canonical-owner extension before new rails, and no root command catalogs.
- `docs/standards/README.md`: standards routing, proof/source precedence, and placement rules.
- `docs/standards/AGENTS.md`: active standards corpus rules, research-as-input boundary, future-standard posture, and close checks.
- `docs/standards/agents-md.md`: `AGENTS.md` semantic slots, root profile, local extension grammar, route-away, anti-fragility, and trust boundaries.
- `docs/standards/agentic-documentation.md`, `information-structure.md`, `proof.md`, `style-guide.md`, and `formatting.md`: salience, container, evidence, prose, and bracketed-heading rules.
- `tools/assay/AGENTS.md`: tool-folder pattern for one engine over data rows, catalog rows, registry binds, tagged detail variants, and no helper or wrapper rails.
- `tools/rhino-bridge/AGENTS.md`: tool-operator delta pattern, source-only scenario boundary, marker scanning, and no durable links to run-local artifacts.
- `libs/csharp/Rasm.Persistence/AGENTS.md`: storage-owner language for query algebra, migrations, typed receipts, provider-proof route, and no repository wrapper.

Local evidence:
- `CLAUDE.md:6` says the monorepo is polymorphic, agnostic, and universal by default.
- `CLAUDE.md:23-24` routes Bash/sh to `coding-bash` and SQL to `coding-pg`.
- `CLAUDE.md:35` requires new sources for research unless stable official docs are the only primary source.
- `CLAUDE.md:62` rejects imperative branching in domain logic in favor of expression, match, dispatch-table, or monadic ROP patterns.
- `CLAUDE.md:79` requires related variants to collapse into one polymorphic surface before adding entrypoints.
- `AGENTS.md:43` requires dense, strongly typed, value-driven implementations and collapse through operation algebras, folds, receipts, or source-owned tables.
- `AGENTS.md:79` rejects command catalogs in root because `CLAUDE.md`, tool READMEs, and nested overlays own command selection.
- `docs/standards/AGENTS.md:40` says research and documentation-improvement work treats older paradigms as replacement targets, not baselines.
- `docs/standards/agents-md.md:56-67` defines the produced functions of every `AGENTS.md`: scope, read behavior, future-standard posture, owner contract, route-away, rejections, and close or stop behavior.
- `docs/standards/agents-md.md:80-87` requires local extension grammar to name trigger, newest target, owner, extension action, and rejected substitute.
- `tools/assay/AGENTS.md:13-31` gives the strongest tool-folder wording model: one engine over data rows; programs are catalog rows; extension is row, axis member, registry bind, tagged detail variant, aspect layer, or tool row.
- `libs/csharp/Rasm.Persistence/AGENTS.md:15-25` gives the strongest SQL-adjacent owner wording model: store rails, query algebra, migrations, snapshots, typed receipts, and no repository wrapper.

Current external sources checked:
- [GNU Bash Reference Manual 5.3](https://www.gnu.org/software/bash/manual/bash.html), edition 5.3, updated 2025-05-18.
- [Bash 5.3 release announcement](https://lists.gnu.org/archive/html/bash-announce/2025-07/msg00000.html), 2025-07-05.
- [ShellCheck SC2086 documentation](https://www.shellcheck.net/wiki/SC2086).
- [mvdan/sh and shfmt README](https://github.com/mvdan/sh).
- [PostgreSQL 18.4 release notes](https://www.postgresql.org/docs/18/release-18-4.html), release date 2026-05-14.
- [PostgreSQL 18 release notes](https://www.postgresql.org/docs/18/release-18.html), release date 2025-09-25.
- [PostgreSQL 18 system catalogs](https://www.postgresql.org/docs/18/catalogs.html).
- [PostgreSQL 18 `COMMENT`](https://www.postgresql.org/docs/18/sql-comment.html).
- [PostgreSQL 18 SQLSTATE error codes](https://www.postgresql.org/docs/18/errcodes-appendix.html).
- [PostgreSQL 18 PL/pgSQL errors and messages](https://www.postgresql.org/docs/18/plpgsql-errors-and-messages.html).
- [PostgreSQL 18 type system](https://www.postgresql.org/docs/18/extend-type-system.html).
- [PostgreSQL 18 PL/pgSQL dynamic commands](https://www.postgresql.org/docs/18/plpgsql-statements.html).
- [PostgreSQL 18 generated columns](https://www.postgresql.org/docs/18/ddl-generated-columns.html).
- [PostgreSQL 18 SQL/JSON functions](https://www.postgresql.org/docs/18/functions-json.html).

Discovery result: `fd -e sh -e bash -e sql .` found no checked-in `.sh`, `.bash`, or `.sql` files in the current repository root, so this report proposes future wording rather than patching an existing shell or SQL subtree overlay.

## [MODERN_CAPABILITIES]

Advanced polymorphism outside OOP means one behavior surface accepts variation as data, type relation, catalog fact, or dispatch key. It does not mean class inheritance. In Bash, the polymorphic unit is usually an associative dispatch table, an option metadata row, an environment contract table, or a nameref output contract. In PostgreSQL, the polymorphic unit is the catalog, type system, SQLSTATE vocabulary, generated column, range/domain constraint, SQL/JSON path, or dynamic SQL command that is identifier-quoted and value-parameterized.

[BASH_5_3]:
- Bash 5.3 is current enough to name directly for future-facing standards. The GNU manual identifies Bash 5.3 and the release announcement lists significant 5.3 features.
- Command substitution now has current-shell forms: `${ command; }` captures output while side effects persist, and `${| command; }` expands from local `REPLY` while leaving stdout in the caller stream. This supports explicit return-channel design instead of hidden subshell capture.
- Associative arrays and `declare -A` support table-driven routing. They are the shell equivalent of a bounded operation algebra when keys are verbs, resources, states, signal numbers, log formats, or validation names.
- `declare` or `local -n` namerefs support output contracts without `eval`, command substitution, or global mutation. A parser can fill `out_args`, a resolver can fill `out_path`, and a catalog read can fill `out_rows` by name.
- `BASH_MONOSECONDS`, `BASH_TRAPSIG`, `BASH_COMMAND`, `BASH_LINENO`, and `BASH_SOURCE` support structured timing, signal dispatch, and error receipts without external process spam.
- `GLOBSORT`, `compgen` variable output, `read -E`, and `source -p PATH` are feature signals for modern Bash, but repo guidance should mention them only when the folder owns completion, sourcing, or glob-order behavior.

[SHELL_DISCIPLINE]:
- ShellCheck is the static-analysis contract, not a style preference. SC2086 makes the core safety rule concrete: quote expansions to prevent word splitting and glob expansion, and use arrays when optional argument shape matters.
- `shfmt` is the parser and formatter contract. Its README defines the project as a shell parser, formatter, and interpreter with Bash support, and it warns that associative-array indexes must be quoted for static parsing.
- Advanced Bash guidance should pair `bash -n`, ShellCheck, shfmt, and self-tests where executable shell is present. It should not create a command catalog inside `AGENTS.md`; the local tool README or root quality route owns command syntax.

[POSTGRESQL_18_4]:
- PostgreSQL 18.4 is the current supported 18.x point release verified by official release notes dated 2026-05-14. PostgreSQL 18 introduced `uuidv7()`, virtual generated columns by default, `OLD` and `NEW` in `RETURNING`, temporal constraints, AIO, and skip scans.
- System catalogs are regular tables that store schema metadata. The design route is to query catalogs and use SQL commands, not to hand-maintain dictionary prose or mutate catalogs by hand.
- `COMMENT ON` stores one comment per database object, replaces/removes it through the same command, and exposes comments through `psql` describe commands plus `obj_description`, `col_description`, and `shobj_description`. Because comments are broadly visible, they must never carry secrets or security-critical details.
- SQLSTATE is the typed error rail. PostgreSQL 18.4 lists defined error codes and condition names; PL/pgSQL `RAISE` can report by condition name, direct five-character SQLSTATE, and structured fields such as `DETAIL`, `HINT`, `COLUMN`, `CONSTRAINT`, `DATATYPE`, `TABLE`, and `SCHEMA`.
- PostgreSQL's polymorphic pseudo-types, including `anyelement`, `anyarray`, `anyrange`, `anymultirange`, and the `anycompatible*` family, make type-driven generic SQL functions explicit. This is real polymorphism in SQL, but it is not the only pattern.
- PL/pgSQL dynamic SQL must separate identifiers from values. Use `$1`, `$2`, and `USING` for values; use `format('%I', identifier)` or `quote_ident` for dynamic table or column names. String concatenation of untrusted identifiers is rejected.
- Generated columns and SQL/JSON path functions let schema and query shape own derived facts. Generated dictionaries should come from catalogs, comments, constraints, generated columns, and controlled extraction queries, not from hand-copied Markdown maps.

## [REPO_TRANSLATION]

For this repo, advanced polymorphism means every Bash, SQL, or tool folder should identify its one owning rail and describe how new variants extend it. The rule should name the table, catalog, query algebra, typed receipt, or data row that grows. It should reject sibling functions, wrapper scripts, copy-pasted migrations, and prose dictionaries that drift away from source truth.

[BASH_TRANSLATION]:
- Script command routing: one `declare -Ar` dispatch table maps `verb:resource` or `command` keys to implementation functions.
- Option routing: one metadata table owns option names, arity, defaults, validation regex, help text, and destination variable.
- Return routing: nameref outputs own structured values; stdout stays for data streams; stderr stays for diagnostics; process exit owns status.
- Error routing: one typed exit-code table and one ERR trap receipt shape own fatal paths.
- Signal routing: one signal dispatch table keyed by `BASH_TRAPSIG` owns cleanup and child forwarding.
- Data routing: `mapfile`, arrays, associative arrays, and JSON via `jq` own data rows; ad hoc `grep | sed | cut` chains are not polymorphism.

[SQL_TRANSLATION]:
- Schema routing: domains, composite types, enums, range or multirange types, constraints, generated columns, comments, and catalogs own structured meaning.
- Query routing: set-based CTEs, `MERGE`, `RETURNING OLD/NEW`, `FILTER`, `LATERAL`, SQL/JSON paths, and polymorphic pseudo-types replace row-at-a-time procedural branches.
- Error routing: SQLSTATE condition names and structured `RAISE ... USING` fields replace text-message parsing.
- Proof routing: `COMMENT ON`, generated catalog extraction, migrations, and source-controlled SQL own durable schema dictionaries; Markdown summarizes the route rather than duplicating the dictionary.
- Dynamic routing: `EXECUTE ... USING` parameterizes values, and `%I` or `quote_ident` quotes identifiers. There is no acceptable stringly SQL rail for untrusted names.

[TOOL_FOLDER_TRANSLATION]:
- Tool folders should follow the `tools/assay/AGENTS.md` model: one engine, one row shape, one registry, one envelope, one detail union, and one artifact route.
- A shell or SQL tool should describe extension as row addition, catalog-query addition, SQLSTATE case addition, or dispatch-table entry addition.
- The overlay should route command syntax, public operator workflow, and generated dictionaries to the README or generated reference. `AGENTS.md` should carry only behavior-changing local rules.

## [AGENTS_MD_WORDING]

Universal wording for `AGENTS.md` standard guidance:

```markdown conceptual
When a local overlay governs Bash, SQL, migrations, shell entrypoints, or tool operators, state the polymorphic owner before any rejection. Name the table, dispatch map, catalog query, typed receipt, generated dictionary, or operation algebra that grows, then name the wrapper, branch chain, or prose copy that is rejected.
```

Shell-folder wording:

```markdown conceptual
## [2][OWNER_CONTRACT]

This folder owns Bash entrypoint behavior through one dispatch table, one option metadata table, one stderr diagnostic rail, one stdout data stream contract, and nameref output contracts for structured values. Add behavior by extending the owning table or parser row; do not add sibling scripts, helper files, branch ladders, or `eval` indirection.

## [3][EXTENSION_GRAMMAR]

- Command: add one `verb:resource` dispatch entry and one implementation function.
- Option: add one option metadata row and route parsing through the existing parser.
- Output value: add one nameref field in the owning output contract.
- Stream: keep data on stdout and diagnostics on stderr.
- Error: add one exit-code or ShellCheck-justified exception row.
- Signal: add one signal dispatch entry keyed by the trap signal.

## [4][REJECTIONS]

- No `if` or `case` ladders for command routing when a dispatch table can own the variant.
- No `eval` for indirect calls or output assignment.
- No unquoted expansion except a documented ShellCheck-clean array or glob boundary.
- No `grep | sed | cut` parsing chain where `jq`, `mapfile`, arrays, or one `awk` program owns the data shape.
- No helper shell libraries for one caller.
```

SQL-folder wording:

```markdown conceptual
## [2][OWNER_CONTRACT]

This folder owns SQL behavior through schema types, constraints, generated columns, catalog extraction, SQLSTATE error rails, and set-based query algebra. Extend domains, composite types, comments, generated dictionaries, migration rows, and typed receipts before adding procedural branches or duplicate lookup prose.

## [3][EXTENSION_GRAMMAR]

- Semantic scalar: add or extend a domain type.
- Structured return: add or extend a composite type.
- Derived fact: use a generated column, view, or catalog extraction query.
- Error: use a SQLSTATE condition name or five-character code with structured `RAISE ... USING` fields.
- Dictionary: generate from `pg_catalog`, `COMMENT ON`, constraints, and source-controlled migrations.
- Dynamic SQL: parameterize values with `USING`; quote identifiers with `%I` or `quote_ident`.

## [4][REJECTIONS]

- No text-message parsing for database errors; use SQLSTATE and structured fields.
- No hand-maintained Markdown dictionary when catalog extraction can generate it.
- No string-concatenated identifiers without identifier quoting.
- No row-at-a-time PL/pgSQL loops when a set query, CTE, `MERGE`, or `RETURNING` expression owns the operation.
- No schema meaning hidden in untyped `text` when a domain, enum, composite type, range, or JSON path contract can express it.
```

Tool-folder wording:

```markdown conceptual
## [2][ARCHITECTURE_CONTRACT]

This tool is one engine over data rows. Programs, commands, SQL checks, shell checks, output envelopes, and artifacts extend through catalog rows and typed detail variants. Operator prose, command syntax, and generated dictionaries route to the README or generated reference.

## [3][EXTENSION_GRAMMAR]

- Program: add one catalog row.
- Bash check: add one ShellCheck or shfmt-backed row and one receipt variant.
- SQL check: add one catalog query or migration-proof row and one SQLSTATE-aware receipt variant.
- Output: extend the existing envelope or tagged detail union.
- Artifact: add one artifact route under the existing artifact contract.

## [4][REJECTIONS]

- No parallel command registry, output envelope, status enum, report struct, helper file, or wrapper object for one concept.
- No durable documentation link to run-local artifacts.
- No command catalog in `AGENTS.md`; route syntax to the tool README.
```

Root or parent overlay wording:

```markdown conceptual
When root-started work touches shell, SQL, migration, or tool folders, discover the nearest nested `AGENTS.md` before editing. If no nested overlay exists, apply `coding-bash` for `.sh` and `.bash`, `coding-pg` for `.sql`, and route generated dictionaries or command syntax to the owning README or generated reference before adding instruction prose.
```

## [ANTI_PATTERNS]

- Treating polymorphism as class inheritance only. Bash and SQL usually express polymorphism through data rows, maps, catalogs, type variables, and generated source truth.
- Replacing a dispatch table with `if` or `case` ladders for command routing. `case` remains valid for pattern matching and option parsing, not command-family ownership.
- Using `eval` for indirect function calls or output assignment in Bash. Use dispatch maps and namerefs.
- Capturing structured Bash output through command substitution when a nameref contract or `${| ...; }` `REPLY` channel is the clearer owner.
- Parsing JSON, CSV, or SQL output with string fragments when a structured tool, catalog query, or data row owns the shape.
- Treating ShellCheck warnings as cosmetic. Suppression is a local proof obligation, not a formatting preference.
- Treating shfmt as semantic proof. It proves parse and formatting shape; it does not prove quoting, traps, exit rails, output contracts, or runtime behavior.
- Maintaining schema dictionaries by hand. Generate them from `pg_catalog`, `COMMENT ON`, constraints, migration sources, and checked extraction queries.
- Storing secrets or security-sensitive values in SQL comments. PostgreSQL comments are broadly visible to connected users.
- Parsing PostgreSQL error text. Use SQLSTATE condition names, five-character codes, and structured error fields.
- Building dynamic SQL by concatenating untrusted identifiers or literal values. Use `USING` for values and `%I` or `quote_ident` for identifiers.
- Adding repository wrappers, helper scripts, utility files, one-off registries, or parallel status enums when the existing row, table, catalog, receipt, or envelope can grow.

## [CONFIDENCE]

Confidence: high for the Bash 5.3 and PostgreSQL 18.4 capability claims because they come from current official manuals or release notes. Confidence: high for `AGENTS.md` wording because it follows the active repo standard slots and mirrors existing tool-folder overlays. Confidence: medium for direct repo adoption priority because no checked-in `.sh`, `.bash`, or `.sql` files exist in the current tree; the guidance is future-facing and should be applied when such folders or generated SQL surfaces appear.

Proof gap: this report did not run Bash, SQL, static, test, bridge, or generated-dictionary validations because the task is research-only and active code was not changed. The only appropriate close check is Markdown diff hygiene for the added `_reports/` report.
