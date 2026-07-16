# [PY_ARTIFACTS_DIAGRAM_DRAW]

`DiagramDraw` is the named-layer diagram emission owner: one owner folding the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` sequence into a rendered artifact through the arm the `DrawTarget` selects — the `drawsvg` named-layer SVG arm (default, feeding `export/layered#LAYERED`) or the `drawpyo` editable-`.drawio` arm — both lowering the SAME closed grammar through one total `match`, never two parallel owners. Every label mark outlines to font-independent `<path>` geometry through `ziafont` (a `$math$` annotation through `ziamath`), deleting the font-dependent `drawsvg.Text` that renders wrong wherever the font is absent, so the emitted diagram is self-contained; color arrives from `graphic/color/derive#DERIVE` as the `Derivation.coords` palette the `GlyphStyle.fill`/`stroke` indices project to hex through `hex_ramp`.

Glyph fold is one total dispatch over the closed `DiagramGlyph` case: `Node` to its `NodeShape` silhouette through the total `_shape` fold (each arm the real ISO 5807 geometry) plus its `Port` marks and outlined label; `Edge` to a polyline whose stroke scales with the Sankey `weight` and whose `caps` reference the shared per-`EndCap` `<defs>` `Marker` table; `Swimlane` to a translucent band; `Annotation` to an outlined text or formula; `Marker` to the `MarkerKind` glyph; `Area` to a closed true-polygon ring labeled at its centroid; `Fragment` to an owned-geometry backdrop path. `DiagramDraw.emit` lands ONE producer node per rendered kind (per-member PRE-RUN key, per-kind elision); `_compute` offloads the one synchronous render kernel onto the runtime thread lane — keyed to a kernel touching the isolate-unsafe `ziafont`/`ziamath`/`numpy` C-extensions the subinterpreter cannot load. Draw contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case (the SVG arm `"diagram-svg"`/`"drawsvg"`, the `.drawio` arm `"diagram-drawio"`/`"drawpyo"`); `export/layered#LAYERED` contributes the named-layer evidence off the SVG arm's `Layer` rows.

## [01]-[INDEX]

- [01]-[DRAW]: `DiagramDraw`'s glyph-to-artifact fold over the `DrawTarget`-selected arm — the `drawsvg` SVG arm bucketing each mark into its `GlyphStyle.layer` `Group` and outlining labels through `ziafont`/`ziamath`, or the `drawpyo` `.drawio` arm lowering the same grammar to editable mxGraph, both threading the palette hex and contributing the `ArtifactReceipt.Diagram` facts.

## [02]-[DRAW]

