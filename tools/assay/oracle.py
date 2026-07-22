"""Own the api-claim source boundary: adapters, the ilspy port, the typed surface cache, and TFM ranking.

Resolution precedence is host bundle, NuGet cache, Python dist-info, then node_modules declarations. Four frozen
adapters satisfy the ``Oracle`` protocol (probe/resolve/surface/member); ``rails/api.py`` composes them into verb
reports. Every C# spawn is a catalog row filled through typed splice slots; the surface cache is a typed entry
carrying producer identity, probed producer version, and content _fingerprint.
"""

import annotationlib  # PEP 749: STRING annotations avoid evaluating unresolvable forward refs
from dataclasses import dataclass, field
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
from typing import override, Protocol, runtime_checkable, TYPE_CHECKING, TypeAliasType
import xml.etree.ElementTree as ET  # noqa: S405  # trusted local MSBuild XML, never network-sourced

from expression import Error, Ok, Result
import msgspec
from tree_sitter import Parser as TSParser, QueryCursor
import tree_sitter_typescript

from tools.assay.composition.catalog import select
from tools.assay.composition.settings import AssaySettings  # noqa: TC001  # beartype resolves adapter annotations at runtime
from tools.assay.composition.store import ArtifactScope  # noqa: TC001  # beartype resolves adapter annotations at runtime
from tools.assay.core.exec import Executor  # noqa: TC001  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.model import (
    ApiResolution,
    ApiSource,
    ArtifactKind,
    Base,
    Check,
    Claim,
    Completed,
    Fault,
    Language,
    Mode,
    RailStatus,
    receipt,
    RESULT_CAP,
    SourceKind,
    Tool,
    ToolArgs,
)
from tools.assay.core.routing import parse_csproj, Routed, Scope
from tools.assay.diagnostics import Capture, CAPTURE_ENCODER, CAPTURES, node_text, ts_language, ts_query


if TYPE_CHECKING:
    from collections.abc import Callable

    from tree_sitter import Node
    from upath import UPath

    from tools.assay.core.model import InprocThunk

    type SourceResolver = Callable[[AssaySettings, str, dict[str, str]], Source | None]


# --- [TYPES] ----------------------------------------------------------------------------

type PathKind = str  # resolve kind token: all | assembly | xml | nuspec | deps | package-root


class Fidelity(StrEnum):
    """Decompilation fidelity of an API surface, derived from its SourceKind."""

    DECOMPILED = "decompiled"  # ilspycmd IL->C# reconstruction (assembly, nuget)
    INTROSPECTED = "introspected"  # live inspect of an imported Python distribution
    DECLARED = "declared"  # parsed .d.ts ambient declarations


class _Outcome(StrEnum):
    """Classification of one ilspycmd decompile attempt.

    MISS is the typed "Could not find type definition" verdict regardless of exit code; FAULT covers every
    other nonzero exit or exit-0 stderr fault, so a tool failure never masquerades as a symbol miss.
    """

    HIT = "hit"  # exit 0 with decompiled source on stdout
    MISS = "miss"  # type definition absent from this assembly's type system
    EMPTY = "empty"  # clean exit, empty output, silent stderr: a genuine soft miss
    FAULT = "fault"  # operational failure: nonzero exit or an exit-0 stderr fault


