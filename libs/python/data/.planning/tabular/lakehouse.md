# [PY_DATA_LAKEHOUSE]

Table-format interchange crosses one `LakeOp` operation axis with one `TableFormat` provider axis on one `Lakehouse` owner over Delta, Iceberg, Lance, DuckLake, and the non-transactional Parquet tree. `Lakehouse.run` folds the ensure/write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore/reference lifecycle through the `LakeOp` tagged union and dispatches one `(TableFormat, tag)` arm to a `RuntimeResult[LakeResult]` — the operation axis format-agnostic, the format binding a separate discriminant, so a new format is one `TableFormat` row and its arms, never a parallel Iceberg or Lance owner, and formats reaching fewer operations state that as `_REFUSAL` rows. `Lakehouse` commits and reads snapshots over the provider surface; it holds no durable store.

Time travel is one vocabulary both directions of the axis read: `Read`/`Restore` consume a generation, instant, or named ref, and `Reference` authors names. `Ancestry` projects provider history into `Generation` rows, while change feeds leave on `LakeResult.payload` for materialization. Mutations ride the `RetryClass.LAKE_COMMIT` envelope, project file churn through `Metrics`, and record durable audit and storage facts through `Journal` on the caller plane.

## [01]-[INDEX]

- [02]-[LAKEHOUSE]: `Lakehouse` crosses one `LakeOp` operation axis with one `TableFormat` provider axis and returns `LakeResult`.

## [02]-[LAKEHOUSE]

- Owner: `Lakehouse` over the `LakeOp` operation axis (a `tagged_union` matched by `match (self.table_format, op)`) and the `TableFormat` `StrEnum` provider axis, dispatched one `(format, tag)` arm — two orthogonal discriminants, so a new operation is one `LakeOp` case and a new format one `TableFormat` row, never a `read_delta`/`write_delta`/`delete_delta` method family and never a parallel `IcebergLakehouse`/`LanceLakehouse` pair. Writer tuning rides one `WriteTuning` policy `Struct` carried on `Write`, never a parallel `WriteTuned` op or a knob tail; the merge delete-on-no-match rides one `delete_unmatched` discriminant selecting the third `when_not_matched_by_source_delete` clause, never a `MergeDelete` op.
- Owner: `TableLayout` states the authored table spec as DATA — schema, `PartitionTransform`-keyed partition pairs, sort order, properties — and each format arm projects that one declaration onto its own grammar (`bucket[16]` tokens at the iceberg catalog, `bucket(16, col)` SQL at ducklake, bare columns at delta), so a second store arms through the same row shape and a transform a format cannot spell refuses by name rather than vanishing.
- Entry: `open` admits the dataset and provider coordinates. `run` and `run_async` share the reach, fence, hook, retry, and provider dispatch paths, returning the exact provider measurements on `LakeResult`.
- Result: `_snapshot` reads the provider handle already opened by the operation. `_result` combines its version, file churn, byte volume, quantity, unit, matched count, payload, and content key, then projects commit churn to `Metrics`.
- Law: caller-plane commits with file churn record `AuditFact` and `MeterFact` through `Journal`; ledger-plane commits do not recurse into their own journal.
- Law: `quantity` stays result-only, its `LakeUnit` varying per arm, so one descriptor never carries four magnitudes.
- Law: a non-committing op moves no files and records nothing, keeping read and changefeed arms off the commit series.
- Result: `quantity` and `matched` split an upsert's LANDED rows from its REDELIVERED ones — Iceberg answers the pair natively off `UpsertResult.rows_inserted`/`rows_updated`, and the Delta arm reads `num_target_rows_inserted`/`num_target_rows_updated` because `num_output_rows` counts the rewritten output files — inserted, updated, and copied together — and so exceeds the offered batch whenever an untouched row shares a rewritten file; a consumer deriving duplicates by subtracting one fused tally from its own batch length reports zero forever.
- Boundary: no durable store, no schema evolution, no global Delta or catalog connection, no blocking commit run inline on an event loop where `run_async` owns the band hop, and no bare `trace.get_tracer(scope)` beside the faults-owned `scoped` stamp that binds the version and semconv triple; the metadata-only `Read` count is not the read lane — column-projected zero-copy reads route to the `tabular/columnar#SCAN` reader, not this commit owner. Reject law is data: `_REFUSAL` rows every `(format, tag)` cell a provider surface cannot portably reach and `_conditional` rows every cell the op's own operands decide, each row carrying its `LakeRefusal` member as the reason the fault reports, so a reject is a table edit and never an arm spending itself on a sentence. `_reach` reads that matrix ahead of the hook point and the retry envelope, `_apply`'s `case _, _` tail answers an admitted cell no arm executes, and every reject returns `Error(LAKE_REFUSED.raised(...))` carrying the operation beside the typed member — never a silent no-op, never a `raise` into a `boundary` that re-keys and discards it, and never a hand-opened `stamina.retry_context` where `guarded_sync` owns the envelope.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterator
from contextlib import contextmanager
from datetime import UTC, datetime, timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import lance
import pyarrow as pa
from anyio import BusyResourceError, ResourceGuard
import pyarrow.compute as pc
import pyarrow.dataset as pads
from beartype import beartype
from deltalake import (
    BloomFilterProperties,
    ColumnProperties,
    CommitProperties,
    DeltaTable,
    Transaction,
    PostCommitHookProperties,
    QueryBuilder,
    WriterProperties,
    write_deltalake,
)
from deltalake.exceptions import DeltaError
from deltalake.schema import Field
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field
from msgspec.json import decode as json_decode
from opentelemetry import trace

lazy from pyiceberg.catalog import load_catalog
lazy from pyiceberg.exceptions import CommitFailedException, ValidationError as IcebergError
lazy from pyiceberg.transforms import IdentityTransform
lazy from pyiceberg.types import IcebergType

from rasm.data.tabular.columnar import (
    Attach,
    ColumnarEgress,
    DatasetKind,
    DatasetRef,
    DatasetWrite,
    DdlStep,
    DdlVerb,
    DuckDbExtension,
    DuckDbSession,
    Idempotence,
    SecretRow,
    quote_ident,
    quote_literal,
    remote_store,
)
from rasm.data.tabular.interop import ColumnSpec, DataLeg, arrow_bytes, column_frame
from rasm.runtime.faults import (
    FAULT_CONF,
    TERMINAL,
    TRANSIENT,
    BoundaryFault,
    Catch,
    Depth,
    Disposition,
    FaultRow,
    Posture,
    RuntimeResult,
    async_boundary,
    boundary,
    rostered,
    scoped,
    traversed,
)
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, MeterFact, Party, Resource, Retain, Shifted
from rasm.runtime.metrics import Metrics
from rasm.runtime.lanes import on_thread
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded, guarded_sync

if TYPE_CHECKING:
    import duckdb
    from pyiceberg.table import Table

# --- [TYPES] ----------------------------------------------------------------------------

type WriteMode = Literal["error", "append", "overwrite", "ignore"]
type LanceMode = Literal["create", "overwrite", "append"]
type LanceStorage = Literal["stable", "2.1", "next"]
type Compression = Literal["UNCOMPRESSED", "SNAPPY", "GZIP", "BROTLI", "LZ4", "LZ4_RAW", "ZSTD"]
type VectorIndex = Literal["IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"]
type ScalarIndex = Literal["BTREE", "BITMAP", "LABEL_LIST", "ZONEMAP", "BLOOMFILTER", "RTREE", "INVERTED", "FTS", "NGRAM"]
type IndexKind = VectorIndex | ScalarIndex
type Metric = Literal["L2", "cosine", "dot"]
type Evolution = tuple[tuple[tuple[str, str], ...], tuple[str, ...], tuple[tuple[str, str], ...], Map[str, str]]
type Partition = tuple[tuple[str, str, str], ...]
type PartitionTransform = Literal["identity", "year", "month", "day", "hour", "bucket", "truncate"]
_WIDTH_TRANSFORMS: Final[frozenset[PartitionTransform]] = frozenset({"bucket", "truncate"})
type SortDirection = Literal["asc", "desc"]


class TableFormat(StrEnum):
    DELTA = "delta"
    ICEBERG = "iceberg"
    LANCE = "lance"
    DUCKLAKE = "ducklake"
    PARQUET = "parquet"


