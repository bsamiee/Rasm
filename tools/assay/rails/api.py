"""The C#-only ``api`` rail: one ilspycmd surface, one fingerprint cache, one shape fold.

Owns the four read-only host/NuGet metadata verbs ``doctor | resolve | query | show`` over a single
hand-shaped ``ApiSurface``. One ilspycmd catalog row drives both ``-l cisde`` (surface) and
``-t <type> --no-dead-code --no-dead-stores`` (decompile); the per-call assembly tail splices onto
``tool.command`` via ``msgspec.structs.replace`` so the engine projector stays generic. The
index/namespace/type/member/search distinction is computed once from the symbol-token shape
(``shape_of``), never from verb proliferation.

Read-only throughout — no ``dotnet build``, no mutation, no exclusive lease. A non-zero ilspycmd exit
rides ``Completed(FAILED)``; only a spawn failure or ``doctor --strict`` promotion takes the ``Error``
channel. The cache fingerprint hashes assembly *content* (size + mtime_ns + SHA-256 of the bytes), not
bare mtime: a RhinoWIP reinstall preserves mtime on unchanged DLLs, so a content hash is the
correctness boundary against a stale surface silently misreporting ``[Obsolete]`` markers.
"""

import annotationlib  # PEP 749 (3.14): get_annotations(format=STRING) surfaces a signature WITHOUT evaluating unresolvable forward refs
from dataclasses import dataclass
import hashlib
import importlib
import importlib.metadata
import inspect
from pathlib import Path
import re
from typing import assert_never, TYPE_CHECKING
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local Directory.Packages.props XML, never network-sourced

from expression import Error, Ok, Result
import msgspec
from tree_sitter import Language as TSLanguage, Parser as TSParser, Query as TSQuery, QueryCursor
import tree_sitter_typescript  # the SAME grammar binding code.py parses .d.ts with; never a second import surface

from tools.assay.composition.catalog import Capture, CAPTURE_ENCODER, CAPTURES, select  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # registry passes both at runtime
from tools.assay.core.engine import run_check  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    ApiResolution,
    ApiSurface,
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,
    Fault,
    fold,
    InprocThunk,  # noqa: TC001  # unconditional so beartype @checked resolves the thunk-builder return forward-ref under PEP 649
    Language,
    Match,
    Mode,
    receipt,
    Report,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
    SourceKind,
    SymbolShape,
    Tool,
)
from tools.assay.core.routing import Routed, Scope  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from collections.abc import Callable

    from tree_sitter import Node


# --- [TYPES] ----------------------------------------------------------------------------

type _PathKind = str  # resolve kind token: all | assembly | xml | nuspec | deps | package-root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ApiParams(BaseParams):
    """The ``api`` per-verb CLI params: one frozen dataclass covering all four verbs.

    ``key``/``symbol`` drive ``query``; ``key``/``kind`` drive ``resolve``; ``token``/``latest``/
    ``lines`` drive ``show``; ``strict`` drives ``doctor``'s ``EMPTY``→``FAULTED`` promotion.
    ``max_lines``/``full``/``grep`` bound the decompile/preview window; ``restore`` forces a
    ``dotnet tool restore`` of ilspycmd.
    """

    key: str = "rhino-common"
    symbol: str = ""
    kind: _PathKind = "all"
    token: str = ""
    max_lines: int = 120
    lines: str = ""
    grep: str = ""
    full: bool = False
    latest: bool = False
    restore: bool = False
    strict: bool = False


@dataclass(frozen=True, slots=True)
class _Source:
    """The resolved metadata origin: typed provenance plus the on-disk asset projection.

    A host assembly (``SourceKind.ASSEMBLY``) carries a Rhino-bundle ``.dll``/``.xml`` pair; a NuGet
    package (``SourceKind.NUGET``) adds a restored package root, ``.nuspec``, and asset tree. Every
    downstream verb reads this typed shape; ``ApiSurface`` mints its ``source_kind``/``source_id``/
    ``version`` directly off it.
    """

    key: str
    kind: SourceKind
    version: str = ""
    assemblies: tuple[Path, ...] = ()
    xmls: tuple[Path, ...] = ()
    nuspec: Path | None = None
    package_root: Path | None = None
    asset_paths: tuple[Path, ...] = ()


@dataclass(frozen=True, slots=True)
class _Surface:
    """The parsed ilspycmd ``-l cisde`` roster: the INDEX/NAMESPACE/SEARCH evidence.

    ``types`` is the fully-qualified type set; ``namespaces`` the distinct owning namespaces;
    ``by_namespace`` the namespace→types index the NAMESPACE shape lists; ``cache`` the
    content-addressed ``<key>.<fingerprint>.txt`` artifact the surface decoded from.
    """

    source: _Source
    types: tuple[str, ...]
    namespaces: tuple[str, ...]
    by_namespace: dict[str, tuple[str, ...]]
    cache: Path
    raw: str


@dataclass(frozen=True, slots=True)
class _Body:
    """The decompiled type/member projection: anchored signature, xml doc, bounded window.

    ``signature`` anchors on the modifier-prefixed declaration line, never a ``///`` doc-comment line
    (a bare word boundary on the simple name over-matches the doc-comment occurrence and returns a
    sibling overload). ``window`` is the ``--max-lines`` slice of the grep-filtered body; ``full`` the
    whole decompiled text the ``Artifact`` carries on truncation.
    """

    signature: str
    xml: str
    window: str
    full: str
    selected: int
    truncated: bool


# --- [CONSTANTS] ------------------------------------------------------------------------

_HOST_SPECS: dict[str, tuple[str, str]] = {
    "eto": ("Eto.dll", "Eto.xml"),
    "gh2": ("ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml"),
    "gh2-io": ("ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml"),
    "rhino-code": ("Rhino.Runtime.Code.dll", ""),
    "rhino-common": ("RhinoCommon.dll", "RhinoCommon.xml"),
    "rhino-ui": ("Rhino.UI.dll", "Rhino.UI.xml"),
}
_RHINO_BUNDLES: tuple[str, ...] = ("/Applications/RhinoWIP.app", "/Applications/Rhino 8.app")
_RESOURCE_ROOT: str = "Contents/Frameworks/RhCore.framework/Versions/Current/Resources"
_PACKAGES_PROPS: str = "Directory.Packages.props"
_NUGET_ROOTS: tuple[str, ...] = (".cache/nuget/packages", str(Path.home() / ".nuget/packages"))
_FRAMEWORK_RANK: tuple[str, ...] = ("net10.0", "net9.0", "net8.0", "net7.0", "net6.0", "netstandard2.1", "netstandard2.0")
_ASSET_DIRS: tuple[str, ...] = ("lib", "ref", "runtimes", "build", "buildTransitive", "analyzers", "tools")
_SURFACE_KINDS: frozenset[str] = frozenset(("Class", "Struct", "Interface", "Delegate", "Enum"))
_PREVIEW_ROWS: int = 12  # inline preview row cap; the full listing rides the surface Artifact
_RESULT_CAP: int = 1000  # Report.results bound; over-cap sets truncated and the full set rides the cache
_CANDIDATE_CAP: int = 8  # ApiResolution.candidates bound on a miss: the top-N nearest matches an agent needs to retry
_DEPS_PARTS: frozenset[str] = frozenset(("build", "buildTransitive", "analyzers", "tools", "runtimes"))
_DECOMPILE_FLAGS: tuple[str, ...] = ("--no-dead-code", "--no-dead-stores")
_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")  # content-addressable wire order for report artifacts
_NODE_MODULES: str = "node_modules"
_PNPM_STORE: str = "node_modules/.pnpm"  # pnpm store: <mangled>@<ver>[+peer]/node_modules/<pkg>; @scope/pkg → @scope+pkg in dir name
_DTS_GLOB: str = "*.d.ts"
_DTS_ENTRY: str = "index.d.ts"  # entrypoint fallback when package.json declares no types/typings
_TS_DECL_QUERY: str = (  # the .d.ts roster S-expr: every exported declaration name → one @type capture (INDEX/NAMESPACE/SEARCH)
    "(class_declaration name: (type_identifier) @type)"
    " (abstract_class_declaration name: (type_identifier) @type)"
    " (interface_declaration name: (type_identifier) @type)"
    " (type_alias_declaration name: (type_identifier) @type)"
    " (enum_declaration name: (identifier) @type)"
    " (function_signature name: (identifier) @type)"
    " (module name: (identifier) @type)"
)
_TS_GRAMMAR: Callable[[], object] = tree_sitter_typescript.language_typescript  # shared with code.py's _GRAMMARS[Language.TYPESCRIPT]
_DECL_NODES: frozenset[str] = frozenset(  # the .d.ts declaration node kinds a member lookup anchors a full signature on
    (
        "class_declaration",
        "abstract_class_declaration",
        "interface_declaration",
        "type_alias_declaration",
        "enum_declaration",
        "function_signature",
        "method_signature",
        "property_signature",
        "public_field_definition",
    )
)
_INSPECT_KINDS: tuple[tuple[str, Callable[[object], bool]], ...] = (  # the pydist roster predicates: one @type capture per class/function/submodule
    ("type", inspect.isclass),
    ("type", inspect.isfunction),
)
_NAME_CAP: int = 320  # per-capture text bound, mirroring code._TEXT_CAP
_SIG_CAP: int = 480  # signature/doc slice bound, mirroring _xml_doc's 480


