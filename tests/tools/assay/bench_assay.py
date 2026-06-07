"""Polymorphic Assay perf-law suite: engine fan-out, routing, fold, storage, registry, automation, CLI.

Perf mandate: fold(1000 Completed) mean < 1ms; routing place() over changed .py paths sub-millisecond;
storage write/read round-trips bounded payloads through fsspec; CLI cold start stays an executable boundary.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from dataclasses import dataclass
from itertools import count
import sys
from typing import Final, overload, Protocol, TYPE_CHECKING

import anyio
import anyio.lowlevel
from expression import Ok
import msgspec
import psutil
import pytest

from tools.assay.automation import engine as automation_engine
from tools.assay.automation.engine import _debounce, _fire, _hardened_fire  # noqa: PLC2701  # benchmarking automation engine internals
from tools.assay.automation.model import Program
from tools.assay.composition.registry import rail, REGISTRY
from tools.assay.core.engine import exclusive_lease, fan_out, leased, run_check
from tools.assay.core.model import Check, Claim, Envelope, envelope, fold, Input, Language, Mode, receipt, Runner, Tool
from tools.assay.core.routing import place, Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.tools.assay.conftest import AssayHarness, VerbRunner
    from tools.assay.automation.engine import ChangeBatch
    from tools.assay.composition.settings import ArtifactStore
    from tools.assay.core.model import Completed, Fault, Report


# --- [TYPES] ----------------------------------------------------------------------------


class BenchmarkFixture(Protocol):
    """Typed surface for pytest-benchmark's runtime fixture; extra_info persists to .artifacts/python/benchmarks."""

    extra_info: dict[str, object]

    @overload
    def pedantic[T](self, target: Callable[[], T], *, rounds: int, warmup_rounds: int) -> T: ...
    @overload
    def pedantic[T](self, target: Callable[..., T], *, args: tuple[object, ...], rounds: int, warmup_rounds: int) -> T: ...
    @overload
    def pedantic[T](self, target: Callable[..., T], *, args: tuple[object, ...], kwargs: dict[str, object], rounds: int, warmup_rounds: int) -> T: ...
    @property
    def stats(self) -> dict[str, float]: ...


@dataclass(frozen=True, slots=True)
class BenchMeta:
    """Resource + status snapshot of an Envelope-producing benchmark, persisted to ``extra_info``."""

    status: RailStatus | None = None
    rows: int = 0
    truncated: bool = False
    artifact_bytes: int = 0
    rss_delta: int = 0
    cpu_delta: float = 0.0

    @staticmethod
    def record(env: Envelope, *, rss_before: int, cpu_before: float) -> BenchMeta:
        """Capture status/rows/artifact bytes plus RSS and CPU deltas around one Envelope.

        Returns:
            Recorded benchmark metadata for ``benchmark.extra_info``.
        """
        proc = psutil.Process()
        report = env.report
        return BenchMeta(
            status=env.status,
            rows=report.counts.total if report else 0,
            truncated=env.truncated,
            artifact_bytes=sum(a.bytes for a in (report.artifacts if report else ())),
            rss_delta=proc.memory_info().rss - rss_before,
            cpu_delta=sum(proc.cpu_times()[:2]) - cpu_before,
        )


# --- [CONSTANTS] -----------------------------------------------------------------------

pytestmark = pytest.mark.benchmark

_FANOUT_SIZES: Final = (1, 2, 4, 8, 16)
_FOLD_BUDGET_MS: Final = 1.0  # calibrate to CI hardware
_PROTOCOLS: Final = ("file", "memory")
_PAYLOAD: Final = b"x" * 4096
_NOOP_TOOL: Final = Tool(
    name="bench-noop",
    runner=Runner.DIRECT,
    command=(sys.executable, "-c", ""),
    input=Input.NONE,
    language=Language.PYTHON,
    claim=Claim.STATIC,
    mode=Mode.CHECK,
)
_ECHO_TOOL: Final = Tool(
    name="bench-echo",
    runner=Runner.DIRECT,
    command=("/bin/echo", "hello"),
    input=Input.NONE,
    language=Language.CSHARP,
    claim=Claim.STATIC,
    mode=Mode.CHECK,
)
_PY_TOOL: Final = Tool(
    name="py-check", runner=Runner.UV, command=("ruff", "check"), input=Input.FILES, language=Language.PYTHON, claim=Claim.CODE, mode=Mode.CHECK
)
_ROUTED: Final = Routed(language=Language.PYTHON, scope=Scope.FULL)
_SMALL: Final = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=tuple(f"pkg/mod_{i}.py" for i in range(10)))
_THOUSAND_OK: Final = tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(1000))
_HUNDRED_OK: Final = tuple(receipt(("t",), 0, status=RailStatus.OK) for _ in range(100))


