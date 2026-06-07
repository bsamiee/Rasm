"""Compose Assay registry binds, runners, Envelopes, and history commands."""

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
from pydantic import ValidationError
import structlog

from tools.assay.composition.catalog import select, TOOLS
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.aspect import _RING, checked_call, compose, Layer, logged, Slot, traced  # noqa: PLC2701
from tools.assay.core.engine import _RESOURCE, _snapshot, fan_out  # noqa: PLC2701
from tools.assay.core.model import (
    _HINT_CAP,  # noqa: PLC2701  # intra-package private import: shared model cap, canonical clip site
    _RESULT_CAP,  # noqa: PLC2701  # intra-package private import: shared model cap, canonical saturation site
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
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
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
from tools.assay.rails.code import CodeParams
from tools.assay.rails.docs import DocsParams, FaultedPromotion
from tools.assay.rails.package import PackageParams
from tools.assay.rails.static import StaticParams
from tools.assay.rails.test import TestParams


if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

    from tools.assay.composition.settings import ArtifactStore


# --- [TYPES] ----------------------------------------------------------------------------

# Per-verb adapter; `P` is the verb params type, not a ParamSpec.
type Handler[P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]
type ReportLayer = Layer[[AssaySettings, ArtifactScope, object], Report]


# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACT_CAP: Final = 100
# field_cap keeps parse messages and hints within the same bounds msgspec enforces when history is read back.
_MESSAGE_CAP: Final[int] = field_cap(Fault, "message", default=1 << 62)
_DISPATCH_NONE: Final = "dispatch=none"
# Seeded per invocation so long-lived automation fires do not trip the one-Envelope guard after the first run.
_WRITES: ContextVar[Iterator[int]] = ContextVar("assay_writes")
_LOG: Final = structlog.get_logger("assay.registry")
_PROBE_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
_PROBE_TIMEOUT: Final = 8.0
# Warm self-test reuses --version probes within the TTL; an upgraded tool changes its (program, mtime, lockfile) token and misses.
_PROBE_TTL: Final = 900.0
_PROBE_CACHE_FILE: Final = "probe-cache.json"
_PROBE_CACHE_KEY: Final = "probe:%s"
# Launcher-prefix -> lockfile that pins the launched program; ordered longest-prefix-first so the matcher resolves MODULE
# before UV on their shared `uv run` head. Tokens stat the launched program (argv slot after the prefix) and fold the
# lockfile mtime, so an in-place lockfile-driven upgrade (ruff/ty/ast-grep under uv.lock, JS tools under pnpm-lock) flips
# the token even though the uv/pnpm shim mtime is fixed. DIRECT (yak) and DOTNET (the probed SDK) carry no prefix entry:
# argv[0] IS the tool, so they keep the launcher token with no lockfile fold.
_PROBE_LOCKED: Final[tuple[tuple[tuple[str, ...], str], ...]] = (
    (Runner.MODULE.prefix, "uv.lock"),
    (Runner.UV.prefix, "uv.lock"),
    (Runner.PNPM.prefix, "pnpm-lock.yaml"),
)
_ENVELOPE_DECODER: Final[msgspec.json.Decoder[Envelope]] = msgspec.json.Decoder(Envelope)
_PYPROJECT: Final[Path] = Path(__file__).resolve().parents[3] / "pyproject.toml"


# --- [OPERATIONS] -----------------------------------------------------------------------


def _identity_hom(*_a: object, **_k: object) -> Result[Report, Fault]:
    return Ok(fold(Claim.STATIC, "self-test", ()))


def _checked_report(fn: Handler[object]) -> Handler[object]:
    return checked_call(fn)


def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object) -> Mapping[str, object]:
    return {"run_id": settings.run_id, "strict": getattr(params, "strict", False), **settings.agent_context}


def _bound(params: object, claim: Claim, verb: str) -> Result[object, Fault]:
    # BaseParams.bound owns positional arity for every verb; surplus tokens fold into the parse taxonomy.
    # The surplus path seeds the same dispatch ring shape as parse_fault for one Diagnostic format.
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


