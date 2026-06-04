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

from dataclasses import dataclass
import hashlib
from pathlib import Path
import re
from typing import assert_never
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local Directory.Packages.props XML, never network-sourced

from expression import Error, Ok, Result
import msgspec

from tools.assay.composition.catalog import select  # intra-package import; tools.assay is the package root
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
    Language,
    Match,
    Report,  # noqa: TC001  # unconditional so beartype @checked resolves the rail's -> Result[Report, Fault] forward-ref under PEP 649
    SourceKind,
    SymbolShape,
    Tool,
)
from tools.assay.core.routing import Routed, Scope  # intra-package import; tools.assay is the package root
from tools.assay.core.status import RailStatus  # intra-package import; tools.assay is the package root


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


def _source(settings: AssaySettings, key: str) -> Result[_Source, ApiResolution]:
    """Resolve one source key to its typed ``_Source``: host bundle first, else fuzzy NuGet.

    A miss rides the ``Error`` channel as a typed ``ApiResolution`` (nearest scored candidates + reason),
    which the ``resolve``/``query`` handlers fold onto an ``UNSUPPORTED`` ``Report`` — the richer-on-failure
    contract: the agent gets the next-step disambiguation, never a bare empty result or a broken-operation 2.
    """
    match _host_source(settings, key):
        case _Source() as host:
            return Ok(host)
        case None:
            packages = _packages(settings)
            return _resolve_key(packages, key).map(lambda package: _nuget_source(settings, package, packages[package]))


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
    """Resolve the type/namespace roster, cache-first by content fingerprint.

    A cache hit skips ilspycmd entirely; a miss runs ``-l cisde`` over each assembly. An
    assembly-empty source folds ``EMPTY`` (off-host / unrestored) so a speculative poll exits 0. A
    non-zero exit on a *present* assembly is a tool fault (``FAULTED``, exit 2): assay could not surface
    the API, so the stderr rides the ``Fault`` message on the ``Error`` channel — distinct (per the
    ``Fault`` contract) from a check that ran and found defects (``FAILED``, exit 1, the success channel).
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
    type_set = frozenset(types)
    namespace_of = {fqn: _namespace_of(fqn, type_set) for fqn in types}
    namespaces = tuple(sorted({ns for ns in namespace_of.values() if ns}))
    return _Surface(
        source=source,
        types=types,
        namespaces=namespaces,
        by_namespace={ns: tuple(fqn for fqn in types if namespace_of[fqn] == ns) for ns in namespaces},
        cache=cache,
        raw=text,
    )


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
    """Decompile one type via ``ilspycmd -t <type>`` and anchor the member signature.

    Member tokens drop their final segment to find the owning type. ``signature`` anchors on the
    modifier-prefixed declaration line of the simple identifier — never a ``///`` doc-comment
    occurrence, which a bare word boundary over-matches into a sibling overload. An unresolvable type
    or empty decompile yields an empty ``signature`` so the ``query`` fold falls through to SEARCH.
    """
    head, _, tail = symbol.rpartition(".")
    fqn = _rank_type(surface.types, symbol) or _rank_type(surface.types, head)
    match next((t for t in select(Claim.API, Language.CSHARP)), None):
        case None:
            return Error(Fault(("api", "decompile", symbol), status=RailStatus.FAULTED, message="no ilspycmd catalog row"))
        case Tool() if not fqn:
            return Ok(_Body(signature="", xml="", window="", full="", selected=0, truncated=False))
        case Tool() as tool:
            done = _run_decompile(settings, scope, tool, fqn, surface.source.assemblies)
            return Ok(_body(done, tail or symbol.rsplit(".", 1)[-1], symbol, surface.source, p))


def _run_decompile(settings: AssaySettings, scope: ArtifactScope, tool: Tool, fqn: str, assemblies: tuple[Path, ...]) -> Completed:
    """Decompile ``fqn`` across the assemblies (ref-first), returning the first non-empty success (else first attempt)."""
    ordered = sorted(assemblies, key=lambda a: ("/ref/" not in a.as_posix(), a.as_posix().casefold()))
    decompile_tool = msgspec.structs.replace(tool, command=(*(c for c in tool.command if c not in {"-l", "cisde"}), "-t", fqn, *_DECOMPILE_FLAGS))
    attempts = tuple(_invoke(settings, scope, decompile_tool, str(asm)) for asm in ordered)
    return next((d for d in attempts if d.returncode == 0 and d.stdout), attempts[0] if attempts else Completed(("ilspycmd",), 1))


def _body(done: Completed, simple: str, symbol: str, source: _Source, p: ApiParams) -> _Body:
    """Window the decompiled text and anchor the signature on the declaration line (excluding doc comments)."""
    text = done.stdout.decode(errors="replace")
    lines = tuple(line for line in text.splitlines() if not p.grep or p.grep.casefold() in line.casefold())
    boundary = re.compile(rf"\b{re.escape(simple)}\b", re.IGNORECASE)
    declared = tuple(off for off, line in enumerate(lines) if boundary.search(line) and not line.lstrip().startswith("///"))
    anchor = next(iter(declared), 0)
    window = lines if p.full else lines[anchor : anchor + p.max_lines]
    signature = next((lines[off].strip() for off in declared), next(iter(window), "").strip())
    return _Body(
        signature=signature if done.returncode == 0 and text else "",
        xml=_xml_doc(source, symbol),
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
    """Fold the host bundle, ilspycmd version, and central package map into ``(ok, note)`` inventory rows."""
    packages = _packages(settings)
    host_keys = tuple(k for k in _HOST_SPECS if (src := _host_source(settings, k)) is not None and src.assemblies)
    return (
        _present("rhino-app", app is not None and app.is_dir(), str(app) if app is not None else ""),
        _present("ilspycmd", version.returncode == 0, version.stdout.decode(errors="replace").strip() or "unavailable"),
        _present("host-sources", bool(host_keys), ",".join(host_keys)),
        _present("nuget-sources", bool(packages), str(len(packages))),
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
            return Ok(_roster_report(settings, surface, SymbolShape.INDEX, surface.namespaces, "namespace", p))
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
