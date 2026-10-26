"""Installs and enables extension archive in matching Blender GUI session, else through extension CLI."""

from typing import override
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


class _Answer(msgspec.Struct, frozen=True, tag_field="status", tag="ok"):
    stdout: str = ""
    stderr: str = ""

    def forwarded(self) -> Result[int, Fault]:
        """Writes remote output, then returns zero."""
        sys.stdout.write(self.stdout)
        sys.stderr.write(self.stderr)
        return Ok(0)


class _Failed(_Answer, frozen=True, kw_only=True, tag="error"):
    message: str

    @override
    def forwarded(self) -> Result[int, Fault]:
        """Writes remote output, then returns fault of the remote traceback."""
        return super().forwarded().bind(lambda _: Error(Fault(message=self.message)))


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
    pids = await anyio.to_thread.run_sync(
        lambda: [
            process.pid
            for process in psutil.process_iter(("exe", "cmdline", "environ"))
            if None not in (info := process.info).values()
            and {"-b", "--background", "-c", "--command"}.isdisjoint(info["cmdline"])
            and {name: value for name, value in info["environ"].items() if name.startswith("BLENDER_USER_")} == profile
            and catch(blender.samefile, exception=OSError)(info["exe"]).default_value(value=False)
        ]
    )
    if not pids:
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
        return catch(msgspec.convert, exception=msgspec.ValidationError)(called.structured_content, type=_Answer | _Failed).map_error(Fault.of).bind(lambda answer: answer.forwarded())
    return Error(failed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install"]
