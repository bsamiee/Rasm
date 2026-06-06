# [BASH_02_SHELLCHECK_SHFMT]

ShellCheck directives are source comments with executable lint meaning, so the standard should treat them as documentation only when they record the local invariant that makes a diagnostic suppression, source map, or dialect override true. `shfmt` proves parseability and layout under its selected dialect and flags; it does not prove ShellCheck directive correctness, runtime behavior, source-path truth, portability, or comment quality.

## [1][DECISION]

[KEEP]:
- Keep the current split in `docs/standards/reference/code-documentation.md`: `shfmt` owns layout and parse shape only, while ShellCheck directives are documentation when they name a diagnostic class and the local invariant.
- Keep the rejection of bare ShellCheck disables. A directive that changes analysis without saying why the warning is locally false is an undocumented exception, not a completed comment.
- Keep the rejection of trailing directive rationales as a Rasm style rule, but clarify if active text changes that this is stricter than upstream ShellCheck. Upstream permits a short end-of-directive explanation; Rasm should prefer a preceding rationale line so the machine directive stays easy to scan and the human invariant stays independently editable.

[ADD_OR_CLARIFY]:
- Clarify that ShellCheck directives apply to the next complete command or compound command, or file-wide when placed before the first command. They are not line comments attached to arbitrary text, individual `elif` branches, case labels, or post-command trailing comments.
- Clarify that `source` and `source-path` directives document ShellCheck's static source map, not the script's runtime source semantics. Runtime path ownership stays in Bash code or script-header comments; the directive only tells ShellCheck which file to include or where to search.
- Clarify that `# shellcheck shell=bash` is a lint dialect override, not a portability claim. Use it when a file lacks a recognizable shebang, uses a wrapper shebang, or is meant to be sourced; do not use it to claim POSIX portability or Bash support.
- Clarify that `shfmt -d` is formatting and syntax proof only. It does not prove ShellCheck clean output, directive placement, source resolution, runtime sourcing, stdout or stderr contracts, exit statuses, traps, cleanup, durable writes, or security behavior.

## [2][DIRECTIVE_BOUNDARIES]

ShellCheck directives have two jobs that must not be conflated. First, they are machine-readable configuration that changes static analysis. Second, because they live in source comments, a local directive can also carry source documentation when it records the invariant that justifies the machine override.

[DISABLES]:
- Use a local `disable` only beside the command or compound command whose diagnostic is intentionally false or intentionally accepted.
- Name each SC code and write the invariant in a nearby comment before the directive. Use one disable directive for one invariant; split unrelated codes instead of hiding several risks under one comment.
- Use file-wide disables only for generated, vendor, or structurally exceptional files with a visible file-level reason. Prefer `.shellcheckrc` only for project policy that applies uniformly; do not turn source comments into a global diagnostic catalog.
- Treat `disable=all` as a generated-file or quarantine escape hatch, not normal source documentation.

[SOURCE_MAPS]:
- Use `source=<path>` when ShellCheck cannot infer a dynamic source path and one concrete checked file represents the runtime include.
- Use `source=/dev/null` only when analysis of the sourced file is intentionally impossible or irrelevant, and state what prevents sourced definitions from becoming hidden dependencies.
- Use `source-path=<path>` or `source-path=SCRIPTDIR` when several includes share one search base. If the same base applies project-wide, prefer `.shellcheckrc` over repeated local comments.
- Keep `external-sources=true` out of script comments. Upstream requires enabling it from `.shellcheckrc`; a script may not grant itself broad source-file access.

[SHELL_DIALECT]:
- Let a recognized shebang own the runtime shell.
- Use `# shellcheck shell=bash`, `shell=sh`, `shell=dash`, or `shell=ksh` only to tell ShellCheck how to parse a source file whose shebang is missing, wrapped, or intentionally nonstandard.
- Do not use a ShellCheck dialect directive to document `zsh`, PowerShell, Python, SQL, or other unsupported languages. ShellCheck does not support those surfaces, even though `shfmt` can parse some dialects ShellCheck cannot.
- Route a real portability claim to the script contract: Bash-only scripts say Bash; portable scripts state the supported shell and avoid Bash-only constructs. POSIX.1-2024 belongs only when the script explicitly claims portable shell semantics.

## [3][SHFMT_BOUNDARIES]

`shfmt` is a parser and printer. It can check formatting with `-d`, write formatting with `-w`, list differing files with `-l`, and select a dialect with `-ln` or `-p`. Its dialect auto-detection uses filename and shebang signals, treats `.sh` as POSIX unless a valid shebang overrides it, and falls back to Bash when no signal exists.

[PROVES]:
- The file parses as the selected `shfmt` dialect.
- The printed form matches `shfmt` under the selected flags or EditorConfig options.
- A syntax failure is observable for the parser and dialect used by the command.

