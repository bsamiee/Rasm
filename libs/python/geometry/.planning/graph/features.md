# [PY_GEOMETRY_GRAPH_FEATURES]

Mesh-feature detection projected onto the `networkx` analytic graph: sharp-edge, planar, curvature, and boundary detection are rows of one `FEATURE_OPS` detect/project table, never sibling functions, and the `ANALYTICS` table closes the connectivity/centrality/spanning/cycle/community families over that one projection, `mode_guard` skipping a directed-only or undirected-only algorithm by data rather than an inline graph-kind branch. `Features` is the mesh-feature-projection producer of `network-graph`; the `algebra` sibling is the compas-adjacency producer of the same literal, never folded into one file.

Reducer-return vocabulary (`AnalyticValue`, `ranked`, the census projections) imports downward from the tier-0 `graph/analytic` substrate, no local twin. `@receipted` sits on the one pure module-level `_extracted` both paths share, fence OUTSIDE — `run`'s `boundary` on the sync arm, the lane's `async_boundary` on the bridged arm — matching the sibling wiring, and `bridged` ships that module-qualified body `REFERENCE` as a `KernelTrait.HOSTILE` kernel onto the warm process pool with only the mesh and request as crossing payload and zero geometry-minted limiters, because the `trimesh`/`numpy` detector band imports under no isolated subinterpreter. Each feature graph graduates under `GeometrySubject.NETWORK_GRAPH`; the compute crossing is the carrier's `wire()` data, never an import.

## [01]-[INDEX]

- [01]-[FEATURES]: `FEATURE_OPS` detect/project table, `MARK_PROJECT` projection algebra, `ANALYTICS` reducer table, and the `run`/`bridged` pair under one `ReceiptContributor`.

## [02]-[FEATURES]

- Owner: `Features` holds the conditioned `trimesh.Trimesh`. `GraphMode` resolves `create_using` over the full `Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` family, so directedness and multiplicity form one bounded vocabulary, never a `directed`/`multi` knob pair; `GraphBackend` threads once as `backend=` into every reducer, never forked per call site nor mutating a global `nx.config`; `MarkSpace` keys `MARK_PROJECT` dispatch, so a detector's mark space and its projection cannot cross-index — the kinds reuse two edge arms plus one facet arm. Every threshold, cap, solver bound, and analytic toggle is a `FeaturePolicy` field; `power_iter` caps power iteration, threaded as `max_iter` into the eigenvector/pagerank reducers.
- Entry: `run` discriminates a single request or a batch; the `NetworkX*` taxonomy (including `PowerIterationFailedConvergence`) and trimesh cache faults convert exactly once at the fence. `bridged` is NOT itself `@receipted` and never collapses an offload fault into a synthetic empty result — a failure stays an `Error(BoundaryFault)` on the returned rail.
- Receipt: phase is data-driven — `emitted` for a graph with nodes, `admitted` for a vacuous feature set; `_HEAD_OPS` centrality rows read `peak` so the load-bearing centrality signal survives onto the flat facts map, count/partition rows read `as_scalar`, and leaderboards/partitions stay OFF the flat map on the typed `Census.values`; every kind gates `empty_graph_fraction` against the zero ceiling, so a no-node projection does not graduate; node-link evidence is real JSON bytes, never a Python `repr`.
- Packages: `trimesh`, `numpy`, and `networkx` per the fence imports; the analytic vocabulary and the graduation spine import downward from their geometry owners.
- Growth: a new feature kind is one `FeatureKind` row plus one `FEATURE_OPS` row and one `CASE` row; a new mark space is one `MarkSpace` member plus one `MARK_PROJECT` arm; a new analytic is one `AnalyticOp` row plus one `ANALYTICS` row — plus `_HEAD_OPS` membership when its flat fact is the extremum rather than the count; a new `AnalyticValue` shape lands on the `graph/analytic` owner; a threshold, cap, selection, or backend switch is a `FeaturePolicy` value.
- Boundary: mesh repair/winding/boolean is the `mesh/repair` sibling's over `trimesh`/`manifold3d`; non-manifold cell/aperture topology is the `nonmanifold` sibling's; compas numerical/form-finding is the `algebra` sibling's; raw mesh-file decode/encode and columnar edge-list reframing stay at the data seam. Both `network-graph` producers cross on the one geometry `HandoffAxis` case — mesh-feature projection here, compas adjacency there.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Mapping, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final, Literal, assert_never

