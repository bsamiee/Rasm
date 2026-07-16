# [PY_ARTIFACTS_DIAGRAM_LAYOUT]

`DiagramLayout` is the data-driven diagram coordinate-assignment owner: one owner folding a `data/tabular#GRAPH` adjacency frame into the laid-out `visualization/diagram/glyphset#GLYPHSET` mark sequence for every `DiagramKind`. It builds a `rustworkx` `PyDiGraph` from the adjacency rows — the stable non-recycled integer node index the coordinates and marks both key on — assigns coordinates through the `LayoutPolicy` the `DiagramKind` selects, then constructs each positioned `DiagramGlyph` with its resolved geometry, palette index, and layer name. Coordinates and graph-edge routes are all it owns: no SVG (`visualization/diagram/draw#DRAW`'s), no graph analysis (the `data/graph#GRAPH` `rustworkx` kernel stays at the data plane; this owner builds a presentation graph and runs only layout functions).

`LayoutPolicy` is a closed five-case union: `Force` (`spring_layout`), `Radial` (`circular_layout`/`shell_layout` by `RingMode`), `Layered` over the `HierarchyEngine` provider split (`RUSTWORKX` `topological_generations`, `GRANDALF` `SugiyamaLayout`, `FAST_SUGIYAMA` native-Rust Sugiyama, `ELK` ports/nesting/orthogonal), `Projected` over the closed `ProjectionKind` AEC transforms, and `Constrained` over a `kiwisolver` Cassowary solve with PLAN-ANCHORED seeding — a node carrying typed `east`/`north` columns seats at its real plan position under a `strong` stay. `ELK` is sole owner of true `ORTHOGONAL` routing (native `sections[].bendPoints`) and of the `parent`-axis compound-children nesting; `LayoutPolicy.Layered` rewires an `ORTHOGONAL` request to `ELK` so the route never degrades to the straight-line alias a Sugiyama engine cannot escape. V15 area law rules the three area kinds: `STACKING` segments floor bands by program `area`, `PROGRAM` sizes boxes by `sqrt(area)`, `SITE` renders true `Area` polygons from typed `ring`/`footprint` columns with shoelace magnitude, never a fixed rectangle wearing an area label. Plan geometry arrives as typed columns computed upstream, never re-derived here; the synchronous build is one `_render` kernel `_compute` offloads onto the runtime thread lane; the diagram receipt is the draw terminal's — layout returns positioned glyph substrate only.

## [01]-[INDEX]

- [01]-[LAYOUT]: `DiagramLayout`'s coordinate-assignment fold over a `data/tabular#GRAPH` adjacency frame — coordinates via the `LayoutPolicy` the `DiagramKind` selects (force, radial, four-engine layered, projected AEC transforms, plan-anchored constrained), emitting the V15 area-law marks and the composed solar furniture onto the stable node index, offloaded to the runtime thread lane.

## [02]-[LAYOUT]

- Owner: `DiagramLayout` discriminating layout over the `visualization/diagram/glyphset#GLYPHSET` `DiagramKind`; `LayoutPolicy` an `expression.tagged_union` whose every case carries its own typed payload, dispatched by one total `match` folding onto the node coordinate map and optional edge-route map, never a per-diagram-kind layout method family; the adjacency frame is the `data/tabular#GRAPH` BYODF input the `PyDiGraph` builds from, plan geometry riding as typed columns (`east`/`north`, per-program `area`, `ring`/`footprint` vertices, `sublayout` engines, `source_cardinality`/`target_cardinality` caps, `latitude`/`longitude`/`year` site rows) computed upstream, never an untyped attribute bag re-derived here; `LayoutMap` the resolved node-index-to-coordinate map every arm returns, `RouteMap` the optional edge-waypoint overlay the route-capable engines fill.
- Cases: `Force`/`Radial`/`Layered`/`Projected`/`Constrained`, one total `match` over the policy `tag`; `Layered`'s `engine` selects the provider via the `_ENGINE` table, `Radial` the `_RING` mode, `Projected` the `PROJECTION` transform — each reading typed node attributes through `_attr`, never a fragile `attrs[key]`; `Constrained` seeds plan-anchored (typed `east`/`north` under a `strong` stay, every other node the `circular_layout` spread under a `weak` stay) then folds the `LayoutRule` set to `kiwisolver` `Constraint`s; `CIRCULATION` and `SECTION_CALLOUT` select `Constrained()` in `_KIND_POLICY` because their marks are plan-anchored building geometry, the `Force()` default that scattered rooms by topology being the rejected form.
- Auto: `_compute` offloads the one synchronous `_render` kernel onto the runtime thread lane so every native placement runs off the event loop; `_position` is the total `LayoutPolicy` dispatch. `FAST_SUGIYAMA` is the default layered placement (native-Rust, superseding the `GRANDALF` core): `_sugiyama_layout` filters the synthetic dummy ids, strands the isolated nodes `from_edges` drops onto a trailing rank, and traces each long edge's dummy chain into a `RouteMap` polyline. `ELK` folds one `ELK().layout` result, never a second render — `_nest` reads the `parent` node-domain-id into a compound-children map under `INCLUDE_CHILDREN`, each `Port`'s fixed `at` escalating its node to `FIXED_POS`, a container's `sublayout` selecting its own inner `elk.algorithm`, and `_absolutize` lifting parent-relative positions to absolute. `SUN_PATH` composes `visualization/diagram/solar#SOLAR`: `project` positions each mark under the declared hemisphere projection and `furniture` supplies the backdrop as `Fragment` marks when the frame carries `latitude`/`longitude`/`year` columns, so a caller-angles-only frame degrades to bare marks, never fake furniture. `_emit` threads the `GlyphStyle` palette-and-layer binding and the typed `EndCap` caps, the ER `source_cardinality`/`target_cardinality` columns mapping through `_CARDINALITY` onto crow's-foot caps.
- Growth: a new diagram kind is one `DiagramKind` row plus one `_emit` arm and one `_KIND_POLICY` selection, never a new layout method family; a new layered engine one `HierarchyEngine` member plus one `_ENGINE` row, never a fifth `LayeredPolicy` case; a new AEC projection one `ProjectionKind` member plus a `_solar_arc`-shaped transform plus a `PROJECTION` row; a new ring mode one `RingMode`/`_RING` pair; a new constraint one `LayoutRule` case plus one `_rule_constraints` arm; a new container engine one `_SUB_ALGORITHM` row; a new cardinality token one `_CARDINALITY` row; a new route style rides the engine that owns it.
- Boundary: no SVG emission (`visualization/diagram/draw#DRAW`'s), no graph analysis (`data/graph#GRAPH`'s centrality/shortest-path/community kernel stays at the data plane; this owner runs only layout functions), no ephemeris computation (`visualization/diagram/solar#SOLAR` solves sun positions; this owner composes `project`/`furniture`), no ad-hoc color (the `GlyphStyle` indices key the `graphic/color/derive#DERIVE` palette); `rustworkx` owns force/radial/topological placement, `grandalf`/`fast-sugiyama` Sugiyama, `pyelk` ports/nesting/orthogonal, `kiwisolver` the Cassowary solve, all behind one `LayoutPolicy` owner — a hand-rolled Sugiyama/spring/constraint loop, an `ORTHOGONAL` aliased to a straight-line router, a synchronous native layout left on the event loop, a `Force()` default on a plan-anchored kind, and a fixed rectangle standing for a measured area are the rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from itertools import groupby, pairwise
from math import sqrt
from operator import itemgetter
from typing import Final, Literal, assert_never

import polars as pl
import rustworkx as rx
from expression import case, tag, tagged_union
from expression.collections import Map
from fast_sugiyama import from_edges
from grandalf.graphs import Edge as GrandalfEdge, Graph as GrandalfGraph, Vertex as GrandalfVertex
from grandalf.layouts import SugiyamaLayout, VertexViewer
from grandalf.routing import EdgeViewer, route_with_lines, route_with_splines
from kiwisolver import Constraint, Solver, Variable
from msgspec import Struct
from pyelk import ELK
from pyelk.graph import collect_edges, validate_graph

from rasm.artifacts.visualization.diagram.solar import Site, SolarProjection, furniture, project

from rasm.runtime.identity import ContentIdentity
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.visualization.diagram.glyphset import (
    DiagramGlyph,
    DiagramKind,
    EndCap,
    GlyphStyle,
    MarkerKind,
    NodeShape,
    Port,
    PortSide,
    SubLayout,
    TextAnchor,
)


# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type LayoutMap = dict[int, Point]
type RouteMap = dict[tuple[int, int], tuple[Point, ...]]
type LayoutResult = tuple[LayoutMap, RouteMap]
type Projection = Callable[[dict[str, object], float], Point]
type Router = Callable[[GrandalfEdge, list[object]], None]
type LayerDirection = Literal["TB", "BT", "LR", "RL"]
type RuleAxis = Literal["x", "y"]
type RuleStrength = Literal["weak", "medium", "strong", "required"]


class HierarchyEngine(StrEnum):
    RUSTWORKX = "rustworkx"  # deterministic topological_generations fallback
    GRANDALF = "grandalf"  # parity oracle + spline routes, superseded by FAST_SUGIYAMA
    FAST_SUGIYAMA = "fast-sugiyama"  # native-Rust Sugiyama placement, the default
    ELK = "elk"  # ports/nesting/orthogonal, native bendPoints


class EdgeRoute(StrEnum):
    LINES = "lines"
    SPLINES = "splines"
    ORTHOGONAL = "orthogonal"  # owned by the ELK engine's native section geometry


class RingMode(StrEnum):
    CIRCULAR = "circular"  # one ring, circular_layout
    SHELL = "shell"  # concentric rings, shell_layout over the ring-attribute partition


class ProjectionKind(StrEnum):
    SOLAR_ARC = "solar_arc"  # azimuth/altitude -> the solar owner's stereographic sun-path fold
    FLOOR_BAND = "floor_band"  # level -> stacking band y
    PARCEL_GRID = "parcel_grid"  # east/north -> site cartesian cell


class LayeredPolicy(Struct, frozen=True):
    direction: LayerDirection = "TB"
    engine: HierarchyEngine = HierarchyEngine.FAST_SUGIYAMA
    route: EdgeRoute = EdgeRoute.LINES


@tagged_union(frozen=True)
class LayoutRule:
    # constraint vocabulary the Constrained policy folds onto the kiwisolver Solver; each case names the node indices and strength band.
    tag: Literal["align", "separate", "distribute", "anchor", "mirror"] = tag()
    align: tuple[tuple[int, ...], RuleAxis, RuleStrength] = case()
    separate: tuple[int, int, RuleAxis, float, RuleStrength] = case()
    distribute: tuple[tuple[int, ...], RuleAxis, float, RuleStrength] = case()
    anchor: tuple[int, Point, RuleStrength] = case()
    mirror: tuple[int, int, RuleAxis, float, RuleStrength] = case()

    @staticmethod
    def Align(nodes: tuple[int, ...], axis: RuleAxis = "x", strength: RuleStrength = "strong") -> "LayoutRule":
        return LayoutRule(align=(nodes, axis, strength))

    @staticmethod
    def Separate(before: int, after: int, axis: RuleAxis = "x", gap: float = 48.0, strength: RuleStrength = "required") -> "LayoutRule":
        return LayoutRule(separate=(before, after, axis, gap, strength))

    @staticmethod
    def Distribute(nodes: tuple[int, ...], axis: RuleAxis = "x", gap: float = 64.0, strength: RuleStrength = "medium") -> "LayoutRule":
        return LayoutRule(distribute=(nodes, axis, gap, strength))

    @staticmethod
    def Anchor(node: int, at: Point, strength: RuleStrength = "required") -> "LayoutRule":
        return LayoutRule(anchor=(node, at, strength))

    @staticmethod
    def Mirror(left: int, right: int, about: float, axis: RuleAxis = "x", strength: RuleStrength = "medium") -> "LayoutRule":
        return LayoutRule(mirror=(left, right, axis, about, strength))


@tagged_union(frozen=True)
class LayoutPolicy:
    tag: Literal["force", "radial", "layered", "projected", "constrained"] = tag()
    force: tuple[int, int] = case()
    radial: tuple[float, RingMode] = case()
    layered: LayeredPolicy = case()
    projected: tuple[ProjectionKind, float] = case()
    constrained: tuple[LayoutRule, ...] = case()

    @staticmethod
    def Force(seed: int = 0, iterations: int = 100) -> "LayoutPolicy":
        return LayoutPolicy(force=(seed, iterations))

    @staticmethod
    def Radial(scale: float = 1.0, mode: RingMode = RingMode.CIRCULAR) -> "LayoutPolicy":
        return LayoutPolicy(radial=(scale, mode))

    @staticmethod
    def Layered(
        direction: LayerDirection = "TB", engine: HierarchyEngine = HierarchyEngine.FAST_SUGIYAMA, route: EdgeRoute = EdgeRoute.LINES
    ) -> "LayoutPolicy":
        # ORTHOGONAL is ELK's native capability; force the engine so it never degrades to a straight-line alias on a Sugiyama engine.
        resolved = HierarchyEngine.ELK if route is EdgeRoute.ORTHOGONAL else engine
        return LayoutPolicy(layered=LayeredPolicy(direction=direction, engine=resolved, route=route))

    @staticmethod
    def Projected(kind: ProjectionKind, scale: float = 1.0) -> "LayoutPolicy":
        return LayoutPolicy(projected=(kind, scale))

    @staticmethod
    def Constrained(*rules: LayoutRule) -> "LayoutPolicy":
        return LayoutPolicy(constrained=rules)


# --- [CONSTANTS] ------------------------------------------------------------------------
_NODE_W: Final[float] = 48.0
_NODE_H: Final[float] = 32.0
_LAYER_GAP: Final[float] = 60.0
_BAND_H: Final[float] = 30.0  # stacking floor-band height in sheet units
_AREA_UNIT: Final[float] = 0.5  # sheet units per plan-area unit along a stacking band
_AREA_SIDE: Final[float] = 4.0  # sheet units per sqrt(plan-area) for area-true program boxes


# --- [TABLES] ---------------------------------------------------------------------------
_ELK_DIRECTION: Final[Map[LayerDirection, str]] = Map.of_seq([("TB", "DOWN"), ("BT", "UP"), ("LR", "RIGHT"), ("RL", "LEFT")])
_ELK_OPTIONS: Final[Map[str, str]] = Map.of_seq([
    ("elk.layered.crossingMinimization.strategy", "LAYER_SWEEP"),
    ("elk.layered.layering.strategy", "LONGEST_PATH"),
])
_RANKING: Final[Map[LayerDirection, tuple[str, bool]]] = Map.of_seq([
    ("TB", ("down", False)),
    ("BT", ("up", False)),
    ("LR", ("down", True)),
    ("RL", ("up", True)),  # (ranking_type, swap_xy)
])
_AXIS: Final[Map[RuleAxis, int]] = Map.of_seq([("x", 0), ("y", 1)])
_NODE_SHAPES: Final[Map[str, NodeShape]] = Map.of_seq((shape.value, shape) for shape in NodeShape)
_PORT_SIDES: Final[Map[str, PortSide]] = Map.of_seq((side.value, side) for side in PortSide)
_SUB_ALGORITHM: Final[Map[SubLayout, str]] = Map.of_seq([
    # the registered ELK provider per SubLayout member; INHERIT never reaches the lookup (guarded upstream)
    (SubLayout.LAYERED, "layered"),
    (SubLayout.TREE, "mrtree"),
    (SubLayout.RADIAL, "radial"),
    (SubLayout.FORCE, "force"),
    (SubLayout.STRESS, "stress"),
    (SubLayout.PACKED, "rectpacking"),
])
_CARDINALITY: Final[Map[str, EndCap]] = Map.of_seq([
    # typed ER cardinality tokens on the edge columns -> crow's-foot caps; absent/foreign -> NONE
    ("one", EndCap.ER_ONE),
    ("many", EndCap.ER_MANY),
    ("zero_one", EndCap.ER_ZERO_ONE),
    ("one_many", EndCap.ER_ONE_MANY),
    ("zero_many", EndCap.ER_ZERO_MANY),
])
_KIND_POLICY: Final[Map[DiagramKind, LayoutPolicy]] = Map.of_seq([
    (DiagramKind.SUN_PATH, LayoutPolicy.Projected(ProjectionKind.SOLAR_ARC, 200.0)),
    (DiagramKind.CIRCULATION, LayoutPolicy.Constrained()),  # plan-anchored, never a spring scatter
    (DiagramKind.STACKING, LayoutPolicy.Projected(ProjectionKind.FLOOR_BAND, 40.0)),
    (DiagramKind.PROGRAM, LayoutPolicy.Layered(engine=HierarchyEngine.FAST_SUGIYAMA)),
    (DiagramKind.SITE, LayoutPolicy.Projected(ProjectionKind.PARCEL_GRID, 1.0)),
    (DiagramKind.NODE_LINK, LayoutPolicy.Force()),
    (DiagramKind.FLOWCHART, LayoutPolicy.Layered(direction="TB", route=EdgeRoute.ORTHOGONAL)),
    (DiagramKind.ENTITY_RELATION, LayoutPolicy.Layered(engine=HierarchyEngine.ELK)),
    (DiagramKind.SANKEY, LayoutPolicy.Layered(direction="LR", engine=HierarchyEngine.FAST_SUGIYAMA)),
    (DiagramKind.SECTION_CALLOUT, LayoutPolicy.Constrained()),  # plan-anchored detail frames, never a spring scatter
])


# --- [MODELS] ---------------------------------------------------------------------------
class DiagramLayout(Struct, frozen=True):
    kind: DiagramKind
    adjacency: pl.DataFrame
    attributes: pl.DataFrame
    policy: LayoutPolicy | None = None

    async def assign(self) -> RuntimeRail[tuple[DiagramGlyph, ...]]:
        # coordinate substrate for draw; layout mints no receipt.
        return await async_boundary(f"diagram.layout.{self.kind}", self._compute)

    async def _compute(self) -> tuple[DiagramGlyph, ...]:
        return (await LanePolicy.offload(self._render, modality=Modality.THREAD, retry=RetryClass.OCCT)).default_with(_layout_raise)

    def _render(self) -> tuple[DiagramGlyph, ...]:
        policy = self.policy or _KIND_POLICY[self.kind]
        graph = self._as_graph()
        coords, routes = self._position(graph, policy)
        return self._emit(graph, policy, coords, routes)

    def _as_graph(self) -> rx.PyDiGraph:
        graph: rx.PyDiGraph = rx.PyDiGraph()
        rows = self.attributes.to_dicts()
        indices = graph.add_nodes_from(rows)
        lookup = dict(zip((row["id"] for row in rows), indices, strict=True))
        graph.add_edges_from((lookup[r["source"]], lookup[r["target"]], r) for r in self.adjacency.to_dicts())
        return graph

    def _position(self, graph: rx.PyDiGraph, policy: LayoutPolicy) -> LayoutResult:
        match policy:
            case LayoutPolicy(tag="force", force=(seed, iterations)):
                pos = rx.spring_layout(graph, seed=seed, num_iter=iterations)
                return {i: _pt(pos[i]) for i in graph.node_indices()}, {}
            case LayoutPolicy(tag="radial", radial=(scale, mode)):
                pos = _RING[mode](graph, scale)
                return {i: _pt(pos[i]) for i in graph.node_indices()}, {}
            case LayoutPolicy(tag="layered", layered=layered):
                return _ENGINE[layered.engine](graph, layered)
            case LayoutPolicy(tag="projected", projected=(kind, scale)):
                transform = PROJECTION[kind]
                return {i: transform(graph.get_node_data(i), scale) for i in graph.node_indices()}, {}
            case LayoutPolicy(tag="constrained", constrained=rules):
                return _constrained_layout(graph, rules)
            case _ as unreachable:
                assert_never(unreachable)

    def _emit(self, graph: rx.PyDiGraph, policy: LayoutPolicy, coords: LayoutMap, routes: RouteMap) -> tuple[DiagramGlyph, ...]:
        match self.kind:
            case DiagramKind.SUN_PATH:
                scale = _proj_scale(policy)
                ticks = tuple(
                    DiagramGlyph.Marker(*coords[i], MarkerKind.TICK, _attr(graph.get_node_data(i), "azimuth"), GlyphStyle("sun"))
                    for i in graph.node_indices()
                )
                arcs = tuple(
                    DiagramGlyph.Edge(s, t, _edge_points(routes, coords, s, t), None, GlyphStyle("path", stroke=1)) for s, t in graph.edge_list()
                )
                return (*_sun_furniture(graph, scale), *arcs, *ticks, DiagramGlyph.Marker(0.0, 0.0, MarkerKind.NORTH, 0.0, GlyphStyle("compass")))
            case DiagramKind.CIRCULATION:
                boxes = tuple(
                    DiagramGlyph.Node(i, *coords[i], 40.0, 24.0, _label(graph.get_node_data(i), "label"), GlyphStyle("spaces"))
                    for i in graph.node_indices()
                )
                flows = tuple(
                    DiagramGlyph.Edge(
                        s, t, _edge_points(routes, coords, s, t), None, GlyphStyle("circulation", stroke=2), caps=(EndCap.NONE, EndCap.ARROW)
                    )
                    for s, t in graph.edge_list()
                )
                return (*boxes, *flows)
            case DiagramKind.STACKING:
                _roots, kids = _nest(graph)
                floors = tuple(i for i in graph.node_indices() if isinstance(graph.get_node_data(i).get("level"), int | float))
                bands = tuple(
                    DiagramGlyph.Swimlane(
                        i,
                        0.0,
                        coords[i][1],
                        max(_NODE_W, _floor_total(graph, kids.get(i, ())) * _AREA_UNIT),
                        _BAND_H,
                        _label(graph.get_node_data(i), "floor"),
                        GlyphStyle("floors", fill=i),
                    )
                    for i in floors
                )
                segments = tuple(segment for i in floors for segment in _segments(graph, i, kids.get(i, ()), coords[i][1]))
                return (*bands, *segments)
            case DiagramKind.PROGRAM:
                boxes = tuple(
                    DiagramGlyph.Node(i, *coords[i], side, side, _label(d, "program"), GlyphStyle("program", fill=i))
                    for i in graph.node_indices()
                    for d in (graph.get_node_data(i),)
                    for side in (_program_side(d),)
                )
                adj = tuple(
                    DiagramGlyph.Edge(s, t, _edge_points(routes, coords, s, t), None, GlyphStyle("adjacency", stroke=3)) for s, t in graph.edge_list()
                )
                return (*boxes, *adj)
            case DiagramKind.SITE:
                scale = _proj_scale(policy)
                polygons = tuple(
                    mark
                    for i in graph.node_indices()
                    for d in (graph.get_node_data(i),)
                    for mark in (
                        _polygon(d, "ring", scale, i, _label(d, "label"), "parcels"),
                        _polygon(d, "footprint", scale, i, None, "buildings"),
                    )
                    if mark is not None
                )
                callouts = tuple(
                    DiagramGlyph.Annotation(*coords[i], text, TextAnchor.MIDDLE, GlyphStyle("callouts"))
                    for i in graph.node_indices()
                    if (text := _label(graph.get_node_data(i), "label")) is not None
                )
                return (*polygons, *callouts)
            case DiagramKind.NODE_LINK:
                nodes = tuple(
                    DiagramGlyph.Node(
                        i,
                        *coords[i],
                        _NODE_W,
                        _NODE_H,
                        _label((d := graph.get_node_data(i)), "label"),
                        GlyphStyle("nodes", fill=i),
                        _shape_of(d),
                        _ports_of(d),
                    )
                    for i in graph.node_indices()
                )
                links = tuple(
                    DiagramGlyph.Edge(
                        s,
                        t,
                        _edge_points(routes, coords, s, t),
                        _label(graph.get_edge_data(s, t), "label"),
                        GlyphStyle("links", stroke=1),
                        caps=(EndCap.NONE, EndCap.ARROW),
                    )
                    for s, t in graph.edge_list()
                )
                return (*nodes, *links)
            case DiagramKind.FLOWCHART:
                steps = tuple(
                    DiagramGlyph.Node(
                        i,
                        *coords[i],
                        _NODE_W,
                        _NODE_H,
                        _label((d := graph.get_node_data(i)), "label"),
                        GlyphStyle("nodes", fill=i),
                        _shape_of(d),
                        _ports_of(d),
                    )
                    for i in graph.node_indices()
                )
                flow = tuple(
                    DiagramGlyph.Edge(
                        s,
                        t,
                        _edge_points(routes, coords, s, t),
                        _label(graph.get_edge_data(s, t), "label"),
                        GlyphStyle("flow", stroke=2),
                        caps=(EndCap.NONE, EndCap.ARROW),
                    )
                    for s, t in graph.edge_list()
                )
                return (*steps, *flow)
            case DiagramKind.ENTITY_RELATION:
                entities = tuple(
                    DiagramGlyph.Node(
                        i,
                        *coords[i],
                        _NODE_W * 1.6,
                        _NODE_H * 2.0,
                        _label((d := graph.get_node_data(i)), "entity"),
                        GlyphStyle("entities", fill=i),
                        NodeShape.ENTITY,
                        _ports_of(d),
                    )
                    for i in graph.node_indices()
                )
                relations = tuple(
                    DiagramGlyph.Edge(
                        s,
                        t,
                        _edge_points(routes, coords, s, t),
                        _label(e, "label"),
                        GlyphStyle("relations", stroke=3),
                        caps=(_cap_of(e, "source_cardinality"), _cap_of(e, "target_cardinality")),
                    )
                    for s, t in graph.edge_list()
                    for e in (graph.get_edge_data(s, t),)
                )
                return (*entities, *relations)
            case DiagramKind.SANKEY:
                stages = tuple(
                    DiagramGlyph.Node(i, *coords[i], _NODE_W, _NODE_H, _label(graph.get_node_data(i), "stage"), GlyphStyle("stages", fill=i))
                    for i in graph.node_indices()
                )
                ribbons = tuple(
                    DiagramGlyph.Edge(s, t, _edge_points(routes, coords, s, t), None, GlyphStyle("ribbons", stroke=s), _weight(graph, s, t))
                    for s, t in graph.edge_list()
                )
                return (*stages, *ribbons)
            case DiagramKind.SECTION_CALLOUT:
                frames = tuple(
                    DiagramGlyph.Swimlane(i, *coords[i], 160.0, 120.0, _label(graph.get_node_data(i), "detail"), GlyphStyle("frame", fill=i))
                    for i in graph.node_indices()
                )
                refs = tuple(
                    DiagramGlyph.Annotation(*coords[i], text, TextAnchor.MIDDLE, GlyphStyle("callouts"))
                    for i in graph.node_indices()
                    if (text := _label(graph.get_node_data(i), "reference")) is not None
                )
                return (*frames, *refs)
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _wire_attrs(data: dict[str, object], /) -> dict[str, str]:
    return {name: str(value) for name, value in data.items()}


def _label(data: dict[str, object], key: str, /) -> str | None:
    return None if (value := data.get(key)) is None else str(value)


def _attr(data: dict[str, object], key: str, /) -> float:
    # optional node-attribute read; a missing or non-numeric column folds to 0.0, never a KeyError, so a partial frame never faults the layout.
    return float(value) if isinstance(value := data.get(key), int | float) else 0.0


def _shape_of(data: dict[str, object], /) -> NodeShape:
    # node silhouette from the optional `shape` attribute; absent -> RECTANGLE.
    return _NODE_SHAPES.try_find(value).default_value(NodeShape.RECTANGLE) if isinstance(value := data.get("shape"), str) else NodeShape.RECTANGLE


def _sublayout_of(data: dict[str, object], /) -> SubLayout:
    # inner-engine override from the optional `sublayout` attribute; absent -> INHERIT.
    return _sub_member(value) if isinstance(value := data.get("sublayout"), str) else SubLayout.INHERIT


def _sub_member(value: str, /) -> SubLayout:
    return SubLayout(value) if value in {member.value for member in SubLayout} else SubLayout.INHERIT


def _cap_of(data: dict[str, object], key: str, /) -> EndCap:
    # ER cardinality from the optional edge column; absent or foreign -> NONE.
    return _CARDINALITY.try_find(value).default_value(EndCap.NONE) if isinstance(value := data.get(key), str) else EndCap.NONE


def _ports_of(data: dict[str, object], /) -> tuple[Port, ...]:
    # typed connection ports from the optional `ports` attribute; side-local index is the list position, an explicit coordinate seats FIXED_POS.
    return tuple(_port_of(entry, index) for index, entry in enumerate(raw)) if isinstance(raw := data.get("ports"), list | tuple) else ()


def _port_of(entry: object, index: int, /) -> Port:
    match entry:
        case (str() as pid, str() as side, (float() | int() as ax, float() | int() as ay)):
            return Port(pid, _side_member(side), index, at=(float(ax), float(ay)))
        case (str() as pid, str() as side):
            return Port(pid, _side_member(side), index)
        case str() as pid:
            return Port(pid, PortSide.EAST, index)
        case _:
            return Port(f"p{index}", PortSide.EAST, index)


def _side_member(value: str, /) -> PortSide:
    return _PORT_SIDES.try_find(value).default_value(PortSide.EAST)


def _weight(graph: rx.PyDiGraph, source: int, target: int, /) -> float:
    # Sankey ribbon magnitude from the edge `weight`.
    return _attr(graph.get_edge_data(source, target), "weight")


def _parent_of(data: dict[str, object], /) -> object:
    # compound-nesting parent from the optional `parent` id; absent -> None (a root).
    return None if isinstance(value := data.get("parent"), bool) else value


def _pt(point: tuple[float, float], /) -> Point:
    return (float(point[0]), float(point[1]))


def _oriented(point: tuple[float, float], swap: bool, /) -> Point:
    return (float(point[1]), float(point[0])) if swap else (float(point[0]), float(point[1]))


def _edge_points(routes: RouteMap, coords: LayoutMap, source: int, target: int, /) -> tuple[Point, ...]:
    return routes.get((source, target), (coords[source], coords[target]))


def _proj_scale(policy: LayoutPolicy, /) -> float:
    match policy:
        case LayoutPolicy(tag="projected", projected=(_, scale)):
            return scale
        case _:
            return 1.0


def _algorithm(policy: LayoutPolicy, /) -> str:
    match policy:
        case LayoutPolicy(tag="layered", layered=LayeredPolicy(engine=engine)):
            return engine.value
        case LayoutPolicy(tag="projected", projected=(kind, _)):
            return kind.value
        case LayoutPolicy(tag="radial", radial=(_, mode)):
            return mode.value
        case _:
            return policy.tag


# --- [AREA_LAW] ---------------------------------------------------------------------------
def _floor_total(graph: rx.PyDiGraph, children: tuple[int, ...], /) -> float:
    # floor band's plan-area total over its program children; band length derives from real area.
    return sum(_attr(graph.get_node_data(child), "area") for child in children)


def _segments(graph: rx.PyDiGraph, floor: int, children: tuple[int, ...], y: float, /):
    # per-program band segments: cumulative x offsets proportional to the typed `area` columns.
    x = 0.0
    for child in children:  # Exemption: the cumulative x offset is an ordered running fold the generator carries
        width = max(1.0, _attr(graph.get_node_data(child), "area") * _AREA_UNIT)
        yield DiagramGlyph.Node(child, x, y, width, _BAND_H, _label(graph.get_node_data(child), "program"), GlyphStyle("program", fill=child), parent=floor)
        x += width


def _program_side(data: dict[str, object], /) -> float:
    # area-true: drawn side tracks sqrt(area), so drawn footprint is proportional to program area.
    area = _attr(data, "area")
    return sqrt(area) * _AREA_SIDE if area > 0.0 else _NODE_W


def _polygon(data: dict[str, object], key: str, scale: float, index: int, label: str | None, layer: str, /) -> DiagramGlyph | None:
    # typed plan-polygon ingress: (east, north) vertices computed upstream, same transform as PARCEL_GRID; shoelace magnitude on the raw ring.
    raw = data.get(key)
    if not isinstance(raw, list | tuple) or len(raw) < 3:
        return None
    ring = tuple((float(east) * scale, -float(north) * scale) for east, north in raw)
    return DiagramGlyph.Area(index, ring, label, GlyphStyle(layer, fill=index), _shoelace(raw))


def _shoelace(raw: "list | tuple", /) -> float:
    pairs = tuple((float(east), float(north)) for east, north in raw)
    return abs(sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in pairwise((*pairs, pairs[0])))) / 2.0


