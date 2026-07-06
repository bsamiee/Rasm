# [PY_ARTIFACTS_DIAGRAM_DRAW]

The named-layer diagram emission owner. `DiagramDraw` is ONE owner folding the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` sequence the `visualization/diagram/layout#LAYOUT` coordinate assignment emits into a rendered artifact through the egress arm the `DrawTarget` selects: the `drawsvg` named-layer SVG arm (the default, feeding `export/layered#LAYERED`) or the `drawpyo` editable-`.drawio` arm (a standalone diagrams.net file), both lowering the SAME closed `DiagramGlyph` grammar through one total `match`, never two parallel owners. The glyph fold is one total dispatch over the closed `DiagramGlyph` case — `Node` to its `NodeShape` geometry (a `Rectangle`, a decision `Lines` diamond, a terminal `Ellipse`, an ER `ENTITY` record) plus its `Port` connection marks plus an outlined label, `Edge` to a `Lines`/`Path` polyline (its stroke width scaled by the Sankey `weight`) with a shared arrowhead `Marker`, `Swimlane` to a translucent band, `Annotation` to an outlined text or `ziamath` formula callout, `Marker` to the `MarkerKind`-keyed glyph. Every label mark is outlined to font-independent `<path>` geometry through `ziafont` — a `$math$` annotation through `ziamath` — deleting the font-dependent `drawsvg.Text`/`<text>` element that renders wrong in any consumer lacking the font, so the emitted diagram is self-contained. Color arrives from `graphic/color/derive#DERIVE` as the `ColorReceipt.coords` palette array, the `GlyphStyle.fill`/`stroke` integer indices projected to hex through the shared `hex_ramp`. The draw owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case (the SVG arm `"diagram-svg"`/`"drawsvg"`, the `.drawio` arm `"diagram-drawio"`/`"drawpyo"`); the `export/layered#LAYERED` owner contributes the named-layer `Preview`/`Egress` evidence off the SVG arm's bound `Layer` rows. The draw owner composites nothing and re-renders nothing.

## [01]-[INDEX]

- [01]-[DRAW]: the `DiagramDraw` named-layer emitter folding the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` sequence through the `DrawTarget`-selected egress arm — the `drawsvg` SVG arm bucketing each mark into its `GlyphStyle.layer` named `Group` and outlining every label through `ziafont`/`ziamath` to `<path>` geometry, or the `drawpyo` `.drawio` arm lowering the SAME grammar to editable mxGraph objects — both lowering the closed `DiagramGlyph` case (its `NodeShape`/`Port`/`weight`/`dash` topology) through one total `match`, threading the `graphic/color/derive#DERIVE` palette index to hex through `hex_ramp`, returning one closed `DrawArtifact` (`Layered` named-layer rows the `export/layered#LAYERED` owner binds, or the `.drawio` `Drawio` bytes) and contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` facts.

## [02]-[DRAW]

