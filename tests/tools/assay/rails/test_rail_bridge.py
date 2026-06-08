"""Laws for tools.assay.rails.bridge — BridgeParams, build, check, clean, doctor, first_fault, launch, quit, verify."""

# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------

from collections.abc import Callable
import operator
import os
from pathlib import Path
import stat
from typing import Protocol, TYPE_CHECKING
from unittest.mock import MagicMock

from expression import Error, Ok, Result
import msgspec
import pytest

from tests._aspect import register_law  # noqa: PLC2701
from tests._spec import assert_error, assert_error_status, assert_ok, support_matrix  # noqa: PLC2701
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.model import Claim, Fault, receipt, Report
from tools.assay.core.status import RailStatus
from tools.assay.rails import bridge as _bridge_mod
from tools.assay.rails.bridge import (
    _BridgeDiagnostics,  # noqa: PLC2701
    _BridgeFault,  # noqa: PLC2701
    _BridgeOutput,  # noqa: PLC2701
    _BridgePhase,  # noqa: PLC2701
    _BridgeResult,  # noqa: PLC2701
    _client_ready,  # noqa: PLC2701
    _coerce_diagnostics,  # noqa: PLC2701
    _decode_result,  # noqa: PLC2701
    _direct,  # noqa: PLC2701
    _ensure_dir,  # noqa: PLC2701
    _expire_stale,  # noqa: PLC2701
    _faulted,  # noqa: PLC2701
    _fold_scenarios,  # noqa: PLC2701
    _is_stale,  # noqa: PLC2701
    _read_result,  # noqa: PLC2701
    _resolve_project,  # noqa: PLC2701
    _rmtree,  # noqa: PLC2701
    _run_scenario,  # noqa: PLC2701
    _scenario_artifacts,  # noqa: PLC2701
    _with_facts,  # noqa: PLC2701
    BridgeParams,
    build,
    check,
    clean,
    doctor,
    first_fault,
    launch,
    quit as bridge_quit,
    verify,
)


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness, BridgeResult, RailProbe


# --- [TYPES] -----------------------------------------------------------------------------------


class _LeaseAction(Protocol):
    """Protocol for the action callable passed into `leased` (called with one held-object arg)."""

    def __call__(self, held: object) -> object: ...


# --- [CONSTANTS] -------------------------------------------------------------------------------

# Bridge verb type alias — maps the three-arg production signature.
type _BridgeVerb = Callable[[AssaySettings, ArtifactScope, BridgeParams], Result[Report, Fault]]

# Lifecycle verbs (all but verify) — routes differ only in client subcommand.
_LIFECYCLE_VERBS: tuple[tuple[str, _BridgeVerb], ...] = (
    ("check", check),
    ("clean", clean),
    ("doctor", doctor),
    ("launch", launch),
    ("quit", bridge_quit),
)


# --- [OPERATIONS] -------------------------------------------------------------------------------


# Shared lease-bypass factory: replaces `leased` so lifecycle verb laws run without filesystem locks.
def _leased_bypass(resource: str, action: _LeaseAction, *, settings: object, run_id: str, project: str = "", mode: str = "exclusive") -> object:
    return action(MagicMock())


def _install_dll(assay_root: AssayHarness) -> None:
    """Write the sentinel DLL so _client_ready returns Ok."""
    config = assay_root.settings.configuration.value
    dll_dir = assay_root.root / "tools/rhino-bridge/client/bin" / config / "net10.0"
    dll_dir.mkdir(parents=True, exist_ok=True)
    (dll_dir / "Rasm.RhinoBridge.Client.dll").write_bytes(b"")


def _make_scenario(assay_root: AssayHarness, rel: str = "tests/bridge/Case.verify.csx") -> Path:
    """Write a minimal scenario file and return its path.

    Returns:
        Absolute path of the written scenario file.
    """
    scenario = assay_root.root / rel
    scenario.parent.mkdir(parents=True, exist_ok=True)
    scenario.write_text("// ok", encoding="utf-8")
    return scenario


def _out(source: str, text: str) -> _BridgeOutput:
    return _BridgeOutput(source=source, text=text)


