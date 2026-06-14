"""Law matrix for the unified static rail route."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Error, Ok
import pytest

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error_status, assert_ok, support_matrix
from tools.assay.composition.registry import REGISTRY
from tools.assay.core.model import Check, Claim, Fault, Input, Language, Mode, receipt, Runner, StaticRun, Tool
from tools.assay.core.routing import Routed, Scope, TargetFiles
from tools.assay.core.status import RailStatus
import tools.assay.rails.static as static_rail
from tools.assay.rails.static import run, StaticParams


if TYPE_CHECKING:
    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness, VerbRunner
    from tools.assay.core.model import Completed


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok_static_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
    return tuple(Ok(receipt((check.tool.name,), 0, status=RailStatus.OK)) for check in checks)


def _recording_fan(calls: list[tuple[Mode, ...]]) -> object:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append(tuple(check.tool.mode for check in checks))
        return _ok_static_fan(checks)

    return fan


# --- [STATICPARAMS_LAWS]


def test_staticparams_defaults() -> None:
    """Default static params carry no implicit project, solution, path, or language selector."""
    params = StaticParams()
    assert params.all is False
    assert not params.project
    assert params.folders == ()
    assert params.files == ()
    assert not hasattr(params, "paths")
    assert not hasattr(params, "language")


register_law(StaticParams, "defaults_are_folder_file_only")


def test_registry_exposes_one_polymorphic_static_verb() -> None:
    """The static claim collapses to exactly one verb; no check/build/fix split remains."""
    assert tuple(row.verb for row in REGISTRY if row.claim is Claim.STATIC) == ("static",)


register_law(REGISTRY, "static_collapses_to_one_verb")


def test_cli_consumes_grouped_folder_and_file_targets(cli: VerbRunner, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Grouped --folder and --file flags consume multiple targets."""
    assay_root.write("src/a.py", "")
    assay_root.write("pkg/b.py", "")
    assay_root.write("single/c.py", "")
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    result = cli("static", "--folder", "src", "pkg", "--file", "single/c.py")
    report = result.envelope.report
    assert report is not None
    assert isinstance(report.detail, StaticRun)
    assert report.detail.targets == (("folder", "src"), ("folder", "pkg"), ("file", "single/c.py"))
    assert any(row[0] == "python" and row[2] == "3" for row in report.detail.routes)


register_law(StaticParams, "grouped_folder_file_cli_consumption")


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


register_law(run, "help_admits_scoped_target_flags")


# --- [LANE_LAWS]


def test_python_lane_runs_fix_before_diagnostics(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Python static runs fix rows before diagnostics."""
    assay_root.write("tools/assay/probe.py", "")
    calls: list[tuple[Mode, ...]] = []
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", _recording_fan(calls))
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("tools/assay",))))
    assert isinstance(report.detail, StaticRun)
    assert report.detail.phases == ("fix", "diagnostic")
    write_index = next(i for i, modes in enumerate(calls) if Mode.WRITE in modes)
    check_index = next(i for i, modes in enumerate(calls) if Mode.CHECK in modes)
    assert write_index < check_index
    assert report.notes[0] == f"planned={len(report.detail.planned)} skipped={len(report.detail.skipped)}"


register_law(run, "python_lane_fixes_before_diagnostics")


def test_csharp_project_lane_runs_full_ordered_lane(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """A C# project target runs the full ordered lane: fix, diagnostics, restore, then build."""
    assay_root.write("src/App/App.csproj", "<Project />")
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(project="src/App/App.csproj")))
    assert isinstance(report.detail, StaticRun)
    assert report.detail.phases == ("fix", "diagnostic", "restore", "build")
    assert any(row[0] == "csharp" and row[3] == "1" for row in report.detail.routes)
    assert any("dotnet build" in argv and "src/App/App.csproj" in argv for _, _, argv in report.detail.planned)


register_law(run, "csharp_project_runs_full_ordered_lane")


def test_folder_lane_spans_python_typescript_and_csharp(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """A mixed folder runs the admitted fix + diagnostic + build rows for every language touched."""
    assay_root.write("src/App/App.csproj", "<Project />")
    assay_root.write("src/App/a.cs", "class A {}")
    assay_root.write("src/pkg/a.py", "")
    assay_root.write("src/web/a.ts", "")
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("src",))))
    assert isinstance(report.detail, StaticRun)
    planned_names = {name for _, name, _ in report.detail.planned}
    assert {"ruff", "ruff-format", "ty", "mypy", "ast-grep-py", "py-analyzer"} <= planned_names
    assert {"biome", "ast-grep-ts"} <= planned_names
    assert {"dotnet-format", "dotnet-restore", "dotnet-build"} <= planned_names
    assert report.counts.total == len(report.detail.planned)


register_law(run, "folder_lane_spans_all_languages")


