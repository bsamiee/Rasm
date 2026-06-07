"""Adversarial laws for the Python quality operator."""

# --- [IMPORTS] ------------------------------------------------------------------------

from pathlib import Path
import plistlib
import shutil
import sys
import tempfile
from types import SimpleNamespace
from typing import TYPE_CHECKING

from dirty_equals import IsFloat, IsPartialDict
from expression import Error, Ok, Result
from hypothesis import given, strategies as st
from inline_snapshot import snapshot
import msgspec
import pytest

from tests.tools.quality.conftest import (
    API_DOCTOR,
    CLI_LAWS,
    PackageShape,
    ProjectGraph,
    QualityHarness,
    QualityWorkspaceDouble,
    RailProbe,
    TARGET_LAWS,
)
from tools.quality import __main__ as quality_cli, process as process_mod
from tools.quality.process import Completed, dotnet_args, dotnet_build_invocations, DotnetInvocation, fold, ProcessFault, RailStatus, run, Workspace
from tools.quality.rails import api as api_rail, bridge as bridge_rail, package as package_rail, static as static_rail, test as test_rail
from tools.quality.settings import QualitySettings


if TYPE_CHECKING:
    from tests.tools.quality.conftest import CliLaw, TargetLaw


# --- [OPERATIONS] ----------------------------------------------------------------------