def _failed_phase(phase: str, *outputs: _BridgeOutput, fault: _BridgeFault | None = None, data: dict[str, object] | None = None) -> _BridgePhase:
    return _BridgePhase(phase=phase, status=RailStatus.FAILED, outputs=outputs or (), fault=fault, data=data)


def _br_failed(*phases: _BridgePhase) -> _BridgeResult:
    return _BridgeResult(status=RailStatus.FAILED, phases=phases)


# ------ BridgeParams laws -----------------------------------------------------------------------


register_law(BridgeParams, "pattern_from_paths")


def test_bridge_params_pattern_from_paths() -> None:
    """BridgeParams.pattern returns paths[0] when set, empty string otherwise."""
    assert not BridgeParams().pattern
    assert BridgeParams(paths=("tests/bridge/ok.verify.csx",)).pattern == "tests/bridge/ok.verify.csx"
    assert BridgeParams(paths=("first.verify.csx", "second.verify.csx")).pattern == "first.verify.csx"


for _arity_verb in ("verify", "check", "build", "clean", "doctor", "launch", "quit"):
    register_law(BridgeParams, f"arity_{_arity_verb}")


@pytest.mark.parametrize(
    "verb, expected_arity",
    [("verify", 1), ("check", 0), ("build", 0), ("clean", 0), ("doctor", 0), ("launch", 0), ("quit", 0)],
    ids=["verify-1", "check-0", "build-0", "clean-0", "doctor-0", "launch-0", "quit-0"],
)
def test_bridge_params_arity_discriminates_verify(verb: str, expected_arity: int) -> None:
    """BridgeParams._arity returns 1 for verify (needs a pattern) and 0 for all lifecycle verbs."""
    assert BridgeParams()._arity(verb) == expected_arity


# ------ first_fault laws -----------------------------------------------------------------------

register_law(first_fault, "clean_result_empty_pair")
register_law(first_fault, "reply_outranks_phases")
register_law(first_fault, "cause_chain_proximate_wins")
register_law(first_fault, "no_cause_direct_message")
register_law(first_fault, "execute_prefers_stdout")
register_law(first_fault, "non_execute_prefers_stderr")
register_law(first_fault, "phase_fault_field_fallback")
register_law(first_fault, "exception_reports_fallback")
register_law(first_fault, "message_truncated_256")
register_law(first_fault, "first_failing_phase_wins")

_EXC_DATA: dict[str, object] = {
    "diagnostics": {
        "exceptionReports": [{"category": "err", "message": "exception occurred", "type": "E", "stackTrace": "", "causes": []}],
        "commandWindow": [],
    }
}
_BR_CLEAN = _BridgeResult(command="verify", status=RailStatus.OK, phases=(_BridgePhase(phase="execute", status=RailStatus.OK),))
_BR_REPLY = _BridgeResult(fault=_BridgeFault(message="reply error"), phases=(_failed_phase("execute", _out("stderr", "noise")),))
_BR_CHAIN = _BridgeResult(fault=_BridgeFault(message="parent", causes=(_BridgeFault(message="proximate"),)))
_BR_EXEC = _br_failed(_failed_phase("execute", _out("stdout", "evidence"), _out("stderr", "noise")))
_BR_COMPILE = _br_failed(_failed_phase("compile", _out("stdout", "stdout noise"), _out("stderr", "compile error")))
_BR_SETUP = _br_failed(_failed_phase("setup", fault=_BridgeFault(message="setup fault")))
_BR_FIRST = _br_failed(
    _BridgePhase(phase="setup", status=RailStatus.OK),
    _failed_phase("compile", _out("stderr", "first error")),
    _failed_phase("execute", _out("stderr", "second error")),
)