class LakeUnit(StrEnum):
    ROWS = "rows"
    FILES = "files"
    FRAGMENTS = "fragments"
    SNAPSHOTS = "snapshots"
    NONE = "none"


class Capability(StrEnum):
    ENSURE = "ensure"
    WRITE = "write"
    READ = "read"
    DELETE = "delete"
    UPDATE = "update"
    MERGE = "merge"
    EVOLVE = "evolve"
    OPTIMIZE = "optimize"
    VACUUM = "vacuum"
    CHANGEFEED = "changefeed"
    INDEX = "index"
    RESTORE = "restore"
    REFERENCE = "reference"
    ANCESTRY = "ancestry"


class Fence(Struct, frozen=True):
    app_id: str
    expected: int

    def stale(self, held: int) -> "Option[BoundaryFault]":
        return Nothing if held == self.expected else Some(LAKE_STALE_FENCE.raised(str(self.expected), str(held)))


class Generation(Struct, frozen=True):
    version: int
    parent: "Posture[int]"
    at: "Posture[datetime]"
    refs: tuple[str, ...] = ()


class LakeRefusal(StrEnum):
    DELTA_NO_INDEX = "delta reaches no vector or scalar index surface"
    DELTA_COLUMN_SURGERY = "delta alter reaches no portable column drop or rename"
    DELTA_NO_REFERENCE = "delta names no branch or tag; an int version is its whole reference vocabulary"
    DUCKLAKE_NO_REFERENCE = "ducklake catalogs name a snapshot by id or timestamp alone; no ref vocabulary"
    ICEBERG_NO_UPDATE = "pyiceberg reaches no predicate-scoped row update"
    ICEBERG_NO_OPTIMIZE = "pyiceberg reaches no rewrite_data_files compaction"
    ICEBERG_NO_CHANGEFEED = "pyiceberg reaches no change-data feed"
    ICEBERG_NO_INDEX = "iceberg reaches no index surface"
    ICEBERG_PARTITION_SPEC = "partition_by is table-spec-owned; author PartitionSpec at create"
    ICEBERG_WRITE_EXISTS = "error mode forbids a write into an existing table"
    ICEBERG_CONSTRAINTS = "constraint governance is delta alter.add_constraint only"
    ICEBERG_REF_ABSENT = "the iceberg table resolves no snapshot under that ref, instant, or head"
    LANCE_NO_EVOLVE = "lance reaches no schema-evolution surface"
    LANCE_NO_CHANGEFEED = "lance reaches no change-data feed"
    LANCE_NO_ZORDER = "lance compaction reaches no z-order clustering"
    DUCKLAKE_NO_EVOLVE = "ducklake reaches no schema-evolution surface"
    DUCKLAKE_NO_INDEX = "ducklake reaches no index surface"
    DUCKLAKE_NO_RESTORE = "ducklake reaches no snapshot restore"
    DUCKLAKE_OPTIMIZE_TUNING = "ducklake file merge reaches no target size or z-order"
    DUCKLAKE_READ_TAG = "ducklake travel takes a version int or a timestamp; no tag vocabulary"
    PARQUET_NO_TRANSACTION = "the parquet tree commits no transaction; a correction rewrites its partition whole"
    PARQUET_NO_EVOLVE = "the parquet tree reaches no alter surface; a widened generation carries its own schema"
    PARQUET_NO_OPTIMIZE = "the parquet tree compacts by rewriting a partition through its own write"
    PARQUET_NO_VACUUM = "object-plane lifecycle owns cold-tail expiry; this format holds no snapshot to expire"
    PARQUET_NO_CHANGEFEED = "the parquet tree records no row-level change feed"
    PARQUET_NO_INDEX = "the parquet tree reaches no index surface beyond its own file statistics"
    PARQUET_NO_TRAVEL = "the parquet tree keeps no version history; every read sees the tree as it stands"
    PARQUET_READ_SCOPE = "the tree count takes no version or predicate; a scoped read routes to the columnar scan"
    PARQUET_CODEC = "the tree writer reaches no lz4-raw codec; the transactional formats alone carry it"
    LANCE_NO_PARTITION_FILTER = "lance compaction reaches no partition predicate; it compacts the dataset whole"
    DUCKLAKE_OPTIMIZE_PARTITION = "ducklake file merge reaches no partition predicate"
    PARQUET_NO_TABLE_SPEC = "the parquet tree records no table object; a generation carries its own schema"
    DELTA_PARTITION_TRANSFORM = "delta partitions on columns; every non-identity transform is iceberg-only"
    DUCKLAKE_PARTITION_TRUNCATE = "ducklake partition functions reach year, month, day, hour, and bucket alone"
    LANCE_NO_TABLE_SPEC = "lance authors no partition spec or sort order; the dataset carries neither"
    PARTITION_TRANSFORM_WIDTH = "bucket and truncate each declare a width; every other transform declares none"
    SORT_ORDER_CATALOG_ONLY = "a declared sort order authors at the iceberg catalog alone"
    UNARMED = "reach admits the cell yet no arm executes it"


class LakePlane(StrEnum):
    CALLER = "caller"
    LEDGER = "ledger"


# --- [MODELS] ---------------------------------------------------------------------------


class WriteTuning(Struct, frozen=True):
    compression: Compression = "ZSTD"
    statistics_truncate_length: int | None = None
    target_file_size: int | None = None
    bloom_columns: tuple[str, ...] = ()
    custom_metadata: Map[str, str] = field(default_factory=lambda: Map.of_seq([]))
    max_commit_retries: int | None = None
    create_checkpoint: bool = True
    cleanup_expired_logs: bool = True
    data_storage_version: LanceStorage = "stable"

    def writer_properties(self) -> WriterProperties:
        return WriterProperties(
            compression=self.compression,
            statistics_truncate_length=self.statistics_truncate_length,
            column_properties={
                column: ColumnProperties(bloom_filter_properties=BloomFilterProperties(set_bloom_filter_enabled=True))
                for column in self.bloom_columns
            }
            or None,
        )

    def commit_properties(self) -> CommitProperties:
        return CommitProperties(custom_metadata=dict(self.custom_metadata.items()) or None, max_commit_retries=self.max_commit_retries)

    def hook_properties(self) -> PostCommitHookProperties:
        return PostCommitHookProperties(create_checkpoint=self.create_checkpoint, cleanup_expired_logs=self.cleanup_expired_logs)


