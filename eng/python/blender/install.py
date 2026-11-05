"""Installs a packed extension through a local static repository, in a matching Blender GUI session, else in a background run."""

lazy import ast
lazy from functools import partial
lazy from itertools import takewhile
lazy import os
lazy from pathlib import Path
lazy import sys
lazy from typing import Annotated

import msgspec
lazy import anyio
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingDirectory
lazy from expression import Result
lazy from expression.extra.result import catch
lazy import httpx2
lazy from mcp import Client, MCPError
lazy from mcp.types import TextContent
lazy import psutil
lazy from pydantic import ValidationError

lazy from blender.records.faults import Error, Fault, Ok, Outcome, Reason
lazy from eng.python.blender.assembly import BACKGROUND, Blender, indexed, ran

# --- [CONSTANTS] ------------------------------------------------------------------------

_REFRESH = """\
import os
import bpy
from bl_pkg.cli.blender_ext import platform_from_this_system


def refreshed(url, name, archives, pids):
    if pids and os.getpid() not in pids:
        raise ProcessLookupError(os.getpid())
    preferences = bpy.context.preferences
    extensions = preferences.extensions
    repository = next((held for held in extensions.repos if held.remote_url == url), None) or extensions.repos.new(name=name, module=name, remote_url=url)
    active, extensions.active_repo = extensions.active_repo, list(extensions.repos).index(repository)
    with bpy.context.temp_override(extension_repo=repository):
        bpy.ops.extensions.repo_sync_all(use_active_only=True)
    bpy.ops.extensions.package_install_files(filepath=archives[platform_from_this_system()], repo=repository.module)
    extensions.active_repo = active
    if preferences.is_dirty:
        bpy.ops.wm.save_userpref()
"""

# --- [MODELS] ---------------------------------------------------------------------------


class _Archive(msgspec.Struct, frozen=True):
    archive_url: str
    platforms: tuple[str, ...]


class _Listing(msgspec.Struct, frozen=True):
    data: tuple[_Archive, ...]


class _Answer(msgspec.Struct, frozen=True, kw_only=True, tag_field="status"):
    stdout: str = ""
    stderr: str = ""


class _Done(_Answer, frozen=True, kw_only=True, tag="ok"): ...


class _Failed(_Answer, frozen=True, kw_only=True, tag="error"):
    message: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def _refresh(directory: Path, blender: Path, pids: tuple[int, ...], /) -> Outcome[str]:
    """Writes the static repository listing and returns the refresh source over its `file` URL, each platform's archive, and the GUI process IDs it runs in."""

    def decoded(listing: Path) -> Outcome[str]:
        try:
            archives = {platform: str(directory / archive.archive_url) for archive in msgspec.json.decode(listing.read_bytes(), type=_Listing).data for platform in archive.platforms}
        except (OSError, msgspec.DecodeError) as error:
            return Error(Fault.of(Reason.UNREADABLE, str(listing), error))
        arguments = [
            ast.Constant(listing.as_uri()),
            ast.Constant(directory.name),
            ast.Dict([*map(ast.Constant, archives)], [*map(ast.Constant, archives.values())]),
            ast.Tuple([*map(ast.Constant, pids)]),
        ]
        return Ok("\n".join((_REFRESH, ast.unparse(ast.Call(ast.Name("refreshed"), arguments, [])))))

    return indexed(directory, blender).bind(decoded)


async def install(directory: ResolvedExistingDirectory, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Blender) -> Outcome[int]:
    """Lists the archives `pack` built as a static repository, registered once with no network, and installs the host platform's archive over any version that repository holds.

    Installs replace a build of equal version, since `extensions.package_upgrade_all` compares versions alone and an uncommitted change keeps the MinVer value.
    GUI sessions with same profile load the build in place.

    Args:
        directory: Static repository folder `pack` built, named by its extension.
        endpoint: Blender Lab MCP endpoint.
        blender: Blender executable.

    Returns:
        Zero, or fault of a failed listing, background run, transport, decode, or remote execution.
    """
    prefix = "BLENDER_USER_"
    profile = frozendict({name: value for name, value in os.environ.items() if name.startswith(prefix)})
    try:
        pids = await anyio.to_thread.run_sync(
            lambda: tuple(
                process.pid
                for process in psutil.process_iter(("exe", "cmdline", "environ"))
                if None not in (info := process.info).values()
                and catch(blender.samefile, exception=FileNotFoundError)(info["exe"]).default_value(value=False)
                and BACKGROUND.isdisjoint(takewhile(lambda argument: argument != "--", info["cmdline"]))
                and {name: value for name, value in info["environ"].items() if name.startswith(prefix)} == profile
            )
        )
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, str(blender), error))
    match await anyio.to_thread.run_sync(_refresh, directory, blender, pids):
        case Result(tag="ok", ok=code):
            pass
        case Result(error=faults):
            return Error(faults)
    if not pids:
        return (await anyio.to_thread.run_sync(lambda: ran(blender, "-b", "--python-exit-code", "1", "--python-expr", code))).map(lambda _: 0)
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
