"""Stdlib members a bound logger of `rasm_telemetry.logger` passes through, copied from `TestBoundLogger` of structlog `tests/test_stdlib.py` (26.1.0), and its output with no handler configured, copied from `LastResortTest` of CPython `Lib/test/test_logging.py` (3.15.2)."""

from collections.abc import Iterator
import io
import logging

import pytest

import rasm_telemetry

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("attribute_name", ["name", "level", "parent", "propagate", "handlers", "disabled"])
def test_stdlib_passthrough_attributes(attribute_name: str) -> None:
    """Stdlib logger attributes are available on the bound logger (`test_stdlib.py:239-252`)."""
    stdlib_logger = logging.getLogger("Test")
    stdlib_logger_attribute = getattr(stdlib_logger, attribute_name)
    bl = rasm_telemetry.logger("Test")
    bound_logger_attribute = getattr(bl, attribute_name)
    assert bound_logger_attribute == stdlib_logger_attribute


@pytest.mark.parametrize(
    "method_name,method_args",
    [
        ("addHandler", [None]),
        ("removeHandler", [None]),
        ("hasHandlers", None),
        ("callHandlers", [None]),
        ("handle", [None]),
        ("setLevel", [None]),
        ("getEffectiveLevel", None),
        ("isEnabledFor", [None]),
        ("findCaller", None),
        ("makeRecord", ["name", "debug", "test_func", "1", "test msg", ["foo"], False]),
        ("getChild", [None]),
    ],
)
def test_stdlib_passthrough_methods(method_name: str, method_args: list[object] | None) -> None:
    """Stdlib logger methods are available on the bound logger (`test_stdlib.py:254-304`)."""
    called_stdlib_method = [False]

    def validate(*_args: object, **_kw: object) -> None:
        called_stdlib_method[0] = True

    stdlib_logger = logging.getLogger("Test")
    if getattr(stdlib_logger, method_name, None):
        setattr(stdlib_logger, method_name, validate)
        bl = rasm_telemetry.logger("Test")
        bound_logger_method = getattr(bl, method_name)
        assert bound_logger_method is not None
        if method_args is not None:
            bound_logger_method(*method_args)
        else:
            bound_logger_method()
        assert called_stdlib_method == [True]


@pytest.fixture
def root_hdlr() -> Iterator[logging.Handler]:
    """Yields the stream handler `BaseTest.setUp` puts on the root logger at `DEBUG`, removed as `tearDown` removes it (CPython `Lib/test/test_logging.py:99-144`)."""
    root_logger = logging.getLogger("")
    original_logging_level = root_logger.getEffectiveLevel()
    stream = io.StringIO()
    root_logger.handlers.clear()
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


def test_last_resort(root_hdlr: logging.Handler, capsys: pytest.CaptureFixture[str]) -> None:
    """With no handler anywhere, a warning of the bound logger goes to standard error through `logging.lastResort` (CPython `Lib/test/test_logging.py:5041-5077`)."""
    root = logging.getLogger("")
    log = rasm_telemetry.logger("root")
    root.removeHandler(root_hdlr)
    old_lastresort = logging.lastResort
    old_raise_exceptions = logging.raiseExceptions
    try:
        capsys.readouterr()
        log.debug("This should not appear")
        assert capsys.readouterr().err == ""
        log.warning("Final chance!")
        assert capsys.readouterr().err == "Final chance!\n"
        logging.lastResort = None
        log.warning("Final chance!")
        msg = 'No handlers could be found for logger "root"\n'
        assert capsys.readouterr().err == msg
        log.warning("Final chance!")
        assert capsys.readouterr().err == ""
        root.manager.emittedNoHandlerWarning = False
        logging.raiseExceptions = False
        log.warning("Final chance!")
        assert capsys.readouterr().err == ""
    finally:
        root.addHandler(root_hdlr)
        logging.lastResort = old_lastresort
        logging.raiseExceptions = old_raise_exceptions