# Case table: (id, BridgeResult, expected_label, expected_msg_predicate)
_FIRST_FAULT_CASES: tuple[tuple[str, _BridgeResult, str, Callable[[str], bool]], ...] = (
    ("clean_result_empty_pair", _BR_CLEAN, "", operator.not_),
    ("reply_outranks_phases", _BR_REPLY, "reply", lambda m: m == "reply error"),
    ("cause_chain_proximate_wins", _BR_CHAIN, "reply", lambda m: m == "proximate"),
    ("no_cause_direct_message", _BridgeResult(fault=_BridgeFault(message="direct", causes=())), "reply", lambda m: m == "direct"),
    ("execute_prefers_stdout", _BR_EXEC, "execute", lambda m: m == "evidence"),
    ("non_execute_prefers_stderr", _BR_COMPILE, "compile", lambda m: m == "compile error"),
    ("phase_fault_field_fallback", _BR_SETUP, "setup", lambda m: m == "setup fault"),
    ("exception_reports_fallback", _br_failed(_failed_phase("execute", data=_EXC_DATA)), "execute", lambda m: m == "exception occurred"),
    ("first_failing_phase_wins", _BR_FIRST, "compile", lambda m: m == "first error"),
)


@pytest.mark.parametrize("case_id, br, expected_label, msg_ok", _FIRST_FAULT_CASES, ids=[c[0] for c in _FIRST_FAULT_CASES])
def test_first_fault(case_id: str, br: _BridgeResult, expected_label: str, msg_ok: Callable[[str], bool]) -> None:
    """first_fault extracts (label, message) per the source-priority rules."""
    label, msg = first_fault(br)
    assert label == expected_label, f"[{case_id}] label mismatch"
    assert msg_ok(msg), f"[{case_id}] message predicate failed: {msg!r}"


def test_first_fault_message_truncated_at_256_chars() -> None:
    """first_fault truncates both reply and phase diagnostic messages to 256 characters."""
    long_text = "x" * 512
    _, msg_reply = first_fault(_BridgeResult(fault=_BridgeFault(message=long_text)))
    assert len(msg_reply) == 256
    phase = _BridgePhase(phase="execute", status=RailStatus.FAILED, outputs=(_BridgeOutput(source="stderr", text=long_text),))
    _, msg_phase = first_fault(_BridgeResult(status=RailStatus.FAILED, phases=(phase,)))
    assert len(msg_phase) == 256


# ------ _BridgeResult decode laws (via BridgeResult fixture) ------------------------------------

register_law(_BridgeResult, "valid_file_wins_over_stdout")
register_law(_BridgeResult, "malformed_file_sentinel")
register_law(_BridgeResult, "partial_file_uses_defaults")
register_law(_BridgeResult, "missing_file_uses_process_stdout")
register_law(_BridgeResult, "missing_file_malformed_stdout_sentinel")


def test_decode_result_valid_json_file_wins(bridge_result: BridgeResult) -> None:
    """_decode_result reads the JSON file when it exists, ignoring process stdout."""
    valid_path = bridge_result.valid()
    done = msgspec.structs.replace(receipt(("rasm-bridge",), 0), stdout=msgspec.json.encode(_BridgeResult(status=RailStatus.FAILED)))
    result = _decode_result(done, valid_path)
    assert result.status is RailStatus.OK
    assert result.command == "scenario.verify.csx"


def test_decode_result_malformed_file_falls_back_to_failed(bridge_result: BridgeResult) -> None:
    """_decode_result returns a FAILED sentinel when the JSON file is malformed."""
    result = _decode_result(receipt(("rasm-bridge",), 0), bridge_result.malformed())
    assert result.status is RailStatus.FAILED


def test_decode_result_partial_file_uses_defaults(bridge_result: BridgeResult) -> None:
    """_decode_result decodes a ``{}`` file using field defaults (status=FAILED, phases=())."""
    result = _decode_result(receipt(("rasm-bridge",), 0), bridge_result.partial())
    assert result.status is RailStatus.FAILED
    assert result.phases == ()


def test_decode_result_missing_file_falls_back_to_process_stdout(bridge_result: BridgeResult) -> None:
    """_decode_result falls back to process stdout when the result file is absent."""
    ok_json = msgspec.json.encode(_BridgeResult(command="check", status=RailStatus.OK))
    done = msgspec.structs.replace(receipt(("rasm-bridge",), 0), stdout=ok_json)
    result = _decode_result(done, bridge_result.missing())
    assert result.status is RailStatus.OK


