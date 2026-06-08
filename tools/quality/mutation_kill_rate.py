"""Mutation kill-rate gate over a completed `mutmut run`.

mutmut owns no kill-rate threshold of its own, so this thin reader is the single justified
project-local script: it folds the persisted per-mutant exit codes into the standard mutation
score `killed / (killed + survived)` and exits non-zero when the score falls below a floor.

It binds mutmut's own result surface (`walk_mutatable_files` + `SourceFileMutationData` +
`status_by_exit_code` — the exact triple the built-in `mutmut results` command iterates), so the
status taxonomy can never drift from the tool. `no tests` / `suspicious` / `timeout` mutants are
reported but excluded from the denominator: the score measures test-suite kill power over the
mutants the suite actually exercised, matching the conventional mutation-score definition.

Run after a mutation pass from the same working directory the run produced its cache in:

    COVERAGE_RCFILE=.config/coverage-mutmut.ini PYO3_USE_ABI3_FORWARD_COMPATIBILITY=1 \
        .cache/mut-venv/bin/python -m mutmut run
    .cache/mut-venv/bin/python tools/quality/mutation_kill_rate.py --threshold 0.80

Current ceiling (Wave 4, accepted): the Rank-13 `relative_files=false` COVERAGE_RCFILE fix
unblocks "no covered mutants" — the runner enters the stats phase. The stats phase then aborts
with pytest exit 4 (startup/usage error, not a test failure) when mutmut-3 invokes pytest under
`change_cwd(mutants/)` on CPython 3.15.0b1. The identical pytest command run manually returns 0
(1035 passed). Root cause is a mutmut-3 + 3.15.0b1 interaction in the stats pass; this gate is
structurally complete but unexercisable until a newer mutmut-3 release or stable 3.15 resolves it.
"""

from __future__ import annotations

from collections import Counter
from functools import reduce
import sys

import cyclopts
from mutmut.__main__ import SourceFileMutationData, status_by_exit_code, walk_mutatable_files
from mutmut.configuration import Config


# --- [CONSTANTS] -----------------------------------------------------------

SCORED: frozenset[str] = frozenset({"killed", "survived"})


# --- [OPERATIONS] ----------------------------------------------------------


def _tally() -> Counter[str]:
    """Fold every persisted mutant's exit code into a status histogram via mutmut's own surface.

    Returns:
        A `Counter` keyed by mutmut status name (`killed`, `survived`, `no tests`, `timeout`,
        `suspicious`, `not checked`) over every mutant recorded in the run cache.
    """
    Config.ensure_loaded()
    loaded = ((p, SourceFileMutationData(path=p)) for p in walk_mutatable_files())
    return reduce(
        lambda acc, sd: acc + Counter(status_by_exit_code[code] for code in sd.exit_code_by_key.values()),
        (sd for _, sd in loaded if sd.load() is None),
        Counter[str](),
    )


def gate(threshold: float) -> int:
    """Print the one-line mutation-score verdict and return its process exit code.

    Args:
        threshold: Minimum acceptable mutation score `killed / (killed + survived)`.

    Returns:
        `0` when the score meets `threshold` over a non-empty scored population, `1` otherwise.
    """
    tally = _tally()
    killed, survived = tally["killed"], tally["survived"]
    scored = killed + survived
    score = killed / scored if scored else 0.0
    detail = " ".join(f"{status}={count}" for status, count in sorted(tally.items()))
    verdict = "PASS" if scored and score >= threshold else "FAIL"
    print(f"[{verdict}] mutation-score={score:.3f} (killed={killed}/{scored}) threshold={threshold:.3f}  {detail}")
    return 0 if verdict == "PASS" else 1


# --- [COMPOSITION] ---------------------------------------------------------

app = cyclopts.App(name="mutmut-kill-rate", help="Assert mutmut killed/(killed+survived) >= threshold.")


@app.default
def main(*, threshold: float = 0.80) -> None:
    """Exit non-zero when the persisted mutation score is below `threshold`."""
    sys.exit(gate(threshold))


if __name__ == "__main__":
    app()
