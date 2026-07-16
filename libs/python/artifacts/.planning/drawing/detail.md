# [PY_ARTIFACTS_DRAWING_DETAIL]

The AEC detail-management owner over three bound concerns publication drafting shares nowhere else: the detail CALLOUT (a reference-bearing mark citing a detail drawn on another sheet), the content-keyed detail LIBRARY (the `ezdxf` reusable-block store where one typical detail is authored once and placed N times), and the cross-reference DAG between sheets. `Detail` is ONE owner over the closed `Callout`/`CalloutBoundary`/`DetailSource` families and the `DetailLibrary` store, discriminating egress on the shared `drawing/symbol#SYMBOL` `SymbolTarget` — dual-lowering each callout to a `drawsvg` named-layer boundary/leader group AND authoring the `ezdxf` block store (`doc.blocks.new` + `add_attdef` + `add_blockref`, foreign details through `Importer`/`xref`). It composes `drawing/symbol#SYMBOL` for the bubble vocabulary (the callout projects a `SymbolKind` head the sheet's `Symbol` producer draws), `drawing/regime#REGIME` for the ISO layer/scale codes, and `graphic/color/derive#DERIVE` for the palette, holding no IFC — the drawing plane is a documentation projection, never the `csharp:Rasm.Bim` semantic model.

The identity regime is the design crux: a `DetailEntry` derives its `ContentKey` ONCE from the authored block bytes, so identical typical details across a sheet set dedup to ONE block placed N times — never a per-placement geometry copy — yet the cross-reference edge joins a structural discriminant, the `(host, ordinal)` placement key, to the target's content digest, so a detail referenced from ten sheets is one deduplicated block yet ten DISTINCT edges, never a content-only collision that erases nine referrers. The cross-reference graph is a `rustworkx` `PyDAG(check_cycle=True)`: `add_edge` carries the discriminant, `DAGWouldCycle` rejects a reference loop at mutation (the cycling edge captured as evidence and skipped so the acyclic remainder still orders), `transitive_reduction` is the minimal graph, `ancestors` the revision-impact closure, `topological_generations` the index depth, and `node_link_json` the content-key wire a `parse_node_link_json` round-trip re-audits. A double-claimed designator surfaces as `collided`; coverage verdicts (`cyclic`/`dangling`/`collided`) ride `ReferenceReport.severed` as issue-time AUDIT evidence, never a fatal rail — a set with a dangling reference still renders. The render offloads onto the runtime thread lane, and the owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case and one `core/plan#PLAN` `ArtifactWork` node — computing no sheet placement (`composition/sheet#SHEET`) and authoring no bubble geometry (`drawing/symbol#SYMBOL`).

## [01]-[INDEX]

- [01]-[DETAIL]: the `Detail` owner over the `Callout` family, the content-addressed `DetailLibrary` block store, and the `rustworkx` cross-reference `PyDAG`, dual-lowering over `SymbolTarget` to a `drawsvg` boundary/leader group or the `ezdxf` detail-library block store.

## [02]-[DETAIL]

- Owner: `Detail` holds `callouts`, the `DetailLibrary` store, the `Palette`, and the `SymbolTarget` value, discriminating on the `_ENGINES` dual-lowering table — never a per-target subtype. `Callout` is the one reference-bearing value; its `(host, ordinal)` is the structural discriminant keeping two identical-`target` callouts from distinct sheets DISTINCT edges. `DetailRef.cite()` is the printed `"3/A-501"` citation the DAG nodes and the library index both key on; `SymbolStyle` is the shared drawing-plane mark-style, never a parallel `DetailStyle`.
- Cases: the `CalloutBoundary` union (`circle`/`rectangle`/`polygon`/`section_line`) each projecting its leader-origin `anchor()` through one total `match`; the `CalloutKind` `StrEnum` keying `_BUBBLE` to the `SymbolKind` bubble the `bubbles()` projection folds; the `DetailSource` union (`authored`/`imported`/`referenced`) selecting how `_author_detail` populates the block. `DetailFault` is ONE family, two dispositions: the coverage cases (`dangling`/`cyclic`/`collided`) ride `ReferenceReport.severed` as evidence, the provider cases (`foreign`/`render`/`contract`) ride the runtime rail as the boundary-converted raise.
- Entry: `Detail.over` normalizes `Callout | Iterable[Callout]` by a structural `match` at the head — never a `batch` knob. `resolve` returns `RuntimeRail[ArtifactReceipt]` beside the `layered()` `LayerPlan` projection; `_svg_engine` folds each callout's boundary+leader into its `SymbolStyle.layer` `drawsvg.Group`, `_dxf_engine` authors each DISTINCT library detail ONCE as a `doc.blocks.new` block, places each callout through `add_auto_blockref`, and imports a foreign detail through `Importer`/`xref`. `bubbles()` is the callout→`SymbolKind` projection; `impact`/`depends`/`depth_layers` are the `ancestors`/`descendants`/`bfs_layers` revision-impact and dependency queries; `Detail.audit` re-checks a persisted `node_link_json` wire through `parse_node_link_json` + `digraph_find_cycle`.
- Auto: the library is content-addressed — `DetailEntry` derives the `ContentKey` ONCE from the block bytes (the single fact), so `DetailLibrary.of` deduplicates identical typical details to one block placed N times while `by_ref` indexes each by `.cite()` and `_collision`'s `Counter` surfaces a double-claimed designator as `collided`. `_graph` builds the `PyDAG` in one native-sink fold; `_resolve` derives the report from that one graph — `lexicographical_topological_sort(key=<cite>)` is the STABLE tie-broken order (the node data IS the `.cite()`, deterministic where plain `topological_sort` fragments on ties and the wire inherits the instability), `dag_longest_path` the revision-risk `depth`, `transitive_reduction` the minimal edge count. The DXF block name is `_block_name(ref)` — the CITE, never the bare designator that collides same-numbered details across sheets; an `authored`/`imported` detail reconstructs from its CAPTURED `block` bytes, never a re-read of a since-moved origin `path`. The `node_link_json` callbacks are MANDATORY so two glyph-distinct reference graphs key distinctly.
- Growth: a new callout boundary is one `CalloutBoundary` case plus one `anchor()`/`_callout_svg` arm; a new callout kind is one `CalloutKind` member plus one `_BUBBLE` row; a new import source is one `DetailSource` case plus one `_author_detail` arm; a new egress is one `_ENGINES` row; a new cross-reference query is one `rustworkx` call on the existing graph surfaced as one `ReferenceReport` field or `Detail` method; a new coverage cause is one `DetailFault` case plus one `severed` arm plus one report tuple; a new detail-metadata axis is one `DetailEntry` field. Zero new surface for a new callout, library detail, or cross-reference resolution.
- Boundary: no dimension, annotation, or sheet-placement logic — `drawing/dimension#DIMENSION`, `drawing/annotate#ANNOTATE`, `composition/sheet#SHEET`. `ezdxf` owns the detail-library block store, `rustworkx` the cross-reference DAG, `drawsvg` the named-layer boundary/leader container, `drawing/symbol#SYMBOL` the bubble geometry, `drawing/regime#REGIME` the ISO layer/scale codes and `drawing/standard#STANDARD` the discipline-pen DXF lowering, `graphic/color/derive#DERIVE` the palette, `specification/classify#CLASSIFY` the classification tables, and `csharp:Rasm.Bim` the IFC; identity minting is the runtime's.
- Packages: `ezdxf` the reusable-block store (`doc.blocks.new`/`add_attdef`/`add_blockref`/`add_auto_blockref`), the `Importer`/`xref` cross-document import, `decode_base64` block ingest, and `bbox.extents` the true placed-block extent; `rustworkx` the cross-reference `PyDAG`/`DAGWouldCycle` container and the `ancestors`/`descendants`/`bfs_layers`/`lexicographical_topological_sort`/`dag_longest_path`/`transitive_reduction`/`node_link_json` DAG-order surface the `.api` tier scopes to `detail`, never the analysis kernel `data/graph#GRAPH` holds; `drawsvg` the named-layer boundary/leader container; `expression` the vocabularies and rail; `beartype` the `over` contract; `collections.Counter` the `collided` detection; `drawing/regime#REGIME` `Discipline`/`ScaleRatio` and `drawing/standard#STANDARD` `Standard` the discipline-pen lowering; `drawing/symbol#SYMBOL` `SymbolKind`/`SymbolStyle`/`SymbolTarget`; `graphic/color/derive#DERIVE` `Palette`/`hex_ramp`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import os
from collections import Counter
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Literal, Self, assert_never

from beartype import beartype
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.regime import Discipline, ScaleRatio
from artifacts.drawing.standard import Standard
from artifacts.drawing.symbol import SymbolKind, SymbolStyle, SymbolTarget
from artifacts.graphic.layer import LayerContent, LayerIntent, LayerNode, LayerPlan, NamingSchema
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

# each proxy reifies on first render-arm use in the offloaded worker
lazy import drawsvg
lazy import ezdxf
lazy from ezdxf import bbox, xref
lazy from ezdxf.addons.importer import Importer
lazy from rustworkx import DAGWouldCycle, PyDAG, ancestors, bfs_layers, dag_longest_path
lazy from rustworkx import descendants, digraph_find_cycle, is_directed_acyclic_graph
lazy from rustworkx import lexicographical_topological_sort, node_link_json, parse_node_link_json
lazy from rustworkx import topological_generations, transitive_reduction

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Box = tuple[float, float, float, float]

_RADIUS: float = 6.0  # the default callout-bubble radius (mm) the SymbolKind projection carries


class CalloutKind(StrEnum):  # selects the drawing/symbol#SYMBOL bubble the sheet's Symbol producer draws
    DETAIL = "detail"  # a detail bubble
    SECTION = "section"  # a building/wall section cut
    ELEVATION = "elevation"  # an interior-elevation marker
    ENLARGEMENT = "enlargement"  # an enlarged-plan window
    WALL_SECTION = "wall_section"  # a vertical wall-section cut
    BLOWUP = "blowup"  # a blow-up of a small region


# --- [MODELS] ---------------------------------------------------------------------------
class DetailRef(Struct, frozen=True):
    # the cross-reference endpoint — .cite() is the "3/A-501" both the DAG nodes and the library index key on.
    designator: str  # the detail number/tag ("3")
    sheet: str  # the sheet the detail is drawn on ("A-501")
    discipline: Discipline = Discipline.ARCHITECTURAL

    def cite(self) -> str:
        return f"{self.designator}/{self.sheet}"


@tagged_union(frozen=True)
class CalloutBoundary:
    # the enlarged-region outline on the host drawing the callout leader runs from to the marker.
    tag: Literal["circle", "rectangle", "polygon", "section_line"] = tag()
    circle: tuple[Point, float] = case()  # center, radius — the classic detail-boundary bubble
    rectangle: tuple[Point, Point] = case()  # corner, corner — an enlarged-plan window
    polygon: tuple[Point, ...] = case()  # a freeform enlarged region
    section_line: tuple[tuple[Point, ...], float] = case()  # cut-line vertices + bearing — a section-cut boundary

    def anchor(self) -> Point:
        # the leader origin — the boundary centroid; one total projection, never a per-tag getattr.
        match self:
            case CalloutBoundary(tag="circle", circle=(center, _)):
                return center
            case CalloutBoundary(tag="rectangle", rectangle=((x0, y0), (x1, y1))):
                return ((x0 + x1) / 2.0, (y0 + y1) / 2.0)
            case CalloutBoundary(tag="polygon", polygon=verts):
                span = max(len(verts), 1)
                return (sum(x for x, _ in verts) / span, sum(y for _, y in verts) / span)
            case CalloutBoundary(tag="section_line", section_line=(verts, _)):
                return verts[0]
            case _ as unreachable:
                assert_never(unreachable)


class Callout(Struct, frozen=True):
    # the reference-bearing mark — `(host, ordinal)` keeps two identical-target callouts from distinct sheets DISTINCT edges.
    host: str  # ("A-101")
    ordinal: int  # the placement ordinal — the structural edge discriminant
    target: DetailRef
    anchor: Point  # the marker centre — the leader endpoint AND the bubble centre
    kind: CalloutKind  # selects the SymbolKind bubble the sheet's Symbol producer draws
    boundary: CalloutBoundary
    style: SymbolStyle  # the shared drawing/symbol#SYMBOL mark-style, never a DetailStyle


@tagged_union(frozen=True)
class DetailSource:
    # how a library block is populated — authored reconstructs from stored bytes, a foreign detail imports (copy) or xrefs (reference).
    tag: Literal["authored", "imported", "referenced"] = tag()
    authored: None = case()  # drawn in-project; the block bytes reconstruct through Importer.import_modelspace
    imported: str = case()  # origin path recorded; the captured block bytes reconstruct (content-addressed)
    referenced: str = case()  # linked from an external drawing (filename) through ezdxf xref.attach


class DetailEntry(Struct, frozen=True):
    # a content-addressed library detail — `key` derives ONCE from `block`, so identical typical details dedup to one entry.
    ref: DetailRef
    title: str
    scale: ScaleRatio
    source: DetailSource
    block: bytes  # the authored standalone-DXF detail body (base64) the key derives from
    key: ContentKey
    classification: str = ""  # MasterFormat/UniFormat code — the specification/classify#CLASSIFY seam
    keynotes: tuple[str, ...] = ()
    revision: str = ""

    @classmethod
    def authored(
        cls,
        ref: DetailRef,
        title: str,
        scale: ScaleRatio,
        block: bytes,
        /,
        *,
        classification: str = "",
        keynotes: tuple[str, ...] = (),
        revision: str = "",
    ) -> Self:
        return cls(
            ref=ref,
            title=title,
            scale=scale,
            source=DetailSource(authored=None),
            block=block,
            key=ContentIdentity.of("drawing-detail-block", block),
            classification=classification,
            keynotes=keynotes,
            revision=revision,
        )

    @classmethod
    def imported(cls, ref: DetailRef, title: str, scale: ScaleRatio, path: str, block: bytes, /, *, classification: str = "") -> Self:
        return cls(
            ref=ref,
            title=title,
            scale=scale,
            source=DetailSource(imported=path),
            block=block,
            key=ContentIdentity.of("drawing-detail-block", block),
            classification=classification,
        )


class DetailLibrary(Struct, frozen=True):
    # the content-addressed ezdxf-block store — `entries` deduped by content key, `by_ref` the citation resolution index.
    entries: Map[ContentKey, DetailEntry] = Map.empty()
    by_ref: Map[str, ContentKey] = Map.empty()

    @classmethod
    def of(cls, entries: Iterable[DetailEntry] = (), /) -> Self:
        held = Block.of_seq(entries)
        return cls(entries=Map.of_seq((entry.key, entry) for entry in held), by_ref=Map.of_seq((entry.ref.cite(), entry.key) for entry in held))

    def resolve(self, ref: DetailRef, /) -> Option[DetailEntry]:
        return self.by_ref.try_find(ref.cite()).bind(self.entries.try_find)


class ReferenceReport(Struct, frozen=True):
    # the cross-reference DAG evidence — raw coverage tuples stored, `severed` DERIVES the issue-time verdict, never a fatal rail.
    order: tuple[str, ...] = ()  # stable tie-broken citation order (lexicographical_topological_sort keyed on .cite())
    generations: tuple[int, ...] = ()  # per-generation node counts (topological_generations -> index depth)
    depth: int = 0  # deepest cross-reference chain (dag_longest_path) — the revision-risk sheet-hop metric
    edges: int = 0
    reduced: int = 0  # transitive-reduction edge count (the minimal cross-ref graph)
    dangling: tuple[str, ...] = ()  # callout citations the library can't resolve
    cyclic: tuple[tuple[str, str], ...] = ()  # the cross-reference cycle's (host, target) edges
    collided: tuple[str, ...] = ()  # designators claimed by >1 distinct-content detail
    wire: bytes = b""  # the node_link_json content-key input

    @property
    def severed(self) -> "Option[DetailFault]":
        # the coverage verdict in repair-priority order — cyclic before collided before dangling, Nothing if sound.
        if self.cyclic:
            return Some(DetailFault(cyclic=self.cyclic))
        if self.collided:
            return Some(DetailFault(collided=self.collided))
        return Some(DetailFault(dangling=self.dangling)) if self.dangling else Nothing


class Resolved(Struct, frozen=True):
    report: ReferenceReport
    index: frozendict[str, int]  # citation -> stable rustworkx node index (the join key)


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DetailFault:
    # ONE family, two dispositions — coverage (dangling/cyclic/collided) rides severed as evidence, provider (foreign/render/contract) the runtime rail.
    tag: Literal["dangling", "cyclic", "collided", "foreign", "render", "contract"] = tag()
    dangling: tuple[str, ...] = case()
    cyclic: tuple[tuple[str, str], ...] = case()
    collided: tuple[str, ...] = case()
    foreign: str = case()  # an ezdxf Importer/xref cross-document import raise (path)
    render: str = case()  # a drawsvg/ezdxf render raise
    contract: str = case()  # the @beartype contract violation


# --- [SERVICES] -------------------------------------------------------------------------
class Detail(Struct, frozen=True):
    callouts: tuple[Callout, ...]
    library: DetailLibrary
    palette: Palette
    target: SymbolTarget = SymbolTarget.SVG

    @classmethod
    @beartype
    def over(
        cls, callouts: Callout | Iterable[Callout], library: DetailLibrary, palette: Palette, /, *, target: SymbolTarget = SymbolTarget.SVG
    ) -> Self:
        match callouts:  # the one modal-arity head — a lone callout the singleton, a sheet set the multi-element
            case Callout():
                return cls(callouts=(callouts,), library=library, palette=palette, target=target)
            case _:
                return cls(callouts=tuple(callouts), library=library, palette=palette, target=target)

    def bubbles(self) -> tuple[SymbolKind, ...]:
        # the callout -> SymbolKind projection: this owner authors the reference apparatus, the sheet's Symbol producer draws the heads.
        return tuple(_BUBBLE[callout.kind](callout) for callout in self.callouts)

    def impact(self, ref: DetailRef, /) -> frozenset[str]:
        # the revision-impact closure — every host referencing this detail (ancestors); revise it, and every returned sheet must be reissued.
        dag, index, *_ = _graph(self)
        reverse = {node: cite for cite, node in index.items()}
        return frozenset(reverse[node] for node in ancestors(dag, index[ref.cite()])) if ref.cite() in index else frozenset()

    def depends(self, ref: DetailRef, /) -> frozenset[str]:
        dag, index, *_ = _graph(self)
        reverse = {node: cite for cite, node in index.items()}
        return frozenset(reverse[node] for node in descendants(dag, index[ref.cite()])) if ref.cite() in index else frozenset()

    def depth_layers(self, ref: DetailRef, /) -> tuple[tuple[str, ...], ...]:
        # the breadth-first reference-depth layers from a root sheet — the per-nesting-level grouping (bfs_layers over the built DAG).
        dag, index, *_ = _graph(self)
        if ref.cite() not in index:
            return ()
        reverse = {node: cite for cite, node in index.items()}
        return tuple(tuple(reverse[node] for node in layer) for layer in bfs_layers(dag, [index[ref.cite()]]))

    @staticmethod
    def audit(wire: bytes, /) -> Option[DetailFault]:
        # re-audit a PERSISTED wire — the PyDAG(check_cycle=True) gate guards an authored set, this guards an INGESTED
        # graph whose edges arrive pre-built, so is_directed_acyclic_graph/digraph_find_cycle are the post-hoc audit.
        graph = parse_node_link_json(wire.decode(), node_attrs=lambda d: d["cite"], edge_attrs=lambda d: d["host"])
        return (
            Nothing if is_directed_acyclic_graph(graph) else Some(DetailFault(cyclic=tuple((str(a), str(b)) for a, b in digraph_find_cycle(graph))))
        )

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.callouts)))

    @property
    def _key(self) -> ContentKey:
        # key over the frozen INPUT spec — the library and palette are bytes-producing inputs, so identical callouts over distinct libraries never share a key.
        return ContentIdentity.of(
            f"drawing-detail-{self.target}", (self.callouts, self.library, self.palette, self.target), policy=CANONICAL_POLICY
        )

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the render thunk — the receipt threads the pre-run key; the layer payload is the layered() projection.
        return (await async_boundary(f"drawing.detail.{self.target}", self._crossed)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # the engine rows as one LayerPlan tree — substrate data the layered/sheet consumers compose, not the producer rail.
        return (await async_boundary(f"drawing.detail.{self.target}", self._crossed)).map(
            lambda pair: LayerPlan(schema=NamingSchema.ISO13567, roots=pair[0])
        )

    async def _crossed(self) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
        # synchronous native fold — crosses the runtime thread lane.
        crossed = await LanePolicy.offload(_ENGINES[self.target].arm, self, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(lambda fault: _fold_raise(fault))


def _fold_raise(fault: object) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # terminal collapse at the render boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _row(*, name: str, source: bytes, bbox: tuple[float, float, float, float] | None = None, group: str | None = None) -> LayerNode:
    # one engine row into the graphic/layer vocabulary — a group name nests as the LayerPlan path prefix, z rides row order.
    return LayerNode(name=name if group is None else f"{group}/{name}", intent=LayerIntent.ANNOTATION, content=Some(LayerContent(fragment=source)))


class DetailEngine(Struct, frozen=True):
    arm: Callable[[Detail], tuple[tuple[LayerNode, ...], ArtifactReceipt]]
    engine: str  # the ArtifactReceipt.Drawing engine descriptor


# --- [OPERATIONS] -----------------------------------------------------------------------
def _graph(detail: Detail) -> tuple[object, dict[str, int], tuple[tuple[str, str], ...], tuple[str, ...]]:
    # build the cross-reference PyDAG(check_cycle=True): a node per host sheet + resolvable citation, an edge per
    # callout carrying (host, ordinal). DAGWouldCycle captures a cycling edge as evidence and skips it; an unresolvable target is dangling.
    dag = PyDAG(check_cycle=True)
    index: dict[str, int] = {}
    dangling: list[str] = []
    cyclic: list[tuple[str, str]] = []

    def node(cite: str) -> int:  # Exemption: PyDAG is the native stateful sink; add_node mutates in place
        if (found := index.get(cite)) is None:
            found = dag.add_node(cite)
            index[cite] = found
        return found

    for callout in detail.callouts:  # Exemption: the cross-ref graph accretes on the native PyDAG in place
        target = callout.target.cite()
        if detail.library.resolve(callout.target).is_none():
            dangling.append(target)
            continue
        try:
            dag.add_edge(node(callout.host), node(target), (callout.host, callout.ordinal))
        except DAGWouldCycle:
            cyclic.append((callout.host, target))
    return dag, index, tuple(cyclic), tuple(dangling)


def _collision(library: DetailLibrary) -> tuple[str, ...]:
    # a citation claimed by >1 DISTINCT content key is a double-claimed designator (Map.of_seq last-wins would drop one) — the collided verdict.
    claims = Counter(entry.ref.cite() for entry in library.entries.values())
    return tuple(cite for cite, count in claims.items() if count > 1)


def _resolve(detail: Detail) -> Resolved:
    dag, index, cyclic, dangling = _graph(detail)
    reduced, _ = transitive_reduction(dag)
    wire = (node_link_json(dag, node_attrs=_wire_node, edge_attrs=_wire_edge) or "").encode()
    report = ReferenceReport(
        # the node data IS the `.cite()`, so lexicographical_topological_sort yields the tie-broken cite order directly —
        # deterministic where plain topological_sort fragments on ties and the wire inherits the instability.
        order=tuple(lexicographical_topological_sort(dag, key=lambda cite: cite)),
        generations=tuple(len(generation) for generation in topological_generations(dag)),
        depth=len(dag_longest_path(dag)),  # the deepest citation chain — how many sheet-hops a revision propagates
        edges=dag.num_edges(),
        reduced=reduced.num_edges(),
        dangling=dangling,
        cyclic=cyclic,
        collided=_collision(detail.library),
        wire=wire,
    )
    return Resolved(report=report, index=frozendict(index))


def _wire_node(cite: str) -> dict[str, str]:  # MANDATORY node_link_json callback — the bare null-data call collapses glyph-distinct graphs
    return {"cite": cite}


def _wire_edge(ref: tuple[str, int]) -> dict[str, str]:
    return {"host": ref[0], "ordinal": str(ref[1])}


def _bbox(detail: Detail) -> Box:
    xs = tuple(callout.anchor[0] for callout in detail.callouts) or (0.0,)
    ys = tuple(callout.anchor[1] for callout in detail.callouts) or (0.0,)
    return (min(xs), min(ys), max(xs), max(ys))


def _dxf_extent(msp: object, fallback: Box, /) -> tuple[int, int]:
    # the TRUE modelspace extent over the placed block inserts + boundary/leader entities via ezdxf.bbox.extents — never the callout-anchor hull the blocks miss.
    box = bbox.extents(msp, fast=True)
    return (round(box.size.x), round(box.size.y)) if box.has_data else (int(fallback[2] - fallback[0]), int(fallback[3] - fallback[1]))


def _callout_svg(callout: Callout, ramp: list[str]) -> bytes:
    # the enlarged-region boundary + leader to the marker anchor through drawsvg structured primitives (never an
    # f-string splice); the stroke palette-indexes the ramp, the ISO 128 weight pen sets the width.
    stroke, width = ramp[callout.style.stroke % len(ramp)], float(callout.style.weight.value)
    group = drawsvg.Group()
    match callout.boundary:
        case CalloutBoundary(tag="circle", circle=(center, radius)):
            group.append(drawsvg.Circle(center[0], center[1], radius, fill="none", stroke=stroke, stroke_width=width))
        case CalloutBoundary(tag="rectangle", rectangle=((x0, y0), (x1, y1))):
            group.append(drawsvg.Rectangle(x0, y0, x1 - x0, y1 - y0, fill="none", stroke=stroke, stroke_width=width))
        case CalloutBoundary(tag="polygon", polygon=verts):
            group.append(drawsvg.Lines(*(coord for point in verts for coord in point), close=True, fill="none", stroke=stroke, stroke_width=width))
        case CalloutBoundary(tag="section_line", section_line=(verts, _)):
            group.append(drawsvg.Lines(*(coord for point in verts for coord in point), close=False, fill="none", stroke=stroke, stroke_width=width))
        case _ as unreachable:
            assert_never(unreachable)
    origin = callout.boundary.anchor()
    group.append(drawsvg.Line(origin[0], origin[1], callout.anchor[0], callout.anchor[1], stroke=stroke, stroke_width=width))
    canvas = drawsvg.Drawing(1.0, 1.0, origin=(0.0, 0.0))
    canvas.append(group)
    return canvas.as_svg().encode()


def _layer_svg(name: str, frags: list[bytes]) -> bytes:
    canvas = drawsvg.Drawing(1.0, 1.0, origin=(0.0, 0.0))
    group = drawsvg.Group(id=name)
    for frag in frags:
        group.append(drawsvg.Raw(frag.decode()))
    canvas.append(group)
    return canvas.as_svg().encode()


_ATTDEFS: tuple[tuple[str, Point], ...] = (  # the number/sheet/title/scale ATTRIB placeholders add_auto_blockref fills
    ("DETAIL_NO", (0.0, 0.0)),
    ("SHEET", (0.0, -1.0)),
    ("TITLE", (0.0, -2.0)),
    ("SCALE", (0.0, -3.0)),
)


def _block_name(ref: DetailRef, /) -> str:
    # the DXF block identity is the CITE, not the bare designator: detail numbers restart per sheet, so
    # "1/A-501" and "1/A-502" are distinct blocks a bare-designator `doc.blocks.new` would collide (a raise).
    return f"DTL_{ref.designator}_{ref.sheet}".replace("/", "_").replace(" ", "_")


def _author_detail(doc: object, entry: DetailEntry) -> None:
    # one cite-named block per content key — authored/imported reconstructs from the CAPTURED `block` bytes (never
    # a re-read of a since-moved origin `path`), referenced xrefs the external drawing; number/title/scale ride the ATTRIBs.
    name = _block_name(entry.ref)
    match entry.source:  # Exemption: doc.blocks / xref are the native block-authoring sinks
        case DetailSource(tag="referenced", referenced=path):
            xref.attach(doc, block_name=name, filename=path)
        case DetailSource(tag="authored") | DetailSource(tag="imported"):
            block = doc.blocks.new(name)
            for attdef, offset in _ATTDEFS:  # Exemption: ezdxf BlockLayout is the GraphicsFactory sink; attdefs add in place
                block.add_attdef(attdef, offset)
            reconstruct = Importer(ezdxf.decode_base64(entry.block), doc)
            reconstruct.import_modelspace(block)
            reconstruct.finalize()
        case _ as unreachable:
            assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------
def _svg_engine(detail: Detail) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # each callout buckets into its SymbolStyle.layer `<g>` group; the bubble is drawing/symbol#SYMBOL's separate
    # contribution keyed to the same anchor, so this owner authors only the reference apparatus.
    resolved, ramp = _resolve(detail), hex_ramp(detail.palette)
    groups: dict[str, list[bytes]] = {}
    for callout in detail.callouts:  # Exemption: the drawsvg named-layer tree buckets by the composed LayerName through a mutable dict
        groups.setdefault(callout.style.layer.compose(), []).append(_callout_svg(callout, ramp))
    layers = tuple(_row(name=name, source=_layer_svg(name, frags), bbox=_bbox(detail)) for name, frags in sorted(groups.items()))
    box, data = _bbox(detail), b"".join(layer.source for layer in layers)
    key = detail._key
    return layers, ArtifactReceipt.Drawing(key, "detail", len(detail.callouts), "drawsvg", int(box[2] - box[0]), int(box[3] - box[1]), len(data))


def _dxf_engine(detail: Detail) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # the detail-library block store — each DISTINCT detail authored ONCE (dedup by content key), each callout's
    # target placed via ONE add_auto_blockref (ATTRIBs filled per placement) + the boundary/leader entities.
    resolved = _resolve(detail)
    doc, std = ezdxf.new("R2018", setup=True), Standard.of()
    std.seed(doc, layers=tuple({callout.style.layer for callout in detail.callouts}))
    msp = doc.modelspace()
    for entry in detail.library.entries.values():  # Exemption: doc.blocks is the native block table
        _author_detail(doc, entry)
    for callout in detail.callouts:  # Exemption: modelspace is the GraphicsFactory sink; add_* mutate in place
        attribs = std.graphics(callout.style.layer).asdict()  # the discipline pen off the owned LayerName
        if detail.library.resolve(callout.target).is_some():
            msp.add_auto_blockref(
                _block_name(callout.target),
                callout.anchor,
                {"DETAIL_NO": callout.target.designator, "SHEET": callout.target.sheet},
                dxfattribs=attribs,
            )
        msp.add_line(callout.boundary.anchor(), callout.anchor, dxfattribs=attribs)
    stream = io.StringIO()
    doc.write(stream)
    box, data = _bbox(detail), stream.getvalue().encode()
    width, height = _dxf_extent(msp, box)
    key = detail._key
    return (_row(name="dxf", source=data, bbox=box),), ArtifactReceipt.Drawing(
        key, "detail", len(detail.callouts), "ezdxf", width, height, len(data)
    )


# the callout-kind -> drawing/symbol#SYMBOL bubble constructor; the single edit site a new callout reaches.
_BUBBLE: frozendict[CalloutKind, Callable[[Callout], SymbolKind]] = frozendict({
    CalloutKind.DETAIL: lambda c: SymbolKind.Detail(c.anchor, _RADIUS, c.target.designator, c.target.sheet, c.style),
    CalloutKind.SECTION: lambda c: SymbolKind.Section(c.anchor, _RADIUS, c.target.designator, c.target.sheet, 0.0, c.style),
    CalloutKind.ELEVATION: lambda c: SymbolKind.Elevation(c.anchor, _RADIUS, c.target.designator, c.target.sheet, 0.0, c.style),
    CalloutKind.ENLARGEMENT: lambda c: SymbolKind.Detail(c.anchor, _RADIUS, c.target.designator, c.target.sheet, c.style),
    CalloutKind.WALL_SECTION: lambda c: SymbolKind.Section(c.anchor, _RADIUS, c.target.designator, c.target.sheet, 90.0, c.style),
    CalloutKind.BLOWUP: lambda c: SymbolKind.Detail(c.anchor, _RADIUS, c.target.designator, c.target.sheet, c.style),
})

_ENGINES: frozendict[SymbolTarget, DetailEngine] = frozendict({
    SymbolTarget.SVG: DetailEngine(arm=_svg_engine, engine="drawsvg"),
    SymbolTarget.DXF: DetailEngine(arm=_dxf_engine, engine="ezdxf"),
})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Callout",
    "CalloutBoundary",
    "CalloutKind",
    "Detail",
    "DetailEntry",
    "DetailFault",
    "DetailLibrary",
    "DetailRef",
    "DetailSource",
    "ReferenceReport",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
