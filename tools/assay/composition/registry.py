"""Assay composition: registry, runners, Envelope emission, and history commands.

Owns the full execution path from a parsed Cyclopts invocation to a written Envelope:
claim dispatch, parameter binding and validation, rail-layer composition, artifact-scope
lifecycle, one-Envelope-per-call invariant enforcement, history persistence, and the
self-test/delta/parse-fault entry points that sit outside the REGISTRY fold.

Public surfaces: REGISTRY (Bind tuple), build_app, rail, self_test, delta, parse_fault,
Handler (type alias), and ORPHAN_MIN_AGE_S.
"""

from collections import deque
from collections.abc import Callable
from contextvars import ContextVar
from functools import reduce
from itertools import count, groupby
from operator import attrgetter
from pathlib import Path
import sys
import time
import tomllib
from types import FunctionType
from typing import Final, TYPE_CHECKING

from beartype.roar import BeartypeCallHintViolation
from cyclopts import App, Parameter
from expression import Error, Ok, Result
import msgspec
from opentelemetry import trace
import psutil
from pydantic import ValidationError
import structlog

from tools.assay.composition.catalog import select, TOOLS
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.aspect import _RING, checked_call, compose, Layer, logged, Slot, traced  # noqa: PLC2701
from tools.assay.core.engine import _RESOURCE, _snapshot, fan_out  # noqa: PLC2701
from tools.assay.core.model import (
    _HINT_CAP,  # noqa: PLC2701  # private symbol; canonical hint-cap clip site
    _RESULT_CAP,  # noqa: PLC2701  # private symbol; canonical result-cap saturation site
    Artifact,
    ArtifactKind,
    BaseParams,
    Bind,
    Check,
    Claim,
    Completed,
    Counts,
    Diagnostic,
    Envelope,
    envelope,
    Fault,
    field_cap,
    fold,
    Input,
    Language,
    Match,
    receipt,
    Report,
    RunDelta,
    Runner,
    RunSnapshot,
    Tool,
    validate_detail,
    wire_encode,
    wire_safe,
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus, Step
from tools.assay.rails import (
    api as api_rail,
    bridge as bridge_rail,
    code as code_rail,
    docs as docs_rail,
    package as package_rail,
    static as static_rail,
    test as test_rail,
)
from tools.assay.rails.api import ApiParams
from tools.assay.rails.bridge import BridgeParams
from tools.assay.rails.code import _cap_note, CodeParams  # noqa: PLC2701  # _cap_note: single truncation-note grammar owner
from tools.assay.rails.docs import DocsParams, FaultedPromotion
from tools.assay.rails.package import PackageParams
from tools.assay.rails.static import StaticParams
from tools.assay.rails.test import TestParams


if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping


# --- [TYPES] ----------------------------------------------------------------------------

# `P` is the verb params type, not a ParamSpec.
type Handler[P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]
type ReportLayer = Layer[[AssaySettings, ArtifactScope, object], Report]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACT_CAP: Final = 100
# Mirrors the msgspec-enforced cap on Fault.message so history round-trips without silent truncation.
_MESSAGE_CAP: Final[int] = field_cap(Fault, "message", default=1 << 62)
_DISPATCH_NONE: Final = f"{Step.DISPATCH}=none"
# Per-invocation counter; long-lived automation processes must not trip the one-Envelope guard on reuse.
_WRITES: ContextVar[Iterator[int]] = ContextVar("assay_writes")
_PROBE_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
_PROBE_TIMEOUT: Final = 8.0
_PROBE_TTL: Final = 900.0
_PROBE_CACHE_FILE: Final = "probe-cache.json"
_PROBE_CACHE_KEY: Final = "probe:%s"
_PYPROJECT: Final[Path] = Path(__file__).resolve().parents[3] / "pyproject.toml"
ORPHAN_MIN_AGE_S: Final = 900.0
_ORPHAN_PROCESS_TOKENS: Final[frozenset[str]] = frozenset(("python", "python3", "uv", "ty"))
# Longest-prefix-first: MODULE resolves before UV on the shared `uv run` head;
# DIRECT and DOTNET omitted — argv[0] is the tool binary, no lockfile fold needed.
_PROBE_LOCKED: Final[tuple[tuple[tuple[str, ...], str], ...]] = (
    (Runner.MODULE.prefix, "uv.lock"),
    (Runner.UV.prefix, "uv.lock"),
    (Runner.PNPM.prefix, "pnpm-lock.yaml"),
)
# Help-truth usage slots: the claim default names the polymorphic positional surface; verb overrides
# name the slots that BaseParams.bound projects out of paths (pattern/symbol/key/token stay legal tokens).
_CLAIM_SLOTS: Final[dict[Claim, str]] = {
    Claim.STATIC: "[--all | --project PROJECT | --folder F... --file F...]",
    Claim.CODE: "PATTERN [PATHS]...",
    Claim.TEST: "[PATHS]...",
    Claim.BRIDGE: "",
    Claim.PACKAGE: "",
    Claim.API: "",
    Claim.DOCS: "[PATHS]...",
}
_VERB_SLOTS: Final[dict[tuple[Claim, str], str]] = {
    (Claim.BRIDGE, "verify"): "[PATTERN]",
    (Claim.API, "resolve"): "[KEY [KIND]]",
    (Claim.API, "query"): "[SYMBOL]",
    (Claim.API, "show"): "[TOKEN]",
}

# --- [MODELS] ---------------------------------------------------------------------------


class _ProbeRow(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    # token encodes path+mtime; a tool upgrade changes the token and invalidates the cached entry.
    token: str = ""
    ts: float = 0.0
    note: str = ""
    ok: bool = True


class _ProcessRow(msgspec.Struct, frozen=True, gc=False):
    pid: int
    age_s: float
    command: str


# --- [TABLES] ---------------------------------------------------------------------------

_ENVELOPE_DECODER: Final[msgspec.json.Decoder[Envelope]] = msgspec.json.Decoder(Envelope)
_PROBE_DECODER: Final[msgspec.json.Decoder[dict[str, _ProbeRow]]] = msgspec.json.Decoder(dict[str, _ProbeRow])

# --- [SERVICES] -------------------------------------------------------------------------

_LOG: Final = structlog.get_logger("assay.registry")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _identity_hom(*_a: object, **_k: object) -> Result[Report, Fault]:
    return Ok(fold(Claim.STATIC, "self-test", ()))


def _checked_report(fn: Handler[object]) -> Handler[object]:
    return checked_call(fn)


def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object) -> Mapping[str, object]:
    return {"run_id": settings.run_id, "strict": getattr(params, "strict", False), **settings.agent_context}


def _seed_parse_ring(dispatch: str, tokens: tuple[str, ...]) -> None:
    cmd = " ".join(tokens)[:_HINT_CAP]
    match _RING.get():
        case deque() as ring:
            ring.extend((f"{Step.DISPATCH}={dispatch}", cmd))
        case None:
            _RING.set(deque((f"{Step.DISPATCH}={dispatch}", cmd), maxlen=16))


def _bound(params: object, claim: Claim, verb: str) -> Result[object, Fault]:
    # BaseParams.bound validates positional arity; surplus tokens surface via _seed_parse_ring for hint distillation.
    match params:
        case BaseParams() as p:
            match p.bound(verb):
                case Fault() as projected:
                    _seed_parse_ring(claim.value, (claim.value, verb, *p.paths))
                    return Error(projected)
                case bound_params:
                    return Ok(bound_params)
        case _:
            return Ok(params)


def _strict(outcome: Result[Report, Fault], params: object) -> Result[Report, Fault]:
    # EMPTY and SKIP promote to FAULTED under --strict; defect and infrastructure statuses already dominate.
    match outcome:
        case Result(tag="ok", ok=report) if getattr(params, "strict", False) and report.status in {RailStatus.EMPTY, RailStatus.SKIP}:
            return Error(Fault((), RailStatus.FAULTED, f"{Step.STRICT}: empty/skipped fold"))
        case _:
            return outcome


def _failing_step(fault: Fault) -> Step:
    # Derives the failing step from status, argv, and the prefix-scan roster owned by _guard.
    match (fault.status, fault.argv):
        case (RailStatus.TIMEOUT, _):
            return Step.TIMEOUT
        case (RailStatus.BUSY, _):
            return Step.LEASE_BUSY
        case (_, ()):
            return next((step for step in Step if step.scan and step is not Step.SPAWN and fault.message.startswith(f"{step}:")), Step.SPAWN)
        case _:
            return Step.SPAWN


def _distill(
    fault: Fault, duration_ms: float, *, events: tuple[str, ...] | None = None, resource: tuple[tuple[str, float], ...] = (), step: Step | None = None
) -> tuple[Diagnostic, bool]:
    # Resource snapshots are lazy; clipping any byte sets Envelope.truncated.
    ring = events if events is not None else tuple(_RING.get() or ())
    step = step if step is not None else _failing_step(fault)
    reason = fault.message.removeprefix(f"{step}: ") or (ring[-1] if ring else "")
    framing = f"{step}: after {duration_ms:.1f}ms"
    # Reserve one byte for the space separating `budgeted` and the framing suffix, keeping len(hint) <= _HINT_CAP.
    budgeted = reason[: max(_HINT_CAP - len(framing) - 1, 0)]
    hint = f"{step}: {budgeted} after {duration_ms:.1f}ms"
    truncated = len(reason) > len(budgeted) or len(fault.message) >= _MESSAGE_CAP or fault.message.endswith("…")
    snap = resource or _RESOURCE.get() or tuple(_snapshot().items())
    ctx = trace.get_current_span().get_span_context()
    ids = (f"{ctx.trace_id:032x}", f"{ctx.span_id:016x}") if ctx.is_valid else ("", "")
    return Diagnostic(
        failing_step=step,
        recent_events=ring,
        elapsed_ms=duration_ms,
        hint=hint,
        dispatched=not (ring and ring[0] == _DISPATCH_NONE),
        resource=snap,
        trace_id=ids[0],
        span_id=ids[1],
    ), truncated


def _full_report_artifact(settings: AssaySettings, bind: Bind, report: Report) -> tuple[Artifact, ...]:
    name = f"{bind.claim.value}-{bind.verb}.full-report.json"
    try:
        path, size = settings.store().write_full_report(settings.run_id, name, report)
    except OSError as exc:
        _LOG.warning("history.full_report_failed", run_id=settings.run_id, error=str(exc)[:200])
        return ()
    return (Artifact(id="full-report", kind=ArtifactKind.HISTORY, path=path, bytes=size, lines=0),)


def _ok_envelope(bind: Bind, settings: AssaySettings, ms: float, report: Report) -> Envelope:
    # FAILED reports carry a Diagnostic built from defect result rows, matching the fault-rail shape.
    # The in-band N-of-M note rides the rails/code.py _cap_note grammar with the registry's full-report
    # suffix; the cap that fired (results before artifacts) owns the (shown, total) pair.
    truncated = len(report.results) > _RESULT_CAP or len(report.artifacts) > _ARTIFACT_CAP
    if truncated:
        artifact = _full_report_artifact(settings, bind, report)
        artifacts = ((*artifact, *report.artifacts) if artifact else report.artifacts)[:_ARTIFACT_CAP]
        cap, total = (_RESULT_CAP, len(report.results)) if len(report.results) > _RESULT_CAP else (_ARTIFACT_CAP, len(report.artifacts))
        note = _cap_note(cap, total, cap, tail=f"full report artifact under {settings.run_id}")
        report = msgspec.structs.replace(report, results=report.results[:_RESULT_CAP], artifacts=artifacts, notes=(*report.notes, *note))
    failed = tuple(m for m in report.results if m.severity == "failed")
    defect_events = tuple(f"{m.id}: {m.text[:120]}" for m in failed)
    ctx = (
        _distill(Fault((), RailStatus.FAILED, f"{len(failed)} tool(s) failed"), ms, events=defect_events, step=Step.DEFECTS)[0]
        if report.status is RailStatus.FAILED
        else None
    )
    return Envelope(
        schema_version=1,
        claim=bind.claim,
        verb=bind.verb,
        status=report.status,
        exit_code=report.status.exit_code,
        run_id=settings.run_id,
        duration_ms=ms,
        report=report,
        error_context=ctx,
        truncated=truncated,
        notes=report.notes,
    )


def _narrow(handler: object) -> Handler[object]:
    # Rail modules rely on return annotations for beartype; FunctionType confirms the match without a cast.
    match handler:
        case FunctionType() as fn:
            return fn
        case _:
            raise TypeError(f"Bind.handler must be a module-level def (FunctionType), got {type(handler).__name__}")


def _validated(outcome: Result[Report, Fault]) -> Result[Report, Fault]:
    match outcome:
        case Result(tag="ok", ok=report):
            validate_detail(report.detail)
            return outcome
        case _:
            return outcome


def _guard(thunk: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:
    # Owns the strict:/validation:/config: fault-prefix contract; rail modules must not emit these prefixes.
    try:
        return thunk()
    except FaultedPromotion as promoted:
        return Error(Fault((), RailStatus.FAULTED, f"{Step.STRICT}: {promoted}"))
    except BeartypeCallHintViolation as violation:
        return Error(Fault((), RailStatus.FAULTED, f"{Step.VALIDATION}: {violation}"))
    except msgspec.MsgspecError as malformed:
        return Error(Fault((), RailStatus.FAULTED, f"{Step.VALIDATION}: {malformed}"))


def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    # Enforces the one-write guard; parse faults skip history persistence.
    ms = (time.perf_counter() - started) * 1000.0
    persist = True
    match outcome:
        case Result(tag="ok", ok=report):
            envelope = _ok_envelope(bind, settings, ms, report)
        case Result(error=fault):
            diagnostic, truncated = _distill(fault, ms)
            persist = diagnostic.failing_step != Step.PARSE
            envelope = Envelope(
                schema_version=1,
                claim=bind.claim,
                verb=bind.verb,
                status=fault.status,
                exit_code=fault.status.exit_code,
                run_id=settings.run_id,
                duration_ms=ms,
                error=fault,
                error_context=diagnostic,
                truncated=truncated,
            )
    match next(_WRITES.get()):
        case 0:
            return _emit_envelope(settings, envelope, persist=persist)
        case rank:
            doubled = Envelope(
                schema_version=1,
                claim=bind.claim,
                verb=bind.verb,
                status=RailStatus.FAULTED,
                exit_code=RailStatus.FAULTED.exit_code,
                run_id=settings.run_id,
                error=Fault((), RailStatus.FAULTED, f"second Envelope suppressed (write #{rank}); Invariant 1 violated"),
            )
            sys.stderr.buffer.write(wire_encode(doubled) + b"\n")
            return doubled


def rail(bind: Bind, settings: AssaySettings | None = None) -> Callable[[object], Envelope]:
    """Build the registry runner for one bound verb.

    Each call to the returned runner emits exactly one Envelope to stdout and,
    unless the failing step is "parse", persists it to the history store.  Ring,
    write counter, and resource snapshot context-vars are invocation-scoped and
    reset in the finally block.  An OSError from ArtifactScope.open folds to a
    FAULTED envelope rather than propagating.

    Args:
        bind: Registry row carrying the claim, verb, handler, and params type.
        settings: Resolved settings; constructed from the environment when absent.

    Returns:
        Runner that accepts a params object and returns the emitted Envelope.
    """
    handler: Handler[object] = compose(*_RAIL_LAYERS)(_narrow(bind.handler))

    def run(params: object) -> Envelope:
        active = settings or AssaySettings()
        started = time.perf_counter()
        ring_token, writes_token, resource_token = _RING.set(deque(maxlen=16)), _WRITES.set(count()), _RESOURCE.set(())
        try:
            # ArtifactScope.open raises OSError when the .artifacts root is unwritable; fold it here outside _guard.
            scope = ArtifactScope.open(active, bind.claim)
            outcome = _guard(lambda: _bound(params, bind.claim, bind.verb).bind(lambda p: _validated(_strict(handler(active, scope, p), p))))
            return _emit(bind, active, started, outcome)
        except OSError as exc:
            return _emit(bind, active, started, Error(Fault((), RailStatus.FAULTED, f"scope: {exc}")))
        finally:
            _RESOURCE.reset(resource_token)
            _WRITES.reset(writes_token)
            _RING.reset(ring_token)

    run.__name__ = bind.verb
    return run


def _encode(envelope: Envelope) -> bytes:
    # Lone surrogates in untrusted argv/paths raise UnicodeEncodeError; fold to a scrubbed FAULTED Envelope to preserve the one-Envelope contract.
    try:
        return wire_encode(envelope)
    except UnicodeEncodeError:
        safe = Envelope(
            schema_version=1,
            claim=envelope.claim,
            verb=wire_safe(envelope.verb),
            status=RailStatus.FAULTED,
            exit_code=RailStatus.FAULTED.exit_code,
            notes=("output contained invalid characters",),
        )
        return wire_encode(safe)


def _persist(settings: AssaySettings, envelope: Envelope) -> None:
    # Best-effort; `delta` decodes raw encoded bytes directly from the history store.
    try:
        store = settings.store()
        store.write_history(settings.run_id, _encode(envelope))
        store.retain_history(settings.artifact_retention)
        store.retain_scopes(envelope.claim, settings.artifact_retention)
    except OSError as exc:
        _LOG.warning("history.persist_failed", run_id=settings.run_id, error=str(exc)[:200])


def _emit_envelope(settings: AssaySettings, envelope: Envelope, *, persist: bool) -> Envelope:
    sys.stdout.buffer.write(_encode(envelope) + b"\n")
    if persist:
        _persist(settings, envelope)
    return envelope


def _prior(run_ids: tuple[str, ...], run_id: str) -> str:
    earlier = tuple(r for r in run_ids if r < run_id)
    return earlier[-1] if earlier else ""


def _delta_report(before_id: str, after_id: str, before: Envelope | None, after: Envelope | None) -> Report:
    # Compares by status, counts, and `(id, line)` result keys; a missing side folds to EMPTY.
    def snapshot(run_id: str, env: Envelope) -> tuple[RunSnapshot, frozenset[tuple[str, int]]]:
        report = env.report
        counts = report.counts if report is not None else Counts()
        keys = frozenset((m.id, m.line) for m in (report.results if report is not None else ()))
        return RunSnapshot(id=run_id, status=env.status, counts=counts), keys

    match (before, after):
        case (Envelope() as b, Envelope() as a):
            (before_snap, before_keys), (after_snap, after_keys) = snapshot(before_id, b), snapshot(after_id, a)
            detail = RunDelta(before=before_snap, after=after_snap, added=len(after_keys - before_keys), removed=len(before_keys - after_keys))
            # No note: status/counts/added/removed live in detail.before/after/added/removed — agents read the structured RunDelta.
            return fold(Claim.STATIC, "delta", (Completed(("delta", after_id), 0, status=RailStatus.OK),), detail=detail)
        case _:
            missing = after_id if after is None else before_id
            note = f"delta: run not found in history: {missing or '(no prior run)'}"
            return fold(Claim.STATIC, "delta", (Completed(("delta", after_id), 0, status=RailStatus.EMPTY, notes=(note,)),))


def delta(run_id: str, *, against: str = "") -> Envelope:
    """Diff two persisted run Envelopes from the history store.

    When against is empty, the comparison base is the most recent run whose id
    sorts lexicographically before run_id.  The result Envelope is emitted to
    stdout but not persisted to history.

    Args:
        run_id: Target run id; must exist in the history store.
        against: Explicit base run id; defaults to the nearest prior run.

    Returns:
        Emitted delta Envelope with a RunDelta detail, or an EMPTY Envelope when
        either run is absent from the store.
    """
    settings = AssaySettings()
    store = settings.store()
    before_id = against or _prior(store.sorted_history_ids(), run_id)
    report = _delta_report(before_id, run_id, store.load_history(before_id), store.load_history(run_id))
    env = msgspec.structs.replace(envelope(report, claim=Claim.STATIC, verb="delta", run_id=settings.run_id), notes=report.notes)
    return _emit_envelope(settings, env, persist=False)


def _parse_dispatch(tokens: tuple[str, ...]) -> tuple[Claim, str, str]:
    match tokens:
        case (head, *rest) if head in Claim._value2member_map_:
            return Claim(head), (rest[0] if rest else ""), head
        case (head, *_):
            return Claim.STATIC, head, "none"
        case _:
            return Claim.STATIC, "", "none"


def _validation_message(error: ValidationError) -> str:
    rows = tuple(
        f"{'.'.join(str(part) for part in item.get('loc', ()) if part != '__root__') or 'settings'}: {item.get('msg', 'invalid')}"
        for item in error.errors(include_url=False, include_context=False, include_input=False)
    )
    return "; ".join(rows) or str(error)


def parse_fault(tokens: tuple[str, ...], message: str, *, step: Step = Step.PARSE) -> Envelope:
    """Convert a Cyclopts parse failure into a FAULTED Envelope and emit it.

    The emitted fault message takes the form "{step}: {message}".  When
    AssaySettings construction fails, the step is overridden to "config" and the
    message contains the Pydantic validation summary.  The Envelope is not
    persisted to history.

    Args:
        tokens: Raw argv tokens used to derive the claim, verb, and parse-ring hint.
        message: Cyclopts error text; clipped to the Fault message cap.
        step: Fault-prefix label; "parse" by default; callers may pass "config".

    Returns:
        Emitted parse-fault Envelope with a FAULTED status and diagnostic context.
    """
    try:
        settings = AssaySettings()
        fault_message = f"{step}: {message}"
    except ValidationError as config_error:
        settings = AssaySettings.model_construct()
        fault_message = f"{Step.CONFIG}: {_validation_message(config_error)}"
    claim, verb, dispatch = _parse_dispatch(tokens)
    fault = Fault((), RailStatus.FAULTED, fault_message[:_MESSAGE_CAP])
    _seed_parse_ring(dispatch, tokens)
    diagnostic, truncated = _distill(fault, 0.0)
    env = msgspec.structs.replace(envelope(fault, claim=claim, verb=verb, run_id=settings.run_id, error_context=diagnostic), truncated=truncated)
    return _emit_envelope(settings, env, persist=False)


def _probe_token(argv: tuple[str, ...]) -> str | None:
    import shutil  # noqa: PLC0415  # deferred: avoids module-load cost on non-probe paths

    def mtime(path: str | None) -> int | None:
        try:
            return Path(path).stat().st_mtime_ns if path is not None else None
        except OSError:
            return None

    def lock_path(name: str) -> str | None:
        # Walks from the .venv shim (not the Nix-store target) to the nearest ancestor lockfile; None means program mtime alone forms the token.
        bases = Path(sys.executable).parents
        return next((str(base / name) for base in bases if (base / name).exists()), None)

    matched = next(((prefix, lock) for prefix, lock in _PROBE_LOCKED if argv[: len(prefix)] == prefix), None) if argv else None
    match matched:
        case (prefix, lock):
            program = argv[len(prefix)] if len(argv) > len(prefix) else ""
            resolved = shutil.which(program) or (sys.executable if "." in program or not program else None)
            program_mtime, lock_mtime = mtime(resolved), mtime(lock_path(lock))
            return None if program_mtime is None and lock_mtime is None else f"{resolved}|{program_mtime}|{lock}:{lock_mtime}"
        case _ if argv:
            path = shutil.which(argv[0])
            return None if (m := mtime(path)) is None else f"{path}|{m}"
        case _:
            return None


def _tooling_process(cmdline: tuple[str, ...]) -> bool:
    return any(Path(part).name in _ORPHAN_PROCESS_TOKENS or Path(part).name.startswith("python") for part in cmdline)


def _orphan_process(proc: psutil.Process, root: Path, *, now: float) -> _ProcessRow | None:
    try:
        info = proc.info
        cmdline = tuple(str(part) for part in (info.get("cmdline") or ()))
        command = " ".join(cmdline) or str(info.get("name") or "")
        in_root = bool(info.get("cwd")) and Path(str(info["cwd"])).resolve().is_relative_to(root)
    except OSError, psutil.Error, TypeError, ValueError:
        return None
    age = max(now - float(info.get("create_time") or now), 0.0)
    match (int(info.get("ppid") or -1) == 1, in_root, age >= ORPHAN_MIN_AGE_S, _tooling_process(cmdline or (command,))):
        case (True, True, True, True):
            return _ProcessRow(pid=int(info.get("pid") or 0), age_s=age, command=command[:160])
        case _:
            return None


def _process_hygiene(settings: AssaySettings) -> tuple[tuple[Match, ...], tuple[str, ...]]:
    root, now = Path(str(settings.root)).resolve(), time.time()
    rows = tuple(
        filter(None, (_orphan_process(proc, root, now=now) for proc in psutil.process_iter(("pid", "ppid", "create_time", "name", "cmdline", "cwd"))))
    )
    if not rows:
        note = "process hygiene: no orphaned repo Python/UV/type-server processes"
        return (Match(id="process-hygiene", kind=ArtifactKind.PROCESS, text=note),), (note,)
    notes = tuple(f"process hygiene: orphan pid={row.pid} age={row.age_s / 60:.0f}m command={row.command}" for row in rows)
    return tuple(
        Match(id=f"process-hygiene-{row.pid}", kind=ArtifactKind.PROCESS, text=note, severity="failed") for row, note in zip(rows, notes, strict=True)
    ), notes


def _probe(name: str, argv: tuple[str, ...]) -> Check:
    tool = Tool(name=name, runner=Runner.DIRECT, command=argv, input=Input.NONE, language=Language.PYTHON, claim=Claim.STATIC, timeout=_PROBE_TIMEOUT)
    return Check(tool=tool)


_GIT_HEAD: Final = _probe("git-head", ("git", "rev-parse", "--short", "HEAD"))
_GIT_DIRTY: Final = _probe("git-dirty", ("git", "status", "--porcelain"))


def _cache_hit(cache: Mapping[str, _ProbeRow], argv: tuple[str, ...]) -> tuple[str, bool] | None:
    match (cache.get(_PROBE_CACHE_KEY % "\x00".join(argv)), _probe_token(argv)):
        case (_ProbeRow(token=stored, ts=ts, note=note, ok=ok), str() as token) if stored == token and 0 <= time.time() - ts < _PROBE_TTL:
            return note, ok
        case _:
            return None


def _probe_cache_load(settings: AssaySettings) -> dict[str, _ProbeRow]:
    try:
        return _PROBE_DECODER.decode(settings.store().read_bytes("cache", _PROBE_CACHE_FILE))
    except OSError, msgspec.MsgspecError:
        return {}


def _probe_cache_store(
    settings: AssaySettings, prior: Mapping[str, _ProbeRow], fresh: Mapping[tuple[str, ...], tuple[str, bool]], current: frozenset[tuple[str, ...]]
) -> None:
    # Best-effort; only token-resolvable probes for catalogued tools persist; removed-tool keys evict.
    fresh_rows = {
        _PROBE_CACHE_KEY % "\x00".join(argv): _ProbeRow(token=token, ts=time.time(), note=note, ok=ok)
        for argv, (note, ok) in fresh.items()
        if argv in current
        for token in (_probe_token(argv),)
        if token is not None
    }
    current_keys = frozenset(_PROBE_CACHE_KEY % "\x00".join(argv) for argv in current)
    retained = {key: row for key, row in prior.items() if key in current_keys and key not in fresh_rows}
    try:
        store = settings.store()
        store.write_bytes(wire_encode({**retained, **fresh_rows}), "cache", _PROBE_CACHE_FILE)
    except OSError as exc:
        _LOG.warning("probe_cache.store_failed", run_id=settings.run_id, error=str(exc)[:200])


def _tool_probes() -> tuple[Check, ...]:
    # DOTNET tools share one SDK probe; INPROC tools have no external --version surface.
    def keyed(tool: Tool) -> tuple[str, tuple[str, ...]]:
        match tool.runner:
            case Runner.DOTNET:
                return "dotnet", ("dotnet", "--version")
            case Runner.UV if tool.groups:
                group_flags = tuple(part for group in tool.groups for part in ("--group", group))
                return f"{tool.runner.value}:{tool.command[0]}:{','.join(tool.groups)}", (
                    *tool.runner.prefix,
                    *group_flags,
                    tool.command[0],
                    "--version",
                )
            case _:
                return f"{tool.runner.value}:{tool.command[0]}", (*tool.runner.prefix, tool.command[0], "--version")

    deduped = dict(keyed(t) for t in TOOLS if t.runner is not Runner.INPROC)
    return tuple(_probe(argv[-2], argv) for argv in deduped.values())


def _probe_note(check: Check, result: Result[Completed, Fault]) -> tuple[str, bool]:
    name = check.tool.name
    match result.ok if result.is_ok() else None:
        case None:
            note = f"git: {name.removeprefix('git-')} unavailable" if name in {"git-head", "git-dirty"} else f"tool {name}: MISSING"
            return note, name in {"git-head", "git-dirty"}
        case Completed() as d if name == "git-head":
            return f"git: HEAD {d.stdout.decode(errors='replace').strip()[:40] or 'unknown'}", True
        case Completed() as d if name == "git-dirty":
            return f"git: {'dirty' if d.stdout.strip() else 'clean'}", True
        case Completed(returncode=0) as d:
            lines = d.stdout.decode(errors="replace").strip().splitlines()
            return f"tool {name}: {lines[0][:80] if lines else 'present'}", True
        case Completed() as d:
            # Exit-code alone cannot distinguish a present-but-nonzero `--version` from a launcher that swallowed a missing-tool error.
            lines = (d.stdout or d.stderr).decode(errors="replace").strip().splitlines()
            version = f": {lines[0][:80]}" if lines else ""
            return f"tool {name}: present (exit {d.returncode}){version}", True


def _census() -> bool:
    # Every Tool must select back to itself; catches catalog/claim mismatches when adding TOOLS rows.
    return all(t in select(t.claim, t.language) for t in TOOLS)


def _yak_ready() -> bool:
    # shutil.which matches the DIRECT runner's execvp PATH lookup; os.access would check CWD-relative paths instead.
    import shutil  # noqa: PLC0415  # deferred: executed only on --rhino path

    return any(
        t.runner is Runner.DIRECT and t.command and t.command[0] == "yak" and shutil.which(t.command[0]) is not None
        for t in TOOLS
        if t.claim is Claim.PACKAGE
    )


def _health(settings: AssaySettings) -> tuple[tuple[Match, ...], tuple[str, ...]]:
    # Missing tools appear in notes, not faults. Tool probes cache by (path, mtime, TTL); git probes are always live.
    probes = (*_tool_probes(), _GIT_HEAD, _GIT_DIRTY)
    volatile = frozenset(c.tool.command for c in (_GIT_HEAD, _GIT_DIRTY))
    cache = _probe_cache_load(settings)
    hits = {c.tool.command: hit for c in probes if c.tool.command not in volatile for hit in (_cache_hit(cache, c.tool.command),) if hit is not None}
    misses = tuple(c for c in probes if hits.get(c.tool.command) is None)
    results = fan_out(misses, settings=settings, scope=None, routed=_PROBE_ROUTED)
    fresh = dict(zip((c.tool.command for c in misses), map(_probe_note, misses, results, strict=True), strict=True))
    current = frozenset(c.tool.command for c in probes if c.tool.command not in volatile)
    _probe_cache_store(settings, cache, fresh, current)
    noted = tuple(hits.get(c.tool.command) or fresh[c.tool.command] for c in probes)
    hygiene = _process_hygiene(settings)
    return (
        (
            *tuple(
                Match(id=check.tool.name, kind=ArtifactKind.PROCESS, text=note, severity="failed" if not ok else None)
                for check, (note, ok) in zip(probes, noted, strict=True)
            ),
            *hygiene[0],
        ),
        (*tuple(note for note, _ in noted), *hygiene[1]),
    )


def self_test(*, rhino: bool = False) -> Envelope:
    """Run the Assay composition preflight and emit a health Envelope.

    Verifies handler callability for every REGISTRY row, claim coverage, rail-layer
    composition, catalog census, and tool/process health probes.  When rhino is
    True, the check additionally requires that a yak binary is present for the
    PACKAGE claim; a missing yak yields FAILED rather than OK.

    Args:
        rhino: When True, fail if the Rhino packaging tool (yak) is not on PATH.

    Returns:
        Emitted preflight Envelope with OK or FAILED status and health-probe notes.
    """
    settings = AssaySettings()
    bound_claims = frozenset(b.claim for b in REGISTRY)
    health_probes, health_notes = _health(settings)
    yak = _yak_ready()
    # Roster derives from the Claim enum, not from REGISTRY: every declared claim must have at least one bound row,
    # so the conjunct can actually fail when a claim loses its last verb (the REGISTRY-derived set was tautological).
    healthy = all(callable(b.handler) for b in REGISTRY) and all(c in bound_claims for c in Claim) and _composes() and _census()
    status = RailStatus.FAILED if (not healthy or (rhino and not yak)) else RailStatus.OK
    summary = (
        f"rows={len(REGISTRY)} claims={len(bound_claims)} tools={len(TOOLS)} "
        f"healthy={healthy} yak_ready={yak} rhino={'required' if rhino else 'skipped'}"
    )
    report = fold(Claim.STATIC, "self-test", (receipt(("assay", "self-test"), 0 if status is RailStatus.OK else 1, status=status, notes=(summary,)),))
    report = msgspec.structs.replace(
        report,
        results=(
            *report.results,
            *(Match(id=b.verb, kind=ArtifactKind.PROCESS, text=f"{b.claim.value} {b.verb}", severity=None) for b in REGISTRY),
            *health_probes,
        ),
    )
    return _emit_envelope(
        settings,
        msgspec.structs.replace(envelope(report, claim=Claim.STATIC, verb="self-test", run_id=settings.run_id), notes=(summary, *health_notes)),
        persist=True,
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

# compose sorts layers by Slot; rails apply checked/logged/traced without spawn retry.
_RAIL_LAYERS: Final[tuple[ReportLayer, ...]] = (
    (Slot.checked, _checked_report),
    logged(event="rail", keys=_correlate),
    traced(span="assay.rail", attrs=_correlate),
)

REGISTRY: Final[tuple[Bind, ...]] = (
    Bind(Claim.STATIC, "check", static_rail.check, StaticParams, "Scoped route and argv preview."),
    Bind(Claim.STATIC, "build", static_rail.build, StaticParams, "Single-project or whole-workspace diagnostics, restore, and build."),
    Bind(Claim.STATIC, "fix", static_rail.fix, StaticParams, "Scoped native formatter and autofix."),
    Bind(Claim.CODE, "search", code_rail.search, CodeParams, "Search: $-metavar -> ast-grep structural; literal -> ripgrep content."),
    Bind(Claim.CODE, "query", code_rail.query, CodeParams, "AST query via tree-sitter (in-process)."),
    Bind(Claim.TEST, "run", test_rail.run, TestParams, "Unit + coverage + mutation fold."),
    Bind(Claim.TEST, "list", test_rail.list, TestParams, "Bounded discovery JSON."),
    Bind(Claim.TEST, "coverage", test_rail.coverage, TestParams, "Coverlet json + cobertura."),
    Bind(Claim.BRIDGE, "verify", bridge_rail.verify, BridgeParams, "Live RhinoWIP scenario fold."),
    Bind(Claim.BRIDGE, "doctor", bridge_rail.doctor, BridgeParams, "Bridge host health."),
    Bind(Claim.BRIDGE, "launch", bridge_rail.launch, BridgeParams, "Start RhinoWIP under lease."),
    Bind(Claim.BRIDGE, "quit", bridge_rail.quit, BridgeParams, "Clean Cocoa terminate."),
    Bind(Claim.BRIDGE, "check", bridge_rail.check, BridgeParams, "Liveness probe."),
    Bind(Claim.BRIDGE, "clean", bridge_rail.clean, BridgeParams, "Clear crash markers + autosave."),
    Bind(Claim.BRIDGE, "build", bridge_rail.build, BridgeParams, "Compile rasm-bridge plugin."),
    Bind(Claim.PACKAGE, "stage", package_rail.stage, PackageParams, "Yak stage commit under lease."),
    Bind(Claim.PACKAGE, "deploy", package_rail.deploy, PackageParams, "Yak install to live host."),
    Bind(Claim.PACKAGE, "publish", package_rail.publish, PackageParams, "Yak push to server."),
    Bind(Claim.PACKAGE, "list", package_rail.list, PackageParams, "Package project roster: slug/csproj pairs."),
    Bind(Claim.PACKAGE, "plan", package_rail.plan, PackageParams, "Stage plan into notes."),
    Bind(Claim.API, "doctor", api_rail.doctor, ApiParams, "Host/NuGet/tool health; --strict -> FAULTED."),
    Bind(Claim.API, "resolve", api_rail.resolve, ApiParams, "Asset path resolution."),
    Bind(Claim.API, "query", api_rail.query, ApiParams, "Polymorphic ilspy surface; fingerprint cache."),
    Bind(Claim.API, "show", api_rail.show, ApiParams, "Artifact preview."),
    Bind(Claim.DOCS, "check", docs_rail.check, DocsParams, "Markdown + Mermaid validation."),
)


def _composes() -> bool:
    # Identity handler catches Slot-order inversions in _RAIL_LAYERS without spawning a process.
    try:
        compose(*_RAIL_LAYERS)(_identity_hom)
    except TypeError:
        return False
    return True


def _read_version() -> str:
    try:
        v = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8")).get("project", {}).get("version", "")
        return v if isinstance(v, str) and v else "0.0.0"
    except OSError, tomllib.TOMLDecodeError:
        return "0.0.0"


_VERSION: Final[str] = _read_version()


def _leaf(bind: Bind) -> Callable[[object], Envelope]:
    # Cyclopts needs a concrete `params` default; functools.wraps is avoided because __wrapped__ exposes
    # the defaultless runner to Cyclopts signature inspection. PEP 649 lazy annotations satisfy ty without imports.
    runner = rail(bind)

    def command(params: object = bind.params()) -> Envelope:
        return runner(params)

    command.__name__ = bind.verb
    command.__qualname__ = f"_leaf.{bind.verb}"
    command.__doc__ = bind.help
    command.__annotate__ = lambda _format: {"params": bind.params, "return": Envelope}
    return command


def _usage(bind: Bind) -> str:
    slots = _VERB_SLOTS.get((bind.claim, bind.verb), _CLAIM_SLOTS[bind.claim])
    return " ".join(part for part in ("Usage: assay", bind.claim.value, bind.verb, slots, "[OPTIONS]") if part)


def _register[**P](
    app: App,
    obj: App | Callable[P, object],
    *,
    name: str | None = None,
    help: str = "",  # noqa: A002  # cyclopts names this kwarg "help"; intentional shadow
    usage: str | None = None,
) -> App:
    # App.command returns the registered sub-app, not the parent; returning app keeps reduce folds linear.
    extras: dict[str, str] = {key: value for key, value in (("help", help), ("usage", usage)) if value}
    match name:
        case None:
            app.command(obj)
        case verb:
            app.command(obj, name=verb, **extras)
    return app


def build_app(registry: tuple[Bind, ...]) -> App:
    """Build the Cyclopts command tree from registry rows.

    Groups rows by claim to produce one sub-app per claim, then registers each
    verb as a leaf command under its claim sub-app.  self_test and delta are
    registered directly on the root app outside the claim fold.

    Args:
        registry: Ordered Bind tuple; typically the module-level REGISTRY constant.

    Returns:
        Configured Cyclopts App with claim sub-apps, verb leaves, self-test, and
        delta registered.
    """
    root = App(
        name="assay",
        help="Rasm polyglot quality operator.",
        version=_VERSION,
        # Global --version is disabled so the token routes to verb params (PackageParams.version);
        # with the default flag active, cyclopts intercepts `package plan --version X` and prints the
        # app version on bare stdout — a total envelope-contract violation. `self-test` reports versions.
        version_flags=(),
        default_parameter=Parameter(show_default=True),
        result_action="return_value",
        backend="asyncio",
        exit_on_error=False,
        print_error=False,
        help_on_error=False,
    )
    keyed = sorted(registry, key=lambda b: b.claim.value)
    subs = tuple(
        reduce(
            lambda app, row: _register(app, _leaf(row), name=row.verb, help=row.help, usage=_usage(row)),
            tuple(rows),
            App(name=claim.value, version_flags=(), usage=f"Usage: assay {claim.value} COMMAND"),
        )
        for claim, rows in groupby(keyed, key=attrgetter("claim"))
    )
    app = reduce(_register, subs, root)
    _register(app, self_test, name="self-test")
    _register(app, delta, name="delta")
    return app


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ORPHAN_MIN_AGE_S", "REGISTRY", "Handler", "build_app", "delta", "parse_fault", "rail", "self_test"]