# --- [OPERATIONS] -----------------------------------------------------------------------


def shape_of(symbol: str) -> SymbolShape:
    """Discriminate the request token into one ``SymbolShape``, computed once to drive one ``match``.

    Empty token → INDEX roster; dotless → NAMESPACE; a dotted token whose final segment begins
    uppercase with no call syntax → TYPE; everything else (lowercase tail, parens) → MEMBER. SEARCH is
    never produced here — it is the decompile-miss fallback the ``query`` fold derives, keeping this
    projection total over the request grammar without a sixth arm.
    """
    match symbol.strip():
        case "":
            return SymbolShape.INDEX
        case s if "." not in s:
            return SymbolShape.NAMESPACE
        case s if s.rsplit(".", 1)[-1][:1].isupper() and "(" not in s:
            return SymbolShape.TYPE
        case _:
            return SymbolShape.MEMBER


def _safe(key: str) -> str:
    """Project a source key onto the filesystem-safe ``<safe-key>`` half of the cache name."""
    return re.sub(r"[^A-Za-z0-9_.-]+", "-", key).strip("-") or "source"


def _fingerprint(assemblies: tuple[Path, ...]) -> str:
    """Content-address the assembly set: hash size + mtime_ns + bytes, never bare mtime.

    A RhinoWIP reinstall preserves mtime on unchanged DLLs, so the SHA-256 of the actual bytes is
    folded in alongside the cheap ``(size, mtime_ns)`` discriminant — the digest changes iff the
    decompiled surface would, making the cache the correctness boundary rather than a stale mirror.
    """
    seed = "|".join(f"{p}:{p.stat().st_size}:{p.stat().st_mtime_ns}:{hashlib.sha256(p.read_bytes()).hexdigest()}" for p in assemblies if p.is_file())
    return hashlib.sha256(seed.encode()).hexdigest()[:16]


def _rhino_app(settings: AssaySettings) -> Path | None:
    """Resolve the Rhino bundle: a worktree-root ``rhino-app`` wins, else the first extant macOS install.

    The repo-rooted ``rhino-app`` lets a CI runner pin a vendored/symlinked bundle without env. A
    missing bundle projects to ``None`` (sentinel → ``Option``-style absence): ``doctor`` reports a
    ``missing`` inventory row and ``query``/``resolve`` fold ``EMPTY`` rather than a fault.
    """
    candidates = (Path(str(settings.root)) / "rhino-app", *(Path(b) for b in _RHINO_BUNDLES))  # bundle resolution is local-fs; UPath → Path
    return next((c for c in candidates if c.is_dir()), None)


def _host_source(settings: AssaySettings, key: str) -> _Source | None:
    """Build a host ``_Source`` (``SourceKind.ASSEMBLY``) from the Rhino bundle, or ``None`` when off-host.

    A key absent from ``_HOST_SPECS`` is not a host source (the caller falls through to NuGet); a
    present key with no resolvable bundle yields an assembly-empty ``_Source`` so ``doctor`` still
    reports the declared-but-missing inventory row.
    """
    match _HOST_SPECS.get(key):
        case None:
            return None
        case (asm_name, xml_name):
            app = _rhino_app(settings)
            resources = (app / _RESOURCE_ROOT) if app is not None else None
            assemblies = (resources / asm_name,) if resources is not None and asm_name else ()
            xmls = (resources / xml_name,) if resources is not None and xml_name else ()
            return _Source(
                key=key, kind=SourceKind.ASSEMBLY, assemblies=tuple(a for a in assemblies if a.is_file()), xmls=tuple(x for x in xmls if x.is_file())
            )


def _packages(settings: AssaySettings) -> dict[str, str]:
    """Parse the central ``Directory.Packages.props`` into an ``Include → Version`` map (NuGet truth)."""
    path = settings.root / _PACKAGES_PROPS
    match path.is_file():
        case False:
            return {}
        case True:
            try:
                root = ET.fromstring(path.read_bytes())  # noqa: S314  # trusted local Directory.Packages.props, never network-sourced
            except ET.ParseError:
                return {}
            return {
                inc: ver
                for node in root.iterfind(".//PackageVersion")
                for inc, ver in ((node.get("Include", ""), node.get("Version", "")),)
                if inc and ver
            }


