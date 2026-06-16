# [PY_DATA_GRAPH_MESH]

Graph payloads, chunked N-D tensor stores, and mesh-file exchange. `GraphPayload` carries graph kind/nodes/edges/attrs/directionality/algorithm-metadata over a `rustworkx` fast-path with a `networkx` compat row, emitting typed algorithm receipts with tabular/JSON/GraphML/bundle egress; `TensorStore` carries chunked N-D arrays over `zarr` with a bounded-memory `cubed` plan, a ragged `awkward` row, and a deploy-gated `icechunk` versioned-store row; `MeshPayload` carries mesh-file identity/cell-block topology/units/metadata/preview-export over meshio + trimesh. This is file exchange and analysis — the IFC to GLB tessellation rail belongs to the geometry package, the numeric/labelled-array compute trio belongs to the compute package, and remote AEC streams are a runtime `TransportResource` row.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                       |
| :-----: | :-------- | :---------------------------------------------------------- |
|   [1]   | GRAPH     | graph payloads, backend-dispatched algorithms, graph egress |
|   [2]   | TENSOR    | chunked N-D tensor store, codec/region rows, ragged + cubed |
|   [3]   | MESH      | mesh-file identity, cell-block topology, preview export      |

## [2]-[GRAPH]

- Owner: `GraphPayload` — graph kind/nodes/edges/attrs/directionality over a `GraphBackend` axis (`rustworkx` `PyGraph`/`PyDiGraph` fast-path, `networkx` `Graph`/`DiGraph` compat); `GraphAlgorithm` the tagged-union algorithm intent (traversal/shortest-path/centrality/community); `GraphResult` the discriminated typed receipt; `GraphEgress` the node-link JSON / GraphML / tabular edge-list export.
- Entry: `GraphPayload.of` admits a `rustworkx.PyGraph`/`PyDiGraph` or `networkx.Graph`/`DiGraph` and returns the frozen owner with its backend recovered from the source shape; `GraphPayload.analyze` runs a `GraphAlgorithm` and returns a `RuntimeRail[GraphResult]`; `GraphEgress.write` emits keyed by `ContentIdentity`. The backend is recoverable, never a knob — `rustworkx` is the default fast-path, `networkx` the interop row when a caller hands a `networkx` graph or asks for a `networkx`-only codec.
- Packages: `rustworkx` (`PyGraph`/`PyDiGraph`/`dijkstra_shortest_paths`/`astar_shortest_path`/`betweenness_centrality`/`closeness_centrality`/`pagerank`/`connected_components`/`strongly_connected_components`/`bfs_search`/`dfs_edges`/`topological_sort`/`distance_matrix`/`node_link_json`), `networkx` (`Graph`/`DiGraph`/`node_link_data`/`write_graphml`/`from_pandas_edgelist`/`shortest_path`/`betweenness_centrality`/`connected_components`), runtime (`ContentIdentity`/`RuntimeRail`/`ReceiptContributor`).
- Growth: a new graph kind is one `GraphKind` row; a new algorithm is one `GraphAlgorithm` case plus one `_run_rx`/`_run_nx` dispatch arm; a new backend is one `GraphBackend` tag; zero new surface and never a per-algorithm `analyze_*` family.
- Boundary: no product collaboration store, no bridge lifecycle, no compute-package numeric trio; graph algorithms emitting typed receipts, not product state. A per-algorithm `get_*` family, a parallel `RustworkxGraph`/`NetworkxGraph` pair, and a generic `IReceipt` are the deleted forms. `rustworkx==0.17.1` (abi3) and `networkx==3.6.1` are cp315-installed and reflected.

