# [PY_GEOMETRY_GRAPH_FEATURES]

`Features` is one `ReceiptContributor` producer minting a `FeatureResult` whose `run` discriminates a single `FeatureRequest` or a batch `Sequence[FeatureRequest]` over the `FEATURE_OPS` detect/project table. Sharp-edge, planar, curvature, and boundary detection are four rows of one data-driven table, never four functions and never sibling methods on a namespace class. Each row is one woven rail stacking four `.api` surfaces in a single pass: it reads the `trimesh` cached-topology algebra (`face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`edges_face`/`edges_unique`), folds a `numpy` vectorized reduction against the `FeaturePolicy` cosine/defect threshold to mark the feature element set, lifts the marked set into a `networkx` payload through the `MARK_PROJECT` table keyed by the row's `MarkSpace` (`from_edgelist`/`from_numpy_array` under one `create_using`), and the analytics aspect folds the closed `AnalyticOp` table over that one graph. The receipt carries the whole census in one fold rather than per-analytic inlined calls.

The analytics table closes five families under one `backend=` dispatch row: connectivity (`connected_components`/`weakly_connected_components`/`strongly_connected_components`), centrality (`betweenness_centrality`/`degree_centrality`/`closeness_centrality`/`eigenvector_centrality`/`pagerank`), spanning structure (`minimum_spanning_tree`), cycle census (`simple_cycles`), and community partition (`nx.community.louvain_communities`). The row `mode_guard` skips a directed-only or undirected-only algorithm by data, never by an inline `if graph.is_directed()` branch.

The feature graph graduates through `FeatureResult.graduates`, minting the geometry `GraduationReceipt` over `HandoffAxis(geometry="network-graph")` on the same compute admission rail the `algebra` sibling rides for the identical subject. This owner is the mesh-feature-projection producer of `network-graph`; the `algebra` sibling is the compas-adjacency producer of the same literal, never folded into one file. Raw mesh-file exchange stays at the data seam, mesh repair/boolean is the `mesh` sibling, and non-manifold cell topology is the `nonmanifold` sibling.

`trimesh`, `numpy`, and `networkx` ride the runtime lane directly.

## [01]-[INDEX]

- [01]-[FEATURES]: the `FeatureKind` detect/project op-table, the `MarkSpace`-keyed `MARK_PROJECT` projection algebra over the spec's `EdgeSource`, the `AnalyticOp`/`AnalyticValue` analytics table folding to one typed `Census`, the `CASE` ledger/ceiling spec table, the `Features` producer capsule with its `@receipted` `_extract` (sync) and `_offload` (async) bodies, its polymorphic single-or-batch `run`, and its async `bridged` fence mirror routing the `@receipted` `_offload` heavy/light split through `_ANALYTIC_LIMITER`-bounded `anyio.to_thread.run_sync` and returning the bare `RuntimeRail`, the typed `FeatureResult`, and the per-kind `graduates` rail under one `ReceiptContributor`.

## [02]-[FEATURES]

- Owner: `Features` — the producer capsule holding the conditioned `trimesh.Trimesh`, its polymorphic `run` discriminating a single `FeatureRequest` or a batch through the `FEATURE_OPS` detect/project table; `@receipted(_REDACTION)` wraps the pure `_extract` (sync) and `_offload` (async heavy/light split) bodies so each harvests its `FeatureResult.contribute` stream on exit — the sync arm through `Signals.emit`, the async arm through `Signals.emit_async` off the aspect's `iscoroutinefunction` dispatch — while the `boundary`/`async_boundary` fence sits OUTSIDE in `run`/`bridged`, so a raise is an `Error(BoundaryFault)` on the rail the aspect never falsely emits and the public surface is the contributor the caller reads, never a per-op rail the aspect discards. `FeatureKind` selects the kind; `GraphMode` resolves `create_using` over the full `nx.Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` family through its `MODE_CREATE` row, so directedness and multiplicity are a bounded vocabulary, never a `directed: bool`/`multi: bool` knob pair; `GraphBackend` names the `@nx._dispatchable` dispatch backend (`DEFAULT`/`PARALLEL`/`CUGRAPH`/`GRAPHBLAS`), threaded once as `backend=` into every reducer rather than forked per call site; `MarkSpace` tags which index space a detector's marks live in (`EDGE_ROW`/`VERTEX`/`FACET`) and is the key the `MARK_PROJECT` table dispatches the projection on, so a `FeatureSpec`'s detector and projection cannot cross-index and the projection is one data row per space rather than a function reference threaded per kind. `FeatureSpec` binds a kind to its `(Detector, EdgeSource, MarkSpace)` triple — `EdgeSource` resolving the `(n, 2)` index array the `EDGE_ROW`/`VERTEX` projections lift, so the four kinds reuse two projection arms rather than four projection functions; `AnalyticSpec` binds an `AnalyticOp` to its `(reducer, mode_guard)` pair; `AnalyticValue` the `@tagged_union` collapsing every reducer return — a `scalar` `float`, a `leaderboard` `Leaders`, or a `groups` `Partition` partition — to one typed carrier owning its own dual projection (`as_scalar` the cardinality, `peak` the head-magnitude) so `_analyse` folds to `Map[AnalyticOp, AnalyticValue]` with no `isinstance` reconstruction and no module-level `_peak` fold beside it. `Census` the typed value object holding the kind/mode/backend tags, node/edge/marks counts, and that `Map`, projected to facts through `msgspec.structs.asdict` with the `_HEAD_OPS` centrality rows reading `peak` and the count/partition rows reading `as_scalar`. `FeatureResult` the sole `ReceiptContributor`, carrying the kind, the `Census`, the node-link graph projection, and the case-keyed `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`). `FeaturePolicy` the `msgspec.Struct` value object carrying the `dihedral_cos`/`coplanar_cos`/`defect` thresholds, the `GraphMode`, the `GraphBackend`, the `centrality_top` leaderboard cap, the `power_iter` power-iteration ceiling, and the `AnalyticOp` selection set, so every threshold, cap, solver bound, and analytic toggle is a policy field, never a positional float or a boolean knob.
- Cases: `FeatureKind` rows `SHARP_EDGE` (face pairs across `face_adjacency` whose `face_normals` cosine falls below `dihedral_cos`, the dihedral crease set) · `PLANAR` (the `facets` coplanar groups whose member normals stay above `coplanar_cos`, the flat-region partition) · `CURVATURE` (vertices whose `vertex_defects` angle-defect — the discrete Gaussian curvature — exceeds `defect`, the high-curvature set) · `BOUNDARY` (the `edges_unique` rows whose `edges_face` incidence count is exactly one — the open-boundary loop, read off the catalogue-confirmed `edges_face` per-edge face accessor rather than a hand-rolled triangle-edge expansion) — each row a `FeatureSpec(detector, edge_source, mark_space)`, never a dispatch branch; a fifth kind is one `FeatureKind` row plus one `FEATURE_OPS` entry. `AnalyticOp` rows close the analytics vocabulary across five families — components (`COMPONENTS`/`WEAK_COMPONENTS`/`STRONG_COMPONENTS`), centrality (`BETWEENNESS`/`DEGREE`/`CLOSENESS`/`EIGENVECTOR`/`PAGERANK`), spanning (`SPANNING_WEIGHT`), cycles (`CYCLES`), community (`COMMUNITY`) — each one `ANALYTICS` row folding one catalogue-confirmed `networkx` algorithm over the one projection, its directed/undirected applicability gated by the row's `mode_guard` predicate.
- Entry: `Features.run(request)` is the one polymorphic entrypoint discriminating a single `FeatureRequest` or a batch `Sequence[FeatureRequest]`. A single request lifts `_extract` through `boundary(f"features.{kind}", ...)` — the single exception-to-fault conversion at the host edge, where the `NetworkX*` taxonomy (including `PowerIterationFailedConvergence` from an unconverged eigen/pagerank pass) and the `trimesh` cache faults convert exactly once, interior code receiving only the rail. A batch builds a `Block` of per-request rails in one comprehension and folds them through `runtime.faults.traversed` (`Disposition.ACCUMULATE`) so one fault stays addressable in the aggregate while every successful `FeatureResult` already landed on the contributed stream. `bridged` is the bare async fence mirror returning `RuntimeRail[FeatureResult]`: it routes the `@receipted` `_offload` body — whose heavy analytics band (`betweenness_centrality`/`pagerank`/`simple_cycles`, all-pairs and enumeration cores that must not block the event loop) hoists through `_ANALYTIC_LIMITER`-bounded `anyio.to_thread.run_sync` while the cheap rows fold inline — through `async_boundary` so the aspect emits on `_offload`'s exit through `emit_async` exactly as the sync arm emits on `_extract`'s, and a concurrent fan of bridged analytics holds at most `_ANALYTIC_LIMITER` worker slots rather than draining anyio's runtime-shared default 40-token pool. `bridged` is not itself `@receipted` and never `.default_value`-collapses an offload fault into a synthetic empty result the aspect would falsely emit as `admitted`; a failed offload stays an `Error(BoundaryFault)` on the returned rail exactly as the `algebra` sibling's `bridged` holds, the `_SOLVER_LIMITER`-bound fence-outside-the-aspect wiring applied to the analytics band rather than a parallel async surface.
- Auto: the detector kernels never branch per kind — each is a closed `numpy` reduction over the `trimesh` cached property the row names, woven with the policy threshold in one vectorized fold. The crease set is a single `numpy.where` over the clipped row-dot of `linalg.norm`-normalized `face_normals` indexed by `face_adjacency` against `dihedral_cos` (no inverse trig); the planar set is the `facets` coplanar groups admitted only where the group's `face_normals` mean-direction agreement clears `coplanar_cos`; curvature is `numpy.abs(vertex_defects) > defect`; the boundary loop is the `edges_unique` rows whose `edges_face` incidence is a single face, computed by one `numpy.where` over `(edges_face >= 0).sum(axis=1) == 1` — the catalogue-confirmed `edges_face` `(e, 2)` per-edge face accessor (`-1` filling the open side) is the incidence source, so a count-1 mark indexes `edges_unique` directly and the detector and its projection share that one array with no triangle-edge expansion. The projection is data-driven on the row's `MarkSpace` through the `MARK_PROJECT` table: `EDGE_ROW` and `VERTEX` both resolve a `(n, 2)` edge array from the row's `EdgeSource` and lift `rows[marks]` through `from_edgelist` — `VERTEX` first scatters a boolean vertex flag (`flags[marks]`, then `flags[edges].any(axis=1)`) so a vertex-index mark selects the incident `edges_unique` rows rather than indexing edge rows by a vertex id — while `FACET` builds the boolean facet-adjacency matrix through `from_numpy_array`, so an edge-index mark and a vertex-index mark never cross-index. All lift through `create_using=policy.mode.create_using` so one call discriminates graph kind, and `backend=policy.backend.value` threads the dispatch backend into every reducer. The node-link evidence is `msgspec.json.encode(node_link_data(graph, edges="edges"))` so the graduation payload is real JSON bytes, never a Python `repr`.
- Receipt: `FeatureResult` conforms to `ReceiptContributor`; `contribute` returns the one-element `tuple[Receipt, ...]` the port streams, minting `Receipt.of("geometry.graph.features", (phase, subject, facts))` per the runtime two-argument factory — the `(Phase, subject, facts)` evidence triple minting the `fact` case. The phase is data-driven: `phase="emitted"` for a graph that produced nodes and `phase="admitted"` for a vacuous feature set (no marks, an empty graph) the census flags as the entry caveat rather than asserting a result. The facts ride as native `dict[str, object]` — the `Census` projected through `msgspec.structs.asdict` carrying the kind/mode/backend tags, the node/edge/marks counts, and one native scalar per selected analytic: the count/spanning/cycle/community rows their `as_scalar` cardinality (component/weak/strong counts, spanning-tree weight, cycle count, community count) and the `_HEAD_OPS` centrality rows their `peak` top head score (the max betweenness/degree/closeness/eigenvector/pagerank value, not the board length, so the load-bearing centrality signal survives onto the flat map) — never `str()`-coerced, because the `observability/receipts#RECEIPT` `Encoder(enc_hook=repr, order="deterministic")` renderer serializes native scalars and a pre-`str()` map is the deleted form that owner rejects. The tuple-of-tuple leaderboards and the community partition stay off the flat facts map and ride the typed `Census.values` `Map`. `FeatureResult.graduates(evidence_key)` routes the per-kind `CASE.ledger`/`CASE.ceiling` through the one `GraduationReceipt.graduates(source_package, HandoffAxis(geometry=subject), evidence_key, measured, ceiling)` admission — the same residual-over-ceiling fold the `algebra` and `nonmanifold` siblings feed — gating an `empty_graph_fraction` against the zero ceiling so a vacuous projection (no nodes) is an `Error(BoundaryFault)` rather than a graduated handoff. The `node_link` JSON bytes are the graduation evidence that fold keys; the census is the evidence it reads, never a re-measured value.
- Packages: `trimesh` (`Trimesh.face_adjacency`/`face_normals`/`vertex_defects`/`facets`/`edges_face`/`edges_unique`/`vertices`/`faces`), `numpy` (`linalg.norm`/`clip`/`sum`/`where`/`abs`/`asarray`/`zeros`/`full`/`empty`), `networkx` (`from_edgelist`/`from_numpy_array`/`connected_components`/`weakly_connected_components`/`strongly_connected_components`/`betweenness_centrality`/`degree_centrality`/`closeness_centrality`/`eigenvector_centrality`/`pagerank`/`minimum_spanning_tree`/`simple_cycles`/`community.louvain_communities`/`node_link_data`, the `Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` payload family, the `create_using`/`backend=` axes), `msgspec` (`Struct`/`structs.asdict`/`json.encode`), `anyio` (`to_thread.run_sync`/`CapacityLimiter` the bounded heavy-analytics offload pool), `expression` (`tagged_union`/`case`/`tag`, `Block`/`Map`), runtime (`RuntimeRail`/`boundary`/`async_boundary`/`traversed`/`Disposition`, `Receipt`/`ReceiptContributor`/`Redaction`/`receipted`, `ContentKey`), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`).
- Growth: a new feature kind is one `FeatureKind` row plus one `FEATURE_OPS` row binding its `(detector, edge_source, mark_space)` triple and one `CASE` ledger/ceiling row; a new mark space is one `MarkSpace` member plus one `MARK_PROJECT` arm; a new analytic is one `AnalyticOp` row plus one `ANALYTICS` row binding its `(reducer, mode_guard)` pair, plus one `_HEAD_OPS` membership when its flat fact is the extremum rather than the count; a new `AnalyticValue` shape is one case plus one arm in each projection (`as_scalar` and `peak`); a stricter threshold, a larger leaderboard, a different analytic selection, or a directedness/backend switch is a `FeaturePolicy` field value the caller passes; zero new surface, no per-kind dispatch branch.
- Boundary: this owner detects features and projects them onto the `networkx` analytic graph — mesh repair/winding/boolean is the `mesh/repair` sibling over `trimesh`/`manifold3d`, non-manifold cell/aperture topology is the `nonmanifold` sibling over `topologicpy`, compas numerical/form-finding geometry is the `algebra` sibling, and raw mesh-file decode/encode plus columnar edge-list reframing into Arrow stays at the data seam. The `network-graph` subject crosses from this owner alongside the `algebra` sibling's `network-graph` arm on the one geometry `HandoffAxis` case; the two are distinct producers of the same subject (mesh-feature projection here, compas adjacency there), never folded into one file. Backend selection rides the policy `GraphBackend` row threaded as `backend=`, never a forked per-backend call site nor a global `nx.config` mutation this owner reaches across. The deleted forms: a flat module-level `extract` function in place of the polymorphic single-or-batch `Features.run` capsule with its async `bridged` mirror; a `_extract` constructing `FeatureResult` against fields it never declares (`policy`/`marks`/`nodes`/`edges`) where the `Census` carries the counts and `FeatureResult` carries only `(kind, census, node_link, graduation_subject)`; a reducer threading a phantom `p.pagerank_iter` where the policy field is `power_iter`; a `Census(values=...)` constructed without its `kind`/`mode`/`backend`/`marks`/`nodes`/`edges` slots where `_analyse` returns the full census; four projection functions threaded per kind where the `MARK_PROJECT` table dispatches two arms on `MarkSpace`; a hand-rolled `numpy.unique` triangle-edge expansion where the catalogue-confirmed `edges_face` accessor is the incidence source; a hand-built `dict[str, object]` facts map where `structs.asdict` projects the typed `Census`; a constant `phase="emitted"` where the empty-graph verdict data-drives the phase; a four-positional `Receipt.of(phase, owner, subject, facts)` against the runtime two-argument contract; a `str()`-coerced `dict[str, str]` facts map; an unbounded `anyio.to_thread.run_sync` analytics offload draining the default 40-token pool where `_ANALYTIC_LIMITER` bounds the heavy band exactly as the `algebra` sibling's `_SOLVER_LIMITER` does; a second batch method where the one `traversed` fold drains a `Sequence`; a `bridged` that `.default_value`-collapses an offload fault into a synthetic empty `FeatureResult` the `@receipted` aspect falsely emits as an `admitted` receipt where `bridged` returns the bare `RuntimeRail[FeatureResult]` and a raise stays an `Error(BoundaryFault)` the caller reads; a `bridged` doubly `@receipted` over an unwrapped value where the aspect lives on the inner `_offload` body and `bridged` is the bare async fence mirror exactly as the `algebra` sibling wires it; an async `_offload` body the `@receipted` aspect never wraps where the sync `_extract` carries the aspect, leaving the bridged path emitting through a `bridged`-level aspect over a fault-collapsed value.

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
type EdgeArray = NDArray[np.int64]
type GraphFamily = type[nx.Graph] | type[nx.DiGraph] | type[nx.MultiGraph] | type[nx.MultiDiGraph]
type Detector = Callable[[trimesh.Trimesh, "FeaturePolicy"], Marks]
type EdgeSource = Callable[[trimesh.Trimesh], EdgeArray]
type Projection = Callable[[trimesh.Trimesh, "FeatureSpec", Marks, "FeaturePolicy"], nx.Graph]
type Reducer = Callable[[nx.Graph, "FeaturePolicy"], "AnalyticValue"]
type Leaders = tuple[tuple[int, float], ...]
type Partition = tuple[tuple[int, ...], ...]


class FeatureKind(StrEnum):
    SHARP_EDGE = "sharp-edge"
    PLANAR = "planar"
    CURVATURE = "curvature"
    BOUNDARY = "boundary"


class MarkSpace(StrEnum):
    # the projection-dispatch key: EDGE_ROW marks index the spec's EdgeSource array directly, VERTEX
    # marks scatter incidence onto it, FACET marks build the facet-adjacency matrix. MARK_PROJECT folds.
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

# the default analytic selection a `FeaturePolicy` carries: the mode_guard then prunes the directed-
# only rows on an undirected graph, so this set is the menu and the guard is the per-graph filter.
_DEFAULT_OPS: Final[frozenset[AnalyticOp]] = frozenset(
    {AnalyticOp.COMPONENTS, AnalyticOp.STRONG_COMPONENTS, AnalyticOp.BETWEENNESS, AnalyticOp.PAGERANK, AnalyticOp.SPANNING_WEIGHT}
)
# the all-pairs / enumeration band whose blocking cores `bridged` hoists to a worker thread so the
# event loop never stalls; every other reducer is cheap enough to fold inline on either path.
_HEAVY_OPS: Final[frozenset[AnalyticOp]] = frozenset({AnalyticOp.BETWEENNESS, AnalyticOp.PAGERANK, AnalyticOp.CYCLES})
# the centrality band whose flat fact IS the top head score, not the board cardinality: `Census.facts`
# projects `peak()` for these rows and `as_scalar()` for the count/partition rows, so a betweenness fact
# rides its max centrality while a component-count fact rides its count — one membership test, no branch.
_HEAD_OPS: Final[frozenset[AnalyticOp]] = frozenset(
    {AnalyticOp.BETWEENNESS, AnalyticOp.DEGREE, AnalyticOp.CLOSENESS, AnalyticOp.EIGENVECTOR, AnalyticOp.PAGERANK}
)
# this owner's OWN bounded worker pool the heavy-band offload holds: a concurrent fan of bridged analytics
# holds at most four slots rather than draining anyio's runtime-shared default 40-token pool — the
# explicit-limiter law every bounded subsystem rides. The per-owner 4-slot PATTERN the `graph/algebra.md`
# `_SOLVER_LIMITER` and `graph/nonmanifold.md` `_ANALYTIC_LIMITER` siblings each mint INDEPENDENTLY, never a
_ANALYTIC_LIMITER: Final[anyio.CapacityLimiter] = anyio.CapacityLimiter(4)
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # feature facts carry no secret field
# a vacuous projection (no nodes) breaches the zero ceiling, so an empty feature graph does not graduate.
_EMPTY_CEILING: Final[Mapping[str, float]] = MappingProxyType({"empty_graph_fraction": 0.0})

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
        # the cardinality projection: a scalar carries its value, a leaderboard / partition carries its
        # member count, so a count-keyed analytic reads one `float` off the flat facts map.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return v
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return float(len(rows))
            case AnalyticValue(tag="groups", groups=partition):
                return float(len(partition))
            case _ as unreachable:
                assert_never(unreachable)

    def peak(self) -> float:
        # the head-magnitude projection where the extremum is the signal, not the count: a scalar IS its
        # peak, a leaderboard its top `numpy.asarray(...).max` head score, a partition its member count —
        # the one fold that lets a centrality fact ride its max score off the same union `as_scalar` reads.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return v
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return float(np.asarray([score for _, score in rows]).max(initial=0.0))
            case AnalyticValue(tag="groups", groups=partition):
                return float(len(partition))
            case _ as unreachable:
                assert_never(unreachable)


class FeatureSpec(Struct, frozen=True, gc=False):
    # one row per FeatureKind: the detector, the (n, 2) edge array the EDGE_ROW/VERTEX projection lifts
    # (FACET ignores it), and the MarkSpace MARK_PROJECT dispatches on — so a detector's mark space and
    # its projection cannot drift apart and the four kinds reuse two edge arms plus one facet arm.
    detector: Detector
    edge_source: EdgeSource
    mark_space: MarkSpace


class AnalyticSpec(Struct, frozen=True, gc=False):
    op: AnalyticOp
    reducer: Reducer
    mode_guard: Callable[[GraphMode], bool]


class Census(Struct, frozen=True):
    # the native tag/count scalars `structs.asdict` projects straight onto the receipt facts; `values`
    # is the typed analytics carrier `facts` strips and replaces with one `_HEAD_OPS`-keyed `peak`/`as_scalar`
    # projection per op, so the tuple-of-tuple leaderboards and partitions never flatten onto the map.
    kind: FeatureKind
    mode: GraphMode
    backend: GraphBackend
    marks: int
    nodes: int
    edges: int
    values: Map[AnalyticOp, AnalyticValue]

    def scalar(self, op: AnalyticOp) -> float:
        proj = AnalyticValue.peak if op in _HEAD_OPS else AnalyticValue.as_scalar
        return self.values.try_find(op).map(proj).default_value(0.0)

    def facts(self) -> dict[str, object]:
        # the count/partition rows project `as_scalar` (cardinality), the centrality rows `peak` (top
        # head score), so the flat `dict[str, object]` carries the load-bearing scalar per analytic kind.
        flat = {key: value for key, value in structs.asdict(self).items() if key != "values"}
        return flat | {op.value: self.scalar(op) for op in self.values}


class CaseSpec(Struct, frozen=True):
    # one row per FeatureKind: the subject the case crosses and the Census-read empty-graph ledger plus
    # ceiling the graduation fold gates, so a vacuous projection does not graduate and a new kind is one row.
    subject: GeometrySubject
    ledger: Callable[[Census], dict[str, float]]
    ceiling: Mapping[str, float]


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
            spec.ledger(self.census), dict(spec.ceiling),
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
    # `mesh.facets` is a ragged list of per-group face-index arrays, so the per-facet mean-direction
    # agreement is a comprehension over the groups — never a mutable `list.append` accumulator. Each
    # group admits where its members' min dot against the unit-mean normal clears `coplanar_cos`; the
    # `m :=` walrus binds the group mean once for both the normalize and the agreement reduction.
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
    # `edges_face` is the catalogue-confirmed (e, 2) per-edge face accessor aligned to `edges_unique`,
    # `-1` filling an open side; a single incident face is the open-boundary discriminant. No expansion.
    incidence = (np.asarray(mesh.edges_face, dtype=np.int64) >= 0).sum(axis=1)
    return np.asarray(np.where(incidence == 1)[0], dtype=np.int64)


# the three EdgeSource arms the EDGE_ROW/VERTEX projections lift; FACET binds `_no_edges` since it
# reads `face_adjacency`/`facets` directly. A new edge-keyed kind is one EdgeSource plus one FEATURE_OPS row.
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
    # a vertex-index mark selects the `edges_unique` rows incident to a marked vertex rather than
    # indexing edge rows by a vertex id: scatter the flag, then keep rows touching a flagged endpoint.
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

# one row per `GraphMode` resolving the `nx` payload class `create_using` selects, so directedness
# AND multiplicity are bounded data rather than a `directed: bool`/`multi: bool` knob pair.
MODE_CREATE: Final[Mapping[GraphMode, GraphFamily]] = MappingProxyType({
    GraphMode.UNDIRECTED: nx.Graph,
    GraphMode.DIRECTED: nx.DiGraph,
    GraphMode.MULTI: nx.MultiGraph,
    GraphMode.MULTI_DIRECTED: nx.MultiDiGraph,
})

# the projection dispatch: one arm per MarkSpace rather than four projection functions threaded per
# kind, so EDGE_ROW and VERTEX reuse the spec's EdgeSource and only FACET owns the matrix build.
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
    AnalyticSpec(AnalyticOp.EIGENVECTOR, lambda g, p: _ranked(nx.eigenvector_centrality(g, max_iter=p.power_iter, backend=p.backend.value), p) if g.number_of_nodes() else AnalyticValue.Leaderboard(()), lambda _: True),
    AnalyticSpec(AnalyticOp.PAGERANK, lambda g, p: _ranked(nx.pagerank(g, max_iter=p.power_iter, backend=p.backend.value), p) if g.number_of_nodes() else AnalyticValue.Leaderboard(()), lambda _: True),
    AnalyticSpec(AnalyticOp.SPANNING_WEIGHT, lambda g, p: AnalyticValue.Scalar(float(nx.minimum_spanning_tree(g, backend=p.backend.value).number_of_edges())), lambda m: not m.directed),
    AnalyticSpec(AnalyticOp.CYCLES, lambda g, p: AnalyticValue.Scalar(float(sum(1 for _ in nx.simple_cycles(g, backend=p.backend.value)))), lambda _: True),
    AnalyticSpec(AnalyticOp.COMMUNITY, lambda g, p: AnalyticValue.Groups(tuple(tuple(sorted(int(n) for n in c)) for c in nx.community.louvain_communities(g, backend=p.backend.value))) if g.number_of_nodes() else AnalyticValue.Groups(()), lambda m: not m.directed),
)

# one row per FeatureKind owning the subject the case crosses and the empty-graph ledger/ceiling the
# graduation fold gates: every kind keys the same `empty_graph_fraction` so a no-node projection does
# not graduate. The kind already names its row, so a new kind is one CaseSpec, never a subject match.
CASE: Final[Mapping[FeatureKind, CaseSpec]] = MappingProxyType({
    kind: CaseSpec(subject, lambda c: {"empty_graph_fraction": 0.0 if c.nodes else 1.0}, _EMPTY_CEILING)
    for kind, subject in (
        (FeatureKind.SHARP_EDGE, "network-graph"),
        (FeatureKind.PLANAR, "network-graph"),
        (FeatureKind.CURVATURE, "network-graph"),
        (FeatureKind.BOUNDARY, "network-graph"),
    )
})


def _analyse(graph: nx.Graph, policy: FeaturePolicy, *, ops: frozenset[AnalyticOp]) -> Map[AnalyticOp, AnalyticValue]:
    # the policy `ops` set AND the row `mode_guard` both gate by data: a directed-only or
    # undirected-only algorithm and a deselected op are skipped without an inline graph-kind branch.
    selected = (spec for spec in ANALYTICS if spec.op in ops and spec.mode_guard(policy.mode))
    return Map.of_seq([(spec.op, spec.reducer(graph, policy)) for spec in selected])


def _project(mesh: trimesh.Trimesh, kind: FeatureKind, policy: FeaturePolicy) -> tuple[nx.Graph, Marks]:
    # the one detect/project fold: the spec's detector marks the feature element set and MARK_PROJECT
    # lifts it into the `nx` payload on the row's MarkSpace, so neither step branches per kind.
    spec = FEATURE_OPS[kind]
    marks = spec.detector(mesh, policy)
    return MARK_PROJECT[spec.mark_space](mesh, spec, marks, policy), marks


def _assemble(graph: nx.Graph, marks: Marks, kind: FeatureKind, policy: FeaturePolicy, values: Map[AnalyticOp, AnalyticValue]) -> FeatureResult:
    census = Census(
        kind=kind, mode=policy.mode, backend=policy.backend,
        marks=int(marks.size), nodes=graph.number_of_nodes(), edges=graph.number_of_edges(), values=values,
    )
    return FeatureResult(
        kind=kind, census=census, graduation_subject=CASE[kind].subject,
        node_link=msgspec.json.encode(nx.node_link_data(graph, edges="edges")),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


class Features(Struct, frozen=True):
    # the one ReceiptContributor producer: `run` discriminates a single FeatureRequest or a batch over
    # the FEATURE_OPS table; the `@receipted` aspect sits on the pure `_extract` (sync) and `_offload`
    # (async heavy/light split) bodies, and the `boundary`/`async_boundary` fence sits OUTSIDE in
    # `run`/`bridged` so a raise stays an `Error(BoundaryFault)` the aspect never falsely emits. The
    # capsule holds the conditioned mesh; the policy rides each request.
    mesh: trimesh.Trimesh

    @receipted(_REDACTION)
    def _extract(self, request: FeatureRequest) -> FeatureResult:
        # the pure sync detect/project/analyse body the `@receipted` aspect harvests on exit; the
        # `boundary` fence in `run` is the single exception-to-fault seam, so this threads no emit
        # and no try/except. The async path rides the sibling `_offload` body, not this one.
        graph, marks = _project(self.mesh, request.kind, request.policy)
        return _assemble(graph, marks, request.kind, request.policy, _analyse(graph, request.policy, ops=request.policy.ops))

    def run(self, request: FeatureRequest | Sequence[FeatureRequest]) -> RuntimeRail[FeatureResult] | RuntimeRail[Block[FeatureResult]]:
        # one entrypoint over the input axis: a single request fences `_extract` once so the aspect emits
        # and the result-is-contributor is the public surface; a batch folds a Block of the same fenced
        # rail through `traversed` ACCUMULATE so one fault stays addressable while every success emitted.
        match request:
            case Sequence() as batch:
                return traversed(Block.of_seq([boundary(f"features.{item.kind}", lambda i=item: self._extract(i)) for item in batch]), by=Disposition.ACCUMULATE)
            case FeatureRequest() as single:
                return boundary(f"features.{single.kind}", lambda: self._extract(single))
            case _ as unreachable:
                assert_never(unreachable)

    async def bridged(self, request: FeatureRequest) -> RuntimeRail[FeatureResult]:
        # the async fence mirror of `run`: it routes the `@receipted` `_offload` through the bounded
        # worker pool, so the aspect emits on `_offload`'s exit exactly as the sync arm emits on
        # `_extract`'s. `bridged` is NOT itself `@receipted` and returns the bare `RuntimeRail` —
        # a failed offload stays an `Error(BoundaryFault)` the caller reads, never a `.default_value`
        # collapse into a synthetic empty result the aspect would falsely emit as `admitted`.
        return await async_boundary(f"features.{request.kind}", lambda: self._offload(request))

    @receipted(_REDACTION)
    async def _offload(self, request: FeatureRequest) -> FeatureResult:
        # the async detect/project/analyse body the `@receipted` aspect harvests on exit through
        # `emit_async`; the `async_boundary` fence in `bridged` is the single exception-to-fault seam,
        # so a raise never reaches the aspect and no synthetic result is emitted. The heavy analytic
        # band — betweenness/pagerank/simple_cycles whose all-pairs and enumeration cores must not block
        # the loop — hoists to the `_ANALYTIC_LIMITER`-bounded worker pool, the cheap rows fold inline,
        # and the two maps merge through `Map.of_seq` over their items.
        graph, marks = _project(self.mesh, request.kind, request.policy)
        heavy, light = request.policy.ops & _HEAVY_OPS, request.policy.ops - _HEAVY_OPS
        offloaded = await anyio.to_thread.run_sync(lambda: _analyse(graph, request.policy, ops=heavy), limiter=_ANALYTIC_LIMITER)
        values = Map.of_seq([*_analyse(graph, request.policy, ops=light).items(), *offloaded.items()])
        return _assemble(graph, marks, request.kind, request.policy, values)
```

## [03]-[RESEARCH]

- [BOUNDARY_INCIDENCE]: the `_boundary_edges` kernel reads the catalogue-confirmed `edges_face` `(e, 2)` per-edge face accessor (`.api/trimesh.md` edge-topology row [08], confirmed cached property at the members line) directly aligned to `edges_unique`, `-1` filling an open side, and selects the rows whose live-face count `(edges_face >= 0).sum(axis=1)` is one — one `numpy.where` over the confirmed accessor, no triangle-edge expansion. `edges_unique` is the projection's `EdgeSource` so a count-1 mark indexes it exactly; both members are catalogue-confirmed cached properties keyed off the `vertices`/`faces` content hash, and `numpy.where`/`sum`/`asarray` are confirmed (`numpy.md` `where` row [04], `sum`, `asarray`). The prior `_face_edge_set` `numpy.unique(tri_edges, axis=0, return_counts=True)` over the sorted `faces` triangle-edge fancy-index is the deleted form: it re-derived the incidence the confirmed `edges_face` accessor already carries. The `facets_boundary` outline path (also a confirmed cached property) admits as a future facet-boundary `FeatureKind` row reading the per-facet boundary edges directly, never a replacement of the `edges_face` open-edge discriminant.
- [NETWORKX_ANALYTICS]: the `ANALYTICS` table folds eleven catalogue-confirmed `networkx` algorithm families — `connected_components` (row [06], undirected set generator), `weakly_connected_components` (row [10], directed set generator), `strongly_connected_components` (row [07], directed set generator), `betweenness_centrality` (row [17], node-score dict), `degree_centrality`/`closeness_centrality`/`eigenvector_centrality` (row [18], node-score dict), `pagerank` (row [16], node-score dict), `minimum_spanning_tree` (row [08], graph), `simple_cycles` (row [14], cycle generator), and `nx.community.louvain_communities` (row [21], `list[set]` partition) — plus the `node_link_data` node-link projection verb (row [13]). The `from_edgelist`/`from_numpy_array` conversion bridges (rows [07]/[03]), the `create_using` directedness/multiplicity axis over the `Graph`/`DiGraph`/`MultiGraph`/`MultiDiGraph` family (PUBLIC_TYPES [01]–[04]), and the `backend=`/`**backend_kwargs` dispatch axis on every `@nx._dispatchable` algorithm (IMPLEMENTATION_LAW BACKEND_DISPATCH) are catalogue-confirmed. The `networkx` catalogue lives at the branch `libs/python/data/.api/networkx.md` because `data` is the package owner of record; `graph/features` is the geometry-side consumer of the same admitted package, citing the one branch catalogue rather than minting a duplicate folder `.api`. The `GraphBackend` policy row threads `backend=` into each reducer per the catalogue's "receipt records the backend and never forks parallel call sites per backend" law; the page never mutates process-global `nx.config`, leaving dispatch policy to the application boundary the catalogue names.
- [COMMUNITY_ANALYTIC]: `nx.community.louvain_communities(G, weight='weight', resolution=1, threshold=1e-07, max_level=None, seed=None)` returning `list[set]` is catalogue-confirmed (`networkx.md` ENTRYPOINTS row [21]) and is the ARCHITECTURE-charter community-detection analytic, now a live `AnalyticOp.COMMUNITY` row rather than a deferred fence — its partition projects through the `AnalyticValue.Groups` case so the receipt carries the community count as a native scalar while the full partition rides the typed `Census.values` `Map`. The sibling partition verbs `greedy_modularity_communities` (row [22]) and `girvan_newman`/`modularity` (row [23]) admit as further `AnalyticOp` rows the same way; `louvain_communities` is the seeded default the policy `ops` set toggles.
- [MARK_SPACE_PAIRING]: each `FeatureSpec` carries a `MarkSpace` tag and an `EdgeSource` resolver, and the `MARK_PROJECT` table dispatches the projection on that tag — three arms over four kinds rather than four projection functions. `EDGE_ROW` (`_edge_row` lifts the spec's `EdgeSource` array through `rows[marks]`: `SHARP_EDGE` binds `_adjacency_rows` over `face_adjacency`, `BOUNDARY` binds `_unique_edges` over `edges_unique` so the count-1 `edges_face` mark indexes it exactly), `VERTEX` (`_vertex_edge` scatters a boolean vertex flag `flags[marks]` and keeps the `EdgeSource` rows where `flags[edges].any(axis=1)`, so `CURVATURE`'s vertex-index marks select the incident `edges_unique` rows rather than indexing edge rows by a vertex id), and `FACET` (`_facet_matrix` builds the facet-adjacency matrix from `face_adjacency`, `PLANAR` binding the `_no_edges` source it ignores). The pairing is a `FEATURE_OPS` row property, so a detector returning a different mark space binds its matching `MarkSpace`/`EdgeSource` in the same row and the cross-index defect (an edge projection consuming vertex marks as edge-row indices) cannot recur. All marks stay 1-D index arrays so the `Census.marks` count (`int(marks.size)`) is total across every kind.
- [NODE_LINK_EDGES_KEY]: `node_link_data` takes the `edges=` key argument selecting the edge-collection key in the emitted mapping; the explicit `edges="edges"` pin produces the stable forward-compatible node-link schema and the `links` default is the removed legacy spelling the `networkx.md` INTEGRATION law forbids pinning — `edges="edges"` is the settled 3.x default. `msgspec.json.encode` (`msgspec.md` ENTRYPOINTS [01]) serializes the resulting mapping to JSON bytes — the graduation payload is real JSON, never `str(dict)` Python `repr` — and the `networkx.md` INTEGRATION law confirms a `node_link_data` payload is the canonical persisted graph document keyed through the `msgspec`/JSON codec rail.
- [ANALYTIC_VALUE_COLLAPSE]: the prior page modelled analytics as a flat `tuple[AnalyticSpec, ...]` of lambdas returning `float | int | tuple[tuple[int, float], ...]`, reconstructed into a five-field `Analytics` struct through a `folded.get(op, 0)` per-field remap and an `isinstance`-guarded `rank` lambda — a drift surface where a new analytic forced a struct field plus a remap edit. The rebuild collapses the reducer return to the `AnalyticValue` `@tagged_union` (`scalar`/`leaderboard`/`groups`) — the IDENTICAL shape the `nonmanifold` sibling rides, owning its own dual projection (`as_scalar` the cardinality, `peak` the `numpy.asarray(...).max` head score) so neither page mints a parallel msgspec subclass family nor a module-level `_peak` fold beside the union — so `_analyse` folds to one `Map[AnalyticOp, AnalyticValue]` with no `isinstance` reconstruction, and `Census` is a thin projection over that map: `Census.facts` keys the projection on `_HEAD_OPS`, reading `peak()` off a centrality row (its top head score) and `as_scalar()` off a count/partition row (its value or cardinality), so the receipt's flat `dict[str, object]` carries the load-bearing native scalar per selected analytic while the full leaderboards and partitions ride the typed `values` `Map`. A new analytic is one `AnalyticOp` row plus one `ANALYTICS` reducer and, if its flat fact is the extremum rather than the count, one `_HEAD_OPS` membership; a new return shape is one `AnalyticValue` case plus one arm in each projection.
- [RECEIPT_CONTRACT]: `FeatureResult` conforms to the runtime `observability/receipts#RECEIPT` `ReceiptContributor` `Protocol` and `contribute` returns the `Iterable[Receipt]` the port streams (a one-element tuple), minting through the two-argument `Receipt.of(owner, evidence)` factory with the `(Phase, subject, facts)` evidence triple — never the four-positional `Receipt.of(phase, owner, subject, facts)` form, which is not the factory's signature. The facts ride as `dict[str, object]` carrying native `int`/`float` scalars, because the receipts owner's `Encoder(enc_hook=repr, order="deterministic")` renderer serializes native scalars and a `str()`-coerced `dict[str, str]` is the deleted form that owner rejects. `@receipted(_REDACTION)` on the pure `_extract` (sync) and `_offload` (async) bodies is the AOP aspect harvesting and emitting the contributor stream on exit — the sync arm through `Signals.emit`, the async arm through `Signals.emit_async` off the aspect's `iscoroutinefunction` dispatch so a high-volume bridged path offloads render-and-sink rather than blocking the loop — receipt egress a decorator rail, not an inline `Signals.emit`, the same `@receipted` seam the sibling `mesh/cad.md#CAD` `CadReceipt` rides. The fence sits OUTSIDE the aspect in `run`/`bridged`, so an offload raise stays an `Error(BoundaryFault)` on the returned rail rather than a `.default_value`-collapsed synthetic empty result the aspect would falsely emit. The keep-all `_REDACTION` (an empty-classification `Redaction`) matches the `compute/graduation/handoff#GRADUATION` graduation facts that carry no secret field.
