# [PY_DATA_SOLVE]

Batch-solve and contribution-analysis leg of the impact plane: `LcaBatch` runs the `bw2calc.MultiLCA` shared-factorization solve — many functional units × many impact categories over ONE matrix factorization — for the sweeps too wide for the carrier's per-solve arity, and `Contribution` mines the drivers behind a score through the `bw2analyzer` surface: top processes, top emissions, and the recursive supply-chain walk. Both feed the carrier's world without forking it: the batch lowers to a score FRAME the columnar plane scans (a sweep is analytics, not a material carrier), a single-material score stays the carrier's `ImpactSource.brightway` arm, and contribution rows compose the carrier's own `ContributionRow` so a deep mine widens no band. `Mined` keeps the walk's indented text apart from those measured rows, because a text line carries no score to report.

Demand vocabulary is the open project's: functional units arrive as resolved `(activity id, amount)` maps exactly as the carrier's `LcaSolve.demand` does, and `bd.get_multilca_data_objs` mounts the datapackages the one factorization consumes — this page assembles no matrix and re-derives no demand key. Embedded solvers carry no scrape surface, so the solve span is the whole observability, the `query`-plane law the carrier already applies.

## [01]-[INDEX]

- [02]-[SOLVE]: the `LcaBatch` shared-factorization sweep — `MultiLCA` over one mounted datapackage set, the score-frame lowering, the typed batch receipt.
- [03]-[CONTRIBUTION]: the `Contribution` driver-mining axis over `bw2analyzer` — processes, emissions, the recursive walk.

## [02]-[SOLVE]

