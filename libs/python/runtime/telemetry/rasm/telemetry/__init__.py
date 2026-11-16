"""Log settings, entries as JSON records in rotated files through structlog and stdlib handlers, child process output relayed as records, unhandled exceptions as entries, and trace context on entries."""

__lazy_modules__ = frozenset({"rasm.telemetry._entries"})

from rasm.telemetry._entries import correlated, logger, LogSettings, PROCESSORS, relayed, unhandled

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROCESSORS", "LogSettings", "correlated", "logger", "relayed", "unhandled"]
