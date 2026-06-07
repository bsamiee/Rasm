"""Core executor laws: lease algebra, _stale PID-reuse, _decode_owner tamper, _drain tail cap, INPROC thunk."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

import contextlib
from contextlib import aclosing
import fcntl
import os
import sys
import tempfile
import time
from types import SimpleNamespace
from typing import override, TYPE_CHECKING, TypedDict

import anyio
from anyio.abc import ByteReceiveStream
import anyio.lowlevel
from expression import Error, Ok
from hypothesis import given, settings as hyp_settings, strategies as st
from hypothesis.stateful import Bundle, consumes, invariant, rule, RuleBasedStateMachine, run_state_machine_as_test
from hypothesis.strategies import binary
import msgspec
import psutil
import pytest
from upath import UPath

from tests.tools.assay.conftest import _make_psutil_module, _proc, assert_result_status  # noqa: PLC2701
from tools.assay.composition.settings import AssaySettings
import tools.assay.core.engine as engine_mod
from tools.assay.core.engine import (
    _decode_owner,  # noqa: PLC2701
    _drain,  # noqa: PLC2701
    _governed,  # noqa: PLC2701
    _inproc,  # noqa: PLC2701
    _remote_command,  # noqa: PLC2701
    _retry_on,  # noqa: PLC2701
    _splice,  # noqa: PLC2701
    _ssh_outcome,  # noqa: PLC2701
    _stale,  # noqa: PLC2701
    _total,  # noqa: PLC2701
    discover,
    exclusive_lease,
    fan_out,
    run_check,
    run_check_async,
)
from tools.assay.core.model import ArtifactKind, Check, Claim, Completed, Fault, Input, Language, Mode, receipt, Runner, Stage, Tool  # noqa: TC001
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Mapping, Sequence
    from pathlib import Path

    from anyio.streams.memory import MemoryObjectReceiveStream
    from expression import Result

    from tests.tools.assay.conftest import AssayHarness, SshLoopback


# --- [CONSTANTS] -----------------------------------------------------------------------


class _ProcKw(TypedDict, total=False):
    """Keyword payload accepted by the psutil process double."""

    raise_no_such: bool
    running: bool
    create_time: float


_ECHO_TOOL = Tool(
    name="test-echo",
    runner=Runner.DIRECT,
    command=("/bin/echo", "hello"),
    input=Input.NONE,
    language=Language.CSHARP,
    claim=Claim.STATIC,
    mode=Mode.CHECK,
)
_STREAM_TOOL = Tool(
    name="test-stream",
    runner=Runner.DIRECT,
    command=("/bin/echo", "stream-ok"),
    input=Input.NONE,
    language=Language.CSHARP,
    claim=Claim.STATIC,
    mode=Mode.BUILD,
)
_PYTHON_TOOL = Tool(
    name="test-python", runner=Runner.INPROC, command=(), input=Input.NONE, language=Language.PYTHON, claim=Claim.CODE, mode=Mode.CHECK
)
_ROUTED_CHANGED = Routed(language=Language.CSHARP, scope=Scope.CHANGED)

_CT = 1_700_000_000.0
_STALE_CASES: tuple[tuple[_ProcKw, bool], ...] = (
    ({"raise_no_such": True}, True),
    ({"running": False, "create_time": _CT}, True),
    ({"running": True, "create_time": _CT}, False),
    ({"running": True, "create_time": _CT + 5.0}, True),
)

_REMOTE_TOOL = Tool("remote", Runner.DOTNET, ("test",), Input.NONE, Language.CSHARP, Claim.STATIC)
# Emits 8 KiB of 'x' deterministically (> the 4096-byte default stream_tail_bytes) so the clip law has a true tail to truncate.
_LARGE_STREAM_TOOL = Tool(
    name="test-large-stream",
    runner=Runner.DIRECT,
    command=(sys.executable, "-c", "import sys; sys.stdout.write('x' * 8192)"),
    input=Input.NONE,
    language=Language.PYTHON,
    claim=Claim.STATIC,
    mode=Mode.BUILD,
)


# --- [DRAIN_ADAPTERS] -------------------------------------------------------------------
# ByteReceiveStream adapter for anyio.create_memory_object_stream[bytes]: bridges the ObjectReceiveStream
# (no max_bytes arg) to the ByteReceiveStream protocol that _drain requires. No cursor hand-rolling —
# backing store is fully owned by anyio's memory channel.


class _MemBytesStream(ByteReceiveStream):
    """Minimal ByteReceiveStream adapter over ``anyio.MemoryObjectReceiveStream[bytes]``.

    Delegates ``receive(max_bytes)`` to the underlying channel's ``receive()`` (chunks are
    pre-sized by the sender); maps ``ClosedResourceError`` / ``EndOfStream`` → ``anyio.EndOfStream``
    per the ``ByteReceiveStream`` protocol.
    """

    def __init__(self, recv: MemoryObjectReceiveStream[bytes]) -> None:
        self._recv = recv

    @override
    async def receive(self, max_bytes: int = 65536) -> bytes:  # max_bytes: chunks are pre-sized by sender
        try:
            return await self._recv.receive()
        except anyio.EndOfStream, anyio.ClosedResourceError:
            raise anyio.EndOfStream from None

    @override
    async def aclose(self) -> None:
        await self._recv.aclose()


async def _byte_stream(chunks: list[bytes]) -> _MemBytesStream:
    """Build a ready-to-drain ``_MemBytesStream`` from an explicit chunk list.

    Sends all chunks then closes the send end — ``_drain`` sees EOF after the last chunk.
    Buffer size equals ``len(chunks)`` so ``send`` never blocks.

    Returns:
        A stream adapter ready for ``_drain``.
    """
    send, recv = anyio.create_memory_object_stream[bytes](max_buffer_size=len(chunks) + 1)
    for c in chunks:
        await send.send(c)
    await send.aclose()
    return _MemBytesStream(recv)


# --- [OPERATIONS] -----------------------------------------------------------------------


@given(binary(max_size=512))
def test_decode_owner_is_total(data: bytes) -> None:
    """``_decode_owner(arbitrary_bytes)`` is total — never raises; returns None or a _LeaseOwner."""
    result = _decode_owner(data)
    # None (stealable) or a valid _LeaseOwner — both are acceptable; only raising is not
    assert result is None or hasattr(result, "resource")


def test_decode_owner_empty_bytes_is_none() -> None:
    """``_decode_owner(b'')`` is ``None`` — empty lock file is unconditionally stealable."""
    assert _decode_owner(b"") is None


def test_decode_owner_malformed_is_none() -> None:
    """Corrupt JSON is ``None`` — a crashed holder never FAULTs a contender."""
    assert _decode_owner(b"{not json") is None
    assert _decode_owner(b'{"resource": "x", "pid": "not-an-int"}') is None


# --- [PSUTIL_ORACLE] -------------------------------------------------------------------


@pytest.mark.parametrize("proc_kw,expected", _STALE_CASES)
def test_stale_oracle(proc_kw: _ProcKw, expected: bool, monkeypatch: pytest.MonkeyPatch) -> None:  # noqa: FBT001
    """``_stale`` oracle: NoSuchProcess→True; not_running→True; live+matching→False; drift→True."""
    fake = _make_psutil_module({None: _proc(), 99999: _proc(pid=99999, **proc_kw)})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    owner_raw = msgspec.json.encode(engine_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT))
    owner = engine_mod._DECODER.decode(owner_raw)
    assert _stale(owner, tolerance=1.0) is expected


def test_stale_access_denied_stays_live(monkeypatch: pytest.MonkeyPatch) -> None:
    """AccessDenied means the OS still owns the pid, so lease stealing stays disabled while pid_exists is true."""
    proc = _proc(pid=99999, running=True, create_time=_CT)
    proc.create_time.side_effect = psutil.AccessDenied(pid=99999)
    fake = _make_psutil_module({99999: proc})
    fake.pid_exists.return_value = True
    monkeypatch.setattr(engine_mod, "psutil", fake)
    owner = engine_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT)

    assert _stale(owner, tolerance=1.0) is False


def test_retry_classifier_keeps_exit_codes_out_of_retry() -> None:
    """Retry classification targets spawn/connect failures, not normal tool exits or parse defects."""
    direct = Check(tool=_ECHO_TOOL)
    remote = Check(tool=_REMOTE_TOOL)

    assert _retry_on(direct, None)(FileNotFoundError("missing")) is False
    assert _retry_on(direct, None)(OSError("local")) is False
    assert _retry_on(remote, None)(OSError("remote transport")) is True
    assert _retry_on(remote, None)(TimeoutError("deadline")) is False


def test_discover_maps_read_only_process_status_to_result(tmp_path: Path) -> None:
    """Engine-owned discovery returns stdout on zero and Fault on non-zero without routing-owned subprocess code."""
    ok = discover((sys.executable, "-c", "print('a')"), root=tmp_path, timeout=5.0)
    bad = discover((sys.executable, "-c", "import sys; sys.stderr.write('bad'); sys.exit(2)"), root=tmp_path, timeout=5.0)

    assert ok == Ok(b"a\n")
    assert bad.is_error()
    assert bad.error.status is RailStatus.FAULTED
    assert bad.error.message == "bad"


@pytest.mark.anyio
async def test_run_check_async_is_public_event_loop_boundary(assay_root: AssayHarness) -> None:
    """Async callers use run_check_async directly instead of importing the private woven spawn function."""
    outcome = await run_check_async(Check(tool=_ECHO_TOOL), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)

    assert outcome.is_ok()
    assert b"hello" in outcome.ok.stdout


def test_retry_attempts_are_reported_after_transient_spawn_recovery(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Recovered transient spawn/connect retries leave agent-visible attempt evidence."""
    calls = 0

    async def flaky(*_args: object, **_kwargs: object) -> Completed:
        nonlocal calls
        await anyio.lowlevel.checkpoint()
        calls += 1
        if calls == 1:
            raise OSError("temporary transport")
        return receipt(("dotnet", "test"), 0)

    monkeypatch.setattr(engine_mod, "_execute", flaky)
    outcome = run_check(Check(tool=_REMOTE_TOOL), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)

    assert outcome.is_ok()
    assert calls == 2
    assert "retry attempts=2" in outcome.ok.notes


