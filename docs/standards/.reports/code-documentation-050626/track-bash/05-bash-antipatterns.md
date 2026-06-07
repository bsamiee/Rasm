# [BASH_05_ANTIPATTERNS_RESEARCH]

Research date: 2026-06-05.
Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`.
Assignment: BASH 5; Bash comment anti-patterns and advanced style: pseudo-docstrings, overcommenting functions, portable-shell hedging, collection-loop versus stream comments, secrets and redaction.
Mutation boundary: no active standards edited; this report is the only assigned output file.

## [1][EXECUTIVE_FINDINGS]

[F1][KEEP]: `docs/standards/reference/code-documentation.md` already has the correct Bash posture. Bash has comments and ShellCheck directives, not docstrings or a generated-reference profile, so the active standard correctly keeps Bash documentation in script headers, command-function contracts, traps, streaming loops, dispatch tables, environment contracts, nameref returns, durable writes, and ShellCheck rationale.

[F2][CHANGE]: Strengthen the anti-docstring rule into a positive contract-comment budget. A later edit should say Bash comments document public script contracts, command surfaces, analyzer directives, and non-obvious shell boundaries; they do not create pseudo-docstring blocks, generated catalogs, library-style per-function records, or mechanical parameter headers.

[F3][CHANGE]: Keep rejecting "comments for every function," but split the rule by surface. Public command functions, dispatch handlers, trap handlers, nameref-return functions, and stream consumers may need contract comments; short internal functions whose name, arguments, and local declarations carry the fact should not get blanket header blocks.

[F4][CHANGE]: Make portable-shell hedging a target-shell smell. If a script is Bash-only, the shebang, ShellCheck shell directive, local Bash baseline, and Bash features should own the shell target. POSIX.1-2024 belongs only when the script explicitly claims portable `sh` behavior or runs in POSIX mode.

[F5][CHANGE]: Convert collection-loop versus stream-loop guidance into an edit rule. Use `mapfile` or `readarray` when collecting records into an array; use `while IFS= read -r` only when the body consumes a stream, preserves ordering/backpressure, mutates boundary state intentionally, or processes NUL-delimited filenames. A comment may call a loop a stream only when that boundary is real.

[F6][CHANGE]: Add Bash-specific redaction guidance only where Bash owns the behavior. Source comments may name secret class, validation shape, redaction rule, export behavior, and log sink; they must not include literal secrets, sample credentials, retrieval paths, raw CI masked values, or debug traces. In GitHub Actions, masking must happen before the value is logged; in Bash, `set -x` and `BASH_XTRACEFD` are trace-output surfaces that require explicit redaction or disablement.

[F7][NO_CHANGE]: Do not import Google Shell Style Guide function-header templates wholesale. They are useful background for stdout/stderr and non-obvious comments, but Rasm's active source-comment standard is narrower and should not require `Globals`, `Arguments`, `Outputs`, and `Returns` blocks for every Bash function.

## [2][REPO_EVIDENCE]

[REQUESTED_FILES_READ]:
- `docs/standards/reference/code-documentation.md`: full file read with line numbers.
- `docs/standards/README.md`: full file read with line numbers.
- `docs/standards/AGENTS.md`: full file read with line numbers.
- `docs/standards/information-structure.md`: full file read with line numbers.
- `docs/standards/proof.md`: full file read with line numbers.
- ``: full file read with line numbers.
- `docs/standards/style-guide.md`: full file read with line numbers.
- `docs/standards/formatting.md`: full file read with line numbers.

[GOVERNING_INSTRUCTIONS_READ]:
- `CLAUDE.md`: confirms Markdown routes through `docs/standards`, current-source verification for research, and no comments that describe only "what."
- `AGENTS.md`: confirms docs work routes through `docs/standards/README.md`, requires nested overlay discovery, and rejects C# static, test, or bridge proof claims for docs-only edits.
- `docs/standards/AGENTS.md`: confirms active standards must stay durable and route each rule to its owner; `.reports/` research is input material only.
- `/Users/bardiasamiee/.codex/skills/coding-bash/SKILL.md`: confirms repo-aligned Bash doctrine around Bash 5.3, dispatch tables, strict mode, `mapfile`, streaming-boundary comments, ShellCheck, and redaction-aware logging.

[ACTIVE_STANDARD_SPANS]:
- `docs/standards/reference/code-documentation.md:3-14`: comments own omitted caller-visible obligations, outcomes, failure channels, side effects, security exposure, lifecycle signals, and rationale; generated catalogs and task routes leave comments.
- `docs/standards/reference/code-documentation.md:119-123`: Bash scripts, command functions, dispatch tables, traps, nameref outputs, and streaming loops use the script surface profile with explicit exit-status failure.
- `docs/standards/reference/code-documentation.md:266-292`: the Bash capsule says Bash 5.3+ has no docstrings, names contract-comment owners, names current-shell substitution features, and rejects pseudo-docstrings, blanket function comments, portable-shell hedging, bare ShellCheck disables, mixed stdout data/logs, and collection loops documented as streams.
- `docs/standards/reference/code-documentation.md:344-349`: Bash cross-references use literal command, variable, function, and ShellCheck code spans; there is no generated-reference profile by default.
- `docs/standards/reference/code-documentation.md:357-367`: cross-language anti-patterns already reject type-restating parameters, name-echo summaries, profile leakage, line narration, and generated catalogs.
- `docs/standards/reference/code-documentation.md:393-411`: validation checks that Bash surfaces name stdout, stderr, exit status, traps, resources, state, and ShellCheck rationale, and that inline comments state reasons instead of narration.
- `docs/standards/proof.md:78-85`: maintained upstream sources and current sources are required for changing tools, security guidance, support status, and provider behavior.
- `:133-135`: generated or mirrored files must exclude secrets, personal data, task notes, and private machine details; public, internal, restricted, and secret material must be separated or filtered.
- `docs/standards/style-guide.md:41-46`: remove filler, transient language, and non-load-bearing hedges; preserve real uncertainty through proof.
- `docs/standards/formatting.md:151-161`: hidden comments are source notation only and cannot be the only carrier of safety, proof, intent, or required constraints.

[WORKTREE_CONTEXT]:
- `git status --short -- docs/standards/reference/code-documentation.md .reports/code-documentation-050626/track-bash/05-bash-antipatterns.md` showed `docs/standards/reference/code-documentation.md` already modified before this report write.
- No active standards were edited by this worker.

## [3][CURRENT_SOURCE_CHECKS]

[LOCAL_COMMANDS]:
- `bash --version` returned `GNU bash, version 5.3.9(1)-release (aarch64-apple-darwin25.3.0)`.
- `shellcheck --version` returned `version: 0.11.0`.
- `shfmt --version` returned `3.13.1`.
- `rg -n "shellcheck|shfmt|Bash|bash|\\.sh|\\.bash|mapfile|readarray|redact|secret|add-mask" pyproject.toml package.json pnpm-workspace.yaml tools docs -g '*.md' -g '*.toml' -g '*.json'` found current Bash/ShellCheck/shfmt references in `tools/assay/README.md`, the active `code-documentation.md` Bash capsule, and sibling `.reports/` research, but no repo `.sh` or `.bash` source files were discovered by `fd -a '\\.(sh|bash)$' .`.

[PRIMARY_SOURCES]:
- GNU Bash Reference Manual 5.3, top-level manual: https://www.gnu.org/software/bash/manual/bash.html
- GNU Bash Reference Manual 5.3, Comments: https://www.gnu.org/software/bash/manual/html_node/Comments.html
- GNU Bash Reference Manual 5.3, Shell Functions: https://www.gnu.org/software/bash/manual/html_node/Shell-Functions.html
- GNU Bash Reference Manual 5.3, Command Substitution: https://www.gnu.org/software/bash/manual/html_node/Command-Substitution.html
- GNU Bash Reference Manual 5.3, Bash Builtins: https://www.gnu.org/software/bash/manual/html_node/Bash-Builtins.html
- GNU Bash Reference Manual 5.3, Bash Variables: https://www.gnu.org/software/bash/manual/html_node/Bash-Variables.html
- GNU Bash Reference Manual 5.3, Bash POSIX Mode: https://www.gnu.org/software/bash/manual/html_node/Bash-POSIX-Mode.html
- POSIX.1-2024 Shell Command Language: https://pubs.opengroup.org/onlinepubs/9799919799/utilities/V3_chap02.html
- ShellCheck directive documentation, edited 2026-01-30: https://github.com/koalaman/shellcheck/wiki/Directive
- ShellCheck SC2148: https://www.shellcheck.net/wiki/SC2148
- ShellCheck SC2013: https://www.shellcheck.net/wiki/SC2013
- ShellCheck SC2044: https://www.shellcheck.net/wiki/SC2044
- GitHub Actions workflow commands and `add-mask`: https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-commands
- Google Shell Style Guide, comments and shell target rules: https://google.github.io/styleguide/shellguide.html

## [4][SOURCE_NOTES]

[BASH_COMMENT_MODEL]:
- GNU Bash 5.3 manual identifies itself as Edition 5.3, last updated 2025-05-18, and current local Bash is 5.3.9.
- Bash comments are parser-ignored text starting with `#` in the supported comment positions. The manual does not define docstrings, structured source-comment tags, or a generated documentation pipeline.
- Because comments are ignored, Bash cannot enforce a comment profile. The only comment-like surfaces with tool behavior are the shebang and ShellCheck directives.

