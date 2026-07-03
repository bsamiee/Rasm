"""Govern laws: leases, concurrency ceilings, stream draining, stall triage, reaping, and resource evidence.

Pure transforms use oracle laws; boundary surfaces use real subprocess, psutil-double, and flock seams.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections import deque
import contextlib
import fcntl
import functools
import os
import signal
import sys
import tempfile
import time
from types import SimpleNamespace
from typing import override, TYPE_CHECKING, TypedDict

import anyio
from dirty_equals import IsInt, IsPartialDict, IsPositiveFloat
from expression import Error, Ok
from hypothesis import given, HealthCheck, settings as hyp_settings, strategies as st, target
from hypothesis.stateful import Bundle, consumes, invariant, rule, RuleBasedStateMachine
import msgspec
from opentelemetry import trace
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter  # noqa: TC002  # collection-time fixture annotation
import psutil
import pytest
from upath import UPath

from tests.python._testkit.spec import assert_error_status, assert_ok, model_based, monotone, roundtrip, support_matrix, validity_matrix

# Hypothesis resolves fixture annotations at collection time under PEP 649.
from tests.python.tools.assay.kit import _make_psutil_module, _proc, AssayHarness  # noqa: TC001
from tools.assay.composition.settings import AssaySettings
from tools.assay.core.aspect import RING  # ring seam seeded directly for ring-content assertions
from tools.assay.core.exec import run_check
import tools.assay.core.govern as govern_mod
from tools.assay.core.govern import (
    Captured,
    captured_outputs,
    decode_lease_owner,
    diagnose,
    dotnet_slot,
    drain_pair,
    drain_stream,
    exclusive_lease,
    ExecPlan,
    governed_concurrency,
    is_lease_stale,
    leased,
    line_count,
    max_resources,
    measure,
    Measurements,
    proc_dead,
    reap,
    recv_anyio,
    resource_monitor,
    resource_projection,
    resource_sample,
    stall_monitor,
    StalledProcess,
    stream_artifacts,
    touched,
    WriteSink,
)
from tools.assay.core.model import ArtifactKind, Check, Claim, Fault, Input, Language, Mode, RailStatus, receipt, Runner, Tool
from tools.assay.core.routing import Routed, Scope


if TYPE_CHECKING:
    from collections.abc import Iterator
    from pathlib import Path

    from expression import Result

    from tools.assay.composition.store import ArtifactScope
    from tools.assay.core.model import Completed


# --- [TYPES] ----------------------------------------------------------------------------


class _ProcKw(TypedDict, total=False):
    """Process-double keyword payload for staleness and liveness sweeps."""

    raise_no_such: bool
    running: bool
    create_time: float
    status: str


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (
    Captured, captured_outputs, decode_lease_owner, diagnose, dotnet_slot, drain_pair, drain_stream, ExecPlan,
    exclusive_lease, governed_concurrency, is_lease_stale, leased, line_count, max_resources, measure, Measurements,
    proc_dead, reap, recv_anyio, resource_monitor, resource_projection, resource_sample, stall_monitor,
    StalledProcess, stream_artifacts, touched, WriteSink,
)  # fmt: skip

_CT: float = 1_700_000_000.0
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
_ROUTED_CHANGED = Routed(language=Language.CSHARP, scope=Scope.CHANGED)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _run(check: Check, harness: AssayHarness, *, scope: ArtifactScope | None = None) -> Result[Completed, Fault]:
    """Drive ``run_check`` against the harness with the no-sink local arm as default.

    Returns:
        Completed outcome from running ``check`` over the harness settings.
    """
    return run_check(check, settings=harness.settings, scope=scope, routed=_ROUTED_CHANGED)


def _stream_tool(name: str, command: tuple[str, ...], language: Language = Language.CSHARP) -> Tool:
    """Build a BUILD-mode DIRECT tool that exercises streaming tail and artifact paths.

    Returns:
        BUILD-mode DIRECT tool over ``command``.
    """
    return Tool(name, Runner.DIRECT, command, Input.NONE, language, Claim.STATIC, mode=Mode.BUILD)


def _recv_of(chunks: tuple[bytes, ...]) -> govern_mod.ByteRecv:
    """Build a ``ByteRecv`` double yielding chunks then ``None`` at EOF.

    Returns:
        Async receiver draining ``chunks`` before signalling EOF.
    """
    pending = list(chunks)

    async def _recv() -> bytes | None:
        await anyio.sleep(0.0)
        return pending.pop(0) if pending else None

    return _recv


# --- [DECODE_OWNER]


def test_decode_lease_owner_roundtrip_and_corrupt_bytes() -> None:
    """``decode_lease_owner`` inverts ``msgspec.json.encode`` on a real owner block; corrupt bytes decode to ``None``."""
    owner = govern_mod._LeaseOwner(resource="r", run_id="run-x", pid=4321, create_time=_CT, project="p", mode="exclusive")
    roundtrip(owner, msgspec.json.encode, lambda raw: decode_lease_owner(raw) or pytest.fail("decode_lease_owner lost a valid owner block"))
    validity_matrix(
        (
            ("empty", b"", True),
            ("not-json", b"{not json", True),
            ("missing-required", b'{"resource": "x"}', True),
            ("pid-wrong-type", b'{"resource": "x", "pid": "not-an-int"}', True),
        ),
        lambda raw: decode_lease_owner(raw) is None,
    )


@given(data=st.binary(max_size=512))
def test_decode_lease_owner_is_total(data: bytes) -> None:
    """``decode_lease_owner`` never raises on arbitrary bytes — only ``None`` or a real ``_LeaseOwner``."""
    decoded = decode_lease_owner(data)
    assert decoded is None or isinstance(decoded, govern_mod._LeaseOwner)


# --- [LIVENESS]

_STALE_CASES: tuple[tuple[str, _ProcKw, bool], ...] = (
    ("no-such-process", {"raise_no_such": True}, True),
    ("not-running", {"running": False, "create_time": _CT}, True),
    ("live-and-matching", {"running": True, "create_time": _CT}, False),
    ("create-time-drift", {"running": True, "create_time": _CT + 5.0}, True),
    ("zombie-holder", {"running": True, "create_time": _CT, "status": psutil.STATUS_ZOMBIE}, True),
    ("dead-holder", {"running": True, "create_time": _CT, "status": psutil.STATUS_DEAD}, True),
)


@pytest.mark.parametrize("label, proc_kw, expected", _STALE_CASES, ids=[c[0] for c in _STALE_CASES])
def test_is_lease_stale_decision_table(label: str, proc_kw: _ProcKw, expected: bool, monkeypatch: pytest.MonkeyPatch) -> None:  # noqa: FBT001
    """``is_lease_stale``: dead/not-running/create-time-drift/zombie/dead-status ⇒ True; live-and-matching ⇒ False."""
    _ = label
    fake = _make_psutil_module({None: _proc(), 99999: _proc(pid=99999, **proc_kw)})
    monkeypatch.setattr(govern_mod, "psutil", fake)
    owner = govern_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT)
    assert is_lease_stale(owner, tolerance=1.0) is expected


@given(drift=st.floats(min_value=0.0, max_value=0.99))
def test_is_lease_stale_monotone_in_drift(drift: float) -> None:
    """Holding a live matching pid, staleness (0=fresh, 1=stale) is monotone in tolerance — a wider band never flips fresh→stale."""
    fake = _make_psutil_module({99999: _proc(pid=99999, running=True, create_time=_CT + drift)})
    owner = govern_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT)
    with pytest.MonkeyPatch.context() as patch:
        patch.setattr(govern_mod, "psutil", fake)
        monotone(drift + 0.005, drift - 0.005, lambda tol: int(is_lease_stale(owner, tolerance=tol)))


_PROC_DEAD_CASES: tuple[tuple[str, _ProcKw, bool], ...] = (
    ("live-running", {"running": True}, False),
    ("zombie", {"status": psutil.STATUS_ZOMBIE}, True),
    ("dead-status", {"status": psutil.STATUS_DEAD}, True),
    ("no-such-process", {"raise_no_such": True}, True),
)


@pytest.mark.parametrize("label, proc_kw, expected", _PROC_DEAD_CASES, ids=[c[0] for c in _PROC_DEAD_CASES])
def test_proc_dead_truth_table(label: str, proc_kw: _ProcKw, expected: bool, monkeypatch: pytest.MonkeyPatch) -> None:  # noqa: FBT001
    """``proc_dead``: zombie/dead/vanished pids are dead, a running pid is live — the shared ownership-liveness ladder."""
    _ = label
    monkeypatch.setattr(govern_mod, "psutil", _make_psutil_module({4242: _proc(pid=4242, **proc_kw)}))
    assert proc_dead(4242) is expected


def test_liveness_access_denied_defers_to_pid_exists(monkeypatch: pytest.MonkeyPatch) -> None:
    """``AccessDenied`` proves the OS still owns the pid: both liveness ladders defer to ``pid_exists``; corrupt pids fold dead.

    ``psutil.Process(pid<=0)`` raises ``ValueError``; a corrupt marker/owner pid folds to dead, never raises.
    """
    guarded = _proc(pid=4242)
    guarded.status.side_effect = psutil.AccessDenied(pid=4242)
    fake = _make_psutil_module({4242: guarded})
    monkeypatch.setattr(govern_mod, "psutil", fake)
    fake.pid_exists.return_value = True
    assert proc_dead(4242) is False
    fake.pid_exists.return_value = False
    assert proc_dead(4242) is True
    assert proc_dead(-1) is True

    holder = _proc(pid=99999, running=True, create_time=_CT)
    holder.create_time.side_effect = psutil.AccessDenied(pid=99999)
    fake_stale = _make_psutil_module({99999: holder})
    fake_stale.pid_exists.return_value = True
    monkeypatch.setattr(govern_mod, "psutil", fake_stale)
    owner = govern_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT)
    assert is_lease_stale(owner, tolerance=1.0) is False


# --- [GOVERNED_CONCURRENCY]


@pytest.mark.parametrize(
    "cpu_count, max_checks, dotnet, mutation, runner_modes, expected",
    [
        (4, 8, 4, 4, (), 4),  # cpu_count caps below max_checks
        (4, 2, 4, 4, (), 2),  # max_checks caps below cpu_count
        (8, 8, 8, 8, (), 8),  # equal axes
        (1, 8, 8, 8, (), 1),  # a single usable cpu floors the limit at 1
        (4, 8, 2, 8, ((Runner.DOTNET, Mode.CHECK),), 2),  # a dotnet check engages the runner cap
        (4, 8, 8, 1, ((Runner.DIRECT, Mode.MUTATION),), 1),  # a mutation batch engages the mode cap
        (4, 8, 2, 3, ((Runner.DOTNET, Mode.MUTATION),), 2),  # dotnet(2) ∩ mutation(3) ⇒ 2
    ],
)
def test_governed_concurrency_cap_table(
    cpu_count: int,
    max_checks: int,
    dotnet: int,
    mutation: int,
    runner_modes: tuple[tuple[Runner, Mode], ...],
    expected: int,
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """``governed_concurrency`` folds the cpu / dotnet-runner / mutation-mode ceilings into one floor ≥ 1."""
    # Host pressure is pinned below every ceiling so the cap table stays deterministic.
    monkeypatch.setattr(govern_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=50.0))
    monkeypatch.setattr(govern_mod, "_foreign_dotnet_count", lambda: 0)
    settings = assay_root.settings.model_copy(
        update={"cpu_count": cpu_count, "max_checks": max_checks, "dotnet_max_cpu": dotnet, "mutation_max_cpu": mutation}
    )
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=r, mode=m)) for r, m in runner_modes)
    assert governed_concurrency(settings, checks) == expected


def test_governed_concurrency_halves_under_pressure_sources(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, log_events: list[dict[str, object]]
) -> None:
    """≥ 90% system RAM halves any batch (floor 1); a foreign dotnet census at/above ``cpu_count`` halves DOTNET batches only.

    Each pressured fold emits one ``concurrency.backpressure`` event carrying the pressure-source fields.
    """
    monkeypatch.setattr(govern_mod, "_foreign_dotnet_count", lambda: 0)
    monkeypatch.setattr(govern_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=92.5))
    wide = assay_root.settings.model_copy(update={"cpu_count": 8, "max_checks": 8, "dotnet_max_cpu": 8})
    narrow = assay_root.settings.model_copy(update={"cpu_count": 1, "max_checks": 8})
    assert governed_concurrency(wide, ()) == 4, "pressured limit did not halve"
    assert governed_concurrency(narrow, ()) == 1, "halving broke the floor of 1"
    monkeypatch.setattr(govern_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=89.9))
    assert governed_concurrency(wide, ()) == 8, "sub-ceiling memory must not halve"
    events = tuple(e for e in log_events if e.get("event") == "concurrency.backpressure")
    assert len(events) == 2, f"backpressure telemetry not emitted once per pressured fold: {events!r}"
    assert (events[0].get("limit"), events[0].get("backpressure_limit"), events[0].get("sys_mem_percent")) == (8, 4, 92.5)

    monkeypatch.setattr(govern_mod, "_foreign_dotnet_count", lambda: 8)
    dotnet_batch = (Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=Runner.DOTNET)),)
    assert governed_concurrency(wide, dotnet_batch) == 4, "census at cpu_count did not halve a DOTNET batch"
    assert governed_concurrency(wide, ()) == 8, "census must not throttle a batch with no DOTNET runner"
    monkeypatch.setattr(govern_mod, "_foreign_dotnet_count", lambda: 7)
    assert governed_concurrency(wide, dotnet_batch) == 8, "sub-threshold census must not halve"
    census_event = next(e for e in log_events if e.get("event") == "concurrency.backpressure" and e.get("dotnet_pressure"))
    assert (census_event.get("foreign_dotnet"), census_event.get("mem_pressure")) == (8, False)


# assay_root is read-only (model_copy only), so function_scoped_fixture is suppressed.
@hyp_settings(parent=hyp_settings.get_profile("rasm"), suppress_health_check=[HealthCheck.function_scoped_fixture])
@given(cpu=st.integers(min_value=1, max_value=256), maxc=st.integers(min_value=1, max_value=64))
def test_governed_concurrency_bounded_in_unit_to_cpu(cpu: int, maxc: int, assay_root: AssayHarness) -> None:
    """For any settings the empty-batch limit lies in ``[1, min(max_checks, cpu_count)]`` — never 0, never unbounded."""
    settings = assay_root.settings.model_copy(update={"cpu_count": cpu, "max_checks": maxc})
    limit = governed_concurrency(settings, ())
    assert limit == IsInt(ge=1, le=min(maxc, cpu)), f"limit {limit} escaped [1, {min(maxc, cpu)}]"


# --- [DRAIN_STREAM_CAPTURED]


@given(chunks=st.lists(st.binary(max_size=64), max_size=24).map(tuple))
def test_drain_stream_aggregates_tail_size_and_lines(chunks: tuple[bytes, ...]) -> None:
    """A stdout drain under the spill cap carries the full payload while aggregating size and line totals."""
    whole = b"".join(chunks)
    captured = anyio.run(lambda: drain_stream(_recv_of(chunks), tail_cap=16, spill_cap=1 << 20, kind="out"))
    expected_lines = whole.count(b"\n") + (1 if whole and not whole.endswith(b"\n") else 0)
    assert (captured.full, captured.spilled, captured.size, captured.lines) == (whole, False, len(whole), expected_lines), (
        f"drain aggregate wrong: {captured!r}"
    )


class _ListSink:
    """Concrete ``WriteSink`` recording the full unclipped drain stream."""

    def __init__(self) -> None:
        self.chunks: list[bytes] = []

    def write(self, payload: bytes) -> object:
        self.chunks.append(payload)
        return len(payload)


@given(chunks=st.lists(st.binary(min_size=1, max_size=48), min_size=1, max_size=16).map(tuple))
def test_write_sink_receives_every_drained_chunk(chunks: tuple[bytes, ...]) -> None:
    """The tee sink receives the full stream while the captured stderr preview remains bounded."""
    sink: WriteSink = _ListSink()
    whole = b"".join(chunks)
    target(float(len(whole)), label="drained_bytes")
    captured = anyio.run(lambda: drain_stream(_recv_of(chunks), tail_cap=8, spill_cap=1 << 20, kind="err", sink=sink))
    assert isinstance(sink, _ListSink)
    assert (b"".join(sink.chunks), captured.size, captured.preview) == (whole, len(whole), whole[-8:]), "sink/capture lost or clipped a chunk"


def test_drain_stream_terminus_and_empty_rows() -> None:
    """Deterministic newline-terminus rows plus the empty-EOF and absent-pipe arms property seeds may miss."""
    rows: tuple[tuple[tuple[bytes, ...], int], ...] = (((b"ab\n",), 1), ((b"ab",), 1), ((b"ab\n", b""), 1), ((b"a\nb",), 2))
    for chunks, expected_lines in rows:
        captured = anyio.run(functools.partial(drain_stream, _recv_of(chunks), tail_cap=16, spill_cap=1 << 20, kind="out"))
        assert captured.lines == expected_lines, f"{chunks!r} drained to {captured.lines} lines, expected {expected_lines}"
    empty = anyio.run(lambda: drain_stream(_recv_of(()), tail_cap=16, spill_cap=1 << 20, kind="out", path="art/out.log"))
    assert empty == Captured(path="art/out.log"), "empty EOF did not drain to the zero capture with its path preserved"
    # recv_anyio(None, ...) is the inherited-fd / absent-pipe arm: white-box, no public driver reaches it with None.
    assert anyio.run(lambda: drain_stream(recv_anyio(None, 32), tail_cap=128, spill_cap=1 << 20)) == Captured()


def _spill_plan(assay_root: AssayHarness, scope: ArtifactScope, spill_cap: int) -> ExecPlan:
    """Build a scoped ``ExecPlan`` with a small spill cap for capture-boundary laws.

    Returns:
        Streaming plan whose ``spill_cap`` forces the inline/spill split at a unit-testable threshold.
    """
    return ExecPlan(
        argv=("/bin/echo", "spill-law"),
        check=Check(tool=_stream_tool("spill-law", ("/bin/echo", "x"))),
        cwd=str(assay_root.root),
        env={},
        settings=assay_root.settings,
        scope=scope,
        streaming=True,
        tail_cap=assay_root.settings.stream_tail_bytes,
        spill_cap=spill_cap,
        chunk=assay_root.settings.stream_chunk_bytes,
        thread_limiter=None,
    )


@pytest.mark.parametrize("size, expect_spill", [(64, False), (65, True)], ids=("at-cap-inline", "over-cap-spills"))
def test_capture_spill_boundary_is_strict_greater_than(size: int, expect_spill: bool, assay_root: AssayHarness) -> None:  # noqa: FBT001
    """At exactly ``spill_cap`` capture stays inline; one byte past it spills and ``read`` resolves from the store.

    White-box: the non-streaming ``_capture_payload`` and streaming ``drain_stream`` paths share one spill predicate.
    """
    spill_cap, scope = 64, assay_root.scope(Claim.STATIC)
    payload = b"x" * size
    plan = _spill_plan(assay_root, scope, spill_cap)
    captured = govern_mod._capture_payload(plan, "out", payload)
    assert captured.spilled is expect_spill, f"_capture_payload spill verdict wrong at size={size}: {captured!r}"
    assert (captured.full == b"") is expect_spill, f"inline ``full`` retained iff not spilled: {captured!r}"
    assert captured.read(scope.store) == payload, "capture-payload read did not resolve the full payload"

    path, handle = govern_mod._stream_writer(plan, "out")
    assert handle is not None, "scoped plan must return a real _WriteContext"
    with handle as sink:
        drained = anyio.run(
            functools.partial(drain_stream, _recv_of((payload,)), tail_cap=spill_cap, spill_cap=spill_cap, kind="out", sink=sink, path=path)
        )
    assert drained.spilled is expect_spill, f"drain spill verdict wrong at size={size}: {drained!r}"
    assert (drained.full == b"") is expect_spill, f"drain inline ``full`` retained iff not spilled: {drained!r}"
    assert drained.read(scope.store) == payload, "drain read did not resolve the full payload"


def test_capture_inline_read_never_touches_store(assay_root: AssayHarness) -> None:
    """An at-cap inline capture resolves ``read`` from ``full`` without dereferencing the store."""
    captured = Captured(full=b"x" * 64, spilled=False, preview=b"x" * 64, path="art/out.log", size=64, lines=1)
    assert captured.read(None) == b"x" * 64, "inline read must not require a store"


def test_stream_writer_rejects_non_context_backend_handle(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_stream_writer`` enforces the ``_WriteContext`` backend handle contract — bad handles fail loudly, never drop bytes."""
    scope = assay_root.scope(Claim.STATIC)
    monkeypatch.setattr(type(scope.store), "open_write", lambda _self, *_parts: ("art/path", object()))
    with pytest.raises(TypeError, match="non-context writer"):
        govern_mod._stream_writer(_spill_plan(assay_root, scope, 64), "out")


