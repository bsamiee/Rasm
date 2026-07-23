"""Own process-wide structlog and stdlib logging configuration.

`configure_logging` is the single entrypoint for CLI, tests, and embedded callers.
Stderr resolves per write so capture lifecycles cannot strand loggers on closed
streams; stdout stays reserved for the Envelope wire.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import logging
import sys
import threading
from typing import assert_never, Final, override

import msgspec
from pydantic import ValidationError
import structlog
from structlog import make_filtering_bound_logger
from structlog.contextvars import merge_contextvars
from structlog.dev import ConsoleRenderer
from structlog.processors import add_log_level, CallsiteParameter, CallsiteParameterAdder, dict_tracebacks, JSONRenderer, TimeStamper
from structlog.stdlib import ProcessorFormatter
from structlog.typing import (
    Processor,  # ruff:ignore[typing-only-third-party-import]  # beartype claw resolves the _chain return annotation at runtime
)

from tools.assay.composition.settings import AssaySettings, LogFormat
from tools.assay.core.aspect import ring_processor


# --- [CONSTANTS] ------------------------------------------------------------------------

_LEVELS: Final[dict[str, int]] = logging.getLevelNamesMapping()

# --- [SERVICES] -------------------------------------------------------------------------

_LOG_ENCODER: Final = msgspec.json.Encoder(enc_hook=str)  # telemetry coerces unencodable fields; Envelope encoding stays strict


class _StderrLogger:
    """Write each event to the current ``sys.stderr``."""

    _lock = threading.Lock()

    def msg(self, message: str) -> None:
        with self._lock:
            stream = sys.stderr
            stream.write(message + "\n")
            stream.flush()

    debug = info = warning = error = critical = msg  # FilteringBoundLogger resolves levels by attribute
    log = warn = fatal = failure = err = exception = msg


_STDERR = _StderrLogger()


class _StderrBridgeHandler(logging.Handler):
    """Route stdlib records through the shared processor chain."""

    @override
    def emit(self, record: logging.LogRecord) -> None:
        _STDERR.msg(self.format(record))


# --- [COMPOSITION] ----------------------------------------------------------------------

_LOCK = threading.Lock()
_LATCH: dict[str, bool] = {"configured": False}  # dict cell avoids a module-level `global` rebind


def _chain() -> tuple[Processor, ...]:
    return (
        merge_contextvars,  # contextvars must bind before processors inspect event data
        ring_processor,
        add_log_level,
        CallsiteParameterAdder(  # MODULE is the portable foreign-record fallback for callsite metadata
            parameters=(CallsiteParameter.QUAL_MODULE, CallsiteParameter.MODULE, CallsiteParameter.FUNC_NAME, CallsiteParameter.LINENO)
        ),
        dict_tracebacks,
        TimeStamper(fmt="iso", utc=True),
    )


def _renderer(fmt: LogFormat) -> Processor:
    match fmt:
        case LogFormat.CI:
            return JSONRenderer(serializer=lambda v, **_k: _LOG_ENCODER.encode(v).decode())
        case LogFormat.HUMAN:
            return ConsoleRenderer(colors=True)
        case never:
            assert_never(never)


def configure_logging(log_format: LogFormat | None = None) -> None:
    """Install process-global structlog and stdlib bridge configuration.

    Args:
        log_format: Explicit renderer format. ``None`` lazily reads settings and becomes a
            no-op after first configuration; explicit values reconfigure and refresh log level.
    """
    with _LOCK:
        if _LATCH["configured"] and log_format is None:
            return
        try:
            settings = AssaySettings()
        except ValidationError:
            settings = AssaySettings.model_construct()  # invalid env surfaces at dispatch; logging uses defaults
        fmt = log_format if log_format is not None else settings.log_format
        chain = _chain()
        renderer = _renderer(fmt)
        level = _LEVELS[settings.log_level.upper()]
        structlog.configure(
            processors=[*chain, renderer],
            wrapper_class=make_filtering_bound_logger(level),
            logger_factory=lambda *_a: _STDERR,  # stdout belongs to the Envelope wire
            cache_logger_on_first_use=False,  # capture_logs() and import-time loggers must see the final chain
        )
        bridge = _StderrBridgeHandler(level=level)
        bridge.setFormatter(ProcessorFormatter(foreign_pre_chain=chain, processors=[ProcessorFormatter.remove_processors_meta, renderer]))
        root = logging.getLogger()  # ruff:ignore[banned-api]  # the bridge owner is the one sanctioned root-logger touchpoint
        root.handlers[:] = [bridge]  # replace rather than stack bridges on reconfiguration
        root.setLevel(level)
        _LATCH["configured"] = True


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["configure_logging"]
