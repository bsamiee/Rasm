"""Content of each JSON record `rasm_telemetry.LogSettings.handler` writes, copied from the opentelemetry-instrumentation-logging handler tests, the opentelemetry-instrumentation level mapping test, and one ecs-logging case."""

from collections.abc import Iterator
from datetime import datetime, UTC
import io
import logging

import msgspec
from opentelemetry._logs import SeverityNumber
from opentelemetry.sdk import trace
from opentelemetry.semconv.attributes import code_attributes, exception_attributes
from opentelemetry.trace import INVALID_SPAN_CONTEXT
import pytest

from rasm_telemetry import LogSettings

# --- [OPERATIONS] -----------------------------------------------------------------------


def get_log_record(stream: io.StringIO, i: int) -> dict[str, object]:
    """Returns one written record, where the exemplar's `FakeProcessor.get_log_record` returns one emitted (`test_handler.py:808-809`)."""
    return msgspec.json.decode(stream.getvalue().splitlines()[i], type=dict[str, object])


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def stream() -> io.StringIO:
    """Returns the text stream the handler under test writes."""
    return io.StringIO()


@pytest.fixture
def handler(stream: io.StringIO) -> logging.Handler:
    """Returns the library's handler over the stream, where the exemplars construct `LoggingHandler(...)` and `StdlibFormatter()` on a stream handler."""
    return LogSettings().handler(stream, service="rasm-telemetry-tests", version="0")


@pytest.fixture
def logger(handler: logging.Handler) -> Iterator[logging.Logger]:
    """Yields logger `foo` with the handler added, removed after the case, as `set_up_test_logging` and each case's last line (`test_handler.py:776-789`)."""
    foo = logging.getLogger("foo")
    foo.addHandler(handler)
    yield foo
    foo.removeHandler(handler)


def test_log_record_no_span_context(logger: logging.Logger, stream: io.StringIO) -> None:
    """A warning outside any span holds the invalid span context (`test_handler.py:105-122`)."""
    logger.warning("Warning message")
    record = get_log_record(stream, 0)
    assert record is not None
    trace_id, span_id, trace_flags = record.get("trace_id", "0"), record.get("span_id", "0"), record.get("trace_flags", "0")
    assert isinstance(trace_id, str)
    assert isinstance(span_id, str)
    assert isinstance(trace_flags, str)
    assert int(trace_id, 16) == INVALID_SPAN_CONTEXT.trace_id
    assert int(span_id, 16) == INVALID_SPAN_CONTEXT.span_id
    assert int(trace_flags, 16) == INVALID_SPAN_CONTEXT.trace_flags


def test_log_record_with_code_attributes(logger: logging.Logger, stream: io.StringIO) -> None:
    """A warning holds its call site beside its `extra` (`test_handler.py:152-174`)."""
    logger.warning("Warning message", extra={"http.status_code": 200})
    record = get_log_record(stream, 0)
    assert record is not None
    attributes = record["attributes"]
    assert isinstance(attributes, dict)
    assert len(attributes) == 4
    assert attributes["http.status_code"] == 200
    assert attributes[code_attributes.CODE_FILE_PATH].endswith("test_record.py")
    assert attributes[code_attributes.CODE_FUNCTION_NAME] == "test_log_record_with_code_attributes"
    assert code_attributes.CODE_LINE_NUMBER in attributes


def test_log_record_exception(logger: logging.Logger, stream: io.StringIO) -> None:
    """Exception information is included in attributes (`test_handler.py:176-206`)."""
    error = ZeroDivisionError("division by zero")
    try:
        raise error
    except ZeroDivisionError:
        logger.exception("Zero Division Error")
    record = get_log_record(stream, 0)
    assert record is not None
    assert isinstance(record["body"], str)
    assert record["body"] == "Zero Division Error"
    attributes = record["attributes"]
    assert isinstance(attributes, dict)
    assert attributes[exception_attributes.EXCEPTION_TYPE] == ZeroDivisionError.__name__
    assert attributes[exception_attributes.EXCEPTION_MESSAGE] == "division by zero"
    stack_trace = attributes[exception_attributes.EXCEPTION_STACKTRACE]
    assert isinstance(stack_trace, str)
    assert "Traceback" in stack_trace
    assert "ZeroDivisionError" in stack_trace
    assert "division by zero" in stack_trace
    assert __file__ in stack_trace


