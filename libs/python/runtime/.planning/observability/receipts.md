# [PY_RUNTIME_RECEIPTS]

Local evidence production. `Receipt` is the one tagged-union evidence family (admitted/planned/emitted/rejected/drained rows) that owns its own projection — each case folds to a `(LogLevel, EventDict)` through one total `project` method, so `Signals.emit` is a renderer-agnostic fold over the union rather than three hand-built dict arms. `ReceiptContributor` is the Protocol port every other package's typed receipt — data `QueryReceipt`, compute `GraduationReceipt`, geometry/artifacts `ArtifactReceipt` — streams through (`contribute` yields its receipt sequence, never a single forced fact), and `@receipted` is the cross-cutting aspect that wraps a measured operation, harvests its contributor stream, and emits on exit so receipt production is a decorator rail, not inline emit calls scattered through the siblings. `Redaction` is a `Classification`-keyed field policy table (`drop`/`mask`/`hash`), the `hash` class threading stdlib `hashlib.blake2b` so a secret yields a stable correlation token rather than an opaque `***`; the `structlog` processor chain injects OTel trace context and structured fault tracebacks, and `psutil` supplies process facts. `Signals` carries both directions of the W3C trace-context seam: the outbound `trace_context` processor injects the active span's `trace_id`/`span_id`/sampled flag into every event, and the inbound seam is one polymorphic pair — `continue_inbound` is the pure extract that decodes the C#-minted parent off the gRPC carrier into a `Context` value, and `attach` is the token-paired activation scope the consumer opens around exactly its measured body — so the servicer leg can thread the extracted `Context` through its admission rail before opening the span scope rather than being forced to extract and activate in one fused scope it cannot place. OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`; the package contributes receipts and structured facts, never the provider install, product telemetry export, or health.

## [01]-[INDEX]

- [01]-[RECEIPT]: the self-projecting receipt union with its shape-polymorphic `of` factory, the streaming contributor port, the `@receipted` emit aspect, the classification-keyed redaction table, the structlog processor chain, the inbound trace-context `continue_inbound` extract paired with the `attach` activation scope.

## [02]-[RECEIPT]

