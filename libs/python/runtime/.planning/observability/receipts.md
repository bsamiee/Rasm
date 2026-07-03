# [PY_RUNTIME_RECEIPTS]

This owner produces local evidence — typed receipts and structured log facts — and contributes nothing to provider install, product telemetry export, or health.

`Receipt` is the one tagged-union evidence family over `fact`/`rejected`/`drained` cases, and it owns its own projection. Each case folds to a `(LogLevel, EventDict)` pair through one total `project` `match`, so `Signals.emit` is a renderer-agnostic fold over the union rather than three hand-built dict arms.

`ReceiptContributor` is the `Protocol` port every sibling's typed receipt streams through — data `QueryReceipt`, compute `GraduationReceipt`, geometry/artifacts `ArtifactReceipt` — its `contribute` yielding a receipt sequence rather than one forced fact. `@receipted` is the cross-cutting aspect that wraps a measured operation, harvests its contributor stream, and emits on exit, so receipt production is a decorator rail rather than inline `emit` calls scattered through the siblings. The aspect routes its sync arm through `Signals.emit` and its async arm through `Signals.emit_async`, the latter awaiting the loop-friendly `FilteringBoundLogger` `a*` mirror so a high-volume async serve path offloads render-and-sink to a worker thread rather than blocking the event loop.

`Redaction` is a `Classification`-keyed field-policy table over `drop`/`mask`/`hash`, and it is a chain-resident processor, never a pre-chain fold. The `hash` class threads stdlib `hashlib.blake2b` so a secret yields a stable correlation token rather than an opaque `***`. The placement is load-bearing: a per-emit `Redaction` rides bound onto the event under one reserved `_REDACTION` key, and the `redact` processor sits AFTER `merge_contextvars`/`trace_context` so it scrubs the ambient `Correlation`/`RuntimeContext` fields and the injected `trace_id`/`span_id` alongside the receipt's own facts — a pre-chain `_render`-side `apply` over only the projected `EventDict` lets a classified field arriving through `bind_contextvars` reach the line unredacted, the deleted form. The `structlog` processor chain injects OTel trace context and structured fault tracebacks, and `psutil` supplies the process RSS fact.

`Signals` carries both directions of the W3C trace-context seam. The outbound `trace_context` processor injects the active span's `trace_id`/`span_id`/sampled flag into every event. The inbound seam is one polymorphic pair: `continue_inbound` is the pure extract that decodes the C#-minted parent off the gRPC carrier into a `Context` value, and `attach` is the token-paired activation scope the consumer opens around exactly its measured body. The servicer leg threads the extracted `Context` through its admission rail before opening the span scope, a placement a single fused extract-and-activate scope cannot serve. OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`, since `structlog` mints no native OTLP log export.

## [01]-[INDEX]

- [01]-[RECEIPT]: the self-projecting receipt union with its shape-polymorphic `of` factory, the streaming contributor port, the `@receipted` aspect feeding the `emit`/`emit_async` sink pair over the `(sync, async)` `LEVEL_METHOD` rows, the classification-keyed redaction table applied by the chain-resident `redact` processor, the structlog processor chain, the inbound trace-context `continue_inbound` extract paired with the `attach` activation scope.

## [02]-[RECEIPT]

