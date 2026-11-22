"""JSON lines the library handler writes for stdlib records and structlog entries: field order, time, severity, body, code location, redaction, failures, trace context, added processors, and file sinks."""

import inspect
import io
import logging
import os
from pathlib import Path

import msgspec
from opentelemetry.sdk.trace import TracerProvider
import pytest
from structlog.typing import EventDict, WrappedLogger

from rasm import telemetry

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_a_record_is_one_line_in_the_log_record_order(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """A stdlib warning at a fixed time inside a span writes, in order, its UTC time to the millisecond, `WARN` severity 13, interpolated body, logger scope, span context, attributes, and resource."""
    record = logging.LogRecord("suite.part", logging.WARNING, "/srv/part.py", 7, "%d of %s", (3, "frames"), None, func="rendered")
    record.created, record.msecs = 1584713566.123, 123
    with TracerProvider().get_tracer("suite").start_as_current_span("render") as span:
        handler.handle(record)
    (line,) = written
    assert list(line) == ["timestamp", "severity_text", "severity_number", "body", "scope", "trace_id", "span_id", "trace_flags", "attributes", "resource"]
    assert (line["timestamp"], line["severity_text"], line["severity_number"], line["body"], line["scope"]) == ("2020-03-20T14:12:46.123Z", "WARN", 13, "3 of frames", "suite.part")
    assert (line["trace_id"], line["span_id"], line["trace_flags"]) == (
        f"{span.get_span_context().trace_id:032x}",
        f"{span.get_span_context().span_id:016x}",
        f"{span.get_span_context().trace_flags:02x}",
    )
    assert line["attributes"] == {"code.file.path": "/srv/part.py", "code.line.number": 7, "code.function.name": "rendered"}
    assert line["resource"] == {"service.name": "suite", "service.version": "1.2.3", "process.pid": os.getpid()}


def test_a_record_outside_a_span_and_without_a_level_holds_neither(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """A record with level 0 outside every span is `UNSPECIFIED` severity 0 and writes no trace context."""
    handler.handle(logging.makeLogRecord({"msg": "bare", "levelno": 0, "name": "suite"}))
    (line,) = written
    assert (line["severity_text"], line["severity_number"]) == ("UNSPECIFIED", 0)
    assert not {"trace_id", "span_id", "trace_flags"} & line.keys()


def test_an_entry_writes_its_fields_its_event_name_and_its_call_site(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """A bound logger's entry writes its fields under `attributes` beside the file, line, and function of the call, and its `otel.event.name` as `event_name` after the body."""
    log = telemetry.logger("suite.entries")
    log.addHandler(handler)
    log.setLevel(logging.INFO)
    try:
        called = inspect.getframeinfo(inspect.stack(context=0)[0].frame)
        log.info("saved", frames=48, **{"otel.event.name": "Saved"})
    finally:
        log.removeHandler(handler)
    (line,) = written
    assert list(line)[:5] == ["timestamp", "severity_text", "severity_number", "body", "event_name"]
    assert (line["body"], line["event_name"], line["scope"]) == ("saved", "Saved", "suite.entries")
    assert line["attributes"] == {"frames": 48, "code.file.path": __file__, "code.line.number": called.lineno + 1, "code.function.name": called.function}


@pytest.mark.parametrize(
    "field,redacted",
    [
        ("password", True),
        ("db.passwd", True),
        ("client_secret", True),
        ("Authorization", True),
        ("Cookie", True),
        ("apiKey", True),
        ("private-key", True),
        ("auth.token", True),
        ("token", True),
        ("tokens", False),
        ("token_count", False),
        ("keyboard", False),
        ("passage", False),
    ],
)
def test_a_field_named_like_a_credential_is_redacted(handler: logging.Handler, written: list[dict[str, object]], field: str, *, redacted: bool) -> None:
    """Fields naming a password, secret, credential, authorization, cookie, API or private key, or ending in a token word hold `[redacted]`, other fields their value."""
    handler.handle(logging.makeLogRecord({"msg": "sent", "levelno": logging.INFO, "name": "suite", field: "s3cr3t"}))
    (line,) = written
    attributes = line["attributes"]
    assert isinstance(attributes, dict)
    assert attributes[field] == ("[redacted]" if redacted else "s3cr3t")


def test_a_failure_writes_its_type_message_and_whole_cause_chain(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """An exception record writes `exception.type`, `exception.message`, and a stack trace holding the cause it was raised from."""

    def counted(text: bytes, /) -> int:
        try:
            return msgspec.json.decode(text, type=int)
        except msgspec.DecodeError as cause:
            raise LookupError("no frame count") from cause

    with pytest.raises(LookupError) as raised:
        counted(b"frames")
    handler.handle(logging.LogRecord("suite", logging.ERROR, __file__, 1, "failed", None, (LookupError, raised.value, raised.value.__traceback__)))
    (line,) = written
    attributes = line["attributes"]
    assert isinstance(attributes, dict)
    assert (attributes["exception.type"], attributes["exception.message"]) == ("LookupError", "no frame count")
    stack = attributes["exception.stacktrace"]
    assert isinstance(stack, str)
    assert "msgspec.DecodeError: JSON is malformed" in stack
    assert "The above exception was the direct cause of the following exception" in stack


def test_added_processors_read_redacted_fields_and_write_attributes() -> None:
    """A processor the handler adds runs after redaction and before fields nest, so it reads `[redacted]` and its field lands under `attributes`."""

    def copied(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
        return {**event_dict, "copied": event_dict["password"]}

    stream = io.StringIO()
    telemetry.LogSettings().handler(stream, copied, service="suite", version="1.2.3").handle(logging.makeLogRecord({"msg": "sent", "levelno": logging.INFO, "name": "suite", "password": "hunter2"}))
    attributes = msgspec.json.decode(stream.getvalue(), type=dict[str, object])["attributes"]
    assert isinstance(attributes, dict)
    assert (attributes["password"], attributes["copied"]) == ("[redacted]", "[redacted]")


def test_a_path_sink_creates_its_folder_and_rotates_past_its_size(tmp_path: Path) -> None:
    """A log path in a missing folder is created, and past `size` bytes the file rotates, keeping `files` rotated files compressed under `gzip`."""
    log = tmp_path / "logs" / "nested" / "suite.log"
    handler = telemetry.LogSettings(size=400, files=2, gzip=True).handler(log, service="suite", version="1.2.3")
    try:
        for frame in range(12):
            handler.handle(logging.makeLogRecord({"msg": f"frame {frame}", "levelno": logging.INFO, "name": "suite"}))
    finally:
        handler.close()
    rotated = sorted(path.name for path in log.parent.iterdir() if path.name.startswith("suite.log."))
    assert len(rotated) == 2
    assert all(name.endswith(".gz") for name in rotated)
    assert msgspec.json.decode(log.read_bytes().splitlines()[-1], type=dict[str, object])["body"] == "frame 11"
