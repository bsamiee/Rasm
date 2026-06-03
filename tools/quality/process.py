"""Subprocess and dotnet execution primitives shared by all quality rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections import deque
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager
from dataclasses import dataclass
from datetime import datetime, UTC
from functools import cache, reduce
from importlib import import_module
import os
from pathlib import Path
import shutil
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
type RailStatus = Literal["ok", "empty", "skip", "busy", "timeout", "unsupported", "failed"]


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
_STREAM_TAIL_BYTES: Final[int] = 128 * 1024
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
    status: RailStatus = "failed"

    @staticmethod
    def fail(*argv: str, detail: str | bytes, returncode: int = 2, status: RailStatus | None = None) -> ProcessFault:
        match detail:
            case bytes() as stderr:
                pass
            case text:
                stderr = text.encode()
        return ProcessFault(argv=argv, returncode=returncode, stderr=stderr, status=status or _fault_status(returncode))

    @property
    def message(self) -> str:
        detail = self.stderr.decode(errors="replace").strip()
        return f"exit {self.returncode}: {' '.join(self.argv)}" + (f"\n{detail}" if detail else "")


@dataclass(frozen=True, slots=True)
class ResourceBusyError(Exception):
    lock: Path
    owner: str


@dataclass(frozen=True, slots=True)
class DotnetInvocation:
    args: tuple[str, ...]
    scoped: bool = True
    check: bool = True
    timeout: float | None = None
    mode: ProcessMode = "capture"
    cwd: Path | None = None

    def argv(self, scope: ArtifactScope | None = None) -> tuple[str, ...]:
        match scope:
            case ArtifactScope() as active if self.scoped and self.args and self.args[0] in _ARTIFACT_SCOPED_VERBS:
                separator = self.args.index("--") if "--" in self.args else len(self.args)
                return ("dotnet", *self.args[:separator], *active.dotnet_flags, *self.args[separator:])
            case _:
                return ("dotnet", *self.args)

    @staticmethod
    def env(scope: ArtifactScope | None = None) -> dict[str, str] | None:
        return scope.dotnet_env if isinstance(scope, ArtifactScope) else None

    def artifact_dir(self, scope: ArtifactScope | None = None) -> Path | None:
        return scope.path / "process" if isinstance(scope, ArtifactScope) and self.mode == "stream" else None

    def run(self, scope: ArtifactScope | None = None) -> Result[Completed, ProcessFault]:
        return run(
            self.argv(scope),
            cwd=self.cwd,
            env=self.env(scope),
            check=self.check,
            timeout=self.timeout,
            mode=self.mode,
            artifact_dir=self.artifact_dir(scope),
        )


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

    def paths(self, args: tuple[str, ...], *, cwd: Path | None = None, suffix: str = "") -> Result[tuple[Path, ...], ProcessFault]:
        return run(args, cwd=cwd or self.root, check=True).map(lambda done: tuple(Path(line) for line in done.lines(suffix=suffix)))

    def projects(self, directory: str = ".") -> Result[tuple[str, ...], ProcessFault]:
        return self.paths(fd_args("csproj", ".", directory), suffix=".csproj").map(lambda rows: tuple(sorted(str(path) for path in rows)))

    def changed(self) -> Result[tuple[str, ...], ProcessFault]:
        seed: tuple[str, ...] = ()
        return fold(
            tuple(("git", "-C", str(self.root), *args) for args in _GIT_CHANGE_ARGS),
            seed,
            lambda acc, command: run(command, check=True).map(lambda done: (*acc, *done.lines())),
        ).map(lambda rows: tuple(sorted(frozenset(rows))))

    def index(self) -> Result[ProjectIndex, ProcessFault]:
        return self.projects().map(lambda rows: {(self.root / rel).parent: self.root / rel for rel in rows})

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
        match xml_root(project) if project.is_file() else None:
            case ET.Element() as node:
                return node if not tag else next((value.strip() for child in xml_iter(node, tag) for value in (child.text,) if value), None)
            case None:
                return None


# --- [OPERATIONS] ------------------------------------------------------------------------


def decode_json[T](payload: bytes | str, model: type[T]) -> Result[T, ProcessFault]:
    match payload:
        case bytes() as raw:
            pass
        case str() as text:
            raw = text.encode()
    try:
        return Ok(msgspec.json.decode(raw, type=model))
    except msgspec.DecodeError as exc:
        return Error(ProcessFault.fail("decode", model.__name__, detail=str(exc)))


def xml_root(path: Path) -> ET.Element | None:
    try:
        return ET.parse(path).getroot()  # noqa: S314
    except ET.ParseError, OSError:
        return None


def xml_iter(root: ET.Element, tag: str) -> Iterable[ET.Element]:
    return (node for node in root.iter() if node.tag.rpartition("}")[-1] == tag)


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
            guard.seek(0)
            guard.truncate()
            guard.flush()
            posix.flock(guard.fileno(), posix.unlock)


def lease_owner(settings: QualitySettings, resource: str, *, project: str = "", mode: str = "exclusive") -> str:
    return "".join((
        f"resource={resource}\n",
        f"run_id={settings.run_id}\n",
        f"pid={os.getpid()}\n",
        f"cwd={settings.root}\n",
        f"mode={mode}\n",
        *((f"project={project}\n",) if project else ()),
        f"started_at={datetime.now(tz=UTC).isoformat()}\n",
    ))


def leased[T](
    settings: QualitySettings, lock: Path, resource: str, action: Callable[[], Result[T, ProcessFault]], *, project: str = "", mode: str = "exclusive"
) -> Result[T, ProcessFault]:
    # One non-blocking-lease boundary for every rail: busy -> typed exit-5 fault, lock/runtime errors -> typed fault, never a hang or raw raise.
    try:
        with exclusive_lease(lock, lease_owner(settings, resource, project=project, mode=mode)):
            return action()
    except ResourceBusyError as exc:
        return Error(ProcessFault.fail(resource, "busy", detail=f"{resource} is busy: {exc.lock} held by\n{exc.owner}", returncode=5, status="busy"))
    except (OSError, RuntimeError) as exc:
        return Error(ProcessFault.fail(resource, "lock", detail=str(exc)))


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
    return DotnetInvocation(args=args, scoped=scoped, check=check, timeout=timeout, mode=mode, cwd=cwd).run(scope)


def dotnet_args(
    op: DotnetOp,
    target: str | Path,
    *,
    configuration: str = "",
    version: tuple[str, ...] = (),
    disable_parallel: bool = False,
    max_cpu: int | None = None,
    serial: bool = False,
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
            logger = ("-v:quiet", "/clp:ErrorsOnly") if quiet else ()
            servers = ("--disable-build-servers",) if disable_build_servers else ()
            return ("build", str(target), "--configuration", configuration, "--no-restore", *logger, *servers, *version, *parallel)
        case unreachable:
            assert_never(unreachable)


def dotnet_build_invocations(
    settings: QualitySettings,
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
    quiet: bool = False,
    disable_build_servers: bool = False,
) -> tuple[DotnetInvocation, ...]:
    configs = configurations or (settings.configuration,)
    version_args = settings.version_props(version)
    restores = restore_targets or ((restore,) if restore is not None else ())
    return (
        *(DotnetInvocation(dotnet_args("restore", item, disable_parallel=disable_parallel), scoped=scoped, mode=mode) for item in restores),
        *(
            DotnetInvocation(
                dotnet_args(
                    "build",
                    target,
                    configuration=configuration,
                    version=version_args,
                    serial=serial,
                    max_cpu=max_cpu,
                    quiet=quiet,
                    disable_build_servers=disable_build_servers,
                ),
                scoped=scoped,
                mode=mode,
            )
            for configuration in configs
            for target in targets
        ),
    )


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
    quiet: bool = False,
    disable_build_servers: bool = False,
) -> Result[None, ProcessFault]:
    invocations = dotnet_build_invocations(
        settings,
        restore=restore,
        restore_targets=restore_targets,
        targets=targets,
        configurations=configurations,
        version=version,
        disable_parallel=disable_parallel,
        serial=serial,
        max_cpu=max_cpu,
        scoped=scoped,
        mode=mode,
        quiet=quiet,
        disable_build_servers=disable_build_servers,
    )
    return fold(invocations, None, lambda _, invocation: invocation.run(scope).map(lambda _: None))


@cache
def _dotnet_root() -> str | None:
    # Resolve the valid runtime root once per process (the `dotnet --list-runtimes` probe is process-stable); tests reset via cache_clear.
    def valid(path: str) -> str | None:
        root = Path(path).expanduser()
        return str(root) if (root / "shared" / "Microsoft.NETCore.App").is_dir() else None

    def runtime_root(line: str) -> str | None:
        match line.rsplit("[", maxsplit=1):
            case [_, raw] if raw.endswith("]"):
                return valid(str(Path(raw[:-1]).parent.parent))
            case _:
                return None

    muxer = shutil.which("dotnet")
    listed = run(("dotnet", "--list-runtimes"), check=False).map(lambda done: done.text.splitlines()).default_with(lambda _: ())
    candidates = (
        *(root for root in (runtime_root(line) for line in listed) if root is not None),
        valid(os.environ.get("DOTNET_ROOT", "")),  # noqa: TID251
        *(valid(str(Path(muxer).resolve().parent)) for _ in (muxer,) if muxer),
    )
    return next((root for root in candidates if root), None)


def dotnet_apphost_env(env: dict[str, str] | None = None) -> dict[str, str]:
    # nix sets DOTNET_ROOT to a wrapped path lacking shared/Microsoft.NETCore.App; overlay the resolved runtime root so
    # apphost-deployed tools (ilspycmd, rhinocode) resolve a runtime. dotnet SDK verbs self-locate via the muxer and skip this.
    base = dict(os.environ if env is None else env)  # noqa: TID251
    match _dotnet_root():
        case str(root):
            return {**base, "DOTNET_ROOT": root, "DOTNET_MULTILEVEL_LOOKUP": "0"}
        case None:
            return {key: value for key, value in base.items() if key != "DOTNET_ROOT"}


def dotnet_tool(scope: ArtifactScope, name: str, *args: str, check: bool = False, timeout: float | None = None) -> Result[Completed, ProcessFault]:
    # `dotnet tool run` resolves the manifest at the worktree root and the runtime via the muxer (the nix-resilient path);
    # the apphost overlay is belt-and-suspenders for the spawned tool process.
    return run(("dotnet", "tool", "run", name, "--", *args), cwd=scope.root, env=dotnet_apphost_env(scope.dotnet_env), check=check, timeout=timeout)


def dotnet_tool_restore(scope: ArtifactScope) -> Result[None, ProcessFault]:
    return run(("dotnet", "tool", "restore"), cwd=scope.root, env=dotnet_apphost_env(scope.dotnet_env), check=True).map(lambda _: None)


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
    artifact_dir: Path | None = None,
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
                streamed = await _stream(argv, cwd=cwd, env=env, artifact_dir=artifact_dir)
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
                    return Error(ProcessFault.fail(*argv, detail=detail, returncode=124, status="timeout"))

    return anyio.run(_guarded)


async def _stream(
    argv: tuple[str, ...], *, cwd: Path | None, env: dict[str, str] | None, artifact_dir: Path | None
) -> Result[Completed, ProcessFault]:
    stdout_tail: deque[bytes] = deque()
    stderr_tail: deque[bytes] = deque()
    process_dir = _process_dir(argv, artifact_dir)
    process_dir.mkdir(parents=True, exist_ok=True)
    process = await anyio.open_process(list(argv), cwd=str(cwd) if cwd else None, env=env, stdout=_PIPE, stderr=_PIPE)
    # "wb": each run rewrites its streamed log; warm-closure build dirs are serialized by the build lease so there is one writer.
    try:
        with (process_dir / "stdout.log").open("wb") as stdout_file, (process_dir / "stderr.log").open("wb") as stderr_file:
            async with anyio.create_task_group() as group:
                group.start_soon(_collect_stream, process.stdout, stdout_tail, sys.stderr.buffer, stdout_file)
                group.start_soon(_collect_stream, process.stderr, stderr_tail, sys.stderr.buffer, stderr_file)
            returncode = await process.wait()
    except BaseException:
        process.kill()
        with anyio.CancelScope(shield=True):
            await process.wait()
        raise
    return Ok(Completed(argv=argv, returncode=returncode, stdout=b"".join(stdout_tail), stderr=b"".join(stderr_tail)))


async def _collect_stream(stream: ByteReceiveStream | None, tail: deque[bytes], target: ByteWriter, artifact: ByteWriter) -> None:
    match stream:
        case None:
            return
        case _:
            retained = 0
            async for chunk in stream:
                tail.append(chunk)
                retained += len(chunk)
                while retained > _STREAM_TAIL_BYTES:
                    retained -= len(tail.popleft())
                artifact.write(chunk)
                artifact.flush()
                target.write(chunk)
                target.flush()


def _process_dir(argv: tuple[str, ...], artifact_dir: Path | None) -> Path:
    base = artifact_dir or Path.cwd() / ".artifacts" / "quality" / "process"
    text = "-".join("".join(character if character.isalnum() else "-" for character in part).strip("-").lower() for part in argv[:6] if part.strip())
    command_id = text[:96] or "command"
    return base / command_id


def _fault_status(returncode: int) -> RailStatus:
    match returncode:
        case 0:
            return "empty"
        case 5:
            return "busy"
        case 124:
            return "timeout"
        case _:
            return "failed"
