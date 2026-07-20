# [PY_RUNTIME_FAULTS]

One fault family and one `Result`/`Option` rail span the whole branch: `BoundaryFault` is the one tagged union every package returns through — its ingress classes and the `aggregate` case keep every member structurally addressable — and `RuntimeRail` is the one `Result[T, BoundaryFault]` carrier every fallible function returns. Domain logic returns `Result`/`Option` and never raises; exceptions convert exactly once at the owning boundary, and interior code receives only the rail. Absence rides the `expression` `Option` directly — no fault-bound alias, since `Option` carries no error slot to bind.

One fault-lift core backs every application shape — the explicit-thunk `boundary`, the awaitable `async_boundary`, and the `@trapped` decorator — so the sync/async split is one coroutine-detection branch, never a parallel rail. Classification is the ordered data-driven `CLASSIFY` table, and the same conversion is the trace-egress seam: each caught exception is recorded on the active OTel span inside the one fold, the owner never minting, naming, or ending a span — span lifecycle stays with the measured operation. `latched`, the branch's one-shot install latch, and `Scope`/`SCOPES`, the one instrumentation-scope vocabulary every tracer, meter, and service literal mints from, live here because faults is the one tier below every consumer.

## [01]-[INDEX]

- [01]-[FAULT]: the closed fault family, the classification table, the rail carriers and disposition-parameterized traversal, the three fault-lift shapes, the install latch, and the instrumentation-scope vocabulary.

## [02]-[FAULT]

- Owner: `BoundaryFault` and its rail, tables, and cross-cutting tenants per the fence. Traversal splits by shape: `traversed` folds a homogeneous `Block` of already-evaluated rails under one `Disposition`, while `railed` is the bound `effect.result` builder for free-form interleaved binds whose later steps depend on earlier bound values — a variadic short-circuit collector beside them is `traversed(by=ABORT)` re-spelled and never lands.
- Cases: `config` versus `boundary` splits on who can repair the refusal — `config` carries a caller-repairable construction refusal (a policy value, roster, credential row, or precondition the same inputs deterministically refuse), while `boundary` carries the seam classification of a provider or runtime raise during work (a codec, render, parse, or engine failure a re-issue may clear), so a render-class or draw-class fault rides `boundary=` and a refused composition rides `config=` in every consumer. `wire` is reserved for explicit code-carrying construction where a numeric protocol/status code is the discriminant; a caught codec exception carries no code, so the `CLASSIFY` `msgspec` row lands it in the subject-carrying `boundary` case. A deadline-owning fence constructs `deadline` explicitly with its real budget and tripped-axis `cause`; the `CLASSIFY` `TimeoutError` row, with no budget in hand, defaults `budget` to `0.0` — the budget-unknown floor a consumer reads as unspecified, never a true zero deadline.
- Entry: the three lift shapes share one `_convert`; `catch` admits a class tuple so an engine boundary narrows over its real multi-class raise surface instead of the `Exception` catch-all, and it never widens past `Exception` — converting the `anyio` cancellation exception into a fault is the forbidden widening, cancellation being scope-owned flow control rather than an ingress class.
- Auto: `facts` is the one structured egress projection the `observability/receipts#RECEIPT` `rejected` projection spreads whole, so every leaf case carries its own `subject` inline and the receipts owner re-derives nothing.
- Packages: `expression`, `beartype`, `msgspec`, `anyio`, and `opentelemetry-api` per the fence imports; the OTel dependency is `-api` only — the owner reads the active span and never touches the SDK or a tracer mint.
- Growth: a new fault class is one `case()` with one recovery-membership row; a new exception family is one ordered `CLASSIFY` row reaching every lift shape and the trace weave; a new egress slot is one `facts` arm; a new traversal output shape is one `Disposition` member with one fold arm; a new instrumentation scope is one `Scope` member with one `SCOPES` row; a new one-shot install owner is one `@latched(...)` application.
- Boundary: no C# `Expected` clone and no exception taxonomy copied from a C# owner. Recovery keys on the fault's own `FaultTag` and never imports `reliability/resilience#RESILIENCE` `RetryClass` — resilience depends on faults, never the reverse; the rail maps exceptions to fault classes, the policy table maps retry classes to exception sets, and the two meet only through the rail outcome.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import inspect
from collections.abc import Awaitable, Callable
from enum import StrEnum
from functools import wraps
from typing import Any, Final, Literal, assert_never, overload

