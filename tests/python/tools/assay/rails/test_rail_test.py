"""Law matrix for test params, coverage, roster, dispatch, and mutation gate."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import dataclasses
from pathlib import Path
from types import SimpleNamespace
from typing import TYPE_CHECKING

from dirty_equals import IsInt
from expression import Ok
from expression.collections import block
import msgspec
from mutmut import configuration as mutmut_configuration
import pytest

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error_status, assert_ok, refutes, support_matrix, validity_matrix, ValidityCase
from tools.assay.composition.catalog import TOOLS
from tools.assay.composition.settings import ArtifactScope
from tools.assay.core.engine import apply_row_status, exclusive_lease
from tools.assay.core.model import (
    ArtifactKind,
    Check,
    Claim,
    Fault,
    fold,
    Input,
    Language,
    Mode,
    MutationLane,
    receipt,
    Runner,
    Stage,
    TestRun,
    Tool,
    ToolGroup,
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
from tools.assay.rails import test as test_rail
from tools.assay.rails.mutation_gate import _tally, gate
from tools.assay.rails.test import (
    _adopt_coverage,
    _checks,
    _classified_projects,
    _detail,
    _dispatch,
    _dispatch_all,
    _eligible,
    _filter,
    _gate,
    _GATE_TOOL,
    _roster_matches,
    _routed,
    _scoped_mutation,
    _select,
    _TestProjectLane,
    _thin_rail,
    _unsupported_scope,
    coverage_percent,
    TestParams,
)


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.model import Completed


# --- [CONSTANTS] ------------------------------------------------------------------------

_PY = Language.PYTHON
_PY_ROUTED = Routed(language=_PY, scope=Scope.CHANGED)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok(argv: tuple[str, ...], stdout: bytes = b"", *, status: RailStatus = RailStatus.OK) -> Completed:
    return receipt(argv, 0, status=status, stdout=stdout)


def _seed_coverage_json(root: Path, payload: bytes) -> Path:
    """Write coverage.json at the canonical artifact path under root.

    Returns:
        Path to the written coverage.json file.
    """
    target = root / ".artifacts" / "python" / "coverage" / "coverage.json"
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(payload)
    return target


def _wire(monkeypatch: pytest.MonkeyPatch, *outcomes: Completed, routed: Routed = _PY_ROUTED, seam: str = "_dispatch") -> None:
    monkeypatch.setattr(test_rail, "_routed", lambda *_a, **_k: Ok(block.of_seq((routed,))))
    monkeypatch.setattr(test_rail, seam, lambda *_a, **_k: tuple(Ok(o) for o in outcomes))


def _capture_rail(monkeypatch: pytest.MonkeyPatch) -> list[tuple[TestParams, object, object]]:
    seen: list[tuple[TestParams, object, object]] = []

    def _fake(settings: object, scope: object, params: TestParams, *, claim: object, verb: object, mode: object) -> Result[object, object]:
        seen.append((params, claim, verb))
        return Ok(object())

    monkeypatch.setattr(test_rail, "_thin_rail", _fake)
    return seen


def _tool(name: str = "pytest", mode: Mode = Mode.RUN, runner: Runner = Runner.UV, language: Language = _PY) -> Tool:
    """Build a minimal TEST catalog row for eligibility laws, carrying the catalog's policy groups for the name.

    Returns:
        TEST catalog row for ``name`` with the catalog's policy groups.
    """
    groups = next((t.groups for t in TOOLS if t.name == name and t.claim is Claim.TEST), ())
    return Tool(name=name, runner=runner, command=(name,), input=Input.NONE, language=language, claim=Claim.TEST, mode=mode, groups=groups)


def _mutation_tool(name: str = "mutmut") -> Tool:
    return _tool(name, Mode.MUTATION, Runner.UV)


def _stryker_tool() -> Tool:
    """Build the dotnet-stryker mutation row used by scoped-mutation laws.

    Returns:
        Stryker mutation catalog row.
    """
    return Tool(
        name="dotnet-stryker",
        runner=Runner.DOTNET,
        command=("tool", "run", "dotnet-stryker", "--"),
        input=Input.PROJECT,
        language=Language.CSHARP,
        claim=Claim.TEST,
        mode=Mode.MUTATION,
    )


def _seed_solution(assay_root: AssayHarness) -> tuple[str, ...]:
    projects = (
        "libs/csharp/Rasm/Rasm.csproj",
        "tests/csharp/_architecture/Rasm.Architecture.Tests.csproj",
        "tests/csharp/_benchmarks/Rasm.Benchmarks.csproj",
        "tests/csharp/_testkit/Rasm.TestKit.csproj",
        "tests/csharp/libs/Rasm/Rasm.Tests.csproj",
        "tests/csharp/libs/Rasm.Empty/Rasm.Empty.Tests.csproj",
        "tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj",
        "tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj",
        "tests/csharp/tools/cs-analyzer/Csp.Analyzer.Tests.csproj",
        "tests/csharp/tools/rhino-bridge/Contract/Contract.csproj",
        "tests/csharp/tools/rhino-bridge/Scenarios/Scenarios.csproj",
        "tests/csharp/tools/rhino-bridge/Supervisor/Supervisor.csproj",
    )
    assay_root.write(
        "Workspace.slnx",
        "<Solution>"
        + "".join(f'<Folder Name="/{Path(project).parent.as_posix()}/"><Project Path="{project}" /></Folder>' for project in projects)
        + "</Solution>",
    )
    assay_root.write(
        "tests/csharp/libs/Rasm/Rasm.Tests.csproj", "<Project><PropertyGroup><AssayHostBound>true</AssayHostBound></PropertyGroup></Project>"
    )
    assay_root.write(
        "tests/csharp/libs/Rasm.Empty/Rasm.Empty.Tests.csproj",
        "<Project><PropertyGroup><IsTestProject>false</IsTestProject></PropertyGroup></Project>",
    )
    for project in projects:
        path = assay_root.root / project
        if not path.exists():
            assay_root.write(project, "<Project />")
    return projects


# --- [COMPOSITION] ----------------------------------------------------------------------
# Law coverage requires every exported symbol to appear in register_law.

register_law(coverage_percent, "coverage_percent_from_json")
register_law(coverage_percent, "coverage_percent_absent_or_malformed_none")
register_law(coverage_percent, "adopt_coverage_artifact_fields")

register_law(TestParams, "testparams_default_invariants")
register_law(TestParams, "testparams_frozen")
register_law(TestParams, "testparams_mutation_lane_validity")
register_law(TestParams, "testparams_coverage_benchmark_exclusive_intent")
register_law(TestParams, "eligible_branch_matrix")
register_law(TestParams, "filter_discriminant_arms")
register_law(TestParams, "scoped_mutation_arms")
register_law(TestParams, "scoped_mutation_governor_caps_max_children")
register_law(TestParams, "scoped_mutation_mutmut_changed_lane_globs")
register_law(TestParams, "checks_splice_arms")
register_law(TestParams, "unsupported_scope_arms")
register_law(TestParams, "select_solution_admission_arms")
register_law(TestParams, "select_default_arm_routed_closure_untouched")
register_law(TestParams, "select_classifies_solution_project_lanes")
register_law(TestParams, "dispatch_empty_checks_returns_unsupported_only")
register_law(TestParams, "dispatch_non_empty_checks_calls_fan_out")
register_law(TestParams, "roster_matches_skip_and_kind_arms")
register_law(TestParams, "detail_mutation_and_coverage_arms")

register_law(test_rail.coverage, "coverage_forces_coverage_flag")
register_law(test_rail.coverage, "coverage_verb_claim_is_test")

register_law(test_rail.list, "list_report_projection_arms")
register_law(test_rail.list, "list_discovery_failure_changes_status")
register_law(test_rail.list, "list_empty_discovery_counts_as_unresolved")
register_law(test_rail.list, "list_roster_artifact_written_to_scope")
register_law(test_rail.list, "list_detail_selected_is_pre_limit_discovered_total")

register_law(test_rail.run, "run_envelope_names_host_routed_projects")


def test_run_envelope_names_host_routed(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Run notes name host-routed projects excluded from TEST dispatch."""
    routed = Routed(
        language=Language.CSHARP, scope=Scope.CHANGED, projects=("src/App/App.csproj", "src/Lib/Lib.csproj"), host_bound=("src/App/App.csproj",)
    )
    _wire(monkeypatch, _ok(("dotnet", "test")), routed=routed, seam="_dispatch_all")
    report = assert_ok(test_rail.run(assay_root.settings, assay_root.scope(Claim.TEST), TestParams(language=Language.CSHARP)))
    assert "closure[csharp]: included=1 excluded=0 cached=0 host-routed=1" in report.notes
    assert "host-routed[csharp]: src/App/App.csproj" in report.notes


