# [PY_RUNTIME_RECEIPTS]

This owner produces local evidence — typed receipts and structured log facts — and contributes nothing to provider install, product telemetry export, or health. Its drain taxonomy is MINTED here: a drain receipt IS local evidence, so `DrainOutcome`, the derived `DRAIN_COLUMNS` beside the `DRAIN_DISPOSITIONS` carve that strips the admitted total off the terminal partition, and `DrainReceipt[T]` live on this page, and `execution/lanes#LANE` (the producing drain) and `observability/metrics#METRIC` (the `rasm.lane.drained` counter) import the taxonomy FROM this owner — no upward import survives and neither column set can drift.

`ReceiptContributor` is the Protocol port every sibling's typed receipt streams through; `@receipted` harvests a contributor's stream on exit, so receipt production is a decorator rail, never scattered inline `emit` calls. `measured` is the one call-shaped evidence weave — span from the caller's scope, fault fence, rail flatten, fenced harvest, two-sided status close — the folder's sole span-lifecycle owner. `Redaction` is a `Classification`-keyed field policy the emit fold binds under `REDACTION_KEY`, classifying by key name at every depth; the `observability/logging#PIPELINE` chain applies it to the assembled line and ships under its `LogShip` law. Inbound context rides the `continue_inbound`/`attach` pair — the cross-`libs/` `ONE_DISTRIBUTED_TRACE` Python leg against the C# `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` producer. This page mints the `ScopeKey` composition axis naming which embedded composition owns a custody row, and composes the `reliability/faults#FAULT` `scoped` stamp naming which library emitted a signal — the instrumentation coordinate homing one tier lower because `identity`, `clock`, `shapes`, and `wire` emit spans from below this owner and no import of theirs reaches it.

## [01]-[INDEX]

- [02]-[RECEIPT]: minted drain taxonomy, the `ScopeKey` composition axis and its default-sink resolution, bracket-cheap `Cost` process-spend evidence, the self-projecting receipt union, the contributor port, the `@receipted` aspect and the `measured` weave, the `emit`/`emit_async` sink pair, the emit-bound redaction policy, and the inbound trace-context pair.

## [02]-[RECEIPT]

