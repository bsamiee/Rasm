# [PY_DATA_GRAPH]

The graph-payload owner over a rustworkx fast-path with a networkx compat row. `GraphPayload` carries graph kind/nodes/edges/attrs/directionality over a `GraphBackend` axis (`rustworkx` `PyGraph`/`PyDiGraph` fast-path, `networkx` `Graph`/`DiGraph` compat); `GraphAlgorithm` is one tagged-union algorithm intent (traversal/shortest-path/centrality/community); `GraphResult` is one discriminated typed receipt whose shape recovers the algorithm class. The backend is recovered from the source shape, never a knob — rustworkx is the default, networkx the interop row when a caller hands a networkx graph or asks for a networkx-only codec. Node-link JSON, GraphML, and tabular edge-list egress emit keyed by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[GRAPH]: graph payloads, backend-dispatched algorithms, typed result receipts, graph egress.

## [02]-[GRAPH]

- Owner: `GraphPayload` — graph kind/nodes/edges/attrs/directionality over a `GraphBackend` axis; `GraphAlgorithm` the tagged-union algorithm intent; `GraphResult` the discriminated typed receipt; node-link JSON / GraphML / tabular edge-list egress.
- Entry: `GraphPayload.of` admits a `rustworkx.PyGraph`/`PyDiGraph` or `networkx.Graph`/`DiGraph` and returns the frozen owner with its backend recovered from the source shape; `GraphPayload.analyze` runs a `GraphAlgorithm` and returns a `RuntimeRail[GraphResult]`; `write_node_link` emits keyed by `ContentIdentity`.
- Auto: arity and filters live in the `GraphAlgorithm` case payload, never a parallel name; the centrality family folds through one `_NX_CENTRALITY` table on the networkx row rather than three sibling arms; the `graph_*`/`digraph_*` prefixed rustworkx variants are the typed dispatch the bare functions select by graph subtype, so the owner never names both.
- Packages: `rustworkx` (`PyGraph`/`PyDiGraph`/`dijkstra_shortest_paths`/`betweenness_centrality`/`closeness_centrality`/`pagerank`/`connected_components`/`strongly_connected_components`/`bfs_successors`/`dfs_edges`/`topological_sort`/`distance_matrix`/`node_link_json`), `networkx` (`Graph`/`DiGraph`/`node_link_data`/`write_graphml`/`from_pandas_edgelist`/`shortest_path`/`betweenness_centrality`/`connected_components`), runtime (`ContentIdentity`/`RuntimeRail`/`ReceiptContributor`).
- Growth: a new graph kind is one `GraphKind` row; a new algorithm is one `GraphAlgorithm` case plus one `_run_rx`/`_run_nx` dispatch arm; a new backend is one `GraphBackend` tag; zero new surface and never a per-algorithm `analyze_*` family.
- Boundary: no product collaboration store, no bridge lifecycle, no compute-package numeric trio; graph algorithms emit typed receipts, not product state. A per-algorithm `get_*` family, a parallel `RustworkxGraph`/`NetworkxGraph` pair, and a generic receipt are the deleted forms.

