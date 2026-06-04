"""Aspect-oriented composition spine: the sole seam where rail/engine touch structlog, opentelemetry, stamina, beartype.

Four slot-ordered weave factories (``checked``/``logged``/``traced``/``retried``) feed three
weavers: ``assemble`` folds ``Layer``s into a monotonic-``Slot`` chain, ``compose`` builds the
rail-facing ``Hom -> Hom`` stack (rejects ``retried``), and ``compose_spawn`` lifts the
engine-only ``retried`` ``Spawn -> Spawn`` layer (rejects ``logged``).

One correlation context threads end to end: ``logged`` binds it on the rail; ``traced`` runs at
both seams and outside the engine ``retried`` loop, re-binding ``run_id`` plus the fleet
``agent_task_id`` so the engine seam (which forbids ``logged``) still carries both, then mirrors
the bound context into OTel baggage. The module-scope ``stamina`` hook reads it back through the
populated context-vars, closing the baggage <-> contextvars <-> hook loop at the engine seam so
every scheduled retry and child span carries the run and driving-agent task id.
"""

from collections import deque
from collections.abc import Callable, Coroutine, Mapping
from contextvars import ContextVar
from dataclasses import dataclass
from enum import IntEnum
from functools import wraps
import inspect
from operator import itemgetter
from typing import assert_never, TYPE_CHECKING

from beartype import beartype, BeartypeConf, BeartypeStrategy
from expression import Ok, Result
from expression.collections import block
from opentelemetry import baggage, context, trace
from opentelemetry.trace import Status, StatusCode
import stamina
from stamina.instrumentation import set_on_retry_hooks
import structlog
from structlog.contextvars import bound_contextvars, get_contextvars

from tools.assay.core.model import Fault, ResourceBusyError  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from expression.collections import Block
    from opentelemetry.trace import Span
    from stamina.instrumentation import RetryDetails
    from structlog.typing import EventDict


# --- [TYPES] ----------------------------------------------------------------------------

type Attrs = Mapping[str, object]
type Hook = Callable[[Exception], bool]
type Bind[**P] = Callable[P, Mapping[str, object]]
type Hom[**P, T] = Callable[P, Result[T, Fault]]
type Spawn[**P, T] = Callable[P, Coroutine[None, None, T]]
type Layer[**P, T] = tuple[Slot, Callable[[Hom[P, T]], Hom[P, T]]]
type SpawnLayer[**P, T] = tuple[Slot, Callable[[Spawn[P, T]], Spawn[P, T]]]


class Slot(IntEnum):
    """Weave order: ``checked`` outermost, ``retried`` innermost; the fold sorts by rank."""

    checked = 0
    logged = 1
    traced = 2
    retried = 3


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Inversion(Exception):  # noqa: N818  # surfaced via TypeError, not an *Error leaf
    """A non-monotonic ``assemble`` fold or a mis-slotted ``compose_spawn``.

    ``outer`` already wove at ``depth``; ``inner`` would weave at an equal-or-lower slot, inverting
    runtime order. Surfaced as the cause of a decoration-time ``TypeError`` — never crosses a
    runtime seam as a ``Result`` value.
    """

    outer: Slot
    inner: Slot
    depth: int


# --- [CONSTANTS] ------------------------------------------------------------------------

_CONF = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1)
_TRACER = trace.get_tracer("assay.core")
_LOG = structlog.get_logger("assay")
_ATTR_CAP = 256
# method NAME, resolved per-call via getattr so the lazy proxy binds the CONFIGURED chain: capturing
# _LOG.error/.info here would freeze the import-time default (stdout/ConsoleRenderer) before
# __init__._configure runs — corrupting the sole-stdout wire and starving ring_processor on the Fault terminal.
_TERMINAL: dict[bool, str] = {True: "error", False: "info"}
_RING: ContextVar[deque[str] | None] = ContextVar(
    "assay_ring", default=None
)  # invocation-scoped recent-events ring; set ONCE by registry.rail, appended in place so it survives the anyio.run boundary, read by registry._emit


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transient(exc: Exception) -> bool:
    """Retry predicate: transport faults retry, a held lease never does.

    A contended lease (``ResourceBusyError`` -> ``BUSY``) is a routing fact, not flaky transport,
    so it short-circuits to ``False`` before the transport arm.
    """
    match exc:
        case ResourceBusyError():
            return False
        case ConnectionError() | TimeoutError() | BrokenPipeError() | OSError():
            return True
        case _:
            return False


