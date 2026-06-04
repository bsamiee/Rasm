"""Route a git change-set through a single ``Source`` into a uniform ``Routed`` per ``Language``.

Owns the corpus's sole ``Source`` ``Protocol`` and the whole strategy asymmetry, keyed by
``Language.strategy``: C# (``"closure"``) resolves owning-``.csproj`` reverse-dependency closures from
the project graph; every glob language resolves suffix globs from the change-set. A sixth ``Language``
adds no arm; a sixth filesystem is a new ``Source`` injected at ``route(..., source=)``. The closure
arm reads every ``.csproj`` through ``source.read`` (never ``Path.read_bytes``), so the graph algorithm
is filesystem-blind — a ``memory://`` ``Source`` resolves the same closure with zero IO. ``LOCAL`` binds
git + ``fd`` + ``upath.UPath`` at one marked subprocess boundary mapping ``OSError``/``TimeoutError`` to a
``Fault`` value. ``place`` is the sole argv-tail projector, keyed by the ``Input`` axis, never ``Language``.
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
from upath import UPath

from tools.assay._TMP.composition.settings import AssaySettings  # noqa: PLC2701  # intra-staging import; _TMP is the package root
from tools.assay._TMP.core.model import (  # noqa: PLC2701, TC001  # msgspec needs Language/Tool annotations at runtime; intra-staging _TMP root
    Base,
    Fault,
    Input,
    Language,
    Tool,
)
from tools.assay._TMP.core.status import RailStatus  # noqa: PLC2701  # intra-staging import; _TMP is the package root


# --- [TYPES] ----------------------------------------------------------------------------


type RoutePaths = tuple[str, ...]
type ProjectIndex = Mapping[str, str]  # owner-dir -> root-relative .csproj


class Scope(StrEnum):
    """Route-scope axis (``Routed.scope``): Cyclopts choice token, ``msgspec`` wire value, and ``match`` discriminant.

    ``CHANGED`` grows a dependents closure off the change-set; ``FULL`` lists the whole target
    (``place`` reads ``SOLUTION``).
    """

    CHANGED = "changed"
    FULL = "full"


@runtime_checkable
class Source(Protocol):
    """Assay's sole ``Protocol``: change-set + enumeration + read provider, keeping routing filesystem-blind.

    ``changed`` yields the language-agnostic git change-set; ``enumerate`` expands an explicit
    ``paths`` selection (dir → ``fd`` scan, file verbatim, missing → ``Fault``); ``read`` fetches
    ``.csproj`` XML for the closure arm. A new backend is one implementation, never a new ``route`` signature.
    """

    def changed(self) -> Result[tuple[str, ...], Fault]: ...
    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]: ...
    def read(self, rel: str) -> Result[bytes, Fault]: ...


# --- [MODELS] ---------------------------------------------------------------------------


class Routed(Base, frozen=True):
    """One uniform routed-input shape across every ``Language``; each strategy arm fills a subset.

    The closure arm populates ``projects``/``groups`` and leaves the glob-only fields ``()``; the glob
    arm does the inverse.

    Attributes:
        files: Sorted root-relative source for the ``FILES``/``INCLUDE``/``GLOB`` projections.
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


# --- [OPERATIONS] -----------------------------------------------------------------------


async def _spawn(argv: tuple[str, ...], root: UPath) -> bytes:
    """The one async git/fd spawn: ``anyio.run_process`` under a ``fail_after`` deadline, captured stdout.

    ``check=True`` and ``fail_after`` raise ``CalledProcessError``/``TimeoutError``; both surface to
    ``_git``'s marked boundary where they map to a ``Fault`` value — no exception crosses into domain
    logic. ``cwd`` takes ``str(root)`` — the ``UPath`` ``path`` of a local root, never the backend URI.
    """
    with anyio.fail_after(_TIMEOUT):
        done = await anyio.run_process(list(argv), cwd=str(root), check=True)
    return done.stdout


def _git(argv: tuple[str, ...], *, root: UPath) -> Result[tuple[str, ...], Fault]:
    """Run one git/fd probe at the marked spawn boundary: OSError/Timeout/non-zero → ``Fault`` value.

    The sync façade calls ``anyio.run`` exactly once; the try/except is the lone exception seam this
    module owns. Empty output entries are dropped so a trailing newline never yields a phantom ``""``
    row.
    """
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
    """Sort-dedupe a path bag into the canonical root-relative POSIX shape (the ``Routed.files`` invariant)."""
    return tuple(sorted({PurePosixPath(p).as_posix() for p in paths}))


