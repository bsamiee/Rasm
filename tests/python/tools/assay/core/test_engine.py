"""Engine laws: pure-fn algebra (decode/stale/governed/retry/splice/ssh/drain) + the lease RBSM.

Pure-function laws are oracle-driven via ``tests.python._testkit.spec`` (inverse / monotone / validity_matrix /
projection_matrix / support_matrix / assert_ok / assert_error_status). The synchronous ``exclusive_lease``
mutual-exclusion contract is held by the ported ``LeaseStateMachine`` RBSM. ``run_check`` / ``fan_out`` /
``discover`` are exercised through real subprocess / socketpair-ssh / psutil-double seams (no mocks of the boundary).
``ByteRecv`` / ``WriteSink`` (Callable / Protocol aliases) and ``_RESOURCE`` (the resource ContextVar seam)
are law-less by nature and are carried as exemptions in the assay conftest ``_EXEMPT`` set.

``_LAWS`` registers all covered symbols at import time, fixing the MANIFEST total at collection time so
missing coverage is detected before any test runs.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections import deque
import contextlib
import fcntl
import functools
from itertools import starmap
import os
import signal
import sys
import tempfile
import time
from types import SimpleNamespace
from typing import override, TYPE_CHECKING, TypedDict

import anyio
from dirty_equals import Contains, IsInt, IsPartialDict, IsPositiveFloat
from expression import Error, Ok
from hypothesis import given, HealthCheck, settings as hyp_settings, strategies as st, target
from hypothesis.stateful import Bundle, consumes, invariant, rule, RuleBasedStateMachine
import msgspec
from opentelemetry import trace
from opentelemetry.sdk.trace.export.in_memory_span_exporter import (
    InMemorySpanExporter,  # noqa: TC002  # fixture type annotations evaluated at collection time; must be a runtime import
)
import psutil
import pytest
from upath import UPath

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import (
    assert_error,
    assert_error_status,
    assert_ok,
    model_based,
    monotone,
    projection_matrix,
    ProjectionCase,
    roundtrip,
    support_matrix,
    validity_matrix,
)

# AssayHarness is a runtime (not TYPE_CHECKING) import: hypothesis evaluates @given fixture
# annotations under PEP 649 eval_str, so it must resolve at module runtime though it appears only in annotations.
from tests.python.tools.assay.kit import _make_psutil_module, _proc, AssayHarness, fault_st  # noqa: TC001
from tools.assay.composition.settings import AssaySettings
from tools.assay.core.aspect import _RING  # co-owned ring seam; seeded directly for ring-content assertions
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
    proc_dead,
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
    from collections.abc import Awaitable, Callable, Iterator, Mapping, Sequence
    from pathlib import Path

    import asyncssh
    from expression import Result

    from tests.python._testkit.env import Provisioned
    from tools.assay.composition.settings import ArtifactScope
    from tools.assay.core.model import Completed

    type SshEnv = Provisioned[Awaitable[asyncssh.SSHClientConnection]]


# --- [TYPES] ----------------------------------------------------------------------------


class _ProcKw(TypedDict, total=False):
    """Keyword payload accepted by the psutil process double for the staleness sweep."""

    raise_no_such: bool
    running: bool
    create_time: float
    status: str


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

# --- [LAW_COVERAGE]

_LAWS: tuple[tuple[object, str], ...] = (
    (decode_lease_owner, "decode_lease_owner_inverts_encode"),
    (is_lease_stale, "is_lease_stale_decision_table"),
    (proc_dead, "proc_dead_truth_table"),
    (proc_dead, "proc_dead_access_denied_defers_to_pid_exists"),
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
    (engine_mod._stall_sample, "stall_sample_aggregates_process_tree"),
    (engine_mod._stall_verdict, "stall_verdict_triage_table"),
    (engine_mod._stall_monitor, "stall_monitor_shrunk_constant_matrix"),
    (engine_mod.discover_async, "discover_async_via_discover"),
    (engine_mod._dotnet_root, "dotnet_root_probe_precedence"),
    (engine_mod._apphost, "apphost_overlays_tool_run_heads_only"),
    (run_check, "run_check_executes_direct_tool"),
    (engine_mod._overlay, "overlay_merges_row_env_into_spawned_process"),
    (run_check, "spawned_children_detach_into_own_session"),
    (discover, "discover_detaches_child_session"),
    (engine_mod._terminate_process_tree, "terminate_tree_reap_ledger_per_phase"),
    (engine_mod._child_pgid, "child_pgid_self_group_guard"),
    (run_check_async, "run_check_async_is_the_event_loop_boundary"),
    (fan_out, "fan_out_contains_escaped_check_fault"),
    (fan_out, "fan_out_preserves_order_and_backfills_timeout"),
    (exclusive_lease, "exclusive_lease_is_busy_under_a_live_holder"),
    (leased, "leased_runs_action_only_when_held"),
    (engine_mod._claim, "claim_contention_busy_vs_steal_decision"),
    (engine_mod._steal, "claim_contention_busy_vs_steal_decision"),
    (engine_mod._snapshot, "snapshot_and_sys_pressure_pin_exact_metric_projection"),
    (engine_mod._sys_pressure, "snapshot_and_sys_pressure_pin_exact_metric_projection"),
    (engine_mod._guarded, "guarded_projects_argv_scope_and_governed_limiter_into_execute"),
    (engine_mod._dotnet_slot, "dotnet_slot_surfaces_queue_and_pressure_note"),
    (engine_mod._guarded, "guarded_fault_messages_are_stamped_exactly"),
    (engine_mod._run_process_backend, "local_spawn_arms_honor_check_cwd_and_exit_code"),
    (engine_mod._run_process_backend, "run_process_backend_routes_on_exec_target"),
    (discover, "discover_runs_at_root_and_pins_fault_evidence"),
    (argv_for, "argv_for_pins_uv_group_project_segments_and_query_passthrough"),
    (splice_command, "splice_command_cuts_before_first_separator"),
    (engine_mod._contained, "contained_verdicts_and_stage_fault_evidence"),
    (engine_mod._copy_stage_input, "contained_verdicts_and_stage_fault_evidence"),
    (engine_mod._materialize, "staged_tool_executes_from_materialized_worktree"),
    (drain_stream, "drain_stream_newline_terminus_rows"),
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
    ("zombie-holder", {"running": True, "create_time": _CT, "status": psutil.STATUS_ZOMBIE}, True),
    ("dead-holder", {"running": True, "create_time": _CT, "status": psutil.STATUS_DEAD}, True),
)


@pytest.mark.parametrize("label, proc_kw, expected", _STALE_CASES, ids=[c[0] for c in _STALE_CASES])
def test_is_lease_stale_decision_table(label: str, proc_kw: _ProcKw, expected: bool, monkeypatch: pytest.MonkeyPatch) -> None:  # noqa: FBT001
    """``is_lease_stale``: dead/not-running/create-time-drift/zombie/dead-status ⇒ True; live-and-matching ⇒ False."""
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


# --- [PROC_DEAD]

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
    monkeypatch.setattr(engine_mod, "psutil", _make_psutil_module({4242: _proc(pid=4242, **proc_kw)}))
    assert proc_dead(4242) is expected


def test_proc_dead_corrupt_pid_is_dead() -> None:
    """``psutil.Process(pid<=0)`` raises ``ValueError``; a corrupt marker/owner pid folds to dead, never raises."""
    assert proc_dead(-1) is True


def test_proc_dead_access_denied_defers_to_pid_exists(monkeypatch: pytest.MonkeyPatch) -> None:
    """``AccessDenied`` proves the OS still owns the pid: the verdict defers to ``pid_exists`` instead of declaring death."""
    guarded = _proc(pid=4242)
    guarded.status.side_effect = psutil.AccessDenied(pid=4242)
    fake = _make_psutil_module({4242: guarded})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    fake.pid_exists.return_value = True
    assert proc_dead(4242) is False
    fake.pid_exists.return_value = False
    assert proc_dead(4242) is True


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
    # Pin system RAM and the foreign-dotnet census below their ceilings so the cap table stays deterministic on a loaded host.
    monkeypatch.setattr(engine_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=50.0))
    monkeypatch.setattr(engine_mod, "_foreign_dotnet_count", lambda: 0)
    settings = assay_root.settings.model_copy(
        update={"cpu_count": cpu_count, "max_checks": max_checks, "dotnet_max_cpu": dotnet, "mutation_max_cpu": mutation}
    )
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=r, mode=m)) for r, m in runner_modes)
    assert governed_concurrency(settings, checks) == expected


def test_governed_concurrency_halves_under_memory_pressure(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, log_events: list[dict[str, object]]
) -> None:
    """At ≥ 90% system RAM the folded limit halves (floor 1) and each pressured fold emits ``concurrency.backpressure``."""
    monkeypatch.setattr(engine_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=92.5))
    wide = assay_root.settings.model_copy(update={"cpu_count": 8, "max_checks": 8})
    narrow = assay_root.settings.model_copy(update={"cpu_count": 1, "max_checks": 8})
    assert governed_concurrency(wide, ()) == 4, "pressured limit did not halve"
    assert governed_concurrency(narrow, ()) == 1, "halving broke the floor of 1"
    monkeypatch.setattr(engine_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=89.9))
    assert governed_concurrency(wide, ()) == 8, "sub-ceiling memory must not halve"
    events = tuple(e for e in log_events if e.get("event") == "concurrency.backpressure")
    assert len(events) == 2, f"backpressure telemetry not emitted once per pressured fold: {events!r}"
    assert (events[0].get("limit"), events[0].get("backpressure_limit"), events[0].get("sys_mem_percent")) == (8, 4, 92.5)


def test_governed_concurrency_halves_under_foreign_dotnet_census(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, log_events: list[dict[str, object]]
) -> None:
    """A foreign dotnet-family census at/above ``cpu_count`` halves DOTNET batches only; non-DOTNET batches never census.

    Kills the gate flips: census applied to non-DOTNET batches (the `if any(...DOTNET...)` guard),
    the `>=` -> `>` threshold mutation, and dropped telemetry fields on the census arm.
    """
    monkeypatch.setattr(engine_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=50.0))
    monkeypatch.setattr(engine_mod, "_foreign_dotnet_count", lambda: 8)
    settings = assay_root.settings.model_copy(update={"cpu_count": 8, "max_checks": 8, "dotnet_max_cpu": 8})
    dotnet_batch = (Check(tool=msgspec.structs.replace(_ECHO_TOOL, runner=Runner.DOTNET)),)
    assert governed_concurrency(settings, dotnet_batch) == 4, "census at cpu_count did not halve a DOTNET batch"
    assert governed_concurrency(settings, ()) == 8, "census must not throttle a batch with no DOTNET runner"
    monkeypatch.setattr(engine_mod, "_foreign_dotnet_count", lambda: 7)
    assert governed_concurrency(settings, dotnet_batch) == 8, "sub-threshold census must not halve"
    event = next(e for e in log_events if e.get("event") == "concurrency.backpressure")
    assert (event.get("foreign_dotnet"), event.get("dotnet_pressure"), event.get("mem_pressure")) == (8, True, False)


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
    argv = assert_ok(argv_for(Check(tool=tool), routed, settings=assay_root.settings, scope=scope))
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


# --- [APPHOST_ENV]


@pytest.fixture
def _fresh_dotnet_root() -> Iterator[None]:
    """Reset the process-cached dotnet-root probe around a test that steers its sources."""
    engine_mod._dotnet_root.cache_clear()
    yield
    engine_mod._dotnet_root.cache_clear()


def _runtime_tree(base: Path) -> Path:
    """Materialize ``shared/Microsoft.NETCore.App`` under base so the probe's validity check passes.

    Returns:
        The base path, now a valid dotnet runtime root.
    """
    (base / "shared" / "Microsoft.NETCore.App").mkdir(parents=True)
    return base


def _dotnet_env_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # Valid DOTNET_ROOT short-circuits: the listing probe must never spawn.
    root = _runtime_tree(tmp_path / "env-root")
    mp.setenv("DOTNET_ROOT", str(root))
    mp.setattr(engine_mod, "discover", lambda *_a, **_kw: (_ for _ in ()).throw(AssertionError("probe must not run")))
    return str(root)


def _dotnet_listing_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # Invalid env root falls through to the runtime listing, whose bracketed path resolves two levels up.
    real = _runtime_tree(tmp_path / "real-root")
    mp.setenv("DOTNET_ROOT", str(tmp_path / "absent"))
    listing = f"Microsoft.NETCore.App 10.0.0 [{real}/shared/Microsoft.NETCore.App]\nnoise\n"
    mp.setattr(engine_mod, "discover", lambda *_a, **_kw: Ok(listing.encode()))
    return str(real)


def _dotnet_muxer_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # No env override + a failing listing leaves the muxer's resolved parent as the last candidate.
    real = _runtime_tree(tmp_path / "muxer-root")
    (real / "dotnet").write_bytes(b"")
    mp.delenv("DOTNET_ROOT", raising=False)
    mp.setattr(engine_mod, "discover", lambda *_a, **_kw: Error(Fault(("dotnet", "--list-runtimes"), RailStatus.FAULTED)))
    mp.setattr(engine_mod.shutil, "which", lambda _name: str(real / "dotnet"))
    return str(real)


@pytest.mark.usefixtures("_fresh_dotnet_root")
@pytest.mark.parametrize("setup", [_dotnet_env_wins, _dotnet_listing_wins, _dotnet_muxer_wins], ids=["env", "listing", "muxer"])
def test_dotnet_root_probe_precedence(setup: Callable[[Path, pytest.MonkeyPatch], str], tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_dotnet_root`` walks env → ``--list-runtimes`` → muxer parent, taking the first that resolves a real runtime tree.

    Each row pins one precedence rung: a valid env short-circuits the probe; an invalid env falls to the
    bracketed listing path resolved two levels up; a failing listing falls to the muxer's resolved parent.
    """
    expected = setup(tmp_path, monkeypatch)
    assert engine_mod._dotnet_root() == expected


