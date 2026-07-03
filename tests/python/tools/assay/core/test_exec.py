"""Exec boundary laws: argv composition, spawn arms, retry, fan-out, staging, and the Executor port.

Pure transforms use oracle laws; boundary surfaces use real subprocesses and the woven spawn seam.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import os
from pathlib import Path
import sys
import time
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
from expression import Error, Ok
from hypothesis import given, strategies as st
import msgspec
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter  # noqa: TC002  # collection-time fixture annotation
import pytest

from tests.python._testkit.spec import assert_error, assert_error_status, assert_ok, ProjectionCase, projection_matrix, validity_matrix

# Hypothesis resolves fixture annotations at collection time under PEP 649.
from tests.python.tools.assay.kit import AssayHarness  # noqa: TC001
import tools.assay.core.exec as exec_mod
from tools.assay.core.exec import argv_for, EngineExecutor, Executor, fan_out, retry_predicate, run_check, run_check_async, splice_command
import tools.assay.core.govern as govern_mod
from tools.assay.core.govern import fan_schedule, remaining, reset_foreign_census
from tools.assay.core.model import Check, Claim, Fault, Input, Language, Mode, RailStatus, receipt, Runner, Stage, Tool, ToolGroup
from tools.assay.core.routing import discover, discover_async, Routed, Scope


if TYPE_CHECKING:
    from collections.abc import Callable, Iterator, Mapping, Sequence

    from expression import Result

    from tools.assay.composition.settings import AssaySettings
    from tools.assay.composition.store import ArtifactScope
    from tools.assay.core.model import Completed


# --- [CONSTANTS] ------------------------------------------------------------------------

# fan_schedule/reset_foreign_census/remaining are the govern scheduling seams the fan and deadline laws drive.
COVERS: tuple[object, ...] = (
    argv_for, discover, discover_async, EngineExecutor, Executor, fan_out, fan_schedule, remaining,
    reset_foreign_census, retry_predicate, run_check, run_check_async, splice_command,
)  # fmt: skip

_ECHO_TOOL = Tool(
    name="test-echo",
    runner=Runner.DIRECT,
    command=("/bin/echo", "hello"),
    input=Input.NONE,
    language=Language.CSHARP,
    claim=Claim.STATIC,
    mode=Mode.CHECK,
)
_REMOTE_TOOL = Tool(name="remote", runner=Runner.DOTNET, command=("test",), input=Input.NONE, language=Language.CSHARP, claim=Claim.STATIC)
_TOOL_RUN = Tool(
    name="ilspycmd",
    runner=Runner.DOTNET,
    command=("tool", "run", "ilspycmd", "--", "-l", "cisde"),
    input=Input.NONE,
    language=Language.CSHARP,
    claim=Claim.STATIC,
    mode=Mode.QUERY,
)
_ROUTED_CHANGED = Routed(language=Language.CSHARP, scope=Scope.CHANGED)
_PY_CHANGED = Routed(language=Language.PYTHON, scope=Scope.CHANGED)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _run(check: Check, harness: AssayHarness, *, scope: ArtifactScope | None = None) -> Result[Completed, Fault]:
    """Drive ``run_check`` against the harness with the no-sink local arm as default.

    Returns:
        Completed outcome from running ``check`` over the harness settings.
    """
    return run_check(check, settings=harness.settings, scope=scope, routed=_ROUTED_CHANGED)


# --- [EXECUTOR_PORT]


def test_engine_executor_port_runs_and_fans(assay_root: AssayHarness) -> None:
    """``EngineExecutor`` satisfies the ``Executor`` protocol; ``run`` drives a real DIRECT check and ``fan`` preserves slot order."""
    port = EngineExecutor()
    assert isinstance(port, Executor)
    done = assert_ok(port.run(Check(tool=_ECHO_TOOL), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED))
    assert b"hello" in done.stdout
    first = msgspec.structs.replace(_ECHO_TOOL, name="first", command=("echo", "one"))
    second = msgspec.structs.replace(_ECHO_TOOL, name="second", command=("echo", "two"))
    rows = port.fan((Check(tool=first), Check(tool=second)), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)
    assert len(rows) == 2
    assert b"one" in assert_ok(rows[0]).stdout
    assert b"two" in assert_ok(rows[1]).stdout


@pytest.mark.anyio
async def test_run_check_async_is_the_event_loop_boundary(assay_root: AssayHarness) -> None:
    """``run_check_async`` is the public in-loop entrypoint — async callers never import the woven spawn."""
    outcome = await run_check_async(Check(tool=_ECHO_TOOL), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)
    assert b"hello" in assert_ok(outcome).stdout


# --- [SPAWN_ENV]


def test_run_check_env_laws(assay_root: AssayHarness, otel_spans: InMemorySpanExporter) -> None:
    """The spawned env carries W3C ``traceparent`` and the row-scoped ``Tool.env`` merge; a bare row leaks nothing."""
    _ = otel_spans
    carrying = msgspec.structs.replace(_ECHO_TOOL, name="env-row", command=("/usr/bin/env",), env=(("ASSAY_ROW_PROBE", "row-value"),))
    with_env = assert_ok(_run(Check(tool=carrying), assay_root)).stdout
    without_env = assert_ok(_run(Check(tool=msgspec.structs.replace(carrying, env=())), assay_root)).stdout
    assert b"traceparent=00-" in with_env, f"traceparent missing from subprocess env: {with_env[:400]!r}"
    assert b"ASSAY_ROW_PROBE=row-value" in with_env, f"row env did not reach the spawned process: {with_env[:400]!r}"
    assert b"ASSAY_ROW_PROBE=" not in without_env, f"row env leaked into a tool that declared none: {without_env[:400]!r}"


@pytest.mark.parametrize("mode", [Mode.CHECK, Mode.BUILD], ids=["run-process-arm", "open-process-arm"])
def test_spawned_children_detach_into_own_session(mode: Mode, assay_root: AssayHarness) -> None:
    """Both local spawn backends pass ``start_new_session=True``: the child leads its own process group.

    The no-orphan guarantee rests on this — group-kill reaches the whole child tree and never the engine's own group.
    """
    probe = (sys.executable, "-c", "import os; print(os.getpgid(0))")
    tool = Tool("session-law", Runner.DIRECT, probe, Input.NONE, Language.PYTHON, Claim.STATIC, mode=mode)
    done = assert_ok(_run(Check(tool=tool), assay_root))
    assert int(done.stdout.split()[0]) != os.getpgid(0), f"{mode}: child shares the engine's process group"  # ty: ignore[possibly-missing-attribute]


@pytest.mark.parametrize("mode", [Mode.CHECK, Mode.BUILD], ids=["run-process-arm", "open-process-arm"])
def test_local_spawn_arms_honor_check_cwd_and_exit_code(mode: Mode, assay_root: AssayHarness) -> None:
    """Both local spawn backends run the child in ``check.cwd`` and surface its exit code verbatim."""
    workdir = assay_root.root / "spawn-cwd-law"
    workdir.mkdir(parents=True, exist_ok=True)
    probe = (sys.executable, "-c", "import os, sys; print(os.getcwd()); sys.exit(7)")
    tool = Tool("cwd-law", Runner.DIRECT, probe, Input.NONE, Language.PYTHON, Claim.TEST, mode=mode)
    done = assert_ok(run_check(Check(tool=tool, cwd=workdir), settings=assay_root.settings, scope=None, routed=_PY_CHANGED))
    assert done.returncode == 7, f"{mode}: child exit code not preserved: {done.returncode!r}"
    reported = os.path.realpath(done.stdout.decode().strip())
    assert reported == os.path.realpath(str(workdir)), f"{mode}: child ran outside check.cwd: {reported!r}"


# --- [RETRY]


@pytest.mark.parametrize(
    "label, runner, exc, budget, expected",
    [
        ("remote-connection-reset", Runner.DOTNET, ConnectionError("reset"), None, True),
        ("remote-broken-pipe", Runner.DOTNET, BrokenPipeError("pipe"), None, True),
        ("remote-generic-oserror", Runner.DOTNET, OSError("transport"), None, True),
        ("direct-oserror-is-terminal", Runner.DIRECT, OSError("local"), None, False),
        ("missing-binary-is-capability-gap", Runner.DOTNET, FileNotFoundError("absent"), None, False),
        ("value-error-is-terminal", Runner.DOTNET, ValueError("nul"), None, False),
        ("timeout-is-terminal", Runner.DOTNET, TimeoutError("deadline"), None, False),
        ("unclassified-exception-is-terminal", Runner.DOTNET, RuntimeError("unexpected"), None, False),
        ("exhausted-budget-blocks-transport-retry", Runner.DOTNET, ConnectionError("reset"), -1.0, False),
    ],
)
def test_retry_predicate_decision_table(label: str, runner: Runner, exc: BaseException, budget: float | None, expected: bool) -> None:  # noqa: FBT001
    """``retry_predicate`` retries transport faults on non-direct runners only, and never once the deadline budget is spent."""
    check = Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=runner))
    started = None if budget is None else time.monotonic() + budget
    validity_matrix(((label, exc, expected),), retry_predicate(check, started))


def test_run_check_retries_transient_spawn_via_rail_probe(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, log_events: list[dict[str, object]]
) -> None:
    """A transient remote spawn fault is retried with receipt and telemetry evidence attributed to the tool name."""
    calls = [0]

    async def flaky(*_args: object, **_kwargs: object) -> Completed:
        await anyio.sleep(0.0)
        calls[0] += 1
        match calls[0]:
            case 1:
                raise OSError("temporary transport")
            case _:
                return receipt(("dotnet", "test"), 0)

    monkeypatch.setattr(exec_mod, "_execute", flaky)
    done = assert_ok(_run(Check(tool=_REMOTE_TOOL), assay_root))
    assert (calls[0], "retry attempts=2" in done.notes) == (2, True), f"expected one retry with attempt evidence, got {calls[0]} attempts: {done!r}"
    scheduled = tuple(e for e in log_events if e.get("event") == "stamina.retry_scheduled")
    assert scheduled, "stamina retry hook emitted no telemetry"
    assert scheduled[0].get("callable") == _REMOTE_TOOL.name, f"retry telemetry not attributed to the tool: {scheduled[0]!r}"


# --- [FAN_OUT]


def test_fan_out_preserves_order_and_backfills_timeout(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``fan_out`` preserves input order, completes fast slots, and back-fills a deadline-cancelled slot as TIMEOUT."""

    async def indexed(check: Check, *_args: object, **_kwargs: object) -> Completed:
        idx = int(check.tool.name.split("-")[1])
        await anyio.sleep(0.0 if idx < 2 else 10.0)
        return receipt((check.tool.name,), 0)

    monkeypatch.setattr(exec_mod, "_execute", indexed)
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, name=f"check-{i}", runner=Runner.DOTNET)) for i in range(3))
    results = fan_out(checks, settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED, deadline=time.monotonic() + 0.3)
    assert len(results) == 3
    assert_ok(results[0])
    assert_ok(results[1])
    assert_error_status(results[2], RailStatus.TIMEOUT)


