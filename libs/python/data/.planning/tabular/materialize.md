# [PY_DATA_MATERIALIZE]

One incremental CDC-materialization owner, the composing concern above the engines it composes: `DerivedSnapshot` folds the `tabular/lakehouse#LAKEHOUSE` change feed, the `tabular/query#QUERY` engine, and runtime `ContentIdentity` into a partition-delta recompute, and `PartitionBundle` is the per-partition content-keyed Arrow bundle. All three composition edges point strictly down the folder order, keeping `columnar` a pure base with zero back-edges.

Only CDF-changed partitions recompute — an unchanged partition's content key is reused untouched, and a full re-scan is the rejected form. Each touched bundle keys through `columnar`'s public `arrow_bytes` fold, and the parent `snapshot_key` Merkle-folds the child keys. Change-feed reads ride the same `deltalake.load_cdf` surface the `lakehouse` `ChangeFeed` op binds; the `lakehouse` `_PORTABLE` row proves `changefeed` is Delta-exclusive, so the non-Delta reject here is table-derived law.

## [01]-[INDEX]

- [01]-[MATERIALIZE]: the `DerivedSnapshot`/`PartitionBundle` partition-delta recompute over the change feed and the query engine.

## [02]-[MATERIALIZE]

- Auto: `load_cdf` returns an `arro3.core.RecordBatchReader`, and the arro3 frame carries no `pyarrow` sort or compute surface — so the zero-copy `pa.table(...)` PyCapsule re-import is the load-bearing hop before the key-sorted partition split. Partitioning is one strict sorted pass over every CDF record. `register_data_hooks(scope)` folds the package point table through `Hooks.register`; every emitting owner carries that same scope, so registry custody and fire cannot cross compositions. Each recomputed bundle fires `REFRESH_POINT` on that scoped registry, and a late subscriber drains the bounded replay ring.
- Growth: a new transform is a different `QuerySpec`; a new partition strategy is one `partition_by` tuple; a second CDF source is one arm on the lakehouse format axis; a new data hook is one `DATA_HOOK_POINTS` row and its owner fire.
- Boundary: composes the `lakehouse` `ChangeFeed` op, the `query` engine, the `columnar` `arrow_bytes` fold, and the owner's composition-root-bound `lane` — `refresh` accepts only operation inputs, the partition fan-out drains under `LanePolicy.drain`, never a page-local task-group rig; a casualty fails the refresh closed, no durable derived store, no parallel materialization module, no second CDF reader.

