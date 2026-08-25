# [PY_DATA_MATERIALIZE]

One incremental CDC-materialization owner, the composing concern above the engines it composes: `DerivedSnapshot` folds the `tabular/lakehouse#LAKEHOUSE` change feed, the `tabular/query#QUERY` engine, and runtime `ContentIdentity` into a partition-delta recompute, and `PartitionBundle` is the per-partition content-keyed Arrow bundle. All three composition edges point strictly down the folder order, keeping `columnar` a pure base with zero back-edges.

Only CDF-changed partitions recompute — an unchanged partition reuses its content key untouched, and a full re-scan is the rejected form. Each touched bundle keys through `interop`'s `arrow_bytes` fold, and `snapshot_key` Merkle-folds the child keys. `LakeOp.ChangeFeed` through `Lakehouse.run_async` supplies the feed on its receipt `payload` slot, so this consumer opens no provider, and `_CHANGE_STATE` rows which change-type spellings a recompute reads — a format the lakehouse arms but this table leaves unrowed refuses typed rather than filtering an unknown discriminant to silent emptiness.

## [01]-[INDEX]

- [02]-[MATERIALIZE]: the `DerivedSnapshot`/`PartitionBundle` partition-delta recompute over the change feed and the query engine.

## [02]-[MATERIALIZE]

