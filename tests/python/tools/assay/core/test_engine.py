"""Engine laws for leases, subprocesses, SSH, streaming, argv shaping, and resource evidence.

Pure transforms use oracle laws; boundary surfaces use real subprocess, socketpair-SSH, psutil-double,
and flock seams. ``_LAWS`` registers covered symbols at import time so the MANIFEST is complete before
the policy check runs.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections import deque
import contextlib
import fcntl
import functools
from itertools import starmap
import os
from pathlib import Path
import signal
import sys
import tempfile
import time
from types import SimpleNamespace
from typing import override, TYPE_CHECKING, TypedDict

import anyio
from dirty_equals import Contains, IsInt, IsPartialDict, IsPositiveFloat
from expression import Error, Ok
import fsspec
from hypothesis import given, HealthCheck, settings as hyp_settings, strategies as st, target
from hypothesis.stateful import Bundle, consumes, invariant, rule, RuleBasedStateMachine
import msgspec
from opentelemetry import trace
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter  # noqa: TC002  # collection-time fixture annotation
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

# Hypothesis resolves fixture annotations at collection time under PEP 649.
from tests.python.tools.assay.kit import _make_psutil_module, _proc, AssayHarness, fault_st  # noqa: TC001
from tools.assay.composition.settings import AssaySettings, PullStrategy
from tools.assay.core.aspect import _RING  # ring seam seeded directly for ring-content assertions
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
    resource_projection,
    retry_predicate,
    run_check,
    run_check_async,
    splice_command,
    ssh_outcome,
    WriteSink,
)
from tools.assay.core.model import ArtifactKind, Check, Claim, Fault, Input, Language, Mode, receipt, Runner, Stage, Tool, ToolGroup
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from collections.abc import AsyncIterator, Awaitable, Callable, Iterator, Mapping, Sequence

    import asyncssh
    from expression import Result
    from fsspec.spec import AbstractFileSystem

    from tests.python._testkit.env import Provisioned
    from tools.assay.composition.settings import ArtifactScope
    from tools.assay.core.model import Completed

    type SshEnv = Provisioned[Awaitable[asyncssh.SSHClientConnection]]


# --- [TYPES] ----------------------------------------------------------------------------


class _ProcKw(TypedDict, total=False):
    """Process-double keyword payload for staleness and liveness sweeps."""

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
    (resource_projection, "resource_projection_aggregates_receipts_and_notes"),
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
    (run_check, "provision_process_suppresses_raw_artifacts"),
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
    (engine_mod._measure, "measure_and_load_info_pin_exact_metric_projection"),
    (engine_mod._load_info, "measure_and_load_info_pin_exact_metric_projection"),
    (engine_mod.Measurements, "to_resources_projects_exact_metric_evidence_eliding_degraded_arms"),
    (engine_mod.StalledProcess, "stall_sample_aggregates_process_tree"),
    (engine_mod._guarded, "guarded_projects_argv_scope_and_governed_limiter_into_execute"),
    (engine_mod._dotnet_slot, "dotnet_slot_surfaces_queue_and_pressure_note"),
    (engine_mod._guarded, "guarded_fault_messages_are_stamped_exactly"),
    (engine_mod._run_process_backend, "local_spawn_arms_honor_check_cwd_and_exit_code"),
    (engine_mod._run_process_backend, "run_process_backend_routes_on_exec_target"),
    (engine_mod._remote_transfer, "remote_transfer_pushes_manifest_then_pulls_scope_tree"),
    (engine_mod._remote_transfer, "remote_transfer_keeps_exec_cancellable_under_deadline"),
    (engine_mod._run_remote, "remote_transfer_keeps_exec_cancellable_under_deadline"),
    (engine_mod._sftp_pull_scope, "remote_transfer_pushes_manifest_then_pulls_scope_tree"),
    (engine_mod._push_repo, "remote_transfer_pushes_manifest_then_pulls_scope_tree"),
    (engine_mod._push_repo, "push_repo_pipelines_nested_tree_preserving_structure"),
    (engine_mod._grouped_by_parent, "push_repo_pipelines_nested_tree_preserving_structure"),
    (engine_mod._stale_remote_runs, "stale_remote_runs_keeps_newest_per_host_namespace"),
    (engine_mod._remote_prune, "remote_prune_sweeps_only_this_hosts_stale_run_dirs"),
    (engine_mod._sweep_remote_runs, "remote_prune_sweeps_only_this_hosts_stale_run_dirs"),
    (engine_mod._probe_toolchain, "probe_toolchain_faults_unsupported_on_missing_remote_tool"),
    (engine_mod._fold_receipt, "fold_receipt_projects_exec_facts_onto_completed"),
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
    # Host pressure is pinned below every ceiling so the cap table stays deterministic.
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

    Rows pin DOTNET-only census gating, the inclusive threshold, and emitted pressure fields.
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


def test_splice_command_isolates_project_artifact_path(assay_root: AssayHarness) -> None:
    """Project-scoped dotnet invocations get separate artifact roots under the run scope."""
    scope = assay_root.scope(Claim.TEST)
    command = ("test", "--minimum-expected-tests", "1", "--project", "tests/csharp/libs/Shape/Shape.Tests.csproj")
    spliced = splice_command(Runner.DOTNET, command, scope, assay_root.settings.scoped_verbs, Mode.RUN)
    artifact_path = spliced[spliced.index("--artifacts-path") + 1]
    assert artifact_path != scope.path
    assert artifact_path.endswith("/dotnet/tests__csharp__libs__Shape__Shape.Tests")


def test_argv_for_composes_runner_prefix_scope_and_routed_tails(assay_root: AssayHarness) -> None:
    """``argv_for`` prepends the runner prefix, threads the spliced body, and appends routed input tails."""
    scope = assay_root.scope(Claim.STATIC)
    tool = msgspec.structs.replace(_ECHO_TOOL, runner=Runner.UV, command=("ruff", "check"), input=Input.FILES)
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=("a.py", "b.py"))
    argv = assert_ok(argv_for(Check(tool=tool), routed, settings=assay_root.settings, scope=scope))
    assert argv[:2] == Runner.UV.prefix, f"runner prefix not leading argv: {argv!r}"
    assert {"ruff", "check", "a.py", "b.py"} <= set(argv), f"command body or routed tails lost in {argv!r}"


def test_argv_for_scopes_dotnet_project_tail_after_expansion(assay_root: AssayHarness) -> None:
    """Argv composition injects the per-project scope after routed ``--project`` tails are known."""
    scope = assay_root.scope(Claim.TEST)
    tool = Tool("dotnet-test", Runner.DOTNET, ("test",), Input.PROJECT, Language.CSHARP, Claim.TEST, mode=Mode.RUN)
    routed = Routed(language=Language.CSHARP, scope=Scope.FULL, projects=("tests/csharp/libs/Shape/Shape.Tests.csproj",))
    argv = assert_ok(argv_for(Check(tool=tool), routed, settings=assay_root.settings, scope=scope))
    artifact_path = argv[argv.index("--artifacts-path") + 1]
    assert artifact_path.endswith("/dotnet/tests__csharp__libs__Shape__Shape.Tests")
    assert argv[argv.index("--project") + 1] == "tests/csharp/libs/Shape/Shape.Tests.csproj"


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
    """Build a ``ByteRecv`` double yielding chunks then ``None`` at EOF.

    Returns:
        Async receiver draining ``chunks`` before signalling EOF.
    """
    pending = list(chunks)

    async def _recv() -> bytes | None:
        await anyio.sleep(0.0)
        return pending.pop(0) if pending else None

    return _recv


@given(chunks=st.lists(st.binary(max_size=64), max_size=24).map(tuple))
def test_drain_stream_aggregates_tail_size_and_lines(chunks: tuple[bytes, ...]) -> None:
    """A stdout drain under the spill cap carries the full payload while aggregating size and line totals."""
    whole = b"".join(chunks)
    captured = anyio.run(lambda: drain_stream(_recv_of(chunks), tail_cap=16, spill_cap=1 << 20, kind="out"))
    expected_lines = whole.count(b"\n") + (1 if whole and not whole.endswith(b"\n") else 0)
    assert (captured.full, captured.spilled, captured.size, captured.lines) == (whole, False, len(whole), expected_lines), (
        f"drain aggregate wrong: {captured!r}"
    )


def test_drain_stream_empty_source_is_zero_capture() -> None:
    """A ``ByteRecv`` at immediate EOF drains to the zero ``Captured`` while preserving the recorded path."""
    captured = anyio.run(lambda: drain_stream(_recv_of(()), tail_cap=16, spill_cap=1 << 20, kind="out", path="art/out.log"))
    assert captured == Captured(path="art/out.log")


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


def test_captured_defaults_are_the_empty_aggregate() -> None:
    """``Captured()`` is the empty drain identity: blank full/preview/path, no spill, and zero size/line counts."""
    assert (Captured().full, Captured().spilled, Captured().preview, Captured().path, Captured().size, Captured().lines) == (
        b"",
        False,
        b"",
        "",
        0,
        0,
    )


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
    """Reset the cached dotnet-root probe around source-precedence tests."""
    engine_mod._dotnet_root.cache_clear()
    yield
    engine_mod._dotnet_root.cache_clear()


def _runtime_tree(base: Path) -> Path:
    """Materialize a valid dotnet runtime root under ``base``.

    Returns:
        The runtime root path carrying the shared framework tree.
    """
    (base / "shared" / "Microsoft.NETCore.App").mkdir(parents=True)
    return base


def _dotnet_env_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # A valid DOTNET_ROOT short-circuits before the listing probe can spawn.
    root = _runtime_tree(tmp_path / "env-root")
    mp.setenv("DOTNET_ROOT", str(root))
    mp.setattr(engine_mod, "discover", lambda *_a, **_kw: (_ for _ in ()).throw(AssertionError("probe must not run")))
    return str(root)


def _dotnet_listing_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # Invalid DOTNET_ROOT falls through to the bracketed runtime listing path.
    real = _runtime_tree(tmp_path / "real-root")
    mp.setenv("DOTNET_ROOT", str(tmp_path / "absent"))
    listing = f"Microsoft.NETCore.App 10.0.0 [{real}/shared/Microsoft.NETCore.App]\nnoise\n"
    mp.setattr(engine_mod, "discover", lambda *_a, **_kw: Ok(listing.encode()))
    return str(real)


def _dotnet_muxer_wins(tmp_path: Path, mp: pytest.MonkeyPatch) -> str:
    # With no env root and a failed listing, the muxer parent is the final candidate.
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

    Rows pin the env short-circuit, bracketed listing path, and muxer-parent fallback.
    """
    expected = setup(tmp_path, monkeypatch)
    assert engine_mod._dotnet_root() == expected


