"""Subprocess and dotnet execution primitives shared by all quality rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from collections.abc import Callable, Mapping
from dataclasses import dataclass
from functools import reduce
from pathlib import Path
from typing import assert_never, Final, Literal
import xml.etree.ElementTree as ET  # noqa: S405

import anyio
from beartype import beartype
from expression import Error, Ok, Result
import msgspec

from tools.quality.settings import ArtifactScope, PROJECT_EXCLUDE_DIRS, QualitySettings


# --- [TYPES] ---------------------------------------------------------------------------

type DotnetOp = Literal["restore", "build"]
type FdExtension = Literal["csproj", "csx"]
type ProjectIndex = Mapping[Path, Path]


# --- [CONSTANTS] -----------------------------------------------------------------------

_GIT_CHANGE_ARGS: Final[tuple[tuple[str, ...], ...]] = (
    ("diff", "--name-only", "--diff-filter=ACDMRTUXB"),
    ("diff", "--cached", "--name-only", "--diff-filter=ACDMRTUXB"),
    ("ls-files", "--others", "--exclude-standard"),
)
# Build-graph verbs only: scoped invocations splice `--artifacts-path`/`--disable-build-servers`; tool-driver verbs reject them.
_ARTIFACT_SCOPED_VERBS: Final[frozenset[str]] = frozenset(("build", "clean", "msbuild", "pack", "publish", "restore", "run", "test", "vstest"))


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


def dotnet(
    *args: str, scope: ArtifactScope | None = None, cwd: Path | None = None, check: bool = True, timeout: float | None = None, scoped: bool = True
) -> Result[Completed, ProcessFault]:
    # scoped=True splices artifact flags for build-graph verbs; scoped=False keeps default bin/ for scenario-kit and yak staging.
    match scope:
        case ArtifactScope() as active if scoped and args and args[0] in _ARTIFACT_SCOPED_VERBS:
            separator = args.index("--") if "--" in args else len(args)
            return run(
                ("dotnet", *args[:separator], *active.dotnet_flags, *args[separator:]), cwd=cwd, env=active.dotnet_env, check=check, timeout=timeout
            )
        case ArtifactScope() as active:
            return run(("dotnet", *args), cwd=cwd, env=active.dotnet_env, check=check, timeout=timeout)
        case _:
            return run(("dotnet", *args), cwd=cwd, env=None, check=check, timeout=timeout)


def dotnet_args(
    op: DotnetOp,
    target: str | Path,
    *,
    configuration: str = "",
    version: tuple[str, ...] = (),
    disable_parallel: bool = False,
    max_cpu: int | None = None,
    serial: bool = False,
) -> tuple[str, ...]:
    match op:
        case "restore":
            return ("restore", str(target), "--locked-mode", *(("--disable-parallel",) if disable_parallel else ()))
        case "build":
            cpu = 1 if serial else max_cpu
            extra = ("-p:BuildInParallel=false",) if serial else ()
            parallel = (f"-maxcpucount:{cpu}", *extra) if cpu is not None else ()
            return ("build", str(target), "--configuration", configuration, "--no-restore", *version, *parallel)
        case unreachable:
            assert_never(unreachable)


@beartype
def dotnet_build(
    settings: QualitySettings,
    scope: ArtifactScope,
    *,
    restore: str | Path,
    targets: tuple[str | Path, ...],
    configurations: tuple[str, ...] = (),
    version: str = "",
    disable_parallel: bool = False,
    serial: bool = False,
    max_cpu: int | None = None,
    scoped: bool = True,
) -> Result[None, ProcessFault]:
    configs = configurations or (settings.configuration,)
    version_args = settings.version_props(version)
    commands = (
        dotnet_args("restore", restore, disable_parallel=disable_parallel),
        *(
            dotnet_args("build", target, configuration=configuration, version=version_args, serial=serial, max_cpu=max_cpu)
            for configuration in configs
            for target in targets
        ),
    )
    return fold(commands, None, lambda _, command: dotnet(*command, scope=scope, scoped=scoped, check=True).map(lambda _: None))


def fd_args(extension: FdExtension, pattern: str = ".", *roots: str | Path, exclude: bool = True) -> tuple[str, ...]:
    paths = tuple(str(root) for root in roots) or (".",)
    excludes = tuple(item for name in PROJECT_EXCLUDE_DIRS for item in ("--exclude", name)) if exclude else ()
    return ("fd", "-H", "-e", extension, pattern, *paths, *excludes)


def fold[T, U](items: tuple[T, ...], init: U, step: Callable[[U, T], Result[U, ProcessFault]]) -> Result[U, ProcessFault]:
    return reduce(lambda acc, item: acc.bind(lambda value: step(value, item)), items, Ok(init))


def run(
    argv: tuple[str, ...], *, cwd: Path | None = None, env: dict[str, str] | None = None, check: bool = False, timeout: float | None = None
) -> Result[Completed, ProcessFault]:
    async def _exec() -> Result[Completed, ProcessFault]:
        completed = await anyio.run_process(list(argv), cwd=str(cwd) if cwd else None, env=env, check=False)
        outcome = Completed(argv=argv, returncode=completed.returncode, stdout=completed.stdout, stderr=completed.stderr)
        detail = b"\n".join(filter(None, (completed.stderr, completed.stdout)))
        return Error(ProcessFault.fail(*argv, detail=detail, returncode=completed.returncode)) if completed.returncode != 0 and check else Ok(outcome)

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