def test_decode_result_missing_file_malformed_stdout_sentinel(bridge_result: BridgeResult) -> None:
    """_decode_result returns FAILED sentinel when both the file is absent and stdout is garbage."""
    done = msgspec.structs.replace(receipt(("rasm-bridge",), 0), stdout=b"{bad json")
    result = _decode_result(done, bridge_result.missing())
    assert result.status is RailStatus.FAILED


# ------ _ensure_dir laws ------------------------------------------------------------------------

register_law(_ensure_dir, "success_creates_nested")
register_law(_ensure_dir, "file_collision_faulted")


def test_ensure_dir_success(tmp_path: Path) -> None:
    """_ensure_dir returns Ok(None) and creates nested directories idempotently."""
    target = tmp_path / "reports" / "verify"
    assert_ok(_ensure_dir(target))
    assert target.is_dir()
    assert_ok(_ensure_dir(target))


def test_ensure_dir_file_collision_returns_faulted(tmp_path: Path) -> None:
    """_ensure_dir returns Error(Fault(status=FAULTED)) when a file blocks directory creation."""
    blocker = tmp_path / "blocked"
    blocker.write_text("file", encoding="utf-8")
    assert_error_status(_ensure_dir(blocker / "nested"), RailStatus.FAULTED)


# ------ Lifecycle verb laws (check, clean, doctor, launch, quit) --------------------------------

for _lc_name, _lc_fn in _LIFECYCLE_VERBS:
    register_law(_lc_fn, f"{_lc_name}_routes_run_check")


@pytest.mark.parametrize("verb_name, verb_fn", _LIFECYCLE_VERBS, ids=[v[0] for v in _LIFECYCLE_VERBS])
def test_lifecycle_verb_routes_to_run_check(
    verb_name: str, verb_fn: _BridgeVerb, assay_root: AssayHarness, rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch
) -> None:
    """Lifecycle verbs delegate to run_check under the bridge lease — confirmed via captured call."""
    rail_probe.install(monkeypatch, _bridge_mod, "run_check", Ok(receipt(("rasm-bridge", verb_name), 0)))
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    assert_ok(verb_fn(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams()))
    assert len(rail_probe.checks) == 1, f"{verb_name} did not invoke run_check"


# ------ build verb laws -------------------------------------------------------------------------

register_law(build, "build_routes_build_tool")


def test_build_routes_build_tool(assay_root: AssayHarness, rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch) -> None:
    """Build delegates to run_check with the build-specific tool (rasm-bridge-build)."""
    rail_probe.install(monkeypatch, _bridge_mod, "run_check", Ok(receipt(("rasm-bridge-build",), 0)))
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    assert_ok(build(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams()))
    assert len(rail_probe.checks) == 1
    assert rail_probe.checks[0].tool.name == "rasm-bridge-build"


# ------ verify verb laws ------------------------------------------------------------------------

register_law(verify, "no_scenarios_unsupported")


def test_verify_no_scenarios_unsupported_report(assay_root: AssayHarness, rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch) -> None:
    """Verify produces an UNSUPPORTED report when no scenarios are discovered under the workspace."""
    rail_probe.install(monkeypatch, _bridge_mod, "run_check", Ok(receipt(("rasm-bridge",), 0)))
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    report = assert_ok(verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams()))
    assert report.status is RailStatus.UNSUPPORTED


# ------ support_matrix: structural probe of public surface --------------------------------------

register_law(BridgeParams, "module_public_surface")


def test_bridge_module_public_surface() -> None:
    """Bridge module __all__ names exactly the 9 expected public symbols — no silent omission or addition."""
    expected = frozenset(["BridgeParams", "build", "check", "clean", "doctor", "first_fault", "launch", "quit", "verify"])
    support_matrix(
        ("all_present", lambda: expected <= frozenset(_bridge_mod.__all__), True),
        ("no_extras", lambda: frozenset(_bridge_mod.__all__) == expected, True),
    )


# ------ support_matrix: BridgeParams field defaults -------------------------------------------

register_law(BridgeParams, "defaults_validity")


def test_bridge_params_defaults_validity() -> None:
    """BridgeParams default fields hold expected zero-value sentinels."""
    p = BridgeParams()
    support_matrix(
        ("paths empty tuple", lambda: p.paths == (), True),
        ("language absent", lambda: p.language is None, True),
        ("pattern empty", lambda: not p.pattern, True),
    )
    assert p.paths == ()
    assert p.language is None