# --- [STREAMING_LOCAL]

_STREAM_LOCAL: tuple[tuple[str, tuple[str, ...], Language, bytes, bool], ...] = (
    ("scoped-persists-full", ("/bin/echo", "stream-ok"), Language.CSHARP, b"stream-ok\n", True),
    ("8kib-full-payload-persists", (sys.executable, "-c", "import sys; sys.stdout.write('x' * 8192)"), Language.PYTHON, b"x" * 8192, True),
    ("noscope-full-inline-no-artifact", ("/bin/echo", "stream-ok"), Language.CSHARP, b"stream-ok\n", False),
)


@pytest.mark.parametrize("label, command, language, payload, scoped", _STREAM_LOCAL, ids=[c[0] for c in _STREAM_LOCAL])
def test_streaming_local_receipt_carries_full_payload_and_artifact(
    label: str,
    command: tuple[str, ...],
    language: Language,
    payload: bytes,
    scoped: bool,  # noqa: FBT001
    assay_root: AssayHarness,
) -> None:
    """Streaming local tools resolve the receipt stdout to the full sub-cap payload, persisting the scoped artifact.

    ``scope=None`` is the no-sink tee arm carrying the full payload inline; the scoped rows round-trip the artifact.
    """
    scope = assay_root.scope(Claim.STATIC) if scoped else None
    done = assert_ok(_run(Check(tool=_stream_tool(f"{label}-tool", command, language)), assay_root, scope=scope))
    assert done.stdout == payload, f"receipt stdout not the full payload: len={len(done.stdout)}"
    artifact = next((a for a in done.artifacts if a.id.startswith(f"{label}-tool-") and a.id.endswith("-out")), None)
    match scope:
        case None:
            assert done.artifacts == (), f"scope-less stream emitted artifacts: {done.artifacts!r}"
        case _:
            assert artifact is not None, f"no persisted stdout artifact in {done.artifacts!r}"
            assert scope.store.read_path(artifact.path) == payload, "persisted artifact lost the streamed bytes"


