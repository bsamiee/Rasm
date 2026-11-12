"""Trace context `rasm_telemetry.process.instrumented` reads from the environment it is given, copied from `TestEnvironmentCarrierWithTraceContext` of opentelemetry-api `tests/propagators/test__envcarrier.py` (1.45.2)."""

import io

from opentelemetry import trace
from opentelemetry.context import Context
from opentelemetry.propagators._envcarrier import EnvironmentSetter
from opentelemetry.sdk.trace import ReadableSpan
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator
import pytest

from rasm_telemetry import LogSettings
from rasm_telemetry.process import instrumented

# --- [CONSTANTS] ------------------------------------------------------------------------

TRACE_ID = 0x4BF92F3577B34DA6A3CE929D0E0E4736
SPAN_ID = 0x00F067AA0BA902B7

# --- [OPERATIONS] -----------------------------------------------------------------------


def _extract_with_env(env_vars: dict[str, str]) -> Context:
    """Returns a context holding the parent of the first span started inside the process scope entered with the variables given, where the exemplar's helper extracts from `os.environ` (`test__envcarrier.py:252-256`)."""
    environ = {"OTEL_TRACES_EXPORTER": "none", "OTEL_METRICS_EXPORTER": "none", "OTEL_LOGS_EXPORTER": "none"} | env_vars
    with instrumented(io.StringIO(), service="rasm-telemetry-tests", version="0", settings=LogSettings(), scopes=(), environ=environ):
        span = trace.get_tracer(__name__).start_span("child")
        assert isinstance(span, ReadableSpan)
        return Context() if span.parent is None else trace.set_span_in_context(trace.NonRecordingSpan(span.parent))


def test_extract_valid_traceparent() -> None:
    """A valid `TRACEPARENT` is the remote parent of the scope's spans (`test__envcarrier.py:265-274`)."""
    traceparent = f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01"
    ctx = _extract_with_env({"TRACEPARENT": traceparent})
    span_context = trace.get_current_span(ctx).get_span_context()
    assert span_context.trace_id == TRACE_ID
    assert span_context.span_id == SPAN_ID
    assert span_context.trace_flags.sampled
    assert span_context.is_remote


@pytest.mark.parametrize(
    "invalid_tp",
    [
        "invalid-format",
        "00-00000000000000000000000000000000-1234567890123456-00",
        "00-12345678901234567890123456789012-0000000000000000-00",
        "00-12345678901234567890123456789012-1234567890123456-00-extra",
    ],
)
def test_extract_invalid_traceparent(invalid_tp: str) -> None:
    """An invalid `TRACEPARENT` is ignored (`test__envcarrier.py:308-324`)."""
    ctx = _extract_with_env({"TRACEPARENT": invalid_tp})
    span = trace.get_current_span(ctx)
    assert span.get_span_context() == trace.INVALID_SPAN_CONTEXT


def test_extract_missing_traceparent() -> None:
    """A missing `TRACEPARENT` leaves an empty context (`test__envcarrier.py:326-330`)."""
    ctx = _extract_with_env({})
    span = trace.get_current_span(ctx)
    assert isinstance(span.get_span_context(), trace.SpanContext)


def test_roundtrip_preserves_traceparent() -> None:
    """A `TRACEPARENT` read by the scope and injected again is unchanged (`test__envcarrier.py:380-399`)."""
    original_traceparent = f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01"
    ctx1 = _extract_with_env({"TRACEPARENT": original_traceparent})
    setter = EnvironmentSetter()
    env_dict: dict[str, str] = {}
    TraceContextTextMapPropagator().inject(env_dict, context=ctx1, setter=setter)
    assert env_dict["TRACEPARENT"] == original_traceparent
    ctx2 = _extract_with_env(env_dict)
    span1 = trace.get_current_span(ctx1).get_span_context()
    span2 = trace.get_current_span(ctx2).get_span_context()
    assert span1.trace_id == span2.trace_id
    assert span1.span_id == span2.span_id
    assert span1.trace_flags == span2.trace_flags
