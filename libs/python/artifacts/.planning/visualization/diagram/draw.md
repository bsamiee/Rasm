# [PY_ARTIFACTS_DIAGRAM_DRAW]

The `drawsvg` named-layer SVG emission of the laid-out AEC diagram. `DiagramDraw` is ONE owner folding the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` sequence the `visualization/diagram/layout#LAYOUT` coordinate assignment emits into a `drawsvg` `Drawing`, bucketing every mark into its named `Group` by the `GlyphStyle.layer` key so the diagram emits as named SVG layers the `export/layered#LAYERED` OCG/SVG-layer owner binds directly. The glyph fold is one total `match` over the closed `DiagramGlyph` case — `Node` to a `Rectangle` (plus an on-box `Text`), `Edge` to a `Lines`/`Path` polyline (plus a `Marker` def for the arrowhead), `Swimlane` to a `Rectangle` band (plus a `Text` title), `Annotation` to a `Text` callout, `Marker` to the `MarkerKind`-keyed glyph — so a sun-path arc, a circulation connector, and a stacking band all lower through one closed grammar, never a per-mark special case. Color arrives from `graphic/color/derive#DERIVE` as the `ColorReceipt.coords` palette array, the `GlyphStyle.fill`/`stroke` integer indices projected to hex through the shared `hex_ramp`, so every mark's color traces to one palette index. The emitted SVG plus its named-`Group`-per-layer map is the flat handoff the draw owner hands to `export/layered#LAYERED` as a `tuple[Layer, ...]` for the OCG/SVG-layer binding, and the draw owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` carrying the node/edge glyph counts and the `drawsvg` render algorithm; the `export/layered#LAYERED` owner contributes the named-layer `Preview`/`Egress` evidence (element/layer/byte facts) off the bound layers. The draw owner composites nothing and re-renders nothing.

## [01]-[INDEX]

- [01]-[DRAW]: the `DiagramDraw` `drawsvg` named-layer SVG emitter folding the positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` sequence into one `Drawing`, bucketing each mark into its `GlyphStyle.layer` named `Group` and lowering the closed `DiagramGlyph` case to its `drawsvg` element through one total `match`, threading the `graphic/color/derive#DERIVE` palette index to hex through `hex_ramp`, handing the named layers to `export/layered#LAYERED` as a `tuple[Layer, ...]` and contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` facts.

## [02]-[DRAW]

