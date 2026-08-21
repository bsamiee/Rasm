# [PY_DATA_COST]

Dataset cost-ledger owner folds emitted receipts into per-key, per-tenant cost facts priced by a caller-supplied rate policy. `CostFact.of` is one polymorphic harvest discriminating the receipt shape — `QueryReceipt` rows and profile seconds, `LakeReceipt` commit file churn and stored bytes, `EgressReceipt` byte volume, `PartitionBundle` recompute rows, `ReceiptFact` durable evidence — and `CostLedger.frame` group-folds the facts into one content-keyed priced Arrow frame naming no downstream owner. Journal and evidence rows stay the billing truth; the ledger is strictly their projection, so it records no metric, opens no span, and mints no second metering pipeline.

Live harvest and durable reconstruction are ONE stream: `CostLedger.of` expands a `tabular/lakehouse#LAKEHOUSE` residence scan into its priced rows before the same harvest fold runs, so a cost window rebuilt after every producing process is gone differs from a live one in provenance alone — save the host-crossing tensor plane, whose `PlanReceipt` arrives as a wire mapping and never enters the receipt stream, so it reconstructs only where its producing composition re-supplies that wire. Which engine ran that scan — duckdb, daft, datafusion, polars, or the Flight SQL end — never reaches this fold, because every engine lands the same Arrow frame the residence schema pins. That expansion is COLUMNAR: the residence carries no cardinality ceiling by law, so `pc.map_lookup` pulls each priced fact out of the open map as a column and one `group_by` sums it on the slot grain, and the plane's own scale never becomes Python objects.

Tenant attribution splits by source and the split is load-bearing. Live receipts read the `rasm.tenant` baggage entry at harvest through the runtime `TENANT_BAGGAGE` key — the same fold `Metrics.record` applies to measurements — so receipts stay tenant-free on disk. Durable rows price in the tenancy RECORDED when the evidence was produced, never the reconstructing process's, because stamping today's tenancy onto another tenant's history is the exact loss the residence exists to prevent. Rendered content identity is the grain: a keyed fact groups at `(key, tenant, domain, kind)`, and a keyless wire mapping coarsens to the `(domain, kind, tenant)` slot. Apex position closes the tabular strata: cost imports its receipt owners strictly downward and admits the tensor plan only as wire data.

## [01]-[INDEX]

- [02]-[COST]: `CostFact` harvests every receipt family, `CostLedger` group-folds the slots, `RatePolicy` prices them, and one Arrow frame leaves keyed by its own content.

## [02]-[COST]

