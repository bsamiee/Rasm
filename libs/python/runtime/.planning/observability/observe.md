# [PY_RUNTIME_OBSERVE]

This owner seats the observation primitives every producing site composes — the one call-shaped span weave, the composition-scope axis, the field-redaction policy, the counted evidence window, the process-cost bracket, and the drain tally — and contributes nothing to provider install, product telemetry export, or health. The drain taxonomy is MINTED here: `DrainOutcome`, the derived `DRAIN_COLUMNS` beside the `DRAIN_DISPOSITIONS` carve that strips the admitted total off the terminal partition, and `Drained[T]` live on this page, and `execution/lanes#LANE` (the producing drain) and `observability/metrics#METRIC` (the `rasm.lane.drained` counter) import the taxonomy FROM this owner — no upward import survives and neither column set can drift.

`measured` is the folder's sole span-lifecycle owner: span from the caller's scope, the fault fence INSIDE the live span, rail flatten, and the two-sided status close. A producer writes its own line through `structlog` at the site and its own instrument through `observability/metrics#METRIC`; `Redaction` is the `Classification`-keyed field policy that line binds under `REDACTION_KEY`, classifying by key name at every depth, and the `observability/logging#PIPELINE` chain applies it to the assembled line and ships under its `LogShip` law. Inbound context re-attaches through `attach` over the propagator's own extract — the cross-`libs/` `ONE_DISTRIBUTED_TRACE` Python leg against the C# `dotnet:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` producer. This page mints the `ScopeKey` composition axis naming which embedded composition owns a custody row, and composes the `reliability/faults#FAULT` `scoped` stamp naming which library emitted a signal — the instrumentation coordinate homing one tier lower because `identity`, `clock`, `shapes`, and `wire` emit spans from below this owner and no import of theirs reaches it.

## [01]-[INDEX]

- [02]-[OBSERVE]: minted drain taxonomy and the `Drained[T]` tally, the `ScopeKey` composition axis with its scope-bound logger, the counted evidence ring, bracket-cheap `Cost` process spend, the redaction policy, the `measured` span weave, and the inbound context bracket.

## [02]-[OBSERVE]

