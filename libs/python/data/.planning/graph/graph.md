# [PY_DATA_GRAPH]

The graph-payload owner over a permissive-license `rustworkx` analysis core, a `networkx` codec/egress lane, and a GPL-confined `igraph` community-detection engine. `GraphPayload` carries the recovered backend, the directed/multigraph `GraphKind`, the node/edge counts, and the content-key seam over a `GraphBackend` axis; `GraphAlgorithm` is one tagged-union algorithm intent folded by family (traversal, shortest-path, all-pairs, DAG, connectivity, cut, centrality, coloring, matching, spanning, structure, layout, community); `GraphResult` is one discriminated typed receipt whose case recovers the algorithm class and parameterizes the output (node order, path, score map, dense `numpy` matrix, partition, tree, coloring, matching, 2-D layout, scalar, flag). The backend is recovered from the source shape, never a knob, and the analysis axis collapses onto ONE kernel: every algorithm runs on `rustworkx` keyed by its stable non-recycled integer index, a `networkx` or `igraph` source converting once through the catalogue's one-way `rx.networkx_converter` bridge (`_as_rx`) so analysis never forks a second kernel and the `NodeId` stays the rx `int` the node-keyed frame seam joins on. `networkx` owns the read-side codec/egress lane (`node_link_data`/`write_graphml`/`to_pandas_edgelist`), `igraph` the Leiden/Louvain/Infomap community split rustworkx lacks and the BSD core cannot license. Every analysis rides one `boundary`-fenced rail that records on the active span; payload identity is the railed `ContentIdentity` fingerprint over the canonical node-link wire, and a run streams onto the runtime `ReceiptContributor` rail.

## [01]-[INDEX]

- [01]-[GRAPH]: graph payloads, the single rustworkx analysis kernel over `_as_rx`-coerced sources, family-folded algorithm intent, typed result receipts, content-keyed graph egress, and the igraph community-detection split.

## [02]-[GRAPH]

