# [PY_DATA_GRAPH]

One graph-payload owner over a license-split backend triangle: the permissive `rustworkx` analysis core, the `networkx` codec/egress lane, and the GPL-confined `igraph` community engine carrying the Leiden/Louvain/Infomap split rustworkx lacks and the BSD core cannot license. Its backend is recovered from the source shape, never a knob, and analysis collapses onto ONE kernel: every algorithm runs on `rustworkx` keyed by its stable non-recycled integer index, a `networkx` or `igraph` source converting once through the one-way `_as_rx` bridge, so the `NodeId` stays the rx `int` the node-keyed frame seam joins on.

Payload identity is the railed `ContentIdentity` fingerprint over the canonical node-link wire, never a `repr(dict)` byte stream. `GraphResult.frame` lowers node-index-keyed results into one canonical `node`-keyed `pa.Table` the `tabular/columnar#SCAN` plane left-joins by `node` — a centrality run is a left-join enrichment, never a re-keyed copy. `igraph`'s GPL core stays in this data graph rail and is never linked into a host-distributed plugin. `organization_graph` folds the C#-minted `rasm.organization` organization document into the containment graph, decoding through the branch's one wire-shape owner at `runtime/transport/shapes#VOCABULARY` and minting no wire struct here; the capacity-network flow family rides the sibling `graph/network#NETWORK` owner over the networkx lane this kernel does not spell.

## [01]-[INDEX]

- [02]-[GRAPH]: the `GraphPayload` owner — one rustworkx kernel over `_as_rx`-coerced sources, family-folded algorithm intent, typed result receipts, content-keyed egress.
- [03]-[TOPOLOGY]: `organization_graph` recursive-forest fold — ordered entities and nested members folded onto one kernel after one bounded admission.

## [02]-[GRAPH]

- Owner: `GraphPayload` carries the admitted graph as `self.graph`, so the recovered `backend`/`kind` never decouple from the analyzed graph — `analyze`/`write` take no graph parameter; a re-passed handle breaking that invariant is the rejected form.
- Cases: payload splits follow real provider arity — `all_pairs_distance` carries the `null_value` its `distance_matrix` substrate declares while `floyd_warshall` carries only its `WeightSelector` (`floyd_warshall_numpy` takes `weight_fn`, no null-value parameter); the connectivity polarity is recovered from `kind.directed`, never a caller flag; every weighted member carries a `WeightSelector` slot defaulting `WEIGHT_IDENTITY`, so a non-float edge payload is weightable by one policy value.
- Entry: `analyze` absorbs a lone `GraphAlgorithm` or a `Block` over one `match` at the head — the arity is the value's shape, the `Disposition` selects the batch output shape through the `@overload` ladder and is inert for a lone algorithm, so the input shape and the disposition together carry the output type. A non-node-keyed result case carries no per-node row, so `frame` names the case as non-node-keyed rather than minting a degenerate frame; `write` routes the `_EGRESS` codec directly on the source backend, never through the analysis-coercion path.
- Auto: the bare-name rustworkx members dispatch on graph subtype, so the owner never names the `graph_*`/`digraph_*` typed forms; the dense matrices stay `npt.NDArray[np.float64]` so they fold straight into the tensor carriers.
- Receipt: the content key derives once at admission from the canonical node-link wire and the receipt reuses it — an unchanged graph keys byte-stable, an added edge re-admits to a new key; the algorithm receipt is typed rail evidence, never product graph-database state. `contribute` projects node/edge counts onto the runtime `Metrics.record` arm under `domain="graph"` keyed by algorithm, and `_one` opens the kernel span — the no-scrape analysis engine's whole observability surface, the runtime fence marking the span on a failed leg.
- Packages: `pyarrow` and the GPL `igraph` each bind one module-scope `lazy import`, so the codec-only graph path never loads Arrow and a run that never reaches the community split never links the igraph C core. Dereference is confined to the folds that already need the binding — `_frame` and `_arrow_raises` for Arrow, `_ig_from` and the two catch resolvers for the C core — and each catch resolves at the CALL for exactly that reason: a module-scope `Final[Catch]` naming a provider's exception class links that provider at import, on every run, which is the confinement the `lazy` bind exists to hold.
- Growth: a new algorithm is one `GraphAlgorithm` case plus one `_run_rx` arm; a new community algorithm one `IG_COMMUNITY` row; a new centrality metric one `RX_CENTRALITY` row; a new egress one `GraphFormat` row plus one `_EGRESS` codec row; a new layout one `LayoutKind` row. A networkx `@_dispatchable` accelerator lands as one `backend=`/`nx.config.backend_priority` policy on the codec lane when such a backend enters the manifest roster, never a second analysis kernel — a phantom accelerator axis claimed but unwired is the rejected form. Deferred rustworkx residue is the named set — VF2 isomorphism (`vf2_mapping`/`is_isomorphic`), the `rustworkx.generators` builders, the DOT/Matrix-Market IO codecs, group centrality, edge coloring — each one case plus one arm when a consumer names it.
- Boundary: the graph plane produces the node-keyed enrichment frame; the relational join belongs to the tabular plane, never a graph-database node table re-minted here. `NodeId` is never widened to `Hashable` to admit a networkx analysis kernel — conversion keeps it the rx `int`. No product collaboration store, no bridge lifecycle, no compute numeric trio.