def _candidates(names: tuple[str, ...], needle: str, *, n: int = _CANDIDATE_CAP) -> tuple[tuple[str, int], ...]:
    """Score a name bag against a needle into the top-N ``(name, score)`` agent-disambiguation rows.

    One shared scoring projection for every api miss (key + symbol): exact casefold (100) ▷ prefix (70)
    ▷ substring (40) ▷ shared-segment overlap (20·matched ÷ token count), with a shorter-name bonus
    breaking ties toward the closest match. Sorted descending, capped at ``n`` — the nearest hops an
    agent retries with, never the full bag.
    """
    casefold = needle.casefold()
    segments = frozenset(casefold.replace("-", ".").split("."))

    def score(name: str) -> int:
        low = name.casefold()
        tokens = frozenset(low.replace("-", ".").split("."))
        overlap = len(segments & tokens) * 20 // max(1, len(tokens))
        base = 100 if low == casefold else 70 if low.startswith(casefold) else 40 if casefold in low else overlap
        return base + max(0, 20 - len(name) // 4)

    ranked = sorted(((name, score(name)) for name in names), key=lambda row: (-row[1], row[0]))
    return tuple((name, sc) for name, sc in ranked[:n] if sc > 0)


def _resolve_key(packages: dict[str, str], key: str) -> Result[str, ApiResolution]:
    """Fuzzy-resolve a NuGet key (exact casefold → unique prefix → unique substring) to one package id.

    A unique hit takes the ``Ok`` channel; a miss takes the ``Error`` channel as a typed ``ApiResolution``
    carrying the top-N scored ``candidates`` plus a ``reason`` (``unknown`` for an empty match set,
    ``ambiguous`` for a multi-hit set) — the agent gets the nearest central pins and *why* the key did
    not resolve, never a bare string. ``languageext``/``avalonia.datagrid`` stay addressable without a verb.
    """
    casefold = key.casefold()
    exact = tuple(n for n in packages if n.casefold() == casefold)
    fuzzy = tuple(n for n in packages if n.casefold().startswith(casefold)) or tuple(n for n in packages if casefold in n.casefold())
    hits = exact or fuzzy
    match (bool(exact) or len(hits) == 1, len(hits)):
        case (True, _):
            return Ok(hits[0])
        case (_, 0):
            return Error(ApiResolution(candidates=_candidates(tuple(packages), key), reason="unknown"))
        case _:
            return Error(ApiResolution(candidates=_candidates(hits, key, n=len(hits)), reason="ambiguous"))


def _package_root(settings: AssaySettings, package: str, version: str) -> Path:
    """Locate the restored package root across the project-local and home NuGet caches (first extant wins)."""
    candidates = tuple(
        Path(root) if Path(root).is_absolute() else Path(str(settings.root)) / root for root in _NUGET_ROOTS
    )  # NuGet cache is local-fs; UPath → Path
    targets = tuple(base / package.casefold() / version for base in candidates)
    return next((t for t in targets if t.is_dir()), targets[0])


def _framework_dir(root: Path, asset: str) -> Path | None:
    """Pick the best target-framework dir under ``root/<asset>`` by the ``_FRAMEWORK_RANK`` preference."""
    base = root / asset
    frameworks = tuple(p for p in base.iterdir() if p.is_dir()) if base.is_dir() else ()
    ranked = (*(p for name in _FRAMEWORK_RANK for p in frameworks if p.name == name), *sorted(p for p in frameworks if p.name not in _FRAMEWORK_RANK))
    return next(iter(ranked), None)


def _nuget_source(settings: AssaySettings, package: str, version: str) -> _Source:
    """Build a NuGet ``_Source`` (``SourceKind.NUGET``) from the restored package root.

    Ordering puts the package-stem assembly first so its sidecar ``.xml`` wins doc lookup. An
    unrestored root yields an assembly-empty source so ``query`` folds ``EMPTY`` rather than faulting.
    """
    root = _package_root(settings, package, version)
    selected = _framework_dir(root, "ref") or _framework_dir(root, "lib")
    assemblies = tuple(sorted(selected.glob("*.dll"))) if selected is not None else ()
    primary = tuple(a for a in assemblies if a.stem.casefold() == package.casefold())
    ordered = (*primary, *(a for a in assemblies if a not in primary))
    xmls = tuple(a.with_suffix(".xml") for a in ordered if a.with_suffix(".xml").is_file())
    nuspec = next(iter(sorted(root.glob("*.nuspec"))), None) if root.is_dir() else None
    assets = tuple(sorted(p for d in _ASSET_DIRS for base in (root / d,) if base.is_dir() for p in base.rglob("*") if p.is_file()))
    return _Source(
        key=package,
        kind=SourceKind.NUGET,
        version=version,
        assemblies=ordered,
        xmls=xmls,
        nuspec=nuspec,
        package_root=root if root.is_dir() else None,
        asset_paths=assets,
    )


def _pydist_names() -> tuple[str, ...]:
    """The installed Python distribution roster (``importlib.metadata`` names) for doctor + cross-kind miss candidates."""
    return tuple(sorted({dist.metadata["Name"] for dist in importlib.metadata.distributions() if dist.metadata["Name"]}))


def _pydist_source(key: str) -> _Source | None:
    """Build a PYDIST ``_Source`` from ``importlib.metadata``, or ``None`` when the dist is not installed.

    ``asset_paths`` projects the dist's recorded files to absolute paths (``.files`` may be ``None`` →
    ``()``); ``package_root`` is the site-packages anchor (``locate_file('')``). No assemblies — the
    roster/decompile rides the INPROC ``inspect`` thunk, not ilspycmd.
    """
    try:
        dist = importlib.metadata.distribution(key)  # codec boundary: an uninstalled key raises PackageNotFoundError → None (fall through)
    except importlib.metadata.PackageNotFoundError:
        return None
    root = Path(str(dist.locate_file("")))
    match dist.files:
        case None:
            assets: tuple[Path, ...] = ()
        case files:
            assets = tuple(Path(str(dist.locate_file(f))) for f in files)
    return _Source(
        key=dist.metadata["Name"] or key,
        kind=SourceKind.PYDIST,
        version=dist.version or "",
        package_root=root if root.is_dir() else None,
        asset_paths=assets,
    )


def _pydist_modules(key: str) -> tuple[str, ...]:
    """The dist's import-root module names: ``top_level.txt`` first, else the ``packages_distributions`` reverse map.

    ``top_level.txt`` is the declared truth but often absent (modern wheels omit it); the fallback inverts
    ``packages_distributions()`` (module → dists) to recover the real import roots — e.g. ``Pygments`` →
    ``pygments`` — where the casefolded key alone would miss the case-fold. A final ``key`` normalization
    seeds a single candidate when both metadata sources are silent.
    """
    try:
        text = importlib.metadata.distribution(key).read_text("top_level.txt")
    except importlib.metadata.PackageNotFoundError:
        return ()
    declared = tuple(line.strip() for line in (text or "").splitlines() if line.strip())
    casefold = key.casefold()
    mapped = tuple(module for module, dists in importlib.metadata.packages_distributions().items() if any(d.casefold() == casefold for d in dists))
    return declared or mapped or (key.replace("-", "_"),)


def _tsdecl_names(settings: AssaySettings) -> tuple[str, ...]:
    """The npm package roster under the worktree ``node_modules`` (hoisted + scoped), bounded for doctor + miss candidates."""
    base = Path(str(settings.root)) / _NODE_MODULES
    match base.is_dir():
        case False:
            return ()
        case True:
            top = tuple(d.name for d in base.iterdir() if d.is_dir() and not d.name.startswith((".", "@")))
            scoped = tuple(
                f"{scope.name}/{pkg.name}"
                for scope in base.iterdir()
                if scope.is_dir() and scope.name.startswith("@")
                for pkg in scope.iterdir()
                if pkg.is_dir()
            )
            return tuple(sorted((*top, *scoped)))


def _tsdecl_dir(settings: AssaySettings, key: str) -> Path | None:
    """Resolve the ``node_modules/<pkg>`` dir: hoisted first, else the pnpm store (``@scope/pkg`` → ``@scope+pkg``)."""
    base = Path(str(settings.root)) / _NODE_MODULES
    hoisted = base / key
    mangled = key.replace("/", "+")
    pnpm = tuple(sorted((Path(str(settings.root)) / _PNPM_STORE).glob(f"{mangled}@*/{_NODE_MODULES}/{key}")))
    candidates = (hoisted, *pnpm)
    return next((c for c in candidates if (c / "package.json").is_file() or any(c.glob(_DTS_GLOB))), None)


def _tsdecl_source(settings: AssaySettings, key: str) -> _Source | None:
    """Build a TSDECL ``_Source`` from a resolved ``node_modules`` package, or ``None`` when unresolvable.

    The entrypoint prefers ``package.json`` ``types``/``typings`` else ``index.d.ts``; ``asset_paths``
    is the entrypoint plus its sibling ``.d.ts`` set (one-level glob). A package with NO ``.d.ts`` folds
    to ``asset_paths=()`` so ``_surface`` yields EMPTY (exit 0) — never FAULTED.
    """
    match _tsdecl_dir(settings, key):
        case None:
            return None
        case pkg_dir:
            version = _tsdecl_version(settings, key)
            entry = _tsdecl_entry(pkg_dir)
            siblings = tuple(sorted((entry.parent if entry is not None else pkg_dir).glob(_DTS_GLOB)))
            assets = tuple(dict.fromkeys((*((entry,) if entry is not None else ()), *siblings)))
            return _Source(key=key, kind=SourceKind.TSDECL, version=version, package_root=pkg_dir, asset_paths=assets)


def _tsdecl_entry(pkg_dir: Path) -> Path | None:
    """Project the package's typings entrypoint: ``package.json`` ``types``/``typings`` else ``index.d.ts`` (extant only)."""
    manifest = pkg_dir / "package.json"
    declared = _json_field(manifest, "types") or _json_field(manifest, "typings")
    target = (pkg_dir / declared) if declared else (pkg_dir / _DTS_ENTRY)
    return target if target.is_file() else None


def _tsdecl_version(settings: AssaySettings, key: str) -> str:
    """Parse the package version off the pnpm store dir tail (``<pkg>@<ver>[+peer]``), else the hoisted ``package.json``."""
    mangled = key.replace("/", "+")
    pnpm = tuple(sorted((Path(str(settings.root)) / _PNPM_STORE).glob(f"{mangled}@*")))
    tail = next((d.name.removeprefix(f"{mangled}@").split("+", 1)[0].split("_", 1)[0] for d in pnpm), "")
    return tail or _json_field((Path(str(settings.root)) / _NODE_MODULES / key) / "package.json", "version")


def _json_field(path: Path, field: str) -> str:
    """Read one top-level string field from a JSON manifest at the marked codec boundary (missing/malformed/non-string → ``""``)."""
    try:
        data = msgspec.json.decode(path.read_bytes(), type=dict[str, msgspec.Raw])  # codec boundary: a missing/malformed package.json folds to ""
        return msgspec.json.decode(data[field], type=str) if field in data else ""  # nested codec boundary: a non-string value folds to ""
    except OSError, KeyError, msgspec.DecodeError:
        return ""


def _source(settings: AssaySettings, key: str) -> Result[_Source, ApiResolution]:
    """Resolve one source key to its typed ``_Source`` across all kinds: host bundle ▷ NuGet ▷ PYDIST ▷ TSDECL.

    An ordered resolver fold (cheapest, side-effect-free origins first): a host bundle, then a fuzzy
    central NuGet pin, then an installed Python distribution, then an npm ``.d.ts`` package. The first
    non-``None`` resolver wins; only the NuGet arm governs fuzzy resolution, so its miss reason rides
    through. A total miss rides the ``Error`` channel as a typed ``ApiResolution`` whose ``candidates``
    aggregate across NuGet + PyPI + npm names — the richer-on-failure contract generalizes: an agent sees
    the nearest dist/package, not just a NuGet pin, never a bare empty result or a broken-operation 2.
    """
    packages = _packages(settings)
    resolvers: tuple[Callable[[], _Source | None], ...] = (
        lambda: _host_source(settings, key),
        lambda: _resolve_key(packages, key).map(lambda package: _nuget_source(settings, package, packages[package])).default_value(None),
        lambda: _pydist_source(key),
        lambda: _tsdecl_source(settings, key),
    )
    match next((source for resolver in resolvers if (source := resolver()) is not None), None):
        case _Source() as source:
            return Ok(source)
        case None:
            names = (*tuple(packages), *_pydist_names(), *_tsdecl_names(settings))
            return Error(ApiResolution(candidates=_candidates(names, key), reason="unknown"))


def _module_members(module: object, prefix: str, *, depth: int = 1) -> tuple[Capture, ...]:
    """Roster a module's classes/functions into ``@type`` captures, descending one level into same-prefix submodules.

    ``depth`` gates the single recursion into same-prefix submodules (``1`` at the import root, ``0`` inside
    a submodule), so the roster never walks the transitive module graph — one level is the roster breadth.
    """
    name = getattr(module, "__name__", prefix)
    file = str(getattr(module, "__file__", "") or "")
    own = tuple(
        Capture(name=cap, text=f"{name}.{ident}"[:_NAME_CAP], file=file, line=0)
        for cap, predicate in _INSPECT_KINDS
        for ident, obj in inspect.getmembers(module, predicate)
        if not ident.startswith("_") and getattr(obj, "__module__", prefix).startswith(prefix)
    )
    submodules = (
        tuple(
            obj
            for ident, obj in inspect.getmembers(module, inspect.ismodule)
            if getattr(obj, "__name__", "").startswith(f"{name}.") and not ident.startswith("_")
        )
        if depth > 0
        else ()
    )
    return (*own, *(cap for sub in submodules for cap in _module_members(sub, prefix, depth=depth - 1)))


def _import(name: str) -> object | None:
    """Import one module at the marked spawn boundary; an unimportable module (C-ext / missing dep) degrades to ``None`` (never raises)."""
    try:
        return importlib.import_module(name)  # spawn boundary: an unresolvable transitive dep degrades to metadata-only, never crashes
    except Exception:  # noqa: BLE001  # INPROC defensive: per-module import fault → drop that module, the thunk still surfaces the rest
        return None


def _pydist_thunk(key: str, symbol: str) -> InprocThunk:
    """The PYDIST surface/member thunk (symmetric with ``_tsdecl_thunk``): empty ``symbol`` rosters ``@type`` captures, else member captures.

    Empty ``symbol`` imports each top-level module (defensively via ``_import`` → ``None`` on a
    C-ext/missing-dep fault) and rosters its classes/functions into ``@type`` captures. A non-empty
    ``symbol`` resolves it via import + ``getattr`` walk and emits ``@signature``/``@doc``/``@full``
    captures (``inspect``/PEP 749 ``annotationlib``). Captures ride ``CAPTURE_ENCODER`` — the identical
    wire code.py's ``_ts_thunk`` rides, so the api fold decodes them through the same ``CAPTURES`` codec.
    """

    def run(_check: Check) -> Completed:
        match symbol:
            case "":
                modules = tuple((name, _import(name)) for name in _pydist_modules(key))
                captures = tuple(cap for name, module in modules if module is not None for cap in _module_members(module, name))
                return receipt(("py-api", "surface", key), 0, stdout=CAPTURE_ENCODER.encode(captures))
            case _:
                obj = _resolve_symbol(key, symbol)
                member = _member_captures(obj, symbol) if obj is not None else ()
                return receipt(("py-api", "member", key, symbol), 0, stdout=CAPTURE_ENCODER.encode(member))

    return run


def _inproc_thunk(source: _Source, symbol: str) -> InprocThunk:
    """Pick the PYDIST/TSDECL surface (empty ``symbol``) or member thunk — the sole kind→thunk dispatch both INPROC arms share."""
    return _pydist_thunk(source.key, symbol) if source.kind is SourceKind.PYDIST else _tsdecl_thunk(source, symbol)


def _resolve_symbol(key: str, symbol: str) -> object | None:
    """Resolve ``symbol`` to its object: import the longest dotted module prefix under a dist root, then ``getattr``-walk the tail.

    ``msgspec.json.Decoder`` imports ``msgspec.json`` then ``getattr``s ``Decoder``; ``msgspec.Struct``
    imports ``msgspec`` then ``getattr``s ``Struct``; a bare ``Struct`` walks off each top-level root. Each
    candidate qualified name is rooted at a ``top_level.txt`` module, so an arbitrary token never imports an
    unrelated installed package; the longest importable module prefix wins (``range`` descends).
    """
    roots = _pydist_modules(key)
    qualified = tuple(dict.fromkeys((symbol, *(f"{root}.{symbol}" for root in roots if not symbol.startswith(f"{root}.") and symbol != root))))
    return next((resolved for qname in qualified for resolved in (_resolve_qualified(qname, roots),) if resolved is not None), None)


def _resolve_qualified(qname: str, roots: tuple[str, ...]) -> object | None:
    """Import the longest dist-rooted dotted prefix of ``qname`` as a module, then ``getattr``-walk the remaining tail."""
    parts = tuple(qname.split("."))
    splits = tuple((parts[:i], parts[i:]) for i in range(len(parts), 0, -1) if parts[0] in roots)
    return next(
        (
            resolved
            for head, tail in splits
            for module in (_import(".".join(head)),)
            if module is not None
            for resolved in (_getattr_walk(module, tail),)
            if resolved is not None
        ),
        None,
    )


def _getattr_walk(root: object, parts: tuple[str, ...]) -> object | None:
    """Fold ``getattr`` over the dotted segments, short-circuiting to ``None`` on the first missing attribute."""
    match parts:
        case (head, *tail):
            attr = getattr(root, head, None)
            return _getattr_walk(attr, tuple(tail)) if attr is not None else None
        case _:
            return root


def _member_captures(obj: object, symbol: str) -> tuple[Capture, ...]:
    """Project a resolved object into the ``@signature``/``@doc``/``@full`` captures the member decode reads."""
    return (
        Capture(name="signature", text=f"{symbol}{_signature(obj)}"[:_SIG_CAP], file=str(getattr(obj, "__module__", "")), line=0),
        Capture(name="doc", text=(inspect.getdoc(obj) or "")[:_SIG_CAP], file="", line=0),
        Capture(name="full", text=_object_source(obj)[: _NAME_CAP * 8], file="", line=0),
    )


def _signature(obj: object) -> str:
    """Project a callable's signature with stringized annotations, falling back to PEP 749 ``annotationlib`` on a C-builtin reject.

    ``inspect.signature(eval_str=False)`` keeps annotations as their source strings so an unresolvable
    forward ref never evaluates. When ``inspect`` itself rejects the object (a C builtin / a default it
    cannot repr), ``annotationlib.get_annotations(format=STRING)`` (PEP 749) still surfaces the
    parameter→type map without evaluation — the verified lever for a dist with unresolvable deps; a final
    annotation fault folds to ``(...)``.
    """
    try:
        return str(inspect.signature(obj, eval_str=False))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]  # any resolved symbol; non-callable → TypeError arm
    except ValueError, TypeError:
        annotations = _annotations(obj)
        params = tuple(f"{name}: {kind}" for name, kind in annotations.items() if name != "return")
        return f"({', '.join(params)})" if params else "(...)"


