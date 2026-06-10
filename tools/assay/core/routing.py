"""Route changed or explicit paths into language-specific inputs.

Discriminates between a glob strategy (suffix-only filter, scope always CHANGED) and a
closure strategy (project-graph walk, escalates to FULL when trigger files are touched).
The ``route`` entry point is the public surface; ``place`` projects a ``Routed`` result
into per-tool argument tail groups.
"""

from collections.abc import Mapping
from dataclasses import dataclass
from enum import StrEnum
from functools import reduce
from pathlib import PurePosixPath
from posixpath import normpath
from typing import assert_never, Protocol, runtime_checkable
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local .csproj XML from source.read, never network-sourced

from expression import Error, Ok, Result
import structlog
from upath import UPath

from tools.assay.composition.settings import AssaySettings
from tools.assay.core.model import Base, Fault, Input, Language, Mode, Tool  # noqa: TC001  # msgspec needs Language/Tool annotations at runtime
from tools.assay.core.status import RailStatus


# --- [TYPES] ----------------------------------------------------------------------------


type RoutePaths = tuple[str, ...]
type ProjectIndex = Mapping[str, str]


class Scope(StrEnum):
    """Route scope for a language projection."""

    CHANGED = "changed"
    FULL = "full"


@runtime_checkable
class Source(Protocol):
    """Filesystem-independent source of changed paths, enumerated paths, and file bytes."""

    def changed(self) -> Result[tuple[str, ...], Fault]:
        """Return the normalized, deduplicated set of changed root-relative paths.

        Returns:
            Ok with the change set, or a Fault if the underlying provider fails.
        """

    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        """Expand explicit route paths into normalized root-relative file paths.

        Args:
            paths: Root-relative paths to files or directories to enumerate.

        Returns:
            Ok with flattened file paths, or a Fault if expansion fails.
        """

    def read(self, rel: str) -> Result[bytes, Fault]:
        """Read the raw bytes of one root-relative file.

        Args:
            rel: Root-relative path to the file.

        Returns:
            Ok with file bytes, or a Fault on read failure.
        """


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
    """Resolved routing result for one language and one invocation scope.

    ``scope`` is FULL when trigger files are present; CHANGED otherwise.
    ``projects`` carries the transitive project-graph closure for closure-strategy
    languages; ``groups`` pairs each seed project with the changed files it owns.
    Glob-strategy languages populate only ``files``.
    """

    language: Language
    scope: Scope
    files: tuple[str, ...] = ()
    projects: tuple[str, ...] = ()
    groups: tuple[tuple[str, tuple[str, ...]], ...] = ()
    full_triggers: tuple[str, ...] = ()


# --- [SERVICES] -------------------------------------------------------------------------

_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.routing")


@dataclass(frozen=True, slots=True)
class _LocalSource:
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
    # A missing explicit target warns and contributes zero rows; one stale path does not fault the full request.
    absolute = root / target
    match (absolute.is_dir(), absolute.is_file()):
        case (True, _):
            return _git((*_FD, target, *_FD_EXCLUDE), root=root).map(lambda rows: tuple(PurePosixPath(r).as_posix() for r in rows))
        case (_, True):
            return Ok((PurePosixPath(target).as_posix(),))
        case _:
            _LOG.warning("route.path_missing", target=target)
            return Ok(())


# --- [OPERATIONS] -----------------------------------------------------------------------


def _git(argv: tuple[str, ...], *, root: UPath) -> Result[tuple[str, ...], Fault]:
    from tools.assay.core.engine import discover  # noqa: PLC0415  # deferred: engine imports routing; cycle broken here

    return discover(argv, root=root, timeout=_TIMEOUT).map(lambda raw: tuple(line for line in raw.decode(errors="replace").splitlines() if line))


def _norm(paths: tuple[str, ...]) -> tuple[str, ...]:
    return tuple(sorted({PurePosixPath(p).as_posix() for p in paths}))


def _owner(rel: str, index: ProjectIndex) -> str | None:
    rel_path = PurePosixPath(rel)
    owners = tuple(d for d in index if rel_path.is_relative_to(d))
    return max(owners, key=str.__len__, default=None)


def _refs(rel: str, source: Source) -> frozenset[str]:
    # Unreadable or malformed .csproj becomes an isolated graph node; fault does not propagate.
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
    # Monotone fixed-point: len(graph) passes is sufficient because each pass adds at least one node or terminates.
    graph = {rel: _refs(rel, source) for rel in index.values()}
    return reduce(
        lambda current, _: current | frozenset(p for p, refs in graph.items() if p not in current and bool(current & refs)), range(len(graph)), seeds
    )


def _escalate(files: tuple[str, ...], settings: AssaySettings | None) -> tuple[str, ...]:
    trigger_files = settings.trigger_files if settings is not None else _TRIGGER_FILES
    trigger_prefixes = settings.trigger_prefixes if settings is not None else _TRIGGER_PREFIXES
    return tuple(f for f in files if f in trigger_files or any(f.startswith(prefix) for prefix in trigger_prefixes))


def _glob(language: Language, files: tuple[str, ...]) -> Result[Routed, Fault]:
    # Glob-strategy languages have no project graph, so scope is always CHANGED regardless of trigger files.
    return Ok(Routed(language=language, scope=Scope.CHANGED, files=_norm(tuple(f for f in files if PurePosixPath(f).suffix in language.suffixes))))


