"""Canonical live-resolving structlog config shared by Assay and the quality CLI.

Both packages drive the one process-global structlog config through `configure_logging`;
`_StderrLogger` resolves `sys.stderr` per write, so a sibling suite's pytest-closed capture
fd can never strand a logger on a dead stream (`ValueError: I/O operation on closed file`).
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import sys
import threading
from typing import TYPE_CHECKING

import msgspec
import structlog
from structlog import make_filtering_bound_logger
from structlog.contextvars import merge_contextvars
from structlog.dev import ConsoleRenderer
from structlog.processors import add_log_level, CallsiteParameter, CallsiteParameterAdder, dict_tracebacks, JSONRenderer, TimeStamper

from tools.assay.composition.settings import AssaySettings, LogFormat
from tools.assay.core.aspect import ring_processor


if TYPE_CHECKING:
    from structlog.typing import Processor


# --- [CONSTANTS] ------------------------------------------------------------------------

_INFO: int = 20


# --- [SERVICES] -------------------------------------------------------------------------


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


# --- [COMPOSITION] ----------------------------------------------------------------------

_STDERR = _StderrLogger()
_LATCH: dict[str, bool] = {"configured": False}  # dict cell avoids a module-level `global` rebind


def configure_logging(log_format: LogFormat | None = None) -> None:
    """Install the canonical live-stderr structlog config; idempotent unless a format is forced.

    Args:
        log_format: Renderer format. ``None`` reads ``AssaySettings().log_format`` lazily and is a
            no-op once configured, so re-entry and ``quality.main()`` never re-clobber the global config.
    """
    if _LATCH["configured"] and log_format is None:
        return
    fmt = log_format if log_format is not None else AssaySettings().log_format
    renderer: Processor = (
        JSONRenderer(serializer=lambda v, **_k: msgspec.json.encode(v).decode())  # msgspec encodes datetime + struct types stdlib json cannot
        if fmt is LogFormat.CI
        else ConsoleRenderer(colors=True)
    )
    structlog.configure(
        processors=[
            merge_contextvars,  # must be first; binds contextvars before any processor reads the event dict
            ring_processor,
            add_log_level,
            CallsiteParameterAdder(parameters=(CallsiteParameter.MODULE, CallsiteParameter.FUNC_NAME, CallsiteParameter.LINENO)),
            dict_tracebacks,
            TimeStamper(fmt="iso", utc=True),
            renderer,
        ],
        wrapper_class=make_filtering_bound_logger(_INFO),
        logger_factory=lambda *_a: _STDERR,  # stdout belongs to the Envelope wire
        cache_logger_on_first_use=False,  # caching would freeze processors; pytest capture_logs() and import-time loggers must see the final chain
    )
    _LATCH["configured"] = True


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["configure_logging"]
