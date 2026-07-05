# [PY_RUNTIME_RECEIPTS]

This owner produces local evidence — typed receipts and structured log facts — and contributes nothing to provider install, product telemetry export, or health.

`Receipt` is the one tagged-union evidence family over `fact`/`rejected`/`drained` cases, and it owns its own projection. Each case folds to a `(LogLevel, name, EventDict)` triple through one total `project` `match` — the event name its own slot, never packed under an `"event"` key the sink re-pops — so `Signals.emit` is a renderer-agnostic fold over the union rather than three hand-built dict arms.

The drain taxonomy is MINTED here: a drain receipt IS local evidence, so `DrainOutcome`, the `get_args`-derived `DRAIN_COLUMNS`, and the `DrainReceipt[T]` model with its `.of` split-fold live on this page — the `Receipt.drained` case binds `DrainReceipt[object]` at class body, the `cache: Map[ContentKey, T]` column imports `rasm.runtime.identity` strictly downward, and `execution/lanes#LANE` (the producing drain) and `observability/metrics#METRIC` (the `lane.drained` counter) import the taxonomy FROM this owner, so the receipt column set, the counter outcome dimension, and the drain fold can never drift and no upward import survives.

`ReceiptContributor` is the `Protocol` port every sibling's typed receipt streams through — data `QueryReceipt`, compute `GraduationReceipt`, geometry/artifacts `ArtifactReceipt` — its `contribute` yielding a receipt sequence rather than one forced fact. `@receipted` is the cross-cutting aspect that wraps a measured operation, harvests its contributor stream, and emits on exit, so receipt production is a decorator rail rather than inline `emit` calls scattered through the siblings. The aspect routes its sync arm through `Signals.emit` and its async arm through `Signals.emit_async`, the latter awaiting the loop-friendly `FilteringBoundLogger` `a*` mirror so a high-volume async serve path offloads render-and-sink to a worker thread rather than blocking the event loop.

`Redaction` is a `Classification`-keyed field-policy table over `drop`/`mask`/`hash`, and it is a chain-resident processor, never a pre-chain fold. The `hash` class threads stdlib `hashlib.blake2b` so a secret yields a stable correlation token rather than an opaque `***`. The placement is load-bearing: a per-emit `Redaction` rides bound onto the event under one reserved `_REDACTION` key, and the `redact` processor sits AFTER `merge_contextvars`/`trace_context` so it scrubs the ambient `Correlation`/`RuntimeContext` fields and the injected `trace_id`/`span_id` alongside the receipt's own facts — a pre-chain `_render`-side `apply` over only the projected `EventDict` lets a classified field arriving through `bind_contextvars` reach the line unredacted, the deleted form. The `structlog` processor chain injects OTel trace context and structured fault tracebacks, and `psutil` supplies the process RSS fact.

`Signals` carries both directions of the W3C trace-context seam. The outbound `trace_context` processor injects the active span's `trace_id`/`span_id`/sampled flag into every event. The inbound seam is one polymorphic pair: `continue_inbound` is the pure extract that decodes the C#-minted parent off the gRPC carrier into a `Context` value, and `attach` is the token-paired activation scope the consumer opens around exactly its measured body. The servicer leg threads the extracted `Context` through its admission rail before opening the span scope, a placement a single fused extract-and-activate scope cannot serve. OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`, since `structlog` mints no native OTLP log export.

## [01]-[INDEX]

- [01]-[RECEIPT]: the minted `DrainOutcome`/`DRAIN_COLUMNS`/`DrainReceipt[T]` drain taxonomy with its `.of` split-fold, the self-projecting receipt union with its shape-polymorphic `of` factory, the streaming contributor port, the `@receipted` aspect feeding the `emit`/`emit_async` sink pair over the level-numbered `(sync, async)` `LEVEL_METHOD` rows, the classification-keyed redaction table applied by the chain-resident `redact` processor, the structlog processor chain, the inbound trace-context `continue_inbound` extract paired with the `attach` activation scope.

## [02]-[RECEIPT]