# --- [BENCHMARKS] ----------------------------------------------------------------------


@pytest.mark.parametrize("n", _FANOUT_SIZES)
def bench_fan_out_concurrency(benchmark: BenchmarkFixture, assay_root: AssayHarness, n: int) -> None:
    """Fan-out preserves order while scaling a noop-process batch across the concurrency sweep (bench_engine)."""
    checks = tuple(Check(tool=_NOOP_TOOL) for _ in range(n))
    results: tuple[Result[Completed, Fault], ...] = benchmark.pedantic(
        fan_out, args=(checks,), kwargs={"settings": assay_root.settings, "scope": None, "routed": _ROUTED}, rounds=max(1, 20 // n), warmup_rounds=1
    )
    assert len(results) == n
    assert all(r.is_ok() for r in results)
    benchmark.extra_info.update(n=n, ok=sum(r.is_ok() for r in results))


def bench_local_engine_spawn(benchmark: BenchmarkFixture, assay_root: AssayHarness) -> None:
    """Local engine spawn completes on the Completed channel for one process exit (bench_engine)."""
    check = Check(tool=_NOOP_TOOL)
    result: Result[Completed, Fault] = benchmark.pedantic(
        run_check, args=(check,), kwargs={"settings": assay_root.settings, "scope": None, "routed": _ROUTED}, rounds=10, warmup_rounds=2
    )
    assert result.is_ok()


def bench_lock_contention_busy(benchmark: BenchmarkFixture, assay_root: AssayHarness) -> None:
    """Busy leases fail fast when another holder owns the resource (bench_engine)."""
    action: Callable[[object], Result[str, Fault]] = lambda _held: Ok("acquired")  # noqa: E731
    with exclusive_lease("bench-lock", "holder", settings=assay_root.settings):
        result: Result[str, Fault] = benchmark.pedantic(
            leased, args=("bench-lock", action), kwargs={"settings": assay_root.settings, "run_id": "blocked"}, rounds=100, warmup_rounds=5
        )
    assert result.is_error()
    assert result.error.status is RailStatus.BUSY


def bench_routing_changed_paths(benchmark: BenchmarkFixture, assay_root: AssayHarness) -> None:
    """place() over changed .py paths is sub-millisecond — the changed-path hot path (bench_routing)."""
    result = benchmark.pedantic(place, args=(_SMALL, _PY_TOOL), kwargs={"settings": assay_root.settings}, rounds=200, warmup_rounds=10)
    assert result == (_SMALL.files,)


def bench_fold_throughput(benchmark: BenchmarkFixture) -> None:
    """fold(1000 Completed) mean < _FOLD_BUDGET_MS — O(N) fold is the core hot path (bench_model)."""
    result: Report = benchmark.pedantic(fold, args=(Claim.STATIC, "check", _THOUSAND_OK), rounds=100, warmup_rounds=5)
    assert result.status is RailStatus.OK
    assert benchmark.stats["mean"] * 1e3 < _FOLD_BUDGET_MS


def bench_envelope_encode(benchmark: BenchmarkFixture) -> None:
    """Msgspec deterministic encoder frames a 100-row Report Envelope; len > 0 guards silent empty-encode (bench_model)."""
    proc = psutil.Process()
    rss_before, cpu_before = proc.memory_info().rss, sum(proc.cpu_times()[:2])
    env = envelope(fold(Claim.STATIC, "check", _HUNDRED_OK), claim=Claim.STATIC, verb="check")
    result: bytes = benchmark.pedantic(_ENCODER.encode, args=(env,), rounds=500, warmup_rounds=10)
    assert len(result) > 0
    meta = BenchMeta.record(env, rss_before=rss_before, cpu_before=cpu_before)
    benchmark.extra_info.update(rows=meta.rows, status=str(meta.status), artifact_bytes=meta.artifact_bytes)


@pytest.mark.parametrize("protocol", _PROTOCOLS)
def bench_artifact_store_history(benchmark: BenchmarkFixture, assay_root: AssayHarness, protocol: str) -> None:
    """ArtifactStore writes and reads a bounded history payload through both fsspec backends (bench_storage)."""
    store = _store(assay_root, protocol)
    keys = count()
    write_read: Callable[[], bytes] = lambda: store.read_path(store.write_bytes(_PAYLOAD, "history", f"{next(keys)}.json"))  # noqa: E731
    assert benchmark.pedantic(write_read, rounds=100, warmup_rounds=5) == _PAYLOAD


@pytest.mark.parametrize("protocol", _PROTOCOLS)
def bench_artifact_store_full_report(benchmark: BenchmarkFixture, assay_root: AssayHarness, protocol: str) -> None:
    """ArtifactStore round-trips a full 100-row Report Envelope through both fsspec backends (bench_storage)."""
    store = _store(assay_root, protocol)
    payload = _ENCODER.encode(envelope(fold(Claim.STATIC, "check", _HUNDRED_OK), claim=Claim.STATIC, verb="check"))
    keys = count()
    write_read: Callable[[], bytes] = lambda: store.read_path(store.write_bytes(payload, "benchmark", f"{next(keys)}.json"))  # noqa: E731
    assert benchmark.pedantic(write_read, rounds=100, warmup_rounds=5) == payload


def bench_registry_static_plan(benchmark: BenchmarkFixture, assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """In-process registry rail invocation stays fast independent of CLI parsing (bench_registry)."""
    bind = next(row for row in REGISTRY if row.claim is Claim.STATIC and row.verb == "plan")
    runner = rail(bind, assay_root.settings)

    def invoke() -> Envelope:
        env = runner(bind.params())
        rows = capsysbinary.readouterr().out.splitlines()
        assert len(rows) == 1
        assert msgspec.json.decode(rows[0], type=Envelope) == env
        return env

    env = benchmark.pedantic(invoke, rounds=10, warmup_rounds=2)
    assert env.status in RailStatus


def bench_automation_program_fire(benchmark: BenchmarkFixture, assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Automation program fire includes engine dispatch and NDJSON Envelope framing (bench_automation)."""
    fire = _fire(Program(argv=(sys.executable, "-c", "")), assay_root.settings, limiter=anyio.CapacityLimiter(1), cpu_threshold=None)

    async def run() -> bytes:
        await fire(())
        rows = capsysbinary.readouterr().out.splitlines()
        assert len(rows) == 1
        return rows[0]

    assert benchmark.pedantic(lambda: anyio.run(run), rounds=5, warmup_rounds=1)


def bench_automation_hardened_fire(benchmark: BenchmarkFixture, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Hardened fire benchmarks scheduling jitter separately from direct dispatch (bench_automation)."""
    monkeypatch.setattr(automation_engine, "_JITTER_MS", 1)

    async def leaf(_changes: ChangeBatch) -> None:
        await anyio.lowlevel.checkpoint()

    hardened = _hardened_fire(leaf, assay_root.settings)
    assert benchmark.pedantic(lambda: anyio.run(hardened, ()), rounds=10, warmup_rounds=2) is None


def bench_automation_debounce_path(benchmark: BenchmarkFixture) -> None:
    """Debounce coalesces a burst through the channel-backed action path used by watch triggers (bench_automation)."""
    calls = [0]

    async def leaf(_changes: ChangeBatch) -> None:
        await anyio.lowlevel.checkpoint()
        calls[0] += 1

    async def run() -> int:
        calls[0] = 0
        fire, worker = _debounce(leaf, 1, collapse=True)
        async with anyio.create_task_group() as tg:
            tg.start_soon(worker)
            await fire(())
            await fire(())
            await anyio.sleep(0.01)
            tg.cancel_scope.cancel()
        return calls[0]

    assert benchmark.pedantic(lambda: anyio.run(run), rounds=20, warmup_rounds=2) == 1


def bench_cli_inprocess(benchmark: BenchmarkFixture, cli: VerbRunner) -> None:
    """In-process CLI invocation keeps the registry path fast while preserving one Envelope stdout (bench_cli)."""
    result = benchmark.pedantic(lambda: cli("self-test"), rounds=10, warmup_rounds=2)
    assert result.exit_code in {0, 1}
    assert isinstance(result.envelope.status, RailStatus)


def bench_cli_cold_start(benchmark: BenchmarkFixture, cli: VerbRunner) -> None:
    """Cold subprocess CLI invocation stays an executable command boundary; rounds=3 keeps stddev defined (bench_cli)."""
    result = benchmark.pedantic(lambda: cli("self-test", isolate=True), rounds=3, warmup_rounds=0)
    assert result.exit_code in {0, 1}
    assert isinstance(result.envelope.status, RailStatus)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _store(assay_root: AssayHarness, protocol: str) -> ArtifactStore:
    """Build the file or memory ArtifactStore for the parametrized backend.

    Returns:
        File-backed store for ``protocol == "file"``; a memory-backed store otherwise.
    """
    match protocol:
        case "file":
            return assay_root.settings.store()
        case _:
            return assay_root.settings.store(protocol="memory", root=f"mem-bench/{assay_root.settings.run_id}")


# --- [COMPOSITION] ----------------------------------------------------------------------

_ENCODER: Final = msgspec.json.Encoder(order="deterministic")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BenchMeta", "BenchmarkFixture"]