def _closure(language: Language, files: tuple[str, ...], source: Source, settings: AssaySettings | None) -> Result[Routed, Fault]:
    triggers = _escalate(files, settings)
    match triggers:
        case ():
            return source.enumerate((".",)).bind(lambda all_files: _resolve(language, files, all_files, source))
        case _:
            return Ok(Routed(language=language, scope=Scope.FULL, files=_norm(files), full_triggers=_norm(triggers)))


def _resolve(language: Language, changed: tuple[str, ...], universe: tuple[str, ...], source: Source) -> Result[Routed, Fault]:
    index = {PurePosixPath(p).parent.as_posix(): PurePosixPath(p).as_posix() for p in universe if PurePosixPath(p).suffix == _CSPROJ}
    owned = {f: owner for f in changed if (owner := _owner(f, index)) is not None}
    seeds = frozenset(index[owner] for owner in owned.values())
    closure = _dependents(seeds, index, source)
    groups = tuple((owner_proj, tuple(sorted(f for f, owner in owned.items() if index[owner] == owner_proj))) for owner_proj in sorted(seeds))
    return Ok(Routed(language=language, scope=Scope.CHANGED, files=_norm(tuple(owned.keys())), projects=tuple(sorted(closure)), groups=groups))


def infer_languages(paths: RoutePaths, available: tuple[Language, ...]) -> tuple[Language, ...]:
    """Infer target languages from path suffixes, preserving ``available`` order.

    Smart-default resolution for rails when ``--language`` is omitted: suffix-set
    intersection narrows to the languages the given paths actually touch, so explicit
    paths never fan out to unrelated language toolchains.

    Args:
        paths: Root-relative paths whose suffixes select languages; directories and
            suffixless paths contribute nothing.
        available: Candidate languages for the calling rail, in dispatch order.

    Returns:
        Languages from ``available`` whose suffix sets intersect the path suffixes, or
        all of ``available`` when no path carries a recognized suffix.
    """
    suffixes = frozenset(suffix for p in paths if (suffix := PurePosixPath(p).suffix))
    return tuple(language for language in available if suffixes & language.suffixes) or available


def route(
    language: Language, paths: RoutePaths = (), *, source: Source | None = None, settings: AssaySettings | None = None
) -> Result[Routed, Fault]:
    """Resolve paths into routed inputs for one language.

    When ``paths`` is empty, the changed-file set from ``source`` is used.
    The root is taken from ``settings.root`` (defaulting to ``AssaySettings()``) so that
    ``ASSAY_ROOT`` can point CI at a worktree other than the process cwd.

    Args:
        language: Target language; determines glob vs. closure routing strategy.
        paths: Explicit root-relative paths to route; uses the change set when empty.
        source: Provider for changed paths, enumeration, and file reads; defaults to a
            local git-backed source rooted at ``settings.root``.
        settings: Active assay configuration; defaults to ``AssaySettings()``.

    Returns:
        Ok with the routed projection, or a Fault propagated from the source provider.
    """
    src = source if source is not None else _LocalSource(root=UPath((settings or AssaySettings()).root))
    enumerated = src.enumerate(paths) if paths else src.changed()
    return enumerated.bind(lambda files: _closure(language, files, src, settings) if language.strategy == "closure" else _glob(language, files))


def routable_files(files: RoutePaths, settings: AssaySettings) -> RoutePaths:
    """Filter probe-fixture paths from an explicit file list.

    Type checkers honor ``pyproject.toml`` excludes only for project-wide runs; passing
    probe-fixture paths explicitly would type-check files that are intentionally excluded.
    Prefixes are governed by ``AssaySettings.probe_fixture_prefixes``.

    Returns:
        File paths with probe-fixture prefixes removed.
    """
    return tuple(path for path in files if not any(path.startswith(prefix) for prefix in settings.probe_fixture_prefixes))


def place(routed: Routed, tool: Tool, *, settings: AssaySettings) -> tuple[tuple[str, ...], ...]:
    """Project routed inputs into command argument tail groups for one tool.

    Each inner tuple is one invocation's argument tail. An empty outer tuple means no
    invocations are needed. For ``Input.PROJECT`` with no routed projects and
    ``tool.mode`` is ``Mode.LIST``, falls back to ``settings.test_target``.

    Args:
        routed: Resolved routing result for the target language.
        tool: Tool descriptor that determines the input mode and dispatch shape.
        settings: Active assay configuration; provides solution, test-target, and
            probe-fixture prefix overrides.

    Returns:
        Tuple of argument tail tuples, one per invocation.
    """
    match tool.input:
        case Input.FILES:
            files = routable_files(routed.files, settings)
            return ((*files,),) if files else ()
        case Input.INCLUDE:
            return tuple((project, *Input.INCLUDE.flag, *files) for project, files in routed.groups)
        case Input.PROJECT:
            projects = routed.projects or ((str(settings.test_target),) if tool.mode is Mode.LIST else ())
            return tuple((project,) for project in projects)
        case Input.SOLUTION:
            return ((str(settings.solution),),)
        case Input.NONE:
            files = routable_files(routed.files, settings)
            return ((*files,),) if files else ((),)
        case never:  # pragma: no cover
            assert_never(never)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ProjectIndex", "RoutePaths", "Routed", "Scope", "Source", "infer_languages", "place", "routable_files", "route"]
