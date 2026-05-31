from __future__ import annotations

import os
from pathlib import Path
import shutil
from typing import TYPE_CHECKING

from expression import Error, Ok
import msgspec
import pytest

from tools.quality import __main__ as quality_main, process
from tools.quality.__main__ import main
from tools.quality.process import Completed
from tools.quality.rails import api, bridge, package, static, test
from tools.quality.settings import ArtifactScope, QualitySettings


if TYPE_CHECKING:
    from expression import Result

    from tools.quality.process import ProcessFault


def _scope(tmp_path: Path, rail: str = "static") -> ArtifactScope:
    scope_path = tmp_path / ".artifacts" / "quality" / rail
    scope_path.mkdir(parents=True, exist_ok=True)
    return ArtifactScope(root=tmp_path, rail=rail, scope_path=scope_path, dotnet_env={"DOTNET_CLI_HOME": str(scope_path / "dotnet-cli")})


def test_discover_root_finds_workspace(tmp_path: Path) -> None:
    root = tmp_path / "workspace"
    nested = root / "a/b/c"
    nested.mkdir(parents=True)
    (root / "Workspace.slnx").write_text("")

    assert QualitySettings.anchor(nested) == root


def test_settings_defaults() -> None:
    settings = QualitySettings()
    assert settings.mutation_eligible is True
    assert settings.solution.name == "Workspace.slnx"


def test_artifact_scope_is_run_scoped(tmp_path: Path) -> None:
    settings = QualitySettings(root=tmp_path, run_id="run-1")

    with ArtifactScope.open(settings, "test") as scope:
        assert scope.path == tmp_path / ".artifacts" / "quality" / "test" / "run-1"
        assert scope.dotnet_env["DOTNET_CLI_HOME"] == str(scope.path / "dotnet-cli")


@pytest.mark.parametrize(
    "env_key, expected",
    [
        ("QUALITY_TEST_TARGET", Path("tests/tools/cs-analyzer/CsAnalyzer.Tests.csproj")),
        ("TEST_TARGET", Path("tests/csharp/libs/Rasm/Rasm.Tests.csproj")),
    ],
)
def test_settings_test_target_env_contract(monkeypatch: pytest.MonkeyPatch, env_key: str, expected: Path) -> None:
    monkeypatch.setenv(env_key, "tests/tools/cs-analyzer/CsAnalyzer.Tests.csproj")

    assert QualitySettings().test_target == expected


