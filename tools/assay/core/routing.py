"""Route changed or explicit paths into language-specific inputs."""

from collections.abc import Mapping
from dataclasses import dataclass
from enum import StrEnum
from functools import reduce
from pathlib import PurePosixPath
from posixpath import normpath
from typing import assert_never, Final, Protocol, runtime_checkable
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local .csproj XML from source.read, never network-sourced

from expression import Error, Ok, Result
import structlog
from upath import UPath

from tools.assay.composition.settings import AssaySettings
from tools.assay.core.model import (  # noqa: TC001  # msgspec needs Language/Tool annotations at runtime; intra-package (tools.assay package root)
    Base,
    Fault,
    Input,
    Language,
    Mode,
    Tool,
)
from tools.assay.core.status import RailStatus


# --- [TYPES] ----------------------------------------------------------------------------


type RoutePaths = tuple[str, ...]
type ProjectIndex = Mapping[str, str]

_PROBE_FIXTURE_PREFIXES: Final[tuple[str, ...]] = ("tests/tools/ast-grep/", "tests/tools/py_analyzer/")


class Scope(StrEnum):
    """Route scope for a language projection."""

    CHANGED = "changed"
    FULL = "full"


@runtime_checkable
class Source(Protocol):
    """Filesystem-independent source of changed paths, enumerated paths, and file bytes."""

    def changed(self) -> Result[tuple[str, ...], Fault]:
        """Return the language-agnostic change set."""

    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        """Expand explicit route paths."""

    def read(self, rel: str) -> Result[bytes, Fault]:
        """Read one root-relative file."""


# --- [CONSTANTS] ------------------------------------------------------------------------

_TRIGGER_FILES: frozenset[str] = frozenset((
    ".config/dotnet-tools.json",
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    "Workspace.slnx",
    ".editorconfig",
    "global.json",
))
_TRIGGER_PREFIXES: tuple[str, ...] = ("tools/cs-analyzer/",)

_CSPROJ: str = ".csproj"
_PROJECT_REF: str = ".//ProjectReference"
_TIMEOUT: float = 30.0
_DIFF: tuple[str, ...] = ("git", "diff", "--name-only", "--diff-filter=ACDMRTUXB")
_CACHED: tuple[str, ...] = ("git", "diff", "--cached", "--name-only", "--diff-filter=ACDMRTUXB")
_UNTRACKED: tuple[str, ...] = ("git", "ls-files", "--others", "--exclude-standard")
_FD: tuple[str, ...] = ("fd", "-H", "-t", "f", ".")
_FD_EXCLUDE: tuple[str, ...] = ("--exclude", ".git", "--exclude", "bin", "--exclude", "obj")


# --- [MODELS] ---------------------------------------------------------------------------


class Routed(Base, frozen=True):
    """Resolved inputs for one language."""

    language: Language
    scope: Scope
    files: tuple[str, ...] = ()
    projects: tuple[str, ...] = ()
    groups: tuple[tuple[str, tuple[str, ...]], ...] = ()
    full_triggers: tuple[str, ...] = ()


# --- [SERVICES] -------------------------------------------------------------------------

_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.routing")


# --- [OPERATIONS] -----------------------------------------------------------------------


def _git(argv: tuple[str, ...], *, root: UPath) -> Result[tuple[str, ...], Fault]:
    # Discovery subprocess ownership lives in core.engine; routing only decodes stdout rows.
    from tools.assay.core.engine import discover  # noqa: PLC0415  # local import avoids the engine->routing import cycle

    return discover(argv, root=root, timeout=_TIMEOUT).map(lambda raw: tuple(line for line in raw.decode(errors="replace").splitlines() if line))


def _norm(paths: tuple[str, ...]) -> tuple[str, ...]:
    return tuple(sorted({PurePosixPath(p).as_posix() for p in paths}))


def _owner(rel: str, index: ProjectIndex) -> str | None:
    rel_path = PurePosixPath(rel)
    owners = tuple(d for d in index if rel_path.is_relative_to(d))
    return max(owners, key=str.__len__, default=None)


def _refs(rel: str, source: Source) -> frozenset[str]:
    # Read one project node through Source; unreadable or malformed XML becomes an isolated node.
    match source.read(rel):
        case Result(tag="ok", ok=raw):
            base = PurePosixPath(rel).parent
            tree = _parse(raw)
            return frozenset(
                normpath(str(base / inc.replace("\\", "/"))) for ref in tree.iterfind(_PROJECT_REF) if (inc := ref.get("Include")) is not None
            )
        case _:
            return frozenset()


def _parse(raw: bytes) -> ET.Element:
    try:
        return ET.fromstring(raw)  # noqa: S314  # trusted local .csproj XML from source.read, never network-sourced
    except ET.ParseError:
        return ET.Element("Project")


def _dependents(seeds: frozenset[str], index: ProjectIndex, source: Source) -> frozenset[str]:
    # Reverse reachability converges within len(graph) monotone passes over the project DAG.
    graph = {rel: _refs(rel, source) for rel in index.values()}
    return reduce(
        lambda current, _: current | frozenset(p for p, refs in graph.items() if p not in current and bool(current & refs)), range(len(graph)), seeds
    )


def _escalate(files: tuple[str, ...], settings: AssaySettings | None) -> tuple[str, ...]:
    # Settings can widen the trigger vocabulary; tests and settings-free callers use module constants.
    trigger_files = settings.trigger_files if settings is not None else _TRIGGER_FILES
    trigger_prefixes = settings.trigger_prefixes if settings is not None else _TRIGGER_PREFIXES
    return tuple(f for f in files if f in trigger_files or any(f.startswith(prefix) for prefix in trigger_prefixes))


