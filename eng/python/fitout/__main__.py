"""Build and install Blender extensions through the native CLI, and sync the distributions they bundle as wheels into a folder Blender imports from."""

import subprocess
import sys

from cyclopts import App
from expression import Result
from mcp.types import CallToolResult
import structlog

from eng.python.fitout.assembly import pack, site
from eng.python.fitout.install import install, RemoteFailure

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported[E](result: Result[int, E]) -> int:
    """Return the native status, or render the failure and return the failed child's status or 1."""
    match result:
        case Result(tag="ok", ok=status):
            return status
        case Result(error=ExceptionGroup() as errors):
            structlog.get_logger(__name__).error("Extension operation failed", exc_info=errors)
            return 1
        case Result(error=CallToolResult() as error):
            sys.stderr.write(f"{error.model_dump_json(by_alias=True)}\n")
            return 1
        case Result(error=RemoteFailure(message=message)):
            sys.stderr.write(f"{message}\n")
            return 1
        case Result(error=subprocess.CalledProcessError(returncode=status) as error):
            sys.stderr.write(f"{error}\n")
            return status
        case Result(error=error):
            sys.stderr.write(f"{error}\n")
            return 1


# --- [COMPOSITION] ----------------------------------------------------------------------

structlog.configure(logger_factory=structlog.PrintLoggerFactory(file=sys.stderr))
app = App(help=__doc__, result_action=(_reported, "sys_exit"))
app.command(pack)
app.command(install)
app.command(site)


if __name__ == "__main__":
    app()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
