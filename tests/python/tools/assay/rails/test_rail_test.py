"""Law matrix for test params, coverage, roster, dispatch, TRX evidence, and mutation gate."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path
from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Ok
from expression.collections import block
import msgspec
from mutmut import configuration as mutmut_configuration
import pytest

from tests.python._testkit.spec import assert_error_status, assert_ok, refutes
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.composition.catalog import TOOLS
from tools.assay.composition.store import ArtifactScope
from tools.assay.core.exec import apply_row_status
from tools.assay.core.govern import exclusive_lease
from tools.assay.core.model import (
    ArtifactKind,
    Check,
    Claim,
    Fault,
    Input,
    Language,
    Mode,
    MutationLane,
    RailStatus,
    receipt,
    Runner,
    TestRun,
    Tool,
    ToolGroup,
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.rails import test as test_rail
from tools.assay.rails.mutation_gate import _tally, gate
from tools.assay.rails.test import (
    _adopt_coverage,
    _checks,
    _detail,
    _dispatch,
    _dispatch_all,
    _eligible,
    _filter,
    _gate,
    _mutation_args,
    _project_lane,
    _roster_matches,
    _routed,
    _rows,
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

COVERS: tuple[object, ...] = (TestParams, coverage_percent, test_rail.coverage, test_rail.list, test_rail.run, gate, apply_row_status)

_PY = Language.PYTHON
_PY_ROUTED = Routed(language=_PY, scope=Scope.CHANGED)
_SHELL_CSPROJ = "<Project><PropertyGroup><AssayTestShell>true</AssayTestShell></PropertyGroup></Project>"
_HOST_CSPROJ = "<Project><PropertyGroup><AssayHostBound>true</AssayHostBound></PropertyGroup></Project>"
_NON_TEST_CSPROJ = "<Project><PropertyGroup><IsTestProject>false</IsTestProject></PropertyGroup></Project>"
# A shell csproj that still looks like a full test project: the marker must win over its content.
_SHELL_WITH_CONTENT_CSPROJ = (
    "<Project><PropertyGroup><AssayTestShell>true</AssayTestShell></PropertyGroup>"
    '<ItemGroup><ProjectReference Include="../../../../libs/csharp/Rasm/Rasm.csproj" /></ItemGroup></Project>'
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok(argv: tuple[str, ...], stdout: bytes = b"", *, status: RailStatus = RailStatus.OK) -> Completed:
    return receipt(argv, 0, status=status, stdout=stdout)


def _row(name: str, mode: Mode) -> Tool:
    """Select one real TEST catalog row by (name, mode); laws compose argv from the catalog, never synthetic rows.

    Returns:
        The unique catalog row.
    """
    (row,) = tuple(t for t in TOOLS if t.name == name and t.mode is mode and t.claim is Claim.TEST)
    return row


def _tool(name: str = "pytest", mode: Mode = Mode.RUN, runner: Runner = Runner.UV, language: Language = _PY) -> Tool:
    """Build a minimal TEST row for pure-policy laws, carrying the catalog's policy groups for the name.

    Returns:
        TEST row for ``name`` with the catalog's policy groups.
    """
    groups = next((t.groups for t in TOOLS if t.name == name and t.claim is Claim.TEST), ())
    return Tool(name=name, runner=runner, command=(name,), input=Input.NONE, language=language, claim=Claim.TEST, mode=mode, groups=groups)


# The one justified synthetic row: no real mutation row lacks a CHANGED scope projection, but the
# degradation law (unscoped -> UNSUPPORTED, never a full-tree run) must hold for future runners.
def _unscoped_mutation() -> Tool:
    return _tool("vitest-mut", Mode.MUTATION)


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

    def _fake(
        _settings: object, _scope: object, params: TestParams, _executor: object, *, claim: object, verb: object, mode: object
    ) -> Result[object, object]:
        del mode
        seen.append((params, claim, verb))
        return Ok(object())

    monkeypatch.setattr(test_rail, "_thin_rail", _fake)
    return seen


def _seed_solution(assay_root: AssayHarness) -> tuple[str, ...]:
    """Seed Workspace.slnx plus the marker-bearing csproj roster; the Ghost project stays unwritten on purpose.

    Returns:
        Every project path admitted to the seeded solution.
    """
    projects = (
        "libs/csharp/Rasm/Rasm.csproj",
        "tests/csharp/_architecture/Rasm.Architecture.Tests.csproj",
        "tests/csharp/_benchmarks/Rasm.Benchmarks.csproj",
        "tests/csharp/_scenariokit/Rasm.ScenarioKit.csproj",
        "tests/csharp/_testkit/Rasm.TestKit.csproj",
        "tests/csharp/libs/Rasm/Rasm.Tests.csproj",
        "tests/csharp/libs/Rasm.Empty/Rasm.Empty.Tests.csproj",
        "tests/csharp/libs/Rasm.Ghost/Rasm.Ghost.Tests.csproj",
        "tests/csharp/libs/Rasm.Host/Rasm.Host.Tests.csproj",
        "tests/csharp/scenarios/Rasm.Scenarios.csproj",
        "tests/csharp/tools/cs-analyzer/Csp.Analyzer.Tests.csproj",
        "tests/csharp/tools/rhino-bridge/Contract/Contract.csproj",
        "tests/csharp/tools/rhino-bridge/Supervisor/Supervisor.csproj",
    )
    assay_root.write(
        "Workspace.slnx",
        "<Solution>"
        + "".join(f'<Folder Name="/{Path(project).parent.as_posix()}/"><Project Path="{project}" /></Folder>' for project in projects)
        + "</Solution>",
    )
    assay_root.write("tests/csharp/libs/Rasm/Rasm.Tests.csproj", _SHELL_WITH_CONTENT_CSPROJ)
    assay_root.write("tests/csharp/scenarios/Rasm.Scenarios.csproj", _SHELL_CSPROJ)
    assay_root.write("tests/csharp/libs/Rasm.Host/Rasm.Host.Tests.csproj", _HOST_CSPROJ)
    assay_root.write("tests/csharp/libs/Rasm.Empty/Rasm.Empty.Tests.csproj", _NON_TEST_CSPROJ)
    assay_root.write("tests/csharp/_scenariokit/Rasm.ScenarioKit.csproj", _NON_TEST_CSPROJ)
    for project in projects:
        path = assay_root.root / project
        if "Ghost" not in project and not path.exists():
            assay_root.write(project, "<Project />")
    return projects


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


def test_adopt_coverage_artifact_fields(assay_root: AssayHarness) -> None:
    """_adopt_coverage reads the file, stores it, and populates artifact id/kind/bytes/lines."""
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    cov_file = _seed_coverage_json(assay_root.root, b'{"totals": {"percent_covered": 87.5}}\n')
    artifact = _adopt_coverage(scope.store, assay_root.settings.run_id, cov_file)
    assert (artifact.id, artifact.kind, artifact.bytes, artifact.lines) == ("coverage.json", ArtifactKind.TEST, 38, 1)


# --- [LAWS_TESTPARAMS]


def test_testparams_rejects_multiple_language_flags() -> None:
    """TestParams rejects ambiguous language selectors."""
    fault = TestParams(csharp=True, typescript=True).bound("run")
    assert isinstance(fault, Fault)
    assert "--csharp" in fault.message
    assert "--typescript" in fault.message


def test_test_help_exposes_boolean_flags(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Test help exposes the boolean language selectors and the TRX flag, and omits the removed --language value flag."""
    from tools.assay import __main__ as main_mod  # noqa: PLC0415

    neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
    monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
    code = main_mod.main(["test", "run", "--help"])
    cap = capsysbinary.readouterr()
    assert code == 0
    assert all(flag in cap.out for flag in (b"--csharp", b"--python", b"--typescript", b"--trx"))
    assert b"--language" not in cap.out


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
    ],
)
def test_filter_discriminant_arms(expr: str, expected: tuple[str, ...]) -> None:
    """_filter maps each filter expression to the correct MTP flag tuple."""
    assert _filter(expr) == expected