def test_self_test_exit_zero(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    solution = tmp_path / "Workspace.slnx"
    target = tmp_path / "tests/csharp/libs/Rasm/Rasm.Tests.csproj"
    manifest = tmp_path / ".config/dotnet-tools.json"
    yak = tmp_path / "RhinoWIP.app/Contents/Resources/bin/yak"

    def touch(path: Path) -> Path:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text("", encoding="utf-8")
        return path

    tuple(touch(path) for path in (solution, target, manifest, yak))

    settings = QualitySettings(root=tmp_path, test_target=target.relative_to(tmp_path))
    settings = settings.model_copy(update={"rhino_app": tmp_path / "RhinoWIP.app"})

    monkeypatch.setattr(quality_main, "QualitySettings", lambda: settings)
    monkeypatch.setattr(shutil, "which", lambda _cmd: "/bin/tool")
    monkeypatch.setattr(os, "access", lambda _path, _mode: True)

    assert main(["self-test"]) == 0


def test_self_test_reports_missing_runtime_tools(monkeypatch: pytest.MonkeyPatch, tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    settings = QualitySettings(root=tmp_path, test_target=Path("missing.csproj"))
    settings = settings.model_copy(update={"rhino_app": tmp_path / "RhinoWIP.app"})

    monkeypatch.setattr(quality_main, "QualitySettings", lambda: settings)
    monkeypatch.setattr(shutil, "which", lambda cmd: None if cmd in {"rg", "ilspycmd"} else "/bin/tool")

    assert main(["self-test"]) == 2
    assert "ilspycmd" in capsys.readouterr().err


def test_dotnet_scope_flags_precede_passthrough_args(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_run(
        argv: tuple[str, ...],
        *,
        cwd: Path | None = None,
        env: dict[str, str] | None = None,
        check: bool = False,
        timeout: float | None = None,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (cwd, env, check, timeout, mode)
        seen.append(argv)
        return Ok(Completed(argv=argv, returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(process, "run", fake_run)
    scope = _scope(tmp_path, rail="bridge")

    assert process.dotnet("run", "--project", "client.csproj", "--", "doctor", scope=scope).is_ok()
    assert seen == [
        (
            "dotnet",
            "run",
            "--project",
            "client.csproj",
            "--artifacts-path",
            str(tmp_path / ".artifacts" / "quality" / "bridge"),
            "--disable-build-servers",
            "--",
            "doctor",
        )
    ]


def test_api_search_returns_ripgrep_stdout(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    xml = tmp_path / "Contents/Frameworks/RhCore.framework/Versions/Current/Resources/RhinoCommon.xml"
    xml.parent.mkdir(parents=True)
    xml.write_text("<member name='Mesh' />")

    def fake_run(
        argv: tuple[str, ...],
        *,
        cwd: Path | None = None,
        env: dict[str, str] | None = None,
        check: bool = False,
        timeout: float | None = None,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (argv, cwd, env, check, timeout, mode)
        return Ok(Completed(argv=argv, returncode=0, stdout=b"Mesh hit\n", stderr=b""))

    monkeypatch.setattr(api, "run", fake_run)

    assert api.api(tmp_path, "search", "rhino-common", pattern="Mesh").default_value("") == "Mesh hit\n"


def test_api_decompile_returns_ilspy_stdout(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    def fake_run(
        argv: tuple[str, ...],
        *,
        cwd: Path | None = None,
        env: dict[str, str] | None = None,
        check: bool = False,
        timeout: float | None = None,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (cwd, env, check, timeout, mode)
        return Ok(Completed(argv=argv, returncode=0, stdout=b"class Mesh {}\n", stderr=b""))

    monkeypatch.setattr(api, "run", fake_run)

    assert api.api(tmp_path, "decompile", "rhino-common", type_name="Rhino.Geometry.Mesh").default_value("") == ("class Mesh {}\n")


def _invoke_verify(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, status: bridge.BridgeStatus = "ok", *, client_fault: process.ProcessFault | None = None
) -> tuple[bridge.BridgeResult, tuple[tuple[str, ...], ...], tuple[dict[str, object], ...]]:
    calls: list[tuple[str, ...]] = []
    kwargs_seen: list[dict[str, object]] = []
    settings = QualitySettings(root=tmp_path)
    scope = _scope(tmp_path)
    report_dir = settings.bridge_verify_dir
    report_dir.mkdir(parents=True, exist_ok=True)
    scenario = tmp_path / "case.verify.csx"
    scenario.write_text("// scenario")
    result_path = report_dir / "case.json"

    def fake_client_run(
        settings: QualitySettings, scope: ArtifactScope, *args: str, check: bool = True, timeout: float | None = None
    ) -> Result[Completed, ProcessFault]:
        _ = (settings, scope)
        calls.append(args)
        kwargs_seen.append({"check": check, "timeout": timeout})
        match client_fault:
            case process.ProcessFault() as fault:
                return Error(fault)
            case None:
                result_path.write_bytes(msgspec.json.encode(bridge.BridgeResult(status=status)))
                return Ok(Completed(argv=("bridge", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(bridge, "client_run", fake_client_run)

    return (bridge._verify_invoke(settings, scope, report_dir, tmp_path / "Rasm.csproj", scenario), tuple(calls), tuple(kwargs_seen))


@pytest.mark.parametrize("status", ["ok", "failed", "unsupported", "skipped", "busy", "timeout"])
def test_verify_invoke_passes_status_through_single_call(monkeypatch: pytest.MonkeyPatch, tmp_path: Path, status: bridge.BridgeStatus) -> None:
    result, calls, kwargs_seen = _invoke_verify(monkeypatch, tmp_path, status)

    assert result.status == status
    assert tuple(call[0] for call in calls) == ("check",)
    assert kwargs_seen == ({"check": False, "timeout": 180.0},)


def test_verify_invoke_maps_client_fault_to_failed(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    result, calls, _ = _invoke_verify(monkeypatch, tmp_path, client_fault=process.ProcessFault.fail("bridge", detail=b"boom", returncode=9))

    assert result.status == "failed"
    assert result.fault is not None
    assert "boom" in result.fault.message
    assert len(calls) == 1


def test_verify_discover_returns_directory_matches(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    scenarios = tmp_path / "scenarios"
    scenarios.mkdir()
    scenario = scenarios / "case.verify.csx"
    scenario.write_text("// scenario")

    def fake_paths(self: process.Workspace, args: tuple[str, ...], *, cwd: Path | None = None, suffix: str = "") -> tuple[Path, ...]:
        _ = (self, args, cwd, suffix)
        return (scenario,)

    monkeypatch.setattr(process.Workspace, "paths", fake_paths)

    assert bridge._verify_discover(process.Workspace(tmp_path), tmp_path, str(scenarios)) == (scenario,)


def test_verify_expire_removes_stale_report_dirs(tmp_path: Path) -> None:
    root = tmp_path / ".artifacts/rhino/verify"
    stale = root / "stale"
    fresh = root / "fresh"
    stale.mkdir(parents=True)
    fresh.mkdir()
    os.utime(stale, (1.0, 1.0))

    result = bridge._verify_expire(root, 300.0)

    assert result.is_ok()
    assert not stale.exists()
    assert fresh.exists()


def test_decode_bridge_result_accepts_camel_case_wire() -> None:
    payload = (
        b'{"status":"failed","reportPath":"report.json","fault":{"message":"boom","stackTrace":"trace"},'
        b'"phases":[{"phase":"check","status":"ok","durationMs":1.5}]}'
    )

    result = bridge.try_decode_bridge(payload).default_value(bridge.BridgeResult())

    assert result.report_path == "report.json"
    assert result.fault is not None
    assert result.fault.stack_trace == "trace"
    assert result.phases == (bridge.BridgePhase(phase="check", status="ok", duration_ms=1.5),)


def test_bridge_client_run_uses_canonical_no_build_output(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    client = tmp_path / "tools/rhino-bridge/client/bin/Release/net10.0/Rasm.RhinoBridge.Client.dll"
    client.parent.mkdir(parents=True)
    client.write_text(data="", encoding="utf-8")
    settings = QualitySettings(root=tmp_path)
    scope = _scope(tmp_path, rail="bridge")
    seen: list[tuple[tuple[str, ...], bool]] = []

    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, mode)
        seen.append((args, scoped))
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"{}", stderr=b""))

    monkeypatch.setattr(bridge, "dotnet", fake_dotnet)

    assert bridge.client_run(settings, scope, "doctor", check=False, timeout=12.0).is_ok()
    assert seen == [
        (("run", "--no-build", "--project", str(settings.bridge_client), "--configuration", settings.configuration, "--", "doctor"), False)
    ]


def test_verify_summary_lifts_first_failure_to_report_root(tmp_path: Path) -> None:
    report_dir = tmp_path / "verify"
    report_dir.mkdir()
    result = bridge._verify_summary(
        report_dir,
        (
            bridge.BridgeResult(status="ok", report_path="first.verify.json"),
            bridge.BridgeResult(status="failed", report_path="second.verify.json", fault=bridge.BridgeFault(message="boom")),
            bridge.BridgeResult(status="failed", report_path="third.verify.json"),
        ),
        300.0,
    ).default_value(bridge.VerifyReport(bridge.VerifySummaryCounts(ok=0, failed=0, total=0)))

    assert result.failed == 2
    assert result.first_failure is not None
    assert result.first_failure.name == "second.verify"
    assert result.first_failure.fault is not None
    assert result.first_failure.fault.message == "boom"
    assert msgspec.json.decode((report_dir / "summary.json").read_bytes())["first_failure"]["name"] == "second.verify"


def test_static_changed_routes_cs_relevant_file_to_owner(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    project = tmp_path / "libs/csharp/Rasm/Rasm.csproj"

    class FakeWorkspace:
        def __init__(self, root: Path) -> None:
            self.root = root

        @staticmethod
        def index() -> process.ProjectIndex:
            return {project.parent: project}

        @staticmethod
        def owner_rel(_index: process.ProjectIndex, _file: Path) -> str | None:
            return "libs/csharp/Rasm/Rasm.csproj"

        @staticmethod
        def changed() -> tuple[str, ...]:
            return ("libs/csharp/Rasm/Vectors/kernel.json",)

        @staticmethod
        def projects() -> tuple[str, ...]:
            return ("libs/csharp/Rasm/Rasm.csproj",)

        @staticmethod
        def csproj(_project: Path, _tag: str = "") -> object | str | None:
            return None

    monkeypatch.setattr(static, "Workspace", FakeWorkspace)

    scope = _scope(tmp_path)

    plan = static._plan(QualitySettings(root=tmp_path), scope).default_value(static.StaticPlan("changed", ()))

    assert plan.scope == "changed"
    assert plan.projects == ("libs/csharp/Rasm/Rasm.csproj",)


def test_static_changed_skips_without_changes(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    class FakeWorkspace:
        def __init__(self, root: Path) -> None:
            self.root = root

        @staticmethod
        def index() -> process.ProjectIndex:
            return {}

        @staticmethod
        def changed() -> tuple[str, ...]:
            return ()

    monkeypatch.setattr(static, "Workspace", FakeWorkspace)
    scope = _scope(tmp_path)

    plan = static._plan(QualitySettings(root=tmp_path), scope).default_value(static.StaticPlan("full", ("x",)))

    assert plan == static.StaticPlan(scope="changed", projects=())


def test_static_full_forces_solution_plan_without_changes(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    project = "libs/csharp/Rasm/Rasm.csproj"

    class FakeWorkspace:
        def __init__(self, root: Path) -> None:
            self.root = root

        @staticmethod
        def index() -> process.ProjectIndex:
            return {}

        @staticmethod
        def changed() -> tuple[str, ...]:
            return ()

        @staticmethod
        def projects() -> tuple[str, ...]:
            return (project,)

    def fake_run(
        argv: tuple[str, ...],
        *,
        cwd: Path | None = None,
        env: dict[str, str] | None = None,
        check: bool = False,
        timeout: float | None = None,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (argv, cwd, env, check, timeout, mode)
        return Ok(Completed(argv=("dotnet", "sln", "list"), returncode=0, stdout=f"{project}\n".encode(), stderr=b""))

    monkeypatch.setattr(static, "Workspace", FakeWorkspace)
    monkeypatch.setattr(static, "run", fake_run)
    scope = _scope(tmp_path)

    plan = static._plan(QualitySettings(root=tmp_path), scope, "full").default_value(static.StaticPlan("changed", ()))

    assert plan == static.StaticPlan(scope="full", projects=(project,))


def test_static_changed_ignores_python_analyzer_fixture(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    class FakeWorkspace:
        def __init__(self, root: Path) -> None:
            self.root = root

        @staticmethod
        def index() -> process.ProjectIndex:
            return {}

        @staticmethod
        def changed() -> tuple[str, ...]:
            return ("tests/tools/py_analyzer/fixtures/case.json",)

    monkeypatch.setattr(static, "Workspace", FakeWorkspace)

    scope = _scope(tmp_path)

    plan = static._plan(QualitySettings(root=tmp_path), scope).default_value(static.StaticPlan("full", ("x",)))

    assert plan == static.StaticPlan(scope="changed", projects=())


def test_static_check_applies_scoped_format_without_build(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, scoped, mode)
        seen.append(args)
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    def fail_build(*args: object, **kwargs: object) -> Result[None, ProcessFault]:
        _ = (args, kwargs)
        pytest.fail("static check must not run build")

    monkeypatch.setattr(
        static,
        "_plan",
        lambda _settings, _scope: Ok(
            static.StaticPlan(
                scope="changed",
                projects=("libs/csharp/Rasm/Rasm.csproj",),
                format_groups=(("libs/csharp/Rasm/Rasm.csproj", ("libs/csharp/Rasm/Vectors/Space.cs",)),),
            )
        ),
    )
    monkeypatch.setattr(static, "dotnet", fake_dotnet)
    monkeypatch.setattr(static, "dotnet_build", fail_build)

    assert static.run_static_rail(QualitySettings(root=tmp_path), _scope(tmp_path), "check").is_ok()
    assert seen == [
        ("format", "whitespace", str(tmp_path / "libs/csharp/Rasm/Rasm.csproj"), "--include", "libs/csharp/Rasm/Vectors/Space.cs", "--no-restore"),
        (
            "format",
            "style",
            str(tmp_path / "libs/csharp/Rasm/Rasm.csproj"),
            "--include",
            "libs/csharp/Rasm/Vectors/Space.cs",
            "--severity",
            "error",
            "--no-restore",
        ),
        (
            "format",
            "analyzers",
            str(tmp_path / "libs/csharp/Rasm/Rasm.csproj"),
            "--include",
            "libs/csharp/Rasm/Vectors/Space.cs",
            "--severity",
            "error",
            "--no-restore",
        ),
    ]


def test_static_build_runs_routed_build_without_format(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[Path, tuple[str | Path, ...], tuple[str, ...]]] = []

    def fake_dotnet_build(
        settings: QualitySettings,
        scope: ArtifactScope,
        *,
        restore: str | Path,
        targets: tuple[str | Path, ...],
        configurations: tuple[str, ...] = (),
        version: str = "",
        disable_parallel: bool = False,
        serial: bool = False,
        max_cpu: int | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "stream",
    ) -> Result[None, ProcessFault]:
        _ = (settings, scope, version, disable_parallel, serial, max_cpu, scoped, mode)
        seen.append((Path(restore), targets, configurations))
        return Ok(None)

    def fail_dotnet(*args: object, **kwargs: object) -> Result[Completed, ProcessFault]:
        _ = (args, kwargs)
        pytest.fail("static build must not run format")

    monkeypatch.setattr(static, "_plan", lambda _settings, _scope: Ok(static.StaticPlan(scope="changed", projects=("libs/csharp/Rasm/Rasm.csproj",))))
    monkeypatch.setattr(static, "dotnet_build", fake_dotnet_build)
    monkeypatch.setattr(static, "dotnet", fail_dotnet)

    assert static.run_static_rail(QualitySettings(root=tmp_path), _scope(tmp_path), "build").is_ok()
    assert seen == [(tmp_path / "Workspace.slnx", (str(tmp_path / "libs/csharp/Rasm/Rasm.csproj"),), ("Debug",))]


def test_focused_test_target_skips_mutation(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, scoped, mode)
        seen.append(args)
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path, test_target=Path("tests/tools/cs-analyzer/CsAnalyzer.Tests.csproj"))
    scope = _scope(tmp_path)

    assert test.run_test_rail(settings, scope, "run").is_ok()
    assert tuple(command[0] for command in seen) == ("test",)
    assert seen[0][:3] == ("test", "--project", str(tmp_path / "tests/tools/cs-analyzer/CsAnalyzer.Tests.csproj"))


def test_all_test_targets_use_solution_discovery(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, scoped, mode)
        seen.append(args)
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path)

    assert test.run_test_rail(settings, _scope(tmp_path), "list", all_targets=True).is_ok()
    assert seen == [
        (
            "test",
            "--solution",
            str(tmp_path / "Workspace.slnx"),
            "--configuration",
            settings.configuration,
            "--results-directory",
            str(tmp_path / ".artifacts/test/all" / settings.run_id),
            "--minimum-expected-tests",
            "1",
            "--list-tests",
        )
    ]


def test_default_test_target_runs_stryker_under_lock_with_bounded_concurrency(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []
    workdirs: list[Path | None] = []

    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, check, timeout, scoped, mode)
        seen.append(args)
        workdirs.append(cwd)
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path, mutation_max_cpu=2)
    scope = _scope(tmp_path)

    assert test.run_test_rail(settings, scope, "run", mutation="full").is_ok()
    assert tuple(command[0] for command in seen) == ("test", "tool", "tool")
    stryker = seen[-1]
    assert stryker[:4] == ("tool", "run", "dotnet-stryker", "--")
    assert "--solution" not in stryker
    assert stryker[stryker.index("--test-runner") + 1] == "mtp"
    assert stryker[stryker.index("--output") + 1] == str(settings.mutation_output_dir)
    assert stryker[stryker.index("--concurrency") + 1] == "2"
    assert tuple(stryker[index + 1] for index, value in enumerate(stryker) if value == "--mutate") == (
        "**/*.cs",
        "!**/bin/**/*.cs",
        "!**/obj/**/*.cs",
    )
    assert workdirs[-1] == tmp_path / "libs/csharp/Rasm"
    assert settings.mutation_lock.parent.is_dir()
    assert not settings.mutation_lock.exists()


def test_default_test_target_recovers_stale_mutation_lock(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, scoped, mode)
        seen.append(args)
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path)
    settings.mutation_lock.parent.mkdir(parents=True)
    settings.mutation_lock.write_text(data="", encoding="utf-8")

    assert test.run_test_rail(settings, _scope(tmp_path), "run", mutation="full").is_ok()
    assert tuple(command[0] for command in seen) == ("test", "tool", "tool")
    assert not settings.mutation_lock.exists()


def test_default_test_target_rejects_live_mutation_lock(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, scoped, mode)
        seen.append(args)
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path)
    settings.mutation_lock.parent.mkdir(parents=True)
    with settings.mutation_lock.open(mode="a+", encoding="utf-8") as lock:
        test._POSIX_LOCK.flock(lock.fileno(), test._POSIX_LOCK.exclusive | test._POSIX_LOCK.nonblock)
        try:
            result = test.run_test_rail(settings, _scope(tmp_path), "run", mutation="full")
        finally:
            test._POSIX_LOCK.flock(lock.fileno(), test._POSIX_LOCK.unlock)

    assert result.is_error()
    assert "mutation lock is already held" in result.error.message
    assert tuple(command[0] for command in seen) == ("test",)


def test_default_test_target_fails_stryker_zero_discovery(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, scoped, mode)
        match args[0]:
            case "tool" if args[1] == "run":
                return Ok(Completed(argv=("dotnet", *args), returncode=1, stdout=b"Number of tests found: 0\nNo test result reported.\n", stderr=b""))
            case _:
                return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path)
    scope = _scope(tmp_path)

    result = test.run_test_rail(settings, scope, "run", mutation="full")

    assert result.is_error()
    assert "Stryker discovered zero tests" in result.error.message


def test_default_test_target_keeps_real_stryker_failure(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    def fake_dotnet(
        *args: str,
        scope: ArtifactScope | None = None,
        cwd: Path | None = None,
        check: bool = True,
        timeout: float | None = None,
        scoped: bool = True,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (scope, cwd, check, timeout, scoped, mode)
        match args[0]:
            case "tool" if args[1] == "run":
                return Ok(Completed(argv=("dotnet", *args), returncode=1, stdout=b"mutation infrastructure failed", stderr=b""))
            case _:
                return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path)
    scope = _scope(tmp_path)

    result = test.run_test_rail(settings, scope, "run", mutation="full")

    assert result.is_error()
    assert "mutation infrastructure failed" in result.error.message


def test_package_duplicate_slug_fails_before_staging(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    (tmp_path / "apps").mkdir()
    (tmp_path / "tools").mkdir()
    projects = (tmp_path / "tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj", tmp_path / "apps/grasshopper/Radyab/Radyab.csproj")

    class FakeWorkspace:
        def __init__(self, root: Path) -> None:
            self.root = root

        @staticmethod
        def paths(args: tuple[str, ...], *, cwd: Path | None = None, suffix: str = "") -> tuple[Path, ...]:
            _ = (args, cwd, suffix)
            return projects

        @staticmethod
        def csproj(_project: Path, _tag: str = "") -> str:
            return "rasm-bridge"

    monkeypatch.setattr(package, "Workspace", FakeWorkspace)
    settings = QualitySettings(root=tmp_path)
    scope = _scope(tmp_path)

    result = package.run_package_rail(settings, scope, "package", "rasm-bridge", "0.0.0")

    assert result == Error(
        process.ProcessFault(argv=("package", "rasm-bridge"), returncode=2, stderr=b"Expected one package project for rasm-bridge, found duplicate")
    )


@pytest.mark.parametrize(
    "mode, slug, push_source, expected",
    [
        ("deploy", "rasm-bridge", "", ("quit", "install", "refresh")),
        ("deploy", "other", "", ("install",)),
        ("publish", "other", "yak-feed", ("install", "push")),
        ("publish", "rasm-bridge", "yak-feed", ("quit", "install", "refresh", "push")),
    ],
)
def test_package_finish_keeps_mode_and_slug_step_order(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, mode: package.PackageMode, slug: str, push_source: str, expected: tuple[str, ...]
) -> None:
    seen: list[str] = []
    package_file = tmp_path / "stage/package.yak"
    package_file.parent.mkdir()
    package_file.write_text("yak")
    meta = package.PackageMeta(
        project=tmp_path / "Rasm.RhinoBridge.Plugin.csproj",
        manifest_dir=tmp_path,
        target_dir=tmp_path,
        assembly_name="Rasm.RhinoBridge.Plugin",
        target_ext=".rhp",
        yak_path=Path("yak"),
        yak_platform="mac",
        yak_push_source=push_source,
        package_dir=tmp_path,
        package_pattern="*.yak",
    )
    artifact = package.PackageArtifact(stage=package_file.parent, meta=meta)
    settings = QualitySettings(root=tmp_path)
    scope = _scope(tmp_path)

    def fake_run(
        argv: tuple[str, ...],
        *,
        cwd: Path | None = None,
        env: dict[str, str] | None = None,
        check: bool = False,
        timeout: float | None = None,
        mode: process.ProcessMode = "capture",
    ) -> Result[Completed, ProcessFault]:
        _ = (cwd, env, check, timeout, mode)
        seen.append(argv[1])
        return Ok(Completed(argv=argv, returncode=0, stdout=b"", stderr=b""))

    def fake_client_step(name: str) -> Result[None, ProcessFault]:
        seen.append(name)
        return Ok(None)

    monkeypatch.setattr(package, "build_client", lambda *_: Ok(None))
    monkeypatch.setattr(package, "client_quit", lambda *_: fake_client_step(name="quit"))
    monkeypatch.setattr(package, "client_refresh", lambda *_: fake_client_step(name="refresh"))
    monkeypatch.setattr(package, "run", fake_run)

    assert package._finish(settings, scope, mode, slug, artifact).is_ok()
    assert tuple(seen) == expected