```python signature
import msgspec.json
import pyarrow as pa
import pyarrow.compute as pc
from collections.abc import Iterable
from deltalake import DeltaTable
from expression import Error, Ok
from expression.collections import Block
from functools import partial
from itertools import accumulate, groupby
from msgspec import Struct
from opentelemetry import trace
from typing import Final

from rasm.data.tabular.columnar import arrow_bytes
from rasm.data.tabular.contract import VERDICT_POINT
from rasm.data.tabular.egress import COPY_POINT, DELETE_POINT, PUT_POINT, RENAME_POINT
from rasm.data.tabular.lakehouse import LAKE_COMMIT_POINT, Lakehouse, TableFormat
from rasm.data.tabular.query import QueryEngine, QuerySpec
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import Admit, LanePolicy, on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

_TRACER: Final = trace.get_tracer("rasm.data.tabular.materialize")

# CDF post-state survivor set: `update_preimage` carries the OLD image beside its postimage twin and a delete carries no
# surviving row, so only these two change types feed a recompute — a kept preimage doubles every updated row.
_POST_STATE: Final[tuple[str, ...]] = ("insert", "update_postimage")


class PartitionBundle(Struct, frozen=True):
    partition: str
    rows: int
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        # merge-side metric: recomputed row volume lands on the metric spine under domain="materialize"; the partition
        # id stays receipt-only — unbounded cardinality never becomes a metric dimension.
        Metrics.record({"rasm.materialize.rows": float(self.rows)}, domain="materialize", kind="cdc")
        return (Receipt.of("derived-snapshot", ("emitted", self.partition, {"rows": self.rows})),)


# late-attach replay edge: every recomputed bundle fires the REPLAY row inside its composition scope.
REFRESH_POINT: Final[HookPoint[PartitionBundle]] = HookPoint(
    id="rasm.data.materialize.refresh", payload=PartitionBundle, modality=Modality.REPLAY, buffer=64
)

DATA_HOOK_POINTS: Final[tuple[HookPoint[Struct], ...]] = (
    LAKE_COMMIT_POINT,
    PUT_POINT,
    DELETE_POINT,
    COPY_POINT,
    RENAME_POINT,
    REFRESH_POINT,
    VERDICT_POINT,
)


def register_data_hooks(scope: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[tuple[HookPoint[Struct], ...]]":
    # one composition fold consumes every registration rail; duplicate ids and malformed rows fail the root.
    return Block.of_seq(DATA_HOOK_POINTS).fold(
        lambda registered, point: registered.bind(
            lambda rows: Hooks.register(point, scope=scope).map(lambda admitted: (*rows, admitted))
        ),
        Ok(()),
    )


class DerivedSnapshot(Struct, frozen=True):
    partition_by: tuple[str, ...]
    transform: QuerySpec
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner,
    # and a consumer hands `refresh` only operation inputs while capacity, deadline, and cancellation ride this binding.
    lane: LanePolicy
    scope: ScopeKey = DEFAULT_SCOPE

    def __post_init__(self) -> None:
        # an empty partition column set has no partition identity to key or slice on, so it refuses at construction.
        if not self.partition_by:
            raise ValueError("derived snapshot requires at least one partition column")

    async def refresh(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        # `QueryEngine.run` is a coroutine, so the fence is `async_boundary`; `.bind(lambda rail: rail)` self-flattens the
        # nested rail the wrapped thunk returns, never a second fault fence. The refresh span parents every per-partition
        # recompute's query span — in-process composition correlates parent-child, so no add_link rides the receipts.
        with _TRACER.start_as_current_span("derived.refresh", attributes={"rasm.materialize.partitions": len(prior)}):
            railed = await async_boundary("derived.refresh", lambda: self._materialize(source, start, end, prior))
            return railed.bind(lambda rail: rail)

    async def _materialize(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        if source.table_format is not TableFormat.DELTA:
            return Error(BoundaryFault(boundary=("derived.refresh", f"{source.table_format.value} carries no load_cdf")))
        # CDF scan blocks on the native Delta reader, so it crosses the thread band instead of stalling the loop.
        cdf = pa.table(await on_thread(lambda: DeltaTable(source.table_uri).load_cdf(starting_version=start, ending_version=end).read_all()))
        # ONE key-sorted pass splits the feed over EVERY CDF record — deletes included, so a delete-only partition
        # still reaches recomputation — adjacent runs over the sorted key tuples bound each partition and every delta
        # is a zero-copy slice, never a fresh full-table filter per partition; each slice then keeps only its
        # `_POST_STATE` rows, so a fully-deleted partition recomputes over an empty input and overrides its stale
        # prior bundle instead of carrying it forever.
        ordered = cdf.sort_by([(col, "ascending") for col in self.partition_by])
        tuples = list(zip(*(ordered.column(col).to_pylist() for col in self.partition_by), strict=True))
        runs = tuple((key, sum(1 for _ in members)) for key, members in groupby(tuples))
        offsets = tuple(accumulate((count for _key, count in runs), initial=0))[:-1]
        deltas = tuple(
            (ordered.slice(offset, count).filter(pc.field("_change_type").isin(_POST_STATE)), self._key_id(key))
            for (key, count), offset in zip(runs, offsets, strict=True)
        )
        # independent recomputes drain as bare units under the owner's lane — capacity, deadline, cancellation, and the
        # drain receipt arrive from the fabric instead of a page-local task-group rig; any casualty fails the refresh
        # closed with the combined aggregate, because a snapshot that merges survivors over stale priors is mixed-version.
        receipt = await self.lane.drain(Block.of_seq([Admit(bare=partial(self._recompute, delta, partition)) for delta, partition in deltas]))
        return (
            Error(receipt.faults.reduce(BoundaryFault.combine))
            if not receipt.faults.is_empty()
            else Ok(self._merge(prior, receipt.values))
        )

    def _key_id(self, key: tuple[object, ...]) -> str:
        # canonical JSON of the component tuple keeps the composite id injective on BOTH ambiguity axes — separator
        # (("a/b", "c") vs ("a", "b/c")) and component type ((1, "2") vs ("1", 2)) — one codec, no hand-rolled scheme.
        return msgspec.json.encode(key).decode()

    def _merge(self, prior: tuple[PartitionBundle, ...], bundles: "Block[PartitionBundle]") -> tuple[PartitionBundle, ...]:
        # recomputed bundles override prior; unchanged prior carries its `ContentKey` by reference; the emit sorts by partition.
        merged = {b.partition: b for b in prior} | {b.partition: b for b in bundles}
        return tuple(merged[partition] for partition in sorted(merged))

    async def _recompute(self, delta: pa.Table, partition: str) -> "RuntimeRail[PartitionBundle]":
        # a query fault PROPAGATES — keying the raw delta in place of the transform's output lands untransformed
        # rows under a materialized identity; `arrow_bytes` is the imported `columnar` public fold, never a re-spelled
        # serialization.
        railed = await QueryEngine.of({"delta": delta}).run(self.transform)
        return railed.bind(
            lambda result: ContentIdentity.of("partition", arrow_bytes(result)).map(
                lambda key: PartitionBundle(partition=partition, rows=result.num_rows, content_key=key)
            )
        ).bind(lambda bundle: Hooks.fire(REFRESH_POINT.id, bundle, scope=self.scope))


def snapshot_key(bundles: tuple[PartitionBundle, ...]) -> "RuntimeRail[ContentKey]":
    # children hash in partition-sorted order, so identical content yields one key regardless of completion order.
    ordered = sorted(bundles, key=lambda b: b.partition)
    return ContentIdentity.of("derived-snapshot", tuple(b.content_key for b in ordered))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