def _sun_furniture(graph: rx.PyDiGraph, radius: float, /) -> tuple[DiagramGlyph, ...]:
    # composes the solar furniture when the frame carries site columns; an angles-only frame draws bare marks, never fabricated furniture.
    head = next(
        (data for i in graph.node_indices() if isinstance(data := graph.get_node_data(i), dict) and "latitude" in data and "year" in data), None
    )
    if head is None:
        return ()
    site = Site(latitude=_attr(head, "latitude"), longitude=_attr(head, "longitude"))
    return tuple(
        DiagramGlyph.Fragment(row.fragment, row.label or None, row.anchor, GlyphStyle("furniture"))
        for row in furniture(site, SolarProjection.STEREOGRAPHIC, radius, int(_attr(head, "year")))
        if row.fragment
    )


# --- [PROJECTIONS] ----------------------------------------------------------------------
def _solar_arc(data: dict[str, object], scale: float, /) -> Point:
    # the SAME projection fold that positions the furniture, so a mark and its backdrop cannot disagree.
    return project(_attr(data, "azimuth"), _attr(data, "altitude"), SolarProjection.STEREOGRAPHIC, scale)


def _floor_band(data: dict[str, object], scale: float, /) -> Point:
    return (0.0, _attr(data, "level") * scale)  # model-space stacking band; the sheet viewport flips vertical