@runtime_checkable
class Oracle(Protocol):
    """API boundary port: one frozen adapter per source provenance."""

    @property
    def source(self) -> Source:
        """The resolved source this adapter answers for."""
        ...

    def probe(self) -> ApiSource:
        """Project the source's health/inventory row."""
        ...

    def resolve(self, kind: PathKind) -> tuple[Path, ...]:
        """Project the source's concrete asset paths for one kind token."""
        ...

    def surface(self, scope: ArtifactScope) -> Result[Surface, Fault]:
        """List the source's type roster, replaying the typed fingerprint cache when valid."""
        ...

    def member(self, scope: ArtifactScope, surface: Surface, symbol: str) -> Result[Completed, Fault]:
        """Return the raw member-evidence receipt for one symbol."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

CANDIDATE_CAP: int = 8
PATH_KINDS: frozenset[PathKind] = frozenset(("all", "assembly", "xml", "nuspec", "deps", "package-root"))
_HOST_SPECS: dict[str, tuple[str, str]] = {
    "eto": ("Eto.dll", "Eto.xml"),
    "eto-macos": ("Eto.macOS.dll", ""),
    "gh2": ("ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/Grasshopper2.xml"),
    "gh2-io": ("ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.dll", "ManagedPlugIns/Grasshopper2Plugin.rhp/GrasshopperIO.xml"),
    "microsoft-macos": ("Microsoft.macOS.dll", ""),
    "rhino-code": ("Rhino.Runtime.Code.dll", ""),
    "rhino-code-remote": ("Rhino.Runtime.Code.Remote.dll", ""),
    "rhino-common": ("RhinoCommon.dll", "RhinoCommon.xml"),
    "rhino-ui": ("Rhino.UI.dll", "Rhino.UI.xml"),
}
HOST_KEYS: frozenset[str] = frozenset(_HOST_SPECS)
_RHINO_BUNDLE: str = "/Applications/RhinoWIP.app"
_RESOURCE_ROOT: str = "Contents/Frameworks/RhCore.framework/Versions/Current/Resources"
_BUILD_PROPS: str = "Directory.Build.props"
_PACKAGES_PROPS: str = "Directory.Packages.props"
_NUGET_ROOTS: tuple[str, ...] = (".cache/nuget/packages", str(Path.home() / ".nuget/packages"))
_ASSET_DIRS: tuple[str, ...] = ("lib", "ref", "runtimes", "build", "buildTransitive", "analyzers", "tools")
_DEPS_PARTS: frozenset[str] = frozenset(("build", "buildTransitive", "analyzers", "tools", "runtimes"))
_SURFACE_KINDS: frozenset[str] = frozenset(("Class", "Struct", "Interface", "Delegate", "Enum"))
# cisde renders open generics by bare name (no arity marker, no angle bracket); every `<` in a type name is a
# compiler synthetic: `<Module>`, `<>c`/`<>c__DisplayClass`, `<>f__AnonymousType`, `<MethodName>d__NN` iterators,
# and `<PrivateImplementationDetails>`. Filtering on the angle bracket drops synthetics and never a legitimate generic.
_NOISE: re.Pattern[str] = re.compile(r"<")
# Consumer TFM floor fallback when Directory.Build.props omits TargetFramework; the props value wins when present.
_TFM_FLOOR_FALLBACK: tuple[int, int] = (10, 0)
_TFM_MODERN: re.Pattern[str] = re.compile(r"^net(\d+)\.(\d+)$")
_TFM_NETCOREAPP: re.Pattern[str] = re.compile(r"^netcoreapp(\d+)\.(\d+)$")
_TFM_NETSTANDARD: re.Pattern[str] = re.compile(r"^netstandard(\d+)\.(\d+)$")
_TFM_NETFRAMEWORK: re.Pattern[str] = re.compile(r"^net(4\d{1,2})$")
_TARGET_FRAMEWORK: re.Pattern[str] = re.compile(r"<TargetFramework>\s*net(\d+)\.(\d+)\s*</TargetFramework>")
# ilspycmd's typed not-found verdict; it can ride stderr under any exit code, so classification keys on the text.
_ILSPY_MISS: str = "Could not find type definition"
# ilspycmd decompiler-crash markers (an ICSharpCode.Decompiler transform bug on one method aborts the whole
# type); stderr tail-clips under the stream cap, so the trace-frame marker — present throughout the stack —
# detects a crash whose "Error decompiling" head was clipped. The rail retries the attempt at the downgraded
# language version, whose simpler transform pipeline decompiles the type cleanly, and stamps the fallback note.
_ILSPY_CRASH: tuple[str, ...] = ("Error decompiling", "ICSharpCode.Decompiler")
_ILSPY_LV_FALLBACK: tuple[str, ...] = ("-lv", "CSharp7_3")
# XMLDoc ids arity-mark generics (`N types, ``N methods); the CLR reflection tail is single-backtick only.
_ARITY: re.Pattern[str] = re.compile(r"`+\d+")
_ARITY_TAIL: re.Pattern[str] = re.compile(r"^(?P<base>.+)`(?P<n>\d+)$")
# ECMA-335 #~ table schemas: column tokens are u2/u4 fixed, S/G/B heap indexes, I:<tid> simple indexes, and
# C:<family> coded indexes. Row widths derive from these schemas so the reader can skip every table between
# TypeDef (0x02) and NestedClass (0x29); an unknown present table id aborts the parse fail-open.
_MD_TABLES: dict[int, tuple[str, ...]] = {
    0x00: ("u2", "S", "G", "G", "G"),
    0x01: ("C:ResolutionScope", "S", "S"),
    0x02: ("u4", "S", "S", "C:TypeDefOrRef", "I:04", "I:06"),
    0x03: ("I:04",),
    0x04: ("u2", "S", "B"),
    0x05: ("I:06",),
    0x06: ("u4", "u2", "u2", "S", "B", "I:08"),
    0x07: ("I:08",),
    0x08: ("u2", "u2", "S"),
    0x09: ("I:02", "C:TypeDefOrRef"),
    0x0A: ("C:MemberRefParent", "S", "B"),
    0x0B: ("u2", "C:HasConstant", "B"),
    0x0C: ("C:HasCustomAttribute", "C:CustomAttributeType", "B"),
    0x0D: ("C:HasFieldMarshal", "B"),
    0x0E: ("u2", "C:HasDeclSecurity", "B"),
    0x0F: ("u2", "u4", "I:02"),
    0x10: ("u4", "I:04"),
    0x11: ("B",),
    0x12: ("I:02", "I:14"),
    0x13: ("I:14",),
    0x14: ("u2", "S", "C:TypeDefOrRef"),
    0x15: ("I:02", "I:17"),
    0x16: ("I:17",),
    0x17: ("u2", "S", "B"),
    0x18: ("u2", "I:06", "C:HasSemantics"),
    0x19: ("I:02", "C:MethodDefOrRef", "C:MethodDefOrRef"),
    0x1A: ("S",),
    0x1B: ("B",),
    0x1C: ("u2", "C:MemberForwarded", "S", "I:1A"),
    0x1D: ("u4", "I:04"),
    0x1E: ("u4", "u4"),
    0x1F: ("u4",),
    0x20: ("u4", "u2", "u2", "u2", "u2", "u4", "B", "S", "S"),
    0x21: ("u4",),
    0x22: ("u4", "u4", "u4"),
    0x23: ("u2", "u2", "u2", "u2", "u4", "B", "S", "S", "B"),
    0x24: ("u4", "I:23"),
    0x25: ("u4", "u4", "u4", "I:23"),
    0x26: ("u4", "S", "B"),
    0x27: ("u4", "u4", "S", "S", "C:Implementation"),
    0x28: ("u4", "u4", "S", "C:Implementation"),
    0x29: ("I:02", "I:02"),
    0x2A: ("u2", "u2", "C:TypeOrMethodDef", "S"),
    0x2B: ("C:MethodDefOrRef", "B"),
    0x2C: ("I:2A", "C:TypeDefOrRef"),
}
# Coded-index families (ECMA-335 II.24.2.6); zero entries hold unused tag slots so tag-bit widths stay exact.
_MD_FAMILIES: dict[str, tuple[int, ...]] = {
    "TypeDefOrRef": (0x02, 0x01, 0x1B),
    "HasConstant": (0x04, 0x08, 0x17),
    "HasCustomAttribute": (
        *(0x06, 0x04, 0x01, 0x02, 0x08, 0x09, 0x0A, 0x00, 0x0E, 0x17, 0x14),
        *(0x11, 0x1A, 0x1B, 0x20, 0x23, 0x26, 0x27, 0x28, 0x2A, 0x2C, 0x2B),
    ),
    "HasFieldMarshal": (0x04, 0x08),
    "HasDeclSecurity": (0x02, 0x06, 0x20),
    "MemberRefParent": (0x02, 0x01, 0x1A, 0x06, 0x1B),
    "HasSemantics": (0x14, 0x17),
    "MethodDefOrRef": (0x06, 0x0A),
    "MemberForwarded": (0x04, 0x06),
    "Implementation": (0x26, 0x23, 0x27),
    "CustomAttributeType": (0, 0, 0x06, 0x0A, 0),
    "ResolutionScope": (0x00, 0x1A, 0x23, 0x01),
    "TypeOrMethodDef": (0x02, 0x06),
}
_MD_MAGIC: int = 0x424A5342
_MD_FIXED: dict[str, int] = {"u2": 2, "u4": 4}
_MD_HEAP_BIT: dict[str, int] = {"S": 1, "G": 2, "B": 4}
# Surface-cache file shape (`<16-hex>.json` or `unresolved.json`); the write-time reaper removes only these.
_FINGERPRINT_FILE: re.Pattern[str] = re.compile(r"(?:[0-9a-f]{16}|unresolved)\.json")
# Producer identities stamped into typed cache entries; a mismatched producer is a cache miss, never a parse guess.
_ILSPY_PRODUCER: str = "ilspycmd"
_PY_PRODUCER: str = "py-api"
_TS_PRODUCER: str = "ts-api"
# Roster-grammar dispatch keyed on the probed ilspycmd version; the catch-all row is the stable cisde grammar.
# A future grammar break adds a version-pattern row here instead of sniffing output shape.
_ILSPY_VERSION: re.Pattern[str] = re.compile(r"(\d+(?:\.\d+)+)")
_PACKAGE_KINDS: frozenset[SourceKind] = frozenset((SourceKind.NUGET, SourceKind.PYDIST, SourceKind.TSDECL))  # key is also a package name
_NODE_MODULES: str = "node_modules"
_PNPM_STORE: str = "node_modules/.pnpm"  # pnpm mangles @scope/pkg as @scope+pkg
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
_TS_GRAMMAR: Callable[[], object] = tree_sitter_typescript.language_typescript
_DECL_NODES: frozenset[str] = frozenset((
    "class_declaration",
    "abstract_class_declaration",
    "interface_declaration",
    "type_alias_declaration",
    "enum_declaration",
    "internal_module",
    "function_signature",
    "method_signature",
    "property_signature",
    "public_field_definition",
    "variable_declarator",
))
# Flat symbol lookups prefer type-level declarations over value/member-level ones sharing the name, so an
# interface's `Effect` property never shadows the `Effect` type the query names; document order breaks ties.
_DECL_RANK: dict[str, int] = {
    "class_declaration": 0,
    "abstract_class_declaration": 0,
    "interface_declaration": 0,
    "type_alias_declaration": 0,
    "enum_declaration": 0,
    "internal_module": 0,
    "function_signature": 1,
    "variable_declarator": 1,
    "method_signature": 2,
    "property_signature": 2,
    "public_field_definition": 2,
}
_RANK_EXPORT: int = 8  # export-alias fallback outranks nothing but a miss
_RANK_NONE: int = 99  # no declaration and no export alias in this file
# Declaration-node kinds projected onto the member-truth band a report consumer reads typed.
_TS_KIND: dict[str, str] = {
    "class_declaration": "class",
    "abstract_class_declaration": "class",
    "interface_declaration": "interface",
    "type_alias_declaration": "type-alias",
    "enum_declaration": "enum",
    "internal_module": "namespace",
    "function_signature": "function",
    "variable_declarator": "const",
    "method_signature": "method",
    "property_signature": "property",
    "public_field_definition": "field",
}
_EXPORT_SPEC: frozenset[str] = frozenset(("export_specifier",))
_TYPE_CAP: str = "type"  # roster capture-name vocabulary: every INPROC/tree-sitter declaration row caps under this name
_INSPECT_KINDS: tuple[tuple[str, Callable[[object], bool]], ...] = (
    (_TYPE_CAP, inspect.isclass),
    (_TYPE_CAP, inspect.isfunction),
    (_TYPE_CAP, lambda obj: isinstance(obj, TypeAliasType)),  # PEP 695 `type` aliases surface alongside classes and functions
)
NAME_CAP: int = 320
_SIG_CAP: int = 480
_FULL_CAP: int = 2560  # full .d.ts member body capture

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Source:
    """Resolved API source with its concrete assets; ``tfm`` is the consumer-bound framework for NuGet sources."""

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
    tfm: str = ""


@dataclass(frozen=True, slots=True)
class Surface:
    """Type roster listed from one source, with its cache path, raw listing, and reflection-name map.

    ``reflection`` maps each display FQN (dotted, arity-free, as the roster renders it) to its CLR reflection
    names (backtick arity, ``+`` nested separators) read from assembly metadata; one display name fans to
    several reflection names when arities collide. Empty for INPROC sources.
    """

    source: Source
    types: tuple[str, ...]
    namespaces: tuple[str, ...]
    by_namespace: dict[str, tuple[str, ...]]
    cache: str
    raw: str
    reflection: dict[str, tuple[str, ...]] = field(default_factory=dict)


class CacheEntry(Base, frozen=True):
    """Typed surface-cache record: producer identity, probed producer version, input fingerprint, and payload.

    A read replays only when the decoded entry's producer and fingerprint match the current source; grammar
    sniffing over raw text is dead. An empty payload is a legitimately typeless surface, never a rebuild loop.
    """

    producer: str
    version: str
    fingerprint: str
    payload: str


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RANKING]


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


def rank_candidates(haystack: tuple[str, ...], needle: str, *, n: int = CANDIDATE_CAP) -> tuple[tuple[str, int], ...]:
    """Rank nearest-name candidates for a miss.

    Returns:
        Up to ``n`` positive-scored (name, score) rows, best first.
    """
    ranked = sorted(((name, _score_name(name, needle)) for name in haystack), key=lambda row: (-row[1], row[0]))
    return tuple((name, sc) for name, sc in ranked[:n] if sc > 0)


def rank_type(types: tuple[str, ...], needle: str) -> str:
    """Rank a type roster against a needle.

    Returns:
        The exact-name match, else the shortest dotted-suffix match, else ``""``.
    """
    casefold = needle.casefold()
    matched = tuple(fqn for fqn in types if fqn.casefold() == casefold or fqn.casefold().endswith("." + casefold))
    ranked = sorted(matched, key=lambda fqn: (fqn.casefold() != casefold, len(fqn)))
    return next(iter(ranked), "")


def rank_namespace(surface: Surface, symbol: str) -> str:
    """Rank a namespace roster against a symbol, folding exact case-insensitive hits.

    Returns:
        The canonical namespace spelling, or the symbol unchanged when no roster row matches.
    """
    casefold = symbol.casefold()
    return next((ns for ns in surface.namespaces if ns.casefold() == casefold), symbol)


# --- [TFM_POLICY]


def _props_digest(path: Path | UPath) -> str | None:
    # Content digest keying the props lru_caches; None marks an unreadable file for the caller's fallback.
    try:
        return hashlib.sha256(path.read_bytes()).hexdigest()[:16]
    except OSError:
        return None


@lru_cache(maxsize=8)
def _tfm_floor_at(path_str: str, digest: str) -> tuple[int, int]:
    _ = digest  # lru_cache key slot: content hash, immune to mtime-preserving rewrites
    match _TARGET_FRAMEWORK.search(Path(path_str).read_text(encoding="utf-8", errors="replace")):
        case None:
            return _TFM_FLOOR_FALLBACK
        case found:
            return (int(found.group(1)), int(found.group(2)))


def consumer_tfm_floor(settings: AssaySettings) -> tuple[int, int]:
    """Read the workspace consumer TargetFramework floor from ``Directory.Build.props``.

    Returns:
        (major, minor) of the workspace ``<TargetFramework>``, or the net10.0 fallback when absent.
    """
    path = settings.root / _BUILD_PROPS
    digest = _props_digest(path)
    return _TFM_FLOOR_FALLBACK if digest is None else _tfm_floor_at(str(path), digest)


def tfm_rank(tfm: str, floor: tuple[int, int]) -> tuple[int, int, int] | None:
    """Rank one ``lib/<tfm>`` candidate against the consumer floor under NuGet precedence.

    Tiers: modern ``net{M}.{m}`` at or below the floor beat ``netcoreapp``, which beat ``netstandard``,
    which beat classic ``net4x``; within a tier a higher version wins. A modern TFM above the floor is
    incompatible with the consumer and never selected while any compatible candidate exists.

    Returns:
        Sort key (tier, -major, -minor) where lower sorts first, or ``None`` for an incompatible/foreign TFM.
    """
    match _TFM_MODERN.match(tfm), _TFM_NETCOREAPP.match(tfm), _TFM_NETSTANDARD.match(tfm), _TFM_NETFRAMEWORK.match(tfm):
        case (found, _, _, _) if found is not None:
            version = (int(found.group(1)), int(found.group(2)))
            return (0, -version[0], -version[1]) if version <= floor else None
        case (_, found, _, _) if found is not None:
            return (1, -int(found.group(1)), -int(found.group(2)))
        case (_, _, found, _) if found is not None:
            return (2, -int(found.group(1)), -int(found.group(2)))
        case (_, _, _, found) if found is not None:
            return (3, -int(found.group(1)), 0)
        case _:
            return None


def _ranked_tfm_dirs(root: Path, asset: str, floor: tuple[int, int]) -> tuple[Path, ...]:
    base = root / asset
    frameworks = tuple(p for p in base.iterdir() if p.is_dir()) if base.is_dir() else ()
    compatible = sorted((p for p in frameworks if tfm_rank(p.name, floor) is not None), key=lambda p: (tfm_rank(p.name, floor), p.name))
    # A package with zero compatible TFMs still resolves (sorted fallback) so `resolve` keeps reporting its assets.
    return tuple(compatible) or tuple(sorted(frameworks))


def _select_tfm_dir(root: Path, floor: tuple[int, int]) -> Path | None:
    # ref/ wins over lib/ for the same reason the compiler binds reference assemblies first.
    return next((ranked[0] for asset in ("ref", "lib") for ranked in (_ranked_tfm_dirs(root, asset, floor),) if ranked), None)


def _frameworks(root: Path, floor: tuple[int, int]) -> tuple[str, ...]:
    found = {p.name for asset in ("ref", "lib") for base in (root / asset,) if base.is_dir() for p in base.iterdir() if p.is_dir()}
    compatible = sorted((name for name in found if tfm_rank(name, floor) is not None), key=lambda name: (tfm_rank(name, floor), name))
    return (*compatible, *sorted(name for name in found if tfm_rank(name, floor) is None))


# --- [RESOLUTION]


def safe_key(key: str) -> str:
    """Sanitize a source key into an artifact path segment.

    Returns:
        The key with non-portable characters folded to ``-``, never empty.
    """
    return re.sub(r"[^A-Za-z0-9_.-]+", "-", key).strip("-") or "source"


@lru_cache(maxsize=256)
def _asm_digest(path_str: str, size: int, mtime_ns: int) -> str:  # noqa: ARG001  # size+mtime_ns are lru_cache key slots, not used in the body
    # RhinoWIP reinstalls can preserve DLL mtimes, so content-hash is the discriminant.
    return hashlib.sha256(Path(path_str).read_bytes()).hexdigest()


def _fingerprint(paths: tuple[Path, ...]) -> str:
    # 16-hex content fingerprint (path, size, mtime, digest) keying the typed surface cache.
    seed = "|".join(
        f"{p}:{st.st_size}:{st.st_mtime_ns}:{_asm_digest(str(p), st.st_size, st.st_mtime_ns)}" for p in paths if p.is_file() for st in (p.stat(),)
    )
    return hashlib.sha256(seed.encode()).hexdigest()[:16]


def rhino_app(settings: AssaySettings) -> Path | None:
    """Locate the Rhino app bundle: worktree ``rhino-app`` symlink, then ``RHINO_WIP_APP_PATH``, then the installed RhinoWIP bundle.

    Returns:
        Bundle path, or ``None`` when no bundle is present.
    """
    candidates = (  # bundle resolution is local-fs; UPath -> Path; mirrors the rhino-bridge host contract
        Path(str(settings.root)) / "rhino-app",
        *((Path(settings.rhino_wip_app_path),) if settings.rhino_wip_app_path else ()),
        Path(_RHINO_BUNDLE),
    )
    return next((c for c in candidates if c.is_dir()), None)


def _host_source(settings: AssaySettings, key: str) -> Source | None:
    # Empty sources let status report absent bundles instead of hiding the row.
    match _HOST_SPECS.get(key):
        case None:
            return None
        case (asm_name, xml_name):
            app = rhino_app(settings)
            resources = (app / _RESOURCE_ROOT) if app is not None else None
            assemblies = (resources / asm_name,) if resources is not None and asm_name else ()
            xmls = (resources / xml_name,) if resources is not None and xml_name else ()
            return Source(
                key=key, kind=SourceKind.ASSEMBLY, assemblies=tuple(a for a in assemblies if a.is_file()), xmls=tuple(x for x in xmls if x.is_file())
            )


def host_sources(settings: AssaySettings) -> tuple[Source, ...]:
    """Resolve every host-bundle spec into a source row.

    Returns:
        One source per host spec; absent bundles yield assetless sources so inventory reports them.
    """
    return tuple(src for key in _HOST_SPECS for src in (_host_source(settings, key),) if src is not None)


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


def packages(settings: AssaySettings) -> dict[str, str]:
    """Read the central package/version map from ``Directory.Packages.props``.

    Returns:
        Package name to pinned version, empty when the props file is absent.
    """
    path = settings.root / _PACKAGES_PROPS
    digest = _props_digest(path)
    return {} if digest is None else _packages_at(str(path), digest)


def resolve_key(package_map: dict[str, str], key: str) -> Result[str, ApiResolution]:
    """Resolve a fuzzy package key against the central package map.

    Returns:
        ``Ok`` with the canonical package name, or ``Error(ApiResolution)`` carrying ranked candidates.
    """
    casefold = key.casefold()
    exact = tuple(n for n in package_map if n.casefold() == casefold)
    fuzzy = tuple(n for n in package_map if n.casefold().startswith(casefold)) or tuple(n for n in package_map if casefold in n.casefold())
    hits = exact or fuzzy
    match (bool(exact) or len(hits) == 1, len(hits)):
        case (True, _):
            return Ok(hits[0])
        case (_, 0):
            return Error(ApiResolution(candidates=rank_candidates(tuple(package_map), key), reason="unknown"))
        case _:
            return Error(ApiResolution(candidates=rank_candidates(hits, key, n=len(hits)), reason="ambiguous"))


def _package_root(settings: AssaySettings, package: str, version: str) -> Path:
    candidates = tuple(
        Path(root) if Path(root).is_absolute() else Path(str(settings.root)) / root for root in _NUGET_ROOTS
    )  # NuGet cache is local-fs; UPath -> Path
    targets = tuple(base / package.casefold() / version for base in candidates)
    return next((t for t in targets if t.is_dir()), targets[0])


def _project_references(path: Path) -> tuple[str, ...]:
    try:
        raw = path.read_bytes()
    except OSError:
        return ()
    return tuple(ref.casefold() for ref in parse_csproj(raw, "PackageReference", "Include", "Update"))


def _csproj_paths(root: Path) -> tuple[Path, ...]:
    # The one owner-index discovery: generated trees never own packages.
    return tuple(sorted(p for p in root.rglob("*.csproj") if not any(part in {".artifacts", ".cache", "bin", "obj"} for part in p.parts)))


@lru_cache(maxsize=8)
def _package_owner_index_at(root_str: str, index_fingerprint: str) -> dict[str, tuple[str, ...]]:
    _ = index_fingerprint  # lru_cache key slot; encodes sorted csproj mtime_ns so re-computation triggers on project graph changes
    root = Path(root_str)
    rows = sorted((package, path.relative_to(root).as_posix()) for path in _csproj_paths(root) for package in _project_references(path))
    return {package: tuple(sorted(owner for _, owner in group)) for package, group in itertools.groupby(rows, key=operator.itemgetter(0))}


def package_owner_index(settings: AssaySettings) -> dict[str, tuple[str, ...]]:
    """Index which repo projects reference each central package.

    Returns:
        Casefolded package name to sorted owning ``.csproj`` relative paths.
    """
    root = Path(str(settings.root))
    index_fingerprint = hashlib.sha256("|".join(f"{p}:{p.stat().st_mtime_ns}" for p in _csproj_paths(root)).encode()).hexdigest()[:16]
    return _package_owner_index_at(str(root), index_fingerprint)


def nuget_source(
    settings: AssaySettings, package: str, version: str, *, include_assets: bool = True, owners: tuple[str, ...] | None = None
) -> Source:
    """Build a NuGet source with its consumer-TFM-ranked assemblies.

    The ``ref``/``lib`` framework dir is chosen by ``tfm_rank`` against the workspace floor, so ``primary_assembly``
    is the asset the build actually binds; the chosen TFM rides ``Source.tfm`` onto every receipt.

    Returns:
        Resolved NuGet source; an unrestored root folds to an assetless (EMPTY) source.
    """
    root = _package_root(settings, package, version)
    floor = consumer_tfm_floor(settings)
    selected = _select_tfm_dir(root, floor)
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
    return Source(
        key=package,
        kind=SourceKind.NUGET,
        version=version,
        assemblies=ordered,
        xmls=xmls,
        nuspec=nuspec,
        package_root=root if root.is_dir() else None,
        asset_paths=assets,
        frameworks=_frameworks(root, floor),
        owners=owners if owners is not None else package_owner_index(settings).get(package.casefold(), ()),
        tfm=selected.name if selected is not None else "",
    )


def _pydist_names() -> tuple[str, ...]:
    return tuple(sorted({dist.metadata["Name"] for dist in importlib.metadata.distributions() if dist.metadata["Name"]}))


def _dist_root(dist: importlib.metadata.Distribution) -> Path:
    try:
        return Path(str(dist.locate_file("")))
    except OSError:
        return Path()


def pydist_inventory_sources() -> tuple[ApiSource, ...]:
    """Project every installed Python distribution into an inventory row.

    Returns:
        Sorted per-distribution ApiSource rows without per-file asset expansion.
    """
    rows = (
        ApiSource(
            source_kind=SourceKind.PYDIST,
            source_id=name,
            version=dist.version or "",
            package=name,
            package_root=str(root) if root.is_dir() else "",
            status=RailStatus.OK,
        )
        for dist in importlib.metadata.distributions()
        for name in (dist.metadata["Name"] or "",)
        if name
        for root in (_dist_root(dist),)
    )
    return tuple(sorted(rows, key=lambda row: row.source_id.casefold()))


def _pydist_source(key: str) -> Source | None:
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
    return Source(
        key=dist.metadata["Name"] or key,
        kind=SourceKind.PYDIST,
        version=dist.version or "",
        package_root=root if root.is_dir() else None,
        asset_paths=assets,
    )


def _pydist_modules(key: str) -> tuple[str, ...]:
    # Declared top_level.txt roots, else packages_distributions() mapped roots, else dashes-to-underscores.
    # top_level.txt is absent in many modern wheels; packages_distributions() recovers real import roots.
    try:
        text = importlib.metadata.distribution(key).read_text("top_level.txt")
    except importlib.metadata.PackageNotFoundError:
        return ()
    declared = tuple(line.strip() for line in (text or "").splitlines() if line.strip())
    casefold = key.casefold()
    mapped = tuple(module for module, dists in importlib.metadata.packages_distributions().items() if any(d.casefold() == casefold for d in dists))
    return declared or mapped or (key.replace("-", "_"),)


def tsdecl_names(settings: AssaySettings) -> tuple[str, ...]:
    """List node_modules package names, hoisted and scoped.

    Returns:
        Sorted package names, empty when node_modules is absent.
    """
    base = Path(str(settings.root)) / _NODE_MODULES
    dirs = tuple(d for d in base.iterdir() if d.is_dir()) if base.is_dir() else ()
    top = (d.name for d in dirs if not d.name.startswith((".", "@")))
    scoped = (f"{scope.name}/{pkg.name}" for scope in dirs if scope.name.startswith("@") for pkg in scope.iterdir() if pkg.is_dir())
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


def tsdecl_source(settings: AssaySettings, key: str) -> Source | None:
    """Resolve a node_modules package into a declaration source.

    Returns:
        Source over the package's ``.d.ts`` files, or ``None`` when the package is absent.
    """
    # Hoisted node_modules entry wins, pnpm store paths are fallbacks; a package with no .d.ts files yields EMPTY, not FAULTED.
    mangled = key.replace("/", "+")
    store = Path(str(settings.root)) / _PNPM_STORE
    candidates = (Path(str(settings.root)) / _NODE_MODULES / key, *sorted(store.glob(f"{mangled}@*/{_NODE_MODULES}/{key}")))
    match next((c for c in candidates if (c / "package.json").is_file() or any(c.glob(_DTS_GLOB))), None):
        case None:
            return None
        case pkg_dir:
            manifest = _json_fields(pkg_dir / "package.json", "types", "typings", "version")
            # package.json `types`/`typings` wins over the `index.d.ts` convention. Declarations walk the whole
            # package tree (an entry that only re-exports nested files rosters nothing otherwise), skipping the
            # package's own vendored node_modules.
            declared = pkg_dir / (manifest["types"] or manifest["typings"] or _DTS_ENTRY)
            entry = declared if declared.is_file() else None
            siblings = tuple(sorted(p for p in pkg_dir.rglob(_DTS_GLOB) if _NODE_MODULES not in p.relative_to(pkg_dir).parts))
            assets = tuple(dict.fromkeys((*((entry,) if entry is not None else ()), *siblings)))
            # pnpm store dir encodes `{mangled}@version(+peer)(_hash)`; strip the peer/hash suffixes.
            pnpm = next((d.name.removeprefix(f"{mangled}@").split("+", 1)[0].split("_", 1)[0] for d in sorted(store.glob(f"{mangled}@*"))), "")
            return Source(key=key, kind=SourceKind.TSDECL, version=pnpm or manifest["version"], package_root=pkg_dir, asset_paths=assets)


def _resolve_targets(source: Source, kind: PathKind) -> tuple[Path, ...]:
    # Deduplicated paths per kind token; the caller validates the token against PATH_KINDS.
    catalog: dict[PathKind, tuple[Path, ...]] = {
        "all": (*source.assemblies, *source.xmls, *((source.nuspec,) if source.nuspec is not None else ()), *source.asset_paths),
        "assembly": source.assemblies,
        "xml": source.xmls,
        "nuspec": (source.nuspec,) if source.nuspec is not None else (),
        "deps": tuple(p for p in source.asset_paths if any(part in _DEPS_PARTS for part in p.parts)),
        "package-root": (source.package_root,) if source.package_root is not None else (),
    }
    return tuple(dict.fromkeys(catalog[kind]))


# SourceKind-keyed resolver order is the bundle > NuGet > pydist > tsdecl precedence; iteration takes the first hit.
_RESOLVE_TABLE: dict[SourceKind, SourceResolver] = {
    SourceKind.ASSEMBLY: lambda settings, key, _packages_map: _host_source(settings, key),
    SourceKind.NUGET: lambda settings, key, packages_map: (
        resolve_key(packages_map, key).map(lambda package: nuget_source(settings, package, packages_map[package])).default_value(None)
    ),
    SourceKind.PYDIST: lambda _settings, key, _packages_map: _pydist_source(key),
    SourceKind.TSDECL: lambda settings, key, _packages_map: tsdecl_source(settings, key),
}

# Fidelity is the single SourceKind->Fidelity correspondence: the surface stores no fidelity, every read derives it
# here, so it cannot drift. ASSEMBLY/NUGET decompile through ilspycmd; PYDIST is live introspection; TSDECL parses declarations.
_FIDELITY: dict[SourceKind, Fidelity] = {
    SourceKind.ASSEMBLY: Fidelity.DECOMPILED,
    SourceKind.NUGET: Fidelity.DECOMPILED,
    SourceKind.TOOL: Fidelity.DECOMPILED,
    SourceKind.PYDIST: Fidelity.INTROSPECTED,
    SourceKind.TSDECL: Fidelity.DECLARED,
}


def fidelity_note(source: Source) -> str:
    """Render the SourceKind-derived fidelity note for report notes.

    Returns:
        ``fidelity: <decompiled|introspected|declared>``.
    """
    return f"fidelity: {_FIDELITY[source.kind].value}"


def _resolve_source(settings: AssaySettings, key: str) -> Result[Source, ApiResolution]:
    # Misses aggregate candidates across every resolver kind.
    package_map = packages(settings)

    def _attempt(resolver: SourceResolver) -> Source | None:
        # Empty/NUL keys can raise in metadata and glob resolvers; unknown stays a miss.
        try:
            return resolver(settings, key, package_map)
        except ValueError, OSError:
            return None

    match next((source for resolver in _RESOLVE_TABLE.values() if (source := _attempt(resolver)) is not None), None):
        case Source() as source:
            return Ok(source)
        case None:
            names = (*tuple(package_map), *_pydist_names(), *tsdecl_names(settings))
            return Error(ApiResolution(candidates=rank_candidates(names, key), reason="unknown"))


def to_api_source(source: Source, *, status: RailStatus | None = None, selected: tuple[Path, ...] = ()) -> ApiSource:
    """Project a resolved source onto the wire ApiSource detail, stamping the consumer-bound TFM.

    Returns:
        Wire detail row with restore state derived from the package root.
    """
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
        tfm=source.tfm,
    )


# --- [ECMA335_METADATA]


def _cli_streams(raw: bytes) -> tuple[bytes, bytes] | None:
    # PE -> COFF -> optional-header data directory 14 (CLI) -> metadata root -> (#~|#-, #Strings) payloads.
    pe = int.from_bytes(raw[0x3C:0x40], "little")
    if raw[pe : pe + 4] != b"PE\x00\x00":
        return None
    section_count = int.from_bytes(raw[pe + 6 : pe + 8], "little")
    opt = pe + 24
    opt_size = int.from_bytes(raw[pe + 20 : pe + 22], "little")
    dirs = opt + (96 if int.from_bytes(raw[opt : opt + 2], "little") == 0x10B else 112)
    table = opt + opt_size
    sections = tuple(
        (
            int.from_bytes(raw[base + 12 : base + 16], "little"),  # virtual address
            int.from_bytes(raw[base + 8 : base + 12], "little"),  # virtual size
            int.from_bytes(raw[base + 20 : base + 24], "little"),  # raw pointer
        )
        for index in range(section_count)
        for base in (table + 40 * index,)
    )

    def off(rva: int) -> int | None:
        return next((pointer + (rva - address) for address, size, pointer in sections if address <= rva < address + size), None)

    cli = off(int.from_bytes(raw[dirs + 112 : dirs + 116], "little"))
    md = off(int.from_bytes(raw[cli + 8 : cli + 12], "little")) if cli is not None else None
    if md is None or int.from_bytes(raw[md : md + 4], "little") != _MD_MAGIC:
        return None
    version_len = int.from_bytes(raw[md + 12 : md + 16], "little")
    stream_count = int.from_bytes(raw[md + 16 + version_len + 2 : md + 16 + version_len + 4], "little")
    cursor, streams = md + 16 + version_len + 4, dict[str, bytes]()
    for _ in range(stream_count):
        payload_off = int.from_bytes(raw[cursor : cursor + 4], "little")
        payload_size = int.from_bytes(raw[cursor + 4 : cursor + 8], "little")
        terminator = raw.index(b"\x00", cursor + 8)
        streams[raw[cursor + 8 : terminator].decode("ascii", errors="replace")] = raw[md + payload_off : md + payload_off + payload_size]
        cursor = cursor + 8 + ((terminator - (cursor + 8)) // 4 + 1) * 4
    tables = streams.get("#~") or streams.get("#-")
    strings = streams.get("#Strings")
    return None if tables is None or strings is None else (tables, strings)


def _md_width(token: str, heap: int, counts: dict[int, int]) -> int:
    match token:
        case "u2" | "u4":
            return _MD_FIXED[token]
        case "S" | "G" | "B":
            return 4 if heap & _MD_HEAP_BIT[token] else 2
        case _ if token.startswith("I:"):
            return 4 if counts.get(int(token.removeprefix("I:"), 16), 0) > 0xFFFF else 2
        case _:
            family = _MD_FAMILIES[token.removeprefix("C:")]
            tag = max(1, (len(family) - 1).bit_length())
            return 4 if max((counts.get(member, 0) for member in family), default=0) >= 1 << (16 - tag) else 2


def _heap_str(strings: bytes, index: int) -> str:
    terminator = strings.find(b"\x00", index)
    return strings[index:terminator].decode("utf-8", errors="replace") if 0 <= index < len(strings) and terminator >= 0 else ""


def _typedef_reflection(raw: bytes) -> tuple[str, ...]:
    # TypeDef names carry the backtick arity verbatim in metadata; NestedClass supplies the `+` chain.
    match _cli_streams(raw):
        case None:
            return ()
        case (tables, strings):
            heap = tables[6]
            valid = int.from_bytes(tables[8:16], "little")
            present = tuple(index for index in range(64) if valid >> index & 1)
            if any(tid not in _MD_TABLES for tid in present):
                return ()
            counts = {tid: int.from_bytes(tables[24 + 4 * slot : 28 + 4 * slot], "little") for slot, tid in enumerate(present)}
            widths = {tid: sum(_md_width(token, heap, counts) for token in _MD_TABLES[tid]) for tid in present}
            offsets, cursor = dict[int, int](), 24 + 4 * len(present)
            for tid in present:
                offsets[tid] = cursor
                cursor += widths[tid] * counts[tid]
            string_width = _md_width("S", heap, counts)
            typedef_width = _md_width("I:02", heap, counts)
            own = tuple(
                (
                    _heap_str(strings, int.from_bytes(tables[at + 4 + string_width : at + 4 + 2 * string_width], "little")),
                    _heap_str(strings, int.from_bytes(tables[at + 4 : at + 4 + string_width], "little")),
                )
                for row in range(counts.get(0x02, 0))
                for at in (offsets.get(0x02, 0) + row * widths.get(0x02, 0),)
            )
            nested_base, nested_width = offsets.get(0x29, 0), widths.get(0x29, 0)
            enclosing = {
                nested - 1: parent - 1
                for row in range(counts.get(0x29, 0))
                for at in (nested_base + row * nested_width,)
                for nested in (int.from_bytes(tables[at : at + typedef_width], "little"),)
                for parent in (int.from_bytes(tables[at + typedef_width : at + 2 * typedef_width], "little"),)
            }

            def full(index: int, seen: frozenset[int]) -> str:
                namespace, name = own[index]
                parent = enclosing.get(index)
                if parent is None or parent in seen or not 0 <= parent < len(own):
                    return f"{namespace}.{name}" if namespace else name
                return f"{full(parent, seen | {index})}+{name}"

            return tuple(name for index in range(len(own)) for name in (full(index, frozenset()),) if "<" not in name)


@lru_cache(maxsize=64)
def _reflection_names_at(path_str: str, size: int, mtime_ns: int) -> tuple[str, ...]:
    _ = (size, mtime_ns)  # lru_cache key slots: a rewritten assembly re-reads, an unchanged one replays
    try:
        return _typedef_reflection(Path(path_str).read_bytes())
    except OSError, ValueError, IndexError:
        return ()


def reflection_map(assemblies: tuple[Path, ...]) -> dict[str, tuple[str, ...]]:
    """Map each display FQN to its CLR reflection names across the source's assemblies.

    Returns:
        Display name (dotted, arity-free) to reflection names (backtick arity, ``+`` nesting), sorted per key.
    """
    names = tuple(
        dict.fromkeys(
            name
            for path in assemblies
            if path.is_file()
            for st in (path.stat(),)
            for name in _reflection_names_at(str(path), st.st_size, st.st_mtime_ns)
        )
    )
    rows = sorted((_ARITY.sub("", name).replace("+", "."), name) for name in names)
    return {display: tuple(name for _, name in group) for display, group in itertools.groupby(rows, key=operator.itemgetter(0))}


# --- [ILSPY_PORT]


def _api_row(language: Language, mode: Mode) -> Tool | None:
    return next((t for t in select(Claim.API, language) if t.mode is mode), None)


def _invoke(settings: AssaySettings, executor: Executor, tool: Tool, args: ToolArgs) -> Completed:
    # scope=None preserves the real dotnet-tools.json CLI home for `dotnet tool run ilspycmd`.
    routed = Routed(language=Language.CSHARP, scope=Scope.CHANGED)
    check = Check(tool=tool, args=args)
    match executor.run(check, settings=settings, scope=None, routed=routed):
        case Result(tag="ok", ok=done):
            return done
        case Result(error=fault):
            return Completed((*tool.runner.prefix, *tool.command), 1, stderr=fault.message.encode())


def probe_ilspy(settings: AssaySettings, executor: Executor) -> tuple[str, int]:
    """Probe the ilspycmd producer version through the CHECK catalog row.

    Returns:
        (first stdout line or ``""``, returncode); the version gates the roster parser and rides cache entries.
    """
    match _api_row(Language.CSHARP, Mode.CHECK):
        case None:
            return ("", 1)
        case Tool() as tool:
            done = _invoke(settings, executor, tool, ToolArgs())
            line = next((ln.strip() for ln in done.stdout.decode(errors="replace").splitlines() if ln.strip()), "")
            # A failed probe reports its real cause (first stderr line) instead of a bare empty version.
            reason = next((ln.strip() for ln in done.stderr.decode(errors="replace").splitlines() if ln.strip()), "")
            return (line or (f"unavailable: {reason[:200]}" if reason else ""), done.returncode)


def _parse_cisde(text: str) -> tuple[str, ...]:
    # `Kind FullyQualifiedName` rows; angle-bracket names are compiler synthetics (see _NOISE).
    return tuple(
        dict.fromkeys(
            parts[1]
            for line in text.splitlines()
            if line.strip() and not line.startswith("# ")
            for parts in (line.split(maxsplit=1),)
            if len(parts) == 2 and parts[0] in _SURFACE_KINDS and not _NOISE.search(parts[1])
        )
    )


# Version-gated roster grammars: rows match on the probed producer version; the catch-all is today's cisde grammar.
_ROSTER_PARSERS: tuple[tuple[re.Pattern[str], Callable[[str], tuple[str, ...]]], ...] = ((re.compile(r""), _parse_cisde),)


def _roster_parser(version: str) -> Callable[[str], tuple[str, ...]]:
    # Version-gated parser selection; the catch-all cisde grammar handles every currently shipped version.
    numeric = found.group(1) if (found := _ILSPY_VERSION.search(version)) else version
    return next(parser for pattern, parser in _ROSTER_PARSERS if pattern.search(numeric) is not None)


def split_arity(symbol: str) -> tuple[str, int]:
    """Split an explicit reflection-arity tail off a queried symbol.

    Returns:
        (base symbol, arity); arity 0 when the symbol carries no backtick tail.
    """
    match _ARITY_TAIL.fullmatch(symbol):
        case None:
            return (symbol, 0)
        case found:
            return (found.group("base"), int(found.group("n")))


def _classify(done: Completed) -> _Outcome:
    stderr = done.stderr.decode(errors="replace")
    match (done.returncode, bool(done.stdout), _ILSPY_MISS in stderr):
        case (_, _, True):
            return _Outcome.MISS
        case (0, True, _):
            return _Outcome.HIT
        case (0, False, _) if not stderr.strip():
            return _Outcome.EMPTY
        case _:
            return _Outcome.FAULT


def _attempt_spelling(
    settings: AssaySettings, executor: Executor, tool: Tool, spelling: str, ordered: tuple[Path, ...], refs: tuple[str, ...]
) -> tuple[_Outcome, Completed | None, tuple[str, ...], bool]:
    # Assemblies scan ref-first with early exit on the first hit; fault stderr accrues across the scan. A
    # decompiler crash retries the same pair once at the downgraded language version before counting as a fault.
    outcome: _Outcome = _Outcome.MISS
    faults: tuple[str, ...] = ()
    for assembly in ordered:
        done = _invoke(settings, executor, tool, ToolArgs(assembly=str(assembly), fqn=spelling, refs=refs))
        downgraded = False
        if _classify(done) is _Outcome.FAULT and any(marker in done.stderr.decode(errors="replace") for marker in _ILSPY_CRASH):
            done = _invoke(settings, executor, tool, ToolArgs(assembly=str(assembly), fqn=spelling, langversion=_ILSPY_LV_FALLBACK, refs=refs))
            downgraded = True
        match _classify(done):
            case _Outcome.HIT:
                return (_Outcome.HIT, done, faults, downgraded)
            case _Outcome.FAULT:
                outcome = _Outcome.FAULT
                faults = (*faults, done.stderr.decode(errors="replace").strip() or f"ilspycmd exit {done.returncode} on {assembly.name}")
            case _Outcome.EMPTY if outcome is _Outcome.MISS:
                outcome = _Outcome.EMPTY
            case _:
                pass
    return (outcome, None, faults, False)


def _run_decompile(settings: AssaySettings, executor: Executor, symbol: str, surface: Surface) -> Result[Completed, Fault]:
    # The reflection map supplies the CLR spellings (backtick arity, `+` nesting) the display FQN elides; every
    # colliding arity decompiles and the bodies join under per-spelling headers. A typed not-found on every
    # spelling is a Completed soft miss (the caller's roster/search fallback); any real tool failure is an
    # operational Fault carrying the aggregated stderr, never a silent degradation into fuzzy search.
    base, arity = split_arity(symbol)
    ordered = tuple(sorted(surface.source.assemblies, key=lambda a: ("/ref/" not in a.as_posix(), a.as_posix().casefold())))
    refs = tuple(part for parent in dict.fromkeys(str(a.parent) for a in ordered) for part in ("-r", parent))
    spellings = surface.reflection.get(base, (symbol,))
    selected = (tuple(s for s in spellings if s.endswith(f"`{arity}")) or spellings) if arity else spellings
    match _api_row(Language.CSHARP, Mode.LIST):
        case None:
            return Error(Fault(("api", "decompile", symbol), status=RailStatus.FAULTED, message="no ilspycmd catalog row"))
        case Tool() as tool:
            swept = tuple((spelling, _attempt_spelling(settings, executor, tool, spelling, ordered, refs)) for spelling in selected)
            hits = tuple(
                (f"{spelling} (lv=CSharp7_3)" if downgraded else spelling, done)
                for spelling, (outcome, done, _, downgraded) in swept
                if outcome is _Outcome.HIT and done is not None
            )
            match (hits, any(outcome is _Outcome.FAULT for _, (outcome, _, _, _) in swept)):
                case ((), True):
                    detail = "\n".join(dict.fromkeys(line for _, (_, _, lines, _) in swept for line in lines))
                    return Error(Fault(("api", "decompile", symbol), status=RailStatus.FAULTED, message=detail[:1024]))
                case ((), False):
                    note = f"no type definition for '{symbol}' (tried: {', '.join(selected)})"
                    return Ok(receipt(("api", "decompile", symbol), 0, notes=(note,)))
                case _:
                    body = hits[0][1].stdout if len(hits) == 1 else b"\n".join(f"// --- {sp} ---\n".encode() + done.stdout for sp, done in hits)
                    return Ok(receipt(("api", "decompile", symbol), 0, stdout=body, notes=(f"decompiled: {', '.join(sp for sp, _ in hits)}",)))


# --- [SURFACE_CACHE]


def _cache_path(settings: AssaySettings, source: Source, content_fingerprint: str) -> str:
    # Store path co-located under the per-key scope dir.
    return settings.store().path(ArtifactKind.SCOPE.value, "api", safe_key(source.key), f"{content_fingerprint or 'unresolved'}.json")


def _cache_read(settings: AssaySettings, path: str, *, producer: str, content_fingerprint: str) -> CacheEntry | None:
    store = settings.store()
    if not store.exists_path(path):
        return None
    try:
        entry = msgspec.json.decode(store.read_path(path), type=CacheEntry)
    except OSError, msgspec.MsgspecError:
        return None
    # Producer + _fingerprint identity is the replay guard; a mismatched entry is a miss, never a parse guess.
    return entry if entry.producer == producer and entry.fingerprint == content_fingerprint else None


def _cache_write(settings: AssaySettings, path: str, entry: CacheEntry) -> None:
    # transaction=True commits atomically so a concurrent reader never decodes a torn entry into a rebuild loop.
    store = settings.store()
    store.write_bytes_path(msgspec.json.encode(entry), path, transaction=True)
    # One live fingerprint per key: superseded entries (an updated DLL, a bumped package) reap on write, so the
    # per-key cache dir never accretes stale fingerprints across host or dependency upgrades.
    stale = tuple(
        sibling
        for sibling in store.walk(*path.removeprefix(f"{store.root}/").split("/")[:-1])
        if isinstance(sibling, str) and sibling != path and _FINGERPRINT_FILE.fullmatch(sibling.rsplit("/", 1)[-1]) is not None
    )
    for sibling in stale:
        try:
            store.remove_path(sibling)
        except FileNotFoundError:
            _ = store.exists_path(sibling)  # a concurrent reaper already removed it


# --- [SURFACE]


def _namespace_of(fqn: str, types: frozenset[str]) -> str:
    # The longest known owner prefix becomes the namespace.
    parts = fqn.split(".")
    return next((".".join(parts[:i]) for i in range(len(parts)) if ".".join(parts[: i + 1]) in types), ".".join(parts[:-1]))


def _roster(source: Source, cache: str, types: tuple[str, ...], raw: str) -> Surface:
    type_set = frozenset(types)
    namespace_of = {fqn: _namespace_of(fqn, type_set) for fqn in types}
    namespaces = tuple(sorted({ns for ns in namespace_of.values() if ns}))
    by_namespace = {ns: tuple(fqn for fqn in types if namespace_of[fqn] == ns) for ns in namespaces}
    return Surface(
        source=source,
        types=types,
        namespaces=namespaces,
        by_namespace=by_namespace,
        cache=cache,
        raw=raw,
        reflection=reflection_map(source.assemblies),
    )


def _parse_inproc(source: Source, cache: str, payload: str) -> Surface:
    types = tuple(dict.fromkeys(cap.text for cap in CAPTURES.decode(payload.encode() or b"[]") if cap.name == _TYPE_CAP and cap.text))
    return _roster(source, cache, types, payload)


def _cs_surface(settings: AssaySettings, source: Source, executor: Executor) -> Result[Surface, Fault]:
    assemblies = source.assemblies
    content_fingerprint = _fingerprint(assemblies) if assemblies else ""
    cache = _cache_path(settings, source, content_fingerprint)
    if not assemblies:
        # No assemblies → EMPTY surface, never FAULTED.
        return Ok(Surface(source, (), (), {}, cache, ""))
    match _cache_read(settings, cache, producer=_ILSPY_PRODUCER, content_fingerprint=content_fingerprint):
        case CacheEntry() as entry:
            return Ok(_roster(source, cache, _roster_parser(entry.version)(entry.payload), entry.payload))
        case None:
            return _cs_list(settings, source, assemblies, cache, content_fingerprint, executor)


def _cs_list(
    settings: AssaySettings, source: Source, assemblies: tuple[Path, ...], cache: str, content_fingerprint: str, executor: Executor
) -> Result[Surface, Fault]:
    match _api_row(Language.CSHARP, Mode.QUERY):
        case None:
            return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message="no ilspycmd catalog row"))
        case Tool() as tool:
            version, _rc = probe_ilspy(settings, executor)
            attempts = tuple((asm, _invoke(settings, executor, tool, ToolArgs(assembly=str(asm)))) for asm in assemblies)
            match any(done.returncode == 0 for _, done in attempts):
                case False:
                    detail = "\n".join(d.stderr.decode(errors="replace") for _, d in attempts if d.stderr) or "ilspycmd type listing failed"
                    return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message=detail[:1024]))
                case True:
                    listing = "\n".join(f"# {asm}\n{done.stdout.decode(errors='replace')}" for asm, done in attempts if done.stdout)
                    _cache_write(
                        settings, cache, CacheEntry(producer=_ILSPY_PRODUCER, version=version, fingerprint=content_fingerprint, payload=listing)
                    )
                    return Ok(_roster(source, cache, _roster_parser(version)(listing), listing))


def _inproc_check(settings: AssaySettings, source: Source, mode: Mode, thunk: InprocThunk, executor: Executor) -> Result[Completed, Fault]:
    # Thunks run through the executor port to share the deadline, rate limiter, and trace span with subprocess rails.
    language = Language.PYTHON if source.kind is SourceKind.PYDIST else Language.TYPESCRIPT
    match _api_row(language, mode):
        case None:
            return Error(Fault(("api", "surface", source.key), status=RailStatus.FAULTED, message=f"no {language.value} INPROC api row"))
        case Tool() as tool:
            check = Check(tool=tool, thunk=thunk)
            routed = Routed(language=language, scope=Scope.CHANGED)
            return executor.run(check, settings=settings, scope=None, routed=routed)


def _inproc_surface(settings: AssaySettings, source: Source, executor: Executor) -> Result[Surface, Fault]:
    producer = _PY_PRODUCER if source.kind is SourceKind.PYDIST else _TS_PRODUCER
    content_fingerprint = _fingerprint(source.asset_paths)
    cache = _cache_path(settings, source, content_fingerprint)
    match _cache_read(settings, cache, producer=producer, content_fingerprint=content_fingerprint):
        case CacheEntry() as entry:
            return Ok(_parse_inproc(source, cache, entry.payload))
        case None:

            def _persist(done: Completed) -> Surface:
                payload = done.stdout.decode(errors="replace")
                _cache_write(settings, cache, CacheEntry(producer=producer, version=source.version, fingerprint=content_fingerprint, payload=payload))
                return _parse_inproc(source, cache, payload)

            return _inproc_check(settings, source, Mode.QUERY, _inproc_thunk(source, ""), executor).map(_persist)


# --- [PYDIST_THUNK]


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
        for clipped, cut in (_clip(f"{name}.{ident}", NAME_CAP),)
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


def _inproc_thunk(source: Source, symbol: str) -> InprocThunk:
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


def _py_kind(obj: object) -> str:
    kinds: tuple[tuple[str, Callable[[object], bool]], ...] = (
        ("class", inspect.isclass),
        ("module", inspect.ismodule),
        ("method", inspect.ismethod),
        ("function", inspect.isfunction),
        ("function", inspect.isbuiltin),
        ("property", lambda o: isinstance(o, property)),
        ("type-alias", lambda o: isinstance(o, TypeAliasType)),
    )
    return next((kind for kind, predicate in kinds if predicate(obj)), type(obj).__name__)


def _member_captures(obj: object, symbol: str) -> tuple[Capture, ...]:
    sig, sig_cut = _clip(f"{symbol}{_signature(obj)}", _SIG_CAP)
    doc, doc_cut = _clip(inspect.getdoc(obj) or "", _SIG_CAP)
    full, full_cut = _clip(_object_source(obj) or _live_surface(obj, symbol), NAME_CAP * 8)
    return (
        Capture(name="signature", text=sig, file=str(getattr(obj, "__module__", "")), line=0, truncated=sig_cut),
        Capture(name="kind", text=_py_kind(obj), file="", line=0),
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


# --- [TSDECL_THUNK]


def _tsdecl_thunk(source: Source, symbol: str) -> InprocThunk:
    def run(_check: Check) -> Completed:
        parser = TSParser(ts_language(_TS_GRAMMAR))  # parser is mutable: never cached, one per thunk run
        match symbol:
            case "":
                captures = tuple(cap for path in source.asset_paths for cap in _ts_captures(parser, path))
            case _:
                # One winner across the whole declaration tree: files rank by their best declaration kind, so a
                # member-level namesake in an early file never shadows the type-level declaration in a later one.
                ranked = tuple((rank, caps) for path in source.asset_paths for rank, caps in (_ts_member_ranked(parser, path, symbol),) if caps)
                captures = min(ranked, key=operator.itemgetter(0))[1] if ranked else ()
        return receipt(("ts-api", "member" if symbol else "surface", source.key, symbol), 0, stdout=CAPTURE_ENCODER.encode(captures))

    return run


def _ts_captures(parser: TSParser, path: Path) -> tuple[Capture, ...]:
    try:
        src = path.read_bytes()
    except OSError:
        return ()
    root = parser.parse(src).root_node
    is_dts = path.name.endswith(".d.ts")
    parse_fault = (Capture(name="parse_error", text="tree-sitter parse error", file=path.name, line=1, parse_error=True),) if root.has_error else ()
    return (*parse_fault, *_ts_declared(root, path, is_dts=is_dts), *_export_specs(root, path))


def _ts_member_ranked(parser: TSParser, path: Path, symbol: str) -> tuple[int, tuple[Capture, ...]]:
    try:
        src = path.read_bytes()
    except OSError:
        return (_RANK_NONE, ())
    root = parser.parse(src).root_node
    is_dts = path.name.endswith(".d.ts")
    parse_fault = (Capture(name="parse_error", text="tree-sitter parse error", file=path.name, line=1, parse_error=True),) if root.has_error else ()
    owner, _dot, target = symbol.rpartition(".")
    # An owner segment also names a declaration FILE (`Effect.gen` -> Effect.d.ts top level): a target the
    # owner type does not nest falls back to that module's top level, and the file-owner bonus makes the
    # module's own declaration beat same-named exports scattered across sibling modules.
    file_owner = bool(owner) and path.name.split(".d.", 1)[0] == owner.rsplit(".", 1)[-1]
    owner_node = _find_decl(root, owner, is_dts=is_dts) if owner else None
    nested = _find_decl(owner_node, target, is_dts=None) if owner_node is not None else None
    flat = _find_decl(root, target, is_dts=is_dts) if nested is None and (owner_node is None or file_owner) else None
    node = nested if nested is not None else flat
    match node:
        case None:
            specs = _export_specs(root, path, target)
            return (_RANK_EXPORT, (*parse_fault, *specs)) if specs else (_RANK_NONE, ())
        case node:
            full = node_text(node)
            sig, sig_cut = _clip(full.splitlines()[0] if full else "", _SIG_CAP)
            doc, doc_cut = _clip(_ts_doc(node), _SIG_CAP)
            captures = (
                _span_capture("signature", sig, node, path, truncated=sig_cut),
                Capture(name="kind", text=_TS_KIND.get(node.type, node.type), file=path.name, line=node.start_point.row + 1),
                Capture(name="full", text=full[:_FULL_CAP], file=path.name, line=node.start_point.row + 1, truncated=len(full) > _FULL_CAP),
                *((Capture(name="doc", text=doc, file=path.name, line=node.start_point.row + 1, truncated=doc_cut),) if doc else ()),
            )
            rank = _DECL_RANK.get(node.type, _RANK_EXPORT)
            return (rank - 4 if file_owner else rank, (*parse_fault, *captures))


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
            return (Capture(name="query_error", text=str(exc)[:NAME_CAP], file=path.name, line=1, parse_error=True),)
        case compiled:
            cursor = QueryCursor(compiled.ok)
    cursor.match_limit = RESULT_CAP
    nodes = tuple(node for group in cursor.captures(root).values() for node in group if _exported(node, is_dts=is_dts))
    # Cursor match-limit or roster overflow marks every kept row as truncated.
    saturated = cursor.did_exceed_match_limit or len(nodes) > RESULT_CAP
    return tuple(
        _span_capture(_TYPE_CAP, clipped, node, path, truncated=cut or saturated)
        for node in nodes[:RESULT_CAP]
        for clipped, cut in (_clip(node_text(node), NAME_CAP),)
    )


def _find_decl(scope: Node, target: str, *, is_dts: bool | None) -> Node | None:
    # is_dts None skips the export gate: owner-scoped members inherit the owner's export.
    matched = tuple(
        n
        for n in _walk(scope, _DECL_NODES)
        if (name := n.child_by_field_name("name")) is not None and node_text(name) == target and (is_dts is None or _exported(name, is_dts=is_dts))
    )
    return min(matched, key=lambda n: _DECL_RANK.get(n.type, len(_DECL_RANK)), default=None)


def _ts_doc(node: Node) -> str:
    # The TSDoc block is the comment immediately preceding the declaration's top-level statement.
    top = next((p for p in _parents(node) if p.parent is not None and p.parent.type == "program"), node)
    prev = top.prev_sibling
    return node_text(prev).strip() if prev is not None and prev.type == "comment" and node_text(prev).startswith("/**") else ""


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


def _export_specs(root: Node, path: Path, target: str | None = None) -> tuple[Capture, ...]:
    # target None rosters every alias name; a target returns at most one signature capture.
    rows = tuple(
        _span_capture(_TYPE_CAP if target is None else "signature", clipped, spec, path, truncated=cut)
        for spec in _walk(root, _EXPORT_SPEC)
        for name in (_export_name(spec),)
        if name and (target is None or name == target)
        for clipped, cut in (_clip(name if target is None else node_text(spec), NAME_CAP if target is None else _SIG_CAP),)
    )
    return rows if target is None else rows[:1]


def _export_name(node: Node) -> str:
    names = tuple(child for child in node.children if child.type in {"identifier", "type_identifier"})
    return node_text(names[-1]) if names else ""


def _walk(node: Node, kinds: frozenset[str]) -> tuple[Node, ...]:
    own = (node,) if node.type in kinds else ()
    return (*own, *(d for child in node.children for d in _walk(child, kinds)))


# --- [XML_DOC]


def xml_doc(source: Source, symbol: str) -> str:
    """Look a symbol's summary up in the source's sidecar XMLDoc files.

    Returns:
        Stripped summary text clipped at the signature cap, or ``""`` when no member matches.
    """
    # Strips the XMLDoc kind prefix (M:/T:), the parameter list, and generic arity marks (`N types, ``N
    # methods), then matches on a dotted-segment boundary — a generic symbol matches its arity-marked id.
    needle = _ARITY.sub("", symbol).casefold()
    return next(
        (
            "".join(member.itertext()).strip()[:_SIG_CAP]
            for path in source.xmls
            if path.is_file()
            for member in (_xml_members(path))
            for name in (_ARITY.sub("", member.get("name", "").split(":", 1)[-1].split("(", 1)[0]).casefold(),)
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


# --- [ADAPTERS]


@dataclass(frozen=True, slots=True)
class _Adapter:
    """Shared adapter body; leaves override only the arm their provenance changes."""

    settings: AssaySettings
    executor: Executor
    source: Source

    def probe(self) -> ApiSource:
        """Project the source's health/inventory row.

        Returns:
            ApiSource row with status derived from resolved assets.
        """
        return to_api_source(self.source)

    def resolve(self, kind: PathKind) -> tuple[Path, ...]:
        """Project the source's concrete asset paths for one kind token.

        Returns:
            Deduplicated paths for the kind.
        """
        return _resolve_targets(self.source, kind)


@dataclass(frozen=True, slots=True)
class _CsOracle(_Adapter):
    """Shared C# adapter body: host bundle and NuGet differ only in resolution, not in the ilspy port."""

    def surface(self, scope: ArtifactScope) -> Result[Surface, Fault]:
        """List the type roster through the QUERY row, replaying the typed fingerprint cache when valid.

        Returns:
            Surface roster, or a fault when every listing attempt fails.
        """
        _ = scope
        return _cs_surface(self.settings, self.source, self.executor)

    def member(self, scope: ArtifactScope, surface: Surface, symbol: str) -> Result[Completed, Fault]:
        """Decompile one ranked type through the LIST row, resolving CLR reflection spellings from the surface.

        Returns:
            The decompile receipt (soft miss on a typed not-found), or a fault when the tool itself fails.
        """
        _ = scope
        return _run_decompile(self.settings, self.executor, symbol, surface)


