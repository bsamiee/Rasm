"""Assay pytest wiring: law registration, fixtures, and log processor overrides."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from contextlib import ExitStack
import functools
from itertools import starmap
import operator
import os
import shutil
from types import SimpleNamespace
from typing import Final, TYPE_CHECKING

import anyio
import pytest

from tests.python._testkit.env import provision, SshHost
from tests.python._testkit.laws import register_sut
from tests.python._testkit.runtime import isolate, REPO_ROOT
from tests.python._testkit.seams import SeamProbe, Sync
from tests.python.tools.assay.kit import (
    assay_settings,
    AssayHarness,
    BridgeResult,
    CliResult,
    install_cpu_double,
    RailProbe,
    read_one_envelope_from_bytes,
    SeamExecutor,  # noqa: TC001  # fixture-signature annotation evaluated by pytest at runtime
    YakShape,
)


if TYPE_CHECKING:
    from collections.abc import Awaitable, Generator
    from pathlib import Path
    from unittest.mock import MagicMock

    import asyncssh
    from structlog.types import Processor

    from tests.python._testkit.env import Provisioned
    from tests.python.tools.assay.kit import CpuDoubleInstaller, CpuSampler, VerbRunner
    from tools.assay.composition.settings import Ssh
    from tools.assay.composition.store import ArtifactStore
    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

_UV = shutil.which("uv")

# Law coverage walks this package's public surface.
SUT_PACKAGE: Final = "tools.assay"

# pytest-benchmark's repo-root storage fallback; the configure hook rebinds it to the canonical artifact URI.
_BENCHMARK_ROOT_DEFAULT: Final = "file://./.benchmarks"

_SUT_SEAMS: dict[str, ExitStack] = {}

# Type aliases, ContextVar seams, codecs, and caps auto-exempt by predicate; classes and callables need laws or COVERS credit.
register_sut(SUT_PACKAGE)

# --- [COMPOSITION] ----------------------------------------------------------------------


def pytest_configure(config: pytest.Config) -> None:
    """Pin benchmark storage off pytest-benchmark's repo-root default before its session builds.

    The ``--benchmark-storage`` pin rides ``addopts``; an ad-hoc ``-o addopts=`` that drops the pin but keeps the
    plugin loaded would fall back to ``file://./.benchmarks`` and litter the repo root the instant
    ``BenchmarkSession`` constructs its ``FileStorage`` (eager ``mkdir``). This conftest is auto-discovered by path
    even when ``addopts`` is empty — unlike the runtime ``-p`` plugin — so the rebind survives the escape. The
    pin is unconditional for the assay tree: pytest-benchmark registers its session ``trylast``, so this option
    write lands before it reads ``benchmark_storage``.
    """
    if hasattr(config.pluginmanager.hook, "pytest_benchmark_update_json") and config.getoption("benchmark_storage") == _BENCHMARK_ROOT_DEFAULT:
        from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI  # noqa: PLC0415  # canonical URI imported only on the ad-hoc escape path

        config.option.benchmark_storage = BENCHMARK_STORAGE_URI


def pytest_runtest_setup(item: pytest.Item) -> None:
    """Reset SUT ContextVars and structlog context before each in-process test."""
    import structlog  # noqa: PLC0415

    from tools.assay.automation.engine import _CPU_PRIMED  # noqa: PLC0415
    from tools.assay.core.aspect import RING  # noqa: PLC0415
    from tools.assay.core.govern import RESOURCE  # noqa: PLC0415
    from tools.assay.core.remote import _SSH_CACHE  # noqa: PLC0415

    structlog.contextvars.clear_contextvars()
    seams = ExitStack()
    seams.enter_context(isolate(RING, None))
    seams.enter_context(isolate(_CPU_PRIMED, value=False))
    seams.enter_context(isolate(RESOURCE, ()))
    seams.enter_context(isolate(_SSH_CACHE, None))
    _SUT_SEAMS[item.nodeid] = seams


def pytest_runtest_teardown(item: pytest.Item, nextitem: pytest.Item | None) -> None:
    """Close SUT ContextVar seams and clear structlog context after each test."""
    import structlog  # noqa: PLC0415

    _ = nextitem
    _SUT_SEAMS.pop(item.nodeid).close()
    structlog.contextvars.clear_contextvars()


@pytest.fixture
def assay_root(tmp_path: Path) -> AssayHarness:
    """Isolated harness rooted in pytest's tmp tree.

    Returns:
        Harness whose settings root at ``tmp_path``.
    """
    return AssayHarness(root=tmp_path, settings=assay_settings(tmp_path))


@pytest.fixture
def mem_store(assay_root: AssayHarness) -> Generator[ArtifactStore]:
    """In-memory artifact store under the current run id, removed on teardown.

    Yields:
        Memory-backed artifact store for the active run.
    """
    store = assay_root.settings.store(protocol="memory", root=f"mem-store/{assay_root.settings.run_id}")
    yield store
    store.remove_path(store.root, recursive=True) if store.exists_path(store.root) else None


@pytest.fixture
def cli(
    assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes], monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest
) -> VerbRunner:
    """Run the CLI in-process by default, with subprocess isolation for argv and fd laws.

    In-process calls patch ``ASSAY_ROOT`` and return the decoded stdout Envelope plus raw channels. The
    subprocess arm uses the real module entrypoint when interpreter startup, fd separation, or env propagation
    is the law under test; coverage credit flows through ``[tool.coverage.run] patch = ["subprocess"]``.

    Returns:
        Synchronous ``run(*argv, isolate=False, extra_env=None, executor=None)`` fixture callable; a canned
        executor rebuilds the dispatch app around the injected port (in-process arm only).
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))

    def run(*argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None, executor: SeamExecutor | None = None) -> CliResult:
        list(starmap(monkeypatch.setenv, (extra_env or {}).items()))
        match isolate:
            case False:
                from tools.assay import __main__ as main_mod  # noqa: PLC0415  # in-proc import keeps the subprocess path import-clean

                if executor is not None:
                    # The public injection channel: rebuild the app with the canned port; main() dispatches through the module global.
                    from tools.assay.composition.registry import build_app, REGISTRY  # noqa: PLC0415

                    monkeypatch.setattr(main_mod, "app", build_app(REGISTRY, executor=executor))
                # Keep the session tracer provider alive while exercising main's drain path.
                neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
                monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
                code = main_mod.main([*argv])
                cap = capsysbinary.readouterr()
                return CliResult(envelope=read_one_envelope_from_bytes(cap.out), exit_code=code, stdout=cap.out, stderr=cap.err)
            case True:
                assert executor is None, "cli(isolate=True) spawns the real interpreter; a canned executor cannot cross the process boundary"
                if _UV is None:
                    pytest.skip("uv not on PATH")
                if request.node.get_closest_marker("subprocess") is None:
                    pytest.fail("cli(isolate=True) requires @pytest.mark.subprocess; mutation lanes deselect via -m 'not subprocess'")
                spawn_env = {**os.environ, "ASSAY_ROOT": str(assay_root.root), **(extra_env or {})}  # noqa: TID251  # subprocess env clone
                spawn = functools.partial(anyio.run_process, env=spawn_env, cwd=str(REPO_ROOT), check=False)
                result = anyio.run(spawn, ["uv", "run", "python", "-m", "tools.assay", *argv])
                return CliResult(
                    envelope=read_one_envelope_from_bytes(result.stdout), exit_code=result.returncode, stdout=result.stdout, stderr=result.stderr
                )

    return run