# --- [LAWS_SCOPED_MUTATION]


def _filled_law(args: object, tool: Tool, expected: tuple[str, ...]) -> None:
    """Assert the typed splice values weave the row's holes into the exact expected argv body."""
    assert args is not None, "mutation args dropped out of the lane unexpectedly"
    assert args.fill(tool.command) == expected, f"filled {args.fill(tool.command)} != {expected}"  # type: ignore[attr-defined]


def test_mutation_args_compose_catalog_argv(assay_root: AssayHarness) -> None:
    """The real catalog mutation rows compose their full argv: governor cap, changed scope, and the staged Stryker anchors."""
    settings = assay_root.settings.model_copy(update={"mutation_max_cpu": 2})
    mutmut, stryker = _row("mutmut", Mode.MUTATION), _row("dotnet-stryker", Mode.MUTATION)

    assert _mutation_args(_unscoped_mutation(), TestParams(mutation=MutationLane.CHANGED), settings, ("src/foo.py",)) is None

    full = _mutation_args(mutmut, TestParams(mutation=MutationLane.FULL), settings, ("src/foo.py",))
    _filled_law(full, mutmut, ("mutmut", "run", "--max-children=2"))
    changed = _mutation_args(mutmut, TestParams(mutation=MutationLane.CHANGED), settings, ("src/a.py", "src/b.py"))
    _filled_law(changed, mutmut, ("mutmut", "run", "--max-children=2", "src.a.*", "src.b.*"))

    scoped = _mutation_args(stryker, TestParams(mutation=MutationLane.CHANGED), settings, ("src/Foo.cs", "src/Bar.cs"))
    assert scoped is not None
    output_dir = Path(str(settings.root)).resolve() / ".artifacts/csharp/stryker"
    assert (scoped.solution, scoped.output) == (str(settings.solution), str(output_dir))
    assert output_dir.is_dir(), "the rail pre-creates the Stryker report --output dir"
    _filled_law(
        scoped,
        stryker,
        (
            "tool",
            "run",
            "dotnet-stryker",
            "--",
            "--test-runner",
            "mtp",
            "--mutation-level",
            "Standard",
            "--config-file",
            ".config/stryker-config.json",
            "--solution",
            str(settings.solution),
            "--output",
            str(output_dir),
            "--mutate",
            "src/Foo.cs",
            "--mutate",
            "src/Bar.cs",
        ),
    )

    # Wrong filled shapes prove the transform is non-vacuous.
    refutes(changed, _filled_law, mutmut, ("mutmut", "run", "src/a.py", "src/b.py"))  # passthrough: no governor, no module-name glob
    refutes(changed, _filled_law, mutmut, ("mutmut", "run", "--max-children=2", "src/a.py*", "src/b.py*"))  # path-shaped glob: zero mutant NAMES
    refutes(full, _filled_law, mutmut, ("mutmut", "run"))  # governor dropped
    refutes(scoped, _filled_law, stryker, ("tool", "run", "dotnet-stryker", "--", "src/Foo.cs", "src/Bar.cs"))  # --mutate flag dropped