def _annotations(obj: object) -> dict[str, str]:
    """PEP 749 stringized annotations at the marked boundary; a C-builtin / unresolvable object folds to ``{}`` (no raise)."""
    try:
        return annotationlib.get_annotations(obj, format=annotationlib.Format.STRING)
    except TypeError, NameError:
        return {}


def _object_source(obj: object) -> str:
    """Read an object's source slice at the marked codec boundary; a C-builtin / unreadable object folds to ``""`` (no raise)."""
    try:
        return inspect.getsource(obj)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]  # any resolved symbol; a C-builtin / unreadable object → TypeError arm
    except OSError, TypeError:
        return ""


def _tsdecl_thunk(source: _Source, symbol: str) -> InprocThunk:
    """The TSDECL surface/member thunk: tree-sitter-parse each ``.d.ts`` and emit ``@type`` roster or a member signature.

    Empty ``symbol`` runs the roster query (``_TS_DECL_QUERY``) over every ``.d.ts`` → one ``@type``
    capture per exported declaration name (INDEX/NAMESPACE/SEARCH). A non-empty ``symbol`` anchors the
    matching declaration node and emits one ``@signature`` capture (TYPE/MEMBER). REUSES code.py's TS
    grammar binding; captures ride the same ``CAPTURE_ENCODER`` wire — the api fold decodes them identically.
    """

    def run(_check: Check) -> Completed:
        ts_lang = TSLanguage(_TS_GRAMMAR())
        parser = TSParser(ts_lang)
        captures = tuple(cap for path in source.asset_paths for cap in _ts_captures(parser, ts_lang, path, symbol))
        return receipt(("ts-api", "member" if symbol else "surface", source.key, symbol), 0, stdout=CAPTURE_ENCODER.encode(captures))

    return run


def _ts_captures(parser: TSParser, language: TSLanguage, path: Path, symbol: str) -> tuple[Capture, ...]:
    """Read+parse one ``.d.ts`` (unreadable → ``()``): the roster when ``symbol`` is empty, else the matched declaration's signature."""
    try:
        src = path.read_bytes()  # FS boundary: an unreadable .d.ts drops out of the roster, never faults the thunk
    except OSError:
        return ()
    root = parser.parse(src).root_node
    match symbol:
        case "":
            query = TSQuery(language, _TS_DECL_QUERY)
            return tuple(
                Capture(name="type", text=_node_text(node)[:_NAME_CAP], file=path.name, line=node.start_point.row + 1)
                for nodes in QueryCursor(query).captures(root).values()
                for node in nodes
            )
        case _:
            return _ts_member(root, symbol, path)


