# [PY_GEOMETRY_GRAPH_FEATURES]

Mesh feature extraction projected onto the analytic network graph. `extract` is one module-level entrypoint over the `FeatureKind` op-table — sharp-edge, planar, curvature, and boundary detection are four rows of one data-driven detect/project table, never four functions and never methods on a namespace class. Each row is one woven rail that stacks four `.api` capabilities in a single pass: it reads the `trimesh` cached-topology surface (`face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`edges_unique`), folds a `numpy` vectorized reduction against the `FeaturePolicy` cosine/defect threshold to mark the feature element set, lifts the marked set into a `networkx` graph through the `create_using`-keyed conversion bridge (`from_edgelist`/`from_numpy_array`), and the analytics aspect folds the closed `AnalyticOp` table (`connected_components`, `betweenness_centrality`, `pagerank`, `shortest_path`, `strongly_connected_components`, `minimum_spanning_tree`) over that one graph — so the receipt carries connectivity, centrality, and pathing in one fold rather than three inlined calls. The feature graph graduates via the compute `HandoffAxis` geometry case carrying the canonical `network-graph` `GeometrySubject` literal. This owner is the network-graph analytics consumer; raw mesh-file exchange stays at the data seam, mesh repair/boolean is the `mesh` sibling, and non-manifold cell topology is the `nonmanifold` sibling.

`trimesh`, `numpy`, and `networkx` are the intended cp315 core, so this owner rides the core directly — no companion gate, no dark band.

## [01]-[INDEX]

- [01]-[FEATURES]: the `FeatureKind` detect/project op-table, the `AnalyticOp` analytics table, the one woven extraction rail, the network projection, and the `ReceiptContributor` aspect under one typed receipt.

## [02]-[FEATURES]

- Owner: `extract` — the one module-level entrypoint dispatching the closed `FeatureKind` vocabulary through the `FEATURE_OPS` detect/project table; `FeatureKind` the `StrEnum` selecting the kind; `GraphMode` the `StrEnum` selecting graph directedness (`UNDIRECTED`/`DIRECTED`) so `create_using` is a bounded vocabulary, never a `directed: bool` knob; `FeatureSpec` the frozen row binding a kind to its `(Detector, Projection)` pair; `Analytics` the typed sub-receipt carrying the component census, the centrality leaderboard, the spanning-tree weight, and the strongly-connected-component count keyed by `AnalyticOp`; `FeatureResult` the typed receipt carrying the kind, the policy, the marked element set, the node-link graph projection, the `Analytics` block, and the dispatch backend, with the graduation subject as the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`). `FeaturePolicy` is the `msgspec.Struct` value-object carrying the `dihedral_cos`/`coplanar_cos`/`defect` thresholds, the `GraphMode`, the `centrality_top` cap, and the `pagerank_iter` power-iteration ceiling, so every threshold, cap, and solver bound is a policy field, never a positional float or a boolean knob; `contribute` on `FeatureResult` is the one AOP seam emitting the emitted-phase `Receipt.of` row, so census/telemetry is woven once on the receipt rather than re-inlined per kind.
- Cases: `FeatureKind` rows `SHARP_EDGE` (face pairs across `face_adjacency` whose `face_normals` cosine falls below `dihedral_cos`, the dihedral crease set) · `PLANAR` (the `facets` coplanar groups whose member normals stay above `coplanar_cos`, the flat-region partition) · `CURVATURE` (vertices whose `vertex_defects` angle-defect — the discrete Gaussian curvature — exceeds `defect`, the high-curvature set) · `BOUNDARY` (the deduplicated face-edge rows whose face-incidence count — from a single `numpy.unique(..., return_counts=True)` over the `faces` triangle-edge expansion — is exactly one, the open-boundary loop) — each row is a `FeatureSpec(detector, projection)`, never a dispatch branch; a fifth kind is one `FeatureKind` row plus one `FEATURE_OPS` entry. `AnalyticOp` rows `COMPONENTS`/`STRONG_COMPONENTS`/`CENTRALITY`/`PAGERANK`/`SPANNING_WEIGHT` close the analytics vocabulary — each is one `ANALYTICS` table row folding one catalogue-confirmed `networkx` algorithm family over the one projection, the directed/undirected applicability gated by the row's `mode_guard` predicate so a directed-only or undirected-only algorithm is skipped by data, never by an inline `if graph.is_directed()` branch.
- Entry: `extract(mesh, kind, policy)` takes a `trimesh.Trimesh`, a `FeatureKind`, and a `FeaturePolicy`, and returns a `RuntimeRail[FeatureResult]`; the table's `detector` marks the feature element set as a `numpy` index array, the table's `projection` lifts the marked set into a `networkx` graph via the conversion bridge (`from_edgelist`/`from_numpy_array`) under the policy `GraphMode.create_using`, and `_analyse` folds the `ANALYTICS` table into one `Analytics` block so the receipt carries connectivity, centrality, and spanning structure in one pass. `boundary` is the single exception-to-fault conversion at the host edge — the `NetworkX*` taxonomy and the `trimesh` cache faults convert exactly once here, interior code receives only the rail.
- Auto: the detector kernels never branch per kind — each is a closed `numpy` reduction over the `trimesh` cached property the row names, woven with the policy threshold in one vectorized fold: the crease set is a single `numpy.where` over the clipped row-dot of `linalg.norm`-normalized `face_normals` indexed by `face_adjacency` against `dihedral_cos` (no inverse trig); the planar set is the `facets` coplanar groups admitted only where the group's `face_normals` mean-direction agreement clears `coplanar_cos` (the policy field is read, never declared-and-ignored); curvature is `numpy.abs(vertex_defects) > defect`; the boundary loop is the deduplicated face-edge rows whose face-incidence count is one, computed by one `numpy.unique(tri_edges, axis=0, return_counts=True)` over the sorted `faces` triangle-edge expansion (`_face_edge_set`) so the incidence vector is built from the confirmed `faces` member and confirmed `unique` set-op alone — `edges_unique` is already deduplicated upstream, so its own uniqueness count never discriminates a boundary, and the per-face edge expansion is the catalogue-confirmed incidence source. The boundary detector and its `_boundary_projection` share the one `_face_edge_set` reduction, so the count-1 marks index exactly the unique edge array the projection rebuilds. The projection lifts the marked element set into the network graph through the conversion bridge keyed by kind, and the projection is paired to the mark space the detector produces so an edge-index mark and a vertex-index mark never cross-index: the crease marks are `face_adjacency` row indices and the boundary marks are `_face_edge_set` deduplicated-edge row indices, both lifted through `from_edgelist` over the array they index, the curvature marks are vertex indices lifted through a boolean vertex-flag scatter (`flags[marks]`, then `flags[edges].any(axis=1)`) selecting `edges_unique` rows incident to a high-curvature vertex (never positional edge indexing by a vertex id), and the planar facet-adjacency lifts through `from_numpy_array` over the boolean facet-adjacency matrix — all with `create_using=policy.mode.create_using` so one projection call discriminates graph kind. The analytics aspect threads the one graph through every `ANALYTICS` row whose `mode_guard` admits the projection's directedness, recording the dispatch `backend` once in the receipt facts; the node-link evidence is `msgspec.json.encode(node_link_data(graph, edges="edges"))` so the graduation payload is real JSON bytes, never a Python `repr`.
- Receipt: `FeatureResult.contribute` emits one emitted-phase `Receipt.of("emitted", "geometry.graph.features", subject, facts)` row through `ReceiptContributor`, projecting the scalar `Analytics` census (component count, strong-component count, spanning-tree weight) plus the centrality/pagerank leaderboard cardinalities, mode, marks, and node/edge counts into the flat `dict[str, str]` facts map — the tuple-of-tuple leaderboards stay off the flat facts map and ride the typed `FeatureResult.analytics` field. The `node_link` JSON bytes are the graduation evidence the compute `graduates` rail keys, and the `graduation_subject` (`network-graph`) is the `GeometrySubject` literal the compute `HandoffAxis` geometry case carries; the `GraduationReceipt` itself is the compute-side record the handoff owner mints from this subject, never minted here.
- Packages: `trimesh` (`Trimesh.faces`/`face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`edges_unique`), `numpy` (`linalg.norm`/`clip`/`sum`/`where`/`abs`/`min`/`unique`/`sort`/`reshape`/`asarray`/`zeros`/`full`), `networkx` (`from_edgelist`/`from_numpy_array`/`connected_components`/`strongly_connected_components`/`betweenness_centrality`/`pagerank`/`minimum_spanning_tree`/`node_link_data`), `msgspec` (`Struct`/`json.encode`), runtime (`RuntimeRail`/`boundary`/`ReceiptContributor`/`Receipt`), compute (`GeometrySubject`).
- Growth: a new feature kind is one `FeatureKind` row plus one `FEATURE_OPS` row binding its `(detector, projection)` pair; a new analytic is one `AnalyticOp` row plus one `ANALYTICS` row binding its `(algorithm, mode_guard, reducer)` triple; a stricter threshold or a larger leaderboard is a `FeaturePolicy` field value the caller passes; a directedness switch is a `GraphMode` row; zero new surface, no per-kind dispatch branch.
- Boundary: this owner detects features and projects them onto the `networkx` analytic graph — mesh repair/winding/boolean is the `mesh/repair` sibling over `trimesh`/`manifold3d`, non-manifold cell/aperture topology is the `nonmanifold` sibling over `topologicpy`, compas numerical/form-finding geometry is the `algebra` sibling, and raw mesh-file decode/encode plus columnar edge-list reframing into Arrow stays at the data seam. The `network-graph` subject crosses from this owner alongside the `algebra` sibling's `network-graph` arm on the one geometry `HandoffAxis` case; the two are distinct producers of the same subject (mesh-feature projection here, compas adjacency there), never folded into one file.

```python signature
from collections.abc import Callable
from enum import StrEnum
from types import MappingProxyType
from typing import Final, Mapping

import msgspec
import networkx as nx
import numpy as np
import trimesh
from msgspec import Struct
from numpy.typing import NDArray

from rasm.compute.graduation.handoff import GeometrySubject
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor

type Marks = NDArray[np.int64]
type Detector = Callable[[trimesh.Trimesh, "FeaturePolicy"], Marks]
type Projection = Callable[[trimesh.Trimesh, Marks, "FeaturePolicy"], nx.Graph]
type Reducer = Callable[[nx.Graph, "FeaturePolicy"], float | int | tuple[tuple[int, float], ...]]


class FeatureKind(StrEnum):
    SHARP_EDGE = "sharp-edge"
    PLANAR = "planar"
    CURVATURE = "curvature"
    BOUNDARY = "boundary"


class GraphMode(StrEnum):
    UNDIRECTED = "undirected"
    DIRECTED = "directed"

    @property
    def create_using(self) -> type[nx.Graph] | type[nx.DiGraph]:
        return nx.DiGraph if self is GraphMode.DIRECTED else nx.Graph


class AnalyticOp(StrEnum):
    COMPONENTS = "components"
    STRONG_COMPONENTS = "strong-components"
    CENTRALITY = "centrality"
    PAGERANK = "pagerank"
    SPANNING_WEIGHT = "spanning-weight"


class FeaturePolicy(Struct, frozen=True, gc=False):
    dihedral_cos: float = 0.5
    coplanar_cos: float = 0.999
    defect: float = 0.1
    mode: GraphMode = GraphMode.UNDIRECTED
    centrality_top: int = 8
    pagerank_iter: int = 200


class FeatureSpec(Struct, frozen=True):
    detector: Detector
    projection: Projection


class AnalyticSpec(Struct, frozen=True):
    op: AnalyticOp
    reducer: Reducer
    mode_guard: Callable[[GraphMode], bool]


class Analytics(Struct, frozen=True):
    components: int
    strong_components: int
    centrality: tuple[tuple[int, float], ...]
    pagerank: tuple[tuple[int, float], ...]
    spanning_weight: float


class FeatureResult(Struct, frozen=True):
    kind: FeatureKind
    policy: FeaturePolicy
    marks: tuple[int, ...]
    node_link: bytes
    analytics: Analytics
    nodes: int
    edges: int
    backend: str = "default"
    graduation_subject: GeometrySubject = "network-graph"

    def contribute(self) -> Receipt:
        a = self.analytics
        facts = {
            "kind": self.kind.value, "mode": self.policy.mode.value, "marks": str(len(self.marks)),
            "nodes": str(self.nodes), "edges": str(self.edges), "backend": self.backend,
            "components": str(a.components), "strong_components": str(a.strong_components),
            "spanning_weight": str(a.spanning_weight), "centrality_leaders": str(len(a.centrality)),
            "pagerank_leaders": str(len(a.pagerank)),
        }
        return Receipt.of("emitted", "geometry.graph.features", self.graduation_subject, facts)


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


def _boundary_projection(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    uniques, _ = _face_edge_set(mesh)
    return _edgelist(uniques, marks, policy)


def _vertex_edge_projection(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    edges = np.asarray(mesh.edges_unique, dtype=np.int64)
    flags = np.zeros(len(mesh.vertices), dtype=bool)
    flags[marks] = True
    touched = np.where(flags[edges].any(axis=1))[0]
    return _edgelist(edges, touched, policy)


def _crease_projection(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    return _edgelist(np.asarray(mesh.face_adjacency), marks, policy)


def _facet_projection(mesh: trimesh.Trimesh, marks: Marks, policy: FeaturePolicy) -> nx.Graph:
    face_to_pos = np.full(len(mesh.faces), -1, dtype=np.int64)
    for pos, group in enumerate(marks.tolist()):
        face_to_pos[np.asarray(mesh.facets[group], dtype=np.int64)] = pos
    pairs = np.asarray(mesh.face_adjacency, dtype=np.int64)
    ga, gb = face_to_pos[pairs[:, 0]], face_to_pos[pairs[:, 1]]
    link = (ga >= 0) & (gb >= 0) & (ga != gb)
    span = np.zeros((marks.size, marks.size), dtype=np.int64)
    span[ga[link], gb[link]] = span[gb[link], ga[link]] = 1
    return nx.from_numpy_array(span, create_using=policy.mode.create_using())


FEATURE_OPS: Final[Mapping[FeatureKind, FeatureSpec]] = MappingProxyType({
    FeatureKind.SHARP_EDGE: FeatureSpec(_sharp_edges, _crease_projection),
    FeatureKind.PLANAR: FeatureSpec(_planar_facets, _facet_projection),
    FeatureKind.CURVATURE: FeatureSpec(_curvature_vertices, _vertex_edge_projection),
    FeatureKind.BOUNDARY: FeatureSpec(_boundary_edges, _boundary_projection),
})


def _ranked(scores: Mapping[int, float], policy: FeaturePolicy) -> tuple[tuple[int, float], ...]:
    ranked = sorted(scores.items(), key=lambda kv: kv[1], reverse=True)[: policy.centrality_top]
    return tuple((int(n), float(s)) for n, s in ranked)


ANALYTICS: Final[tuple[AnalyticSpec, ...]] = (
    AnalyticSpec(AnalyticOp.COMPONENTS, lambda g, _: sum(1 for _ in nx.connected_components(g)), lambda m: m is GraphMode.UNDIRECTED),
    AnalyticSpec(AnalyticOp.STRONG_COMPONENTS, lambda g, _: sum(1 for _ in nx.strongly_connected_components(g)), lambda m: m is GraphMode.DIRECTED),
    AnalyticSpec(AnalyticOp.CENTRALITY, lambda g, p: _ranked(nx.betweenness_centrality(g), p), lambda _: True),
    AnalyticSpec(AnalyticOp.PAGERANK, lambda g, p: _ranked(nx.pagerank(g, max_iter=p.pagerank_iter), p) if g.number_of_nodes() else (), lambda _: True),
    AnalyticSpec(AnalyticOp.SPANNING_WEIGHT, lambda g, _: float(nx.minimum_spanning_tree(g).number_of_edges()), lambda m: m is GraphMode.UNDIRECTED),
)


def _analyse(graph: nx.Graph, policy: FeaturePolicy) -> Analytics:
    folded: Mapping[AnalyticOp, float | int | tuple[tuple[int, float], ...]] = {
        spec.op: spec.reducer(graph, policy) for spec in ANALYTICS if spec.mode_guard(policy.mode)
    }
    rank = lambda op: folded[op] if op in folded and isinstance(folded[op], tuple) else ()
    return Analytics(
        components=int(folded.get(AnalyticOp.COMPONENTS, 0)),
        strong_components=int(folded.get(AnalyticOp.STRONG_COMPONENTS, 0)),
        centrality=rank(AnalyticOp.CENTRALITY),
        pagerank=rank(AnalyticOp.PAGERANK),
        spanning_weight=float(folded.get(AnalyticOp.SPANNING_WEIGHT, 0.0)),
    )


def extract(mesh: trimesh.Trimesh, kind: FeatureKind, policy: FeaturePolicy = FeaturePolicy()) -> "RuntimeRail[FeatureResult]":
    return boundary(f"features.{kind}", lambda: _run(mesh, kind, policy))


def _run(mesh: trimesh.Trimesh, kind: FeatureKind, policy: FeaturePolicy) -> FeatureResult:
    spec = FEATURE_OPS[kind]
    marks = spec.detector(mesh, policy)
    graph = spec.projection(mesh, marks, policy)
    return FeatureResult(
        kind=kind,
        policy=policy,
        marks=tuple(int(m) for m in marks.tolist()),
        node_link=msgspec.json.encode(nx.node_link_data(graph, edges="edges")),
        analytics=_analyse(graph, policy),
        nodes=graph.number_of_nodes(),
        edges=graph.number_of_edges(),
    )
```

## [03]-[RESEARCH]

- [TRIMESH_CACHE_SHAPES]: the `Trimesh.face_adjacency` `(n, 2)` face-pair shape, the `face_normals` `(faces, 3)` row alignment to `faces`, the `facets` list-of-arrays member shape (each entry a face-index array per coplanar group), the `vertex_defects` `(vertices,)` angle-defect vector (discrete Gaussian curvature, `2π − Σθ`), and the `edges_unique` `(e, 2)` vertex-pair shape confirm against the folder `.api/trimesh.md` cached-property table (rows [06]–[08]) on the cp315 core interpreter; the `face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`edges_unique` members are catalogue-confirmed.
- [BOUNDARY_INCIDENCE]: the `_boundary_edges` kernel resolves single-incidence open-boundary edges through `_face_edge_set` — one `numpy.unique(tri_edges, axis=0, return_counts=True)` over the sorted `faces` triangle-edge expansion (`[[0,1],[1,2],[2,0]]` fancy-index) — and selects the count-1 rows; `faces` is the only mesh member this fold reads and `numpy.unique`/`sort`/`reshape` are catalogue-confirmed (`numpy.md` `unique` row [10], `sort` row [09], `reshape` row [01]), while `faces` is the geometry root the `.api/trimesh.md` cached-property axis is keyed off (`Trimesh` exposes derived geometry keyed off `vertices`/`faces`). The earlier `np.unique(edges_unique)` count fold was wrong — `edges_unique` is deduplicated upstream so its own uniqueness count is one for every row and marks every edge as boundary; per-face incidence is the correct discriminant. The direct trimesh open-edge accessor (`mesh.edges_face` incidence, the `mesh.facets_boundary`/outline path) is NOT on the folder catalogue and is a future density refinement gated on a catalogue admission, never a present dependency.
- [NETWORKX_ANALYTICS]: the `node_link_data` node-link projection verb (row [13]) and the analytics families the `ANALYTICS` table folds — `connected_components` (row [06], undirected set generator), `strongly_connected_components` (row [07], directed set generator), `betweenness_centrality` (row [10], node-score dict), `pagerank` (row [09], node-score dict), and `minimum_spanning_tree` (row [08], graph) — are catalogue-confirmed against `networkx` ENTRYPOINTS algorithm families; the `from_edgelist`/`from_numpy_array` conversion bridges and the `create_using` directedness axis are catalogue-confirmed (rows [07], [03]). The `networkx` catalogue lives at the branch `libs/python/data/.api/networkx.md` because `data` is the package owner of record; `graph/features` is the geometry-side consumer of the same admitted package, citing the one branch catalogue rather than minting a duplicate folder `.api`. Community detection (`louvain_communities`/greedy modularity) is the ARCHITECTURE-charter analytic NOT yet on the `networkx` catalogue — its admission is one `AnalyticOp.COMMUNITY` row plus one `ANALYTICS` triple once the community-algorithm family is catalogued, a RESEARCH-gated growth on the existing table, never a present fence.
- [MARK_SPACE_PAIRING]: each `FeatureSpec` pairs a detector to a projection that reads the detector's mark space — `SHARP_EDGE` marks are row indices into `face_adjacency` projected by `_crease_projection`'s direct `rows[marks]` selection; `BOUNDARY` marks are row indices into the `_face_edge_set` deduplicated edge array, and `_boundary_projection` rebuilds that same array so the marks index it exactly; `CURVATURE` marks are vertex indices projected through `_vertex_edge_projection`, which scatters a boolean vertex flag (`flags[marks]`) and selects `edges_unique` rows where `flags[edges].any(axis=1)` rather than indexing edge rows by a vertex id; `PLANAR` marks are facet-group indices projected through `_facet_projection` over the facet-adjacency matrix. The pairing is a `FEATURE_OPS` row property, so a detector returning a different mark space binds its matching projection in the same row and the cross-index defect (an edge projection consuming vertex marks as edge-row indices) cannot recur. All marks stay 1-D index arrays so the `FeatureResult.marks` projection (`tuple(int(m) for m in marks.tolist())`) is total across every kind.
- [NODE_LINK_EDGES_KEY]: `node_link_data` takes the `edges=` key argument selecting the edge-collection key in the emitted mapping; the explicit `edges="edges"` pin produces the stable forward-compatible node-link schema rather than the legacy `links` default, and `msgspec.json.encode` (`msgspec` ENTRYPOINTS [01]) serializes the resulting mapping to JSON bytes — the graduation payload is real JSON, never `str(dict)` Python `repr`. The exact `edges=` default transition on the cp315-core `networkx` is the gated spelling to confirm against the installed distribution before the key pin is treated as settled; the pin is the forward-compatible form regardless.
- [FACET_INCIDENCE]: the `_facet_projection` builds the facet-adjacency boolean matrix over only the marked coplanar groups via `from_numpy_array`, scattering each marked group's faces into a `face_to_pos` lookup (`numpy.full(-1)`) then masking the `face_adjacency` pairs whose two faces fall in distinct marked groups (`(ga>=0)&(gb>=0)&(ga!=gb)`), so the per-edge link is a vectorized boolean fold rather than a Python pair loop. The single `for` over marked groups is the irreducible ragged scatter (each `facets` group is a variable-length face array). The `facets` member shape (face-index array per coplanar group) and the `edges_unique`/`face_adjacency` shapes are catalogue-confirmed (folder `.api/trimesh.md` rows [07]–[08]).
