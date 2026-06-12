"""Assay suite pytest wiring: SUT registration for the law-coverage gate, fixtures, and the log_processors override.

Library payload (strategy registry, ``RailProbe``/``AssayHarness``/``BridgeResult``/``YakShape`` capsules,
psutil doubles, wire oracles) lives in ``tests.python.tools.assay.kit`` — test modules import it from there;
this module owns only what pytest itself consumes.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import functools
from itertools import starmap
import operator
import os
import shutil
from types import SimpleNamespace
from typing import Final, override, TYPE_CHECKING

import anyio
import pytest
from upath import UPath

from tests.python._testkit.laws import register_sut
from tests.python._testkit.runtime import REPO_ROOT
from tests.python._testkit.seams import loopback_server, SeamProbe, Sync
from tests.python.tools.assay.kit import (
    AssayHarness,
    BridgeResult,
    CliResult,
    install_cpu_double,
    RailProbe,
    read_one_envelope_from_bytes,
    SshLoopback,
    YakShape,
)
from tools.assay.composition.settings import AssaySettings


if TYPE_CHECKING:
    from collections.abc import AsyncGenerator, Awaitable, Callable, Generator
    from contextvars import ContextVar
    from pathlib import Path
    from unittest.mock import MagicMock

    from structlog.types import Processor

    from tests.python.tools.assay.kit import CpuDoubleInstaller, CpuSampler, VerbRunner
    from tools.assay.composition.settings import ArtifactStore
    from tools.assay.core.model import Envelope


# --- [CONSTANTS] ------------------------------------------------------------------------

_UV = shutil.which("uv")

# The assay SUT package whose public surface the law-coverage gate (tests.python._testkit.laws) walks.
SUT_PACKAGE: Final = "tools.assay"

# Callable/type aliases and ContextVar seams with no independently testable behavior; kept minimal.
# `Bind` exempts aspect.Bind (alias) only; model.Bind (a real binding) still earns a law in S3b.
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
    """Reset every SUT module-level ContextVar + structlog context per test — they leak in-process.

    The CLI/``@logged``/``@checked``/engine seams bind into ContextVars (aspect ``_RING``, automation
    ``_CPU_PRIMED``, engine ``_RESOURCE``/``_SSH_CACHE``) and structlog's bound context that persist within
    the test process; forcing each to its declared default at test start keeps every ring / governor /
    resource / history law order-independent under pytest-randomly. One shared seam — the universal isolation
    the whole suite relies on (``_WRITES`` has no default and is owned per-invocation, so it is left alone).
    """
    import structlog  # noqa: PLC0415

    from tools.assay.automation.engine import _CPU_PRIMED  # noqa: PLC0415
    from tools.assay.core.aspect import _RING  # noqa: PLC0415
    from tools.assay.core.engine import _RESOURCE, _SSH_CACHE  # noqa: PLC0415

    def _arm[T](var: ContextVar[T], *, default: T) -> Callable[[], None]:
        token = var.set(default)
        return lambda: var.reset(token)

    structlog.contextvars.clear_contextvars()
    undo = (_arm(_RING, default=None), _arm(_CPU_PRIMED, default=False), _arm(_RESOURCE, default=()), _arm(_SSH_CACHE, default=None))
    yield
    [reset() for reset in undo]
    structlog.contextvars.clear_contextvars()


@pytest.fixture
def assay_root(tmp_path: Path) -> AssayHarness:
    """Isolated AssayHarness rooted at tmp_path with exec_target="" and a stub Workspace.slnx.

    Returns:
        AssayHarness capsule wrapping the isolated tmp settings tree.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath(tmp_path), exec_target="", exec_known_hosts=None)
    return AssayHarness(root=tmp_path, settings=settings)


