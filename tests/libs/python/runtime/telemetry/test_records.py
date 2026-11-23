"""JSON lines the library handler writes for stdlib records and structlog entries, the bound loggers writing them, child output relayed as records, and the level names of the log settings."""

from collections.abc import Callable
from functools import partial, reduce
import inspect
import io
import logging
import os
from pathlib import Path

from hypothesis import given, strategies
import msgspec
from opentelemetry.sdk.trace import TracerProvider
import pytest
import structlog
from structlog.contextvars import bound_contextvars
from structlog.typing import EventDict, WrappedLogger

from rasm import telemetry

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RECORDS]


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


@given(
    path=strategies.lists(
        strategies.tuples(strategies.sampled_from(("headers", "extra", "db", "request", "upstream")), strategies.sampled_from((lambda held: held, lambda held: [held], lambda held: (held,)))),
        min_size=1,
        max_size=5,
    ),
    field=strategies.sampled_from(("password", "db.passwd", "client_secret", "Authorization", "Cookie", "apiKey", "private-key", "auth.token", "token")),
    secret=strategies.text(alphabet="0123456789abcdef", min_size=12, max_size=24).map("s3cr3t-{}".format),
)
def test_a_credential_nested_at_any_depth_is_redacted(path: list[tuple[str, Callable[[object], object]]], field: str, secret: str) -> None:
    """A credential field under nested mappings, each one bare or inside a list or tuple, writes `[redacted]` where its value stood, every other part of the field written as given."""
    nested = partial(reduce, lambda held, level: {level[0]: level[1](held)}, path[::-1])
    stream = io.StringIO()
    telemetry.LogSettings().handler(stream, service="suite", version="1.2.3").handle(
        logging.makeLogRecord({"msg": "sent", "levelno": logging.INFO, "name": "suite", **nested({field: secret, "frames": 48})})
    )
    attributes = msgspec.json.decode(stream.getvalue(), type=dict[str, object])["attributes"]
    assert isinstance(attributes, dict)
    assert secret not in stream.getvalue()
    assert msgspec.json.decode(msgspec.json.encode(nested({field: "[redacted]", "frames": 48}))).items() <= attributes.items()


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
    assert ("LookupError: no frame count" in stack, "msgspec.DecodeError: JSON is malformed" in stack) == (True, True)
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


# --- [LOGGERS]


def test_an_entry_merges_context_local_fields(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """Fields bound to the context reach every entry logged inside it beside the entry's own, and no entry logged after it."""
    log = telemetry.logger("suite.context")
    log.addHandler(handler)
    log.setLevel(logging.INFO)
    with bound_contextvars(job="Scene 1-24"):
        log.info("started", frames=48)
    log.info("idle")
    log.removeHandler(handler)
    attributes = [line["attributes"] for line in written]
    assert [{key: value for key, value in held.items() if not key.startswith("code.")} for held in attributes if isinstance(held, dict)] == [{"job": "Scene 1-24", "frames": 48}, {}]


def test_the_logger_ignores_structlog_process_configuration(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """A process-wide structlog configuration dropping every event leaves the library's logger writing."""

    def dropped(_logger: WrappedLogger, _method: str, _event: EventDict, /) -> EventDict:
        raise structlog.DropEvent

    structlog.configure(processors=[dropped])
    log = telemetry.logger("suite.configured")
    log.addHandler(handler)
    log.setLevel(logging.INFO)
    log.info("kept")
    log.removeHandler(handler)
    assert [line["body"] for line in written] == ["kept"]


# --- [RELAYED]


def test_each_relayed_line_is_one_info_record_written_as_it_is_consumed(caplog: pytest.LogCaptureFixture) -> None:
    """Lines lose trailing space and keep leading space, blank lines are left out, a line that is no UTF-8 is its bytes' `repr`, and each is one `INFO` record under the scope once consumed, before the next line exists."""
    lines = telemetry.relayed(iter([b"  indented frame 1 \r\n", b"\n", b" \t\n", b"\xffraw\n", b"last"]), caplog.handler, "blender.child")
    first = next(lines)
    assert [record.getMessage() for record in caplog.records] == [first]
    yielded = [first, *lines]
    assert yielded == ["  indented frame 1", "b'\\xffraw'", "last"]
    assert [(record.name, record.levelname, record.getMessage()) for record in caplog.records] == [("blender.child", "INFO", text) for text in yielded]


# --- [SETTINGS]


@pytest.mark.parametrize("name,level", [(" DeBug ", 10), ("WARNING", 30), ("critical", 50)])
def test_a_level_name_reads_in_any_case_with_space_around_it(name: str, level: int) -> None:
    """A structlog level name, any case and surrounding space ignored, reads as its stdlib level number."""
    settings = telemetry.LogSettings(level=name, filter={"httpx": name})
    assert (settings.severity, settings.overrides) == (level, {"httpx": level})


def test_unknown_level_names_read_as_notset_and_are_reported() -> None:
    """Names no stdlib level holds read as `NOTSET` (0), each reported with its setting and the names accepted."""
    settings = telemetry.LogSettings(level="loud", filter={"httpx": "chatty", "anyio": "debug"})
    assert (settings.severity, settings.overrides) == (0, {"httpx": 0, "anyio": 10})
    assert [(setting, value) for setting, value, _ in settings.unrecognized] == [("level", "loud"), ("filter", "httpx=chatty")]
    assert all({"debug", "warning", "critical"} <= set(accepted.split()) for _, _, accepted in settings.unrecognized)


def test_a_variable_supplies_each_field_an_argument_leaves_out(monkeypatch: pytest.MonkeyPatch) -> None:
    """`LOG_<FIELD>` variables fill the fields no argument names, and an argument wins over its variable."""
    monkeypatch.setenv("LOG_LEVEL", "error")
    monkeypatch.setenv("LOG_FILES", "3")
    monkeypatch.setenv("LOG_FILTER", '{"httpx": "debug"}')
    settings = telemetry.LogSettings(files=5)
    assert (settings.severity, settings.files, settings.overrides) == (40, 5, {"httpx": 10})
