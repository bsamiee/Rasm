"""Behavior laws for the Rasm quality operator: one fake-process capsule, one real-child fixture."""

# --- [IMPORTS] ------------------------------------------------------------------------

from dataclasses import dataclass, field
from pathlib import Path
import shutil
import sys
from typing import TYPE_CHECKING

from expression import Error, Ok, Result
from hypothesis import given, settings as hyp_settings, strategies as st
from hypothesis.configuration import storage_directory
from hypothesis.database import DirectoryBasedExampleDatabase
import msgspec
import pytest

from tools.quality import __main__ as main_mod, process
from tools.quality.process import Completed, ProcessFault, Workspace
from tools.quality.rails import api as api_rail, bridge as bridge_rail, package as pkg_rail, static as static_rail, test as test_rail
from tools.quality.settings import ArtifactScope, QualitySettings


if TYPE_CHECKING:
    from collections.abc import Callable, Iterator


# --- [MODELS] ----------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class _Route:
    rc: int = 0
    out: bytes = b""
    err: bytes = b""


@dataclass(slots=True)
class _FakeProc:
    """Route table keyed on (argv[0], argv[1]); echoes the real argv into every Completed."""

    routes: dict[tuple[str, str], _Route] = field(default_factory=dict)
    default: _Route = _Route()
    seen: list[tuple[str, ...]] = field(default_factory=list)

    def on(self, head: str, sub: str, route: _Route) -> _FakeProc:
        self.routes[head, sub] = route
        return self

    def _resolve(self, argv: tuple[str, ...]) -> _Route:
        head = argv[0] if argv else ""
        sub = argv[1] if len(argv) > 1 else ""
        return self.routes.get((head, sub), self.routes.get((head, "*"), self.default))

    def run(self, argv: tuple[str, ...], *, check: bool = False, **_: object) -> Result[Completed, ProcessFault]:
        self.seen.append(tuple(argv))
        route = self._resolve(tuple(argv))
        done = Completed(argv=tuple(argv), returncode=route.rc, stdout=route.out, stderr=route.err)
        return Error(ProcessFault.fail(*argv, detail=route.err or route.out, returncode=route.rc)) if route.rc and check else Ok(done)

    def dotnet(self, *args: str, check: bool = True, **_: object) -> Result[Completed, ProcessFault]:
        return self.run(("dotnet", *args), check=check)

    def dotnet_tool(self, _scope: object, name: str, *args: str, check: bool = False, **__: object) -> Result[Completed, ProcessFault]:
        return self.run(("dotnet", "tool", "run", name, "--", *args), check=check)

    def dotnet_tool_restore(self, _scope: object) -> Result[None, ProcessFault]:
        return self.run(("dotnet", "tool", "restore"), check=True).map(lambda _: None)

    def dotnet_build(self, _settings: object, _scope: object, **__: object) -> Result[None, ProcessFault]:
        return self.run(("dotnet", "build"), check=True).map(lambda _: None)


# --- [FIXTURES] --------------------------------------------------------------------------


@pytest.fixture
def workspace(tmp_path: Path) -> Path:
    (tmp_path / "Workspace.slnx").write_text("<Solution />", encoding="utf-8")
    (tmp_path / ".config").mkdir()
    (tmp_path / ".config" / "dotnet-tools.json").write_text("{}", encoding="utf-8")
    tests = tmp_path / "tests" / "csharp" / "libs" / "Rasm"
    tests.mkdir(parents=True)
    (tests / "Rasm.Tests.csproj").write_text("<Project />", encoding="utf-8")
    return tmp_path


@pytest.fixture
def cfg(workspace: Path) -> QualitySettings:
    return QualitySettings(root=workspace, rhino_app=None)


@pytest.fixture
def scope(cfg: QualitySettings) -> Iterator[ArtifactScope]:
    with ArtifactScope.open(cfg, "test") as opened:
        yield opened


@pytest.fixture
def fake(monkeypatch: pytest.MonkeyPatch) -> _FakeProc:
    capsule = _FakeProc()
    monkeypatch.setattr(process, "run", capsule.run)
    bindings = (
        ("run", capsule.run),
        ("dotnet", capsule.dotnet),
        ("dotnet_build", capsule.dotnet_build),
        ("dotnet_tool", capsule.dotnet_tool),
        ("dotnet_tool_restore", capsule.dotnet_tool_restore),
    )
    for mod in (api_rail, bridge_rail, pkg_rail, static_rail, test_rail):
        for name, bound in bindings:
            if hasattr(mod, name):
                monkeypatch.setattr(mod, name, bound)
    return capsule


