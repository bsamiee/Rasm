"""Span export over OTLP/HTTP against a collector the test hosts, each case from opentelemetry-python `test_proto_span_exporter.py` or `test_common.py` at v1.45.2, or from the OTLP specification's OTLP/HTTP sections."""

from collections.abc import Callable, Iterator, Mapping, Sequence
from contextlib import ExitStack
from datetime import datetime, timedelta, UTC
from email.utils import format_datetime
import functools
import gzip
from http import HTTPStatus
import logging
import socket
import time
import zlib

import anyio
from anyio.from_thread import BlockingPortal
from google.rpc.status_pb2 import Status
import msgspec
from opentelemetry.environment_variables import OTEL_TRACES_EXPORTER
from opentelemetry.exporter.otlp.proto.common.trace_encoder import encode_spans
from opentelemetry.exporter.otlp.proto.http import trace_exporter
from opentelemetry.proto.collector.trace.v1.trace_service_pb2 import ExportTracePartialSuccess, ExportTraceServiceRequest, ExportTraceServiceResponse
from opentelemetry.sdk.environment_variables import (
    OTEL_EXPORTER_OTLP_COMPRESSION,
    OTEL_EXPORTER_OTLP_ENDPOINT,
    OTEL_EXPORTER_OTLP_HEADERS,
    OTEL_EXPORTER_OTLP_TRACES_COMPRESSION,
    OTEL_EXPORTER_OTLP_TRACES_ENDPOINT,
    OTEL_EXPORTER_OTLP_TRACES_HEADERS,
)
from opentelemetry.sdk.trace import ReadableSpan, TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor, SpanExportResult
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest

from rasm_telemetry.otlp import Collector
from rasm_testing.hosting import Answer, Hosted, Received

# --- [TYPES] ----------------------------------------------------------------------------

type Answered = Callable[[*tuple[Answer, ...]], tuple[str, Sequence[Received]]]
type Exporting = Callable[[str, timedelta], trace_exporter.OTLPSpanExporter]
type Exported = Callable[[Mapping[str, str]], None]

# --- [CONSTANTS] ------------------------------------------------------------------------

PROTOBUF = frozendict({"content-type": "application/x-protobuf"})

LONG = timedelta(seconds=30)
SHORT = timedelta(seconds=1.5)
REFUSED = Answer(status=503)

# --- [OPERATIONS] -----------------------------------------------------------------------


def answering(hosted: Hosted, /, *answers: Answer) -> tuple[str, Sequence[Received]]:
    """Hosts an OTLP/HTTP collector answering one script entry per request in order and the last entry ever after, each under the protobuf content type and a failure holding no body with a `google.rpc.Status`.

    Returns:
        The collector's base URL and the requests it received so far, in arrival order.
    """
    failed = Status(message="refused").SerializeToString()
    told = tuple(msgspec.structs.replace(answer, headers={**PROTOBUF, **answer.headers}, body=answer.body or (failed if answer.status >= HTTPStatus.BAD_REQUEST else b"")) for answer in answers)
    remaining = iter(told)
    return hosted(lambda _request: next(remaining, told[-1]))


def posting(stack: ExitStack, endpoint: str, timeout: timedelta, /) -> trace_exporter.OTLPSpanExporter:
    """Returns the OTLP/HTTP span exporter the library builds for a collector at a base URL, each export bounded by `timeout`, shut down when the exit stack's scope ends."""
    exporter = Collector(endpoint=endpoint, timeout=timeout).exporter()
    stack.callback(exporter.shutdown)
    return exporter


