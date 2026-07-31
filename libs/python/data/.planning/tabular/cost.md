# [PY_DATA_COST]

Dataset cost-ledger owner folds emitted receipts into per-key, per-tenant cost facts priced by a caller-supplied rate policy. `CostFact.of` is one polymorphic harvest discriminating the receipt shape — `QueryReceipt` rows and profile seconds, `LakeReceipt` commit file churn and stored bytes, `EgressReceipt` byte volume, `PartitionBundle` recompute rows, `ReceiptFact` durable evidence — and `CostLedger.frame` group-folds the facts into one content-keyed priced Arrow frame naming no downstream owner. Journal and evidence rows stay the billing truth; the ledger is strictly their projection, so it records no metric, opens no span, and mints no second metering pipeline.

Live harvest and durable reconstruction are ONE stream: `CostLedger.of` expands a `tabular/lakehouse#LAKEHOUSE` residence scan into its priced rows before the same harvest fold runs, so a cost window rebuilt after every producing process is gone differs from a live one in provenance alone — save the host-crossing tensor plane, whose `PlanReceipt` arrives as a wire mapping and never enters the receipt stream, so it reconstructs only where its producing composition re-supplies that wire. Which engine ran that scan — duckdb, daft, datafusion, polars, or the Flight SQL end — never reaches this fold, because every engine lands the same Arrow frame the residence schema pins. That expansion is COLUMNAR: the residence carries no cardinality ceiling by law, so `pc.map_lookup` pulls each priced fact out of the open map as a column and one `group_by` sums it on the slot grain, and the plane's own scale never becomes Python objects.

Tenant attribution splits by source and the split is load-bearing. Live receipts read the `rasm.tenant` baggage entry at harvest through the runtime `TENANT_BAGGAGE` key — the same fold `Metrics.record` applies to measurements — so receipts stay tenant-free on disk. Durable rows price in the tenancy RECORDED when the evidence was produced, never the reconstructing process's, because stamping today's tenancy onto another tenant's history is the exact loss the residence exists to prevent. Rendered content identity is the grain: a keyed fact groups at `(key, tenant, domain, kind)`, and a keyless wire mapping coarsens to the `(domain, kind, tenant)` slot. Apex position closes the tabular strata: cost imports its receipt owners strictly downward and admits the tensor plan only as wire data.

## [01]-[INDEX]

- [02]-[COST]: `CostFact` harvests every receipt family, `CostLedger` group-folds the slots, `RatePolicy` prices them, and one Arrow frame leaves keyed by its own content.

## [02]-[COST]

