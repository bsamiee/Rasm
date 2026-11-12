"""OpenTelemetry SDK parts a process entry or a host plug-in composes: the resource of an exported signal, a tracer provider outside the API's process-wide one, and a span processor exporting on flush alone, on the thread that flushes."""

from collections import deque
from collections.abc import Callable
from functools import cache
from typing import Final, override

from opentelemetry.sdk.resources import get_aggregated_resources, Resource, SERVICE_NAME, SERVICE_VERSION
from opentelemetry.sdk.trace import ReadableSpan, SpanProcessor, TracerProvider
from opentelemetry.sdk.trace.export import SpanExporter, SpanExportResult
from opentelemetry.util._importlib_metadata import entry_points

# --- [CONSTANTS] ------------------------------------------------------------------------

_DETECTORS: Final = frozenset({"host", "os", "process"})

# --- [SERVICES] -------------------------------------------------------------------------


class ManualSpanProcessor(SpanProcessor):
    """Span processor holding ended sampled spans until `force_flush` or `shutdown` exports them on the calling thread, starting no thread and exporting nothing at span end.

    A tracer provider flushes it through `TracerProvider.force_flush`, and a meter provider collects the same way through `PeriodicExportingMetricReader(<exporter>, export_interval_millis=math.inf)`.
    """

    __slots__ = ("_allowed", "_ended", "_exporter")

    def __init__(self, exporter: SpanExporter, /, *, held: int = 2048, allowed: Callable[[], bool] = lambda: True) -> None:
        """Initializes the processor over an exporter.

        Args:
            exporter: Exporter receiving each flushed batch, its own timeout bounding the flush.
            held: Ended spans kept between flushes, the oldest dropped past it, `BatchSpanProcessor`'s queue size by default.
            allowed: Whether a host permits a send, read at each flush, a refused flush keeping its spans and a refused shutdown dropping them.
        """
        self._exporter, self._ended, self._allowed = exporter, deque[ReadableSpan](maxlen=held), allowed

    @override
    def on_end(self, span: ReadableSpan) -> None:
        """Holds an ended span its sampler recorded for export, dropping it after shutdown."""
        if span.context.trace_flags.sampled:
            self._ended.append(span)

    @override
    def force_flush(self, timeout_millis: int = 30000) -> bool:
        """Exports every held span in one batch where a send is allowed, returning whether no span stays held and the exporter accepted the batch."""
        batch = [self._ended.popleft() for _ in range(len(self._ended) if self._allowed() else 0)]
        return not self._ended and (not batch or self._exporter.export(batch) is SpanExportResult.SUCCESS)

    @override
    def shutdown(self) -> None:
        """Exports held spans where a send is allowed, drops the rest and every span ended later, and shuts the exporter down."""
        self.force_flush()
        self._ended = deque(maxlen=0)
        self._exporter.shutdown()


# --- [OPERATIONS] -----------------------------------------------------------------------


def provider(service: str, version: str, /, *processors: SpanProcessor) -> TracerProvider:
    """Returns a tracer provider of one service over its detected resource, process-wide only once its caller sets it so.

    Its spans give log records and child processes their ids, and reach each processor in order.
    It shuts its processors down once: at its owner's `shutdown`, else at interpreter exit as every SDK provider does.
    """
    held = TracerProvider(resource=detected(service, version))
    for processor in processors:
        held.add_span_processor(processor)
    return held


@cache
def detected(service: str, version: str, /) -> Resource:
    """Returns the resource of a service's exported signals, detected once per process.

    Its name and version are the ones given, over `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES` as `Resource.create` orders them.
    Every other attribute of those variables and of the detectors `OTEL_EXPERIMENTAL_RESOURCE_DETECTORS` names sits over the process, runtime, operating system, and host attributes the SDK's own detectors read.
    """
    detectors = tuple(point.load()() for point in entry_points(group="opentelemetry_resource_detector") if point.name in _DETECTORS)
    return get_aggregated_resources(detectors, Resource.get_empty()).merge(Resource.create({SERVICE_NAME: service, SERVICE_VERSION: version}))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ManualSpanProcessor", "detected", "provider"]
