# [PY_ARTIFACTS_DIAGRAM_DRAW]

`DiagramDraw` folds one positioned `DiagramGlyph` sequence into a named-layer SVG or editable `.drawio` artifact. Target admission rejects unspellable payloads and dangling topology before provider mutation; SVG grouping accumulates mixed-intent layer and typesetting faults before serialization. SVG labels outline through `ziafont`, mathematical labels compose `Formula.laid()`, and `GlyphStyle` indices project through one `hex_ramp` palette.

Glyph fold is one total dispatch over the closed `DiagramGlyph` case. SVG lowers each named payload to geometry, shared terminal definitions, and outlined text; `.drawio` lowers the representable subset to editable objects, containers, port seats, and routed edges while preserving source text. `_INTENT` assigns one `LayerIntent` to each SVG layer. `DiagramDraw.emit` mints its pre-run `ContentKey` from the length-framed glyph, palette, frame, target, and font preimage; fixed `ziafont.config.precision` and `ziafont.config.svg2` make the SVG projection deterministic for that key. Both targets contribute `ArtifactReceipt.Diagram`.

## [01]-[INDEX]

- [01]-[DRAW]: `DiagramDraw`'s glyph-to-artifact fold over the `DrawTarget`-selected arm — the `drawsvg` SVG arm bucketing each mark into its `GlyphStyle.layer` `Group` under its `_INTENT` class and outlining labels through `ziafont`/`Formula.laid()`, or the `drawpyo` `.drawio` arm lowering the same grammar to editable mxGraph with port seats, container parenting, and rotated markers — both threading the palette hex, refusing unspellable payloads as `DrawFault`, and contributing the `ArtifactReceipt.Diagram` facts.

## [02]-[DRAW]