@pytest.fixture
def mem_store(assay_root: AssayHarness) -> Generator[ArtifactStore]:
    """In-memory ArtifactStore scoped to this test's run_id; removes its root on teardown.

    Yields:
        ArtifactStore backed by the memory protocol under a per-run-id root.
    """
    store = assay_root.settings.store(protocol="memory", root=f"mem-store/{assay_root.settings.run_id}")
    yield store
    store.remove_path(store.root, recursive=True) if store.exists_path(store.root) else None


@pytest.fixture
def cli(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes], monkeypatch: pytest.MonkeyPatch) -> VerbRunner:
    """Run the assay CLI and return its single stdout Envelope, exit code, and raw stdout/stderr.

    Default (``isolate=False``): drives ``main([*argv])`` in-process under ``ASSAY_ROOT=<root>``
    (~0.001s vs ~0.65s subprocess), reading the one Envelope from ``capsysbinary`` and the returned exit
    code. ``isolate=True`` spawns the real ``uv run python -m tools.assay`` subprocess via ``anyio.run``
    for the few argv/exit-code isolation laws. Synchronous: ``main`` opens its own ``anyio`` loop, so an
    async wrapper would raise "Already running asyncio". Structlog writes to stderr; one Envelope reaches
    stdout — both are preserved on the returned ``CliResult`` for channel-separation laws.

    Returns:
        Synchronous callable ``run(*argv, isolate=False, extra_env=None) -> CliResult``.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))

    def run(*argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None) -> CliResult:
        list(starmap(monkeypatch.setenv, (extra_env or {}).items()))
        match isolate:
            case False:
                from tools.assay import __main__ as main_mod  # noqa: PLC0415  # in-proc import keeps the subprocess path import-clean

                # main() shuts down the global tracer provider; in-process that tears down the session provider otel_spans reads.
                # Neutralize teardown while keeping recording.
                neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
                monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: neutralized)
                code = main_mod.main([*argv])
                cap = capsysbinary.readouterr()
                return CliResult(envelope=read_one_envelope_from_bytes(cap.out), exit_code=code, stdout=cap.out, stderr=cap.err)
            case True:
                if _UV is None:
                    pytest.skip("uv not on PATH")
                # Subprocess coverage credit: when the parent session runs under `coverage run` (COVERAGE_RUN set),
                # the child measures via site-packages' a1_coverage.pth + the parallel-mode subprocess ini.
                cov_env = {"COVERAGE_PROCESS_START": str(REPO_ROOT / ".config" / "coverage-subprocess.ini")} if "COVERAGE_RUN" in os.environ else {}  # noqa: TID251
                spawn_env = {**os.environ, "ASSAY_ROOT": str(assay_root.root), **cov_env, **(extra_env or {})}  # noqa: TID251  # subprocess env clone
                spawn = functools.partial(anyio.run_process, env=spawn_env, cwd=str(REPO_ROOT), check=False)
                result = anyio.run(spawn, ["uv", "run", "python", "-m", "tools.assay", *argv])
                return CliResult(
                    envelope=read_one_envelope_from_bytes(result.stdout), exit_code=result.returncode, stdout=result.stdout, stderr=result.stderr
                )

    return run


@pytest.fixture
def log_processors() -> tuple[Processor, ...]:
    """Specialize the testkit ``log_events`` chain with assay's ``ring_processor``.

    Overrides the project-agnostic ``_testkit.runtime.log_processors`` default (empty) so the ring
    buffer and trace-correlation processor fire while ``capture_logs`` records: a test that seeds
    ``aspect._RING`` asserts ring content and captured event dicts in one pass, with no manual
    ``_RING.set`` capture workaround.

    Returns:
        Single-element tuple binding ``aspect.ring_processor`` into the capture chain.
    """
    from tools.assay.core.aspect import ring_processor  # noqa: PLC0415  # processor imported at fixture time to keep collection import-clean

    return (ring_processor,)


@pytest.fixture
def rail_probe() -> RailProbe:
    """Fresh RailProbe for canned-seam installation and call capture in a single test.

    Returns:
        Empty RailProbe with no recorded calls or captured checks.
    """
    return RailProbe()


@pytest.fixture
def cpu_double(monkeypatch: pytest.MonkeyPatch) -> CpuDoubleInstaller:
    """CpuDoubleInstaller bound to this test's monkeypatch; call it to pin automation.engine's psutil.

    Returns:
        Callable that installs a psutil module double with a canned cpu_percent sampler.
    """

    def install(cpu_percent: CpuSampler, *, cpu_count: int = 4) -> MagicMock:
        return install_cpu_double(monkeypatch, cpu_percent, cpu_count=cpu_count)

    return install


@pytest.fixture
def captured_emits(monkeypatch: pytest.MonkeyPatch) -> list[Envelope]:
    """Redirect ``automation.engine._emit`` to a recording ``SeamProbe`` sink and yield its capture list.

    The engine's ``_emit(envelope)`` is patched to a ``Sync(None)`` seam whose ``project`` captures the sole
    positional Envelope (``args[:1]``) per call, so ``captured`` is a live ``list[Envelope]`` matching the HEAD
    semantics — a ``drive`` law asserts the exact emitted sequence without parsing NDJSON off ``capsysbinary``.

    Returns:
        The live ``list[Envelope]`` collecting each ``_emit`` call's Envelope, in emission order.
    """
    from tools.assay.automation import engine as automation_engine  # noqa: PLC0415  # patch target re-imported here

    probe: SeamProbe[Envelope] = SeamProbe(project=operator.itemgetter(slice(1)))
    probe.install(monkeypatch, automation_engine, "_emit", Sync(None))
    return probe.captured


@pytest.fixture
async def ssh_loopback(socket_enabled: None) -> AsyncGenerator[SshLoopback]:
    """Live loopback asyncssh server (network-marked): yields the ssh ``exec_target`` capsule.

    Wraps ``asyncssh.listen`` in the engine's ``loopback_server`` async-context lifecycle — no daemon threads,
    no manual teardown, no ResourceWarning under ``filterwarnings=["error"]``. ``asyncssh`` is imported lazily
    so the ~194ms import tax is paid only on network runs.

    Yields:
        ``SshLoopback`` carrying the bound port and ``ssh://x@127.0.0.1:<port>`` ``exec_target``.
    """
    _ = socket_enabled
    import asyncssh  # noqa: PLC0415  # lazy: keep the import off the non-network collection path

    class _Server(asyncssh.SSHServer):
        @override
        def begin_auth(self, username: str) -> bool:
            _ = username
            return False

    async def _handler(process: asyncssh.SSHServerProcess[str]) -> None:  # noqa: RUF029  # no await; asyncssh drives the handler synchronously
        command = process.command or ""
        process.stdout.write(f"remote-ok:{command}\n")
        process.exit(0)

    key = asyncssh.generate_private_key("ssh-ed25519")

    def _listen() -> Awaitable[asyncssh.SSHAcceptor]:
        # asyncssh.listen returns an _ACMWrapper (Awaitable, not Coroutine); SSHAcceptor is its awaited type.
        return asyncssh.listen("127.0.0.1", 0, server_host_keys=[key], server_factory=_Server, process_factory=_handler)

    def _port(server: asyncssh.SSHAcceptor) -> int:
        return server.get_port()

    # ty cannot resolve loopback_server's S typevar through @asynccontextmanager; runtime is sound.
    async with loopback_server(_listen, _port) as lb:  # ty: ignore[invalid-argument-type]
        yield SshLoopback(port=lb.port)


@pytest.fixture
def bridge_result(tmp_path: Path) -> BridgeResult:
    """BridgeResult writer rooted at tmp_path/verify for adversarial bridge-parse laws.

    Returns:
        BridgeResult capsule whose variant methods write into tmp_path/verify.
    """
    return BridgeResult(tmp_path / "verify")


@pytest.fixture
def yak_shape() -> YakShape:
    """Default YakShape materializer for package-rail laws.

    Returns:
        YakShape with default slug, project path, and assembly/framework settings.
    """
    return YakShape()