- Owner: `Receipt` — the one evidence union with slot/kind metadata owning its own `(LogLevel, EventDict)` projection and a shape-polymorphic `of` factory that mints every case by discriminating its input evidence; `LogLevel` the bounded structlog-method vocabulary the `LEVEL_METHOD` selector table keys; `ReceiptContributor` the Protocol port the sibling typed receipts implement as a receipt stream; `Classification`/`Redaction` the field-classification policy table; `Signals` the static surface owning the `structlog` processor chain, the `psutil` process telemetry facts, the `@receipted` emit aspect, the pure `continue_inbound` parent-context extract, and the `attach` activation scope, the trace context riding the processor chain rather than a cached tracer.
- Cases: the three lifecycle phases share one `fact` case carrying `(Phase, owner, subject, facts)` — `admitted`/`planned`/`emitted` are a `Phase` literal value the case routes, not three identical-payload sibling cases; `rejected` carries `(owner, subject, BoundaryFault)` and `drained` carries `(owner, DrainReceipt)` because their payloads differ; correlation flows through `merge_contextvars`, never a per-case field. Each case's log disposition is data, not a dispatch arm: `Receipt.project` is the one total `match` mapping every case to its `(LogLevel, EventDict)` pair — the `fact` case projects the `Phase`-keyed level off the `PHASE_LEVEL` table so `admitted`/`planned` log at `debug` and `emitted` at `info` without a phase branch, `rejected` projects `warning` plus the `BoundaryFault` folded to a structured `fault` node, `drained` projects `info` plus the `DRAIN_COLUMNS` outcome map and the RSS fact, so a new case is one match arm on `project` and a new phase is one `PHASE_LEVEL` row, never an `emit`-side dispatch edit.
- Entry: `Receipt.of(owner, evidence)` is the one shape-polymorphic factory — it `match`es the `evidence` argument so a `(Phase, subject, facts)` triple mints the `fact` case, a `BoundaryFault` mints the `rejected` case (reading its own `facts()` subject slot), and a `DrainReceipt` mints the `drained` case, so a sibling never hand-builds `Receipt(rejected=...)`/`Receipt(drained=...)` and a new evidence shape is one match arm rather than a parallel `of_rejected`/`of_drained` factory family; `Receipt.project` is the total case-to-`(LogLevel, EventDict)` fold the union owns; `ReceiptContributor.contribute` is the one method a sibling implements to yield its typed receipt stream into the pipeline; `Signals.emit` is renderer-agnostic and polymorphic over its input — a single `Receipt`, an `Iterable[Receipt]`, or a `ReceiptContributor` all fold through one path that calls `project`, redacts the `EventDict` through `Redaction.apply`, and writes the event off the per-call `structlog.get_logger` by indexing the `LEVEL_METHOD` selector table on the projected bounded `LogLevel` rather than a stringly `getattr(log, <level-name>)` over an open attribute namespace; `Signals.continue_inbound(carrier)` is the pure extract that runs `propagate.extract(carrier)` over the `dict[str, str]` carrier (resolved through `propagate.get_global_textmap`, the composite `observability/telemetry#TELEMETRY` installs) and returns the decoded `Context` as a value the servicer threads through its admission rail before opening any span scope, and `Signals.attach(parent)` is the separate token-paired `@contextmanager` that `context.attach`es the extracted `Context`, yields it, and `context.detach`es in `finally`, so the consumer activates the parent around exactly its measured body — the `transport/serve#SERVE` `ServerHost.inbound` extracts the `Context` outside the wire-fault fence and `dispatch` opens `attach(parent)` around the decode/handler/encode body, a split a single fused extract-and-activate scope cannot serve; `@receipted` is the AOP entry that wraps a `ReceiptContributor`-returning operation (sync or async) and emits its harvested stream on exit through the same `emit` fold, so a measured kernel declares `@receipted(redaction)` and never threads an emit call through its body.
- Auto: the `structlog` processor chain binds `merge_contextvars` (carrying the `Correlation`/`RuntimeContext` bound context) first, a custom `trace_context` processor reading `opentelemetry.trace.get_current_span()` to inject `trace_id`/`span_id`/`trace_flags` into every event, `CallsiteParameterAdder` resolving the emitting module/function once after the level admits the event, `dict_tracebacks` transforming any bound `exc_info`/`BoundaryFault` cause into a JSON-safe structured stack rather than a flattened `str(exc)`, a `TimeStamper`, an `EventRenamer` mapping `event` to the OTLP `body` key, and a `JSONRenderer` whose `serializer` is the `msgspec` `json.Encoder(enc_hook=repr).encode` so domain `Struct` facts serialize through the one fast encoder that owns the wire and native ints/floats reach the log line without a `str()` coerce, the `enc_hook=repr` degrading any value outside the native JSON set to its repr so a bound domain object renders rather than raising on the hot logging path (a renderer that throws kills the emit), paired with a `BytesLoggerFactory` sink and a `make_filtering_bound_logger` wrapper that compiles sub-threshold levels to no-ops; span creation belongs to the measured operation, not to receipt emission, so `emit` writes the event under whatever span is active; OTLP log egress rides the `observability/telemetry#TELEMETRY`-installed `LoggerProvider`/`OTLPLogExporter` (the `structlog`-to-OTel bridge, since `structlog` mints no native OTLP log export) — this owner wires no private `LogRecordProcessor`/`OTLPLogExporter`; `continue_inbound` resolves the parent through the installed composite propagator, so before the install the extract reads the default no-op propagator and returns an empty `Context` so `attach` activates a no-op scope and the C# parent is dropped (the mechanical reason the extract sequences after the install); `Redaction.apply` classifies each fact through the `Classification`-keyed table so a `drop` field never reaches a log line, a `mask` field renders the fixed sentinel, and a `hash` field renders a truncated `blake2b` correlation token keyed by the redaction's `salt` so two log lines carrying the same secret correlate without leaking the value — the classification is a `Map[str, Classification]` row, never a parallel allow/deny set; the drained projection threads the `DrainReceipt` through `msgspec.structs.asdict` indexed by the imported `execution/lanes#LANE`-owned `DRAIN_COLUMNS` so the log line carries exactly the five outcome columns of the shared `DrainOutcome` taxonomy (the typed `faults` `Block` being the structural carry on the receipt, not a count column) — the same `accepted`/`completed`/`cancelled`/`rejected`/`hit` columns the sibling `observability/metrics#METRIC` `lane.drained` `ObservableCounter` keys by its `outcome` attribute through the identical `asdict`-by-`DRAIN_COLUMNS` projection, cache-`hit` a first-class outcome dimension on both signals rather than a receipt-only fact — the receipt is the per-drain point fact and the counter the streamed gauge over the latest fold, both reading the one taxonomy the `DrainReceipt` owner declares, so the log line and the `outcome`-keyed counter can never disagree on the column set and a new `DrainOutcome` member reaches both signals by one field add on the shared owner; `_rss()` reads `_PROCESS.memory_info().rss` off the one own-process handle minted once at module load (the metrics sibling carries its own handle; receipts never owns the `Process` the metrics owner streams) under `contextlib.suppress(*PROCESS_FAULTS)`, so the drained projection stays a pure dict spread that attaches the RSS slot when the read succeeds and omits it on a `NoSuchProcess`/`ZombieProcess`/`AccessDenied` race rather than raising on the emit path and killing the measured operation's egress — the same RSS fact the metrics `process.memory.rss` `ObservableGauge` streams (one `psutil` source, the receipt a point fact, the gauge the stream), guarded exactly as the metrics owner's `ProcessReading.sample` guards its read; the logger is fetched per emit through `structlog.get_logger`, never cached as a module constant.
- Packages: `expression` (`tagged_union`/`case`/`tag`, `Map` over the redaction table, the `PHASE_LEVEL` level table, and the `LEVEL_METHOD` bound-method selector table), `msgspec` (`Struct`, `structs.asdict`, `json.Encoder(*, enc_hook, ...)` stateful-codec ENTRYPOINTS [01] with `enc_hook=repr` as the renderer serializer so an unencodable bound value renders through its repr rather than raising), `structlog` (`get_logger`/`configure`/`contextvars.merge_contextvars`/`processors.add_log_level`/`processors.CallsiteParameterAdder`/`processors.dict_tracebacks`/`processors.TimeStamper`/`processors.EventRenamer`/`processors.JSONRenderer`/`BytesLoggerFactory`/`make_filtering_bound_logger`, the `FilteringBoundLogger` `debug`/`info`/`warning`/`error` level methods the `LEVEL_METHOD` selectors bind), `opentelemetry-api` (`trace.get_current_span`, `trace.format_trace_id` ENTRYPOINTS [07], `trace.format_span_id` ENTRYPOINTS [08], `propagate.extract` ENTRYPOINTS [11], `propagate.get_global_textmap` ENTRYPOINTS [13], `Context` PUBLIC_TYPES [05], `context.attach`/`context.detach` ENTRYPOINTS [02]/[03], `TextMapPropagator` PUBLIC_TYPES [07], `DefaultGetter` PUBLIC_TYPES [10]), `psutil` (`Process.memory_info` process-metrics ENTRYPOINTS [01] off the one own-process handle, guarding `NoSuchProcess`/`ZombieProcess`/`AccessDenied` PUBLIC_TYPES [02]/[03]/[04] under `contextlib.suppress` so the drained emit never raises on a dead-process race), `hashlib` (stdlib `blake2b` keyed digest for the `hash` classification, cp315-native — never the companion-gated `evidence/identity#IDENTITY` `xxhash` content owner, a distinct concern in a distinct lane). The SDK `LoggerProvider`/`OTLPLogExporter` are consumed by `observability/telemetry#TELEMETRY`, never imported here.
- Growth: a new lifecycle phase is one `Phase` literal plus one `PHASE_LEVEL` row absorbed by the existing `fact` case; a distinct-payload evidence kind is one `Receipt` case plus its `project` match arm plus its `of` discriminant arm (the factory minting it by input shape); a new classified field is one `Redaction` table row keyed by its `Classification`; a new classification transform is one `Classification` member plus one `apply` arm; a new log level is one `LogLevel` literal plus one `LEVEL_METHOD` selector row; a new processor is one entry in the `structlog` chain; a new measured op participates by returning a `ReceiptContributor` under `@receipted`; zero new emit surface.
- Boundary: no AppHost envelope, health status, support-bundle capture, exporter ownership, provider install, or C# receipt minting; the suite classification taxonomy stays AppHost-owned; the redaction `hash` class uses stdlib `hashlib.blake2b` for cp315-native correlation tokens and never imports the companion-gated `xxhash` the `evidence/identity#IDENTITY` content-addressing owner holds (log-field redaction and content identity are distinct concerns in distinct lanes — sharing the hasher would trample that owner and break the cp315 venv resolution); the `propagate.extract` `continue_inbound` runs and the `context.attach`/`context.detach` token-pair `attach` runs are the same aligned `Context` stitch the sibling `execution/lanes#LANE` `traced_kernel` shim runs on the subinterpreter-offload hop, but the two are distinct seams over distinct hops (receipts owns the inbound gRPC W3C extract per the ARCHITECTURE `[WIRE]` seam, lanes owns the intra-process PEP 734 offload stitch), aligned pattern not merged owner; a per-package parallel receipt rail, a stdlib `logging` call outside the structlog bridge, a `str()`-coerced fact map where the `msgspec` renderer carries native types, a bare `Encoder()` whose serializer raises on an unexpected bound value where the `enc_hook=repr` degrades it, three hand-built `emit` dispatch arms where the union's own `project` folds them, a stringly `getattr(log, <level-name>)` where the `LEVEL_METHOD` table dispatches the bound method over the closed `LogLevel` vocabulary, a parallel `of_rejected`/`of_drained` factory family where the one shape-polymorphic `of` discriminates by input, a hand-built `Receipt(rejected=...)`/`Receipt(drained=...)` at a sibling call site where `of` mints it, a flat `***`-only redaction where the classification table discriminates `drop`/`mask`/`hash`, a fused extract-and-activate `continue_inbound` scope a servicer cannot place where the pure extract feeds the rail and the separate `attach` scope wraps the body, a second tracer minted for the inbound extract, and a fresh-root span where the C# parent is on the inbound carrier are the deleted forms.

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
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.json import Encoder
from msgspec.structs import asdict
from opentelemetry import context, propagate, trace
from opentelemetry.context import Context