def _patch_settings(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    monkeypatch.setattr(quality_cli, "QualitySettings", lambda: quality.settings)


def _bridge_completed(status: RailStatus) -> Completed:
    return Completed(("bridge",), 0, msgspec.json.encode(bridge_rail.BridgeResult(status=status)), b"")


# --- [CLI LAWS] ------------------------------------------------------------------------


@pytest.mark.parametrize("law", CLI_LAWS, ids=lambda law: "-".join(law.argv))
def test_cli_verbs_emit_one_json_envelope(
    monkeypatch: pytest.MonkeyPatch, quality: QualityHarness, capsys: pytest.CaptureFixture[str], law: CliLaw
) -> None:
    probe = RailProbe()
    _patch_settings(monkeypatch, quality)
    law.install(monkeypatch, probe)

    assert quality_cli.main(list(law.argv)) == 0
    envelope = quality.envelope(capsys)

    assert envelope == IsPartialDict(
        rail=law.rail, verb=law.verb, command_path=list(law.command_path), status="ok", exit_code=0, duration_ms=IsFloat(ge=0)
    )
    assert envelope["data"] is not None
    assert probe.calls
    assert probe.calls[0][0] == law.member


def test_cli_remaining_verbs_emit_total_status_envelopes(
    monkeypatch: pytest.MonkeyPatch, quality: QualityHarness, capsys: pytest.CaptureFixture[str]
) -> None:
    _patch_settings(monkeypatch, quality)
    monkeypatch.setattr(shutil, "which", lambda _: None)

    assert quality_cli.main(["self-test"]) == 2
    failed = quality.envelope(capsys)
    assert failed["rail"] == "self"
    assert failed["status"] == "failed"

    monkeypatch.setattr(shutil, "which", lambda _: "/bin/tool")
    monkeypatch.setattr(quality_cli, "dotnet_build", lambda *_args, **_kwargs: Ok(None))
    monkeypatch.setattr(bridge_rail, "with_bridge_lease", lambda _settings, action: action())
    monkeypatch.setattr(bridge_rail, "build_client", lambda *_args: Ok(None))
    monkeypatch.setattr(bridge_rail, "client_run", lambda *_args, **_kwargs: Ok(_bridge_completed(RailStatus.SKIP)))
    monkeypatch.setattr(static_rail, "run_static_rail", lambda *_args, **_kwargs: Ok("full-trigger-skip"))
    monkeypatch.setattr(
        test_rail, "run_test_rail", lambda *_args, **_kwargs: Ok(test_rail.TestRunReport(query={"op": "run"}, status="ok", artifact_paths={}))
    )
    monkeypatch.setattr(
        package_rail,
        "package_list_payload",
        lambda *_args: Ok(
            msgspec.json.encode(package_rail.PackageListReport(query={"op": "package-list"}, status="ok", count=0, packages=(), artifact_paths={}))
        ),
    )
    monkeypatch.setattr(package_rail, "run_package_rail", lambda *_args: Ok(None))

    for argv, rail, status, code in (
        (("self-test",), "self", "ok", 0),
        (("static", "fix"), "static", "skip", 0),
        (("test", "run"), "test", "ok", 0),
        (("test", "coverage"), "test", "ok", 0),
        (("bridge", "build-bridge"), "bridge", "ok", 0),
        (("bridge", "check", "target"), "bridge", "skip", 0),
        (("bridge", "clean"), "bridge", "skip", 0),
        (("bridge", "package", "list"), "package", "ok", 0),
        (("bridge", "package", "rasm-bridge", "1.0.0"), "package", "ok", 0),
    ):
        assert quality_cli.main(list(argv)) == code
        assert quality.envelope(capsys) == IsPartialDict(rail=rail, status=status, exit_code=code)


def test_cli_projection_laws() -> None:
    status_laws: tuple[tuple[RailStatus, int], ...] = (
        (RailStatus.OK, 0),
        (RailStatus.EMPTY, 0),
        (RailStatus.SKIP, 0),
        (RailStatus.UNSUPPORTED, 3),
        (RailStatus.BUSY, 5),
        (RailStatus.TIMEOUT, 5),
        (RailStatus.FAILED, 1),
    )
    for status, code in status_laws:
        assert status.exit_code == code
    assert quality_cli.api_status(msgspec.json.encode({"status": "empty"})) == "empty"
    assert quality_cli.api_status(msgspec.json.encode({"status": "missing"})) == "failed"
    assert quality_cli.api_status(b"not-json") == "ok"
    assert quality_cli.api_exit_code(msgspec.json.encode({"status": "failed"}), strict=True) == 2
    assert quality_cli.api_exit_code(msgspec.json.encode({"status": "failed"}), strict=False) == 0
    assert quality_cli.quality_payload(42) == "42"
    assert quality_cli.payload_json(None) is None
    assert QualityHarness.json_object(quality_cli.payload_json(API_DOCTOR))["status"] == "ok"
    item = QualityHarness.json_sequence(QualityHarness.json_object(quality_cli.json_value({"items": [object()]}))["items"])[0]
    assert isinstance(item, str)
    assert item.startswith("<object object")
    assert QualityHarness.json_object(quality_cli.payload_json(Completed(("cmd",), 0, b"out", b"err")))["stdout"] == "out"
    assert quality_cli.payload_json(b"not-json") == "not-json"
    assert quality_cli.static_status("done") == "ok"
    assert quality_cli.static_notes("full-trigger-skip") == ("full-trigger input requires static full",)
    assert quality_cli.static_notes("skip") == ("no C#-relevant changes",)
    assert quality_cli.static_notes("done") == ()
    assert quality_cli.bridge_status(Completed(("bridge",), 0, b"not-json", b"")) == "ok"
    assert quality_cli.bridge_status(Completed(("bridge",), 9, b"not-json", b"")) == "failed"
    assert quality_cli.bridge_exit_code(Completed(("bridge",), 9, b"not-json", b"")) == 9
    wire_laws: tuple[tuple[bytes, RailStatus], ...] = (
        (b'{"status":"ok"}', RailStatus.OK),
        (b'{"status":"skipped"}', RailStatus.SKIP),
        (b'{"status":"unsupported"}', RailStatus.UNSUPPORTED),
        (b'{"status":"failed"}', RailStatus.FAILED),
        (b'{"status":"timeout"}', RailStatus.TIMEOUT),
        (b'{"status":"busy"}', RailStatus.BUSY),
    )
    for wire, rail in wire_laws:
        assert bridge_rail.try_decode_bridge(wire).default_value(bridge_rail.BridgeResult()).status is rail


# --- [PROCESS AND SETTINGS LAWS] -------------------------------------------------------


def test_process_stream_timeout_settings_and_dotnet_laws(quality: QualityHarness, tmp_path: Path, capfd: pytest.CaptureFixture[str]) -> None:
    scope = quality.scope("process", tmp_path / "scope")
    noisy = "import sys; print('child-out'); print('child-err', file=sys.stderr); sys.stdout.write('o' * 140000)"
    streamed = run((sys.executable, "-c", noisy), mode="stream", artifact_dir=tmp_path)
    captured = capfd.readouterr()
    process_dir = next(path for path in tmp_path.iterdir() if (path / "stdout.log").is_file())

    assert streamed.is_ok()
    assert not captured.out
    assert "child-out" in captured.err
    assert "child-err" in captured.err
    assert len(streamed.default_value(Completed((), 1, b"", b"")).stdout) < 140010
    assert (process_dir / "stdout.log").stat().st_size > 140000
    assert (process_dir / "stderr.log").read_text(encoding="utf-8").strip() == "child-err"
    assert (
        run((sys.executable, "-c", "import sys; print('bad'); sys.exit(7)"), check=True)
        .swap()
        .default_value(ProcessFault.fail("x", detail=b""))
        .returncode
        == 7
    )
    assert (
        run((sys.executable, "-c", "import time; time.sleep(1)"), timeout=0.01).swap().default_value(ProcessFault.fail("x", detail=b"")).status
        == "timeout"
    )
    assert quality.settings.model_copy(update={"configurations": "Debug Release"}).static_configurations("full") == ("Debug", "Release")
    assert QualitySettings.anchor(quality.root / "nested") == quality.root
    assert quality.settings.version_props("1.2.3") == ("/p:Version=1.2.3", "/p:InformationalVersion=1.2.3")
    assert quality.settings.dotnet_env(scope.path)["DOTNET_CLI_HOME"].endswith("scope/dotnet-cli")
    assert DotnetInvocation(("build", "App.csproj", "--", "--flag")).argv(scope)[:5] == (
        "dotnet",
        "build",
        "App.csproj",
        "--artifacts-path",
        str(scope.path),
    )
    assert DotnetInvocation(("tool", "restore")).argv(scope) == ("dotnet", "tool", "restore")
    assert DotnetInvocation(("build", "App.csproj"), mode="stream").artifact_dir(scope) == scope.path / "process"
    assert DotnetInvocation(("build", "App.csproj")).env() is None
    assert dotnet_args("restore", "App.csproj", disable_parallel=True) == snapshot(("restore", "App.csproj", "--locked-mode", "--disable-parallel"))
    assert "-maxcpucount:1" in dotnet_args("build", "App.csproj", configuration="Release", serial=True, quiet=True)
    assert tuple(item.argv(scope)[1] for item in dotnet_build_invocations(quality.settings, targets=("App.csproj",), restore="App.csproj")) == (
        "restore",
        "build",
    )
    assert fold((1, 2, 3), 0, lambda acc, item: Ok(acc + item)).default_value(0) == 6
    assert process_mod.RailStatus.from_returncode(5) == "busy"


def test_workspace_xml_and_apphost_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness, tmp_path: Path) -> None:
    project = quality.project("libs/A/A.csproj", "<Project><PropertyGroup><YakPackageSlug>slug</YakPackageSlug></PropertyGroup></Project>")
    calls: list[tuple[str, ...]] = []

    def fake_run(argv: tuple[str, ...], **_kwargs: object) -> Result[Completed, ProcessFault]:
        calls.append(argv)
        match argv:
            case ("fd", *_):
                return Ok(Completed(argv, 0, b"libs/A/A.csproj\n", b""))
            case ("git", *_):
                return Ok(Completed(argv, 0, b"libs/A/A.csproj\n", b""))
            case ("dotnet", "--list-runtimes"):
                return Ok(Completed(argv, 0, f"Microsoft.NETCore.App 10.0.0 [{tmp_path / 'dotnet/shared/Microsoft.NETCore.App'}]\n".encode(), b""))
            case _:
                return Ok(Completed(argv, 0, b"", b""))

    (tmp_path / "dotnet/shared/Microsoft.NETCore.App").mkdir(parents=True)
    monkeypatch.setattr(process_mod, "run", fake_run)
    process_mod._dotnet_root.cache_clear()
    workspace = Workspace(quality.root)
    assert workspace.projects().default_value(()) == ("libs/A/A.csproj",)
    assert workspace.changed().default_value(()) == ("libs/A/A.csproj",)
    assert Workspace.csproj(project, "YakPackageSlug") == "slug"
    assert Workspace.csproj(quality.root / "missing.csproj") is None
    assert process_mod.fd_args("csproj", ".", tmp_path)[0:3] == ("fd", "-H", "-e")
    assert process_mod.dotnet_apphost_env({"DOTNET_ROOT": "/bad"})["DOTNET_MULTILEVEL_LOOKUP"] == "0"
    assert calls