register_law(test_rail.run, "run_ok_report_from_dispatch")
register_law(test_rail.run, "run_mutation_gap_note_emitted")
register_law(test_rail.run, "run_coverage_percent_populates_detail")
register_law(test_rail.run, "run_claim_is_test")
register_law(test_rail.run, "dispatch_all_mode_arms")
register_law(test_rail.run, "thin_rail_gap_note_emitted_in_report_notes")
register_law(test_rail.run, "thin_rail_mutation_eligible_calls_leased")
register_law(test_rail.run, "thin_rail_multi_language_mutation_nests_sorted_leases")
register_law(test_rail.run, "thin_rail_per_language_mutation_leases_do_not_contend")
register_law(test_rail.run, "thin_rail_no_mutation_calls_work_directly")
register_law(test_rail.run, "routed_yields_ok_for_valid_languages")
register_law(test_rail.run, "routed_multi_language_combines_results")

register_law(gate, "gate_seeded_cache_matrix")
register_law(gate, "gate_tool_groups_splice_not_catalog")
register_law(test_rail.run, "gate_check_rides_staged_mutmut_success")
register_law(test_rail.run, "gate_skips_without_success_or_stage")
register_law(test_rail.run, "dispatch_all_gate_rides_mutation_results")

# --- [LAWS_COVERAGE_PERCENT]


@pytest.mark.parametrize(
    "payload,expected",
    [
        pytest.param(b'{"totals": {"percent_covered": 87.5}}', 87.5, id="float-percent"),
        pytest.param(b'{"totals": {"percent_covered": 100}}', 100.0, id="int-percent-coerced-to-float"),
        pytest.param(b'{"totals": {"percent_covered": 0.0}}', 0.0, id="zero-percent"),
        pytest.param(b'{"totals": {"percent_covered": 72.345}, "files": {}}', 72.345, id="extra-keys-ignored"),
        pytest.param(b'{"meta": {}}', None, id="missing-totals-none"),
        pytest.param(b'{"totals": {}}', None, id="missing-percent-none"),
        pytest.param(b"{bad json", None, id="malformed-json-none"),
        pytest.param(None, None, id="absent-file-none"),
    ],
)
def test_coverage_percent_from_json(tmp_path: Path, payload: bytes | None, expected: float | None) -> None:
    """coverage_percent decodes totals and degrades missing or malformed files to None."""
    if payload is not None:
        _seed_coverage_json(tmp_path, payload)
    result = coverage_percent(tmp_path)
    assert result is None if expected is None else result == pytest.approx(expected)


# --- [LAWS_ADOPT_COVERAGE]


def test_adopt_coverage_artifact_fields(assay_root: AssayHarness) -> None:
    """_adopt_coverage reads the file, stores it, and populates artifact id/kind/bytes/lines."""
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    root = assay_root.root / ".artifacts" / "python" / "coverage"
    root.mkdir(parents=True)
    cov_file = root / "coverage.json"
    content = b'{"totals": {"percent_covered": 87.5}}\n'
    cov_file.write_bytes(content)

    artifact = _adopt_coverage(scope.store, assay_root.settings.run_id, cov_file)
    assert (artifact.id, artifact.kind, artifact.bytes, artifact.lines) == ("coverage.json", ArtifactKind.TEST, len(content), 1)


# --- [LAWS_TESTPARAMS]


def test_testparams_default_invariants() -> None:
    """TestParams default construction produces a coherent zero-state parameter bag."""
    p = TestParams()
    support_matrix(
        ("target is None", lambda: p.target is None, True),
        ("all is False", lambda: p.all is False, True),
        ("csharp flag is False", lambda: p.csharp is False, True),
        ("python flag is False", lambda: p.python is False, True),
        ("typescript flag is False", lambda: p.typescript is False, True),
        ("mutation is OFF", lambda: p.mutation is MutationLane.OFF, True),
        ("benchmark is False", lambda: p.benchmark is False, True),
        ("coverage is False", lambda: p.coverage is False, True),
        ("filter is empty", lambda: not p.filter, True),
        ("limit is 0", lambda: p.limit == 0, True),
        ("grep is empty", lambda: not p.grep, True),
    )


