# [PY_RUNTIME_RECEIPTS]

This owner produces local evidence — typed receipts and structured log facts — and contributes nothing to provider install, product telemetry export, or health. Its drain taxonomy is MINTED here: a drain receipt IS local evidence, so `DrainOutcome`, the derived `DRAIN_COLUMNS`, and `DrainReceipt[T]` live on this page, and `execution/lanes#LANE` (the producing drain) and `observability/metrics#METRIC` (the `lane.drained` counter) import the taxonomy FROM this owner — no upward import survives and the column set can never drift.

`ReceiptContributor` is the Protocol port every sibling's typed receipt streams through, and `@receipted` is the aspect that harvests a measured operation's contributor stream and emits on exit, so receipt production is a decorator rail, never inline `emit` calls scattered through the siblings. `measured` is the one call-shaped evidence weave — span from the caller's scope, fault fence, rail flatten, fenced harvest, OK close — every branch evidence producer composes with its own scope vocabulary and redaction row, so a package-local tracer mint, a second span-fence-emit aspect, or an inline span open beside it has no owner. `Redaction` is a `Classification`-keyed field policy applied by a chain-resident processor. OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`, since `structlog` mints no native OTLP export, and the inbound `continue_inbound`/`attach` pair realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` Python leg against the C# `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` producer.

## [01]-[INDEX]

- [01]-[RECEIPT]: the minted drain taxonomy, the self-projecting receipt union, the contributor port, the `@receipted` aspect and the `measured` weave, the `emit`/`emit_async` sink pair, the chain-resident redaction, and the inbound trace-context pair.

## [02]-[RECEIPT]

- Owner: `Receipt` owns its own projection — every case folds to a `(LogLevel, name, EventDict)` triple through one total `project`, the event name its own slot, never packed under an `"event"` key the sink re-pops — so `Signals.emit` is a renderer-agnostic fold, never three hand-built dict arms. Case log disposition is data: the `fact` case reads its level off `PHASE_LEVEL`, so a new phase is one row, never a phase branch.
- Cases: the three lifecycle phases share the one `fact` case as a `Phase` value, never three identical-payload sibling cases. `rejected` carries the whole fault and spreads the `reliability/faults#FAULT`-owned `BoundaryFault.facts()` projection — the subject is never a pre-extracted slot beside the fault, and no private fault walk re-implements the owner's fold. Correlation flows through `merge_contextvars`, never a per-case field.
- Entry: `Signals.emit`/`emit_async` are polymorphic on both axes — input normalized through one `_stream`, output any `FilteringBoundLogger`, so a `capture_logs` test or `wrap_logger` consumer drives the same fold without a second emit surface. `emit_async` awaits the `a*` mirror, so a high-volume async serve path offloads render-and-sink to a worker thread rather than blocking the event loop. This `continue_inbound`/`attach` split is load-bearing at the `execution/lanes#LANE` offload stitch — the loop side injects, the worker kernel extracts then attaches around exactly the offloaded body, a placement one fused extract-and-activate scope cannot serve — and the gRPC ingress composes neither: the `transport/serve#SERVE` interceptor is that seam's one context authority. Before the telemetry install the extract reads the default no-op propagator and the C# parent drops — the mechanical reason the extract sequences after the install.
- Auto: `@receipted` is parameterized over the concrete contributor type through the `R: ReceiptContributor` bound, so a decorated operation statically returns its concrete receipt rather than collapsing to the bare Protocol, and a consumer's `Ok` arm reads concrete members without a static error. Span creation belongs to the measured operation, never to emission — emit writes under whatever span is active, and `measured` is that operation-owned weave stated once: one entry discriminating modality on the dispatch shape, the fault fence INSIDE the live span so a provider raise records on a recording span, a rail-returning dispatch flattened so an offload composes without double-nesting, emission fenced so a render or sink raise folds onto the rail, and the OK status set exactly once on the clean exit. Its drained projection reads the outcome counts per column off `DRAIN_COLUMNS` — a full `asdict` allocates the receipt's containers per emit only to drop them — and the metrics counter keys the identical columns, so the line and the counter cannot disagree. A `hash`-class field renders a stable correlation token, so two lines carrying the same secret correlate without leaking the value. Receipts' RSS slot is a point fact and the metrics gauge the stream over one `psutil` source, each owner holding its own handle.
- Growth: a new drain outcome is one `DrainOutcome` member plus one `DrainReceipt` field, reaching the drained line, the metrics counter, and the lanes fold through the one `DRAIN_COLUMNS` derivation with zero consumer edits; a new lifecycle phase one `Phase` literal plus one `PHASE_LEVEL` row; a distinct-payload evidence kind one `Receipt` case plus its `project` and `of` arms; a new classified field one `Redaction` table row; a new classification transform one `Classification` member plus one `_reduce` arm; a new log level one `LogLevel` literal plus one `LEVEL_METHOD` row reaching the floor and both emit arms at once; a new sink target the `sink` argument, never a second emit method.
- Boundary: `Signals.configure` wires the one stdlib `ProcessorFormatter` bridge, so a foreign `logging` record — grpcio, apscheduler, an executor pool — renders through the same processor chain under the keep-all `OPEN` fold and no stdlib `logging` call exists outside that bridge; no private `LogRecordProcessor`/`OTLPLogExporter` beside the telemetry-installed egress, and no second drain vocabulary or upward `lanes` import beside the taxonomy this page mints.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import logging  # the ProcessorFormatter bridge in Signals.configure is the one sanctioned stdlib-logging call site
from collections.abc import Awaitable, Callable, Iterable, Iterator, Mapping
from contextlib import contextmanager, suppress
from functools import cache, wraps
from hashlib import blake2b
from inspect import isawaitable, iscoroutinefunction
from typing import Final, Literal, Protocol, assert_never, get_args, runtime_checkable

