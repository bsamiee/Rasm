# [PY_ARTIFACTS_DIAGRAM_GLYPHSET]

`DiagramGlyph` is the bounded data-driven diagram-primitive vocabulary: one closed `tagged_union` over the seven marks every data-driven diagram composes — `Node`, `Edge`, `Swimlane`, `Annotation`, `Marker`, `Area`, `Fragment` — each case carrying its own typed geometry-topology-style payload keyed by the stable node/edge index `visualization/diagram/layout#LAYOUT` emits. A sun-path arc, a circulation connector, an ER entity with field ports, a flowchart decision, a Sankey ribbon, and a site parcel polygon read as one mark grammar, never a per-diagram-kind shape family. Marks carry geometry, topology, and identity alone: no SVG byte (`visualization/diagram/draw#DRAW`'s), no coordinate (`visualization/diagram/layout#LAYOUT`'s), so the three diagram owners compose around one shared vocabulary.

Topology axes give the admitted packages typed fields to key onto: a `Node` owns a `NodeShape` silhouette, a `Port` sub-vocabulary (side/index boundary points the `pyelk` ELK `portConstraints` seats, a fixed `at` escalating to `FIXED_POS`), and a nesting `parent`; an `Edge` owns a `weight` (the Sankey ribbon width) and an `EndCap` `caps` pair (crow's-foot cardinality and arrowhead as typed end marks); an `Area` owns a true polygon ring and measured `magnitude`; a `Fragment` carries the owned vector-path geometry the solar furniture generates; `GlyphStyle` carries `fill`/`stroke` indices into the `graphic/color/derive#DERIVE` `Derivation.coords` palette, a `layer` binding the `export/layered#LAYERED` owner reads, and a `TextRun` typography axis. A named symbol with bound anchor terminals — a resistor, a NAND gate, an op-amp — is the one class the seven marks cannot express; `visualization/diagram/schematic#SCHEMATIC`'s `schemdraw` engine owns it, never an eighth glyph.

## [01]-[INDEX]

- [01]-[GLYPHSET]: `DiagramGlyph`'s closed mark vocabulary over the `DiagramKind` class axis — each mark carrying its laid-out geometry, its topology axes, and a palette-indexed `GlyphStyle` with layer binding and `TextRun` typography.

## [02]-[GLYPHSET]

- Owner: `DiagramGlyph` a frozen `tagged_union` whose every case carries its own typed payload, never a parallel per-kind shape class family nor an erased attribute `dict`; `DiagramKind` a selector over the layout owner's position policy, never a glyph type; `NodeShape` the ISO 5807 flowchart + ER/datastore silhouette roster the draw owner lowers each `Node` through, never a `Rectangle`-only body; `Port` a boundary connection point the ELK `portConstraints` seats (a set `at` escalating to `FIXED_POS`), never the node center; `EndCap` carrying ER crow's-foot cardinality as a typed end marker, never a stringly `cardinality` label both arms reparse; `SubLayout` the per-container inner-engine override consumed pre-emission, never riding the mark; `GlyphStyle` carrying palette-index identity so a mark's color traces to one index never an ad-hoc hex, and a `layer` name binding the mark into its named egress group.
- Cases: each mark keys on the stable layout index the coordinates and marks share — `Edge`'s one point sequence is any route shape (orthogonal, spline, or straight, never a per-route case); `Annotation` anchors through the closed `TextAnchor` vocabulary, never a bare-`str` anchor, its plain-or-`$math$` text routed to the typesetter; `Area` is the true-polygon mark a rectangle band cannot express; `Fragment` is the owned vector-path geometry the solar furniture generates; one total `match` over `tag` in the draw emitter folds all seven.
- Entry: the seven constructors are the marks the layout owner emits already-positioned; no `DiagramGlyph.of` composer exists, because each mark is built with its resolved geometry and topology, never a post-hoc positioning hop; the trailing `shape`/`ports`/`parent`/`weight`/`caps`/`magnitude` axes default so a minimal call stays minimal while the topology-bearing kinds pass them.
- Auto: `DiagramKind` selects the layout policy and emitted mark set, not the glyph type, so every kind draws from the same seven cases; `GlyphStyle.fill`/`stroke` resolve through the draw owner's `hex_ramp`-projected palette so a recolor is a palette swap; `GlyphStyle.layer` buckets marks into the named `drawsvg` `Group` / `drawpyo` container the `export/layered#LAYERED` owner reads.
- Packages: `expression` (the `tagged_union`) and `msgspec` (the frozen `Struct`s) alone — this vocabulary emits no SVG, computes no coordinates, admits no engine; its topology axes exist so admitted packages have typed fields to key onto — the `Port`/`parent` axes the ELK document routes and nests, the `NodeShape`/`EndCap` axes the draw arms lower, the `Fragment` d-string the `graphic/vector/path#VECTOR` `fragment` form carries.
- Growth: a new diagram kind is one `DiagramKind` row plus one layout-policy arm, never a new glyph type; a new node shape one `NodeShape` row, a new marker one `MarkerKind` row, a new terminal one `EndCap` row, a new visual axis one `GlyphStyle` field; a new topology axis is one field appended to the owning case tuple, defaulted so existing constructors are untouched (`caps` on `Edge`, `magnitude` on `Area` entered by exactly this law); a genuinely-new mark is one `DiagramGlyph` case plus one draw arm, earned only when the seven do not cover it (`Area` and `Fragment` entered so).
- Boundary: no SVG emission (`visualization/diagram/draw#DRAW`'s), no coordinate/ring/port-route computation (`visualization/diagram/layout#LAYOUT`'s), no furniture generation (`visualization/diagram/solar#SOLAR`'s — this vocabulary carries only the generated `Fragment` d-string), no graph analysis (`data/graph#GRAPH`'s), no named-symbol schematic (`visualization/diagram/schematic#SCHEMATIC`'s `schemdraw` engine owns the anchored-symbol class the seven marks cannot express).

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
    # ISO 5807 flowchart + ER/datastore silhouette roster the draw owner lowers to a `drawsvg.Path` arm and a draw.io style token.
    RECTANGLE = "rectangle"  # process / program box
    DIAMOND = "diamond"  # decision
    OVAL = "oval"  # terminal
    PARALLELOGRAM = "parallelogram"  # data / IO
    ENTITY = "entity"  # ER entity: titled record with field-port rows
    HEXAGON = "hexagon"  # preparation
    CYLINDER = "cylinder"  # datastore
    DOCUMENT = "document"
    MULTI_DOCUMENT = "multi_document"
    PREDEFINED_PROCESS = "predefined_process"  # subroutine
    MANUAL_INPUT = "manual_input"  # keyed input
    MANUAL_OPERATION = "manual_operation"  # manual step
    OFF_PAGE = "off_page"  # off-page connector
    STORED_DATA = "stored_data"
    DISPLAY = "display"
    DELAY = "delay"
    CONNECTOR = "connector"  # on-page connector
    CARD = "card"  # punched card
    TAPE = "tape"  # punched tape


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
    # edge-terminal vocabulary on `Edge.caps`; the draw owner lowers each to a shared `drawsvg.Marker` def and the drawpyo `line_end_*` glyph set.
    NONE = "none"
    ARROW = "arrow"  # filled arrowhead
    OPEN = "open"  # open v-arrowhead
    BLOCK = "block"  # block arrowhead
    DIAMOND = "diamond"  # UML aggregation/composition; the `fill` index picks hollow vs solid
    CIRCLE = "circle"  # association endpoint
    CROSS = "cross"  # measurement / negation tick
    ER_ONE = "er_one"  # crow's-foot: exactly one
    ER_MANY = "er_many"  # crow's-foot: many
    ER_ZERO_ONE = "er_zero_one"  # crow's-foot: zero-or-one
    ER_ONE_MANY = "er_one_many"  # crow's-foot: one-or-many
    ER_ZERO_MANY = "er_zero_many"  # crow's-foot: zero-or-many


class SubLayout(StrEnum):
    # per-container inner-engine override mapped onto the ELK per-node `elk.algorithm`; consumed pre-emission, never on the mark.
    INHERIT = "inherit"
    LAYERED = "layered"
    TREE = "tree"
    RADIAL = "radial"
    FORCE = "force"
    STRESS = "stress"
    PACKED = "packed"


class TextAnchor(StrEnum):  # SVG text-anchor domain
    START = "start"
    MIDDLE = "middle"
    END = "end"


class TextRun(Struct, frozen=True):
    # label typography on `GlyphStyle.text`; None selects the default face and size.
    size: float = 12.0  # em size in user units
    weight: int = 400  # CSS numeric weight (400 normal .. 700 bold)
    italic: bool = False
    family: str = ""  # "" = the draw owner's default face; else a named face the outline engine loads
    ink: int | None = None  # palette index for the label glyph; None = inherit the mark's stroke index


class Port(Struct, frozen=True):
    id: str  # ELK port id an edge endpoint may reference
    side: PortSide = PortSide.EAST
    index: int = 0  # side-local order
    width: float = 8.0  # field-port hit-box the router seats an edge on
    height: float = 8.0
    at: Point | None = None  # fixed node-relative seat; set escalates the node to FIXED_POS
    label: str | None = None  # ER field name / flowchart record-slot caption


class GlyphStyle(Struct, frozen=True):
    layer: str
    fill: int = 0
    stroke: int = 0
    width: float = 1.0
    opacity: float = 1.0
    dash: tuple[float, ...] = ()  # stroke-dasharray run; () = solid
    corner: float = 0.0  # corner radius; 0 = sharp
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
