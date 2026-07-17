# [PY_ARTIFACTS_DIAGRAM_GLYPHSET]

`DiagramGlyph` is the bounded data-driven diagram-primitive vocabulary: one closed `tagged_union` over the seven marks every data-driven diagram composes — `node`, `edge`, `swimlane`, `annotation`, `marker`, `area`, `fragment` — each case carrying one named frozen `Struct` payload keyed by the stable node/edge index `visualization/diagram/layout#LAYOUT` emits. A sun-path arc, a circulation connector, an ER entity with field ports, a flowchart decision, a Sankey ribbon, and a site parcel polygon read as one mark grammar, never a per-diagram-kind shape family. Geometry authority is single: `visualization/diagram/layout#LAYOUT` computes coordinates, routes, rings, and anchors; each mark carries that resolved geometry as immutable data, and `visualization/diagram/draw#DRAW` reads it — no SVG byte lives here and no mark is re-positioned downstream.

Topology axes give the admitted packages typed fields to key onto: a `NodeMark` owns a `NodeShape` silhouette, a `Port` sub-vocabulary (side/index boundary points the `pyelk` ELK `portConstraints` seats, a fixed `at` escalating to `FIXED_POS`), and a nesting `parent`; an `EdgeMark` owns a `weight` (the Sankey ribbon width), an `EndCap` `caps` pair (crow's-foot cardinality and arrowhead as typed end marks), and `source_port`/`target_port` referencing the `Port.id` an ELK edge endpoint seats on — an ER field-row relationship is unspellable without them; an `AreaMark` owns a true polygon ring and measured `magnitude`; a `FragmentMark` carries the owned vector-path geometry the solar furniture generates; `GlyphStyle` carries `fill`/`stroke` indices into the `graphic/color/derive#DERIVE` `Derivation.coords` palette, the `LineCap`/`LineJoin` stroke-end vocabulary ISO 128 linework and dashed centerlines render through, a `layer` binding the `export/layered#LAYERED` owner reads, and a `TextRun` typography axis. A named symbol with bound anchor terminals — a resistor, a NAND gate, an op-amp — is the one class the seven marks cannot express; `visualization/diagram/schematic#SCHEMATIC`'s `schemdraw` engine owns it, never an eighth glyph.

## [01]-[INDEX]

- [01]-[GLYPHSET]: `DiagramGlyph`'s closed mark vocabulary over the `DiagramKind` class axis — each mark a named frozen payload carrying its laid-out geometry, its topology axes, and a palette-indexed `GlyphStyle` with layer binding and `TextRun` typography.

## [02]-[GLYPHSET]

- Owner: `DiagramGlyph` a frozen `tagged_union` whose every case carries one named frozen `msgspec.Struct` payload — field access is by name, a stale consumer breaks loudly on a renamed field, and no downstream `match` depends on tuple ordinals; `DiagramKind` a selector over the layout owner's position policy, never a glyph type; `NodeShape` the ISO 5807 flowchart + ER/datastore silhouette roster the draw owner lowers each `NodeMark` through, never a `Rectangle`-only body; `Port` a boundary connection point the ELK `portConstraints` seats (a set `at` escalating to `FIXED_POS`), never the node center; `EndCap` carrying arrowhead, UML aggregation/composition, association, measurement, and ER cardinality semantics as typed end markers, never a stringly label or palette-dependent terminal; `SubLayout` the per-container inner-engine override consumed pre-emission, never riding the mark; `GlyphStyle` carrying palette-index identity so a mark's color traces to one index never an ad-hoc hex, `LineCap`/`LineJoin` stroke-end policy, and a `layer` name binding the mark into its named egress group.
- Cases: each mark keys on the stable layout index the coordinates and marks share — `EdgeMark.points` is any route shape under the typed `EdgeRoute` regime (never a per-route case or a stringly token), its `source_port`/`target_port` naming the seated `Port.id` when the endpoint binds a field row rather than the node boundary; `AnnotationMark` anchors through the closed `TextAnchor` vocabulary with an `angle` for rotated section-callout and axis text, its plain-or-`$math$` text routed to the typesetter; `AreaMark` is the true-polygon mark a rectangle band cannot express; `FragmentMark` is the owned vector-path geometry the solar furniture generates; one total `match` over `tag` in the draw emitter folds all seven.
- Entry: construction is the case payload itself — `DiagramGlyph(node=NodeMark(...))` — built by the layout owner with resolved geometry and topology, never a post-hoc positioning hop and never a forwarding constructor beside the named Struct; trailing topology axes (`shape`/`ports`/`parent`/`weight`/`caps`/`source_port`/`target_port`/`magnitude`/`angle`) default so a minimal payload stays minimal while the topology-bearing kinds fill them.
- Auto: `DiagramKind` selects the layout policy and emitted mark set, not the glyph type, so every kind draws from the same seven cases; `GlyphStyle.fill`/`stroke` resolve through the draw owner's `hex_ramp`-projected palette so a recolor is a palette swap; `GlyphStyle.layer` buckets marks into the named `drawsvg` `Group` / `drawpyo` container the `export/layered#LAYERED` owner reads. Shared lowering derivations live here so the SVG, `.drawio`, and CAD arms consume one truth: `DiagramGlyph.mark` projects each case's named payload, `Port.seat` and `AreaMark.centroid` derive from carried geometry alone, and `ENTITY_BAND`/`ER_CAPS` carry the title-band and crow's-foot composition rows.
- Packages: `expression` (the `tagged_union`), `msgspec` (the frozen `Struct` payloads), and `frozendict` (the composition rows) alone — this vocabulary emits no SVG, computes no coordinates, admits no engine; its topology axes exist so admitted packages have typed fields to key onto — the `Port`/`parent`/`source_port`/`target_port` axes the ELK document routes and nests, the `NodeShape`/`EndCap`/`LineCap`/`LineJoin` axes the draw arms lower, the `FragmentMark.d` string the `graphic/vector/path#PATH` `fragment` form carries.
- Growth: a new diagram kind is one `DiagramKind` row plus one layout-policy arm, never a new glyph type; a new node shape one `NodeShape` row, a new marker one `MarkerKind` row, a new terminal one `EndCap` row (a crow's-foot member also lands its `ER_CAPS` composition row), a new route regime one `EdgeRoute` member realized by the layout engine that owns it, a new visual axis one `GlyphStyle` field; a new topology axis is one defaulted field on the owning mark Struct — existing construction sites untouched, a consumer reading it opts in by name (`caps` and `source_port` on `EdgeMark`, `magnitude` on `AreaMark`, and `angle` on `AnnotationMark` entered by exactly this law); a genuinely-new mark is one `DiagramGlyph` case plus one draw arm, earned only when the seven do not cover it (`area` and `fragment` entered so).
- Boundary: no SVG emission (`visualization/diagram/draw#DRAW`'s), no coordinate/ring/port-route computation (`visualization/diagram/layout#LAYOUT`'s — marks carry the computed result, they never re-derive it; the `Port.seat` and `AreaMark.centroid` projections derive from carried geometry alone), no furniture generation (`visualization/diagram/solar#SOLAR`'s — this vocabulary carries only the generated `FragmentMark.d` string), no graph analysis (`data/graph#GRAPH`'s), no named-symbol schematic (`visualization/diagram/schematic#SCHEMATIC`'s `schemdraw` engine owns the anchored-symbol class the seven marks cannot express). A positional case tuple, a forwarding constructor beside the named payload, and an erased attribute `dict` are the rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Final, Literal, assert_never

from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct

# --- [TYPES] ----------------------------------------------------------------------------
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
    RECTANGLE = "rectangle"
    DIAMOND = "diamond"
    OVAL = "oval"
    PARALLELOGRAM = "parallelogram"
    ENTITY = "entity"
    HEXAGON = "hexagon"
    CYLINDER = "cylinder"
    DOCUMENT = "document"
    MULTI_DOCUMENT = "multi_document"
    PREDEFINED_PROCESS = "predefined_process"
    MANUAL_INPUT = "manual_input"
    MANUAL_OPERATION = "manual_operation"
    OFF_PAGE = "off_page"
    STORED_DATA = "stored_data"
    DISPLAY = "display"
    DELAY = "delay"
    CONNECTOR = "connector"
    CARD = "card"
    TAPE = "tape"


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


class EdgeRoute(StrEnum):  # the layout-resolved routing regime every egress lowers; layout alone selects the engine that realizes it
    LINES = "lines"
    SPLINES = "splines"
    ORTHOGONAL = "orthogonal"


class EndCap(StrEnum):
    NONE = "none"
    ARROW = "arrow"
    OPEN = "open"
    BLOCK = "block"
    DIAMOND_OPEN = "diamond_open"
    DIAMOND_FILLED = "diamond_filled"
    CIRCLE = "circle"
    CROSS = "cross"
    ER_ONE = "er_one"
    ER_MANY = "er_many"
    ER_ZERO_ONE = "er_zero_one"
    ER_ONE_MANY = "er_one_many"
    ER_ZERO_MANY = "er_zero_many"


class LineCap(StrEnum):
    BUTT = "butt"
    ROUND = "round"
    SQUARE = "square"


class LineJoin(StrEnum):
    MITER = "miter"
    ROUND = "round"
    BEVEL = "bevel"


class SubLayout(StrEnum):
    INHERIT = "inherit"
    LAYERED = "layered"
    TREE = "tree"
    RADIAL = "radial"
    FORCE = "force"
    STRESS = "stress"
    PACKED = "packed"


class TextAnchor(StrEnum):
    START = "start"
    MIDDLE = "middle"
    END = "end"


# --- [CONSTANTS] ------------------------------------------------------------------------
ENTITY_BAND: Final[float] = 16.0  # ER-entity title-band height; every lowering seats the header divider and the title run on it
ER_CAPS: Final[frozendict[EndCap, tuple[bool, bool, bool]]] = frozendict({
    # crow's-foot family as (ring, bar, fan) composition rows — every lowering derives its five cardinality terminals from this one table
    EndCap.ER_ONE: (False, True, False),
    EndCap.ER_MANY: (False, False, True),
    EndCap.ER_ZERO_ONE: (True, True, False),
    EndCap.ER_ONE_MANY: (False, True, True),
    EndCap.ER_ZERO_MANY: (True, False, True),
})


# --- [MODELS] ---------------------------------------------------------------------------
class TextRun(Struct, frozen=True):
    size: float = 12.0
    weight: int = 400
    italic: bool = False
    family: str | None = None
    ink: int | None = None


class Port(Struct, frozen=True):
    id: str
    side: PortSide = PortSide.EAST
    index: int = 0
    width: float = 8.0
    height: float = 8.0
    at: Point | None = None
    label: str | None = None

    def __post_init__(self) -> None:
        # construction is the admission boundary: a negative index would reach `seat`'s bit-reversal as a negative
        # ordinal — a malformed binary literal at -2, a colliding lane at -1 — so it refuses at mint (msgspec lifts
        # this ValueError to ValidationError on decode).
        if self.index < 0:
            raise ValueError(f"<port-index:{self.index}>")

    def seat(self, x: float, y: float, w: float, h: float, /) -> Point:
        # derives the port's boundary seat from carried data alone: a fixed `at` wins; else the lane is the bit-reversed
        # dyadic fraction of index+1 mapped into (0.08, 0.92) — midpoint-first, collision-free at every index, where a
        # fixed-step outward walk saturates its clamp and collides from index 7.
        if self.at is not None:
            return (x + self.at[0], y + self.at[1])
        ordinal = self.index + 1
        lane = 0.08 + 0.84 * (int(f"{ordinal:b}"[::-1], 2) / (1 << ordinal.bit_length()))
        match self.side:
            case PortSide.NORTH:
                return (x + w * lane, y)
            case PortSide.SOUTH:
                return (x + w * lane, y + h)
            case PortSide.WEST:
                return (x, y + h * lane)
            case PortSide.EAST:
                return (x + w, y + h * lane)
            case _ as unreachable:
                assert_never(unreachable)


class GlyphStyle(Struct, frozen=True):
    layer: str
    fill: int = 0
    stroke: int = 0
    width: float = 1.0
    opacity: float = 1.0
    dash: tuple[float, ...] = ()
    cap: LineCap = LineCap.BUTT
    join: LineJoin = LineJoin.MITER
    corner: float = 0.0
    text: TextRun | None = None


class NodeMark(Struct, frozen=True):
    index: int
    x: float
    y: float
    w: float
    h: float
    label: str | None
    style: GlyphStyle
    shape: NodeShape = NodeShape.RECTANGLE
    ports: tuple[Port, ...] = ()
    parent: int | None = None


class EdgeMark(Struct, frozen=True):
    source: int
    target: int
    points: tuple[Point, ...]
    label: str | None
    style: GlyphStyle
    weight: float = 0.0
    caps: tuple[EndCap, EndCap] = (EndCap.NONE, EndCap.NONE)
    route: EdgeRoute = EdgeRoute.LINES  # the layout-resolved routing regime egress lowers, never a stringly token or per-target hardcode
    source_port: str | None = None
    target_port: str | None = None


class SwimlaneMark(Struct, frozen=True):
    index: int
    x: float
    y: float
    w: float
    h: float
    title: str | None
    style: GlyphStyle
    parent: int | None = None


class AnnotationMark(Struct, frozen=True):
    x: float
    y: float
    text: str
    anchor: TextAnchor
    style: GlyphStyle
    angle: float = 0.0


class MarkerMark(Struct, frozen=True):
    x: float
    y: float
    kind: MarkerKind
    angle: float
    style: GlyphStyle


class AreaMark(Struct, frozen=True):
    index: int
    ring: tuple[Point, ...]
    label: str | None
    style: GlyphStyle
    magnitude: float = 0.0
    parent: int | None = None

    @property
    def centroid(self) -> Point:
        # area-weighted polygon centroid (shoelace form), so a concave parcel's label anchor stays inside the region;
        # a degenerate zero-area ring falls back to the vertex mean, and the empty ring seats the origin — total by construction.
        if not self.ring:
            return (0.0, 0.0)
        pairs = tuple(zip(self.ring, (*self.ring[1:], self.ring[0]), strict=True))
        doubled = sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in pairs)
        if doubled == 0.0:
            return (sum(px for px, _py in self.ring) / len(self.ring), sum(py for _px, py in self.ring) / len(self.ring))
        return (
            sum((x0 + x1) * (x0 * y1 - x1 * y0) for (x0, y0), (x1, y1) in pairs) / (3.0 * doubled),
            sum((y0 + y1) * (x0 * y1 - x1 * y0) for (x0, y0), (x1, y1) in pairs) / (3.0 * doubled),
        )


class FragmentMark(Struct, frozen=True):
    d: str
    label: str | None
    anchor: Point
    style: GlyphStyle


type AnyMark = NodeMark | EdgeMark | SwimlaneMark | AnnotationMark | MarkerMark | AreaMark | FragmentMark


@tagged_union(frozen=True)
class DiagramGlyph:
    tag: GlyphTag = tag()
    node: NodeMark = case()
    edge: EdgeMark = case()
    swimlane: SwimlaneMark = case()
    annotation: AnnotationMark = case()
    marker: MarkerMark = case()
    area: AreaMark = case()
    fragment: FragmentMark = case()

    @property
    def mark(self) -> AnyMark:
        # one case-to-named-payload projection every consumer reads; each mark Struct carries `.style`, so style reads ride the union
        match self:
            case DiagramGlyph(tag="node", node=mark):
                return mark
            case DiagramGlyph(tag="edge", edge=mark):
                return mark
            case DiagramGlyph(tag="swimlane", swimlane=mark):
                return mark
            case DiagramGlyph(tag="annotation", annotation=mark):
                return mark
            case DiagramGlyph(tag="marker", marker=mark):
                return mark
            case DiagramGlyph(tag="area", area=mark):
                return mark
            case DiagramGlyph(tag="fragment", fragment=mark):
                return mark
            case _ as unreachable:
                assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