def test_process_lease_and_error_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness, tmp_path: Path) -> None:
    project = quality.project("libs/A/A.csproj", "<Project><PropertyGroup><YakPackageSlug>slug</YakPackageSlug></PropertyGroup></Project>")
    workspace = Workspace(quality.root)
    assert process_mod.leased(quality.settings, tmp_path / "lock", "resource", lambda: Ok("ok")).default_value("") == "ok"
    with pytest.MonkeyPatch.context() as lease_patch:
        lease_patch.setattr(
            process_mod, "exclusive_lease", lambda *_args, **_kwargs: (_ for _ in ()).throw(process_mod.ResourceBusyError(tmp_path / "lock", "owner"))
        )
        assert (
            process_mod
            .leased(quality.settings, tmp_path / "lock", "resource", lambda: Ok(None))
            .swap()
            .default_value(ProcessFault.fail("x", detail=b""))
            .status
            == "busy"
        )
    with pytest.MonkeyPatch.context() as lease_patch:
        lease_patch.setattr(process_mod, "exclusive_lease", lambda *_args, **_kwargs: (_ for _ in ()).throw(OSError("locked")))
        assert process_mod.leased(quality.settings, tmp_path / "lock", "resource", lambda: Ok(None)).is_error()
    assert process_mod.decode_json(b"{", dict).is_error()
    assert process_mod.decode_json('{"x":1}', dict).default_value({}) == {"x": 1}
    assert process_mod.xml_root(tmp_path / "missing.csproj") is None
    assert Completed(("x",), 0, b"a.cs\nb.txt\n", b"").lines(suffix=".cs") == ("a.cs",)
    assert workspace.owner({}, tmp_path / "NoOwner.cs").is_error()
    assert workspace.owner({tmp_path: project, tmp_path / "src": tmp_path / "B.csproj"}, tmp_path / "src/File.cs").is_error()
    with pytest.MonkeyPatch.context() as dotnet_root_patch:
        dotnet_root_patch.setattr(process_mod, "_dotnet_root", lambda: None)
        assert "DOTNET_ROOT" not in process_mod.dotnet_apphost_env({"DOTNET_ROOT": "/bad"})


def test_dotnet_tool_boundary_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    monkeypatch.setattr(process_mod, "run", lambda argv, **_kwargs: Ok(Completed(tuple(argv), 0, b"", b"")))
    assert DotnetInvocation(("tool", "restore")).run(quality.scope()).is_ok()
    assert process_mod.dotnet("tool", "restore", scope=quality.scope()).is_ok()
    assert process_mod.dotnet_build(quality.settings, quality.scope(), targets=("A.csproj",)).is_ok()
    assert process_mod.dotnet_tool(quality.scope(), "ilspycmd", "--version").is_ok()
    assert process_mod.dotnet_tool_restore(quality.scope()).is_ok()
    process_mod._dotnet_root.cache_clear()
    monkeypatch.setattr(shutil, "which", lambda _: None)
    monkeypatch.setattr(process_mod, "run", lambda *_args, **_kwargs: Ok(Completed(("dotnet",), 0, b"invalid runtime line\n", b"")))
    assert process_mod._dotnet_root() is None