- Owner: `DiagramDraw` owns one total `DrawTarget` and `DiagramGlyph` dispatch. Glyphset's `DiagramGlyph.mark` projection exposes each named payload without ordinal coupling. SVG groups require one `LayerIntent` per layer name; conflicting classes reject through `DrawFault.layer_intent`. `.drawio` admission proves node identity, parent identity, edge endpoints, and named port seats before `File`/`Page` mutation, then stages objects before parenting and edges. `_CAPS` derives shared SVG definitions, `_DRAWIO_CAP` carries target token plus fill semantics for every `EndCap`, and `_rendered` traps provider refusals once as `DrawFault.provider`.
- Cases: SVG lowers every `DiagramGlyph` case through `_shape`, `_marker`, `_CAPS`, `_paint`, and `_caption`; `TextRun` selects face, size, ink, bold, and oblique policy, while mathematical text composes `Formula.laid()` and re-spells `MathFault` through `DrawFault.typeset`. `.drawio` retains editable labels, node shapes, entity header bands, ports, parents, marker rotation, edge routes, and terminal fill. `AreaMark` and `FragmentMark` reject through `DrawFault.unrepresentable` because drawpyo owns no faithful arbitrary-ring or path object.
- Entry: `DiagramDraw.emit` returns ONE `ArtifactWork` per rendered kind (suite construction is `core/issue#ISSUE`'s `Diagrams` arm, which constructs `DiagramDraw(glyphs=..., palette=...)` per assigned layout); `_key` is `ContentIdentity.key` over the `_seed` length-framed canonical chunks — `(tag, payload)` glyph rows, palette octets, frame/target/font bundle — minted PRE-RUN so keyed admission probes the warm seed and `receipt.slot == node.key`; `_emit` renders once through `self.lane.offload(Kernel.of(..., KernelTrait.RELEASING))` (the kernel touches the isolate-unsafe `ziafont`/`numpy` C-extensions the subinterpreter cannot load, so the thread arm is its one placement) and mints the receipt; `layered()` projects the SVG arm's `graphic/layer#LAYER` `LayerPlan`, each root's intent from the `_INTENT` class. `_render_svg` serializes EACH named `Group` to its OWN `<svg>`; the `.drawio` kernel serializes `File.xml` into a standalone diagrams.net-editable file (the `load_diagram` inverse ingests a template, mutates `get_by_id` rows, and re-emits).
- Growth: a new mark element is one `DiagramGlyph` case plus one `_lower` arm plus one `_INTENT` row; a new node silhouette one `NodeShape` row plus one `_shape` arm plus one `_DRAWIO_STYLE` row; a new marker one `MarkerKind`/`_marker` pair plus one `_DRAWIO_MARKER` row; a new crow's-foot terminal one glyphset `ER_CAPS` row; a new generic terminal one `EndCap` row plus one `_cap_glyph` arm plus one `_DRAWIO_CAP` row; a new style axis one `GlyphStyle` field; a new named layer a new `GlyphStyle.layer` value the `_groups` partition already buckets; a new egress arm one `DrawTarget` row plus one `DrawArtifact` case plus one `_render_*` arm; a new route regime one glyphset `EdgeRoute` member plus one `_DRAWIO_ROUTE` row. A print/PDF-X plane requiring non-scaling outlined strokes composes `graphic/vector/region#REGION` `RegionOp.Outline`/`RegionOp.Boolean` one hop through the vector owner, never a draw-owned `pathops` import; a CAD-native diagram deliverable is `export/dxf#DXF`'s `Diagram` arm consuming the same positioned glyph sequence under the `drawing/regime#REGIME` pen vocabulary, never a draw-local ezdxf arm shipping vendor-default linework.
- Boundary: pre-run canonical input owns node identity, and rendering never fingerprints a second byte stream. Layout supplies coordinates and routes; `hex_ramp` supplies color; SVG labels outline to paths, while `.drawio` labels remain editable source text. Typed refusal replaces every silent payload drop.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import xml.etree.ElementTree as ET
from collections.abc import Iterable
from enum import StrEnum
from functools import lru_cache
from itertools import chain
from typing import Final, Literal, assert_never

import drawsvg as draw
import ziafont
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, json

from builtins import frozendict
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp
from rasm.artifacts.graphic.layer import EDITORIAL, LayerContent, LayerIntent, LayerMeta, LayerNode, LayerPlan
from rasm.artifacts.typography.math import Formula, FormulaSpec, MixedSpec
from rasm.artifacts.visualization.diagram.glyphset import (
    DiagramGlyph,
    EdgeMark,
    EdgeRoute,
    EndCap,
    ENTITY_BAND,
    ER_CAPS,
    GlyphStyle,
    GlyphTag,
    MarkerKind,
    NodeMark,
    NodeShape,
    SwimlaneMark,
    TextAnchor,
    TextRun,
)
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy from drawpyo import File, Page
lazy from drawpyo.diagram import Edge as DrawioEdge, Object as DrawioObject

# --- [TYPES] ----------------------------------------------------------------------------
type Paint = dict[str, object]
type CapDefs = dict[EndCap, "draw.Marker"]


class DrawTarget(StrEnum):
    SVG = "svg"  # drawsvg named-layer SVG -> export/layered
    DRAWIO = "drawio"  # drawpyo editable .drawio (mxGraph XML)


# --- [CONSTANTS] ------------------------------------------------------------------------
_PRECISION: Final[float] = 3.0  # ziafont d-float places; the content-key determinism lever
_INK: Final[str] = "#000000"  # readable label ink over any light fill; annotations use the palette
_CANON: Final = json.Encoder(order="deterministic")
_HALIGN: Final[Map[TextAnchor, str]] = Map.of_seq([  # closed TextAnchor -> ziafont/Formula halign domain
    (TextAnchor.START, "left"),
    (TextAnchor.MIDDLE, "center"),
    (TextAnchor.END, "right"),
])
_INTENT: Final[frozendict[GlyphTag, LayerIntent]] = frozendict({
    # layer-intent class per glyph tag; export/layered routes whole intent classes on it
    "node": LayerIntent.LINEWORK,
    "edge": LayerIntent.LINEWORK,
    "swimlane": LayerIntent.BACKGROUND,
    "annotation": LayerIntent.ANNOTATION,
    "marker": LayerIntent.SYMBOL,
    "area": LayerIntent.LINEWORK,
    "fragment": LayerIntent.REFERENCE,
})
_DRAWIO_UNSPELLABLE: Final[frozenset[str]] = frozenset({"area", "fragment"})  # outside the drawpyo object vocabulary; refused at admission
_DRAWIO_MARKER: Final[Map[MarkerKind, str]] = Map.of_seq([
    # draw.io style tokens verbatim; the `rotation` attribute carries the mark angle
    (MarkerKind.DOT, "ellipse"),
    (MarkerKind.ARROW, "triangle"),
    (MarkerKind.TICK, "line"),
    (MarkerKind.NORTH, "triangle"),
    (MarkerKind.CROSS, "cross"),
])
_DRAWIO_STYLE: Final[Map[NodeShape, str]] = Map.of_seq([
    # draw.io style tokens verbatim; RECTANGLE rides the default body, and ENTITY uses the swimlane header.
    (NodeShape.DIAMOND, "rhombus"),
    (NodeShape.OVAL, "ellipse"),
    (NodeShape.PARALLELOGRAM, "parallelogram"),
    (NodeShape.HEXAGON, "hexagon"),
    (NodeShape.CYLINDER, "cylinder3"),
    (NodeShape.ENTITY, "swimlane"),
    (NodeShape.DOCUMENT, "shape=mxgraph.flowchart.document"),
    (NodeShape.MULTI_DOCUMENT, "shape=mxgraph.flowchart.multi-document"),
    (NodeShape.PREDEFINED_PROCESS, "shape=mxgraph.flowchart.predefined_process"),
    (NodeShape.MANUAL_INPUT, "shape=mxgraph.flowchart.manual_input"),
    (NodeShape.MANUAL_OPERATION, "shape=mxgraph.flowchart.manual_operation"),
    (NodeShape.OFF_PAGE, "shape=mxgraph.flowchart.off-page_reference"),
    (NodeShape.STORED_DATA, "shape=mxgraph.flowchart.stored_data"),
    (NodeShape.DISPLAY, "shape=mxgraph.flowchart.display"),
    (NodeShape.DELAY, "shape=mxgraph.flowchart.delay"),
    (NodeShape.CONNECTOR, "shape=mxgraph.flowchart.on-page_reference"),
    (NodeShape.CARD, "shape=mxgraph.flowchart.card"),
    (NodeShape.TAPE, "shape=mxgraph.flowchart.paper_tape"),
])
# maps the layout-resolved EdgeMark.route regime onto drawpyo waypoint tokens — total over the closed EdgeRoute vocabulary.
_DRAWIO_ROUTE: Final[frozendict[EdgeRoute, str]] = frozendict({EdgeRoute.LINES: "straight", EdgeRoute.SPLINES: "curved", EdgeRoute.ORTHOGONAL: "orthogonal"})
_DRAWIO_CAP: Final[Map[EndCap, tuple[str, bool]]] = Map.of_seq([
    # drawpyo terminal token plus fill semantics; NONE never reaches the lookup.
    (EndCap.ARROW, ("classic", True)),
    (EndCap.OPEN, ("open", False)),
    (EndCap.BLOCK, ("block", True)),
    (EndCap.DIAMOND_OPEN, ("diamond", False)),
    (EndCap.DIAMOND_FILLED, ("diamond", True)),
    (EndCap.CIRCLE, ("circle", False)),
    (EndCap.CROSS, ("cross", False)),
    (EndCap.ER_ONE, ("ERone", False)),
    (EndCap.ER_MANY, ("ERmany", False)),
    (EndCap.ER_ZERO_ONE, ("ERzeroToOne", False)),
    (EndCap.ER_ONE_MANY, ("ERoneToMany", False)),
    (EndCap.ER_ZERO_MANY, ("ERzeroToMany", False)),
])


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DrawArtifact:
    tag: Literal["layered", "drawio"] = tag()
    layered: tuple[LayerNode, ...] = case()  # SVG arm -> graphic/layer LayerNode rows the exporters compose
    drawio: bytes = case()  # drawio arm -> standalone editable .drawio bytes


@tagged_union(frozen=True)
class DrawFault:
    tag: Literal["unrepresentable", "reference", "layer_intent", "typeset", "provider", "many"] = tag()
    unrepresentable: tuple[str, ...] = case()  # glyph tags the selected target cannot spell faithfully
    reference: tuple[str, ...] = case()
    layer_intent: tuple[str, str, str] = case()
    typeset: str = case()  # a Formula MathFault re-spelled at the label seam
    provider: str = case()
    many: tuple["DrawFault", ...] = case()


class DiagramDraw(Struct, frozen=True):
    glyphs: tuple[DiagramGlyph, ...]
    palette: Palette
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    width: float = 800.0
    height: float = 600.0
    target: DrawTarget = DrawTarget.SVG
    font_family: str | None = None  # None -> the bundled DejaVuSans outline fallback

    def emit(self, /) -> "Iterable[ArtifactWork]":
        # ONE node per DiagramKind render; suite construction is core/issue's Diagrams arm.
        return (ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.glyphs) or 1)),)

    @property
    def _seed(self) -> tuple[bytes, ...]:
        # length-framed canonical preimage chunks (patterns rows [05]/[06]); the lane is execution policy, outside the preimage.
        framed = lambda chunk: len(chunk).to_bytes(8, "little") + chunk
        rows = _CANON.encode(tuple((glyph.tag, glyph.mark) for glyph in self.glyphs))
        bundle = _CANON.encode((self.width, self.height, self.target.value, self.font_family))
        return (framed(rows), framed(self.palette.tobytes()), framed(bundle))

    @property
    def _key(self) -> ContentKey:
        # PRE-RUN key over the canonical input; receipt.slot == node.key, render determinism pinned by _PRECISION/svg2.
        return ContentIdentity.key(f"diagram-{self.target}", self._seed)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        crossed = await self.lane.offload(Kernel.of(self._rendered, KernelTrait.RELEASING))
        return crossed.bind(lambda inner: inner.map(lambda pair: pair[1]).map_error(self._fault))

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # named diagram layers as one LayerPlan tree the exporters compose; SVG-arm projection.
        crossed = await self.lane.offload(Kernel.of(self._rendered, KernelTrait.RELEASING))
        return crossed.bind(
            lambda inner: inner.bind(
                lambda pair: Ok(LayerPlan(schema=EDITORIAL, roots=pair[0].layered))
                if pair[0].tag == "layered"
                else Error(DrawFault(unrepresentable=("layered",)))
            ).map_error(self._fault)
        )

    def _fault(self, fault: DrawFault, /) -> BoundaryFault:
        return BoundaryFault(boundary=(f"diagram.draw.{self.target}", fault.tag))

    def _rendered(self) -> Result[tuple[DrawArtifact, ArtifactReceipt], DrawFault]:
        # one synchronous render kernel; crosses the runtime thread lane.
        try:
            match self.target:
                case DrawTarget.SVG:
                    return self._render_svg()
                case DrawTarget.DRAWIO:
                    return self._render_drawio()
                case _ as unreachable:
                    assert_never(unreachable)
        except (AttributeError, KeyError, OSError, TypeError, ValueError) as bad:
            return Error(DrawFault(provider=f"{self.target}:{bad}"))

    def _tally(self) -> tuple[int, int]:
        return (sum(1 for g in self.glyphs if g.tag == "node"), sum(1 for g in self.glyphs if g.tag == "edge"))

    def _render_svg(self) -> Result[tuple[DrawArtifact, ArtifactReceipt], DrawFault]:
        ziafont.config.precision = _PRECISION  # idempotent global render policy; every render writes the same value
        ziafont.config.svg2 = True  # inline <path> egress (no <symbol>/<use>)
        ramp = hex_ramp(self.palette)
        face = ziafont.Font(self.font_family)
        nodes, edges = self._tally()

        def _artifact(groups: dict[str, tuple[LayerIntent, "draw.Group"]]) -> tuple[DrawArtifact, ArtifactReceipt]:
            rendered = tuple((name, intent, self._layer_svg(group)) for name, (intent, group) in sorted(groups.items()))
            leaves = tuple(LayerNode.Leaf(LayerMeta(name=name, intent=intent), LayerContent.Fragment(svg)) for name, intent, svg in rendered)
            volume = sum(len(svg) for _, _, svg in rendered)
            return (DrawArtifact(layered=leaves), ArtifactReceipt.Diagram(self._key, "diagram-svg", nodes, edges, "drawsvg", volume))

        return self._groups(ramp, face, _cap_defs()).map(_artifact)

    def _groups(self, ramp: tuple[str, ...], face: "ziafont.Font", caps: CapDefs) -> Result[dict[str, tuple[LayerIntent, "draw.Group"]], DrawFault]:
        groups: dict[str, tuple[LayerIntent, draw.Group]] = {}
        faults: list[DrawFault] = []
        for glyph in self.glyphs:  # Exemption: drawsvg Group is a MutableSequence sink; admission still accumulates independent glyph faults.
            match _lower(glyph, ramp, face, caps):
                case Result(tag="error", error=fault):
                    faults.append(fault)
                case Result(tag="ok", ok=elements):
                    style = glyph.mark.style
                    expected = _INTENT[glyph.tag]
                    if (current := groups.get(style.layer)) is not None and current[0] is not expected:
                        faults.append(DrawFault(layer_intent=(style.layer, current[0].value, expected.value)))
                    else:  # membership-guarded mint: the Group constructs only for an absent layer, never as a discarded setdefault default
                        _intent, group = current if current is not None else groups.setdefault(style.layer, (expected, draw.Group(id=style.layer)))
                        group.extend(elements)
        return Error(faults[0] if len(faults) == 1 else DrawFault(many=tuple(faults))) if faults else Ok(groups)

    def _layer_svg(self, group: "draw.Group") -> bytes:
        canvas = draw.Drawing(self.width, self.height, origin=(0, 0))
        canvas.append(group)
        return canvas.as_svg().encode()

    @staticmethod
    def _parent_cycles(parents: dict[int, int], /) -> tuple[int, ...]:
        cyclic: list[int] = []
        for origin in parents:  # Exemption: each parent-pointer chain is a bounded admission trace with no provider combinator.
            seen: set[int] = set()
            cursor = origin
            while cursor in parents and cursor not in seen:
                seen.add(cursor)
                cursor = parents[cursor]
            if cursor in seen:
                cyclic.append(origin)
        return tuple(cyclic)

    def _render_drawio(self) -> Result[tuple[DrawArtifact, ArtifactReceipt], DrawFault]:
        # target admission first: a payload the object vocabulary cannot spell refuses, never degrades.
        if refused := tuple(sorted({glyph.tag for glyph in self.glyphs if glyph.tag in _DRAWIO_UNSPELLABLE})):
            return Error(DrawFault(unrepresentable=refused))
        marks = tuple(glyph.mark for glyph in self.glyphs)
        owner_rows = tuple(mark for mark in marks if isinstance(mark, NodeMark | SwimlaneMark))
        owner_ids = tuple(mark.index for mark in owner_rows)
        owner_set = frozenset(owner_ids)
        port_rows = tuple((mark.index, port.id) for mark in marks if isinstance(mark, NodeMark) for port in mark.ports)
        ports = frozenset(port_rows)
        parents = {mark.index: mark.parent for mark in owner_rows if mark.parent is not None}
        references = (
            *(f"duplicate:{index}" for position, index in enumerate(owner_ids) if index in owner_ids[:position]),
            *(f"duplicate_port:{index}:{port}" for position, (index, port) in enumerate(port_rows) if (index, port) in port_rows[:position]),
            *(f"parent:{mark.parent}" for mark in owner_rows if mark.parent is not None and mark.parent not in owner_set),
            *(f"parent_cycle:{index}" for index in self._parent_cycles(parents)),
            *(
                f"edge:{mark.source}->{mark.target}"
                for mark in marks
                if isinstance(mark, EdgeMark) and (mark.source not in owner_set or mark.target not in owner_set)
            ),
            *(
                f"source_port:{mark.source}:{mark.source_port}"
                for mark in marks
                if isinstance(mark, EdgeMark) and mark.source_port is not None and (mark.source, mark.source_port) not in ports
            ),
            *(
                f"target_port:{mark.target}:{mark.target_port}"
                for mark in marks
                if isinstance(mark, EdgeMark) and mark.target_port is not None and (mark.target, mark.target_port) not in ports
            ),
        )
        if references:
            return Error(DrawFault(reference=references))
        ramp = hex_ramp(self.palette)
        doc = File()
        page = Page(file=doc)
        placed: dict[int, object] = {}
        seats: dict[tuple[int, str], object] = {}
        for glyph in (glyph for glyph in self.glyphs if glyph.tag != "edge"):  # Exemption: drawpyo mutates the File/Page tree imperatively.
            _lower_drawio(glyph, page, ramp, placed, seats)
        for index, parent in parents.items():  # one parenting site: the admission-proven parent map rebases after every owner is placed
            placed[index].parent = placed[parent]
        for glyph in (glyph for glyph in self.glyphs if glyph.tag == "edge"):
            _lower_drawio(glyph, page, ramp, placed, seats)
        nodes, edges = self._tally()
        data = doc.xml.encode()
        return Ok((DrawArtifact(drawio=data), ArtifactReceipt.Diagram(self._key, "diagram-drawio", nodes, edges, "drawpyo", len(data))))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _cap_defs() -> CapDefs:
    # one shared <defs> Marker per EndCap; drawsvg auto-collects and dedupes by id. `context-stroke` inherits the edge stroke, so one def serves every
    # index; `refX=2` anchors the terminal tip ON the vertex and `auto-start-reverse` points a start cap outward — the CAD cap-block insert parity
    # (drawsvg emits no refX itself, so the default 0 would overshoot every tip by half the cap).
    defs: CapDefs = {}
    for cap in EndCap:  # Exemption: the def table assembles once per render; Marker is a mutable defs container
        if cap is EndCap.NONE:
            continue
        marker = draw.Marker(-2.4, -1.6, 2.4, 1.6, orient="auto-start-reverse", refX=2, id=f"cap-{cap.value}")
        marker.append(_er_cap(*ER_CAPS[cap]) if cap in ER_CAPS else _cap_glyph(cap))
        defs[cap] = marker
    return defs


