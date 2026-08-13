# [PY_DATA_SOLVE]

Batch-solve and contribution-analysis leg of the impact plane: `LcaBatch` runs the `bw2calc.MultiLCA` shared-factorization solve — many functional units × many impact categories over ONE matrix factorization — for the sweeps too wide for the carrier's per-solve arity, and `Contribution` mines the drivers behind a score through the `bw2analyzer` surface: top processes, top emissions, and the recursive supply-chain walk. Both feed the carrier's world without forking it: the batch lowers to a score FRAME the columnar plane scans (a sweep is analytics, not a material carrier), a single-material score stays the carrier's `ImpactSource.brightway` arm, and contribution rows land on the carrier's own `ContributionRow` shape so a deep mine widens no band.

Demand vocabulary is the open project's: functional units arrive as resolved `(activity id, amount)` maps exactly as the carrier's `LcaSolve.demand` does, and `bd.get_multilca_data_objs` mounts the datapackages the one factorization consumes — this page assembles no matrix and re-derives no demand key. Embedded solvers carry no scrape surface, so the solve span is the whole observability, the `query`-plane law the carrier already applies.

## [01]-[INDEX]

- [02]-[SOLVE]: the `LcaBatch` shared-factorization sweep — `MultiLCA` over one mounted datapackage set, the score-frame lowering, the typed batch receipt.
- [03]-[CONTRIBUTION]: the `Contribution` driver-mining axis over `bw2analyzer` — processes, emissions, the recursive walk.

## [02]-[SOLVE]

