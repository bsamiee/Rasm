"""Law matrix for the supervisor-backed ``tools.assay.rails.bridge`` rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable
import hashlib
from pathlib import Path
from typing import Literal, TYPE_CHECKING

from expression import Error, Ok, Result
import msgspec
import pytest

from tests.python._testkit.spec import assert_error_status, assert_ok
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.composition.settings import AssaySettings
from tools.assay.composition.store import ArtifactScope
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BridgeLifecycle,
    Claim,
    Fault,
    Mode,
    RailStatus,
    receipt,
    Report,
    validate_detail,
    VerifySummary,
)
from tools.assay.rails.bridge import (
    _aggregate_closure,
    _completed_from_stdout,
    _decode_envelope,
    _evidence_status,
    _EvidenceCertificate,
    _faulted,
    _freshness,
    _FRESHNESS_ABSENT,
    _freshness_note,
    _FRESHNESS_STALE,
    _INSTALLED_PLUGIN_GLOB,
    _reference_problems,
    _REFERENCE_UNPROMOTED_PREFIX,
    _ReferenceEvidenceResult,
    _scenario_artifacts,
    _scenario_closure,
    _selection,
    bridge_lease,
    BridgeParams,
    build,
    client_run,
    first_fault,
    quit as bridge_quit,
    RHINO_LINE_MAJOR,
    status,
    verify,
)


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.exec import Executor
    from tools.assay.core.model import Check, Completed


# --- [TYPES] ----------------------------------------------------------------------------

type _BridgeVerb = Callable[[AssaySettings, ArtifactScope, BridgeParams, Executor], Result[Report, Fault]]

# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (BridgeParams, bridge_lease, build, client_run, first_fault, bridge_quit, status, verify)

_LIFECYCLE_VERBS: tuple[tuple[str, _BridgeVerb], ...] = (("quit", bridge_quit), ("status", status))

_SELECTION_CASES: tuple[tuple[str, str, dict[str, object]], ...] = (
    ("empty", "", {"$type": "all"}),
    ("all-tokens", "all,*", {"$type": "all"}),
    ("padded-all", "  all  ", {"$type": "all"}),
    ("bare-themes-dedup", "blocks, blocks ,camera", {"$type": "themes", "themes": ["blocks", "camera"]}),
    ("dotted-promotes-names", "blocks.CoreRail, ui.Paint", {"$type": "names", "names": ["blocks.CoreRail", "ui.Paint"]}),
    ("mixed-promotes-names", "blocks,ui.Paint", {"$type": "names", "names": ["blocks", "ui.Paint"]}),
    ("unknown-passes-through", "not-a-scenario", {"$type": "themes", "themes": ["not-a-scenario"]}),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


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


def _bridge_run(envelope: bytes, verbs: list[str] | None = None, build_outcome: Result[Completed, Fault] | None = None) -> Callable[..., object]:
    """Executor run lane dispatching on the bridge tool mode: BUILD folds a receipt, VERIFY plays the supervisor envelope.

    Returns:
        Run-lane callable for ``SeamExecutor(run_fn=...)``.
    """

    def run(check: Check, **_kw: object) -> Result[Completed, Fault]:
        match check.tool.mode:
            case Mode.BUILD:
                return build_outcome if build_outcome is not None else Ok(receipt(("dotnet", "build"), 0, status=RailStatus.OK))
            case _:
                verbs.append(str(check.args.verb)) if verbs is not None else None
                return Ok(receipt(tuple(check.args.fill(check.tool.command)), 0, stdout=envelope, status=RailStatus.OK))

    return run


# --- [BRIDGE_PARAMS]


def test_bridge_params_pattern_and_bound() -> None:
    """The first positional path binds as the selection pattern; verify accepts an empty pattern."""
    assert not BridgeParams().pattern
    assert BridgeParams(paths=("blocks",)).pattern == "blocks"
    assert BridgeParams().bound("verify") == BridgeParams()


def test_freshness_note_maps_state_to_remediation() -> None:
    """_freshness_note emits a remediation note only for stale/absent; fresh and unknown stay silent."""
    assert _freshness_note("stale") == (_FRESHNESS_STALE,)
    assert _freshness_note("absent") == (_FRESHNESS_ABSENT,)
    assert _freshness_note("fresh") == ()
    assert _freshness_note("unknown") == ()


def test_freshness_reports_bounded_state(assay_root: AssayHarness) -> None:
    """_freshness reports one of the four bounded states over the real tree, never raising."""
    assert _freshness(assay_root.settings) in {"fresh", "stale", "absent", "unknown"}


def test_installed_plugin_glob_derives_from_rhino_line_major() -> None:
    """The installed-plugin probe derives its packages segment from RHINO_LINE_MAJOR, mirroring the supervisor's BundleInfo.RhinoLineMajor anchor."""
    assert RHINO_LINE_MAJOR == 9
    assert f"/packages/{RHINO_LINE_MAJOR}.0/" in _INSTALLED_PLUGIN_GLOB