def test_testparams_rejects_multiple_language_flags() -> None:
    """TestParams rejects ambiguous language selectors."""
    fault = TestParams(csharp=True, typescript=True).bound("run")
    assert isinstance(fault, Fault)
    assert "--csharp" in fault.message
    assert "--typescript" in fault.message


def test_test_help_exposes_boolean_language_flags(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Test help exposes boolean language selectors and omits the removed --language value flag."""
    from tools.assay import __main__ as main_mod  # noqa: PLC0415

    neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
    monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
    code = main_mod.main(["test", "run", "--help"])
    cap = capsysbinary.readouterr()
    assert code == 0
    assert b"--csharp" in cap.out
    assert b"--python" in cap.out
    assert b"--typescript" in cap.out
    assert b"--language" not in cap.out


def test_testparams_frozen() -> None:
    """TestParams is frozen: mutation attempts raise FrozenInstanceError (dataclass contract)."""
    field = "coverage"  # runtime-determined attr name; satisfies ruff B010 (no constant-literal setattr)
    with pytest.raises(dataclasses.FrozenInstanceError):
        setattr(TestParams(), field, True)


def test_testparams_mutation_lane_validity() -> None:
    """TestParams.mutation accepts every MutationLane member, each producing a distinct object."""
    validity_matrix(
        [ValidityCase(label=lane.name, value=lane, expected=True) for lane in MutationLane], lambda lane: TestParams(mutation=lane).mutation is lane
    )


def test_testparams_coverage_benchmark_exclusive_intent() -> None:
    """TestParams accepts coverage+benchmark; the rail resolves exclusivity."""
    p = TestParams(coverage=True, benchmark=True)
    assert (p.coverage, p.benchmark) == (True, True)


# --- [LAWS_ELIGIBLE]


@pytest.mark.parametrize(
    "tool,params,expected",
    [
        pytest.param(_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.OFF), False, id="mutation-OFF→F"),
        pytest.param(_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.CHANGED), True, id="mutation-CHANGED→T"),
        pytest.param(_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.FULL), True, id="mutation-FULL→T"),
        pytest.param(_tool("dotnet-build", Mode.BUILD, Runner.DOTNET), TestParams(), True, id="build-default→T"),
        pytest.param(_tool("pytest"), TestParams(benchmark=False, coverage=False), True, id="pytest-plain→T"),
        pytest.param(_tool("pytest"), TestParams(coverage=True), False, id="pytest-coverage→F"),
        pytest.param(_tool("pytest"), TestParams(benchmark=True), False, id="pytest-benchmark→F"),
        pytest.param(_tool("coverage"), TestParams(coverage=False), False, id="coverage-no-flag→F"),
        pytest.param(_tool("coverage"), TestParams(coverage=True), True, id="coverage-flag→T"),
        pytest.param(_tool("coverage-json"), TestParams(coverage=False), False, id="coverage-json-no-flag→F"),
        pytest.param(_tool("coverage-json"), TestParams(coverage=True), True, id="coverage-json-flag→T"),
        pytest.param(_tool("pytest-benchmark"), TestParams(benchmark=False), False, id="pytest-benchmark-no-flag→F"),
        pytest.param(_tool("pytest-benchmark"), TestParams(benchmark=True), True, id="pytest-benchmark-flag→T"),
    ],
)
def test_eligible_branch_matrix(tool: Tool, params: TestParams, expected: bool) -> None:  # noqa: FBT001 -- bool param injected by parametrize matrix
    """_eligible projects every (mode, params) arm to the correct True/False gate."""
    assert _eligible(tool, params) is expected


# --- [LAWS_FILTER]


@pytest.mark.parametrize(
    "expr,expected",
    [
        pytest.param("", (), id="empty→empty"),
        pytest.param("   ", (), id="whitespace→empty"),
        pytest.param("/SomeTrait", ("--filter-query", "/SomeTrait"), id="query-slash"),
        pytest.param("  /SomeTrait  ", ("--filter-query", "/SomeTrait"), id="query-trimmed"),
        pytest.param("Category=unit", ("--filter-trait", "Category=unit"), id="trait-kv"),
        pytest.param("MyTests", ("--filter-class", "*MyTests*"), id="class-Tests-suffix"),
        pytest.param("MyLaws", ("--filter-class", "*MyLaws*"), id="class-Laws-suffix"),
        pytest.param("MySpec", ("--filter-class", "*MySpec*"), id="class-Spec-suffix"),
        pytest.param("My+Class", ("--filter-class", "*My+Class*"), id="class-plus-sign"),
        pytest.param("test_something", ("--filter-method", "*test_something*"), id="plain-method"),
        pytest.param("test_edge_case", ("--filter-method", "*test_edge_case*"), id="another-method"),
    ],
)
def test_filter_discriminant_arms(expr: str, expected: tuple[str, ...]) -> None:
    """_filter maps each filter expression to the correct MTP flag tuple."""
    assert _filter(expr) == expected


# --- [LAWS_SCOPED_MUTATION]


def _command_law(produced: Tool | None, expected: tuple[str, ...]) -> None:
    """Assert scoped mutation produced the exact expected command."""
    assert produced is not None, "scoped tool dropped out of the lane unexpectedly"
    assert produced.command == expected, f"command {produced.command} != {expected}"


def test_scoped_mutation_arms(assay_root: AssayHarness) -> None:
    """_scoped_mutation applies changed/full and scoped/unscoped arms."""
    settings = assay_root.settings.model_copy(update={"mutation_max_cpu": 2})
    mutmut, stryker, plain, unscoped = _mutation_tool(), _stryker_tool(), _tool("pytest", Mode.RUN), _mutation_tool("vitest-mut")
    files = ("src/Foo.cs", "src/Bar.cs")

    assert _scoped_mutation(unscoped, TestParams(mutation=MutationLane.CHANGED), settings, ("src/foo.py",)) is None
    assert _scoped_mutation(plain, TestParams(mutation=MutationLane.CHANGED), settings, ()) is plain

    full = _scoped_mutation(mutmut, TestParams(mutation=MutationLane.FULL), settings, ("src/foo.py",))
    _command_law(full, ("mutmut", "--max-children", "2"))
    changed = _scoped_mutation(mutmut, TestParams(mutation=MutationLane.CHANGED), settings, ("src/a.py", "src/b.py"))
    _command_law(changed, ("mutmut", "--max-children", "2", "src.a.*", "src.b.*"))

    scoped = _scoped_mutation(stryker, TestParams(mutation=MutationLane.CHANGED), settings, files)
    _command_law(scoped, ("tool", "run", "dotnet-stryker", "--", "--mutate", "src/Foo.cs", "--mutate", "src/Bar.cs"))

    # Wrong command shapes prove the transform is non-vacuous.
    refutes(changed, _command_law, ("mutmut", "src/a.py", "src/b.py"))  # passthrough: no governor, no module-name glob
    refutes(changed, _command_law, ("mutmut", "--max-children", "2", "src/a.py*", "src/b.py*"))  # path-shaped glob: matches zero mutant NAMES
    refutes(full, _command_law, ("mutmut",))  # governor dropped
    refutes(scoped, _command_law, ("tool", "run", "dotnet-stryker", "--", "src/Foo.cs", "src/Bar.cs"))  # --mutate flag dropped


# --- [LAWS_CHECKS]


def test_checks_splice_arms(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_checks splices DOTNET RUN filters and leaves UV/MUTATION rows scoped."""
    settings = assay_root.settings.model_copy(update={"mutation_max_cpu": 2})
    dotnet = Tool(
        name="dotnet-test", runner=Runner.DOTNET, command=("test",), input=Input.PROJECT, language=Language.CSHARP, claim=Claim.TEST, mode=Mode.RUN
    )

    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (dotnet,))
    csharp_routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, files=())
    spliced = _checks(csharp_routed, TestParams(filter="test_something"), settings, Mode.RUN)
    assert any("--filter-method" in c.tool.command for c in spliced)

    mutmut = _mutation_tool()
    py_routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/a.py",))
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (mutmut,))
    changed_checks = _checks(py_routed, TestParams(mutation=MutationLane.CHANGED), settings, Mode.MUTATION)
    assert [c.tool.command for c in changed_checks] == [("mutmut", "--max-children", "2", "src.a.*")]

    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (_mutation_tool("vitest-mut"),))
    assert _checks(py_routed, TestParams(mutation=MutationLane.CHANGED), settings, Mode.MUTATION) == ()

    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (mutmut,))
    monkeypatch.setattr(test_rail, "_scoped_mutation", lambda t, *_a, **_k: t)
    mut_checks = _checks(py_routed, TestParams(filter="MyTests"), settings, Mode.MUTATION)
    assert all("--filter-class" not in c.tool.command for c in mut_checks)

    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (_tool("pytest", Mode.RUN, Runner.UV),))
    uv_checks = _checks(py_routed, TestParams(filter="test_something"), settings, Mode.RUN)
    assert len(uv_checks) == 1
    assert "--filter-method" not in uv_checks[0].tool.command


