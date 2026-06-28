# [PY_ARTIFACTS_DIAGRAM_DRAW]

The `drawsvg` named-layer SVG emission of the laid-out AEC diagram. `DiagramDraw` is ONE owner folding the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` sequence the `visualization/diagram/layout#LAYOUT` coordinate assignment emits into a `drawsvg` `Drawing`, bucketing every mark into its named `Group` by the `GlyphStyle.layer` key so the diagram emits as named SVG layers the `export/layered#LAYERED` OCG/SVG-layer owner binds directly. The glyph fold is one total `match` over the closed `DiagramGlyph` case — `Node` to a `Rectangle` (plus an on-box `Text`), `Edge` to a `Lines`/`Path` polyline (plus a `Marker` def for the arrowhead), `Swimlane` to a `Rectangle` band (plus a `Text` title), `Annotation` to a `Text` callout, `Marker` to the `MarkerKind`-keyed glyph — so a sun-path arc, a circulation connector, and a stacking band all lower through one closed grammar, never a per-mark special case. Color arrives from `graphic/color/derive#DERIVE` as the `ColorReceipt.coords` palette array, the `GlyphStyle.fill`/`stroke` integer indices projected to hex through the shared `hex_ramp`, so every mark's color traces to one palette index. The emitted SVG plus its named-`Group`-per-layer map is the flat handoff the draw owner hands to `export/layered#LAYERED` as a `tuple[Layer, ...]` for the OCG/SVG-layer binding, and the draw owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` carrying the node/edge glyph counts and the `drawsvg` render algorithm; the `export/layered#LAYERED` owner contributes the named-layer `Preview`/`Egress` evidence (element/layer/byte facts) off the bound layers. The draw owner composites nothing and re-renders nothing.

## [01]-[INDEX]

- [01]-[DRAW]: the `DiagramDraw` `drawsvg` named-layer SVG emitter folding the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` sequence into one `Drawing`, bucketing each mark into its `GlyphStyle.layer` named `Group` and lowering the closed `DiagramGlyph` case to its `drawsvg` element through one total `match`, threading the `graphic/color/derive#DERIVE` palette index to hex through `hex_ramp`, handing the named layers to `export/layered#LAYERED` as a `tuple[Layer, ...]` and contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` facts.

## [02]-[DRAW]

