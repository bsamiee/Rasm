# BASH 4 Traps Resources Research

Report path: `docs/standards/_reports/code-documentation-050626/track-bash/04-traps-resources.md`
Focus: Bash trap, cleanup, stream, retry, and resource comments for `errexit`, `inherit_errexit`, `ERR` traps, signals, child forwarding, temp dirs, durable writes, and rollback.
Date: 2026-06-05.

## Scope

This report is source material for a later active-standard edit. It does not edit `docs/standards/reference/code-documentation.md` or any other active standard.

The controlling local question is whether the Bash capsule in `code-documentation.md` needs stronger source-comment guidance for failure rails and resource boundaries whose behavior Bash declarations cannot express.

## Read Transcript

Local instruction and target files read fully:

- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/reference/code-documentation.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/information-structure.md`
- `docs/standards/style-guide.md`
- `docs/standards/proof.md`
- `docs/standards/formatting.md`

Adjacent `_reports/` report read for shape:

- `docs/standards/_reports/code-documentation-050626/track-general/05-general-lifecycle.md`

Local status check:

- `git status --short -- docs/standards/_reports/code-documentation-050626 docs/standards/reference/code-documentation.md docs/standards/README.md docs/standards/AGENTS.md`

External primary-source checks:

- GNU Bash Reference Manual 5.3, last updated 2025-05-18.
- GNU Coreutils 9.11 manual, `mktemp` and `mv`.
- ShellCheck Wiki directive pages for `SC1090`, `SC1091`, `SC1126`, `SC1144`, and directive placement or source resolution.
- POSIX.1-2024 `rename()` page and rationale search results.
- Linux man-pages 6.18 `rename(2)` and `fsync(2)`, current 2026 manual pages.

## Source Notes

Local source notes:

- `code-documentation.md` already makes source comments conditional: document caller-visible semantics that declarations, shell syntax, and catalog surfaces do not carry.
- The current Bash capsule already names script headers, command functions, nameref outputs, dispatch tables, environment contracts, trap handlers, streaming loops, retry comments, durable writes, and ShellCheck directives as comment-owned surfaces where caller behavior depends on them.
- The current Bash capsule already rejects pseudo-docstrings, comments for every function, portable-shell hedging in Bash-only scripts, bare ShellCheck disables, trailing directive rationales, mixed stdout data/logs, and collection loops documented as streams.
- The current Bash capsule is Bash 5.3+. This report treats `BASH 4` as the research-shard label, not as a Bash 4.x runtime floor. If a later editor intends Bash 4.x compatibility, the active capsule's Bash 5.3 features need a separate support-matrix or capsule-split decision.
- `proof.md` requires current maintained sources for drift-prone language and tool behavior, and `information-structure.md` favors compact records or tables only when they change reader action.

External source notes:

- GNU Bash 5.3 defines `set -e` as conditional: the shell does not exit for failures in `while` or `until` condition lists, `if` or `elif` tests, non-final `&&` or `||` list elements, non-last pipeline elements unless `pipefail` changes the pipeline status, or commands inverted with `!`.
- GNU Bash 5.3 gives the `ERR` trap the same ignored-context conditions as `errexit`.
- GNU Bash 5.3 `set -E` or `errtrace` makes `ERR` traps inherited by shell functions, command substitutions, and subshell commands; without it, `ERR` is not normally inherited there.
- GNU Bash 5.3 `inherit_errexit` makes command substitutions inherit `errexit`; without it, Bash clears `-e` in command-substitution subshells outside POSIX mode.
- GNU Bash 5.3 resets trapped signals in subshell environments, cannot trap ignored signals in a non-interactive shell, and delays trap execution while waiting for a foreground command until that command completes.
- GNU Bash 5.3 `wait` returns a status greater than `128` when interrupted by a trapped signal, while `kill` assumes process exit statuses greater than `128` correspond to signal termination.
- GNU Bash 5.3 exposes `BASH_TRAPSIG` only while a trap action is executing, and `BASH_MONOSECONDS` and `SRANDOM` give native monotonic-time and jitter primitives where the script baseline permits them.
- GNU Bash 5.3 redirection order is significant, `{varname}` file descriptors persist unless managed by `varredir_close`, and `exec` redirections alter the current shell environment. Comments should state FD ownership only when the lifetime crosses a caller, cleanup, or stream boundary.
- GNU Coreutils 9.11 `mktemp -d` creates a user-only directory, further restricted by a stricter umask, while `mktemp -u` only prints a name and is unsafe for creating later.
- GNU Coreutils 9.11 `mv --no-copy` fails instead of silently copying across filesystems, and `mv --exchange` is atomic only when the source and destination are on a single filesystem that supports atomic exchange.
- Linux `rename(2)` atomically replaces an existing destination path and guarantees that an existing destination remains in place if the operation fails; it also fails with `EXDEV` across mounted filesystems and has NFS retry caveats.
- Linux `fsync(2)` flushes file data and file metadata, but directory entries need an explicit `fsync()` on a directory file descriptor for the directory update to reach disk.
- ShellCheck directives are source comments with tool semantics: `source=` directs static resolution for dynamic source paths, directives must precede commands, and `external-sources=true` belongs in `.shellcheckrc`, not in a script comment.

## Findings

### Finding 1: The current Bash capsule is directionally correct.

The active standard already puts Bash comments on the right side of the source-truth boundary: comments own caller-visible stdout, stderr, exit-status, trap, cleanup, environment, and resource contracts when syntax cannot carry them. Primary Bash sources confirm that these contracts are semantic rather than mechanical because trap inheritance, subshell reset, signal timing, and `errexit` ignored contexts are not visible from a function name or local declaration.

Confidence: high.

Recommendation: preserve the capsule's conditional-comment posture. Add only sharper decision criteria for traps, `errexit`, signal forwarding, durable writes, and rollback.

### Finding 2: `errexit` and `ERR` comments need an ignored-context rule.

`set -e` and `ERR` traps are often misread as blanket failure handling, but Bash explicitly exempts conditional tests, non-final list elements, non-last pipeline elements subject to `pipefail`, and inverted commands. A source comment should document `errexit` or an `ERR` trap only when a public script surface relies on that behavior, and it should name the contexts where failure is intentionally suppressed or converted.

Confidence: high.

Recommendation: add a compact rule to the Bash capsule:

- Comment `errexit` or `ERR` only when the caller relies on the failure rail.
- State ignored contexts, `pipefail` dependency, `errtrace` inheritance, and `inherit_errexit` command-substitution behavior when they affect observable outcome.
- Reject comments that say `set -e handles errors` without naming the failure rail and the exceptions.

### Finding 3: Signal forwarding comments should name the process relationship.

Bash signal behavior depends on interactivity, job control, foreground process groups, asynchronous children, `wait`, and whether a signal was ignored when the shell started. A PID 1 or supervisor script comment should not merely say "forward signals"; it should name the child PID or process group, trapped signals, forwarding command, wait strategy, reentrancy guard, and exit-status mapping.

Confidence: high.

Recommendation: add signal-forwarding fields to the existing trap and cleanup comment list:

- `Signal set`
- `Forward target`
- `Wait owner`
- `Exit mapping`
- `Reentrancy guard`
- `Ignored-signal caveat`

Use these as semantic fields in prose, not as mandatory labels for every trap comment.

### Finding 4: Cleanup comments need acquisition order and rollback state, not broad cleanup prose.

The active capsule already names LIFO cleanup ownership and idempotence. The missing nuance is that cleanup comments should distinguish successful release, failed acquisition, rollback after partial mutation, and signal-time cleanup. This matters because Bash traps can run after an asynchronous `wait` interruption, can be delayed while a foreground command runs, and can run with partially acquired resources.

Confidence: high.

Recommendation: extend the cleanup rule to state acquisition and release order, partial-acquisition behavior, idempotence, and whether cleanup runs on success, ordinary failure, `ERR`, `EXIT`, or trapped signals. Reject one-line `cleanup temp files` comments when rollback state is observable.

### Finding 5: Durable-write comments should separate atomic replacement from crash durability.

`mktemp` plus same-directory rename or `mv` is a strong atomic-replacement pattern, but it does not prove crash durability by itself. Current Linux manual pages distinguish atomic replacement from storage synchronization: `rename(2)` covers atomic path replacement and destination preservation on failure, while `fsync(2)` requires a file flush and a directory flush when directory entries must reach disk.

Confidence: high for Linux and POSIX-like local filesystems; medium for network filesystems because `rename(2)` documents NFS caveats.

Recommendation: add a durable-write distinction:

- Atomic replacement comment: temp path, same filesystem or same directory, destination replacement policy, `EXDEV` or `--no-copy` behavior, and rollback.
- Durable write comment: file flush, directory-entry flush, crash model, filesystem caveat, and proof route.

Do not let a source comment claim "durable" when the script only does a temp write and `mv`.

### Finding 6: Stream-loop comments should protect stdout, stderr, delimiter, and subshell boundaries.

The Bash capsule already rejects collection loops documented as streams. The source-backed addition is that comments should state the boundary where Bash semantics affect output or mutation: pipeline subshell execution, `lastpipe` dependency, current-shell redirection, delimiter, backpressure, ordering, and finalization. A stream loop comment is justified when stdout remains machine data, stderr remains diagnostic, or the loop intentionally consumes a process stream without collecting it first.

Confidence: high.

Recommendation: keep the existing stream-loop rule and add `subshell/current-shell mutation` and `stdout/stderr separation` as required semantics when applicable.

### Finding 7: Retry comments need idempotence and terminal failure, not only backoff shape.

Bash has no generic retry contract. Bash sources provide primitives such as exit status, `wait`, `BASH_MONOSECONDS`, and `SRANDOM`, but the semantic retry policy is local. Comments should therefore document retryable status classes, idempotence, maximum attempts, capped delay, jitter source, state mutation, and terminal failure rail only where the public command's observable outcome depends on retry.

Confidence: high.

Recommendation: preserve the active retry comment list and add a rejection rule for comments that name exponential backoff without naming idempotence and terminal failure status.

### Finding 8: ShellCheck directives are source comments and need local invariants.

ShellCheck treats directives as comments with tool effect. Primary ShellCheck pages require directives to precede commands, use `source=` for static source resolution, and keep `external-sources=true` in `.shellcheckrc`. The active capsule is right to reject bare disables and trailing directive rationales.

Confidence: high.

Recommendation: add one ShellCheck-specific note only if later editors see recurring misuse: every directive comment names the diagnostic, the exact local invariant, and the narrower alternative checked first.

## Add Recommendations

- Add an `errexit` and `ERR` decision rule in the Bash capsule:
  - State ignored contexts when caller-visible.
  - Name `pipefail`, `errtrace`, and `inherit_errexit` only when the public surface relies on them.
  - Reject blanket strict-mode comments that imply total error handling.

- Add a compact trap and supervisor field list for source-comment review:
  - `Signal set`
  - `Forward target`
  - `Wait owner`
  - `Exit mapping`
  - `Reentrancy guard`
  - `Ignored-signal caveat`

- Add a durable-write distinction:
  - Atomic replacement: same-directory temp path, rename or `mv` path, replacement policy, cross-filesystem failure, rollback.
  - Crash durability: file flush, directory flush, filesystem or network caveat, proof route.

- Add stream-loop wording that names `stdout`, `stderr`, delimiter, ordering, finalization, and subshell or current-shell mutation when those affect callers.

- Add retry wording that requires idempotence and terminal failure status before backoff details.

## Remove Recommendations

- Remove any future wording that treats `set -e`, `ERR`, or `trap` as unconditional error handling.
- Remove any future durable-write example that calls a temp write plus `mv` "durable" without a flush or a proof gap.
- Remove comments that document shell syntax alone: `trap cleanup EXIT`, `mktemp -d`, `exec {fd}>file`, `while read`, or `sleep` backoff without a caller-visible semantic contract.
- Remove source-comment profiles that force the field names above into every Bash function. They are review criteria, not mandatory emitted labels.

## Change Recommendations

- Change the Bash capsule's trap sentence only narrowly if edited later:
  - current direction: trap and cleanup comments state signal, `BASH_TRAPSIG`, reentrancy guard, cleanup order, child forwarding, exit-status mapping, acquisition/release order, LIFO ownership, idempotence, temporary path, same-filesystem rename assumption, sensitive `umask`, durability choice, and rollback behavior.
  - proposed direction: keep that list, but split it into `failure rail`, `signal supervisor`, `cleanup ownership`, and `durable write` sub-concerns so readers do not treat every trap comment as needing every field.

- Change any future `strict mode` wording to say `set -Eeuo pipefail` and `inherit_errexit` establish a failure rail with documented exceptions, not a complete correctness proof.

- Change durable-write wording to say "atomic replacement" unless the script proves file and directory synchronization or explicitly states a manual durability gap.

## No-Change Confirmations

- Keep Bash comments conditional: public script, command, trap, cleanup, stream, retry, and environment contracts are documented only when caller-visible semantics are omitted by source.
- Keep stdout as machine data, stderr as diagnostics and logs, and exit status as the Bash failure channel.
- Keep nameref comments for structured outputs where command substitution would hide mutation or split data.
- Keep ShellCheck directives as documentation requiring local rationale.
- Keep active-standard routing: operational response belongs in runbooks, task steps belong in how-to guides, generated or curated command inventories belong in API or reference routes, and source comments own only source-local semantics.

## Source List

- GNU Bash Reference Manual 5.3: https://www.gnu.org/software/bash/manual/bash.html
- GNU Bash redirections: https://www.gnu.org/software/bash/manual/html_node/Redirections.html
- GNU Coreutils 9.11 `mktemp`: https://www.gnu.org/savannah-checkouts/gnu/coreutils/manual/html_node/mktemp-invocation.html
- GNU Coreutils 9.11 manual, `mv`: https://www.gnu.org/software/coreutils/manual/coreutils.html
- ShellCheck `SC1090`: https://www.shellcheck.net/wiki/SC1090
- ShellCheck `SC1091`: https://www.shellcheck.net/wiki/SC1091
- ShellCheck `SC1126`: https://www.shellcheck.net/wiki/SC1126
- ShellCheck `SC1144`: https://www.shellcheck.net/wiki/SC1144
- POSIX.1-2024 `rename()`: https://pubs.opengroup.org/onlinepubs/9799919799/functions/rename.html
- Linux man-pages 6.18 `rename(2)`: https://man7.org/linux/man-pages/man2/rename.2.html
- Linux man-pages 6.18 `fsync(2)`: https://man7.org/linux/man-pages/man2/fsync.2.html

## Validation

- Report file created only under `_reports/`.
- Active standards not edited by this worker.
- Current primary sources checked for Bash, Coreutils, ShellCheck, POSIX rename, Linux rename, and Linux fsync claims.
- No static, test, bridge, or generated-reference rails run because this is a research report only.
