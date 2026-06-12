"""Mutation kill-rate gate over a completed `mutmut run` cache.

mutmut owns no kill-rate threshold of its own, so this `python -m` leaf folds the persisted
per-mutant exit codes into the standard mutation score `killed / (killed + survived)` by binding
mutmut's own result surface (`walk_mutatable_files` + `SourceFileMutationData` +
`status_by_exit_code` — the exact triple the built-in `mutmut results` command iterates), so the
status taxonomy can never drift from the tool. `no tests` / `timeout` / `suspicious` mutants are
reported on stderr but excluded from the denominator: the score measures kill power over the
mutants the suite actually exercised. stdout carries exactly one TestRun-shaped JSON line; the
process exits 1 when the score falls below the 0.80 floor. Run from the working directory that
owns the `mutants/` cache of the completed run.

stderr carries a second, structured JSON line: per-file survivor/no-tests/timeout/suspicious
clusters plus per-file duration totals and the unified diffs of the top-N survivors. The cluster
projection rides the SAME `SourceFileMutationData.load()` the score tally consumes (zero extra
I/O), so the wire stdout line is untouched while a triage agent gets every survivor it must
harden-or-delete in one read.
"""

from collections import Counter
import contextlib
from functools import reduce
import io
import sys
from typing import TYPE_CHECKING

import msgspec
from mutmut.__main__ import get_diff_for_mutant, status_by_exit_code, walk_mutatable_files
from mutmut.mutation.data import SourceFileMutationData

from tools.assay.core.model import MutationLane, TestRun


if TYPE_CHECKING:
    from collections.abc import Iterable
    from pathlib import Path


# --- [CONSTANTS] ------------------------------------------------------------------------

_FLOOR: float = 0.80
_TOP_SURVIVORS: int = 10

# --- [MODELS] ---------------------------------------------------------------------------


class FileCluster(msgspec.Struct, frozen=True):
    """One source file's mutant outcome clusters, keyed by mutant name within each status bucket.

    ``survivors`` are the harden-or-delete population; ``no_tests``/``timeouts``/``suspicious`` carry
    the unscored mutants for triage. ``duration_s`` is the file's total measured test time across all
    mutants (from ``durations_by_key``) so a triage pass can rank files by mutation wall-clock.
    """

    path: str
    survivors: tuple[str, ...] = ()
    no_tests: tuple[str, ...] = ()
    timeouts: tuple[str, ...] = ()
    suspicious: tuple[str, ...] = ()
    duration_s: float = 0.0


