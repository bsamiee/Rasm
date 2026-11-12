"""Trace context and baggage `rasm_telemetry.process.instrumented` reads from the environment it is given, copied from opentelemetry-api `tests/propagators/test__envcarrier.py` (1.45.2)."""

from collections.abc import Callable
import os
from pathlib import Path
from unittest.mock import patch

from opentelemetry import context, trace
from opentelemetry.baggage import get_all, set_baggage
from opentelemetry.baggage.propagation import W3CBaggagePropagator
from opentelemetry.context import Context
from opentelemetry.propagators._envcarrier import EnvironmentSetter
from opentelemetry.trace.propagation.tracecontext import TraceContextTextMapPropagator
import pytest

from rasm_telemetry import LogSettings
from rasm_telemetry.process import instrumented

# --- [TYPES] ----------------------------------------------------------------------------

type Extract = Callable[[dict[str, str]], Context]

# --- [CONSTANTS] ------------------------------------------------------------------------

TRACE_ID = 0x4BF92F3577B34DA6A3CE929D0E0E4736
SPAN_ID = 0x00F067AA0BA902B7

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def extract_with_env(tmp_path: Path) -> Extract:
    """Returns the call entering the process scope with the variables given and returning the context current inside it, where the exemplar's `_extract_with_env` extracts from `os.environ` (`test__envcarrier.py:252-256,464-468`)."""

    def extracting(env_vars: dict[str, str], /) -> Context:
        environ = {"OTEL_TRACES_EXPORTER": "none", "OTEL_METRICS_EXPORTER": "none", "OTEL_LOGS_EXPORTER": "none"} | env_vars
        with patch.dict(os.environ, environ, clear=True), instrumented(tmp_path / "scope.log", service="rasm-telemetry-tests", version="0", settings=LogSettings(), scopes=()):
            return context.get_current()

    return extracting


def test_extract_valid_traceparent(extract_with_env: Extract) -> None:
    """A valid `TRACEPARENT` is the remote parent of the scope's spans (`test__envcarrier.py:265-274`)."""
    traceparent = f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01"
    ctx = extract_with_env({"TRACEPARENT": traceparent})
    span_context = trace.get_current_span(ctx).get_span_context()
    assert span_context.trace_id == TRACE_ID
    assert span_context.span_id == SPAN_ID
    assert span_context.trace_flags.sampled
    assert span_context.is_remote


def test_extract_traceparent_not_sampled(extract_with_env: Extract) -> None:
    """A `TRACEPARENT` with the sampled flag off gives an unsampled parent (`test__envcarrier.py:276-282`)."""
    traceparent = f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-00"
    ctx = extract_with_env({"TRACEPARENT": traceparent})
    span_context = trace.get_current_span(ctx).get_span_context()
    assert not span_context.trace_flags.sampled


def test_extract_with_tracestate(extract_with_env: Extract) -> None:
    """`TRACESTATE` beside `TRACEPARENT` reaches the parent's trace state (`test__envcarrier.py:284-293`)."""
    traceparent = f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01"
    tracestate = "vendor1=value1,vendor2=value2"
    ctx = extract_with_env({"TRACEPARENT": traceparent, "TRACESTATE": tracestate})
    span_context = trace.get_current_span(ctx).get_span_context()
    assert span_context.trace_state.get("vendor1") == "value1"
    assert span_context.trace_state.get("vendor2") == "value2"


@pytest.mark.skipif(os.name == "nt", reason="Windows environment variable names are case-insensitive")
def test_extract_ignores_lowercase_trace_context_names(extract_with_env: Extract) -> None:
    """Lowercase `traceparent` and `tracestate` names are not read (`test__envcarrier.py:295-306`)."""
    traceparent = f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01"
    ctx = extract_with_env({"traceparent": traceparent, "tracestate": "vendor=value"})
    span_context = trace.get_current_span(ctx).get_span_context()
    assert not span_context.is_valid


@pytest.mark.parametrize(
    "invalid_tp",
    [
        "invalid-format",
        "00-00000000000000000000000000000000-1234567890123456-00",
        "00-12345678901234567890123456789012-0000000000000000-00",
        "00-12345678901234567890123456789012-1234567890123456-00-extra",
    ],
)
def test_extract_invalid_traceparent(extract_with_env: Extract, invalid_tp: str) -> None:
    """An invalid `TRACEPARENT` gives no parent (`test__envcarrier.py:308-324`)."""
    ctx = extract_with_env({"TRACEPARENT": invalid_tp})
    span = trace.get_current_span(ctx)
    assert span.get_span_context() == trace.INVALID_SPAN_CONTEXT


