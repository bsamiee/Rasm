# [PY_DATA_NETWORK]

Capacity-constrained network owner for building-service graphs — duct, pipe, cable, and egress-circulation networks answer max-flow, min-cut, minimum-cost flow, and network-simplex questions over one `FlowNetwork` payload. The flow family is the one algorithm set the `graph/graph#GRAPH` rustworkx kernel does not spell, so this page rides the networkx lane for exactly that family and nothing beside it: admission, node vocabulary, and result lowering compose the sibling owner — `NodeId` stays the stable `int` index, and results lower through `GraphResult` (`flows` the edge-keyed case this family mints).

Edge capacity, cost, and node demand are DATA on the admitted rows, never networkx attribute strings a caller spells — the owner projects its rows onto the `capacity`/`weight`/`demand` attribute vocabulary the provider reads, so the provider's naming convention is an interior fact with one spelling site. Infeasibility is a typed refusal: `nx.NetworkXUnfeasible` on an unbalanced demand set rails through the boundary fence as the fault a sizing consumer reads, never an exception crossing the rail.

## [01]-[INDEX]

- [02]-[NETWORK]: the `FlowNetwork` owner — capacity-row admission, the `FlowAlgorithm` family over the networkx flow kernels, and `GraphResult` lowering.

## [02]-[NETWORK]

- Owner: `FlowNetwork` — one frozen payload carrying the built `nx.DiGraph` beside its edge roster and content key, so the analyzed graph never decouples from the admitted rows; `FlowEdge` the capacity-annotated edge row (`capacity` required, `weight` the per-unit cost defaulting free); node demands one `Map[NodeId, float]` where negative supplies and positive demands follow the provider's own sign convention, stated once here.
- Law: this page owns ONLY the flow family — every other analysis question routes to the `graph/graph#GRAPH` kernel, and a second analysis surface here is the rejected parallel kernel; the split predicate is kernel availability, the same law the folder's fast-path ruling states. The networkx graph is built HERE from admitted rows and never accepted as a caller-passed handle, so attribute spelling, multigraph refusal, and node vocabulary stay this owner's interior.
- Entry: `analyze` folds one `FlowAlgorithm` through the provider kernel under one boundary fence per run — `max_flow` answers the flow value beside its edge assignment, `min_cut` the cut value beside the partition, `min_cost` and `simplex` the demand-satisfying assignment, `max_flow_min_cost` the cheapest maximum flow — each lowering onto `GraphResult` so the frame join rides the sibling surface unchanged.
- Law: the payload keys once at admission over the canonical edge-roster bytes, so an unchanged network keys byte-stable.
- Packages: `networkx` (`maximum_flow`, `minimum_cut`, `min_cost_flow`, `network_simplex`, `max_flow_min_cost` — the flow kernels, `capacity=`/`weight=`/`demand=` their attribute keywords), `msgspec` (frozen rows and the canonical key encoding), runtime (`RuntimeRail`/`boundary`/`Catch`/`FaultRow`/`ContentIdentity`/`scoped`).
- Growth: a new flow question is one `FlowAlgorithm` case plus one `_run_flow` arm; a new edge annotation is one `FlowEdge` field projected at the one build site; a networkx `@_dispatchable` flow accelerator is the same `backend=` policy row the sibling codec lane names, never a second kernel.
- Boundary: no durable network store, no hydraulic or electrical physics (sizing semantics belong to the consumer reading the flow evidence), no undirected admission — a service network is directed by construction and an undirected question routes to the sibling kernel; `NodeId` never widens beyond the stable `int` index the folder's frame seam joins on.

