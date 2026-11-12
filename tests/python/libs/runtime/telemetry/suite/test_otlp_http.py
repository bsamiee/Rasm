"""Spans over OTLP/HTTP from the exporter of `rasm_telemetry.otlp.Collector`, and that exporter after `rasm_telemetry.process.instrumented` is left, copied from opentelemetry-exporter-otlp-proto-http `tests/test_proto_span_exporter.py` (1.45.2)."""

from collections.abc import Iterator
from contextlib import AbstractContextManager
from datetime import timedelta
import io
import os
import threading
import time
from typing import Protocol
from unittest.mock import patch

from mocket import Mocket, Mocketizer
from mocket.mocks.mockhttp import Entry, Response
from opentelemetry.exporter.otlp.proto.common.trace_encoder import encode_spans
from opentelemetry.proto.collector.trace.v1.trace_service_pb2 import ExportTraceServiceRequest
from opentelemetry.sdk.trace import ReadableSpan, SpanProcessor, TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor, SpanExportResult
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest

from rasm_telemetry import LogSettings
from rasm_telemetry.otlp import Collector
from rasm_telemetry.process import instrumented

# --- [TYPES] ----------------------------------------------------------------------------


class MakeSpan(Protocol):
    """Call ending one span of a name and returning every span finished so far, the signature of the exemplar's `_make_span`."""

    def __call__(self, name: str = "test-span") -> list[ReadableSpan]:
        """Ends one span and returns the finished spans."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_TEST_ENDPOINT = "http://localhost:4318/v1/traces"
_COLLECTOR = "http://localhost:4318"
_LOGGER_NAME = "opentelemetry.exporter.otlp.proto.http.trace_exporter"
_INTERRUPTED = 0.5
_NO_EXPORTERS = {"OTEL_TRACES_EXPORTER": "none", "OTEL_METRICS_EXPORTER": "none", "OTEL_LOGS_EXPORTER": "none"}

# --- [OPERATIONS] -----------------------------------------------------------------------


def _left(*span_processors: SpanProcessor) -> AbstractContextManager[object]:
    """Returns the process scope with the processors given joined and no exporter named, left where the exemplar calls `exporter.shutdown()`."""
    return instrumented(io.StringIO(), service="rasm-telemetry-tests", version="0", settings=LogSettings(), scopes=(), environ=_NO_EXPORTERS, span_processors=span_processors)


# --- [COMPOSITION] ----------------------------------------------------------------------

pytestmark = pytest.mark.usefixtures("env_patcher")


@pytest.fixture
def env_patcher() -> Iterator[None]:
    """Empties the environment for the test, as the exemplar's `setUp` (`test_proto_span_exporter.py:89-92`)."""
    with patch.dict(os.environ, {}, clear=True):
        yield


@pytest.fixture
def in_memory() -> InMemorySpanExporter:
    """Returns the exporter the test's own spans end in, as `setUp` (`test_proto_span_exporter.py:94`)."""
    return InMemorySpanExporter()


@pytest.fixture
def make_span(in_memory: InMemorySpanExporter) -> MakeSpan:
    """Returns the call ending one span and returning every finished span, as `setUp`, `_finished_spans`, and `_make_span` (`test_proto_span_exporter.py:95-97,102-108`)."""
    provider = TracerProvider()
    provider.add_span_processor(SimpleSpanProcessor(in_memory))
    tracer = provider.get_tracer(__name__)

    def making(name: str = "test-span") -> list[ReadableSpan]:
        with tracer.start_as_current_span(name):
            pass
        return list(in_memory.get_finished_spans())

    return making


@pytest.mark.enable_socket
@pytest.mark.parametrize("env,endpoint", [pytest.param({}, "http://custom.example:9999", id="constructor")])
def test_custom_endpoint(in_memory: InMemorySpanExporter, make_span: MakeSpan, env: dict[str, str], endpoint: str) -> None:
    """The endpoint the collector names receives the export (`test_proto_span_exporter.py:313-336`, row `constructor`)."""
    url = "http://custom.example:9999/v1/traces"
    with patch.dict(os.environ, env, clear=True), Mocketizer(strict_mode=True):
        Entry.single_register(Entry.POST, url, status=200)
        exporter = Collector(endpoint=endpoint).exporter()
        in_memory.clear()
        result = exporter.export(make_span())
        assert result == SpanExportResult.SUCCESS