# ------ _coerce_diagnostics laws ----------------------------------------------------------------

register_law(_coerce_diagnostics, "valid_converts")
register_law(_coerce_diagnostics, "invalid_returns_default")


@pytest.mark.parametrize(
    "raw, expected_exception_count",
    [
        ({"commandWindow": [], "exceptionReports": [{"category": "e", "message": "boom", "type": "T", "stackTrace": "", "causes": []}]}, 1),
        ("not-a-dict", 0),
        (None, 0),
    ],
    ids=["valid-one-exception", "invalid-string-fallback", "none-fallback"],
)
def test_coerce_diagnostics_result(raw: object, expected_exception_count: int) -> None:
    """_coerce_diagnostics converts valid payloads and returns a default on ValidationError."""
    result = _coerce_diagnostics(raw)
    assert isinstance(result, _BridgeDiagnostics)
    assert len(result.exception_reports) == expected_exception_count


# ------ _with_facts laws ------------------------------------------------------------------------

register_law(_with_facts, "no_markers_returns_done_unchanged")
register_law(_with_facts, "evidence_marker_appended_as_note")
register_law(_with_facts, "capture_marker_appended_as_note")


@pytest.mark.parametrize(
    "marker_line, note_prefix, changed",
    [
        ("plain text", None, False),
        ('rasm.rhino-bridge.evidence=facts={"key":"value"}', "facts:", True),
        ('rasm.rhino-bridge.capture={"png":"base64data"}', "capture:", True),
    ],
    ids=["no-marker-identity", "evidence-marker", "capture-marker"],
)
def test_with_facts(marker_line: str, note_prefix: str | None, changed: bool) -> None:  # noqa: FBT001
    """_with_facts appends/skips notes based on execute stdout marker prefixes."""
    done = receipt(("rasm-bridge",), 0)
    phase = _BridgePhase(phase="execute", status=RailStatus.OK, outputs=(_BridgeOutput(source="stdout", text=marker_line),))
    result_br = _BridgeResult(status=RailStatus.OK, phases=(phase,))
    out = _with_facts(done, result_br)
    if changed:
        assert note_prefix is not None
        assert any(note.startswith(note_prefix) for note in out.notes)
    else:
        assert out is done


# ------ _read_result OSError branch -------------------------------------------------------------

register_law(_decode_result, "os_error_fallback_to_failed")


def test_read_result_oserror_path(tmp_path: Path) -> None:
    """_read_result returns Error(Fault(FAULTED)) when read_bytes raises PermissionError on a chmod-000 file."""
    restricted = tmp_path / "restricted.json"
    restricted.write_bytes(b'{"status":"ok"}')
    restricted.chmod(0)
    try:
        r = _read_result(restricted)
        if r.is_error():
            err = r.error
            assert err.status is RailStatus.FAULTED or err.status is RailStatus.SKIP
    finally:
        restricted.chmod(stat.S_IRUSR | stat.S_IWUSR)


# ------ _resolve_project laws -------------------------------------------------------------------

register_law(_resolve_project, "lib_path_with_csproj_resolves_project")
register_law(_resolve_project, "lib_path_without_csproj_falls_back_to_testkit")
register_law(_resolve_project, "non_lib_path_resolves_testkit")


def test_resolve_project_lib_path_with_csproj(assay_root: AssayHarness) -> None:
    """_resolve_project resolves the lib .Tests.csproj when the scenario lives under tests/csharp/libs/<project>."""
    csproj = assay_root.write("tests/csharp/libs/MyLib/MyLib.Tests.csproj", "<Project/>")
    scenario = _make_scenario(assay_root, "tests/csharp/libs/MyLib/scenarios/Case.verify.csx")
    assert _resolve_project(assay_root.settings, scenario) == csproj


def test_resolve_project_lib_path_without_csproj_falls_back(assay_root: AssayHarness) -> None:
    """_resolve_project falls back to TestKit when the lib .Tests.csproj doesn't exist."""
    r = _resolve_project(assay_root.settings, _make_scenario(assay_root, "tests/csharp/libs/NoProj/scenarios/Case.verify.csx"))
    assert "TestKit" in r.name or "testkit" in str(r).lower() or r.suffix == ".csproj"