[FUNCTION_SURFACE]:
- Bash shell functions group commands under a name, execute like simple commands, and run in the current shell context without a new process.
- Function execution replaces positional parameters during the call, restores them afterward, and returns the status of the last command unless `return` supplies a status.
- Bash local variables are dynamically scoped. This is a real caller-visible concern for command functions, nameref outputs, trap handlers, and sourced surfaces; it is not a reason to document every local declaration.

[BASH_5_CURRENT_FEATURES]:
- Standard `$(command)` command substitution runs in a subshell and deletes trailing newlines. Bash 5.3's `${ command; }` runs in the current execution environment, so side effects persist.
- Bash 5.3's `${| command; }` expands from local `REPLY` while stdout remains on the caller stream, which matters when comments explain why stdout is reserved for machine data.
- `mapfile` reads lines into an indexed array; `readarray` is a synonym. This supports using array collection primitives instead of narrating collection loops as streams.
- `BASH_MONOSECONDS` expands from the monotonic clock where available; `EPOCHREALTIME` is wall-clock time. Duration comments should not conflate these.
- `BASH_TRAPSIG` exposes the signal number during a trap. Trap comments can therefore name a signal-dispatch contract instead of manually listing parallel trap bodies.
- `BASH_XTRACEFD` redirects `set -x` trace output to a file descriptor and can close stderr if misused. Redaction comments must treat trace output as a leak surface, not just logging prose.

