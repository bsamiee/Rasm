"""Level names of `rasm_telemetry.LogSettings` read as stdlib levels, copied from `GetLogLevelTestCase` of opentelemetry-instrumentation-logging `tests/test_handler.py` (0.66b2) and `BuiltinLevelsTest.test_flat` of CPython `Lib/test/test_logging.py` (3.15.2)."""

import io
import itertools
import logging
import re

import pytest

from rasm_telemetry import LogSettings

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "level,expected",
    [
        pytest.param("notset", logging.NOTSET, id="test_get_log_level_notset"),
        pytest.param(" NOTSET ", logging.NOTSET, id="test_get_log_level_notset_with_whitespace"),
        pytest.param(" DeBug ", logging.DEBUG, id="test_get_log_level_debug"),
        pytest.param(" info ", logging.INFO, id="test_get_log_level_info"),
        pytest.param(" warnING ", logging.WARNING, id="test_get_log_level_warning"),
        pytest.param(" eRroR", logging.ERROR, id="test_get_log_level_error"),
    ],
)
def test_get_log_level(level: str, expected: int) -> None:
    """A level name in any case and with surrounding space reads as its stdlib level (`test_handler.py:752-768`)."""
    assert LogSettings(level=level).severity == expected


def test_get_log_level_invalid_falls_back_to_notset() -> None:
    """An unknown level name reads as `NOTSET` and is reported by name (`test_handler.py:770-773`)."""
    settings = LogSettings(level="foobar")
    assert settings.severity == logging.NOTSET
    assert any("foobar" in line for line in settings.unrecognized)


def test_flat(root_hdlr: logging.StreamHandler[io.StringIO]) -> None:
    """Loggers in a flat namespace pass the levels at and above the one a settings record gives them (CPython `Lib/test/test_logging.py:197-242`)."""
    m = map(str, itertools.count(1)).__next__
    err = logging.getLogger("ERR")
    err.setLevel(LogSettings(level="error").severity)
    inf = logging.LoggerAdapter(logging.getLogger("INF"), {})
    inf.setLevel(LogSettings(level="info").severity)
    deb = logging.getLogger("DEB")
    deb.setLevel(LogSettings(level="debug").severity)
    err.log(logging.CRITICAL, m())
    err.error(m())
    inf.log(logging.CRITICAL, m())
    inf.error(m())
    inf.warning(m())
    inf.info(m())
    deb.log(logging.CRITICAL, m())
    deb.error(m())
    deb.warning(m())
    deb.info(m())
    deb.debug(m())
    err.warning(m())
    err.info(m())
    err.debug(m())
    inf.debug(m())
    passed = {"ERR": ("CRITICAL", "ERROR"), "INF": ("CRITICAL", "ERROR", "WARNING", "INFO"), "DEB": ("CRITICAL", "ERROR", "WARNING", "INFO", "DEBUG")}
    expected_values = [(name, level, str(number)) for number, (name, level) in enumerate(((name, level) for name, levels in passed.items() for level in levels), 1)]
    stream = root_hdlr.stream
    pat = re.compile(r"^([\w.]+) -> (\w+): (\d+)$")
    actual_lines = stream.getvalue().splitlines()
    assert len(actual_lines) == len(expected_values)
    for actual, expected in zip(actual_lines, expected_values, strict=True):
        match = pat.search(actual)
        assert match, "Log line does not match expected pattern:\n" + actual
        assert tuple(match.groups()) == expected
    s = stream.read()
    assert not s, "Remaining output at end of log stream:\n" + s