def test_a_span_is_posted_to_the_traces_path_as_its_protobuf_request(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_export_single_span`, "OTLP/HTTP Request": one POST to `/v1/traces` whose body decodes to the request the spans encode to."""
    url, received = answered(Answer())
    assert exporting(url, LONG).export(spans) is SpanExportResult.SUCCESS
    (request,) = received
    assert (request.method, request.path) == ("POST", "/v1/traces")
    assert ExportTraceServiceRequest.FromString(request.body) == encode_spans(spans)


def test_a_request_states_the_protobuf_content_type_and_the_exporter_as_its_agent(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_default_endpoint_and_headers`, "Binary Protobuf Encoding": the content type is `application/x-protobuf` and the user agent names the OTLP exporter."""
    url, received = answered(Answer())
    assert exporting(url, LONG).export(spans) is SpanExportResult.SUCCESS
    assert received[0].headers["content-type"] == "application/x-protobuf"
    assert received[0].headers["user-agent"].startswith("OTel-OTLP-Exporter-Python/")


@pytest.mark.parametrize("closing", [pytest.param("", id="base_no_trailing_slash"), pytest.param("/", id="base_trailing_slash_normalized")])
def test_a_base_endpoint_reaches_the_traces_path_with_or_without_its_closing_slash(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan], closing: str) -> None:
    """`test_resolve_endpoint` rows `base_no_trailing_slash` and `base_trailing_slash_normalized`: either form of a base URL posts to `/v1/traces`."""
    url, received = answered(Answer())
    assert exporting(f"{url}{closing}", LONG).export(spans) is SpanExportResult.SUCCESS
    assert [request.path for request in received] == ["/v1/traces"]


@pytest.mark.parametrize("status", [429, 502, 503, 504])
def test_a_retryable_status_is_sent_again(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan], status: int) -> None:
    """`test_export_retryable_status_codes`, "Retryable Response Codes": 429, 502, 503, and 504 are retried, and the export succeeds on the second answer."""
    url, received = answered(Answer(status=status), Answer())
    assert exporting(url, LONG).export(spans) is SpanExportResult.SUCCESS
    assert len(received) == 2


@pytest.mark.parametrize("status", [400, 401, 403, 404, 408, 500, 501])
def test_any_other_failure_status_is_sent_once(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan], status: int) -> None:
    """`test_export_non_retryable_status_codes`, "Retryable Response Codes": every other 4xx and 5xx status fails the export without a retry."""
    url, received = answered(Answer(status=status))
    assert exporting(url, LONG).export(spans) is SpanExportResult.FAILURE
    assert len(received) == 1


def test_retries_end_at_the_sixth_request(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_export_max_retries`: a collector answering 503 without end receives six requests, each wait cut to nothing by `Retry-After: 0` where the exemplar mocks the clock."""
    url, received = answered(Answer(status=503, headers={"retry-after": "0"}))
    assert exporting(url, timedelta(seconds=1000)).export(spans) is SpanExportResult.FAILURE
    assert len(received) == 6


def test_a_retry_waits_the_seconds_of_retry_after(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_export_retry_after_header`, "OTLP/HTTP Throttling": the retry of a 429 comes no sooner than its `Retry-After` seconds, here longer than the first backoff."""
    url, received = answered(Answer(status=429, headers={"retry-after": "2"}), Answer())
    assert exporting(url, timedelta(seconds=60)).export(spans) is SpanExportResult.SUCCESS
    first, second = received
    assert second.at - first.at >= timedelta(seconds=2)


def test_a_retry_waits_until_the_date_of_retry_after(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_export_retry_after_header_http_date`, "OTLP/HTTP Throttling": the retry of a 503 waits for the HTTP-date of its `Retry-After`, a whole second between 3 s and 4 s ahead."""
    url, received = answered(Answer(status=503, headers={"retry-after": format_datetime(datetime.now(UTC) + timedelta(seconds=4), usegmt=True)}), Answer())
    assert exporting(url, timedelta(seconds=120)).export(spans) is SpanExportResult.SUCCESS
    first, second = received
    assert second.at - first.at >= timedelta(seconds=2.5)


def test_a_retry_past_the_timeout_is_not_made(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_retry_timeout_records_metrics`: under a 1.5 s timeout a collector answering 503 gets the request and its first retry, and the 2 s backoff after them is not waited."""
    url, received = answered(REFUSED)
    began = time.monotonic()
    assert exporting(url, SHORT).export(spans) is SpanExportResult.FAILURE
    assert timedelta(seconds=time.monotonic() - began) < SHORT
    assert len(received) == 2


@pytest.mark.enable_socket
def test_an_unreachable_collector_fails_the_export_within_its_timeout(exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_export_connection_error_records_metrics`, "OTLP/HTTP Connection": refused connections are retried with backoff and the export fails once the timeout has no room for the next one."""
    with socket.socket() as unused:
        unused.bind(("127.0.0.1", 0))
        port = unused.getsockname()[1]
    began = time.monotonic()
    assert exporting(f"http://127.0.0.1:{port}", SHORT).export(spans) is SpanExportResult.FAILURE
    assert timedelta(seconds=time.monotonic() - began) < SHORT


def test_a_shutdown_ends_the_wait_before_a_retry(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan], portal: BlockingPortal) -> None:
    """`test_shutdown_interrupts_retry_backoff`: an export waiting out its first backoff returns within half a second of a shutdown issued 50 ms in."""
    url, _ = answered(REFUSED)
    exporter = exporting(url, SHORT)
    began = time.monotonic()
    running = portal.start_task_soon(anyio.to_thread.run_sync, exporter.export, spans)
    portal.call(anyio.sleep, 0.05)
    exporter.shutdown()
    assert running.result() is SpanExportResult.FAILURE
    assert timedelta(seconds=time.monotonic() - began) < SHORT / 3


def test_an_export_after_shutdown_sends_nothing(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """`test_export_after_shutdown`: a shut down exporter fails the batch and no request leaves it."""
    url, received = answered(Answer())
    exporter = exporting(url, LONG)
    exporter.shutdown()
    assert exporter.export(spans) is SpanExportResult.FAILURE
    assert not received


def test_a_second_shutdown_is_a_warning(answered: Answered, exporting: Exporting, caplog: pytest.LogCaptureFixture) -> None:
    """`test_shutdown_idempotent`: shutting an exporter down twice logs a warning from the trace exporter and raises nothing."""
    url, _ = answered(Answer())
    exporter = exporting(url, LONG)
    exporter.shutdown()
    with caplog.at_level(logging.WARNING, logger=trace_exporter.__name__):
        exporter.shutdown()
    assert [record.levelno for record in caplog.records if record.name == trace_exporter.__name__] == [logging.WARNING]


def test_a_partial_success_is_not_sent_again(answered: Answered, exporting: Exporting, spans: Sequence[ReadableSpan]) -> None:
    """OTLP "Partial Success": a 200 whose response rejects one span is final, and the client MUST NOT retry the request."""
    rejected = ExportTraceServiceResponse(partial_success=ExportTracePartialSuccess(rejected_spans=1, error_message="one span rejected"))
    url, received = answered(Answer(body=rejected.SerializeToString()))
    assert exporting(url, LONG).export(spans) is SpanExportResult.SUCCESS
    assert len(received) == 1


@pytest.mark.parametrize(
    "variable,path,expected",
    [pytest.param(OTEL_EXPORTER_OTLP_ENDPOINT, "", "/v1/traces", id="generic_env"), pytest.param(OTEL_EXPORTER_OTLP_TRACES_ENDPOINT, "/custom/traces", "/custom/traces", id="per_signal_env")],
)
def test_a_process_exports_to_the_endpoint_its_variables_name(answered: Answered, exported: Exported, variable: str, path: str, expected: str) -> None:
    """`test_custom_endpoint`, `test_resolve_endpoint` row `per_signal_verbatim`: the base variable gains the traces path, the traces variable is used as written."""
    url, received = answered(Answer())
    exported({OTEL_TRACES_EXPORTER: "otlp_proto_http", variable: f"{url}{path}"})
    assert [request.path for request in received] == [expected]
    assert [span.name for group in ExportTraceServiceRequest.FromString(received[0].body).resource_spans for scope in group.scope_spans for span in scope.spans] == ["a"]


@pytest.mark.parametrize("variable", [pytest.param(OTEL_EXPORTER_OTLP_HEADERS, id="generic_env"), pytest.param(OTEL_EXPORTER_OTLP_TRACES_HEADERS, id="per_signal_env")])
def test_a_process_sends_the_headers_its_variables_name(answered: Answered, exported: Exported, variable: str) -> None:
    """`test_custom_headers`: a header named by the base or the traces variable arrives on the export request."""
    url, received = answered(Answer())
    exported({OTEL_TRACES_EXPORTER: "otlp_proto_http", OTEL_EXPORTER_OTLP_ENDPOINT: url, variable: "x-api-key=secret"})
    assert received[0].headers["x-api-key"] == "secret"


@pytest.mark.parametrize(
    "variable,encoding,restored",
    [pytest.param(OTEL_EXPORTER_OTLP_COMPRESSION, "gzip", gzip.decompress, id="common_env"), pytest.param(OTEL_EXPORTER_OTLP_TRACES_COMPRESSION, "deflate", zlib.decompress, id="signal_env")],
)
def test_a_process_compresses_as_its_variables_name(answered: Answered, exported: Exported, variable: str, encoding: str, restored: Callable[[bytes], bytes]) -> None:
    """`test_compression_options`, `test_configuration_precedence`, "OTLP/HTTP Request": a compressed body states its `Content-Encoding` and restores to the export request."""
    url, received = answered(Answer())
    exported({OTEL_TRACES_EXPORTER: "otlp_proto_http", OTEL_EXPORTER_OTLP_ENDPOINT: url, variable: encoding})
    assert received[0].headers["content-encoding"] == encoding
    assert ExportTraceServiceRequest.FromString(restored(received[0].body)).resource_spans


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def spans() -> Sequence[ReadableSpan]:
    """Returns one finished span `test-span` of an SDK tracer provider."""
    kept = InMemorySpanExporter()
    tracing = TracerProvider(shutdown_on_exit=False)
    tracing.add_span_processor(SimpleSpanProcessor(kept))
    tracing.get_tracer("suite").start_span("test-span").end()
    return kept.get_finished_spans()


@pytest.fixture
def answered(hosted: Hosted) -> Answered:
    """Returns the start of an OTLP/HTTP collector answering from a script of answers, which returns its base URL and the requests it received."""
    return functools.partial(answering, hosted)


@pytest.fixture
def exporting() -> Iterator[Exporting]:
    """Yields the library's OTLP/HTTP span exporter for a collector base URL and an export timeout, shut down after the test."""
    with ExitStack() as stack:
        yield functools.partial(posting, stack)