# --- [SELECTION]


@pytest.mark.parametrize("label, pattern, expected", _SELECTION_CASES, ids=[label for label, _, _ in _SELECTION_CASES])
def test_selection_projects_pattern_to_payload(label: str, pattern: str, expected: dict[str, object]) -> None:
    """_selection maps all/theme/name tokens to the typed selection payload; unknown tokens pass through unvalidated."""
    assert msgspec.json.decode(_selection(pattern).encode()) == expected, f"[{label}]"


# --- [SESSION_ENVELOPE]


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
    rows = (("events/facts.jsonl", "spool", "application/x-ndjson", "{}"), ("captures/blocks.CoreRail/view.png", "capture", "image/png", "png"))
    for rel, _role, _media, payload in rows:
        target = tmp_path / rel
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(payload.encode())
    (tmp_path / "ignore.bin").write_bytes(b"bin")
    (tmp_path / "bridge-certificate.json").write_bytes(
        msgspec.json.encode({
            "artifacts": [
                {
                    "id": rel,
                    "role": role,
                    "relativePath": rel,
                    "mediaType": media,
                    "bytes": len(payload),
                    "hash": {"algorithm": "sha256", "value": hashlib.sha256(payload.encode()).hexdigest()},
                    "retention": "evidence",
                    "scenario": "blocks.CoreRail",
                }
                for rel, role, media, payload in rows
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
    assert "evidenceMode" not in payload, "argv owns the evidence mode; the closure manifest never carries it"
    (reference_root,) = payload["referenceRoots"]
    assert reference_root["assembly"] == "Rasm.Scenarios.dll"
    assert reference_root["path"].endswith("tests/csharp/scenarios/_references")


def test_missing_scenario_closure_faults(assay_root: AssayHarness) -> None:
    """A manifest naming only foreign assemblies never satisfies the scenario closure lookup."""
    scope = assay_root.scope(Claim.BRIDGE)
    _closure(Path(scope.ensure()) / "other" / "bridge-closure.json", "Rasm.Other.dll")
    fault = assert_error_status(_scenario_closure(scope), RailStatus.FAULTED)
    assert "Rasm.Scenarios.dll" in fault.message


# --- [CLIENT_AND_LIFECYCLE]


def test_client_run_spawns_built_supervisor_binary(assay_root: AssayHarness) -> None:
    checks: list[Check] = []

    def _spawn(check: Check, **_kw: object) -> Result[Completed, Fault]:
        checks.append(check)
        return Ok(receipt(("supervisor",), 0, stdout=_envelope()))

    binary = assay_root.supervisor()
    done = assert_ok(client_run(assay_root.settings, "status", executor=SeamExecutor(run_fn=_spawn)))
    check = checks[0]
    filled = check.args.fill(check.tool.command)
    assert done.status is RailStatus.OK
    assert filled[0] == str(binary)
    assert filled[-1] == "status"
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
def test_lifecycle_verbs_fold_supervisor_completion(verb_name: str, verb_fn: _BridgeVerb, assay_root: AssayHarness) -> None:
    """Lifecycle verbs ride the real lease and the built supervisor binary, folding the executor completion."""
    assay_root.supervisor()
    verbs: list[str] = []
    executor = SeamExecutor(run_fn=_bridge_run(_envelope(), verbs))
    report = assert_ok(verb_fn(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), executor))
    assert report.claim is Claim.BRIDGE
    assert report.verb == verb_name
    assert report.status is RailStatus.OK
    assert verbs == [verb_name]


def test_lifecycle_detail_projects_host_and_capabilities(assay_root: AssayHarness) -> None:
    """A lifecycle fold projects the supervisor host versions and capability rows into a wire-round-tripping BridgeLifecycle detail."""
    envelope = _envelope(
        report_dir="report/status",
        host={"bundleVersion": "9.0", "rhinoCommonVersion": "9.0", "grasshopper2Version": "", "runtimeVersion": "10.0"},
        capabilities=({"key": "rail.core", "outcome": "ok", "receipt": "warm"}, {"key": "rail.vectors", "outcome": "skipped", "receipt": ""}),
    )
    assay_root.supervisor()
    report = assert_ok(status(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), SeamExecutor(run_fn=_bridge_run(envelope))))
    detail = report.detail
    assert isinstance(detail, BridgeLifecycle)
    assert (detail.verb, detail.report_dir) == ("status", "report/status")
    # Empty grasshopper2 version is elided; surviving rows keep fingerprint order.
    assert detail.host == (("bundle", "9.0"), ("rhinoCommon", "9.0"), ("runtime", "10.0"))
    # Capability admission rows keep their key/outcome/receipt triple.
    assert detail.capabilities == (("rail.core", "ok", "warm"), ("rail.vectors", "skipped", ""))
    assert validate_detail(detail) == detail, "BridgeLifecycle did not survive the tagged-union wire codec"


def test_build_folds_bridge_build_receipt(assay_root: AssayHarness) -> None:
    """Build folds the per-project executor receipts into one bridge-build report."""
    report = assert_ok(build(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), SeamExecutor(run_fn=_bridge_run(_envelope()))))
    assert report.claim is Claim.BRIDGE
    assert report.verb == "build"
    assert report.status is RailStatus.OK
    assert report.results == ()


# --- [VERIFY]


def test_verify_folds_session_summary(assay_root: AssayHarness) -> None:
    """Verify drives build, closure aggregation, and the supervisor session end-to-end over the executor port."""
    assay_root.write("tests/csharp/scenarios/Blocks/CoreRail.cs", "// scenario source")
    scope = assay_root.scope(Claim.BRIDGE)
    # The verify prelude reads the closure from the bridge BUILD scope, not the claim scope.
    build_root = Path(str(ArtifactScope.build(assay_root.settings, "bridge").path))
    _closure(build_root / "scenarios" / "bridge-closure.json", "Rasm.Scenarios.dll")
    cargo = build_root / "bin" / "Cargo" / assay_root.settings.configuration.value.lower()
    cargo.mkdir(parents=True)
    (cargo / "Cargo.dll").write_bytes(b"")
    assay_root.supervisor()
    envelope = _envelope(
        scenarios=({"scenario": "blocks.CoreRail", "status": "ok", "durationMs": 1.0},),
        evidence=({"$type": "fact", "stamp": {"scenario": "blocks.CoreRail"}, "key": "mesh.count", "value": 3},),
    )
    verbs: list[str] = []

    report = assert_ok(verify(assay_root.settings, scope, BridgeParams(paths=("blocks",)), SeamExecutor(run_fn=_bridge_run(envelope, verbs))))
    assert report.claim is Claim.BRIDGE
    assert report.verb == "verify"
    assert verbs == ["verify"]
    assert isinstance(report.detail, VerifySummary)
    assert report.detail.facts
    assert report.detail.facts[0][0] == "blocks.CoreRail"


def test_verify_empty_corpus_short_circuits_unsupported(assay_root: AssayHarness) -> None:
    """With zero scenario sources, verify neither builds nor spawns: the lane-less executor would fail loudly if reached."""
    for params in (BridgeParams(), BridgeParams(evidence="author")):
        report = assert_ok(verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), params, SeamExecutor()))
        assert report.status is RailStatus.UNSUPPORTED
        assert any("scenario corpus empty" in note for note in report.notes)
        wire = assay_root.envelope_of(report, claim=Claim.BRIDGE, verb="verify")
        assert wire.exit_code == RailStatus.UNSUPPORTED.exit_code


