# [BASH_01_BASH53_FEATURES]

Bash 5.3 source comments should document observable shell contracts created by current-shell execution, dynamic variables, glob ordering, traps, and portability claims. The active standard already has the right Bash capsule shape; this report tightens the source evidence and identifies where POSIX.1-2024 changes older portability wording.

## [1][SCOPE]

Workspace: `/Users/bardiasamiee/Documents/99.Github/Rasm`
Assigned output: `docs/standards/_reports/code-documentation-050626/track-bash/01-bash53-features.md`
Active standards edited: no
Research date: 2026-06-05
Primary source family: GNU Bash 5.3 manual, Bash 5.3 release announcement, and POSIX.1-2024 Issue 8.

Read scope:
- `CLAUDE.md`
- root `AGENTS.md`
- `docs/standards/AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/information-structure.md`
- `docs/standards/style-guide.md`
- `docs/standards/proof.md`
- `docs/standards/formatting.md`
- `coding-bash` skill entrypoint plus `version-features.md`, `variable-features.md`, and `bash-portability.md`

Current workspace note: `docs/standards/reference/code-documentation.md` was already modified in the worktree before this report. This report leaves that active file untouched.

## [2][SOURCE_MAP]

GNU Bash 5.3 documentation is the controlling upstream source for Bash-only behavior. POSIX.1-2024 Issue 8 controls portable-shell claims; when the Bash manual and POSIX differ, the standard should say which shell contract the script claims.

[CURRENT_PRIMARY]:
- GNU Bash Reference Manual, `Command Substitution`: <https://www.gnu.org/software/bash/manual/html_node/Command-Substitution.html>. Evidence: edition 5.3 documents `${ command; }` and `${| command; }`, current-shell execution, side-effect persistence, shared positional parameters, `return`, `exit`, stdout capture, `REPLY` expansion, and `REPLY` restoration. Last verified: 2026-06-05. Review trigger: Bash command-substitution grammar or REPLY semantics change.
- GNU Bash Reference Manual, `Bash Variables`: <https://www.gnu.org/software/bash/manual/html_node/Bash-Variables.html>. Evidence: `BASH_MONOSECONDS`, `BASH_TRAPSIG`, `EPOCHREALTIME`, `GLOBSORT`, and `REPLY` variable contracts. Last verified: 2026-06-05. Review trigger: Bash dynamic-variable docs change.
- GNU Bash Reference Manual, `The Shopt Builtin`: <https://www.gnu.org/software/bash/manual/html_node/The-Shopt-Builtin.html>. Evidence: `array_expand_once` suppresses repeated evaluation of associative and indexed array subscripts; `assoc_expand_once` is deprecated as a synonym. Last verified: 2026-06-05. Review trigger: `shopt` option names or semantics change.
- GNU Bash Reference Manual, `Bash Builtin Commands`: <https://www.gnu.org/software/bash/manual/html_node/Bash-Builtins.html>. Evidence: `read [-Eers] ...` and `source [-p path] filename ...` are current Bash 5.3 builtin forms. Last verified: 2026-06-05. Review trigger: builtin synopsis or option behavior changes.
- Bash 5.3 release announcement by Chet Ramey: <https://lists.nongnu.org/archive/html/bash-announce/2025-07/msg00000.html>. Evidence: Bash 5.3 added current-shell command substitution, `GLOBSORT`, `read -E`, `source -p PATH`, `array_expand_once`, `BASH_MONOSECONDS`, and `BASH_TRAPSIG`; the announcement says the man page is the definitive feature description. Last verified: 2026-06-05. Review trigger: Bash 5.3 patches or Bash 5.4 release change these features.
- POSIX.1-2024 Issue 8, Shell Command Language: <https://pubs.opengroup.org/onlinepubs/9799919799/utilities/V3_chap02.html>. Evidence: standard command substitution runs commands in a subshell environment; pipeline exit status now depends on `pipefail`; pathname expansion order is locale collation; command names including `local` have unspecified results in the command-search table. Last verified: 2026-06-05. Review trigger: POSIX Issue 8 interpretations or Issue 9 changes.
- POSIX.1-2024 Issue 8, `find`: <https://pubs.opengroup.org/onlinepubs/9799919799/utilities/find.html>. Evidence: `find -print0` is standardized and writes a null byte after each pathname; `-exec ... {} +` aggregates pathnames and is usually safer than `find -print0 | xargs -0` when partial pathname output is a concern. Last verified: 2026-06-05. Review trigger: POSIX `find` page or defect interpretations change.
- POSIX.1-2024 Issue 8, `xargs`: <https://pubs.opengroup.org/onlinepubs/9799919799/utilities/xargs.html>. Evidence: `xargs -0` is standardized and processes null-byte-delimited byte input rather than text input. Last verified: 2026-06-05. Review trigger: POSIX `xargs` page or defect interpretations change.

