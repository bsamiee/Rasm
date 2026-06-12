"""Multi-source logging laws for tools.assay [configure_logging].

Proves the four caller-visible contracts of the process-global logging owner: the configured
``logger_factory`` resolves ``sys.stderr`` per write (a stream closed since configure time cannot
strand a later log on a dead fd); stdlib ``logging`` records bridge structured to stderr through the
same processor chain; values msgspec cannot natively encode degrade to ``str`` instead of raising;
``ASSAY_LOG_LEVEL`` gates both rails; and reconfiguring never stacks bridge handlers on the root
logger. Drives the installed logger directly rather than through ``capture_logs`` (which bypasses
the factory).
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # runtime: SinkLaw.drive/check are msgspec.Struct field annotations resolved at definition time
import io
from itertools import starmap
import logging
import threading

import msgspec
import pytest
import structlog

from tests.python._testkit.laws import register_laws
from tools.assay import configure_logging
from tools.assay._logging import _StderrBridgeHandler, _StderrLogger  # private factory products are the test surface
from tools.assay.composition.settings import LogFormat


# --- [TABLES]


class _SinkLaw(msgspec.Struct, frozen=True, gc=False):
    """One stderr-sink logging law: env applied before ``configure_logging``, a driver, and a payload check.

    ``drive`` receives the installed logger factory's product, the active ``monkeypatch`` (so a row may
    reassign ``sys.stderr`` mid-write), and the initial sink; it performs the emission(s) and returns the
    text to assert (usually the final sink's value). ``check`` legislates that text — a decoded JSON payload,
    a substring, or a suppression check — keeping every row's distinguishing assertion intact.
    """

    label: str
    drive: Callable[[_StderrLogger, pytest.MonkeyPatch, io.StringIO], str]
    check: Callable[[str], None]
    env: tuple[tuple[str, str], ...] = ()


def _emit_then_value(emit: Callable[[], None]) -> Callable[[_StderrLogger, pytest.MonkeyPatch, io.StringIO], str]:
    """Build a ``drive`` that runs ``emit`` against the scaffold's single sink and returns its text.

    Returns:
        A ``SinkLaw.drive`` closure ignoring the logger/monkeypatch and projecting ``sink.getvalue()``.
    """
    return lambda _logger, _mp, sink: (emit(), sink.getvalue())[1]


def _stdlib_warn(event: str) -> Callable[[], None]:
    """An emitter warning ``event`` through the stdlib rail (the foreign-record bridge subject).

    Returns:
        A zero-arg emitter; ``logging`` is the law's subject so the project-internal-rail lint is waived.
    """
    return lambda: logging.getLogger("asyncssh.bridge.law").warning(event)  # noqa: TID251  # the stdlib rail is the law's subject


def _structured_payload(event: str) -> Callable[[str], None]:
    """Assert the decoded one-line JSON payload carries ``event``/``warning``/``timestamp`` (structured-bridge law).

    Returns:
        A ``SinkLaw.check`` over the sink text.
    """

    def check(text: str) -> None:
        lines = text.splitlines()
        assert len(lines) == 1
        payload = msgspec.json.decode(lines[0])
        assert payload["event"] == event
        assert payload["level"] == "warning"
        assert "timestamp" in payload

    return check


def _module_callsite(text: str) -> None:
    """A synthesized foreign ``LogRecord`` renders ``module`` (``record.module``) but never ``qual_module``.

    ``CallsiteParameterAdder._record_attribute_map`` has no ``QUAL_MODULE`` entry, so a stdlib record bridged
    through the foreign pre-chain carries ``module`` only. Kills any mutant dropping ``CallsiteParameter.MODULE``
    from the adder tuple — without it the foreign record renders no module field at all.
    """
    payload = msgspec.json.decode(text)
    assert payload["module"] == "test_logging"
    assert "qual_module" not in payload


def _unencodable_degrades(text: str) -> None:
    """A ``threading.Lock`` and a lambda degrade to ``str`` in the payload instead of raising (encoder fallback law)."""
    payload = msgspec.json.decode(text)
    assert payload["event"] == "unencodable-event"
    assert isinstance(payload["lock"], str)
    assert isinstance(payload["fn"], str)


def _resolve_per_write(logger: _StderrLogger, monkeypatch: pytest.MonkeyPatch, first: io.StringIO) -> str:
    """Prove ``_StderrLogger`` resolves ``sys.stderr`` per write: a sink closed since configure cannot strand a later log.

    Writes to the scaffold's first sink, closes it, reassigns a second sink, writes again, and returns the
    second sink's text. The intermediate first-sink assertion stays inline.

    Returns:
        The second sink's accumulated text for the matrix ``check``.
    """
    logger.info("before-close")
    assert "before-close" in first.getvalue()
    first.close()
    second = io.StringIO()
    monkeypatch.setattr("sys.stderr", second)
    logger.info("after-reassign")
    return second.getvalue()


def _error_suppresses_info(_logger: _StderrLogger, _monkeypatch: pytest.MonkeyPatch, sink: io.StringIO) -> str:
    """Under ``ASSAY_LOG_LEVEL=error`` both rails drop ``info`` and only ``error`` reaches the sink.

    Returns:
        The sink text after the error emission, for the matrix ``check`` (which asserts the emitted event).
    """
    structlog.get_logger("assay.bridge.law").info("suppressed-event")
    logging.getLogger("assay.bridge.law").info("suppressed-stdlib-event")  # noqa: TID251  # the stdlib rail is the law's subject
    assert not sink.getvalue()
    structlog.get_logger("assay.bridge.law").error("emitted-event")
    return sink.getvalue()


def _contains(substring: str) -> Callable[[str], None]:
    """Assert ``substring`` reaches the sink text — the reassigned-sink and post-suppression-emit witnesses.

    Returns:
        A ``SinkLaw.check`` asserting ``substring in text``.
    """
    return lambda text: _assert_in(substring, text)


def _assert_in(substring: str, text: str) -> None:
    assert substring in text, f"{substring!r} absent from sink text {text!r}"


_SINK_LAWS: tuple[_SinkLaw, ...] = (
    _SinkLaw("resolves-stderr-per-write", _resolve_per_write, _contains("after-reassign")),
    _SinkLaw("stdlib-bridge-structured", _emit_then_value(_stdlib_warn("stdlib-bridge-event")), _structured_payload("stdlib-bridge-event")),
    _SinkLaw("stdlib-bridge-module-callsite", _emit_then_value(_stdlib_warn("module-callsite-event")), _module_callsite),
    _SinkLaw(
        "unencodable-degrades-to-str",
        _emit_then_value(lambda: structlog.get_logger("assay.bridge.law").warning("unencodable-event", lock=threading.Lock(), fn=lambda: None)),
        _unencodable_degrades,
    ),
    _SinkLaw("env-error-suppresses-info", _error_suppresses_info, _contains("emitted-event"), env=(("ASSAY_LOG_LEVEL", "error"),)),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("row", _SINK_LAWS, ids=[r.label for r in _SINK_LAWS])
def test_configure_logging_stderr_sink_matrix(row: _SinkLaw, monkeypatch: pytest.MonkeyPatch) -> None:
    """``configure_logging`` drives every emission to a freshly-resolved ``sys.stderr`` per row.

    Covers per-write resolution, the stdlib structured bridge, foreign-record ``module``, unencodable-value
    str degradation, and ``ASSAY_LOG_LEVEL`` gating. Each row applies its pre-configure ``env``, installs a
    fresh ``StringIO`` sink, drives its emission(s) via the factory's ``_StderrLogger`` product, and legislates
    the resulting text — every distinguishing assertion of the five original sink laws preserved as a row
    ``check``. ``env`` is reset by ``monkeypatch`` teardown; the default config is then reinstalled so no row
    strands ``ASSAY_LOG_LEVEL`` for the next test.
    """
    list(starmap(monkeypatch.setenv, row.env))
    configure_logging(LogFormat.CI)
    logger = structlog.get_config()["logger_factory"]()
    assert isinstance(logger, _StderrLogger)
    sink = io.StringIO()
    monkeypatch.setattr("sys.stderr", sink)
    row.check(row.drive(logger, monkeypatch, sink))
    monkeypatch.undo()
    configure_logging(LogFormat.CI)


def test_reconfigure_does_not_stack_bridge_handlers() -> None:
    configure_logging(LogFormat.CI)
    configure_logging(LogFormat.CI)

    root = logging.getLogger()  # noqa: TID251  # the root-handler set is the law's subject
    bridges = [h for h in root.handlers if isinstance(h, _StderrBridgeHandler)]
    assert len(bridges) == 1
    assert root.handlers == bridges


# --- [COMPOSITION] ----------------------------------------------------------------------

register_laws((configure_logging, ("test_configure_logging_stderr_sink_matrix", "test_reconfigure_does_not_stack_bridge_handlers")))
