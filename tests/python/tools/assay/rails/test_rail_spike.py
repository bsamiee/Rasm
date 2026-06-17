"""Laws for the spike provisioning rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

from expression import Ok

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_ok
from tools.assay.core.model import Claim, Fault, receipt
from tools.assay.core.status import RailStatus
from tools.assay.rails import spike as spike_rail
from tools.assay.rails.spike import SpikeParams


if TYPE_CHECKING:
    from expression import Result
    import pytest

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.model import Check, Completed

# --- [OPERATIONS] -----------------------------------------------------------------------


def _recording_fan(calls: list[tuple[tuple[str, ...], ...]]) -> object:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append(tuple(check.tool.command for check in checks))
        return tuple(Ok(receipt(check.tool.command, 0, status=RailStatus.OK)) for check in checks)

    return fan


# --- [SPIKE_PARAMS]


def test_spikeparams_arity_is_zero() -> None:
    """Spike commands do not accept positional tokens."""
    assert SpikeParams()._arity("verify") == 0
    surplus = SpikeParams(paths=("extra",)).bound("verify")
    assert isinstance(surplus, Fault)
    assert surplus.status is RailStatus.FAULTED


register_law(SpikeParams, "arity_is_zero")


def test_spike_env_delegates_to_stack_env(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The env verb delegates to the Forge-owned stack command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(spike_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(spike_rail.env(assay_root.settings, assay_root.scope(Claim.SPIKE), SpikeParams()))
    assert report.claim is Claim.SPIKE
    assert report.verb == "env"
    assert calls == [((("rasm-spike-stack", "env")),)]


register_law(spike_rail.env, "delegates_to_stack_env")


def test_spike_verify_folds_stack_and_local_probes(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The verify verb runs stack extension proof plus local runtime probes."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(spike_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(spike_rail.verify(assay_root.settings, assay_root.scope(Claim.SPIKE), SpikeParams()))
    commands = calls[0]
    assert report.claim is Claim.SPIKE
    assert report.verb == "verify"
    assert ("rasm-spike-stack", "verify-extensions") in commands
    assert ("duckdb", "--version") in commands
    assert any(command[:2] == ("forge-scientific-env", "python3") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "pkg-config", "--modversion") for command in commands)


register_law(spike_rail.verify, "folds_stack_and_local_probes")
