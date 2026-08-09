# [PY_DATA_GRAPH]

One graph-payload owner over a license-split backend triangle: the permissive `rustworkx` analysis core, the `networkx` codec/egress lane, and the GPL-confined `igraph` community engine carrying the Leiden/Louvain/Infomap split rustworkx lacks and the BSD core cannot license. Its backend is recovered from the source shape, never a knob, and analysis collapses onto ONE kernel: every algorithm runs on `rustworkx` keyed by its stable non-recycled integer index, a `networkx` or `igraph` source converting once through the one-way `_as_rx` bridge, so the `NodeId` stays the rx `int` the node-keyed frame seam joins on.

Payload identity is the railed `ContentIdentity` fingerprint over the canonical node-link wire, never a `repr(dict)` byte stream. `GraphResult.frame` lowers node-index-keyed results into one canonical `node`-keyed `pa.Table` the `tabular/columnar#SCAN` plane left-joins by `node` — a centrality run is a left-join enrichment, never a re-keyed copy. `igraph`'s GPL core stays in this data graph rail and is never linked into a host-distributed plugin. `organization_graph` folds the C#-minted `rasm.organization.v1` organization document into the containment graph, decoding through the branch's one wire-shape owner at `runtime/transport/shapes#VOCABULARY` and minting no wire struct here; the capacity-network flow family rides the sibling `graph/network#NETWORK` owner over the networkx lane this kernel does not spell.

## [01]-[INDEX]

- [02]-[GRAPH]: the `GraphPayload` owner — one rustworkx kernel over `_as_rx`-coerced sources, family-folded algorithm intent, typed result receipts, content-keyed egress.
- [03]-[TOPOLOGY]: `organization_graph` containment fold — wire-carried organizational entities and containment edges folded onto the one kernel, `OrganizationIndex` carrying one address-to-index map per key space.

## [02]-[GRAPH]

- Owner: `GraphPayload` carries the admitted graph as `self.graph`, so the recovered `backend`/`kind` never decouple from the analyzed graph — `analyze`/`write` take no graph parameter; a re-passed handle breaking that invariant is the rejected form.
- Cases: payload splits follow real provider arity — `all_pairs_distance` carries the `null_value` its `distance_matrix` substrate declares while `floyd_warshall` carries only its `WeightSelector` (`floyd_warshall_numpy` takes `weight_fn`, no null-value parameter); the connectivity polarity is recovered from `kind.directed`, never a caller flag; every weighted member carries a `WeightSelector` slot defaulting `WEIGHT_IDENTITY`, so a non-float edge payload is weightable by one policy value.
- Entry: `analyze` absorbs a lone `GraphAlgorithm` or a `Block` over one `match` at the head — the arity is the value's shape, the `Disposition` selects the batch output shape through the `@overload` ladder and is inert for a lone algorithm, so the input shape and the disposition together carry the output type. A non-node-keyed result case carries no per-node row, so `frame` names the case as non-node-keyed rather than minting a degenerate frame; `write` routes the `_EGRESS` codec directly on the source backend, never through the analysis-coercion path.
- Auto: the bare-name rustworkx members dispatch on graph subtype, so the owner never names the `graph_*`/`digraph_*` typed forms; the dense matrices stay `npt.NDArray[np.float64]` so they fold straight into the tensor carriers.
- Receipt: the content key derives once at admission from the canonical node-link wire and the receipt reuses it — an unchanged graph keys byte-stable, an added edge re-admits to a new key; the algorithm receipt is typed rail evidence, never product graph-database state. `contribute` projects node/edge counts onto the runtime `Metrics.record` arm under `domain="graph"` keyed by algorithm, and `_one` opens the kernel span — the no-scrape analysis engine's whole observability surface, the runtime fence marking the span on a failed leg.
- Packages: `pyarrow` binds function-local, so the codec-only graph path never loads Arrow.
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
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, boundary, scoped, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.transport.shapes import OrganizationWire