def _seed_parse_ring(dispatch: str, tokens: tuple[str, ...]) -> None:
    # Seed dispatch and reconstructed command line for parse diagnostics.
    cmd = " ".join(tokens)[:_HINT_CAP]
    match _RING.get():
        case deque() as ring:
            ring.extend((f"dispatch={dispatch}", cmd))
        case None:
            _RING.set(deque((f"dispatch={dispatch}", cmd), maxlen=16))


def _strict(outcome: Result[Report, Fault], params: object) -> Result[Report, Fault]:
    # Under --strict, only no-op folds promote; defect and infrastructure statuses already dominate.
    match outcome:
        case Result(tag="ok", ok=report) if getattr(params, "strict", False) and report.status in {RailStatus.EMPTY, RailStatus.SKIP}:
            return Error(Fault((), RailStatus.FAULTED, "strict: empty/skipped fold"))
        case _:
            return outcome


def _distill(
    fault: Fault, duration_ms: float, *, events: tuple[str, ...] | None = None, resource: tuple[tuple[str, float], ...] = (), step: str | None = None
) -> tuple[Diagnostic, bool]:
    # Distill Faults and FAILED defect rows into one Diagnostic shape.
    # Resource snapshots are lazy on the error path, and every clipped parse byte sets Envelope.truncated.
    ring = events if events is not None else tuple(_RING.get() or ())
    step = step if step is not None else _failing_step(fault)
    reason = fault.message.removeprefix(f"{step}: ") or (ring[-1] if ring else "")
    framing = f"{step}: after {duration_ms:.1f}ms"
    # Reserve one byte for the separator space the hint inserts between `budgeted` and `after`, so len(hint) <= _HINT_CAP.
    budgeted = reason[: max(_HINT_CAP - len(framing) - 1, 0)]
    hint = f"{step}: {budgeted} after {duration_ms:.1f}ms"
    truncated = len(reason) > len(budgeted) or len(fault.message) >= _MESSAGE_CAP or fault.message.endswith("…")
    dispatched = not (ring and ring[0] == _DISPATCH_NONE)
    snap = resource or _RESOURCE.get() or tuple(_snapshot().items())
    return Diagnostic(failing_step=step, recent_events=ring, elapsed_ms=duration_ms, hint=hint, dispatched=dispatched, resource=snap), truncated


def _failing_step(fault: Fault) -> str:
    # Name the stage from status, argv, and canonical synthetic-fault prefixes.
    match (fault.status, fault.argv):
        case (RailStatus.TIMEOUT, _):
            return "timeout"
        case (RailStatus.BUSY, _):
            return "lease_busy"
        case (_, ()):
            return next((step for step in ("strict", "validation", "config", "dispatch", "parse") if fault.message.startswith(f"{step}:")), "spawn")
        case _:
            return "spawn"


