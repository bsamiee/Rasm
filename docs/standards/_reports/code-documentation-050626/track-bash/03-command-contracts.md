# [BASH_03_COMMAND_CONTRACTS_RESEARCH]

Research scope: `docs/standards/reference/code-documentation.md` with emphasis on Bash script command-surface comments: stdout, stderr, exit-status contracts, namerefs, dispatch tables, command functions, environment variables, machine data, and diagnostics. This report is source material for a later standards edit and does not edit active standards.

## [1][BOUNDARY]

- Assigned output: `docs/standards/_reports/code-documentation-050626/track-bash/03-command-contracts.md`.
- Active standards edited: none.
- Worktree note: `docs/standards/reference/code-documentation.md` was already modified before this report write. This report treats the current worktree version as the evaluated baseline and does not infer authorship.
- Current date: 2026-06-05.

## [2][TRANSCRIPT]

1. Read `CLAUDE.md` fully.
2. Read root `AGENTS.md` fully.
3. Read `docs/standards/README.md` fully.
4. Read `docs/standards/AGENTS.md` fully.
5. Read `docs/standards/reference/code-documentation.md` fully.
6. Read governing shared standards fully where they control the report shape:
   - `docs/standards/information-structure.md`
   - `docs/standards/style-guide.md`
   - `docs/standards/proof.md`
   - `docs/standards/formatting.md`
7. Checked the active standards corpus with `fd -t f . docs/standards | sort`.
8. Checked current worktree status for `docs/standards/_reports` and the active governing files.
9. Loaded `coding-bash` because the focus is Bash script-surface contract guidance.
10. Read Bash skill references relevant to command contracts:
    - `/Users/bardiasamiee/.codex/skills/coding-bash/SKILL.md`
    - `/Users/bardiasamiee/.codex/skills/coding-bash/references/bash-scripting-guide.md`
    - `/Users/bardiasamiee/.codex/skills/coding-bash/references/script-patterns.md`
    - `/Users/bardiasamiee/.codex/skills/coding-bash/references/variable-features.md`
    - `/Users/bardiasamiee/.codex/skills/coding-bash/references/bash-logging.md`
11. Used current maintained Bash and ShellCheck primary sources for drift-prone external claims.
12. Created only the assigned `_reports/` report path.

## [3][LOCAL_SOURCES]

- `CLAUDE.md`: docs standards route, Bash skill requirement for `.sh` and `.bash`, no what-comments, greenfield collapse, dispatch preference, and docs-only proof boundaries.
- `AGENTS.md`: docs route through `docs/standards/README.md`; source comments must not become command catalogs or subtree-local implementation fact dumps.
- `docs/standards/AGENTS.md`: active corpus definition, owner routing, artifact contract, omission rules, and close checks.
- `docs/standards/README.md`: reader-need routing, source precedence, shared-standard owners, placement, lifecycle, split-link, and validation.
- `docs/standards/reference/code-documentation.md`: evaluated target; Bash capsule currently owns script headers, command functions, namerefs, dispatch tables, environment contracts, stdout, stderr, exit statuses, traps, cleanup, and ShellCheck directives.
- `docs/standards/proof.md`: evidence hierarchy, maintained-source preference, proof-field ownership, and docs-only gate selection.
- `docs/standards/information-structure.md`: record, table, definition-block, and code-block shape for recommendations.
- `docs/standards/style-guide.md`: front-and-close prose, direct wording, code-safe Markdown, examples, links, and final proofing.
- ``: salience, durable artifact separation, evidence-before-synthesis, and provider-claim proof.
- `docs/standards/formatting.md`: bracketed headings, table rubrics, status markers, and whitespace.
- `coding-bash` skill: local Bash doctrine for strict mode, dispatch-table routing, stdout/stderr separation, nameref return channels, structured logging, env contracts, traps, cleanup, and ShellCheck compliance.

## [4][EXTERNAL_SOURCES]

Use these only as supporting evidence. Repo standards and local Bash skill guidance control conflicts.