## [3][FINDINGS]

### [3.1][CURRENT_SHELL_SUBSTITUTION]

Current fact: Bash 5.3 adds an alternate command-substitution family. `${ command; }` executes `command` in the current execution environment and expands to captured stdout after removing trailing newlines. `${| command; }` executes in the current environment but expands to the local `REPLY` value after `command` runs; stdout remains the caller's stdout and trailing newlines in `REPLY` are not stripped.

Documentation implication: this feature changes more than performance. Current-shell substitution preserves side effects, shares positional parameters with the caller, allows `return` to complete the substitution body, and lets `exit` exit the shell. A source comment is justified when those effects are part of the public script or command-function contract.

Standard-ready rule: document `${ command; }` only when the caller-visible contract depends on current-shell mutation, working directory changes, traps, positional parameters, `return`, `exit`, or stdout capture that would behave differently from `$()`.

Standard-ready rule: document `${| command; }` when the public contract separates a structured return value in `REPLY` from caller-visible stdout. The comment should name that stdout remains data/log stream output and that `REPLY` is restored after substitution.

Standard-ready rejection: do not comment on every Bash 5.3 substitution merely because it is new syntax. If the substitution is an implementation-local fork reduction and no caller observes mutation, stdout separation, or exit behavior, the declaration and shell syntax carry enough shape.

### [3.2][REPLY_BOUNDARIES]

Current fact: `REPLY` is already the default destination for `read` without a variable name. Bash 5.3 adds a distinct local-`REPLY` command-substitution shape: Bash creates `REPLY` as initially unset for the `${| command; }` body and restores its previous value after the expansion.

Documentation implication: `REPLY` now has two nearby but different source-comment concerns. `read` without variables uses global `REPLY`; `${| command; }` uses a substitution-local `REPLY` value as the expansion result. A comment should state which `REPLY` contract applies only when callers or maintainers could confuse data flow.

Standard-ready rule: for nameref-style output functions, prefer explicit nameref comments over `REPLY` comments unless the function is intentionally a `${| ...; }` body or a public `read` wrapper. `REPLY` should not become a generic hidden return channel in script APIs.

Standard-ready rule: when `${| ...; }` is used inside a public command function, document whether stdout is intentionally unused, forwarded, or reserved for machine data. This aligns with the active Bash capsule's stdout/stderr/exit-status separation.

### [3.3][TIME_VARIABLES]

Current fact: `EPOCHREALTIME` expands to Unix epoch time as a floating-point value with microsecond granularity; assignments are ignored, and unsetting it removes its special properties. `BASH_MONOSECONDS` expands to the system monotonic clock when available; if no monotonic clock exists, it is equivalent to `EPOCHSECONDS`, and unsetting it also removes special properties.

Documentation implication: `EPOCHREALTIME` and `BASH_MONOSECONDS` document different contracts. `EPOCHREALTIME` is wall-clock timestamp material and can move with system-time adjustments. `BASH_MONOSECONDS` is elapsed-time material when the platform supplies a monotonic clock, but the manual explicitly defines the fallback to `EPOCHSECONDS`.

Standard-ready rule: document `BASH_MONOSECONDS` when elapsed duration, timeout, service-level deadline, retry budget, or watchdog semantics are caller-visible. State the fallback only when the script claims monotonic behavior across platforms and the fallback affects correctness.

Standard-ready rule: document `EPOCHREALTIME` when the public output, log, trace, receipt, cache key, or generated artifact embeds wall-clock time or microsecond precision. Do not use it as the preferred elapsed-time comment example where `BASH_MONOSECONDS` is available and seconds resolution is sufficient.

Standard-ready rejection: do not state that `BASH_MONOSECONDS` is unconditionally monotonic on every platform. The Bash manual says it uses the monotonic clock if one is available and otherwise equals `EPOCHSECONDS`.

### [3.4][GLOBSORT]

Current fact: `GLOBSORT` controls Bash filename expansion sorting. Unset or null values use historical ascending lexicographic sorting under `LC_COLLATE`. Valid values use an optional `+` or `-` sort-order prefix followed by a sort specifier; Bash 5.3 release notes name `name`, `size`, `blocks`, `mtime`, `atime`, `ctime`, `numeric`, and `none`.

Documentation implication: `GLOBSORT` can turn glob order into semantic output. It is source-comment-worthy only when glob ordering changes which files are processed first, which item is selected, or whether output order is a public guarantee.

Standard-ready rule: document `GLOBSORT` beside the function or script header when the semantic result depends on newest-first, largest-first, numeric, or unsorted glob order. The comment should name the order as a contract and route broad support status to a support matrix only if users choose shells or platforms from that claim.

