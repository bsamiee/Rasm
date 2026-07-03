# [PY_ARTIFACTS_DRAWING_DETAIL]

The AEC detail-management owner over three bound concerns publication drafting shares nowhere else: the detail CALLOUT (a reference-bearing mark citing a detail drawn on another sheet), the content-keyed detail LIBRARY (the `ezdxf` reusable-block store where one typical detail is authored once and placed N times), and the cross-reference DAG between sheets (`rustworkx` topological ordering, cycle rejection, and revision-impact closure). `Detail` is ONE owner over the closed `Callout`/`CalloutBoundary`/`DetailSource` families and the `DetailLibrary` store, discriminating the egress on the shared `drawing/symbol#SYMBOL` `SymbolTarget` policy value — dual-lowering each callout to a `drawsvg` named-layer boundary/leader group for the sheet-figure path AND authoring the `ezdxf` detail-library block store (`doc.blocks.new` + `add_attdef` + `add_blockref`, one block definition placed N times, foreign details imported through `Importer`/`xref`) for the CAD path. It composes `drawing/symbol#SYMBOL` for the bubble vocabulary (the callout projects a `SymbolKind` head the sheet's `Symbol` producer draws), `drawing/standard#STANDARD` for the ISO layer/scale codes, `graphic/color/derive#DERIVE` for the palette, and `csharp:Rasm.Bim` for nothing — the drawing plane is a documentation projection, never the IFC semantic model.

The detail LIBRARY is content-addressed end to end. A `DetailEntry` derives its `ContentKey` ONCE from the authored block bytes (`ContentIdentity.of`), so two identical typical details across a sheet set dedup to ONE library entry — one cite-named `doc.blocks.new` block, N `add_blockref` inserts — never a per-placement geometry copy and never a bare-designator block name that collides same-numbered details across sheets; a foreign detail reconstructs from its captured `block` bytes through `ezdxf.addons.Importer.import_modelspace` (copy) or links through `xref.attach` (reference), a classification code tags it for the `specification/classify#CLASSIFY` seam, and the number/title/scale ride `add_attdef` ATTRIBs the `add_auto_blockref` fills per placement. The cross-reference DAG is a `rustworkx` `PyDAG(check_cycle=True)`: each callout is one `add_edge` from its host sheet to its target detail carrying the `(host, ordinal)` structural discriminant, a reference cycle raises `DAGWouldCycle` at mutation (the authoring-time gate, the cycling edge captured as evidence and skipped so the acyclic remainder still orders), the minimal cross-reference graph is `transitive_reduction`, the revision-impact closure is `ancestors` (revise this detail, and every referring sheet must be reissued), the reference-depth layers are `topological_generations`, and the canonical topology keys through `node_link_json(node_attrs=, edge_attrs=)`; a persisted wire re-audits through `parse_node_link_json` + `is_directed_acyclic_graph`/`digraph_find_cycle`, the ingested-graph counterpart of the authoring gate.

The IDENTITY-REGIME is the design crux: a content-keyed detail index deduplicates by content, but two structurally-distinct callouts citing the same detail from different sheets must stay DISTINCT edges. The cross-reference edge therefore joins a structural discriminant — the `(host, ordinal)` placement key — to the target's content digest, so a typical detail referenced from ten sheets is ONE deduplicated library block yet ten distinct reference edges, never a content-only collision that erases nine referrers. The library's citation index separately catches a double-claimed designator (one citation, two distinct-content details) as `collided` coverage evidence exactly as `core/plan#PLAN` catches a double-minted key. Coverage verdicts (`cyclic`/`dangling`/`collided`) ride `ReferenceReport.severed` as a documentation-quality AUDIT the composition root reads before issue — a set with a dangling reference still renders, the audit merely flags it — while provider raises ride the runtime `RuntimeRail`. The synchronous `rustworkx`/`ezdxf`/`drawsvg` fold offloads through one `CapacityLimiter`-bounded `anyio.to_thread.run_sync`, and the owner contributes ONE `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case and ONE `core/plan#PLAN` `ArtifactWork` node; it computes no sheet placement (`composition/sheet#SHEET`), authors no marker bubble geometry (`drawing/symbol#SYMBOL`), and holds no IFC.

## [01]-[INDEX]

- [01]-[DETAIL]: the `Detail` owner over the closed `Callout` value family (`host`/`ordinal` structural discriminant + `target` `DetailRef` + `anchor` + `CalloutKind` + `CalloutBoundary` + `SymbolStyle`), the `CalloutBoundary` `expression.tagged_union` (`circle`/`rectangle`/`polygon`/`section_line` enlarged-region outlines with the derived leader `anchor`), the `DetailLibrary` content-addressed `ezdxf` block store (`Map[ContentKey, DetailEntry]` deduped by content + a citation index catching `collided` designators, admitting authored/imported/referenced `DetailSource` cases), and the `rustworkx` cross-reference `ReferenceGraph` (`PyDAG(check_cycle=True)` build, `DAGWouldCycle` mutation-time cycle rejection, `lexicographical_topological_sort` STABLE tie-broken assembly order, `dag_longest_path` deepest-chain revision-risk `depth`, `bfs_layers` reference-depth-from-root layering, `transitive_reduction` minimal graph, `ancestors`/`descendants` revision-impact/dependency closures, `topological_generations` index depth, `node_link_json`/`parse_node_link_json` content-key wire + re-audit) — dual-lowering over `drawing/symbol#SYMBOL` `SymbolTarget` into a `drawsvg` named-layer boundary/leader group or the `ezdxf` `doc.blocks.new`/`add_attdef`/`add_blockref`/`Importer`/`xref` detail-library block store, projecting the `SymbolKind` bubble the sheet's `Symbol` producer draws, palette-indexed to `graphic/color/derive#DERIVE`, railed through `expression` `Result` with `ReferenceReport.severed` coverage evidence, offloaded via `anyio.to_thread`, contributing one `ArtifactReceipt.Drawing` case and one `core/plan#PLAN` `ArtifactWork` node.

## [02]-[DETAIL]

- Owner: `Detail` the one detail-management owner holding `callouts: tuple[Callout, ...]`, the `DetailLibrary` store, the `graphic/color/derive#DERIVE` `Palette`, and the `drawing/symbol#SYMBOL` `SymbolTarget` egress policy value — discriminating its render on the `_ENGINES` `frozendict[SymbolTarget, DetailEngine]` dual-lowering table, never a per-target subtype. `Callout` is the ONE reference-bearing value carrying `host` (the sheet the callout sits on), `ordinal` (the placement ordinal on that host — the structural discriminant that keeps two identical-target callouts DISTINCT edges), `target` (the referenced `DetailRef`), `anchor` (the marker centre / leader endpoint), `kind` (the `CalloutKind` selecting the `SymbolKind` bubble), `boundary` (the enlarged-region `CalloutBoundary`), and `style` (the shared `drawing/symbol#SYMBOL` `SymbolStyle` — the drawing-plane mark-style, never a parallel `DetailStyle`). `DetailRef` is the cross-reference endpoint value (`designator`/`sheet`/`discipline`) whose `.cite()` is the printed `"3/A-501"` citation the DAG nodes and the library index both key on. `DetailLibrary` is the content-addressed `ezdxf`-block store; `ReferenceReport` the cross-reference DAG evidence; `Resolved` the internal resolution carrier. `ezdxf` owns the DXF block model (the reusable `doc.blocks.new` `BlockLayout`, `add_attdef` ATTRIBs, `add_blockref`/`add_auto_blockref` `Insert` placement, `addons.Importer` cross-document block copy, `xref.attach`/`define` external reference, `decode_base64` block ingest) under the `drawing/standard#STANDARD` `Standard.graphics`/`Standard.seed` discipline-pen lowering (the same owned ISO `LayerName`/`LineWeight` pens `drawing/symbol#SYMBOL` draws with, never a bare-float attribute); `rustworkx` owns the cross-reference `PyDAG` algebra; `drawsvg` owns the named-layer boundary/leader container. No dimension, annotation, or sheet-placement logic crosses this owner — those are `drawing/dimension#DIMENSION`, `drawing/annotate#ANNOTATE`, and `composition/sheet#SHEET`.
- Cases: the `CalloutBoundary` `expression.tagged_union` — `circle(center, radius)` the classic detail-boundary bubble region, `rectangle(corner, corner)` an enlarged-plan window, `polygon(vertices)` a freeform enlarged region, `section_line(vertices, bearing)` a section-cut boundary — each projecting its leader-origin `anchor()` (the centroid the callout leader runs from to the marker) through one total `match`; the `CalloutKind` `StrEnum` (`DETAIL`/`SECTION`/`ELEVATION`/`ENLARGEMENT`/`WALL_SECTION`/`BLOWUP`) keying the `_BUBBLE` `frozendict` to the `SymbolKind` bubble constructor the `bubbles()` projection folds; the `DetailSource` `expression.tagged_union` (`authored` drawn in-project, `imported` copied from an external library drawing through the `ezdxf` `Importer`, `referenced` linked through `xref`) selecting how `_author_detail` populates the library block. `DetailFault` is the ONE closed family, two dispositions: the coverage cases `dangling`/`cyclic`/`collided` ride `ReferenceReport.severed` as EVIDENCE (never a fatal rail — a set with a dangling reference still renders), the provider cases `foreign`/`render`/`contract` ride the runtime `RuntimeRail` as the boundary-converted raise. Matched by one total `match`/`case` in each projection, closed by `assert_never`.
- Entry: `Detail.over` is the one modal-arity entrypoint normalizing `Callout | Iterable[Callout]` into the `callouts` tuple by a structural `match` at the head (a lone callout the singleton, a sheet set the multi-element case), never a `batch` knob; `resolve` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]`, and offloads the whole synchronous cross-reference-resolve-plus-lower fold onto `to_thread.run_sync(_ENGINES[self.target].arm, self, limiter=_LANES)` — the shared-address-space thread arm (the `rustworkx` Rust core plus the `ezdxf`/`drawsvg` render touch the `numpy` palette and the `msgspec` owners a `to_interpreter` isolate cannot load, the same lane `drawing/symbol#SYMBOL` and `visualization/diagram/draw#DRAW` take). `_svg_engine` folds each callout's boundary + leader into its `SymbolStyle.layer` `drawsvg.Group`, serializing each named group to a `Layer` row; `_dxf_engine` authors each DISTINCT library detail ONCE as a `doc.blocks.new` block with the number/title/scale ATTRIBs, places each callout's target through one `add_auto_blockref`, and imports a foreign detail through the `Importer`/`xref` seam. `bubbles()` is the callout→`SymbolKind` projection (the sheet's `Symbol` producer draws the heads), and `impact`/`depends` are the revision-impact/dependency closure queries a revision manager reads off `ancestors`/`descendants`; `Detail.audit` re-checks a persisted `node_link_json` wire through `parse_node_link_json` + `is_directed_acyclic_graph`/`digraph_find_cycle`.
- Auto: the detail library is content-addressed — `DetailEntry.authored`/`imported` derive the `ContentKey` ONCE through `ContentIdentity.of("drawing-detail-block", block)` off the authored block bytes (the SINGLE FACT), so `DetailLibrary.of` folds the entries into a `Map[ContentKey, DetailEntry]` that deduplicates two identical typical details to one block placed N times, while `by_ref: Map[str, ContentKey]` indexes each by its `.cite()` citation and `_collision` runs one `Counter` over the deduplicated entries' citations to surface a double-claimed designator as `collided` (the `Map.of_seq` last-wins drop `core/plan#PLAN` guards the same way), the raw coverage tuples stored on `ReferenceReport` and the issue-time verdict DERIVED by its `severed` property in repair-priority order. `_graph` builds the `PyDAG(check_cycle=True)` in one imperative native-sink fold — `add_node` mints the stable citation index, `add_edge(host, target, (host, ordinal))` carries the structural discriminant, `DAGWouldCycle` captures a cycling edge as `cyclic` evidence and skips it so the acyclic remainder still `topological_sort`s — and `_resolve` derives the `ReferenceReport` from that one graph: `lexicographical_topological_sort(dag, key=<cite>)` the STABLE tie-broken assembly/numbering order (the node data IS the `.cite()`, so it yields cites directly with no reverse map — deterministic across runs where plain `topological_sort` fragments on ties and the `node_link_json` wire inherits the instability), `dag_longest_path` the `depth` — the deepest citation chain, the revision-risk sheet-hop count a change propagates through — `transitive_reduction` the minimal cross-reference edge count a detail index renders, `topological_generations` the per-generation index depth, `node_link_json(node_attrs=_wire_node, edge_attrs=_wire_edge)` the content-key wire (the callbacks MANDATORY so two glyph-distinct reference graphs key distinctly). The `_dxf_engine` places each callout through `add_auto_blockref(_block_name(target), anchor, {"DETAIL_NO": …, "SHEET": …})` filling the ATTRIBs per placement, so a detail referenced ten times is one cite-named `doc.blocks.new` definition and ten inserts (the block name is `_block_name(ref)` — the cite, never the bare designator that would collide same-numbered details across sheets); an `authored`/`imported` detail reconstructs its block through `Importer.import_modelspace` over the captured `ezdxf.decode_base64(entry.block)` bytes (never a re-read of the since-moved origin `path`), a `referenced` detail links through `xref.attach(doc, block_name=, filename=)`. Every callout palette-indexes its `SymbolStyle` to one `graphic/color/derive#DERIVE` ramp through `hex_ramp`, and the boundary/leader author through `drawsvg` structured primitives (`Circle`/`Rectangle`/`Lines`/`Line`), never an f-string markup splice.
- Growth: a new callout boundary is one `CalloutBoundary` case plus one `anchor()` arm and one `_callout_svg` arm; a new callout kind is one `CalloutKind` member plus one `_BUBBLE` row (the `SymbolKind` bubble constructor); a new library-import source is one `DetailSource` case plus one `_author_detail` arm; a new egress is one `SymbolTarget` reuse plus one `_ENGINES` row; a new cross-reference query is one `rustworkx` DAG-surface call on the existing graph surfaced as one `ReferenceReport` field or one `Detail` method — exactly the shape the realized `depth` (`dag_longest_path`) field, the `depth_layers` (`bfs_layers`) method, and the `order` (`lexicographical_topological_sort` tie-break) take; a new coverage cause is one `DetailFault` case plus one `ReferenceReport.severed` arm plus one `ReferenceReport` tuple; a new detail-metadata axis (a keynote set, a revision, a classification) is one `DetailEntry` field; a new receipt fact rides the `ArtifactReceipt.Drawing` case; zero new surface for a new callout, a new library detail, or a new cross-reference resolution.
- Boundary: the deleted forms are a per-callout `SectionCallout`/`DetailBubble`/`Enlargement` class family where one `Callout` value plus the `CalloutBoundary`/`CalloutKind` closed families state them; a content-ONLY cross-reference index where the `(host, ordinal)` structural discriminant joined to the content digest keeps identical-target callouts from distinct sheets DISTINCT (the IDENTITY-REGIME collision `core/plan#PLAN`'s `Map.of_seq` guards); a per-placement detail-geometry copy where `doc.blocks.new` + N `add_blockref` place one block definition; a hand-rolled cycle scan where `PyDAG(check_cycle=True)` + `DAGWouldCycle` reject at mutation and `parse_node_link_json` + `digraph_find_cycle` re-audit a persisted wire; a re-implemented topological sort / transitive reduction / ancestor closure where `rustworkx` owns them; a bare `node_link_json` (null `data`) content key that collapses glyph-distinct reference graphs; a parallel `DetailStyle` where the shared `SymbolStyle` mark-style applies; a re-rendered marker bubble where the `bubbles()` projection hands the `SymbolKind` to the sheet's `Symbol` producer; an f-string SVG splice where `drawsvg` structured primitives author the boundary/leader; a `batch`/`mode` knob where `SymbolTarget` and the modal `over` head discriminate; a synchronous native fold on the event loop where `to_thread.run_sync` offloads it; a coverage verdict raised as a fatal fault where `ReferenceReport.severed` carries it as issue-time audit evidence; a parallel drawing receipt where the shared `ArtifactReceipt.Drawing` case carries the callout/entity/byte facts; and any IFC authoring the `csharp:Rasm.Bim` owner holds. `ezdxf` owns the detail-library block store, `rustworkx` the cross-reference DAG, `drawsvg` the named-layer boundary/leader container, `drawing/symbol#SYMBOL` the bubble geometry, `drawing/standard#STANDARD` the ISO layer/scale codes, `graphic/color/derive#DERIVE` the palette, `composition/sheet#SHEET` the placement, `specification/classify#CLASSIFY` the classification tables, and `csharp:Rasm.Bim` the IFC; identity minting is the runtime's.
- Packages: `ezdxf` (`new`/`decode_base64` document IO, `doc.blocks.new` the reusable-block store, `add_attdef` the ATTRIB placeholders, `add_blockref`/`add_auto_blockref` the `Insert` placement, `addons.importer.Importer.import_modelspace`/`finalize` the captured-bytes block reconstruction, `xref.attach` external reference, `bbox.extents` the TRUE placed-block modelspace extent, `doc.modelspace`/`doc.write` the layout + egress); `rustworkx` (`PyDAG`/`DAGWouldCycle` the cycle-rejecting cross-reference container, `ancestors`/`descendants` the revision-impact/dependency closures, `bfs_layers` the reference-depth-from-root layering, `lexicographical_topological_sort`/`topological_generations` the STABLE tie-broken assembly order + index depth, `dag_longest_path` the deepest-citation-chain `depth` revision-risk metric, `transitive_reduction` the minimal cross-reference graph, `node_link_json`/`parse_node_link_json` the content-key wire + re-audit, `is_directed_acyclic_graph`/`digraph_find_cycle` the ingested-graph audit gate — all in the DAG-order surface the `.api` tier scopes to `detail`, never the analysis kernel the `data/graph#GRAPH` owner holds); `drawsvg` (`Drawing`/`Group`/`Circle`/`Rectangle`/`Line`/`Lines`/`Raw` the named-layer boundary/leader container); `expression` (`tagged_union`/`tag`/`case`/`Result`/`Option`/`Some`/`Nothing`/`Block`/`Map` the vocabularies and rail); `beartype` (`@beartype` the `over` construction contract, matching the finalized `drawing/dimension#DIMENSION`); `msgspec` (`Struct(frozen=True)` the value objects); `builtins.frozendict` (the `_ENGINES`/`_BUBBLE` tables); `collections.Counter` (the `collided` designator detection); `anyio` (`CapacityLimiter`/`to_thread` the offload); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`); `core/receipt#RECEIPT` (`ArtifactReceipt.Drawing`); `export/layered#LAYERED` (`Layer`); `drawing/standard#STANDARD` (`Discipline`/`ScaleRatio` the `DetailRef` codes + `Standard.of`/`graphics`/`seed` the discipline-pen DXF lowering); `drawing/symbol#SYMBOL` (`SymbolKind`/`SymbolStyle`/`SymbolTarget` the bubble vocabulary + ISO-grounded mark-style + egress selector); `visualization/chart/spec#CHART` (`Palette`/`hex_ramp`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import os
from collections import Counter
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Literal, Self, assert_never

from anyio import CapacityLimiter, to_thread
from beartype import beartype
from builtins import frozendict
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.standard import Discipline, ScaleRatio, Standard
from artifacts.drawing.symbol import SymbolKind, SymbolStyle, SymbolTarget
from artifacts.export.layered import Layer
from artifacts.visualization.chart.spec import Palette, hex_ramp

# each proxy reifies on first render-arm use in the `to_thread` worker
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

_LANES: CapacityLimiter = CapacityLimiter(os.process_cpu_count() or 4)
_RADIUS: float = 6.0  # the default callout-bubble radius (mm) the SymbolKind projection carries


class CalloutKind(StrEnum):  # selects the drawing/symbol#SYMBOL bubble the sheet's Symbol producer draws
    DETAIL = "detail"  # a detail bubble -> a drawn detail elsewhere
    SECTION = "section"  # a building/wall section cut -> a section drawing
    ELEVATION = "elevation"  # an interior-elevation marker -> an elevation
    ENLARGEMENT = "enlargement"  # an enlarged-plan window -> an enlarged plan
    WALL_SECTION = "wall_section"  # a vertical wall-section cut
    BLOWUP = "blowup"  # a blow-up of a small region -> a magnified detail


# --- [MODELS] ---------------------------------------------------------------------------
class DetailRef(Struct, frozen=True):
    # the cross-reference endpoint — a detail's own identity AND a callout's target; `.cite()` is the
    # printed "3/A-501" citation both the ReferenceGraph nodes and the DetailLibrary index key on.
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
    # the reference-bearing mark: `(host, ordinal)` is the STRUCTURAL discriminant keeping two
    # identical-`target` callouts from distinct sheets DISTINCT edges under the content-keyed index.
    host: str  # the sheet the callout is placed on ("A-101")
    ordinal: int  # the placement ordinal on the host — the structural edge discriminant
    target: DetailRef  # the referenced detail
    anchor: Point  # the callout-marker centre — the leader endpoint AND the bubble centre
    kind: CalloutKind  # selects the SymbolKind bubble the sheet's Symbol producer draws
    boundary: CalloutBoundary  # the enlarged-region outline
    style: SymbolStyle  # the shared drawing/symbol#SYMBOL mark-style (palette/layer), never a DetailStyle


@tagged_union(frozen=True)
class DetailSource:
    # how a library block is populated — an authored block reconstructs from its stored bytes, a foreign
    # detail imports (copy) or xrefs (reference) an external library drawing.
    tag: Literal["authored", "imported", "referenced"] = tag()
    authored: None = case()  # drawn in-project; the block bytes reconstruct through Importer.import_modelspace
    imported: str = case()  # origin path recorded at registration; the captured block bytes reconstruct (content-addressed)
    referenced: str = case()  # linked from an external drawing (filename) through ezdxf xref.attach


class DetailEntry(Struct, frozen=True):
    # a content-addressed library detail — the `key` derives ONCE from `block` (the SINGLE FACT), so two
    # identical typical details dedup to one entry (one block, N inserts), never a per-sheet copy.
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
    # the content-addressed ezdxf-block store: `entries` deduped by content key, `by_ref` the citation
    # resolution index; a double-claimed designator is caught by _collision's Counter, never a silent drop.
    entries: Map[ContentKey, DetailEntry] = Map.empty()
    by_ref: Map[str, ContentKey] = Map.empty()

    @classmethod
    def of(cls, entries: Iterable[DetailEntry] = (), /) -> Self:
        held = Block.of_seq(entries)
        return cls(entries=Map.of_seq((entry.key, entry) for entry in held), by_ref=Map.of_seq((entry.ref.cite(), entry.key) for entry in held))

    def resolve(self, ref: DetailRef, /) -> Option[DetailEntry]:
        return self.by_ref.try_find(ref.cite()).bind(self.entries.try_find)


class ReferenceReport(Struct, frozen=True):
    # the cross-reference DAG evidence the receipt, the detail index, and the composition root read; the
    # raw coverage tuples are stored and `severed` DERIVES the issue-time verdict (core/plan#PLAN's
    # PipelinePlan.severed pattern — never a fatal rail, and no Option field on a wire-shaped Struct).
    order: tuple[str, ...] = ()  # stable tie-broken citation order (lexicographical_topological_sort keyed on .cite() -> deterministic numbering)
    generations: tuple[int, ...] = ()  # per-generation node counts (topological_generations -> index depth)
    depth: int = 0  # deepest cross-reference chain length (dag_longest_path) — the revision-risk sheet-hop metric a change propagates through
    edges: int = 0  # cross-reference edge count
    reduced: int = 0  # transitive-reduction edge count (the minimal cross-ref graph)
    dangling: tuple[str, ...] = ()  # callout citations the library can't resolve
    cyclic: tuple[tuple[str, str], ...] = ()  # the cross-reference cycle's (host, target) edges
    collided: tuple[str, ...] = ()  # designators claimed by >1 distinct-content detail
    wire: bytes = b""  # the node_link_json content-key input

    @property
    def severed(self) -> "Option[DetailFault]":
        # the coverage verdict in repair-priority order — cyclic (a reference loop, the hardest) before
        # collided (a double-claimed designator) before dangling (an unresolved target), Nothing if sound.
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
    # ONE closed family, two dispositions: the coverage cases (dangling/cyclic/collided) ride
    # ReferenceReport.severed as issue-time EVIDENCE, the provider cases (foreign/render/contract) ride
    # the runtime RuntimeRail as the boundary-converted raise.
    tag: Literal["dangling", "cyclic", "collided", "foreign", "render", "contract"] = tag()
    dangling: tuple[str, ...] = case()  # callout citations the library can't resolve
    cyclic: tuple[tuple[str, str], ...] = case()  # the cross-reference cycle's (host, target) edges
    collided: tuple[str, ...] = case()  # designators claimed by >1 distinct-content detail
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
        # the callout -> drawing/symbol#SYMBOL bubble projection: this owner authors the reference
        # apparatus (boundary/leader/library/cross-ref), the sheet's Symbol producer draws the heads.
        return tuple(_BUBBLE[callout.kind](callout) for callout in self.callouts)

    def impact(self, ref: DetailRef, /) -> frozenset[str]:
        # the revision-impact closure — every host that references this detail (ancestors): revise the
        # detail, and every returned sheet must be reissued. The consumer runs it inside its own offload.
        dag, index, *_ = _graph(self)
        reverse = {node: cite for cite, node in index.items()}
        return frozenset(reverse[node] for node in ancestors(dag, index[ref.cite()])) if ref.cite() in index else frozenset()

    def depends(self, ref: DetailRef, /) -> frozenset[str]:
        dag, index, *_ = _graph(self)
        reverse = {node: cite for cite, node in index.items()}
        return frozenset(reverse[node] for node in descendants(dag, index[ref.cite()])) if ref.cite() in index else frozenset()

    def depth_layers(self, ref: DetailRef, /) -> tuple[tuple[str, ...], ...]:
        # the breadth-first reference-depth layers FROM a root sheet — the per-nesting-level grouping the
        # composition/sheet#SHEET detail-index consumer reads to order enlargement callouts by how many
        # cross-reference hops separate each from the root (bfs_layers over the DAG already built).
        dag, index, *_ = _graph(self)
        if ref.cite() not in index:
            return ()
        reverse = {node: cite for cite, node in index.items()}
        return tuple(tuple(reverse[node] for node in layer) for layer in bfs_layers(dag, [index[ref.cite()]]))

    @staticmethod
    def audit(wire: bytes, /) -> Option[DetailFault]:
        # re-audit a PERSISTED cross-reference wire — the mutation-time PyDAG(check_cycle=True) gate guards
        # an AUTHORED set, this guards an INGESTED graph whose edges arrive pre-built through
        # parse_node_link_json, so is_directed_acyclic_graph/digraph_find_cycle are the post-hoc audit.
        graph = parse_node_link_json(wire.decode(), node_attrs=lambda d: d["cite"], edge_attrs=lambda d: d["host"])
        return (
            Nothing if is_directed_acyclic_graph(graph) else Some(DetailFault(cyclic=tuple((str(a), str(b)) for a, b in digraph_find_cycle(graph))))
        )

    async def resolve(self) -> RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]:
        return await async_boundary(f"drawing.detail.{self.target}", self._compute)

    async def _compute(self) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
        # the cross-reference-resolve + block-store/boundary/leader fold is synchronous native/CPU work
        # (rustworkx Rust core, ezdxf/drawsvg), so it crosses one to_thread seam off the event loop in the
        # shared address space (the subinterpreter arm cannot load the numpy palette / msgspec owners).
        return await to_thread.run_sync(_ENGINES[self.target].arm, self, limiter=_LANES)


