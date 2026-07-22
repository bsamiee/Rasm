"""Law matrix for test params, coverage, roster, dispatch, TRX evidence, and mutation gate."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path
from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Ok
from expression.collections import block
import msgspec
from mutmut import configuration as mutmut_configuration
from mutmut.mutation.data import SourceFileMutationData
import pytest

from tests.python._testkit.spec import assert_error_status, assert_ok, refutes, validity_matrix
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.composition.catalog import TOOLS
from tools.assay.composition.store import ArtifactScope, CS_ARTIFACT_ROOTS
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
from tools.assay.rails.mutation_gate import _tally, gate, schema_report
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
    from tools.assay.core.model import Completed, ToolArgs


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (TestParams, coverage_percent, test_rail.coverage, test_rail.list, test_rail.run, gate, schema_report, apply_row_status)

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
_STRYKER_POLICY = ("--test-runner", "mtp", "--mutation-level", "Standard")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok(argv: tuple[str, ...], stdout: bytes = b"", *, status: RailStatus = RailStatus.OK) -> Completed:
    return receipt(argv, 0, status=status, stdout=stdout)


# Laws compose argv from the real catalog rows, never synthetic mirrors.
def _row(name: str, mode: Mode) -> Tool:
    (row,) = tuple(t for t in TOOLS if t.name == name and t.mode is mode and t.claim is Claim.TEST)
    return row


# Minimal TEST row for pure-policy laws, carrying the catalog's policy groups for the name.
def _tool(name: str = "pytest", mode: Mode = Mode.RUN, runner: Runner = Runner.UV, language: Language = _PY) -> Tool:
    groups = next((t.groups for t in TOOLS if t.name == name and t.claim is Claim.TEST), ())
    return Tool(name=name, runner=runner, command=(name,), input=Input.NONE, language=language, claim=Claim.TEST, mode=mode, groups=groups)


# The one justified synthetic row: every real mutation row projects a CHANGED scope, but the
# degradation law (unscoped -> UNSUPPORTED, never a full-tree run) must hold for future runners.
def _unscoped_mutation() -> Tool:
    return _tool("vitest-mut", Mode.MUTATION)


def _seed_coverage_json(root: Path, payload: bytes) -> Path:
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


# Seed Workspace.slnx plus the marker-bearing csproj roster; the Ghost project stays unwritten on purpose.
def _seed_solution(assay_root: AssayHarness) -> tuple[str, ...]:
    markers = {
        "libs/csharp/Rasm/Rasm.csproj": "<Project />",
        "tests/csharp/_architecture/Rasm.Architecture.Tests.csproj": "<Project />",
        "tests/csharp/_benchmarks/Rasm.Benchmarks.csproj": "<Project />",
        "tests/csharp/_scenariokit/Rasm.ScenarioKit.csproj": _NON_TEST_CSPROJ,
        "tests/csharp/_testkit/Rasm.TestKit.csproj": "<Project />",
        "tests/csharp/libs/Rasm/Rasm.Tests.csproj": _SHELL_WITH_CONTENT_CSPROJ,
        "tests/csharp/libs/Rasm.Empty/Rasm.Empty.Tests.csproj": _NON_TEST_CSPROJ,
        "tests/csharp/libs/Rasm.Ghost/Rasm.Ghost.Tests.csproj": None,
        "tests/csharp/libs/Rasm.Host/Rasm.Host.Tests.csproj": _HOST_CSPROJ,
        "tests/csharp/scenarios/Rasm.Scenarios.csproj": _SHELL_CSPROJ,
        "tests/csharp/tools/cs-analyzer/Csp.Analyzer.Tests.csproj": "<Project />",
        "tests/csharp/tools/rhino-bridge/Contract/Rasm.Bridge.Contract.Tests.csproj": "<Project />",
        "tests/csharp/tools/rhino-bridge/Supervisor/Rasm.Bridge.Supervisor.Tests.csproj": "<Project />",
    }
    folders = "".join(f'<Folder Name="/{Path(p).parent.as_posix()}/"><Project Path="{p}" /></Folder>' for p in markers)
    assay_root.write("Workspace.slnx", f"<Solution>{folders}</Solution>")
    [assay_root.write(project, content) for project, content in markers.items() if content is not None]
    return tuple(markers)


# --- [LAWS_COVERAGE_ARTIFACTS] ----------------------------------------------------------


def test_coverage_percent_decodes_or_degrades(tmp_path: Path) -> None:
    """coverage_percent decodes totals and degrades missing or malformed files to None."""
    rows: tuple[tuple[str, bytes | None, float | None], ...] = (
        ("float-percent", b'{"totals": {"percent_covered": 87.5}}', 87.5),
        ("int-coerced", b'{"totals": {"percent_covered": 100}}', 100.0),
        ("zero-percent", b'{"totals": {"percent_covered": 0.0}}', 0.0),
        ("extra-keys-ignored", b'{"totals": {"percent_covered": 72.345}, "files": {}}', 72.345),
        ("missing-totals", b'{"meta": {}}', None),
        ("missing-percent", b'{"totals": {}}', None),
        ("malformed-json", b"{bad json", None),
        ("absent-file", None, None),
    )
    for index, (label, payload, expected) in enumerate(rows):
        root = tmp_path / str(index)
        _seed_coverage_json(root, payload) if payload is not None else None
        got = coverage_percent(root)
        assert got is None if expected is None else got == pytest.approx(expected), label


def test_adopt_coverage_artifact_fields(assay_root: AssayHarness) -> None:
    """_adopt_coverage reads the file, stores it, and populates artifact id/kind/bytes/lines."""
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    cov_file = _seed_coverage_json(assay_root.root, b'{"totals": {"percent_covered": 87.5}}\n')
    artifact = _adopt_coverage(scope.store, assay_root.settings.run_id, cov_file)
    assert (artifact.id, artifact.kind, artifact.bytes, artifact.lines) == ("coverage.json", ArtifactKind.TEST, 38, 1)


# --- [LAWS_PARAMS_AND_POLICY] -----------------------------------------------------------


def test_testparams_language_flags_and_help(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Ambiguous language selectors fault; help exposes the boolean selectors and --trx, never a --language value flag."""
    fault = TestParams(csharp=True, typescript=True).bound("run")
    assert isinstance(fault, Fault)
    assert "--csharp" in fault.message
    assert "--typescript" in fault.message

    from tools.assay import __main__ as main_mod  # noqa: PLC0415

    monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None))
    assert main_mod.main(["test", "run", "--help"]) == 0
    out = capsysbinary.readouterr().out
    assert all(flag in out for flag in (b"--csharp", b"--python", b"--typescript", b"--trx"))
    assert b"--language" not in out