# --- [SPLICE_ORACLE] -------------------------------------------------------------------


@pytest.mark.parametrize(
    "runner,command,claim,mode,contains,not_contains",
    [
        (Runner.DOTNET, ("build", "Workspace.slnx"), Claim.STATIC, Mode.BUILD, ["--artifacts-path", "--disable-build-servers"], []),
        (Runner.UV, ("ruff", "check", "."), Claim.CODE, Mode.CHECK, [], ["--artifacts-path"]),
        (Runner.DOTNET, ("test", "Rasm.Tests.csproj", "--list-tests"), Claim.TEST, Mode.LIST, [], ["--artifacts-path"]),
        (Runner.DOTNET, ("build", "Workspace.slnx"), Claim.STATIC, Mode.BUILD, [], []),  # None scope — passthrough
    ],
)
def test_splice_oracle(
    runner: Runner, command: tuple[str, ...], claim: Claim, mode: Mode, contains: list[str], not_contains: list[str], assay_root: AssayHarness
) -> None:
    """``_splice`` oracle: dotnet build-graph verbs inject flags; UV/LIST/None-scope pass verbatim."""
    scope = assay_root.scope(claim)
    # 4th parametrize row: None scope tests passthrough path — both contains and not_contains are empty (row-4 sentinel)
    use_scope = None if (not contains and not not_contains and runner is Runner.DOTNET and mode is Mode.BUILD) else scope
    result = _splice(runner, command, use_scope, assay_root.settings.scoped_verbs, mode)
    for flag in contains:
        assert flag in result
    for flag in not_contains:
        assert flag not in result


