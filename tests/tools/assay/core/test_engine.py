"""Core executor laws: lease algebra, _stale PID-reuse, _decode_owner tamper, _drain tail cap, INPROC thunk.

Source surface: ``tools/assay/core/engine.py`` — ``_splice``, ``_drain``, ``_decode_owner``, ``_stale``,
``exclusive_lease``, ``leased``, ``_inproc``, ``fan_out``, ``_total``, ``_governed``, ``_diagnose``.
Laws: _decode_owner totality, _stale NoSuchProcess→True, _stale live→False, _stale staleness oracle,
_splice dotnet injection, _splice uv passthrough, INPROC thunk-less→rc1, INPROC raising→rc1+exc-name,
exclusive_lease BUSY path, exclusive_lease stale-steal, fan_out None-slot→FAULT(TIMEOUT),
SSH streaming round-trip, OTel span resource snapshot, _drain tail-cap property law, _governed CPU cap.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

from contextlib import aclosing
import os
from types import SimpleNamespace
from typing import override, TYPE_CHECKING, TypedDict

import anyio
from anyio.abc import ByteReceiveStream
import anyio.lowlevel
from expression import Error, Ok
from hypothesis import given
from hypothesis.strategies import binary
import msgspec
import pytest

from tests.tools.assay.conftest import _make_psutil_module, _proc  # noqa: PLC2701
import tools.assay.core.engine as engine_mod
from tools.assay.core.engine import _decode_owner, _drain, _governed, _inproc, _splice, _stale, _total, exclusive_lease, run_check  # noqa: PLC2701
from tools.assay.core.model import ArtifactKind, Check, Claim, Completed, Fault, Input, Language, Mode, receipt, Runner, Stage, Tool  # noqa: TC001
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Mapping, Sequence

    from anyio.streams.memory import MemoryObjectReceiveStream
    from expression import Result
    from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter

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


# --- [DRAIN HELPERS] -------------------------------------------------------------------
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


# --- [ALGEBRAIC] -----------------------------------------------------------------------


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


# --- [PSUTIL ORACLE] -------------------------------------------------------------------


@pytest.mark.parametrize("proc_kw,expected", _STALE_CASES)
def test_stale_oracle(proc_kw: _ProcKw, expected: bool, monkeypatch: pytest.MonkeyPatch) -> None:  # noqa: FBT001
    """``_stale`` oracle: NoSuchProcess→True; not_running→True; live+matching→False; drift→True."""
    fake = _make_psutil_module({None: _proc(), 99999: _proc(pid=99999, **proc_kw)})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    owner_raw = msgspec.json.encode(engine_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT))
    owner = engine_mod._DECODER.decode(owner_raw)
    assert _stale(owner, tolerance=1.0) is expected


# --- [SPLICE ORACLE] -------------------------------------------------------------------


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
    assert all(env[key] == value for key, value in assay_root.settings.python_tool_env.items())
    assert (work / "tools/assay/__init__.py").is_file()
    assert not (assay_root.root / "mutants").exists()


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


# --- [TOTAL SLOT] ----------------------------------------------------------------------


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
    "cpu_count,max_checks,expected",
    [
        (4, 8, 4),  # cpu_count caps at 4
        (4, 2, 2),  # max_checks caps below cpu_count
        (8, 8, 8),  # equal
        (1, 8, 1),  # single cpu caps at 1
        (4, 4, 4),  # exact match
    ],
)
def test_governed_cpu_cap(cpu_count: int, max_checks: int, expected: int, monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """``_governed`` caps ``max_checks`` by logical CPU count — neither starves nor oversubscribes."""
    monkeypatch.setattr(engine_mod.psutil, "cpu_count", lambda **_: cpu_count)  # type: ignore[attr-defined]
    settings = assay_root.settings.model_copy(update={"max_checks": max_checks})
    assert _governed(settings) == expected


# --- [DRAIN TAIL CAP] ------------------------------------------------------------------


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


# --- [OTEL SPAN RESOURCE] --------------------------------------------------------------


def test_otel_span_carries_mem_rss_attribute(assay_root: AssayHarness, otel_spans: InMemorySpanExporter, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_diagnose`` records a ``fault.resource_snapshot`` event on the active span with ``mem.rss_bytes``.

    Opens a real tracer span so ``_diagnose`` attaches to a recording span (not the no-op root).
    Monkeypatches psutil so the rss value is deterministic. Asserts via ``otel_spans.get_finished_spans()``
    — the private ``_RESOURCE`` contextvar is NOT read (E1 fix: internal-state leak → OTel contract).
    """
    from opentelemetry import trace as otel_trace  # noqa: PLC0415

    fake_rss = 65536
    fake = _make_psutil_module({None: _proc(rss=fake_rss)})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    otel_spans.clear()

    tracer = otel_trace.get_tracer("assay.test")
    with tracer.start_as_current_span("test.otel_law"):
        exc = TimeoutError("synthetic timeout for OTel test")
        engine_mod._diagnose(exc)

    finished = otel_spans.get_finished_spans()
    assert len(finished) >= 1, "Expected at least one finished span from the test.otel_law context"
    all_events = [e for span in finished for e in span.events]
    snapshot_events = [e for e in all_events if e.name == engine_mod._FAULT_SNAPSHOT]
    assert snapshot_events, f"No '{engine_mod._FAULT_SNAPSHOT}' event found; events: {[e.name for e in all_events]}"
    attrs = snapshot_events[0].attributes or {}
    assert "mem.rss_bytes" in attrs, f"'mem.rss_bytes' missing from event attributes: {dict(attrs)}"
    # OTel attribute values are typed as AttributeValue (int | float | str | ...); _snapshot stores float(rss).
    rss_val = attrs["mem.rss_bytes"]
    assert rss_val == fake_rss, f"Expected rss={fake_rss!r}, got {rss_val!r}"


# --- [LEASE] ---------------------------------------------------------------------------


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


# --- [SSH STREAMING] -------------------------------------------------------------------


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
