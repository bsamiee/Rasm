"""Law matrix for the supervisor-backed ``tools.assay.rails.bridge`` rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable
import hashlib
from pathlib import Path
from typing import Protocol, TYPE_CHECKING
from unittest.mock import MagicMock

from expression import Error, Ok, Result
import msgspec
import pytest

from tests.python._testkit.laws import register_law, register_laws
from tests.python._testkit.spec import assert_error_status, assert_ok
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.model import Artifact, ArtifactKind, BridgeLifecycle, Claim, Fault, RailStatus, receipt, Report, validate_detail, VerifySummary
from tools.assay.rails import bridge as _bridge_mod
from tools.assay.rails.bridge import (
    _aggregate_closure,
    _ClosureManifest,
    _completed_from_stdout,
    _decode_envelope,
    _faulted,
    _freshness,
    _FRESHNESS_ABSENT,
    _freshness_note,
    _FRESHNESS_STALE,
    _HostFingerprint,
    _scenario_artifacts,
    _scenario_closure,
    _selection,
    bridge_lease,
    BridgeParams,
    build,
    client_run,
    first_fault,
    quit as bridge_quit,
    status,
    verify,
)


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.engine import Executor
    from tools.assay.core.model import Check, Completed


# --- [TYPES] ----------------------------------------------------------------------------


class _LeaseAction(Protocol):
    def __call__(self, held: object) -> object: ...


type _BridgeVerb = Callable[[AssaySettings, ArtifactScope, BridgeParams, Executor], Result[Report, Fault]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_LIFECYCLE_VERBS: tuple[tuple[str, _BridgeVerb], ...] = (("quit", bridge_quit), ("status", status))

register_laws((bridge_quit, ("lifecycle_verbs_fold_supervisor_completion",)), (status, ("lifecycle_verbs_fold_supervisor_completion",)))


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
    expected = frozenset(("BridgeParams", "bridge_lease", "build", "client_run", "first_fault", "quit", "status", "verify"))
    assert frozenset(_bridge_mod.__all__) == expected


def test_freshness_note_maps_state_to_remediation() -> None:
    """_freshness_note emits a remediation note only for stale/absent; fresh and unknown stay silent."""
    assert _freshness_note("stale") == (_FRESHNESS_STALE,)
    assert _freshness_note("absent") == (_FRESHNESS_ABSENT,)
    assert _freshness_note("fresh") == ()
    assert _freshness_note("unknown") == ()


def test_freshness_reports_bounded_state(assay_root: AssayHarness) -> None:
    """_freshness reports one of the four bounded states over the real tree, never raising."""
    assert _freshness(assay_root.settings) in {"fresh", "stale", "absent", "unknown"}


# --- [SELECTION]

register_law(_selection, "empty_pattern_selects_all")
register_law(_selection, "bare_tokens_pass_through_as_themes")
register_law(_selection, "dotted_tokens_pass_through_as_names")
register_law(_selection, "unknown_tokens_pass_through_unvalidated")


@pytest.mark.parametrize("pattern", ["", "all", "*", "  all  ", "all,*"])
def test_selection_empty_pattern_selects_all(pattern: str) -> None:
    assert msgspec.json.decode(_selection(pattern).encode()) == {"$type": "all"}


def test_selection_bare_tokens_pass_through_as_themes() -> None:
    """Bare comma tokens become a deduplicated, order-preserving themes payload."""
    assert msgspec.json.decode(_selection("blocks, blocks ,camera").encode()) == {"$type": "themes", "themes": ["blocks", "camera"]}


def test_selection_dotted_tokens_pass_through_as_names() -> None:
    """Any dotted token promotes the whole selection to names; the shell's typed zero-match fault owns stray tokens."""
    assert msgspec.json.decode(_selection("blocks.CoreRail, ui.Paint").encode()) == {"$type": "names", "names": ["blocks.CoreRail", "ui.Paint"]}
    assert msgspec.json.decode(_selection("blocks,ui.Paint").encode()) == {"$type": "names", "names": ["blocks", "ui.Paint"]}


