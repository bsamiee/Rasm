# [PY_RUNTIME_FAULTS]

The single fault family and Result/Option rail for the whole branch. `BoundaryFault` is the one tagged union every package returns through, discriminating the seven ingress classes and carrying an `aggregate` case so an accumulating boundary keeps every member structurally addressable. `RuntimeRail` is the one `Result[T, BoundaryFault]` carrier every fallible function returns, its monadic algebra selecting abort versus accumulate; absence rides the `expression` `Option` directly (no fault-bound alias, since `Option` carries no error slot to bind). One fault-lift core (`_convert`) backs every application shape — the explicit-thunk `boundary` (sync) and `async_boundary` (awaitable), and the `@trapped` decorator that auto-detects sync versus async and folds the conversion onto any raising callable — so a subprocess-seam, coroutine boundary, or decorated function never grows a parallel async rail and the sync/async split is a coroutine-detection branch at the edge, not a duplicated `try`/`except`. The conversion is data-driven: one ordered `CLASSIFY` table maps a caught exception family (`anyio` worker/timeout/resource breaks, `msgspec` decode/validation, `beartype` violations, stdlib `ImportError`/`OSError`/`TimeoutError`) onto the precise fault tag and payload, so `TimeoutError` lands as `deadline`, a codec failure as the subject-carrying `boundary` case, a type-contract violation as `api`, and an unclassified raise as `boundary` — never a stringly `type(cause).__name__` degradation that erases the taxonomy. The same conversion is the trace-egress seam: each caught exception is recorded on the active OTel span (`record_exception` + `StatusCode.ERROR`) inside the one fold, so a fault is visible in the distributed trace without the faults owner ever minting or owning a span — span lifecycle stays with the measured operation, exactly as receipts and resilience hold it. Domain logic returns `Result`/`Option` and never raises; exceptions convert exactly once at the owning boundary, and interior code receives only the rail.

## [01]-[INDEX]

- [01]-[FAULT]: the boundary-fault tagged union with the aggregate combination law, the data-driven exception classification table, the OTel trace-egress weave on the conversion, the beartype-violation redirect closing the contract seam, the Result/Option rail carriers, the disposition-parameterized traversal owning abort/accumulate/partition output shapes, the one sync/async/decorated fault-lift core, the structured fault-to-fact egress projection, and the `effect.result` multi-bind builder.

## [02]-[FAULT]