def test_checks_pytest_scope_pin(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Explicit [paths...] leave the pytest family unpinned for file tails; the changed default and --all pin an empty tail."""
    pytest_row = Tool(
        name="pytest", runner=Runner.UV, command=("pytest",), input=Input.FILES, language=Language.PYTHON, claim=Claim.TEST, mode=Mode.RUN
    )
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (pytest_row,))
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=("tests/t_a.py",))

    (suite_wide,) = _checks(routed, TestParams(), assay_root.settings, Mode.RUN)
    assert suite_wide.tail == ()
    (all_wide,) = _checks(routed, TestParams(paths=("tests",), all=True), assay_root.settings, Mode.RUN)
    assert all_wide.tail == ()
    (scoped,) = _checks(routed, TestParams(paths=("tests",)), assay_root.settings, Mode.RUN)
    assert scoped.tail is None


register_law(_checks, "pytest_scope_pins_empty_tail_without_explicit_paths")


@pytest.mark.parametrize(
    "signature, rc, stdout, stderr, expected_status",
    [
        ((5, b""), 5, b"", b"no tests ran", RailStatus.EMPTY),  # pytest exit 5 → empty scope on the returncode alone
        ((1, b"No test files found"), 1, b"", b"No test files found, exiting with code 1", RailStatus.EMPTY),  # vitest stderr marker
        ((1, b"No test files found"), 1, b"No test files found\n", b"", RailStatus.EMPTY),  # marker on stdout admits too
        ((1, b"No test files found"), 1, b"", b"2 tests failed", RailStatus.FAILED),  # rc matches, marker absent → genuine failure
        ((5, b""), 1, b"", b"", RailStatus.FAILED),  # rc mismatch → signature never fires
        (None, 5, b"", b"", RailStatus.BUSY),  # signature-less row keeps the raw returncode projection
    ],
    ids=[
        "pytest_exit5_empty",
        "vitest_marker_stderr",
        "vitest_marker_stdout",
        "marker_absent_failed",
        "rc_mismatch_failed",
        "no_signature_unchanged",
    ],
)
def test_apply_row_status_empty_signature(
    signature: tuple[int, bytes] | None, rc: int, stdout: bytes, stderr: bytes, expected_status: RailStatus
) -> None:
    """A row's (returncode, marker) empty signature maps a nothing-to-do receipt to EMPTY; any mismatch keeps the tool verdict."""
    tool = Tool(
        name="vitest",
        runner=Runner.PNPM,
        command=("vitest", "run"),
        input=Input.NONE,
        language=Language.TYPESCRIPT,
        claim=Claim.TEST,
        mode=Mode.RUN,
        empty_signature=signature,
    )
    done = receipt(("vitest", "run"), rc, stdout=stdout, stderr=stderr)
    assert apply_row_status(tool, done).status is expected_status


def test_catalog_test_runners_carry_empty_signature() -> None:
    """Every RUN/LIST test runner whose tool signals no-eligible-work declares the signature on its catalog row."""
    expected = {
        ("pytest", Mode.RUN): (5, b""),
        ("pytest", Mode.LIST): (5, b""),
        ("pytest-benchmark", Mode.RUN): (5, b""),
        ("coverage", Mode.RUN): (5, b""),
        ("vitest", Mode.RUN): (1, b"No test files found"),
    }
    rows = {(t.name, t.mode): t.empty_signature for t in TOOLS if (t.name, t.mode) in expected}
    assert rows == expected


register_law(apply_row_status, "empty_signature_maps_no_eligible_work_to_empty")
register_law(apply_row_status, "test_runner_rows_declare_empty_signature")


# --- [LAWS_UNSUPPORTED_SCOPE]


def test_unsupported_scope_arms(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """_unsupported_scope emits only CHANGED unscoped mutation receipts."""
    unscoped = _mutation_tool("vitest-mut")
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/foo.py",))
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (unscoped,))

    results = _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), assay_root.settings, Mode.MUTATION)
    assert len(results) == 1
    completed = assert_ok(results[0])
    assert completed.status is RailStatus.UNSUPPORTED
    assert any("vitest-mut" in n for n in completed.notes)

    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (_mutation_tool(),))
    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), assay_root.settings, Mode.MUTATION) == ()

    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (unscoped,))
    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.FULL), assay_root.settings, Mode.MUTATION) == ()
    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), assay_root.settings, Mode.RUN) == ()

    host = Routed(
        Language.CSHARP,
        Scope.CHANGED,
        projects=("tests/csharp/libs/Rasm/Rasm.Tests.csproj",),
        host_bound=("tests/csharp/libs/Rasm/Rasm.Tests.csproj",),
    )
    host_receipt = assert_ok(_unsupported_scope(host, TestParams(), assay_root.settings, Mode.RUN)[0])
    assert host_receipt.status is RailStatus.UNSUPPORTED