def _ts_member(root: Node, symbol: str, path: Path) -> tuple[Capture, ...]:
    """Anchor the named declaration node (``name:`` field == the symbol tail) and emit its ``@signature`` capture; first match wins."""
    target = symbol.rsplit(".", 1)[-1]
    match next((n for n in _walk_decls(root) if (name := n.child_by_field_name("name")) is not None and _node_text(name) == target), None):
        case None:
            return ()
        case node:
            return (Capture(name="signature", text=_node_text(node)[:_SIG_CAP], file=path.name, line=node.start_point.row + 1),)


def _walk_decls(node: Node) -> tuple[Node, ...]:
    """Flatten the declaration nodes (``_DECL_NODES`` kinds) reachable from ``node`` via a depth-first child fold."""
    own = (node,) if node.type in _DECL_NODES else ()
    return (*own, *(d for child in node.children for d in _walk_decls(child)))


def _node_text(node: Node) -> str:
    """Decode a tree-sitter node's source slice (``Node.text`` is ``bytes | None``; an absent source folds to ``""``)."""
    raw = node.text
    return raw.decode(errors="replace") if raw is not None else ""


def _invoke(settings: AssaySettings, scope: ArtifactScope, tool: Tool, *args: str) -> Completed:
    """Run one ilspycmd invocation through the engine: splice the per-call args, capture.

    The per-call assembly path (and ``-t <type>`` / decompile flags) splice onto ``tool.command`` via
    ``structs.replace`` so the engine projector stays generic and ``Input.NONE`` appends no routing
    tail. A spawn fault degrades to a synthetic non-zero ``Completed`` so the caller's surface fold
    reads exit code uniformly — the rail reserves the ``Error`` rail for ``--strict`` alone. ``scope`` is
    bound here (cache-path namespace) but the engine spawn runs ``scope=None``: the engine ``_overlay``
    folds the ``DOTNET_CLI_HOME`` isolation env for ANY scope, which redirects the dotnet CLI home away
    from the real ``dotnet-tools.json`` manifest and breaks ``dotnet tool run ilspycmd`` — this rail needs
    the real CLI home, so the dotnet env isolation is suppressed at the ilspycmd spawn boundary.
    """
    _ = scope
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED)
    check = Check(tool=msgspec.structs.replace(tool, command=(*tool.command, *args)))
    match run_check(check, settings=settings, scope=None, routed=routed):
        case Result(tag="ok", ok=done):
            return done
        case Result(error=fault):
            return Completed((*tool.runner.prefix, *tool.command, *args), 1, stderr=fault.message.encode())


def _surface(settings: AssaySettings, scope: ArtifactScope, source: _Source) -> Result[_Surface, Fault]:
    """Resolve the type/namespace roster, polymorphic on ``source.kind``: ilspycmd for C#, INPROC for PYDIST/TSDECL.

    The C# arm (``ASSEMBLY``/``NUGET``) is byte-unchanged: cache-first by content fingerprint, ``-l cisde``
    on a miss. The PYDIST/TSDECL arm runs the bound ``inspect``/tree-sitter roster thunk through ``Runner.INPROC``
    and decodes the ``Capture`` array into the SAME ``_Surface`` shape — so ``_query_shape``/``_resolve_report``
    need zero change. An empty source folds ``EMPTY`` (off-host / no ``.d.ts``) so a speculative poll exits 0.
    """
    match source.kind:
        case SourceKind.ASSEMBLY | SourceKind.NUGET:
            return _cs_surface(settings, scope, source)
        case SourceKind.PYDIST | SourceKind.TSDECL:
            return _inproc_surface(settings, source)
        case _:  # pragma: no cover  # SourceKind.TOOL never resolves through _source
            return Ok(_Surface(source, (), (), {}, _cache_path(settings, source, ""), ""))


def _cs_surface(settings: AssaySettings, scope: ArtifactScope, source: _Source) -> Result[_Surface, Fault]:
    """The C# ilspycmd roster: cache-first by content fingerprint, ``-l cisde`` over each assembly on a miss.

    A non-zero exit on a *present* assembly is a tool fault (``FAULTED``, exit 2): assay could not surface
    the API, so the stderr rides the ``Fault`` message on the ``Error`` channel — distinct (per the ``Fault``
    contract) from a check that ran and found defects (``FAILED``, exit 1, the success channel).
    """
    assemblies = source.assemblies
    match next((t for t in select(Claim.API, Language.CSHARP)), None):
        case None:
            return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message="no ilspycmd catalog row"))
        case Tool() if not assemblies:
            return Ok(_Surface(source, (), (), {}, _cache_path(settings, source, ""), ""))
        case Tool() as tool:
            cache = _cache_path(settings, source, _fingerprint(assemblies))
            match cache.is_file():
                case True:
                    return Ok(_parse_surface(source, cache, cache.read_text(encoding="utf-8", errors="replace")))
                case False:
                    return _run_surface(settings, scope, source, tool, assemblies, cache)


def _inproc_check(settings: AssaySettings, source: _Source, mode: Mode, thunk: InprocThunk) -> Result[Completed, Fault]:
    """Splice ``thunk`` onto the ``(Claim.API, language, mode)`` INPROC catalog row and run it through the engine.

    Mirrors ``_invoke`` but on the INPROC splice axis (``tool.thunk`` rather than ``tool.command``): selects
    the surface (``Mode.QUERY``) or member (``Mode.LIST``) row, binds the thunk, and runs one ``Check`` so the
    in-process ``inspect``/tree-sitter roster rides the same deadline, CapacityLimiter, and traced span as a
    subprocess spawn. A spawn fault rides the ``Error`` channel; a thunk fault is a non-zero ``Completed``.
    """
    language = Language.PYTHON if source.kind is SourceKind.PYDIST else Language.TYPESCRIPT
    match next((t for t in select(Claim.API, language) if t.mode is mode), None):
        case None:
            return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message=f"no {language.value} INPROC api row"))
        case Tool() as tool:
            check = Check(tool=msgspec.structs.replace(tool, thunk=thunk))
            routed = Routed(language=language, scope=Scope.CHANGED)
            return run_check(check, settings=settings, scope=None, routed=routed)


def _inproc_surface(settings: AssaySettings, source: _Source) -> Result[_Surface, Fault]:
    """Run the PYDIST/TSDECL roster thunk and fold the decoded ``Capture`` array into the ``_Surface`` shape.

    An empty ``asset_paths`` (a dist with no importable module, an npm package with no ``.d.ts``) folds to an
    empty ``_Surface`` (EMPTY, exit 0) — never FAULTED. The decoded ``@type`` captures' ``text`` are the
    fully-qualified type names ``_parse_inproc`` folds into the namespace index, identical to ``_parse_surface``.
    """
    cache = _cache_path(settings, source, _fingerprint(source.asset_paths))
    return _inproc_check(settings, source, Mode.QUERY, _inproc_thunk(source, "")).map(lambda done: _parse_inproc(source, cache, done))


def _parse_inproc(source: _Source, cache: Path, done: Completed) -> _Surface:
    """Decode the INPROC ``@type`` capture array into the type/namespace roster (the PYDIST/TSDECL INDEX/NAMESPACE model)."""
    types = tuple(dict.fromkeys(cap.text for cap in CAPTURES.decode(done.stdout or b"[]") if cap.name == "type" and cap.text))
    return _roster(source, cache, types, done.stdout.decode(errors="replace"))


def _cache_path(settings: AssaySettings, source: _Source, fingerprint: str) -> Path:
    """The content-addressed surface cache path: ``.artifacts/assay/scope/api/<safe-key>.<fingerprint>.txt``."""
    return Path(
        str(settings.artifact(ArtifactKind.SCOPE, "api", f"{_safe(source.key)}.{fingerprint or 'unresolved'}.txt"))
    )  # ilspy decompile cache is local-fs; UPath → Path


def _run_surface(
    settings: AssaySettings, scope: ArtifactScope, source: _Source, tool: Tool, assemblies: tuple[Path, ...], cache: Path
) -> Result[_Surface, Fault]:
    """Run ``-l cisde`` over each assembly, write the deterministic cache, and parse the roster."""
    attempts = tuple((asm, _invoke(settings, scope, tool, str(asm))) for asm in assemblies)
    match any(done.returncode == 0 for _, done in attempts):
        case False:
            detail = "\n".join(d.stderr.decode(errors="replace") for _, d in attempts if d.stderr) or "ilspycmd type listing failed"
            return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message=detail[:1024]))
        case True:
            text = "\n".join(f"# {asm}\n{done.stdout.decode(errors='replace')}" for asm, done in attempts if done.stdout)
            cache.parent.mkdir(parents=True, exist_ok=True)
            cache.write_text(text, encoding="utf-8")
            return Ok(_parse_surface(source, cache, text))


def _namespace_of(fqn: str, types: frozenset[str]) -> str:
    """Project the owning namespace of a fully-qualified type: the longest prefix that is itself a known type's owner."""
    parts = fqn.split(".")
    return next((".".join(parts[:i]) for i in range(len(parts)) if ".".join(parts[: i + 1]) in types), ".".join(parts[:-1]))


