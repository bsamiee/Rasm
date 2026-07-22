# [PY_DATA_COST]

Dataset cost-ledger owner folds emitted receipts into per-`ContentKey`, per-tenant cost facts priced by a caller-supplied rate policy. `CostFact.of` is one polymorphic harvest discriminating the receipt shape — `QueryReceipt` rows and profile seconds, `LakeReceipt` commit file counts, `EgressReceipt` byte volume, `PartitionBundle` recompute rows — and `CostLedger.frame` group-folds the facts into one priced Arrow frame the artifacts renderer and the estate boards consume. Journal and evidence rows stay the billing truth; the ledger is strictly their projection, so it records no metric, opens no span, and mints no second metering pipeline.

Tenant attribution reads the `rasm.tenant` baggage entry at harvest through the runtime `TENANT_BAGGAGE` key — the same fold `Metrics.record` applies to measurements — so receipts stay tenant-free on disk and the fact stamps the tenancy active when the evidence is harvested. Content identity is the grain: a keyed fact groups at `(content_key, tenant, domain, kind)`, and a keyless wire mapping coarsens to the `(domain, kind, tenant)` slot. Apex position closes the tabular strata: cost imports its receipt owners strictly downward and admits the tensor plan only as wire data.

## [01]-[INDEX]

- [01]-[COST]: the receipt-derived cost fact, the slot-grouped ledger fold, the rate policy, and the priced Arrow frame egress.

## [02]-[COST]

- Owner: `CostFact` — the one harvested cost row carrying domain, kind, optional `ContentKey`, harvest-time tenant, and the four quantity axes (`rows`/`bytes_moved`/`seconds`/`tasks`); `CostDomain` the closed domain vocabulary over the harvested planes; `CostUnit` the quantity-axis vocabulary the rate rows key on; `RatePolicy` the caller-supplied `(domain, unit) -> price` row set with its currency token, never constants; `CostLedger` the group-fold owner; `CostReceipt` the emitted evidence row. One fact shape spans every source — a per-plane fact type is the deleted form.
- Entry: `CostFact.of` discriminates receipt, wire-mapping, and already-harvested fact shapes under `assert_never`, so every source reaches one railed entry; only the host-crossing tensor mapping runs the complete `PlanReceipt` wire admission fold. `CostLedger.of` traverses that mixed stream before `frame(policy)` folds it — `CostFact.combined` the associative quantity monoid, the `Map` slot fold key-sorted so identical fact sets yield one byte-stable frame — and `boundary` captures numeric conversion, Arrow materialization, and canonical `arrow_bytes` egress before the frame's own `ContentKey` enters the `CostReceipt`.
- Auto: the materialize arm keys `kind="cdc"` and never the partition id — unbounded partition cardinality stays receipt-only, the standing metric-dimension law applied to the cost grain; the query arm reads `bytes_moved` and `seconds` off the optional `EngineProfile` band and harvests zero when unprofiled; a keyless fact groups at its coarse slot with `content_key=""` in the frame, never a dropped row.
- Receipt: `CostReceipt.contribute` emits one emitted-phase `Receipt.of("cost-ledger", ("emitted", subject, facts))` row carrying slot count, tenant count, priced total, currency, and the frame `ContentKey`; it records no `Metrics.record` measure because every harvested quantity already projected at its source receipt's own `contribute` — a second recording does double-count the spine.
- Packages: `pyarrow` (the priced frame constructor), `msgspec` (`Struct` the frozen owners), `expression` (`Block`/`Map`/`Some` the slot fold), `opentelemetry-api` (`baggage.get_baggage` over the current context — the one tenant read), `beartype` (`@beartype(conf=FAULT_CONF)` on the public `RatePolicy.of`/`CostLedger.of` factories), `tabular/columnar#SCAN` (`QueryReceipt`/`arrow_bytes`), `tabular/lakehouse#LAKEHOUSE` (`LakeReceipt`), `tabular/materialize#MATERIALIZE` (`PartitionBundle`), `tabular/egress#EGRESS` (`EgressReceipt`), runtime (`RuntimeRail`/`FAULT_CONF`/`ContentIdentity`/`ContentKey`/`Receipt`/`TENANT_BAGGAGE`).
- Growth: a new harvested receipt family is one `CostFact.of` arm; a new cost axis is one `CostFact` field, one `CostUnit` member, and one `combined` term; a new priced plane is one `CostDomain` member and its rate rows; a new frame column derives inside `frame`; zero new surface.
- Boundary: a projection over receipts, never a second metering pipeline — no `Metrics.record`, no span, no durable store, no currency conversion; rates arrive as policy rows, never module constants; the gridded `PlanReceipt` crosses as wire data (its `to_builtins` lowering), never an upward `rasm.data.gridded` import; a tenant field on any source receipt, a per-plane fact type, a partition-id cost dimension, and a hand-rolled hash over the priced frame where `arrow_bytes` with `ContentIdentity` own identity are the deleted forms.

