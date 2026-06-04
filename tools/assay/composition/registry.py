"""Composition root: one ``Bind`` table, one ``rail`` runner, one ``_emit`` stdout writer.

The runner stack is ``checked ▷ logged ▷ traced`` (no ``@retried`` — a rail is a ``Hom``, not a
``Spawn``), folded by ``Slot`` rank in ``compose`` so a ``Slot`` inversion is a decoration-time
``TypeError``. ``_emit`` alone writes stdout (one ``Envelope`` via the cached ``_ENCODER``);
``structlog`` lines, ``Fault.message`` egress, and truncation notes ride stderr.
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

from tools.assay.composition.catalog import select, TOOLS  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # intra-package import; tools.assay is the package root
from tools.assay.core.aspect import _RING, checked, compose, logged, traced  # noqa: PLC2701  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    Bind,
    Claim,
    Completed,
    Diagnostic,
    Envelope,
    Fault,
    fold,
    receipt,
    Report,
    validate_detail,
)
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

    from tools.assay.core.aspect import Layer  # intra-package import; tools.assay is the package root


# --- [TYPES] ----------------------------------------------------------------------------

# The per-verb 3-arg adapter the registry binds. ``P`` is the verb's Params type — a single TypeVar,
# never a ParamSpec: the third positional is one value, not a call signature, so ``Callable[[S, A, P], R]`` holds.
type Handler[P] = Callable[[AssaySettings, ArtifactScope, P], Result[Report, Fault]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_RESULT_CAP: Final = 1000  # the Report.results bound the fold saturates; a saturated collection signals truncation to _emit
_ARTIFACT_CAP: Final = 100  # the Report.artifacts bound the fold saturates; the full output rides the run's scope dir
# PER-INVOCATION one-Envelope guard (Invariant 1), seeded fresh in rail.run: a process-static
# count() would fault every fire after the first in the long-lived automation loop.
_WRITES: ContextVar[Iterator[int]] = ContextVar("assay_writes")


def _identity_hom(*_a: object, **_k: object) -> Result[Report, Fault]:
    """Identity ``Hom`` that ``_composes`` weaves to probe the decoration-time ``Slot`` inversion."""
    return Ok(None)  # type: ignore[arg-type]  # ty: ignore[invalid-return-type]  # the probe never inspects the value; Ok(None) stands in for any Report


def _correlate(settings: AssaySettings, _scope: ArtifactScope, params: object) -> Mapping[str, object]:
    """Project rail args + ``agent_context`` onto the one map both ``logged`` and ``traced`` bind.

    Carries ``run_id``/``strict`` plus the ``settings.agent_context`` (``{run.id, agent.task.id}``)
    pair. ``logged`` binds it into ``structlog`` context-vars; ``traced`` mirrors and
    namespace-normalizes it (dotted ``run.id``/``agent.task.id`` → ``run_id``/``agent_task_id``) into
    OTel baggage, correlating the parent rail span with each ``run_check`` child span. ``strict`` is
    read via ``getattr`` so it stays inert on any ``Params`` lacking the field.
    """
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


def _strict(outcome: Result[Report, Fault], params: object) -> Result[Report, Fault]:
    """Promote a no-op fold to ``Error(Fault(FAULTED))`` under ``--strict``.

    Reads ``strict`` via ``getattr`` so the promotion is inert on any ``Params`` lacking the field.
    Only a folded ``EMPTY``/``SKIP`` promotes — a ``FAILED``/``BUSY``/``TIMEOUT`` fold already
    dominates by severity, and a real ``Fault`` passes through untouched. This is the sole synthetic
    error channel the composition root authors (the engine fold never emits a ``Fault``); the docs
    rail's ``FaultedPromotion`` is the only other strict promoter, caught in ``_guard`` and mapped to
    the identical ``Fault``.
    """
    match outcome:
        case Result(tag="ok", ok=report) if getattr(params, "strict", False) and report.status in {RailStatus.EMPTY, RailStatus.SKIP}:
            return Error(Fault((), RailStatus.FAULTED, "strict: empty/skipped fold"))
        case _:
            return outcome


def _distill(fault: Fault, duration_ms: float) -> Diagnostic:
    """Distill the invocation-scoped ring + the ``Fault`` into one ``Diagnostic`` for the Error branch.

    The auto-observability payoff: the bounded recent-events ring ``rail`` seated into ``_RING`` (appended
    in place by ``ring_processor``, surviving the engine's ``anyio.run`` via the copied context-var
    reference) is read back here at envelope time. ``_failing_step`` names the faulting stage from the
    locked ``Fault`` shape; the ``hint`` synthesizes the last ring event (or the fault message) with the
    elapsed wall time so an agent retriages off the wire without re-running. Rides ``Envelope.error_context``
    only — the success branch leaves it ``None`` so ``omit_defaults`` keeps the success wire terse.
    """
    events = tuple(_RING.get() or ())
    step = _failing_step(fault)
    reason = fault.message.removeprefix(f"{step}: ") or (events[-1] if events else "")  # strip the canonical prefix the step already names
    hint = f"{step}: {reason} after {duration_ms:.1f}ms"
    return Diagnostic(failing_step=step, recent_events=events, elapsed_ms=duration_ms, hint=hint)


def _failing_step(fault: Fault) -> str:
    """Name the faulting stage from the locked ``Fault`` shape: status ▷ argv ▷ canonical message prefix.

    ``TIMEOUT``/``BUSY`` are unambiguous. A ``FAULTED`` fault carrying a non-empty ``argv`` named the
    failing command (an engine spawn or a routing probe), so it is a ``spawn``. The composition root's
    synthetic promotions all carry ``argv=()`` plus a canonical ``strict:``/``validation:`` message prefix
    (``_strict`` here; the ``_guard`` seam for the docs ``FaultedPromotion`` and the ``_validated`` codec),
    so the prefix — never a heuristic ring scan — names ``strict`` vs ``validation``. An empty-argv fault
    with no such prefix is the rare lease-fd ``OSError`` (a resource fault), folded to ``spawn``.
    """
    match (fault.status, fault.argv):
        case (RailStatus.TIMEOUT, _):
            return "timeout"
        case (RailStatus.BUSY, _):
            return "lease_busy"
        case (_, ()):
            return next((step for step in ("strict", "validation") if fault.message.startswith(f"{step}:")), "spawn")
        case _:
            return "spawn"


def _emit(bind: Bind, settings: AssaySettings, started: float, outcome: Result[Report, Fault]) -> Envelope:
    """Fold the rail ``Result`` into one ``Envelope`` and write it to stdout — the sole stdout writer.

    A statement ``match`` projects the success channel's ``Report`` or the error channel's ``Fault``
    onto an ``Envelope`` whose ``exit_code`` derives from the single ``RailStatus.exit_code`` source.
    Truncation is read off the capped collections the fold produced — ``Report`` carries no
    ``truncated`` field, so the registry derives it from the saturated ``results``/``artifacts`` bounds
    and writes a stderr pointer at the run's scope dir. ``_WRITES`` is the process-static one-Envelope
    guard (Invariant 1): the first write returns ``0`` and rides ``stdout.buffer``; any later write
    matches a non-zero rank, so a second writer is a wiring defect — the registry returns a ``FAULTED``
    ``Envelope`` to stderr instead of a second stdout line. The cached ``_ENCODER`` serializes in
    deterministic order.
    """
    ms = (time.perf_counter() - started) * 1000.0
    match outcome:
        case Result(tag="ok", ok=report):
            truncated = len(report.results) >= _RESULT_CAP or len(report.artifacts) >= _ARTIFACT_CAP
            truncated and sys.stderr.write(f"assay: {bind.claim.value} {bind.verb} output truncated; full results under {settings.run_id}\n")
            envelope = Envelope(
                schema_version=1,
                claim=bind.claim,
                verb=bind.verb,
                status=report.status,
                exit_code=report.status.exit_code,
                run_id=settings.run_id,
                duration_ms=ms,
                report=report,
                truncated=truncated,
                notes=report.notes,
            )
        case Result(error=fault):
            envelope = Envelope(
                schema_version=1,
                claim=bind.claim,
                verb=bind.verb,
                status=fault.status,
                exit_code=fault.status.exit_code,
                run_id=settings.run_id,
                duration_ms=ms,
                error=fault,
                error_context=_distill(fault, ms),
            )
    match next(_WRITES.get()):
        case 0:
            sys.stdout.buffer.write(_ENCODER.encode(envelope) + b"\n")
            return envelope
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
    """Build the per-verb runner — a plain function (it returns, never yields).

    Weaves ``checked ▷ logged ▷ traced`` once over the bound ``Handler`` (``compose`` filters any
    ``Slot.retried`` and surfaces a ``Slot`` inversion as a decoration-time ``TypeError``), then
    returns the closure CLI dispatch invokes per call. ``_strict`` and the docs ``FaultedPromotion``
    catch sit between the handler and ``_emit``; the runner is named for the verb so the
    structlog/trace correlation and the Cyclopts leaf read a stable identity.
    """
    handler: Handler[object] = compose(*_RAIL_LAYERS)(_narrow(bind.handler))

    def run(params: object) -> Envelope:
        settings = AssaySettings()
        started = time.perf_counter()
        scope = ArtifactScope.open(settings, bind.claim)
        # invocation-scoped ring + one-Envelope counter; both reset in finally so the automation loop reuses rail() per fire
        ring_token, writes_token = _RING.set(deque(maxlen=16)), _WRITES.set(count())
        try:
            outcome = _guard(lambda: _validated(_strict(handler(settings, scope, params), params)))
            return _emit(bind, settings, started, outcome)
        finally:
            _WRITES.reset(writes_token)
            _RING.reset(ring_token)

    run.__name__ = bind.verb
    return run


def _narrow(handler: object) -> Handler[object]:
    """Narrow ``Bind.handler`` (erased to ``object``) to the 3-arg ``Handler`` the rail seam weaves.

    The single validated adapter (no ``cast``, banned by TID251): every ``REGISTRY`` row binds a
    module-level ``def`` whose ``-> Result[Report, Fault]`` return annotation each rail now imports
    unconditionally, so ``checked``'s beartype forward-ref resolution succeeds under PEP 649 without
    mutating a rail's ``__globals__``. A non-function bind is a wiring defect surfaced fail-fast as a
    ``TypeError``, never silently passed through.
    """
    match handler:
        case FunctionType() as fn:
            return fn  # the FunctionType narrow satisfies the Handler[object] callable shape; the REGISTRY rows are the proof of arity
        case _:
            raise TypeError(f"Bind.handler must be a module-level def (FunctionType), got {type(handler).__name__}")


def _validated(outcome: Result[Report, Fault]) -> Result[Report, Fault]:
    """Round-trip the success ``Report.detail`` through the tagged-union codec so malformity fails loud.

    ``validate_detail`` re-encodes then decodes the ``detail`` against the cached ``AnyDetail | None``
    codec (``core/model``); a malformed variant raises ``msgspec.MsgspecError`` here inside the
    ``_guard`` thunk, where the seam maps it to the identical ``Fault{argv=(), FAULTED}`` as the docs
    promotion. A ``None`` detail or a well-formed variant round-trips inertly and the outcome passes
    through untouched — the registry never re-emits the detail, it only asserts the wire is decodable.
    """
    match outcome:
        case Result(tag="ok", ok=report):
            validate_detail(report.detail)
            return outcome
        case _:
            return outcome


def _guard(thunk: Callable[[], Result[Report, Fault]]) -> Result[Report, Fault]:
    """Catch the docs rail's ``FaultedPromotion`` + a malformed ``Detail`` at the seam: the one ``except`` boundary.

    The docs ``--strict`` promotion raises ``FaultedPromotion`` and ``_validated`` raises
    ``msgspec.MsgspecError`` on a malformed ``Report.detail``; both ride exactly the one ``raise`` (docs /
    the codec) and the one ``except`` here, never crossing a seam into domain logic. Each maps to
    ``Fault{argv=(), status=FAULTED}`` under a canonical ``strict:``/``validation:`` message prefix, so the
    composition root OWNS the synthetic-fault taxonomy and ``_failing_step`` names the stage structurally
    (prefix), never by a heuristic ring scan.
    """
    try:
        return thunk()
    except FaultedPromotion as promoted:
        return Error(Fault((), RailStatus.FAULTED, f"strict: {promoted}"))
    except msgspec.MsgspecError as malformed:
        return Error(Fault((), RailStatus.FAULTED, f"validation: {malformed}"))


# --- [COMPOSITION] ----------------------------------------------------------------------

_ENCODER: Final = msgspec.json.Encoder(order="deterministic")  # content-addressable wire order; the sole stdout codec, cached once


def self_test(*, rhino: bool = False) -> Envelope:
    """Root preflight: affirm the composition is wired + the catalog is unrotted; ``--rhino`` extends to the live host.

    Attached to the **root** ``App`` outside the claim tree, so the contract
    ``names == {b.claim.value for b in REGISTRY}`` checks claim sub-apps only. Folds the structural
    invariants plus a ``REGISTRY``+``TOOLS`` census into one ``Report``: every ``REGISTRY`` row binds a
    callable ``Handler``, every claim resolves a non-empty verb set, the rail seam composes without a
    ``Slot`` inversion (``_composes`` probes the eager decoration-time ``TypeError``), every catalog
    row is routable via ``select()``, and every catalog parser is callable on an empty ``Completed``
    without raising — folded to ``FAILED`` on the first broken row so day-one catalog rot is caught at
    preflight, not at a live rail. ``--rhino`` is surfaced as a note so the headless and live preflights
    share one ``Envelope`` shape. Returns the ``Envelope`` directly — it is a root command, not a bound rail.
    """
    claims = frozenset(b.claim for b in REGISTRY)
    healthy = all(callable(b.handler) for b in REGISTRY) and all(any(b.claim is c for b in REGISTRY) for c in claims) and _composes() and _census()
    status = RailStatus.OK if healthy else RailStatus.FAILED
    note = f"rows={len(REGISTRY)} claims={len(claims)} tools={len(TOOLS)} rhino={'probed' if rhino else 'skipped'}"
    report = fold(Claim.STATIC, "self-test", (receipt(("assay", "self-test"), 0 if healthy else 1, status=status, notes=(note,)),))
    envelope = Envelope(
        schema_version=1,
        claim=Claim.STATIC,
        verb="self-test",
        status=report.status,
        exit_code=report.status.exit_code,
        report=report,
        notes=report.notes,
    )
    sys.stdout.buffer.write(_ENCODER.encode(envelope) + b"\n")  # root command: write the Envelope through the wire so census evidence is visible
    return envelope


def _census() -> bool:
    """Affirm every ``TOOLS`` catalog row is routable via ``select()`` and parses an empty ``Completed`` clean.

    The day-one catalog-rot catch: a row whose ``(claim, language)`` does not surface it through
    ``select()`` (a routing fracture) or whose ``parser`` raises on a degenerate empty ``Completed``
    (a decode-shape drift) folds the census to ``False``. ``select(claim, language)`` mirrors the rail
    fan-out slice, so membership proves the row is reachable; ``_parses`` is the marked-boundary probe
    that an attached ``Parser`` survives the empty receipt without an exception.
    """
    return all(t in select(t.claim, t.language) for t in TOOLS) and all(_parses(t.parser) for t in TOOLS)


def _parses(parser: object) -> bool:
    """Probe one catalog ``Parser`` against an empty ``Completed``: a marked-boundary census check.

    A ``None`` parser is trivially safe (the rail reads status off ``Completed`` directly). A callable
    parser must fold the degenerate ``Completed((), 0)`` (empty ``stdout``) without raising — every
    catalog parser already guards with ``done.stdout or b"…"``, so a raise here signals a decode-shape
    regression in the row, exactly what the census must surface at preflight.
    """
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


def _composes() -> bool:
    """Affirm ``compose(*_RAIL_LAYERS)`` weaves without a ``Slot`` inversion: a marked-boundary probe.

    Besides the ``FaultedPromotion`` seam this is the only ``try/except`` in the module: ``compose``
    surfaces a ``Slot`` inversion as a decoration-time ``TypeError``, so weaving the identity ``Hom`` is
    the cheapest structural proof the ``checked ▷ logged ▷ traced`` order is monotonic without invoking
    a real rail or spawning a process.
    """
    try:
        compose(*_RAIL_LAYERS)(_identity_hom)
    except TypeError:
        return False
    return True


def _leaf(bind: Bind) -> Callable[..., Envelope]:
    """Wrap one ``rail(bind)`` runner as a Cyclopts leaf command flattening its ``Params``.

    The flatten rides ``BaseParams``' inherited ``@Parameter(name="*")`` (``__cyclopts__``), so the leaf
    needs no ``Annotated`` wrapper — only a runtime ``__annotate__`` (the Python 3.14 PEP 649 lazy form, not
    a direct ``__annotations__`` write) pinning the concrete ``Params`` type cyclopts resolves via
    ``inspect.signature`` (and the ``Envelope`` return that drives ``__cyclopts_returncode__``).
    The ``params: object`` static annotation is overridden at runtime, so neither checker sees the
    value-as-type binding the old ``Annotated[bind.params, …]`` form forced two suppressions for. The
    command defaults to a zero-arg ``bind.params()`` so a no-token invocation injects the param defaults.
    Identity is set directly rather than via ``functools.wraps`` — ``wraps`` would stamp a ``__wrapped__``
    pointing at the defaultless ``run`` closure, which cyclopts follows during signature inspection and
    rejects (a flatten param needs a default on the inspected signature).
    """
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

    Pre-sorts on ``b.claim.value`` (the ``StrEnum`` wire token, not the member) so each ``groupby`` run
    is contiguous and the sub-``App`` name equals the wire/dispatch key — a future enum reorder can never
    fragment a claim into two sub-apps. Each claim folds its verb rows into one sub-``App`` via ``reduce``
    (``_register`` returns the sub-app so the fold threads it), then the sub-apps fold onto the root.
    ``self-test`` attaches **after** the claim fold, outside the tree, and ``result_action="return_value"``
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
    return app


def _register(app: App, obj: App | Callable[..., object], *, name: str | None = None, help: str = "") -> App:  # noqa: A002  # cyclopts command kwarg is named "help"; mirroring the CLI surface
    """Register ``obj`` on ``app`` and return ``app`` for the fold (cyclopts ``command`` returns ``obj``).

    ``App.command`` returns the registered object, not the parent app, so a bare ``reduce`` over it
    would thread the leaf forward instead of the sub-app; this seam performs the side-effecting
    registration and returns ``app`` so the ``groupby``/sub-app folds stay point-free. ``name``/``help``
    are forwarded only when present so an ``App``-flatten registration (no name) and a named-verb leaf
    share one entry point.
    """
    match (name, help):
        case (None, _):
            app.command(obj)
        case (verb, ""):
            app.command(obj, name=verb)
        case (verb, text):
            app.command(obj, name=verb, help=text)
    return app


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["REGISTRY", "Handler", "build_app", "rail", "self_test"]