import anyio
import msgspec
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Ok, Result, Some, case, effect, tag, tagged_union
from expression.collections import Block, Map
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

# --- [TYPES] ----------------------------------------------------------------------------

type FaultTag = Literal["config", "resource", "deadline", "api", "import_", "wire", "boundary", "aggregate"]
type ClassifyMarker = type[BaseException] | tuple[type[BaseException], ...] | frozenset[str]
type ClassifyRow = tuple[ClassifyMarker, Callable[[str, BaseException], "BoundaryFault"]]
type RuntimeRail[T] = Result[T, "BoundaryFault"]
type Trapped[**P, T] = Callable[P, RuntimeRail[T]] | Callable[P, Awaitable[RuntimeRail[T]]]


class Disposition(StrEnum):
    ABORT = "abort"
    ACCUMULATE = "accumulate"
    PARTITION = "partition"


class Scope(StrEnum):
    WIRE = "wire"
    METER = "meter"
    SERVICE = "service"
    RESILIENCE = "resilience"
    IDENTITY = "identity"
    EVIDENCE = "evidence"
    RECIPE = "recipe"


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class BoundaryFault:
    tag: FaultTag = tag()
    config: tuple[str, str] = case()
    resource: tuple[str, str] = case()
    deadline: tuple[str, float, str] = case()  # (subject, budget, cause) — cause keeps the per-signal/per-axis identity classification would erase
    api: tuple[str, str] = case()
    import_: tuple[str, str] = case()
    wire: tuple[str, int] = case()
    boundary: tuple[str, str] = case()
    aggregate: tuple["BoundaryFault", ...] = case()

    @staticmethod
    def of(subject: str, cause: BaseException) -> "BoundaryFault":
        # catch-all keeps `str(cause)` (the message is its discriminant); CLASSIFY rows keep the type name — their tag IS the type.
        matched = CLASSIFY.choose(lambda row: Some(row[1](subject, cause)) if _hits(row[0], cause) else Nothing)
        return matched.try_head().default_with(lambda: BoundaryFault(boundary=(subject, str(cause) or type(cause).__name__)))

    @staticmethod
    def combine(left: "BoundaryFault", right: "BoundaryFault") -> "BoundaryFault":
        match (left, right):
            case (BoundaryFault(tag="aggregate"), BoundaryFault(tag="aggregate")):
                return BoundaryFault(aggregate=(*left.aggregate, *right.aggregate))
            case (BoundaryFault(tag="aggregate"), _):
                return BoundaryFault(aggregate=(*left.aggregate, right))
            case (_, BoundaryFault(tag="aggregate")):
                return BoundaryFault(aggregate=(left, *right.aggregate))
            case _:
                return BoundaryFault(aggregate=(left, right))

    def recoverable(self, codes: frozenset[FaultTag]) -> bool:
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return any(member.recoverable(codes) for member in members)
            case _:
                return self.tag in codes

    def facts(self) -> dict[str, object]:
        # `budget`/`code` ride as native scalars the receipts `EventDict` renderer serializes; a pre-`str()` coerce erases comparability.
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return {"tag": "aggregate", "subject": "aggregate", "members": ",".join(m.tag for m in members)}
            case BoundaryFault(tag="deadline", deadline=(subject, budget, cause)):
                return {"tag": "deadline", "subject": subject, "budget": budget, "cause": cause}
            case BoundaryFault(tag="wire", wire=(subject, code)):
                return {"tag": "wire", "subject": subject, "code": code}
            case (
                BoundaryFault(tag=tag, config=(subject, detail))
                | BoundaryFault(tag=tag, resource=(subject, detail))
                | BoundaryFault(tag=tag, api=(subject, detail))
                | BoundaryFault(tag=tag, import_=(subject, detail))
                | BoundaryFault(tag=tag, boundary=(subject, detail))
            ):
                return {"tag": tag, "subject": subject, "detail": detail}
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# row order is load-bearing: `TimeoutError` subclasses `OSError`, so the `deadline` row must precede the `resource` row
# or the first-match fold coalesces a timeout into `resource`, and the asyncssh terminal rows precede the channel row
# because `HostKeyNotVerifiable`/`PermissionDenied` SUBCLASS `DisconnectError`. A frozenset row matches by MODULE-QUALIFIED
# qualname over the MRO — a gated executor's death (loky/pebble pool markers) and a gated SSH channel's death classify with
# zero provider imports at this BASE tier, and the defining-module anchor keeps an unrelated class re-using a provider's
# bare name from classifying; a builtin would spell `builtins.<Name>`, but every builtin here rides its own class row.
CLASSIFY: Final[Block[ClassifyRow]] = Block.of_seq([
    (TimeoutError, lambda subject, cause: BoundaryFault(deadline=(subject, 0.0, str(cause) or type(cause).__name__))),
    ((msgspec.ValidationError, msgspec.DecodeError), lambda subject, cause: BoundaryFault(boundary=(subject, type(cause).__name__))),
    (BeartypeCallHintViolation, lambda subject, cause: BoundaryFault(api=(subject, type(cause).__name__))),
    (ImportError, lambda subject, cause: BoundaryFault(import_=(subject, type(cause).__name__))),
    (
        # caller-repairable refusals land `config`: a kernel nesting pools past LOKY_MAX_DEPTH, and the asyncssh trust and
        # credential rows a re-issue never clears — the fleet counterpart of a bad policy value.
        frozenset({"loky.process_executor.LokyRecursionError", "asyncssh.misc.HostKeyNotVerifiable", "asyncssh.misc.PermissionDenied"}),
        lambda subject, cause: BoundaryFault(config=(subject, type(cause).__name__)),
    ),
    (
        # pool deaths: loky mints its own subclass tree while pebble surfaces the STDLIB BrokenProcessPool, so both
        # spellings ride the row and the interpreter-pool death matches its concurrent.futures home.
        frozenset({
            "loky.process_executor.TerminatedWorkerError",
            "loky.process_executor.BrokenProcessPool",
            "loky.process_executor.ShutdownExecutorError",
            "concurrent.futures.process.BrokenProcessPool",
            "concurrent.futures.interpreter.BrokenInterpreterPool",
            "pebble.common.types.ProcessExpired",
        }),
        lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__)),
    ),
    (
        # asyncssh channel deaths — the remote arm's worker-death names, classified at the same bar as the local pools.
        frozenset({"asyncssh.misc.DisconnectError", "asyncssh.misc.ChannelOpenError"}),
        lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__)),
    ),
    (
        (
            anyio.BrokenWorkerProcess,
            anyio.BrokenWorkerInterpreter,
            anyio.BrokenResourceError,
            anyio.ClosedResourceError,
            anyio.ConnectionFailed,
            OSError,
        ),
        lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__)),
    ),
])

