"""Law matrix for the unified static rail route."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Error, Ok
import msgspec
import pytest

from tests.python._testkit.spec import assert_error_status, assert_ok
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.composition.registry import REGISTRY
from tools.assay.core.model import Check, Claim, Fault, Input, Language, Mode, RailStatus, receipt, Runner, StaticRun, Tool
from tools.assay.core.routing import Routed, Scope, TargetFiles
from tools.assay.diagnostics import sarif_status
import tools.assay.rails.static as static_rail
from tools.assay.rails.static import run, StaticParams


if TYPE_CHECKING:
    from collections.abc import Callable
    from pathlib import Path

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness, VerbRunner
    from tools.assay.core.model import Completed


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (StaticParams, run, sarif_status)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok_static_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
    return tuple(Ok(receipt((check.tool.name,), 0, status=RailStatus.OK)) for check in checks)


def _compiling_probe(check: Check, **_kw: object) -> Result[Completed, Fault]:
    return Ok(receipt((check.tool.name,), 0, status=RailStatus.OK))


def _failing_probe(check: Check, **_kw: object) -> Result[Completed, Fault]:
    return Ok(receipt((check.tool.name,), 1, status=RailStatus.FAILED))


def _recording_fan(calls: list[tuple[Mode, ...]]) -> Callable[..., tuple[Result[Completed, Fault], ...]]:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append(tuple(check.tool.mode for check in checks))
        return _ok_static_fan(checks)

    return fan


# --- [VERB_SURFACE_LAWS]


def test_registry_exposes_one_polymorphic_static_verb() -> None:
    """The static claim collapses to exactly one verb; no check/build/fix split remains."""
    assert tuple(row.verb for row in REGISTRY if row.claim is Claim.STATIC) == ("static",)


def test_cli_consumes_grouped_folder_and_file_targets(cli: VerbRunner, assay_root: AssayHarness) -> None:
    """Grouped --folder and --file flags consume multiple targets."""
    assay_root.write("src/a.py", "")
    assay_root.write("pkg/b.py", "")
    assay_root.write("single/c.py", "")
    result = cli("static", "--folder", "src", "pkg", "--file", "single/c.py", executor=SeamExecutor(fan_fn=_ok_static_fan))
    report = result.envelope.report
    assert report is not None
    assert isinstance(report.detail, StaticRun)
    assert report.detail.targets == (("folder", "src"), ("folder", "pkg"), ("file", "single/c.py"))
    assert any(row[0] == "python" and row[2] == "3" for row in report.detail.routes)


def test_static_help_admits_scoped_target_flags(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """``static --help`` is the root leaf and admits the full value-driven target surface."""
    from tools.assay import __main__ as main_mod  # noqa: PLC0415

    neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
    monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
    code = main_mod.main(["static", "--help"])
    cap = capsysbinary.readouterr()
    assert code == 0
    assert b"--all" in cap.out
    assert b"--project" in cap.out
    assert b"--folder" in cap.out
    assert b"--file" in cap.out
    assert b"--no-all" not in cap.out


# --- [LANE_LAWS]


def test_python_lane_runs_fix_before_diagnostics(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Python static runs fix rows before diagnostics."""
    assay_root.write("tools/assay/probe.py", "")
    calls: list[tuple[Mode, ...]] = []
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    executor = SeamExecutor(fan_fn=_recording_fan(calls))
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("tools/assay",)), executor))
    assert isinstance(report.detail, StaticRun)
    assert report.detail.phases == ("fix", "diagnostic")
    write_index = next(i for i, modes in enumerate(calls) if Mode.WRITE in modes)
    check_index = next(i for i, modes in enumerate(calls) if Mode.CHECK in modes)
    assert write_index < check_index
    assert report.notes[0] == f"planned={len(report.detail.planned)} skipped={len(report.detail.skipped)}"


def test_csharp_project_lane_runs_full_ordered_lane(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """A C# project target runs the full ordered lane: fix, diagnostics, restore, then build."""
    assay_root.write("src/App/App.csproj", "<Project />")
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    executor = SeamExecutor(run_fn=_compiling_probe, fan_fn=_ok_static_fan)
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(project="src/App/App.csproj"), executor))
    assert isinstance(report.detail, StaticRun)
    assert report.detail.phases == ("fix", "diagnostic", "restore", "build")
    assert any(row[0] == "csharp" and row[3] == "1" for row in report.detail.routes)
    assert any("dotnet build" in argv and "src/App/App.csproj" in argv for _, _, argv in report.detail.planned)