def test_nonstreaming_scoped_process_persists_output_artifacts(assay_root: AssayHarness) -> None:
    """Scoped non-streaming tools persist full stdout/stderr artifacts while the receipt carries the full sub-cap payload."""
    payload = b"x" * (assay_root.settings.stream_tail_bytes + 8)
    script = f"import sys; sys.stdout.buffer.write({payload!r}); sys.stderr.buffer.write(b'err-tail')"
    tool = Tool("nonstream-artifact-law", Runner.DIRECT, (sys.executable, "-c", script), Input.NONE, Language.PYTHON, Claim.STATIC)
    scope = assay_root.scope(Claim.STATIC)
    done = assert_ok(_run(Check(tool=tool), assay_root, scope=scope))
    assert done.stdout == payload
    artifact = next((a for a in done.artifacts if a.id.startswith("nonstream-artifact-law-") and a.id.endswith("-out")), None)
    assert artifact is not None, f"non-streaming process emitted no stdout artifact: {done.artifacts!r}"
    assert scope.store.read_path(artifact.path) == payload
    # The non-streaming receipt carries the unified _measure key set, child-tree rows included, matching the streaming path.
    keys = {name for name, _ in done.resources}
    assert {"proc.children", "proc.children_rss_bytes", "process.duration_ms"} <= keys, f"non-streaming receipt key set drifted: {sorted(keys)!r}"


