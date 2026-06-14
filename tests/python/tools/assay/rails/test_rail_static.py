"""Law matrix for ``tools.assay.rails.static`` scoped command surface."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Error, Ok
import pytest

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error_status, assert_ok, support_matrix
from tools.assay.composition.registry import REGISTRY
from tools.assay.core.model import Check, Claim, Fault, Input, Language, Mode, receipt, Runner, StaticRun, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
import tools.assay.rails.static as static_rail
from tools.assay.rails.static import build, check, fix, StaticBuildParams, StaticParams


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness, VerbRunner
    from tools.assay.composition.settings import ArtifactScope, AssaySettings
    from tools.assay.core.model import Completed, Report

    type _VerbFn = Callable[[AssaySettings, ArtifactScope, StaticParams], Result[Report, Fault]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok_static_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
    return tuple(Ok(receipt((check.tool.name,), 0, status=RailStatus.OK)) for check in checks)

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


def test_staticbuildparams_surface_is_build_only() -> None:
    """Build params expose only the explicit all/project target shape."""
    params = StaticBuildParams()
    assert params.all is False
    assert not params.project
    assert not hasattr(params, "folders")
    assert not hasattr(params, "files")


register_law(StaticBuildParams, "surface_is_build_only")


def test_registry_exposes_only_scoped_static_verbs() -> None:
    """Static registry surface is exactly check/build/fix, with no compatibility verbs."""
    assert tuple(row.verb for row in REGISTRY if row.claim is Claim.STATIC) == ("check", "build", "fix")


register_law(REGISTRY, "static_verbs_are_collapsed")


def test_cli_consumes_grouped_folder_and_file_targets(
    cli: VerbRunner, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch
) -> None:
    """One ``--folder`` consumes multiple folders until ``--file``; one ``--file`` consumes multiple files."""
    assay_root.write("src/a.py", "")
    assay_root.write("pkg/b.py", "")
    assay_root.write("single/c.py", "")
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    result = cli("static", "check", "--folder", "src", "pkg", "--file", "single/c.py")
    report = result.envelope.report
    assert report is not None
    assert isinstance(report.detail, StaticRun)
    assert report.detail.targets == (("folder", "src"), ("folder", "pkg"), ("file", "single/c.py"))
    assert any(row[0] == "python" and row[2] == "3" for row in report.detail.routes)


register_law(StaticParams, "grouped_folder_file_cli_consumption")


def test_static_build_help_excludes_scoped_file_flags(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Static build help advertises only --all/--project and suppresses Cyclopts' negative --all form."""
    from tools.assay import __main__ as main_mod  # noqa: PLC0415

    neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
    monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
    code = main_mod.main(["static", "build", "--help"])
    cap = capsysbinary.readouterr()
    assert code == 0
    assert b"--all" in cap.out
    assert b"--project" in cap.out
    assert b"--folder" not in cap.out
    assert b"--file" not in cap.out
    assert b"--no-all" not in cap.out


register_law(build, "help_excludes_scoped_file_flags")


# --- [CHECK_LAWS]


