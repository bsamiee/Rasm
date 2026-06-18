"""Law matrix for the unified static rail route."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Error, Ok
import msgspec

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error_status, assert_ok, support_matrix
from tools.assay.composition.registry import REGISTRY
from tools.assay.core.model import _sarif_status, Check, Claim, Fault, Input, Language, Mode, receipt, Runner, StaticRun, Tool
from tools.assay.core.routing import Routed, Scope, TargetFiles
from tools.assay.core.status import RailStatus
import tools.assay.rails.static as static_rail
from tools.assay.rails.static import run, StaticParams


if TYPE_CHECKING:
    from pathlib import Path

    from expression import Result
    import pytest

    from tests.python.tools.assay.kit import AssayHarness, VerbRunner
    from tools.assay.core.model import Completed


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok_static_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
    return tuple(Ok(receipt((check.tool.name,), 0, status=RailStatus.OK)) for check in checks)


def _compiling_probe(check: Check, **_kw: object) -> Result[Completed, Fault]:
    return Ok(receipt((check.tool.name,), 0, status=RailStatus.OK))


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
    monkeypatch.setattr(static_rail, "run_check", _compiling_probe)
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
    monkeypatch.setattr(static_rail, "run_check", _compiling_probe)
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
    result = static_rail._build_fan(
        ((static_rail.Phase.RESTORE, (restore,)), (static_rail.Phase.BUILD, (compile_check,))), routed, assay_root.settings
    )
    assert calls == [(Mode.RESTORE,)]
    skipped = assert_ok(result[1])
    assert skipped.status is RailStatus.SKIP
    assert skipped.notes == ("restore failed; build skipped",)


register_law(static_rail._build_fan, "restore_failure_skips_build_phase")


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


def test_format_gate_drops_write_and_check_format_rows_when_target_does_not_compile(
    monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness
) -> None:
    """A non-compiling probe drops both dotnet-format rows (write fix and its read-only check) but runs restore and build.

    dotnet-format binds against the analyzer view and mutates a target the compiler itself rejects, so the format phase is
    gated whole; the closure restore and build still run, proving compiles (probe) and blocked (restore->build) stay distinct.
    """
    routed = Routed(Language.CSHARP, Scope.CHANGED, files=("src/App/a.cs",), projects=("src/App/App.csproj",))
    ran: list[tuple[str, ...]] = []

    def recording_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        ran.append(tuple(check.tool.name for check in checks))
        return _ok_static_fan(checks)

    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", recording_fan)
    monkeypatch.setattr(static_rail, "run_check", lambda check, **_kw: Ok(receipt((check.tool.name,), 1, status=RailStatus.FAILED)))
    rows = static_rail._dispatch(routed, phases=_csharp_closure_phases(), settings=assay_root.settings, scope=assay_root.scope(Claim.STATIC))
    names_run = tuple(name for batch in ran for name in batch)
    assert "dotnet-format" not in names_run, "the format phase is skipped entirely on a non-compiling target"
    assert {"dotnet-restore", "dotnet-build"} <= set(names_run), "restore and build still run despite the gated format phase"
    assert all(assert_ok(row).status is RailStatus.OK for row in rows)


register_law(static_rail._dispatch, "format_gate_skips_format_when_probe_fails")


def test_format_gate_runs_format_when_target_compiles(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """A compiling probe leaves the full lane intact: both format rows, restore, and build all run."""
    routed = Routed(Language.CSHARP, Scope.CHANGED, files=("src/App/a.cs",), projects=("src/App/App.csproj",))
    ran: list[str] = []

    def recording_fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        ran.extend(check.tool.name for check in checks)
        return _ok_static_fan(checks)

    monkeypatch.setattr(static_rail, "leased", lambda _resource, run, **_kw: run(object()))
    monkeypatch.setattr(static_rail, "fan_out", recording_fan)
    monkeypatch.setattr(static_rail, "run_check", _compiling_probe)
    static_rail._dispatch(routed, phases=_csharp_closure_phases(), settings=assay_root.settings, scope=assay_root.scope(Claim.STATIC))
    assert ran.count("dotnet-format") == 2, "both the write fix and the read-only format check run when the target compiles"
    assert {"dotnet-restore", "dotnet-build"} <= set(ran)


register_law(static_rail._dispatch, "format_gate_runs_format_when_probe_compiles")


def test_static_status_matrix() -> None:
    """The mode vocabulary preserves the write/read distinctions the lane dispatch relies on."""
    support_matrix(
        ("check is read-only", lambda: not Mode.CHECK.writes, True),
        ("build is read-only", lambda: not Mode.BUILD.writes, True),
        ("restore is read-only", lambda: not Mode.RESTORE.writes, True),
        ("write mutates", lambda: Mode.WRITE.writes, True),
    )


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
    assert _sarif_status(outcomes, str(sarif_dir)) == (
        ("App", "produced:2"),
        ("Lib", "absent:incremental"),
        ("Skip", "absent:no-build"),
        ("Bad", "absent:build-failed"),
    )
    # A non-build receipt contributes no row; a .slnx workspace build keys against the whole directory.
    assert _sarif_status((receipt(("ruff", "check"), 0, status=RailStatus.OK),), str(sarif_dir)) == ()
    assert _sarif_status((_build_receipt("Workspace.slnx", RailStatus.OK),), str(sarif_dir)) == (("Workspace", "produced:2"),)


register_law(_sarif_status, "distinguishes_incremental_from_clean")


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


register_law(static_rail._backpressure_note, "threads_structured_pressure")
