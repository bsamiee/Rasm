# [PY_ARTIFACTS_DIAGRAM_LAYOUT]

The data-driven diagram coordinate-assignment owner. `DiagramLayout` is ONE owner folding a `data/tabular#GRAPH` adjacency frame into the laid-out `visualization/diagram/glyphset#GLYPHSET` mark sequence for every `DiagramKind` — the AEC sun-path, circulation, stacking, program, and site kinds plus the general/technical node-link, flowchart, entity-relation, Sankey, and section-callout kinds, one `_emit` glyph-fold arm each threading the `NodeShape`/`Port`/`weight`/`EndCap` topology axes the kind demands. It builds a `rustworkx` `PyDiGraph` from the adjacency rows (the stable non-recycled integer node index the coordinates and the glyph marks both key on), assigns coordinates through the `LayoutPolicy` the `DiagramKind` selects — a closed five-case `tagged_union` over `Force` (`rustworkx` `spring_layout`), `Radial` (`circular_layout`/`shell_layout` discriminated by `RingMode`), `Layered` (one `LayeredPolicy` whose `HierarchyEngine` picks the placement provider: `RUSTWORKX` `topological_generations`, `GRANDALF` `SugiyamaLayout`, `FAST_SUGIYAMA` native-Rust Sugiyama, or `ELK` ports/nesting/orthogonal), `Projected` (the deterministic AEC domain transforms — solar-azimuth-to-arc, floor-to-band, parcel-grid — over a closed `ProjectionKind` family), and `Constrained` (a `kiwisolver` Cassowary solve over an alignment/separation/distribution/anchor/mirror `LayoutRule` set, seeded PLAN-ANCHORED: a node carrying typed `east`/`north` plan columns seats at its real plan position under a `strong` stay, so a circulation space or a section-callout frame sits where the building puts it) — then constructs each positioned `DiagramGlyph` with its resolved `x`/`y`/`points`/`ring`/extent and its `GlyphStyle` palette index and layer name. The V15 AREA LAW rules the three area kinds: `STACKING` floor bands segment into per-program `Node` segments whose widths are proportional to the program `area` columns, `PROGRAM` boxes size by the square root of their `area` so drawn footprint tracks real program area, and `SITE` parcels and building footprints render as true `Area` polygons from typed `ring`/`footprint` vertex columns with the shoelace-derived magnitude on the mark — never a fixed-dimension rectangle wearing an area label. `ELK` is the sole owner of true `EdgeRoute.ORTHOGONAL` routing (its layered algorithm emits native `sections[].bendPoints`); `LayoutPolicy.Layered` rewires an `ORTHOGONAL` request to the `ELK` engine so the route never degrades to the straight-line alias a Sugiyama engine cannot escape. The layout owns coordinates and graph-edge point routes only: it emits no SVG (that is `visualization/diagram/draw#DRAW`'s) and re-owns no graph analysis (the `data/graph#GRAPH` `rustworkx` analysis kernel stays at the data plane; this owner builds a presentation graph from the adjacency feed and runs only layout functions). The synchronous graph build, layout, and glyph emission are one `_render` kernel `_compute` offloads onto the runtime thread lane, so the native layout work never blocks the event loop. The diagram receipt is the draw terminal's — layout returns positioned glyph substrate only.

## [01]-[INDEX]

- [01]-[LAYOUT]: the `DiagramLayout` coordinate-assignment owner folding a `data/tabular#GRAPH` adjacency frame into a `rustworkx` `PyDiGraph` then assigning coordinates through the `LayoutPolicy` the `DiagramKind` selects — `Force` `spring_layout`, `Radial` `circular_layout`/`shell_layout`, `Layered` over the four-member `HierarchyEngine` (`topological_generations`/`grandalf`/`fast-sugiyama`/`ELK`), `Projected` over the closed `ProjectionKind` AEC domain transforms, and `Constrained` over a `kiwisolver` `Solver` with plan-anchored seeding — emitting the V15 area-law marks (segmented stacking bands, area-sized program boxes, true `Area` site polygons) and the composed solar furniture, constructing each positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` keyed on the stable node index, the whole synchronous render offloaded onto the runtime thread lane; the diagram receipt is the draw terminal's.

## [02]-[LAYOUT]

- Owner: `DiagramLayout` the one coordinate-assignment owner discriminating the layout over the `visualization/diagram/glyphset#GLYPHSET` `DiagramKind`; `LayoutPolicy` an `expression.tagged_union` whose every case carries its own typed layout payload — `Force(seed, iterations)` for the `rustworkx` `spring_layout` force-directed kinds, `Radial(scale, mode)` for the `circular_layout` single-ring and `shell_layout` concentric-ring radial kinds (`RingMode` discriminated, the `nlist` shells partitioned by the node `ring` attribute), `Layered(LayeredPolicy(direction, engine, route))` for the four hierarchical placement providers, `Projected(kind, scale)` for the deterministic AEC domain projections over the closed `ProjectionKind` vocabulary, and `Constrained(*rules)` for the `kiwisolver` constraint solve over a `LayoutRule` set — dispatched by one total `match`/`case` folding the policy onto the positioned node coordinate map and optional edge-route map, never a per-diagram-kind layout method family; the adjacency frame is the `data/tabular#GRAPH` BYODF input the `rustworkx` `PyDiGraph` builds from, the stable integer node index the coordinates and the glyph marks both key on, and plan geometry arrives as TYPED COLUMNS on that frame — `east`/`north` plan coordinates, per-program `area`, `ring`/`footprint` polygon vertex lists, `sublayout` container engines, `source_cardinality`/`target_cardinality` edge caps, `latitude`/`longitude`/`year` site rows — computed upstream, never an untyped attribute bag re-derived here. `LayoutMap` is the resolved `dict[int, Point]` node-index-to-coordinate map every policy arm returns, the one carrier the glyph-construction fold reads; `RouteMap` is the optional `dict[tuple[int, int], tuple[Point, ...]]` edge-waypoint overlay the route-capable engines (`ELK` orthogonal `bendPoints`, `fast-sugiyama` dummy-vertex chains, `grandalf` `route_with_*` polylines) fill.
- Solar: the `SUN_PATH` arm composes `visualization/diagram/solar#SOLAR` — `project` positions each mark through the owner's declared hemisphere projection, and `furniture` supplies the horizon/ring/compass/date-arc/hour-line backdrop as `Furnishing` rows the arm lowers to `DiagramGlyph.Fragment` marks on the "furniture" layer when the frame carries `latitude`/`longitude`/`year` site columns — so sun geometry is computed astronomy under one projection law shared between the positioned marks and their backdrop, and a caller-supplied-angles-only frame degrades to bare marks, never to fake furniture.
- Cases: `LayoutPolicy` cases — `Force(seed, iterations)` (the `rustworkx` `spring_layout(graph, seed=, num_iter=)` force-directed `Pos2DMapping` for topology-only kinds where node placement minimizes edge crossing) · `Radial(scale, mode)` (`RingMode.CIRCULAR` -> `circular_layout(graph, scale=)`, `RingMode.SHELL` -> `shell_layout(graph, nlist=_shells(graph), scale=)` concentric rings for compass and multi-ring cycle diagrams, the `_RING` table the single dispatch) · `Layered(LayeredPolicy)` (the one hierarchical case whose `engine` selects the provider via the `_ENGINE` table: `RUSTWORKX` deterministic `topological_generations` with `TB`/`BT`/`LR`/`RL` direction, `GRANDALF` `SugiyamaLayout` plus `route_with_lines`/`route_with_splines`, `FAST_SUGIYAMA` native-Rust `from_edges` placement plus dummy-vertex routes, `ELK` `ELK().layout` ports/nesting/orthogonal placement plus native `sections` bend routes) · `Projected(kind, scale)` (the deterministic AEC domain projection over `ProjectionKind` — `SOLAR_ARC` azimuth/altitude to a stereographic sun-path point, `FLOOR_BAND` level to a stacking band, `PARCEL_GRID` east/north to a site cell — the `PROJECTION` table the single dispatch, each transform reading its typed node attributes through `_attr`, never a fragile `attrs[key]` access) · `Constrained(*rules)` (the `kiwisolver` `Solver` over an `x`/`y` `Variable` pair per stable node index, PLAN-ANCHORED seeding — a node carrying typed `east`/`north` columns seats at its plan position under a `strong` stay, every other node takes the `circular_layout` spread under a `weak` stay — the `LayoutRule` family folded to `Constraint`s and solved, `updateVariables()` read back) — matched by one total `match`/`case` over the policy `tag`, each arm returning the `LayoutResult`. `CIRCULATION` and `SECTION_CALLOUT` select `Constrained()` in `_KIND_POLICY`: their marks are plan-anchored building geometry, and the deleted `Force()` default that scattered rooms by graph topology is the rejected form.
- Auto: `_compute` is the thin async seam that offloads the one synchronous `_render` kernel onto the runtime thread lane so every native placement — the GIL-releasing `rustworkx` Rust core, the `fast-sugiyama` PyO3 pass, the pure-Python `pyelk`/`kiwisolver` solve, the `grandalf` draw — runs off the event loop; `_render` folds graph build, position, and glyph emission into the positioned glyph tuple. `_as_graph` folds the adjacency frame rows into a `PyDiGraph` once — `add_nodes_from` over the node attribute rows yields the stable index, `add_edges_from` over the source/target/weight rows yields the edges, so the presentation graph is built once from the `data/tabular#GRAPH` feed and never re-keyed. `_position` is the total `LayoutPolicy` dispatch; the layered arm reads `_ENGINE[layered.engine]`, the radial arm `_RING[mode]`, the projected arm `PROJECTION[kind]`, so a new provider, ring mode, or projection is one table row. `_sugiyama_layout` maps `graph.edge_list()` through `from_edges(..., dummy_vertices=True)`, folds the `Layouts.dot_layout()` positions into the `LayoutMap` (filtering the synthetic dummy ids, stranding the isolated nodes `from_edges` drops), and traces each long edge's dummy chain into a `RouteMap` polyline. `_elk_layout` lowers the graph to an ELK-JSON document — `_nest` partitioning the node set into roots and a parent-index->child-index map from the `parent` attribute so a `parent`-bearing node lands as a compound `children` sub-graph under `INCLUDE_CHILDREN`, `_elk_node` recursing it with the node's `sublayout` column mapped through `_SUB_ALGORITHM` onto the per-container `elk.algorithm` override, and each `Port` lowered by `_elk_port` with its `width`/`height` and its fixed `at` seat escalating the node to `elk.portConstraints=FIXED_POS` — runs one `ELK().layout` call, and folds the single result (never a second render) into the `LayoutMap` through `_absolutize` (accumulating each container's origin down the compound tree so the parent-relative ELK `x`/`y` become absolute, identical to a flat `collect_nodes` read when un-nested) and each edge's `[startPoint, *bendPoints, endPoint]` from `collect_edges` into the `RouteMap`. `_emit` discriminates the `DiagramKind` and constructs the positioned marks threading the `GlyphStyle(layer=, fill=palette_index, stroke=palette_index)` so each mark binds its named SVG group and its palette color, the edge waypoints the laid-out `points` the layout resolved, and the directed kinds passing typed `EndCap` caps — the circulation/flowchart/node-link arrowhead explicit, the ER `source_cardinality`/`target_cardinality` columns mapped through `_CARDINALITY` onto crow's-foot caps, never a bare-string cardinality label the draw arms reparse.
- Growth: a new AEC diagram kind is one `DiagramKind` row (in `visualization/diagram/glyphset#GLYPHSET`) plus one `_emit` glyph-fold arm and one `_KIND_POLICY` selection, never a new layout method family; a new layered placement engine is one `HierarchyEngine` member plus one `_ENGINE` row folding the admitted provider, never a fifth `LayeredPolicy` case; a new deterministic AEC projection is one `ProjectionKind` member plus one `_solar_arc`-shaped transform plus one `PROJECTION` row; a new ring mode is one `RingMode` member plus one `_RING` row; a new constraint kind is one `LayoutRule` case plus one `_rule_constraints` arm; a new container engine is one `_SUB_ALGORITHM` row over the registered ELK providers; a new cardinality token is one `_CARDINALITY` row; a new route style rides the engine that owns it (`ORTHOGONAL` the `ELK` bend geometry, `SPLINES` the `grandalf` curve); a new layout knob is one policy-field on the existing case; zero new surface for a new diagram kind.
- Boundary: no SVG emission (that is `visualization/diagram/draw#DRAW`'s — this owner emits the positioned `DiagramGlyph` sequence the draw owner folds to bytes); no graph analysis (the `data/graph#GRAPH` `rustworkx` analysis kernel — centrality, shortest-path, community — stays at the data plane; this owner builds a presentation graph from the `data/tabular#GRAPH` adjacency feed and runs only layout functions, never a second analysis kernel); no ephemeris computation (the `visualization/diagram/solar#SOLAR` owner solves sun positions and generates furniture; this owner composes `project` and `furniture` and lowers the rows to marks); no ad-hoc color (the `GlyphStyle` indices key the `graphic/color/derive#DERIVE` palette); `rustworkx` owns force/radial/topological data-plane graph algorithms, `grandalf`/`fast-sugiyama` own Sugiyama placement, `pyelk` owns ports/nesting/orthogonal placement and routing, `kiwisolver` owns the Cassowary constraint solve, all behind one local `LayoutPolicy` owner; a per-diagram-kind layout method family, a re-keying of the layout coordinate away from the stable `rustworkx` node index, a hand-rolled Sugiyama or spring or constraint loop where a provider owns it, an `EdgeRoute.ORTHOGONAL` aliased to a straight-line router, a synchronous native layout left on the event loop, a `Force()` default on a plan-anchored kind, a fixed-dimension rectangle standing in for a measured area, and a second graph-analysis kernel are the deleted forms — one coordinate-assignment owner, policy-discriminated layout, stable-index-keyed marks, area-true geometry.

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
    GRANDALF = "grandalf"  # parity oracle + spline routes, superseded-pending-removal
    FAST_SUGIYAMA = "fast-sugiyama"  # native-Rust Sugiyama placement, supersedes grandalf placement
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
    # the constraint vocabulary the Constrained policy folds onto the kiwisolver Solver; each case
    # names the stable node indices and the strength band the rule enters at.
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
        # orthogonal routing is ELK's native capability; force the engine so ORTHOGONAL never
        # degrades to a straight-line alias on a Sugiyama engine that cannot route around boxes.
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
    (DiagramKind.CIRCULATION, LayoutPolicy.Constrained()),  # plan-anchored: rooms sit where the building puts them
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
        # coordinate SUBSTRATE for draw — layout mints NO receipt (draw is the sole diagram producer);
        # the positioned glyph sequence is data the terminal's own node consumes.
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
                # the V15 area law: floor bands sized by their program total, segmented into per-program
                # Node segments whose widths are proportional to the typed `area` columns.
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
                # the V15 area law: parcels and footprints are TRUE polygons from typed `ring`/`footprint`
                # vertex columns; the shoelace magnitude rides the mark, so drawn area is measured area.
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
    # optional node-attribute read for the domain projections; a missing or non-numeric column
    # folds to 0.0 rather than a fragile KeyError, so a partial adjacency frame never faults the layout.
    return float(value) if isinstance(value := data.get(key), int | float) else 0.0


def _shape_of(data: dict[str, object], /) -> NodeShape:
    # flowchart/ER node silhouette read from the optional `shape` attribute; absent -> RECTANGLE.
    return _NODE_SHAPES.try_find(value).default_value(NodeShape.RECTANGLE) if isinstance(value := data.get("shape"), str) else NodeShape.RECTANGLE


def _sublayout_of(data: dict[str, object], /) -> SubLayout:
    # per-container inner-engine override read from the optional `sublayout` attribute; absent -> INHERIT.
    return _sub_member(value) if isinstance(value := data.get("sublayout"), str) else SubLayout.INHERIT


def _sub_member(value: str, /) -> SubLayout:
    return SubLayout(value) if value in {member.value for member in SubLayout} else SubLayout.INHERIT


def _cap_of(data: dict[str, object], key: str, /) -> EndCap:
    # typed ER cardinality read from the optional edge column; absent or foreign -> NONE.
    return _CARDINALITY.try_find(value).default_value(EndCap.NONE) if isinstance(value := data.get(key), str) else EndCap.NONE


def _ports_of(data: dict[str, object], /) -> tuple[Port, ...]:
    # typed connection ports read from the optional `ports` attribute (an id, (id, side), or
    # (id, side, (x, y)) sequence); side-local index is the list position, so an ER field-row /
    # flowchart record-slot seats on a boundary, and an explicit coordinate seats FIXED_POS.
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
    # Sankey ribbon magnitude read from the edge `weight` attribute through the robust accessor.
    return _attr(graph.get_edge_data(source, target), "weight")


def _parent_of(data: dict[str, object], /) -> object:
    # the compound-nesting parent read from the optional `parent` node-domain-id attribute; absent -> None (a root).
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
    # a floor band's plan-area total over its program children — the band length derives from real area.
    return sum(_attr(graph.get_node_data(child), "area") for child in children)


def _segments(graph: rx.PyDiGraph, floor: int, children: tuple[int, ...], y: float, /):
    # per-program band segments: cumulative x offsets proportional to the typed `area` columns.
    x = 0.0
    for child in children:  # Exemption: the cumulative x offset is an ordered running fold the generator carries
        width = max(1.0, _attr(graph.get_node_data(child), "area") * _AREA_UNIT)
        yield DiagramGlyph.Node(child, x, y, width, _BAND_H, _label(graph.get_node_data(child), "program"), GlyphStyle("program", fill=child), parent=floor)
        x += width


def _program_side(data: dict[str, object], /) -> float:
    # area-true program box: drawn side tracks sqrt(area) so drawn footprint is proportional to program area.
    area = _attr(data, "area")
    return sqrt(area) * _AREA_SIDE if area > 0.0 else _NODE_W


def _polygon(data: dict[str, object], key: str, scale: float, index: int, label: str | None, layer: str, /) -> DiagramGlyph | None:
    # typed plan-polygon ingress: the vertex column carries (east, north) pairs computed upstream; the
    # same plan transform as PARCEL_GRID, the shoelace magnitude measured on the raw plan ring.
    raw = data.get(key)
    if not isinstance(raw, list | tuple) or len(raw) < 3:
        return None
    ring = tuple((float(east) * scale, -float(north) * scale) for east, north in raw)
    return DiagramGlyph.Area(index, ring, label, GlyphStyle(layer, fill=index), _shoelace(raw))


def _shoelace(raw: "list | tuple", /) -> float:
    pairs = tuple((float(east), float(north)) for east, north in raw)
    return abs(sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in pairwise((*pairs, pairs[0])))) / 2.0


def _sun_furniture(graph: rx.PyDiGraph, radius: float, /) -> tuple[DiagramGlyph, ...]:
    # composes the solar owner's furniture when the frame carries site columns; an angles-only frame
    # draws bare marks — the backdrop is real astronomy or absent, never fabricated.
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
    # composes the solar owner's projection law — the SAME fold that positions the chart furniture,
    # so a positioned mark and its date-arc/hour-line backdrop cannot disagree.
    return project(_attr(data, "azimuth"), _attr(data, "altitude"), SolarProjection.STEREOGRAPHIC, scale)


def _floor_band(data: dict[str, object], scale: float, /) -> Point:
    return (0.0, _attr(data, "level") * scale)  # model-space stacking band; the sheet viewport flips vertical


def _parcel_grid(data: dict[str, object], scale: float, /) -> Point:
    return (_attr(data, "east") * scale, -_attr(data, "north") * scale)


def _shells(graph: rx.PyDiGraph, /) -> list[list[int]] | None:
    # concentric shells partitioned by the node `ring` attribute; groupby needs the ring-sorted stream,
    # so band by (ring, index) then group on the ring head. no partition -> None -> shell_layout single ring.
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
        # grandalf ships no orthogonal router; ORTHOGONAL is rewired to the ELK engine at LayoutPolicy.Layered,
        # so this arm only ever binds a curve — the deleted route_with_lines alias for ORTHOGONAL was the straight-line lie.
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
    _absolutize(result.get("children") or [], (0.0, 0.0), coords)  # parent-relative ELK x/y -> absolute over the compound-children tree
    routes: RouteMap = {
        (int(edge["sources"][0]), int(edge["targets"][0])): _elk_route(section)
        for edge in collect_edges(result)
        if "startPoint" in (section := (edge.get("sections") or ({},))[0])
    }
    return coords, routes


def _elk_document(graph: rx.PyDiGraph, policy: LayeredPolicy, /) -> dict:
    # the foreign ELK-JSON contract, built structurally (never an f-string splice into markup); node id is the
    # stringified stable index, edge endpoints its source/target, so positions read back on the same key. A `parent`-
    # bearing node nests under its container (the compound-children hierarchy ELK sizes to enclose); INCLUDE_CHILDREN
    # then lets a cross-container edge route, every edge kept at the root so its section geometry stays root-absolute.
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
    # a leaf or compound node: `ports` bind under FIXED_ORDER so the layered algorithm seats each port on its
    # resolved side/index (the ER field-port / flowchart record-slot boundary the draw owner marks) — a port
    # carrying a fixed `at` seat escalates the node to FIXED_POS with the coordinate on the port shape — and a
    # node owning `children` (the `parent`-axis compound-nesting) recurses into an enclosed sub-graph ELK sizes
    # to contain, its `sublayout` column selecting the container's own `elk.algorithm` when it departs INHERIT.
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
    # partition the node set into roots and a parent-index -> child-index map from the optional `parent` node-domain-id
    # attribute resolved through the id->index lookup; a node whose `parent` names a sibling nests under it (compound
    # children), every other node is a root. No `parent` attribute anywhere -> all roots, empty map, the flat document.
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
    # fold each ELK-positioned node's parent-relative `x`/`y` into an ABSOLUTE coordinate by accumulating the container
    # origin down the compound-children tree — a flat document recurses once at (0, 0), identical to the pre-nesting
    # `collect_nodes` read; a nested document offsets each child by its container's absolute corner.
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
    # PLAN-ANCHORED seeding: a node carrying typed `east`/`north` plan columns seats at its real plan
    # position under a `strong` stay; every other node takes the circular spread under a `weak` stay.
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

`DiagramLayout.assign` builds the presentation `PyDiGraph` from the `data/tabular#GRAPH` adjacency frame, assigns coordinates through the `LayoutPolicy` the `DiagramKind` selects, and constructs each positioned `DiagramGlyph` keyed on the stable `rustworkx` node index. The `_position` fold is the one layout dispatch over the five `LayoutPolicy` cases, its layered arm reading the `_ENGINE` table so `RUSTWORKX` `topological_generations`, `GRANDALF` `SugiyamaLayout`, `FAST_SUGIYAMA` `from_edges`, and `ELK` `ELK().layout` each own one arm body while the total `match` stays the closed dispatch. `FAST_SUGIYAMA` is the default layered placement (native-Rust, superseding the `GRANDALF` core); its `_sugiyama_layout` filters the synthetic dummy ids out of the `LayoutMap`, strands the isolated nodes `from_edges` drops onto a trailing rank, and traces each long edge's dummy chain into a `RouteMap` polyline. `ELK` is the sole owner of `EdgeRoute.ORTHOGONAL`: its layered algorithm emits native `sections[].bendPoints`, read off one `ELK().layout` result (never a second render) as `[startPoint, *bendPoints, endPoint]`, and `LayoutPolicy.Layered` rewires an `ORTHOGONAL` request onto the `ELK` engine so a Sugiyama engine never aliases it to a straight line. `ELK` is likewise the sole owner of the `parent`-axis compound-children nesting the `glyphset#GLYPHSET` `Node`/`Swimlane` declare: `_nest` reads the `parent` node-domain-id attribute, `_elk_document` nests each child under its container and sets `elk.hierarchyHandling=INCLUDE_CHILDREN`, ELK sizes each container to enclose its sub-graph, a container's `sublayout` column selects its own inner `elk.algorithm` through `_SUB_ALGORITHM`, a port's fixed `at` seat escalates its node to `FIXED_POS`, and `_absolutize` lifts the parent-relative positions to absolute so the enclosed marks and the framing container both key on the one stable node index. `Projected` folds the closed `ProjectionKind` AEC transforms — `SOLAR_ARC` reading azimuth/altitude through the solar owner's own `project` law, `FLOOR_BAND` reading level, `PARCEL_GRID` reading east/north through the robust `_attr` accessor — so the sun-path, stacking, and site kinds derive coordinates from their real domain geometry rather than graph topology; the `SUN_PATH` arm additionally lowers the composed `furniture` rows to `Fragment` marks so the horizon, altitude rings, date arcs, and hour lines ride the glyph stream under the same projection as the positioned marks. The V15 area law is the `_emit` fold for the three area kinds: `STACKING` reads the `_nest` parent map and the `area` columns to size each floor band and its cumulative program segments, `PROGRAM` sizes each box by `sqrt(area)` so drawn footprint is proportional to real program area, and `SITE` lowers `ring`/`footprint` vertex columns to true `Area` polygons carrying their shoelace magnitude. `Constrained` solves an alignment/separation/distribution/anchor/mirror `LayoutRule` set on a `kiwisolver` `Solver` keyed on the same stable node index with plan-anchored seeding — the `CIRCULATION` and `SECTION_CALLOUT` kinds select it in `_KIND_POLICY` so building geometry, not graph topology, places their marks — `required` bands the hard rules and `weak`/`medium` the aesthetic ones, reading `var.value()` back into the `LayoutMap`. The whole synchronous build is one `_render` kernel `_compute` offloads onto the runtime thread lane, so every native placement runs off the loop. The content key is the `rustworkx` `node_link_json` wire — node and edge payloads serialized through `_wire_attrs` so two graphs that differ only in a node label or an edge weight key distinctly, since the bare `node_link_json` emits a null `data` field that collapses them — joined to the resolved coordinate and route maps and the resolved algorithm scalar, so two layouts of one graph under distinct engines or policies key distinctly and a diagram is content-addressable on its full laid-out form.