```python signature
import sys
import tempfile
from collections.abc import Iterable
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never, overload

import msgspec
import networkx as nx
import numpy as np
import rustworkx as rx
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from protovalidate import CompilationError, EvaluationError, ValidationError, validate

lazy import igraph
lazy import pyarrow as pa

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import (
    TERMINAL,
    BoundaryFault,
    Catch,
    Depth,
    Disposition,
    FaultRow,
    RuntimeRail,
    boundary,
    rostered,
    scoped,
    traversed,
)
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.contracts.rasm.contracts.organization.organization_pb import Entity, Organization

if TYPE_CHECKING:
    from collections.abc import Callable

    import numpy.typing as npt


# --- [TYPES] ----------------------------------------------------------------------------

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.graph")

type NodeId = int
type RxGraph = rx.PyGraph | rx.PyDiGraph
type NxGraph = nx.Graph | nx.DiGraph
type AnyGraph = "RxGraph | NxGraph | igraph.Graph"
type GraphBackend = Literal["rustworkx", "networkx", "igraph"]
type ScoreMap = tuple[tuple[NodeId, float], ...]
type Partition = tuple[tuple[NodeId, ...], ...]
type Matrix = npt.NDArray[np.float64]
type WeightSelector = Callable[[Any], float]

WEIGHT_IDENTITY: Final[WeightSelector] = float

type LeidenObjective = Literal["CPM", "modularity"]
_IG_WEIGHT: Final[str] = "weight"


class GraphFormat(StrEnum):
    NODE_LINK = "node_link"
    GRAPHML = "graphml"
    EDGE_LIST = "edge_list"


class LayoutKind(StrEnum):
    SPRING = "spring"
    CIRCULAR = "circular"
    KAMADA_KAWAI = "kamada_kawai"


# --- [CONSTANTS] ------------------------------------------------------------------------

_RX_RAISES: Final[Catch] = (
    rx.DAGHasCycle,
    rx.DAGWouldCycle,
    rx.FailedToConverge,
    rx.GraphNotBipartite,
    rx.InvalidMapping,
    rx.InvalidNode,
    rx.JSONSerializationError,
    rx.NegativeCycle,
    rx.NoEdgeBetweenNodes,
    rx.NoPathFound,
    rx.NoSuitableNeighbors,
    rx.NullGraph,
    nx.NetworkXException,
    TypeError,
    ValueError,
)
_ORG_RAISES: Final[Catch] = (*_RX_RAISES, TypeError, ValueError, CompilationError, EvaluationError, ValidationError)

GRAPH_ANALYZE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRAPH, point="analyze", arm="boundary", defect="kernel", retriability=TERMINAL
)
GRAPH_FRAME: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRAPH, point="frame", arm="boundary", defect="frame-lowering", retriability=TERMINAL
)
GRAPH_UNFRAMED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRAPH, point="frame.index", arm="config", defect="no-index-row", retriability=TERMINAL, slots=("result",)
)
GRAPH_EGRESS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRAPH, point="egress", arm="boundary", defect="codec", retriability=TERMINAL
)
ORG_BUILD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRAPH, point="organization", arm="boundary", defect="forest-build", retriability=TERMINAL
)
ORG_INVALID: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.GRAPH, point="organization.forest", arm="config", defect="invalid-forest", retriability=TERMINAL, slots=("reason",)
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    GRAPH_ANALYZE,
    GRAPH_FRAME,
    GRAPH_UNFRAMED,
    GRAPH_EGRESS,
    ORG_BUILD,
    ORG_INVALID,
]))


# --- [MODELS] ---------------------------------------------------------------------------


class GraphKind(Struct, frozen=True, gc=False):
    directed: bool
    multigraph: bool


@tagged_union(frozen=True)
class GraphAlgorithm:
    tag: Literal[
        "bfs",
        "dfs",
        "topo_sort",
        "ancestors",
        "descendants",
        "shortest_path",
        "bellman_ford",
        "astar",
        "k_shortest",
        "all_simple_paths",
        "all_pairs_distance",
        "floyd_warshall",
        "longest_path",
        "transitive_reduction",
        "dominators",
        "connected",
        "strongly_connected",
        "articulation",
        "bridges",
        "cycle_basis",
        "condensation",
        "core_number",
        "min_cut",
        "betweenness",
        "closeness",
        "eigenvector",
        "katz",
        "pagerank",
        "hits",
        "degree",
        "greedy_color",
        "max_weight_matching",
        "spanning_tree",
        "steiner_tree",
        "transitivity",
        "is_planar",
        "layout",
        "leiden",
        "louvain",
        "infomap",
    ] = tag()
    bfs: tuple[NodeId, Depth] = case()
    dfs: NodeId | None = case()
    topo_sort: None = case()
    ancestors: tuple[NodeId, Depth] = case()
    descendants: tuple[NodeId, Depth] = case()
    shortest_path: tuple[NodeId, NodeId, WeightSelector] = case()
    bellman_ford: tuple[NodeId, NodeId, WeightSelector] = case()
    astar: tuple[NodeId, NodeId, WeightSelector, "Callable[[NodeId], float]"] = case()
    k_shortest: tuple[NodeId, int, WeightSelector] = case()
    all_simple_paths: tuple[NodeId, NodeId] = case()
    all_pairs_distance: float = case()
    floyd_warshall: WeightSelector = case()
    longest_path: None = case()
    transitive_reduction: None = case()
    dominators: NodeId = case()
    connected: None = case()
    strongly_connected: None = case()
    articulation: None = case()
    bridges: None = case()
    cycle_basis: NodeId | None = case()
    condensation: None = case()
    core_number: None = case()
    min_cut: WeightSelector = case()
    betweenness: bool = case()
    closeness: bool = case()
    eigenvector: int = case()
    katz: float = case()
    pagerank: float = case()
    hits: int = case()
    degree: None = case()
    greedy_color: None = case()
    max_weight_matching: tuple[bool, WeightSelector] = case()
    spanning_tree: WeightSelector = case()
    steiner_tree: tuple[tuple[NodeId, ...], WeightSelector] = case()
    transitivity: None = case()
    is_planar: None = case()
    layout: LayoutKind = case()
    leiden: "tuple[float, WeightSelector, LeidenObjective, int]" = case()
    louvain: tuple[float, WeightSelector] = case()
    infomap: tuple[int, WeightSelector] = case()


@tagged_union(frozen=True)
class GraphResult:
    tag: Literal[
        "order", "layered", "path", "paths", "scores", "matrix", "partition", "tree", "coloring", "matching", "layout",
        "scalar", "flag", "flows",
    ] = tag()
    order: tuple[NodeId, ...] = case()
    layered: tuple[tuple[NodeId, int], ...] = case()
    path: tuple[NodeId, ...] = case()
    paths: tuple[tuple[NodeId, ...], ...] = case()
    scores: ScoreMap = case()
    matrix: Matrix = case()
    partition: Partition = case()
    tree: tuple[tuple[NodeId, NodeId], ...] = case()
    coloring: tuple[tuple[NodeId, int], ...] = case()
    matching: tuple[tuple[NodeId, NodeId], ...] = case()
    layout: tuple[tuple[NodeId, tuple[float, float]], ...] = case()
    scalar: float = case()
    flag: bool = case()
    flows: tuple[tuple[NodeId, NodeId, float], ...] = case()

    def frame(self) -> "RuntimeRail[pa.Table]":
        return boundary(GRAPH_FRAME, lambda: _frame(self), catch=_arrow_raises()).bind(lambda railed: railed)


class GraphReceipt(Struct, frozen=True, gc=False):
    backend: GraphBackend
    kind: GraphKind
    node_count: int
    edge_count: int
    algorithm: str
    result: str
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        Metrics.record(
            {"rasm.graph.nodes": float(self.node_count), "rasm.graph.edges": float(self.edge_count)}, domain="graph", kind=self.algorithm
        )
        yield Receipt.of(
            "graph",
            (
                "emitted",
                self.backend,
                {
                    "kind": f"directed={self.kind.directed},multi={self.kind.multigraph}",
                    "nodes": self.node_count,
                    "edges": self.edge_count,
                    "algorithm": self.algorithm,
                    "result": self.result,
                },
            ),
        )


class GraphPayload(Struct, frozen=True, gc=False):
    graph: Any
    backend: GraphBackend
    kind: GraphKind
    node_count: int
    edge_count: int
    content_key: ContentKey

    @classmethod
    def of(cls, graph: "AnyGraph") -> "RuntimeRail[GraphPayload]":
        backend, kind, n, e, wire = _shape(graph)
        return ContentIdentity.of("graph", wire).map(
            lambda key: cls(graph=graph, backend=backend, kind=kind, node_count=n, edge_count=e, content_key=key)
        )

    @overload
    def analyze(self, algo: "GraphAlgorithm", *, by: Disposition = ...) -> "RuntimeRail[GraphResult]": ...
    @overload
    def analyze(
        self, algo: "Block[GraphAlgorithm]", *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ...
    ) -> "RuntimeRail[Block[GraphResult]]": ...
    @overload
    def analyze(
        self, algo: "Block[GraphAlgorithm]", *, by: Literal[Disposition.PARTITION]
    ) -> "RuntimeRail[tuple[Block[GraphResult], Block[BoundaryFault]]]": ...
    def analyze(
        self, algo: "GraphAlgorithm | Block[GraphAlgorithm]", *, by: Disposition = Disposition.ABORT
    ) -> "RuntimeRail[GraphResult] | RuntimeRail[Block[GraphResult]] | RuntimeRail[tuple[Block[GraphResult], Block[BoundaryFault]]]":
        match algo:
            case Block() as algos:
                return traversed(algos.map(self._one), by=by)
            case lone:
                return self._one(lone)

    def _one(self, algo: "GraphAlgorithm") -> "RuntimeRail[GraphResult]":
        with _TRACER.start_as_current_span(
            f"graph.analyze.{algo.tag}",
            attributes={"rasm.graph.algorithm": algo.tag, "rasm.graph.backend": self.backend, "rasm.graph.nodes": self.node_count},
        ):
            return boundary(GRAPH_ANALYZE, lambda: _run_rx(_as_rx(self.graph), algo, self.kind), catch=_kernel_raises(algo))

    def write(self, fmt: GraphFormat) -> "RuntimeRail[bytes]":
        return boundary(GRAPH_EGRESS, lambda: _EGRESS[self.backend][fmt](self.graph), catch=_egress_raises(self.backend))

    def receipt(self, algo: "GraphAlgorithm", result: GraphResult) -> GraphReceipt:
        return GraphReceipt(
            backend=self.backend,
            kind=self.kind,
            node_count=self.node_count,
            edge_count=self.edge_count,
            algorithm=algo.tag,
            result=result.tag,
            content_key=self.content_key,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _node_link(g: NxGraph) -> bytes:
    return msgspec.json.encode(nx.node_link_data(g, edges="edges"))


def _wire(graph: "AnyGraph", backend: GraphBackend) -> bytes:
    match backend:
        case "rustworkx":
            return rx.node_link_json(graph).encode()
        case "igraph":
            return _node_link(graph.to_networkx())
        case _:
            return _node_link(graph)


def _is_ig(graph: object) -> bool:
    ig = sys.modules.get("igraph")
    return ig is not None and isinstance(graph, ig.Graph)


def _shape(graph: "AnyGraph") -> "tuple[GraphBackend, GraphKind, int, int, bytes]":
    match graph:
        case rx.PyGraph() | rx.PyDiGraph():
            kind = GraphKind(directed=isinstance(graph, rx.PyDiGraph), multigraph=graph.multigraph)
            return "rustworkx", kind, graph.num_nodes(), graph.num_edges(), _wire(graph, "rustworkx")
        case _ if _is_ig(graph):
            kind = GraphKind(directed=graph.is_directed(), multigraph=graph.has_multiple())
            return "igraph", kind, graph.vcount(), graph.ecount(), _wire(graph, "igraph")
        case _:
            kind = GraphKind(directed=graph.is_directed(), multigraph=graph.is_multigraph())
            return "networkx", kind, graph.number_of_nodes(), graph.number_of_edges(), _wire(graph, "networkx")


def _arrow_raises() -> Catch:
    return (pa.ArrowException, TypeError, ValueError)


def _frame(result: GraphResult) -> "RuntimeRail[pa.Table]":
    match result:
        case GraphResult(tag="scores", scores=rows):
            return Ok(pa.Table.from_pydict({"node": [n for n, _ in rows], "value": [v for _, v in rows]}))
        case GraphResult(tag="coloring", coloring=rows):
            return Ok(pa.Table.from_pydict({"node": [n for n, _ in rows], "color": [c for _, c in rows]}))
        case GraphResult(tag="partition", partition=blocks):
            return Ok(pa.Table.from_pydict({
                "node": [n for block in blocks for n in block],
                "component": [i for i, block in enumerate(blocks) for _ in block],
            }))
        case GraphResult(tag="order", order=nodes):
            return Ok(pa.Table.from_pydict({"node": list(nodes), "rank": list(range(len(nodes)))}))
        case GraphResult(tag="layered", layered=rows):
            return Ok(pa.Table.from_pydict({"node": [n for n, _ in rows], "depth": [d for _, d in rows]}))
        case GraphResult(tag="layout", layout=rows):
            return Ok(pa.Table.from_pydict({"node": [n for n, _ in rows], "x": [xy[0] for _, xy in rows], "y": [xy[1] for _, xy in rows]}))
        case GraphResult(tag="flows", flows=rows):
            return Ok(pa.Table.from_pydict({
                "source": [u for u, _, _ in rows],
                "target": [v for _, v, _ in rows],
                "flow": [f for _, _, f in rows],
            }))
        case GraphResult(tag="tree", tree=rows) | GraphResult(tag="matching", matching=rows):
            return Ok(pa.Table.from_pydict({"source": [u for u, _ in rows], "target": [v for _, v in rows]}))
        case _:
            return Error(GRAPH_UNFRAMED.raised(result.tag))


# --- [RUSTWORKX_KERNEL] -----------------------------------------------------------------

RX_CENTRALITY: "Final[Map[str, Callable[[RxGraph, GraphAlgorithm], dict[NodeId, float]]]]" = Map.of_seq([
    ("betweenness", lambda g, a: rx.betweenness_centrality(g, normalized=a.betweenness)),
    ("closeness", lambda g, a: rx.closeness_centrality(g, wf_improved=a.closeness)),
    ("eigenvector", lambda g, a: rx.eigenvector_centrality(g, max_iter=a.eigenvector)),
    ("katz", lambda g, a: rx.katz_centrality(g, alpha=a.katz)),
    ("pagerank", lambda g, a: rx.pagerank(g, alpha=a.pagerank)),
    ("degree", lambda g, _: rx.degree_centrality(g)),
])
RX_LAYOUT: "Final[Map[LayoutKind, Callable[[RxGraph], rx.Pos2DMapping]]]" = Map.of_seq([
    (LayoutKind.SPRING, rx.spring_layout),
    (LayoutKind.CIRCULAR, rx.circular_layout),
    (LayoutKind.KAMADA_KAWAI, rx.kamada_kawai_layout),
])


def _as_rx(graph: "AnyGraph") -> RxGraph:
    match graph:
        case rx.PyGraph() | rx.PyDiGraph():
            return graph
        case _ if _is_ig(graph):
            return rx.networkx_converter(graph.to_networkx())
        case _:
            return rx.networkx_converter(graph)


def _upstream(g: RxGraph) -> RxGraph:
    if not isinstance(g, rx.PyDiGraph):
        return g
    upstream = g.copy()
    upstream.reverse()
    return upstream


def _reached(bound: Depth, hop: int) -> bool:
    match bound:
        case Depth(tag="fixpoint"):
            return True
        case Depth(tag="bounded", bounded=limit):
            return hop <= limit
        case _ as unreachable:
            assert_never(unreachable)


def _layered(g: RxGraph, source: NodeId, bound: Depth, *, seeded: bool) -> GraphResult:
    return GraphResult(layered=tuple(
        (node, hop)
        for hop, layer in enumerate(rx.bfs_layers(g, [source]))
        if (hop > 0 or seeded) and _reached(bound, hop)
        for node in layer
    ))


def _run_rx(g: RxGraph, algo: GraphAlgorithm, kind: GraphKind) -> GraphResult:
    match algo:
        case GraphAlgorithm(tag="bfs", bfs=(source, bound)):
            return _layered(g, source, bound, seeded=True)
        case GraphAlgorithm(tag="dfs"):
            return GraphResult(order=tuple(n for edge in rx.dfs_edges(g, algo.dfs) for n in edge))
        case GraphAlgorithm(tag="topo_sort"):
            return GraphResult(order=tuple(rx.topological_sort(g)))
        case GraphAlgorithm(tag="ancestors", ancestors=(source, bound)):
            return _layered(_upstream(g), source, bound, seeded=False)
        case GraphAlgorithm(tag="descendants", descendants=(source, bound)):
            return _layered(g, source, bound, seeded=False)
        case GraphAlgorithm(tag="shortest_path", shortest_path=(src, dst, weight)):
            paths = rx.dijkstra_shortest_paths(g, src, target=dst, weight_fn=weight)
            return GraphResult(path=tuple(paths[dst]) if dst in paths else ())
        case GraphAlgorithm(tag="bellman_ford", bellman_ford=(src, dst, weight)):
            paths = rx.bellman_ford_shortest_paths(g, src, target=dst, weight_fn=weight)
            return GraphResult(path=tuple(paths[dst]) if dst in paths else ())
        case GraphAlgorithm(tag="astar", astar=(src, dst, edge_cost, estimate)):
            return GraphResult(path=tuple(rx.astar_shortest_path(g, src, lambda n: n == dst, edge_cost, estimate)))
        case GraphAlgorithm(tag="k_shortest", k_shortest=(src, k, weight)):
            return GraphResult(scores=tuple(rx.k_shortest_path_lengths(g, src, k, weight).items()))
        case GraphAlgorithm(tag="all_simple_paths", all_simple_paths=(src, dst)):
            return GraphResult(paths=tuple(tuple(p) for p in rx.all_simple_paths(g, src, dst)))
        case GraphAlgorithm(tag="all_pairs_distance"):
            return GraphResult(matrix=np.asarray(rx.distance_matrix(g, null_value=algo.all_pairs_distance), dtype=np.float64))
        case GraphAlgorithm(tag="floyd_warshall", floyd_warshall=weight):
            return GraphResult(matrix=np.asarray(rx.floyd_warshall_numpy(g, weight_fn=weight), dtype=np.float64))
        case GraphAlgorithm(tag="longest_path"):
            return GraphResult(order=tuple(rx.dag_longest_path(g)))
        case GraphAlgorithm(tag="transitive_reduction"):
            return GraphResult(tree=tuple(rx.transitive_reduction(g)[0].edge_list()))
        case GraphAlgorithm(tag="dominators"):
            return GraphResult(scores=tuple((n, float(d)) for n, d in rx.immediate_dominators(g, algo.dominators).items()))
        case GraphAlgorithm(tag="connected"):
            comp = rx.weakly_connected_components(g) if kind.directed else rx.connected_components(g)
            return GraphResult(partition=tuple(tuple(c) for c in comp))
        case GraphAlgorithm(tag="strongly_connected"):
            return GraphResult(partition=tuple(tuple(c) for c in rx.strongly_connected_components(g)))
        case GraphAlgorithm(tag="articulation"):
            return GraphResult(order=tuple(rx.articulation_points(g)))
        case GraphAlgorithm(tag="bridges"):
            return GraphResult(tree=tuple(rx.bridges(g)))
        case GraphAlgorithm(tag="cycle_basis"):
            return GraphResult(paths=tuple(tuple(c) for c in rx.cycle_basis(g, root=algo.cycle_basis)))
        case GraphAlgorithm(tag="condensation"):
            return GraphResult(tree=tuple(rx.condensation(g).edge_list()))
        case GraphAlgorithm(tag="core_number"):
            return GraphResult(scores=tuple((n, float(k)) for n, k in rx.core_number(g).items()))
        case GraphAlgorithm(tag="min_cut", min_cut=weight):
            cut, _ = rx.stoer_wagner_min_cut(g, weight_fn=weight)
            return GraphResult(scalar=cut)
        case GraphAlgorithm(tag="betweenness" | "closeness" | "eigenvector" | "katz" | "pagerank" | "degree"):
            return GraphResult(scores=tuple(RX_CENTRALITY[algo.tag](g, algo).items()))
        case GraphAlgorithm(tag="hits"):
            hubs, _ = rx.hits(g, max_iter=algo.hits)
            return GraphResult(scores=tuple(hubs.items()))
        case GraphAlgorithm(tag="greedy_color"):
            return GraphResult(coloring=tuple(rx.graph_greedy_color(g, strategy=rx.ColoringStrategy.Saturation).items()))
        case GraphAlgorithm(tag="max_weight_matching", max_weight_matching=(max_cardinality, weight)):
            return GraphResult(matching=tuple(rx.max_weight_matching(g, max_cardinality=max_cardinality, weight_fn=lambda e: int(weight(e)))))
        case GraphAlgorithm(tag="spanning_tree", spanning_tree=weight):
            return GraphResult(tree=tuple(rx.minimum_spanning_tree(g, weight_fn=weight).edge_list()))
        case GraphAlgorithm(tag="steiner_tree", steiner_tree=(terminals, weight)):
            return GraphResult(tree=tuple(rx.steiner_tree(g, list(terminals), weight).edge_list()))
        case GraphAlgorithm(tag="transitivity"):
            return GraphResult(scalar=rx.transitivity(g))
        case GraphAlgorithm(tag="is_planar"):
            return GraphResult(flag=rx.is_planar(g))
        case GraphAlgorithm(tag="layout"):
            return GraphResult(layout=tuple((n, tuple(xy)) for n, xy in RX_LAYOUT[algo.layout](g).items()))
        case GraphAlgorithm(tag="leiden" | "louvain" | "infomap"):
            return _run_ig(_ig_from(g, kind, getattr(algo, algo.tag)[1]), algo, kind)
        case unreachable:
            assert_never(unreachable)


# --- [IGRAPH_COMMUNITY] -----------------------------------------------------------------

IG_COMMUNITY: "Final[Map[str, Callable[[igraph.Graph, GraphAlgorithm], igraph.VertexClustering]]]" = Map.of_seq([
    ("leiden", lambda g, a: g.community_leiden(objective_function=a.leiden[2], resolution=a.leiden[0], weights=_IG_WEIGHT, n_iterations=a.leiden[3])),
    ("louvain", lambda g, a: g.community_multilevel(resolution=a.louvain[0], weights=_IG_WEIGHT)),
    ("infomap", lambda g, a: g.community_infomap(trials=a.infomap[0], edge_weights=_IG_WEIGHT)),
])


def _kernel_raises(algo: GraphAlgorithm) -> Catch:
    if algo.tag not in IG_COMMUNITY:
        return _RX_RAISES
    return (*_RX_RAISES, igraph.InternalError)


def _egress_raises(backend: GraphBackend) -> Catch:
    codec: Catch = (rx.JSONSerializationError, nx.NetworkXException, TypeError, ValueError, OSError)
    return codec if backend != "igraph" else (*codec, igraph.InternalError)


def _ig_from(g: RxGraph, kind: GraphKind, weight: WeightSelector) -> "igraph.Graph":
    ig = igraph.Graph.TupleList(
        ((u, v, weight(payload)) for u, v, payload in g.weighted_edge_list()), directed=kind.directed, weights=True
    )
    named = set(ig.vs["name"])
    isolated = [n for n in g.node_indices() if n not in named]
    if isolated:
        ig.add_vertices(len(isolated), attributes={"name": isolated})
    return ig


def _run_ig(g: "igraph.Graph", algo: GraphAlgorithm, _: GraphKind) -> GraphResult:
    match algo:
        case GraphAlgorithm(tag="leiden" | "louvain" | "infomap"):
            names = g.vs["name"]
            return GraphResult(partition=tuple(tuple(names[v] for v in block) for block in IG_COMMUNITY[algo.tag](g, algo)))
        case off_lane:
            raise NotImplementedError(f"igraph backend owns only the community split, not {off_lane.tag}; route to rustworkx")


# --- [COMPOSITION] ----------------------------------------------------------------------


def _graphml(write: "Callable[[str], object]") -> bytes:
    with tempfile.NamedTemporaryFile(suffix=".graphml") as handle:
        write(handle.name)
        return Path(handle.name).read_bytes()


_EGRESS: "Final[Map[GraphBackend, Map[GraphFormat, Callable[[Any], bytes]]]]" = Map.of_seq([
    (
        "rustworkx",
        Map.of_seq([
            (GraphFormat.NODE_LINK, lambda g: rx.node_link_json(g).encode()),
            (GraphFormat.GRAPHML, lambda g: _graphml(lambda path: rx.write_graphml(g, path))),
            (GraphFormat.EDGE_LIST, lambda g: "\n".join(f"{u} {v}" for u, v in g.edge_list()).encode()),
        ]),
    ),
    (
        "networkx",
        Map.of_seq([
            (GraphFormat.NODE_LINK, _node_link),
            (GraphFormat.GRAPHML, lambda g: _graphml(lambda path: nx.write_graphml(g, path))),
            (GraphFormat.EDGE_LIST, lambda g: nx.to_pandas_edgelist(g).to_csv(index=False).encode()),
        ]),
    ),
    (
        "igraph",
        Map.of_seq([
            (GraphFormat.NODE_LINK, lambda g: _node_link(g.to_networkx())),
            (GraphFormat.GRAPHML, lambda g: _graphml(lambda path: g.write_graphml(path))),
            (GraphFormat.EDGE_LIST, lambda g: g.get_edge_dataframe().to_csv(index=False).encode()),
        ]),
    ),
])
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TD
    accTitle: Graph payload flow
    accDescr: Source admission into the payload, coercion onto the rustworkx kernel, the community delegation, result frames joining the tabular plane, and the receipt.
    src["rx.PyGraph·PyDiGraph / nx.Graph·DiGraph / igraph.Graph"] -->|_shape recovers backend·kind·counts·wire| payload["GraphPayload"]
    payload -->|ContentIdentity.of over node-link wire| key["RuntimeRail[ContentKey]"]
    payload -->|analyze lone: _one boundary fence| coerce["_as_rx: rx identity · nx networkx_converter · ig to_networkx+converter"]
    coerce -->|single rustworkx kernel, bare-name dispatch| rxk["_run_rx: path·centrality·structure·cut·layout"]
    rxk -->|community arm delegates| igk["_run_ig over TupleList: Leiden·Louvain·Infomap"]
    rxk --> result["GraphResult: order·layered·path·scores·matrix·partition·tree·coloring·matching·layout·scalar·flag"]
    igk --> result
    payload -->|analyze Block: traversed by Disposition| batch["RuntimeRail[Block[GraphResult]] | (results, faults)"]
    payload -->|write over GraphFormat: codec/egress lane| egress["_EGRESS[backend][fmt]: node_link·graphml·edge_list bytes"]
    result -->|frame: node-keyed cases| node_frame["RuntimeRail[pa.Table] node·value/color/component/rank/depth/x·y"]
    node_frame -->|columnar#SCAN pa.Table.join keys=node left outer| enrich["node-attribute enrichment"]
    result -->|receipt| receipt["GraphReceipt"]
    key --> receipt
    receipt -->|contribute| sink["runtime ReceiptContributor"]
```

