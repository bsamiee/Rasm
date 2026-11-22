"""Process state each case runs under and the records it reads: the root logger, structlog's configuration, and the OpenTelemetry provider globals put back around each test, and the decoded JSON lines of a library handler."""

from collections.abc import Iterator
import io
import logging
from typing import override

import msgspec
from opentelemetry.test.globals_test import reset_logging_globals, reset_metrics_globals, reset_trace_globals
import pytest
import structlog

from rasm.telemetry import LogSettings

# --- [SERVICES] -------------------------------------------------------------------------


class _Decoding(io.StringIO):
    """Text stream decoding each JSON line written to it into a list."""

    def __init__(self, written: list[dict[str, object]], /) -> None:
        """Keeps the list each decoded line is appended to."""
        super().__init__()
        self.written = written

    @override
    def write(self, text: str, /) -> int:
        """Appends each line of the text as its decoded JSON object."""
        self.written.extend(msgspec.json.decode(line, type=dict[str, object]) for line in text.splitlines())
        return len(text)


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(autouse=True)
def isolated() -> Iterator[None]:
    """Restores the root logger's handlers and level, structlog's configuration, and the tracer, meter, and logger provider globals around the test."""
    root = logging.getLogger()
    handlers, level = root.handlers.copy(), root.level
    structlog.reset_defaults()
    reset_trace_globals()
    reset_metrics_globals()
    reset_logging_globals()
    yield
    root.handlers, root.level = handlers, level
    structlog.reset_defaults()
    reset_trace_globals()
    reset_metrics_globals()
    reset_logging_globals()


@pytest.fixture
def written() -> list[dict[str, object]]:
    """Returns the records the test's handler wrote, each line decoded, in write order."""
    return []


@pytest.fixture
def handler(written: list[dict[str, object]]) -> logging.Handler:
    """Returns the library handler of default settings writing for service `suite` at version `1.2.3`, each line decoded into `written`."""
    return LogSettings().handler(_Decoding(written), service="suite", version="1.2.3")
