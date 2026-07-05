# [PY_DATA_MATERIALIZE]

The incremental CDC-materialization owner — the composing concern ABOVE the engines it composes: `DerivedSnapshot` folds the `tabular/lakehouse#LAKEHOUSE` change feed, the `tabular/query#QUERY` engine, and runtime `ContentIdentity` into a partition-delta recompute, and `PartitionBundle` is the per-partition content-keyed Arrow bundle. This page is the reason `columnar` is a pure base: the derived view composes the `Lakehouse` owner for the source `table_uri` identity, reads the Change Data Feed between two snapshot versions through the same `deltalake.load_cdf` surface the `lakehouse` `ChangeFeed` op binds, derives the changed-partition set from the CDF `_commit_version` range, routes only the changed rows through the awaitable `QueryEngine`, keys each touched bundle through `columnar`'s PUBLIC `arrow_bytes` canonical whole-table fold, and Merkle-folds the parent `snapshot_key` over the child keys — an unchanged partition's content key is reused untouched, and a full re-scan is the deleted form. All three composition edges point strictly DOWN the `[00]` order (`columnar` < `lakehouse` < `query` < `materialize`), so the page carries the folder's CDC discipline with zero back-edges.

## [01]-[INDEX]

- [01]-[MATERIALIZE]: the `DerivedSnapshot`/`PartitionBundle` partition-delta recompute over the lakehouse change feed and the query engine, content-keyed per partition through the public `arrow_bytes` fold and Merkle-folded to one `snapshot_key`.

## [02]-[MATERIALIZE]

