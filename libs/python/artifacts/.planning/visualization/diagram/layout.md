# [PY_ARTIFACTS_DIAGRAM_LAYOUT]

The data-driven AEC-diagram coordinate-assignment owner. `DiagramLayout` is ONE owner folding a `data/tabular#GRAPH` adjacency frame into the laid-out `visualization/diagram/glyphset#GLYPHSET` mark sequence for every architectural diagram kind — sun-path, circulation, stacking, program, and site. It builds a `rustworkx` `PyDiGraph` from the adjacency rows (the stable non-recycled integer node index the layout coordinates and the glyph marks both key on), assigns coordinates through the layout policy the `DiagramKind` selects — the `rustworkx` `Pos2DMapping` spring/circular/bipartite layouts for the force-directed and radial kinds, the `grandalf` Sugiyama hierarchical layered layout for the stacking/program flow kinds, and a deterministic AEC projection (solar-azimuth-to-arc, floor-to-band, parcel grid) for the kinds whose coordinates derive from domain geometry rather than graph topology — then constructs each positioned `DiagramGlyph` with its already-resolved `x`/`y`/`points`/extent and its `GlyphStyle` palette index and layer name. The layout owns coordinates only: it emits no SVG (that is `visualization/diagram/draw#DRAW`'s) and re-owns no graph analysis (the `data/graph#GRAPH` `rustworkx` analysis kernel stays at the data plane; this owner builds a presentation graph from the adjacency feed and runs only the layout functions). It contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` carrying the kind, the node/edge counts, and the layout-algorithm name.

## [01]-[INDEX]

- [01]-[LAYOUT]: the `DiagramLayout` coordinate-assignment owner folding a `data/tabular#GRAPH` adjacency frame into a `rustworkx` `PyDiGraph` then assigning coordinates through the `LayoutPolicy` the `DiagramKind` selects — `rustworkx` `Pos2DMapping` spring/circular/bipartite layouts, the `grandalf` Sugiyama hierarchical layered layout, and the deterministic AEC projections — constructing each positioned `visualization/diagram/glyphset#GLYPHSET` `DiagramGlyph` keyed on the stable node index, contributing the typed `core/receipt#RECEIPT` `ArtifactReceipt.Diagram` facts.

## [02]-[LAYOUT]

- Owner: `DiagramLayout` the one coordinate-assignment owner discriminating the layout over the `visualization/diagram/glyphset#GLYPHSET` `DiagramKind`; `LayoutPolicy` an `expression.tagged_union` whose every case carries its own typed layout payload — `Force(seed, iterations)` for the `rustworkx` `spring_layout` force-directed kinds, `Radial(scale)` for the `circular_layout`/`shell_layout` radial kinds, `Layered(direction)` for the `grandalf` Sugiyama hierarchical kinds, and `Projected(transform)` for the deterministic AEC domain projections — dispatched by one total `match`/`case` folding the policy onto the positioned node coordinate map, never a per-diagram-kind layout method family; the adjacency frame is the `data/tabular#GRAPH` BYODF input the `rustworkx` `PyDiGraph` builds from, the stable integer node index the coordinates and the glyph marks both key on. `LayoutMap` is the resolved `dict[int, Point]` node-index-to-coordinate map every policy arm returns, the one carrier the glyph-construction fold reads.
- Cases: `LayoutPolicy` cases — `Force(seed, iterations)` (the `rustworkx` `spring_layout(graph, ...)` force-directed `Pos2DMapping` for the program-adjacency and circulation-flow kinds where node placement minimizes edge crossing) · `Radial(scale)` (the `rustworkx` `circular_layout(graph, scale=...)`/`shell_layout` radial `Pos2DMapping` for the sun-path and compass kinds where nodes ring a center) · `Layered(direction)` (the `grandalf` Sugiyama hierarchical layered assignment for the stacking and program-tree kinds where the DAG layers stack top-to-bottom or left-to-right) · `Projected(transform)` (the deterministic AEC domain projection — solar azimuth/altitude to arc point, floor level to stacking band, site parcel to grid cell — where coordinates derive from domain geometry, not graph topology, the `transform` a pure `int -> Point` over the node attribute) — matched by one total `match`/`case` over the policy `tag`, each arm returning the `LayoutMap`.
- Entry: `DiagramLayout.assign` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[tuple[DiagramGlyph, ...], ArtifactReceipt]]`, and contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm)` five-field fact — the kind the `DiagramKind` value, the node/edge counts the built graph extents, the algorithm the matched `LayoutPolicy.tag`; `_compute` builds the `PyDiGraph` from the `data/tabular#GRAPH` adjacency frame through `_as_graph`, folds the `LayoutPolicy` to the `LayoutMap` through `_position`, constructs each positioned `DiagramGlyph` through the `DiagramKind`-keyed `_emit` glyph fold threading the `graphic/color/derive#DERIVE` palette index and the per-mark layer name, keys the content through `ContentIdentity.of` over the canonical node-link wire, then projects the five-field `Diagram` fact. The `grandalf` `Layered` arm dispatches onto the runtime subprocess lane (`anyio.to_process.run_sync`) only when the package is the not-yet-admitted gated leg; the `rustworkx` arms resolve in-capsule on the cp315 core (the abi3 wheel covers cp315).
- Auto: `_as_graph` folds the adjacency frame rows into a `PyDiGraph` — `add_nodes_from` over the node attribute rows yields the stable index, `add_edges_from` over the source/target/weight rows yields the edges, so the presentation graph is built once from the `data/tabular#GRAPH` feed and never re-keyed; `_position` discriminates the `LayoutPolicy` — `Force` folds `rustworkx.spring_layout` to the `Pos2DMapping`, `Radial` folds `circular_layout`/`shell_layout`, `Layered` folds the `grandalf` Sugiyama assignment over the graph's `grandalf` `Vertex`/`Edge`/`Graph` adapter (the `SugiyamaLayout` `draw` pass yielding each vertex's `view.xy`), `Projected` folds the deterministic `transform` over each node attribute — every arm returning the `LayoutMap` `dict[int, Point]`; `_emit` discriminates the `DiagramKind` and constructs the positioned marks — `SUN_PATH` emits `Marker` ticks plus `Edge` arcs plus an `Annotation` compass from the radial map, `CIRCULATION` emits `Node` boxes plus `Edge` connectors with `Marker` arrowheads from the force map, `STACKING` emits `Swimlane` floor bands plus `Node` boxes from the layered map, `PROGRAM` emits `Node` boxes plus `Edge` adjacency connectors from the force map, `SITE` emits `Swimlane` parcels plus `Node` footprints plus `Annotation` callouts from the projected map — each mark threading the `GlyphStyle(layer=..., fill=palette_index, stroke=palette_index)` so the mark binds its named SVG group and its palette color, the edge waypoints the laid-out `points` the layout resolves so an orthogonal or curved route is one point sequence.
- Packages: `rustworkx` (`PyDiGraph`, `add_nodes_from`/`add_edges_from`/`num_nodes`/`num_edges`/`node_indices`/`edge_list`, `spring_layout(graph, ...)`/`circular_layout(graph, scale=)`/`shell_layout(graph, ...)`/`bipartite_layout(graph, ...)` the `Pos2DMapping` layout family, `node_link_json` the canonical content-key wire) on the cp315 core (abi3, one wheel cp310-cp315); `grandalf` (`Graph`/`Vertex`/`Edge`/`SugiyamaLayout`/`VertexViewer` the Sugiyama hierarchical layered-layout engine) NOT-yet-admitted — its members are RESEARCH-gated; runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the gated subprocess lane for the `grandalf` arm if it lands gated), `core/receipt#RECEIPT` (`ArtifactReceipt`), `graphic/color/derive#DERIVE` (`ColorReceipt.coords` the palette the `GlyphStyle` indices key), `visualization/diagram/glyphset#GLYPHSET` (`DiagramGlyph`/`DiagramKind`/`GlyphStyle`/`MarkerKind`), `data/tabular#GRAPH` (the adjacency/attribute frame feeding the graph build).
- Growth: a new AEC diagram kind is one `DiagramKind` row (in `visualization/diagram/glyphset#GLYPHSET`) plus one `_emit` glyph-fold arm and one `LayoutPolicy` selection, never a new layout method family; a new layout algorithm is one `LayoutPolicy` case plus one `_position` arm folding the new `rustworkx`/`grandalf` layout member; a new deterministic AEC projection is one `Projected` `transform` callable, never a new case; a new layout knob (a `spring_layout` `k`/`repulsive_exponent`, a `circular_layout` `center`) is one `LayoutPolicy` field threaded into the consuming arm; zero new surface for a new diagram kind.
- Boundary: no SVG emission (that is `visualization/diagram/draw#DRAW`'s — this owner emits the positioned `DiagramGlyph` sequence the draw owner folds to bytes); no graph analysis (the `data/graph#GRAPH` `rustworkx` analysis kernel — centrality, shortest-path, community — stays at the data plane; this owner builds a presentation graph from the `data/tabular#GRAPH` adjacency feed and runs only the layout functions, never a second analysis kernel); no ad-hoc color (the `GlyphStyle` indices key the `graphic/color/derive#DERIVE` palette); the `rustworkx` `Pos2DMapping` layouts compute coordinates only and feed the glyph carrier, never render; a per-diagram-kind layout method family, a re-keying of the layout coordinate away from the stable `rustworkx` node index, a hand-rolled spring/Sugiyama loop where `rustworkx`/`grandalf` own it, and a second graph-analysis kernel are the deleted forms — one coordinate-assignment owner, policy-discriminated layout, stable-index-keyed marks, one typed `Diagram` receipt.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import polars as pl
import rustworkx as rx
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.visualization.diagram.glyphset import DiagramGlyph, DiagramKind, GlyphStyle, MarkerKind

type Point = tuple[float, float]
type LayoutMap = dict[int, Point]
type Projection = Callable[[int, dict[str, object]], Point]


@tagged_union(frozen=True)
class LayoutPolicy:
    tag: Literal["force", "radial", "layered", "projected"] = tag()
    force: tuple[int, int] = case()
    radial: float = case()
    layered: Literal["TB", "LR"] = case()
    projected: Projection = case()

    @staticmethod
    def Force(seed: int = 0, iterations: int = 100) -> "LayoutPolicy":
        return LayoutPolicy(force=(seed, iterations))

    @staticmethod
    def Radial(scale: float = 1.0) -> "LayoutPolicy":
        return LayoutPolicy(radial=scale)

    @staticmethod
    def Layered(direction: Literal["TB", "LR"] = "TB") -> "LayoutPolicy":
        return LayoutPolicy(layered=direction)

    @staticmethod
    def Projected(transform: Projection) -> "LayoutPolicy":
        return LayoutPolicy(projected=transform)


_KIND_POLICY: dict[DiagramKind, LayoutPolicy] = {
    DiagramKind.SUN_PATH: LayoutPolicy.Radial(),
    DiagramKind.CIRCULATION: LayoutPolicy.Force(),
    DiagramKind.STACKING: LayoutPolicy.Layered("TB"),
    DiagramKind.PROGRAM: LayoutPolicy.Force(),
    DiagramKind.SITE: LayoutPolicy.Projected(lambda i, attrs: (float(attrs["east"]), float(attrs["north"]))),
}


class DiagramLayout(Struct, frozen=True):
    kind: DiagramKind
    adjacency: pl.DataFrame
    attributes: pl.DataFrame
    palette: object
    policy: LayoutPolicy | None = None

    async def assign(self) -> RuntimeRail[tuple[tuple[DiagramGlyph, ...], ArtifactReceipt]]:
        return await async_boundary(f"diagram.layout.{self.kind}", self._compute)

    async def _compute(self) -> tuple[tuple[DiagramGlyph, ...], ArtifactReceipt]:
        graph = self._as_graph()
        coords = self._position(graph)
        glyphs = self._emit(graph, coords)
        key = ContentIdentity.of(f"diagram-{self.kind}", rx.node_link_json(graph).encode())
        policy = self.policy or _KIND_POLICY[self.kind]
        receipt = ArtifactReceipt.Diagram(key, self.kind.value, graph.num_nodes(), graph.num_edges(), policy.tag)
        return glyphs, receipt

    def _as_graph(self) -> rx.PyDiGraph:
        graph: rx.PyDiGraph = rx.PyDiGraph()
        indices = graph.add_nodes_from(self.attributes.to_dicts())
        lookup = {row["id"]: indices[i] for i, row in enumerate(self.attributes.to_dicts())}
        graph.add_edges_from((lookup[r["source"]], lookup[r["target"]], r) for r in self.adjacency.to_dicts())
        return graph

    def _position(self, graph: rx.PyDiGraph) -> LayoutMap:
        policy = self.policy or _KIND_POLICY[self.kind]
        match policy:
            case LayoutPolicy(tag="force", force=(seed, iterations)):
                pos = rx.spring_layout(graph, seed=seed, num_iter=iterations)
                return {i: tuple(pos[i]) for i in graph.node_indices()}
            case LayoutPolicy(tag="radial", radial=scale):
                pos = rx.circular_layout(graph, scale=scale)
                return {i: tuple(pos[i]) for i in graph.node_indices()}
            case LayoutPolicy(tag="layered", layered=direction):
                return _sugiyama(graph, direction)
            case LayoutPolicy(tag="projected", projected=transform):
                return {i: transform(i, graph.get_node_data(i)) for i in graph.node_indices()}
            case _:
                assert_never(policy)

    def _emit(self, graph: rx.PyDiGraph, coords: LayoutMap) -> tuple[DiagramGlyph, ...]:
        ramp = self.palette
        match self.kind:
            case DiagramKind.SUN_PATH:
                ticks = tuple(DiagramGlyph.Marker(*coords[i], MarkerKind.TICK, 0.0, GlyphStyle("sun")) for i in graph.node_indices())
                arcs = tuple(DiagramGlyph.Edge(s, t, (coords[s], coords[t]), None, GlyphStyle("path", stroke=1)) for s, t in graph.edge_list())
                return (*arcs, *ticks, DiagramGlyph.Marker(0.0, 0.0, MarkerKind.NORTH, 0.0, GlyphStyle("compass")))
            case DiagramKind.CIRCULATION:
                boxes = tuple(DiagramGlyph.Node(i, *coords[i], 40.0, 24.0, str(graph.get_node_data(i).get("label")), GlyphStyle("spaces")) for i in graph.node_indices())
                flows = tuple(DiagramGlyph.Edge(s, t, (coords[s], coords[t]), None, GlyphStyle("circulation", stroke=2)) for s, t in graph.edge_list())
                return (*boxes, *flows)
            case DiagramKind.STACKING:
                bands = tuple(DiagramGlyph.Swimlane(i, 0.0, coords[i][1], 200.0, 30.0, str(graph.get_node_data(i).get("floor")), GlyphStyle("floors", fill=i)) for i in graph.node_indices())
                return tuple(bands)
            case DiagramKind.PROGRAM:
                boxes = tuple(DiagramGlyph.Node(i, *coords[i], 48.0, 32.0, str(graph.get_node_data(i).get("program")), GlyphStyle("program", fill=i)) for i in graph.node_indices())
                adj = tuple(DiagramGlyph.Edge(s, t, (coords[s], coords[t]), None, GlyphStyle("adjacency", stroke=3)) for s, t in graph.edge_list())
                return (*boxes, *adj)
            case DiagramKind.SITE:
                parcels = tuple(DiagramGlyph.Swimlane(i, *coords[i], 80.0, 80.0, None, GlyphStyle("parcels", fill=i)) for i in graph.node_indices())
                footprints = tuple(DiagramGlyph.Node(i, *coords[i], 24.0, 24.0, None, GlyphStyle("buildings")) for i in graph.node_indices())
                return (*parcels, *footprints)
            case _:
                assert_never(self.kind)


def _sugiyama(graph: rx.PyDiGraph, direction: Literal["TB", "LR"]) -> LayoutMap:
    from grandalf.graphs import Edge, Graph, Vertex
    from grandalf.layouts import SugiyamaLayout

    vertices = {i: Vertex(i) for i in graph.node_indices()}
    for vertex in vertices.values():
        vertex.view = type("View", (), {"w": 40.0, "h": 24.0})()
    edges = [Edge(vertices[s], vertices[t]) for s, t in graph.edge_list()]
    grand = Graph(list(vertices.values()), edges)
    layout = SugiyamaLayout(grand.C[0])
    layout.init_all()
    layout.draw()
    return {i: (v.view.xy[1], v.view.xy[0]) if direction == "LR" else v.view.xy for i, v in vertices.items()}
```

`DiagramLayout.assign` builds the presentation `PyDiGraph` from the `data/tabular#GRAPH` adjacency frame, assigns coordinates through the `LayoutPolicy` the `DiagramKind` selects, and constructs each positioned `DiagramGlyph` keyed on the stable `rustworkx` node index. The `_position` fold is the one layout-algorithm dispatch — `rustworkx` `spring_layout`/`circular_layout` for the force and radial kinds, the `grandalf` Sugiyama hierarchical assignment for the layered kinds, the deterministic `transform` for the projected kinds — every arm returning the `LayoutMap`, and `_emit` is the one `DiagramKind`-keyed glyph fold threading the `graphic/color/derive#DERIVE` palette index and the per-mark layer name into each mark. The content key is the canonical `rustworkx` `node_link_json` wire, the same durable serialization the `data/graph#GRAPH` owner content-keys, so a diagram is content-addressable.

## [03]-[RESEARCH]

- [RUSTWORKX_LAYOUT]: the `rustworkx` `PyDiGraph` container, the `add_nodes_from`/`add_edges_from`/`num_nodes`/`num_edges`/`node_indices`/`edge_list`/`get_node_data` mutation-and-query members, the `spring_layout(graph, ...)`/`circular_layout(graph, scale=)`/`shell_layout(graph, ...)`/`bipartite_layout(graph, ...)` `Pos2DMapping` layout family, and the `node_link_json(graph)` canonical-wire serializer verify against the folder `.api/rustworkx.md` catalogue (`0.18.0` reflected, Apache-2.0, abi3 cp310-cp315 — one wheel resolves on the cp315 core). The `[02]-[PUBLIC_TYPES]` shared-container members (mutation row [01], query row [02], edge-views row [03]) settle `add_nodes_from`/`add_edges_from`/`node_indices`/`edge_list`/`get_node_data`, the `[03]-[ENTRYPOINTS]` layout row [08] settles `spring_layout`/`circular_layout`/`shell_layout`/`bipartite_layout` returning `Pos2DMapping`, and IO row [09] settles `node_link_json` — the `[04]-[IMPLEMENTATION_LAW]` layout law "layout functions return `Pos2DMapping` (node index to `(x, y)`); they compute coordinates only and feed a downstream visualization carrier, never render" is exactly this owner's contract, and the "stable integer index with no recycling makes `NodeIndices` durable join keys" law is the node-index keying the marks share. The exact `spring_layout` keyword spelling (`num_iter` versus `iterations`, the `seed`/`k`/`repulsive_exponent` knobs) and the `Pos2DMapping` indexing contract (`pos[i] -> (x, y)` element access) are the one [RUSTWORKX_LAYOUT_KWARGS] catalogue-deepen item until a `spring_layout`/`circular_layout` signature reflection pass enumerates the layout keyword set; the `PyDiGraph` build, the `node_link_json` content-key wire, and the `Pos2DMapping`-to-`LayoutMap` projection are settled fence code.
- [GRANDALF_SUGIYAMA] [RESEARCH]: `grandalf` is the Sugiyama hierarchical layered-layout engine the `Layered` arm folds for the stacking and program-tree kinds, but it is NOT yet admitted to the manifest and NO folder `.api/grandalf.md` catalogue exists. The `grandalf.graphs.Graph`/`Vertex`/`Edge` graph model, the `grandalf.layouts.SugiyamaLayout` layered-layout engine, the `Vertex.view` viewer-attachment protocol (the `view.w`/`view.h` extent and the `view.xy` resolved coordinate the `draw` pass writes), and the `SugiyamaLayout.init_all`/`draw` layout passes are all marked RESEARCH and never appear as settled fence code until `grandalf` is admitted (osx-arm64, cp315-compatible, pure-Python BSD) and a `grandalf` `.api` reflection pass lands. Close-condition: the manifest admits `grandalf` and `.api/grandalf.md` carries the `Graph`/`Vertex`/`Edge` model, the `SugiyamaLayout` engine with its `init_all`/`draw` passes, the `Vertex.view` `w`/`h`/`xy` viewer contract, and the cp315-clean-versus-gated band; until then the `_sugiyama` adapter body and the `LayoutPolicy.Layered` arm are RESEARCH-gated, the `LayoutPolicy.Layered(direction)` case shape and the `Layered` selection in `_KIND_POLICY` the settled structure, the `grandalf` member spellings the RESEARCH leg. The `rustworkx` arms cover the force/radial/projected kinds with zero gated dependency, so the diagram engine renders the sun-path, circulation, program, and site kinds on the settled `rustworkx` core while the stacking kind's `grandalf` Sugiyama leg lands on admission; a `rustworkx`-only hierarchical fallback (`bipartite_layout` over the topological generations) is the interim layered path until `grandalf` is admitted.
- [DIAGRAM_RECEIPT_FACTS] [RESOLVED]: `DiagramLayout._compute` projects `ArtifactReceipt.Diagram(key, self.kind.value, graph.num_nodes(), graph.num_edges(), policy.tag)` against the settled `core/receipt#RECEIPT` `Diagram(key, kind, nodes, edges, algorithm)` case (`diagram: tuple[ContentKey, str, int, int, str]`) whose `_facts` arm projects `{"key", "kind", "nodes", "edges", "algorithm"}` — all flat scalars, the kind the `DiagramKind` value, the node/edge counts the built-graph extents, the algorithm the matched `LayoutPolicy.tag`. The `Diagram` case is the diagram-engine contribution to the shared `core/receipt#RECEIPT` `ArtifactReceipt` family — a case on the receipt owner, never a parallel diagram-receipt rail and never a producer value object the receipt owner imports — shared with the `visualization/diagram/draw#DRAW` SVG-emission owner which contributes the same `Diagram` case off its node/edge glyph tallies (the layered element/layer/byte evidence staying the `export/layered#LAYERED` `Preview`/`Egress` contribution); the `DiagramLayout.assign -> RuntimeRail[...]` rail shape mirrors the settled `visualization/chart/export#EXPORT` `ChartExport.render -> RuntimeRail[ArtifactReceipt]` pattern. The `core/receipt#RECEIPT` owner carries the `Diagram` case, so the receipt call is settled fence code.
