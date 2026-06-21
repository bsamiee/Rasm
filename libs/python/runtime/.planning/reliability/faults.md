# [PY_RUNTIME_FAULTS]

The single fault family and `Result`/`Option` rail for the whole branch. `BoundaryFault` is the one tagged union every package returns through, discriminating the seven ingress classes and carrying an `aggregate` case so an accumulating boundary keeps every member structurally addressable. `RuntimeRail` is the one `Result[T, BoundaryFault]` carrier every fallible function returns, its monadic algebra selecting abort versus accumulate. Absence rides the `expression` `Option` directly — no fault-bound alias, since `Option` carries no error slot to bind.

One fault-lift core (`_convert`) backs every application shape: the explicit-thunk `boundary` (sync), `async_boundary` (awaitable), and the `@trapped` decorator that auto-detects sync versus async and folds the conversion onto any raising callable. A subprocess-seam, coroutine boundary, or decorated function never grows a parallel async rail; the sync/async split is one coroutine-detection branch at the edge, not a duplicated `try`/`except`.

The conversion is data-driven. One ordered `CLASSIFY` table maps a caught exception family (`anyio` worker/timeout/resource breaks, `msgspec` decode/validation, `beartype` violations, stdlib `ImportError`/`OSError`/`TimeoutError`) onto the precise fault tag and payload, so `TimeoutError` lands as `deadline`, a codec failure as the subject-carrying `boundary` case, a type-contract violation as `api`, and an unclassified raise as `boundary` — never a stringly `type(cause).__name__` degradation that erases the taxonomy.

The same conversion is the trace-egress seam. Each caught exception is recorded on the active OTel span (`record_exception` plus `StatusCode.ERROR`) inside the one fold, so a fault is visible in the distributed trace without the faults owner ever minting or owning a span. Span lifecycle stays with the measured operation, exactly as receipts and resilience hold it.

Domain logic returns `Result`/`Option` and never raises. Exceptions convert exactly once at the owning boundary, and interior code receives only the rail.

## [01]-[INDEX]

- [01]-[FAULT]: the boundary-fault tagged union with the aggregate combination law, the data-driven exception classification table, the OTel trace-egress weave on the conversion, the beartype-violation redirect closing the contract seam, the Result/Option rail carriers, the disposition-parameterized traversal owning abort/accumulate/partition output shapes, the one sync/async/decorated fault-lift core, the structured fault-to-fact egress projection, and the `effect.result` multi-bind builder.

## [02]-[FAULT]