class DetailEngine(Struct, frozen=True):
    arm: Callable[[Detail], tuple[tuple[Layer, ...], ArtifactReceipt]]
    engine: str  # the ArtifactReceipt.Drawing engine descriptor


# --- [OPERATIONS] -----------------------------------------------------------------------
def _graph(detail: Detail) -> tuple[object, dict[str, int], tuple[tuple[str, str], ...], tuple[str, ...]]:
    # build the cross-reference PyDAG(check_cycle=True): a node per host sheet + resolvable detail
    # citation, an edge per callout carrying the (host, ordinal) structural discriminant. A cycle raises
    # DAGWouldCycle at add_edge (the mutation-time rejection); the cycling edge is captured as evidence and
    # skipped so the acyclic remainder still orders. An unresolvable target is dangling evidence.
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
    # a citation claimed by >1 DISTINCT content key is a double-claimed designator (Map.of_seq last-wins
    # would silently drop one) — the collided coverage verdict, core/plan#PLAN's Counter-over-keys pattern.
    claims = Counter(entry.ref.cite() for entry in library.entries.values())
    return tuple(cite for cite, count in claims.items() if count > 1)


def _resolve(detail: Detail) -> Resolved:
    dag, index, cyclic, dangling = _graph(detail)
    reduced, _ = transitive_reduction(dag)
    wire = (node_link_json(dag, node_attrs=_wire_node, edge_attrs=_wire_edge) or "").encode()
    report = ReferenceReport(
        # the node data IS the `.cite()`, so `lexicographical_topological_sort` yields the tie-broken cite order
        # directly (no reverse map) — deterministic across runs where plain `topological_sort` fragments on ties,
        # closing the non-determinism the `node_link_json` wire and any citation-numbering consumer inherits.
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
    # the TRUE modelspace extent over the placed detail-library block inserts + boundary/leader entities — the
    # finalized `drawing/dimension#DIMENSION` reads `ezdxf.bbox.extents`, never the callout-anchor hull the blocks miss.
    box = bbox.extents(msp, fast=True)
    return (round(box.size.x), round(box.size.y)) if box.has_data else (int(fallback[2] - fallback[0]), int(fallback[3] - fallback[1]))


def _callout_svg(callout: Callout, ramp: list[str]) -> bytes:
    # the enlarged-region boundary + the leader to the marker anchor, authored through drawsvg structured
    # primitives (never an f-string markup splice — TEMPLATE-SAFETY); the stroke palette-indexes the
    # graphic/color/derive#DERIVE ramp and the ISO 128 `weight` pen sets the width, the bubble symbol.md's.
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
    # one cite-named library block per content key — an authored/imported detail reconstructs its CAPTURED
    # `block` bytes (the fact the ContentKey derives from, never a re-read of a since-moved origin `path`), a
    # referenced detail xrefs the external drawing; the number/title/scale ride the add_attdef ATTRIBs.
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
def _svg_engine(detail: Detail) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
    # the callout boundary + leader named-layer drawsvg — each callout buckets into its SymbolStyle.layer
    # `<g>` group the export/layered#LAYERED OCG owner binds; the bubble is drawing/symbol#SYMBOL's
    # separate contribution keyed to the same anchor, so this owner authors only the reference apparatus.
    resolved, ramp = _resolve(detail), hex_ramp(detail.palette)
    groups: dict[str, list[bytes]] = {}
    for callout in detail.callouts:  # Exemption: the drawsvg named-layer tree buckets by the composed LayerName through a mutable dict
        groups.setdefault(callout.style.layer.compose(), []).append(_callout_svg(callout, ramp))
    layers = tuple(Layer(name=name, source=_layer_svg(name, frags), bbox=_bbox(detail)) for name, frags in sorted(groups.items()))
    box, data = _bbox(detail), b"".join(layer.source for layer in layers)
    key = ContentIdentity.of("drawing-detail-svg", resolved.report.wire + data)
    return layers, ArtifactReceipt.Drawing(key, "detail", len(detail.callouts), "drawsvg", int(box[2] - box[0]), int(box[3] - box[1]), len(data))


def _dxf_engine(detail: Detail) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
    # the detail-library block store — each DISTINCT library detail authored ONCE (dedup by content key),
    # each callout's target placed via ONE add_auto_blockref (N inserts of one block, ATTRIBs filled per
    # placement) + the boundary/leader entities; the apiUnderutilized BlockLayout/Insert store realized.
    resolved = _resolve(detail)
    doc, std = ezdxf.new("R2018", setup=True), Standard.of()
    std.seed(doc, layers=tuple({callout.style.layer for callout in detail.callouts}))
    msp = doc.modelspace()
    for entry in detail.library.entries.values():  # Exemption: doc.blocks is the native block table
        _author_detail(doc, entry)
    for callout in detail.callouts:  # Exemption: modelspace is the GraphicsFactory sink; add_* mutate in place
        attribs = std.graphics(callout.style.layer).asdict()  # the discipline pen off the owned LayerName, exactly as symbol.md
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
    key = ContentIdentity.of("drawing-detail-dxf", resolved.report.wire + data)
    return (Layer(name="dxf", source=data, bbox=box),), ArtifactReceipt.Drawing(
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

`Detail` is the one detail-management owner every AEC detail callout, typical-detail library, and sheet cross-reference is built from: the `Callout` value carries the `(host, ordinal)` structural discriminant plus its `target` `DetailRef`, `anchor`, `CalloutKind`, `CalloutBoundary`, and `SymbolStyle`, so a callout is a reference edge whose apparatus (boundary + leader) this owner authors and whose bubble head `drawing/symbol#SYMBOL` draws. The `DetailLibrary` is the content-addressed `ezdxf` block store — a `DetailEntry` derives its `ContentKey` once from the authored block bytes, so an identical typical detail across a sheet set deduplicates to one `doc.blocks.new` block placed N times by `add_auto_blockref`, foreign details importing through `Importer`/`xref` and a double-claimed designator surfacing as `collided`. The cross-reference DAG is a `rustworkx` `PyDAG(check_cycle=True)`: `add_edge` carries the structural discriminant so identical-target callouts stay distinct edges, `DAGWouldCycle` rejects a reference loop at mutation, `transitive_reduction` yields the minimal cross-reference graph, `ancestors` the revision-impact closure, `topological_generations` the index depth, and `node_link_json` the content-key wire a `parse_node_link_json` round-trip re-audits. Coverage verdicts ride `ReferenceReport.severed` as issue-time evidence, the synchronous fold offloads onto `to_thread` off the event loop, and the owner contributes one `ArtifactReceipt.Drawing` case and one `core/plan#PLAN` `ArtifactWork` node — computing no sheet placement, authoring no bubble geometry, and holding no IFC.

## [03]-[RESEARCH]

- [DETAIL_LIBRARY_BLOCK_STORE] [RESOLVED]: the content-keyed detail library is the `ezdxf` reusable-block store — `doc.blocks.new(_block_name(ref))` mints one cite-named `BlockLayout` per distinct detail, `add_attdef` declares the number/title/sheet/scale ATTRIB placeholders, and each callout places its target through ONE `add_auto_blockref(name, insert, values, dxfattribs=)` (verified `add_auto_blockref(self, name, insert, values: dict[str, str], dxfattribs=None) -> Insert`, `add_blockref`, `add_attdef` on the live `GraphicsFactory`) filling the ATTRIBs per placement, so a detail referenced N times is one block definition and N `Insert` entities — the apiUnderutilized `BlockLayout/Insert detail-library block store` realized, never a per-placement geometry copy. An authored/imported detail reconstructs its cite-named block from the captured `ezdxf.decode_base64(entry.block)` bytes through `Importer.import_modelspace` (verified `import_modelspace`, `finalize`, `decode_base64`; the block is named `_block_name(ref)` — the cite, not the bare designator that would collide same-numbered details across sheets, and the captured bytes are used at author time so the block matches the ContentKey rather than a since-moved origin file), a referenced detail links through `xref.attach(doc, block_name=, filename=)` (verified `xref.attach`/`define`/`load_modelspace`). Justified on PACKAGE (the verified `ezdxf` block/import/xref surface) and DOMAIN (a typical-detail library is a reusable-block store deduplicated by content).
- [CROSS_REFERENCE_DAG] [RESOLVED]: the sheet↔detail cross-references fold a `rustworkx` `PyDAG(check_cycle=True)` — `add_edge` (verified, raises `DAGWouldCycle` at a cycling mutation, `PyDAG(PyDiGraph)` with `check_cycle` verified) carries the `(host, ordinal)` structural discriminant, `topological_sort`/`topological_generations` (verified) give the assembly order and index depth, `transitive_reduction` (verified `-> tuple[PyDiGraph, dict[int, int]]`) the minimal cross-reference graph, `ancestors`/`descendants` (verified `-> set[int]`) the revision-impact/dependency closures, and `node_link_json(graph, node_attrs=, edge_attrs=)` (verified `-> str | None`, callbacks `Callable[[_S], dict[str, str]]`) the content-key wire with the callbacks MANDATORY (the bare null-`data` call collapses glyph-distinct graphs); a persisted wire re-audits through `parse_node_link_json` + `is_directed_acyclic_graph`/`digraph_find_cycle` (all verified). The `.api` catalog explicitly names `a drawing/detail sheet cross-reference` as a `rustworkx` DAG-order consumer, so this composes the named surface rather than a hand-rolled graph. Justified on PACKAGE (the verified `rustworkx` DAG surface, the catalog-named use) and DOMAIN (sheet cross-references form a dependency DAG whose cycles, closures, and reductions are real detail-index concerns).
- [IDENTITY_REGIME] [RESOLVED]: the cross-reference edge joins the `(host, ordinal)` STRUCTURAL discriminant to the target's content digest, so a typical detail referenced from ten sheets is one deduplicated `Map[ContentKey, DetailEntry]` library block yet ten distinct reference edges — the content-only-key collision `boundaries.md#MEMO_KEY` and `shapes.md` legislate (identical-content siblings under one parent collapse in a content-only index). The library's citation index separately catches a double-claimed designator through one `Counter` over the deduplicated entries' citations, the `Map.of_seq` last-wins drop `core/plan#PLAN` guards identically. Justified on DOMAIN (a shared typical detail is one library entry but many distinct callouts) and CONSUMER (the revision-impact `ancestors` closure is a lie if identical-target callouts collapse).
- [COVERAGE_AS_EVIDENCE] [RESOLVED]: `cyclic`/`collided`/`dangling` ride `ReferenceReport.severed: Option[DetailFault]` as issue-time AUDIT evidence, not a fatal rail — a drawing set with a dangling detail reference still renders and the audit flags it, exactly the `core/plan#PLAN` `PipelinePlan.severed` pattern (a coverage-severed plan still rides the rail). The provider raises (`ezdxf`/`rustworkx`/`drawsvg`) ride the runtime `RuntimeRail` through `async_boundary`, so `DetailFault` is ONE closed family with two dispositions rather than a coverage rail plus a provider rail. Justified on CONSUMER (the composition root reads `severed` before issuing the sheet set) and the `rails-and-effects.md` closed-fault-vocabulary law.
- [RECEIPT_DRAWING_CASE] [RESOLVED]: the owner contributes the shared `core/receipt#RECEIPT` `ArtifactReceipt.Drawing(key, "detail", entities, engine, width, height, bytes)` case (the callout count as `entities`, the `"drawsvg"`/`"ezdxf"` render engine in the fourth slot, the rendered extents and byte count), NOT a parallel receipt rail — the brief `[07]` legislates one shared `Drawing` case across the drawing plane, and this owner fills the case shape `core/receipt#RECEIPT` already declares (`(key, kind, entities, style, width, height, bytes_)`) EXACTLY as `drawing/symbol#SYMBOL`'s own `ArtifactReceipt.Drawing(key, "symbol", …, "drawsvg", …)` mint does — the two drawing-plane producers fill the neutral fourth `style` slot with the render-engine descriptor uniformly, so the case is composed as-is with NO cross-file receipt edit. The receipt carries PRODUCTION evidence (kind, entity count, engine, extent, byte count); the cross-reference TOPOLOGY facts (`order`/`generations`/`edges`/`reduced`/`dangling`) `ReferenceReport` computes are documentation-quality AUDIT the `composition/sheet#SHEET` detail-index consumer reads off `ReferenceReport` before issue, not receipt-stream scalars — so they ride `ReferenceReport` and are not under-reported on the receipt (a receipt records what was produced, the reference graph records how the set cross-references). Justified on CONSUMER (the settled shared `Drawing` case + the `ReferenceReport` detail-index reader) and the brief `[07]` one-shared-case law. The former open item is now SETTLED on `core/receipt#RECEIPT`: the `Drawing` fourth slot is the neutral `style` (`Drawing(key, kind, entities, style, width, height, bytes_)`), each drawing producer filling it with its own convention token — `detail`/`symbol` the render engine (`"drawsvg"`/`"ezdxf"`), `dimension` the ISO 129-1 dimension-style name, `annotate` the ISO 128-2 leader convention — so `detail.md` fills slot four with `"drawsvg"`/`"ezdxf"` against the settled neutral shape, no cross-file edit outstanding.
- [REVISION_RISK_DEPTH] [RESOLVED]: `ReferenceReport` computed `topological_generations` but not the deepest cross-reference chain — the revision-risk metric a detail index needs (how many sheet-hops a change propagates). The new `depth` field reads `len(dag_longest_path(dag))` (verified `dag_longest_path(graph, weight_fn=None) -> NodeIndices`, the hop-count form returning the longest DAG chain — exercised live returning `[0,1,2]` on a 3-node chain), and the new `Detail.depth_layers(ref)` method reads `bfs_layers(dag, [index[ref.cite()]])` (verified `bfs_layers(graph, sources=None) -> list[list[int]]`, live `[[0],[2,1]]`) so the `composition/sheet#SHEET` detail-index consumer orders enlargement callouts by nesting level from a root sheet. Both stay inside the DAG-ORDER surface the artifacts `rustworkx` `.api` tier scopes to `detail` — never the analysis kernel (centrality/pagerank/shortest-path) that catalog EXPLICITLY forbids as the `data/graph#GRAPH` owner's, so no phantom reach across the tier boundary. Justified on PACKAGE (the verified DAG-order members) and DOMAIN (deepest reference nesting is the real revision-risk metric) and CONSUMER (the sheet detail-index depth-ordering).
- [STABLE_ORDER_AND_CONTRACT] [RESOLVED]: `ReferenceReport.order` used plain `topological_sort`, NON-deterministic on ties, so the citation assembly order (and the `node_link_json` content-key wire that joins it) drifted across runs; `order` now reads `lexicographical_topological_sort(dag, key=lambda cite: cite)` (verified `(dag, /, key, *, reverse=False, initial=None) -> list[_S]`, returning node DATA — the `.cite()` directly, so no reverse map), a stable content-keyable numbering. `Detail.over` gains the `@beartype` boundary contract the finalized `drawing/dimension#DIMENSION`/`drawing/standard#STANDARD` apply, so a malformed `Callout | Iterable[Callout]` payload is refused at construction rather than failing deep in the `_graph` fold. Justified on PACKAGE (the verified tie-broken sort) and CONSUMER (a stable content key + the construction contract).
