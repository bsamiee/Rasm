# [GENERAL_03_SECURITY_INLINE_RESEARCH]

Research date: 2026-06-05.
Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`.
Assignment: GENERAL 3; inline comments, rationale comments, security/privacy/data exposure in comments, secrets and public metadata exposure across languages.
Mutation boundary: no active standards edited; this report is the only assigned output file.

## [1][EXECUTIVE_FINDINGS]

[F1][KEEP]: `docs/standards/reference/code-documentation.md` already has the right top-level ownership model. It says comments carry only caller-visible semantics omitted by declarations, including security and data exposure, and it routes generated mirrors, catalogs, architecture, tasks, support, and operations out of comments.

[F2][CHANGE]: Add one cross-language comment-safety rule to `code-documentation.md`. PostgreSQL has a strong `COMMENT ON` ban for secrets and sensitive operational data, but inline and generated-comment guidance should explicitly ban literal secrets, credentials, private tenant identifiers, public metadata leaks, exploit details, sensitive log samples, raw personal data, internal hostnames, and nonpublic local paths across C#, TypeScript, Python, Bash, and SQL.

[F3][CHANGE]: Generalize "public metadata exposure" beyond PostgreSQL. C# compiler XML, DocFX, TSDoc, API Extractor, TypeDoc, Python `__doc__` and generated references, Bash help/header comments, SQL catalogs, generated mirrors, and package metadata can make comments discoverable outside source review. A safe standard should treat every generated comment surface as potentially public unless the route proves a stricter access class.

[F4][CHANGE]: Correct drift-prone language baseline claims before promoting any active edit. Repo-local truth currently pins C# 14 and TypeScript `6.0.3`, requires Python `>=3.14`, and local tooling reports Bash `5.3.9`; the active standard currently says TypeScript 7 and Python 3.15. PostgreSQL 18.4 is current official documentation, but local `psql` is absent.

[F5][NO_CHANGE]: Do not add scanner-dependent proof as the safety rule. GitHub push protection and secret scanning are useful secondary controls, but current GitHub docs describe detection limits. The standard should make source-comment review responsible for avoiding sensitive disclosures, with scanners as supplemental gates only when repo tooling proves them.

## [2][REPO_EVIDENCE]

[REQUESTED_FILES_READ]:
- `docs/standards/reference/code-documentation.md`: full file read with line numbers.
- `docs/standards/style-guide.md`: full file read with line numbers.
- `docs/standards/agentic-documentation.md`: full file read with line numbers.
- `docs/standards/README.md`: full file read with line numbers.
- `docs/standards/AGENTS.md`: full file read with line numbers.

[GOVERNING_INSTRUCTIONS_READ]:
- `CLAUDE.md`: confirms Markdown routes through `docs/standards`, current-source verification for research, no comments that describe only "what", and docs-only proof boundaries.
- `AGENTS.md`: confirms docs work routes through `docs/standards/README.md`, nested overlays, and no C# static/test/bridge proof claims for docs-only instruction edits.

[ACTIVE_STANDARD_SPANS]:
- `docs/standards/README.md:16-23`: current repo source and runnable tool output outrank prior notes; drift-prone claims must be proved with repository truth.
- `docs/standards/README.md:83-90`: source files own public symbol documentation and rationale that names and types cannot express; prefer one source for a claim.
- `docs/standards/AGENTS.md:54-58`: forbids publishing session state, secrets, nonpublic machine paths, invented tooling claims, and generic metadata whitelists.
- `docs/standards/agentic-documentation.md:133-135`: generated or mirrored files must exclude secrets, personal data, task notes, and private machine details; public, internal, restricted, and secret material must be separated or filtered.
- `docs/standards/agentic-documentation.md:206-207`: validation already checks no secrets, nonpublic local paths, or unverified provider claims are exposed.
- `docs/standards/style-guide.md:41-45`: remove transient interaction language and preserve only load-bearing uncertainty.
- `docs/standards/style-guide.md:124-130`: code-safe Markdown rules preserve copyability of paths, commands, symbols, and placeholders.
- `docs/standards/reference/code-documentation.md:3-14`: comments own omitted caller-visible semantics, security exposure, lifecycle signals, and inline rationale; generated/reference/task/operation routes leave comments.
- `docs/standards/reference/code-documentation.md:28-37`: document when declarations omit trusted boundary, tenant scope, security behavior, or data exposure; omit private details unless non-obvious rationale is needed.
- `docs/standards/reference/code-documentation.md:71-83`: source truth separates machine shape, semantic contract, generated reference, and routed documentation.
- `docs/standards/reference/code-documentation.md:139-150`: boundary contracts include security/data exposure and inline comments state why a non-obvious implementation, migration, shell, query, or security choice exists.
- `docs/standards/reference/code-documentation.md:299-324`: PostgreSQL capsule distinguishes `COMMENT ON` catalog comments from SQL source comments and bans secrets, credentials, privileged assumptions, exploit details, credential routes, tenant IDs, and sensitive operational data in `COMMENT ON`.
- `docs/standards/reference/code-documentation.md:331-351`: cross-reference routes and inline comments reserve comments for reasons, not next-line narration.
- `docs/standards/reference/code-documentation.md:408-417`: validation already checks inline comments state a reason, no anti-pattern remains, and docs-only work does not run executable rails.

[WORKTREE_CONTEXT]:
- `git status --short -- docs/standards/_reports/code-documentation-050626 docs/standards/reference/code-documentation.md ...` showed `docs/standards/reference/code-documentation.md` already modified before this report write.
- No active standards were edited by this worker.

## [3][CURRENT_SOURCE_CHECKS]

[LOCAL_COMMANDS]:
- `npm view typescript version time.version --json` returned `"6.0.3"`.
- `python3 --version` returned `Python 3.14.4`.
- `bash --version | head -n 1` returned `GNU bash, version 5.3.9(1)-release (aarch64-apple-darwin25.3.0)`.
- `psql --version` failed because `psql` is not installed or not on `PATH`.
- `rg -n "<LangVersion>|TargetFramework|TypeScript|typescript|python_requires|requires-python|Bash|PostgreSQL|postgres" ...` found `Directory.Build.props` with `TargetFramework=net10.0` and `LangVersion=14.0`, `pyproject.toml` with `requires-python = ">=3.14"`, and `package.json` plus `pnpm-workspace.yaml` with `typescript: 6.0.3`.

[SEARCH_COMMANDS]:
- `rg -n "secret|credential|token|PII|personal|privacy|data exposure|inline comment|rationale|COMMENT ON|TypeScript 7|Python 3\\.15|PostgreSQL 18\\.4|Bash 5\\.3|C# 14|metadata|nonpublic|machine paths|public metadata" ...` verified current active coverage and gaps.
- `rg -n "api-key|apikey|password|secret|token|credential|connection string|tenant ID|tenant id|private key|BEGIN .*PRIVATE|AWS_ACCESS|GITHUB_TOKEN|personal access token|nonpublic machine|/Users/" docs/standards docs -g '*.md'` found standards references to sensitive concepts but did not find literal credential-like examples in the requested active files.

[PRIMARY_SOURCES]:
- Microsoft Learn, "Recommended XML tags for C# documentation comments": https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
- API Extractor, "Doc comment syntax": https://api-extractor.com/pages/tsdoc/doc_comment_syntax/
- API Extractor, "`ae-extra-release-tag`": https://api-extractor.com/pages/messages/ae-extra-release-tag/
- Python docs 3.14.5, stable/current documentation landing: https://docs.python.org/3.14/
- PEP 257, "Docstring Conventions": https://peps.python.org/pep-0257/
- Google Python Style Guide, comments and docstrings: https://google.github.io/styleguide/pyguide.html
- GNU Bash Reference Manual 5.3: https://www.gnu.org/software/bash/manual/
- PostgreSQL 18, `COMMENT`: https://www.postgresql.org/docs/18/sql-comment.html
- PostgreSQL 18, `pg_description`: https://www.postgresql.org/docs/18/catalog-pg-description.html
- PostgreSQL current lexical comments: https://www.postgresql.org/docs/current/sql-syntax-lexical.html
- OWASP Logging Cheat Sheet, "Data to exclude": https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html
- OWASP Secrets Management Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html
- GitHub Docs, push protection: https://docs.github.com/en/code-security/concepts/secret-security/push-protection
- GitHub Docs, secret scanning detection scope: https://docs.github.com/en/code-security/reference/secret-security/secret-scanning-scope
- CWE-615, "Inclusion of Sensitive Information in Source Code Comments": https://cwe.mitre.org/data/definitions/615.html

## [4][SOURCE_NOTES]

[SECURITY_COMMENT_EXPOSURE]:
- CWE-615 explicitly treats sensitive information in source comments as an exposure class and names filenames, old links, hidden paths, old code fragments, version numbers, internal hostnames, full pathnames, and real usernames as observed examples.
- CWE-615 mitigation says to remove comments that contain sensitive design or implementation information because exposed comments can affect security posture.
- OWASP Logging says application source code, session identifiers, access tokens, sensitive personal data, passwords, database connection strings, encryption keys, bank or card data, higher-classification data, and legally restricted information should not be recorded directly in logs.
- OWASP Logging also flags file paths, connection strings, internal network names, internal addresses, and personal names, phone numbers, and email addresses as data that may require special handling before recording.
- OWASP Secrets Management says exposed keys need revocation, rotation, and deletion, including secrets discovered in code or logs; secrets in logs need a removal process that preserves log integrity.
- GitHub push protection blocks detected secrets before they reach protected repositories, but GitHub secret scanning detection varies by pattern pairs, token types, and settings, and push protection has size, pattern, legacy-token, and alert-display limits.

[GENERATED_METADATA_EXPOSURE]:
- PostgreSQL `COMMENT` docs state comments can be viewed through `psql` describe commands and built-in functions, and that there is no security mechanism for viewing comments; users connected to a database can see comments for objects in that database, and cluster-shared comments are visible across databases.
- PostgreSQL `pg_description` docs state optional descriptions are stored in `pg_description`, manipulated by `COMMENT`, and viewed by `psql` describe commands.
- PostgreSQL lexical docs state SQL source comments are removed from the input stream before syntax analysis and effectively replaced by whitespace.
- Microsoft C# XML docs state the compiler copies XML documentation comments into output XML and validates some references such as `<param name>` and `<exception cref>`.
- API Extractor says it parses TypeScript comments that immediately precede exported declarations, uses TSDoc, supports release tags, and warns when a doc comment has more than one release tag.
- PEP 257 states a Python docstring becomes the object's `__doc__` attribute; public exported modules, functions, classes, and methods normally have docstrings.
- Google Python style says tricky or non-obvious code deserves comments, but comments must not describe code the reader already understands.
- GNU Bash 5.3 is current stable manual territory for Bash comment syntax and parameter expansion, but Bash has no docstring system; source comments and help text are the discoverable contract only when scripts expose them.

## [5][RECOMMENDATIONS]

[ADD][COMMENT_SAFETY_RULE]:
Add a compact cross-language rule near `code-documentation.md` `[BOUNDARY_CONTRACTS]` or before language capsules:

```text
[COMMENT_SAFETY]:
- State the security boundary, access class, redaction rule, or data-exposure class a caller must respect.
- Do not include literal secrets, credentials, tokens, connection strings, private keys, tenant IDs, raw personal data, internal hostnames, nonpublic local paths, exploit steps, credential routes, or sensitive log samples in source comments, docstrings, catalog comments, generated mirrors, examples, or rationale comments.
- Treat generated comment output as public unless the generated route proves a narrower access class.
- If the comment needs a secret-handling fact, name the secret class and route to the secret owner or runbook; never publish the value, retrieval path, bypass procedure, or privileged assumption.
```

Reason: this preserves the active model that comments may document security/data exposure while preventing the comment from becoming the exposure.

[CHANGE][PUBLIC_METADATA_SCOPE]:
Change the existing "Security or data exposure" field from a narrow list into a generated-surface rule. Suggested direction:

```text
Security or data exposure: auth boundary, tenant scope, row-level security, security definer or invoker behavior, leakproof behavior, access class, redaction rule, personal-data class, generated-reference visibility, public catalog visibility, trace/log output, and secret class.
```

Reason: the current line mentions public catalog visibility and trace/log output, but not generated reference visibility or redaction rule. That matters for C# XML, TSDoc, TypeDoc, Python docstrings, Bash help surfaces, and PostgreSQL comments.

[CHANGE][INLINE_RATIONALE]:
Keep the current "why, not narration" rule, but add a security-specific boundary:

```text
Inline rationale may explain why a boundary choice exists, such as eager validation, redaction, hash-before-log, lock ordering, or search-path hardening. It must not reveal the sensitive value, privileged route, exploit recipe, internal hostname, private path, or raw incident sample that made the choice necessary.
```

Reason: security rationale is often necessary for maintainers, but CWE-615 shows implementation hints and hidden paths can be exploitable when published.

[CHANGE][LANGUAGE_BASELINES]:
Before active edits, resolve whether language capsule headings should reflect current repo truth or future target truth:
- Keep `C# 14` because `Directory.Build.props` pins `LangVersion=14.0`.
- Change `TypeScript 7` to `TypeScript 6.x` or `TypeScript 6.0.3` unless the repo manifest is about to move.
- Change `Python 3.15` to `Python 3.14+` or `Python 3.14` unless the repo manifest is about to move; current official Python docs list 3.14 as stable and 3.15 as pre-release.
- Keep `Bash 5.3+`; local Bash reports 5.3.9.
- Keep or qualify `PostgreSQL 18.4` only as a current official docs target; local `psql` was unavailable, so do not claim local validation.

