# [PY_RUNTIME_FAULTS]

One fault family and one `Result`/`Option` rail span the whole branch: `BoundaryFault` is the one tagged union every package returns through — its ingress classes and the `aggregate` case keep every member structurally addressable — and `RuntimeRail` is the one `Result[T, BoundaryFault]` carrier every fallible function returns. Domain logic returns `Result`/`Option` and never raises; exceptions convert exactly once at the owning boundary, and interior code receives only the rail. Absence rides the `expression` `Option` directly — no fault-bound alias, since `Option` carries no error slot to bind.

One fault-lift core backs every application shape — the explicit-thunk `boundary`, the awaitable `async_boundary`, and the `@trapped` decorator — so the sync/async split is one coroutine-detection branch, never a parallel rail. Classification is the ordered data-driven `CLASSIFY` table, and the same conversion is the trace-egress seam: each caught exception is recorded on the active OTel span inside the one fold, the owner never minting, naming, or ending a span — span lifecycle stays with the measured operation. `latched`, the branch's one-shot install latch, and the WHOLE instrumentation-scope coordinate — `Scope`/`SCOPES` naming the emitting library beside the `scoped` stamp binding that name to its distribution version and semconv url — live here because faults is the one tier below every consumer, the sub-receipts `identity`, `clock`, `shapes`, and `wire` strata included, so no emitting page in the branch is left minting the unversioned scope a backend cannot join against its versioned siblings.

## [01]-[INDEX]

- [02]-[FAULT]: the closed fault family, the classification table, the rail carriers and disposition-parameterized traversal, the three fault-lift shapes, the carried-fault span fold, the install latch, and the versioned instrumentation-scope stamp.

## [02]-[FAULT]

- Owner: `BoundaryFault` and its rail, tables, and cross-cutting tenants per the fence. Traversal splits by shape: `traversed` folds a homogeneous `Block` of already-evaluated rails under one `Disposition`, while `railed` is the bound `effect.result` builder for free-form interleaved binds whose later steps depend on earlier bound values — a variadic short-circuit collector beside them is `traversed(by=ABORT)` re-spelled and never lands.
- Cases: `config` versus `boundary` splits on who can repair the refusal — `config` carries a caller-repairable construction refusal (a policy value, roster, credential row, or precondition the same inputs deterministically refuse), while `boundary` carries the seam classification of a provider or runtime raise during work (a codec, render, parse, or engine failure a re-issue may clear), so a render-class or draw-class fault rides `boundary=` and a refused composition rides `config=` in every consumer. `wire` is reserved for explicit code-carrying construction where a numeric protocol/status code is the discriminant; a caught codec exception carries no code, so the `CLASSIFY` `msgspec` row lands it in the subject-carrying `boundary` case. A deadline-owning fence constructs `deadline` explicitly with its real budget and tripped-axis `cause`; the `CLASSIFY` `TimeoutError` row, with no budget in hand, defaults `budget` to `0.0` — the budget-unknown floor a consumer reads as unspecified, never a true zero deadline.
- Entry: the three lift shapes share one `_convert`; `catch` admits a class tuple so an engine boundary narrows over its real multi-class raise surface instead of the `Exception` catch-all, and it never widens past `Exception` — converting the `anyio` cancellation exception into a fault is the forbidden widening, cancellation being scope-owned flow control rather than an ingress class.
- Auto: `facts` is the one structured egress projection the `observability/receipts#RECEIPT` `rejected` projection spreads whole, so every leaf case carries its own `subject` inline and the receipts owner re-derives nothing. `faulted` is its span-side twin and the branch's ONE Error-arm fold for a fault the rail CARRIED into a live span: `_convert` covers the raise this fence caught, `faulted` covers the typed fault that crossed a worker, lane, or sibling boundary and would otherwise leave an `UNSET` span beside an uncorrelated line. It seats here for the reason `scoped` does — every producer plane reaches this tier and none reaches a peer's — so the four hand-rolled copies a charter declaring universal behavior had grown collapse onto one body, and its `**fields` band is the growth axis a stage, subject, or crossing index rides.
- Packages: `expression`, `beartype`, `msgspec`, `anyio`, `structlog`, `opentelemetry-api`, and `opentelemetry-semantic-conventions` per the fence imports; the OTel dependency is `-api` with the semconv constant surface — the owner reads the active span, `scoped` stamps a caller-supplied factory, and no SDK type or provider instance enters. `structlog` enters as the PACKAGE alone, whose `get_logger` proxy resolves the configured chain at first bind, so this tier composes the log egress without importing the `observability/logging` module the rail seats above it.
- Growth: a new fault class is one `case()` with one recovery-membership row; a new exception family is one ordered `CLASSIFY` row reaching every lift shape and the trace weave; a new egress slot is one `facts` arm; a new traversal output shape is one `Disposition` member with one fold arm; a new span-side error dimension is one `**fields` key at a `faulted` call site, never a second fold; a new instrumentation scope is one `Scope` member with one `SCOPES` row; a semconv bump one `Schemas` member swap reaching every meter, tracer, logger, and `Resource` at once; a fourth scope-minting API one `scoped` call, the port already covering any factory sharing the positional `(name, version, provider, schema_url)` shape; a new one-shot install owner is one `@latched(...)` application.
- Boundary: no C# `Expected` clone and no exception taxonomy copied from a C# owner. Recovery keys on the fault's own `FaultTag` and never imports `reliability/resilience#RESILIENCE` `RetryClass` — resilience depends on faults, never the reverse; the rail maps exceptions to fault classes, the policy table maps retry classes to exception sets, and the two meet through the rail outcome and the exported `spelled` matcher alone, which resilience reads and never re-derives.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import inspect
from collections.abc import Awaitable, Callable
from enum import StrEnum
from functools import wraps
from importlib.metadata import distributions
from typing import Any, Final, Literal, Protocol, assert_never, overload