def _parcel_grid(data: dict[str, object], scale: float, /) -> Point:
    return (_attr(data, "east") * scale, -_attr(data, "north") * scale)


def _shells(graph: rx.PyDiGraph, /) -> list[list[int]] | None:
    # concentric shells by the node `ring` attribute; groupby needs the ring-sorted stream. No partition -> None -> single ring.
    banded = sorted((_attr(graph.get_node_data(i), "ring"), i) for i in graph.node_indices())
    return [[node for _band, node in run] for _ring, run in groupby(banded, key=itemgetter(0))] or None


PROJECTION: Final[Map[ProjectionKind, Projection]] = Map.of_seq([
    (ProjectionKind.SOLAR_ARC, _solar_arc),
    (ProjectionKind.FLOOR_BAND, _floor_band),
    (ProjectionKind.PARCEL_GRID, _parcel_grid),
])
_RING: Final[Map[RingMode, Callable[[rx.PyDiGraph, float], rx.Pos2DMapping]]] = Map.of_seq([
    (RingMode.CIRCULAR, lambda graph, scale: rx.circular_layout(graph, scale=scale)),
    (RingMode.SHELL, lambda graph, scale: rx.shell_layout(graph, nlist=_shells(graph), scale=scale)),
])


# --- [ENGINES] --------------------------------------------------------------------------
def _layered(graph: rx.PyDiGraph, direction: LayerDirection, gap: float = _LAYER_GAP) -> LayoutMap:
    return {
        node: _layer_point(depth, rank, direction, gap)
        for depth, layer in enumerate(rx.topological_generations(graph))
        for rank, node in enumerate(layer)
    }