def test_apphost_overlays_tool_run_heads_only(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """DOTNET ``tool run`` heads gain apphost env; every other head passes through."""
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


def test_run_check_injects_traceparent_into_subprocess_env(assay_root: AssayHarness, otel_spans: InMemorySpanExporter) -> None:
    """W3C trace context crosses the spawn boundary as a lowercase ``traceparent`` env entry."""
    _ = otel_spans
    tool = msgspec.structs.replace(_ECHO_TOOL, name="env-dump", command=("/usr/bin/env",))
    done = assert_ok(_run(Check(tool=tool), assay_root))
    assert b"traceparent=00-" in done.stdout, f"traceparent missing from subprocess env: {done.stdout[:400]!r}"


def test_run_check_merges_row_env_into_spawned_process(assay_root: AssayHarness) -> None:
    """A ``Tool.env`` row reaches the spawned process environment; a row without ``env`` leaves the base untouched.

    The carrying/bare pair proves the merge is row-scoped and never a global leak.
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
    """A transient remote spawn fault is retried with receipt and telemetry evidence.

    Stamina retry telemetry must attribute the retry to the tool name, not an anonymous context block.
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
    # A contended action returns FAULTED only if the BUSY short-circuit is broken.
    breach = Fault((), status=RailStatus.FAULTED, message="action ran under a held lease")
    with exclusive_lease("act", "holder", settings=assay_root.settings) as guard:
        assert_ok(guard)
        contended = leased("act", lambda _held: Error(breach), settings=assay_root.settings, run_id="run-b")
        assert_error_status(contended, RailStatus.BUSY)


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


# --- [FAULT_RAIL_PROBE]


@given(fault=fault_st)
def test_engine_fault_rail_is_error_addressable(fault: Fault) -> None:
    """The conftest ``fault_st`` alias (``resolve(Fault)``) is encode-clean and ``assert_error_status``-addressable.

    Engine ROP laws stay anchored to real bounded-message shapes.
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
    """``run_check`` routes absent binaries, deadlines, and NUL argv to their guarded fault statuses."""
    _ = label
    tool = Tool("guarded-law", Runner.DIRECT, command, Input.NONE, Language.PYTHON, Claim.TEST, mode=Mode.CHECK, timeout=timeout)
    assert_error_status(run_check(Check(tool=tool), settings=assay_root.settings, scope=None, routed=_PY_CHANGED), status)


# --- [INPROC_THUNK]


def test_inproc_thunk_outcomes(assay_root: AssayHarness) -> None:
    """``Runner.INPROC`` through ``_inproc`` classifies thunk outcomes.

    Missing and raising thunks return contained rc=1 receipts; healthy thunks return verbatim receipts.
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


def _spill_plan(assay_root: AssayHarness, scope: ArtifactScope, spill_cap: int) -> engine_mod._ExecPlan:
    """Build a scoped ``_ExecPlan`` with a small spill cap for capture-boundary laws.

    Returns:
        Streaming plan whose ``spill_cap`` forces the inline/spill split at a unit-testable threshold.
    """
    return engine_mod._ExecPlan(
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

    Both the non-streaming ``_capture_payload`` and the streaming ``drain_stream`` paths share one spill predicate.
    """
    spill_cap, scope = 64, assay_root.scope(Claim.STATIC)
    payload = b"x" * size
    plan = _spill_plan(assay_root, scope, spill_cap)
    captured = engine_mod._capture_payload(plan, "out", payload)
    assert captured.spilled is expect_spill, f"_capture_payload spill verdict wrong at size={size}: {captured!r}"
    assert (captured.full == b"") is expect_spill, f"inline ``full`` retained iff not spilled: {captured!r}"
    assert captured.read(scope.store) == payload, "capture-payload read did not resolve the full payload"

    path, handle = engine_mod._stream_writer(plan, "out")
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


# --- [STALL_TELEMETRY]

_STALL_RUNS: tuple[tuple[str, tuple[str, ...], bool], ...] = (
    ("silent-slow-notes-once", (sys.executable, "-c", "import time; time.sleep(1.2)"), True),
    ("fast-child-no-note", ("/bin/echo", "fast-ok"), False),
)


def test_stall_verdict_triage_table() -> None:
    """Stall triage precedence is cpu-bound, disk-wait, scheduler-contention, then quiet.

    Rows pin one-core rendering, the inclusive contention boundary, and the negative-delta quiet clamp.
    """
    window = engine_mod._STALL_SAMPLE_S

    def sample(cpu_s: float = 0.0, invol: float = 0.0, status: str = psutil.STATUS_RUNNING, procs: int = 1) -> engine_mod.StalledProcess:
        return engine_mod.StalledProcess(cpu_s=cpu_s, invol=invol, status=status, procs=procs)

    idle = sample()
    cases: tuple[ProjectionCase[tuple[engine_mod.StalledProcess, engine_mod.StalledProcess]], ...] = (
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

    def project(intent: tuple[engine_mod.StalledProcess, engine_mod.StalledProcess]) -> str:
        first, second = intent
        return engine_mod._stall_verdict(first, second)

    projection_matrix(cases, project)


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
    monkeypatch.setattr(engine_mod, "psutil", _make_psutil_module({777: root}))
    sample = engine_mod._stall_sample(777)
    expected = engine_mod.StalledProcess(cpu_s=3.75, invol=42.0, status=psutil.STATUS_RUNNING, procs=3)
    assert sample == expected, f"tree aggregate wrong: {sample!r}"
    empty = engine_mod._stall_sample(2_147_483_646)
    assert empty == engine_mod.StalledProcess(cpu_s=0.0, invol=0.0, status="", procs=0), f"vanished pid did not degrade: {empty!r}"


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


# --- [STAGE_MATERIALIZE]


def test_staged_tool_materializes_workdir_and_runs(assay_root: AssayHarness) -> None:
    """A staged UV tool copies contained inputs into an artifact worktree and runs there.

    The success arm covers destructive replacement only after containment has accepted every input.
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
        groups=(ToolGroup.MUTATION,),
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


@pytest.mark.parametrize("claim", [Claim.BRIDGE, Claim.PACKAGE, Claim.PROVISION], ids=["bridge", "package", "provision"])
def test_host_bound_tool_requires_local_execution(assay_root: AssayHarness, claim: Claim) -> None:
    """A host-bound tool against a remote ``exec_target`` faults ``UNSUPPORTED`` before argv composition."""
    remote = assay_root.remote("ssh://x@127.0.0.1:2222")
    tool = Tool("host-bound-remote-law", Runner.DOTNET, ("run", "--", "verify"), Input.NONE, Language.CSHARP, claim, mode=Mode.VERIFY)
    fault = assert_error_status(run_check(Check(tool=tool), settings=remote, scope=None, routed=_PY_CHANGED), RailStatus.UNSUPPORTED)
    assert "host-bound tools require local execution" in fault.message, f"wrong host-bound message: {fault!r}"


# --- [REAP]


def test_reap_terminates_live_tree_and_passes_through_exited(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_reap`` kills live child trees and preserves already-exited return codes."""
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

    Unresolvable pids take the ``psutil.Error`` arm without raising.
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
    """SIGTERM then SIGKILL handles resistant survivors and emits each reaping phase."""
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
    """``_child_pgid`` protects the engine group, vanished pids, and session-leader children."""
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
    """``_diagnose`` folds the real ``_measure`` chain onto the active span event (``mem.rss_bytes``).

    It also seeds ``_RESOURCE`` so cross-``anyio.run`` fault construction carries resource evidence.
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


def test_measure_pressure_arms_degrade_to_empty(monkeypatch: pytest.MonkeyPatch) -> None:
    """The measurement is best-effort evidence, never a second fault source.

    Memory, load, child, file-descriptor, and bytes-projection arms degrade independently: a faulted source elides its keys.
    """
    self_proc = _proc(rss=4096)
    self_proc.children.side_effect = psutil.Error("children down")
    fake = _make_psutil_module({None: self_proc})
    fake.virtual_memory.side_effect = psutil.Error("vm down")
    fake.swap_memory.side_effect = psutil.Error("swap down")
    monkeypatch.setattr(engine_mod, "psutil", fake)
    fd_proc = _proc()
    fd_proc.num_fds.side_effect = NotImplementedError("no num_fds")

    def _fault() -> float:
        raise ValueError("probe down")

    support_matrix(
        ("load-info-mem-fault→load-only", lambda: set(engine_mod._load_info().to_rows()) == {"sys.load1_percent"}, True),
        ("measure-children-fault→elided", lambda: "proc.children" not in dict(engine_mod._measure().to_resources()), True),
        ("num-fds-not-implemented→default", lambda: engine_mod._safe_call(lambda: float(fd_proc.num_fds()), -1.0) == pytest.approx(-1.0), True),
        ("measure-keeps-num-fds-key", lambda: "proc.num_fds" in dict(engine_mod._measure().to_resources()), True),
        ("safe-call-value-arm", lambda: engine_mod._safe_call(lambda: 7.0, -1.0) == pytest.approx(7.0), True),
        ("safe-call-default-arm", lambda: engine_mod._safe_call(_fault, -1.0) == pytest.approx(-1.0), True),
        (
            "as-bytes-projection",
            lambda: (engine_mod._as_bytes(b"x"), engine_mod._as_bytes(None), engine_mod._as_bytes("s")) == (b"x", b"", b"s"),
            True,
        ),
    )


def test_load_info_load_arm_degrades_without_getloadavg(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_load_info`` omits ``sys.load1_percent`` when ``os.getloadavg`` is absent and when it raises ``OSError``."""

    def _boom() -> tuple[float, float, float]:
        raise OSError("load unavailable")

    monkeypatch.setattr(os, "getloadavg", _boom, raising=False)
    assert "sys.load1_percent" not in engine_mod._load_info().to_rows(), "failing getloadavg not degraded to an absent key"
    monkeypatch.delattr(os, "getloadavg", raising=False)
    assert "sys.load1_percent" not in engine_mod._load_info().to_rows(), "absent getloadavg not degraded to an absent key"


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
    owner = engine_mod._LeaseOwner(resource=label, run_id="holder", pid=os.getpid(), create_time=time.time())
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

    monkeypatch.setattr(engine_mod, "exclusive_lease", _boom)

    def _action(_held: object) -> Result[object, Fault]:
        ran[0] = True
        return Ok(1)

    outcome = leased("res", _action, settings=assay_root.settings, run_id="run")
    assert_error_status(outcome, RailStatus.FAULTED)
    assert ran[0] is False, "action ran despite a lease acquisition fault"


def test_steal_rewrites_owner_on_uncontended_lock(assay_root: AssayHarness) -> None:
    """``_steal`` re-locks an uncontended fd and rewrites the owner block."""
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
    """The non-streaming remote arm shell-quotes argv and returns the ssh double reply."""
    remote = assay_root.remote(ssh_env.url)
    check = Check(tool=_ECHO_TOOL, cwd=assay_root.root)
    # Bridge run_check's owned event loop through a thread under anyio tests.
    outcome = await anyio.to_thread.run_sync(  # ty: ignore[unresolved-attribute]
        lambda: run_check(check, settings=remote, scope=None, routed=_ROUTED_CHANGED)
    )
    done = assert_ok(outcome)
    assert (b"remote-ok:" in done.stdout, done.returncode) == (True, 0), f"ssh double reply missing from {done.stdout!r}"


@pytest.mark.anyio
@pytest.mark.parametrize("scoped", [False, True], ids=["non-persisted", "scoped-persists-artifact"])
async def test_run_check_remote_streaming_round_trips(
    scoped: bool,  # noqa: FBT001
    assay_root: AssayHarness,
    ssh_env: SshEnv,
) -> None:
    """The remote streaming arm drains the ssh double reply and tail-caps the receipt.

    The scoped row pins the writer-handle sink and artifact persistence arm.
    """
    remote = assay_root.remote(ssh_env.url)
    scope = assay_root.scope(Claim.STATIC) if scoped else None
    name = "remote-scoped-stream-law" if scoped else "remote-stream-law"
    check = Check(tool=_stream_tool(name, ("/bin/echo", "stream-ok")), cwd=assay_root.root)
    outcome = await anyio.to_thread.run_sync(  # ty: ignore[unresolved-attribute]
        lambda: run_check(check, settings=remote, scope=scope, routed=_ROUTED_CHANGED)
    )
    done = assert_ok(outcome)
    assert (b"/bin/echo" in done.stdout, done.returncode) == (True, 0), f"streamed remote command not in tail: {done.stdout!r}"
    match scope:
        case None:
            pass
        case _:
            artifact = next((a for a in done.artifacts if a.id.startswith(f"{name}-") and a.id.endswith("-out")), None)
            assert artifact is not None, f"scoped remote stream emitted no artifact: {done.artifacts!r}"
            assert b"remote-ok:" in scope.store.read_path(artifact.path), "persisted remote artifact lost the ssh double reply"


@pytest.mark.anyio
async def test_fan_out_remote_pools_ssh_connection(assay_root: AssayHarness, ssh_env: SshEnv) -> None:
    """``fan_out`` over a remote runner pools one ssh connection across workers and ``_pooled_ssh`` closes it on scope exit.

    The two slots pin first-open, cache-reuse, reply round-trip, and close-on-exit behavior.
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


# Scope-tree seed shared by the remote-write seeding and the landed-artifact assertions; coverage.xml lives under sarif/.
_SEED_FILES: tuple[tuple[str, bytes], ...] = (("results.sarif", b'{"runs":[]}\n'), ("coverage.xml", b"<coverage/>\nline2\n"))


async def _git_seed(root: Path, files: Mapping[str, bytes]) -> None:
    # A minimal real git repo makes `git ls-files` the manifest source: the push test pushes exactly the tracked set.
    for rel, payload in files.items():
        (root / rel).parent.mkdir(parents=True, exist_ok=True)
        (root / rel).write_bytes(payload)
    ident = {"GIT_AUTHOR_NAME": "t", "GIT_AUTHOR_EMAIL": "t@t", "GIT_COMMITTER_NAME": "t", "GIT_COMMITTER_EMAIL": "t@t"}
    env = {**os.environ, "GIT_CONFIG_GLOBAL": "/dev/null", "GIT_CONFIG_SYSTEM": "/dev/null", **ident}  # noqa: TID251  # subprocess env clone for the test-local git seed
    for argv in (("git", "init", "-q"), ("git", "add", "-A"), ("git", "-c", "commit.gpgsign=false", "commit", "-q", "-m", "seed", "--no-verify")):
        await anyio.run_process([*argv], cwd=str(root), env=env, check=True)


@pytest.mark.anyio
async def test_remote_transfer_pushes_manifest_then_pulls_scope_tree(  # noqa: PLR0914, PLR0915  # one end-to-end transfer law: push the git manifest, run, pull the scope tree, and assert receipt counts in a single scenario a helper split would fragment
    assay_root: AssayHarness, tmp_path: Path
) -> None:
    """``_remote_transfer`` pushes the git-tracked working tree to ``<workroot>/<run_id>`` then pulls the scope tree back.

    The chrooted loopback SFTP server stands in for the remote host. The push leg lands exactly the ``git ls-files``
    manifest under the remote run dir; the pull leg downloads the tool-written scope tree to the agent-local store at
    the same scope-relative parts, with real byte/line counts, no absolute host path leaking into ``Artifact.path``,
    and the ``ExecReceipt`` carrying the pushed and pulled file counts.
    """
    from tests.python._testkit.env import provision, SshHost  # noqa: PLC0415  # sftp-capable loopback for the real put/get
    from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: PLC0415  # runtime scope construction
    from tools.assay.core.engine import _ExecPlan  # noqa: PLC0415  # plan carries the scope/backend the transfer dispatches on

    # The Offload derives the sftp backend from the host: <workroot>/<run_id>/.artifacts/assay, sited inside the SFTP chroot.
    remote = AssaySettings.model_validate({
        "root": UPath(assay_root.root),
        "exec_target": "ssh://x@127.0.0.1",
        "exec_known_hosts": None,
        "exec_workroot": "/work",
    })
    offload = remote.offload
    assert offload is not None
    backend_root, run_id, claim = offload.backend.root, remote.run_id, Claim.STATIC.value
    manifest = {"Workspace.slnx": b"", "src/a.cs": b"class A;\n", "src/nested/b.txt": b"b\n"}
    await _git_seed(Path(str(remote.local_root)), manifest)
    # The remote cwd is the run dir the push lands under; the chroot resolves /work/<run_id> to tmp_path/work/<run_id>.
    remote_cwd = offload.target.remote_workroot(run_id)

    # The agent landing store is local-file; the scope is rooted there so `_scope_relative` yields the same parts the remote tool used.
    landing = remote.store(protocol="file", root="")
    scope = ArtifactScope(store=landing, path=landing.path(claim, run_id), dotnet_flags=())
    # Seed the remote scope tree under the chroot: this is what the remote tool wrote to its backend.
    remote_scope = tmp_path / backend_root.lstrip("/") / claim / run_id
    (remote_scope / "sarif").mkdir(parents=True)
    for name, payload in _SEED_FILES:
        (remote_scope / ("sarif" if name == "coverage.xml" else ".") / name).write_bytes(payload)

    provisioned = provision(SshHost(sftp_root=tmp_path))
    conn = await provisioned.client_factory()
    plan = _ExecPlan(
        argv=("echo",),
        check=Check(tool=_stream_tool("remote-transfer-law", ("/bin/echo", "x"))),
        cwd=remote_cwd,
        env={},
        settings=remote,
        scope=scope,
        streaming=False,
        tail_cap=4096,
        spill_cap=1 << 20,
        chunk=65536,
        thread_limiter=None,
    )
    try:
        async with engine_mod._remote_transfer(conn, plan) as transfer:
            pass  # exec is a no-op here: the push runs on bracket entry, the pull on transfer.pull
        pulled = await transfer.pull({})
        # The remote run dir holds both the pushed manifest and the pre-seeded scope tree; the push leg owns only the former.
        run_dir = tmp_path / "work" / run_id
        landed_manifest = {
            rel
            for p in run_dir.rglob("*")
            if p.is_file()
            for rel in (str(p.relative_to(run_dir)).replace("\\", "/"),)
            if not rel.startswith(".artifacts/")
        }
    finally:
        conn.close()
        await conn.wait_closed()

    assert transfer.notes == (), f"a clean push must add no degrade note: {transfer.notes!r}"
    assert transfer.pushed == len(manifest), f"push count != manifest size: {transfer.pushed} != {len(manifest)}"
    assert landed_manifest == set(manifest), f"pushed tree diverged from the git manifest: {landed_manifest} != {set(manifest)}"
    assert pulled.notes == (), f"a clean sftp pull must add no degrade note: {pulled.notes!r}"
    assert pulled.count == len(_SEED_FILES), f"pull count != scope tree size: {pulled.count} != {len(_SEED_FILES)}"
    by_name = {a.path.rsplit("/", 1)[-1]: a for a in pulled.artifacts}
    assert {"results.sarif", "coverage.xml"} <= set(by_name), f"sftp pull lost a scope file: {by_name.keys()}"
    for name, expected in _SEED_FILES:
        row = by_name[name]
        assert (row.kind, row.bytes) == (ArtifactKind.SCOPE, len(expected)), f"wrong kind/size for {name}: {row!r}"
        assert f"/{claim}/{run_id}/" in f"/{row.path}/", f"scope-relative path lost for {name}: {row.path!r}"
        # The agent-local landing path is recorded, never the remote backend root that hosted the source tree.
        assert not row.path.startswith(backend_root.lstrip("/")), f"remote backend path leaked into {name}: {row.path!r}"
        assert f"{backend_root}/" not in f"/{row.path}", f"remote backend path leaked into {name}: {row.path!r}"
        assert landing.read_path(row.path) == expected, f"landed bytes diverged for {name}"
    assert by_name["coverage.xml"].lines == 2, "line count not derived from the real landed bytes"


@pytest.mark.anyio
async def test_push_repo_pipelines_nested_tree_preserving_structure(assay_root: AssayHarness, tmp_path: Path) -> None:
    """``_push_repo`` pipelines per-directory puts concurrently yet lands the exact ``git ls-files`` tree, no flattening.

    A deep manifest with many single-file directories (the ``.claude/skills``/``docs`` shape) stresses the concurrent
    push: the same file set must land at ``<abs-workroot>/<run_id>/<relpath>`` with the relative tree intact and no
    literal ``~`` anywhere. The chrooted loopback SFTP server is the remote; success is byte-for-byte path-and-content parity.
    """
    from tests.python._testkit.env import provision, SshHost  # noqa: PLC0415  # real sftp loopback for the concurrent put/makedirs
    from tools.assay.composition.settings import AssaySettings  # noqa: PLC0415  # runtime settings construction
    from tools.assay.core.engine import _ExecPlan  # noqa: PLC0415  # plan carries the resolved absolute workroot the push lands under

    remote = AssaySettings.model_validate({
        "root": UPath(assay_root.root),
        "exec_target": "ssh://x@127.0.0.1",
        "exec_known_hosts": None,
        "exec_workroot": "/work",
    })
    run_id = remote.run_id
    # Many single-file directories plus repeated basenames across dirs: a flat-basename push would collide/flatten these.
    manifest = {
        "Workspace.slnx": b"slnx\n",
        "a.txt": b"root-a\n",
        ".claude/skills/one/SKILL.md": b"one\n",
        ".claude/skills/two/SKILL.md": b"two\n",
        ".claude/skills/three/SKILL.md": b"three\n",
        "docs/x/index.md": b"x\n",
        "docs/y/index.md": b"y\n",
        "docs/y/deep/leaf.md": b"leaf\n",
    }
    await _git_seed(Path(str(remote.local_root)), manifest)
    remote_cwd = remote.exec_target.remote_workroot(run_id) if isinstance(remote.exec_target, engine_mod.Ssh) else ""
    assert "~" not in remote_cwd, f"workroot must be absolute, not a literal tilde: {remote_cwd!r}"

    conn = await provision(SshHost(sftp_root=tmp_path)).client_factory()
    plan = _ExecPlan(
        argv=("echo",),
        check=Check(tool=_stream_tool("push-shape-law", ("/bin/echo", "x"))),
        cwd=remote_cwd,
        env={},
        settings=remote,
        scope=None,
        streaming=False,
        tail_cap=4096,
        spill_cap=1 << 20,
        chunk=65536,
        thread_limiter=None,
    )
    try:
        pushed, notes = await engine_mod._push_repo(conn, plan, tuple(sorted(manifest)))
        run_dir = tmp_path / "work" / run_id
        landed = {
            rel: (run_dir / rel).read_bytes() for p in run_dir.rglob("*") if p.is_file() for rel in (str(p.relative_to(run_dir)).replace("\\", "/"),)
        }
    finally:
        conn.close()
        await conn.wait_closed()

    assert notes == (), f"a clean concurrent push must add no failure note: {notes!r}"
    assert pushed == len(manifest), f"push count != manifest size: {pushed} != {len(manifest)}"
    assert landed == manifest, f"concurrent push flattened or dropped the nested tree: {set(landed)} != {set(manifest)}"


def test_stale_remote_runs_keeps_newest_per_host_namespace() -> None:
    """``_stale_remote_runs`` prunes only this host's surplus run dirs, oldest-first, never another host's namespace.

    The run-id host token partitions a shared workroot: rows for a foreign token are inert regardless of age, and within
    this host the oldest ``len-keep`` dirs by ``(mtime, run_id)`` are returned. ``keep >= own`` selects nothing.
    """
    from tools.assay.composition.settings import run_id_host_token  # noqa: PLC0415  # token owner under test for the namespace filter

    mine, theirs, absent = "aaaaaaaa", "bbbbbbbb", "cccccccc"
    rows = (
        (f"2026-01-01T00-00-00.0-{mine}-100", 100.0),
        (f"2026-01-02T00-00-00.0-{mine}-101", 200.0),
        (f"2026-01-03T00-00-00.0-{mine}-102", 300.0),
        (f"2026-01-01T00-00-00.0-{theirs}-999", 50.0),  # foreign host: never selected
        ("custom-no-token", 10.0),  # tokenless id: filtered out by every host token
    )
    assert run_id_host_token(rows[0][0]) == mine, "host token must round-trip out of the canonical run id"
    keep1 = engine_mod._stale_remote_runs(rows, token=mine, keep=1)
    assert keep1 == (rows[0][0], rows[1][0]), f"keep=1 must drop this host's two oldest, newest-first retained: {keep1!r}"
    assert all(theirs not in run_id and "custom" not in run_id for run_id in keep1), "a foreign or tokenless run leaked into the prune set"
    assert engine_mod._stale_remote_runs(rows, token=mine, keep=3) == (), "keep>=own count prunes nothing"
    assert engine_mod._stale_remote_runs(rows, token=absent, keep=0) == (), "an absent host token owns no runs to prune"


@pytest.mark.anyio
async def test_remote_prune_sweeps_only_this_hosts_stale_run_dirs(assay_root: AssayHarness, tmp_path: Path) -> None:
    """``_remote_prune`` removes this host's orphaned ``<workroot>/<run_id>`` dirs over SFTP, sparing another host's runs.

    Prior offloads orphan a full source copy per run under the shared workroot; the sweep keeps ``artifact_retention``
    newest of this host's own runs and ``rmtree``s the rest, while a foreign-token run dir is untouched on the shared root.
    The chrooted loopback SFTP server is the remote; success is the surviving directory set plus the receipt note.
    """
    from tests.python._testkit.env import provision, SshHost  # noqa: PLC0415  # real sftp loopback for scandir + rmtree
    from tools.assay.composition.settings import AssaySettings  # noqa: PLC0415  # runtime settings carry the host token + retention

    remote = AssaySettings.model_validate({
        "root": UPath(assay_root.root),
        "exec_target": "ssh://x@127.0.0.1",
        "exec_known_hosts": None,
        "exec_workroot": "/work",
        "artifact_retention": 1,
    })
    token = remote.host_run_token
    assert token, "the default run id must embed a host token for the namespace filter"
    workdir = tmp_path / "work"
    # Three of this host's runs (only the newest survives at retention=1), plus one foreign-token run that must persist.
    mine_old = f"2026-01-01T00-00-00.0-{token}-1"
    mine_mid = f"2026-01-02T00-00-00.0-{token}-2"
    mine_new = remote.run_id  # the current run: newest, always retained
    theirs = "2026-06-01T00-00-00.0-deadbeef-9"
    for run_id, payload in ((mine_old, b"old"), (mine_mid, b"mid"), (mine_new, b"new"), (theirs, b"foreign")):
        (workdir / run_id / "src").mkdir(parents=True)
        (workdir / run_id / "src" / "f.cs").write_bytes(payload)

    conn = await provision(SshHost(sftp_root=tmp_path)).client_factory()
    try:
        notes = await engine_mod._remote_prune(conn, remote)
        survivors = {p.name for p in workdir.iterdir() if p.is_dir()}
    finally:
        conn.close()
        await conn.wait_closed()

    assert notes == ("remote.prune.removed runs=2",), f"prune note must report exactly this host's two removed runs: {notes!r}"
    assert survivors == {mine_new, theirs}, f"prune must keep the newest own run and the foreign run, drop the rest: {survivors!r}"


def _manifest_plan(harness: AssayHarness, tool: Tool, paths: tuple[str, ...] = ()) -> engine_mod._ExecPlan:
    # Minimal _ExecPlan carrying just the lane discriminant (runner) and the seed tokens the manifest scoper reads.
    return engine_mod._ExecPlan(
        argv=tool.command,
        check=Check(tool=tool, paths=paths),
        cwd="",
        env={},
        settings=harness.settings,
        scope=None,
        streaming=False,
        tail_cap=4096,
        spill_cap=1 << 20,
        chunk=65536,
        thread_limiter=None,
    )


def test_lane_manifest_csharp_scopes_to_transitive_project_closure(assay_root: AssayHarness) -> None:
    """The C# lane manifest is the transitive ProjectReference closure plus root build config, not the whole git tree.

    A naive subtree scope would drop ``libs/B`` when the build seeds ``libs/A`` that references it across directories;
    the closure walk keeps B and its files while excluding the unrelated ``libs/C`` project tree entirely.
    """
    root = Path(str(assay_root.settings.local_root))
    (root / "libs/A").mkdir(parents=True)
    (root / "libs/B").mkdir(parents=True)
    (root / "libs/C").mkdir(parents=True)
    (root / "libs/A/A.csproj").write_bytes(b'<Project><ItemGroup><ProjectReference Include="../B/B.csproj"/></ItemGroup></Project>')
    (root / "libs/B/B.csproj").write_bytes(b"<Project/>")
    (root / "libs/C/C.csproj").write_bytes(b"<Project/>")
    universe = (
        "Directory.Build.props",
        "Directory.Packages.props",
        "README.md",  # repo-root non-config file: excluded (not on the closure, not a build-config anchor)
        "libs/A/A.csproj",
        "libs/A/Owner.cs",
        "libs/B/B.csproj",
        "libs/B/Dep.cs",
        "libs/C/C.csproj",  # unrelated project: excluded
        "libs/C/Other.cs",
    )
    tool = Tool("cs-build", Runner.DOTNET, ("build", str(root / "libs/A/A.csproj")), Input.OWNED, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD)
    scoped = frozenset(engine_mod._lane_manifest(_manifest_plan(assay_root, tool), universe))
    closure_files = frozenset({"libs/A/Owner.cs", "libs/B/Dep.cs"})
    config_files = frozenset({"Directory.Build.props", "Directory.Packages.props"})
    assert closure_files <= scoped, f"closure dropped a transitive project file: {scoped!r}"
    assert config_files <= scoped, f"root build config must always cross: {scoped!r}"
    assert not any(p.startswith("libs/C/") for p in scoped), f"unrelated project tree must not cross: {scoped!r}"
    assert "README.md" not in scoped, f"a non-config repo-root file is off the C# closure: {scoped!r}"


register_law(engine_mod._lane_manifest, "lane_manifest_csharp_scopes_to_transitive_project_closure")
register_law(engine_mod._project_closure, "lane_manifest_csharp_scopes_to_transitive_project_closure")


def test_lane_manifest_python_scopes_to_package_tests_and_config(assay_root: AssayHarness) -> None:
    """The Python lane manifest is package source + tests + config anchors, never the whole git tree or C# sources."""
    universe = ("pyproject.toml", "uv.lock", "tools/assay/core/engine.py", "tests/python/conftest.py", "libs/csharp/Rasm/Foo.cs", "docs/x.md")
    tool = Tool("py-lint", Runner.UV, ("ruff", "check"), Input.NONE, Language.PYTHON, Claim.STATIC)
    scoped = set(engine_mod._lane_manifest(_manifest_plan(assay_root, tool), universe))
    expected = {"pyproject.toml", "uv.lock", "tools/assay/core/engine.py", "tests/python/conftest.py"}
    assert scoped == expected, f"python lane scope drifted: {scoped!r}"


register_law(engine_mod._lane_manifest, "lane_manifest_python_scopes_to_package_tests_and_config")
register_law(engine_mod._python_manifest, "lane_manifest_python_scopes_to_package_tests_and_config")


def test_lane_manifest_unknown_lane_keeps_full_universe(assay_root: AssayHarness) -> None:
    """A lane with no project graph (DIRECT runner) keeps the full git universe: nothing to scope against."""
    universe = ("a", "b/c", "d/e/f")
    tool = Tool("direct", Runner.DIRECT, ("echo",), Input.NONE, Language.CSHARP, Claim.STATIC)
    assert engine_mod._lane_manifest(_manifest_plan(assay_root, tool), universe) == universe


register_law(engine_mod._lane_manifest, "lane_manifest_unknown_lane_keeps_full_universe")


def test_remote_scope_argv_rebases_host_absolute_scope_paths(assay_root: AssayHarness) -> None:
    """``_remote_scope_argv`` rebases CspSarifDir and --artifacts-path host-absolute paths under the remote workroot.

    A remote Linux build never sees a macOS-absolute scope path (CS0016): the ``prop=<abs>`` value tail and the bare
    ``<abs>`` token both rebind ``<local_root>/X -> <remote_root>/X``, while flags and non-local tokens pass through.
    """
    local_root = str(assay_root.settings.local_root)
    remote_root = "/work/run-1"
    argv = (
        "dotnet",
        "build",
        f"-p:CspSarifDir={local_root}/.artifacts/assay/build/bridge/Release/sarif",
        "--artifacts-path",
        f"{local_root}/.artifacts/assay/build/bridge/Release",
        "/p:Unrelated=value",
        f"{local_root}/libs/A/A.csproj",
    )
    rebased = engine_mod._remote_scope_argv(argv, local_root=local_root, remote_root=remote_root)
    assert rebased[2] == f"-p:CspSarifDir={remote_root}/.artifacts/assay/build/bridge/Release/sarif", f"CspSarifDir not rebased: {rebased[2]!r}"
    assert rebased[4] == f"{remote_root}/.artifacts/assay/build/bridge/Release", f"--artifacts-path value not rebased: {rebased[4]!r}"
    assert rebased[:2] == ("dotnet", "build"), "leading runner/verb tokens must survive the rebase"
    assert rebased[3] == "--artifacts-path", "the --artifacts-path flag token must survive the rebase"
    assert rebased[5] == "/p:Unrelated=value", "a prop token carrying no local-root path must pass through untouched"
    assert rebased[6] == f"{remote_root}/libs/A/A.csproj", "a bare absolute project token under local_root rebases whole"


register_law(engine_mod._remote_scope_argv, "remote_scope_argv_rebases_host_absolute_scope_paths")
register_law(engine_mod._resolve_remote_plan, "remote_scope_argv_rebases_host_absolute_scope_paths")


@contextlib.contextmanager
def _moto_s3(monkeypatch: pytest.MonkeyPatch) -> Iterator[AbstractFileSystem]:
    # A moto-backed S3FileSystem double: ambient AWS env (creds + endpoint) is production parity — the SHARED-pull store
    # reads them off the executor env, never a settings knob. One real loopback object store, zero local fixtures.
    from moto.server import ThreadedMotoServer  # noqa: PLC0415  # loopback object-store double for the real s3fs read

    for key, value in (("AWS_ACCESS_KEY_ID", "test"), ("AWS_SECRET_ACCESS_KEY", "test"), ("AWS_DEFAULT_REGION", "us-east-1")):
        monkeypatch.setenv(key, value)
    server = ThreadedMotoServer(ip_address="127.0.0.1", port=0, verbose=False)
    server.start()
    try:
        host, port = server.get_host_and_port()
        monkeypatch.setenv("AWS_ENDPOINT_URL", f"http://{host}:{port}")
        fs = fsspec.filesystem("s3", skip_instance_cache=True)
        fs.mkdirs("bkt", exist_ok=True)  # the bucket pre-exists in production; create it once so the tool-written seed lands
        yield fs
    finally:
        server.stop()


@pytest.mark.anyio
async def test_remote_transfer_reads_shared_cloud_scope_without_byte_transfer(  # noqa: PLR0914, PLR0915  # one SHARED-pull law: seed a remote-written s3 tree, read it scope-relative with zero transfer, and degrade a missing tree to a note
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, socket_enabled: None
) -> None:
    """A SHARED cloud offload reads the tool-written scope tree straight from the object store with zero byte transfer.

    The remote tool writes scope artifacts to the shared s3 store; the agent opens the SAME universal paths directly,
    folding ``Artifact`` rows scope-relative with byte counts from backend metadata — no SFTP, no payload crossing the
    wire (``cat_file`` is never reached on this arm). A prefix with no keys is the absent-tree signal and degrades to a note.
    """
    from tests.python._testkit.env import (  # noqa: PLC0415  # ssh double satisfies the _Transfer conn type; the SHARED arm never touches it
        provision,
        SshHost,
    )
    from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: PLC0415  # runtime scope/settings construction
    from tools.assay.core.engine import _ExecPlan, _Transfer  # noqa: PLC0415  # the transfer dispatches pull on the offload strategy

    _ = socket_enabled  # lifts the INET socket ban for the moto loopback server; the hook auto-applies the network marker
    with _moto_s3(monkeypatch) as fs:
        # The SHARED offload pins the s3 store at <root>/<run_id>/.artifacts/assay; this is the universal path both sides address.
        remote = AssaySettings.model_validate({
            "root": UPath(assay_root.root),
            "exec_target": "ssh://x@127.0.0.1",
            "exec_known_hosts": None,
            "artifact_backend": {"protocol": "s3", "root": "bkt/runs"},
        })
        offload = remote.offload
        assert offload is not None
        assert offload.pull_strategy is PullStrategy.SHARED
        backend_root, run_id, claim = offload.backend.root, remote.run_id, Claim.STATIC.value
        # The scope's store is the same SHARED backend: _scope_relative yields <claim>/<run_id>, the parts both sides agree on.
        shared = remote.store(protocol="s3", root=backend_root, skip_instance_cache=True)
        scope = ArtifactScope(store=shared, path=shared.path(claim, run_id), dotnet_flags=())
        plan = _ExecPlan(
            argv=("echo",),
            check=Check(tool=_stream_tool("shared-pull-law", ("/bin/echo", "x"))),
            cwd=offload.target.remote_workroot(run_id),
            env={},
            settings=remote,
            scope=scope,
            streaming=False,
            tail_cap=4096,
            spill_cap=1 << 20,
            chunk=65536,
            thread_limiter=None,
        )
        conn = await provision(SshHost()).client_factory()
        transfer = _Transfer(conn=conn, plan=plan, pushed=0, notes=())
        try:
            # Absent tree first: no keys under the scope prefix degrade to a note, no artifacts, parity with the sftp degrade path.
            missing = await transfer.pull({})
            assert (missing.count, missing.artifacts) == (0, ()), f"absent shared tree must fold no artifacts: {missing!r}"
            assert missing.notes == ("remote.artifacts.degraded missing_tree",), f"absent shared tree must degrade to a note: {missing.notes!r}"

            # The remote tool writes the scope tree to the shared store: coverage.xml nests under sarif/ to exercise the recursive walk.
            scope_prefix = f"{backend_root}/{claim}/{run_id}"
            fs.pipe_file(f"{scope_prefix}/results.sarif", b'{"runs":[]}\n')
            fs.pipe_file(f"{scope_prefix}/sarif/coverage.xml", b"<coverage/>\nline2\n")

            pulled = await transfer.pull({})
        finally:
            conn.close()
            await conn.wait_closed()

    assert pulled.notes == (), f"a present shared tree must add no degrade note: {pulled.notes!r}"
    assert pulled.count == 2, f"pull count != shared scope tree size: {pulled.count}"
    by_name = {a.path.rsplit("/", 1)[-1]: a for a in pulled.artifacts}
    assert {"results.sarif", "coverage.xml"} == set(by_name), f"shared read lost a scope file: {by_name.keys()}"
    for name, expected in (("results.sarif", 12), ("coverage.xml", 18)):
        row = by_name[name]
        assert (row.kind, row.bytes) == (ArtifactKind.SCOPE, expected), f"wrong kind/size for {name}: {row!r}"
        # The shared scope-relative path carries <claim>/<run_id> and is a bucket-rooted key, never an absolute s3:// URL.
        assert f"/{claim}/{run_id}/" in f"/{row.path}/", f"scope-relative path lost for {name}: {row.path!r}"
        assert not row.path.startswith("s3://"), f"absolute cloud URL leaked into {name}: {row.path!r}"
        assert row.path.startswith(f"{backend_root}/"), f"shared path not rooted at the backend store: {row.path!r}"
    assert by_name["coverage.xml"].path.endswith("/sarif/coverage.xml"), "recursive walk dropped the nested scope file"


register_law(engine_mod._Transfer, "remote_transfer_reads_shared_cloud_scope_without_byte_transfer")
register_law(engine_mod._shared_read_scope, "remote_transfer_reads_shared_cloud_scope_without_byte_transfer")


@pytest.mark.anyio
async def test_remote_transfer_keeps_exec_cancellable_under_deadline(  # noqa: PLR0915  # deadline + shielded push-leg + cancellation integration scenario
    assay_root: AssayHarness, tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A wedged remote exec inside the transfer bracket is reclaimed by the check deadline; the push leg still completes shielded.

    The push shield must scope to the push leg only — never the bracketed exec — or a hung remote tool runs unbounded and
    defeats the deadline. This drives the production topology (``_run_remote`` builds the bracket around ``_remote_exec``)
    with a stalled exec, asserting an outer ``fail_after`` shorter than the stall cancels the run while the up-front push lands.
    """
    from tests.python._testkit.env import provision, SshHost  # noqa: PLC0415  # chrooted loopback so the real push leg lands the manifest
    from tools.assay.composition.settings import AssaySettings  # noqa: PLC0415  # runtime remote-settings construction
    from tools.assay.core.engine import _ExecPlan  # noqa: PLC0415  # plan carries the cwd/settings the bracket pushes under

    remote = AssaySettings.model_validate({
        "root": UPath(assay_root.root),
        "exec_target": "ssh://x@127.0.0.1",
        "exec_known_hosts": None,
        "exec_workroot": "/work",
    })
    offload = remote.offload
    assert offload is not None
    target = offload.target
    run_id, remote_cwd = remote.run_id, target.remote_workroot(remote.run_id)
    await _git_seed(Path(str(remote.local_root)), {"Workspace.slnx": b"", "src/a.cs": b"class A;\n"})

    conn = await provision(SshHost(sftp_root=tmp_path)).client_factory()

    @contextlib.asynccontextmanager
    async def _fixed_conn(_target: object) -> AsyncIterator[object]:
        yield conn  # inject the chrooted double so the bracket pushes/pulls without a real SSH host

    async def _wedged_exec(*_args: object, **_kw: object) -> object:
        await anyio.sleep(5.0)  # a hung remote tool: longer than the deadline, must be cancelled not awaited
        raise AssertionError("wedged remote exec was awaited to completion — the deadline failed to cancel it")

    monkeypatch.setattr(engine_mod, "_ssh_connection", _fixed_conn)
    monkeypatch.setattr(engine_mod, "_remote_exec", _wedged_exec)
    monkeypatch.setattr(engine_mod, "_probe_toolchain", lambda *_a, **_k: _async_none())

    plan = _ExecPlan(
        argv=("dotnet", "test"),
        check=Check(tool=_stream_tool("remote-deadline-law", ("/bin/echo", "x"))),
        cwd=remote_cwd,
        env={},
        settings=remote,
        scope=None,
        streaming=False,
        tail_cap=4096,
        spill_cap=1 << 20,
        chunk=65536,
        thread_limiter=None,
    )
    started = time.monotonic()
    try:
        with pytest.raises(TimeoutError):
            with anyio.fail_after(0.4):
                await engine_mod._run_remote(plan, target)
    finally:
        conn.close()
        await conn.wait_closed()
    elapsed = time.monotonic() - started
    assert elapsed < 2.0, f"deadline did not cancel the wedged remote exec: elapsed={elapsed:.2f}s (shield leaked over the bracketed exec)"
    landed = {str(p.relative_to(tmp_path / "work" / run_id)).replace("\\", "/") for p in (tmp_path / "work" / run_id).rglob("*") if p.is_file()}
    assert {"Workspace.slnx", "src/a.cs"} <= landed, f"push leg did not complete before the exec stalled: {landed!r}"


async def _async_none() -> None:  # noqa: RUF029  # no-op coroutine: the _probe_toolchain mock must return an awaitable
    return None


@pytest.mark.anyio
async def test_probe_toolchain_faults_unsupported_on_missing_remote_tool() -> None:
    """``_probe_toolchain`` returns ``(tool, detail)`` when ``command -v`` exits non-zero, else ``None`` for a present tool."""
    from tests.python._testkit.env import provision, SshHost  # noqa: PLC0415  # loopback exec host with a status-driven handler

    def _handler(command: str) -> tuple[str, int]:
        # The probe runs `command -v <tool>`; the double reports the missing tool as exit 1, a present tool as exit 0.
        return ("", 1) if "missing-tool" in command else ("/usr/bin/git\n", 0)

    conn = await provision(SshHost(handler=_handler)).client_factory()
    try:
        absent = await engine_mod._probe_toolchain(conn, ("missing-tool", "build"))
        present = await engine_mod._probe_toolchain(conn, ("git", "ls-files"))
        absolute = await engine_mod._probe_toolchain(conn, ("/bin/echo", "x"))
    finally:
        conn.close()
        await conn.wait_closed()
    assert absent is not None, "a missing remote tool must surface a probe result"
    assert absent[0] == "missing-tool", f"missing remote tool must surface its name: {absent!r}"
    assert present is None, f"a present remote tool must probe clean: {present!r}"
    assert absolute is None, "an absolute-path command is self-locating and must skip the probe"


def test_fold_receipt_projects_exec_facts_onto_completed() -> None:
    """``_fold_receipt`` stamps the remote target URL/host, exit status, signal, and push/pull counts onto ``Completed.exec``."""
    from tools.assay.composition.settings import Ssh  # noqa: PLC0415  # the value object owns the url/host projection

    target = Ssh(host="vps", port=22, user="root")
    done = engine_mod._fold_receipt(receipt(("dotnet", "test"), 0), target, exit_status=0, signal="", notes=("n",), pushed=3, pulled=2)
    assert done.exec is not None
    assert (done.exec.target, done.exec.host, done.exec.exit_status) == ("ssh://root@vps:22", "vps", 0)
    assert (done.exec.pushed, done.exec.pulled, done.exec.notes) == (3, 2, ("n",))


@pytest.mark.anyio
@pytest.mark.parametrize("exc_factory", ["asyncssh", "oserror"])
async def test_pooled_ssh_logs_close_failures(exc_factory: str) -> None:
    """``_pooled_ssh`` logs close failures from ``asyncssh.Error`` and ``OSError``.

    A broken close must not abort cleanup of sibling connections.
    """
    import asyncssh  # noqa: PLC0415  # deferred: double raises asyncssh.Error directly

    boom: BaseException = asyncssh.Error(code=1, reason="close failed") if exc_factory == "asyncssh" else OSError("socket reset on close")
    closed = [False]

    def _mark_closed() -> None:
        closed[0] = True

    async def _wait_closed() -> None:  # noqa: RUF029  # asyncssh-compatible wait_closed double
        raise boom

    conn = SimpleNamespace(close=_mark_closed, wait_closed=_wait_closed)
    # Local-target settings: the teardown's once-per-fan prune is Ssh-gated, so it skips the structural conn double here.
    settings = AssaySettings(exec_known_hosts=None)
    async with engine_mod._pooled_ssh(settings):
        cache = engine_mod._SSH_CACHE.get()
        assert cache is not None, "_pooled_ssh did not seed the connection cache"
        cache.conns["ssh://x@host:22"] = conn  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # structural conn double
    assert closed[0] is True, "pooled connection close was not attempted before wait_closed faulted"


# --- [ENGINE_INTERNALS]


def test_drain_none_stream_is_empty_tail() -> None:
    """``_recv_anyio(None, ...)`` (the inherited-fd / absent-pipe arm) drains to the empty capture immediately."""
    assert anyio.run(lambda: drain_stream(engine_mod._recv_anyio(None, 32), tail_cap=128, spill_cap=1 << 20)) == Captured(), (
        "None stream did not drain to empty"
    )


def test_total_backfills_none_slot_as_timeout() -> None:
    """``_total`` backfills absent fan-out slots as TIMEOUT and passes present results through."""
    backfilled = engine_mod._total(None)
    present = Ok(receipt(("echo",), 0))
    assert_error_status(backfilled, RailStatus.TIMEOUT)
    assert engine_mod._total(present) is present, "present slot not passed through by identity"


def test_measure_children_sum_resident_set_sizes(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_measure`` counts the recursive child set and folds each child's RSS through the ``_safe_call`` guard."""
    parent, kid_a, kid_b = _proc(pid=os.getpid()), _proc(rss=8192), _proc(rss=4096)
    parent.children.return_value = (kid_a, kid_b)
    monkeypatch.setattr(engine_mod, "psutil", _make_psutil_module({None: parent}))
    rows = dict(engine_mod._measure().to_resources())
    assert (rows["proc.children"], rows["proc.children_rss_bytes"]) == (2.0, 12288.0)


def test_copy_stage_input_reports_missing_input(tmp_path: Path) -> None:
    """``_copy_stage_input`` distinguishes contained-missing inputs from containment escapes."""
    root, work = tmp_path / "root", tmp_path / "work"
    root.mkdir()
    work.mkdir()
    fault = engine_mod._copy_stage_input(Check(tool=_ECHO_TOOL), root, work, "absent-input.txt")
    assert fault is not None, "missing stage input did not fault"
    assert "missing stage input" in fault.message, f"wrong missing-input message: {fault!r}"


def test_stream_writer_rejects_non_context_backend_handle(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_stream_writer`` enforces the ``_WriteContext`` backend handle contract.

    Bad backend handles must fail loudly instead of silently dropping streamed bytes.
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
        spill_cap=assay_root.settings.capture_spill_bytes,
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


def test_measure_and_load_info_pin_exact_metric_projection(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_measure``/``_load_info`` project the exact key→value evidence map from a deterministic psutil double.

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
    monkeypatch.setattr(engine_mod, "psutil", fake)
    monkeypatch.setattr(os, "getloadavg", lambda: (2.0, 9.0, 9.0), raising=False)
    load = {"sys.mem_available_bytes": 4096.0, "sys.mem_percent": 37.5, "sys.swap_percent": 12.5, "sys.load1_percent": 50.0}
    assert engine_mod._load_info().to_rows() == load, "load projection drifted from the doubled sources"
    assert dict(engine_mod._measure().to_resources()) == {
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


def test_guarded_projects_argv_scope_and_governed_limiter_into_execute(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Spawn execution receives projected argv, intact scope, and governed limiter cap.

    The captured triple pins scope threading, argv projection, and governed limiter propagation.
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

    First attempts must not gain retry stamps, and spawn/deadline faults must retain message plus argv evidence.
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

    The child-reported cwd and exit code pin cwd forwarding through both local spawn arms.
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

    The recorder pins remote dispatch and that the declared row env reaches ``_run_remote``; the allowlist projection and
    PATH injection now run inside ``_run_remote`` once the connection resolves the remote home (owned by ``remote_env``).
    """
    recorded: list[tuple[str, dict[str, str]]] = []

    async def _record(plan: object, target: object) -> object:  # noqa: RUF029  # async to match _run_remote's awaited signature
        recorded.append((target.url, dict(getattr(plan, "env", {}))))  # ty: ignore[unresolved-attribute]  # target is the Ssh value object
        return receipt(("remote",), 0, stdout=b"recorded")

    monkeypatch.setattr(engine_mod, "_run_remote", _record)
    env_tool = msgspec.structs.replace(_ECHO_TOOL, name="route-law", env=(("ASSAY_ROW_DECLARED", "row-value"),))
    monkeypatch.setenv("ASSAY_AMBIENT_UNDECLARED", "ambient-value")

    local = assert_ok(run_check(Check(tool=env_tool, cwd=assay_root.root), settings=assay_root.settings, scope=None, routed=_ROUTED_CHANGED))
    assert (recorded, local.stdout) == ([], b"hello\n"), f"local target must never reach _run_remote: {recorded!r}"

    remote = assay_root.remote("ssh://x@127.0.0.1:2222")
    done = assert_ok(run_check(Check(tool=env_tool, cwd=assay_root.root), settings=remote, scope=None, routed=_ROUTED_CHANGED))
    assert (done.stdout, len(recorded)) == (b"recorded", 1), f"remote target did not route to _run_remote: {recorded!r}"
    target, plan_env = recorded[0]
    assert target == "ssh://x@127.0.0.1:2222", f"_run_remote received the wrong target: {target!r}"
    assert plan_env.get("ASSAY_ROW_DECLARED") == "row-value", f"declared row env did not reach _run_remote: {plan_env!r}"


def test_discover_runs_at_root_and_pins_fault_evidence(tmp_path: Path) -> None:
    """``discover`` spawns at ``root`` and every fault carries the exact argv plus a pinned message.

    Timeout, nonzero, and spawn-fault rows pin cwd, argv, message, and stdout fallback evidence.
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
    """UV argv shaping is exact and QUERY mode never splices scope flags.

    Rows pin UV-only project/group segments, real project-root rendering, and QUERY splice bypass.
    """
    settings = assay_root.settings
    uv_tool = Tool(
        "uv-argv-law", Runner.UV, ("ruff", "check"), Input.NONE, Language.PYTHON, Claim.TEST, groups=(ToolGroup.MUTATION,), stage=Stage(project=True)
    )
    uv_argv = assert_ok(argv_for(Check(tool=uv_tool), _PY_CHANGED, settings=settings, scope=None))
    assert uv_argv == ("uv", "run", "--locked", "--project", str(settings.root), "--group", "mutation", "ruff", "check"), (
        f"uv argv drifted: {uv_argv!r}"
    )
    module_tool = Tool("module-argv-law", Runner.MODULE, ("tools.py_analyzer", "check"), Input.NONE, Language.PYTHON, Claim.STATIC)
    module_argv = assert_ok(argv_for(Check(tool=module_tool), _PY_CHANGED, settings=settings, scope=None))
    assert module_argv == ("uv", "run", "--locked", "python", "-m", "tools.py_analyzer", "check"), f"module argv drifted: {module_argv!r}"
    direct = msgspec.structs.replace(uv_tool, runner=Runner.DIRECT)
    direct_argv = assert_ok(argv_for(Check(tool=direct), _PY_CHANGED, settings=settings, scope=None))
    assert direct_argv == ("ruff", "check"), f"non-UV runner leaked uv segments: {direct_argv!r}"
    scope = assay_root.scope(Claim.STATIC)
    query = Tool("query-argv-law", Runner.DOTNET, ("build", "Workspace.slnx"), Input.NONE, Language.CSHARP, Claim.STATIC, mode=Mode.QUERY)
    query_argv = assert_ok(argv_for(Check(tool=query), _ROUTED_CHANGED, settings=settings, scope=scope))
    assert query_argv == ("dotnet", "build", "Workspace.slnx"), f"QUERY mode must never splice scope flags: {query_argv!r}"


def test_splice_command_cuts_before_first_separator(assay_root: AssayHarness) -> None:
    """Scope flags splice immediately before the FIRST ``--`` separator; everything after rides verbatim.

    Single and double separator rows pin the first-cut rule and tail passthrough.
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

    The rows pin separator normalization, escape rejection, safe resolution, and stage fault evidence.
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

    The child-reported cwd pins prepared-check rebinding after materialization.
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

    Rows pin newline-terminated and empty-chunk trailing streams that property seeds may miss.
    """
    rows: tuple[tuple[tuple[bytes, ...], int], ...] = (((b"ab\n",), 1), ((b"ab",), 1), ((b"ab\n", b""), 1), ((b"a\nb",), 2))
    for chunks, expected_lines in rows:
        captured = anyio.run(functools.partial(drain_stream, _recv_of(chunks), tail_cap=16, spill_cap=1 << 20, kind="out"))
        assert captured.lines == expected_lines, f"{chunks!r} drained to {captured.lines} lines, expected {expected_lines}"


# --- [MUTATION_LANE]


def test_mutation_lane_is_populated(request: pytest.FixtureRequest) -> None:
    """The ``mutation`` marker lane carries the engine's destructive-boundary laws so mutmut has a target set."""
    items = getattr(request.session, "items", ())
    marked = tuple(item for item in items if item.get_closest_marker("mutation") is not None)
    assert len(marked) >= 2, f"mutation lane underpopulated: {len(marked)} marked"