if TYPE_CHECKING:
    from collections.abc import Callable

    import igraph
    import numpy.typing as npt
    import pyarrow as pa


# --- [TYPES] ----------------------------------------------------------------------------

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.graph")

type NodeId = int
type RxGraph = rx.PyGraph | rx.PyDiGraph
type NxGraph = nx.Graph | nx.DiGraph
# GPL igraph members are TYPE_CHECKING-only, so the alias is checker-facing while the runtime
# carrier slot on `GraphPayload` stays the honest `Any` wire floor.
type AnyGraph = "RxGraph | NxGraph | igraph.Graph"
type GraphBackend = Literal["rustworkx", "networkx", "igraph"]
type ScoreMap = tuple[tuple[NodeId, float], ...]
type Partition = tuple[tuple[NodeId, ...], ...]
type Matrix = npt.NDArray[np.float64]
type WeightSelector = Callable[[Any], float]

WEIGHT_IDENTITY: Final[WeightSelector] = float

# leiden's objective is a real engine choice, not a call-site literal: under CPM the resolution is a DENSITY
# threshold and under modularity a SCALE factor, so one number means two things and the row must say which.
type LeidenObjective = Literal["CPM", "modularity"]
# edge attribute `Graph.TupleList(weights=True)` writes, and the key every weighted community member reads back.
_IG_WEIGHT: Final[str] = "weight"


class GraphFormat(StrEnum):
    NODE_LINK = "node_link"
    GRAPHML = "graphml"
    EDGE_LIST = "edge_list"


class LayoutKind(StrEnum):
    SPRING = "spring"
    CIRCULAR = "circular"
    KAMADA_KAWAI = "kamada_kawai"


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
    bfs: NodeId = case()
    dfs: NodeId | None = case()
    topo_sort: None = case()
    ancestors: NodeId = case()
    descendants: NodeId = case()
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
    # community rows carry the branch `WeightSelector` exactly as every other weighted member does. Without it the
    # three arms built their graph off a bare edge list, so a weighted network partitioned as if every edge were
    # unit — the one input community detection exists to read — and neither the objective nor the refinement
    # iteration count was reachable at all. Each row seats its selector at slot 1, so one projection feeds the build.
    leiden: "tuple[float, WeightSelector, LeidenObjective, int]" = case()
    louvain: tuple[float, WeightSelector] = case()
    infomap: tuple[int, WeightSelector] = case()


@tagged_union(frozen=True)
class GraphResult:
    tag: Literal[
        "order", "path", "paths", "scores", "matrix", "partition", "tree", "coloring", "matching", "layout", "scalar", "flag", "flows"
    ] = tag()
    order: tuple[NodeId, ...] = case()
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
    # edge-keyed flow assignment the `graph/network#NETWORK` owner mints: (source, target, flow) per edge.
    flows: tuple[tuple[NodeId, NodeId, float], ...] = case()

    def frame(self) -> "RuntimeRail[pa.Table]":
        return boundary(f"graph.frame.{self.tag}", lambda: _frame(self))


class GraphReceipt(Struct, frozen=True, gc=False):
    backend: GraphBackend
    kind: GraphKind
    node_count: int
    edge_count: int
    algorithm: str
    result: str
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        # receipts stay truth, instruments stay projections: every kernel run lands its structure sizes on the metric
        # spine under domain="graph", keyed by algorithm tag — the no-scrape rustworkx kernel's only metric surface.
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
        # the kernel span is the run's trace surface; the boundary fence inside marks it ERROR + record_exception on a raise.
        with _TRACER.start_as_current_span(
            f"graph.analyze.{algo.tag}",
            attributes={"rasm.graph.algorithm": algo.tag, "rasm.graph.backend": self.backend, "rasm.graph.nodes": self.node_count},
        ):
            return boundary(f"graph.analyze.{algo.tag}", lambda: _run_rx(_as_rx(self.graph), algo, self.kind))

    def write(self, fmt: GraphFormat) -> "RuntimeRail[bytes]":
        return boundary(f"graph.egress.{fmt}", lambda: _EGRESS[self.backend][fmt](self.graph))

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
    # `node_link_data` is the canonical persisted graph document, encoded through the shared `msgspec` JSON rail.
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
    # STRUCTURAL GPL confinement: the probe reads `sys.modules` and never imports — an igraph
    # source can only exist in-process if the caller already linked the GPL core, so a run that
    # never sees one never loads igraph; module-top `import igraph` is the deleted form.
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