- Owner: `DiagramDraw` the one diagram SVG emitter discriminating the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` case over one total `match` folding each mark to its `drawsvg` element, bucketing the elements into one named `drawsvg` `Group` per `GlyphStyle.layer` so the diagram emits as named SVG layers; the `Drawing` is the one document canvas the `append`/`draw(..., z=)` polymorphic insertion surface orders, never a per-shape `add_rect`/`add_circle` family; the palette is the `graphic/color/derive#DERIVE` `ColorReceipt.coords` array the `GlyphStyle.fill`/`stroke` indices key through the shared `hex_ramp` RGB-to-hex projection (imported from `visualization/chart/spec#CHART` where it is declared once), never a per-mark hex literal. The arrowhead `Marker` is the one shared `<defs>`-tier owner the `Edge` arm references through `marker_end=`; `drawsvg` auto-collects that referenced def into each per-layer `<svg>` and dedupes it by id, so the arrowhead is declared once and never duplicated inline per edge, while the `MarkerKind` dot/tick/north/cross glyphs draw inline as their own `Circle`/`Path` elements (the cross a two-subpath `Path` so it renders as a true `+`, never a one-polyline zigzag through the four cardinal points).
- Cases: the glyph fold is one `match` over the closed `DiagramGlyph` case, never a knob — `Node` lowers to a `drawsvg.Rectangle(x, y, w, h, fill=ramp[style.fill], stroke=ramp[style.stroke])` plus an on-box `drawsvg.Text(label, ...)` when the label is non-empty · `Edge` lowers to a `drawsvg.Lines(*flatten(points), close=False, stroke=ramp[style.stroke])` polyline (a `drawsvg.Path` with the laid-out waypoints for a curved route) plus a `marker_end=` arrowhead `Marker` def reference · `Swimlane` lowers to a `drawsvg.Rectangle` band with reduced `opacity` plus a `drawsvg.Text` title · `Annotation` lowers to a `drawsvg.Text(text, x, y, text_anchor=anchor)` callout · `Marker` lowers to the `MarkerKind`-keyed glyph (the `DOT` `Circle`, the `ARROW`/`TICK` `Path` rotated by `angle`, the `NORTH` north-pointing triangle `Path` rotated by `angle`, the `CROSS` two-subpath `Path`) — each element appended to its `GlyphStyle.layer` `Group`, matched by one total `match`/`case` over the `DiagramGlyph.tag`.
- Entry: `DiagramDraw.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]`, and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Diagram(key, "diagram-svg", nodes, edges, "drawsvg")` fact — the node/edge counts the `Node`/`Edge` glyph tallies, the kind the `"diagram-svg"` descriptor, the algorithm the `drawsvg` render engine; `_compute` is the thin async seam offloading the one synchronous `_render` kernel onto `to_thread.run_sync(self._render, limiter=_DRAW_LANES)` so the `drawsvg` serialization runs off the event loop in the shared address space with zero serialization — the thread arm the offload law keys to a kernel that touches the isolate-unsafe `numpy` palette and returns the `msgspec`-backed `Layer`/`ArtifactReceipt` owners (the subinterpreter the `to_interpreter` arm targets cannot load the `numpy`/`msgspec` C-extensions), the same `to_thread` arm the GIL-releasing-native `rustworkx` layout sibling takes. `_render` builds the shared arrowhead `Marker` the `Edge` arm references, buckets each `DiagramGlyph` into its `GlyphStyle.layer` `Group` through the `_lower` glyph fold threading the `hex_ramp`-projected palette, then serializes EACH named `Group` to its OWN `<svg>` through a per-layer `Drawing.as_svg()` (`drawsvg` auto-collecting the referenced arrowhead def into that layer's document) as that layer's `Layer(name, source, bbox)` row, derives the content key through `ContentIdentity.of` over the joined per-layer SVG bytes (the rendered facts, never a second whole-document render), then projects the `Diagram` fact and returns the `tuple[Layer, ...]` the `export/layered#LAYERED` owner binds (that owner contributing the named-layer `Preview`/`Egress` element/layer/byte evidence off the bound layers).
- Growth: a new mark element is one `DiagramGlyph` case (in `visualization/diagram/glyphset#GLYPHSET`) plus one `_lower` arm folding its `drawsvg` element; a new marker shape is one `MarkerKind` row plus one `_marker` match arm; a new shared `<defs>` owner (a gradient band, a hatch pattern) is one `Edge`/`Marker` reference `drawsvg` auto-collects into each per-layer `<svg>`; a new style axis (a dash pattern, a corner radius) is one `GlyphStyle` field threaded into the consuming `_lower` arm; a new named layer is a new `GlyphStyle.layer` value the `_groups` partition already buckets, never a new owner; zero new surface for a new layer.

