"""Installs and enables extension archive in matching Blender GUI session, else through extension CLI."""

lazy from functools import partial
lazy from itertools import takewhile
lazy import os
lazy import subprocess
lazy import sys
lazy from typing import Annotated

import msgspec
lazy import anyio
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingFile
lazy from expression import Result
lazy from expression.extra.result import catch
lazy import httpx2
lazy from mcp import Client, MCPError
lazy from mcp.types import TextContent
lazy import psutil
lazy from pydantic import ValidationError

lazy from blender.records.faults import Error, Fault, Ok, Outcome, Reason
lazy from eng.python.blender.assembly import BACKGROUND, Blender

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
    prefix = "BLENDER_USER_"
    profile = frozendict({name: value for name, value in os.environ.items() if name.startswith(prefix)})
    try:
        pids = await anyio.to_thread.run_sync(
            lambda: [
                process.pid
                for process in psutil.process_iter(("exe", "cmdline", "environ"))
                if None not in (info := process.info).values()
                and catch(blender.samefile, exception=FileNotFoundError)(info["exe"]).default_value(value=False)
                and BACKGROUND.isdisjoint(takewhile(lambda argument: argument != "--", info["cmdline"]))
                and {name: value for name, value in info["environ"].items() if name.startswith(prefix)} == profile
            ]
        )
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, str(blender), error))
    if not pids:
        try:
            done = await anyio.run_process((blender, "--command", "extension", "install-file", "--repo", repository, "--enable", archive), stderr=None)
        except (OSError, subprocess.CalledProcessError) as error:
            return Error(Fault.of(Reason.FAILED, str(blender), error))
        sys.stdout.buffer.write(done.stdout)
        faults = tuple(*Fault.of(Reason.ANSWERED, str(archive), member=line.decode()) for line in done.stdout.splitlines() if line.startswith((b"ERROR ", b"FATAL_ERROR ")))
        match faults:
            case (head, *rest):
                return Error((head, *rest))
            case _:
                return Ok(0)
    code = (
        f"import os\nimport bpy\nif os.getpid() not in {pids!r}:\n    raise ProcessLookupError(os.getpid())\n"
        f"bpy.ops.extensions.package_install_files(filepath={str(archive)!r}, repo={repository!r})\n"
        "if bpy.context.preferences.is_dirty:\n    bpy.ops.wm.save_userpref()\n"
    )
    try:
        async with Client(endpoint) as client:
            called = await client.call_tool("execute_blender_code", {"code": code})
    except* (OSError, httpx2.HTTPError, MCPError, ValidationError) as errors:
        failed = Fault.of(Reason.FAILED, endpoint, errors)
    else:
        if called.is_error:
            return Error(Fault.of(Reason.ANSWERED, endpoint, member="\n".join(block.text for block in called.content if isinstance(block, TextContent))))
        match catch(lambda: msgspec.convert(called.structured_content, type=_Done | _Failed), exception=msgspec.ValidationError)().map_error(partial(Fault.of, Reason.INVALID, endpoint)):
            case Result(tag="ok", ok=answer):
                sys.stdout.write(answer.stdout)
                sys.stderr.write(answer.stderr)
                return Error(Fault.of(Reason.ANSWERED, endpoint, member=answer.message)) if isinstance(answer, _Failed) else Ok(0)
            case Result(error=failed):
                return Error(failed)
    return Error(failed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install"]