- Owner: `Ring` is the bounded evidence window every retaining plane parks through — cap, retained facts, and the two loss counters — so an authorized drop is a number a reader subtracts rather than a silence it cannot see: `shed` names what the window evicted under pressure and `lost` names the facts whose own sink refused them, which the window still holds as the evidence that sink never carried. One owner keeps the trim, the count, and the accounting in one place, so no plane re-implements oldest-out beside a counter of its own.
- Owner: `Drained[T]` is the lane's own drain result — the terminal count partition, the values and faults that settled, the threaded session cache, and the drain's spend — so a caller reads what finished off one shape and `facts` is the one projection its log line spreads: the counts per `DRAIN_COLUMNS` column beside the `Cost` facts, so the line carries the denominator its parts sum to while the metrics counter keys the `DRAIN_DISPOSITIONS` carve of that same literal and carries the partition alone.
- Owner: `Cost` is the one process-spend bracket — two `own` reads around a window fold to one `spent` delta through the `psutil` `oneshot` batch — and its `facts`/`measures` pair is the whole projection: a platform-gated column takes the optional slot and the `_volume` fold, so both projections omit its key rather than publishing a zero.
- Law: the composition axis is ONE `ScopeKey` value threaded through the `scope` keyword every custody surface carries — hooks tables, metrics state, the install maps — and `logger` is its first custody surface: the default scope resolves the bare global logger preserving the standing call shape, a non-default scope resolves a `composition`-bound logger, so two compositions' lines partition and self-identify with no second emit surface.
- Entry: `measured` is the one operation-owned span weave stated once — one entry discriminating modality on the dispatch shape, the caller's `facts` mapping stamped at span open with `scope`/`subject` authoritative last, the fault fence INSIDE the live span so a provider raise records on a recording span through the faults-owned conversion, a rail-returning dispatch flattened through the faults-owned `faulted` fold so an offload composes without double-nesting and a carried fault marks the same span the raise path marks, and the status close two-sided — OK set exactly once on the clean exit, ERROR at the rail lift. Its free `scope` parameter refuses off-grammar before a span mints — `SCOPE_ID` is the branch telemetry namespace the hook, meter, and instrument owners already enforce, and this weave is the one exported ingress a sibling package hands a scope, so a bare package-prefixed value refuses here rather than reaching an exporter as an instrumentation scope no backend joins. Its tracer mints through the faults-owned `scoped` stamp, memoized per scope: the API caches no handle, so a per-call mint allocates on the weave's hot path, while the pre-install proxy this memo holds re-reads the global at every `start_span` and upgrades at the install with no invalidation.
- Entry: `attach` is the one context bracket — the loop side injects, the worker kernel extracts through the propagator and attaches around exactly the offloaded body, a placement one fused extract-and-activate scope cannot serve — and the gRPC ingress composes none of it: the `transport/serve#SERVE` interceptor is that seam's one context authority. Before the telemetry install the extract reads the default no-op propagator and the C# parent drops — the mechanical reason the extract sequences after the install.
- Law: every fence resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.OBSERVE`; the dispatch fence keeps the plane's catch-all because a dispatch is caller work, while the scope refusal carries the off-grammar value as its NAMED slot.
- Law: `Redaction` scrubs by key name at EVERY depth and every `hash`-class field renders a stable keyed digest, so two lines carrying the same secret correlate without leaking the value; a producer binds its policy on the logger under `REDACTION_KEY`, never per call, and the chain's `redact` row strips the key before render.
- Growth: a new retaining plane is one `Ring` field on its own custody map, parking through the same two arms rather than minting a second trim or a private drop counter; a new drain outcome is one `DrainOutcome` member with its `Drained` field, reaching the drained line through `DRAIN_COLUMNS` and the metrics counter through the `DRAIN_DISPOSITIONS` carve with zero consumer edits; a new cost column is one `Cost` field reaching the drained line, the crossing bracket, and the `rasm.cost.<measure>` projection through `facts`/`measures` with zero consumer edits; a new classified field one `Redaction` table row; a new redaction transform one `Scrub` member and one `_reduce` arm; a producer's new span fact one `facts` entry at its own call site with zero weave edits; a new composition one `ScopeKey` value threaded through the `scope` keyword, never a sibling registry; a widened instrumentation namespace one `SCOPE_ID` pattern edit.
- Boundary: this page opens spans and binds policy; it configures nothing — the `observability/logging#PIPELINE` owner wires the processor chain and the stdlib bridge every line renders through, no private `LogRecordProcessor`/`OTLPLogExporter` stands beside the composition-root egress, and no second drain vocabulary or upward `lanes` import stands beside the taxonomy this page mints. No semconv or scope-version literal re-spells beside the faults-owned pair, and the stamp stays a coordinate rather than a provider seam: `scoped` passes `None` for the provider slot so the global the `observability/telemetry#TELEMETRY` install published stays the one resolution, and no page beside that install ever names a provider instance.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Awaitable, Callable, Iterator, Mapping, Sequence
from contextlib import contextmanager, suppress
from functools import cache
from hashlib import blake2b
from inspect import isawaitable, iscoroutinefunction
from typing import Final, Literal, assert_never, get_args

import psutil
import structlog
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
from msgspec import Struct, structs
from msgspec.json import Encoder
from opentelemetry import context, trace
from opentelemetry.context import Context
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import OBSERVE_DISPATCH, OBSERVE_SCOPE, BoundaryFault, RuntimeRail, async_boundary, boundary, faulted, scoped
from rasm.runtime.identity import ContentKey

# --- [TYPES] ----------------------------------------------------------------------------

type ScopeKey = str
type DrainOutcome = Literal["accepted", "completed", "cancelled", "rejected", "hit"]
type Scrub = Literal["drop", "mask", "hash"]
type Facts = dict[str, object]
type AttributeValue = str | bool | int | float
type BoundLogger = structlog.typing.FilteringBoundLogger


# --- [CONSTANTS] ------------------------------------------------------------------------

DEFAULT_SCOPE: Final[ScopeKey] = "default"

ADMITTED: Final[DrainOutcome] = "accepted"
DRAIN_COLUMNS: Final[tuple[DrainOutcome, ...]] = get_args(DrainOutcome.__value__)
DRAIN_DISPOSITIONS: Final[tuple[DrainOutcome, ...]] = tuple(column for column in DRAIN_COLUMNS if column != ADMITTED)

SCOPE_ID: Final[re.Pattern[str]] = re.compile(r"\Arasm(\.[a-z0-9_]+)+\Z")

REDACTED: Final[str] = "***"

REDACTION_KEY: Final[str] = "_redaction"

ENCODE: Final[Callable[[object], bytes]] = Encoder(enc_hook=repr, order="deterministic").encode