import psutil
import structlog
from expression import Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import Encoder
from opentelemetry import context, propagate, trace
from opentelemetry.trace import Span, Status, StatusCode
from opentelemetry.context import Context

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.identity import ContentKey

# --- [TYPES] ----------------------------------------------------------------------------

type DrainOutcome = Literal["accepted", "completed", "cancelled", "rejected", "hit"]
type Phase = Literal["admitted", "planned", "emitted"]
type LogLevel = Literal["debug", "info", "warning", "error"]
type EventDict = dict[str, object]
type Classification = Literal["drop", "mask", "hash"]
type ReceiptEvidence = tuple[Phase, str, dict[str, object]] | BoundaryFault | DrainReceipt[object]  # evidence/evidence owns the branch-facing `Evidence` union
type Streamable = Receipt | Iterable[Receipt] | ReceiptContributor
type Contributing[**P, R: ReceiptContributor] = Callable[P, R] | Callable[P, Awaitable[R]]
type BoundLogger = structlog.typing.FilteringBoundLogger
type LevelSelector = Callable[[BoundLogger], Callable[..., object]]
type LevelBinding = tuple[int, LevelSelector, LevelSelector]

# --- [CONSTANTS] ------------------------------------------------------------------------

# column set IS the DrainOutcome literal, so the drained line, the metrics counter, and the lanes fold can never disagree.
DRAIN_COLUMNS: Final[tuple[DrainOutcome, ...]] = get_args(DrainOutcome.__value__)

PHASE_LEVEL: Final[Map[Phase, LogLevel]] = Map.of_seq([("admitted", "debug"), ("planned", "debug"), ("emitted", "info")])

REDACTED: Final[str] = "***"

# reserved event key the per-emit Redaction rides so the chain-resident `redact` scrubs the whole line; `apply` strips it before render.
_REDACTION: Final[str] = "_redaction"

# one row per LogLevel carrying (numeric level, sync selector, async mirror) — configure derives the filter floor and both emit arms bind
# their selector halves; never a stringly `getattr(log, level)` and never a bare numeric floor literal beside the row that owns it.
LEVEL_METHOD: Final[Map[LogLevel, LevelBinding]] = Map.of_seq([
    ("debug", (10, lambda log: log.debug, lambda log: log.adebug)),
    ("info", (20, lambda log: log.info, lambda log: log.ainfo)),
    ("warning", (30, lambda log: log.warning, lambda log: log.awarning)),
    ("error", (40, lambda log: log.error, lambda log: log.aerror)),
])

# enc_hook=repr degrades non-native values instead of raising on the hot path; order="deterministic" fixes key order so the one
# encoder doubles as the canonical hash-class input. encode_into's reused-buffer form stays out: the renderer contract returns
# each line's own bytes and emit crosses threads, so a shared buffer would race.
_ENCODE: Final[Callable[[object], bytes]] = Encoder(enc_hook=repr, order="deterministic").encode

