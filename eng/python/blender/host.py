"""Reads the facts the installed Blender states once per build of it, runs the child processes around it, and syncs the user bases its interpreter loads."""

import sys
lazy from collections.abc import Iterable
lazy import hashlib
lazy from itertools import chain, takewhile
lazy from pathlib import Path
lazy import subprocess
lazy from typing import Annotated, Self

import msgspec
lazy from cyclopts import Parameter
lazy from cyclopts.types import ResolvedExistingFile
lazy from packaging.tags import mac_platforms
lazy from packaging.version import Version

lazy from blender.records.faults import Error, Fault, Ok, Outcome, Reason
lazy from blender.records.stores import committed, folder_made, RecordFile

# --- [TYPES] ----------------------------------------------------------------------------

type Blender = Annotated[ResolvedExistingFile, Parameter(env_var="BLENDER_PATH")]
type Deployment = Annotated[Version, Parameter(env_var="MACOSX_DEPLOYMENT_TARGET", accepts_keys=False)]

# --- [MODELS] ---------------------------------------------------------------------------


class Host(msgspec.Struct, frozen=True):
    """Facts one Blender build states: its interpreter and release, its user site layout and default repository, its bundled distributions, its factory tool text color, its own extension platform, and the extension platforms of each wheel platform the probe names."""

    executable: str
    version: str
    python: str
    user_site: str
    repository: str
    bundled: frozendict[str, str]
    text: tuple[float, float, float]
    platform: str
    platforms: frozendict[str, tuple[str, ...]]


class Upstream(msgspec.Struct, frozen=True):
    """One upstream commit: its base URL and each file under it with its sha256."""

    base: str
    files: frozendict[str, str]


class _Named(msgspec.Struct, frozen=True):
    name: str


class _BlenderTable(msgspec.Struct, frozen=True):
    fetch: frozendict[str, Upstream] = frozendict()
    turned: frozendict[str, str] = frozendict()
    theme: frozendict[str, object] = frozendict()


class _ToolTable(msgspec.Struct, frozen=True):
    blender: _BlenderTable = _BlenderTable()


class Pyproject(msgspec.Struct, frozen=True):
    """`pyproject.toml` tables of an extension: its uv member name, the upstream commits staged per folder, its turned icon variants, and its themes, which the theme program decodes inside Blender."""

    project: _Named
    tool: _ToolTable = _ToolTable()


class Manifest(msgspec.Struct, frozen=True):
    """`blender_manifest.toml` member selecting what a build stages."""

    type: str


class Target(msgspec.Struct, frozen=True):
    """Extension platform as the marker values selecting it from a lock and its wheel platform tags, best first: macOS from the deployment release, Linux from the glibc 2.28 ABI Blender builds its libraries against."""

    sys_platform: str
    platform_machine: str
    os_name: str
    platform_system: str
    tags: tuple[str, ...]

    @classmethod
    def every(cls, macos: Version, /) -> tuple[Self, ...]:
        """Returns each extension platform a build selects wheels for, macOS tags from the deployment release down."""
        return (
            cls("darwin", "arm64", "posix", "Darwin", tuple(mac_platforms((macos.major, macos.minor), "arm64"))),
            *(cls("linux", machine, "posix", "Linux", tuple(f"manylinux_2_{minor}_{machine}" for minor in range(28, 4, -1))) for machine in ("x86_64", "aarch64")),
            cls("win32", "AMD64", "nt", "Windows", ("win_amd64",)),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROCESSES]


def ran(*command: str | Path, stdout: int | None = None, stderr: int | None = None, env: dict[str, str] | None = None) -> Outcome[subprocess.CompletedProcess[bytes]]:
    """Runs a child process, a failed launch or nonzero exit as fault, whatever a failed child's captured streams held printed.

    Args:
        command: Executable and its arguments.
        stdout: Output stream target, inherited when None.
        stderr: Error stream target, inherited when None.
        env: Process environment, inherited when None.

    Returns:
        Finished process, or fault of a failed launch or of the nonzero exit's `CalledProcessError`.
    """
    try:
        return Ok(subprocess.run(command, stdout=stdout, stderr=stderr, env=env, check=True))
    except subprocess.CalledProcessError as error:
        sys.stdout.buffer.writelines(filter(None, (error.stdout,)))
        sys.stderr.buffer.writelines(filter(None, (error.stderr,)))
        return Error(Fault.of(Reason.FAILED, str(command[0]), error))
    except OSError as error:
        return Error(Fault.of(Reason.FAILED, str(command[0]), error))