- Owner: `DerivedSnapshot` — the one incremental CDC-materialization owner folding the `lakehouse` change feed, the `query` engine, and `ContentIdentity` into a partition-delta recompute; `PartitionBundle` the per-partition content-keyed Arrow bundle contributing through runtime `ReceiptContributor`. The derived view composes the `lakehouse#LAKEHOUSE` `Lakehouse` owner for the source `table_uri` identity and reads the CDF between two snapshot versions through the one `deltalake.load_cdf` surface the `lakehouse` `ChangeFeed` op binds (the `_PORTABLE` row proves `changefeed` is Delta-exclusive, so a non-Delta source is a typed reject here), derives the changed-partition set from the CDF `_commit_version` range, routes only the changed rows through the `query#QUERY` `QueryEngine`, and re-keys only the touched partition bundles — an unchanged partition's content key is reused untouched. A full re-scan, a second CDF reader, and a parallel materialization module are the deleted forms.
- Entry: the awaitable `DerivedSnapshot.refresh` admits the source `Lakehouse`, a `start`/`end` version range, and the prior `tuple[PartitionBundle, ...]` (the `QuerySpec` transform is carried on the owner); it reads the CDF over the `Lakehouse.table_uri`, partitions the CDF frame by the `partition_by` keys, recomputes each changed partition through the awaitable `QueryEngine.run` over the changed rows, and folds one partition-sorted `tuple[PartitionBundle, ...]` (the canonical order the Merkle `snapshot_key` needs to stay timing-invariant) where every untouched partition carries its prior `ContentKey` by reference; the return is a `RuntimeRail[tuple[PartitionBundle, ...]]`. `refresh` is `async` because the `query#QUERY` `QueryEngine.run` it composes is a coroutine that offloads every blocking leg to the `anyio` worker pool — a synchronous `run(...).default_value(...)` call on the coroutine object is the deleted form; the per-partition recomputes are independent, so they run concurrently under one `anyio.create_task_group()` (the failure boundary) streaming their rails into one memory-object-stream inbox before the `traversed` fold, never `asyncio.gather`.
- Auto: `load_cdf` returns an `arro3.core.RecordBatchReader`, so `_materialize` drains it through `.read_all()` to an `arro3.core.Table` and zero-copy re-imports it into a real `pa.Table` through `pa.table(...)` over the Arrow PyCapsule C-stream (`arro3.core.Table` exports `__arrow_c_stream__`, the protocol `pa.table` consumes) before the `Table.group_by(partition_by).aggregate([])` distinct-tuple plane, the `pc.field(col) == value` predicate fold, and `Table.filter` touch it — the arro3 reader carries no `pyarrow` grouped-aggregate or compute surface, so the bridge is the load-bearing hop, never a direct `pc` call on the arro3 frame. The whole materialization is fenced by `async_boundary("derived.refresh", ...)` (the awaitable fault fence, since the body awaits the `QueryEngine.run` coroutine) and self-flattened through `.bind(lambda rail: rail)`, never the synchronous `boundary`; a non-Delta source returns `Error(BoundaryFault(boundary=(...)))` directly rather than raising into the fence re-key, so the typed key survives. The changed-partition set is exactly the distinct `partition_by`-tuple combinations present in the CDF `_change_type`-filtered frame over the `_commit_version` range (the `Table.group_by(partition_by).aggregate([])` distinct-key rows, each delta filtered by the `_key_mask` conjunction over every partition column and identified by the `_key_id` composite string, never the first column alone), so an unchanged partition never re-keys and a composite partition discriminates by its full tuple; the recompute stays lazy — the `QueryEngine.Rel`/`Sql` path pushes the delta predicate into the DuckDB relation rather than materializing the full table — and `_recompute` awaits `QueryEngine.run` then unwraps with `.default_value(delta)` (the reuse-on-fault policy the refresh boundary owns: a query fault reuses the prior delta rather than propagating); each `_recompute` keys by the railed `ContentIdentity.of` over the `columnar` public `arrow_bytes` fold, the per-partition rails gathered concurrently through `_gathered` (one `anyio.create_task_group()` plus a `len(deltas)`-bounded memory-object-stream, order-independent because `_merge` folds by partition into a partition-sorted canonical tuple) and folded through `traversed(..., by=Disposition.ABORT)` into one `RuntimeRail[Block[PartitionBundle]]`, and the parent `snapshot_key` sorts by `partition` before folding the partition `ContentKey`s through the Merkle `tuple[ContentKey, ...]` `ContentIdentity.of` source (itself railed) so identical partition content yields one snapshot key regardless of the `_gathered` completion order — a single changed partition flips the snapshot key while the unchanged children stay byte-stable.
- Receipt: each `PartitionBundle` contributes an emitted-phase `Receipt.of("derived-snapshot", ("emitted", partition, facts))` through `ReceiptContributor` (the two-argument owner-plus-`(Phase, subject, facts)` factory, native-typed `rows` facts the receipts `dict[str, object]` carries without a `str()` coerce); no new receipt rail.
- Packages: `deltalake` (the `load_cdf(starting_version=, ending_version=)` change feed returning an `arro3.core.RecordBatchReader` with the `_change_type`/`_commit_version` CDF columns, composed through `lakehouse`), `arro3-core` (the `RecordBatchReader.read_all` drain to an `arro3.core.Table` the `pa.table(...)` re-import lifts zero-copy), `pyarrow` (`table(...)` the PyCapsule re-import, `Table.group_by(keys).aggregate([])` the distinct partition-tuple rows, `compute.field`/the `Expression` `==`/`&` operators the `_key_mask` conjunction folds, `Table.filter` the per-partition isolation), `tabular/columnar` (`arrow_bytes` the PUBLIC canonical whole-table `combine_chunks().to_batches()[0].serialize()` content-key source — imported, never re-spelled; the fold stays a branch-consumer export the geometry `energy/simulate` and compute render-frame projections also ride), `tabular/lakehouse` (`Lakehouse`/`TableFormat` the source identity and format gate), `tabular/query` (`QueryEngine`/`QuerySpec` the awaited transform), `expression` (`Block.of_seq` the per-partition rail collection, `Error` the typed reject), stdlib `operator`/`functools` (`reduce(operator.and_, ...)` the composite-key predicate conjunction), `anyio` (`create_task_group` the failure boundary the independent per-partition recomputes run under and `create_memory_object_stream` the bounded inbox their rails stream into, never `asyncio.gather`), runtime (`ContentIdentity`/`ContentKey`/`RuntimeRail`/`BoundaryFault`/`async_boundary`/`traversed`/`Disposition`/`ReceiptContributor`/`Receipt`).
- Growth: a new transform is a different `QuerySpec`; a new partition strategy is one `partition_by` tuple; a second source format is the `lakehouse` `TableFormat` axis with zero change here (a future `DUCKLAKE` `table_changes` feed lands as one CDF-source arm the same recompute plane reads); zero new surface.
- Boundary: composes the `lakehouse` `ChangeFeed` op, the `query` `QueryEngine`, and the `columnar` `arrow_bytes` fold, never re-minting any; no full re-scan, no parallel materialization module, no durable derived store, no second CDF reader; a per-partition recompute class family, a re-derived change feed, a re-spelled canonical-bytes serialization where the imported `arrow_bytes` owns it, a synchronous `run(...).default_value(...)` call on the awaitable `QueryEngine.run` coroutine where `_recompute` awaits it, an `asyncio.gather` over the per-partition recomputes where one `anyio.create_task_group()` owns the failure boundary, a `raise ValueError` non-Delta guard re-keyed by the fence where the direct `Error(BoundaryFault(...))` preserves the typed key, a synchronous `boundary` fence where the body awaits a coroutine and `async_boundary` owns the lift, and a `RuntimeRail[ContentKey]` collapsed into a bare `content_key` field where the railed `ContentIdentity.of` threads through `.map`/`traversed` are the deleted forms.

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
        # `QueryEngine.run` is awaitable (the `query#QUERY` owner offloads every blocking leg to the
        # `anyio` worker pool), so the whole materialization is async and the fence is `async_boundary`;
        # `.bind(lambda rail: rail)` self-flattens the `RuntimeRail[RuntimeRail[...]]` the wrapped thunk
        # returns, never a second fault fence.
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
        # the composite-partition predicate: the conjunction of `pc.field(col) == value` over every
        # `partition_by` column, so a multi-column partition discriminates by its full key tuple
        # rather than the first column alone (the deleted single-key `partition_by[0]` grouping).
        return reduce(operator.and_, (pc.field(col) == row[col] for col in self.partition_by))

    def _key_id(self, row: dict[str, object]) -> str:
        return "/".join(str(row[col]) for col in self.partition_by)

    async def _gathered(self, deltas: tuple[tuple[pa.Table, str], ...]) -> "Block[RuntimeRail[PartitionBundle]]":
        # the per-partition recomputes are independent, so they run concurrently under one task group
        # (the failure boundary) and stream their rails into one inbox — never `asyncio.gather`, never a
        # shared mutable list; `traversed(ABORT)` downstream is order-independent (`_merge` folds by partition into a sorted canonical tuple).
        send, receive = anyio.create_memory_object_stream["RuntimeRail[PartitionBundle]"](max_buffer_size=len(deltas))

        async def run(delta: pa.Table, partition: str, sink: "MemoryObjectSendStream[RuntimeRail[PartitionBundle]]") -> None:
            async with sink:
                await sink.send(await self._recompute(delta, partition))

        async with anyio.create_task_group() as group, send:
            for delta, partition in deltas:
                group.start_soon(run, delta, partition, send.clone())
        return Block.of_seq([rail async for rail in receive])

    def _merge(self, prior: tuple[PartitionBundle, ...], bundles: "Block[PartitionBundle]") -> tuple[PartitionBundle, ...]:
        # partition-keyed fold: recomputed bundles override prior, unchanged prior carries its `ContentKey`
        # by reference, and the emit is sorted by `partition` — a canonical order independent of the
        # `_gathered` completion sequence, so the downstream Merkle `snapshot_key` is timing-invariant.
        merged = {b.partition: b for b in prior} | {b.partition: b for b in bundles}
        return tuple(merged[partition] for partition in sorted(merged))

    async def _recompute(self, delta: pa.Table, partition: str) -> "RuntimeRail[PartitionBundle]":
        # reuse-on-fault: a query fault reuses the prior `delta` rather than propagating, the policy the
        # refresh boundary owns; `QueryEngine.run` is awaited (it is a coroutine), then the railed
        # `ContentIdentity.of` threads the resolved `ContentKey` through `.map` into the bundle —
        # `arrow_bytes` is the imported `columnar` public fold, never a re-spelled serialization.
        result = (await QueryEngine.of({"delta": delta}).run(self.transform)).default_value(delta)
        return ContentIdentity.of("partition", arrow_bytes(result)).map(
            lambda key: PartitionBundle(partition=partition, rows=result.num_rows, content_key=key)
        )


def snapshot_key(bundles: tuple[PartitionBundle, ...]) -> "RuntimeRail[ContentKey]":
    # sort by `partition` before the Merkle fold: the child `ContentKey`s hash in a canonical order, so
    # identical partition content yields one snapshot key regardless of the `_gathered` completion order.
    ordered = sorted(bundles, key=lambda b: b.partition)
    return ContentIdentity.of("derived-snapshot", tuple(b.content_key for b in ordered))
```
