# [PY_RUNTIME_TELEMETRY]

`Telemetry` is the one composition-root install owner for OTLP signal egress — every other observability surface assumes the providers it registers and constructs none. Install custody is two-tier: per-composition receipts key by the receipts-owned `ScopeKey`, while one generation reservation serializes the process-global provider mint and drain. Admission's `emit_otel` cell gates every mint, so a silent profile keeps the `opentelemetry-api` no-ops and opens no batch thread.

One `SIGNAL_SPECS` fold owns the batched span/log pair; the meter stands beside it — `metric_readers` and `views` are construction-only — built FIRST and threaded as `meter_provider=` into both exporters. `PROPAGATORS` registers one W3C composite as the process global, the sole inbound-context reader the `observability/receipts#RECEIPT` extract, the `transport/serve#CAPABILITY_INVOKE` interceptors, and the `execution/admission#CONTEXT` correlation adoption each resolve. `_sampler` honors the parent it decodes, so the Python leg continues `ONE_DISTRIBUTED_TRACE` rather than rooting a second.

## [01]-[INDEX]

- [02]-[TELEMETRY]: scope-keyed install custody over the generation-reserved pipeline mint, the `diagnostic_read` backend-free read plane, the `SIGNAL_SPECS` fold, the `EGRESS` transport table and `SIGNAL_PROFILE` gate, the `ViewRow` projection and ratio sampler, the detector-merged `RUNTIME_RESOURCE`, and the per-signal railed host drain.
- [03]-[CONFORMANCE]: the `CONFORMANCE` cell table proving one row per legislated key at import, the three-disposition `ConformanceRow` family, and the `ConformanceDocument` whose canonical bytes are this branch's contribution to the corpus `TELEMETRY_CONVENTION` comparison.

## [02]-[TELEMETRY]

- Owner: a `SignalSpec` row owns the `(provider, attach)` wiring pair and the global-register cell, so the span/log install is one fold and a new batch-processor signal is one row; the exporter lives apart on the `EGRESS` table — one `EgressTransport`-keyed row-map per transport holding the signal-keyed factory triple, so transport is a policy value the profile row carries and never a sibling install path — and the `_batched` kernel lifts the bound `add_*` method and processor class over one shared queue-and-deadline geometry, never two sibling closures. `InstallReservation` carries owner thread, scope, and generation across unlocked construction or drain; `PendingInstall` carries the completed provider graph and the environment pins that install introduced into the guarded commit, so every fault path gives back the same pair; `InstalledProviders` holds the trio as one atomically-assigned `Drainable`-typed carrier, never parallel `| None` provider slots.
- Law: W3C trace-context is the identity wire — a 16-byte trace id crossing every hop of one trace beside the 8-byte span id naming one hop's parent — and `_commit` publishes the `PROPAGATORS` composite as the one global textmap, so every ingress adopts the decoded parent and mints a root only where the carrier holds none. `evidence/clock#CLOCK` stamps its two-half cell on the same carrier as an independent read projecting onto spans as the `rasm.hlc` attribute, so no causal cell occupies a trace or span id slot.
- Entry: `Telemetry.install(ctx, endpoint, ...)` takes the admitted `RuntimeContext`, so the fork-safety gate reads `ctx.shape.topology` and the emission gate reads `ctx.policy.emit_otel` — the preset name rides through as the geometry key and the receipt column alone. Silent scopes cache a `SILENT` receipt, so a later re-admission under the same composition still no-ops, and OTel under a silent profile costs exactly the API no-op providers. `resource=` and `signal_profile=` are injection seams — the profiles-owned job envelope hands in its hand-built job identity and high-interval geometry, the workers boot hands in its worker identity and small-queue worker geometry, every daemon path resolves `RUNTIME_RESOURCE` and the profile-keyed row — the injected row's `transport` clamps to HTTP outside `GRPC_ELIGIBLE` with the receipt carrying the effective geometry so the fork-hazard fence is visible evidence, never a silent divergence, and `ship=` is the same `LogShip` value the `LogPipeline.configure` call carries, read through that owner's `SHIP_OTLP` row at both ends so the provider half and the chain half of the log egress cannot diverge. `Telemetry.snapshot()` is the one backend-free measurement read, answering `Nothing` under a profile that declined `diagnostic_read` and the collected tree under one that armed it. `install` reserves custody, builds outside the condition, and commits only the matching owner-generation; competing scopes wait, and same-thread recursion refuses. `shutdown` detaches custody before its unlocked `force_flush`-then-`shutdown` fold — each provider runs through `boundary(TELEMETRY_STOP, ..., catch=_DRAIN_RAISES)` under `traversed(ACCUMULATE)`, so a wedged exporter never short-circuits the remaining flushes — then releases the matching generation; a `SILENT`/`ADOPTED` scope's shutdown retires only its own receipt. One process mints the pipeline at most once — the OTel `set_*_provider` globals are set-once with no unset — so a post-drain emitting install refuses loudly, commit publishes only after every fallible construction lands, and custody releases on every commit exit so a fault never wedges a waiting install. Construction owns what it allocates: a raise below the meter unwinds that provider's reader and gives back exactly the `SDK_ENV` keys this install introduced, and a raise inside `_commit` unwinds the same pair off the carrier that holds them, so no refused attempt leaves an exporting reader for an unpublished pipeline or a dead profile's geometry standing as the authority the retry reads. `SignalProfile` admits its own sampling ratio at construction, so `_sampler` reads a settled unit fraction rather than collapsing an out-of-range value onto full sampling.
- Auto: `RUNTIME_RESOURCE` carries the `service.namespace`/`service.name`/`service.instance.id` uniqueness triple and orders the env detector last, so a deployment-time `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` override wins the merge without a code change and the three signals join one identity — a bare `{"service.name": ...}` literal degrades to `unknown_service`. Exporters' own `create_exporter_metrics` counters land on the one threaded meter and the batch processors' `create_processor_metrics` counters on the proxy meter that upgrades at commit, never a parallel pipeline — `SDK_ENV` arms both, since an unarmed flag hands every one of them a no-op recorder. Serve-leg `SERVER` span emission stays `transport/serve#SERVE`-owned: its defaulted `tracer_provider` resolves the global this install registers, and a global `GrpcAioInstrumentorServer` double-patches the leg — two overlapping `SERVER` spans per RPC, the serve health-check filter bypassed — so no instrumentor activates here.
- Growth: a new batch-processor signal is one `Signal` member, one `SignalSpec` row, and one factory cell per `EGRESS` transport row, reaching the install and drain folds with no entrypoint edit; a construction-only-reader signal seeds the carrier beside the fold and registers through the body's matching `set_*_provider`, since it owns no `attach` step; a new egress transport is one `EgressTransport` member with one `EGRESS` row-map and its `GRPC_ELIGIBLE`-style eligibility fact; a new per-profile geometry, policy, or transport knob is one `SignalProfile` column with its `SIGNAL_PROFILE` values — the sampling ratio, the cardinality ceiling, the explicit-bucket roster, and the diagnostic-read arm each landed exactly that way; a new metric-stream shaping axis is one `ViewRow` field at the metrics owner and one argument in `_viewed`, with no call site edited; a new composition is one `ScopeKey` value threaded through `install`/`shutdown`'s `scope` keyword; a new propagator format one `PROPAGATORS` row folded by the one `set_global_textmap`; a new resource detector one entry in the `get_aggregated_resources` list; a new per-span cardinality cap one `SPAN_LIMITS` argument; a new log-record cap one `RecordCaps` field beside its `SDK_ENV` row; a new environment-only SDK pin one `SDK_ENV` row; a new promoted signal dimension one `PROMOTED_BAGGAGE` member reaching span and log promotion through the one predicate.
- Boundary: no second `TracerProvider`/`MeterProvider`/`LoggerProvider`, no AppHost telemetry envelope, sampler-floor ownership, or product export, and no SDK import outside this owner; every provider disables its private atexit hook because `Telemetry.shutdown` owns the sole flush-and-shutdown rail — `_commit` registers the ONE `atexit` final-drain leg instead, because `atexit` runs after `sys.excepthook` and a drain riding only the composition root's unwind would retire the `LoggerProvider` before the logging owner's uncaught-raise door emits its wire line; the diagnostic reader is a READ plane and never an egress — it mounts beside the exporting reader with its own aggregation storage so it drains nothing that reader owes, it stays construction-only like every other reader, and its custody releases with the providers at shutdown so a drained process answers `Nothing` rather than a tree the retired provider no longer feeds. `_logs` and gRPC exporter tiers ride module-scope `lazy from`, reified only when an emitting install selects them — the baggage-promotion package's own `__init__` imports `_logs`, so its pair defers with it — never at module import. Histogram wire shape is ruled here: `WIRE_AGGREGATION` sets the base2-exponential default at the metric exporter's `preferred_aggregation`, matching the estate bucket algebra across languages, and `WIRE_TEMPORALITY` pins every sum and histogram family beside it. Both maps key the SDK instrument families rather than their API base classes — the reader matches a preference key by identity and refuses anything else, so an API-spelled key is an install-killing raise and an omitted family falls to the exporter's environment-seeded table. `views=` is that gate's landed half rather than a promise: `_viewed` projects each metrics-owned `ViewRow` into an SDK `View` at construction — the allow-list bounding the stream's attribute keys, the reservoir selected per row — and a row naming an instrument in the profile's `explicit_buckets` re-selects `ExplicitBucketHistogramAggregation` over that instrument's own API-level advisory. Every other row leaves `aggregation` unset so the view resolves `DefaultAggregation` and defers to the reader's preference, which is what keeps the base2-exponential wire default intact under a per-instrument view set. Views are construction-only, so this is the one place the rows can land; SDK view, aggregation, and reservoir types stay this owner's alone, keeping the metrics tier free of every SDK import. `SPAN_LIMITS` rides the trace row's provider construction, capping span, event, and link attributes and value lengths so a hostile caller-shaped payload never balloons the batch queue's memory envelope; `RecordCaps` is that ceiling's log half, stamped through `SDK_ENV` at the head of the pipeline because the SDK exposes log-record limits on no constructor and a key landing after the first record caps nothing already queued. `SignalProfile.export_timeout_ms` reaches both batch processors and the metric reader, and `max_export_batch_size` reaches BOTH metric exporter rows — the reader hands its whole collection to one export call, so a row leaving that split open ships one unbounded request where its sibling transport ships a bounded stream; exporter transport timeout remains the terminal wire deadline. Export-loss accounting is armed rather than assumed: `SDK_ENV` pins the SDK's internal-metrics flag, without which every processor and exporter builds a no-op recorder and a queue-full drop reaches no series at all. Span- and log-level dimension promotion is this install's seam alone — the `PROMOTED_BAGGAGE`-filtered `BaggageSpanProcessor`/`BaggageLogProcessor` pair registered at provider construction — so no producer page folds a promoted dimension onto spans and the metric-side fold stays the metrics owner's. Both allow-lists this owner projects roster by KEY rather than by presence — that promotion set and every `ViewRow.keys` set `_viewed` lands as `attribute_keys` — so a sometimes-absent dimension stays rostered whatever a given context carries, an unrostered key strips from exactly the entries that do carry it, and an entry missing a rostered key reads as the untagged whole rather than as a stamped placeholder. That posture governs every sometimes-absent dimension crossing this wire, `rasm.tenant` among them and never the tenant alone.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import atexit
from collections.abc import Callable, Iterable, Mapping, Sequence
from enum import StrEnum
from functools import partial
from os import environ
from threading import Condition, RLock, get_ident
from typing import ClassVar, Final, Literal, Protocol, assert_never
from uuid import uuid4