def test_eligible_branch_matrix() -> None:
    """_eligible projects every (mode, params) arm to the correct True/False gate."""
    validity_matrix(
        [
            ("mutation-OFF", (_tool("mutmut", Mode.MUTATION), TestParams()), False),
            ("mutation-CHANGED", (_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.CHANGED)), True),
            ("mutation-FULL", (_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.FULL)), True),
            ("build-default", (_tool("dotnet-build", Mode.BUILD, Runner.DOTNET), TestParams()), True),
            ("pytest-plain", (_tool("pytest"), TestParams()), True),
            ("pytest-under-coverage", (_tool("pytest"), TestParams(coverage=True)), False),
            ("pytest-under-benchmark", (_tool("pytest"), TestParams(benchmark=True)), False),
            ("coverage-no-flag", (_tool("coverage"), TestParams()), False),
            ("coverage-flag", (_tool("coverage"), TestParams(coverage=True)), True),
            ("coverage-json-no-flag", (_tool("coverage-json"), TestParams()), False),
            ("coverage-json-flag", (_tool("coverage-json"), TestParams(coverage=True)), True),
            ("benchmark-no-flag", (_tool("pytest-benchmark"), TestParams()), False),
            ("benchmark-flag", (_tool("pytest-benchmark"), TestParams(benchmark=True)), True),
        ],
        lambda case: _eligible(*case),
    )


def test_filter_discriminant_arms() -> None:
    """_filter maps each filter expression to the correct MTP flag tuple."""
    expected = {
        "": (),
        "   ": (),
        "/SomeTrait": ("--filter-query", "/SomeTrait"),
        "  /SomeTrait  ": ("--filter-query", "/SomeTrait"),
        "Category=unit": ("--filter-trait", "Category=unit"),
        "MyTests": ("--filter-class", "*MyTests*"),
        "MyLaws": ("--filter-class", "*MyLaws*"),
        "MySpec": ("--filter-class", "*MySpec*"),
        "My+Class": ("--filter-class", "*My+Class*"),
        "test_something": ("--filter-method", "*test_something*"),
    }
    assert {expr: _filter(expr) for expr in expected} == expected