- Owner: `DrainOutcome`/`DRAIN_COLUMNS`/`DrainReceipt[T]` — the one canonical drain taxonomy minted HERE: `DrainOutcome` the closed five-column `Literal` vocabulary (`accepted`/`completed`/`cancelled`/`rejected`/`hit`), `DRAIN_COLUMNS` the `get_args(DrainOutcome.__value__)`-derived column tuple so the set is one typed fact, and `DrainReceipt[T]` the parameterized outcome model carrying `values: Block[T]`, `cache: Map[ContentKey, T]` (the `rasm.runtime.identity` downward import), the typed `faults: Block[BoundaryFault]`, and the five counts, its `.of` split-fold draining resolved rails via `Block.choose(rail.to_option())`/`Block.choose(rail.swap().to_option())` and threading the session cache via `Block.fold` — `execution/lanes#LANE` produces it and `observability/metrics#METRIC` streams it, both importing this owner; `Receipt` — the one evidence union with slot/kind metadata owning its own `(LogLevel, name, EventDict)` projection and a shape-polymorphic `of` factory that mints every case by discriminating its input evidence; `LogLevel` the bounded structlog-method vocabulary the `LEVEL_METHOD` table keys, each row a `LevelBinding` carrying the numeric structlog level plus the `(sync, async)` selector pair so one table binds the `configure` filter floor, `log.info`, and the loop-friendly `log.ainfo`; `ReceiptContributor` the Protocol port the sibling typed receipts implement as a receipt stream; `Classification`/`Redaction` the field-classification policy table the chain-resident `redact` processor applies; `Signals` the static surface owning the `structlog` processor chain (`trace_context` and `redact` its two custom processors), the `psutil` process telemetry facts, the `emit`/`emit_async` sink pair feeding the `@receipted` aspect, the pure `continue_inbound` parent-context extract, and the `attach` activation scope, the trace context and the redaction both riding the processor chain rather than a pre-chain fold or a cached tracer.
- Cases: the three lifecycle phases share one `fact` case carrying `(Phase, owner, subject, facts)` — `admitted`/`planned`/`emitted` are a `Phase` literal value the case routes, not three identical-payload sibling cases; `rejected` carries `(owner, BoundaryFault)` and `drained` carries `(owner, DrainReceipt[object])` because their payloads differ; the `rejected` subject is never a pre-extracted slot beside the fault — it lives inside the fault's own `facts()` projection, so the case carries the owner plus the fault and nothing duplicated; correlation flows through `merge_contextvars`, never a per-case field. Each case's log disposition is data, not a dispatch arm: `Receipt.project` is the one total `match` mapping every case to its `(LogLevel, name, EventDict)` triple — the `fact` case projects the `Phase`-keyed level off the `PHASE_LEVEL` table so `admitted`/`planned` log at `debug` and `emitted` at `info` without a phase branch, `rejected` projects `warning` plus the spread of the `reliability/faults#FAULT`-owned `BoundaryFault.facts()` total structured-egress map (`tag`/`subject`/`detail`/`code`/`budget`/`cause`/`members` per the leaf case, the aggregate naming its member tags) rather than a private fault walk, `drained` projects `info` plus the `DRAIN_COLUMNS` outcome map and the RSS fact, so a new case is one match arm on `project` and a new phase is one `PHASE_LEVEL` row, never an `emit`-side dispatch edit.
- Entry: `Receipt.of(owner, evidence)` is the one shape-polymorphic factory over the `Evidence` input alias, `match`ing the argument so a `(Phase, subject, facts)` triple mints `fact`, a `BoundaryFault` mints `rejected` carrying the owner plus the whole fault (the subject deferred to `facts()` at `project` time rather than pre-extracted into a redundant slot), and a `DrainReceipt[object]` mints `drained`. A new evidence shape is one `of` match arm, never a hand-built `Receipt(rejected=...)`/`Receipt(drained=...)` at a call site or a parallel `of_rejected`/`of_drained` family. `Receipt.project` is the total case-to-`(LogLevel, name, EventDict)` fold the union owns. `ReceiptContributor.contribute` is the one method a sibling implements to yield its typed receipt stream into the pipeline.
- Entry: `Signals.emit`/`Signals.emit_async` are the renderer-agnostic sink pair polymorphic over input and output. On the input axis a single `Receipt`, an `Iterable[Receipt]`, or a `ReceiptContributor` normalize through one `_stream` `match` to one `Iterable[Receipt]`. On the output axis the optional `sink` defaults to the global `structlog.get_logger()` but accepts any `FilteringBoundLogger`, so a `structlog.testing.capture_logs`/`LogCapture` test or a `wrap_logger`-local consumer drives the same fold without a second emit surface. The `project` -> bind the per-emit `Redaction` under `_REDACTION` -> `LEVEL_METHOD[level]` lookup is one `_render` generator both methods consume, yielding a `(LevelBinding, name, fields)` triple per receipt so the projection and level dispatch live once rather than once per arm; redaction is NOT folded here — `_render` carries the `Redaction` into the line and the chain-resident `redact` processor applies it after the contextvars/trace injectors so a `bind_contextvars`-sourced classified field cannot bypass it. The two methods reduce to the one shape Python forces — `emit` drives the row's sync selector (`log.info`) and `emit_async` awaits its `a*` mirror (`log.ainfo`) over the same `_render` stream, never a stringly `getattr(log, <level-name>)`/`getattr(log, "a" + <level-name>)` over an open namespace and never a re-walked `_stream`+`project` body duplicated across the pair.
- Entry: `Signals.continue_inbound(carrier)` is the pure `propagate.extract(carrier)` over the `Mapping[str, str]` carrier read directly (the `DefaultGetter` reads the mapping in place, never a `dict(carrier)` re-copy on the inbound hot path), resolving the parent through `propagate.get_global_textmap` (the composite `observability/telemetry#TELEMETRY` installs) and returning the decoded `Context` as a value the servicer threads through its admission rail before any span scope. `Signals.attach(parent)` is the separate token-paired `@contextmanager` that `context.attach`es the `Context`, yields it, and `context.detach`es in `finally`. The split is load-bearing: `transport/serve#SERVE` `ServerHost.inbound` extracts outside the wire-fault fence and `dispatch` opens `attach(parent)` around the decode/handler/encode body, a placement a single fused extract-and-activate scope cannot serve. The C# producer is `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` (the `TraceContextPropagator`/`BaggagePropagator` composite registered as `Propagators.DefaultTextMapPropagator`), so this pair realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` Python leg and mechanically sequences after the `observability/telemetry#TELEMETRY` install — before it the extract reads the default no-op propagator and the C# parent drops.
- Entry: `@receipted` is the AOP aspect that wraps a `ReceiptContributor`-returning operation and emits its harvested stream on exit through the same `_stream`-normalized fold, routing a sync op to `emit` and a coroutine to `emit_async` off `iscoroutinefunction`. A measured kernel declares `@receipted(redaction)` and threads no emit call through its body. The aspect is parameterized over the concrete contributor return type through the `R: ReceiptContributor`-bound `Contributing[P, R]` alias, so a `@receipted`-decorated `_emit(x: T) -> T` over a concrete `T <: ReceiptContributor` (a `ModelAssetManifest`, an `InferenceReceipt`, a `GraduationReceipt`) statically returns `Callable[P, T]` rather than collapsing to the bare `ReceiptContributor` Protocol — so the consumer's `boundary(..., lambda: _emit(...))` `Ok` arm reads the concrete `receipt.span_facts`/`manifest.span_facts` member without a static type error, the bound preserving the subtype the decorator harvests rather than erasing it to the port that declares only `contribute`.
- Auto: the `structlog` processor chain binds, in order, `merge_contextvars` (carrying the `Correlation`/`RuntimeContext` bound context) first, `add_log_level` stamping the admitted level onto the event, the custom `trace_context` reading `opentelemetry.trace.get_current_span()` to inject `trace_id`/`span_id`/`trace_flags`, the `redact` processor applying the per-emit `Redaction` bound under `_REDACTION` to the fully-assembled line (so it scrubs the `merge_contextvars` ambient fields and the injected trace ids, not just the receipt's own facts) and stripping the `_REDACTION` carrier before render, `CallsiteParameterAdder` resolving the emitting module/function once after the level admits the event, `dict_tracebacks` transforming a bound `exc_info`/`BoundaryFault` cause into a JSON-safe structured stack rather than a flattened `str(exc)`, a `TimeStamper`, an `EventRenamer` mapping `event` to the OTLP `body` key, and a renderer-last `JSONRenderer`. The `redact` placement after the contextvars/trace injectors is load-bearing: a pre-chain `_render`-side `apply` over only the projected facts lets a `bind_contextvars`-sourced classified field reach the line unredacted. The renderer `serializer` is the `msgspec` `json.Encoder(enc_hook=repr, order="deterministic").encode` paired with a `BytesLoggerFactory` sink, so domain `Struct` facts and native ints/floats reach the line through the one fast encoder that owns the wire without a `str()` coerce. The `enc_hook=repr` degrades any value outside the native JSON set to its repr so a bound domain object renders rather than killing the emit on the hot path, and `order="deterministic"` fixes mapping key order so the one encoder doubles as the canonical `hash`-class input. `make_filtering_bound_logger` compiles sub-threshold levels to no-ops on both the sync and the `a*` async-mirror methods, its floor the `configure` caller's `LogLevel` read off the owning `LEVEL_METHOD` row, never a bare numeric literal.
- Auto: `emit_async` awaits the `FilteringBoundLogger` `a*` mirror so the async serve and `drained` paths offload the JSON render and byte sink to a worker thread rather than blocking the event loop under high log volume, while `emit` keeps the inline sync path for non-loop callers — one `LEVEL_METHOD` row binding both selectors off the closed `LogLevel`. Span creation belongs to the measured operation, not to receipt emission, so emit writes under whatever span is active.
- Auto: OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`/`OTLPLogExporter` (the `structlog`-to-OTel bridge, since `structlog` mints no native OTLP log export); this owner wires no private `LogRecordProcessor`/`OTLPLogExporter`. `continue_inbound` resolves the parent through the installed composite propagator, so before the install the extract reads the default no-op propagator and returns an empty `Context`, `attach` activates a no-op scope, and the C# parent is dropped — the mechanical reason the extract sequences after the install.
- Auto: `Redaction.apply` is run by the chain-resident `redact` processor, never folded into `_render`: each fact classifies through the `Classification`-keyed `Map[str, Classification]` table, never a parallel allow/deny set — a `drop` field never reaches a line, a `mask` field renders the fixed sentinel, and a `hash` field renders a truncated `blake2b` correlation token over the `order="deterministic"` `_ENCODE` of the value keyed by the redaction's `salt`. The deterministic key order makes a structured secret hash canonically regardless of insertion order, so two lines carrying the same secret correlate by token without leaking the value. `_render` binds the per-emit `Redaction` onto the event under the reserved `_REDACTION` key and `apply` skips that key while scrubbing; a line carrying no bound `Redaction` (a foreign stdlib-bridge record) folds through the keep-all `OPEN` policy — `redact` `isinstance`-narrows the reserved slot, so an unbound line AND a foreign value colliding on `_REDACTION` both fold to `OPEN` instead of faulting the render — so redaction stays the per-receipt-stream contract `emit` binds rather than a global scrub, and `OPEN` anchors after the `Redaction` model rather than top-level `[CONSTANTS]` because it depends on it. `OPEN` is the public keep-all every sibling whose receipt facts carry no classified field threads into `@receipted(OPEN)`, so no producer re-mints a `Redaction(classified=Map.empty())` per file.
- Auto: the drained projection reads each OWN `DRAIN_COLUMNS` column directly off the typed `DrainReceipt` through one `getattr(drain, column)` per column, so the line carries exactly the five outcome counts of the `DrainOutcome` taxonomy this page declares while the typed `faults`/`values`/`cache` `Block`/`Map` containers stay the structural carry, never materialized on the emit path. A full `msgspec.structs.asdict` is the deleted form here, matching the sibling `observability/metrics#METRIC` `_drain_fold`, since `asdict` would allocate the receipt's `values`/`cache`/`faults` containers on every emit only to drop them where the five-scalar `getattr` read stays a typed field access. The same `accepted`/`completed`/`cancelled`/`rejected`/`hit` columns the `observability/metrics#METRIC` `lane.drained` `ObservableCounter` keys by its `outcome` attribute read through the identical per-column `getattr`-over-`DRAIN_COLUMNS`, cache-`hit` a first-class outcome dimension on both signals: the receipt is the per-drain point fact, the counter the streamed gauge over the latest fold, both reading the one taxonomy this owner declares and the siblings import downward, so the line and the `outcome`-keyed counter cannot disagree on the column set and a new `DrainOutcome` member reaches both by one literal member plus one `DrainReceipt` field here.
- Auto: `_rss()` reads `_PROCESS.memory_info().rss` off the one own-process handle minted once at module load under `contextlib.suppress(*PROCESS_FAULTS)`, so the drained projection stays a pure dict spread that attaches the RSS slot on success and omits it on a `NoSuchProcess`/`ZombieProcess`/`AccessDenied` race rather than raising and killing the measured operation's egress. This is the same RSS fact the metrics `process.memory.rss` `ObservableGauge` streams (one `psutil` source, the receipt a point fact, the gauge the stream), guarded exactly as the metrics owner's `ProcessReading.sample` guards its read; the metrics sibling carries its own handle, so receipts never owns the `Process` the metrics owner streams. The default sink resolves through one `_sink` fold both `emit` arms share — `Option.of_optional(sink).default_with(structlog.get_logger)` fetches the global logger per emit (never cached as a module constant) when `sink` is absent and returns the passed `sink` for the `testing.capture_logs`/`wrap_logger` seams, so the override contract lives once rather than a ternary duplicated across the sync/async pair.
- Growth: a new drain outcome is one `DrainOutcome` literal member plus one `DrainReceipt` count field, reaching the drained line, the metrics counter, and the lanes fold through the one `DRAIN_COLUMNS` derivation with zero consumer edit; a new lifecycle phase is one `Phase` literal plus one `PHASE_LEVEL` row absorbed by the existing `fact` case; a distinct-payload evidence kind is one `Receipt` case plus its `project` match arm plus its `of` discriminant arm on the `Evidence` alias (the factory minting it by input shape); a new structured slot on a rejected fault reaches the log line for free through the `BoundaryFault.facts()` spread, never a `project` edit here, since the rejected case folds the owner's projection rather than a private node; a new classified field is one `Redaction` table row keyed by its `Classification`; a new classification transform is one `Classification` member plus one `_reduce` arm (the `assert_never` total-match forcing the addition); a new log level is one `LogLevel` literal plus one `LEVEL_METHOD` row (numeric level plus selector pair) reaching `configure`'s floor, `emit`, and `emit_async` at once; a new processor is one entry in the `structlog` chain; a new measured op participates by returning a `ReceiptContributor` under `@receipted`, the aspect routing it to the loop-friendly `emit_async` when the op is a coroutine; a new sink target (a test capture buffer, a local pipeline) is the `sink` argument on the existing `emit`/`emit_async` pair, never a second emit method; zero new emit surface.
- Boundary: the deleted forms are:
  - a per-package parallel receipt rail; an upward `from rasm.runtime.lanes import ...` edge or a second drain vocabulary beside the one this page mints (the E1 import cycle's two spellings); a bare unparameterized `DrainReceipt` carry where the `DrainReceipt[object]` pin makes the boundary erasure explicit;
  - a stdlib `logging` call outside the structlog bridge; a `str()`-coerced fact map where the `msgspec` renderer carries native types; a bare `Encoder()` whose serializer raises on an unexpected bound value where `enc_hook=repr` degrades it;
  - three hand-built `emit` dispatch arms where the union's own `project` folds them; a stringly `getattr(log, <level-name>)`/`getattr(log, "a" + <level-name>)` where the `(sync, async)` `LEVEL_METHOD` rows dispatch the bound method over the closed `LogLevel`; a parallel async-mirror selector table beside the sync one where one `LevelBinding` row carries both; a synchronous `emit` driven inline on an async serve or `drained` hot path where `emit_async` awaits the loop-friendly `a*` mirror; a duplicated `project`/level-lookup body in `emit` and `emit_async` where the one `_render` generator yields the `(LevelBinding, name, fields)` triple both arms consume and only the sync-call-versus-`await` half differs; an event name packed into the projected fields only for the sink to `pop` it back out where `project` carries the name as its own triple slot; a bare numeric filter floor beside the `LEVEL_METHOD` row's own level number;
  - a pre-chain `_render`-side `Redaction.apply` over only the projected `EventDict` where the chain-resident `redact` processor (after `merge_contextvars`/`trace_context`) scrubs the whole assembled line so a `bind_contextvars`-sourced classified secret cannot leak past redaction; a flat `***`-only redaction where the classification table discriminates `drop`/`mask`/`hash`; a stringly out-of-vocabulary `"keep"` default outside the closed `Classification` set where the `try_find().map(...).default_value((value,))` Option fold keeps the un-classified field and `_reduce` stays total over the real members under `assert_never`;
  - a parallel `of_rejected`/`of_drained` factory family where the one shape-polymorphic `of` discriminates by input; a hand-built `Receipt(rejected=...)`/`Receipt(drained=...)` at a sibling call site where `of` mints it; a non-total `of`/`project`/`_reduce` `match` lacking the `assert_never` tail; a `structs.asdict(drain)`-then-index drained projection allocating the receipt's `values`/`cache`/`faults` containers on every emit where the per-column `getattr` over `DRAIN_COLUMNS` reads five scalars off the typed struct; a private fault-node walk re-implementing the `reliability/faults#FAULT`-owned `BoundaryFault.facts()` aggregate fold (a lossy `{"tag": ...}`-only node dropping the `subject`/`detail`/`code`/`budget` slots the canonical projection carries) where the `rejected` projection spreads `fault.facts()`; a redundant `rejected` subject slot pre-extracted beside the fault where `facts()["subject"]` carries it;
  - a hard-wired global `get_logger()` sink with no override where the `sink` parameter feeds a `testing.capture_logs`/`wrap_logger` consumer; a `sink if sink is not None else get_logger()` ternary duplicated across the `emit`/`emit_async` pair where the one `_sink` `Option.of_optional(...).default_with(get_logger)` fold owns the default-resolution contract;
  - a `dict(carrier)` re-copy in `continue_inbound` where the `DefaultGetter` reads the `Mapping` in place; a fused extract-and-activate `continue_inbound` scope a servicer cannot place where the pure extract feeds the rail and the separate `attach` scope wraps the body; a second tracer minted for the inbound extract; a fresh-root span where the C# parent is on the inbound carrier.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Iterator, Mapping
from contextlib import contextmanager, suppress
from functools import wraps
from hashlib import blake2b
from inspect import iscoroutinefunction
from typing import Final, Literal, Protocol, assert_never, get_args, runtime_checkable

import psutil
import structlog
from expression import Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import Encoder
from opentelemetry import context, propagate, trace
from opentelemetry.context import Context

from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentKey

# --- [TYPES] ----------------------------------------------------------------------------

type DrainOutcome = Literal["accepted", "completed", "cancelled", "rejected", "hit"]
type Phase = Literal["admitted", "planned", "emitted"]
type LogLevel = Literal["debug", "info", "warning", "error"]
type EventDict = dict[str, object]
type Classification = Literal["drop", "mask", "hash"]
type Evidence = tuple[Phase, str, dict[str, object]] | BoundaryFault | DrainReceipt[object]
type Streamable = Receipt | Iterable[Receipt] | ReceiptContributor
type Contributing[**P, R: ReceiptContributor] = Callable[P, R] | Callable[P, Awaitable[R]]
type BoundLogger = structlog.typing.FilteringBoundLogger
type LevelSelector = Callable[[BoundLogger], Callable[..., object]]
type LevelBinding = tuple[int, LevelSelector, LevelSelector]

# --- [CONSTANTS] ------------------------------------------------------------------------

# the taxonomy's one typed derivation: the column set IS the DrainOutcome literal, so the
# drained line, the metrics counter, and the lanes fold can never disagree on the columns.
DRAIN_COLUMNS: Final[tuple[DrainOutcome, ...]] = get_args(DrainOutcome.__value__)

PHASE_LEVEL: Final[Map[Phase, LogLevel]] = Map.of_seq([("admitted", "debug"), ("planned", "debug"), ("emitted", "info")])

REDACTED: Final[str] = "***"

# the reserved event key the per-emit Redaction rides so the chain-resident `redact` scrubs the
# whole assembled line; `apply` strips it before the line renders.
_REDACTION: Final[str] = "_redaction"

# one row per LogLevel carrying (numeric level, sync selector, async mirror) — configure derives
# the filter floor, emit binds log.info, emit_async the loop-friendly log.ainfo — never a stringly
# getattr(log, level) and never a bare numeric floor literal beside the row that owns it.
LEVEL_METHOD: Final[Map[LogLevel, LevelBinding]] = Map.of_seq([
    ("debug", (10, lambda log: log.debug, lambda log: log.adebug)),
    ("info", (20, lambda log: log.info, lambda log: log.ainfo)),
    ("warning", (30, lambda log: log.warning, lambda log: log.awarning)),
    ("error", (40, lambda log: log.error, lambda log: log.aerror)),
])

# the one renderer/redaction encoder: enc_hook=repr degrades non-native values instead of raising
# on the hot path; order="deterministic" fixes key order so it doubles as the canonical hash-class input.
_ENCODE: Final[Callable[[object], bytes]] = Encoder(enc_hook=repr, order="deterministic").encode

# own-process handle minted once; the drained point-fact reads RSS off it rather than
# re-minting a Process() per projection. The metrics sibling carries its own handle.
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

    def project(self) -> tuple[LogLevel, str, EventDict]:
        # the event name is its own slot — never packed under an "event" key the sink re-pops.
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


# the public keep-all policy: an unbound line folds through it, and a sibling with no classified
# field threads @receipted(OPEN); depends on the Redaction model, so it anchors here, never top-level [CONSTANTS].
OPEN: Final[Redaction] = Redaction(classified=Map.empty())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _rss() -> EventDict:
    with suppress(*PROCESS_FAULTS):
        return {"rss": _PROCESS.memory_info().rss}
    return {}


# the one default-sink resolution both emit arms share: explicit `sink` for the capture_logs/
# wrap_logger seam, absent folds to the global get_logger fetched per emit, never cached.
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


# the one render fold both sinks share — emit/emit_async differ only by which selector half they
# drive. Redaction is NOT applied here: the chain-resident `redact` runs after the contextvars/
# trace injectors; this fold only binds it under `_REDACTION`.
def _render(source: Streamable, redaction: Redaction) -> Iterator[tuple[LevelBinding, str, EventDict]]:
    for receipt in _stream(source):
        level, name, fields = receipt.project()
        yield LEVEL_METHOD[level], name, fields | {_REDACTION: redaction}


# scrubs the fully-assembled line — receipt facts, merge_contextvars ambient fields, trace ids —
# so a bind_contextvars-sourced classified field cannot bypass redaction; a line with no bound
# Redaction (a foreign stdlib-bridge record) folds through the keep-all OPEN policy untouched.
def redact(_: object, __: str, event: EventDict) -> EventDict:
    bound = event.get(_REDACTION)
    return (bound if isinstance(bound, Redaction) else OPEN).apply(event)


def trace_context(_: object, __: str, event: EventDict) -> EventDict:
    ctx = trace.get_current_span().get_span_context()
    if ctx.is_valid:
        event.update(trace_id=trace.format_trace_id(ctx.trace_id), span_id=trace.format_span_id(ctx.span_id), trace_flags=int(ctx.trace_flags))
    return event


# --- [SERVICES] -------------------------------------------------------------------------


class Signals:
    @staticmethod
    def configure(floor: LogLevel = "info") -> None:
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
            wrapper_class=structlog.make_filtering_bound_logger(LEVEL_METHOD[floor][0]),
            logger_factory=structlog.BytesLoggerFactory(),
        )

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
