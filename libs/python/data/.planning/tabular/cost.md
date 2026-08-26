# [PY_DATA_COST]

The cost ledger projects producer-owned operation results into per-key, per-tenant quantities priced by caller policy. `CostFact.of` admits `QueryCensus`, `LakeResult`, `EgressResult`, `PartitionBundle`, an already-normalized fact, or the mapping returned by `Materialization.facts()` without importing gridded upward. `CostLedger.frame` groups those facts and returns one content-keyed `CostFrame`; it records no metric, opens no span, and owns no durable store.

## [01]-[INDEX]

- [02]-[COST]: `CostFact` normalizes producer results, `CostLedger` group-folds the slots, and `RatePolicy` prices one keyed Arrow frame.

## [02]-[COST]

- Owner: `CostFact` carries domain, kind, content key, tenant, and the `rows`/`bytes_moved`/`seconds`/`tasks` quantity axes. `CostFrame` carries the Arrow frame with its slot count, tenant count, total, unpriced count, currency, and content key.
- Entry: `CostLedger.of` traverses `CostFact.of` over producer results. `frame(policy)` prices and groups the facts, serializes the Arrow table through `arrow_bytes`, and keys the exact emitted bytes.
- Auto: query profiling omissions increment `unpriced` only when profiling was requested; lake file churn uses the producer's settled option values; materialization never adds partition identity to the cost grain.
- Packages: `pyarrow`, `msgspec`, `expression`, `opentelemetry-api`, `beartype`, the four producer pages, and runtime fault and identity surfaces.
- Growth: a new producer result is one `CostFact.of` arm; a new quantity axis extends `CostFact`, `CostUnit`, `_COST_COLUMNS`, `combined`, and `RatePolicy.price` together.
- Boundary: this is an in-memory projection over canonical producer results. Rates arrive as policy rows; there is no scan owner, storage owner, metric projection, or span here.
- Boundary: the priced frame is the TERMINAL egress and advertises no reader: `python:artifacts/visualization/table#TABLE` `TablePlan.of` admits it as the settled Arrow-capsule frame it is, so naming a renderer here claims a boundary neither end carries.
- Boundary: rates and totals ride `float` and no settlement reads this frame — observed spend is a dashboard reading, while the trapped exact-decimal arithmetic a settled charge demands homes at the runtime journal's rating fold and never at this projection.