[DOES_NOT_PROVE]:
- ShellCheck warnings, optional checks, severity policy, source resolution, or directive scope.
- Runtime behavior under Bash, POSIX sh, dash, ksh, zsh, or the host shell.
- Portability across ShellCheck-supported and shfmt-supported dialects.
- Comment quality, public script contract completeness, stdout or stderr separation, exit-status vocabulary, trap safety, cleanup order, durable-write semantics, or security exposure.
- That simplification with `-s` is behavior-preserving for the caller contract; treat `-s` as a code transform, not documentation proof.

[RASM_CONFIG]:
- `.editorconfig` currently has no `shell_variant`, `simplify`, `binary_next_line`, `switch_case_indent`, `function_next_line`, or shell-specific section. General whitespace settings apply, but there is no checked-in shfmt dialect policy in `.editorconfig`.
- `tools/assay/composition/catalog.py` registers `shellcheck -f json1`, `shfmt -d`, and `shfmt -w` as Bash static tools when available. That proves the assay catalog can invoke the tools; it does not add a docs-standard proof gate for `_reports/` research reports.
- Local tool probes on 2026-06-05 returned ShellCheck `0.11.0` and shfmt `3.13.1`. These match the current upstream surfaces used for this report.

## [4][ACTIVE_STANDARD_IMPLICATIONS]

The Bash capsule should stay compact. It should not become a ShellCheck manual, shfmt option catalog, or shell-style guide. The active standard only needs enough policy to decide whether a source comment is needed and where proof routes.

Suggested replacement-level wording for the current Bash feature bullets:
- `shfmt` owns parse and layout proof under the selected dialect and flags; it does not prove ShellCheck diagnostics, runtime behavior, portability, or script contract documentation.
- ShellCheck directives are parsed lint controls and source documentation: every local `disable`, `source`, `source-path`, or `shell` directive names the diagnostic or analysis gap and the local invariant that makes the override true.

Suggested rejection wording:
- Reject bare ShellCheck disables, `disable=all` in normal source, trailing directive rationales, project-wide source paths repeated beside each include, `external-sources=true` inside scripts, dialect directives used as portability claims, and shfmt proof claims that imply semantic lint, runtime, or documentation correctness.

## [5][SOURCE_REGISTER]

ShellCheck directive page
    Source: https://github.com/koalaman/shellcheck/wiki/Directive
    Current signal: edited 2026-01-30.
    Used for: directive scope, supported directives, `.shellcheckrc`, `source`, `source-path`, `shell`, and upstream allowance for directive-use comments.

ShellCheck ignore page
    Source: https://github.com/koalaman/shellcheck/wiki/Ignore
    Current signal: edited 2026-04-17.
    Used for: local disables, multiple codes, run-level excludes, `.shellcheckrc`, and `disable=all`.

ShellCheck diagnostic pages
    Source: https://www.shellcheck.net/wiki/SC1126, https://www.shellcheck.net/wiki/SC1123, https://www.shellcheck.net/wiki/SC1107, https://www.shellcheck.net/wiki/SC1008, https://www.shellcheck.net/wiki/SC1071, https://www.shellcheck.net/wiki/SC1090, https://www.shellcheck.net/wiki/SC1144.
    Current signal: shellcheck.net wiki pages crawled in the current research pass.
    Used for: directive placement, complete-command scope, unknown directives, supported shell dialects, dynamic source handling, and `.shellcheckrc`-only `external-sources=true`.

shfmt upstream README and manpage
    Source: https://github.com/mvdan/sh and https://github.com/mvdan/sh/blob/master/cmd/shfmt/shfmt.1.scd
    Current signal: latest release `v3.13.1` on 2026-04-06; manpage source read from `master`.
    Used for: parser/printer scope, supported dialects, `-d`, `-w`, `-l`, `-ln`, `-p`, EditorConfig behavior, syntax-check claim, and dialect auto-detection.

Rasm local sources
    Source: `.editorconfig`, `tools/assay/composition/catalog.py`, `tools/assay/README.md`, `docs/standards/reference/code-documentation.md`.
    Current signal: read locally on 2026-06-05.
    Used for: absence of shell-specific EditorConfig shfmt policy, assay Bash static tool registration, and current active Bash capsule text.

## [6][OPEN_QUESTIONS]

- Should the active standard explicitly say that the trailing-rationale ban is Rasm style, not ShellCheck syntax? This would prevent a future maintainer from incorrectly arguing that upstream rejects trailing rationale comments.
- Should `source-path` guidance prefer `.shellcheckrc` once more than one script shares the same source root? Upstream supports both local directives and rc-file entries; Rasm likely wants the project-wide source map in one place when it is not local to a single include.
- Should docs distinguish `shfmt -d` from `shfmt -d -ln=bash` in proof wording? Without an explicit dialect or a reliable shebang/filename signal, shfmt's fallback can make a proof claim too broad.

## [7][VALIDATION]

- [x] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, shared standards, and `docs/standards/reference/code-documentation.md`.
- [x] Checked current ShellCheck and shfmt primary sources.
- [x] Checked Rasm local `.editorconfig`, assay Bash tool catalog, and local tool versions.
- [x] `git diff --check -- docs/standards/_reports/code-documentation-050626/track-bash/02-shellcheck-shfmt.md` passed.