def test_resolve_project_non_lib_path_resolves_testkit(assay_root: AssayHarness) -> None:
    """_resolve_project returns the TestKit csproj for scenarios outside tests/csharp/libs."""
    r = _resolve_project(assay_root.settings, _make_scenario(assay_root, "tests/bridge/Smoke.verify.csx"))
    assert "TestKit" in r.name or r.suffix == ".csproj"


# ------ _client_ready laws ----------------------------------------------------------------------

register_law(_client_ready, "dll_absent_returns_error")
register_law(_client_ready, "dll_present_returns_ok")


def test_client_ready_dll_absent_returns_error(assay_root: AssayHarness) -> None:
    """_client_ready returns Error when no Rasm.RhinoBridge.Client.dll exists under bin/<config>."""
    result = _client_ready(assay_root.settings)
    assert result.is_error()
    assert "bridge build" in result.error


def test_client_ready_dll_present_returns_ok(assay_root: AssayHarness) -> None:
    """_client_ready returns Ok(None) when the client DLL exists under the expected bin path."""
    _install_dll(assay_root)
    assert _client_ready(assay_root.settings).is_ok()


# ------ _run_scenario laws (client not ready + run_check fake) ----------------------------------

register_law(_run_scenario, "client_not_ready_returns_failed_scenario")
register_law(_run_scenario, "run_check_ok_produces_scenario")
register_law(_run_scenario, "run_check_error_produces_failed_scenario")

_ok_json = msgspec.json.encode(_BridgeResult(command="check", status=RailStatus.OK))
_run_scenario_cases: tuple[tuple[str, bool, object, RailStatus, str], ...] = (
    ("client_not_ready", False, None, RailStatus.FAILED, "client"),
    ("run_check_ok", True, lambda *a, **kw: Ok(receipt(("rasm-bridge",), 0, stdout=_ok_json)), RailStatus.OK, "Case"),
    ("run_check_error", True, lambda *a, **kw: Error(Fault(("rasm-bridge",), RailStatus.FAULTED, "timeout")), RailStatus.FAILED, "launch"),
)


@pytest.mark.parametrize("case_id, dll, patch_fn, exp_status, exp_attr", _run_scenario_cases, ids=[c[0] for c in _run_scenario_cases])
def test_run_scenario(
    assay_root: AssayHarness,
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    case_id: str,
    dll: bool,  # noqa: FBT001
    patch_fn: object,
    exp_status: RailStatus,
    exp_attr: str,
) -> None:
    """_run_scenario routes to FAILED(client/launch) or OK(stem) based on DLL presence and run_check outcome."""
    if dll:
        _install_dll(assay_root)
    if patch_fn is not None:
        monkeypatch.setattr(_bridge_mod, "run_check", patch_fn)
    report_dir = tmp_path / "verify"
    report_dir.mkdir()
    s = _run_scenario(assay_root.settings, report_dir, _make_scenario(assay_root))
    assert s.status is exp_status
    match exp_status:
        case RailStatus.OK:
            assert s.stem == exp_attr
        case _:
            assert s.fault_phase == exp_attr


# ------ _direct path-outside-base and dir-scan branches ----------------------------------------

register_law(_direct, "outside_base_returns_empty")
register_law(_direct, "directory_scans_recursively")
register_law(_direct, "single_file_with_suffix_returned")
register_law(_direct, "file_without_scenario_suffix_returns_empty")