def test_apply_row_status_empty_signature() -> None:
    """A row's (returncode, marker) empty signature maps a nothing-to-do receipt to EMPTY; any mismatch keeps the tool verdict."""
    rows: tuple[tuple[str, tuple[int, bytes] | None, int, bytes, bytes, RailStatus], ...] = (
        ("pytest-exit5-empty", (5, b""), 5, b"", b"no tests ran", RailStatus.EMPTY),
        ("vitest-marker-stderr", (1, b"No test files found"), 1, b"", b"No test files found, exiting with code 1", RailStatus.EMPTY),
        ("vitest-marker-stdout", (1, b"No test files found"), 1, b"No test files found\n", b"", RailStatus.EMPTY),
        ("marker-absent-failed", (1, b"No test files found"), 1, b"", b"2 tests failed", RailStatus.FAILED),
        ("rc-mismatch-failed", (5, b""), 1, b"", b"", RailStatus.FAILED),
        ("no-signature-keeps-raw", None, 5, b"", b"", RailStatus.BUSY),
    )
    for label, signature, rc, stdout, stderr, expected in rows:
        tool = msgspec.structs.replace(_tool("vitest", Mode.RUN, Runner.PNPM, Language.TYPESCRIPT), empty_signature=signature)
        assert apply_row_status(tool, receipt(("vitest", "run"), rc, stdout=stdout, stderr=stderr)).status is expected, label


def test_catalog_test_runners_carry_empty_signature() -> None:
    """Every RUN/LIST test runner whose tool signals no-eligible-work declares the signature on its catalog row."""
    expected = {
        ("pytest", Mode.RUN): (5, b""),
        ("pytest", Mode.LIST): (5, b""),
        ("pytest-benchmark", Mode.RUN): (5, b""),
        ("coverage", Mode.RUN): (5, b""),
        ("vitest", Mode.RUN): (1, b"No test files found"),
    }
    assert {(t.name, t.mode): t.empty_signature for t in TOOLS if (t.name, t.mode) in expected} == expected


# --- [LAWS_MUTATION_ARGV] ---------------------------------------------------------------


def _filled_law(args: ToolArgs | None, tool: Tool, expected: tuple[str, ...]) -> None:
    assert args is not None, "mutation args dropped out of the lane unexpectedly"
    assert args.fill(tool.command) == expected, f"filled {args.fill(tool.command)} != {expected}"


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
    root = Path(str(settings.root)).resolve()
    output_dir = root / ".artifacts/csharp/stryker"
    config_file = root / "stryker-config.json"
    assert (scoped.config, scoped.solution, scoped.output) == (str(config_file), str(settings.solution), str(output_dir))
    assert output_dir.is_dir(), "the rail pre-creates the Stryker report --output dir"
    anchors = (
        "--config-file",
        str(config_file),
        "--solution",
        str(settings.solution),
        "--output",
        str(output_dir),
        "--mutate",
        "src/Foo.cs",
        "--mutate",
        "src/Bar.cs",
    )
    _filled_law(scoped, stryker, ("tool", "run", "dotnet-stryker", "--", *_STRYKER_POLICY, *anchors))

    # Wrong filled shapes prove the transform is non-vacuous.
    refutes(changed, _filled_law, mutmut, ("mutmut", "run", "src/a.py", "src/b.py"))  # passthrough: no governor, no module-name glob
    refutes(changed, _filled_law, mutmut, ("mutmut", "run", "--max-children=2", "src/a.py*", "src/b.py*"))  # path-shaped glob: zero mutant NAMES
    refutes(full, _filled_law, mutmut, ("mutmut", "run"))  # governor dropped
    refutes(scoped, _filled_law, stryker, ("tool", "run", "dotnet-stryker", "--", "src/Foo.cs", "src/Bar.cs"))  # --mutate flag dropped


def test_mutation_rows_confine_every_path_to_artifacts() -> None:
    """No mutation row can write to repo root: stage roots, sandbox cwd, and report outputs all live under .artifacts/."""
    mutmut, stryker = _row("mutmut", Mode.MUTATION), _row("dotnet-stryker", Mode.MUTATION)
    assert stryker.stage.root == ".artifacts/csharp/stryker/work", "Stryker cwd (and its .stryker-tmp sandbox) is the staged work root"
    assert mutmut.stage.root == ".artifacts/python/mutmut/work", "mutmut cwd (and its mutants/ cache) is the staged work root"
    assert all(part in stryker.command for part in (*_STRYKER_POLICY, "--config-file")), "policy is pinned; the rail fills {config} absolutely"
    # Every path in both commands is a typed hole the rail fills; the root stryker-config.json additionally bounds bare runs by auto-discovery.
    literal_paths = [t for row in (mutmut, stryker) for t in row.command if "/" in t and "{" not in t]
    assert literal_paths == []


