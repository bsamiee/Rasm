"""Laws for the local provisioning rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

from expression import Ok

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_ok
from tools.assay.core.model import Claim, Fault, receipt
from tools.assay.core.status import RailStatus
from tools.assay.rails import provision as provision_rail
from tools.assay.rails.provision import ProvisionParams


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


# --- [PROVISION_PARAMS]


def test_provisionparams_arity_is_zero() -> None:
    """Provision commands do not accept positional tokens."""
    assert ProvisionParams()._arity("verify") == 0
    surplus = ProvisionParams(paths=("extra",)).bound("verify")
    assert isinstance(surplus, Fault)
    assert surplus.status is RailStatus.FAULTED


register_law(ProvisionParams, "arity_is_zero")


def test_provision_up_delegates_to_stack_up(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The up verb starts the Forge-owned provisioning services via one command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.up(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert report.claim is Claim.PROVISION
    assert report.verb == "up"
    assert calls == [((("rasm-provision", "up")),)]


register_law(provision_rail.up, "delegates_to_stack_up")


def test_provision_down_delegates_to_stack_down(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The down verb stops the labelled provisioning services via one command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.down(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert report.claim is Claim.PROVISION
    assert report.verb == "down"
    assert calls == [((("rasm-provision", "down")),)]


register_law(provision_rail.down, "delegates_to_stack_down")


def test_provision_status_delegates_to_stack_status(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The status verb delegates to the Forge-owned provisioning command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert report.claim is Claim.PROVISION
    assert report.verb == "status"
    assert calls == [((("rasm-provision", "status")),)]


register_law(provision_rail.status, "delegates_to_stack_status")


def test_provision_doctor_delegates_to_stack_doctor(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The doctor verb delegates to the Forge-owned diagnostic command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.doctor(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert report.claim is Claim.PROVISION
    assert report.verb == "doctor"
    assert calls == [((("rasm-provision", "doctor")),)]


register_law(provision_rail.doctor, "delegates_to_stack_doctor")


def test_provision_ports_delegates_to_stack_ports(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The ports verb delegates to the Forge-owned port diagnostic command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.ports(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert report.claim is Claim.PROVISION
    assert report.verb == "ports"
    assert calls == [((("rasm-provision", "ports")),)]


register_law(provision_rail.ports, "delegates_to_stack_ports")


def test_provision_plan_delegates_to_stack_plan(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The plan verb delegates to the Forge-owned read-only compose renderer."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.plan(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert report.claim is Claim.PROVISION
    assert report.verb == "plan"
    assert calls == [((("rasm-provision", "plan")),)]


register_law(provision_rail.plan, "delegates_to_stack_plan")


def test_provision_env_delegates_to_stack_env(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The env verb delegates to the Forge-owned stack command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.env(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert report.claim is Claim.PROVISION
    assert report.verb == "env"
    assert calls == [((("rasm-provision", "env")),)]


register_law(provision_rail.env, "delegates_to_stack_env")


def test_provision_verify_folds_stack_and_local_probes(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The verify verb runs stack extension proof plus local runtime probes."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.verify(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    commands = calls[0]
    assert report.claim is Claim.PROVISION
    assert report.verb == "verify"
    assert ("rasm-provision", "verify") in commands
    assert ("duckdb", "--version") in commands
    assert any(command[:2] == ("forge-scientific-env", "python3") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "pkg-config", "--modversion") for command in commands)


register_law(provision_rail.verify, "folds_stack_and_local_probes")