def _layer_point(depth: int, rank: int, direction: LayerDirection, gap: float, /) -> Point:
    match direction:
        case "TB":
            return (rank * gap, depth * gap)
        case "BT":
            return (rank * gap, -depth * gap)
        case "LR":
            return (depth * gap, rank * gap)
        case "RL":
            return (-depth * gap, rank * gap)
        case _ as unreachable:
            assert_never(unreachable)


def _grandalf_layout(graph: rx.PyDiGraph, policy: LayeredPolicy, /) -> LayoutResult:
    vertices = {i: GrandalfVertex(i) for i in graph.node_indices()}
    if not vertices:
        return {}, {}
    edges = tuple(GrandalfEdge(vertices[source], vertices[target]) for source, target in graph.edge_list())
    component = GrandalfGraph(list(vertices.values()), list(edges)).C[0]
    for vertex in vertices.values():  # Exemption: grandalf VertexViewer/EdgeViewer are mutable view sinks the provider reads before draw()
        vertex.view = VertexViewer(_NODE_W, _NODE_H)
    for edge in edges:
        edge.view = EdgeViewer()
    layout = SugiyamaLayout(component)
    layout.route_edge = _grandalf_router(policy.route)
    layout.init_all()
    layout.draw()
    coords = {index: _pt(vertices[index].view.xy) for index in graph.node_indices()}
    routes = {(int(edge.v[0].data), int(edge.v[1].data)): _edge_route(edge) for edge in edges}
    return coords, routes