@tagged_union(frozen=True)
class LakeOp:
    tag: Literal[
        "ensure", "write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "index", "restore", "reference"
    ] = tag()
    ensure: "TableLayout" = case()
    write: tuple[WriteMode, tuple[str, ...], bool, WriteTuning] = case()
    read: tuple[int | str | datetime | None, tuple[str, ...], str | None] = case()
    delete: tuple[str] = case()
    update: tuple[str, Map[str, str]] = case()
    merge: tuple[str, Map[str, str], bool] = case()
    evolve: Evolution = case()
    optimize: tuple[int | None, tuple[str, ...], Partition] = case()
    vacuum: tuple[int | None, bool] = case()
    changefeed: tuple[int, int | None] = case()
    index: tuple[str, IndexKind, Metric] = case()
    restore: tuple[int | datetime] = case()
    reference: tuple[Literal["tag", "branch"], str, int | None, bool] = case()
    ancestry: tuple[Depth] = case()

    @property
    def committing(self) -> bool:
        match self:
            case LakeOp(tag="read") | LakeOp(tag="changefeed") | LakeOp(tag="ancestry") | LakeOp(tag="vacuum", vacuum=(_, True)):
                return False
            case LakeOp():
                return True

    @property
    def feeding(self) -> bool:
        match self:
            case LakeOp(tag="write") | LakeOp(tag="merge"):
                return True
            case LakeOp():
                return False

    @staticmethod
    def Ensure(layout: "TableLayout") -> "LakeOp":
        return LakeOp(ensure=layout)

    @staticmethod
    def Ancestry(bound: Depth = Depth(fixpoint=None)) -> "LakeOp":
        return LakeOp(ancestry=(bound,))

    @staticmethod
    def Write(
        mode: WriteMode = "error", partition_by: tuple[str, ...] = (), evolve_schema: bool = False, tuning: WriteTuning = WriteTuning()
    ) -> "LakeOp":
        return LakeOp(write=(mode, partition_by, evolve_schema, tuning))

    @staticmethod
    def Read(version: int | str | datetime | None = None, columns: tuple[str, ...] = (), predicate: str | None = None) -> "LakeOp":
        return LakeOp(read=(version, columns, predicate))

    @staticmethod
    def Delete(predicate: str) -> "LakeOp":
        return LakeOp(delete=(predicate,))

    @staticmethod
    def Update(predicate: str, updates: Map[str, str]) -> "LakeOp":
        return LakeOp(update=(predicate, updates))

    @staticmethod
    def Merge(predicate: str, updates: Map[str, str], delete_unmatched: bool = False) -> "LakeOp":
        return LakeOp(merge=(predicate, updates, delete_unmatched))

    @staticmethod
    def Evolve(
        adds: tuple[tuple[str, str], ...] = (),
        drops: tuple[str, ...] = (),
        renames: tuple[tuple[str, str], ...] = (),
        constraints: Map[str, str] | None = None,
    ) -> "LakeOp":
        return LakeOp(evolve=(adds, drops, renames, constraints if constraints is not None else Map.of_seq([])))

    @staticmethod
    def Optimize(target_size: int | None = None, zorder: tuple[str, ...] = (), partition: Partition = ()) -> "LakeOp":
        return LakeOp(optimize=(target_size, zorder, partition))

    @staticmethod
    def Vacuum(retention_hours: int | None = None, dry_run: bool = True) -> "LakeOp":
        return LakeOp(vacuum=(retention_hours, dry_run))

    @staticmethod
    def ChangeFeed(starting_version: int = 0, ending_version: int | None = None) -> "LakeOp":
        return LakeOp(changefeed=(starting_version, ending_version))

    @staticmethod
    def Index(column: str, kind: IndexKind = "IVF_PQ", metric: Metric = "L2") -> "LakeOp":
        return LakeOp(index=(column, kind, metric))

    @staticmethod
    def Restore(target: int | datetime) -> "LakeOp":
        return LakeOp(restore=(target,))

    @staticmethod
    def Reference(kind: Literal["tag", "branch"], name: str, target: int | None = None, drop: bool = False) -> "LakeOp":
        return LakeOp(reference=(kind, name, target, drop))


