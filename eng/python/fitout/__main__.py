"""Packs and installs Blender extensions, syncs their wheels into Blender import folder."""

import subprocess
import sys

from cyclopts import App
from expression import Result
from mcp.types import CallToolResult
import structlog

from eng.python.fitout.install import RemoteFailure

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported[E](result: Result[int, E]) -> int:
    """Returns exit status, writing any failure to stderr."""
    match result:
        case Result(tag="ok", ok=status):
            return status
        case Result(error=ExceptionGroup() as errors):
            structlog.get_logger(__name__).error("Extension command failed", exc_info=errors)
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
app.command("eng.python.fitout.assembly:pack")
app.command("eng.python.fitout.install:install")
app.command("eng.python.fitout.assembly:site")


if __name__ == "__main__":
    app()
