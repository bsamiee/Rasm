"""Spans through `rasm_telemetry.export.provider` and `rasm_telemetry.export.ManualSpanProcessor`, copied from opentelemetry-sdk `tests/trace/export/test_export.py` and `tests/trace/test_trace.py` (1.45.2)."""

from collections.abc import Sequence
import threading
import time
from typing import override
from unittest.mock import MagicMock, patch

from opentelemetry.sdk import trace
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace.id_generator import RandomIdGenerator
from opentelemetry.sdk.trace.export import SimpleSpanProcessor, SpanExporter, SpanExportResult

from rasm_telemetry.export import ManualSpanProcessor, provider

# --- [SERVICES] -------------------------------------------------------------------------


class MySpanExporter(SpanExporter):
    """Very simple span exporter used for testing, as the exemplar's class of this name (`test_export.py:30-56`)."""

    def __init__(self, destination: list[str], max_export_batch_size: int | None = None, export_timeout_millis: float = 0.0, export_event: threading.Event | None = None) -> None:
        """Keeps the list each exported span's name is appended to."""
        self.destination = destination
        self.max_export_batch_size = max_export_batch_size
        self.is_shutdown = False
        self.export_timeout = export_timeout_millis / 1e3
        self.export_event = export_event

    @override
    def export(self, spans: Sequence[trace.ReadableSpan]) -> SpanExportResult:
        """Appends the name of each span."""
        if self.max_export_batch_size is not None and len(spans) > self.max_export_batch_size:
            raise ValueError("Batch is too big")
        time.sleep(self.export_timeout)
        self.destination.extend(span.name for span in spans)
        if self.export_event:
            self.export_event.set()
        return SpanExportResult.SUCCESS

    @override
    def shutdown(self) -> None:
        """Marks the exporter shut down."""
        self.is_shutdown = True


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_simple_span_processor() -> None:
    """Spans of the provider reach the exporter of the processor it was given, innermost first (`test_export.py:60-78`)."""
    spans_names_list: list[str] = []
    my_exporter = MySpanExporter(destination=spans_names_list)
    span_processor = SimpleSpanProcessor(my_exporter)
    tracer_provider = provider("rasm-telemetry-tests", "0", span_processor)
    tracer = tracer_provider.get_tracer(__name__)
    with tracer.start_as_current_span("foo"), tracer.start_as_current_span("bar"), tracer.start_as_current_span("xxx"):
        pass
    assert spans_names_list == ["xxx", "bar", "foo"]
    span_processor.shutdown()
    assert my_exporter.is_shutdown




def test_batch_span_processor_not_sampled() -> None:
    """A flush exports no unsampled span (`test_export.py:286-305`)."""
    tracer_provider = trace.TracerProvider(sampler=trace.sampling.ALWAYS_OFF)
    tracer = tracer_provider.get_tracer(__name__)
    spans_names_list: list[str] = []
    my_exporter = MySpanExporter(destination=spans_names_list, max_export_batch_size=128)
    span_processor = ManualSpanProcessor(my_exporter)
    tracer_provider.add_span_processor(span_processor)
    with tracer.start_as_current_span("foo"):
        pass
    time.sleep(0.05)
    span_processor.force_flush()
    assert not spans_names_list
    span_processor.shutdown()


@patch("opentelemetry.sdk.trace.sampling._get_from_env_or_default")
@patch.object(Resource, "create")
def test_tracer_provider_init_default(resource_patch: MagicMock, sample_patch: MagicMock) -> None:
    """A provider made with no option beyond its service holds the SDK's defaults (opentelemetry-sdk `tests/trace/test_trace.py:2238-2247`)."""
    tracer_provider = provider("rasm-telemetry-tests-init-default", "0")
    assert isinstance(tracer_provider.id_generator, RandomIdGenerator)
    resource_patch.assert_called_once()
    assert tracer_provider._resource is not None
    sample_patch.assert_called_once()
    assert tracer_provider._span_limits is not None
    assert tracer_provider._atexit_handler is not None
