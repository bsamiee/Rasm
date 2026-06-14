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
    from tools.assay.composition.settings import ArtifactStore, AssaySettings
    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

_UV = shutil.which("uv")

# Law coverage walks this package's public surface.
SUT_PACKAGE: Final = "tools.assay"

# Exempt aliases and ContextVar seams with no independent behavior; model.Bind remains covered.
_EXEMPT: Final = frozenset({
    "Attrs", "Bind", "Hom", "Layer", "Inversion",   # aspect: type aliases + decoration-time TypeError
    "ByteRecv", "_RESOURCE",                        # engine: Callable alias + resource ContextVar
    "InprocThunk",                                  # model: Callable alias
    "Handler",                                      # registry: Callable alias
    "Fire", "Worker", "ChangeBatch",                # automation/engine: Callable + type aliases
    "_CHECKED_LAYER", "_RING",                      # aspect: ContextVar seams (assembled layer + ring buffer)
    "_HINT_CAP", "_RESULT_CAP",                     # model: int caps; _HINT_CAP exercised via field_cap
})  # fmt: skip

register_sut(SUT_PACKAGE, exempt=_EXEMPT)

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(autouse=True)
def _isolate_sut_state() -> Generator[None]:
    """Reset SUT ContextVars and structlog context that persist across in-process tests."""
    import structlog  # noqa: PLC0415

    from tools.assay.automation.engine import _CPU_PRIMED  # noqa: PLC0415
    from tools.assay.core.aspect import _RING  # noqa: PLC0415
    from tools.assay.core.engine import _RESOURCE, _SSH_CACHE  # noqa: PLC0415

    structlog.contextvars.clear_contextvars()
    with ExitStack() as seams:
        seams.enter_context(isolate(_RING, None))
        seams.enter_context(isolate(_CPU_PRIMED, value=False))
        seams.enter_context(isolate(_RESOURCE, ()))
        seams.enter_context(isolate(_SSH_CACHE, None))
        yield
    structlog.contextvars.clear_contextvars()


@pytest.fixture
def assay_root(tmp_path: Path) -> AssayHarness:
    """Isolated harness rooted in pytest's tmp tree."""
    return AssayHarness(root=tmp_path, settings=assay_settings(tmp_path))


@pytest.fixture
def mem_store(assay_root: AssayHarness) -> Generator[ArtifactStore]:
    """In-memory artifact store under the current run id, removed on teardown."""
    store = assay_root.settings.store(protocol="memory", root=f"mem-store/{assay_root.settings.run_id}")
    yield store
    store.remove_path(store.root, recursive=True) if store.exists_path(store.root) else None


@pytest.fixture
def cli(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes], monkeypatch: pytest.MonkeyPatch) -> VerbRunner:
    """Run the CLI in-process by default, with subprocess isolation for argv and fd laws.

    In-process calls patch ``ASSAY_ROOT`` and return the decoded stdout Envelope plus raw channels. The
    subprocess arm uses the real module entrypoint when interpreter startup, fd separation, or env propagation
    is the law under test.

    Returns:
        Synchronous ``run(*argv, isolate=False, extra_env=None)`` fixture callable.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))

    def run(*argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None) -> CliResult:
        list(starmap(monkeypatch.setenv, (extra_env or {}).items()))
        match isolate:
            case False:
                from tools.assay import __main__ as main_mod  # noqa: PLC0415  # in-proc import keeps the subprocess path import-clean

                # Keep the session tracer provider alive while exercising main's drain path.
                neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
                monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
                code = main_mod.main([*argv])
                cap = capsysbinary.readouterr()
                return CliResult(envelope=read_one_envelope_from_bytes(cap.out), exit_code=code, stdout=cap.out, stderr=cap.err)
            case True:
                if _UV is None:
                    pytest.skip("uv not on PATH")
                # Coverage subprocess credit is opt-in through the parent's COVERAGE_RUN sentinel.
                cov_env = (
                    {"COVERAGE_PROCESS_START": str(REPO_ROOT / ".config" / "coverage-subprocess.ini")}
                    if "COVERAGE_RUN" in os.environ  # noqa: TID251
                    else {}
                )
                spawn_env = {**os.environ, "ASSAY_ROOT": str(assay_root.root), **cov_env, **(extra_env or {})}  # noqa: TID251  # subprocess env clone
                spawn = functools.partial(anyio.run_process, env=spawn_env, cwd=str(REPO_ROOT), check=False)
                result = anyio.run(spawn, ["uv", "run", "python", "-m", "tools.assay", *argv])
                return CliResult(
                    envelope=read_one_envelope_from_bytes(result.stdout), exit_code=result.returncode, stdout=result.stdout, stderr=result.stderr
                )

    return run


@pytest.fixture
def log_processors() -> tuple[Processor, ...]:
    """Add assay's ring processor to the project-agnostic log capture chain."""
    from tools.assay.core.aspect import ring_processor  # noqa: PLC0415  # processor imported at fixture time to keep collection import-clean

    return (ring_processor,)


@pytest.fixture
def rail_probe() -> RailProbe:
    """Fresh seam probe for one test's canned rail calls."""
    return RailProbe()


@pytest.fixture
def cpu_double(monkeypatch: pytest.MonkeyPatch) -> CpuDoubleInstaller:
    """Installer bound to this test's monkeypatch for canned psutil samples."""

    def install(cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock:
        return install_cpu_double(monkeypatch, cpu_percent, cpu_count=cpu_count)

    return install


@pytest.fixture
def captured_emits(monkeypatch: pytest.MonkeyPatch) -> list[Envelope]:
    """Capture each ``automation.engine._emit`` Envelope without parsing stdout."""
    from tools.assay.automation import engine as automation_engine  # noqa: PLC0415  # patch target re-imported here

    probe: SeamProbe[Envelope] = SeamProbe(project=operator.itemgetter(slice(1)))
    probe.install(monkeypatch, automation_engine, "_emit", Sync(None))
    return probe.captured


@pytest.fixture
def ssh_env(monkeypatch: pytest.MonkeyPatch) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]]:
    """Socketpair-backed SSH double pinned to the engine connect seam.

    Returns:
        Provisioned remote target whose factory runs the asyncssh handshake inside the awaiting loop.
    """
    from tools.assay.core import engine as engine_mod  # noqa: PLC0415  # patch target re-imported here

    provisioned = provision(SshHost())

    def _connect(target: str, settings: AssaySettings) -> Awaitable[asyncssh.SSHClientConnection]:
        _ = target, settings
        return provisioned.client_factory()

    monkeypatch.setattr(engine_mod, "_connect_once", _connect)
    return provisioned


@pytest.fixture
def bridge_result(tmp_path: Path) -> BridgeResult:
    """Bridge-result variant writer rooted under ``tmp_path / "verify"``."""
    return BridgeResult(tmp_path / "verify")


@pytest.fixture
def yak_shape() -> YakShape:
    """Default fake-yak materializer for package rail laws."""
    return YakShape()