[PORTABILITY_AND_SHELLCHECK]:
- Bash POSIX mode changes many behaviors to conform more closely to POSIX, and Bash invoked as `sh` enters POSIX mode after startup files. POSIX wording is therefore a runtime target, not a generic hedge.
- ShellCheck SC2148 says tips depend on the target shell and asks for a shebang, `--shell`, ShellCheck shell directive, or shell-specific extension.
- ShellCheck directives can appear in `.shellcheckrc` or as script comments. Directives after the shebang can apply to the whole script; otherwise they scope to the next complete command.
- ShellCheck documentation recommends documenting why a directive was used. The active Rasm standard can stay stricter by preferring a rationale line before the directive and rejecting trailing rationales as noisy.

[LOOPS_AND_STREAMS]:
- ShellCheck SC2013 explains that `for line in $(cat file)` reads words and triggers glob expansion; it recommends `while IFS= read -r` for line input.
- ShellCheck SC2044 explains that `for file in $(find ...)` is fragile and recommends `find -exec`, globstar where the search is simple, or a NUL-delimited `while read -d ''` loop where a shell loop body is needed.
- These sources validate `while read` for real stream consumption, especially line or NUL-delimited streams. They do not justify calling every array-building loop a stream.

[STDOUT_STDERR_REDACTION]:
- Google Shell Style Guide separates normal output from error/status output by sending errors to stderr. Rasm should keep the stronger rule from the active standard: stdout is machine data only, stderr is diagnostics and logs only.
- GitHub Actions `add-mask` must be registered before the secret appears in logs or workflow commands, and masked values are treated as secrets by the runner.
- GitHub Actions can stop workflow-command processing before logging arbitrary text. Bash comments that include sample workflow commands should not publish actual secrets, handles, or raw masked values.