def test_fan_out_contains_escaped_check_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """An unclassified exception escaping one check is contained to its FAULTED slot; sibling slots still complete."""

    async def volatile(check: Check, *_args: object, **_kwargs: object) -> Completed:
        await anyio.sleep(0.0)
        match check.tool.name.endswith("-1"):
            case True:
                raise RuntimeError("escaped check fault")
            case False:
                return receipt((check.tool.name,), 0)

    monkeypatch.setattr(exec_mod, "_execute", volatile)
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, name=f"contain-{i}")) for i in range(3))
    results = fan_out(checks, settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)
    assert_ok(results[0])
    fault = assert_error_status(results[1], RailStatus.FAULTED)
    assert "RuntimeError" in fault.message, f"escaped exception type lost from the contained slot: {fault!r}"
    assert_ok(results[2])


# --- [SPLICE_COMMAND_ARGV_FOR]


def test_splice_command_injects_scope_flags_for_dotnet_build_verbs(assay_root: AssayHarness) -> None:
    """``splice_command`` injects scope flags into dotnet build-graph verbs and passes everything else verbatim."""
    scope = assay_root.scope(Claim.STATIC)
    verbs = assay_root.settings.scoped_verbs
    cases: tuple[ProjectionCase[tuple[Runner, tuple[str, ...], Mode, bool]], ...] = (
        ProjectionCase(
            label="dotnet-build-splices",
            intent=(Runner.DOTNET, ("build", "Workspace.slnx"), Mode.BUILD, True),
            supported_out=(*scope.dotnet_flags,),
            oracle=None,
            unsupported_out=(),
        ),
        ProjectionCase(
            label="uv-passthrough",
            intent=(Runner.UV, ("ruff", "check", "."), Mode.CHECK, True),
            supported_out=("ruff", "check", "."),
            oracle=None,
            unsupported_out=(),
        ),
        ProjectionCase(
            label="dotnet-list-passthrough",
            intent=(Runner.DOTNET, ("test", "--list-tests"), Mode.LIST, True),
            supported_out=("test", "--list-tests"),
            oracle=None,
            unsupported_out=(),
        ),
        ProjectionCase(
            label="none-scope-passthrough",
            intent=(Runner.DOTNET, ("build", "Workspace.slnx"), Mode.BUILD, False),
            supported_out=("build", "Workspace.slnx"),
            oracle=None,
            unsupported_out=(),
        ),
    )

    def project(intent: tuple[Runner, tuple[str, ...], Mode, bool]) -> tuple[str, ...]:
        runner, command, mode, scoped = intent
        spliced = splice_command(runner, command, scope if scoped else None, verbs, mode)
        return tuple(f for f in scope.dotnet_flags if f in spliced) if runner is Runner.DOTNET and mode is Mode.BUILD and scoped else spliced

    projection_matrix(cases, project)


