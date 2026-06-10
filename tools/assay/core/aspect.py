"""Aspect-layer wiring for assay rail and engine boundaries.

Provides the Slot ordering contract, assemble/compose combinators for stacking
layers in slot order, and three factory functions -- checked, logged, traced --
that cover runtime shape validation, structured logging with ring capture, and
OpenTelemetry tracing with baggage propagation.  The ring_processor structlog
processor and ring_recent snapshot utility expose the per-context event ring
used by engine failure reports.
"""

from collections import deque
from collections.abc import Callable, Mapping
from contextvars import ContextVar
from dataclasses import dataclass
from enum import IntEnum
from functools import wraps
import inspect
from operator import itemgetter
from typing import assert_never, Final, TYPE_CHECKING

from beartype import beartype, BeartypeConf, BeartypeStrategy
from expression import Ok, Result
from expression.collections import block
from opentelemetry import baggage, context, trace
from opentelemetry.trace import Status, StatusCode
import structlog
from structlog.contextvars import bound_contextvars, get_contextvars

from tools.assay.core.model import Fault
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from expression.collections import Block
    from opentelemetry.trace import Span
    from structlog.typing import EventDict


# --- [TYPES] ----------------------------------------------------------------------------

type Attrs = Mapping[str, object]
type Bind[**P] = Callable[P, Mapping[str, object]]
type Hom[**P, T] = Callable[P, Result[T, Fault]]


class Slot(IntEnum):
    """Ordered priority slots for aspect layers; lower value wraps the function first.

    checked is innermost, traced is outermost.  assemble enforces monotonic
    non-decreasing order and raises Inversion on regression.
    """

    checked = 0
    logged = 1
    traced = 2


type Layer[**P, T] = tuple[Slot, Callable[[Hom[P, T]], Hom[P, T]]]


# --- [CONSTANTS] ------------------------------------------------------------------------

_ATTR_CAP = 256
_CONF = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1)
_RING: ContextVar[deque[str] | None] = ContextVar("assay_ring", default=None)


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Inversion(Exception):  # noqa: N818  # surfaced via TypeError, not an *Error leaf
    """Decoration-time layer ordering failure."""

    outer: Slot
    inner: Slot
    depth: int


# --- [SERVICES] -------------------------------------------------------------------------

_LOG: Final = structlog.get_logger("assay")
_TRACER: Final = trace.get_tracer("assay.core")


# --- [OPERATIONS] -----------------------------------------------------------------------


def ring_processor(logger: object, method_name: str, event_dict: EventDict) -> EventDict:  # noqa: ARG001
    """Append one structlog event summary to the active ring and inject trace correlation fields.

    Conforms to the structlog processor signature; logger and method_name are
    required by the protocol but unused.  When a valid OpenTelemetry span is
    active, trace_id and span_id are added to the event dict so log records
    correlate with traces without a separate log-record processor.

    Returns:
        The event dict, mutated in-place when a valid span is active.
    """
    match _RING.get():
        case deque() as ring:
            ring.append(f"{event_dict.get('level', method_name)}:{event_dict.get('event', '')}")
        case None:
            pass
    span = trace.get_current_span()
    ctx = span.get_span_context()
    match ctx.is_valid:
        case True:
            event_dict["trace_id"] = f"{ctx.trace_id:032x}"
            event_dict["span_id"] = f"{ctx.span_id:016x}"
        case False:
            pass
    return event_dict


def ring_recent() -> tuple[str, ...]:
    """Snapshot the active recent-events ring for the current context variable.

    Returns:
        Ordered event summaries, or an empty tuple when no ring is active.
    """
    match _RING.get():
        case deque() as ring:
            return tuple(ring)
        case None:
            return ()


def _once[**P, T](dec: Callable[[Callable[P, T]], Callable[P, T]]) -> Callable[[Callable[P, T]], Callable[P, T]]:
    # Idempotency is per decorator instance; callers wire factories once at module scope.
    tag = id(dec)

    @wraps(dec)
    def guard(fn: Callable[P, T]) -> Callable[P, T]:
        ids: frozenset[int] = getattr(fn, "_assay_ids", frozenset())
        match tag in ids:
            case True:
                return fn
            case False:
                woven = dec(fn)
                # idempotency marker on the wrapper
                woven._assay_ids = ids | {tag}  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]  # noqa: SLF001
                return woven

    return guard


def checked_call[**P, T](fn: Hom[P, T], *, conf: BeartypeConf = _CONF) -> Hom[P, T]:
    """Apply idempotent runtime shape validation to one rail function.

    Returns:
        The function with beartype argument and return validation applied;
        calling this a second time with the same decorator instance is a no-op.
    """
    return _once(beartype(conf=conf))(fn)