## [5][ANTIPATTERN_TABLE]

| [INDEX] | [ANTIPATTERN]                               | [SOURCE_BASIS]                                                                                                                | [STANDARD_ACTION]                                                                                                                           |
| :-----: | :------------------------------------------ | :---------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------ |
|   [1]   | Pseudo-docstring block                      | Bash comments are ignored; no docstring toolchain exists.                                                                     | Reject structured docstring theater; use a script header or narrow contract comment only when a caller-visible contract is omitted.         |
|   [2]   | Function header on every function           | Google allows headers for non-obvious or library functions; Rasm comments exist only when declarations omit caller semantics. | Document public command/trap/stream/nameref surfaces; delete short internal function headers that echo name, args, locals, or exit status.  |
|   [3]   | Mechanical parameter block                  | Bash positional parameters and local declarations carry syntax, not semantic obligations.                                     | Replace `Arguments: $1 path` with unit, origin, trusted boundary, mutability, stdout, stderr, exit status, or delete it.                    |
|   [4]   | Portable-shell hedge in Bash script         | ShellCheck and Bash POSIX mode require a real target shell.                                                                   | Use `#!/usr/bin/env bash`, `# shellcheck shell=bash`, or repo tool config; mention POSIX only for explicit portable-shell targets.          |
|   [5]   | Collection loop labeled as stream           | Bash has `mapfile` and `readarray`; ShellCheck loop fixes target real line or file streams.                                   | Use array primitives for collection; reserve stream comments for ordering, delimiter, backpressure, finalization, and failure propagation.  |
|   [6]   | Bare ShellCheck disable                     | ShellCheck directives are executable analyzer control comments.                                                               | Add a preceding rationale naming diagnostic class and invariant; keep directive syntax compact and scoped to the smallest complete command. |
|   [7]   | Mixed stdout data/logs                      | Active standard and Google both separate stdout from diagnostics.                                                             | Comment stdout as machine data, stderr as logs, and exit status as failure; reject comments that imply logs on stdout.                      |
|   [8]   | Secret value in example comment             | GitHub masks only after registration; source comments are durable exposure.                                                   | Name secret class and redaction rule; never include literal secret, credential route, masked sample value, or debug trace.                  |
|   [9]   | Trace output treated as harmless            | `BASH_XTRACEFD` and `set -x` produce command traces that can contain expanded values.                                         | Document trace sink, redaction, and disablement when tracing crosses a secret or CI boundary.                                               |
|  [10]   | Current-shell substitution without contract | `${ command; }` and `${                                                                                                       | command; }` preserve or separate side effects in Bash 5.3.                                                                                  | Comment only when persisted mutation, stdout separation, `REPLY`, `return`, `exit`, or positional-parameter sharing changes caller behavior. |

## [6][RECOMMENDATIONS]

[ADD][BASH_COMMENT_BUDGET]:
Add a compact positive rule near the Bash capsule before `Reject`:

```text
[COMMENT_BUDGET]:
- Use a Bash comment only when it changes caller action, analyzer behavior, or maintenance safety: script contract, command function contract, environment contract, dispatch route, trap/cleanup ownership, nameref return, stream boundary, durable write, current-shell substitution, redaction boundary, or ShellCheck directive rationale.
- Omit comments that restate Bash syntax, local declarations, readonly state, associative-array shape, positional-parameter numbering, or function names.
```