@pytest.mark.parametrize("mode", [Mode.RUN, Mode.VERIFY], ids=["nonstream", "stream"])
def test_provision_process_suppresses_raw_artifacts(mode: Mode, assay_root: AssayHarness) -> None:
    """Provision claim output remains in the receipt for parsing but never persists raw PROCESS artifacts."""
    payload = b'{"schemaVersion":2,"command":"status","ok":true}\n'
    script = f"import sys; sys.stdout.buffer.write({payload!r}); sys.stderr.buffer.write(b'raw-log')"
    tool = Tool("provision-redaction-law", Runner.DIRECT, (sys.executable, "-c", script), Input.NONE, Language.PYTHON, Claim.PROVISION, mode=mode)
    done = assert_ok(_run(Check(tool=tool), assay_root, scope=assay_root.scope(Claim.PROVISION)))
    assert done.stdout == payload
    assert done.artifacts == ()


def test_streaming_process_emits_progress_and_receipt_resources(assay_root: AssayHarness, log_events: list[dict[str, object]]) -> None:
    """Streaming local processes emit process/resource events and persist sampled resource rows on the receipt."""
    tool = _stream_tool(
        "resource-stream-law", (sys.executable, "-c", "import sys,time; print('ready'); sys.stdout.flush(); time.sleep(0.05)"), Language.PYTHON
    )
    done = assert_ok(_run(Check(tool=tool), assay_root))
    events = tuple(event.get("event") for event in log_events)
    assert "process.start" in events
    assert "resource.sample" in events
    assert "process.end" in events
    assert any(name == "proc.children" for name, _ in done.resources)


# --- [STALL_TELEMETRY]


def test_stall_verdict_triage_table() -> None:
    """Stall triage precedence is cpu-bound, disk-wait, scheduler-contention, then quiet.

    Rows pin one-core rendering, the inclusive contention boundary, and the negative-delta quiet clamp.
    """
    window = govern_mod._STALL_SAMPLE_S

    def sample(cpu_s: float = 0.0, invol: float = 0.0, status: str = psutil.STATUS_RUNNING, procs: int = 1) -> StalledProcess:
        return StalledProcess(cpu_s=cpu_s, invol=invol, status=status, procs=procs)

    idle = sample()
    rows: tuple[tuple[str, StalledProcess, StalledProcess, str], ...] = (
        ("cpu-bound", idle, sample(cpu_s=window, procs=3), "cpu-bound (100% of one core, 3 procs)"),
        (
            "cpu-bound-precedes-disk-wait",
            idle,
            sample(cpu_s=window, status=psutil.STATUS_DISK_SLEEP, procs=2),
            "cpu-bound (100% of one core, 2 procs)",
        ),
        ("disk-wait", idle, sample(status=psutil.STATUS_DISK_SLEEP), "disk-wait"),
        ("scheduler-contention-inclusive-boundary", idle, sample(invol=window * 100.0), "scheduler-contention"),
        ("io-or-lock-wait", idle, sample(), "io-or-lock-wait"),
        ("negative-delta-clamps-quiet", sample(cpu_s=9.0, invol=9_999.0, procs=4), idle, "io-or-lock-wait"),
    )
    for label, first, second, expected in rows:
        assert govern_mod._stall_verdict(first, second) == expected, f"{label}: verdict drifted"


