"""Span export over OTLP/gRPC from an instrumented process to a trace service the test hosts, each case from opentelemetry-python `test_otlp_exporter_mixin.py` or `test_otlp_trace_exporter.py` at v1.45.2, or from the OTLP specification's OTLP/gRPC sections."""

from collections.abc import Callable, Iterator, Mapping, Sequence
from contextlib import ExitStack
from datetime import timedelta
import functools
import time
from typing import Protocol

from anyio.from_thread import BlockingPortal
from google.rpc.error_details_pb2 import RetryInfo
import grpc
import grpc.aio
import msgspec
from opentelemetry.environment_variables import OTEL_TRACES_EXPORTER
from opentelemetry.proto.collector.trace.v1 import trace_service_pb2
from opentelemetry.proto.collector.trace.v1.trace_service_pb2 import ExportTraceServiceRequest, ExportTraceServiceResponse
from opentelemetry.sdk.environment_variables import (
    OTEL_EXPORTER_OTLP_ENDPOINT,
    OTEL_EXPORTER_OTLP_PROTOCOL,
    OTEL_EXPORTER_OTLP_TIMEOUT,
    OTEL_EXPORTER_OTLP_TRACES_ENDPOINT,
    OTEL_EXPORTER_OTLP_TRACES_HEADERS,
)
import pytest

# --- [TYPES] ----------------------------------------------------------------------------

type Exported = Callable[[Mapping[str, str]], None]


class Traced(Protocol):
    """Start of an OTLP trace service inside the scope of a test."""

    def __call__(self, code: grpc.StatusCode, /, *, details: str = "", retry: timedelta | None = None) -> tuple[str, Sequence[Call]]:
        """Starts a service ending every export with one status, and returns its endpoint and the exports it received."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

GRPC = frozendict({OTEL_TRACES_EXPORTER: "otlp_proto_grpc"})
ATTEMPTS = 6
THROTTLED = timedelta(milliseconds=200)

# --- [MODELS] ---------------------------------------------------------------------------


class Call(msgspec.Struct, frozen=True, kw_only=True):
    """Export a trace service received: its request, its metadata, and the monotonic clock at its arrival."""

    request: ExportTraceServiceRequest
    metadata: Mapping[str, str | bytes]
    at: timedelta


# --- [OPERATIONS] -----------------------------------------------------------------------


def collecting(stack: ExitStack, portal: BlockingPortal, code: grpc.StatusCode, /, *, details: str = "", retry: timedelta | None = None) -> tuple[str, Sequence[Call]]:
    """Serves the OTLP trace service on a loopback port, ending every export with one status, a RetryInfo delay in its trailers where `retry` is set, until the exit stack's scope ends.

    Returns:
        The service's `http://` endpoint and the exports it received so far, in arrival order.
    """
    calls: list[Call] = []

    def export(request: ExportTraceServiceRequest, context: grpc.aio.ServicerContext[ExportTraceServiceRequest, ExportTraceServiceResponse]) -> ExportTraceServiceResponse:
        calls.append(Call(request=request, metadata=dict(context.invocation_metadata() or ()), at=timedelta(seconds=time.monotonic())))
        if retry is not None:
            delay = RetryInfo()
            delay.retry_delay.FromTimedelta(retry)
            context.set_trailing_metadata((("google.rpc.retryinfo-bin", delay.SerializeToString()),))
        context.set_code(code)
        context.set_details(details)
        return ExportTraceServiceResponse()

    handler = grpc.unary_unary_rpc_method_handler(export, request_deserializer=ExportTraceServiceRequest.FromString, response_serializer=ExportTraceServiceResponse.SerializeToString)
    server = portal.call(grpc.aio.server)
    server.add_generic_rpc_handlers((grpc.method_handlers_generic_handler(trace_service_pb2.DESCRIPTOR.services_by_name["TraceService"].full_name, {"Export": handler}),))
    port = server.add_insecure_port("127.0.0.1:0")
    portal.call(server.start)
    stack.callback(portal.call, server.stop, None)
    return f"http://127.0.0.1:{port}", calls


@pytest.mark.parametrize(
    "named", [pytest.param(GRPC, id="otlp_proto_grpc"), pytest.param(frozendict({OTEL_TRACES_EXPORTER: "otlp", OTEL_EXPORTER_OTLP_PROTOCOL: "grpc"}), id="otlp-under-grpc-protocol")]
)
def test_a_span_reaches_the_trace_service_over_an_http_scheme_endpoint(traced: Traced, exported: Exported, named: frozendict[str, str]) -> None:
    """`test_otlp_exporter_endpoint` row `http://localhost:4317`, "OTLP/gRPC Request": an `http://` endpoint is dialed in plaintext and one unary `Export` carries the span."""
    endpoint, calls = traced(grpc.StatusCode.OK)
    exported(named | {OTEL_EXPORTER_OTLP_ENDPOINT: endpoint})
    (call,) = calls
    assert [span.name for group in call.request.resource_spans for scope in group.scope_spans for span in scope.spans] == ["a"]


def test_a_call_carries_the_headers_its_variables_name_and_the_exporter_as_its_agent(traced: Traced, exported: Exported) -> None:
    """`test_env_variables`, `test_otlp_headers_from_env`, `test_otlp_exporter_otlp_compression_unspecified`: header keys arrive in lower case with their values kept, under the exporter's user agent."""
    endpoint, calls = traced(grpc.StatusCode.OK)
    exported(GRPC | {OTEL_EXPORTER_OTLP_TRACES_ENDPOINT: endpoint, OTEL_EXPORTER_OTLP_TRACES_HEADERS: "key1=value1,KEY2=VALUE2"})
    (call,) = calls
    assert (call.metadata["key1"], call.metadata["key2"]) == ("value1", "VALUE2")
    assert str(call.metadata["user-agent"]).startswith("OTel-OTLP-Exporter-Python/")