from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from opentelemetry import metrics, propagate, trace
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.exporter.otlp.proto.http import Compression
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.metrics import MeterProvider as ApiMeterProvider
from opentelemetry.propagators.composite import CompositePropagator
from opentelemetry.propagators.textmap import TextMapPropagator
from opentelemetry.resource.detector.containerid import ContainerResourceDetector
from opentelemetry.sdk.metrics import (
    AlignedHistogramBucketExemplarReservoir,
    Counter,
    ExemplarReservoir,
    Histogram,
    MeterProvider,
    ObservableCounter,
    ObservableUpDownCounter,
    SimpleFixedSizeExemplarReservoir,
    TraceBasedExemplarFilter,
    UpDownCounter,
)
from opentelemetry.sdk.environment_variables import (
    OTEL_ATTRIBUTE_COUNT_LIMIT,
    OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT,
    OTEL_LOGRECORD_ATTRIBUTE_COUNT_LIMIT,
    OTEL_LOGRECORD_ATTRIBUTE_VALUE_LENGTH_LIMIT,
    OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED,
)
from opentelemetry.sdk.metrics.export import AggregationTemporality, InMemoryMetricReader, MetricExporter, MetricsData, PeriodicExportingMetricReader
from opentelemetry.sdk.metrics.view import Aggregation, ExplicitBucketHistogramAggregation, ExponentialBucketHistogramAggregation, View
from opentelemetry.sdk.resources import (
    SERVICE_INSTANCE_ID,
    SERVICE_NAME,
    SERVICE_NAMESPACE,
    SERVICE_VERSION,
    OsResourceDetector,
    OTELResourceDetector,
    ProcessResourceDetector,
    Resource,
    ResourceDetector,
    get_aggregated_resources,
)
from opentelemetry.sdk.trace import SpanLimits, TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor, SpanExporter
from opentelemetry.sdk.trace.sampling import ALWAYS_ON, ParentBased, Sampler, TraceIdRatioBased
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator

from rasm.runtime.admission import RuntimeContext, RuntimeProfile, Topology
from rasm.runtime.faults import SCHEMA_URL, SCOPES, SCOPE_VERSION, TELEMETRY_STOP, Catch, Disposition, RuntimeRail, Scope, boundary, traversed
from rasm.runtime.logging import SHIP_OTLP, LogShip
from rasm.runtime.metrics import DOMAINS, INSTRUMENTS, OVERFLOW_KEY, TENANT_BAGGAGE, TENANT_BUDGET, Dimension, ViewRow, views
from rasm.runtime.receipts import DEFAULT_SCOPE, ENCODE, ScopeKey

lazy import grpc  # channel substrate + grpc.Compression; only a selected GRPC row reifies it
lazy from opentelemetry import _logs  # logs tier stays cold: only an install whose ship row arms the wire reifies it
lazy from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter as GrpcLogExporter  # gRPC tier defers with grpc
lazy from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter as GrpcMetricExporter
lazy from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter as GrpcSpanExporter
lazy from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter
lazy from opentelemetry.processor.baggage import BaggageLogProcessor, BaggageSpanProcessor  # package __init__ imports the _logs tier, so the pair defers to install time
lazy from opentelemetry.sdk._logs import LoggerProvider
lazy from opentelemetry.sdk._logs.export import BatchLogRecordProcessor, LogRecordExporter

# --- [TYPES] ----------------------------------------------------------------------------


class Signal(StrEnum):
    TRACE = "traces"
    METRIC = "metrics"
    LOG = "logs"


class EgressTransport(StrEnum):
    HTTP = "http"
    GRPC = "grpc"


# INSTALLED minted the process pipeline; SILENT kept the no-ops; REENTRANT is a same-scope re-install returning its cached
# receipt; ADOPTED is a second composition arriving after the pipeline exists — its receipt records the standing custody.
class InstallOutcome(StrEnum):
    INSTALLED = "installed"
    SILENT = "silent"
    REENTRANT = "reentrant"
    ADOPTED = "adopted"


class Drainable(Protocol):
    def force_flush(self, timeout_millis: int = ...) -> bool: ...
    def shutdown(self) -> None: ...


type SignalExporter = SpanExporter | LogRecordExporter
type WireExporter = SignalExporter | MetricExporter
# header carriers stay `dict` rather than `Mapping`: every OTLP exporter on both transports declares `dict[str, str]`,
# and the gRPC tier additionally admits a pair sequence and a header string this seam never mints.
type ExporterFactory = Callable[[str, dict[str, str] | None, "SignalProfile", ApiMeterProvider | None], WireExporter]
type ProviderFactory = Callable[[Resource, Sampler], Drainable]
type ProcessorAttach = Callable[[Drainable, SignalExporter, "SignalProfile"], None]
# `View`'s reservoir slot takes a factory of the stream's aggregation class returning the builder the SDK then calls
# with that stream's own kwargs — `boundaries=` from an explicit-bucket aggregation, `size=` from an exponential one,
# neither from a sum or last-value stream. Both spellings stay structural: the factory's argument is the SDK's private
# `_Aggregation` subclass, so naming it would import an internal type for a fact the `ViewRow` already carries.
type ReservoirBuilder = Callable[..., ExemplarReservoir]
type ReservoirFactory = Callable[[type], ReservoirBuilder]

# --- [CONSTANTS] ------------------------------------------------------------------------

EXPORT_TIMEOUT_MS: Final[int] = 10_000
EXPORT_TIMEOUT_S: Final[float] = EXPORT_TIMEOUT_MS / 1000.0

# the drain fence's declared raise set, and it names `_drained`'s OWN total surface rather than the SDK's, because the
# three providers publish no typed refusal to name: the trace and log legs swallow every exporter raise inside the
# batch processor and answer `False`, and `MeterProvider.force_flush`/`shutdown` aggregate their readers' failures into
# a BARE `Exception` (`.api/opentelemetry-sdk.md`). `_drained` is therefore the stated drain fold that absorbs that
# unnameable surface once and re-raises TOTALLY — a lone unflushed provider as the `TimeoutError` the `CLASSIFY`
# deadline row reads, every provider raise as one `ExceptionGroup` — so this fence narrows to two named classes and no
# consumer of the drain rail spells `catch=Exception`.
_DRAIN_RAISES: Final[Catch] = (TimeoutError, ExceptionGroup)

# resource uniqueness triple: namespace + name + instance id, the service name off the faults-owned SCOPES row — never a second literal.
# Version and semconv pin arrive as the faults-owned instrumentation-scope pair, so resource, meter, tracer, and
# logger carry one version and one schema url, and a semconv bump lands at exactly one constant.
NAMESPACE: Final[str] = "rasm"

# Detector chain and minted identity each stand as their OWN named value rather than inline arguments, because the
# `[03]-[CONFORMANCE]` resource rows project off exactly these two and an argument list projects off nothing — a row
# restating a chain it cannot read is the drift that form forecloses. Env detector rides LAST so a deployment-time
# `OTEL_RESOURCE_ATTRIBUTES`/`OTEL_SERVICE_NAME` override wins the merge.
RESOURCE_DETECTORS: Final[tuple[ResourceDetector, ...]] = (
    ProcessResourceDetector(),
    OsResourceDetector(),
    ContainerResourceDetector(),
    OTELResourceDetector(),
)

# The uniqueness triple beside the version column. Only its KEY set reaches a conformance row: `service.instance.id`
# is a per-process mint, so a document carrying the values would digest one process rather than one branch.
RUNTIME_IDENTITY: Final[Mapping[str, str]] = {
    SERVICE_NAMESPACE: NAMESPACE,
    SERVICE_NAME: SCOPES[Scope.SERVICE],
    SERVICE_VERSION: SCOPE_VERSION,
    SERVICE_INSTANCE_ID: uuid4().hex,
}

RUNTIME_RESOURCE: Final[Resource] = get_aggregated_resources(
    list(RESOURCE_DETECTORS),
    initial_resource=Resource.create(dict(RUNTIME_IDENTITY), schema_url=SCHEMA_URL),
)

# exemplar slots per fixed-size reservoir. It governs the sum and last-value streams, where the SDK's own default
# reservoir holds ONE sample and a single slot per export window is too thin to click through; an exponential-
# histogram stream re-partials the builder with its bucket-derived size and an explicit-bucket stream takes the
# aligned reservoir instead, so neither reads this value. A handful of slots costs a fixed span-context tuple per
# stream, and the sampler already thins the population upstream, so the size is structural, never a deployment knob.
EXEMPLAR_RESERVOIR_SIZE: Final[int] = 4

