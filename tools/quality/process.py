"""Subprocess and dotnet execution primitives shared by all quality rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager
from dataclasses import dataclass
from functools import reduce
from importlib import import_module
from pathlib import Path
import sys
from types import ModuleType
from typing import assert_never, Final, Literal, Protocol
import xml.etree.ElementTree as ET  # noqa: S405

import anyio
from anyio.abc import ByteReceiveStream
from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.settings import ArtifactScope, PROJECT_EXCLUDE_DIRS, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type DotnetOp = Literal["restore", "build"]
type FdExtension = Literal["csproj", "csx"]
type ProcessMode = Literal["capture", "stream"]
type ProjectIndex = Mapping[Path, Path]


class ByteWriter(Protocol):
    def write(self, data: bytes) -> object: ...

    def flush(self) -> object: ...


# --- [CONSTANTS] -----------------------------------------------------------------------

_GIT_CHANGE_ARGS: Final[tuple[tuple[str, ...], ...]] = (
    ("diff", "--name-only", "--diff-filter=ACDMRTUXB"),
    ("diff", "--cached", "--name-only", "--diff-filter=ACDMRTUXB"),
    ("ls-files", "--others", "--exclude-standard"),
)
# Build-graph verbs only: scoped invocations splice `--artifacts-path`/`--disable-build-servers`; tool-driver verbs reject them.
_ARTIFACT_SCOPED_VERBS: Final[frozenset[str]] = frozenset(("build", "clean", "msbuild", "pack", "publish", "restore", "run", "test"))
_PIPE: Final[int] = -1
_POSIX_MODULE: Final = import_module("fcntl")


# --- [MODELS] ----------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Completed:
    argv: tuple[str, ...]
    returncode: int
    stdout: bytes
    stderr: bytes

    @property
    def text(self) -> str:
        return self.stdout.decode(errors="replace")

    def lines(self, *, suffix: str = "") -> tuple[str, ...]:
        return tuple(line for line in self.text.splitlines() if not suffix or line.endswith(suffix))


@dataclass(frozen=True, slots=True)
class ProcessFault:
    argv: tuple[str, ...]
    returncode: int
    stderr: bytes

    @staticmethod
    def fail(*argv: str, detail: str | bytes, returncode: int = 2) -> ProcessFault:
        match detail:
            case bytes() as stderr:
                pass
            case text:
                stderr = text.encode()
        return ProcessFault(argv=argv, returncode=returncode, stderr=stderr)

    @property
    def message(self) -> str:
        detail = self.stderr.decode(errors="replace").strip()
        return f"exit {self.returncode}: {' '.join(self.argv)}" + (f"\n{detail}" if detail else "")


@dataclass(frozen=True, slots=True)
class ResourceBusyError(Exception):
    lock: Path
    owner: str


@dataclass(frozen=True, slots=True)
class _PosixLock:
    module: ModuleType
    exclusive: int
    nonblock: int
    unlock: int

    def flock(self, fd: int, operation: int, /) -> None:
        match getattr(self.module, "flock", None):
            case action if callable(action):
                action(fd, operation)
            case _:
                raise RuntimeError("fcntl.flock is unavailable")


@dataclass(frozen=True, slots=True)
class Workspace:
    root: Path

    def paths(self, args: tuple[str, ...], *, cwd: Path | None = None, suffix: str = "") -> tuple[Path, ...]:
        return (
            run(args, cwd=cwd or self.root, check=True)
            .map(lambda done: tuple(Path(line) for line in done.lines(suffix=suffix)))
            .default_with(lambda _: ())
        )

    def projects(self, directory: str = ".") -> tuple[str, ...]:
        return tuple(sorted(str(path) for path in self.paths(fd_args("csproj", ".", directory), suffix=".csproj")))

    def changed(self) -> tuple[str, ...]:
        return tuple(
            sorted({
                line
                for args in _GIT_CHANGE_ARGS
                for command in (("git", "-C", str(self.root), *args),)
                for line in run(command, check=True).map(lambda done: done.lines()).default_with(lambda _: ())
            })
        )

    def index(self) -> ProjectIndex:
        return {(self.root / rel).parent: self.root / rel for rel in self.projects()}

    def owner(self, index: ProjectIndex, file: Path) -> Result[Path, ProcessFault]:
        ranked = tuple(
            index[directory] for directory in (file.parent, *file.parent.parents) if directory.is_relative_to(self.root) and directory in index
        )
        match ranked:
            case (project,):
                return Ok(project)
            case ():
                return Error(ProcessFault.fail("resolve", str(file), detail=b"No owning project found"))
            case _:
                return Error(ProcessFault.fail("resolve", str(file), detail=f"Expected one owning project for {file}, found {len(ranked)}"))

    def owner_rel(self, index: ProjectIndex, file: Path) -> str | None:
        return self.owner(index, file).map(lambda project: str(project.relative_to(self.root))).default_value(None)

    @staticmethod
    def csproj(project: Path, tag: str = "") -> ET.Element | str | None:
        match project:
            case Path() as target if target.is_file():
                pass
            case _:
                return None
        # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=xml-etree-csproj ticket=QUALITY-R5 expires=2026-12-31 rationale=stdlib-xml-boundary
        try:
            node = ET.parse(target).getroot()  # noqa: S314
        except ET.ParseError:
            return None
        return (
            node
            if not tag
            else next((value.strip() for child in node.iter() if child.tag.rpartition("}")[-1] == tag for value in (child.text,) if value), None)
        )


# --- [OPERATIONS] ------------------------------------------------------------------------


def decode_json[T](payload: bytes | str, model: type[T]) -> Result[T, ProcessFault]:
    match payload:
        case bytes() as raw:
            pass
        case str() as text:
            raw = text.encode()
    # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=msgspec-decode ticket=QUALITY-R4 expires=2026-12-31 rationale=library-decode-boundary
    try:
        return Ok(msgspec.json.decode(raw, type=model))
    except msgspec.DecodeError as exc:
        return Error(ProcessFault.fail("decode", model.__name__, detail=str(exc)))


@contextmanager
def exclusive_lease(lock: Path, owner: str) -> Generator[None]:
    posix = _posix_lock()
    lock.parent.mkdir(parents=True, exist_ok=True)
    with lock.open(mode="a+", encoding="utf-8") as guard:
        try:
            posix.flock(guard.fileno(), posix.exclusive | posix.nonblock)
        except BlockingIOError as exc:
            guard.seek(0)
            raise ResourceBusyError(lock, guard.read().strip()) from exc
        guard.seek(0)
        guard.truncate()
        guard.write(owner)
        guard.flush()
        try:
            yield
        finally:
            posix.flock(guard.fileno(), posix.unlock)
            lock.unlink(missing_ok=True)


def dotnet(
    *args: str,
    scope: ArtifactScope | None = None,
    cwd: Path | None = None,
    check: bool = True,
    timeout: float | None = None,
    scoped: bool = True,
    mode: ProcessMode = "capture",
) -> Result[Completed, ProcessFault]:
    # scoped=True splices artifact flags for build-graph verbs; scoped=False keeps default bin/ for scenario-kit and yak staging.
    match scope:
        case ArtifactScope() as active if scoped and args and args[0] in _ARTIFACT_SCOPED_VERBS:
            separator = args.index("--") if "--" in args else len(args)
            command = ("dotnet", *args[:separator], *active.dotnet_flags, *args[separator:])
            return run(command, cwd=cwd, env=active.dotnet_env, check=check, timeout=timeout, mode=mode)
        case ArtifactScope() as active:
            return run(("dotnet", *args), cwd=cwd, env=active.dotnet_env, check=check, timeout=timeout, mode=mode)
        case _:
            return run(("dotnet", *args), cwd=cwd, env=None, check=check, timeout=timeout, mode=mode)


def dotnet_args(
    op: DotnetOp,
    target: str | Path,
    *,
    configuration: str = "",
    version: tuple[str, ...] = (),
    disable_parallel: bool = False,
    max_cpu: int | None = None,
    serial: bool = False,
    fresh: bool = False,
    quiet: bool = False,
    disable_build_servers: bool = False,
) -> tuple[str, ...]:
    match op:
        case "restore":
            return ("restore", str(target), "--locked-mode", *(("--disable-parallel",) if disable_parallel else ()))
        case "build":
            cpu = 1 if serial else max_cpu
            extra = ("-p:BuildInParallel=false",) if serial else ()
            parallel = (f"-maxcpucount:{cpu}", *extra) if cpu is not None else ()
            freshness = ("--no-incremental",) if fresh else ()
            logger = ("-v:quiet", "/clp:ErrorsOnly") if quiet else ()
            servers = ("--disable-build-servers",) if disable_build_servers else ()
            return ("build", str(target), "--configuration", configuration, "--no-restore", *freshness, *logger, *servers, *version, *parallel)
        case unreachable:
            assert_never(unreachable)


@beartype
def dotnet_build(
    settings: QualitySettings,
    scope: ArtifactScope,
    *,
    restore: str | Path | None = None,
    restore_targets: tuple[str | Path, ...] = (),
    targets: tuple[str | Path, ...],
    configurations: tuple[str, ...] = (),
    version: str = "",
    disable_parallel: bool = False,
    serial: bool = False,
    max_cpu: int | None = None,
    scoped: bool = True,
    mode: ProcessMode = "stream",
    fresh: bool = False,
    quiet: bool = False,
    disable_build_servers: bool = False,
) -> Result[None, ProcessFault]:
    configs = configurations or (settings.configuration,)
    version_args = settings.version_props(version)
    restores = restore_targets or ((restore,) if restore is not None else ())
    commands = (
        *(dotnet_args("restore", item, disable_parallel=disable_parallel) for item in restores),
        *(
            dotnet_args(
                "build",
                target,
                configuration=configuration,
                version=version_args,
                serial=serial,
                max_cpu=max_cpu,
                fresh=fresh,
                quiet=quiet,
                disable_build_servers=disable_build_servers,
            )
            for configuration in configs
            for target in targets
        ),
    )
    return fold(commands, None, lambda _, command: dotnet(*command, scope=scope, scoped=scoped, check=True, mode=mode).map(lambda _: None))


def _posix_flag(module: ModuleType, name: str) -> int:
    match getattr(module, name, None):
        case int() as value:
            return value
        case _:
            raise RuntimeError(f"fcntl.{name} is unavailable")


def _posix_lock() -> _PosixLock:
    return _PosixLock(
        module=_POSIX_MODULE,
        exclusive=_posix_flag(_POSIX_MODULE, "LOCK_EX"),
        nonblock=_posix_flag(_POSIX_MODULE, "LOCK_NB"),
        unlock=_posix_flag(_POSIX_MODULE, "LOCK_UN"),
    )


def fd_args(extension: FdExtension, pattern: str = ".", *roots: str | Path, exclude: bool = True) -> tuple[str, ...]:
    paths = tuple(str(root) for root in roots) or (".",)
    excludes = tuple(item for name in PROJECT_EXCLUDE_DIRS for item in ("--exclude", name)) if exclude else ()
    return ("fd", "-H", "-e", extension, pattern, *paths, *excludes)


def fold[T, U](items: tuple[T, ...], init: U, step: Callable[[U, T], Result[U, ProcessFault]]) -> Result[U, ProcessFault]:
    return reduce(lambda acc, item: acc.bind(lambda value: step(value, item)), items, Ok(init))


def run(
    argv: tuple[str, ...],
    *,
    cwd: Path | None = None,
    env: dict[str, str] | None = None,
    check: bool = False,
    timeout: float | None = None,
    mode: ProcessMode = "capture",
) -> Result[Completed, ProcessFault]:
    def _outcome(completed: Completed) -> Result[Completed, ProcessFault]:
        detail = b"\n".join(filter(None, (completed.stderr, completed.stdout)))
        return (
            Error(ProcessFault.fail(*argv, detail=detail, returncode=completed.returncode)) if completed.returncode != 0 and check else Ok(completed)
        )

    async def _exec() -> Result[Completed, ProcessFault]:
        match mode:
            case "capture":
                completed = await anyio.run_process(list(argv), cwd=str(cwd) if cwd else None, env=env, check=False)
                return _outcome(Completed(argv=argv, returncode=completed.returncode, stdout=completed.stdout, stderr=completed.stderr))
            case "stream":
                streamed = await _stream(argv, cwd=cwd, env=env)
                return streamed.bind(_outcome)

    async def _guarded() -> Result[Completed, ProcessFault]:
        match timeout:
            case None:
                return await _exec()
            case _ as limit:
                # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=anyio-deadline
                try:
                    with anyio.fail_after(limit):
                        return await _exec()
                except TimeoutError:
                    detail = f"timed out after {limit:g}s".encode()
                    return Error(ProcessFault.fail(*argv, detail=detail, returncode=124))

    return anyio.run(_guarded)


async def _stream(argv: tuple[str, ...], *, cwd: Path | None, env: dict[str, str] | None) -> Result[Completed, ProcessFault]:
    stdout: list[bytes] = []
    stderr: list[bytes] = []
    process = await anyio.open_process(list(argv), cwd=str(cwd) if cwd else None, env=env, stdout=_PIPE, stderr=_PIPE)

    async def collect(stream: ByteReceiveStream | None, sink: list[bytes], target: ByteWriter) -> None:
        match stream:
            case None:
                return
            case _:
                async for chunk in stream:
                    sink.append(chunk)
                    target.write(chunk)
                    target.flush()

    try:
        async with anyio.create_task_group() as group:
            group.start_soon(collect, process.stdout, stdout, sys.stdout.buffer)
            group.start_soon(collect, process.stderr, stderr, sys.stderr.buffer)
            returncode = await process.wait()
    except BaseException:
        process.kill()
        with anyio.CancelScope(shield=True):
            await process.wait()
        raise
    return Ok(Completed(argv=argv, returncode=returncode, stdout=b"".join(stdout), stderr=b"".join(stderr)))