- Owner: `DiagramDraw` discriminating egress on the `DrawTarget` (`SVG`/`DRAWIO`) and the mark on the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` case, both over one total `match`; the SVG arm buckets each mark into one named `drawsvg` `Group` per `GlyphStyle.layer` on the one `Drawing` canvas whose `append`/`draw(..., z=)` polymorphic insertion orders it, never a per-shape `add_rect`/`add_circle` family; the `.drawio` arm folds each mark to one `drawpyo` `Object`/`Edge` on a `File`/`Page` spine serialized through `File.xml`. Both resolve color through the shared `hex_ramp` (imported from `graphic/color/derive#DERIVE` where it is declared once), never a per-mark hex; the `_CAPS` fold builds one shared `<defs>` `drawsvg.Marker` per referenced `EndCap` with `context-stroke` so one def serves every palette index, `drawsvg` auto-collecting and deduping it by id, while the `MarkerKind` glyphs draw inline (the cross a two-subpath `Path` so it renders a true `+`).
- Cases: one `match` over the closed `DiagramGlyph` case, never a knob. SVG arm — `Node` through the total `_shape` fold (`RECTANGLE` a `Rectangle` carrying `corner` as `rx`/`ry`, `ENTITY` a titled record with a header divider one `_ENTITY_BAND` down, the polygon shapes closed `Lines`, the arc/wave shapes `Path` bodies, `MULTI_DOCUMENT` recursing the `DOCUMENT` arm) plus a `_port_marks` `Circle` per `Port` (a fixed `at` winning over the side/index lane) plus a label in the header band for `ENTITY` else centered; `Edge` a `Lines` polyline whose `stroke_width` is the Sankey `weight` (else `style.width`), whose `caps` reference the `_CAPS` defs; `Swimlane` a reduced-opacity band; `Annotation` an outlined text or `ziamath` formula when the text carries `$`; `Marker` the `MarkerKind` glyph; `Area` a closed true-polygon ring captioned at its centroid; `Fragment` a stroked backdrop `Path`; the `.drawio` arm lowers the SAME `match` to `Object`/`Edge`, its `Area`/`Fragment` bounded by the drawpyo object vocabulary (bounding-box container, anchored label — the true ring is the SVG arm's). Every label outlines through `ziafont`/`ziamath` (`svg2`/`precision` fixed once so the `d`-float rounding is deterministic for the content key), the `TextRun` axis selecting the memoized named face, ink index, synthetic bold (outline stroke), and oblique (skew) — never a font-dependent `drawsvg.Text`.
- Entry: `DiagramDraw.emit` returns ONE `ArtifactWork` per rendered kind, its PRE-RUN key over the canonical glyphs⊕palette⊕frame⊕target⊕font input never over layer bytes (`font_family` is a bytes-producing input); `_emit` resolves the receipt and `layered()` projects the `graphic/layer#LAYERED` `LayerPlan`; the SVG kernel serializes EACH named `Group` to its OWN `<svg>` and keys over the joined per-layer bytes; the `.drawio` kernel keys over the `File.xml` bytes and returns a standalone diagrams.net-editable file (the `load_diagram` inverse ingests a template, mutates `get_by_id` rows, and re-emits).
- Growth: a new mark element is one `DiagramGlyph` case plus one `_lower` arm; a new node silhouette one `NodeShape` row plus one `_shape` arm plus one `_DRAWIO_STYLE` row; a new marker one `MarkerKind`/`_marker` pair; a new terminal one `EndCap` row plus one `_cap_glyph` arm plus one `_DRAWIO_CAP` row; a new shared `<defs>` owner one referenced def `drawsvg` auto-collects; a new style axis one `GlyphStyle` field; a new named layer a new `GlyphStyle.layer` value the `_groups` partition already buckets; a new egress arm one `DrawTarget` row plus one `DrawArtifact` case plus one `_render_*` arm. A print/PDF-X plane requiring non-scaling outlined strokes composes `graphic/vector/region#REGION` `RegionOp.outline`/`RegionOp.boolean` one hop through the vector owner, never a draw-owned `pathops` import.
- Boundary: composites nothing and re-renders nothing — the content key fingerprints the rendered bytes once, never a second render; no coordinate or route computation (`visualization/diagram/layout#LAYOUT`'s — draw folds already-positioned marks); no ad-hoc color (the `hex_ramp` palette); no font-dependent text (every label outlines to `<path>`).

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
from expression import Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.graphic.layer import LayerContent, LayerIntent, LayerNode, LayerPlan, NamingSchema
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp
from artifacts.visualization.diagram.glyphset import (
    DiagramGlyph,
    EndCap,
    GlyphStyle,
    MarkerKind,
    NodeShape,
    Port,
    PortSide,
    TextAnchor,
    TextRun,
)

lazy import ziamath
lazy from drawpyo import File, Page
lazy from drawpyo.diagram import Edge as DrawioEdge, Object as DrawioObject

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Paint = dict[str, object]
type CapDefs = dict[EndCap, "draw.Marker"]


class DrawTarget(StrEnum):
    SVG = "svg"  # drawsvg named-layer SVG -> export/layered
    DRAWIO = "drawio"  # drawpyo editable .drawio (mxGraph XML)


# --- [CONSTANTS] ------------------------------------------------------------------------
_PRECISION: Final[float] = 3.0  # ziafont/ziamath d-float places; the content-key stability lever
_INK: Final[str] = "#000000"  # readable label ink over any light fill; annotations use the palette
_ENTITY_BAND: Final[float] = 16.0  # ER-entity title-band height: the header divider offset + the title-run y-position
_HALIGN: Final[Map[TextAnchor, str]] = Map.of_seq([  # closed TextAnchor -> ziafont/ziamath halign domain, no dual-spelling normalization
    (TextAnchor.START, "left"),
    (TextAnchor.MIDDLE, "center"),
    (TextAnchor.END, "right"),
])
_DRAWIO_STYLE: Final[Map[NodeShape, str]] = Map.of_seq([
    # draw.io style tokens verbatim; RECTANGLE/ENTITY ride the default rect body, the rest lower through mxgraph.flowchart stencil keys.
    (NodeShape.DIAMOND, "rhombus"),
    (NodeShape.OVAL, "ellipse"),
    (NodeShape.PARALLELOGRAM, "parallelogram"),
    (NodeShape.HEXAGON, "hexagon"),
    (NodeShape.CYLINDER, "cylinder3"),
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
_DRAWIO_CAP: Final[Map[EndCap, str]] = Map.of_seq([
    # drawpyo line_end_* glyph vocabulary; NONE never reaches the lookup (guarded at the edge arm).
    (EndCap.ARROW, "classic"),
    (EndCap.OPEN, "open"),
    (EndCap.BLOCK, "block"),
    (EndCap.DIAMOND, "diamond"),
    (EndCap.CIRCLE, "circle"),
    (EndCap.CROSS, "cross"),
    (EndCap.ER_ONE, "ERone"),
    (EndCap.ER_MANY, "ERmany"),
    (EndCap.ER_ZERO_ONE, "ERzeroToOne"),
    (EndCap.ER_ONE_MANY, "ERoneToMany"),
    (EndCap.ER_ZERO_MANY, "ERzeroToMany"),
])


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DrawArtifact:
    tag: Literal["layered", "drawio"] = tag()
    layered: tuple[LayerNode, ...] = case()  # SVG arm -> graphic/layer LayerNode rows the exporters compose
    drawio: bytes = case()  # drawio arm -> standalone editable .drawio bytes

    @staticmethod
    def Layered(layers: tuple[LayerNode, ...]) -> "DrawArtifact":
        return DrawArtifact(layered=layers)

    @staticmethod
    def Drawio(data: bytes) -> "DrawArtifact":
        return DrawArtifact(drawio=data)


class DiagramDraw(Struct, frozen=True):
    glyphs: tuple[DiagramGlyph, ...]
    palette: Palette
    width: float = 800.0
    height: float = 600.0
    target: DrawTarget = DrawTarget.SVG
    font_family: str | None = None  # None -> the bundled DejaVuSans outline fallback

    def emit(self, /) -> "Iterable[ArtifactWork]":
        # ONE node per DiagramKind render; suite construction is core/issue's (`IssueRequest(diagram_suite=...)`).
        return (ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.glyphs) or 1)),)

    @property
    def _key(self) -> ContentKey:
        # key-over-input, minted PRE-RUN never over layer bytes; font_family is a bytes-producing input (`ziafont.Font` outline selection).
        return ContentIdentity.of(
            f"diagram-{self.target}",
            (self.glyphs, self.palette, self.width, self.height, self.target, self.font_family),
            policy=CANONICAL_POLICY,
        )

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        railed = await async_boundary(f"diagram.draw.{self.target}", self._crossed)
        return railed.map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # named diagram layers as one LayerPlan tree the exporters compose.
        railed = await async_boundary(f"diagram.draw.{self.target}", self._crossed)
        return railed.map(lambda pair: LayerPlan(schema=NamingSchema.EDITORIAL, roots=pair[0].layered if pair[0].tag == "layered" else ()))

    async def _crossed(self) -> tuple[DrawArtifact, ArtifactReceipt]:
        # synchronous CPU work; crosses the runtime thread lane.
        match self.target:
            case DrawTarget.SVG:
                arm = self._render_svg
            case DrawTarget.DRAWIO:
                arm = self._render_drawio
            case _ as unreachable:
                assert_never(unreachable)
        crossed = await LanePolicy.offload(arm, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(_draw_raise)


    def _tally(self) -> tuple[int, int]:
        return (sum(1 for g in self.glyphs if g.tag == "node"), sum(1 for g in self.glyphs if g.tag == "edge"))

    def _render_svg(self) -> tuple[DrawArtifact, ArtifactReceipt]:
        ziafont.config.precision = _PRECISION  # global render policy set once inside the serialized lane
        ziafont.config.svg2 = True  # inline <path> egress (no <symbol>/<use>); ziamath.config.svg2 delegates
        ramp = hex_ramp(self.palette)
        face = ziafont.Font(self.font_family)
        groups = self._groups(ramp, face, _cap_defs())
        layers = tuple(LayerNode(name=name, intent=LayerIntent.ANNOTATION, content=Some(LayerContent(fragment=self._layer_svg(groups[name])))) for name in sorted(groups))
        nodes, edges = self._tally()
        return DrawArtifact.Layered(layers), ArtifactReceipt.Diagram(self._key, "diagram-svg", nodes, edges, "drawsvg")

    def _layer_svg(self, group: "draw.Group") -> bytes:
        canvas = draw.Drawing(self.width, self.height, origin=(0, 0))
        canvas.append(group)
        return canvas.as_svg().encode()

    def _groups(self, ramp: list[str], face: "ziafont.Font", caps: CapDefs) -> dict[str, "draw.Group"]:
        groups: dict[str, draw.Group] = {}
        for glyph in self.glyphs:  # Exemption: drawsvg Group is a MutableSequence with no functional builder; the layer tree assembles by extend
            style = _style_of(glyph)
            groups.setdefault(style.layer, draw.Group(id=style.layer)).extend(_lower(glyph, ramp, face, caps))
        return groups

    def _render_drawio(self) -> tuple[DrawArtifact, ArtifactReceipt]:
        ramp = hex_ramp(self.palette)
        doc = File()
        page = Page(file=doc)
        placed: dict[int, object] = {}
        for glyph in self.glyphs:  # Exemption: drawpyo mutates the File/Page tree imperatively; there is no functional document builder
            _lower_drawio(glyph, page, ramp, placed)
        data = doc.xml.encode()
        key = self._key
        nodes, edges = self._tally()
        return DrawArtifact.Drawio(data), ArtifactReceipt.Diagram(key, "diagram-drawio", nodes, edges, "drawpyo")


# --- [OPERATIONS] -----------------------------------------------------------------------
def _draw_raise(fault: object) -> "tuple[DrawArtifact, ArtifactReceipt]":
    # terminal collapse at the render boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _cap_defs() -> CapDefs:
    # one shared <defs> Marker per EndCap; drawsvg auto-collects and dedupes by id. `context-stroke` inherits the edge stroke, so one def serves every index.
    defs: CapDefs = {}
    for cap in EndCap:  # Exemption: the def table assembles once per render; Marker is a mutable defs container
        if cap is EndCap.NONE:
            continue
        marker = draw.Marker(-2.4, -1.6, 2.4, 1.6, orient="auto", id=f"cap-{cap.value}")
        marker.append(_cap_glyph(cap))
        defs[cap] = marker
    return defs


def _cap_glyph(cap: EndCap) -> "draw.DrawingElement":
    match cap:
        case EndCap.ARROW:
            return draw.Lines(-2, -1.4, 2, 0, -2, 1.4, close=True, fill="context-stroke")
        case EndCap.OPEN:
            return draw.Lines(-2, -1.4, 2, 0, -2, 1.4, close=False, fill="none", stroke="context-stroke", stroke_width=0.4)
        case EndCap.BLOCK:
            return draw.Rectangle(-1.4, -1.2, 2.8, 2.4, fill="context-stroke")
        case EndCap.DIAMOND:  # hollow vs solid rides the referencing edge's fill index
            return draw.Lines(-2, 0, 0, -1.4, 2, 0, 0, 1.4, close=True, fill="context-stroke")
        case EndCap.CIRCLE:
            return draw.Circle(0, 0, 1.2, fill="none", stroke="context-stroke", stroke_width=0.4)
        case EndCap.CROSS:
            return draw.Path(stroke="context-stroke", stroke_width=0.4, fill="none").M(-1, -1.4).L(1, 1.4).M(-1, 1.4).L(1, -1.4)
        case EndCap.ER_ONE:
            return draw.Line(0, -1.4, 0, 1.4, stroke="context-stroke", stroke_width=0.4)
        case EndCap.ER_MANY:
            return draw.Path(stroke="context-stroke", stroke_width=0.4, fill="none").M(-1.6, 0).L(2, -1.4).M(-1.6, 0).L(2, 0).M(-1.6, 0).L(2, 1.4)
        case EndCap.ER_ZERO_ONE:
            duo = draw.Group()
            duo.append(draw.Circle(-1.4, 0, 0.7, fill="none", stroke="context-stroke", stroke_width=0.4))
            duo.append(draw.Line(0.4, -1.4, 0.4, 1.4, stroke="context-stroke", stroke_width=0.4))
            return duo
        case EndCap.ER_ONE_MANY:
            duo = draw.Group()
            duo.append(draw.Line(-1.2, -1.4, -1.2, 1.4, stroke="context-stroke", stroke_width=0.4))
            duo.append(draw.Path(stroke="context-stroke", stroke_width=0.4, fill="none").M(-0.4, 0).L(2, -1.4).M(-0.4, 0).L(2, 0).M(-0.4, 0).L(2, 1.4))
            return duo
        case EndCap.ER_ZERO_MANY:
            duo = draw.Group()
            duo.append(draw.Circle(-1.6, 0, 0.7, fill="none", stroke="context-stroke", stroke_width=0.4))
            duo.append(draw.Path(stroke="context-stroke", stroke_width=0.4, fill="none").M(-0.6, 0).L(2, -1.4).M(-0.6, 0).L(2, 0).M(-0.6, 0).L(2, 1.4))
            return duo
        case _ as unreachable:
            assert_never(unreachable)


def _dash(style: GlyphStyle) -> Paint:
    return {"stroke_dasharray": ",".join(f"{run:g}" for run in style.dash)} if style.dash else {}


def _paint(style: GlyphStyle, ramp: list[str]) -> Paint:
    return {
        "fill": ramp[style.fill % len(ramp)],
        "stroke": ramp[style.stroke % len(ramp)],
        "stroke_width": style.width,
        "fill_opacity": style.opacity,
        **_dash(style),
    }


def _style_of(glyph: DiagramGlyph) -> GlyphStyle:
    match glyph:
        case DiagramGlyph(tag="node", node=(_i, _x, _y, _w, _h, _label, style, *_rest)):
            return style
        case DiagramGlyph(tag="edge", edge=(_s, _t, _points, _label, style, *_rest)):
            return style
        case DiagramGlyph(tag="swimlane", swimlane=(_i, _x, _y, _w, _h, _title, style, *_rest)):
            return style
        case DiagramGlyph(tag="annotation", annotation=(_x, _y, _text, _anchor, style)):
            return style
        case DiagramGlyph(tag="marker", marker=(_x, _y, _kind, _angle, style)):
            return style
        case DiagramGlyph(tag="area", area=(_i, _ring, _label, style, *_rest)):
            return style
        case DiagramGlyph(tag="fragment", fragment=(_d, _label, _anchor, style)):
            return style
        case _ as unreachable:
            assert_never(unreachable)


@lru_cache(maxsize=16)
def _named_face(family: str) -> "ziafont.Font":
    # per-family sfnt parse memo; a TextRun weight/italic variant names its styled face file here.
    return ziafont.Font(family)


def _caption(
    face: "ziafont.Font", style: GlyphStyle, ramp: list[str], text: str, base: float, x: float, y: float, anchor: TextAnchor, ink: str | None = None
) -> "draw.DrawingElement":
    # the one TextRun resolution seam: size, face family, and ink index come off the run; absent -> defaults.
    run = style.text
    size = run.size if run is not None else base
    color = ramp[run.ink % len(ramp)] if run is not None and run.ink is not None else (ink or _INK)
    return _label(face, text, size, x, y, anchor, color, run)


def _label(
    face: "ziafont.Font", text: str, size: float, x: float, y: float, halign: TextAnchor, color: str, run: TextRun | None = None
) -> "draw.DrawingElement":
    align = _HALIGN[halign]
    chosen = _named_face(run.family) if run is not None and run.family else face
    frag = (
        ziamath.Text(text, size=size, halign=align, color=color).svgxml()
        if "$" in text
        else chosen.text(text, size=size, halign=align, color=color).svgxml()
    )
    inner = "".join(ET.tostring(child, encoding="unicode") for child in frag)  # svg2 -> prefix-free inline <path>/<g>
    slant = " skewX(-12)" if run is not None and run.italic else ""  # synthetic oblique when the face itself is upright
    bold: Paint = {"stroke": color, "stroke_width": size * 0.04} if run is not None and run.weight >= 600 else {}  # synthetic bold: outline stroke
    group = draw.Group(transform=f"translate({x},{y}){slant}", **bold)
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
            record.append(draw.Line(x, y + _ENTITY_BAND, x + w, y + _ENTITY_BAND, stroke=paint["stroke"], stroke_width=paint["stroke_width"]))
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


def _port_xy(port: Port, x: float, y: float, w: float, h: float) -> Point:
    if port.at is not None:  # fixed node-relative seat wins over the side/index lane (the FIXED_POS port)
        return x + port.at[0], y + port.at[1]
    lane = min(0.85, 0.5 + 0.18 * port.index)
    match port.side:
        case PortSide.NORTH:
            return x + w * lane, y
        case PortSide.SOUTH:
            return x + w * lane, y + h
        case PortSide.WEST:
            return x, y + h * lane
        case PortSide.EAST:
            return x + w, y + h * lane
        case _ as unreachable:
            assert_never(unreachable)


def _centroid(ring: tuple[Point, ...]) -> Point:
    return (sum(px for px, _py in ring) / len(ring), sum(py for _px, py in ring) / len(ring))


def _lower(glyph: DiagramGlyph, ramp: list[str], face: "ziafont.Font", caps: CapDefs) -> Iterable["draw.DrawingElement"]:
    match glyph:
        case DiagramGlyph(tag="node", node=(_i, x, y, w, h, label, style, shape, ports, _parent)):
            yield _shape(shape, x, y, w, h, _paint(style, ramp), style.corner)
            for port in ports:
                yield draw.Circle(*_port_xy(port, x, y, w, h), max(1.5, style.width), fill=ramp[style.stroke % len(ramp)])
            if label:
                title_y = (
                    y + _ENTITY_BAND / 2 if shape is NodeShape.ENTITY else y + h / 2
                )  # ENTITY titles ride the header band; others center
                yield _caption(face, style, ramp, label, 10.0, x + w / 2, title_y, TextAnchor.MIDDLE)
        case DiagramGlyph(tag="edge", edge=(_s, _t, points, label, style, weight, ends)):
            terminal: Paint = {slot: caps[cap] for slot, cap in (("marker_start", ends[0]), ("marker_end", ends[1])) if cap is not EndCap.NONE}
            yield draw.Lines(
                *chain.from_iterable(points),
                close=False,
                fill="none",
                stroke=ramp[style.stroke % len(ramp)],
                stroke_width=weight or style.width,
                **terminal,
                **_dash(style),
            )
            if label:
                yield _caption(face, style, ramp, label, 8.0, *points[len(points) // 2], TextAnchor.MIDDLE)
        case DiagramGlyph(tag="swimlane", swimlane=(_i, x, y, w, h, title, style, _parent)):
            yield draw.Rectangle(x, y, w, h, **{**_paint(style, ramp), "fill_opacity": style.opacity * 0.4})
            if title:
                yield _caption(face, style, ramp, title, 9.0, x + 4, y + 12, TextAnchor.START)
        case DiagramGlyph(tag="annotation", annotation=(x, y, text, anchor, style)):
            yield _caption(face, style, ramp, text, 9.0, x, y, anchor, ramp[style.fill % len(ramp)])
        case DiagramGlyph(tag="marker", marker=(x, y, kind, angle, style)):
            yield _marker(x, y, kind, angle, style, ramp)
        case DiagramGlyph(tag="area", area=(_i, ring, label, style, magnitude, _parent)):
            # closed ring + centroid caption (label, or measured magnitude)
            yield draw.Lines(*chain.from_iterable(ring), close=True, **_paint(style, ramp))
            if (text := label or (f"{magnitude:g}" if magnitude else None)) is not None:
                yield _caption(face, style, ramp, text, 9.0, *_centroid(ring), TextAnchor.MIDDLE)
        case DiagramGlyph(tag="fragment", fragment=(d, label, anchor, style)):
            # solar-furniture backdrop: the d-string strokes verbatim, label at anchor
            yield draw.Path(d=d, fill="none", stroke=ramp[style.stroke % len(ramp)], stroke_width=style.width, **_dash(style))
            if label:
                yield _caption(face, style, ramp, label, 8.0, *anchor, TextAnchor.MIDDLE, ramp[style.stroke % len(ramp)])
        case _ as unreachable:
            assert_never(unreachable)


def _marker(x: float, y: float, kind: MarkerKind, angle: float, style: GlyphStyle, ramp: list[str]) -> "draw.DrawingElement":
    fill = ramp[style.fill % len(ramp)]
    match kind:
        case MarkerKind.DOT:
            return draw.Circle(x, y, style.width * 2, fill=fill)
        case MarkerKind.ARROW | MarkerKind.TICK:
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
    # TextRun -> draw.io label typography: fontStyle is the bold|italic bitmask, fontSize the run size.
    run = style.text
    if run is None:
        return {}
    mask = (1 if run.weight >= 600 else 0) | (2 if run.italic else 0)
    return {**({"fontStyle": str(mask)} if mask else {}), "fontSize": f"{run.size:g}"}


def _lower_drawio(glyph: DiagramGlyph, page: object, ramp: list[str], placed: dict[int, object]) -> None:
    match glyph:
        case DiagramGlyph(tag="node", node=(index, x, y, w, h, label, style, shape, _ports, _parent)):
            obj = DrawioObject(value=label or "", position=(x, y), page=page, width=w, height=h)
            if (token := _DRAWIO_STYLE.try_find(shape).default_value(None)) is not None:
                obj.apply_style_string(token)  # draw.io style token verbatim
            obj.apply_attribute_dict({
                "fillColor": ramp[style.fill % len(ramp)],
                "strokeColor": ramp[style.stroke % len(ramp)],
                **({"rounded": "1"} if style.corner > 0.0 else {}),
                **_text_format(style),
            })
            placed[index] = obj
        case DiagramGlyph(tag="edge", edge=(source, target, points, label, style, weight, ends)):
            terminal = {slot: _DRAWIO_CAP[cap] for slot, cap in (("line_end_source", ends[0]), ("line_end_target", ends[1])) if cap is not EndCap.NONE}
            edge = DrawioEdge(
                page=page,
                source=placed.get(source),
                target=placed.get(target),
                label=label or "",
                waypoints="orthogonal",
                pattern="dashed_medium" if style.dash else "solid",
                strokeColor=ramp[style.stroke % len(ramp)],
                strokeWidth=weight or style.width,
                **terminal,
            )
            for px, py in points[1:-1]:
                edge.add_point(px, py)  # interior waypoints for route fidelity
        case DiagramGlyph(tag="swimlane", swimlane=(index, x, y, w, h, title, style, _parent)):
            band = DrawioObject(value=title or "", position=(x, y), page=page, width=w, height=h)
            band.apply_attribute_dict({"container": "1", "fillColor": ramp[style.fill % len(ramp)], "opacity": "40", **_text_format(style)})
            placed[index] = band
        case DiagramGlyph(tag="annotation", annotation=(x, y, text, _anchor, style)):
            note = DrawioObject(value=text, position=(x, y), page=page, width=120, height=20)
            note.apply_attribute_dict({"fillColor": "none", "strokeColor": "none", "fontColor": ramp[style.fill % len(ramp)], **_text_format(style)})
        case DiagramGlyph(tag="marker", marker=(x, y, _kind, _angle, style)):
            dot = DrawioObject(value="", position=(x, y), page=page, width=6, height=6)
            dot.apply_style_string("ellipse")
            dot.apply_attribute_dict({"fillColor": ramp[style.fill % len(ramp)]})
        case DiagramGlyph(tag="area", area=(index, ring, label, style, magnitude, _parent)):
            # drawpyo carries no arbitrary-polygon geometry: the editable arm lowers the ring to a bounding-box container; the true ring is the SVG arm's.
            xs, ys = tuple(px for px, _py in ring), tuple(py for _px, py in ring)
            box = DrawioObject(
                value=label or (f"{magnitude:g}" if magnitude else ""),
                position=(min(xs), min(ys)),
                page=page,
                width=max(xs) - min(xs),
                height=max(ys) - min(ys),
            )
            box.apply_attribute_dict({"container": "1", "fillColor": ramp[style.fill % len(ramp)], "opacity": "40", **_text_format(style)})
            placed[index] = box
        case DiagramGlyph(tag="fragment", fragment=(_d, label, anchor, style)):
            # backdrop path geometry is the SVG arm's; the editable arm carries the anchored furniture label only
            if label:
                seat = DrawioObject(value=label, position=anchor, page=page, width=60, height=16)
                seat.apply_attribute_dict({"fillColor": "none", "strokeColor": "none", "fontColor": ramp[style.stroke % len(ramp)], **_text_format(style)})
        case _ as unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