def _correlate(projected: Attrs) -> dict[str, str]:
    """Normalize wire attrs into structlog/baggage contextvar identifiers, dropping empties.

    Both seams' projections — the rail's ``run_id``/``strict`` and the engine's ``assay.run_id``
    plus the fleet ``agent_context`` (``run.id``/``agent.task.id``) — collapse here: the ``assay.``
    wire prefix is stripped and OTel-style dots become ``_`` so ``agent.task.id`` binds as the
    valid ``agent_task_id`` context-var that ``_on_retry`` reads back. Blank values (an unset
    ``agent_task_id``) are dropped so absent agent context binds nothing extra.
    """
    return {key.removeprefix("assay.").replace(".", "_"): str(val) for key, val in projected.items() if str(val)}


def ring_processor(logger: object, method_name: str, event_dict: EventDict) -> EventDict:  # noqa: ARG001  # mandated structlog Processor signature: (logger, method_name, event_dict)
    """A structlog ``Processor`` (not a ``Slot``/``Layer``): append a bounded summary to the live ring.

    The auto-observability seam: when ``registry.rail`` has seated a ``deque`` into ``_RING`` for the
    current invocation, every log line passing the chain appends a compact ``level:event`` summary in
    place and the ``event_dict`` returns UNCHANGED (pure pass-through, never mutated). The deque is set
    ONCE per invocation and appended by-reference, so the bounded recent-events window survives the
    ``anyio.run`` boundary and ``registry._emit`` reads it back at envelope time. Absent a ring
    (``_RING.get() is None``) this is a zero-cost identity processor — it does not touch the 4-slot stack.
    """
    match _RING.get():
        case deque() as ring:
            ring.append(f"{event_dict.get('log_level', method_name)}:{event_dict.get('event', '')}")
        case None:
            pass
    return event_dict


def ring_recent() -> tuple[str, ...]:
    """Snapshot the current invocation's recent-events ring; empty when no ring is seated."""
    match _RING.get():
        case deque() as ring:
            return tuple(ring)
        case None:
            return ()


def _on_retry(details: RetryDetails) -> None:
    """Module-scope stamina hook closing the baggage<->contextvars<->hook loop.

    Reads the context bound by ``traced`` (``get_contextvars``) — which at the engine seam runs
    outside the ``retried`` loop and binds ``run_id`` plus the fleet ``agent_task_id`` even where
    ``logged`` is forbidden — so each scheduled retry logs under the same ``run_id``/``agent_task_id``
    as its spawn.
    """
    _LOG.warning(
        "retry.scheduled", attempt=details.retry_num, wait_for=details.wait_for, caused_by=type(details.caused_by).__name__, **get_contextvars()
    )


set_on_retry_hooks([_on_retry])


def _once[F: Callable[..., object]](dec: Callable[[F], F]) -> Callable[[F], F]:
    """Idempotency guard keyed on ``id(dec)`` so double-composition of one factory is a no-op.

    Distinct factory instances yield distinct tags; dedup holds only for the *same* instance,
    so callers wire each factory once at module scope (see the registry/engine seams).
    """
    tag = id(dec)

    @wraps(dec)
    def guard(fn: F) -> F:
        ids: frozenset[int] = getattr(fn, "_assay_ids", frozenset())
        match tag in ids:
            case True:
                return fn
            case False:
                woven = dec(fn)
                woven._assay_ids = ids | {tag}  # type: ignore[attr-defined]  # noqa: SLF001  # idempotency marker on the wrapper
                return woven

    return guard


