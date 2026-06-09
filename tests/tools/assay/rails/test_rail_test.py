"""Law-matrix for tools.assay.rails.test [TestParams, coverage, coverage_percent, list, run]."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import dataclasses
from pathlib import Path
from typing import TYPE_CHECKING

from dirty_equals import IsInt
from expression import Ok
from expression.collections import block
import pytest

from tests._aspect import register_law  # noqa: PLC2701
from tests._spec import assert_ok, support_matrix, validity_matrix, ValidityCase  # noqa: PLC2701
from tools.assay.composition.settings import ArtifactScope
from tools.assay.core.model import ArtifactKind, Check, Claim, fold, Input, Language, Mode, MutationLane, receipt, Runner, TestRun, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
from tools.assay.rails import test as test_rail
from tools.assay.rails.test import (
    _adopt_coverage,  # noqa: PLC2701
    _checks,  # noqa: PLC2701
    _detail,  # noqa: PLC2701
    _dispatch,  # noqa: PLC2701
    _dispatch_all,  # noqa: PLC2701
    _eligible,  # noqa: PLC2701
    _filter,  # noqa: PLC2701
    _roster_matches,  # noqa: PLC2701
    _routed,  # noqa: PLC2701
    _scoped_mutation,  # noqa: PLC2701
    _select,  # noqa: PLC2701
    _thin_rail,  # noqa: PLC2701
    _unsupported_scope,  # noqa: PLC2701
    coverage_percent,
    TestParams,
)


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.core.model import Completed


# --- [CONSTANTS] ------------------------------------------------------------------------

_COV_ARGV: tuple[str, ...] = ("uv", "run", "coverage", "report", "--format=total")
_OTHER_ARGV: tuple[str, ...] = ("uv", "run", "pytest")
_PY = Language.PYTHON
_PY_ROUTED = Routed(language=_PY, scope=Scope.CHANGED)


# --- [OPERATIONS] -----------------------------------------------------------------------
# Bare-Completed builders thread the shared model.receipt primitive; status is passed through verbatim
# where a law gates on it (rc=0 derives EMPTY, not OK), and the rail seams wire one canned-receipt fan.


def _ok(argv: tuple[str, ...], stdout: bytes = b"", *, status: RailStatus = RailStatus.OK) -> Completed:
    return receipt(argv, 0, status=status, stdout=stdout)


def _wire(monkeypatch: pytest.MonkeyPatch, *outcomes: Completed, routed: Routed = _PY_ROUTED, seam: str = "_dispatch") -> None:
    # One canned route + one canned dispatch/dispatch_all fan; every list/run/_thin_rail law shares this seam.
    monkeypatch.setattr(test_rail, "_routed", lambda *_a, **_k: Ok(block.of_seq((routed,))))
    monkeypatch.setattr(test_rail, seam, lambda *_a, **_k: tuple(Ok(o) for o in outcomes))


def _capture_rail(monkeypatch: pytest.MonkeyPatch) -> list[tuple[TestParams, object, object]]:
    # Collapse the verb-level `_fake_thin_rail` closure (params/claim/verb capture) shared by run/coverage.
    seen: list[tuple[TestParams, object, object]] = []

    def _fake(settings: object, scope: object, params: TestParams, *, claim: object, verb: object, mode: object) -> Result[object, object]:
        seen.append((params, claim, verb))
        return Ok(object())

    monkeypatch.setattr(test_rail, "_thin_rail", _fake)
    return seen


def _tool(name: str = "pytest", mode: Mode = Mode.RUN, runner: Runner = Runner.UV, language: Language = _PY) -> Tool:
    """Construct a minimal catalog-shaped Tool for eligibility / scoping tests.

    Returns:
        Tool with the given name, mode, runner, and language targeting TEST.
    """
    return Tool(name=name, runner=runner, command=(name,), input=Input.NONE, language=language, claim=Claim.TEST, mode=mode)


# --- [LAW_COVERAGE] ---------------------------------------------------------------------
# Import-time registry: every falsifiable subject below carries a law; the gate (tests._aspect) is TOTAL.

register_law(coverage_percent, "coverage_percent_parametric")
register_law(coverage_percent, "coverage_percent_first_match_wins")
register_law(coverage_percent, "coverage_percent_non_coverage_report_prefix_ignored")
register_law(coverage_percent, "coverage_percent_returncode_agnostic")
register_law(coverage_percent, "coverage_percent_argv_prefix_gate")
register_law(coverage_percent, "adopt_coverage_artifact_fields")

register_law(TestParams, "testparams_default_invariants")
register_law(TestParams, "testparams_frozen")
register_law(TestParams, "testparams_mutation_lane_validity")
register_law(TestParams, "testparams_coverage_benchmark_exclusive_intent")
register_law(TestParams, "eligible_branch_matrix")
register_law(TestParams, "filter_discriminant_arms")
register_law(TestParams, "scoped_mutation_arms")
register_law(TestParams, "checks_splice_arms")
register_law(TestParams, "unsupported_scope_arms")
register_law(TestParams, "select_arms")
register_law(TestParams, "dispatch_empty_checks_returns_unsupported_only")
register_law(TestParams, "dispatch_non_empty_checks_calls_fan_out")
register_law(TestParams, "roster_matches_skip_and_kind_arms")
register_law(TestParams, "detail_mutation_and_coverage_arms")

register_law(test_rail.coverage, "coverage_forces_coverage_flag")
register_law(test_rail.coverage, "coverage_verb_claim_is_test")

register_law(test_rail.list, "list_report_projection_arms")
register_law(test_rail.list, "list_discovery_failure_note")
register_law(test_rail.list, "list_status_ok_even_with_zero_roster")
register_law(test_rail.list, "list_roster_artifact_written_to_scope")

register_law(test_rail.run, "run_ok_report_from_dispatch")
register_law(test_rail.run, "run_mutation_gap_note_emitted")
register_law(test_rail.run, "run_coverage_percent_populates_detail")
register_law(test_rail.run, "run_claim_is_test")
register_law(test_rail.run, "dispatch_all_mode_arms")
register_law(test_rail.run, "thin_rail_gap_note_emitted_in_report_notes")
register_law(test_rail.run, "thin_rail_mutation_eligible_calls_leased")
register_law(test_rail.run, "thin_rail_no_mutation_calls_work_directly")
register_law(test_rail.run, "routed_yields_ok_for_valid_languages")
register_law(test_rail.run, "routed_multi_language_combines_results")


# --- [LAWS: coverage_percent] -----------------------------------------------------------


@pytest.mark.parametrize(
    "done,expected",
    [
        pytest.param((_ok(_COV_ARGV, b"87.5\n"),), 87.5, id="integer-like-percent"),
        pytest.param((_ok(_COV_ARGV, b"100\n"),), 100.0, id="whole-number-100"),
        pytest.param((_ok(_COV_ARGV, b"0\n"),), 0.0, id="zero-as-float"),
        pytest.param((_ok(_COV_ARGV, b"72.345\n"),), 72.345, id="high-precision"),
        pytest.param((_ok(_OTHER_ARGV, b"87.5\n"),), None, id="non-coverage-argv"),
        pytest.param((), None, id="empty-done"),
        pytest.param((_ok(_COV_ARGV, b"no coverage data\n"),), None, id="non-numeric-stdout"),
        pytest.param((_ok(_COV_ARGV, b"\n"),), None, id="blank-stdout"),
        # Returncode-agnostic: fail_under is the coverage tool's concern, not the parser's.
        pytest.param((receipt(_COV_ARGV, 2, stdout=b"87.5\n"),), 87.5, id="returncode-agnostic"),
    ],
)
def test_coverage_percent_parametric(done: tuple[Completed, ...], expected: float | None) -> None:
    """coverage_percent is a pure projection: argv[:4] prefix gates entry; stdout strip→float; non-numeric → None."""
    result = coverage_percent(done)
    assert result is None if expected is None else result == pytest.approx(expected)


@pytest.mark.parametrize(
    "done,expected",
    [
        pytest.param((_ok(_COV_ARGV, b"55.0\n"), _ok(_COV_ARGV, b"99.0\n")), 55.0, id="first-match-wins"),
        pytest.param((_ok(_OTHER_ARGV, b"55.0\n"), _ok(_COV_ARGV, b"88.0\n")), 88.0, id="non-coverage-prefix-ignored"),
    ],
)
def test_coverage_percent_first_match_wins(done: tuple[Completed, ...], expected: float) -> None:
    """coverage_percent returns the FIRST matching coverage-report receipt; unrelated argv never pollutes it."""
    assert coverage_percent(done) == pytest.approx(expected)


@pytest.mark.parametrize(
    "prefix,is_coverage",
    [
        pytest.param(("uv", "run", "coverage", "report"), True, id="exact-prefix"),
        pytest.param(_COV_ARGV, True, id="extended-prefix"),
        pytest.param(("uv", "run", "coverage", "json"), False, id="coverage-json-not-report"),
        pytest.param(_OTHER_ARGV, False, id="pytest-not-report"),
        pytest.param(("coverage", "report"), False, id="bare-coverage-not-prefixed"),
    ],
)
def test_coverage_percent_argv_prefix_gate(prefix: tuple[str, ...], is_coverage: bool) -> None:  # noqa: FBT001
    """coverage_percent gates strictly on argv[:4] == ('uv','run','coverage','report')."""
    result = coverage_percent((_ok(prefix, b"75.0\n"),))
    assert result == pytest.approx(75.0) if is_coverage else result is None


# --- [LAWS: TestParams] -----------------------------------------------------------------


def test_testparams_default_invariants() -> None:
    """TestParams default construction produces a coherent zero-state parameter bag."""
    p = TestParams()
    support_matrix(
        ("target is None", lambda: p.target is None, True),
        ("all is False", lambda: p.all is False, True),
        ("no_build is False", lambda: p.no_build is False, True),
        ("mutation is OFF", lambda: p.mutation is MutationLane.OFF, True),
        ("benchmark is False", lambda: p.benchmark is False, True),
        ("coverage is False", lambda: p.coverage is False, True),
        ("filter is empty", lambda: not p.filter, True),
        ("limit is 0", lambda: p.limit == 0, True),
        ("grep is empty", lambda: not p.grep, True),
    )


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
    """coverage=True + benchmark=True is ACCEPTED by TestParams (the rail, not the bag, excludes benchmark)."""
    p = TestParams(coverage=True, benchmark=True)
    assert (p.coverage, p.benchmark) == (True, True)


# --- [LAWS: _eligible — branch matrix] --------------------------------------------------


@pytest.mark.parametrize(
    "tool,params,expected",
    [
        pytest.param(_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.OFF), False, id="mutation-OFF→F"),
        pytest.param(_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.CHANGED), True, id="mutation-CHANGED→T"),
        pytest.param(_tool("mutmut", Mode.MUTATION), TestParams(mutation=MutationLane.FULL), True, id="mutation-FULL→T"),
        pytest.param(_tool("dotnet-restore", Mode.RESTORE, Runner.DOTNET), TestParams(no_build=True), False, id="restore-no-build→F"),
        pytest.param(_tool("dotnet-restore", Mode.RESTORE, Runner.DOTNET), TestParams(no_build=False), True, id="restore-build→T"),
        pytest.param(_tool("dotnet-build", Mode.BUILD, Runner.DOTNET), TestParams(no_build=True), False, id="build-no-build→F"),
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
def test_eligible_branch_matrix(tool: Tool, params: TestParams, expected: bool) -> None:  # noqa: FBT001
    """_eligible projects every (mode, params) arm to the correct True/False gate."""
    assert _eligible(tool, params) is expected


# --- [LAWS: _filter — discriminant arms] ------------------------------------------------


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


# --- [LAWS: _scoped_mutation — match arms] ----------------------------------------------


def _mutation_tool(name: str = "mutmut") -> Tool:
    return _tool(name, Mode.MUTATION, Runner.UV)


def _stryker_tool() -> Tool:
    # dotnet-stryker is the only entry in _MUTATION_SCOPE; it scopes CHANGED to repeatable --mutate <file> flags.
    return Tool(
        name="dotnet-stryker",
        runner=Runner.DOTNET,
        command=("tool", "run", "dotnet-stryker", "--"),
        input=Input.PROJECT,
        language=Language.CSHARP,
        claim=Claim.TEST,
        mode=Mode.MUTATION,
    )


def test_scoped_mutation_arms() -> None:
    """_scoped_mutation: CHANGED+unscoped runner→None; CHANGED+scoped→--mutate splice; FULL/non-mutation→passthrough."""
    mutmut, stryker, plain = _mutation_tool(), _stryker_tool(), _tool("pytest", Mode.RUN)
    files = ("src/Foo.cs", "src/Bar.cs")

    # CHANGED on a runner absent from _MUTATION_SCOPE → None → UNSUPPORTED downstream.
    assert _scoped_mutation(mutmut, TestParams(mutation=MutationLane.CHANGED), ("src/foo.py",)) is None
    # FULL bypasses scoping; non-mutation rows fall through the wildcard arm — both return the tool unchanged.
    assert _scoped_mutation(mutmut, TestParams(mutation=MutationLane.FULL), ("src/foo.py",)) is mutmut
    assert _scoped_mutation(plain, TestParams(mutation=MutationLane.CHANGED), ()) is plain
    # CHANGED on dotnet-stryker (in _MUTATION_SCOPE) appends one --mutate <file> pair per changed file.
    scoped = _scoped_mutation(stryker, TestParams(mutation=MutationLane.CHANGED), files)
    assert scoped is not None
    assert scoped.command == ("tool", "run", "dotnet-stryker", "--", "--mutate", "src/Foo.cs", "--mutate", "src/Bar.cs")


# --- [LAWS: _checks — splice logic] -----------------------------------------------------


def test_checks_splice_arms(monkeypatch: pytest.MonkeyPatch) -> None:
    """_checks: DOTNET RUN-mode tools get filter flags spliced; UV runners and MUTATION rows do not; None rows drop."""
    dotnet = Tool(
        name="dotnet-test", runner=Runner.DOTNET, command=("test",), input=Input.PROJECT, language=Language.CSHARP, claim=Claim.TEST, mode=Mode.RUN
    )

    # DOTNET RUN + filter → --filter-method injected.
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (dotnet,))
    csharp_routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, files=())
    spliced = _checks(csharp_routed, TestParams(filter="test_something"), Mode.RUN)
    assert any("--filter-method" in c.tool.command for c in spliced)

    # CHANGED mutation tool absent from _MUTATION_SCOPE → _scoped_mutation None → row excluded.
    mutmut = _mutation_tool()
    py_routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/a.py",))
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (mutmut,))
    assert all(c.tool.name != "mutmut" for c in _checks(py_routed, TestParams(mutation=MutationLane.CHANGED), Mode.MUTATION))

    # MUTATION-mode rows never receive --filter-class even when a filter is set (passthrough arm).
    monkeypatch.setattr(test_rail, "_scoped_mutation", lambda t, *_a, **_k: t)
    mut_checks = _checks(py_routed, TestParams(filter="MyTests"), Mode.MUTATION)
    assert all("--filter-class" not in c.tool.command for c in mut_checks)

    # UV runner + filter set → wildcard arm returns the tool unchanged (no flags injected for non-DOTNET).
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (_tool("pytest", Mode.RUN, Runner.UV),))
    uv_checks = _checks(py_routed, TestParams(filter="test_something"), Mode.RUN)
    assert len(uv_checks) == 1
    assert "--filter-method" not in uv_checks[0].tool.command


# --- [LAWS: _unsupported_scope — CHANGED vs non-CHANGED] --------------------------------


def test_unsupported_scope_arms(monkeypatch: pytest.MonkeyPatch) -> None:
    """_unsupported_scope: CHANGED mutation→UNSUPPORTED receipt for unscoped runners; FULL/non-mutation→empty."""
    mutmut = _mutation_tool()
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=("src/foo.py",))
    monkeypatch.setattr(test_rail, "_rows", lambda *_a, **_k: (mutmut,))

    results = _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), Mode.MUTATION)
    assert len(results) == 1
    completed = assert_ok(results[0])
    assert completed.status is RailStatus.UNSUPPORTED
    assert any("mutmut" in n for n in completed.notes)

    # FULL lane → scoping inapplicable; non-MUTATION mode → empty regardless of lane.
    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.FULL), Mode.MUTATION) == ()
    assert _unsupported_scope(routed, TestParams(mutation=MutationLane.CHANGED), Mode.RUN) == ()


# --- [LAWS: _select — target/all/glob arms] ---------------------------------------------


def test_select_arms(assay_root: AssayHarness) -> None:
    """_select: glob languages passthrough; target narrows projects; all unions test_target; csharp-no-target falls through."""
    settings = assay_root.settings
    rasm = "tests/csharp/libs/Rasm/Rasm.Tests.csproj"
    py_routed = Routed(language=_PY, scope=Scope.CHANGED, projects=(rasm,))
    cs_routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED, projects=(rasm,))
    target = Path("tests/csharp/libs/Other/Other.Tests.csproj")

    # Glob-strategy (Python) ignores target/all; CSHARP with no target & all=False falls through — both unchanged.
    assert _select(py_routed, TestParams(), settings) is py_routed
    assert _select(cs_routed, TestParams(), settings) is cs_routed
    # --target narrows routed.projects to the chosen csproj; --all unions the default test target with the closure.
    assert _select(cs_routed, TestParams(target=target), settings).projects == (str(target),)
    unioned = _select(cs_routed, TestParams(all=True), settings).projects
    assert {str(settings.test_target), rasm} <= set(unioned)


# --- [LAWS: _dispatch — checks/unsupported branching] -----------------------------------


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


# --- [LAWS: _roster_matches — skip / kind arms] -----------------------------------------


def test_roster_matches_skip_and_kind_arms() -> None:
    """_roster_matches: skips 'The ' headers + 'N test(s) found' summaries; drops rc!=0 and empty stdout; kind=PROCESS."""
    # rc=0 with mixed header / summary / real rows: headers and summaries drop, real rows survive as PROCESS.
    mixed = _ok(("dotnet", "test", "--list-tests"), b"The following tests are available:\nMyNs.MyClass.test_one\n3 test(s) found\n")
    matches = _roster_matches((mixed,))
    assert [m.id for m in matches] == ["MyNs.MyClass.test_one"]
    assert all(m.kind is ArtifactKind.PROCESS for m in matches)

    # rc!=0 and empty stdout both yield nothing (gate is `returncode == 0 and stdout`).
    assert _roster_matches((receipt(("dotnet", "test", "--list-tests"), 1, stdout=b"X.test\n"),)) == ()
    assert _roster_matches((_ok(("pytest", "--collect-only")),)) == ()


# --- [LAWS: _detail — mutation/coverage arms] -------------------------------------------


@pytest.mark.parametrize(
    "done,params,check",
    [
        pytest.param((), TestParams(mutation=MutationLane.OFF, coverage=False), None, id="off+no-coverage→None"),
        pytest.param(
            (_ok(("mutmut", "run"), b"not-json\n"),), TestParams(mutation=MutationLane.FULL, coverage=False), None, id="full+noise-stdout→None"
        ),
        pytest.param((_ok(("uv", "run", "pytest")),), TestParams(mutation=MutationLane.OFF, coverage=True), None, id="coverage+no-percent→None"),
        pytest.param((_ok(_COV_ARGV, b"91.5\n"),), TestParams(mutation=MutationLane.OFF, coverage=True), 91.5, id="coverage+percent→TestRun"),
    ],
)
def test_detail_mutation_and_coverage_arms(done: tuple[Completed, ...], params: TestParams, check: float | None) -> None:
    """_detail: OFF+no-coverage→None; FULL+undecoded-TestRun→None; coverage-without-percent→None; coverage+percent→TestRun."""
    result = _detail(done, params)
    match check:
        case None:
            assert result is None
        case _:
            assert isinstance(result, TestRun)
            assert result.coverage == pytest.approx(check)


# --- [LAWS: _adopt_coverage — artifact construction] ------------------------------------


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


# --- [LAWS: coverage (verb)] ------------------------------------------------------------


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


# --- [LAWS: list (verb)] ----------------------------------------------------------------


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
    """List projects discovery rows into the report: Ok status + roster artifact, case-insensitive grep, limit + note."""
    _wire(monkeypatch, _ok(("pytest", "--collect-only"), stdout))
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), params))
    assert assertion(report)


def test_list_discovery_failure_note(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List emits diagnostic notes for failed discovery rows without faulting the report."""
    failed = receipt(("dotnet", "test", "--list-tests"), 1, stderr=b"no project")
    _wire(monkeypatch, failed)
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()))
    assert any("discovery" in n and "dotnet test" in n for n in report.notes)