import anyio
import msgspec
import structlog
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Ok, Result, Some, case, effect, tag, tagged_union
from expression.collections import Block, Map
from opentelemetry import trace
from opentelemetry.semconv.schemas import Schemas
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
    LOGGER = "logger"
    SERVICE = "service"
    RESILIENCE = "resilience"
    IDENTITY = "identity"
    EVIDENCE = "evidence"
    RECIPE = "recipe"
    WORKERS = "workers"
    PROFILES = "profiles"
    JOURNAL = "journal"


# Three API scope factories — `metrics.get_meter`, `trace.get_tracer`, `_logs.get_logger` — spell their version
# parameter two ways (`version` against `instrumenting_library_version`) yet share one positional shape, so this
# stamp binds POSITIONALLY through the port and that naming divergence never reaches a call site. Its third slot
# takes the provider override every runtime site declines: `None` resolves whichever global the install published.
class ScopeAcquire[T](Protocol):
    def __call__(self, name: str, version: str, provider: None, schema_url: str, /) -> T: ...


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
        # every anyio resource death spells itself: `ConnectionFailed` alone subclasses `OSError`, so the trailing
        # builtin covers none of its siblings and an unlisted spelling falls to the catch-all, where `str(cause)` erases
        # the type the whole row exists to keep. `BusyResourceError` is the CUSTODY arm — `transport/roots#RESOURCE`'s
        # `ResourceGuard` rails an overlapping second puller through it — so dropping it classifies a real
        # single-consumer breach as an unclassified boundary fault that no `recoverable` membership reaches.
        (
            anyio.BrokenWorkerProcess,
            anyio.BrokenWorkerInterpreter,
            anyio.BrokenResourceError,
            anyio.BusyResourceError,
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

# consumers mint handles through the stamp — scoped(trace.get_tracer, SCOPES[Scope.WIRE]) — never a per-page literal and
# never a bare factory call. A scope names the instrumenting library, never the signal, so the meter and logger slots
# resolve one name for one library's two signals while four independently-emitting planes each keep their own row: a
# backend joining on scope separates the worker crossing, the profiler push, and the durable journal from the served
# host, and `SERVICE` narrows to that host and its CLI app name alone.
SCOPES: Final[Map[Scope, str]] = Map.of_seq([
    (Scope.WIRE, "rasm.wire"),
    (Scope.METER, "rasm.runtime"),
    (Scope.LOGGER, "rasm.runtime"),
    (Scope.SERVICE, "rasm.companion"),
    (Scope.RESILIENCE, "rasm.runtime.resilience"),
    (Scope.IDENTITY, "rasm.runtime.identity"),
    (Scope.EVIDENCE, "rasm.runtime.evidence"),
    (Scope.RECIPE, "rasm.runtime.recipe"),
    (Scope.WORKERS, "rasm.runtime.workers"),
    (Scope.PROFILES, "rasm.runtime.profiles"),
    (Scope.JOURNAL, "rasm.runtime.journal"),
])

# Distribution version and estate-wide semconv pin complete the coordinate `SCOPES` starts, every runtime `Resource`
# carrying that same url. Both home beside the name because a scope is one triple, and both home at THIS tier because
# `identity`, `clock`, `shapes`, and `wire` emit spans from below every observability owner — a coordinate seated any
# higher leaves those pages no import that reaches it. `SCHEMA_URL` reads the semconv distribution's own `Schemas`
# roster rather than re-spelling the url, so a semconv bump moves the pin by moving the member and can never desync
# it silently. `DISTRIBUTION` names the ONE python distribution this estate declares — `pyproject.toml` `[project]
# name`, the per-package manifest being foreclosed by `libs/.planning/README.md` — because installed metadata keys on
# a DISTRIBUTION name, never a package path. The roster read is TOTAL where `version(DISTRIBUTION)` is not: that call
# RAISES `PackageNotFoundError` whenever no metadata answers, and the workspace root declares `[tool.uv] package =
# false`, so every source-tree run and every dev venv resolves nothing for a correctly-spelled name and a module-scope
# raise there kills every `scoped` mint in the process — the whole signal plane and the `Resource` service version
# with it. `SOURCE_VERSION` answers that miss with the local-version segment PEP 440 reserves for a build carrying no
# release identity, so an uninstalled run still mints a versioned, schema-pinned coordinate a backend joins against
# its installed siblings and the segment itself reports which was read; a plausible release number spelled as the miss
# is the deleted form, because it forges provenance the process never had.
SCHEMA_URL: Final[str] = Schemas.V1_43_0.value
DISTRIBUTION: Final[str] = "workspace-foundation-python"
SOURCE_VERSION: Final[str] = "0+source"
SCOPE_VERSION: Final[str] = next((dist.version for dist in distributions() if dist.metadata["Name"] == DISTRIBUTION), SOURCE_VERSION)


# --- [SERVICES] --------------------------------------------------------------------------

# module-scope bound logger — a dependency-backed runtime handle, never an immutable anchor: `structlog.get_logger()`
# is a lazy proxy resolving the configured chain at first bind, so this tier carries the PACKAGE and never the
# `observability/logging` module the import rail seats above it.
_LOG: Final = structlog.get_logger()


# --- [OPERATIONS] -----------------------------------------------------------------------


def scoped[T](acquire: ScopeAcquire[T], scope: str) -> T:
    # one scope stamp every signal mints through: the `SCOPES` row names the emitting library, this pair versions it
    # and pins the semconv coordinate, so a meter, a tracer, and a logger opened for one scope carry an identical
    # instrumentation-scope triple and a semconv bump is one constant. A bare `get_*(scope)` mints an unversioned,
    # schema-free scope a backend cannot join against its versioned siblings, so no call site spells one.
    return acquire(scope, SCOPE_VERSION, None, SCHEMA_URL)


def faulted(span: trace.Span, event: str, fault: BoundaryFault, /, **fields: object) -> BoundaryFault:
    # the ONE Error-arm fold for a fault the rail CARRIED into a live span rather than raised inside it: `_convert`
    # already owns the raise-at-this-fence case, so this is its twin for a typed fault that crossed a worker, a lane,
    # or a sibling boundary and would otherwise leave the span `UNSET` and its error line uncorrelated. Status first,
    # then the structured line off `facts()` — the same projection the receipts `rejected` arm spreads, so the log and
    # the receipt cannot disagree — then the fault back, so a producer's arm reads `.map_error(lambda f: faulted(span,
    # "<event>", f, step=...))` and never a three-statement block whose middle line drifts per page. `**fields` is the
    # growth axis and merges UNDER the fault's own facts, so a caller key can never shadow the tag, subject, or detail
    # a reader gates on. `record_exception` is deliberately absent: the raise converted at its own boundary and is
    # unpicklable across a worker crossing, so the flat projection IS the evidence. Issue scope and tenant arrive
    # ambient through the chain's contextvars and baggage heads — a caller binding either re-owns a logging seam.
    span.set_status(Status(StatusCode.ERROR, fault.tag))
    _LOG.error(event, **(fields | fault.facts()))
    return fault


def spelled(cause: BaseException) -> frozenset[str]:
    # the ONE module-qualified MRO spelling set the branch matches import-free names against — isinstance semantics
    # for a provider class this tier never imports, the defining-module anchor rejecting an unrelated class that
    # re-uses a provider's bare name. Both matchers intersect against it: the `CLASSIFY` frozenset row here and
    # `reliability/resilience#RESILIENCE` `_transient`'s target/refuse rosters, so the convention has one derivation
    # and a spelling change lands once instead of drifting between the classifier and the retry predicate.
    return frozenset(f"{klass.__module__}.{klass.__qualname__}" for klass in type(cause).__mro__)


def _hits(marker: ClassifyMarker, cause: BaseException) -> bool:
    match marker:
        case frozenset() as names:
            return bool(spelled(cause) & names)
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

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
