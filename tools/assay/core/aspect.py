"""Aspect-layer wiring for rail validation, logging, and tracing.

The event ring and trace correlation fields are context-local so fault envelopes can carry nearby telemetry without widening rail signatures.
"""

from collections import deque
from collections.abc import Callable, Mapping
from contextlib import contextmanager
from contextvars import ContextVar
from dataclasses import dataclass
from enum import IntEnum
from functools import wraps
import inspect
from operator import itemgetter
from typing import assert_never, Final, Protocol, TYPE_CHECKING

from beartype import beartype, BeartypeConf, BeartypeStrategy, BeartypeViolationVerbosity
from expression import Ok, Result
from expression.collections import block
from opentelemetry import baggage, context, trace
from opentelemetry.trace import Status, StatusCode
import structlog
from structlog.contextvars import bound_contextvars, get_contextvars

from tools.assay.core.model import Fault, RailStatus


if TYPE_CHECKING:
    from collections.abc import Iterator

    from expression.collections import Block
    from opentelemetry.trace import Span
    from structlog.typing import EventDict


# --- [TYPES] ----------------------------------------------------------------------------

type Attrs = Mapping[str, object]
type Bind[**P] = Callable[P, Mapping[str, object]]


class HasStatus(Protocol):
    """Rail carrier exposing the status that log and trace layers stamp."""

    @property
    def status(self) -> RailStatus: ...


type Hom[**P, T: HasStatus] = Callable[P, Result[T, Fault]]


class Slot(IntEnum):
    """Aspect layer order from innermost validation to outermost tracing."""

    checked = 0
    logged = 1
    traced = 2


type Layer[**P, T: HasStatus] = tuple[Slot, Callable[[Hom[P, T]], Hom[P, T]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ATTR_CAP = 256
_CONF = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1, violation_verbosity=BeartypeViolationVerbosity.MAXIMAL)
RING: ContextVar[deque[str] | None] = ContextVar("assay_ring", default=None)

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
    """Record the event in the context-local ring and stamp active trace IDs.

    Returns:
        The processor event dict; mutated only when a valid span is active.
    """
    match RING.get():
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
    """Return the active context-local event-ring snapshot.

    Returns:
        Ordered event summaries, or ``()`` when no ring is active.
    """
    match RING.get():
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
                woven._assay_ids = ids | {tag}  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]  # noqa: SLF001
                return woven

    return guard


def checked_call[**P, T: HasStatus](fn: Hom[P, T], *, conf: BeartypeConf = _CONF) -> Hom[P, T]:
    """Apply idempotent beartype validation to one rail function.

    Returns:
        The validated rail function; re-application is a no-op.
    """
    return _once(beartype(conf=conf))(fn)


def assemble[**P, T: HasStatus](layers: Block[Layer[P, T]], fn: Hom[P, T]) -> Result[Hom[P, T], Inversion]:
    """Fold ordered layers onto a rail function.

    Returns:
        ``Ok`` with the woven function, or ``Error(Inversion)`` on slot regression.
    """
    seed: Result[tuple[Slot, int, Hom[P, T]], Inversion] = Ok((Slot.checked, 0, fn))
    return layers.fold(
        lambda acc, lyr: acc.bind(
            lambda st: Ok((lyr[0], st[1] + 1, _once(lyr[1])(st[2]))).filter_with(lambda _: lyr[0] >= st[0], lambda _: Inversion(st[0], lyr[0], st[1]))
        ),
        seed,
    ).map(itemgetter(2))