- Owner: `DiagramDraw` the one diagram emitter discriminating the egress on the `DrawTarget` (`SVG`/`DRAWIO`) policy value and the mark on the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` case, both over one total `match`. The SVG arm folds each mark to a `drawsvg` element and buckets it into one named `drawsvg` `Group` per `GlyphStyle.layer` so the diagram emits as named SVG layers; the `Drawing` is the one document canvas the `append`/`draw(..., z=)` polymorphic insertion surface orders, never a per-shape `add_rect`/`add_circle` family. The `.drawio` arm folds each mark to one `drawpyo` `Object`/`Edge` on a `File`/`Page` spine, its `NodeShape` lowered through `apply_style_string` (or the `object_from_library('flowchart', 'decision')` shape factory) and its `GlyphStyle` projected through the TOML-validated `waypoints`/`pattern`/`line_pattern` style axes, serialized through one `File.xml`. Both arms resolve color from the `graphic/color/derive#DERIVE` `ColorReceipt.coords` array the `GlyphStyle.fill`/`stroke` indices key through the shared `hex_ramp` RGB-to-hex projection (imported from `visualization/chart/spec#CHART` where it is declared once), never a per-mark hex literal. In the SVG arm the arrowhead `Marker` is the one shared `<defs>`-tier owner the `Edge` arm references through `marker_end=`; `drawsvg` auto-collects that referenced def into each per-layer `<svg>` and dedupes it by id, so the arrowhead is declared once and never duplicated inline per edge, while the `MarkerKind` dot/tick/north/cross glyphs draw inline as their own `Circle`/`Path` elements (the cross a two-subpath `Path` so it renders as a true `+`, never a one-polyline zigzag).
- Cases: the glyph fold is one `match` over the closed `DiagramGlyph` case, never a knob. In the SVG arm — `Node` lowers to its `NodeShape` silhouette through the `_shape` fold (`RECTANGLE` a `drawsvg.Rectangle`, `ENTITY` a titled record — an outer `Rectangle` plus a `Line` header divider one `_ENTITY_BAND` down under which the title rides, never a plain box, `OVAL` an `Ellipse`, `DIAMOND`/`PARALLELOGRAM`/`HEXAGON` a closed `Lines`, `CYLINDER` an arc `Path`) plus a `_port_marks` `Circle` per `Port` on the resolved node side plus an outlined label placed in the header band for an `ENTITY` and centered otherwise · `Edge` lowers to a `drawsvg.Lines(*flatten(points), close=False, stroke=ramp[style.stroke])` polyline whose `stroke_width` is the Sankey `weight` (or `style.width` when unweighted) and whose `stroke_dasharray` is `GlyphStyle.dash`, plus a `marker_end=` arrowhead `Marker` def reference and an outlined mid-point label · `Swimlane` lowers to a `drawsvg.Rectangle` band with reduced `opacity` plus an outlined title · `Annotation` lowers to an outlined text (or a `ziamath` formula when the text carries `$`) callout · `Marker` lowers to the `MarkerKind`-keyed glyph (the `DOT` `Circle`, the `ARROW`/`TICK`/`NORTH` `Path` rotated by `angle`, the `CROSS` two-subpath `Path`) — each element appended to its `GlyphStyle.layer` `Group`. In the `.drawio` arm the SAME total `match` lowers `Node` to an `Object` (its `NodeShape` set through `apply_style_string`, its color through `apply_attribute_dict`), `Edge` to an `Edge` (its interior `points` pushed through `add_point`, its `weight` the `strokeWidth`, its `dash` the `pattern`), `Swimlane` to a parent container `Object`, `Annotation`/`Marker` to a styled `Object`. Every label is outlined through `ziafont` `Font(...).text(label, size, halign, color).svgxml()` composited into the layer group as a positioned `drawsvg.Raw` `<path>` fragment (`ziafont.config.svg2`/`precision` fixed once so the `d`-float rounding is deterministic for the content key), never a font-dependent `drawsvg.Text`.
- Entry: `DiagramDraw.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[DrawArtifact, ArtifactReceipt]]`, and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` fact — the node/edge counts the `Node`/`Edge` glyph tallies, the `kind`/`algorithm` the arm descriptors (`"diagram-svg"`/`"drawsvg"` for the SVG arm, `"diagram-drawio"`/`"drawpyo"` for the `.drawio` arm); `_compute` discriminates the `DrawTarget` and offloads the one synchronous render kernel onto `to_thread.run_sync(self._render_svg | self._render_drawio, limiter=_DRAW_LANES)` so the `drawsvg`/`ziafont`/`ziamath` XML work and the pure-Python `drawpyo` serialization run off the event loop in the shared address space with zero serialization — the thread arm the offload law keys to a kernel touching the isolate-unsafe `ziafont`/`ziamath`/`numpy` C-extensions (the subinterpreter the `to_interpreter` arm targets cannot load them), the same `to_thread` arm the GIL-releasing-native `rustworkx` layout sibling takes. The SVG kernel builds the shared arrowhead `Marker`, buckets each `DiagramGlyph` into its `GlyphStyle.layer` `Group` through the `_lower` fold threading the `hex_ramp`-projected palette and the `ziafont`-outlined labels, serializes EACH named `Group` to its OWN `<svg>` through a per-layer `Drawing.as_svg()`, derives the content key through `ContentIdentity.of` over the joined per-layer SVG bytes (the rendered facts, never a second render), and returns `DrawArtifact.Layered(tuple[Layer, ...])` the `export/layered#LAYERED` owner binds. The `.drawio` kernel builds the `File`/`Page`, folds each `DiagramGlyph` to its `Object`/`Edge`, content-keys the `File.xml` bytes, and returns `DrawArtifact.Drawio(bytes)` — a standalone diagrams.net-editable file (the `load_diagram` inverse ingests a template, mutates its `get_by_id` rows, and re-emits through the same arm).
- Growth: a new mark element is one `DiagramGlyph` case (in `visualization/diagram/glyphset#GLYPHSET`) plus one `_lower` arm; a new node silhouette is one `NodeShape` row plus one `_shape` arm plus one `_DRAWIO_STYLE` row; a new marker shape is one `MarkerKind` row plus one `_marker` arm; a new shared `<defs>` owner (a gradient band, a hatch pattern) is one `Edge`/`Marker` reference `drawsvg` auto-collects; a new style axis (a corner radius) is one `GlyphStyle` field threaded into the consuming `_lower`/`_node` arm (the `dash` axis is exactly this growth); a new named layer is a new `GlyphStyle.layer` value the `_groups` partition already buckets; a new egress arm (a print-native outlined-stroke plane, an IDML hand-off) is one `DrawTarget` row plus one `DrawArtifact` case plus one `_render_*` arm; zero new surface for a new layer or a new node shape. A print/PDF-X plane requiring non-scaling outlined strokes composes `graphic/vector/region#REGION` `RegionOp.outline` (`skia-pathops` stroke-to-outline of the `Edge` polylines) and `RegionOp.boolean` (`Swimlane`/`Node` overlap union) one hop through the vector owner — a pending growth axis, never a draw-owned `pathops` import.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
import xml.etree.ElementTree as ET
from collections.abc import Iterable
from enum import StrEnum
from itertools import chain
from typing import Literal, assert_never