def _frame(result: GraphResult) -> "pa.Table":  # ruff:ignore[too-many-return-statements]
    # `pyarrow` is module-level-import-banned; the deferred import rides the same boundary the
    # columnar/interop owners bind `pl`/`read_excel` under.
    import pyarrow as pa  # ruff:ignore[import-outside-top-level]

    match result:
        case GraphResult(tag="scores", scores=rows):
            return pa.Table.from_pydict({"node": [n for n, _ in rows], "value": [v for _, v in rows]})
        case GraphResult(tag="coloring", coloring=rows):
            return pa.Table.from_pydict({"node": [n for n, _ in rows], "color": [c for _, c in rows]})
        case GraphResult(tag="partition", partition=blocks):
            return pa.Table.from_pydict({
                "node": [n for block in blocks for n in block],
                "component": [i for i, block in enumerate(blocks) for _ in block],
            })
        case GraphResult(tag="order", order=nodes):
            return pa.Table.from_pydict({"node": list(nodes), "rank": list(range(len(nodes)))})
        case GraphResult(tag="layout", layout=rows):
            return pa.Table.from_pydict({"node": [n for n, _ in rows], "x": [xy[0] for _, xy in rows], "y": [xy[1] for _, xy in rows]})
        case GraphResult(tag="flows", flows=rows):
            # EDGE-keyed frame: the join keys on the (source, target) pair rather than the lone `node` column.
            return pa.Table.from_pydict({
                "source": [u for u, _, _ in rows],
                "target": [v for _, v, _ in rows],
                "flow": [f for _, _, f in rows],
            })
        case GraphResult(tag="tree", tree=rows) | GraphResult(tag="matching", matching=rows):
            # EDGE-keyed exactly as `flows` is: both carry the `(source, target)` pair every edge result joins on, so
            # refusing them claimed an absent index row while holding the very index the flow arm frames.
            return pa.Table.from_pydict({"source": [u for u, _ in rows], "target": [v for _, v in rows]})
        case _:
            raise ValueError(f"{result.tag} carries no index row; only scores/coloring/partition/order/layout/tree/matching/flows key a join table")


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
    # one analysis-coercion seam. `networkx_converter`'s `keep_attributes` default rides the original label as node
    # payload so the rx index stays the stable join key; the nx->rx bridge is the only converter direction, so the igraph
    # leg crosses networkx first (`to_networkx` then convert).
    match graph:
        case rx.PyGraph() | rx.PyDiGraph():
            return graph
        case _ if _is_ig(graph):
            return rx.networkx_converter(graph.to_networkx())
        case _:
            return rx.networkx_converter(graph)


