from __future__ import annotations

from pathlib import Path
from typing import TYPE_CHECKING

from expression import Error, Ok
import msgspec
import pytest

from tools.quality import process
from tools.quality.__main__ import main
from tools.quality.process import Completed
from tools.quality.rails import bridge, package, static, test
from tools.quality.settings import ArtifactScope, QualitySettings


if TYPE_CHECKING:
    from expression import Result

    from tools.quality.process import ProcessFault


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


def test_self_test_exit_zero() -> None:
    assert main(["self-test"]) == 0


def test_dotnet_scope_flags_precede_passthrough_args(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_run(
        argv: tuple[str, ...], *, cwd: Path | None = None, env: dict[str, str] | None = None, check: bool = False
    ) -> Result[Completed, ProcessFault]:
        seen.append(argv)
        return Ok(Completed(argv=argv, returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(process, "run", fake_run)
    scope = ArtifactScope(root=tmp_path, pid=123, dotnet_env={"DOTNET_CLI_HOME": str(tmp_path)})

    assert process.dotnet("run", "--project", "client.csproj", "--", "doctor", scope=scope).is_ok()
    assert seen == [
        (
            "dotnet",
            "run",
            "--project",
            "client.csproj",
            "--artifacts-path",
            str(tmp_path / ".artifacts" / "agents" / "123"),
            "--disable-build-servers",
            "--",
            "doctor",
        )
    ]


def test_api_search_returns_ripgrep_stdout(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    xml = tmp_path / "Contents/Frameworks/RhCore.framework/Versions/A/Resources/RhinoCommon.xml"
    xml.parent.mkdir(parents=True)
    xml.write_text("<member name='Mesh' />")

    def fake_run(
        argv: tuple[str, ...], *, cwd: Path | None = None, env: dict[str, str] | None = None, check: bool = False
    ) -> Result[Completed, ProcessFault]:
        return Ok(Completed(argv=argv, returncode=0, stdout=b"Mesh hit\n", stderr=b""))

    monkeypatch.setattr(bridge, "run", fake_run)

    assert bridge.api(tmp_path, "search", "rhino-common", pattern="Mesh").default_value("") == "Mesh hit\n"


def test_api_decompile_returns_ilspy_stdout(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    def fake_run(
        argv: tuple[str, ...], *, cwd: Path | None = None, env: dict[str, str] | None = None, check: bool = False
    ) -> Result[Completed, ProcessFault]:
        return Ok(Completed(argv=argv, returncode=0, stdout=b"class Mesh {}\n", stderr=b""))

    monkeypatch.setattr(bridge, "run", fake_run)

    assert bridge.api(tmp_path, "decompile", "rhino-common", type_name="Rhino.Geometry.Mesh").default_value("") == (
        "class Mesh {}\n"
    )


def _invoke_verify(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    *statuses: bridge.BridgeStatus,
    client_fault: process.ProcessFault | None = None,
) -> tuple[bridge.BridgeResult, tuple[tuple[str, ...], ...], tuple[dict[str, object], ...]]:
    calls: list[tuple[str, ...]] = []
    kwargs_seen: list[dict[str, object]] = []
    settings = QualitySettings(root=tmp_path)
    scope = ArtifactScope(root=tmp_path, pid=123, dotnet_env={})
    report_dir = settings.bridge_verify_dir
    report_dir.mkdir(parents=True, exist_ok=True)
    scenario = tmp_path / "case.verify.csx"
    scenario.write_text("// scenario")
    result_path = report_dir / "case.json"

    def fake_client_run(
        settings: QualitySettings,
        scope: ArtifactScope,
        *args: str,
        check: bool = True,
        build: bridge.BuildPolicy = "always",
    ) -> Result[Completed, ProcessFault]:
        _ = (settings, scope)
        calls.append(args)
        kwargs_seen.append({"check": check, "build": build})
        match client_fault:
            case process.ProcessFault() as fault:
                return Error(fault)
            case None:
                status = statuses[min(len(calls) - 1, len(statuses) - 1)]
                result_path.write_bytes(msgspec.json.encode(bridge.BridgeResult(status=status)))
                return Ok(Completed(argv=("bridge", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(bridge, "client_run", fake_client_run)

    return (
        bridge._verify_invoke(settings, scope, report_dir, tmp_path / "Rasm.csproj", scenario),
        tuple(calls),
        tuple(kwargs_seen),
    )


@pytest.mark.parametrize("status", ["busy", "timeout"])
def test_verify_invoke_retries_transient_status_then_ok(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, status: bridge.BridgeStatus
) -> None:
    result, calls, _ = _invoke_verify(monkeypatch, tmp_path, status, "ok")

    assert result.status == "ok"
    assert len(calls) == 2


@pytest.mark.parametrize("status", ["ok"])
def test_verify_invoke_does_not_retry_terminal_status(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, status: bridge.BridgeStatus
) -> None:
    result, calls, _ = _invoke_verify(monkeypatch, tmp_path, status)

    assert result.status == status
    assert len(calls) == 1


@pytest.mark.parametrize("status", ["failed", "unsupported", "skipped", "busy", "timeout"])
def test_verify_invoke_retries_non_ok_status(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path, status: bridge.BridgeStatus
) -> None:
    result, calls, _ = _invoke_verify(monkeypatch, tmp_path, status, "ok")

    assert result.status == "ok"
    assert len(calls) == 2


def test_verify_invoke_stops_after_max_tries(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    result, calls, _ = _invoke_verify(monkeypatch, tmp_path, "busy", "busy")

    assert result.status == "busy"
    assert len(calls) == 2


def test_verify_invoke_maps_client_fault_to_failed_without_retry(
    monkeypatch: pytest.MonkeyPatch, tmp_path: Path
) -> None:
    result, calls, _ = _invoke_verify(
        monkeypatch, tmp_path, "ok", client_fault=process.ProcessFault.fail("bridge", detail=b"boom", returncode=9)
    )

    assert result.status == "failed"
    assert result.fault is not None
    assert "boom" in result.fault.message
    assert len(calls) == 2


def test_verify_invoke_check_uses_never_build(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    _, calls, kwargs_seen = _invoke_verify(monkeypatch, tmp_path, "busy", "ok")

    assert tuple(call[0] for call in calls) == ("check", "check")
    assert kwargs_seen == ({"check": False, "build": "never"}, {"check": False, "build": "never"})


def test_verify_discover_returns_directory_matches(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    scenarios = tmp_path / "scenarios"
    scenarios.mkdir()
    scenario = scenarios / "case.verify.csx"
    scenario.write_text("// scenario")

    def fake_paths(
        self: process.Workspace, args: tuple[str, ...], *, cwd: Path | None = None, suffix: str = ""
    ) -> tuple[Path, ...]:
        _ = (self, args, cwd, suffix)
        return (scenario,)

    monkeypatch.setattr(process.Workspace, "paths", fake_paths)

    assert bridge._verify_discover(process.Workspace(tmp_path), tmp_path, str(scenarios)) == (scenario,)


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

    scope = ArtifactScope(root=tmp_path, pid=123, dotnet_env={})

    plan = static._plan(QualitySettings(root=tmp_path), "changed", scope).default_value(
        static.StaticPlan("changed", ())
    )

    assert plan.scope == "changed"
    assert plan.projects == ("libs/csharp/Rasm/Rasm.csproj",)


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

    scope = ArtifactScope(root=tmp_path, pid=123, dotnet_env={})

    plan = static._plan(QualitySettings(root=tmp_path), "changed", scope).default_value(
        static.StaticPlan("full", ("x",))
    )

    assert plan == static.StaticPlan(scope="changed", projects=())


def test_focused_test_target_skips_mutation(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    seen: list[tuple[str, ...]] = []

    def fake_dotnet(
        *args: str, scope: ArtifactScope | None = None, cwd: Path | None = None, check: bool = True
    ) -> Result[Completed, ProcessFault]:
        seen.append(args)
        return Ok(Completed(argv=("dotnet", *args), returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(test, "dotnet", fake_dotnet)
    settings = QualitySettings(root=tmp_path, test_target=Path("tests/tools/cs-analyzer/CsAnalyzer.Tests.csproj"))
    scope = ArtifactScope(root=tmp_path, pid=123, dotnet_env={})

    assert test.run_test_rail(settings, scope, "run").is_ok()
    assert tuple(command[0] for command in seen) == ("test",)


def test_package_duplicate_slug_fails_before_staging(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> None:
    (tmp_path / "apps").mkdir()
    (tmp_path / "tools").mkdir()
    projects = (
        tmp_path / "tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj",
        tmp_path / "apps/grasshopper/Radyab/Radyab.csproj",
    )

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
    scope = ArtifactScope(root=tmp_path, pid=123, dotnet_env={})

    result = package.run_package_rail(settings, scope, "package", "rasm-bridge", "0.0.0")

    assert result == Error(
        process.ProcessFault(
            argv=("package", "rasm-bridge"),
            returncode=2,
            stderr=b"Expected one package project for rasm-bridge, found duplicate",
        )
    )


@pytest.mark.parametrize(
    "mode, slug, push_source, expected",
    [
        ("deploy", "rasm-bridge", "", ("quit", "install", "launch", "doctor")),
        ("deploy", "other", "", ("install",)),
        ("publish", "other", "yak-feed", ("install", "push")),
        ("publish", "rasm-bridge", "yak-feed", ("quit", "install", "launch", "doctor", "push")),
    ],
)
def test_package_finish_keeps_mode_and_slug_step_order(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path: Path,
    mode: package.PackageMode,
    slug: str,
    push_source: str,
    expected: tuple[str, ...],
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
    scope = ArtifactScope(root=tmp_path, pid=123, dotnet_env={})

    def fake_client_run(
        settings: QualitySettings,
        scope: ArtifactScope,
        *args: str,
        check: bool = True,
        build: bridge.BuildPolicy = "always",
    ) -> Result[Completed, ProcessFault]:
        _ = (settings, scope, check, build)
        seen.append(args[0])
        return Ok(Completed(argv=("bridge", *args), returncode=0, stdout=b'{"status":"ok"}', stderr=b""))

    def fake_run(
        argv: tuple[str, ...], *, cwd: Path | None = None, env: dict[str, str] | None = None, check: bool = False
    ) -> Result[Completed, ProcessFault]:
        _ = (cwd, env, check)
        seen.append(argv[1])
        return Ok(Completed(argv=argv, returncode=0, stdout=b"", stderr=b""))

    monkeypatch.setattr(package, "client_run", fake_client_run)
    monkeypatch.setattr(package, "run", fake_run)

    assert package._finish(settings, scope, mode, slug, artifact).is_ok()
    assert tuple(seen) == expected
