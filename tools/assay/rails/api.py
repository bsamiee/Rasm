"""Dispatch api verbs over the oracle boundary and project reports.

``status``/``resolve``/``query``/``show`` bind params, route through ``tools.assay.oracle`` adapters, and
project Match/Report/artifact evidence; every source, cache, ilspy, and TFM mechanic lives in the oracle.
"""

from dataclasses import dataclass, replace
import difflib
import hashlib
import re
from typing import assert_never, ClassVar, override, TYPE_CHECKING

from cyclopts.types import PositiveInt  # noqa: TC002  # Cyclopts evaluates Param dataclass annotations at runtime.
from expression import Error, Ok, Result
import msgspec

from tools.assay.composition.settings import AssaySettings  # noqa: TC001  # beartype resolves these types at runtime in @checked signatures
from tools.assay.composition.store import ArtifactScope, ArtifactStore  # noqa: TC001  # beartype resolves these at runtime in @checked signatures
from tools.assay.core.exec import Executor  # noqa: TC001  # beartype resolves the executor-port annotation at runtime
from tools.assay.core.model import (
    ApiResolution,
    ApiSource,
    ApiSurface,
    Artifact,
    ArtifactKind,
    BaseParams,
    Claim,
    Completed,
    Fault,
    Match,
    RailStatus,
    Report,  # noqa: TC001  # unconditional: beartype @checked resolves the -> Result[Report, Fault] forward-ref under PEP 649
    RESULT_CAP,
    SourceKind,
    SymbolShape,
)
from tools.assay.diagnostics import cap_note, CAPTURES, fold
from tools.assay.oracle import (
    CANDIDATE_CAP as _CANDIDATE_CAP,
    fidelity_note,
    HOST_KEYS,
    host_sources,
    NAME_CAP as _NAME_CAP,
    nuget_source,
    Oracle,  # noqa: TC001  # beartype resolves the adapter-port annotation at runtime
    oracle_for,
    package_owner_index,
    packages,
    PATH_KINDS as _PATH_KINDS,
    PathKind as _PathKind,  # noqa: TC001  # Cyclopts evaluates the ApiParams.kind annotation at runtime
    probe_ilspy,
    pydist_inventory_sources,
    rank_candidates,
    rank_namespace,
    rank_type,
    rhino_app,
    safe_key,
    Source,  # noqa: TC001  # beartype resolves report-projection annotations at runtime under PEP 649
    Surface,  # noqa: TC001  # beartype resolves report-projection annotations at runtime under PEP 649
    to_api_source,
    tsdecl_names,
    tsdecl_source,
    xml_doc,
)


if TYPE_CHECKING:
    from pathlib import Path


# --- [CONSTANTS] ------------------------------------------------------------------------

_PACKAGE_KINDS: frozenset[SourceKind] = frozenset((SourceKind.NUGET, SourceKind.PYDIST, SourceKind.TSDECL))
_COMPACT_VISIBLE: frozenset[SourceKind] = frozenset((SourceKind.ASSEMBLY, SourceKind.NUGET, SourceKind.TOOL))  # status compact-output source kinds
_COMPACT_SUMMARY_IDS: frozenset[str] = frozenset(("python-dists", "ts-decls"))  # polyglot summary rows always shown in compact output
# Strict status faults only on an absent core bundle here, never a transitive package.
_REQUIRED_SOURCE_IDS: frozenset[str] = frozenset(HOST_KEYS) | frozenset(("rhino-app", "ilspycmd"))
_PREVIEW_ROWS: int = 12
_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")  # stable artifact wire order across runs
_LATEST_ARTIFACT: str = "latest"

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ApiParams(BaseParams):
    """Parameters shared by api verbs."""

    # One class-owned verb-slot vocabulary: usage text plus (arity, owning flags); a positional surplus names the exact flags.
    SLOTS: ClassVar[dict[str, str]] = {"": "", "query": "[SYMBOL]", "resolve": "[KEY [KIND]]", "show": "[TOKEN]"}
    VERB_SLOTS: ClassVar[dict[str, tuple[int, tuple[str, ...]]]] = {
        "query": (1, ("--symbol",)),
        "resolve": (2, ("--key", "--kind")),
        "show": (1, ("--token",)),
    }

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
        arity, flags = self.VERB_SLOTS.get(verb, (0, ()))
        match (verb, len(self.paths)):
            case (_, n) if n > arity:
                return self.surplus(verb, self.paths[arity:], flags=flags, arity=arity)
            case ("query", _):
                return replace(self, symbol=head or self.symbol, paths=())
            case ("resolve", _):
                return replace(self, key=head or self.key, kind=(tail or self.kind), paths=())
            case ("show", _):
                return replace(self, token=head or self.token, paths=())
            case _:
                return self


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