def _ok_envelope(bind: Bind, settings: AssaySettings, ms: float, report: Report) -> Envelope:
    # FAILED reports get the same Diagnostic shape as Fault rails, sourced from defect rows.
    truncated = len(report.results) > _RESULT_CAP or len(report.artifacts) > _ARTIFACT_CAP
    if truncated:
        artifact = _full_report_artifact(settings, bind, report)
        artifacts = ((*artifact, *report.artifacts) if artifact else report.artifacts)[:_ARTIFACT_CAP]
        report = msgspec.structs.replace(report, results=report.results[:_RESULT_CAP], artifacts=artifacts)
        sys.stderr.write(f"assay: {bind.claim.value} {bind.verb} output truncated; full report artifact under {settings.run_id}\n")
    failed = tuple(m for m in report.results if m.severity == "failed")
    ctx = (
        _distill(
            Fault((), RailStatus.FAILED, f"{len(failed)} tool(s) failed"), ms, events=tuple(f"{m.id}: {m.text[:120]}" for m in failed), step="defects"
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
        truncated=truncated,
        notes=report.notes,
    )


def _full_report_artifact(settings: AssaySettings, bind: Bind, report: Report) -> tuple[Artifact, ...]:
    name = f"{bind.claim.value}-{bind.verb}.full-report.json"
    try:
        path, size = settings.store().write_full_report(settings.run_id, name, report)
    except OSError as exc:
        _LOG.warning("history.full_report_failed", run_id=settings.run_id, error=str(exc)[:200])
        return ()
    return (Artifact(id="full-report", kind=ArtifactKind.HISTORY, path=path, bytes=size, lines=0),)


def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    # Fold the rail Result into one Envelope and enforce the per-invocation one-write guard.
    # Parse faults are typo-class events, so they do not enter diffable run history.
    ms = (time.perf_counter() - started) * 1000.0
    persist = True
    match outcome:
        case Result(tag="ok", ok=report):
            envelope = _ok_envelope(bind, settings, ms, report)
        case Result(error=fault):
            diagnostic, truncated = _distill(fault, ms)
            persist = diagnostic.failing_step != "parse"
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

    Returns:
        Callable that executes the bound verb and emits its Envelope.
    """
    handler: Handler[object] = compose(*_RAIL_LAYERS)(_narrow(bind.handler))

    def run(params: object) -> Envelope:
        active = settings or AssaySettings()
        started = time.perf_counter()
        # Ring and write count are invocation-scoped so automation can reuse rail() across fires.
        # Resource snapshots stay lazy and only run on Fault or defect paths.
        ring_token, writes_token, resource_token = _RING.set(deque(maxlen=16)), _WRITES.set(count()), _RESOURCE.set(())
        try:
            # An unwritable .artifacts root makes ArtifactScope.open escape _guard; fold that OSError into one FAULTED Envelope here.
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


def _narrow(handler: object) -> Handler[object]:
    # Narrow erased Bind.handler without cast; rail modules import return annotation types for beartype.
    match handler:
        case FunctionType() as fn:
            return fn
        case _:
            raise TypeError(f"Bind.handler must be a module-level def (FunctionType), got {type(handler).__name__}")


def _validated(outcome: Result[Report, Fault]) -> Result[Report, Fault]:
    # Validate success Report.detail against the tagged-union wire without rewriting the outcome.
    match outcome:
        case Result(tag="ok", ok=report):
            validate_detail(report.detail)
            return outcome
        case _:
            return outcome


def _guard(thunk: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:
    # The composition root owns synthetic strict:/validation: fault prefixes.
    try:
        return thunk()
    except FaultedPromotion as promoted:
        return Error(Fault((), RailStatus.FAULTED, f"strict: {promoted}"))
    except BeartypeCallHintViolation as violation:
        return Error(Fault((), RailStatus.FAULTED, f"validation: {violation}"))
    except msgspec.MsgspecError as malformed:
        return Error(Fault((), RailStatus.FAULTED, f"validation: {malformed}"))


# --- [COMPOSITION] ----------------------------------------------------------------------

# compose sorts layer slots; rails use checked/logged/traced without spawn retry.
_RAIL_LAYERS: Final[tuple[ReportLayer, ...]] = (
    (Slot.checked, _checked_report),
    logged(event="rail", keys=_correlate),
    traced(span="assay.rail", attrs=_correlate),
)

REGISTRY: Final[tuple[Bind, ...]] = (
    Bind(Claim.STATIC, "fix", static_rail.fix, StaticParams, "Format, style, analyzer autofix."),
    Bind(Claim.STATIC, "report", static_rail.report, StaticParams, "Non-mutating diagnostics."),
    Bind(Claim.STATIC, "build", static_rail.build, StaticParams, "Closure-leased restore + build + analyzers."),
    Bind(Claim.STATIC, "full", static_rail.full, StaticParams, "Workspace.slnx parity; Debug+Release."),
    Bind(Claim.STATIC, "plan", static_rail.plan, StaticParams, "Owners, triggers, closure into notes."),
    Bind(Claim.CODE, "search", code_rail.search, CodeParams, "Search: $-metavar -> ast-grep structural; literal -> ripgrep content."),
    Bind(Claim.CODE, "rewrite", code_rail.rewrite, CodeParams, "Structural rewrite preview; --apply writes under lease."),
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
    Bind(Claim.PACKAGE, "list", package_rail.list, PackageParams, "Staged + published manifests."),
    Bind(Claim.PACKAGE, "plan", package_rail.plan, PackageParams, "Stage plan into notes."),
    Bind(Claim.API, "doctor", api_rail.doctor, ApiParams, "Host/NuGet/tool health; --strict -> FAULTED."),
    Bind(Claim.API, "resolve", api_rail.resolve, ApiParams, "Asset path resolution."),
    Bind(Claim.API, "query", api_rail.query, ApiParams, "Polymorphic ilspy surface; fingerprint cache."),
    Bind(Claim.API, "show", api_rail.show, ApiParams, "Artifact preview."),
    Bind(Claim.DOCS, "check", docs_rail.check, DocsParams, "Markdown + Mermaid validation."),
)


def wire_safe(text: str) -> str:
    """Replace lone surrogates (PEP 383 surrogateescape from invalid-UTF-8 argv) with U+FFFD.

    Returns:
        A string msgspec can UTF-8 encode for the wire.
    """
    return text.encode("utf-8", "replace").decode("utf-8")


def _encode(envelope: Envelope) -> bytes:
    # The single wire encoder. Untrusted argv/paths can carry lone surrogates msgspec cannot UTF-8 encode
    # (msgspec.to_builtins also rejects them), so that boundary failure folds to a scrubbed minimal FAULTED
    # Envelope, holding the one-Envelope contract. Argv is scrubbed upstream in __main__ so this is the rare path.
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


def _emit_envelope(settings: AssaySettings, envelope: Envelope, *, persist: bool = True) -> Envelope:
    # Single stdout writer; optional history persistence happens after the wire line.
    sys.stdout.buffer.write(_encode(envelope) + b"\n")
    persist and _persist(settings, envelope)  # type: ignore[func-returns-value]  # intentional: _persist returns None; short-circuit is the gate, not a value use
    return envelope


def _persist(settings: AssaySettings, envelope: Envelope) -> None:
    # History writes are best-effort and use the same bytes delta decodes later.
    try:
        store = settings.store()
        store.write_history(settings.run_id, _encode(envelope))
        store.retain_history(settings.artifact_retention)
    except OSError as exc:
        _LOG.warning("history.persist_failed", run_id=settings.run_id, error=str(exc)[:200])


def _retain(store: ArtifactStore, keep: int) -> None:
    store.retain_history(keep)


def _run_ids(store: ArtifactStore) -> tuple[str, ...]:
    return store.history_run_ids()


def _prior(run_ids: tuple[str, ...], run_id: str) -> str:
    earlier = tuple(r for r in run_ids if r < run_id)
    return earlier[-1] if earlier else ""


def _load_run(store: ArtifactStore, run_id: str) -> Envelope | None:
    return store.load_history(run_id)


def _restore_full_report(store: ArtifactStore, env: Envelope) -> Envelope:
    return store.restore_full_report(env)


def _delta_report(before_id: str, after_id: str, before: Envelope | None, after: Envelope | None) -> Report:
    # Compare persisted reports by status, counts, and `(id, line)` result keys; missing sides fold to EMPTY.
    def snapshot(run_id: str, env: Envelope) -> tuple[RunSnapshot, frozenset[tuple[str, int]]]:
        report = env.report
        counts = report.counts if report is not None else Counts()
        keys = frozenset((m.id, m.line) for m in (report.results if report is not None else ()))
        return RunSnapshot(id=run_id, status=env.status, counts=counts), keys

    match (before, after):
        case (Envelope() as b, Envelope() as a):
            (before_snap, before_keys), (after_snap, after_keys) = snapshot(before_id, b), snapshot(after_id, a)
            detail = RunDelta(before=before_snap, after=after_snap, added=len(after_keys - before_keys), removed=len(before_keys - after_keys))
            note = f"delta {before_id} -> {after_id}: {b.status.value} -> {a.status.value}, +{detail.added}/-{detail.removed} results"
            return fold(Claim.STATIC, "delta", (Completed(("delta", after_id), 0, status=RailStatus.OK, notes=(note,)),), detail=detail)
        case _:
            missing = after_id if after is None else before_id
            note = f"delta: run not found in history: {missing or '(no prior run)'}"
            return fold(Claim.STATIC, "delta", (Completed(("delta", after_id), 0, status=RailStatus.EMPTY, notes=(note,)),))


def delta(run_id: str, *, against: str = "") -> Envelope:
    """Diff two persisted run Envelopes.

    Returns:
        Emitted delta Envelope comparing the selected runs.
    """
    settings = AssaySettings()
    store = settings.store()
    before_id = against or _prior(_run_ids(store), run_id)
    report = _delta_report(before_id, run_id, _load_run(store, before_id), _load_run(store, run_id))
    env = msgspec.structs.replace(envelope(report, claim=Claim.STATIC, verb="delta", run_id=settings.run_id), notes=report.notes)
    return _emit_envelope(settings, env, persist=False)


def parse_fault(tokens: tuple[str, ...], message: str, *, step: str = "parse") -> Envelope:
    """Convert a Cyclopts parse failure into the canonical fault Envelope.

    Returns:
        Emitted parse-fault Envelope.
    """
    try:
        settings = AssaySettings()
        fault_message = f"{step}: {message}"
    except ValidationError as config_error:
        settings = AssaySettings.model_construct()
        fault_message = f"config: {_validation_message(config_error)}"
    claim, verb, dispatch = _parse_dispatch(tokens)
    fault = Fault((), RailStatus.FAULTED, fault_message[:_MESSAGE_CAP])
    _seed_parse_ring(dispatch, tokens)
    diagnostic, truncated = _distill(fault, 0.0)
    env = msgspec.structs.replace(envelope(fault, claim=claim, verb=verb, run_id=settings.run_id, error_context=diagnostic), truncated=truncated)
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


def self_test(*, rhino: bool = False) -> Envelope:
    """Run the Assay composition preflight.

    Returns:
        Emitted preflight Envelope.
    """
    settings = AssaySettings()
    claims = frozenset(b.claim for b in REGISTRY)
    health_probes, health_notes = _health(settings)
    # Health is structural soundness only: an absent host tool cannot be distinguished from a present-but-nonzero
    # --version via exit code (uv/dotnet/python -m all front absence as Completed(rc!=0)), so it surfaces as
    # UNSUPPORTED at use-time (engine._guarded FileNotFoundError arm), not as a census verdict.
    healthy = all(callable(b.handler) for b in REGISTRY) and all(any(b.claim is c for b in REGISTRY) for c in claims) and _composes() and _census()
    status = RailStatus.FAILED if (not healthy or (rhino and not _yak_ready())) else RailStatus.OK
    summary = f"rows={len(REGISTRY)} claims={len(claims)} tools={len(TOOLS)} healthy={healthy} rhino={'required' if rhino else 'skipped'}"
    report = fold(Claim.STATIC, "self-test", (receipt(("assay", "self-test"), 0 if status is RailStatus.OK else 1, status=status, notes=(summary,)),))
    # Census rows and health probes are machine data; tool/git probe text rides notes.
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


def _health(settings: AssaySettings) -> tuple[tuple[Match, ...], tuple[str, ...]]:
    # Probe distinct tool versions and git state; missing tools surface as notes, not rail faults.
    # Tool --version probes are cacheable by (resolved-path, mtime, TTL); git state is volatile and always live.
    probes = (*_tool_probes(), _GIT_HEAD, _GIT_DIRTY)
    volatile = frozenset(c.tool.command for c in (_GIT_HEAD, _GIT_DIRTY))
    cache = _probe_cache_load(settings)
    hits = {c.tool.command: hit for c in probes if c.tool.command not in volatile for hit in (_cache_hit(cache, c.tool.command),) if hit is not None}
    misses = tuple(c for c in probes if hits.get(c.tool.command) is None)
    results = fan_out(misses, settings=settings, scope=None, routed=_PROBE_ROUTED)
    fresh = dict(zip((c.tool.command for c in misses), map(_probe_note, misses, results, strict=True), strict=True))
    _probe_cache_store(settings, cache, fresh, volatile)
    noted = tuple(hits.get(c.tool.command) or fresh[c.tool.command] for c in probes)
    probe_matches = tuple(
        Match(id=check.tool.name, kind=ArtifactKind.PROCESS, text=note, severity="failed" if not ok else None)
        for check, (note, ok) in zip(probes, noted, strict=True)
    )
    notes = tuple(note for note, _ in noted)
    return probe_matches, notes


def _probe_token(argv: tuple[str, ...]) -> str | None:
    # Freshness follows the launched program plus lockfile mtime for venv/node launchers. DIRECT and DOTNET keep argv[0]
    # because the launcher is the probed tool/SDK. An unresolvable program has no token and forces a live probe.
    from pathlib import Path  # noqa: PLC0415  # stdlib: deferred import, only on the self-test probe-cache path
    import shutil  # noqa: PLC0415  # stdlib: deferred import, only on the self-test probe-cache path

    def mtime(path: str | None) -> int | None:
        try:
            return Path(path).stat().st_mtime_ns if path is not None else None
        except OSError:
            return None

    def lock_path(name: str) -> str | None:
        # Walk up from the venv interpreter dir (unresolved: the .venv shim, not its Nix-store symlink target) to the
        # nearest dir holding the lockfile; None if no ancestor carries it (then only the program mtime tokens).
        bases = Path(sys.executable).parents
        return next((str(base / name) for base in bases if (base / name).exists()), None)

    # Match argv head against known launcher prefixes; the launched program is the slot after the prefix. A MODULE program
    # is a dotted module name (no PATH executable), so its program signal collapses to the venv interpreter (sys.executable).
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


def _cache_hit(cache: Mapping[str, object], argv: tuple[str, ...]) -> tuple[str, bool] | None:
    # A hit requires a matching freshness token within TTL; any shape mismatch folds to a miss (live probe).
    match (cache.get(_PROBE_CACHE_KEY % "\x00".join(argv)), _probe_token(argv)):
        case ({"token": str() as stored, "ts": (int() | float()) as ts, "note": str() as note, "ok": bool() as ok}, str() as token) if (
            stored == token and 0 <= time.time() - ts < _PROBE_TTL
        ):
            return note, ok
        case _:
            return None


def _probe_cache_load(settings: AssaySettings) -> Mapping[str, object]:
    # Absent or malformed cache folds to empty so probes fall back to live process checks.
    try:
        store = settings.store()
        raw = store.read_bytes("cache", _PROBE_CACHE_FILE)
        match msgspec.json.decode(raw):
            case dict() as parsed:
                return parsed
            case _:
                return {}
    except OSError, msgspec.MsgspecError:
        return {}


def _probe_cache_store(
    settings: AssaySettings, prior: Mapping[str, object], fresh: Mapping[tuple[str, ...], tuple[str, bool]], volatile: frozenset[tuple[str, ...]]
) -> None:
    # Best-effort write-back: only token-resolvable, non-volatile (non-git) probes persist; failures are silent.
    entries = {
        _PROBE_CACHE_KEY % "\x00".join(argv): {"token": token, "ts": time.time(), "note": note, "ok": ok}
        for argv, (note, ok) in fresh.items()
        if argv not in volatile
        for token in (_probe_token(argv),)
        if token is not None
    }
    try:
        store = settings.store()
        store.write_bytes(wire_encode({**prior, **entries}), "cache", _PROBE_CACHE_FILE)
    except OSError as exc:
        _LOG.warning("probe_cache.store_failed", run_id=settings.run_id, error=str(exc)[:200])


def _probe(name: str, argv: tuple[str, ...]) -> Check:
    tool = Tool(name=name, runner=Runner.DIRECT, command=argv, input=Input.NONE, language=Language.PYTHON, claim=Claim.STATIC, timeout=_PROBE_TIMEOUT)
    return Check(tool=tool)


_GIT_HEAD: Final = _probe("git-head", ("git", "rev-parse", "--short", "HEAD"))
_GIT_DIRTY: Final = _probe("git-dirty", ("git", "status", "--porcelain"))


def _tool_probes() -> tuple[Check, ...]:
    # Deduplicate probes by launcher and program; DOTNET tools share one SDK probe and INPROC tools are skipped.
    def keyed(tool: Tool) -> tuple[str, tuple[str, ...]]:
        match tool.runner:
            case Runner.DOTNET:
                return "dotnet", ("dotnet", "--version")
            case _:
                return f"{tool.runner.value}:{tool.command[0]}", (*tool.runner.prefix, tool.command[0], "--version")

    deduped = dict(keyed(t) for t in TOOLS if t.runner is not Runner.INPROC)
    return tuple(_probe(argv[-2], argv) for argv in deduped.values())


def _probe_note(check: Check, result: Result[Completed, Fault]) -> tuple[str, bool]:
    # Project tool/git probe Results to notes; tool and git presence are informational and never fail structural health.
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
            # Exit-code cannot distinguish a present-but-nonzero --version (e.g. py-analyzer/squawk usage exit 2) from a
            # launcher-swallowed absent tool; reported informationally, never failing structural health.
            return f"tool {name}: present (exit {d.returncode})", True


def _census() -> bool:
    # Catch catalog rot: every row must select back to itself through the claim/language axes.
    return all(t in select(t.claim, t.language) for t in TOOLS)


def _yak_ready() -> bool:
    # Rhino preflight requires a direct `yak` on PATH; the DIRECT runner uses execvp PATH lookup at spawn,
    # so shutil.which agrees with runtime resolution where os.access(command[0]) would test the CWD-relative path.
    import shutil  # noqa: PLC0415  # stdlib: deferred import, only on --rhino path

    return any(
        t.runner is Runner.DIRECT and t.command and t.command[0] == "yak" and shutil.which(t.command[0]) is not None
        for t in TOOLS
        if t.claim is Claim.PACKAGE
    )


def _composes() -> bool:
    # Weaving identity catches Slot-order inversions without spawning a process.
    try:
        compose(*_RAIL_LAYERS)(_identity_hom)
    except TypeError:
        return False
    return True


def _leaf(bind: Bind) -> Callable[[object], Envelope]:
    # Cyclopts needs a concrete default plus PEP 649 lazy annotations to flatten each Params type.
    # Avoid functools.wraps because __wrapped__ exposes the defaultless rail runner to signature inspection.
    runner = rail(bind)

    def command(params: object = bind.params()) -> Envelope:
        return runner(params)

    command.__name__ = bind.verb
    command.__qualname__ = f"_leaf.{bind.verb}"
    command.__doc__ = bind.help
    command.__annotate__ = lambda _format: {"params": bind.params, "return": Envelope}
    return command


def build_app(registry: tuple[Bind, ...]) -> App:
    """Build the Cyclopts command tree from registry rows.

    Returns:
        Cyclopts application with claim and verb commands registered.
    """
    root = App(
        name="assay",
        help="Rasm polyglot quality operator.",
        version=_project_version(),
        default_parameter=Parameter(show_default=False),
        result_action="return_value",
        backend="asyncio",
        exit_on_error=False,
        print_error=False,
        help_on_error=False,
    )
    keyed = sorted(registry, key=lambda b: b.claim.value)
    subs = tuple(
        reduce(lambda app, row: _register(app, _leaf(row), name=row.verb, help=row.help), tuple(rows), App(name=claim.value))
        for claim, rows in groupby(keyed, key=attrgetter("claim"))
    )
    app = reduce(_register, subs, root)
    _register(app, self_test, name="self-test")
    _register(app, delta, name="delta")
    return app


def _project_version() -> str:
    try:
        project = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8")).get("project", {})
        version = project.get("version", "")
        return version if isinstance(version, str) and version else "0.0.0"
    except OSError, tomllib.TOMLDecodeError:
        return "0.0.0"


def _register[**P](app: App, obj: App | Callable[P, object], *, name: str | None = None, help: str = "") -> App:  # noqa: A002  # cyclopts command kwarg is named "help"; mirroring the CLI surface
    # App.command returns the registered object, so return the parent to keep registry folds linear.
    match (name, help):
        case (None, _):
            app.command(obj)
        case (verb, ""):
            app.command(obj, name=verb)
        case (verb, text):
            app.command(obj, name=verb, help=text)
    return app


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REGISTRY", "Handler", "build_app", "delta", "parse_fault", "rail", "self_test"]