def test_mutation_rows_confine_every_path_to_artifacts() -> None:
    """No mutation row can write to repo root: stage roots, sandbox, and report outputs all live under .artifacts/."""
    mutmut, stryker = _row("mutmut", Mode.MUTATION), _row("dotnet-stryker", Mode.MUTATION)
    assert stryker.stage.root == ".artifacts/csharp/stryker/work", "Stryker cwd (and its .stryker-tmp sandbox) is the staged work root"
    assert mutmut.stage.root == ".artifacts/python/mutmut/work", "mutmut cwd (and its mutants/ cache) is the staged work root"
    assert stryker.command[8:10] == ("--config-file", ".config/stryker-config.json"), "config is read-only and pinned; discovery-by-cwd is forgone"
    # Every literal path token in both commands is a read-only .config anchor or a typed hole the rail fills under .artifacts.
    literal_paths = [t for row in (mutmut, stryker) for t in row.command if "/" in t and "{" not in t]
    assert literal_paths == [".config/stryker-config.json"]


# --- [LAWS_CHECKS]


def test_checks_splice_arms(assay_root: AssayHarness) -> None:
    """_checks routes DOTNET RUN filters into the typed {filter*} splice and leaves UV/MUTATION rows unfiltered."""
    settings = assay_root.settings.model_copy(update={"mutation_max_cpu": 2})
    csharp_routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=("tests/csharp/libs/A/A.Tests.csproj",))
    spliced = _checks(csharp_routed, TestParams(filter="test_something"), settings, Mode.RUN)
    assert [c.args.filter for c in spliced] == [("--filter-method", "*test_something*")]
    assert all(c.args.fill(c.tool.command) == ("test", "--minimum-expected-tests", "1", "--filter-method", "*test_something*") for c in spliced)
    assert all("{filter*}" in c.tool.command for c in spliced), "the row command is never edited; the hole carries the filter"

    py_routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/a.py",))
    changed_checks = _checks(py_routed, TestParams(mutation=MutationLane.CHANGED, filter="MyTests"), settings, Mode.MUTATION)
    assert [c.args.fill(c.tool.command) for c in changed_checks] == [("mutmut", "run", "--max-children=2", "src.a.*")]
    assert all(c.args.filter == () for c in changed_checks), "MUTATION rows never receive the MTP filter splice"

    uv_checks = _checks(py_routed, TestParams(filter="test_something"), settings, Mode.RUN)
    assert [c.tool.name for c in uv_checks] == ["pytest"]
    assert uv_checks[0].args.filter == (), "UV rows carry no MTP filter splice"