def test_staged_process_materializes_workdir_and_env(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Staged process tools run from artifact workdirs with repo-local Python storage."""
    seen: list[tuple[tuple[str, ...], str, dict[str, str]]] = []
    monkeypatch.setenv("HYPOTHESIS_STORAGE_DIRECTORY", ".hypothesis")
    for rel in ("pyproject.toml", "tools/assay/__init__.py"):
        assay_root.write(rel)

    async def fake_run_process(argv: Sequence[str], *, cwd: str, env: Mapping[str, str], check: bool) -> object:
        _ = check
        seen.append((tuple(argv), cwd, dict(env)))
        await anyio.lowlevel.checkpoint()
        return SimpleNamespace(returncode=0, stdout=b"ok", stderr=b"")

    tool = Tool(
        "stage-law",
        Runner.UV,
        ("mutmut", "run"),
        Input.FILES,
        Language.PYTHON,
        Claim.TEST,
        mode=Mode.RUN,
        stage=Stage(root=".artifacts/python/mutmut/work", inputs=("pyproject.toml", "tools/assay"), project=True),
    )
    monkeypatch.setattr(anyio, "run_process", fake_run_process)
    outcome = run_check(
        Check(tool=tool),
        settings=assay_root.settings,
        scope=None,
        routed=Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=("tests/tools/assay",)),
    )
    work = assay_root.root / ".artifacts/python/mutmut/work"
    argv, cwd, env = seen[0]

    assert outcome.is_ok()
    assert argv == ("uv", "run", "--project", str(assay_root.root), "mutmut", "run", "tests/tools/assay")
    assert cwd == str(work)
    expected_env = assay_root.settings.python_tool_env
    assert all(env[key] == value for key, value in expected_env.items())
    assert env["HYPOTHESIS_STORAGE_DIRECTORY"] == str(assay_root.root / ".cache" / "hypothesis")
    assert env["HYPOTHESIS_STORAGE_DIRECTORY"] != ".hypothesis"
    assert (work / "tools/assay/__init__.py").is_file()
    assert not (assay_root.root / "mutants").exists()


def test_streaming_process_persists_full_artifact(assay_root: AssayHarness) -> None:
    """Streaming receipts keep bounded tails while full stdout is persisted through ArtifactStore."""
    scope = assay_root.scope(Claim.STATIC)
    outcome = run_check(Check(tool=_STREAM_TOOL), settings=assay_root.settings, scope=scope, routed=_ROUTED_CHANGED)

    assert outcome.is_ok()
    assert b"stream-ok" in outcome.ok.stdout
    assert any(artifact.id == "test-stream-out" for artifact in outcome.ok.artifacts)
    stdout_artifact = next(artifact for artifact in outcome.ok.artifacts if artifact.id == "test-stream-out")
    assert scope.store.read_path(stdout_artifact.path).strip() == b"stream-ok"


def test_streaming_tail_clips_at_stream_tail_bytes(assay_root: AssayHarness) -> None:
    """The streaming receipt tail is clipped to ``stream_tail_bytes`` while the full stdout persists through ArtifactStore."""
    scope = assay_root.scope(Claim.STATIC)
    tail_cap = assay_root.settings.stream_tail_bytes
    outcome = run_check(Check(tool=_LARGE_STREAM_TOOL), settings=assay_root.settings, scope=scope, routed=_ROUTED_CHANGED)

    assert outcome.is_ok()
    assert len(outcome.ok.stdout) == tail_cap
    assert outcome.ok.stdout == b"x" * tail_cap
    stdout_artifact = next(artifact for artifact in outcome.ok.artifacts if artifact.id == "test-large-stream-out")
    assert scope.store.read_path(stdout_artifact.path) == b"x" * 8192


@pytest.mark.parametrize(
    "runner,exc,expected",
    [
        (Runner.DOTNET, ConnectionError("reset"), True),  # transport reset retries on a remote runner
        (Runner.DOTNET, BrokenPipeError("pipe"), True),  # broken pipe retries on a remote runner
        (Runner.DOTNET, OSError("transport"), True),  # generic OSError retries on a non-direct runner
        (Runner.DIRECT, OSError("local"), False),  # local OSError never retries — a tool defect, not a transport fault
        (Runner.DOTNET, FileNotFoundError("absent"), False),  # missing binary is a capability gap, never retried
        (Runner.DOTNET, TimeoutError("deadline"), False),  # deadline is terminal, not transient
    ],
)
def test_retry_classifier_table(runner: Runner, exc: BaseException, expected: bool) -> None:  # noqa: FBT001
    """``_retry_on`` returns a predicate that retries transport/spawn faults on non-direct runners only."""
    check = Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=runner))
    assert _retry_on(check, None)(exc) is expected


@pytest.mark.mutation
def test_staged_process_rejects_escaping_paths(assay_root: AssayHarness) -> None:
    """Stage workdirs and inputs are contained before any destructive materialization."""
    for stage in (Stage(root="../outside"), Stage(root=".artifacts/python/work", inputs=("../pyproject.toml",))):
        outcome = run_check(
            Check(tool=Tool("stage-law", Runner.DIRECT, ("true",), Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.RUN, stage=stage)),
            settings=assay_root.settings,
            scope=None,
            routed=Routed(language=Language.PYTHON, scope=Scope.CHANGED),
        )

        assert outcome.is_error()
        assert "unsafe stage path" in outcome.error.message
    assert not (assay_root.root.parent / "outside").exists()


# --- [INPROC] --------------------------------------------------------------------------


def test_inproc_no_thunk_yields_rc1() -> None:
    """``Runner.INPROC`` with no thunk → ``rc=1, b'no thunk' in stderr`` (wiring defect, never raises)."""
    check = Check(tool=_PYTHON_TOOL, paths=())
    done = anyio.run(_inproc, check)
    assert done.returncode == 1
    assert b"no thunk" in done.stderr.lower()


def test_inproc_raising_thunk_yields_rc1_with_exc_name() -> None:
    """A raising INPROC thunk → ``rc=1``, exc type in stderr (never propagates across the seam)."""

    def bad_thunk(check: Check) -> Completed:
        raise RuntimeError("deliberate D50 fault")

    tool = msgspec.structs.replace(_PYTHON_TOOL, thunk=bad_thunk)
    check = Check(tool=tool, paths=())
    done = anyio.run(_inproc, check)
    assert done.returncode == 1
    assert b"RuntimeError" in done.stderr


def test_inproc_successful_thunk_yields_rc0() -> None:
    """A healthy INPROC thunk returns its receipt with the caller's rc."""

    def good_thunk(check: Check) -> Completed:
        return receipt((check.tool.name,), 0, stdout=b"ok")

    tool = msgspec.structs.replace(_PYTHON_TOOL, thunk=good_thunk)
    check = Check(tool=tool, paths=())
    done = anyio.run(_inproc, check)
    assert done.returncode == 0
    assert done.stdout == b"ok"


# --- [TOTAL_SLOT] ----------------------------------------------------------------------


def test_total_none_slot_yields_timeout_fault() -> None:
    """``_total(None)`` → ``Error(Fault(TIMEOUT))`` — fan_out None-slot is always back-filled."""
    result = _total(None)
    assert result.is_error()
    assert result.error.status is RailStatus.TIMEOUT


def test_total_ok_slot_passes_through() -> None:
    """``_total(Ok(done))`` passes the value through unchanged."""
    done = receipt(("echo",), 0)
    result = _total(Ok(done))
    assert result.is_ok()
    assert result.ok is done


def test_total_error_slot_passes_through() -> None:
    """``_total(Error(fault))`` passes the error through unchanged."""
    fault = Fault((), RailStatus.FAULTED, "spawn failed")
    result = _total(Error(fault))
    assert result.is_error()
    assert result.error is fault


# --- [GOVERNED] ------------------------------------------------------------------------


@pytest.mark.parametrize(
    "cpu_count,max_checks,dotnet,mutation,runner_modes,expected",
    [
        (4, 8, 4, 4, [], 4),  # usable_cpu caps at 4
        (4, 2, 4, 4, [], 2),  # max_checks caps below usable_cpu
        (8, 8, 8, 8, [], 8),  # equal
        (1, 8, 8, 8, [], 1),  # single usable cpu caps at 1
        (4, 8, 2, 8, [(Runner.DOTNET, Mode.CHECK)], 2),  # dotnet runner_cap engages mode_cap default
        (4, 8, 8, 1, [(Runner.DIRECT, Mode.MUTATION)], 1),  # mutation mode_cap engages
        (4, 8, 2, 3, [(Runner.DOTNET, Mode.MUTATION)], 2),  # both present: dotnet (2) intersects mutation (3) -> 2
    ],
)
def test_governed_cap_table(
    cpu_count: int,
    max_checks: int,
    dotnet: int,
    mutation: int,
    runner_modes: list[tuple[Runner, Mode]],
    expected: int,
    monkeypatch: pytest.MonkeyPatch,
    assay_root: AssayHarness,
) -> None:
    """``_governed`` folds the 3-axis cap (usable cpu, dotnet runner, mutation mode) into one concurrency floor.

    ``_USABLE_CPU`` (the import-time cpu_affinity constant) is patched directly: Linux ``cpu_affinity`` bypasses a ``cpu_count`` monkeypatch.
    """
    monkeypatch.setattr(engine_mod, "_USABLE_CPU", cpu_count)
    settings = assay_root.settings.model_copy(update={"max_checks": max_checks, "dotnet_max_cpu": dotnet, "mutation_max_cpu": mutation})
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=r, mode=m)) for r, m in runner_modes)
    assert _governed(settings, checks) == expected