def test_stall_sample_aggregates_process_tree(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_stall_sample`` sums ``cpu_times`` (user+system) and involuntary switches across root + recursive children.

    Per-process probe faults degrade to zero while the rest of the process tree still counts.
    """
    root, kid, flaky = _proc(pid=777), _proc(pid=778), _proc(pid=779)
    root.cpu_times.return_value = SimpleNamespace(user=1.0, system=0.5)
    kid.cpu_times.return_value = SimpleNamespace(user=2.0, system=0.25)
    root.num_ctx_switches.return_value = SimpleNamespace(voluntary=1, involuntary=10)
    kid.num_ctx_switches.return_value = SimpleNamespace(voluntary=2, involuntary=32)
    flaky.cpu_times.side_effect = psutil.Error("times down")
    flaky.num_ctx_switches.side_effect = NotImplementedError("off-platform probe")
    root.children.return_value = (kid, flaky)
    monkeypatch.setattr(govern_mod, "psutil", _make_psutil_module({777: root}))
    sample = govern_mod._stall_sample(777)
    expected = StalledProcess(cpu_s=3.75, invol=42.0, status=psutil.STATUS_RUNNING, procs=3)
    assert sample == expected, f"tree aggregate wrong: {sample!r}"
    empty = govern_mod._stall_sample(2_147_483_646)
    assert empty == StalledProcess(cpu_s=0.0, invol=0.0, status="", procs=0), f"vanished pid did not degrade: {empty!r}"


_STALL_RUNS: tuple[tuple[str, tuple[str, ...], bool], ...] = (
    ("silent-slow-notes-once", (sys.executable, "-c", "import time; time.sleep(1.2)"), True),
    ("fast-child-no-note", ("/bin/echo", "fast-ok"), False),
)


@pytest.mark.parametrize("label, command, stalled", _STALL_RUNS, ids=[c[0] for c in _STALL_RUNS])
def test_stall_monitor_shrunk_constant_matrix(
    label: str,
    command: tuple[str, ...],
    stalled: bool,  # noqa: FBT001
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    log_events: list[dict[str, object]],
    otel_spans: InMemorySpanExporter,
) -> None:
    """Shrunk-constant stall matrix over the LOCAL streaming branch.

    A silent slow child emits one receipt note plus log/span events; a fast child emits none.
    """
    monkeypatch.setattr(govern_mod, "_STALL_AFTER_S", 0.2)
    monkeypatch.setattr(govern_mod, "_STALL_SAMPLE_S", 0.05)
    done = assert_ok(_run(Check(tool=_stream_tool(f"{label}-tool", command, Language.PYTHON)), assay_root))
    notes = tuple(n for n in done.notes if n.startswith("proc.stall"))
    events = tuple(e for e in log_events if e.get("event") == "proc.stall")
    span_hits = tuple(ev for span in otel_spans.get_finished_spans() for ev in span.events if ev.name == "proc.stall")
    match stalled:
        case True:
            assert len(notes) == 1, f"expected exactly one stall note, got {done.notes!r}"
            assert (len(events), len(span_hits)) == (1, 1), f"stall telemetry not emitted exactly once: {events!r} / {span_hits!r}"
        case False:
            assert (notes, events, span_hits) == ((), (), ()), f"fast child produced spurious stall telemetry: {done.notes!r} {events!r}"


def test_resource_projection_aggregates_receipts_and_notes(assay_root: AssayHarness) -> None:
    """Resource projection folds pressure, waits, stalls, durations, and child metrics into rows."""
    check = Check(tool=msgspec.structs.replace(_REMOTE_TOOL, mode=Mode.BUILD))
    done = msgspec.structs.replace(
        receipt(("dotnet", "build"), 1, status=RailStatus.FAILED, duration_ms=42.0, notes=("proc.stall silent=30s io-or-lock-wait",)),
        resources=(("proc.children_rss_bytes", 128.0), ("proc.dotnet.count", 2.0), ("proc.last_output_age_s", 7.0)),
    )
    rows = dict(resource_projection(assay_root.settings, (check,), notes=("dotnet.slot index=0 wait_ms=17", *done.notes), receipts=(done,)))
    assert rows["dotnet.slot_wait_ms.max"] == pytest.approx(17.0)
    assert rows["proc.stall.count"] == pytest.approx(1.0)
    assert rows["process.duration_ms.max"] == pytest.approx(42.0)
    assert rows["proc.children_rss_bytes.max"] == pytest.approx(128.0)
    assert rows["proc.dotnet.count.max"] == pytest.approx(2.0)
    assert rows["proc.last_output_age_s.max"] == pytest.approx(7.0)


# --- [DOTNET_SLOT]


@pytest.mark.anyio
async def test_dotnet_slot_surfaces_queue_and_pressure_note(assay_root: AssayHarness) -> None:
    """The machine-wide dotnet slot pool emits the slot, wait, census, and concurrency decision as receipt notes."""
    async with dotnet_slot(Check(tool=_REMOTE_TOOL), assay_root.settings, None) as acquired:
        notes = assert_ok(acquired)
    joined = " ".join(notes)
    for token in ("dotnet.slot", "wait_ms=", "slots=", "foreign_dotnet=", "mem_percent=", "original_concurrency=", "reduced_concurrency="):
        assert token in joined, f"slot note evidence missing {token!r}: {joined!r}"


# --- [EXCLUSIVE_LEASE_LEASED]


@pytest.mark.mutation
def test_exclusive_lease_mutual_exclusion_and_owner_block(assay_root: AssayHarness) -> None:
    """A second acquire of a live resource is BUSY; the held lock carries the full owner block; release frees the resource."""
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "res.lock")
    with exclusive_lease("res", "run-a", settings=assay_root.settings, project="proj", mode="exclusive") as first:
        assert_ok(first)
        owner = decode_lease_owner(lock_path.read_bytes())
        assert owner is not None, "owner block not written while the lease was held"
        expected = {
            "resource": "res",
            "run_id": "run-a",
            "cwd": str(assay_root.root),
            "pid": os.getpid(),
            "project": "proj",
            "mode": "exclusive",
            "create_time": IsPositiveFloat(),
        }
        assert msgspec.structs.asdict(owner) == IsPartialDict(expected), f"owner block fields wrong: {owner!r}"
        with exclusive_lease("res", "run-b", settings=assay_root.settings) as second:
            assert_error_status(second, RailStatus.BUSY)
    with exclusive_lease("res", "run-c", settings=assay_root.settings) as third:
        assert_ok(third)


def test_leased_runs_action_only_when_held(assay_root: AssayHarness) -> None:
    """``leased`` runs the action and returns its value while the lease is free; a contended lease short-circuits to BUSY."""
    token = receipt(("held",), 0)
    first = leased("act", lambda _held: Ok(token), settings=assay_root.settings, run_id="run-a")
    assert assert_ok(first) is token
    # A contended action returns FAULTED only if the BUSY short-circuit is broken.
    breach = Fault((), status=RailStatus.FAULTED, message="action ran under a held lease")
    with exclusive_lease("act", "holder", settings=assay_root.settings) as guard:
        assert_ok(guard)
        contended = leased("act", lambda _held: Error(breach), settings=assay_root.settings, run_id="run-b")
        assert_error_status(contended, RailStatus.BUSY)


def test_exclusive_lease_rejects_non_local_backend(tmp_path: Path) -> None:
    """A non-file (``memory://``) backend cannot host a POSIX ``fcntl.flock`` lease — UNSUPPORTED before any fd opens."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath("memory://assay-lease-backend"), exec_known_hosts=None)
    with exclusive_lease("res", "run", settings=settings) as outcome:
        fault = assert_error_status(outcome, RailStatus.UNSUPPORTED)
    assert "POSIX leases require a local artifact root" in fault.message, f"wrong backend message: {fault!r}"


def test_leased_maps_lease_io_error_to_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Lease acquisition ``OSError`` becomes FAULTED and prevents action execution."""
    ran = [False]

    @contextlib.contextmanager
    def _boom(*_args: object, **_kwargs: object) -> Iterator[object]:
        if not ran[0]:  # always True here; the branch keeps `yield` reachable for the generator typing
            raise OSError("lease fs gone")
        yield None  # pragma: no cover

    monkeypatch.setattr(govern_mod, "exclusive_lease", _boom)

    def _action(_held: object) -> Result[object, Fault]:
        ran[0] = True
        return Ok(1)

    outcome = leased("res", _action, settings=assay_root.settings, run_id="run")
    assert_error_status(outcome, RailStatus.FAULTED)
    assert ran[0] is False, "action ran despite a lease acquisition fault"


# --- [STATEFUL_LEASE_RBSM]


class _LeaseSlot(msgspec.Struct, frozen=True, gc=False):
    """Entered lease slot with stack ownership and live-holder state."""

    stack: contextlib.ExitStack
    is_ok: bool


class LeaseStateMachine(RuleBasedStateMachine):
    """Stateful model of the synchronous ``exclusive_lease`` mutual-exclusion algebra.

    Each acquire owns one ``ExitStack`` for the shared resource; releases close only that token's stack.
    The invariant proves at most one Ok holder is live across all entered leases.
    """

    held = Bundle("held")

    def __init__(self) -> None:
        """Initialise an empty slot registry and minimal artifact root for the POSIX flock surface."""
        super().__init__()
        self._slots: dict[int, _LeaseSlot] = {}
        root = UPath(tempfile.mkdtemp(prefix="assay-lease-rbsm-"))
        (root / "Workspace.slnx").write_text("", encoding="utf-8")
        self._settings = AssaySettings(root=root, exec_known_hosts=None)

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
        # tuple() forces close side effects; ExitStack.close() returns None.
        _ = tuple(slot.stack.close() for slot in self._slots.values())


def test_lease_state_machine_holds_mutual_exclusion() -> None:
    """The synchronous lease RBSM permits at most one live Ok holder across interleavings."""
    model_based(LeaseStateMachine)


# --- [LEASE_CLAIM]


@contextlib.contextmanager
def _lock_fd(path: UPath | Path, *, exclusive: bool = False, seed: bytes | None = None) -> Iterator[int]:
    """Open a lock-file fd (parents created), optionally seed it and take ``LOCK_EX``, always releasing + closing on exit.

    The unconditional unlock also releases the steal arm, whose ``_steal`` flocks the yielded fd.

    Yields:
        Open fd, flocked when ``exclusive=True`` and otherwise free for ``_steal``.
    """
    path.parent.mkdir(parents=True, exist_ok=True)
    fd = os.open(str(path), os.O_RDWR | os.O_CREAT, 0o644)
    _ = os.write(fd, seed) if seed is not None else 0
    match exclusive:
        case True:
            fcntl.flock(fd, fcntl.LOCK_EX)  # ty: ignore[possibly-missing-attribute]
        case False:
            pass
    try:
        yield fd
    finally:
        fcntl.flock(fd, fcntl.LOCK_UN)  # ty: ignore[possibly-missing-attribute]
        os.close(fd)


_HELD_FLOCK: tuple[tuple[str, bool], ...] = (("populated-body", True), ("empty-body", False))


@pytest.mark.mutation
@pytest.mark.parametrize("label, populate", _HELD_FLOCK, ids=[c[0] for c in _HELD_FLOCK])
def test_claim_held_flock_is_busy(label: str, populate: bool, assay_root: AssayHarness) -> None:  # noqa: FBT001
    """A live holder under a sibling-held flock maps the contender to ``BUSY`` (never a steal).

    Populated and empty mid-write bodies both read as live contention under ``LOCK_EX``.
    """
    owner = govern_mod._LeaseOwner(resource=label, run_id="holder", pid=os.getpid(), create_time=time.time())
    seed = msgspec.json.encode(owner) if populate else None
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, f"{label}.lock")
    with _lock_fd(lock_path, exclusive=True, seed=seed), exclusive_lease(label, "contender", settings=assay_root.settings) as contended:
        assert_error_status(contended, RailStatus.BUSY)


@pytest.mark.mutation
def test_claim_steals_dead_holder_lock(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A lock owned by a dead pid (``NoSuchProcess``) is stolen — the full real-``flock`` reclaim path.

    The path pins stale detection, steal fallback, and rewritten owner-block evidence.
    """
    dead_pid, self_pid = 88_888, os.getpid()
    fake = _make_psutil_module({self_pid: _proc(pid=self_pid), dead_pid: _proc(pid=dead_pid, raise_no_such=True)})
    monkeypatch.setattr(govern_mod, "psutil", fake)
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "stale.lock")
    lock_path.parent.mkdir(parents=True, exist_ok=True)
    stale = govern_mod._LeaseOwner(resource="stale", run_id="old-run", pid=dead_pid, create_time=0.0)
    lock_path.write_bytes(msgspec.json.encode(stale))
    with exclusive_lease("stale", "new-run", settings=assay_root.settings) as reclaimed:
        owner = assert_ok(reclaimed).owner
    assert (owner.run_id, owner.pid) == ("new-run", self_pid), f"steal did not rewrite the owner block: {owner!r}"


def test_steal_rewrites_owner_and_yields_busy_on_lost_race(assay_root: AssayHarness) -> None:
    """``_steal`` re-locks an uncontended fd and rewrites the owner block; a lost TOCTOU race yields ``None`` (BUSY)."""
    prior = govern_mod._LeaseOwner(resource="steal-direct", run_id="dead", pid=88_888, create_time=0.0)
    with _lock_fd(assay_root.settings.artifact(ArtifactKind.LOCKS, "steal-direct.lock")) as fd:
        won = govern_mod._steal(fd, "steal-direct", run_id="winner", target="", owner=prior)
    assert won is not None, "_steal did not return a won owner on an uncontended lock"
    assert (won.run_id, won.pid) == ("winner", os.getpid()), f"_steal did not rewrite the owner: {won!r}"
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "steal-lost.lock")
    with _lock_fd(lock_path, exclusive=True), _lock_fd(lock_path) as contender:
        assert govern_mod._steal(contender, "steal-lost", run_id="loser", target="", owner=None) is None, "lost TOCTOU race did not yield BUSY"


