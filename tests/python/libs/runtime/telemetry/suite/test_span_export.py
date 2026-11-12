"""Spans through `rasm_telemetry.export.provider` and `rasm_telemetry.export.ManualSpanProcessor`, copied from opentelemetry-sdk `tests/trace/export/test_export.py`, `tests/trace/test_trace.py`, `tests/resources/test_resources.py`, and `tests/shared_internal/test_batch_processor.py` (1.45.2)."""

from collections.abc import Sequence
from os import environ
import subprocess
import sys
import threading
import time
from typing import override
from unittest import mock
from unittest.mock import Mock, patch

from opentelemetry.sdk import trace
from opentelemetry.sdk.environment_variables import OTEL_RESOURCE_ATTRIBUTES, OTEL_SERVICE_NAME
from opentelemetry.sdk.trace import ReadableSpan
from opentelemetry.sdk.trace.export import SimpleSpanProcessor, SpanExporter, SpanExportResult
from opentelemetry.sdk.util.instrumentation import InstrumentationScope
from opentelemetry.trace import SpanContext, TraceFlags

from rasm_telemetry.export import detected, ManualSpanProcessor, provider

# --- [CONSTANTS] ------------------------------------------------------------------------

BASIC_SPAN = ReadableSpan(
    "MySpan", context=SpanContext(trace_id=1, span_id=1, is_remote=False, trace_flags=TraceFlags(TraceFlags.SAMPLED)), instrumentation_scope=InstrumentationScope("example", "example")
)

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


@patch.dict(environ, {OTEL_SERVICE_NAME: "test-srv-name", OTEL_RESOURCE_ATTRIBUTES: "service.name=svc-name-from-resource"})
def test_service_name_env() -> None:
    """The service name given in code wins over both environment variables (`test_resources.py:571-583`, the assert of lines 582-583)."""
    resource = detected("from-code", "0")
    assert resource.attributes["service.name"] == "from-code"


def test_shutdown() -> None:
    """Shutting the provider down shuts each of its processors down once, at interpreter exit too and once alone after an explicit shutdown (`test_trace.py:90-155`)."""
    mock_processor1 = mock.Mock(spec=trace.SpanProcessor)
    mock_processor2 = mock.Mock(spec=trace.SpanProcessor)
    tracer_provider = provider("rasm-telemetry-tests", "0", mock_processor1, mock_processor2)
    tracer_provider.shutdown()
    assert mock_processor1.shutdown.call_count == 1
    assert mock_processor2.shutdown.call_count == 1
    shutdown_python_code = """
import atexit
from unittest import mock

from opentelemetry.sdk import trace

from rasm_telemetry.export import provider

mock_processor = mock.Mock(spec=trace.SpanProcessor)

def print_shutdown_count():
    print(mock_processor.shutdown.call_count)

# atexit hooks are called in inverse order they are added, so do this before
# creating the tracer
atexit.register(print_shutdown_count)

tracer_provider = provider("rasm-telemetry-tests", "0", mock_processor)

{tracer_shutdown}
"""

    def run_general_code(*, explicit_shutdown: bool) -> bytes:
        tracer_shutdown = "tracer_provider.shutdown()" if explicit_shutdown else ""
        return subprocess.check_output([sys.executable, "-c", shutdown_python_code.format(tracer_shutdown=tracer_shutdown)])

    out = run_general_code(explicit_shutdown=False)
    assert out.startswith(b"1")
    out = run_general_code(explicit_shutdown=True)
    assert out.startswith(b"1")


def test_telemetry_flushed_before_shutdown_and_dropped_after_shutdown() -> None:
    """Shutdown exports the span ended before it, and a span ended after it is dropped (`test_batch_processor.py:119-137`)."""
    exporter = Mock()
    batch_processor = ManualSpanProcessor(exporter)
    batch_processor.on_end(BASIC_SPAN)
    batch_processor.shutdown()
    exporter.export.assert_called_once_with([BASIC_SPAN])
    batch_processor.on_end(BASIC_SPAN)
    exporter.export.assert_called_once()


def test_force_flush_flushes_telemetry() -> None:
    """`force_flush` hands every held span to the exporter in one call (`test_batch_processor.py:140-154`)."""
    exporter = Mock()
    batch_processor = ManualSpanProcessor(exporter)
    for _ in range(10):
        batch_processor.on_end(BASIC_SPAN)
    batch_processor.force_flush()
    exporter.export.assert_called_once_with([BASIC_SPAN for _ in range(10)])
    batch_processor.shutdown()