class SurvivorReport(msgspec.Struct, frozen=True):
    """The structured stderr companion to the stdout ``TestRun`` line.

    ``clusters`` carries one ``FileCluster`` per source file with at least one survivor or unscored
    mutant; ``diffs`` maps the top-N survivor names to their unified diff so a triage agent reads the
    exact code change every surviving mutant exploits without a second mutmut invocation.
    """

    clusters: tuple[FileCluster, ...] = ()
    diffs: tuple[tuple[str, str], ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _loaded(path: Path) -> SourceFileMutationData:
    data = SourceFileMutationData(path=path)
    data.load()
    return data


def _scan() -> tuple[SourceFileMutationData, ...]:
    """Load every mutatable file's persisted meta once; the single I/O pass the tally and clusters share.

    Returns:
        The loaded ``SourceFileMutationData`` for every file ``walk_mutatable_files`` yields.
    """
    return tuple(map(_loaded, walk_mutatable_files()))


def _cluster(data: SourceFileMutationData) -> FileCluster:
    """Project one loaded file's exit-code map into status buckets plus its total measured duration.

    Returns:
        FileCluster grouping the file's mutant names by survivor / no-tests / timeout / suspicious status.
    """
    by_status: dict[str, tuple[str, ...]] = reduce(
        lambda acc, item: {**acc, status_by_exit_code[item[1]]: (*acc.get(status_by_exit_code[item[1]], ()), item[0])},
        data.exit_code_by_key.items(),
        {},
    )
    return FileCluster(
        path=str(data.path),
        survivors=by_status.get("survived", ()),
        no_tests=by_status.get("no tests", ()),
        timeouts=by_status.get("timeout", ()),
        suspicious=by_status.get("suspicious", ()),
        duration_s=sum(data.durations_by_key.values()),
    )


def _diff(name: str) -> str:
    """Best-effort unified diff for a survivor; the mutmut header print is captured off stdout.

    ``get_diff_for_mutant`` prints a ``# <name>: <status>`` header to stdout — captured into a throwaway
    buffer so the one-line stdout wire contract holds — and re-reads the mutant source, which can fail for
    a synthesized or pruned cache. A failure yields an empty diff rather than aborting the whole gate.

    Returns:
        The unified diff for the named mutant, or an empty string when the source re-read fails.
    """
    # The owning boundary that converts mutmut's source re-read fallibility into an empty-diff fact;
    # redirect_stdout keeps the unconditional `# <name>: <status>` header print off the wire stdout line.
    with contextlib.suppress(Exception), contextlib.redirect_stdout(io.StringIO()):  # noqa: TID251  # best-effort diff boundary adapter
        return get_diff_for_mutant(name)
    return ""


def _tally(scanned: Iterable[SourceFileMutationData] | None = None) -> Counter[str]:
    """Fold the persisted per-mutant exit codes into a status-name Counter (mutmut's own taxonomy).

    Args:
        scanned: Pre-loaded file data to fold; ``None`` loads the cache itself (the standalone contract).

    Returns:
        Counter mapping each ``status_by_exit_code`` name to its mutant count; ``.total()`` is the
        selected population and ``["killed"]``/``["survived"]`` drive the score.
    """
    return reduce(
        lambda acc, data: acc + Counter(status_by_exit_code[code] for code in data.exit_code_by_key.values()),
        scanned if scanned is not None else _scan(),
        Counter[str](),
    )


def _survivors(scanned: Iterable[SourceFileMutationData]) -> SurvivorReport:
    """Project the scanned cache into the per-file cluster map plus top-N survivor diffs.

    Returns:
        SurvivorReport carrying one FileCluster per file with at least one survivor or unscored mutant,
        and the unified diffs of the first ``_TOP_SURVIVORS`` survivors across all files.
    """
    clusters = tuple(
        cluster for cluster in map(_cluster, scanned) if (cluster.survivors or cluster.no_tests or cluster.timeouts or cluster.suspicious)
    )
    top = tuple(name for cluster in clusters for name in cluster.survivors)[:_TOP_SURVIVORS]
    return SurvivorReport(clusters=clusters, diffs=tuple((name, _diff(name)) for name in top))


def gate(floor: float = _FLOOR) -> int:
    """Emit one TestRun-shaped JSON line for the persisted mutation score and return the exit code.

    Args:
        floor: Minimum acceptable mutation score ``killed / (killed + survived)``.

    Returns:
        ``0`` when the score meets ``floor`` over a non-empty scored population, ``1`` otherwise.
    """
    scanned = _scan()
    tally = _tally(scanned)
    killed, survived = tally["killed"], tally["survived"]
    scored = killed + survived
    score = killed / scored if scored else 0.0
    verdict = bool(scored) and score >= floor
    detail = " ".join(f"{status.replace(' ', '_')}={count}" for status, count in sorted(tally.items()))
    report = _survivors(scanned)
    sys.stderr.write(f"[{'PASS' if verdict else 'FAIL'}] mutation-score={score:.3f} floor={floor:.3f} {detail}\n")
    sys.stderr.write(msgspec.json.encode(report).decode() + "\n")
    line = msgspec.json.encode(TestRun(mutation=MutationLane.FULL, killed=killed, survived=survived, selected=tally.total()))
    sys.stdout.write(line.decode() + "\n")
    return 0 if verdict else 1


# --- [EXPORTS] --------------------------------------------------------------------------

# FileCluster / SurvivorReport are leaf-internal stderr projections, not part of the wire contract the
# law-coverage gate walks; only `gate` (the `python -m` entrypoint) is the public surface.
__all__ = ["gate"]

# --- [ENTRY] ----------------------------------------------------------------------------

if __name__ == "__main__":
    raise SystemExit(gate())