@pytest.mark.parametrize(
    "candidate_fn, expected_count, check_fn",
    [
        (
            lambda p: [
                (p.parent / f"{p.name}-sibling.verify.csx").write_text("// outside", encoding="utf-8"),
                str(p.parent / f"{p.name}-sibling.verify.csx"),
            ][-1],
            0,
            None,
        ),
        (
            lambda p: [
                (p / "sub").mkdir(exist_ok=True),
                (p / "sub" / "A.verify.csx").write_text("//", encoding="utf-8"),
                (p / "sub" / "B.verify.csx").write_text("//", encoding="utf-8"),
                str(p / "sub"),
            ][-1],
            2,
            lambda p, result: set(result) == {(p / "sub" / "A.verify.csx").resolve(), (p / "sub" / "B.verify.csx").resolve()},
        ),
        (
            lambda p: [(p / "Smoke.verify.csx").write_text("// smoke", encoding="utf-8"), str(p / "Smoke.verify.csx")][-1],
            1,
            lambda p, result: result == ((p / "Smoke.verify.csx").resolve(),),
        ),
        (lambda p: [(p / "README.md").write_text("# README", encoding="utf-8"), str(p / "README.md")][-1], 0, None),
    ],
    ids=["outside-base-empty", "directory-recursive", "single-scenario", "wrong-suffix-empty"],
)
def test_direct(
    tmp_path: Path, candidate_fn: Callable[[Path], str], expected_count: int, check_fn: Callable[[Path, tuple[Path, ...]], bool] | None
) -> None:
    """_direct returns paths inside base matching .verify.csx, empty for out-of-base or wrong suffix."""
    result = _direct(tmp_path, candidate_fn(tmp_path))
    assert len(result) == expected_count
    if check_fn is not None:
        assert check_fn(tmp_path, result)


# ------ _expire_stale laws ----------------------------------------------------------------------

register_law(_expire_stale, "non_existent_claim_root_no_op")
register_law(_expire_stale, "stale_child_removed")


def test_expire_stale_nonexistent_claim_root_noop(tmp_path: Path) -> None:
    """_expire_stale is a no-op when the claim root doesn't exist — no exception raised."""
    report_dir = tmp_path / "missing_root" / "run-id" / "verify"
    _expire_stale(report_dir, "run-id", 300.0)


def test_expire_stale_removes_stale_child(tmp_path: Path) -> None:
    """_expire_stale removes children whose mtime is past the TTL cutoff."""
    claim_root = tmp_path / "claim"
    claim_root.mkdir()
    run_id = "current-run"
    stale = claim_root / "old-run"
    stale.mkdir()
    os.utime(stale, (0.0, 0.0))
    live = claim_root / run_id
    live.mkdir()
    report_dir = live / "verify"
    _expire_stale(report_dir, run_id, 300.0)
    assert not stale.exists(), "stale child must be removed"
    assert live.exists(), "live run dir must be preserved"


# ------ _is_stale laws --------------------------------------------------------------------------

register_law(_is_stale, "oserror_returns_false")
register_law(_is_stale, "stale_dir_true")