| [INDEX] | [SOURCE]                                                                                   | [FRESHNESS]                                                 | [USED_FOR]                                                                                                                                                                              |
| :-----: | :----------------------------------------------------------------------------------------- | :---------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | GNU Bash Reference Manual, Edition 5.3, https://www.gnu.org/software/bash/manual/bash.html | Last updated 2025-05-18; stable official platform reference | Bash function execution context, command substitution, current-shell substitution, `REPLY` substitution, environment inheritance, exit status, nameref, `GLOBSORT`, and Bash variables. |
|   [2]   | ShellCheck Directive wiki, https://github.com/koalaman/shellcheck/wiki/Directive           | Edited 2026-01-30                                           | Directive scopes, `.shellcheckrc`, supported directives, `source`, `source-path`, `shell`, and documented rationale for disables.                                                       |
|   [3]   | ShellCheck SC1090, https://www.shellcheck.net/wiki/SC1090                                  | Current page fetched 2026-06-05                             | Dynamic `source` path handling and `source` or `source-path` directive purpose.                                                                                                         |
|   [4]   | ShellCheck SC1091, https://www.shellcheck.net/wiki/SC1091                                  | Current page fetched 2026-06-05                             | Source-file access failures, `shellcheck -x`, `external-sources`, and dynamic-source routing.                                                                                           |
|   [5]   | ShellCheck SC1144, https://www.shellcheck.net/wiki/SC1144                                  | Current page fetched 2026-06-05                             | `external-sources=true` belongs in `.shellcheckrc`, not an individual file directive.                                                                                                   |

No secondary blog, forum, or Reddit source was used for recommendations.

## [5][SOURCE_NOTES]

[BASH_EXECUTION]:
- Bash functions execute in the calling shell context, and function arguments replace positional parameters during the call. Comments on command functions should name caller-visible mutation, positional-parameter expectations, and return behavior only when those facts are not clear from the handler signature or dispatch route.
- Ordinary `$(command)` command substitution runs in a subshell and replaces the substitution with stdout, with trailing newlines removed. A command contract that relies on exact trailing newline preservation, state mutation, or status propagation should not hide behind command substitution.
- Bash 5.3 `${ command; }` executes in the current shell and captures stdout, with side effects persisting in the current environment. Comments should document this only when persisted mutation, `return`, `exit`, or positional-parameter sharing is caller-visible.
- Bash 5.3 `${| command; }` expands from `REPLY`, leaves stdout on the caller stream, and restores `REPLY` afterward. Comments should document this when stdout separation is the reason the command avoids a subshell.
- `$?` is the status of the most recently executed command, so exit-status contracts remain the natural Bash failure rail for script surfaces.

[OUTPUT_CHANNELS]:
- Local Bash doctrine is stronger than the upstream manual here: stdout is machine data, stderr is diagnostics and logs, and exit status is the failure channel.
- A comment should name stdout shape when a caller, pipeline, test, or machine parser consumes it: JSON Lines, TSV, null-delimited paths, one path per line, silence on success, or a documented payload.
- A comment should name stderr role when it carries human diagnostics, structured logs, redacted context, warning classes, progress, trace IDs, or operator-only output.
- A comment should reject mixed stdout data and logs unless the script surface explicitly exposes a terminal-only mode whose output is not machine data.

[NAMEREFS]:
- Bash `declare -n` or `local -n` creates a name reference whose reads, writes, unsets, and attribute changes operate on the referenced variable.
- Nameref comments are useful for public command functions when the caller allocates the output variable, when multiple variables are populated, when stdout is intentionally unused, or when dynamic scope collisions are a real caller hazard.
- Nameref comments should not restate `local -n`; they should state mutation ownership, result shape, and collision or naming constraints where caller-visible.

[DISPATCH_TABLES]:
- Local Bash doctrine uses `declare -Ar` dispatch tables for command routing and reserves `case` for pattern matching.
- Dispatch table comments should document key grammar, handler signature, table ownership, metadata source, unsupported-route status, and help-generation route.
- Dispatch table comments should not hand-maintain a command catalog when `_USAGE`, `_REQUIRED_ARGS`, or another metadata table already carries the route list.

[ENVIRONMENT]:
- Bash exports shell variables marked for export to child commands and lets assignment statements before a command shape that command's environment. If those assignments precede a shell function, Bash treats them as local to the function and exported to its children.
- Environment contract comments should document required variables, validation regex or accepted vocabulary, redaction, default source, export behavior, and failure status.
- Comments should not restate `readonly`, `export`, `declare -x`, or ordinary variable names when the declaration and validation table already carry the machine shape.

