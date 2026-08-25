# [PY_GEOMETRY_GRAPH_FEATURES]

Mesh-feature detection projected onto the `networkx` analytic graph: sharp-edge, planar, curvature, and boundary detection are rows of one `FEATURE_OPS` detect/project table, never sibling functions, and the `ANALYTICS` table closes the connectivity/centrality/spanning/cycle/community families over that one projection, `mode_guard` skipping a directed-only or undirected-only algorithm by data rather than an inline graph-kind branch. `Features` is the mesh-feature-projection producer of `network-graph`; the `algebra` sibling is the compas-adjacency producer of the same literal, never folded into one file.

Reducer-return vocabulary (`AnalyticValue`, `ranked`, the census projections) imports downward from the tier-0 `graph/analytic` substrate, no local twin. `run` and `bridged` delegate observation through `evidence_run` over the one pure module-level `_extracted` both paths share, and `bridged` ships that body as a `KernelTrait.HOSTILE` kernel with only the mesh and request as crossing payload. `FeatureResult` retains the node-link bytes, structural census, analytic boards, and `NETWORK_GRAPH` subject directly.

## [01]-[INDEX]

- [02]-[FEATURES]: `FEATURE_OPS` detect/project table, `MARK_PROJECT` projection algebra, `ANALYTICS` reducer table, and the `run`/`bridged` pair.

## [02]-[FEATURES]

- Owner: `Features` holds the conditioned `trimesh.Trimesh` beside its lane and its composition `ScopeKey`, so every weave call this owner makes stamps the key the app root bound rather than a default. `GraphMode` resolves `create_using` over the full `Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` family, so directedness and multiplicity form one bounded vocabulary, never a `directed`/`multi` knob pair; `GraphBackend` threads once as `backend=` into every reducer, never forked per call site nor mutating a global `nx.config`; `MarkSpace` keys `MARK_PROJECT` dispatch, so a detector's mark space and its projection cannot cross-index — the kinds reuse two edge arms and one facet arm. Every threshold, cap, solver bound, and analytic toggle is a `FeaturePolicy` field; `power_iter` caps power iteration, threaded as `max_iter` into the eigenvector/pagerank reducers.
- Entry: `run` discriminates a single request or a batch, each returning through its own weave rail; the `NetworkX*` taxonomy (including `PowerIterationFailedConvergence`) and trimesh cache faults convert exactly once at the weave's fence. `bridged` never collapses an offload fault into a synthetic empty result — a failure stays an `Error(BoundaryFault)` on the returned rail.
- Output: leaderboards and partitions stay on typed `Census.values`; node-link evidence stays real JSON bytes; `frame` projects one held analytic board through `EvidenceFrame` using the result's own specification-derived key.
- Packages: `trimesh`, `numpy`, and `networkx` per the fence imports; the analytic vocabulary and the graduation spine import downward from their geometry owners.
- Growth: a new feature kind is one `FeatureKind` row and one `FEATURE_OPS` row; a new mark space is one `MarkSpace` member and one `MARK_PROJECT` arm; a new analytic is one `AnalyticOp` row and one `ANALYTICS` row; a new `AnalyticValue` shape lands on the `graph/analytic` owner; a threshold, cap, selection, or backend switch is a `FeaturePolicy` value.
- Boundary: mesh repair/winding/boolean is the `mesh/repair` sibling's over `trimesh`/`manifold3d`; non-manifold cell/aperture topology is the `nonmanifold` sibling's; compas numerical/form-finding is the `algebra` sibling's; raw mesh-file decode/encode and columnar edge-list reframing stay at the data seam. Both `network-graph` producers cross on the one geometry `HandoffAxis` case — mesh-feature projection here, compas adjacency there.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Mapping, Sequence
from enum import StrEnum
from functools import partial
from types import MappingProxyType
from typing import Final, assert_never

import msgspec
import networkx as nx
import numpy as np
import trimesh
from expression.collections import Block, Map
from msgspec import Struct
from numpy.typing import NDArray

from rasm.geometry.graduation import EvidenceFrame, EvidenceScope, GeometrySubject, evidence_key, evidence_run
from rasm.geometry.graph.analytic import AnalyticValue, ranked
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

type Marks = NDArray[np.int64]
type EdgeArray = NDArray[np.int64]
type GraphFamily = type[nx.Graph] | type[nx.DiGraph] | type[nx.MultiGraph] | type[nx.MultiDiGraph]
type Detector = Callable[[trimesh.Trimesh, "FeaturePolicy"], Marks]
type EdgeSource = Callable[[trimesh.Trimesh], EdgeArray]
type Projection = Callable[[trimesh.Trimesh, "FeatureSpec", Marks, "FeaturePolicy"], nx.Graph]
type Reducer = Callable[[nx.Graph, "FeaturePolicy"], AnalyticValue]