```python
from collections.abc import Iterable
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import msgspec
import networkx as nx
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.graph.graph import GraphResult, NodeId
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey

if TYPE_CHECKING:
    from collections.abc import Callable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.graph.network")


# --- [CONSTANTS] ------------------------------------------------------------------------

_FLOW_RAISES: Final[Catch] = (nx.NetworkXException, msgspec.MsgspecError, TypeError, ValueError)

NETWORK_BUILD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.NETWORK, point="build", arm="boundary", defect="network-build", retriability=TERMINAL
)
NETWORK_FLOW: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.NETWORK, point="flow", arm="boundary", defect="flow-kernel", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([NETWORK_BUILD, NETWORK_FLOW]))


# --- [MODELS] ---------------------------------------------------------------------------


class FlowEdge(Struct, frozen=True, gc=False):
    source: NodeId
    target: NodeId
    capacity: float
    weight: float = 0.0


@tagged_union(frozen=True)
class FlowAlgorithm:
    tag: Literal["max_flow", "min_cut", "min_cost", "simplex", "max_flow_min_cost"] = tag()
    max_flow: tuple[NodeId, NodeId] = case()
    min_cut: tuple[NodeId, NodeId] = case()
    min_cost: None = case()
    simplex: None = case()
    max_flow_min_cost: tuple[NodeId, NodeId] = case()


class FlowNetwork(Struct, frozen=True):
    graph: Any
    edges: tuple[FlowEdge, ...]
    node_count: int
    content_key: ContentKey

    @classmethod
    def of(cls, edges: tuple[FlowEdge, ...], demands: "Map[NodeId, float]" = Map.empty()) -> "RuntimeRail[FlowNetwork]":
        def build() -> "tuple[Any, bytes]":
            graph = nx.DiGraph()
            graph.add_weighted_edges_from(((e.source, e.target, e.capacity) for e in edges), weight="capacity")
            nx.set_edge_attributes(graph, {(e.source, e.target): e.weight for e in edges}, name="weight")
            nx.set_node_attributes(graph, dict(demands), name="demand")
            wire = msgspec.json.encode((
                sorted((e.source, e.target, e.capacity, e.weight) for e in edges),
                sorted(demands.to_list()),
            ))
            return graph, wire

        return boundary(NETWORK_BUILD, build, catch=_FLOW_RAISES).bind(
            lambda built: ContentIdentity.of("network", built[1]).map(
                lambda key: cls(graph=built[0], edges=edges, node_count=built[0].number_of_nodes(), content_key=key)
            )
        )

    def analyze(self, algo: FlowAlgorithm) -> "RuntimeRail[GraphResult]":
        with _TRACER.start_as_current_span(
            f"network.analyze.{algo.tag}",
            attributes={"rasm.graph.algorithm": algo.tag, "rasm.graph.backend": "networkx", "rasm.graph.nodes": self.node_count},
        ):
            return boundary(NETWORK_FLOW, lambda: _run_flow(self.graph, algo), catch=_FLOW_RAISES)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _flows(assignment: "dict[NodeId, dict[NodeId, float]]") -> GraphResult:
    return GraphResult(flows=tuple((u, v, f) for u, targets in assignment.items() for v, f in targets.items()))


def _run_flow(graph: Any, algo: FlowAlgorithm) -> GraphResult:
    match algo:
        case FlowAlgorithm(tag="max_flow", max_flow=(source, sink)):
            _value, assignment = nx.maximum_flow(graph, source, sink, capacity="capacity")
            return _flows(assignment)
        case FlowAlgorithm(tag="min_cut", min_cut=(source, sink)):
            _value, (reachable, unreachable) = nx.minimum_cut(graph, source, sink, capacity="capacity")
            return GraphResult(partition=(tuple(sorted(reachable)), tuple(sorted(unreachable))))
        case FlowAlgorithm(tag="min_cost"):
            return _flows(nx.min_cost_flow(graph, demand="demand", capacity="capacity", weight="weight"))
        case FlowAlgorithm(tag="simplex"):
            _cost, assignment = nx.network_simplex(graph, demand="demand", capacity="capacity", weight="weight")
            return _flows(assignment)
        case FlowAlgorithm(tag="max_flow_min_cost", max_flow_min_cost=(source, sink)):
            return _flows(nx.max_flow_min_cost(graph, source, sink, capacity="capacity", weight="weight"))
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