- Owner: `CostFact` — the one harvested cost row carrying domain, kind, the rendered identity key, tenant, and the four quantity axes (`rows`/`bytes_moved`/`seconds`/`tasks`); `CostDomain` the closed domain vocabulary over the harvested planes; `CostUnit` the quantity-axis vocabulary the rate rows key on; `RatePolicy` the caller-supplied `(domain, unit) -> price` row set with its currency token, never constants; `CostLedger` the group-fold owner; `CostReceipt` the emitted evidence row. One fact shape spans every source — a per-plane fact type is the deleted form.
- Owner: `_DOMAIN` DERIVES a durable row's plane off each `CostDomain` member's own value, `_RECORDED` carrying the single divergence the estate holds (the commit owner records `lake`) and `TENSOR` carving out because its facts arrive as a live wire mapping and never off the residence, and `_EVIDENCE_AXIS` names which recorded fact feeds each quantity — so reconstruction reads rows, never a per-plane decoder, and a live arm reading a field no row names prices its own history at zero. `CostFact.priced` is the ONE projection from a plane's axis roster onto the four quantity slots, its `read` the only difference between the row-wise durable arm and the columnar residence fold, and `_NUMBER` the one numeric admission grammar both apply — scalar through `_number`, vectorized through `_numeric`.
- Entry: `CostFact.of` discriminates receipt, durable-evidence, wire-mapping, and already-harvested fact shapes under `assert_never`, so every source reaches one railed entry; only the host-crossing tensor mapping runs the complete `PlanReceipt` wire admission fold. `CostLedger.of` expands each source through `_expanded` and traverses the flat stream before `frame(policy)` folds it — `CostFact.combined` the associative quantity monoid, the `Map` slot fold key-sorted so identical fact sets yield one byte-stable frame — and `boundary` captures numeric conversion, Arrow materialization, and canonical `arrow_bytes` egress before the frame's own `ContentKey` enters the `CostReceipt`.
- Auto: the lakehouse arm reads the receipt's own `residence` slot to price an evidence commit on `TELEMETRY` and a caller's table on `LAKEHOUSE` — the SAME split the durable row already records through its residence domain, so a window harvested live and one reconstructed from the residence land on one plane rather than two.
- Auto: the materialize arm keys `kind="cdc"` and never the partition id — unbounded partition cardinality stays receipt-only, the standing metric-dimension law applied to the cost grain; the query arm reads `bytes_moved` and `seconds` off the band's own `Posture` slots — a measure the engine WITHHELD prices as unmeasured and COUNTS on the slot's `unpriced` census, never as a proven zero — while a run whose `mode` is `OFF` harvests zero and counts nothing, because nobody asked; a keyless fact groups at its coarse slot with `content_key=""` in the frame, never a dropped row.
- Receipt: `CostReceipt.contribute` emits one emitted-phase `Receipt.of("cost-ledger", ("emitted", subject, facts))` row carrying slot count, tenant count, priced total, the unpriceable-fact census, currency, and the frame `ContentKey`; it records no `Metrics.record` measure because every harvested quantity already projected at its source receipt's own `contribute` — a second recording does double-count the spine.
- Packages: `pyarrow` (the priced frame constructor) with `pyarrow.compute` (`map_lookup`/`fill_null`/`match_substring_regex`/`if_else`/`cast` — the residence fold's whole vectorized half, and `Table.group_by`/`aggregate` for the slot sum), `msgspec` (`Struct` the frozen owners), `expression` (`Block`/`Map`/`Some` the slot fold beside `extra.result.traverse`, the substrate's own fail-fast threader every railed stream here rides), `opentelemetry-api` (`baggage.get_baggage` over the current context — the one tenant read), `beartype` (`@beartype(conf=FAULT_CONF)` on the public `RatePolicy.of`/`CostLedger.of` factories), `tabular/columnar#SCAN` (`QueryReceipt`), `tabular/interop#INTEROP` (`arrow_bytes` the folder's one whole-table serialization the priced frame keys through, `ColumnSpec`/`column_frame` the one declaration its schema and builder both derive off, and `DataLeg` this page anchors its `RAISES` table on), `tabular/lakehouse#LAKEHOUSE` (`LakeReceipt` beside the `ReceiptFact` durable row whose schema the residence owns), `tabular/materialize#MATERIALIZE` (`PartitionBundle`), `tabular/egress#EGRESS` (`EgressReceipt`), runtime (`RuntimeRail`/`FAULT_CONF`/`ContentIdentity`/`ContentKey`/`Receipt`/`TENANT_BAGGAGE`).
- Growth: a new harvested receipt family is one `CostFact.of` arm; a new cost axis is one `CostFact` field, one `CostUnit` member, one `combined` term, one `_COST_COLUMNS` row its schema and builder both derive off, and one `CostFact.priced` slot both readers inherit; a new priced plane is one `CostDomain` member whose value IS its recorded spelling — a `_RECORDED` row only where the producer diverges — plus its `_EVIDENCE_AXIS` row and its rate rows; the columnar fold picking it up with zero edits; a new query engine over the residence is zero edits, the frame shape being the whole contract; a new frame column derives inside `frame`; zero new surface.
- Boundary: a projection over receipts, never a second metering pipeline — no `Metrics.record`, no span, no durable store, no currency conversion, and no scan of its own: the residence frame arrives from the caller that ran it, so this owner never picks an engine or opens a lakehouse; rates arrive as policy rows, never module constants; the gridded `PlanReceipt` crosses as wire data (its `to_builtins` lowering), never an upward `rasm.data.gridded` import; a tenant field on any source receipt, a per-plane fact type, a partition-id cost dimension, and a hand-rolled hash over the priced frame where `arrow_bytes` with `ContentIdentity` own identity are the deleted forms.
- Boundary: the priced frame is the TERMINAL egress and advertises no reader: `python:artifacts/visualization/table#TABLE` `TablePlan.of` admits it as the settled Arrow-capsule frame it is, so naming a renderer here claims a seam neither end carries.
- Boundary: rates and totals ride `float` and no settlement reads this frame — observed spend is a dashboard reading, while the trapped exact-decimal arithmetic a settled charge demands homes at the runtime journal's rating fold and never at this projection.

```python signature
from collections.abc import Callable, Iterable, Mapping
from enum import StrEnum
from math import isfinite
from functools import reduce
from re import fullmatch
from typing import Any, Final, assert_never, cast

import pyarrow as pa
import pyarrow.compute as pc
from beartype import beartype
from expression import Error, Ok, Option, Some
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct, structs
from opentelemetry import baggage
from opentelemetry import context as otel_context

from rasm.data.tabular.columnar import ProfileMode, QueryReceipt
from rasm.data.tabular.egress import EgressReceipt
from rasm.data.tabular.interop import ColumnSpec, DataLeg, arrow_bytes, column_frame
from rasm.data.tabular.lakehouse import LakeReceipt, ReceiptFact
from rasm.data.tabular.materialize import PartitionBundle
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, Posture, RuntimeRail, boundary, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import TENANT_BAGGAGE
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

# Grouping slot is (content_key hex | "", tenant | "", domain, kind), content-key-first and coarse for keyless facts.
type Slot = tuple[str, str, str, str]
# one priced ROW as `tabular/interop#INTEROP` `ColumnSpec` declarations. The slot columns lead so the residence fold
# groups on the SAME grain the live fold groups on, and `_SLOT_COLUMNS` DERIVES off the leading four rather than
# re-spelling them a third time beside the schema and the builder. Each spec lifts off the priced triple, so a fifth
# quantity axis is ONE row rather than a schema field plus a builder key plus a slot name.

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

_COST_COLUMNS: Final[Block[ColumnSpec["PricedSlot", object]]] = Block.of_seq([
    ColumnSpec(name="content_key", arrow=pa.string(), kind=str, lift=lambda row: row.slot[0]),
    ColumnSpec(name="tenant", arrow=pa.string(), kind=str, lift=lambda row: row.slot[1]),
    ColumnSpec(name="domain", arrow=pa.string(), kind=str, lift=lambda row: row.slot[2]),
    ColumnSpec(name="kind", arrow=pa.string(), kind=str, lift=lambda row: row.slot[3]),
    ColumnSpec(name="rows", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.rows),
    ColumnSpec(name="bytes", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.bytes_moved),
    ColumnSpec(name="seconds", arrow=pa.float64(), kind=float, lift=lambda row: row.fact.seconds),
    ColumnSpec(name="tasks", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.tasks),
    # the UNPRICEABLE-rendering census travels with the window it belongs to: a producer defect read as a
    # measurement prices a window at less than it cost, and a count nobody stores is a loss nobody can see.
    ColumnSpec(name="unpriced", arrow=pa.int64(), kind=int, lift=lambda row: row.fact.unpriced),
    ColumnSpec(name="cost", arrow=pa.float64(), kind=float, lift=lambda row: row.price),
    ColumnSpec(name="currency", arrow=pa.string(), kind=str, lift=lambda row: row.currency),
])
_SLOT_COLUMNS: Final[tuple[str, ...]] = tuple(spec.name for spec in _COST_COLUMNS.take(4))

# the source shape every column lifts off, so the frame build reads a NAMED slot rather than a tuple offset: the
# positional `(slot, fact, price)` triple this replaces was destructured five separate ways inside one comprehension
# set, and the currency it did not carry was spread as a parallel list beside them.
class PricedSlot(Struct, frozen=True):
    slot: Slot
    fact: "CostFact"
    price: float
    currency: str


# --- [ERRORS] ---------------------------------------------------------------------------

# the NAMED source a zero carries when a recorded rendering is not a priceable number, so the census counts on a
# token rather than on a free string a second reader would have to match by hand.
UNPRICEABLE: Final[str] = "unpriceable-rendering"


def _cost_raises() -> Catch:
    # the two fenced legs here are a tuple build and an Arrow materialization over already-admitted values, so the
    # Arrow core rail is the whole reach. `pa.ArrowIOError` derives from `OSError` alone and never from
    # `ArrowException` (`.api/pyarrow.md` `[CORE_ERRORS]`), which is why both are named.
    return (pa.ArrowException, OSError)


# this module's whole raise roster. Every row declares TERMINAL: a malformed rate table, an unpriced plane, an
# invalid quantity, a non-finite total, and a malformed wire mapping are all defects in the input a re-issue of the
# same window reproduces exactly. Near-identical rate defects collapse into ONE parameterized row taking subjects.
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
COST_UNPRICED_DOMAIN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="evidence", arm="config", defect="unpriced-domain", retriability=TERMINAL, slots=("domain",)
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
COST_RESIDENCE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COST, point="residence", arm="boundary", defect="residence-expand", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    COST_RATE_POLICY,
    COST_QUANTITY,
    COST_UNPRICED_UNIT,
    COST_NONFINITE,
    COST_UNPRICED_DOMAIN,
    COST_WIRE,
    COST_QUANTITIES,
    COST_FRAME,
    COST_RESIDENCE,
]))


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