# --- [LAWS_CHECKS] ----------------------------------------------------------------------


def test_checks_splice_and_scope_arms(assay_root: AssayHarness) -> None:
    """_checks routes DOTNET RUN filters into {filter*}, leaves UV/MUTATION rows unfiltered, and pins the pytest suite tail."""
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

    # Explicit [paths...] leave the pytest family unpinned for file tails; the changed default and --all pin an empty tail.
    scoped_routed = Routed(language=_PY, scope=Scope.CHANGED, files=("tests/t_a.py",))
    (suite_wide,) = _checks(scoped_routed, TestParams(), assay_root.settings, Mode.RUN)
    (all_wide,) = _checks(scoped_routed, TestParams(paths=("tests",), all=True), assay_root.settings, Mode.RUN)
    (scoped,) = _checks(scoped_routed, TestParams(paths=("tests",)), assay_root.settings, Mode.RUN)
    assert (suite_wide.tail, all_wide.tail, scoped.tail) == ((), (), None)


def test_checks_trx_splice_composes_per_project(assay_root: AssayHarness) -> None:
    """--trx splices the TRX evidence tail per project under the dedicated CS_ARTIFACT_ROOTS trx key; default off leaves the hole empty."""
    assert CS_ARTIFACT_ROOTS["trx"] == ".artifacts/csharp/trx", "the trx root is its own CS_ARTIFACT_ROOTS row, never derived from a sibling"
    trx_root = Path(str(assay_root.settings.root)).resolve() / CS_ARTIFACT_ROOTS["trx"]
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

    (py_check,) = _checks(Routed(language=_PY, scope=Scope.CHANGED, files=()), TestParams(trx=True), assay_root.settings, Mode.RUN)
    assert py_check.args.flags == (), "TRX is a dotnet RUN concern; UV rows never receive the splice"


# --- [LAWS_SELECT_AND_LANES] ------------------------------------------------------------


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
    assert assert_ok(_unsupported_scope(host, TestParams(), assay_root.settings, Mode.RUN)[0]).status is RailStatus.UNSUPPORTED

    shell = "tests/csharp/scenarios/Rasm.Scenarios.csproj"
    assay_root.write(shell, _SHELL_CSPROJ)
    (row,) = _unsupported_scope(Routed(language=Language.CSHARP, scope=Scope.CHANGED), TestParams(target=Path(shell)), assay_root.settings, Mode.RUN)
    target_receipt = assert_ok(row)
    assert (target_receipt.status, target_receipt.returncode) == (RailStatus.UNSUPPORTED, RailStatus.UNSUPPORTED.exit_code)
    assert any("test-target[shell]" in n for n in target_receipt.notes)


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
        "tests/csharp/tools/rhino-bridge/Contract/Rasm.Bridge.Contract.Tests.csproj",
        "tests/csharp/tools/rhino-bridge/Supervisor/Rasm.Bridge.Supervisor.Tests.csproj",
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
    lanes = (_TestProjectLane.NON_TEST, _TestProjectLane.SHELL, _TestProjectLane.HOST_BOUND, _TestProjectLane.MANAGED)
    assert dict(selected.lanes) == dict(zip((lib, shell, host, managed), lanes, strict=True))


def test_select_all_faults_on_corrupt_or_missing_solution(assay_root: AssayHarness) -> None:
    """--all over a corrupt or missing Workspace.slnx is a loud FAULTED result, never a green zero-check run."""
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED)
    assay_root.write("Workspace.slnx", "<Solution")
    corrupt = assert_error_status(_select(routed, TestParams(all=True), assay_root.settings), RailStatus.FAULTED)
    (assay_root.root / "Workspace.slnx").unlink()
    missing = assert_error_status(_select(routed, TestParams(all=True), assay_root.settings), RailStatus.FAULTED)
    assert all("Workspace.slnx" in " ".join(fault.argv) for fault in (corrupt, missing))


