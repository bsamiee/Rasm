"""Laws for the local provisioning rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

from expression import Ok
import msgspec
import pytest

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error_status, assert_ok
from tools.assay.core.model import Claim, Fault, ProvisionRun, receipt
from tools.assay.core.status import RailStatus
from tools.assay.rails import provision as provision_rail
from tools.assay.rails.provision import ProvisionParams


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.composition.settings import ArtifactScope, AssaySettings
    from tools.assay.core.model import Check, Completed, Report

# --- [OPERATIONS] -----------------------------------------------------------------------


def _json(command: str, **extra: object) -> bytes:
    return msgspec.json.encode({"schemaVersion": 1, "command": command, "ok": True, **extra})


def _stdout(command: tuple[str, ...]) -> bytes:
    match command:
        case ("rasm-provision", verb, "--json"):
            return _json(verb)
        case ("duckdb", "--version"):
            return b"DuckDB 1.4.2\n"
        case ("forge-scientific-env", "python3", *_):
            return b"cpython-315 0\n"
        case ("forge-scientific-env", "pkg-config", "--modversion", "openblas"):
            return b"0.3.30\n"
        case ("forge-scientific-env", "sh", "-lc", *_):
            return b"/nix/store/onnxruntime/lib/libonnxruntime.dylib\n"
        case _:
            return b""


def _recording_fan(calls: list[tuple[tuple[str, ...], ...]]) -> object:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        commands = tuple(check.tool.command for check in checks)
        calls.append(commands)
        return tuple(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_stdout(command))) for command in commands)

    return fan


def _fan_payload(command: tuple[str, ...], stdout: bytes) -> object:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        assert tuple(check.tool.command for check in checks) == (command,)
        return (Ok(receipt(command, 0, status=RailStatus.OK, stdout=stdout)),)

    return fan


def _call(
    handler: Callable[[AssaySettings, ArtifactScope, ProvisionParams], Result[Report, Fault]],
    assay_root: AssayHarness,
) -> Result[Report, Fault]:
    return handler(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams())


# --- [PROVISION_PARAMS]


def test_provisionparams_arity_is_zero() -> None:
    """Provision commands do not accept positional tokens."""
    assert ProvisionParams()._arity("verify") == 0
    surplus = ProvisionParams(paths=("extra",)).bound("verify")
    assert isinstance(surplus, Fault)
    assert surplus.status is RailStatus.FAULTED


register_law(ProvisionParams, "arity_is_zero")


_STACK_VERBS = (
    ("up", provision_rail.up, (("rasm-provision", "up"),)),
    ("down", provision_rail.down, (("rasm-provision", "down"),)),
    ("status", provision_rail.status, (("rasm-provision", "status", "--json"),)),
    ("doctor", provision_rail.doctor, (("rasm-provision", "doctor", "--json"),)),
    ("ports", provision_rail.ports, (("rasm-provision", "ports", "--json"),)),
    ("inventory", provision_rail.inventory, (("rasm-provision", "inventory", "--json"),)),
    ("extensions", provision_rail.extensions, (("rasm-provision", "extensions", "--json"),)),
    ("plan", provision_rail.plan, (("rasm-provision", "plan"),)),
    ("env", provision_rail.env, (("rasm-provision", "env", "--json"),)),
)


@pytest.mark.parametrize("verb, handler, expected", _STACK_VERBS, ids=[row[0] for row in _STACK_VERBS])
def test_provision_stack_verb_delegates(
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    verb: str,
    handler: Callable[[AssaySettings, ArtifactScope, ProvisionParams], Result[Report, Fault]],
    expected: tuple[tuple[str, ...], ...],
) -> None:
    """Stack verbs delegate to the Forge-owned provisioning command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(_call(handler, assay_root))
    assert (report.claim, report.verb) == (Claim.PROVISION, verb)
    assert calls == [expected]


for _verb, _handler, _expected in _STACK_VERBS:
    register_law(_handler, f"delegates_to_stack_{_verb}")


def test_provision_status_projects_json_detail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Status JSON is admitted into structured provisioning evidence."""
    payload = _json(
        "status",
        state="present",
        project="rasm-provision-test",
        rootFingerprint="abc123",
        dockerAvailable=True,
        summary={"ok": 2},
        services={"timescale": {"enabled": True, "profile": "timescale", "port": 55432, "image": "timescale/timescaledb-ha:pg18"}},
        ports=[{"service": "timescale", "state": "busy", "owner": "provision:this-project"}],
    )
    monkeypatch.setattr(provision_rail, "fan_out", _fan_payload(("rasm-provision", "status", "--json"), payload))
    report = assert_ok(provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.facts == (
        ("state", "present"),
        ("project", "rasm-provision-test"),
        ("rootFingerprint", "abc123"),
        ("dockerAvailable", "true"),
    )
    assert report.detail.summary == (("ok", 2),)
    assert report.detail.services == (("timescale", "true", "timescale", "55432", "timescale/timescaledb-ha:pg18"),)
    assert report.detail.ports == (("timescale", "busy", "provision:this-project"),)


register_law(provision_rail.status, "projects_json_detail")


def test_provision_extensions_projects_catalog_rows(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Extension catalog rows stay separate from verification state rows."""
    payload = _json(
        "extensions",
        extensions=[{"service": "timescale", "extension": "postgis", "category": "geospatial", "required": True, "createOnVerify": True}],
    )
    monkeypatch.setattr(provision_rail, "fan_out", _fan_payload(("rasm-provision", "extensions", "--json"), payload))
    report = assert_ok(provision_rail.extensions(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.extensions == ()
    assert report.detail.extension_catalog == (("timescale", "postgis", "geospatial", "required", "create-on-verify"),)


register_law(provision_rail.extensions, "projects_catalog_rows")


def test_provision_json_verbs_fault_on_malformed_success_json(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Successful JSON-backed commands must emit valid Forge JSON."""
    monkeypatch.setattr(provision_rail, "fan_out", _fan_payload(("rasm-provision", "status", "--json"), b"{not-json"))
    fault = assert_error_status(provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()), RailStatus.FAULTED)
    assert "invalid rasm-provision JSON" in fault.message


register_law(provision_rail.status, "faults_on_malformed_success_json")


def test_provision_verify_folds_stack_and_local_probes(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The verify verb runs stack extension proof plus local runtime probes."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    monkeypatch.setattr(provision_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(provision_rail.verify(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams()))
    commands = calls[0]
    assert report.claim is Claim.PROVISION
    assert report.verb == "verify"
    assert ("rasm-provision", "verify", "--json") in commands
    assert ("duckdb", "--version") in commands
    assert any(command[:2] == ("forge-scientific-env", "python3") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "pkg-config", "--modversion") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "sh", "-lc") for command in commands)
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.local_probes == (
        ("duckdb-version", "ok"),
        ("forge-python-abi", "ok"),
        ("forge-openblas", "ok"),
        ("forge-onnxruntime-lib", "ok"),
    )
    assert report.detail.local_probe_values[-1] == ("forge-onnxruntime-lib", "ok", "/nix/store/onnxruntime/lib/libonnxruntime.dylib")


register_law(provision_rail.verify, "folds_stack_and_local_probes")