[ADD][VALIDATION_CHECK]:
Add a validation item under `[COMMENTS_BOUNDARY]`:

```text
- [ ] Security and data-exposure comments name boundary, class, and caller obligation without literal secrets, private identifiers, privileged routes, raw personal data, exploit steps, internal hostnames, nonpublic local paths, or sensitive log samples.
```

Reason: this makes the safety rule scoreable without adding a new proof rail.

[REMOVE][AVOID_BLACKLIST_SPAM]:
Do not add a long language-by-language secret blacklist. Use one cross-language rule, then keep PostgreSQL's stronger `COMMENT ON` warning because PostgreSQL official docs make comments broadly visible through database metadata.

[REMOVE][NO_SCANNER_DEPENDENCY]:
Do not require GitHub secret scanning or push protection as the proof that comments are safe. GitHub's current docs describe detection scope and limits; a scanner can supplement review, but the standard should not claim scanner completeness.

## [6][NO_CHANGE_CONFIRMATIONS]

[NO_CHANGE][OWNERSHIP_MODEL]:
No change recommended to the core rule that comments exist only for semantics the declaration cannot express. This is aligned with PEP 257's anti-signature guidance, Google Python's "never describe the code" advice, and the repo's own "why, boundary exceptions, and non-obvious invariants" policy.