@pytest.mark.mutation
def test_claim_contention_busy_vs_steal_decision(  # noqa: PLR0915  # three contention scenarios share one scripted flock + fd harness
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch
) -> None:
    """``_claim`` maps live contention to BUSY and stale contention to owner rewrite.

    The persisted owner block and flock count pin live/stale dispatch plus run/cwd/project/mode/target forwarding.
    """
    self_pid, live_pid, dead_pid = os.getpid(), 88_778, 88_777
    self_proc = _proc(pid=self_pid, create_time=_CT)
    fake = _make_psutil_module({
        None: self_proc,
        self_pid: self_proc,
        live_pid: _proc(pid=live_pid, running=True, create_time=_CT),
        dead_pid: _proc(pid=dead_pid, raise_no_such=True),
    })
    monkeypatch.setattr(govern_mod, "psutil", fake)
    flock_calls: list[int] = []
    busy_first = [False]

    def scripted_flock(_fd: int, flags: int) -> None:
        flock_calls.append(flags)
        if busy_first[0] and len(flock_calls) == 1:
            raise BlockingIOError

    monkeypatch.setattr(govern_mod, "_FLOCK", scripted_flock)

    def claim(name: str, holder_pid: int | None, *, busy: bool) -> tuple[govern_mod._LeaseOwner | None, govern_mod._LeaseOwner | None, int]:
        flock_calls.clear()
        busy_first[0] = busy
        fd = os.open(str(assay_root.root / f"{name}.lock"), os.O_RDWR | os.O_CREAT, 0o644)
        try:
            if holder_pid is not None:
                _ = os.write(fd, msgspec.json.encode(govern_mod._LeaseOwner(resource=name, run_id="holder", pid=holder_pid, create_time=_CT)))
                _ = os.lseek(fd, 0, os.SEEK_SET)
            won = govern_mod._claim(
                fd, name, run_id="claim-run", tolerance=1.0, target="ssh://probe", cwd="/work/claim", project="proj-claim", mode="shared"
            )
            _ = os.lseek(fd, 0, os.SEEK_SET)
            return won, decode_lease_owner(os.read(fd, 4096)), len(flock_calls)
        finally:
            os.close(fd)

    def stamped(resource: str) -> dict[str, object]:
        return {
            "resource": resource,
            "run_id": "claim-run",
            "pid": self_pid,
            "create_time": _CT,
            "cwd": "/work/claim",
            "project": "proj-claim",
            "mode": "shared",
            "target": "ssh://probe",
        }

    won, persisted, flocks = claim("claim-free", None, busy=False)
    assert persisted is not None, "free-path acquire wrote no owner block"
    assert won == persisted, f"free-path return diverged from the persisted block: {won!r}"
    assert flocks == 1, "free flock did not acquire on the first attempt"
    assert msgspec.structs.asdict(persisted) == IsPartialDict(stamped("claim-free")), f"free-path owner block wrong: {persisted!r}"
    won, persisted, flocks = claim("claim-busy", live_pid, busy=True)
    assert persisted is not None, "live holder block vanished under contention"
    assert (won, flocks, persisted.run_id) == (None, 1, "holder"), "live holder must map to BUSY without a steal flock or rewrite"
    won, persisted, flocks = claim("claim-stale", dead_pid, busy=True)
    assert persisted is not None, "steal wrote no owner block"
    assert won == persisted, f"steal return diverged from the persisted block: {won!r}"
    assert flocks == 2, "stale holder must fall through to the steal flock"
    assert msgspec.structs.asdict(persisted) == IsPartialDict(stamped("claim-stale")), f"steal did not rewrite the full owner block: {persisted!r}"