def test_shutdown_idempotent(caplog: pytest.LogCaptureFixture) -> None:
    """A shutdown after the scope is left warns (`test_proto_span_exporter.py:736-741`)."""
    exporter = Collector(endpoint=_COLLECTOR).exporter()
    with _left(SimpleSpanProcessor(exporter)):
        pass
    caplog.clear()
    with caplog.at_level("WARNING", logger=_LOGGER_NAME):
        exporter.shutdown()
    assert [record for record in caplog.records if record.name == _LOGGER_NAME]


def test_shutdown_closes_transport() -> None:
    """Leaving the scope closes the exporter's transport once (`test_proto_span_exporter.py:744-750`)."""
    exporter = Collector(endpoint=_COLLECTOR).exporter()
    with patch.object(exporter._client._transport, "close") as mock_close, _left(SimpleSpanProcessor(exporter)):
        pass
    mock_close.assert_called_once()


@pytest.mark.enable_socket
def test_export_single_span(make_span: MakeSpan) -> None:
    """One span is posted to the collector's `/v1/traces` path as an OTLP request (`test_proto_span_exporter.py:260-275`)."""
    with Mocketizer(strict_mode=True):
        Entry.single_register(Entry.POST, _TEST_ENDPOINT, status=200)
        exporter = Collector(endpoint=_COLLECTOR).exporter()
        spans = make_span("my-span")
        transport = exporter._client._transport
        with patch.object(transport, "request", wraps=transport.request) as mock_request:
            result = exporter.export(spans)
        assert result == SpanExportResult.SUCCESS
        request = Mocket.last_request()
        assert request.method == "POST"
        assert request.path == "/v1/traces"
        sent_data = mock_request.call_args.kwargs["data"]
        assert ExportTraceServiceRequest.FromString(sent_data) == encode_spans(spans)


@pytest.mark.enable_socket
def test_shutdown_interrupts_retry_backoff(make_span: MakeSpan) -> None:
    """Shutting the exporter down ends an export waiting to retry (`test_proto_span_exporter.py:633-649`)."""
    with Mocketizer(strict_mode=True):
        Entry.register(Entry.POST, _TEST_ENDPOINT, *[Response(status=503)] * 6)
        exporter = Collector(endpoint=_COLLECTOR, timeout=timedelta(seconds=1.5)).exporter()
        thread = threading.Thread(target=exporter.export, args=(make_span(),))
        before = time.time()
        thread.start()
        time.sleep(0.05)
        exporter.shutdown()
        thread.join()
        after = time.time()
        assert after - before < _INTERRUPTED


@pytest.mark.enable_socket
def test_export_after_shutdown(make_span: MakeSpan, caplog: pytest.LogCaptureFixture) -> None:
    """An export after the scope is left fails with a warning and sends nothing (`test_proto_span_exporter.py:725-734`)."""
    with Mocketizer(strict_mode=True):
        exporter = Collector(endpoint=_COLLECTOR).exporter()
        with _left(SimpleSpanProcessor(exporter)):
            pass
        caplog.clear()
        with caplog.at_level("WARNING", logger=_LOGGER_NAME):
            result = exporter.export(make_span())
        assert [record for record in caplog.records if record.name == _LOGGER_NAME]
        assert result == SpanExportResult.FAILURE
        assert Mocket.last_request() is None


@pytest.mark.enable_socket
def test_force_flush(make_span: MakeSpan) -> None:
    """The exporter's `force_flush` answers true before and after an export (`test_proto_span_exporter.py:752-759`)."""
    with Mocketizer(strict_mode=True):
        Entry.single_register(Entry.POST, _TEST_ENDPOINT, status=200)
        exporter = Collector(endpoint=_COLLECTOR).exporter()
        assert exporter.force_flush()
        exporter.export(make_span())
        assert exporter.force_flush()