def _grandalf_router(route: EdgeRoute, /) -> Router:
    match route:
        case EdgeRoute.LINES:
            return route_with_lines
        # grandalf ships no orthogonal router; ORTHOGONAL is rewired to ELK upstream, so this arm only ever binds a curve.
        case EdgeRoute.SPLINES | EdgeRoute.ORTHOGONAL:
            return route_with_splines
        case _ as unreachable:
            assert_never(unreachable)


def _edge_route(edge: GrandalfEdge, /) -> tuple[Point, ...]:
    return tuple(_pt(point) for point in edge.view._pts)


def _sugiyama_layout(graph: rx.PyDiGraph, policy: LayeredPolicy, /) -> LayoutResult:
    ranking, swap = _RANKING[policy.direction]
    arranged = from_edges(
        list(graph.edge_list()), dummy_vertices=True, ranking_type=ranking, crossing_minimization="median", vertex_spacing=_LAYER_GAP
    ).dot_layout()
    placed: LayoutMap = {int(node): _oriented(point, swap) for component in arranged for node, point in component[0]}
    real = frozenset(graph.node_indices())
    coords: LayoutMap = {index: placed[index] for index in real if index in placed}
    coords |= _stranded(tuple(index for index in real if index not in placed), coords)  # from_edges drops isolated nodes
    segments = tuple((int(source), int(target)) for component in arranged for source, target in (component[3] or ()))
    return coords, _sugiyama_routes(segments, placed, real)


