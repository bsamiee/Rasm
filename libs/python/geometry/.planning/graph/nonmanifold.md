# [PY_GEOMETRY_GRAPH_NONMANIFOLD]

Non-manifold topological modeling. `run` is one module-level entrypoint over the `TopologyOp` `@tagged_union(frozen=True)` discriminating the stateless `topologicpy` static-method namespace — non-manifold `Topology` construction (a `Cell`/`CellComplex` handle the `_CONSTRUCT` table lifts) from B-rep/OCCT/JSON/OBJ bytes, decomposition into the topological hierarchy, the non-manifold boolean kernel (`Union`/`Difference`/`Intersect`/`Slice`), cell-to-cell adjacency, `Dictionary`-attribute attachment, `BoundingBox`/`Centroid`/`Contains` analysis, and `Graph.ByTopology` dual-graph extraction folded through the `GRAPH_ANALYTIC` table for connectivity, centrality, spanning, and path analytics the C# `IfcSemanticModel` spatial projection does not perform. Each case carries its own payload and folds through one of the `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS`/`GRAPH_ANALYTIC` data tables rather than parallel arms; the dual-graph case graduates via the compute `HandoffAxis` `geometry` case carrying the canonical `topology-graph` `GeometrySubject`. This owner is gated against re-deriving the in-process BIM space-graph.

`topologicpy` is an opt-in Forge-lane companion under a dual gate, excluded from the default server build: it is `AGPL-3.0-or-later` (network copyleft — linking it into a hosted server triggers the AGPL §13 source-disclosure obligation over the whole deployment), AND it dual-gates `requires-python<'3.15'` (no cp315 wheel, so it cannot ride the intended cp315 core). Both gates must clear before any arm is reachable: the owner and fences stay authored, the runtime admitted only on the explicit AGPL-accepting Forge companion lane (`forge-companion-env`, cp312 floor inside the `<'3.15'` band), never linked into the shipped default server. The whole `nonmanifold` surface is a gated companion band, dark in the default build, and `topologicpy` is absent from the manifest until the opt-in row lands.

## [01]-[INDEX]

- [01]-[TOPOLOGY]: the `TopologyOp` `@tagged_union`, the `_CONSTRUCT`/`_BOOLEAN`/`_ANALYSIS` op tables, the woven `topologicpy` handle-chain rail, the `GRAPH_ANALYTIC` analytics fold collapsing every reducer return through the `AnalyticValue` `@tagged_union` with its dual `as_scalar`/`peak` projection methods, the `TopologyPolicy` value object, the typed `TopologyCensus`, the `@receipted` aspect, the polymorphic single-or-batch `run` entrypoint over one pure `_dispatch`, and the async `bridged` mirror offloading the heavy graph-analytics band through `_ANALYTIC_LIMITER`-bounded `anyio.to_thread.run_sync`.

## [02]-[TOPOLOGY]

