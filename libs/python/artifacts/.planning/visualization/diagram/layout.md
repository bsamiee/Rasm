# [PY_ARTIFACTS_DIAGRAM_LAYOUT]

The data-driven AEC-diagram coordinate-assignment owner. `DiagramLayout` is ONE owner folding a `data/tabular#GRAPH` adjacency frame into the laid-out `visualization/diagram/glyphset#GLYPHSET` mark sequence for every architectural diagram kind — sun-path, circulation, stacking, program, and site. It builds a `rustworkx` `PyDiGraph` from the adjacency rows (the stable non-recycled integer node index the layout coordinates and the glyph marks both key on), assigns coordinates through the layout policy the `DiagramKind` selects — the `rustworkx` `Pos2DMapping` spring/circular/shell layouts for the force-directed and radial kinds, `grandalf` `SugiyamaLayout` hierarchy and route viewers for stacking/program flow kinds, the `rustworkx` `topological_generations` row as the deterministic no-route fallback, and a deterministic AEC projection (solar-azimuth-to-arc, floor-to-band, parcel grid) for the kinds whose coordinates derive from domain geometry rather than graph topology — then constructs each positioned `DiagramGlyph` with its already-resolved `x`/`y`/`points`/extent and its `GlyphStyle` palette index and layer name. The layout owns coordinates and graph-edge point routes only: it emits no SVG (that is `visualization/diagram/draw#DRAW`'s) and re-owns no graph analysis (the `data/graph#GRAPH` `rustworkx` analysis kernel stays at the data plane; this owner builds a presentation graph from the adjacency feed and runs only layout functions). The synchronous graph build, layout, and glyph emission are one `_render` kernel `_compute` offloads onto `anyio.to_thread.run_sync` under a `CapacityLimiter`, so the native layout work never blocks the event loop. It contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` carrying the kind, the node/edge counts, and the layout-algorithm name.

## [01]-[INDEX]

- [01]-[LAYOUT]: the `DiagramLayout` coordinate-assignment owner folding a `data/tabular#GRAPH` adjacency frame into a `rustworkx` `PyDiGraph` then assigning coordinates through the `LayoutPolicy` the `DiagramKind` selects — `rustworkx` `Pos2DMapping` spring/circular/shell layouts, `grandalf` `SugiyamaLayout` hierarchy and edge-route viewers, the deterministic `rustworkx` `topological_generations` fallback, and the deterministic AEC projections — constructing each positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` keyed on the stable node index, the whole synchronous render offloaded onto `anyio.to_thread.run_sync`, contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` facts.

## [02]-[LAYOUT]

- Owner: `DiagramLayout` the one coordinate-assignment owner discriminating the layout over the `visualization/diagram/glyphset#GLYPHSET` `DiagramKind`; `LayoutPolicy` an `expression.tagged_union` whose every case carries its own typed layout payload — `Force(seed, iterations)` for the `rustworkx` `spring_layout` force-directed kinds, `Radial(scale)` for the `circular_layout`/`shell_layout` radial kinds, `Layered(LayeredPolicy(direction, engine, route))` for both the deterministic `rustworkx` `topological_generations` fallback and the `grandalf` `SugiyamaLayout` hierarchy/routing provider, and `Projected(transform)` for the deterministic AEC domain projections — dispatched by one total `match`/`case` folding the policy onto the positioned node coordinate map and optional edge-route map, never a per-diagram-kind layout method family; the adjacency frame is the `data/tabular#GRAPH` BYODF input the `rustworkx` `PyDiGraph` builds from, the stable integer node index the coordinates and the glyph marks both key on. `LayoutMap` is the resolved `dict[int, Point]` node-index-to-coordinate map every policy arm returns, the one carrier the glyph-construction fold reads; `RouteMap` is the optional `dict[tuple[int, int], tuple[Point, ...]]` edge-waypoint overlay only route-capable layered policies need.
- Cases: `LayoutPolicy` cases — `Force(seed, iterations)` (the `rustworkx` `spring_layout(graph, ...)` force-directed `Pos2DMapping` for circulation-flow kinds where node placement minimizes edge crossing) · `Radial(scale)` (the `rustworkx` `circular_layout(graph, scale=...)`/`shell_layout` radial `Pos2DMapping` for the sun-path and compass kinds where nodes ring a center) · `Layered(policy)` (the one hierarchical/layered case: `policy.engine=GRANDALF` uses `SugiyamaLayout` plus line/spline/orthogonal route points; `policy.engine=RUSTWORKX` uses deterministic `topological_generations` with `TB`/`BT`/`LR`/`RL` direction) · `Projected(transform)` (the deterministic AEC domain projection — solar azimuth/altitude to arc point, floor level to stacking band, site parcel to grid cell — where coordinates derive from domain geometry, not graph topology, the `transform` a pure `int -> Point` over the node attribute) — matched by one total `match`/`case` over the policy `tag`, each arm returning the `LayoutResult`.
- Auto: `_compute` is the thin async seam that offloads the one synchronous `_render` kernel onto `to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)` so native layout and graph build run off the event loop under the module `CapacityLimiter`; `_render` folds graph build, position, and glyph emission into one `(glyphs, ArtifactReceipt)` value. `_as_graph` folds the adjacency frame rows into a `PyDiGraph` — `add_nodes_from` over the node attribute rows yields the stable index, `add_edges_from` over the source/target/weight rows yields the edges, so the presentation graph is built once from the `data/tabular#GRAPH` feed and never re-keyed; `_position` discriminates the `LayoutPolicy` — `Force` folds `rustworkx.spring_layout` to the `Pos2DMapping`, `Radial` folds `circular_layout`/`shell_layout`, `Layered(engine=GRANDALF)` folds `_grandalf_layout` into coordinates plus routed edge waypoints, `Layered(engine=RUSTWORKX)` folds `_layered` into per-generation depth/rank coordinates, `Projected` folds the deterministic `transform` over each node attribute — every arm returning one `LayoutResult`; `_emit` discriminates the `DiagramKind` and constructs the positioned marks — `SUN_PATH` emits `Marker` ticks plus `Edge` arcs plus a `Marker` NORTH compass from the radial map, `CIRCULATION` emits `Node` boxes plus `Edge` connectors with `Marker` arrowheads from the force map, `STACKING` emits `Swimlane` floor bands plus `Node` boxes from the layered map, `PROGRAM` emits `Node` boxes plus `Edge` adjacency connectors from the layered/force map, `SITE` emits `Swimlane` parcels plus `Node` footprints plus `Annotation` callouts from the projected map — each mark threading the `GlyphStyle(layer=..., fill=palette_index, stroke=palette_index)` so the mark binds its named SVG group and its palette color, the edge waypoints the laid-out `points` the layout resolves so an orthogonal or curved route is one point sequence.
- Growth: a new AEC diagram kind is one `DiagramKind` row (in `visualization/diagram/glyphset#GLYPHSET`) plus one `_emit` glyph-fold arm and one `LayoutPolicy` selection, never a new layout method family; a new layered algorithm is one `HierarchyEngine` member plus one `LayeredPolicy` `_position` arm folding the admitted layout member; a new deterministic AEC projection is one `Projected` `transform` callable, never a new case; a new route style is one `EdgeRoute` member plus one `_grandalf_router` arm over the provider route helpers; a new layout knob is one policy-field on the existing case; zero new surface for a new diagram kind.
- Boundary: no SVG emission (that is `visualization/diagram/draw#DRAW`'s — this owner emits the positioned `DiagramGlyph` sequence the draw owner folds to bytes); no graph analysis (the `data/graph#GRAPH` `rustworkx` analysis kernel — centrality, shortest-path, community — stays at the data plane; this owner builds a presentation graph from the `data/tabular#GRAPH` adjacency feed and runs only layout functions, never a second analysis kernel); no ad-hoc color (the `GlyphStyle` indices key the `graphic/color/derive#DERIVE` palette); `rustworkx` owns force/radial/topological data-plane graph algorithms and `grandalf` owns hierarchy layout/routing, both behind one local policy owner; a per-diagram-kind layout method family, a re-keying of the layout coordinate away from the stable `rustworkx` node index, a hand-rolled Sugiyama loop where `grandalf` owns it, a hand-rolled spring loop where `rustworkx` owns it, a synchronous native layout left on the event loop, and a second graph-analysis kernel are the deleted forms — one coordinate-assignment owner, policy-discriminated layout, stable-index-keyed marks, one typed `Diagram` receipt.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
from collections.abc import Callable
from enum import StrEnum
from typing import Literal, assert_never

import polars as pl
import rustworkx as rx
from anyio import CapacityLimiter, to_thread
from builtins import frozendict
from expression import case, tag, tagged_union
from grandalf.graphs import Edge as GrandalfEdge, Graph as GrandalfGraph, Vertex as GrandalfVertex
from grandalf.layouts import SugiyamaLayout, VertexViewer
from grandalf.routing import EdgeViewer, route_with_lines, route_with_splines
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.visualization.diagram.glyphset import DiagramGlyph, DiagramKind, GlyphStyle, MarkerKind


# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type LayoutMap = dict[int, Point]
type RouteMap = dict[tuple[int, int], tuple[Point, ...]]
type LayoutResult = tuple[LayoutMap, RouteMap]
type Projection = Callable[[int, dict[str, object]], Point]
type LayerDirection = Literal["TB", "BT", "LR", "RL"]


class HierarchyEngine(StrEnum):
    RUSTWORKX = "rustworkx"
    GRANDALF = "grandalf"


class EdgeRoute(StrEnum):
    LINES = "lines"
    SPLINES = "splines"
    ORTHOGONAL = "orthogonal"


class LayeredPolicy(Struct, frozen=True):
    direction: LayerDirection = "TB"
    engine: HierarchyEngine = HierarchyEngine.GRANDALF
    route: EdgeRoute = EdgeRoute.SPLINES


@tagged_union(frozen=True)
class LayoutPolicy:
    tag: Literal["force", "radial", "layered", "projected"] = tag()
    force: tuple[int, int] = case()
    radial: float = case()
    layered: LayeredPolicy = case()
    projected: Projection = case()

    @staticmethod
    def Force(seed: int = 0, iterations: int = 100) -> "LayoutPolicy":
        return LayoutPolicy(force=(seed, iterations))

    @staticmethod
    def Radial(scale: float = 1.0) -> "LayoutPolicy":
        return LayoutPolicy(radial=scale)

    @staticmethod
    def Layered(
        direction: LayerDirection = "TB",
        engine: HierarchyEngine = HierarchyEngine.GRANDALF,
        route: EdgeRoute = EdgeRoute.SPLINES,
    ) -> "LayoutPolicy":
        return LayoutPolicy(layered=LayeredPolicy(direction=direction, engine=engine, route=route))

    @staticmethod
    def Projected(transform: Projection) -> "LayoutPolicy":
        return LayoutPolicy(projected=transform)


# --- [TABLES] ---------------------------------------------------------------------------
_KIND_POLICY: frozendict[DiagramKind, LayoutPolicy] = frozendict({
    DiagramKind.SUN_PATH: LayoutPolicy.Radial(),
    DiagramKind.CIRCULATION: LayoutPolicy.Force(),
    DiagramKind.STACKING: LayoutPolicy.Layered(),
    DiagramKind.PROGRAM: LayoutPolicy.Layered(),
    DiagramKind.SITE: LayoutPolicy.Projected(lambda i, attrs: (float(attrs["east"]), float(attrs["north"]))),
})


# --- [SERVICES] -------------------------------------------------------------------------
_LAYOUT_LANES = CapacityLimiter(os.process_cpu_count() or 4)


# --- [MODELS] ---------------------------------------------------------------------------
class DiagramLayout(Struct, frozen=True):
    kind: DiagramKind
    adjacency: pl.DataFrame
    attributes: pl.DataFrame
    policy: LayoutPolicy | None = None

    async def assign(self) -> RuntimeRail[tuple[tuple[DiagramGlyph, ...], ArtifactReceipt]]:
        return await async_boundary(f"diagram.layout.{self.kind}", self._compute)

    async def _compute(self) -> tuple[tuple[DiagramGlyph, ...], ArtifactReceipt]:
        return await to_thread.run_sync(self._render, limiter=_LAYOUT_LANES)

    def _render(self) -> tuple[tuple[DiagramGlyph, ...], ArtifactReceipt]:
        policy = self.policy or _KIND_POLICY[self.kind]
        graph = self._as_graph()
        coords, routes = self._position(graph, policy)
        glyphs = self._emit(graph, coords, routes)
        wire = rx.node_link_json(graph, node_attrs=_wire_attrs, edge_attrs=_wire_attrs).encode()
        key = ContentIdentity.of(f"diagram-{self.kind}", wire + repr(sorted(coords.items())).encode() + repr(sorted(routes.items())).encode())
        receipt = ArtifactReceipt.Diagram(key, self.kind.value, graph.num_nodes(), graph.num_edges(), policy.tag)
        return glyphs, receipt

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
                return {i: (float(pos[i][0]), float(pos[i][1])) for i in graph.node_indices()}, {}
            case LayoutPolicy(tag="radial", radial=scale):
                pos = rx.circular_layout(graph, scale=scale)
                return {i: (float(pos[i][0]), float(pos[i][1])) for i in graph.node_indices()}, {}
            case LayoutPolicy(tag="layered", layered=LayeredPolicy(engine=HierarchyEngine.GRANDALF) as layered):
                return _grandalf_layout(graph, layered)
            case LayoutPolicy(tag="layered", layered=LayeredPolicy(engine=HierarchyEngine.RUSTWORKX, direction=direction)):
                return _layered(graph, direction), {}
            case LayoutPolicy(tag="projected", projected=transform):
                return {i: transform(i, graph.get_node_data(i)) for i in graph.node_indices()}, {}
            case _:
                assert_never(policy)

    def _emit(self, graph: rx.PyDiGraph, coords: LayoutMap, routes: RouteMap) -> tuple[DiagramGlyph, ...]:
        match self.kind:
            case DiagramKind.SUN_PATH:
                ticks = tuple(DiagramGlyph.Marker(*coords[i], MarkerKind.TICK, 0.0, GlyphStyle("sun")) for i in graph.node_indices())
                arcs = tuple(DiagramGlyph.Edge(s, t, _edge_points(routes, coords, s, t), None, GlyphStyle("path", stroke=1)) for s, t in graph.edge_list())
                return (*arcs, *ticks, DiagramGlyph.Marker(0.0, 0.0, MarkerKind.NORTH, 0.0, GlyphStyle("compass")))
            case DiagramKind.CIRCULATION:
                boxes = tuple(DiagramGlyph.Node(i, *coords[i], 40.0, 24.0, _label(graph.get_node_data(i), "label"), GlyphStyle("spaces")) for i in graph.node_indices())
                flows = tuple(DiagramGlyph.Edge(s, t, _edge_points(routes, coords, s, t), None, GlyphStyle("circulation", stroke=2)) for s, t in graph.edge_list())
                return (*boxes, *flows)
            case DiagramKind.STACKING:
                return tuple(DiagramGlyph.Swimlane(i, 0.0, coords[i][1], 200.0, 30.0, _label(graph.get_node_data(i), "floor"), GlyphStyle("floors", fill=i)) for i in graph.node_indices())
            case DiagramKind.PROGRAM:
                boxes = tuple(DiagramGlyph.Node(i, *coords[i], 48.0, 32.0, _label(graph.get_node_data(i), "program"), GlyphStyle("program", fill=i)) for i in graph.node_indices())
                adj = tuple(DiagramGlyph.Edge(s, t, _edge_points(routes, coords, s, t), None, GlyphStyle("adjacency", stroke=3)) for s, t in graph.edge_list())
                return (*boxes, *adj)
            case DiagramKind.SITE:
                parcels = tuple(DiagramGlyph.Swimlane(i, *coords[i], 80.0, 80.0, None, GlyphStyle("parcels", fill=i)) for i in graph.node_indices())
                footprints = tuple(DiagramGlyph.Node(i, *coords[i], 24.0, 24.0, None, GlyphStyle("buildings")) for i in graph.node_indices())
                callouts = tuple(DiagramGlyph.Annotation(*coords[i], text, "middle", GlyphStyle("callouts")) for i in graph.node_indices() if (text := _label(graph.get_node_data(i), "label")) is not None)
                return (*parcels, *footprints, *callouts)
            case _:
                assert_never(self.kind)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _wire_attrs(data: dict[str, object], /) -> dict[str, str]:
    return {name: str(value) for name, value in data.items()}


def _label(data: dict[str, object], key: str, /) -> str | None:
    return None if (value := data.get(key)) is None else str(value)


def _edge_points(routes: RouteMap, coords: LayoutMap, source: int, target: int, /) -> tuple[Point, ...]:
    return routes.get((source, target), (coords[source], coords[target]))


def _grandalf_layout(graph: rx.PyDiGraph, policy: LayeredPolicy, /) -> LayoutResult:
    vertices = {i: GrandalfVertex(i) for i in graph.node_indices()}
    if not vertices:
        return {}, {}
    edges = tuple(GrandalfEdge(vertices[source], vertices[target]) for source, target in graph.edge_list())
    component = GrandalfGraph(list(vertices.values()), list(edges)).C[0]
    for vertex in vertices.values():
        vertex.view = VertexViewer(48, 32)
    for edge in edges:
        edge.view = EdgeViewer()
    layout = SugiyamaLayout(component)
    layout.route_edge = _grandalf_router(policy.route)
    layout.init_all()
    layout.draw()
    coords = {index: _viewer_point(vertices[index].view.xy) for index in graph.node_indices()}
    routes = {
        (int(edge.v[0].data), int(edge.v[1].data)): _edge_route(edge)
        for edge in edges
    }
    return coords, routes


def _grandalf_router(route: EdgeRoute, /) -> Callable[[GrandalfEdge, list[object]], None]:
    match route:
        case EdgeRoute.LINES | EdgeRoute.ORTHOGONAL:
            return route_with_lines
        case EdgeRoute.SPLINES:
            return route_with_splines
        case _:
            assert_never(route)


def _edge_route(edge: GrandalfEdge, /) -> tuple[Point, ...]:
    return tuple(_viewer_point(point) for point in edge.view._pts)


def _viewer_point(point: object, /) -> Point:
    x, y = point
    return float(x), float(y)


def _layered(graph: rx.PyDiGraph, direction: LayerDirection, gap: float = 60.0) -> LayoutMap:
    return {
        node: _layer_point(depth, rank, direction, gap)
        for depth, layer in enumerate(rx.topological_generations(graph))
        for rank, node in enumerate(layer)
    }


def _layer_point(depth: int, rank: int, direction: LayerDirection, gap: float, /) -> Point:
    match direction:
        case "TB":
            return rank * gap, depth * gap
        case "BT":
            return rank * gap, -depth * gap
        case "LR":
            return depth * gap, rank * gap
        case "RL":
            return -depth * gap, rank * gap
        case _:
            assert_never(direction)
```

`DiagramLayout.assign` builds the presentation `PyDiGraph` from the `data/tabular#GRAPH` adjacency frame, assigns coordinates through the `LayoutPolicy` the `DiagramKind` selects, and constructs each positioned `DiagramGlyph` keyed on the stable `rustworkx` node index. The `_position` fold is the one layout-algorithm dispatch — `rustworkx` `spring_layout`/`circular_layout` for the force and radial kinds, the `LayeredPolicy(engine=GRANDALF)` arm over `grandalf` `SugiyamaLayout` plus `route_with_lines`/`route_with_splines`, the `LayeredPolicy(engine=RUSTWORKX)` arm over deterministic `rustworkx` `topological_generations`, and the deterministic `transform` for the projected kind — every arm returning the `LayoutResult`, and `_emit` is the one `DiagramKind`-keyed glyph fold threading the `graphic/color/derive#DERIVE` palette index and the per-mark layer name into each mark. The whole synchronous build is one `_render` kernel `_compute` offloads onto `anyio.to_thread.run_sync` under the module `CapacityLimiter`, so the native graph layout never blocks the loop. The content key is the `rustworkx` `node_link_json` wire — node and edge payloads serialized through `_wire_attrs` so two graphs that differ only in a node label or an edge weight (both of which feed the emitted glyphs) key distinctly, since the bare `node_link_json` emits a null `data` field and would collapse them — joined to the resolved coordinate map and routed edge points, so two layouts of one graph under distinct `LayoutPolicy` values also key distinctly and a diagram is content-addressable on its full laid-out form (topology, attributes, coordinates, and routes) rather than its bare topology.

## [03]-[RESEARCH]

- [GRANDALF_HIERARCHY] [RESOLVED]: `grandalf` is the admitted hierarchy/routing provider for `DiagramLayout`, not a second diagram owner. The local surface imports `Vertex`/`Edge`/`Graph` under aliases, assigns `VertexViewer` from `grandalf.layouts` and `EdgeViewer` from `grandalf.routing`, binds `layout.route_edge = route_with_lines | route_with_splines` before `SugiyamaLayout(component).init_all().draw()`, and reads the routed path from `edge.view._pts` into the local `LayoutResult`; `rustworkx` remains the presentation graph builder, force/radial layout provider, topological fallback, and content-key wire serializer. Raw grandalf vertices, edges, components, viewers, and layout objects never cross into glyph or receipt shapes.
- [DIAGRAM_RECEIPT_FACTS] [RESOLVED]: `DiagramLayout._compute` projects `ArtifactReceipt.Diagram(key, self.kind.value, graph.num_nodes(), graph.num_edges(), policy.tag)` against the settled `core/receipt#RECEIPT` `Diagram(key, kind, nodes, edges, algorithm)` case (`diagram: tuple[ContentKey, str, int, int, str]`) whose `_facts` arm projects `{"key", "kind", "nodes", "edges", "algorithm"}` — all flat scalars, the kind the `DiagramKind` value, the node/edge counts the built-graph extents, the algorithm the matched `LayoutPolicy.tag`. The `Diagram` case is the diagram-engine contribution to the shared `core/receipt#RECEIPT` `ArtifactReceipt` family — a case on the receipt owner, never a parallel diagram-receipt rail and never a producer value object the receipt owner imports — shared with the `visualization/diagram/draw#DRAW` SVG-emission owner which contributes the same `Diagram` case off its node/edge glyph tallies (the layered element/layer/byte evidence staying the `export/layered#LAYERED` `Preview`/`Egress` contribution); the `DiagramLayout.assign -> RuntimeRail[...]` rail shape mirrors the settled `visualization/chart/export#EXPORT` `ChartExport.render -> RuntimeRail[ArtifactReceipt]` pattern. The `core/receipt#RECEIPT` owner carries the `Diagram` case, so the receipt call is settled fence code.