@pytest.fixture
def log_processors() -> tuple[Processor, ...]:
    """Add assay's ring processor to the project-agnostic log capture chain.

    Returns:
        Processor chain extension carrying the assay ring processor.
    """
    from tools.assay.core.aspect import ring_processor  # noqa: PLC0415  # processor imported at fixture time to keep collection import-clean

    return (ring_processor,)


@pytest.fixture
def rail_probe() -> RailProbe:
    """Fresh seam probe for one test's canned rail calls.

    Returns:
        New rail seam probe.
    """
    return RailProbe()


@pytest.fixture
def cpu_double(monkeypatch: pytest.MonkeyPatch) -> CpuDoubleInstaller:
    """Installer bound to this test's monkeypatch for canned psutil samples.

    Returns:
        Installer callable that mounts a psutil double for the supplied sampler.
    """

    def install(cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock:
        return install_cpu_double(monkeypatch, cpu_percent, cpu_count=cpu_count)

    return install


@pytest.fixture
def captured_emits(monkeypatch: pytest.MonkeyPatch) -> list[Envelope]:
    """Capture each ``automation.engine._emit`` Envelope without parsing stdout.

    Returns:
        Live list accumulating every captured emit Envelope.
    """
    from tools.assay.automation import engine as automation_engine  # noqa: PLC0415  # patch target re-imported here

    probe: SeamProbe[Envelope] = SeamProbe(project=operator.itemgetter(slice(1)))
    probe.install(monkeypatch, automation_engine, "_emit", Sync(None))
    return probe.captured


@pytest.fixture
def ssh_env(monkeypatch: pytest.MonkeyPatch, tmp_path: Path) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]]:
    """Socketpair-backed SSH double pinned to the engine connect seam, sftp-capable for the offload-derived backend pull.

    Returns:
        Provisioned remote target whose factory runs the asyncssh handshake inside the awaiting loop.
    """
    from tools.assay.core import remote as remote_mod  # noqa: PLC0415  # patch target re-imported here

    # The Offload always derives an sftp backend for a remote run; the chrooted SFTP subsystem lets the scope pull resolve.
    provisioned = provision(SshHost(sftp_root=tmp_path))

    def _connect(target: Ssh) -> Awaitable[asyncssh.SSHClientConnection]:
        _ = target
        return provisioned.client_factory()

    monkeypatch.setattr(remote_mod, "_connect_once", _connect)
    return provisioned


@pytest.fixture
def bridge_result(tmp_path: Path) -> BridgeResult:
    """Bridge-result variant writer rooted under ``tmp_path / "verify"``.

    Returns:
        Variant writer for valid and adversarial bridge result files.
    """
    return BridgeResult(tmp_path / "verify")


@pytest.fixture
def yak_shape() -> YakShape:
    """Default fake-yak materializer for package rail laws.

    Returns:
        Default fake-yak/MSBuild tree materializer.
    """
    return YakShape()