- Owner: `BoundaryFault` — the single closed fault family discriminating `config`, `resource`, `deadline`, `api`, `import_`, `wire`, and `boundary` ingress classes plus an `aggregate` case holding typed members; `RuntimeRail` the one `Result[T, BoundaryFault]` alias every fallible function returns; `CLASSIFY` the one ordered exception-family dispatch table the lift folds; `Disposition` the closed traversal-strategy vocabulary parameterizing the one `traversed` fold over its output shape; `boundary`/`async_boundary`/`trapped` the one sync, awaitable, and decorator application of a single `_convert` conversion at ingress/egress; `railed` the bound `effect.result` computation-expression builder a consumer applies to a free-form `yield from`-bind generator past the three-level threshold `traversed` (a homogeneous-fold-over-rails) does not cover.
- Cases: each non-aggregate case is a frozen `case()` keyword-constructed with its typed payload — `config`/`resource`/`api`/`import_`/`boundary` carry `(subject, detail)`, `deadline` carries `(subject, budget)`, `wire` carries `(subject, code)`; `aggregate` carries `tuple[BoundaryFault, ...]` so the conjunctive boundary combines members without flattening to a string. The seven leaf constructors are not seven hand-written factories — `CLASSIFY` is the one ordered `(exception-family, builder)` table and `of` `choose`s the first row whose family matches the caught exception, reads it through `Block.try_head` as an `Option[BoundaryFault]`, and supplies the `boundary` catch-all through `Option.default_with` — totality is the `Option` fold, never a falsy-`None` `or` resting on `tagged_union` truthiness and never a `next(...)` that raises `StopIteration` when a tail row is dropped. Two cases carry a known scalar the caught exception does not — `wire` its numeric code and `deadline` its budget — so both admit explicit keyword construction at the boundary that owns the scalar, distinct from the `CLASSIFY` catch path that defaults the scalar when only an exception is in hand. `wire` is reserved for explicit code-carrying construction (`BoundaryFault(wire=(subject, code))`) where a numeric protocol/status code is the discriminant (the `transport/wire#WIRE` registry-miss `to_result(BoundaryFault(wire=(name, 0)))`, the `transport/serve#SERVE` descriptor miss); a caught codec exception carries no numeric code, so the `CLASSIFY` `msgspec` row lands it in the subject-carrying `boundary` case rather than minting a code-carrying `wire`, and the two stay distinct on whether a numeric code is present — the `boundary("wire", ...)` subject the wire/serve fences pass survives the conversion because the row reads the passed `subject`, never a hardcoded literal. Symmetrically, a deadline-bounded I/O fence that owns its `anyio.fail_after(delay)` budget constructs `BoundaryFault(deadline=(subject, delay))` explicitly to carry the real budget onto the egress fact, while the `CLASSIFY` `TimeoutError` row — catching a bare timeout with no budget in hand — defaults `budget` to `0.0`, the "budget unknown at this fence" floor a downstream consumer reads as unspecified rather than a true zero deadline.
- Entry: `_convert` is the one conversion — it folds a caught exception through `BoundaryFault.of` (the `CLASSIFY` `Block.choose`/`try_head`/`default_with` first-match `Option` fold to the matching row's builder), records it on the active span, and returns `Error(BoundaryFault)`; `_guard` shares the sync `try`/`except` so `boundary(subject, thunk, *, catch=Exception)` (the synchronous application) and the `@trapped` sync arm route through one fence, while `async_boundary(subject, thunk, *, catch=Exception)` awaits the awaitable-returning thunk before the same `_convert` (so a synchronous compute and a subprocess-seam `anyio.to_process.run_sync`/subinterpreter offload share one conversion rather than a second async rail). `@trapped(subject, *, catch=Exception)` is the decorator form that wraps a raising `def`/`async def` by `inspect.iscoroutinefunction` dispatch so the boundary concern composes as an aspect rather than an inline `try` at every call site. The `catch` parameter on all three application shapes is `type[BaseException] | tuple[type[BaseException], ...]` so a boundary narrowing over a multi-class engine fault surface (a `spatial/mesh#MESH` row's `(meshio.ReadError, meshio.WriteError)` or `(FileNotFoundError, OSError)` set) passes its real raise tuple rather than collapsing to the `Exception` catch-all, the `except catch` clause admitting the class-tuple natively. `traversed(rails, *, by=Disposition.ABORT)` threads a `Block` of railed values through one fold whose `Disposition` row selects the output shape. `ABORT` short-circuits to the first `Error` returning `RuntimeRail[Block[T]]`; `ACCUMULATE` `combine`-folds every fault into one aggregate returning `RuntimeRail[Block[T]]`; `PARTITION` returns the total `RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]` split. `ACCUMULATE` and `PARTITION` share one match arm and one pair of `choose` projections — `rail.to_option()` drains the values, `rail.swap().to_option()` drains the faults — so the disjoint-output dispositions fold the rails once rather than re-spelling the two `choose` passes per arm. The output shape is carried statically by `@overload` arms keyed on the `Disposition` `Literal` so a caller narrows on the disposition it passes rather than re-matching the runtime union: the disposition is one data row on the fold, never three sibling functions. `railed` is the bound `effect.result[Any, BoundaryFault]()` computation-expression builder a consumer applies to its own generator function so a free-form chain of interleaved `value = yield from rail` binds reads as straight-line code past the three-level threshold where nested `.bind` lambdas obscure the flow — the generator protocol short-circuits to the first `Error` (`Result.__iter__` raising `EffectError` on the non-`ok` case) and the generator's own `return` value becomes the `Ok` payload through the builder's `return_`. This is the capability `traversed` cannot express: `traversed` folds a homogeneous `Block` of already-evaluated rails under one disposition, while `railed` lets the consumer interleave heterogeneous binds whose later steps depend on earlier bound values and return an arbitrary computed shape. The builder element is `Any` — the `effect.result` `ResultBuilder[_TSource, _TError]` carries one `_TSource` typing both the per-`yield` bind and the `return_` payload, so a chain binding leaf `T` values yet returning a distinct aggregate erases the per-bind element through `Any` rather than forcing one type across both positions or reintroducing a module-level `TypeVar` the modern band forbids; the consumer's generator carries the precise per-step types at its own definition.
- Auto: `combine` is the conjunctive combination law — a `match` spreads each side to its members (an aggregate's tuple, or itself as a singleton) and the two spreads concatenate into one aggregate, so a nested aggregate flattens and a leaf wraps without a per-arm branch; recovery keys on `fault.tag` or membership in `fault.aggregate`, never on a reconstructed message; `recoverable` folds the membership test over the aggregate spine against the closed `FaultTag` vocabulary, so the recovery set is `frozenset[FaultTag]` rather than stringly `frozenset[str]`; `facts` is the one total structured egress projection folding the union to the `dict[str, object]` slot map (`subject`/`detail`/`code`/`budget`/`members`) the `observability/receipts#RECEIPT` `rejected` projection spreads whole (`{"event": "rejected", "owner": owner, **fault.facts()}`), so every leaf case carries its own `subject` key inline rather than the receipts owner re-deriving one, and the `aggregate` case names its member tags. The map element type is `object`, not `str`, because the `deadline` `budget: float` and `wire` `code: int` ride as the native scalar the receipts `EventDict` (`dict[str, object]`) carries and its `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce — a pre-`str()`-formatted `f"{budget:g}"`/`str(code)` here is the deleted form the receipts owner's renderer is built to avoid; only `members` joins to a readable comma-string because it names member tags rather than carrying one scalar. The `(subject, detail)`-shaped cases collapse to one structural arm matched on the shared payload rather than a `getattr(self, self.tag)` escape that defeats the closed union; the carrier compares tag then payload, so a `set`/`Map` keyed on `RuntimeRail` distinguishes distinct faults rather than coalescing them.
- Packages: `expression` (`Result`/`Option`/`Ok`/`Error`/`Some`/`Nothing`/`tagged_union`/`case`/`tag`, the `Block` carrier with `Block.of_seq`/`Block.fold`/`Block.choose`/`Block.reduce`/`Block.append`/`Block.singleton`/`Block.try_head` — `choose`+`try_head` the `CLASSIFY` first-match `Option` fold, `fold` the `ABORT` `bind`-thread, `choose`+`reduce` the `ACCUMULATE` fault fold, `of_seq`/`empty`/`singleton`/`append` the `ABORT` accumulator carriers — `Some`/`Nothing`/`Option.default_with` the `of` catch-all fallback, `Result.swap`/`Result.to_option`/`Option.is_none` the `ACCUMULATE`/`PARTITION` arms project rails and the emptiness gate through, `effect.result[Any, BoundaryFault]()` (the `ResultBuilder` re-export, its `Result.__iter__` `yield`/`EffectError` protocol and `return_` payload-lift) the bound `railed` computation-expression builder a consumer applies to its own generator), `beartype` (`FAULT_CONF` the one shared `BeartypeConf(violation_type=BeartypeCallHintViolation)` domain decorators bind so a contract violation raises the canonical root the `CLASSIFY` `api` row catches and folds onto the rail rather than each adapter re-catching, `roar.BeartypeCallHintViolation` the `api` classification root), `msgspec` (`ValidationError`/`DecodeError` the codec rows folding to the subject-carrying `boundary` case), `anyio` (`to_process.run_sync` the subprocess-seam the async boundary closes over, `BrokenWorkerProcess`/`BrokenWorkerInterpreter`/`BrokenResourceError`/`ClosedResourceError`/`ConnectionFailed` the `resource` rows), `opentelemetry-api` (`trace.get_current_span` reading the active span the conversion records the exception on, `Span.record_exception`/`Span.set_status`/`Span.is_recording`, `trace.Status`/`StatusCode` the span-status value, never `get_tracer`/`start_span` — the owner records on the active span and never mints one), stdlib `inspect` (`iscoroutinefunction` the decorator sync/async discriminant), `functools` (`wraps` preserving the decorated signature), and `typing` (`Any` the `effect.result[Any, BoundaryFault]()` builder element type, since the builder's one `_TSource` types both the per-`yield` bind and the `return_` payload and a chain binding leaf `T` yet returning a distinct aggregate cannot pin one type across both positions; `overload` the per-`Disposition`-`Literal` output-shape carrier on `traversed`). Every type parameter is PEP 695 inline — `def trapped[**P, T]`, `def boundary[T]`/`async def async_boundary[T]`/`def traversed[T]`, and `type RuntimeRail[T]` — with no module-level `TypeVar`/`ParamSpec`; `railed` is a module-level bound builder instance carrying no generic of its own, the per-step types living in each consumer's decorated generator.
- Growth: a new fault class is one `case()` plus one row on the recovery membership set; a new exception family is one ordered row on `CLASSIFY` reaching `of`, every lift shape, and the trace-egress weave by that single add; a new egress slot is one arm on `facts`; a new traversal output shape is one `Disposition` member plus one fold arm rather than a new traversal function; the accumulation law absorbs every new leaf through the existing `aggregate` case with zero new surface.
- Boundary: no C# `Expected` clone, no product receipt minting, no broad exception taxonomy copied from C# owners; the `try`/`except` lives only inside `_guard`/`async_boundary`, never in domain flow; the conversion records on the active span but never mints, activates, names, or ends one — span lifecycle is the measured operation's, the faults owner only annotating whatever span is current (a no-op when none records); the recovery set keys on the fault's own `FaultTag`, never importing `reliability/resilience#RESILIENCE` `RetryClass` (resilience depends on faults, not the reverse — the rail maps exceptions to fault classes, the policy table maps retry classes to exception sets, the two meet only through the rail outcome); a sentinel return, a `None`-as-failure, a bare-`str` fault for the multi-cause domain, an aggregate that joins members into one string, a `type(cause).__name__`-always-`boundary` lift that erases the exception taxonomy, a `getattr`-driven `facts` arm that bypasses the closed match, a `str()`-coerced `dict[str, str]` `facts` map that pre-formats the native `budget: float`/`code: int` the `observability/receipts#RECEIPT` `dict[str, object]` `EventDict` and its `enc_hook=repr` renderer carry natively, a generator `of` that rests totality on a tail row or a falsy-`None` `or` resting on `tagged_union` truthiness where the `choose`/`try_head`/`default_with` `Option` fold owns the catch-all, a hardcoded `"wire"` subject that clobbers the boundary's own subject, parallel abort/accumulate traversal functions where one `Disposition`-keyed fold owns the output shape, a bare widened `RuntimeRail[Block[T]] | RuntimeRail[tuple[...]]` return forcing every caller to re-narrow where the `@overload` arms keyed on the `Disposition` `Literal` carry the precise per-disposition output type, a hand-written per-case constructor where the `CLASSIFY` fold owns the dispatch, a module-level `TypeVar`/`ParamSpec` where PEP 695 inline parameters carry the generics, a `railed` fixed-variadic `*rails: RuntimeRail[T]` collector that pre-evaluates its rails and returns `Block[T]` — the deleted form, since that is `traversed(rails, by=Disposition.ABORT)` exactly, a second short-circuit-collect surface duplicating the ABORT disposition with weaker (`Any`-erased) typing rather than the free-form interleaved-bind capability `railed` uniquely adds over `traversed`, and an `effect.result` builder subscription pinning one type across the per-`yield` bind and a distinct aggregate return where the leaf-bind element erases through `Any` are the deleted forms.

```python signature
import inspect
from collections.abc import Awaitable, Callable
from enum import StrEnum
from functools import wraps
from typing import Any, Final, Literal, assert_never, overload

import anyio
import msgspec
from beartype import BeartypeConf
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Ok, Result, Some, case, effect, tag, tagged_union
from expression.collections import Block
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

type FaultTag = Literal["config", "resource", "deadline", "api", "import_", "wire", "boundary", "aggregate"]
type ClassifyRow = tuple[type[BaseException] | tuple[type[BaseException], ...], Callable[[str, BaseException], "BoundaryFault"]]


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
        # `choose` keeps the first row whose family matches, `try_head` reads it as an
        # `Option`, and `default_with` supplies the `boundary` catch-all — totality is the
        # `Option` fold, never a falsy-`None` `or` resting on `tagged_union` truthiness nor a
        # generator `next(...)` that raises `StopIteration` when a tail row is dropped. The
        # catch-all `detail` is `str(cause) or type(cause).__name__` so an UNCLASSIFIED domain
        # exception carrying a discriminating message (a `mesh/cad#BRIDGE` `BridgeFault` whose
        # `of` mints `step-bridge.<stage>: ReadFile failed (...)`) preserves that message into the
        # `detail` slot the `facts()` egress and the receipts `rejected` projection carry, falling
        # back to the class name only for a bare message-less raise; the CLASSIFY rows keep
        # `type(cause).__name__` where the exception TYPE is the discriminant (a `DecodeError`
        # class name, a `BeartypeCallHintViolation`), since their message is noise, not a tag.
        matched = CLASSIFY.choose(lambda row: Some(row[1](subject, cause)) if isinstance(cause, row[0]) else Nothing)
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
        # `budget: float` and `code: int` ride as native scalars: the `observability/receipts#RECEIPT`
        # `EventDict` is `dict[str, object]` and its `Encoder(enc_hook=repr, order="deterministic")`
        # renderer serializes native ints/floats without a `str()` coerce — pre-stringifying here is
        # the deleted form the receipts owner rejects.
        match self:
            case BoundaryFault(tag="aggregate", aggregate=members):
                return {"tag": "aggregate", "subject": "aggregate", "members": ",".join(m.tag for m in members)}
            case BoundaryFault(tag="deadline", deadline=(subject, budget)):
                return {"tag": "deadline", "subject": subject, "budget": budget}
            case BoundaryFault(tag="wire", wire=(subject, code)):
                return {"tag": "wire", "subject": subject, "code": code}
            case BoundaryFault(tag=tag, config=(subject, detail)) | BoundaryFault(tag=tag, resource=(subject, detail)) | BoundaryFault(tag=tag, api=(subject, detail)) | BoundaryFault(tag=tag, import_=(subject, detail)) | BoundaryFault(tag=tag, boundary=(subject, detail)):
                return {"tag": tag, "subject": subject, "detail": detail}
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# row order is load-bearing: `TimeoutError` subclasses `OSError`, so the `deadline` row must
# precede the `OSError` `resource` row for the first-match `choose`/`try_head` fold to land a
# timeout as `deadline` rather than coalescing it into `resource`.
CLASSIFY: Final[Block[ClassifyRow]] = Block.of_seq([
    (TimeoutError, lambda subject, cause: BoundaryFault(deadline=(subject, 0.0))),
    ((msgspec.ValidationError, msgspec.DecodeError), lambda subject, cause: BoundaryFault(boundary=(subject, type(cause).__name__))),
    (BeartypeCallHintViolation, lambda subject, cause: BoundaryFault(api=(subject, type(cause).__name__))),
    (ImportError, lambda subject, cause: BoundaryFault(import_=(subject, type(cause).__name__))),
    ((anyio.BrokenWorkerProcess, anyio.BrokenWorkerInterpreter, anyio.BrokenResourceError, anyio.ClosedResourceError, anyio.ConnectionFailed, OSError), lambda subject, cause: BoundaryFault(resource=(subject, type(cause).__name__))),
])

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
        # `escaped` stays the default `False`: the exception is converted to `Error(fault)`
        # at this fence, so per OTel semantics it does NOT escape the span scope.
        span.record_exception(cause, attributes={"rasm.fault.tag": fault.tag, "rasm.fault.subject": subject})
        span.set_status(Status(StatusCode.ERROR, fault.tag))
    return fault


def _guard[T](subject: str, thunk: Callable[[], T], catch: type[BaseException] | tuple[type[BaseException], ...]) -> RuntimeRail[T]:
    try:
        return Ok(thunk())
    except catch as cause:
        return Error(_convert(subject, cause))


def boundary[T](subject: str, thunk: Callable[[], T], *, catch: type[BaseException] | tuple[type[BaseException], ...] = Exception) -> RuntimeRail[T]:
    return _guard(subject, thunk, catch)


async def async_boundary[T](subject: str, thunk: Callable[[], Awaitable[T]], *, catch: type[BaseException] | tuple[type[BaseException], ...] = Exception) -> RuntimeRail[T]:
    try:
        return Ok(await thunk())
    except catch as cause:
        return Error(_convert(subject, cause))


def trapped[**P, T](subject: str, *, catch: type[BaseException] | tuple[type[BaseException], ...] = Exception) -> Callable[[Callable[P, T]], Callable[P, RuntimeRail[T]]]:
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


@overload
def traversed[T](rails: Block[RuntimeRail[T]], *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ...) -> RuntimeRail[Block[T]]: ...
@overload
def traversed[T](rails: Block[RuntimeRail[T]], *, by: Literal[Disposition.PARTITION]) -> RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]: ...
def traversed[T](rails: Block[RuntimeRail[T]], *, by: Disposition = Disposition.ABORT) -> RuntimeRail[Block[T]] | RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]:
    # `ABORT`/`ACCUMULATE` collapse to `RuntimeRail[Block[T]]`; only `PARTITION` widens the Ok
    # arm to the `(values, faults)` tuple — the overloads carry the output shape so a caller
    # narrows on the `Disposition` literal it passes, never on the runtime union.
    match by:
        case Disposition.ABORT:
            seed: RuntimeRail[Block[T]] = Ok(Block.empty())
            return rails.fold(lambda acc, rail: acc.bind(lambda done: rail.map(lambda value: done.append(Block.singleton(value)))), seed)
        case Disposition.ACCUMULATE | Disposition.PARTITION:
            values, faults = rails.choose(lambda rail: rail.to_option()), rails.choose(lambda rail: rail.swap().to_option())
            if by is Disposition.PARTITION:
                return Ok((values, faults))
            return Ok(values) if faults.try_head().is_none() else Error(faults.reduce(BoundaryFault.combine))
        case _ as unreachable:
            assert_never(unreachable)


railed = effect.result[Any, BoundaryFault]()
```