def test_splice_command_separator_identity_and_project_isolation(assay_root: AssayHarness) -> None:
    """Splice laws beyond the flag rows: unscoped identity, first-``--`` cut, and per-project artifact-root isolation."""
    verbs = assay_root.settings.scoped_verbs
    command = ("build", "Workspace.slnx", "--", "extra")
    assert splice_command(Runner.DOTNET, command, None, verbs, Mode.BUILD) is command, "unscoped splice must be the identity"
    scope = assay_root.scope(Claim.STATIC)
    flags = tuple(scope.dotnet_flags)
    single = splice_command(Runner.DOTNET, ("build", "App.slnx", "--", "tail-a"), scope, verbs, Mode.BUILD)
    assert single == ("build", "App.slnx", *flags, "--", "tail-a"), f"flags not spliced before the separator: {single!r}"
    double = splice_command(Runner.DOTNET, ("build", "--", "mid", "--", "tail-b"), scope, verbs, Mode.BUILD)
    assert double == ("build", *flags, "--", "mid", "--", "tail-b"), f"flags must cut at the FIRST separator: {double!r}"
    test_scope = assay_root.scope(Claim.TEST)
    project_cmd = ("test", "--minimum-expected-tests", "1", "--project", "tests/csharp/libs/Shape/Shape.Tests.csproj")
    spliced = splice_command(Runner.DOTNET, project_cmd, test_scope, verbs, Mode.RUN)
    artifact_path = spliced[spliced.index("--artifacts-path") + 1]
    assert artifact_path != test_scope.path
    assert artifact_path.endswith("/dotnet/tests__csharp__libs__Shape__Shape.Tests")


