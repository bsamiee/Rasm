"""Installs extensions through Blender's native handler in the GUI session holding the same user profile, else a command process."""

import os
import sys
from typing import Annotated

import anyio
from cyclopts import Parameter
from cyclopts.types import ResolvedExistingFile, ResolvedPath
import httpx2
from mcp import Client, MCPError
from mcp.types import CallToolResult
import msgspec
import psutil
from pydantic import ValidationError
from result import Err, Ok, Result

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
    archive: ResolvedPath, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]
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
    executable = anyio.Path(blender)
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
    code = (
        "import bpy\n"
        "from bl_pkg.bl_extension_cli import cli_extension_handler\n"
        "from bl_pkg.bl_extension_ops import rna_prop_repo_enum_valid_only_itemf\n"
        "repo = rna_prop_repo_enum_valid_only_itemf(None, bpy.context)[0][0]\n"
        f"result = {{'returncode': cli_extension_handler(['install-file', '--repo', repo, '--enable', {str(archive)!r}])}}\n"
    )
    if not pids:
        try:
            process = await anyio.run_process(
                (executable, "--background", "--python-exit-code", "1", "--python-expr", f"import sys\n{code}sys.exit(result['returncode'])"), stdout=None, stderr=None, check=False
            )
        except OSError as error:
            return Err(error)
        return Ok(process.returncode)

    code = f"import os\nif os.getpid() not in {pids!r}:\n    raise RuntimeError('The connected Blender is not a selected running process')\n" + code
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
        return Err(response) if isinstance(response, RemoteFailure) else Ok(response.result.returncode)
    return failure


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["InstallError", "RemoteFailure", "install"]