Reason: this turns "no pseudo-docstrings" and "no comments for every function" into an actionable chooser.

[CHANGE][FUNCTION_COMMENTS]:
Replace any future blanket wording with a surface split:

```text
Command-function comments name public command purpose, admitted arguments, globals read or written, stdout, stderr, exit status, side effect, dispatch route, and boundary obligations only when those facts are not already obvious from the command surface. Short internal functions without boundary behavior use names, `local`, `readonly`, and typed dispatch tables instead of header blocks.
```

Reason: Shell style guides may require function headers for libraries, but Rasm's standard is source-contract-first and rejects comments that restate code.

[CHANGE][PORTABILITY]:
Strengthen portable-shell hedging into target-shell proof:

```text
Bash-only scripts state their target through shebang, ShellCheck directive, repo tool config, or local Bash baseline. Do not mention POSIX, portable `sh`, dash, ksh, zsh, macOS `/bin/bash`, or BusyBox compatibility unless the script is tested or explicitly constrained for that target.
```

Reason: ShellCheck diagnostics and Bash POSIX mode both depend on the target shell; hedging without a tested target creates false support claims.

[CHANGE][STREAMS_COLLECTIONS]:
Make stream comments falsifiable:

```text
Collection uses `mapfile` or `readarray` when the result is an array. A `while IFS= read -r` or `while IFS= read -r -d ''` loop is a stream boundary only when the body consumes incrementally, preserves order, applies backpressure, handles a delimiter, mutates boundary state intentionally, or propagates per-record failure. The comment names that boundary; otherwise delete the stream wording.
```

Reason: ShellCheck validates `while read` for line and file streams, but Rasm's Bash style rejects collection loops documented as streams.

[CHANGE][REDACTION]:
Add Bash-specific redaction wording to the environment contract or rails/resource bullets:

```text
Environment and logging comments name the required variable, validation regex or source class, export behavior, redaction rule, log sink, and failure status. They never include the secret value, a realistic token shape, retrieval path, raw masked output, `set -x` transcript, or credential-handling bypass. In GitHub Actions, register `add-mask` before any possible output; in Bash tracing, route or disable `BASH_XTRACEFD` before expanding sensitive values.
```

Reason: redaction is an observable Bash/CI contract, but comments must not become the leak.

[CHANGE][SHELLCHECK_DIRECTIVES]:
Keep the active rejection of bare disables and trailing directive rationales, with one clarifying sentence:

```text
ShellCheck directives are analyzer-control comments. Put a short rationale immediately before the directive, naming the diagnostic code and invariant; keep the directive line machine-scannable and scoped to the smallest complete command.
```

Reason: ShellCheck permits trailing rationale comments, but a stricter repo rule improves scanning and avoids hiding the reason after a disable.

## [7][NO_CHANGE_CONFIRMATIONS]

[NO_CHANGE][NO_DOCSTRINGS]:
Keep "Bash 5.3+ has no docstrings." GNU Bash documents comments as ignored parser text and has no source-comment tag system comparable to XML comments, TSDoc, Python docstrings, or PostgreSQL `COMMENT ON`.

[NO_CHANGE][NO_GENERATED_CATALOGS]:
Keep generated Bash catalogs rejected by default. Bash comments can support help text and script contracts, but there is no default generated-reference profile in this repo.

[NO_CHANGE][STDOUT_STDERR_EXIT]:
Keep stdout as machine data, stderr as diagnostics/logs, and exit status as the failure rail. This is both current active standard and compatible with external shell style guidance.

[NO_CHANGE][SHELLCHECK_RATIONALE]:
Keep ShellCheck directives inside the Bash capsule. They are one of the few Bash comment forms with tool behavior, so their rationale belongs in code-documentation guidance, not a generic style-only rule.