def _source_notes(source: Source) -> tuple[str, ...]:
    # The consumer-bound TFM rides a note beside fidelity so a notes-first reader sees which framework was decompiled.
    tfm = (f"tfm: {source.tfm}",) if source.kind is SourceKind.NUGET and source.tfm else ()
    return (fidelity_note(source), *tfm)


def _matches(rows: tuple[str, ...], kind: ArtifactKind, pattern: str) -> tuple[tuple[Match, ...], tuple[str, ...]]:
    # Identity-shaped ids keep delta comparisons stable across result order changes.
    query_text = pattern.casefold()

    def score(text: str) -> int:
        low = text.casefold()
        contains = 40 if query_text and query_text in low else 0
        base = 100 if query_text and query_text == low else 70 if query_text and low.startswith(query_text) else contains
        char_sim = int(difflib.SequenceMatcher(None, query_text, low).ratio() * 30) if query_text else 0
        return base + char_sim + max(0, 20 - len(text) // 4)

    filtered = tuple(r for r in rows if not pattern or query_text in r.casefold())
    scored = sorted(((r, score(r)) for r in filtered), key=lambda x: (-x[1], x[0]))
    shown = tuple(Match(id=f"{kind.value}:{r[:_NAME_CAP]}", kind=kind, text=r[:_NAME_CAP], score=s) for r, s in scored[:RESULT_CAP])
    return shown, cap_note(len(shown), len(scored), RESULT_CAP)


def _artifact(settings: AssaySettings, source: Source, name: str, content: str) -> Artifact:
    raw = content.encode()
    path = settings.store().write_bytes(raw, ArtifactKind.SCOPE.value, "api", safe_key(source.key), name)
    digest = hashlib.sha256(f"{path}|{name}".encode()).hexdigest()[:12]
    return Artifact(id=digest, kind=ArtifactKind.SCOPE, path=str(path), bytes=len(raw), lines=len(content.splitlines()))


def _api_detail(  # noqa: PLR0913  # single ApiSurface constructor surface; keyword-only slots prevent positional confusion
    source: Source,
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
        source=to_api_source(source),
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


def _strict(report: Report, p: ApiParams, core_absent: tuple[ApiSource, ...] = ()) -> Result[Report, Fault]:
    match (p.strict, report.status):
        case (True, _) if core_absent:
            absent = ", ".join(s.source_id for s in core_absent)
            return Error(Fault(("api", report.verb), status=RailStatus.FAULTED, message=f"required sources absent: {absent}"))
        case (True, status) if status is not RailStatus.OK:
            return Error(Fault(("api", report.verb), status=RailStatus.FAULTED, message=f"{report.verb} incomplete under --strict"))
        case _:
            return Ok(report)


def _miss_report(verb: str, key: str, resolution: ApiResolution) -> Report:
    # The nearest keys ride the note inline so a notes-first reader self-corrects without opening detail.
    nearest = ", ".join(name for name, _ in resolution.candidates[:3])
    note = f"no '{key}' source; {resolution.reason}" + (f", nearest: {nearest}" if nearest else "")
    done = Completed(("api", verb, key), 0, status=RailStatus.UNSUPPORTED, notes=(note,))
    return fold(Claim.API, verb, (done,), detail=resolution)


# --- [COMPOSITION] ----------------------------------------------------------------------


def status(settings: AssaySettings, scope: ArtifactScope, p: ApiParams, executor: Executor) -> Result[Report, Fault]:
    """Inventory API source health.

    Returns:
        Inventory report, or a strict-mode fault when required sources are incomplete.
    """
    _ = scope
    version_line, returncode = probe_ilspy(settings, executor)
    ilspy_ver = version_line or "unavailable"
    sources = _filtered_sources(_inventory_sources(settings, rhino_app(settings), ilspy_ver, returncode), p.sources)
    artifacts = _inventory_artifacts(settings, sources)
    done = Completed(
        ("api", "status"),
        0,
        status=RailStatus.OK if sources else RailStatus.EMPTY,
        notes=(f"{sum(s.status is RailStatus.OK for s in sources)}/{len(sources)} inventory sources healthy",),
    )
    report = fold(Claim.API, "status", (done,), detail=_status_detail(sources, artifacts))
    return _strict(
        msgspec.structs.replace(report, artifacts=artifacts, results=_status_results(_compact_sources(sources))),
        p,
        tuple(s for s in sources if s.source_id in _REQUIRED_SOURCE_IDS and s.status is not RailStatus.OK),
    )


def _status_detail(sources: tuple[ApiSource, ...], artifacts: tuple[Artifact, ...]) -> ApiSurface:
    compact = _compact_sources(sources)
    return ApiSurface(
        source=next((s for s in sources if s.source_id == "ilspycmd"), ApiSource(source_kind=SourceKind.TOOL, source_id="ilspycmd")),
        shape=SymbolShape.INDEX,
        preview="\n".join(
            f"{s.status.value}\t{s.source_kind.value}\t{s.source_id}\t{s.version or s.package_root or s.primary_assembly}" for s in compact
        ),
        lines=len(sources),
        selected=sum(s.status is RailStatus.OK for s in sources),
        artifact_paths=tuple(a.path for a in artifacts),
    )


def _status_results(compact: tuple[ApiSource, ...]) -> tuple[Match, ...]:
    return tuple(
        Match(
            id=f"inventory:{source.source_id}",
            kind=ArtifactKind.SCOPE,
            text=_status_row_text(source),
            score=100 if source.status is RailStatus.OK else 0,
            severity=None if source.status is RailStatus.OK else "missing",
        )
        for source in compact
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
    hosts = tuple(to_api_source(src, status=RailStatus.OK if src.assemblies else RailStatus.EMPTY) for src in host_sources(settings))
    return (app_source, tool_source, *hosts)


def _nuget_inventory_sources(settings: AssaySettings) -> tuple[ApiSource, ...]:
    package_map = packages(settings)
    owners = package_owner_index(settings)
    return tuple(
        to_api_source(nuget_source(settings, package, version, include_assets=False, owners=owners.get(package.casefold(), ())), status=None)
        for package, version in sorted(package_map.items())
    )


def _polyglot_inventory_sources(settings: AssaySettings) -> tuple[ApiSource, ...]:
    pydists = pydist_inventory_sources()
    pydist_names = tuple(source.source_id for source in pydists)
    decl_names = tsdecl_names(settings)
    pydist_summary = ApiSource(source_kind=SourceKind.PYDIST, source_id="python-dists", status=RailStatus.OK if pydist_names else RailStatus.EMPTY)
    tsdecl_summary = ApiSource(source_kind=SourceKind.TSDECL, source_id="ts-decls", status=RailStatus.OK if decl_names else RailStatus.EMPTY)
    tsdecls = tuple(
        src for name in decl_names for source in (tsdecl_source(settings, name),) if source is not None for src in (to_api_source(source),)
    )
    return (pydist_summary, tsdecl_summary, *pydists, *tsdecls)


def _compact_sources(sources: tuple[ApiSource, ...]) -> tuple[ApiSource, ...]:
    return tuple(source for source in sources if source.source_kind in _COMPACT_VISIBLE or source.source_id in _COMPACT_SUMMARY_IDS)


def resolve(settings: AssaySettings, scope: ArtifactScope, p: ApiParams, executor: Executor) -> Result[Report, Fault]:
    """Resolve a source key to asset paths.

    Returns:
        Asset path report, or unsupported-source candidate evidence.
    """
    _ = scope
    match oracle_for(settings, executor, p.key):
        case Result(tag="ok", ok=orc):
            return _strict(_resolve_report(settings, orc, p), p)
        case Result(error=resolution):
            return _strict(_miss_report("resolve", p.key, resolution), p)


def _resolve_report(settings: AssaySettings, orc: Oracle, p: ApiParams) -> Report:
    # Full path list rides an artifact; ranked previews ride results.
    if p.kind not in _PATH_KINDS:
        done = Completed(("api", "resolve", p.key, p.kind), 0, status=RailStatus.UNSUPPORTED, notes=(f"unknown kind: {p.kind}",))
        return fold(
            Claim.API, "resolve", (done,), detail=ApiResolution(candidates=tuple((kind, 100) for kind in sorted(_PATH_KINDS)), reason="unknown-kind")
        )
    targets = orc.resolve(p.kind)
    existing = tuple(t for t in targets if t.exists())
    results, cap_notes = _matches(tuple(str(t) for t in targets), ArtifactKind.SCOPE, "")
    # The full path list rides an artifact only when ranked results saturate the cap; small sets ride inline.
    artifact = _artifact(settings, orc.source, f"{p.kind}.paths.txt", "\n".join(str(t) for t in targets)) if cap_notes else None
    note = f"{len(existing)}/{len(targets)} {p.kind} paths present"
    done = Completed(("api", "resolve", p.key), 0, status=RailStatus.OK if existing else RailStatus.EMPTY, notes=(note, *cap_notes))
    detail = _api_detail(
        orc.source,
        SymbolShape.SEARCH,
        preview="\n".join(str(t) for t in existing[:_PREVIEW_ROWS]),
        lines=len(targets),
        selected=len(existing),
        artifact_paths=(artifact.path,) if artifact is not None else (),
    )
    report = fold(Claim.API, "resolve", (done,), detail=detail)
    return msgspec.structs.replace(report, artifacts=(artifact,) if artifact is not None else (), results=results)


def query(settings: AssaySettings, scope: ArtifactScope, p: ApiParams, executor: Executor) -> Result[Report, Fault]:
    """Query a source for namespaces, types, members, or search hits.

    Returns:
        API surface report, or unsupported-source candidate evidence.
    """
    match oracle_for(settings, executor, p.key):
        case Result(tag="ok", ok=orc):
            return orc.surface(scope).bind(lambda surface: _query_shape(settings, scope, orc, surface, p)).bind(lambda report: _strict(report, p))
        case Result(error=resolution):
            return _strict(_miss_report("query", p.key, resolution), p)


def _query_shape(settings: AssaySettings, scope: ArtifactScope, orc: Oracle, surface: Surface, p: ApiParams) -> Result[Report, Fault]:
    # Namespace-shaped symbols still decompile when they also match a type suffix.
    match shape_of(p.symbol):
        case SymbolShape.INDEX:
            rows = surface.namespaces or surface.types
            return Ok(_roster_report(settings, surface, SymbolShape.INDEX, rows, p))
        case SymbolShape.NAMESPACE:
            return _resolve_namespace(settings, scope, orc, surface, p)
        case SymbolShape.TYPE | SymbolShape.MEMBER as shape:
            return _member_report(settings, scope, orc, surface, p.symbol, shape, p)
        case SymbolShape.SEARCH:  # pragma: no cover  # shape_of never emits SEARCH; the decompile-miss path routes through _decompile_report
            return Ok(_search_report(settings, scope, orc, surface, p))
        case never:  # pragma: no cover
            assert_never(never)


def _resolve_namespace(settings: AssaySettings, scope: ArtifactScope, orc: Oracle, surface: Surface, p: ApiParams) -> Result[Report, Fault]:
    # Type-suffix match wins over exact namespace roster; no match falls through to search.
    type_fqn = rank_type(surface.types, p.symbol)
    owned = surface.by_namespace.get(rank_namespace(surface, p.symbol), ()) if not type_fqn else ()
    return (
        _member_report(settings, scope, orc, surface, type_fqn, SymbolShape.TYPE, p)
        if type_fqn
        else Ok(_roster_report(settings, surface, SymbolShape.NAMESPACE, owned, p))
        if owned
        else Ok(_search_report(settings, scope, orc, surface, p))
    )


def _member_report(
    settings: AssaySettings, scope: ArtifactScope, orc: Oracle, surface: Surface, symbol: str, shape: SymbolShape, p: ApiParams
) -> Result[Report, Fault]:
    match surface.source.kind:
        case SourceKind.ASSEMBLY | SourceKind.NUGET:
            return _cs_member(settings, scope, orc, surface, symbol, shape, p)
        case SourceKind.PYDIST | SourceKind.TSDECL:
            return _inproc_member(settings, scope, orc, surface, symbol, shape, p)
        case SourceKind.TOOL:  # pragma: no cover  # TOOL never resolves through the oracle
            return Ok(_search_report(settings, scope, orc, surface, p))
        case never:  # pragma: no cover
            assert_never(never)


def _cs_member(  # noqa: PLR0914  # decompile rendering uses its locals as independent pipeline stages
    settings: AssaySettings, scope: ArtifactScope, orc: Oracle, surface: Surface, symbol: str, shape: SymbolShape, p: ApiParams
) -> Result[Report, Fault]:
    head, _, tail = symbol.rpartition(".")
    fqn = rank_type(surface.types, symbol) or rank_type(surface.types, head)
    if not fqn:
        return Ok(_decompile_report(settings, scope, orc, surface, shape, "", "", "", "", 0, truncated=False, p=p))
    match orc.member(scope, surface, fqn):
        case Result(tag="ok", ok=done):
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
                    orc,
                    surface,
                    shape,
                    sig,
                    xml_doc(surface.source, symbol),
                    "\n".join(window),
                    text,
                    selected,
                    truncated=truncated,
                    p=p,
                )
            )
        case Result(error=fault):
            return Error(fault)


def _inproc_member(
    settings: AssaySettings, scope: ArtifactScope, orc: Oracle, surface: Surface, symbol: str, shape: SymbolShape, p: ApiParams
) -> Result[Report, Fault]:
    # Docs come from thunk captures (inspect.getdoc / TSDoc comment), not XMLDoc sidecar lookup.
    def _build(done: Completed) -> Report:
        captured = {cap.name: cap.text for cap in CAPTURES.decode(done.stdout or b"[]")}
        signature = captured.get("signature", "")
        full = captured.get("full", "") or signature
        lines = tuple(line for line in full.splitlines() if not p.grep or p.grep.casefold() in line.casefold())
        window = lines if p.full else lines[: p.max_lines]
        return _decompile_report(
            settings,
            scope,
            orc,
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

    return orc.member(scope, surface, symbol).map(_build)


def _roster_report(settings: AssaySettings, surface: Surface, shape: SymbolShape, rows: tuple[str, ...], p: ApiParams) -> Report:
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
        notes=(f"{len(surface.types)} types across {len(surface.namespaces)} namespaces", *_source_notes(source), *cap_notes),
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


def _search_report(settings: AssaySettings, scope: ArtifactScope, orc: Oracle, surface: Surface, p: ApiParams) -> Report:
    # Misses carry nearest candidates so callers can route to suggestions.
    source = surface.source
    needle = p.symbol.casefold()
    hits = tuple(fqn for fqn in surface.types if needle in fqn.casefold())
    match hits:
        case () if p.grep and source.kind in {SourceKind.ASSEMBLY, SourceKind.NUGET}:
            # No type hit but a --grep member needle: fan out member decompiles over the cap-bounded, relevance-ranked candidates.
            return _grep_member_report(settings, scope, orc, surface, p)
        case ():
            done = Completed(("api", "query", source.key), 0, status=RailStatus.UNSUPPORTED, notes=(f"no match for '{p.symbol}'",))
            return fold(Claim.API, "query", (done,), detail=ApiResolution(candidates=rank_candidates(surface.types, p.symbol), reason="partial"))
        case _:
            results, cap_notes = _matches(hits, ArtifactKind.SCOPE, p.symbol)
            done = Completed(
                ("api", "query", source.key), 0, status=RailStatus.OK, notes=(f"{len(hits)} search hits", *_source_notes(source), *cap_notes)
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


def _grep_miss(surface: Surface, p: ApiParams, note: str) -> Report:
    done = Completed(("api", "query", surface.source.key), 0, status=RailStatus.UNSUPPORTED, notes=(note,))
    return fold(Claim.API, "query", (done,), detail=ApiResolution(candidates=rank_candidates(surface.types, p.grep), reason="partial"))


def _grep_hits(scope: ArtifactScope, orc: Oracle, surface: Surface, p: ApiParams) -> tuple[tuple[str, ...], str]:
    # _CANDIDATE_CAP is the explosion guard: name-relevance ranks first; a pure member needle resembling no type name
    # falls back to the first cap roster types, so a broad needle decompiles at most _CANDIDATE_CAP candidates.
    needle = p.grep.casefold()
    candidates = tuple(name for name, _score in rank_candidates(surface.types, p.grep, n=_CANDIDATE_CAP)) or surface.types[:_CANDIDATE_CAP]
    fault_note = ""
    rows: list[str] = []
    for fqn in candidates:
        match orc.member(scope, surface, fqn):
            case Result(tag="error", error=fault):
                fault_note = fault_note or fault.message
            case Result(tag="ok", ok=done) if done.returncode == 0:
                rows.extend(
                    f"{fqn} :: {line.strip()}"
                    for line in done.stdout.decode(errors="replace").splitlines()
                    if needle in line.casefold() and not line.lstrip().startswith("///")
                )
            case _:
                pass
    return tuple(rows), fault_note


def _grep_member_report(settings: AssaySettings, scope: ArtifactScope, orc: Oracle, surface: Surface, p: ApiParams) -> Report:
    source = surface.source
    hits, fault_note = _grep_hits(scope, orc, surface, p)
    match hits:
        case () if fault_note:
            return _grep_miss(surface, p, fault_note)
        case ():
            return _grep_miss(surface, p, f"no member match for '{p.grep}'")
        case _:
            results, cap_notes = _matches(hits[:RESULT_CAP], ArtifactKind.SCOPE, p.grep)
            artifact = _artifact(settings, source, "grep-members.txt", "\n".join(hits)) if len(hits) > RESULT_CAP or cap_notes else None
            done = Completed(
                ("api", "query", source.key), 0, status=RailStatus.OK, notes=(f"{len(hits)} member hits", *_source_notes(source), *cap_notes)
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
    orc: Oracle,
    surface: Surface,
    shape: SymbolShape,
    signature: str,
    doc: str,
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
            ns_key = rank_namespace(surface, p.symbol)
            owned = surface.by_namespace.get(ns_key, ()) if ns_key != p.symbol or ns_key in surface.by_namespace else ()
            return _roster_report(settings, surface, SymbolShape.NAMESPACE, owned, p) if owned else _search_report(settings, scope, orc, surface, p)
        case _:
            direct_fqn = rank_type(surface.types, p.symbol)
            head = p.symbol.rpartition(".")[0]
            resolved_fqn = direct_fqn or (rank_type(surface.types, head) if head else "")
            is_member = shape is SymbolShape.MEMBER or bool(head and resolved_fqn and not direct_fqn)
            final_shape = SymbolShape.MEMBER if is_member else SymbolShape.TYPE
            suffix = {SourceKind.PYDIST: ".py", SourceKind.TSDECL: ".d.ts"}.get(surface.source.kind, ".cs")
            # The full decompiled body rides an artifact only when the inline window truncates; untruncated output rides the preview.
            artifact = _artifact(settings, surface.source, f"decompile{suffix}", full) if truncated else None
            detail = _api_detail(
                surface.source,
                final_shape,
                signature=signature,
                doc=doc,
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
                notes=(f"{selected} selected lines", *_source_notes(surface.source), *window_note),
            )
            identity = (resolved_fqn or p.symbol)[:_NAME_CAP]
            result = Match(id=f"{final_shape.value}:{identity}", kind=ArtifactKind.SCOPE, text=identity, score=100)
            artifacts = (artifact,) if artifact is not None else ()
            return msgspec.structs.replace(fold(Claim.API, "query", (note,), detail=detail), artifacts=artifacts, results=(result,))


def show(settings: AssaySettings, scope: ArtifactScope, p: ApiParams, executor: Executor) -> Result[Report, Fault]:
    """Preview a previously written API artifact.

    Returns:
        Artifact preview report, or an empty report when the artifact is absent.
    """
    _ = (scope, executor)
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