```python signature
import os
from collections.abc import Iterable
from itertools import chain
from typing import assert_never

import drawsvg as draw
from anyio import CapacityLimiter, to_thread
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer
from artifacts.visualization.chart.spec import Palette, hex_ramp
from artifacts.visualization.diagram.glyphset import DiagramGlyph, GlyphStyle, MarkerKind

type Point = tuple[float, float]
_DRAW_LANES = CapacityLimiter(os.process_cpu_count() or 4)


class DiagramDraw(Struct, frozen=True):
    glyphs: tuple[DiagramGlyph, ...]
    palette: Palette
    width: float = 800.0
    height: float = 600.0

    async def render(self) -> RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]:
        return await async_boundary("diagram.draw.svg", self._compute)

    async def _compute(self) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
        return await to_thread.run_sync(self._render, limiter=_DRAW_LANES)

    def _render(self) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
        ramp = hex_ramp(self.palette)
        groups = self._groups(ramp, self._arrow())
        bbox = (0.0, 0.0, self.width, self.height)
        layers = tuple(Layer(name=name, source=self._layer_svg(groups[name]), bbox=bbox) for name in sorted(groups))
        key = ContentIdentity.of("diagram-svg", b"".join(layer.source for layer in layers))
        nodes = sum(1 for glyph in self.glyphs if glyph.tag == "node")
        edges = sum(1 for glyph in self.glyphs if glyph.tag == "edge")
        return layers, ArtifactReceipt.Diagram(key, "diagram-svg", nodes, edges, "drawsvg")

    @staticmethod
    def _arrow() -> draw.Marker:
        arrow = draw.Marker(-1, -1, 1, 1, orient="auto", id="arrow")
        arrow.append(draw.Lines(-1, -1, 1, 0, -1, 1, close=True, fill="context-stroke"))
        return arrow

    def _layer_svg(self, group: draw.Group) -> bytes:
        canvas = draw.Drawing(self.width, self.height, origin=(0, 0))
        canvas.append(group)
        return canvas.as_svg().encode()

    def _groups(self, ramp: list[str], arrow: draw.Marker) -> dict[str, draw.Group]:
        groups: dict[str, draw.Group] = {}
        for glyph in self.glyphs:  # Exemption: drawsvg Group is a mutable MutableSequence with no functional builder; the layer tree assembles by extend
            style = _style_of(glyph)
            groups.setdefault(style.layer, draw.Group(id=style.layer)).extend(_lower(glyph, ramp, arrow))
        return groups


def _paint(style: GlyphStyle, ramp: list[str]) -> dict[str, object]:
    return {"fill": ramp[style.fill % len(ramp)], "stroke": ramp[style.stroke % len(ramp)], "stroke_width": style.width, "fill_opacity": style.opacity}


def _style_of(glyph: DiagramGlyph) -> GlyphStyle:
    match glyph:
        case DiagramGlyph(tag="node", node=(_i, _x, _y, _w, _h, _label, style)):
            return style
        case DiagramGlyph(tag="edge", edge=(_s, _t, _points, _label, style)):
            return style
        case DiagramGlyph(tag="swimlane", swimlane=(_i, _x, _y, _w, _h, _title, style)):
            return style
        case DiagramGlyph(tag="annotation", annotation=(_x, _y, _text, _anchor, style)):
            return style
        case DiagramGlyph(tag="marker", marker=(_x, _y, _kind, _angle, style)):
            return style
        case _:
            assert_never(glyph)


def _lower(glyph: DiagramGlyph, ramp: list[str], arrow: draw.Marker) -> Iterable[draw.DrawingElement]:
    match glyph:
        case DiagramGlyph(tag="node", node=(_i, x, y, w, h, label, style)):
            yield draw.Rectangle(x, y, w, h, **_paint(style, ramp))
            if label:
                yield draw.Text(label, 10, x + w / 2, y + h / 2, center=True)
        case DiagramGlyph(tag="edge", edge=(_s, _t, points, label, style)):
            yield draw.Lines(*chain.from_iterable(points), close=False, fill="none", stroke=ramp[style.stroke % len(ramp)], stroke_width=style.width, marker_end=arrow)
            if label:
                yield draw.Text(label, 8, *points[len(points) // 2])
        case DiagramGlyph(tag="swimlane", swimlane=(_i, x, y, w, h, title, style)):
            yield draw.Rectangle(x, y, w, h, **{**_paint(style, ramp), "fill_opacity": style.opacity * 0.4})
            if title:
                yield draw.Text(title, 9, x + 4, y + 12)
        case DiagramGlyph(tag="annotation", annotation=(x, y, text, anchor, style)):
            yield draw.Text(text, 9, x, y, text_anchor=anchor, fill=ramp[style.fill % len(ramp)])
        case DiagramGlyph(tag="marker", marker=(x, y, kind, angle, style)):
            yield _marker(x, y, kind, angle, style, ramp)
        case _:
            assert_never(glyph)


def _marker(x: float, y: float, kind: MarkerKind, angle: float, style: GlyphStyle, ramp: list[str]) -> draw.DrawingElement:
    fill = ramp[style.fill % len(ramp)]
    match kind:
        case MarkerKind.DOT:
            return draw.Circle(x, y, style.width * 2, fill=fill)
        case MarkerKind.ARROW | MarkerKind.TICK:
            return draw.Path(stroke=fill, stroke_width=style.width, transform=f"rotate({angle},{x},{y})").M(x - 4, y).L(x + 4, y)
        case MarkerKind.NORTH:
            return draw.Path(fill=fill, stroke=fill, stroke_width=style.width, transform=f"rotate({angle},{x},{y})").M(x, y - 6).L(x - 3, y + 4).L(x + 3, y + 4).Z()
        case MarkerKind.CROSS:
            return draw.Path(stroke=fill, stroke_width=style.width).M(x - 3, y).L(x + 3, y).M(x, y - 3).L(x, y + 3)
        case _:
            assert_never(kind)
```

