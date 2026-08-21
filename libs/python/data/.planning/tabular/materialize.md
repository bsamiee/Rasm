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

# faults-owned scope stamp: `scoped` binds the version and semconv triple, so no page re-spells the pin.
_TRACER: Final = scoped(trace.get_tracer, "rasm.data.tabular.materialize")

# per-format change vocabulary: the discriminant COLUMN and the post-state survivor set a recompute keeps.
# `update_preimage` carries the OLD image beside its postimage twin and a delete carries no surviving row, so only
# only rowed change types feed a recompute — a kept preimage doubles every updated row. A format the lakehouse arms
# but this table does not row refuses TYPED, because filtering an unknown discriminant yields zero rows and
# publishes a silently empty recompute over a real feed; a second CDF format is one row carrying its own spelling.
_CHANGE_STATE: Final[Map[TableFormat, tuple[str, tuple[str, ...]]]] = Map.of_seq([
    (TableFormat.DELTA, ("_change_type", ("insert", "update_postimage"))),
    # DuckLake spells the discriminant `change_type` without the Delta underscore and emits the same four states, so
    # survivors stay the POST-state pair here too: `update_preimage` and `delete` describe rows LEAVING the partition,
    # which a recompute reads off the survivors it keeps rather than off the records that left.
    (TableFormat.DUCKLAKE, ("change_type", ("insert", "update_postimage"))),
])


# the metric segment this owner's series, its lifted evidence columns, and its metered surface all derive from,
# spelled once so a rename cannot strand a series under one name and a durable row under another.
DOMAIN: Final[str] = "materialize"

# --- [ERRORS] ---------------------------------------------------------------------------


def _refresh_raises() -> Catch:
    # the refresh fence spans the whole materialization: the CDF read and the per-partition recompute both run
    # beneath composed owners that already fence their own providers, so what reaches here is the Arrow slicing and
    # sorting over the fed frame plus the lane's own cancellation surface.
    return (pa.ArrowException, OSError)


# this module's whole raise roster. The empty partition set and the unrowed change format are TERMINAL admission
# defects a re-issue reproduces; the refresh fence is TRANSIENT, since a CDF read and a recompute may clear.
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
        # merge-side metric: recomputed row volume lands on the metric spine under domain="materialize"; the partition
        # id stays receipt-only — unbounded cardinality never becomes a metric dimension. The metric rides the
        # generator's own advance, exactly as every sibling `contribute` emits, so a receipt built and discarded
        # records nothing and a drained one records once. `domain`/`kind`/`key` are the lifted evidence contract the
        # `tabular/lakehouse#LAKEHOUSE` residence reads — the SAME pair handed `Metrics.record` beside the minted
        # key — so a stored row rejoins the series its live twin emitted and a cost slot reconstructs from it.
        Metrics.record({"rasm.materialize.rows": float(self.rows)}, domain=DOMAIN, kind="cdc")
        yield Receipt.of(
            "derived-snapshot",
            ("emitted", self.partition, {"domain": DOMAIN, "kind": "cdc", "key": self.content_key.hex, "rows": self.rows}),
        )


def _metered(bundles: "Block[PartitionBundle]") -> "Block[MeterFact]":
    # ONE `RECORD` fact per refresh over the rows this drain actually recomputed: a fact per partition keys the
    # composite id — unbounded cardinality — onto `surface`, which is exactly the slot the receipt already keeps
    # partition-free. A refresh whose feed touched nothing recomputes zero rows and meters nothing, so an idle
    # cadence lands no row rather than one zero row per tick on a plane that never sheds.
    recomputed = sum(bundle.rows for bundle in bundles)
    return Block.of_seq((MeterFact(resource=Resource.RECORD, quantity=recomputed, surface=DOMAIN),) if recomputed else ())


# late-attach replay edge: every recomputed bundle fires the REPLAY row inside its composition scope.
# the retained DEPTH rides the retaining arm alone: `Modality(replay=64)` is one case payload where the retired
# `Modality.REPLAY, buffer=64` pair was two coordinated fields a non-retaining row could also spell.
REFRESH_POINT: Final[HookPoint[PartitionBundle]] = HookPoint(
    id=DataHook.MATERIALIZE_REFRESH, payload=PartitionBundle, modality=Modality(replay=64)
)

# this package deposits its install receipt under this ledger key; a support-bundle capsule reads an ABSENT row as the
# diagnosis that the data leg never ran, so one constant carries the name rather than a literal at the deposit site.
OWNER: Final[str] = "data.tabular"


class DataInstall(Struct, frozen=True, gc=False):
    # composition-time proof this package's WHOLE point roster landed in the caller's composition — the ids now
    # deliverable there, flat native scalars alone so the capsule renders the row through `structs.asdict` with no
    # nested mapping to breach its depth-walking redaction. Handing the registry's own `HookPoint` rows back instead
    # leaks a `type[Struct]` field no receipt projection renders and names the registry's product rather than this
    # package's admission.
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
    # ONE gated transition claims the whole roster: `Hooks.register` swaps the point table only past its last admitted
    # row and reports every breach together, so a duplicate or malformed id leaves custody exactly as it stood. The
    # short-circuiting fold this replaces stopped at the FIRST breach with every prior point already mounted and no
    # accumulated diagnosis — a half-mount no retire verb is owed against, and the exact alternative the registry's
    # roster arm exists to delete. The deposit passes its receipt through, so the install IS the rail's terminal.
    return Hooks.register(DATA_HOOK_POINTS, scope=scope).map(
        lambda points: Hooks.installed(OWNER, DataInstall(points=tuple(point.id for point in points)), scope=scope)
    )


