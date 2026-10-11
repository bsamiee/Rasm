"""Build and install Blender extensions through the native CLI."""

import subprocess
import sys

from cyclopts import App
from mcp.types import CallToolResult
from result import Err, Ok, Result
import structlog

from eng.python.fitout.assembly import pack
from eng.python.fitout.install import install, RemoteFailure

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported[E](result: Result[int, E]) -> int:
    """Return native status or render a filesystem or process launch failure."""
    match result:
        case Ok(status):
            return status
        case Err(ExceptionGroup() as errors):
            structlog.get_logger(__name__).error("Extension operation failed", exc_info=errors)
            return 1
        case Err(error):
            if isinstance(error, CallToolResult):
                sys.stderr.write(f"{error.model_dump_json(by_alias=True)}\n")
                return 1
            sys.stderr.write(f"{error.message if isinstance(error, RemoteFailure) else error}\n")
            if isinstance(error, subprocess.CalledProcessError):
                if error.stdout:
                    sys.stdout.buffer.write(error.stdout)
                if error.stderr:
                    sys.stderr.buffer.write(error.stderr)
                return error.returncode
            return 1


# --- [COMPOSITION] ----------------------------------------------------------------------

structlog.configure(logger_factory=structlog.PrintLoggerFactory(file=sys.stderr))
app = App(help=__doc__, result_action=(_reported, "sys_exit"))
app.command(pack)
app.command(install)


if __name__ == "__main__":
    app()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
