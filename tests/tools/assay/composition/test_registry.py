"""Registry history, retention, delta, and truncation laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from dataclasses import dataclass
from typing import TYPE_CHECKING

from cyclopts import App
import msgspec
import pytest

from tests.tools.assay.conftest import make_history_envelope, pipe_history
from tools.assay.composition import registry as registry_mod
from tools.assay.composition.registry import (
    _delta_report,  # noqa: PLC2701
    _ok_envelope,  # noqa: PLC2701
    _ORPHAN_MIN_AGE_S,  # noqa: PLC2701
    _prior,  # noqa: PLC2701
    _process_hygiene,  # noqa: PLC2701
    build_app,
    REGISTRY,
)
from tools.assay.core.model import _RESULT_CAP, ArtifactKind, Claim, envelope, fold, Match, receipt, Report, RunDelta  # noqa: PLC2701
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.composition.settings import ArtifactStore


@dataclass(frozen=True, slots=True)
class _Proc:
    info: dict[str, object]


# --- [PRIOR_ORACLE] --------------------------------------------------------------------


@pytest.mark.mutation
def test_prior_lexicographic_oracle() -> None:
    """``_prior`` returns the largest run_id strictly less than the given id — ISO-timestamp order is chronological."""
    ids = ("2026-01-01T00-00-00", "2026-01-02T00-00-00", "2026-01-03T00-00-00")
    assert _prior(ids, "2026-01-03T00-00-00") == "2026-01-02T00-00-00"
    assert _prior(ids, "2026-01-02T00-00-00") == "2026-01-01T00-00-00"
    assert not _prior(ids, "2026-01-01T00-00-00")
    assert not _prior((), "anything")


# --- [REGISTRY_SHAPE] ------------------------------------------------------------------


def test_registry_unique_claim_verb_pairs() -> None:
    """Each ``(claim, verb)`` binding is unique — the Cyclopts sub-app/command tree has no colliding leaf."""
    pairs = [(b.claim, b.verb) for b in REGISTRY]
    assert len(pairs) == len(set(pairs))


def test_build_app_reaches_every_registry_leaf() -> None:
    """``build_app(REGISTRY)`` returns a Cyclopts ``App`` whose every ``(claim, verb)`` leaf is reachable.

    Each row weaves into a sub-app keyed by ``claim.value`` carrying a command named ``verb``; reachability
    is membership of ``verb`` in ``app[claim.value]``.
    """
    app = build_app(REGISTRY)
    assert isinstance(app, App)
    assert all(b.verb in app[b.claim.value] for b in REGISTRY)


def test_process_hygiene_reports_old_repo_orphans(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Self-test hygiene reports orphaned repo Python/UV/type-server processes without flagging active children."""
    root = str(assay_root.root)
    rows = (
        _Proc({"pid": 123, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 456, "ppid": 99, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": root}),
        _Proc({"pid": 789, "ppid": 1, "create_time": 100.0, "name": "python3", "cmdline": ("python3", "-"), "cwd": str(assay_root.root.parent)}),
    )

    monkeypatch.setattr(registry_mod.__dict__["psutil"], "process_iter", lambda _attrs: rows)
    monkeypatch.setattr(registry_mod.__dict__["time"], "time", lambda: 100.0 + _ORPHAN_MIN_AGE_S + 1.0)
    matches, notes = _process_hygiene(assay_root.settings)

    assert len(matches) == 1
    assert matches[0].severity == "failed"
    assert "pid=123" in matches[0].text
    assert all("pid=456" not in note and "pid=789" not in note for note in notes)


# --- [LOAD_RUN_ORACLE] -----------------------------------------------------------------


def test_load_history_corrupt_returns_none(mem_store: ArtifactStore) -> None:
    """``load_history`` on corrupt bytes folds to ``None`` — a crashing prior run never FAULTs delta."""
    mem_store.write_bytes(b"{not-valid-json}", "history", "corrupt-run", "envelope.json")
    assert mem_store.load_history("corrupt-run") is None


def test_load_history_empty_id_returns_none(mem_store: ArtifactStore) -> None:
    """``load_history("")`` folds to ``None`` — the no-prior sentinel passes through cleanly."""
    assert mem_store.load_history("") is None


def test_load_history_absent_returns_none(mem_store: ArtifactStore) -> None:
    """``load_history`` on a run_id with no persisted file folds to ``None``."""
    assert mem_store.load_history("nonexistent-run-id") is None


def test_persist_retain_reload_delta_round_trip(mem_store: ArtifactStore) -> None:
    """E11: persist->retain(prune)->reload->delta law — the full registry history pipeline over mem_store.

    Persists 3 runs into ``mem_store``, retains only the 2 newest (pruning the oldest), reloads
    both survivors, and asserts ``_delta_report`` computes the symmetric-difference drift correctly.
    """
    run_a = "2026-01-01T00-00-00.000000-9001"
    run_b = "2026-01-02T00-00-00.000000-9001"
    run_c = "2026-01-03T00-00-00.000000-9001"
    pipe_history(mem_store, (run_a, run_b, run_c))

    mem_store.retain_history(2)
    surviving = mem_store.sorted_history_ids()
    assert run_a not in surviving, f"oldest run {run_a!r} should have been pruned"
    assert run_b in surviving
    assert run_c in surviving
    assert len(surviving) == 2

    loaded_b = mem_store.load_history(run_b)
    loaded_c = mem_store.load_history(run_c)
    assert loaded_b is not None, "run_b should survive retention"
    assert loaded_c is not None, "run_c should survive retention"
    assert loaded_b.run_id == run_b
    assert loaded_c.run_id == run_c
    assert _prior(surviving, run_c) == run_b

    delta = _delta_report(run_b, run_c, loaded_b, loaded_c)
    assert delta.status is RailStatus.OK
    assert isinstance(delta.detail, RunDelta)
    assert delta.detail.added == 0
    assert delta.detail.removed == 0
    assert delta.detail.before.id == run_b
    assert delta.detail.after.id == run_c


def test_delta_report_missing_after_folds_to_empty() -> None:
    """``_delta_report`` with ``after=None`` folds to EMPTY — a missing run is safe (pure function, no store)."""
    report = _delta_report("run-b", "run-c", make_history_envelope("run-b"), None)
    assert report.status is RailStatus.EMPTY


def test_delta_report_symmetric_difference_oracle() -> None:
    """``_delta_report`` added/removed counts equal the symmetric-difference of (id, line) result keys (pure)."""
    before_env = make_history_envelope("run-x")
    after_report = fold(Claim.STATIC, "check", (receipt(("tool",), 1, status=RailStatus.FAILED),))
    after_env = msgspec.structs.replace(envelope(after_report, claim=Claim.STATIC, verb="check"), run_id="run-y")
    delta = _delta_report("run-x", "run-y", before_env, after_env)
    assert isinstance(delta.detail, RunDelta)
    assert delta.detail.added == 1
    assert delta.detail.removed == 0


def test_ok_envelope_truncation_persists_full_report(assay_root: AssayHarness) -> None:
    """Large reports keep compact stdout while a full report artifact preserves every row."""
    rows = tuple(Match(id=f"row-{i}", kind=ArtifactKind.PROCESS, text="x") for i in range(_RESULT_CAP + 1))
    report = Report(Claim.STATIC, "fix", RailStatus.OK, results=rows)
    env = _ok_envelope(REGISTRY[0], assay_root.settings, 1.0, report)

    assert env.truncated
    assert env.report is not None
    assert len(env.report.results) == _RESULT_CAP
    assert env.report.artifacts[0].id == "full-report"

    raw = assay_root.settings.store().read_path(env.report.artifacts[0].path)
    decoded = msgspec.json.decode(raw, type=Report)
    assert len(decoded.results) == _RESULT_CAP + 1