def test_check_executes_non_mutating_rows(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Check executes non-mutating catalog rows and records the same planned shape in StaticRun."""
    assay_root.write("tools/assay/probe.py", "")
    calls: list[tuple[Mode, ...]] = []

    def fake_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append(tuple(check.tool.mode for check in checks))
        return _ok_static_fan(checks)

    monkeypatch.setattr(static_rail, "fan_out", fake_fan)
    report = assert_ok(check(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("tools/assay",))))
    assert isinstance(report.detail, StaticRun)
    assert report.status is RailStatus.OK
    assert calls
    assert all(mode is not Mode.WRITE for phase in calls for mode in phase)
    assert report.detail.planned
    assert report.notes[0] == f"planned={len(report.detail.planned)} skipped={len(report.detail.skipped)}"


register_law(check, "executes_non_mutating_rows")


def test_check_project_uses_single_project_build(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Project check runs diagnostics plus direct owner restore/build, never reverse-dependent closure."""
    assay_root.write("src/App/App.csproj", "<Project />")
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(check(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(project="src/App/App.csproj")))
    assert isinstance(report.detail, StaticRun)
    assert any(row[0] == "csharp" and row[3] == "1" for row in report.detail.routes)
    assert any("dotnet build" in argv and "src/App/App.csproj" in argv for _, _, argv in report.detail.planned)
    assert report.detail.phases == ("diagnostic", "restore", "build")
    assert all(phase != "fix" for phase, _, _ in report.detail.planned)


register_law(check, "project_uses_single_project_build")


def test_check_folder_uses_non_mutating_diagnostics(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Folder/file check runs scoped non-mutating diagnostics and leaves mutating fixers to static fix."""
    assay_root.write("src/App/a.py", "")
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(check(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("src/App",))))
    assert isinstance(report.detail, StaticRun)
    assert report.detail.phases == ("diagnostic",)
    assert all(phase == "diagnostic" for phase, _, _ in report.detail.planned)


register_law(check, "folder_uses_non_mutating_diagnostics")


def test_check_folder_filters_project_files_but_routes_sources(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Folder expansion skips project/config files while C# source files still derive an owner closure."""
    assay_root.write("src/App/App.csproj", "<Project />")
    assay_root.write("src/App/a.cs", "class A {}")
    assay_root.write("src/App/packages.lock.json", "{}")
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(check(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("src/App",))))
    assert isinstance(report.detail, StaticRun)
    assert ("folder", "src/App/App.csproj", "project-or-solution") in report.detail.skipped
    assert any(route[0] == "csharp" and route[3] == "1" for route in report.detail.routes)
    assert all("packages.lock.json" not in argv for _, _, argv in report.detail.planned)
    assert report.notes[0] == f"planned={len(report.detail.planned)} skipped={len(report.detail.skipped)}"
    assert any(note.startswith("closure[csharp]:") for note in report.notes)


register_law(check, "folder_skips_projects_and_routes_sources")


def test_check_includes_python_typescript_and_csharp_catalog_rows(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Check plans and executes the admitted non-mutating rows for Python, TypeScript, and C# targets."""
    assay_root.write("src/App/App.csproj", "<Project />")
    assay_root.write("src/App/a.cs", "class A {}")
    assay_root.write("src/pkg/a.py", "")
    assay_root.write("src/web/a.ts", "")
    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", _ok_static_fan)
    report = assert_ok(check(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("src",))))
    assert isinstance(report.detail, StaticRun)
    planned_names = {name for _, name, _ in report.detail.planned}
    assert {"ruff", "ruff-format", "ty", "mypy", "ast-grep-py", "py-analyzer"} <= planned_names
    assert {"biome", "ast-grep-ts"} <= planned_names
    assert {"dotnet-format", "dotnet-restore", "dotnet-build"} <= planned_names
    assert report.counts.total == len(report.detail.planned)


register_law(check, "includes_python_typescript_and_csharp_catalog_rows")


def test_full_static_route_keeps_typescript_build_row(assay_root: AssayHarness) -> None:
    """Full TypeScript static routes keep the project-wide tsc build row while scoped TS files skip it."""
    full = Routed(Language.TYPESCRIPT, Scope.FULL, files=(".",))
    scoped = Routed(Language.TYPESCRIPT, Scope.CHANGED, files=("src/web/a.ts",))
    full_phases, full_skipped = static_rail._phase_checks(full, (Mode.CHECK, Mode.BUILD), assay_root.settings, assay_root.scope(Claim.STATIC))
    scoped_phases, scoped_skipped = static_rail._phase_checks(scoped, (Mode.CHECK, Mode.BUILD), assay_root.settings, assay_root.scope(Claim.STATIC))
    assert any(check.tool.name == "tsc" for _, checks in full_phases for check in checks)
    assert not any(row[1] == "tsc" for row in full_skipped)
    assert not any(check.tool.name == "tsc" for _, checks in scoped_phases for check in checks)
    assert ("build", "tsc", "project-wide tool unsupported by scoped static") in scoped_skipped


register_law(static_rail._phase_checks, "full_typescript_keeps_build_row")


def test_fix_executes_mutating_rows_only(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Fix executes write-mode formatter/fixer rows without smuggling build/check work into the write lane."""
    assay_root.write("src/pkg/a.py", "")
    calls: list[tuple[Mode, ...]] = []

    def fake_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        calls.append(tuple(check.tool.mode for check in checks))
        return _ok_static_fan(checks)

    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", fake_fan)
    report = assert_ok(fix(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(files=("src/pkg/a.py",))))
    assert isinstance(report.detail, StaticRun)
    assert report.detail.phases == ("fix",)
    assert calls
    assert all(mode is Mode.WRITE for phase in calls for mode in phase)


register_law(fix, "executes_mutating_rows_only")


# --- [EXECUTION_GUARDS]


@pytest.mark.parametrize("verb, fn, status", [("build", build, RailStatus.UNSUPPORTED), ("fix", fix, RailStatus.FAULTED)], ids=["build", "fix"])
def test_execution_verbs_require_explicit_targets(verb: str, fn: _VerbFn, status: RailStatus, assay_root: AssayHarness) -> None:
    """build/fix reject empty input instead of falling back to changed files or Workspace.slnx."""
    fault = assert_error_status(fn(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams()), status)
    assert f"{verb}: requires" in fault.message


register_law(build, "requires_explicit_project_or_all_target")
register_law(fix, "requires_explicit_targets")


def test_build_rejects_folder_file_targets(assay_root: AssayHarness) -> None:
    """Build rejects folder/file expansion; C# compile is one project or explicit workspace only."""
    assay_root.write("src/App/App.csproj", "<Project />")
    assay_root.write("src/App/a.cs", "class A {}")
    fault = assert_error_status(
        build(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(folders=("src/App",))), RailStatus.UNSUPPORTED
    )
    assert "build: requires --project or --all" in fault.message
    assert fault.argv == ("static", "build", "--folder", "src/App")


register_law(build, "rejects_folder_file_targets")


@pytest.mark.parametrize("path", ["src/App/App.csproj", "Workspace.slnx", "Directory.Build.props"], ids=["csproj", "slnx", "props"])
def test_execution_verbs_reject_explicit_project_solution_and_trigger_files(path: str, assay_root: AssayHarness) -> None:
    """Explicit file targets that imply project/solution/full-trigger work fault before fix execution."""
    fault = assert_error_status(fix(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(files=(path,))), RailStatus.UNSUPPORTED)
    assert path in fault.message


register_law(fix, "rejects_project_solution_and_trigger_file_inputs")


# --- [SELECTION_LAWS]


def test_phase_checks_skip_project_wide_glob_tools_and_keep_native_fixers(assay_root: AssayHarness) -> None:
    """TypeScript build skips repo-wide tsc while fix keeps scoped Biome write rows."""
    routed = Routed(Language.TYPESCRIPT, Scope.CHANGED, files=("src/view.ts",))
    build_phases, build_skips = static_rail._phase_checks(routed, (Mode.CHECK, Mode.BUILD), assay_root.settings, assay_root.scope(Claim.STATIC))
    fix_phases, fix_skips = static_rail._phase_checks(routed, (Mode.WRITE,), assay_root.settings, assay_root.scope(Claim.STATIC))
    assert ("build", "tsc", "project-wide tool unsupported by scoped static") in build_skips
    assert not fix_skips
    assert any(check.tool.name == "biome" and check.tool.mode is Mode.WRITE for _, checks in fix_phases for check in checks)
    assert all(check.tool.name != "tsc" for _, checks in build_phases for check in checks)


register_law(static_rail._phase_checks, "glob_project_tools_skip_and_native_fixers_stay")


def test_csharp_format_uses_include_placement(assay_root: AssayHarness) -> None:
    """C# format rows bind to the owner project plus ``--include`` file tails, not Workspace.slnx."""
    routed = Routed(
        Language.CSHARP, Scope.CHANGED, files=("src/App/a.cs",), projects=("src/App/App.csproj",), groups=(("src/App/App.csproj", ("src/App/a.cs",)),)
    )
    phases, skipped = static_rail._phase_checks(routed, (Mode.CHECK,), assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned("build", routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert any("dotnet format" in argv and "src/App/App.csproj --include src/App/a.cs" in argv for _, _, argv in planned)


register_law(static_rail._phase_checks, "csharp_format_uses_project_include")


def test_csharp_project_format_uses_project_placement(assay_root: AssayHarness) -> None:
    """Direct project routes format the project, not an empty ``--include`` tail."""
    routed = Routed(Language.CSHARP, Scope.CHANGED, projects=("src/App/App.csproj",))
    phases, skipped = static_rail._phase_checks(routed, (Mode.CHECK,), assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned("build", routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert any(argv.endswith("src/App/App.csproj") for _, name, argv in planned if name == "dotnet-format")


register_law(static_rail._phase_checks, "csharp_project_format_uses_project_placement")


def test_csharp_workspace_format_uses_solution_placement(assay_root: AssayHarness) -> None:
    """Explicit all-workspace routes place dotnet format/restore/build on Workspace.slnx."""
    routed = Routed(Language.CSHARP, Scope.FULL, full_triggers=(str(assay_root.settings.solution),))
    phases, skipped = static_rail._phase_checks(routed, (Mode.CHECK, Mode.RESTORE, Mode.BUILD), assay_root.settings, assay_root.scope(Claim.STATIC))
    planned = static_rail._planned("build", routed, phases, assay_root.settings, assay_root.scope(Claim.STATIC))
    assert skipped == ()
    assert all(str(assay_root.settings.solution) in argv for _, name, argv in planned if name.startswith("dotnet-"))
    assert any(f"-maxCpuCount:{assay_root.settings.dotnet_max_cpu}" in argv for _, name, argv in planned if name == "dotnet-build")
    assert all("--disable-build-servers" not in argv for _, _, argv in planned)


register_law(static_rail._phase_checks, "csharp_workspace_uses_solution_placement")


# --- [BUILD_PHASE_LAWS]


def test_build_fan_restores_before_build_and_skips_after_restore_failure(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Build phase is ordered: restore failure blocks the build phase instead of racing ``--no-restore``."""
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
    """Public mode vocabulary preserves write/read distinctions used by static dispatch."""
    support_matrix(
        ("check is read-only", lambda: not Mode.CHECK.writes, True),
        ("build is read-only", lambda: not Mode.BUILD.writes, True),
        ("write mutates", lambda: Mode.WRITE.writes, True),
    )
