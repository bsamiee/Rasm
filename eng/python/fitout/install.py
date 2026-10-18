"""Installs extensions through Blender's native handler in the GUI session holding the same user profile, else a command process."""

import os
import sys
from typing import Annotated

import anyio
from cyclopts import Parameter
from cyclopts.types import ResolvedExistingFile
from expression import Error, Ok, Result
from expression.extra.result import catch
import httpx2
from mcp import Client, MCPError
from mcp.types import CallToolResult
import msgspec
import psutil
from pydantic import ValidationError

# --- [MODELS] ---------------------------------------------------------------------------


class _Status(msgspec.Struct, frozen=True):
    returncode: int


class _Completed(msgspec.Struct, frozen=True, tag="ok", tag_field="status"):
    result: _Status
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
    archive: ResolvedExistingFile, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]
) -> Result[int, InstallError]:
    """Install and enable a Blender extension in the user repository.

    Args:
        archive: Extension ZIP file to install.
        blender: Blender executable, supplied by BLENDER_PATH.
        endpoint: Blender Lab service endpoint, supplied by BLENDER_MCP_URL.

    Returns:
        The native handler's status, or a process, transport,
        response-decoding, or remote execution error. Native output is forwarded.
    """
    profile = {name: value for name, value in os.environ.items() if name.startswith("BLENDER_USER_")}
    pids = await anyio.to_thread.run_sync(
        lambda: [
            process.pid
            for process in psutil.process_iter(("exe", "cmdline", "environ"))
            if (path := process.info["exe"])
            and (cmdline := process.info["cmdline"]) is not None
            and not {"-b", "--background", "-c", "--command"} & set(cmdline)
            and (environ := process.info["environ"]) is not None
            and {name: value for name, value in environ.items() if name.startswith("BLENDER_USER_")} == profile
            and catch(lambda: blender.samefile(path), exception=OSError)().default_with(lambda _: False)
        ]
    )
    arguments = ["install-file", "--repo", "user_default", "--enable", str(archive)]
    if not pids:
        try:
            process = await anyio.run_process((blender, "--command", "extension", *arguments), stdout=None, stderr=None, check=False)
        except OSError as error:
            return Error(error)
        return Ok(process.returncode)

    def decoded(result: CallToolResult) -> Result[_Completed | RemoteFailure, InstallError]:
        return Error(result) if result.is_error else catch(lambda: msgspec.convert(result.structured_content, type=_Completed | RemoteFailure), exception=msgspec.ValidationError)()

    def forwarded(response: _Completed | RemoteFailure) -> Result[int, InstallError]:
        sys.stdout.write(response.stdout)
        sys.stderr.write(response.stderr)
        return Error(response) if isinstance(response, RemoteFailure) else Ok(response.result.returncode)

    code = (
        f"import os\nif os.getpid() not in {pids!r}:\n    raise RuntimeError('The connected Blender is not a selected running process')\n"
        f"from bl_pkg.bl_extension_cli import cli_extension_handler\nresult = {{'returncode': cli_extension_handler({arguments!r})}}\n"
    )
    try:
        async with Client(endpoint) as client:
            called = await client.call_tool("execute_blender_code", {"code": code})
    except* (OSError, httpx2.HTTPError, MCPError, ValidationError) as errors:
        failed = Error(errors)
    else:
        return decoded(called).bind(forwarded)
    return failed


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["InstallError", "RemoteFailure", "install"]