from rasm.runtime.faults import BoundaryFault
from rasm.runtime.lanes import DRAIN_COLUMNS, DrainReceipt

# --- [TYPES] ----------------------------------------------------------------------------

type Phase = Literal["admitted", "planned", "emitted"]
type LogLevel = Literal["debug", "info", "warning", "error"]
type EventDict = dict[str, object]
type Classification = Literal["drop", "mask", "hash"]

# --- [CONSTANTS] ------------------------------------------------------------------------

PHASE_LEVEL: Final[Map[Phase, LogLevel]] = Map.of_seq(
    [("admitted", "debug"), ("planned", "debug"), ("emitted", "info")]
)

REDACTED: Final[str] = "***"

# the bounded LogLevel vocabulary owns which FilteringBoundLogger level method each case
# emits through — a data table of method selectors keyed by the closed literal, so emit
# resolves the bound method over the vocabulary rather than a stringly getattr(log, level).
LEVEL_METHOD: Final[Map[LogLevel, Callable[[object], Callable[..., None]]]] = Map.of_seq([
    ("debug", lambda log: log.debug),
    ("info", lambda log: log.info),
    ("warning", lambda log: log.warning),
    ("error", lambda log: log.error),
])

# the one renderer/redaction encoder; the enc_hook degrades any value outside the native
# JSON set to its repr so a bound domain object never raises on the hot logging path.
_ENCODE: Final[Callable[[object], bytes]] = Encoder(enc_hook=repr).encode