def test_folder_lane_spans_python_typescript_and_csharp(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """A mixed folder runs the admitted fix + diagnostic + build rows for every language touched."""
    assay_root.write("src/App/App.csproj", "<Project />")
    assay_root.write("src/App/a.cs", "class A {}")
    assay_root.write("src/pkg/a.py", "")
    assay_root.write("src/web/a.ts", "")
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    executor = SeamExecutor(run_fn=_compiling_probe, fan_fn=_ok_static_fan)
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("src",)), executor))
    assert isinstance(report.detail, StaticRun)
    planned_names = {name for _, name, _ in report.detail.planned}
    assert {"ruff", "ruff-format", "ty", "mypy", "lint-imports", "ast-grep-py", "py-analyzer"} <= planned_names
    assert {"biome", "ast-grep-ts"} <= planned_names
    assert {"dotnet-format", "dotnet-restore", "dotnet-build"} <= planned_names
    assert report.counts.total == len(report.detail.planned)


def test_empty_target_is_changed_default_not_a_fault(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Empty target input routes through the changed-default lane."""
    monkeypatch.setattr(static_rail, "target_files", lambda *_a, **_k: Ok(TargetFiles()))
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(), SeamExecutor(fan_fn=_ok_static_fan)))
    assert isinstance(report.detail, StaticRun)


def test_unsupported_file_target_is_skipped_not_faulted(assay_root: AssayHarness) -> None:
    """An unsupported --file surfaces as a skipped target row, not the removed hard execution fault."""
    executor = SeamExecutor(fan_fn=_ok_static_fan)
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(files=("Workspace.slnx",)), executor))
    assert isinstance(report.detail, StaticRun)
    assert any(kind == "file" and "Workspace.slnx" in path for kind, path, _ in report.detail.skipped)


def test_only_one_target_axis_admitted(assay_root: AssayHarness) -> None:
    """The value-driven parse rejects combining --all with --project or folder/file targets."""
    fault = assert_error_status(
        run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(all=True, project="src/App/App.csproj"), SeamExecutor()),
        RailStatus.UNSUPPORTED,
    )
    assert "choose only one" in fault.message


# --- [SELECTION_LAWS]

# White-box seams below (_phase_checks/_planned/_build_fan/_dispatch) prove selection and phase-gating
# decisions the public verb cannot isolate per-language without spawning real toolchains.


def test_typescript_scope_gates_tsc_and_keeps_scoped_fixer(assay_root: AssayHarness) -> None:
    """Full TS keeps the project-wide tsc build row; scoped TS skips it whole while the scoped Biome write fixer stays."""
    full_phases, full_skipped = static_rail._phase_checks(
        Routed(Language.TYPESCRIPT, Scope.FULL, files=(".",)), assay_root.settings, assay_root.scope(Claim.STATIC)
    )
    scoped_phases, scoped_skipped = static_rail._phase_checks(
        Routed(Language.TYPESCRIPT, Scope.CHANGED, files=("src/web/a.ts",)), assay_root.settings, assay_root.scope(Claim.STATIC)
    )
    assert any(check.tool.name == "tsc" for _, checks in full_phases for check in checks)
    assert not any(row[1] == "tsc" for row in full_skipped)
    assert all(check.tool.name != "tsc" for _, checks in scoped_phases for check in checks)
    assert ("build", "tsc", "project-wide tool unsupported by scoped static") in scoped_skipped
    assert any(check.tool.name == "biome" and check.tool.mode is Mode.WRITE for _, checks in scoped_phases for check in checks)


def test_full_typescript_tsc_has_no_file_tail(assay_root: AssayHarness) -> None:
    """Full TypeScript build invokes the project owner once; it never mixes ``-p`` with file tails."""
    routed = Routed(Language.TYPESCRIPT, Scope.FULL, files=("vite.config.ts", "vitest.config.ts"))
    phases, _ = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned(routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert ("build", "tsc", "pnpm exec tsc --noEmit -p tsconfig.base.json") in planned


_PLACEMENT_ROWS: tuple[tuple[str, Routed, Callable[[tuple[tuple[str, str, str], ...]], bool]], ...] = (
    (
        "changed-files-bind-project-plus-include",
        Routed(
            Language.CSHARP,
            Scope.CHANGED,
            files=("src/App/a.cs",),
            projects=("src/App/App.csproj",),
            groups=(("src/App/App.csproj", ("src/App/a.cs",)),),
        ),
        lambda planned: any("dotnet format" in argv and "src/App/App.csproj --include src/App/a.cs" in argv for _, _, argv in planned),
    ),
    (
        "direct-project-binds-project-not-empty-include",
        Routed(Language.CSHARP, Scope.CHANGED, projects=("src/App/App.csproj",)),
        lambda planned: any(argv.endswith("src/App/App.csproj") for _, name, argv in planned if name == "dotnet-format"),
    ),
)


@pytest.mark.parametrize("routed, admitted", [row[1:] for row in _PLACEMENT_ROWS], ids=[row[0] for row in _PLACEMENT_ROWS])
def test_csharp_format_placement_matrix(
    assay_root: AssayHarness, routed: Routed, admitted: Callable[[tuple[tuple[str, str, str], ...]], bool]
) -> None:
    """C# format rows bind to the owner project (plus ``--include`` file tails when files route), never Workspace.slnx."""
    phases, skipped = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned(routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert admitted(planned)


def test_csharp_workspace_format_uses_solution_placement(assay_root: AssayHarness) -> None:
    """Explicit all-workspace routes place dotnet format/restore/build on Workspace.slnx."""
    routed = Routed(Language.CSHARP, Scope.FULL, full_triggers=(str(assay_root.settings.solution),))
    phases, skipped = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned(routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert all(str(assay_root.settings.solution) in argv for _, name, argv in planned if name.startswith("dotnet-"))
    assert any(f"-maxCpuCount:{assay_root.settings.dotnet_max_cpu}" in argv for _, name, argv in planned if name == "dotnet-build")
    assert all("--disable-build-servers" not in argv for _, _, argv in planned)


def test_csharp_build_checks_use_distinct_sarif_dirs(assay_root: AssayHarness) -> None:
    """Expanded C# build checks write SARIF into per-invocation directories, not one shared project-name path."""
    scope = assay_root.scope(Claim.STATIC)
    routed = Routed(Language.CSHARP, Scope.CHANGED, projects=("src/App/App.csproj", "src/Lib/Lib.csproj"))
    phases, skipped = static_rail._phase_checks(routed, assay_root.settings, scope)
    assert skipped == ()
    sarif_dirs = tuple(check.args.sarif_dir for phase, checks in phases if phase is static_rail.Phase.BUILD for check in checks)
    assert len(sarif_dirs) == 2
    assert len(set(sarif_dirs)) == 2
    assert all(path.startswith(f"{scope.sarif_dir}/") for path in sarif_dirs)
    assert all(f"-p:CspSarifDir={path}" not in check.tool.command for path in sarif_dirs for _, checks in phases for check in checks), (
        "the SARIF drop dir is a typed splice value, never a command-surgery token"
    )


# --- [BUILD_PHASE_LAWS]


def test_build_fan_restores_before_build_and_skips_after_restore_failure(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Restore failure blocks closure build instead of racing --no-restore."""
    routed = Routed(Language.CSHARP, Scope.CHANGED, files=("src/App/a.cs",), projects=("src/App/App.csproj",))
    restore = Check(Tool("restore", Runner.DIRECT, ("restore",), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.RESTORE))
    compile_check = Check(Tool("compile", Runner.DIRECT, ("compile",), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD))
    calls: list[tuple[Mode, ...]] = []

    def fake_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append(tuple(check.tool.mode for check in checks))
        return (Error(Fault(("restore",), RailStatus.FAILED, "restore failed")),)

    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    result = static_rail._build_fan(
        ((static_rail.Phase.RESTORE, (restore,)), (static_rail.Phase.BUILD, (compile_check,))),
        routed,
        assay_root.settings,
        SeamExecutor(fan_fn=fake_fan),
    )
    assert calls == [(Mode.RESTORE,)]
    skipped = assert_ok(result[1])
    assert skipped.status is RailStatus.SKIP
    assert skipped.notes == ("restore failed; build skipped",)


# --- [PROBE_GATE_LAWS]


def _csharp_closure_phases() -> static_rail.PhaseChecks:
    fmt_write = Check(
        Tool("dotnet-format", Runner.DOTNET, ("format", "--severity", "error"), Input.INCLUDE, Language.CSHARP, Claim.STATIC, mode=Mode.WRITE)
    )
    fmt_check = Check(Tool("dotnet-format", Runner.DOTNET, ("format", "--verify-no-changes"), Input.INCLUDE, Language.CSHARP, Claim.STATIC))
    restore = Check(Tool("dotnet-restore", Runner.DOTNET, ("restore",), Input.PROJECT, Language.CSHARP, Claim.STATIC, mode=Mode.RESTORE))
    build = Check(
        Tool("dotnet-build", Runner.DOTNET, ("build", "--no-restore"), Input.PROJECT, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD),
        tail=("src/App/App.csproj",),
    )
    return (
        (static_rail.Phase.FIX, (fmt_write,)),
        (static_rail.Phase.DIAGNOSTIC, (fmt_check,)),
        (static_rail.Phase.RESTORE, (restore,)),
        (static_rail.Phase.BUILD, (build,)),
    )


@pytest.mark.parametrize("compiles", [True, False], ids=["compiling", "non-compiling"])
def test_format_gate_follows_compile_probe(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness, *, compiles: bool) -> None:
    """A non-compiling probe drops both dotnet-format rows whole; a compiling probe leaves the full lane intact.

    The closure restore and build run either way — compiles (probe) and blocked (restore->build) stay distinct.
    """
    routed = Routed(Language.CSHARP, Scope.CHANGED, files=("src/App/a.cs",), projects=("src/App/App.csproj",))
    ran: list[str] = []

    def recording_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        ran.extend(check.tool.name for check in checks)
        return _ok_static_fan(checks)

    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    executor = SeamExecutor(run_fn=_compiling_probe if compiles else _failing_probe, fan_fn=recording_fan)
    rows = static_rail._dispatch(
        routed, phases=_csharp_closure_phases(), settings=assay_root.settings, scope=assay_root.scope(Claim.STATIC), executor=executor
    )
    assert ran.count("dotnet-format") == (2 if compiles else 0)
    assert {"dotnet-restore", "dotnet-build"} <= set(ran)
    assert all(assert_ok(row).status is RailStatus.OK for row in rows)


# --- [SARIF_STATUS_LAWS]


def _build_receipt(project: str, status: RailStatus) -> Completed:
    return receipt(("dotnet", "build", "--no-restore", project), 0 if status is not RailStatus.FAILED else 1, status=status)


def test_sarif_status_distinguishes_incremental_from_clean(tmp_path: Path) -> None:
    """Per-build SARIF status separates produced:N from absent:incremental, absent:no-build, and absent:build-failed.

    A warm-incremental OK build with no SARIF reads as ``absent:incremental``, never as a clean produced pass — the
    exact distinction the analyzer's incremental design would otherwise erase.
    """
    sarif_dir = tmp_path / "sarif"
    sarif_dir.mkdir()
    (sarif_dir / "App.sarif").write_bytes(
        msgspec.json.encode({"version": "2.1.0", "runs": [{"results": [{"ruleId": "CSP0101", "message": {"text": "x"}}] * 2}]})
    )
    outcomes = (
        _build_receipt("src/App/App.csproj", RailStatus.OK),
        _build_receipt("src/Lib/Lib.csproj", RailStatus.EMPTY),
        _build_receipt("src/Skip/Skip.csproj", RailStatus.SKIP),
        _build_receipt("src/Bad/Bad.csproj", RailStatus.FAILED),
    )
    assert sarif_status(outcomes, str(sarif_dir)) == (
        ("App", "produced:2"),
        ("Lib", "absent:incremental"),
        ("Skip", "absent:no-build"),
        ("Bad", "absent:build-failed"),
    )
    # A non-build receipt contributes no row; a .slnx workspace build keys against the whole directory.
    assert sarif_status((receipt(("ruff", "check"), 0, status=RailStatus.OK),), str(sarif_dir)) == ()
    assert sarif_status((_build_receipt("Workspace.slnx", RailStatus.OK),), str(sarif_dir)) == (("Workspace", "produced:2"),)


# --- [BACKPRESSURE_LAWS]


def test_backpressure_note_threads_structured_pressure() -> None:
    """A halving or any dotnet.slot wait appends one structured backpressure note carrying the real fan numbers.

    Every field rides the typed resource rows resource_projection folded; nothing is re-parsed out of note text.
    """
    resources = (
        ("concurrency.original", 8.0),
        ("concurrency.reduced", 4.0),
        ("dotnet.foreign", 12.0),
        ("dotnet.slot_wait_ms.max", 1500.0),
        ("memory.percent", 91.0),
    )
    assert static_rail._backpressure_note(resources) == (
        "concurrency.backpressure: reduced 8->4 (mem=91% foreign_dotnet=12); dotnet.slot max_wait=1500ms",
    )
    quiet = (("concurrency.original", 8.0), ("concurrency.reduced", 8.0), ("dotnet.foreign", 0.0), ("memory.percent", 40.0))
    assert static_rail._backpressure_note(quiet) == ()
    waited = (("concurrency.original", 8.0), ("concurrency.reduced", 8.0), ("dotnet.slot_wait_ms.max", 1500.0), ("memory.percent", 40.0))
    assert static_rail._backpressure_note(waited)[0].endswith("dotnet.slot max_wait=1500ms")