class LakeCommit(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    operation: str


class TableLayout(Struct, frozen=True):
    schema: pa.Schema
    partition_by: tuple[tuple[str, PartitionTransform, int | None], ...] = ()
    sort_by: tuple[tuple[str, SortDirection], ...] = ()
    properties: Map[str, str] = field(default_factory=lambda: Map.of_seq([]))

    @property
    def columns(self) -> tuple[str, ...]:
        return tuple(column for column, _transform, _width in self.partition_by)

    @property
    def identity_only(self) -> bool:
        return all(transform == "identity" for _column, transform, _width in self.partition_by)

    @property
    def widths_paired(self) -> bool:
        return all((width is not None) is (transform in _WIDTH_TRANSFORMS) for _column, transform, width in self.partition_by)

    def iceberg_fields(self) -> "Block[tuple[str, str]]":
        return Block.of_seq(
            (column, transform if width is None else f"{transform}[{width}]") for column, transform, width in self.partition_by
        )

    def ducklake_terms(self) -> "Block[str]":
        return Block.of_seq(
            quote_ident(column)
            if transform == "identity"
            else f"{transform}({quote_ident(column)})"
            if width is None
            else f"{transform}({width}, {quote_ident(column)})"
            for column, transform, width in self.partition_by
        )

    def empty(self) -> pa.Table:
        return self.schema.empty_table()


LAKE_COMMIT_POINT: Final[HookPoint[LakeCommit]] = HookPoint(id="rasm.data.lakehouse.commit", payload=LakeCommit, modality=Modality(veto=None))

OWNER: Final[str] = "lakehouse"
DOMAIN: Final[str] = "lake"


class LakeResult(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    operation: str
    version: "Option[int]"
    files_added: "Option[int]"
    files_removed: "Option[int]"
    byte_length: int
    quantity: int
    unit: LakeUnit
    matched: int
    content_key: ContentKey
    payload: pa.Table | None = None


def _evidence(result: LakeResult, plane: LakePlane) -> Block[Fact]:
    churn = result.files_added.default_value(0) + result.files_removed.default_value(0)
    if plane is not LakePlane.CALLER or not churn:
        return Block.empty()
    audited = AuditFact(
        action=f"{DOMAIN}.{result.operation}",
        actor=Party(kind=Actor.SERVICE, key=OWNER),
        target=Party(kind="table", key=result.table_uri),
        retention=Retain.OPERATIONAL,
        change=(
            Assigned(path="/version", next=result.version.map(str).default_value("unarmed")),
            Shifted(path="/files", prior=str(result.files_removed.default_value(0)), next=str(result.files_added.default_value(0))),
        ),
    )
    metered = MeterFact(resource=Resource.STORAGE, quantity=result.byte_length, surface=result.table_uri)
    return Block.of_seq((audited, metered) if result.byte_length else (audited,))


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.tabular.lakehouse")


class Lakehouse(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    kind: DatasetKind
    catalog: str | None = None
    identifier: str | None = None
    dsn: str | None = None
    filesystem: Any | None = None
    secrets: tuple[SecretRow, ...] = ()
    scope: ScopeKey = DEFAULT_SCOPE
    plane: LakePlane = LakePlane.CALLER
    guard: ResourceGuard = field(default_factory=lambda: ResourceGuard("committing"))
    fence: "Option[Fence]" = Nothing

    @classmethod
    @beartype(conf=FAULT_CONF)
    def open(
        cls,
        dataset: DatasetRef,
        table_format: TableFormat = TableFormat.DELTA,
        *,
        catalog: str | None = None,
        identifier: str | None = None,
        dsn: str | None = None,
        secrets: tuple[SecretRow, ...] = (),
        scope: ScopeKey = DEFAULT_SCOPE,
        plane: LakePlane = LakePlane.CALLER,
        fence: "Option[Fence]" = Nothing,
    ) -> "RuntimeResult[Lakehouse]":
        row = _ADMIT[table_format]
        coordinates = {"catalog": catalog, "identifier": identifier, "dsn": dsn}
        missing = tuple(name for name in row.needs if coordinates[name] is None)
        if row.kinds and dataset.kind not in row.kinds:
            return Error(LAKE_ADMIT_KIND.raised(table_format.value, dataset.kind.value, dataset.ref.relative))
        if missing:
            return Error(LAKE_ADMIT_NEEDS.raised(table_format.value, "-".join(missing), dataset.ref.relative))
        return Ok(
            cls(
                table_uri=str(dataset.ref.path),
                table_format=table_format,
                kind=dataset.kind,
                catalog=catalog,
                identifier=identifier,
                dsn=dsn,
                filesystem=remote_store(dataset.ref),
                secrets=secrets,
                scope=scope,
                plane=plane,
                fence=fence,
            )
        )

    @beartype(conf=FAULT_CONF)
    def run(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeResult[LakeResult]":
        subject = self._subject(op)
        with _TRACER.start_as_current_span(subject, attributes=self._dimensions(op)):
            return self._gated(op, data).bind(
                lambda admitted: (
                    self._exclusive(lambda: guarded_sync(RetryClass.LAKE_COMMIT, self._apply, admitted, data, at=LAKE_COMMIT), LAKE_COMMIT)
                    if admitted.committing
                    else boundary(LAKE_READ, lambda: self._apply(admitted, data), catch=_commit_raises())
                ).bind(lambda held: held)
            )

    def _exclusive[T](self, commit: "Callable[[], RuntimeResult[T]]", at: "FaultRow[DataLeg]") -> "RuntimeResult[T]":
        try:
            with self.guard:
                return commit()
        except BusyResourceError:
            return Error(LAKE_CONTENDED.raised(at.point))

    async def _exclusive_async[T](self, commit: "Callable[[], Awaitable[RuntimeResult[T]]]", at: "FaultRow[DataLeg]") -> "RuntimeResult[T]":
        try:
            with self.guard:
                return await commit()
        except BusyResourceError:
            return Error(LAKE_CONTENDED.raised(at.point))

    @beartype(conf=FAULT_CONF)
    async def run_async(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeResult[LakeResult]":
        subject = self._subject(op)
        with _TRACER.start_as_current_span(subject, attributes=self._dimensions(op)):
            match self._gated(op, data):
                case Result(tag="error", error=fault):
                    return Error(fault)
                case Result(tag="ok", ok=admitted):
                    fenced = (
                        await self._exclusive_async(
                            lambda: guarded(RetryClass.LAKE_COMMIT, on_thread, self._apply, admitted, data, at=LAKE_COMMIT), LAKE_COMMIT
                        )
                        if admitted.committing
                        else await async_boundary(LAKE_READ, lambda: on_thread(self._apply, admitted, data), catch=_commit_raises())
                    )
                    match fenced.bind(lambda held: held):
                        case Result(tag="ok", ok=result):
                            return (await Journal.record(_evidence(result, self.plane), scope=self.scope)).map(lambda _landed: result)
                        case refused:
                            return Error(refused.error)
                case unreachable:
                    assert_never(unreachable)

    def _gated(self, op: LakeOp, data: pa.Table | None) -> "RuntimeResult[LakeOp]":
        if op.feeding and data is None:
            return Error(LAKE_PAYLOAD.raised(op.tag))
        return self._reach(op).bind(self._fenced).bind(
            lambda admitted: Hooks.fire(
                LAKE_COMMIT_POINT.id,
                LakeCommit(table_uri=self.table_uri, table_format=self.table_format, operation=admitted.tag),
                scope=self.scope,
            ).map(lambda _fact: admitted)
            if admitted.committing
            else Ok(admitted)
        )

    def _fenced(self, op: LakeOp) -> "RuntimeResult[LakeOp]":
        return self.fence.bind(
            lambda held: held.stale(_transaction_version(self.table_uri, held.app_id)).map(Error)
        ).default_value(Ok(op)) if op.committing else Ok(op)

    def _subject(self, op: LakeOp) -> str:
        return f"lake.{self.table_format}.{op.tag}"

    def _dimensions(self, op: LakeOp) -> dict[str, str]:
        return {"rasm.lake.format": self.table_format.value, "rasm.lake.op": op.tag}

    @beartype(conf=FAULT_CONF)
    def demand(self, capabilities: "Block[Capability]") -> "RuntimeResult[Block[Capability]]":
        return traversed(capabilities.map(self._served), by=Disposition.ACCUMULATE).map(lambda _held: capabilities)

    def _served(self, capability: Capability) -> "RuntimeResult[Capability]":
        return (
            _REFUSAL.try_find((self.table_format, capability.value))
            .map(lambda refusal: Error(LAKE_REFUSED.raised(capability.value, refusal.value)))
            .default_value(Ok(capability))
        )

    def _reach(self, op: LakeOp) -> "RuntimeResult[LakeOp]":
        return (
            _REFUSAL.try_find((self.table_format, op.tag))
            .or_else_with(lambda: _conditional(self.table_format, op))
            .map(lambda refusal: Error(LAKE_REFUSED.raised(op.tag, refusal.value)))
            .default_value(Ok(op))
        )

    def _apply(self, op: LakeOp, data: pa.Table | None) -> "RuntimeResult[LakeResult]":
        match self.table_format, op:
            case TableFormat.DELTA, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)) if DeltaTable.is_deltatable(self.table_uri):
                return self._result(op, added=Posture(declared=0), removed=Posture(declared=0), byte_length=Posture(declared=0))
            case TableFormat.DELTA, LakeOp(tag="ensure", ensure=layout):
                table = DeltaTable.create(
                    self.table_uri,
                    layout.schema,
                    mode="ignore",
                    partition_by=list(layout.columns) or None,
                    configuration=dict(layout.properties.items()) or None,
                )
                return self._result(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="write", write=(mode, partition_by, evolve, tuning)):
                prior = _add_paths(self.table_uri)
                write_deltalake(
                    self.table_uri,
                    data,
                    mode=mode,
                    partition_by=list(partition_by) or None,
                    schema_mode="merge" if evolve else None,
                    target_file_size=tuning.target_file_size,
                    writer_properties=tuning.writer_properties(),
                    commit_properties=_fenced_properties(tuning.commit_properties(), self.fence),
                    post_commithook_properties=tuning.hook_properties(),
                )
                table = DeltaTable(self.table_uri)
                return self._result(op, byte_length=Posture(declared=_added_bytes(table, prior)), handle=table)
            case TableFormat.DELTA, LakeOp(tag="read", read=(version, _columns, predicate)):
                table = DeltaTable(self.table_uri)
                if version is not None:
                    table.load_as_version(version)
                rows = (
                    QueryBuilder().register("t", table).execute(f"SELECT count(*) AS n FROM t WHERE {predicate}").read_all().column("n")[0].as_py()
                    if predicate
                    else table.to_pyarrow_dataset().count_rows()
                )
                actions = pa.table(table.get_add_actions())
                stored = int(pc.sum(actions.column("size_bytes")).as_py() or 0)
                return self._result(op, quantity=rows, unit=LakeUnit.ROWS, byte_length=Posture(declared=stored), handle=table)
            case TableFormat.DELTA, LakeOp(tag="delete", delete=(predicate,)):
                table = DeltaTable(self.table_uri)
                metrics = table.delete(predicate)
                deleted = _delta_metric(metrics, "num_deleted_rows").option().default_value(0)
                return self._result(op, quantity=deleted, unit=LakeUnit.ROWS, handle=table)
            case TableFormat.DELTA, LakeOp(tag="update", update=(predicate, updates)):
                table = DeltaTable(self.table_uri)
                metrics = table.update(updates=dict(updates.items()), predicate=predicate)
                updated = _delta_metric(metrics, "num_updated_rows").option().default_value(0)
                return self._result(op, quantity=updated, unit=LakeUnit.ROWS, handle=table)
            case TableFormat.DELTA, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                clauses = dict(updates.items())
                table = DeltaTable(self.table_uri)
                merger = table.merge(data, predicate=predicate).when_matched_update(updates=clauses).when_not_matched_insert(updates=clauses)
                metrics = (merger.when_not_matched_by_source_delete() if delete_unmatched else merger).execute()
                return self._result(
                    op,
                    quantity=_delta_metric(metrics, "num_target_rows_inserted").option().default_value(0),
                    unit=LakeUnit.ROWS,
                    matched=_delta_metric(metrics, "num_target_rows_updated").option().default_value(0),
                    handle=table,
                )
            case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(adds, _drops, _renames, constraints)):
                table = DeltaTable(self.table_uri)
                if adds:
                    table.alter.add_columns([Field(name, dtype) for name, dtype in adds])
                if constraints:
                    table.alter.add_constraint(dict(constraints.items()))
                return self._result(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="optimize", optimize=(target_size, zorder, partition)):
                table = DeltaTable(self.table_uri)
                filters = [(column, operator, value) for column, operator, value in partition] or None
                (
                    table.optimize.z_order(list(zorder), partition_filters=filters, target_size=target_size)
                    if zorder
                    else table.optimize.compact(partition_filters=filters, target_size=target_size)
                )
                return self._result(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                table = DeltaTable(self.table_uri)
                removed = table.vacuum(retention_hours=retention_hours, dry_run=dry_run)
                return self._result(op, removed=Posture(declared=len(removed)), quantity=len(removed), unit=LakeUnit.FILES, handle=table)
            case TableFormat.DELTA, LakeOp(tag="changefeed", changefeed=(start, end)):
                table = DeltaTable(self.table_uri)
                feed = pa.table(table.load_cdf(starting_version=start, ending_version=end).read_all())
                return self._result(op, quantity=feed.num_rows, unit=LakeUnit.ROWS, payload=feed, handle=table)
            case TableFormat.DELTA, LakeOp(tag="restore", restore=(target,)):
                table = DeltaTable(self.table_uri)
                metrics = table.restore(target)
                return self._result(
                    op,
                    added=_delta_metric(metrics, "numRestoredFile"),
                    removed=_delta_metric(metrics, "numRemovedFile"),
                    quantity=_delta_metric(metrics, "numRestoredFile").option().default_value(0),
                    unit=LakeUnit.FILES,
                    handle=table,
                )
            case TableFormat.ICEBERG, LakeOp(tag="ensure", ensure=layout):
                table = load_catalog(self.catalog).create_table_if_not_exists(
                    self.identifier, layout.schema, properties=dict(layout.properties.items())
                )
                if layout.partition_by and not table.spec().fields:
                    layout.iceberg_fields().fold(lambda spec, row: spec.add_field(*row), table.update_spec()).commit()
                if layout.sort_by and not table.sort_order().fields:
                    Block.of_seq(layout.sort_by).fold(
                        lambda order, row: order.asc(row[0], IdentityTransform()) if row[1] == "asc" else order.desc(row[0], IdentityTransform()),
                        table.update_sort_order(),
                    ).commit()
                return self._result(op, handle=table.refresh())
            case TableFormat.ICEBERG, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)):
                return self._result(op)
            case TableFormat.ICEBERG, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                txn = self._iceberg().transaction()
                txn.overwrite(data) if mode == "overwrite" else txn.append(data)
                txn.commit_transaction()
                return self._result(op)
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(None, _columns, predicate)):
                with DuckDbSession(extensions=(DuckDbExtension.ICEBERG,), secrets=self.secrets).connect() as con:
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM iceberg_scan({quote_literal(self.table_uri)}){where}").fetchone()[0]
                return self._result(op, quantity=int(rows), unit=LakeUnit.ROWS)
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(version, _columns, predicate)):
                table = self._iceberg()
                match _iceberg_snapshot(table, version):
                    case Option(tag="none"):
                        return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.ICEBERG_REF_ABSENT.value))
                    case Option(tag="some", some=snapshot_id):
                        counted = table.scan(row_filter=predicate or "true", snapshot_id=snapshot_id).count()
                        return self._result(op, quantity=counted, unit=LakeUnit.ROWS, pinned=Posture(declared=snapshot_id), handle=table)
            case TableFormat.ICEBERG, LakeOp(tag="delete", delete=(predicate,)):
                txn = self._iceberg().transaction()
                txn.delete(predicate)
                txn.commit_transaction()
                return self._result(op)
            case TableFormat.ICEBERG, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                txn = self._iceberg().transaction()
                outcome = txn.upsert(data, join_cols=list(updates.keys()))
                txn.commit_transaction()
                return self._result(op, quantity=outcome.rows_inserted, unit=LakeUnit.ROWS, matched=outcome.rows_updated)
            case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(adds, drops, renames, _constraints)):
                with self._iceberg().update_schema() as schema:
                    for name, dtype in adds:
                        schema.add_column(name, IcebergType.model_validate(dtype))
                    for column in drops:
                        schema.delete_column(column)
                    for old, new in renames:
                        schema.rename_column(old, new)
                return self._result(op)
            case TableFormat.ICEBERG, LakeOp(tag="restore", restore=(target,)):
                manage = self._iceberg().manage_snapshots()
                (manage.rollback_to_snapshot(target) if isinstance(target, int) else manage.rollback_to_timestamp(_millis(target))).commit()
                return self._result(op)
            case TableFormat.ICEBERG, LakeOp(tag="reference", reference=(kind, name, target, False)):
                table = self._iceberg()
                match _iceberg_head(table, target):
                    case Option(tag="none"):
                        return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.ICEBERG_REF_ABSENT.value))
                    case Option(tag="some", some=snapshot_id):
                        getattr(table.manage_snapshots(), _ICEBERG_REFERENCE[(kind, False)])(snapshot_id, name).commit()
                        return self._result(op, handle=table.refresh())
            case TableFormat.ICEBERG, LakeOp(tag="reference", reference=(kind, name, _target, True)):
                table = self._iceberg()
                getattr(table.manage_snapshots(), _ICEBERG_REFERENCE[(kind, True)])(name).commit()
                return self._result(op, handle=table.refresh())
            case TableFormat.ICEBERG, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                cutoff = _retention(retention_hours)
                table = self._iceberg()
                aged = sum(1 for committed in table.inspect.snapshots().column("committed_at").to_pylist() if committed < cutoff)
                if not dry_run:
                    table.maintenance.expire_snapshots().older_than(cutoff).commit()
                return self._result(op, quantity=aged, unit=LakeUnit.SNAPSHOTS)
            case TableFormat.LANCE, LakeOp(tag="ensure", ensure=_layout) if _lance_exists(self.table_uri):
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="ensure", ensure=layout):
                lance.write_dataset(layout.empty(), self.table_uri, mode="create")
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)) if _lance_exists(self.table_uri):
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, tuning)):
                lance.write_dataset(
                    data,
                    self.table_uri,
                    mode=_LANCE_MODE[mode],
                    max_rows_per_file=tuning.target_file_size or _LANCE_FRAGMENT_ROWS,
                    data_storage_version=tuning.data_storage_version,
                )
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="read", read=(version, _columns, predicate)):
                ds = _lance_travel(self.table_uri, version)
                return self._result(op, quantity=ds.count_rows(filter=predicate), unit=LakeUnit.ROWS, handle=ds)
            case TableFormat.LANCE, LakeOp(tag="delete", delete=(predicate,)):
                lance.dataset(self.table_uri).delete(predicate)
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="update", update=(predicate, updates)):
                lance.dataset(self.table_uri).update(dict(updates.items()), where=predicate)
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                builder = lance.dataset(self.table_uri).merge_insert(list(updates.keys()))
                builder.when_matched_update_all().when_not_matched_insert_all().execute(data)
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="index", index=(column, kind, metric)):
                ds = lance.dataset(self.table_uri)
                ds.create_index(column, index_type=kind, metric=metric) if kind in _VECTOR_INDEX else ds.create_scalar_index(column, index_type=kind)
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(target_size, _zorder, _partition)):
                metrics = lance.dataset(self.table_uri).optimize.compact_files(target_rows_per_fragment=target_size)
                return self._result(op, quantity=metrics.fragments_added, unit=LakeUnit.FRAGMENTS)
            case TableFormat.LANCE, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                ds = lance.dataset(self.table_uri)
                cutoff = datetime.now(UTC).replace(tzinfo=None) - _age(retention_hours)
                aged = sum(1 for row in ds.versions() if row["timestamp"] < cutoff)
                expired = aged if dry_run else ds.cleanup_old_versions(older_than=_age(retention_hours)).old_versions
                return self._result(op, quantity=expired, unit=LakeUnit.SNAPSHOTS)
            case TableFormat.LANCE, LakeOp(tag="restore", restore=(target,)):
                ds = _lance_travel(self.table_uri, target)
                ds.restore()
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("tag", name, target, False)):
                lance.dataset(self.table_uri).tags.create(name, target)
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("branch", name, target, False)):
                lance.dataset(self.table_uri).create_branch(name, target)
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("tag", name, _target, True)):
                lance.dataset(self.table_uri).tags.delete(name)
                return self._result(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("branch", name, _target, True)):
                lance.dataset(self.table_uri).branches.delete(name)
                return self._result(op)
            case TableFormat.DUCKLAKE, LakeOp(tag="ensure", ensure=layout):
                with self._ducklake(layout.empty()) as (con, table):
                    terms = ", ".join(layout.ducklake_terms())
                    planted = Block.of_seq((
                        DdlStep(
                            verb=DdlVerb.CREATE_TABLE,
                            idempotence=Idempotence.ENSURED,
                            text=f"CREATE TABLE IF NOT EXISTS {table} AS SELECT * FROM payload",
                        ),
                        *(
                            (
                                DdlStep(
                                    verb=DdlVerb.ALTER_TABLE,
                                    idempotence=Idempotence.REPLACED,
                                    text=f"ALTER TABLE {table} SET PARTITIONED BY ({terms})",
                                ),
                            )
                            if terms
                            else ()
                        ),
                    ))
                    for step in planted:
                        step.run(con)
                    return self._result(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                with self._ducklake(data) as (con, table):
                    con.execute(
                        f"CREATE OR REPLACE TABLE {table} AS SELECT * FROM payload"
                        if mode == "overwrite"
                        else f"INSERT INTO {table} SELECT * FROM payload"
                    )
                    return self._result(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="read", read=(version, _columns, predicate)):
                with self._ducklake(data) as (con, table):
                    at = f" AT ({'VERSION' if isinstance(version, int) else 'TIMESTAMP'} => ?)" if version is not None else ""
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM {table}{at}{where}", [version] if version is not None else []).fetchone()[0]
                    return self._result(op, quantity=int(rows), unit=LakeUnit.ROWS, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="delete", delete=(predicate,)):
                with self._ducklake(data) as (con, table):
                    con.execute(f"DELETE FROM {table} WHERE {predicate}")
                    return self._result(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="update", update=(predicate, updates)):
                with self._ducklake(data) as (con, table):
                    assignments = ", ".join(f"{quote_ident(column)} = {expr}" for column, expr in updates.items())
                    con.execute(f"UPDATE {table} SET {assignments} WHERE {predicate}")
                    return self._result(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                with self._ducklake(data) as (con, table):
                    sets = ", ".join(f"{quote_ident(column)} = payload.{quote_ident(column)}" for column in updates.keys())
                    tail = " WHEN NOT MATCHED BY SOURCE THEN DELETE" if delete_unmatched else ""
                    con.execute(
                        f"MERGE INTO {table} USING payload ON {predicate}"
                        f" WHEN MATCHED THEN UPDATE SET {sets} WHEN NOT MATCHED THEN INSERT BY NAME{tail}"
                    )
                    return self._result(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="changefeed", changefeed=(start, end)):
                with self._ducklake(data) as (con, _table):
                    match Option.of_optional(end).or_else(_head(con)):
                        case Option(tag="none"):
                            return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.UNARMED.value))
                        case Option(tag="some", some=bound):
                            feed = con.execute("SELECT * FROM table_changes(?, ?, ?)", [self.identifier, start, bound]).to_arrow_table()
                            return self._result(op, quantity=feed.num_rows, unit=LakeUnit.ROWS, payload=feed, handle=con)
            case _, LakeOp(tag="ancestry", ancestry=(bound,)):
                return self._generations(bound).bind(
                    lambda roster: boundary(
                        LAKE_READ, lambda: column_frame(_GENERATION_COLUMNS, roster), catch=_commit_raises()
                    ).bind(lambda frame: self._result(op, quantity=len(roster), unit=LakeUnit.SNAPSHOTS, payload=frame))
                )
            case TableFormat.DUCKLAKE, LakeOp(tag="optimize"):
                with self._ducklake(data) as (con, _table):
                    DdlStep(
                        verb=DdlVerb.CALL, idempotence=Idempotence.REPLACED, text="CALL ducklake_merge_adjacent_files('lake')"
                    ).run(con)
                    return self._result(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                with self._ducklake(data) as (con, _table):
                    interval = retention_hours or _DEFAULT_RETENTION_HOURS
                    expired = DdlStep(
                        verb=DdlVerb.CALL,
                        idempotence=Idempotence.REPLACED,
                        text="CALL ducklake_expire_snapshots('lake', older_than => now() - to_hours(?), dry_run => ?)",
                        parameters=(interval, dry_run),
                    )
                    expired.run(con)
                    if not dry_run:
                        DdlStep(
                            verb=DdlVerb.CALL,
                            idempotence=Idempotence.REPLACED,
                            text="CALL ducklake_cleanup_old_files('lake', cleanup_all => true)",
                        ).run(con)
                    return self._result(op, handle=con)
            case TableFormat.PARQUET, LakeOp(tag="write", write=(mode, partition_by, _evolve, tuning)):
                return ContentIdentity.of(DOMAIN, arrow_bytes(data)).bind(
                    lambda key: ColumnarEgress.Dataset(
                        self.table_uri,
                        partition_by,
                        _PARQUET_CODEC[tuning.compression],
                        _PARQUET_EXISTING[mode],
                        f"part-{key.hex[:16]}-{{i}}.parquet",
                        self.filesystem,
                    )
                    .emit(data)
                    .bind(
                        lambda landed: self._result(
                            op,
                            key=key,
                            added=Posture(declared=landed.files),
                            byte_length=Posture(declared=landed.byte_length),
                            quantity=data.num_rows,
                            unit=LakeUnit.ROWS,
                        )
                    )
                )
            case TableFormat.PARQUET, LakeOp(tag="read"):
                counted = pads.dataset(self.table_uri, format="parquet", partitioning="hive", filesystem=self.filesystem).count_rows()
                return self._result(op, quantity=counted, unit=LakeUnit.ROWS)
            case _, _:
                return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.UNARMED.value))

    @contextmanager
    def _ducklake(self, data: pa.Table | None) -> "Iterator[tuple[duckdb.DuckDBPyConnection, str]]":
        session = DuckDbSession(
            attach=(Attach(alias="lake", target=f"ducklake:{self.dsn}", kind=DuckDbExtension.DUCKLAKE, current=True),), secrets=self.secrets
        )
        with session.connect() as con:
            if data is not None:
                con.register("payload", data)
            yield con, quote_ident(self.identifier)

    def _iceberg(self) -> "Table":
        return load_catalog(self.catalog).load_table(self.identifier)

    def _generations(self, bound: Depth) -> "RuntimeResult[Block[Generation]]":
        return boundary(LAKE_READ, lambda: self._history(bound), catch=_commit_raises())

    def _history(self, bound: Depth) -> "Block[Generation]":
        match self.table_format:
            case TableFormat.DELTA:
                entries = DeltaTable(self.table_uri).history(_limit(bound))
                return Block.of_seq(
                    Generation(
                        version=int(entry["version"]),
                        parent=Posture(declared=int(entry["version"]) - 1) if int(entry["version"]) else Posture(absent=None),
                        at=Posture.of_option(Option.of_optional(entry.get("timestamp")).map(_instant)),
                    )
                    for entry in entries
                )
            case TableFormat.ICEBERG:
                rows = self._iceberg().inspect.snapshots().to_pylist()
                return _bounded(
                    Block.of_seq(
                        Generation(
                            version=int(row["snapshot_id"]),
                            parent=Posture.of_option(Option.of_optional(row.get("parent_id")).map(int)),
                            at=Posture.of_optional(row.get("committed_at")),
                        )
                        for row in rows
                    ),
                    bound,
                )
            case TableFormat.LANCE:
                rows = lance.dataset(self.table_uri).versions()
                return _bounded(
                    Block.of_seq(
                        Generation(
                            version=int(row["version"]),
                            parent=Posture(declared=int(row["version"]) - 1) if int(row["version"]) > 1 else Posture(absent=None),
                            at=Posture.of_optional(row.get("timestamp")),
                        )
                        for row in rows
                    ),
                    bound,
                )
            case TableFormat.DUCKLAKE | TableFormat.PARQUET:
                with self._ducklake(None) as (con, _table):
                    rows = con.execute("SELECT snapshot_id, parent_id, snapshot_time FROM snapshots()").fetchall()
                return _bounded(
                    Block.of_seq(
                        Generation(
                            version=int(version),
                            parent=Posture.of_option(Option.of_optional(parent).map(int)),
                            at=Posture.of_optional(stamped),
                        )
                        for version, parent, stamped in rows
                    ),
                    bound,
                )
            case unreachable:
                assert_never(unreachable)

    def _snapshot(self, op: LakeOp, handle: Any | None) -> "tuple[Posture[int], Posture[int], Posture[int], int]":
        blank: "tuple[Posture[int], Posture[int], int]" = (Posture(absent=None), Posture(absent=None), 0)
        if op.tag == "read" and isinstance(op.read[0], int):
            return Posture(declared=op.read[0]), *blank
        match self.table_format:
            case TableFormat.DELTA:
                table = handle if handle is not None else DeltaTable(self.table_uri)
                return (Posture(declared=table.version()), *_delta_churn(table, op))
            case TableFormat.ICEBERG:
                history = (handle if handle is not None else self._iceberg()).inspect.snapshots()
                head = Some(history.column("snapshot_id")[-1].as_py()) if history.num_rows else Nothing
                return Posture.of_option(head), *blank
            case TableFormat.LANCE:
                return Posture(declared=(handle if handle is not None else lance.dataset(self.table_uri)).version), *blank
            case TableFormat.DUCKLAKE:
                return Posture.of_option(_head(handle)), *blank
            case TableFormat.PARQUET:
                return Posture(defaulted=(0, "parquet")), *blank
            case unreachable:
                assert_never(unreachable)

    def _result(
        self,
        op: LakeOp,
        *,
        key: ContentKey | None = None,
        quantity: int = 0,
        unit: LakeUnit = LakeUnit.NONE,
        matched: int = 0,
        added: "Posture[int]" = Posture(absent=None),
        removed: "Posture[int]" = Posture(absent=None),
        byte_length: "Posture[int]" = Posture(absent=None),
        pinned: "Posture[int]" = Posture(absent=None),
        payload: pa.Table | None = None,
        handle: Any | None = None,
    ) -> "RuntimeResult[LakeResult]":
        snapshot, churn_added, churn_removed, churn_bytes = self._snapshot(op, handle)
        version = pinned.option().or_else(snapshot.option())
        keyed = f"{self.table_uri}@{version.map(str).default_value('unarmed')}"
        identity = Ok(key) if key is not None else ContentIdentity.of(DOMAIN, keyed.encode())

        def completed(resolved: ContentKey) -> LakeResult:
            result = LakeResult(
                table_uri=self.table_uri,
                table_format=self.table_format,
                operation=op.tag,
                version=version,
                files_added=added.option().or_else(churn_added.option()),
                files_removed=removed.option().or_else(churn_removed.option()),
                byte_length=byte_length.option().default_value(churn_bytes),
                quantity=quantity,
                unit=unit,
                matched=matched,
                content_key=resolved,
                payload=payload,
            )
            churn = result.files_added.default_value(0) + result.files_removed.default_value(0)
            if churn:
                Metrics.record(
                    {
                        "rasm.lake.commit.files_added": float(result.files_added.default_value(0)),
                        "rasm.lake.commit.files_removed": float(result.files_removed.default_value(0)),
                    },
                    domain=DOMAIN,
                    kind=result.operation,
                )
            return result

        return identity.map(completed)