# own-process handle minted once; the drained point-fact reads RSS off it rather than
# re-minting a Process() per projection. The metrics sibling carries its own handle.
_PROCESS: Final[psutil.Process] = psutil.Process()

PROCESS_FAULTS: Final[tuple[type[psutil.Error], ...]] = (psutil.NoSuchProcess, psutil.ZombieProcess, psutil.AccessDenied)

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Receipt:
    tag: Literal["fact", "rejected", "drained"] = tag()
    fact: tuple[Phase, str, str, dict[str, object]] = case()
    rejected: tuple[str, str, BoundaryFault] = case()
    drained: tuple[str, DrainReceipt] = case()

    @staticmethod
    def of(owner: str, evidence: "tuple[Phase, str, dict[str, object]] | BoundaryFault | DrainReceipt") -> "Receipt":
        match evidence:
            case BoundaryFault() as fault:
                return Receipt(rejected=(owner, fault.facts().get("subject", owner), fault))
            case DrainReceipt() as drain:
                return Receipt(drained=(owner, drain))
            case (phase, subject, facts):
                return Receipt(fact=(phase, owner, subject, facts))

    def project(self) -> tuple[LogLevel, EventDict]:
        match self:
            case Receipt(tag="fact", fact=(phase, owner, subject, facts)):
                return PHASE_LEVEL[phase], {"event": phase, "owner": owner, "subject": subject, **facts}
            case Receipt(tag="rejected", rejected=(owner, subject, fault)):
                return "warning", {"event": "rejected", "owner": owner, "subject": subject, "fault": _fault_node(fault)}
            case Receipt(tag="drained", drained=(owner, drain)):
                columns = asdict(drain)
                return "info", {
                    "event": "drained",
                    "owner": owner,
                    **_rss(),
                    **{column: columns[column] for column in DRAIN_COLUMNS},
                }
            case _ as unreachable:
                assert_never(unreachable)


