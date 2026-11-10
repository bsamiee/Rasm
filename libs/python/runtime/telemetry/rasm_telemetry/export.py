"""OpenTelemetry SDK parts a process entry or a host plug-in composes: the resource of an exported signal, a tracer provider outside the API's process-wide one, and a span processor exporting on flush alone, on the thread that flushes."""

from collections import deque
from functools import cache
from typing import Final, override

from opentelemetry.sdk.resources import get_aggregated_resources, OTELResourceDetector, Resource, SERVICE_NAME, SERVICE_VERSION
from opentelemetry.sdk.trace import ReadableSpan, SpanProcessor, TracerProvider
from opentelemetry.sdk.trace.export import SpanExporter, SpanExportResult
from opentelemetry.util._importlib_metadata import entry_points

# --- [CONSTANTS] ------------------------------------------------------------------------

_DETECTORS: Final = frozenset({"host", "os", "process"})

# --- [SERVICES] -------------------------------------------------------------------------


class ManualSpanProcessor(SpanProcessor):
    """Span processor holding ended sampled spans until `force_flush` exports them on the calling thread, starting no thread and exporting nothing at span end.

    A tracer provider flushes it through `TracerProvider.force_flush`, and a meter provider collects the same way through `PeriodicExportingMetricReader(<exporter>, export_interval_millis=math.inf)`.
    """

    __slots__ = ("_ended", "_exporter")

    def __init__(self, exporter: SpanExporter, /, *, held: int = 2048) -> None:
        """Initializes the processor over an exporter.

        Args:
            exporter: Exporter receiving each flushed batch, its own timeout bounding the flush.
            held: Ended spans kept between flushes, the oldest dropped past it, `BatchSpanProcessor`'s queue size by default.
        """
        self._exporter, self._ended = exporter, deque[ReadableSpan](maxlen=held)

    @override
    def on_end(self, span: ReadableSpan) -> None:
        """Holds an ended span its sampler recorded for export."""
        if span.context.trace_flags.sampled:
            self._ended.append(span)

    @override
    def force_flush(self, timeout_millis: int = 30000) -> bool:
        """Exports every held span in one batch, returning whether the exporter accepted it."""
        batch = tuple(self._ended.popleft() for _ in range(len(self._ended)))
        return not batch or self._exporter.export(batch) is SpanExportResult.SUCCESS

    @override
    def shutdown(self) -> None:
        """Drops held spans no flush exported and shuts the exporter down, sending nothing a host no longer allows."""
        self._ended.clear()
        self._exporter.shutdown()


# --- [OPERATIONS] -----------------------------------------------------------------------


def provider(service: str, version: str, /, *processors: SpanProcessor) -> TracerProvider:
    """Returns a tracer provider of one service that registers no exit hook and is process-wide only once its caller sets it so.

    Without a processor it gives each span the ids its log records and child processes carry, starts no thread, sends nothing, and detects no resource.
    With processors it holds the detected resource, its owner calling `force_flush` where a send is allowed and `shutdown` at release.
    """
    held = TracerProvider(resource=detected(service, version) if processors else Resource.get_empty(), shutdown_on_exit=False)
    for processor in processors:
        held.add_span_processor(processor)
    return held


@cache
def detected(service: str, version: str, /) -> Resource:
    """Returns the resource of a service's exported signals, detected once per process: its name, version, and instance id under the process, runtime, operating system, and host attributes the SDK's detectors read, each under `OTEL_RESOURCE_ATTRIBUTES` and `OTEL_SERVICE_NAME`."""
    detectors = tuple(point.load()() for point in entry_points(group="opentelemetry_resource_detector") if point.name in _DETECTORS)
    return get_aggregated_resources((*detectors, OTELResourceDetector()), Resource.create({SERVICE_NAME: service, SERVICE_VERSION: version}))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ManualSpanProcessor", "detected", "provider"]