def test_argv_for_exact_argv_rows(assay_root: AssayHarness) -> None:
    """``argv_for`` composes runner prefix, spliced body, and routed tails exactly per runner/mode row.

    Rows pin UV group/project segments, MODULE prefix, DIRECT passthrough, QUERY splice bypass, routed file
    tails, and per-project dotnet scope injection after ``--project`` expansion.
    """
    settings = assay_root.settings
    uv_tool = Tool(
        "uv-argv-law", Runner.UV, ("ruff", "check"), Input.NONE, Language.PYTHON, Claim.TEST, groups=(ToolGroup.MUTATION,), stage=Stage(project=True)
    )
    uv_argv = assert_ok(argv_for(Check(tool=uv_tool), _PY_CHANGED, settings=settings, scope=None))
    assert uv_argv == ("uv", "run", "--locked", "--group", "mutation", "--project", str(settings.root), "ruff", "check"), (
        f"uv argv drifted: {uv_argv!r}"
    )
    module_tool = Tool("module-argv-law", Runner.MODULE, ("tools.py_analyzer", "check"), Input.NONE, Language.PYTHON, Claim.STATIC)
    module_argv = assert_ok(argv_for(Check(tool=module_tool), _PY_CHANGED, settings=settings, scope=None))
    assert module_argv == ("uv", "run", "--locked", "python", "-m", "tools.py_analyzer", "check"), f"module argv drifted: {module_argv!r}"
    direct = msgspec.structs.replace(uv_tool, runner=Runner.DIRECT)
    direct_argv = assert_ok(argv_for(Check(tool=direct), _PY_CHANGED, settings=settings, scope=None))
    assert direct_argv == ("ruff", "check"), f"non-UV runner leaked uv segments: {direct_argv!r}"
    scope = assay_root.scope(Claim.STATIC)
    query = Tool("query-argv-law", Runner.DOTNET, ("build", "Workspace.slnx"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.QUERY)
    query_argv = assert_ok(argv_for(Check(tool=query), _ROUTED_CHANGED, settings=settings, scope=scope))
    assert query_argv == ("dotnet", "build", "Workspace.slnx"), f"QUERY mode must never splice scope flags: {query_argv!r}"
    files_tool = msgspec.structs.replace(_ECHO_TOOL, runner=Runner.UV, command=("ruff", "check"), input=Input.FILES)
    routed_files = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=("a.py", "b.py"))
    tails = assert_ok(argv_for(Check(tool=files_tool), routed_files, settings=settings, scope=scope))
    assert tails[:2] == Runner.UV.prefix, f"runner prefix not leading argv: {tails!r}"
    assert {"ruff", "check", "a.py", "b.py"} <= set(tails), f"command body or routed tails lost in {tails!r}"
    project_tool = Tool(
        "dotnet-test", Runner.DOTNET, ("test",), Input.PROJECT, Language.CSHARP, Claim.TEST, mode=Mode.RUN, input_flag=("--project",)
    )
    routed_projects = Routed(language=Language.CSHARP, scope=Scope.FULL, projects=("tests/csharp/libs/Shape/Shape.Tests.csproj",))
    project_argv = assert_ok(argv_for(Check(tool=project_tool), routed_projects, settings=settings, scope=assay_root.scope(Claim.TEST)))
    assert project_argv[project_argv.index("--artifacts-path") + 1].endswith("/dotnet/tests__csharp__libs__Shape__Shape.Tests")
    assert project_argv[project_argv.index("--project") + 1] == "tests/csharp/libs/Shape/Shape.Tests.csproj"


# --- [APPHOST_ENV]


@pytest.fixture
def _fresh_dotnet_root() -> Iterator[None]:
    """Reset the cached dotnet-root probe around source-precedence tests."""
    exec_mod._dotnet_root.cache_clear()
    yield
    exec_mod._dotnet_root.cache_clear()


