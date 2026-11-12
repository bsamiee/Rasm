"""Structlog entries as OpenTelemetry log records inside `rasm_telemetry.process.instrumented`, copied from `TestStructlogProcessor` of opentelemetry-instrumentation-structlog `tests/test_structlog.py` (0.66b2)."""

from collections.abc import Iterator
import io
import sys

from opentelemetry import trace
from opentelemetry._logs import SeverityNumber
from opentelemetry.sdk._logs.export import InMemoryLogRecordExporter, SimpleLogRecordProcessor
from opentelemetry.semconv.attributes import exception_attributes
from opentelemetry.trace import TraceFlags
import pytest
import structlog

from rasm_telemetry import LogSettings
from rasm_telemetry.process import instrumented

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def exporter() -> Iterator[InMemoryLogRecordExporter]:
    """Yields the in-memory exporter of the scope's log records, the scope entered where the exemplar's `setUp` configures structlog with `StructlogProcessor` and left where its `tearDown` resets (`test_structlog.py:30-47,49-53`)."""
    exporting = InMemoryLogRecordExporter()
    with instrumented(
        io.StringIO(),
        service="rasm-telemetry-tests",
        version="0",
        settings=LogSettings(level="debug"),
        scopes=(__name__,),
        environ={"OTEL_TRACES_EXPORTER": "none", "OTEL_METRICS_EXPORTER": "none", "OTEL_LOGS_EXPORTER": "none"},
        log_record_processors=(SimpleLogRecordProcessor(exporting),),
    ):
        yield exporting
    exporting.clear()


def test_basic_log_emission(exporter: InMemoryLogRecordExporter) -> None:
    """Basic logs are emitted as records (`test_structlog.py:55-69`)."""
    logger = structlog.get_logger()
    logger.info("test message", user="alice", count=42)
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    assert log.log_record.body == "test message"
    assert log.log_record.severity_text == "INFO"
    assert log.log_record.severity_number == SeverityNumber.INFO
    assert log.log_record.attributes is not None
    assert "user" in log.log_record.attributes
    assert log.log_record.attributes["user"] == "alice"
    assert "count" in log.log_record.attributes
    assert log.log_record.attributes["count"] == 42


def test_exception_from_exc_info_tuple(exporter: InMemoryLogRecordExporter) -> None:
    """Exception attributes are read from an `exc_info` tuple (`test_structlog.py:158-181`)."""
    logger = structlog.get_logger()
    error = ValueError("test error")
    try:
        raise error
    except ValueError:
        exc_info = sys.exc_info()
        logger.error("error occurred", exc_info=exc_info)
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    attrs = log.log_record.attributes
    assert attrs is not None
    assert exception_attributes.EXCEPTION_TYPE in attrs
    assert attrs[exception_attributes.EXCEPTION_TYPE] == "ValueError"
    assert exception_attributes.EXCEPTION_MESSAGE in attrs
    assert attrs[exception_attributes.EXCEPTION_MESSAGE] == "test error"
    assert exception_attributes.EXCEPTION_STACKTRACE in attrs
    stacktrace = attrs[exception_attributes.EXCEPTION_STACKTRACE]
    assert isinstance(stacktrace, str)
    assert "ValueError" in stacktrace
    assert "test error" in stacktrace


def test_exception_from_exc_info_true(exporter: InMemoryLogRecordExporter) -> None:
    """Exception attributes are read under `exc_info=True` (`test_structlog.py:183-202`)."""
    logger = structlog.get_logger()
    error = RuntimeError("runtime error")
    try:
        raise error
    except RuntimeError:
        logger.error("error occurred", exc_info=True)
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    attrs = log.log_record.attributes
    assert attrs is not None
    assert exception_attributes.EXCEPTION_TYPE in attrs
    assert attrs[exception_attributes.EXCEPTION_TYPE] == "RuntimeError"
    assert exception_attributes.EXCEPTION_MESSAGE in attrs
    assert attrs[exception_attributes.EXCEPTION_MESSAGE] == "runtime error"
    assert exception_attributes.EXCEPTION_STACKTRACE in attrs


def test_trace_context_with_active_span(exporter: InMemoryLogRecordExporter) -> None:
    """Trace context is captured with an active span (`test_structlog.py:238-258`)."""
    logger = structlog.get_logger()
    tracer = trace.get_tracer(__name__)
    with tracer.start_as_current_span("test-span") as span:
        logger.info("message in span")
        logs = exporter.get_finished_logs()
        assert len(logs) == 1
        log = logs[0]
        assert log.log_record.trace_id is not None
        assert log.log_record.span_id is not None
        span_context = span.get_span_context()
        assert log.log_record.trace_id == span_context.trace_id
        assert log.log_record.span_id == span_context.span_id
        assert log.log_record.trace_flags == TraceFlags(span_context.trace_flags)


def test_without_active_span(exporter: InMemoryLogRecordExporter) -> None:
    """Logging works without an active span (`test_structlog.py:260-269`)."""
    logger = structlog.get_logger()
    logger.info("message without span")
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    assert log.log_record.body == "message without span"