@runtime_checkable
class ReceiptContributor(Protocol):
    def contribute(self) -> Iterable[Receipt]: ...


class Redaction(Struct, frozen=True):
    classified: Map[str, Classification]
    salt: bytes = b"rasm"

    def apply(self, facts: EventDict) -> EventDict:
        return {
            key: redacted
            for key, value in facts.items()
            for redacted in self._classify(key, value)
        }

    def _classify(self, key: str, value: object) -> tuple[object, ...]:
        match self.classified.try_find(key).default_value("keep"):
            case "drop":
                return ()
            case "mask":
                return (REDACTED,)
            case "hash":
                return (blake2b(_ENCODE(value), key=self.salt, digest_size=8).hexdigest(),)
            case _:
                return (value,)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _fault_node(fault: BoundaryFault) -> EventDict:
    match fault:
        case BoundaryFault(tag="aggregate"):
            return {"tag": "aggregate", "members": [_fault_node(member) for member in fault.aggregate]}
        case _:
            return {"tag": fault.tag}


def _rss() -> EventDict:
    with suppress(*PROCESS_FAULTS):
        return {"rss": _PROCESS.memory_info().rss}
    return {}


def trace_context(_: object, __: str, event: EventDict) -> EventDict:
    ctx = trace.get_current_span().get_span_context()
    if ctx.is_valid:
        event["trace_id"] = trace.format_trace_id(ctx.trace_id)
        event["span_id"] = trace.format_span_id(ctx.span_id)
        event["trace_flags"] = int(ctx.trace_flags)
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
    def emit(source: Receipt | Iterable[Receipt] | ReceiptContributor, redaction: Redaction) -> None:
        log = structlog.get_logger()
        stream = source.contribute() if isinstance(source, ReceiptContributor) else (source,) if isinstance(source, Receipt) else source
        for receipt in stream:
            level, event = receipt.project()
            LEVEL_METHOD[level](log)(event.pop("event"), **redaction.apply(event))

    @staticmethod
    def continue_inbound(carrier: Mapping[str, str]) -> Context:
        return propagate.extract(dict(carrier))

    @staticmethod
    @contextmanager
    def attach(parent: Context) -> Iterator[Context]:
        token = context.attach(parent)
        try:
            yield parent
        finally:
            context.detach(token)


def receipted[**P](
    redaction: Redaction,
) -> Callable[[Callable[P, ReceiptContributor] | Callable[P, Awaitable[ReceiptContributor]]], Callable[P, object]]:
    def wrap(operation: Callable[P, object]) -> Callable[P, object]:
        @wraps(operation)
        async def emit_async(*args: P.args, **kwargs: P.kwargs) -> ReceiptContributor:
            contributor = await operation(*args, **kwargs)
            Signals.emit(contributor, redaction)
            return contributor

        @wraps(operation)
        def emit_sync(*args: P.args, **kwargs: P.kwargs) -> ReceiptContributor:
            contributor = operation(*args, **kwargs)
            Signals.emit(contributor, redaction)
            return contributor

        return emit_async if iscoroutinefunction(operation) else emit_sync

    return wrap