```python signature
from builtins import frozendict
from typing import TYPE_CHECKING, Literal, assert_never

import networkx as nx
import rustworkx as rx
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from collections.abc import Callable


type NodeId = int
type Weight = float
type RxGraph = rx.PyGraph | rx.PyDiGraph
type NxGraph = nx.Graph | nx.DiGraph
type GraphBackend = Literal["rustworkx", "networkx"]


class GraphKind(Struct, frozen=True):
    directed: bool
    multigraph: bool


@tagged_union(frozen=True)
class GraphAlgorithm:
    tag: Literal[
        "bfs", "dfs", "topo_sort", "shortest_path", "all_pairs_distance", "betweenness", "closeness", "pagerank", "connected", "strongly_connected"
    ] = tag()
    bfs: NodeId = case()
    dfs: NodeId | None = case()
    topo_sort: None = case()
    shortest_path: tuple[NodeId, NodeId] = case()
    all_pairs_distance: float = case()
    betweenness: bool = case()
    closeness: bool = case()
    pagerank: float = case()
    connected: None = case()
    strongly_connected: None = case()

    @staticmethod
    def Bfs(source: NodeId) -> "GraphAlgorithm":
        return GraphAlgorithm(bfs=source)

    @staticmethod
    def Dfs(source: NodeId | None = None) -> "GraphAlgorithm":
        return GraphAlgorithm(dfs=source)

    @staticmethod
    def TopoSort() -> "GraphAlgorithm":
        return GraphAlgorithm(topo_sort=None)

    @staticmethod
    def ShortestPath(source: NodeId, target: NodeId) -> "GraphAlgorithm":
        return GraphAlgorithm(shortest_path=(source, target))

    @staticmethod
    def AllPairsDistance(null_value: float = 0.0) -> "GraphAlgorithm":
        return GraphAlgorithm(all_pairs_distance=null_value)

    @staticmethod
    def Betweenness(normalized: bool = True) -> "GraphAlgorithm":
        return GraphAlgorithm(betweenness=normalized)

    @staticmethod
    def Closeness(wf_improved: bool = True) -> "GraphAlgorithm":
        return GraphAlgorithm(closeness=wf_improved)

    @staticmethod
    def PageRank(alpha: float = 0.85) -> "GraphAlgorithm":
        return GraphAlgorithm(pagerank=alpha)

    @staticmethod
    def Connected() -> "GraphAlgorithm":
        return GraphAlgorithm(connected=None)

    @staticmethod
    def StronglyConnected() -> "GraphAlgorithm":
        return GraphAlgorithm(strongly_connected=None)


@tagged_union(frozen=True)
class GraphResult:
    tag: Literal["order", "path", "scores", "matrix", "partition"] = tag()
    order: tuple[NodeId, ...] = case()
    path: tuple[NodeId, ...] = case()
    scores: tuple[tuple[NodeId, float], ...] = case()
    matrix: tuple[tuple[float, ...], ...] = case()
    partition: tuple[tuple[NodeId, ...], ...] = case()


class GraphPayload(Struct, frozen=True):
    backend: GraphBackend
    kind: GraphKind
    node_count: int
    edge_count: int
    content_key: ContentKey

    @classmethod
    def of(cls, graph: "RxGraph | NxGraph") -> "GraphPayload":
        return _of_rx(graph) if isinstance(graph, rx.PyGraph | rx.PyDiGraph) else _of_nx(graph)

    def analyze(self, graph: "RxGraph | NxGraph", algo: GraphAlgorithm) -> "RuntimeRail[GraphResult]":
        run = _run_rx if self.backend == "rustworkx" else _run_nx
        return boundary(f"graph.analyze.{algo.tag}", lambda: run(graph, algo))

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "graph", self.backend, {"nodes": str(self.node_count), "edges": str(self.edge_count)})


def _of_rx(g: "RxGraph") -> GraphPayload:
    directed = isinstance(g, rx.PyDiGraph)
    return GraphPayload(
        backend="rustworkx",
        kind=GraphKind(directed=directed, multigraph=g.multigraph),
        node_count=g.num_nodes(),
        edge_count=g.num_edges(),
        content_key=ContentIdentity.of("graph", rx.node_link_json(g).encode()),
    )


def _of_nx(g: "NxGraph") -> GraphPayload:
    return GraphPayload(
        backend="networkx",
        kind=GraphKind(directed=g.is_directed(), multigraph=g.is_multigraph()),
        node_count=g.number_of_nodes(),
        edge_count=g.number_of_edges(),
        content_key=ContentIdentity.of("graph", repr(nx.node_link_data(g)).encode()),
    )


def _run_rx(g: "RxGraph", algo: GraphAlgorithm) -> GraphResult:  # noqa: PLR0911
    match algo:
        case GraphAlgorithm(tag="bfs"):
            order = rx.bfs_successors(g, algo.bfs)
            return GraphResult(order=(algo.bfs, *(c for _, kids in order for c in kids)))
        case GraphAlgorithm(tag="dfs"):
            return GraphResult(order=tuple(n for edge in rx.dfs_edges(g, algo.dfs) for n in edge))
        case GraphAlgorithm(tag="topo_sort"):
            return GraphResult(order=tuple(rx.topological_sort(g)))
        case GraphAlgorithm(tag="shortest_path"):
            src, dst = algo.shortest_path
            paths = rx.dijkstra_shortest_paths(g, src, target=dst, weight_fn=float)
            return GraphResult(path=tuple(paths.get(dst, [])))
        case GraphAlgorithm(tag="all_pairs_distance"):
            mat = rx.distance_matrix(g, null_value=algo.all_pairs_distance)
            return GraphResult(matrix=tuple(tuple(row) for row in mat.tolist()))
        case GraphAlgorithm(tag="betweenness"):
            return GraphResult(scores=tuple(rx.betweenness_centrality(g, normalized=algo.betweenness).items()))
        case GraphAlgorithm(tag="closeness"):
            return GraphResult(scores=tuple(rx.closeness_centrality(g, wf_improved=algo.closeness).items()))
        case GraphAlgorithm(tag="pagerank"):
            return GraphResult(scores=tuple(rx.pagerank(g, alpha=algo.pagerank).items()))
        case GraphAlgorithm(tag="connected"):
            return GraphResult(partition=tuple(tuple(c) for c in rx.connected_components(g)))
        case GraphAlgorithm(tag="strongly_connected"):
            return GraphResult(partition=tuple(tuple(c) for c in rx.strongly_connected_components(g)))
        case unreachable:
            assert_never(unreachable)


_NX_CENTRALITY: "frozendict[str, Callable[[NxGraph], dict[NodeId, float]]]" = frozendict({
    "betweenness": nx.betweenness_centrality,
    "closeness": nx.closeness_centrality,
    "pagerank": nx.pagerank,
})


def _run_nx(g: "NxGraph", algo: GraphAlgorithm) -> GraphResult:  # noqa: PLR0911
    match algo:
        case GraphAlgorithm(tag="bfs"):
            return GraphResult(order=tuple(nx.bfs_tree(g, algo.bfs).nodes()))
        case GraphAlgorithm(tag="dfs"):
            return GraphResult(order=tuple(nx.dfs_preorder_nodes(g, algo.dfs)))
        case GraphAlgorithm(tag="topo_sort"):
            return GraphResult(order=tuple(nx.topological_sort(g)))
        case GraphAlgorithm(tag="shortest_path"):
            src, dst = algo.shortest_path
            return GraphResult(path=tuple(nx.shortest_path(g, src, dst, weight="weight")))
        case GraphAlgorithm(tag="all_pairs_distance"):
            order = list(g.nodes())
            lengths = dict(nx.all_pairs_dijkstra_path_length(g, weight="weight"))
            null = algo.all_pairs_distance
            return GraphResult(matrix=tuple(tuple(lengths.get(r, {}).get(c, null) for c in order) for r in order))
        case GraphAlgorithm(tag="betweenness" | "closeness" | "pagerank"):
            return GraphResult(scores=tuple(_NX_CENTRALITY[algo.tag](g).items()))
        case GraphAlgorithm(tag="connected"):
            comp = nx.weakly_connected_components(g) if g.is_directed() else nx.connected_components(g)
            return GraphResult(partition=tuple(tuple(c) for c in comp))
        case GraphAlgorithm(tag="strongly_connected"):
            return GraphResult(partition=tuple(tuple(c) for c in nx.strongly_connected_components(g)))
        case unreachable:
            assert_never(unreachable)


def write_node_link(payload: GraphPayload, graph: "RxGraph | NxGraph") -> "RuntimeRail[bytes]":
    encode = (lambda: rx.node_link_json(graph).encode()) if payload.backend == "rustworkx" else (lambda: repr(nx.node_link_data(graph)).encode())
    return boundary("graph.egress", encode)
```