- Owner: `DiagramDraw` the one diagram SVG emitter discriminating the `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` case over one total `match` folding each mark to its `drawsvg` element, bucketing the elements into one named `drawsvg` `Group` per `GlyphStyle.layer` so the diagram emits as named SVG layers; the `Drawing` is the one document canvas the `append`/`draw(..., z=)` polymorphic insertion surface orders, never a per-shape `add_rect`/`add_circle` family; the palette is the `graphic/color/derive#DERIVE` `ColorReceipt.coords` array the `GlyphStyle.fill`/`stroke` indices key through the shared `hex_ramp` RGB-to-hex projection (imported from `visualization/chart/spec#CHART` where it is declared once), never a per-mark hex literal. `LAYER_DEFS` is the `<defs>`-tier owner registry — the arrowhead `Marker`, the north-arrow symbol, the dimension-tick — registered once through `append_def` and referenced by id, never duplicated inline per edge.
- Cases: the glyph fold is one `match` over the closed `DiagramGlyph` case, never a knob — `Node` lowers to a `drawsvg.Rectangle(x, y, w, h, fill=ramp[style.fill], stroke=ramp[style.stroke])` plus an on-box `drawsvg.Text(label, ...)` when the label is non-empty · `Edge` lowers to a `drawsvg.Lines(*flatten(points), close=False, stroke=ramp[style.stroke])` polyline (a `drawsvg.Path` with the laid-out waypoints for a curved route) plus an `end_marker=` arrowhead `Marker` def reference · `Swimlane` lowers to a `drawsvg.Rectangle` band with reduced `opacity` plus a `drawsvg.Text` title · `Annotation` lowers to a `drawsvg.Text(text, x, y, text_anchor=anchor)` callout · `Marker` lowers to the `MarkerKind`-keyed glyph (the `DOT` `Circle`, the `ARROW`/`TICK` `Path` rotated by `angle`, the `NORTH` north-arrow `Use` of the registered def, the `CROSS` `Lines`) — each element appended to its `GlyphStyle.layer` `Group`, matched by one total `match`/`case` over the `DiagramGlyph.tag`.
- Entry: `DiagramDraw.render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]`, and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Diagram(key, "diagram-svg", nodes, edges, "drawsvg")` fact — the node/edge counts the `Node`/`Edge` glyph tallies, the kind the `"diagram-svg"` descriptor, the algorithm the `drawsvg` render engine; `_compute` seeds the `Drawing` from the canvas extent, registers the `LAYER_DEFS` `<defs>` owners once through `append_def`, buckets each `DiagramGlyph` into its `GlyphStyle.layer` `Group` through the `_lower` glyph fold threading the `hex_ramp`-projected palette, appends each named `Group` to the `Drawing` in layer order, serializes the per-layer `Group` to its own `Layer(name, source, bbox)` row (the whole-document `as_svg()` carrying the named-`<g>` structure as each layer's source), keys the content through `ContentIdentity.of` over the SVG bytes, then projects the `Diagram` fact and returns the `tuple[Layer, ...]` the `export/layered#LAYERED` owner binds (that owner contributing the named-layer `Preview`/`Egress` element/layer/byte evidence off the bound layers).
- Auto: the build is one ordered fold over the glyph sequence into named-layer buckets — `_groups` partitions the `DiagramGlyph` sequence by `GlyphStyle.layer` into one `drawsvg.Group(id=layer)` per distinct layer name so a recolor or a layer toggle is a group-level operation, never a per-element edit; `_lower` discriminates the `DiagramGlyph` case and appends the lowered element to its layer `Group` through the `Drawing.append`/`draw(..., z=)` polymorphic insertion surface, the `z=` ordering the within-layer paint order; `_paint` projects the `GlyphStyle.fill`/`stroke` palette indices to hex through `hex_ramp(palette)[index]` so the color is one palette lookup; the `<defs>` arrowhead/north/tick owners register once through `append_def` and the `Edge`/`Marker` arms reference them by id through `drawsvg.Use` or the `Lines` `end_marker=` keyword, never an inline duplicate; the `Marker` arm folds the `MarkerKind` over a small element map keyed by the bounded shape, the `angle` riding a `drawsvg` `transform="rotate(...)"` so the arrowhead and tick share one rotated-glyph path. The diagram is pure-Python `drawsvg` authoring on the cp315 core — no raster, no subprocess — so the arm resolves in-capsule; the `as_svg()` SVG string is the durable artifact the content key fingerprints.
- Packages: `drawsvg` (`Drawing(width, height, origin=)` the document canvas, `append`/`draw(..., z=)`/`append_def` the polymorphic insertion-and-defs surface, `Group(id=)` the named-layer container, `Rectangle`/`Lines`/`Path`/`Circle`/`Text`/`Use` the element vocabulary, `Marker(minx, miny, maxx, maxy, orient=)` the `<defs>` arrowhead/vertex owner, `Path.M`/`L`/`A`/`arc` the typed path builders, `as_svg()` the SVG-string egress, `all_elements()` the element-count query) MIT pure-Python universal wheel cp315-clean on the core; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`), `core/receipt#RECEIPT` (`ArtifactReceipt`), `export/layered#LAYERED` (`Layer` the named-layer binding row plus the named-layer SVG binding consumer), `graphic/color/derive#DERIVE` (`ColorReceipt.coords` the palette), `visualization/chart/spec#CHART` (`hex_ramp` the shared RGB-to-hex projection), `visualization/diagram/glyphset#GLYPHSET` (`DiagramGlyph`/`GlyphStyle`/`MarkerKind`), `visualization/diagram/layout#LAYOUT` (the positioned `DiagramGlyph` sequence input).
- Growth: a new mark element is one `DiagramGlyph` case (in `visualization/diagram/glyphset#GLYPHSET`) plus one `_lower` arm folding its `drawsvg` element; a new marker shape is one `MarkerKind` row plus one entry in the `Marker` arm's element map; a new `<defs>` owner (a gradient band, a hatch pattern) is one `LAYER_DEFS` registration referenced by id; a new style axis (a dash pattern, a corner radius) is one `GlyphStyle` field threaded into the consuming `_lower` arm; a new named layer is a new `GlyphStyle.layer` value the `_groups` partition already buckets, never a new owner; zero new surface for a new layer.
- Boundary: no coordinate computation (that is `visualization/diagram/layout#LAYOUT`'s — this owner reads the positioned `DiagramGlyph` geometry and lowers it to SVG); no graph analysis or layout (that stays at the layout owner); no rasterization (the `drawsvg` `save_png`/`rasterize` extras are absent on the cp315 core — PNG/PDF routes to `graphic/raster#RASTER` `resvg-py`/`pyvips` downstream, never an in-page blocking call); no compositing or n-up placement (that is `composition/compose#COMPOSE`'s — the emitted SVG is the flat source the placement owner lays out beside its siblings); no ad-hoc color (the palette indices key the `graphic/color/derive#DERIVE` array); a hand-emitted XML tag, a hand-built `d` string, an `add_rect`/`add_circle` insertion family, and a per-mark inline arrowhead are the deleted forms — one `drawsvg` `Drawing`, the `append`/`draw(..., z=)` insertion surface, the `<defs>` owners registered once, every mark bucketed into its named `Group` the `export/layered#LAYERED` owner binds as an editable named layer.

