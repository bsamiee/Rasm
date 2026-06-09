"""Engine laws: pure-fn algebra (decode/stale/governed/retry/splice/ssh/drain) + the lease RBSM.

Pure-function laws are oracle-driven via ``tests._spec`` (inverse / monotone / validity_matrix /
projection_matrix / support_matrix / assert_ok / assert_error_status). The synchronous ``exclusive_lease``
mutual-exclusion contract is held by the ported ``LeaseStateMachine`` RBSM. ``run_check`` / ``fan_out`` /
``discover`` are exercised through real subprocess / loopback / psutil-double seams (no mocks of the boundary).
``ByteRecv`` / ``WriteSink`` (Callable / Protocol aliases) and ``_RESOURCE`` (the resource ContextVar seam)
are law-less by nature and are carried as exemptions in the assay conftest ``_EXEMPT`` set.

``_LAWS`` registers all covered symbols at import time, fixing the MANIFEST total at collection time so
missing coverage is detected before any test runs.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import contextlib
import fcntl
from itertools import starmap
import os
import sys
import tempfile
import time
from types import SimpleNamespace
from typing import override, TYPE_CHECKING, TypedDict

import anyio
from dirty_equals import Contains, IsInt, IsPartialDict, IsPositiveFloat
from expression import Error, Ok
from hypothesis import given, HealthCheck, settings as hyp_settings, strategies as st, target
from hypothesis.stateful import Bundle, consumes, invariant, rule, RuleBasedStateMachine, run_state_machine_as_test
import msgspec
from opentelemetry import trace
import psutil
import pytest
from upath import UPath

from tests._aspect import register_law  # noqa: PLC2701  # _-prefixed test-internal surface; private import is intentional
from tests._spec import (  # noqa: PLC2701  # _-prefixed test-internal surface; private import is intentional
    assert_error,
    assert_error_status,
    assert_ok,
    monotone,
    projection_matrix,
    ProjectionCase,
    roundtrip,
    support_matrix,
    validity_matrix,
)

# AssayHarness/SshLoopback are runtime (not TYPE_CHECKING) imports: hypothesis evaluates @given fixture
# annotations under PEP 649 eval_str, so they must resolve at module runtime though they appear only in annotations.
from tests.tools.assay.conftest import _make_psutil_module, _proc, AssayHarness, fault_st, SshLoopback  # noqa: PLC2701, TC001
from tools.assay.composition.settings import AssaySettings
import tools.assay.core.engine as engine_mod
from tools.assay.core.engine import (
    argv_for,
    Captured,
    decode_lease_owner,
    discover,
    drain_stream,
    exclusive_lease,
    fan_out,
    governed_concurrency,
    is_lease_stale,
    leased,
    remote_command,
    retry_predicate,
    run_check,
    run_check_async,
    splice_command,
    ssh_outcome,
    WriteSink,
)
from tools.assay.core.model import ArtifactKind, Check, Claim, Fault, Input, Language, Mode, receipt, Runner, Stage, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import Iterator, Sequence
    from pathlib import Path

    from expression import Result

    from tools.assay.composition.settings import ArtifactScope
    from tools.assay.core.model import Completed


# --- [TYPES] ----------------------------------------------------------------------------


class _ProcKw(TypedDict, total=False):
    """Keyword payload accepted by the psutil process double for the staleness sweep."""

    raise_no_such: bool
    running: bool
    create_time: float


# --- [CONSTANTS] ------------------------------------------------------------------------

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
_PY_CHANGED = Routed(language=Language.PYTHON, scope=Scope.CHANGED)


# --- [LAW_COVERAGE]

_LAWS: tuple[tuple[object, str], ...] = (
    (decode_lease_owner, "decode_lease_owner_inverts_encode"),
    (is_lease_stale, "is_lease_stale_decision_table"),
    (governed_concurrency, "governed_concurrency_cap_table"),
    (retry_predicate, "retry_predicate_decision_table"),
    (splice_command, "splice_command_injects_scope_flags_for_dotnet_build_verbs"),
    (argv_for, "argv_for_composes_runner_prefix_scope_and_routed_tails"),
    (ssh_outcome, "ssh_outcome_projects_status_and_signal"),
    (remote_command, "remote_command_shell_quotes_cwd_env_argv"),
    (drain_stream, "drain_stream_aggregates_tail_size_and_lines"),
    (Captured, "captured_defaults_are_the_empty_aggregate"),
    (WriteSink, "write_sink_receives_every_drained_chunk"),
    (discover, "discover_maps_process_status_to_result"),
    (engine_mod.discover_async, "discover_async_via_discover"),
    (run_check, "run_check_executes_direct_tool"),
    (run_check_async, "run_check_async_is_the_event_loop_boundary"),
    (fan_out, "fan_out_preserves_order_and_backfills_timeout"),
    (exclusive_lease, "exclusive_lease_is_busy_under_a_live_holder"),
    (leased, "leased_runs_action_only_when_held"),
)
_ = tuple(starmap(register_law, _LAWS))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [HARNESS]


def _run(check: Check, harness: AssayHarness, *, scope: ArtifactScope | None = None) -> Result[Completed, Fault]:
    """Drive ``run_check`` against the harness, scope optional (default the no-sink local arm).

    Returns:
        The check outcome — Ok receipt or addressable Fault.
    """
    return run_check(check, settings=harness.settings, scope=scope, routed=_ROUTED_CHANGED)


def _stream_tool(name: str, command: tuple[str, ...], language: Language = Language.CSHARP) -> Tool:
    """A BUILD-mode DIRECT tool whose stdout the streaming backend tees to a bounded tail + artifact.

    Returns:
        A streaming-mode ``Tool`` carrying ``name``/``command``/``language``.
    """
    return Tool(name, Runner.DIRECT, command, Input.NONE, language, Claim.STATIC, mode=Mode.BUILD)


# --- [DECODE_OWNER]


def test_decode_lease_owner_inverts_encode() -> None:
    """``decode_lease_owner`` is the left inverse of ``msgspec.json.encode`` over a real owner block."""
    owner = engine_mod._LeaseOwner(resource="r", run_id="run-x", pid=4321, create_time=_CT, project="p", mode="exclusive")
    roundtrip(owner, msgspec.json.encode, lambda raw: decode_lease_owner(raw) or pytest.fail("decode_lease_owner lost a valid owner block"))


@given(data=st.binary(max_size=512))
def test_decode_lease_owner_is_total(data: bytes) -> None:
    """``decode_lease_owner`` never raises on arbitrary bytes — only ``None`` or a real ``_LeaseOwner``."""
    decoded = decode_lease_owner(data)
    assert decoded is None or isinstance(decoded, engine_mod._LeaseOwner)


def test_decode_lease_owner_corrupt_bytes_are_none() -> None:
    """Empty, truncated, and type-violating lock bytes all decode to ``None`` (a stealable absent holder)."""
    validity_matrix(
        (
            ("empty", b"", True),
            ("not-json", b"{not json", True),
            ("missing-required", b'{"resource": "x"}', True),
            ("pid-wrong-type", b'{"resource": "x", "pid": "not-an-int"}', True),
        ),
        lambda raw: decode_lease_owner(raw) is None,
    )


# --- [IS_LEASE_STALE]

_STALE_CASES: tuple[tuple[str, _ProcKw, bool], ...] = (
    ("no-such-process", {"raise_no_such": True}, True),
    ("not-running", {"running": False, "create_time": _CT}, True),
    ("live-and-matching", {"running": True, "create_time": _CT}, False),
    ("create-time-drift", {"running": True, "create_time": _CT + 5.0}, True),
)


@pytest.mark.parametrize("label, proc_kw, expected", _STALE_CASES, ids=[c[0] for c in _STALE_CASES])
def test_is_lease_stale_decision_table(label: str, proc_kw: _ProcKw, expected: bool, monkeypatch: pytest.MonkeyPatch) -> None:  # noqa: FBT001
    """``is_lease_stale``: dead/not-running/create-time-drift ⇒ True; live-and-matching ⇒ False."""
    _ = label
    fake = _make_psutil_module({None: _proc(), 99999: _proc(pid=99999, **proc_kw)})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    owner = engine_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT)
    assert is_lease_stale(owner, tolerance=1.0) is expected


@given(drift=st.floats(min_value=0.0, max_value=0.99))
def test_is_lease_stale_monotone_in_drift(drift: float) -> None:
    """Holding a live matching pid, staleness (0=fresh, 1=stale) is monotone in tolerance.

    A wider band never flips fresh→stale — the tolerance is an upper gate.
    """
    fake = _make_psutil_module({99999: _proc(pid=99999, running=True, create_time=_CT + drift)})
    owner = engine_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT)
    with pytest.MonkeyPatch.context() as patch:
        patch.setattr(engine_mod, "psutil", fake)
        monotone(drift + 0.005, drift - 0.005, lambda tol: int(is_lease_stale(owner, tolerance=tol)))


def test_is_lease_stale_access_denied_stays_live(monkeypatch: pytest.MonkeyPatch) -> None:
    """``AccessDenied`` proves the OS still owns the pid: do not steal while ``pid_exists`` is true."""
    proc = _proc(pid=99999, running=True, create_time=_CT)
    proc.create_time.side_effect = psutil.AccessDenied(pid=99999)
    fake = _make_psutil_module({99999: proc})
    fake.pid_exists.return_value = True
    monkeypatch.setattr(engine_mod, "psutil", fake)
    owner = engine_mod._LeaseOwner(resource="r", run_id="x", pid=99999, create_time=_CT)
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
) -> None:
    """``governed_concurrency`` folds the cpu / dotnet-runner / mutation-mode ceilings into one floor ≥ 1."""
    settings = assay_root.settings.model_copy(
        update={"cpu_count": cpu_count, "max_checks": max_checks, "dotnet_max_cpu": dotnet, "mutation_max_cpu": mutation}
    )
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=r, mode=m)) for r, m in runner_modes)
    assert governed_concurrency(settings, checks) == expected


# assay_root is read-only (model_copy only), so function_scoped_fixture is suppressed.
@hyp_settings(parent=hyp_settings.get_profile("rasm"), suppress_health_check=[HealthCheck.function_scoped_fixture])
@given(cpu=st.integers(min_value=1, max_value=256), maxc=st.integers(min_value=1, max_value=64))
def test_governed_concurrency_bounded_in_unit_to_cpu(cpu: int, maxc: int, assay_root: AssayHarness) -> None:
    """For any settings the empty-batch limit lies in ``[1, min(max_checks, cpu_count)]`` — never 0, never unbounded."""
    settings = assay_root.settings.model_copy(update={"cpu_count": cpu, "max_checks": maxc})
    limit = governed_concurrency(settings, ())
    assert limit == IsInt(ge=1, le=min(maxc, cpu)), f"limit {limit} escaped [1, {min(maxc, cpu)}]"


# --- [RETRY_PREDICATE]


@pytest.mark.parametrize(
    "label, runner, exc, expected",
    [
        ("remote-connection-reset", Runner.DOTNET, ConnectionError("reset"), True),
        ("remote-broken-pipe", Runner.DOTNET, BrokenPipeError("pipe"), True),
        ("remote-generic-oserror", Runner.DOTNET, OSError("transport"), True),
        ("direct-oserror-is-terminal", Runner.DIRECT, OSError("local"), False),
        ("missing-binary-is-capability-gap", Runner.DOTNET, FileNotFoundError("absent"), False),
        ("value-error-is-terminal", Runner.DOTNET, ValueError("nul"), False),
        ("timeout-is-terminal", Runner.DOTNET, TimeoutError("deadline"), False),
        ("unclassified-exception-is-terminal", Runner.DOTNET, RuntimeError("unexpected"), False),
    ],
)
def test_retry_predicate_decision_table(label: str, runner: Runner, exc: BaseException, expected: bool) -> None:  # noqa: FBT001
    """``retry_predicate`` retries transport/spawn faults on non-direct runners only; spawn/value/timeout never retry."""
    check = Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=runner))
    validity_matrix(((label, exc, expected),), retry_predicate(check, None))


def test_retry_predicate_exhausted_budget_blocks_retry() -> None:
    """A transport fault is not retried once the remaining deadline budget is below the retry floor."""
    spent = time.monotonic() - 1.0
    assert retry_predicate(Check(tool=_REMOTE_TOOL), spent)(ConnectionError("reset")) is False


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


def test_splice_command_is_passthrough_identity_when_unscoped(assay_root: AssayHarness) -> None:
    """With no scope, ``splice_command`` returns its command argument unchanged for every runner."""
    verbs = assay_root.settings.scoped_verbs
    command = ("build", "Workspace.slnx", "--", "extra")
    assert splice_command(Runner.DOTNET, command, None, verbs, Mode.BUILD) is command


def test_argv_for_composes_runner_prefix_scope_and_routed_tails(assay_root: AssayHarness) -> None:
    """``argv_for`` prepends the runner prefix, threads the spliced body, and appends routed input tails."""
    scope = assay_root.scope(Claim.STATIC)
    tool = msgspec.structs.replace(_ECHO_TOOL, runner=Runner.UV, command=("ruff", "check"), input=Input.FILES)
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=("a.py", "b.py"))
    argv = argv_for(Check(tool=tool), routed, settings=assay_root.settings, scope=scope)
    assert argv[:2] == Runner.UV.prefix, f"runner prefix not leading argv: {argv!r}"
    assert {"ruff", "check", "a.py", "b.py"} <= set(argv), f"command body or routed tails lost in {argv!r}"


# --- [SSH_OUTCOME_REMOTE_COMMAND]


def test_ssh_outcome_projects_status_and_signal() -> None:
    """``ssh_outcome``: integer exits pass through (no notes); a signal kill maps to 255 with the signal name."""
    cases: tuple[ProjectionCase[tuple[int | None, object | None]], ...] = (
        ProjectionCase(label="clean-exit", intent=(0, None), supported_out=(0, ()), oracle=None, unsupported_out=()),
        ProjectionCase(label="nonzero-exit", intent=(2, None), supported_out=(2, ()), oracle=None, unsupported_out=()),
        ProjectionCase(label="signal-no-name", intent=(None, None), supported_out=(255, ()), oracle=None, unsupported_out=()),
        ProjectionCase(
            label="signal-named", intent=(None, ("TERM", False, "", "")), supported_out=(255, ("ssh.signal=TERM",)), oracle=None, unsupported_out=()
        ),
    )
    projection_matrix(cases, lambda intent: ssh_outcome(intent[0], intent[1]))


@pytest.mark.parametrize(
    "argv, cwd, env, fragments",
    [
        (("dotnet", "test"), "/work", {}, ("cd /work &&", "dotnet test")),
        (("ruff", "check", "."), "/repo root", {"PYTHONHASHSEED": "0"}, ("cd '/repo root'", "PYTHONHASHSEED=0", "ruff check .")),
        (("echo", "a b"), "/w", {"K": "v w"}, ("cd /w &&", "K='v w'", "echo 'a b'")),
    ],
)
def test_remote_command_shell_quotes_cwd_env_argv(argv: tuple[str, ...], cwd: str, env: dict[str, str], fragments: tuple[str, ...]) -> None:
    """``remote_command`` shell-quotes the cwd prefix, env exports, and every argv segment into one ``cd ... && ...`` line."""
    command = remote_command(argv, cwd=cwd, env=env)
    assert command == Contains(*fragments), f"missing fragment in {command!r}"


# --- [DRAIN_STREAM_CAPTURED]


def _recv_of(chunks: tuple[bytes, ...]) -> engine_mod.ByteRecv:
    """Build a ``ByteRecv`` double yielding ``chunks`` then ``None`` at EOF.

    Returns:
        An async read primitive returning each chunk in turn, ``None`` once exhausted.
    """
    pending = list(chunks)

    async def _recv() -> bytes | None:
        await anyio.sleep(0.0)
        return pending.pop(0) if pending else None

    return _recv


@given(chunks=st.lists(st.binary(max_size=64), max_size=24).map(tuple))
def test_drain_stream_aggregates_tail_size_and_lines(chunks: tuple[bytes, ...]) -> None:
    """``drain_stream`` over a ``ByteRecv`` double yields a ``Captured`` whose tail/size/line totals match the concatenation."""
    tail_cap = 16
    whole = b"".join(chunks)
    captured = anyio.run(lambda: drain_stream(_recv_of(chunks), tail_cap=tail_cap))
    expected_lines = whole.count(b"\n") + (1 if whole and not whole.endswith(b"\n") else 0)
    assert (captured.tail, captured.size, captured.lines) == (whole[-tail_cap:], len(whole), expected_lines), f"drain aggregate wrong: {captured!r}"


def test_drain_stream_empty_source_is_zero_capture() -> None:
    """A ``ByteRecv`` at immediate EOF drains to the zero ``Captured`` while preserving the recorded path."""
    captured = anyio.run(lambda: drain_stream(_recv_of(()), tail_cap=16, path="art/out.log"))
    assert captured == Captured(path="art/out.log")


class _ListSink:
    """Concrete ``WriteSink`` recording every chunk teed during a drain (full, un-clipped)."""

    def __init__(self) -> None:
        self.chunks: list[bytes] = []

    def write(self, payload: bytes) -> object:
        self.chunks.append(payload)
        return len(payload)


@given(chunks=st.lists(st.binary(min_size=1, max_size=48), min_size=1, max_size=16).map(tuple))
def test_write_sink_receives_every_drained_chunk(chunks: tuple[bytes, ...]) -> None:
    """``drain_stream`` tees the FULL stream into a ``WriteSink`` while the captured tail stays bounded — the tee is tail-cap-free."""
    sink: WriteSink = _ListSink()
    whole = b"".join(chunks)
    target(float(len(whole)), label="drained_bytes")
    captured = anyio.run(lambda: drain_stream(_recv_of(chunks), tail_cap=8, sink=sink))
    assert isinstance(sink, _ListSink)
    assert (b"".join(sink.chunks), captured.size, captured.tail) == (whole, len(whole), whole[-8:]), "sink/capture lost or clipped a chunk"


def test_captured_defaults_are_the_empty_aggregate() -> None:
    """``Captured()`` is the empty drain identity: blank tail/path and zero size/line counts."""
    assert (Captured().tail, Captured().path, Captured().size, Captured().lines) == (b"", "", 0, 0)


# --- [DISCOVER]


def test_discover_maps_process_status_to_result(tmp_path: Path) -> None:
    """``discover`` returns stdout on a zero exit and a ``FAULTED`` fault carrying the stderr tail on non-zero."""
    ok = discover((sys.executable, "-c", "print('a')"), root=tmp_path, timeout=10.0)
    bad = discover((sys.executable, "-c", "import sys; sys.stderr.write('bad'); sys.exit(2)"), root=tmp_path, timeout=10.0)
    assert assert_ok(ok) == b"a\n"
    assert assert_error_status(bad, RailStatus.FAULTED).message == "bad"


def test_discover_deadline_and_spawn_faults(tmp_path: Path) -> None:
    """``discover`` maps an over-budget child to ``TIMEOUT`` and an unspawnable binary to ``FAULTED``.

    Both early discovery rails against real subprocess behaviour: ``anyio.fail_after`` vs ``OSError`` at spawn.
    """
    slow = discover((sys.executable, "-c", "import time; time.sleep(5)"), root=tmp_path, timeout=0.2)
    absent = discover(("/nonexistent/assay-discover-binary",), root=tmp_path, timeout=10.0)
    assert_error_status(slow, RailStatus.TIMEOUT)
    assert_error_status(absent, RailStatus.FAULTED)


# --- [RUN_CHECK_FAN_OUT]


def test_run_check_executes_direct_tool(assay_root: AssayHarness) -> None:
    """``run_check`` runs a DIRECT tool under its own event loop and returns an Ok receipt with the tool stdout."""
    assert b"hello" in assert_ok(_run(Check(tool=_ECHO_TOOL), assay_root)).stdout


@pytest.mark.anyio
async def test_run_check_async_is_the_event_loop_boundary(assay_root: AssayHarness) -> None:
    """``run_check_async`` is the public in-loop entrypoint — async callers never import the woven spawn."""
    outcome = await run_check_async(Check(tool=_ECHO_TOOL), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)
    assert b"hello" in assert_ok(outcome).stdout


def test_run_check_retries_transient_spawn_via_rail_probe(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A transient spawn fault on a remote runner is retried and the recovered receipt carries attempt evidence."""
    calls = [0]

    async def flaky(*_args: object, **_kwargs: object) -> Completed:
        await anyio.sleep(0.0)
        calls[0] += 1
        match calls[0]:
            case 1:
                raise OSError("temporary transport")
            case _:
                return receipt(("dotnet", "test"), 0)

    monkeypatch.setattr(engine_mod, "_execute", flaky)
    done = assert_ok(_run(Check(tool=_REMOTE_TOOL), assay_root))
    assert (calls[0], "retry attempts=2" in done.notes) == (2, True), f"expected one retry with attempt evidence, got {calls[0]} attempts: {done!r}"