def _er_cap(ring: bool, bar: bool, fan: bool, /) -> "draw.DrawingElement":
    # crow's-foot builder: every ER cap is a (ring, bar, fan) composition stacked toward the terminal — fan at the
    # end, bar behind it, ring behind that — never a copied arm per cardinality.
    duo = draw.Group()
    if ring:
        duo.append(draw.Circle(-1.6 if fan else -1.4, 0, 0.7, fill="none", stroke="context-stroke", stroke_width=0.4))
    if bar:
        bar_x = -1.2 if fan else 0.4
        duo.append(draw.Line(bar_x, -1.4, bar_x, 1.4, stroke="context-stroke", stroke_width=0.4))
    if fan:
        duo.append(draw.Path(stroke="context-stroke", stroke_width=0.4, fill="none").M(-0.4, 0).L(2, -1.4).M(-0.4, 0).L(2, 0).M(-0.4, 0).L(2, 1.4))
    return duo


def _cap_glyph(cap: EndCap) -> "draw.DrawingElement":
    match cap:
        case EndCap.ARROW:
            return draw.Lines(-2, -1.4, 2, 0, -2, 1.4, close=True, fill="context-stroke")
        case EndCap.OPEN:
            return draw.Lines(-2, -1.4, 2, 0, -2, 1.4, close=False, fill="none", stroke="context-stroke", stroke_width=0.4)
        case EndCap.BLOCK:
            return draw.Rectangle(-1.4, -1.2, 2.8, 2.4, fill="context-stroke")
        case EndCap.DIAMOND_OPEN:
            return draw.Lines(-2, 0, 0, -1.4, 2, 0, 0, 1.4, close=True, fill="none", stroke="context-stroke", stroke_width=0.4)
        case EndCap.DIAMOND_FILLED:
            return draw.Lines(-2, 0, 0, -1.4, 2, 0, 0, 1.4, close=True, fill="context-stroke")
        case EndCap.CIRCLE:
            return draw.Circle(0, 0, 1.2, fill="none", stroke="context-stroke", stroke_width=0.4)
        case EndCap.CROSS:
            return draw.Path(stroke="context-stroke", stroke_width=0.4, fill="none").M(-1, -1.4).L(1, 1.4).M(-1, 1.4).L(1, -1.4)
        case _ as unreachable:  # the ER family derives through _er_cap and never reaches this fold
            assert_never(unreachable)