## [03]-[TOPOLOGY]

- Owner: `organization_graph` folds one decoded generated `Organization` forest into the graph plane's node-link source — nodes are organizational addresses and authority-issued member keys, directed edges are recursive parent-child and member containment — returning `GraphPayload` beside `OrganizationIndex`.
- Law: schema and codec mint in C# beside `dotnet:Rasm.Rhino/Document/layers#ORGANIZATION_PROJECTION`; the generated class is this branch's ONE wire shape. Recursive occurrence makes parentage and acyclicity structural, ordered child lists carry sibling order, and members/view overrides cannot detach from their owner.
- Law: key SPACES stay separated — `OrganizationIndex.entities` maps content-addressed organizational addresses and `OrganizationIndex.members` federation keys the producing authority issued. One merged map lets an authority-issued key spelling a 32-hex address collide with an entity, silently re-pointing a containment query at the wrong node.
- Law: content-key spelling lowers exactly once at this decode — the wire carries 16 big-endian bytes and this branch's own key face is lowercase hex, so a consumer joining an address against any peer lowers and never uppercases.
- Entry: `organization_graph` performs the single schema-gap admission while flattening depth-first in sibling-list order: global entity-key uniqueness, at most 65,536 entities, depth at most 64, and exact current-path resolution. Only then does it allocate graph indexes; no dictionary insertion can overwrite a prior entity.
- Growth: one appended nested entity field carries a new axis; presentation evidence enters no graph edge.
- Boundary: generated Protovalidate already owns field, list, member, and view-row constraints. This fold proves only cross-node limits and selection resolution, re-mints no wire, and exposes the resolved current node as typed optional index evidence.