- Owner: `CostFact` — the one harvested cost row carrying domain, kind, the rendered identity key, tenant, and the four quantity axes (`rows`/`bytes_moved`/`seconds`/`tasks`); `CostDomain` the closed domain vocabulary over the harvested planes; `CostUnit` the quantity-axis vocabulary the rate rows key on; `RatePolicy` the caller-supplied `(domain, unit) -> price` row set with its currency token, never constants; `CostLedger` the group-fold owner; `CostReceipt` the emitted evidence row. One fact shape spans every source — a per-plane fact type is the deleted form.
- Owner: `_DOMAIN` resolves a durable row's plane off the metric-domain string its producer already records, and `_EVIDENCE_AXIS` names which recorded fact feeds each quantity — so reconstruction reads rows, never a per-plane decoder, and a live arm reading a field no row names prices its own history at zero. `CostFact.priced` is the ONE projection from a plane's axis roster onto the four quantity slots, its `read` the only difference between the row-wise durable arm and the columnar residence fold, and `_NUMBER` the one numeric admission grammar both apply — scalar through `_number`, vectorized through `_numeric`.
- Entry: `CostFact.of` discriminates receipt, durable-evidence, wire-mapping, and already-harvested fact shapes under `assert_never`, so every source reaches one railed entry; only the host-crossing tensor mapping runs the complete `PlanReceipt` wire admission fold. `CostLedger.of` expands each source through `_expanded` and traverses the flat stream before `frame(policy)` folds it — `CostFact.combined` the associative quantity monoid, the `Map` slot fold key-sorted so identical fact sets yield one byte-stable frame — and `boundary` captures numeric conversion, Arrow materialization, and canonical `arrow_bytes` egress before the frame's own `ContentKey` enters the `CostReceipt`.
- Auto: the lakehouse arm reads the receipt's own `residence` slot to price an evidence commit on `TELEMETRY` and a caller's table on `LAKEHOUSE` — the SAME split the durable row already records through its residence domain, so a window harvested live and one reconstructed from the residence land on one plane rather than two.
- Auto: the materialize arm keys `kind="cdc"` and never the partition id — unbounded partition cardinality stays receipt-only, the standing metric-dimension law applied to the cost grain; the query arm reads `bytes_moved` and `seconds` off the optional `EngineProfile` band and harvests zero when unprofiled; a keyless fact groups at its coarse slot with `content_key=""` in the frame, never a dropped row.
- Receipt: `CostReceipt.contribute` emits one emitted-phase `Receipt.of("cost-ledger", ("emitted", subject, facts))` row carrying slot count, tenant count, priced total, currency, and the frame `ContentKey`; it records no `Metrics.record` measure because every harvested quantity already projected at its source receipt's own `contribute` — a second recording does double-count the spine.
- Packages: `pyarrow` (the priced frame constructor) with `pyarrow.compute` (`map_lookup`/`fill_null`/`match_substring_regex`/`if_else`/`cast` — the residence fold's whole vectorized half, and `Table.group_by`/`aggregate` for the slot sum), `msgspec` (`Struct` the frozen owners), `expression` (`Block`/`Map`/`Some` the slot fold beside `extra.result.traverse`, the substrate's own fail-fast threader every railed stream here rides), `opentelemetry-api` (`baggage.get_baggage` over the current context — the one tenant read), `beartype` (`@beartype(conf=FAULT_CONF)` on the public `RatePolicy.of`/`CostLedger.of` factories), `tabular/columnar#SCAN` (`QueryReceipt`), `tabular/interop#INTEROP` (`arrow_bytes`, the folder's one whole-table serialization the priced frame keys through), `tabular/lakehouse#LAKEHOUSE` (`LakeReceipt` beside the `ReceiptFact` durable row whose schema the residence owns), `tabular/materialize#MATERIALIZE` (`PartitionBundle`), `tabular/egress#EGRESS` (`EgressReceipt`), runtime (`RuntimeRail`/`FAULT_CONF`/`ContentIdentity`/`ContentKey`/`Receipt`/`TENANT_BAGGAGE`).
- Growth: a new harvested receipt family is one `CostFact.of` arm; a new cost axis is one `CostFact` field, one `CostUnit` member, one `combined` term, and one `CostFact.priced` slot both readers inherit; a new priced plane is one `CostDomain` member with its `_DOMAIN` spelling, its `_EVIDENCE_AXIS` row, and its rate rows, the columnar fold picking it up with zero edits; a new query engine over the residence is zero edits, the frame shape being the whole contract; a new frame column derives inside `frame`; zero new surface.
- Boundary: a projection over receipts, never a second metering pipeline — no `Metrics.record`, no span, no durable store, no currency conversion, and no scan of its own: the residence frame arrives from the caller that ran it, so this owner never picks an engine or opens a lakehouse; rates arrive as policy rows, never module constants; the gridded `PlanReceipt` crosses as wire data (its `to_builtins` lowering), never an upward `rasm.data.gridded` import; a tenant field on any source receipt, a per-plane fact type, a partition-id cost dimension, and a hand-rolled hash over the priced frame where `arrow_bytes` with `ContentIdentity` own identity are the deleted forms.
- Boundary: the priced frame is the TERMINAL egress and advertises no reader: `python:artifacts/visualization/table#TABLE` `TablePlan.of` admits it as the settled Arrow-capsule frame it is, so naming a renderer here claims a seam neither end carries.
- Boundary: rates and totals ride `float` and no settlement reads this frame — observed spend is a dashboard reading, while the trapped exact-decimal arithmetic a settled charge demands homes at the runtime journal's rating fold and never at this projection.

```python signature
from collections.abc import Callable, Iterable, Mapping
from enum import StrEnum
from math import isfinite
from re import fullmatch
from typing import Any, Final, assert_never, cast

import pyarrow as pa
import pyarrow.compute as pc
from beartype import beartype
from expression import Error, Ok, Some
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.data.tabular.columnar import QueryReceipt
from rasm.data.tabular.egress import EgressReceipt
from rasm.data.tabular.interop import arrow_bytes
from rasm.data.tabular.lakehouse import LakeReceipt, ReceiptFact
from rasm.data.tabular.materialize import PartitionBundle
from rasm.runtime.faults import BoundaryFault, FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import TENANT_BAGGAGE
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

# Grouping slot is (content_key hex | "", tenant | "", domain, kind), content-key-first and coarse for keyless facts.
type Slot = tuple[str, str, str, str]
# same slot spelled as residence columns, so the columnar fold groups on the SAME grain the live fold groups on.
_SLOT_COLUMNS: Final[tuple[str, ...]] = ("content_key", "tenant", "domain", "kind")
# ONE numeric admission grammar, applied scalar by `_number` and vectorized by `_numeric`. Decimal and NON-NEGATIVE:
# a residence stores every fact as its own string rendering, and the two spellings this grammar excludes are exactly
# the ones the rate gate refuses one fold later — `float()` would admit `inf`/`nan` and a leading sign would admit a
# negative quantity, each stranding the WHOLE window on a gate the reconstruction cannot answer. Every axis this plane
# prices is a count, a byte volume, or an elapsed span, so a signed rendering is a producer defect reading zero here
# rather than a window loss thousands of rows wide.
_NUMBER: Final[str] = r"\d+(\.\d+)?"
type Harvested = QueryReceipt | LakeReceipt | EgressReceipt | PartitionBundle | ReceiptFact | Mapping[str, object]
type CostInput = CostFact | Harvested
# a residence scan lands ONE Arrow frame whatever engine ran it, so the ledger admits the frame beside the live
# receipts and reconstructs a cost window the producing processes no longer hold.
type CostSource = CostInput | pa.Table

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
    # evidence planes price their OWN storage: a residence commit moves files and holds bytes exactly as any other
    # plane does, and a telemetry estate whose own cost is invisible is the one plane nobody can budget.
    TELEMETRY = "telemetry"


class CostUnit(StrEnum):
    ROWS = "rows"
    BYTES = "bytes"
    SECONDS = "s"
    TASKS = "tasks"


# --- [TABLES] ---------------------------------------------------------------------------

# `_DOMAIN` maps the metric-domain spelling each contributor hands `Metrics.record` onto the plane it prices — the
# residence stores that exact string, so a durable row resolves its plane with no second discriminant. Domains this
# table does not row are evidence the cost plane deliberately leaves unpriced, refused by name rather than folded into
# a neighbouring plane at whatever rate that plane happens to carry.
_DOMAIN: Final[Map[str, CostDomain]] = Map.of_seq([
    ("query", CostDomain.QUERY),
    ("lake", CostDomain.LAKEHOUSE),
    ("egress", CostDomain.EGRESS),
    ("materialize", CostDomain.MATERIALIZE),
    ("telemetry", CostDomain.TELEMETRY),
])

# which recorded fact each plane's quantity axis reads out of a durable row. A row here names the SAME quantity the
# live arm reads off its typed receipt, so a fact and its durable twin price identically; a live arm reading a field
# no row names prices the reconstruction at zero and reports a cheaper past than the present it mirrors. Repeated
# units accumulate, which is how the commit's two file counters land as one task total.
_EVIDENCE_AXIS: Final[Map[CostDomain, tuple[tuple[CostUnit, str], ...]]] = Map.of_seq([
    (CostDomain.QUERY, ((CostUnit.ROWS, "rows"), (CostUnit.BYTES, "bytes"), (CostUnit.SECONDS, "latency_s"))),
    (CostDomain.LAKEHOUSE, ((CostUnit.TASKS, "added"), (CostUnit.TASKS, "removed"), (CostUnit.BYTES, "bytes"))),
    (CostDomain.EGRESS, ((CostUnit.BYTES, "bytes"),)),
    (CostDomain.MATERIALIZE, ((CostUnit.ROWS, "rows"),)),
    # this plane prices on the SAME two axes its commits report: file churn is the write work and bytes are storage
    # held, so a residence whose commits are counted but never sized budgets on a number that stops moving as soon
    # as compaction settles.
    (CostDomain.TELEMETRY, ((CostUnit.TASKS, "added"), (CostUnit.TASKS, "removed"), (CostUnit.BYTES, "bytes"))),
])


# --- [MODELS] ---------------------------------------------------------------------------


def _tenant() -> str | None:
    # harvest-time tenancy: the same rasm.tenant baggage entry Metrics.record folds onto measurements — receipts
    # stay tenant-free on disk, and the fact stamps the tenancy active when the evidence is harvested.
    held = baggage.get_baggage(TENANT_BAGGAGE, otel_context.get_current())
    return str(held) if held is not None else None


class CostFact(Struct, frozen=True):
    domain: CostDomain
    kind: str
    # `key` carries the RENDERED identity the slot groups on, never the typed `ContentKey`: this ledger stores nothing
    # keyed by that value and a durable evidence row carries the rendered form alone, so holding the typed key here
    # would make a reconstructed fact ungroupable beside the live fact it mirrors.
    key: str
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
            case ReceiptFact() as evidence:
                # this residence-sourced arm discards the harvest-time tenancy above: a durable row prices in the
                # tenancy RECORDED when the evidence was produced, and stamping the reconstructing process's
                # tenancy onto another tenant's history is the exact loss this residence exists to prevent.
                return cls._evidence(evidence)
            case QueryReceipt() as query:
                profile = query.profile
                return Ok(cls(
                    domain=CostDomain.QUERY,
                    kind=query.engine,
                    key=query.content_key.hex,
                    tenant=tenant,
                    rows=query.row_count,
                    bytes_moved=(profile.bytes_read + profile.bytes_written) if profile is not None else 0,
                    seconds=profile.latency_s if profile is not None else 0.0,
                ))
            case LakeReceipt() as lake:
                # a residence commit prices on the residence's OWN plane, so the evidence tail is budgetable apart
                # from the caller tables sharing this owner; both read the identical two axes, which is what keeps a
                # durable row and its live twin priced the same after the producing process is gone.
                return Ok(cls(
                    domain=CostDomain.TELEMETRY if lake.residence is not None else CostDomain.LAKEHOUSE,
                    kind=lake.operation,
                    key=lake.content_key.hex,
                    tenant=tenant,
                    bytes_moved=lake.byte_length,
                    tasks=lake.files_added + lake.files_removed,
                ))
            case EgressReceipt() as egress:
                return Ok(cls(
                    domain=CostDomain.EGRESS,
                    kind=egress.operation,
                    key=egress.content_key.hex if egress.content_key is not None else "",
                    tenant=tenant,
                    bytes_moved=egress.byte_length,
                ))
            case PartitionBundle() as bundle:
                # kind stays the constant "cdc": the unbounded partition id is receipt-only, never a cost dimension.
                return Ok(cls(domain=CostDomain.MATERIALIZE, kind="cdc", key=bundle.content_key.hex, tenant=tenant, rows=bundle.rows))
            case Mapping() as facts:
                return cls._tensor(facts, tenant)
            case unreachable:
                assert_never(unreachable)

    @classmethod
    def priced(cls, domain: CostDomain, kind: str, key: str, tenant: str, read: Callable[[str], float]) -> "CostFact":
        # ONE projection from a plane's axis roster onto the four quantity slots, read by the row-wise durable arm and
        # by the columnar residence fold alike — `read` is their only difference, a map lookup on one side and an
        # already-summed column on the other, so a fifth unit cannot drift two spellings apart. Repeated units
        # accumulate, which is how a commit's two file counters land as one task total.
        axes = _EVIDENCE_AXIS.get(domain) or ()
        totals = {unit: sum(read(name) for candidate, name in axes if candidate is unit) for unit, _name in axes}
        return cls(
            domain=domain,
            kind=kind,
            key=key,
            tenant=tenant or None,
            rows=int(totals.get(CostUnit.ROWS, 0.0)),
            bytes_moved=int(totals.get(CostUnit.BYTES, 0.0)),
            seconds=totals.get(CostUnit.SECONDS, 0.0),
            tasks=int(totals.get(CostUnit.TASKS, 0.0)),
        )

    @classmethod
    def _evidence(cls, row: ReceiptFact) -> "RuntimeRail[CostFact]":
        # `_evidence` IS the durable half of the one harvest: `_DOMAIN` resolves the plane off the recorded metric-domain
        # string and `_EVIDENCE_AXIS` names which recorded fact feeds each quantity, so reconstruction reads rows rather
        # than a per-plane decoder. Non-numeric and absent facts contribute zero — the residence keeps every dimension
        # a producer wrote, and a fact this plane does not price is not a malformed row.
        return (
            _DOMAIN.try_find(row.domain)
            .map(lambda domain: Ok(cls.priced(domain, row.kind, row.content_key, row.tenant, lambda name: _number(row.facts.get(name)))))
            .default_with(lambda: Error(BoundaryFault(boundary=("cost.evidence", f"unpriced domain {row.domain}"))))
        )

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
                    key="",
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
            key=left.key,
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
            # the rate table IS an `expression.collections.Map` carrying `contains_key`/`get`, so both reads go
            # straight to it: a `dict(self.rates.items())` materialization rebuilt the WHOLE table once per priced
            # slot, inside the very fold whose sibling comment names per-element respread as the quadratic cost to
            # avoid. A `get` miss answers `None`, so the unpriced-unit gate above is what makes the `or 0.0` tail
            # reachable only on a zero quantity.
            if any(not isfinite(amount) or amount < 0.0 for _unit, amount in quantities):
                return Error(BoundaryFault(boundary=("cost.rate", f"invalid quantity for {fact.domain}")))
            missing = tuple(unit for unit, amount in quantities if amount != 0.0 and not self.rates.contains_key((fact.domain, unit)))
            if missing:
                return Error(BoundaryFault(boundary=("cost.rate", f"unpriced {fact.domain}: {','.join(unit.value for unit in missing)}")))
            total = sum((self.rates.get((fact.domain, unit)) or 0.0) * amount for unit, amount in quantities)
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
        # so the ledger emits evidence alone and never double-counts the metric spine. Its `domain` is deliberately
        # absent from `_DOMAIN`, so a residence scan carrying this row drops it rather than pricing the pricing —
        # a priced ledger emission compounds every window it appears in against itself.
        return (
            Receipt.of(
                "cost-ledger",
                (
                    "emitted",
                    f"cost[{self.currency}]",
                    {
                        "domain": "cost",
                        "kind": self.currency,
                        "key": self.content_key.hex,
                        "slots": self.slots,
                        "tenants": self.tenants,
                        "total": self.total,
                    },
                ),
            ),
        )


# --- [SERVICES] -------------------------------------------------------------------------


class CostLedger(Struct, frozen=True):
    facts: tuple[CostFact, ...]

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, sources: Iterable[CostSource]) -> "RuntimeRail[CostLedger]":
        # TWO traversals, not one: every source expands to its inputs first, `collect` flattens the per-source blocks
        # with no intermediate nesting, then one harvest runs over the flat stream — so a residence scan and a live
        # receipt enter the same ledger and a cost window reconstructed from the lake differs from one harvested live
        # in provenance alone. Which engine ran the scan never reaches this fold. `traverse` is the substrate's own
        # fail-fast threader, so the first refusal short-circuits the whole stream and the accumulator stays the
        # persistent block rather than a tuple respread per element, which prices a large window quadratically.
        return traverse(_expanded, Block.of_seq(sources)).bind(
            lambda expanded: traverse(CostFact.of, expanded.collect(Block.of_seq)).map(lambda facts: cls(tuple(facts)))
        )

    def frame(self, policy: RatePolicy) -> "RuntimeRail[tuple[pa.Table, CostReceipt]]":
        # Group-fold Map iteration is key-sorted, so identical fact sets yield one byte-stable frame and the
        # railed ContentIdentity over the canonical arrow_bytes keys the priced frame itself. The same substrate
        # threader `of` rides prices every slot, so an unpriced axis short-circuits the frame with no slot respread.
        return traverse(
            lambda row: policy.price(row[1]).map(lambda price: (row[0], row[1], price)),
            Block.of_seq(self._grouped().to_seq()),
        ).bind(lambda rows: self._frame(policy, tuple(rows)))

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
            slot: Slot = (fact.key, fact.tenant or "", fact.domain.value, fact.kind)
            return acc.change(slot, lambda held: Some(held.map(lambda prior: CostFact.combined(prior, fact)).default_value(fact)))

        return Block.of_seq(self.facts).fold(folded, Map.empty())


def _expanded(source: CostSource) -> "RuntimeRail[tuple[CostInput, ...]]":
    # a residence scan expands COLUMN-WISE into already-grouped facts. That plane carries no cardinality ceiling BY LAW,
    # so decoding it row-wise allocates one Python object per evidence row to produce a handful of priced slots — the
    # exact cost the residence exists to avoid. A plane `_DOMAIN` does not row is evidence this ledger deliberately
    # leaves unpriced and never enters the fold, while a `ReceiptFact` handed in directly still refuses by name so a
    # caller never silently loses a row it named.
    match source:
        case pa.Table() as frame:
            return boundary("cost.residence", lambda: _residence(frame))
        case _:
            return Ok((source,))


def _residence(frame: pa.Table) -> tuple[CostInput, ...]:
    # ONE columnar pass per priced plane: `_DOMAIN` selects the rows a plane owns and `_planed` sums that plane's axes
    # on the SAME `(key, tenant, domain, kind)` slot the live fold groups on, so a reconstruction and a live harvest
    # land identical facts. `receipt_facts` stays the residence's row decoder for a consumer wanting the rows themselves.
    return tuple(fact for recorded, domain in _DOMAIN.items() for fact in _planed(frame.filter(pc.equal(frame.column("domain"), recorded)), domain))


def _planed(rows: pa.Table, domain: CostDomain) -> tuple[CostFact, ...]:
    # each axis pulls its fact out of the open map as a COLUMN through `pc.map_lookup`, so the residence's scale never
    # reaches Python: one grouped row per slot leaves this fold, not one object per evidence row. Column names are the
    # fact names themselves, which the slot roster cannot collide with, so the summed column reads back by name.
    axes = _EVIDENCE_AXIS.get(domain) or ()
    if not rows.num_rows or not axes:
        return ()
    quantities = {name: _numeric(pc.map_lookup(rows.column("facts"), query_key=name, occurrence="first")) for _unit, name in axes}
    grouped = pa.table({column: rows.column(column) for column in _SLOT_COLUMNS} | quantities).group_by(list(_SLOT_COLUMNS)).aggregate(
        [(name, "sum") for name in quantities]
    )
    return tuple(_slot_fact(row, domain) for row in grouped.to_pylist())


def _slot_fact(row: Mapping[str, Any], domain: CostDomain) -> CostFact:
    # one grouped slot read back through the SAME `CostFact.priced` projection the durable row-wise arm reads, its
    # `read` closing over the summed column rather than the open map.
    return CostFact.priced(domain, row["kind"], row["content_key"], row["tenant"], lambda name: float(row[f"{name}_sum"] or 0.0))


def _numeric(held: Any) -> Any:
    # vectorized twin of `_number` over the SAME grammar: a value this plane cannot price — prose, non-finite, or
    # signed — reads zero exactly as the row-wise arm reads it, never a cast raising over one fact and losing the
    # whole window with it.
    filled = pc.fill_null(held, "0")
    return pc.cast(pc.if_else(pc.match_substring_regex(filled, rf"^{_NUMBER}$"), filled, "0"), pa.float64())


def _number(held: str | None) -> float:
    # a recorded fact arrives as the residence's string rendering, so a value outside `_NUMBER` — a prose fact, a
    # non-finite spelling, or a signed quantity no priced axis admits — contributes zero rather than aborting a window
    # over thousands of rows for one row's fact.
    return float(held) if held and fullmatch(_NUMBER, held) else 0.0
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