```python signature
from typing import TYPE_CHECKING, Literal

import networkx as nx
import rustworkx as rx
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Callable


# --- [TYPES] ----------------------------------------------------------------------------
type NodeId = int
type Weight = float
type RxGraph = rx.PyGraph | rx.PyDiGraph
type NxGraph = nx.Graph | nx.DiGraph


class GraphKind(Struct, frozen=True, tag_field="kind"):
    directed: bool
    multigraph: bool


# rustworkx is the default; networkx is the interop row when a caller hands an nx graph
# or asks for an nx-only codec. The backend is recovered from the source shape, not a knob.
type GraphBackend = Literal["rustworkx", "networkx"]


@tagged_union(frozen=True)
class GraphAlgorithm:
    """One algorithm intent; arity/filters live in the case payload, never a parallel name."""

    tag: Literal[
        "bfs", "dfs", "topo_sort", "shortest_path", "all_pairs_distance",
        "betweenness", "closeness", "pagerank", "connected", "strongly_connected",
    ] = tag()
    bfs: NodeId = case()
    dfs: NodeId | None = case()
    topo_sort: None = case()
    shortest_path: tuple[NodeId, NodeId] = case()       # (source, target); weighted by edge float
    all_pairs_distance: float = case()                  # null_value for unreachable pairs
    betweenness: bool = case()                          # normalized
    closeness: bool = case()                            # wf_improved
    pagerank: float = case()                            # alpha damping
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


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class GraphResult:
    """Discriminated typed receipt; one result owner, the shape recovers the algorithm class."""

    tag: Literal["order", "path", "scores", "matrix", "partition"] = tag()
    order: tuple[NodeId, ...] = case()                  # bfs/dfs/topo node order
    path: tuple[NodeId, ...] = case()                   # shortest-path node sequence
    scores: tuple[tuple[NodeId, float], ...] = case()   # centrality/pagerank node->score
    matrix: tuple[tuple[float, ...], ...] = case()      # all-pairs distance matrix rows
    partition: tuple[tuple[NodeId, ...], ...] = case()  # component membership sets


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
        return Receipt.Emitted(
            "graph", self.backend,
            {"nodes": str(self.node_count), "edges": str(self.edge_count)},
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
def _of_rx(g: "RxGraph") -> GraphPayload:
    directed = isinstance(g, rx.PyDiGraph)
    return GraphPayload(
        backend="rustworkx",
        kind=GraphKind(directed=directed, multigraph=g.multigraph),
        node_count=g.num_nodes(),
        edge_count=g.num_edges(),
        content_key=ContentIdentity.key("graph", rx.node_link_json(g).encode()),
    )


def _of_nx(g: "NxGraph") -> GraphPayload:
    return GraphPayload(
        backend="networkx",
        kind=GraphKind(directed=g.is_directed(), multigraph=g.is_multigraph()),
        node_count=g.number_of_nodes(),
        edge_count=g.number_of_edges(),
        content_key=ContentIdentity.key("graph", repr(nx.node_link_data(g)).encode()),
    )


def _run_rx(g: "RxGraph", algo: GraphAlgorithm) -> GraphResult:  # noqa: PLR0911 — total match
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
        case _:
            raise AssertionError(algo.tag)  # unreachable: tagged-union exhaustion


_NX_CENTRALITY: "dict[str, Callable[[NxGraph], dict[NodeId, float]]]" = {
    "betweenness": nx.betweenness_centrality,
    "closeness": nx.closeness_centrality,
    "pagerank": nx.pagerank,
}


def _run_nx(g: "NxGraph", algo: GraphAlgorithm) -> GraphResult:  # noqa: PLR0911 — total match
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
            return GraphResult(matrix=tuple(
                tuple(lengths.get(r, {}).get(c, null) for c in order) for r in order
            ))
        case GraphAlgorithm(tag="betweenness" | "closeness" | "pagerank"):
            return GraphResult(scores=tuple(_NX_CENTRALITY[algo.tag](g).items()))
        case GraphAlgorithm(tag="connected"):
            comp = nx.weakly_connected_components(g) if g.is_directed() else nx.connected_components(g)
            return GraphResult(partition=tuple(tuple(c) for c in comp))
        case GraphAlgorithm(tag="strongly_connected"):
            return GraphResult(partition=tuple(tuple(c) for c in nx.strongly_connected_components(g)))
        case _:
            raise AssertionError(algo.tag)  # unreachable: tagged-union exhaustion


def write_node_link(payload: GraphPayload, graph: "RxGraph | NxGraph") -> "RuntimeRail[bytes]":
    encode = (lambda: rx.node_link_json(graph).encode()) if payload.backend == "rustworkx" \
        else (lambda: repr(nx.node_link_data(graph)).encode())
    return boundary("graph.egress", encode)
```