@pytest.fixture
def py(tmp_path: Path) -> Callable[..., tuple[str, ...]]:
    """Real child: emit src lines to stdout/stderr, exit with code; for genuine stream/timeout/check branches."""

    def factory(*src: str, rc: int = 0, sleep: float = 0.0, err: str = "") -> tuple[str, ...]:
        body = (
            f"import sys,time\ntime.sleep({sleep})\n"
            + "".join(f"sys.stdout.write({line!r}+chr(10));sys.stdout.flush()\n" for line in src)
            + (f"sys.stderr.write({err!r}+chr(10))\n" if err else "")
            + f"sys.exit({rc})\n"
        )
        script = tmp_path / f"child_{abs(hash((src, rc, sleep, err)))}.py"
        script.write_text(body, encoding="utf-8")
        return (sys.executable, str(script))

    return factory


def _capture(seen: list[tuple[str, ...]]) -> Callable[..., Result[Completed, ProcessFault]]:
    def runner(argv: tuple[str, ...], **_: object) -> Result[Completed, ProcessFault]:
        seen.append(tuple(argv))
        return Ok(Completed(argv=tuple(argv), returncode=0, stdout=b"", stderr=b""))

    return runner


def _run_main(monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str], cfg: QualitySettings, *argv: str) -> tuple[int, str, str]:
    monkeypatch.setattr("tools.quality.__main__.QualitySettings", lambda: cfg)
    code = main_mod.main(list(argv))
    captured = capsys.readouterr()
    return code, captured.out, captured.err


def _seed_projects(monkeypatch: pytest.MonkeyPatch, cfg: QualitySettings, changed: tuple[str, ...]) -> None:
    monkeypatch.setattr(Workspace, "projects", lambda _self, _directory=".": Ok(changed))
    monkeypatch.setattr(Workspace, "changed", lambda _self: Ok(changed))
    monkeypatch.setattr(Workspace, "index", lambda _self: Ok({(cfg.root / p).parent: cfg.root / p for p in changed}))


# --- [PARAMETRIZE TABLES] ----------------------------------------------------------------

STATIC_ROUTES = (pytest.param((), "skip", id="changed-empty-skip"), pytest.param(("libs/csharp/Rasm/Rasm.csproj",), "done", id="changed-owned-done"))

LEASE_GUARDS = (
    pytest.param("build", id="build-lock-busy"),
    pytest.param("bridge", id="bridge-lock-busy"),
    pytest.param("mutation", id="mutation-lock-busy"),
)

FORMAT_MODES = (pytest.param("fix", (), id="fix-no-report-flag"), pytest.param("report", ("--verify-no-changes",), id="report-verify-no-changes"))

STRYKER = (
    pytest.param((b"ok\n", b""), 0, True, id="clean-pass"),
    pytest.param((b"Number of tests found: 0\nNo test result reported\n", b""), 2, False, id="zero-discovery-fail"),
    pytest.param((b"boom\n", b""), 7, False, id="nonzero-fail"),
)

PACKAGE_FINISH = (
    pytest.param("package", id="package-stage-only"),
    pytest.param("deploy", id="deploy-install"),
    pytest.param("publish", id="publish-install-push"),
)

API_SHAPE = (
    pytest.param("", "index", id="empty-index"),
    pytest.param("Rhino.Geometry", "namespace", id="ns-match"),
    pytest.param("Rhino.Geometry.Mesh", "type", id="type-fqn"),
    pytest.param("Mesh.Weld", "member", id="member"),
    pytest.param("nonexistent-token", "search", id="search-fallback"),
)

VERIFY_STATUS = (
    pytest.param("ok", 0, id="ok-exit0"),
    pytest.param("skipped", 0, id="skipped-exit0"),
    pytest.param("unsupported", 3, id="unsupported-exit3"),
    pytest.param("failed", 1, id="failed-exit1"),
    pytest.param("timeout", 5, id="timeout-exit5"),
    pytest.param("busy", 5, id="busy-exit5"),
)


# --- [LAWS: PROCESS PRIMITIVES] ----------------------------------------------------------


def test_run_capture_real_child(py: Callable[..., tuple[str, ...]]) -> None:
    result = process.run(py("alpha", "beta"), mode="capture")
    assert result.is_ok()
    assert result.ok.lines() == ("alpha", "beta")


def test_run_check_nonzero_is_error(py: Callable[..., tuple[str, ...]]) -> None:
    result = process.run(py("x", rc=3, err="bad"), check=True)
    assert result.is_error()
    assert result.error.returncode == 3
    assert "bad" in result.error.message


