"""Assay execution engine: argv composition, local runs, retry, and the Executor port.

Catalog templates plus typed splice values compose every argv; local process and INPROC backends run
under govern's stream and slot substrate; the Ssh case dispatches through the remote transport.
"""

from collections.abc import Callable
from contextvars import ContextVar
from dataclasses import dataclass
from functools import cache
import os
from pathlib import Path, PurePosixPath
import shutil
import time
from typing import Protocol, runtime_checkable, TYPE_CHECKING

import anyio
from anyio import to_thread
from expression import Error, Ok, Result
import msgspec
from opentelemetry import propagate, trace
import stamina
import structlog
from upath import UPath
lazy import asyncssh

from assay.composition.catalog import launch
from assay.composition.settings import AssaySettings, Local, Ssh
from assay.composition.store import ArtifactScope
from assay.core.aspect import checked, compose, traced
from assay.core.govern import (
    Captured,
    captured_outputs,
    diagnose,
    dotnet_slot,
    drain_pair,
    ExecPlan,
    fan_schedule,
    governed_concurrency,
    max_resources,
    measure,
    reap,
    recv_anyio,
    remaining,
    reset_foreign_census,
    resource_monitor,
    stall_monitor,
    stream_artifacts,
    touched,
)
from assay.core.model import Check, Completed, Fault, HOST_BOUND_CLAIMS, Mode, RailStatus, receipt, Runner, Tool, ToolGroup
from assay.core.remote import pooled_ssh, run_remote
from assay.core.routing import discover, place, Routed
from assay.diagnostics import AST_MATCHES


if TYPE_CHECKING:
    from collections.abc import Coroutine, Mapping, MutableMapping


# --- [SERVICES] -------------------------------------------------------------------------

_LOG = structlog.get_logger("assay.exec")
_TRACER = trace.get_tracer("assay.exec")

# --- [OPERATIONS] -----------------------------------------------------------------------

type _Woven = Callable[[Check, AssaySettings, ArtifactScope | None, Routed, float | None], Coroutine[None, None, Result[Completed, Fault]]]