# --- [TABLES] ---------------------------------------------------------------------------

_DEFAULT_RETENTION_HOURS: Final[int] = 168

_LANCE_FRAGMENT_ROWS: Final[int] = 1024 * 1024

_VECTOR_INDEX: Final[frozenset[str]] = frozenset({"IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"})

_ICEBERG_REFERENCE: Final[Map[tuple[str, bool], str]] = Map.of_seq([
    (("tag", False), "create_tag"),
    (("branch", False), "create_branch"),
    (("tag", True), "remove_tag"),
    (("branch", True), "remove_branch"),
])

_LANCE_MODE: Final[Map[WriteMode, LanceMode]] = Map.of_seq([
    ("error", "create"),
    ("ignore", "create"),
    ("overwrite", "overwrite"),
    ("append", "append"),
])

_COMMIT_METRIC: Final[Map[str, tuple[str, str, str | None]]] = Map.of_seq([
    ("write", ("num_added_files", "num_removed_files", None)),
    ("delete", ("num_added_files", "num_removed_files", None)),
    ("update", ("num_added_files", "num_removed_files", None)),
    ("merge", ("num_target_files_added", "num_target_files_removed", None)),
    ("optimize", ("numFilesAdded", "numFilesRemoved", "filesAdded")),
])

# --- [ERRORS] ---------------------------------------------------------------------------