## [3]-[TENSOR]

- Owner: `TensorStore` — one chunked N-D tensor store over a `TensorBackend` axis: the dense `zarr` store (chunk grid + codec pipeline + region write), the bounded-memory `cubed` plan that streams blockwise within an `allowed_mem` budget, the ragged `awkward` row for variable-length nested arrays, and a deploy-gated `icechunk` versioned-store row. `TensorChunking` carries the chunk grid; `TensorCodec` the compression pipeline; `TensorRegion` the slice; `TensorReceipt` the typed write receipt.
- Entry: `TensorStore.create` opens a `zarr` array/group rooted at a `ResourceRef` with a `TensorChunking` grid and `TensorCodec` pipeline; `TensorStore.write_region` writes a `TensorRegion` slice (orthogonal/block selection) and folds a `TensorReceipt` keyed by `ContentIdentity`; `TensorStore.plan` lifts the same store into a `cubed.Array` under a `Spec(allowed_mem=...)` for bounded-memory blockwise reductions that materialize back through `cubed.to_zarr`; `TensorStore.ragged` admits an `awkward.Array` and round-trips through `ak.to_parquet`/`ak.from_parquet`. The backend is recovered from the source shape and the requested memory bound, never a parallel store class.
- Packages: `zarr` (`create_array(serializer=, compressors=)`/`open_array`/`open_group`/`Array.set_orthogonal_selection`/`Array.blocks`/`codecs.BytesCodec`/`codecs.ShardingCodec` as `ArrayBytesCodec` serializers, `codecs.BloscCodec`/`codecs.ZstdCodec`/`codecs.GzipCodec` as `BytesBytesCodec` compressors/`storage.LocalStore`/`consolidate_metadata`), `cubed` (`from_zarr`/`to_zarr`/`Spec`/`compute`/`sum`/`mean`/`rechunk`/`map_blocks`), `awkward` (`Array`/`from_iter`/`to_parquet`/`from_parquet`/`num`/`flatten`/`to_regular`), `icechunk` (deploy-gated: `Repository`/`Storage`/`Session.commit`), runtime (`ResourceRef`/`ContentIdentity`/`RuntimeRail`/`ReceiptContributor`).
- Growth: a new codec is one `TensorCodec` case; a new chunk strategy is one `TensorChunking` field; a new backend (e.g. `tensorstore`/`n5`) is one `TensorBackend` tag plus one `_open` dispatch arm; the versioned-store dimension is one deploy-gated `icechunk` row, never a parallel store owner.
- Boundary: no compute-package numeric trio (NumPy/SciPy/labelled-array compute is `compute`), no production tensor session, no durable product store; `data` emits a portable content-addressed chunked store and a bounded-memory plan, not a runtime compute graph. A parallel `ZarrStore`/`CubedStore`/`AwkwardStore` family, an `xarray` re-derivation, and a hand-rolled chunk codec are the deleted forms. `zarr==3.2.1`, `cubed==0.28.0`, and `awkward==2.9.1` are cp315-installed and reflected; `icechunk` carries no cp315 wheel and is authored against documented API behind a deploy-asset-gate marker (verified-by-stability), the native-BLAS posture.