import drawsvg as draw
import ziafont
from anyio import CapacityLimiter, to_thread
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer
from artifacts.visualization.chart.spec import Palette, hex_ramp
from artifacts.visualization.diagram.glyphset import DiagramGlyph, GlyphStyle, MarkerKind, NodeShape, Port, PortSide, TextAnchor

lazy import ziamath
lazy from drawpyo import File, Page
lazy from drawpyo.diagram import Edge as DrawioEdge, Object as DrawioObject

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Paint = dict[str, object]


class DrawTarget(StrEnum):
    SVG = "svg"  # drawsvg named-layer SVG -> export/layered
    DRAWIO = "drawio"  # drawpyo editable .drawio (mxGraph XML)


# --- [CONSTANTS] ------------------------------------------------------------------------
_DRAW_LANES = CapacityLimiter(os.process_cpu_count() or 4)
_PRECISION: float = 3.0  # ziafont/ziamath d-float places; the content-key stability lever
_INK: str = "#000000"  # readable label ink over any light fill; annotations use the palette
_ENTITY_BAND: float = 16.0  # ER-entity title-band height: the header divider offset + the title-run y-position
_HALIGN: frozendict[TextAnchor, str] = frozendict({  # closed TextAnchor -> ziafont/ziamath halign domain, no dual-spelling normalization
    TextAnchor.START: "left",
    TextAnchor.MIDDLE: "center",
    TextAnchor.END: "right",
})
_DRAWIO_STYLE: frozendict[NodeShape, str] = frozendict({
    NodeShape.DIAMOND: "rhombus",
    NodeShape.OVAL: "ellipse",
    NodeShape.PARALLELOGRAM: "parallelogram",
    NodeShape.HEXAGON: "hexagon",
    NodeShape.CYLINDER: "cylinder3",
})


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DrawArtifact:
    tag: Literal["layered", "drawio"] = tag()
    layered: tuple[Layer, ...] = case()  # SVG arm -> export/layered named-layer rows
    drawio: bytes = case()  # drawio arm -> standalone editable .drawio bytes

    @staticmethod
    def Layered(layers: tuple[Layer, ...]) -> "DrawArtifact":
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

    async def render(self) -> RuntimeRail[tuple[DrawArtifact, ArtifactReceipt]]:
        return await async_boundary(f"diagram.draw.{self.target}", self._compute)

    async def _compute(self) -> tuple[DrawArtifact, ArtifactReceipt]:
        match self.target:
            case DrawTarget.SVG:
                return await to_thread.run_sync(self._render_svg, limiter=_DRAW_LANES)
            case DrawTarget.DRAWIO:
                return await to_thread.run_sync(self._render_drawio, limiter=_DRAW_LANES)
            case _ as unreachable:
                assert_never(unreachable)

    def _tally(self) -> tuple[int, int]:
        return (sum(1 for g in self.glyphs if g.tag == "node"), sum(1 for g in self.glyphs if g.tag == "edge"))

    def _render_svg(self) -> tuple[DrawArtifact, ArtifactReceipt]:
        ziafont.config.precision = _PRECISION  # global render policy set once inside the serialized lane
        ziafont.config.svg2 = True  # inline <path> egress (no <symbol>/<use>); ziamath.config.svg2 delegates
        ramp = hex_ramp(self.palette)
        face = ziafont.Font(self.font_family)
        groups = self._groups(ramp, face, _arrow())
        bbox = (0.0, 0.0, self.width, self.height)
        layers = tuple(Layer(name=name, source=self._layer_svg(groups[name]), bbox=bbox) for name in sorted(groups))
        key = ContentIdentity.of("diagram-svg", b"".join(layer.source for layer in layers))
        nodes, edges = self._tally()
        return DrawArtifact.Layered(layers), ArtifactReceipt.Diagram(key, "diagram-svg", nodes, edges, "drawsvg")

    def _layer_svg(self, group: "draw.Group") -> bytes:
        canvas = draw.Drawing(self.width, self.height, origin=(0, 0))
        canvas.append(group)
        return canvas.as_svg().encode()

    def _groups(self, ramp: list[str], face: "ziafont.Font", arrow: "draw.Marker") -> dict[str, "draw.Group"]:
        groups: dict[str, draw.Group] = {}
        for glyph in self.glyphs:  # Exemption: drawsvg Group is a MutableSequence with no functional builder; the layer tree assembles by extend
            style = _style_of(glyph)
            groups.setdefault(style.layer, draw.Group(id=style.layer)).extend(_lower(glyph, ramp, face, arrow))
        return groups

    def _render_drawio(self) -> tuple[DrawArtifact, ArtifactReceipt]:
        ramp = hex_ramp(self.palette)
        doc = File()
        page = Page(file=doc)
        placed: dict[int, object] = {}
        for glyph in self.glyphs:  # Exemption: drawpyo mutates the File/Page tree imperatively; there is no functional document builder
            _lower_drawio(glyph, page, ramp, placed)
        data = doc.xml.encode()
        key = ContentIdentity.of("diagram-drawio", data)
        nodes, edges = self._tally()
        return DrawArtifact.Drawio(data), ArtifactReceipt.Diagram(key, "diagram-drawio", nodes, edges, "drawpyo")


