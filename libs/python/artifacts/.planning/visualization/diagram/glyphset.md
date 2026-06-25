# [PY_ARTIFACTS_DIAGRAM_GLYPHSET]

The bounded AEC diagram-primitive vocabulary. `DiagramGlyph` is ONE closed `tagged_union` over the five marks every data-driven architectural diagram is built from — `Node`, `Edge`, `Swimlane`, `Annotation`, and `Marker` — each case carrying its own typed geometry-and-style payload keyed by the stable node/edge index the `visualization/diagram/layout#LAYOUT` coordinate assignment emits, so a sun-path arc, a circulation connector, a stacking band, a program adjacency edge, and a site callout all read as one mark grammar rather than a per-diagram-kind shape family. The vocabulary is deliberately small and total: a diagram kind (`SUN_PATH`/`CIRCULATION`/`STACKING`/`PROGRAM`/`SITE`) selects WHICH glyphs the layout emits and how it positions them, never a new glyph type. `GlyphStyle` carries the per-mark visual identity — the fill/stroke index into the `graphic/color/derive#DERIVE` palette array, the stroke width, the layer name binding the mark into its named SVG group — as one frozen row, never a per-call style scatter. The glyph carries geometry and identity only; it emits no SVG bytes (that is `visualization/diagram/draw#DRAW`'s) and computes no coordinates (that is `visualization/diagram/layout#LAYOUT`'s), so the three diagram owners compose around this one shared vocabulary.

## [01]-[INDEX]

- [01]-[GLYPHSET]: the closed `DiagramGlyph` mark vocabulary (`Node`/`Edge`/`Swimlane`/`Annotation`/`Marker`) over the `DiagramKind` AEC-diagram axis, each mark carrying its laid-out geometry plus a `GlyphStyle` palette-indexed identity and a layer-name binding, folded by the `visualization/diagram/draw#DRAW` emitter into named SVG groups and positioned by the `visualization/diagram/layout#LAYOUT` coordinate assignment — one mark grammar across every architectural diagram kind, never a per-kind shape family.

## [02]-[GLYPHSET]

- Owner: `DiagramGlyph` the one diagram-mark vocabulary, a frozen `tagged_union` whose `tag` carries the closed `GlyphTag` literal (`node`/`edge`/`swimlane`/`annotation`/`marker`) and whose every case carries its own typed geometry-and-style payload — never a parallel per-diagram-kind shape class family and never an erased `dict` of mark attributes; `DiagramKind` the closed `StrEnum` (`SUN_PATH`/`CIRCULATION`/`STACKING`/`PROGRAM`/`SITE`) naming WHICH AEC diagram the layout owner builds, a selector over the layout/position policy, never a glyph type; `GlyphStyle` the one frozen `Struct` carrying each mark's visual identity — the `fill`/`stroke` palette-index into the `graphic/color/derive#DERIVE` `ColorReceipt.coords` array, the `width` stroke, the `layer` named-group binding, the `opacity` — so a mark's color traces to one palette index never an ad-hoc hex literal, and the layer name binds the mark into the `visualization/diagram/draw#DRAW` named SVG group the `export/layered#LAYERED` owner reads.
- Cases: `DiagramGlyph` cases — `Node(index, x, y, w, h, label, style)` (a placed program/space/element box keyed by the stable layout node index, carrying its laid-out top-left and extent plus an optional label) · `Edge(source, target, points, label, style)` (a circulation/adjacency/flow connector keyed by the source/target node indices, carrying the laid-out polyline waypoints the layout emits so an orthogonal or curved route is one point sequence not a per-route-shape case) · `Swimlane(index, x, y, w, h, title, style)` (a stacking-band/program-zone/site-parcel region the nodes group into, carrying its extent and title) · `Annotation(x, y, text, anchor, style)` (a callout/dimension/north-arrow/sun-position text mark anchored at a laid-out point) · `Marker(x, y, kind, angle, style)` (a vertex/endpoint/compass-tick glyph carrying its position, a bounded `MarkerKind` shape, and an orientation angle the sun-path tick and circulation arrowhead share) — matched by one total `match`/`case` over `tag` in the `visualization/diagram/draw#DRAW` emitter.
- Entry: `DiagramGlyph.Node`/`Edge`/`Swimlane`/`Annotation`/`Marker` are the five mark constructors the `visualization/diagram/layout#LAYOUT` coordinate assignment emits — the layout owner positions the marks (assigning `x`/`y`/`points`/`w`/`h` from the graph-layout coordinate map) and threads the `GlyphStyle` palette index and layer name, returning the ordered `tuple[DiagramGlyph, ...]` the draw owner folds; there is no `DiagramGlyph.of` composer because the layout owner constructs each mark with its already-resolved geometry, never a post-hoc positioning hop.
- Auto: `DiagramKind` selects the layout policy not the glyph type — `SUN_PATH` emits `Marker` sun-position ticks plus `Edge` arc paths and an `Annotation` compass, `CIRCULATION` emits `Node` space boxes plus `Edge` flow connectors with `Marker` arrowheads, `STACKING` emits `Swimlane` floor bands stacking `Node` program boxes, `PROGRAM` emits `Node` program boxes plus `Edge` adjacency connectors, `SITE` emits `Swimlane` parcels plus `Node` building footprints and `Annotation` callouts — so a diagram kind is one layout-policy arm in the layout owner, the same five `DiagramGlyph` cases across every kind. `GlyphStyle.fill`/`stroke` are integer indices into the palette array the draw owner resolves through `hex_ramp`-projected `ColorReceipt.coords`, so a recolor is a palette swap never a per-glyph edit; `GlyphStyle.layer` is the named-group key the draw owner buckets marks into so every `Node` on the "spaces" layer and every `Edge` on the "circulation" layer bind into their own `drawsvg` `Group` the `export/layered#LAYERED` OCG/SVG-layer owner reads.
- Packages: no external package — this is the pure vocabulary the `rustworkx`/`grandalf` layout (`visualization/diagram/layout#LAYOUT`) and the `drawsvg` emitter (`visualization/diagram/draw#DRAW`) both compose; `graphic/color/derive#DERIVE` (`ColorReceipt.coords` the palette array the `GlyphStyle.fill`/`stroke` indices key into).
- Growth: a new AEC diagram kind is one `DiagramKind` row plus one layout-policy arm in `visualization/diagram/layout#LAYOUT`, never a new glyph type — the five marks cover every architectural diagram; a new marker shape is one `MarkerKind` row folded in the draw owner's `Marker` arm; a new mark visual axis (a dash pattern, a corner radius) is one `GlyphStyle` field threaded through the draw arm; a genuinely-new mark (a hatch region, a gradient band) is one `DiagramGlyph` case plus one draw arm, only when the five do not cover it; zero new surface for a new diagram kind.
- Boundary: no SVG byte emission (that is `visualization/diagram/draw#DRAW`'s — this vocabulary carries geometry and identity, the draw owner folds each case to a `drawsvg` element); no coordinate computation (that is `visualization/diagram/layout#LAYOUT`'s — the layout owner assigns `x`/`y`/`points` from the graph-layout map and constructs each mark already positioned); no ad-hoc color (the `GlyphStyle.fill`/`stroke` indices key the `graphic/color/derive#DERIVE` palette, never a hex literal); no graph analysis (that stays at `data/graph#GRAPH` and the layout owner consumes the adjacency); a per-diagram-kind shape class family, an erased mark-attribute `dict`, and a per-glyph hex color are the deleted forms — one closed mark vocabulary, palette-indexed identity, layer-bound for the named-SVG egress.