def test_fan_out_preserves_order_and_backfills_timeout(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``fan_out`` preserves input order, completes fast slots, and back-fills a deadline-cancelled slot as TIMEOUT."""

    async def indexed(check: Check, *_args: object, **_kwargs: object) -> Completed:
        idx = int(check.tool.name.split("-")[1])
        await anyio.sleep(0.0 if idx < 2 else 10.0)
        return receipt((check.tool.name,), 0)

    monkeypatch.setattr(engine_mod, "_execute", indexed)
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, name=f"check-{i}", runner=Runner.DOTNET)) for i in range(3))
    results = fan_out(checks, settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED, deadline=time.monotonic() + 0.3)
    assert len(results) == 3
    assert_ok(results[0])
    assert_ok(results[1])
    assert_error_status(results[2], RailStatus.TIMEOUT)


# --- [EXCLUSIVE_LEASE_LEASED]


@pytest.mark.mutation
def test_exclusive_lease_is_busy_under_a_live_holder(assay_root: AssayHarness) -> None:
    """A second acquire of a live resource yields ``Error(Fault(BUSY))`` — mutual exclusion at the POSIX flock seam."""
    with exclusive_lease("res", "run-a", settings=assay_root.settings) as first:
        assert_ok(first)
        with exclusive_lease("res", "run-b", settings=assay_root.settings) as second:
            assert_error_status(second, RailStatus.BUSY)


def test_exclusive_lease_releases_after_context(assay_root: AssayHarness) -> None:
    """After the context exits the lock is truncated/released, so the next acquire succeeds."""
    with exclusive_lease("rel", "run-a", settings=assay_root.settings) as first:
        assert_ok(first)
    with exclusive_lease("rel", "run-b", settings=assay_root.settings) as second:
        assert_ok(second)


def test_leased_runs_action_only_when_held(assay_root: AssayHarness) -> None:
    """``leased`` runs the action and returns its value while the lease is free; a contended lease short-circuits to BUSY."""
    token = receipt(("held",), 0)
    first = leased("act", lambda _held: Ok(token), settings=assay_root.settings, run_id="run-a")
    assert assert_ok(first) is token
    # The contended action must never run; if it does, returning a FAULTED fault makes the BUSY assertion fail loudly.
    breach = Fault((), status=RailStatus.FAULTED, message="action ran under a held lease")
    with exclusive_lease("act", "holder", settings=assay_root.settings) as guard:
        assert_ok(guard)
        contended = leased("act", lambda _held: Error(breach), settings=assay_root.settings, run_id="run-b")
        assert_error_status(contended, RailStatus.BUSY)


# --- [STATEFUL_LEASE_RBSM]


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
        """Initialise with an empty slot registry and a fresh temp artifact root containing ``Workspace.slnx``.

        ``AssaySettings`` requires a ``Workspace.slnx`` sentinel to locate the artifact root;
        the sentinel is written empty because the RBSM exercises only the POSIX flock surface.
        """
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
        # tuple() forces the generator; ExitStack.close() returns None so a plain list comprehension would be discarded.
        _ = tuple(slot.stack.close() for slot in self._slots.values())


def test_lease_state_machine_holds_mutual_exclusion() -> None:
    """Drive the synchronous ``exclusive_lease`` RBSM: across all acquire/release interleavings at most one Ok holder is live."""
    run_state_machine_as_test(LeaseStateMachine, settings=hyp_settings.get_profile("rasm-stateful"))  # type: ignore[no-untyped-call]


# --- [FAULT_RAIL_PROBE]


@given(fault=fault_st)
def test_engine_fault_rail_is_error_addressable(fault: Fault) -> None:
    """The conftest ``fault_st`` alias (``resolve(Fault)``) is encode-clean and ``assert_error_status``-addressable.

    Anchors every ROP law over an engine ``Error(Fault(...))`` to real bounded-message shapes.
    """
    decoded = msgspec.json.decode(msgspec.json.encode(fault), type=type(fault))
    assert decoded == fault, f"fault not encode-clean: {decoded!r} != {fault!r}"
    assert assert_error_status(Error(fault), fault.status) is fault


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


# --- [GUARDED_FAULT_RAILS]

_GUARDED_TOOLS: tuple[tuple[str, tuple[str, ...], float | None, RailStatus], ...] = (
    ("missing-binary", ("/nonexistent/assay-guarded-binary",), None, RailStatus.UNSUPPORTED),
    ("deadline", (sys.executable, "-c", "import time; time.sleep(10)"), 0.2, RailStatus.TIMEOUT),
    ("nul-in-argv", ("/bin/echo", "a\x00b"), None, RailStatus.FAULTED),
)


@pytest.mark.parametrize("label, command, timeout, status", _GUARDED_TOOLS, ids=[c[0] for c in _GUARDED_TOOLS])
def test_run_check_classifies_spawn_faults(
    label: str, command: tuple[str, ...], timeout: float | None, status: RailStatus, assay_root: AssayHarness
) -> None:
    """``run_check`` routes each real ``_guarded`` spawn failure: absent binary → ``UNSUPPORTED``, deadline → ``TIMEOUT``, NUL argv → ``FAULTED``."""
    _ = label
    tool = Tool("guarded-law", Runner.DIRECT, command, Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.CHECK, timeout=timeout)
    assert_error_status(run_check(Check(tool=tool), settings=assay_root.settings, scope=None, routed=_PY_CHANGED), status)


# --- [INPROC_THUNK]


def test_inproc_thunk_outcomes(assay_root: AssayHarness) -> None:
    """``Runner.INPROC`` through ``_inproc`` classifies thunk outcomes.

    Missing thunk → rc=1 defect, raising thunk → contained rc=1 (type in stderr), healthy → verbatim receipt.
    """
    base = Tool("inproc-law", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.CODE, mode=Mode.CHECK)

    def _raise(_check: Check) -> Completed:
        raise RuntimeError("deliberate thunk fault")

    def _good(check: Check) -> Completed:
        return receipt((check.tool.name,), 0, stdout=b"inproc-ok")

    no_thunk = assert_ok(_run(Check(tool=base), assay_root))
    raising = assert_ok(_run(Check(tool=msgspec.structs.replace(base, thunk=_raise), paths=("p",)), assay_root))
    healthy = assert_ok(_run(Check(tool=msgspec.structs.replace(base, thunk=_good)), assay_root))
    assert (no_thunk.returncode, b"no thunk" in no_thunk.stderr.lower()) == (1, True), f"missing-thunk receipt wrong: {no_thunk!r}"
    assert (raising.returncode, b"RuntimeError" in raising.stderr) == (1, True), f"raising-thunk receipt wrong: {raising!r}"
    assert (healthy.returncode, healthy.stdout) == (0, b"inproc-ok"), f"healthy-thunk receipt wrong: {healthy!r}"


# --- [STREAMING_LOCAL]

_STREAM_LOCAL: tuple[tuple[str, tuple[str, ...], Language, bytes, bool], ...] = (
    ("scoped-persists-full", ("/bin/echo", "stream-ok"), Language.CSHARP, b"stream-ok\n", True),
    ("8kib-clips-tail-full-persists", (sys.executable, "-c", "import sys; sys.stdout.write('x' * 8192)"), Language.PYTHON, b"x" * 8192, True),
    ("noscope-bounded-tail-no-artifact", ("/bin/echo", "stream-ok"), Language.CSHARP, b"stream-ok\n", False),
)


@pytest.mark.parametrize("label, command, language, payload, scoped", _STREAM_LOCAL, ids=[c[0] for c in _STREAM_LOCAL])
def test_streaming_local_tail_clips_while_full_artifact_persists(
    label: str,
    command: tuple[str, ...],
    language: Language,
    payload: bytes,
    scoped: bool,  # noqa: FBT001
    assay_root: AssayHarness,
) -> None:
    """A streaming local tool tees the full payload to the artifact while the receipt keeps a bounded ``stream_tail_bytes`` tail.

    ``scope=None`` is the no-sink tee arm (bounded tail, no artifact); the 8 KiB row is falsifiable: clipped tail vs complete persisted payload.
    """
    scope = assay_root.scope(Claim.STATIC) if scoped else None
    cap = assay_root.settings.stream_tail_bytes
    done = assert_ok(_run(Check(tool=_stream_tool(f"{label}-tool", command, language)), assay_root, scope=scope))
    assert done.stdout == payload[-cap:], f"tail not clipped to {cap}: len={len(done.stdout)}"
    artifact = next((a for a in done.artifacts if a.id == f"{label}-tool-out"), None)
    match scope:
        case None:
            assert done.artifacts == (), f"scope-less stream emitted artifacts: {done.artifacts!r}"
        case _:
            assert artifact is not None, f"no persisted stdout artifact in {done.artifacts!r}"
            assert scope.store.read_path(artifact.path) == payload, "persisted artifact lost the streamed bytes"


# --- [STAGE_MATERIALIZE]


def test_staged_tool_materializes_workdir_and_runs(assay_root: AssayHarness) -> None:
    """A staged UV tool copies contained file + directory inputs into an artifact worktree, runs from it, and never touches the repo root.

    Drives the ``_materialize`` + ``_copy_stage_input`` + ``_contained`` success arms (destructive ``rmtree`` + ``copytree`` against contained paths).
    """
    assay_root.write("pyproject.toml", "[project]\n")
    assay_root.write("tools/assay/__init__.py", "")
    stage = Stage(root=".artifacts/python/work", inputs=("pyproject.toml", "tools/assay"), project=True)
    tool = Tool(
        "stage-run-law",
        Runner.UV,
        ("python", "--version"),
        Input.FILES,
        Language.PYTHON,
        Claim.TEST,
        mode=Mode.RUN,
        groups=("mutation",),
        stage=stage,
    )
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=("tests/x",))
    assert_ok(run_check(Check(tool=tool), settings=assay_root.settings, scope=None, routed=routed))
    work = assay_root.root / ".artifacts/python/work"
    assert (work / "pyproject.toml").is_file(), "staged file input not copied into the worktree"
    assert (work / "tools/assay/__init__.py").is_file(), "staged directory input not copied into the worktree"


def test_staged_tool_requires_local_execution(assay_root: AssayHarness) -> None:
    """A staged tool against a remote ``exec_target`` is ``UNSUPPORTED`` — staging is a local-fs-only worktree."""
    remote = assay_root.remote("ssh://x@127.0.0.1:2222")
    tool = Tool(
        "stage-remote-law",
        Runner.UV,
        ("python", "--version"),
        Input.NONE,
        Language.PYTHON,
        Claim.TEST,
        mode=Mode.RUN,
        stage=Stage(root=".artifacts/python/work"),
    )
    fault = assert_error_status(run_check(Check(tool=tool), settings=remote, scope=None, routed=_PY_CHANGED), RailStatus.UNSUPPORTED)
    assert "staged tools require local execution" in fault.message, f"wrong stage-remote message: {fault!r}"


# --- [REAP]


def test_reap_terminates_live_tree_and_passes_through_exited(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_reap`` kills a still-running child's tree then waits (``case None``); an already-exited child short-circuits to the wait (``case _``)."""
    monkeypatch.setattr(engine_mod, "_reap_tree", lambda _pid: None)

    async def _live() -> int | None:
        proc = await anyio.open_process([sys.executable, "-c", "import time; time.sleep(30)"])
        await engine_mod._reap(proc)
        return proc.returncode

    async def _exited() -> int | None:
        proc = await anyio.open_process([sys.executable, "-c", "pass"])
        await proc.wait()
        await engine_mod._reap(proc)
        return proc.returncode

    assert anyio.run(_live) is not None, "live child not reaped to a terminal returncode"
    assert anyio.run(_exited) == 0, "exited child returncode not preserved through _reap"


def test_reap_tree_terminates_real_process_tree() -> None:
    """``_reap_tree`` (the worker-thread body ``_reap`` offloads) walks and terminates a real child via psutil.

    An unresolvable pid takes the ``psutil.Error`` arm without raising.
    """

    async def _drive() -> int | None:
        async with await anyio.open_process([sys.executable, "-c", "import time; time.sleep(30)"]) as proc:
            await anyio.to_thread.run_sync(engine_mod._reap_tree, proc.pid)  # ty: ignore[unresolved-attribute]
            with anyio.fail_after(5.0):
                await proc.wait()
            return proc.returncode

    assert anyio.run(_drive) is not None, "live process not terminated by _reap_tree"
    engine_mod._reap_tree(2_147_483_646)


def test_terminate_process_tree_kills_terminate_resistant_child() -> None:
    """``_terminate_process_tree`` escalates to ``kill`` (the SIGKILL fallback) for a child still ``alive`` after ``terminate`` + ``wait_procs``."""
    survivor = _proc(pid=4242, running=True)
    already_dead = _proc(pid=4243, running=False)
    fake = _make_psutil_module({4242: survivor, 4243: already_dead})
    fake.wait_procs.return_value = ((already_dead,), (survivor,))
    with pytest.MonkeyPatch.context() as patch:
        patch.setattr(engine_mod, "psutil", fake)
        engine_mod._terminate_process_tree((already_dead, survivor))
    survivor.terminate.assert_called_once()
    survivor.kill.assert_called_once()
    already_dead.terminate.assert_not_called()
    already_dead.kill.assert_not_called()


# --- [DIAGNOSE_SNAPSHOT]


def test_diagnose_records_resource_snapshot(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_diagnose`` folds the real ``_snapshot`` chain onto the active span event (``mem.rss_bytes``).

    Also seeds the ``_RESOURCE`` ContextVar for the cross-``anyio.run`` Fault build.
    """
    fake = _make_psutil_module({None: _proc(rss=65536)})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    events: list[tuple[str, dict[str, float]]] = []
    exceptions: list[BaseException] = []
    span = SimpleNamespace(record_exception=exceptions.append, add_event=lambda name, attributes: events.append((name, attributes)))
    monkeypatch.setattr(trace, "get_current_span", lambda: span)
    token = engine_mod._RESOURCE.set(())
    try:
        exc = TimeoutError("synthetic diagnose timeout")
        engine_mod._diagnose(exc)
        assert exceptions == [exc], "exception not recorded on the fault span"
        name, attrs = events[0]
        assert (name, attrs.get("mem.rss_bytes")) == (engine_mod._FAULT_SNAPSHOT, 65536.0), f"snapshot event wrong: {name!r} {attrs!r}"
        rss = dict(engine_mod._RESOURCE.get())["mem.rss_bytes"]
        assert rss == pytest.approx(65536.0), "resource ContextVar not seeded from the snapshot"
    finally:
        engine_mod._RESOURCE.reset(token)


def test_snapshot_pressure_arms_degrade_to_empty(monkeypatch: pytest.MonkeyPatch) -> None:
    """The snapshot is best-effort evidence, never a second fault source.

    ``_memory_pressure``/``_children`` swallow ``psutil.Error`` to ``{}``, ``_num_fds`` swallows ``AttributeError`` to
    ``-1``, ``_safe_call`` returns its default, ``_as_bytes`` projects str/None/bytes.
    """
    fake = _make_psutil_module({None: _proc(rss=4096)})
    fake.virtual_memory.side_effect = psutil.Error("vm down")
    fake.swap_memory.side_effect = psutil.Error("swap down")
    monkeypatch.setattr(engine_mod, "psutil", fake)
    fd_proc = _proc()
    fd_proc.num_fds.side_effect = AttributeError("no num_fds")
    kids_proc = _proc()
    kids_proc.children.side_effect = psutil.Error("children down")

    def _fault() -> float:
        raise ValueError("probe down")

    support_matrix(
        ("memory-pressure-fault→{}", lambda: engine_mod._memory_pressure() == {}, True),
        ("children-fault→{}", lambda: engine_mod._children(kids_proc) == {}, True),
        ("num-fds-attr-error→-1", lambda: engine_mod._num_fds(fd_proc) == -1, True),
        ("safe-call-value-arm", lambda: engine_mod._safe_call(lambda: 7.0, -1.0) == pytest.approx(7.0), True),
        ("safe-call-default-arm", lambda: engine_mod._safe_call(_fault, -1.0) == pytest.approx(-1.0), True),
        (
            "as-bytes-projection",
            lambda: (engine_mod._as_bytes(b"x"), engine_mod._as_bytes(None), engine_mod._as_bytes("s")) == (b"x", b"", b"s"),
            True,
        ),
    )


def test_load_pressure_handles_absent_and_failing_getloadavg(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_load_pressure`` returns ``{}`` when ``os.getloadavg`` is absent and when it raises ``OSError``."""

    def _boom() -> tuple[float, float, float]:
        raise OSError("load unavailable")

    monkeypatch.setattr(os, "getloadavg", _boom, raising=False)
    assert engine_mod._load_pressure() == {}, "failing getloadavg not degraded to empty"
    monkeypatch.delattr(os, "getloadavg", raising=False)
    assert engine_mod._load_pressure() == {}, "absent getloadavg not degraded to empty"


# --- [LEASE_CLAIM]


@contextlib.contextmanager
def _lock_fd(path: UPath | Path, *, exclusive: bool = False, seed: bytes | None = None) -> Iterator[int]:
    """Open a lock-file fd (parents created), optionally seed it and take ``LOCK_EX``, always releasing + closing on exit.

    The unlock is unconditional so the steal arm (whose ``_steal`` flocks the yielded fd itself) is also released.

    Yields:
        The open file descriptor — flocked when ``exclusive=True``, otherwise free for ``_steal`` to win.
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

    Both a populated owner block and an empty mid-write body (the ``case b""`` arm) read under contention as a
    live holder. Both rows hold ``LOCK_EX``; the contender's non-blocking flock raises ``BlockingIOError`` so
    ``_claim`` returns BUSY without entering the steal path.
    """
    owner = engine_mod._LeaseOwner(resource=label, run_id="holder", pid=os.getpid(), create_time=time.time())
    seed = msgspec.json.encode(owner) if populate else None
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, f"{label}.lock")
    with _lock_fd(lock_path, exclusive=True, seed=seed), exclusive_lease(label, "contender", settings=assay_root.settings) as contended:
        assert_error_status(contended, RailStatus.BUSY)


@pytest.mark.mutation
def test_claim_steals_dead_holder_lock(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A lock owned by a dead pid (``NoSuchProcess``) is stolen — the full real-``flock`` reclaim path.

    ``is_lease_stale`` True → ``_claim`` falls through to ``_steal`` → ``exclusive_lease`` returns ``Ok`` with the rewritten owner block.
    """
    dead_pid, self_pid = 88_888, os.getpid()
    fake = _make_psutil_module({self_pid: _proc(pid=self_pid), dead_pid: _proc(pid=dead_pid, raise_no_such=True)})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "stale.lock")
    lock_path.parent.mkdir(parents=True, exist_ok=True)
    stale = engine_mod._LeaseOwner(resource="stale", run_id="old-run", pid=dead_pid, create_time=0.0)
    lock_path.write_bytes(msgspec.json.encode(stale))
    with exclusive_lease("stale", "new-run", settings=assay_root.settings) as reclaimed:
        owner = assert_ok(reclaimed).owner
    assert (owner.run_id, owner.pid) == ("new-run", self_pid), f"steal did not rewrite the owner block: {owner!r}"


def test_lease_owner_block_stamps_every_field(assay_root: AssayHarness) -> None:
    """A held lease stamps resource/run_id/cwd/project/mode and a positive create_time into the on-disk owner block."""
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "fields.lock")
    with exclusive_lease("fields", "run-fields", settings=assay_root.settings, project="proj", mode="exclusive") as held:
        assert_ok(held)
        owner = decode_lease_owner(lock_path.read_bytes())
    assert owner is not None, "owner block not written while the lease was held"
    expected = {
        "resource": "fields",
        "run_id": "run-fields",
        "cwd": str(assay_root.root),
        "pid": os.getpid(),
        "project": "proj",
        "mode": "exclusive",
        "create_time": IsPositiveFloat(),
    }
    assert msgspec.structs.asdict(owner) == IsPartialDict(expected), f"owner block fields wrong: {owner!r}"


def test_exclusive_lease_rejects_non_local_backend(tmp_path: Path) -> None:
    """A non-file (``memory://``) backend cannot host a POSIX ``fcntl.flock`` lease.

    ``exclusive_lease`` short-circuits to ``UNSUPPORTED`` before any fd is opened.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath("memory://assay-lease-backend"), exec_target="", exec_known_hosts=None)
    with exclusive_lease("res", "run", settings=settings) as outcome:
        fault = assert_error_status(outcome, RailStatus.UNSUPPORTED)
    assert "POSIX leases require a local artifact root" in fault.message, f"wrong backend message: {fault!r}"


def test_leased_maps_lease_io_error_to_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``leased`` projects an ``OSError`` from the lease context manager to a ``FAULTED`` result, never a raise — and the action never runs."""
    ran = [False]

    @contextlib.contextmanager
    def _boom(*_args: object, **_kwargs: object) -> Iterator[object]:
        if not ran[0]:  # always True here; the branch keeps `yield` reachable for the generator typing
            raise OSError("lease fs gone")
        yield None  # pragma: no cover

    monkeypatch.setattr(engine_mod, "exclusive_lease", _boom)

    def _action(_held: object) -> Result[object, Fault]:
        ran[0] = True
        return Ok(1)

    outcome = leased("res", _action, settings=assay_root.settings, run_id="run")
    assert_error_status(outcome, RailStatus.FAULTED)
    assert ran[0] is False, "action ran despite a lease acquisition fault"


def test_steal_rewrites_owner_on_uncontended_lock(assay_root: AssayHarness) -> None:
    """``_steal`` re-locks an uncontended fd and rewrites the owner block, returning the new live owner — the reclaim half of stale-lease theft."""
    prior = engine_mod._LeaseOwner(resource="steal-direct", run_id="dead", pid=88_888, create_time=0.0)
    with _lock_fd(assay_root.settings.artifact(ArtifactKind.LOCKS, "steal-direct.lock")) as fd:
        won = engine_mod._steal(fd, "steal-direct", run_id="winner", target="", owner=prior)
    assert won is not None, "_steal did not return a won owner on an uncontended lock"
    assert (won.run_id, won.pid) == ("winner", os.getpid()), f"_steal did not rewrite the owner: {won!r}"


def test_steal_yields_busy_on_lost_toctou_race(assay_root: AssayHarness) -> None:
    """``_steal`` returns ``None`` (BUSY, never FAULTED) when a sibling holds the flock through the steal attempt."""
    lock_path = assay_root.settings.artifact(ArtifactKind.LOCKS, "steal-lost.lock")
    with _lock_fd(lock_path, exclusive=True), _lock_fd(lock_path) as contender:
        assert engine_mod._steal(contender, "steal-lost", run_id="loser", target="", owner=None) is None, "lost TOCTOU race did not yield BUSY"


# --- [SSH_ROUND_TRIP]


@pytest.mark.anyio
async def test_run_check_remote_round_trips_through_loopback(assay_root: AssayHarness, ssh_loopback: SshLoopback, socket_enabled: None) -> None:
    """The remote arm shell-quotes argv through ``remote_command`` and returns the loopback reply (non-streaming ``conn.run`` arm)."""
    _ = socket_enabled
    remote = assay_root.remote(ssh_loopback.exec_target)
    check = Check(tool=_ECHO_TOOL, cwd=assay_root.root)
    # run_check drives its own anyio.run loop; bridge to a thread to avoid nested event loops under anyio.
    done = assert_ok(await anyio.to_thread.run_sync(lambda: run_check(check, settings=remote, scope=None, routed=_ROUTED_CHANGED)))  # ty: ignore[unresolved-attribute]
    assert (b"remote-ok:" in done.stdout, done.returncode) == (True, 0), f"loopback reply missing from {done.stdout!r}"


@pytest.mark.anyio
@pytest.mark.parametrize("scoped", [False, True], ids=["non-persisted", "scoped-persists-artifact"])
async def test_run_check_remote_streaming_round_trips(
    scoped: bool,  # noqa: FBT001
    assay_root: AssayHarness,
    ssh_loopback: SshLoopback,
    socket_enabled: None,
) -> None:
    """The remote streaming arm tees the loopback reply through ``_recv_ssh`` + ``drain_stream`` and tail-caps the receipt.

    The scoped row drives the with-handle arm: ``_stream_writer`` sink + ``_stream_artifacts`` persist.
    """
    _ = socket_enabled
    remote = assay_root.remote(ssh_loopback.exec_target)
    scope = assay_root.scope(Claim.STATIC) if scoped else None
    name = "remote-scoped-stream-law" if scoped else "remote-stream-law"
    check = Check(tool=_stream_tool(name, ("/bin/echo", "stream-ok")), cwd=assay_root.root)
    done = assert_ok(await anyio.to_thread.run_sync(lambda: run_check(check, settings=remote, scope=scope, routed=_ROUTED_CHANGED)))  # ty: ignore[unresolved-attribute]
    assert (b"/bin/echo" in done.stdout, done.returncode) == (True, 0), f"streamed remote command not in tail: {done.stdout!r}"
    match scope:
        case None:
            pass
        case _:
            artifact = next((a for a in done.artifacts if a.id == f"{name}-out"), None)
            assert artifact is not None, f"scoped remote stream emitted no artifact: {done.artifacts!r}"
            assert b"remote-ok:" in scope.store.read_path(artifact.path), "persisted remote artifact lost the loopback reply"


@pytest.mark.anyio
async def test_fan_out_remote_pools_ssh_connection(assay_root: AssayHarness, ssh_loopback: SshLoopback, socket_enabled: None) -> None:
    """``fan_out`` over a remote runner pools one ssh connection across workers and ``_pooled_ssh`` closes it on scope exit.

    First opens, second reuses the ``_SshCache`` arm; both slots round-trip the loopback reply (pooled-reuse + close-loop path).
    """
    _ = socket_enabled
    remote = assay_root.remote(ssh_loopback.exec_target)
    base = Tool("remote-fan-law", Runner.DIRECT, ("/bin/echo", "hi"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.CHECK)
    checks = tuple(Check(tool=msgspec.structs.replace(base, name=f"remote-fan-{i}"), cwd=assay_root.root) for i in range(2))

    def _sync() -> tuple[Result[Completed, Fault], ...]:
        return fan_out(checks, settings=remote, scope=None, routed=_ROUTED_CHANGED, deadline=time.monotonic() + 10.0)

    results = await anyio.to_thread.run_sync(_sync)  # ty: ignore[unresolved-attribute]
    assert len(results) == 2, f"fan_out lost a remote slot: {results!r}"
    for index, result in enumerate(results):
        assert b"remote-ok:" in assert_ok(result).stdout, f"remote slot {index} missing loopback reply"


@pytest.mark.anyio
@pytest.mark.parametrize("exc_factory", ["asyncssh", "oserror"])
async def test_pooled_ssh_logs_close_failures(exc_factory: str) -> None:
    """``_pooled_ssh`` swallows both ``asyncssh.Error`` and ``OSError`` raised by ``wait_closed`` on scope exit (logging each).

    A broken close never aborts cleanup of sibling connections.
    """
    import asyncssh  # noqa: PLC0415  # deferred: the double must raise asyncssh.Error directly; the type is unavailable at module level

    boom: BaseException = asyncssh.Error(code=1, reason="close failed") if exc_factory == "asyncssh" else OSError("socket reset on close")
    closed = [False]

    def _mark_closed() -> None:
        closed[0] = True

    async def _wait_closed() -> None:  # noqa: RUF029  # async to match asyncssh's awaitable wait_closed signature; raises before yielding
        raise boom

    conn = SimpleNamespace(close=_mark_closed, wait_closed=_wait_closed)
    async with engine_mod._pooled_ssh():
        cache = engine_mod._SSH_CACHE.get()
        assert cache is not None, "_pooled_ssh did not seed the connection cache"
        cache.conns["ssh://x@host:22"] = conn  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # SimpleNamespace conn double; structural duck-type only
    assert closed[0] is True, "pooled connection close was not attempted before wait_closed faulted"


# --- [ENGINE_INTERNALS]


def test_drain_none_stream_is_empty_tail() -> None:
    """``_drain(None, ...)`` short-circuits the inherited-fd / non-streaming arm to an empty tail via ``_recv_anyio`` ``None``."""
    assert anyio.run(lambda: engine_mod._drain(None, tail_cap=128, chunk=32)) == b"", "None stream did not drain to empty"


def test_total_backfills_none_slot_as_timeout() -> None:
    """``_total`` back-fills a never-completed fan-out slot (``None``) as ``TIMEOUT``; a present ``Result`` rides through by identity."""
    backfilled = engine_mod._total(None)
    present = Ok(receipt(("echo",), 0))
    assert_error_status(backfilled, RailStatus.TIMEOUT)
    assert engine_mod._total(present) is present, "present slot not passed through by identity"


def test_child_rss_reads_resident_set_size() -> None:
    """``_child_rss`` reads a child's RSS from ``memory_info`` and falls back to ``0`` when the field is absent."""
    assert engine_mod._child_rss(_proc(rss=8192)) == 8192, "child RSS not read from memory_info"


def test_copy_stage_input_reports_missing_input(tmp_path: Path) -> None:
    """``_copy_stage_input`` faults ``missing stage input`` for a contained-but-absent input (existence-guard arm, not containment-escape)."""
    root, work = tmp_path / "root", tmp_path / "work"
    root.mkdir()
    work.mkdir()
    fault = engine_mod._copy_stage_input(Check(tool=_ECHO_TOOL), root, work, "absent-input.txt")
    assert fault is not None, "missing stage input did not fault"
    assert "missing stage input" in fault.message, f"wrong missing-input message: {fault!r}"


def test_stream_writer_rejects_non_context_backend_handle(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_stream_writer`` raises ``TypeError`` (the ``_WriteContext`` contract) when a backend ``open_write`` returns a non-context handle.

    It rejects the bad handle loudly rather than silently dropping the stream.
    """
    scope = assay_root.scope(Claim.STATIC)
    monkeypatch.setattr(type(scope.store), "open_write", lambda _self, *_parts: ("art/path", object()))
    plan = engine_mod._ExecPlan(
        argv=("/bin/echo", "x"),
        check=Check(tool=_stream_tool("sw-law", ("/bin/echo", "x"))),
        cwd=str(assay_root.root),
        env={},
        settings=assay_root.settings,
        scope=scope,
        streaming=True,
        tail_cap=assay_root.settings.stream_tail_bytes,
        chunk=assay_root.settings.stream_chunk_bytes,
        thread_limiter=None,
    )
    with pytest.raises(TypeError, match="non-context writer"):
        engine_mod._stream_writer(plan, "out")


# --- [MUTATION_LANE]


def test_mutation_lane_is_populated(request: pytest.FixtureRequest) -> None:
    """The ``mutation`` marker lane carries the engine's destructive-boundary laws so mutmut has a target set."""
    items = getattr(request.session, "items", ())
    marked = tuple(item for item in items if item.get_closest_marker("mutation") is not None)
    assert len(marked) >= 2, f"mutation lane underpopulated: {len(marked)} marked"


# --- [EXPORTS] --------------------------------------------------------------------------

LeaseStateMachineTest = LeaseStateMachine