def test_checks_pytest_scope_pin(assay_root: AssayHarness) -> None:
    """Explicit [paths...] leave the pytest family unpinned for file tails; the changed default and --all pin an empty tail."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=("tests/t_a.py",))
    (suite_wide,) = _checks(routed, TestParams(), assay_root.settings, Mode.RUN)
    assert suite_wide.tail == ()
    (all_wide,) = _checks(routed, TestParams(paths=("tests",), all=True), assay_root.settings, Mode.RUN)
    assert all_wide.tail == ()
    (scoped,) = _checks(routed, TestParams(paths=("tests",)), assay_root.settings, Mode.RUN)
    assert scoped.tail is None


def test_checks_trx_splice_composes_per_project(assay_root: AssayHarness) -> None:
    """--trx splices the TRX evidence tail per project under .artifacts/csharp/trx/<project>; default off leaves the hole empty."""
    trx_root = Path(str(assay_root.settings.root)).resolve() / ".artifacts/csharp/trx"
    projects = ("tests/csharp/libs/A/A.Tests.csproj", "tests/csharp/libs/B/B.Tests.csproj")
    multi = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=projects)

    fanned = _checks(multi, TestParams(trx=True), assay_root.settings, Mode.RUN)
    assert [c.args.flags for c in fanned] == [
        ("--report-trx", "--results-directory", str(trx_root / "A.Tests")),
        ("--report-trx", "--results-directory", str(trx_root / "B.Tests")),
    ]
    assert all(c.args.fill(c.tool.command)[-3:-1] == ("--report-trx", "--results-directory") for c in fanned)

    single = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=projects[:1])
    (pinned,) = _checks(single, TestParams(trx=True), assay_root.settings, Mode.RUN)
    assert pinned.args.flags == ("--report-trx", "--results-directory", str(trx_root / "A.Tests"))

    (off,) = _checks(single, TestParams(), assay_root.settings, Mode.RUN)
    assert off.args.flags == ()
    assert "--report-trx" not in off.args.fill(off.tool.command), "TRX evidence is opt-in; the default argv carries no report flags"

    (py_row,) = _checks(Routed(language=_PY, scope=Scope.CHANGED, files=()), TestParams(trx=True), assay_root.settings, Mode.RUN)
    assert py_row.args.flags == (), "TRX is a dotnet RUN concern; UV rows never receive the splice"


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
    ids=["pytest_exit5_empty", "vitest_marker_stderr", "vitest_marker_stdout", "marker_absent_failed", "rc_mismatch_failed", "no_signature_asis"],
)
def test_apply_row_status_empty_signature(
    signature: tuple[int, bytes] | None, rc: int, stdout: bytes, stderr: bytes, expected_status: RailStatus
) -> None:
    """A row's (returncode, marker) empty signature maps a nothing-to-do receipt to EMPTY; any mismatch keeps the tool verdict."""
    tool = msgspec.structs.replace(_tool("vitest", Mode.RUN, Runner.PNPM, Language.TYPESCRIPT), empty_signature=signature)
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


# --- [LAWS_UNSUPPORTED_SCOPE]


