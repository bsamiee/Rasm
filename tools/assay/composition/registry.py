"""Composition root: one ``Bind`` table, one ``rail`` runner, one ``_emit_envelope`` stdout writer.

The runner stack is ``checked ▷ logged ▷ traced`` (no ``@retried`` — a rail is a ``Hom``, not a
``Spawn``), folded by ``Slot`` rank in ``compose`` so a ``Slot`` inversion is a decoration-time
``TypeError``. ``_emit_envelope`` is the SOLE stdout writer — ``_emit`` (rail), ``delta``, ``self_test``,
and ``parse_fault`` all route their one Envelope through it (``persist`` toggles the run-history side
write, which only the rail wire and root self-test claim); structlog lines and truncation notes ride stderr.
"""

from collections import deque
from collections.abc import Callable
from contextvars import ContextVar
from functools import reduce
from itertools import count, groupby
from operator import attrgetter
import sys
import time
from types import FunctionType
from typing import Final, TYPE_CHECKING

from cyclopts import App, Parameter
from expression import Error, Ok, Result
import msgspec
import structlog

from tools.assay.composition.catalog import select, TOOLS  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # intra-package import; tools.assay is the package root
from tools.assay.core.aspect import _RING, checked, compose, logged, traced  # noqa: PLC2701  # intra-package import; tools.assay is the package root
from tools.assay.core.engine import _RESOURCE, _snapshot, fan_out  # noqa: PLC2701  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    _HINT_CAP,  # noqa: PLC2701  # intra-package private import: sole definition in model.py, canonical clip site
    _RESULT_CAP,  # noqa: PLC2701  # intra-package private import: sole definition in model.py, canonical saturation site
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
)
from tools.assay.core.routing import Routed, Scope  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root
from tools.assay.rails import (  # intra-package import; tools.assay is the package root
    api as api_rail,
    bridge as bridge_rail,
    code as code_rail,
    docs as docs_rail,
    package as package_rail,
    static as static_rail,
    test as test_rail,
)
from tools.assay.rails.api import ApiParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.bridge import BridgeParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.code import CodeParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.docs import DocsParams, FaultedPromotion  # intra-package import; tools.assay is the package root
from tools.assay.rails.package import PackageParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.static import StaticParams  # intra-package import; tools.assay is the package root
from tools.assay.rails.test import TestParams  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

    from tools.assay.composition.settings import ArtifactStore  # intra-package import; tools.assay is the package root
    from tools.assay.core.aspect import Layer  # intra-package import; tools.assay is the package root


# --- [TYPES] ----------------------------------------------------------------------------

# The per-verb 3-arg adapter the registry binds. ``P`` is the verb's Params type — a single TypeVar,
# never a ParamSpec: the third positional is one value, not a call signature, so ``Callable[[S, A, P], R]`` holds.
type Handler[P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_ARTIFACT_CAP: Final = 100  # the Report.artifacts bound the fold saturates; the full output rides the run's scope dir
# Wire caps read through the SOLE field_cap introspection (model.py) so the clip tracks each type: a parse message/hint past
# its cap sets Envelope.truncated honestly. Fault.message (1024) bounds the constructed parse message; Diagnostic.hint (256)
# bounds the distilled hint so the auto-observability envelope round-trips through the history decoder (msgspec enforces
# max_length on DECODE only — an unclipped wire encodes to stdout yet faults on _load_run, folding the run to None in delta).
_MESSAGE_CAP: Final[int] = field_cap(Fault, "message", default=1 << 62)
# _HINT_CAP and _RESULT_CAP are defined once in core/model.py (the sole saturation + clip sites) and imported above.
_DISPATCH_NONE: Final = "dispatch=none"  # the shared token _seed_parse_ring writes and _distill reads; one definition, two sites
# PER-INVOCATION one-Envelope guard (Invariant 1), seeded fresh in rail.run: a process-static
# count() would fault every fire after the first in the long-lived automation loop.
_WRITES: ContextVar[Iterator[int]] = ContextVar("assay_writes")
_LOG: Final = structlog.get_logger("assay.registry")  # best-effort run-history persist rail (history writes never fault a rail)
_PROBE_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)  # Input.NONE probe seed: place emits one empty tail
_PROBE_TIMEOUT: Final = 8.0  # per `--version`/git probe deadline so a hung tool never strands self-test
_ENVELOPE_DECODER: Final[msgspec.json.Decoder[Envelope]] = msgspec.json.Decoder(Envelope)  # run-history read-back