- Owner: `LcaBatch` — one frozen batch per project: named functional units (`dict[str, dict[int, float]]`, the provider's own demand shape), the impact-category tuple set, and the Monte Carlo iteration count; ONE `MultiLCA` construction serves every `(unit, category)` cell, which is the whole point — N×M scores at one factorization instead of N×M solves.
- Law: `method_config` spells `{"impact_categories": [...]}` — the provider's own config key, spelled once here — and `data_objs` mounts through `bd.get_multilca_data_objs(functional_units, method_config)`, so the mounted set derives from the SAME two inputs the solve reads and a hand-assembled datapackage list cannot drift from the demand it serves.
- Law: the batch lowers to a self-describing score frame — `unit`/`category`/`amount` columns keyed by the batch `ContentKey` — through the folder's canonical Arrow fold at the caller's columnar seam; a per-cell `MaterialImpact` mint is the rejected form, because a sweep row is analytics evidence, not a material declaration, and the carrier stays the one EN 15804 matrix owner.
- Receipt: one `SolveReceipt` per batch — unit count, category count, iterations, the batch key — under `domain="impact"`/`kind="batch"`; identity folds the sorted functional-unit and category rosters through the deterministic encoder, so an identical sweep dedupes and a widened one re-keys.
- Packages: `bw2calc` (`MultiLCA(demands, method_config, data_objs, use_distributions)`, `.lci()`/`.lcia()`/`.scores`, `keep_first_iteration`), `bw2data` (`projects.set_current`, `get_multilca_data_objs`), `pyarrow` (the score-frame lowering) — every one bound at module scope through its own `lazy import`, so the compiled solver and Arrow loads fall on first use with no function-local import — runtime (`RuntimeRail`/`boundary`/`ContentIdentity`/`scoped`/`Metrics`/`on_thread`).
- Growth: a new sweep axis is more rows in the two admitted rosters — zero surface; a distribution summary beyond the mean is one field on `SolveReceipt`; zero new solver.
- Boundary: no matrix assembly (`impact/inventory#PACKAGES` custodies datapackages), no per-material carrier mint (the carrier's `brightway` arm owns it), no method authoring — categories name methods the project already holds, and an absent method surfaces as the provider's own raise railed at the fence.

```python signature
from typing import TYPE_CHECKING, Final, Literal

from msgspec import Struct
from msgspec import json as msgjson
from opentelemetry import trace

lazy import bw2calc as bc
lazy import bw2data as bd
lazy import pyarrow as pa

from rasm.runtime.faults import RuntimeRail, boundary, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from collections.abc import Iterable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact.solve")
_ENCODER: Final = msgjson.Encoder(order="deterministic")


class SolveReceipt(Struct, frozen=True, gc=False):
    units: int
    categories: int
    iterations: int
    content_key: ContentKey

    def contribute(self) -> "Iterable[Receipt]":
        Metrics.record({"rasm.impact.solves": float(self.units * self.categories)}, domain="impact", kind="batch")
        yield Receipt.of(
            "solve",
            (
                "emitted",
                "multilca",
                {
                    "domain": "impact",
                    "kind": "batch",
                    "key": self.content_key.hex,
                    "units": self.units,
                    "categories": self.categories,
                    "iterations": self.iterations,
                },
            ),
        )


class LcaBatch(Struct, frozen=True):
    project: str
    # the provider's own demand shape: name -> {activity id: amount}; names key the score frame's `unit` column.
    functional_units: "dict[str, dict[int, float]]"
    categories: tuple[tuple[str, ...], ...]
    iterations: int = 0

    async def solved(self) -> "RuntimeRail[tuple[pa.Table, SolveReceipt]]":
        # ONE factorization serves every (unit, category) cell; the blocking sparse solve rides the band hop and
        # the span is the embedded engine's whole observability surface.
        def run() -> "tuple[pa.Table, SolveReceipt]":
            bd.projects.set_current(self.project)
            config = {"impact_categories": [tuple(category) for category in self.categories]}
            data_objs = bd.get_multilca_data_objs(functional_units=self.functional_units, method_config=config)
            lca = bc.MultiLCA(
                demands=self.functional_units, method_config=config, data_objs=data_objs, use_distributions=self.iterations > 0
            )
            lca.lci()
            lca.lcia()
            # `scores` keys (category tuple, unit name) -> float; the frame carries both halves as columns.
            rows = tuple((unit, ":".join(category), float(score)) for (category, unit), score in lca.scores.items())
            table = pa.Table.from_pydict({
                "unit": [unit for unit, _, _ in rows],
                "category": [category for _, category, _ in rows],
                "amount": [amount for _, _, amount in rows],
            })
            identity = _ENCODER.encode((sorted(self.functional_units), sorted(self.categories), self.iterations))
            key = ContentIdentity.key("impact", identity)
            receipt = SolveReceipt(
                units=len(self.functional_units), categories=len(self.categories), iterations=self.iterations, content_key=key
            )
            return table, receipt

        with _TRACER.start_as_current_span(
            "solve.batch", attributes={"rasm.impact.units": len(self.functional_units), "rasm.impact.categories": len(self.categories)}
        ):
            railed = await on_thread(lambda: boundary("solve.batch", run))
            return railed.bind(lambda rail: rail)
```

## [03]-[CONTRIBUTION]

- Owner: `Contribution` — the driver-mining axis over one solved `LCA`: `processes` and `emissions` the annotated top-N tables (`bw2analyzer.ContributionAnalysis`), `recursive` the depth-bounded supply-chain walk (`print_recursive_calculation` captured off its own `file_obj` seam, never a stdout scrape). Rows land on the carrier's `ContributionRow` shape — score, supply, name — so a mined driver joins the carrier's contribution slot with zero new row family; the recursive walk answers indented text rows, its own evidence kind.
- Law: mining consumes a SOLVED `lca` the caller threads in — the carrier's `_from_score` arm or the batch above both hold one — so a mine never re-solves; the depth and cutoff are case payload, never owner fields, because two mines over one solve legitimately carry two depths.
- Growth: a new contribution kind is one `Contribution` case plus one arm (`compare_activities_by_grouped_leaves` lands this way when a comparison consumer names it); zero new surface.
- Boundary: no solve, no carrier mint, no plotting — the analysis surface's chart members are artifacts-plane concerns this owner never touches.

```python signature
import io
from typing import Literal, assert_never

from expression import case, tag, tagged_union

lazy from bw2analyzer import ContributionAnalysis, print_recursive_calculation


@tagged_union(frozen=True)
class Contribution:
    tag: Literal["processes", "emissions", "recursive"] = tag()
    processes: int = case()  # top-N limit
    emissions: int = case()  # top-N limit
    recursive: tuple[tuple[str, ...], int, float] = case()  # (lcia method, max_level, cutoff)

    def mined(self, lca: object, activity: object | None = None) -> "RuntimeRail[tuple[tuple[float, float, str], ...]]":
        # (score, supply, name) triples on the carrier's ContributionRow shape; the recursive arm captures the
        # provider's own file_obj text seam and yields (0, 0, line) rows — indented text IS its evidence kind.
        def run() -> tuple[tuple[float, float, str], ...]:
            match self:
                case Contribution(tag="processes", processes=limit):
                    mined = ContributionAnalysis().annotated_top_processes(lca, limit=limit)
                    return tuple((float(score), float(supply), str(name)) for score, supply, name in mined)
                case Contribution(tag="emissions", emissions=limit):
                    mined = ContributionAnalysis().annotated_top_emissions(lca, limit=limit)
                    return tuple((float(score), float(supply), str(name)) for score, supply, name in mined)
                case Contribution(tag="recursive", recursive=(method, max_level, cutoff)):
                    sink = io.StringIO()
                    print_recursive_calculation(activity, tuple(method), max_level=max_level, cutoff=cutoff, file_obj=sink)
                    return tuple((0.0, 0.0, line) for line in sink.getvalue().splitlines())
                case unreachable:
                    assert_never(unreachable)

        with _TRACER.start_as_current_span(f"solve.contribution.{self.tag}"):
            return boundary(f"solve.contribution.{self.tag}", run)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