- Owner: `Receipt` owns its own projection — every case folds to a `(LogLevel, name, EventDict)` triple through one total `project`, the event name its own slot, never packed under an `"event"` key the sink re-pops — so `Signals.emit` is a renderer-agnostic fold, never three hand-built dict arms. Case log disposition is data: the `fact` case reads its level off `PHASE_LEVEL`, so a new phase is one row, never a phase branch.
- Cases: the three lifecycle phases share the one `fact` case as a `Phase` value, never three identical-payload sibling cases. `rejected` carries the whole fault and spreads the `reliability/faults#FAULT`-owned `BoundaryFault.facts()` projection — the subject is never a pre-extracted slot beside the fault, and no private fault walk re-implements the owner's fold. Correlation flows through `merge_contextvars`, never a per-case field.
- Entry: `Signals.emit`/`emit_async` are polymorphic on both axes — input normalized through one `_stream`, output any `FilteringBoundLogger`, so a `capture_logs` test or `wrap_logger` consumer drives the same fold without a second emit surface. This page mints the folder's composition-scope vocabulary — `ScopeKey` and the pinned `DEFAULT_SCOPE` spelling every scope-keyed custody surface (hooks tables, metrics state, the install-receipt maps) imports from here, receipts being the observability tier below every consumer — and the default-sink resolution is its first custody surface: the default scope resolves the bare global logger preserving the standing call shape, a non-default scope resolves a `composition`-bound logger, so two compositions' lines partition and self-identify with no second emit surface. `emit_async` awaits the `a*` mirror, so a high-volume async serve path offloads render-and-sink to a worker thread rather than blocking the event loop. This `continue_inbound`/`attach` split is load-bearing at the `execution/lanes#LANE` offload stitch — the loop side injects, the worker kernel extracts then attaches around exactly the offloaded body, a placement one fused extract-and-activate scope cannot serve — and the gRPC ingress composes neither: the `transport/serve#SERVE` interceptor is that seam's one context authority. Before the telemetry install the extract reads the default no-op propagator and the C# parent drops — the mechanical reason the extract sequences after the install. Its free `scope` parameter refuses off-grammar before a span mints — `SCOPE_ID` is the branch telemetry namespace the hook, meter, and instrument owners already enforce, and this weave is the one exported ingress a sibling package hands a scope, so a bare package-prefixed value refuses here rather than reaching an exporter as an instrumentation scope no backend joins. Its tracer mints through the faults-owned `scoped` stamp, memoized per scope: the API caches no handle, so a per-call mint allocates on the weave's hot path, while the pre-install proxy this memo holds re-reads the global at every `start_span` and upgrades at the install with no invalidation.
- Auto: `@receipted` is parameterized over the concrete contributor type through the `R: ReceiptContributor` bound, so a decorated operation statically returns its concrete receipt rather than collapsing to the bare Protocol, and a consumer's `Ok` arm reads concrete members without a static error. Its admitted `ScopeKey` reaches both contributor harvest arms, so a composition never falls back to the default sink during decorator emission. Span creation belongs to the measured operation, never to emission — emit writes under whatever span is active, and `measured` is that operation-owned weave stated once: one entry discriminating modality on the dispatch shape, the caller's `facts` mapping stamped at span open with `scope`/`subject` authoritative last, the fault fence INSIDE the live span so a provider raise records on a recording span, a rail-returning dispatch flattened so an offload composes without double-nesting, emission fenced under the caller's `composition` key so a render or sink raise folds onto the rail, and the status close two-sided — OK set exactly once on the clean exit, ERROR with the fault-fact event landing at the rail lift, so a pre-railed fault marks the same span the raise path marks through the faults-owned conversion and the OTLP trace carries every fault the receipt stream reports. Its drained projection reads the outcome counts per column off `DRAIN_COLUMNS` — a full `asdict` allocates the receipt's containers per emit only to drop them — while the metrics counter keys the `DRAIN_DISPOSITIONS` carve of that same literal, so the line carries the denominator its parts sum to and the counter carries the partition alone. Every `hash`-class field renders a stable correlation token, so two lines carrying the same secret correlate without leaking the value. Receipts' RSS slot is a point fact and the metrics gauge the stream over one `psutil` source, each owner holding its own handle.
- Growth: a new drain outcome is one `DrainOutcome` member with its `DrainReceipt` field, reaching the drained line through `DRAIN_COLUMNS` and the metrics counter through the `DRAIN_DISPOSITIONS` carve with zero consumer edits; a new cost column is one `Cost` field reaching the drained line, the crossing bracket, and the `rasm.cost.<measure>` projection through `facts`/`measures` with zero consumer edits, a platform-gated one taking the optional slot and the `_volume` fold so both projections omit its key rather than publishing a zero; a new lifecycle phase one `Phase` literal and one `PHASE_LEVEL` row; a distinct-payload evidence kind one `Receipt` case with its `project` and `of` arms; a new classified field one `Redaction` table row; a new redaction transform one `Scrub` member and one `_reduce` arm; a producer's new crossing fact one `facts` entry at its own call site with zero weave edits; a new log level one `LogLevel` literal and one `LEVEL_METHOD` row reaching the floor and both emit arms at once; a new sink target the `sink` argument, never a second emit method; a new composition one `ScopeKey` value threaded through the `scope` and `composition` keywords, never a sibling registry; a widened instrumentation namespace one `SCOPE_ID` pattern edit.
- Boundary: the `observability/logging#PIPELINE` owner wires the processor chain and the stdlib bridge this fold renders through — this page emits and never configures; no private `LogRecordProcessor`/`OTLPLogExporter` beside the composition-root egress, and no second drain vocabulary or upward `lanes` import beside the taxonomy this page mints. No semconv or scope-version literal re-spells beside the faults-owned pair, and the stamp stays a coordinate rather than a provider seam: `scoped` passes `None` for the provider slot so the global the `observability/telemetry#TELEMETRY` install published stays the one resolution, and no page beside that install ever names a provider instance.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Awaitable, Callable, Iterable, Iterator, Mapping, Sequence
from contextlib import contextmanager, suppress
from functools import cache, partial, wraps
from hashlib import blake2b
from inspect import isawaitable, iscoroutinefunction
from typing import Final, Literal, Protocol, assert_never, get_args, runtime_checkable