def test_unsupported_scope_arms(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """_unsupported_scope emits CHANGED-unscoped mutation, host-bound, and shell-target receipts — and nothing else."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/foo.py",))
    # Counterfactual catalog: every real mutation row is scoped, so the degradation arm needs the synthetic row.
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (_unscoped_mutation(),))
    (unscoped_row,) = _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), assay_root.settings, Mode.MUTATION)
    completed = assert_ok(unscoped_row)
    assert (completed.status, completed.returncode) == (RailStatus.UNSUPPORTED, RailStatus.UNSUPPORTED.exit_code)
    assert any("vitest-mut" in n for n in completed.notes)
    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.FULL), assay_root.settings, Mode.MUTATION) == ()
    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), assay_root.settings, Mode.RUN) == ()
    monkeypatch.undo()

    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), assay_root.settings, Mode.MUTATION) == (), (
        "every real catalog mutation row projects a CHANGED scope"
    )

    project = "tests/csharp/libs/Rasm/Rasm.Tests.csproj"
    host = Routed(Language.CSHARP, Scope.CHANGED, projects=(project,), host_bound=(project,))
    host_receipt = assert_ok(_unsupported_scope(host, TestParams(), assay_root.settings, Mode.RUN)[0])
    assert host_receipt.status is RailStatus.UNSUPPORTED

    shell = "tests/csharp/scenarios/Rasm.Scenarios.csproj"
    assay_root.write(shell, _SHELL_CSPROJ)
    (row,) = _unsupported_scope(Routed(language=Language.CSHARP, scope=Scope.CHANGED), TestParams(target=Path(shell)), assay_root.settings, Mode.RUN)
    target_receipt = assert_ok(row)
    assert (target_receipt.status, target_receipt.returncode) == (RailStatus.UNSUPPORTED, RailStatus.UNSUPPORTED.exit_code)
    assert any("test-target[shell]" in n for n in target_receipt.notes)


# --- [LAWS_SELECT]


def test_select_solution_admission_arms(assay_root: AssayHarness) -> None:
    """_select handles glob passthrough, target narrowing, and solution-backed all-selection with full lane classification."""
    settings = assay_root.settings
    _seed_solution(assay_root)
    host = "tests/csharp/libs/Rasm.Host/Rasm.Host.Tests.csproj"
    py_routed = Routed(language=_PY, scope=Scope.CHANGED, projects=(host,))
    cs_routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=(host,))
    target = Path("tests/csharp/libs/Other/Other.Tests.csproj")
    assay_root.write(target, "<Project />")

    assert assert_ok(_select(py_routed, TestParams(), settings)).routed is py_routed
    assert assert_ok(_select(cs_routed, TestParams(target=target), settings)).routed.projects == (str(target),)
    selected = assert_ok(_select(cs_routed, TestParams(all=True), settings))
    assert selected.routed.scope is Scope.FULL
    assert selected.routed.host_bound == (host,)
    assert set(selected.routed.projects) == {
        "tests/csharp/_architecture/Rasm.Architecture.Tests.csproj",
        "tests/csharp/tools/cs-analyzer/Csp.Analyzer.Tests.csproj",
        "tests/csharp/tools/rhino-bridge/Contract/Contract.csproj",
        "tests/csharp/tools/rhino-bridge/Supervisor/Supervisor.csproj",
        host,
    }
    # Shell projects never reach dispatch even when they carry real content — the marker wins; the unreadable
    # Ghost roster entry is fault-shaped NON_TEST, never silently MANAGED.
    assert not {"tests/csharp/libs/Rasm/Rasm.Tests.csproj", "tests/csharp/libs/Rasm.Ghost/Rasm.Ghost.Tests.csproj"} & set(selected.routed.projects)
    counts = {lane.value: total for lane in _TestProjectLane if (total := sum(1 for _, actual in selected.lanes if actual is lane))}
    assert counts == {"managed": 4, "host_bound": 1, "shell": 2, "support": 1, "benchmark": 1, "non_test": 4}


def test_select_changed_arm_classifies_marker_lanes(assay_root: AssayHarness) -> None:
    """The changed-scope arm classifies the routed closure itself; a stale routed.host_bound never survives."""
    _seed_solution(assay_root)
    shell = "tests/csharp/libs/Rasm/Rasm.Tests.csproj"
    host = "tests/csharp/libs/Rasm.Host/Rasm.Host.Tests.csproj"
    managed = "tests/csharp/_architecture/Rasm.Architecture.Tests.csproj"
    lib = "libs/csharp/Rasm/Rasm.csproj"
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=(lib, shell, host, managed), host_bound=(managed,))
    selected = assert_ok(_select(routed, TestParams(), assay_root.settings))
    assert set(selected.routed.projects) == {host, managed}
    assert selected.routed.host_bound == (host,), "host_bound is rebuilt from the marker table, not trusted from routing"
    assert dict(selected.lanes) == {
        lib: _TestProjectLane.NON_TEST,
        shell: _TestProjectLane.SHELL,
        host: _TestProjectLane.HOST_BOUND,
        managed: _TestProjectLane.MANAGED,
    }


@pytest.mark.parametrize("content", [pytest.param("<Solution", id="corrupt-xml"), pytest.param(None, id="missing-file")])
def test_select_all_faults_on_corrupt_or_missing_solution(assay_root: AssayHarness, content: str | None) -> None:
    """--all over a corrupt or missing Workspace.slnx is a loud FAULTED result, never a green zero-check run."""
    match content:
        case None:
            (assay_root.root / "Workspace.slnx").unlink()
        case _:
            assay_root.write("Workspace.slnx", content)
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED)
    fault = assert_error_status(_select(routed, TestParams(all=True), assay_root.settings), RailStatus.FAULTED)
    assert "Workspace.slnx" in " ".join(fault.argv)


# --- [LAWS_PROJECT_LANE]


@pytest.mark.parametrize(
    "rel,content,expected",
    [
        pytest.param("tests/csharp/libs/A/A.Tests.csproj", "<Project />", _TestProjectLane.MANAGED, id="no-marker→managed"),
        pytest.param("tests/csharp/libs/A/A.Tests.csproj", _SHELL_WITH_CONTENT_CSPROJ, _TestProjectLane.SHELL, id="shell-marker-wins-over-content"),
        pytest.param(
            "tests/csharp/libs/A/A.Tests.csproj",
            "<Project><PropertyGroup><AssayTestShell>true</AssayTestShell><AssayHostBound>true</AssayHostBound></PropertyGroup></Project>",
            _TestProjectLane.SHELL,
            id="shell-outranks-host-bound",
        ),
        pytest.param("tests/csharp/libs/A/A.Tests.csproj", _HOST_CSPROJ, _TestProjectLane.HOST_BOUND, id="host-bound-marker"),
        pytest.param("tests/csharp/libs/A/A.Tests.csproj", _NON_TEST_CSPROJ, _TestProjectLane.NON_TEST, id="istestproject-false"),
        pytest.param("tests/csharp/libs/A/A.Tests.csproj", None, _TestProjectLane.NON_TEST, id="unreadable→fault-shaped-non-test"),
        pytest.param("tests/csharp/_testkit/Kit.csproj", _SHELL_CSPROJ, _TestProjectLane.SUPPORT, id="testkit-path-outranks-markers"),
        pytest.param("tests/csharp/_benchmarks/Bench.csproj", "<Project />", _TestProjectLane.BENCHMARK, id="benchmarks-path"),
        pytest.param("libs/csharp/Rasm/Rasm.csproj", "<Project />", _TestProjectLane.NON_TEST, id="outside-tests-csharp"),
    ],
)
def test_project_lane_marker_matrix(assay_root: AssayHarness, rel: str, content: str | None, expected: _TestProjectLane) -> None:
    """_project_lane routes path arms then the one-read marker table; an unreadable csproj is never silently MANAGED."""
    if content is not None:
        assay_root.write(rel, content)
    assert _project_lane(rel, assay_root.settings) is expected


# --- [LAWS_DISPATCH]


def test_dispatch_routes_checks_to_fan_and_appends_unsupported(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_dispatch with no checks returns only unsupported receipts; with checks it fans and concatenates the tail."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/foo.py",))
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    monkeypatch.setattr(test_rail, "_checks", lambda *_a, **_k: ())
    monkeypatch.setattr(test_rail, "_unsupported_scope", lambda *_a, **_k: (Ok(object()),))
    assert len(_dispatch(routed, TestParams(), settings=assay_root.settings, scope=scope, mode=Mode.MUTATION, executor=SeamExecutor())) == 1

    ok = _ok(("pytest",))
    monkeypatch.setattr(test_rail, "_checks", lambda *_a, **_k: (Check(tool=_tool("pytest", Mode.RUN), paths=()),))
    monkeypatch.setattr(test_rail, "_unsupported_scope", lambda *_a, **_k: ())
    executor = SeamExecutor(fan_fn=lambda *_a, **_k: (Ok(ok),))
    assert _dispatch(routed, TestParams(), settings=assay_root.settings, scope=scope, mode=Mode.RUN, executor=executor) == (Ok(ok),)


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


# --- [LAWS_VERBS]


def test_verbs_route_claim_verb_and_forced_params(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run and coverage pass Claim.TEST with their verb; coverage forces coverage=True and benchmark=False."""
    seen = _capture_rail(monkeypatch)
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    test_rail.run(assay_root.settings, scope, TestParams(), SeamExecutor())
    test_rail.coverage(assay_root.settings, scope, TestParams(coverage=False, benchmark=True), SeamExecutor())
    assert [(claim, verb) for _, claim, verb in seen] == [(Claim.TEST, "run"), (Claim.TEST, "coverage")]
    forced, _, _ = seen[1]
    assert (forced.coverage, forced.benchmark) == (True, False)


# --- [LAWS_LIST_VERB]


@pytest.mark.parametrize(
    "params,outcome,assertion",
    [
        pytest.param(
            TestParams(),
            _ok(("pytest", "--collect-only"), b"tests/a.py::test_one\ntests/a.py::test_two\n"),
            lambda r: r.counts.total == 2 and "test-roster" in {a.id for a in r.artifacts},
            id="ok-report+roster-artifact",
        ),
        pytest.param(
            TestParams(grep="ALPHA"),
            _ok(("pytest", "--collect-only"), b"tests/a.py::test_alpha\ntests/a.py::test_beta\n"),
            lambda r: [row.id for row in r.results] == ["tests/a.py::test_alpha"],
            id="grep-filters-roster-case-insensitive",
        ),
        pytest.param(
            TestParams(limit=1),
            _ok(("pytest", "--collect-only"), b"tests/a.py::test_one\ntests/a.py::test_two\ntests/a.py::test_three\n"),
            lambda r: r.counts.total == 1 and any("total=3" in n and "returned=1" in n for n in r.notes) and r.detail.selected == 3,
            id="limit-trims-roster+detail-keeps-pre-limit-total",
        ),
        pytest.param(
            TestParams(),
            receipt(("dotnet", "test", "--list-tests"), 1, stderr=b"no project"),
            lambda r: (
                r.status is RailStatus.FAILED
                and any(row.severity == "failed" for row in r.results)
                and any("discovery" in n and "dotnet test" in n for n in r.notes)
            ),
            id="discovery-failure-changes-status",
        ),
        pytest.param(
            TestParams(),
            _ok(("pytest", "--collect-only")),
            lambda r: ("empty_or_failed_discovery", 1) in r.detail.discovery_counts,
            id="empty-discovery-counted-as-unresolved",
        ),
    ],
)
def test_list_report_projection_arms(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, params: TestParams, outcome: Completed, assertion: Callable[..., bool]
) -> None:
    """List projects roster artifacts, grep filtering, limit notes, discovery failures, and empty discovery."""
    _wire(monkeypatch, outcome)
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), params, SeamExecutor()))
    assert assertion(report)