```

## [03]-[RESEARCH]

- [OTLP_LOG_EGRESS]: reflection-confirmed — the `opentelemetry-sdk` `_logs.LogRecordProcessor`/`_logs.export.BatchLogRecordProcessor` log-egress wiring (no native `structlog` OTLP export) pairs with the `opentelemetry-exporter-otlp-proto-http` `OTLPLogExporter(endpoint, headers, timeout, compression, ...)` constructor, and `structlog.contextvars.merge_contextvars` is the bound-context processor. The provider/processor/exporter NOW INSTALL once at `observability/telemetry#TELEMETRY` (the composition-root install owner this campaign lands); this owner reads the installed `LoggerProvider` and wires no private `LogRecordProcessor`/`OTLPLogExporter`. The metric egress is the sibling `observability/metrics#METRIC` reading the installed `MeterProvider` over the same shared OTLP exporter family and one `Resource`. The log-egress wiring is settled.
- [PROCESSOR_CHAIN]: reflection-confirmed against `libs/python/.api/structlog.md` `[03]-[ENTRYPOINTS]`/`[04]-[IMPLEMENTATION_LAW]` — the production chain orders `contextvars.merge_contextvars` (ENTRYPOINTS contextvars [01], first so the ambient `Correlation`/`RuntimeContext` is present downstream) → `processors.add_log_level` (PUBLIC_TYPES processors [03]) → the custom `trace_context` reading `trace.get_current_span().get_span_context()` for `trace_id`/`span_id`/`trace_flags` → `processors.CallsiteParameterAdder` (PUBLIC_TYPES processors [04], placed after the filtering wrapper admits per the `CallsiteParameterAdder` cost law) → `processors.dict_tracebacks` (PUBLIC_TYPES processors [12], the preconfigured `ExceptionRenderer(ExceptionDictTransformer())` that renders a bound `BoundaryFault`/`exc_info` to a JSON-safe structured stack rather than the rejected `str(exc)` form) → `processors.TimeStamper` → `processors.EventRenamer(to="body")` (PUBLIC_TYPES processors [06], aligning the `event` key to the OTLP log `body` field the bridge reads) → `processors.JSONRenderer(serializer=...)` (PUBLIC_TYPES processors [16]) renderer-last. The renderer `serializer` is the `msgspec` `json.Encoder(enc_hook=repr).encode` per the structlog `STACKS_WITH` "msgspec fast JSON" row, paired with `BytesLoggerFactory` (PUBLIC_TYPES [06]) so the encoder's `bytes` output flows to the sink with no stdlib `json` re-encode and native ints/floats reach the line without `str()` coercion, the `enc_hook=repr` (stateful-codec ENTRYPOINTS [01]) degrading any value outside the native JSON set to its repr so a renderer raise never kills the emit, and `make_filtering_bound_logger(20)` (ENTRYPOINTS [04]) is the `wrapper_class` compiling sub-`INFO` levels to no-ops at method resolution. The bounded `LogLevel` keys the `LEVEL_METHOD` selector `Map` that binds the `FilteringBoundLogger.debug`/`info`/`warning`/`error` method per emit (ENTRYPOINTS bound-method [02]) rather than a stringly `getattr(log, level)`. The chain and serializer spellings are settled.
- [REDACTION_CLASSIFICATION]: the field-redaction policy is a `Classification`-keyed `expression` `Map[str, Classification]` table discriminating `drop` (field removed before the line), `mask` (fixed `***` sentinel), and `hash` (a truncated keyed `hashlib.blake2b(value, key=salt, digest_size=8).hexdigest()` correlation token) — a `drop`/`mask`/`hash` member added on the `Classification` literal plus one `apply` arm, never a parallel allow/deny set. `blake2b` is the stdlib keyed hasher (cp315-native, no wheel gate), chosen deliberately over the `evidence/identity#IDENTITY` `xxhash` content owner: log-field redaction and content-addressing are distinct concerns in distinct lanes, and `xxhash` is companion-gated `<3.15` (absent from the default cp315 venv per `.api/xxhash.md`), so importing it here would both trample that owner and break cp315 resolution. The correlation-preserving `hash` class lets two redacted log lines carrying the same secret correlate by token without leaking the value, which a flat `***` mask cannot. The classification taxonomy is settled.
- [RECEIPT_PROJECTION]: the union owns its own `(LogLevel, EventDict)` projection through one total `Receipt.project` `match`, so `Signals.emit` is a renderer-agnostic fold over `project` polymorphic across `Receipt | Iterable[Receipt] | ReceiptContributor` rather than three hand-built dispatch arms — the `fact` case keys its level off the `PHASE_LEVEL` `Map` (`admitted`/`planned`→`debug`, `emitted`→`info`) with no phase branch, `rejected` folds the `BoundaryFault` to a structured `fault` node spreading the `aggregate` members, and `drained` projects the `DRAIN_COLUMNS`-indexed `msgspec.structs.asdict` outcome map plus the `psutil` RSS fact; the projected `LogLevel` selects the bound `structlog` method through the `LEVEL_METHOD` selector `Map` keyed on the closed literal rather than a stringly `getattr(log, level)`, and `Receipt.of` mints each case by `match`ing its input evidence shape (`(Phase, subject, facts)`→`fact`, `BoundaryFault`→`rejected`, `DrainReceipt`→`drained`) so every sibling mints through one factory rather than a hand-built case constructor or a parallel `of_*` family. The renderer/redaction `Encoder` carries `enc_hook=repr` so an unexpected bound value degrades to its repr rather than raising and killing the emit. `ReceiptContributor.contribute` yields an `Iterable[Receipt]` so a multi-phase contributor (a `GraduationReceipt` carrying admit+emit) streams its facts rather than forcing one receipt per call, and `@receipted` is the AOP aspect wrapping a contributor-returning operation (sync via the function, async via the awaited coroutine, dispatched on `iscoroutinefunction`) to harvest and emit the stream on exit through the same fold — receipt emission a decorator rail, not an inline call threaded through each measured kernel. The projection/contributor/aspect shapes are settled.
- [TRACE_INBOUND_EXTRACT]: reflection-confirmed against the branch `libs/python/.api/opentelemetry-api.md` — `propagate.extract(carrier, context)` (ENTRYPOINTS [11]) decodes the W3C `traceparent`/`tracestate` carrier into a `Context` (PUBLIC_TYPES [05]) resolved through `propagate.get_global_textmap()` (ENTRYPOINTS [13], the composite `observability/telemetry#TELEMETRY` installs via `set_global_textmap` ENTRYPOINTS [14]); the carrier is a plain `dict[str, str]` per the `DefaultGetter` (PUBLIC_TYPES [10]) contract, and `context.attach`/`context.detach` (ENTRYPOINTS [02]/[03]) are the token-paired scoped activation. The seam is one polymorphic pair, not a fused scope: `Signals.continue_inbound` is the pure `propagate.extract` returning the decoded `Context` as a value, and `Signals.attach` is the separate `@contextmanager` that `context.attach`es it, yields, and `context.detach`es in `finally`. The split is load-bearing because the `transport/serve#SERVE` `ServerHost.inbound` must thread the extracted `Context` through its `RuntimeRail` admission pair (`(Context, RuntimeContext)`) outside the `boundary("wire", ...)` fence — a non-failing parent resolve, never a parse fault — while `ServerHost.dispatch` opens `attach(parent)` around exactly the decode/handler/encode body after the admission rail resolves `Ok`; a single fused extract-and-activate scope cannot be placed at that split. The activation `context.attach`/`context.detach` token-pair is the same `Context` stitch the sibling `execution/lanes#LANE` `traced_kernel` runs on the offload hop. `ServerHost.inbound` reads the carrier off `grpc.aio.ServicerContext.invocation_metadata()` projected to `dict[str, str]` and calls `continue_inbound` before the servicer body opens its span scope. Producer is `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` (the `TraceContextPropagator`/`BaggagePropagator` composite registered as `Propagators.DefaultTextMapPropagator`); the extract resolving through the default no-op propagator before the install is the mechanical reason this sequences after `observability/telemetry#TELEMETRY`. Realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` Python leg. Spellings settled.