def test_project_lane_marker_matrix(assay_root: AssayHarness) -> None:
    """_project_lane routes path arms then the one-read marker table; an unreadable csproj is never silently MANAGED."""
    dual = "<Project><PropertyGroup><AssayTestShell>true</AssayTestShell><AssayHostBound>true</AssayHostBound></PropertyGroup></Project>"
    lane = _TestProjectLane
    rows: tuple[tuple[str, str, str | None, _TestProjectLane], ...] = (
        ("no-marker-managed", "tests/csharp/libs/A/A.Tests.csproj", "<Project />", lane.MANAGED),
        ("shell-marker-wins-over-content", "tests/csharp/libs/A/A.Tests.csproj", _SHELL_WITH_CONTENT_CSPROJ, lane.SHELL),
        ("shell-outranks-host-bound", "tests/csharp/libs/A/A.Tests.csproj", dual, lane.SHELL),
        ("host-bound-marker", "tests/csharp/libs/A/A.Tests.csproj", _HOST_CSPROJ, lane.HOST_BOUND),
        ("istestproject-false", "tests/csharp/libs/A/A.Tests.csproj", _NON_TEST_CSPROJ, lane.NON_TEST),
        ("unreadable-fault-shaped-non-test", "tests/csharp/libs/Missing/Missing.Tests.csproj", None, lane.NON_TEST),
        ("testkit-path-outranks-markers", "tests/csharp/_testkit/Kit.csproj", _SHELL_CSPROJ, lane.SUPPORT),
        ("benchmarks-path", "tests/csharp/_benchmarks/Bench.csproj", "<Project />", lane.BENCHMARK),
        ("outside-tests-csharp", "libs/csharp/Rasm/Rasm.csproj", "<Project />", lane.NON_TEST),
    )
    for label, rel, content, expected in rows:
        assay_root.write(rel, content) if content is not None else None
        assert _project_lane(rel, assay_root.settings) is expected, label


# --- [LAWS_DISPATCH_AND_ROSTER] ---------------------------------------------------------


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


def test_roster_matches_skip_and_kind_arms() -> None:
    """_roster_matches skips headers/summaries and non-success discovery."""
    mixed = _ok(("dotnet", "test", "--list-tests"), b"The following tests are available:\nMyNs.MyClass.test_one\n3 test(s) found\n")
    matches = _roster_matches((mixed,))
    assert [m.id for m in matches] == ["MyNs.MyClass.test_one"]
    assert all(m.kind is ArtifactKind.PROCESS for m in matches)
    assert _roster_matches((receipt(("dotnet", "test", "--list-tests"), 1, stdout=b"X.test\n"),)) == ()
    assert _roster_matches((_ok(("pytest", "--collect-only")),)) == ()


def test_detail_mutation_and_coverage_arms(tmp_path: Path) -> None:
    """_detail projects mutation and coverage combinations into TestRun or None."""
    rows: tuple[tuple[str, tuple[Completed, ...], TestParams, bytes | None, float | None], ...] = (
        ("off+no-coverage", (), TestParams(), None, None),
        ("full+noise-stdout", (_ok(("mutmut", "run"), b"not-json\n"),), TestParams(mutation=MutationLane.FULL), None, None),
        ("coverage+no-json", (_ok(("uv", "run", "pytest")),), TestParams(coverage=True), None, None),
        ("coverage+json", (_ok(("uv", "run", "pytest")),), TestParams(coverage=True), b'{"totals": {"percent_covered": 91.5}}', 91.5),
    )
    for index, (label, done, params, seed, expected) in enumerate(rows):
        root = tmp_path / str(index)
        _seed_coverage_json(root, seed) if seed is not None else root.mkdir()
        result = _detail(done, params, root)
        match expected:
            case None:
                assert result is None, label
            case _:
                assert isinstance(result, TestRun), label
                assert result.coverage == pytest.approx(expected), label


# --- [LAWS_VERBS] -----------------------------------------------------------------------