[NO_CHANGE][POSTGRESQL_WARNING]:
No weakening recommended for the PostgreSQL `COMMENT ON` rejection. Current PostgreSQL docs directly support treating catalog comments as broadly visible metadata.

[NO_CHANGE][GENERATED_ROUTE_SEPARATION]:
No change recommended to route generated mirrors, catalogs, architecture, tasks, support, and operations out of source comments. This protects comments from becoming public-surface catalogs and preserves generated-reference truth.

[NO_CHANGE][PROOF_RAIL]:
No static, test, bridge, SQL runtime, generated-reference, or scanner rail should be added for this report or for a future comment-safety wording-only edit unless active files, generated artifacts, or repo tooling claims change. `git diff --check -- docs/standards` remains the likely docs-only close check after active edits.

[NO_CHANGE][PEOPLE_METADATA_BAN]:
No change recommended to the `AGENTS.md` ban on people/process metadata and nonpublic machine paths. Security-comment guidance should not reintroduce owners, teams, reviewers, or path-bearing accountability records as comment metadata.

## [7][DRAFT_EDIT_MAP]

[PRIMARY_OWNER]:
- `docs/standards/reference/code-documentation.md` owns source comments, docstrings, catalog comments, inline rationale comments, generated-reference handoffs, and language capsules.

