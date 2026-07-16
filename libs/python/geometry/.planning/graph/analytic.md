# [PY_GEOMETRY_GRAPH_ANALYTIC]

The tier-0 graph-analytics substrate — the one owner of the reducer-return algebra both graph-analytics producers compose. `AnalyticValue` collapses every reducer return — a `scalar`, a `leaderboard`, a `groups` partition — to one typed carrier owning its own dual projection, and `ranked` is the one polymorphic ranking fold over both provider shapes, discriminating on input shape, never a sibling fold per producing page. This page authors NO analytics, receipts, or graduation — the producing pages own those.

`graph/features` owns the `networkx` reducer table and `graph/nonmanifold` the `topologicpy` one; both import this vocabulary downward and mint no parallel value family. `graph/algebra` is the compas-numerics sibling minting its own scalar `Census` — it produces COMPAS-JSON handles plus residuals, never a reducer-return analytic, so it composes the graduation spine directly and this substrate not at all.

## [01]-[INDEX]

- [01]-[ANALYTIC]: the `AnalyticValue` union with its dual projections, the polymorphic `ranked` fold, and the `peak_of`/`scalar_of` census projections.

## [02]-[ANALYTIC]

- Owner: `AnalyticValue` is the one carrier for every graph-analytic reducer return; each projection closes with `assert_never`, so a new return shape breaks every census at type-check.
- Packages: `expression` and `numpy` per the fence imports; `msgspec`-free — this owner carries no wire shape, the consuming pages serialize.
- Growth: a new return shape is one case plus one arm per projection; a new census read is a consumer-side `peak_of`/`scalar_of` call, never a new projection here; a provider whose scores arrive keyed by string node ids extends `ranked`'s probe by one arm.
- Boundary: no analytics tables, no graph construction, no receipts, no graduation — the producing pages own those; no parallel `AnalyticValue` twin authored beside this one, no module-level `_peak` fold beside the union, and no msgspec subclass family for the same bounded variant set.

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
    # a node-score mapping (networkx centrality dict) and a vertex-ordered score list (topologicpy
    # centrality) both rank through one sort — the input shape is the discriminant.
    pairs = scores.items() if isinstance(scores, Mapping) else enumerate(scores)
    board = sorted(pairs, key=lambda pair: pair[1], reverse=True)[:cap]
    return AnalyticValue.Leaderboard(tuple((int(node), float(score)) for node, score in board))


def peak_of[K](values: Map[K, AnalyticValue], key: K) -> float:
    # the census-peak projection: an absent row folds to 0.0 with no None arm, so census fields stay total.
    return values.try_find(key).map(lambda value: value.peak()).default_value(0.0)


def scalar_of[K](values: Map[K, AnalyticValue], key: K) -> float:
    return values.try_find(key).map(lambda value: value.as_scalar()).default_value(0.0)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
