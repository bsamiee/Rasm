"""Subprocess and dotnet execution primitives shared by all quality rails."""

# --- [IMPORTS] ------------------------------------------------------------------------

from __future__ import annotations

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

from tools.quality.settings import ArtifactScope, PROJECT_EXCLUDE_DIRS, QualitySettings, SERIAL_BUILD


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
                return ProcessFault(argv=argv, returncode=returncode, stderr=stderr)
            case text:
                return ProcessFault(argv=argv, returncode=returncode, stderr=text.encode())

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
            index[directory]
            for directory in (file.parent, *file.parent.parents)
            if directory.is_relative_to(self.root) and directory in index
        )
        match ranked:
            case (project,):
                return Ok(project)
            case ():
                return Error(ProcessFault.fail("resolve", str(file), detail=b"No owning project found"))
            case _:
                return Error(
                    ProcessFault.fail(
                        "resolve", str(file), detail=f"Expected one owning project for {file}, found {len(ranked)}"
                    )
                )

    def owner_rel(self, index: ProjectIndex, file: Path) -> str | None:
        return self.owner(index, file).map(lambda project: str(project.relative_to(self.root))).default_value(None)

    @staticmethod
    def csproj(project: Path, tag: str = "") -> ET.Element | str | None:
        match project:
            case Path() as target if target.is_file():
                pass
            case _:
                return None
        # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=xml-etree-csproj ticket=QUALITY-R5
        # expires=2026-12-31 rationale=stdlib-xml-boundary
        try:
            node = ET.parse(target).getroot()  # noqa: S314
        except ET.ParseError:
            return None
        return (
            node
            if not tag
            else next(
                (
                    value.strip()
                    for child in node.iter()
                    if child.tag.rpartition("}")[-1] == tag
                    for value in (child.text,)
                    if value
                ),
                None,
            )
        )


# --- [OPERATIONS] ------------------------------------------------------------------------


def decode_json[T](payload: bytes | str, model: type[T]) -> Result[T, ProcessFault]:
    match payload:
        case bytes() as raw:
            pass
        case str() as text:
            raw = text.encode()
    # RASM_BOUNDARY_EXEMPTION: rule=PYS0001 reason=msgspec-decode ticket=QUALITY-R4
    # expires=2026-12-31 rationale=library-decode-boundary
    try:
        return Ok(msgspec.json.decode(raw, type=model))
    except msgspec.DecodeError as exc:
        return Error(ProcessFault.fail("decode", model.__name__, detail=str(exc)))


def dotnet(
    *args: str, scope: ArtifactScope | None = None, cwd: Path | None = None, check: bool = True
) -> Result[Completed, ProcessFault]:
    match scope:
        case ArtifactScope() as active:
            separator = args.index("--") if "--" in args else len(args)
            return run(
                ("dotnet", *args[:separator], *active.dotnet_flags, *args[separator:]),
                cwd=cwd,
                env=active.dotnet_env,
                check=check,
            )
        case _:
            return run(("dotnet", *args), cwd=cwd, env=None, check=check)


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
            parallel = SERIAL_BUILD if serial else ((f"-maxcpucount:{max_cpu}",) if max_cpu else ())
            return ("build", str(target), "--configuration", configuration, "--no-restore", *version, *parallel)
        case unreachable:
            assert_never(unreachable)


def dotnet_fold(scope: ArtifactScope, commands: tuple[tuple[str, ...], ...]) -> Result[None, ProcessFault]:
    return fold(commands, None, lambda _, command: dotnet(*command, scope=scope, check=True).map(lambda _: None))


@beartype
def dotnet_rail(
    settings: QualitySettings,
    scope: ArtifactScope,
    *,
    restore: Path,
    targets: tuple[Path, ...],
    version: str = "",
    disable_parallel: bool = False,
) -> Result[None, ProcessFault]:
    version_args = settings.version_props(version)
    commands = (
        dotnet_args("restore", restore, disable_parallel=disable_parallel),
        *(
            dotnet_args("build", target, configuration=settings.configuration, version=version_args, serial=True)
            for target in targets
        ),
    )
    return dotnet_fold(scope, commands)


def fd_args(extension: FdExtension, pattern: str = ".", *roots: str | Path, exclude: bool = True) -> tuple[str, ...]:
    paths = tuple(str(root) for root in roots) or (".",)
    excludes = tuple(item for name in PROJECT_EXCLUDE_DIRS for item in ("--exclude", name)) if exclude else ()
    return ("fd", "-H", "-e", extension, pattern, *paths, *excludes)


def fold[T, U](
    items: tuple[T, ...], init: U, step: Callable[[U, T], Result[U, ProcessFault]]
) -> Result[U, ProcessFault]:
    return reduce(lambda acc, item: acc.bind(lambda value: step(value, item)), items, Ok(init))


def run(
    argv: tuple[str, ...], *, cwd: Path | None = None, env: dict[str, str] | None = None, check: bool = False
) -> Result[Completed, ProcessFault]:
    async def _exec() -> Result[Completed, ProcessFault]:
        completed = await anyio.run_process(list(argv), cwd=str(cwd) if cwd else None, env=env, check=False)
        outcome = Completed(
            argv=argv, returncode=completed.returncode, stdout=completed.stdout, stderr=completed.stderr
        )
        detail = b"\n".join(filter(None, (completed.stderr, completed.stdout)))
        return (
            Error(ProcessFault.fail(*argv, detail=detail, returncode=completed.returncode))
            if completed.returncode != 0 and check
            else Ok(outcome)
        )

    return anyio.run(_exec)


def run_fold(scope: ArtifactScope, commands: tuple[tuple[str, ...], ...]) -> Result[None, ProcessFault]:
    return fold(commands, None, lambda _, command: run(command, env=scope.dotnet_env, check=True).map(lambda _: None))
