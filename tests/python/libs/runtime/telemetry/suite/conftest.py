"""Collectors a test hosts on loopback ports: scripted OTLP/HTTP answers, a trace service ending each export with a scripted status, and the exporters and finished spans a case sends through them."""

from collections.abc import Callable, Iterator, Mapping, Sequence
from contextlib import ExitStack
from datetime import timedelta
import functools

import anyio
from anyio.from_thread import BlockingPortal
from google.rpc.error_details_pb2 import RetryInfo
from google.rpc.status_pb2 import Status
import grpc.aio
import msgspec
import opentelemetry.exporter.otlp.proto.grpc.trace_exporter
import opentelemetry.exporter.otlp.proto.http.trace_exporter
from opentelemetry.proto.collector.trace.v1 import trace_service_pb2
from opentelemetry.proto.collector.trace.v1.trace_service_pb2 import ExportTraceServiceRequest, ExportTraceServiceResponse
from opentelemetry.sdk.trace import ReadableSpan, TracerProvider
from opentelemetry.sdk.trace.export import SimpleSpanProcessor
from opentelemetry.sdk.trace.export.in_memory_span_exporter import InMemorySpanExporter
import pytest

from rasm_telemetry.export import ManualSpanProcessor, provider
from rasm_telemetry.otlp import Collector
from rasm_testing import Answer, Received

# --- [CONSTANTS] ------------------------------------------------------------------------

SECOND = timedelta(seconds=1)
PROTOBUF = frozendict({"content-type": "application/x-protobuf"})
ACCEPTED = Answer(headers=PROTOBUF, body=ExportTraceServiceResponse().SerializeToString())

# --- [MODELS] ---------------------------------------------------------------------------


class Reply(msgspec.Struct, frozen=True, kw_only=True):
    """Status a trace service ends one export with once `held` has passed, a RetryInfo delay in its trailers where `retry` is set."""

    code: grpc.StatusCode = grpc.StatusCode.OK
    details: str = ""
    retry: timedelta | None = None
    held: timedelta = timedelta()


class Call(msgspec.Struct, frozen=True, kw_only=True):
    """Export a trace service received: its request, its metadata, and the event loop clock at its arrival."""

    request: ExportTraceServiceRequest
    metadata: Mapping[str, str | bytes]
    at: timedelta


# --- [OPERATIONS] -----------------------------------------------------------------------


def refusing(status: int, /, **headers: str) -> Answer:
    """Returns the OTLP/HTTP failure answer of one status: a `google.rpc.Status` body under the protobuf content type and the given headers."""
    return Answer(status=status, headers=PROTOBUF | headers, body=Status(message=f"refused with {status}").SerializeToString())


def scripted[Asked, Told](*told: Told) -> Callable[[Asked], Told]:
    """Returns the answering function of a script: one entry per request in order, the last entry for every later request."""
    remaining = iter(told)
    return lambda _asked: next(remaining, told[-1])


def collecting(stack: ExitStack, portal: BlockingPortal, /, *replies: Reply) -> tuple[str, Sequence[Call]]:
    """Serves the OTLP trace service on a loopback port, ending each export with the next reply of the script and the last reply ever after, until the exit stack's scope ends.

    Returns:
        The service's `http://` endpoint and the exports it received so far, in arrival order.
    """
    calls: list[Call] = []
    replying = scripted[Call, Reply](*replies)

    async def export(request: ExportTraceServiceRequest, context: grpc.aio.ServicerContext[ExportTraceServiceRequest, ExportTraceServiceResponse]) -> ExportTraceServiceResponse:
        call = Call(request=request, metadata=dict(context.invocation_metadata() or ()), at=timedelta(seconds=anyio.current_time()))
        calls.append(call)
        reply = replying(call)
        await anyio.sleep(reply.held / SECOND)
        if reply.retry is not None:
            delay = RetryInfo()
            delay.retry_delay.FromTimedelta(reply.retry)
            context.set_trailing_metadata((("google.rpc.retryinfo-bin", delay.SerializeToString()),))
        context.set_code(reply.code)
        context.set_details(reply.details)
        return ExportTraceServiceResponse()

    handler = grpc.unary_unary_rpc_method_handler(export, request_deserializer=ExportTraceServiceRequest.FromString, response_serializer=ExportTraceServiceResponse.SerializeToString)
    server = portal.call(grpc.aio.server)
    server.add_generic_rpc_handlers((grpc.method_handlers_generic_handler(trace_service_pb2.DESCRIPTOR.services_by_name["TraceService"].full_name, {"Export": handler}),))
    port = server.add_insecure_port("127.0.0.1:0")
    portal.call(server.start)
    stack.callback(portal.call, server.stop, None)
    return f"http://127.0.0.1:{port}", calls


def posting(stack: ExitStack, endpoint: str, timeout: timedelta, /) -> opentelemetry.exporter.otlp.proto.http.trace_exporter.OTLPSpanExporter:
    """Returns the OTLP/HTTP span exporter the library builds for a collector at a base URL, each export bounded by `timeout`, shut down when the exit stack's scope ends."""
    exporter = Collector(endpoint=endpoint, timeout=timeout).exporter()
    stack.callback(exporter.shutdown)
    return exporter


def flushing(stack: ExitStack, endpoint: str, timeout: timedelta, /) -> TracerProvider:
    """Returns the library's tracer provider holding one ended span `a` behind its flush-driven processor over an OTLP/gRPC exporter, shut down when the exit stack's scope ends."""
    held = provider("suite", "0.0.0", ManualSpanProcessor(opentelemetry.exporter.otlp.proto.grpc.trace_exporter.OTLPSpanExporter(endpoint=endpoint, timeout=timeout / SECOND)))
    stack.callback(held.shutdown)
    held.get_tracer("suite").start_span("a").end()
    return held


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
def answered(hosted: Callable[[Callable[[Received], Answer]], tuple[str, Sequence[Received]]]) -> Callable[..., tuple[str, Sequence[Received]]]:
    """Returns the start of an OTLP/HTTP collector answering from a script of answers, which returns its base URL and the requests it received."""
    return lambda *answers: hosted(scripted(*answers))


@pytest.fixture
def traced(portal: BlockingPortal) -> Iterator[Callable[..., tuple[str, Sequence[Call]]]]:
    """Yields the start of an OTLP trace service answering from a script of replies, which returns its endpoint and the exports it received, stopped after the test."""
    with ExitStack() as stack:
        yield functools.partial(collecting, stack, portal)


@pytest.fixture
def exporting() -> Iterator[Callable[[str, timedelta], opentelemetry.exporter.otlp.proto.http.trace_exporter.OTLPSpanExporter]]:
    """Yields the library's OTLP/HTTP span exporter for a collector base URL and an export timeout, shut down after the test."""
    with ExitStack() as stack:
        yield functools.partial(posting, stack)


@pytest.fixture
def held() -> Iterator[Callable[[str, timedelta], TracerProvider]]:
    """Yields the library's tracer provider for an OTLP/gRPC endpoint and an export timeout, one ended span held for its flush, shut down after the test."""
    with ExitStack() as stack:
        yield functools.partial(flushing, stack)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ACCEPTED", "SECOND", "Call", "Reply", "refusing"]