# --- [REAP]


def test_reap_terminates_live_tree_and_passes_through_exited() -> None:
    """``reap`` kills live child trees and preserves already-exited return codes; ``_reap_tree`` walks a real tree via psutil.

    Unresolvable pids take the ``psutil.Error`` arm without raising.
    """

    async def _live() -> int | None:
        proc = await anyio.open_process([sys.executable, "-c", "import time; time.sleep(30)"])
        with pytest.MonkeyPatch.context() as patch:
            patch.setattr(govern_mod, "_reap_tree", lambda _pid: None)
            await reap(proc)
        return proc.returncode

    async def _exited() -> int | None:
        proc = await anyio.open_process([sys.executable, "-c", "pass"])
        await proc.wait()
        await reap(proc)
        return proc.returncode

    async def _tree() -> int | None:
        async with await anyio.open_process([sys.executable, "-c", "import time; time.sleep(30)"]) as proc:
            await anyio.to_thread.run_sync(govern_mod._reap_tree, proc.pid)  # ty: ignore[unresolved-attribute]
            with anyio.fail_after(5.0):
                await proc.wait()
            return proc.returncode

    assert anyio.run(_live) is not None, "live child not reaped to a terminal returncode"
    assert anyio.run(_exited) == 0, "exited child returncode not preserved through _reap"
    assert anyio.run(_tree) is not None, "live process not terminated by _reap_tree"
    govern_mod._reap_tree(2_147_483_646)


def test_terminate_process_tree_kills_terminate_resistant_child(log_events: list[dict[str, object]]) -> None:
    """SIGTERM then SIGKILL handles resistant survivors and emits each reaping phase."""
    survivor = _proc(pid=4242, running=True)
    already_dead = _proc(pid=4243, running=False)
    fake = _make_psutil_module({4242: survivor, 4243: already_dead})
    fake.wait_procs.side_effect = (((already_dead,), (survivor,)), ((survivor,), ()))
    with pytest.MonkeyPatch.context() as patch:
        patch.setattr(govern_mod, "psutil", fake)
        govern_mod._terminate_process_tree((already_dead, survivor), None)
    sent = tuple(call.args[0] for call in survivor.send_signal.call_args_list)
    assert sent == (signal.SIGTERM, signal.SIGKILL), f"ladder escalation wrong: {sent!r}"  # ty: ignore[possibly-missing-attribute]
    already_dead.send_signal.assert_not_called()
    ledger = tuple((e.get("killed"), e.get("survived")) for e in log_events if e.get("event") == "proc.reaped")
    assert ledger == ((1, 1), (1, 0)), f"reap ledger not raised through both wait phases: {ledger!r}"


