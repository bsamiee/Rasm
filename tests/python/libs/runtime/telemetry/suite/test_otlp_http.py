"""Spans over OTLP/HTTP from the exporter of `rasm_telemetry.otlp.Collector`, and that exporter after `rasm_telemetry.process.instrumented` is left, copied from opentelemetry-exporter-otlp-proto-http `tests/test_proto_span_exporter.py` (1.45.2)."""

from collections.abc import Callable, Generator, Iterator
from contextlib import AbstractContextManager, contextmanager
from functools import partial
import os
from pathlib import Path
import threading
import time
from typing import Protocol
from unittest.mock import patch

from mocket import Mocket, Mocketizer
from mocket.mocks.mockhttp import Entry, Response
from opentelemetry.exporter.otlp.common.http import Compression
from opentelemetry.exporter.otlp.proto.common.trace_encoder import encode_spans
from opentelemetry.exporter.otlp.proto.http._common import _DEFAULT_TIMEOUT
from opentelemetry.exporter.otlp.proto.http.trace_exporter import DEFAULT_ENDPOINT, DEFAULT_TRACES_EXPORT_PATH
from opentelemetry.exporter.otlp.proto.http.version import __version__
from opentelemetry.proto.collector.trace.v1.trace_service_pb2 import ExportTraceServiceRequest
from opentelemetry.sdk.environment_variables import (
    OTEL_EXPORTER_OTLP_COMPRESSION,
    OTEL_EXPORTER_OTLP_ENDPOINT,
    OTEL_EXPORTER_OTLP_HEADERS,
    OTEL_EXPORTER_OTLP_TIMEOUT,
    OTEL_EXPORTER_OTLP_TRACES_COMPRESSION,
    OTEL_EXPORTER_OTLP_TRACES_ENDPOINT,
    OTEL_EXPORTER_OTLP_TRACES_HEADERS,
    OTEL_EXPORTER_OTLP_TRACES_TIMEOUT,
)
from opentelemetry.sdk.trace import ReadableSpan, SpanProcessor, TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor, SpanExportResult
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest

from rasm_telemetry import LogSettings
from rasm_telemetry.otlp import Collector
from rasm_telemetry.process import instrumented

# --- [TYPES] ----------------------------------------------------------------------------

type Left = Callable[[SpanProcessor], AbstractContextManager[object]]


class MakeSpan(Protocol):
    """Call ending one span of a name and returning every span finished so far, the signature of the exemplar's `_make_span`."""

    def __call__(self, name: str = "test-span") -> list[ReadableSpan]:
        """Ends one span and returns the finished spans."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_TEST_ENDPOINT = "http://localhost:4318/v1/traces"
_LOGGER_NAME = "opentelemetry.exporter.otlp.proto.http.trace_exporter"
_USER_AGENT = "OTel-OTLP-Exporter-Python/" + __version__
_BASE_HEADERS = {"content-type": "application/x-protobuf", "user-agent": _USER_AGENT}
_COMMON_ENV = {
    OTEL_EXPORTER_OTLP_ENDPOINT: "http://common.example:4318",
    OTEL_EXPORTER_OTLP_HEADERS: "common-key=common-value",
    OTEL_EXPORTER_OTLP_COMPRESSION: "gzip",
    OTEL_EXPORTER_OTLP_TIMEOUT: "5",
}
_SIGNAL_ENV = {
    OTEL_EXPORTER_OTLP_TRACES_ENDPOINT: "http://signal.example:4318/v1/traces",
    OTEL_EXPORTER_OTLP_TRACES_HEADERS: "signal-key=signal-value",
    OTEL_EXPORTER_OTLP_TRACES_COMPRESSION: "deflate",
    OTEL_EXPORTER_OTLP_TRACES_TIMEOUT: "7",
}
_INTERRUPTED = 0.5

# --- [COMPOSITION] ----------------------------------------------------------------------

pytestmark = pytest.mark.usefixtures("env_patcher")


@pytest.fixture
def env_patcher() -> Iterator[None]:
    """Empties the environment for the test but for a proxy bypass, as the exemplar's `setUp` (`test_proto_span_exporter.py:89-92`)."""
    with patch.dict(os.environ, {"no_proxy": "*"}, clear=True):
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