```python
from collections.abc import Iterable, Mapping
from enum import StrEnum
from math import isfinite
from typing import Final, assert_never, cast

import pyarrow as pa
from beartype import beartype
from expression import Error, Ok, Some
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.data.tabular.columnar import ProfileMode, QueryCensus
from rasm.data.tabular.egress import EgressResult
from rasm.data.tabular.interop import ColumnSpec, DataLeg, arrow_bytes, column_frame
from rasm.data.tabular.lakehouse import LakeResult
from rasm.data.tabular.materialize import PartitionBundle
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, RuntimeResult, boundary, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import TENANT_BAGGAGE

# --- [TYPES] ----------------------------------------------------------------------------

type Slot = tuple[str, str, str, str]

type Harvested = QueryCensus | LakeResult | EgressResult | PartitionBundle | Mapping[str, object]
type CostInput = CostFact | Harvested

_PLAN_COUNTS: Final[tuple[str, ...]] = (
    "allowed_mem",
    "reserved_mem",
    "npartitions",
    "arity",
    "operations",
    "tasks",
    "peak_mem",
)

_COST_COLUMNS: Final[Block[ColumnSpec["PricedSlot", object]]] = Block.of_seq([
    ColumnSpec(name="content_key", arrow=pa.string(), kind=str, lift=lambda row: row.slot[0]),
    ColumnSpec(name="tenant", arrow=pa.string(), kind=str, lift=lambda row: row.slot[1]),
    ColumnSpec(name="domain", arrow=pa.string(), kind=str, lift=lambda row: row.slot[2]),
    ColumnSpec(name="kind", arrow=pa.string(), kind=str, lift=lambda row: row.slot[3]),
    ColumnSpec(name="rows", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.rows),
    ColumnSpec(name="bytes", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.bytes_moved),
    ColumnSpec(name="seconds", arrow=pa.float64(), kind=float, lift=lambda row: row.fact.seconds),
    ColumnSpec(name="tasks", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.tasks),
    ColumnSpec(name="unpriced", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.unpriced),
    ColumnSpec(name="cost", arrow=pa.float64(), kind=float, lift=lambda row: row.price),
    ColumnSpec(name="currency", arrow=pa.string(), kind=str, lift=lambda row: row.currency),
])
class PricedSlot(Struct, frozen=True):
    slot: Slot
    fact: "CostFact"
    price: float
    currency: str


# --- [ERRORS] ---------------------------------------------------------------------------

def _cost_raises() -> Catch:
    return (pa.ArrowException, OSError)


COST_RATE_POLICY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="rate.policy", arm="config", defect="invalid-policy", retriability=TERMINAL
)
COST_QUANTITY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="rate.amount", arm="config", defect="invalid-quantity", retriability=TERMINAL, slots=("domain",)
)
COST_UNPRICED_UNIT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="rate.unit", arm="config", defect="unpriced-unit", retriability=TERMINAL, slots=("domain", "units")
)
COST_NONFINITE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="rate.total", arm="config", defect="non-finite", retriability=TERMINAL, slots=("scope", "extent")
)
COST_WIRE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="tensor", arm="wire", defect="invalid-plan-wire", retriability=TERMINAL
)
COST_QUANTITIES: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="rate.quantity", arm="boundary", defect="quantity-read", retriability=TERMINAL
)
COST_FRAME: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="frame", arm="boundary", defect="frame-build", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    COST_RATE_POLICY,
    COST_QUANTITY,
    COST_UNPRICED_UNIT,
    COST_NONFINITE,
    COST_WIRE,
    COST_QUANTITIES,
    COST_FRAME,
]))


class CostDomain(StrEnum):
    QUERY = "query"
    LAKEHOUSE = "lakehouse"
    EGRESS = "egress"
    MATERIALIZE = "materialize"
    TENSOR = "tensor"


class CostUnit(StrEnum):
    ROWS = "rows"
    BYTES = "bytes"
    SECONDS = "s"
    TASKS = "tasks"


# --- [MODELS] ---------------------------------------------------------------------------


def _tenant() -> str | None:
    held = baggage.get_baggage(TENANT_BAGGAGE, otel_context.get_current())
    return str(held) if held is not None else None


class CostFact(Struct, frozen=True):
    domain: CostDomain
    kind: str
    key: str
    tenant: str | None
    rows: int = 0
    bytes_moved: int = 0
    seconds: float = 0.0
    tasks: int = 0
    unpriced: int = 0

    @classmethod
    def of(cls, source: CostInput) -> "RuntimeResult[CostFact]":
        tenant = _tenant()
        match source:
            case CostFact() as fact:
                return Ok(fact)
            case QueryCensus() as query:
                volume = query.profile.bind(lambda held: held.bytes_read.option().map2(lambda r, w: r + w, held.bytes_written.option()))
                seconds = query.profile.bind(lambda held: held.latency_s.option())
                return Ok(cls(
                    domain=CostDomain.QUERY,
                    kind=query.engine,
                    key=query.content_key.hex,
                    tenant=tenant,
                    rows=query.row_count,
                    bytes_moved=volume.default_value(0),
                    seconds=seconds.default_value(0.0),
                    unpriced=sum(1 for held in (volume, seconds) if held.is_none()) if query.mode is not ProfileMode.OFF else 0,
                ))
            case LakeResult() as lake:
                return Ok(cls(
                    domain=CostDomain.LAKEHOUSE,
                    kind=lake.operation,
                    key=lake.content_key.hex,
                    tenant=tenant,
                    bytes_moved=lake.byte_length,
                    tasks=lake.files_added.default_value(0) + lake.files_removed.default_value(0),
                ))
            case EgressResult() as egress:
                return Ok(cls(
                    domain=CostDomain.EGRESS,
                    kind=egress.operation,
                    key=egress.content_key.hex if egress.content_key is not None else "",
                    tenant=tenant,
                    bytes_moved=egress.byte_length,
                ))
            case PartitionBundle() as bundle:
                return Ok(cls(domain=CostDomain.MATERIALIZE, kind="cdc", key=bundle.content_key.hex, tenant=tenant, rows=bundle.rows))
            case Mapping() as facts:
                return cls._tensor(facts, tenant)
            case unreachable:
                assert_never(unreachable)

    @classmethod
    def _tensor(cls, facts: Mapping[str, object], tenant: str | None) -> "RuntimeResult[CostFact]":
        op, executor, target = facts.get("op"), facts.get("executor"), facts.get("target")
        malformed = (
            not isinstance(op, str)
            or not op
            or not isinstance(executor, str)
            or not executor
            or not isinstance(target, str)
            or not target
            or any(type(facts.get(name)) is not int or cast(int, facts[name]) < 0 for name in _PLAN_COUNTS)
        )
        return (
            Error(COST_WIRE.raised())
            if malformed
            else Ok(
                cls(
                    domain=CostDomain.TENSOR,
                    kind=executor,
                    key="",
                    tenant=tenant,
                    bytes_moved=cast(int, facts["peak_mem"]),
                    tasks=cast(int, facts["tasks"]),
                )
            )
        )

    @staticmethod
    def combined(left: "CostFact", right: "CostFact") -> "CostFact":
        return CostFact(
            domain=left.domain,
            kind=left.kind,
            key=left.key,
            tenant=left.tenant,
            rows=left.rows + right.rows,
            bytes_moved=left.bytes_moved + right.bytes_moved,
            seconds=left.seconds + right.seconds,
            tasks=left.tasks + right.tasks,
            unpriced=left.unpriced + right.unpriced,
        )


class RatePolicy(Struct, frozen=True):
    currency: str
    rates: Map[tuple[CostDomain, CostUnit], float]

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, currency: str, *rows: tuple[CostDomain, CostUnit, float]) -> "RuntimeResult[RatePolicy]":
        keys = tuple((domain, unit) for domain, unit, _price in rows)
        invalid = (
            not currency
            or currency != currency.strip()
            or not currency.isascii()
            or not currency.isalpha()
            or currency != currency.upper()
            or len(keys) != len(set(keys))
            or any(not isfinite(price) or price < 0.0 for _domain, _unit, price in rows)
        )
        return (
            Error(COST_RATE_POLICY.raised())
            if invalid
            else Ok(cls(currency=currency, rates=Map.of_seq(((domain, unit), price) for domain, unit, price in rows)))
        )

    def price(self, fact: CostFact) -> "RuntimeResult[float]":
        def priced(quantities: tuple[tuple[CostUnit, float], ...]) -> "RuntimeResult[float]":
            if any(not isfinite(amount) or amount < 0.0 for _unit, amount in quantities):
                return Error(COST_QUANTITY.raised(fact.domain.value))
            missing = tuple(unit for unit, amount in quantities if amount != 0.0 and not self.rates.contains_key((fact.domain, unit)))
            if missing:
                return Error(COST_UNPRICED_UNIT.raised(fact.domain.value, ",".join(unit.value for unit in missing)))
            total = sum((self.rates.get((fact.domain, unit)) or 0.0) * amount for unit, amount in quantities)
            return Ok(total) if isfinite(total) else Error(COST_NONFINITE.raised(fact.domain.value, "1"))

        return boundary(
            COST_QUANTITIES,
            lambda: (
                (CostUnit.ROWS, float(fact.rows)),
                (CostUnit.BYTES, float(fact.bytes_moved)),
                (CostUnit.SECONDS, fact.seconds),
                (CostUnit.TASKS, float(fact.tasks)),
            ),
            catch=_cost_raises(),
        ).bind(priced)


class CostFrame(Struct, frozen=True):
    frame: pa.Table
    slots: int
    tenants: int
    total: float
    unpriced: int
    currency: str
    content_key: ContentKey


# --- [SERVICES] -------------------------------------------------------------------------


class CostLedger(Struct, frozen=True):
    facts: tuple[CostFact, ...]

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, sources: Iterable[CostInput]) -> "RuntimeResult[CostLedger]":
        return traverse(CostFact.of, Block.of_seq(sources)).map(lambda facts: cls(tuple(facts)))

    def frame(self, policy: RatePolicy) -> "RuntimeResult[CostFrame]":
        return traverse(
            lambda row: policy.price(row[1]).map(lambda price: PricedSlot(slot=row[0], fact=row[1], price=price, currency=policy.currency)),
            Block.of_seq(self._grouped().to_seq()),
        ).bind(lambda rows: self._frame(policy, Block.of_seq(rows)))

    def _frame(self, policy: RatePolicy, priced: "Block[PricedSlot]") -> "RuntimeResult[CostFrame]":
        def materialized() -> tuple[pa.Table, bytes]:
            table = column_frame(_COST_COLUMNS, priced)
            return table, arrow_bytes(table)

        total = sum(row.price for row in priced)
        if not isfinite(total):
            return Error(COST_NONFINITE.raised("aggregate", str(len(priced))))
        tenants = len({row.slot[1] for row in priced if row.slot[1]})
        unpriced = sum(row.fact.unpriced for row in priced)
        return boundary(COST_FRAME, materialized, catch=_cost_raises()).bind(
            lambda held: ContentIdentity.of("cost", held[1]).map(
                lambda key: CostFrame(
                    frame=held[0],
                    slots=len(priced),
                    tenants=tenants,
                    total=total,
                    unpriced=unpriced,
                    currency=policy.currency,
                    content_key=key,
                )
            )
        )

    def _grouped(self) -> "Map[Slot, CostFact]":
        def folded(acc: "Map[Slot, CostFact]", fact: CostFact) -> "Map[Slot, CostFact]":
            slot: Slot = (fact.key, fact.tenant or "", fact.domain.value, fact.kind)
            return acc.change(slot, lambda held: Some(held.map(lambda prior: CostFact.combined(prior, fact)).default_value(fact)))

        return Block.of_seq(self.facts).fold(folded, Map.empty())
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
