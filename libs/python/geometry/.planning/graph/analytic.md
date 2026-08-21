# [PY_GEOMETRY_GRAPH_ANALYTIC]

Tier-0 graph-analytics substrate owning the reducer-return algebra both graph-analytics producers compose. `AnalyticValue` collapses every reducer return — `scalar`, `leaderboard`, `groups` partition, `reach` per-hop census — to one typed carrier with its own three projections, `ranked` folds both provider shapes through one ranking discriminating on input shape, and `reached` is the band's one layered reachability walk, never a sibling fold per producing page. Authors no analytics, receipts, or graduation; the producing pages own those.

`graph/features` owns the `networkx` reducer table, `graph/nonmanifold` the `topologicpy` one; both import this vocabulary downward and mint no parallel value family. `graph/algebra`, the compas-numerics sibling, mints its own scalar `Census` — it produces COMPAS-JSON handles and residuals, never a reducer-return analytic, so it composes the graduation spine directly and this substrate not at all.

## [01]-[INDEX]

- [02]-[ANALYTIC]: `AnalyticValue` union with its scalar, peak, and columnar projections, the polymorphic `ranked` fold, the `Depth`-bounded `reached` walk, and the `peak_of`/`scalar_of` census projections.

## [02]-[ANALYTIC]

- Law: a reachability walk PUBLISHES the per-hop census it computed and the bare reach derives off it — `as_scalar` counts the nodes reached and `peak` reads the eccentricity, both from one evidence a single scalar could not reconstruct. `reached` states its merge law AT the fold: a node reachable from two seeds keeps the SMALLEST depth, so seed order cannot change the answer. The bound is the runtime `Depth` carrier and exhaustion is the band's own `unreached` token, so a bounded walk still holding a frontier refuses rather than certifying a truncated census as a converged one.
- Owner: `AnalyticValue` is the one carrier for every graph-analytic reducer return; each projection closes with `assert_never`, so a new return shape breaks every census at type-check. `tabled` is the columnar third projection — the shape the graduation `EvidenceFrame` port admits — so an analytic board crosses the geometry-to-data seam through the producing page's frame row while this substrate stays graduation-free.
- Packages: `expression` and `numpy` per the fence imports, and the runtime `Depth` walk bound alone — no folder sibling, so this substrate stays below every producer; `msgspec`-free, this owner carrying no wire shape while the consuming pages serialize.
- Growth: a new return shape is one case and one arm per projection; a new reachability question is one `reached` call with its own neighbourhood and seed set, never a second walk; a new census read is a consumer-side `peak_of`/`scalar_of` call, never a new projection here; a provider whose scores arrive keyed by string node ids extends `ranked`'s probe by one arm.
- Boundary: no analytics tables, no graph construction, no receipts, no graduation — the producing pages own those; no parallel `AnalyticValue` twin authored beside this one, no module-level `_peak` fold beside the union, and no msgspec subclass family for the same bounded variant set.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Mapping, Sequence
from typing import Literal, assert_never

import numpy as np
from expression import Option, case, tag, tagged_union
from expression.collections import Block, Map

from rasm.runtime.faults import Depth

# --- [TYPES] ----------------------------------------------------------------------------

type Leaders = tuple[tuple[int, float], ...]
type Partition = tuple[tuple[int, ...], ...]
# the per-hop census a reachability walk COMPUTES, as (node, hop depth) pairs in node order — the evidence a bare
# reach count and a bare eccentricity both derive from, and which neither of them can reconstruct.
type Reaches = tuple[tuple[int, int], ...]