def _glob(language: Language, files: tuple[str, ...]) -> Result[Routed, Fault]:
    # Glob languages never route to SOLUTION, so suffix filtering always stays CHANGED.
    return Ok(Routed(language=language, scope=Scope.CHANGED, files=_norm(tuple(f for f in files if PurePosixPath(f).suffix in language.suffixes))))


def _closure(language: Language, files: tuple[str, ...], source: Source, settings: AssaySettings | None) -> Result[Routed, Fault]:
    # Full escalation skips project graph work; otherwise resolve the changed-project closure.
    triggers = _escalate(files, settings)
    match triggers:
        case ():
            return source.enumerate((".",)).bind(lambda all_files: _resolve(language, files, all_files, source))
        case _:
            return Ok(Routed(language=language, scope=Scope.FULL, files=_norm(files), full_triggers=_norm(triggers)))


def _resolve(language: Language, changed: tuple[str, ...], universe: tuple[str, ...], source: Source) -> Result[Routed, Fault]:
    # Index by owner directory, then group changed files for dotnet format --include.
    index = {PurePosixPath(p).parent.as_posix(): PurePosixPath(p).as_posix() for p in universe if PurePosixPath(p).suffix == _CSPROJ}
    owned = {f: owner for f in changed if (owner := _owner(f, index)) is not None}
    seeds = frozenset(index[owner] for owner in owned.values())
    closure = _dependents(seeds, index, source)
    groups = tuple((owner_proj, tuple(sorted(f for f, owner in owned.items() if index[owner] == owner_proj))) for owner_proj in sorted(seeds))
    return Ok(Routed(language=language, scope=Scope.CHANGED, files=_norm(tuple(owned.keys())), projects=tuple(sorted(closure)), groups=groups))


def route(
    language: Language, paths: RoutePaths = (), *, source: Source | None = None, settings: AssaySettings | None = None
) -> Result[Routed, Fault]:
    """Resolve paths into routed inputs for one language.

    Returns:
        Routed input projection, or a fault from the source provider.
    """
    # Default source binds the changeset to settings.root, not ambient cwd, so ASSAY_ROOT routes its own worktree.
    src = source if source is not None else _LocalSource(root=UPath((settings or AssaySettings()).root))
    enumerated = src.enumerate(paths) if paths else src.changed()
    return enumerated.bind(lambda files: _closure(language, files, src, settings) if language.strategy == "closure" else _glob(language, files))


def _routable_files(files: RoutePaths) -> RoutePaths:
    # ty/mypy honor pyproject excludes only for project-wide runs; explicit argv still type-checks probe fixtures.
    return tuple(path for path in files if not any(path.startswith(prefix) for prefix in _PROBE_FIXTURE_PREFIXES))


def place(routed: Routed, tool: Tool, *, settings: AssaySettings) -> tuple[tuple[str, ...], ...]:
    """Project routed inputs into command argument tails.

    Returns:
        Command argument tail groups for the tool input mode.
    """
    match tool.input:
        case Input.FILES:
            files = _routable_files(routed.files)
            return ((*files,),) if files else ()
        case Input.INCLUDE:
            return tuple((project, *Input.INCLUDE.flag, *files) for project, files in routed.groups)
        case Input.PROJECT:
            projects = routed.projects or ((str(settings.test_target),) if tool.mode is Mode.LIST else ())
            return tuple((project,) for project in projects)
        case Input.SOLUTION:
            return ((str(settings.solution),),)
        case Input.NONE:
            files = _routable_files(routed.files)
            return ((*files,),) if files else ((),)
        case never:  # pragma: no cover
            assert_never(never)


# --- [COMPOSITION] ----------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class _LocalSource:
    # Default Source: git change-set, fd enumeration, and UPath reads rooted at the configured workspace.
    root: UPath

    def changed(self) -> Result[tuple[str, ...], Fault]:
        seed: Result[tuple[str, ...], Fault] = Ok(())
        return reduce(
            lambda acc, argv: acc.bind(lambda seen: _git(argv, root=self.root).map(lambda rows: seen + rows)), (_DIFF, _CACHED, _UNTRACKED), seed
        ).map(_norm)

    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        seed: Result[tuple[str, ...], Fault] = Ok(())
        return reduce(lambda acc, p: acc.bind(lambda seen: _expand(p, root=self.root).map(lambda rows: seen + rows)), paths, seed).map(_norm)

    def read(self, rel: str) -> Result[bytes, Fault]:
        try:
            return Ok((self.root / rel).read_bytes())
        except OSError as exc:
            return Error(Fault(("read", rel), RailStatus.FAULTED, str(exc)[:1024]))


def _expand(target: str, *, root: UPath) -> Result[tuple[str, ...], Fault]:
    # Missing explicit paths warn and contribute zero rows so one stale target does not fault the full request.
    absolute = root / target
    match (absolute.is_dir(), absolute.is_file()):
        case (True, _):
            return _git((*_FD, target, *_FD_EXCLUDE), root=root).map(lambda rows: tuple(PurePosixPath(r).as_posix() for r in rows))
        case (_, True):
            return Ok((PurePosixPath(target).as_posix(),))
        case _:
            _LOG.warning("route.path_missing", target=target)
            return Ok(())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ProjectIndex", "RoutePaths", "Routed", "Scope", "Source", "place", "route"]