```python signature
from collections.abc import Iterable, Mapping
from enum import StrEnum
from math import isfinite
from typing import Final, assert_never, cast

import pyarrow as pa
from beartype import beartype
from expression import Error, Ok, Some
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.data.tabular.columnar import QueryReceipt, arrow_bytes
from rasm.data.tabular.egress import EgressReceipt
from rasm.data.tabular.lakehouse import LakeReceipt
from rasm.data.tabular.materialize import PartitionBundle
from rasm.runtime.faults import BoundaryFault, FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import TENANT_BAGGAGE
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

# Grouping slot is (content_key hex | "", tenant | "", domain, kind), content-key-first and coarse for keyless facts.
type Slot = tuple[str, str, str, str]
type Harvested = QueryReceipt | LakeReceipt | EgressReceipt | PartitionBundle | Mapping[str, object]
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

_COST_SCHEMA: Final[pa.Schema] = pa.schema([
    pa.field("content_key", pa.string(), nullable=False),
    pa.field("tenant", pa.string(), nullable=False),
    pa.field("domain", pa.string(), nullable=False),
    pa.field("kind", pa.string(), nullable=False),
    pa.field("rows", pa.int64(), nullable=False),
    pa.field("bytes", pa.int64(), nullable=False),
    pa.field("seconds", pa.float64(), nullable=False),
    pa.field("tasks", pa.int64(), nullable=False),
    pa.field("cost", pa.float64(), nullable=False),
    pa.field("currency", pa.string(), nullable=False),
])


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
    # harvest-time tenancy: the same rasm.tenant baggage entry Metrics.record folds onto measurements — receipts
    # stay tenant-free on disk, and the fact stamps the tenancy active when the evidence is harvested.
    held = baggage.get_baggage(TENANT_BAGGAGE, otel_context.get_current())
    return str(held) if held is not None else None


class CostFact(Struct, frozen=True):
    domain: CostDomain
    kind: str
    content_key: ContentKey | None
    tenant: str | None
    rows: int = 0
    bytes_moved: int = 0
    seconds: float = 0.0
    tasks: int = 0

    @classmethod
    def of(cls, receipt: CostInput) -> "RuntimeRail[CostFact]":
        # ONE polymorphic harvest over the receipt shape — a new family is one arm; the query arm reads volume and
        # time off the optional profile band and harvests zero when unprofiled, never a guessed quantity.
        tenant = _tenant()
        match receipt:
            case CostFact() as fact:
                return Ok(fact)
            case QueryReceipt() as query:
                profile = query.profile
                return Ok(cls(
                    domain=CostDomain.QUERY,
                    kind=query.engine,
                    content_key=query.content_key,
                    tenant=tenant,
                    rows=query.row_count,
                    bytes_moved=(profile.bytes_read + profile.bytes_written) if profile is not None else 0,
                    seconds=profile.latency_s if profile is not None else 0.0,
                ))
            case LakeReceipt() as lake:
                return Ok(cls(
                    domain=CostDomain.LAKEHOUSE,
                    kind=lake.operation,
                    content_key=lake.content_key,
                    tenant=tenant,
                    tasks=lake.files_added + lake.files_removed,
                ))
            case EgressReceipt() as egress:
                return Ok(cls(
                    domain=CostDomain.EGRESS, kind=egress.operation, content_key=egress.content_key, tenant=tenant, bytes_moved=egress.byte_length
                ))
            case PartitionBundle() as bundle:
                # kind stays the constant "cdc": the unbounded partition id is receipt-only, never a cost dimension.
                return Ok(cls(domain=CostDomain.MATERIALIZE, kind="cdc", content_key=bundle.content_key, tenant=tenant, rows=bundle.rows))
            case Mapping() as facts:
                return cls._tensor(facts, tenant)
            case unreachable:
                assert_never(unreachable)

    @classmethod
    def _tensor(cls, facts: Mapping[str, object], tenant: str | None) -> "RuntimeRail[CostFact]":
        # `op` is the PlanReceipt wire discriminator; the complete scalar roster admits before any projection.
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
            Error(BoundaryFault(boundary=("cost.tensor", "invalid PlanReceipt wire")))
            if malformed
            else Ok(
                cls(
                    domain=CostDomain.TENSOR,
                    kind=executor,
                    content_key=None,
                    tenant=tenant,
                    bytes_moved=cast(int, facts["peak_mem"]),
                    tasks=cast(int, facts["tasks"]),
                )
            )
        )

    @staticmethod
    def combined(left: "CostFact", right: "CostFact") -> "CostFact":
        # Associative quantity monoid reduces the slot fold; identity slots agree by construction.
        return CostFact(
            domain=left.domain,
            kind=left.kind,
            content_key=left.content_key,
            tenant=left.tenant,
            rows=left.rows + right.rows,
            bytes_moved=left.bytes_moved + right.bytes_moved,
            seconds=left.seconds + right.seconds,
            tasks=left.tasks + right.tasks,
        )


class RatePolicy(Struct, frozen=True):
    currency: str
    rates: Map[tuple[CostDomain, CostUnit], float]

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, currency: str, *rows: tuple[CostDomain, CostUnit, float]) -> "RuntimeRail[RatePolicy]":
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
            Error(BoundaryFault(boundary=("cost.rate", "invalid currency or rate row")))
            if invalid
            else Ok(cls(currency=currency, rates=Map.of_seq(((domain, unit), price) for domain, unit, price in rows)))
        )

    def price(self, fact: CostFact) -> "RuntimeRail[float]":
        def priced(quantities: tuple[tuple[CostUnit, float], ...]) -> "RuntimeRail[float]":
            rates = dict(self.rates.items())
            if any(not isfinite(amount) or amount < 0.0 for _unit, amount in quantities):
                return Error(BoundaryFault(boundary=("cost.rate", f"invalid quantity for {fact.domain}")))
            missing = tuple(unit for unit, amount in quantities if amount != 0.0 and (fact.domain, unit) not in rates)
            if missing:
                return Error(BoundaryFault(boundary=("cost.rate", f"unpriced {fact.domain}: {','.join(unit.value for unit in missing)}")))
            total = sum(rates.get((fact.domain, unit), 0.0) * amount for unit, amount in quantities)
            return Ok(total) if isfinite(total) else Error(BoundaryFault(boundary=("cost.rate", f"non-finite total for {fact.domain}")))

        return boundary(
            "cost.rate.quantity",
            lambda: (
                (CostUnit.ROWS, float(fact.rows)),
                (CostUnit.BYTES, float(fact.bytes_moved)),
                (CostUnit.SECONDS, fact.seconds),
                (CostUnit.TASKS, float(fact.tasks)),
            ),
        ).bind(priced)


class CostReceipt(Struct, frozen=True):
    slots: int
    tenants: int
    total: float
    currency: str
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        # no Metrics.record here: every harvested quantity already projected at its source receipt's own contribute,
        # so the ledger emits evidence alone and never double-counts the metric spine.
        return (
            Receipt.of(
                "cost-ledger",
                ("emitted", f"cost[{self.currency}]", {"slots": self.slots, "tenants": self.tenants, "total": self.total, "key": self.content_key.hex}),
            ),
        )


# --- [SERVICES] -------------------------------------------------------------------------


class CostLedger(Struct, frozen=True):
    facts: tuple[CostFact, ...]

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, facts: Iterable[CostInput]) -> "RuntimeRail[CostLedger]":
        return Block.of_seq(facts).fold(
            lambda admitted, source: admitted.bind(
                lambda rows: CostFact.of(source).map(lambda fact: (*rows, fact))
            ),
            Ok(()),
        ).map(cls)

    def frame(self, policy: RatePolicy) -> "RuntimeRail[tuple[pa.Table, CostReceipt]]":
        # Group-fold Map iteration is key-sorted, so identical fact sets yield one byte-stable frame and the
        # railed ContentIdentity over the canonical arrow_bytes keys the priced frame itself.
        grouped = tuple(self._grouped().to_seq())
        priced = Block.of_seq(grouped).fold(
            lambda admitted, row: admitted.bind(
                lambda rows: policy.price(row[1]).map(lambda price: (*rows, (row[0], row[1], price)))
            ),
            Ok(()),
        )
        return priced.bind(lambda rows: self._frame(policy, rows))

    def _frame(
        self, policy: RatePolicy, priced: tuple[tuple[Slot, CostFact, float], ...]
    ) -> "RuntimeRail[tuple[pa.Table, CostReceipt]]":
        def materialized() -> tuple[pa.Table, bytes]:
            table = pa.table({
                "content_key": [slot[0] for slot, _, _ in priced],
                "tenant": [slot[1] for slot, _, _ in priced],
                "domain": [slot[2] for slot, _, _ in priced],
                "kind": [slot[3] for slot, _, _ in priced],
                "rows": [fact.rows for _, fact, _ in priced],
                "bytes": [fact.bytes_moved for _, fact, _ in priced],
                "seconds": [fact.seconds for _, fact, _ in priced],
                "tasks": [fact.tasks for _, fact, _ in priced],
                "cost": [price for _, _, price in priced],
                "currency": [policy.currency] * len(priced),
            }, schema=_COST_SCHEMA)
            return table, arrow_bytes(table)

        # per-row prices are finite by the rate gate, yet their sum can still overflow to inf; the aggregate re-proves
        # finiteness on the same rail, so an invalid CostReceipt is unconstructible.
        total = sum(price for _, _, price in priced)
        if not isfinite(total):
            return Error(BoundaryFault(boundary=("cost.frame", f"non-finite aggregate total over {len(priced)} slots")))
        tenants = len({slot[1] for slot, _, _ in priced if slot[1]})
        return boundary("cost.frame", materialized).bind(
            lambda held: ContentIdentity.of("cost", held[1]).map(
                lambda key: (held[0], CostReceipt(slots=len(priced), tenants=tenants, total=total, currency=policy.currency, content_key=key))
            )
        )

    def _grouped(self) -> "Map[Slot, CostFact]":
        def folded(acc: "Map[Slot, CostFact]", fact: CostFact) -> "Map[Slot, CostFact]":
            slot: Slot = (fact.content_key.hex if fact.content_key is not None else "", fact.tenant or "", fact.domain.value, fact.kind)
            return acc.change(slot, lambda held: Some(held.map(lambda prior: CostFact.combined(prior, fact)).default_value(fact)))

        return Block.of_seq(self.facts).fold(folded, Map.empty())
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