def test_verify_non_empty_corpus_proceeds_past_guard(assay_root: AssayHarness) -> None:
    """One scenario source defeats the guard: verify reaches the build stage and propagates its fault."""
    assay_root.write("tests/csharp/scenarios/Blocks/CoreRail.cs", "// scenario source")
    stop = Error(Fault(("rasm-bridge-build",), RailStatus.FAULTED, "stop after the guard"))
    executor = SeamExecutor(run_fn=_bridge_run(_envelope(), build_outcome=stop))
    outcome = verify(assay_root.settings, assay_root.scope(Claim.BRIDGE), BridgeParams(), executor)
    assert "stop after the guard" in assert_error_status(outcome, RailStatus.FAULTED).message


def test_evidence_status_reference_lanes() -> None:
    """_evidence_status folds reference problems honestly: unpromoted-only degrades, any promoted-corpus problem faults, alone or mixed."""
    unpromoted = ("reference.unpromoted:blocks.CoreRail", "reference.unpromoted:ui.Paint")
    mismatch = ("reference.mismatch:blocks.CoreRail",)
    rows: tuple[tuple[str, Literal["verify", "author"], RailStatus, bool, tuple[str, ...], RailStatus], ...] = (
        ("clean-verify-ok", "verify", RailStatus.OK, True, (), RailStatus.OK),
        ("unpromoted-only-degrades", "verify", RailStatus.OK, True, unpromoted, RailStatus.DEGRADED),
        ("promoted-mismatch-faults", "verify", RailStatus.OK, True, mismatch, RailStatus.FAULTED),
        ("mixed-unpromoted-and-mismatch-faults", "verify", RailStatus.OK, True, (*unpromoted, *mismatch), RailStatus.FAULTED),
        ("certificate-problem-faults", "verify", RailStatus.OK, False, (), RailStatus.FAULTED),
        ("author-always-candidate", "author", RailStatus.OK, True, unpromoted, RailStatus.CANDIDATE),
        ("supervisor-verdict-passes-through", "verify", RailStatus.FAILED, True, unpromoted, RailStatus.FAILED),
    )
    for label, evidence, done, certificate_ok, problems, expected in rows:
        got = _evidence_status(evidence=evidence, done=done, certificate_ok=certificate_ok, reference_problems=problems)
        assert got is expected, f"[{label}] {got.value} != {expected.value}"


def test_reference_problems_bind_the_unpromoted_prefix() -> None:
    """_reference_problems emits admission-prefixed rows: only unpromoted rows carry the lane prefix, matched rows vanish."""
    certificate = _EvidenceCertificate(
        references=(
            _ReferenceEvidenceResult(scenario="blocks.CoreRail", admission="unpromoted"),
            _ReferenceEvidenceResult(scenario="camera.Sweep", admission="matched", matched=True),
            _ReferenceEvidenceResult(scenario="ui.Paint", admission="mismatch"),
        )
    )
    problems = _reference_problems(certificate)
    assert problems == ("reference.unpromoted:blocks.CoreRail", "reference.mismatch:ui.Paint")
    assert [problem.startswith(_REFERENCE_UNPROMOTED_PREFIX) for problem in problems] == [True, False]


def test_scenario_artifacts_tolerates_missing_report_dir(tmp_path: Path) -> None:
    assert _scenario_artifacts(tmp_path / "missing") == ()