# the metric-domain spelling each contributor hands `Metrics.record`, which the residence then stores verbatim. Key
# identity holds for every member but ONE — the commit owner records `lake` where this plane is named `lakehouse` —
# so the roster carries the DIVERGENCE alone and every other spelling derives off the member's own value: a five-row
# mirror of a closed vocabulary re-types four spellings the vocabulary already holds and leaves a renamed member
# resolving under two names with nothing raising. `TENSOR` carves out because its facts arrive as a live
# `PlanReceipt` wire mapping and never off the residence, so a residence token for it would key rows nobody writes.
_RECORDED: Final[Map[CostDomain, str]] = Map.of_seq([(CostDomain.LAKEHOUSE, "lake")])
_DOMAIN: Final[Map[str, CostDomain]] = Map.of_seq(
    (_RECORDED.try_find(member).default_value(member.value), member) for member in CostDomain if member is not CostDomain.TENSOR
)

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
    # how many recorded facts this slot could not price. The page's own trade-off stands — a signed or non-finite
    # rendering contributes zero rather than aborting a window thousands of rows wide — but a producer defect read as
    # a measurement with NO evidence emitted anywhere is the half that was missing: a window now carries both its
    # price and the count of facts behind it that never reached one.
    unpriced: int = 0

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
                # every profiled quantity reads through the band's own `Posture`: a measure the engine WITHHELD is
                # not a proven zero, so it prices as unmeasured AND counts on this slot's census rather than
                # silently costing a window the engine really measured at nothing.
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
    def priced(cls, domain: CostDomain, kind: str, key: str, tenant: str, read: Callable[[str], "Posture[float]"]) -> "CostFact":
        # ONE projection from a plane's axis roster onto the four quantity slots, read by the row-wise durable arm and
        # by the columnar residence fold alike — `read` is their only difference, a map lookup on one side and an
        # already-summed column on the other, so a fifth unit cannot drift two spellings apart. Repeated units
        # accumulate, which is how a commit's two file counters land as one task total. Each reading is a `Posture`
        # because THREE states one `0.0` fused: the plane recorded no such fact (`absent` — normal, and not a defect),
        # the plane recorded a value this grammar prices (`declared`), and the plane recorded a rendering no priced
        # axis admits (`defaulted` under `UNPRICEABLE`), which contributes zero AND is counted.
        axes = _EVIDENCE_AXIS.get(domain) or ()
        readings = tuple((unit, read(name)) for unit, name in axes)
        totals = {unit: sum(held.option().default_value(0.0) for candidate, held in readings if candidate is unit) for unit, _name in axes}
        return cls(
            domain=domain,
            kind=kind,
            key=key,
            tenant=tenant or None,
            rows=int(totals.get(CostUnit.ROWS, 0.0)),
            bytes_moved=int(totals.get(CostUnit.BYTES, 0.0)),
            seconds=totals.get(CostUnit.SECONDS, 0.0),
            tasks=int(totals.get(CostUnit.TASKS, 0.0)),
            unpriced=sum(1 for _unit, held in readings if held.source == Some(UNPRICEABLE)),
        )

    @classmethod
    def _evidence(cls, row: ReceiptFact) -> "RuntimeRail[CostFact]":
        # `_evidence` IS the durable half of the one harvest: `_DOMAIN` resolves the plane off the recorded metric-domain
        # string and `_EVIDENCE_AXIS` names which recorded fact feeds each quantity, so reconstruction reads rows rather
        # than a per-plane decoder. Non-numeric and absent facts contribute zero — the residence keeps every dimension
        # a producer wrote, and a fact this plane does not price is not a malformed row.
        return (
            _DOMAIN.try_find(row.domain)
            .map(lambda domain: Ok(cls.priced(domain, row.kind, row.content_key, row.tenant, lambda name: _reading(row.facts, name))))
            .default_with(lambda: Error(COST_UNPRICED_DOMAIN.raised(row.domain)))
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
            unpriced=left.unpriced + right.unpriced,
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
            Error(COST_RATE_POLICY.raised())
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


class CostReceipt(Struct, frozen=True):
    slots: int
    tenants: int
    total: float
    # how many recorded facts this window could not price, summed over its slots. The page's stated trade-off keeps
    # the zero rather than aborting a window over one row, and this count is the evidence half that trade-off owed:
    # a window carries BOTH its price and the number of facts behind it that never reached one.
    unpriced: int
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
                        "unpriced": self.unpriced,
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
            lambda row: policy.price(row[1]).map(lambda price: PricedSlot(slot=row[0], fact=row[1], price=price, currency=policy.currency)),
            Block.of_seq(self._grouped().to_seq()),
        ).bind(lambda rows: self._frame(policy, Block.of_seq(rows)))

    def _frame(self, policy: RatePolicy, priced: "Block[PricedSlot]") -> "RuntimeRail[tuple[pa.Table, CostReceipt]]":
        def materialized() -> tuple[pa.Table, bytes]:
            # the frame DERIVES off `_COST_COLUMNS`: schema and builder are two projections of one declaration, so a
            # fifth quantity axis is one row rather than a schema field plus a builder key that still compile apart.
            table = column_frame(_COST_COLUMNS, priced)
            return table, arrow_bytes(table)

        # per-row prices are finite by the rate gate, yet their sum can still overflow to inf; the aggregate re-proves
        # finiteness on the same rail, so an invalid CostReceipt is unconstructible.
        total = sum(row.price for row in priced)
        if not isfinite(total):
            return Error(COST_NONFINITE.raised("aggregate", str(len(priced))))
        tenants = len({row.slot[1] for row in priced if row.slot[1]})
        unpriced = sum(row.fact.unpriced for row in priced)
        return boundary(COST_FRAME, materialized, catch=_cost_raises()).bind(
            lambda held: ContentIdentity.of("cost", held[1]).map(
                lambda key: (
                    held[0],
                    CostReceipt(
                        slots=len(priced), tenants=tenants, total=total, unpriced=unpriced, currency=policy.currency, content_key=key
                    ),
                )
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
            return boundary(COST_RESIDENCE, lambda: _residence(frame), catch=_cost_raises())
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
    looked = {name: pc.map_lookup(rows.column("facts"), query_key=name, occurrence="first") for _unit, name in axes}
    # the census rides as its OWN summed column: one boolean pass per axis marks a recorded rendering this grammar
    # cannot price, so a whole plane's unpriceable count leaves in the same group-by that sums its quantities.
    quantities = {name: _numeric(held) for name, held in looked.items()} | {
        "unpriced": reduce(pc.add, (pc.cast(_unpriceable(held), pa.int64()) for held in looked.values()))
    }
    grouped = pa.table({column: rows.column(column) for column in _SLOT_COLUMNS} | quantities).group_by(list(_SLOT_COLUMNS)).aggregate(
        [(name, "sum") for name in quantities]
    )
    return tuple(_slot_fact(row, domain) for row in grouped.to_pylist())


def _slot_fact(row: Mapping[str, Any], domain: CostDomain) -> CostFact:
    # one grouped slot read back through the SAME `CostFact.priced` projection the durable row-wise arm reads, its
    # `read` closing over the summed column rather than the open map. Every summed column is `declared`: `fill_null`
    # ran upstream and the caller already refused an empty row set, so a null sum is unreachable here and the
    # `or 0.0` tail this deletes guarded nothing. The unpriceable CENSUS arrives as its own summed column rather
    # than through `read`, because the vectorized grammar counts the whole plane in one pass.
    return structs.replace(
        CostFact.priced(domain, row["kind"], row["content_key"], row["tenant"], lambda name: Posture(declared=float(row[f"{name}_sum"]))),
        unpriced=int(row["unpriced_sum"]),
    )


def _numeric(held: Any) -> Any:
    # vectorized twin of `_number` over the SAME grammar: a value this plane cannot price — prose, non-finite, or
    # signed — reads zero exactly as the row-wise arm reads it, never a cast raising over one fact and losing the
    # whole window with it.
    filled = pc.fill_null(held, "0")
    return pc.cast(pc.if_else(pc.match_substring_regex(filled, rf"^{_NUMBER}$"), filled, "0"), pa.float64())


def _unpriceable(held: Any) -> Any:
    # the vectorized census twin of `_reading`'s `defaulted` arm: a recorded rendering outside `_NUMBER` marks true,
    # while an ABSENT fact marks false — a plane recording no such fact is normal and never a producer defect.
    return pc.and_(pc.is_valid(held), pc.invert(pc.match_substring_regex(pc.fill_null(held, "0"), rf"^{_NUMBER}$")))


def _reading(facts: Mapping[str, Any], name: str) -> "Posture[float]":
    # a recorded fact arrives as the residence's string rendering, and THREE states the bare `0.0` fused are three
    # different facts: the plane recorded no such fact at all (`absent` — normal, priced as nothing and counted as
    # nothing), the plane recorded a value this grammar prices (`declared`), and the plane recorded a rendering no
    # priced axis admits — a prose fact, a non-finite spelling, or a signed quantity (`defaulted` under
    # `UNPRICEABLE`). The third still contributes zero rather than aborting a window over thousands of rows for one
    # row's fact, exactly as this page has always ruled, but it is now COUNTED where before it vanished.
    match facts.get(name):
        case None:
            return Posture(absent=None)
        case held if isinstance(held, str) and fullmatch(_NUMBER, held):
            return Posture(declared=float(held))
        case _:
            return Posture(defaulted=(0.0, UNPRICEABLE))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