def _parse_surface(source: _Source, cache: Path, text: str) -> _Surface:
    """Parse ilspycmd ``-l cisde`` text into the type/namespace roster (the INDEX/NAMESPACE/SEARCH model)."""
    types = tuple(
        dict.fromkeys(
            parts[1]
            for line in text.splitlines()
            if line.strip() and not line.startswith("# ")
            for parts in (line.split(maxsplit=1),)
            if len(parts) == 2 and parts[0] in _SURFACE_KINDS
        )
    )
    return _roster(source, cache, types, text)


def _roster(source: _Source, cache: Path, types: tuple[str, ...], raw: str) -> _Surface:
    """Fold a flat type bag into the ``_Surface`` namespace index — the sole roster projection both the ilspy and INPROC parsers share."""
    type_set = frozenset(types)
    namespace_of = {fqn: _namespace_of(fqn, type_set) for fqn in types}
    namespaces = tuple(sorted({ns for ns in namespace_of.values() if ns}))
    by_namespace = {ns: tuple(fqn for fqn in types if namespace_of[fqn] == ns) for ns in namespaces}
    return _Surface(source=source, types=types, namespaces=namespaces, by_namespace=by_namespace, cache=cache, raw=raw)


def _rank_type(types: tuple[str, ...], needle: str) -> str:
    """Rank a type-name needle to its FQN: exact casefold first, then the shortest matching suffix."""
    casefold = needle.casefold()
    matched = tuple(fqn for fqn in types if fqn.casefold() == casefold or fqn.casefold().endswith("." + casefold))
    ranked = sorted(matched, key=lambda fqn: (fqn.casefold() != casefold, len(fqn)))
    return next(iter(ranked), "")


def _xml_doc(source: _Source, symbol: str) -> str:
    """Extract the ``///`` prose for a symbol, anchored on the .NET XMLDoc member-ID grammar.

    A ``<member name="X:Ns.Type.Member(params)">`` id is stripped of its ``X:`` kind prefix and the
    ``(`` param list, leaving the fully-qualified dotted name; the simple symbol token anchors on a
    dotted-segment boundary (``== name`` for an FQN token, else ``name.endswith("." + symbol)``) — a
    suffix on the segment grammar, never an unanchored substring. ``Point3d`` resolves the ``T:`` type
    summary, ``Point3d.X`` the ``P:`` property, ``Mesh.Weld`` the ``M:`` method — and a sibling whose
    signature merely mentions the name (a parameter type) no longer steals the lookup.
    """
    needle = symbol.casefold()
    return next(
        (
            "".join(member.itertext()).strip()[:480]
            for path in source.xmls
            if path.is_file()
            for member in (_xml_members(path))
            for name in (member.get("name", "").split(":", 1)[-1].split("(", 1)[0].casefold(),)
            if name == needle or name.endswith("." + needle)
        ),
        "",
    )


def _xml_members(path: Path) -> tuple[ET.Element, ...]:
    """Read the ``<member>`` nodes of a doc ``.xml``; malformed XML degrades to an empty tuple (no raise)."""
    try:
        root = ET.fromstring(path.read_bytes())  # noqa: S314  # trusted local sidecar .xml from the resolved source, never network-sourced
    except ET.ParseError:
        return ()
    return tuple(root.iterfind(".//member"))


def _decompile(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, symbol: str, p: ApiParams) -> Result[_Body, Fault]:
    """Project one type/member to a ``_Body``, polymorphic on ``source.kind``: ilspycmd ``-t`` for C#, INPROC for PYDIST/TSDECL.

    The C# arm (``ASSEMBLY``/``NUGET``) is byte-unchanged: drop a member token's final segment to find
    the owning type, decompile via ``-t <type>``, anchor the signature on the modifier-prefixed declaration
    line (never a ``///`` occurrence). The PYDIST/TSDECL arm runs the bound member thunk and projects its
    single ``@signature``/``@doc``/``@full`` captures into the same ``_Body``. An unresolvable symbol yields
    an empty ``signature`` so the ``query`` fold falls through to SEARCH.
    """
    match surface.source.kind:
        case SourceKind.ASSEMBLY | SourceKind.NUGET:
            return _cs_decompile(settings, scope, surface, symbol, p)
        case SourceKind.PYDIST | SourceKind.TSDECL:
            return _inproc_decompile(settings, surface, symbol, p)
        case _:  # pragma: no cover  # SourceKind.TOOL never resolves through _source
            return Ok(_Body(signature="", xml="", window="", full="", selected=0, truncated=False))


def _cs_decompile(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, symbol: str, p: ApiParams) -> Result[_Body, Fault]:
    """The C# ilspycmd ``-t <type>`` decompile: anchor the member signature on the declaration line, doc from the sidecar XML."""
    head, _, tail = symbol.rpartition(".")
    fqn = _rank_type(surface.types, symbol) or _rank_type(surface.types, head)
    match next((t for t in select(Claim.API, Language.CSHARP)), None):
        case None:
            return Error(Fault(("api", "decompile", symbol), status=RailStatus.FAULTED, message="no ilspycmd catalog row"))
        case Tool() if not fqn:
            return Ok(_Body(signature="", xml="", window="", full="", selected=0, truncated=False))
        case Tool() as tool:
            done = _run_decompile(settings, scope, tool, fqn, surface.source.assemblies)
            return Ok(_body(done, tail or symbol.rsplit(".", 1)[-1], p, doc=_xml_doc(surface.source, symbol)))


def _inproc_decompile(settings: AssaySettings, surface: _Surface, symbol: str, p: ApiParams) -> Result[_Body, Fault]:
    """The PYDIST/TSDECL member decompile: run the bound member thunk and project its captures into ``_Body``.

    PYDIST emits ``@signature`` (inspect/annotationlib), ``@doc`` (``inspect.getdoc``), ``@full``
    (``inspect.getsource``); TSDECL emits the matching declaration's ``@signature`` (the JSDoc-adjacent
    doc rides ``""`` — a ``.d.ts`` declaration is the signature). The doc rides the thunk's captures, never
    ``_xml_doc`` (which is XMLDoc-specific), so ``_body`` takes the pre-resolved doc as a param.
    """
    source = surface.source
    return _inproc_check(settings, source, Mode.LIST, _inproc_thunk(source, symbol)).map(lambda done: _inproc_body(done, p))


def _inproc_body(done: Completed, p: ApiParams) -> _Body:
    """Fold the member thunk's ``@signature``/``@doc``/``@full`` captures into a windowed ``_Body``."""
    captured = {cap.name: cap.text for cap in CAPTURES.decode(done.stdout or b"[]")}
    signature = captured.get("signature", "")
    full = captured.get("full", "") or signature
    lines = tuple(line for line in full.splitlines() if not p.grep or p.grep.casefold() in line.casefold())
    window = lines if p.full else lines[: p.max_lines]
    return _Body(
        signature=signature,
        xml=captured.get("doc", ""),
        window="\n".join(window),
        full=full,
        selected=len(lines),
        truncated=not p.full and len(lines) > len(window),
    )


def _run_decompile(settings: AssaySettings, scope: ArtifactScope, tool: Tool, fqn: str, assemblies: tuple[Path, ...]) -> Completed:
    """Decompile ``fqn`` across the assemblies (ref-first), returning the first non-empty success (else first attempt)."""
    ordered = sorted(assemblies, key=lambda a: ("/ref/" not in a.as_posix(), a.as_posix().casefold()))
    decompile_tool = msgspec.structs.replace(tool, command=(*(c for c in tool.command if c not in {"-l", "cisde"}), "-t", fqn, *_DECOMPILE_FLAGS))
    attempts = tuple(_invoke(settings, scope, decompile_tool, str(asm)) for asm in ordered)
    return next((d for d in attempts if d.returncode == 0 and d.stdout), attempts[0] if attempts else Completed(("ilspycmd",), 1))


def _body(done: Completed, simple: str, p: ApiParams, *, doc: str) -> _Body:
    """Window the decompiled text and anchor the signature on the declaration line (excluding doc comments).

    ``doc`` is the pre-resolved prose slot the caller owns (the C# arm passes ``_xml_doc(source, symbol)``,
    so the XMLDoc lookup never mis-fires on a non-C# source); ``simple`` anchors the modifier-prefixed
    declaration line of the member identifier, never a ``///`` doc-comment occurrence.
    """
    text = done.stdout.decode(errors="replace")
    lines = tuple(line for line in text.splitlines() if not p.grep or p.grep.casefold() in line.casefold())
    boundary = re.compile(rf"\b{re.escape(simple)}\b", re.IGNORECASE)
    declared = tuple(off for off, line in enumerate(lines) if boundary.search(line) and not line.lstrip().startswith("///"))
    anchor = next(iter(declared), 0)
    window = lines if p.full else lines[anchor : anchor + p.max_lines]
    signature = next((lines[off].strip() for off in declared), next(iter(window), "").strip())
    return _Body(
        signature=signature if done.returncode == 0 and text else "",
        xml=doc,
        window="\n".join(window),
        full=text,
        selected=len(lines),
        truncated=not p.full and len(lines) > len(window),
    )