def _identity_hom(*_a: object, **_k: object) -> Result[Report, Fault]:
    # Identity Hom _composes weaves to probe the decoration-time Slot inversion.
    return Ok(None)  # type: ignore[arg-type]  # ty: ignore[invalid-return-type]  # the probe never inspects the value; Ok(None) stands in for any Report


def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object) -> Mapping[str, object]:
    # The one correlation map both logged and traced bind; strict read via getattr stays inert on Params lacking the field.
    return {"run_id": settings.run_id, "strict": getattr(params, "strict", False), **settings.agent_context}


# checked ▷ logged ▷ traced sorted by Slot in compose; NO retried (a rail is a Hom, not a Spawn).
_RAIL_LAYERS: Final[tuple[Layer[..., Report], ...]] = (
    checked(),
    logged(event="rail", keys=_correlate),
    traced(span="assay.rail", attrs=_correlate),  # run_id + agent_task_id correlate the rail span with each run_check child span
)

REGISTRY: Final[tuple[Bind, ...]] = (  # binds the per-verb ADAPTERS (static_rail.fix etc.), never thin_rail
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


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bound(params: object, claim: Claim, verb: str) -> Result[object, Fault]:
    # The single positional-arity boundary: project the variadic `paths` sink onto the verb's contract via
    # the polymorphic BaseParams.bound (variadic path rail passes through; package/bridge/api-doctor reject a
    # surplus token; api query/resolve/show cap arity). A Fault short-circuits the handler so the surplus
    # token rides the canonical `parse` taxonomy _failing_step names — never a silent black-hole exit 0.
    # On a surplus Fault, seed the SAME dispatch=<claim> + FULL reconstructed command-line row parse_fault
    # seeds, so both parse raise sites distill ONE Diagnostic shape (recent_events[1] is the whole `<claim>
    # <verb> <tokens>` line at BOTH sites, never inverted — a surplus reads the same shape as an unknown verb).
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
    # Seed the invocation-scoped ring _distill reads so a parse Fault carries the dispatch discriminant +
    # the joined FULL command line off machine data: recent_events[0]=dispatch=<claim|none>, recent_events[1]=
    # the whole reconstructed `<claim> <verb> <tokens>` line — ONE shape both parse raise sites write.
    cmd = " ".join(tokens)[:_HINT_CAP]
    match _RING.get():
        case deque() as ring:
            ring.extend((f"dispatch={dispatch}", cmd))
        case None:
            _RING.set(deque((f"dispatch={dispatch}", cmd), maxlen=16))


def _strict(outcome: Result[Report, Fault], params: object) -> Result[Report, Fault]:
    # Promote a no-op fold to Error(Fault(FAULTED)) under --strict. Only EMPTY/SKIP promotes —
    # FAILED/BUSY/TIMEOUT already dominate by severity; getattr keeps it inert on Params lacking strict.
    match outcome:
        case Result(tag="ok", ok=report) if getattr(params, "strict", False) and report.status in {RailStatus.EMPTY, RailStatus.SKIP}:
            return Error(Fault((), RailStatus.FAULTED, "strict: empty/skipped fold"))
        case _:
            return outcome


def _distill(
    fault: Fault, duration_ms: float, *, events: tuple[str, ...] | None = None, resource: tuple[tuple[str, float], ...] = (), step: str | None = None
) -> tuple[Diagnostic, bool]:
    # Distill a Fault + context into one Diagnostic (+ wire truncation flag).  Two callers:
    #   • Error(fault) branch: events=None → reads _RING; step=None → _failing_step names the infra stage.
    #   • Ok(FAILED) branch: events = defect Match rows; step="defects" names the stage the tools failed at.
    # Generalised so BOTH rails feed ONE Diagnostic shape — zero per-rail ceremony. The resource snapshot
    # is taken LAZILY here (error path only) when neither the caller nor _diagnose seeded one, so the
    # psutil oneshot never touches the happy wire.
    # The truncation flag is the SOLE parse-wire honesty signal: it fires when the framed reason had to
    # be clipped, the Fault message hit _MESSAGE_CAP, OR surplus() already buried the `…` overflow
    # sentinel — every byte dropped on the parse wire sets Envelope.truncated, never a silent clip.
    ring = events if events is not None else tuple(_RING.get() or ())
    step = step if step is not None else _failing_step(fault)
    reason = fault.message.removeprefix(f"{step}: ") or (ring[-1] if ring else "")  # strip the canonical prefix the step already names
    framing = f"{step}: after {duration_ms:.1f}ms"  # fixed shape the reason wraps; the trailing `after …ms` discriminant MUST survive
    budgeted = reason[: max(_HINT_CAP - len(framing), 0)]  # clip the variable reason FIRST so the framing is never severed at the tail
    hint = f"{step}: {budgeted} after {duration_ms:.1f}ms"
    truncated = len(reason) > len(budgeted) or len(fault.message) >= _MESSAGE_CAP or fault.message.endswith("…")
    dispatched = not (ring and ring[0] == _DISPATCH_NONE)  # the TYPED dispatch fact off the SAME ring _seed_parse_ring writes
    snap = (
        resource or _RESOURCE.get() or tuple(_snapshot().items())
    )  # caller ▷ _diagnose's at-fault seed ▷ lazy snapshot (error path only; never the happy wire)
    return Diagnostic(failing_step=step, recent_events=ring, elapsed_ms=duration_ms, hint=hint, dispatched=dispatched, resource=snap), truncated


def _failing_step(fault: Fault) -> str:
    # Name the faulting stage structurally: status ▷ argv ▷ canonical message prefix. Synthetic promotions
    # carry argv=() plus a strict:/validation:/parse: prefix (never a heuristic ring scan); an empty-argv
    # fault with no such prefix is the rare lease-fd OSError, folded to spawn.
    match (fault.status, fault.argv):
        case (RailStatus.TIMEOUT, _):
            return "timeout"
        case (RailStatus.BUSY, _):
            return "lease_busy"
        case (_, ()):
            return next((step for step in ("strict", "validation", "parse") if fault.message.startswith(f"{step}:")), "spawn")
        case _:
            return "spawn"


def _ok_envelope(bind: Bind, settings: AssaySettings, ms: float, report: Report) -> Envelope:
    # Build the success Envelope: on FAILED, distill error_context from the fold-projected defect rows
    # so the agent sees which/why identically to the Fault rail (W1 + W5). omit_defaults keeps OK/EMPTY terse.
    truncated = len(report.results) >= _RESULT_CAP or len(report.artifacts) >= _ARTIFACT_CAP
    truncated and sys.stderr.write(f"assay: {bind.claim.value} {bind.verb} output truncated; full results under {settings.run_id}\n")
    failed = tuple(m for m in report.results if m.severity == "failed")
    ctx = (
        _distill(
            Fault((), RailStatus.FAILED, f"{len(failed)} tool(s) failed"),
            ms,
            events=tuple(f"{m.id}: {m.text[:120]}" for m in failed),
            step="defects",  # a check ran and found defects — NOT an infra spawn/timeout/lease fault
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


def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    # The sole stdout writer: fold the rail Result into one Envelope. Truncation on the success wire derives
    # off the saturated results/artifacts bounds; on the Error wire off _distill's clip flag (parse path).
    # _WRITES is the one-Envelope guard (Invariant 1): the first write rides stdout; any later rank is a
    # wiring defect → FAULTED to stderr. A surplus-positional parse Fault MUST NOT persist — it is the same
    # typo'd-token class parse_fault refuses to ring-pollute, so it shares persist=False (failing_step="parse").
    ms = (time.perf_counter() - started) * 1000.0
    persist = True
    match outcome:
        case Result(tag="ok", ok=report):
            envelope = _ok_envelope(bind, settings, ms, report)
        case Result(error=fault):
            diagnostic, truncated = _distill(fault, ms)
            persist = diagnostic.failing_step != "parse"  # a typo'd surplus token never enters the diffable history ring
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
            sys.stderr.buffer.write(_ENCODER.encode(doubled) + b"\n")
            return doubled


def rail(bind: Bind) -> Callable[[object], Envelope]:
    """Build the per-verb runner — weaves ``checked ▷ logged ▷ traced`` once over the bound ``Handler``.

    ``_strict`` and the docs ``FaultedPromotion`` catch sit between the handler and ``_emit``; the
    runner is named for the verb so structlog/trace correlation and the Cyclopts leaf read a stable
    identity.
    """
    handler: Handler[object] = compose(*_RAIL_LAYERS)(_narrow(bind.handler))

    def run(params: object) -> Envelope:
        settings = AssaySettings()
        started = time.perf_counter()
        scope = ArtifactScope.open(settings, bind.claim)
        # invocation-scoped ring + one-Envelope counter, reset in finally so the automation loop reuses
        # rail() per fire. The W5 resource snapshot is NOT seeded here — _diagnose seeds it at fault time
        # and _distill takes it lazily on the defect path, so the psutil oneshot never costs the happy wire.
        ring_token, writes_token = _RING.set(deque(maxlen=16)), _WRITES.set(count())
        try:
            outcome = _guard(lambda: _bound(params, bind.claim, bind.verb).bind(lambda p: _validated(_strict(handler(settings, scope, p), p))))
            return _emit(bind, settings, started, outcome)
        finally:
            _WRITES.reset(writes_token)
            _RING.reset(ring_token)

    run.__name__ = bind.verb
    return run


def _narrow(handler: object) -> Handler[object]:
    # Narrow Bind.handler (erased to object) to the 3-arg Handler without cast (banned by TID251). Each
    # rail imports its -> Result[Report, Fault] return annotation unconditionally so checked's beartype
    # forward-ref resolution succeeds under PEP 649 without mutating __globals__; a non-function bind faults.
    match handler:
        case FunctionType() as fn:
            return fn  # the FunctionType narrow satisfies the Handler[object] callable shape; the REGISTRY rows are the proof of arity
        case _:
            raise TypeError(f"Bind.handler must be a module-level def (FunctionType), got {type(handler).__name__}")


def _validated(outcome: Result[Report, Fault]) -> Result[Report, Fault]:
    # Round-trip the success Report.detail through the tagged-union codec so malformity fails loud:
    # validate_detail raises msgspec.MsgspecError inside the _guard thunk. Asserts the wire is decodable
    # only — the outcome passes through untouched, the detail is never re-emitted.
    match outcome:
        case Result(tag="ok", ok=report):
            validate_detail(report.detail)
            return outcome
        case _:
            return outcome


def _guard(thunk: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:
    # The one except boundary: catch the docs FaultedPromotion + a malformed Detail, each mapped to
    # Fault{argv=(), FAULTED} under a canonical strict:/validation: prefix so the composition root OWNS the
    # synthetic-fault taxonomy and _failing_step names the stage by prefix, never a heuristic ring scan.
    try:
        return thunk()
    except FaultedPromotion as promoted:
        return Error(Fault((), RailStatus.FAULTED, f"strict: {promoted}"))
    except msgspec.MsgspecError as malformed:
        return Error(Fault((), RailStatus.FAULTED, f"validation: {malformed}"))


# --- [COMPOSITION] ----------------------------------------------------------------------

_ENCODER: Final = msgspec.json.Encoder(order="deterministic")  # content-addressable wire order; the sole stdout codec, cached once


def _emit_envelope(settings: AssaySettings, envelope: Envelope, *, persist: bool = True) -> Envelope:
    # The SOLE stdout Envelope writer: write the one deterministic-order wire line, then optionally
    # side-write it to run-history (rail wire + self-test census persist; delta/parse_fault are read-only/
    # pre-dispatch and MUST NOT pollute the diffable ring _prior/delta walk, so they pass persist=False).
    # The history write rides AFTER the stdout line (Invariant 1 untouched). The persist gate lives HERE,
    # the one caller that owns it, so _persist stays an unconditional best-effort side write.
    sys.stdout.buffer.write(_ENCODER.encode(envelope) + b"\n")
    persist and _persist(settings, envelope)  # type: ignore[func-returns-value]  # intentional: _persist returns None; short-circuit is the gate, not a value use
    return envelope


def _persist(settings: AssaySettings, envelope: Envelope) -> None:
    # Run-history side write + prune to artifact_retention. Best-effort: an FS error degrades to a structlog
    # warning, never a fault. pipe_file writes the same wire bytes so a persisted run round-trips through delta.
    try:
        store = settings.store()
        directory = store.ensure(ArtifactKind.HISTORY.value, settings.run_id)
        store.fs.pipe_file(f"{directory}/envelope.json", _ENCODER.encode(envelope))
        _retain(store, settings.artifact_retention)
    except OSError as exc:
        _LOG.warning("history.persist_failed", run_id=settings.run_id, error=str(exc)[:200])


def _retain(store: ArtifactStore, keep: int) -> None:
    # Prune run-history to the newest keep runs. run_id is an ISO-timestamp prefix so a lexical sort is
    # chronological (the current run survives); fsspec rm is idempotent so concurrent invocations never fault.
    runs = sorted(store.glob(f"{ArtifactKind.HISTORY.value}/*"))
    excess = runs[: max(0, len(runs) - keep)]
    tuple(store.fs.rm(path, recursive=True) for path in excess)


def _run_ids(store: ArtifactStore) -> tuple[str, ...]:
    # The persisted run_ids, chronologically sorted (ISO-timestamp basename of each history run dir).
    return tuple(path.rstrip("/").rsplit("/", 1)[-1] for path in sorted(store.glob(f"{ArtifactKind.HISTORY.value}/*")))


def _prior(run_ids: tuple[str, ...], run_id: str) -> str:
    # The largest persisted id < run_id (chronologically just before it); "" when none precedes it.
    earlier = tuple(r for r in run_ids if r < run_id)
    return earlier[-1] if earlier else ""


def _load_run(store: ArtifactStore, run_id: str) -> Envelope | None:
    # Decode one persisted run Envelope; absence / decode-failure / an empty id folds to None.
    match run_id:
        case "":
            return None
        case _:
            try:
                return _ENVELOPE_DECODER.decode(store.fs.cat_file(f"{store.root}/{ArtifactKind.HISTORY.value}/{run_id}/envelope.json"))
            except OSError, msgspec.MsgspecError:
                return None


def _delta_report(before_id: str, after_id: str, before: Envelope | None, after: Envelope | None) -> Report:
    # Fold two persisted Envelopes' status/counts/result drift into a RunDelta Report (missing side → EMPTY).
    # Results keyed by (id, line) so added/removed are symmetric-difference cardinalities; an error-channel
    # Envelope (no report) contributes zero counts.
    def snapshot(
        run_id: str, env: Envelope
    ) -> tuple[RunSnapshot, frozenset[tuple[str, int]]]:  # one projection per endpoint: no per-field None cascade
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
    """Root verb ``delta <run-id> [--against <id>]``: diff two persisted run Envelopes into a ``RunDelta`` Detail.

    Sibling to ``self-test`` (a ROOT command, NOT a Claim); ``--against`` defaults to the chronologically
    prior persisted run. A run absent from history folds to EMPTY. Read-only — no rail params, no lease.
    """
    settings = AssaySettings()
    store = settings.store()
    before_id = against or _prior(_run_ids(store), run_id)
    report = _delta_report(before_id, run_id, _load_run(store, before_id), _load_run(store, run_id))
    # Route through the single envelope() factory (matching self_test), lifting report.notes onto the wire.
    env = msgspec.structs.replace(envelope(report, claim=Claim.STATIC, verb="delta", run_id=settings.run_id), notes=report.notes)
    return _emit_envelope(settings, env, persist=False)  # read-only diff: stdout only, never re-enters the diffable history ring


def parse_fault(tokens: tuple[str, ...], message: str) -> Envelope:
    """Fold a Cyclopts parse error (unknown command/option, unused token) into the canonical Fault envelope.

    The single pre-dispatch fault writer: a malformed token set never reaches ``rail.run`` (no ``Bind``,
    no lease, no ``_RING``), so the boundary maps it to ``Fault((), FAULTED, …)`` and routes it through
    the SAME ``envelope()`` builder + ``_emit_envelope`` stdout writer the rails use — the parse fault
    rides the canonical Fault Envelope with structured ``error_context`` instead of a bare Rich panel.
    It is NOT persisted (``persist=False``): a typo'd token set must never enter the diffable run-history
    ring that ``_prior``/``delta`` walk, nor burn an ``artifact_retention`` slot.

    ``claim``/``verb`` are a DISPATCH FACT only when ``head`` is a real ``Claim`` (the sub-app resolved but
    its verb/option was malformed). On a bare/unknown root token NO rail dispatched, so ``claim`` folds to a
    ``STATIC`` PLACEHOLDER (the enum is required + we never invent a member) and ``verb`` carries the raw
    bogus token. An agent reads the TYPED ``error_context.dispatched`` discriminant (``False`` here) — no
    substring-scraping ``recent_events[0]`` — to know the wire ``claim``/``verb`` are placeholders, not facts.

    This shares ONE Diagnostic builder (``_distill``) and ONE wire shape with the surplus-positional parse
    Fault (``BaseParams.surplus`` → ``_bound`` → ``_emit``): the boundary seeds ``_RING`` with the
    ``dispatch=…`` + full-command-line rows the rail path also seeds, prefixes the Fault message with
    ``parse:`` so ``_failing_step`` names it structurally, then distills — both raise sites carry
    ``recent_events`` + ``elapsed_ms`` + a ``parse: … after …ms`` hint + the typed ``dispatched`` +
    ``truncated`` honesty signal identically (zero-surprise across the two sites).
    """
    settings = AssaySettings()
    match tokens:
        case (head, *rest) if head in Claim._value2member_map_:  # StrEnum value lookup avoids a try/except ValueError round-trip
            claim, verb, dispatch = Claim(head), (rest[0] if rest else ""), head  # head IS a rail: claim/verb are a dispatch fact
        case (head, *_):
            claim, verb, dispatch = Claim.STATIC, head, "none"  # no rail dispatched: claim=static is a placeholder, verb is the raw token
        case _:
            claim, verb, dispatch = Claim.STATIC, "", "none"
    fault = Fault((), RailStatus.FAULTED, f"parse: {message}"[:_MESSAGE_CAP])  # clip the WHOLE prefixed string so the field cap holds; step="parse"
    _seed_parse_ring(
        dispatch, tokens
    )  # seed the ring _distill reads: dispatch=<claim|none> + the FULL command line, the SAME shape the rail surplus path seeds
    diagnostic, truncated = _distill(fault, 0.0)  # dispatched (off dispatch=none) + truncated (off the message clip) derive in the SOLE distill site
    env = msgspec.structs.replace(envelope(fault, claim=claim, verb=verb, run_id=settings.run_id, error_context=diagnostic), truncated=truncated)
    return _emit_envelope(settings, env, persist=False)


def self_test(*, rhino: bool = False) -> Envelope:
    """Root preflight: affirm the composition is wired + the catalog is unrotted; ``--rhino`` gates status on Rhino availability.

    Folds the structural invariants plus a ``REGISTRY``+``TOOLS`` census into one ``Report`` (every row
    binds a callable ``Handler``, every claim resolves a non-empty verb set, the rail seam composes
    without a ``Slot`` inversion, every catalog row is routable via ``select()`` and parses an empty
    ``Completed``), folded to ``FAILED`` on the first broken row. ``_health`` deepens it with a concurrent
    toolchain/git probe whose findings ride typed ``Match`` rows (kind=PROCESS) — surfaced-but-not-fatal.
    When ``--rhino`` is set and no yak executable is available, status gates to ``FAILED``.
    A root command, not a bound rail.
    """
    settings = AssaySettings()
    claims = frozenset(b.claim for b in REGISTRY)
    healthy = all(callable(b.handler) for b in REGISTRY) and all(any(b.claim is c for b in REGISTRY) for c in claims) and _composes() and _census()
    health_probes, health_notes = _health(settings)
    status = RailStatus.FAILED if (not healthy or (rhino and not _yak_ready())) else RailStatus.OK
    summary = f"rows={len(REGISTRY)} claims={len(claims)} tools={len(TOOLS)} healthy={healthy} rhino={'required' if rhino else 'skipped'}"
    report = fold(Claim.STATIC, "self-test", (receipt(("assay", "self-test"), 0 if status is RailStatus.OK else 1, status=status, notes=(summary,)),))
    # Census rows (typed Match) + health probes join defect rows; tool/git probe lines ride notes (human summary only).
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
    # Probe each distinct tool's --version + git worktree staleness CONCURRENTLY via the rails' fan_out.
    # Returns (typed Match rows for machine data, human notes for the wire).
    # Surfaced-but-not-fatal: a MISSING tool rides a note, never the self-test OK/FAILED contract.
    probes = (*_tool_probes(), _GIT_HEAD, _GIT_DIRTY)
    results = fan_out(probes, settings=settings, scope=None, routed=_PROBE_ROUTED)
    noted = tuple(map(_probe_note, probes, results, strict=True))
    probe_matches = tuple(
        Match(id=check.tool.name, kind=ArtifactKind.PROCESS, text=note, severity="failed" if not ok else None)
        for check, (note, ok) in zip(probes, noted, strict=True)
    )
    notes = tuple(note for note, _ in noted)
    return probe_matches, notes


def _probe(name: str, argv: tuple[str, ...]) -> Check:
    # One DIRECT/Input.NONE probe Check: the full argv runs verbatim under a deadline.
    tool = Tool(name=name, runner=Runner.DIRECT, command=argv, input=Input.NONE, language=Language.PYTHON, claim=Claim.STATIC, timeout=_PROBE_TIMEOUT)
    return Check(tool=tool)


_GIT_HEAD: Final = _probe("git-head", ("git", "rev-parse", "--short", "HEAD"))  # worktree HEAD short-sha probe (read-only, no lease)
_GIT_DIRTY: Final = _probe("git-dirty", ("git", "status", "--porcelain"))  # worktree clean/dirty probe (read-only, no lease)


def _tool_probes() -> tuple[Check, ...]:
    # One --version probe per DISTINCT program in TOOLS (dedup by launcher+program; DOTNET → one SDK probe; INPROC skipped).
    def keyed(tool: Tool) -> tuple[str, tuple[str, ...]]:  # one match per tool: the dedup key plus the version-probe argv
        match tool.runner:
            case Runner.DOTNET:
                return "dotnet", ("dotnet", "--version")  # every DOTNET tool shares the one SDK probe
            case _:
                return f"{tool.runner.value}:{tool.command[0]}", (*tool.runner.prefix, tool.command[0], "--version")

    deduped = dict(keyed(t) for t in TOOLS if t.runner is not Runner.INPROC)
    return tuple(_probe(argv[-2], argv) for argv in deduped.values())


def _probe_note(check: Check, result: Result[Completed, Fault]) -> tuple[str, bool]:
    # Project one probe Result to (note, ok): a nested match discriminates the probe family (git vs tool) on
    # tool.name, then the resolved Completed (or None on a spawn fault) — one surface, no per-family helper.
    # ok=False when a tool is MISSING; git probes are informational (ok=True always) — surfaced-but-not-fatal.
    name = check.tool.name
    match result.ok if result.is_ok() else None:  # outer = the Completed|None spawn result (ty-total in two cases); inner = the probe family
        case None:
            note = f"git: {name.removeprefix('git-')} unavailable" if name in {"git-head", "git-dirty"} else f"tool {name}: MISSING"
            return note, name in {"git-head", "git-dirty"}  # git unavailable is informational; tool MISSING is a failure
        case Completed() as d if name == "git-head":
            return f"git: HEAD {d.stdout.decode(errors='replace').strip()[:40] or 'unknown'}", True
        case Completed() as d if name == "git-dirty":
            return f"git: {'dirty' if d.stdout.strip() else 'clean'}", True
        case Completed(returncode=0) as d:
            lines = d.stdout.decode(errors="replace").strip().splitlines()
            return f"tool {name}: {lines[0][:80] if lines else 'present'}", True
        case Completed() as d:
            return f"tool {name}: present (exit {d.returncode})", True


def _census() -> bool:
    # Day-one catalog-rot catch: a row not surfaced by select() (routing fracture) or whose parser raises
    # on the empty Completed (decode-shape drift) folds to False. select mirrors the rail fan-out slice.
    return all(t in select(t.claim, t.language) for t in TOOLS) and all(_parses(t.parser) for t in TOOLS)


def _parses(parser: object) -> bool:
    # Probe one catalog Parser against an empty Completed. A None parser is trivially safe; a callable
    # must fold the degenerate Completed((), 0) without raising — a raise signals a decode-shape regression.
    match parser:
        case None:
            return True
        case fn if callable(fn):
            try:
                fn(Completed((), 0))
            except Exception:  # noqa: BLE001  # census probe: ANY parser raise on the empty receipt is a rotted row, folded to False
                return False
            return True
        case _:
            return False


def _yak_ready() -> bool:
    # Probe whether a yak executable is reachable via the configured catalog (heuristic: at least one
    # PACKAGE-claim Tool has a DIRECT runner whose command starts with "yak" and the path is executable).
    import os  # noqa: PLC0415  # stdlib: deferred import, only on --rhino path

    return any(
        t.runner is Runner.DIRECT and t.command and t.command[0] == "yak" and os.access(t.command[0], os.X_OK)
        for t in TOOLS
        if t.claim is Claim.PACKAGE
    )


def _composes() -> bool:
    # compose surfaces a Slot inversion as a decoration-time TypeError, so weaving the identity Hom is the
    # cheapest structural proof the checked ▷ logged ▷ traced order is monotonic without spawning a process.
    try:
        compose(*_RAIL_LAYERS)(_identity_hom)
    except TypeError:
        return False
    return True


def _leaf(bind: Bind) -> Callable[..., Envelope]:
    # Wrap rail(bind) as a Cyclopts leaf flattening its Params. The flatten rides BaseParams' inherited
    # @Parameter(name="*"), so the leaf needs only a runtime __annotate__ (PEP 649 lazy form, NOT a direct
    # __annotations__ write) pinning the concrete Params type. Identity is set directly, NOT via
    # functools.wraps — wraps would stamp __wrapped__ at the defaultless run closure, which cyclopts
    # follows during signature inspection and rejects (a flatten param needs a default on the signature).
    runner = rail(bind)

    def command(
        params: object = bind.params(),
    ) -> Envelope:  # cyclopts needs a concrete default on the flatten param; the frozen Params instance is immutable so the shared default is safe
        return runner(params)

    command.__name__ = bind.verb
    command.__qualname__ = f"_leaf.{bind.verb}"
    command.__doc__ = bind.help
    command.__annotate__ = lambda _format: {"params": bind.params, "return": Envelope}  # PEP 649 lazy form, never a direct __annotations__ write
    return command


def build_app(registry: tuple[Bind, ...]) -> App:
    """Fold ``registry`` into the Cyclopts ``App`` tree: ``groupby(claim)`` sub-apps, ``self-test`` on root.

    Pre-sorts on ``b.claim.value`` (the ``StrEnum`` wire token) so each ``groupby`` run is contiguous and
    a future enum reorder can never fragment a claim into two sub-apps. ``result_action="return_value"``
    makes each command return its ``Envelope`` so ``__main__`` resolves the exit code.
    """
    root = App(name="assay", help="Rasm polyglot quality operator.", default_parameter=Parameter(show_default=False), result_action="return_value")
    keyed = sorted(registry, key=lambda b: b.claim.value)
    subs = tuple(
        reduce(lambda app, row: _register(app, _leaf(row), name=row.verb, help=row.help), tuple(rows), App(name=claim.value))
        for claim, rows in groupby(keyed, key=attrgetter("claim"))
    )
    app = reduce(_register, subs, root)
    _register(app, self_test, name="self-test")
    _register(app, delta, name="delta")  # root verb: read-back of the auto-persisted run history (sibling to self-test)
    return app


def _register(app: App, obj: App | Callable[..., object], *, name: str | None = None, help: str = "") -> App:  # noqa: A002  # cyclopts command kwarg is named "help"; mirroring the CLI surface
    # App.command returns the registered object, not the parent, so this seam returns app to keep the
    # groupby/sub-app folds point-free; name/help are forwarded only when present (App-flatten has no name).
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