@pytest.fixture
def left(tmp_path: Path) -> Left:
    """Returns the call making the process scope over a log file with one processor joined and no exporter named, left where the exemplar calls `exporter.shutdown()`."""
    environ = {"OTEL_TRACES_EXPORTER": "none", "OTEL_METRICS_EXPORTER": "none", "OTEL_LOGS_EXPORTER": "none", "no_proxy": "*"}

    @contextmanager
    def leaving(processor: SpanProcessor) -> Generator[object]:
        with (
            patch.dict(os.environ, environ, clear=True),
            instrumented(tmp_path / "scope.log", service="rasm-telemetry-tests", version="0", settings=LogSettings(), scopes=(), span_processors=(processor,)) as scope,
        ):
            yield scope

    return leaving


@pytest.mark.enable_socket
def test_export_single_span(make_span: MakeSpan) -> None:
    """One span is posted to the endpoint as an OTLP request (`test_proto_span_exporter.py:260-275`)."""
    with Mocketizer(strict_mode=True):
        Entry.single_register(Entry.POST, _TEST_ENDPOINT, status=200)
        exporter = Collector(endpoint=_TEST_ENDPOINT).exporter()
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
def test_default_endpoint_and_headers(make_span: MakeSpan) -> None:
    """A collector made with no argument posts to the default endpoint with the OTLP headers (`test_proto_span_exporter.py:301-311`)."""
    with Mocketizer(strict_mode=True):
        Entry.single_register(Entry.POST, "http://localhost:4318/v1/traces", status=200)
        exporter = Collector().exporter()
        result = exporter.export(make_span())
        assert result == SpanExportResult.SUCCESS
        headers = Mocket.last_request().headers
        assert headers["content-type"] == "application/x-protobuf"
        assert headers["user-agent"].startswith("OTel-OTLP-Exporter-Python/")


@pytest.mark.enable_socket
@pytest.mark.parametrize(
    "env,making",
    [
        pytest.param({}, partial(Collector, endpoint="http://custom.example:9999/v1/traces"), id="constructor"),
        pytest.param({OTEL_EXPORTER_OTLP_ENDPOINT: "http://custom.example:9999"}, Collector, id="generic_env"),
        pytest.param({OTEL_EXPORTER_OTLP_TRACES_ENDPOINT: "http://custom.example:9999/v1/traces"}, Collector, id="per_signal_env"),
    ],
)
def test_custom_endpoint(in_memory: InMemorySpanExporter, make_span: MakeSpan, env: dict[str, str], making: Callable[[], Collector]) -> None:
    """The endpoint the constructor or an OTLP endpoint variable names receives the export (`test_proto_span_exporter.py:313-336`)."""
    url = "http://custom.example:9999/v1/traces"
    with patch.dict(os.environ, env), Mocketizer(strict_mode=True):
        Entry.single_register(Entry.POST, url, status=200)
        exporter = making().exporter()
        in_memory.clear()
        result = exporter.export(make_span())
        assert result == SpanExportResult.SUCCESS