def _dash(style: GlyphStyle) -> Paint:
    return {"stroke_dasharray": ",".join(f"{run:g}" for run in style.dash)} if style.dash else {}


def _stroke_policy(style: GlyphStyle) -> Paint:
    return {"stroke_linecap": style.cap.value, "stroke_linejoin": style.join.value}


def _paint(style: GlyphStyle, ramp: tuple[str, ...]) -> Paint:
    return {
        "fill": ramp[style.fill % len(ramp)],
        "stroke": ramp[style.stroke % len(ramp)],
        "stroke_width": style.width,
        "fill_opacity": style.opacity,
        **_stroke_policy(style),
        **_dash(style),
    }


@lru_cache(maxsize=16)
def _named_face(family: str) -> "ziafont.Font":
    # per-family sfnt parse memo; a TextRun weight/italic variant names its styled face file here.
    return ziafont.Font(family)


def _caption(
    face: "ziafont.Font",
    style: GlyphStyle,
    ramp: tuple[str, ...],
    text: str,
    base: float,
    x: float,
    y: float,
    anchor: TextAnchor,
    ink: str | None = None,
    angle: float = 0.0,
) -> Result["draw.DrawingElement", DrawFault]:
    # one TextRun resolution seam: size, face family, and ink index come off the run; absent -> defaults.
    run = style.text
    size = run.size if run is not None else base
    color = ramp[run.ink % len(ramp)] if run is not None and run.ink is not None else (ink or _INK)
    return _label(face, text, size, x, y, anchor, color, run, angle)