_PROCESS: Final[psutil.Process] = psutil.Process()

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (psutil.NoSuchProcess, psutil.ZombieProcess, psutil.AccessDenied)

# --- [MODELS] ---------------------------------------------------------------------------


class Ring[T](Struct, frozen=True):
    cap: int
    held: Block[T] = Block.empty()
    shed: int = 0
    lost: int = 0

    def park(self, value: T) -> "Ring[T]":
        filled = self.held.append(Block.singleton(value))
        evicted = max(len(filled) - self.cap, 0)
        return structs.replace(self, held=filled.skip(evicted), shed=self.shed + evicted)

    def refused(self, value: T) -> "Ring[T]":
        return structs.replace(self.park(value), lost=self.lost + 1)

    def moved(self, prior: "Ring[T]") -> tuple[int, int]:
        return self.shed - prior.shed, self.lost - prior.lost

    def facts(self) -> Facts:
        return {"cap": self.cap, "retained": len(self.held), "shed": self.shed, "lost": self.lost}


class Cost(Struct, frozen=True, gc=False):
    cpu_ms: float
    rss_bytes: int
    switches: int
    io_bytes: int | None = None

    @classmethod
    def sampled(cls, process: psutil.Process) -> "Option[Cost]":
        with suppress(*PROCESS_FAULTS), process.oneshot():
            times = process.cpu_times()
            ctx = process.num_ctx_switches()
            return Some(
                cls(
                    cpu_ms=(times.user + times.system) * 1000.0,
                    rss_bytes=process.memory_info().rss,
                    switches=ctx.voluntary + ctx.involuntary,
                    io_bytes=_io_volume(process),
                )
            )
        return Nothing

    @classmethod
    def own(cls) -> "Option[Cost]":
        return cls.sampled(_PROCESS)

    @staticmethod
    def _volume(left: int | None, right: int | None, fold: Callable[[int, int], int]) -> int | None:
        return None if left is None or right is None else fold(left, right)

    @staticmethod
    def spent(closing: "Option[Cost]", opening: "Option[Cost]") -> "Option[Cost]":
        return opening.bind(
            lambda prior: closing.map(
                lambda now: Cost(
                    cpu_ms=max(now.cpu_ms - prior.cpu_ms, 0.0),
                    rss_bytes=now.rss_bytes - prior.rss_bytes,
                    switches=max(now.switches - prior.switches, 0),
                    io_bytes=Cost._volume(now.io_bytes, prior.io_bytes, lambda after, before: max(after - before, 0)),
                )
            )
        )

    @staticmethod
    def combined(left: "Cost", right: "Cost") -> "Cost":
        return Cost(
            cpu_ms=left.cpu_ms + right.cpu_ms,
            rss_bytes=left.rss_bytes + right.rss_bytes,
            switches=left.switches + right.switches,
            io_bytes=Cost._volume(left.io_bytes, right.io_bytes, int.__add__),
        )

    def facts(self) -> Facts:
        held: Facts = {"cpu_ms": self.cpu_ms, "rss_bytes": self.rss_bytes, "switches": self.switches}
        return held if self.io_bytes is None else held | {"io_bytes": self.io_bytes}

    def measures(self) -> dict[str, float]:
        held = {
            "rasm.cost.cpu_time": self.cpu_ms,
            "rasm.cost.memory_delta": float(self.rss_bytes),
            "rasm.cost.ctx_switches": float(self.switches),
        }
        return held if self.io_bytes is None else held | {"rasm.cost.byte_volume": float(self.io_bytes)}