def test_log_exc_info_false(logger: logging.Logger, stream: io.StringIO) -> None:
    """Exception information is not included in attributes under `exc_info=False` (`test_handler.py:239-266`)."""
    error = ZeroDivisionError("division by zero")
    try:
        raise error
    except ZeroDivisionError:
        logger.error("Zero Division Error", exc_info=False)
    record = get_log_record(stream, 0)
    assert record is not None
    assert record["body"] == "Zero Division Error"
    attributes = record["attributes"]
    assert isinstance(attributes, dict)
    assert exception_attributes.EXCEPTION_TYPE not in attributes
    assert exception_attributes.EXCEPTION_MESSAGE not in attributes
    assert exception_attributes.EXCEPTION_STACKTRACE not in attributes


def test_log_record_trace_correlation_deprecated(logger: logging.Logger, stream: io.StringIO) -> None:
    """A critical message inside a span holds `FATAL` severity and the span's context (`test_handler.py:338-356`)."""
    tracer = trace.TracerProvider().get_tracer(__name__)
    with tracer.start_as_current_span("test") as span:
        logger.critical("Critical message within span")
        record = get_log_record(stream, 0)
        assert record["body"] == "Critical message within span"
        assert record["severity_text"] == "FATAL"
        assert record["severity_number"] == SeverityNumber.FATAL.value
        span_context = span.get_span_context()
        assert record["trace_id"] == format(span_context.trace_id, "032x")
        assert record["span_id"] == format(span_context.span_id, "016x")
        assert record["trace_flags"] == format(span_context.trace_flags, "02x")


@pytest.mark.parametrize(
    "levelno,expected",
    [
        (0, SeverityNumber.UNSPECIFIED),
        (9, SeverityNumber.UNSPECIFIED),
        (10, SeverityNumber.DEBUG),
        (14, SeverityNumber.DEBUG4),
        (20, SeverityNumber.INFO),
        (30, SeverityNumber.WARN),
        (40, SeverityNumber.ERROR),
        (50, SeverityNumber.FATAL),
        (53, SeverityNumber.FATAL4),
        (54, SeverityNumber.FATAL4),
    ],
)
def test_std_to_otel(handler: logging.Handler, stream: io.StringIO, levelno: int, expected: SeverityNumber) -> None:
    """Each stdlib level number maps to its severity number (opentelemetry-instrumentation `tests/test_log_utils.py:11-25`)."""
    handler.handle(logging.makeLogRecord({"msg": "message", "levelno": levelno, "levelname": logging.getLevelName(levelno), "name": "foo"}))
    assert get_log_record(stream, 0)["severity_number"] == expected.value


def test_can_be_set_on_handler(handler: logging.Handler, stream: io.StringIO) -> None:
    """A handled record is written with the time its `created` and `msecs` hold, its level, its interpolated message, and its call site (ecs-logging `test_stdlib_formatter.py:88-99`)."""
    record = logging.LogRecord(name="logger-name", level=logging.DEBUG, pathname="/path/file.py", lineno=10, msg="%d: %s", args=(1, "hello"), func="test_function", exc_info=None)
    record.created = 1584713566
    record.msecs = 123
    handler.handle(record)
    written = get_log_record(stream, 0)
    attributes = written.get("attributes", {})
    assert isinstance(attributes, dict)
    timestamp = written.get("timestamp")
    assert isinstance(timestamp, str)
    assert (
        datetime.fromisoformat(timestamp),
        written.get("severity_text"),
        written.get("body"),
        attributes.get(code_attributes.CODE_LINE_NUMBER),
        attributes.get(code_attributes.CODE_FUNCTION_NAME),
    ) == (datetime(2020, 3, 20, 14, 12, 46, 123000, tzinfo=UTC), SeverityNumber.DEBUG.name, "1: hello", 10, "test_function")