def _label(
    face: "ziafont.Font",
    text: str,
    size: float,
    x: float,
    y: float,
    halign: TextAnchor,
    color: str,
    run: TextRun | None = None,
    angle: float = 0.0,
) -> Result["draw.DrawingElement", DrawFault]:
    # plain text outlines through ziafont; a `$math$` label composes the typography/math Formula owner — never a second ziamath import.
    align = _HALIGN[halign]
    chosen = _named_face(run.family) if run is not None and run.family else face
    if "$" in text:
        laid = Formula(spec=FormulaSpec(mixed=MixedSpec(source=text, size=size, color=color, halign=align))).laid()
        # `seat` is the canonical math placement fold — the origin derives from fragment height and baseline,
        # and rotation applies around that seated origin, never the raw anchor.
        return laid.map(lambda frag: _seated(frag.svg, *seat(frag, x, y), angle)).map_error(lambda fault: DrawFault(typeset=fault.tag))
    frag = chosen.text(text, size=size, halign=align, color=color).svgxml()
    inner = "".join(ET.tostring(child, encoding="unicode") for child in frag)  # svg2 -> prefix-free inline <path>/<g>
    return Ok(_transformed(inner, x, y, run, size, color, angle))


def _seated(svg: str, x: float, y: float, angle: float, /) -> "draw.DrawingElement":
    # a Formula fragment carries its own typography; the seat applies placement only, never synthetic bold/oblique.
    spin = f" rotate({angle:g})" if angle else ""
    group = draw.Group(transform=f"translate({x},{y}){spin}")
    group.append(draw.Raw(svg))
    return group