def test_retries_wait_the_delay_of_retry_info(traced: Traced, exported: Exported) -> None:
    """`test_retry_info_is_respected`, "OTLP/gRPC Throttling": UNAVAILABLE with a 0.2 s RetryInfo is called six times in about one second."""
    endpoint, calls = traced(grpc.StatusCode.UNAVAILABLE, retry=THROTTLED)
    exported(GRPC | {OTEL_EXPORTER_OTLP_ENDPOINT: endpoint, OTEL_EXPORTER_OTLP_TIMEOUT: "10"})
    assert len(calls) == ATTEMPTS
    assert abs(calls[-1].at - calls[0].at - THROTTLED * (ATTEMPTS - 1)) < timedelta(seconds=0.5)


def test_a_retry_past_the_timeout_is_not_made(traced: Traced, exported: Exported) -> None:
    """`test_retry_not_made_if_would_exceed_timeout`: under a 4 s timeout UNAVAILABLE is called at 0 s, 1 s, and 3 s within the 20% jitter, and the 4 s backoff after them is not waited."""
    endpoint, calls = traced(grpc.StatusCode.UNAVAILABLE)
    exported(GRPC | {OTEL_EXPORTER_OTLP_ENDPOINT: endpoint, OTEL_EXPORTER_OTLP_TIMEOUT: "4"})
    first, _, third = calls
    assert timedelta(seconds=2.35) < third.at - first.at < timedelta(seconds=3.65)


def test_a_permanent_failure_is_called_once(traced: Traced, exported: Exported) -> None:
    """`test_permanent_failure`: ALREADY_EXISTS with its details ends the export at the first call."""
    endpoint, calls = traced(grpc.StatusCode.ALREADY_EXISTS, details="This already exists.")
    exported(GRPC | {OTEL_EXPORTER_OTLP_ENDPOINT: endpoint, OTEL_EXPORTER_OTLP_TIMEOUT: "10"})
    assert len(calls) == 1


@pytest.mark.parametrize(
    "code",
    [
        grpc.StatusCode.CANCELLED,
        grpc.StatusCode.DEADLINE_EXCEEDED,
        grpc.StatusCode.RESOURCE_EXHAUSTED,
        grpc.StatusCode.ABORTED,
        grpc.StatusCode.OUT_OF_RANGE,
        grpc.StatusCode.UNAVAILABLE,
        grpc.StatusCode.DATA_LOSS,
    ],
)
def test_a_retryable_code_is_called_again(traced: Traced, exported: Exported, code: grpc.StatusCode) -> None:
    """OTLP "Failures" table, `test_retryable_error_codes_custom`: each retryable code, RESOURCE_EXHAUSTED under a RetryInfo, is called until the six attempts end."""
    endpoint, calls = traced(code, retry=timedelta(milliseconds=10))
    exported(GRPC | {OTEL_EXPORTER_OTLP_ENDPOINT: endpoint, OTEL_EXPORTER_OTLP_TIMEOUT: "10"})
    assert len(calls) == ATTEMPTS


@pytest.mark.parametrize(
    "code",
    [
        grpc.StatusCode.UNKNOWN,
        grpc.StatusCode.INVALID_ARGUMENT,
        grpc.StatusCode.NOT_FOUND,
        grpc.StatusCode.PERMISSION_DENIED,
        grpc.StatusCode.UNAUTHENTICATED,
        grpc.StatusCode.FAILED_PRECONDITION,
        grpc.StatusCode.UNIMPLEMENTED,
        grpc.StatusCode.INTERNAL,
    ],
)
def test_a_code_outside_the_retryable_rows_is_called_once(traced: Traced, exported: Exported, code: grpc.StatusCode) -> None:
    """OTLP "Failures" table: a non-retryable code MUST NOT be sent again, a RetryInfo beside it included."""
    endpoint, calls = traced(code, retry=timedelta(milliseconds=10))
    exported(GRPC | {OTEL_EXPORTER_OTLP_ENDPOINT: endpoint, OTEL_EXPORTER_OTLP_TIMEOUT: "10"})
    assert len(calls) == 1


def test_resource_exhausted_without_retry_info_is_called_once(traced: Traced, exported: Exported) -> None:
    """OTLP "Failures": RESOURCE_EXHAUSTED is retryable only where the server signals recovery through a RetryInfo, and SHOULD be treated as non-retryable without one."""
    endpoint, calls = traced(grpc.StatusCode.RESOURCE_EXHAUSTED)
    exported(GRPC | {OTEL_EXPORTER_OTLP_ENDPOINT: endpoint, OTEL_EXPORTER_OTLP_TIMEOUT: "3"})
    assert len(calls) == 1


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def traced(portal: BlockingPortal) -> Iterator[Traced]:
    """Yields the start of an OTLP trace service ending every export with one status, which returns its endpoint and the exports it received, stopped after the test."""
    with ExitStack() as stack:
        yield functools.partial(collecting, stack, portal)