def _run_rx(g: RxGraph, algo: GraphAlgorithm, kind: GraphKind) -> GraphResult:  # ruff:ignore[too-many-return-statements, complex-structure]
    match algo:
        case GraphAlgorithm(tag="bfs"):
            return GraphResult(order=(algo.bfs, *(c for _, kids in rx.bfs_successors(g, algo.bfs) for c in kids)))
        case GraphAlgorithm(tag="dfs"):
            return GraphResult(order=tuple(n for edge in rx.dfs_edges(g, algo.dfs) for n in edge))
        case GraphAlgorithm(tag="topo_sort"):
            return GraphResult(order=tuple(rx.topological_sort(g)))
        case GraphAlgorithm(tag="ancestors"):
            return GraphResult(order=tuple(rx.ancestors(g, algo.ancestors)))
        case GraphAlgorithm(tag="descendants"):
            return GraphResult(order=tuple(rx.descendants(g, algo.descendants)))
        case GraphAlgorithm(tag="shortest_path", shortest_path=(src, dst, weight)):
            # `PathMapping` is a `__contains__`/`__getitem__` view, not a `dict` — `.get` does not
            # exist, so the membership-gated subscript reads the path or the empty unreachable path.
            paths = rx.dijkstra_shortest_paths(g, src, target=dst, weight_fn=weight)
            return GraphResult(path=tuple(paths[dst]) if dst in paths else ())
        case GraphAlgorithm(tag="bellman_ford", bellman_ford=(src, dst, weight)):
            paths = rx.bellman_ford_shortest_paths(g, src, target=dst, weight_fn=weight)
            return GraphResult(path=tuple(paths[dst]) if dst in paths else ())
        case GraphAlgorithm(tag="astar", astar=(src, dst, edge_cost, estimate)):
            # carried selector pair: `edge_cost` reads the edge payload, `estimate` the node —
            # admissible-heuristic policy is DATA on the case, never a hardcoded unit lambda.
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
            # `transitive_reduction` returns the `(reduced_graph, index_map)` pair, not a bare graph.
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
            # `tree` is this union's EDGE-LIST carrier — the transitive reduction, the condensation, and both
            # spanning trees all land there — so a bridge set lands there too. Reporting it as `matching` named the
            # receipt's `result` for a pairing the algorithm never computes.
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
            # `pagerank` is rustworkx-directed-only — a `pagerank` run on an undirected `PyGraph`
            # raises `TypeError`, which the enclosing `boundary` fence rails to a `BoundaryFault`
            # rather than crashing; the other five centralities run on either graph kind.
            return GraphResult(scores=tuple(RX_CENTRALITY[algo.tag](g, algo).items()))
        case GraphAlgorithm(tag="hits"):
            # `hits` is rustworkx-directed-only (the boundary fence rails an undirected misuse).
            hubs, _ = rx.hits(g, max_iter=algo.hits)
            return GraphResult(scores=tuple(hubs.items()))
        case GraphAlgorithm(tag="greedy_color"):
            return GraphResult(coloring=tuple(rx.graph_greedy_color(g, strategy=rx.ColoringStrategy.Saturation).items()))
        case GraphAlgorithm(tag="max_weight_matching", max_weight_matching=(max_cardinality, weight)):
            # rustworkx matching demands an int weight — the carried float selector quantizes at the
            # call head, so the policy stays one selector row rather than a parallel int selector kind.
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
            # every community row seats its selector at slot 1, so the build reads ONE projection rather than three
            # arms restating a lookup the case shape already fixes.
            return _run_ig(_ig_from(g, kind, getattr(algo, algo.tag)[1]), algo, kind)
        case unreachable:
            assert_never(unreachable)


# --- [IGRAPH_COMMUNITY] -----------------------------------------------------------------

# community rows call methods ON the passed C-core graph, so the table itself links no GPL
# symbol — the one import site is `_ig_from`, reached only from the `_run_rx` community arm.
IG_COMMUNITY: "Final[Map[str, Callable[[igraph.Graph, GraphAlgorithm], igraph.VertexClustering]]]" = Map.of_seq([
    ("leiden", lambda g, a: g.community_leiden(objective_function=a.leiden[2], resolution=a.leiden[0], weights=_IG_WEIGHT, n_iterations=a.leiden[3])),
    ("louvain", lambda g, a: g.community_multilevel(resolution=a.louvain[0], weights=_IG_WEIGHT)),
    ("infomap", lambda g, a: g.community_infomap(trials=a.infomap[0], edge_weights=_IG_WEIGHT)),
])