import msgspec
import networkx as nx
import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs
from numpy.typing import NDArray

from rasm.geometry.graduation import GeometryHandoff, GeometrySubject
from rasm.geometry.graph.analytic import AnalyticValue, peak_of, ranked, scalar_of
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

type Phase = Literal["admitted", "emitted"]
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
    # Projection-dispatch key: EDGE_ROW marks index the spec's EdgeSource array directly, VERTEX marks
    # scatter incidence onto it, FACET marks build the facet-adjacency matrix, MARK_PROJECT folds.
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

# Default menu; row mode_guard is the per-graph filter pruning directed-only rows on an undirected graph.
_DEFAULT_OPS: Final[frozenset[AnalyticOp]] = frozenset({
    AnalyticOp.COMPONENTS,
    AnalyticOp.STRONG_COMPONENTS,
    AnalyticOp.BETWEENNESS,
    AnalyticOp.PAGERANK,
    AnalyticOp.SPANNING_WEIGHT,
})
# centrality band whose flat fact IS the top head score, not the board cardinality — one membership test, no branch.
_HEAD_OPS: Final[frozenset[AnalyticOp]] = frozenset({
    AnalyticOp.BETWEENNESS,
    AnalyticOp.DEGREE,
    AnalyticOp.CLOSENESS,
    AnalyticOp.EIGENVECTOR,
    AnalyticOp.PAGERANK,
})
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # feature facts carry no secret field
_EMPTY_CEILING: Final[Mapping[str, float]] = MappingProxyType({"empty_graph_fraction": 0.0})

# --- [MODELS] ---------------------------------------------------------------------------


class FeaturePolicy(Struct, frozen=True, gc=False):
    dihedral_cos: float = 0.5
    coplanar_cos: float = 0.999
    defect: float = 0.1
    mode: GraphMode = GraphMode.UNDIRECTED
    backend: GraphBackend = GraphBackend.DEFAULT
    centrality_top: int = 8
    power_iter: int = 200  # eigenvector/pagerank power-iteration ceiling both reducers thread as max_iter
    ops: frozenset[AnalyticOp] = _DEFAULT_OPS


class FeatureRequest(Struct, frozen=True):
    kind: FeatureKind
    policy: FeaturePolicy = FeaturePolicy()


class FeatureSpec(Struct, frozen=True, gc=False):
    # one row per FeatureKind: detector, the (n, 2) edge array the EDGE_ROW/VERTEX projections lift (FACET ignores it), MarkSpace.
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

    def scalar(self, op: AnalyticOp) -> float:
        return peak_of(self.values, op) if op in _HEAD_OPS else scalar_of(self.values, op)

    def facts(self) -> dict[str, object]:
        flat = {key: value for key, value in structs.asdict(self).items() if key != "values"}
        return flat | {op.value: self.scalar(op) for op in self.values}


class CaseSpec(Struct, frozen=True):
    # one row per FeatureKind: subject, Census-read empty-graph ledger, ceiling.
    subject: GeometrySubject
    ledger: Callable[[Census], dict[str, float]]
    ceiling: Mapping[str, float]


class FeatureResult(Struct, frozen=True):
    kind: FeatureKind
    census: Census
    node_link: bytes
    graduation_subject: GeometrySubject

    def contribute(self) -> Iterable[Receipt]:
        phase: Phase = "emitted" if self.census.nodes else "admitted"
        return (Receipt.of("geometry.graph.features", (phase, self.graduation_subject, self.census.facts())),)

    def graduates(self, evidence_key: ContentKey) -> GeometryHandoff:
        spec = CASE[self.kind]
        return GeometryHandoff.of(self.graduation_subject, evidence_key, spec.ledger(self.census), spec.ceiling)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _unit_dots(normals: NDArray[np.float64], pairs: NDArray[np.int64]) -> NDArray[np.float64]:
    units = normals / np.clip(np.linalg.norm(normals, axis=1, keepdims=True), 1e-12, None)
    return np.clip(np.sum(units[pairs[:, 0]] * units[pairs[:, 1]], axis=1), -1.0, 1.0)


