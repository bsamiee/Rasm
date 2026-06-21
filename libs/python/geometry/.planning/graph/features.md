# [PY_GEOMETRY_GRAPH_FEATURES]

`Features` is one `ReceiptContributor` capsule discriminating a single `FeatureKind` request or a batch `Sequence` over the `FEATURE_OPS` detect/project table. Sharp-edge, planar, curvature, and boundary detection are four rows of one data-driven table, never four functions and never sibling methods on a namespace class. Each row is one woven rail stacking four `.api` surfaces in a single pass: it reads the `trimesh` cached-topology algebra (`face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`faces`/`edges_unique`), folds a `numpy` vectorized reduction against the `FeaturePolicy` cosine/defect threshold to mark the feature element set, lifts the marked set into a `networkx` graph through the `create_using`-keyed conversion bridge (`from_edgelist`/`from_numpy_array`), and the analytics aspect folds the closed `AnalyticOp` table over that one graph. The receipt carries the whole census in one fold rather than per-analytic inlined calls.

The analytics table closes five families under one `backend=` dispatch row: connectivity (`connected_components`/`weakly_connected_components`/`strongly_connected_components`), centrality (`betweenness_centrality`/`degree_centrality`/`closeness_centrality`/`eigenvector_centrality`/`pagerank`), spanning structure (`minimum_spanning_tree`), cycle census (`simple_cycles`), and community partition (`nx.community.louvain_communities`). The row `mode_guard` skips a directed-only or undirected-only algorithm by data, never by an inline `if graph.is_directed()` branch.

The feature graph graduates through `FeatureResult.graduates`, minting the geometry `GraduationReceipt` over `HandoffAxis(geometry="network-graph")` on the same compute admission rail the `algebra` sibling rides for the identical subject. This owner is the mesh-feature-projection producer of `network-graph`; the `algebra` sibling is the compas-adjacency producer of the same literal, never folded into one file. Raw mesh-file exchange stays at the data seam, mesh repair/boolean is the `mesh` sibling, and non-manifold cell topology is the `nonmanifold` sibling.

`trimesh`, `numpy`, and `networkx` are the intended cp315 core, so this owner rides the core directly — no companion gate, no dark band.

## [01]-[INDEX]

- [01]-[FEATURES]: the `FeatureKind` detect/project op-table, the `MarkSpace`-keyed projection algebra, the `AnalyticOp`/`AnalyticValue` analytics table folding to one typed `Census`, the `CASE` ledger/ceiling spec table, the `@receipted` `Features` capsule with its polymorphic single-or-batch `run` and async `bridged` mirror, the typed `FeatureResult`, and the per-kind `graduates` rail under one `ReceiptContributor`.

## [02]-[FEATURES]

- Owner: `Features` — the `ReceiptContributor` capsule discriminating a single `FeatureKind` request or a batch through the `FEATURE_OPS` detect/project table; `@receipted(_REDACTION)` harvests `contribute` on exit so the public surface is the contributor the caller reads, never a per-op rail the aspect discards. `FeatureKind` selects the kind; `GraphMode` resolves `create_using` over the full `nx.Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` family through its `MODE_CREATE` row, so directedness and multiplicity are a bounded vocabulary, never a `directed: bool`/`multi: bool` knob pair; `GraphBackend` names the `@nx._dispatchable` dispatch backend (`DEFAULT`/`PARALLEL`/`CUGRAPH`/`GRAPHBLAS`), threaded once as `backend=` into every reducer rather than forked per call site; `MarkSpace` tags which index space a detector's marks live in (`EDGE_ROW`/`VERTEX`/`FACET`) so a `FeatureSpec`'s detector and projection cannot cross-index. `FeatureSpec` binds a kind to its `(Detector, Projection, MarkSpace)` triple; `AnalyticSpec` binds an `AnalyticOp` to its `(reducer, mode_guard)` pair; `AnalyticValue` the `@tagged_union` collapsing every reducer return — a `scalar` `float`, a `leaderboard` `Leaders`, or a `groups` `Partition` partition — to one typed carrier so `_analyse` folds to `Map[AnalyticOp, AnalyticValue]` with no `isinstance` reconstruction. `Census` the typed value object holding the kind/mode/backend tags, node/edge/marks counts, and that `Map`, projected to facts through `msgspec.structs.asdict`. `FeatureResult` the sole `ReceiptContributor`, carrying the kind, the `Census`, the node-link graph projection, and the case-keyed `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`). `FeaturePolicy` the `msgspec.Struct` value object carrying the `dihedral_cos`/`coplanar_cos`/`defect` thresholds, the `GraphMode`, the `GraphBackend`, the `centrality_top` leaderboard cap, the `power_iter` power-iteration ceiling, and the `AnalyticOp` selection set, so every threshold, cap, solver bound, and analytic toggle is a policy field, never a positional float or a boolean knob.
- Cases: `FeatureKind` rows `SHARP_EDGE` (face pairs across `face_adjacency` whose `face_normals` cosine falls below `dihedral_cos`, the dihedral crease set) · `PLANAR` (the `facets` coplanar groups whose member normals stay above `coplanar_cos`, the flat-region partition) · `CURVATURE` (vertices whose `vertex_defects` angle-defect — the discrete Gaussian curvature — exceeds `defect`, the high-curvature set) · `BOUNDARY` (the deduplicated face-edge rows whose face-incidence count — from one `numpy.unique(..., return_counts=True)` over the `faces` triangle-edge expansion — is exactly one, the open-boundary loop) — each row a `FeatureSpec(detector, projection, mark_space)`, never a dispatch branch; a fifth kind is one `FeatureKind` row plus one `FEATURE_OPS` entry. `AnalyticOp` rows close the analytics vocabulary across five families — components (`COMPONENTS`/`WEAK_COMPONENTS`/`STRONG_COMPONENTS`), centrality (`BETWEENNESS`/`DEGREE`/`CLOSENESS`/`EIGENVECTOR`/`PAGERANK`), spanning (`SPANNING_WEIGHT`), cycles (`CYCLES`), community (`COMMUNITY`) — each one `ANALYTICS` row folding one catalogue-confirmed `networkx` algorithm over the one projection, its directed/undirected applicability gated by the row's `mode_guard` predicate.
- Entry: `Features.run(request)` is the one polymorphic entrypoint discriminating a single `FeatureRequest` or a batch `Sequence[FeatureRequest]`. A single request lifts `_extract` through `boundary(f"features.{kind}", ...)` — the single exception-to-fault conversion at the host edge, where the `NetworkX*` taxonomy (including `PowerIterationFailedConvergence` from an unconverged eigen/pagerank pass) and the `trimesh` cache faults convert exactly once, interior code receiving only the rail. A batch builds a `Block` of per-request rails in one comprehension and folds them through `runtime.faults.traversed` (`Disposition.ACCUMULATE`) so one fault stays addressable in the aggregate while every successful `FeatureResult` already landed on the contributed stream. `bridged` is the async mirror routing the heavy analytics band — the `betweenness_centrality`/`pagerank`/`simple_cycles` reducers whose all-pairs and enumeration cores must not block the event loop — through `anyio.to_thread.run_sync` so the blocking fold becomes the awaitable thunk `async_boundary` requires, sharing the one fault rail rather than a parallel async surface.
- Auto: the detector kernels never branch per kind — each is a closed `numpy` reduction over the `trimesh` cached property the row names, woven with the policy threshold in one vectorized fold. The crease set is a single `numpy.where` over the clipped row-dot of `linalg.norm`-normalized `face_normals` indexed by `face_adjacency` against `dihedral_cos` (no inverse trig); the planar set is the `facets` coplanar groups admitted only where the group's `face_normals` mean-direction agreement clears `coplanar_cos`; curvature is `numpy.abs(vertex_defects) > defect`; the boundary loop is the deduplicated face-edge rows whose face-incidence count is one, computed by one `numpy.unique(tri_edges, axis=0, return_counts=True)` over the sorted `faces` triangle-edge expansion (`_face_edge_set`). `edges_unique` is already deduplicated upstream, so its own uniqueness count never discriminates a boundary, and the per-face edge expansion is the catalogue-confirmed incidence source. The boundary detector and its projection share the one `_face_edge_set` reduction, so the count-1 marks index exactly the unique edge array the projection rebuilds. Each `FeatureSpec` carries a `MarkSpace` tag the projection reads: `EDGE_ROW` lifts `rows[marks]` through `from_edgelist`, `VERTEX` scatters a boolean vertex flag (`flags[marks]`, then `flags[edges].any(axis=1)`) selecting `edges_unique` rows incident to a marked vertex before `from_edgelist`, and `FACET` builds the boolean facet-adjacency matrix through `from_numpy_array` — so an edge-index mark and a vertex-index mark never cross-index. All lift through `create_using=policy.mode.create_using` so one call discriminates graph kind, and `backend=policy.backend.value` threads the dispatch backend into every reducer. The node-link evidence is `msgspec.json.encode(node_link_data(graph, edges="edges"))` so the graduation payload is real JSON bytes, never a Python `repr`.
- Receipt: `FeatureResult` conforms to `ReceiptContributor`; `contribute` returns the one-element `tuple[Receipt, ...]` the port streams, minting `Receipt.of("geometry.graph.features", (phase, subject, facts))` per the runtime two-argument factory — the `(Phase, subject, facts)` evidence triple minting the `fact` case. The phase is data-driven: `phase="emitted"` for a graph that produced nodes and `phase="admitted"` for a vacuous feature set (no marks, an empty graph) the census flags as the entry caveat rather than asserting a result. The facts ride as native `dict[str, object]` — the `Census` projected through `msgspec.structs.asdict` carrying the kind/mode/backend tags, the node/edge/marks counts, and one native scalar per selected analytic (component/weak/strong counts, spanning-tree weight, cycle count, community count, leaderboard cardinalities) — never `str()`-coerced, because the `observability/receipts#RECEIPT` `Encoder(enc_hook=repr, order="deterministic")` renderer serializes native scalars and a pre-`str()` map is the deleted form that owner rejects. The tuple-of-tuple leaderboards and the community partition stay off the flat facts map and ride the typed `Census.values` `Map`. `FeatureResult.graduates(evidence_key)` routes the per-kind `CASE.ledger`/`CASE.ceiling` through the one `GraduationReceipt.graduates(source_package, HandoffAxis(geometry=subject), evidence_key, measured, ceiling)` admission — the same residual-over-ceiling fold the `algebra` and `nonmanifold` siblings feed — gating an `empty_graph_fraction` against the zero ceiling so a vacuous projection (no nodes) is an `Error(BoundaryFault)` rather than a graduated handoff. The `node_link` JSON bytes are the graduation evidence that fold keys; the census is the evidence it reads, never a re-measured value.
- Packages: `trimesh` (`Trimesh.faces`/`face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`edges_unique`/`vertices`), `numpy` (`linalg.norm`/`clip`/`sum`/`where`/`abs`/`unique`/`sort`/`reshape`/`asarray`/`zeros`/`full`), `networkx` (`from_edgelist`/`from_numpy_array`/`connected_components`/`weakly_connected_components`/`strongly_connected_components`/`betweenness_centrality`/`degree_centrality`/`closeness_centrality`/`eigenvector_centrality`/`pagerank`/`minimum_spanning_tree`/`simple_cycles`/`community.louvain_communities`/`node_link_data`, the `Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` payload family, the `create_using`/`backend=` axes), `msgspec` (`Struct`/`structs.asdict`/`json.encode`), `anyio` (`to_thread.run_sync`), `expression` (`tagged_union`/`case`/`tag`, `Block`/`Map`), runtime (`RuntimeRail`/`boundary`/`async_boundary`/`traversed`/`Disposition`, `Receipt`/`ReceiptContributor`/`Redaction`/`receipted`, `ContentKey`), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`).
- Growth: a new feature kind is one `FeatureKind` row plus one `FEATURE_OPS` row binding its `(detector, projection, mark_space)` triple and one `CASE` ledger/ceiling row; a new mark space is one `MarkSpace` member plus one projection arm; a new analytic is one `AnalyticOp` row plus one `ANALYTICS` row binding its `(reducer, mode_guard)` pair and one `AnalyticValue` projection arm if its return shape is new; a new `AnalyticValue` shape is one case plus one `as_scalar` arm; a stricter threshold, a larger leaderboard, a different analytic selection, or a directedness/backend switch is a `FeaturePolicy` field value the caller passes; zero new surface, no per-kind dispatch branch.
- Boundary: this owner detects features and projects them onto the `networkx` analytic graph — mesh repair/winding/boolean is the `mesh/repair` sibling over `trimesh`/`manifold3d`, non-manifold cell/aperture topology is the `nonmanifold` sibling over `topologicpy`, compas numerical/form-finding geometry is the `algebra` sibling, and raw mesh-file decode/encode plus columnar edge-list reframing into Arrow stays at the data seam. The `network-graph` subject crosses from this owner alongside the `algebra` sibling's `network-graph` arm on the one geometry `HandoffAxis` case; the two are distinct producers of the same subject (mesh-feature projection here, compas adjacency there), never folded into one file. Backend selection rides the policy `GraphBackend` row threaded as `backend=`, never a forked per-backend call site nor a global `nx.config` mutation this owner reaches across. The deleted forms: a flat module-level `extract` function in place of the polymorphic single-or-batch capsule; a `FeatureResult` that mints a `graduation_subject` it never crosses where `graduates` folds the per-kind ledger through the one admission rail; a hand-built `dict[str, object]` facts map where `structs.asdict` projects the typed `Census`; a constant `phase="emitted"` where the empty-graph verdict data-drives the phase; a four-positional `Receipt.of(phase, owner, subject, facts)` against the runtime two-argument contract; a `str()`-coerced `dict[str, str]` facts map; a second batch method where the one `traversed` fold drains a `Sequence`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Mapping, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final, Literal, assert_never

import anyio
import msgspec
import networkx as nx
import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs
from numpy.typing import NDArray

from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import Disposition, RuntimeRail, async_boundary, boundary, traversed
from rasm.runtime.receipts import Receipt, ReceiptContributor, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------

type Phase = Literal["admitted", "emitted"]
type Marks = NDArray[np.int64]
type GraphFamily = type[nx.Graph] | type[nx.DiGraph] | type[nx.MultiGraph] | type[nx.MultiDiGraph]
type Detector = Callable[[trimesh.Trimesh, "FeaturePolicy"], Marks]
type Projection = Callable[[trimesh.Trimesh, Marks, "FeaturePolicy"], nx.Graph]
type Reducer = Callable[[nx.Graph, "FeaturePolicy"], "AnalyticValue"]
type Leaders = tuple[tuple[int, float], ...]
type Partition = tuple[tuple[int, ...], ...]


class FeatureKind(StrEnum):
    SHARP_EDGE = "sharp-edge"
    PLANAR = "planar"
    CURVATURE = "curvature"
    BOUNDARY = "boundary"


class MarkSpace(StrEnum):
    EDGE_ROW = "edge-row"  # marks index rows of a precomputed (n, 2) edge array
    VERTEX = "vertex"      # marks index vertices; the projection scatters incidence onto edges_unique
    FACET = "facet"        # marks index facet groups; the projection builds a facet-adjacency matrix


class GraphMode(StrEnum):
    UNDIRECTED = "undirected"
    DIRECTED = "directed"
    MULTI = "multi"
    MULTI_DIRECTED = "multi-directed"

    @property
    def create_using(self) -> GraphFamily:
        return MODE_CREATE[self]

    @property
    def directed(self) -> bool:
        return self in (GraphMode.DIRECTED, GraphMode.MULTI_DIRECTED)


class GraphBackend(StrEnum):
    DEFAULT = "default"
    PARALLEL = "parallel"
    CUGRAPH = "cugraph"
    GRAPHBLAS = "graphblas"


class AnalyticOp(StrEnum):
    COMPONENTS = "components"
    WEAK_COMPONENTS = "weak-components"
    STRONG_COMPONENTS = "strong-components"
    BETWEENNESS = "betweenness"
    DEGREE = "degree"
    CLOSENESS = "closeness"
    EIGENVECTOR = "eigenvector"
    PAGERANK = "pagerank"
    SPANNING_WEIGHT = "spanning-weight"
    CYCLES = "cycles"
    COMMUNITY = "community"


# the default analytic selection a `FeaturePolicy` carries: the mode_guard then prunes the directed-
# only rows on an undirected graph, so this set is the menu and the guard is the per-graph filter.
_DEFAULT_OPS: Final[frozenset[AnalyticOp]] = frozenset(
    {AnalyticOp.COMPONENTS, AnalyticOp.STRONG_COMPONENTS, AnalyticOp.BETWEENNESS, AnalyticOp.PAGERANK, AnalyticOp.SPANNING_WEIGHT}
)

_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # feature facts carry no secret field
# a vacuous projection (no nodes) breaches the zero ceiling, so an empty feature graph does not graduate.
_EMPTY_CEILING: Final[float] = 0.0

# --- [TABLES] ---------------------------------------------------------------------------

# one row per `GraphMode` resolving the `nx` payload class `create_using` selects, so directedness
# AND multiplicity are bounded data rather than a `directed: bool`/`multi: bool` knob pair.
MODE_CREATE: Final[Mapping[GraphMode, GraphFamily]] = MappingProxyType({
    GraphMode.UNDIRECTED: nx.Graph,
    GraphMode.DIRECTED: nx.DiGraph,
    GraphMode.MULTI: nx.MultiGraph,
    GraphMode.MULTI_DIRECTED: nx.MultiDiGraph,
})

# --- [MODELS] ---------------------------------------------------------------------------


class FeaturePolicy(Struct, frozen=True, gc=False):
    dihedral_cos: float = 0.5
    coplanar_cos: float = 0.999
    defect: float = 0.1
    mode: GraphMode = GraphMode.UNDIRECTED
    backend: GraphBackend = GraphBackend.DEFAULT
    centrality_top: int = 8
    power_iter: int = 200  # the eigenvector/pagerank power-iteration ceiling both reducers thread as max_iter
    ops: frozenset[AnalyticOp] = _DEFAULT_OPS


class FeatureRequest(Struct, frozen=True):
    kind: FeatureKind
    policy: FeaturePolicy = FeaturePolicy()


@tagged_union(frozen=True)
class AnalyticValue:
    tag: Literal["scalar", "leaderboard", "groups"] = tag()
    scalar: float = case()
    leaderboard: Leaders = case()
    groups: Partition = case()

    @staticmethod
    def Scalar(value: float) -> "AnalyticValue":
        return AnalyticValue(scalar=value)

    @staticmethod
    def Leaderboard(rows: Leaders) -> "AnalyticValue":
        return AnalyticValue(leaderboard=rows)

    @staticmethod
    def Groups(partition: Partition) -> "AnalyticValue":
        return AnalyticValue(groups=partition)

    def as_scalar(self) -> float:
        # the tag is the closed discriminant: a scalar carries its value, a leaderboard / partition
        # carries its cardinality, so the flat facts map reads one `float` off every analytic kind.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return v
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return float(len(rows))
            case AnalyticValue(tag="groups", groups=partition):
                return float(len(partition))
            case _ as unreachable:
                assert_never(unreachable)


class FeatureSpec(Struct, frozen=True):
    detector: Detector
    projection: Projection
    mark_space: MarkSpace


class AnalyticSpec(Struct, frozen=True):
    op: AnalyticOp
    reducer: Reducer
    mode_guard: Callable[[GraphMode], bool]


class Census(Struct, frozen=True):
    # the native tag/count scalars `structs.asdict` projects straight onto the receipt facts; `values`
    # is the typed analytics carrier `facts` strips and replaces with one `as_scalar` projection per op,
    # so the tuple-of-tuple leaderboards and partitions never flatten onto the `dict[str, object]` map.
    kind: FeatureKind
    mode: GraphMode
    backend: GraphBackend
    marks: int
    nodes: int
    edges: int
    values: Map[AnalyticOp, AnalyticValue]

    def scalar(self, op: AnalyticOp) -> float:
        return self.values.try_find(op).map(lambda v: v.as_scalar()).default_value(0.0)

    def facts(self) -> dict[str, object]:
        flat = {key: value for key, value in structs.asdict(self).items() if key != "values"}
        return flat | {op.value: value.as_scalar() for op, value in self.values.items()}


class CaseSpec(Struct, frozen=True):
    # one row per FeatureKind: the subject the case crosses and the Census-read empty-graph ledger plus
    # ceiling the graduation fold gates, so a vacuous projection does not graduate and a new kind is one row.
    subject: GeometrySubject
    ledger: Callable[[Census], dict[str, float]]
    ceiling: dict[str, float]


class FeatureResult(Struct, ReceiptContributor, frozen=True):
    kind: FeatureKind
    census: Census
    node_link: bytes
    graduation_subject: GeometrySubject

    def contribute(self) -> Iterable[Receipt]:
        # runtime two-argument Receipt.of(owner, (phase, subject, facts)); the empty-graph verdict
        # data-drives the phase, and the Census native scalars ride the EventDict dict[str, object] slots.
        phase: Phase = "emitted" if self.census.nodes else "admitted"
        return (Receipt.of("geometry.graph.features", (phase, self.graduation_subject, self.census.facts())),)

    def graduates(self, evidence_key: ContentKey) -> "RuntimeRail[GraduationReceipt]":
        # the per-kind CaseSpec supplies subject, the empty-graph ledger, and the zero ceiling, folded
        # through the one compute residual-over-ceiling admission; never a re-measured value or a second gate.
        spec = CASE[self.kind]
        return GraduationReceipt.graduates(
            "geometry.graph.features", HandoffAxis(geometry=self.graduation_subject), evidence_key,
            spec.ledger(self.census), spec.ceiling,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _unit_dots(normals: NDArray[np.float64], pairs: NDArray[np.int64]) -> NDArray[np.float64]:
    units = normals / np.clip(np.linalg.norm(normals, axis=1, keepdims=True), 1e-12, None)
    return np.clip(np.sum(units[pairs[:, 0]] * units[pairs[:, 1]], axis=1), -1.0, 1.0)


def _sharp_edges(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    pairs = np.asarray(mesh.face_adjacency)
    dots = _unit_dots(np.asarray(mesh.face_normals), pairs)
    return np.asarray(np.where(dots < policy.dihedral_cos)[0], dtype=np.int64)


def _planar_facets(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    normals = np.asarray(mesh.face_normals)
    units = normals / np.clip(np.linalg.norm(normals, axis=1, keepdims=True), 1e-12, None)
    flat: list[int] = []
    for i, facet in enumerate(mesh.facets):
        members = np.asarray(facet, dtype=np.int64)
        mean = units[members].mean(axis=0)
        mean /= max(float(np.linalg.norm(mean)), 1e-12)
        if members.size and float(np.min(units[members] @ mean)) >= policy.coplanar_cos:
            flat.append(i)
    return np.asarray(flat, dtype=np.int64)


def _curvature_vertices(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    return np.asarray(np.where(np.abs(np.asarray(mesh.vertex_defects)) > policy.defect)[0], dtype=np.int64)


def _face_edge_set(mesh: trimesh.Trimesh) -> tuple[NDArray[np.int64], NDArray[np.int64]]:
    faces = np.asarray(mesh.faces, dtype=np.int64)
    tri_edges = np.sort(faces[:, [[0, 1], [1, 2], [2, 0]]].reshape(-1, 2), axis=1)
    uniques, counts = np.unique(tri_edges, axis=0, return_counts=True)
    return uniques, counts


def _boundary_edges(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    _, counts = _face_edge_set(mesh)
    return np.asarray(np.where(counts == 1)[0], dtype=np.int64)


def _edgelist(rows: NDArray[np.int64], marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    edges = rows[marks] if marks.size else np.empty((0, 2), dtype=np.int64)
    return nx.from_edgelist(map(tuple, edges.tolist()), create_using=policy.mode.create_using())


def _facet_matrix(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    face_to_pos = np.full(len(mesh.faces), -1, dtype=np.int64)
    for pos, group in enumerate(marks.tolist()):
        face_to_pos[np.asarray(mesh.facets[group], dtype=np.int64)] = pos
    pairs = np.asarray(mesh.face_adjacency, dtype=np.int64)
    ga, gb = face_to_pos[pairs[:, 0]], face_to_pos[pairs[:, 1]]
    link = (ga >= 0) & (gb >= 0) & (ga != gb)
    span = np.zeros((marks.size, marks.size), dtype=np.int64)
    span[ga[link], gb[link]] = span[gb[link], ga[link]] = 1
    return nx.from_numpy_array(span, create_using=policy.mode.create_using())


# the projection reads the row's `MarkSpace`: an edge-row mark indexes a precomputed (n, 2) array,
# a vertex mark scatters incidence onto `edges_unique`, and a facet mark builds the adjacency matrix.
# one source-resolving fold, so a detector's mark space and its projection cannot drift apart.
def _crease_projection(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    return _edgelist(np.asarray(mesh.face_adjacency, dtype=np.int64), marks, policy)


def _boundary_projection(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    uniques, _ = _face_edge_set(mesh)
    return _edgelist(uniques, marks, policy)


def _vertex_edge_projection(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    edges = np.asarray(mesh.edges_unique, dtype=np.int64)
    flags = np.zeros(len(mesh.vertices), dtype=bool)
    flags[marks] = True
    touched = np.where(flags[edges].any(axis=1))[0]
    return _edgelist(edges, touched, policy)


FEATURE_OPS: Final[Mapping[FeatureKind, FeatureSpec]] = MappingProxyType({
    FeatureKind.SHARP_EDGE: FeatureSpec(_sharp_edges, _crease_projection, MarkSpace.EDGE_ROW),
    FeatureKind.PLANAR: FeatureSpec(_planar_facets, _facet_matrix, MarkSpace.FACET),
    FeatureKind.CURVATURE: FeatureSpec(_curvature_vertices, _vertex_edge_projection, MarkSpace.VERTEX),
    FeatureKind.BOUNDARY: FeatureSpec(_boundary_edges, _boundary_projection, MarkSpace.EDGE_ROW),
})


def _ranked(scores: Mapping[int, float], policy: FeaturePolicy) -> AnalyticValue:
    ranked = sorted(scores.items(), key=lambda kv: kv[1], reverse=True)[: policy.centrality_top]
    return AnalyticValue.Leaderboard(tuple((int(n), float(s)) for n, s in ranked))


def _component_count(generator: Iterable[object]) -> AnalyticValue:
    return AnalyticValue.Scalar(float(sum(1 for _ in generator)))


# each reducer threads `backend=policy.backend.value` into its `@nx._dispatchable` algorithm and
# returns one typed `AnalyticValue`, so the dispatch backend is one policy row, never a forked site.
ANALYTICS: Final[tuple[AnalyticSpec, ...]] = (
    AnalyticSpec(AnalyticOp.COMPONENTS, lambda g, p: _component_count(nx.connected_components(g, backend=p.backend.value)), lambda m: not m.directed),
    AnalyticSpec(AnalyticOp.WEAK_COMPONENTS, lambda g, p: _component_count(nx.weakly_connected_components(g, backend=p.backend.value)), lambda m: m.directed),
    AnalyticSpec(AnalyticOp.STRONG_COMPONENTS, lambda g, p: _component_count(nx.strongly_connected_components(g, backend=p.backend.value)), lambda m: m.directed),
    AnalyticSpec(AnalyticOp.BETWEENNESS, lambda g, p: _ranked(nx.betweenness_centrality(g, backend=p.backend.value), p), lambda _: True),
    AnalyticSpec(AnalyticOp.DEGREE, lambda g, p: _ranked(nx.degree_centrality(g, backend=p.backend.value), p), lambda _: True),
    AnalyticSpec(AnalyticOp.CLOSENESS, lambda g, p: _ranked(nx.closeness_centrality(g, backend=p.backend.value), p), lambda _: True),
    AnalyticSpec(AnalyticOp.EIGENVECTOR, lambda g, p: _ranked(nx.eigenvector_centrality(g, max_iter=p.pagerank_iter, backend=p.backend.value), p) if g.number_of_nodes() else AnalyticValue.Leaderboard(()), lambda _: True),
    AnalyticSpec(AnalyticOp.PAGERANK, lambda g, p: _ranked(nx.pagerank(g, max_iter=p.pagerank_iter, backend=p.backend.value), p) if g.number_of_nodes() else AnalyticValue.Leaderboard(()), lambda _: True),
    AnalyticSpec(AnalyticOp.SPANNING_WEIGHT, lambda g, p: AnalyticValue.Scalar(float(nx.minimum_spanning_tree(g, backend=p.backend.value).number_of_edges())), lambda m: not m.directed),
    AnalyticSpec(AnalyticOp.CYCLES, lambda g, p: AnalyticValue.Scalar(float(sum(1 for _ in nx.simple_cycles(g, backend=p.backend.value)))), lambda _: True),
    AnalyticSpec(AnalyticOp.COMMUNITY, lambda g, p: AnalyticValue.Groups(tuple(tuple(sorted(int(n) for n in c)) for c in nx.community.louvain_communities(g, backend=p.backend.value))) if g.number_of_nodes() else AnalyticValue.Groups(()), lambda m: not m.directed),
)


def _analyse(graph: nx.Graph, policy: FeaturePolicy) -> Census:
    # the policy `ops` set AND the row `mode_guard` both gate by data: a directed-only or
    # undirected-only algorithm and a deselected op are skipped without an inline graph-kind branch.
    selected = (spec for spec in ANALYTICS if spec.op in policy.ops and spec.mode_guard(policy.mode))
    return Census(values=Map.of_seq([(spec.op, spec.reducer(graph, policy)) for spec in selected]))


@receipted(_REDACTION)
def _extract(mesh: trimesh.Trimesh, kind: FeatureKind, policy: FeaturePolicy) -> FeatureResult:
    spec = FEATURE_OPS[kind]
    marks = spec.detector(mesh, policy)
    graph = spec.projection(mesh, marks, policy)
    return FeatureResult(
        kind=kind,
        policy=policy,
        marks=tuple(int(m) for m in marks.tolist()),
        node_link=msgspec.json.encode(nx.node_link_data(graph, edges="edges")),
        census=_analyse(graph, policy),
        nodes=graph.number_of_nodes(),
        edges=graph.number_of_edges(),
    )


def extract(mesh: trimesh.Trimesh, kind: FeatureKind, policy: FeaturePolicy = FeaturePolicy()) -> "RuntimeRail[FeatureResult]":
    return boundary(f"features.{kind}", lambda: _extract(mesh, kind, policy))
```

## [03]-[RESEARCH]

- [TRIMESH_CACHE_SHAPES]: the `Trimesh.face_adjacency` `(n, 2)` face-pair shape, the `face_normals` `(faces, 3)` row alignment to `faces`, the `facets` list-of-arrays member shape (each entry a face-index array per coplanar group), the `vertex_defects` `(vertices,)` angle-defect vector (discrete Gaussian curvature, `2π − Σθ`), and the `edges_unique` `(e, 2)` vertex-pair shape confirm against the folder `.api/trimesh.md` cached-property table (rows [07]–[12]) on the cp315 core interpreter; the `face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`edges_unique` members are catalogue-confirmed.
- [BOUNDARY_INCIDENCE]: the `_boundary_edges` kernel resolves single-incidence open-boundary edges through `_face_edge_set` — one `numpy.unique(tri_edges, axis=0, return_counts=True)` over the sorted `faces` triangle-edge expansion (`[[0,1],[1,2],[2,0]]` fancy-index) — and selects the count-1 rows; `faces` is the only mesh member this fold reads and `numpy.unique`/`sort`/`reshape` are catalogue-confirmed (`numpy.md` `unique` row [16], `sort` row [15], `reshape` row [01]), while `faces` is the geometry root the `.api/trimesh.md` cached-property axis is keyed off. `edges_unique` is deduplicated upstream so its own uniqueness count is one for every row and cannot discriminate a boundary; per-face incidence is the correct discriminant. The direct trimesh open-edge accessor (`mesh.edges_face` incidence, the `mesh.facets_boundary` outline path — confirmed cached properties per `.api/trimesh.md` CAPTURE_GAP) is a future density refinement gated on a catalogue admission of their exact incidence/return shapes, never a present dependency replacing the `faces`-only fold.
- [NETWORKX_ANALYTICS]: the `ANALYTICS` table folds eleven catalogue-confirmed `networkx` algorithm families — `connected_components` (row [06], undirected set generator), `weakly_connected_components` (row [10], directed set generator), `strongly_connected_components` (row [07], directed set generator), `betweenness_centrality` (row [17], node-score dict), `degree_centrality`/`closeness_centrality`/`eigenvector_centrality` (row [18], node-score dict), `pagerank` (row [16], node-score dict), `minimum_spanning_tree` (row [08], graph), `simple_cycles` (row [14], cycle generator), and `nx.community.louvain_communities` (row [21], `list[set]` partition) — plus the `node_link_data` node-link projection verb (row [13]). The `from_edgelist`/`from_numpy_array` conversion bridges (rows [07]/[03]), the `create_using` directedness/multiplicity axis over the `Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` family (PUBLIC_TYPES [01]–[04]), and the `backend=`/`**backend_kwargs` dispatch axis on every `@nx._dispatchable` algorithm (IMPLEMENTATION_LAW BACKEND_DISPATCH) are catalogue-confirmed. The `networkx` catalogue lives at the branch `libs/python/data/.api/networkx.md` because `data` is the package owner of record; `graph/features` is the geometry-side consumer of the same admitted package, citing the one branch catalogue rather than minting a duplicate folder `.api`. The `GraphBackend` policy row threads `backend=` into each reducer per the catalogue's "receipt records the backend and never forks parallel call sites per backend" law; the page never mutates process-global `nx.config`, leaving dispatch policy to the application boundary the catalogue names.
- [COMMUNITY_ANALYTIC]: `nx.community.louvain_communities(G, weight='weight', resolution=1, threshold=1e-07, max_level=None, seed=None)` returning `list[set]` is catalogue-confirmed (`networkx.md` ENTRYPOINTS row [21]) and is the ARCHITECTURE-charter community-detection analytic, now a live `AnalyticOp.COMMUNITY` row rather than a deferred fence — its partition projects through the `AnalyticValue.Groups` case so the receipt carries the community count as a native scalar while the full partition rides the typed `Census.values` `Map`. The sibling partition verbs `greedy_modularity_communities` (row [22]) and `girvan_newman`/`modularity` (row [23]) admit as further `AnalyticOp` rows the same way; `louvain_communities` is the seeded default the policy `ops` set toggles.
- [MARK_SPACE_PAIRING]: each `FeatureSpec` carries a `MarkSpace` tag binding its detector's index space to its projection — `EDGE_ROW` (`SHARP_EDGE` marks index `face_adjacency` rows projected by `_crease_projection`'s direct `rows[marks]` selection; `BOUNDARY` marks index the `_face_edge_set` deduplicated edge array, and `_boundary_projection` rebuilds that same array so the marks index it exactly), `VERTEX` (`CURVATURE` marks are vertex indices projected through `_vertex_edge_projection`, which scatters a boolean vertex flag `flags[marks]` and selects `edges_unique` rows where `flags[edges].any(axis=1)` rather than indexing edge rows by a vertex id), and `FACET` (`PLANAR` marks are facet-group indices projected through `_facet_matrix` over the facet-adjacency matrix). The pairing is a `FEATURE_OPS` row property tagged by `MarkSpace`, so a detector returning a different mark space binds its matching projection and tag in the same row and the cross-index defect (an edge projection consuming vertex marks as edge-row indices) cannot recur. All marks stay 1-D index arrays so the `FeatureResult.marks` projection (`tuple(int(m) for m in marks.tolist())`) is total across every kind.
- [NODE_LINK_EDGES_KEY]: `node_link_data` takes the `edges=` key argument selecting the edge-collection key in the emitted mapping; the explicit `edges="edges"` pin produces the stable forward-compatible node-link schema and the `links` default is the removed legacy spelling the `networkx.md` INTEGRATION law forbids pinning — `edges="edges"` is the settled 3.x default. `msgspec.json.encode` (`msgspec.md` ENTRYPOINTS [01]) serializes the resulting mapping to JSON bytes — the graduation payload is real JSON, never `str(dict)` Python `repr` — and the `networkx.md` INTEGRATION law confirms a `node_link_data` payload is the canonical persisted graph document keyed through the `msgspec`/JSON codec rail.
- [ANALYTIC_VALUE_COLLAPSE]: the prior page modelled analytics as a flat `tuple[AnalyticSpec, ...]` of lambdas returning `float | int | tuple[tuple[int, float], ...]`, reconstructed into a five-field `Analytics` struct through a `folded.get(op, 0)` per-field remap and an `isinstance`-guarded `rank` lambda — a drift surface where a new analytic forced a struct field plus a remap edit. The rebuild collapses the reducer return to the `AnalyticValue` `@tagged_union` (`scalar`/`leaderboard`/`census`) so `_analyse` folds to one `Map[AnalyticOp, AnalyticValue]` with no `isinstance` reconstruction, and `Census` is a thin projection over that map: `Census.facts` reads `as_scalar()` off each value (a scalar carries its value, a leaderboard/partition its cardinality) so the receipt's flat `dict[str, object]` map carries one native scalar per selected analytic while the full leaderboards and partitions ride the typed `values` `Map`. A new analytic is one `AnalyticOp` row plus one `ANALYTICS` reducer; a new return shape is one `AnalyticValue` case plus one `as_scalar` arm.
- [RECEIPT_CONTRACT]: `FeatureResult` conforms to the runtime `observability/receipts#RECEIPT` `ReceiptContributor` `Protocol` and `contribute` returns the `Iterable[Receipt]` the port streams (a one-element tuple), minting through the two-argument `Receipt.of(owner, evidence)` factory with the `(Phase, subject, facts)` evidence triple — never the four-positional `Receipt.of(phase, owner, subject, facts)` form, which is not the factory's signature. The facts ride as `dict[str, object]` carrying native `int`/`float` scalars, because the receipts owner's `Encoder(enc_hook=repr, order="deterministic")` renderer serializes native scalars and a `str()`-coerced `dict[str, str]` is the deleted form that owner rejects. `@receipted(_REDACTION)` on `_extract` is the AOP aspect harvesting and emitting the contributor stream on exit — receipt egress a decorator rail, not an inline `Signals.emit`, the same `@receipted` seam the sibling `mesh/cad.md#CAD` `CadReceipt` rides. The keep-all `_REDACTION` (an empty-classification `Redaction`) matches the `compute/graduation/handoff#GRADUATION` graduation facts that carry no secret field.
