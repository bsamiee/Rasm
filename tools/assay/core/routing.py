"""Route a git change-set through a single ``Source`` into a uniform ``Routed`` per ``Language``.

The strategy asymmetry is keyed by ``Language.strategy``: ``"closure"`` (C#) resolves owning-``.csproj``
reverse-dependency closures from the project graph; ``"glob"`` resolves suffix globs from the change-set.
A sixth ``Language`` adds no arm; a sixth filesystem is a new ``Source`` injected at ``route(..., source=)``.
The closure arm reads ``.csproj`` through ``source.read``, so the graph algorithm is filesystem-blind.
"""

from collections.abc import Mapping
from enum import StrEnum
from functools import reduce
from pathlib import PurePosixPath
from posixpath import normpath
from subprocess import CalledProcessError  # noqa: S404  # exception type only; anyio.run_process owns the spawn
from typing import assert_never, Protocol, runtime_checkable
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local .csproj XML from source.read, never network-sourced

import anyio
from expression import Error, Ok, Result
import structlog
from upath import UPath

from tools.assay.composition.settings import AssaySettings  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # noqa: TC001  # msgspec needs Language/Tool annotations at runtime; intra-package (tools.assay package root)
    Base,
    Fault,
    Input,
    Language,
    Tool,
)
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


# --- [TYPES] ----------------------------------------------------------------------------


type RoutePaths = tuple[str, ...]
type ProjectIndex = Mapping[str, str]  # owner-dir -> root-relative .csproj


class Scope(StrEnum):
    """Route-scope axis (``Routed.scope``): ``CHANGED`` grows a dependents closure; ``FULL`` lists the whole target."""

    CHANGED = "changed"
    FULL = "full"


@runtime_checkable
class Source(Protocol):
    """Assay's sole ``Protocol``: change-set + enumeration + read provider, keeping routing filesystem-blind."""

    def changed(self) -> Result[tuple[str, ...], Fault]:
        """Yield the language-agnostic git change-set."""

    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        """Expand an explicit ``paths`` selection: dir → ``fd`` scan, file verbatim, missing → ``Fault``."""

    def read(self, rel: str) -> Result[bytes, Fault]:
        """Fetch one root-relative path's bytes (``.csproj`` XML for the closure arm)."""


# --- [MODELS] ---------------------------------------------------------------------------


class Routed(Base, frozen=True):
    """One uniform routed-input shape across every ``Language``; each strategy arm fills a subset.

    Attributes:
        files: Sorted root-relative source for the ``FILES``/``INCLUDE`` projections.
        groups: ``owner -> changed-files`` pairing the ``dotnet format --include`` arm fans over.
        full_triggers: Change-set rows that escalated ``scope`` to ``FULL`` on the fast path,
            skipping closure resolution.
    """

    language: Language
    scope: Scope
    files: tuple[str, ...] = ()
    projects: tuple[str, ...] = ()
    groups: tuple[tuple[str, tuple[str, ...]], ...] = ()
    full_triggers: tuple[str, ...] = ()


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
_TRIGGER_PREFIXES: tuple[str, ...] = ("tools/cs-analyzer/",)  # any descendant escalates the route to FULL

_CSPROJ: str = ".csproj"
_PROJECT_REF: str = ".//ProjectReference"
_TIMEOUT: float = 30.0
_DIFF: tuple[str, ...] = ("git", "diff", "--name-only", "--diff-filter=ACDMRTUXB")
_CACHED: tuple[str, ...] = ("git", "diff", "--cached", "--name-only", "--diff-filter=ACDMRTUXB")
_UNTRACKED: tuple[str, ...] = ("git", "ls-files", "--others", "--exclude-standard")
_FD: tuple[str, ...] = ("fd", "-H", "-t", "f", ".")
_FD_EXCLUDE: tuple[str, ...] = ("--exclude", ".git", "--exclude", "bin", "--exclude", "obj")
_LOG: structlog.stdlib.BoundLogger = structlog.get_logger("assay.routing")


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _spawn(argv: tuple[str, ...], root: UPath) -> bytes:
    # check=True + fail_after raise CalledProcessError/TimeoutError, caught at _git's boundary;
    # cwd takes str(root) (the UPath path of a local root), never the backend URI
    with anyio.fail_after(_TIMEOUT):
        done = await anyio.run_process(list(argv), cwd=str(root), check=True)
    return done.stdout


def _git(argv: tuple[str, ...], *, root: UPath) -> Result[tuple[str, ...], Fault]:
    # the lone exception seam this module owns: OSError/Timeout/non-zero -> Fault value;
    # empty output entries are dropped so a trailing newline never yields a phantom "" row
    try:
        raw = anyio.run(_spawn, argv, root)
    except TimeoutError:
        return Error(Fault(argv, RailStatus.TIMEOUT, f"timeout after {_TIMEOUT}s"))
    except CalledProcessError as exc:
        return Error(Fault(argv, RailStatus.FAULTED, (exc.stderr or b"").decode(errors="replace")[:1024]))
    except OSError as exc:
        return Error(Fault(argv, RailStatus.FAULTED, str(exc)[:1024]))
    return Ok(tuple(line for line in raw.decode(errors="replace").splitlines() if line))