def _sugiyama_routes(segments: tuple[tuple[int, int], ...], placed: LayoutMap, real: frozenset[int], /) -> RouteMap:
    successor = {source: target for source, target in segments if source not in real}  # each dummy routes one edge
    routes: RouteMap = {}
    for source, hop in segments:  # Exemption: a long edge decomposes into a single-successor dummy chain the trace follows to its real terminal
        if source in real and hop not in real:
            chain, node = [source, hop], hop
            while node not in real:
                node = successor[node]
                chain.append(node)
            routes[(source, node)] = tuple(placed[step] for step in chain)
    return routes


def _stranded(absent: tuple[int, ...], placed: LayoutMap, /) -> LayoutMap:
    base = max((y for _x, y in placed.values()), default=0.0) + _LAYER_GAP
    return {node: (rank * _LAYER_GAP, base) for rank, node in enumerate(absent)}


def _elk_layout(graph: rx.PyDiGraph, policy: LayeredPolicy, /) -> LayoutResult:
    document = _elk_document(graph, policy)
    validate_graph(document)  # InvalidGraphException (an ElkError) on a malformed doc, trapped at async_boundary
    result = ELK().layout(document, layout_options=document["layoutOptions"])  # hierarchyHandling rides the doc root when nested
    coords: LayoutMap = {}
    _absolutize(result.get("children") or [], (0.0, 0.0), coords)
    routes: RouteMap = {
        (int(edge["sources"][0]), int(edge["targets"][0])): _elk_route(section)
        for edge in collect_edges(result)
        if "startPoint" in (section := (edge.get("sections") or ({},))[0])
    }
    return coords, routes