```python signature
from collections.abc import Iterable
from itertools import chain
from typing import assert_never

import drawsvg as draw
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer
from artifacts.visualization.chart.spec import hex_ramp
from artifacts.visualization.diagram.glyphset import DiagramGlyph, GlyphStyle, MarkerKind

type Point = tuple[float, float]


class DiagramDraw(Struct, frozen=True):
    glyphs: tuple[DiagramGlyph, ...]
    palette: object
    width: float = 800.0
    height: float = 600.0

    async def render(self) -> RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]:
        return await async_boundary("diagram.draw.svg", self._compute)

    async def _compute(self) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
        ramp = hex_ramp(self.palette)
        canvas = draw.Drawing(self.width, self.height, origin=(0, 0))
        arrow = draw.Marker(-1, -1, 1, 1, orient="auto", id="arrow")
        arrow.append(draw.Lines(-1, -1, 1, 0, -1, 1, close=True, fill="context-stroke"))
        canvas.append_def(arrow)
        groups = self._groups(ramp, arrow)
        for name in sorted(groups):
            canvas.append(groups[name])
        svg = canvas.as_svg().encode()
        key = ContentIdentity.of("diagram-svg", svg)
        bbox = (0.0, 0.0, self.width, self.height)
        layers = tuple(Layer(name=name, source=svg, bbox=bbox) for name in sorted(groups))
        nodes = sum(1 for glyph in self.glyphs if glyph.tag == "node")
        edges = sum(1 for glyph in self.glyphs if glyph.tag == "edge")
        receipt = ArtifactReceipt.Diagram(key, "diagram-svg", nodes, edges, "drawsvg")
        return layers, receipt

    def _groups(self, ramp: list[str], arrow: draw.Marker) -> dict[str, draw.Group]:
        groups: dict[str, draw.Group] = {}
        for glyph in self.glyphs:
            style = _style_of(glyph)
            group = groups.setdefault(style.layer, draw.Group(id=style.layer))
            for element in _lower(glyph, ramp, arrow):
                group.append(element)
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
            yield _marker(x, y, kind, angle, style, ramp, arrow)
        case _:
            assert_never(glyph)


def _marker(x: float, y: float, kind: MarkerKind, angle: float, style: GlyphStyle, ramp: list[str], arrow: draw.Marker) -> draw.DrawingElement:
    fill = ramp[style.fill % len(ramp)]
    match kind:
        case MarkerKind.DOT:
            return draw.Circle(x, y, style.width * 2, fill=fill)
        case MarkerKind.ARROW | MarkerKind.TICK:
            return draw.Path(stroke=fill, stroke_width=style.width, transform=f"rotate({angle},{x},{y})").M(x - 4, y).L(x + 4, y)
        case MarkerKind.NORTH:
            return draw.Use(arrow, x, y, transform=f"rotate({angle},{x},{y})")
        case MarkerKind.CROSS:
            return draw.Lines(x - 3, y, x + 3, y, x, y - 3, x, y + 3, stroke=fill, stroke_width=style.width)
        case _:
            assert_never(kind)
```

`DiagramDraw.render` lowers the positioned `DiagramGlyph` sequence into one `drawsvg.Drawing`, bucketing every mark into its `GlyphStyle.layer` named `Group` so the diagram emits as named SVG layers, each `Group` projected to one `export/layered#LAYERED` `Layer(name, source, bbox)` row the OCG/SVG-layer owner binds directly as editable layers. The `_lower` fold is the one closed-case glyph-to-element dispatch — `Node` to a `Rectangle`, `Edge` to a `Lines` polyline with the registered arrowhead `Marker`, `Swimlane` to a translucent band, `Annotation` to anchored `Text`, `Marker` to the `MarkerKind`-keyed glyph — and `_paint` projects the `GlyphStyle.fill`/`stroke` palette indices to hex through the shared `hex_ramp` so every mark's color traces to one `graphic/color/derive#DERIVE` palette index. The `<defs>` arrowhead registers once through `append_def` and the `Edge`/`Marker` arms reference it, never an inline duplicate; the `as_svg()` string is the durable artifact the content key fingerprints, and the draw owner contributes `ArtifactReceipt.Diagram` (node/edge counts + `drawsvg` engine) while `export/layered#LAYERED` contributes the named-layer `Preview`/`Egress` element/layer/byte evidence off the bound `Layer` rows.

## [03]-[RESEARCH]