[SHELLCHECK]:
- ShellCheck directives can appear in `.shellcheckrc` or script comments, and script comments are scoped to the following complete command unless they replace or immediately follow the shebang for file-wide effect.
- ShellCheck's own directive docs recommend documenting why a directive is used.
- `source` and `source-path` directives are documentation-grade because they make static analysis follow a maintained source route.
- `external-sources=true` cannot be enabled inside an individual script; it belongs in `.shellcheckrc`. Script comments that imply otherwise would be stale or wrong.

## [6][CURRENT_STRENGTHS]

[SCRIPT_SURFACE]:
- The target already has a distinct `Script surface` profile with stdout, stderr, state, environment, trap, cleanup, and exit-status contracts.
- The Bash capsule already names the high-value surfaces: script headers, command functions, nameref outputs, dispatch tables, environment contracts, traps, cleanup, streaming loops, and ShellCheck directives.
- The current stdout, stderr, and exit-status rule is correct and should stay central.

[OMIT_DISCIPLINE]:
- The target correctly rejects pseudo-docstring blocks, comments for every function, portable-shell hedging in Bash-only scripts, and comments that restate shell syntax.
- The target correctly treats ShellCheck directives as documentation only when each local directive names the diagnostic class and local invariant.

[BASH_5_3]:
- The target already includes Bash 5.3 current-shell substitution, `REPLY` substitution, `BASH_MONOSECONDS`, `EPOCHREALTIME`, and `GLOBSORT`.
- The current wording mostly matches the GNU Bash 5.3 manual and local Bash skill doctrine.

## [7][FINDINGS]

### [7.1][ADD_COMMAND_CONTRACT_RECORD]

Finding: The Bash capsule names the comment-owned fields, but it does not provide a compact record shape for reviewing a public script or command function.

Why it matters: Bash comments drift into either mechanical headers or under-specified usage blurbs. A record shape would force agents to separate machine data, diagnostics, failure rail, state mutation, and route ownership without making every function carry boilerplate.

Recommendation: add a conditional command-surface review record to the Bash capsule or `PRODUCED_SHAPE`.

Candidate shape:

```text template
Command surface: `<script, subcommand, command function, dispatch key, or handler>`
Input contract: `<argv, stdin, env, nameref, or config source the declaration omits>`
Stdout contract: `<machine payload, delimiter, schema, silence, or omit when unused>`
Stderr contract: `<diagnostics, logs, progress, redaction, or omit when silent>`
Exit statuses: `<0 success; 2 usage; custom status meanings; signal mapping when exposed>`
State and resources: `<globals, files, FDs, child PIDs, traps, cleanup, or omit when absent>`
Route owner: `<dispatch table, metadata table, help generator, or direct entrypoint>`
Omit when: `<declaration, metadata, validation table, or test already carries the fact>`
```

Keep it a review record, not a required source-comment header.

Confidence: high.

### [7.2][KEEP_STDOUT_STDERR_EXIT_AS_THE_SCRIPT_RAIL]

Finding: The current Bash rule that stdout is machine data, stderr is diagnostics and logs, and exit status is the failure channel is the right organizing principle.

Why it matters: This one rule prevents the highest-cost script documentation failures: tests asserting diagnostic prose from stdout, JSON consumers receiving progress logs, callers parsing stderr, and functions returning structured data through command substitution when a nameref should own it.

Recommendation: preserve the rule and make validation sharper:
- Bash comments name stdout schema only when a machine consumer exists.
- Bash comments name stderr semantics only when diagnostics, logs, redaction, progress, trace fields, or operator UX affect the caller.
- Bash comments name every public nonzero exit status the caller can intentionally branch on.
- Undocumented nonzero status remains general failure only when no caller has a supported branch.

Confidence: high.

### [7.3][SPLIT_MACHINE_DATA_FROM_DIAGNOSTIC_LOGGING]

Finding: The current target says stdout is machine data and stderr is diagnostics, but it does not yet distinguish structured machine logs from command payload data.

Why it matters: Bash tools often emit JSON logs for machines. The rule should not imply that all machine-readable bytes belong on stdout. Local Bash logging doctrine defaults `_LOG_FD` to stderr specifically to preserve stdout as the pipeline payload channel.