# fork-hazard fence as data on the topology axis: a persistent gRPC channel never survives fork(), so only the
# non-forking sidecar daemon shape may carry the GRPC transport value; install clamps every other injected row to HTTP
# with the receipt as witness, and a new non-forking shape earns the transport by one member here.
GRPC_ELIGIBLE: Final[frozenset[Topology]] = frozenset({Topology.SIDECAR})

# conformance pair every metric exporter row carries, keyed on the SDK instrument FAMILIES — `opentelemetry.sdk.metrics`
# and `opentelemetry.metrics` each export a `Counter` and each spells its synchronous gauge `_Gauge`, and the reader
# matches a preference key by identity against the SDK family and RAISES on every other class, so an API-spelled key
# kills each emitting install at reader construction rather than drifting quietly. The exporter seeds its own family
# table from `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE` — CUMULATIVE absent the variable — then merges this
# map over it, so every family the estate rules carries a row here and an omitted one is left to a deployment
# environment variable, the drift this pin forecloses. Rows transcribe the specification's DELTA preference whole:
# both monotonic sum families and the histogram ride DELTA, both non-monotonic sum families CUMULATIVE because a delta
# of a rise-and-fall sum reconstructs no level without a known origin. Neither gauge family rows — a last-value stream
# exports as a Gauge point, which carries no temporality to pin.
WIRE_TEMPORALITY: Final[Mapping[type, AggregationTemporality]] = {
    Counter: AggregationTemporality.DELTA,
    UpDownCounter: AggregationTemporality.CUMULATIVE,
    Histogram: AggregationTemporality.DELTA,
    ObservableCounter: AggregationTemporality.DELTA,
    ObservableUpDownCounter: AggregationTemporality.CUMULATIVE,
}

# histogram wire shape on the same SDK-family key axis: the exporter's own seed reads
# `OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION` and falls to explicit buckets, so this row is what makes
# base2-exponential the wire default and holds the C# and TS bucket algebra identical with no translation re-bucketing.
WIRE_AGGREGATION: Final[Mapping[type, Aggregation]] = {Histogram: ExponentialBucketHistogramAggregation()}

# per-span cardinality caps: the runtime ingests caller-shaped payloads, so a hostile emitter's attribute, event, or
# link fan is bounded at provider construction and the value-length cap bounds the string blowup a batch queue buffers.
SPAN_LIMITS: Final[SpanLimits] = SpanLimits(
    max_attributes=64,
    max_events=128,
    max_links=32,
    max_event_attributes=64,
    max_link_attributes=64,
    max_attribute_length=4096,
    max_span_attribute_length=4096,
)

# Every SDK behaviour this install pins through the environment rather than a constructor, folded once before the
# first provider, record, or processor exists. Each key spells the distribution's OWN exported constant, so a
# renamed variable breaks at import instead of degrading to a pin nothing reads. A key the deployment already set
# stays authoritative and this fold never introduces it, matching how the env resource detector outranks the
# code-side identity — which is also what makes the failed-construction unwind exact rather than destructive.
# Log-record caps reach the SDK through the environment ALONE — `ReadWriteLogRecord.limits` default-factories a
# fresh `LogRecordLimits` per record from those four keys, and neither `LoggerProvider` nor `Logger.emit` nor the
# record constructor carries a limits argument — so the trace leg's `SPAN_LIMITS` constructor slot has no log
# counterpart. `observability/logging#PIPELINE` holds its chain bound as the policy FLOOR both renders cross, while
# these keys stand as the deployment CEILING behind the wire.
# Export-loss accounting is the second pin and the whole reason a dropped batch is evidence: every batch processor,
# simple processor, and OTLP exporter builds its instrument set through `create_processor_metrics`/
# `create_exporter_metrics`, which hand back a NO-OP recorder unless this flag reads true — so an unarmed process
# reports a queue-full drop and a rejected export as one stdlib warning the SDK's own `DuplicateFilter` collapses
# on repeat, and the loss reaches no series at all. Armed, the counters mount on the API proxy meter this fold
# already relies on and upgrade in place at the `set_meter_provider` commit, so the accounting lands on the one
# installed reader with no second pipeline.
# Exemplar filtering stays OUT of this table by contrast: a constructor slot carries it, so the pin holds against
# every deployment value rather than yielding to one, which is what a conformance row demands and a geometry knob
# does not.
SDK_ENV: Final[Map[str, Callable[["SignalProfile"], str]]] = Map.of_seq([
    (OTEL_ATTRIBUTE_COUNT_LIMIT, lambda geometry: str(geometry.record_caps.attribute_count)),
    (OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT, lambda geometry: str(geometry.record_caps.attribute_length)),
    (OTEL_LOGRECORD_ATTRIBUTE_COUNT_LIMIT, lambda geometry: str(geometry.record_caps.record_attribute_count)),
    (OTEL_LOGRECORD_ATTRIBUTE_VALUE_LENGTH_LIMIT, lambda geometry: str(geometry.record_caps.record_attribute_length)),
    (OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED, lambda _geometry: "true"),
])

# W3C composite the install folds into one `set_global_textmap`: trace-context beside baggage, the one inbound reader
# every extract seam resolves, so a new wire format is one row here and never a second propagator install.
PROPAGATORS: Final[Sequence[TextMapPropagator]] = (TraceContextTextMapPropagator(), W3CBaggagePropagator())

# signal-side dimension promotion: the metrics-owned rasm.tenant baggage entry lands as a span attribute at
# start and a log attribute at emit, so tenant rides every signal exactly as it rides the metric dimension;
# membership stays closed — an ALLOW_ALL predicate would stamp arbitrary caller baggage past SPAN_LIMITS intent.
PROMOTED_BAGGAGE: Final[frozenset[str]] = frozenset({TENANT_BAGGAGE})

# --- [MODELS] ---------------------------------------------------------------------------


# Map key IS the profile: no `profile` field rides the row (the admission `ProfilePolicy`
# no-drift law), `InstallReceipt.profile` carrying the resolved key separately. `transport`
# defaults HTTP on every standing row; the GRPC value enters only via the injected seam.
# `export_timeout_ms` defaults to the one conformance deadline, so every constructor — standing
# row, job envelope, worker boot — inherits it and a lane overrides only when its geometry differs.
# Three trailing columns carry signal POLICY rather than buffer geometry: head-sampling ratio for the root
# sampler, tenant-value ceiling for the metrics owner to enforce, and instrument names a deployment re-arms
# onto their own bucket advisory — each a value the injected seam overrides per lane.
# log-record ceiling as POLICY beside the trace leg's `SPAN_LIMITS`, so a profile carrying a hostile-payload posture
# states it for both signals rather than for spans alone. Values mirror `SPAN_LIMITS` because one caller-shaped payload
# reaches a span attribute and a log attribute through the same emit.
class RecordCaps(Struct, frozen=True):
    attribute_count: int = 64
    attribute_length: int = 4096
    record_attribute_count: int = 64
    record_attribute_length: int = 4096


class SignalProfile(Struct, frozen=True):
    export_interval_ms: int
    schedule_delay_ms: int
    max_queue_size: int
    max_export_batch_size: int
    compression: Compression
    export_timeout_ms: int = EXPORT_TIMEOUT_MS
    transport: EgressTransport = EgressTransport.HTTP
    sample_ratio: float = 1.0
    cardinality_budget: int = TENANT_BUDGET
    explicit_buckets: frozenset[str] = frozenset()
    record_caps: RecordCaps = RecordCaps()
    diagnostic_read: bool = False

    def __post_init__(self) -> None:
        # The head-sampling ratio admits ONCE, here, on every construction route msgspec ships — standing row,
        # injected lane geometry, decode, and `replace` alike. `_sampler` reads it as a settled unit fraction and
        # cannot re-derive that: a value above one collapses onto `ALWAYS_ON` and silently samples every root a
        # deployment asked to thin, a negative or non-finite one reaches `TraceIdRatioBased` as a bound nothing
        # names. The comparison closes the non-finite domain too — `nan` fails both halves and `inf` fails the top.
        if not 0.0 <= self.sample_ratio <= 1.0:
            raise ValueError(f"sample_ratio {self.sample_ratio} outside the unit interval")


# exporter construction lives on the EGRESS table, keyed by the profile row's transport — a spec row wires and registers.
class SignalSpec(Struct, frozen=True):
    signal: Signal
    provider: ProviderFactory
    attach: ProcessorAttach
    register: Callable[[Drainable], None]


# `ship` rides the receipt because it DECIDES the installed signal set — the LOG row arms off `SHIP_OTLP[ship]` — so a
# receipt carrying geometry alone cannot answer which signals this process exports, which the bundle capsule, the
# worker boot capture, and the `[03]-[CONFORMANCE]` egress rows each read off exactly this value.
class InstallReceipt(Struct, frozen=True):
    profile: RuntimeProfile
    outcome: InstallOutcome
    endpoint: str
    signal_profile: SignalProfile
    ship: LogShip


class InstallReservation(Struct, frozen=True):
    generation: int
    scope: ScopeKey
    thread: int


class InstalledProviders(Struct, frozen=True):
    by_signal: Map[Signal, Drainable]

    def flush(self) -> RuntimeRail[Block[Signal]]:
        # drain the meter LAST — the span/log force_flush drives their exporters' `create_exporter_metrics` counters onto the meter's
        # reader, so the meter must still be live to export that final self-observability batch before its own shutdown.
        ordered = sorted(self.by_signal.items(), key=lambda kv: kv[0] == Signal.METRIC)
        rails = Block.of_seq(ordered).map(lambda kv: boundary(TELEMETRY_STOP, lambda kv=kv: _drained(kv[1], kv[0]), catch=_DRAIN_RAISES))
        return traversed(rails, by=Disposition.ACCUMULATE)