```python signature
from typing import TYPE_CHECKING, Literal

import awkward as ak
import cubed
import zarr
from expression import case, tag, tagged_union
from msgspec import Struct
from zarr import codecs as zc

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary
from rasm.runtime.resources_lanes import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Sequence

    import numpy as np
    from zarr.abc.codec import ArrayBytesCodec, BytesBytesCodec


# --- [TYPES] ----------------------------------------------------------------------------
type Shape = tuple[int, ...]
type ChunkGrid = tuple[int, ...]
type DType = str  # numpy dtype spelling, e.g. "float32", "int64"
type TensorBackend = Literal["zarr", "cubed", "awkward", "icechunk"]


class TensorChunking(Struct, frozen=True):
    """Chunk grid + optional shard grid; sharding groups chunks into one object on disk."""

    chunks: ChunkGrid
    shards: ChunkGrid | None = None


@tagged_union(frozen=True)
class TensorCodec:
    """Compression/serialization pipeline; one codec owner, the case selects the zarr v3 codec.

    zarr v3 splits the pipeline into role-typed slots: `serializer=` takes an `ArrayBytesCodec`
    (`BytesCodec`/`ShardingCodec`), `compressors=` takes `BytesBytesCodec` rows
    (`BloscCodec`/`ZstdCodec`/`GzipCodec`). `pipeline` returns the `(serializer, compressors)`
    pair the boundary feeds straight into `create_array`; conflating the slots fails the
    `ArrayBytesCodec` type check.
    """

    tag: Literal["blosc", "zstd", "gzip", "sharding", "raw"] = tag()
    blosc: tuple[str, int] = case()      # (cname, clevel) e.g. ("zstd", 5)
    zstd: int = case()                   # level
    gzip: int = case()                   # level
    sharding: ChunkGrid = case()         # inner chunk grid for ShardingCodec
    raw: None = case()

    def pipeline(self) -> "tuple[ArrayBytesCodec, tuple[BytesBytesCodec, ...]]":
        match self:
            case TensorCodec(tag="blosc"):
                cname, clevel = self.blosc
                return (zc.BytesCodec(), (zc.BloscCodec(cname=cname, clevel=clevel),))
            case TensorCodec(tag="zstd"):
                return (zc.BytesCodec(), (zc.ZstdCodec(level=self.zstd),))
            case TensorCodec(tag="gzip"):
                return (zc.BytesCodec(), (zc.GzipCodec(level=self.gzip),))
            case TensorCodec(tag="sharding"):
                inner = zc.ShardingCodec(chunk_shape=self.sharding, codecs=(zc.BytesCodec(), zc.ZstdCodec()))
                return (inner, ())
            case TensorCodec(tag="raw"):
                return (zc.BytesCodec(), ())
            case _:
                raise AssertionError(self.tag)  # unreachable: tagged-union exhaustion


class TensorRegion(Struct, frozen=True):
    """A hyperslab; per-axis (start, stop) folded into the orthogonal selection zarr applies."""

    bounds: tuple[tuple[int, int], ...]

    def selection(self) -> tuple[slice, ...]:
        return tuple(slice(lo, hi) for lo, hi in self.bounds)


# --- [MODELS] ---------------------------------------------------------------------------
class TensorReceipt(Struct, frozen=True):
    backend: TensorBackend
    shape: Shape
    chunks: ChunkGrid
    dtype: DType
    codec: str
    bytes_stored: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.Emitted(
            "tensor", self.backend,
            {"shape": "x".join(map(str, self.shape)), "codec": self.codec, "stored": str(self.bytes_stored)},
        )


class TensorStore(Struct, frozen=True):
    backend: TensorBackend
    ref: ResourceRef
    shape: Shape
    chunking: TensorChunking
    dtype: DType
    codec: TensorCodec

    @classmethod
    def create(
        cls, ref: ResourceRef, shape: Shape, dtype: DType,
        chunking: TensorChunking, codec: TensorCodec = TensorCodec(raw=None),
    ) -> "RuntimeRail[TensorStore]":
        return boundary("tensor.create", lambda: _create_zarr(ref, shape, dtype, chunking, codec))

    def write_region(self, region: TensorRegion, data: "np.ndarray") -> "RuntimeRail[TensorReceipt]":
        return boundary("tensor.write_region", lambda: _write_region(self, region, data))

    def plan(self, allowed_mem: str = "2GB") -> "RuntimeRail[cubed.Array]":
        # bounded-memory: cubed streams blockwise within allowed_mem, never loading the full store
        return boundary(
            "tensor.plan",
            lambda: cubed.from_zarr(str(self.ref.path), spec=cubed.Spec(allowed_mem=allowed_mem)),
        )

    @staticmethod
    def ragged(values: "Sequence[Sequence[object]]") -> "RuntimeRail[ak.Array]":
        # variable-length nested arrays: awkward owns the ragged axis zarr's regular grid cannot
        return boundary("tensor.ragged", lambda: ak.from_iter(values))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _create_zarr(
    ref: ResourceRef, shape: Shape, dtype: DType, chunking: TensorChunking, codec: TensorCodec,
) -> TensorStore:
    serializer, compressors = codec.pipeline()
    zarr.create_array(
        store=zarr.storage.LocalStore(str(ref.path)),
        shape=shape, dtype=dtype, chunks=chunking.chunks, shards=chunking.shards,
        serializer=serializer, compressors=compressors, overwrite=True,
    )
    return TensorStore(backend="zarr", ref=ref, shape=shape, chunking=chunking, dtype=dtype, codec=codec)


def _write_region(store: "TensorStore", region: TensorRegion, data: "np.ndarray") -> TensorReceipt:
    arr = zarr.open_array(store=zarr.storage.LocalStore(str(store.ref.path)), mode="r+")
    arr.set_orthogonal_selection(region.selection(), data)
    return TensorReceipt(
        backend="zarr", shape=tuple(arr.shape), chunks=tuple(arr.chunks),
        dtype=str(arr.dtype), codec=store.codec.tag, bytes_stored=arr.nbytes_stored(),
        content_key=ContentIdentity.key("tensor", data.tobytes()),
    )


def reduce_plan(plan: "cubed.Array", op: Literal["sum", "mean", "nansum", "std"], axis: int | None) -> "cubed.Array":
    # one bounded-memory reduction dispatch; cubed evaluates the plan within the Spec budget
    return {"sum": cubed.sum, "mean": cubed.mean, "nansum": cubed.nansum, "std": cubed.std}[op](plan, axis=axis)


def ragged_egress(ref: ResourceRef, array: "ak.Array") -> "RuntimeRail[ContentKey]":
    # ragged round-trip parks on Parquet (awkward's content-addressable columnar form)
    def _write() -> ContentKey:
        ak.to_parquet(array, str(ref.path))
        return ContentIdentity.key("tensor.ragged", ref.path.read_bytes())
    return boundary("tensor.ragged.egress", _write)
```

