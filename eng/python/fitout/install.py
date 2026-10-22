"""Installs an extension archive through Blender's native handler and records it enabled, in the GUI session holding the same user profile, else a background process."""

import os
from pathlib import Path
import sys
from typing import Annotated
from zipfile import ZipFile

import anyio
from cyclopts import Parameter
from cyclopts.types import ResolvedExistingDirectory, ResolvedExistingFile
from expression import Error, Ok, Result
from expression.extra.result import catch
import httpx2
from mcp import Client, MCPError
from mcp.types import CallToolResult
import msgspec
import psutil
from pydantic import ValidationError

# --- [CONSTANTS] ------------------------------------------------------------------------

_REPOSITORY = "user_default"

# --- [MODELS] ---------------------------------------------------------------------------


class _Manifest(msgspec.Struct, frozen=True):
    id: str


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


def _reconciled(archive: Path, *flags: str) -> str:
    """Return Blender Python installing the archive and keeping its extension's preference row, so each startup syncs its wheels before any extension imports them."""
    with ZipFile(archive) as bundle:
        module = f"bl_ext.{_REPOSITORY}.{msgspec.toml.decode(bundle.read('blender_manifest.toml'), type=_Manifest).id}"
    arguments = ["install-file", "--repo", _REPOSITORY, *flags, str(archive)]
    return (
        f"import bpy\nfrom bl_pkg.bl_extension_cli import cli_extension_handler\nresult = {{'returncode': cli_extension_handler({arguments!r})}}\n"
        f"if (addons := bpy.context.preferences.addons).get({module!r}) is None:\n    addons.new().module = {module!r}\n    bpy.ops.wm.save_userpref()\n"
    )


async def install(
    directory: ResolvedExistingDirectory, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]
) -> Result[int, InstallError]:
    """Install the archive `pack` built for this host into the user repository and keep its extension enabled.

    A GUI session holding the same profile loads it in place, and every later startup loads it with
    its wheels extracted before any extension imports a distribution they share.

    Args:
        directory: Folder holding the extension's archive.
        blender: Blender executable, supplied by BLENDER_PATH.
        endpoint: Blender Lab service endpoint, supplied by BLENDER_MCP_URL.

    Returns:
        The native handler's status, or a process, transport,
        response-decoding, or remote execution error. Native output is forwarded.
    """
    (archive,) = directory.glob("*.zip")
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
    if not pids:
        try:
            process = await anyio.run_process(
                (blender, "--background", "--python-exit-code", "1", "--python-expr", f"{_reconciled(archive)}import sys\nsys.exit(result['returncode'])\n"), stdout=None, stderr=None, check=False
            )
        except OSError as error:
            return Error(error)
        return Ok(process.returncode)

    def decoded(result: CallToolResult) -> Result[_Completed | RemoteFailure, InstallError]:
        return Error(result) if result.is_error else catch(lambda: msgspec.convert(result.structured_content, type=_Completed | RemoteFailure), exception=msgspec.ValidationError)()

    def forwarded(response: _Completed | RemoteFailure) -> Result[int, InstallError]:
        sys.stdout.write(response.stdout)
        sys.stderr.write(response.stderr)
        return Error(response) if isinstance(response, RemoteFailure) else Ok(response.result.returncode)

    code = f"import os\nif os.getpid() not in {pids!r}:\n    raise RuntimeError('The connected Blender is not a selected running process')\n{_reconciled(archive, '--enable')}"
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