import psutil
import structlog
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import Encoder
from opentelemetry import context, propagate, trace
from opentelemetry.context import Context
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary, scoped
from rasm.runtime.identity import ContentKey

# --- [TYPES] ----------------------------------------------------------------------------

type ScopeKey = str
type DrainOutcome = Literal["accepted", "completed", "cancelled", "rejected", "hit"]
type Phase = Literal["admitted", "planned", "emitted"]
type LogLevel = Literal["debug", "info", "warning", "error", "critical"]
type EventDict = dict[str, object]
type Scrub = Literal["drop", "mask", "hash"]
type ReceiptEvidence = tuple[Phase, str, dict[str, object]] | BoundaryFault | DrainReceipt[object]  # evidence/evidence owns the branch-facing `Evidence` union
type Streamable = Receipt | Iterable[Receipt] | ReceiptContributor
type Contributing[**P, R: ReceiptContributor] = Callable[P, R] | Callable[P, Awaitable[R]]
type BoundLogger = structlog.typing.FilteringBoundLogger
type LevelSelector = Callable[[BoundLogger], Callable[..., object]]
type LevelBinding = tuple[int, LevelSelector, LevelSelector]


# --- [CONSTANTS] ------------------------------------------------------------------------

# pinned composition-scope default: every scope-keyed custody surface (hooks tables, metrics state, install-receipt
# maps, this page's default-sink resolution) imports this one spelling, so the bare call shape stays scope-free.
DEFAULT_SCOPE: Final[ScopeKey] = "default"

# One literal owns the drain vocabulary and both tuples derive from it, so the drained line, the metrics counter, and the
# lanes fold can never disagree on a column and a new disposition reaches all three from one member. `accepted` is the
# ADMITTED TOTAL the terminal columns exactly partition — `DrainReceipt.of` derives `cancelled` as the remainder, so
# hit + completed + rejected + cancelled == accepted by construction. The line reports both because a reader wants the
# denominator beside its parts; the tag-partitioned counter folds `DRAIN_DISPOSITIONS` alone, where a total riding as one
# more dimension VALUE makes every sum across that dimension read twice the work drained.
ADMITTED: Final[DrainOutcome] = "accepted"
DRAIN_COLUMNS: Final[tuple[DrainOutcome, ...]] = get_args(DrainOutcome.__value__)
DRAIN_DISPOSITIONS: Final[tuple[DrainOutcome, ...]] = tuple(column for column in DRAIN_COLUMNS if column != ADMITTED)

PHASE_LEVEL: Final[Map[Phase, LogLevel]] = Map.of_seq([("admitted", "debug"), ("planned", "debug"), ("emitted", "info")])

# instrumentation-scope grammar the whole branch already enforces at its hook, meter, and instrument owners, spelled
# HERE because `measured` is the one exported ingress carrying a FREE scope a sibling package supplies. Anchors are
# `\\A`/`\\Z` rather than `^`/`$`, so the pattern bounds itself under every match verb: `$` matches ahead of a
# trailing newline, handing a `search` or `match` caller two scopes that render alike.
SCOPE_ID: Final[re.Pattern[str]] = re.compile(r"\Arasm(\.[a-z0-9_]+)+\Z")

REDACTED: Final[str] = "***"

# reserved event key the per-emit Redaction rides so the logging-owned chain processor scrubs the whole line; `apply` strips it pre-render.
REDACTION_KEY: Final[str] = "_redaction"

# one row per LogLevel carrying (numeric level, sync selector, async mirror) — configure derives the filter floor and both emit arms bind
# their selector halves; never a stringly `getattr(log, level)` and never a bare numeric floor literal beside the row that owns it.
LEVEL_METHOD: Final[Map[LogLevel, LevelBinding]] = Map.of_seq([
    ("debug", (10, lambda log: log.debug, lambda log: log.adebug)),
    ("info", (20, lambda log: log.info, lambda log: log.ainfo)),
    ("warning", (30, lambda log: log.warning, lambda log: log.awarning)),
    ("error", (40, lambda log: log.error, lambda log: log.aerror)),
    ("critical", (50, lambda log: log.critical, lambda log: log.acritical)),
])