def _owner(rel: str, index: ProjectIndex) -> str | None:
    """Project the owning ``.csproj`` of a changed file: the deepest index dir that is an ancestor.

    Returns ``None`` when no project claims the file, so an orphan ``.cs`` edit contributes no closure
    seed rather than a phantom.
    """
    rel_path = PurePosixPath(rel)
    owners = tuple(d for d in index if rel_path.is_relative_to(d))
    return max(owners, key=str.__len__, default=None)  # deepest ancestor dir; str.__len__ keeps the key result str-typed


def _refs(rel: str, source: Source) -> frozenset[str]:
    """The root-relative ``ProjectReference`` set of one ``.csproj`` (one project-graph node).

    Parses XML through ``source.read`` so the graph is filesystem-blind; each ``Include`` is resolved
    relative to the project's own directory and normalized to a root-relative POSIX key matching
    ``index.values()``. A read fault or unparseable XML yields ``frozenset()`` — an isolated node,
    never a raised exception — keeping ``_dependents`` total. Memoization is the per-run ``graph`` dict
    in ``_dependents``, not a process-static ``@cache`` that would bleed across runs of a mutated
    worktree.
    """
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
    """Parse ``.csproj`` bytes into an element root; malformed XML degrades to an empty node (no raise)."""
    try:
        return ET.fromstring(raw)  # noqa: S314  # trusted local .csproj XML from source.read, never network-sourced
    except ET.ParseError:
        return ET.Element("Project")


def _dependents(seeds: frozenset[str], index: ProjectIndex, source: Source) -> frozenset[str]:
    """Reverse-reachability fixpoint in pure ``frozenset`` set-algebra: the affected-dependents set.

    Grows ``seeds`` by every project whose ``ProjectReference`` set intersects the current closure
    (changing ``P`` rebuilds everything referencing ``P``), iterated ``len(graph)`` times — the exact
    worst-case longest reverse-reachability chain in a DAG. Each pass is monotone-growing under union,
    so the closure is reached no later than the final iteration and re-running a settled set is
    idempotent; the bound is what keeps an early-stop ``while`` (imperative branching) out of the rail.
    """
    graph = {rel: _refs(rel, source) for rel in index.values()}
    return reduce(
        lambda current, _: current | frozenset(p for p, refs in graph.items() if p not in current and bool(current & refs)), range(len(graph)), seeds
    )


def _escalate(files: tuple[str, ...], settings: AssaySettings | None) -> tuple[str, ...]:
    """The change-set rows that escalate scope to FULL: trigger files or analyzer-tree edits.

    Reads ``settings.trigger_files``/``settings.trigger_prefixes`` (the Tier-A escalation vocabulary)
    when ``settings`` is provided, else falls back to the ``_TRIGGER_FILES``/``_TRIGGER_PREFIXES``
    module constants — so a settings-free ``route(...)`` (and the property tests) keep the canonical
    vocabulary while a rail-supplied ``settings`` lets the operator widen the escalation set via env.
    """
    trigger_files = settings.trigger_files if settings is not None else _TRIGGER_FILES
    trigger_prefixes = settings.trigger_prefixes if settings is not None else _TRIGGER_PREFIXES
    return tuple(f for f in files if f in trigger_files or any(f.startswith(prefix) for prefix in trigger_prefixes))


def _glob(language: Language, files: tuple[str, ...]) -> Result[Routed, Fault]:
    """The glob arm: suffix-filter the change-set by ``language.suffixes``, leaving ``projects``/``groups`` ``()``.

    Always ``Scope.CHANGED``: FULL-trigger escalation is a ``CLOSURE``-arm concern feeding ``place``'s
    dotnet-only ``SOLUTION`` projection. A glob language never routes to ``SOLUTION``, so a C#
    trigger-file edit is simply not a ``Python``/``TypeScript``/``Bash``/``SQL``/``Docs`` row and drops
    out of the suffix filter rather than escalating the scope.
    """
    return Ok(Routed(language=language, scope=Scope.CHANGED, files=_norm(tuple(f for f in files if PurePosixPath(f).suffix in language.suffixes))))


def _closure(language: Language, files: tuple[str, ...], source: Source, settings: AssaySettings | None) -> Result[Routed, Fault]:
    """The C# closure arm: escalate-or-resolve the owning-project reverse-dependency closure.

    A FULL escalation emits ``Routed(scope=FULL, full_triggers=…)`` and skips the index touch and
    ``_dependents`` entirely (``place`` then reads ``SOLUTION``). Otherwise it discovers the whole
    ``.csproj`` graph via ``source.enumerate``, maps each changed file to its owning project for the
    dependents seeds, grows the bounded fixpoint, and projects ``groups`` (owner → its changed files)
    for the ``INCLUDE`` arm and ``projects`` (sorted closure) for the ``PROJECT`` arm. ``settings``
    threads the escalation vocabulary into ``_escalate`` (``None`` falls back to the module constants).
    """
    triggers = _escalate(files, settings)
    match triggers:
        case ():
            return source.enumerate((".",)).bind(lambda all_files: _resolve(language, files, all_files, source))
        case _:
            return Ok(Routed(language=language, scope=Scope.FULL, files=_norm(files), full_triggers=_norm(triggers)))