def test_run_stream_writes_stderr_only(py: Callable[..., tuple[str, ...]], capsys: pytest.CaptureFixture[str], tmp_path: Path) -> None:
    result = process.run(py("line-out", err="line-err"), mode="stream", artifact_dir=tmp_path / "proc")
    captured = capsys.readouterr()
    assert result.is_ok()
    assert "line-out" in captured.out
    assert "line-err" in captured.err
    assert (tmp_path / "proc").exists()


def test_run_timeout_returns_124(py: Callable[..., tuple[str, ...]]) -> None:
    result = process.run(py("slow", sleep=2.0), timeout=0.2)
    assert result.is_error()
    assert result.error.returncode == 124
    assert "timed out" in result.error.message


def test_process_fault_fail_bytes_and_str() -> None:
    assert ProcessFault.fail("a", detail=b"raw").stderr == b"raw"
    assert ProcessFault.fail("a", detail="text").stderr == b"text"


def test_decode_json_str_and_error() -> None:
    ok = process.decode_json('{"Properties":{"k":"v"}}', pkg_rail._MsbuildProps)
    bad = process.decode_json(b"not-json", pkg_rail._MsbuildProps)
    assert ok.ok.Properties == {"k": "v"}
    assert bad.is_error()


def test_xml_root_handles_missing_and_bad(tmp_path: Path) -> None:
    assert process.xml_root(tmp_path / "absent.xml") is None
    bad = tmp_path / "bad.xml"
    bad.write_text("<not-closed", encoding="utf-8")
    assert process.xml_root(bad) is None


def test_dotnet_scoped_build_inserts_flags(cfg: QualitySettings, monkeypatch: pytest.MonkeyPatch) -> None:
    scope = ArtifactScope.build(cfg, "closure")
    seen: list[tuple[str, ...]] = []
    monkeypatch.setattr(process, "run", _capture(seen))
    process.dotnet("build", "x.csproj", scope=scope)
    process.dotnet("restore", "x.csproj", "--", "trailing", scope=scope)
    assert "--artifacts-path" in seen[0]
    assert seen[1][-1] == "trailing"
    assert "--artifacts-path" in seen[1]


def test_dotnet_unscoped_verb_no_flags(cfg: QualitySettings, monkeypatch: pytest.MonkeyPatch) -> None:
    scope = ArtifactScope.build(cfg, "closure")
    seen: list[tuple[str, ...]] = []
    monkeypatch.setattr(process, "run", _capture(seen))
    process.dotnet("sln", "list", scope=scope)
    assert "--artifacts-path" not in seen[0]


def test_dotnet_args_restore_and_build_shapes() -> None:
    restore = process.dotnet_args("restore", "x.csproj", disable_parallel=True)
    build = process.dotnet_args("build", "x.csproj", configuration="Debug", serial=True, quiet=True, disable_build_servers=True)
    assert restore == ("restore", "x.csproj", "--locked-mode", "--disable-parallel")
    assert "--no-restore" in build
    assert "-maxcpucount:1" in build
    assert "--disable-build-servers" in build


def test_fold_short_circuits_on_error() -> None:
    calls: list[int] = []

    def step(_acc: int, item: int) -> Result[int, ProcessFault]:
        calls.append(item)
        return Error(ProcessFault.fail("x", detail="stop")) if item == 2 else Ok(item)

    assert process.fold((1, 2, 3), 0, step).is_error()
    assert calls == [1, 2]


# --- [LAWS: LEASE / BUSY EXIT-5] ---------------------------------------------------------


def test_exclusive_lease_round_trip(cfg: QualitySettings, tmp_path: Path) -> None:
    lock = tmp_path / "x.lock"
    owner = process.lease_owner(cfg, "demo", project="p", mode="exclusive")
    with process.exclusive_lease(lock, owner):
        assert lock.read_text(encoding="utf-8").strip() == owner.strip()
    assert not lock.read_text(encoding="utf-8")


@pytest.mark.parametrize("resource", LEASE_GUARDS)
def test_leased_busy_maps_to_exit5(cfg: QualitySettings, tmp_path: Path, resource: str) -> None:
    lock = tmp_path / f"{resource}.lock"
    with process.exclusive_lease(lock, process.lease_owner(cfg, resource)):
        busy = process.leased(cfg, lock, resource, lambda: Ok(None))
    assert busy.is_error()
    assert busy.error.returncode == 5
    assert "busy" in busy.error.message