def _sharp_edges(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    pairs = np.asarray(mesh.face_adjacency)
    dots = _unit_dots(np.asarray(mesh.face_normals), pairs)
    return np.asarray(np.where(dots < policy.dihedral_cos)[0], dtype=np.int64)


def _planar_facets(mesh: trimesh.Trimesh, policy: FeaturePolicy) -> Marks:
    # `mesh.facets` is a ragged list of per-group face-index arrays; a group admits where its members' min dot against the
    # unit-mean normal clears `coplanar_cos`, the `m :=` walrus binding the group mean once for normalize and reduction.
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
    # `edges_face` is the (e, 2) per-edge face accessor aligned to `edges_unique`,
    # `-1` filling an open side; a single incident face is the open-boundary discriminant. No expansion.
    incidence = (np.asarray(mesh.edges_face, dtype=np.int64) >= 0).sum(axis=1)
    return np.asarray(np.where(incidence == 1)[0], dtype=np.int64)


# EdgeSource arms the EDGE_ROW/VERTEX projections lift; FACET binds `_no_edges` since it reads `face_adjacency`/`facets` directly.
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
    # A vertex-index mark selects the `edges_unique` rows incident to a marked vertex, not indexing edge
    # rows by a vertex id: scatter the flag, then keep rows touching a flagged endpoint.
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


# each reducer threads `backend=policy.backend.value` into its `@nx._dispatchable` algorithm and returns one typed `AnalyticValue`.
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

CASE: Final[Mapping[FeatureKind, CaseSpec]] = MappingProxyType({
    kind: CaseSpec(subject, lambda c: {"empty_graph_fraction": 0.0 if c.nodes else 1.0}, _EMPTY_CEILING)
    for kind, subject in (
        (FeatureKind.SHARP_EDGE, GeometrySubject.NETWORK_GRAPH),
        (FeatureKind.PLANAR, GeometrySubject.NETWORK_GRAPH),
        (FeatureKind.CURVATURE, GeometrySubject.NETWORK_GRAPH),
        (FeatureKind.BOUNDARY, GeometrySubject.NETWORK_GRAPH),
    )
})


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
        kind=kind, census=census, graduation_subject=CASE[kind].subject, node_link=msgspec.json.encode(nx.node_link_data(graph, edges="edges"))
    )


@receipted(_REDACTION)
def _extracted(mesh: trimesh.Trimesh, request: FeatureRequest) -> FeatureResult:
    # the one pure extraction body both arms share, module-qualified so the bridged kernel ships REFERENCE and
    # only the numpy-backed mesh plus the frozen request pickle — never an owner closure whose lane drags
    # loop-side runtime state across the process seam; the aspect emits on worker-side exit.
    graph, marks = _project(mesh, request.kind, request.policy)
    return _assemble(graph, marks, request.kind, request.policy, _analyse(graph, request.policy, ops=request.policy.ops))


# --- [COMPOSITION] ----------------------------------------------------------------------


class Features(Struct, frozen=True):
    mesh: trimesh.Trimesh
    lane: LanePolicy

    def run(self, request: FeatureRequest | Sequence[FeatureRequest]) -> RuntimeRail[FeatureResult] | RuntimeRail[Block[FeatureResult]]:
        # a batch folds a Block of the same fenced rail through traversed(ACCUMULATE); `i=item` binds the loop variable per closure.
        match request:
            case Sequence() as batch:
                return traversed(
                    Block.of_seq([boundary(f"features.{item.kind}", lambda i=item: _extracted(self.mesh, i)) for item in batch]),
                    by=Disposition.ACCUMULATE,
                )
            case FeatureRequest() as single:
                return boundary(f"features.{single.kind}", lambda: _extracted(self.mesh, single))
            case _ as unreachable:
                assert_never(unreachable)

    async def bridged(self, request: FeatureRequest) -> RuntimeRail[FeatureResult]:
        # HOSTILE: the trimesh detector band is native and the all-pairs/enumeration cores (betweenness/pagerank/
        # simple_cycles) are CPU-bound; the module-qualified `_extracted` resolves by name on the warm process pool
        # and its arguments carry the whole crossing payload.
        return await self.lane.offload(Kernel.of(_extracted, KernelTrait.HOSTILE), self.mesh, request)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