@runtime_checkable
class Executor(Protocol):
    """Execution port rails spawn checks through; the registry weave threads the bound instance into every handler."""

    def run(
        self, check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> Result[Completed, Fault]:
        """Run one check to a completed receipt or an operational fault."""
        ...

    def fan(
        self, checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> tuple[Result[Completed, Fault], ...]:
        """Run checks concurrently, preserving input order, one result slot per check."""
        ...


_DOTNET_PROBE_TIMEOUT_S: float = 15.0
_DOTNET_SHARED_RUNTIME: tuple[str, str] = ("shared", "Microsoft.NETCore.App")
_RETRY_MIN_REMAINING: float = 0.05
_FAN_LIMITER: ContextVar[anyio.CapacityLimiter | None] = ContextVar("assay_fan_limiter", default=None)


@dataclass(frozen=True, slots=True)
class _PreparedExec:
    check: Check
    argv: tuple[str, ...]
    cwd: str
    env: Mapping[str, str]
    bound: float | None
    thread_limiter: anyio.CapacityLimiter


def splice_command(
    runner: Runner, command: tuple[str, ...], scope: ArtifactScope | None, scoped_verbs: frozenset[str], mode: Mode
) -> tuple[str, ...]:
    """Inject scope flags into eligible DOTNET build-graph commands.

    Returns:
        Command with scope flags inserted before ``--``, or the original command when ineligible.
    """
    match (runner, command, scope):
        case (Runner.DOTNET, (verb, *_), ArtifactScope() as s) if verb in scoped_verbs and mode not in {Mode.QUERY, Mode.LIST}:
            cut = command.index("--") if "--" in command else len(command)
            return (*command[:cut], *_dotnet_scope_flags(command=command, scope=s), *command[cut:])
        case _:
            return command


def _dotnet_scope_flags(command: tuple[str, ...], scope: ArtifactScope) -> tuple[str, ...]:
    match _project_scope(command):
        case "":
            return scope.dotnet_flags
        case segment:
            return tuple(
                f"{scope.path.rstrip('/')}/dotnet/{segment}" if prior == "--artifacts-path" else current
                for prior, current in zip(("", *scope.dotnet_flags[:-1]), scope.dotnet_flags, strict=True)
            )


def _project_scope(command: tuple[str, ...]) -> str:
    try:
        project = command[command.index("--project") + 1]
    except ValueError, IndexError:
        return ""
    stem = PurePosixPath(project.replace("\\", "/")).with_suffix("").as_posix().replace("/", "__")
    return "".join(ch if ch.isalnum() or ch in "-._" else "_" for ch in stem).strip("._")


def _overlay(tool: Tool, settings: AssaySettings, scope: ArtifactScope | None) -> Mapping[str, str]:
    base: MutableMapping[str, str] = dict(os.environ)  # ruff:ignore[banned-api]
    base.update(settings.python_tool_env)
    base.update(tool.env)
    match scope:
        case ArtifactScope() as s:
            return {**base, **s.dotnet_env}
        case None:
            return base


def _argv(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> Result[tuple[str, ...], Fault]:
    tool = check.tool
    try:
        body = check.args.fill(tool.command)
    except ValueError as exc:
        return Error(Fault((tool.name,), message=str(exc)))
    body = ("--project", str(settings.root), *body) if tool.runner is Runner.UV and tool.stage.project else body
    prefix = launch(tool)

    def scoped(pinned: tuple[str, ...]) -> tuple[str, ...]:
        return splice_command(tool.runner, (*body, *pinned), scope, settings.scoped_verbs, tool.mode)

    match check.tail:
        case tuple() as pinned:
            return Ok((*prefix, *scoped(pinned)))
        case None:
            tails = place(routed, tool, settings=settings)
            match tails:
                case () | (_,):
                    return Ok((*prefix, *scoped(tuple(part for tail in tails for part in tail))))
                case _:
                    return Error(Fault((tool.name,), message=f"incoherent closure: {len(tails)} tails for one check"))


def argv_for(check: Check, routed: Routed, *, settings: AssaySettings, scope: ArtifactScope | None) -> Result[tuple[str, ...], Fault]:
    """Project the exact argv the engine would execute for a check.

    Returns:
        Command argv, or a fault when an unpinned check resolves to multiple tails.
    """
    return _argv(check, routed, settings=settings, scope=scope)


@cache
def _dotnet_root() -> str | None:
    def valid(path: str) -> str | None:
        root = Path(path).expanduser()
        return str(root) if path and Path(root, *_DOTNET_SHARED_RUNTIME).is_dir() else None

    def runtime_root(line: str) -> str | None:
        match line.rsplit("[", maxsplit=1):
            case [_, raw] if raw.endswith("]"):
                return valid(str(Path(raw[:-1]).parent.parent))
            case _:
                return None

    def from_env() -> str | None:
        return valid(os.environ.get("DOTNET_ROOT", ""))  # ruff:ignore[banned-api]

    def from_runtimes() -> str | None:
        listed = discover(("dotnet", "--list-runtimes"), root=Path.cwd(), timeout=_DOTNET_PROBE_TIMEOUT_S)
        lines = listed.map(lambda out: out.decode(errors="replace").splitlines()).default_value([])
        return next((root for line in lines for root in (runtime_root(line),) if root is not None), None)

    def from_muxer() -> str | None:
        muxer = shutil.which("dotnet")
        return valid(str(Path(muxer).resolve().parent)) if muxer else None

    return next((root for probe in (from_env, from_runtimes, from_muxer) for root in (probe(),) if root is not None), None)


def _apphost(tool: Tool, env: Mapping[str, str]) -> Mapping[str, str]:
    match (tool.runner, tool.command[:2]):
        case (Runner.DOTNET, ("tool", "run")):
            match _dotnet_root():
                case str(root):
                    return {**env, "DOTNET_ROOT": root, "DOTNET_MULTILEVEL_LOOKUP": "0"}
                case None:
                    return {key: value for key, value in env.items() if key != "DOTNET_ROOT"}
        case _:
            return env


def _materialize(check: Check, settings: AssaySettings) -> Result[Check, Fault]:
    stage = check.tool.stage
    if not stage.root:
        return Ok(check)
    if settings.exec_target:
        return Error(Fault((check.tool.name, "stage"), status=RailStatus.UNSUPPORTED, message="staged tools require local execution"))
    root = Path(str(settings.root)).resolve()
    work = _contained(root, stage.root)
    if isinstance(work, ValueError):
        return Error(Fault((check.tool.name, "stage"), message=str(work)))
    shutil.rmtree(work, ignore_errors=True)
    work.mkdir(parents=True, exist_ok=True)
    for rel in stage.inputs:
        if (fault := _copy_stage_input(check, root, work, rel)) is not None:
            return Error(fault)
    launch_dir = _contained(work, stage.chdir) if stage.chdir else work
    if isinstance(launch_dir, ValueError):
        return Error(Fault((check.tool.name, "stage", stage.chdir), message=str(launch_dir)))
    return Ok(msgspec.structs.replace(check, cwd=launch_dir))


def _copy_stage_input(check: Check, root: Path, work: Path, rel: str) -> Fault | None:
    src = _contained(root, rel)
    if isinstance(src, ValueError):
        return Fault((check.tool.name, "stage", rel), message=str(src))
    dst = _contained(work, rel)
    if isinstance(dst, ValueError):  # pragma: no cover
        return Fault((check.tool.name, "stage", rel), message=str(dst))
    if not src.exists():
        return Fault((check.tool.name, "stage", rel), message=f"missing stage input: {rel}")
    if src.is_dir():
        shutil.copytree(src, dst)
    else:
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dst)
    return None


def _contained(root: Path, rel: str) -> Path | ValueError:
    text = rel.replace("\\", "/")
    parts = text.split("/")
    match text.startswith("/") or "\x00" in text or any(p in {"", ".", ".."} for p in parts):
        case True:
            return ValueError(f"unsafe stage path: {rel!r}")
        case False:
            target = (root / text).resolve()
            return target if target.is_relative_to(root) else ValueError(f"stage path escaped root: {rel!r}")


async def _run_process_backend(plan: ExecPlan) -> Completed:
    started = time.monotonic()
    _LOG.info(
        "process.start", tool=plan.check.tool.name, argv=plan.argv, cwd=plan.cwd, streaming=plan.streaming, remote=bool(plan.settings.exec_target)
    )
    match plan.settings.exec_target:
        case Local():
            match plan.streaming:
                case True:
                    proc = await anyio.open_process(list(plan.argv), cwd=plan.cwd, env=plan.env, start_new_session=True)
                    try:
                        last_output, stall, samples = [time.monotonic()], list[str](), list[tuple[tuple[str, float], ...]]()
                        async with anyio.create_task_group() as tg:
                            _ = tg.start_soon(stall_monitor, proc.pid, last_output, stall)
                            _ = tg.start_soon(resource_monitor, proc.pid, last_output, samples, plan.check.tool.name)
                            streams = await drain_pair(
                                plan,
                                touched(recv_anyio(proc.stdout, plan.chunk), last_output),
                                touched(recv_anyio(proc.stderr, plan.chunk), last_output),
                                proc.wait,
                                stall,
                            )
                            tg.cancel_scope.cancel()
                        resources = max_resources(tuple(samples))
                        duration_ms = (time.monotonic() - started) * 1000.0
                        _LOG.info(
                            "process.end",
                            tool=plan.check.tool.name,
                            argv=plan.argv,
                            returncode=proc.returncode or 0,
                            duration_ms=round(duration_ms, 1),
                            **dict(resources),
                        )
                        return msgspec.structs.replace(
                            receipt(
                                plan.argv,
                                proc.returncode or 0,
                                stdout=streams.get("out", Captured()).read(plan.local_store()),
                                stderr=streams.get("err", Captured()).preview,
                                notes=tuple(stall),
                                artifacts=stream_artifacts(plan.scope, plan.settings, plan.check, streams),
                            ),
                            resources=(*resources, ("process.duration_ms", duration_ms)),
                        )
                    finally:
                        await reap(proc, plan.thread_limiter)
                case False:
                    done = await anyio.run_process(list(plan.argv), cwd=plan.cwd, env=plan.env, check=False, start_new_session=True)
                    streams = captured_outputs(plan, done.stdout, done.stderr)
                    resources = tuple(sorted(measure().to_resources()))
                    duration_ms = (time.monotonic() - started) * 1000.0
                    _LOG.info(
                        "process.end",
                        tool=plan.check.tool.name,
                        argv=plan.argv,
                        returncode=done.returncode,
                        duration_ms=round(duration_ms, 1),
                        **dict(resources),
                    )
                    return msgspec.structs.replace(
                        receipt(
                            plan.argv,
                            done.returncode,
                            stdout=streams.get("out", Captured()).read(plan.local_store()),
                            stderr=streams.get("err", Captured()).preview,
                            artifacts=stream_artifacts(plan.scope, plan.settings, plan.check, streams),
                        ),
                        resources=(*resources, ("process.duration_ms", duration_ms)),
                    )
        case Ssh() as target:
            remote_done = await run_remote(plan, target)
            _LOG.info(
                "process.end",
                tool=plan.check.tool.name,
                argv=plan.argv,
                returncode=remote_done.returncode,
                duration_ms=round((time.monotonic() - started) * 1000.0, 1),
                remote=True,
            )
            return remote_done


def apply_row_status(tool: Tool, done: Completed) -> Completed:
    """Apply a tool row's status policy to a process receipt.

    An ``empty-on-exit1`` row whose returncode-1 stdout decodes as the no-match document maps to ``EMPTY``
    (the tool signals "no match" through exit 1); non-document stdout on exit 1 stays a tool fault (FAILED).
    A row carrying an ``empty_signature`` maps its (returncode, marker) nothing-to-do receipt to ``EMPTY`` —
    a runner with no eligible work (pytest exit 5, vitest "No test files found") is an empty scope, never a defect.

    A row declaring ``defect_exit`` types its exit algebra: that exact code is the tool's rule-violation exit and reads
    ``FAILED`` with its parsed rows, every other non-zero exit is a tool failure and reads ``FAULTED`` carrying the
    stderr tail; ``TIMEOUT`` and ``BUSY`` receipts stay untouched.

    Returns:
        The receipt with the row-driven status applied, or unchanged when no policy matches.
    """
    empty = (ToolGroup.EMPTY_ON_EXIT1 in tool.groups and done.returncode == 1 and _is_match_document(done.stdout)) or (
        tool.empty_signature is not None and done.returncode == tool.empty_signature[0] and tool.empty_signature[1] in done.stdout + done.stderr
    )
    match (empty, tool.defect_exit, done.returncode, done.status):
        case (True, _, _, _):
            return msgspec.structs.replace(done, status=RailStatus.EMPTY)
        case (False, int(defect), rc, _) if rc == defect:
            return msgspec.structs.replace(done, status=RailStatus.FAILED)
        case (False, int(), rc, RailStatus.FAILED) if rc != 0:
            return msgspec.structs.replace(done, status=RailStatus.FAULTED)
        case _:
            return done


def _is_match_document(raw: bytes) -> bool:
    try:
        AST_MATCHES.decode(raw or b"[]")
    except msgspec.MsgspecError:
        return False
    return True


async def _guarded(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[Completed, Fault]:
    t0 = time.monotonic()
    attempts = [1]
    argv: tuple[str, ...] = (check.tool.name,)

    try:
        match await _prepare_exec(check, settings, scope, routed, deadline):
            case Result(tag="ok", ok=prepared):
                argv = prepared.argv
                return (await _run_prepared(prepared, settings, scope, attempts)).map(
                    lambda done: apply_row_status(
                        check.tool,
                        msgspec.structs.replace(
                            done, duration_ms=(time.monotonic() - t0) * 1000.0, parser=check.tool.parser, sarif_dir=check.args.sarif_dir
                        ),
                    )
                )
            case Result(error=fault):
                return Error(fault)
    except (TimeoutError, FileNotFoundError, ValueError, OSError) as exc:
        return Error(_spawn_fault(argv, exc, attempts[0]))
    except asyncssh.Error as exc:
        return Error(_spawn_fault(argv, exc, attempts[0]))


def _exec_cwd(check: Check, settings: AssaySettings) -> str:
    match settings.exec_target:
        case Ssh() as target:
            return target.remote_workroot(settings.run_id)
        case _:
            return str(UPath(check.cwd or settings.local_root).path)


async def _prepare_exec(
    check: Check, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None
) -> Result[_PreparedExec, Fault]:
    match await to_thread.run_sync(_materialize, check, settings):
        case Result(tag="ok", ok=prepared):
            check = prepared
        case Result(error=fault):
            return Error(fault)
    if check.tool.claim in HOST_BOUND_CLAIMS and settings.exec_target:
        return Error(
            Fault((check.tool.name, check.tool.claim.value), status=RailStatus.UNSUPPORTED, message="host-bound tools require local execution")
        )
    match _argv(check, routed, settings=settings, scope=scope):
        case Result(tag="ok", ok=argv):
            pass
        case Result(error=fault):
            return Error(fault)
    limit = check.timeout if check.timeout is not None else check.tool.timeout
    bound = deadline if deadline is not None else (time.monotonic() + limit if limit is not None else None)
    cwd = _exec_cwd(check, settings)
    env = await to_thread.run_sync(_apphost, check.tool, _overlay(check.tool, settings, scope), abandon_on_cancel=True)
    propagate.inject(env)
    trace.get_current_span().set_attribute("exec.target", settings.exec_target.url if isinstance(settings.exec_target, Ssh) else "")
    return Ok(
        _PreparedExec(
            check=check,
            argv=argv,
            cwd=cwd,
            env=env,
            bound=bound,
            thread_limiter=_FAN_LIMITER.get() or anyio.CapacityLimiter(governed_concurrency(settings, (check,))),
        )
    )


async def _run_prepared(
    prepared: _PreparedExec, settings: AssaySettings, scope: ArtifactScope | None, attempts: list[int]
) -> Result[Completed, Fault]:
    async with dotnet_slot(prepared.check, settings, prepared.bound) as slot:
        match slot:
            case Result(tag="error", error=fault):
                return Error(fault)
            case Result(tag="ok", ok=slot_notes):
                done = await _execute_retrying(
                    prepared.check,
                    settings,
                    scope,
                    argv=prepared.argv,
                    cwd=prepared.cwd,
                    env=prepared.env,
                    thread_limiter=prepared.thread_limiter,
                    deadline=prepared.bound,
                    attempts=attempts,
                )
                return Ok(msgspec.structs.replace(done, notes=(*slot_notes, *done.notes)) if slot_notes else done)
            case never:  # pragma: no cover
                return Error(Fault(prepared.argv, status=RailStatus.FAULTED, message=str(never)))


def _spawn_fault(argv: tuple[str, ...], exc: BaseException, attempts: int) -> Fault:
    diagnose(exc)
    message = "deadline exceeded" if isinstance(exc, TimeoutError) else str(exc)
    stamped = f"{message} [attempts={attempts}]" if attempts > 1 else message
    match exc:
        case TimeoutError():
            return Fault(argv, status=RailStatus.TIMEOUT, message=stamped)
        case FileNotFoundError():
            return Fault(argv, status=RailStatus.UNSUPPORTED, message=stamped)
        case _:
            return Fault(argv, status=RailStatus.FAULTED, message=stamped)


async def _execute_retrying(  # ruff:ignore[too-many-arguments]
    check: Check,
    settings: AssaySettings,
    scope: ArtifactScope | None,
    *,
    argv: tuple[str, ...],
    cwd: str,
    env: Mapping[str, str],
    thread_limiter: anyio.CapacityLimiter,
    deadline: float | None,
    attempts: list[int],
) -> Completed:
    done: Completed | None = None
    retrying = stamina.retry_context(on=retry_predicate(check, deadline), attempts=3, timeout=_retry_timeout(deadline))
    async for attempt in retrying.with_name(check.tool.name, (), {}):
        attempts[0] = attempt.num
        with attempt:
            with anyio.fail_after(remaining(deadline)):
                done = await _execute(check, settings, scope, argv=argv, cwd=cwd, env=env, thread_limiter=thread_limiter)
    match done:
        case None:  # pragma: no cover
            raise RuntimeError("stamina exhausted without raising — invariant violated")
        case Completed() as result:
            return msgspec.structs.replace(result, notes=(*result.notes, f"retry attempts={attempts[0]}")) if attempts[0] > 1 else result


async def _execute(
    check: Check,
    settings: AssaySettings,
    scope: ArtifactScope | None,
    *,
    argv: tuple[str, ...],
    cwd: str,
    env: Mapping[str, str],
    thread_limiter: anyio.CapacityLimiter,
) -> Completed:
    match check.tool.runner:
        case Runner.INPROC:
            return await _inproc(check, limiter=thread_limiter)
        case _:
            return await _run_process_backend(
                ExecPlan(
                    argv=argv,
                    check=check,
                    cwd=cwd,
                    env=env,
                    settings=settings,
                    scope=scope,
                    streaming=check.tool.mode.stream,
                    tail_cap=settings.stream_tail_bytes,
                    spill_cap=settings.capture_spill_bytes,
                    chunk=settings.stream_chunk_bytes,
                    thread_limiter=thread_limiter,
                )
            )


def _retry_timeout(deadline: float | None) -> float:
    return min(30.0, remaining(deadline) or 30.0)


def retry_predicate(check: Check, deadline: float | None) -> Callable[[BaseException], bool]:
    """Build a stamina retry predicate that retries transport/spawn faults within the remaining budget.

    Returns:
        A predicate that retries connection/OS faults on non-direct runners while budget remains, never spawn/value/timeout faults.
    """

    def within_budget() -> bool:
        left = remaining(deadline)
        return left is None or left > _RETRY_MIN_REMAINING

    def classify(exc: BaseException) -> bool:
        match exc:
            case FileNotFoundError() | ValueError() | TimeoutError():
                return False
            case asyncssh.Error() | ConnectionError():
                return within_budget()
            case OSError():
                return check.tool.runner is not Runner.DIRECT and within_budget()
            case _:
                return False

    return classify


async def _inproc(check: Check, limiter: anyio.CapacityLimiter | None = None) -> Completed:
    match check.thunk:
        case None:
            return receipt((check.tool.name,), 1, stderr=b"INPROC check carries no thunk")
        case thunk:
            try:
                return await to_thread.run_sync(thunk, check, limiter=limiter)
            except Exception as exc:  # ruff:ignore[blind-except]
                return receipt((check.tool.name, *check.paths), 1, stderr=f"{type(exc).__name__}: {exc}".encode()[:1024])


def _spawn(check: Check, settings: AssaySettings) -> _Woven:
    span = traced(
        span=check.tool.name,
        attrs=lambda *_a, **_k: {"assay.run_id": settings.run_id, "assay.tool": check.tool.name},
        agent=lambda *_a, **_k: settings.agent_context,
    )
    weave: Callable[[_Woven], _Woven] = compose(checked(), span)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return weave(_guarded)


def run_check(
    check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> Result[Completed, Fault]:
    """Run one check under a single event loop.

    Returns:
        Completed receipt, or a fault when spawn, lease, or timeout handling fails.
    """

    async def _run() -> Result[Completed, Fault]:
        return await run_check_async(check, settings=settings, scope=scope, routed=routed, deadline=deadline)

    return anyio.run(_run)


async def run_check_async(
    check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> Result[Completed, Fault]:
    """Run one check inside an existing event loop.

    Returns:
        Completed receipt, or a fault when spawn, lease, or timeout handling fails.
    """
    return await _spawn(check, settings)(check, settings, scope, routed, deadline)


@dataclass(frozen=True, slots=True)
class EngineExecutor:
    """Production Executor over the engine spawn rails; the registry weave constructs the one bound instance."""

    def run(  # ruff:ignore[no-self-use]
        self, check: Check, *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> Result[Completed, Fault]:
        """Run one check under a single event loop.

        Returns:
            Completed receipt, or a fault when spawn, lease, or timeout handling fails.
        """
        return run_check(check, settings=settings, scope=scope, routed=routed, deadline=deadline)

    def fan(  # ruff:ignore[no-self-use]
        self, checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
    ) -> tuple[Result[Completed, Fault], ...]:
        """Run checks concurrently and preserve input order.

        Returns:
            One completed-or-fault result per input check.
        """
        return fan_out(checks, settings=settings, scope=scope, routed=routed, deadline=deadline)


def fan_out(
    checks: tuple[Check, ...], *, settings: AssaySettings, scope: ArtifactScope | None, routed: Routed, deadline: float | None = None
) -> tuple[Result[Completed, Fault], ...]:
    """Run checks concurrently and preserve input order.

    ``deadline`` is a shared absolute ``time.monotonic()`` ceiling; expired checks yield timeout faults in their slots.

    Returns:
        One completed-or-fault result per input check.
    """

    async def _worker(check: Check) -> Result[Completed, Fault]:
        return await run_check_async(check, settings=settings, scope=scope, routed=routed, deadline=deadline)

    async def _run() -> tuple[Result[Completed, Fault], ...]:
        reset_foreign_census()
        limit = governed_concurrency(settings, checks)
        token = _FAN_LIMITER.set(anyio.CapacityLimiter(limit))
        try:
            with _TRACER.start_as_current_span("assay.fan_out") as parent:
                parent.set_attribute("assay.checks_total", len(checks))
                parent.set_attribute("assay.checks_concurrency", limit)
                async with pooled_ssh(settings):
                    return await fan_schedule(checks, _worker, deadline=deadline, limit=limit)
        finally:
            _FAN_LIMITER.reset(token)

    return anyio.run(_run)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "EngineExecutor",
    "Executor",
    "apply_row_status",
    "argv_for",
    "fan_out",
    "retry_predicate",
    "run_check",
    "run_check_async",
    "splice_command",
]