def _transformed(inner: str, x: float, y: float, run: TextRun | None, size: float, color: str, angle: float, /) -> "draw.DrawingElement":
    slant = " skewX(-12)" if run is not None and run.italic else ""  # synthetic oblique when the face itself is upright
    spin = f" rotate({angle:g})" if angle else ""
    bold: Paint = {"stroke": color, "stroke_width": size * 0.04} if run is not None and run.weight >= 600 else {}  # synthetic bold: outline stroke
    group = draw.Group(transform=f"translate({x},{y}){spin}{slant}", **bold)
    group.append(draw.Raw(inner))
    return group


def _shape(shape: NodeShape, x: float, y: float, w: float, h: float, paint: Paint, corner: float = 0.0) -> "draw.DrawingElement":
    match shape:
        case NodeShape.RECTANGLE:
            rounding: Paint = {"rx": corner, "ry": corner} if corner > 0.0 else {}
            return draw.Rectangle(x, y, w, h, **rounding, **paint)
        case NodeShape.ENTITY:  # titled record: outer box + header divider the title rides under
            record = draw.Group()
            record.append(draw.Rectangle(x, y, w, h, **paint))
            record.append(draw.Line(x, y + ENTITY_BAND, x + w, y + ENTITY_BAND, stroke=paint["stroke"], stroke_width=paint["stroke_width"]))
            return record
        case NodeShape.OVAL:
            return draw.Ellipse(x + w / 2, y + h / 2, w / 2, h / 2, **paint)
        case NodeShape.DIAMOND:
            return draw.Lines(x + w / 2, y, x + w, y + h / 2, x + w / 2, y + h, x, y + h / 2, close=True, **paint)
        case NodeShape.PARALLELOGRAM:
            skew = w * 0.2
            return draw.Lines(x + skew, y, x + w, y, x + w - skew, y + h, x, y + h, close=True, **paint)
        case NodeShape.HEXAGON:
            cut = w * 0.15
            return draw.Lines(x + cut, y, x + w - cut, y, x + w, y + h / 2, x + w - cut, y + h, x + cut, y + h, x, y + h / 2, close=True, **paint)
        case NodeShape.CYLINDER:
            cap = h * 0.12
            return (
                draw
                .Path(**paint)
                .M(x, y + cap)
                .A(w / 2, cap, 0, 0, 1, x + w, y + cap)
                .L(x + w, y + h - cap)
                .A(w / 2, cap, 0, 0, 1, x, y + h - cap)
                .Z()
            )
        case NodeShape.DOCUMENT:  # wavy base
            dip = h * 0.15
            return (
                draw
                .Path(**paint)
                .M(x, y)
                .L(x + w, y)
                .L(x + w, y + h - dip)
                .C(x + w * 0.66, y + h + dip, x + w * 0.33, y + h - 3 * dip, x, y + h - dip)
                .Z()
            )
        case NodeShape.MULTI_DOCUMENT:  # offset back sheets + a front DOCUMENT body (arm recursion)
            off = min(w, h) * 0.1
            stack = draw.Group()
            stack.append(draw.Rectangle(x + 2 * off, y, w - 2 * off, h - 2 * off, **paint))
            stack.append(draw.Rectangle(x + off, y + off, w - 2 * off, h - 2 * off, **paint))
            stack.append(_shape(NodeShape.DOCUMENT, x, y + 2 * off, w - 2 * off, h - 2 * off, paint))
            return stack
        case NodeShape.PREDEFINED_PROCESS:
            bar = w * 0.12
            sub = draw.Group()
            sub.append(draw.Rectangle(x, y, w, h, **paint))
            sub.append(draw.Line(x + bar, y, x + bar, y + h, stroke=paint["stroke"], stroke_width=paint["stroke_width"]))
            sub.append(draw.Line(x + w - bar, y, x + w - bar, y + h, stroke=paint["stroke"], stroke_width=paint["stroke_width"]))
            return sub
        case NodeShape.MANUAL_INPUT:
            return draw.Lines(x, y + h * 0.25, x + w, y, x + w, y + h, x, y + h, close=True, **paint)
        case NodeShape.MANUAL_OPERATION:
            inset = w * 0.15
            return draw.Lines(x, y, x + w, y, x + w - inset, y + h, x + inset, y + h, close=True, **paint)
        case NodeShape.OFF_PAGE:
            return draw.Lines(x, y, x + w, y, x + w, y + h * 0.6, x + w / 2, y + h, x, y + h * 0.6, close=True, **paint)
        case NodeShape.STORED_DATA:  # both edges bow left
            bow = w * 0.15
            return (
                draw
                .Path(**paint)
                .M(x + bow, y)
                .L(x + w, y)
                .A(bow, h / 2, 0, 0, 0, x + w, y + h)
                .L(x + bow, y + h)
                .A(bow, h / 2, 0, 0, 1, x + bow, y)
                .Z()
            )
        case NodeShape.DISPLAY:
            nose = w * 0.2
            return (
                draw
                .Path(**paint)
                .M(x + nose, y)
                .L(x + w - nose, y)
                .A(nose, h / 2, 0, 0, 1, x + w - nose, y + h)
                .L(x + nose, y + h)
                .L(x, y + h / 2)
                .Z()
            )
        case NodeShape.DELAY:
            return draw.Path(**paint).M(x, y).L(x + w - h / 2, y).A(h / 2, h / 2, 0, 0, 1, x + w - h / 2, y + h).L(x, y + h).Z()
        case NodeShape.CONNECTOR:
            return draw.Circle(x + w / 2, y + h / 2, min(w, h) / 2, **paint)
        case NodeShape.CARD:  # clipped top-left corner
            cut = min(w, h) * 0.25
            return draw.Lines(x + cut, y, x + w, y, x + w, y + h, x, y + h, x, y + cut, close=True, **paint)
        case NodeShape.TAPE:  # mirrored S-waves top and base
            dip = h * 0.12
            return (
                draw
                .Path(**paint)
                .M(x, y + dip)
                .C(x + w * 0.33, y - dip, x + w * 0.66, y + 3 * dip, x + w, y + dip)
                .L(x + w, y + h - dip)
                .C(x + w * 0.66, y + h + dip, x + w * 0.33, y + h - 3 * dip, x, y + h - dip)
                .Z()
            )
        case _ as unreachable:
            assert_never(unreachable)