# own-process handle minted once; the metrics sibling carries its own handle.
_PROCESS: Final[psutil.Process] = psutil.Process()

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (psutil.NoSuchProcess, psutil.ZombieProcess, psutil.AccessDenied)

# --- [MODELS] ---------------------------------------------------------------------------


class DrainReceipt[T](Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int
    values: Block[T] = Block.empty()
    cache: Map[ContentKey, T] = Map.empty()
    faults: Block[BoundaryFault] = Block.empty()
    hit: int = 0

    @staticmethod
    def of[U](
        accepted: int,
        hit: int,
        resolved: Block[tuple[Option[ContentKey], RuntimeRail[U]]],
        replayed: Block[tuple[ContentKey, U]],
        cache: Map[ContentKey, U],
    ) -> "DrainReceipt[U]":
        merged = resolved.append(replayed.map(lambda pair: (Some(pair[0]), Ok(pair[1]))))
        completed = resolved.choose(lambda pair: pair[1].to_option())
        faults = resolved.choose(lambda pair: pair[1].swap().to_option())
        threaded = merged.fold(
            lambda acc, pair: pair[0].bind(lambda key: pair[1].to_option().map(lambda v: acc.add(key, v))).default_value(acc), cache
        )
        return DrainReceipt(
            accepted=accepted,
            completed=len(completed),
            cancelled=accepted - hit - len(resolved),
            rejected=len(faults),
            values=merged.choose(lambda pair: pair[1].to_option()),
            cache=threaded,
            faults=faults,
            hit=hit,
        )


@tagged_union(frozen=True)
class Receipt:
    tag: Literal["fact", "rejected", "drained"] = tag()
    fact: tuple[Phase, str, str, dict[str, object]] = case()
    rejected: tuple[str, BoundaryFault] = case()
    drained: tuple[str, DrainReceipt[object]] = case()

    @staticmethod
    def of(owner: str, evidence: ReceiptEvidence) -> "Receipt":
        match evidence:
            case BoundaryFault() as fault:
                return Receipt(rejected=(owner, fault))
            case DrainReceipt() as drain:
                return Receipt(drained=(owner, drain))
            case (phase, subject, facts):
                return Receipt(fact=(phase, owner, subject, facts))
            case _ as unreachable:
                assert_never(unreachable)

    def project(self) -> tuple[LogLevel, str, EventDict]:
        # event name is its own slot — never packed under an "event" key the sink re-pops.
        match self:
            case Receipt(tag="fact", fact=(phase, owner, subject, facts)):
                return PHASE_LEVEL[phase], phase, {"owner": owner, "subject": subject, **facts}
            case Receipt(tag="rejected", rejected=(owner, fault)):
                return "warning", "rejected", {"owner": owner, **fault.facts()}
            case Receipt(tag="drained", drained=(owner, drain)):
                return "info", "drained", {"owner": owner, **_rss(), **{column: getattr(drain, column) for column in DRAIN_COLUMNS}}
            case _ as unreachable:
                assert_never(unreachable)


@runtime_checkable
class ReceiptContributor(Protocol):
    def contribute(self) -> Iterable[Receipt]: ...


class Redaction(Struct, frozen=True):
    classified: Map[str, Classification]
    salt: bytes = b"rasm"

    def apply(self, facts: EventDict) -> EventDict:
        return {key: redacted for key, value in facts.items() if key != _REDACTION for redacted in self._classify(key, value)}

    def _classify(self, key: str, value: object) -> tuple[object, ...]:
        return self.classified.try_find(key).map(lambda cls: self._reduce(cls, value)).default_value((value,))

    def _reduce(self, classification: Classification, value: object) -> tuple[object, ...]:
        match classification:
            case "drop":
                return ()
            case "mask":
                return (REDACTED,)
            case "hash":
                return (blake2b(_ENCODE(value), key=self.salt, digest_size=8).hexdigest(),)
            case _ as unreachable:
                assert_never(unreachable)


# public keep-all policy a sibling with no classified field threads into @receipted(OPEN); depends on the Redaction model, so it anchors here.
OPEN: Final[Redaction] = Redaction(classified=Map.empty())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _rss() -> EventDict:
    with suppress(*PROCESS_FAULTS):
        return {"rss": _PROCESS.memory_info().rss}
    return {}


# one default-sink resolution both emit arms share: absent folds to the global get_logger fetched per emit, never cached.
def _sink(sink: BoundLogger | None) -> BoundLogger:
    return Option.of_optional(sink).default_with(structlog.get_logger)


def _stream(source: Streamable) -> Iterable[Receipt]:
    match source:
        case Receipt():
            return (source,)
        case ReceiptContributor():
            return source.contribute()
        case _:
            return source


# one render fold both sinks share — emit/emit_async differ only by which selector half they drive. Redaction is NOT applied here:
# chain-resident `redact` runs after the contextvars/trace injectors; this fold only binds it under `_REDACTION`.
def _render(source: Streamable, redaction: Redaction) -> Iterator[tuple[LevelBinding, str, EventDict]]:
    for receipt in _stream(source):
        level, name, fields = receipt.project()
        yield LEVEL_METHOD[level], name, fields | {_REDACTION: redaction}


# scrubs the fully-assembled line — receipt facts, ambient contextvars fields, trace ids — so a bind_contextvars-sourced classified
# field cannot bypass redaction; a line with no bound Redaction (a foreign stdlib-bridge record) folds through the keep-all OPEN policy.
def redact(_: object, __: str, event: EventDict) -> EventDict:
    bound = event.get(_REDACTION)
    return (bound if isinstance(bound, Redaction) else OPEN).apply(event)


def trace_context(_: object, __: str, event: EventDict) -> EventDict:
    ctx = trace.get_current_span().get_span_context()
    if ctx.is_valid:
        event.update(trace_id=trace.format_trace_id(ctx.trace_id), span_id=trace.format_span_id(ctx.span_id), trace_flags=int(ctx.trace_flags))
    return event


# --- [SERVICES] -------------------------------------------------------------------------

# the ONE stdlib bridge handle: identity-stable across re-configures, so repeated Signals.configure re-formats this
# handler in place instead of stacking siblings or force-resetting host handlers an embedding process installed.
_BRIDGE: Final[logging.Handler] = logging.StreamHandler()


class Signals:
    @staticmethod
    def configure(floor: LogLevel = "info") -> None:
        # additional_ignores skips the emit machinery's own frames, so FUNC_NAME/LINENO name the producing owner, never `_render`.
        shared = [
            structlog.contextvars.merge_contextvars,
            structlog.processors.add_log_level,
            trace_context,
            redact,
            structlog.processors.CallsiteParameterAdder(additional_ignores=["rasm.runtime.receipts"]),
            structlog.processors.dict_tracebacks,
            structlog.processors.TimeStamper(fmt="iso"),
        ]
        structlog.configure(
            processors=[*shared, structlog.processors.EventRenamer(to="body"), structlog.processors.JSONRenderer(serializer=_ENCODE)],
            wrapper_class=structlog.make_filtering_bound_logger(LEVEL_METHOD[floor][0]),
            logger_factory=structlog.BytesLoggerFactory(),
        )
        # one stdlib bridge: a foreign logging record (grpcio, apscheduler, an executor pool) rides the SAME shared chain with
        # keep-all OPEN covering its unredacted line, so no record bypasses trace correlation or the JSON render. Install is
        # idempotent and additive — the identity-guarded `_BRIDGE` lands once and re-formats on re-configure, host handlers
        # survive untouched, and the root level lowers to the floor so bridged records reach the formatter.
        _BRIDGE.setFormatter(
            structlog.stdlib.ProcessorFormatter(
                foreign_pre_chain=shared,
                processors=[
                    structlog.stdlib.ProcessorFormatter.remove_processors_meta,
                    structlog.processors.EventRenamer(to="body"),
                    structlog.processors.JSONRenderer(serializer=lambda line, **_kw: _ENCODE(line).decode()),
                ],
            )
        )
        root = logging.getLogger()
        if _BRIDGE not in root.handlers:
            root.addHandler(_BRIDGE)
        root.setLevel(LEVEL_METHOD[floor][0])

    @staticmethod
    def emit(source: Streamable, redaction: Redaction, sink: BoundLogger | None = None) -> None:
        log = _sink(sink)
        for (_, sync, _), name, fields in _render(source, redaction):
            sync(log)(name, **fields)

    @staticmethod
    async def emit_async(source: Streamable, redaction: Redaction, sink: BoundLogger | None = None) -> None:
        log = _sink(sink)
        for (_, _, amirror), name, fields in _render(source, redaction):
            await amirror(log)(name, **fields)

    @staticmethod
    def continue_inbound(carrier: Mapping[str, str]) -> Context:
        return propagate.extract(carrier)

    @staticmethod
    @contextmanager
    def attach(parent: Context) -> Iterator[Context]:
        token = context.attach(parent)
        try:
            yield parent
        finally:
            context.detach(token)


@cache
def _tracer(scope: str) -> trace.Tracer:
    return trace.get_tracer(scope)


def _flat[T](value: "T | RuntimeRail[T]") -> "RuntimeRail[T]":
    # a rail-returning dispatch (a lane offload, a fenced fold) already carries its rail; a bare value lifts Ok.
    return value if isinstance(value, Result) else Ok(value)


def _harvested[T](value: T, redaction: Redaction) -> T:
    # fenced emit step: a ReceiptContributor streams its receipts, a plain value passes through untouched.
    if isinstance(value, ReceiptContributor):
        Signals.emit(value, redaction)
    return value


async def _harvested_async[T](value: T, redaction: Redaction) -> T:
    # async mirror of the fenced emit step: the woven arm awaits `Signals.emit_async` so a contributor harvest
    # never runs a sync sink on the event loop.
    if isinstance(value, ReceiptContributor):
        await Signals.emit_async(value, redaction)
    return value


def _closed[T](span: Span, value: T) -> T:
    span.set_status(Status(StatusCode.OK))
    return value


def measured[T](
    scope: str, subject: str, redaction: Redaction, dispatch: Callable[[], T] | Callable[[], Awaitable[T]]
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # Exemption: the span lifecycle and the modality probe are the platform-forced seam every evidence producer
    # composes instead of re-authoring; emission runs inside its own fence, so an emit-time raise folds onto the rail.
    def opened() -> Span:
        span = _tracer(scope).start_span(subject)
        if span.is_recording():
            span.set_attributes({"scope": scope, "subject": subject})
        return span

    async def settled(span: Span, pending: Callable[[], Awaitable[T]]) -> RuntimeRail[T]:
        # one async settle for both modalities: the declared-async arm hands in the span it just opened, the sync arm
        # hands the SAME still-open span its dispatch ran under — a sync-minted awaitable finishes inside the original
        # span, never a sibling pair whose first member closed un-statused at the mint.
        with trace.use_span(span, end_on_exit=True):
            rail = (await async_boundary(f"{scope}.{subject}", pending)).bind(_flat)
            match rail:  # one total carrier match — `expression` ships no async builder, and the harvest awaits emit_async
                case Result(tag="ok", ok=value):
                    return (await async_boundary(f"{scope}.emit", lambda: _harvested_async(value, redaction))).map(
                        lambda live: _closed(span, live)
                    )
                case _:
                    return rail

    # declared modality routes first: a coroutine function or an async-`__call__` instance never touches the sync arm.
    if iscoroutinefunction(dispatch) or iscoroutinefunction(getattr(dispatch, "__call__", None)):
        return settled(opened(), dispatch)
    span = opened()
    with trace.use_span(span, end_on_exit=False):
        railed = boundary(f"{scope}.{subject}", dispatch).bind(_flat)
        match railed:
            case Result(tag="ok", ok=pending) if isawaitable(pending):
                # a sync-declared callable can still RETURN an awaitable (a lambda minting a coroutine); the settle
                # continues under the SAME open span, which ends when the awaited half resolves — Ok(coroutine) never escapes.
                return settled(span, lambda: pending)
            case _:
                outcome = railed.bind(lambda value: boundary(f"{scope}.emit", lambda: _harvested(value, redaction))).map(
                    lambda value: _closed(span, value)
                )
                span.end()
                return outcome


def receipted[**P, R: ReceiptContributor](redaction: Redaction) -> Callable[[Contributing[P, R]], Contributing[P, R]]:
    def wrap(operation: Contributing[P, R]) -> Contributing[P, R]:
        if iscoroutinefunction(operation):

            @wraps(operation)
            async def harvested_async(*args: P.args, **kwargs: P.kwargs) -> R:
                contributor = await operation(*args, **kwargs)
                await Signals.emit_async(contributor, redaction)
                return contributor

            return harvested_async

        @wraps(operation)
        def harvested(*args: P.args, **kwargs: P.kwargs) -> R:
            contributor = operation(*args, **kwargs)
            Signals.emit(contributor, redaction)
            return contributor

        return harvested

    return wrap
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