# --- [LAWS_RUN_VERB]


def test_run_envelope_names_host_routed(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Run notes name marker-classified host projects; routed.host_bound is rebuilt, never trusted."""
    managed = "tests/csharp/libs/A/A.Tests.csproj"
    host = "tests/csharp/libs/B/B.Tests.csproj"
    assay_root.write(managed, "<Project />")
    assay_root.write(host, _HOST_CSPROJ)
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=(managed, host))
    _wire(monkeypatch, _ok(("dotnet", "test")), routed=routed, seam="_dispatch_all")
    report = assert_ok(test_rail.run(assay_root.settings, assay_root.scope(Claim.TEST), TestParams(language=Language.CSHARP), SeamExecutor()))
    assert "closure[csharp]: included=1 excluded=0 cached=0 host-routed=1" in report.notes
    assert f"host-routed[csharp]: {host}" in report.notes
    assert isinstance(report.detail, TestRun)
    assert {("managed", 1), ("host_bound", 1)} <= set(report.detail.project_counts)


def test_run_mutation_gap_note_emitted(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run emits a mutation gap note when the language has no eligible runner (typescript has no mutation row by design)."""
    routed = Routed(language=Language.TYPESCRIPT, scope=Scope.CHANGED)
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")
    params = TestParams(mutation=MutationLane.FULL, language=Language.TYPESCRIPT)
    report = assert_ok(test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), params, SeamExecutor()))
    assert any("mutation" in n for n in report.notes)