class DerivedSnapshot(Struct, frozen=True):
    partition_by: tuple[str, ...]
    transform: QuerySpec
    # `generation` binds the recompute's `QueryEngine` to one admitted backend contract, arriving at the
    # composition root exactly as `lane` does: a per-partition engine minted without one carries no contract
    # generation, and merged bundles share a version only when every recompute reads the same one.
    generation: BackendGeneration
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner,
    # and a consumer hands `refresh` only operation inputs while capacity, deadline, and cancellation ride this binding.
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
        # an empty partition column set has no partition identity to key or slice on, so admission refuses it on
        # exactly the rail every sibling owner admits through: a construction-time `raise` crosses the one
        # boundary this branch fences nowhere, since composition roots build the owner outside every fence.
        if not partition_by:
            return Error(SNAPSHOT_PARTITION.raised())
        return Ok(cls(partition_by=partition_by, transform=transform, generation=generation, lane=lane, scope=scope))

    @beartype(conf=FAULT_CONF)
    async def refresh(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        # `QueryEngine.run` is a coroutine, so the fence is `async_boundary`; `.bind(lambda rail: rail)` self-flattens the
        # nested rail the wrapped thunk returns, never a second fault fence. The refresh span parents every per-partition
        # recompute's query span — in-process composition correlates parent-child, so no add_link rides the receipts.
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
        # `LakeOp.ChangeFeed` is the LAKEHOUSE owner's op: composing `run_async` reads its reach matrix, its commit span,
        # its band hop, and its receipt payload, where re-opening `DeltaTable` here forks a second CDF reader behind
        # an owner that already holds one, and strands every non-Delta feed that owner arms. A rowed format's
        # changefeed arm always carries the frame, so the payload is total over the vocabularies this table admits.
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
        # ONE key-sorted pass splits the feed over EVERY CDF record — deletes included, so a delete-only partition
        # still reaches recomputation — adjacent runs over the sorted key tuples bound each partition and every delta
        # is a zero-copy slice, never a fresh full-table filter per partition; each slice then keeps only its rowed
        # survivor states, so a fully-deleted partition recomputes over an empty input and overrides its stale
        # prior bundle instead of carrying it forever.
        column, survivors = vocabulary
        ordered = cdf.sort_by([(col, "ascending") for col in self.partition_by])
        tuples = list(zip(*(ordered.column(col).to_pylist() for col in self.partition_by), strict=True))
        runs = tuple((key, sum(1 for _ in members)) for key, members in groupby(tuples))
        offsets = tuple(accumulate((count for _key, count in runs), initial=0))[:-1]
        deltas = tuple(
            (ordered.slice(offset, count).filter(pc.field(column).isin(survivors)), self._key_id(key))
            for (key, count), offset in zip(runs, offsets, strict=True)
        )
        # independent recomputes drain as bare units under the owner's lane — capacity, deadline, cancellation, and the
        # drain receipt arrive from the crossing instead of a page-local task-group rig; any casualty fails the refresh
        # closed with the combined aggregate, because a snapshot that merges survivors over stale priors is mixed-version.
        receipt = await self.lane.drain(Block.of_seq([Admit(bare=partial(self._recompute, delta, partition)) for delta, partition in deltas]))
        if not receipt.faults.is_empty():
            return Error(receipt.faults.reduce(BoundaryFault.combine))
        # durable evidence for the refresh, seated where the recomputed set is whole and the merge has not yet folded
        # prior bundles back in: metering `_merge`'s output would charge every unchanged partition again on every
        # drain. ONE `RECORD` fact carries the refresh's own recomputed rows — a fact per partition keys a composite
        # of unbounded cardinality onto `surface`, exactly what the receipt already refuses for the metric — and a
        # refresh that recomputed nothing meters nothing rather than a zero row per empty feed.
        return (await Journal.record(_metered(receipt.values), scope=self.scope)).map(lambda _landed: self._merge(prior, receipt.values))

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
        # rows under a materialized identity; `arrow_bytes` is the imported `interop` folder fold, never a re-spelled
        # serialization. The engine binds the owner's admitted `generation` beside the named delta frame, so every
        # partition of one refresh reads one contract generation.
        railed = await QueryEngine.of(self.generation, {"delta": delta}).run(self.transform)
        return railed.bind(
            lambda result: ContentIdentity.of("partition", arrow_bytes(result)).map(
                lambda key: PartitionBundle(partition=partition, rows=result.num_rows, content_key=key)
            )
        ).bind(lambda bundle: Hooks.fire(REFRESH_POINT.id, bundle, scope=self.scope).map(lambda _fact: bundle))


def snapshot_key(bundles: tuple[PartitionBundle, ...]) -> "RuntimeRail[ContentKey]":
    # children hash in partition-sorted order, so identical content yields one key regardless of completion order.
    ordered = sorted(bundles, key=lambda b: b.partition)
    return ContentIdentity.of("derived-snapshot", tuple(b.content_key for b in ordered))
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