def test_selection_unknown_tokens_pass_through_unvalidated() -> None:
    """No local roster: an unknown token reaches the host as-is instead of raising a local unsupported fault."""
    assert msgspec.json.decode(_selection("not-a-scenario").encode()) == {"$type": "themes", "themes": ["not-a-scenario"]}


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


def test_decode_empty_stdout_reads_process_log(tmp_path: Path) -> None:
    out_log = tmp_path / "out.log"
    out_log.write_bytes(_envelope(report_dir=str(tmp_path), scenarios=({"scenario": "analysis.NativeRail", "status": "ok"},)))

    done = receipt(
        ("rasm-bridge",), 0, stdout=b"", artifacts=(Artifact(id="out", kind=ArtifactKind.PROCESS, path=str(out_log), bytes=out_log.stat().st_size),)
    )

    envelope = _decode_envelope(done)
    assert envelope.status is RailStatus.OK
    assert envelope.report_dir == str(tmp_path)
    assert tuple(scenario.scenario for scenario in envelope.scenarios) == ("analysis.NativeRail",)


def test_completed_from_stdout_projects_status_notes_and_artifacts(tmp_path: Path) -> None:
    (tmp_path / "events").mkdir()
    (tmp_path / "captures" / "blocks.CoreRail").mkdir(parents=True)
    facts = tmp_path / "events" / "facts.jsonl"
    view = tmp_path / "captures" / "blocks.CoreRail" / "view.png"
    facts.write_text("{}", encoding="utf-8")
    view.write_bytes(b"png")
    (tmp_path / "ignore.bin").write_bytes(b"bin")
    (tmp_path / "bridge-certificate.json").write_bytes(
        msgspec.json.encode({
            "artifacts": [
                {
                    "id": "events/facts.jsonl",
                    "role": "spool",
                    "relativePath": "events/facts.jsonl",
                    "mediaType": "application/x-ndjson",
                    "bytes": 2,
                    "hash": {"algorithm": "sha256", "value": hashlib.sha256(facts.read_bytes()).hexdigest()},
                    "retention": "evidence",
                    "scenario": "blocks.CoreRail",
                },
                {
                    "id": "captures/blocks.CoreRail/view.png",
                    "role": "capture",
                    "relativePath": "captures/blocks.CoreRail/view.png",
                    "mediaType": "image/png",
                    "bytes": 3,
                    "hash": {"algorithm": "sha256", "value": hashlib.sha256(view.read_bytes()).hexdigest()},
                    "retention": "evidence",
                    "scenario": "blocks.CoreRail",
                },
            ]
        })
    )
    done = _completed_from_stdout(receipt(("rasm-bridge",), 0, stdout=_envelope(report_dir=str(tmp_path))))
    assert done.status is RailStatus.OK
    assert done.notes == (f"bridge.reportDir={tmp_path}",)
    assert {Path(artifact.path).name for artifact in done.artifacts} == {"facts.jsonl", "view.png"}


def test_faulted_maps_failed_completion_to_fault() -> None:
    fault = assert_error_status(_faulted(Ok(receipt(("rasm-bridge",), 1, status=RailStatus.FAILED, stderr=b"boom"))), RailStatus.FAULTED)
    assert "boom" in fault.message
    assert _faulted(Ok(receipt(("rasm-bridge",), 0, status=RailStatus.OK))).is_ok()


# --- [CLOSURE_AGGREGATION]

register_law(_scenario_closure, "finds_single_scenario_closure")
register_law(_scenario_closure, "missing_scenario_closure_faults")
register_law(_aggregate_closure, "aggregates_scenario_closure_with_cargo")


