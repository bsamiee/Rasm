# [PY_DATA_MATERIALIZE]

The incremental CDC-materialization owner, the composing concern above the engines it composes: `DerivedSnapshot` folds the `tabular/lakehouse#LAKEHOUSE` change feed, the `tabular/query#QUERY` engine, and runtime `ContentIdentity` into a partition-delta recompute, and `PartitionBundle` is the per-partition content-keyed Arrow bundle. All three composition edges point strictly down the folder order, keeping `columnar` a pure base with zero back-edges.

Only CDF-changed partitions recompute — an unchanged partition's content key is reused untouched, and a full re-scan is the rejected form. Each touched bundle keys through `columnar`'s public `arrow_bytes` fold, and the parent `snapshot_key` Merkle-folds the child keys. The change feed reads through the same `deltalake.load_cdf` surface the `lakehouse` `ChangeFeed` op binds; the `lakehouse` `_PORTABLE` row proves `changefeed` is Delta-exclusive, so the non-Delta reject here is table-derived law.

## [01]-[INDEX]

- [01]-[MATERIALIZE]: the `DerivedSnapshot`/`PartitionBundle` partition-delta recompute over the change feed and the query engine.

## [02]-[MATERIALIZE]

- Auto: `load_cdf` returns an `arro3.core.RecordBatchReader`, and the arro3 frame carries no `pyarrow` grouped-aggregate or compute surface — so the zero-copy `pa.table(...)` PyCapsule re-import is the load-bearing hop before the distinct-tuple, predicate, and filter planes, never a direct `pc` call on the arro3 frame. A non-Delta source returns `Error(BoundaryFault(...))` directly rather than raising into the fence re-key, so the typed fault key survives. The recompute stays lazy: the engine pushes the delta predicate into the DuckDB relation rather than materializing the full table.
- Growth: a new transform is a different `QuerySpec`; a new partition strategy is one `partition_by` tuple; a second CDF source is one arm on the same recompute plane through the `lakehouse` `TableFormat` axis, zero change here.
- Boundary: composes the `lakehouse` `ChangeFeed` op, the `query` engine, and the `columnar` `arrow_bytes` fold, never re-minting any; no durable derived store, no parallel materialization module, no second CDF reader.

```python signature
import operator

import anyio
import pyarrow as pa
import pyarrow.compute as pc
from anyio.streams.memory import MemoryObjectSendStream
from collections.abc import Iterable
from deltalake import DeltaTable
from expression import Error
from expression.collections import Block
from functools import reduce
from msgspec import Struct

from rasm.data.tabular.columnar import arrow_bytes
from rasm.data.tabular.lakehouse import Lakehouse, TableFormat
from rasm.data.tabular.query import QueryEngine, QuerySpec
from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, async_boundary, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt


class PartitionBundle(Struct, frozen=True):
    partition: str
    rows: int
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        return (Receipt.of("derived-snapshot", ("emitted", self.partition, {"rows": self.rows})),)


class DerivedSnapshot(Struct, frozen=True):
    partition_by: tuple[str, ...]
    transform: QuerySpec

    async def refresh(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        # `QueryEngine.run` is a coroutine, so the fence is `async_boundary`; `.bind(lambda rail: rail)` self-flattens the
        # nested rail the wrapped thunk returns, never a second fault fence.
        railed = await async_boundary("derived.refresh", lambda: self._materialize(source, start, end, prior))
        return railed.bind(lambda rail: rail)

    async def _materialize(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        if source.table_format is not TableFormat.DELTA:
            return Error(BoundaryFault(boundary=("derived.refresh", f"{source.table_format.value} carries no load_cdf")))
        cdf = pa.table(DeltaTable(source.table_uri).load_cdf(starting_version=start, ending_version=end).read_all())
        touched = cdf.filter(pc.field("_change_type") != "delete")
        keys = list(self.partition_by)
        changed = touched.group_by(keys).aggregate([]).to_pylist()
        deltas = tuple((touched.filter(self._key_mask(row)), self._key_id(row)) for row in changed)
        rails = await self._gathered(deltas)
        return traversed(rails, by=Disposition.ABORT).map(lambda bundles: self._merge(prior, bundles))

    def _key_mask(self, row: dict[str, object]) -> "pc.Expression":
        # the conjunction over every `partition_by` column, so a composite partition discriminates by its full key tuple
        # rather than the first column alone.
        return reduce(operator.and_, (pc.field(col) == row[col] for col in self.partition_by))

    def _key_id(self, row: dict[str, object]) -> str:
        return "/".join(str(row[col]) for col in self.partition_by)

    async def _gathered(self, deltas: tuple[tuple[pa.Table, str], ...]) -> "Block[RuntimeRail[PartitionBundle]]":
        # independent recomputes run concurrently under one task group (the failure boundary) streaming rails into a bounded
        # inbox — never `asyncio.gather`; downstream folds are order-independent because `_merge` sorts by partition.
        send, receive = anyio.create_memory_object_stream["RuntimeRail[PartitionBundle]"](max_buffer_size=len(deltas))

        async def run(delta: pa.Table, partition: str, sink: "MemoryObjectSendStream[RuntimeRail[PartitionBundle]]") -> None:
            async with sink:
                await sink.send(await self._recompute(delta, partition))

        async with anyio.create_task_group() as group, send:
            for delta, partition in deltas:
                group.start_soon(run, delta, partition, send.clone())
        return Block.of_seq([rail async for rail in receive])

    def _merge(self, prior: tuple[PartitionBundle, ...], bundles: "Block[PartitionBundle]") -> tuple[PartitionBundle, ...]:
        # recomputed bundles override prior; unchanged prior carries its `ContentKey` by reference; the emit sorts by partition.
        merged = {b.partition: b for b in prior} | {b.partition: b for b in bundles}
        return tuple(merged[partition] for partition in sorted(merged))

    async def _recompute(self, delta: pa.Table, partition: str) -> "RuntimeRail[PartitionBundle]":
        # reuse-on-fault: a query fault reuses the prior `delta` rather than propagating; `arrow_bytes` is the imported
        # `columnar` public fold, never a re-spelled serialization.
        result = (await QueryEngine.of({"delta": delta}).run(self.transform)).default_value(delta)
        return ContentIdentity.of("partition", arrow_bytes(result)).map(
            lambda key: PartitionBundle(partition=partition, rows=result.num_rows, content_key=key)
        )


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