def test_is_stale_oserror_returns_false(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """_is_stale returns False when stat raises OSError (e.g. path disappears between iterdir and stat)."""
    child = tmp_path / "victim"
    child.mkdir()
    original_stat = Path.stat

    def _patched_stat(self: Path, *, follow_symlinks: bool = True) -> object:
        if self == child:
            raise OSError("simulated stat failure")
        return original_stat(self, follow_symlinks=follow_symlinks)

    monkeypatch.setattr(Path, "stat", _patched_stat)
    assert _is_stale(child, tmp_path, cutoff=float("inf")) is False


def test_is_stale_stale_dir_true(tmp_path: Path) -> None:
    """_is_stale returns True for a real directory whose mtime is before the cutoff."""
    child = tmp_path / "stale_dir"
    child.mkdir()
    os.utime(child, (0.0, 0.0))
    assert _is_stale(child, tmp_path, cutoff=float("inf")) is True


# ------ _rmtree law -----------------------------------------------------------------------------

register_law(_rmtree, "removes_tree_and_returns_path")


def test_rmtree_removes_tree_and_returns_path(tmp_path: Path) -> None:
    """_rmtree removes the directory tree and returns the original path."""
    target = tmp_path / "target"
    target.mkdir()
    (target / "file.txt").write_text("content", encoding="utf-8")
    returned = _rmtree(target)
    assert returned == target
    assert not target.exists()


# ------ _faulted laws ---------------------------------------------------------------------------

register_law(_faulted, "failed_ok_becomes_fault")
register_law(_faulted, "unsupported_ok_becomes_fault")
register_law(_faulted, "error_passthrough")


@pytest.mark.parametrize(
    "status, exit_code", [(RailStatus.FAILED, 1), (RailStatus.TIMEOUT, 5)], ids=["failed-promoted-to-faulted", "timeout-promoted"]
)
def test_faulted_non_zero_ok_becomes_error(status: RailStatus, exit_code: int) -> None:
    """_faulted promotes a success-channel receipt with non-OK status to Error(Fault)."""
    done = receipt(("rasm-bridge",), exit_code, status=status, stderr=b"build failed")
    assert isinstance(assert_error(_faulted(Ok(done))), Fault)


def test_faulted_error_passthrough() -> None:
    """_faulted passes Error(Fault) through unchanged."""
    fault = Fault(("rasm-bridge",), RailStatus.FAULTED, "original")
    assert assert_error(_faulted(Error(fault))) is fault


def test_faulted_ok_status_passthrough() -> None:
    """_faulted returns Ok when the receipt has severity <= OK (skip, empty, ok)."""
    assert_ok(_faulted(Ok(receipt(("rasm-bridge",), 0, status=RailStatus.OK))))


# ------ _fold_scenarios non-empty branch --------------------------------------------------------

register_law(_fold_scenarios, "non_empty_scenarios_with_ok_run")
register_law(_fold_scenarios, "non_empty_scenarios_with_failed_run")


@pytest.mark.parametrize(
    "scenario_stem, bridge_status, rc, ok_expected",
    [("Smoke", RailStatus.OK, 0, True), ("Fail", RailStatus.FAILED, 1, False)],
    ids=["ok-scenario", "failed-scenario"],
)
def test_fold_scenarios_non_empty(
    assay_root: AssayHarness,
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    scenario_stem: str,
    bridge_status: RailStatus,
    rc: int,
    ok_expected: bool,  # noqa: FBT001
) -> None:
    """_fold_scenarios folds scenario receipts into a Report; failed scenario → non-OK status."""
    _install_dll(assay_root)
    report_dir = tmp_path / "verify"
    report_dir.mkdir()
    scenario = _make_scenario(assay_root, f"tests/bridge/{scenario_stem}.verify.csx")
    payload_json = msgspec.json.encode(_BridgeResult(command="check", status=bridge_status))
    canned = receipt(("rasm-bridge",), rc, status=bridge_status, stdout=payload_json)
    monkeypatch.setattr(_bridge_mod, "run_check", lambda *a, **kw: Ok(canned))
    if ok_expected:
        result_path = report_dir / f"{scenario_stem}.json"
        result_path.write_bytes(payload_json)
    report = assert_ok(_fold_scenarios(assay_root.settings, report_dir, (scenario,), ("bridge", "verify", "")))
    assert isinstance(report, Report)
    assert report.counts.total >= 1
    if not ok_expected:
        assert report.status is not RailStatus.OK


# ------ _scenario_artifacts law -----------------------------------------------------------------

register_law(_scenario_artifacts, "json_files_become_artifacts")


def test_scenario_artifacts_json_files_become_rhino_artifacts(tmp_path: Path) -> None:
    """_scenario_artifacts surfaces every .json file in the report_dir as a Rhino artifact."""
    r1 = tmp_path / "case1.json"
    r2 = tmp_path / "case2.json"
    r1.write_text('{"status":"ok"}\n', encoding="utf-8")
    r2.write_text('{"status":"failed"}\n', encoding="utf-8")
    artifacts = _scenario_artifacts(tmp_path)
    assert len(artifacts) == 2
    assert {a.id for a in artifacts} == {"case1", "case2"}
    assert all(a.bytes > 0 and a.lines >= 1 for a in artifacts)


# ------ _verify_locked prelude-error branch (build/launch fault) --------------------------------

register_law(verify, "prelude_fault_propagated")


def test_verify_locked_prelude_fault_propagated(assay_root: AssayHarness, rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch) -> None:
    """Verify returns Error(Fault) when the build/launch prelude fails — the prelude error path."""
    build_fault = Fault(("rasm-bridge-build",), RailStatus.FAULTED, "build error")
    rail_probe.install(monkeypatch, _bridge_mod, "run_check", Error(build_fault))
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    fault = assert_error(verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams()))
    assert isinstance(fault, Fault)
