# [PY_ARTIFACTS_DIAGRAM_GLYPHSET]

The bounded data-driven diagram-primitive vocabulary. `DiagramGlyph` is ONE closed `tagged_union` over the seven marks every data-driven diagram is built from — `Node`, `Edge`, `Swimlane`, `Annotation`, `Marker`, `Area`, and `Fragment` — each case carrying its own typed geometry-topology-and-style payload keyed by the stable node/edge index the `visualization/diagram/layout#LAYOUT` coordinate assignment emits, so a sun-path arc, a circulation connector, an ER entity with field ports, a flowchart decision diamond, a Sankey ribbon, and a site parcel polygon all read as one mark grammar rather than a per-diagram-kind shape family. The seven marks span every architectural AND general/technical data-driven diagram — `SUN_PATH`/`CIRCULATION`/`STACKING`/`PROGRAM`/`SITE` plus `NODE_LINK`/`FLOWCHART`/`ENTITY_RELATION`/`SANKEY`/`SECTION_CALLOUT` — because the closure carries the topology axes those kinds demand: a `Node` owns a `NodeShape` (rectangle/diamond/oval/entity/…), a typed `Port` sub-vocabulary (the side/index connection points ER field-rows and flowchart record-slots route to), and a nesting `parent` (the compound-node axis a zone→program container needs); an `Edge` owns a `weight` magnitude (the Sankey ribbon width) and a `caps` end-terminal pair over the `EndCap` vocabulary (the ER crow's-foot cardinality and the circulation arrowhead as TYPED end marks); an `Area` owns a true polygon ring and its measured `magnitude` (the V15 room/zone/parcel/footprint mark rectangles cannot express); a `Fragment` carries owned vector-path geometry in sheet coordinates (the sun-path horizon, altitude rings, date arcs, and hour lines the `visualization/diagram/solar#SOLAR` furniture generates); and `GlyphStyle` carries `dash`, `corner`, and a `TextRun` label-typography axis. The one class the seven marks structurally cannot express — a named symbol with bound anchor terminals (a resistor, a NAND gate, an op-amp) — is the schematic class the sibling `schemdraw` engine owns, never an eighth glyph. The glyph carries geometry, topology, and identity only; it emits no SVG (`visualization/diagram/draw#DRAW`'s) and computes no coordinates (`visualization/diagram/layout#LAYOUT`'s), so the three diagram owners compose around this one shared vocabulary.

## [01]-[INDEX]

- [01]-[GLYPHSET]: the closed `DiagramGlyph` mark vocabulary (`Node`/`Edge`/`Swimlane`/`Annotation`/`Marker`/`Area`/`Fragment`) over the `DiagramKind` diagram-class axis, each mark carrying its laid-out geometry, its topology axes (`NodeShape`/`Port`/`parent` on `Node`, nesting `parent` on `Swimlane` and `Area`, `weight` and `EndCap` caps on `Edge`, the polygon ring and measured `magnitude` on `Area`, the owned path d-string on `Fragment`), and a `GlyphStyle` palette-indexed identity plus layer-name binding with its `TextRun` label-typography axis — folded by the `visualization/diagram/draw#DRAW` emitter into named SVG groups / `.drawio` objects, positioned and port-routed by the `visualization/diagram/layout#LAYOUT` coordinate assignment, one mark grammar across every architectural and general diagram kind, never a per-kind shape family.

## [02]-[GLYPHSET]

- Owner: `DiagramGlyph` the one diagram-mark vocabulary, a frozen `tagged_union` whose `tag` carries the closed `GlyphTag` literal (`node`/`edge`/`swimlane`/`annotation`/`marker`/`area`/`fragment`) and whose every case carries its own typed geometry-topology-and-style payload — never a parallel per-diagram-kind shape class family and never an erased `dict` of mark attributes; `DiagramKind` the closed `StrEnum` naming WHICH diagram the layout owner builds (the AEC kinds `SUN_PATH`/`CIRCULATION`/`STACKING`/`PROGRAM`/`SITE` plus the general/technical kinds `NODE_LINK`/`FLOWCHART`/`ENTITY_RELATION`/`SANKEY`/`SECTION_CALLOUT`), a selector over the layout/position policy, never a glyph type; `NodeShape` the closed `StrEnum` (the ISO 5807 flowchart + ER/datastore silhouette roster) the draw owner lowers a `Node` to shape-appropriate geometry through (a flowchart decision to a diamond, an ER entity to a titled record, a datastore to a cylinder), never a `Rectangle`-only body; `Port` the frozen `Struct` (`id`/`side: PortSide`/`index`/`width`/`height`/`at`/`label`) carrying the typed connection point the `pyelk` ELK `portConstraints` layout seats on a node BOUNDARY position (an ER field-port, a flowchart record-slot) rather than the node center — a set `at` node-relative coordinate escalates the node to `FIXED_POS` so the port seats exactly where the domain fixes it — drawn as a boundary mark by the emitter; `EndCap` the closed edge-terminal vocabulary the draw owner lowers to a shared `drawsvg.Marker` def per cap and the drawpyo `line_end_source`/`line_end_target` glyph set, carrying the ER crow's-foot cardinality as a TYPED end marker, never a bare-string `cardinality` edge label both arms reparse; `SubLayout` the per-node inner-engine override vocabulary the layout owner reads off the node attribute row and maps onto the ELK per-node `elk.algorithm` option (a compound container laying its own sub-graph out under a different engine than the parent) — consumed pre-emission, so it never rides the positioned mark; `TextRun` the frozen label-typography identity on `GlyphStyle.text` (size/weight/italic/family/ink) the draw owner resolves at the label fold; `GlyphStyle` the one frozen `Struct` carrying each mark's visual identity — the `fill`/`stroke` palette-index into the `graphic/color/derive#DERIVE` `Derivation.coords` array, the `width` stroke, the `layer` named-group binding, the `opacity`, the `dash` stroke-dasharray pattern, the `corner` radius the draw owner threads to `rx`/`rounded`, and the `text` run — so a mark's color traces to one palette index never an ad-hoc hex literal, and the layer name binds the mark into the `visualization/diagram/draw#DRAW` named SVG group the `export/layered#LAYERED` owner reads.
- Cases: `DiagramGlyph` cases — `Node(index, x, y, w, h, label, style, shape, ports, parent)` (a placed program/space/element/entity/decision box keyed by the stable layout node index, carrying its laid-out top-left and extent, an optional label, its `NodeShape`, its typed `Port` tuple, and its optional nesting `parent` node index) · `Edge(source, target, points, label, style, weight, caps)` (a circulation/adjacency/flow/relation connector keyed by the source/target node-or-port indices, carrying the laid-out polyline waypoints — an orthogonal, spline, or straight route is one point sequence never a per-route-shape case — the `weight` ribbon magnitude the Sankey renderer maps to band width, and the `(source, target)` `EndCap` pair the draw owner lowers to typed end markers) · `Swimlane(index, x, y, w, h, title, style, parent)` (a stacking-band/program-zone/compound-container region carrying its extent, title, and optional nesting `parent`) · `Annotation(x, y, text, anchor, style)` (a callout/dimension/north-arrow/formula text mark anchored at a laid-out point through the closed `TextAnchor` start/middle/end vocabulary — never a bare-`str` anchor — plain text or a `$math$`-bearing run the draw owner routes to the math typesetter) · `Marker(x, y, kind, angle, style)` (a vertex/endpoint/compass-tick/port glyph carrying its position, a bounded `MarkerKind` shape, and an orientation angle the sun-path tick and circulation arrowhead share) · `Area(index, ring, label, style, magnitude, parent)` (the positioned TRUE-POLYGON mark — a room, zone, parcel, or building footprint carrying its laid-out closed ring, its measured `magnitude` in plan units the layout sizes and the label states, and its optional nesting `parent`; the V15 mark a rectangle band cannot express) · `Fragment(d, label, anchor, style)` (an owned vector-path geometry mark in sheet coordinates — the sun-path horizon circle, altitude ring, labeled date arc, and analemma hour line the solar furniture generates — carrying the path d-string, an optional label seated at `anchor`, and the style the theme rows select) — matched by one total `match`/`case` over `tag` in the `visualization/diagram/draw#DRAW` emitter.
- Entry: `DiagramGlyph.Node`/`Edge`/`Swimlane`/`Annotation`/`Marker`/`Area`/`Fragment` are the seven mark constructors the `visualization/diagram/layout#LAYOUT` coordinate assignment emits — the layout owner positions the marks (assigning `x`/`y`/`points`/`ring`/`w`/`h` from the graph-layout coordinate map, threading each `Node`'s `Port` set through the `pyelk` `portConstraints` so each port seats on the node boundary, and carrying the nesting `parent` axis for the compound-node growth) and threads the `GlyphStyle` palette index and layer name, returning the ordered `tuple[DiagramGlyph, ...]` the draw owner folds; the trailing `shape`/`ports`/`parent`/`weight`/`caps`/`magnitude` axes default (`RECTANGLE`/`()`/`None`/`0.0`/`(NONE, NONE)`/`0.0`) so a minimal constructor call stays minimal while the topology-bearing kinds pass them, and there is no `DiagramGlyph.of` composer because the layout owner constructs each mark with its already-resolved geometry and topology, never a post-hoc positioning hop.
- Auto: `DiagramKind` selects the layout policy and the emitted mark set, not the glyph type — `SUN_PATH` emits `Fragment` furniture (horizon/rings/compass labels/date arcs/hour lines) plus `Marker` sun-position ticks plus `Edge` arc paths and a `Marker` NORTH compass, `CIRCULATION` emits plan-anchored `Node` space boxes plus `Edge` flow connectors capped with the typed arrowhead, `STACKING` emits `Swimlane` floor bands segmented by area-proportioned `Node` program segments, `PROGRAM` emits area-sized `Node` program boxes plus `Edge` adjacency connectors, `SITE` emits `Area` parcel polygons plus `Area` building footprints and `Annotation` callouts, `NODE_LINK` a force/ELK-laid `Node`/`Edge` graph, `FLOWCHART` `NodeShape`-varied `Node`s (`DIAMOND` decision, `OVAL` terminal, `PARALLELOGRAM` data) plus port-routed `Edge`s, `ENTITY_RELATION` `ENTITY`-shaped `Node`s carrying field `Port`s plus `Edge`s whose `caps` carry the crow's-foot cardinality, `SANKEY` weighted `Edge` ribbons between `Node` stages, `SECTION_CALLOUT` plan-anchored `Swimlane` detail frames plus `Annotation` cross-references — so a diagram kind is one layout-policy arm in the layout owner, the same seven `DiagramGlyph` cases across every kind. `GlyphStyle.fill`/`stroke` are integer indices into the palette array the draw owner resolves through `hex_ramp`-projected `Derivation.coords`, so a recolor is a palette swap never a per-glyph edit; `GlyphStyle.layer` is the named-group key the draw owner buckets marks into so every `Node` on the "spaces" layer and every `Edge` on the "circulation" layer bind into their own `drawsvg` `Group` / `drawpyo` container the `export/layered#LAYERED` OCG/SVG-layer owner reads.
- Packages: the vocabulary itself imports only `expression` (the `tagged_union`) and `msgspec` (the frozen `Struct`s) — it emits no SVG, computes no coordinates, and admits no engine directly. Its topology axes exist to give the admitted diagram packages real fields to key onto: the `Port` axis is what the `visualization/diagram/layout#LAYOUT` `pyelk` ELK arm routes through typed `ports` (`{id, layoutOptions:{port.side, port.index}}`, a set `at` escalating to `FIXED_POS`), and the `parent` axis is the recursive compound-`children` nesting the same ELK document admits, with `SubLayout` the per-container `elk.algorithm` override riding the node attribute row; the `NodeShape` axis is what the `visualization/diagram/draw#DRAW` `drawsvg`/`drawpyo` arms lower to shape geometry; the `EndCap` axis is what the draw arms lower to shared `drawsvg.Marker` defs and the drawpyo `line_end_source`/`line_end_target` vocabulary; the `Fragment` d-string is the `graphic/vector/path#VECTOR` `fragment` form the solar furniture emits; the label text on `Node`/`Swimlane`/`Edge`/`Annotation`/`Area` is the mark the draw owner resolves to font-independent `<path>` outlines under the `TextRun` typography (a `$math$` annotation through the math typesetter). The palette-index contract is `graphic/color/derive#DERIVE` `Derivation.coords`.
- Growth: a new diagram kind is one `DiagramKind` row plus one layout-policy arm in `visualization/diagram/layout#LAYOUT`, never a new glyph type — the seven marks cover every data-driven diagram; a new node shape is one `NodeShape` row folded in the draw owner's `Node` geometry dispatch; a new marker shape is one `MarkerKind` row folded in the draw owner's `Marker` arm; a new end terminal is one `EndCap` row plus one def arm in the draw owner's cap table; a new mark visual axis is one `GlyphStyle` field threaded through the draw arm (`dash`, `corner`, and `text` are exactly this growth); a new topology axis is one field appended to the owning case tuple (the `caps` on `Edge` and the `magnitude` on `Area` are exactly this growth, each defaulted so the existing constructors are untouched); a genuinely-new mark (a hatch region, a gradient band) is one `DiagramGlyph` case plus one draw arm, only when the seven do not cover it (`Area` and `Fragment` entered by exactly this law); zero new surface for a new diagram kind.
- Boundary: no SVG byte emission (`visualization/diagram/draw#DRAW`'s — this vocabulary carries geometry, topology, and identity, the draw owner folds each case to a `drawsvg` element or `drawpyo` object); no coordinate, ring, or port-route computation (`visualization/diagram/layout#LAYOUT`'s — the layout owner assigns `x`/`y`/`points`/`ring` and routes edges to `Port` coordinates from the graph-layout map, constructing each mark already positioned); no furniture generation (the `Fragment` d-string arrives generated from `visualization/diagram/solar#SOLAR`; this vocabulary only carries it); no ad-hoc color (the `GlyphStyle.fill`/`stroke` indices key the `graphic/color/derive#DERIVE` palette, never a hex literal); no text outlining (the draw owner resolves the label mark under its `TextRun`, this vocabulary carries only the string and its typography identity); no graph analysis (that stays at `data/graph#GRAPH` and the layout owner consumes the adjacency); no named-symbol schematic (the `schemdraw` engine owns the anchored-symbol class the seven marks cannot express); a per-diagram-kind shape class family, an erased mark-attribute `dict`, a per-glyph hex color, a `Rectangle`-only `Node`, and a stringly cardinality label are the deleted forms — one closed mark vocabulary, palette-indexed identity, shape/port/cap/weight/area topology, layer-bound for the named egress.

```python signature
from enum import StrEnum
from typing import Literal

from expression import case, tag, tagged_union
from msgspec import Struct

type GlyphTag = Literal["node", "edge", "swimlane", "annotation", "marker", "area", "fragment"]
type Point = tuple[float, float]


class DiagramKind(StrEnum):
    SUN_PATH = "sun_path"
    CIRCULATION = "circulation"
    STACKING = "stacking"
    PROGRAM = "program"
    SITE = "site"
    NODE_LINK = "node_link"
    FLOWCHART = "flowchart"
    ENTITY_RELATION = "entity_relation"
    SANKEY = "sankey"
    SECTION_CALLOUT = "section_callout"


class NodeShape(StrEnum):
    # ISO 5807 flowchart + ER/datastore silhouette roster; the draw owner lowers each to a `drawsvg.Path`
    # geometry arm and a draw.io style token, never a Rectangle-only body. A new shape is one row folded in
    # the draw owner's `Node` geometry dispatch — the marks and layout stay untouched.
    RECTANGLE = "rectangle"  # process / program box
    DIAMOND = "diamond"  # decision
    OVAL = "oval"  # terminal (start/end, stadium)
    PARALLELOGRAM = "parallelogram"  # data / IO
    ENTITY = "entity"  # ER entity — titled record with field-port rows
    HEXAGON = "hexagon"  # preparation
    CYLINDER = "cylinder"  # datastore / direct-access database
    DOCUMENT = "document"  # single document (wavy base)
    MULTI_DOCUMENT = "multi_document"  # stacked documents
    PREDEFINED_PROCESS = "predefined_process"  # subroutine (double side-bars)
    MANUAL_INPUT = "manual_input"  # keyed input (sloped top)
    MANUAL_OPERATION = "manual_operation"  # manual step (inverted trapezoid)
    OFF_PAGE = "off_page"  # off-page connector (home-plate pentagon)
    STORED_DATA = "stored_data"  # stored data (curved left+right)
    DISPLAY = "display"  # display (rounded-left bullet)
    DELAY = "delay"  # delay (D-shape)
    CONNECTOR = "connector"  # on-page connector (small circle)
    CARD = "card"  # punched card (clipped top-left)
    TAPE = "tape"  # punched tape (wavy top + base)


class PortSide(StrEnum):
    NORTH = "north"
    EAST = "east"
    SOUTH = "south"
    WEST = "west"


class MarkerKind(StrEnum):
    DOT = "dot"
    ARROW = "arrow"
    TICK = "tick"
    NORTH = "north"
    CROSS = "cross"


class EndCap(StrEnum):
    # edge-terminal marker vocabulary consumed by `Edge.caps`; the draw owner lowers each to a shared
    # `drawsvg.Marker` def and the drawpyo `line_end_source`/`line_end_target` glyph set; the `ER_*`
    # crow's-feet are the ENTITY_RELATION cardinality carried as a TYPED end marker, never a bare-string
    # `cardinality` edge label both arms reparse.
    NONE = "none"
    ARROW = "arrow"  # filled arrowhead (drawpyo `classic`)
    OPEN = "open"  # open v-arrowhead (drawpyo `open`)
    BLOCK = "block"  # block arrowhead
    DIAMOND = "diamond"  # UML aggregation/composition (the `fill` index picks hollow vs solid)
    CIRCLE = "circle"  # association endpoint (drawpyo `circle`)
    CROSS = "cross"  # measurement / negation tick
    ER_ONE = "er_one"  # crow's-foot: exactly one (single bar) -> drawpyo `ERone`
    ER_MANY = "er_many"  # crow's-foot: many (fork) -> drawpyo `ERmany`
    ER_ZERO_ONE = "er_zero_one"  # crow's-foot: zero-or-one (circle + bar) -> drawpyo `ERzeroToOne`
    ER_ONE_MANY = "er_one_many"  # crow's-foot: one-or-many (bar + fork) -> drawpyo `ERoneToMany`
    ER_ZERO_MANY = "er_zero_many"  # crow's-foot: zero-or-many (circle + fork) -> drawpyo `ERzeroToMany`


class SubLayout(StrEnum):
    # per-container inner-engine override the layout owner reads off the node attribute row and maps onto
    # the ELK per-node `elk.algorithm` option, so a compound (children-bearing) container lays its own
    # sub-graph out under a DIFFERENT engine than the parent; consumed pre-emission, never on the mark.
    INHERIT = "inherit"
    LAYERED = "layered"
    TREE = "tree"
    RADIAL = "radial"
    FORCE = "force"
    STRESS = "stress"
    PACKED = "packed"


class TextAnchor(StrEnum):
    START = "start"  # SVG text-anchor start -> halign "left"
    MIDDLE = "middle"  # -> "center"
    END = "end"  # -> "right"


class TextRun(Struct, frozen=True):
    # label typography identity carried on `GlyphStyle.text`; the draw owner resolves family/size at the
    # label fold, weight/italic as synthetic bold (outline stroke) / oblique (skew) on the SVG arm and as
    # the `fontStyle` axis on the `.drawio` arm. None on `GlyphStyle.text` selects the default face and size.
    size: float = 12.0  # em size in user units
    weight: int = 400  # CSS numeric weight (400 normal .. 700 bold)
    italic: bool = False
    family: str = ""  # "" = the draw owner's default face; else a named face the outline engine loads
    ink: int | None = None  # palette index for the label glyph; None = inherit the mark's stroke index


class Port(Struct, frozen=True):
    id: str  # the ELK port id an edge endpoint may reference
    side: PortSide = PortSide.EAST  # -> elk.port.side under portConstraints
    index: int = 0  # -> elk.port.index (side-local order)
    width: float = 8.0  # -> ELK port {width}; the field-port hit-box the router seats an edge on
    height: float = 8.0  # -> ELK port {height}
    at: Point | None = None  # fixed node-relative port coordinate; set -> elk.portConstraints FIXED_POS
    label: str | None = None  # ELK port label (an ER field name / flowchart record-slot caption)


class GlyphStyle(Struct, frozen=True):
    layer: str
    fill: int = 0
    stroke: int = 0
    width: float = 1.0
    opacity: float = 1.0
    dash: tuple[float, ...] = ()  # stroke-dasharray run; () = solid
    corner: float = 0.0  # corner radius; 0 = sharp (drawsvg Rectangle rx/ry, drawpyo `rounded`)
    text: TextRun | None = None  # label typography; None = the draw owner's default face/size


@tagged_union(frozen=True)
class DiagramGlyph:
    tag: GlyphTag = tag()
    node: tuple[int, float, float, float, float, str | None, GlyphStyle, NodeShape, tuple[Port, ...], int | None] = case()
    edge: tuple[int, int, tuple[Point, ...], str | None, GlyphStyle, float, tuple[EndCap, EndCap]] = case()
    swimlane: tuple[int, float, float, float, float, str | None, GlyphStyle, int | None] = case()
    annotation: tuple[float, float, str, TextAnchor, GlyphStyle] = case()
    marker: tuple[float, float, MarkerKind, float, GlyphStyle] = case()
    area: tuple[int, tuple[Point, ...], str | None, GlyphStyle, float, int | None] = case()
    fragment: tuple[str, str | None, Point, GlyphStyle] = case()

    @staticmethod
    def Node(
        index: int,
        x: float,
        y: float,
        w: float,
        h: float,
        label: str | None,
        style: GlyphStyle,
        shape: NodeShape = NodeShape.RECTANGLE,
        ports: tuple[Port, ...] = (),
        parent: int | None = None,
    ) -> "DiagramGlyph":
        return DiagramGlyph(node=(index, x, y, w, h, label, style, shape, ports, parent))

    @staticmethod
    def Edge(
        source: int,
        target: int,
        points: tuple[Point, ...],
        label: str | None,
        style: GlyphStyle,
        weight: float = 0.0,
        caps: tuple[EndCap, EndCap] = (EndCap.NONE, EndCap.NONE),
    ) -> "DiagramGlyph":
        return DiagramGlyph(edge=(source, target, points, label, style, weight, caps))

    @staticmethod
    def Swimlane(
        index: int, x: float, y: float, w: float, h: float, title: str | None, style: GlyphStyle, parent: int | None = None
    ) -> "DiagramGlyph":
        return DiagramGlyph(swimlane=(index, x, y, w, h, title, style, parent))

    @staticmethod
    def Annotation(x: float, y: float, text: str, anchor: TextAnchor, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(annotation=(x, y, text, anchor, style))

    @staticmethod
    def Marker(x: float, y: float, kind: MarkerKind, angle: float, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(marker=(x, y, kind, angle, style))

    @staticmethod
    def Area(
        index: int, ring: tuple[Point, ...], label: str | None, style: GlyphStyle, magnitude: float = 0.0, parent: int | None = None
    ) -> "DiagramGlyph":
        return DiagramGlyph(area=(index, ring, label, style, magnitude, parent))

    @staticmethod
    def Fragment(d: str, label: str | None, anchor: Point, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(fragment=(d, label, anchor, style))
```

`DiagramGlyph` is the one mark grammar every diagram kind is built from: the `DiagramKind` selector decides which marks the layout owner emits and how it positions them, not a new glyph type, so the seven cases span the AEC sun-path/circulation/stacking/program/site diagrams AND the general node-link, flowchart, ER, Sankey, and section-callout diagrams. The topology axes are what make the marks span the broader scope: a `Node` carries a `NodeShape` so the draw owner lowers a flowchart decision to a diamond and an ER entity to a titled record rather than a rectangle, a typed `Port` tuple so the `pyelk` ELK layout routes an edge to a field-port boundary position rather than the node center (a fixed `at` seat escalating to `FIXED_POS`), and a nesting `parent` so a compound zone→program container lays out as an enclosed sub-graph under its own `SubLayout` engine; an `Edge` carries a `weight` so the Sankey renderer maps it to a ribbon width and a `caps` pair so the circulation arrowhead and the ER crow's-foot cardinality lower as typed end markers; an `Area` carries the true polygon ring and measured magnitude the V15 site/stacking/program area law sizes and labels from; a `Fragment` carries the solar furniture's owned path geometry so the sun-path backdrop rides the same glyph stream as the data marks. Each mark carries a `GlyphStyle` whose `fill`/`stroke` are integer indices into the `graphic/color/derive#DERIVE` palette array, whose `layer` is the named-group key, whose `dash`/`corner` are the stroke and corner axes, and whose `text` is the `TextRun` typography the label fold resolves — so the draw owner buckets every mark into its named `drawsvg` `Group` / `drawpyo` container the `export/layered#LAYERED` owner reads, a recolor is a palette swap, and a diagram's label text outlines to font-independent geometry rather than a font-dependent `<text>` element.
