"""Stdlib members a bound logger of `rasm_telemetry.logger` passes through, copied from `TestBoundLogger` of structlog `tests/test_stdlib.py` (26.1.0), and its output with no handler configured, copied from `LastResortTest` of CPython `Lib/test/test_logging.py` (3.15.2)."""

import contextlib
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


def test_is_enabled_for() -> None:
    """`is_enabled_for` answers from the wrapped logger's level, set from a settings record (`test_stdlib.py:306-316`)."""
    stdlib_logger = logging.getLogger("test_is_enabled_for")
    stdlib_logger.setLevel(rasm_telemetry.LogSettings(level="warning").severity)
    bl = rasm_telemetry.logger("test_is_enabled_for")
    assert (bl.is_enabled_for(logging.WARNING), bl.is_enabled_for(logging.DEBUG)) == (True, False)


def test_get_effective_level() -> None:
    """`get_effective_level` answers the wrapped logger's level, set from a settings record (`test_stdlib.py:318-328`)."""
    stdlib_logger = logging.getLogger("test_get_effective_level")
    stdlib_logger.setLevel(rasm_telemetry.LogSettings(level="warning").severity)
    bl = rasm_telemetry.logger("test_get_effective_level")
    assert bl.get_effective_level() == logging.WARNING


def test_last_resort(root_hdlr: logging.Handler) -> None:
    """With no handler anywhere, a warning of the bound logger goes to standard error through `logging.lastResort` (CPython `Lib/test/test_logging.py:5042-5077`)."""
    root = logging.getLogger("")
    log = rasm_telemetry.logger("root")
    root.handlers = [root_hdlr]
    root.removeHandler(root_hdlr)
    old_lastresort = logging.lastResort
    old_raise_exceptions = logging.raiseExceptions
    try:
        with contextlib.redirect_stderr(io.StringIO()) as stderr:
            log.debug("This should not appear")
            assert not stderr.getvalue()
            log.warning("Final chance!")
            assert stderr.getvalue() == "Final chance!\n"
        logging.lastResort = None
        with contextlib.redirect_stderr(io.StringIO()) as stderr:
            log.warning("Final chance!")
            msg = 'No handlers could be found for logger "root"\n'
            assert stderr.getvalue() == msg
        with contextlib.redirect_stderr(io.StringIO()) as stderr:
            log.warning("Final chance!")
            assert not stderr.getvalue()
        root.manager.emittedNoHandlerWarning = False
        logging.raiseExceptions = False
        with contextlib.redirect_stderr(io.StringIO()) as stderr:
            log.warning("Final chance!")
            assert not stderr.getvalue()
    finally:
        root.addHandler(root_hdlr)
        logging.lastResort = old_lastresort
        logging.raiseExceptions = old_raise_exceptions