def assemble[**P, T](layers: Block[Layer[P, T]], fn: Hom[P, T]) -> Result[Hom[P, T], Inversion]:
    """Fold ordered layers onto a rail function, enforcing monotonic slot progression.

    Returns:
        Ok carrying the woven function, or Error(Inversion) when a layer's slot
        is less than the preceding slot.
    """
    seed: Result[tuple[Slot, int, Hom[P, T]], Inversion] = Ok((Slot.checked, 0, fn))
    return layers.fold(
        lambda acc, lyr: acc.bind(
            lambda st: Ok((lyr[0], st[1] + 1, _once(lyr[1])(st[2]))).filter_with(lambda _: lyr[0] >= st[0], lambda _: Inversion(st[0], lyr[0], st[1]))
        ),
        seed,
    ).map(itemgetter(2))


def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    """Build a decorator that applies the given layers in slot order.

    Returns:
        A decorator whose application raises TypeError (wrapping Inversion)
        when any layer's slot is less than the preceding layer's slot.
    """
    homs = block.of_seq(layers)

    def weave(fn: Hom[P, T]) -> Hom[P, T]:
        match assemble(homs, fn):
            case Result(tag="ok", ok=woven):
                return woven
            case Result(error=inv):
                raise TypeError(inv)
            case never:  # pragma: no cover
                assert_never(never)

    return weave


def checked[**P, T](*, conf: BeartypeConf = _CONF) -> Layer[P, T]:
    """Return a Layer tuple for the Slot.checked position."""
    return (Slot.checked, lambda fn: checked_call(fn, conf=conf))


_CHECKED_LAYER = checked()  # type: ignore[var-annotated]  # Layer[**P, T] PEP 696 vars cannot be inferred at module-level singleton instantiation


def logged[**P, T](*, event: str, keys: Bind[P]) -> Layer[P, T]:
    """Return a Layer tuple that emits structured log events at Slot.logged.

    On success, logs event.finish at info level with the result status.  On
    fault, logs at error when fault severity reaches FAILED, otherwise at info.
    The keys callable binds structlog contextvars for the duration of the call.
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
                        finish = f"{event}.finish"
                        match f.status.severity >= RailStatus.FAILED.severity:
                            case True:
                                _LOG.error(finish, status=f.status, message=f.message, argv=f.argv)
                            case False:
                                _LOG.info(finish, status=f.status, message=f.message, argv=f.argv)
                return res  # @wraps re-scopes Result[T] under ty and mypy --strict; runtime shape is correct

        return woven

    return (Slot.logged, dec)


def _correlate(projected: Attrs) -> dict[str, str]:
    return {key.removeprefix("assay.").replace(".", "_"): str(val) for key, val in projected.items() if val is not None and str(val)}


def _stamp[T](s: Span, res: Result[T, Fault]) -> Result[T, Fault]:
    match res:
        case Result(tag="ok", ok=done):
            s.set_attribute("assay.status", str(getattr(done, "status", RailStatus.OK)))
            s.set_status(Status(StatusCode.OK))
        case Result(error=f):
            s.set_attributes({"assay.status": f.status.value, "assay.message": f.message[:_ATTR_CAP]})
            s.add_event(
                "assay.fault",
                attributes={"assay.status": f.status.value, "assay.message": f.message[:_ATTR_CAP], "assay.argv": " ".join(f.argv)[:_ATTR_CAP]},
            )
            s.set_status(Status(StatusCode.ERROR, f.status.value))
    return res


def traced[**P, T](*, span: str, attrs: Callable[P, Attrs], agent: Callable[P, Attrs] | None = None) -> Layer[P, T]:
    """Return a Layer tuple that creates an OpenTelemetry span at Slot.traced.

    Projected attributes from attrs (and optional agent) are set on the span
    and propagated as OpenTelemetry baggage.  Correlation fields are injected
    into structlog contextvars so log records carry trace_id and span_id.
    Dispatches to an async wrapper when the decorated function is a coroutine.
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
                        return _stamp(s, fn(*a, **k))  # ty re-scopes nested _stamp[T] vs traced's T; mypy unifies them
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
                        # fn is a coroutine function (guarded by iscoroutinefunction) but PEP 695 ParamSpec makes it opaque to both checkers
                        return _stamp(s, await fn(*a, **k))  # type: ignore[misc]  # ty: ignore[invalid-await]
                finally:
                    context.detach(token)

        # union of awoven/woven cannot unify to Hom[P, T] under strict ParamSpec; runtime dispatch is correct
        return awoven if inspect.iscoroutinefunction(fn) else woven  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    return (Slot.traced, dec)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Attrs",
    "Bind",
    "Hom",
    "Inversion",
    "Layer",
    "Slot",
    "_CHECKED_LAYER",
    "_RING",
    "assemble",
    "checked",
    "checked_call",
    "compose",
    "logged",
    "ring_processor",
    "ring_recent",
    "traced",
]