`DiagramDraw.render` lowers the positioned `DiagramGlyph` sequence into one `drawsvg.Drawing`, bucketing every mark into its `GlyphStyle.layer` named `Group` so the diagram emits as named SVG layers, each `Group` projected to one `export/layered#LAYERED` `Layer(name, source, bbox)` row the OCG/SVG-layer owner binds directly as editable layers. The `_lower` fold is the one closed-case glyph-to-element dispatch — `Node` to a `Rectangle`, `Edge` to a `Lines` polyline with the auto-collected arrowhead `Marker` def, `Swimlane` to a translucent band, `Annotation` to anchored `Text`, `Marker` to the `MarkerKind`-keyed inline glyph (dot `Circle`, tick/arrow `Path`, north triangle `Path`, cross two-subpath `Path`) — and `_paint` projects the `GlyphStyle.fill`/`stroke` palette indices to hex through the shared `hex_ramp` so every mark's color traces to one `graphic/color/derive#DERIVE` palette index. The arrowhead `Marker` is the one shared `<defs>` owner the `Edge` arm references via `marker_end`, `drawsvg` auto-collecting it into each per-layer `<svg>` and deduping it by id, never an inline duplicate; each named `Group` serializes to its OWN `<svg>` through a per-layer `Drawing` so every `Layer.source` carries only its own marks, the joined per-layer SVG bytes are the durable artifact the content key fingerprints, the whole synchronous serialization offloads onto `to_thread` off the event loop in the shared address space, and the draw owner contributes `ArtifactReceipt.Diagram` (node/edge counts + `drawsvg` engine) while `export/layered#LAYERED` contributes the named-layer `Preview`/`Egress` element/layer/byte evidence off the bound `Layer` rows.

## [03]-[RESEARCH]

- [LAYERED_BINDING] [RESOLVED]: the `DiagramDraw.render` return threads the `tuple[Layer, ...]` named-layer rows the `export/layered#LAYERED` owner binds as `drawsvg` named-layer SVG plus `pymupdf`/`pikepdf` PDF OCG optional-content groups — the `ARCHITECTURE.md` `[02]-[SEAMS]` `visualization/diagram/draw → export/layered [LAYERED]: laid-out diagram SVG bound as named layers` edge, the same named-layer egress `graphic/marks/encode → export/layered [LAYERED]` makes. The `export/layered#LAYERED` `Layer(name, source, bbox, visible, locked)` row is the settled binding contract: one `Layer` per `GlyphStyle.layer` named `Group`, the `name` the layer label, the `source` each named `Group` serialized to its OWN per-layer `<svg>` (the named `<g>` plus the arrowhead def `drawsvg` auto-collects) so a layer's source carries only its own marks, the `bbox` the canvas extent — the `SvgLayers`/`PdfOcg` arms fold each row into one `Group(id=name)`/OCG. The return shape `tuple[tuple[Layer, ...], ArtifactReceipt]` mirrors the settled `export/layered#LAYERED` `Layer`-row ingestion; the named-`Group`-per-layer emission projected to one `Layer` row apiece is the settled structure.
- [DIAGRAM_RECEIPT_FACTS] [RESOLVED]: `DiagramDraw._compute` projects `ArtifactReceipt.Diagram(key, "diagram-svg", nodes, edges, "drawsvg")` against the shared `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case (`diagram: tuple[ContentKey, str, int, int, str]` as content key / kind / node count / edge count / algorithm), the node/edge counts the `Node`/`Edge` glyph tallies and the algorithm the `drawsvg` render engine, so the draw owner contributes the SVG-render facts through the `Diagram` case rather than the mis-shaped 5-positional string-literal `Egress` call the real `egress: tuple[ContentKey, int, int, int, int, int]` case rejected; the `DiagramLayout#LAYOUT` coordinate-assignment owner also contributes the `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm)` facts off the built presentation graph (its `kind` the `DiagramKind` value, its `algorithm` the `LayoutPolicy.tag`), and the named-layer element/layer/byte evidence is the `export/layered#LAYERED` owner's `Preview`/`Egress` contribution off the bound `Layer` rows — so the diagram engine's receipt contributions are the layout+draw `Diagram` facts plus the layered-export `Preview`/`Egress` named-layer evidence on the one shared `core/receipt#RECEIPT` family, no mis-shaped `Egress` call on this owner.