class FeatureKind(StrEnum):
    SHARP_EDGE = "sharp-edge"
    PLANAR = "planar"
    CURVATURE = "curvature"
    BOUNDARY = "boundary"


class MarkSpace(StrEnum):
    EDGE_ROW = "edge-row"
    VERTEX = "vertex"
    FACET = "facet"


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


# --- [CONSTANTS] ------------------------------------------------------------------------

_DEFAULT_OPS: Final[frozenset[AnalyticOp]] = frozenset({
    AnalyticOp.COMPONENTS,
    AnalyticOp.STRONG_COMPONENTS,
    AnalyticOp.BETWEENNESS,
    AnalyticOp.PAGERANK,
    AnalyticOp.SPANNING_WEIGHT,
})

# --- [MODELS] ---------------------------------------------------------------------------


class FeaturePolicy(Struct, frozen=True, gc=False):
    dihedral_cos: float = 0.5
    coplanar_cos: float = 0.999
    defect: float = 0.1
    mode: GraphMode = GraphMode.UNDIRECTED
    backend: GraphBackend = GraphBackend.DEFAULT
    centrality_top: int = 8
    power_iter: int = 200
    ops: frozenset[AnalyticOp] = _DEFAULT_OPS


class FeatureRequest(Struct, frozen=True):
    kind: FeatureKind
    policy: FeaturePolicy = FeaturePolicy()


class FeatureSpec(Struct, frozen=True, gc=False):
    detector: Detector
    edge_source: EdgeSource
    mark_space: MarkSpace


class AnalyticSpec(Struct, frozen=True, gc=False):
    op: AnalyticOp
    reducer: Reducer
    mode_guard: Callable[[GraphMode], bool]


class Census(Struct, frozen=True):
    kind: FeatureKind
    mode: GraphMode
    backend: GraphBackend
    marks: int
    nodes: int
    edges: int
    values: Map[AnalyticOp, AnalyticValue]


class FeatureResult(Struct, frozen=True):
    kind: FeatureKind
    census: Census
    node_link: bytes
    graduation_subject: GeometrySubject

    @property
    def spec(self) -> bytes:

        return b"|".join((self.kind.value.encode(), self.node_link))

    def frame(self, op: AnalyticOp) -> "RuntimeRail[EvidenceFrame]":
        board = self.census.values.try_find(op).default_value(AnalyticValue.Leaderboard(())).tabled()
        return EvidenceFrame.of(self.graduation_subject, evidence_key(self.graduation_subject, self.spec), board)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _unit_dots(normals: NDArray[np.float64], pairs: NDArray[np.int64]) -> NDArray[np.float64]:
    units = normals / np.clip(np.linalg.norm(normals, axis=1, keepdims=True), 1e-12, None)
    return np.clip(np.sum(units[pairs[:, 0]] * units[pairs[:, 1]], axis=1), -1.0, 1.0)