- [DRAWSVG_EMIT]: the `drawsvg` `Drawing(width, height, origin=)` canvas, the `append`/`draw(..., z=)`/`append_def` insertion-and-defs surface, the `Group(id=)` named-layer container, the `Rectangle`/`Lines`/`Path`/`Circle`/`Text`/`Use` element vocabulary, the `Marker(minx, miny, maxx, maxy, orient=)` `<defs>` owner, the `Path.M`/`L`/`A`/`arc` typed builders, the `as_svg()` egress, and the `all_elements()` query verify against the folder `.api/drawsvg.md` catalogue (`2.4.1` reflected, MIT, pure-Python universal wheel cp315-clean with zero install-time dependencies). The `[02]-[PUBLIC_TYPES]` element vocabulary (`Rectangle`/`Circle`/`Text`/`Use`/`Group`/`Lines`/`Path` rows), the def-tier `Marker` owner row, the `[03]-[ENTRYPOINTS]` `Drawing.append`/`draw(..., z=)`/`append_def` build rows and `as_svg`/`all_elements` serialize/query rows, and the `Path` command-builder rows settle the emit surface; the `[04]-[IMPLEMENTATION_LAW]` insertion law "`append`/`draw`/`extend` (each taking `z=`) is the one polymorphic child-insertion surface — there is no `add_rect`/`add_circle` family" is exactly this owner's contract, and the def law "gradients/patterns/filters/clips/masks/markers are `DrawingDef` owners registered once via `append_def`/`draw_def` and referenced by id" is the `LAYER_DEFS` arrowhead pattern. The `[STACKING]` note "a diagram figure builds a `Drawing`, registers a `Marker`/`Filter` via `append_def`, draws the `Rectangle`/`Path`/`Text` vocabulary with `z=` ordering, then `as_svg()` emits the SVG string the figure owner records under a content-key" is this owner's exact rail. The raster-boundary law settles the no-`save_png`-on-core posture — rasterization routes to `graphic/raster#RASTER`. The exact `Lines` arrowhead-reference keyword (`marker_end=` versus an `end_marker=` spelling versus a `marker-end` attribute) and the `Text` `center`/`text_anchor` keyword set are the one [DRAWSVG_MARKER_KWARGS] catalogue-deepen item until a `Lines`/`Text` signature reflection pass enumerates the marker-reference and text-anchor keyword spellings; every `Drawing`/`Group`/element/`append_def`/`as_svg` member spelling is settled fence code.
- [LAYERED_BINDING] [RESOLVED]: the `DiagramDraw.render` return threads the `tuple[Layer, ...]` named-layer rows the `export/layered#LAYERED` owner binds as `drawsvg` named-layer SVG plus `pymupdf`/`pikepdf` PDF OCG optional-content groups — the `ARCHITECTURE.md` `[02]-[SEAMS]` `visualization/diagram/draw → export/layered [LAYERED]: laid-out diagram SVG bound as named layers` edge, the same named-layer egress `graphic/marks/encode → export/layered [LAYERED]` makes. The `export/layered#LAYERED` `Layer(name, source, bbox, visible, locked)` row is the settled binding contract: one `Layer` per `GlyphStyle.layer` named `Group`, the `name` the layer label, the `source` the serialized named-`<g>` SVG bytes, the `bbox` the canvas extent — the `SvgLayers`/`PdfOcg` arms fold each row into one `Group(id=name)`/OCG. The return shape `tuple[tuple[Layer, ...], ArtifactReceipt]` mirrors the settled `export/layered#LAYERED` `Layer`-row ingestion; the named-`Group`-per-layer emission projected to one `Layer` row apiece is the settled structure.
- [DIAGRAM_RECEIPT_FACTS] [RESOLVED]: `DiagramDraw._compute` projects `ArtifactReceipt.Diagram(key, "diagram-svg", nodes, edges, "drawsvg")` against the shared `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` case (`diagram: tuple[ContentKey, str, int, int, str]` as content key / kind / node count / edge count / algorithm), the node/edge counts the `Node`/`Edge` glyph tallies and the algorithm the `drawsvg` render engine, so the draw owner contributes the SVG-render facts through the `Diagram` case rather than the mis-shaped 5-positional string-literal `Egress` call the real `egress: tuple[ContentKey, int, int, int, int, int]` case rejected; the `DiagramLayout#LAYOUT` coordinate-assignment owner also contributes the `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm)` facts off the built presentation graph (its `kind` the `DiagramKind` value, its `algorithm` the `LayoutPolicy.tag`), and the named-layer element/layer/byte evidence is the `export/layered#LAYERED` owner's `Preview`/`Egress` contribution off the bound `Layer` rows — so the diagram engine's receipt contributions are the layout+draw `Diagram` facts plus the layered-export `Preview`/`Egress` named-layer evidence on the one shared `core/receipt#RECEIPT` family, no mis-shaped `Egress` call on this owner.