def test_leased_oserror_maps_to_fault(cfg: QualitySettings, tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    def boom(*_: object, **__: object) -> object:
        raise OSError("disk")

    monkeypatch.setattr(process, "exclusive_lease", boom)
    result = process.leased(cfg, tmp_path / "z.lock", "z", lambda: Ok(None))
    assert result.is_error()
    assert "disk" in result.error.message


# --- [LAWS: WORKSPACE GRAPH] -------------------------------------------------------------


def test_workspace_changed_dedup_sorted(cfg: QualitySettings, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(process, "run", lambda argv, **_: Ok(Completed(argv=tuple(argv), returncode=0, stdout=b"b.cs\na.cs\nb.cs\n", stderr=b"")))
    assert Workspace(cfg.root).changed().ok == ("a.cs", "b.cs")


def test_workspace_owner_single_and_none(cfg: QualitySettings) -> None:
    workspace = Workspace(cfg.root)
    proj = cfg.root / "libs" / "X" / "X.csproj"
    index = {proj.parent: proj}
    assert workspace.owner(index, cfg.root / "libs" / "X" / "a.cs").ok == proj
    assert workspace.owner(index, cfg.root / "other" / "b.cs").is_error()


def test_workspace_owner_ambiguous_ancestors_error(cfg: QualitySettings) -> None:
    # owner ranks ALL ancestor directories present in the index; >1 ancestor is a typed ambiguity Error, not nearest-wins.
    workspace = Workspace(cfg.root)
    nested = cfg.root / "a" / "b"
    index = {cfg.root / "a": cfg.root / "a" / "A.csproj", nested: nested / "B.csproj"}
    result = workspace.owner(index, nested / "c.cs")
    assert result.is_error()
    assert "found 2" in result.error.message


def test_csproj_tag_extraction(cfg: QualitySettings, tmp_path: Path) -> None:
    proj = tmp_path / "p.csproj"
    proj.write_text("<Project><PropertyGroup><YakPackageSlug>my-slug</YakPackageSlug></PropertyGroup></Project>", encoding="utf-8")
    assert Workspace(cfg.root).csproj(proj, "YakPackageSlug") == "my-slug"
    assert Workspace(cfg.root).csproj(tmp_path / "absent.csproj") is None


# --- [LAWS: STATIC RAIL] -----------------------------------------------------------------


@pytest.mark.parametrize("changed,outcome", STATIC_ROUTES)
def test_static_build_routes(
    cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc, monkeypatch: pytest.MonkeyPatch, changed: tuple[str, ...], outcome: str
) -> None:
    _seed_projects(monkeypatch, cfg, changed)
    for project in changed:
        (cfg.root / project).parent.mkdir(parents=True, exist_ok=True)
        (cfg.root / project).write_text("<Project />", encoding="utf-8")
    result = static_rail.run_static_rail(cfg, scope, "build")
    assert result.is_ok()
    assert result.ok == outcome


@pytest.mark.parametrize("mode,report_flag", FORMAT_MODES)
def test_static_format_commands_flatten(
    cfg: QualitySettings, scope: ArtifactScope, mode: static_rail.FormatMode, report_flag: tuple[str, ...]
) -> None:
    plan = static_rail.StaticPlan(scope="changed", projects=("libs/X/X.csproj",), format_groups=(("libs/X/X.csproj", ("libs/X/a.cs",)),))
    flat = tuple(token for command in static_rail._format_commands(cfg, scope, plan, mode) for token in command)
    assert "format" in flat
    assert "--severity" in flat
    assert "error" in flat
    assert all(flag in flat for flag in report_flag)


def test_static_full_trigger_skip_outcome(cfg: QualitySettings, scope: ArtifactScope) -> None:
    plan = static_rail.StaticPlan(scope="full", projects=("libs/X/X.csproj",), full_triggers=(".editorconfig",))
    assert static_rail._format_plan(cfg, scope, plan, "fix").ok == "full-trigger-skip"


def test_static_route_step_branches(cfg: QualitySettings) -> None:
    workspace = Workspace(cfg.root)
    proj = cfg.root / "libs" / "X" / "X.csproj"
    index = {proj.parent: proj}
    trigger = static_rail._route_step(static_rail._ChangedRoute(), "Directory.Build.props", index=index, workspace=workspace, root=cfg.root)
    owned = static_rail._route_step(static_rail._ChangedRoute(), "libs/X/a.cs", index=index, workspace=workspace, root=cfg.root)
    ignored = static_rail._route_step(static_rail._ChangedRoute(), "tests/tools/py_analyzer/x.cs", index=index, workspace=workspace, root=cfg.root)
    orphan = static_rail._route_step(static_rail._ChangedRoute(), "stray/a.cs", index={}, workspace=workspace, root=cfg.root)
    assert trigger.full
    assert "libs/X/X.csproj" in owned.projects
    assert ("libs/X/X.csproj", "libs/X/a.cs") in owned.format_routes
    assert ignored.projects == frozenset()
    assert orphan.full


def test_static_plan_payload_is_parseable(cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc, monkeypatch: pytest.MonkeyPatch) -> None:
    _seed_projects(monkeypatch, cfg, ())
    payload = static_rail.plan_payload(cfg, scope, ())
    report = msgspec.json.decode(payload.ok, type=static_rail.StaticPlanReport)
    assert report.scope == "changed"
    assert set(report.commands) == {"fix", "report", "build"}


# --- [LAWS: TEST RAIL] -------------------------------------------------------------------


def test_test_run_passes(cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc) -> None:
    assert test_rail.run_test_rail(cfg, scope, "run").is_ok()
    assert any(argv[:2] == ("dotnet", "test") for argv in fake.seen)


def test_failing_test_is_nonzero(cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc) -> None:
    fake.on("dotnet", "test", _Route(rc=1, err=b"assert failed"))
    result = test_rail.run_test_rail(cfg, scope, "run")
    assert result.is_error()
    assert result.error.returncode == 1


def test_test_target_conflict_rejected(cfg: QualitySettings, scope: ArtifactScope) -> None:
    result = test_rail.run_test_rail(cfg, scope, "run", all_targets=True, explicit_target=True)
    assert result.is_error()
    assert "exactly one" in result.error.message


def test_mutation_preflight_blocks_non_run(cfg: QualitySettings, scope: ArtifactScope) -> None:
    result = test_rail.run_test_rail(cfg, scope, "coverage", mutation="full")
    assert result.is_error()
    assert "test run" in result.error.message


@pytest.mark.parametrize("output,rc,expected_ok", STRYKER)
def test_mutation_result_classification(output: tuple[bytes, bytes], rc: int, expected_ok: object) -> None:
    completed = Completed(argv=("dotnet", "stryker"), returncode=rc, stdout=output[0], stderr=output[1])
    assert test_rail._mutation_result(completed).is_ok() is expected_ok


def test_list_tests_payload_filters_and_limits(cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc) -> None:
    fake.on("dotnet", "test", _Route(out=b"Alpha.One\nAlpha.Two\nBeta.Three\nnoise word\n"))
    payload = test_rail.list_tests_payload(cfg, scope, grep="alpha", limit=1)
    report = msgspec.json.decode(payload.ok, type=test_rail.TestListReport)
    assert report.count == 2
    assert report.returned == 1


def test_filter_args_dispatch() -> None:
    assert test_rail._filter_args("/query") == ("--filter-query", "/query")
    assert test_rail._filter_args("Cat=Unit") == ("--filter-trait", "Cat=Unit")
    assert test_rail._filter_args("MyTests") == ("--filter-class", "*MyTests*")
    assert test_rail._filter_args("Ns.Method") == ("--filter-method", "*Ns.Method*")
    assert test_rail._filter_args("") == ()


# --- [LAWS: BRIDGE / VERIFY] -------------------------------------------------------------


@pytest.mark.parametrize("status,exit_code", VERIFY_STATUS)
def test_bridge_result_exit_map(status: bridge_rail.BridgeStatus, exit_code: int) -> None:
    assert bridge_rail.BridgeResult(status=status).exit_code == exit_code


def test_bridge_result_from_process_fault_timeout() -> None:
    timeout = bridge_rail.BridgeResult.from_process_fault(ProcessFault.fail("x", detail="t", returncode=124))
    failed = bridge_rail.BridgeResult.from_process_fault(ProcessFault.fail("x", detail="f", returncode=1))
    assert timeout.status == "timeout"
    assert failed.status == "failed"


def test_bridge_facts_and_captures_decoded() -> None:
    stdout = 'noise\nrasm.rhino-bridge.evidence=facts={"area": 42}\nrasm.rhino-bridge.capture={"path": "/img.png"}\n'
    result = bridge_rail.BridgeResult(
        status="ok",
        phases=(bridge_rail.BridgePhase(phase="execute", status="ok", outputs=(bridge_rail.BridgeOutput(source="stdout", text=stdout),)),),
    )
    assert result.facts == ({"area": 42},)
    assert result.captures == ({"path": "/img.png"},)


def test_skipped_scenario_yields_exit0(scope: ArtifactScope, workspace: Path) -> None:
    skipped = bridge_rail.BridgeResult(status="skipped", report_path=str(workspace / "s.json"))
    report = bridge_rail._verify_summary(scope.path, (skipped,), 300.0)
    assert report.ok.summary.failed == 0
    assert report.ok.failed == 0


def test_verify_no_scenarios_is_error(cfg: QualitySettings, scope: ArtifactScope, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(bridge_rail, "_verify_discover", lambda *_a, **_k: Ok(()))
    result = bridge_rail._run_verify_unlocked(cfg, scope, "none")
    assert result.is_error()
    assert "No *.verify.csx" in result.error.message


def test_client_run_unbuilt_is_error(cfg: QualitySettings, scope: ArtifactScope) -> None:
    result = bridge_rail.client_run(cfg, scope, "doctor")
    assert result.is_error()
    assert "not built" in result.error.message


def test_try_decode_bridge_missing_key_is_parseable() -> None:
    empty = bridge_rail.try_decode_bridge(b"{}")
    blank = bridge_rail.try_decode_bridge(b"")
    assert empty.is_ok()
    assert empty.ok.status == "failed"
    assert blank.is_error()


# --- [LAWS: PACKAGE RAIL] ----------------------------------------------------------------


@pytest.mark.parametrize("mode", PACKAGE_FINISH)
def test_package_finish_steps(cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc, tmp_path: Path, mode: pkg_rail.PackageMode) -> None:
    stage = tmp_path / "stage"
    stage.mkdir()
    pattern = "rasm-rh9_0-mac.yak"
    (stage / pattern).write_bytes(b"yak")
    yak = tmp_path / "yak"
    yak.write_text("#!/bin/sh\n", encoding="utf-8")
    yak.chmod(0o700)
    meta = pkg_rail.PackageMeta(
        project=tmp_path / "p.csproj",
        manifest_dir=tmp_path,
        target_dir=tmp_path / "bin",
        assembly_name="P",
        target_ext=".rhp",
        yak_path=yak,
        yak_platform="mac",
        yak_push_source="",
        package_dir=stage,
        package_pattern=pattern,
    )
    artifact = pkg_rail.PackageArtifact(stage=stage, meta=meta)
    result = pkg_rail._finish(cfg, scope, mode, "some-slug", artifact)
    assert result.is_ok()
    if mode == "package":
        assert result.ok is artifact
    if mode in {"deploy", "publish"}:
        assert any(argv[0] == str(yak) and argv[1] == "install" for argv in fake.seen)
    if mode == "publish":
        assert any(argv[1] == "push" for argv in fake.seen if argv[0] == str(yak))


def test_package_meta_missing_props_is_error() -> None:
    result = pkg_rail.PackageMeta.from_props(Path("p.csproj"), {}, QualitySettings(rhino_app=None), "slug")
    assert result.is_error()
    assert "Missing MSBuild" in result.error.message


def test_package_yak_argv_shapes(tmp_path: Path) -> None:
    yak = tmp_path / "yak"
    meta = pkg_rail.PackageMeta(
        project=Path(),
        manifest_dir=tmp_path,
        target_dir=tmp_path,
        assembly_name="P",
        target_ext=".rhp",
        yak_path=yak,
        yak_platform="mac",
        yak_push_source="https://src",
        package_dir=tmp_path,
        package_pattern="*.yak",
    )
    build = pkg_rail._yak_argv(meta, "build", version="1.2.3")
    push = pkg_rail._yak_argv(meta, "push", package_file=tmp_path / "x.yak")
    assert build == (str(yak), "build", "--platform", "mac", "--version", "1.2.3")
    assert "--source" in push
    assert "https://src" in push


def test_package_list_payload_parseable(cfg: QualitySettings, scope: ArtifactScope, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(pkg_rail, "_package_projects", lambda _s: Ok(()))
    payload = pkg_rail.package_list_payload(cfg, scope)
    report = msgspec.json.decode(payload.ok, type=pkg_rail.PackageListReport)
    assert report.status == "ok"
    assert report.count == 0


# --- [LAWS: API RAIL] --------------------------------------------------------------------


@pytest.mark.parametrize("symbol,shape", API_SHAPE)
def test_api_resolve_symbol_shapes(symbol: str, shape: str) -> None:
    surface = api_rail._Surface(
        types=("Rhino.Geometry.Mesh", "Rhino.Geometry.Point3d"),
        namespaces=("Rhino.Geometry",),
        by_namespace={"Rhino.Geometry": ("Rhino.Geometry.Mesh", "Rhino.Geometry.Point3d")},
        artifact=Path("surface.txt"),
    )
    assert api_rail._resolve_symbol(surface, symbol).shape == shape


def test_api_query_missing_key_is_parseable_error(cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc) -> None:
    result = api_rail.api(cfg, scope, "query", key="no-such-package", symbol="X")
    assert result.is_error()
    assert "Unknown API source" in result.error.message


def test_api_nuget_fuzzy_resolution(cfg: QualitySettings, scope: ArtifactScope, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(api_rail, "_packages", lambda _s: {"LanguageExt.Core": "5.0.0", "Avalonia.Controls.DataGrid": "11.0.0"})
    exact = api_rail._nuget_source(cfg, scope, "languageext")
    ambiguous = api_rail._nuget_source(cfg, scope, "a")
    assert exact.ok.package == "LanguageExt.Core"
    assert ambiguous.is_error()
    assert "Ambiguous" in ambiguous.error.message


def test_api_resolve_report_il_and_resources(scope: ArtifactScope, tmp_path: Path) -> None:
    dll = tmp_path / "Lib.dll"
    dll.write_bytes(b"MZ")
    source = api_rail._ApiSource(key="lib", kind="package", assembly=dll, xml=None, assemblies=(dll,), package="Lib", version="1.0.0")
    report = msgspec.json.decode(api_rail._resolve_report(scope.path / "resolve", source, "assembly"), type=api_rail.ApiQueryReport)
    assert report.status == "ok"
    assert report.counts["paths"] == 1


def test_api_doctor_parseable_without_rhino(cfg: QualitySettings, scope: ArtifactScope, fake: _FakeProc) -> None:
    fake.on("dotnet", "tool", _Route(rc=1, err=b"unavailable"))
    payload = api_rail._doctor(cfg, scope, scope.path / "doctor", None)
    report = msgspec.json.decode(payload.ok, type=api_rail.ApiDoctorReport)
    assert report.status == "failed"
    assert report.rhino["status"] == "missing"


def test_api_slice_text_windows() -> None:
    text = "\n".join(f"line{n}" for n in range(20))
    window, total, truncated = api_rail._slice_text(text, lines="2:4", grep="", max_lines=5)
    assert window == "line1\nline2\nline3"
    assert total == 20
    assert truncated


# --- [LAWS: SETTINGS] --------------------------------------------------------------------


def test_settings_anchor_finds_marker(workspace: Path) -> None:
    nested = workspace / "a" / "b"
    nested.mkdir(parents=True)
    assert QualitySettings.anchor(nested) == workspace


def test_settings_static_configurations_dispatch(cfg: QualitySettings) -> None:
    assert cfg.static_configurations("changed") == ("Debug",)
    assert cfg.static_configurations("full") == ("Debug", "Release")
    assert cfg.model_copy(update={"configurations": "Release"}).static_configurations("changed") == ("Release",)


def test_settings_invalid_configurations_raises() -> None:
    with pytest.raises(ValueError, match="invalid values"):
        QualitySettings(configurations="Banana", rhino_app=None)


def test_settings_version_props(cfg: QualitySettings) -> None:
    assert cfg.version_props("1.0") == ("/p:Version=1.0", "/p:InformationalVersion=1.0")
    assert cfg.version_props() == ()


def test_artifactscope_flags_side_effect_free(cfg: QualitySettings) -> None:
    scope = ArtifactScope.build(cfg, "closure")
    flags_a = scope.dotnet_flags
    flags_b = scope.dotnet_flags
    assert flags_a == flags_b
    assert flags_a[0] == "--artifacts-path"
    assert scope.artifacts_property == ("-p:ArtifactsPath=" + str(scope.path),)


def test_hypothesis_profile_uses_repo_cache(hypothesis_home_dir: Path, hypothesis_examples_dir: Path) -> None:
    assert hyp_settings.default is not None
    database = hyp_settings.default.database
    assert isinstance(database, DirectoryBasedExampleDatabase)
    assert database.path == hypothesis_examples_dir
    assert storage_directory("constants", intent_to_write=False).path == hypothesis_home_dir / "constants"
    database.save(b"rasm-quality-profile", b"probe")
    assert hypothesis_examples_dir.is_dir()


# --- [LAWS: CLI ENTRYPOINT — EMIT + STRUCTLOG-TO-STDERR] ---------------------------------


def test_main_static_skip_exit0(cfg: QualitySettings, fake: _FakeProc, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]) -> None:
    _seed_projects(monkeypatch, cfg, ())
    code, _out, err = _run_main(monkeypatch, capsys, cfg, "static", "build")
    assert code == 0
    assert "phase" in err


def test_main_test_run_emits_on_stderr_only(
    cfg: QualitySettings, fake: _FakeProc, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]
) -> None:
    code, out, err = _run_main(monkeypatch, capsys, cfg, "test", "run")
    assert code == 0
    assert "test" in err
    assert not out


def test_main_failing_test_nonzero(
    cfg: QualitySettings, fake: _FakeProc, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]
) -> None:
    fake.on("dotnet", "test", _Route(rc=1, err=b"fail"))
    code, _out, err = _run_main(monkeypatch, capsys, cfg, "test", "run")
    assert code == 1
    assert "failed" in err


def test_main_api_query_missing_key_parseable(
    cfg: QualitySettings, fake: _FakeProc, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]
) -> None:
    code, _out, err = _run_main(monkeypatch, capsys, cfg, "api", "query", "no-such-key", "X")
    assert code == 1
    assert "Unknown API source" in err


def test_main_static_plan_payload_on_stdout(
    cfg: QualitySettings, fake: _FakeProc, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]
) -> None:
    _seed_projects(monkeypatch, cfg, ())
    code, out, _err = _run_main(monkeypatch, capsys, cfg, "static", "plan")
    assert code == 0
    assert msgspec.json.decode(out.encode(), type=static_rail.StaticPlanReport).scope == "changed"