```python signature
from enum import StrEnum
from typing import Literal

from expression import case, tag, tagged_union
from msgspec import Struct

type GlyphTag = Literal["node", "edge", "swimlane", "annotation", "marker"]
type Point = tuple[float, float]


class DiagramKind(StrEnum):
    SUN_PATH = "sun_path"
    CIRCULATION = "circulation"
    STACKING = "stacking"
    PROGRAM = "program"
    SITE = "site"


class MarkerKind(StrEnum):
    DOT = "dot"
    ARROW = "arrow"
    TICK = "tick"
    NORTH = "north"
    CROSS = "cross"


class GlyphStyle(Struct, frozen=True):
    layer: str
    fill: int = 0
    stroke: int = 0
    width: float = 1.0
    opacity: float = 1.0


@tagged_union(frozen=True)
class DiagramGlyph:
    tag: GlyphTag = tag()
    node: tuple[int, float, float, float, float, str | None, GlyphStyle] = case()
    edge: tuple[int, int, tuple[Point, ...], str | None, GlyphStyle] = case()
    swimlane: tuple[int, float, float, float, float, str | None, GlyphStyle] = case()
    annotation: tuple[float, float, str, str, GlyphStyle] = case()
    marker: tuple[float, float, MarkerKind, float, GlyphStyle] = case()

    @staticmethod
    def Node(index: int, x: float, y: float, w: float, h: float, label: str | None, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(node=(index, x, y, w, h, label, style))

    @staticmethod
    def Edge(source: int, target: int, points: tuple[Point, ...], label: str | None, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(edge=(source, target, points, label, style))

    @staticmethod
    def Swimlane(index: int, x: float, y: float, w: float, h: float, title: str | None, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(swimlane=(index, x, y, w, h, title, style))

    @staticmethod
    def Annotation(x: float, y: float, text: str, anchor: str, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(annotation=(x, y, text, anchor, style))

    @staticmethod
    def Marker(x: float, y: float, kind: MarkerKind, angle: float, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(marker=(x, y, kind, angle, style))
```

`DiagramGlyph` is the one mark grammar every AEC diagram kind is built from: the `DiagramKind` selector decides which marks the layout owner emits and how it positions them, not a new glyph type, so the five cases (`Node`/`Edge`/`Swimlane`/`Annotation`/`Marker`) span the sun-path, circulation, stacking, program, and site diagrams. Each mark carries its laid-out geometry plus a `GlyphStyle` whose `fill`/`stroke` are integer indices into the `graphic/color/derive#DERIVE` palette array and whose `layer` is the named-group key — so the draw owner buckets every mark into its named `drawsvg` `Group` the `export/layered#LAYERED` OCG/SVG-layer owner reads, and a recolor is a palette swap rather than a per-glyph edit.

## [03]-[RESEARCH]

No open items. `DiagramGlyph`/`DiagramKind`/`GlyphStyle`/`MarkerKind` are the pure local vocabulary the diagram engine's layout and draw owners compose — no external package member is named, so there is nothing to verify against a folder `.api` catalogue. The palette-index contract is the `graphic/color/derive#DERIVE` `ColorReceipt.coords` array (settled there); the named-layer binding contract is the `visualization/diagram/draw#DRAW` `drawsvg` `Group` per `GlyphStyle.layer`, each group projected by the draw owner to one `export/layered#LAYERED` `Layer(name, source, bbox)` row (settled in the draw owner's `drawsvg` catalogue). The five-mark closure is the design claim: a new AEC diagram kind is a layout-policy arm, never a sixth glyph, until a diagram is found that needs a mark none of the five express.