def test_extract_missing_traceparent(extract_with_env: Extract) -> None:
    """With no `TRACEPARENT` the scope opens over a span context (`test__envcarrier.py:326-330`)."""
    ctx = extract_with_env({})
    span = trace.get_current_span(ctx)
    assert isinstance(span.get_span_context(), trace.SpanContext)


def test_roundtrip_preserves_traceparent(extract_with_env: Extract) -> None:
    """A `TRACEPARENT` read by the scope and injected again is unchanged (`test__envcarrier.py:380-399`)."""
    original_traceparent = f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01"
    ctx1 = extract_with_env({"TRACEPARENT": original_traceparent})
    env_dict: dict[str, str] = {}
    TraceContextTextMapPropagator().inject(env_dict, context=ctx1, setter=EnvironmentSetter())
    assert env_dict["TRACEPARENT"] == original_traceparent
    ctx2 = extract_with_env(env_dict)
    span1 = trace.get_current_span(ctx1).get_span_context()
    span2 = trace.get_current_span(ctx2).get_span_context()
    assert span1.trace_id == span2.trace_id
    assert span1.span_id == span2.span_id
    assert span1.trace_flags == span2.trace_flags


def test_roundtrip_preserves_tracestate(extract_with_env: Extract) -> None:
    """A `TRACESTATE` read by the scope is injected again (`test__envcarrier.py:401-413`)."""
    env_vars = {"TRACEPARENT": f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01", "TRACESTATE": "vendor1=value1,vendor2=value2"}
    ctx1 = extract_with_env(env_vars)
    env_dict: dict[str, str] = {}
    TraceContextTextMapPropagator().inject(env_dict, context=ctx1, setter=EnvironmentSetter())
    assert "TRACESTATE" in env_dict
    assert "vendor1=value1" in env_dict["TRACESTATE"]
    assert "vendor2=value2" in env_dict["TRACESTATE"]


def test_extract_baggage(extract_with_env: Extract) -> None:
    """`BAGGAGE` is the baggage of the context inside the scope (`test__envcarrier.py:477-482`)."""
    ctx = extract_with_env({"BAGGAGE": "key1=value1,key2=value2,key3=value3"})
    baggage = get_all(ctx)
    assert baggage == {"key1": "value1", "key2": "value2", "key3": "value3"}


def test_extract_empty_baggage(extract_with_env: Extract) -> None:
    """An empty `BAGGAGE` gives no baggage (`test__envcarrier.py:484-488`)."""
    ctx = extract_with_env({"BAGGAGE": ""})
    baggage = get_all(ctx)
    assert baggage == {}


def test_extract_missing_baggage(extract_with_env: Extract) -> None:
    """With no `BAGGAGE` the context holds no baggage (`test__envcarrier.py:490-494`)."""
    ctx = extract_with_env({})
    baggage = get_all(ctx)
    assert baggage == {}


def test_roundtrip_baggage(extract_with_env: Extract) -> None:
    """Baggage injected into an environment is the baggage the scope reads from it (`test__envcarrier.py:515-526`)."""
    ctx1 = context.get_current()
    ctx1 = set_baggage("user_id", "12345", context=ctx1)
    ctx1 = set_baggage("session_id", "abc-def", context=ctx1)
    env_dict: dict[str, str] = {}
    W3CBaggagePropagator().inject(env_dict, context=ctx1, setter=EnvironmentSetter())
    ctx2 = extract_with_env(env_dict)
    baggage1 = get_all(ctx1)
    baggage2 = get_all(ctx2)
    assert baggage1 == baggage2


def test_extract_all_w3c_headers(extract_with_env: Extract) -> None:
    """`TRACEPARENT`, `TRACESTATE`, and `BAGGAGE` together give the parent and the baggage (`test__envcarrier.py:556-576`)."""
    env_vars = {"TRACEPARENT": f"00-{TRACE_ID:032x}-{SPAN_ID:016x}-01", "TRACESTATE": "vendor=value", "BAGGAGE": "user_id=12345,session_id=abc-def"}
    ctx = extract_with_env(env_vars)
    span_context = trace.get_current_span(ctx).get_span_context()
    assert span_context.trace_id == TRACE_ID
    assert span_context.span_id == SPAN_ID
    baggage = get_all(ctx)
    assert baggage["user_id"] == "12345"
    assert baggage["session_id"] == "abc-def"


def test_empty_environment(extract_with_env: Extract) -> None:
    """With no carrier variable the scope opens over a context (`test__envcarrier.py:598-605`)."""
    ctx = extract_with_env({})
    assert isinstance(ctx, Context)