def _resolve(language: Language, changed: tuple[str, ...], universe: tuple[str, ...], source: Source) -> Result[Routed, Fault]:
    """Build the project index from the universe, seed from changed files, grow the dependents closure.

    ``index`` keys every project by its owning directory so ``_owner`` can match the deepest ancestor;
    ``groups`` pairs each seed owner with the changed files it owns for the ``dotnet format --include``
    projection.
    """
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

    Discriminates ``paths`` (explicit → ``source.enumerate``; empty → ``source.changed``), then binds
    the resolved rows into the strategy arm keyed by ``language.strategy``. ``Language.strategy`` is the
    dispatch key, never the ``Language`` identity, so adding ``BASH``/``SQL`` adds no arm. The injected
    ``Source`` defaults to the git+``fd``+``pathlib`` ``LOCAL`` binding; a sixth filesystem is a new
    ``Source``. ``settings`` (already held by every rail) threads the Tier-A escalation vocabulary into
    the ``CLOSURE`` arm; ``None`` falls back to the ``_TRIGGER_FILES``/``_TRIGGER_PREFIXES`` constants.
    """
    src = source if source is not None else LOCAL
    enumerated = src.enumerate(paths) if paths else src.changed()
    return enumerated.bind(lambda files: _closure(language, files, src, settings) if language.strategy == "closure" else _glob(language, files))


def _globs(routed: Routed) -> tuple[str, ...]:
    """The GLOB arm's suffix-glob projection: the distinct ``**/*<suffix>`` patterns of the routed language."""
    return tuple(sorted({f"**/*{suffix}" for suffix in routed.language.suffixes}))


def place(routed: Routed, tool: Tool, *, settings: AssaySettings) -> tuple[tuple[str, ...], ...]:  # noqa: PLR0912  # six-arm exhaustive Input match + assert_never is the canonical total projection
    """The sole argv-tail projector: a total projection keyed by ``Input``, never by ``Language``.

    Statement-form ``match`` (never ``return match``) over the six ``Input`` members; ``settings`` is
    needed only for the ``SOLUTION`` path. ``assert_never`` closes the exhaustive match, so a seventh
    ``Input`` member is a type error here rather than a silent empty tail.
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
        case Input.GLOB:
            return ((*_globs(routed),),)
        case Input.NONE:
            return ((),)
        case never:  # pragma: no cover
            assert_never(never)


# --- [COMPOSITION] ----------------------------------------------------------------------


class _LocalSource:
    """The default ``Source``: git change-set + ``fd`` enumeration + ``UPath`` read, rooted at cwd.

    ``changed`` is the sorted-deduped union of three git probes (tracked working-tree Δ, staged Δ,
    untracked-but-not-ignored); ``enumerate`` expands a dir via ``fd`` (recursive file scan minus build
    trees), passes a file verbatim, and faults on a missing path. All I/O rides the marked ``_git``
    boundary or a single ``UPath.read_bytes`` whose ``OSError`` becomes a ``Fault`` value, never a
    raised exception. ``_root`` is a ``UPath``: a plain ``AssaySettings().root`` stays a local
    ``PosixUPath`` (byte-identical to ``Path``); a ``memory://``/remote root reads the change-set
    unchanged with zero local IO.
    """

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
    """Expand one explicit ``paths`` entry: a dir → ``fd`` recursive scan, a file → verbatim, missing → ``Fault``.

    The ``is_dir``/``is_file`` probe is the only place ``LOCAL`` touches the concrete filesystem for
    shape (not contents); contents still flow through the marked ``_git`` boundary. ``root`` is a
    ``UPath`` so the shape probe targets the same backend the change-set reads.
    """
    absolute = root / target
    match (absolute.is_dir(), absolute.is_file()):
        case (True, _):
            return _git((*_FD, target, *_FD_EXCLUDE), root=root).map(lambda rows: tuple(PurePosixPath(r).as_posix() for r in rows))
        case (_, True):
            return Ok((PurePosixPath(target).as_posix(),))
        case _:
            return Error(Fault(("enumerate", target), RailStatus.FAULTED, f"path not found: {target}"))


LOCAL: Source = _LocalSource()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LOCAL", "ProjectIndex", "RoutePaths", "Routed", "Scope", "Source", "place", "route"]
