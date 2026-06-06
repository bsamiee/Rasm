"""Registry: _emit one-Envelope guard, _distill Diagnostic, delta algebra, self_test bypass, parse_fault Envelope."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from typing import TYPE_CHECKING

import msgspec

from tools.assay.composition.registry import _delta_report, _load_run, _prior, _retain, _run_ids  # noqa: PLC2701
from tools.assay.core.model import (  # noqa: TC001 — Envelope in runtime return annotations (no PEP 563)
    Claim,
    Envelope,
    envelope,
    fold,
    receipt,
    RunDelta,
)
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tools.assay.composition.settings import ArtifactStore


# --- [CONSTANTS] -----------------------------------------------------------------------

_WIRE = msgspec.json.Encoder(order="deterministic")


# --- [OPERATIONS] -------------------------------------------------------------------------


def _make_envelope(run_id: str, claim: Claim = Claim.STATIC) -> Envelope:
    """Build a deterministic OK Envelope for a given run_id.

    Returns:
        Persistable history Envelope for registry retention tests.
    """
    report = fold(claim, "check", (receipt(("tool",), 0, status=RailStatus.OK),))
    return msgspec.structs.replace(envelope(report, claim=claim, verb="check"), run_id=run_id)


def _pipe(store: ArtifactStore, run_ids: tuple[str, ...]) -> None:
    """Write canned Envelope bytes for each run_id into the mem_store history tree."""
    for run_id in run_ids:
        directory = store.ensure("history", run_id)
        store.fs.pipe_file(f"{directory}/envelope.json", _WIRE.encode(_make_envelope(run_id)))


# --- [PRIOR_ORACLE] --------------------------------------------------------------------


def test_prior_lexicographic_oracle() -> None:
    """``_prior`` returns the largest run_id strictly less than the given id — ISO-timestamp order is chronological."""
    ids = ("2026-01-01T00-00-00", "2026-01-02T00-00-00", "2026-01-03T00-00-00")
    assert _prior(ids, "2026-01-03T00-00-00") == "2026-01-02T00-00-00"
    assert _prior(ids, "2026-01-02T00-00-00") == "2026-01-01T00-00-00"
    assert not _prior(ids, "2026-01-01T00-00-00")
    assert not _prior((), "anything")


# --- [LOAD_RUN_ORACLE] -----------------------------------------------------------------


def test_load_run_corrupt_returns_none(mem_store: ArtifactStore) -> None:
    """``_load_run`` on corrupt bytes folds to ``None`` — a crashing prior run never FAULTs delta."""
    directory = mem_store.ensure("history", "corrupt-run")
    mem_store.fs.pipe_file(f"{directory}/envelope.json", b"{not-valid-json}")
    assert _load_run(mem_store, "corrupt-run") is None


def test_load_run_empty_id_returns_none(mem_store: ArtifactStore) -> None:
    """``_load_run(store, "")`` folds to ``None`` — the no-prior sentinel passes through cleanly."""
    assert _load_run(mem_store, "") is None


def test_load_run_absent_returns_none(mem_store: ArtifactStore) -> None:
    """``_load_run`` on a run_id with no persisted file folds to ``None``."""
    assert _load_run(mem_store, "nonexistent-run-id") is None


# --- [PERSIST_RETAIN_RELOAD_DELTA] -----------------------------------------------
# E11: the full persist->retain(prune)->reload->delta round-trip over mem_store.


def _assert_retain_prune(mem_store: ArtifactStore, run_a: str, run_b: str, run_c: str) -> None:
    """Assert retain prunes run_a and keeps run_b and run_c."""
    _retain(mem_store, keep=2)
    surviving = _run_ids(mem_store)
    assert run_a not in surviving, f"oldest run {run_a!r} should have been pruned"
    assert run_b in surviving
    assert run_c in surviving
    assert len(surviving) == 2


def _assert_reload_roundtrip(mem_store: ArtifactStore, run_b: str, run_c: str) -> tuple[Envelope, Envelope]:
    """Reload survivors and assert run_ids survive byte-identically.

    Returns:
        Reloaded survivor envelopes in input order.
    """
    loaded_b = _load_run(mem_store, run_b)
    loaded_c = _load_run(mem_store, run_c)
    assert loaded_b is not None, "run_b should survive retention"
    assert loaded_c is not None, "run_c should survive retention"
    assert loaded_b.run_id == run_b
    assert loaded_c.run_id == run_c
    return loaded_b, loaded_c


def _assert_delta_zero_drift(mem_store: ArtifactStore, run_b: str, run_c: str, loaded_b: Envelope, loaded_c: Envelope) -> None:
    """Assert delta between two OK-zero-result runs is 0 added, 0 removed."""
    surviving = _run_ids(mem_store)
    assert _prior(surviving, run_c) == run_b
    delta = _delta_report(run_b, run_c, loaded_b, loaded_c)
    assert delta.status is RailStatus.OK
    assert isinstance(delta.detail, RunDelta)
    assert delta.detail.added == 0
    assert delta.detail.removed == 0
    assert delta.detail.before.id == run_b
    assert delta.detail.after.id == run_c


def test_persist_retain_reload_delta_round_trip(mem_store: ArtifactStore) -> None:
    """E11: persist->retain(prune)->reload->delta law — the full registry history pipeline over mem_store.

    Persists 3 runs into ``mem_store``, retains only the 2 newest (pruning the oldest), reloads
    both survivors, and asserts ``_delta_report`` computes the symmetric-difference drift correctly.
    """
    run_a = "2026-01-01T00-00-00.000000-9001"
    run_b = "2026-01-02T00-00-00.000000-9001"
    run_c = "2026-01-03T00-00-00.000000-9001"
    _pipe(mem_store, (run_a, run_b, run_c))
    _assert_retain_prune(mem_store, run_a, run_b, run_c)
    loaded_b, loaded_c = _assert_reload_roundtrip(mem_store, run_b, run_c)
    _assert_delta_zero_drift(mem_store, run_b, run_c, loaded_b, loaded_c)


def test_delta_report_missing_after_folds_to_empty() -> None:
    """``_delta_report`` with ``after=None`` folds to EMPTY — a missing run is safe (pure function, no store)."""
    report = _delta_report("run-b", "run-c", _make_envelope("run-b"), None)
    assert report.status is RailStatus.EMPTY


def test_delta_report_symmetric_difference_oracle() -> None:
    """``_delta_report`` added/removed counts equal the symmetric-difference of (id, line) result keys (pure)."""
    before_env = _make_envelope("run-x")
    after_report = fold(Claim.STATIC, "check", (receipt(("tool",), 1, status=RailStatus.FAILED),))
    after_env = msgspec.structs.replace(envelope(after_report, claim=Claim.STATIC, verb="check"), run_id="run-y")
    delta = _delta_report("run-x", "run-y", before_env, after_env)
    assert isinstance(delta.detail, RunDelta)
    assert delta.detail.added == 1
    assert delta.detail.removed == 0