def _runtime_tree(base: Path) -> Path:
    """Materialize a valid dotnet runtime root under ``base``.

    Returns:
        The runtime root path carrying the shared framework tree.
    """
    (base / "shared" / "Microsoft.NETCore.App").mkdir(parents=True)
    return base


def _dotnet_env_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # A valid DOTNET_ROOT short-circuits before the listing probe can spawn.
    root = _runtime_tree(tmp_path / "env-root")
    mp.setenv("DOTNET_ROOT", str(root))
    mp.setattr(exec_mod, "discover", lambda *_a, **_kw: (_ for _ in ()).throw(AssertionError("probe must not run")))
    return str(root)


def _dotnet_listing_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # Invalid DOTNET_ROOT falls through to the bracketed runtime listing path.
    real = _runtime_tree(tmp_path / "real-root")
    mp.setenv("DOTNET_ROOT", str(tmp_path / "absent"))
    listing = f"Microsoft.NETCore.App 10.0.0 [{real}/shared/Microsoft.NETCore.App]\nnoise\n"
    mp.setattr(exec_mod, "discover", lambda *_a, **_kw: Ok(listing.encode()))
    return str(real)


def _dotnet_muxer_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # With no env root and a failed listing, the muxer parent is the final candidate.
    real = _runtime_tree(tmp_path / "muxer-root")
    (real / "dotnet").write_bytes(b"")
    mp.delenv("DOTNET_ROOT", raising=False)
    mp.setattr(exec_mod, "discover", lambda *_a, **_kw: Error(Fault(("dotnet", "--list-runtimes"), RailStatus.FAULTED)))
    mp.setattr(exec_mod.shutil, "which", lambda _name: str(real / "dotnet"))
    return str(real)