def _commit_raises() -> Catch:
    return (DeltaError, IcebergError, CommitFailedException, duckdb.Error, pa.ArrowException, ValueError, OSError)


LAKE_REFUSED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="reach", arm="boundary", defect="cell-refused", retriability=TERMINAL, slots=("operation", "reason")
)
LAKE_ADMIT_KIND: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="open.kind", arm="config", defect="ref-kind-refused", retriability=TERMINAL, slots=("format", "kind", "ref")
)
LAKE_ADMIT_NEEDS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="open.needs", arm="config", defect="coordinates-absent", retriability=TERMINAL, slots=("format", "missing", "ref")
)
LAKE_PAYLOAD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="gate", arm="config", defect="payload-absent", retriability=TERMINAL, slots=("operation",)
)
LAKE_CONTENDED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="fence", arm="resource", defect="concurrent-commit", retriability=TRANSIENT, slots=("operation",)
)
LAKE_STALE_FENCE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="fence.generation", arm="config", defect="stale-fence", retriability=TERMINAL, slots=("expected", "held")
)
LAKE_COMMIT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="commit", arm="boundary", defect="commit-leg", retriability=TRANSIENT
)
LAKE_READ: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="read", arm="boundary", defect="read-leg", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    LAKE_REFUSED,
    LAKE_ADMIT_KIND,
    LAKE_ADMIT_NEEDS,
    LAKE_PAYLOAD,
    LAKE_CONTENDED,
    LAKE_STALE_FENCE,
    LAKE_COMMIT,
    LAKE_READ,
]))