def test_run_coverage_percent_populates_detail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run with coverage=True decodes coverage.json under settings.root into the TestRun detail."""
    _seed_coverage_json(assay_root.root, b'{"totals": {"percent_covered": 82.5}}')
    _wire(monkeypatch, _ok(("uv", "run", "pytest")), seam="_dispatch_all")
    report = assert_ok(
        test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams(coverage=True), SeamExecutor())
    )
    assert isinstance(report.detail, TestRun)
    assert report.detail.coverage == pytest.approx(82.5)


# --- [LAWS_DISPATCH_ALL]


@pytest.mark.parametrize(
    "params,expected_modes",
    [
        pytest.param(TestParams(mutation=MutationLane.FULL), [Mode.RUN, Mode.MUTATION], id="mutation→adds-MUTATION"),
        pytest.param(TestParams(coverage=True), [Mode.RUN, Mode.STAGE, Mode.CLIENT], id="coverage→adds-STAGE-then-CLIENT"),
        pytest.param(TestParams(), [Mode.RUN], id="plain→RUN-only"),
    ],
)
def test_dispatch_all_mode_arms(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, params: TestParams, expected_modes: list[Mode]) -> None:
    """_dispatch_all adds mutation, coverage-combine, and coverage-report sub-dispatches in order; combine precedes every report row."""
    call_modes: list[Mode] = []

    def _record(*_a: object, mode: Mode, **_k: object) -> tuple[Result[Completed, object], ...]:
        call_modes.append(mode)
        return (Ok(_ok(("pytest",))),)

    monkeypatch.setattr(test_rail, "_dispatch", _record)
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _dispatch_all(routed, params, settings=assay_root.settings, scope=scope, mode=Mode.RUN, executor=SeamExecutor())
    assert call_modes == expected_modes


# --- [LAWS_ROUTED]


def test_routed_sequences_one_routed_per_language(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_routed sequences routing results into one Routed per language, single or combined."""
    monkeypatch.setattr(test_rail, "route", lambda lang, *_a, **_k: Ok(Routed(language=lang, scope=Scope.CHANGED, files=())))
    assert [r.language for r in assert_ok(_routed((_PY,), (), assay_root.settings))] == [_PY]
    assert {r.language for r in assert_ok(_routed((_PY, Language.TYPESCRIPT), (), assay_root.settings))} == {_PY, Language.TYPESCRIPT}


# --- [LAWS_THIN_RAIL]