# enc_hook=repr degrades non-native values instead of raising on the hot path; order="deterministic" fixes key order so the one
# encoder doubles as the canonical hash-class input and the logging-owned render serializer. encode_into's reused-buffer form
# stays out: the renderer contract returns each line's own bytes and emit crosses threads, so a shared buffer would race.
ENCODE: Final[Callable[[object], bytes]] = Encoder(enc_hook=repr, order="deterministic").encode

# own-process handle minted once; the metrics sibling carries its own handle.
_PROCESS: Final[psutil.Process] = psutil.Process()

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (psutil.NoSuchProcess, psutil.ZombieProcess, psutil.AccessDenied)

# --- [MODELS] ---------------------------------------------------------------------------


class Cost(Struct, frozen=True, gc=False):
    # bracket-cheap process-spend evidence: two `sampled` reads per bracket, `spent` the spend between them. `rss_bytes`
    # is an instantaneous sample whose signed delta attributes retained-memory change without claiming an unobserved peak.
    # Every measured slot is REQUIRED, so no construction publishes a reading nobody took and the type carries no zeroed
    # identity a fold could seed from. `io_bytes` alone spells absence, its counter platform-gating whole — macOS binds
    # no `io_counters` at all — and an unavailable counter erases neither the CPU, RSS, nor switch sample. That absence
    # rides both projections as an OMITTED key, exactly as an unfilled dimension does, never as the zero a
    # `rasm.cost.byte_volume` distribution would then trend as a real transfer that moved nothing.
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
        # a vanished, zombied, or access-denied process MEASURED NOTHING, and this is the one shape that says so —
        # `ProcessReading.sample` takes that same absence at the gauge owner, held here for the same reason. A zeroed
        # `Cost` in its place is not a small error: the reads are CUMULATIVE process counters, so a failed OPENING
        # read makes the closing one's whole process lifetime read as this bracket's spend, and a failed CLOSING read
        # publishes zero CPU beside a hugely negative RSS delta. Both land on `rasm.cost.*` as measured facts.
        return Nothing

    @classmethod
    def own(cls) -> "Option[Cost]":
        return cls.sampled(_PROCESS)

    @staticmethod
    def _volume(left: int | None, right: int | None, fold: Callable[[int, int], int]) -> int | None:
        # Gated axes fold only where BOTH ends measured. Counter presence is a platform fact fixed for the whole
        # process, so this never drops a real reading, and seeding the missing half at zero would report a bracket's
        # whole transfer as the other half's alone.
        return None if left is None or right is None else fold(left, right)

    @staticmethod
    def spent(closing: "Option[Cost]", opening: "Option[Cost]") -> "Option[Cost]":
        # a bracket has a spend only when BOTH ends read: absence on either side is what forecloses attributing a
        # cumulative counter's whole history to one window. RSS stays signed because retained-memory change is
        # legitimately negative; the three monotonic counters floor at zero, where a negative reading can only mean
        # a replacement process stood between the two reads.
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
        # receipts-semigroup cost instance: aggregation over brackets, never a generic accumulator. It carries no
        # identity element because a zero-filled `Cost` is a reading no bracket took, so a fold over brackets seeds
        # from its own first member.
        return Cost(
            cpu_ms=left.cpu_ms + right.cpu_ms,
            rss_bytes=left.rss_bytes + right.rss_bytes,
            switches=left.switches + right.switches,
            io_bytes=Cost._volume(left.io_bytes, right.io_bytes, int.__add__),
        )

    def facts(self) -> EventDict:
        held: EventDict = {"cpu_ms": self.cpu_ms, "rss_bytes": self.rss_bytes, "switches": self.switches}
        return held if self.io_bytes is None else held | {"io_bytes": self.io_bytes}

    def measures(self) -> dict[str, float]:
        # instrument projection keyed by the metrics-owned `rasm.cost.<measure>` rows — the bench correspondence
        # pattern: the producer spells the mapping, the one `INSTRUMENTS` table owns the rows. A host binding no
        # I/O counter omits its measure entirely, so `rasm.cost.byte_volume` carries the crossings that measured
        # transfer and nothing else, where a zeroed reading would drag every percentile of that distribution down.
        held = {
            "rasm.cost.cpu_time": self.cpu_ms,
            "rasm.cost.memory_delta": float(self.rss_bytes),
            "rasm.cost.ctx_switches": float(self.switches),
        }
        return held if self.io_bytes is None else held | {"rasm.cost.byte_volume": float(self.io_bytes)}


