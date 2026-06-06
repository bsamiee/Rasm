"""Define ordered aspect layers for rail and engine boundaries."""

from collections import deque
from collections.abc import Callable, Coroutine, Mapping
from contextvars import ContextVar
from dataclasses import dataclass
from enum import IntEnum
from functools import wraps
import inspect
from operator import itemgetter
from typing import assert_never, Protocol, runtime_checkable, TYPE_CHECKING

from beartype import beartype, BeartypeConf, BeartypeStrategy
from expression import Ok, Result
from expression.collections import block
from opentelemetry import baggage, context, trace
from opentelemetry.trace import Status, StatusCode
import stamina
from stamina.instrumentation import set_on_retry_hooks
import structlog
from structlog.contextvars import bound_contextvars, get_contextvars

from tools.assay.core.model import Fault, ResourceBusyError
from tools.assay.core.status import RailStatus


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
    """Aspect layer order."""

    checked = 0
    logged = 1
    traced = 2
    retried = 3


@runtime_checkable
class _AssayLogger(Protocol):
    def info(self, event: str, **kw: object) -> object: ...

    def warning(self, event: str, **kw: object) -> object: ...

    def error(self, event: str, **kw: object) -> object: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_CONF = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1)
_TRACER = trace.get_tracer("assay.core")
_ATTR_CAP = 256
_RING: ContextVar[deque[str] | None] = ContextVar("assay_ring", default=None)


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Inversion(Exception):  # noqa: N818  # surfaced via TypeError, not an *Error leaf
    """Decoration-time layer ordering failure."""

    outer: Slot
    inner: Slot
    depth: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transient(exc: Exception) -> bool:
    # ResourceBusyError is a lease fact, not flaky transport, so it never retries.
    match exc:
        case ResourceBusyError():
            return False
        case ConnectionError() | TimeoutError() | BrokenPipeError() | OSError():
            return True
        case _:
            return False


def _log() -> _AssayLogger:
    logger: _AssayLogger = structlog.get_logger("assay")
    return logger


def _correlate(projected: Attrs) -> dict[str, str]:
    # Normalize OTel-style attrs into contextvar identifiers and drop empty values.
    return {key.removeprefix("assay.").replace(".", "_"): str(val) for key, val in projected.items() if str(val)}


def ring_processor(logger: object, method_name: str, event_dict: EventDict) -> EventDict:  # noqa: ARG001  # mandated structlog Processor signature: (logger, method_name, event_dict)
    """Append one structlog event summary to the active recent-events ring.

    Returns:
        Event dictionary passed through to the structlog processor chain.
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
    """Snapshot the active recent-events ring.

    Returns:
        Recent event summaries for the current context.
    """
    match _RING.get():
        case deque() as ring:
            return tuple(ring)
        case None:
            return ()


def _on_retry(details: RetryDetails) -> None:
    # Retry hooks read context bound outside the retried loop, including seams where logged is forbidden.
    wait_for = float(getattr(details.wait_for, "total_seconds", lambda: details.wait_for)())
    trace.get_current_span().add_event(
        "retry.scheduled",
        attributes={"retry.attempt": details.retry_num, "retry.wait_for": wait_for, "retry.cause": type(details.caused_by).__name__},
    )
    _log().warning(
        "retry.scheduled", attempt=details.retry_num, wait_for=details.wait_for, caused_by=type(details.caused_by).__name__, **get_contextvars()
    )


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
                woven._assay_ids = ids | {tag}  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]  # noqa: SLF001  # idempotency marker on the wrapper
                return woven

    return guard


def assemble[**P, T](layers: Block[Layer[P, T]], fn: Hom[P, T]) -> Result[Hom[P, T], Inversion]:
    """Apply ordered layers to a rail function.

    Returns:
        Woven function, or an inversion error when slot order regresses.
    """
    seed: Result[tuple[Slot, int, Hom[P, T]], Inversion] = Ok((Slot.checked, 0, fn))
    return layers.fold(
        lambda acc, lyr: acc.bind(
            lambda st: Ok((lyr[0], st[1] + 1, _once(lyr[1])(st[2]))).filter_with(lambda _: lyr[0] >= st[0], lambda _: Inversion(st[0], lyr[0], st[1]))
        ),
        seed,
    ).map(itemgetter(2))


def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    """Build a rail-layer decorator.

    Returns:
        Decorator that applies the non-retry layers in slot order.
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
    """Build an engine spawn-layer decorator.

    Returns:
        Decorator that applies checked, traced, and retry spawn layers.

    Raises:
        TypeError: The layer is not a retry slot.

    """
    match layer[0]:
        case Slot.retried:
            return layer[1]
        case slot:
            raise TypeError(Inversion(Slot.retried, slot, 0))


def checked[**P, T](*, conf: BeartypeConf = _CONF) -> Layer[P, T]:
    """Create the runtime shape-validation layer.

    Returns:
        Layer tuple for the checked slot.
    """
    return (Slot.checked, _once(beartype(conf=conf)))


def logged[**P, T](*, event: str, keys: Bind[P]) -> Layer[P, T]:
    """Create the rail logging layer.

    Returns:
        Layer tuple for the logged slot.
    """

    def dec(fn: Hom[P, T]) -> Hom[P, T]:
        @wraps(fn)
        def woven(*a: P.args, **k: P.kwargs) -> Result[T, Fault]:
            with bound_contextvars(**keys(*a, **k)):
                res = fn(*a, **k)
                match res:
                    case Result(tag="ok", ok=done):
                        # T is unbounded, so getattr is the generic status projection.
                        _log().info(f"{event}.finish", status=getattr(done, "status", RailStatus.OK))
                    case Result(error=f):
                        logger = _log()
                        finish = f"{event}.finish"
                        match f.status.severity >= RailStatus.FAILED.severity:
                            case True:
                                logger.error(finish, status=f.status, message=f.message, argv=f.argv)
                            case False:
                                logger.info(finish, status=f.status, message=f.message, argv=f.argv)
                return res  # ty: ignore[invalid-return-type]  # mypy strict needs the Result[T] annotation; @wraps re-scopes that T under ty: the rail is identical

        return woven  # ty: ignore[invalid-return-type]  # @wraps over expression.Result yields ty's _Wrapped re-scope; mypy --strict accepts the Hom passthrough

    return (Slot.logged, dec)


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
    """Create the tracing layer for sync rail and async spawn functions.

    Returns:
        Layer tuple for the traced slot.
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

        # awoven is the async spawn boundary; compose owns the @wraps Hom re-scope suppression.
        return awoven if inspect.iscoroutinefunction(fn) else woven  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    return (Slot.traced, dec)


def retried[**P, T](*, on: Hook = _transient, attempts: int = 3, timeout: float = 30.0) -> SpawnLayer[P, T]:
    """Create the engine retry layer.

    Returns:
        Spawn layer tuple for the retried slot.
    """
    return (Slot.retried, _once(stamina.retry(on=on, attempts=attempts, timeout=timeout)))


# --- [COMPOSITION] ----------------------------------------------------------------------

set_on_retry_hooks([_on_retry])


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