- Owner: `GraphPayload` — the recovered `GraphBackend`/`GraphKind`/counts/content-key value object over one backend axis; `GraphAlgorithm` the family-folded tagged-union intent; `GraphResult` the discriminated typed receipt parameterizing the output shape, owning the `frame` lowering of its node-index-keyed cases into the canonical `node`-keyed Arrow seam; `GraphFormat` the closed egress-format axis; `GraphReceipt` the typed `ReceiptContributor` evidence keyed by `ContentIdentity` over the canonical node-link fingerprint. A new graph kind is one `GraphKind` field, a new analysis algorithm one `GraphAlgorithm` case plus one `_run_rx` arm (community algorithms one `IG_COMMUNITY` row the existing community arm folds), a new egress one `GraphFormat` row plus one `_EGRESS` codec row, a new node-keyed enrichment one `GraphResult.frame` arm — never a per-algorithm `analyze_*`/`get_*` family, never a parallel `RustworkxGraph`/`NetworkxGraph`/`IgraphGraph` carrier triple, and never a second per-backend analysis kernel where `_as_rx` coerces every source onto the one rustworkx core.
- Cases: `GraphAlgorithm` is the one intent axis closed by `assert_never`. The traversal family (`bfs`/`dfs`/`topo_sort`/`ancestors`/`descendants`) carries a source `NodeId` or `None`; the shortest-path family (`shortest_path`/`bellman_ford`/`astar`/`k_shortest`/`all_simple_paths`) carries the `(source, target)` pair plus the path policy; the all-pairs family (`all_pairs_distance`/`floyd_warshall`) carries the `null_value`; the DAG family (`longest_path`/`transitive_reduction`/`dominators`) carries the optional root; the connectivity family (`connected`/`strongly_connected`/`articulation`/`bridges`/`cycle_basis`/`condensation`/`core_number`) recovers the weak-vs-strong component polarity from `kind.directed` in the `connected` arm rather than a separate tag; the cut family (`min_cut`) carries the weight selector; the centrality family (`betweenness`/`closeness`/`eigenvector`/`katz`/`pagerank`/`hits`/`degree`) carries its tuning scalar and folds through the one `RX_CENTRALITY` table rather than one arm per metric; the structure family (`greedy_color`/`max_weight_matching`/`spanning_tree`/`steiner_tree`/`transitivity`/`is_planar`) carries its parameters; the `layout` family carries the `LayoutKind`; the community family (`leiden`/`louvain`/`infomap`) carries its resolution and routes to `igraph`. Arity, filters, weight selector, normalization, and resolution live in the case payload, never a parallel name.
- Entry: `GraphPayload.of` admits a `rustworkx.PyGraph`/`PyDiGraph`/`PyDAG`, a `networkx.Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph`, or an `igraph.Graph`, recovers the backend and `GraphKind` from the source shape, and returns a `RuntimeRail[GraphPayload]` because the canonical-encode content-key seam is fallible. `GraphPayload.analyze` is the one modality-polymorphic analysis entrypoint: it absorbs a lone `GraphAlgorithm` or a `Block[GraphAlgorithm]` over one `match` at the head, recovering the singular-versus-batch disposition from the algorithm value's shape rather than an `analyze_many` sibling or a `mode` flag — the same arity collapse the `gridded/store#STORE` `write_region` and `gridded/virtual#VIRTUAL` `register` arms run on the slab count. The owner carries the admitted graph as `self.graph` so the recovered `backend`/`kind` never decouple from the analyzed graph — `analyze`/`write` take no graph parameter, and a re-passed handle that could break the backend/kind invariant is the deleted form. A lone algorithm threads the private `_one` kernel fence (`boundary(f"graph.analyze.{algo.tag}", ...)`) that coerces the admitted graph onto the one rustworkx analysis core through `_as_rx` (identity on a native rustworkx graph, `rx.networkx_converter` on a networkx source, `to_networkx` then `networkx_converter` on an igraph source) and returns `RuntimeRail[GraphResult]` over the one discriminated receipt whose case the caller matches — the result is parameterized over its case (a `pagerank` run carries `scores`, a `connected` run `partition`, an all-pairs run the dense `matrix`), so the output shape is the union case rather than a stringly `tuple`, and a centrality run over a networkx source keys on the converted rustworkx index rather than the arbitrary-hashable networkx label. A `Block` maps each algorithm through `_one` and folds the rails through `traversed(rails, by=by)`, the `Disposition` row selecting the multi-algorithm output shape through the `@overload` ladder mirroring the faults owner's `traversed` (`ABORT`/`ACCUMULATE` collapse to `RuntimeRail[Block[GraphResult]]`, `PARTITION` returns the `RuntimeRail[tuple[Block[GraphResult], Block[BoundaryFault]]]` split a health sweep reads) — so the input shape and the disposition together carry the output type, and `by` is inert for a lone algorithm. This is the genuine input-and-output parameterized surface: one entrypoint, the arity recovered from the value, the disposition the only knob and only on the batch arm. `GraphResult.frame` lowers a node-index-keyed result — `scores` (centrality/k-shortest/dominators/core-number/hits), `coloring`, `partition` (components/community membership), `order` (traversal/topo/articulation), and `layout` — into a `RuntimeRail[pa.Table]` carrying one canonical `node` column keyed by the stable non-recycled rustworkx integer index plus the per-case value column(s), so the `tabular/columnar#SCAN` plane left-joins a node-attribute `pa.Table` by `node` (`pa.Table.join(other, keys=["node"], join_type="left outer")`) and a centrality run is a left-join enrichment rather than a re-keyed copy; the produced frame rides the `tabular/interop#INTEROP` `ArrowCStream.of` carrier zero-copy at the downstream hop exactly as the `columnar#SCAN` `Corpus` arm does. A non-node-keyed case (`path`/`paths`/`matrix`/`tree`/`matching`/`scalar`/`flag`) carries no per-node index row, so the `boundary` names the case as non-node-keyed rather than minting a degenerate frame. `GraphPayload.write` routes the `_EGRESS` codec by the recovered `GraphFormat` over `self.graph` directly on the source backend — the codec/egress lane `networkx` and `igraph` own — inside one `boundary` fence, never through the analysis-coercion path.
- Auto: the analysis axis is ONE kernel — `_run_rx` over the rustworkx core — and `_as_rx` is the one coercion seam folding every backend onto it (identity on rustworkx, `networkx_converter` on networkx, `to_networkx`+`networkx_converter` on igraph), never a per-backend analysis kernel triple where the converter already exists. The centrality family folds through one `RX_CENTRALITY` `Final[Map[...]]` behavior table keyed by the algorithm tag rather than seven sibling arms — the `expression` `Map.of_seq` dispatch the sibling `tabular/contract#QUALITY` `_CMP` and `tabular/interop#INTEROP` `_BACKEND` rows own, never a `from builtins import frozendict` table; the `graph_*`/`digraph_*` prefixed rustworkx variants are the typed dispatch the bare names select by graph subtype, so the owner names only the bare form; the connectivity polarity (`connected` vs `weakly_connected`) is recovered from `kind.directed` through one fold, never a caller flag; the community family delegates INSIDE `_run_rx` to the `IG_COMMUNITY` method-binding `Map` on an `igraph.Graph` built off the converted rustworkx edge list, exactly as the sibling `tabular/profile#PROFILE` `ProbeTables` binds the unbound `pb.Validate` methods, never a detector class per algorithm and never a separate igraph kernel route. The dense `all_pairs_distance`/`floyd_warshall` matrices and the `adjacency`/`distance` exports stay `numpy`-typed `npt.NDArray[np.float64]` so they fold straight into the `pyarrow`/`numpy` tensor carriers.
- Receipt: `GraphReceipt.contribute` emits an `emitted`-phase `Receipt.of` row keyed by `ContentIdentity` over the payload `content_key` — `GraphPayload.of` derives that key once at admission from the canonical node-link wire (the backend's own `node_link_json`/`node_link_data`, never a `repr(dict)` byte stream), so the receipt reuses the admitted key rather than re-deriving over a re-passed graph; an unchanged graph reuses its key byte-stable while an added edge re-admits to a new key. The receipt carries the backend, the `(directed, multigraph)` kind, the node/edge counts, the algorithm tag, and the result discriminant as one typed evidence stream the metrics/lanes fold reads, exactly as the sibling `tabular/profile#PROFILE` `ProfileReceipt` and `tabular/columnar#SCAN` `QueryReceipt` do. The algorithm receipt is the typed evidence the graph rail emits, never product graph-database state.
- Packages: `rustworkx` (`PyGraph`/`PyDiGraph`/`PyDAG`/`num_nodes`/`num_edges`/`multigraph`/`edge_list`/`bfs_successors`/`dfs_edges`/`topological_sort`/`ancestors`/`descendants`/`dijkstra_shortest_paths`/`bellman_ford_shortest_paths`/`astar_shortest_path`/`k_shortest_path_lengths`/`all_simple_paths`/`distance_matrix`/`floyd_warshall_numpy`/`dag_longest_path`/`transitive_reduction`/`immediate_dominators`/`connected_components`/`strongly_connected_components`/`weakly_connected_components`/`articulation_points`/`bridges`/`cycle_basis`/`condensation`/`core_number`/`stoer_wagner_min_cut`/`betweenness_centrality`/`closeness_centrality`/`eigenvector_centrality`/`katz_centrality`/`pagerank`/`hits`/`degree_centrality`/`graph_greedy_color`/`ColoringStrategy`/`max_weight_matching`/`minimum_spanning_tree`/`steiner_tree`/`transitivity`/`is_planar`/`spring_layout`/`circular_layout`/`kamada_kawai_layout`/`networkx_converter` (the one-way nx->rx analysis-coercion bridge `_as_rx` threads)/`node_link_json`/`write_graphml`/`NoPathFound`/`DAGHasCycle`/`NullGraph`/`FailedToConverge`), `networkx` (codec/egress lane only: `Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph`/`number_of_nodes`/`number_of_edges`/`is_directed`/`is_multigraph`/`edges`/`node_link_data`/`write_graphml`/`to_pandas_edgelist`/`NetworkXException`), `igraph` (`Graph.TupleList`/`is_directed`/`has_multiple`/`vcount`/`ecount`/`community_leiden`/`community_multilevel`/`community_infomap`/`VertexClustering.membership`/`VertexClustering.modularity`/`to_networkx`/`get_edge_dataframe`/`write_graphml`/`InternalError`), `msgspec` (`json.encode` the canonical node-link wire codec), `numpy` (`npt.NDArray[np.float64]` the dense matrix carriers, `asarray`/`array`), `pyarrow` (`Table.from_pydict` the node-index-keyed `GraphResult.frame` column lift on this pyarrow-resident plane, the seam the `tabular/columnar#SCAN` `pa.Table.join(keys=["node"], join_type="left outer")` left-joins a node-attribute table against — banned-module-level-import-bound exactly as the `columnar`/`interop` owners), stdlib (`tempfile.NamedTemporaryFile`/`pathlib.Path` the GraphML scratch-path read), runtime (`ContentIdentity`/`ContentKey`/`RuntimeRail`/`boundary`/`traversed`/`Disposition`/`BoundaryFault`/`Receipt`/`ReceiptContributor`).
- Growth: a new traversal/path/centrality/structure algorithm is one `GraphAlgorithm` case plus one `_run_rx` arm; a new community algorithm is one `IG_COMMUNITY` row the existing community arm folds; a new centrality metric is one `RX_CENTRALITY` row; a new dense export is one `GraphResult.matrix` producer; a new node-index-keyed enrichment column is one `GraphResult.frame` arm the `tabular/columnar#SCAN` `pa.Table.join` left-joins by `node`; a new egress is one `GraphFormat` row plus one `_EGRESS` codec row; a new layout is one `LayoutKind` row; an admitted networkx `@_dispatchable` accelerator (cugraph/graphblas/parallel) lands as one `backend=`/`nx.config.backend_priority` policy on the codec lane when such a backend enters the manifest roster, never a second analysis kernel beside the rustworkx core; zero new entrypoint and never a per-algorithm `analyze_*` family.
- Boundary: `rustworkx` owns the full analysis suite (path/DAG/connectivity/cut/centrality/coloring/matching/spanning/structure/layout) and the stable non-recycled integer node index every analysis output keys on and this owner lowers into the `node`-keyed Arrow seam through `GraphResult.frame`, `networkx` the read-side codec/egress lane (`node_link_data`/`write_graphml`/`to_pandas_edgelist`) and the one-way `networkx_converter` analysis-coercion bridge, `igraph` the GPL-confined Leiden/Louvain/Infomap community split reached only inside `_run_rx`, `numpy` the dense matrix carrier, `pyarrow` the `GraphResult.frame` node-keyed `pa.Table` the `tabular/columnar#SCAN` plane left-joins a node-attribute table against by the same index, runtime the identity/rail/receipt. The graph plane produces the node-keyed enrichment frame; the relational join that enriches a node-attribute table is the `columnar#SCAN`/`query#QUERY` `pa.Table.join` the tabular plane already owns, never a graph-database node table re-minted here. The GPL `igraph` core stays in this data graph rail and is never linked into a host-distributed plugin. No product collaboration store, no bridge lifecycle, no compute numeric trio; a parallel per-backend analysis kernel (`_run_nx`/per-source kernel) where one `_run_rx` over the `_as_rx`-coerced graph owns the analysis axis and `networkx_converter` already bridges nx->rx, a networkx analysis arm whose arbitrary-hashable node id breaks the `node`-keyed `int` frame seam where `_as_rx` keys analysis on the rustworkx index, a `NodeId` widened to `Hashable` to admit a networkx analysis kernel where conversion keeps it the rx `int`, a hand-rolled Floyd-Warshall over `all_pairs_dijkstra_path_length` where `rustworkx.floyd_warshall_numpy` (or `networkx.floyd_warshall`) owns the dense all-pairs matrix, a phantom `nx.config.backend_priority`/`@_dispatchable` axis claimed but never wired, a per-algorithm `get_*` family, a parallel `RustworkxGraph`/`NetworkxGraph`/`IgraphGraph` carrier triple, a `backend=` knob where the source shape recovers it, a `repr(dict)` content-key byte stream where the canonical node-link wire keys it, a `content_key=ContentIdentity.of(...)` field assignment ignoring the returned rail, a `from builtins import frozendict` dispatch table where the `expression` `Map.of_seq` behavior rows own the callable lookup the sibling `tabular/contract#QUALITY` `_CMP` and `tabular/interop#INTEROP` `_BACKEND` tables carry, a four-positional `Receipt.of("emitted", owner, subject, {...})` call against the two-argument `Receipt.of(owner, evidence)` factory, a stringly `GraphResult` over weak `tuple` shapes, a `node`-keyed frame re-keyed away from the stable rustworkx index or a `GraphResult.frame` minting a row for a non-node-keyed `path`/`matrix`/`tree`/`matching`/`scalar`/`flag` case, a local graph-database node-table owner where the tabular plane owns the relational join, a hand-rolled BFS/DFS/Dijkstra loop where the Rust core owns it, a hand-rolled modularity-optimization where the `igraph` C core owns it, and a generic `IReceipt` replacing the typed `GraphReceipt` are the deleted forms.

```python signature
import tempfile
from collections.abc import Iterable
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never, overload

import igraph
import msgspec
import networkx as nx
import numpy as np
import rustworkx as rx
from expression import Block, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from collections.abc import Callable

    import numpy.typing as npt
    import pyarrow as pa


# --- [TYPES] ----------------------------------------------------------------------------

type NodeId = int
type RxGraph = rx.PyGraph | rx.PyDiGraph
type NxGraph = nx.Graph | nx.DiGraph
type AnyGraph = RxGraph | NxGraph | igraph.Graph
type GraphBackend = Literal["rustworkx", "networkx", "igraph"]
type ScoreMap = tuple[tuple[NodeId, float], ...]
type Partition = tuple[tuple[NodeId, ...], ...]
type Matrix = npt.NDArray[np.float64]


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
        "bfs", "dfs", "topo_sort", "ancestors", "descendants",
        "shortest_path", "bellman_ford", "astar", "k_shortest", "all_simple_paths",
        "all_pairs_distance", "floyd_warshall",
        "longest_path", "transitive_reduction", "dominators",
        "connected", "strongly_connected", "articulation", "bridges", "cycle_basis", "condensation", "core_number",
        "min_cut",
        "betweenness", "closeness", "eigenvector", "katz", "pagerank", "hits", "degree",
        "greedy_color", "max_weight_matching", "spanning_tree", "steiner_tree", "transitivity", "is_planar",
        "layout", "leiden", "louvain", "infomap",
    ] = tag()
    bfs: NodeId = case()
    dfs: NodeId | None = case()
    topo_sort: None = case()
    ancestors: NodeId = case()
    descendants: NodeId = case()
    shortest_path: tuple[NodeId, NodeId] = case()
    bellman_ford: tuple[NodeId, NodeId] = case()
    astar: tuple[NodeId, NodeId] = case()
    k_shortest: tuple[NodeId, int] = case()
    all_simple_paths: tuple[NodeId, NodeId] = case()
    all_pairs_distance: float = case()
    floyd_warshall: float = case()
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
    min_cut: None = case()
    betweenness: bool = case()
    closeness: bool = case()
    eigenvector: int = case()
    katz: float = case()
    pagerank: float = case()
    hits: int = case()
    degree: None = case()
    greedy_color: None = case()
    max_weight_matching: bool = case()
    spanning_tree: None = case()
    steiner_tree: tuple[NodeId, ...] = case()
    transitivity: None = case()
    is_planar: None = case()
    layout: LayoutKind = case()
    leiden: float = case()
    louvain: float = case()
    infomap: int = case()


@tagged_union(frozen=True)
class GraphResult:
    tag: Literal["order", "path", "paths", "scores", "matrix", "partition", "tree", "coloring", "matching", "layout", "scalar", "flag"] = tag()
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

    def frame(self) -> "RuntimeRail[pa.Table]":
        # the fence converts a non-node-keyed case to a `BoundaryFault` rather than minting a frame.
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
        yield Receipt.of(
            "graph",
            ("emitted", self.backend, {
                "kind": f"directed={self.kind.directed},multi={self.kind.multigraph}",
                "nodes": self.node_count, "edges": self.edge_count,
                "algorithm": self.algorithm, "result": self.result,
            }),
        )


class GraphPayload(Struct, frozen=True, gc=False):
    graph: AnyGraph
    backend: GraphBackend
    kind: GraphKind
    node_count: int
    edge_count: int
    content_key: ContentKey

    @classmethod
    def of(cls, graph: AnyGraph) -> "RuntimeRail[GraphPayload]":
        backend, kind, n, e, wire = _shape(graph)
        return ContentIdentity.of("graph", wire).map(
            lambda key: cls(graph=graph, backend=backend, kind=kind, node_count=n, edge_count=e, content_key=key)
        )

    @overload
    def analyze(self, algo: "GraphAlgorithm", *, by: Disposition = ...) -> "RuntimeRail[GraphResult]": ...
    @overload
    def analyze(self, algo: "Block[GraphAlgorithm]", *, by: Literal[Disposition.ABORT, Disposition.ACCUMULATE] = ...) -> "RuntimeRail[Block[GraphResult]]": ...
    @overload
    def analyze(self, algo: "Block[GraphAlgorithm]", *, by: Literal[Disposition.PARTITION]) -> "RuntimeRail[tuple[Block[GraphResult], Block[BoundaryFault]]]": ...
    def analyze(self, algo: "GraphAlgorithm | Block[GraphAlgorithm]", *, by: Disposition = Disposition.ABORT) -> "RuntimeRail[GraphResult] | RuntimeRail[Block[GraphResult]] | RuntimeRail[tuple[Block[GraphResult], Block[BoundaryFault]]]":
        # arity recovers from the algorithm value's shape, never a `_many` sibling or flag; `by` is
        # inert for a lone algorithm. The graph is `self.graph`, so the recovered `backend`/`kind`
        # can never decouple from the analyzed graph — a re-passed handle is the deleted form.
        match algo:
            case Block() as algos:
                return traversed(algos.map(self._one), by=by)
            case lone:
                return self._one(lone)

    def _one(self, algo: "GraphAlgorithm") -> "RuntimeRail[GraphResult]":
        # analysis runs on ONE kernel keyed by the stable rustworkx integer index — `_run_rx` is
        # the sole analysis path. `_as_rx` is identity on a native rustworkx graph and converts a
        # networkx or igraph source once (the catalogue's one-way `networkx_converter`, with the
        # igraph leg crossing `to_networkx` first), so EVERY analysis output keys on the rx index
        # `GraphResult.frame` joins on rather than the arbitrary-hashable networkx label or igraph's
        # reindexed vertex. The community family delegates to `igraph` INSIDE `_run_rx`, so igraph is
        # reached only for the community split from any source; networkx and igraph never run a
        # parallel analysis kernel — networkx owns the codec/egress lane on `write`, never the hot
        # path the Rust core owns.
        return boundary(f"graph.analyze.{algo.tag}", lambda: _run_rx(_as_rx(self.graph), algo, self.kind))

    def write(self, fmt: GraphFormat) -> "RuntimeRail[bytes]":
        return boundary(f"graph.egress.{fmt}", lambda: _EGRESS[self.backend][fmt](self.graph))

    def receipt(self, algo: "GraphAlgorithm", result: GraphResult) -> GraphReceipt:
        return GraphReceipt(
            backend=self.backend, kind=self.kind, node_count=self.node_count,
            edge_count=self.edge_count, algorithm=algo.tag, result=result.tag, content_key=self.content_key,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

def _node_link(g: NxGraph) -> bytes:
    # `node_link_data` is the canonical persisted graph document; the dict encodes through the
    # shared `msgspec` JSON rail, never a non-canonical `repr(dict)` byte stream.
    return msgspec.json.encode(nx.node_link_data(g, edges="edges"))


def _wire(graph: AnyGraph, backend: GraphBackend) -> bytes:
    match backend:
        case "rustworkx":
            return rx.node_link_json(graph).encode()
        case "igraph":
            return _node_link(graph.to_networkx())
        case _:
            return _node_link(graph)


def _shape(graph: AnyGraph) -> "tuple[GraphBackend, GraphKind, int, int, bytes]":
    match graph:
        case rx.PyGraph() | rx.PyDiGraph():
            kind = GraphKind(directed=isinstance(graph, rx.PyDiGraph), multigraph=graph.multigraph)
            return "rustworkx", kind, graph.num_nodes(), graph.num_edges(), _wire(graph, "rustworkx")
        case igraph.Graph():
            kind = GraphKind(directed=graph.is_directed(), multigraph=graph.has_multiple())
            return "igraph", kind, graph.vcount(), graph.ecount(), _wire(graph, "igraph")
        case _:
            kind = GraphKind(directed=graph.is_directed(), multigraph=graph.is_multigraph())
            return "networkx", kind, graph.number_of_nodes(), graph.number_of_edges(), _wire(graph, "networkx")


def _frame(result: GraphResult) -> "pa.Table":  # noqa: PLR0911
    # `pyarrow` is module-level-import-banned; the deferred import rides the same boundary the
    # columnar/interop owners bind `pl`/`read_excel` under.
    import pyarrow as pa  # noqa: PLC0415

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
            return pa.Table.from_pydict({
                "node": [n for n, _ in rows], "x": [xy[0] for _, xy in rows], "y": [xy[1] for _, xy in rows],
            })
        case _:
            raise ValueError(f"{result.tag} carries no per-node index row; only scores/coloring/partition/order/layout key the node table")


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
    (LayoutKind.SPRING, rx.spring_layout), (LayoutKind.CIRCULAR, rx.circular_layout), (LayoutKind.KAMADA_KAWAI, rx.kamada_kawai_layout),
])


def _as_rx(graph: AnyGraph) -> RxGraph:
    # the one analysis-coercion seam: identity on a native rustworkx graph, `networkx_converter`
    # on a networkx source (`keep_attributes` left default so the original label rides as the node
    # payload and the rx index is the stable join key), and `to_networkx` then `networkx_converter`
    # on an igraph source — the catalogue's one-way nx->rx bridge is the only converter direction, so
    # the igraph leg crosses networkx first. Analysis never runs on a networkx/igraph kernel; the
    # converted rx graph carries the stable non-recycled integer index `GraphResult.frame` joins on.
    match graph:
        case rx.PyGraph() | rx.PyDiGraph():
            return graph
        case igraph.Graph():
            return rx.networkx_converter(graph.to_networkx())
        case _:
            return rx.networkx_converter(graph)


def _run_rx(g: RxGraph, algo: GraphAlgorithm, kind: GraphKind) -> GraphResult:  # noqa: PLR0911, C901
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
        case GraphAlgorithm(tag="shortest_path", shortest_path=(src, dst)):
            # `PathMapping` is a `__contains__`/`__getitem__` view, not a `dict` — `.get` does not
            # exist, so the membership-gated subscript reads the path or the empty unreachable path.
            paths = rx.dijkstra_shortest_paths(g, src, target=dst, weight_fn=float)
            return GraphResult(path=tuple(paths[dst]) if dst in paths else ())
        case GraphAlgorithm(tag="bellman_ford", bellman_ford=(src, dst)):
            paths = rx.bellman_ford_shortest_paths(g, src, target=dst, weight_fn=float)
            return GraphResult(path=tuple(paths[dst]) if dst in paths else ())
        case GraphAlgorithm(tag="astar", astar=(src, dst)):
            return GraphResult(path=tuple(rx.astar_shortest_path(g, src, lambda n: n == dst, lambda _: 1.0, lambda _: 0.0)))
        case GraphAlgorithm(tag="k_shortest", k_shortest=(src, k)):
            return GraphResult(scores=tuple(rx.k_shortest_path_lengths(g, src, k, lambda _: 1.0).items()))
        case GraphAlgorithm(tag="all_simple_paths", all_simple_paths=(src, dst)):
            return GraphResult(paths=tuple(tuple(p) for p in rx.all_simple_paths(g, src, dst)))
        case GraphAlgorithm(tag="all_pairs_distance"):
            return GraphResult(matrix=np.asarray(rx.distance_matrix(g, null_value=algo.all_pairs_distance), dtype=np.float64))
        case GraphAlgorithm(tag="floyd_warshall"):
            return GraphResult(matrix=np.asarray(rx.floyd_warshall_numpy(g, weight_fn=float), dtype=np.float64))
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
            return GraphResult(matching=tuple(rx.bridges(g)))
        case GraphAlgorithm(tag="cycle_basis"):
            return GraphResult(paths=tuple(tuple(c) for c in rx.cycle_basis(g, root=algo.cycle_basis)))
        case GraphAlgorithm(tag="condensation"):
            return GraphResult(tree=tuple(rx.condensation(g).edge_list()))
        case GraphAlgorithm(tag="core_number"):
            return GraphResult(scores=tuple((n, float(k)) for n, k in rx.core_number(g).items()))
        case GraphAlgorithm(tag="min_cut"):
            cut, _ = rx.stoer_wagner_min_cut(g, weight_fn=float)
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
        case GraphAlgorithm(tag="max_weight_matching"):
            return GraphResult(matching=tuple(rx.max_weight_matching(g, max_cardinality=algo.max_weight_matching, weight_fn=int)))
        case GraphAlgorithm(tag="spanning_tree"):
            return GraphResult(tree=tuple(rx.minimum_spanning_tree(g, weight_fn=float).edge_list()))
        case GraphAlgorithm(tag="steiner_tree"):
            return GraphResult(tree=tuple(rx.steiner_tree(g, list(algo.steiner_tree), float).edge_list()))
        case GraphAlgorithm(tag="transitivity"):
            return GraphResult(scalar=rx.transitivity(g))
        case GraphAlgorithm(tag="is_planar"):
            return GraphResult(flag=rx.is_planar(g))
        case GraphAlgorithm(tag="layout"):
            return GraphResult(layout=tuple((n, tuple(xy)) for n, xy in RX_LAYOUT[algo.layout](g).items()))
        case GraphAlgorithm(tag="leiden" | "louvain" | "infomap"):
            return _run_ig(_ig_from(g, kind), algo, kind)
        case unreachable:
            assert_never(unreachable)


# --- [IGRAPH_COMMUNITY] -----------------------------------------------------------------

IG_COMMUNITY: "Final[Map[str, Callable[[igraph.Graph, GraphAlgorithm], igraph.VertexClustering]]]" = Map.of_seq([
    ("leiden", lambda g, a: g.community_leiden(objective_function="modularity", resolution=a.leiden)),
    ("louvain", lambda g, a: g.community_multilevel(resolution=a.louvain)),
    ("infomap", lambda g, a: g.community_infomap(trials=a.infomap)),
])


def _ig_from(g: RxGraph, kind: GraphKind) -> igraph.Graph:
    # the community leg builds the C-core graph from the rustworkx edge list (`_as_rx` already
    # coerced any networkx/igraph source to rustworkx, so the edge list is always rx integer
    # indices). `Graph.TupleList`'s default `vertex_name_attr="name"` stores each rx endpoint index
    # in the `name` vertex attribute, so the membership read recovers the rx index — robust to rx
    # index gaps after node removal — rather than igraph's reindexed 0-based vertex. `TupleList`
    # creates a vertex only per endpoint, so an ISOLATED rx node carries no edge and would vanish
    # from the partition; the `add_vertices` pass re-admits the edgeless rx indices as `name`-carrying
    # singleton vertices so the community partition stays TOTAL over the node set.
    ig = igraph.Graph.TupleList(g.edge_list(), directed=kind.directed)
    isolated = [n for n in g.node_indices() if n not in set(ig.vs["name"])]
    if isolated:
        ig.add_vertices(len(isolated), attributes={"name": isolated})
    return ig


def _run_ig(g: igraph.Graph, algo: GraphAlgorithm, _: GraphKind) -> GraphResult:
    match algo:
        case GraphAlgorithm(tag="leiden" | "louvain" | "infomap"):
            # `VertexClustering` membership is keyed by igraph's reindexed 0-based vertex; `_ig_from`'s
            # `TupleList`+`add_vertices` carries each rx index in the `name` attribute, so the partition
            # lowers back onto the rx index the rest of the rail (and `GraphResult.frame`) joins on.
            names = g.vs["name"]
            return GraphResult(partition=tuple(tuple(names[v] for v in block) for block in IG_COMMUNITY[algo.tag](g, algo)))
        case off_lane:
            # igraph owns only the community split — a reachable out-of-lane rejection the fence rails.
            raise NotImplementedError(f"igraph backend owns only the community split, not {off_lane.tag}; route to rustworkx")


# --- [COMPOSITION] ----------------------------------------------------------------------


def _graphml(write: "Callable[[str], object]") -> bytes:
    # GraphML is path-keyed on every backend (`rx.write_graphml(g, path)`, `nx.write_graphml(g, path)`);
    # the one helper reads the written document back through a scratch path rather than re-encoding
    # through a foreign codec or an unconfirmed byte-streaming variant.
    with tempfile.NamedTemporaryFile(suffix=".graphml") as handle:
        write(handle.name)
        return Path(handle.name).read_bytes()


_EGRESS: "Final[Map[GraphBackend, Map[GraphFormat, Callable[[AnyGraph], bytes]]]]" = Map.of_seq([
    ("rustworkx", Map.of_seq([
        (GraphFormat.NODE_LINK, lambda g: rx.node_link_json(g).encode()),
        (GraphFormat.GRAPHML, lambda g: _graphml(lambda path: rx.write_graphml(g, path))),
        (GraphFormat.EDGE_LIST, lambda g: "\n".join(f"{u} {v}" for u, v in g.edge_list()).encode()),
    ])),
    ("networkx", Map.of_seq([
        (GraphFormat.NODE_LINK, _node_link),
        (GraphFormat.GRAPHML, lambda g: _graphml(lambda path: nx.write_graphml(g, path))),
        (GraphFormat.EDGE_LIST, lambda g: nx.to_pandas_edgelist(g).to_csv(index=False).encode()),
    ])),
    ("igraph", Map.of_seq([
        (GraphFormat.NODE_LINK, lambda g: _node_link(g.to_networkx())),
        (GraphFormat.GRAPHML, lambda g: _graphml(lambda path: g.write_graphml(path))),
        (GraphFormat.EDGE_LIST, lambda g: g.get_edge_dataframe().to_csv(index=False).encode()),
    ])),
])
```

`_run_rx` is the one analysis kernel — every algorithm runs on the Rust core, `_as_rx` having coerced a networkx source through `rx.networkx_converter` and an igraph source through `to_networkx`+`networkx_converter` so the kernel always reads the stable rustworkx integer index. It routes through the bare-name dispatch (`rx.connected_components`/`rx.betweenness_centrality`/`rx.distance_matrix`) the catalogue confirms dispatches on graph subtype, so the owner never names the `graph_*`/`digraph_*` typed forms; the centrality family folds through `RX_CENTRALITY` keyed on the algorithm tag rather than one arm per metric, and the community arm delegates to `_run_ig(_ig_from(g, kind), ...)` — building the igraph C-core graph from the converted rustworkx edge list through `Graph.TupleList` because rustworkx's permissive core has no Leiden/Louvain. There is no networkx analysis kernel: `networkx` owns only the codec/egress lane (`_node_link`/`_EGRESS`), and a networkx graph admitted for analysis converts to rustworkx exactly as the `rustworkx.md` `[SIBLING_STACK]` law fixes ("the networkx sibling stays the read-side interop surface and rustworkx owns the hot algorithm path"). `_run_ig` is the one GPL-confined community engine binding `community_leiden`/`community_multilevel`/`community_infomap` through the `IG_COMMUNITY` `Map` exactly as `tabular/profile#PROFILE` `ProbeTables` binds the unbound `pb.Validate` methods, reached only from the `_run_rx` community arm, and recovering the rustworkx index off the `TupleList` `name` attribute (`g.vs["name"]`) before lowering the `VertexClustering` membership so the partition keys on the same rustworkx index `GraphResult.frame` joins on rather than igraph's reindexed vertex.

```mermaid
flowchart TD
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

## [03]-[RESEARCH]

- [BACKEND_SPLIT]: the three-package axis is the catalogue-confirmed license-and-capability split, and analysis collapses onto ONE kernel. `rustworkx` ([01]-[PACKAGE_SURFACE], Apache-2.0, the abi3 cp310-cp315 single wheel) owns the entire analysis suite (path/DAG/connectivity/cut/centrality/coloring/matching/spanning/structure/layout) and the stable non-recycled integer index `GraphResult.frame` lowers into the durable `node`-keyed Arrow column the `tabular/columnar#SCAN` `pa.Table.join(keys=["node"], join_type="left outer")` left-joins a node-attribute table against; `networkx` ([01]-[PACKAGE_SURFACE], BSD-3-Clause, pure-Python) owns the read-side codec/egress lane (`node_link_data`/`write_graphml`/`to_pandas_edgelist`) and the one-way `networkx_converter` analysis-coercion bridge, NOT a parallel analysis kernel; `igraph` ([01]-[PACKAGE_SURFACE], GPL-2.0+, the libigraph C core) owns the `community_*` Leiden/Louvain/Infomap split the Apache `rustworkx` core lacks and the BSD `networkx` core cannot match at C-core speed. The `rustworkx.md` `[SIBLING_STACK]` law fixes the route directly: "the networkx sibling stays the read-side interop surface and rustworkx owns the hot algorithm path... `networkx_converter` is the one-way NetworkX-to-rustworkx bridge at the boundary," GRAPH_COMMUNITY to `igraph`, GRAPH_PATH/GRAPH_CENTRALITY/GRAPH_STRUCTURE to `rustworkx`, the GPL dependency off the host-distributed plugin — so a networkx graph admitted for analysis converts once to rustworkx through `_as_rx` rather than running a partial pure-Python kernel whose arbitrary-hashable node id breaks the `node`-keyed `int` frame seam. The backend is recovered from the source shape through `_shape`, never a `backend=` knob, exactly as the sibling `tabular/columnar#COLUMNAR` `DatasetKind` recovers the source by shape; an admitted networkx `@_dispatchable` accelerator (cugraph/graphblas/parallel) would land as one `nx.config.backend_priority` policy on the codec lane only when such a backend enters the manifest roster, and is not claimed as a current capability.
- [RUSTWORKX_SUITE]: the `_run_rx` kernel arms bind catalogue-confirmed bare-name entrypoints ([03]-[ENTRYPOINTS] shortest-path [01]-[10], DAG [11]-[13], connectivity/traversal/centrality, coloring/matching/spanning/structure/layout): `bfs_successors`/`dfs_edges`/`topological_sort`/`ancestors`/`descendants` (traversal [06]/[11]), `dijkstra_shortest_paths(graph, source, target=, weight_fn=)`/`bellman_ford_shortest_paths`/`astar_shortest_path(graph, node, goal_fn, edge_cost_fn, estimate_cost_fn)`/`k_shortest_path_lengths(graph, start, k, edge_cost)`/`all_simple_paths` ([01]-[10]), `distance_matrix(graph, null_value=)`/`floyd_warshall_numpy` ([09]/[08]) returning the dense NumPy matrix, `dag_longest_path`/`transitive_reduction`/`immediate_dominators(graph, start_node)` ([12]-[13]), `connected_components`/`strongly_connected_components`/`weakly_connected_components`/`articulation_points`/`bridges`/`cycle_basis(graph, root=)`/`condensation`/`core_number` (connectivity [01]-[04]), `stoer_wagner_min_cut(graph, weight_fn=)` returning the `(cut, partition)` pair (cut [05]), `betweenness_centrality(graph, normalized=)`/`closeness_centrality(graph, wf_improved=)`/`eigenvector_centrality(graph, max_iter=)`/`katz_centrality(graph, alpha=)`/`pagerank(graph, alpha=)`/`hits(graph, max_iter=)` returning the hub/authority pair/`degree_centrality` (centrality [08]-[12]), `graph_greedy_color(graph, strategy=ColoringStrategy.Saturation)` (coloring [01]), `max_weight_matching(graph, max_cardinality=, weight_fn=)` (matching [02]), `minimum_spanning_tree(graph, weight_fn=)`/`steiner_tree(graph, terminal_nodes, weight_fn)` (spanning [03]), `transitivity`/`is_planar` (structure [06]), and the `spring_layout`/`circular_layout`/`kamada_kawai_layout` `Pos2DMapping` producers (layout [08]). The `RX_CENTRALITY` fold keys six metrics on the tag rather than six arms, the same data-table collapse the sibling `tabular/profile#PROFILE` `ProbeTables` and `tabular/contract#QUALITY` `_CMP` tables use. The `ColoringStrategy.Saturation` DSATUR ordering ([02]-[PUBLIC_TYPES] coloring strategy [07]) is the catalogue-named parameter, never a hand-rolled saturation order.
- [NETWORKX_CODEC]: `networkx` is the codec/egress lane, never an analysis kernel. The lane binds catalogue-confirmed codec/egress members: `node_link_data(G, edges="edges")` ([03]-[ENTRYPOINTS] file-format codecs [13], the canonical wire form, the settled 3.x `edges` key, never the removed `links` spelling), `write_graphml` ([07]), `to_pandas_edgelist` ([01]) for the `EDGE_LIST` egress, and `number_of_nodes`/`number_of_edges`/`is_directed`/`is_multigraph` ([02]) the `_shape` kind-recovery reads. A networkx graph admitted for ANALYSIS converts once to rustworkx through `_as_rx` (`rx.networkx_converter` [03]-[ENTRYPOINTS] conversion [11], original labels preserved as node payloads, the rx index the stable join key), so every analysis output keys on the stable rustworkx index rather than networkx's arbitrary-hashable label — the `rustworkx.md` `[SIBLING_STACK]` law that networkx is the read-side interop surface and rustworkx the hot algorithm path. A pure-Python networkx analysis kernel is rejected on three grounds the lane makes structural: the dense all-pairs matrix is `rustworkx.floyd_warshall_numpy` (or `networkx.floyd_warshall` [03]-[ENTRYPOINTS] [09], never a hand-rolled `all_pairs_dijkstra_path_length` nested comprehension); a networkx node id is an arbitrary hashable the `NodeId = int` frame seam cannot key on; and the `@_dispatchable`/`nx.config.backend_priority` accelerator axis carries no admitted backend in the manifest roster, so it is a deferred growth axis, never a current analysis route.
- [IGRAPH_COMMUNITY]: the `_run_ig`/`IG_COMMUNITY` community engine binds catalogue-confirmed `igraph.Graph` community methods ([03]-[ENTRYPOINTS] community detection [01]-[03]): `community_leiden(objective_function='modularity', resolution=)` (the refined Leiden [01], `objective_function` selecting `CPM`/`modularity`), `community_multilevel(resolution=)` (the Louvain implementation [02]), and `community_infomap(trials=)` ([03]), each returning a `VertexClustering` whose membership iterates to the per-community node partition ([02]-[PUBLIC_TYPES] clustering carriers, `VertexClustering.membership`/`modularity`). The `IG_COMMUNITY` table binds the bound methods exactly as `tabular/profile#PROFILE` `ProbeTables` binds the unbound `pb.Validate` step methods, never a detector class per algorithm. The community arm — reached only inside `_run_rx`, since `_as_rx` has already coerced any source to rustworkx — constructs the C-core graph through `igraph.Graph.TupleList(edges, directed=)` ([03]-[ENTRYPOINTS] construction [01], the edge-tuple builder) off the rustworkx edge-list seam (`rx.PyGraph.edge_list` [03]-[PUBLIC_TYPES] edge views [03]), the one shape the converted graph always exposes — never a `repr(dict)` round-trip. `TupleList` mints a vertex only per edge endpoint, so `_ig_from` re-admits every edgeless rx index through `add_vertices(attributes={"name": ...})` and the partition stays TOTAL over the node set — an isolated node lands as its own singleton community rather than vanishing. `VertexClustering` membership is keyed by igraph's reindexed 0-based vertex, NOT the rustworkx index, so `_run_ig` recovers the rustworkx index off the `TupleList` default `vertex_name_attr="name"` attribute (`g.vs["name"]`) before lowering the partition. Without this recovery a community `partition` would carry igraph-internal indices that fail the `GraphResult.frame` `node`-keyed left-join against a rustworkx-keyed node-attribute table; the recovery is the cross-index correctness seam, never a raw membership iterate. The `igraph.md` `[GRAPH_COMMUNITY]` law confines the GPL C core to this data graph rail and off any host-distributed plugin. The container-introspection members the `_shape`/`_EGRESS` igraph arms read for a NATIVE igraph source — `has_multiple`, `vcount`/`ecount`, `to_networkx`, `get_edge_dataframe`, `write_graphml` — bind directly on the live `Graph` alongside the catalogued `community_*`/`VertexClustering`/`TupleList` surface; the `community_*` signatures, the `TupleList` construction, and the `VertexClustering` partition read all bind catalogue-confirmed members.
- [RESULT_PARAMETERIZATION]: the `GraphResult` union parameterizes the output over twelve cases so a centrality run types `scores: ScoreMap`, a component run `partition: Partition`, an all-pairs run `matrix: npt.NDArray[np.float64]`, a spanning/transitive run `tree`, a coloring run `coloring`, a matching/bridges run `matching`, a layout run `layout`, a min-cut/transitivity run `scalar`, and a planarity run `flag` — no stringly weak `tuple` shape, the `numpy` matrix folding straight into the `pyarrow`/`numpy` tensor carriers (`rustworkx.md` `[SIBLING_STACK]`). One `GraphPayload.analyze` owns every analysis arity, the `@overload` ladder keyed on the input shape rather than a `_many` sibling: a lone `GraphAlgorithm` resolves `RuntimeRail[GraphResult]` whose case the caller matches (the per-algorithm shape rides the `GraphResult` case, not the entrypoint, since one closed union cannot widen its return per runtime tag), a `Block[GraphAlgorithm]` resolves `RuntimeRail[Block[GraphResult]]` under `ABORT`/`ACCUMULATE` or the `RuntimeRail[tuple[Block[GraphResult], Block[BoundaryFault]]]` partition split — the same input-shape arity collapse the `gridded/store#STORE` `write_region` and `gridded/virtual#VIRTUAL` `register` arms run, and the same `Disposition`-keyed overload pair the faults owner's `traversed` (`reliability/faults#FAULT`) and the runtime `evidence/evidence#EVIDENCE` `GrammarRegistry.scan` carry, so a batch sweep narrows on the input shape and the disposition it passes rather than a parallel `analyze_many` name re-narrowing the runtime union. The lone arm threads the private `_one` kernel fence the `Block` arm also maps, so the single-algorithm path is one fence shared by both arities, never duplicated.
- [NODE_FRAME_SEAM]: `GraphResult.frame` is the producer side of the `graph → tabular [WIRE]` node-index-keyed seam (`data/ARCHITECTURE.md` `[SEAMS]`). The rustworkx integer index is stable and never recycled after node removal (`rustworkx.md` [02]-[PUBLIC_TYPES], "integer indices are stable and never recycled after removal, so `NodeIndices`/`EdgeIndices` are valid stable keys into sibling tables"), so it is a genuinely durable join key — the node-keyed `GraphResult` cases lower into a `pa.Table` carrying one canonical `node` column keyed by that index plus the per-case value column(s): `scores`→`(node, value)`, `coloring`→`(node, color)`, `partition`→`(node, component)` over the membership-block enumerate, `order`→`(node, rank)` over the position, `layout`→`(node, x, y)`. `pyarrow.Table.from_pydict` is the catalogue-confirmed column lift (folder `pyarrow.md` [03]-[ENTRYPOINTS] row [06], "build `Table` from Python/arrays"), and the consumer is the catalogue-confirmed `pyarrow.Table.join(right, keys, join_type='left outer', ..., filter_expression=None)` (folder `pyarrow.md` [04]-[ENTRYPOINTS] row [06b], the left-outer hash join) the `tabular/columnar#SCAN`/`query#QUERY` plane already owns — so a centrality/coloring/community run left-joins a node-attribute table by `node` as enrichment, never a re-keyed copy and never a local graph-database node table. The produced frame rides the `tabular/interop#INTEROP` `ArrowCStream.of` PyCapsule carrier zero-copy downstream exactly as the `columnar#SCAN` `Corpus` arm does. The non-node-keyed cases (`path`/`paths`/`matrix`/`tree`/`matching`/`scalar`/`flag`) carry no per-node index row, so the `boundary(f"graph.frame.{tag}", ...)` fence converts the explicit non-node-keyed raise to a `BoundaryFault` rather than minting a degenerate frame; `pyarrow` rides the banned-module-level-import boundary inside `_frame` under `# noqa: PLC0415` exactly as the `columnar`/`interop` owners bind `pl`/`read_excel`.
- [IDENTITY_AND_RECEIPT]: `GraphPayload.of` returns `RuntimeRail[GraphPayload]` because `ContentIdentity.of("graph", wire)` returns `RuntimeRail[ContentKey]` ([02]-[IDENTITY] `of` overload ladder, the `RuntimeRail[KeyRender]` result), so the payload threads the railed key through `.map` rather than the deleted `content_key=ContentIdentity.of(...)` direct field assignment that ignored the rail and mis-typed the field — the single most consequential correctness fix over the prior page. The `content_key` is derived once in `of` over the backend's own canonical node-link wire (`_shape` -> `_wire`), never re-derived through a second `fingerprint` method over a re-passed graph: the rustworkx arm encodes `rx.node_link_json(g)` (a JSON `str`) directly, and the networkx/igraph arms encode `nx.node_link_data(g, edges="edges")` through the shared `msgspec.json.encode` rail rather than a non-canonical `repr(dict)` byte stream, the canonical-document discipline the `networkx.md` `[INTEGRATION]` law fixes (`node_link_data` serializes through the `msgspec`/JSON codec rail as the canonical persisted graph document keyed by the same content-identity discipline as other data payloads, and the `edges='edges'` key is the settled 3.x default, never the removed `links` spelling). `GraphReceipt.contribute` mints one `emitted`-phase receipt through the canonical two-argument `Receipt.of("graph", ("emitted", backend, {...}))` factory — `owner` first, the `(Phase, subject, facts)` `Evidence` tuple second, per the `observability/receipts#RECEIPT` `Receipt.of(owner, evidence)` signature that constructs the internal `fact=(phase, owner, subject, facts)` case — so a graph analysis is structured evidence on the one receipt rail rather than product state.
- [PROVISION]: `rustworkx 0.18.0` (abi3 cp310-cp315 single wheel), `networkx 3.6.1` (pure-Python), and `igraph 1.0.0` (`python-igraph`, the libigraph C core) are the data-manifest GRAPH roster ([02]-[DOMAIN_PACKAGES] GRAPH). All three resolve clean on the cp315 core: `rustworkx` and `igraph` ship native wheels and `networkx` is pure-Python, so the module-level `import rustworkx as rx`/`import networkx as nx`/`import igraph` hold exactly as the sibling `gridded/ragged#RAGGED` imports `awkward`/`nanoarrow` at module scope — none transitively loads a manifest-banned module-level dependency. `pyarrow` is the one banned-module-level import this owner touches: the `GraphResult.frame` node-keyed seam binds `import pyarrow as pa` inside `_frame` under `# noqa: PLC0415` exactly as the `columnar`/`interop` owners bind their banned `pl`/`read_excel`, and the `TYPE_CHECKING` block carries the `import pyarrow as pa` annotation-only handle so the `pa.Table` return type resolves without a runtime module-top load. The GPL `igraph` admission is confined to this data graph rail per the manifest license law and never linked into the host-distributed plugin.
