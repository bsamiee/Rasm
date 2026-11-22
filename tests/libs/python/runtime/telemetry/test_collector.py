"""Spans posted over OTLP/HTTP by the exporter of `telemetry.otlp.Collector` to a collector the test hosts."""

import gzip

from opentelemetry.exporter.otlp.common.http import Compression
from opentelemetry.proto.collector.trace.v1.trace_service_pb2 import ExportTraceServiceRequest
from opentelemetry.sdk.trace import ReadableSpan
from opentelemetry.sdk.trace.export import SpanExportResult
from opentelemetry.trace import SpanContext, TraceFlags
import pytest

from rasm.telemetry.otlp import Collector
from rasm.testing.hosting import Answer, Hosted

# --- [CONSTANTS] ------------------------------------------------------------------------

SPAN = ReadableSpan("render", context=SpanContext(trace_id=7, span_id=3, is_remote=False, trace_flags=TraceFlags(TraceFlags.SAMPLED)))

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_the_collector_options_decide_where_and_how_spans_post(hosted: Hosted, monkeypatch: pytest.MonkeyPatch) -> None:
    """Endpoint, headers, and compression given to the collector win over their `OTEL_EXPORTER_OTLP_*` variables: spans post gzip-compressed to the whole URL with the headers given."""
    url, received = hosted(lambda _: Answer())
    monkeypatch.setenv("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", f"{url}/from-variable")
    monkeypatch.setenv("OTEL_EXPORTER_OTLP_TRACES_HEADERS", "tenant=from-variable")
    exporter = Collector(endpoint=f"{url}/ingest/traces", headers={"tenant": "studio"}, compression=Compression.GZIP, timeout=5).exporter()
    assert exporter.export([SPAN]) is SpanExportResult.SUCCESS
    (request,) = received
    assert (request.method, request.path, request.headers["tenant"], request.headers["content-encoding"]) == ("POST", "/ingest/traces", "studio", "gzip")
    sent = ExportTraceServiceRequest.FromString(gzip.decompress(request.body))
    assert [span.name for resource in sent.resource_spans for scope in resource.scope_spans for span in scope.spans] == ["render"]


def test_an_option_left_out_takes_its_variable(hosted: Hosted, monkeypatch: pytest.MonkeyPatch) -> None:
    """A collector naming no option posts uncompressed to `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` with `OTEL_EXPORTER_OTLP_TRACES_HEADERS`."""
    url, received = hosted(lambda _: Answer())
    monkeypatch.setenv("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", f"{url}/from-variable")
    monkeypatch.setenv("OTEL_EXPORTER_OTLP_TRACES_HEADERS", "tenant=from-variable")
    assert Collector().exporter().export([SPAN]) is SpanExportResult.SUCCESS
    (request,) = received
    assert (request.path, request.headers["tenant"], "content-encoding" in request.headers) == ("/from-variable", "from-variable", False)