# --- [OPERATIONS] -----------------------------------------------------------------------
def _arrow() -> "draw.Marker":
    arrow = draw.Marker(-1, -1, 1, 1, orient="auto", id="arrow")
    arrow.append(draw.Lines(-1, -1, 1, 0, -1, 1, close=True, fill="context-stroke"))
    return arrow


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
        case _ as unreachable:
            assert_never(unreachable)


def _label(face: "ziafont.Font", text: str, size: float, x: float, y: float, halign: TextAnchor, color: str) -> "draw.DrawingElement":
    align = _HALIGN[halign]
    frag = (
        ziamath.Text(text, size=size, halign=align, color=color).svgxml()
        if "$" in text
        else face.text(text, size=size, halign=align, color=color).svgxml()
    )
    inner = "".join(ET.tostring(child, encoding="unicode") for child in frag)  # svg2 -> prefix-free inline <path>/<g>
    group = draw.Group(transform=f"translate({x},{y})")
    group.append(draw.Raw(inner))
    return group


def _shape(shape: NodeShape, x: float, y: float, w: float, h: float, paint: Paint) -> "draw.DrawingElement":
    match shape:
        case NodeShape.RECTANGLE:
            return draw.Rectangle(x, y, w, h, **paint)
        case NodeShape.ENTITY:  # a titled record: outer box + a header divider under the title band the label rides
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
        case _ as unreachable:
            assert_never(unreachable)