[NO_CHANGE][SECURITY_BLACKLIST]:
Do not add a long Bash-only secret blacklist. Use the cross-language security-comment rule from sibling research, then add Bash-specific trace, redaction, and `add-mask` ordering only where Bash/CI behavior controls the contract.

## [8][DRAFT_EDIT_MAP]

[PRIMARY_OWNER]:
- `docs/standards/reference/code-documentation.md` owns Bash source comments, script contracts, language capsule rules, inline rationale, generated-reference absence, ShellCheck directive rationale, and Bash anti-patterns.

[SUPPORTING_OWNER]:
- `docs/standards/proof.md` owns current-source proof and freshness for Bash/ShellCheck/shfmt version claims.
- `` owns generated mirror separation, access classes, and exclusion of secrets from generated surfaces.
- `docs/standards/style-guide.md` owns sentence mechanics and hedge removal; it should not duplicate Bash-specific comment rules.

[DO_NOT_EDIT_FOR_THIS]:
- `docs/standards/README.md` already routes code documentation correctly.
- `docs/standards/formatting.md` already owns hidden Markdown comments and directive-style notation, but ShellCheck directives stay in the Bash code-documentation capsule because they are tool behavior.

## [9][CONFIDENCE]

[HIGH]:
- Bash comments are ignored parser text and Bash has no native docstring profile.
- Bash 5.3 current-shell substitution, `mapfile`, `readarray`, `BASH_MONOSECONDS`, `BASH_TRAPSIG`, and `BASH_XTRACEFD` are current official Bash 5.3 surfaces.
- ShellCheck directives are comment syntax with real analyzer scope and should be documented when disabled.
- Bash-only scripts should not claim POSIX portability unless that is the explicit target.

[MEDIUM]:
- `BASH_XTRACEFD` should be named in the active standard. It is a real leak surface, but the later editor may prefer a generic "trace output" rule to avoid overfitting.
- `add-mask` should be named in Bash guidance. It is GitHub Actions-specific, but the repo has CI-facing Bash documentation and the active standard already names redaction as an environment-contract field.

[LOW]:
- Any claim about repo-local Bash source patterns. `fd -a '\\.(sh|bash)$' .` found no `.sh` or `.bash` source files in this checkout, so this report is standards research rather than a source-pattern audit.

## [10][TRANSCRIPT]

1. Read memory routing for recent `docs/standards` work; memory established that active standards outrank `.reports/`, and `.reports/` is source material only.
2. Discovered requested files and instruction files with `fd`.
3. Read `CLAUDE.md`, root `AGENTS.md`, and `docs/standards/AGENTS.md`.
4. Read requested active standards in full with `nl -ba`: `README.md`, `reference/code-documentation.md`, `information-structure.md`, `proof.md`, `style-guide.md`, and `formatting.md`.
5. Re-read `code-documentation.md` Bash capsule lines `266-292` after the full file output truncated neighboring language capsules.
6. Loaded `/Users/bardiasamiee/.codex/skills/coding-bash/SKILL.md` because the task focuses on Bash 5 comment/style rules.
7. Checked current worktree status for assigned scope and target active standard. Found pre-existing modification to `docs/standards/reference/code-documentation.md`.
8. Queried current primary sources for GNU Bash 5.3 comments, functions, command substitution, builtins, variables, POSIX mode, POSIX.1-2024 shell language, ShellCheck directives and diagnostics, GitHub Actions masking, and Google Shell Style Guide.
9. Ran local version checks for Bash, ShellCheck, and shfmt.
10. Ran targeted `rg` scans over docs and tooling for Bash, ShellCheck, shfmt, redaction, and secret terms.
11. Wrote only this assigned `.reports/` report.

## [11][CLOSE_CHECK]

- [x] Assigned report file created at `.reports/code-documentation-050626/track-bash/05-bash-antipatterns.md`.
- [x] Requested active standards were read fully.
- [x] Current primary sources were used for drift-prone Bash, ShellCheck, CI masking, and POSIX claims.
- [x] Findings, source notes, add/change/no-change recommendations, confidence, and transcript are included.
- [x] No active standards were edited.