def test_list_status_ok_even_with_zero_roster(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List with empty discovery still yields Ok (status folded from receipts, not the discovered count)."""
    _wire(monkeypatch, _ok(("pytest", "--collect-only")))
    assert test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()).is_ok()


def test_list_roster_artifact_written_to_scope(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """List writes the roster artifact into the scope store and surfaces it in report.artifacts."""
    _wire(monkeypatch, _ok(("pytest", "--collect-only"), b"tests/a.py::test_one\n"))
    report = assert_ok(test_rail.list(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()))
    assert "test-roster" in {a.id for a in report.artifacts}


# --- [LAWS: run (verb)] -----------------------------------------------------------------


def test_run_ok_report_from_dispatch(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run produces an Ok report when dispatch completes without fault."""
    _wire(monkeypatch, _ok(("pytest",)), seam="_dispatch_all")
    report = assert_ok(test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams()))
    assert report.status in {RailStatus.OK, RailStatus.EMPTY}


def test_run_mutation_gap_note_emitted(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run with mutation=FULL emits the gap note when language has no eligible mutation runner.

    TypeScript has no mutation runner in the catalog; narrowing language to TYPESCRIPT forces the gap.
    """
    routed = Routed(language=Language.TYPESCRIPT, scope=Scope.CHANGED)
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")
    params = TestParams(mutation=MutationLane.FULL, language=Language.TYPESCRIPT)
    report = assert_ok(test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), params))
    assert any("mutation" in n for n in report.notes)


def test_run_coverage_percent_populates_detail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run with coverage=True propagates the coverage percent into the TestRun detail."""
    _wire(monkeypatch, _ok(_COV_ARGV, b"82.5\n"), seam="_dispatch_all")
    report = assert_ok(test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams(coverage=True)))
    assert isinstance(report.detail, TestRun)
    assert report.detail.coverage == pytest.approx(82.5)