def compose[**P, T: HasStatus](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    """Build a decorator from a monotonic layer sequence.

    Returns:
        Decorator that raises ``TypeError`` when layer slots regress.
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


def checked[**P, T: HasStatus](*, conf: BeartypeConf = _CONF) -> Layer[P, T]:
    """Return a Layer tuple for the Slot.checked position."""
    return (Slot.checked, lambda fn: checked_call(fn, conf=conf))


CHECKED_LAYER = checked()  # type: ignore[var-annotated]  # Layer[**P, T] PEP 696 vars cannot be inferred at module-level singleton instantiation


def logged[**P, T: HasStatus](*, event: str, keys: Bind[P]) -> Layer[P, T]:
    """Return a logging layer for rail completion events.

    Faults at ``FAILED`` severity or above log at error level; lower-severity faults and successes log at info level.

    A raised exception bypasses the ``Result`` rail entirely, so the wrapper emits a ``FAULTED`` finish event and
    re-propagates; downstream ``_guard`` owns fault classification. Type checking: ``@wraps`` re-scopes the
    ``Result[T]`` return under ty and mypy ``--strict`` even though the runtime shape is correct.
    """

    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with bound_contextvars(**keys(*a, **k)):
                try:
                    res = fn(*a, **k)
                except BaseException:
                    _LOG.error(f"{event}.finish", status=RailStatus.FAULTED, exc_info=True)
                    raise
                match res:
                    case Result(tag="ok", ok=done):
                        _LOG.info(f"{event}.finish", status=done.status)
                    case Result(error=f):
                        finish = f"{event}.finish"
                        match f.status.severity >= RailStatus.FAILED.severity:
                            case True:
                                _LOG.error(finish, status=f.status, message=f.message, argv=f.argv)
                            case False:
                                _LOG.info(finish, status=f.status, message=f.message, argv=f.argv)
                return res

        return woven

    return (Slot.logged, dec)


def _correlate(projected: Attrs) -> dict[str, str]:
    return {key.removeprefix("assay.").replace(".", "_"): str(val) for key, val in projected.items() if val is not None and str(val)}


def _stamp[T: HasStatus](s: Span, res: Result[T, Fault]) -> Result[T, Fault]:
    match res:
        case Result(tag="ok", ok=done):
            s.set_attribute("assay.status", str(done.status))
            s.set_status(Status(StatusCode.OK))
        case Result(error=f):
            s.set_attributes({"assay.status": f.status.value, "assay.message": f.message[:_ATTR_CAP]})
            s.add_event(
                "assay.fault",
                attributes={"assay.status": f.status.value, "assay.message": f.message[:_ATTR_CAP], "assay.argv": " ".join(f.argv)[:_ATTR_CAP]},
            )
            s.set_status(Status(StatusCode.ERROR, f.status.value))
    return res


def traced[**P, T: HasStatus](*, span: str, attrs: Callable[P, Attrs], agent: Callable[P, Attrs] | None = None) -> Layer[P, T]:
    """Return a tracing layer that stamps spans, baggage, and log context.

    Async callables receive an async wrapper because ParamSpec cannot expose coroutine shape through ``Hom``.
    """

    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @contextmanager
        def _scope(*a: P.args, **k: P.kwargs) -> Iterator[Span]:
            projected = {**attrs(*a, **k), **(agent(*a, **k) if agent is not None else {})}
            with bound_contextvars(**_correlate(projected)):
                ctx = block.of_seq(tuple(get_contextvars().items())).fold(
                    lambda c, kv: baggage.set_baggage(kv[0], kv[1], context=c), context.get_current()
                )
                token = context.attach(ctx)
                try:
                    with _TRACER.start_as_current_span(span) as s:
                        s.set_attributes({key: str(val) for key, val in projected.items()})
                        yield s
                finally:
                    context.detach(token)

        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with _scope(*a, **k) as s:
                return _stamp(s, fn(*a, **k))

        @wraps(fn)
        async def awoven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with _scope(*a, **k) as s:
                # iscoroutinefunction narrows runtime shape; ParamSpec remains opaque to both checkers.
                return _stamp(s, await fn(*a, **k))  # type: ignore[misc]  # ty: ignore[invalid-await]

        # Runtime coroutine dispatch is precise; strict ParamSpec cannot unify the wrapper union.
        return awoven if inspect.iscoroutinefunction(fn) else woven  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    return (Slot.traced, dec)


# --- [EXPORTS] -------------------------------------------------------------------------

__all__ = [
    "Attrs",
    "Bind",
    "Hom",
    "Inversion",
    "Layer",
    "Slot",
    "CHECKED_LAYER",
    "RING",
    "assemble",
    "checked",
    "checked_call",
    "compose",
    "logged",
    "ring_processor",
    "ring_recent",
    "traced",
]