def test_verbs_route_claim_verb_and_forced_params(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run and coverage pass Claim.TEST with their verb; coverage forces coverage=True and benchmark=False."""
    seen = _capture_rail(monkeypatch)
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    test_rail.run(assay_root.settings, scope, TestParams(), SeamExecutor())
    test_rail.coverage(assay_root.settings, scope, TestParams(coverage=False, benchmark=True), SeamExecutor())
    assert [(claim, verb) for _, claim, verb in seen] == [(Claim.TEST, "run"), (Claim.TEST, "coverage")]
    forced, _, _ = seen[1]
    assert (forced.coverage, forced.benchmark) == (True, False)


def test_list_report_projection_arms(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List projects roster artifacts, grep filtering, limit notes, discovery failures, and empty discovery."""
    roster = _ok(("pytest", "--collect-only"), b"tests/a.py::test_one\ntests/a.py::test_two\n")
    three = _ok(("pytest", "--collect-only"), b"tests/a.py::test_one\ntests/a.py::test_two\ntests/a.py::test_three\n")
    greppable = _ok(("pytest", "--collect-only"), b"tests/a.py::test_alpha\ntests/a.py::test_beta\n")
    rows: tuple[tuple[str, TestParams, Completed, Callable[..., bool]], ...] = (
        ("ok-report+roster-artifact", TestParams(), roster, lambda r: r.counts.total == 2 and "test-roster" in {a.id for a in r.artifacts}),
        ("grep-case-insensitive", TestParams(grep="ALPHA"), greppable, lambda r: [m.id for m in r.results] == ["tests/a.py::test_alpha"]),
        (
            "limit-trims+detail-keeps-pre-limit-total",
            TestParams(limit=1),
            three,
            lambda r: r.counts.total == 1 and any("total=3" in n and "returned=1" in n for n in r.notes) and r.detail.selected == 3,
        ),
        (
            "discovery-failure-changes-status",
            TestParams(),
            receipt(("dotnet", "test", "--list-tests"), 1, stderr=b"no project"),
            lambda r: (
                r.status is RailStatus.FAILED
                and any(m.severity == "failed" for m in r.results)
                and any("discovery" in n and "dotnet test" in n for n in r.notes)
            ),
        ),
        (
            "empty-discovery-counted-unresolved",
            TestParams(),
            _ok(("pytest", "--collect-only")),
            lambda r: ("empty_or_failed_discovery", 1) in r.detail.discovery_counts,
        ),
    )
    for label, params, outcome, law in rows:
        _wire(monkeypatch, outcome)
        report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), params, SeamExecutor()))
        assert law(report), label


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


def test_run_gap_note_and_coverage_detail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run notes the TS mutation gap (no TS mutation row by design) and decodes coverage.json into the TestRun detail."""
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _wire(monkeypatch, _ok(("pytest",)), routed=Routed(language=Language.TYPESCRIPT, scope=Scope.CHANGED), seam="_dispatch_all")
    params = TestParams(mutation=MutationLane.FULL, language=Language.TYPESCRIPT)
    gap_report = assert_ok(test_rail.run(assay_root.settings, scope, params, SeamExecutor()))
    assert any("mutation" in n for n in gap_report.notes)

    _seed_coverage_json(assay_root.root, b'{"totals": {"percent_covered": 82.5}}')
    _wire(monkeypatch, _ok(("uv", "run", "pytest")), seam="_dispatch_all")
    report = assert_ok(test_rail.run(assay_root.settings, scope, TestParams(coverage=True), SeamExecutor()))
    assert isinstance(report.detail, TestRun)
    assert report.detail.coverage == pytest.approx(82.5)


# --- [LAWS_THIN_RAIL] -------------------------------------------------------------------


def test_dispatch_all_mode_arms(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_dispatch_all adds mutation, coverage-combine, and coverage-report sub-dispatches in order; combine precedes every report row."""
    call_modes: list[Mode] = []

    def _record(*_a: object, mode: Mode, **_k: object) -> tuple[Result[Completed, object], ...]:
        call_modes.append(mode)
        return (Ok(_ok(("pytest",))),)

    monkeypatch.setattr(test_rail, "_dispatch", _record)
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    for params, expected in (
        (TestParams(mutation=MutationLane.FULL), [Mode.RUN, Mode.MUTATION]),
        (TestParams(coverage=True), [Mode.RUN, Mode.STAGE, Mode.CLIENT]),
        (TestParams(), [Mode.RUN]),
    ):
        call_modes.clear()
        _dispatch_all(routed, params, settings=assay_root.settings, scope=scope, mode=Mode.RUN, executor=SeamExecutor())
        assert call_modes == expected