def test_run_claim_is_test(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Run passes Claim.TEST through _thin_rail so the envelope claim is correct."""
    seen = _capture_rail(monkeypatch)
    test_rail.run(assay_root.settings, ArtifactScope.open(assay_root.settings, Claim.TEST), TestParams())
    assert [claim for _, claim, _ in seen] == [Claim.TEST]


# --- [LAWS: _dispatch_all — mode arms] --------------------------------------------------


@pytest.mark.parametrize(
    "params,expected_modes",
    [
        pytest.param(TestParams(mutation=MutationLane.FULL), {Mode.RUN, Mode.MUTATION}, id="mutation→adds-MUTATION"),
        pytest.param(TestParams(coverage=True), {Mode.RUN, Mode.CLIENT}, id="coverage→adds-CLIENT"),
        pytest.param(TestParams(), {Mode.RUN}, id="plain→RUN-only"),
    ],
)
def test_dispatch_all_mode_arms(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, params: TestParams, expected_modes: set[Mode]) -> None:
    """_dispatch_all fans the RUN dispatch plus MUTATION (when mutation!=OFF) and CLIENT (when coverage) sub-dispatches."""
    call_modes: list[Mode] = []

    def _record(*_a: object, mode: Mode, **_k: object) -> tuple[Result[Completed, object], ...]:
        call_modes.append(mode)
        return (Ok(_ok(("pytest",))),)

    monkeypatch.setattr(test_rail, "_dispatch", _record)
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    _dispatch_all(routed, params, settings=assay_root.settings, scope=scope, mode=Mode.RUN)
    assert set(call_modes) == expected_modes


# --- [LAWS: _routed — sequence composition] ---------------------------------------------


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


# --- [LAWS: _thin_rail — gap / lease / direct-work arms] --------------------------------


def test_thin_rail_gap_note_emitted_in_report_notes(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail emits the gap note in report.notes when mutation is requested but no eligible runner exists."""
    routed = Routed(language=Language.TYPESCRIPT, scope=Scope.CHANGED, files=())
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    params = TestParams(mutation=MutationLane.FULL, language=Language.TYPESCRIPT)
    report = assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert any("mutation" in n for n in report.notes)


def test_thin_rail_mutation_eligible_calls_leased(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail acquires the 'mutation' lease when at least one eligible tool has mode=MUTATION."""
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

    params = TestParams(mutation=MutationLane.FULL, language=_PY)
    assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert leased_calls == ["mutation"]


def test_thin_rail_no_mutation_calls_work_directly(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_thin_rail with mutation=OFF calls _work() directly (no lease), returning Ok."""
    routed = Routed(language=_PY, scope=Scope.CHANGED, files=())
    _wire(monkeypatch, _ok(("pytest",)), routed=routed, seam="_dispatch_all")
    scope = ArtifactScope.open(assay_root.settings, Claim.TEST)
    params = TestParams(mutation=MutationLane.OFF, language=_PY)
    report = assert_ok(_thin_rail(assay_root.settings, scope, params, claim=Claim.TEST, verb="run", mode=Mode.RUN))
    assert report.status in {RailStatus.OK, RailStatus.EMPTY}