def test_main_self_test_passes(cfg: QualitySettings, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]) -> None:
    monkeypatch.setattr(shutil, "which", lambda _cmd: "/usr/bin/x")
    monkeypatch.setattr("tools.quality.__main__.QualitySettings", lambda: cfg)
    code = main_mod.self_test_cmd()
    assert code == 0
    assert "passed" in capsys.readouterr().out


def test_emit_modes() -> None:
    assert main_mod._emit(b"bytes") == 0
    assert main_mod._emit("text", 3) == 3
    assert main_mod._emit(None) == 0


def test_emit_api_strict_status() -> None:
    assert main_mod._emit_api(msgspec.json.encode({"status": "failed"}), strict=True) == 2
    assert main_mod._emit_api(msgspec.json.encode({"status": "ok"}), strict=True) == 0
    assert main_mod._emit_api(None) == 0


# --- [PROPERTY LAWS] ---------------------------------------------------------------------


@given(value=st.sampled_from(["Debug Release", "Release", "Debug", ""]))
@hyp_settings(max_examples=25, deadline=None)
def test_env_override_static_configurations(value: str) -> None:
    cfg = QualitySettings(configurations=value or None, rhino_app=None)
    result = cfg.static_configurations("changed")
    assert all(item in {"Debug", "Release"} for item in result)
    assert len(result) >= 1