def test_child_pgid_guards_engine_group() -> None:
    """``_child_pgid`` protects the engine group, vanished pids, and session-leader children."""
    assert govern_mod._child_pgid(os.getpid()) is None, "own process group must never be group-killed"
    assert govern_mod._child_pgid(2_147_483_646) is None, "vanished pid must degrade to the walk fallback"

    async def _spawn() -> tuple[int | None, int]:
        proc = await anyio.open_process([sys.executable, "-c", "import time; time.sleep(30)"], start_new_session=True)
        try:
            return govern_mod._child_pgid(proc.pid), proc.pid
        finally:
            await reap(proc)

    pgid, pid = anyio.run(_spawn)
    assert pgid == pid, "session-leader child must own its process group"


# --- [DIAGNOSE_MEASURE]


def test_diagnose_records_resource_snapshot(monkeypatch: pytest.MonkeyPatch) -> None:
    """``diagnose`` folds the real ``_measure`` chain onto the active span event and seeds ``RESOURCE`` for fault evidence."""
    fake = _make_psutil_module({None: _proc(rss=65536)})
    monkeypatch.setattr(govern_mod, "psutil", fake)
    events: list[tuple[str, dict[str, object]]] = []
    exceptions: list[BaseException] = []
    span = SimpleNamespace(record_exception=exceptions.append, add_event=lambda name, attributes: events.append((name, attributes)))
    monkeypatch.setattr(trace, "get_current_span", lambda: span)
    token = govern_mod.RESOURCE.set(())
    ring_token = RING.set(deque(("info:probe.start", "warning:probe.fault")))
    try:
        exc = TimeoutError("synthetic diagnose timeout")
        diagnose(exc)
        assert exceptions == [exc], "exception not recorded on the fault span"
        name, attrs = events[0]
        assert (name, attrs.get("mem.rss_bytes")) == (govern_mod._FAULT_SNAPSHOT, 65536.0), f"snapshot event wrong: {name!r} {attrs!r}"
        assert events[1] == (govern_mod._RING_SNAPSHOT, {"events": ("info:probe.start", "warning:probe.fault")}), (
            f"ring event not built from ring_recent(): {events[1]!r}"
        )
        rss = dict(govern_mod.RESOURCE.get())["mem.rss_bytes"]
        assert rss == pytest.approx(65536.0), "resource ContextVar not seeded from the snapshot"
    finally:
        RING.reset(ring_token)
        govern_mod.RESOURCE.reset(token)


def test_measure_pressure_arms_degrade_to_empty(monkeypatch: pytest.MonkeyPatch) -> None:
    """The measurement is best-effort evidence, never a second fault source.

    Memory, load, child, and file-descriptor arms degrade independently: a faulted source elides its keys.
    """
    self_proc = _proc(rss=4096)
    self_proc.children.side_effect = psutil.Error("children down")
    fake = _make_psutil_module({None: self_proc})
    fake.virtual_memory.side_effect = psutil.Error("vm down")
    fake.swap_memory.side_effect = psutil.Error("swap down")
    monkeypatch.setattr(govern_mod, "psutil", fake)
    fd_proc = _proc()
    fd_proc.num_fds.side_effect = NotImplementedError("no num_fds")

    def _fault() -> float:
        raise ValueError("probe down")

    support_matrix(
        ("load-info-mem-fault→load-only", lambda: set(govern_mod._load_info().to_rows()) == {"sys.load1_percent"}, True),
        ("measure-children-fault→elided", lambda: "proc.children" not in dict(measure().to_resources()), True),
        ("num-fds-not-implemented→default", lambda: govern_mod._safe_call(lambda: float(fd_proc.num_fds()), -1.0) == pytest.approx(-1.0), True),
        ("measure-keeps-num-fds-key", lambda: "proc.num_fds" in dict(measure().to_resources()), True),
        ("safe-call-value-arm", lambda: govern_mod._safe_call(lambda: 7.0, -1.0) == pytest.approx(7.0), True),
        ("safe-call-default-arm", lambda: govern_mod._safe_call(_fault, -1.0) == pytest.approx(-1.0), True),
    )


def test_load_info_load_arm_degrades_without_getloadavg(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_load_info`` omits ``sys.load1_percent`` when ``os.getloadavg`` is absent and when it raises ``OSError``."""

    def _boom() -> tuple[float, float, float]:
        raise OSError("load unavailable")

    monkeypatch.setattr(os, "getloadavg", _boom, raising=False)
    assert "sys.load1_percent" not in govern_mod._load_info().to_rows(), "failing getloadavg not degraded to an absent key"
    monkeypatch.delattr(os, "getloadavg", raising=False)
    assert "sys.load1_percent" not in govern_mod._load_info().to_rows(), "absent getloadavg not degraded to an absent key"


def test_measure_and_load_info_pin_exact_metric_projection(monkeypatch: pytest.MonkeyPatch) -> None:
    """``measure``/``_load_info`` project the exact key→value evidence map from a deterministic psutil double.

    Exact values pin key names, default degradation, RSS percentage arithmetic, child-tree fold, and loadavg index selection.
    """
    proc, kid = _proc(pid=os.getpid()), _proc(rss=512)
    proc.memory_info.return_value = SimpleNamespace(rss=2048, vms=4096)
    proc.memory_full_info.return_value = SimpleNamespace(uss=1024)
    proc.num_fds.return_value = 6
    proc.num_threads.return_value = 3
    proc.children.return_value = (kid,)
    fake = _make_psutil_module({None: proc})
    fake.virtual_memory.return_value = SimpleNamespace(total=8192, available=4096, percent=37.5)
    fake.swap_memory.return_value = SimpleNamespace(percent=12.5)
    monkeypatch.setattr(govern_mod, "psutil", fake)
    monkeypatch.setattr(os, "getloadavg", lambda: (2.0, 9.0, 9.0), raising=False)
    load = {"sys.mem_available_bytes": 4096.0, "sys.mem_percent": 37.5, "sys.swap_percent": 12.5, "sys.load1_percent": 50.0}
    assert govern_mod._load_info().to_rows() == load, "load projection drifted from the doubled sources"
    assert dict(measure().to_resources()) == {
        "mem.rss_bytes": 2048.0,
        "mem.vms_bytes": 4096.0,
        "mem.uss_bytes": 1024.0,
        "mem.percent_rss": 25.0,
        "proc.num_fds": 6.0,
        "proc.num_threads": 3.0,
        "proc.children": 1.0,
        "proc.children_rss_bytes": 512.0,
        **load,
    }, "measure projection drifted from the doubled process"
