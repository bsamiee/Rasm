"""Compose Assay registry rows into commands, rail runners, and persisted envelopes.

This module owns claim dispatch, parameter binding, rail-layer composition, artifact
scope lifecycle, one-envelope enforcement, history persistence, and root commands that
sit outside the `REGISTRY` fold. Probe, cache, and hygiene evidence folds in from the
health rail; the registry keeps only verb binding and layer-composition checks.
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
from typing import Annotated, Final, TYPE_CHECKING

from beartype.roar import BeartypeCallHintViolation
from cyclopts import App, Parameter
from expression import Error, Ok, Result
import msgspec
from opentelemetry import trace
from pydantic import ValidationError
import structlog

from tools.assay.composition.catalog import TOOLS
from tools.assay.composition.settings import AssaySettings
from tools.assay.composition.store import ArtifactScope, prune_python_artifacts
from tools.assay.core.aspect import CHECKED_LAYER, compose, Layer, logged, RING, traced
from tools.assay.core.exec import EngineExecutor, Executor
from tools.assay.core.govern import measure, RESOURCE
from tools.assay.core.model import (
    Artifact,
    ArtifactKind,
    BaseParams,
    Bind,
    BridgeLifecycle,
    Claim,
    Completed,
    Counts,
    Diagnostic,
    Envelope,
    envelope,
    Fault,
    field_cap,
    HINT_CAP,
    Match,
    RailStatus,
    receipt,
    Report,
    RESULT_CAP,
    RunDelta,
    RunSnapshot,
    StaticRun,
    Step,
    validate_detail,
    VerifySummary,
    wire_encode,
    wire_safe,
)
from tools.assay.diagnostics import cap_note, fold
from tools.assay.rails import (
    api as api_rail,
    bridge as bridge_rail,
    code as code_rail,
    docs as docs_rail,
    health as health_rail,
    package as package_rail,
    provision as provision_rail,
    static as static_rail,
    test as test_rail,
)
from tools.assay.rails.api import ApiParams
from tools.assay.rails.bridge import BridgeParams
from tools.assay.rails.code import CodeParams
from tools.assay.rails.docs import DocsParams, FaultedPromotion
from tools.assay.rails.package import PackageParams
from tools.assay.rails.provision import ProvisionParams
from tools.assay.rails.static import StaticParams
from tools.assay.rails.test import TestParams


if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping


# --- [TYPES] ----------------------------------------------------------------------------

# `P` is a verb-params type, not a ParamSpec.
type Handler[P] = Callable[[AssaySettings, ArtifactScope, P, Executor], Result[Report, Fault]]
type ReportLayer = Layer[[AssaySettings, ArtifactScope, object, Executor], Report]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACT_CAP: Final = 100
# Severities that name a real defect: the truthful fault count and the defect-preserving display cap both key on them.
_DEFECT_SEVERITIES: Final[frozenset[str]] = frozenset(("error", "failed"))
# Mirrors the msgspec-enforced cap on Fault.message so history round-trips without silent truncation.
_MESSAGE_CAP: Final[int] = field_cap(Fault, "message", default=1 << 62)
_DISPATCH_NONE: Final = f"{Step.DISPATCH}=none"
# Per-invocation counter; long-lived automation processes must not trip the one-Envelope guard on reuse.
_WRITES: ContextVar[Iterator[int]] = ContextVar("assay_writes")
_PYPROJECT: Final[Path] = Path(__file__).resolve().parents[3] / "pyproject.toml"
# --- [MODELS] ---------------------------------------------------------------------------


# --- [TABLES] ---------------------------------------------------------------------------

_ENVELOPE_DECODER: Final[msgspec.json.Decoder[Envelope]] = msgspec.json.Decoder(Envelope)

# --- [SERVICES] -------------------------------------------------------------------------

_LOG: Final = structlog.get_logger("assay.registry")
# The one production wiring site: rails receive this bound port unless a caller injects its own through rail()/build_app().
_EXECUTOR: Final[Executor] = EngineExecutor()

# --- [OPERATIONS] -----------------------------------------------------------------------


def _identity_hom(*_a: object, **_k: object) -> Result[Report, Fault]:
    return Ok(fold(Claim.STATIC, "self-test", ()))


def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object, _executor: Executor) -> Mapping[str, object]:
    return {"run_id": settings.run_id, "strict": getattr(params, "strict", False), **settings.agent_context}


def _seed_parse_ring(dispatch: str, tokens: tuple[str, ...]) -> None:
    cmd = " ".join(tokens)[:HINT_CAP]
    match RING.get():
        case deque() as ring:
            ring.extend((f"{Step.DISPATCH}={dispatch}", cmd))
        case None:
            RING.set(deque((f"{Step.DISPATCH}={dispatch}", cmd), maxlen=16))


def _bound(params: object, claim: Claim, verb: str) -> Result[object, Fault]:
    # BaseParams.bound owns arity; surplus tokens feed the parse ring for hint distillation.
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
    ring = events if events is not None else tuple(RING.get() or ())
    step = step if step is not None else _failing_step(fault)
    reason = fault.message.removeprefix(f"{step}: ") or (ring[-1] if ring else "")
    framing = f"{step}: after {duration_ms:.1f}ms"
    # Reserve the separator byte so the final hint cannot exceed HINT_CAP.
    budgeted = reason[: max(HINT_CAP - len(framing) - 1, 0)]
    hint = f"{step}: {budgeted} after {duration_ms:.1f}ms"
    truncated = len(reason) > len(budgeted) or len(fault.message) >= _MESSAGE_CAP or fault.message.endswith("…")
    # One-hop into private engine.measure is intentional: engine owns the Measurements/to_resources shape — a co-owned
    # seam beside the RESOURCE reach. The fallback carries proc.children/proc.children_rss_bytes for the streaming receipt keys.
    snap = resource or RESOURCE.get() or measure().to_resources()
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


def _defect_preserving_cap(rows: tuple[Match, ...]) -> tuple[Match, ...]:
    # The blind slice clips real error/failed rows when generated diagnostics are numerous; reserving every defect row
    # first, then filling the remaining budget with non-defects in original order, keeps the cap from hiding a fault while
    # preserving the source-first/defects-last contract _result_rows pins.
    defects = sum(1 for row in rows if row.severity in _DEFECT_SEVERITIES)
    budget = max(RESULT_CAP - defects, 0)
    kept: list[Match] = []
    spent = 0
    for row in rows:
        if row.severity in _DEFECT_SEVERITIES:
            kept.append(row)
        elif spent < budget:
            kept.append(row)
            spent += 1
    return tuple(kept)


def _ok_envelope(bind: Bind, settings: AssaySettings, ms: float, report: Report) -> Envelope:
    # FAILED reports carry a Diagnostic built from defect result rows, matching the fault-rail shape.
    # _cap_note owns the N-of-M grammar; the first cap tripped owns the (shown, total) pair.
    # defect_rows reads the full pre-cap results so the fault count stays truthful even when the display slice drops rows.
    defect_rows = tuple(m for m in report.results if m.severity in _DEFECT_SEVERITIES)
    truncated = len(report.results) > RESULT_CAP or len(report.artifacts) > _ARTIFACT_CAP
    if truncated:
        artifact = _full_report_artifact(settings, bind, report)
        artifacts = ((*artifact, *report.artifacts) if artifact else report.artifacts)[:_ARTIFACT_CAP]
        cap, total = (RESULT_CAP, len(report.results)) if len(report.results) > RESULT_CAP else (_ARTIFACT_CAP, len(report.artifacts))
        report = msgspec.structs.replace(
            report,
            results=_defect_preserving_cap(report.results),
            artifacts=artifacts,
            notes=(*report.notes, *cap_note(cap, total, cap, tail=f"full report artifact under {settings.run_id}")),
        )
    defect_events = tuple(f"{m.id}: {m.text[:120]}" for m in defect_rows[:16])
    ctx = (
        _distill(
            Fault((), RailStatus.FAILED, f"{len(defect_rows)} diagnostic(s) failed"),
            ms,
            events=defect_events,
            resource=report.detail.resources if isinstance(report.detail, StaticRun) else (),
            step=Step.DEFECTS,
        )[0]
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
        exec=report.exec,
        truncated=truncated,
        notes=report.notes,
    )


def _narrow(handler: object) -> Handler[object]:
    # FunctionType preserves beartype's annotation contract without a cast.
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
    # The registry owns strict:/validation:/config: prefixes; rails must not emit them.
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
            # Faults carry no Report, hence no ExecReceipt: a remote timeout/spawn-fault is interrupted before the receipt is stamped.
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


def rail(bind: Bind, settings: AssaySettings | None = None, executor: Executor | None = None) -> Callable[[object], Envelope]:
    """Build the registry runner for one bound verb.

    Each invocation emits exactly one Envelope. Parse failures skip history persistence;
    context variables are invocation-scoped; artifact-scope open failures fold to a
    FAULTED Envelope instead of escaping.

    Args:
        bind: Registry row carrying the claim, verb, handler, and params type.
        settings: Resolved settings; constructed from the environment when absent.
        executor: Execution port threaded into the handler; the engine-bound port when absent.

    Returns:
        Runner that accepts a params object and returns the emitted Envelope.
    """
    handler: Handler[object] = compose(*_RAIL_LAYERS)(_narrow(bind.handler))
    port = executor if executor is not None else _EXECUTOR

    def run(params: object) -> Envelope:
        active = settings or AssaySettings()
        started = time.perf_counter()
        ring_token, writes_token, resource_token = RING.set(deque(maxlen=16)), _WRITES.set(count()), RESOURCE.set(())
        try:
            # ArtifactScope.open raises OSError when the .artifacts root is unwritable; fold it here outside _guard.
            scope = ArtifactScope.open(active, bind.claim)
            outcome = _guard(lambda: _bound(params, bind.claim, bind.verb).bind(lambda p: _validated(_strict(handler(active, scope, p, port), p))))
            return _emit(bind, active, started, outcome)
        except OSError as exc:
            return _emit(bind, active, started, Error(Fault((), RailStatus.FAULTED, f"scope: {exc}")))
        finally:
            RESOURCE.reset(resource_token)
            _WRITES.reset(writes_token)
            RING.reset(ring_token)

    run.__name__ = bind.verb
    return run


def _encode(envelope: Envelope) -> bytes:
    # Scrub lone surrogates from untrusted argv/paths so the one-envelope contract survives encoding failure.
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
        store.retain_builds(settings.build_scope_retention)
    except OSError as exc:
        _LOG.warning("history.persist_failed", run_id=settings.run_id, error=str(exc)[:200])
    # Heavy-lane benchmark autosaves live outside the assay store boundary; bound them by the same retention setting.
    prune_python_artifacts(Path(str(settings.root)), settings.artifact_retention)


def _emit_envelope(settings: AssaySettings, envelope: Envelope, *, persist: bool) -> Envelope:
    # Per-envelope flush preserves newline framing under long-lived automation, where each fire reuses this writer
    # and the default block-buffered stdout would otherwise coalesce or stall rows across fires.
    sys.stdout.buffer.write(_encode(envelope) + b"\n")
    sys.stdout.buffer.flush()
    if persist:
        _persist(settings, envelope)
    return envelope


def _prior(run_ids: tuple[str, ...], run_id: str) -> str:
    earlier = tuple(r for r in run_ids if r < run_id)
    return earlier[-1] if earlier else ""


_DRIFT_KEYS: Final[tuple[str, ...]] = ("rhinoVersion", "mcp.platform.version", "mcp.listener", "rpc.streamjsonrpc")


def _host_facts(report: Report | None) -> dict[str, str]:
    # Cross-session host fingerprint: bridge lifecycle host/capability rows or verify facts; absent for non-bridge runs.
    match report.detail if report is not None else None:
        case BridgeLifecycle(host=host, capabilities=caps):
            return {**dict(host), **{key: f"{outcome}: {receipt}" for key, outcome, receipt in caps}}
        case VerifySummary(facts=facts):
            return dict(facts)
        case _:
            return {}


def _delta_report(before_id: str, after_id: str, before: Envelope | None, after: Envelope | None) -> Report:
    # Delta + host-drift projection over both endpoints: compares status, counts, and `(id, line)` result keys; a missing side folds to EMPTY.
    def snapshot(run_id: str, env: Envelope) -> tuple[RunSnapshot, frozenset[tuple[str, int]], dict[str, str]]:
        report = env.report
        counts = report.counts if report is not None else Counts()
        keys = frozenset((m.id, m.line) for m in (report.results if report is not None else ()))
        return RunSnapshot(id=run_id, status=env.status, counts=counts), keys, _host_facts(report)

    match (before, after):
        case (Envelope() as b, Envelope() as a):
            (before_snap, before_keys, bf), (after_snap, after_keys, af) = snapshot(before_id, b), snapshot(after_id, a)
            drift = tuple((k, bf.get(k, ""), af.get(k, "")) for k in _DRIFT_KEYS if bf.get(k, "") != af.get(k, ""))
            detail = RunDelta(
                before=before_snap, after=after_snap, added=len(after_keys - before_keys), removed=len(before_keys - after_keys), drift=drift
            )
            # No note: RunDelta already owns status, counts, added, removed, and cross-session host-fact drift.
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
        fault_step = step
    except ValidationError as config_error:
        settings = AssaySettings.model_construct()
        fault_message = f"{Step.CONFIG}: {_validation_message(config_error)}"
        fault_step = Step.CONFIG
    claim, verb, dispatch = _parse_dispatch(tokens)
    fault = Fault(tuple(wire_safe(token) for token in tokens), RailStatus.FAULTED, fault_message[:_MESSAGE_CAP])
    _seed_parse_ring(dispatch, tokens)
    diagnostic, truncated = _distill(fault, 0.0, step=fault_step)
    env = msgspec.structs.replace(envelope(fault, claim=claim, verb=verb, run_id=settings.run_id, error_context=diagnostic), truncated=truncated)
    return _emit_envelope(settings, env, persist=False)


def self_test(*, rhino: bool = False, executor: Annotated[Executor | None, Parameter(parse=False)] = None) -> Envelope:
    """Run the Assay composition preflight and emit a health Envelope.

    Verifies registry callability, claim coverage, rail-layer composition, catalog
    census, and tool/process health. Rhino mode also requires `yak` for PACKAGE.

    Args:
        rhino: When True, fail if the Rhino packaging tool (yak) is not on PATH.
        executor: Execution port for the health-probe fan; the engine-bound port when absent.

    Returns:
        Emitted preflight Envelope with OK or FAILED status and health-probe notes.
    """
    settings = AssaySettings()
    bound_claims = frozenset(b.claim for b in REGISTRY)
    health_probes, health_notes = health_rail.probes(settings, executor if executor is not None else _EXECUTOR)
    yak = health_rail.yak_ready()
    # Claim enum coverage makes the check fail when a declared claim loses its last verb.
    healthy = all(callable(b.handler) for b in REGISTRY) and all(c in bound_claims for c in Claim) and _composes() and health_rail.census()
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
# PEP 696: the aspect-owned checked layer's free vars do not unify with ReportLayer; same seam as engine._spawn's compose.
_CHECKED: ReportLayer = CHECKED_LAYER  # ty: ignore[invalid-assignment]
_RAIL_LAYERS: Final[tuple[ReportLayer, ...]] = (_CHECKED, logged(event="rail", keys=_correlate), traced(span="assay.rail", attrs=_correlate))

REGISTRY: Final[tuple[Bind, ...]] = (
    Bind(Claim.STATIC, "static", static_rail.run, StaticParams, "Polyglot quality: diagnose + build per language; --fix runs fixers first."),
    Bind(Claim.CODE, "search", code_rail.search, CodeParams, "Search: $-metavar -> ast-grep structural; literal -> ripgrep content."),
    Bind(Claim.CODE, "query", code_rail.query, CodeParams, "AST query via tree-sitter (in-process)."),
    Bind(Claim.TEST, "run", test_rail.run, TestParams, "Unit + coverage + mutation fold."),
    Bind(Claim.TEST, "list", test_rail.list, TestParams, "Bounded discovery JSON."),
    Bind(Claim.TEST, "coverage", test_rail.coverage, TestParams, "Coverlet json + cobertura."),
    Bind(Claim.BRIDGE, "build", bridge_rail.build, BridgeParams, "Compile rasm-bridge plugin."),
    Bind(Claim.BRIDGE, "verify", bridge_rail.verify, BridgeParams, "Live RhinoWIP scenario fold."),
    Bind(Claim.BRIDGE, "status", bridge_rail.status, BridgeParams, "Bridge host health."),
    Bind(Claim.BRIDGE, "quit", bridge_rail.quit, BridgeParams, "Clean Cocoa terminate."),
    Bind(Claim.PACKAGE, "publish", package_rail.publish, PackageParams, "Yak stage + install + push under lease."),
    Bind(Claim.PACKAGE, "plan", package_rail.plan, PackageParams, "Publish dry-run: package metadata into notes."),
    Bind(Claim.PACKAGE, "list", package_rail.list, PackageParams, "Package project roster: slug/csproj pairs."),
    Bind(Claim.API, "resolve", api_rail.resolve, ApiParams, "Asset path resolution."),
    Bind(Claim.API, "query", api_rail.query, ApiParams, "Polymorphic ilspy surface; fingerprint cache."),
    Bind(Claim.API, "show", api_rail.show, ApiParams, "Artifact preview."),
    Bind(Claim.API, "status", api_rail.status, ApiParams, "Host/NuGet/tool health; --strict -> FAULTED."),
    Bind(Claim.DOCS, "check", docs_rail.check, DocsParams, "Markdown prose gate + Mermaid validation."),
    Bind(Claim.PROVISION, "up", provision_rail.up, ProvisionParams, "Start enabled Forge-owned provisioning services."),
    Bind(Claim.PROVISION, "down", provision_rail.down, ProvisionParams, "Stop labelled provisioning services while preserving owned volumes."),
    Bind(Claim.PROVISION, "status", provision_rail.status, ProvisionParams, "Show local provisioning service status."),
    Bind(Claim.PROVISION, "doctor", provision_rail.doctor, ProvisionParams, "Diagnose local Docker, Colima, paths, and ports."),
    Bind(Claim.PROVISION, "ports", provision_rail.ports, ProvisionParams, "Show configured provisioning ports and current listeners."),
    Bind(Claim.PROVISION, "inventory", provision_rail.inventory, ProvisionParams, "Show owned provisioning resources and Docker diagnostics."),
    Bind(Claim.PROVISION, "extensions", provision_rail.extensions, ProvisionParams, "Show provisioning extension targets."),
    Bind(Claim.PROVISION, "plan", provision_rail.plan, ProvisionParams, "Render provisioning compose plan without writing assets."),
    Bind(Claim.PROVISION, "env", provision_rail.env, ProvisionParams, "Report redacted provisioning connection metadata."),
    Bind(Claim.PROVISION, "check", provision_rail.check, ProvisionParams, "Read provisioning evidence and local runtime probes."),
    Bind(Claim.PROVISION, "apply", provision_rail.apply, ProvisionParams, "Create admitted provisioning extensions."),
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


def _leaf(bind: Bind, executor: Executor) -> Callable[[object], Envelope]:
    # The params default must stay visible to Cyclopts; __wrapped__ would expose the defaultless runner.
    runner = rail(bind, executor=executor)

    def command(params: object = bind.params()) -> Envelope:
        return runner(params)

    command.__name__ = bind.verb
    command.__qualname__ = f"_leaf.{bind.verb}"
    command.__doc__ = bind.help
    command.__annotate__ = lambda _format: {"params": bind.params, "return": Envelope}
    return command


def _usage(bind: Bind, *, root_leaf: bool = False) -> str:
    # Slot grammar is params-owned data: the verb row wins, the "" row is the claim default. Root leaves omit the verb token.
    slots_map: dict[str, str] = getattr(bind.params, "SLOTS", {})
    slots = slots_map.get(bind.verb, slots_map.get("", ""))
    verb = "" if root_leaf else bind.verb
    return " ".join(part for part in ("Usage: assay", bind.claim.value, verb, slots, "[OPTIONS]") if part)


def _register[**P](
    app: App,
    obj: App | Callable[P, object],
    *,
    name: str | None = None,
    help: str = "",  # noqa: A002  # cyclopts names this kwarg "help"; intentional shadow
    usage: str | None = None,
) -> App:
    # App.command returns the registered object (the leaf command or sub-app), never the parent; returning app keeps reduce folds linear.
    extras: dict[str, str] = {key: value for key, value in (("help", help), ("usage", usage)) if value}
    match name:
        case None:
            app.command(obj)
        case verb:
            app.command(obj, name=verb, **extras)
    return app


def build_app(registry: tuple[Bind, ...], *, executor: Executor | None = None) -> App:
    """Build the Cyclopts command tree from registry rows.

    Every claim registers as a sub-app whose leaves are its verbs; a claim collapses
    to a root leaf only when its lone verb duplicates the claim token. `self_test`
    and `delta` stay outside the claim fold.

    Args:
        registry: Ordered Bind tuple; typically the module-level REGISTRY constant.
        executor: Execution port threaded into every leaf runner; the engine-bound port when absent.

    Returns:
        Configured Cyclopts App with claim sub-apps, verb leaves, self-test, and
        delta registered.
    """
    port = executor if executor is not None else _EXECUTOR
    root = App(
        name="assay",
        help="Rasm polyglot quality operator. Global: --exec <local|ssh://[user@]host[:port]> offloads execution (default local).",
        version=_VERSION,
        # Disable global --version so verb params can own the token and stdout remains envelope-only.
        version_flags=(),
        default_parameter=Parameter(show_default=True),
        result_action="return_value",
        backend="asyncio",
        exit_on_error=False,
        print_error=False,
        help_on_error=False,
    )
    keyed = sorted(registry, key=lambda b: b.claim.value)
    groups = tuple((claim, tuple(rows)) for claim, rows in groupby(keyed, key=attrgetter("claim")))
    app = reduce(lambda acc, group: _register_claim(acc, group, port), groups, root)
    _register(app, self_test, name="self-test")
    _register(app, delta, name="delta")
    return app


def _register_claim(app: App, group: tuple[Claim, tuple[Bind, ...]], executor: Executor) -> App:
    # A claim collapses to a root leaf only when its lone verb duplicates the claim token; every other claim
    # takes the explicit verb token, so the surface, help roster, and Envelope verb agree across the registry.
    claim, rows = group
    match rows:
        case (single,) if single.verb == claim.value:
            return _register(app, _leaf(single, executor), name=claim.value, help=single.help, usage=_usage(single, root_leaf=True))
        case _:
            sub = reduce(
                lambda sub_app, row: _register(sub_app, _leaf(row, executor), name=row.verb, help=row.help, usage=_usage(row)),
                rows,
                App(name=claim.value, version_flags=(), usage=f"Usage: assay {claim.value} COMMAND"),
            )
            return _register(app, sub)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REGISTRY", "Handler", "build_app", "delta", "parse_fault", "rail", "self_test"]