Recommendation: add a short clarification:

Accepted: stdout carries the command payload; stderr carries JSON-ND logs or human diagnostics.
Rejected: stdout carries both payload rows and progress or log events.
Reason: stdout is the composable payload rail; machine-readable logs are still diagnostics unless the command's public payload is the log stream.

Confidence: high.

### [7.4][MAKE_NAMEREF_COMMENTS_MUTATION_CONTRACTS]

Finding: The current Bash capsule correctly names nameref output, but it could be stricter that nameref comments are mutation contracts, not syntax explanations.

Why it matters: Official Bash behavior makes namerefs powerful and hazardous: operations on the nameref operate on the referenced variable. A caller needs to know ownership, output shape, and collision risks, not that `local -n` exists.

Recommendation: change or add wording:
- Document namerefs when the caller supplies storage, when stdout is intentionally unused, when multiple outputs are populated, or when dynamic scope collision changes safe names.
- State target ownership, shape, and collision rule.
- Omit comments that only say a name is a nameref.

Confidence: high.

### [7.5][ROUTE_DISPATCH_TABLES_AWAY_FROM_COMMAND_CATALOGS]

Finding: Dispatch-table comments need a boundary against becoming hand-maintained command catalogs.

Why it matters: The script metadata tables are the machine shape. Comments should explain the dispatch invariant and route, while `_USAGE`, `_REQUIRED_ARGS`, or equivalent metadata owns the command list.

Recommendation: add a rule:
- Dispatch-table comments own key grammar, handler signature, metadata source, and unsupported-route status.
- Help or route metadata owns the list of commands.
- Generated or curated command catalogs route to reference docs, generated help output, or tests, not source comments.

Confidence: high.

### [7.6][TIGHTEN_ENVIRONMENT_CONTRACT_COMMENTS]

Finding: Environment comments are listed, but the actionable fields can be sharper.

Why it matters: Env vars are both configuration and data-exposure surfaces. Comments that say only `requires FOO` do not tell a caller whether `FOO` is validated, exported, redacted, inherited by children, or fatal when absent.

Recommendation: add a compact environment contract field set:
- Required variable and accepted shape.
- Default source or absence behavior.
- Export behavior to child commands.
- Redaction rule for logs and errors.
- Failure status on missing or invalid value.

Do not duplicate a validation table. If `_ENV_CONTRACT` carries the regex and status mapping, a nearby comment should name only the semantic reason and redaction boundary.

Confidence: high.

### [7.7][DOCUMENT_CURRENT_SHELL_SUBSTITUTION_ONLY_AT_BOUNDARIES]

Finding: The target correctly documents Bash 5.3 `${ command; }` and `${| command; }`, but this feature can invite over-commenting.

Why it matters: The GNU manual makes the important distinction clear: `${ command; }` persists side effects in the current execution environment, while `${| command; }` expands from `REPLY` and leaves stdout on the caller stream. Comments should appear only where those properties are caller-visible.

Recommendation: add an omit rule:
- Document `${ command; }` only when current-shell side effects, `return`, `exit`, positional parameters, or trailing-newline behavior affect a public contract.
- Document `${| command; }` only when the separation between `REPLY` and stdout is a public contract.
- Omit performance-only comments at every substitution site; use one local rationale when a hot path chooses the construct.

Confidence: high.

### [7.8][MAKE_SHELLCHECK_DIRECTIVE_RATIONALE_VISIBLE]

Finding: The current target says ShellCheck directives are documentation and rejects bare disables. That is correct, and ShellCheck's maintained directive page supports it.

Why it matters: ShellCheck directives change the analyzer's view of the source. Without a reason, a disable becomes unreviewable and can hide real failures.

Recommendation: add two precise rules:
- A local `disable` directive names the ShellCheck code and the local invariant that makes the diagnostic false or intentionally inapplicable.
- `source`, `source-path`, and `shell` directives name analysis routing or dialect truth, not runtime behavior.
- `external-sources=true` belongs in `.shellcheckrc`; do not suggest it as a script-local directive.

Confidence: high.

### [7.9][KEEP_TRAP_AND_CLEANUP_DETAILS_CONDITIONAL]

Finding: The target has a very detailed trap and cleanup comment list. It is valuable but dense.