def test_scenario_closure_and_aggregate(assay_root: AssayHarness) -> None:
    scope = assay_root.scope(Claim.BRIDGE)
    root = Path(scope.ensure())
    _closure(root / "scenarios" / "bridge-closure.json", "Rasm.Scenarios.dll", "Core.dll")
    cargo = root / "bin" / "Cargo" / assay_root.settings.configuration.value.lower()
    cargo.mkdir(parents=True)
    (cargo / "Cargo.dll").write_bytes(b"")

    closure = assert_ok(_scenario_closure(scope))
    assert closure[0].name == "bridge-closure.json"
    assert "Rasm.Scenarios.dll" in {Path(assembly).name for assembly in closure[1].assemblies}

    target = assert_ok(_aggregate_closure(assay_root.settings, scope, closure))
    payload = msgspec.json.decode(target.read_bytes())
    assert {Path(row).name for row in payload["assemblies"]} == {"Rasm.Scenarios.dll", "Core.dll", "Cargo.dll"}
    assert payload["scenarioAssemblies"] == ["Rasm.Scenarios.dll"]
    assert payload["hostPlugins"] == ["b45a29b1-4343-4035-989e-044e8580d9cf"]
    (reference_root,) = payload["referenceRoots"]
    assert reference_root["assembly"] == "Rasm.Scenarios.dll"
    assert reference_root["path"].endswith("tests/csharp/scenarios/_references")


def test_missing_scenario_closure_faults(assay_root: AssayHarness) -> None:
    """A manifest naming only foreign assemblies never satisfies the scenario closure lookup."""
    scope = assay_root.scope(Claim.BRIDGE)
    _closure(Path(scope.ensure()) / "other" / "bridge-closure.json", "Rasm.Other.dll")
    fault = assert_error_status(_scenario_closure(scope), RailStatus.FAULTED)
    assert "Rasm.Scenarios.dll" in fault.message


def test_read_closure_fallback_shape_is_empty() -> None:
    empty = _ClosureManifest()
    assert empty.assemblies == ()
    assert empty.built_against == _HostFingerprint()


# --- [CLIENT_AND_LIFECYCLE]

register_law(client_run, "spawns_built_supervisor_binary_with_single_json_envelope")
register_law(client_run, "faults_without_built_supervisor")
register_law(bridge_lease, "serializes_bridge_resource")


def test_client_run_spawns_built_supervisor_binary(assay_root: AssayHarness) -> None:
    checks: list[Check] = []

    def _spawn(check: Check, **_kw: object) -> Result[Completed, Fault]:
        checks.append(check)
        return Ok(receipt(("supervisor",), 0, stdout=_envelope()))

    pivot = f"{assay_root.settings.configuration.value.lower()}_test-rid"
    binary = Path(str(ArtifactScope.build(assay_root.settings, "bridge").path)) / "bin" / "Supervisor" / pivot / "Rasm.Bridge.Supervisor"
    binary.parent.mkdir(parents=True, exist_ok=True)
    binary.write_bytes(b"")
    done = assert_ok(client_run(assay_root.settings, "status", executor=SeamExecutor(run_fn=_spawn)))
    check = checks[0]
    assert done.status is RailStatus.OK
    assert check.tool.command[0] == str(binary)
    assert check.tool.command[-1] == "status"
    assert check.cwd == assay_root.root


def test_client_run_faults_without_built_supervisor(assay_root: AssayHarness) -> None:
    # Lane-less executor: the absent-binary fault must land before any spawn.
    fault = assert_error_status(client_run(assay_root.settings, "status", executor=SeamExecutor()), RailStatus.FAULTED)
    assert "bridge build" in fault.message


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
    report = assert_ok(verb_fn(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), SeamExecutor()))
    assert report.claim is Claim.BRIDGE
    assert report.verb == verb_name
    assert report.status is RailStatus.OK


register_law(BridgeLifecycle, "lifecycle_detail_projects_host_and_capabilities")