def scripted(
    blender: Path, program: Path, /, *arguments: str | Path, stdout: int | None = None, stderr: int | None = None, env: dict[str, str] | None = None
) -> Outcome[subprocess.CompletedProcess[bytes]]:
    """Runs a Python program in a windowless factory Blender, its arguments after `--`, a stated environment reaching its interpreter.

    Args:
        blender: Blender executable.
        program: Python file the run executes.
        arguments: Arguments the program reads.
        stdout: Output stream target, inherited when None.
        stderr: Error stream target, inherited when None.
        env: Process environment, inherited and ignored by the interpreter when None.

    Returns:
        Finished process, or fault of a failed launch or of the nonzero exit a raised program or `sys.exit` message makes.
    """
    return ran(
        blender,
        *(() if env is None else ("--python-use-system-env",)),
        "-b",
        "--factory-startup",
        "--python-exit-code",
        "1",
        "--python",
        program,
        "--",
        *arguments,
        stdout=stdout,
        stderr=stderr,
        env=env,
    )


def root() -> Outcome[Path]:
    """Returns the folder `uv workspace dir` prints."""
    return ran("uv", "workspace", "dir", stdout=subprocess.PIPE).map(lambda found: Path(found.stdout.decode().strip()))


def headless(arguments: Iterable[str], /) -> bool:
    """Whether Blender arguments ahead of the first `--` select a run with no window."""
    return not frozenset({"-b", "--background", "-c", "--command"}).isdisjoint(takewhile(lambda argument: argument != "--", arguments))


def declared[T](path: Path, form: type[T], /) -> Outcome[T]:
    """Decodes a TOML file of an extension project through its record."""
    try:
        return Ok(msgspec.toml.decode(path.read_bytes(), type=form))
    except (OSError, msgspec.DecodeError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(path), error))


# --- [FACTS]


def probed(base: Path, blender: Path, /, *tags: str) -> Outcome[Host]:
    """Reads cached host facts or probes the installed Blender build.

    Args:
        base: Workspace root.
        blender: Blender executable.
        tags: Wheel platform tags the build maps to its extension platforms.

    Returns:
        Host facts, or fault of the run, its answer, or a cached answer's read.
    """
    program = base / "libs/python/blender/probe.py"
    try:
        built = blender.stat()
        stamp = hashlib.blake2b(msgspec.msgpack.encode((str(blender), built.st_mtime_ns, built.st_size, program.read_bytes(), tags)), digest_size=16).hexdigest()
    except OSError as error:
        return Error(Fault.of(Reason.UNREADABLE, str(blender), error))
    record = RecordFile(path=base / f".cache/blender/host/{stamp}.json", form=Host)

    def staged(path: Path) -> Outcome[Host]:
        return scripted(blender, program, path, *tags, stdout=subprocess.PIPE, stderr=subprocess.PIPE).bind(lambda _: RecordFile(path=path, form=Host).read())

    return record.found().bind(lambda found: found.map(Ok).default_with(lambda: committed(record.path, staged).bind(lambda _: record.read())))


# --- [ENVIRONMENTS]


def exported(base: Path, host: Host, lock: Path, /, *options: str) -> Outcome[subprocess.CompletedProcess[bytes]]:
    """Writes the locked `pylock.toml` of the workspace at base without Blender's bundled distributions."""
    return ran(
        "uv",
        "export",
        "--project",
        base,
        "--locked",
        "--no-default-groups",
        "--no-header",
        *chain.from_iterable(("--no-emit-package", name) for name in host.bundled),
        *options,
        "--output-file",
        lock,
        stdout=subprocess.DEVNULL,
    )


def site(base: Path, host: Host, /, *, package: str | None = None, groups: tuple[str, ...] = ()) -> Outcome[Path]:
    """Reconciles locked requirements into the user base of one selection on the installed Blender version, editable projects kept.

    Selections keep separate user bases, since a sync removes what its lock omits from a base a concurrent run reads.

    Args:
        base: Workspace root.
        host: Facts from the Blender build that loads the packages.
        package: Workspace member selecting locked runtime requirements, workspace root when omitted.
        groups: Additional uv dependency group selectors.

    Returns:
        The workspace's `.cache/blender/python/<version>/<package>+<group>...` user base, or fault of an export or package sync.
    """
    options = (*(("--package", package) if package is not None else ()), *chain.from_iterable(("--group", group) for group in groups))

    folder = base / ".cache/blender/python" / host.version / "+".join((package or base.name, *groups))
    lock = folder / "pylock.toml"
    return (
        folder_made(folder)
        .bind(lambda _: exported(base, host, lock, *options))
        .bind(lambda _: ran("uv", "pip", "sync", "--preview-features", "pylock", "--python", host.executable, "--target", folder / host.user_site, lock))
        .map(lambda _: folder)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Blender", "Deployment", "Host", "Manifest", "Pyproject", "Target", "Upstream", "declared", "exported", "headless", "probed", "ran", "root", "scripted", "site"]