def assemble[**P, T](layers: Block[Layer[P, T]], fn: Hom[P, T]) -> Result[Hom[P, T], Inversion]:
    """Fold ``Layer``s onto ``fn`` under monotonic ``Slot`` order, short-circuiting on inversion.

    Threads ``(slot, depth, woven)`` through ``Block.fold`` seeded at ``Slot.checked`` so the first
    real layer is trivially monotonic; ``filter_with`` rejects a slot not ``>=`` the accumulated
    slot, naming the exact outer/inner pair — cheaper than a post-hoc sort that would silently
    repair caller error.
    """
    seed: Result[tuple[Slot, int, Hom[P, T]], Inversion] = Ok((Slot.checked, 0, fn))
    return layers.fold(
        lambda acc, lyr: acc.bind(
            lambda st: Ok((lyr[0], st[1] + 1, _once(lyr[1])(st[2]))).filter_with(lambda _: lyr[0] >= st[0], lambda _: Inversion(st[0], lyr[0], st[1]))
        ),
        seed,
    ).map(itemgetter(2))


def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    """Build the rail-facing ``Hom -> Hom`` stack; ``Slot.retried`` is filtered before assembly.

    The rail seam can never weave a spawn layer; an ``Inversion`` from a mis-ordered caller becomes
    a decoration-time ``TypeError`` so the error is structural, not a runtime surprise.
    """
    homs = block.of_seq(tuple(lyr for lyr in layers if lyr[0] is not Slot.retried))

    def weave(fn: Hom[P, T]) -> Hom[P, T]:
        match assemble(homs, fn):
            case Result(tag="ok", ok=woven):
                return woven
            case Result(error=inv):
                raise TypeError(inv)
            case never:  # pragma: no cover
                assert_never(never)

    return weave


def compose_spawn[**P, T](layer: SpawnLayer[P, T]) -> Callable[[Spawn[P, T]], Spawn[P, T]]:
    """Lift the engine-only ``retried`` ``Spawn -> Spawn`` layer; rejects any other slot.

    A non-``retried`` slot is a decoration-time ``TypeError`` carrying the offending slot, so the
    inversion fails loud at wiring time. Returns the deferred weaver (not ``layer[1](fn)``) because
    only the two-step application lets ty specialize ``retried()``'s **P/T from the concrete spawn —
    ty resolves a no-arg generic factory at the *application* site, never against a declared target.
    """
    match layer[0]:
        case Slot.retried:
            return layer[1]
        case slot:
            raise TypeError(Inversion(Slot.retried, slot, 0))


def checked[**P, T](*, conf: BeartypeConf = _CONF) -> Layer[P, T]:
    """Slot 0 (beartype): a ``@wraps``-preserved shape boundary, idempotent per factory instance."""
    return (Slot.checked, _once(beartype(conf=conf)))


def logged[**P, T](*, event: str, keys: Bind[P]) -> Layer[P, T]:
    """Slot 1 (structlog, rail only): bind correlation context, then terminal on the ``Result``.

    ``bound_contextvars`` enters before the call and auto-exits after, so every nested log on the
    rail flows under the same context (the engine seam binds its own via ``traced``). Terminal
    dispatch keys on the severity predicate (``f.status.severity >= FAILED.severity``).
    """

    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with bound_contextvars(**keys(*a, **k)):
                res = fn(*a, **k)
                match res:
                    case Result(tag="ok", ok=done):
                        _LOG.info(f"{event}.finish", status=getattr(done, "status", RailStatus.OK))
                    case Result(error=f):
                        getattr(_LOG, _TERMINAL[f.status.severity >= RailStatus.FAILED.severity])(
                            f"{event}.finish", status=f.status, message=f.message, argv=f.argv
                        )
                return res  # ty: ignore[invalid-return-type]  # mypy strict needs the Result[T] annotation; @wraps re-scopes that T under ty — the rail is identical

        return woven  # ty: ignore[invalid-return-type]  # @wraps over expression.Result yields ty's _Wrapped re-scope; mypy --strict accepts the Hom passthrough

    return (Slot.logged, dec)


def _stamp[T](s: Span, res: Result[T, Fault]) -> Result[T, Fault]:
    """Map the awaited/returned ``Result`` onto the live span's attributes + status; returns ``res`` unchanged."""
    match res:
        case Result(tag="ok", ok=done):
            s.set_attribute("assay.status", str(getattr(done, "status", RailStatus.OK)))
            s.set_status(Status(StatusCode.OK))
        case Result(error=f):
            s.set_attributes({"assay.status": f.status, "assay.message": f.message[:_ATTR_CAP]})
            s.set_status(Status(StatusCode.ERROR, f.status))
    return res