Standard-ready rule: avoid preserving `sort`-pipeline examples in comments when `GLOBSORT` is the controlling source behavior. If a fallback pipeline remains for Bash 5.2 or POSIX mode, document the fallback boundary and runtime gate instead of implying `GLOBSORT` is portable.

### [3.5][TRAPS_AND_BUILTINS]

Current fact: Bash 5.3 adds `BASH_TRAPSIG`, set to the numeric signal number while a trap action is executing. It also adds `read -E`, `source -p path`, `array_expand_once`, and related completion/source loading changes.

Documentation implication: these features matter only when the public surface exposes signal routing, interactive input completion, controlled source search, or safe array-subscript evaluation. The standard should not become a Bash 5.3 feature catalog.

Standard-ready rule: trap comments may mention `BASH_TRAPSIG` when one handler dispatches by numeric signal and exit-status mapping, child forwarding, or cleanup order is caller-visible. Otherwise, trap comments should name signal, cleanup order, and exit status without spelling the implementation variable.

Standard-ready rule: `source -p path` deserves a comment when script loading isolation is part of the security or reproducibility contract. The comment should name the allowed library search path and what happens when a required source file is absent.

Standard-ready rule: `array_expand_once` deserves a comment only when a public script relies on suppressing repeated subscript evaluation for safety or side-effect control. Ordinary `shopt -s array_expand_once` should stay uncommented when it is just baseline hardening.

### [3.6][POSIX_2024_BOUNDARY]

Current fact: POSIX.1-2024 Issue 8 standardizes more shell and utility behavior than older portability notes assumed. `set -o pipefail` is in the `set` utility; pipeline exit status depends on the `pipefail` setting when execution begins. `find -print0` and `xargs -0` are standardized. POSIX command substitution still runs commands in a subshell environment. POSIX filename expansion sorting uses locale collation, not Bash `GLOBSORT`. The command-search table marks names including `local` as unspecified if used as command names in conforming applications.

Documentation implication: the Bash capsule should distinguish three cases: Bash 5.3-only behavior, POSIX.1-2024 standardized behavior, and implementation adoption risk. Older claims that `pipefail`, `find -print0`, or `xargs -0` are simply non-POSIX are stale under Issue 8; the durable concern is whether the runtime shell or utility on the target system implements Issue 8.

Standard-ready rule: a script that claims POSIX portability should not use Bash current-shell substitution, `REPLY` substitution, `BASH_MONOSECONDS`, `BASH_TRAPSIG`, `GLOBSORT`, namerefs, arrays, or `local` as a portable contract. If it uses Issue 8 features such as `pipefail`, `find -print0`, or `xargs -0`, the comment should state the Issue 8 dependency or runtime probe when deployment includes older `/bin/sh` or utility sets.

Standard-ready rule: avoid "POSIX says no" wording for `pipefail`, `find -print0`, and `xargs -0`. Use "POSIX.1-2024 Issue 8 standardizes this; gate older runtimes by probe or support matrix" when compatibility matters.

## [4][CAPSULE_IMPLICATIONS]

The current Bash capsule in `code-documentation.md` is directionally correct: Bash has no docstrings, script comments own public script contracts, stdout/stderr/exit status are the rail, and POSIX appears only when a script explicitly claims portable shell semantics. The main refinement is to make the Bash 5.3 list less promotional and more contract-driven.

Recommended additions to consider in the active standard:
- Current-shell substitution: state that comments are required only when current-shell side effects, shared positional parameters, `return`, `exit`, stdout capture, or `REPLY` separation affect the public contract.
- `REPLY`: distinguish the `read` default variable from `${| ...; }` local `REPLY` expansion, and reject `REPLY` as an unannounced generic return channel.
- Timing: distinguish `BASH_MONOSECONDS` elapsed-time contracts from `EPOCHREALTIME` wall-clock timestamp contracts; include the `BASH_MONOSECONDS` fallback to `EPOCHSECONDS` only when platform behavior affects correctness.
- `GLOBSORT`: document it only when glob ordering is semantic output or selection behavior.
- POSIX.1-2024: update portability wording so `pipefail`, `find -print0`, and `xargs -0` are Issue 8 standardized but still deployment-sensitive.

Recommended rejections to consider in the active standard:
- Bash 5.3 comments that exist only to explain new syntax.
- Performance-only `${ command; }` comments where no public side effect or output contract changes.
- Comments that call `BASH_MONOSECONDS` universally monotonic without the manual's fallback qualifier.
- Portability comments that label Issue 8 features as non-POSIX instead of runtime-adoption-sensitive.
- Broad feature catalogs for `read -E`, `source -p`, `array_expand_once`, or `BASH_TRAPSIG` where no public contract depends on them.

## [5][STANDARD_DELTA_CANDIDATE]

The following candidate wording is intentionally not applied to the active standard in this task.