# --- [LAWS_SELECT]


def test_select_solution_admission_arms(assay_root: AssayHarness) -> None:
    """_select handles passthrough, target narrowing, and solution-backed all-selection."""
    settings = assay_root.settings
    _seed_solution(assay_root)
    rasm = "tests/csharp/libs/Rasm/Rasm.Tests.csproj"
    py_routed = Routed(language=_PY, scope=Scope.CHANGED, projects=(rasm,))
    cs_routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=(rasm,))
    target = Path("tests/csharp/libs/Other/Other.Tests.csproj")
    assay_root.write(target, "<Project />")

    assert _select(py_routed, TestParams(), settings) is py_routed
    assert _select(cs_routed, TestParams(), settings) is cs_routed
    assert _select(cs_routed, TestParams(target=target), settings).projects == (str(target),)
    selected = _select(cs_routed, TestParams(all=True), settings)
    assert selected.scope is Scope.FULL
    assert rasm in selected.host_bound
    assert {
        "tests/csharp/_architecture/Rasm.Architecture.Tests.csproj",
        "tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj",
        "tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj",
        "tests/csharp/tools/cs-analyzer/Csp.Analyzer.Tests.csproj",
        "tests/csharp/tools/rhino-bridge/Contract/Contract.csproj",
        "tests/csharp/tools/rhino-bridge/Scenarios/Scenarios.csproj",
        "tests/csharp/tools/rhino-bridge/Supervisor/Supervisor.csproj",
    } <= set(selected.projects)
    assert "tests/csharp/_testkit/Rasm.TestKit.csproj" not in selected.projects
    assert "tests/csharp/_benchmarks/Rasm.Benchmarks.csproj" not in selected.projects
    assert "tests/csharp/libs/Rasm.Empty/Rasm.Empty.Tests.csproj" not in selected.projects
    assert "libs/csharp/Rasm/Rasm.csproj" not in selected.projects


def test_select_default_arm_routed_closure_untouched(assay_root: AssayHarness) -> None:
    """_select default arm preserves the routed C# closure identity."""
    settings = assay_root.settings
    new_owner = "tests/csharp/apps/Future/Future.Tests.csproj"
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=(new_owner,))

    selected = _select(routed, TestParams(), settings)

    assert selected is routed, "default arm must pass the routed closure through identically"
    assert selected.projects == (new_owner,)


def test_select_classifies_solution_project_lanes(assay_root: AssayHarness) -> None:
    """The solution roster classifies managed, host, support, benchmark, and non-test lanes."""
    _seed_solution(assay_root)
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED)
    rows = _classified_projects(TestParams(all=True), routed, assay_root.settings)
    counts = {lane.value: total for lane in _TestProjectLane if (total := sum(1 for _, actual in rows if actual is lane))}
    assert counts == {"managed": 7, "host_bound": 1, "support": 1, "benchmark": 1, "non_test": 2}


# --- [LAWS_DISPATCH]


def test_dispatch_empty_checks_returns_unsupported_only(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_dispatch with no eligible checks returns only the unsupported receipts."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/foo.py",))
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    monkeypatch.setattr(test_rail, "_checks", lambda *_a, **_k: ())
    monkeypatch.setattr(test_rail, "_unsupported_scope", lambda *_a, **_k: (Ok(object()),))
    results = _dispatch(routed, TestParams(), settings=assay_root.settings, scope=scope, mode=Mode.MUTATION)
    assert len(results) == 1