class Drained[T](Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int
    values: Block[T] = Block.empty()
    cache: Map[ContentKey, T] = Map.empty()
    faults: Block[BoundaryFault] = Block.empty()
    hit: int = 0
    cost: Option[Cost] = Nothing

    @staticmethod
    def of[U](
        accepted: int,
        hit: int,
        resolved: Block[tuple[Option[ContentKey], RuntimeRail[U]]],
        replayed: Block[tuple[ContentKey, U]],
        cache: Map[ContentKey, U],
        cost: Option[Cost] = Nothing,
    ) -> "Drained[U]":
        merged = resolved.append(replayed.map(lambda pair: (Some(pair[0]), Ok(pair[1]))))
        completed = resolved.choose(lambda pair: pair[1].to_option())
        faults = resolved.choose(lambda pair: pair[1].swap().to_option())
        threaded = merged.fold(
            lambda acc, pair: pair[0].bind(lambda key: pair[1].to_option().map(lambda v: acc.add(key, v))).default_value(acc), cache
        )
        return Drained(
            accepted=accepted,
            completed=len(completed),
            cancelled=accepted - hit - len(resolved),
            rejected=len(faults),
            values=merged.choose(lambda pair: pair[1].to_option()),
            cache=threaded,
            faults=faults,
            hit=hit,
            cost=cost,
        )

    def facts(self) -> Facts:
        return {column: getattr(self, column) for column in DRAIN_COLUMNS} | self.cost.map(Cost.facts).default_value({})


class Redaction(Struct, frozen=True):
    classified: Map[str, Scrub]
    salt: bytes = b"rasm"

    def apply(self, facts: Facts) -> Facts:
        return {key: redacted for key, value in facts.items() if key != REDACTION_KEY for redacted in self._classify(key, value)}

    def _classify(self, key: str, value: object) -> tuple[object, ...]:
        match self.classified.try_find(key), value:
            case Option(tag="some", some=scrub), _:
                return self._reduce(scrub, value)
            case _, Mapping() as nested:
                return (self.apply(dict(nested)),)
            case _, Sequence() as members if not isinstance(members, str | bytes | bytearray | memoryview):
                return (tuple(held for member in members for held in self._classify(key, member)),)
            case _:
                return (value,)

    def _reduce(self, scrub: Scrub, value: object) -> tuple[object, ...]:
        match scrub:
            case "drop":
                return ()
            case "mask":
                return (REDACTED,)
            case "hash":
                return (blake2b(ENCODE(value), key=self.salt, digest_size=8).hexdigest(),)
            case _ as unreachable:
                assert_never(unreachable)


OPEN: Final[Redaction] = Redaction(classified=Map.empty())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _io_volume(process: psutil.Process) -> int | None:
    if not hasattr(process, "io_counters"):
        return None
    with suppress(*PROCESS_FAULTS):
        counters = process.io_counters()
        return counters.read_bytes + counters.write_bytes
    return None


def logger(scope: ScopeKey = DEFAULT_SCOPE) -> BoundLogger:
    return structlog.get_logger() if scope == DEFAULT_SCOPE else structlog.get_logger(composition=scope)


@contextmanager
def attach(parent: Context) -> Iterator[Context]:
    token = context.attach(parent)
    try:
        yield parent
    finally:
        context.detach(token)


@cache
def _tracer(scope: str) -> trace.Tracer:
    return scoped(trace.get_tracer, scope)


def _flat[T](span: Span, subject: str, value: "T | RuntimeRail[T]") -> "RuntimeRail[T]":
    return value.map_error(lambda fault: faulted(span, subject, fault)) if isinstance(value, Result) else Ok(value)


def _closed[T](span: Span, value: T) -> T:
    span.set_status(Status(StatusCode.OK))
    return value


async def _lifted[T](rail: RuntimeRail[T]) -> RuntimeRail[T]:
    return rail


def measured[T](
    scope: str, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], facts: Mapping[str, AttributeValue] = Map.empty()
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    awaiting = iscoroutinefunction(dispatch) or iscoroutinefunction(getattr(dispatch, "__call__", None))
    if SCOPE_ID.fullmatch(scope) is None:
        refusal: RuntimeRail[T] = Error(OBSERVE_SCOPE.raised(scope))
        return _lifted(refusal) if awaiting else refusal

    def opened() -> Span:
        span = _tracer(scope).start_span(subject)
        if span.is_recording():
            span.set_attributes({**facts, "scope": scope, "subject": subject})
        return span

    async def settled(span: Span, pending: Callable[[], Awaitable[T]]) -> RuntimeRail[T]:
        with trace.use_span(span, end_on_exit=True):
            railed = await async_boundary(OBSERVE_DISPATCH, pending, catch=Exception)
            return railed.bind(lambda value: _flat(span, subject, value)).map(lambda live: _closed(span, live))

    if awaiting:
        return settled(opened(), dispatch)
    span = opened()
    with trace.use_span(span, end_on_exit=False):
        railed = boundary(OBSERVE_DISPATCH, dispatch, catch=Exception).bind(lambda value: _flat(span, subject, value))
        match railed:
            case Result(tag="ok", ok=pending) if isawaitable(pending):
                return settled(span, lambda: pending)
            case _:
                outcome = railed.map(lambda value: _closed(span, value))
                span.end()
                return outcome
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