def test_routed_sequences_one_routed_per_language(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_routed sequences routing results into one Routed per language, single or combined."""
    monkeypatch.setattr(test_rail, "route", lambda lang, *_a, **_k: Ok(Routed(language=lang, scope=Scope.CHANGED, files=())))
    assert [r.language for r in assert_ok(_routed((_PY,), (), assay_root.settings))] == [_PY]
    assert {r.language for r in assert_ok(_routed((_PY, Language.TYPESCRIPT), (), assay_root.settings))} == {_PY, Language.TYPESCRIPT}


def test_thin_rail_mutation_nests_sorted_language_leases(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail acquires one mutation lease per eligible language, nested in sorted order."""
    leased_calls: list[str] = []
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _wire(monkeypatch, _ok(("mutmut", "run")), routed=Routed(language=_PY, scope=Scope.CHANGED, files=()), seam="_dispatch_all")
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
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _wire(monkeypatch, _ok(("mutmut", "run")), routed=Routed(language=_PY, scope=Scope.CHANGED, files=()), seam="_dispatch_all")
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
    _wire(monkeypatch, _ok(("pytest",)), routed=Routed(language=_PY, scope=Scope.CHANGED, files=()), seam="_dispatch_all")

    def _forbidden(*_a: object, **_k: object) -> Result[object, object]:
        raise AssertionError("mutation=OFF must never lease")

    monkeypatch.setattr(test_rail, "leased", _forbidden)
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    params = TestParams(mutation=MutationLane.OFF, language=Language.PYTHON)
    report = assert_ok(_thin_rail(assay_root.settings, scope, params, SeamExecutor(), claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert report.status in {RailStatus.OK, RailStatus.EMPTY}


# --- [LAWS_MUTATION_GATE] ---------------------------------------------------------------


# Synthesize mutmut's persisted result surface: cwd pyproject + source tree + mutants/ meta cache.
def _seed_cache(root: Path, monkeypatch: pytest.MonkeyPatch, exit_codes: dict[str, int | None]) -> None:
    (root / "src").mkdir(parents=True)
    (root / "src" / "mod.py").write_text("X = 1\n")
    (root / "pyproject.toml").write_text('[tool.mutmut]\nsource_paths = ["src"]\n')
    (root / "mutants" / "src").mkdir(parents=True)
    meta = {"exit_code_by_key": exit_codes, "durations_by_key": {}, "estimated_durations_by_key": {}}
    (root / "mutants" / "src" / "mod.py.meta").write_bytes(msgspec.json.encode(meta))
    monkeypatch.chdir(root)
    monkeypatch.setattr(mutmut_configuration, "_config", None)


def _gate_row() -> Tool:
    (row,) = tuple(t for t in TOOLS if t.claim is Claim.TEST and t.mode is Mode.VERIFY and ToolGroup.MUTATION in t.groups)
    return row


def test_gate_seeded_cache_matrix(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]) -> None:
    """Mutation gate keeps tally, JSON line, verdict stream, and exit code aligned."""
    rows: tuple[tuple[str, dict[str, int | None], int, int, int, int], ...] = (
        ("floor-edge-0.800-pass", {"a": 1, "b": 1, "c": 1, "d": 1, "e": 0}, 0, 4, 1, 5),
        ("score-0.5-fail", {"a": 1, "b": 0}, 1, 1, 1, 2),
        ("unscored-excluded-from-denominator", {"a": 1, "b": 33, "c": 36, "d": None}, 0, 1, 0, 4),
        ("empty-cache-fail", {}, 1, 0, 0, 0),
    )
    for index, (label, codes, rc, killed, survived, selected) in enumerate(rows):
        _seed_cache(tmp_path / str(index), monkeypatch, codes)
        tally = _tally()
        assert (tally["killed"], tally["survived"], tally.total()) == (killed, survived, selected), label
        code = gate()
        out, err = capsys.readouterr()
        lines = out.splitlines()
        assert (code, len(lines)) == (rc, 1), label
        assert err.startswith("[PASS]" if rc == 0 else "[FAIL]"), label
        done = receipt(("uv", "run", "python", "-m", "tools.assay.rails.mutation_gate"), code, stdout=lines[0].encode())
        expected = TestRun(mutation=MutationLane.FULL, killed=killed, survived=survived, selected=selected)
        assert _detail((done,), TestParams(mutation=MutationLane.FULL), tmp_path / str(index)) == expected, label


def test_schema_report_projects_mutmut_results(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]) -> None:
    """schema_report maps mutmut meta to schema mutants — id, mutatorName, status, function-span location; gate() lands the artifact."""
    (tmp_path / "src").mkdir()
    source = "def alpha() -> int:\n    return 1\n\n\nclass Owner:\n    def beta(self) -> int:\n        return 2\n"
    (tmp_path / "src" / "mod.py").write_text(source)
    (tmp_path / "pyproject.toml").write_text('[tool.mutmut]\nsource_paths = ["src"]\n')
    (tmp_path / "mutants" / "src").mkdir(parents=True)
    codes: dict[str, int | None] = {
        "src.mod.x_alpha__mutmut_1": 1,
        "src.mod.x_alpha__mutmut_2": 0,
        "src.mod.x_alpha__mutmut_3": None,
        "src.mod.xǁOwnerǁx_beta__mutmut_1": 5,
    }
    meta = {"exit_code_by_key": codes, "durations_by_key": {}, "estimated_durations_by_key": {}}
    (tmp_path / "mutants" / "src" / "mod.py.meta").write_bytes(msgspec.json.encode(meta))
    monkeypatch.chdir(tmp_path)
    monkeypatch.setattr(mutmut_configuration, "_config", None)
    data = SourceFileMutationData(path=Path("src/mod.py"))
    data.load()

    wire = msgspec.json.decode(msgspec.json.encode(schema_report((data,), floor=0.85)))

    assert (wire["schemaVersion"], wire["thresholds"]) == ("2", {"high": 90, "low": 85})
    entry = wire["files"]["src/mod.py"]
    assert (entry["language"], entry["source"]) == ("python", source)
    by_id = {m["id"]: m for m in entry["mutants"]}
    assert set(by_id) == set(codes)
    assert all(set(m) == {"id", "mutatorName", "status", "location"} and m["mutatorName"] == "mutmut" for m in by_id.values())
    assert {key: by_id[key]["status"] for key in codes} == {
        "src.mod.x_alpha__mutmut_1": "Killed",
        "src.mod.x_alpha__mutmut_2": "Survived",
        "src.mod.x_alpha__mutmut_3": "Pending",
        "src.mod.xǁOwnerǁx_beta__mutmut_1": "NoCoverage",
    }
    assert by_id["src.mod.x_alpha__mutmut_1"]["location"] == {"start": {"line": 1, "column": 1}, "end": {"line": 2, "column": 13}}
    assert by_id["src.mod.xǁOwnerǁx_beta__mutmut_1"]["location"] == {"start": {"line": 6, "column": 5}, "end": {"line": 7, "column": 17}}

    assert gate() == 1  # score 0.5 under the 0.80 floor; the schema artifact still lands
    capsys.readouterr()
    emitted = msgspec.json.decode((tmp_path / ".artifacts" / "python" / "mutmut" / "mutation-report.json").read_bytes())
    assert set(emitted["files"]) == {"src/mod.py"}
    assert len(emitted["files"]["src/mod.py"]["mutants"]) == len(codes)


def test_gate_check_rides_staged_mutmut_success(assay_root: AssayHarness) -> None:
    """_gate fans one staged-worktree catalog gate-row check after mutmut success — real rows end to end."""
    row = _gate_row()
    assert (row.name, row.runner, row.groups, row.mode) == ("mutmut-gate", Runner.UV, (ToolGroup.MUTATION,), Mode.VERIFY)
    assert (row.stage.root, row.stage.project, row.input) == ("", True, Input.OWNED)
    for mode in (Mode.RUN, Mode.MUTATION, Mode.LIST):
        assert row not in _rows(_PY, TestParams(mutation=MutationLane.FULL), mode), f"gate row leaked into the {mode.value} fan"

    (staged,) = (t for t in _rows(_PY, TestParams(mutation=MutationLane.FULL), Mode.MUTATION) if t.stage.root)
    seen: list[Check] = []

    def _fake_fan(checks: tuple[Check, ...], **_k: object) -> tuple[Result[Completed, object], ...]:
        seen.extend(checks)
        return (Ok(_ok(("uv", "run", "python", "-m", "tools.assay.rails.mutation_gate"))),)

    done = (Ok(_ok(("uv", "run", "--group", "mutation", "mutmut", "run"))),)
    executor = SeamExecutor(fan_fn=_fake_fan)
    out = _gate(
        done, _PY_ROUTED, TestParams(mutation=MutationLane.FULL), settings=assay_root.settings, scope=assay_root.scope(Claim.TEST), executor=executor
    )
    assert len(out) == 1
    assert [c.tool for c in seen] == [row]
    assert seen[0].cwd == Path(str(assay_root.settings.root)) / staged.stage.root


def test_gate_skips_without_success_or_stage(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_gate stays empty without mutmut success and a staged row."""

    def _forbidden(*_a: object, **_k: object) -> tuple[Result[Completed, object], ...]:
        raise AssertionError("executor.fan must not run")

    executor = SeamExecutor(fan_fn=_forbidden)
    scope = assay_root.scope(Claim.TEST)
    params = TestParams(mutation=MutationLane.FULL)
    for argv, code in ((("uv", "run", "mutmut", "run"), 1), (("uv", "run", "pytest"), 0)):
        assert _gate((Ok(receipt(argv, code)),), _PY_ROUTED, params, settings=assay_root.settings, scope=scope, executor=executor) == ()
    # Counterfactual catalog: the real mutmut row is staged, so the stage-less arm needs the synthetic row.
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (_tool("mutmut", Mode.MUTATION),))
    done = (Ok(receipt(("uv", "run", "mutmut", "run"), 0)),)
    assert _gate(done, _PY_ROUTED, params, settings=assay_root.settings, scope=scope, executor=executor) == ()


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