def _elk_document(graph: rx.PyDiGraph, policy: LayeredPolicy, /) -> dict:
    # foreign ELK-JSON contract, built structurally never an f-string splice; node id is the stringified stable index.
    # Every edge stays at the root so its section geometry reads back root-absolute; INCLUDE_CHILDREN lets a cross-container edge route.
    roots, kids = _nest(graph)
    options = dict(_ELK_OPTIONS.items()) | {"elk.algorithm": "layered", "elk.direction": _ELK_DIRECTION[policy.direction]}
    return {
        "id": "root",
        "layoutOptions": options | ({"elk.hierarchyHandling": "INCLUDE_CHILDREN"} if kids else {}),
        "children": [_elk_node(index, graph, kids) for index in roots],
        "edges": [
            {"id": f"e{ordinal}", "sources": [str(source)], "targets": [str(target)]} for ordinal, (source, target) in enumerate(graph.edge_list())
        ],
    }


def _elk_node(index: int, graph: rx.PyDiGraph, kids: frozendict[int, tuple[int, ...]], /) -> dict:
    # ports bind under FIXED_ORDER so the layered algorithm seats each on its side/index; a fixed `at` seat escalates the node
    # to FIXED_POS. A `children`-owning node recurses into an enclosed sub-graph, its `sublayout` selecting the container's
    # own `elk.algorithm` when it departs INHERIT.
    data = graph.get_node_data(index)
    base: dict[str, object] = {"id": str(index), "width": _NODE_W, "height": _NODE_H}
    options: dict[str, str] = {}
    if ports := _ports_of(data):
        base["ports"] = [_elk_port(port) for port in ports]
        options["elk.portConstraints"] = "FIXED_POS" if any(port.at is not None for port in ports) else "FIXED_ORDER"
    if nested := kids.get(index):
        base["children"] = [_elk_node(child, graph, kids) for child in nested]
        if (sub := _sublayout_of(data)) is not SubLayout.INHERIT:
            options["elk.algorithm"] = _SUB_ALGORITHM[sub]  # per-container inner engine (recursive compound layout)
    if options:
        base["layoutOptions"] = options
    return base