def test_dispatch_non_empty_checks_calls_fan_out(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_dispatch forwards non-empty checks to fan_out and concatenates the unsupported tail."""
    ok = _ok(("pytest",))
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    monkeypatch.setattr(test_rail, "_checks", lambda *_a, **_k: (Check(tool=_tool("pytest", Mode.RUN), paths=()),))
    monkeypatch.setattr(test_rail, "_unsupported_scope", lambda *_a, **_k: ())
    monkeypatch.setattr(test_rail, "fan_out", lambda *_a, **_k: (Ok(ok),))
    results = _dispatch(routed, TestParams(), settings=assay_root.settings, scope=scope, mode=Mode.RUN)
    assert results == (Ok(ok),)


# --- [LAWS_ROSTER_MATCHES]


def test_roster_matches_skip_and_kind_arms() -> None:
    """_roster_matches skips headers/summaries and non-success discovery."""
    mixed = _ok(("dotnet", "test", "--list-tests"), b"The following tests are available:\nMyNs.MyClass.test_one\n3 test(s) found\n")
    matches = _roster_matches((mixed,))
    assert [m.id for m in matches] == ["MyNs.MyClass.test_one"]
    assert all(m.kind is ArtifactKind.PROCESS for m in matches)

    assert _roster_matches((receipt(("dotnet", "test", "--list-tests"), 1, stdout=b"X.test\n"),)) == ()
    assert _roster_matches((_ok(("pytest", "--collect-only")),)) == ()


# --- [LAWS_DETAIL]


@pytest.mark.parametrize(
    "done,params,seed,check",
    [
        pytest.param((), TestParams(mutation=MutationLane.OFF, coverage=False), None, None, id="off+no-coverage→None"),
        pytest.param(
            (_ok(("mutmut", "run"), b"not-json\n"),), TestParams(mutation=MutationLane.FULL, coverage=False), None, None, id="full+noise-stdout→None"
        ),
        pytest.param((_ok(("uv", "run", "pytest")),), TestParams(mutation=MutationLane.OFF, coverage=True), None, None, id="coverage+no-json→None"),
        pytest.param(
            (_ok(("uv", "run", "pytest")),),
            TestParams(mutation=MutationLane.OFF, coverage=True),
            b'{"totals": {"percent_covered": 91.5}}',
            91.5,
            id="coverage+json→TestRun",
        ),
    ],
)
def test_detail_mutation_and_coverage_arms(
    tmp_path: Path, done: tuple[Completed, ...], params: TestParams, seed: bytes | None, check: float | None
) -> None:
    """_detail projects mutation and coverage combinations into TestRun or None."""
    if seed is not None:
        _seed_coverage_json(tmp_path, seed)
    result = _detail(done, params, tmp_path)
    match check:
        case None:
            assert result is None
        case _:
            assert isinstance(result, TestRun)
            assert result.coverage == pytest.approx(check)


# --- [LAWS_COVERAGE_VERB]


def test_coverage_forces_coverage_flag(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Coverage verb forces coverage=True and benchmark=False even when TestParams disagrees."""
    seen = _capture_rail(monkeypatch)
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    test_rail.coverage(assay_root.settings, scope, TestParams(coverage=False, benchmark=True))
    assert len(seen) == 1
    params, _, _ = seen[0]
    assert (params.coverage, params.benchmark) == (True, False)


def test_coverage_verb_claim_is_test(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Coverage verb passes Claim.TEST and verb='coverage' through to _thin_rail."""
    seen = _capture_rail(monkeypatch)
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    test_rail.coverage(assay_root.settings, scope, TestParams())
    assert [(claim, verb) for _, claim, verb in seen] == [(Claim.TEST, "coverage")]


# --- [LAWS_LIST_VERB]


@pytest.mark.parametrize(
    "params,stdout,assertion",
    [
        pytest.param(
            TestParams(),
            b"tests/a.py::test_one\ntests/a.py::test_two\n",
            lambda r: r.counts.total == IsInt(ge=1) and "test-roster" in {a.id for a in r.artifacts},
            id="ok-report+roster-artifact",
        ),
        pytest.param(
            TestParams(grep="ALPHA"),
            b"tests/a.py::test_alpha\ntests/a.py::test_beta\n",
            lambda r: [row.id for row in r.results] == ["tests/a.py::test_alpha"],
            id="grep-filters-roster-case-insensitive",
        ),
        pytest.param(
            TestParams(limit=1),
            b"tests/a.py::test_one\ntests/a.py::test_two\n",
            lambda r: r.counts.total == 1 and any("total=2" in n and "returned=1" in n for n in r.notes),
            id="limit-trims-roster+discovery-note",
        ),
    ],
)
def test_list_report_projection_arms(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, params: TestParams, stdout: bytes, assertion: Callable[[object], bool]
) -> None:
    """List projects roster artifacts, grep filtering, and limit notes."""
    _wire(monkeypatch, _ok(("pytest", "--collect-only"), stdout))
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), params))
    assert assertion(report)


def test_list_discovery_failure_changes_status(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List discovery failures affect status and results instead of hiding in notes."""
    failed = receipt(("dotnet", "test", "--list-tests"), 1, stderr=b"no project")
    _wire(monkeypatch, failed)
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()))
    assert report.status is RailStatus.FAILED
    assert any(row.severity == "failed" for row in report.results)
    assert any("discovery" in n and "dotnet test" in n for n in report.notes)


def test_list_empty_discovery_counts_as_unresolved(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List with empty discovery stays explicit in TestRun discovery counts."""
    _wire(monkeypatch, _ok(("pytest", "--collect-only")))
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()))
    assert isinstance(report.detail, TestRun)
    assert ("empty_or_failed_discovery", 1) in report.detail.discovery_counts


def test_list_roster_artifact_written_to_scope(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List writes the roster artifact into the scope store and surfaces it in report.artifacts."""
    _wire(monkeypatch, _ok(("pytest", "--collect-only"), b"tests/a.py::test_one\n"))
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()))
    assert "test-roster" in {a.id for a in report.artifacts}


def test_list_detail_selected_is_pre_limit_discovered_total(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List detail.selected keeps the pre-limit discovered total."""
    _wire(monkeypatch, _ok(("pytest", "--collect-only"), b"tests/a.py::test_one\ntests/a.py::test_two\ntests/a.py::test_three\n"))
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams(limit=1)))
    assert isinstance(report.detail, TestRun)
    assert report.detail.selected == 3, "detail.selected is the pre-limit discovered total, not the post-limit roster of 1"
    assert report.counts.total == 1, "the wire roster is trimmed to --limit while detail.selected keeps the full discovered count"


# --- [LAWS_RUN_VERB]


def test_run_ok_report_from_dispatch(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run produces an Ok report when dispatch completes without fault."""
    _wire(monkeypatch, _ok(("pytest",)), seam="_dispatch_all")
    report = assert_ok(test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()))
    assert report.status in {RailStatus.OK, RailStatus.EMPTY}


def test_run_mutation_gap_note_emitted(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run emits a mutation gap note when the language has no eligible runner."""
    routed = Routed(language=Language.TYPESCRIPT, scope=Scope.CHANGED)
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")
    params = TestParams(mutation=MutationLane.FULL, language=Language.TYPESCRIPT)
    report = assert_ok(test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), params))
    assert any("mutation" in n for n in report.notes)


def test_run_coverage_percent_populates_detail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run with coverage=True decodes coverage.json under settings.root into the TestRun detail."""
    _seed_coverage_json(assay_root.root, b'{"totals": {"percent_covered": 82.5}}')
    _wire(monkeypatch, _ok(("uv", "run", "pytest")), seam="_dispatch_all")
    report = assert_ok(test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams(coverage=True)))
    assert isinstance(report.detail, TestRun)
    assert report.detail.coverage == pytest.approx(82.5)