class PendingInstall(Struct, frozen=True):
    # `pinned` rides the carrier because the environment keys this install introduced outlive `_pipeline`: a raise
    # BELOW the first set-once registration unwinds inside that bracket, while a raise inside `_commit` unwinds out
    # here, and only a carried roster lets the second path give back exactly what the first would have.
    receipt: InstallReceipt
    providers: InstalledProviders
    meter: MeterProvider
    diagnostic: InMemoryMetricReader | None
    pinned: Block[str] = Block.empty()


# --- [OPERATIONS] -----------------------------------------------------------------------


# an explicit exporter `endpoint=` is used VERBATIM — the SDK appends `/v1/<signal>` only when resolving the env base URL — so the
# per-signal path derives here and one base HTTP endpoint fans to three non-colliding sinks; a gRPC target is a bare
# `host:port` netloc the channel multiplexes, so the GRPC arm returns the base untouched.
def _signal_endpoint(base: str, signal: Signal, transport: EgressTransport) -> str:
    return base if transport is EgressTransport.GRPC else f"{base.rstrip('/')}/v1/{signal.value}"


def _selected(ship: LogShip) -> Block[SignalSpec]:
    # The LOG row arms off the logging owner's own dispatch row rather than a member test, so the provider half and
    # the chain half read ONE table and a ship value gaining or losing its wire arm moves both at once. One member
    # answers both readers — the install fold and the conformance egress row — so the installed signal set the
    # document reports and the set the install builds can never be two different filters.
    return SIGNAL_SPECS.filter(lambda spec: spec.signal is not Signal.LOG or SHIP_OTLP[ship])


# one processor-attach kernel both batched rows parameterize: the bound `add_*` method and the processor class are the only per-signal
# variation, so the identical queue-triple wiring lives once.
def _batched(add: Callable[[Drainable, object], None], processor: Callable[..., object]) -> ProcessorAttach:
    return lambda prov, exp, prof: add(
        prov,
        processor(
            exp,
            max_queue_size=prof.max_queue_size,
            schedule_delay_millis=prof.schedule_delay_ms,
            max_export_batch_size=prof.max_export_batch_size,
            export_timeout_millis=prof.export_timeout_ms,
        ),
    )


def _tracer_provider(resource: Resource, sampler: Sampler) -> TracerProvider:
    # TracerProvider carries no span_processors= constructor slot, so the promotion registers through
    # add_span_processor BEFORE the batch attach and the global set: every span starts already stamped.
    provider = TracerProvider(resource=resource, sampler=sampler, span_limits=SPAN_LIMITS, shutdown_on_exit=False)
    provider.add_span_processor(BaggageSpanProcessor(PROMOTED_BAGGAGE.__contains__))
    return provider


def _pinned(key: str, value: str) -> str:
    environ[key] = value
    return key


def _unpinned(keys: Block[str]) -> None:
    # A failed construction gives back every key it introduced, so the retry's own membership test sees the
    # environment the process actually carried: left standing, a dead profile's derived geometry becomes the
    # preexisting value the next install declines to move, and every later attempt runs under caps nothing chose.
    keys.fold(lambda _held, key: environ.pop(key, ""), None)


def _environed(geometry: SignalProfile) -> Block[str]:
    # Pins land BEFORE the first provider, record, or processor exists — each record default-factories its own
    # `LogRecordLimits`, each processor and exporter resolves its accounting recorder once at construction, and the
    # meter resolves its exemplar filter at the same instant — so a value landing after `_pipeline` opens governs
    # nothing already built. One fold owns every such key, so a new pin is one `SDK_ENV` row and no second site
    # writes the environment. A key the deployment already set stays authoritative and never enters the returned
    # roster, matching how the env resource detector outranks the code-side identity, so the unwind gives back
    # exactly what this install introduced and touches no preexisting global.
    return Block.of_seq(SDK_ENV.items()).choose(
        lambda row: Nothing if row[0] in environ else Some(_pinned(row[0], row[1](geometry)))
    )


def _log_attach(provider: Drainable, exporter: SignalExporter, profile: SignalProfile) -> None:
    # log-side promotion registers first so the promoted attributes ride the record before the batch processor enqueues;
    # on_emit never overwrites an attribute a stdlib or structlog field already set.
    LoggerProvider.add_log_record_processor(provider, BaggageLogProcessor(PROMOTED_BAGGAGE.__contains__))
    _batched(LoggerProvider.add_log_record_processor, BatchLogRecordProcessor)(provider, exporter, profile)


def _sampler(ratio: float) -> Sampler:
    # head sampling rides the parent decision whole — an inbound sampled parent is honored so one distributed trace
    # never fractures across runtimes — and the ratio governs the ROOT arm alone, where this process starts a trace.
    # Ratio 1.0 takes ALWAYS_ON rather than TraceIdRatioBased(1.0), whose bound excludes the single maximum id.
    return ParentBased(root=ALWAYS_ON if ratio >= 1.0 else TraceIdRatioBased(ratio))


def _reservoir(row: ViewRow) -> ReservoirFactory:
    # Reservoir selection per matched stream, decided off the ROW rather than the aggregation class the SDK hands the
    # factory: that argument is the private `_Aggregation` subclass, not the public aggregation the view declares, so
    # branching on it would name an internal type for a fact the row already carries. Boundaries present take the
    # bucket-aligned reservoir — the aggregation itself re-partials the builder with those boundaries — and every
    # other stream takes the bounded random sample. The returned builder therefore ignores its slot argument.
    builder: ReservoirBuilder = (
        AlignedHistogramBucketExemplarReservoir
        if row.boundaries is not None
        else partial(SimpleFixedSizeExemplarReservoir, size=EXEMPLAR_RESERVOIR_SIZE)
    )
    return lambda _aggregation: builder


def _viewed(row: ViewRow) -> View:
    # `_viewed` projects each metrics-owned row into its SDK object at the one surface allowed to name SDK types.
    # Boundaries present mark the deployment-re-armed explicit shape: that row takes the explicit aggregation and the
    # bucket-aligned reservoir, so one exemplar lands per bucket and a click-through resolves the bucket that moved.
    # Every other row passes `aggregation=None`, which resolves `DefaultAggregation` and DEFERS to the reader's own
    # `preferred_aggregation` — the base2-exponential wire default survives every view this fold mints, and a view
    # naming an aggregation outright would be the surface that silently overrode it. Each row's `keys` allow-list
    # drops every unlisted attribute before the stream is identified.
    return View(
        instrument_name=row.instrument,
        attribute_keys=set(row.keys),
        aggregation=ExplicitBucketHistogramAggregation(row.boundaries) if row.boundaries is not None else None,
        exemplar_reservoir_factory=_reservoir(row),
    )


def _meter_provider(
    endpoint: str, headers: dict[str, str] | None, profile: SignalProfile, resource: Resource
) -> tuple[MeterProvider, InMemoryMetricReader | None]:
    exporter = EGRESS[profile.transport][Signal.METRIC](_signal_endpoint(endpoint, Signal.METRIC, profile.transport), headers, profile, None)
    reader = PeriodicExportingMetricReader(
        exporter,
        export_interval_millis=profile.export_interval_ms,
        export_timeout_millis=profile.export_timeout_ms,
    )
    # This reader is the branch's BACKEND-FREE read plane: a support bundle is pulled exactly when the exporter,
    # collector, or store is what failed, so the plane answering what this process measured composes no exporter. Each reader owns independent aggregation storage, so this one drains nothing the exporting reader
    # owes. Constructed BARE on purpose — the reader default is CUMULATIVE for every instrument class while the
    # wire pins DELTA, so a second `snapshot` reports a total rather than the sliver since the last read.
    diagnostic = InMemoryMetricReader() if profile.diagnostic_read else None
    # `exemplar_filter` arms the metrics `context=` hand-off: a sampled active span admits the measurement's
    # exemplar. Naming it here HARD-PINS the conformance row — an open slot resolves `OTEL_METRICS_EXEMPLAR_FILTER`
    # instead, where an `always_off` deployment value deletes every metric-to-trace click-through and no series
    # changes shape to show it. `views` are construction-only — a reader or view added after this call is
    # unreachable — so the metrics-owned rows and the diagnostic reader project here or nowhere.
    return (
        MeterProvider(
            metric_readers=[reader] if diagnostic is None else [reader, diagnostic],
            resource=resource,
            exemplar_filter=TraceBasedExemplarFilter(),
            views=tuple(_viewed(row) for row in views(profile.explicit_buckets)),
            shutdown_on_exit=False,
        ),
        diagnostic,
    )


def _drained(provider: Drainable, signal: Signal) -> Signal:
    # shutdown runs on EVERY exit — a raising or false flush included — so a wedged exporter still releases its
    # provider before the carrier clears; BOTH legs' failures survive together: a shutdown raise joins the held
    # flush failure as one ExceptionGroup instead of masking it, so the per-signal deadline classification the
    # false-flush TimeoutError carries reaches the caller even when the shutdown leg also raises. This IS the
    # branch's one stated drain fold, and it earns the bare `Exception` catch the faults owner otherwise bans by
    # being the seam that RETIRES it: the SDK publishes no typed drain refusal to narrow against, so absorbing that
    # surface here once buys a NAMED raise set every fence above narrows on. The re-raise is total by construction —
    # a lone false flush leaves as the `TimeoutError` this fold minted, and any provider raise leaves inside one
    # `ExceptionGroup`, the signal riding its message because the drain row seats one subject for all three legs.
    faults: list[BaseException] = []
    try:  # Exemption: the flush leg's raise is held, never propagated mid-drain, so the shutdown leg always runs
        if not provider.force_flush(timeout_millis=EXPORT_TIMEOUT_MS):
            faults.append(TimeoutError(signal.value))
    except Exception as flush_fault:
        faults.append(flush_fault)
    try:
        provider.shutdown()
    except Exception as shutdown_fault:
        faults.append(shutdown_fault)
    match faults:
        case []:
            return signal
        case [TimeoutError() as unflushed]:
            raise unflushed
        case held:
            raise ExceptionGroup(f"drain:{signal.value}", held)


# --- [TABLES] ---------------------------------------------------------------------------