# --- [FAN_OUT] -------------------------------------------------------------------------


@pytest.mark.mutation
def test_fan_out_deadline_integrates_order_and_timeout(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``fan_out`` preserves input order, completes the fast slots, and back-fills the deadline-cancelled slot as TIMEOUT.

    The idx-2 slot sleeps 10s but the per-check ``fail_after(_remaining(deadline))`` cancels it at ~0.3s,
    so the test never stalls on the never-completing worker — deadline teardown converts it to a TIMEOUT fault.
    """

    async def indexed(check: Check, *_args: object, **_kwargs: object) -> Completed:
        idx = int(check.tool.name.split("-")[1])
        await anyio.sleep(0 if idx < 2 else 10.0)
        return receipt((check.tool.name,), 0)

    monkeypatch.setattr(engine_mod, "_execute", indexed)
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, name=f"check-{i}", runner=Runner.DOTNET)) for i in range(3))
    results = fan_out(checks, settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED, deadline=time.monotonic() + 0.3)

    assert len(results) == 3
    assert results[0].is_ok()
    assert results[1].is_ok()
    assert_result_status(results[2], RailStatus.TIMEOUT)


# --- [DRAIN_TAIL_CAP] ------------------------------------------------------------------


@given(binary(min_size=0, max_size=4096))
def test_drain_tail_cap_property(data: bytes) -> None:
    """``_drain`` tail is always the last ``tail_cap`` bytes of the complete stream (for any chunking).

    Chunks are 17 bytes through a drain with ``chunk=32`` — misalignment exercises the slice path in
    ``_next_chunk`` (E12: aligned chunks masked the truncation branch).
    """
    tail_cap = 128
    sender_chunk = 17  # intentionally != drain chunk=32 to exercise the slice path

    async def _run() -> bytes:
        # Split into 17-byte sender chunks; _drain reads with chunk=32
        raw_chunks = [data[i : i + sender_chunk] for i in range(0, len(data), sender_chunk)] if data else [b""]
        async with aclosing(await _byte_stream(raw_chunks)) as stream:
            return await _drain(stream, tail_cap=tail_cap, chunk=32)

    result = anyio.run(_run)
    expected = data[-tail_cap:] if len(data) >= tail_cap else data
    assert result == expected


def test_drain_none_stream_returns_empty() -> None:
    """``_drain(None, ...)`` short-circuits to ``b""`` (inherited fd / non-streaming arm)."""

    async def _run() -> bytes:
        return await _drain(None, tail_cap=128, chunk=32)

    result = anyio.run(_run)
    assert result == b""


# --- [OTEL_SPAN_RESOURCE] --------------------------------------------------------------


def test_otel_span_carries_mem_rss_attribute(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_diagnose`` records a ``fault.resource_snapshot`` event on the active span with ``mem.rss_bytes``."""
    fake_rss = 65536
    fake = _make_psutil_module({None: _proc(rss=fake_rss)})
    monkeypatch.setattr(engine_mod, "psutil", fake)

    class Span:
        def __init__(self) -> None:
            self.events: list[tuple[str, object]] = []
            self.exceptions: list[BaseException] = []

        def record_exception(self, exc: BaseException) -> None:
            self.exceptions.append(exc)

        def add_event(self, name: str, attributes: object) -> None:
            self.events.append((name, attributes))

    span = Span()
    monkeypatch.setattr(engine_mod.__dict__["trace"], "get_current_span", lambda: span)

    exc = TimeoutError("synthetic timeout for OTel test")
    engine_mod._diagnose(exc)

    assert span.exceptions == [exc]
    assert span.events
    name, attrs = span.events[0]
    assert name == engine_mod._FAULT_SNAPSHOT
    match attrs:
        case {"mem.rss_bytes": int() | float() as rss}:
            assert rss == fake_rss
        case _:
            pytest.fail(f"missing mem.rss_bytes in {attrs!r}")


# --- [LEASE] ---------------------------------------------------------------------------


@pytest.mark.mutation
def test_exclusive_lease_busy_on_live_holder(assay_root: AssayHarness) -> None:
    """A second ``exclusive_lease`` on the same resource while the first is held yields ``Error(Fault(BUSY))``."""
    with exclusive_lease("test-resource", "run-a", settings=assay_root.settings) as first:
        assert first.is_ok(), f"expected Ok on first acquire, got: {first}"
        with exclusive_lease("test-resource", "run-b", settings=assay_root.settings) as second:
            assert second.is_error()
            assert second.error.status is RailStatus.BUSY


def test_exclusive_lease_releases_after_context(assay_root: AssayHarness) -> None:
    """After the context exits, a subsequent acquire succeeds (lock file is truncated/released)."""
    with exclusive_lease("release-test", "run-a", settings=assay_root.settings) as first:
        assert first.is_ok()
    with exclusive_lease("release-test", "run-b", settings=assay_root.settings) as second:
        assert second.is_ok()


def test_exclusive_lease_stale_steal(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A dead-pid lock is stolen: ``exclusive_lease`` returns ``Ok`` on a stale (NoSuchProcess) holder.

    Writes a lock with a guaranteed-dead process identity (pid=88888, create_time=0.0), then
    monkeypatches psutil so ``Process(88888)`` raises ``NoSuchProcess`` — proving ``_stale``
    returns True and ``_claim`` steals rather than yielding BUSY.
    """
    dead_pid = 88888
    dead_ct = 0.0
    self_pid = os.getpid()
    # _write_owner calls psutil.Process(os.getpid()) — register the live PID so the factory returns it.
    fake = _make_psutil_module({self_pid: _proc(pid=self_pid), dead_pid: _proc(pid=dead_pid, raise_no_such=True)})
    monkeypatch.setattr(engine_mod, "psutil", fake)

    # Write a stale owner block directly into the lock file before the test acquires.
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "stale-resource.lock")
    lock_path.parent.mkdir(parents=True, exist_ok=True)
    stale_owner = engine_mod._LeaseOwner(resource="stale-resource", run_id="old-run", pid=dead_pid, create_time=dead_ct)
    lock_path.write_bytes(msgspec.json.encode(stale_owner))

    with exclusive_lease("stale-resource", "new-run", settings=assay_root.settings) as result:
        assert result.is_ok(), f"stale-steal should succeed, got: {result}"


@pytest.mark.mutation
def test_lease_owner_block_fields(assay_root: AssayHarness) -> None:
    """A held lease stamps every owner-block field: resource/run_id/cwd/pid and a positive create_time."""
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "field-test.lock")
    with exclusive_lease("field-test", "run-field", settings=assay_root.settings, project="my-project", mode="exclusive") as held:
        assert held.is_ok()
        owner = engine_mod._DECODER.decode(lock_path.read_bytes())

    assert (owner.resource, owner.run_id, owner.cwd, owner.pid) == ("field-test", "run-field", str(assay_root.root), os.getpid())
    assert owner.project == "my-project"
    assert owner.mode == "exclusive"
    assert owner.create_time > 0.0


@pytest.mark.mutation
def test_claim_busy_under_separately_held_flock(assay_root: AssayHarness) -> None:
    """An empty/corrupt body STEALS unless a sibling fd holds the flock — a held flock maps the contender to BUSY."""
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "contended.lock")
    lock_path.parent.mkdir(parents=True, exist_ok=True)
    fd = os.open(str(lock_path), os.O_RDWR | os.O_CREAT)
    fcntl.flock(fd, fcntl.LOCK_EX)  # ty: ignore[possibly-missing-attribute]  # POSIX-only; verified present on macOS/Linux
    try:
        with exclusive_lease("contended", "run-c", settings=assay_root.settings) as contender:
            assert_result_status(contender, RailStatus.BUSY)
    finally:
        fcntl.flock(fd, fcntl.LOCK_UN)  # ty: ignore[possibly-missing-attribute]  # POSIX-only; symmetric release
        os.close(fd)


# --- [STATEFUL_LEASE] ------------------------------------------------------------------


class _LeaseSlot(msgspec.Struct, frozen=True, gc=False):
    """One entered lease: its per-token ``ExitStack`` plus whether the acquire returned Ok (a live holder)."""

    stack: contextlib.ExitStack
    is_ok: bool


class LeaseStateMachine(RuleBasedStateMachine):
    """Stateful model of the synchronous ``exclusive_lease`` mutual-exclusion algebra.

    Each ``acquire`` enters a real ``exclusive_lease`` context for one shared resource through a per-token
    ``ExitStack`` and records the held-vs-busy outcome. ``release`` consumes a slot and closes only that
    token's stack. The invariant proves the contract: at most one Ok holder is live across all entered
    leases, so any acquire while the resource is held yields BUSY. Purely synchronous — no event loop.
    """

    held = Bundle("held")

    def __init__(self) -> None:
        """Initialise the state machine with an empty slot registry and a fresh temporary artifact root."""
        super().__init__()
        self._slots: dict[int, _LeaseSlot] = {}
        root = UPath(tempfile.mkdtemp(prefix="assay-lease-rbsm-"))
        (root / "Workspace.slnx").write_text("", encoding="utf-8")
        self._settings = AssaySettings(root=root, exec_target="", exec_known_hosts=None)

    @rule(target=held, run_id=st.text(alphabet="abcdef0123456789", min_size=1, max_size=8))
    def acquire(self, run_id: str) -> int:
        stack = contextlib.ExitStack()
        result = stack.enter_context(exclusive_lease("shared", run_id, settings=self._settings))
        slot = _LeaseSlot(stack=stack, is_ok=result.is_ok())
        key = id(slot)
        self._slots[key] = slot
        return key

    @rule(key=consumes(held))
    def release(self, key: int) -> None:
        slot = self._slots.pop(key, None)
        match slot:
            case _LeaseSlot(stack=stack):
                stack.close()
            case None:
                pass

    @invariant()
    def at_most_one_live_holder(self) -> None:
        live = sum(1 for slot in self._slots.values() if slot.is_ok)
        assert live <= 1, f"mutual exclusion broken: {live} live holders"

    @override
    def teardown(self) -> None:
        # Drain every still-open slot stack through a forcing comprehension (no imperative loop in the model).
        _ = tuple(slot.stack.close() for slot in self._slots.values())  # type: ignore[func-returns-value]


def test_lease_state_machine_holds_mutual_exclusion() -> None:
    """Drive the synchronous ``exclusive_lease`` RBSM: across all acquire/release interleavings, at most one Ok holder is ever live."""
    run_state_machine_as_test(LeaseStateMachine, settings=hyp_settings(stateful_step_count=50, deadline=None))  # type: ignore[no-untyped-call]


# --- [REMOTE_UNITS] --------------------------------------------------------------------


@pytest.mark.parametrize(
    "argv,cwd,env,fragments",
    [
        (("dotnet", "test"), "/work", {}, ("cd /work &&", "dotnet test")),
        (("ruff", "check", "."), "/repo root", {"PYTHONHASHSEED": "0"}, ("cd '/repo root'", "PYTHONHASHSEED=0", "ruff check .")),
        (("echo", "a b"), "/w", {"K": "v w"}, ("cd /w &&", "K='v w'", "echo 'a b'")),
    ],
)
def test_remote_command_quotes_cwd_env_and_argv(argv: tuple[str, ...], cwd: str, env: dict[str, str], fragments: tuple[str, ...]) -> None:
    """``_remote_command`` shell-quotes the cwd prefix, env exports, and every argv segment into one ``cd ... && ...`` line."""
    command = _remote_command(argv, cwd=cwd, env=env)
    assert all(fragment in command for fragment in fragments), f"missing fragment in {command!r}"


@pytest.mark.parametrize(
    "status,signal,expected",
    [
        (0, None, (0, ())),  # clean exit passes through with no signal note
        (2, None, (2, ())),  # non-zero exit passes through with no signal note
        (None, None, (255, ())),  # signal-killed (None status) maps to the sentinel; no signal tuple to note
        (None, ("TERM", False, "", ""), (255, ("ssh.signal=TERM",))),  # asyncssh exit_signal 4-tuple surfaces its name as evidence
    ],
)
def test_ssh_outcome_maps_signal_none_to_sentinel(status: int | None, signal: object | None, expected: tuple[int, tuple[str, ...]]) -> None:
    """``_ssh_outcome`` passes integer exit codes through and maps a ``None`` status (signal kill) to ``255``, noting the signal name."""
    assert _ssh_outcome(status, signal) == expected


# --- [SSH_STREAMING] -------------------------------------------------------------------


@pytest.mark.anyio
@pytest.mark.network
async def test_ssh_loopback_non_streaming_round_trip(assay_root: AssayHarness, ssh_loopback: SshLoopback, socket_enabled: None) -> None:
    """The engine's ``_run_remote`` non-streaming arm returns the canned loopback reply.

    ``run_check`` calls ``anyio.run`` internally; ``anyio.from_thread.run_sync`` bridges it into the
    async test's event loop without nesting event loops.
    """
    _ = socket_enabled
    remote = assay_root.remote(ssh_loopback.exec_target)
    check = Check(tool=_ECHO_TOOL, cwd=assay_root.root)

    def _sync() -> Result[Completed, Fault]:
        return run_check(check, settings=remote, scope=None, routed=_ROUTED_CHANGED)

    outcome = await anyio.to_thread.run_sync(_sync)  # ty: ignore[unresolved-attribute]
    assert outcome.is_ok()
    # Handler echoes "remote-ok:<command>\n" — assert token AND that the command transited (E4 fix).
    assert b"remote-ok:" in outcome.ok.stdout, f"expected 'remote-ok:' prefix, got {outcome.ok.stdout!r}"
    # T12 verified-gap: the run_id must ride the remote env export so a remote tool stamps the same run.
    # remote_env() admits ASSAY_RUN_ID but only forwards it when already present in os.environ — this is the gap.
    assert b"ASSAY_RUN_ID=" in outcome.ok.stdout
    assert outcome.ok.returncode == 0


@pytest.mark.anyio
@pytest.mark.network
async def test_ssh_streaming_round_trip(assay_root: AssayHarness, ssh_loopback: SshLoopback, socket_enabled: None) -> None:
    r"""The engine's ``_run_remote`` streaming arm drains stdout via anyio TaskGroup + ``_drain_reader``.

    Covers the ``create_process`` + TaskGroup + ``_drain_reader`` path. Handler echoes
    ``"remote-ok:<command>\n"`` — both connectivity and command-fidelity are asserted (E4 fix).
    """
    _ = socket_enabled
    remote = assay_root.remote(ssh_loopback.exec_target)
    check = Check(tool=_STREAM_TOOL, cwd=assay_root.root)

    def _sync() -> Result[Completed, Fault]:
        return run_check(check, settings=remote, scope=None, routed=_ROUTED_CHANGED)

    outcome = await anyio.to_thread.run_sync(_sync)  # ty: ignore[unresolved-attribute]
    assert outcome.is_ok(), f"streaming SSH round-trip faulted: {outcome}"
    # The handler echoes "remote-ok:<full_command>" but the streaming path tail-caps stdout.
    # The tail contains the command suffix (e.g. "/bin/echo stream-ok") — assert that transited.
    assert b"/bin/echo" in outcome.ok.stdout, f"command suffix not found in stdout tail: {outcome.ok.stdout!r}"
    assert outcome.ok.returncode == 0


# --- [MUTATION_LANE] -------------------------------------------------------------------


def test_mutation_lane_is_populated(request: pytest.FixtureRequest) -> None:
    """The ``mutation`` marker lane carries the engine's destructive-boundary laws so Stryker/mutmut has a target set."""
    collected = request.session.items if hasattr(request.session, "items") else ()
    marked = [item for item in collected if item.get_closest_marker("mutation") is not None]
    assert len(marked) >= 5, f"mutation lane underpopulated: {len(marked)} marked"