def _port_xy(port: Port, x: float, y: float, w: float, h: float) -> Point:
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


def _lower(glyph: DiagramGlyph, ramp: list[str], face: "ziafont.Font", arrow: "draw.Marker") -> Iterable["draw.DrawingElement"]:
    match glyph:
        case DiagramGlyph(tag="node", node=(_i, x, y, w, h, label, style, shape, ports, _parent)):
            yield _shape(shape, x, y, w, h, _paint(style, ramp))
            for port in ports:
                yield draw.Circle(*_port_xy(port, x, y, w, h), max(1.5, style.width), fill=ramp[style.stroke % len(ramp)])
            if label:
                title_y = (
                    y + _ENTITY_BAND / 2 if shape is NodeShape.ENTITY else y + h / 2
                )  # ENTITY titles ride the header band; every other shape centers
                yield _label(face, label, 10.0, x + w / 2, title_y, TextAnchor.MIDDLE, _INK)
        case DiagramGlyph(tag="edge", edge=(_s, _t, points, label, style, weight)):
            yield draw.Lines(
                *chain.from_iterable(points),
                close=False,
                fill="none",
                stroke=ramp[style.stroke % len(ramp)],
                stroke_width=weight or style.width,
                marker_end=arrow,
                **_dash(style),
            )
            if label:
                yield _label(face, label, 8.0, *points[len(points) // 2], TextAnchor.MIDDLE, _INK)
        case DiagramGlyph(tag="swimlane", swimlane=(_i, x, y, w, h, title, style, _parent)):
            yield draw.Rectangle(x, y, w, h, **{**_paint(style, ramp), "fill_opacity": style.opacity * 0.4})
            if title:
                yield _label(face, title, 9.0, x + 4, y + 12, TextAnchor.START, _INK)
        case DiagramGlyph(tag="annotation", annotation=(x, y, text, anchor, style)):
            yield _label(face, text, 9.0, x, y, anchor, ramp[style.fill % len(ramp)])
        case DiagramGlyph(tag="marker", marker=(x, y, kind, angle, style)):
            yield _marker(x, y, kind, angle, style, ramp)
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


def _lower_drawio(glyph: DiagramGlyph, page: object, ramp: list[str], placed: dict[int, object]) -> None:
    match glyph:
        case DiagramGlyph(tag="node", node=(index, x, y, w, h, label, style, shape, _ports, _parent)):
            obj = DrawioObject(value=label or "", position=(x, y), page=page, width=w, height=h)
            if (token := _DRAWIO_STYLE.get(shape)) is not None:
                obj.apply_style_string(token)  # draw.io base-shape token verbatim (rhombus/ellipse/...)
            obj.apply_attribute_dict({"fillColor": ramp[style.fill % len(ramp)], "strokeColor": ramp[style.stroke % len(ramp)]})
            placed[index] = obj
        case DiagramGlyph(tag="edge", edge=(source, target, points, label, style, weight)):
            edge = DrawioEdge(
                page=page,
                source=placed.get(source),
                target=placed.get(target),
                label=label or "",
                waypoints="orthogonal",
                pattern="dashed_medium" if style.dash else "solid",
                strokeColor=ramp[style.stroke % len(ramp)],
                strokeWidth=weight or style.width,
            )
            for px, py in points[1:-1]:
                edge.add_point(px, py)  # push the laid-out interior waypoints for route fidelity
        case DiagramGlyph(tag="swimlane", swimlane=(index, x, y, w, h, title, style, _parent)):
            band = DrawioObject(value=title or "", position=(x, y), page=page, width=w, height=h)
            band.apply_attribute_dict({"container": "1", "fillColor": ramp[style.fill % len(ramp)], "opacity": "40"})
            placed[index] = band
        case DiagramGlyph(tag="annotation", annotation=(x, y, text, _anchor, style)):
            note = DrawioObject(value=text, position=(x, y), page=page, width=120, height=20)
            note.apply_attribute_dict({"fillColor": "none", "strokeColor": "none", "fontColor": ramp[style.fill % len(ramp)]})
        case DiagramGlyph(tag="marker", marker=(x, y, _kind, _angle, style)):
            dot = DrawioObject(value="", position=(x, y), page=page, width=6, height=6)
            dot.apply_style_string("ellipse")
            dot.apply_attribute_dict({"fillColor": ramp[style.fill % len(ramp)]})
        case _ as unreachable:
            assert_never(unreachable)
```

`DiagramDraw.render` lowers the positioned `DiagramGlyph` sequence through the `DrawTarget`-selected arm. The SVG arm buckets every mark into its `GlyphStyle.layer` named `Group`, each `Group` projected to one `export/layered#LAYERED` `Layer(name, source, bbox)` row bound directly as an editable layer; the `_lower` fold is the one closed-case glyph-to-element dispatch — `Node` to its `NodeShape` silhouette (`_shape`) plus its `Port` marks plus an outlined label, `Edge` to a `Lines` polyline whose width scales with the Sankey `weight` and whose dash is `GlyphStyle.dash`, `Swimlane` to a translucent band, `Annotation` to an outlined text or `ziamath` formula, `Marker` to the `MarkerKind`-keyed glyph — and every label mark is outlined through `ziafont` `Font(...).text(...).svgxml()` composited as a positioned `drawsvg.Raw` `<path>` fragment (with `ziafont.config.svg2`/`precision` fixed once so the outline bytes are deterministic for the content key), deleting the font-dependent `drawsvg.Text` that renders wrong when the font is absent — so the diagram SVG is self-contained. The `.drawio` arm lowers the SAME grammar to `drawpyo` `Object`/`Edge` on a `File`/`Page` spine — `NodeShape` through `apply_style_string`, color through `apply_attribute_dict` from the `hex_ramp`-projected palette, the laid-out waypoints through `add_point`, the dash through the TOML-validated `pattern` axis — serialized through `File.xml` to a standalone diagrams.net-editable file. Both arms return one closed `DrawArtifact` and contribute the shared `ArtifactReceipt.Diagram` case (distinct `kind`/`algorithm` descriptors), the joined per-layer SVG bytes or the `.drawio` XML bytes the content key fingerprints once, the whole synchronous render offloaded onto `to_thread` off the event loop in the shared address space.

## [03]-[RESEARCH]

- [FONT_OUTLINE] [RESOLVED]: every label mark outlines to font-independent `<path>` geometry through `ziafont` (`.api/ziafont.md`), deleting the font-dependent `drawsvg.Text`/`<text>` element the prior `Node`/`Edge`/`Swimlane`/`Annotation` arms emitted (which renders wrong in any consumer lacking the font). The `_label` helper folds `ziafont.Font(family).text(text, size, halign, color).svgxml()` (an ET `<svg>` of inline `<path>` outlines under `ziafont.config.svg2 = True`) into a positioned `drawsvg.Group(transform="translate(x,y)")` holding a `drawsvg.Raw` of the run's prefix-free `<path>` children — the self-contained-diagram fix — while `ziafont.config.precision` is fixed once inside the serialized `to_thread` lane so the `d`-float rounding is deterministic for the `ContentIdentity` content key. A `$math$`-bearing `Annotation` routes to `ziamath.Text` (`.api/ziamath.md`, which composes `ziafont` for its text runs and its own OpenType-MATH layout for the math), a plain label to `ziafont` directly — the two share the `<path>`/`<svg>` egress the layer group composites, so a mixed text+formula callout renders on one SVG canvas with no live `<text>`/MathML dependency. The `ziafont.Font`/`ziamath` render kernel is GIL-bound shared-address-space work on the `to_thread` arm (never the subinterpreter `to_interpreter` arm — the C-extension `ziafont`/`ziamath`/`numpy` neighbors are isolate-hostile).
- [DRAWIO_EGRESS] [RESOLVED]: the `.drawio` arm is a SECOND egress over the SAME `DiagramGlyph` grammar (`.api/drawpyo.md`), selected by `DrawTarget.DRAWIO`, never a parallel owner. One total `match` lowers `Node` to a `drawpyo.diagram.Object` (its `NodeShape` set through the verified `apply_style_string` with the draw.io base-shape token — `rhombus`/`ellipse`/`parallelogram`/`hexagon`/`cylinder3` — or `object_from_library('flowchart', 'decision')` for a full library shape, its color through `apply_attribute_dict`), `Edge` to a `drawpyo.diagram.Edge` (its `weight` the `strokeWidth`, its `dash` the TOML-validated `pattern`, its laid-out interior waypoints pushed through `add_point`), `Swimlane` to a container `Object`, `Annotation`/`Marker` to a styled `Object`; the `GlyphStyle.fill`/`stroke` indices project to hex through the shared `hex_ramp`, `File.xml` serializes the whole tree, and the bytes content-key through `ContentIdentity.of("diagram-drawio", ...)` into the SAME `ArtifactReceipt.Diagram` case with the `"diagram-drawio"`/`"drawpyo"` descriptors — never a parallel drawio-only receipt shape. The inverse `load_diagram` parses a `.drawio` template into typed `Object`/`Edge` rows a round-trip consumer mutates by `get_by_id` and re-emits through the same arm. Both arms ride the one `to_thread`/`_DRAW_LANES` offload; `drawpyo` is pure-Python, so the serialization is shared-address-space work with no native crash risk.
- [LAYERED_BINDING] [RESOLVED]: the SVG arm returns `DrawArtifact.Layered(tuple[Layer, ...])` the `export/layered#LAYERED` owner binds as `drawsvg` named-layer SVG plus `pymupdf`/`pikepdf` PDF OCG optional-content groups — the `ARCHITECTURE.md` `[02]-[SEAMS]` `visualization/diagram/draw → export/layered` edge, the same named-layer egress `graphic/marks/encode → export/layered` makes. One `Layer(name, source, bbox)` per `GlyphStyle.layer` named `Group`, the `source` each named `Group` serialized to its OWN per-layer `<svg>` (the named `<g>` plus the arrowhead def `drawsvg` auto-collects plus the `ziafont`-outlined `<path>` labels) so a layer's source carries only its own marks. The `.drawio` arm's `DrawArtifact.Drawio(bytes)` is a standalone editable deliverable routed to the artifact store directly, not through `export/layered` (a `.drawio` file is not an SVG/PDF-OCG layer). The `tuple[DrawArtifact, ArtifactReceipt]` return threads the polymorphic artifact the caller routes by case — `Layered` to the layered owner, `Drawio` to the store — never a mis-shaped `Egress` call on this owner.
- [DIAGRAM_RECEIPT_FACTS] [RESOLVED]: `DiagramDraw` projects `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm)` against the shared `core/receipt#RECEIPT` `Diagram` case (`diagram: tuple[ContentKey, str, int, int, str]`), the node/edge counts the `Node`/`Edge` glyph tallies and the `kind`/`algorithm` the arm descriptors (`"diagram-svg"`/`"drawsvg"` or `"diagram-drawio"`/`"drawpyo"`), so both egress arms contribute the SAME shared case the `visualization/diagram/layout#LAYOUT` coordinate-assignment owner also contributes (its `kind` the `DiagramKind` value, its `algorithm` the `LayoutPolicy.tag`) — the diagram engine's receipt contributions are the layout+draw `Diagram` facts plus the layered-export `Preview`/`Egress` named-layer evidence, no mis-shaped `Egress` call on this owner and no parallel per-arm receipt rail.
</content>