- Owner: `run` is the one module-level entrypoint discriminating a single `TopologyOp` or a batch `Sequence[TopologyOp]`, mirroring the `graph/features.md` `Features.run` and `graph/algebra.md` `run` polymorphic single-or-batch siblings — there is no stateful capsule and no mutable receipt accumulator. `TopologyResult` is the sole `ReceiptContributor`, the typed receipt carrying the op tag, the result-handle summary strings, a typed `TopologyCensus` value object, and the graduation subject as the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`, never a bare `str`). `contribute` is the one emission, projecting `TopologyCensus` through `msgspec.structs.asdict` into one `Receipt.of("geometry.graph.nonmanifold", (phase, subject, facts))` `fact` row — `phase="emitted"` for a clean extraction and `phase="admitted"` for a degenerate result (an empty decomposition, a null boolean, or a zero-node dual graph) whose census flags the caveat rather than asserting a result — so the admitted and emitted rows ride the one contributor path rather than a discarded `Receipt.of` minted inside a decorator that never reaches the sink. The `facts` map is `dict[str, object]` carrying native handle/cell/edge/analytic counts the `observability/receipts#RECEIPT` `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce. Retry and telemetry are the `boundary(f"topology.{op.tag}")` fence subject (the single runtime seam), not a second hand-rolled aspect; the dual-graph case alone graduates through `TopologyResult.graduates`.
- Cases: `TopologyOp` cases `construct(source, kind)` (the `SourceKind`-keyed `_CONSTRUCT` table — `BREP` via `Topology.ByBREPString`, `OCCT` via `Topology.ByOCCTShape`, `JSON` via `Topology.ByJSONString`, `OBJ` via `Topology.ByOBJPath` — lifting bytes into a non-manifold `Topology` handle, never a `By*` family per source) · `decompose(source)` (`Topology.Decompose` into the Cell/Face/Edge/Vertex hierarchy projected through `Topology.Analyze`) · `adjacency(source)` (`Topology.AdjacentTopologies` cell-to-cell adjacency over `Topology.Cells`) · `boolean(a, b, kind)` (the `BooleanKind`-keyed `_BOOLEAN` table — `UNION`/`DIFFERENCE`/`INTERSECT`/`SLICE` over `Topology.Union`/`Difference`/`Intersect`/`Slice`, one polymorphic non-manifold boolean, never a per-op method) · `analysis(source, kind)` (the `AnalysisKind`-keyed `_ANALYSIS` table — `BBOX`/`CENTROID`/`CONTAINS` over `Topology.BoundingBox`/`Centroid`/`Contains`) · `attribute(source, keys, values)` (`Dictionary.ByKeysValues` built and attached via `Topology.AddDictionary`) · `dual_graph(source, policy)` (`Graph.ByTopology` extraction folded through the policy-selected `GRAPH_ANALYTIC` rows and serialized as JSON-bytes evidence via `msgspec.json.encode`) — matched by total `match`/`case` closed with `assert_never`, each consuming the prior static-call handle. The sub-kind of every parameterized case is a closed `StrEnum`, never a raw string literal inside the payload.
- Entry: `run(op)` discriminates a single `TopologyOp` or a `Sequence[TopologyOp]` on the input axis, returning `RuntimeRail[TopologyResult]` for a single op and `RuntimeRail[Block[TopologyResult]]` for a batch. The `@receipted(REDACTION)` aspect wraps the pure `_extract`, harvesting `contribute()` on exit through `Signals.emit`, and the `boundary(f"topology.{op.tag}", ...)` exception-to-fault fence wraps `_extract` from OUTSIDE — the single runtime seam, exactly the `graph/features.md` `extract` wiring where the fence sits outside the `@receipted` body rather than nesting inside it. A batch builds a `Block` of the same fenced rail in one comprehension (the `i=item` default binding the loop variable per closure) and folds them through `runtime.faults.traversed` with `Disposition.ACCUMULATE` so one fault stays addressable in the aggregate while every successful `TopologyResult` already emitted through the aspect — never a self-mutating capsule. `bridged` is the bare async fence mirror returning `RuntimeRail[TopologyResult]`: it routes the `@receipted` `_offload` body — whose blocking `_dispatch` (the `Graph.BetweennessCentrality`/`ClosenessCentrality`/`ShortestPath` all-pairs cores the `dual_graph` case folds, which must not block the loop) crosses to the loop through `_ANALYTIC_LIMITER`-bounded `anyio.to_thread.run_sync` — through `async_boundary` so the aspect emits on `_offload`'s exit through its `iscoroutinefunction`-dispatched `emit_async` arm exactly as the sync arm emits on `_extract`'s, holding at most four worker slots rather than draining the runtime-shared default 40-token pool. `bridged` is NOT itself `@receipted` and never `.default_value`-collapses an offload fault into a synthetic degenerate `TopologyResult` the aspect would falsely emit as `admitted`; a failed offload stays an `Error(BoundaryFault)` on the returned rail exactly as the `graph/features.md`/`graph/algebra.md` siblings hold, the `_ANALYTIC_LIMITER`-bound fence-outside-the-aspect wiring (bounded as the `graph/algebra.md` sibling's `_SOLVER_LIMITER` bounds its proxy band), never a parallel async surface beside the sync `run`. Source bytes lift through the `_CONSTRUCT` table once per op; `Cells`/`Decompose` enumerate the sub-topologies, `AdjacentTopologies` threads cell adjacency, and `Graph.ByTopology` projects the dual graph for graduation.
- Auto: the `topologicpy` surface is a stateless static-method namespace operating on `topologic_core` handles — each `Topology.ByX(...)` returns a handle the next static call consumes, so the dispatch threads handles through the chain rather than mutating an object. The construction case folds `_CONSTRUCT` by `SourceKind` so a new intake format is one `SourceKind` row plus one table entry; the boolean case folds `_BOOLEAN` by `BooleanKind`; the analysis case folds `_ANALYSIS` by `AnalysisKind` — never a dispatch branch. The dual-graph case threads the one `Graph` handle through every policy-selected `GRAPH_ANALYTIC` row, each reducer minting its return through the `AnalyticValue.Scalar`/`Leaderboard`/`Groups` factories over the one `@tagged_union` (a `scalar` `float`, a `leaderboard` `tuple[tuple[int, float], ...]`, or a `groups` `tuple[tuple[int, ...], ...]` partition) so `_graph_analytics` folds to a `Map[GraphAnalytic, AnalyticValue]` with no `isinstance` reconstruction — the `features.md` `AnalyticValue` `scalar`/`leaderboard`/`groups` shape and `case()`/`tag()` form, extended with the second `peak` projection the topology census needs, so the union owns both total-`match` methods (`as_scalar` the flat-facts cardinality, `peak` the head-magnitude the census reads) rather than a parallel msgspec subclass family or a module-level `_peak` fold beside the union. `_graph_census` reads `peak()` off the map through the `try_find().map(...).default_value(0.0)` Option fold so `betweenness_max` rides the max centrality and the leaderboard cardinality stays available to `as_scalar`, both off the one union and never two inlined `max`/`len` calls. `_census` is the matching topology fold reading `Cells`/`Faces`/`Vertices` once over a typed handle, and `_decompose_census` the parallel fold over the decomposed-parts tuple, so neither probes shape through a runtime `isinstance(topo, (list, tuple))`. The cross-cutting emit is the `@receipted` aspect alone — each arm writes only its handle-chain body and returns a `TopologyResult`.
- Receipt: each `run` produces a `TopologyResult` that is the sole `ReceiptContributor`; `contribute` emits one phase-keyed `Receipt.of` `fact` row — `phase="emitted"` for a clean extraction and `phase="admitted"` for a degenerate result (zero cells decomposed, a null boolean handle, a zero-node dual graph) whose census flags the caveat rather than asserting a result — the facts being the `TopologyCensus` value object (`structs.asdict`-projected) carrying the op tag, the handle count, the cell/face/edge census, and the dual-graph analytic scalars where the case produces them. The admitted and emitted rows ride this one contributor, never a discarded `Receipt.of` minted inside a decorator. `dual_graph` alone graduates a geometry subject (`topology-graph`) through `TopologyResult.graduates(evidence_key)`, routing the `msgspec.json.encode` byte payload as the evidence the compute `GraduationReceipt.graduates` admission fold keys against `_GRAPH_CEILING` — the analytic census is the evidence that fold reads, never a re-measured value; the non-graph ops emit the receipt only, since they produce intermediate topology the dual-graph case graduates downstream.
- Packages: `topologicpy` (`Topology.ByBREPString`/`ByOCCTShape`/`ByJSONString`/`ByOBJPath`/`Cells`/`Faces`/`Vertices`/`Decompose`/`AdjacentTopologies`/`Analyze`/`Union`/`Difference`/`Intersect`/`Slice`/`BoundingBox`/`Centroid`/`Contains`/`AddDictionary`, `Graph.ByTopology`/`Vertices`/`Edges`/`Connectivity`/`ShortestPath`/`BetweennessCentrality`/`ClosenessCentrality`/`DegreeCentrality`/`Tree`/`JSONData`, `Dictionary.ByKeysValues`/`ValueAtKey`), runtime (`RuntimeRail`/`boundary`/`async_boundary`/`traversed`/`Disposition`, `Receipt`/`ReceiptContributor`/`Redaction`/`receipted`, `ContentKey`), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`), `expression` (`tagged_union`/`tag`/`case`, `Block`/`Map` with `Map.of_seq`/`Map.try_find` and the `Option.map`/`default_value` fold), `anyio` (`to_thread.run_sync`/`CapacityLimiter` offloading the heavy all-pairs analytics band off the event loop under a dedicated bounded worker pool), `numpy` (`asarray`/`max(initial=...)` the `AnalyticValue.peak` head-score reduction), `msgspec` (`Struct`/`structs.asdict`/`json.encode`).
- Growth: a new topological operation is one `TopologyOp` case plus one `match` arm threading the handle chain; a new intake format is one `SourceKind` row plus one `_CONSTRUCT` entry; a new non-manifold boolean is one `BooleanKind` row plus one `_BOOLEAN` entry; a new analysis verb is one `AnalysisKind` row plus one `_ANALYSIS` entry; a new graph analytic is one `GraphAnalytic` row plus one `GRAPH_ANALYTIC` entry binding its reducer, with one `AnalyticValue` case plus its `as_scalar`/`peak` arms only if its return shape is new; a stricter centrality cap, a different analytic selection, or a boundary-extrema strategy is a `TopologyPolicy` field the caller passes; IFC-bytes ingestion is one `SourceKind.IFC` row admitting the `ifcopenshell.file`-to-`topologicpy` bridge alongside the B-rep path (never a new owner, never a temp-file detour), gated on the constructor-arity confirmation in `nonmanifold#3-IFC_BYTES_CONSTRUCTOR`; aperture-into-host binding is one `TopologyOp.aperture(aperture, context)` case threading `Aperture.ByTopologyContext` (a face-into-host binding distinct from the byte-stream `_CONSTRUCT` path, never a `_CONSTRUCT` row), gated on the `Aperture.ByTopologyContext(aperture, context)` arity confirmation in `nonmanifold#3-TOPOLOGIC_HANDLES`; zero new surface.
- Boundary: `topologicpy` is admitted ONLY for non-manifold cell/aperture analysis the C# `IfcSemanticModel` does not extract — the BIM space-graph (spatial hierarchy/adjacency) is projected in-process and is never re-derived here; numerical and form-finding geometry is the `graph/algebra` sibling over `compas`, not this owner; mesh-feature projection onto the network graph is the `graph/features` sibling over `trimesh`/`networkx`; raw mesh-file exchange stays at the data `MeshPayload` seam — `run` returns `topologicpy` handle/JSON-bytes summaries across the wire and never writes a topology file. The `topology-graph` subject crosses HERE; the `network-graph` subject (mesh-feature and compas adjacency producers) stays in the `features`/`algebra` siblings, distinct concerns on distinct subjects. The deleted forms: a stateful `TopologyAlgebra(ReceiptContributor)` capsule mutating a `Block[Receipt]` accumulator where a module-level `run` over a pure `_dispatch` and the result-is-contributor pattern own the surface; a flat `StrEnum`-keyed `match` where the `@tagged_union` case carries its own payload; a `By*` accessor/boolean family per topology kind over the polymorphic `Topology` row; a `GRAPH_ANALYTIC` returning `object` reduced through ad-hoc `float(max(...))`/`len(...)` where the `AnalyticValue` `@tagged_union` collapses every reducer return; a parallel `AnalyticValue(Struct, tag_field="kind")` family of `Scalar`/`Leaderboard`/`Groups` subclasses plus a module-level `_peak` fold beside it where the `features.md` `@tagged_union` + `case()`/`tag()` form owns the bounded variant set and the union carries its own `as_scalar`/`peak` projection methods — the corpus models every bounded variant as the one `expression` `@tagged_union`, never a sibling msgspec discriminated-struct hierarchy for the SAME concept the sibling page already shapes; a two-analytic graph fold over the 179-method `Graph` facade where the policy-selected `GRAPH_ANALYTIC` table folds connectivity, centrality, spanning, and path in one pass; a sync-only heavy-analytics surface with no async mirror where `bridged` offloads the `Graph.BetweennessCentrality`/`ClosenessCentrality`/`ShortestPath` all-pairs band through `anyio.to_thread.run_sync` lifted by `async_boundary` exactly as the `features.md`/`algebra.md` siblings do, never blocking the event loop and never a parallel async rail beside the sync `run`; a `bridged` that is itself `@receipted` and `.default_value`-collapses an offload fault into a synthetic degenerate `TopologyResult` the aspect falsely emits as `admitted` where `bridged` returns the bare `RuntimeRail[TopologyResult]`, the `@receipted` aspect lives on the inner `_offload` body, and a raise stays an `Error(BoundaryFault)` the caller reads (the fault-preserving rail-return the `features.md`/`algebra.md` siblings hold, never a synthetic-on-fault emit the receipts/graduation contract names an anti-pattern); an unbounded `anyio.to_thread.run_sync` analytics offload draining the default 40-token pool where `_ANALYTIC_LIMITER` bounds the heavy band exactly as the `algebra.md` sibling's `_SOLVER_LIMITER` does; a `_census` probing shape through a runtime `isinstance(topo, (list, tuple))` where typed `_census`/`_decompose_census` folds dispatch on the arm; a `ByOCCTShape(ByBREPString(...))` double-call where the OCCT row reads the OCCT-handle constructor directly; a `TopologyResult` that is not a `ReceiptContributor` and mints no graduation subject; a 4-positional `Receipt.of(phase, owner, subject, facts)` where the runtime contract is the 2-arg `Receipt.of(owner, (phase, subject, facts))`; a `str()`-coerced `dict[str, str]` facts map where the `EventDict` carries native `dict[str, object]`; an inline `Receipt.of` threaded through the dispatch body where `@receipted` owns the emit and the census folds own the facts; a second batch method where the one `traversed` fold drains a `Sequence`; a temp-file round-trip where the byte-stream constructor owns the intake.

```python signature
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final, Literal, assert_never

# the gated-companion `topologicpy` facade is imported module-top like the `graph/algebra.md` `compas`
# sibling: the page is a dark companion band never reached in the default cp315 build, so the import
# is a no-op there and a live facade only on the opt-in Forge lane.
import anyio
import msgspec
import numpy as np
from topologicpy.Dictionary import Dictionary
from topologicpy.Graph import Graph
from topologicpy.Topology import Topology
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import Disposition, RuntimeRail, async_boundary, boundary, traversed
from rasm.runtime.receipts import Receipt, ReceiptContributor, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------

type OpTag = Literal["construct", "decompose", "adjacency", "boolean", "analysis", "attribute", "dual_graph"]
type Phase = Literal["admitted", "emitted"]
# every static call returns a bare `topologic_core` handle (`Topology`/`Graph`/`Dictionary` C++
# object), opaque with no Python stub, so handles thread the chain typed only as `object`.
type Handle = object
type Reducer = Callable[[Handle, "TopologyPolicy"], "AnalyticValue"]
type Leaders = tuple[tuple[int, float], ...]
type Partition = tuple[tuple[int, ...], ...]


class SourceKind(StrEnum):
    BREP = "brep"
    OCCT = "occt"
    JSON = "json"
    OBJ = "obj"


class BooleanKind(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECT = "intersect"
    SLICE = "slice"


class AnalysisKind(StrEnum):
    BBOX = "bounding-box"
    CENTROID = "centroid"
    CONTAINS = "contains"


class GraphAnalytic(StrEnum):
    CONNECTIVITY = "connectivity"
    BETWEENNESS = "betweenness-centrality"
    CLOSENESS = "closeness-centrality"
    DEGREE = "degree-centrality"
    SPANNING = "spanning-tree"
    SHORTEST_PATH = "shortest-path"


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUBJECT: Final[GeometrySubject] = "topology-graph"
REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # topology facts carry no secret field
# the dual-graph residual the compute admission fold gates: a degenerate graph (zero nodes) breaches.
_GRAPH_CEILING: Final[Mapping[str, float]] = MappingProxyType({"empty_node_fraction": 0.0})
# this owner's OWN dedicated thread-pool bound for the blocking `Graph` all-pairs analytics `bridged`
# offloads through `to_thread.run_sync`: a betweenness/closeness/shortest-path solve holds a worker slot for
# the whole pass, so a 4-token limiter keeps concurrent `bridged` calls from draining anyio's default
# 40-token pool that the rest of the runtime shares. The per-owner 4-slot PATTERN the `graph/algebra.md`
# `_SOLVER_LIMITER` and `graph/features.md` `_ANALYTIC_LIMITER` siblings each mint INDEPENDENTLY, never a
# shared runtime instance: the three graph owners run on distinct companion bands, so each caps its own fan.
_ANALYTIC_LIMITER: Final[anyio.CapacityLimiter] = anyio.CapacityLimiter(4)

# --- [MODELS] ---------------------------------------------------------------------------


class TopologyPolicy(Struct, frozen=True, gc=False):
    # the dual-graph analytic selection and reduction bounds; every threshold and toggle is a field,
    # never a positional float or a per-call-site fork. Mirrors the `graph/features.md` `FeaturePolicy`.
    analytics: frozenset[GraphAnalytic] = frozenset(GraphAnalytic)
    centrality_top: int = 16  # leaderboard cap the betweenness/closeness/degree rows truncate to


@tagged_union(frozen=True)
class AnalyticValue:
    # one `@tagged_union` carrier collapsing every `GRAPH_ANALYTIC` reducer return so `_graph_analytics`
    # folds to a `Map[GraphAnalytic, AnalyticValue]` with no `isinstance` reconstruction — the
    # `graph/features.md` `AnalyticValue` shape (`scalar`/`leaderboard`/`groups` + static factories +
    # `as_scalar`) plus the `peak` head-magnitude projection the topology census reads, both on the
    # union, not a parallel msgspec subclass family the corpus does not use.
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
        # the flat-facts cardinality projection mirroring `features.md`: a scalar carries its value, a
        # leaderboard / partition its member count, so the receipt facts read one `float` off every kind.
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
        # the head-magnitude projection the topology census reads where the extremum is the score, not
        # the cardinality: a scalar IS its peak, a leaderboard its `numpy.asarray(...).max` head score,
        # a `Groups` its member count — the one fold that lets `betweenness_max` ride the max centrality
        # while `as_scalar` keeps the leaderboard cardinality, both off the one union, no second `_peak`.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return v
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return float(np.asarray([score for _, score in rows]).max(initial=0.0))
            case AnalyticValue(tag="groups", groups=partition):
                return float(len(partition))
            case _ as unreachable:
                assert_never(unreachable)


class TopologyCensus(Struct, frozen=True, gc=False):
    op: OpTag
    handles: int
    cells: int = 0
    faces: int = 0
    edges: int = 0
    vertices: int = 0
    nodes: int = 0  # dual-graph node count; 0 for non-graph ops
    components: int = 0
    betweenness_max: float = 0.0
    path_length: int = 0


class TopologyResult(Struct, ReceiptContributor, frozen=True):
    op: OpTag
    handles: tuple[str, ...]  # `Topology.Analyze` summaries; the dual-graph arm a compact node/edge descriptor
    census: TopologyCensus
    evidence: bytes = b""  # `msgspec.json.encode` dual-graph payload; () for non-graph ops
    graduation_subject: GeometrySubject = _SUBJECT
    degenerate: bool = False

    def contribute(self) -> tuple[Receipt, ...]:
        # runtime `Receipt.of(owner, (phase, subject, facts))` two-arg contract; native count/scalar
        # facts ride the `EventDict` `dict[str, object]` slots the `enc_hook=repr` renderer serializes.
        phase: Phase = "admitted" if self.degenerate else "emitted"
        facts: dict[str, object] = {**structs.asdict(self.census), "handle_count": len(self.handles)}
        return (Receipt.of("geometry.graph.nonmanifold", (phase, self.graduation_subject, facts)),)

    def graduates(self, evidence_key: ContentKey) -> "RuntimeRail[GraduationReceipt]":
        # only the dual-graph case graduates: the `topology-graph` `GeometrySubject` crosses the one
        # geometry `HandoffAxis` case, the measured empty-node fraction gated against `_GRAPH_CEILING`.
        empty = 0.0 if self.census.nodes else 1.0
        return GraduationReceipt.graduates(
            "geometry.graph.nonmanifold",
            HandoffAxis(geometry=self.graduation_subject),
            evidence_key,
            measured={"empty_node_fraction": empty},
            ceiling=dict(_GRAPH_CEILING),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


@tagged_union(frozen=True)
class TopologyOp:
    tag: OpTag = tag()
    construct: tuple[bytes, SourceKind] = case()
    decompose: bytes = case()
    adjacency: bytes = case()
    boolean: tuple[bytes, bytes, BooleanKind] = case()
    analysis: tuple[bytes, AnalysisKind] = case()
    attribute: tuple[bytes, tuple[str, ...], tuple[str, ...]] = case()
    dual_graph: tuple[bytes, TopologyPolicy] = case()

    @staticmethod
    def Construct(source: bytes, kind: SourceKind = SourceKind.BREP) -> "TopologyOp":
        return TopologyOp(construct=(source, kind))

    @staticmethod
    def Decompose(source: bytes) -> "TopologyOp":
        return TopologyOp(decompose=source)

    @staticmethod
    def Adjacency(source: bytes) -> "TopologyOp":
        return TopologyOp(adjacency=source)

    @staticmethod
    def Boolean(a: bytes, b: bytes, kind: BooleanKind) -> "TopologyOp":
        return TopologyOp(boolean=(a, b, kind))

    @staticmethod
    def Analysis(source: bytes, kind: AnalysisKind) -> "TopologyOp":
        return TopologyOp(analysis=(source, kind))

    @staticmethod
    def Attribute(source: bytes, keys: tuple[str, ...], values: tuple[str, ...]) -> "TopologyOp":
        return TopologyOp(attribute=(source, keys, values))

    @staticmethod
    def DualGraph(source: bytes, policy: TopologyPolicy = TopologyPolicy()) -> "TopologyOp":
        return TopologyOp(dual_graph=(source, policy))


def _lift(source: bytes, kind: SourceKind) -> Handle:
    return _CONSTRUCT[kind](source)


def _leaders(scores: Sequence[float], cap: int) -> AnalyticValue:
    # the top-`cap` (index, score) board the union's `peak()` re-reads through `numpy.asarray(...).max`,
    # so the receipt scalar reads the same reduction the leaderboard ranks; no inverse-sort allocation
    # beyond the cap slice. The static `Leaderboard` factory mints the case, mirroring `features.md`.
    ranked = sorted(enumerate(scores), key=lambda pair: pair[1], reverse=True)[:cap]
    return AnalyticValue.Leaderboard(tuple((node, float(score)) for node, score in ranked))


def _path(graph: Handle) -> AnalyticValue:
    # the hop count of the geodesic between the extreme vertices, read off the one vertex list rather
    # than re-enumerating `Graph.Vertices(graph)` per endpoint. A disconnected graph has no such path,
    # so `Graph.ShortestPath` returning a null/empty handle folds to `Scalar(0.0)` rather than crashing
    # `Graph.Vertices(None)` — the row stays total over the disconnected case instead of converting a
    # legitimate no-path graph into a `boundary` fault the fence would otherwise raise.
    verts = Graph.Vertices(graph)
    span = Graph.ShortestPath(graph, verts[0], verts[-1]) if len(verts) > 1 else None
    return AnalyticValue.Scalar(float(len(Graph.Vertices(span))) if span is not None else 0.0)


_CONSTRUCT: Final[Mapping[SourceKind, Callable[[bytes], Handle]]] = MappingProxyType({
    SourceKind.BREP: lambda b: Topology.ByBREPString(b.decode()),
    SourceKind.OCCT: lambda b: Topology.ByOCCTShape(b),  # OCCT-handle constructor reads the shape payload directly
    SourceKind.JSON: lambda b: Topology.ByJSONString(b.decode()),
    SourceKind.OBJ: lambda b: Topology.ByOBJPath(b.decode()),
})

_BOOLEAN: Final[Mapping[BooleanKind, Callable[[Handle, Handle], Handle]]] = MappingProxyType({
    BooleanKind.UNION: Topology.Union,
    BooleanKind.DIFFERENCE: Topology.Difference,
    BooleanKind.INTERSECT: Topology.Intersect,
    BooleanKind.SLICE: Topology.Slice,
})

_ANALYSIS: Final[Mapping[AnalysisKind, Callable[[Handle], Handle]]] = MappingProxyType({
    AnalysisKind.BBOX: Topology.BoundingBox,
    AnalysisKind.CENTROID: Topology.Centroid,
    AnalysisKind.CONTAINS: lambda t: Topology.Contains(t, Topology.Centroid(t)),
})

# every reducer collapses its return to one `AnalyticValue` case: connectivity to a `Scalar` component
# count, the centralities to a `Leaderboard` truncated at `policy.centrality_top`, the spanning tree to
# `Groups` of its edge endpoints, the path to a `Scalar` hop count. A new analytic is one row plus one
# `AnalyticValue` projection arm only when its return shape is new.
GRAPH_ANALYTIC: Final[Mapping[GraphAnalytic, Reducer]] = MappingProxyType({
    GraphAnalytic.CONNECTIVITY: lambda g, _: AnalyticValue.Scalar(float(Graph.Connectivity(g))),
    GraphAnalytic.BETWEENNESS: lambda g, p: _leaders(Graph.BetweennessCentrality(g), p.centrality_top),
    GraphAnalytic.CLOSENESS: lambda g, p: _leaders(Graph.ClosenessCentrality(g), p.centrality_top),
    GraphAnalytic.DEGREE: lambda g, p: _leaders(Graph.DegreeCentrality(g), p.centrality_top),
    GraphAnalytic.SPANNING: lambda g, _: AnalyticValue.Groups(tuple(tuple(Graph.Vertices(e)) for e in Graph.Edges(Graph.Tree(g)))),
    GraphAnalytic.SHORTEST_PATH: lambda g, _: _path(g),
})


def _dispatch(op: TopologyOp) -> TopologyResult:
    match op:
        case TopologyOp(tag="construct", construct=(source, kind)):
            topo = _lift(source, kind)
            return _result("construct", (Topology.Analyze(topo),), _census("construct", topo, handles=1))
        case TopologyOp(tag="decompose", decompose=source):
            parts = tuple(Topology.Decompose(_lift(source, SourceKind.BREP)))
            return _result("decompose", tuple(Topology.Analyze(p) for p in parts), _decompose_census("decompose", parts))
        case TopologyOp(tag="adjacency", adjacency=source):
            topo = _lift(source, SourceKind.BREP)
            adj = tuple(a for c in Topology.Cells(topo) for a in Topology.AdjacentTopologies(c, topo))
            return _result("adjacency", tuple(Topology.Analyze(a) for a in adj), _census("adjacency", topo, handles=len(adj)))
        case TopologyOp(tag="boolean", boolean=(a, b, kind)):
            combined = _BOOLEAN[kind](_lift(a, SourceKind.BREP), _lift(b, SourceKind.BREP))
            handles = (Topology.Analyze(combined),) if combined is not None else ()
            return _result("boolean", handles, _census("boolean", combined, handles=len(handles)), degenerate=combined is None)
        case TopologyOp(tag="analysis", analysis=(source, kind)):
            value = _ANALYSIS[kind](_lift(source, SourceKind.BREP))
            return _result("analysis", (Topology.Analyze(value),), _census("analysis", value, handles=1))
        case TopologyOp(tag="attribute", attribute=(source, keys, values)):
            topo = Topology.AddDictionary(_lift(source, SourceKind.BREP), Dictionary.ByKeysValues(list(keys), list(values)))
            return _result("attribute", (Topology.Analyze(topo),), _census("attribute", topo, handles=1))
        case TopologyOp(tag="dual_graph", dual_graph=(source, policy)):
            graph = Graph.ByTopology(_lift(source, SourceKind.BREP))
            census = _graph_census(graph, _graph_analytics(graph, policy))
            # the wire summary is the compact census-keyed descriptor, NOT the full graph re-decoded: the
            # whole serialized document rides `evidence` once, the `handles` slot carries the same
            # node/edge/component summary `Topology.Analyze` gives the topology arms rather than a multi-KB
            # JSON string duplicated across both fields.
            evidence = msgspec.json.encode(Graph.JSONData(graph))
            summary = f"graph nodes={census.nodes} components={census.components} path={census.path_length}"
            return _result("dual_graph", (summary,), census, evidence=evidence, degenerate=census.nodes == 0)
        case _ as unreachable:
            assert_never(unreachable)


# --- [COMPOSITION] ----------------------------------------------------------------------


def _result(op: OpTag, handles: tuple[str, ...], census: TopologyCensus, *, evidence: bytes = b"", degenerate: bool = False) -> TopologyResult:
    return TopologyResult(op=op, handles=handles, census=census, evidence=evidence, degenerate=degenerate or not handles)


def _census(op: OpTag, topo: Handle, *, handles: int) -> TopologyCensus:
    # `Cells`/`Faces`/`Vertices` enumerate the constituent handles of ONE topology; a null boolean
    # handle yields the zero census. No runtime shape probe — `_decompose_census` owns the tuple arm.
    if topo is None:
        return TopologyCensus(op=op, handles=handles)
    return TopologyCensus(
        op=op, handles=handles,
        cells=len(Topology.Cells(topo)), faces=len(Topology.Faces(topo)), vertices=len(Topology.Vertices(topo)),
    )


def _decompose_census(op: OpTag, parts: tuple[Handle, ...]) -> TopologyCensus:
    # the decomposed-parts arm measures the lead handle and carries the part count, parallel to `_census`
    # over a single handle so neither folds an `isinstance(topo, (list, tuple))` runtime probe.
    return _census(op, parts[0], handles=len(parts)) if parts else TopologyCensus(op=op, handles=0)


def _graph_analytics(graph: Handle, policy: TopologyPolicy) -> Map[GraphAnalytic, AnalyticValue]:
    # the one fold over the policy-selected `GRAPH_ANALYTIC` rows, each reducer collapsing its return
    # through `AnalyticValue`; a zero/one-node graph skips the path/centrality rows by data, not a branch.
    nodes = len(Graph.Vertices(graph))
    selected = (a for a in GraphAnalytic if a in policy.analytics and (nodes > 1 or a is GraphAnalytic.CONNECTIVITY))
    return Map.of_seq((a, GRAPH_ANALYTIC[a](graph, policy)) for a in selected) if nodes else Map.empty()


def _graph_census(graph: Handle, analytics: Map[GraphAnalytic, AnalyticValue]) -> TopologyCensus:
    # the dual-graph census reads the folded `AnalyticValue` map through the union's own `peak()` method
    # rather than re-measuring: connectivity yields the component count, betweenness the max centrality,
    # path the hops. The `try_find().map(peak).default_value(0.0)` Option fold is the `features.md`
    # `Census.scalar` shape — an absent row folds to 0.0 with no `None` arm and no module-level `_peak`.
    def peak(a: GraphAnalytic) -> float:
        return analytics.try_find(a).map(lambda v: v.peak()).default_value(0.0)

    return TopologyCensus(
        op="dual_graph", handles=1, nodes=len(Graph.Vertices(graph)),
        components=int(peak(GraphAnalytic.CONNECTIVITY)),
        betweenness_max=peak(GraphAnalytic.BETWEENNESS),
        path_length=int(peak(GraphAnalytic.SHORTEST_PATH)),
    )


@receipted(REDACTION)
def _extract(op: TopologyOp) -> TopologyResult:
    # the pure dispatch the `@receipted` aspect wraps: it returns the `TopologyResult` contributor the
    # aspect harvests through `Signals.emit` on exit, threading no emit call. The `boundary` fence lives
    # OUTSIDE in `run`, so the single exception-to-fault seam wraps `_extract` rather than nesting inside.
    return _dispatch(op)


def run(op: TopologyOp | Sequence[TopologyOp]) -> RuntimeRail[TopologyResult] | RuntimeRail[Block[TopologyResult]]:
    # one entrypoint over the input axis: a single op fences `_extract` once, a batch folds a `Block` of
    # the same fenced rail through `traversed` ACCUMULATE so one fault stays addressable in the aggregate
    # while every successful result already emitted through the aspect. The default `i=item` binds the
    # loop variable per closure so the comprehension does not capture the last `item` by reference.
    match op:
        case Sequence() as batch:
            return traversed(Block.of_seq([boundary(f"topology.{item.tag}", lambda i=item: _extract(i)) for item in batch]), by=Disposition.ACCUMULATE)
        case TopologyOp() as single:
            return boundary(f"topology.{single.tag}", lambda: _extract(single))
        case _ as unreachable:
            assert_never(unreachable)


@receipted(REDACTION)
async def _offload(op: TopologyOp) -> TopologyResult:
    # the pure async dispatch body the `@receipted` aspect harvests on exit through `emit_async` (off the
    # aspect's `iscoroutinefunction` dispatch): the `Graph.BetweennessCentrality`/`ClosenessCentrality`/
    # `ShortestPath` all-pairs cores the `dual_graph` case folds are CPU-blocking, so the blocking `_dispatch`
    # crosses to the loop through `_ANALYTIC_LIMITER`-bounded `anyio.to_thread.run_sync` (the heavy topologicpy
    # band holds at most 4 worker slots rather than draining the runtime-shared default 40-token pool, the
    # `_ANALYTIC_LIMITER` bound mirroring the `graph/algebra.md` sibling's `_SOLVER_LIMITER`). The
    # `async_boundary` fence lives OUTSIDE in `bridged`, so a worker raise never reaches the aspect and no
    # synthetic result is emitted — the same fence-outside-the-aspect wiring `_extract` carries on the sync arm.
    return await anyio.to_thread.run_sync(lambda: _dispatch(op), limiter=_ANALYTIC_LIMITER)


async def bridged(op: TopologyOp) -> RuntimeRail[TopologyResult]:
    # the bare async fence mirror of `run` returning `RuntimeRail[TopologyResult]`, exactly the
    # `graph/features.md`/`graph/algebra.md` sibling wiring: it routes the `@receipted` `_offload` body
    # through `async_boundary` so the aspect emits on `_offload`'s exit through `emit_async` exactly as the
    # sync arm emits on `_extract`'s. `bridged` is NOT itself `@receipted` and never `.default_value`-collapses
    # an offload fault into a synthetic degenerate `TopologyResult` the aspect would falsely emit as
    # `admitted` — a failed offload stays an `Error(BoundaryFault)` on the returned rail the caller reads,
    # the fault-preserving rail-return the receipts/graduation contract requires (a synthetic-on-fault
    # emit is the named anti-pattern), never a parallel async surface beside the sync `run`.
    return await async_boundary(f"topology.{op.tag}", lambda: _offload(op))
```

`Topology.Analyze(handle)` is the one summary projection threaded across every topology arm while the `dual_graph` arm carries a compact census-keyed descriptor string (the full graph document riding `evidence` once, never re-decoded into `handles`), `_census`/`_decompose_census` fold the constituent-handle counts into the one `TopologyCensus`, and `_graph_analytics`/`_graph_census` fold the policy-selected `GRAPH_ANALYTIC` reducers through `AnalyticValue` so the dual-graph receipt carries connectivity, centrality, spanning, and path in one pass rather than two inlined calls.

## [03]-[RESEARCH]

- [TOPOLOGIC_HANDLES]: the `Topology.Decompose` return collection, the `Topology.Faces`/`Topology.Vertices` sub-topology accessors parallel to `Cells`, the `Topology.AdjacentTopologies(item, host)` arity, the `Topology.Analyze` handle-summary return shape, the `Topology.BoundingBox`/`Centroid`/`Contains` analysis arities, the `Topology.Union`/`Difference`/`Intersect`/`Slice(a, b)` boolean-pair arities, the `Topology.AddDictionary(topology, dict)` attach arity, the `Dictionary.ByKeysValues(keys, values)` constructor, the `Topology.ByOCCTShape(shape)` payload form (whether the byte stream is the OCCT shape the constructor reads directly or requires an `OCC.Core`-decoded shape handle), and the `Aperture.ByTopologyContext(aperture, context)` face-into-host binding confirm against the folder `.api/topologicpy.md` entrypoint tables on the companion interpreter; `Topology.ByBREPString` (entrypoints [02]), `Topology.ByOCCTShape` ([06]), `Topology.ByJSONString` ([07]), `Topology.ByOBJPath` ([03]), `Topology.Cells` (analysis [01]), `Topology.Faces` ([02]), `Topology.AdjacentTopologies` ([04]), the four boolean ops ([05]-[08]), `Topology.BoundingBox`/`Centroid`/`Contains` ([09]-[11]), `Topology.AddDictionary` ([12]), `Graph.ByTopology`/`ShortestPath`/`BetweennessCentrality` (constructors [09]-[11]), and `Dictionary.ByKeysValues`/`ValueAtKey` ([12]-[13]) are catalogue-confirmed.
- [GRAPH_ANALYTIC_SURFACE]: the `Graph` facade carries 179 methods (`.api/topologicpy.md` PUBLIC_TYPES analysis [01]); the `GRAPH_ANALYTIC` table folds the catalogue-confirmed `BetweennessCentrality`/`ShortestPath` rows plus the gated `Connectivity`/`ClosenessCentrality`/`DegreeCentrality`/`Tree`/`Edges`/`Vertices`/`JSONData` rows the companion-interpreter assay confirms before the fence is final — these are NOT yet on a cited table line and so are companion-interpreter-gated, NOT asserted as catalogue-confirmed. The exact spellings to confirm: whether the component count is `Graph.Connectivity` or a `Graph.ConnectedComponents`-length; whether `ClosenessCentrality`/`DegreeCentrality` exist parallel to the cited `BetweennessCentrality`; whether the minimum spanning structure is `Graph.Tree`/`Graph.MinimumSpanningTree`; whether the graph-side `Graph.Vertices`/`Graph.Edges` accessors index the centrality/path extrema (the catalogue cites the `Topology.Vertices` accessor at analysis row [03] but the `Graph.Vertices`/`Graph.Edges` graph-side accessors are gated); and whether `Graph.JSONData` returns the JSON-serializable dict `msgspec.json.encode` consumes or `Graph.JSONString` returns a pre-serialized string. A reducer whose gated spelling fails resolution drops from the `analytics` default set without a fence rewrite, since `_graph_analytics` folds only the policy-selected rows.
- [IFC_BYTES_CONSTRUCTOR]: the folder `.api/topologicpy.md` confirms ONLY `Topology.ByIFCFile(file)` (entrypoint row [01]) — no `By*Bytes` or in-memory byte-stream constructor enumerates. The open question gating the `SourceKind.IFC` growth row: does `Topology.ByIFCFile(file)` accept an already-opened `ifcopenshell.file` object (the catalogue's bare `(file)` spelling is ambiguous between a path string and a file handle) or strictly a path string? Resolution paths, in priority: (1) if `ByIFCFile` accepts an `ifcopenshell.file`, the row opens the byte stream through `ifcopenshell.file.from_string` (the in-memory SPF loader the `mesh/daemon.md#DAEMON` `ifc` arm uses, confirmed on `.api/ifcopenshell.md` entrypoints [17]) and hands the model object straight in; (2) failing that, the `IFC` integration facade (`topologicpy.md` BIM-module row [01]) is inspected for a model-object entry; (3) failing both, the `SourceKind.OCCT` row over `Topology.ByOCCTShape(shape)` is the bridge — the `ifcopenshell.file` geometry lifts to an OCCT shape `ByOCCTShape` ingests. The temp-file detour (writing the byte stream to disk so the path constructor reads it) is the FORBIDDEN form: the byte stream owns the model everywhere else in the companion, and a path round-trip re-introduces filesystem I/O the daemon's in-memory open exists to avoid. This residual stays a marked RESEARCH gate until a branch `topologicpy` companion-interpreter assay confirms one of the three constructor arities; the `SourceKind.IFC` row admits IFC alongside B-rep as ONE `_CONSTRUCT` table growth (never a new owner) only after that confirmation. Secondary gate: `topologicpy` is itself absent from the manifest (consumed by this page and `graph/algebra.md`), so the admission is moot until the opt-in companion-band row lands.

## [04]-[UPSTREAM]

- [DUAL_GATE_EXCLUSION]: `topologicpy` carries two independent blockers that BOTH must clear before this owner is reachable. License: `AGPL-3.0-or-later` is network-copyleft — §13 extends the source-disclosure obligation to anyone interacting with a hosted deployment over a network, so linking `topologicpy` into the shipped server would place the whole deployment under AGPL. The default server build therefore EXCLUDES `topologicpy`; the runtime is admitted only on the explicit, AGPL-accepting Forge companion lane (`forge-companion-env`) as an opt-in dependency, isolated at the process boundary like the copyleft IFC wheel. Wheel band: `requires-python<'3.15'` means no cp315 wheel exists, so even absent the license concern the package cannot ride the intended cp315 core — it pins to the cp312 floor inside the `<'3.15'` companion band. Both gates resolve to the same conclusion: this surface is dark in the default build, fully authored but never imported there, and only lit on the opt-in companion lane once the manifest gains the AGPL-acknowledged `python_version<'3.15'` row. Replacement of the AGPL dependency is mandated by card `geometry [GEO_TOPOLOGICPY_LICENSE_REPLACE]` (reference only).
</content>
</invoke>