```python signature
class OrganizationIndex(Struct, frozen=True):
    entities: Map[str, NodeId]
    members: Map[str, NodeId]
    views: Map[str, Block[tuple[str, bool]]]
    current: NodeId | None


NESTS: Final[str] = "nests"
MEMBER: Final[str] = "member"


def _address(key: bytes) -> str:
    return key.hex()


def organization_graph(payload: bytes) -> "RuntimeRail[tuple[GraphPayload, OrganizationIndex]]":
    def build() -> "RuntimeRail[tuple[Any, OrganizationIndex]]":
        wire = Organization.from_binary(payload)
        rows: list[tuple[str, str | None, Entity]] = []
        keys: set[str] = set()
        holdings: list[tuple[str, str]] = []
        stack = [(root, None, 1) for root in reversed(wire.roots)]
        while stack:
            entity, parent, depth = stack.pop()
            if depth > 64:
                return Error(ORG_INVALID.raised("depth"))
            if len(rows) >= 65_536:
                return Error(ORG_INVALID.raised("nodes"))
            address = _address(entity.key)
            if address in keys:
                return Error(ORG_INVALID.raised(f"duplicate:{address}"))
            keys.add(address)
            rows.append((address, parent, entity))
            holdings.extend((address, member) for member in entity.members)
            stack.extend((child, address, depth + 1) for child in reversed(entity.children))

        selected: str | None = None
        if wire.current is not None:
            level = wire.roots
            for index in wire.current.indexes:
                if index >= len(level):
                    return Error(ORG_INVALID.raised("current"))
                current = level[index]
                selected = _address(current.key)
                level = current.children

        validate(wire)
        graph = rx.PyDiGraph(multigraph=False)
        entities = {address: graph.add_node(address) for address, _, _ in rows}
        members = {key: graph.add_node(key) for key in sorted({member for _, member in holdings})}
        views = {
            address: Block.of_seq((override.view, override.visible) for override in entity.overrides)
            for address, _, entity in rows
        }
        graph.add_edges_from([
            (entities[parent], entities[address], NESTS)
            for address, parent, _ in rows
            if parent is not None
        ])
        graph.add_edges_from([(entities[parent], members[member], MEMBER) for parent, member in holdings])
        return Ok((graph, OrganizationIndex(
            entities=Map.of_seq(entities.items()),
            members=Map.of_seq(members.items()),
            views=Map.of_seq(views.items()),
            current=None if selected is None else entities[selected],
        )))

    return boundary(ORG_BUILD, build, catch=_ORG_RAISES).bind(lambda railed: railed).bind(
        lambda built: GraphPayload.of(built[0]).map(lambda payload: (payload, built[1]))
    )
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