def test_run_claim_is_test(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run passes Claim.TEST through _thin_rail so the envelope claim is correct."""
    seen = _capture_rail(monkeypatch)
    test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams())
    assert [claim for _, claim, _ in seen] == [Claim.TEST]


# --- [LAWS_DISPATCH_ALL]


@pytest.mark.parametrize(
    "params,expected_modes",
    [
        pytest.param(TestParams(mutation=MutationLane.FULL), {Mode.RUN, Mode.MUTATION}, id="mutation→adds-MUTATION"),
        pytest.param(TestParams(coverage=True), {Mode.RUN, Mode.CLIENT}, id="coverage→adds-CLIENT"),
        pytest.param(TestParams(), {Mode.RUN}, id="plain→RUN-only"),
    ],
)
def test_dispatch_all_mode_arms(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, params: TestParams, expected_modes: set[Mode]) -> None:
    """_dispatch_all adds mutation and coverage sub-dispatches from params."""
    call_modes: list[Mode] = []

    def _record(*_a: object, mode: Mode, **_k: object) -> tuple[Result[Completed, object], ...]:
        call_modes.append(mode)
        return (Ok(_ok(("pytest",))),)

    monkeypatch.setattr(test_rail, "_dispatch", _record)
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _dispatch_all(routed, params, settings=assay_root.settings, scope=scope, mode=Mode.RUN)
    assert set(call_modes) == expected_modes


# --- [LAWS_ROUTED]


def test_routed_yields_ok_for_valid_languages(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_routed sequences routing results per language and returns Ok with one Routed when all succeed."""
    monkeypatch.setattr(test_rail, "route", lambda lang, *_a, **_k: Ok(Routed(language=lang, scope=Scope.CHANGED, files=())))
    result = _routed((_PY,), (), assay_root.settings)
    routed_list = list(assert_ok(result))
    assert [r.language for r in routed_list] == [_PY]


def test_routed_multi_language_combines_results(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_routed with multiple languages produces a Block containing one Routed per language."""
    monkeypatch.setattr(test_rail, "route", lambda lang, *_a, **_k: Ok(Routed(language=lang, scope=Scope.CHANGED, files=())))
    result = _routed((_PY, Language.TYPESCRIPT), (), assay_root.settings)
    assert {r.language for r in assert_ok(result)} == {_PY, Language.TYPESCRIPT}


# --- [LAWS_THIN_RAIL]


def test_thin_rail_gap_note_emitted_in_report_notes(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail emits mutation gap notes when no eligible runner exists."""
    routed = Routed(language=Language.TYPESCRIPT, scope=Scope.CHANGED, files=())
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    params = TestParams(mutation=MutationLane.FULL, language=Language.TYPESCRIPT)
    report = assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert any("mutation" in n for n in report.notes)


def test_thin_rail_mutation_eligible_calls_leased(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail leases mutation-<lang> when mutation tools are eligible."""
    leased_calls: list[str] = []
    ok = _ok(("mutmut", "run"))
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    canned_report = fold(Claim.TEST, "run", (ok,))
    _wire(monkeypatch, ok, routed=routed, seam="_dispatch_all")

    def _record_lease(resource: str, *_a: object, **_k: object) -> Result[object, object]:
        leased_calls.append(resource)
        return Ok(canned_report)

    monkeypatch.setattr(test_rail, "leased", _record_lease)

    params = TestParams(mutation=MutationLane.FULL, language=Language.PYTHON)
    assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert leased_calls == ["mutation-python"]


def test_thin_rail_multi_language_mutation_nests_sorted_leases(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail nests multi-language mutation leases in sorted order."""
    leased_calls: list[str] = []
    ok = _ok(("mutmut", "run"))
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _wire(monkeypatch, ok, routed=routed, seam="_dispatch_all")
    monkeypatch.setattr(test_rail, "resolve_languages", lambda *_a, **_k: Ok((Language.CSHARP, _PY)))

    def _descend(resource: str, action: Callable[[object], Result[object, object]], **_k: object) -> Result[object, object]:
        leased_calls.append(resource)
        return action(object())

    monkeypatch.setattr(test_rail, "leased", _descend)

    params = TestParams(mutation=MutationLane.FULL)
    assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert leased_calls == ["mutation-csharp", "mutation-python"]


def test_thin_rail_per_language_mutation_leases_do_not_contend(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Per-language mutation leases isolate csharp and python contention."""
    ok = _ok(("mutmut", "run"))
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _wire(monkeypatch, ok, routed=routed, seam="_dispatch_all")
    params = TestParams(mutation=MutationLane.FULL, language=Language.PYTHON)
    with exclusive_lease("mutation-csharp", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    with exclusive_lease("mutation-python", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_error_status(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN), RailStatus.BUSY)


def test_thin_rail_no_mutation_calls_work_directly(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail with mutation=OFF calls _work() directly (no lease), returning Ok."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    params = TestParams(mutation=MutationLane.OFF, language=Language.PYTHON)
    report = assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert report.status in {RailStatus.OK, RailStatus.EMPTY}


# --- [LAWS_MUTATION_GATE]


def _seed_cache(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, exit_codes: dict[str, int | None]) -> None:
    """Synthesize mutmut's persisted result surface: cwd pyproject + source tree + mutants/ meta cache."""
    (tmp_path / "src").mkdir()
    (tmp_path / "src" / "mod.py").write_text("X = 1\n")
    (tmp_path / "pyproject.toml").write_text('[tool.mutmut]\nsource_paths = ["src"]\n')
    (tmp_path / "mutants" / "src").mkdir(parents=True)
    meta = {"exit_code_by_key": exit_codes, "durations_by_key": {}, "estimated_durations_by_key": {}}
    (tmp_path / "mutants" / "src" / "mod.py.meta").write_bytes(msgspec.json.encode(meta))
    monkeypatch.chdir(tmp_path)
    monkeypatch.setattr(mutmut_configuration, "_config", None)


def _staged_mutmut() -> Tool:
    return Tool(
        name="mutmut",
        runner=Runner.UV,
        command=("mutmut", "run"),
        input=Input.NONE,
        language=_PY,
        claim=Claim.TEST,
        mode=Mode.MUTATION,
        groups=(ToolGroup.MUTATION,),
        stage=Stage(root=".artifacts/python/mutmut/work", project=True),
    )


@pytest.mark.parametrize(
    "codes,rc,killed,survived,selected",
    [
        pytest.param({"a": 1, "b": 1, "c": 1, "d": 1, "e": 0}, 0, 4, 1, 5, id="floor-edge-0.800→PASS"),
        pytest.param({"a": 1, "b": 0}, 1, 1, 1, 2, id="score-0.5→FAIL"),
        pytest.param({"a": 1, "b": 33, "c": 36, "d": None}, 0, 1, 0, 4, id="unscored-excluded-from-denominator"),
        pytest.param({}, 1, 0, 0, 0, id="empty-cache→FAIL"),
    ],
)
def test_gate_seeded_cache_matrix(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    capsys: pytest.CaptureFixture[str],
    codes: dict[str, int | None],
    rc: int,
    killed: int,
    survived: int,
    selected: int,
) -> None:
    """Mutation gate keeps tally, JSON line, verdict stream, and exit code aligned."""
    _seed_cache(tmp_path, monkeypatch, codes)
    tally = _tally()
    assert (tally["killed"], tally["survived"], tally.total()) == (killed, survived, selected)
    code = gate()
    out, err = capsys.readouterr()
    lines = out.splitlines()
    assert (code, len(lines)) == (rc, 1)
    assert err.startswith("[PASS]" if rc == 0 else "[FAIL]")
    done = receipt(("uv", "run", "python", "-m", "tools.assay.rails.mutation_gate"), code, stdout=lines[0].encode())
    assert _detail((done,), TestParams(mutation=MutationLane.FULL), tmp_path) == TestRun(
        mutation=MutationLane.FULL, killed=killed, survived=survived, selected=selected
    )


def test_gate_tool_groups_splice_not_catalog() -> None:
    """_GATE_TOOL is lease-riding only and absent from the catalog census."""
    assert _GATE_TOOL not in TOOLS
    assert all(t.name != _GATE_TOOL.name for t in TOOLS)
    assert (_GATE_TOOL.runner, _GATE_TOOL.groups, _GATE_TOOL.mode) == (Runner.UV, (ToolGroup.MUTATION,), Mode.MUTATION)
    assert (_GATE_TOOL.stage.root, _GATE_TOOL.stage.project) == ("", True)
    assert _GATE_TOOL.input is Input.OWNED
    assert _GATE_TOOL.command == ("python", "-m", "tools.assay.rails.mutation_gate")


def test_gate_check_rides_staged_mutmut_success(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_gate fans one staged-worktree _GATE_TOOL check after mutmut success."""
    staged = _staged_mutmut()
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (staged,))
    seen: list[Check] = []

    def _fake_fan_out(checks: tuple[Check, ...], **_k: object) -> tuple[Result[Completed, object], ...]:
        seen.extend(checks)
        return (Ok(_ok(("uv", "run", "python", "-m", "tools.assay.rails.mutation_gate"))),)

    monkeypatch.setattr(test_rail, "fan_out", _fake_fan_out)
    done = (Ok(_ok(("uv", "run", "--group", "mutation", "mutmut", "run"))),)
    out = _gate(done, _PY_ROUTED, TestParams(mutation=MutationLane.FULL), settings=assay_root.settings, scope=assay_root.scope(Claim.TEST))
    assert len(out) == 1
    assert [c.tool for c in seen] == [_GATE_TOOL]
    assert seen[0].cwd == Path(str(assay_root.settings.root)) / staged.stage.root


@pytest.mark.parametrize(
    "argv,code,staged",
    [
        pytest.param(("uv", "run", "mutmut", "run"), 1, True, id="mutmut-failed→no-gate"),
        pytest.param(("uv", "run", "pytest"), 0, True, id="non-mutmut-success→no-gate"),
        pytest.param(("uv", "run", "mutmut", "run"), 0, False, id="no-staged-row→no-gate"),
    ],
)
def test_gate_skips_without_success_or_stage(
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    argv: tuple[str, ...],
    code: int,
    staged: bool,  # noqa: FBT001  # parametrize matrix field, not a call-site positional bool
) -> None:
    """_gate stays empty without mutmut success and a staged row."""

    def _forbidden(*_a: object, **_k: object) -> tuple[Result[Completed, object], ...]:
        raise AssertionError("fan_out must not run")

    rows = (_staged_mutmut(),) if staged else (_mutation_tool(),)
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: rows)
    monkeypatch.setattr(test_rail, "fan_out", _forbidden)
    done = (Ok(receipt(argv, code)),)
    assert _gate(done, _PY_ROUTED, TestParams(mutation=MutationLane.FULL), settings=assay_root.settings, scope=assay_root.scope(Claim.TEST)) == ()


def test_dispatch_all_gate_rides_mutation_results(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_dispatch_all splices gate receipts after mutation results only."""
    mut_ok = Ok(_ok(("uv", "run", "mutmut", "run")))
    gate_ok = Ok(_ok(("uv", "run", "python", "-m", "tools.assay.rails.mutation_gate")))
    handed: list[tuple[Result[Completed, object], ...]] = []

    def _dispatch_stub(*_a: object, mode: Mode, **_k: object) -> tuple[Result[Completed, object], ...]:
        return (mut_ok,) if mode is Mode.MUTATION else ()

    def _gate_stub(done: tuple[Result[Completed, object], ...], *_a: object, **_k: object) -> tuple[Result[Completed, object], ...]:
        handed.append(done)
        return (gate_ok,)

    monkeypatch.setattr(test_rail, "_dispatch", _dispatch_stub)
    monkeypatch.setattr(test_rail, "_gate", _gate_stub)
    scope = assay_root.scope(Claim.TEST)
    out = _dispatch_all(_PY_ROUTED, TestParams(mutation=MutationLane.FULL), settings=assay_root.settings, scope=scope, mode=Mode.RUN)
    assert handed == [(mut_ok,)]
    assert out == (mut_ok, gate_ok)
    assert _dispatch_all(_PY_ROUTED, TestParams(), settings=assay_root.settings, scope=scope, mode=Mode.RUN) == ()
    assert handed == [(mut_ok,)]