def _norm(paths: tuple[str, ...]) -> tuple[str, ...]:
    # canonical root-relative POSIX shape: the Routed.files invariant
    return tuple(sorted({PurePosixPath(p).as_posix() for p in paths}))


def _owner(rel: str, index: ProjectIndex) -> str | None:
    # deepest ancestor index dir, or None: an orphan .cs edit seeds no closure rather than a phantom
    rel_path = PurePosixPath(rel)
    owners = tuple(d for d in index if rel_path.is_relative_to(d))
    return max(owners, key=str.__len__, default=None)  # deepest ancestor dir; str.__len__ keeps the key result str-typed


def _refs(rel: str, source: Source) -> frozenset[str]:
    # ProjectReference set of one .csproj node, read via source.read (filesystem-blind);
    # a read fault or unparseable XML yields frozenset() (isolated node) keeping _dependents total.
    # Memoization is the per-run graph dict in _dependents, never a process-static @cache that would
    # bleed across runs of a mutated worktree.
    match source.read(rel):
        case Result(tag="ok", ok=raw):
            base = PurePosixPath(rel).parent
            tree = _parse(raw)
            return frozenset(
                normpath(str(base / inc.replace("\\", "/")))  # posix lexical fold of ../ — PurePosixPath is non-normalizing
                for ref in tree.iterfind(_PROJECT_REF)
                if (inc := ref.get("Include")) is not None
            )
        case _:
            return frozenset()


def _parse(raw: bytes) -> ET.Element:
    # malformed XML degrades to an empty Project node, never a raise
    try:
        return ET.fromstring(raw)  # noqa: S314  # trusted local .csproj XML from source.read, never network-sourced
    except ET.ParseError:
        return ET.Element("Project")


def _dependents(seeds: frozenset[str], index: ProjectIndex, source: Source) -> frozenset[str]:
    # reverse-reachability fixpoint in frozenset set-algebra, iterated len(graph) times — the exact
    # worst-case DAG reverse-reachability chain length; each pass is monotone under union, so this bound
    # reaches the closure and keeps an early-stop while (imperative branching) out of the rail.
    graph = {rel: _refs(rel, source) for rel in index.values()}
    return reduce(
        lambda current, _: current | frozenset(p for p, refs in graph.items() if p not in current and bool(current & refs)), range(len(graph)), seeds
    )


def _escalate(files: tuple[str, ...], settings: AssaySettings | None) -> tuple[str, ...]:
    # change-set rows escalating scope to FULL (trigger files / analyzer-tree edits); settings supplies
    # the Tier-A vocabulary, None falls back to the module constants so settings-free route(...) + the
    # property tests keep the canonical set while a rail-supplied settings can widen it via env.
    trigger_files = settings.trigger_files if settings is not None else _TRIGGER_FILES
    trigger_prefixes = settings.trigger_prefixes if settings is not None else _TRIGGER_PREFIXES
    return tuple(f for f in files if f in trigger_files or any(f.startswith(prefix) for prefix in trigger_prefixes))


def _glob(language: Language, files: tuple[str, ...]) -> Result[Routed, Fault]:
    # suffix-filter the change-set; always Scope.CHANGED — FULL escalation is a CLOSURE-arm concern
    # feeding place's dotnet-only SOLUTION projection, and a glob language never routes to SOLUTION
    return Ok(Routed(language=language, scope=Scope.CHANGED, files=_norm(tuple(f for f in files if PurePosixPath(f).suffix in language.suffixes))))


def _closure(language: Language, files: tuple[str, ...], source: Source, settings: AssaySettings | None) -> Result[Routed, Fault]:
    # C# escalate-or-resolve. On a FULL escalation we skip the index touch and the _dependents fixpoint
    # entirely, leaving place to read SOLUTION. Otherwise we discover the project graph and grow it.
    triggers = _escalate(files, settings)
    match triggers:
        case ():
            return source.enumerate((".",)).bind(lambda all_files: _resolve(language, files, all_files, source))
        case _:
            return Ok(Routed(language=language, scope=Scope.FULL, files=_norm(files), full_triggers=_norm(triggers)))


