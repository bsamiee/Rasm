"""Law matrix for the supervisor-backed ``tools.assay.rails.bridge`` rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable
from pathlib import Path
from typing import Protocol, TYPE_CHECKING
from unittest.mock import MagicMock

from expression import Ok, Result
import msgspec
import pytest

from tests.python._testkit.laws import register_law, register_laws
from tests.python._testkit.spec import assert_error_status, assert_ok
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.model import BridgeLifecycle, Claim, Fault, receipt, Report, validate_detail, VerifySummary
from tools.assay.core.status import RailStatus
from tools.assay.rails import bridge as _bridge_mod
from tools.assay.rails.bridge import (
    _aggregate_closure,
    _closure_index,
    _ClosureManifest,
    _completed_from_stdout,
    _decode_envelope,
    _faulted,
    _HostFingerprint,
    _plan,
    _scenario_artifacts,
    bridge_lease,
    BridgeParams,
    build,
    check,
    clean,
    client_run,
    doctor,
    first_fault,
    launch,
    quit as bridge_quit,
    verify,
)


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import AssayHarness, RailProbe


# --- [TYPES] ----------------------------------------------------------------------------


class _LeaseAction(Protocol):
    def __call__(self, held: object) -> object: ...


type _BridgeVerb = Callable[[AssaySettings, ArtifactScope, BridgeParams], Result[Report, Fault]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_LIFECYCLE_VERBS: tuple[tuple[str, _BridgeVerb], ...] = (
    ("check", check),
    ("clean", clean),
    ("doctor", doctor),
    ("launch", launch),
    ("quit", bridge_quit),
)

register_laws(
    (check, ("lifecycle_verbs_fold_supervisor_completion",)),
    (clean, ("lifecycle_verbs_fold_supervisor_completion",)),
    (doctor, ("lifecycle_verbs_fold_supervisor_completion",)),
    (launch, ("lifecycle_verbs_fold_supervisor_completion",)),
    (bridge_quit, ("lifecycle_verbs_fold_supervisor_completion",)),
)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _leased_bypass(resource: str, action: _LeaseAction, *, settings: object, run_id: str, project: str = "", mode: str = "exclusive") -> object:
    _ = (resource, settings, run_id, project, mode)
    return action(MagicMock())


def _envelope(
    *,
    status: RailStatus = RailStatus.OK,
    report_dir: str = "",
    first_failure: str = "",
    first_fault_phase: str | None = None,
    scenarios: tuple[dict[str, object], ...] = (),
    evidence: tuple[dict[str, object], ...] = (),
    host: dict[str, object] | None = None,
    capabilities: tuple[dict[str, object], ...] = (),
) -> bytes:
    return msgspec.json.encode({
        "runId": "run-1",
        "verb": "verify",
        "status": status.value,
        "durationMs": 12.5,
        "reportDir": report_dir,
        "host": host or {},
        "capabilities": list(capabilities),
        "scenarios": list(scenarios),
        "evidence": list(evidence),
        "firstFailure": first_failure,
        "firstFaultPhase": first_fault_phase,
    })


def _closure(path: Path, *assemblies: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    for assembly in assemblies:
        (path.parent / assembly).write_text(assembly, encoding="utf-8")
    path.write_bytes(
        msgspec.json.encode({
            "assemblies": list(assemblies),
            "hostPlugins": ["b45a29b1-4343-4035-989e-044e8580d9cf"],
            "builtAgainst": {"bundleVersion": "9.0", "rhinoCommonVersion": "9.0", "grasshopper2Version": "2.0", "runtimeVersion": "10.0"},
        })
    )


# --- [BRIDGE_PARAMS]

register_law(BridgeParams, "pattern_from_paths")
register_law(BridgeParams, "verify_accepts_empty_pattern")


def test_bridge_params_pattern_and_arity() -> None:
    assert not BridgeParams().pattern
    assert BridgeParams(paths=("blocks",)).pattern == "blocks"
    assert BridgeParams()._arity("verify") == 1
    assert BridgeParams().bound("verify") == BridgeParams()


def test_bridge_module_public_surface() -> None:
    expected = frozenset((
        "BridgeParams",
        "bridge_lease",
        "build",
        "check",
        "clean",
        "client_run",
        "doctor",
        "first_fault",
        "launch",
        "quit",
        "verify",
    ))
    assert frozenset(_bridge_mod.__all__) == expected


# --- [SELECTION_PLAN]

register_law(_plan, "empty_pattern_selects_all")
register_law(_plan, "theme_pattern_selects_theme_case")
register_law(_plan, "bare_names_project_to_qualified_names")
register_law(_plan, "project_path_selects_corpus_not_literal_name")
register_law(_plan, "unknown_pattern_is_unsupported")


def test_plan_empty_pattern_selects_all() -> None:
    plan = assert_ok(_plan(""))
    assert msgspec.json.decode(plan.selection_json.encode()) == {"$type": "all"}
    assert {corpus.assembly for corpus in plan.corpora} == {"Rasm.Tests.dll", "Rasm.Rhino.Tests.dll", "Rasm.Grasshopper.Tests.dll"}


def test_plan_theme_pattern_selects_theme_case() -> None:
    plan = assert_ok(_plan("blocks"))
    assert msgspec.json.decode(plan.selection_json.encode()) == {"$type": "themes", "themes": ["blocks"]}
    assert tuple(corpus.assembly for corpus in plan.corpora) == ("Rasm.Rhino.Tests.dll",)


def test_plan_bare_names_project_to_qualified_names() -> None:
    plan = assert_ok(_plan("CoreRail,NativeRail"))
    payload = msgspec.json.decode(plan.selection_json.encode())
    assert payload == {"$type": "names", "names": ["blocks.CoreRail", "analysis.NativeRail"]}
    assert {corpus.assembly for corpus in plan.corpora} == {"Rasm.Rhino.Tests.dll", "Rasm.Tests.dll"}


def test_plan_project_path_selects_corpus_not_literal_name() -> None:
    plan = assert_ok(_plan("tests/csharp/libs/Rasm.Rhino/Blocks/Scenarios"))
    payload = msgspec.json.decode(plan.selection_json.encode())
    assert payload["$type"] == "themes"
    assert "tests/csharp/libs/Rasm.Rhino/Blocks/Scenarios" not in payload.get("names", ())
    assert tuple(corpus.assembly for corpus in plan.corpora) == ("Rasm.Rhino.Tests.dll",)


def test_plan_unknown_pattern_is_unsupported() -> None:
    assert_error_status(_plan("not-a-scenario"), RailStatus.UNSUPPORTED)


# --- [SESSION_ENVELOPE]

register_law(_decode_envelope, "stdout_session_envelope_decodes")
register_law(first_fault, "first_failure_pair")
register_law(_completed_from_stdout, "status_notes_artifacts_from_envelope")
register_law(_faulted, "failed_completed_maps_to_fault")


def test_decode_envelope_and_first_fault() -> None:
    done = receipt(("rasm-bridge",), 0, stdout=_envelope(status=RailStatus.FAILED, first_failure="compile failed", first_fault_phase="load"))
    envelope = _decode_envelope(done)
    assert envelope.status is RailStatus.FAILED
    assert first_fault(envelope) == ("load", "compile failed")


def test_decode_malformed_stdout_returns_failed_sentinel() -> None:
    envelope = _decode_envelope(receipt(("rasm-bridge",), 0, stdout=b"{bad", stderr=b"raw failure"))
    assert envelope.status is RailStatus.FAILED
    assert envelope.first_failure == "raw failure"


def test_completed_from_stdout_projects_status_notes_and_artifacts(tmp_path: Path) -> None:
    (tmp_path / "facts.jsonl").write_text("{}", encoding="utf-8")
    (tmp_path / "view.png").write_bytes(b"png")
    (tmp_path / "ignore.bin").write_bytes(b"bin")
    done = _completed_from_stdout(receipt(("rasm-bridge",), 0, stdout=_envelope(report_dir=str(tmp_path))))
    assert done.status is RailStatus.OK
    assert done.notes == (f"bridge.reportDir={tmp_path}",)
    assert {Path(artifact.path).name for artifact in done.artifacts} == {"facts.jsonl", "view.png"}


def test_faulted_maps_failed_completion_to_fault() -> None:
    fault = assert_error_status(_faulted(Ok(receipt(("rasm-bridge",), 1, status=RailStatus.FAILED, stderr=b"boom"))), RailStatus.FAULTED)
    assert "boom" in fault.message
    assert _faulted(Ok(receipt(("rasm-bridge",), 0, status=RailStatus.OK))).is_ok()


# --- [CLOSURE_AGGREGATION]

register_law(_closure_index, "indexes_typed_scenario_closures")
register_law(_aggregate_closure, "aggregates_selected_closures")
register_law(_aggregate_closure, "missing_selected_closure_faults")


def test_closure_index_and_aggregate_selected_corpus(assay_root: AssayHarness) -> None:
    scope = assay_root.scope(Claim.BRIDGE)
    root = Path(scope.ensure())
    _closure(root / "rasm" / "bridge-closure.json", "Rasm.Tests.dll", "Core.dll")
    _closure(root / "rhino" / "bridge-closure.json", "Rasm.Rhino.Tests.dll", "Rhino.dll")
    cargo = root / "bin" / "Cargo" / assay_root.settings.configuration.value.lower()
    cargo.mkdir(parents=True)
    (cargo / "Cargo.dll").write_bytes(b"")

    index = assert_ok(_closure_index(scope))
    assert set(index) == {"Rasm.Tests.dll", "Rasm.Rhino.Tests.dll"}

    target = assert_ok(_aggregate_closure(assay_root.settings, scope, assert_ok(_plan("blocks")), index))
    payload = msgspec.json.decode(target.read_bytes())
    assert {Path(row).name for row in payload["assemblies"]} == {"Rasm.Rhino.Tests.dll", "Rhino.dll", "Cargo.dll"}
    assert payload["hostPlugins"] == ["b45a29b1-4343-4035-989e-044e8580d9cf"]


def test_aggregate_missing_selected_closure_faults(assay_root: AssayHarness) -> None:
    fault = assert_error_status(
        _aggregate_closure(assay_root.settings, assay_root.scope(Claim.BRIDGE), assert_ok(_plan("analysis")), {}), RailStatus.FAULTED
    )
    assert "Rasm.Tests.dll" in fault.message


def test_read_closure_fallback_shape_is_empty() -> None:
    empty = _ClosureManifest()
    assert empty.assemblies == ()
    assert empty.built_against == _HostFingerprint()


# --- [CLIENT_AND_LIFECYCLE]

register_law(client_run, "runs_supervisor_project_with_single_json_envelope")
register_law(bridge_lease, "serializes_bridge_resource")


def test_client_run_invokes_canonical_supervisor_project(assay_root: AssayHarness, rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch) -> None:
    rail_probe.install(monkeypatch, _bridge_mod, "run_check", Ok(receipt(("dotnet",), 0, stdout=_envelope())))
    done = assert_ok(client_run(assay_root.settings, "doctor"))
    check = rail_probe.checks[0]
    assert done.status is RailStatus.OK
    assert any(Path(part).as_posix().endswith("tools/rhino-bridge/Supervisor/Supervisor.csproj") for part in check.tool.command)
    assert check.tool.command[-1] == "doctor"
    assert check.cwd == assay_root.root


def test_bridge_lease_second_contender_sees_busy(assay_root: AssayHarness) -> None:
    nested: list[Result[str, Fault]] = []

    def contend() -> Result[str, Fault]:
        nested.append(bridge_lease(assay_root.settings, lambda: Ok("second")))
        return Ok("first")

    assert assert_ok(bridge_lease(assay_root.settings, contend)) == "first"
    assert nested
    assert nested[0].is_error()
    assert assert_ok(bridge_lease(assay_root.settings, lambda: Ok("third"))) == "third"


@pytest.mark.parametrize("verb_name, verb_fn", _LIFECYCLE_VERBS, ids=[name for name, _ in _LIFECYCLE_VERBS])
def test_lifecycle_verbs_fold_supervisor_completion(
    verb_name: str, verb_fn: _BridgeVerb, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch
) -> None:
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    monkeypatch.setattr(_bridge_mod, "client_run", lambda _settings, *args, **_kw: Ok(receipt(("rasm-bridge", *args), 0, status=RailStatus.OK)))
    report = assert_ok(verb_fn(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams()))
    assert report.claim is Claim.BRIDGE
    assert report.verb == verb_name
    assert report.status is RailStatus.OK


register_law(BridgeLifecycle, "lifecycle_detail_projects_host_and_capabilities")


def test_lifecycle_detail_projects_host_and_capabilities(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A lifecycle fold projects the supervisor host versions and capability rows into a wire-round-tripping BridgeLifecycle detail."""
    envelope = _envelope(
        report_dir="report/doctor",
        host={"bundleVersion": "9.0", "rhinoCommonVersion": "9.0", "grasshopper2Version": "", "runtimeVersion": "10.0"},
        capabilities=({"key": "rail.core", "outcome": "ok", "receipt": "warm"}, {"key": "rail.vectors", "outcome": "skipped", "receipt": ""}),
    )
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    monkeypatch.setattr(
        _bridge_mod, "client_run", lambda _settings, *args, **_kw: Ok(receipt(("rasm-bridge", *args), 0, stdout=envelope, status=RailStatus.OK))
    )
    report = assert_ok(doctor(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams()))
    detail = report.detail
    assert isinstance(detail, BridgeLifecycle)
    assert (detail.verb, detail.report_dir) == ("doctor", "report/doctor")
    # Empty grasshopper2 version is elided; surviving rows keep fingerprint order.
    assert detail.host == (("bundle", "9.0"), ("rhinoCommon", "9.0"), ("runtime", "10.0"))
    # Capability admission rows keep their key/outcome/receipt triple.
    assert detail.capabilities == (("rail.core", "ok", "warm"), ("rail.vectors", "skipped", ""))
    assert validate_detail(detail) == detail, "BridgeLifecycle did not survive the tagged-union wire codec"