_REFUSAL: Final[Map[tuple[TableFormat, str], LakeRefusal]] = Map.of_seq([
    ((TableFormat.DELTA, "index"), LakeRefusal.DELTA_NO_INDEX),
    ((TableFormat.DELTA, "reference"), LakeRefusal.DELTA_NO_REFERENCE),
    ((TableFormat.DUCKLAKE, "reference"), LakeRefusal.DUCKLAKE_NO_REFERENCE),
    ((TableFormat.PARQUET, "reference"), LakeRefusal.PARQUET_NO_TRAVEL),
    ((TableFormat.ICEBERG, "update"), LakeRefusal.ICEBERG_NO_UPDATE),
    ((TableFormat.ICEBERG, "optimize"), LakeRefusal.ICEBERG_NO_OPTIMIZE),
    ((TableFormat.ICEBERG, "changefeed"), LakeRefusal.ICEBERG_NO_CHANGEFEED),
    ((TableFormat.ICEBERG, "index"), LakeRefusal.ICEBERG_NO_INDEX),
    ((TableFormat.LANCE, "evolve"), LakeRefusal.LANCE_NO_EVOLVE),
    ((TableFormat.LANCE, "changefeed"), LakeRefusal.LANCE_NO_CHANGEFEED),
    ((TableFormat.DUCKLAKE, "evolve"), LakeRefusal.DUCKLAKE_NO_EVOLVE),
    ((TableFormat.DUCKLAKE, "index"), LakeRefusal.DUCKLAKE_NO_INDEX),
    ((TableFormat.DUCKLAKE, "restore"), LakeRefusal.DUCKLAKE_NO_RESTORE),
    ((TableFormat.PARQUET, "delete"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "update"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "merge"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "evolve"), LakeRefusal.PARQUET_NO_EVOLVE),
    ((TableFormat.PARQUET, "optimize"), LakeRefusal.PARQUET_NO_OPTIMIZE),
    ((TableFormat.PARQUET, "vacuum"), LakeRefusal.PARQUET_NO_VACUUM),
    ((TableFormat.PARQUET, "changefeed"), LakeRefusal.PARQUET_NO_CHANGEFEED),
    ((TableFormat.PARQUET, "index"), LakeRefusal.PARQUET_NO_INDEX),
    ((TableFormat.PARQUET, "restore"), LakeRefusal.PARQUET_NO_TRAVEL),
    ((TableFormat.PARQUET, "ancestry"), LakeRefusal.PARQUET_NO_TRAVEL),
    ((TableFormat.PARQUET, "ensure"), LakeRefusal.PARQUET_NO_TABLE_SPEC),
])

_DUCKLAKE_TRANSFORMS: Final[frozenset[PartitionTransform]] = frozenset({"year", "month", "day", "hour", "bucket"})


class _Admission(Struct, frozen=True):
    kinds: frozenset[DatasetKind] = frozenset()
    needs: tuple[str, ...] = ()


_ADMIT: Final[Map[TableFormat, _Admission]] = Map.of_seq([
    (TableFormat.DELTA, _Admission(kinds=frozenset({DatasetKind.DELTA}))),
    (TableFormat.ICEBERG, _Admission(kinds=frozenset({DatasetKind.ICEBERG}), needs=("catalog", "identifier"))),
    (TableFormat.LANCE, _Admission()),
    (TableFormat.DUCKLAKE, _Admission(needs=("dsn", "identifier"))),
    (TableFormat.PARQUET, _Admission(kinds=frozenset({DatasetKind.PARQUET}))),
])

_PARQUET_EXISTING: Final[Map[WriteMode, DatasetWrite]] = Map.of_seq([
    ("error", "error"),
    ("ignore", "overwrite_or_ignore"),
    ("append", "overwrite_or_ignore"),
    ("overwrite", "delete_matching"),
])

_PARQUET_CODEC: Final[Map[Compression, str]] = Map.of_seq([
    ("UNCOMPRESSED", "none"),
    ("SNAPPY", "snappy"),
    ("GZIP", "gzip"),
    ("BROTLI", "brotli"),
    ("LZ4", "lz4"),
    ("ZSTD", "zstd"),
])