# --- [STATIC RAIL LAWS] ----------------------------------------------------------------


@pytest.mark.property
@given(st.lists(st.sampled_from(("README.md", "tools/quality/process.py", "docs/usage/README.md")), max_size=8).map(tuple))
def test_static_empty_or_irrelevant_changes_never_resolve_project_graph(rows: tuple[str, ...]) -> None:
    with tempfile.TemporaryDirectory() as raw, pytest.MonkeyPatch.context() as monkeypatch:
        harness = QualityHarness(Path(raw), QualitySettings(root=Path(raw), rhino_app=None, run_id="test-run"))
        harness.write("Workspace.slnx")
        workspace = QualityWorkspaceDouble(harness.root, rows, fail_graph=True)
        monkeypatch.setattr(static_rail, "Workspace", lambda _: workspace)
        report = harness.expect_json(static_rail.plan_payload(harness.settings, harness.scope("static")))
        assert report["projects"] == []
        assert workspace.calls["index"] == 0
        assert workspace.calls["projects"] == 0


@pytest.mark.property
@given(rows=st.permutations(("tests/tools/py_analyzer/x.cs", "Directory.Build.props", "README.md", "README.md")))
def test_static_plan_is_permutation_invariant_and_lazy_for_full_triggers(rows: tuple[str, ...]) -> None:
    with tempfile.TemporaryDirectory() as raw, pytest.MonkeyPatch.context() as monkeypatch:
        root = Path(raw)
        (root / "Workspace.slnx").write_text("", encoding="utf-8")
        harness = QualityHarness(root, QualitySettings(root=root, rhino_app=None, run_id="test-run"))
        graph = ProjectGraph()
        graph.materialize(harness)
        workspace = graph.workspace(harness, changed=rows)
        monkeypatch.setattr(static_rail, "Workspace", lambda _: workspace)
        monkeypatch.setattr(
            static_rail, "run", lambda *_args, **_kwargs: Ok(Completed(("dotnet", "sln"), 0, b"libs/A/A.csproj\nlibs/B/B.csproj\n", b""))
        )

        report = harness.expect_json(static_rail.plan_payload(harness.settings, harness.scope("static")))

        assert report["scope"] == "full"
        assert report["full_triggers"] == ["Directory.Build.props"]
        assert workspace.calls["index"] == 0
        assert report["projects"] == ["libs/A/A.csproj", "libs/B/B.csproj"]