@pytest.mark.parametrize(
    "env,making,expected",
    [
        pytest.param(
            {}, Collector, {"endpoint": DEFAULT_ENDPOINT + DEFAULT_TRACES_EXPORT_PATH, "compression": Compression.NONE, "headers": _BASE_HEADERS, "timeout": float(_DEFAULT_TIMEOUT)}, id="defaults"
        ),
        pytest.param(
            _COMMON_ENV,
            Collector,
            {
                "endpoint": "http://common.example:4318/v1/traces",
                "compression": Compression.GZIP,
                "headers": {**_BASE_HEADERS, "common-key": "common-value", "Content-Encoding": "gzip"},
                "timeout": 5.0,
            },
            id="common_env",
        ),
        pytest.param(
            {**_COMMON_ENV, **_SIGNAL_ENV},
            Collector,
            {
                "endpoint": "http://signal.example:4318/v1/traces",
                "compression": Compression.DEFLATE,
                "headers": {**_BASE_HEADERS, "signal-key": "signal-value", "Content-Encoding": "deflate"},
                "timeout": 7.0,
            },
            id="signal_env_overrides_common",
        ),
        pytest.param(
            {**_COMMON_ENV, **_SIGNAL_ENV},
            partial(Collector, endpoint="http://explicit.example:4318/v1/traces", headers={"explicit-key": "explicit-value"}, compression=Compression.NONE, timeout=9),
            {
                "endpoint": "http://explicit.example:4318/v1/traces",
                "compression": Compression.NONE,
                "headers": {**_BASE_HEADERS, "signal-key": "signal-value", "explicit-key": "explicit-value"},
                "timeout": 9.0,
            },
            id="constructor_overrides_all",
        ),
    ],
)
def test_configuration_precedence(env: dict[str, str], making: Callable[[], Collector], expected: dict[str, object]) -> None:
    """The exporter takes endpoint, compression, headers, and timeout from the OTLP variables, the trace ones over the common ones, and the collector's own arguments over both (`test_proto_span_exporter.py:367-450`)."""
    with patch.dict(os.environ, env, clear=True):
        exporter = making().exporter()
        assert exporter._endpoint == expected["endpoint"]
        assert exporter._compression is expected["compression"]
        assert exporter._client._headers == expected["headers"]
        assert exporter._client._timeout == expected["timeout"]


@pytest.mark.enable_socket
def test_shutdown_interrupts_retry_backoff(make_span: MakeSpan) -> None:
    """Shutting the exporter down ends an export waiting to retry (`test_proto_span_exporter.py:633-649`)."""
    with Mocketizer(strict_mode=True):
        Entry.register(Entry.POST, _TEST_ENDPOINT, *[Response(status=503)] * 6)
        exporter = Collector(endpoint=_TEST_ENDPOINT, timeout=1.5).exporter()
        thread = threading.Thread(target=exporter.export, args=(make_span(),))
        before = time.time()
        thread.start()
        time.sleep(0.05)
        exporter.shutdown()
        thread.join()
        after = time.time()
        assert after - before < _INTERRUPTED


@pytest.mark.enable_socket
def test_export_after_shutdown(make_span: MakeSpan, left: Left, caplog: pytest.LogCaptureFixture) -> None:
    """An export after the scope is left fails with a warning and sends nothing (`test_proto_span_exporter.py:725-734`)."""
    with Mocketizer(strict_mode=True):
        Entry.single_register(Entry.POST, _TEST_ENDPOINT, status=200)
        exporter = Collector(endpoint=_TEST_ENDPOINT).exporter()
        with left(SimpleSpanProcessor(exporter)):
            pass
        caplog.clear()
        with caplog.at_level("WARNING", logger=_LOGGER_NAME):
            result = exporter.export(make_span())
        assert [record for record in caplog.records if record.name == _LOGGER_NAME]
        assert result == SpanExportResult.FAILURE
        assert Mocket.last_request() is None


def test_shutdown_idempotent(left: Left, caplog: pytest.LogCaptureFixture) -> None:
    """A shutdown after the scope is left warns (`test_proto_span_exporter.py:736-741`)."""
    exporter = Collector(endpoint=_TEST_ENDPOINT).exporter()
    with left(SimpleSpanProcessor(exporter)):
        pass
    caplog.clear()
    with caplog.at_level("WARNING", logger=_LOGGER_NAME):
        exporter.shutdown()
    assert [record for record in caplog.records if record.name == _LOGGER_NAME]


def test_shutdown_closes_transport(left: Left) -> None:
    """Leaving the scope closes the exporter's transport once (`test_proto_span_exporter.py:744-750`)."""
    exporter = Collector(endpoint=_TEST_ENDPOINT).exporter()
    with patch.object(exporter._client._transport, "close") as mock_close, left(SimpleSpanProcessor(exporter)):
        pass
    mock_close.assert_called_once()