- Owner: `DerivedSnapshot` — the one incremental-materialization owner over a `(partition_by, transform, generation, lane)` policy, folding one change-feed range into per-partition recomputes; `PartitionBundle` is the per-partition content-keyed unit and `snapshot_key` its Merkle root. Partition identity is the composite key alone, so a second partitioning strategy is a `partition_by` tuple, never a sibling snapshot type.
- Entry: `DerivedSnapshot.of` admits the policy on the rail — an empty `partition_by` refuses typed, matching the sibling `Lakehouse.open`/`ObjectEgress.of`/`QueryEngine.of` admission contract under `@beartype(conf=FAULT_CONF)`, never a construction-time raise the composition root has no fence for. `refresh` is the one operation entrypoint, taking only the change-feed range and the prior bundle set; `register_data_hooks` is the package's one hook-registration fold.
- Auto: the `lakehouse` changefeed arm owns the arro3-to-`pyarrow` PyCapsule re-import, so the frame reaching this owner already carries the sort and compute surface the partition split needs. Partitioning is one strict sorted pass over every CDF record, and the `_CHANGE_STATE` row supplies both the discriminant column and its survivor set. `register_data_hooks(scope)` claims the package point table in ONE `Hooks.register` roster transition and deposits its `DataInstall` receipt on that same registry's install ledger, so a capsule reads this package's admission where an absent row is the stated diagnosis; every emitting owner carries that same scope, so registry custody and fire cannot cross compositions. Each recomputed bundle fires `REFRESH_POINT` on that scoped registry, and a late subscriber drains the bounded replay ring.
- Law: a refresh lands ONE `RECORD` `MeterFact` on the `python:runtime/observability/journal#LEDGER` plane over the rows it recomputed, seated where the recomputed set is whole and the merge has not yet folded prior bundles back in — metering the merged output charges every unchanged partition again on every drain, which is the exact re-pricing the incremental fold exists to delete. The seat is the async split fold under the runtime producer-seam law, never `contribute`, and the record rail binds into the refresh verdict; a drain that recomputed nothing meters nothing.
- Receipt: `PartitionBundle.contribute` yields one emitted-phase `Receipt.of("derived-snapshot", ("emitted", partition, facts))` and projects recomputed row volume onto the metric spine under `domain="materialize"`; the partition id stays receipt-only because a composite key carries unbounded cardinality. `snapshot_key` Merkle-folds the partition-sorted child keys, so identical content yields one key whatever order the lane completes in.
- Packages: `pyarrow`/`pyarrow.compute` (the sorted slice and the change-type filter), `msgspec` (the frozen `Struct` shapes and the canonical-JSON composite key), `expression` (`Block`/`Map` and the `RuntimeRail` carrier), `beartype` (`@beartype(conf=FAULT_CONF)` on the public `of`/`refresh` seams), `tabular/lakehouse#LAKEHOUSE` (`LakeOp.ChangeFeed` through `Lakehouse.run_async`, the receipt `payload` carrying the feed), `tabular/query#QUERY` (the recompute engine), `tabular/interop#INTEROP` (the `arrow_bytes` fold, the `DataHook` point-id roster every fired point keys on, and `DataLeg` this page anchors its `RAISES` table on), runtime (`BackendGeneration`/`ContentIdentity`/`RuntimeRail`/`async_boundary`/`LanePolicy`/`Metrics`/`Journal.record`/`scoped`, with `Hooks.register`'s roster arm the one whole-set claim and `Hooks.installed` the producer-install ledger this package deposits on). No provider package binds directly: the lakehouse owner holds every change-feed provider this page reads.
- Growth: a new transform is a different `QuerySpec`; a new partition strategy is one `partition_by` tuple; a second CDF source is one `_CHANGE_STATE` row carrying its discriminant column and survivor states, the lakehouse arm supplying the feed; a new data hook is one `DataHook` member at the `tabular/interop#INTEROP` roster seat plus one `DATA_HOOK_POINTS` row and its owner fire, the install receipt widening by derivation because it names the landed ids rather than a hand-kept list; a new admission fact this package proves at composition is one `DataInstall` field of native scalars; a new admission invariant is one refusal arm on `of`.
- Boundary: composes the `lakehouse` `ChangeFeed` op, the `query` engine under the owner's admitted `BackendGeneration`, the `interop` `arrow_bytes` fold, and the owner's composition-root-bound `lane` — `refresh` accepts only operation inputs, the partition fan-out drains under `LanePolicy.drain`, never a page-local task-group rig; a casualty fails the refresh closed, no durable derived store, no parallel materialization module, no second CDF reader. Deleted forms: a per-partition engine minted with no generation, an admission invariant raising at construction where the composition root carries no fence to convert it, a second CDF reader opened behind the lakehouse owner that already holds one, a hardcoded single-format test where the change vocabulary is a row, a hook-fire rail riding out as the recompute's own value, an accumulating per-point registration fold beside the registry's own roster arm — it short-circuits at the first breach with every prior point mounted, which is the half-mount that arm exists to foreclose — and a bare `trace.get_tracer(scope)` beside the faults-owned `scoped` stamp.

```python signature
from collections.abc import Iterable
from functools import partial
from itertools import accumulate, groupby
from typing import Final, assert_never

import msgspec.json
import pyarrow as pa
import pyarrow.compute as pc
from beartype import beartype
from expression import Error, Ok, Result
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.contract import VERDICT_POINT
from rasm.data.tabular.egress import COPY_POINT, DELETE_POINT, PUT_POINT, RENAME_POINT
from rasm.data.tabular.interop import DataHook, DataLeg, arrow_bytes
from rasm.data.tabular.lakehouse import LAKE_COMMIT_POINT, LakeOp, Lakehouse, TableFormat
from rasm.data.tabular.query import QueryEngine, QuerySpec
from rasm.runtime.admission import BackendGeneration
from rasm.runtime.faults import FAULT_CONF, TERMINAL, TRANSIENT, BoundaryFault, Catch, FaultRow, RuntimeRail, async_boundary, rostered, scoped
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Journal, MeterFact, Resource
from rasm.runtime.lanes import Admit, LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.tabular.materialize")

_CHANGE_STATE: Final[Map[TableFormat, tuple[str, tuple[str, ...]]]] = Map.of_seq([
    (TableFormat.DELTA, ("_change_type", ("insert", "update_postimage"))),
    (TableFormat.DUCKLAKE, ("change_type", ("insert", "update_postimage"))),
])


DOMAIN: Final[str] = "materialize"

# --- [ERRORS] ---------------------------------------------------------------------------


def _refresh_raises() -> Catch:
    return (pa.ArrowException, OSError)


SNAPSHOT_PARTITION: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MATERIALIZE, point="admit", arm="config", defect="partition-by-required", retriability=TERMINAL
)
FEED_UNROWED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MATERIALIZE, point="refresh.feed", arm="config", defect="no-rowed-change-feed", retriability=TERMINAL, slots=("format",)
)
REFRESH_RUN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.MATERIALIZE, point="refresh", arm="boundary", defect="refresh-run", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([SNAPSHOT_PARTITION, FEED_UNROWED, REFRESH_RUN]))


class PartitionBundle(Struct, frozen=True):
    partition: str
    rows: int
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        Metrics.record({"rasm.materialize.rows": float(self.rows)}, domain=DOMAIN, kind="cdc")
        yield Receipt.of(
            "derived-snapshot",
            ("emitted", self.partition, {"domain": DOMAIN, "kind": "cdc", "key": self.content_key.hex, "rows": self.rows}),
        )


def _metered(bundles: "Block[PartitionBundle]") -> "Block[MeterFact]":
    recomputed = sum(bundle.rows for bundle in bundles)
    return Block.of_seq((MeterFact(resource=Resource.RECORD, quantity=recomputed, surface=DOMAIN),) if recomputed else ())


REFRESH_POINT: Final[HookPoint[PartitionBundle]] = HookPoint(
    id=DataHook.MATERIALIZE_REFRESH, payload=PartitionBundle, modality=Modality(replay=64)
)

OWNER: Final[str] = "data.tabular"


class DataInstall(Struct, frozen=True, gc=False):
    points: tuple[str, ...]


DATA_HOOK_POINTS: Final[Block[HookPoint[Struct]]] = Block.of_seq([
    LAKE_COMMIT_POINT,
    PUT_POINT,
    DELETE_POINT,
    COPY_POINT,
    RENAME_POINT,
    REFRESH_POINT,
    VERDICT_POINT,
])


def register_data_hooks(scope: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[DataInstall]":
    return Hooks.register(DATA_HOOK_POINTS, scope=scope).map(
        lambda points: Hooks.installed(OWNER, DataInstall(points=tuple(point.id for point in points)), scope=scope)
    )


class DerivedSnapshot(Struct, frozen=True):
    partition_by: tuple[str, ...]
    transform: QuerySpec
    generation: BackendGeneration
    lane: LanePolicy
    scope: ScopeKey = DEFAULT_SCOPE

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(
        cls,
        partition_by: tuple[str, ...],
        transform: QuerySpec,
        generation: BackendGeneration,
        lane: LanePolicy,
        *,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeRail[DerivedSnapshot]":
        if not partition_by:
            return Error(SNAPSHOT_PARTITION.raised())
        return Ok(cls(partition_by=partition_by, transform=transform, generation=generation, lane=lane, scope=scope))

    @beartype(conf=FAULT_CONF)
    async def refresh(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        with _TRACER.start_as_current_span("derived.refresh", attributes={"rasm.materialize.partitions": len(prior)}):
            railed = await async_boundary(REFRESH_RUN, lambda: self._materialize(source, start, end, prior), catch=_refresh_raises())
            return railed.bind(lambda rail: rail)

    async def _materialize(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        match self._vocabulary(source):
            case Result(tag="error", error=fault):
                return Error(fault)
            case Result(tag="ok", ok=vocabulary):
                return await self._fed(source, start, end, prior, vocabulary)
            case unreachable:
                assert_never(unreachable)

    def _vocabulary(self, source: Lakehouse) -> "RuntimeRail[tuple[str, tuple[str, ...]]]":
        return (
            _CHANGE_STATE.try_find(source.table_format)
            .map(Ok)
            .default_with(lambda: Error(FEED_UNROWED.raised(source.table_format.value)))
        )

    async def _fed(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...], vocabulary: tuple[str, tuple[str, ...]]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        fed = await source.run_async(LakeOp.ChangeFeed(starting_version=start, ending_version=end))
        match fed:
            case Result(tag="error", error=fault):
                return Error(fault)
            case Result(tag="ok", ok=receipt):
                return await self._split(receipt.payload, prior, vocabulary)
            case unreachable:
                assert_never(unreachable)

    async def _split(
        self, cdf: pa.Table, prior: tuple[PartitionBundle, ...], vocabulary: tuple[str, tuple[str, ...]]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        column, survivors = vocabulary
        ordered = cdf.sort_by([(col, "ascending") for col in self.partition_by])
        tuples = list(zip(*(ordered.column(col).to_pylist() for col in self.partition_by), strict=True))
        runs = tuple((key, sum(1 for _ in members)) for key, members in groupby(tuples))
        offsets = tuple(accumulate((count for _key, count in runs), initial=0))[:-1]
        deltas = tuple(
            (ordered.slice(offset, count).filter(pc.field(column).isin(survivors)), self._key_id(key))
            for (key, count), offset in zip(runs, offsets, strict=True)
        )
        receipt = await self.lane.drain(Block.of_seq([Admit(bare=partial(self._recompute, delta, partition)) for delta, partition in deltas]))
        if not receipt.faults.is_empty():
            return Error(receipt.faults.reduce(BoundaryFault.combine))
        return (await Journal.record(_metered(receipt.values), scope=self.scope)).map(lambda _landed: self._merge(prior, receipt.values))

    def _key_id(self, key: tuple[object, ...]) -> str:
        return msgspec.json.encode(key).decode()

    def _merge(self, prior: tuple[PartitionBundle, ...], bundles: "Block[PartitionBundle]") -> tuple[PartitionBundle, ...]:
        merged = {b.partition: b for b in prior} | {b.partition: b for b in bundles}
        return tuple(merged[partition] for partition in sorted(merged))

    async def _recompute(self, delta: pa.Table, partition: str) -> "RuntimeRail[PartitionBundle]":
        railed = await QueryEngine.of(self.generation, {"delta": delta}).run(self.transform)
        return railed.bind(
            lambda result: ContentIdentity.of("partition", arrow_bytes(result)).map(
                lambda key: PartitionBundle(partition=partition, rows=result.num_rows, content_key=key)
            )
        ).bind(lambda bundle: Hooks.fire(REFRESH_POINT.id, bundle, scope=self.scope).map(lambda _fact: bundle))


def snapshot_key(bundles: tuple[PartitionBundle, ...]) -> "RuntimeRail[ContentKey]":
    ordered = sorted(bundles, key=lambda b: b.partition)
    return ContentIdentity.of("derived-snapshot", tuple(b.content_key for b in ordered))
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