[DEPLOY_GATE]: `icechunk` versioned-store rows (transactional, git-like snapshots over the same chunked store) author behind a deploy-asset-gate marker — no cp315 wheel publishes today; the fence is verified-by-stability against the documented API (the C# native-BLAS posture), reflected once the marker-floor environment admits it. `Repository.open(Storage.local_filesystem(...))`, `repo.writable_session("main")`, `session.store` (a zarr-compatible store), and `session.commit("message")` are the documented seam; the `TensorBackend="icechunk"` tag routes `TensorStore.create`/`write_region` through the icechunk session store instead of `LocalStore`, adding snapshot identity on top of the zarr chunk grid without a parallel store owner.

## [4]-[MESH]

- Owner: `MeshPayload` — mesh-file identity/cell-block topology/units/metadata/preview-export over a `MeshBackend` axis: `meshio.read`/`write` for FE volume/cell-block meshes, `trimesh.load`/`Trimesh.export` for surface meshes. `MeshBackend` is recovered from the source extension, never a knob; the cell-block topology folds both engines onto one `cell_blocks` field (meshio `CellBlock.type`, trimesh's single `triangle` block).
- Entry: `MeshPayload.read` admits a mesh file and returns the frozen owner keyed by `ContentIdentity` with the backend recovered from the source shape; `MeshPayload.preview` emits a `glb` preview render through whichever engine owns the loaded mesh; `MeshPayload.write` round-trips through the format the requested extension selects. Read/preview/write all return a `RuntimeRail`, never raising in the boundary.
- Packages: `meshio` (`read`/`write`/`Mesh`/`CellBlock.type`), `trimesh` (`load`/`Trimesh`/`Trimesh.export`/`Trimesh.vertices`/`Trimesh.faces`), runtime (`ContentIdentity`/`ContentKey`/`ResourceRef`/`RuntimeRail`/`boundary`/`Receipt`).
- Growth: a new surface format is one `trimesh` extension string; a new FE format is one `meshio` format string; a new engine is one `MeshBackend` tag plus one `_read`/`_export` dispatch arm; zero new surface and never a per-format `read_*` family.
- Boundary: no geometry kernel (that is the geometry package), no bridge lifecycle; the IFC to GLB tessellation rail is geometry-owned, never re-derived here; a hand-rolled mesh parser and a parallel `MeshioPayload`/`TrimeshPayload` pair are the deleted forms. `meshio==5.3.5` and `trimesh==4.12.2` are cp315-installed and reflected; the residual is the `.api/api-meshio.md`/`.api/api-trimesh.md` capture, not a wheel gap.

```python signature
from typing import Literal

import meshio
import trimesh
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary
from rasm.runtime.resources_lanes import ResourceRef


# --- [TYPES] ----------------------------------------------------------------------------
type MeshBackend = Literal["meshio", "trimesh"]

# trimesh owns surface (display/exchange) extensions; meshio owns FE volume/cell-block formats.
_TRIMESH_EXTS: frozenset[str] = frozenset({".stl", ".obj", ".ply", ".glb", ".gltf", ".off", ".3mf"})


# --- [MODELS] ---------------------------------------------------------------------------
class MeshPayload(Struct, frozen=True):
    backend: MeshBackend
    content_key: ContentKey
    point_count: int
    cell_blocks: tuple[str, ...]
    units: str

    @classmethod
    def read(cls, ref: ResourceRef) -> "RuntimeRail[MeshPayload]":
        return boundary("mesh.read", lambda: _read(ref))

    def preview(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return boundary("mesh.preview", lambda: _export(ref, out, "glb"))

    def write(self, ref: ResourceRef, out: ResourceRef) -> "RuntimeRail[ContentKey]":
        return boundary("mesh.write", lambda: _export(ref, out, out.path.suffix.lstrip(".")))

    def contribute(self) -> Receipt:
        return Receipt.Emitted(
            "mesh", self.backend,
            {"points": str(self.point_count), "blocks": ",".join(self.cell_blocks), "units": self.units},
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
def _backend_of(ref: ResourceRef) -> MeshBackend:
    return "trimesh" if ref.path.suffix.lower() in _TRIMESH_EXTS else "meshio"


def _read(ref: ResourceRef) -> MeshPayload:
    match _backend_of(ref):
        case "meshio":
            mesh = meshio.read(str(ref.path))
            return MeshPayload(
                backend="meshio",
                content_key=ContentIdentity.key("mesh", mesh.points.tobytes()),
                point_count=len(mesh.points),
                cell_blocks=tuple(block.type for block in mesh.cells),
                units="m",
            )
        case "trimesh":
            surface = trimesh.load(str(ref.path), force="mesh")
            return MeshPayload(
                backend="trimesh",
                content_key=ContentIdentity.key("mesh", surface.vertices.tobytes()),
                point_count=len(surface.vertices),
                cell_blocks=("triangle",) if len(surface.faces) else (),
                units=surface.units or "m",
            )
        case _ as unreachable:
            raise AssertionError(unreachable)  # unreachable: Literal exhaustion


def _export(ref: ResourceRef, out: ResourceRef, fmt: str) -> ContentKey:
    match _backend_of(ref):
        case "meshio":
            meshio.write(str(out.path), meshio.read(str(ref.path)), file_format=fmt)
        case "trimesh":
            trimesh.load(str(ref.path), force="mesh").export(str(out.path), file_type=fmt)
        case _ as unreachable:
            raise AssertionError(unreachable)  # unreachable: Literal exhaustion
    return ContentIdentity.key("mesh.export", out.path.read_bytes())
```

## [5]-[RESEARCH]

- [RUSTWORKX_BACKEND]: `rustworkx==0.17.1` (abi3) is cp315-installed and reflected. `PyGraph`/`PyDiGraph` expose `num_nodes`/`num_edges`/`multigraph`; the bare functions `dijkstra_shortest_paths(graph, source, target=, weight_fn=)` returns a `PathMapping`, `betweenness_centrality(graph, normalized=, parallel_threshold=)` and `closeness_centrality(graph, wf_improved=)` and `pagerank(graph, /, alpha=)` return `CentralityMapping`/`dict`, `connected_components`/`strongly_connected_components` return `list[set[int]]`, `distance_matrix(graph, null_value=)` returns a NumPy matrix, and `node_link_json(graph)` emits the JSON egress. The `graph_*`/`digraph_*` prefixed variants are the typed dispatch the bare functions select by graph subtype — the owner never names both.
- [NETWORKX_INTEROP]: `networkx==3.6.1` is reflected (`.api/api-networkx.md`, no gap). The compat row uses `bfs_tree`/`dfs_preorder_nodes`/`topological_sort`/`shortest_path(weight=)`/`all_pairs_dijkstra_path_length`/`betweenness_centrality`/`closeness_centrality`/`pagerank`/`connected_components`/`weakly_connected_components`/`strongly_connected_components`/`node_link_data`. Centrality folds through one `_NX_CENTRALITY` table rather than three sibling arms.
- [TENSOR_STACK]: `zarr==3.2.1`, `cubed==0.28.0`, `awkward==2.9.1` are cp315-installed and reflected. `zarr.create_array(store=, shape=, dtype=, chunks=, shards=, serializer=)` and `zarr.codecs.{BloscCodec,ZstdCodec,GzipCodec,ShardingCodec,BytesCodec}` and `zarr.storage.LocalStore` and `Array.set_orthogonal_selection`/`nbytes_stored`/`blocks` are reflected. `cubed.from_zarr(store, spec=)`/`cubed.Spec(allowed_mem=)`/`cubed.{sum,mean,nansum,std,rechunk}`/`cubed.to_zarr` carry the bounded-memory blockwise plan. `awkward.{from_iter,Array,to_parquet,from_parquet,num,flatten,to_regular}` carry the ragged axis. These three finalize the TENSOR owner; `.api` rows (`api-zarr.md`, `api-cubed.md`, `api-awkward.md`) pend capture.
- [ICECHUNK_GATE]: `icechunk` carries no cp315 wheel (`import icechunk` → `ModuleNotFoundError`); the versioned-store rows author against documented API behind a deploy-asset-gate marker and reflect once the marker-floor environment admits it (suite TASKLOG `PY_API_002a`).
- [MESH_ENGINES]: `meshio==5.3.5` and `trimesh==4.12.2` are cp315-installed and reflected. `meshio.read`/`meshio.write(file_format=)`/`meshio.Mesh.points`/`meshio.Mesh.cells` with `CellBlock.type`, and `trimesh.load(force="mesh")` -> `Trimesh.vertices`/`Trimesh.faces`/`Trimesh.units`/`Trimesh.export(file_type=)` are reflected; `.api/api-meshio.md` and `.api/api-trimesh.md` exist (no wheel gap). The residual is the cell-block-vs-surface fold edge cases, not engine availability.