# every standing row keeps the HTTP transport default; the SIDECAR daemon alone may inject a GRPC-valued row through
# `signal_profile=`, and GRPC_ELIGIBLE clamps every other profile's injection back to HTTP at install. Gzip rides
# EVERY standing row, so the egress wire law holds on any row that ever binds an emitting provider and no row carries
# a value that would silently violate it — a deployment that must drop compression injects its own geometry instead.
# Ratio splits attended from unattended: an attended run samples every root it starts because a developer is
# reading the trace it produced, while the unattended daemon rides the estate thinning ratio its C# peer spells.
SIGNAL_PROFILE: Final[Map[RuntimeProfile, SignalProfile]] = Map.of_seq(
    (
        p,
        SignalProfile(
            export_interval_ms=i,
            schedule_delay_ms=d,
            max_queue_size=q,
            max_export_batch_size=b,
            compression=Compression.Gzip,
            sample_ratio=r,
            diagnostic_read=x,
        ),
    )
    # `x` arms the backend-free read: a long-lived composition is the one a support bundle gets pulled from, so
    # it carries the reader; a tool or package run ends before anyone pulls and pays no second aggregation store.
    for p, i, d, q, b, r, x in (
        (RuntimeProfile.SIDECAR, 2000, 1000, 2048, 512, 0.1, True),
        (RuntimeProfile.TOOL, 5000, 5000, 512, 128, 1.0, False),
        (RuntimeProfile.PACKAGE, 5000, 5000, 512, 128, 1.0, False),
        (RuntimeProfile.TEST, 5000, 5000, 512, 128, 1.0, True),
    )
)

SIGNAL_SPECS: Final[Block[SignalSpec]] = Block.of_seq([
    SignalSpec(
        Signal.TRACE,
        _tracer_provider,
        _batched(TracerProvider.add_span_processor, BatchSpanProcessor),
        trace.set_tracer_provider,
    ),
    SignalSpec(
        Signal.LOG,
        # every LOG cell defers its lazy dereference to call time — a module-scope bound-method or module-attribute read
        # reifies the cold _logs tier at import — so the row's install cost lands only when a ship row arms the wire.
        lambda resource, _: LoggerProvider(resource=resource, shutdown_on_exit=False),
        _log_attach,
        lambda provider: _logs.set_logger_provider(provider),
    ),
])

# per-transport exporter-factory triples: the profile row's transport selects the row-map, the signal the cell. Every cell
# defers its lazy dereference to call time, so the gRPC tier and the _logs tier reify only when a selected install fires.
# Both enums spell NoCompression/Deflate/Gzip, so the gRPC cell derives grpc.Compression by member name — one compression
# column on the profile row, projected per transport at the seam, never a second knob. The METRIC cell carries the wire
# law pair and DECLINES the `meter_provider` slot its exporter does carry: the only provider in scope is the one this
# reader drives, so seating it feeds the exporter's own export counters back through the pipeline that exports them,
# where the span and log cells land theirs on a reader no signal of theirs closes a loop through.
EGRESS: Final[Map[EgressTransport, Map[Signal, ExporterFactory]]] = Map.of_seq([
    (
        EgressTransport.HTTP,
        Map.of_seq([
            (
                Signal.TRACE,
                lambda ep, hd, prof, mp: OTLPSpanExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=prof.compression, meter_provider=mp
                ),
            ),
            (
                Signal.METRIC,
                lambda ep, hd, prof, _mp: OTLPMetricExporter(
                    endpoint=ep,
                    headers=hd,
                    timeout=EXPORT_TIMEOUT_S,
                    compression=prof.compression,
                    preferred_temporality=dict(WIRE_TEMPORALITY),
                    preferred_aggregation=dict(WIRE_AGGREGATION),
                    max_export_batch_size=prof.max_export_batch_size,
                ),
            ),
            (
                Signal.LOG,
                lambda ep, hd, prof, mp: OTLPLogExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=prof.compression, meter_provider=mp
                ),
            ),
        ]),
    ),
    (
        EgressTransport.GRPC,
        Map.of_seq([
            (
                Signal.TRACE,
                lambda ep, hd, prof, mp: GrpcSpanExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=grpc.Compression[prof.compression.name], meter_provider=mp
                ),
            ),
            (
                Signal.METRIC,
                lambda ep, hd, prof, _mp: GrpcMetricExporter(
                    endpoint=ep,
                    headers=hd,
                    timeout=EXPORT_TIMEOUT_S,
                    compression=grpc.Compression[prof.compression.name],
                    preferred_temporality=dict(WIRE_TEMPORALITY),
                    preferred_aggregation=dict(WIRE_AGGREGATION),
                    max_export_batch_size=prof.max_export_batch_size,
                ),
            ),
            (
                Signal.LOG,
                lambda ep, hd, prof, mp: GrpcLogExporter(
                    endpoint=ep, headers=hd, timeout=EXPORT_TIMEOUT_S, compression=grpc.Compression[prof.compression.name], meter_provider=mp
                ),
            ),
        ]),
    ),
])

# --- [SERVICES] -------------------------------------------------------------------------


