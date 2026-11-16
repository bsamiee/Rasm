"""Fixtures every case runs under: the root logger, structlog's configuration, and the OpenTelemetry provider globals put back around each test."""

from collections.abc import Iterator
import io
import logging

from opentelemetry.test.globals_test import reset_logging_globals, reset_metrics_globals, reset_trace_globals
import pytest
import structlog

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(autouse=True)
def ensure_logging_framework_not_altered() -> Iterator[None]:
    """Restores the root logger's handlers and level, as structlog's `_ensure_logging_framework_not_altered` (`tests/conftest.py:26-37`)."""
    before_handlers = logging.getLogger().handlers.copy()
    before_level = logging.getLogger().level
    yield
    logging.getLogger().handlers = before_handlers
    logging.getLogger().setLevel(before_level)


@pytest.fixture(autouse=True)
def reset_config() -> None:
    """Resets structlog's configuration before the test, as structlog's `_reset_config` (`tests/conftest.py:74-76`)."""
    structlog.reset_defaults()


@pytest.fixture(autouse=True)
def reset_globals() -> Iterator[None]:
    """Clears the tracer, meter, and logger provider globals before and after the test, as `TestBase.setUp` and `tearDown` (`test_base.py:36-58`) with the logger provider added."""
    reset_trace_globals()
    reset_metrics_globals()
    reset_logging_globals()
    yield
    reset_trace_globals()
    reset_metrics_globals()
    reset_logging_globals()


@pytest.fixture
def root_hdlr() -> Iterator[logging.StreamHandler[io.StringIO]]:
    """Yields the stream handler `BaseTest.setUp` puts on the root logger at `DEBUG`, removed as `tearDown` removes it (CPython `Lib/test/test_logging.py:99-136,138-167`)."""
    root_logger = logging.getLogger("")
    original_logging_level = root_logger.getEffectiveLevel()
    stream = io.StringIO()
    root_logger.setLevel(logging.DEBUG)
    hdlr = logging.StreamHandler(stream)
    hdlr.setFormatter(logging.Formatter("%(name)s -> %(levelname)s: %(message)s"))
    root_logger.addHandler(hdlr)
    yield hdlr
    stream.close()
    root_logger.removeHandler(hdlr)
    while root_logger.handlers:
        h = root_logger.handlers[0]
        root_logger.removeHandler(h)
        h.close()
    root_logger.setLevel(original_logging_level)