@tagged_union(frozen=True)
class AnalyticValue:
    tag: Literal["scalar", "leaderboard", "groups", "reach"] = tag()
    scalar: float = case()
    leaderboard: Leaders = case()
    groups: Partition = case()
    reach: Reaches = case()

    @staticmethod
    def Scalar(value: float) -> "AnalyticValue":
        return AnalyticValue(scalar=value)

    @staticmethod
    def Leaderboard(rows: Leaders) -> "AnalyticValue":
        return AnalyticValue(leaderboard=rows)

    @staticmethod
    def Groups(partition: Partition) -> "AnalyticValue":
        return AnalyticValue(groups=partition)

    @staticmethod
    def Reach(census: Reaches) -> "AnalyticValue":
        return AnalyticValue(reach=census)

    def as_scalar(self) -> float:
        # Cardinality projection: a scalar carries its value, a board or partition its member count, so a count-keyed analytic reads one float off the flat facts map.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return v
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return float(len(rows))
            case AnalyticValue(tag="groups", groups=partition):
                return float(len(partition))
            case AnalyticValue(tag="reach", reach=census):
                return float(len(census))  # how many nodes the walk reached — the bare reach, DERIVED off the census
            case _ as unreachable:
                assert_never(unreachable)

    def peak(self) -> float:
        # Head-magnitude projection where extremum is the signal: a scalar IS its peak, a board its top score,
        # a partition its member count — a centrality fact rides its max score.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return v
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return float(np.asarray([score for _, score in rows]).max(initial=0.0))
            case AnalyticValue(tag="groups", groups=partition):
                return float(len(partition))
            case AnalyticValue(tag="reach", reach=census):
                # the eccentricity from the walk's own seed set — the deepest hop any reached node sits at.
                return float(max((depth for _, depth in census), default=0.0))
            case _ as unreachable:
                assert_never(unreachable)

    def tabled(self) -> dict[str, np.ndarray]:
        # Columnar projection the graduation frame port consumes — dict order IS the column order; the producing
        # pages key the frame's subject, so this substrate stays receipt- and graduation-free.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return {"value": np.asarray([v], dtype=np.float64)}
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return {
                    "node": np.asarray([node for node, _ in rows], dtype=np.int64),
                    "score": np.asarray([score for _, score in rows], dtype=np.float64),
                }
            case AnalyticValue(tag="groups", groups=partition):
                return {
                    "group": np.arange(len(partition), dtype=np.int64),
                    "members": np.asarray([len(group) for group in partition], dtype=np.int64),
                }
            case AnalyticValue(tag="reach", reach=census):
                return {
                    "node": np.asarray([node for node, _ in census], dtype=np.int64),
                    "depth": np.asarray([depth for _, depth in census], dtype=np.int64),
                }
            case _ as unreachable:
                assert_never(unreachable)


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class GraphFault(Exception):
    # the graph band's ONE structured refusal, seated at the substrate both producers already import: raised INTO the
    # converting fence of whichever producer folded this reducer, so the offending value survives as a kwarg the
    # boundary fault lifts whole rather than as an f-string a reader re-parses. Both producers fold their reducers
    # inside an offloaded kernel, and that crossing carries the token as `CrossedFault` DATA, re-minting this family's
    # own case parent-side per `execution/workers#CROSSING` — the substrate declares the cases and edits nothing.
    tag: Literal["negative_cap", "unreached"] = tag()
    negative_cap: int = case()  # the board cap a caller supplied below zero
    unreached: tuple[str, int] = case()  # (the spent bound, the frontier still standing) — a walk that never converged

    def __str__(self) -> str:
        # `BoundaryFault.of` admits a `Tagged()` token AHEAD of every `CLASSIFY` row, so this family crosses the
        # conversion door WHOLE on the `domain` case and the catch-all's `str(cause)` half never renders it. A
        # worker seam carries it whole too: `execution/workers#CROSSING` lowers the token onto `CrossedFault` DATA
        # at `shipped` and re-mints this family's own case parent-side, so a raise inside a HOSTILE kernel needs no
        # edit here. `__str__` serves the LOG and HOST edge alone — a token surfacing in a worker traceback or a log
        # line before the seam lowers it — where `Exception.__str__` answers the EMPTY string for a kwarg-only
        # union. The law half IS the tag, so no arm re-spells its own case name and a renamed case cannot drift.
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case GraphFault(tag="negative_cap", negative_cap=cap):
                return str(cap)
            case GraphFault(tag="unreached", unreached=(spent, frontier)):
                return f"{spent}[{frontier}]"
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


def ranked(scores: Mapping[int, float] | Sequence[float], cap: int) -> AnalyticValue:
    # Node-score mapping (networkx dict) and vertex-ordered score list (topologicpy) both rank through one sort — input shape is the discriminant.
    if cap < 0:
        raise GraphFault(negative_cap=cap)  # a negative cap slices tail rows off silently instead of bounding the board; cap=0 stays the empty board
    pairs = scores.items() if isinstance(scores, Mapping) else enumerate(scores)
    board = sorted(pairs, key=lambda pair: pair[1], reverse=True)[:cap]
    return AnalyticValue.Leaderboard(tuple((int(node), float(score)) for node, score in board))


def reached(neighbours: Callable[[int], Iterable[int]], seeds: Iterable[int], bound: Depth) -> AnalyticValue:
    # the band's ONE reachability walk, layered rather than per-node: seeds enter at depth 0 and each layer expands
    # the previous one, so the value published IS the per-hop census the walk computed. A reach count and an
    # eccentricity both DERIVE off that census through `as_scalar` and `peak`, where a walk publishing one scalar
    # throws away the evidence the other reading needs and hands a reader a number nothing can be recomputed from.
    # MERGE LAW, stated where the fold runs: a node reachable from two seeds keeps the SMALLEST depth — the first
    # layer that reaches it — because a multi-seed reach asks how far the NEAREST seed is, and a later layer
    # re-recording it would publish a distance no shortest path takes. Held membership is that law's own authority:
    # a node already seated is never re-seated, so the ordering of the seed set cannot change the answer.
    # Exhaustion is TYPED, never a truncated success: a bounded walk still holding a frontier when its bound runs out
    # raises the band's own `unreached` token into the producer's converting fence, so an unconverged reach can never
    # certify as a complete one. `Depth.fixpoint` is the honest case for a walk that converges on its own.
    held: Map[int, int] = Map.of_seq((int(seed), 0) for seed in seeds)
    frontier, hop, budget = Block.of_seq(sorted(held.keys())), 0, bound
    while not frontier.is_empty():  # Exemption: each layer reads its own predecessor — the walk IS the sequential seam
        match budget.stepped():
            case Option(tag="none"):
                raise GraphFault(unreached=(budget.spelled, len(frontier)))
            case Option(tag="some", some=stepped):
                budget = stepped
            case _ as unreachable:
                assert_never(unreachable)
        hop += 1
        found = frontier.collect(lambda node: Block.of_seq(int(peer) for peer in neighbours(node))).filter(lambda peer: peer not in held)
        held = found.fold(lambda seat, peer: seat.add(peer, hop), held)
        frontier = Block.of_seq(sorted(frozenset(found)))
    return AnalyticValue.Reach(tuple(sorted(held.items())))


def peak_of[K](values: Map[K, AnalyticValue], key: K) -> float:
    # Census-peak projection: an absent row folds to 0.0 with no None arm, so census fields stay total.
    return values.try_find(key).map(lambda value: value.peak()).default_value(0.0)


def scalar_of[K](values: Map[K, AnalyticValue], key: K) -> float:
    return values.try_find(key).map(lambda value: value.as_scalar()).default_value(0.0)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