def _lower(glyph: DiagramGlyph, ramp: tuple[str, ...], face: "ziafont.Font", caps: CapDefs) -> Result[tuple["draw.DrawingElement", ...], DrawFault]:
    match glyph:
        case DiagramGlyph(tag="node", node=n):
            base: tuple[draw.DrawingElement, ...] = (
                _shape(n.shape, n.x, n.y, n.w, n.h, _paint(n.style, ramp), n.style.corner),
                *(draw.Circle(*port.seat(n.x, n.y, n.w, n.h), max(1.5, n.style.width), fill=ramp[n.style.stroke % len(ramp)]) for port in n.ports),
            )
            if not n.label:
                return Ok(base)
            title_y = n.y + ENTITY_BAND / 2 if n.shape is NodeShape.ENTITY else n.y + n.h / 2  # ENTITY titles ride the header band
            return _caption(face, n.style, ramp, n.label, 10.0, n.x + n.w / 2, title_y, TextAnchor.MIDDLE).map(lambda el: (*base, el))
        case DiagramGlyph(tag="edge", edge=e):
            terminal: Paint = {slot: caps[cap] for slot, cap in (("marker_start", e.caps[0]), ("marker_end", e.caps[1])) if cap is not EndCap.NONE}
            line = draw.Lines(
                *chain.from_iterable(e.points),
                close=False,
                fill="none",
                stroke=ramp[e.style.stroke % len(ramp)],
                stroke_width=e.weight or e.style.width,
                **terminal,
                **_stroke_policy(e.style),
                **_dash(e.style),
            )
            if not e.label:
                return Ok((line,))
            return _caption(face, e.style, ramp, e.label, 8.0, *e.points[len(e.points) // 2], TextAnchor.MIDDLE).map(lambda el: (line, el))
        case DiagramGlyph(tag="swimlane", swimlane=s):
            band = draw.Rectangle(s.x, s.y, s.w, s.h, **{**_paint(s.style, ramp), "fill_opacity": s.style.opacity * 0.4})
            if not s.title:
                return Ok((band,))
            return _caption(face, s.style, ramp, s.title, 9.0, s.x + 4, s.y + 12, TextAnchor.START).map(lambda el: (band, el))
        case DiagramGlyph(tag="annotation", annotation=a):
            return _caption(face, a.style, ramp, a.text, 9.0, a.x, a.y, a.anchor, ramp[a.style.fill % len(ramp)], a.angle).map(lambda el: (el,))
        case DiagramGlyph(tag="marker", marker=m):
            return Ok((_marker(m.x, m.y, m.kind, m.angle, m.style, ramp),))
        case DiagramGlyph(tag="area", area=r):
            ring = draw.Lines(*chain.from_iterable(r.ring), close=True, **_paint(r.style, ramp))
            if (text := r.label or (f"{r.magnitude:g}" if r.magnitude else None)) is None:
                return Ok((ring,))
            return _caption(face, r.style, ramp, text, 9.0, *r.centroid, TextAnchor.MIDDLE).map(lambda el: (ring, el))
        case DiagramGlyph(tag="fragment", fragment=f):
            path = draw.Path(d=f.d, fill="none", stroke=ramp[f.style.stroke % len(ramp)], stroke_width=f.style.width, **_dash(f.style))
            if not f.label:
                return Ok((path,))
            return _caption(face, f.style, ramp, f.label, 8.0, *f.anchor, TextAnchor.MIDDLE, ramp[f.style.stroke % len(ramp)]).map(lambda el: (path, el))
        case _ as unreachable:
            assert_never(unreachable)


def _marker(x: float, y: float, kind: MarkerKind, angle: float, style: GlyphStyle, ramp: tuple[str, ...]) -> "draw.DrawingElement":
    fill = ramp[style.fill % len(ramp)]
    match kind:
        case MarkerKind.DOT:
            return draw.Circle(x, y, style.width * 2, fill=fill)
        case MarkerKind.ARROW:  # barbed head keeps ARROW distinct from TICK, matching the drawio arm's triangle-vs-line split
            return (
                draw
                .Path(stroke=fill, stroke_width=style.width, fill="none", transform=f"rotate({angle},{x},{y})")
                .M(x - 4, y)
                .L(x + 4, y)
                .M(x + 1.5, y - 2)
                .L(x + 4, y)
                .L(x + 1.5, y + 2)
            )
        case MarkerKind.TICK:
            return draw.Path(stroke=fill, stroke_width=style.width, transform=f"rotate({angle},{x},{y})").M(x - 4, y).L(x + 4, y)
        case MarkerKind.NORTH:
            return (
                draw
                .Path(fill=fill, stroke=fill, stroke_width=style.width, transform=f"rotate({angle},{x},{y})")
                .M(x, y - 6)
                .L(x - 3, y + 4)
                .L(x + 3, y + 4)
                .Z()
            )
        case MarkerKind.CROSS:
            return draw.Path(stroke=fill, stroke_width=style.width).M(x - 3, y).L(x + 3, y).M(x, y - 3).L(x, y + 3)
        case _ as unreachable:
            assert_never(unreachable)


def _text_format(style: GlyphStyle) -> dict[str, str]:
    # TextRun -> draw.io label typography: fontStyle is the bold|italic bitmask, fontSize the run size, fontFamily the named face.
    run = style.text
    if run is None:
        return {}
    mask = (1 if run.weight >= 600 else 0) | (2 if run.italic else 0)
    return {
        **({"fontStyle": str(mask)} if mask else {}),
        **({"fontFamily": run.family} if run.family else {}),
        "fontSize": f"{run.size:g}",
    }


def _drawio_dash(style: GlyphStyle) -> dict[str, str]:
    # vertex mirror of the edge arm's `pattern=` selection: mxGraph vertices dash through `dashed`/`dashPattern`,
    # so nodes and swimlanes honor GlyphStyle.dash exactly as edges and the SVG `_paint` fold already do.
    return {"dashed": "1", "dashPattern": " ".join(f"{run:g}" for run in style.dash)} if style.dash else {}


def _lower_drawio(glyph: DiagramGlyph, page: object, ramp: tuple[str, ...], placed: dict[int, object], seats: dict[tuple[int, str], object]) -> None:
    match glyph:
        case DiagramGlyph(tag="node", node=n):
            obj = DrawioObject(value=n.label or "", position=(n.x, n.y), page=page, width=n.w, height=n.h)
            if (token := _DRAWIO_STYLE.try_find(n.shape).default_value(None)) is not None:
                obj.apply_style_string(token)  # draw.io style token verbatim
            obj.apply_attribute_dict({
                "fillColor": ramp[n.style.fill % len(ramp)],
                "strokeColor": ramp[n.style.stroke % len(ramp)],
                **({"rounded": "1"} if n.style.corner > 0.0 else {}),
                **({"startSize": f"{ENTITY_BAND:g}"} if n.shape is NodeShape.ENTITY else {}),
                **_drawio_dash(n.style),
                **_text_format(n.style),
            })
            for port in n.ports:  # each port is an editable seat child; an edge naming the port binds it
                px, py = port.seat(n.x, n.y, n.w, n.h)
                seat = DrawioObject(value=port.label or "", position=(px - 3, py - 3), page=page, width=6, height=6)
                seat.apply_style_string("ellipse")
                seat.apply_attribute_dict({"fillColor": ramp[n.style.stroke % len(ramp)]})
                seat.parent = obj
                seats[(n.index, port.id)] = seat
            placed[n.index] = obj
        case DiagramGlyph(tag="edge", edge=e):
            terminal = {
                key: value
                for suffix, cap in (("source", e.caps[0]), ("target", e.caps[1]))
                if cap is not EndCap.NONE
                for token, filled in (_DRAWIO_CAP[cap],)
                for key, value in ((f"line_end_{suffix}", token), (f"endFill_{suffix}", "1" if filled else "0"))
            }
            edge = DrawioEdge(
                page=page,
                # admission proved every endpoint and every named seat, so the reads are total; an unnamed port never falls through to a seat
                source=seats[(e.source, e.source_port)] if e.source_port is not None else placed[e.source],
                target=seats[(e.target, e.target_port)] if e.target_port is not None else placed[e.target],
                label=e.label or "",
                waypoints=_DRAWIO_ROUTE[e.route],
                pattern="dashed_medium" if e.style.dash else "solid",
                strokeColor=ramp[e.style.stroke % len(ramp)],
                strokeWidth=e.weight or e.style.width,
                **terminal,
            )
            for px, py in e.points[1:-1]:
                edge.add_point(px, py)  # interior waypoints for route fidelity
        case DiagramGlyph(tag="swimlane", swimlane=s):
            band = DrawioObject(value=s.title or "", position=(s.x, s.y), page=page, width=s.w, height=s.h)
            band.apply_attribute_dict({
                "container": "1",
                "fillColor": ramp[s.style.fill % len(ramp)],
                "opacity": "40",
                **_drawio_dash(s.style),
                **_text_format(s.style),
            })
            placed[s.index] = band
        case DiagramGlyph(tag="annotation", annotation=a):
            note = DrawioObject(value=a.text, position=(a.x, a.y), page=page, width=120, height=20)
            note.apply_attribute_dict({
                "fillColor": "none",
                "strokeColor": "none",
                "fontColor": ramp[a.style.fill % len(ramp)],
                **({"rotation": f"{a.angle:g}"} if a.angle else {}),
                **_text_format(a.style),
            })
        case DiagramGlyph(tag="marker", marker=m):
            dot = DrawioObject(value="", position=(m.x, m.y), page=page, width=8, height=8)
            dot.apply_style_string(_DRAWIO_MARKER[m.kind])  # kind token verbatim; rotation keeps the angle recoverable
            dot.apply_attribute_dict({"fillColor": ramp[m.style.fill % len(ramp)], **({"rotation": f"{m.angle:g}"} if m.angle else {})})
        case DiagramGlyph(tag="area") | DiagramGlyph(tag="fragment"):
            return None  # structurally unreachable: _render_drawio refused these tags at admission, never a silent lowering
        case _ as unreachable:
            assert_never(unreachable)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "DiagramDraw",
    "DrawArtifact",
    "DrawFault",
    "DrawTarget",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
