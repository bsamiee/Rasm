"""Assay suite pytest wiring: SUT registration for the law-coverage gate, fixtures, and the log_processors override.

Library payload (strategy registry, ``RailProbe``/``AssayHarness``/``BridgeResult``/``YakShape`` capsules,
psutil doubles, wire oracles) lives in ``tests.python.tools.assay.kit`` — test modules import it from there;
this module owns only what pytest itself consumes.
"""

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
    the test process; pinning each to its declared default via the testkit ``isolate`` seam keeps every ring /
    governor / resource / history law order-independent under pytest-randomly. One shared seam — the universal
    isolation the whole suite relies on (``_WRITES`` has no default and is owned per-invocation, so it is left alone).
    """
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
    """Isolated AssayHarness rooted at tmp_path via the suite's ``KitFactory[AssaySettings]`` seam.

    Returns:
        AssayHarness capsule wrapping the isolated tmp settings tree.
    """
    return AssayHarness(root=tmp_path, settings=assay_settings(tmp_path))


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
def ssh_env(monkeypatch: pytest.MonkeyPatch) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]]:
    """Socketpair-backed SSH double pinned onto the engine's connect seam — no TCP, default-lane safe.

    ``provision(SshHost())`` defers the asyncssh handshake to the factory, which runs it over an AF_UNIX
    socketpair INSIDE whichever event loop awaits it — so the engine's remote/pooling/streaming arms execute
    unchanged in their own ``anyio.run`` loop while ``_connect_once`` is the one boundary re-bound.

    Returns:
        ``Provisioned`` whose ``url`` routes the remote arm and whose factory feeds the patched connect seam.
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