def test_thin_rail_mutation_nests_sorted_language_leases(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail acquires one mutation lease per eligible language, nested in sorted order."""
    leased_calls: list[str] = []
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _wire(monkeypatch, _ok(("mutmut", "run")), routed=routed, seam="_dispatch_all")
    monkeypatch.setattr(test_rail, "resolve_languages", lambda *_a, **_k: Ok((Language.CSHARP, _PY)))

    def _descend(resource: str, action: Callable[[object], Result[object, object]], **_k: object) -> Result[object, object]:
        leased_calls.append(resource)
        return action(object())

    monkeypatch.setattr(test_rail, "leased", _descend)
    params = TestParams(mutation=MutationLane.FULL)
    assert_ok(_thin_rail(assay_root.settings, scope, params, SeamExecutor(), claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert leased_calls == ["mutation-csharp", "mutation-python"]


def test_thin_rail_per_language_mutation_leases_do_not_contend(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Per-language mutation leases isolate csharp and python contention."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _wire(monkeypatch, _ok(("mutmut", "run")), routed=routed, seam="_dispatch_all")
    params = TestParams(mutation=MutationLane.FULL, language=Language.PYTHON)
    with exclusive_lease("mutation-csharp", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_ok(_thin_rail(assay_root.settings, scope, params, SeamExecutor(), claim=Claim.TEST, verb="run", mode=Mode.RUN))
    with exclusive_lease("mutation-python", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_error_status(
            _thin_rail(assay_root.settings, scope, params, SeamExecutor(), claim=Claim.TEST, verb="run", mode=Mode.RUN), RailStatus.BUSY
        )


def test_thin_rail_no_mutation_skips_leasing(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail with mutation=OFF calls the work directly: no lease is ever acquired."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")

    def _forbidden(*_a: object, **_k: object) -> Result[object, object]:
        raise AssertionError("mutation=OFF must never lease")

    monkeypatch.setattr(test_rail, "leased", _forbidden)
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    params = TestParams(mutation=MutationLane.OFF, language=Language.PYTHON)
    report = assert_ok(_thin_rail(assay_root.settings, scope, params, SeamExecutor(), claim=Claim.TEST, verb="run", mode=Mode.RUN))
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


def _gate_row() -> Tool:
    """Select the one catalog gate row: TEST claim, VERIFY mode, MUTATION-tagged.

    Returns:
        The mutmut-gate catalog row.
    """
    (row,) = tuple(t for t in TOOLS if t.claim is Claim.TEST and t.mode is Mode.VERIFY and ToolGroup.MUTATION in t.groups)
    return row


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


def test_gate_tool_is_catalog_row_off_dispatch_fans() -> None:
    """The kill-rate gate is one catalog row whose VERIFY mode keeps it off every RUN/MUTATION dispatch fan."""
    row = _gate_row()
    assert (row.name, row.runner, row.groups, row.mode) == ("mutmut-gate", Runner.UV, (ToolGroup.MUTATION,), Mode.VERIFY)
    assert (row.stage.root, row.stage.project) == ("", True)
    assert row.input is Input.OWNED
    assert row.command == ("python", "-m", "tools.assay.rails.mutation_gate")
    for mode in (Mode.RUN, Mode.MUTATION, Mode.LIST):
        assert row not in _rows(_PY, TestParams(mutation=MutationLane.FULL), mode), f"gate row leaked into the {mode.value} fan"


def test_gate_check_rides_staged_mutmut_success(assay_root: AssayHarness) -> None:
    """_gate fans one staged-worktree catalog gate-row check after mutmut success — real rows end to end."""
    (staged,) = (t for t in _rows(_PY, TestParams(mutation=MutationLane.FULL), Mode.MUTATION) if t.stage.root)
    seen: list[Check] = []

    def _fake_fan(checks: tuple[Check, ...], **_k: object) -> tuple[Result[Completed, object], ...]:
        seen.extend(checks)
        return (Ok(_ok(("uv", "run", "python", "-m", "tools.assay.rails.mutation_gate"))),)

    done = (Ok(_ok(("uv", "run", "--group", "mutation", "mutmut", "run"))),)
    out = _gate(
        done,
        _PY_ROUTED,
        TestParams(mutation=MutationLane.FULL),
        settings=assay_root.settings,
        scope=assay_root.scope(Claim.TEST),
        executor=SeamExecutor(fan_fn=_fake_fan),
    )
    assert len(out) == 1
    assert [c.tool for c in seen] == [_gate_row()]
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
        raise AssertionError("executor.fan must not run")

    if not staged:
        # Counterfactual catalog: the real mutmut row is staged, so the stage-less arm needs the synthetic row.
        monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (_tool("mutmut", Mode.MUTATION),))
    executor = SeamExecutor(fan_fn=_forbidden)
    done = (Ok(receipt(argv, code)),)
    gated = _gate(
        done, _PY_ROUTED, TestParams(mutation=MutationLane.FULL), settings=assay_root.settings, scope=assay_root.scope(Claim.TEST), executor=executor
    )
    assert gated == ()


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
    out = _dispatch_all(
        _PY_ROUTED, TestParams(mutation=MutationLane.FULL), settings=assay_root.settings, scope=scope, mode=Mode.RUN, executor=SeamExecutor()
    )
    assert handed == [(mut_ok,)]
    assert out == (mut_ok, gate_ok)
    assert _dispatch_all(_PY_ROUTED, TestParams(), settings=assay_root.settings, scope=scope, mode=Mode.RUN, executor=SeamExecutor()) == ()
    assert handed == [(mut_ok,)]