```markdown template
Bash 5.3 current-shell substitution: `${ command; }` and `${| command; }` deserve source comments only when current-shell behavior is caller-visible. Comment the contract when the body mutates caller state, shares positional parameters, relies on `return`, can `exit`, captures stdout as the result, or separates a structured `REPLY` result from caller-visible stdout. Omit comments for implementation-local fork reduction.

Timing variables: `BASH_MONOSECONDS` owns elapsed-duration contracts and `EPOCHREALTIME` owns wall-clock timestamp contracts. Mention the `BASH_MONOSECONDS` fallback to `EPOCHSECONDS` only when platform support changes correctness. Do not describe `EPOCHREALTIME` as a monotonic duration source.

POSIX.1-2024 boundary: `pipefail`, `find -print0`, and `xargs -0` are Issue 8 standardized, but older `/bin/sh` or utility deployments may still lack them. Portable-shell comments should name the Issue 8 dependency, runtime probe, or support route instead of calling those features non-POSIX. Bash-only comments should not hedge Bash features as portable shell behavior.
```

## [6][OPEN_CHECKS]

- [ ] Before active-standard edits, verify whether Rasm actually has Bash scripts that target Bash 5.3 rather than Bash 5.2 or `/bin/sh`; route exact repo baselines to manifests, shebangs, or `coding-bash` rather than this research note.
- [ ] If the active standard adds `GLOBSORT` examples, run a minimal local Bash 5.3 check or mark the example conceptual, because the current macOS or CI Bash may not be 5.3.
- [ ] If the active standard changes POSIX portability wording, consider a sibling `support-matrix.md` or `reference.md` route for runtime adoption instead of embedding platform support tables in code-comment doctrine.

## [7][TRANSCRIPT]

Local governing read:
- `sed -n '1,240p' CLAUDE.md`
- `sed -n '1,240p' AGENTS.md`
- `sed -n '1,260p' docs/standards/README.md`
- `sed -n '1,260p' docs/standards/AGENTS.md`
- `sed -n '1,620p' docs/standards/reference/code-documentation.md`
- `sed -n '1,560p' docs/standards/agentic-documentation.md`
- `sed -n '1,520p' docs/standards/information-structure.md`
- `sed -n '1,240p' docs/standards/style-guide.md`
- `sed -n '1,260p' docs/standards/proof.md`
- `sed -n '1,260p' docs/standards/formatting.md`
- `sed -n '1,220p' /Users/bardiasamiee/.codex/skills/coding-bash/SKILL.md`
- `sed -n '1,520p' /Users/bardiasamiee/.codex/skills/coding-bash/references/version-features.md`
- `sed -n '1,220p' /Users/bardiasamiee/.codex/skills/coding-bash/references/variable-features.md`
- `sed -n '1,220p' /Users/bardiasamiee/.codex/skills/coding-bash/references/bash-portability.md`

Current-source research:
- Web search and open against GNU Bash 5.3 manual, Bash 5.3 release announcement, POSIX Issue 8 shell language, POSIX `find`, and POSIX `xargs`.
- `curl -L --fail --silent https://www.gnu.org/software/bash/manual/html_node/Bash-Variables.html | rg -n -C 2 "BASH_MONOSECONDS|EPOCHREALTIME|GLOBSORT|BASH_TRAPSIG|REPLY"`
- `curl -L --fail --silent https://www.gnu.org/software/bash/manual/html_node/The-Shopt-Builtin.html | rg -n -C 2 "array_expand_once|assoc_expand_once"`
- `curl -L --fail --silent https://www.gnu.org/software/bash/manual/html_node/Bash-Builtins.html | rg -n -C 2 "read .*\\-E|source.*\\-p|compgen.*\\-V|source \\[|read \\["`
- `curl -L --fail --silent https://pubs.opengroup.org/onlinepubs/9799919799/utilities/V3_chap02.html | rg -n -C 2 "pipefail|Command Substitution|local|subshell|current shell"`
- `curl -L --fail --silent https://pubs.opengroup.org/onlinepubs/9799919799/utilities/find.html | rg -n -C 2 "print0|exec.*\\+|OPTIONS|OPERANDS"`
- `curl -L --fail --silent https://pubs.opengroup.org/onlinepubs/9799919799/utilities/xargs.html | rg -n -C 2 -- "-0|null|NUL|OPTIONS|STDIN"`

## [8][VALIDATION]

- [x] Read `CLAUDE.md`, root `AGENTS.md`, `docs/standards/AGENTS.md`, `docs/standards/README.md`, the full target `docs/standards/reference/code-documentation.md`, and shared governing standards.
- [x] Used current primary sources from GNU Bash 5.3 documentation, Bash 5.3 release announcement, and POSIX.1-2024 Issue 8.
- [x] Edited only `docs/standards/_reports/code-documentation-050626/track-bash/01-bash53-features.md`.
- [x] Left active standards untouched.