[SUPPORTING_OWNER]:
- `docs/standards/agentic-documentation.md` already owns generated/mirrored artifact separation and access-class boundaries.

[STYLE_OWNER]:
- `docs/standards/style-guide.md` already owns prose mechanics and should not duplicate security-comment policy.

[DO_NOT_EDIT_FOR_THIS]:
- `docs/standards/README.md` already routes source files and code documentation correctly.
- `docs/standards/AGENTS.md` already bans secrets and nonpublic local paths in standards work.

## [8][CONFIDENCE]

[HIGH]:
- Comments can expose security-sensitive design, implementation, paths, hostnames, usernames, and old links: supported by CWE-615.
- Literal secrets, access tokens, session identifiers, passwords, connection strings, keys, sensitive personal data, and raw sensitive log content should not be published in logs or comments: supported by OWASP Logging, OWASP Secrets Management, and GitHub push-protection docs.
- PostgreSQL `COMMENT ON` is broadly visible metadata and should not contain security-critical information: supported by PostgreSQL 18 docs.
- Inline comments should explain non-obvious rationale and avoid narrating code: supported by current repo policy and Google Python style.

[MEDIUM]:
- TypeScript and Python capsule names should change immediately. Local manifests make the current active wording drift-prone, but a future-target policy may be intentional; active editors should resolve target-vs-current before patching.
- Generated TypeDoc, DocFX, Griffe, and mkdocstrings exposure should be treated as public by default. This is a safe documentation posture, but exact exposure depends on the repo's generated-reference publication route.