def test_apphost_overlays_tool_run_heads_only(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """``("tool", "run")`` DOTNET heads gain ``DOTNET_ROOT`` + ``DOTNET_MULTILEVEL_LOOKUP=0``; every other head passes through identically."""
    root = _runtime_tree(tmp_path / "env-root")
    monkeypatch.setattr(engine_mod, "_dotnet_root", lambda: str(root))
    base = {"PATH": "/bin"}
    assert engine_mod._apphost(_TOOL_RUN, base) == {"PATH": "/bin", "DOTNET_ROOT": str(root), "DOTNET_MULTILEVEL_LOOKUP": "0"}
    sdk = msgspec.structs.replace(_TOOL_RUN, command=("build", "--no-restore"))
    assert engine_mod._apphost(sdk, base) is base
    assert engine_mod._apphost(_ECHO_TOOL, base) is base


def test_apphost_strips_unresolvable_root_for_tool_run(monkeypatch: pytest.MonkeyPatch) -> None:
    """When no runtime root resolves, a ``("tool", "run")`` head sheds an inherited (possibly nix-broken) ``DOTNET_ROOT``."""
    monkeypatch.setattr(engine_mod, "_dotnet_root", lambda: None)
    assert engine_mod._apphost(_TOOL_RUN, {"DOTNET_ROOT": "/nix/store/garbage", "PATH": "/bin"}) == {"PATH": "/bin"}


# --- [RUN_CHECK_FAN_OUT]


def test_run_check_executes_direct_tool(assay_root: AssayHarness) -> None:
    """``run_check`` runs a DIRECT tool under its own event loop and returns an Ok receipt with the tool stdout."""
    assert b"hello" in assert_ok(_run(Check(tool=_ECHO_TOOL), assay_root)).stdout


def test_run_check_injects_traceparent_into_subprocess_env(assay_root: AssayHarness) -> None:
    """W3C trace context crosses the spawn boundary: the woven span is injected as a lowercase ``traceparent`` child env entry."""
    tool = msgspec.structs.replace(_ECHO_TOOL, name="env-dump", command=("/usr/bin/env",))
    done = assert_ok(_run(Check(tool=tool), assay_root))
    assert b"traceparent=00-" in done.stdout, f"traceparent missing from subprocess env: {done.stdout[:400]!r}"


def test_run_check_merges_row_env_into_spawned_process(assay_root: AssayHarness) -> None:
    """A ``Tool.env`` row reaches the spawned process environment; a row without ``env`` leaves the base untouched.

    Falsifiable pair: the env-carrying tool dumps ``ASSAY_ROW_PROBE=row-value``; the bare tool — identical but for
    ``env=()`` — must not, proving the merge is row-scoped and never a global leak.
    """
    carrying = msgspec.structs.replace(_ECHO_TOOL, name="env-row", command=("/usr/bin/env",), env=(("ASSAY_ROW_PROBE", "row-value"),))
    bare = msgspec.structs.replace(carrying, env=())
    with_env = assert_ok(_run(Check(tool=carrying), assay_root)).stdout
    without_env = assert_ok(_run(Check(tool=bare), assay_root)).stdout
    assert b"ASSAY_ROW_PROBE=row-value" in with_env, f"row env did not reach the spawned process: {with_env[:400]!r}"
    assert b"ASSAY_ROW_PROBE=" not in without_env, f"row env leaked into a tool that declared none: {without_env[:400]!r}"


@pytest.mark.parametrize("mode", [Mode.CHECK, Mode.BUILD], ids=["run-process-arm", "open-process-arm"])
def test_spawned_children_detach_into_own_session(mode: Mode, assay_root: AssayHarness) -> None:
    """Both local spawn backends pass ``start_new_session=True``: the child leads its own process group.

    The no-orphan guarantee rests on this — group-kill reaches the whole child tree and can never
    reach the engine's own group.
    """
    probe = (sys.executable, "-c", "import os; print(os.getpgid(0))")
    tool = Tool("session-law", Runner.DIRECT, probe, Input.NONE, Language.PYTHON, Claim.STATIC, mode=mode)
    done = assert_ok(_run(Check(tool=tool), assay_root))
    assert int(done.stdout.split()[0]) != os.getpgid(0), f"{mode}: child shares the engine's process group"  # ty: ignore[possibly-missing-attribute]


def test_discover_detaches_child_session(tmp_path: Path) -> None:
    """``discover`` rides the same ``start_new_session`` spawn posture as the check backends."""
    out = assert_ok(discover((sys.executable, "-c", "import os; print(os.getpgid(0))"), root=tmp_path, timeout=10.0))
    assert int(out.split()[0]) != os.getpgid(0), "discovery child shares the engine's process group"  # ty: ignore[possibly-missing-attribute]


@pytest.mark.anyio
async def test_run_check_async_is_the_event_loop_boundary(assay_root: AssayHarness) -> None:
    """``run_check_async`` is the public in-loop entrypoint — async callers never import the woven spawn."""
    outcome = await run_check_async(Check(tool=_ECHO_TOOL), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)
    assert b"hello" in assert_ok(outcome).stdout


@pytest.mark.anyio
async def test_dotnet_slot_surfaces_queue_and_pressure_note(assay_root: AssayHarness) -> None:
    """The machine-wide dotnet slot pool emits the slot, wait, census, and concurrency decision as receipt notes."""
    async with engine_mod._dotnet_slot(Check(tool=_REMOTE_TOOL), assay_root.settings, None) as acquired:
        notes = assert_ok(acquired)
    joined = " ".join(notes)
    assert "dotnet.slot" in joined
    assert "wait_ms=" in joined
    assert "slots=" in joined
    assert "foreign_dotnet=" in joined
    assert "mem_percent=" in joined
    assert "original_concurrency=" in joined
    assert "reduced_concurrency=" in joined


def test_run_check_retries_transient_spawn_via_rail_probe(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, log_events: list[dict[str, object]]
) -> None:
    """A transient spawn fault on a remote runner is retried; the receipt carries attempt evidence and retry telemetry names the tool.

    The ``with_name`` attribution pin: stamina's StructlogOnRetryHook must log the tool name, not ``<context block>``.
    """
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
    scheduled = tuple(e for e in log_events if e.get("event") == "stamina.retry_scheduled")
    assert scheduled, "stamina retry hook emitted no telemetry"
    assert scheduled[0].get("callable") == _REMOTE_TOOL.name, f"retry telemetry not attributed to the tool: {scheduled[0]!r}"


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


def test_fan_out_contains_escaped_check_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """An unclassified exception escaping one check is contained to its FAULTED slot; sibling slots still complete."""

    async def volatile(check: Check, *_args: object, **_kwargs: object) -> Completed:
        await anyio.sleep(0.0)
        match check.tool.name.endswith("-1"):
            case True:
                raise RuntimeError("escaped check fault")
            case False:
                return receipt((check.tool.name,), 0)

    monkeypatch.setattr(engine_mod, "_execute", volatile)
    checks = tuple(Check(tool=msgspec.structs.replace(_ECHO_TOOL, name=f"contain-{i}")) for i in range(3))
    results = fan_out(checks, settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED)
    assert_ok(results[0])
    fault = assert_error_status(results[1], RailStatus.FAULTED)
    assert "RuntimeError" in fault.message, f"escaped exception type lost from the contained slot: {fault!r}"
    assert_ok(results[2])


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
    model_based(LeaseStateMachine)


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
    artifact = next((a for a in done.artifacts if a.id.startswith(f"{label}-tool-") and a.id.endswith("-out")), None)
    match scope:
        case None:
            assert done.artifacts == (), f"scope-less stream emitted artifacts: {done.artifacts!r}"
        case _:
            assert artifact is not None, f"no persisted stdout artifact in {done.artifacts!r}"
            assert scope.store.read_path(artifact.path) == payload, "persisted artifact lost the streamed bytes"


def test_nonstreaming_scoped_process_persists_output_artifacts(assay_root: AssayHarness) -> None:
    """Scoped non-streaming tools persist full stdout/stderr artifacts while receipts carry bounded tails."""
    payload = b"x" * (assay_root.settings.stream_tail_bytes + 8)
    script = f"import sys; sys.stdout.buffer.write({payload!r}); sys.stderr.buffer.write(b'err-tail')"
    tool = Tool("nonstream-artifact-law", Runner.DIRECT, (sys.executable, "-c", script), Input.NONE, Language.PYTHON, Claim.STATIC)
    scope = assay_root.scope(Claim.STATIC)
    done = assert_ok(_run(Check(tool=tool), assay_root, scope=scope))
    assert done.stdout == payload[-assay_root.settings.stream_tail_bytes :]
    artifact = next((a for a in done.artifacts if a.id.startswith("nonstream-artifact-law-") and a.id.endswith("-out")), None)
    assert artifact is not None, f"non-streaming process emitted no stdout artifact: {done.artifacts!r}"
    assert scope.store.read_path(artifact.path) == payload


# --- [STALL_TELEMETRY]

_STALL_RUNS: tuple[tuple[str, tuple[str, ...], bool], ...] = (
    ("silent-slow-notes-once", (sys.executable, "-c", "import time; time.sleep(1.2)"), True),
    ("fast-child-no-note", ("/bin/echo", "fast-ok"), False),
)


def test_stall_verdict_triage_table() -> None:
    """``_stall_verdict`` two-sample triage: cpu-bound precedes disk-wait precedes scheduler-contention; the quiet floor is io-or-lock-wait.

    The cpu rows consume exactly one core-window (``_STALL_SAMPLE_S`` seconds of cpu time) so the rendered
    percentage is constant-independent; the contention row sits exactly on the 100/s inclusive boundary;
    the reversed row pins the negative-delta clamp (a shrunken second tree must read as quiet, not phantom-busy).
    """
    window = engine_mod._STALL_SAMPLE_S

    def sample(cpu_s: float = 0.0, invol: float = 0.0, status: str = psutil.STATUS_RUNNING, procs: int = 1) -> engine_mod._StallSample:
        return engine_mod._StallSample(cpu_s=cpu_s, invol=invol, status=status, procs=procs)

    idle = sample()
    cases: tuple[ProjectionCase[tuple[engine_mod._StallSample, engine_mod._StallSample]], ...] = (
        ProjectionCase(
            label="cpu-bound",
            intent=(idle, sample(cpu_s=window, procs=3)),
            supported_out="cpu-bound (100% of one core, 3 procs)",
            oracle=None,
            unsupported_out="",
        ),
        ProjectionCase(
            label="cpu-bound-precedes-disk-wait",
            intent=(idle, sample(cpu_s=window, status=psutil.STATUS_DISK_SLEEP, procs=2)),
            supported_out="cpu-bound (100% of one core, 2 procs)",
            oracle=None,
            unsupported_out="",
        ),
        ProjectionCase(
            label="disk-wait", intent=(idle, sample(status=psutil.STATUS_DISK_SLEEP)), supported_out="disk-wait", oracle=None, unsupported_out=""
        ),
        ProjectionCase(
            label="scheduler-contention-inclusive-boundary",
            intent=(idle, sample(invol=window * 100.0)),
            supported_out="scheduler-contention",
            oracle=None,
            unsupported_out="",
        ),
        ProjectionCase(label="io-or-lock-wait", intent=(idle, sample()), supported_out="io-or-lock-wait", oracle=None, unsupported_out=""),
        ProjectionCase(
            label="negative-delta-clamps-quiet",
            intent=(sample(cpu_s=9.0, invol=9_999.0, procs=4), idle),
            supported_out="io-or-lock-wait",
            oracle=None,
            unsupported_out="",
        ),
    )

    def project(intent: tuple[engine_mod._StallSample, engine_mod._StallSample]) -> str:
        first, second = intent
        return engine_mod._stall_verdict(first, second)

    projection_matrix(cases, project)


def test_stall_sample_aggregates_process_tree(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_stall_sample`` sums ``cpu_times`` (user+system) and involuntary switches across root + recursive children.

    A per-process probe fault contributes its zero default while the rest of the tree still counts; a vanished
    pid (``NoSuchProcess``) degrades through ``_safe_call`` to the empty zero sample rather than raising.
    """
    root, kid, flaky = _proc(pid=777), _proc(pid=778), _proc(pid=779)
    root.cpu_times.return_value = SimpleNamespace(user=1.0, system=0.5)
    kid.cpu_times.return_value = SimpleNamespace(user=2.0, system=0.25)
    root.num_ctx_switches.return_value = SimpleNamespace(voluntary=1, involuntary=10)
    kid.num_ctx_switches.return_value = SimpleNamespace(voluntary=2, involuntary=32)
    flaky.cpu_times.side_effect = psutil.Error("times down")
    flaky.num_ctx_switches.side_effect = NotImplementedError("off-platform probe")
    root.children.return_value = (kid, flaky)
    monkeypatch.setattr(engine_mod, "psutil", _make_psutil_module({777: root}))
    sample = engine_mod._stall_sample(777)
    expected = engine_mod._StallSample(cpu_s=3.75, invol=42.0, status=psutil.STATUS_RUNNING, procs=3)
    assert sample == expected, f"tree aggregate wrong: {sample!r}"
    empty = engine_mod._stall_sample(2_147_483_646)
    assert empty == engine_mod._StallSample(cpu_s=0.0, invol=0.0, status="", procs=0), f"vanished pid did not degrade: {empty!r}"


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

    A deliberately silent slow child earns exactly ONE bounded ``proc.stall`` receipt note plus the structlog
    event and the span event; a fast child earns none — the false-positive pin for always-on telemetry.
    """
    monkeypatch.setattr(engine_mod, "_STALL_AFTER_S", 0.2)
    monkeypatch.setattr(engine_mod, "_STALL_SAMPLE_S", 0.05)
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


def test_terminate_process_tree_kills_terminate_resistant_child(log_events: list[dict[str, object]]) -> None:
    """SIGTERM → SIGKILL ladder for a resistant survivor; a ``proc.reaped`` ledger line is raised through each ``wait_procs`` phase."""
    survivor = _proc(pid=4242, running=True)
    already_dead = _proc(pid=4243, running=False)
    fake = _make_psutil_module({4242: survivor, 4243: already_dead})
    fake.wait_procs.side_effect = (((already_dead,), (survivor,)), ((survivor,), ()))
    with pytest.MonkeyPatch.context() as patch:
        patch.setattr(engine_mod, "psutil", fake)
        engine_mod._terminate_process_tree((already_dead, survivor), None)
    sent = tuple(call.args[0] for call in survivor.send_signal.call_args_list)
    assert sent == (signal.SIGTERM, signal.SIGKILL), f"ladder escalation wrong: {sent!r}"  # ty: ignore[possibly-missing-attribute]
    already_dead.send_signal.assert_not_called()
    ledger = tuple((e.get("killed"), e.get("survived")) for e in log_events if e.get("event") == "proc.reaped")
    assert ledger == ((1, 1), (1, 0)), f"reap ledger not raised through both wait phases: {ledger!r}"


def test_child_pgid_guards_engine_group() -> None:
    """``_child_pgid`` yields None for the engine's own group (killpg would suicide) and for vanished pids; a session-leader child owns its pgid."""
    assert engine_mod._child_pgid(os.getpid()) is None, "own process group must never be group-killed"
    assert engine_mod._child_pgid(2_147_483_646) is None, "vanished pid must degrade to the walk fallback"

    async def _spawn() -> tuple[int | None, int]:
        proc = await anyio.open_process([sys.executable, "-c", "import time; time.sleep(30)"], start_new_session=True)
        try:
            return engine_mod._child_pgid(proc.pid), proc.pid
        finally:
            await engine_mod._reap(proc)

    pgid, pid = anyio.run(_spawn)
    assert pgid == pid, "session-leader child must own its process group"


# --- [DIAGNOSE_SNAPSHOT]


def test_diagnose_records_resource_snapshot(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_diagnose`` folds the real ``_snapshot`` chain onto the active span event (``mem.rss_bytes``).

    Also seeds the ``_RESOURCE`` ContextVar for the cross-``anyio.run`` Fault build.
    """
    fake = _make_psutil_module({None: _proc(rss=65536)})
    monkeypatch.setattr(engine_mod, "psutil", fake)
    events: list[tuple[str, dict[str, object]]] = []
    exceptions: list[BaseException] = []
    span = SimpleNamespace(record_exception=exceptions.append, add_event=lambda name, attributes: events.append((name, attributes)))
    monkeypatch.setattr(trace, "get_current_span", lambda: span)
    token = engine_mod._RESOURCE.set(())
    ring_token = _RING.set(deque(("info:probe.start", "warning:probe.fault")))
    try:
        exc = TimeoutError("synthetic diagnose timeout")
        engine_mod._diagnose(exc)
        assert exceptions == [exc], "exception not recorded on the fault span"
        name, attrs = events[0]
        assert (name, attrs.get("mem.rss_bytes")) == (engine_mod._FAULT_SNAPSHOT, 65536.0), f"snapshot event wrong: {name!r} {attrs!r}"
        assert events[1] == (engine_mod._RING_SNAPSHOT, {"events": ("info:probe.start", "warning:probe.fault")}), (
            f"ring event not built from ring_recent(): {events[1]!r}"
        )
        rss = dict(engine_mod._RESOURCE.get())["mem.rss_bytes"]
        assert rss == pytest.approx(65536.0), "resource ContextVar not seeded from the snapshot"
    finally:
        _RING.reset(ring_token)
        engine_mod._RESOURCE.reset(token)


def test_snapshot_pressure_arms_degrade_to_empty(monkeypatch: pytest.MonkeyPatch) -> None:
    """The snapshot is best-effort evidence, never a second fault source.

    ``_sys_pressure`` degrades its memory arm independently of the load arm, ``_children`` swallows
    ``psutil.Error`` to ``{}``, ``_safe_call`` absorbs ``NotImplementedError`` (psutil's off-POSIX
    ``num_fds`` signal) to its default while ``proc.num_fds`` stays a snapshot key, ``_as_bytes``
    projects str/None/bytes.
    """
    fake = _make_psutil_module({None: _proc(rss=4096)})
    fake.virtual_memory.side_effect = psutil.Error("vm down")
    fake.swap_memory.side_effect = psutil.Error("swap down")
    monkeypatch.setattr(engine_mod, "psutil", fake)
    fd_proc = _proc()
    fd_proc.num_fds.side_effect = NotImplementedError("no num_fds")
    kids_proc = _proc()
    kids_proc.children.side_effect = psutil.Error("children down")

    def _fault() -> float:
        raise ValueError("probe down")

    support_matrix(
        ("sys-pressure-mem-fault→load-only", lambda: set(engine_mod._sys_pressure()) == {"sys.load1_percent"}, True),
        ("children-fault→{}", lambda: engine_mod._children(kids_proc) == {}, True),
        ("num-fds-not-implemented→default", lambda: engine_mod._safe_call(lambda: float(fd_proc.num_fds()), -1.0) == pytest.approx(-1.0), True),
        ("snapshot-keeps-num-fds-key", lambda: "proc.num_fds" in engine_mod._snapshot(), True),
        ("safe-call-value-arm", lambda: engine_mod._safe_call(lambda: 7.0, -1.0) == pytest.approx(7.0), True),
        ("safe-call-default-arm", lambda: engine_mod._safe_call(_fault, -1.0) == pytest.approx(-1.0), True),
        (
            "as-bytes-projection",
            lambda: (engine_mod._as_bytes(b"x"), engine_mod._as_bytes(None), engine_mod._as_bytes("s")) == (b"x", b"", b"s"),
            True,
        ),
    )


def test_sys_pressure_load_arm_degrades_without_getloadavg(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_sys_pressure`` omits ``sys.load1_percent`` when ``os.getloadavg`` is absent and when it raises ``OSError``."""

    def _boom() -> tuple[float, float, float]:
        raise OSError("load unavailable")

    monkeypatch.setattr(os, "getloadavg", _boom, raising=False)
    assert "sys.load1_percent" not in engine_mod._sys_pressure(), "failing getloadavg not degraded to an absent key"
    monkeypatch.delattr(os, "getloadavg", raising=False)
    assert "sys.load1_percent" not in engine_mod._sys_pressure(), "absent getloadavg not degraded to an absent key"


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
async def test_run_check_remote_round_trips_through_ssh_double(assay_root: AssayHarness, ssh_env: SshEnv) -> None:
    """The remote arm shell-quotes argv through ``remote_command`` and returns the ssh double's reply (non-streaming ``conn.run`` arm)."""
    remote = assay_root.remote(ssh_env.url)
    check = Check(tool=_ECHO_TOOL, cwd=assay_root.root)
    # run_check drives its own anyio.run loop; bridge to a thread to avoid nested event loops under anyio.
    done = assert_ok(await anyio.to_thread.run_sync(lambda: run_check(check, settings=remote, scope=None, routed=_ROUTED_CHANGED)))  # ty: ignore[unresolved-attribute]
    assert (b"remote-ok:" in done.stdout, done.returncode) == (True, 0), f"ssh double reply missing from {done.stdout!r}"


@pytest.mark.anyio
@pytest.mark.parametrize("scoped", [False, True], ids=["non-persisted", "scoped-persists-artifact"])
async def test_run_check_remote_streaming_round_trips(
    scoped: bool,  # noqa: FBT001
    assay_root: AssayHarness,
    ssh_env: SshEnv,
) -> None:
    """The remote streaming arm tees the ssh double's reply through ``_recv_ssh`` + ``drain_stream`` and tail-caps the receipt.

    The scoped row drives the with-handle arm: ``_stream_writer`` sink + ``_stream_artifacts`` persist.
    """
    remote = assay_root.remote(ssh_env.url)
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
            assert b"remote-ok:" in scope.store.read_path(artifact.path), "persisted remote artifact lost the ssh double reply"


@pytest.mark.anyio
async def test_fan_out_remote_pools_ssh_connection(assay_root: AssayHarness, ssh_env: SshEnv) -> None:
    """``fan_out`` over a remote runner pools one ssh connection across workers and ``_pooled_ssh`` closes it on scope exit.

    First opens, second reuses the ``_SshCache`` arm; both slots round-trip the double's reply (pooled-reuse + close-loop path).
    """
    remote = assay_root.remote(ssh_env.url)
    base = Tool("remote-fan-law", Runner.DIRECT, ("/bin/echo", "hi"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.CHECK)
    checks = tuple(Check(tool=msgspec.structs.replace(base, name=f"remote-fan-{i}"), cwd=assay_root.root) for i in range(2))

    def _sync() -> tuple[Result[Completed, Fault], ...]:
        return fan_out(checks, settings=remote, scope=None, routed=_ROUTED_CHANGED, deadline=time.monotonic() + 10.0)

    results = await anyio.to_thread.run_sync(_sync)  # ty: ignore[unresolved-attribute]
    assert len(results) == 2, f"fan_out lost a remote slot: {results!r}"
    for index, result in enumerate(results):
        assert b"remote-ok:" in assert_ok(result).stdout, f"remote slot {index} missing ssh double reply"


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
    """``_recv_anyio(None, ...)`` (the inherited-fd / absent-pipe arm) drains to the empty capture immediately."""
    assert anyio.run(lambda: drain_stream(engine_mod._recv_anyio(None, 32), tail_cap=128)) == Captured(), "None stream did not drain to empty"


def test_total_backfills_none_slot_as_timeout() -> None:
    """``_total`` back-fills a never-completed fan-out slot (``None``) as ``TIMEOUT``; a present ``Result`` rides through by identity."""
    backfilled = engine_mod._total(None)
    present = Ok(receipt(("echo",), 0))
    assert_error_status(backfilled, RailStatus.TIMEOUT)
    assert engine_mod._total(present) is present, "present slot not passed through by identity"


def test_children_sums_resident_set_sizes() -> None:
    """``_children`` counts the recursive child set and folds each child's RSS through the ``_safe_call`` guard."""
    parent, kid_a, kid_b = _proc(), _proc(rss=8192), _proc(rss=4096)
    parent.children.return_value = (kid_a, kid_b)
    assert engine_mod._children(parent) == {"proc.children": 2.0, "proc.children_rss_bytes": 12288.0}


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


# --- [MUTATION_HARDENING]


@pytest.mark.mutation
def test_claim_contention_busy_vs_steal_decision(  # noqa: PLR0915  # three contention scenarios share one scripted flock + fd harness
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch
) -> None:
    """``_claim`` contention dispatch: live holder → BUSY without a second flock; stale holder → steal rewrites every owner field.

    Kills the survivor classes on the lease write path: the live/stale match-arm flip (steal suppressed or
    BUSY inverted), dropped or nulled keyword forwards through ``_claim`` → ``_steal`` → ``_write_owner``
    (run_id/cwd/project/mode/target), and free-path keyword drops — each changes the persisted owner block
    bytes or the flock call count pinned here.
    """
    self_pid, live_pid, dead_pid = os.getpid(), 88_778, 88_777
    self_proc = _proc(pid=self_pid, create_time=_CT)
    fake = _make_psutil_module({
        None: self_proc,
        self_pid: self_proc,
        live_pid: _proc(pid=live_pid, running=True, create_time=_CT),
        dead_pid: _proc(pid=dead_pid, raise_no_such=True),
    })
    monkeypatch.setattr(engine_mod, "psutil", fake)
    flock_calls: list[int] = []
    busy_first = [False]

    def scripted_flock(_fd: int, flags: int) -> None:
        flock_calls.append(flags)
        if busy_first[0] and len(flock_calls) == 1:
            raise BlockingIOError

    monkeypatch.setattr(engine_mod, "_FLOCK", scripted_flock)

    def claim(name: str, holder_pid: int | None, *, busy: bool) -> tuple[engine_mod._LeaseOwner | None, engine_mod._LeaseOwner | None, int]:
        flock_calls.clear()
        busy_first[0] = busy
        fd = os.open(str(assay_root.root / f"{name}.lock"), os.O_RDWR | os.O_CREAT, 0o644)
        try:
            if holder_pid is not None:
                _ = os.write(fd, msgspec.json.encode(engine_mod._LeaseOwner(resource=name, run_id="holder", pid=holder_pid, create_time=_CT)))
                _ = os.lseek(fd, 0, os.SEEK_SET)
            won = engine_mod._claim(
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


def test_snapshot_and_sys_pressure_pin_exact_metric_projection(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_snapshot``/``_sys_pressure`` project the exact key→value evidence map from a deterministic psutil double.

    Kills the snapshot survivor classes: key renames (``XXmem.vms_bytesXX``), probe bodies nulled to
    ``float(None)`` (which degrade to the -1.0 default and diverge from the pinned values), the
    ``percent_rss`` ``*``/``/`` operator flip, and the ``getloadavg()[0]`` → ``[1]`` index drift — each
    changes one pinned dict entry.
    """
    proc = _proc(pid=os.getpid())
    proc.memory_info.return_value = SimpleNamespace(rss=2048, vms=4096)
    proc.memory_full_info.return_value = SimpleNamespace(uss=1024)
    proc.num_fds.return_value = 6
    proc.num_threads.return_value = 3
    fake = _make_psutil_module({None: proc})
    fake.virtual_memory.return_value = SimpleNamespace(total=8192, available=4096, percent=37.5)
    fake.swap_memory.return_value = SimpleNamespace(percent=12.5)
    monkeypatch.setattr(engine_mod, "psutil", fake)
    monkeypatch.setattr(os, "getloadavg", lambda: (2.0, 9.0, 9.0), raising=False)
    pressure = {"sys.mem_available_bytes": 4096.0, "sys.mem_percent": 37.5, "sys.swap_percent": 12.5, "sys.load1_percent": 50.0}
    assert engine_mod._sys_pressure() == pressure, "sys pressure projection drifted from the doubled sources"
    assert engine_mod._snapshot() == {
        "mem.rss_bytes": 2048.0,
        "mem.vms_bytes": 4096.0,
        "mem.uss_bytes": 1024.0,
        "mem.percent_rss": 25.0,
        "proc.num_fds": 6.0,
        "proc.num_threads": 3.0,
        **pressure,
    }, "snapshot projection drifted from the doubled process"


def test_guarded_projects_argv_scope_and_governed_limiter_into_execute(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The spawned argv equals the public ``argv_for`` projection, the scope rides intact, and the limiter carries the governed cap.

    Kills ``_guarded``'s ``scope=None`` at the ``_argv`` call site, the ``governed_concurrency(settings,)``
    checks-drop on the limiter, scope drops through ``_execute_retrying``/``_fan_schedule``, and argv
    perturbations between ``_guarded`` and ``_execute`` — each diverges from the captured triple.
    """
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

    monkeypatch.setattr(engine_mod, "_execute", capture)
    monkeypatch.setattr(engine_mod.psutil, "virtual_memory", lambda: SimpleNamespace(percent=50.0))
    monkeypatch.setattr(engine_mod, "_foreign_dotnet_count", lambda: 0)
    settings = assay_root.settings.model_copy(update={"cpu_count": 4, "max_checks": 8, "dotnet_max_cpu": 2, "mutation_max_cpu": 8})
    scope = assay_root.scope(Claim.STATIC)
    tool = Tool("argv-proj-law", Runner.DOTNET, ("build", "Workspace.slnx"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD)
    check = Check(tool=tool)
    expected = assert_ok(argv_for(check, _ROUTED_CHANGED, settings=settings, scope=scope))
    assert set(scope.dotnet_flags) <= set(expected), "law vacuous: scope flags absent from the projected argv"
    assert_ok(run_check(check, settings=settings, scope=scope, routed=_ROUTED_CHANGED))
    assert governed_concurrency(settings, (check,)) == 2, "law vacuous: governed cap must differ from the empty-batch cap"
    assert seen[0][0] is scope, "scope did not reach _execute intact on the single-check path"
    assert (seen[0][1], seen[0][2]) == (expected, 2), f"spawned argv/limiter diverged from the projection: {seen[0]!r}"
    assert_ok(fan_out((check,), settings=settings, scope=scope, routed=_ROUTED_CHANGED)[0])
    assert seen[1][0] is scope, "scope did not reach _execute intact through the fan"
    assert (seen[1][1], seen[1][2]) == (expected, 2), f"fan argv/limiter diverged from the projection: {seen[1]!r}"


def test_guarded_fault_messages_are_stamped_exactly(assay_root: AssayHarness) -> None:
    """First-attempt spawn faults carry exact, unstamped messages plus the projected argv as fault evidence.

    Kills the ``_stamped`` ``> 1`` → ``>= 1`` flip (which suffixes ``[attempts=1]`` onto every message), the
    dropped ``message=`` keyword on the UNSUPPORTED arm, and fault-argv nulling — pinned via exact message
    equality on the deadline arm and message/argv content on the spawn arms.
    """
    deadline_tool = Tool(
        "msg-deadline", Runner.DIRECT, (sys.executable, "-c", "import time; time.sleep(10)"), Input.NONE, Language.PYTHON, Claim.TEST, timeout=0.2
    )
    check = Check(tool=deadline_tool)
    fault = assert_error_status(run_check(check, settings=assay_root.settings, scope=None, routed=_PY_CHANGED), RailStatus.TIMEOUT)
    assert fault.message == "deadline exceeded", f"deadline message not exact/unstamped: {fault.message!r}"
    assert fault.argv == assert_ok(argv_for(check, _PY_CHANGED, settings=assay_root.settings, scope=None)), f"deadline fault lost its argv: {fault!r}"
    absent = Tool("msg-absent", Runner.DIRECT, ("/nonexistent/assay-message-law",), Input.NONE, Language.PYTHON, Claim.TEST)
    fault = assert_error_status(run_check(Check(tool=absent), settings=assay_root.settings, scope=None, routed=_PY_CHANGED), RailStatus.UNSUPPORTED)
    assert "/nonexistent/assay-message-law" in fault.message, f"UNSUPPORTED message lost the missing binary: {fault.message!r}"
    assert "attempts=" not in fault.message, f"first attempt must not stamp attempt evidence: {fault.message!r}"
    nul = Tool("msg-nul", Runner.DIRECT, ("/bin/echo", "a\x00b"), Input.NONE, Language.PYTHON, Claim.TEST)
    fault = assert_error_status(run_check(Check(tool=nul), settings=assay_root.settings, scope=None, routed=_PY_CHANGED), RailStatus.FAULTED)
    assert fault.message, "FAULTED message dropped"
    assert "attempts=" not in fault.message, f"first attempt must not stamp attempt evidence: {fault.message!r}"


@pytest.mark.parametrize("mode", [Mode.CHECK, Mode.BUILD], ids=["run-process-arm", "open-process-arm"])
def test_local_spawn_arms_honor_check_cwd_and_exit_code(mode: Mode, assay_root: AssayHarness) -> None:
    """Both local spawn backends run the child in ``check.cwd`` and surface its exit code verbatim.

    Kills the dropped ``cwd=`` keyword on ``anyio.run_process``/``anyio.open_process`` (child would report
    the engine's own cwd), the ``cwd=None`` forward through ``_execute_retrying``, and the streaming-arm
    ``proc.returncode or 0`` → ``None`` returncode nulling.
    """
    workdir = assay_root.root / "spawn-cwd-law"
    workdir.mkdir(parents=True, exist_ok=True)
    probe = (sys.executable, "-c", "import os, sys; print(os.getcwd()); sys.exit(7)")
    tool = Tool("cwd-law", Runner.DIRECT, probe, Input.NONE, Language.PYTHON, Claim.TEST, mode=mode)
    done = assert_ok(run_check(Check(tool=tool, cwd=workdir), settings=assay_root.settings, scope=None, routed=_PY_CHANGED))
    assert done.returncode == 7, f"{mode}: child exit code not preserved: {done.returncode!r}"
    reported = os.path.realpath(done.stdout.decode().strip())
    assert reported == os.path.realpath(str(workdir)), f"{mode}: child ran outside check.cwd: {reported!r}"


def test_run_process_backend_routes_on_exec_target(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_run_process_backend`` dispatches to ``_run_remote`` exactly when ``exec_target`` is set — no real SSH.

    Kills the whole remote-dispatch arm at the seam itself: deleting ``case target`` (or flipping the match)
    makes a remote target silently run locally — pinned here without any ssh handshake, complementing the
    socketpair-double round-trip laws. The recorder also pins item-4's row-env forwarding: ``tool.env`` keys
    cross into the remote env projection while the ambient allowlist still gates undeclared host env.
    """
    recorded: list[tuple[str, dict[str, str]]] = []

    async def _record(plan: object, target: str) -> object:  # noqa: RUF029  # async to match _run_remote's awaited signature
        recorded.append((target, dict(getattr(plan, "env", {}))))
        return receipt(("remote",), 0, stdout=b"recorded")

    monkeypatch.setattr(engine_mod, "_run_remote", _record)
    env_tool = msgspec.structs.replace(_ECHO_TOOL, name="route-law", env=(("ASSAY_ROW_DECLARED", "row-value"),))
    monkeypatch.setenv("ASSAY_AMBIENT_UNDECLARED", "ambient-value")

    local = assert_ok(run_check(Check(tool=env_tool, cwd=assay_root.root), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED))
    assert (recorded, local.stdout) == ([], b"hello\n"), f"local target must never reach _run_remote: {recorded!r}"

    remote = assay_root.remote("ssh://x@127.0.0.1:2222")
    done = assert_ok(run_check(Check(tool=env_tool, cwd=assay_root.root), settings=remote, scope=None, routed=_ROUTED_CHANGED))
    assert (done.stdout, len(recorded)) == (b"recorded", 1), f"remote target did not route to _run_remote: {recorded!r}"
    target, remote_env = recorded[0]
    assert target == "ssh://x@127.0.0.1:2222", f"_run_remote received the wrong target: {target!r}"
    assert remote_env.get("ASSAY_ROW_DECLARED") == "row-value", f"declared row env did not cross the SSH boundary: {remote_env!r}"
    assert "ASSAY_AMBIENT_UNDECLARED" not in remote_env, f"ambient non-allowlisted env leaked across SSH: {remote_env!r}"


def test_discover_runs_at_root_and_pins_fault_evidence(tmp_path: Path) -> None:
    """``discover`` spawns at ``root`` and every fault carries the exact argv plus a pinned message.

    Kills ``discover_async``'s ``cwd=None`` spawn drop, fault-argv nulling on the timeout/nonzero arms, the
    timeout message template mutation, and the ``or`` → ``and`` stdout-fallback flip that blanks the
    nonzero-exit tail.
    """
    here = assert_ok(discover((sys.executable, "-c", "import os; print(os.getcwd())"), root=tmp_path, timeout=10.0))
    assert os.path.realpath(here.decode().strip()) == os.path.realpath(str(tmp_path)), "discovery child did not run at root"
    slow_argv = (sys.executable, "-c", "import time; time.sleep(5)")
    slow = assert_error_status(discover(slow_argv, root=tmp_path, timeout=0.2), RailStatus.TIMEOUT)
    assert (slow.argv, slow.message) == (slow_argv, "timeout after 0.2s"), f"timeout fault evidence wrong: {slow!r}"
    tail_argv = (sys.executable, "-c", "import sys; sys.stdout.write('warn-tail'); sys.exit(3)")
    tail = assert_error_status(discover(tail_argv, root=tmp_path, timeout=10.0), RailStatus.FAULTED)
    assert (tail.argv, tail.message) == (tail_argv, "warn-tail"), f"stdout-only nonzero tail lost: {tail!r}"
    absent_argv = ("/nonexistent/assay-discover-law",)
    absent = assert_error_status(discover(absent_argv, root=tmp_path, timeout=10.0), RailStatus.FAULTED)
    assert absent.argv == absent_argv, f"spawn fault lost its argv: {absent!r}"
    assert absent.message, f"spawn fault message dropped: {absent!r}"


def test_argv_for_pins_uv_group_project_segments_and_query_passthrough(assay_root: AssayHarness) -> None:
    """``_argv`` UV shaping is exact — ``--project <root>`` then ``--group <g>`` before the body — and QUERY mode never splices.

    Kills the ``--group``/``--project`` literal mutations, the UV-runner gate flips (``is not``/``or``)
    that leak segments onto non-UV runners, ``str(None)`` as the project root, and the ``mode=None``
    forward into ``splice_command`` that would splice scope flags into QUERY commands.
    """
    settings = assay_root.settings
    uv_tool = Tool(
        "uv-argv-law", Runner.UV, ("ruff", "check"), Input.NONE, Language.PYTHON, Claim.TEST, groups=("mutation",), stage=Stage(project=True)
    )
    uv_argv = assert_ok(argv_for(Check(tool=uv_tool), _PY_CHANGED, settings=settings, scope=None))
    assert uv_argv == ("uv", "run", "--project", str(settings.root), "--group", "mutation", "ruff", "check"), f"uv argv drifted: {uv_argv!r}"
    direct = msgspec.structs.replace(uv_tool, runner=Runner.DIRECT)
    direct_argv = assert_ok(argv_for(Check(tool=direct), _PY_CHANGED, settings=settings, scope=None))
    assert direct_argv == ("ruff", "check"), f"non-UV runner leaked uv segments: {direct_argv!r}"
    scope = assay_root.scope(Claim.STATIC)
    query = Tool("query-argv-law", Runner.DOTNET, ("build", "Workspace.slnx"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.QUERY)
    query_argv = assert_ok(argv_for(Check(tool=query), _ROUTED_CHANGED, settings=settings, scope=scope))
    assert query_argv == ("dotnet", "build", "Workspace.slnx"), f"QUERY mode must never splice scope flags: {query_argv!r}"


def test_splice_command_cuts_before_first_separator(assay_root: AssayHarness) -> None:
    """Scope flags splice immediately before the FIRST ``--`` separator; everything after rides verbatim.

    Kills the cut-point survivors: ``index`` → ``rindex`` (flags after the second separator),
    ``index(None)``/``index("XX--XX")`` raises, ``cut=None`` slicing, and the membership-probe mutation
    that appends flags at the tail instead of before the separator.
    """
    scope = assay_root.scope(Claim.STATIC)
    verbs = assay_root.settings.scoped_verbs
    flags = tuple(scope.dotnet_flags)
    single = splice_command(Runner.DOTNET, ("build", "App.slnx", "--", "tail-a"), scope, verbs, Mode.BUILD)
    assert single == ("build", "App.slnx", *flags, "--", "tail-a"), f"flags not spliced before the separator: {single!r}"
    double = splice_command(Runner.DOTNET, ("build", "--", "mid", "--", "tail-b"), scope, verbs, Mode.BUILD)
    assert double == ("build", *flags, "--", "mid", "--", "tail-b"), f"flags must cut at the FIRST separator: {double!r}"


@pytest.mark.mutation
def test_contained_verdicts_and_stage_fault_evidence(tmp_path: Path) -> None:
    """``_contained`` verdict table (unsafe / resolve-escape / safe) plus ``_copy_stage_input`` fault slots and messages.

    Kills the containment survivors — security-adjacent: backslash-normalization removal, weakened
    unsafe-disjunct chains, ``is_relative_to`` escape-arm flips, message-literal mutations, and the
    ``(tool, "stage", rel)`` fault-slot casing/message drops in ``_copy_stage_input``.
    """
    base = tmp_path.resolve()
    root = base / "root"
    (root / "sub").mkdir(parents=True)
    outside = base / "outside"
    outside.mkdir()
    (root / "link").symlink_to(outside)
    unsafe = ("..\\evil", "a\x00b", "/abs/path", "", ".", "..", "a//b", "nested/../escape")
    for rel in unsafe:
        verdict = engine_mod._contained(root, rel)
        assert isinstance(verdict, ValueError), f"unsafe path admitted: {rel!r} -> {verdict!r}"
        assert str(verdict) == f"unsafe stage path: {rel!r}", f"unsafe message drifted: {verdict!s}"
    escaped = engine_mod._contained(root, "link/f.txt")
    assert isinstance(escaped, ValueError), f"symlink escape admitted: {escaped!r}"
    assert str(escaped) == "stage path escaped root: 'link/f.txt'", f"escape message drifted: {escaped!s}"
    safe = engine_mod._contained(root, "sub/file.txt")
    assert safe == (root / "sub" / "file.txt").resolve(), f"safe path mis-resolved: {safe!r}"
    work = base / "work"
    work.mkdir()
    missing = engine_mod._copy_stage_input(Check(tool=_ECHO_TOOL), root, work, "sub/absent.txt")
    assert missing is not None, "missing stage input did not fault"
    assert (missing.argv, missing.message) == ((_ECHO_TOOL.name, "stage", "sub/absent.txt"), "missing stage input: sub/absent.txt"), (
        f"missing-input fault evidence wrong: {missing!r}"
    )
    breach = engine_mod._copy_stage_input(Check(tool=_ECHO_TOOL), root, work, "../x")
    assert breach is not None, "escaping stage input did not fault"
    assert (breach.argv, breach.message) == ((_ECHO_TOOL.name, "stage", "../x"), "unsafe stage path: '../x'"), (
        f"escape fault evidence wrong: {breach!r}"
    )


@pytest.mark.mutation
def test_staged_tool_executes_from_materialized_worktree(assay_root: AssayHarness) -> None:
    """A staged tool EXECUTES from the materialized worktree, not merely copies into it.

    Kills ``_materialize``'s ``cwd=None``/dropped-``cwd`` replace survivors and ``_guarded``'s removed
    Ok-arm rebind (``check = prepared``) — under each the child runs from the repo root while the worktree
    assertions of the sibling staging law still pass.
    """
    stage = Stage(root=".artifacts/python/stage-cwd-law")
    probe = (sys.executable, "-c", "import os; print(os.getcwd())")
    tool = Tool("stage-cwd-law", Runner.DIRECT, probe, Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.RUN, stage=stage)
    done = assert_ok(run_check(Check(tool=tool), settings=assay_root.settings, scope=None, routed=_PY_CHANGED))
    reported = os.path.realpath(done.stdout.decode().strip())
    worktree = os.path.realpath(str(assay_root.root / ".artifacts/python/stage-cwd-law"))
    assert reported == worktree, f"staged child ran outside the worktree: {reported!r}"


def test_drain_stream_newline_terminus_rows() -> None:
    """Deterministic newline-terminus rows for the synthetic-final-line count.

    Kills the ``last``-probe survivors the hypothesis law misses by seed luck: ``last = None``,
    ``read[-1:] and last``, and ``read[+1:]`` — each miscounts a newline-terminated or
    empty-chunk-trailing stream by one synthetic line.
    """
    rows: tuple[tuple[tuple[bytes, ...], int], ...] = (((b"ab\n",), 1), ((b"ab",), 1), ((b"ab\n", b""), 1), ((b"a\nb",), 2))
    for chunks, expected_lines in rows:
        captured = anyio.run(functools.partial(drain_stream, _recv_of(chunks), tail_cap=16))
        assert captured.lines == expected_lines, f"{chunks!r} drained to {captured.lines} lines, expected {expected_lines}"


# --- [MUTATION_LANE]


def test_mutation_lane_is_populated(request: pytest.FixtureRequest) -> None:
    """The ``mutation`` marker lane carries the engine's destructive-boundary laws so mutmut has a target set."""
    items = getattr(request.session, "items", ())
    marked = tuple(item for item in items if item.get_closest_marker("mutation") is not None)
    assert len(marked) >= 2, f"mutation lane underpopulated: {len(marked)} marked"