_GENERATION_COLUMNS: Final[Block[ColumnSpec[Generation, object]]] = Block.of_seq([
    ColumnSpec(name="node", arrow=pa.int64(), kind=int, lift=lambda row: row.version),
    ColumnSpec(name="parent", arrow=pa.int64(), kind=int, nullable=True, lift=lambda row: row.parent.option().to_optional()),
    ColumnSpec(name="at", arrow=pa.timestamp("us", tz="UTC"), kind=datetime, nullable=True, lift=lambda row: row.at.option().to_optional()),
    ColumnSpec(name="refs", arrow=pa.list_(pa.string()), kind=tuple, lift=lambda row: list(row.refs)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _conditional(table_format: TableFormat, op: LakeOp) -> Option[LakeRefusal]:
    match table_format, op:
        case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(_adds, drops, renames, _constraints)) if drops or renames:
            return Some(LakeRefusal.DELTA_COLUMN_SURGERY)
        case _, LakeOp(tag="ensure", ensure=layout) if not layout.widths_paired:
            return Some(LakeRefusal.PARTITION_TRANSFORM_WIDTH)
        case _, LakeOp(tag="ensure", ensure=layout) if layout.sort_by and table_format is not TableFormat.ICEBERG:
            return Some(LakeRefusal.SORT_ORDER_CATALOG_ONLY)
        case TableFormat.DELTA, LakeOp(tag="ensure", ensure=layout) if not layout.identity_only:
            return Some(LakeRefusal.DELTA_PARTITION_TRANSFORM)
        case TableFormat.LANCE, LakeOp(tag="ensure", ensure=layout) if layout.partition_by:
            return Some(LakeRefusal.LANCE_NO_TABLE_SPEC)
        case TableFormat.DUCKLAKE, LakeOp(tag="ensure", ensure=layout) if any(
            transform not in _DUCKLAKE_TRANSFORMS and transform != "identity" for _column, transform, _width in layout.partition_by
        ):
            return Some(LakeRefusal.DUCKLAKE_PARTITION_TRUNCATE)
        case TableFormat.ICEBERG, LakeOp(tag="write", write=(_mode, partition_by, _evolve, _tuning)) if partition_by:
            return Some(LakeRefusal.ICEBERG_PARTITION_SPEC)
        case TableFormat.ICEBERG, LakeOp(tag="write", write=("error", _partition_by, _evolve, _tuning)):
            return Some(LakeRefusal.ICEBERG_WRITE_EXISTS)
        case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(_adds, _drops, _renames, constraints)) if constraints:
            return Some(LakeRefusal.ICEBERG_CONSTRAINTS)
        case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(_target_size, zorder, _partition)) if zorder:
            return Some(LakeRefusal.LANCE_NO_ZORDER)
        case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(_target_size, _zorder, partition)) if partition:
            return Some(LakeRefusal.LANCE_NO_PARTITION_FILTER)
        case TableFormat.DUCKLAKE, LakeOp(tag="optimize", optimize=(_target_size, _zorder, partition)) if partition:
            return Some(LakeRefusal.DUCKLAKE_OPTIMIZE_PARTITION)
        case TableFormat.DUCKLAKE, LakeOp(tag="optimize", optimize=(target_size, zorder, _partition)) if target_size is not None or zorder:
            return Some(LakeRefusal.DUCKLAKE_OPTIMIZE_TUNING)
        case TableFormat.DUCKLAKE, LakeOp(tag="read", read=(str(), _columns, _predicate)):
            return Some(LakeRefusal.DUCKLAKE_READ_TAG)
        case TableFormat.PARQUET, LakeOp(tag="read", read=(version, _columns, predicate)) if version is not None or predicate:
            return Some(LakeRefusal.PARQUET_READ_SCOPE)
        case TableFormat.PARQUET, LakeOp(tag="write", write=(_mode, _partition_by, _evolve, tuning)) if (
            _PARQUET_CODEC.try_find(tuning.compression).is_none()
        ):
            return Some(LakeRefusal.PARQUET_CODEC)
        case _, _:
            return Nothing


def _fenced_properties(properties: "CommitProperties | None", fence: "Option[Fence]") -> "CommitProperties | None":
    return fence.map(
        lambda held: CommitProperties(
            custom_metadata=properties.custom_metadata if properties else None,
            max_commit_retries=properties.max_commit_retries if properties else None,
            app_transactions=[Transaction(app_id=held.app_id, version=held.expected + 1)],
        )
    ).default_value(properties)


def _transaction_version(uri: str, app_id: str) -> int:
    return DeltaTable(uri).transaction_version(app_id)


def _limit(bound: Depth) -> int:
    match bound:
        case Depth(tag="bounded", bounded=held):
            return held
        case Depth(tag="fixpoint"):
            return 0
        case _ as unreachable:
            assert_never(unreachable)


def _bounded(roster: "Block[Generation]", bound: Depth) -> "Block[Generation]":
    ordered = roster.sort_with(lambda row: -row.version)
    return ordered if bound.tag == "fixpoint" else ordered.take(min(_limit(bound), len(ordered)))


def _instant(held: object) -> datetime:
    return datetime.fromtimestamp(int(held) / 1000.0, UTC) if isinstance(held, int) else held


def _head(con: "duckdb.DuckDBPyConnection") -> Option[int]:
    row = con.execute("SELECT max(snapshot_id) FROM snapshots()").fetchone()
    return Option.of_optional(row[0]).map(int)


def _lance_exists(uri: str) -> bool:
    try:
        lance.dataset(uri)
    except ValueError:
        return False
    return True


def _lance_travel(uri: str, target: int | str | datetime | None) -> Any:
    return lance.dataset(uri, asof=target) if isinstance(target, datetime) else lance.dataset(uri, version=target)


def _millis(moment: datetime) -> int:
    return int(moment.timestamp() * 1000)


def _iceberg_snapshot(table: "Table", version: int | str | datetime) -> "Option[int]":
    match version:
        case int() as snapshot_id:
            return Some(snapshot_id)
        case str() as named:
            return Option.of_optional(table.snapshot_by_name(named)).map(lambda snapshot: snapshot.snapshot_id)
        case moment:
            return Option.of_optional(table.snapshot_as_of_timestamp(_millis(moment))).map(lambda snapshot: snapshot.snapshot_id)


def _iceberg_head(table: "Table", target: int | None) -> "Option[int]":
    return Some(target) if target is not None else Option.of_optional(table.current_snapshot()).map(lambda snapshot: snapshot.snapshot_id)


def _add_paths(uri: str) -> frozenset[str]:
    return frozenset(pa.table(DeltaTable(uri).get_add_actions()).column("path").to_pylist()) if DeltaTable.is_deltatable(uri) else frozenset()


def _added_bytes(table: "DeltaTable", prior: frozenset[str]) -> int:
    actions = pa.table(table.get_add_actions())
    return sum(
        size
        for path, size in zip(actions.column("path").to_pylist(), actions.column("size_bytes").to_pylist(), strict=True)
        if path not in prior
    )


def _delta_metric(metrics: dict[str, object], key: str) -> "Posture[int]":
    return Posture.of_option(Option.of_optional(metrics.get(key)).map(int))


def _delta_churn(table: DeltaTable, op: LakeOp) -> "tuple[Posture[int], Posture[int], int]":
    absent: "tuple[Posture[int], Posture[int], int]" = (Posture(absent=None), Posture(absent=None), 0)
    return (
        _COMMIT_METRIC.try_find(op.tag).map(lambda keys: _churn(table.history(1), keys)).default_value(absent)
        if op.committing
        else absent
    )


def _churn(entries: list[dict[str, Any]], keys: tuple[str, str, str | None]) -> "tuple[Posture[int], Posture[int], int]":
    metrics = (entries[0].get("operationMetrics") or {}) if entries else {}
    added, removed, volume = keys
    return _delta_metric(metrics, added), _delta_metric(metrics, removed), _delta_volume(metrics, volume)


def _delta_volume(metrics: dict[str, object], key: str | None) -> int:
    held = metrics.get(key) if key is not None else None
    return int(json_decode(held)["totalSize"]) if isinstance(held, str) else 0


def _retention(retention_hours: int | None) -> datetime:
    return datetime.now(UTC) - timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)


def _age(retention_hours: int | None) -> timedelta:
    return timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