def _resolve(language: Language, changed: tuple[str, ...], universe: tuple[str, ...], source: Source) -> Result[Routed, Fault]:
    # index keys each project by owning dir (so _owner matches the deepest ancestor); groups pairs each
    # seed owner with its changed files for the dotnet format --include projection
    index = {PurePosixPath(p).parent.as_posix(): PurePosixPath(p).as_posix() for p in universe if PurePosixPath(p).suffix == _CSPROJ}
    owned = {f: owner for f in changed if (owner := _owner(f, index)) is not None}
    seeds = frozenset(index[owner] for owner in owned.values())
    closure = _dependents(seeds, index, source)
    groups = tuple((owner_proj, tuple(sorted(f for f, owner in owned.items() if index[owner] == owner_proj))) for owner_proj in sorted(seeds))
    return Ok(Routed(language=language, scope=Scope.CHANGED, files=_norm(tuple(owned.keys())), projects=tuple(sorted(closure)), groups=groups))


def route(
    language: Language, paths: RoutePaths = (), *, source: Source | None = None, settings: AssaySettings | None = None
) -> Result[Routed, Fault]:
    """Fold a git change-set or explicit ``paths`` into routed inputs for one ``Language``.

    Discriminates ``paths`` (explicit → ``source.enumerate``; empty → ``source.changed``), then binds the
    rows into the strategy arm keyed by ``language.strategy`` (never ``Language`` identity, so a new
    language adds no arm). ``source`` defaults to ``LOCAL``; ``settings`` threads the escalation vocabulary.
    """
    src = source if source is not None else LOCAL
    enumerated = src.enumerate(paths) if paths else src.changed()
    return enumerated.bind(lambda files: _closure(language, files, src, settings) if language.strategy == "closure" else _glob(language, files))


def place(routed: Routed, tool: Tool, *, settings: AssaySettings) -> tuple[tuple[str, ...], ...]:
    """The sole argv-tail projector: a total projection keyed by ``Input``, never ``Language``.

    ``assert_never`` closes the exhaustive ``match``, so a sixth ``Input`` is a type error here, not a
    silent empty tail. A self-walking tool (ast-grep, biome ``ci``) rides ``Input.NONE`` and splices its
    own paths into ``tool.command`` — there is no glob projection.
    """
    match tool.input:
        case Input.FILES:
            return ((*routed.files,),) if routed.files else ()
        case Input.INCLUDE:
            return tuple((project, *Input.INCLUDE.flag, *files) for project, files in routed.groups)
        case Input.PROJECT:
            return tuple((project,) for project in routed.projects)
        case Input.SOLUTION:
            return ((str(settings.solution),),)
        case Input.NONE:
            return ((),)
        case never:  # pragma: no cover
            assert_never(never)


# --- [COMPOSITION] ----------------------------------------------------------------------


class _LocalSource:
    # default Source: git change-set + fd enumeration + UPath read, rooted at cwd; all I/O rides the
    # marked _git boundary or a single UPath.read_bytes whose OSError becomes a Fault value.
    # _root is a UPath so a memory://-rooted Source reads the change-set unchanged with zero local IO.

    @property
    def _root(self) -> UPath:
        return UPath(AssaySettings().root)

    def changed(self) -> Result[tuple[str, ...], Fault]:
        root = self._root
        seed: Result[tuple[str, ...], Fault] = Ok(())
        return reduce(
            lambda acc, argv: acc.bind(lambda seen: _git(argv, root=root).map(lambda rows: seen + rows)), (_DIFF, _CACHED, _UNTRACKED), seed
        ).map(_norm)

    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        root = self._root
        seed: Result[tuple[str, ...], Fault] = Ok(())
        return reduce(lambda acc, p: acc.bind(lambda seen: _expand(p, root=root).map(lambda rows: seen + rows)), paths, seed).map(_norm)

    def read(self, rel: str) -> Result[bytes, Fault]:
        try:
            return Ok((self._root / rel).read_bytes())
        except OSError as exc:
            return Error(Fault(("read", rel), RailStatus.FAULTED, str(exc)[:1024]))


def _expand(target: str, *, root: UPath) -> Result[tuple[str, ...], Fault]:
    # dir -> fd recursive scan, file -> verbatim, missing -> skip + warn. The missing path is the
    # resilience boundary: a hard Fault would short-circuit sequence and nuke every other valid path,
    # so a stale/typo'd path logs route.path_missing and contributes ZERO rows (partial results, never
    # an opaque whole-request failure). The is_dir/is_file probe is the only concrete-filesystem touch.
    absolute = root / target
    match (absolute.is_dir(), absolute.is_file()):
        case (True, _):
            return _git((*_FD, target, *_FD_EXCLUDE), root=root).map(lambda rows: tuple(PurePosixPath(r).as_posix() for r in rows))
        case (_, True):
            return Ok((PurePosixPath(target).as_posix(),))
        case _:
            _LOG.warning("route.path_missing", target=target)
            return Ok(())


LOCAL: Source = _LocalSource()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LOCAL", "ProjectIndex", "RoutePaths", "Routed", "Scope", "Source", "place", "route"]