- Owner: `BoundaryFault` — the single closed fault family discriminating `config`, `resource`, `deadline`, `api`, `import_`, `wire`, and `boundary` ingress classes plus an `aggregate` case holding typed members; `RuntimeRail` the one `Result[T, BoundaryFault]` alias every fallible function returns; `CLASSIFY` the one ordered exception-family dispatch table the lift folds; `Disposition` the closed traversal-strategy vocabulary parameterizing the one `traversed` fold over its output shape; `boundary`/`async_boundary`/`trapped` the one sync, awaitable, and decorator application of a single `_convert` conversion at ingress/egress.
- Cases: each non-aggregate case is a frozen `case()` keyword-constructed with its typed payload — `config`/`resource`/`api`/`import_`/`boundary` carry `(subject, detail)`, `deadline` carries `(subject, budget)`, `wire` carries `(subject, code)`; `aggregate` carries `tuple[BoundaryFault, ...]` so the conjunctive boundary combines members without flattening to a string. The seven leaf constructors are not seven hand-written factories — `CLASSIFY` is the one ordered `(exception-family, builder)` table and `of` folds a caught exception down it to the first matching row's `BoundaryFault`, the fold seeded with the `boundary` catch-all so totality is structural rather than resting on a generator that raises `StopIteration` when a tail row is dropped. The `wire` case is reserved for explicit code-carrying construction (`BoundaryFault(wire=(subject, code))`) where a numeric protocol/status code is the discriminant (the `transport/wire#WIRE` registry-miss `to_result(BoundaryFault(wire=(name, 0)))`, the `transport/serve#SERVE` descriptor miss); a caught codec exception carries no numeric code, so the `CLASSIFY` `msgspec` row lands it in the subject-carrying `boundary` case rather than minting a code-carrying `wire`, and the two stay distinct on whether a numeric code is present — the `boundary("wire", ...)` subject the wire/serve fences pass survives the conversion because the row reads the passed `subject`, never a hardcoded literal.
- Entry: `_convert` is the one conversion — it folds a caught exception through `CLASSIFY.of`, records it on the active span, and returns `Error(BoundaryFault)`; `_guard` shares the sync `try`/`except` so `boundary(subject, thunk, *, catch=Exception)` (the synchronous application) and the `@trapped` sync arm route through one fence, while `async_boundary(subject, thunk, *, catch=Exception)` awaits the awaitable-returning thunk before the same `_convert` (so a synchronous compute and a subprocess-seam `anyio.to_process.run_sync`/subinterpreter offload share one conversion rather than a second async rail). `@trapped(subject, *, catch=Exception)` is the decorator form that wraps a raising `def`/`async def` by `inspect.iscoroutinefunction` dispatch so the boundary concern composes as an aspect rather than an inline `try` at every call site. `traversed(rails, *, by=Disposition.ABORT)` threads a `Block` of railed values through one fold whose `Disposition` row selects the output shape: `ABORT` short-circuits to the first `Error` returning `RuntimeRail[Block[T]]`, `ACCUMULATE` `combine`-folds every fault into one aggregate returning `RuntimeRail[Block[T]]`, and `PARTITION` returns the total `RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]` split so the caller polymorphises over the output without a parallel traversal — the disposition is one data row on the fold, never three sibling functions; `railed` is the `@effect.result` multi-bind builder a consumer drives with `yield from` for bind chains past three levels, the same rail without nested `.bind` ceremony, folding bound values into a `Block` rather than a mutable list.
- Auto: `combine` is the conjunctive combination law — a `match` spreads each side to its members (an aggregate's tuple, or itself as a singleton) and the two spreads concatenate into one aggregate, so a nested aggregate flattens and a leaf wraps without a per-arm branch; recovery keys on `fault.tag` or membership in `fault.aggregate`, never on a reconstructed message; `recoverable` folds the membership test over the aggregate spine against the closed `FaultTag` vocabulary, so the recovery set is `frozenset[FaultTag]` rather than stringly `frozenset[str]`; `facts` is the one total structured egress projection folding the union to the `dict[str, str]` slot map (`subject`/`detail`/`code`/`budget`/`members`) the `observability/receipts#RECEIPT` `rejected` emit reads (it reads `facts()["subject"]` with an owner fallback, so every leaf case carries a `subject` key and the `aggregate` case names its member tags), the `(subject, detail)`-shaped cases collapsing to one structural arm matched on the shared payload rather than a `getattr(self, self.tag)` escape that defeats the closed union; the carrier compares tag then payload, so a `set`/`Map` keyed on `RuntimeRail` distinguishes distinct faults rather than coalescing them.
- Packages: `expression` (`Result`/`Option`/`Ok`/`Error`/`Some`/`Nothing`/`Block`/`tagged_union`/`case`/`tag`/`effect.result`), `beartype` (`FAULT_CONF` the one shared `BeartypeConf(violation_type=BeartypeCallHintViolation)` domain decorators bind so a contract violation raises the canonical root the `CLASSIFY` `api` row catches and folds onto the rail rather than each adapter re-catching, `roar.BeartypeCallHintViolation` the `api` classification root), `msgspec` (`ValidationError`/`DecodeError` the codec rows folding to the subject-carrying `boundary` case), `anyio` (`to_process.run_sync` the subprocess-seam the async boundary closes over, `BrokenWorkerProcess`/`BrokenWorkerInterpreter`/`BrokenResourceError`/`ClosedResourceError`/`ConnectionFailed` the `resource` rows), `opentelemetry-api` (`trace.get_current_span` reading the active span the conversion records the exception on, `Span.record_exception`/`Span.set_status`/`Span.is_recording`, `trace.Status`/`StatusCode` the span-status value, never `get_tracer`/`start_span` — the owner records on the active span and never mints one), stdlib `inspect` (`iscoroutinefunction` the decorator sync/async discriminant) and `functools` (`wraps` preserving the decorated signature).
- Growth: a new fault class is one `case()` plus one row on the recovery membership set; a new exception family is one ordered row on `CLASSIFY` reaching `of`, every lift shape, and the trace-egress weave by that single add; a new egress slot is one arm on `facts`; a new traversal output shape is one `Disposition` member plus one fold arm rather than a new traversal function; the accumulation law absorbs every new leaf through the existing `aggregate` case with zero new surface.
- Boundary: no C# `Expected` clone, no product receipt minting, no broad exception taxonomy copied from C# owners; the `try`/`except` lives only inside `_guard`/`async_boundary`, never in domain flow; the conversion records on the active span but never mints, activates, names, or ends one — span lifecycle is the measured operation's, the faults owner only annotating whatever span is current (a no-op when none records); the recovery set keys on the fault's own `FaultTag`, never importing `reliability/resilience#RESILIENCE` `RetryClass` (resilience depends on faults, not the reverse — the rail maps exceptions to fault classes, the policy table maps retry classes to exception sets, the two meet only through the rail outcome); a sentinel return, a `None`-as-failure, a bare-`str` fault for the multi-cause domain, an aggregate that joins members into one string, a `type(cause).__name__`-always-`boundary` lift that erases the exception taxonomy, a `getattr`-driven `facts` arm that bypasses the closed match, a generator `of` that rests totality on a tail row, a hardcoded `"wire"` subject that clobbers the boundary's own subject, parallel abort/accumulate traversal functions where one `Disposition`-keyed fold owns the output shape, and a hand-written per-case constructor where the `CLASSIFY` fold owns the dispatch are the deleted forms.

```python signature
import inspect
from collections.abc import Awaitable, Callable, Generator
from enum import StrEnum
from functools import wraps
from typing import Final, Literal, ParamSpec, TypeVar

import anyio
import msgspec
from beartype import BeartypeConf
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result, case, effect, tag, tagged_union
from expression.collections import Block
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

type FaultTag = Literal["config", "resource", "deadline", "api", "import_", "wire", "boundary", "aggregate"]

P = ParamSpec("P")
T = TypeVar("T")


class Disposition(StrEnum):
    ABORT = "abort"
    ACCUMULATE = "accumulate"
    PARTITION = "partition"


@tagged_union(frozen=True)
class BoundaryFault:
    tag: FaultTag = tag()
    config: tuple[str, str] = case()
    resource: tuple[str, str] = case()
    deadline: tuple[str, float] = case()
    api: tuple[str, str] = case()
    import_: tuple[str, str] = case()
    wire: tuple[str, int] = case()
    boundary: tuple[str, str] = case()
    aggregate: tuple["BoundaryFault", ...] = case()

    @staticmethod
    def of(subject: str, cause: BaseException) -> "BoundaryFault":
        seed = BoundaryFault(boundary=(subject, type(cause).__name__))
        return next((build(subject, cause) for kinds, build in CLASSIFY if isinstance(cause, kinds)), seed)

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

    def facts(self) -> dict[str, str]:
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return {"tag": "aggregate", "subject": "aggregate", "members": ",".join(m.tag for m in members)}
            case BoundaryFault(tag="deadline", deadline=(subject, budget)):
                return {"tag": "deadline", "subject": subject, "budget": f"{budget:g}"}
            case BoundaryFault(tag="wire", wire=(subject, code)):
                return {"tag": "wire", "subject": subject, "code": str(code)}
            case BoundaryFault(tag=tag, config=(subject, detail)) | BoundaryFault(tag=tag, resource=(subject, detail)) | BoundaryFault(tag=tag, api=(subject, detail)) | BoundaryFault(tag=tag, import_=(subject, detail)) | BoundaryFault(tag=tag, boundary=(subject, detail)):
                return {"tag": tag, "subject": subject, "detail": detail}
            case _ as unreachable:
                raise AssertionError(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

CLASSIFY: Final[tuple[tuple[type[BaseException] | tuple[type[BaseException], ...], Callable[[str, BaseException], BoundaryFault]], ...]] = (
    (TimeoutError, lambda subject, cause: BoundaryFault(deadline=(subject, 0.0))),
    ((msgspec.ValidationError, msgspec.DecodeError), lambda subject, cause: BoundaryFault(boundary=(subject, type(cause).__name__))),
    (BeartypeCallHintViolation, lambda subject, cause: BoundaryFault(api=(subject, type(cause).__name__))),
    (ImportError, lambda subject, cause: BoundaryFault(import_=(subject, type(cause).__name__))),
    ((anyio.BrokenWorkerProcess, anyio.BrokenWorkerInterpreter, anyio.BrokenResourceError, anyio.ClosedResourceError, anyio.ConnectionFailed, OSError), lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__))),
)

# the one shared domain BeartypeConf: a @beartype-guarded domain function binds it so a
# contract violation raises the canonical BeartypeCallHintViolation the CLASSIFY `api` row
# folds onto the rail, rather than each adapter re-catching the violation inline.
FAULT_CONF: Final[BeartypeConf] = BeartypeConf(violation_type=BeartypeCallHintViolation)


# --- [OPERATIONS] -----------------------------------------------------------------------

type RuntimeRail[T] = Result[T, BoundaryFault]


def _convert(subject: str, cause: BaseException) -> BoundaryFault:
    fault = BoundaryFault.of(subject, cause)
    span = trace.get_current_span()
    if span.is_recording():
        span.record_exception(cause, attributes={"rasm.fault.tag": fault.tag, "rasm.fault.subject": subject})
        span.set_status(Status(StatusCode.ERROR, fault.tag))
    return fault


def _guard[T](subject: str, thunk: Callable[[], T], catch: type[BaseException]) -> RuntimeRail[T]:
    try:
        return Ok(thunk())
    except catch as cause:
        return Error(_convert(subject, cause))


def boundary[T](subject: str, thunk: Callable[[], T], *, catch: type[BaseException] = Exception) -> RuntimeRail[T]:
    return _guard(subject, thunk, catch)


async def async_boundary[T](subject: str, thunk: Callable[[], Awaitable[T]], *, catch: type[BaseException] = Exception) -> RuntimeRail[T]:
    try:
        return Ok(await thunk())
    except catch as cause:
        return Error(_convert(subject, cause))


def trapped(subject: str, *, catch: type[BaseException] = Exception) -> Callable[[Callable[P, T]], Callable[P, RuntimeRail[T]]]:
    def decorate(fn: Callable[P, T]) -> Callable[P, RuntimeRail[T]]:
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


def traversed[T](rails: Block[RuntimeRail[T]], *, by: Disposition = Disposition.ABORT) -> RuntimeRail[Block[T]] | RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]:
    match by:
        case Disposition.ABORT:
            seed: RuntimeRail[Block[T]] = Ok(Block.empty())
            return rails.fold(lambda acc, rail: acc.bind(lambda done: rail.map(lambda value: done.append(Block.singleton(value)))), seed)
        case Disposition.ACCUMULATE:
            faults = rails.choose(lambda rail: rail.swap().to_option())
            return Ok(rails.choose(lambda rail: rail.to_option())) if faults.is_empty() else Error(faults.reduce(BoundaryFault.combine))
        case Disposition.PARTITION:
            return Ok((rails.choose(lambda rail: rail.to_option()), rails.choose(lambda rail: rail.swap().to_option())))


@effect.result[Block[T], BoundaryFault]()
def railed[T](*rails: RuntimeRail[T]) -> Generator[T, T, Block[T]]:
    return Block.of_seq([(yield from rail) for rail in rails])
```
