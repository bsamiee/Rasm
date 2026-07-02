# [PY_ARTIFACTS_DIAGRAM_GLYPHSET]

The bounded data-driven diagram-primitive vocabulary. `DiagramGlyph` is ONE closed `tagged_union` over the five marks every data-driven diagram is built from — `Node`, `Edge`, `Swimlane`, `Annotation`, and `Marker` — each case carrying its own typed geometry-topology-and-style payload keyed by the stable node/edge index the `visualization/diagram/layout#LAYOUT` coordinate assignment emits, so a sun-path arc, a circulation connector, an ER entity with field ports, a flowchart decision diamond, and a Sankey ribbon all read as one mark grammar rather than a per-diagram-kind shape family. The five marks span every architectural AND general/technical data-driven diagram — `SUN_PATH`/`CIRCULATION`/`STACKING`/`PROGRAM`/`SITE` plus `NODE_LINK`/`FLOWCHART`/`ENTITY_RELATION`/`SANKEY`/`SECTION_CALLOUT` — because the closure carries the topology axes those kinds demand: a `Node` owns a `NodeShape` (rectangle/diamond/oval/entity/…), a typed `Port` sub-vocabulary (the side/index connection points ER field-rows and flowchart record-slots route to), and a nesting `parent` (the compound-node axis a zone→program container needs); an `Edge` owns a `weight` magnitude (the Sankey ribbon width); and `GlyphStyle` carries a `dash` stroke axis. The one class the five marks structurally cannot express — a named symbol with bound anchor terminals (a resistor, a NAND gate, an op-amp) — is the schematic class the sibling `schemdraw` engine owns, never a sixth glyph. The glyph carries geometry, topology, and identity only; it emits no SVG (`visualization/diagram/draw#DRAW`'s) and computes no coordinates (`visualization/diagram/layout#LAYOUT`'s), so the three diagram owners compose around this one shared vocabulary.

## [01]-[INDEX]

- [01]-[GLYPHSET]: the closed `DiagramGlyph` mark vocabulary (`Node`/`Edge`/`Swimlane`/`Annotation`/`Marker`) over the `DiagramKind` diagram-class axis, each mark carrying its laid-out geometry, its topology axes (`NodeShape`/`Port`/`parent` on `Node`, nesting `parent` on `Swimlane`, `weight` on `Edge`), and a `GlyphStyle` palette-indexed identity plus layer-name binding — folded by the `visualization/diagram/draw#DRAW` emitter into named SVG groups / `.drawio` objects, positioned and port-routed by the `visualization/diagram/layout#LAYOUT` coordinate assignment, one mark grammar across every architectural and general diagram kind, never a per-kind shape family.

## [02]-[GLYPHSET]

