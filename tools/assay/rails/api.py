"""Resolve API metadata from host assemblies, packages, Python dists, and TS declarations.

Resolution priority is host bundle, NuGet manifest, installed Python distribution, then node_modules
declarations. C# sources route through ilspycmd; Python and TypeScript sources route through INPROC thunks.
"""

import annotationlib  # PEP 749: STRING annotations avoid evaluating unresolvable forward refs
from dataclasses import dataclass, replace
import difflib
from enum import StrEnum
from functools import lru_cache
import hashlib
import importlib
import importlib.metadata
import inspect
import itertools
import operator
from pathlib import Path
import re
from typing import assert_never, override, TYPE_CHECKING, TypeAliasType
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local Directory.Packages.props XML, never network-sourced

from cyclopts.types import PositiveInt  # noqa: TC002  # Cyclopts evaluates Param dataclass annotations at runtime.
from expression import Error, Ok, Result
import msgspec
from tree_sitter import Parser as TSParser, QueryCursor
import tree_sitter_typescript

from tools.assay.composition.catalog import Capture, CAPTURE_ENCODER, CAPTURES, select
from tools.assay.composition.settings import (  # noqa: TC001  # beartype resolves these types at runtime in @checked signatures
    ArtifactScope,
    ArtifactStore,
    AssaySettings,
)
from tools.assay.core.engine import run_check
from tools.assay.core.model import (
    _RESULT_CAP,  # noqa: PLC2701  # shared saturation site
    ApiResolution,
    ApiSource,
    ApiSurface,
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Completed,
    Fault,
    fold,
    Language,
    Match,
    Mode,
    receipt,
    Report,  # noqa: TC001  # unconditional: beartype @checked resolves the -> Result[Report, Fault] forward-ref under PEP 649
    SourceKind,
    SymbolShape,
    Tool,
)
from tools.assay.core.routing import parse_csproj, Routed, Scope
from tools.assay.core.status import RailStatus
from tools.assay.rails.code import _cap_note, _node_text, ts_language, ts_query  # noqa: PLC2701  # shared rail primitives owned by code.py


if TYPE_CHECKING:
    from collections.abc import Callable

    from tree_sitter import Node

    from tools.assay.core.model import InprocThunk

    type _SourceResolver = Callable[[AssaySettings, str, dict[str, str]], _Source | None]


# --- [TYPES] ----------------------------------------------------------------------------

type _PathKind = str  # resolve kind token: all | assembly | xml | nuspec | deps | package-root


class Fidelity(StrEnum):
    """Decompilation fidelity of an API surface, derived from its SourceKind."""

    DECOMPILED = "decompiled"  # ilspycmd IL->C# reconstruction (assembly, nuget)
    INTROSPECTED = "introspected"  # live inspect of an imported Python distribution
    DECLARED = "declared"  # parsed .d.ts ambient declarations


# --- [CONSTANTS] ------------------------------------------------------------------------