def _sharp_edges(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    pairs = np.asarray(mesh.face_adjacency)
    dots = _unit_dots(np.asarray(mesh.face_normals), pairs)
    return np.asarray(np.where(dots < policy.dihedral_cos)[0], dtype=np.int64)


def _planar_facets(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    units = np.asarray(mesh.face_normals) / np.clip(np.linalg.norm(np.asarray(mesh.face_normals), axis=1, keepdims=True), 1e-12, None)
    return np.asarray(
        [
            i
            for i, facet in enumerate(mesh.facets)
            if (g := np.asarray(facet, dtype=np.int64)).size
            and float(np.min(units[g] @ ((m := units[g].mean(axis=0)) / max(float(np.linalg.norm(m)), 1e-12)))) >= policy.coplanar_cos
        ],
        dtype=np.int64,
    )


def _curvature_vertices(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    return np.asarray(np.where(np.abs(np.asarray(mesh.vertex_defects)) > policy.defect)[0], dtype=np.int64)


def _boundary_edges(mesh: trimesh.Trimesh, _: FeaturePolicy) -> Marks:
    incidence = (np.asarray(mesh.edges_face, dtype=np.int64) >= 0).sum(axis=1)
    return np.asarray(np.where(incidence == 1)[0], dtype=np.int64)


def _adjacency_rows(mesh: trimesh.Trimesh) -> EdgeArray:
    return np.asarray(mesh.face_adjacency, dtype=np.int64)


def _unique_edges(mesh: trimesh.Trimesh) -> EdgeArray:
    return np.asarray(mesh.edges_unique, dtype=np.int64)


def _no_edges(_: trimesh.Trimesh) -> EdgeArray:
    return np.empty((0, 2), dtype=np.int64)


def _edgelist(rows: EdgeArray, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    edges = rows[marks] if marks.size else np.empty((0, 2), dtype=np.int64)
    return nx.from_edgelist(map(tuple, edges.tolist()), create_using=policy.mode.create_using)


def _edge_row(mesh: trimesh.Trimesh, spec: FeatureSpec, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    return _edgelist(spec.edge_source(mesh), marks, policy)


def _vertex_edge(mesh: trimesh.Trimesh, spec: FeatureSpec, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    edges = spec.edge_source(mesh)
    flags = np.zeros(len(mesh.vertices), dtype=bool)
    flags[marks] = True
    touched = np.where(flags[edges].any(axis=1))[0]
    return _edgelist(edges, touched, policy)


def _facet_matrix(mesh: trimesh.Trimesh, _: FeatureSpec, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    face_to_pos = np.full(len(mesh.faces), -1, dtype=np.int64)
    for pos, group in enumerate(marks.tolist()):
        face_to_pos[np.asarray(mesh.facets[group], dtype=np.int64)] = pos
    pairs = np.asarray(mesh.face_adjacency, dtype=np.int64)
    ga, gb = face_to_pos[pairs[:, 0]], face_to_pos[pairs[:, 1]]
    link = (ga >= 0) & (gb >= 0) & (ga != gb)
    span = np.zeros((marks.size, marks.size), dtype=np.int64)
    span[ga[link], gb[link]] = span[gb[link], ga[link]] = 1
    return nx.from_numpy_array(span, create_using=policy.mode.create_using)


# --- [TABLES] ---------------------------------------------------------------------------

MODE_CREATE: Final[Mapping[GraphMode, GraphFamily]] = MappingProxyType({
    GraphMode.UNDIRECTED: nx.Graph,
    GraphMode.DIRECTED: nx.DiGraph,
    GraphMode.MULTI: nx.MultiGraph,
    GraphMode.MULTI_DIRECTED: nx.MultiDiGraph,
})

MARK_PROJECT: Final[Mapping[MarkSpace, Projection]] = MappingProxyType({
    MarkSpace.EDGE_ROW: _edge_row,
    MarkSpace.VERTEX: _vertex_edge,
    MarkSpace.FACET: _facet_matrix,
})

FEATURE_OPS: Final[Mapping[FeatureKind, FeatureSpec]] = MappingProxyType({
    FeatureKind.SHARP_EDGE: FeatureSpec(_sharp_edges, _adjacency_rows, MarkSpace.EDGE_ROW),
    FeatureKind.PLANAR: FeatureSpec(_planar_facets, _no_edges, MarkSpace.FACET),
    FeatureKind.CURVATURE: FeatureSpec(_curvature_vertices, _unique_edges, MarkSpace.VERTEX),
    FeatureKind.BOUNDARY: FeatureSpec(_boundary_edges, _unique_edges, MarkSpace.EDGE_ROW),
})


def _component_count(generator: Iterable[object]) -> AnalyticValue:
    return AnalyticValue.Scalar(float(sum(1 for _ in generator)))


ANALYTICS: Final[tuple[AnalyticSpec, ...]] = (
    AnalyticSpec(AnalyticOp.COMPONENTS, lambda g, p: _component_count(nx.connected_components(g, backend=p.backend.value)), lambda m: not m.directed),
    AnalyticSpec(
        AnalyticOp.WEAK_COMPONENTS, lambda g, p: _component_count(nx.weakly_connected_components(g, backend=p.backend.value)), lambda m: m.directed
    ),
    AnalyticSpec(
        AnalyticOp.STRONG_COMPONENTS,
        lambda g, p: _component_count(nx.strongly_connected_components(g, backend=p.backend.value)),
        lambda m: m.directed,
    ),
    AnalyticSpec(AnalyticOp.BETWEENNESS, lambda g, p: ranked(nx.betweenness_centrality(g, backend=p.backend.value), p.centrality_top), lambda _: True),
    AnalyticSpec(AnalyticOp.DEGREE, lambda g, p: ranked(nx.degree_centrality(g, backend=p.backend.value), p.centrality_top), lambda _: True),
    AnalyticSpec(AnalyticOp.CLOSENESS, lambda g, p: ranked(nx.closeness_centrality(g, backend=p.backend.value), p.centrality_top), lambda _: True),
    AnalyticSpec(
        AnalyticOp.EIGENVECTOR,
        lambda g, p: (
            ranked(nx.eigenvector_centrality(g, max_iter=p.power_iter, backend=p.backend.value), p.centrality_top)
            if g.number_of_nodes()
            else AnalyticValue.Leaderboard(())
        ),
        lambda _: True,
    ),
    AnalyticSpec(
        AnalyticOp.PAGERANK,
        lambda g, p: (
            ranked(nx.pagerank(g, max_iter=p.power_iter, backend=p.backend.value), p.centrality_top) if g.number_of_nodes() else AnalyticValue.Leaderboard(())
        ),
        lambda _: True,
    ),
    AnalyticSpec(
        AnalyticOp.SPANNING_WEIGHT,
        lambda g, p: AnalyticValue.Scalar(float(nx.minimum_spanning_tree(g, backend=p.backend.value).number_of_edges())),
        lambda m: not m.directed,
    ),
    AnalyticSpec(
        AnalyticOp.CYCLES, lambda g, p: AnalyticValue.Scalar(float(sum(1 for _ in nx.simple_cycles(g, backend=p.backend.value)))), lambda _: True
    ),
    AnalyticSpec(
        AnalyticOp.COMMUNITY,
        lambda g, p: (
            AnalyticValue.Groups(tuple(tuple(sorted(int(n) for n in c)) for c in nx.community.louvain_communities(g, backend=p.backend.value)))
            if g.number_of_nodes()
            else AnalyticValue.Groups(())
        ),
        lambda m: not m.directed,
    ),
)

def _analyse(graph: nx.Graph, policy: FeaturePolicy, *, ops: frozenset[AnalyticOp]) -> Map[AnalyticOp, AnalyticValue]:
    selected = (spec for spec in ANALYTICS if spec.op in ops and spec.mode_guard(policy.mode))
    return Map.of_seq([(spec.op, spec.reducer(graph, policy)) for spec in selected])


def _project(mesh: trimesh.Trimesh, kind: FeatureKind, policy: FeaturePolicy) -> tuple[nx.Graph, Marks]:
    spec = FEATURE_OPS[kind]
    marks = spec.detector(mesh, policy)
    return MARK_PROJECT[spec.mark_space](mesh, spec, marks, policy), marks


def _assemble(graph: nx.Graph, marks: Marks, kind: FeatureKind, policy: FeaturePolicy, values: Map[AnalyticOp, AnalyticValue]) -> FeatureResult:
    census = Census(
        kind=kind,
        mode=policy.mode,
        backend=policy.backend,
        marks=int(marks.size),
        nodes=graph.number_of_nodes(),
        edges=graph.number_of_edges(),
        values=values,
    )
    return FeatureResult(
        kind=kind,
        census=census,
        graduation_subject=GeometrySubject.NETWORK_GRAPH,
        node_link=msgspec.json.encode(nx.node_link_data(graph, edges="edges")),
    )


def _extracted(mesh: trimesh.Trimesh, request: FeatureRequest) -> FeatureResult:
    graph, marks = _project(mesh, request.kind, request.policy)
    return _assemble(graph, marks, request.kind, request.policy, _analyse(graph, request.policy, ops=request.policy.ops))


# --- [COMPOSITION] ----------------------------------------------------------------------


class Features(Struct, frozen=True):
    mesh: trimesh.Trimesh
    lane: LanePolicy
    composition: ScopeKey = DEFAULT_SCOPE

    def run(self, request: FeatureRequest | Sequence[FeatureRequest]) -> RuntimeRail[FeatureResult] | RuntimeRail[Block[FeatureResult]]:
        match request:
            case Sequence() as batch:
                return traversed(
                    Block.of_seq([
                        evidence_run(
                            EvidenceScope.GRAPH_FEATURES, f"run.{item.kind}", lambda i=item: _extracted(self.mesh, i), composition=self.composition
                        )
                        for item in batch
                    ]),
                    by=Disposition.ACCUMULATE,
                )
            case FeatureRequest() as single:
                return evidence_run(
                    EvidenceScope.GRAPH_FEATURES, f"run.{single.kind}", lambda: _extracted(self.mesh, single), composition=self.composition
                )
            case _ as unreachable:
                assert_never(unreachable)

    async def bridged(self, request: FeatureRequest) -> RuntimeRail[FeatureResult]:
        return await evidence_run(
            EvidenceScope.GRAPH_FEATURES,
            f"bridged.{request.kind}",
            partial(self.lane.offload, Kernel.of(_extracted, KernelTrait.HOSTILE), self.mesh, request),
            composition=self.composition,
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