@given(items=st.lists(st.integers(min_value=0, max_value=5), max_size=6))
@hyp_settings(max_examples=40, deadline=None)
def test_fold_associativity(items: list[int]) -> None:
    folded = process.fold(tuple(items), 0, lambda acc, item: Ok(acc + item))
    assert folded.ok == sum(items)


@given(leaf=st.text(alphabet="abc", min_size=1, max_size=3), depth=st.integers(min_value=1, max_value=4))
@hyp_settings(max_examples=40, deadline=None)
def test_owner_ranking_unique_ancestor(leaf: str, depth: int, tmp_path: Path) -> None:
    # Exactly one indexed directory on the ancestry chain resolves unambiguously regardless of nesting depth.
    parts = [f"d{n}" for n in range(depth)]
    owner_dir = tmp_path.joinpath(*parts)
    index = {owner_dir: owner_dir / "P.csproj"}
    result = Workspace(tmp_path).owner(index, owner_dir / f"{leaf}.cs")
    assert result.ok == owner_dir / "P.csproj"


@given(key=st.sampled_from(["languageext", "language", "LANGUAGEEXT.CORE", "ext.core", "lang ext"]))
@hyp_settings(max_examples=25, deadline=None)
def test_fuzzy_key_resolution(key: str, cfg: QualitySettings, scope: ArtifactScope, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(api_rail, "_packages", lambda _s: {"LanguageExt.Core": "5.0.0"})
    assert api_rail._nuget_source(cfg, scope, key).ok.package == "LanguageExt.Core"


@given(projects=st.lists(st.text(alphabet="abcd", min_size=1, max_size=8).map(lambda s: f"libs/{s}.csproj"), min_size=1, max_size=5, unique=True))
@hyp_settings(max_examples=30, deadline=None)
def test_closure_key_stability(projects: list[str]) -> None:
    plan = static_rail.StaticPlan(scope="changed", projects=tuple(projects))
    reversed_plan = static_rail.StaticPlan(scope="changed", projects=tuple(reversed(projects)))
    assert static_rail._closure_key(plan, "changed") == static_rail._closure_key(reversed_plan, "changed")
    assert static_rail._closure_key(plan, "full") == "solution"