_API_ARITY: dict[str, int] = {"query": 1, "resolve": 2, "show": 1}
# Each api verb is flag-only: a positional surplus names the exact flags the agent should have used instead, per slot.
_API_VERB_FLAGS: dict[str, tuple[str, ...]] = {"query": ("--symbol",), "resolve": ("--key", "--kind"), "show": ("--token",)}
_HOST_SPECS: dict[str, tuple[str, str]] = {
    "eto": ("Eto.dll", "Eto.xml"),
    "gh2": ("ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml"),
    "gh2-io": ("ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml"),
    "rhino-code": ("Rhino.Runtime.Code.dll", ""),
    "rhino-code-remote": ("Rhino.Runtime.Code.Remote.dll", ""),
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
# cisde renders open generics by bare name (no arity marker, no angle bracket); every `<` in a type name is a
# compiler synthetic: `<Module>`, `<>c`/`<>c__DisplayClass`, `<>f__AnonymousType`, `<MethodName>d__NN` iterators,
# and `<PrivateImplementationDetails>`. Filtering on the angle bracket drops synthetics and never a legitimate generic.
_NOISE: re.Pattern[str] = re.compile(r"<")
# Writer sentinel for a legitimately typeless assembly so its cache validates instead of rebuilding forever.
_EMPTY_SURFACE: str = "# assay:empty-surface"
_PACKAGE_KINDS: frozenset[SourceKind] = frozenset((SourceKind.NUGET, SourceKind.PYDIST, SourceKind.TSDECL))  # key is also a package name
_COMPACT_VISIBLE: frozenset[SourceKind] = frozenset((SourceKind.ASSEMBLY, SourceKind.NUGET, SourceKind.TOOL))  # status compact-output source kinds
_COMPACT_SUMMARY_IDS: frozenset[str] = frozenset(("python-dists", "ts-decls"))  # polyglot summary rows always shown in compact output
# Strict status faults only on an absent core bundle here, never a transitive package.
_REQUIRED_SOURCE_IDS: frozenset[str] = frozenset(_HOST_SPECS) | frozenset(("rhino-app", "ilspycmd"))
_PREVIEW_ROWS: int = 12
_CANDIDATE_CAP: int = 8
_DEPS_PARTS: frozenset[str] = frozenset(("build", "buildTransitive", "analyzers", "tools", "runtimes"))
_DECOMPILE_FLAGS: tuple[str, ...] = ("--no-dead-code", "--no-dead-stores")
_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")  # stable artifact wire order across runs
_NODE_MODULES: str = "node_modules"
_PNPM_STORE: str = "node_modules/.pnpm"  # pnpm mangles @scope/pkg as @scope+pkg
_PATH_KINDS: frozenset[_PathKind] = frozenset(("all", "assembly", "xml", "nuspec", "deps", "package-root"))
_DTS_GLOB: str = "*.d.ts"
_DTS_ENTRY: str = "index.d.ts"
_TS_DECL_QUERY: str = (
    "(class_declaration name: (type_identifier) @type)"
    " (abstract_class_declaration name: (type_identifier) @type)"
    " (interface_declaration name: (type_identifier) @type)"
    " (type_alias_declaration name: (type_identifier) @type)"
    " (enum_declaration name: (identifier) @type)"
    " (function_signature name: (identifier) @type)"
    " (module name: (identifier) @type)"
    " (variable_declarator name: (identifier) @type)"  # export declare const NAME
    " (namespace_export (identifier) @type)"  # export * as NAME
)
_TS_GRAMMAR: Callable[[], object] = tree_sitter_typescript.language_typescript  # shared with code.py
_DECL_NODES: frozenset[str] = frozenset((
    "class_declaration",
    "abstract_class_declaration",
    "interface_declaration",
    "type_alias_declaration",
    "enum_declaration",
    "function_signature",
    "method_signature",
    "property_signature",
    "public_field_definition",
    "variable_declarator",
))
_TYPE_CAP: str = "type"  # roster capture-name vocabulary: every INPROC/tree-sitter declaration row caps under this name
_INSPECT_KINDS: tuple[tuple[str, Callable[[object], bool]], ...] = (
    (_TYPE_CAP, inspect.isclass),
    (_TYPE_CAP, inspect.isfunction),
    (_TYPE_CAP, lambda obj: isinstance(obj, TypeAliasType)),  # PEP 695 `type` aliases surface alongside classes and functions
)
_NAME_CAP: int = 320
_SIG_CAP: int = 480
_FULL_CAP: int = 2560  # full .d.ts member body capture
_LATEST_ARTIFACT: str = "latest"

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ApiParams(BaseParams):
    """Parameters shared by api verbs."""

    key: str = "rhino-common"
    symbol: str = ""
    kind: _PathKind = "all"
    token: str = ""
    max_lines: PositiveInt = 120
    lines: str = ""
    grep: str = ""
    full: bool = False
    strict: bool = False
    sources: tuple[str, ...] = ()  # non-empty → restrict status inventory to source_ids matching these prefixes

    @override
    def bound(self, verb: str) -> ApiParams | Fault:
        """Project positional tokens into slots owned by an API verb.

        Args:
            verb: API verb controlling positional arity and slot projection.

        Returns:
            Bound params, or a parse fault for surplus positional tokens.
        """
        head, tail = (self.paths[0] if self.paths else ""), (self.paths[1] if len(self.paths) > 1 else "")
        arity = _API_ARITY.get(verb, 0)
        match (verb, len(self.paths)):
            case (_, n) if n > arity:
                return self.surplus(verb, self.paths[arity:], flags=_API_VERB_FLAGS.get(verb, ()), arity=arity)
            case ("query", _):
                return replace(self, symbol=head or self.symbol, paths=())
            case ("resolve", _):
                return replace(self, key=head or self.key, kind=(tail or self.kind), paths=())
            case ("show", _):
                return replace(self, token=head or self.token, paths=())
            case _:
                return self


@dataclass(frozen=True, slots=True)
class _Source:
    key: str
    kind: SourceKind
    version: str = ""
    assemblies: tuple[Path, ...] = ()
    xmls: tuple[Path, ...] = ()
    nuspec: Path | None = None
    package_root: Path | None = None
    asset_paths: tuple[Path, ...] = ()
    frameworks: tuple[str, ...] = ()
    owners: tuple[str, ...] = ()


@dataclass(frozen=True, slots=True)
class _Surface:
    source: _Source
    types: tuple[str, ...]
    namespaces: tuple[str, ...]
    by_namespace: dict[str, tuple[str, ...]]
    cache: str
    raw: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def shape_of(symbol: str) -> SymbolShape:
    """Classify an API query symbol.

    Returns:
        Symbol shape used by API query dispatch.
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
    return re.sub(r"[^A-Za-z0-9_.-]+", "-", key).strip("-") or "source"


@lru_cache(maxsize=256)
def _asm_digest(path_str: str, size: int, mtime_ns: int) -> str:  # noqa: ARG001  # size+mtime_ns are lru_cache key slots, not used in the body
    # RhinoWIP reinstalls can preserve DLL mtimes, so content-hash is the discriminant.
    return hashlib.sha256(Path(path_str).read_bytes()).hexdigest()


def _fingerprint(assemblies: tuple[Path, ...]) -> str:
    seed = "|".join(
        f"{p}:{st.st_size}:{st.st_mtime_ns}:{_asm_digest(str(p), st.st_size, st.st_mtime_ns)}"
        for p in assemblies
        if p.is_file()
        for st in (p.stat(),)
    )
    return hashlib.sha256(seed.encode()).hexdigest()[:16]


def _rhino_app(settings: AssaySettings) -> Path | None:
    # Worktree `rhino-app` symlink wins for CI; falls back to first installed bundle, or None.
    candidates = (Path(str(settings.root)) / "rhino-app", *(Path(b) for b in _RHINO_BUNDLES))  # bundle resolution is local-fs; UPath -> Path
    return next((c for c in candidates if c.is_dir()), None)


def _host_source(settings: AssaySettings, key: str) -> _Source | None:
    # Empty sources let status report absent bundles instead of hiding the row.
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


@lru_cache(maxsize=8)
def _packages_at(root_str: str, digest: str) -> dict[str, str]:
    _ = digest  # lru_cache key slot: content hash, immune to mtime-preserving rewrites
    try:
        root = ET.fromstring(Path(root_str).read_bytes())  # noqa: S314  # trusted local Directory.Packages.props, never network-sourced
    except OSError, ET.ParseError:
        return {}
    return {
        inc: ver for node in root.iterfind(".//PackageVersion") for inc, ver in ((node.get("Include", ""), node.get("Version", "")),) if inc and ver
    }


def _packages(settings: AssaySettings) -> dict[str, str]:
    path = settings.root / _PACKAGES_PROPS
    try:
        raw = path.read_bytes()
    except OSError:
        return {}
    return _packages_at(str(path), hashlib.sha256(raw).hexdigest()[:16])


def _score_name(name: str, needle: str) -> int:
    # Ranking favors exact, prefix, substring, segment overlap, then character similarity.
    casefold = needle.casefold()
    segments = frozenset(casefold.replace("-", ".").split("."))
    low = name.casefold()
    tokens = frozenset(low.replace("-", ".").split("."))
    overlap = len(segments & tokens) * 20 // max(1, len(tokens))
    base = 100 if low == casefold else 70 if low.startswith(casefold) else 40 if casefold in low else overlap
    best_seg = min(tokens, key=lambda t: abs(len(t) - len(casefold)), default=low)
    char_sim = int(difflib.SequenceMatcher(None, casefold, best_seg).ratio() * 30)
    length_bonus = max(0, 20 - len(name) // 4) if char_sim > 0 else 0
    return base + char_sim + length_bonus


def _rank_candidates(haystack: tuple[str, ...], needle: str, *, n: int = _CANDIDATE_CAP) -> tuple[tuple[str, int], ...]:
    ranked = sorted(((name, _score_name(name, needle)) for name in haystack), key=lambda row: (-row[1], row[0]))
    return tuple((name, sc) for name, sc in ranked[:n] if sc > 0)


def _resolve_key(packages: dict[str, str], key: str) -> Result[str, ApiResolution]:
    casefold = key.casefold()
    exact = tuple(n for n in packages if n.casefold() == casefold)
    fuzzy = tuple(n for n in packages if n.casefold().startswith(casefold)) or tuple(n for n in packages if casefold in n.casefold())
    hits = exact or fuzzy
    match (bool(exact) or len(hits) == 1, len(hits)):
        case (True, _):
            return Ok(hits[0])
        case (_, 0):
            return Error(ApiResolution(candidates=_rank_candidates(tuple(packages), key), reason="unknown"))
        case _:
            return Error(ApiResolution(candidates=_rank_candidates(hits, key, n=len(hits)), reason="ambiguous"))


def _package_root(settings: AssaySettings, package: str, version: str) -> Path:
    candidates = tuple(
        Path(root) if Path(root).is_absolute() else Path(str(settings.root)) / root for root in _NUGET_ROOTS
    )  # NuGet cache is local-fs; UPath -> Path
    targets = tuple(base / package.casefold() / version for base in candidates)
    return next((t for t in targets if t.is_dir()), targets[0])


def _framework_dir(root: Path, asset: str) -> Path | None:
    base = root / asset
    frameworks = tuple(p for p in base.iterdir() if p.is_dir()) if base.is_dir() else ()
    ranked = (*(p for name in _FRAMEWORK_RANK for p in frameworks if p.name == name), *sorted(p for p in frameworks if p.name not in _FRAMEWORK_RANK))
    return next(iter(ranked), None)


def _frameworks(root: Path) -> tuple[str, ...]:
    found = {p.name for asset in ("ref", "lib") for base in (root / asset,) if base.is_dir() for p in base.iterdir() if p.is_dir()}
    ranked = (*(name for name in _FRAMEWORK_RANK if name in found), *sorted(found - frozenset(_FRAMEWORK_RANK)))
    return tuple(ranked)


def _project_references(path: Path) -> tuple[str, ...]:
    try:
        raw = path.read_bytes()
    except OSError:
        return ()
    return tuple(ref.casefold() for ref in parse_csproj(raw, "PackageReference", "Include", "Update"))


@lru_cache(maxsize=8)
def _package_owner_index_at(root_str: str, fingerprint: str) -> dict[str, tuple[str, ...]]:
    _ = fingerprint  # lru_cache key slot; encodes sorted csproj mtime_ns so re-computation triggers on project graph changes
    root = Path(root_str)
    rows = sorted(
        (package, path.relative_to(root).as_posix())
        for path in root.rglob("*.csproj")
        if not any(part in {".artifacts", ".cache", "bin", "obj"} for part in path.parts)
        for package in _project_references(path)
    )
    return {package: tuple(sorted(owner for _, owner in group)) for package, group in itertools.groupby(rows, key=operator.itemgetter(0))}


def _package_owner_index(settings: AssaySettings) -> dict[str, tuple[str, ...]]:
    root = Path(str(settings.root))
    csproj_paths = sorted(p for p in root.rglob("*.csproj") if not any(part in {".artifacts", ".cache", "bin", "obj"} for part in p.parts))
    fingerprint = hashlib.sha256("|".join(f"{p}:{p.stat().st_mtime_ns}" for p in csproj_paths).encode()).hexdigest()[:16]
    return _package_owner_index_at(str(root), fingerprint)


def _package_owners(settings: AssaySettings, package: str) -> tuple[str, ...]:
    return _package_owner_index(settings).get(package.casefold(), ())


def _nuget_source(
    settings: AssaySettings, package: str, version: str, *, include_assets: bool = True, owners: tuple[str, ...] | None = None
) -> _Source:
    # Package-stem assemblies lead so the sidecar XML doc is first; an unrestored root folds to EMPTY.
    root = _package_root(settings, package, version)
    selected = _framework_dir(root, "ref") or _framework_dir(root, "lib")
    assemblies = tuple(sorted(selected.glob("*.dll"))) if selected is not None else ()
    primary = tuple(a for a in assemblies if a.stem.casefold() == package.casefold())
    ordered = (*primary, *(a for a in assemblies if a not in primary))
    xmls = tuple(a.with_suffix(".xml") for a in ordered if a.with_suffix(".xml").is_file())
    nuspec = next(iter(sorted(root.glob("*.nuspec"))), None) if root.is_dir() else None
    assets = (
        tuple(sorted(p for d in _ASSET_DIRS for base in (root / d,) if base.is_dir() for p in base.rglob("*") if p.is_file()))
        if include_assets
        else ()
    )
    return _Source(
        key=package,
        kind=SourceKind.NUGET,
        version=version,
        assemblies=ordered,
        xmls=xmls,
        nuspec=nuspec,
        package_root=root if root.is_dir() else None,
        asset_paths=assets,
        frameworks=_frameworks(root),
        owners=owners if owners is not None else _package_owners(settings, package),
    )


def _pydist_names() -> tuple[str, ...]:
    return tuple(sorted({dist.metadata["Name"] for dist in importlib.metadata.distributions() if dist.metadata["Name"]}))


def _pydist_inventory_sources() -> tuple[ApiSource, ...]:
    rows = []
    for dist in importlib.metadata.distributions():
        name = dist.metadata["Name"] or ""
        if not name:
            continue
        try:
            root = Path(str(dist.locate_file("")))
        except OSError:
            root = Path()
        rows.append(
            ApiSource(
                source_kind=SourceKind.PYDIST,
                source_id=name,
                version=dist.version or "",
                package=name,
                package_root=str(root) if root.is_dir() else "",
                status=RailStatus.OK,
            )
        )
    return tuple(sorted(rows, key=lambda row: row.source_id.casefold()))


def _pydist_source(key: str) -> _Source | None:
    # No assemblies: PYDIST roster and decompile use the INPROC inspect thunk, not ilspycmd.
    try:
        dist = importlib.metadata.distribution(key)  # codec boundary: an uninstalled key raises PackageNotFoundError -> None (fall through)
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
    # top_level.txt is absent in many modern wheels; packages_distributions() recovers real import roots.
    try:
        text = importlib.metadata.distribution(key).read_text("top_level.txt")
    except importlib.metadata.PackageNotFoundError:
        return ()
    declared = tuple(line.strip() for line in (text or "").splitlines() if line.strip())
    casefold = key.casefold()
    mapped = tuple(module for module, dists in importlib.metadata.packages_distributions().items() if any(d.casefold() == casefold for d in dists))
    return declared or mapped or (key.replace("-", "_"),)


def _tsdecl_names(settings: AssaySettings) -> tuple[str, ...]:
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


def _json_fields(path: Path, *fields: str) -> dict[str, str]:
    # One decode per manifest, but each field's str-decode is isolated: a malformed sibling value must not zero a valid field.
    try:
        data = msgspec.json.decode(path.read_bytes(), type=dict[str, msgspec.Raw])
    except OSError, msgspec.DecodeError:
        return dict.fromkeys(fields, "")

    def _field(field: str) -> str:
        try:
            return msgspec.json.decode(data[field], type=str) if field in data else ""
        except msgspec.DecodeError:
            return ""

    return {field: _field(field) for field in fields}


def _tsdecl_entry(pkg_dir: Path, manifest: dict[str, str]) -> Path | None:
    # package.json `types`/`typings` field wins over the `index.d.ts` convention.
    declared = manifest["types"] or manifest["typings"]
    target = (pkg_dir / declared) if declared else (pkg_dir / _DTS_ENTRY)
    return target if target.is_file() else None


def _pnpm_version(store: Path, mangled: str) -> str:
    # pnpm store directory name encodes the version as `{mangled}@version(+peer)(_hash)`; strip the peer/hash suffixes.
    return next((d.name.removeprefix(f"{mangled}@").split("+", 1)[0].split("_", 1)[0] for d in sorted(store.glob(f"{mangled}@*"))), "")


def _tsdecl_source(settings: AssaySettings, key: str) -> _Source | None:
    # Hoisted node_modules entry wins, pnpm store paths are fallbacks; a package with no .d.ts files yields EMPTY, not FAULTED.
    mangled = key.replace("/", "+")
    store = Path(str(settings.root)) / _PNPM_STORE
    candidates = (Path(str(settings.root)) / _NODE_MODULES / key, *sorted(store.glob(f"{mangled}@*/{_NODE_MODULES}/{key}")))
    match next((c for c in candidates if (c / "package.json").is_file() or any(c.glob(_DTS_GLOB))), None):
        case None:
            return None
        case pkg_dir:
            manifest = _json_fields(pkg_dir / "package.json", "types", "typings", "version")
            entry = _tsdecl_entry(pkg_dir, manifest)
            siblings = tuple(sorted((entry.parent if entry is not None else pkg_dir).glob(_DTS_GLOB)))
            assets = tuple(dict.fromkeys((*((entry,) if entry is not None else ()), *siblings)))
            version = _pnpm_version(store, mangled) or manifest["version"]
            return _Source(key=key, kind=SourceKind.TSDECL, version=version, package_root=pkg_dir, asset_paths=assets)


# --- [TABLES] ---------------------------------------------------------------------------

# SourceKind-keyed resolver order is the bundle > NuGet > pydist > tsdecl precedence; iteration takes the first hit.
_RESOLVE_TABLE: dict[SourceKind, _SourceResolver] = {
    SourceKind.ASSEMBLY: lambda settings, key, _packages_map: _host_source(settings, key),
    SourceKind.NUGET: lambda settings, key, packages_map: (
        _resolve_key(packages_map, key).map(lambda package: _nuget_source(settings, package, packages_map[package])).default_value(None)
    ),
    SourceKind.PYDIST: lambda _settings, key, _packages_map: _pydist_source(key),
    SourceKind.TSDECL: lambda settings, key, _packages_map: _tsdecl_source(settings, key),
}

# Fidelity is the single SourceKind->Fidelity correspondence (DERIVED_LOGIC): the surface stores no fidelity, every
# read derives it here, so it cannot drift. ASSEMBLY/NUGET decompile through ilspycmd; PYDIST is live introspection; TSDECL parses declarations.
_FIDELITY: dict[SourceKind, Fidelity] = {
    SourceKind.ASSEMBLY: Fidelity.DECOMPILED,
    SourceKind.NUGET: Fidelity.DECOMPILED,
    SourceKind.TOOL: Fidelity.DECOMPILED,
    SourceKind.PYDIST: Fidelity.INTROSPECTED,
    SourceKind.TSDECL: Fidelity.DECLARED,
}


def _resolve_source(settings: AssaySettings, key: str) -> Result[_Source, ApiResolution]:
    # Misses aggregate candidates across every resolver kind.
    packages = _packages(settings)

    def _attempt(resolver: _SourceResolver) -> _Source | None:
        # Empty/NUL keys can raise in metadata and glob resolvers; unknown stays a miss.
        try:
            return resolver(settings, key, packages)
        except ValueError, OSError:
            return None

    match next((source for resolver in _RESOLVE_TABLE.values() if (source := _attempt(resolver)) is not None), None):
        case _Source() as source:
            return Ok(source)
        case None:
            names = (*tuple(packages), *_pydist_names(), *_tsdecl_names(settings))
            return Error(ApiResolution(candidates=_rank_candidates(names, key), reason="unknown"))


def _clip(text: str, cap: int) -> tuple[str, bool]:
    # Slice detection: the bool marks a capture whose text was cut at the cap.
    return text[:cap], len(text) > cap


def _module_members(module: object, prefix: str, *, depth: int = 1) -> tuple[Capture, ...]:
    # depth=1 captures same-prefix submodules without walking the full transitive import graph.
    name = getattr(module, "__name__", prefix)
    file = str(getattr(module, "__file__", "") or "")
    own = tuple(
        Capture(name=cap, text=clipped, file=file, line=0, truncated=cut)
        for cap, predicate in _INSPECT_KINDS
        for ident, obj in inspect.getmembers(module, predicate)
        if not ident.startswith("_") and getattr(obj, "__module__", prefix).startswith(prefix)
        for clipped, cut in (_clip(f"{name}.{ident}", _NAME_CAP),)
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
    try:
        return importlib.import_module(name)
    except Exception:  # noqa: BLE001  # INPROC defensive: per-module import fault -> drop that module, the thunk still surfaces the rest
        return None


def _pydist_thunk(key: str, symbol: str) -> InprocThunk:
    def run(_check: Check) -> Completed:
        match symbol:
            case "":
                modules = tuple((name, _import(name)) for name in _pydist_modules(key))
                captures = tuple(cap for name, module in modules if module is not None for cap in _module_members(module, name))
                return receipt(("py-api", "surface", key), 0, stdout=CAPTURE_ENCODER.encode(captures))
            case _:
                obj = _resolve_py_symbol(key, symbol)
                member = _member_captures(obj, symbol) if obj is not None else ()
                return receipt(("py-api", "member", key, symbol), 0, stdout=CAPTURE_ENCODER.encode(member))

    return run


def _inproc_thunk(source: _Source, symbol: str) -> InprocThunk:
    return _pydist_thunk(source.key, symbol) if source.kind is SourceKind.PYDIST else _tsdecl_thunk(source, symbol)


def _resolve_py_symbol(key: str, symbol: str) -> object | None:
    # Candidate imports stay under declared distribution roots; the longest importable module prefix that walks to the attr wins.
    roots = _pydist_modules(key)
    qualified = tuple(dict.fromkeys((symbol, *(f"{root}.{symbol}" for root in roots if not symbol.startswith(f"{root}.") and symbol != root))))
    return next(
        (
            resolved
            for qname in qualified
            for parts in (tuple(qname.split(".")),)
            for i in range(len(parts), 0, -1)
            if parts[0] in roots
            for module in (_import(".".join(parts[:i])),)
            if module is not None
            for resolved in (_walk_attrs(module, parts[i:]),)
            if resolved is not None
        ),
        None,
    )


def _walk_attrs(root: object, parts: tuple[str, ...]) -> object | None:
    match parts:
        case (head, *tail):
            attr = getattr(root, head, None)
            return _walk_attrs(attr, tuple(tail)) if attr is not None else None
        case _:
            return root


def _member_captures(obj: object, symbol: str) -> tuple[Capture, ...]:
    sig, sig_cut = _clip(f"{symbol}{_signature(obj)}", _SIG_CAP)
    doc, doc_cut = _clip(inspect.getdoc(obj) or "", _SIG_CAP)
    full, full_cut = _clip(_object_source(obj) or _live_surface(obj, symbol), _NAME_CAP * 8)
    return (
        Capture(name="signature", text=sig, file=str(getattr(obj, "__module__", "")), line=0, truncated=sig_cut),
        Capture(name="doc", text=doc, file="", line=0, truncated=doc_cut),
        Capture(name="full", text=full, file="", line=0, truncated=full_cut),
    )


def _live_surface(obj: object, symbol: str) -> str:
    # Sourceless owners (C-extension classes/modules) reconstruct their public surface from live members,
    # so a member query stays a real extraction instead of degrading to the bare symbol name.
    if not (inspect.isclass(obj) or inspect.ismodule(obj)):
        return ""
    rows = tuple(
        f"{name}{_signature(member)}" if callable(member) else f"{name}: {type(member).__name__}"
        for name, member in inspect.getmembers(obj)
        if not name.startswith("_")
    )
    return "\n".join((f"{symbol}{_signature(obj)}:", *(f"    {row}" for row in rows))) if rows else ""


def _signature(obj: object) -> str:
    try:
        return str(
            inspect.signature(
                obj,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]  # any resolved symbol; non-callable -> TypeError arm
                annotation_format=annotationlib.Format.STRING,
            )
        )
    except ValueError, TypeError:
        annotations = _annotations(obj)
        params = tuple(f"{name}: {kind}" for name, kind in annotations.items() if name != "return")
        return f"({', '.join(params)})" if params else "(...)"


def _annotations(obj: object) -> dict[str, str]:
    try:
        return annotationlib.get_annotations(obj, format=annotationlib.Format.STRING)
    except TypeError, NameError:
        return {}


def _object_source(obj: object) -> str:
    try:
        # resolved symbol; C-builtin or unreadable object -> TypeError
        return inspect.getsource(obj)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    except OSError, TypeError:
        return ""


def _tsdecl_thunk(source: _Source, symbol: str) -> InprocThunk:
    def run(_check: Check) -> Completed:
        parser = TSParser(ts_language(_TS_GRAMMAR))  # parser is mutable: never cached, one per thunk run
        captures = tuple(cap for path in source.asset_paths for cap in _ts_captures(parser, path, symbol))
        return receipt(("ts-api", "member" if symbol else "surface", source.key, symbol), 0, stdout=CAPTURE_ENCODER.encode(captures))

    return run


def _ts_captures(parser: TSParser, path: Path, symbol: str) -> tuple[Capture, ...]:
    try:
        src = path.read_bytes()
    except OSError:
        return ()
    root = parser.parse(src).root_node
    is_dts = path.name.endswith(".d.ts")
    parse_fault = (Capture(name="parse_error", text="tree-sitter parse error", file=path.name, line=1, parse_error=True),) if root.has_error else ()
    match symbol:
        case "":
            return (*parse_fault, *_ts_declared(root, path, is_dts=is_dts), *_export_aliases(root, path))
        case _:
            return (*parse_fault, *_ts_member(root, symbol, path, is_dts=is_dts))


def _span_capture(name: str, text: str, node: Node, path: Path, *, truncated: bool = False) -> Capture:
    # tree-sitter points are 0-indexed; Capture line/column are 1-indexed.
    return Capture(
        name=name,
        text=text,
        file=path.name,
        line=node.start_point.row + 1,
        column=node.start_point.column + 1,
        end_line=node.end_point.row + 1,
        end_column=node.end_point.column + 1,
        start_byte=node.start_byte,
        end_byte=node.end_byte,
        truncated=truncated,
    )


def _ts_declared(root: Node, path: Path, *, is_dts: bool) -> tuple[Capture, ...]:
    # QueryError on a malformed roster query surfaces as a single capture, mirroring the code.py query rail.
    match ts_query(_TS_GRAMMAR, _TS_DECL_QUERY):
        case Result(tag="error", error=exc):
            return (Capture(name="query_error", text=str(exc)[:_NAME_CAP], file=path.name, line=1, parse_error=True),)
        case compiled:
            cursor = QueryCursor(compiled.ok)
    cursor.match_limit = _RESULT_CAP
    nodes = tuple(node for group in cursor.captures(root).values() for node in group if _exported(node, is_dts=is_dts))
    # Cursor match-limit or roster overflow marks every kept row as truncated.
    saturated = cursor.did_exceed_match_limit or len(nodes) > _RESULT_CAP
    return tuple(
        _span_capture(_TYPE_CAP, clipped, node, path, truncated=cut or saturated)
        for node in nodes[:_RESULT_CAP]
        for clipped, cut in (_clip(_node_text(node), _NAME_CAP),)
    )


def _ts_member(root: Node, symbol: str, path: Path, *, is_dts: bool) -> tuple[Capture, ...]:
    # Owner-qualified lookups search the owner first; flat lookups anchor to exported declarations.
    owner, _dot, target = symbol.rpartition(".")
    owner_node = next(
        (
            n
            for n in _walk_decls(root)
            if owner and (name := n.child_by_field_name("name")) is not None and _node_text(name) == owner and _exported(name, is_dts=is_dts)
        ),
        None,
    )
    node = (
        next((n for n in _walk_decls(owner_node) if (name := n.child_by_field_name("name")) is not None and _node_text(name) == target), None)
        if owner_node is not None
        else next(
            (
                n
                for n in _walk_decls(root)
                if (name := n.child_by_field_name("name")) is not None and _node_text(name) == target and _exported(name, is_dts=is_dts)
            ),
            None,
        )
    )
    match node:
        case None:
            return _export_member(root, target, path)
        case node:
            full = _node_text(node)
            sig, sig_cut = _clip(full.splitlines()[0] if full else "", _SIG_CAP)
            doc, doc_cut = _clip(_ts_doc(node), _SIG_CAP)
            return (
                _span_capture("signature", sig, node, path, truncated=sig_cut),
                Capture(name="full", text=full[:_FULL_CAP], file=path.name, line=node.start_point.row + 1, truncated=len(full) > _FULL_CAP),
                *((Capture(name="doc", text=doc, file=path.name, line=node.start_point.row + 1, truncated=doc_cut),) if doc else ()),
            )


def _ts_doc(node: Node) -> str:
    # The TSDoc block is the comment immediately preceding the declaration's top-level statement.
    top = next((p for p in _parents(node) if p.parent is not None and p.parent.type == "program"), node)
    prev = top.prev_sibling
    return _node_text(prev).strip() if prev is not None and prev.type == "comment" and _node_text(prev).startswith("/**") else ""


def _exported(node: Node, *, is_dts: bool) -> bool:
    # Ambient .d.ts top-level declarations count as exported, with one ambient wrapper admitted.
    chain = tuple(p.type for p in _parents(node))
    ambient_depth1 = is_dts and chain[1:] in {("program",), ("ambient_declaration", "program")}
    return "export_statement" in chain or ambient_depth1


def _parents(node: Node) -> tuple[Node, ...]:
    match node.parent:
        case None:
            return ()
        case parent:
            return (parent, *_parents(parent))


def _export_aliases(root: Node, path: Path) -> tuple[Capture, ...]:
    return tuple(
        _span_capture(_TYPE_CAP, clipped, spec, path, truncated=cut)
        for spec in _walk(root, "export_specifier")
        if _export_name(spec)
        for clipped, cut in (_clip(_export_name(spec), _NAME_CAP),)
    )


def _export_member(root: Node, target: str, path: Path) -> tuple[Capture, ...]:
    return tuple(
        _span_capture("signature", clipped, spec, path, truncated=cut)
        for spec in _walk(root, "export_specifier")
        if _export_name(spec) == target
        for clipped, cut in (_clip(_node_text(spec), _SIG_CAP),)
    )[:1]


def _export_name(node: Node) -> str:
    names = tuple(child for child in node.children if child.type in {"identifier", "type_identifier"})
    return _node_text(names[-1]) if names else ""


def _walk(node: Node, kind: str) -> tuple[Node, ...]:
    own = (node,) if node.type == kind else ()
    return (*own, *(d for child in node.children for d in _walk(child, kind)))


def _walk_decls(node: Node) -> tuple[Node, ...]:
    own = (node,) if node.type in _DECL_NODES else ()
    return (*own, *(d for child in node.children for d in _walk_decls(child)))


def _invoke(settings: AssaySettings, scope: ArtifactScope, tool: Tool, *args: str) -> Completed:
    # scope=None preserves the real dotnet-tools.json CLI home for `dotnet tool run ilspycmd`.
    _ = scope
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED)
    check = Check(tool=msgspec.structs.replace(tool, command=(*tool.command, *args)))
    match run_check(check, settings=settings, scope=None, routed=routed):
        case Result(tag="ok", ok=done):
            return done
        case Result(error=fault):
            return Completed((*tool.runner.prefix, *tool.command, *args), 1, stderr=fault.message.encode())


def _surface(settings: AssaySettings, scope: ArtifactScope, source: _Source) -> Result[_Surface, Fault]:  # noqa: PLR0912  # one arm per SourceKind; the axis is closed
    # Content-fingerprint cache avoids repeat listings; the strategy splits on source.kind (ilspycmd subprocess vs INPROC thunk).
    store = settings.store()
    match source.kind:
        case SourceKind.ASSEMBLY | SourceKind.NUGET:
            assemblies = source.assemblies
            cache = _cache_path(settings, source, _fingerprint(assemblies) if assemblies else "")
            match next((t for t in select(Claim.API, Language.CSHARP)), None):
                case None:
                    return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message="no ilspycmd catalog row"))
                case Tool() as tool:
                    # No assemblies → EMPTY surface, never FAULTED; a well-formed cache replays the prior ilspycmd listing,
                    # while a truncated/garbage cache fails the guard and rebuilds, so a no-match becomes provable absence.
                    cached = store.read_text_path(cache) if assemblies and store.exists_path(cache) else ""
                    return (
                        Ok(_Surface(source, (), (), {}, cache, ""))
                        if not assemblies
                        else Ok(_parse_surface(source, cache, cached))
                        if _cache_valid(cached)
                        else _run_surface(settings, scope, source, tool, assemblies, cache)
                    )
        case SourceKind.PYDIST | SourceKind.TSDECL:
            # No asset_paths → EMPTY, not FAULTED.
            cache = _cache_path(settings, source, _fingerprint(source.asset_paths))
            if store.exists_path(cache):
                return Ok(_parse_inproc(source, cache, store.read_text_path(cache).encode()))

            def _persist(done: Completed) -> _Surface:
                store.write_text_path(done.stdout.decode(errors="replace"), cache)
                return _parse_inproc(source, cache, done.stdout)

            return _inproc_check(settings, source, Mode.QUERY, _inproc_thunk(source, "")).map(_persist)
        case SourceKind.TOOL:  # pragma: no cover  # TOOL never resolves through _resolve_source
            return Ok(_Surface(source, (), (), {}, _cache_path(settings, source, ""), ""))
        case never:  # pragma: no cover
            assert_never(never)


def _inproc_check(settings: AssaySettings, source: _Source, mode: Mode, thunk: InprocThunk) -> Result[Completed, Fault]:
    # Thunks run through run_check to share the deadline, rate limiter, and trace span with subprocess rails.
    language = Language.PYTHON if source.kind is SourceKind.PYDIST else Language.TYPESCRIPT
    match next((t for t in select(Claim.API, language) if t.mode is mode), None):
        case None:
            return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message=f"no {language.value} INPROC api row"))
        case Tool() as tool:
            check = Check(tool=msgspec.structs.replace(tool, thunk=thunk))
            routed = Routed(language=language, scope=Scope.CHANGED)
            return run_check(check, settings=settings, scope=None, routed=routed)


def _parse_inproc(source: _Source, cache: str, stdout: bytes) -> _Surface:
    types = tuple(dict.fromkeys(cap.text for cap in CAPTURES.decode(stdout or b"[]") if cap.name == _TYPE_CAP and cap.text))
    return _roster(source, cache, types, stdout.decode(errors="replace"))


def _cache_path(settings: AssaySettings, source: _Source, fingerprint: str) -> str:
    # Co-located under the per-key dir alongside this source's report artifacts, not a flat sibling of every other key.
    return settings.store().path(ArtifactKind.SCOPE.value, "api", _safe(source.key), f"{fingerprint or 'unresolved'}.txt")


def _run_surface(
    settings: AssaySettings, scope: ArtifactScope, source: _Source, tool: Tool, assemblies: tuple[Path, ...], cache: str
) -> Result[_Surface, Fault]:
    attempts = tuple((asm, _invoke(settings, scope, tool, str(asm))) for asm in assemblies)
    match any(done.returncode == 0 for _, done in attempts):
        case False:
            detail = "\n".join(d.stderr.decode(errors="replace") for _, d in attempts if d.stderr) or "ilspycmd type listing failed"
            return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message=detail[:1024]))
        case True:
            listing = "\n".join(f"# {asm}\n{done.stdout.decode(errors='replace')}" for asm, done in attempts if done.stdout)
            parsed = _parse_surface(source, cache, listing)
            # A legitimately typeless assembly caches the empty-surface sentinel so a future read validates instead of rebuilding forever.
            text = listing if parsed.types else f"{listing}\n{_EMPTY_SURFACE}".lstrip()
            settings.store().write_text_path(text, cache)
            return Ok(parsed)


def _namespace_of(fqn: str, types: frozenset[str]) -> str:
    # The longest known owner prefix becomes the namespace.
    parts = fqn.split(".")
    return next((".".join(parts[:i]) for i in range(len(parts)) if ".".join(parts[: i + 1]) in types), ".".join(parts[:-1]))


def _cache_valid(text: str) -> bool:
    # Positive well-formedness, not non-emptiness: a cache validates iff it holds at least one roster line whose first
    # token is a surface kind, or the explicit empty-surface sentinel; a truncated/garbage cache fails and rebuilds.
    return any(
        line.split(maxsplit=1)[0] in _SURFACE_KINDS or line == _EMPTY_SURFACE
        for line in text.splitlines()
        if line.strip() and not line.startswith("# ")
    )


def _parse_surface(source: _Source, cache: str, text: str) -> _Surface:
    types = tuple(
        dict.fromkeys(
            parts[1]
            for line in text.splitlines()
            if line.strip() and not line.startswith("# ")
            for parts in (line.split(maxsplit=1),)
            if len(parts) == 2 and parts[0] in _SURFACE_KINDS and not _NOISE.search(parts[1])
        )
    )
    return _roster(source, cache, types, text)


def _roster(source: _Source, cache: str, types: tuple[str, ...], raw: str) -> _Surface:
    type_set = frozenset(types)
    namespace_of = {fqn: _namespace_of(fqn, type_set) for fqn in types}
    namespaces = tuple(sorted({ns for ns in namespace_of.values() if ns}))
    by_namespace = {ns: tuple(fqn for fqn in types if namespace_of[fqn] == ns) for ns in namespaces}
    return _Surface(source=source, types=types, namespaces=namespaces, by_namespace=by_namespace, cache=cache, raw=raw)


def _rank_type(types: tuple[str, ...], needle: str) -> str:
    # Exact type names beat the shortest matching suffix.
    casefold = needle.casefold()
    matched = tuple(fqn for fqn in types if fqn.casefold() == casefold or fqn.casefold().endswith("." + casefold))
    ranked = sorted(matched, key=lambda fqn: (fqn.casefold() != casefold, len(fqn)))
    return next(iter(ranked), "")


def _xml_doc(source: _Source, symbol: str) -> str:
    # Strips the XMLDoc kind prefix (M:/T:) and parameter list, then matches on a dotted-segment boundary.
    needle = symbol.casefold()
    return next(
        (
            "".join(member.itertext()).strip()[:_SIG_CAP]
            for path in source.xmls
            if path.is_file()
            for member in (_xml_members(path))
            for name in (member.get("name", "").split(":", 1)[-1].split("(", 1)[0].casefold(),)
            if name == needle or name.endswith("." + needle)
        ),
        "",
    )


def _xml_members(path: Path) -> tuple[ET.Element[str], ...]:
    try:
        root = ET.fromstring(path.read_bytes())  # noqa: S314  # trusted local sidecar .xml from the resolved source, never network-sourced
    except ET.ParseError:
        return ()
    return tuple(root.iterfind(".//member"))


def _decompile(
    settings: AssaySettings, scope: ArtifactScope, surface: _Surface, symbol: str, shape: SymbolShape, p: ApiParams
) -> Result[Report, Fault]:
    match surface.source.kind:
        case SourceKind.ASSEMBLY | SourceKind.NUGET:
            return _cs_decompile(settings, scope, surface, symbol, shape, p)
        case SourceKind.PYDIST | SourceKind.TSDECL:
            return _inproc_decompile(settings, scope, surface, symbol, shape, p)
        case SourceKind.TOOL:  # pragma: no cover  # TOOL never resolves through _resolve_source
            return Ok(_search_report(settings, scope, surface, p))
        case never:  # pragma: no cover
            assert_never(never)


def _cs_decompile(  # noqa: PLR0914  # decompile pipeline uses all 14 locals as independent pipeline stages
    settings: AssaySettings, scope: ArtifactScope, surface: _Surface, symbol: str, shape: SymbolShape, p: ApiParams
) -> Result[Report, Fault]:
    head, _, tail = symbol.rpartition(".")
    fqn = _rank_type(surface.types, symbol) or _rank_type(surface.types, head)
    match next((t for t in select(Claim.API, Language.CSHARP)), None):
        case None:
            return Error(Fault(("api", "decompile", symbol), status=RailStatus.FAULTED, message="no ilspycmd catalog row"))
        case Tool() if not fqn:
            return Ok(_decompile_report(settings, scope, surface, shape, "", "", "", "", 0, truncated=False, p=p))
        case Tool() as tool:
            done = _run_decompile(settings, scope, tool, fqn, surface.source.assemblies)
            text = done.stdout.decode(errors="replace")
            lines = tuple(line for line in text.splitlines() if not p.grep or p.grep.casefold() in line.casefold())
            boundary = re.compile(rf"\b{re.escape(tail or symbol.rsplit('.', 1)[-1])}\b", re.IGNORECASE)
            declared = tuple(off for off, line in enumerate(lines) if boundary.search(line) and not line.lstrip().startswith("///"))
            anchor = next(iter(declared), 0)
            window = lines if p.full else lines[anchor : anchor + p.max_lines]
            signature = next((lines[off].strip() for off in declared), next(iter(window), "").strip())
            sig = signature if done.returncode == 0 and text else ""
            selected = len(lines)
            truncated = not p.full and len(lines) > len(window)
            return Ok(
                _decompile_report(
                    settings,
                    scope,
                    surface,
                    shape,
                    sig,
                    _xml_doc(surface.source, symbol),
                    "\n".join(window),
                    text,
                    selected,
                    truncated=truncated,
                    p=p,
                )
            )


def _inproc_decompile(
    settings: AssaySettings, scope: ArtifactScope, surface: _Surface, symbol: str, shape: SymbolShape, p: ApiParams
) -> Result[Report, Fault]:
    # Docs come from thunk captures (inspect.getdoc / TSDoc comment), not XMLDoc sidecar lookup.
    source = surface.source

    def _build(done: Completed) -> Report:
        captured = {cap.name: cap.text for cap in CAPTURES.decode(done.stdout or b"[]")}
        signature = captured.get("signature", "")
        full = captured.get("full", "") or signature
        lines = tuple(line for line in full.splitlines() if not p.grep or p.grep.casefold() in line.casefold())
        window = lines if p.full else lines[: p.max_lines]
        return _decompile_report(
            settings,
            scope,
            surface,
            shape,
            signature,
            captured.get("doc", ""),
            "\n".join(window),
            full,
            len(lines),
            truncated=not p.full and len(lines) > len(window),
            p=p,
        )

    return _inproc_check(settings, source, Mode.LIST, _inproc_thunk(source, symbol)).map(_build)


def _run_decompile(settings: AssaySettings, scope: ArtifactScope, tool: Tool, fqn: str, assemblies: tuple[Path, ...]) -> Completed:
    # Ref assemblies precede lib assemblies; first non-empty successful decompile wins.
    ordered = sorted(assemblies, key=lambda a: ("/ref/" not in a.as_posix(), a.as_posix().casefold()))
    decompile_tool = msgspec.structs.replace(tool, command=(*(c for c in tool.command if c not in {"-l", "cisde"}), "-t", fqn, *_DECOMPILE_FLAGS))
    attempts = tuple(_invoke(settings, scope, decompile_tool, str(asm)) for asm in ordered)
    return next((d for d in attempts if d.returncode == 0 and d.stdout), attempts[0] if attempts else Completed(("ilspycmd",), 1))


def _artifact(settings: AssaySettings, source: _Source, name: str, content: str) -> Artifact:
    raw = content.encode()
    path = settings.store().write_bytes(raw, ArtifactKind.SCOPE.value, "api", _safe(source.key), name)
    digest = hashlib.sha256(f"{path}|{name}".encode()).hexdigest()[:12]
    return Artifact(id=digest, kind=ArtifactKind.SCOPE, path=str(path), bytes=len(raw), lines=len(content.splitlines()))


def _api_source(source: _Source, *, status: RailStatus | None = None, selected: tuple[Path, ...] = ()) -> ApiSource:
    assemblies = tuple(str(p) for p in source.assemblies)
    xmls = tuple(str(p) for p in source.xmls)
    assets = tuple(str(p) for p in source.asset_paths)
    package_root = str(source.package_root) if source.package_root is not None else ""
    return ApiSource(
        source_kind=source.kind,
        source_id=source.key,
        version=source.version,
        package=source.key if source.kind in _PACKAGE_KINDS else "",
        primary_assembly=assemblies[0] if assemblies else "",
        primary_xml=xmls[0] if xmls else "",
        assemblies=assemblies,
        xmls=xmls,
        assets=assets,
        package_root=package_root,
        nuspec=str(source.nuspec) if source.nuspec is not None else "",
        frameworks=source.frameworks,
        owners=source.owners,
        restore="restored" if package_root else ("missing" if source.kind is SourceKind.NUGET else ""),
        status=status or (RailStatus.OK if source.assemblies or source.asset_paths or source.package_root else RailStatus.EMPTY),
        selected=tuple(str(p) for p in selected),
    )


def _matches(rows: tuple[str, ...], kind: ArtifactKind, pattern: str) -> tuple[tuple[Match, ...], tuple[str, ...]]:
    # Identity-shaped ids keep delta comparisons stable across result order changes.
    query = pattern.casefold()

    def score(text: str) -> int:
        low = text.casefold()
        base = 100 if query and query == low else 70 if query and low.startswith(query) else 40 if query and query in low else 0
        char_sim = int(difflib.SequenceMatcher(None, query, low).ratio() * 30) if query else 0
        return base + char_sim + max(0, 20 - len(text) // 4)

    filtered = tuple(r for r in rows if not pattern or query in r.casefold())
    scored = sorted(((r, score(r)) for r in filtered), key=lambda x: (-x[1], x[0]))
    shown = tuple(Match(id=f"{kind.value}:{r[:_NAME_CAP]}", kind=kind, text=r[:_NAME_CAP], score=s) for r, s in scored[:_RESULT_CAP])
    return shown, _cap_note(len(shown), len(scored), _RESULT_CAP)


# --- [COMPOSITION] ----------------------------------------------------------------------


def status(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """Inventory API source health.

    Returns:
        Inventory report, or a strict-mode fault when required sources are incomplete.
    """
    surface_tool = next((t for t in select(Claim.API, Language.CSHARP)), None)
    version = _invoke(settings, scope, surface_tool, "--version") if surface_tool is not None else Completed(("ilspycmd",), 1)
    ilspy_ver = (version.stdout.decode(errors="replace").splitlines()[0].strip() if version.stdout else "") or "unavailable"
    sources = _filtered_sources(_inventory_sources(settings, _rhino_app(settings), ilspy_ver, version.returncode), p.sources)
    healthy = tuple(s for s in sources if s.status is RailStatus.OK)
    artifacts = _inventory_artifacts(settings, sources)
    compact = _compact_sources(sources)
    done = Completed(
        ("api", "status"),
        0,
        status=RailStatus.OK if sources else RailStatus.EMPTY,
        notes=(f"{len(healthy)}/{len(sources)} inventory sources healthy",),
    )
    detail = ApiSurface(
        source=next((s for s in sources if s.source_id == "ilspycmd"), ApiSource(source_kind=SourceKind.TOOL, source_id="ilspycmd")),
        shape=SymbolShape.INDEX,
        preview="\n".join(
            f"{s.status.value}\t{s.source_kind.value}\t{s.source_id}\t{s.version or s.package_root or s.primary_assembly}" for s in compact
        ),
        lines=len(sources),
        selected=len(healthy),
        artifact_paths=tuple(a.path for a in artifacts),
    )
    results = tuple(
        Match(
            id=f"inventory:{source.source_id}",
            kind=ArtifactKind.SCOPE,
            text=_status_row_text(source),
            score=100 if source.status is RailStatus.OK else 0,
            severity=None if source.status is RailStatus.OK else "missing",
        )
        for source in compact
    )
    return _strict(
        msgspec.structs.replace(fold(Claim.API, "status", (done,), detail=detail), artifacts=artifacts, results=results),
        p,
        tuple(s for s in sources if s.source_id in _REQUIRED_SOURCE_IDS and s.status is not RailStatus.OK),
    )


def _filtered_sources(all_sources: tuple[ApiSource, ...], prefixes: tuple[str, ...]) -> tuple[ApiSource, ...]:
    return tuple(s for s in all_sources if any(s.source_id.startswith(prefix) for prefix in prefixes)) if prefixes else all_sources


def _inventory_artifacts(settings: AssaySettings, sources: tuple[ApiSource, ...]) -> tuple[Artifact, ...]:
    # One durable inventory artifact: the structured JSON. The compact preview rides the envelope; the TSV had no reader.
    json_raw = _ENCODER.encode(sources)
    json_path = settings.store().write_bytes(json_raw, Claim.API.value, settings.run_id, "status-inventory.json")
    return (Artifact(id="status-inventory-json", kind=ArtifactKind.SCOPE, path=json_path, bytes=len(json_raw), lines=len(sources)),)


def _status_row_text(source: ApiSource) -> str:
    """Project one inventory source into the stable ``key=value`` health grammar.

    Grammar (single-space-separated, fixed key order):
    ``<source_id> status=<status> assembly=present|missing xml=present|missing version=<version|->``.

    Returns:
        The keyed health row text consumed by Match parsers and pinned by the status row-text law.
    """
    presence = (
        ("assembly", "present" if source.primary_assembly or source.package_root else "missing"),
        ("xml", "present" if source.primary_xml else "missing"),
        ("version", source.version or "-"),
    )
    return f"{source.source_id} status={source.status.value} " + " ".join(f"{key}={value}" for key, value in presence)


def _inventory_sources(settings: AssaySettings, app: Path | None, ilspy_ver: str, returncode: int) -> tuple[ApiSource, ...]:
    roots = _root_inventory_sources(settings, app, ilspy_ver, returncode)
    nugets = _nuget_inventory_sources(settings)
    polyglot = _polyglot_inventory_sources(settings)
    return (*roots, *nugets, *polyglot)


def _root_inventory_sources(settings: AssaySettings, app: Path | None, ilspy_ver: str, returncode: int) -> tuple[ApiSource, ...]:
    app_source = ApiSource(
        source_kind=SourceKind.ASSEMBLY,
        source_id="rhino-app",
        package_root=str(app) if app is not None else "",
        status=RailStatus.OK if app is not None and app.is_dir() else RailStatus.EMPTY,
    )
    tool_source = ApiSource(
        source_kind=SourceKind.TOOL, source_id="ilspycmd", version=ilspy_ver, status=RailStatus.OK if returncode == 0 else RailStatus.EMPTY
    )
    hosts = tuple(
        _api_source(src, status=RailStatus.OK if src.assemblies else RailStatus.EMPTY)
        for key in _HOST_SPECS
        for src in (_host_source(settings, key),)
        if src is not None
    )
    return (app_source, tool_source, *hosts)


def _nuget_inventory_sources(settings: AssaySettings) -> tuple[ApiSource, ...]:
    packages = _packages(settings)
    owners = _package_owner_index(settings)
    return tuple(
        _api_source(_nuget_source(settings, package, version, include_assets=False, owners=owners.get(package.casefold(), ())), status=None)
        for package, version in sorted(packages.items())
    )


def _polyglot_inventory_sources(settings: AssaySettings) -> tuple[ApiSource, ...]:
    pydists = _pydist_inventory_sources()
    pydist_names = tuple(source.source_id for source in pydists)
    tsdecl_names = _tsdecl_names(settings)
    pydist_summary = ApiSource(source_kind=SourceKind.PYDIST, source_id="python-dists", status=RailStatus.OK if pydist_names else RailStatus.EMPTY)
    tsdecl_summary = ApiSource(source_kind=SourceKind.TSDECL, source_id="ts-decls", status=RailStatus.OK if tsdecl_names else RailStatus.EMPTY)
    tsdecls = tuple(
        src for name in tsdecl_names for source in (_tsdecl_source(settings, name),) if source is not None for src in (_api_source(source),)
    )
    return (pydist_summary, tsdecl_summary, *pydists, *tsdecls)


def _compact_sources(sources: tuple[ApiSource, ...]) -> tuple[ApiSource, ...]:
    return tuple(source for source in sources if source.source_kind in _COMPACT_VISIBLE or source.source_id in _COMPACT_SUMMARY_IDS)


def _strict(report: Report, p: ApiParams, core_absent: tuple[ApiSource, ...] = ()) -> Result[Report, Fault]:
    match (p.strict, report.status):
        case (True, _) if core_absent:
            absent = ", ".join(s.source_id for s in core_absent)
            return Error(Fault(("api", report.verb), status=RailStatus.FAULTED, message=f"required sources absent: {absent}"))
        case (True, status) if status is not RailStatus.OK:
            return Error(Fault(("api", report.verb), status=RailStatus.FAULTED, message=f"{report.verb} incomplete under --strict"))
        case _:
            return Ok(report)


def resolve(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """Resolve a source key to asset paths.

    Returns:
        Asset path report, or unsupported-source candidate evidence.
    """
    _ = scope
    match _resolve_source(settings, p.key):
        case Result(tag="ok", ok=source):
            return _strict(_resolve_report(settings, source, p), p)
        case Result(error=resolution):
            return _strict(_miss_report("resolve", p.key, resolution), p)


def _resolve_report(settings: AssaySettings, source: _Source, p: ApiParams) -> Report:
    # Full path list rides an artifact; ranked previews ride results.
    if p.kind not in _PATH_KINDS:
        done = Completed(("api", "resolve", p.key, p.kind), 0, status=RailStatus.UNSUPPORTED, notes=(f"unknown kind: {p.kind}",))
        return fold(
            Claim.API, "resolve", (done,), detail=ApiResolution(candidates=tuple((kind, 100) for kind in sorted(_PATH_KINDS)), reason="unknown-kind")
        )
    targets = _resolve_targets(source, p.kind)
    existing = tuple(t for t in targets if t.exists())
    results, cap_notes = _matches(tuple(str(t) for t in targets), ArtifactKind.SCOPE, "")
    # The full path list rides an artifact only when ranked results saturate the cap; small sets ride inline.
    artifact = _artifact(settings, source, f"{p.kind}.paths.txt", "\n".join(str(t) for t in targets)) if cap_notes else None
    note = f"{len(existing)}/{len(targets)} {p.kind} paths present"
    done = Completed(("api", "resolve", p.key), 0, status=RailStatus.OK if existing else RailStatus.EMPTY, notes=(note, *cap_notes))
    detail = _api_detail(
        source,
        SymbolShape.SEARCH,
        preview="\n".join(str(t) for t in existing[:_PREVIEW_ROWS]),
        lines=len(targets),
        selected=len(existing),
        artifact_paths=(artifact.path,) if artifact is not None else (),
    )
    report = fold(Claim.API, "resolve", (done,), detail=detail)
    return msgspec.structs.replace(report, artifacts=(artifact,) if artifact is not None else (), results=results)


def _resolve_targets(source: _Source, kind: _PathKind) -> tuple[Path, ...]:
    catalog: dict[_PathKind, tuple[Path, ...]] = {
        "all": (*source.assemblies, *source.xmls, *((source.nuspec,) if source.nuspec is not None else ()), *source.asset_paths),
        "assembly": source.assemblies,
        "xml": source.xmls,
        "nuspec": (source.nuspec,) if source.nuspec is not None else (),
        "deps": tuple(p for p in source.asset_paths if any(part in _DEPS_PARTS for part in p.parts)),
        "package-root": (source.package_root,) if source.package_root is not None else (),
    }
    return tuple(dict.fromkeys(catalog[kind]))


def query(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """Query a source for namespaces, types, members, or search hits.

    Returns:
        API surface report, or unsupported-source candidate evidence.
    """
    match _resolve_source(settings, p.key):
        case Result(tag="ok", ok=source):
            return (
                _surface(settings, scope, source)
                .bind(lambda surface: _query_shape(settings, scope, surface, p))
                .bind(lambda report: _strict(report, p))
            )
        case Result(error=resolution):
            return _strict(_miss_report("query", p.key, resolution), p)


def _resolve_namespace(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, p: ApiParams) -> Result[Report, Fault]:
    # Type-suffix match wins over exact namespace roster; no match falls through to search.
    type_fqn = _rank_type(surface.types, p.symbol)
    owned = surface.by_namespace.get(_rank_namespace(surface, p.symbol), ()) if not type_fqn else ()
    return (
        _decompile(settings, scope, surface, type_fqn, SymbolShape.TYPE, p)
        if type_fqn
        else Ok(_roster_report(settings, surface, SymbolShape.NAMESPACE, owned, p))
        if owned
        else Ok(_search_report(settings, scope, surface, p))
    )


def _query_shape(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, p: ApiParams) -> Result[Report, Fault]:
    # Namespace-shaped symbols still decompile when they also match a type suffix.
    match shape_of(p.symbol):
        case SymbolShape.INDEX:
            rows = surface.namespaces or surface.types
            return Ok(_roster_report(settings, surface, SymbolShape.INDEX, rows, p))
        case SymbolShape.NAMESPACE:
            return _resolve_namespace(settings, scope, surface, p)
        case SymbolShape.TYPE | SymbolShape.MEMBER as shape:
            return _decompile(settings, scope, surface, p.symbol, shape, p)
        case SymbolShape.SEARCH:  # pragma: no cover  # shape_of never emits SEARCH; the decompile-miss path routes through _decompile_report
            return Ok(_search_report(settings, scope, surface, p))
        case never:  # pragma: no cover
            assert_never(never)


def _rank_namespace(surface: _Surface, symbol: str) -> str:
    casefold = symbol.casefold()
    return next((ns for ns in surface.namespaces if ns.casefold() == casefold), symbol)


def _roster_report(settings: AssaySettings, surface: _Surface, shape: SymbolShape, rows: tuple[str, ...], p: ApiParams) -> Report:
    # INDEX's sole durable artifact is the content-fingerprinted surface cache (unfiltered source output); other shapes
    # write a .txt only when ranked results saturate the cap, so small rosters ride the envelope with zero artifacts.
    source = surface.source
    results, cap_notes = _matches(rows, ArtifactKind.SCOPE, p.grep)
    store = settings.store()
    cache = _cache_artifact(store, surface.cache) if shape is SymbolShape.INDEX and store.exists_path(surface.cache) else None
    artifact = _artifact(settings, source, f"{shape.value}.txt", "\n".join(rows)) if cap_notes and shape is not SymbolShape.INDEX else None
    artifacts = tuple(a for a in (artifact, cache) if a is not None)
    done = Completed(
        ("api", "query", source.key),
        0,
        status=RailStatus.OK if rows else RailStatus.EMPTY,
        notes=(f"{len(surface.types)} types across {len(surface.namespaces)} namespaces", _fidelity_note(source), *cap_notes),
    )
    detail = _api_detail(
        source,
        shape,
        preview="\n".join(rows[:_PREVIEW_ROWS]),
        truncated=len(rows) > _PREVIEW_ROWS,
        lines=len(rows),
        selected=len(rows),
        artifact_paths=tuple(a.path for a in artifacts),
    )
    return msgspec.structs.replace(fold(Claim.API, "query", (done,), detail=detail), artifacts=artifacts, results=results)


def _search_report(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, p: ApiParams) -> Report:
    # Misses carry nearest candidates so callers can route to suggestions.
    source = surface.source
    needle = p.symbol.casefold()
    hits = tuple(fqn for fqn in surface.types if needle in fqn.casefold())
    match hits:
        case () if p.grep and source.kind in {SourceKind.ASSEMBLY, SourceKind.NUGET}:
            # No type hit but a --grep member needle: fan out ilspycmd -t over the cap-bounded, relevance-ranked candidate types.
            return _grep_member_report(settings, scope, surface, p)
        case ():
            done = Completed(("api", "query", source.key), 0, status=RailStatus.UNSUPPORTED, notes=(f"no match for '{p.symbol}'",))
            return fold(Claim.API, "query", (done,), detail=ApiResolution(candidates=_rank_candidates(surface.types, p.symbol), reason="partial"))
        case _:
            results, cap_notes = _matches(hits, ArtifactKind.SCOPE, p.symbol)
            done = Completed(
                ("api", "query", source.key), 0, status=RailStatus.OK, notes=(f"{len(hits)} search hits", _fidelity_note(source), *cap_notes)
            )
            # The artifact carries the full hit set only when results saturate the cap; small sets ride inline.
            artifact = _artifact(settings, source, "search.txt", "\n".join(hits)) if cap_notes else None
            detail = _api_detail(
                source,
                SymbolShape.SEARCH,
                preview="\n".join(hits[:_PREVIEW_ROWS]),
                truncated=len(hits) > _PREVIEW_ROWS,
                lines=len(hits),
                selected=len(hits),
                artifact_paths=(artifact.path,) if artifact is not None else (),
            )
            artifacts = (artifact,) if artifact is not None else ()
            return msgspec.structs.replace(fold(Claim.API, "query", (done,), detail=detail), artifacts=artifacts, results=results)


def _grep_miss(surface: _Surface, p: ApiParams, note: str) -> Report:
    done = Completed(("api", "query", surface.source.key), 0, status=RailStatus.UNSUPPORTED, notes=(note,))
    return fold(Claim.API, "query", (done,), detail=ApiResolution(candidates=_rank_candidates(surface.types, p.grep), reason="partial"))


def _grep_hits(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, tool: Tool, p: ApiParams) -> tuple[str, ...]:
    # _CANDIDATE_CAP is the explosion guard: name-relevance ranks first; a pure member needle resembling no type name
    # falls back to the first cap roster types, so a broad needle decompiles at most _CANDIDATE_CAP candidates, one assembly each.
    needle = p.grep.casefold()
    candidates = tuple(name for name, _score in _rank_candidates(surface.types, p.grep, n=_CANDIDATE_CAP)) or surface.types[:_CANDIDATE_CAP]
    return tuple(
        f"{fqn} :: {line.strip()}"
        for fqn in candidates
        for done in (_run_decompile(settings, scope, tool, fqn, surface.source.assemblies),)
        if done.returncode == 0
        for line in done.stdout.decode(errors="replace").splitlines()
        if needle in line.casefold() and not line.lstrip().startswith("///")
    )


def _grep_member_report(settings: AssaySettings, scope: ArtifactScope, surface: _Surface, p: ApiParams) -> Report:
    source = surface.source
    tool = next((t for t in select(Claim.API, Language.CSHARP)), None)
    hits = _grep_hits(settings, scope, surface, tool, p) if tool is not None else ()
    match (tool, hits):
        case (None, _):
            return _grep_miss(surface, p, "no ilspycmd catalog row")
        case (_, ()):
            return _grep_miss(surface, p, f"no member match for '{p.grep}'")
        case _:
            results, cap_notes = _matches(hits[:_RESULT_CAP], ArtifactKind.SCOPE, p.grep)
            artifact = _artifact(settings, source, "grep-members.txt", "\n".join(hits)) if len(hits) > _RESULT_CAP or cap_notes else None
            done = Completed(
                ("api", "query", source.key), 0, status=RailStatus.OK, notes=(f"{len(hits)} member hits", _fidelity_note(source), *cap_notes)
            )
            detail = _api_detail(
                source,
                SymbolShape.SEARCH,
                preview="\n".join(hits[:_PREVIEW_ROWS]),
                truncated=len(hits) > _PREVIEW_ROWS,
                lines=len(hits),
                selected=len(hits),
                artifact_paths=(artifact.path,) if artifact is not None else (),
            )
            artifacts = (artifact,) if artifact is not None else ()
            return msgspec.structs.replace(fold(Claim.API, "query", (done,), detail=detail), artifacts=artifacts, results=results)


def _decompile_report(  # noqa: PLR0913,PLR0914,PLR0917  # all slots are structural caller positions shared across C# and INPROC paths
    settings: AssaySettings,
    scope: ArtifactScope,
    surface: _Surface,
    shape: SymbolShape,
    signature: str,
    xml: str,
    window: str,
    full: str,
    selected: int,
    *,
    truncated: bool,
    p: ApiParams,
) -> Report:
    # Empty signatures try namespace roster before falling to fuzzy search.
    match signature:
        case "":
            ns_key = _rank_namespace(surface, p.symbol)
            owned = surface.by_namespace.get(ns_key, ()) if ns_key != p.symbol or ns_key in surface.by_namespace else ()
            return _roster_report(settings, surface, SymbolShape.NAMESPACE, owned, p) if owned else _search_report(settings, scope, surface, p)
        case _:
            direct_fqn = _rank_type(surface.types, p.symbol)
            head = p.symbol.rpartition(".")[0]
            resolved_fqn = direct_fqn or (_rank_type(surface.types, head) if head else "")
            is_member = shape is SymbolShape.MEMBER or bool(head and resolved_fqn and not direct_fqn)
            final_shape = SymbolShape.MEMBER if is_member else SymbolShape.TYPE
            suffix = {SourceKind.PYDIST: ".py", SourceKind.TSDECL: ".d.ts"}.get(surface.source.kind, ".cs")
            # The full decompiled body rides an artifact only when the inline window truncates; untruncated output rides the preview.
            artifact = _artifact(settings, surface.source, f"decompile{suffix}", full) if truncated else None
            detail = _api_detail(
                surface.source,
                final_shape,
                signature=signature,
                doc=xml,
                preview=window,
                member=p.symbol.rpartition(".")[2] if is_member else "",
                truncated=truncated,
                lines=selected,
                selected=selected,
                artifact_paths=(artifact.path,) if artifact is not None else (),
            )
            shown = len(window.splitlines())
            window_note = (f"window: {shown} of {selected} lines (--full or --max-lines to widen)",) if truncated else ()
            note = Completed(
                ("api", "query", surface.source.key),
                0,
                status=RailStatus.OK,
                notes=(f"{selected} selected lines", _fidelity_note(surface.source), *window_note),
            )
            identity = (resolved_fqn or p.symbol)[:_NAME_CAP]
            result = Match(id=f"{final_shape.value}:{identity}", kind=ArtifactKind.SCOPE, text=identity, score=100)
            artifacts = (artifact,) if artifact is not None else ()
            return msgspec.structs.replace(fold(Claim.API, "query", (note,), detail=detail), artifacts=artifacts, results=(result,))


def _api_detail(  # noqa: PLR0913  # single ApiSurface constructor surface; keyword-only slots prevent positional confusion
    source: _Source,
    shape: SymbolShape,
    *,
    signature: str = "",
    doc: str = "",
    preview: str = "",
    member: str = "",
    truncated: bool = False,
    lines: int = 0,
    selected: int = 0,
    artifact_paths: tuple[str, ...] = (),
) -> ApiSurface:
    return ApiSurface(
        source=_api_source(source),
        shape=shape,
        signature=signature,
        doc=doc,
        preview=preview,
        member=member,
        truncated=truncated,
        lines=lines,
        selected=selected,
        artifact_paths=artifact_paths,
    )


def _fidelity_note(source: _Source) -> str:
    # Fidelity is derived from SourceKind through the single _FIDELITY map and rides Report.notes, never stored on the surface.
    return f"fidelity: {_FIDELITY[source.kind].value}"


def _miss_report(verb: str, key: str, resolution: ApiResolution) -> Report:
    # The nearest keys ride the note inline so a notes-first reader self-corrects without opening detail.
    nearest = ", ".join(name for name, _ in resolution.candidates[:3])
    note = f"no '{key}' source; {resolution.reason}" + (f", nearest: {nearest}" if nearest else "")
    done = Completed(("api", verb, key), 0, status=RailStatus.UNSUPPORTED, notes=(note,))
    return fold(Claim.API, verb, (done,), detail=resolution)


def show(settings: AssaySettings, scope: ArtifactScope, p: ApiParams) -> Result[Report, Fault]:
    """Preview a previously written API artifact.

    Returns:
        Artifact preview report, or an empty report when the artifact is absent.
    """
    _ = scope
    store = settings.store()
    match _show_targets(store, p):
        case (path, *rest):
            return _strict(_show_store_report(store, path, p, matched=1 + len(rest)), p)
        case _:
            done = Completed(("api", "show", p.token), 0, status=RailStatus.EMPTY, notes=(f"artifact not found: {p.token}",))
            return _strict(fold(Claim.API, "show", (done,)), p)


def _show_store_report(store: ArtifactStore, path: str, p: ApiParams, *, matched: int = 1) -> Report:
    text = store.read_text_path(path)
    window, total, truncated = _slice(text, lines=p.lines, grep=p.grep, max_lines=p.max_lines, full=p.full)
    digest = hashlib.sha256(f"{path}|{p.token}".encode()).hexdigest()[:12]
    artifact = Artifact(
        id=digest, kind=ArtifactKind.SCOPE, path=path, bytes=store.size_path(path, fallback=len(text.encode())), lines=len(text.splitlines())
    )
    source = ApiSource(source_kind=SourceKind.TOOL, source_id=p.token, assets=(path,), status=RailStatus.OK, selected=(path,))
    detail = ApiSurface(
        source=source, shape=SymbolShape.SEARCH, preview=window, truncated=truncated, lines=total, selected=total, artifact_paths=(path,)
    )
    notes = (
        f"{total} selected lines",
        *((f"window: {len(window.splitlines())} of {total} lines (--full or --max-lines to widen)",) if truncated else ()),
        *(f"{matched} artifact matches; selected newest" for _ in range(1) if matched > 1),
    )
    done = Completed(("api", "show", p.token), 0, status=RailStatus.OK, notes=notes)
    return msgspec.structs.replace(fold(Claim.API, "show", (done,), detail=detail), artifacts=(artifact,))


def _show_targets(store: ArtifactStore, p: ApiParams) -> tuple[str, ...]:
    return store.resolve_artifacts(
        p.token,
        Claim.API.value,
        f"{ArtifactKind.SCOPE.value}/api",
        ArtifactKind.SCOPE.value,
        ArtifactKind.HISTORY.value,
        ArtifactKind.PROCESS.value,
        ArtifactKind.TEST.value,
        ArtifactKind.CODE.value,
        latest=p.token == _LATEST_ARTIFACT,
    )


def _cache_artifact(store: ArtifactStore, path: str) -> Artifact:
    text = store.read_text_path(path)
    return Artifact(
        id=hashlib.sha256(f"{path}|surface".encode()).hexdigest()[:12],
        kind=ArtifactKind.SCOPE,
        path=path,
        bytes=store.size_path(path, fallback=len(text.encode())),
        lines=len(text.splitlines()),
    )


def _slice(text: str, *, lines: str, grep: str, max_lines: int, full: bool) -> tuple[str, int, bool]:
    # --lines windows after grep; --full overrides both.
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

__all__ = ["ApiParams", "query", "resolve", "shape_of", "show", "status"]