- Owner: `Receipt` — the one evidence union with slot/kind metadata owning its own `(LogLevel, EventDict)` projection and a shape-polymorphic `of` factory that mints every case by discriminating its input evidence; `LogLevel` the bounded structlog-method vocabulary the `LEVEL_METHOD` table keys, each row a `(sync, async)` `LevelBinding` selector pair so one table binds both `log.info` and the loop-friendly `log.ainfo`; `ReceiptContributor` the Protocol port the sibling typed receipts implement as a receipt stream; `Classification`/`Redaction` the field-classification policy table the chain-resident `redact` processor applies; `Signals` the static surface owning the `structlog` processor chain (`trace_context` and `redact` its two custom processors), the `psutil` process telemetry facts, the `emit`/`emit_async` sink pair feeding the `@receipted` aspect, the pure `continue_inbound` parent-context extract, and the `attach` activation scope, the trace context and the redaction both riding the processor chain rather than a pre-chain fold or a cached tracer.
- Cases: the three lifecycle phases share one `fact` case carrying `(Phase, owner, subject, facts)` — `admitted`/`planned`/`emitted` are a `Phase` literal value the case routes, not three identical-payload sibling cases; `rejected` carries `(owner, BoundaryFault)` and `drained` carries `(owner, DrainReceipt[object])` because their payloads differ; the `rejected` subject is never a pre-extracted slot beside the fault — it lives inside the fault's own `facts()` projection, so the case carries the owner plus the fault and nothing duplicated; correlation flows through `merge_contextvars`, never a per-case field. Each case's log disposition is data, not a dispatch arm: `Receipt.project` is the one total `match` mapping every case to its `(LogLevel, EventDict)` pair — the `fact` case projects the `Phase`-keyed level off the `PHASE_LEVEL` table so `admitted`/`planned` log at `debug` and `emitted` at `info` without a phase branch, `rejected` projects `warning` plus the spread of the `reliability/faults#FAULT`-owned `BoundaryFault.facts()` total structured-egress map (`tag`/`subject`/`detail`/`code`/`budget`/`members` per the leaf case, the aggregate naming its member tags) rather than a private fault walk, `drained` projects `info` plus the `DRAIN_COLUMNS` outcome map and the RSS fact, so a new case is one match arm on `project` and a new phase is one `PHASE_LEVEL` row, never an `emit`-side dispatch edit.
- Entry: `Receipt.of(owner, evidence)` is the one shape-polymorphic factory over the `Evidence` input alias, `match`ing the argument so a `(Phase, subject, facts)` triple mints `fact`, a `BoundaryFault` mints `rejected` carrying the owner plus the whole fault (the subject deferred to `facts()` at `project` time rather than pre-extracted into a redundant slot), and a `DrainReceipt[object]` mints `drained`. A new evidence shape is one `of` match arm, never a hand-built `Receipt(rejected=...)`/`Receipt(drained=...)` at a call site or a parallel `of_rejected`/`of_drained` family. `Receipt.project` is the total case-to-`(LogLevel, EventDict)` fold the union owns. `ReceiptContributor.contribute` is the one method a sibling implements to yield its typed receipt stream into the pipeline.
- Entry: `Signals.emit`/`Signals.emit_async` are the renderer-agnostic sink pair polymorphic over input and output. On the input axis a single `Receipt`, an `Iterable[Receipt]`, or a `ReceiptContributor` normalize through one `_stream` `match` to one `Iterable[Receipt]`. On the output axis the optional `sink` defaults to the global `structlog.get_logger()` but accepts any `FilteringBoundLogger`, so a `structlog.testing.capture_logs`/`LogCapture` test or a `wrap_logger`-local consumer drives the same fold without a second emit surface. The `project` -> `event.pop("event")` -> bind the per-emit `Redaction` under `_REDACTION` -> `LEVEL_METHOD[level]` lookup is one `_render` generator both methods consume, yielding a `(LevelBinding, name, fields)` triple per receipt so the projection and level dispatch live once rather than once per arm; redaction is NOT folded here — `_render` carries the `Redaction` into the line and the chain-resident `redact` processor applies it after the contextvars/trace injectors so a `bind_contextvars`-sourced classified field cannot bypass it. The two methods reduce to the one shape Python forces — `emit` drives the `LevelBinding[0]` sync selector (`log.info`) and `emit_async` awaits the `LevelBinding[1]` mirror (`log.ainfo`) over the same `_render` stream, never a stringly `getattr(log, <level-name>)`/`getattr(log, "a" + <level-name>)` over an open namespace and never a re-walked `_stream`+`project` body duplicated across the pair.
- Entry: `Signals.continue_inbound(carrier)` is the pure `propagate.extract(carrier)` over the `Mapping[str, str]` carrier read directly (the `DefaultGetter` reads the mapping in place, never a `dict(carrier)` re-copy on the inbound hot path), resolving the parent through `propagate.get_global_textmap` (the composite `observability/telemetry#TELEMETRY` installs) and returning the decoded `Context` as a value the servicer threads through its admission rail before any span scope. `Signals.attach(parent)` is the separate token-paired `@contextmanager` that `context.attach`es the `Context`, yields it, and `context.detach`es in `finally`. The split is load-bearing: `transport/serve#SERVE` `ServerHost.inbound` extracts outside the wire-fault fence and `dispatch` opens `attach(parent)` around the decode/handler/encode body, a placement a single fused extract-and-activate scope cannot serve.
- Entry: `@receipted` is the AOP aspect that wraps a `ReceiptContributor`-returning operation and emits its harvested stream on exit through the same `_stream`-normalized fold, routing a sync op to `emit` and a coroutine to `emit_async` off `iscoroutinefunction`. A measured kernel declares `@receipted(redaction)` and threads no emit call through its body. The aspect is parameterized over the concrete contributor return type through the `R: ReceiptContributor`-bound `Contributing[P, R]` alias, so a `@receipted`-decorated `_emit(x: T) -> T` over a concrete `T <: ReceiptContributor` (a `ModelAssetManifest`, an `InferenceReceipt`, a `GraduationReceipt`) statically returns `Callable[P, T]` rather than collapsing to the bare `ReceiptContributor` Protocol — so the consumer's `boundary(..., lambda: _emit(...))` `Ok` arm reads the concrete `receipt.span_facts`/`manifest.span_facts` member without a static type error, the bound preserving the subtype the decorator harvests rather than erasing it to the port that declares only `contribute`.
- Auto: the `structlog` processor chain binds, in order, `merge_contextvars` (carrying the `Correlation`/`RuntimeContext` bound context) first, the custom `trace_context` reading `opentelemetry.trace.get_current_span()` to inject `trace_id`/`span_id`/`trace_flags`, the `redact` processor applying the per-emit `Redaction` bound under `_REDACTION` to the fully-assembled line (so it scrubs the `merge_contextvars` ambient fields and the injected trace ids, not just the receipt's own facts) and stripping the `_REDACTION` carrier before render, `CallsiteParameterAdder` resolving the emitting module/function once after the level admits the event, `dict_tracebacks` transforming a bound `exc_info`/`BoundaryFault` cause into a JSON-safe structured stack rather than a flattened `str(exc)`, a `TimeStamper`, an `EventRenamer` mapping `event` to the OTLP `body` key, and a renderer-last `JSONRenderer`. The `redact` placement after the contextvars/trace injectors is load-bearing: a pre-chain `_render`-side `apply` over only the projected facts lets a `bind_contextvars`-sourced classified field reach the line unredacted. The renderer `serializer` is the `msgspec` `json.Encoder(enc_hook=repr, order="deterministic").encode` paired with a `BytesLoggerFactory` sink, so domain `Struct` facts and native ints/floats reach the line through the one fast encoder that owns the wire without a `str()` coerce. The `enc_hook=repr` degrades any value outside the native JSON set to its repr so a bound domain object renders rather than killing the emit on the hot path, and `order="deterministic"` fixes mapping key order so the one encoder doubles as the canonical `hash`-class input. `make_filtering_bound_logger` compiles sub-threshold levels to no-ops on both the sync and the `a*` async-mirror methods.
- Auto: `emit_async` awaits the `FilteringBoundLogger` `a*` mirror so the async serve and `drained` paths offload the JSON render and byte sink to a worker thread rather than blocking the event loop under high log volume, while `emit` keeps the inline sync path for non-loop callers — one `(sync, async)` `LEVEL_METHOD` row binding both off the closed `LogLevel`. Span creation belongs to the measured operation, not to receipt emission, so emit writes under whatever span is active.
- Auto: OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`/`OTLPLogExporter` (the `structlog`-to-OTel bridge, since `structlog` mints no native OTLP log export); this owner wires no private `LogRecordProcessor`/`OTLPLogExporter`. `continue_inbound` resolves the parent through the installed composite propagator, so before the install the extract reads the default no-op propagator and returns an empty `Context`, `attach` activates a no-op scope, and the C# parent is dropped — the mechanical reason the extract sequences after the install.
- Auto: `Redaction.apply` is run by the chain-resident `redact` processor, never folded into `_render`: each fact classifies through the `Classification`-keyed `Map[str, Classification]` table, never a parallel allow/deny set — a `drop` field never reaches a line, a `mask` field renders the fixed sentinel, and a `hash` field renders a truncated `blake2b` correlation token over the `order="deterministic"` `_ENCODE` of the value keyed by the redaction's `salt`. The deterministic key order makes a structured secret hash canonically regardless of insertion order, so two lines carrying the same secret correlate by token without leaking the value. `_render` binds the per-emit `Redaction` onto the event under the reserved `_REDACTION` key and `apply` skips that key while scrubbing; a line carrying no bound `Redaction` (a foreign stdlib-bridge record) folds through the keep-all `OPEN` policy via `Option.of_optional(event.get(_REDACTION)).default_value(OPEN)`, so redaction stays the per-receipt-stream contract `emit` binds rather than a global scrub, and `OPEN` anchors after the `Redaction` model rather than top-level `[CONSTANTS]` because it depends on it. `OPEN` is the public keep-all every sibling whose receipt facts carry no classified field threads into `@receipted(OPEN)`, so no producer re-mints a `Redaction(classified=Map.empty())` per file.
- Auto: the drained projection reads each imported `execution/lanes#LANE`-owned `DRAIN_COLUMNS` directly off the typed `DrainReceipt` through one `getattr(drain, column)` per column, so the line carries exactly the five outcome counts of the shared `DrainOutcome` taxonomy while the typed `faults`/`values`/`cache` `Block`/`Map` containers stay the structural carry, never materialized on the emit path. A full `msgspec.structs.asdict` is the deleted form here, matching the sibling `observability/metrics#METRIC` `_drain_fold`, since `asdict` would allocate the receipt's `values`/`cache`/`faults` containers on every emit only to drop them where the five-scalar `getattr` read stays a typed field access. The same `accepted`/`completed`/`cancelled`/`rejected`/`hit` columns the `observability/metrics#METRIC` `lane.drained` `ObservableCounter` keys by its `outcome` attribute read through the identical per-column `getattr`-over-`DRAIN_COLUMNS`, cache-`hit` a first-class outcome dimension on both signals: the receipt is the per-drain point fact, the counter the streamed gauge over the latest fold, both reading the one taxonomy the `DrainReceipt` owner declares, so the line and the `outcome`-keyed counter cannot disagree on the column set and a new `DrainOutcome` member reaches both by one field add on the shared owner.
- Auto: `_rss()` reads `_PROCESS.memory_info().rss` off the one own-process handle minted once at module load under `contextlib.suppress(*PROCESS_FAULTS)`, so the drained projection stays a pure dict spread that attaches the RSS slot on success and omits it on a `NoSuchProcess`/`ZombieProcess`/`AccessDenied` race rather than raising and killing the measured operation's egress. This is the same RSS fact the metrics `process.memory.rss` `ObservableGauge` streams (one `psutil` source, the receipt a point fact, the gauge the stream), guarded exactly as the metrics owner's `ProcessReading.sample` guards its read; the metrics sibling carries its own handle, so receipts never owns the `Process` the metrics owner streams. The default sink resolves through one `_sink` fold both `emit` arms share — `Option.of_optional(sink).default_with(structlog.get_logger)` fetches the global logger per emit (never cached as a module constant) when `sink` is absent and returns the passed `sink` for the `testing.capture_logs`/`wrap_logger` seams, so the override contract lives once rather than a ternary duplicated across the sync/async pair.
- Growth: a new lifecycle phase is one `Phase` literal plus one `PHASE_LEVEL` row absorbed by the existing `fact` case; a distinct-payload evidence kind is one `Receipt` case plus its `project` match arm plus its `of` discriminant arm on the `Evidence` alias (the factory minting it by input shape); a new structured slot on a rejected fault reaches the log line for free through the `BoundaryFault.facts()` spread, never a `project` edit here, since the rejected case folds the owner's projection rather than a private node; a new classified field is one `Redaction` table row keyed by its `Classification`; a new classification transform is one `Classification` member plus one `_reduce` arm (the `assert_never` total-match forcing the addition); a new log level is one `LogLevel` literal plus one `(sync, async)` `LEVEL_METHOD` selector row reaching both `emit` and `emit_async` at once; a new processor is one entry in the `structlog` chain; a new measured op participates by returning a `ReceiptContributor` under `@receipted`, the aspect routing it to the loop-friendly `emit_async` when the op is a coroutine; a new sink target (a test capture buffer, a local pipeline) is the `sink` argument on the existing `emit`/`emit_async` pair, never a second emit method; zero new emit surface.
- Boundary: the deleted forms are a per-package parallel receipt rail; a stdlib `logging` call outside the structlog bridge; a `str()`-coerced fact map where the `msgspec` renderer carries native types; a bare `Encoder()` whose serializer raises on an unexpected bound value where `enc_hook=repr` degrades it; three hand-built `emit` dispatch arms where the union's own `project` folds them; a stringly `getattr(log, <level-name>)`/`getattr(log, "a" + <level-name>)` where the `(sync, async)` `LEVEL_METHOD` rows dispatch the bound method over the closed `LogLevel`; a synchronous `emit` driven inline on an async serve or `drained` hot path where `emit_async` awaits the loop-friendly `a*` mirror; a parallel async-mirror selector table beside the sync one where one `LevelBinding` row carries both; a duplicated `project`/`event.pop`/level-lookup body in `emit` and `emit_async` where the one `_render` generator yields the `(LevelBinding, name, fields)` triple both arms consume and only the sync-call-versus-`await` half differs; a pre-chain `_render`-side `Redaction.apply` over only the projected `EventDict` where the chain-resident `redact` processor (after `merge_contextvars`/`trace_context`) scrubs the whole assembled line so a `bind_contextvars`-sourced classified secret cannot leak past redaction; a `dict(carrier)` re-copy in `continue_inbound` where the `DefaultGetter` reads the `Mapping` in place; a parallel `of_rejected`/`of_drained` factory family where the one shape-polymorphic `of` discriminates by input; a non-total `of`/`project`/`_reduce` `match` lacking the `assert_never` tail; a hand-built `Receipt(rejected=...)`/`Receipt(drained=...)` at a sibling call site where `of` mints it; a `structs.asdict(drain)`-then-index drained projection allocating the receipt's `values`/`cache`/`faults` containers on every emit where the per-column `getattr` over `DRAIN_COLUMNS` reads five scalars off the typed struct; a hard-wired global `get_logger()` sink with no override where the `sink` parameter feeds a `testing.capture_logs`/`wrap_logger` consumer; a `sink if sink is not None else get_logger()` ternary duplicated across the `emit`/`emit_async` pair where the one `_sink` `Option.of_optional(...).default_with(get_logger)` fold owns the default-resolution contract; a private fault-node walk re-implementing the `reliability/faults#FAULT`-owned `BoundaryFault.facts()` aggregate fold (a lossy `{"tag": ...}`-only node dropping the `subject`/`detail`/`code`/`budget` slots the canonical projection carries) where the `rejected` projection spreads `fault.facts()`; a redundant `rejected` subject slot pre-extracted beside the fault where `facts()["subject"]` carries it; a bare unparameterized `DrainReceipt` carry where the `DrainReceipt[object]` pin makes the boundary erasure explicit; a flat `***`-only redaction where the classification table discriminates `drop`/`mask`/`hash`; a stringly out-of-vocabulary `"keep"` default outside the closed `Classification` set where the `try_find().map(...).default_value((value,))` Option fold keeps the un-classified field and `_reduce` stays total over the real members under `assert_never`; a fused extract-and-activate `continue_inbound` scope a servicer cannot place where the pure extract feeds the rail and the separate `attach` scope wraps the body; a second tracer minted for the inbound extract; and a fresh-root span where the C# parent is on the inbound carrier.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Iterator, Mapping
from contextlib import contextmanager, suppress
from functools import wraps
from hashlib import blake2b
from inspect import iscoroutinefunction
from typing import Final, Literal, Protocol, assert_never, runtime_checkable

import psutil
import structlog
from expression import Option, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.json import Encoder
from opentelemetry import context, propagate, trace
from opentelemetry.context import Context

from rasm.runtime.faults import BoundaryFault
from rasm.runtime.lanes import DRAIN_COLUMNS, DrainReceipt

# --- [TYPES] ----------------------------------------------------------------------------

type Phase = Literal["admitted", "planned", "emitted"]
type LogLevel = Literal["debug", "info", "warning", "error"]
type EventDict = dict[str, object]
type Classification = Literal["drop", "mask", "hash"]
type Evidence = tuple[Phase, str, dict[str, object]] | BoundaryFault | DrainReceipt[object]
type Streamable = Receipt | Iterable[Receipt] | ReceiptContributor
type Contributing[**P, R: ReceiptContributor] = Callable[P, R] | Callable[P, Awaitable[R]]
type BoundLogger = structlog.typing.FilteringBoundLogger
type LevelSelector = Callable[[BoundLogger], Callable[..., object]]
type LevelBinding = tuple[LevelSelector, LevelSelector]

# --- [CONSTANTS] ------------------------------------------------------------------------

PHASE_LEVEL: Final[Map[Phase, LogLevel]] = Map.of_seq([("admitted", "debug"), ("planned", "debug"), ("emitted", "info")])

REDACTED: Final[str] = "***"

# the reserved event key the per-emit Redaction rides on so the chain-resident `redact`
# processor scrubs the whole line (receipt facts + ambient contextvars + trace context),
# not just the receipt's own projected facts; stripped by `apply` before the line renders.
_REDACTION: Final[str] = "_redaction"

# the bounded LogLevel vocabulary owns which FilteringBoundLogger method each case emits
# through — one row per level carrying the (sync, async-mirror) bound-method selector pair,
# so emit binds log.info and emit_async binds the loop-friendly await log.ainfo off the one
# table over the closed literal rather than a stringly getattr(log, level)/getattr(log, "a"+level).
LEVEL_METHOD: Final[Map[LogLevel, LevelBinding]] = Map.of_seq([
    ("debug", (lambda log: log.debug, lambda log: log.adebug)),
    ("info", (lambda log: log.info, lambda log: log.ainfo)),
    ("warning", (lambda log: log.warning, lambda log: log.awarning)),
    ("error", (lambda log: log.error, lambda log: log.aerror)),
])

# the one renderer/redaction encoder; enc_hook=repr degrades any value outside the native
# JSON set to its repr so a bound domain object never raises on the hot logging path, and
# order="deterministic" makes the hash class canonical so the same structured secret yields
# the same blake2b correlation token across log lines regardless of mapping insertion order.
_ENCODE: Final[Callable[[object], bytes]] = Encoder(enc_hook=repr, order="deterministic").encode

# own-process handle minted once; the drained point-fact reads RSS off it rather than
# re-minting a Process() per projection. The metrics sibling carries its own handle.
_PROCESS: Final[psutil.Process] = psutil.Process()

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (psutil.NoSuchProcess, psutil.ZombieProcess, psutil.AccessDenied)

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Receipt:
    tag: Literal["fact", "rejected", "drained"] = tag()
    fact: tuple[Phase, str, str, dict[str, object]] = case()
    rejected: tuple[str, BoundaryFault] = case()
    drained: tuple[str, DrainReceipt[object]] = case()

    @staticmethod
    def of(owner: str, evidence: Evidence) -> "Receipt":
        match evidence:
            case BoundaryFault() as fault:
                return Receipt(rejected=(owner, fault))
            case DrainReceipt() as drain:
                return Receipt(drained=(owner, drain))
            case (phase, subject, facts):
                return Receipt(fact=(phase, owner, subject, facts))
            case _ as unreachable:
                assert_never(unreachable)

    def project(self) -> tuple[LogLevel, EventDict]:
        match self:
            case Receipt(tag="fact", fact=(phase, owner, subject, facts)):
                return PHASE_LEVEL[phase], {"event": phase, "owner": owner, "subject": subject, **facts}
            case Receipt(tag="rejected", rejected=(owner, fault)):
                return "warning", {"event": "rejected", "owner": owner, **fault.facts()}
            case Receipt(tag="drained", drained=(owner, drain)):
                return "info", {"event": "drained", "owner": owner, **_rss(), **{column: getattr(drain, column) for column in DRAIN_COLUMNS}}
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


# the public keep-all policy: the chain-resident `redact` folds a line carrying no bound `Redaction`
# through it, and every sibling whose receipt facts carry no classified field threads `@receipted(OPEN)`
# rather than re-minting `Redaction(classified=Map.empty())` per file; depends on the `Redaction` model
# so it anchors here, never top-level [CONSTANTS].
OPEN: Final[Redaction] = Redaction(classified=Map.empty())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _rss() -> EventDict:
    with suppress(*PROCESS_FAULTS):
        return {"rss": _PROCESS.memory_info().rss}
    return {}


# the one default-sink resolution both emit arms share: an explicit `sink` overrides for the
# testing.capture_logs/wrap_logger seam, an absent one folds to the global get_logger fetched
# per emit (never cached as a module constant) through the Option default_with thunk.
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


# the one render fold both sinks share: project -> split the level into the (sync, async)
# selector pair and bind the per-emit Redaction onto the event under `_REDACTION`, so
# emit/emit_async differ only by which half they drive. Redaction is NOT applied here — the
# chain-resident `redact` processor runs it after the contextvars/trace injectors so it
# scrubs the whole line, not just the receipt-projected facts; this fold only carries it in.
def _render(source: Streamable, redaction: Redaction) -> Iterator[tuple[LevelBinding, str, EventDict]]:
    for receipt in _stream(source):
        level, event = receipt.project()
        yield LEVEL_METHOD[level], event.pop("event"), event | {_REDACTION: redaction}


# chain-resident redaction: reads the per-emit Redaction bound under `_REDACTION` and scrubs
# the fully-assembled line — receipt facts, the merge_contextvars Correlation/RuntimeContext,
# and the trace_context ids — so a classified field arriving through bind_contextvars cannot
# bypass redaction the way a pre-chain apply does. A line carrying no bound Redaction (a foreign
# stdlib-bridge record, a direct log call) folds through the keep-all `OPEN` policy untouched,
# since redaction is the per-receipt-stream contract emit binds, never a global scrub.
def redact(_: object, __: str, event: EventDict) -> EventDict:
    return Option.of_optional(event.get(_REDACTION)).default_value(OPEN).apply(event)


def trace_context(_: object, __: str, event: EventDict) -> EventDict:
    ctx = trace.get_current_span().get_span_context()
    if ctx.is_valid:
        event.update(trace_id=trace.format_trace_id(ctx.trace_id), span_id=trace.format_span_id(ctx.span_id), trace_flags=int(ctx.trace_flags))
    return event


# --- [SERVICES] -------------------------------------------------------------------------


class Signals:
    @staticmethod
    def configure() -> None:
        structlog.configure(
            processors=[
                structlog.contextvars.merge_contextvars,
                structlog.processors.add_log_level,
                trace_context,
                redact,
                structlog.processors.CallsiteParameterAdder(),
                structlog.processors.dict_tracebacks,
                structlog.processors.TimeStamper(fmt="iso"),
                structlog.processors.EventRenamer(to="body"),
                structlog.processors.JSONRenderer(serializer=_ENCODE),
            ],
            wrapper_class=structlog.make_filtering_bound_logger(20),
            logger_factory=structlog.BytesLoggerFactory(),
        )

    @staticmethod
    def emit(source: Streamable, redaction: Redaction, sink: BoundLogger | None = None) -> None:
        log = _sink(sink)
        for (sync, _), name, fields in _render(source, redaction):
            sync(log)(name, **fields)

    @staticmethod
    async def emit_async(source: Streamable, redaction: Redaction, sink: BoundLogger | None = None) -> None:
        log = _sink(sink)
        for (_, amirror), name, fields in _render(source, redaction):
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

- [OTLP_LOG_EGRESS]: reflection-confirmed — the `opentelemetry-sdk` `_logs.LogRecordProcessor`/`_logs.export.BatchLogRecordProcessor` log-egress wiring (no native `structlog` OTLP export) pairs with the `opentelemetry-exporter-otlp-proto-http` `OTLPLogExporter(endpoint, headers, timeout, compression, ...)` constructor, and `structlog.contextvars.merge_contextvars` is the bound-context processor. The provider/processor/exporter NOW INSTALL once at `observability/telemetry#TELEMETRY` (the composition-root install owner this campaign lands); this owner reads the installed `LoggerProvider` and wires no private `LogRecordProcessor`/`OTLPLogExporter`. The metric egress is the sibling `observability/metrics#METRIC` reading the installed `MeterProvider` over the same shared OTLP exporter family and one `Resource`. The log-egress wiring is settled.
- [PROCESSOR_CHAIN]: reflection-confirmed against `libs/python/.api/structlog.md` `[03]-[ENTRYPOINTS]`/`[04]-[IMPLEMENTATION_LAW]` — the production chain orders `contextvars.merge_contextvars` (ENTRYPOINTS contextvars [01], first so the ambient `Correlation`/`RuntimeContext` is present downstream) → `processors.add_log_level` (PUBLIC_TYPES processors [03]) → the custom `trace_context` reading `trace.get_current_span().get_span_context()` for `trace_id`/`span_id`/`trace_flags` → the custom `redact` processor applying the per-emit `Redaction` bound under `_REDACTION` to the fully-assembled line and stripping the carrier (placed after the contextvars/trace injectors so it scrubs the `Correlation`/`RuntimeContext`/`trace_id` fields a pre-chain `_render` fold would miss, and before the renderer) → `processors.CallsiteParameterAdder` (PUBLIC_TYPES processors [04], placed after the filtering wrapper admits per the `CallsiteParameterAdder` cost law) → `processors.dict_tracebacks` (PUBLIC_TYPES processors [12], the preconfigured `ExceptionRenderer(ExceptionDictTransformer())` that renders a bound `BoundaryFault`/`exc_info` to a JSON-safe structured stack rather than the rejected `str(exc)` form) → `processors.TimeStamper` → `processors.EventRenamer(to="body")` (PUBLIC_TYPES processors [06], aligning the `event` key to the OTLP log `body` field the bridge reads) → `processors.JSONRenderer(serializer=...)` (PUBLIC_TYPES processors [16]) renderer-last. The renderer `serializer` is the `msgspec` `json.Encoder(enc_hook=repr).encode` per the structlog `STACKS_WITH` "msgspec fast JSON" row, paired with `BytesLoggerFactory` (PUBLIC_TYPES [06]) so the encoder's `bytes` output flows to the sink with no stdlib `json` re-encode and native ints/floats reach the line without `str()` coercion, the `enc_hook=repr` (stateful-codec ENTRYPOINTS [01]) degrading any value outside the native JSON set to its repr so a renderer raise never kills the emit, and `make_filtering_bound_logger(20)` (ENTRYPOINTS [04]) is the `wrapper_class` compiling sub-`INFO` levels to no-ops at method resolution. The bounded `LogLevel` keys the `LEVEL_METHOD` `Map` whose every row is a `(sync, async)` `LevelBinding` selector pair: `emit` binds the `FilteringBoundLogger.debug`/`info`/`warning`/`error` sync method (ENTRYPOINTS bound-method [02]) and `emit_async` awaits the `adebug`/`ainfo`/`awarning`/`aerror` mirror (ENTRYPOINTS bound-method [03]) off the one row rather than a stringly `getattr(log, level)`/`getattr(log, "a" + level)`. The async mirror offloads the render-and-sink to a worker thread per the structlog `STACKS_WITH` "anyio structured concurrency" row, so a high-volume async serve or `drained` path emits without blocking the event loop while the sync `emit` serves non-loop callers, and `make_filtering_bound_logger(20)` compiles both the sync and the async sub-`INFO` calls to no-ops. The chain, serializer, and sink-pair spellings are settled.
- [RECEIPT_PROJECTION]: the union owns its own `(LogLevel, EventDict)` projection through one total `Receipt.project` `match`, so `Signals.emit` is a renderer-agnostic fold over `project` polymorphic across the `Streamable` alias (`Receipt | Iterable[Receipt] | ReceiptContributor`), one input-shape `match` normalizing every source to one `Iterable[Receipt]` rather than three hand-built dispatch arms — the `fact` case keys its level off the `PHASE_LEVEL` `Map` (`admitted`/`planned`→`debug`, `emitted`→`info`) with no phase branch, `rejected` spreads the `reliability/faults#FAULT`-owned `BoundaryFault.facts()` total structured-egress projection (the one `match` the fault owner declares, carrying `tag`/`subject`/`detail`/`code`/`budget`/`members` and naming an aggregate's member tags) rather than a private fault walk this owner re-implements — folding through the owner's projection keeps the rejected log line's slot set in lockstep with `reliability/faults#FAULT` and the `csharp:Rasm.AppHost` span fault attributes the same projection feeds, and the lossy `{"tag": ...}`-only node that dropped `subject`/`detail` is the deleted form; `drained` projects the five outcome counts read per-column off the typed `DrainReceipt` through `getattr(drain, column)` over the imported `DRAIN_COLUMNS` (a full `asdict` the deleted form, matching the sibling `observability/metrics#METRIC` `_drain_fold` so neither signal allocates the receipt's `values`/`cache`/`faults` containers on the egress path) plus the `psutil` RSS fact; the projected `LogLevel` selects the bound `structlog` method through the `(sync, async)` `LEVEL_METHOD` rows keyed on the closed literal rather than a stringly `getattr(log, level)`, and `Receipt.of` mints each case by `match`ing its `Evidence`-aliased input shape (`(Phase, subject, facts)`→`fact`, `BoundaryFault`→`rejected`, `DrainReceipt[object]`→`drained`) so every sibling mints through one factory rather than a hand-built case constructor or a parallel `of_*` family. The renderer/redaction `Encoder` carries `enc_hook=repr` so an unexpected bound value degrades to its repr rather than raising and killing the emit, and `order="deterministic"` so the one encoder serves both the renderer and the canonical `hash`-class input. `ReceiptContributor.contribute` yields an `Iterable[Receipt]` so a multi-phase contributor (a `GraduationReceipt` carrying admit+emit) streams its facts rather than forcing one receipt per call, and `@receipted` is the AOP aspect wrapping a contributor-returning operation (sync via the function routed to `emit`, async via the awaited coroutine routed to the loop-friendly `emit_async`, dispatched on `iscoroutinefunction`) to harvest and emit the stream on exit through the same `_stream`-normalized fold — receipt emission a decorator rail, not an inline call threaded through each measured kernel. The projection/contributor/aspect shapes are settled.
- [TRACE_INBOUND_EXTRACT]: reflection-confirmed against the branch `libs/python/.api/opentelemetry-api.md` — `propagate.extract(carrier, context)` (ENTRYPOINTS [11]) decodes the W3C `traceparent`/`tracestate` carrier into a `Context` (PUBLIC_TYPES [05]) resolved through `propagate.get_global_textmap()` (ENTRYPOINTS [13], the composite `observability/telemetry#TELEMETRY` installs via `set_global_textmap` ENTRYPOINTS [14]); the carrier is the `Mapping[str, str]` the servicer projects from `invocation_metadata()` and the `DefaultGetter` (PUBLIC_TYPES [10]) reads it in place — `propagate.extract(carrier)` takes the mapping directly, never a `dict(carrier)` re-copy on the inbound hop — and `context.attach`/`context.detach` (ENTRYPOINTS [02]/[03]) are the token-paired scoped activation. The seam is one polymorphic pair, not a fused scope: `Signals.continue_inbound` is the pure `propagate.extract` returning the decoded `Context` as a value, and `Signals.attach` is the separate `@contextmanager` that `context.attach`es it, yields, and `context.detach`es in `finally`. The split is load-bearing because the `transport/serve#SERVE` `ServerHost.inbound` must thread the extracted `Context` through its `RuntimeRail` admission pair (`(Context, RuntimeContext)`) outside the `boundary("wire", ...)` fence — a non-failing parent resolve, never a parse fault — while `ServerHost.dispatch` opens `attach(parent)` around exactly the decode/handler/encode body after the admission rail resolves `Ok`; a single fused extract-and-activate scope cannot be placed at that split. The activation `context.attach`/`context.detach` token-pair is the same `Context` stitch the sibling `execution/lanes#LANE` `traced_kernel` runs on the offload hop. `ServerHost.inbound` reads the carrier off `grpc.aio.ServicerContext.invocation_metadata()` projected to `dict[str, str]` and calls `continue_inbound` before the servicer body opens its span scope. Producer is `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` (the `TraceContextPropagator`/`BaggagePropagator` composite registered as `Propagators.DefaultTextMapPropagator`); the extract resolving through the default no-op propagator before the install is the mechanical reason this sequences after `observability/telemetry#TELEMETRY`. Realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` Python leg. Spellings settled.