# one shared domain BeartypeConf: binding it makes every contract violation raise the canonical
# BeartypeCallHintViolation the CLASSIFY `api` row folds onto the rail, so no adapter re-catches inline.
FAULT_CONF: Final[BeartypeConf] = BeartypeConf(violation_type=BeartypeCallHintViolation)

# consumers mint handles from rows — trace.get_tracer(SCOPES[Scope.WIRE]) — never a per-page literal.
SCOPES: Final[Map[Scope, str]] = Map.of_seq([
    (Scope.WIRE, "rasm.wire"),
    (Scope.METER, "rasm.runtime"),
    (Scope.SERVICE, "rasm.companion"),
    (Scope.RESILIENCE, "rasm.runtime.resilience"),
    (Scope.IDENTITY, "rasm.runtime.identity"),
    (Scope.EVIDENCE, "rasm.runtime.evidence"),
    (Scope.RECIPE, "rasm.runtime.recipe"),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _hits(marker: ClassifyMarker, cause: BaseException) -> bool:
    match marker:
        case frozenset() as names:
            # module-qualified qualname over the MRO — isinstance semantics for a provider class this tier never
            # imports, the defining-module anchor rejecting an unrelated class that re-uses a provider's bare name;
            # resilience's `_transient` retry predicate carries the same dotted-spelling convention.
            return any(f"{klass.__module__}.{klass.__qualname__}" in names for klass in type(cause).__mro__)
        case classes:
            return isinstance(cause, classes)


def _convert(subject: str, cause: BaseException) -> BoundaryFault:
    fault = BoundaryFault.of(subject, cause)
    span = trace.get_current_span()
    if span.is_recording():
        # `escaped` stays the default `False`: converted to `Error(fault)` at this fence, the exception never escapes the span scope.
        span.record_exception(cause, attributes={"rasm.fault.tag": fault.tag, "rasm.fault.subject": subject})
        span.set_status(Status(StatusCode.ERROR, fault.tag))
    return fault


def _guard[T](subject: str, thunk: Callable[[], T], catch: type[BaseException] | tuple[type[BaseException], ...]) -> RuntimeRail[T]:
    try:
        return Ok(thunk())
    except catch as cause:
        return Error(_convert(subject, cause))


@beartype(conf=FAULT_CONF)
def boundary[T](subject: str, thunk: Callable[[], T], *, catch: type[BaseException] | tuple[type[BaseException], ...] = Exception) -> RuntimeRail[T]:
    return _guard(subject, thunk, catch)


@beartype(conf=FAULT_CONF)
async def async_boundary[T](
    subject: str, thunk: Callable[[], Awaitable[T]], *, catch: type[BaseException] | tuple[type[BaseException], ...] = Exception
) -> RuntimeRail[T]:
    try:
        return Ok(await thunk())
    except catch as cause:
        return Error(_convert(subject, cause))


def trapped[**P, T](
    subject: str, *, catch: type[BaseException] | tuple[type[BaseException], ...] = Exception
) -> Callable[[Callable[P, T] | Callable[P, Awaitable[T]]], Trapped[P, T]]:
    def decorate(fn: Callable[P, T] | Callable[P, Awaitable[T]]) -> Trapped[P, T]:
        if inspect.iscoroutinefunction(fn):

            @wraps(fn)
            async def awaited(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
                return await async_boundary(subject, lambda: fn(*args, **kwargs), catch=catch)

            return awaited

        @wraps(fn)
        def called(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
            return _guard(subject, lambda: fn(*args, **kwargs), catch)

        return called

    return decorate


@overload
def traversed[T](rails: Block[RuntimeRail[T]], *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ...) -> RuntimeRail[Block[T]]: ...
@overload
def traversed[T](rails: Block[RuntimeRail[T]], *, by: Literal[Disposition.PARTITION]) -> RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]: ...
def traversed[T](
    rails: Block[RuntimeRail[T]], *, by: Disposition = Disposition.ABORT
) -> RuntimeRail[Block[T]] | RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]:
    # overloads carry the per-disposition output shape so a caller narrows on the `Disposition` literal it passes,
    # never on the runtime union; only `PARTITION` widens the Ok arm to the `(values, faults)` tuple.
    match by:
        case Disposition.ABORT:
            seed: RuntimeRail[Block[T]] = Ok(Block.empty())
            return rails.fold(lambda acc, rail: acc.bind(lambda done: rail.map(lambda value: done.append(Block.singleton(value)))), seed)
        case Disposition.ACCUMULATE | Disposition.PARTITION:
            values, faults = rails.choose(lambda rail: rail.to_option()), rails.choose(lambda rail: rail.swap().to_option())
            if by is Disposition.PARTITION:
                # total by construction — the split cannot fail; the uniform RuntimeRail return keeps one overloaded surface.
                return Ok((values, faults))
            return Ok(values) if faults.try_head().is_none() else Error(faults.reduce(BoundaryFault.combine))
        case _ as unreachable:
            assert_never(unreachable)


def latched[R, **P](
    read: Callable[[], R | None], write: Callable[[R], None], reentrant: Callable[[R], R]
) -> Callable[[Callable[P, R]], Callable[P, R]]:
    # mint once, restamp the prior receipt on re-entry; consumers inject a `msgspec.structs.replace`-built `reentrant` closure.
    def aspect(mint: Callable[P, R]) -> Callable[P, R]:
        @wraps(mint)
        def guarded(*args: P.args, **kwargs: P.kwargs) -> R:
            match read():
                case None:
                    write(receipt := mint(*args, **kwargs))
                    return receipt
                case prior:
                    return reentrant(prior)

        return guarded

    return aspect


# one _TSource types both the per-yield bind and the return_ payload, so the leaf element erases through Any.
railed = effect.result[Any, BoundaryFault]()
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
