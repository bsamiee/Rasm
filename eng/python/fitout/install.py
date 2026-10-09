# ast-grep-ignore: no-parse-error-python

"""Installs extensions through Blender's native handler in the owning process."""

import errno
from pathlib import Path
import shutil
import sys

import anyio
import httpx2
from mcp import Client, MCPError
from mcp.types import CallToolResult
import msgspec
from pydantic import ValidationError
from result import Err, Ok, Result
lazy from AppKit import NSWorkspace
lazy from Foundation import NSURL

# --- [MODELS] ---------------------------------------------------------------------------


class _Completed(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    result: dict[str, int]
    stdout: str = ""
    stderr: str = ""


# --- [ERRORS] ---------------------------------------------------------------------------


class RemoteFailure(msgspec.Struct, frozen=True, tag="error", tag_field="status"):
    """Connected Blender process's execution traceback and captured output."""

    message: str
    stdout: str = ""
    stderr: str = ""


type InstallError = OSError | msgspec.ValidationError | ExceptionGroup[OSError | httpx2.HTTPError | MCPError | ValidationError] | CallToolResult | RemoteFailure


# --- [OPERATIONS] -----------------------------------------------------------------------


async def install(archive: Path, blender: str, endpoint: str) -> Result[int, InstallError]:
    """Installs and enables an archive in the selected Blender's user repository.

    Args:
        archive: Native extension archive to install or replace.
        blender: Executable identifying the Blender installation.
        endpoint: Native Blender Lab execution service endpoint.

    Returns:
        The native handler's status, or an executable, transport,
        response-decoding, or remote execution error. Native output is forwarded.
    """
    if (executable := shutil.which(blender)) is None:
        return Err(FileNotFoundError(errno.ENOENT, "Blender executable was not found", blender))
    executable_url = NSURL.fileURLWithPath_(executable).URLByResolvingSymlinksInPath()
    pids = tuple(application.processIdentifier() for application in NSWorkspace.sharedWorkspace().runningApplications() if application.executableURL() == executable_url)
    arguments = ("install-file", "--repo", "user_default", "--enable", str(archive))
    if not pids:
        try:
            process = await anyio.run_process((executable, "--command", "extension", *arguments), stdout=None, stderr=None, check=False)
        except OSError as error:
            return Err(error)
        return Ok(process.returncode)

    code = (
        "import bpy, os\n"
        "from bl_pkg.bl_extension_cli import cli_extension_handler\n"
        f"if os.getpid() not in {pids!r} or bpy.app.background:\n"
        "    raise RuntimeError('The connected Blender is not a selected running GUI process')\n"
        f"result = {{'returncode': cli_extension_handler({list(arguments)!r})}}\n"
    )
    try:
        async with Client(endpoint) as client:
            result = await client.call_tool("execute_blender_code", {"code": code})
    except* (OSError, httpx2.HTTPError, MCPError, ValidationError) as errors:
        failure = Err(errors)
    else:
        if result.is_error:
            return Err(result)
        try:
            response: _Completed | RemoteFailure = msgspec.convert(result.structured_content, type=_Completed | RemoteFailure)
        except msgspec.ValidationError as error:
            return Err(error)
        sys.stdout.write(response.stdout)
        sys.stderr.write(response.stderr)
        return Err(response) if isinstance(response, RemoteFailure) else Ok(response.result["returncode"])
    return failure


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["InstallError", "RemoteFailure", "install"]