@pytest.mark.usefixtures("_fresh_dotnet_root")
@pytest.mark.parametrize("setup", [_dotnet_env_wins, _dotnet_listing_wins, _dotnet_muxer_wins], ids=["env", "listing", "muxer"])
def test_dotnet_root_probe_precedence(setup: Callable[[Path, pytest.MonkeyPatch], str], tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_dotnet_root`` walks env → ``--list-runtimes`` → muxer parent, taking the first that resolves a real runtime tree."""
    expected = setup(tmp_path, monkeypatch)
    assert exec_mod._dotnet_root() == expected


def test_apphost_overlay_laws(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """DOTNET ``tool run`` heads gain apphost env; other heads pass through; an unresolvable root sheds inherited DOTNET_ROOT."""
    root = _runtime_tree(tmp_path / "env-root")
    monkeypatch.setattr(exec_mod, "_dotnet_root", lambda: str(root))
    base = {"PATH": "/bin"}
    assert exec_mod._apphost(_TOOL_RUN, base) == {"PATH": "/bin", "DOTNET_ROOT": str(root), "DOTNET_MULTILEVEL_LOOKUP": "0"}
    sdk = msgspec.structs.replace(_TOOL_RUN, command=("build", "--no-restore"))
    assert exec_mod._apphost(sdk, base) is base
    assert exec_mod._apphost(_ECHO_TOOL, base) is base
    monkeypatch.setattr(exec_mod, "_dotnet_root", lambda: None)
    assert exec_mod._apphost(_TOOL_RUN, {"DOTNET_ROOT": "/nix/store/garbage", "PATH": "/bin"}) == {"PATH": "/bin"}


# --- [GUARDED_FAULT_RAILS]

# Rows: (label, command, row_timeout, check_timeout, status); message/argv evidence asserted per status arm.
_GUARDED_ROWS: tuple[tuple[str, tuple[str, ...], float | None, float | None, RailStatus], ...] = (
    ("missing-binary", ("/nonexistent/assay-guarded-binary",), None, None, RailStatus.UNSUPPORTED),
    ("row-deadline", (sys.executable, "-c", "import time; time.sleep(10)"), 0.2, None, RailStatus.TIMEOUT),
    ("check-timeout-overrides-row", (sys.executable, "-c", "import time; time.sleep(10)"), 60.0, 0.2, RailStatus.TIMEOUT),
    ("nul-in-argv", ("/bin/echo", "a\x00b"), None, None, RailStatus.FAULTED),
)


@pytest.mark.parametrize("label, command, row_timeout, check_timeout, status", _GUARDED_ROWS, ids=[c[0] for c in _GUARDED_ROWS])
def test_run_check_classifies_spawn_faults(
    label: str, command: tuple[str, ...], row_timeout: float | None, check_timeout: float | None, status: RailStatus, assay_root: AssayHarness
) -> None:
    """``run_check`` routes absent binaries, deadlines, and NUL argv to guarded fault statuses with exact, unstamped evidence.

    The check-timeout row pins ``Check.timeout`` beating a generous row timeout without a sibling row.
    """
    tool = Tool("guarded-law", Runner.DIRECT, command, Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.CHECK, timeout=row_timeout)
    check = Check(tool=tool, timeout=check_timeout)
    fault = assert_error_status(run_check(check, settings=assay_root.settings, scope=None, routed=_PY_CHANGED), status)
    assert "attempts=" not in fault.message, f"first attempt must not stamp attempt evidence: {fault.message!r}"
    match status:
        case RailStatus.TIMEOUT:
            assert fault.message == "deadline exceeded", f"deadline message not exact/unstamped: {fault.message!r}"
            assert fault.argv == assert_ok(argv_for(check, _PY_CHANGED, settings=assay_root.settings, scope=None)), (
                f"deadline fault lost its argv: {fault!r}"
            )
        case RailStatus.UNSUPPORTED:
            assert command[0] in fault.message, f"UNSUPPORTED message lost the missing binary: {fault.message!r}"
        case _:
            assert fault.message, f"{label}: fault message dropped"


# --- [STAGE_GUARD]


@pytest.mark.mutation
def test_run_check_rejects_escaping_stage_paths(assay_root: AssayHarness) -> None:
    """Stage roots and inputs are contained before any destructive materialization — escapes fault, never delete."""
    escapes: Sequence[Stage] = (Stage(root="../outside"), Stage(root=".artifacts/python/work", inputs=("../pyproject.toml",)))
    for stage in escapes:
        tool = Tool("stage-law", Runner.DIRECT, ("true",), Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.RUN, stage=stage)
        fault = assert_error(run_check(Check(tool=tool), settings=assay_root.settings, scope=None, routed=_PY_CHANGED))
        assert "unsafe stage path" in fault.message, f"escape not rejected: {fault!r}"
    assert not (assay_root.root.parent / "outside").exists()


@pytest.mark.mutation
def test_contained_verdicts_and_stage_fault_evidence(tmp_path: Path) -> None:
    """``_contained`` verdict table (unsafe / resolve-escape / safe) plus ``_copy_stage_input`` fault slots and messages.

    White-box: containment is the guard in front of destructive staging; its verdict table has no public driver.
    """
    base = tmp_path.resolve()
    root = base / "root"
    (root / "sub").mkdir(parents=True)
    outside = base / "outside"
    outside.mkdir()
    (root / "link").symlink_to(outside)
    unsafe = ("..\\evil", "a\x00b", "/abs/path", "", ".", "..", "a//b", "nested/../escape")
    for rel in unsafe:
        verdict = exec_mod._contained(root, rel)
        assert isinstance(verdict, ValueError), f"unsafe path admitted: {rel!r} -> {verdict!r}"
        assert str(verdict) == f"unsafe stage path: {rel!r}", f"unsafe message drifted: {verdict!s}"
    escaped = exec_mod._contained(root, "link/f.txt")
    assert isinstance(escaped, ValueError), f"symlink escape admitted: {escaped!r}"
    assert str(escaped) == "stage path escaped root: 'link/f.txt'", f"escape message drifted: {escaped!s}"
    safe = exec_mod._contained(root, "sub/file.txt")
    assert safe == (root / "sub" / "file.txt").resolve(), f"safe path mis-resolved: {safe!r}"
    work = base / "work"
    work.mkdir()
    missing = exec_mod._copy_stage_input(Check(tool=_ECHO_TOOL), root, work, "sub/absent.txt")
    assert missing is not None, "missing stage input did not fault"
    assert (missing.argv, missing.message) == ((_ECHO_TOOL.name, "stage", "sub/absent.txt"), "missing stage input: sub/absent.txt"), (
        f"missing-input fault evidence wrong: {missing!r}"
    )
    breach = exec_mod._copy_stage_input(Check(tool=_ECHO_TOOL), root, work, "../x")
    assert breach is not None, "escaping stage input did not fault"
    assert (breach.argv, breach.message) == ((_ECHO_TOOL.name, "stage", "../x"), "unsafe stage path: '../x'"), (
        f"escape fault evidence wrong: {breach!r}"
    )


@pytest.mark.mutation
def test_staged_tool_executes_from_materialized_worktree(assay_root: AssayHarness) -> None:
    """A staged tool copies contained inputs into the artifact worktree and EXECUTES from it, not merely copies."""
    assay_root.write("pyproject.toml", "[project]\n")
    assay_root.write("tools/assay/__init__.py", "")
    stage = Stage(root=".artifacts/python/work", inputs=("pyproject.toml", "tools/assay"))
    probe = (sys.executable, "-c", "import os; print(os.getcwd())")
    tool = Tool("stage-cwd-law", Runner.DIRECT, probe, Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.RUN, stage=stage)
    done = assert_ok(run_check(Check(tool=tool), settings=assay_root.settings, scope=None, routed=_PY_CHANGED))
    work = assay_root.root / ".artifacts/python/work"
    assert (work / "pyproject.toml").is_file(), "staged file input not copied into the worktree"
    assert (work / "tools/assay/__init__.py").is_file(), "staged directory input not copied into the worktree"
    reported = os.path.realpath(done.stdout.decode().strip())
    assert reported == os.path.realpath(str(work)), f"staged child ran outside the worktree: {reported!r}"


_LOCAL_ONLY_ROWS: tuple[tuple[str, Tool, str], ...] = (
    (
        "staged",
        Tool("stage-remote-law", Runner.UV, ("python", "--version"), Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.RUN, stage=Stage(root=".artifacts/python/work")),
        "staged tools require local execution",
    ),
    *(
        (claim.value, Tool("host-bound-remote-law", Runner.DOTNET, ("run", "--", "verify"), Input.NONE, Language.CSHARP, claim, mode=Mode.VERIFY), "host-bound tools require local execution")
        for claim in (Claim.BRIDGE, Claim.PACKAGE, Claim.PROVISION)
    ),
)  # fmt: skip


@pytest.mark.parametrize("label, tool, message", _LOCAL_ONLY_ROWS, ids=[c[0] for c in _LOCAL_ONLY_ROWS])
def test_staged_and_host_bound_tools_require_local_execution(label: str, tool: Tool, message: str, assay_root: AssayHarness) -> None:
    """Staged and host-bound tools against a remote ``exec_target`` fault ``UNSUPPORTED`` before argv composition."""
    _ = label
    remote = assay_root.remote("ssh://x@127.0.0.1:2222")
    fault = assert_error_status(run_check(Check(tool=tool), settings=remote, scope=None, routed=_PY_CHANGED), RailStatus.UNSUPPORTED)
    assert message in fault.message, f"wrong local-only message: {fault!r}"


# --- [INPROC_THUNK]


def test_inproc_thunk_outcomes(assay_root: AssayHarness) -> None:
    """``Runner.INPROC`` classifies thunk outcomes: missing and raising thunks contain to rc=1, healthy thunks pass verbatim."""
    base = Tool("inproc-law", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.CODE, mode=Mode.CHECK)

    def _raise(_check: Check) -> Completed:
        raise RuntimeError("deliberate thunk fault")

    def _good(check: Check) -> Completed:
        return receipt((check.tool.name,), 0, stdout=b"inproc-ok")

    no_thunk = assert_ok(_run(Check(tool=base), assay_root))
    raising = assert_ok(_run(Check(tool=base, paths=("p",), thunk=_raise), assay_root))
    healthy = assert_ok(_run(Check(tool=base, thunk=_good), assay_root))
    assert (no_thunk.returncode, b"no thunk" in no_thunk.stderr.lower()) == (1, True), f"missing-thunk receipt wrong: {no_thunk!r}"
    assert (raising.returncode, b"RuntimeError" in raising.stderr) == (1, True), f"raising-thunk receipt wrong: {raising!r}"
    assert (healthy.returncode, healthy.stdout) == (0, b"inproc-ok"), f"healthy-thunk receipt wrong: {healthy!r}"


# --- [BACKEND_ROUTING]


def test_run_process_backend_routes_on_exec_target(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_run_process_backend`` dispatches to ``_run_remote`` exactly when ``exec_target`` is set, forwarding declared row env."""
    recorded: list[tuple[str, dict[str, str]]] = []

    async def _record(plan: object, target: object) -> object:  # noqa: RUF029  # async to match _run_remote's awaited signature
        recorded.append((target.url, dict(getattr(plan, "env", {}))))  # ty: ignore[unresolved-attribute]  # target is the Ssh value object
        return receipt(("remote",), 0, stdout=b"recorded")

    monkeypatch.setattr(exec_mod, "run_remote", _record)
    env_tool = msgspec.structs.replace(_ECHO_TOOL, name="route-law", env=(("ASSAY_ROW_DECLARED", "row-value"),))
    monkeypatch.setenv("ASSAY_AMBIENT_UNDECLARED", "ambient-value")

    local = assert_ok(run_check(Check(tool=env_tool, cwd=assay_root.root), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED))
    assert (recorded, local.stdout) == ([], b"hello\n"), f"local target must never reach _run_remote: {recorded!r}"

    remote = assay_root.remote("ssh://x@127.0.0.1:2222")
    done = assert_ok(run_check(Check(tool=env_tool, cwd=assay_root.root), settings=remote, scope=None, routed=_ROUTED_CHANGED))
    assert (done.stdout, len(recorded)) == (b"recorded", 1), f"remote target did not route to _run_remote: {recorded!r}"
    target, plan_env = recorded[0]
    assert target == "ssh://x@127.0.0.1:2222", f"_run_remote received the wrong target: {target!r}"
    assert plan_env.get("ASSAY_ROW_DECLARED") == "row-value", f"declared row env did not reach _run_remote: {plan_env!r}"


def test_guarded_projects_argv_scope_and_governed_limiter_into_execute(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Spawn execution receives projected argv, intact scope, and governed limiter cap through both run and fan paths."""
    seen: list[tuple[object, tuple[str, ...], int]] = []

    async def capture(
        check: Check,
        settings: AssaySettings,
        scope: ArtifactScope | None,
        *,
        argv: tuple[str, ...],
        cwd: str,
        env: Mapping[str, str],
        thread_limiter: anyio.CapacityLimiter,
    ) -> Completed:
        await anyio.sleep(0.0)
        _ = (check, settings, cwd, env)
        seen.append((scope, argv, int(thread_limiter.total_tokens)))
        return receipt(argv, 0)

    monkeypatch.setattr(exec_mod, "_execute", capture)
    monkeypatch.setattr(govern_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=50.0))
    monkeypatch.setattr(govern_mod, "_foreign_dotnet_count", lambda: 0)
    settings = assay_root.settings.model_copy(update={"cpu_count": 4, "max_checks": 8, "dotnet_max_cpu": 2, "mutation_max_cpu": 8})
    scope = assay_root.scope(Claim.STATIC)
    tool = Tool("argv-proj-law", Runner.DOTNET, ("build", "Workspace.slnx"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD)
    check = Check(tool=tool)
    expected = assert_ok(argv_for(check, _ROUTED_CHANGED, settings=settings, scope=scope))
    assert set(scope.dotnet_flags) <= set(expected), "law vacuous: scope flags absent from the projected argv"
    assert_ok(run_check(check, settings=settings, scope=scope, routed=_ROUTED_CHANGED))
    assert seen[0][0] is scope, "scope did not reach _execute intact on the single-check path"
    assert (seen[0][1], seen[0][2]) == (expected, 2), f"spawned argv/limiter diverged from the projection: {seen[0]!r}"
    assert_ok(fan_out((check,), settings=settings, scope=scope, routed=_ROUTED_CHANGED)[0])
    assert seen[1][0] is scope, "scope did not reach _execute intact through the fan"
    assert (seen[1][1], seen[1][2]) == (expected, 2), f"fan argv/limiter diverged from the projection: {seen[1]!r}"


# --- [DISCOVER]


def test_discover_boundary_laws(tmp_path: Path) -> None:
    """``discover`` runs at ``root`` in a detached session; every fault carries exact argv plus a pinned message.

    Arms pin the ok/cwd path, the async wrapper parity, timeout evidence, stdout-tail nonzero evidence,
    spawn-fault evidence, and the ``start_new_session`` spawn posture.
    """
    cwd_argv = (sys.executable, "-c", "import os; print(os.getcwd())")
    here = assert_ok(discover(cwd_argv, root=tmp_path, timeout=10.0))
    assert os.path.realpath(here.decode().strip()) == os.path.realpath(str(tmp_path)), "discovery child did not run at root"
    via_async = assert_ok(anyio.run(lambda: discover_async(cwd_argv, root=tmp_path, limit_s=10.0)))
    assert os.path.realpath(via_async.decode().strip()) == os.path.realpath(str(tmp_path)), "discover_async diverged from discover"
    slow_argv = (sys.executable, "-c", "import time; time.sleep(5)")
    slow = assert_error_status(discover(slow_argv, root=tmp_path, timeout=0.2), RailStatus.TIMEOUT)
    assert (slow.argv, slow.message) == (slow_argv, "timeout after 0.2s"), f"timeout fault evidence wrong: {slow!r}"
    tail_argv = (sys.executable, "-c", "import sys; sys.stdout.write('warn-tail'); sys.exit(3)")
    tail = assert_error_status(discover(tail_argv, root=tmp_path, timeout=10.0), RailStatus.FAULTED)
    assert (tail.argv, tail.message) == (tail_argv, "warn-tail"), f"stdout-only nonzero tail lost: {tail!r}"
    absent_argv = ("/nonexistent/assay-discover-law",)
    absent = assert_error_status(discover(absent_argv, root=tmp_path, timeout=10.0), RailStatus.FAULTED)
    assert (absent.argv, bool(absent.message)) == (absent_argv, True), f"spawn fault lost its evidence: {absent!r}"
    pgid = assert_ok(discover((sys.executable, "-c", "import os; print(os.getpgid(0))"), root=tmp_path, timeout=10.0))
    assert int(pgid.split()[0]) != os.getpgid(0), "discovery child shares the engine's process group"  # ty: ignore[possibly-missing-attribute]


# --- [FAULT_DEADLINE_PROPERTY]


@given(drift=st.floats(min_value=0.05, max_value=5.0))
def test_remaining_deadline_floors_at_one_millisecond(drift: float) -> None:
    """``remaining`` floors an elapsed deadline at 1ms and passes ``None`` through as unbounded."""
    budget = remaining(time.monotonic() - drift)
    assert budget is not None and budget >= 0.001
    assert remaining(None) is None
