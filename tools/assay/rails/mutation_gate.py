"""Gate a completed mutmut cache with one stdout score line and stderr survivor triage.

The score uses mutmut's own persisted status taxonomy. Only killed and survived mutants enter the
denominator; no-tests, timeout, and suspicious mutants stay on the structured stderr triage channel.
"""

from collections import Counter
import contextlib
from functools import reduce
import io
from itertools import groupby
import json
from operator import itemgetter
import sys
from typing import Literal, TYPE_CHECKING

from expression import Error, Ok
import msgspec
from mutmut.__main__ import get_diff_for_mutant, status_by_exit_code, walk_mutatable_files
from mutmut.mutation.data import SourceFileMutationData

from tools.assay.core.model import MutationLane, TestRun


if TYPE_CHECKING:
    from collections.abc import Iterable
    from pathlib import Path

    from expression import Result


# --- [TYPES] ----------------------------------------------------------------------------

# mutmut SourceFileMutationData.load() can fail three ways on a stale cache: corrupt JSON, version-skewed schema
# (a missing required key or unexpected extra keys), or an I/O fault; the cause selects the regenerate guidance.
type _LoadCause = Literal["corrupt", "skewed", "io"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FLOOR: float = 0.80
_TOP_SURVIVORS: int = 10

# --- [MODELS] ---------------------------------------------------------------------------


class _LoadFault(msgspec.Struct, frozen=True):
    """An unloadable mutmut meta file with the cause that excluded it from scoring."""

    path: str
    cause: _LoadCause
    detail: str


class FileCluster(msgspec.Struct, frozen=True):
    """Per-file survivor and unscored mutation buckets for stderr triage."""

    path: str
    survivors: tuple[str, ...] = ()
    no_tests: tuple[str, ...] = ()
    timeouts: tuple[str, ...] = ()
    suspicious: tuple[str, ...] = ()
    duration_s: float = 0.0


class SurvivorReport(msgspec.Struct, frozen=True):
    """Structured stderr companion to the stdout ``TestRun`` score line."""

    clusters: tuple[FileCluster, ...] = ()
    diffs: tuple[tuple[str, str], ...] = ()
    unloadable: tuple[_LoadFault, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _loaded(path: Path) -> Result[SourceFileMutationData, _LoadFault]:
    # Boundary over mutmut's load(): corrupt JSON, a version-skewed schema (missing key / unexpected keys), or an I/O
    # fault converts to a typed rail so one stale meta file is excluded from scoring instead of crashing the gate.
    data = SourceFileMutationData(path=path)
    try:
        data.load()
    except json.JSONDecodeError as exc:
        return Error(_LoadFault(path=str(path), cause="corrupt", detail=str(exc)[:256]))
    except (KeyError, AssertionError) as exc:
        return Error(_LoadFault(path=str(path), cause="skewed", detail=str(exc)[:256]))
    except OSError as exc:
        return Error(_LoadFault(path=str(path), cause="io", detail=str(exc)[:256]))
    return Ok(data)


def _scan() -> tuple[tuple[SourceFileMutationData, ...], tuple[_LoadFault, ...]]:
    """Load persisted mutmut metadata once, partitioning loadable records from unloadable meta files.

    Returns:
        Loaded source-file mutation records and the typed faults for any stale/corrupt meta file.
    """
    railed = tuple(map(_loaded, walk_mutatable_files()))
    loaded = tuple(r.ok for r in railed if r.is_ok())
    faulted = tuple(r.error for r in railed if r.is_error())
    return loaded, faulted


def _cluster(data: SourceFileMutationData) -> FileCluster:
    """Project one loaded file into status buckets and total mutation duration.

    Returns:
        Cluster carrying survivor and unscored mutant names.
    """
    keyed = sorted(((status_by_exit_code[code], key) for key, code in data.exit_code_by_key.items()), key=itemgetter(0))
    by_status: dict[str, tuple[str, ...]] = {status: tuple(key for _, key in group) for status, group in groupby(keyed, key=itemgetter(0))}
    return FileCluster(
        path=str(data.path),
        survivors=by_status.get("survived", ()),
        no_tests=by_status.get("no tests", ()),
        timeouts=by_status.get("timeout", ()),
        suspicious=by_status.get("suspicious", ()),
        duration_s=sum(data.durations_by_key.values()),
    )


def _diff(name: str) -> str:
    """Return a best-effort survivor diff without contaminating stdout.

    Returns:
        Unified diff text, or an empty string when mutmut cannot re-read the mutant.
    """
    # Redirected stdout preserves the one-line TestRun wire contract.
    with contextlib.suppress(Exception), contextlib.redirect_stdout(io.StringIO()):  # noqa: TID251  # best-effort diff boundary adapter
        return get_diff_for_mutant(name)
    return ""


def _tally(scanned: Iterable[SourceFileMutationData] | None = None) -> Counter[str]:
    """Fold persisted exit codes into mutmut's status taxonomy.

    Args:
        scanned: Pre-loaded file data to fold; ``None`` loads the cache itself (the standalone contract).

    Returns:
        Status-name counts; killed and survived counts drive the score denominator.
    """
    return reduce(
        lambda acc, data: acc + Counter(status_by_exit_code[code] for code in data.exit_code_by_key.values()),
        scanned if scanned is not None else _scan()[0],
        Counter[str](),
    )


def _survivors(scanned: Iterable[SourceFileMutationData], faulted: tuple[_LoadFault, ...] = ()) -> SurvivorReport:
    """Project scanned metadata into per-file triage clusters, top survivor diffs, and unloadable meta faults.

    Returns:
        Structured survivor report for stderr.
    """
    clusters = tuple(
        cluster for cluster in map(_cluster, scanned) if (cluster.survivors or cluster.no_tests or cluster.timeouts or cluster.suspicious)
    )
    top = tuple(name for cluster in clusters for name in cluster.survivors)[:_TOP_SURVIVORS]
    return SurvivorReport(clusters=clusters, diffs=tuple((name, _diff(name)) for name in top), unloadable=faulted)


def gate(floor: float = _FLOOR) -> int:
    """Emit the persisted mutation score and return the process exit code.

    Stale, corrupt, or version-skewed meta files are excluded from the score and surfaced on the stderr triage
    channel; the gate fails when the scored population is empty or below the floor, never on an unloadable cache.

    Args:
        floor: Minimum acceptable mutation score ``killed / (killed + survived)``.

    Returns:
        ``0`` when the scored population is non-empty and meets ``floor``; ``1`` otherwise.
    """
    scanned, faulted = _scan()
    tally = _tally(scanned)
    killed, survived = tally["killed"], tally["survived"]
    scored = killed + survived
    score = killed / scored if scored else 0.0
    verdict = bool(scored) and score >= floor
    detail = " ".join(f"{status.replace(' ', '_')}={count}" for status, count in sorted(tally.items()))
    sys.stderr.write(
        f"[{'PASS' if verdict else 'FAIL'}] mutation-score={score:.3f} floor={floor:.3f} {detail}{f' unloadable={len(faulted)}' if faulted else ''}\n"
    )
    sys.stderr.write(msgspec.json.encode(_survivors(scanned, faulted)).decode() + "\n")
    run = TestRun(mutation=MutationLane.FULL, killed=killed, survived=survived, selected=tally.total())
    sys.stdout.write(msgspec.json.encode(run).decode() + "\n")
    return 0 if verdict else 1


# --- [EXPORTS] --------------------------------------------------------------------------

# Only the python -m entrypoint is exported; stderr projections stay leaf-internal.
__all__ = ["gate"]

# --- [ENTRY] ----------------------------------------------------------------------------

if __name__ == "__main__":
    raise SystemExit(gate())
