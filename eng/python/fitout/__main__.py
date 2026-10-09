"""Build and install Blender extensions through the native CLI."""

from pathlib import Path
import subprocess
import sys
from typing import Annotated

from cyclopts import App, Parameter
from cyclopts.types import ResolvedPath
from mcp.types import CallToolResult
from result import Err, Ok, Result
import structlog

from eng.python.fitout.assembly import pack as assembled, PackError
from eng.python.fitout.install import install as installed, InstallError, RemoteFailure

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reported(result: Result[int, PackError | InstallError]) -> int:
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


@app.command
async def pack(source: ResolvedPath, archive: ResolvedPath, /, *, blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender") -> Result[int, PackError]:
    """Build a Blender extension at the requested archive path.

    Args:
        source: Extension directory containing its manifest.
        archive: ZIP file to create or replace.
        blender: Blender executable, supplied by BLENDER_PATH when set.

    Returns:
        Native exit status, or a filesystem or process launch error.
    """
    return await assembled(source, archive, blender, Path.cwd())


@app.command
async def install(
    archive: ResolvedPath, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender"
) -> Result[int, InstallError]:
    """Install and enable a Blender extension in the user repository.

    Args:
        archive: Extension ZIP file to install.
        blender: Blender executable, supplied by BLENDER_PATH when set.
        endpoint: Blender Lab service endpoint, supplied by BLENDER_MCP_URL.

    Returns:
        Native exit status, or a process launch error.
    """
    return await installed(archive, blender, endpoint)


if __name__ == "__main__":
    app()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