@dataclass(frozen=True, slots=True)
class HostBundleOracle(_CsOracle):
    """Rhino host-bundle DLL+XML adapter."""

    @override
    def probe(self) -> ApiSource:
        """Project the source's health/inventory row.

        Returns:
            ApiSource row; assetless bundles report EMPTY.
        """
        return to_api_source(self.source, status=RailStatus.OK if self.source.assemblies else RailStatus.EMPTY)


@dataclass(frozen=True, slots=True)
class NugetOracle(_CsOracle):
    """NuGet package-cache adapter; ``source.tfm`` carries the consumer-bound framework choice."""


@dataclass(frozen=True, slots=True)
class _InprocOracle(_Adapter):
    """Shared INPROC adapter body: pydist and tsdecl differ only in their thunk."""

    def surface(self, scope: ArtifactScope) -> Result[Surface, Fault]:
        """Roster declared types through the QUERY-mode INPROC thunk, replaying the typed cache when valid.

        Returns:
            Surface roster, or a fault when the language's INPROC row is absent.
        """
        _ = scope
        return _inproc_surface(self.settings, self.source, self.executor)

    def member(self, scope: ArtifactScope, surface: Surface, symbol: str) -> Result[Completed, Fault]:
        """Extract one member's captures through the LIST-mode INPROC thunk.

        Returns:
            Thunk receipt carrying signature/doc/full captures, or a fault when the row is absent.
        """
        _ = (scope, surface)
        return _inproc_check(self.settings, self.source, Mode.LIST, _inproc_thunk(self.source, symbol), self.executor)