class Telemetry:
    # two-tier custody: `_receipts` keys per-composition evidence, `_process` holds the process owner, and one
    # generation reservation excludes install/drain overlap without holding the condition across provider work.
    _receipts: ClassVar[Map[ScopeKey, InstallReceipt]] = Map.empty()
    _process: ClassVar[InstallReceipt | None] = None
    _installed: ClassVar[InstalledProviders | None] = None
    _diagnostic: ClassVar[InMemoryMetricReader | None] = None
    _generation: ClassVar[int] = 0
    # mint-once latch: the OTel `set_*_provider` globals are set-once with no unset, so the process pipeline
    # mints at most once per process — after a drain the globals still bind the drained providers, and a later
    # emitting install refuses loudly instead of re-registering into a warning-swallowed no-op.
    _minted: ClassVar[bool] = False
    _reservation: ClassVar[InstallReservation | None] = None
    _gate = Condition(RLock())

    @classmethod
    def _reserve(cls, scope: ScopeKey) -> InstallReservation:
        cls._generation += 1
        cls._reservation = InstallReservation(generation=cls._generation, scope=scope, thread=get_ident())
        return cls._reservation

    @classmethod
    def _release(cls, reservation: InstallReservation) -> None:
        if cls._reservation == reservation:
            cls._reservation = None
            cls._gate.notify_all()

    @classmethod
    def _commit(cls, pending: PendingInstall, scope: ScopeKey) -> InstallReceipt:
        # fail-before-publish: every fallible construction lands before the first set-once global registration,
        # so a raise here leaves the globals untouched and the caller's release keeps custody un-wedged.
        composite = CompositePropagator(list(PROPAGATORS))
        receipts = cls._receipts.add(scope, pending.receipt)
        metrics.set_meter_provider(pending.meter)
        for spec in SIGNAL_SPECS:
            pending.providers.by_signal.try_find(spec.signal).map(spec.register)
        propagate.set_global_textmap(composite)
        cls._minted = True
        cls._installed = pending.providers
        cls._diagnostic = pending.diagnostic
        cls._process = pending.receipt
        cls._receipts = receipts
        # the estate-owned final-drain ordering leg: `atexit` handlers run AFTER `sys.excepthook`, so the logging
        # owner's uncaught-raise door still reaches a LIVE provider on the ordinary crash path and its wire line
        # ships before this leg drains; a root that already drained makes this a clean no-op (non-owner shutdown
        # answers empty), and the SDK's own per-provider `shutdown_on_exit` hooks stay disabled — this one
        # registration is the sole exit-ordered drain. `_commit` runs at most once per process (set-once globals),
        # so the registration cannot stack.
        atexit.register(partial(cls.shutdown, scope))
        return pending.receipt

    @classmethod
    def snapshot(cls) -> Option[MetricsData]:
        # One backend-free measurement read serves every consumer: a doctor probe, a spec, and the support bundle
        # each answer what this process measured with no collector, exporter, or store reachable. Process-wide by construction —
        # one provider carries one diagnostic reader — so this entry takes no `scope`, and the `composition`
        # attribute the metrics owner stamps is what separates two embedded compositions inside the reading.
        # `Nothing` under a declining profile and an EMPTY tree under an armed one stay distinguishable, so a
        # reader answering nothing never reads as a profile that declined the plane.
        match cls._diagnostic:
            case None:
                return Nothing
            case reader:
                return Option.of_optional(reader.get_metrics_data())

    @classmethod
    def _pipeline(
        cls, profile: RuntimeProfile, endpoint: str, headers: dict[str, str] | None, identity: Resource, geometry: SignalProfile, ship: LogShip
    ) -> PendingInstall:
        # Construction OWNS every allocation it makes until `PendingInstall` returns: the environment carries this
        # install's pins and `_meter_provider` starts a periodic reader's own export thread against a live
        # exporter. A raise below therefore unwinds both — the abandoned reader would otherwise keep exporting for
        # a pipeline no commit ever published, and the pins would stand as the authority the retry declines to
        # move. The rollback drain is fenced so a wedged exporter cannot mask the fault that triggered it.
        pinned = _environed(geometry)
        meter, diagnostic = _meter_provider(endpoint, headers, geometry, identity)
        sampler = _sampler(geometry.sample_ratio)

        def emit(installed: Map[Signal, Drainable], spec: SignalSpec) -> Map[Signal, Drainable]:
            exporter = EGRESS[geometry.transport][spec.signal](_signal_endpoint(endpoint, spec.signal, geometry.transport), headers, geometry, meter)
            provider = spec.provider(identity, sampler)
            spec.attach(provider, exporter, geometry)
            return installed.add(spec.signal, provider)

        seed: Map[Signal, Drainable] = Map.empty().add(Signal.METRIC, meter)
        try:  # Exemption: the construction bracket unwinds its own allocations, the one rollback seam a raise reaches
            by_signal = _selected(ship).fold(emit, seed)
        except BaseException:
            boundary(TELEMETRY_STOP, lambda: _drained(meter, Signal.METRIC), catch=_DRAIN_RAISES)
            _unpinned(pinned)
            raise
        return PendingInstall(
            receipt=InstallReceipt(profile, InstallOutcome.INSTALLED, endpoint, geometry, ship),
            providers=InstalledProviders(by_signal=by_signal),
            meter=meter,
            diagnostic=diagnostic,
            pinned=pinned,
        )

    @classmethod
    def install(
        cls,
        ctx: RuntimeContext,
        endpoint: str,
        headers: dict[str, str] | None = None,
        *,
        resource: Resource | None = None,
        signal_profile: SignalProfile | None = None,
        ship: LogShip = LogShip.OTLP_CONSOLE,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> InstallReceipt:
        # resource/signal_profile are injection seams: the profiles-owned job envelope hands in its hand-built identity
        # and high-interval geometry; every daemon path resolves the standing rows. The transport clamp is the
        # fork-hazard fence made visible: the receipt carries the effective geometry, never a silently divergent one.
        # Both gates read axis values off the admitted context — the deployment shape decides fork safety and the
        # bound telemetry provider decides emission, so the preset name stays a geometry key and never a discriminant.
        profile = ctx.profile
        while True:
            with cls._gate:
                match cls._receipts.try_find(scope):
                    case Option(tag="some", some=prior):
                        return replace(prior, outcome=InstallOutcome.REENTRANT)
                    case _:
                        pass
                match cls._reservation:
                    case InstallReservation(thread=owner) if owner == get_ident():
                        raise RuntimeError(f"telemetry install re-entered by reservation owner {scope!r}")
                    case InstallReservation():
                        cls._gate.wait()
                        continue
                    case None:
                        pass

                identity = resource if resource is not None else RUNTIME_RESOURCE
                requested = signal_profile if signal_profile is not None else SIGNAL_PROFILE[profile]
                geometry = (
                    replace(requested, transport=EgressTransport.HTTP)
                    if requested.transport is EgressTransport.GRPC and ctx.shape.topology not in GRPC_ELIGIBLE
                    else requested
                )
                if not ctx.policy.emit_otel:
                    receipt = InstallReceipt(profile, InstallOutcome.SILENT, endpoint, geometry, ship)
                    cls._receipts = cls._receipts.add(scope, receipt)
                    return receipt
                if cls._process is not None:
                    receipt = replace(cls._process, outcome=InstallOutcome.ADOPTED)
                    cls._receipts = cls._receipts.add(scope, receipt)
                    return receipt
                if cls._minted:
                    raise RuntimeError(f"telemetry pipeline already drained; set-once OTel globals cannot re-register for {scope!r}")
                reservation = cls._reserve(scope)
                break

        try:
            pending = cls._pipeline(profile, endpoint, headers, identity, geometry, ship)
        except BaseException:
            with cls._gate:
                cls._release(reservation)
            raise

        # custody releases on EVERY commit exit — success, raise, or lost reservation — so a commit fault can
        # never wedge waiting installs; the abandoned pipeline drains outside the gate, never under the lock.
        commit_fault: BaseException | None = None
        with cls._gate:
            if cls._reservation == reservation:
                try:
                    return cls._commit(pending, scope)
                except BaseException as fault:  # Exemption: held to drain the un-published pipeline outside the gate
                    commit_fault = fault
                finally:
                    cls._release(reservation)
        # the abandoned pipeline gives back BOTH allocations the construction bracket would have: its providers drain
        # and its environment pins retire, so a later attempt reads the environment this process actually carried
        # rather than a dead geometry standing as the authority its own membership test then declines to move.
        pending.providers.flush()
        _unpinned(pending.pinned)
        if commit_fault is not None:
            raise commit_fault
        raise RuntimeError(f"telemetry install reservation {reservation.generation} lost custody")

    @classmethod
    def receipt(cls) -> Option[InstallReceipt]:
        # process-custody read: Some only while an emitting install owns the pipeline — the workers boot capture, the
        # bundle capsule, and the conformance projection read the standing endpoint, profile, geometry, and ship value
        # as data, never the private latch.
        with cls._gate:
            return Option.of_optional(cls._process)

    @classmethod
    def shutdown(cls, scope: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[Block[Signal]]:
        # custody law: only the scope whose receipt reads INSTALLED drains the process pipeline; a SILENT/ADOPTED scope
        # retires its own receipt alone. A pipeline drain clears every scope receipt — an ADOPTED receipt over a drained
        # pipeline is stale evidence — and the mint-once latch stands: the set-once globals still bind the drained
        # providers, so a later emitting install refuses instead of re-minting.
        while True:
            with cls._gate:
                match cls._reservation:
                    case InstallReservation(thread=owner) if owner == get_ident():
                        raise RuntimeError(f"telemetry shutdown re-entered by reservation owner {scope!r}")
                    case InstallReservation():
                        cls._gate.wait()
                        continue
                    case None:
                        pass
                owner = cls._receipts.try_find(scope).map(lambda r: r.outcome is InstallOutcome.INSTALLED).default_value(False)
                match (owner, cls._installed):
                    case (True, InstalledProviders() as installed):
                        reservation = cls._reserve(scope)
                        cls._installed = cls._process = cls._diagnostic = None
                        cls._receipts = Map.empty()
                        break
                    case _:
                        cls._receipts = cls._receipts.remove(scope) if cls._receipts.contains_key(scope) else cls._receipts
                        return Ok(Block.empty())

        try:
            return installed.flush()
        finally:
            with cls._gate:
                cls._release(reservation)
```

## [03]-[CONFORMANCE]

- Owner: `ConformanceKey` closes the row space this minter legislates, `MinterRole` and `RowDisposition` the two estate vocabularies a reader grades a document against, `ConformanceRow` the three-case family carrying exactly the payload its disposition admits, `ConformanceDocument` the role-stamped row store with its canonical bytes, and `CONFORMANCE` the one cell table mapping each key to the projection that reads its value off the member deciding it. `ROSTER` is that table's import-time totality gate and the digest's ordinal spine at once — a total read per legislated key, in the `observability/metrics#METRIC` `MEASURES` idiom — so a key whose cell never landed refuses at import rather than at a mint, and the mint refuses a document dropping a legislated row rather than digesting it as a branch that never legislated one.
- Cases: three dispositions carry three DIFFERENT facts and nothing collapses them. `carried` is the value the plane holds; `absent` is a row this minter's ROLE reaches no seat for at all; `withheld` is a seat the pinned distribution exposes on no member, so the value the branch honours still digests as that value beside the pin that withholds it. Two rows take `withheld` here and both are this plane's own gaps rather than the estate's standing pair — the log-record ceiling, which the SDK seats on no constructor and reaches through the environment alone, and the tenant-value ceiling, which this SDK train exposes as no per-view numeric limit and the attribute fold closes instead. The exemplar filter and the sender encoding knob, the two seats the corpus entry names as commonly withheld, are both CARRIED here, which is exactly the parity difference this document exists to publish.
- Entry: `ConformanceDocument.of(receipt)` returns the one canonical projection of this branch's governance set, whose `canonical()` bytes are the byte-deriving input `tests/contracts/MANIFEST.md` `[02.13]-[TELEMETRY_CONVENTION]` freezes. The document is its own constructor in the branch's `Receipt.of`/`DrainReceipt.of` idiom rather than a static namespace beside it, and the install receipt is the whole input because it carries the EFFECTIVE geometry after the transport clamp, the resolved profile, and the ship value that decides the installed signal set, so a document can never report a geometry the process declined to install.
- Law: this page keeps NO hand-written governance table. Every value PROJECTS off the member that decides it — the detector chain, the minted identity keys, the domain roster beside its subject spellings, the instrument units, the wire temporality and aggregation maps, the propagator composite, the promoted allowlist, the profile's resolved batch square, ratio, cardinality budget, and caps — so a value and the surface it reports move together and a row asserted beside its owner cannot drift from it. The key is not a row field: `CONFORMANCE` holds it, so a row can never spell a key its own cell is not filed under.
- Auto: canonical bytes ride the receipts-owned deterministic `ENCODE` and no second encoder reaches the document, so key order is the encoder's and row order is `ROSTER`'s. Every roster projection dedupes and sorts ORDINALLY before joining, because the three branch comparators agree only over the printable-ASCII alphabet every rostered token and every `ConformanceKey` member stays inside; every scalar renders through Python's locale-free formatting, which is why no invariant-culture argument has a counterpart here. An `absent` row OMITS its value key rather than spelling a null, the same absence posture every sometimes-absent dimension crossing this wire already takes. `service.instance.id` reaches the identity row as a KEY alone — its value is a per-process mint, so carrying it would digest one process rather than one branch.
- Growth: a new conformance obligation is one `ConformanceKey` member and one `CONFORMANCE` cell, with the import gate breaking loudly until the cell lands; a plane that opens a seat this branch reads as withheld re-values that same cell rather than adding one; a new provenance column is one owner argument; a role that reaches a row this one cannot flips exactly one cell onto `absent`.
- Boundary: the row set is composition CODE until something projects it, and three branches holding equivalent code with no document prove nothing against each other — this projection is what makes the estate comparison possible at all. It is TERMINAL: nothing on this page reads it, its one consumer is the corpus conformance fixture, and it constructs no provider, opens no reader, and names no SDK type the install owner does not already hold. Cells name no lazily-imported member, so reading the document never reifies the cold `_logs` or gRPC exporter tiers a silent process declined. Rows this branch does not decide stay off the roster: the profiles rail declares its own row at `observability/profiles#PROFILES`, which imports this owner and is never imported by it.

```python signature
# --- [TYPES] ----------------------------------------------------------------------------

# Roles name what a MINTER can reach, so a row a plane cannot carry and a row a branch dropped stay distinguishable
# at the digest instead of both reading as absence. This branch mints PROCESS; the other two members exist because
# the row space is role-spanned and a reader grades one document against a peer minted under another role.
class MinterRole(StrEnum):
    PROCESS = "process"
    BROWSER = "browser"
    DEPLOY = "deploy"


# wire vocabulary a row's CASE renders onto — the case stays the authority and this is only its spelling.
class RowDisposition(StrEnum):
    CARRIED = "carried"
    ROLE_ABSENT = "role-absent"
    SEAT_WITHHELD = "seat-withheld"


# The closed key space this minter legislates, one member per governance obligation the estate conformance law rules.
# Every member is lowercase dotted printable ASCII on purpose: the three branch comparators order by codepoint, by
# UTF-16 code unit, and by the default JavaScript order respectively, and they agree exactly over that alphabet — a
# key admitting an astral or private-use character forks the three sorts and mints three generations from one row set.
class ConformanceKey(StrEnum):
    RESOURCE_IDENTITY = "resource.identity"
    RESOURCE_DETECTORS = "resource.detectors"
    RESOURCE_PRECEDENCE = "resource.precedence"
    SCHEMA_COORDINATE = "schema.coordinate"
    SCOPE_COORDINATE = "scope.coordinate"
    SCOPE_ROSTER = "scope.roster"
    METRIC_GRAMMAR = "metric.grammar"
    METRIC_SUBJECTS = "metric.subjects"
    METRIC_UNITS = "metric.units"
    METRIC_TEMPORALITY = "metric.temporality"
    METRIC_AGGREGATION = "metric.aggregation"
    METRIC_EXEMPLAR = "metric.exemplar"
    METRIC_VIEWCAP = "metric.viewcap"
    METRIC_VIEWS = "metric.views"
    METRIC_DIMENSIONS = "metric.dimensions"
    METRIC_ABSENCE = "metric.dimension.absence"
    METRIC_PROCESS = "metric.process.series"
    PROPAGATION_DIALECT = "propagation.dialect"
    PROPAGATION_REGISTRATION = "propagation.registration"
    PROPAGATION_ADOPTION = "propagation.adoption"
    SAMPLE_TRACE_RATIO = "sample.trace.ratio"
    SPAN_LIMIT_CAPS = "span.limits"
    TENANT_KEY = "tenant.key"
    TENANT_ALLOWLIST = "tenant.allowlist"
    TENANT_OVERFLOW = "tenant.overflow"
    SIGNAL_ROSTER = "signal.roster"
    SIGNAL_DIAGNOSTIC = "signal.diagnostic"
    SIGNAL_VITALS = "signal.vitals"
    EGRESS_PROTOCOL = "egress.protocol"
    EGRESS_COMPRESSION = "egress.compression"
    EGRESS_ELIGIBILITY = "egress.eligibility"
    EGRESS_ENDPOINT = "egress.endpoint"
    EGRESS_SIGNALS = "egress.signals"
    EGRESS_BATCH_SPAN = "egress.batch.span"
    EGRESS_BATCH_LOG = "egress.batch.log"
    EGRESS_READER = "egress.reader.cadence"
    EGRESS_PINS = "egress.pins"
    EGRESS_LOSS = "egress.loss"
    EGRESS_QUEUE = "egress.queue"
    LOG_IDENTITY = "log.identity"
    LOG_PIPELINE = "log.pipeline"
    LOG_PROMOTION = "log.promotion"
    LOG_RECORD_CAPS = "log.record.caps"
    DRAIN_ORDER = "drain.order"
    DRAIN_BOUND = "drain.bound"
    DRAIN_EXIT = "drain.exit"


type RowCell = Callable[[InstallReceipt], "ConformanceRow"]

# --- [MODELS] ---------------------------------------------------------------------------


# Three dispositions carry three different facts, so each case holds exactly the payload its own admits: a carried
# row with no value and an absent row with one are both unspellable here, where one flat record with two optional
# slots left the pair to construction discipline at every site. `owner` names the MEMBER the value projects off, so
# a stale value is a visible edit at that member rather than a paragraph nobody re-reads.
@tagged_union(frozen=True)
class ConformanceRow:
    tag: Literal["carried", "absent", "withheld"] = tag()
    carried: tuple[str, str] = case()  # (value, owner)
    absent: str = case()  # (owner) — this minter's ROLE reaches no seat for the row at all
    withheld: tuple[str, str, str] = case()  # (value, pin, owner) — the pinned distribution exposes no seat

    def projected(self, key: ConformanceKey) -> dict[str, str]:
        # The wire row, keyed by the map key its cell is filed under so no case can carry a second spelling of it.
        # An absent row OMITS `value` rather than spelling a null: a sentinel here reads on a diff exactly like a
        # carried empty string, which is the one distinction the role column exists to keep.
        match self:
            case ConformanceRow(tag="carried", carried=(value, owner)):
                return {"key": key.value, "disposition": RowDisposition.CARRIED.value, "value": value, "owner": owner}
            case ConformanceRow(tag="absent", absent=owner):
                return {"key": key.value, "disposition": RowDisposition.ROLE_ABSENT.value, "owner": owner}
            case ConformanceRow(tag="withheld", withheld=(value, pin, owner)):
                return {
                    "key": key.value, "disposition": RowDisposition.SEAT_WITHHELD.value,
                    "value": value, "pin": pin, "owner": owner,
                }
            case _ as unreachable:
                assert_never(unreachable)


class ConformanceDocument(Struct, frozen=True):
    role: MinterRole
    schema_url: str
    rows: Map[ConformanceKey, ConformanceRow]

    @staticmethod
    def of(receipt: InstallReceipt) -> "ConformanceDocument":
        # The receipt is the whole byte-deriving input: it carries the EFFECTIVE geometry the transport clamp
        # settled, the resolved profile, and the ship value deciding the installed signal set, so no document can
        # report a geometry this process declined to install. The cell read is total over `ROSTER`, so a mint whose
        # table lost a row refuses here rather than digesting as a branch that never legislated it.
        return ConformanceDocument(
            role=MinterRole.PROCESS,
            schema_url=SCHEMA_URL,
            rows=Map.of_seq((key, CONFORMANCE[key](receipt)) for key in ROSTER),
        )

    def canonical(self) -> bytes:
        # `ROSTER` IS the digest order, read TOTALLY: a document dropping a legislated row refuses here rather than
        # digesting as a branch that never legislated it, and no second ordering exists to disagree with this one.
        # Bytes ride the receipts-owned deterministic encoder — the same one the log render and the bundle archive
        # take — so key order is fixed at one owner and no second JSON spelling of this document can exist.
        return ENCODE({
            "role": self.role.value,
            "schema_url": self.schema_url,
            "rows": tuple(ROSTER.map(lambda key: self.rows[key].projected(key))),
        })


# --- [OPERATIONS] -----------------------------------------------------------------------


def _joined(values: Iterable[str]) -> str:
    # Every roster projection dedupes and sorts ORDINALLY before joining. Python `sorted` orders by codepoint where
    # the C# comparator orders by UTF-16 code unit and the JavaScript default likewise; the three coincide over the
    # printable-ASCII alphabet every rostered token stays inside, so an insertion-ordered join mints a document a
    # peer reproduces from the same rows only by luck.
    return ",".join(sorted(set(values)))


def _square(*bounds: int) -> str:
    # One geometry render for every bound tuple a row carries. Python formats an integer locale-free, so the
    # invariant-culture argument its peers must pass has no counterpart here and no row spells its own separator.
    return "/".join(str(bound) for bound in bounds)


def _batch_square(profile: SignalProfile) -> str:
    # ONE geometry drives both batch processors through the `_batched` kernel, so the span and log rows render off
    # one member and cannot mirror each other wrong — two cells spelling the same four columns is the deleted form.
    return _square(profile.max_queue_size, profile.max_export_batch_size, profile.schedule_delay_ms, profile.export_timeout_ms)


# --- [TABLES] ---------------------------------------------------------------------------

# One cell per legislated key, each reading its value off the member that DECIDES it — never a literal beside that
# member. A posture with no readable owner is the one form a literal legitimately takes, because the posture IS the
# fact: a merge precedence, an allowlist semantic, and a drain ordering are decided by the shape of the code rather
# than by a value it holds. No cell names a lazily-imported member, so minting a document never reifies the cold
# `_logs` or gRPC exporter tiers an install declined.
CONFORMANCE: Final[Map[ConformanceKey, RowCell]] = Map.of_seq([
    (ConformanceKey.RESOURCE_IDENTITY, lambda _r: ConformanceRow(carried=(_joined(RUNTIME_IDENTITY.keys()), "RUNTIME_IDENTITY"))),
    (
        ConformanceKey.RESOURCE_DETECTORS,
        lambda _r: ConformanceRow(carried=(_joined(type(detector).__name__ for detector in RESOURCE_DETECTORS), "RESOURCE_DETECTORS")),
    ),
    (
        ConformanceKey.RESOURCE_PRECEDENCE,
        lambda _r: ConformanceRow(
            carried=("detector-chain order, env detector last, deployment override wins the merge", "get_aggregated_resources")
        ),
    ),
    (ConformanceKey.SCHEMA_COORDINATE, lambda _r: ConformanceRow(carried=(SCHEMA_URL, "SCHEMA_URL"))),
    (
        ConformanceKey.SCOPE_COORDINATE,
        lambda _r: ConformanceRow(carried=("scope name, distribution version, semconv schema url — one triple per emitting library", "scoped")),
    ),
    (ConformanceKey.SCOPE_ROSTER, lambda _r: ConformanceRow(carried=(_joined(SCOPES.values()), "SCOPES"))),
    (ConformanceKey.METRIC_GRAMMAR, lambda _r: ConformanceRow(carried=(f"{NAMESPACE}.<domain>.<measure>", "InstrumentSpec.domain"))),
    # The pair form is what the estate's byte-identity clause actually grades: two minters claiming one segment
    # carry one subject spelling or fail here, and a segment-only roster proves nothing about the spelling.
    (
        ConformanceKey.METRIC_SUBJECTS,
        lambda _r: ConformanceRow(carried=(_joined(f"{segment}={subject}" for segment, subject in DOMAINS.items()), "DOMAINS")),
    ),
    (ConformanceKey.METRIC_UNITS, lambda _r: ConformanceRow(carried=(_joined(spec.unit for spec in INSTRUMENTS), "InstrumentSpec.unit"))),
    (
        ConformanceKey.METRIC_TEMPORALITY,
        lambda _r: ConformanceRow(
            carried=(_joined(f"{family.__name__}={temporality.name.lower()}" for family, temporality in WIRE_TEMPORALITY.items()), "WIRE_TEMPORALITY")
        ),
    ),
    # CARRIED where a peer withholds it: this plane seats the preference on the metric exporter itself, so the
    # base2-exponential wire default is a value the branch holds rather than a ceiling it states without a member.
    (
        ConformanceKey.METRIC_AGGREGATION,
        lambda _r: ConformanceRow(
            carried=(
                _joined(f"{family.__name__}={type(aggregation).__name__}" for family, aggregation in WIRE_AGGREGATION.items()),
                "WIRE_AGGREGATION",
            )
        ),
    ),
    (
        ConformanceKey.METRIC_EXEMPLAR,
        lambda _r: ConformanceRow(
            carried=(
                f"{TraceBasedExemplarFilter.__name__} at the constructor, {EXEMPLAR_RESERVOIR_SIZE}-slot fixed reservoir",
                "_meter_provider",
            )
        ),
    ),
    # WITHHELD: this SDK train exposes no per-view numeric cardinality limit, so the ceiling the branch honours is
    # closed at the one attribute fold every recording path shares. A train opening that seat fails here until the
    # cell re-values, which is what keeps a stated ceiling out of the dropped-row class.
    (
        ConformanceKey.METRIC_VIEWCAP,
        lambda r: ConformanceRow(
            withheld=(str(r.signal_profile.cardinality_budget), "no per-view cardinality seat in this sdk train", "SignalProfile.cardinality_budget")
        ),
    ),
    (
        ConformanceKey.METRIC_VIEWS,
        lambda _r: ConformanceRow(
            carried=("one name-exact view per instrument row, allowlist bounded, aggregation deferred to the reader preference", "views")
        ),
    ),
    (ConformanceKey.METRIC_DIMENSIONS, lambda _r: ConformanceRow(carried=(_joined(Dimension), "Dimension"))),
    (
        ConformanceKey.METRIC_ABSENCE,
        lambda _r: ConformanceRow(
            carried=("per-row key allowlist; a sometimes-absent dimension stays rostered and an absent entry omits its key", "InstrumentSpec.keys")
        ),
    ),
    (
        ConformanceKey.METRIC_PROCESS,
        lambda _r: ConformanceRow(carried=(_joined(spec.name for spec in INSTRUMENTS if spec.domain == "process"), "InstrumentSpec.domain")),
    ),
    (
        ConformanceKey.PROPAGATION_DIALECT,
        lambda _r: ConformanceRow(carried=(_joined(type(propagator).__name__ for propagator in PROPAGATORS), "PROPAGATORS")),
    ),
    (
        ConformanceKey.PROPAGATION_REGISTRATION,
        lambda _r: ConformanceRow(carried=("one composite published as the process global textmap", "Telemetry._commit")),
    ),
    (
        ConformanceKey.PROPAGATION_ADOPTION,
        lambda _r: ConformanceRow(carried=("inbound parent honoured whole; the ratio governs the root arm alone", "_sampler")),
    ),
    (ConformanceKey.SAMPLE_TRACE_RATIO, lambda r: ConformanceRow(carried=(str(r.signal_profile.sample_ratio), "SignalProfile.sample_ratio"))),
    (
        ConformanceKey.SPAN_LIMIT_CAPS,
        lambda _r: ConformanceRow(
            carried=(
                _square(SPAN_LIMITS.max_attributes, SPAN_LIMITS.max_events, SPAN_LIMITS.max_links, SPAN_LIMITS.max_span_attribute_length),
                "SPAN_LIMITS",
            )
        ),
    ),
    (ConformanceKey.TENANT_KEY, lambda _r: ConformanceRow(carried=(TENANT_BAGGAGE, "TENANT_BAGGAGE"))),
    (ConformanceKey.TENANT_ALLOWLIST, lambda _r: ConformanceRow(carried=(_joined(PROMOTED_BAGGAGE), "PROMOTED_BAGGAGE"))),
    (ConformanceKey.TENANT_OVERFLOW, lambda _r: ConformanceRow(carried=(OVERFLOW_KEY, "OVERFLOW_KEY"))),
    (ConformanceKey.SIGNAL_ROSTER, lambda _r: ConformanceRow(carried=(_joined(signal.value for signal in Signal), "Signal"))),
    (
        ConformanceKey.SIGNAL_DIAGNOSTIC,
        lambda r: ConformanceRow(carried=(str(r.signal_profile.diagnostic_read).lower(), "SignalProfile.diagnostic_read")),
    ),
    # The one ROLE-absent row at this pin: interaction vitals are a browser-plane signal, and a process minter
    # reaches no seat for them at all. Marking the row rather than dropping it is what lets a reader tell a role
    # that cannot carry a signal from a branch that quietly stopped carrying one.
    (ConformanceKey.SIGNAL_VITALS, lambda _r: ConformanceRow(absent="MinterRole.PROCESS")),
    (ConformanceKey.EGRESS_PROTOCOL, lambda r: ConformanceRow(carried=(f"{r.signal_profile.transport.value}/protobuf", "SignalProfile.transport"))),
    (ConformanceKey.EGRESS_COMPRESSION, lambda r: ConformanceRow(carried=(r.signal_profile.compression.value, "SignalProfile.compression"))),
    (ConformanceKey.EGRESS_ELIGIBILITY, lambda _r: ConformanceRow(carried=(_joined(topology.value for topology in GRPC_ELIGIBLE), "GRPC_ELIGIBLE"))),
    # The trace leg renders the fan every signal takes, so the row derives the shape instead of restating it.
    (
        ConformanceKey.EGRESS_ENDPOINT,
        lambda r: ConformanceRow(carried=(_signal_endpoint("<base>", Signal.TRACE, r.signal_profile.transport), "_signal_endpoint")),
    ),
    (
        ConformanceKey.EGRESS_SIGNALS,
        lambda r: ConformanceRow(carried=(_joined([Signal.METRIC.value, *(spec.signal.value for spec in _selected(r.ship))]), "_selected")),
    ),
    # Two keys, ONE render: the estate row space keys the span and log squares apart because peers geometry them
    # apart, while this branch drives both processors off a single profile row.
    (ConformanceKey.EGRESS_BATCH_SPAN, lambda r: ConformanceRow(carried=(_batch_square(r.signal_profile), "_batch_square"))),
    (ConformanceKey.EGRESS_BATCH_LOG, lambda r: ConformanceRow(carried=(_batch_square(r.signal_profile), "_batch_square"))),
    (
        ConformanceKey.EGRESS_READER,
        lambda r: ConformanceRow(carried=(_square(r.signal_profile.export_interval_ms, r.signal_profile.export_timeout_ms), "_meter_provider")),
    ),
    (ConformanceKey.EGRESS_PINS, lambda _r: ConformanceRow(carried=(_joined(key for key, _cell in SDK_ENV.items()), "SDK_ENV"))),
    (
        ConformanceKey.EGRESS_LOSS,
        lambda _r: ConformanceRow(
            carried=(f"{OTEL_PYTHON_SDK_INTERNAL_METRICS_ENABLED} armed; processor and exporter recorders mount on the install meter", "SDK_ENV")
        ),
    ),
    (ConformanceKey.EGRESS_QUEUE, lambda _r: ConformanceRow(carried=("gateway persistent queue; no branch-side durable otlp queue armed", "EGRESS"))),
    (ConformanceKey.LOG_IDENTITY, lambda _r: ConformanceRow(carried=("the runtime resource, verbatim", "RUNTIME_RESOURCE"))),
    (ConformanceKey.LOG_PIPELINE, lambda r: ConformanceRow(carried=(f"{r.ship.value}/{'wire' if SHIP_OTLP[r.ship] else 'silent'}", "SHIP_OTLP"))),
    (
        ConformanceKey.LOG_PROMOTION,
        lambda _r: ConformanceRow(
            carried=("allowlisted baggage processors seated on the span and log providers at construction", "PROMOTED_BAGGAGE")
        ),
    ),
    # WITHHELD: neither the provider, the emit seam, nor the record constructor carries a limits argument, so the
    # trace leg's `SPAN_LIMITS` slot has no log counterpart and this ceiling reaches the SDK through the environment
    # alone — where a key the deployment already set stays authoritative and this fold never introduces it.
    (
        ConformanceKey.LOG_RECORD_CAPS,
        lambda r: ConformanceRow(
            withheld=(
                _square(
                    r.signal_profile.record_caps.attribute_count, r.signal_profile.record_caps.attribute_length,
                    r.signal_profile.record_caps.record_attribute_count, r.signal_profile.record_caps.record_attribute_length,
                ),
                "sdk seats log-record limits on no constructor; environment-only and deployment-authoritative",
                "SDK_ENV",
            )
        ),
    ),
    (
        ConformanceKey.DRAIN_ORDER,
        lambda _r: ConformanceRow(
            carried=("span and log providers first, meter last, faults accumulating across signals", "InstalledProviders.flush")
        ),
    ),
    (ConformanceKey.DRAIN_BOUND, lambda _r: ConformanceRow(carried=(f"{EXPORT_TIMEOUT_MS}ms per provider flush", "EXPORT_TIMEOUT_MS"))),
    (
        ConformanceKey.DRAIN_EXIT,
        lambda _r: ConformanceRow(carried=("one atexit final-drain leg; every sdk per-provider exit hook disabled", "Telemetry._commit")),
    ),
])

# Import-time totality beside the digest spine, one member serving both duties. `CONFORMANCE[key]` is the TOTAL read
# — the `observability/metrics#METRIC` `MEASURES` idiom — so a legislated key whose cell never landed refuses the
# module rather than a mint, and the sorted roster is the one row order every digest reads. The reverse direction
# needs no gate: a cell filed under a key outside this vocabulary cannot be spelled at all.
ROSTER: Final[Block[ConformanceKey]] = Block.of_seq([key for key in sorted(ConformanceKey) if CONFORMANCE[key]])
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
