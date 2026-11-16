"""Structlog entries as OpenTelemetry log records inside `rasm_telemetry.process.instrumented`, copied from `TestStructlogProcessor` of opentelemetry-instrumentation-structlog `tests/test_structlog.py` (0.66b2)."""

from collections.abc import Iterator
import os
from pathlib import Path
import sys
from unittest.mock import patch

from opentelemetry import trace
from opentelemetry._logs import SeverityNumber
from opentelemetry.sdk._logs.export import InMemoryLogRecordExporter, SimpleLogRecordProcessor
from opentelemetry.semconv.attributes import exception_attributes
from opentelemetry.trace import TraceFlags
import pytest
import structlog

import rasm_telemetry
from rasm_telemetry.process import instrumented

# --- [CONSTANTS] ------------------------------------------------------------------------

DURATION = 1.5

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def exporter(tmp_path: Path) -> Iterator[InMemoryLogRecordExporter]:
    """Yields the in-memory exporter of the scope's log records, the scope entered where the exemplar's `setUp` configures structlog with `StructlogProcessor` and left where its `tearDown` resets (`test_structlog.py:30-47,49-53`)."""
    exporting = InMemoryLogRecordExporter()
    with (
        patch.dict(os.environ, {"OTEL_TRACES_EXPORTER": "none", "OTEL_METRICS_EXPORTER": "none", "OTEL_LOGS_EXPORTER": "none"}, clear=True),
        instrumented(
            tmp_path / "scope.log",
            service="rasm-telemetry-tests",
            version="0",
            settings=rasm_telemetry.LogSettings(level="debug"),
            scopes=(__name__,),
            log_record_processors=(SimpleLogRecordProcessor(exporting),),
        ),
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


@pytest.mark.parametrize(
    "method,message,severity_text,severity_number",
    [
        pytest.param("debug", "debug message", "DEBUG", SeverityNumber.DEBUG, id="test_debug_level"),
        pytest.param("info", "info message", "INFO", SeverityNumber.INFO, id="test_info_level"),
        pytest.param("warning", "warning message", "WARN", SeverityNumber.WARN, id="test_warning_level"),
        pytest.param("error", "error message", "ERROR", SeverityNumber.ERROR, id="test_error_level"),
        pytest.param("critical", "critical message", "FATAL", SeverityNumber.FATAL, id="test_critical_level"),
    ],
)
def test_level(exporter: InMemoryLogRecordExporter, method: str, message: str, severity_text: str, severity_number: SeverityNumber) -> None:
    """Each level maps to its severity, `warning` normalized to `WARN` and `critical` to `FATAL` (`test_structlog.py:71-114,147-156`)."""
    logger = structlog.get_logger()
    getattr(logger, method)(message)
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    assert log.log_record.severity_text == severity_text
    assert log.log_record.severity_number == severity_number


def test_exception_from_exception_instance(exporter: InMemoryLogRecordExporter) -> None:
    """Exception attributes are read from an exception instance passed as `exc_info` (`test_structlog.py:204-222`)."""
    logger = structlog.get_logger()
    error = KeyError("missing key")
    try:
        raise error
    except KeyError as exc:
        logger.error("error occurred", exc_info=exc)
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    attrs = log.log_record.attributes
    assert attrs is not None
    assert exception_attributes.EXCEPTION_TYPE in attrs
    assert attrs[exception_attributes.EXCEPTION_TYPE] == "KeyError"
    assert exception_attributes.EXCEPTION_MESSAGE in attrs
    assert exception_attributes.EXCEPTION_STACKTRACE in attrs


def test_reserved_keys_filtered(exporter: InMemoryLogRecordExporter) -> None:
    """Reserved structlog keys are filtered from attributes (`test_structlog.py:271-298`)."""
    logger = structlog.get_logger()
    logger.info("test", user="alice", _record="should be filtered", _logger="should be filtered", _name="should be filtered")
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    attrs = log.log_record.attributes
    assert attrs is not None
    assert "user" in attrs
    assert attrs["user"] == "alice"
    assert "_record" not in attrs
    assert "_logger" not in attrs
    assert "_name" not in attrs
    assert "event" not in attrs
    assert "level" not in attrs
    assert "timestamp" not in attrs


def test_custom_attributes_pass_through(exporter: InMemoryLogRecordExporter) -> None:
    """Custom attributes pass through with their types (`test_structlog.py:300-321`)."""
    logger = structlog.get_logger()
    logger.info("test", user_id=123, request_id="abc-def", ip_address="192.168.1.1", duration=DURATION, success=True)
    logs = exporter.get_finished_logs()
    assert len(logs) == 1
    log = logs[0]
    attrs = log.log_record.attributes
    assert attrs is not None
    assert attrs["user_id"] == 123
    assert attrs["request_id"] == "abc-def"
    assert attrs["ip_address"] == "192.168.1.1"
    assert attrs["duration"] == DURATION
    assert attrs["success"] is True