def test_static_owned_routes_explicit_inputs_and_modes(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    graph = ProjectGraph()
    graph.materialize(quality)
    workspace = graph.workspace(quality, changed=("libs/A/A.cs",))
    calls: list[tuple[str, ...]] = []

    def fake_dotnet(*args: str, **_kwargs: object) -> Result[Completed, ProcessFault]:
        calls.append(args)
        return Ok(Completed(("dotnet", *args), 0, b"", b""))

    monkeypatch.setattr(static_rail, "Workspace", lambda _: workspace)
    monkeypatch.setattr(static_rail, "run", lambda argv, **_kwargs: Ok(Completed(tuple(argv), 0, b"libs/A/A.cs\n", b"")))
    monkeypatch.setattr(static_rail, "dotnet", fake_dotnet)
    monkeypatch.setattr(static_rail, "dotnet_build", lambda *_args, **_kwargs: Error(ProcessFault.fail("build", detail=b"", returncode=0)))

    report = quality.expect_json(static_rail.plan_payload(quality.settings, quality.scope("static")))
    assert report["scope"] == "changed"
    assert report["owners"] == ["libs/A/A.csproj"]
    assert report["projects"] == ["libs/A/A.csproj", "libs/B/B.csproj"]
    assert report["format_groups"] == [["libs/A/A.csproj", ["libs/A/A.cs"]]]
    commands = QualityHarness.json_object(report["commands"])
    build_commands = QualityHarness.json_sequence(commands["build"])
    assert tuple(QualityHarness.json_sequence(command)[1] for command in build_commands) == ("restore", "restore", "build", "build")
    assert static_rail.plan_payload(quality.settings, quality.scope("static"), (Path("missing.cs"),)).is_error()
    assert quality.expect_json(static_rail.plan_payload(quality.settings, quality.scope("static"), (quality.root / "libs/A",)))["inputs"] == [
        "libs/A/A.cs"
    ]
    assert static_rail.run_static_rail(quality.settings, quality.scope("static"), "fix").default_value("skip") == "done"
    assert static_rail.run_static_rail(quality.settings, quality.scope("static"), "build").default_value("done") == "skip"
    assert static_rail._skip(ProcessFault.fail("static", detail=b"", returncode=2)).is_error()
    assert any("--include" in call for call in calls)


def test_static_format_and_full_edge_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    scope = quality.scope("static")
    empty = static_rail.StaticPlan(scope="changed", projects=(), full_triggers=())
    full_trigger = static_rail.StaticPlan(scope="full", projects=(), full_triggers=("Directory.Build.props",))
    file_plan = static_rail.StaticPlan(scope="changed", projects=("P.csproj",), format_groups=(("P.csproj", ("A.cs",)),))
    monkeypatch.setattr(static_rail, "dotnet", lambda *args, **_kwargs: Ok(Completed(("dotnet", *args), 0, b"", b"")))
    monkeypatch.setattr(static_rail, "dotnet_build", lambda *_args, **_kwargs: Ok(None))
    monkeypatch.setattr(static_rail, "_plan", lambda *_args, **_kwargs: Ok(static_rail.StaticPlan(scope="full", projects=("P.csproj",))))

    assert static_rail._format_plan(quality.settings, scope, full_trigger, "fix").default_value("done") == "full-trigger-skip"
    assert static_rail._format_plan(quality.settings, scope, empty, "fix").default_value("done") == "skip"
    assert static_rail._format_plan(quality.settings, scope, file_plan, "report").default_value("skip") == "done"
    assert static_rail._route_need("tools/cs-analyzer/Rule.cs") == "full"
    assert (
        static_rail._route_step(static_rail._ChangedRoute(), "orphan.cs", index={}, workspace=Workspace(quality.root), root=quality.root).full is True
    )
    assert static_rail._relative(quality.root, Path("/outside/file.cs")) == "/outside/file.cs"
    assert static_rail.run_static_rail(quality.settings, scope, "full").default_value("skip") == "done"


# --- [TEST RAIL LAWS] ------------------------------------------------------------------


@pytest.mark.parametrize("law", list(TARGET_LAWS), ids=lambda law: f"{law.mode}-{law.mutation}-{law.test_modules or 'default'}")
def test_test_rail_rejects_invalid_target_and_mutation_shapes(quality: QualityHarness, law: TargetLaw) -> None:
    result = test_rail.run_test_rail(
        quality.settings,
        quality.scope(),
        law.mode,
        all_targets=law.all_targets,
        mutation=law.mutation,
        test_modules=law.test_modules,
        explicit_target=law.explicit,
    )
    assert result.is_error()
    assert law.needle in result.swap().default_value(ProcessFault.fail("missing", detail=b"")).stderr


def test_test_rail_selector_list_and_mutation_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    quality.write("libs/csharp/Rasm/Changed.cs")
    calls: list[tuple[str, ...]] = []

    def fake_dotnet(*args: str, **_kwargs: object) -> Result[Completed, ProcessFault]:
        calls.append(args)
        return Ok(Completed(("dotnet", *args), 0, b"AlphaTests\nBeta Laws\nGamma.Spec\n", b""))

    monkeypatch.setattr(test_rail, "dotnet", fake_dotnet)
    monkeypatch.setattr(
        test_rail, "Workspace", lambda _: QualityWorkspaceDouble(quality.root, ("libs/csharp/Rasm/Changed.cs", "libs/csharp/Rasm/obj/Skip.cs"))
    )
    monkeypatch.setattr(test_rail, "log", SimpleNamespace(info=lambda *_args, **_kwargs: None))

    listed = quality.expect_json(test_rail.list_tests_payload(quality.settings, quality.scope(), grep="alpha", limit=1))
    assert listed["tests"] == ["AlphaTests"]
    for filter_expr in ("/name", "Category=Fast", "Nested+Type", "Name.Spec"):
        assert test_rail.run_test_rail(quality.settings, quality.scope(), "run", filter_expr=filter_expr).is_ok()
    assert (
        test_rail
        .run_test_rail(quality.settings, quality.scope(), "run", mutation="changed")
        .default_value(test_rail.TestRunReport(query={}, status="failed", artifact_paths={}))
        .mutation
        == "changed"
    )
    assert test_rail.list_tests_payload(quality.settings, quality.scope(), all_targets=True, test_modules="x.dll").is_error()
    assert test_rail.run_test_rail(
        quality.settings.model_copy(update={"mutation_test_project": Path("Other.csproj")}), quality.scope(), "run", mutation="full"
    ).is_error()
    assert test_rail.run_test_rail(quality.settings, quality.scope(), "run", test_modules="*.dll", filter_expr="Name.Spec", no_build=True).is_ok()
    assert test_rail.run_test_rail(quality.settings, quality.scope(), "run", mutation="full").is_ok()
    assert test_rail._mutation_result(Completed(("stryker",), 0, b"Number of tests found: 0\nNo test result reported", b"")).is_error()
    assert test_rail._mutation_result(Completed(("stryker",), 9, b"bad", b"err")).is_error()
    assert any("dotnet-stryker" in call and "Changed.cs" in call and "Skip.cs" not in call for call in calls)
    assert any("--test-modules" in call for call in calls)
    assert any("**/*.cs" in call for call in calls)


# --- [API LAWS] ------------------------------------------------------------------------


def test_api_package_source_query_show_and_failure_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    scope = quality.scope("api", quality.root / ".artifacts/quality/api/test-run")
    package_root = quality.root / ".cache/nuget/packages/example.core/1.0.0"
    ref = package_root / "ref/net10.0"
    ref.mkdir(parents=True)
    assembly = ref / "Example.Core.dll"
    assembly.write_bytes(b"dll")
    (ref / "Example.Core.xml").write_text(
        "<doc><members>"
        "<member name='T:Example.Core.Widget'>Widget docs</member>"
        "<member name='M:Example.Core.Widget.Run'>Run docs</member>"
        "</members></doc>",
        encoding="utf-8",
    )
    quality.write(
        "Directory.Packages.props",
        "<Project><ItemGroup>"
        '<PackageVersion Include="Example.Core" Version="1.0.0" />'
        '<PackageVersion Include="Example.Ui" Version="1.0.0" />'
        "</ItemGroup></Project>",
    )
    monkeypatch.setattr(api_rail, "dotnet_tool_restore", lambda *_args: Ok(None))
    monkeypatch.setattr(
        api_rail,
        "dotnet_tool",
        lambda _scope, _name, *args, **_kwargs: Ok(
            Completed(
                ("ilspycmd", *args),
                0,
                b"Class Example.Core.Widget\nClass Example.Core.Widget.Nested\nInterface Example.Core.Contract\n"
                if "-l" in args
                else b"namespace Example.Core { public sealed class Widget { public void Run() {} public void Other() {} } }",
                b"",
            )
        ),
    )

    resolve = quality.expect(api_rail.api(quality.settings, scope, "resolve", "example.core", kind="all"), api_rail.ApiQueryReport)
    index = quality.expect(api_rail.api(quality.settings, scope, "query", "example.core", symbol=""), api_rail.ApiQueryReport)
    namespace = quality.expect(api_rail.api(quality.settings, scope, "query", "example.core", symbol="Example.Core"), api_rail.ApiQueryReport)
    member = quality.expect(api_rail.api(quality.settings, scope, "query", "example.core", symbol="Widget.Run"), api_rail.ApiQueryReport)
    search = quality.expect(api_rail.api(quality.settings, scope, "query", "example.core", symbol="Missing"), api_rail.ApiQueryReport)
    shown = quality.expect(api_rail.api(quality.settings, scope, "show", "decompile.cs", full=True, show_policy="latest"), api_rail.ApiShowReport)

    assert resolve.status == "ok"
    assert index.shape == "index"
    assert index.counts["namespaces"] == 1
    assert namespace.shape == "namespace"
    assert namespace.counts["types"] == 3
    assert member.shape == "member"
    assert "Run" in member.signature
    assert search.status == "empty"
    assert shown.content
    assert api_rail.api(quality.settings, scope, "resolve", "example").is_error()


def test_api_doctor_host_and_report_show_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    rhino = quality.mkdir("RhinoWIP.app")
    resources = rhino / "Contents/Frameworks/RhCore.framework/Versions/Current/Resources"
    resources.mkdir(parents=True)
    (resources / "RhinoCommon.dll").write_bytes(b"dll")
    (resources / "RhinoCommon.xml").write_text("<doc />", encoding="utf-8")
    rhinocode = rhino / "Contents/Resources/bin/rhinocode"
    rhinocode.parent.mkdir(parents=True)
    rhinocode.write_text("", encoding="utf-8")
    settings = quality.settings.model_copy(update={"rhino_app": rhino})
    scope = quality.scope("api", quality.root / ".artifacts/quality/api/test-run")
    monkeypatch.setattr(api_rail, "dotnet_tool_restore", lambda *_args: Ok(None))
    monkeypatch.setattr(api_rail, "dotnet_tool", lambda *_args, **_kwargs: Ok(Completed(("ilspycmd",), 0, b"9.0.0", b"")))
    monkeypatch.setattr(api_rail, "run", lambda argv, **_kwargs: Ok(Completed(tuple(argv), 0, b"9.0.1", b"")))

    doctor = quality.expect(api_rail.api(settings, scope, "doctor"), api_rail.ApiDoctorReport)
    resolved = quality.expect(api_rail.api(settings, scope, "resolve", "rhino-common", kind="assembly"), api_rail.ApiQueryReport)
    shown = quality.expect(api_rail.api(settings, scope, "show", "report", show_policy="latest"), api_rail.ApiShowReport)
    assert doctor.status == "ok"
    assert resolved.status == "ok"
    assert shown.preview


def test_api_package_restore_and_empty_surface_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    scope = quality.scope("api", quality.root / ".artifacts/quality/api/test-run")
    quality.write("Directory.Packages.props", '<Project><ItemGroup><PackageVersion Include="Empty.Package" Version="1.0.0" /></ItemGroup></Project>')
    (quality.root / ".cache/nuget/packages/empty.package/1.0.0").mkdir(parents=True)
    calls: list[tuple[str, ...]] = []

    def fake_run(argv: tuple[str, ...], **_kwargs: object) -> Result[Completed, ProcessFault]:
        calls.append(tuple(argv))
        return Ok(Completed(tuple(argv), 0, b"9.0.1", b""))

    monkeypatch.setattr(api_rail, "run", fake_run)

    assert api_rail.api(quality.settings, scope, "resolve", "Empty.Package", restore="never").default_value(b"")
    assert api_rail.api(quality.settings, scope, "resolve", "Empty.Package", restore="always").is_ok()
    assert any(call[0:2] == ("dotnet", "restore") for call in calls)
    source = api_rail._source(quality.settings, scope, "Empty.Package").default_value(None)
    assert source is not None
    assert api_rail._surface(scope, source).is_error()
    assert api_rail.api(quality.settings, scope, "show", "missing-artifact").is_error()
    assert api_rail._slice_text("a\nb\nc", lines="2:3", grep="", max_lines=1) == ("b\nc", 3, True)


# --- [BRIDGE AND PACKAGE LAWS] ---------------------------------------------------------


def test_bridge_payload_verify_client_and_discovery_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    scope = quality.scope("bridge")
    scenario_dir = quality.mkdir("tests/csharp/libs/Rasm/scenarios")
    scenario = quality.write(scenario_dir.relative_to(quality.root) / "one.verify.csx")
    quality.project("libs/csharp/Rasm/Rasm.csproj")
    payload = bridge_rail.BridgeResult(
        command="check",
        status=RailStatus.FAILED,
        report_path=str(scenario),
        phases=(
            bridge_rail.BridgePhase(
                phase="execute",
                status=RailStatus.FAILED,
                data={"diagnostics": {"exceptionReports": [{"message": "boom"}]}},
                outputs=(
                    bridge_rail.BridgeOutput(
                        source="stdout",
                        text='rasm.rhino-bridge.evidence=facts={"a":1}\nrasm.rhino-bridge.capture={"path":"shot.png"}',
                        truncated=True,
                    ),
                ),
            ),
        ),
    )
    report = bridge_rail._verify_summary(quality.root, (payload,), 5.0).default_value(
        bridge_rail.VerifyReport(summary=bridge_rail.VerifySummaryCounts(0, 0, 0))
    )
    assert bridge_rail.try_decode_bridge(msgspec.json.encode(payload)).default_value(bridge_rail.BridgeResult()).exit_code == 1
    assert bridge_rail.VerifyScenario.of(payload).facts == ({"a": 1},)
    assert bridge_rail.VerifyScenario.of(payload).captures == ({"path": "shot.png"},)
    assert report.first_failure is not None
    assert (quality.root / "summary.json").is_file()
    assert bridge_rail._verify_discover(Workspace(quality.root), quality.root, str(scenario)).default_value(()) == (scenario.resolve(),)
    assert bridge_rail._verify_discover(Workspace(quality.root), quality.root, "one.verify.csx").default_value(()) == (scenario.resolve(),)
    assert bridge_rail._verify_resolve(quality.settings, Workspace(quality.root), {}, scenario).default_value(Path()).name == "Rasm.csproj"
    assert bridge_rail.BridgeResult.from_process_fault(ProcessFault.fail("bridge", detail=b"timeout", returncode=124)).status == "timeout"
    assert bridge_rail.client_run(quality.settings, scope, "doctor").is_error()
    ready = quality.settings.bridge_client.parent / "bin" / quality.settings.configuration / "net10.0"
    ready.mkdir(parents=True)
    (ready / "Rasm.RhinoBridge.Client.dll").write_text("", encoding="utf-8")
    monkeypatch.setattr(
        bridge_rail,
        "dotnet",
        lambda *args, **_kwargs: Ok(Completed(("dotnet", *args), 0, msgspec.json.encode(bridge_rail.BridgeResult(status=RailStatus.OK)), b"")),
    )
    assert bridge_rail.client_run(quality.settings, scope, "doctor").is_ok()
    assert bridge_rail.client_quit(quality.settings, scope).is_ok()
    assert bridge_rail.client_refresh(quality.settings, scope).is_ok()
    monkeypatch.setattr(bridge_rail, "with_bridge_lease", lambda _settings, action: action())
    monkeypatch.setattr(bridge_rail, "build_client", lambda *_args: Ok(None))
    monkeypatch.setattr(bridge_rail, "build_scenario_kit", lambda *_args: Ok(None))
    monkeypatch.setattr(bridge_rail, "_verify_discover", lambda *_args: Ok((scenario,)))
    monkeypatch.setattr(bridge_rail, "_verify_resolve", lambda *_args: Ok(quality.settings.bridge_client))
    monkeypatch.setattr(
        bridge_rail, "_verify_invoke", lambda *_args: bridge_rail.BridgeResult(command="check", status=RailStatus.OK, report_path=str(scenario))
    )
    assert (
        bridge_rail
        .run_verify(quality.settings, scope, "*.verify.csx")
        .default_value(bridge_rail.VerifyReport(summary=bridge_rail.VerifySummaryCounts(0, 0, 0)))
        .summary.ok
        == 1
    )


def test_package_meta_listing_validation_and_resolution_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    scope = quality.scope("package")
    shape = PackageShape()
    meta = shape.materialize(quality)
    project = meta.project
    props = shape.props(meta)
    monkeypatch.setattr(package_rail, "_package_projects", lambda _settings: Ok((project,)))
    listed = quality.expect_json(package_rail.package_list_payload(quality.settings, scope))
    assert listed["packages"] == [{"slug": shape.slug, "project": str(shape.project)}]
    assert package_rail._yak_argv(meta, "push", package_file=quality.root / "pkg.yak") == (
        str(meta.yak_path),
        "push",
        "--source",
        "feed",
        str(quality.root / "pkg.yak"),
    )
    assert package_rail.PackageMeta.from_props(project, props, quality.settings, shape.slug).is_ok()
    for patch in (
        {"YakPackageSlug": "other"},
        {"TargetExt": ".dll"},
        {"TargetDir": str(quality.root.parent / "outside")},
        {"YakPlatform": "win"},
        {"YakPackagePattern": "*.yak"},
        {"YakPath": str(quality.root / "missing-yak")},
    ):
        assert package_rail.PackageMeta.from_props(project, {**props, **patch}, quality.settings, shape.slug).is_error()
    assert package_rail.PackageMeta.from_props(project, {}, quality.settings, "slug").is_error()
    assert package_rail._finish(
        quality.settings, scope, "deploy", shape.slug, package_rail.PackageArtifact(quality.mkdir("empty-stage"), meta)
    ).is_error()
    assert package_rail.run_package_rail(quality.settings, scope, "package", "missing", "").is_error()
    duplicate = quality.project("tools/a/a.csproj", "<Project><PropertyGroup><YakPackageSlug>rasm-bridge</YakPackageSlug></PropertyGroup></Project>")
    monkeypatch.setattr(package_rail, "_package_projects", lambda _settings: Ok((project, duplicate)))
    assert package_rail.run_package_rail(quality.settings, scope, "package", "rasm-bridge", "").is_error()


def test_package_stage_run_plan_and_msbuild_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    scope = quality.scope("package")
    shape = PackageShape()
    meta = shape.materialize(quality)
    project = meta.project

    def fake_yak(argv: tuple[str, ...], **kwargs: object) -> Result[Completed, ProcessFault]:
        if argv[1] == "build":
            cwd = kwargs.get("cwd")
            ((cwd if isinstance(cwd, Path) else Path.cwd()) / shape.package_pattern).write_text("", encoding="utf-8")
        return Ok(Completed(tuple(argv), 0, b"", b""))

    monkeypatch.setattr(package_rail, "_package_projects", lambda _settings: Ok((project,)))
    monkeypatch.setattr(package_rail, "_resolve_project", lambda *_args: Ok(project))
    evaluate_meta = package_rail._evaluate_meta
    monkeypatch.setattr(package_rail, "_evaluate_meta", lambda *_args: Ok(meta))
    monkeypatch.setattr(package_rail, "dotnet_build", lambda *_args, **_kwargs: (shape.rebuild(meta), Ok(None))[1])
    monkeypatch.setattr(package_rail, "run", fake_yak)
    monkeypatch.setattr(package_rail, "with_bridge_lease", lambda _settings, action: action())
    monkeypatch.setattr(package_rail, "build_client", lambda *_args: Ok(None))
    monkeypatch.setattr(package_rail, "client_quit", lambda *_args: Ok(None))
    monkeypatch.setattr(package_rail, "client_refresh", lambda *_args: Ok(None))
    meta.package_dir.mkdir(parents=True, exist_ok=True)
    packaged = package_rail.run_package_rail(quality.settings, scope, "package", "rasm-bridge", "1.0.0")
    assert packaged.is_ok(), packaged.swap().default_value(ProcessFault.fail("package", detail=b"unknown")).message
    assert isinstance(packaged.default_value(None), package_rail.PackageArtifact)
    assert package_rail.run_package_rail(quality.settings, scope, "deploy", "other", "1.0.0").default_value(None) is None
    assert package_rail.run_package_rail(quality.settings, scope, "publish", "rasm-bridge", "1.0.0").default_value(None) is None
    plan = quality.expect_json(package_rail.package_plan_payload(quality.settings, scope, "rasm-bridge", "1.0.0"))
    assert QualityHarness.json_object(plan["meta"])["yak_platform"] == "mac"
    monkeypatch.setattr(package_rail, "_evaluate_meta", evaluate_meta)
    monkeypatch.setattr(
        package_rail,
        "run",
        lambda argv, **_kwargs: Ok(Completed(tuple(argv), 0, msgspec.json.encode(package_rail._MsbuildProps(Properties={})), b"")),
    )
    assert package_rail._evaluate_meta(quality.settings, scope, project, "slug", "1.0.0").is_error()


def test_settings_bundle_validation_laws(monkeypatch: pytest.MonkeyPatch, quality: QualityHarness) -> None:
    with pytest.raises(ValueError, match="invalid values"):
        QualitySettings(root=quality.root, rhino_app=None, configurations="Debug Nope")
    with pytest.raises(ValueError, match="not found"):
        QualitySettings._coerce_rhino_bundle(quality.root / "missing.app", label="missing")
    with pytest.raises(ValueError, match="not found"):
        quality.settings.require_rhino_app()
    assert QualitySettings._rhino_listing()
    broken = quality.mkdir("BrokenRhino.app")
    good = quality.mkdir("RhinoWIP.app")
    info = good / "Contents/Info.plist"
    info.parent.mkdir(parents=True)
    info.write_bytes(plistlib.dumps({"CFBundleVersion": "9.99.1"}))
    monkeypatch.setattr(Path, "glob", lambda self, pattern: (broken, good) if str(self) == "/Applications" and pattern == "Rhino*.app" else ())
    assert QualitySettings._newest_rhino_app() == good
    monkeypatch.setenv("RHINO_WIP_APP_PATH", str(good))
    assert QualitySettings._resolve_rhino_app() == good