register_law(build, "folds_bridge_build_receipt")


def test_build_folds_bridge_build_receipt(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(_bridge_mod, "_build_closure", lambda _settings: Ok(receipt(("rasm-bridge-build",), 0, status=RailStatus.OK)))
    report = assert_ok(build(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams()))
    assert report.claim is Claim.BRIDGE
    assert report.verb == "build"
    assert report.status is RailStatus.OK
    assert report.results == ()


# --- [VERIFY]

register_law(verify, "folds_session_summary")


def test_verify_folds_session_summary(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    closure = tmp_path / "bridge-closure.assay.json"
    closure.write_text("{}", encoding="utf-8")
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    monkeypatch.setattr(_bridge_mod, "_build_closure", lambda _settings: Ok(receipt(("rasm-bridge-build",), 0, status=RailStatus.OK)))
    monkeypatch.setattr(_bridge_mod, "_closure_index", lambda _scope: Ok({"Rasm.Rhino.Tests.dll": (closure, _ClosureManifest())}))
    monkeypatch.setattr(_bridge_mod, "_aggregate_closure", lambda _settings, _scope, _plan, _index: Ok(closure))
    monkeypatch.setattr(
        _bridge_mod,
        "client_run",
        lambda _settings, *_args, **_kw: Ok(
            receipt(
                ("rasm-bridge",),
                0,
                status=RailStatus.OK,
                stdout=_envelope(
                    scenarios=({"scenario": "blocks.CoreRail", "status": "ok", "durationMs": 1.0},),
                    evidence=({"$type": "fact", "stamp": {"scenario": "blocks.CoreRail"}, "key": "mesh.count", "value": 3},),
                ),
            )
        ),
    )

    report = assert_ok(verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(paths=("blocks",))))
    assert report.claim is Claim.BRIDGE
    assert report.verb == "verify"
    assert isinstance(report.detail, VerifySummary)
    assert report.detail.facts
    assert report.detail.facts[0][0] == "blocks.CoreRail"


def test_scenario_artifacts_tolerates_missing_report_dir(tmp_path: Path) -> None:
    assert _scenario_artifacts(tmp_path / "missing") == ()