def _artifact(settings: AssaySettings, source: _Source, name: str, content: str) -> Artifact:
    """Write a content artifact under the api scope namespace and project its ``Artifact`` receipt."""
    path = settings.artifact(ArtifactKind.SCOPE, "api", _safe(source.key), name)
    path.parent.mkdir(parents=True, exist_ok=True)
    raw = content.encode()
    path.write_bytes(raw)
    digest = hashlib.sha256(f"{path}|{name}".encode()).hexdigest()[:12]
    return Artifact(id=digest, kind=ArtifactKind.SCOPE, path=str(path), bytes=len(raw), lines=raw.count(b"\n"))


def _matches(rows: tuple[str, ...], kind: str, pattern: str) -> tuple[Match, ...]:
    """Rank a row bag into bounded ``Match`` evidence: substring score, shorter-name bonus, capped."""
    query = pattern.casefold()

    def score(text: str) -> int:
        return (100 if query and query in text.casefold() else 0) + max(0, 40 - len(text) // 4)

    ranked = tuple(sorted((r for r in rows if not pattern or query in r.casefold()), key=lambda r: (-score(r), r)))
    return tuple(
        Match(id=f"{kind}-{i:03d}", kind=ArtifactKind.SCOPE, text=r[:320], score=score(r)) for i, r in enumerate(ranked[:_RESULT_CAP], start=1)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def doctor(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """``api doctor [--strict]``: host/NuGet/tool inventory; ``--strict`` promotes absence to ``FAULTED``.

    Folds the presence of the Rhino bundle, the ilspycmd version, and the central package inventory
    into one ``Completed`` so ``model.fold`` derives status/counts. ``--strict`` promoting the folded
    ``EMPTY``/``SKIP`` to an ``Error`` fault is the *sole* ``api`` fault promoter; every other verb
    exits 0 on absence.
    """
    surface_tool = next((t for t in select(Claim.API, Language.CSHARP)), None)
    app = _rhino_app(settings)
    version = _invoke(settings, scope, surface_tool, "--version") if surface_tool is not None else Completed(("ilspycmd",), 1)
    rows = _inventory(settings, app, version)
    healthy = all(ok for ok, _ in rows)
    done = Completed(("api", "doctor"), 0, status=RailStatus.OK if healthy else RailStatus.EMPTY, notes=tuple(text for _, text in rows))
    return _strict(fold(Claim.API, "doctor", (done,)), p)


def _inventory(settings: AssaySettings, app: Path | None, version: Completed) -> tuple[tuple[bool, str], ...]:
    """Fold the host bundle, ilspycmd version, central packages, Python dists, and npm ``.d.ts`` into ``(ok, note)`` rows."""
    packages = _packages(settings)
    host_keys = tuple(k for k in _HOST_SPECS if (src := _host_source(settings, k)) is not None and src.assemblies)
    pydists = _pydist_names()
    tsdecls = _tsdecl_names(settings)
    return (
        _present("rhino-app", app is not None and app.is_dir(), str(app) if app is not None else ""),
        _present("ilspycmd", version.returncode == 0, version.stdout.decode(errors="replace").strip() or "unavailable"),
        _present("host-sources", bool(host_keys), ",".join(host_keys)),
        _present("nuget-sources", bool(packages), str(len(packages))),
        _present("python-dists", bool(pydists), str(len(pydists))),
        _present("ts-decls", bool(tsdecls), str(len(tsdecls))),
    )


def _present(name: str, ok: bool, detail: str) -> tuple[bool, str]:  # noqa: FBT001  # inventory pair: presence flag + note text
    """Project one inventory probe to a ``(ok, note)`` pair: marked presence plus the resolved detail."""
    marker = "[OK]" if ok else "[MISSING]"
    return ok, f"{marker} {name}: {detail}" if detail else f"{marker} {name}"


def _strict(report: Report, p: ApiParams) -> Result[Report, Fault]:
    """Promote a folded ``EMPTY``/``SKIP`` report to ``Error(Fault(FAULTED))`` under ``--strict``."""
    match (p.strict, report.status):
        case (True, RailStatus.EMPTY | RailStatus.SKIP):
            return Error(Fault(("api", report.verb), status=RailStatus.FAULTED, message=f"{report.verb} inventory incomplete under --strict"))
        case _:
            return Ok(report)


def resolve(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """``api resolve <key> [kind]``: fuzzy-resolve a source and list its ``kind`` asset paths.

    ``kind`` ∈ ``all | assembly | xml | nuspec | deps | package-root``; a resolved source with an
    existing subset folds ``OK`` and one whose declared paths are all absent folds ``EMPTY`` (exit 0 —
    evidence absent, not a broken operation). An *unknown/ambiguous key* instead folds an ``UNSUPPORTED``
    ``ApiResolution`` carrying the nearest scored source candidates + reason (richer-on-failure, exit 3).
    The full declared path list rides a scope ``Artifact``; the bounded ranked subset rides ``Report.results``.
    """
    _ = scope
    match _source(settings, p.key):
        case Result(tag="ok", ok=source):
            return Ok(_resolve_report(settings, source, p))
        case Result(error=resolution):
            return Ok(_miss_report("resolve", p.key, resolution))


def _resolve_report(settings: AssaySettings, source: _Source, p: ApiParams) -> Report:
    """Build the ``resolve`` ``Report``: full path list → scope ``Artifact``; ranked subset → ``Report.results``."""
    targets = _resolve_targets(source, p.kind)
    existing = tuple(t for t in targets if t.exists())
    artifact = _artifact(settings, source, f"{p.kind}.paths.txt", "\n".join(str(t) for t in targets))
    note = f"{len(existing)}/{len(targets)} {p.kind} paths present"
    done = Completed(("api", "resolve", p.key), 0, status=RailStatus.OK if existing else RailStatus.EMPTY, notes=(note,))
    detail = _api_detail(source, SymbolShape.SEARCH, preview="\n".join(str(t) for t in existing[:_PREVIEW_ROWS]))
    report = fold(Claim.API, "resolve", (done,), detail=detail)
    return msgspec.structs.replace(report, artifacts=(artifact,), results=_matches(tuple(str(t) for t in targets), p.kind, ""))


def _resolve_targets(source: _Source, kind: _PathKind) -> tuple[Path, ...]:
    """Project the ``resolve`` ``kind`` token onto its declared path set (table-driven, dedup-ordered)."""
    catalog: dict[_PathKind, tuple[Path, ...]] = {
        "all": (*source.assemblies, *source.xmls, *((source.nuspec,) if source.nuspec is not None else ()), *source.asset_paths),
        "assembly": source.assemblies,
        "xml": source.xmls,
        "nuspec": (source.nuspec,) if source.nuspec is not None else (),
        "deps": tuple(p for p in source.asset_paths if any(part in _DEPS_PARTS for part in p.parts)),
        "package-root": (source.package_root,) if source.package_root is not None else (),
    }
    return tuple(dict.fromkeys(catalog.get(kind, catalog["all"])))


def query(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """``api query <key> [symbol] [--max-lines|--full|--grep]``: one polymorphic shape fold.

    Resolves the cached surface, computes the request ``SymbolShape`` once via ``shape_of``, then one
    ``match`` projects it: INDEX/NAMESPACE emit a roster ``ApiSurface`` preview; TYPE/MEMBER decompile
    and emit a signature/doc/window ``ApiSurface`` (full body → scope ``Artifact`` on truncation); a
    decompile/search miss folds an ``UNSUPPORTED`` ``ApiResolution`` carrying the nearest scored type
    candidates. A key miss likewise rides ``UNSUPPORTED`` with the nearest source candidates (richer-on-
    failure, exit 3); only a spawn fault crosses the ``Error`` channel.
    """
    match _source(settings, p.key):
        case Result(tag="ok", ok=source):
            return _surface(settings, scope, source).bind(lambda surface: _query_shape(settings, scope, surface, p))
        case Result(error=resolution):
            return Ok(_miss_report("query", p.key, resolution))


def _query_shape(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, p: ApiParams) -> Result[Report, Fault]:
    """The single polymorphic ``SymbolShape`` dispatch: one ``match`` arm per shape, ``assert_never`` closed."""
    match shape_of(p.symbol):
        case SymbolShape.INDEX:
            # a namespace-less surface (a flat .d.ts export set) rosters its types directly; a namespaced surface (C#) rosters namespaces
            rows, kind = (surface.namespaces, "namespace") if surface.namespaces else (surface.types, "type")
            return Ok(_roster_report(settings, surface, SymbolShape.INDEX, rows, kind, p))
        case SymbolShape.NAMESPACE:
            owned = surface.by_namespace.get(_rank_namespace(surface, p.symbol), ())
            return Ok(_roster_report(settings, surface, SymbolShape.NAMESPACE, owned, "type", p))
        case SymbolShape.TYPE | SymbolShape.MEMBER as shape:
            return _decompile(settings, scope, surface, p.symbol, p).map(lambda body: _decompile_report(settings, surface, shape, body, p))
        case SymbolShape.SEARCH:  # pragma: no cover  # shape_of never emits SEARCH; the decompile-miss path routes through _decompile_report
            return Ok(_search_report(surface, p))
        case never:  # pragma: no cover
            assert_never(never)


def _rank_namespace(surface: _Surface, symbol: str) -> str:
    """Resolve a namespace token to its canonical casing (case-insensitive match against the roster)."""
    casefold = symbol.casefold()
    return next((ns for ns in surface.namespaces if ns.casefold() == casefold), symbol)


def _roster_report(settings: AssaySettings, surface: _Surface, shape: SymbolShape, rows: tuple[str, ...], kind: str, p: ApiParams) -> Report:
    """Build the INDEX/NAMESPACE roster ``Report``: bounded preview + ranked ``Match`` rows + full-listing ``Artifact``."""
    source = surface.source
    preview = "\n".join(rows[:_PREVIEW_ROWS])
    artifact = _artifact(settings, source, f"{shape.value}.txt", "\n".join(rows))
    status = RailStatus.OK if rows else RailStatus.EMPTY
    note = f"{len(surface.types)} types across {len(surface.namespaces)} namespaces"
    done = Completed(("api", "query", source.key), 0, status=status, notes=(note,))
    report = fold(Claim.API, "query", (done,), detail=_api_detail(source, shape, preview=preview))
    return msgspec.structs.replace(report, artifacts=(artifact,), results=_matches(rows, kind, p.grep))


def _search_report(surface: _Surface, p: ApiParams) -> Report:
    """Build the SEARCH ``Report`` from a decompile miss: ranked substring hits, else a richer ``ApiResolution``.

    Substring hits emit the canonical SEARCH ``ApiSurface`` (``OK``) with ranked ``Match`` rows. Zero hits
    is a genuine symbol miss: rather than a bare ``EMPTY``, fold an ``UNSUPPORTED`` ``ApiResolution``
    carrying the nearest scored type candidates (segment/suffix overlap via ``_candidates``) and reason
    ``partial`` — the agent gets the nearest type names to retry against, the headline richer-on-failure path.
    """
    source = surface.source
    needle = p.symbol.casefold()
    hits = tuple(fqn for fqn in surface.types if needle in fqn.casefold())
    match hits:
        case ():
            done = Completed(("api", "query", source.key), 0, status=RailStatus.UNSUPPORTED, notes=(f"no match for '{p.symbol}'",))
            return fold(Claim.API, "query", (done,), detail=ApiResolution(candidates=_candidates(surface.types, p.symbol), reason="partial"))
        case _:
            done = Completed(("api", "query", source.key), 0, status=RailStatus.OK, notes=(f"{len(hits)} search hits",))
            detail = _api_detail(source, SymbolShape.SEARCH, preview="\n".join(hits[:_PREVIEW_ROWS]))
            return msgspec.structs.replace(fold(Claim.API, "query", (done,), detail=detail), results=_matches(hits, "type", p.symbol))


def _decompile_report(settings: AssaySettings, surface: _Surface, shape: SymbolShape, body: _Body, p: ApiParams) -> Report:
    """Build the TYPE/MEMBER ``Report``: signature/doc/window ``ApiSurface``; full body → scope ``Artifact``.

    An empty signature (unresolvable type or empty decompile) falls back to the ranked SEARCH over the
    cached surface, keeping the verb total without a second entry point. On window truncation the full
    decompiled text rides a scope ``Artifact`` so the inline preview stays bounded.
    """
    source = surface.source
    match body.signature:
        case "":
            return _search_report(surface, p)
        case _:
            detail = _api_detail(source, shape, signature=body.signature, doc=body.xml, preview=body.window)
            artifact = _artifact(settings, source, "decompile.cs", body.full)
            done = Completed(("api", "query", source.key), 0, status=RailStatus.OK, notes=(f"{body.selected} selected lines",))
            report = fold(Claim.API, "query", (done,), detail=detail)
            return msgspec.structs.replace(report, artifacts=(artifact,) if body.truncated else ())


def _api_detail(source: _Source, shape: SymbolShape, *, signature: str = "", doc: str = "", preview: str = "") -> ApiSurface:
    """Mint the rail's sole ``Detail``: typed source provenance + symbol shape, never a ``dict``."""
    return ApiSurface(
        source_kind=source.kind, source_id=source.key, version=source.version, shape=shape, signature=signature, doc=doc, preview=preview
    )


def _miss_report(verb: str, key: str, resolution: ApiResolution) -> Report:
    """Fold a key-miss ``ApiResolution`` onto an ``UNSUPPORTED`` ``Report`` (richer-on-failure, exit 3).

    A miss is not a broken operation (no ``FAILED``/``Fault``) nor a silent ``EMPTY``: ``UNSUPPORTED``
    (exit 3) on the success channel carries the typed ``ApiResolution`` so the agent reads the nearest
    scored candidates and the ``reason`` (``unknown``/``ambiguous``) as crucial next-step evidence.
    """
    note = f"no '{key}' source; {resolution.reason}, {len(resolution.candidates)} nearest candidate(s)"
    done = Completed(("api", verb, key), 0, status=RailStatus.UNSUPPORTED, notes=(note,))
    return fold(Claim.API, verb, (done,), detail=resolution)


def show(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """``api show <token> [--lines|--grep|--full|--latest]``: preview a prior artifact's text.

    Locates the artifact by ``token`` (a direct path, or a glob under the api scope namespace —
    ``--latest`` widens to every run, else the current run only), windows it by ``--lines`` (``a:b``)
    or ``--max-lines`` with optional ``--grep``, and folds the preview onto ``Report.detail.preview``.
    A missing token folds ``EMPTY`` (exit 0) — the artifact has not been produced yet, not a fault.
    """
    _ = scope
    match _show_target(Path(str(settings.artifact(ArtifactKind.SCOPE, "api"))), p):  # api artifact tree is local-fs; UPath → Path
        case Path() as path if path.is_file():
            return Ok(_show_report(path, p))
        case _:
            done = Completed(("api", "show", p.token), 0, status=RailStatus.EMPTY, notes=(f"artifact not found: {p.token}",))
            return Ok(fold(Claim.API, "show", (done,)))


def _show_report(path: Path, p: ApiParams) -> Report:
    """Build the ``show`` ``Report``: window the artifact text onto ``ApiSurface.preview`` + an ``Artifact`` receipt."""
    text = path.read_text(encoding="utf-8", errors="replace")
    window, total, truncated = _slice(text, lines=p.lines, grep=p.grep, max_lines=p.max_lines, full=p.full)
    digest = hashlib.sha256(f"{path}|{path.name}".encode()).hexdigest()[:12]
    artifact = Artifact(id=digest, kind=ArtifactKind.SCOPE, path=str(path), bytes=path.stat().st_size, lines=text.count("\n"))
    detail = ApiSurface(source_kind=SourceKind.TOOL, source_id=p.token, shape=SymbolShape.SEARCH, preview=window)
    done = Completed(("api", "show", p.token), 0, status=RailStatus.OK, notes=(f"{total} selected lines",))
    _ = truncated  # Envelope.truncated derives in registry._emit from the saturated results/artifacts bounds, not from Report
    return msgspec.structs.replace(fold(Claim.API, "show", (done,), detail=detail), artifacts=(artifact,))


def _show_target(base: Path, p: ApiParams) -> Path | None:
    """Locate the ``show`` artifact: a direct path first, then the newest glob match under the api scope tree."""
    direct = Path(p.token).expanduser()
    globbed = sorted(base.glob(f"**/{p.token}"), key=lambda path: path.stat().st_mtime, reverse=True) if base.is_dir() else []
    candidates = ((direct,) if direct.is_file() else ()) + tuple(globbed)
    return next((c for c in candidates if c.is_file()), None)


def _slice(text: str, *, lines: str, grep: str, max_lines: int, full: bool) -> tuple[str, int, bool]:
    """Window text by ``--lines`` (``a:b``) or ``--max-lines`` with optional ``--grep``; report selected count + truncation."""
    selected = tuple(line for line in text.splitlines() if not grep or grep.casefold() in line.casefold())
    match (full, lines.split(":", maxsplit=1)):
        case (True, _):
            window = selected
        case (False, [start, end]) if start.isdigit() and end.isdigit():
            window = selected[int(start) - 1 : int(end)]
        case _:
            window = selected[:max_lines]
    return "\n".join(window), len(selected), len(selected) > len(window)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ApiParams", "doctor", "query", "resolve", "shape_of", "show"]