[LOW]:
- Any claim about local PostgreSQL runtime behavior in this checkout. `psql` was not available, so only official PostgreSQL documentation was verified.

## [9][TRANSCRIPT]

1. Read memory routing for recent `docs/standards` work. Memory established that active standards files outrank `_reports/` and that `_reports/` is source material only.
2. Discovered requested files and instruction files with `fd`.
3. Read `CLAUDE.md` and root `AGENTS.md`.
4. Read requested standards in full with `nl -ba`: `README.md`, `AGENTS.md`, `agentic-documentation.md`, `style-guide.md`, and `reference/code-documentation.md`.
5. Re-read truncated `code-documentation.md` language-capsule ranges with line-numbered `sed`.
6. Checked current worktree status for assigned scope and requested active files. Found pre-existing modification to `docs/standards/reference/code-documentation.md`.
7. Queried current primary sources and official docs for C# XML comments, API Extractor/TSDoc, TypeScript package version, Python docstrings, Bash 5.3, PostgreSQL comments/catalog metadata, OWASP logging and secrets guidance, GitHub push protection and secret scanning detection scope, and CWE-615.
8. Ran local version and manifest checks for TypeScript, Python, Bash, .NET C# language version, and PostgreSQL client availability.
9. Ran targeted `rg` scans over requested standards for security, privacy, inline-comment, metadata, and baseline-version terms.
10. Wrote only this assigned `_reports/` report.

## [10][CLOSE_CHECK]

- [x] Assigned report file created at `docs/standards/_reports/code-documentation-050626/track-general/03-general-security-inline.md`.
- [x] Requested active standards were read fully.
- [x] Current primary sources were used for drift-prone claims.
- [x] Sources, checks, findings, add/remove/change recommendations, confidence, and no-change confirmations are included.
- [x] No active standards were edited.