def _ig_from(g: RxGraph, kind: GraphKind, weight: WeightSelector) -> "igraph.Graph":
    # ONE GPL import site, function-local by law — the community split is the only leg that
    # links the igraph C core, so the confinement is structural rather than prose. The leg builds
    # C-core graph builds from the rustworkx edge list (`_as_rx` already coerced any networkx/igraph
    # source to rustworkx, so the edge list is always rx integer indices). `Graph.TupleList`'s
    # default `vertex_name_attr="name"` stores each rx endpoint index in the `name` vertex
    # attribute, so the membership read recovers the rx index — robust to rx index gaps after node
    # removal — rather than igraph's reindexed 0-based vertex. `TupleList` creates a vertex only
    # per endpoint, so an ISOLATED rx node carries no edge and would vanish from the partition;
    # `add_vertices` re-admits the edgeless rx indices as `name`-carrying singleton
    # vertices so the community partition stays TOTAL over the node set.
    import igraph  # ruff:ignore[import-outside-top-level]

    # `weighted_edge_list` carries each edge PAYLOAD, which the branch selector lowers to the float igraph reads
    # back off `_IG_WEIGHT` — `weights=True` is what makes `TupleList` write that attribute from the third slot.
    # Building off a bare `edge_list()` discards that payload, so every weighted community run silently partitions a
    # unit graph. The name set hoists out of the scan, which rebuilt it once per node.
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
            # `VertexClustering` membership is keyed by igraph's reindexed 0-based vertex; `_ig_from`'s
            # `TupleList`+`add_vertices` carries each rx index in the `name` attribute, so the partition
            # lowers back onto the rx index the rest of the rail (and `GraphResult.frame`) joins on.
            names = g.vs["name"]
            return GraphResult(partition=tuple(tuple(names[v] for v in block) for block in IG_COMMUNITY[algo.tag](g, algo)))
        case off_lane:
            # totality arm: `_run_rx` routes only the community tags here, so this raise fires only for a future direct
            # caller — loud at the fence, never a silent partial backend.
            raise NotImplementedError(f"igraph backend owns only the community split, not {off_lane.tag}; route to rustworkx")


# --- [COMPOSITION] ----------------------------------------------------------------------


def _graphml(write: "Callable[[str], object]") -> bytes:
    # GraphML is path-keyed on every backend (`rx.write_graphml(g, path)`, `nx.write_graphml(g, path)`);
    # one helper reads the written document back through a scratch path rather than re-encoding
    # through a foreign codec or an unconfirmed byte-streaming variant.
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
    rxk --> result["GraphResult: order·path·scores·matrix·partition·tree·coloring·matching·layout·scalar·flag"]
    igk --> result
    payload -->|analyze Block: traversed by Disposition| batch["RuntimeRail[Block[GraphResult]] | (results, faults)"]
    payload -->|write over GraphFormat: codec/egress lane| egress["_EGRESS[backend][fmt]: node_link·graphml·edge_list bytes"]
    result -->|frame: node-keyed cases| node_frame["RuntimeRail[pa.Table] node·value/color/component/rank/x·y"]
    node_frame -->|columnar#SCAN pa.Table.join keys=node left outer| enrich["node-attribute enrichment"]
    result -->|receipt| receipt["GraphReceipt"]
    key --> receipt
    receipt -->|contribute| sink["runtime ReceiptContributor"]
