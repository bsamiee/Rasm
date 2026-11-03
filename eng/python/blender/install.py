"""Installs and enables extension archive in matching Blender GUI session, else through extension CLI."""

lazy from itertools import takewhile
lazy import os
lazy import sys
lazy import traceback
lazy from typing import Annotated

import msgspec
lazy import anyio
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingFile
lazy from expression import Error, Ok, Result
lazy from expression.extra.result import catch
lazy import httpx2
lazy from mcp import Client, MCPError
lazy from mcp.types import TextContent
lazy import psutil
lazy from pydantic import ValidationError

lazy from blender.records.faults import Fault, Outcome, Reason
lazy from eng.python.blender.assembly import BACKGROUND, Blender, ran

# --- [MODELS] ---------------------------------------------------------------------------


class _Answer(msgspec.Struct, frozen=True, kw_only=True, tag_field="status"):
    stdout: str = ""
    stderr: str = ""


class _Done(_Answer, frozen=True, kw_only=True, tag="ok"): ...


class _Failed(_Answer, frozen=True, kw_only=True, tag="error"):
    message: str


# --- [OPERATIONS] -----------------------------------------------------------------------


async def install(archive: ResolvedExistingFile, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Blender) -> Outcome[int]:
    """Installs archive `pack` built for host into user repository with extension enabled.

    GUI sessions with same profile load extension in place.

    Args:
        archive: Extension archive.
        endpoint: Blender Lab MCP endpoint.
        blender: Blender executable.

    Returns:
        Zero, or fault of a failed install, transport, decode, or remote execution.
    """
    repository = "user_default"
    pids = await anyio.to_thread.run_sync(
        lambda: [
            process.pid
            for process in psutil.process_iter(("exe", "cmdline", "environ"))
            if None not in (info := process.info).values()
            and catch(blender.samefile, exception=OSError)(info["exe"]).default_value(value=False)
            and BACKGROUND.isdisjoint(takewhile(lambda argument: argument != "--", info["cmdline"]))
            and not any(name.startswith("BLENDER_USER_") for name, _ in os.environ.items() ^ info["environ"].items())
        ]
    )
    if not pids:
        return (await anyio.to_thread.run_sync(ran, blender, "--command", "extension", "install-file", "--repo", repository, "--enable", archive)).map(lambda _: 0)
    code = (
        f"import os\nimport bpy\nif os.getpid() not in {pids!r}:\n    raise ProcessLookupError(os.getpid())\n"
        f"bpy.ops.extensions.package_install_files(filepath={str(archive)!r}, repo={repository!r})\n"
        "if bpy.context.preferences.is_dirty:\n    bpy.ops.wm.save_userpref()\n"
    )
    try:
        async with Client(endpoint) as client:
            called = await client.call_tool("execute_blender_code", {"code": code})
    except* (OSError, httpx2.HTTPError, MCPError, ValidationError) as errors:
        failed = Fault.of(Reason.FAILED, endpoint, "".join(traceback.format_exception_only(errors, show_group=True)).rstrip())
    else:
        if called.is_error:
            return Error(Fault.of(Reason.ANSWERED, endpoint, member="\n".join(block.text for block in called.content if isinstance(block, TextContent))))
        match catch(msgspec.convert, exception=msgspec.ValidationError)(called.structured_content, type=_Done | _Failed):
            case Result(tag="ok", ok=answer):
                sys.stdout.write(answer.stdout)
                sys.stderr.write(answer.stderr)
                return Error(Fault.of(Reason.ANSWERED, endpoint, member=answer.message)) if isinstance(answer, _Failed) else Ok(0)
            case Result(error=error):
                return Error(Fault.of(Reason.INVALID, endpoint, error))
    return Error(failed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install"]