@dataclass(frozen=True, slots=True)
class PydistOracle(_InprocOracle):
    """Installed Python distribution adapter (live introspection)."""


@dataclass(frozen=True, slots=True)
class TsdeclOracle(_InprocOracle):
    """node_modules ``.d.ts`` declaration adapter (tree-sitter parse)."""


_ADAPTERS: dict[SourceKind, type[_CsOracle | _InprocOracle]] = {
    SourceKind.ASSEMBLY: HostBundleOracle,
    SourceKind.NUGET: NugetOracle,
    SourceKind.PYDIST: PydistOracle,
    SourceKind.TSDECL: TsdeclOracle,
}


def oracle_for(settings: AssaySettings, executor: Executor, key: str) -> Result[Oracle, ApiResolution]:
    """Resolve a source key into its bound adapter under host > NuGet > pydist > tsdecl precedence.

    Returns:
        ``Ok`` with the kind-matched adapter, or ``Error(ApiResolution)`` carrying ranked candidates.
    """
    return _resolve_source(settings, key).map(lambda source: _ADAPTERS[source.kind](settings=settings, executor=executor, source=source))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CANDIDATE_CAP",
    "CacheEntry",
    "HOST_KEYS",
    "HostBundleOracle",
    "NAME_CAP",
    "NugetOracle",
    "Oracle",
    "PATH_KINDS",
    "PathKind",
    "PydistOracle",
    "Source",
    "Surface",
    "TsdeclOracle",
    "consumer_tfm_floor",
    "fidelity_note",
    "host_sources",
    "nuget_source",
    "oracle_for",
    "package_owner_index",
    "packages",
    "probe_ilspy",
    "pydist_inventory_sources",
    "rank_candidates",
    "rank_namespace",
    "rank_type",
    "reflection_map",
    "resolve_key",
    "rhino_app",
    "safe_key",
    "split_arity",
    "tfm_rank",
    "to_api_source",
    "tsdecl_names",
    "tsdecl_source",
    "xml_doc",
]
