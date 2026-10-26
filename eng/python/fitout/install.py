"""Installs and enables extension archive in matching Blender GUI session, else through extension CLI."""

lazy import os
lazy import subprocess
lazy import sys
lazy from typing import Annotated

import msgspec
lazy import anyio
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingDirectory, ResolvedExistingFile
lazy from expression import Error, Ok, Result
lazy from expression.extra.result import catch
lazy import httpx2
lazy from mcp import Client, MCPError
lazy from mcp.types import TextContent
lazy import psutil
lazy from pydantic import ValidationError

lazy from eng.python.fitout.faults import Fault

# --- [MODELS] ---------------------------------------------------------------------------


class _Answer(msgspec.Struct, frozen=True, tag_field="status"):
    stdout: str = ""
    stderr: str = ""

    def forwarded(self) -> Result[int, Fault]:
        """Writes remote output, then returns zero or fault of the remote traceback."""
        sys.stdout.write(self.stdout)
        sys.stderr.write(self.stderr)
        match self:
            case _Failed(message=message):
                return Error(Fault(message=message))
            case _:
                return Ok(0)


class _Done(_Answer, frozen=True, tag="ok"):
    """Remote run that finished."""


class _Failed(_Answer, frozen=True, kw_only=True, tag="error"):
    """Remote run that raised, `message` its traceback."""

    message: str


# --- [OPERATIONS] -----------------------------------------------------------------------


async def install(
    directory: ResolvedExistingDirectory, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]
) -> Result[int, Fault]:
    """Installs archive `pack` built for host into user repository with extension enabled.

    GUI sessions with same profile load extension in place.

    Args:
        directory: Folder with extension archive.
        endpoint: Blender Lab MCP endpoint.
        blender: Blender executable.

    Returns:
        Zero, or fault of a failed install, transport, decode, or remote execution.
    """
    (archive,) = directory.glob("*.zip")
    repository = "user_default"
    profile = frozendict({name: value for name, value in os.environ.items() if name.startswith("BLENDER_USER_")})
    if not (
        pids := await anyio.to_thread.run_sync(
            lambda: [
                process.pid
                for process in psutil.process_iter(("exe", "cmdline", "environ"))
                if (path := process.info["exe"])
                and (cmdline := process.info["cmdline"]) is not None
                and {"-b", "--background", "-c", "--command"}.isdisjoint(cmdline)
                and (environ := process.info["environ"]) is not None
                and {name: value for name, value in environ.items() if name.startswith("BLENDER_USER_")} == profile
                and catch(blender.samefile, exception=OSError)(path).default_value(value=False)
            ]
        )
    ):
        process = await anyio.run_process((blender, "--command", "extension", "install-file", "--repo", repository, "--enable", archive), stdout=None, stderr=None, check=False)
        return catch(process.check_returncode, exception=subprocess.CalledProcessError)().map(lambda _: 0).map_error(Fault.of)
    code = (
        f"import os\nimport bpy\nif os.getpid() not in {pids!r}:\n    raise ProcessLookupError(os.getpid())\n"
        f"bpy.ops.extensions.package_install_files(filepath={str(archive)!r}, repo={repository!r})\n"
        "if bpy.context.preferences.is_dirty:\n    bpy.ops.wm.save_userpref()\n"
    )
    try:
        async with Client(endpoint) as client:
            called = await client.call_tool("execute_blender_code", {"code": code})
    except* (OSError, httpx2.HTTPError, MCPError, ValidationError) as errors:
        failed = Fault.of(errors)
    else:
        if called.is_error:
            return Error(Fault(message="\n".join(block.text for block in called.content if isinstance(block, TextContent))))
        return catch(msgspec.convert, exception=msgspec.ValidationError)(called.structured_content, type=_Done | _Failed).map_error(Fault.of).bind(_Answer.forwarded)
    return Error(failed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install"]