def test_lifecycle_detail_projects_host_and_capabilities(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A lifecycle fold projects the supervisor host versions and capability rows into a wire-round-tripping BridgeLifecycle detail."""
    envelope = _envelope(
        report_dir="report/status",
        host={"bundleVersion": "9.0", "rhinoCommonVersion": "9.0", "grasshopper2Version": "", "runtimeVersion": "10.0"},
        capabilities=({"key": "rail.core", "outcome": "ok", "receipt": "warm"}, {"key": "rail.vectors", "outcome": "skipped", "receipt": ""}),
    )
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    monkeypatch.setattr(
        _bridge_mod, "client_run", lambda _settings, *args, **_kw: Ok(receipt(("rasm-bridge", *args), 0, stdout=envelope, status=RailStatus.OK))
    )
    report = assert_ok(status(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), SeamExecutor()))
    detail = report.detail
    assert isinstance(detail, BridgeLifecycle)
    assert (detail.verb, detail.report_dir) == ("status", "report/status")
    # Empty grasshopper2 version is elided; surviving rows keep fingerprint order.
    assert detail.host == (("bundle", "9.0"), ("rhinoCommon", "9.0"), ("runtime", "10.0"))
    # Capability admission rows keep their key/outcome/receipt triple.
    assert detail.capabilities == (("rail.core", "ok", "warm"), ("rail.vectors", "skipped", ""))
    assert validate_detail(detail) == detail, "BridgeLifecycle did not survive the tagged-union wire codec"


register_law(build, "folds_bridge_build_receipt")


def test_build_folds_bridge_build_receipt(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(_bridge_mod, "_build_closure", lambda _settings, _executor: Ok(receipt(("rasm-bridge-build",), 0, status=RailStatus.OK)))
    report = assert_ok(build(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), SeamExecutor()))
    assert report.claim is Claim.BRIDGE
    assert report.verb == "build"
    assert report.status is RailStatus.OK
    assert report.results == ()


# --- [VERIFY]

register_law(verify, "folds_session_summary")
register_law(verify, "empty_corpus_short_circuits_unsupported")
register_law(verify, "non_empty_corpus_proceeds_past_guard")


def test_verify_folds_session_summary(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    assay_root.write("tests/csharp/scenarios/Blocks/CoreRail.cs", "// scenario source")
    closure = tmp_path / "bridge-closure.assay.json"
    closure.write_text("{}", encoding="utf-8")
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    monkeypatch.setattr(_bridge_mod, "_build_closure", lambda _settings, _executor: Ok(receipt(("rasm-bridge-build",), 0, status=RailStatus.OK)))
    monkeypatch.setattr(_bridge_mod, "_scenario_closure", lambda _scope: Ok((closure, _ClosureManifest())))
    monkeypatch.setattr(_bridge_mod, "_aggregate_closure", lambda _settings, _scope, _closure, **_kw: Ok(closure))
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

    report = assert_ok(verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(paths=("blocks",)), SeamExecutor()))
    assert report.claim is Claim.BRIDGE
    assert report.verb == "verify"
    assert isinstance(report.detail, VerifySummary)
    assert report.detail.facts
    assert report.detail.facts[0][0] == "blocks.CoreRail"


def test_verify_empty_corpus_short_circuits_unsupported(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """With zero scenario sources, verify neither builds nor launches Rhino and reports typed UNSUPPORTED."""

    def _forbidden(*_a: object, **_k: object) -> object:
        raise AssertionError("empty corpus must not build or launch Rhino")

    monkeypatch.setattr(_bridge_mod, "_build_closure", _forbidden)
    monkeypatch.setattr(_bridge_mod, "client_run", _forbidden)
    for params in (BridgeParams(), BridgeParams(evidence="author")):
        report = assert_ok(verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), params, SeamExecutor()))
        assert report.status is RailStatus.UNSUPPORTED
        assert any("scenario corpus empty" in note for note in report.notes)
        wire = assay_root.envelope_of(report, claim=Claim.BRIDGE, verb="verify")
        assert wire.exit_code == RailStatus.UNSUPPORTED.exit_code


def test_verify_non_empty_corpus_proceeds_past_guard(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """One scenario source defeats the guard: verify reaches the build stage."""
    assay_root.write("tests/csharp/scenarios/Blocks/CoreRail.cs", "// scenario source")
    monkeypatch.setattr(_bridge_mod, "leased", _leased_bypass)
    reached: list[str] = []

    def _probe(_settings: AssaySettings, _executor: object) -> Result[object, Fault]:
        reached.append("build")
        return Error(Fault(("rasm-bridge-build",), RailStatus.FAULTED, "stop after the guard"))

    monkeypatch.setattr(_bridge_mod, "_build_closure", _probe)
    assert verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), SeamExecutor()).is_error()
    assert reached == ["build"]


def test_scenario_artifacts_tolerates_missing_report_dir(tmp_path: Path) -> None:
    assert _scenario_artifacts(tmp_path / "missing") == ()