def traced[**P, T](*, span: str, attrs: Callable[P, Attrs], agent: Callable[P, Attrs] | None = None) -> Layer[P, T]:
    """Slot 2 (opentelemetry, both seams): one span per call, status mapped from the awaited/returned ``Result``.

    ``dec`` discriminates on ``inspect.iscoroutinefunction(fn)`` — one factory owning both modalities. The
    rail's synchronous ``Hom`` returns its ``Result`` inside the span; the engine seam's async ``Spawn`` is
    *awaited* inside the span, so the span lifetime, the contextvar/baggage bind, and the ``_stamp`` status
    mapping wrap the real work — not coroutine creation — and the engine's ``_diagnose`` enriches THIS span
    via ``get_current_span``. The projected ``attrs`` (and the optional fleet ``agent`` ``{run.id,
    agent.task.id}`` pair) are merged, ``_correlate``-normalized, and bound into ``structlog`` context-vars +
    OTel baggage around the call, so downstream child spans link and ``stamina``'s ``_on_retry`` reads a
    populated ``run_id``/``agent_task_id`` during the await even where ``logged`` is forbidden.
    ``context.detach`` runs in a ``finally`` *after* the span context-manager exits: the span's ``__exit__``
    re-installs the context captured on entry, so a detach nested inside it would violate strict-LIFO and
    leak baggage. ``RailStatus`` is a ``StrEnum`` so it serializes as its wire token into span attributes.
    """

    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            projected = {**attrs(*a, **k), **(agent(*a, **k) if agent is not None else {})}
            with bound_contextvars(**_correlate(projected)):
                ctx = block.of_seq(tuple(get_contextvars().items())).fold(
                    lambda c, kv: baggage.set_baggage(kv[0], kv[1], context=c), context.get_current()
                )
                token = context.attach(ctx)
                try:
                    with _TRACER.start_as_current_span(span) as s:
                        s.set_attributes({key: str(val) for key, val in projected.items()})
                        return _stamp(s, fn(*a, **k))  # ty: ignore[invalid-return-type]  # ty re-scopes nested _stamp[T] vs traced's T; mypy unifies them
                finally:
                    context.detach(token)

        @wraps(fn)
        async def awoven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            projected = {**attrs(*a, **k), **(agent(*a, **k) if agent is not None else {})}
            with bound_contextvars(**_correlate(projected)):
                ctx = block.of_seq(tuple(get_contextvars().items())).fold(
                    lambda c, kv: baggage.set_baggage(kv[0], kv[1], context=c), context.get_current()
                )
                token = context.attach(ctx)
                try:
                    with _TRACER.start_as_current_span(span) as s:
                        s.set_attributes({key: str(val) for key, val in projected.items()})
                        return _stamp(s, await fn(*a, **k))  # type: ignore[misc]  # ty: ignore[invalid-await]
                finally:
                    context.detach(token)

        # awoven is the async Spawn woven (engine seam); both wovens are the @wraps Hom re-scope compose already suppresses.
        return awoven if inspect.iscoroutinefunction(fn) else woven  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    return (Slot.traced, dec)


def retried[**P, T](*, on: Hook = _transient, attempts: int = 3, timeout: float = 30.0) -> SpawnLayer[P, T]:
    """Slot 3 (stamina, engine only): ``Spawn -> Spawn`` exponential backoff on raised faults.

    Retries on the exception channel (no ``Result`` wrapper); ``_transient`` returns ``False`` for
    ``ResourceBusyError`` so a held lease (``BUSY``) and a deadline (``TIMEOUT``) never re-attempt.
    """
    return (Slot.retried, _once(stamina.retry(on=on, attempts=attempts, timeout=timeout)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Attrs",
    "Bind",
    "Hom",
    "Hook",
    "Inversion",
    "Layer",
    "Slot",
    "Spawn",
    "SpawnLayer",
    "_RING",
    "assemble",
    "checked",
    "compose",
    "compose_spawn",
    "logged",
    "retried",
    "ring_processor",
    "ring_recent",
    "traced",
]