def test_empty_target_is_changed_default_not_a_fault(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Empty target input routes through the changed-default lane."""
    monkeypatch.setattr(static_rail, "target_files", lambda *_a, **_k: Ok(TargetFiles()))
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams()))
    assert isinstance(report.detail, StaticRun)


register_law(run, "empty_target_routes_changed_default")


def test_unsupported_file_target_is_skipped_not_faulted(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """An unsupported --file surfaces as a skipped target row, not the removed hard execution fault."""
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(files=("Workspace.slnx",))))
    assert isinstance(report.detail, StaticRun)
    assert any(kind == "file" and "Workspace.slnx" in path for kind, path, _ in report.detail.skipped)


register_law(run, "unsupported_file_is_skipped_not_faulted")


def test_only_one_target_axis_admitted(assay_root: AssayHarness) -> None:
    """The value-driven parse rejects combining --all with --project or folder/file targets."""
    fault = assert_error_status(
        run(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(all=True, project="src/App/App.csproj")), RailStatus.UNSUPPORTED
    )
    assert "choose only one" in fault.message


register_law(run, "rejects_combined_target_axes")


# --- [SELECTION_LAWS]


def test_full_typescript_keeps_build_row(assay_root: AssayHarness) -> None:
    """Full TypeScript static routes keep the project-wide tsc build row while scoped TS files skip it."""
    full = Routed(Language.TYPESCRIPT, Scope.FULL, files=(".",))
    scoped = Routed(Language.TYPESCRIPT, Scope.CHANGED, files=("src/web/a.ts",))
    full_phases, full_skipped = static_rail._phase_checks(full, assay_root.settings, assay_root.scope(Claim.STATIC))
    scoped_phases, scoped_skipped = static_rail._phase_checks(scoped, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert any(check.tool.name == "tsc" for _, checks in full_phases for check in checks)
    assert not any(row[1] == "tsc" for row in full_skipped)
    assert not any(check.tool.name == "tsc" for _, checks in scoped_phases for check in checks)
    assert ("build", "tsc", "project-wide tool unsupported by scoped static") in scoped_skipped


register_law(static_rail._phase_checks, "full_typescript_keeps_build_row")


def test_full_typescript_tsc_has_no_file_tail(assay_root: AssayHarness) -> None:
    """Full TypeScript build invokes the project owner once; it never mixes ``-p`` with file tails."""
    routed = Routed(Language.TYPESCRIPT, Scope.FULL, files=("vite.config.ts", "vitest.config.ts"))
    phases, _ = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned(routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert ("build", "tsc", "pnpm exec tsc --noEmit -p tsconfig.base.json") in planned


register_law(static_rail._phase_checks, "full_typescript_tsc_has_no_file_tail")


def test_scoped_glob_build_skips_but_native_fixer_stays(assay_root: AssayHarness) -> None:
    """A scoped TypeScript route skips repo-wide tsc while keeping the scoped Biome write fixer."""
    routed = Routed(Language.TYPESCRIPT, Scope.CHANGED, files=("src/view.ts",))
    phases, skips = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert ("build", "tsc", "project-wide tool unsupported by scoped static") in skips
    assert any(check.tool.name == "biome" and check.tool.mode is Mode.WRITE for _, checks in phases for check in checks)
    assert all(check.tool.name != "tsc" for _, checks in phases for check in checks)


register_law(static_rail._phase_checks, "scoped_glob_build_skips_native_fixer_stays")


def test_csharp_format_uses_include_placement(assay_root: AssayHarness) -> None:
    """C# format rows bind to the owner project plus ``--include`` file tails, not Workspace.slnx."""
    routed = Routed(
        Language.CSHARP, Scope.CHANGED, files=("src/App/a.cs",), projects=("src/App/App.csproj",), groups=(("src/App/App.csproj", ("src/App/a.cs",)),)
    )
    phases, skipped = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned(routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert any("dotnet format" in argv and "src/App/App.csproj --include src/App/a.cs" in argv for _, _, argv in planned)


register_law(static_rail._phase_checks, "csharp_format_uses_project_include")


def test_csharp_project_format_uses_project_placement(assay_root: AssayHarness) -> None:
    """Direct project routes format the project, not an empty ``--include`` tail."""
    routed = Routed(Language.CSHARP, Scope.CHANGED, projects=("src/App/App.csproj",))
    phases, skipped = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned(routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert any(argv.endswith("src/App/App.csproj") for _, name, argv in planned if name == "dotnet-format")


register_law(static_rail._phase_checks, "csharp_project_format_uses_project_placement")


def test_csharp_workspace_format_uses_solution_placement(assay_root: AssayHarness) -> None:
    """Explicit all-workspace routes place dotnet format/restore/build on Workspace.slnx."""
    routed = Routed(Language.CSHARP, Scope.FULL, full_triggers=(str(assay_root.settings.solution),))
    phases, skipped = static_rail._phase_checks(routed, assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned(routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert all(str(assay_root.settings.solution) in argv for _, name, argv in planned if name.startswith("dotnet-"))
    assert any(f"-maxCpuCount:{assay_root.settings.dotnet_max_cpu}" in argv for _, name, argv in planned if name == "dotnet-build")
    assert all("--disable-build-servers" not in argv for _, _, argv in planned)


register_law(static_rail._phase_checks, "csharp_workspace_uses_solution_placement")


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
    monkeypatch.setattr(static_rail, "fan_out", fake_fan)
    result = static_rail._build_fan((("restore", (restore,)), ("build", (compile_check,))), routed, assay_root.settings)
    assert calls == [(Mode.RESTORE,)]
    skipped = assert_ok(result[1])
    assert skipped.status is RailStatus.SKIP
    assert skipped.notes == ("restore failed; build skipped",)


register_law(static_rail._build_fan, "restore_failure_skips_build_phase")


def test_static_status_matrix() -> None:
    """The mode vocabulary preserves the write/read distinctions the lane dispatch relies on."""
    support_matrix(
        ("check is read-only", lambda: not Mode.CHECK.writes, True),
        ("build is read-only", lambda: not Mode.BUILD.writes, True),
        ("restore is read-only", lambda: not Mode.RESTORE.writes, True),
        ("write mutates", lambda: Mode.WRITE.writes, True),
    )