Why it matters: Trap and cleanup behavior is caller-visible for scripts, daemon wrappers, and command functions that supervise children or own files. It is not useful as a blanket checklist for every local cleanup registration.

Recommendation: keep the current list but make its trigger explicit:
- Include trap and cleanup fields when the public script surface exposes signal behavior, child forwarding, lock ownership, temporary paths, same-filesystem rename assumptions, sensitive `umask`, rollback, or cleanup idempotence.
- Omit per-line cleanup comments when the cleanup registry and tests already carry the behavior.

Confidence: medium-high.

## [8][ADD_RECOMMENDATIONS]

1. Add the conditional `Command surface` review record in section `PRODUCED_SHAPE` or the Bash capsule.
2. Add a compact stdout/stderr contrast that distinguishes payload data from diagnostics and structured logs.
3. Add an environment contract field set for required variables, validation shape, export behavior, redaction, and failure status.
4. Add ShellCheck directive guidance that separates disable rationale from `source`, `source-path`, `shell`, and `.shellcheckrc` routing.

## [9][CHANGE_RECOMMENDATIONS]

1. Change nameref wording from a general output note into a mutation-ownership rule: caller storage, result shape, collision rule, and stdout silence.
2. Change dispatch-table wording to name key grammar, handler signature, metadata owner, and unsupported-command status while routing command catalogs away.
3. Change Bash 5.3 feature wording to emphasize documentation triggers: current-shell mutation, `REPLY` versus stdout separation, monotonic elapsed time, wall-clock timestamps, and semantic glob ordering.
4. Change trap and cleanup wording only enough to make it conditional on caller-visible resource, signal, child, or durability behavior.

## [10][REMOVE_RECOMMENDATIONS]

1. Remove no current Bash capsule concept.
2. Do not remove the stdout, stderr, and exit-status rule; it is the core script rail.
3. Do not import a full Bash command catalog into `code-documentation.md`.
4. Do not add source-comment templates that require every command function to list every parameter.
5. Do not recommend script-local `external-sources=true`; current ShellCheck documentation rejects that placement.

## [11][NO_CHANGE_CONFIRMATIONS]

[SCRIPT_PROFILE]:
- Keep `Script surface` as a separate profile. Bash command contracts are not pure, rail, throwing, or catalog surfaces.

[BASH_5_3_BASELINE]:
- Keep Bash 5.3+ as the capsule baseline. The GNU Bash 5.3 manual is the right primary source for current-shell substitution, `REPLY` substitution, `GLOBSORT`, `BASH_MONOSECONDS`, and `BASH_TRAPSIG`.

[SOURCE_COMMENT_BOUNDARY]:
- Keep pseudo-docstrings, mechanical parameter blocks, and comments for every function rejected.

[GENERATED_REFERENCE]:
- Keep the statement that Bash has no generated-reference profile by default. Bash references should remain literal command, variable, function, and ShellCheck code spans unless the repository adopts a generator.

[DOCS_ONLY_PROOF]:
- No C#, TypeScript, Python, Bash syntax, ShellCheck, SQL, static, test, bridge, or generated-reference rail should run for this report-only work.

## [12][VALIDATION]

[REPO_CHECKS]:
- Full requested target read: yes, `docs/standards/reference/code-documentation.md`.
- Governing standards read: yes, listed in the transcript.
- Active standards edit avoided: yes.
- Assigned report file written: yes.
- Existing dirty active standard detected: yes, `docs/standards/reference/code-documentation.md`.

[SOURCE_CHECKS]:
- GNU Bash Reference Manual 5.3 used for Bash semantics.
- ShellCheck directive and diagnostic pages used for directive semantics.
- No secondary sources used for recommendations.

[GATE_CHECKS]:
- Static, test, bridge, generated-reference, ShellCheck, and Bash syntax rails not run, because the only change is this report file.

## [13][CLOSE]

The strongest Bash 3 improvement is to make command comments contract-shaped without making them mandatory headers. The active standard already has the right core: stdout is machine payload, stderr is diagnostics and logs, and exit status is the failure channel. The later edit should add a conditional command-surface record, sharpen nameref and dispatch-table ownership, and make environment and ShellCheck directive comments carry only the caller-visible fact that source declarations and metadata do not already express.