- Owner: `LcaBatch` — one frozen batch per project: named functional units (`dict[str, dict[int, float]]`, the provider's own demand shape), the impact-category tuple set, and the Monte Carlo iteration count; ONE `MultiLCA` construction serves every `(unit, category)` cell, which is the whole point — N×M scores at one factorization instead of N×M solves.
- Law: `method_config` spells `{"impact_categories": [...]}` — the provider's own config key, spelled once here — and `data_objs` mounts through `bd.get_multilca_data_objs(functional_units, method_config)`, so the mounted set derives from the SAME two inputs the solve reads and a hand-assembled datapackage list cannot drift from the demand it serves.
- Law: the batch lowers to a self-describing score frame — `unit`/`category`/`amount` columns keyed by the batch `ContentKey` — through the folder's canonical Arrow fold at the caller's columnar seam; a per-cell `MaterialImpact` mint is the rejected form, because a sweep row is analytics evidence, not a material declaration, and the carrier stays the one EN 15804 matrix owner.
- Receipt: one `SolveReceipt` per batch — unit count, category count, iterations, the batch key — under `domain="impact"`/`kind="batch"`; identity folds the sorted functional-unit and category rosters through the deterministic encoder, so an identical sweep dedupes and a widened one re-keys.
- Packages: `bw2calc` (`MultiLCA(demands, method_config, data_objs, use_distributions)`, `.lci()`/`.lcia()`/`.scores`, `keep_first_iteration`), `bw2data` (`projects.set_current`, `get_multilca_data_objs`), `pyarrow` (the score-frame lowering) — every one bound at module scope through its own `lazy import`, so the compiled solver and Arrow loads fall on first use with no function-local import, and each raise set resolves at its call for the same reason — runtime (`RuntimeRail`/`boundary`/`Catch`/`Depth`/`FaultRow`/`ContentIdentity`/`scoped`/`Metrics`/`on_thread`), `impact/impact#IMPACT` (`ContributionRow`, the carrier's own mined-driver row this page composes and never re-declares).
- Growth: a new sweep axis is more rows in the two admitted rosters — zero surface; a distribution summary beyond the mean is one field on `SolveReceipt`; zero new solver.
- Boundary: no matrix assembly (`impact/inventory#PACKAGES` custodies datapackages), no per-material carrier mint (the carrier's `brightway` arm owns it), no method authoring — categories name methods the project already holds, and an absent method surfaces as the provider's own raise railed at the fence.

```python signature
from typing import TYPE_CHECKING, Final, Literal

from expression.collections import Block
from msgspec import Struct
from msgspec import json as msgjson
from opentelemetry import trace

lazy import bw2calc as bc
lazy import bw2data as bd
lazy import pyarrow as pa

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from collections.abc import Iterable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact.solve")
_ENCODER: Final = msgjson.Encoder(order="deterministic")


# --- [CONSTANTS] ------------------------------------------------------------------------


def _solve_raises() -> Catch:
    return (bc.errors.BW2CalcError, bd.errors.BW2Exception, pa.ArrowException, KeyError, TypeError, ValueError, OSError)


def _mine_raises() -> Catch:
    return (bc.errors.BW2CalcError, KeyError, IndexError, TypeError, ValueError, OSError)


SOLVE_BATCH: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SOLVE, point="batch", arm="boundary", defect="multilca", retriability=TERMINAL
)
SOLVE_MINE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SOLVE, point="contribution", arm="boundary", defect="driver-mine", retriability=TERMINAL
)
SOLVE_UNBOUNDED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.SOLVE, point="contribution.depth", arm="config", defect="unbounded-walk", retriability=TERMINAL, slots=("mine",)
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([SOLVE_BATCH, SOLVE_MINE, SOLVE_UNBOUNDED]))


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
    functional_units: "dict[str, dict[int, float]]"
    categories: tuple[tuple[str, ...], ...]
    iterations: int = 0

    async def solved(self) -> "RuntimeRail[tuple[pa.Table, SolveReceipt]]":
        def run() -> "tuple[pa.Table, SolveReceipt]":
            bd.projects.set_current(self.project)
            config = {"impact_categories": [tuple(category) for category in self.categories]}
            data_objs = bd.get_multilca_data_objs(functional_units=self.functional_units, method_config=config)
            lca = bc.MultiLCA(
                demands=self.functional_units, method_config=config, data_objs=data_objs, use_distributions=self.iterations > 0
            )
            lca.lci()
            lca.lcia()
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
            railed = await on_thread(lambda: boundary(SOLVE_BATCH, run, catch=_solve_raises()))
            return railed.bind(lambda rail: rail)
```

## [03]-[CONTRIBUTION]

- Owner: `Contribution` — the driver-mining axis over one solved `LCA`: `processes` and `emissions` the annotated top-N tables (`bw2analyzer.ContributionAnalysis`), `recursive` the depth-bounded supply-chain walk (`print_recursive_calculation` captured off its own `file_obj` seam, never a stdout scrape). The annotated arms COMPOSE the carrier's `ContributionRow` — score, supply, activity — so a mined driver joins the carrier's contribution slot with zero new row family, and `Mined` keeps the walk's indented text in its own case rather than forging a zero score and supply onto a row shape that promises measurements.
- Law: mining consumes a SOLVED `lca` the caller threads in — the carrier's `_from_score` arm or the batch above both hold one — so a mine never re-solves; the activity, the walk bound, and the cutoff are case payload, never owner fields, because two mines over one solve legitimately carry two bounds and only the walking arm takes an activity at all. The bound is the runtime `Depth`, and the provider spells no convergence walk, so a `fixpoint` request refuses by name instead of being lowered to a level the provider would truncate at.
- Growth: a new contribution kind is one `Contribution` case plus one arm (`compare_activities_by_grouped_leaves` lands this way when a comparison consumer names it), landing on the `Mined` case its evidence kind already names; a new refusal law is one `FaultRow` row on this module's `RAISES` table; zero new surface.
- Boundary: no solve, no carrier mint, no plotting — the analysis surface's chart members are artifacts-plane concerns this owner never touches.

```python signature
import io
from typing import Literal, assert_never

from expression import Error, Ok, case, tag, tagged_union

from rasm.data.impact.impact import ContributionRow
from rasm.runtime.faults import Depth

lazy from bw2analyzer import ContributionAnalysis, print_recursive_calculation


@tagged_union(frozen=True)
class Mined:
    tag: Literal["rows", "lines"] = tag()
    rows: "tuple[ContributionRow, ...]" = case()
    lines: tuple[str, ...] = case()


@tagged_union(frozen=True)
class Contribution:
    tag: Literal["processes", "emissions", "recursive"] = tag()
    processes: int = case()
    emissions: int = case()
    recursive: "tuple[object, tuple[str, ...], Depth, float]" = case()

    def mined(self, lca: object) -> "RuntimeRail[Mined]":
        def run() -> "RuntimeRail[Mined]":
            match self:
                case Contribution(tag="processes", processes=limit):
                    mined = ContributionAnalysis().annotated_top_processes(lca, limit=limit)
                    return Ok(Mined(rows=_rows(mined)))
                case Contribution(tag="emissions", emissions=limit):
                    mined = ContributionAnalysis().annotated_top_emissions(lca, limit=limit)
                    return Ok(Mined(rows=_rows(mined)))
                case Contribution(tag="recursive", recursive=(activity, method, bound, cutoff)):
                    match bound:
                        case Depth(tag="fixpoint"):
                            return Error(SOLVE_UNBOUNDED.raised("recursive"))
                        case Depth(tag="bounded", bounded=level):
                            sink = io.StringIO()
                            print_recursive_calculation(activity, tuple(method), max_level=level, cutoff=cutoff, file_obj=sink)
                            return Ok(Mined(lines=tuple(sink.getvalue().splitlines())))
                        case _ as unreachable:
                            assert_never(unreachable)
                case unreachable:
                    assert_never(unreachable)

        with _TRACER.start_as_current_span(f"solve.contribution.{self.tag}"):
            return boundary(SOLVE_MINE, run, catch=_mine_raises()).bind(lambda railed: railed)


def _rows(mined: "Iterable[tuple[float, float, object]]") -> "tuple[ContributionRow, ...]":
    return tuple(ContributionRow(score=float(score), supply=float(supply), activity=str(name)) for score, supply, name in mined)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