def _elk_port(port: Port, /) -> dict[str, object]:
    entry: dict[str, object] = {
        "id": port.id,
        "width": port.width,
        "height": port.height,
        "layoutOptions": {"port.side": port.side.value.upper(), "port.index": str(port.index)},
    }
    if port.at is not None:  # node-relative fixed seat; the owning node escalates to FIXED_POS
        entry["x"], entry["y"] = port.at
    return entry


def _nest(graph: rx.PyDiGraph, /) -> tuple[tuple[int, ...], frozendict[int, tuple[int, ...]]]:
    # roots and a parent-index -> child-index map from the optional `parent` node-domain-id, resolved through the id->index lookup;
    # no `parent` anywhere -> all roots, empty map, the flat document.
    lookup = {data["id"]: index for index in graph.node_indices() if isinstance(data := graph.get_node_data(index), dict) and "id" in data}
    kids: dict[int, list[int]] = {}
    roots: list[int] = []
    for index in graph.node_indices():
        parent = lookup.get(_parent_of(graph.get_node_data(index)))
        if parent is not None and parent != index:
            kids.setdefault(parent, []).append(index)
        else:
            roots.append(index)
    return tuple(roots), frozendict({parent: tuple(children) for parent, children in kids.items()})


def _absolutize(nodes: list[dict], origin: Point, coords: LayoutMap, /) -> None:
    # accumulate the container origin down the compound-children tree so each parent-relative ELK `x`/`y` becomes absolute;
    # a flat document recurses once at (0, 0).
    ox, oy = origin
    for node in nodes:
        absolute = (ox + float(node.get("x", 0.0)), oy + float(node.get("y", 0.0)))
        coords[int(node["id"])] = absolute
        if nested := node.get("children"):
            _absolutize(nested, absolute, coords)


def _elk_route(section: dict, /) -> tuple[Point, ...]:
    bends = tuple(_elk_point(point) for point in section.get("bendPoints") or ())
    return (_elk_point(section["startPoint"]), *bends, _elk_point(section["endPoint"]))


def _elk_point(point: dict, /) -> Point:
    return (float(point["x"]), float(point["y"]))


def _constrained_layout(graph: rx.PyDiGraph, rules: tuple[LayoutRule, ...], /) -> LayoutResult:
    # PLAN-ANCHORED seeding: typed `east`/`north` seats under a `strong` stay, every other node the circular spread under `weak`.
    solver, variables = Solver(), {index: (Variable(), Variable()) for index in graph.node_indices()}
    spread = rx.circular_layout(graph)  # robust any-graph base for nodes without plan coordinates
    for index, (var_x, var_y) in variables.items():  # Exemption: kiwisolver Solver is a mutable sink (addConstraint mutates in place, returns None)
        data = graph.get_node_data(index)
        anchored = isinstance(data.get("east"), int | float) and isinstance(data.get("north"), int | float)
        seed_x, seed_y = (_attr(data, "east"), -_attr(data, "north")) if anchored else _pt(spread[index])
        band = "strong" if anchored else "weak"
        solver.addConstraint((var_x == seed_x) | band)
        solver.addConstraint((var_y == seed_y) | band)
    for constraint in (built for rule in rules for built in _rule_constraints(rule, variables)):
        solver.addConstraint(constraint)
    solver.updateVariables()
    return {index: (float(var_x.value()), float(var_y.value())) for index, (var_x, var_y) in variables.items()}, {}


def _rule_constraints(rule: LayoutRule, variables: dict[int, tuple[Variable, Variable]], /) -> tuple[Constraint, ...]:
    match rule:
        case LayoutRule(tag="align", align=(nodes, axis, strength)):
            head = variables[nodes[0]][_AXIS[axis]]
            return tuple((variables[node][_AXIS[axis]] == head) | strength for node in nodes[1:])
        case LayoutRule(tag="separate", separate=(before, after, axis, gap, strength)):
            return ((variables[after][_AXIS[axis]] - variables[before][_AXIS[axis]] >= gap) | strength,)
        case LayoutRule(tag="distribute", distribute=(nodes, axis, gap, strength)):
            return tuple((variables[nxt][_AXIS[axis]] - variables[cur][_AXIS[axis]] == gap) | strength for cur, nxt in pairwise(nodes))
        case LayoutRule(tag="anchor", anchor=(node, (at_x, at_y), strength)):
            var_x, var_y = variables[node]
            return ((var_x == at_x) | strength, (var_y == at_y) | strength)
        case LayoutRule(tag="mirror", mirror=(left, right, axis, about, strength)):
            return ((variables[left][_AXIS[axis]] + variables[right][_AXIS[axis]] == 2.0 * about) | strength,)
        case _ as unreachable:
            assert_never(unreachable)


_ENGINE: Final[Map[HierarchyEngine, Callable[[rx.PyDiGraph, LayeredPolicy], LayoutResult]]] = Map.of_seq([
    (HierarchyEngine.RUSTWORKX, lambda graph, policy: (_layered(graph, policy.direction), {})),
    (HierarchyEngine.GRANDALF, _grandalf_layout),
    (HierarchyEngine.FAST_SUGIYAMA, _sugiyama_layout),
    (HierarchyEngine.ELK, _elk_layout),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