- Owner: `DiagramGlyph` the one diagram-mark vocabulary, a frozen `tagged_union` whose `tag` carries the closed `GlyphTag` literal (`node`/`edge`/`swimlane`/`annotation`/`marker`) and whose every case carries its own typed geometry-topology-and-style payload — never a parallel per-diagram-kind shape class family and never an erased `dict` of mark attributes; `DiagramKind` the closed `StrEnum` naming WHICH diagram the layout owner builds (the AEC kinds `SUN_PATH`/`CIRCULATION`/`STACKING`/`PROGRAM`/`SITE` plus the general/technical kinds `NODE_LINK`/`FLOWCHART`/`ENTITY_RELATION`/`SANKEY`/`SECTION_CALLOUT`), a selector over the layout/position policy, never a glyph type; `NodeShape` the closed `StrEnum` (`RECTANGLE`/`DIAMOND`/`OVAL`/`PARALLELOGRAM`/`ENTITY`/`HEXAGON`/`CYLINDER`) the draw owner lowers a `Node` to shape-appropriate geometry through (a flowchart decision to a diamond, an ER entity to a titled record, a datastore to a cylinder), never a `Rectangle`-only body; `Port` the frozen `Struct` (`id`/`side: PortSide`/`index`) carrying the typed connection point the `pyelk` ELK `portConstraints` layout seats on a node BOUNDARY position (an ER field-port, a flowchart record-slot) rather than the node center, drawn as a boundary mark by the emitter; `GlyphStyle` the one frozen `Struct` carrying each mark's visual identity — the `fill`/`stroke` palette-index into the `graphic/color/derive#DERIVE` `ColorReceipt.coords` array, the `width` stroke, the `layer` named-group binding, the `opacity`, and the `dash` stroke-dasharray pattern — so a mark's color traces to one palette index never an ad-hoc hex literal, and the layer name binds the mark into the `visualization/diagram/draw#DRAW` named SVG group the `export/layered#LAYERED` owner reads.
- Cases: `DiagramGlyph` cases — `Node(index, x, y, w, h, label, style, shape, ports, parent)` (a placed program/space/element/entity/decision box keyed by the stable layout node index, carrying its laid-out top-left and extent, an optional label, its `NodeShape`, its typed `Port` tuple, and its optional nesting `parent` node index) · `Edge(source, target, points, label, style, weight)` (a circulation/adjacency/flow/relation connector keyed by the source/target node-or-port indices, carrying the laid-out polyline waypoints — an orthogonal, spline, or straight route is one point sequence never a per-route-shape case — and the `weight` ribbon magnitude the Sankey renderer maps to band width) · `Swimlane(index, x, y, w, h, title, style, parent)` (a stacking-band/program-zone/site-parcel/compound-container region carrying its extent, title, and optional nesting `parent`) · `Annotation(x, y, text, anchor, style)` (a callout/dimension/north-arrow/formula text mark anchored at a laid-out point through the closed `TextAnchor` start/middle/end vocabulary — never a bare-`str` anchor — plain text or a `$math$`-bearing run the draw owner routes to the math typesetter) · `Marker(x, y, kind, angle, style)` (a vertex/endpoint/compass-tick/port glyph carrying its position, a bounded `MarkerKind` shape, and an orientation angle the sun-path tick and circulation arrowhead share) — matched by one total `match`/`case` over `tag` in the `visualization/diagram/draw#DRAW` emitter.
- Entry: `DiagramGlyph.Node`/`Edge`/`Swimlane`/`Annotation`/`Marker` are the five mark constructors the `visualization/diagram/layout#LAYOUT` coordinate assignment emits — the layout owner positions the marks (assigning `x`/`y`/`points`/`w`/`h` from the graph-layout coordinate map, threading each `Node`'s `Port` set through the `pyelk` `portConstraints` so each port seats on the node boundary, and carrying the nesting `parent` axis for the compound-node growth) and threads the `GlyphStyle` palette index and layer name, returning the ordered `tuple[DiagramGlyph, ...]` the draw owner folds; the trailing `shape`/`ports`/`parent`/`weight` axes default (`RECTANGLE`/`()`/`None`/`0.0`) so the five AEC kinds construct unchanged while the ER/flowchart/Sankey kinds pass them, and there is no `DiagramGlyph.of` composer because the layout owner constructs each mark with its already-resolved geometry and topology, never a post-hoc positioning hop.
- Auto: `DiagramKind` selects the layout policy and the emitted mark set, not the glyph type — `SUN_PATH` emits `Marker` sun-position ticks plus `Edge` arc paths and a `Marker` NORTH compass, `CIRCULATION` emits `Node` space boxes plus `Edge` flow connectors the draw owner tips with the shared arrowhead `Marker` def, `STACKING` emits `Swimlane` floor bands stacking `Node` program boxes, `PROGRAM` emits `Node` program boxes plus `Edge` adjacency connectors, `SITE` emits `Swimlane` parcels plus `Node` building footprints and `Annotation` callouts, `NODE_LINK` a force/ELK-laid `Node`/`Edge` graph, `FLOWCHART` `NodeShape`-varied `Node`s (`DIAMOND` decision, `OVAL` terminal, `PARALLELOGRAM` data) plus port-routed `Edge`s, `ENTITY_RELATION` `ENTITY`-shaped `Node`s carrying field `Port`s plus cardinality-`Marker` `Edge`s, `SANKEY` weighted `Edge` ribbons between `Node` stages, `SECTION_CALLOUT` a `Swimlane` detail frame plus `Annotation` cross-reference — so a diagram kind is one layout-policy arm in the layout owner, the same five `DiagramGlyph` cases across every kind. `GlyphStyle.fill`/`stroke` are integer indices into the palette array the draw owner resolves through `hex_ramp`-projected `ColorReceipt.coords`, so a recolor is a palette swap never a per-glyph edit; `GlyphStyle.layer` is the named-group key the draw owner buckets marks into so every `Node` on the "spaces" layer and every `Edge` on the "circulation" layer bind into their own `drawsvg` `Group` / `drawpyo` container the `export/layered#LAYERED` OCG/SVG-layer owner reads.
- Packages: the vocabulary itself imports only `expression` (the `tagged_union`) and `msgspec` (the frozen `Struct`s) — it emits no SVG, computes no coordinates, and admits no engine directly. Its topology axes exist to give the admitted diagram packages real fields to key onto: the `Port` axis is what the `visualization/diagram/layout#LAYOUT` `pyelk` ELK arm routes through typed `ports` (`{id, layoutOptions:{port.side, port.index}}`), and the `parent` axis is the recursive compound-`children` nesting the same ELK document admits as the layout owner's growth extension point (a producer emitting `parent`-bearing nodes, the `_elk_document` nesting them, and the draw owner framing the enclosing containers); the `NodeShape` axis is what the `visualization/diagram/draw#DRAW` `drawsvg`/`drawpyo` arms lower to shape geometry (`drawsvg.Path` diamonds, `drawpyo.object_from_library('flowchart', 'decision')`); the label text on `Node`/`Swimlane`/`Edge`/`Annotation` is the mark the draw owner resolves through `ziafont` `SimpleGlyph.svgpath`/`Text.drawon` to font-independent `<path>` outlines (a `$math$` annotation through `ziamath.Latex`). The palette-index contract is `graphic/color/derive#DERIVE` `ColorReceipt.coords`.
- Growth: a new diagram kind is one `DiagramKind` row plus one layout-policy arm in `visualization/diagram/layout#LAYOUT`, never a new glyph type — the five marks cover every data-driven diagram; a new node shape is one `NodeShape` row folded in the draw owner's `Node` geometry dispatch; a new marker shape is one `MarkerKind` row folded in the draw owner's `Marker` arm; a new mark visual axis is one `GlyphStyle` field threaded through the draw arm (the `dash` field is exactly this growth); a new topology axis is one field appended to the owning case tuple (the `NodeShape`/`Port`/`parent` on `Node`, `parent` on `Swimlane`, `weight` on `Edge` are exactly this growth, each defaulted so the existing AEC constructors are untouched); a genuinely-new mark (a hatch region, a gradient band) is one `DiagramGlyph` case plus one draw arm, only when the five do not cover it; zero new surface for a new diagram kind.
- Boundary: no SVG byte emission (`visualization/diagram/draw#DRAW`'s — this vocabulary carries geometry, topology, and identity, the draw owner folds each case to a `drawsvg` element or `drawpyo` object); no coordinate or port-route computation (`visualization/diagram/layout#LAYOUT`'s — the layout owner assigns `x`/`y`/`points` and routes edges to `Port` coordinates from the graph-layout map, constructing each mark already positioned); no ad-hoc color (the `GlyphStyle.fill`/`stroke` indices key the `graphic/color/derive#DERIVE` palette, never a hex literal); no text outlining (the draw owner resolves the label mark through `ziafont`/`ziamath`, this vocabulary carries only the string); no graph analysis (that stays at `data/graph#GRAPH` and the layout owner consumes the adjacency); no named-symbol schematic (the `schemdraw` engine owns the anchored-symbol class the five marks cannot express); a per-diagram-kind shape class family, an erased mark-attribute `dict`, a per-glyph hex color, and a `Rectangle`-only `Node` are the deleted forms — one closed mark vocabulary, palette-indexed identity, shape/port/weight topology, layer-bound for the named egress.

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
    NODE_LINK = "node_link"
    FLOWCHART = "flowchart"
    ENTITY_RELATION = "entity_relation"
    SANKEY = "sankey"
    SECTION_CALLOUT = "section_callout"


class NodeShape(StrEnum):
    # ISO 5807 flowchart + ER/datastore silhouette roster; the draw owner lowers each to a `drawsvg.Path`
    # or a `drawpyo.object_from_library('flowchart', ...)` key, never a Rectangle-only body. A new shape is
    # one row folded in the draw owner's `Node` geometry dispatch — the marks and layout stay untouched.
    RECTANGLE = "rectangle"                    # process / program box
    DIAMOND = "diamond"                        # decision
    OVAL = "oval"                              # terminal (start/end, stadium)
    PARALLELOGRAM = "parallelogram"            # data / IO
    ENTITY = "entity"                          # ER entity — titled record with field-port rows
    HEXAGON = "hexagon"                        # preparation
    CYLINDER = "cylinder"                      # datastore / direct-access database
    DOCUMENT = "document"                      # single document (wavy base)
    MULTI_DOCUMENT = "multi_document"          # stacked documents
    PREDEFINED_PROCESS = "predefined_process"  # subroutine (double side-bars)
    MANUAL_INPUT = "manual_input"              # keyed input (sloped top)
    MANUAL_OPERATION = "manual_operation"      # manual step (inverted trapezoid)
    OFF_PAGE = "off_page"                      # off-page connector (home-plate pentagon)
    STORED_DATA = "stored_data"                # stored data (curved left+right)
    DISPLAY = "display"                        # display (rounded-left bullet)
    DELAY = "delay"                            # delay (D-shape)
    CONNECTOR = "connector"                    # on-page connector (small circle)
    CARD = "card"                              # punched card (clipped top-left)
    TAPE = "tape"                              # punched tape (wavy top + base)


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
    # edge-terminal marker vocabulary the draw owner lowers to a `drawsvg.Marker` def and the drawpyo
    # `Edge.line_end_source`/`line_end_target` glyph set; the `ER_*` crow's-feet are the ENTITY_RELATION
    # cardinality carried as a TYPED end marker, never a bare-string `cardinality` edge label both arms reparse.
    NONE = "none"
    ARROW = "arrow"                # filled arrowhead (drawpyo `classic`)
    OPEN = "open"                  # open v-arrowhead (drawpyo `open`)
    BLOCK = "block"                # block arrowhead
    DIAMOND = "diamond"            # UML aggregation/composition (the `fill` index picks hollow vs solid)
    CIRCLE = "circle"             # association endpoint (drawpyo `oval`)
    CROSS = "cross"               # measurement / negation tick
    ER_ONE = "er_one"              # crow's-foot: exactly one (single bar) -> drawpyo `ERone`
    ER_MANY = "er_many"            # crow's-foot: many (fork) -> drawpyo `ERmany`
    ER_ZERO_ONE = "er_zero_one"    # crow's-foot: zero-or-one (circle + bar) -> drawpyo `ERzeroToOne`
    ER_ONE_MANY = "er_one_many"    # crow's-foot: one-or-many (bar + fork) -> drawpyo `ERoneToMany`
    ER_ZERO_MANY = "er_zero_many"  # crow's-foot: zero-or-many (circle + fork) -> drawpyo `ERzeroToMany`


class SubLayout(StrEnum):
    # per-node inner-layout override for a compound (children-bearing) `Node`/`Swimlane`; the layout owner
    # maps each member to the ELK per-node `elk.algorithm` option so a container lays its own sub-graph out
    # under a DIFFERENT engine than the parent (recursive per-node-algorithm compound layout). INHERIT = none.
    INHERIT = "inherit"
    LAYERED = "layered"
    TREE = "tree"
    RADIAL = "radial"
    FORCE = "force"
    STRESS = "stress"
    PACKED = "packed"


class TextAnchor(StrEnum):
    START = "start"    # SVG text-anchor start -> ziafont/ziamath halign "left"
    MIDDLE = "middle"  # -> "center"
    END = "end"        # -> "right"


class TextRun(Struct, frozen=True):
    # label typography identity carried on `GlyphStyle`; the draw owner resolves size/weight/slant/family
    # through `ziafont.Text`/`ziamath.Latex`, so a label carries its own type rather than inheriting a
    # hardcoded draw-side size. None on `GlyphStyle.text` selects the draw owner's default face and size.
    size: float = 12.0             # em size in user units
    weight: int = 400              # CSS numeric weight (400 normal .. 700 bold)
    italic: bool = False
    family: str = ""               # "" = the draw owner's default face; else a named family
    ink: int | None = None         # palette index for the label glyph; None = inherit the mark's stroke index


class Port(Struct, frozen=True):
    id: str                          # the ELK port id an edge endpoint may reference
    side: PortSide = PortSide.EAST   # -> elk.port.side under portConstraints
    index: int = 0                   # -> elk.port.index (side-local order)
    width: float = 8.0               # -> ELK port {width}; the field-port hit-box the router seats an edge on
    height: float = 8.0              # -> ELK port {height}
    at: Point | None = None          # fixed node-relative port coordinate; set -> elk.portConstraints FIXED_POS
    label: str | None = None         # ELK port label (an ER field name / flowchart record-slot caption)


class GlyphStyle(Struct, frozen=True):
    layer: str
    fill: int = 0
    stroke: int = 0
    width: float = 1.0
    opacity: float = 1.0
    dash: tuple[float, ...] = ()     # stroke-dasharray run; () = solid
    corner: float = 0.0              # corner radius; 0 = sharp (drawsvg Rectangle rx/ry, drawpyo `rounded`)
    text: TextRun | None = None      # label typography; None = the draw owner's default face/size


@tagged_union(frozen=True)
class DiagramGlyph:
    tag: GlyphTag = tag()
    node: tuple[int, float, float, float, float, str | None, GlyphStyle, NodeShape, tuple[Port, ...], int | None] = case()
    edge: tuple[int, int, tuple[Point, ...], str | None, GlyphStyle, float] = case()
    swimlane: tuple[int, float, float, float, float, str | None, GlyphStyle, int | None] = case()
    annotation: tuple[float, float, str, TextAnchor, GlyphStyle] = case()
    marker: tuple[float, float, MarkerKind, float, GlyphStyle] = case()

    @staticmethod
    def Node(
        index: int, x: float, y: float, w: float, h: float, label: str | None, style: GlyphStyle,
        shape: NodeShape = NodeShape.RECTANGLE, ports: tuple[Port, ...] = (), parent: int | None = None,
    ) -> "DiagramGlyph":
        return DiagramGlyph(node=(index, x, y, w, h, label, style, shape, ports, parent))

    @staticmethod
    def Edge(source: int, target: int, points: tuple[Point, ...], label: str | None, style: GlyphStyle, weight: float = 0.0) -> "DiagramGlyph":
        return DiagramGlyph(edge=(source, target, points, label, style, weight))

    @staticmethod
    def Swimlane(index: int, x: float, y: float, w: float, h: float, title: str | None, style: GlyphStyle, parent: int | None = None) -> "DiagramGlyph":
        return DiagramGlyph(swimlane=(index, x, y, w, h, title, style, parent))

    @staticmethod
    def Annotation(x: float, y: float, text: str, anchor: TextAnchor, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(annotation=(x, y, text, anchor, style))

    @staticmethod
    def Marker(x: float, y: float, kind: MarkerKind, angle: float, style: GlyphStyle) -> "DiagramGlyph":
        return DiagramGlyph(marker=(x, y, kind, angle, style))
```

`DiagramGlyph` is the one mark grammar every diagram kind is built from: the `DiagramKind` selector decides which marks the layout owner emits and how it positions them, not a new glyph type, so the five cases span the AEC sun-path/circulation/stacking/program/site diagrams AND the general node-link, flowchart, ER, Sankey, and section-callout diagrams. The topology axes are what make the five marks span the broader scope: a `Node` carries a `NodeShape` so the draw owner lowers a flowchart decision to a diamond and an ER entity to a titled record rather than a rectangle, a typed `Port` tuple so the `pyelk` ELK layout routes an edge to a field-port boundary position rather than the node center, and a nesting `parent` so a compound zone→program container lays out as an enclosed sub-graph; an `Edge` carries a `weight` so the Sankey renderer maps it to a ribbon width. Each mark carries a `GlyphStyle` whose `fill`/`stroke` are integer indices into the `graphic/color/derive#DERIVE` palette array, whose `layer` is the named-group key, and whose `dash` is the stroke pattern — so the draw owner buckets every mark into its named `drawsvg` `Group` / `drawpyo` container the `export/layered#LAYERED` owner reads, a recolor is a palette swap, and a diagram's label text outlines through `ziafont` (a formula through `ziamath`) to font-independent geometry rather than a font-dependent `<text>` element.

## [03]-[RESEARCH]

- [FIVE_MARK_CLOSURE] [RESOLVED]: the five-mark closure is HONEST for every data-driven diagram once the topology axes exist — the AEC kinds and the general node-link/flowchart/ER/Sankey/section-callout kinds all lower to `Node`/`Edge`/`Swimlane`/`Annotation`/`Marker` because `NodeShape` carries the flowchart/ER box variety, `Port`/`parent` carry the ER field-port and compound-nesting topology, and `Edge.weight` carries the Sankey magnitude. The one class the five marks structurally cannot express is a NAMED SYMBOL with bound anchor terminals (a resistor, a NAND gate, an ADC) — that schematic class is the sibling `schemdraw` engine's, a new page beside the `drawsvg`/`drawpyo` general-diagram arms, never a sixth glyph on this vocabulary. A new AEC or general diagram kind is a layout-policy arm and a `DiagramKind` row, never a new mark.
- [ADMISSION_SEAMS] [RESOLVED]: the prior "no open items / no external admission member is named" claim was wrong — the vocabulary's marks ARE real admissions its consumers key onto. The `Node`/`Swimlane`/`Edge`/`Annotation` label text is the mark the `visualization/diagram/draw#DRAW` owner resolves through `ziafont` `SimpleGlyph.svgpath`/`Text.drawon` (`.api/ziafont.md`) to font-independent `<path>` outlines, a `$math$`-bearing `Annotation` through `ziamath.Latex`/`Text` (`.api/ziamath.md`); the `Port` topology axis is what the layout owner's `pyelk` ELK arm (`.api/pyelk.md`) seats through typed `ports` (`port.side`/`port.index` under `elk.portConstraints`), and the `parent` axis is the recursive compound-`children` nesting growth the ELK document admits; the `NodeShape` axis is what the draw owner's `drawsvg`/`drawpyo` (`.api/drawpyo.md`) arms lower to shape geometry and the `drawpyo` TOML-validated `waypoints`/`connection`/`pattern` style axes the `GlyphStyle` projects onto. The vocabulary stays pure (geometry + topology + palette indices, no package import); the admitted packages consume it in the layout and draw owners, so the closed `DiagramGlyph`/`DiagramKind`/`NodeShape`/`Port`/`GlyphStyle`/`MarkerKind` grammar IS the shared substrate both diagram producers contribute the one `ArtifactReceipt.Diagram` case through — it mints no receipt, computes no coordinates, emits no bytes, and holds no `core/plan#PLAN` case of its own.
</content>
</invoke>