class DrainReceipt[T](Struct, frozen=True):
    accepted: int
    completed: int
    cancelled: int
    rejected: int
    values: Block[T] = Block.empty()
    cache: Map[ContentKey, T] = Map.empty()
    faults: Block[BoundaryFault] = Block.empty()
    hit: int = 0
    # absent when either end of the drain-window bracket failed to read: a window with no
    # measurable spend reports none rather than a zero a board reads as a free drain.
    cost: Option[Cost] = Nothing

    @staticmethod
    def of[U](
        accepted: int,
        hit: int,
        resolved: Block[tuple[Option[ContentKey], RuntimeRail[U]]],
        replayed: Block[tuple[ContentKey, U]],
        cache: Map[ContentKey, U],
        cost: Option[Cost] = Nothing,
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
            cost=cost,
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
                return "info", "drained", {"owner": owner, **_rss(), **drain.cost.map(Cost.facts).default_value({}), **{column: getattr(drain, column) for column in DRAIN_COLUMNS}}
            case _ as unreachable:
                assert_never(unreachable)


@runtime_checkable
class ReceiptContributor(Protocol):
    def contribute(self) -> Iterable[Receipt]: ...


class Redaction(Struct, frozen=True):
    classified: Map[str, Scrub]
    salt: bytes = b"rasm"

    def apply(self, facts: EventDict) -> EventDict:
        return {key: redacted for key, value in facts.items() if key != REDACTION_KEY for redacted in self._classify(key, value)}

    def _classify(self, key: str, value: object) -> tuple[object, ...]:
        # classification is by KEY NAME at every depth, never at the top level alone: a collector document nests its
        # receipt maps and replay rings, and a producer's own fact can carry a mapping or a sequence of them, so a
        # classified field seated one level down would reach the archive and the wire unscrubbed while its top-level
        # twin scrubbed. The key's own row is read FIRST, so a classified value reduces WHOLE under its class —
        # dropped, masked, or hashed alike — and only an UNCLASSIFIED container is descended into; descending first
        # would hash a container's already-scrubbed projection and mask members the class had settled entire.
        # Scalars terminate immediately, and every TEXT and BUFFER shape is excluded from the sequence arm: `str`,
        # `bytes`, and `bytearray` each register as Sequences, so iterating one yields characters or integers rather
        # than members and a caller's buffer would reach the sink expanded past recognition.
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


# public keep-all policy a sibling with no classified field threads into @receipted(OPEN); depends on the Redaction model, so it anchors here.
OPEN: Final[Redaction] = Redaction(classified=Map.empty())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _io_volume(process: psutil.Process) -> int | None:
    # gated counter read seated OUTSIDE the bracket's `oneshot`: that block caches the kinfo/taskinfo pair behind cpu
    # times, RSS, and switches alone, so folding this member in buys batching that is not there. An unbound member and
    # a denied read alike MEASURED NOTHING on this axis and answer so.
    if not hasattr(process, "io_counters"):
        return None
    with suppress(*PROCESS_FAULTS):
        counters = process.io_counters()
        return counters.read_bytes + counters.write_bytes
    return None


def _rss() -> EventDict:
    with suppress(*PROCESS_FAULTS):
        return {"rss": _PROCESS.memory_info().rss}
    return {}


# one default-sink resolution both emit arms share: absent folds to the scope-resolved get_logger fetched per emit, never
# cached — the default scope keeps the bare global logger (today's line shape), a composition scope binds its own key.
def _sink(sink: BoundLogger | None, scope: ScopeKey) -> BoundLogger:
    fetched = structlog.get_logger if scope == DEFAULT_SCOPE else partial(structlog.get_logger, composition=scope)
    return Option.of_optional(sink).default_with(fetched)


def _stream(source: Streamable) -> Iterable[Receipt]:
    match source:
        case Receipt():
            return (source,)
        case ReceiptContributor():
            return source.contribute()
        case _:
            return source


# one render fold both sinks share — emit/emit_async differ only by which selector half they drive. Redaction is NOT applied here:
# logging's chain processor runs after the contextvars/trace injectors; this fold only binds it under `REDACTION_KEY`.
def _render(source: Streamable, redaction: Redaction) -> Iterator[tuple[LevelBinding, str, EventDict]]:
    for receipt in _stream(source):
        level, name, fields = receipt.project()
        yield LEVEL_METHOD[level], name, fields | {REDACTION_KEY: redaction}


# --- [SERVICES] -------------------------------------------------------------------------


class Signals:
    @staticmethod
    def emit(source: Streamable, redaction: Redaction, sink: BoundLogger | None = None, *, scope: ScopeKey = DEFAULT_SCOPE) -> None:
        log = _sink(sink, scope)
        for (_, sync, _), name, fields in _render(source, redaction):
            sync(log)(name, **fields)

    @staticmethod
    async def emit_async(source: Streamable, redaction: Redaction, sink: BoundLogger | None = None, *, scope: ScopeKey = DEFAULT_SCOPE) -> None:
        log = _sink(sink, scope)
        for (_, _, amirror), name, fields in _render(source, redaction):
            await amirror(log)(name, **fields)

    @staticmethod
    def continue_inbound(carrier: Mapping[str, str]) -> Context:
        # `context=` stays UNPASSED by law, and the default is not a convenience: `propagate.extract` forwards `None`
        # to the composite, whose trace-context leg seeds a FRESH ROOT and returns it untouched when the carrier
        # holds no `traceparent` — so a crossing that carried no parent attaches a root and runs unparented, which
        # is exactly what the boundary rules for a pickled worker with no install of its own. Passing
        # `context=get_current()` here reads as defensive and silently grafts whatever span the EXTRACTING thread
        # inherited — a thread the instrumentor train may have handed the caller's context — onto the crossing as
        # its parent, minting a hop that never happened. A malformed or version-refused header takes the same root
        # path, so a forged carrier can never re-parent this leg either.
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
    return scoped(trace.get_tracer, scope)


def _marked(fault: BoundaryFault) -> BoundaryFault:
    # faults-owned conversion marks a raise caught under this span; a pre-railed fault arrives already converted,
    # so its ERROR status and fault-fact event land here — each path marks the span exactly once, no double-record.
    span = trace.get_current_span()
    if span.is_recording():
        span.add_event("rasm.fault", attributes={"rasm.fault.tag": fault.tag, "rasm.fault.subject": str(fault.facts()["subject"])})
        span.set_status(Status(StatusCode.ERROR, fault.tag))
    return fault


def _flat[T](value: "T | RuntimeRail[T]") -> "RuntimeRail[T]":
    # a rail-returning dispatch (a lane offload, a fenced fold) already carries its rail; a bare value lifts Ok.
    # a carried Error crossed no fence under this span, so the lift is where its ERROR status lands.
    return value.map_error(_marked) if isinstance(value, Result) else Ok(value)


def _harvested[T](value: T, redaction: Redaction, scope: ScopeKey) -> T:
    # fenced emit step: a ReceiptContributor streams its receipts, a plain value passes through untouched.
    if isinstance(value, ReceiptContributor):
        Signals.emit(value, redaction, scope=scope)
    return value


async def _harvested_async[T](value: T, redaction: Redaction, scope: ScopeKey) -> T:
    # async mirror of the fenced emit step: the woven arm awaits `Signals.emit_async` so a contributor harvest
    # never runs a sync sink on the event loop.
    if isinstance(value, ReceiptContributor):
        await Signals.emit_async(value, redaction, scope=scope)
    return value


def _closed[T](span: Span, value: T) -> T:
    span.set_status(Status(StatusCode.OK))
    return value


async def _lifted[T](rail: RuntimeRail[T]) -> RuntimeRail[T]:
    # a settled rail wearing the awaitable shape a declared-async caller awaits, so one refusal site serves both
    # modalities and neither arm grows a refusal spelling the other lacks.
    return rail


def measured[T](
    scope: str, subject: str, redaction: Redaction, dispatch: Callable[[], T] | Callable[[], Awaitable[T]],
    facts: Mapping[str, str | int | float | bool] = Map.empty(), *, composition: ScopeKey = DEFAULT_SCOPE,
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # Exemption: the span lifecycle and the modality probe are the platform-forced seam every evidence producer
    # composes instead of re-authoring; emission runs inside its own fence, so an emit-time raise folds onto the rail.
    # Two axes this page mints stay SEPARATE across the weave: `scope` names the INSTRUMENTATION scope of the emitting
    # library, spent on `_tracer` and on the fence labels alone, while `composition` carries the custody key the harvest
    # hands `Signals.emit`, where `_sink` binds a `composition`-bound logger. Spending one value on both binds every
    # producer's harvest under a tracer name no `ScopeKey` vocabulary owns, and leaves a producer holding a real
    # composition key no parameter to thread it through.
    # Scope grammar refuses BEFORE any span mints: an off-grammar scope reaching `_tracer` opens a span whose
    # instrumentation scope no backend joins against its versioned siblings, and no later gate retracts an exported
    # span. The modality probe hoists so ONE refusal serves both arms — the async caller awaits `_lifted`, the sync
    # caller reads the rail — where a per-arm refusal lets one modality drift off the other.
    awaiting = iscoroutinefunction(dispatch) or iscoroutinefunction(getattr(dispatch, "__call__", None))
    if SCOPE_ID.fullmatch(scope) is None:
        refusal: RuntimeRail[T] = Error(BoundaryFault(config=("receipts.scope", scope)))
        return _lifted(refusal) if awaiting else refusal

    def opened() -> Span:
        span = _tracer(scope).start_span(subject)
        if span.is_recording():
            span.set_attributes({**facts, "scope": scope, "subject": subject})
        return span

    async def settled(span: Span, pending: Callable[[], Awaitable[T]]) -> RuntimeRail[T]:
        # one async settle for both modalities: the declared-async arm hands in the span it just opened, the sync arm
        # hands the SAME still-open span its dispatch ran under — a sync-minted awaitable finishes inside the original
        # span, never a sibling pair whose first member closed un-statused at the mint.
        with trace.use_span(span, end_on_exit=True):
            rail = (await async_boundary(f"{scope}.{subject}", pending)).map_error(_marked).bind(_flat)
            match rail:  # one total carrier match — `expression` ships no async builder, and the harvest awaits emit_async
                case Result(tag="ok", ok=value):
                    return (
                        await async_boundary(f"{scope}.emit", lambda: _harvested_async(value, redaction, composition))
                    ).map_error(_marked).map(lambda live: _closed(span, live))
                case _:
                    return rail

    # declared modality routes first: a coroutine function or an async-`__call__` instance never touches the sync arm.
    if awaiting:
        return settled(opened(), dispatch)
    span = opened()
    with trace.use_span(span, end_on_exit=False):
        railed = boundary(f"{scope}.{subject}", dispatch).map_error(_marked).bind(_flat)
        match railed:
            case Result(tag="ok", ok=pending) if isawaitable(pending):
                # a sync-declared callable can still RETURN an awaitable (a lambda minting a coroutine); the settle
                # continues under the SAME open span, which ends when the awaited half resolves — Ok(coroutine) never escapes.
                return settled(span, lambda: pending)
            case _:
                outcome = railed.bind(
                    lambda value: boundary(f"{scope}.emit", lambda: _harvested(value, redaction, composition)).map_error(_marked)
                ).map(lambda value: _closed(span, value))
                span.end()
                return outcome


def receipted[**P, R: ReceiptContributor](
    redaction: Redaction, *, scope: ScopeKey = DEFAULT_SCOPE
) -> Callable[[Contributing[P, R]], Contributing[P, R]]:
    def wrap(operation: Contributing[P, R]) -> Contributing[P, R]:
        if iscoroutinefunction(operation):

            @wraps(operation)
            async def harvested_async(*args: P.args, **kwargs: P.kwargs) -> R:
                contributor = await operation(*args, **kwargs)
                await Signals.emit_async(contributor, redaction, scope=scope)
                return contributor

            return harvested_async

        @wraps(operation)
        def harvested(*args: P.args, **kwargs: P.kwargs) -> R:
            contributor = operation(*args, **kwargs)
            Signals.emit(contributor, redaction, scope=scope)
            return contributor

        return harvested

    return wrap
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
