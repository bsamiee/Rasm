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
"""

from collections import Counter
from functools import reduce
import sys
from typing import TYPE_CHECKING

import msgspec
from mutmut.__main__ import status_by_exit_code, walk_mutatable_files
from mutmut.mutation.data import SourceFileMutationData

from tools.assay.core.model import MutationLane, TestRun


if TYPE_CHECKING:
    from pathlib import Path


# --- [CONSTANTS] ------------------------------------------------------------------------

_FLOOR: float = 0.80


# --- [OPERATIONS] -----------------------------------------------------------------------


def _tally() -> Counter[str]:
    def _loaded(path: Path) -> SourceFileMutationData:
        data = SourceFileMutationData(path=path)
        data.load()
        return data

    return reduce(
        lambda acc, data: acc + Counter(status_by_exit_code[code] for code in data.exit_code_by_key.values()),
        map(_loaded, walk_mutatable_files()),
        Counter[str](),
    )


def gate(floor: float = _FLOOR) -> int:
    """Emit one TestRun-shaped JSON line for the persisted mutation score and return the exit code.

    Args:
        floor: Minimum acceptable mutation score ``killed / (killed + survived)``.

    Returns:
        ``0`` when the score meets ``floor`` over a non-empty scored population, ``1`` otherwise.
    """
    tally = _tally()
    killed, survived = tally["killed"], tally["survived"]
    scored = killed + survived
    score = killed / scored if scored else 0.0
    verdict = bool(scored) and score >= floor
    detail = " ".join(f"{status.replace(' ', '_')}={count}" for status, count in sorted(tally.items()))
    sys.stderr.write(f"[{'PASS' if verdict else 'FAIL'}] mutation-score={score:.3f} floor={floor:.3f} {detail}\n")
    line = msgspec.json.encode(TestRun(mutation=MutationLane.FULL, killed=killed, survived=survived, selected=tally.total()))
    sys.stdout.write(line.decode() + "\n")
    return 0 if verdict else 1


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["gate"]


# --- [ENTRY] ----------------------------------------------------------------------------

if __name__ == "__main__":
    raise SystemExit(gate())
