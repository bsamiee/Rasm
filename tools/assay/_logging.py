"""Multi-source structlog owner: one process-global config for every Assay entrypoint.

CLI dispatch, pytest sessions, and embedded callers all converge on `configure_logging`;
a lock-guarded first-caller-wins latch makes concurrent implicit configuration race-free,
while an explicit format always reconfigures. `_StderrLogger` resolves `sys.stderr` per
write (structlog ships no lazy-stream logger factory), so a sibling suite's pytest-closed
capture fd can never strand a logger on a dead stream. A stdlib root-handler bridge routes
third-party `logging` records (asyncssh, fsspec) through the same processor chain to
stderr; stdout stays reserved for the Envelope wire.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import logging
import sys
import threading
from typing import Final, override

import msgspec
from pydantic import ValidationError
import structlog
from structlog import make_filtering_bound_logger
from structlog.contextvars import merge_contextvars
from structlog.dev import ConsoleRenderer
from structlog.processors import add_log_level, CallsiteParameter, CallsiteParameterAdder, dict_tracebacks, JSONRenderer, TimeStamper
from structlog.stdlib import ProcessorFormatter
from structlog.typing import Processor  # noqa: TC002  # beartype claw resolves the _chain return annotation at runtime

from tools.assay.composition.settings import AssaySettings, LogFormat
from tools.assay.core.aspect import ring_processor


# --- [CONSTANTS] ------------------------------------------------------------------------

_LEVELS: Final[dict[str, int]] = {
    "debug": logging.DEBUG,
    "info": logging.INFO,
    "warning": logging.WARNING,
    "error": logging.ERROR,
    "critical": logging.CRITICAL,
}


# --- [SERVICES] -------------------------------------------------------------------------

_LOG_ENCODER: Final = msgspec.json.Encoder(enc_hook=str)  # telemetry degrades unencodable values to str; core.model's wire encoder stays strict


class _StderrLogger:
    """Resolve `sys.stderr` per write; the stream is never captured at configure time."""

    _lock = threading.Lock()

    def msg(self, message: str) -> None:
        with self._lock:
            stream = sys.stderr
            stream.write(message + "\n")
            stream.flush()

    debug = info = warning = error = critical = msg  # FilteringBoundLogger._proxy_to_logger resolves all level names by attribute
    log = warn = fatal = failure = err = exception = msg


_STDERR = _StderrLogger()


class _StderrBridgeHandler(logging.Handler):
    """Route stdlib `logging` records through the shared processor chain to per-write-resolved `sys.stderr`."""

    @override
    def emit(self, record: logging.LogRecord) -> None:
        """Render one stdlib record via the attached ProcessorFormatter and write it to the live stderr."""
        _STDERR.msg(self.format(record))


# --- [COMPOSITION] ----------------------------------------------------------------------

_LOCK = threading.Lock()
_LATCH: dict[str, bool] = {"configured": False}  # dict cell avoids a module-level `global` rebind


def _chain() -> tuple[Processor, ...]:
    return (
        merge_contextvars,  # must be first; binds contextvars before any processor reads the event dict
        ring_processor,
        add_log_level,
        CallsiteParameterAdder(parameters=(CallsiteParameter.QUAL_MODULE, CallsiteParameter.FUNC_NAME, CallsiteParameter.LINENO)),
        dict_tracebacks,
        TimeStamper(fmt="iso", utc=True),
    )


def _renderer(fmt: LogFormat) -> Processor:
    match fmt:
        case LogFormat.CI:
            return JSONRenderer(serializer=lambda v, **_k: _LOG_ENCODER.encode(v).decode())
        case _:
            return ConsoleRenderer(colors=True)


def configure_logging(log_format: LogFormat | None = None) -> None:
    """Install the process-global structlog config and stdlib root-logger bridge; first caller wins.

    Args:
        log_format: Renderer format. ``None`` reads ``AssaySettings().log_format`` lazily and is a
            no-op once configured, so later implicit callers inherit the first configuration; an
            explicit format always reconfigures and re-resolves ``ASSAY_LOG_LEVEL``.
    """
    with _LOCK:
        if _LATCH["configured"] and log_format is None:
            return
        try:
            settings = AssaySettings()
        except ValidationError:
            settings = AssaySettings.model_construct()  # broken env surfaces at dispatch (bootstrap_error); logging falls back to field defaults
        fmt = log_format if log_format is not None else settings.log_format
        chain = _chain()
        renderer = _renderer(fmt)
        level = _LEVELS[settings.log_level]
        structlog.configure(
            processors=[*chain, renderer],
            wrapper_class=make_filtering_bound_logger(level),
            logger_factory=lambda *_a: _STDERR,  # stdout belongs to the Envelope wire
            cache_logger_on_first_use=False,  # caching would freeze processors; capture_logs() and import-time loggers must see the final chain
        )
        bridge = _StderrBridgeHandler(level=level)
        bridge.setFormatter(ProcessorFormatter(foreign_pre_chain=chain, processors=[ProcessorFormatter.remove_processors_meta, renderer]))
        root = logging.getLogger()  # noqa: TID251  # the bridge owner is the one sanctioned root-logger touchpoint
        root.handlers[:] = [bridge]  # replaced, never stacked: reconfiguring swaps the bridge; third-party stdlib records flow structured to stderr
        root.setLevel(level)
        _LATCH["configured"] = True


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["configure_logging"]
