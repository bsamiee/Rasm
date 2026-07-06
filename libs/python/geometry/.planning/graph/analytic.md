# [PY_GEOMETRY_GRAPH_ANALYTIC]

The tier-0 graph-analytics substrate — the one owner of the reducer-return algebra both graph-analytics producers compose. `AnalyticValue` is the `@tagged_union` collapsing every graph-analytic reducer return — a `scalar` float, a `leaderboard` of `(node, score)` rows, a `groups` partition — to one typed carrier owning its own dual projection: `as_scalar` the cardinality (a scalar its value, a board or partition its member count) and `peak` the head magnitude (a scalar its value, a board its top score, a partition its member count). `ranked` is the one polymorphic ranking fold over both provider shapes — a node-score mapping (the `networkx` centrality dict) and a vertex-ordered score sequence (the `topologicpy` centrality list) — discriminating on the input shape, never a sibling `_leaders`/`_ranked` pair per page. `peak_of`/`scalar_of` are the census projections: the `try_find().map(...).default_value(0.0)` Option fold every census reads its analytic scalars through, so an absent row folds to zero with no `None` arm. This page authors NO analytics — `graph/features` owns the `networkx` reducer table and `graph/nonmanifold` the `topologicpy` one; both import this vocabulary downward and mint no parallel value family. `graph/algebra` is the compas-numerics sibling minting its own scalar `Census` — it produces COMPAS-JSON handles plus residuals, never a reducer-return analytic, so it composes the graduation spine directly and this substrate not at all (the acyclic import graph and the seam ledger carry exactly two `→ graph/analytic` edges).

## [01]-[INDEX]

- [01]-[ANALYTIC]: the `AnalyticValue` `@tagged_union` with its `Scalar`/`Leaderboard`/`Groups` factories and dual `as_scalar`/`peak` projections, the polymorphic `ranked` fold, and the `peak_of`/`scalar_of` census projections.

## [02]-[ANALYTIC]

- Owner: `AnalyticValue` — the one carrier for every graph-analytic reducer return; `Leaders`/`Partition` the two composite payload aliases; `ranked` — the shape-discriminated ranking fold minting the truncated leaderboard; `peak_of`/`scalar_of` — the census-read projections over a `Map[K, AnalyticValue]`.
- Cases: `scalar(float)` (component counts, spanning weights, cycle counts, hop lengths) · `leaderboard(Leaders)` (centrality boards, truncated at the caller's cap) · `groups(Partition)` (community partitions, spanning-edge groups) — a new reducer return shape is one case plus one arm in each projection, breaking every census at type-check time.
- Entry: `ranked(scores, cap)` admits `Mapping[int, float] | Sequence[float]` over one shape probe — mapping items or enumerated positions — sorts descending on score, truncates at `cap`, and mints the `Leaderboard` case; `peak_of(values, key)`/`scalar_of(values, key)` fold the Option over an absent row to `0.0` so census fields stay total.
- Packages: `expression` (`tagged_union`/`case`/`tag`, `Map` the census carrier), `numpy` (`asarray(...).max(initial=0.0)` the head-score reduction), `msgspec`-free — this owner carries no wire shape; the consuming pages serialize.
- Growth: a new return shape is one case plus one arm per projection; a new census read is a consumer-side `peak_of`/`scalar_of` call, never a new projection here; a provider whose scores arrive keyed by string node ids extends `ranked`'s probe by one arm.
- Boundary: no analytics tables, no graph construction, no receipts, no graduation — the producing pages own those; no parallel `AnalyticValue` authored beside this one (the deleted form: the verbatim twin the features/nonmanifold pages once each carried); no module-level `_peak` fold beside the union; no msgspec subclass family for the same bounded variant set.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping, Sequence
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map

# --- [TYPES] ----------------------------------------------------------------------------

type Leaders = tuple[tuple[int, float], ...]
type Partition = tuple[tuple[int, ...], ...]


@tagged_union(frozen=True)
class AnalyticValue:
    # one carrier for every graph-analytic reducer return, owning its own dual projection —
    # never a parallel msgspec subclass family and never a module-level `_peak` fold beside it.
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
        # the cardinality projection: a scalar carries its value, a board or partition its member
        # count, so a count-keyed analytic reads one float off the flat facts map.
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
        # the head-magnitude projection where the extremum is the signal: a scalar IS its peak, a
        # board its top score, a partition its member count — a centrality fact rides its max score.
        match self:
            case AnalyticValue(tag="scalar", scalar=v):
                return v
            case AnalyticValue(tag="leaderboard", leaderboard=rows):
                return float(np.asarray([score for _, score in rows]).max(initial=0.0))
            case AnalyticValue(tag="groups", groups=partition):
                return float(len(partition))
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


def ranked(scores: Mapping[int, float] | Sequence[float], cap: int) -> AnalyticValue:
    # one polymorphic ranking fold over both provider shapes: a node-score mapping (networkx
    # centrality dict) and a vertex-ordered score list (topologicpy centrality) rank through one
    # sort — the input shape is the discriminant, never a sibling fold per producing page.
    pairs = scores.items() if isinstance(scores, Mapping) else enumerate(scores)
    board = sorted(pairs, key=lambda pair: pair[1], reverse=True)[:cap]
    return AnalyticValue.Leaderboard(tuple((int(node), float(score)) for node, score in board))


def peak_of[K](values: Map[K, AnalyticValue], key: K) -> float:
    # the census-peak projection: an absent row folds to 0.0 with no None arm, so census fields stay total.
    return values.try_find(key).map(lambda value: value.peak()).default_value(0.0)


def scalar_of[K](values: Map[K, AnalyticValue], key: K) -> float:
    return values.try_find(key).map(lambda value: value.as_scalar()).default_value(0.0)
```
