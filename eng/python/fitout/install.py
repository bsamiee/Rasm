"""Installs extensions through Blender's native handler in the GUI session holding the same user profile, else a command process."""

import errno
import os
import shutil
import sys
from typing import Annotated

import anyio
from cyclopts import Parameter
from cyclopts.types import ResolvedPath
import httpx2
from mcp import Client, MCPError
from mcp.types import CallToolResult
import msgspec
import psutil
from pydantic import ValidationError
from result import Err, Ok, Result

# --- [MODELS] ---------------------------------------------------------------------------


class _Completed(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    result: int
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


async def install(
    archive: ResolvedPath, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Annotated[str, Parameter(env_var="BLENDER_PATH")] = "blender"
) -> Result[int, InstallError]:
    """Install and enable a Blender extension in the user repository.

    Args:
        archive: Extension ZIP file to install.
        blender: Blender executable, supplied by BLENDER_PATH when set.
        endpoint: Blender Lab service endpoint, supplied by BLENDER_MCP_URL.

    Returns:
        The native handler's status, or an executable, transport,
        response-decoding, or remote execution error. Native output is forwarded.
    """
    match shutil.which(blender):
        case None:
            return Err(FileNotFoundError(errno.ENOENT, os.strerror(errno.ENOENT), blender))
        case found:
            executable = anyio.Path(found)
    profile = {name: value for name, value in os.environ.items() if name.startswith("BLENDER_USER_")}
    try:
        pids = [
            process.pid
            for process in await anyio.to_thread.run_sync(tuple, psutil.process_iter(("exe", "cmdline", "environ")))
            if (path := process.info["exe"])
            and not {"-b", "--background", "-c", "--command"} & set(process.info["cmdline"] or ())
            and {name: value for name, value in (process.info["environ"] or {}).items() if name.startswith("BLENDER_USER_")} == profile
            and await executable.samefile(path)
        ]
    except OSError as error:
        return Err(error)
    arguments = ("install-file", "--repo", "user_default", "--enable", str(archive))
    if not pids:
        try:
            process = await anyio.run_process((executable, "--command", "extension", *arguments), stdout=None, stderr=None, check=False)
        except OSError as error:
            return Err(error)
        return Ok(process.returncode)

    code = (
        "import os\n"
        "from bl_pkg.bl_extension_cli import cli_extension_handler\n"
        f"if os.getpid() not in {pids!r}:\n"
        "    raise RuntimeError('The connected Blender is not a selected running process')\n"
        f"result = cli_extension_handler({list(arguments)!r})\n"
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
        return Err(response) if isinstance(response, RemoteFailure) else Ok(response.result)
    return failure


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["InstallError", "RemoteFailure", "install"]
