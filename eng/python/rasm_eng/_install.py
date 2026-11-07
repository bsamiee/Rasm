"""Installs a packed extension through Blender's own installer, in a matching Blender GUI session, else in a background run."""

import os
import sys
lazy import ast
lazy from collections.abc import Mapping
lazy from pathlib import Path
lazy import subprocess
lazy from typing import Annotated
lazy from zipfile import BadZipFile, ZipFile

import msgspec
lazy import anyio
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingDirectory
lazy from expression.extra.result import catch
lazy import httpx2
lazy from mcp import Client, MCPError
lazy from mcp.types import TextContent
lazy import psutil
lazy from pydantic import ValidationError

lazy from rasm_eng._host import _headless, _Host, _probed, _ran, _root, Blender
lazy from rasm_records import Error, Fault, Ok, Outcome, paired, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class _Answer(msgspec.Struct, frozen=True, kw_only=True, tag_field="status"):
    stdout: str = ""
    stderr: str = ""


class _Done(_Answer, frozen=True, kw_only=True, tag="ok"): ...


class _Failed(_Answer, frozen=True, kw_only=True, tag="error"):
    message: str


class _Generated(msgspec.Struct, frozen=True):
    platforms: tuple[str, ...] = ()


class _Build(msgspec.Struct, frozen=True):
    generated: _Generated = _Generated()


class _ArchiveManifest(msgspec.Struct, frozen=True):
    """The platform restriction Blender gives a generated archive under `[build.generated]`, else its source manifest's own."""

    platforms: tuple[str, ...] = ()
    build: _Build = _Build()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _archived(directory: Path, platform: str, /) -> Outcome[Path]:
    """Selects the one archive whose Blender manifest admits the host platform."""
    matches: list[Path] = []
    try:
        for path in directory.glob("*.zip"):
            with ZipFile(path) as packed:
                manifest = msgspec.toml.decode(packed.read("blender_manifest.toml"), type=_ArchiveManifest)
            if not (admitted := manifest.build.generated.platforms or manifest.platforms) or platform in admitted:
                matches.append(path)
    except (OSError, BadZipFile, KeyError, UnicodeError, msgspec.DecodeError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(directory), error))
    match matches:
        case [archive]:
            return Ok(archive)
        case []:
            return Error(Fault.of(Reason.ABSENT, str(directory), member=f"{platform} extension archive"))
        case _:
            return Error(Fault.of(Reason.INVALID, str(directory), f"Multiple extension archives admit {platform}: {', '.join(path.name for path in sorted(matches))}"))


def _sessions(blender: Path, /) -> tuple[int, ...]:
    """Returns the windowed Blender processes running this executable on this process's `BLENDER_USER_*` profile, each read beyond its executable only once that matches."""

    def profile(environment: Mapping[str, str]) -> dict[str, str]:
        return {name: value for name, value in environment.items() if name.startswith("BLENDER_USER_")}

    def matches(process: psutil.Process) -> bool:
        try:
            return blender.samefile(process.info["exe"]) and not _headless(process.cmdline()) and profile(process.environ()) == profile(os.environ)
        except OSError, psutil.Error:
            return False

    return tuple(process.pid for process in psutil.process_iter(("exe",)) if process.info["exe"] and matches(process))


def _background(blender: Path, repository: str, archive: Path, /) -> Outcome[int]:
    """Runs `extension install-file`, whose exit status passes a refusal its `ERROR` line alone states."""

    def answered(done: subprocess.CompletedProcess[bytes]) -> Outcome[int]:
        sys.stdout.buffer.write(done.stdout)
        refused = tuple(line.decode() for line in done.stdout.splitlines() if line.startswith(b"ERROR"))
        return Error(Fault.of(Reason.ANSWERED, str(blender), member="\n".join(refused))) if refused else Ok(0)

    return _ran(blender, "--command", "extension", "install-file", "--repo", repository, "--enable", archive, stdout=subprocess.PIPE).bind(answered)


async def _in_session(endpoint: str, code: str, /) -> Outcome[int]:
    """Runs code in the session behind the MCP endpoint, its printed streams forwarded."""
    try:
        async with Client(endpoint) as client:
            called = await client.call_tool("execute_blender_code", {"code": code})
    except* (OSError, httpx2.HTTPError, MCPError, ValidationError) as errors:
        failed = Fault.of(Reason.FAILED, endpoint, errors)
    else:
        if called.is_error:
            return Error(Fault.of(Reason.ANSWERED, endpoint, member="\n".join(block.text for block in called.content if isinstance(block, TextContent))))
        try:
            answer = msgspec.convert(called.structured_content, type=_Done | _Failed)
        except msgspec.ValidationError as error:
            return Error(Fault.of(Reason.INVALID, endpoint, error))
        sys.stdout.write(answer.stdout)
        sys.stderr.write(answer.stderr)
        return Error(Fault.of(Reason.ANSWERED, endpoint, member=answer.message)) if isinstance(answer, _Failed) else Ok(0)
    return Error(failed)


def install(directory: ResolvedExistingDirectory, /, *, endpoint: Annotated[str, Parameter(env_var="BLENDER_MCP_URL")], blender: Blender) -> Outcome[Path]:
    """Installs the host platform's archive `pack` built into the host's local repository, over any version it holds.

    GUI sessions with same profile load the build in place.

    Args:
        directory: Folder `pack` built, named by its extension.
        endpoint: Blender Lab MCP endpoint.
        blender: Blender executable.

    Returns:
        Installed archive, or fault of a failed probe, background run, transport, decode, or remote execution.
    """

    def installed(host: _Host, archive: Path, sessions: tuple[int, ...]) -> Outcome[Path]:
        call = ast.Expr(ast.Call(ast.Name("_installed"), [ast.Constant(str(archive)), ast.Constant(host.repository), ast.Tuple([ast.Constant(session) for session in sessions])], []))
        return (anyio.run(_in_session, endpoint, f"{Path(__file__).with_name('_session.py').read_text()}\n{ast.unparse(call)}\n") if sessions else _background(blender, host.repository, archive)).map(
            lambda _: archive
        )

    return (
        _root()
        .bind(lambda base: _probed(base, blender))
        .bind(
            lambda host: paired(_archived(directory, host.platform), catch(_sessions, exception=OSError)(blender).map_error(lambda error: Fault.of(Reason.FAILED, str(blender), error))).bind(
                lambda held: installed(host, *held)
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["install"]