```

## [03]-[TOPOLOGY]

- Owner: `organization_graph` folds one decoded `OrganizationWire` into the graph plane's node-link source — nodes the organizational addresses and the federation member keys, directed edges the nesting and membership containment — returning the `GraphPayload` beside the `OrganizationIndex` a caller keys its queries and the `GraphResult.frame` left-join through.
- Law: schema and codec MINT in C# beside `csharp:Rasm.Rhino/Document/layers#ORGANIZATION_PROJECTION`, and `runtime/transport/shapes#VOCABULARY` is this branch's ONE wire-shape owner, so no struct mirrors the document here. Names on that wire state the host-free organizational concept, so this fold reads organizational addresses and federation keys and never a host layer handle, table index, or joined path.
- Law: key SPACES stay separated — `OrganizationIndex.entities` maps content-addressed organizational addresses and `OrganizationIndex.members` federation keys the producing authority issued. One merged map lets an authority-issued key spelling a 32-hex address collide with an entity, silently re-pointing a containment query at the wrong node.
- Law: content-key spelling lowers exactly once at this decode — the wire carries 16 big-endian bytes and this branch's own key face is lowercase hex, so a consumer joining an address against any peer lowers and never uppercases.
- Entry: containment ancestry is `analyze(GraphAlgorithm(ancestors=...))`, membership closure `descendants`, nesting depth the `bfs` order — the existing kernel answers every organizational query with zero new algorithm surface, and the node-keyed frame left-joins organization onto the scan plane by `node` exactly as every enrichment does.
- Growth: one `ContainmentWire` target arm carries a new containment relation as one edge-payload literal and one dispatch row; a new presentation axis rides the decoded overrides untouched, since presentation evidence enters no edge.
- Boundary: decode only — this plane re-mints no wire and answers no render or print product query, which stays producer-side evidence. Sibling ordinal rides the decoded entity rows rather than a node payload, because rank orders siblings and carries no edge. Overrides stay detached on the decoded value, so containment analysis reads one topology whichever view a consumer audits.
- Boundary: containment edges naming an absent container or an absent entity target refuse typed at the fold, mirroring the emitter's own orphan refusal; a member target names a FOREIGN key space and always mints its node, since an unresolvable member is the consuming plane's join miss rather than wire damage.

```python signature
class OrganizationIndex(Struct, frozen=True):
    # one address-to-index map per key space the wire discriminates — never one merged map a foreign key can shadow.
    entities: Map[str, NodeId]
    members: Map[str, NodeId]


# edge payload literals: the containment vocabulary the organizational queries and the frame join read.
NESTS: Final[str] = "nests"
MEMBER: Final[str] = "member"


def _address(key: bytes) -> str:
    # wire keys cross as 16 big-endian bytes and this branch faces them lowercase, so the one lowering seats here.
    return key.hex()


def organization_graph(wire: OrganizationWire) -> "RuntimeRail[tuple[GraphPayload, OrganizationIndex]]":
    def build() -> "tuple[Any, OrganizationIndex]":
        graph = rx.PyDiGraph(multigraph=False)
        entities = {_address(entity.key): graph.add_node(_address(entity.key)) for entity in wire.entities}
        nests = tuple((edge.container, edge.entity) for edge in wire.containment if edge.entity)
        holds = tuple((edge.container, edge.member) for edge in wire.containment if edge.member)
        orphans = tuple(
            _address(container)
            for container, target in nests + holds
            if _address(container) not in entities
        ) + tuple(_address(target) for _, target in nests if _address(target) not in entities)
        if orphans:
            # mirror of the emitter's own orphan refusal: a containment key outside the entity set is wire damage,
            # never a droppable edge — the boundary fence rails this into the typed fault.
            raise ValueError(f"<orphan-containment:{orphans}>")
        # distinct-first: a member key two entities both hold mints ONE node — a per-occurrence add_node strands
        # duplicates behind the last-written index, and sorted assignment keeps indices stable across reads.
        members = {key: graph.add_node(key) for key in sorted({member for _, member in holds} - set(entities))}
        graph.add_edges_from([(entities[_address(c)], entities[_address(t)], NESTS) for c, t in nests])
        graph.add_edges_from([(entities[_address(c)], members[m], MEMBER) for c, m in holds])
        return graph, OrganizationIndex(entities=Map.of_seq(entities.items()), members=Map.of_seq(members.items()))

    return boundary("graph.organization.build", build).bind(
        lambda built: GraphPayload.of(built[0]).map(lambda payload: (payload, built[1]))
    )
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
