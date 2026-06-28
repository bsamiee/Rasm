# [PY_DATA_LAKEHOUSE]

The transactional table-format interchange owner: one `LakeOp` operation axis crossed with one `TableFormat` provider axis on one `Lakehouse` owner, admitting Delta, Iceberg (pyiceberg MERGE plus REST catalog), and Lance (multimodal/AI-asset versioning with `create_index` ANN). `Lakehouse.run` folds the write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore lifecycle through one `LakeOp` tagged union, dispatching one `(TableFormat, tag)` provider arm that returns a `RuntimeRail[LakeReceipt]` directly; `LakeReceipt` is the typed commit receipt — format, version, operation, files-added/removed, content-key. The operation axis is format-agnostic; the format binding is a separate discriminant, so a new format is one `TableFormat` row plus its dispatch arm, never a parallel Iceberg/Lance owner. The reject-law is data, not a buried catch-all: one `_PORTABLE` `frozendict[TableFormat, frozenset[str]]` rows each format's reachable op tags, and a `(format, op)` outside the row falls to the `case _, _` arm returning `Error(BoundaryFault(boundary=(f"lake.{format}.{op}", ...)))` directly — the deleted form is a `raise` into a `boundary` re-key that discards the typed key through `BoundaryFault.of`; only genuine provider-thrown exceptions cross the `boundary` conversion. Maintenance is provider-portable: Iceberg `Vacuum` binds `Table.maintenance.expire_snapshots().older_than().commit()`, Lance `Optimize`/`Vacuum` bind `LanceDataset.optimize.compact_files()`/`cleanup_old_versions()`, and the one polymorphic `retention_hours` axis projects to a `datetime` cutoff for Iceberg and a `timedelta` age for Lance. `changefeed` and `restore` are the two genuinely Delta-exclusive ops (the surface `columnar#MATERIALIZE` `DerivedSnapshot._materialize` reads the change feed through `load_cdf`; `restore` writes a revert commit through `DeltaTable.restore`), reachable on no non-Delta `(format, op)` arm, and a future Iceberg revert or Lance change-feed lands as the format's tag in its `_PORTABLE` row plus one arm before the catch-all without touching the dispatch. Every commit is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[LAKEHOUSE]: the transactional table-format lakehouse owner over one `LakeOp` operation axis crossed with one `TableFormat` provider axis.

## [02]-[LAKEHOUSE]

- Owner: `Lakehouse` — the one transactional-table owner over `deltalake`/`pyiceberg`/`lance`; `LakeOp` the tagged-union operation axis (write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore), matched by `match`/`case` so a new table operation is one `LakeOp` case, never a `read_delta`/`write_delta`/`delete_delta`/`compact_delta` method family; `TableFormat` the `StrEnum` provider axis (`DELTA`/`ICEBERG`/`LANCE`) the owner dispatches one `(format, tag)` arm against, so the operation axis and the provider axis are two orthogonal discriminants. The writer-tuning axis rides one `WriteTuning` policy `Struct` carried on `Write`, never a parallel `WriteTuned` op or a knob tail; the merge delete-on-no-match axis rides one `delete_unmatched` discriminant on `Merge` selecting the catalogued third `TableMerger.when_not_matched_by_source_delete` clause, never a parallel `MergeDelete` op. `LakeReceipt` is the typed commit receipt — format, version, operation, files-added/removed, content-key — folded by one `_receipt` projector that reads the post-op snapshot identity off one polymorphic `_snapshot` method discriminating `match self.table_format` closed by `assert_never`, never three sibling `_<format>_receipt` factories nor a parallel `_SNAPSHOT` dispatch dict over three module-level `_<format>_snapshot` functions. The reject-law is data, not a buried catch-all: `_PORTABLE` is one `frozendict[TableFormat, frozenset[str]]` row per format naming the portable op tags, the `case _, _` arm reading it so a `(format, op)` outside the format's portable set is `Error(BoundaryFault(boundary=(...)))` and `changefeed`/`restore` are the two genuinely Delta-exclusive tags the `columnar#MATERIALIZE` `DerivedSnapshot._materialize` and the time-travel revert consume through `load_cdf`/`DeltaTable.restore`.
- Cases: `LakeOp` rows `Write(mode, partition_by, evolve_schema, tuning)` (Delta `write_deltalake` with `mode` ∈ `error|append|overwrite|ignore`, `schema_mode="merge"`, `partition_by`, `target_file_size`, `writer_properties=` and `commit_properties=` projected from `tuning`; Iceberg `Transaction.append`/`overwrite`; Lance `write_dataset` whose `mode` is the op's `WriteMode` projected through the `_LANCE_MODE` data row onto the Lance `create|overwrite|append` band — the create-intent `error`/`ignore` modes landing on `create` so the fail-on-existing contract survives rather than a flat `else "overwrite"` collapse silently replacing the dataset — and `max_rows_per_file` from `tuning.target_file_size`) · `Read(version, columns, predicate)` (the three-format row-count probe over the pinned version — all metadata-only, never a full-table materialize discarded down to `.num_rows`: Delta the version-pinned `DeltaTable(uri, version=)` count — the no-predicate path the metadata-only `to_pyarrow_dataset().count_rows()` over the version-pinned scan, the SQL-predicate path the native `deltalake.QueryBuilder` DataFusion SQL `register(table).execute("SELECT count(*) WHERE <predicate>").read_all()` that pushes the SQL string natively (the `deltalake`-owned SQL-over-Delta surface, so the predicate is never dropped and no SQL->pyarrow-DNF lowering owner is minted); Iceberg `Table.scan(row_filter=, selected_fields=, snapshot_id=).count()` binding the SQL-string predicate through `row_filter` (which natively accepts `str | BooleanExpression` and parses the string internally) and never dropping it, the catalogued `DataScan.count` row-count path over `.to_arrow().num_rows`; Lance `lance.dataset(uri, version=).count_rows(filter=)` pushing the SQL-string predicate into the catalogued count, never materializing the table — `columns` is irrelevant to a count and the real column-projected zero-copy read is the `columnar#SCAN` `scan_delta`/`scan_iceberg` lazy-reader lane, not this commit owner) · `Delete(predicate)` (Delta `DeltaTable.delete(predicate) -> dict`; Iceberg `Transaction.delete(delete_filter)`; Lance `LanceDataset.delete(predicate)`) · `Update(predicate, updates)` (the `updates` a `frozendict[str, str]` carried immutable on the frozen op, coerced to a plain `dict` only at the delta-rs seam — Delta `DeltaTable.update(updates=dict(updates), predicate=) -> dict`; Iceberg and Lance reject — neither exposes a portable predicate-set row-update outside `merge`) · `Merge(predicate, updates, delete_unmatched)` (the `updates` `frozendict[str, str]` coerced once into the `dict` both Delta clauses read — Delta `DeltaTable.merge(...) -> TableMerger` then `when_matched_update`/`when_not_matched_insert`/the `delete_unmatched`-gated `when_not_matched_by_source_delete`/`execute`, the full catalogued three-clause builder chain; Iceberg `Transaction.upsert(df, join_cols=list(updates.keys())) -> UpsertResult`; Lance `LanceDataset.merge_insert(list(updates.keys())).when_matched_update_all().when_not_matched_insert_all().execute(data)` — the `predicate` SQL condition is the Delta merge spelling, while the column-keyed Iceberg/Lance arms derive the join key from the `updates` columns, never the SQL string as a column name) · `Evolve(adds, drops, renames)` (Delta `alter.add_columns([Field(name, dtype), ...])` over the `adds` clause, the `drops`/`renames` guard arm rejecting because `TableAlterer` exposes no column drop or rename; Iceberg the full `Transaction.update_schema()` context-managed `UpdateSchema` chaining `add_column(name, IcebergType.model_validate(dtype))`/`delete_column(column)`/`rename_column(old, new)` over all three clauses, the `(name, type-string)` add binding the `IcebergType` Pydantic string-parse; Lance rejects, no catalogued column-evolution member) · `Optimize(target_size, zorder)` (Delta `DeltaTable.optimize.compact`/`z_order`; Lance `LanceDataset.optimize.compact_files(target_rows_per_fragment=)` returning `CompactionMetrics.fragments_added`; the Iceberg arm rejects — `rewrite_data_files` absent from the Python `pyiceberg` API) · `Vacuum(retention_hours, dry_run)` (Delta `DeltaTable.vacuum -> list[str]`; Iceberg `Table.maintenance.expire_snapshots().older_than(_retention(retention_hours)).commit()` over a `datetime` cutoff; Lance `cleanup_old_versions(older_than=_age(retention_hours))` over a `timedelta` age returning `CleanupStats.old_versions`) · `ChangeFeed(start, end)` (Delta `load_cdf` Change Data Feed into an `arro3.core.RecordBatchReader`, the local arm reading `.read_all().num_rows`; non-Delta formats reach no portable arm) · `Index(column, kind, metric)` (Lance — one op owning both Lance index families, routing the IVF vector kinds `IVF_PQ`/`IVF_HNSW_PQ`/`IVF_HNSW_SQ` through `LanceDataset.create_index(column, index_type, metric=)` and the scalar/FTS kinds `BTREE`/`BITMAP`/`LABEL_LIST`/`ZONEMAP`/`BLOOMFILTER`/`RTREE`/`INVERTED`/`FTS`/`NGRAM` through `create_scalar_index(column, index_type)` by `_VECTOR_INDEX` membership, the multimodal/AI-asset retrieval rail the Lance format owns; `metric` is consumed only by the vector arm; Delta/Iceberg reject, no catalogued portable index surface) · `Restore(target)` (Delta `DeltaTable.restore(target) -> dict` writing a revert commit to a prior version or timestamp, the `num_removed_file`/`num_restored_file` metric keys read through `_delta_metric`; the Iceberg/Lance arms reach no portable arm — Iceberg time-travel rollback is `ManageSnapshots.rollback_to_snapshot` outside the `_PORTABLE` row until a catalogued revert-equivalent member lands), each binding the exact provider surface the `TableFormat` row selects.
- Entry: `Lakehouse.open` admits a `DatasetRef` plus an explicit `TableFormat` (defaulting to `DELTA`) and returns the frozen owner over the resolved `table_uri` as a `RuntimeRail[Lakehouse]`, cross-validating the format against `dataset.kind` rather than recovering it — `DatasetKind` carries no `LANCE` member, so the format cannot be folded from the kind, and the explicit axis is the one carrier admitting all three providers: a `DELTA` format over a non-`DatasetKind.DELTA` dataset and an `ICEBERG` format missing its `catalog`/`identifier` each return `Error(BoundaryFault(resource=(...)))` directly. `Lakehouse.run` folds one `LakeOp` through one `match (self.table_format, op)` over the portable `(format, op)` cross-product with the `case _, _` arm as total reject, each portable arm returning a `RuntimeRail[LakeReceipt]` directly so a non-portable `(format, op)` is `Error(BoundaryFault(boundary=(...)))` and a provider-thrown exception crosses `boundary` once; time-travel is `LakeOp.Read(version=...)`, never a parallel `read_at_version` entrypoint, and a standalone delete is `LakeOp.Delete(...)`, never a `delete_delta`/`delete_iceberg` family.
- Receipt: the commit contributes an emitted-phase `Receipt.of("lakehouse", ("emitted", subject, facts))` row through `ReceiptContributor` (the runtime two-positional `Receipt.of(owner, evidence)` factory matching the `(phase, subject, facts)` evidence tuple, never a four-positional `Receipt.of(phase, owner, subject, facts)` shape) whose `facts` carries the `version`/`added` counts as native `int` the `dict[str, object]` `EventDict` and its `enc_hook=repr` renderer serialize without a `str()` pre-coerce; the `LakeReceipt` is keyed by `ContentIdentity.of("lake", f"{self.table_uri}@{version}".encode())`, which returns a `RuntimeRail[ContentKey]` the `_receipt` projector threads through `.map(lambda key: LakeReceipt(..., content_key=key))` so the receipt is built inside the rail and a digest fault propagates rather than a `Result` landing in the `content_key: ContentKey` slot — the literal `"lake"` `fmt` namespace and the `(table_uri, monotonic-version)` payload uniquely pin the committed snapshot, so the key is stable across a re-open of an unchanged version without a redundant add-action file-URI enumeration; the snapshot-identity read is one polymorphic `_snapshot` method discriminating `match self.table_format` — Delta `DeltaTable.version()` plus `get_add_actions().num_rows` file count, Iceberg the last `snapshot_id` of `InspectTable.snapshots()`, Lance the scalar `LanceDataset.version` `int` property — folded by one `_receipt` projector, never three sibling `_<format>_snapshot` factories nor a parallel `_SNAPSHOT` dict.
- Packages: `deltalake` (`DeltaTable`/`write_deltalake`/`load_cdf`/`optimize`/`vacuum`/`restore`/`merge`/`TableMerger.{when_matched_update,when_not_matched_insert,when_not_matched_by_source_delete,execute}`/`delete`/`update`/`to_pyarrow_dataset().count_rows()` the no-predicate metadata-only read-count probe/`QueryBuilder.register(name, table).execute(sql).read_all()` the DataFusion SQL surface the predicate-bearing Delta read-count pushes the SQL string through/`alter.add_columns`/`schema.Field`/`get_add_actions`/`version`/`WriterProperties`/`ColumnProperties`/`BloomFilterProperties`/`CommitProperties`), `pyiceberg` (`load_catalog`/`Catalog.load_table`/`Table.transaction`/`Transaction.{append,overwrite,upsert,delete,update_schema,commit_transaction}`/`UpdateSchema.{add_column,delete_column,rename_column}`/`types.IcebergType.model_validate`/`Table.maintenance.expire_snapshots`/`ExpireSnapshots.{older_than,commit}`/`Table.scan(row_filter=,selected_fields=,snapshot_id=)` over a native `str | BooleanExpression` filter parsed by `expressions.parser.parse`/`DataScan.{count,to_arrow}`/`InspectTable.snapshots`, the one `<3.15` gated arm whose `Table` annotation rides `TYPE_CHECKING`), `pylance` (`lance.dataset`/`write_dataset(mode=,max_rows_per_file=)`/`LanceDataset.{count_rows,merge_insert,delete,create_index,create_scalar_index,version,optimize,cleanup_old_versions}`/`DatasetOptimizer.compact_files(target_rows_per_fragment=)`/`MergeInsertBuilder.{when_matched_update_all,when_not_matched_insert_all,execute}`), `pyarrow` (`Table` the write carrier the `data` param admits, `dataset.Dataset.count_rows()` the Delta read-count), `arro3-core` (the `RecordBatchReader`/`Table` the Delta `load_cdf` change-feed egress returns, `.read_all().num_rows` the local row count), `duckdb` (queries any format's Arrow snapshot via `from_arrow`), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `open` factory and the caller-facing `run` submission so a non-`DatasetRef`/`TableFormat` or non-`LakeOp` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the enclosing `boundary`/`guarded_sync` fence rather than an untyped admission, the shared `FAULT_CONF` the sibling `interop`/`egress`/`columnar` admission seams bind; the `_receipt`/`LakeReceipt` projection over the owner's own committed snapshot carries no decorator), runtime (`RuntimeRail`/`BoundaryFault`/`boundary`/`FAULT_CONF` the shared beartype violation-redirect config/`ContentIdentity`/`ReceiptContributor`/`Receipt`, `reliability/resilience#RESILIENCE` `RetryClass.LAKE_COMMIT`/`guarded_sync` the sync commit-conflict retry envelope the mutating arms ride).
- Growth: a new lake operation is one `LakeOp` case absorbed by the `(format, tag)` dispatch; a new write mode is a `Literal` row on `Write`; a new writer-tuning knob is a field on `WriteTuning`; a new Lance vector index kind is a `Literal` row on `VectorIndex` (a scalar/FTS kind on `ScalarIndex`), both absorbed by the one `_VECTOR_INDEX`-routed `Index` arm; a fourth table format (Hudi, Paimon) is one `TableFormat` member plus its `(format, *)` arms on this same owner, never a parallel owner.
- Boundary: no durable store, no schema migration, no global Delta/catalog connection; a `read_delta`/`write_delta`/`delete_delta`/`optimize_delta` family, a per-operation class family, a parallel `WriteTuned` op, three sibling `_<format>_receipt`/`_<format>_snapshot` factories, a `_SNAPSHOT` dispatch dict beside the `match`, a `raise BoundaryFault` reject path into a `boundary` that re-keys it, a parallel `IcebergLakehouse`/`LanceLakehouse` pair, a hand-opened `stamina.retry_context` commit-conflict loop minted on this page where the runtime `guarded_sync(RetryClass.LAKE_COMMIT, ...)` envelope owns the retry/span/lift triplet, a commit-conflict left unretried where the `op.mutating` fence routes every committing op through that envelope, and an undecorated `open`/`run` admitting a caller `DatasetRef`/`TableFormat`/`LakeOp` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `interop`/`egress`/`columnar` admission entrypoints share are the deleted forms. The Iceberg/Lance arms reject the ops their provider surface does not portably reach — the `_PORTABLE` row per format names exactly the reachable tags, so Iceberg rejects `Update`/`Optimize`/`ChangeFeed`/`Index`/`Restore` (`rewrite_data_files`, `load_cdf`, and a version-or-timestamp revert all absent from the Python `pyiceberg` API) and Lance rejects `Update`/`Evolve`/`ChangeFeed`/`Restore` as `Error(BoundaryFault(boundary=(...)))`, never a silent no-op; a rollback or change-feed arm lands as the format's tag added to its `_PORTABLE` row plus one dispatch arm before the catch-all, never a new owner.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from builtins import frozendict
from collections.abc import Iterable
from datetime import UTC, datetime, timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

import lance
import pyarrow as pa
from beartype import beartype
from deltalake import (
    BloomFilterProperties,
    ColumnProperties,
    CommitProperties,
    DeltaTable,
    QueryBuilder,
    WriterProperties,
    write_deltalake,
)
from deltalake.schema import Field
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.data.tabular.columnar import DatasetKind, DatasetRef
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded_sync

if TYPE_CHECKING:
    from pyiceberg.table import Table

# --- [TYPES] ----------------------------------------------------------------------------

type WriteMode = Literal["error", "append", "overwrite", "ignore"]
type LanceMode = Literal["create", "overwrite", "append"]
type Compression = Literal["zstd", "snappy", "gzip", "lz4", "brotli", "uncompressed"]
type VectorIndex = Literal["IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"]
type ScalarIndex = Literal["BTREE", "BITMAP", "LABEL_LIST", "ZONEMAP", "BLOOMFILTER", "RTREE", "INVERTED", "FTS", "NGRAM"]
type IndexKind = VectorIndex | ScalarIndex
type Metric = Literal["L2", "cosine", "dot"]
type Evolution = tuple[tuple[tuple[str, str], ...], tuple[str, ...], tuple[tuple[str, str], ...]]


class TableFormat(StrEnum):
    DELTA = "delta"
    ICEBERG = "iceberg"
    LANCE = "lance"


# --- [MODELS] ---------------------------------------------------------------------------


class WriteTuning(Struct, frozen=True):
    compression: Compression = "zstd"
    statistics_truncate_length: int | None = None
    target_file_size: int | None = None
    bloom_columns: tuple[str, ...] = ()
    custom_metadata: frozendict[str, str] = frozendict()
    max_commit_retries: int | None = None

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
        return CommitProperties(custom_metadata=self.custom_metadata or None, max_commit_retries=self.max_commit_retries)


@tagged_union(frozen=True)
class LakeOp:
    tag: Literal["write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "index", "restore"] = tag()
    write: tuple[WriteMode, tuple[str, ...], bool, WriteTuning] = case()
    read: tuple[int | str | datetime | None, tuple[str, ...], str | None] = case()
    delete: tuple[str] = case()
    update: tuple[str, frozendict[str, str]] = case()
    merge: tuple[str, frozendict[str, str], bool] = case()
    evolve: Evolution = case()
    optimize: tuple[int | None, tuple[str, ...]] = case()
    vacuum: tuple[int | None, bool] = case()
    changefeed: tuple[int, int | None] = case()
    index: tuple[str, IndexKind, Metric] = case()
    restore: tuple[int | datetime] = case()

    @property
    def mutating(self) -> bool:
        # the committing ops (write/delete/update/merge/evolve/optimize/vacuum/index/restore) ride
        # the `LAKE_COMMIT` retry envelope; `read`/`changefeed` are read-only and never conflict.
        return self.tag not in _READ_ONLY

    @staticmethod
    def Write(
        mode: WriteMode = "error",
        partition_by: tuple[str, ...] = (),
        evolve_schema: bool = False,
        tuning: WriteTuning = WriteTuning(),
    ) -> "LakeOp":
        return LakeOp(write=(mode, partition_by, evolve_schema, tuning))

    @staticmethod
    def Read(version: int | str | datetime | None = None, columns: tuple[str, ...] = (), predicate: str | None = None) -> "LakeOp":
        return LakeOp(read=(version, columns, predicate))

    @staticmethod
    def Delete(predicate: str) -> "LakeOp":
        return LakeOp(delete=(predicate,))

    @staticmethod
    def Update(predicate: str, updates: frozendict[str, str]) -> "LakeOp":
        return LakeOp(update=(predicate, updates))

    @staticmethod
    def Merge(predicate: str, updates: frozendict[str, str], delete_unmatched: bool = False) -> "LakeOp":
        return LakeOp(merge=(predicate, updates, delete_unmatched))

    @staticmethod
    def Evolve(
        adds: tuple[tuple[str, str], ...] = (),
        drops: tuple[str, ...] = (),
        renames: tuple[tuple[str, str], ...] = (),
    ) -> "LakeOp":
        return LakeOp(evolve=(adds, drops, renames))

    @staticmethod
    def Optimize(target_size: int | None = None, zorder: tuple[str, ...] = ()) -> "LakeOp":
        return LakeOp(optimize=(target_size, zorder))

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


class LakeReceipt(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    operation: str
    version: int
    files_added: int
    files_removed: int
    content_key: ContentKey

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "lakehouse",
            ("emitted", self.table_uri, {"format": self.table_format, "op": self.operation, "version": self.version, "added": self.files_added}),
        )


# --- [SERVICES] -------------------------------------------------------------------------


class Lakehouse(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    catalog: str | None = None
    identifier: str | None = None

    @classmethod
    @beartype(conf=FAULT_CONF)
    def open(
        cls, dataset: DatasetRef, table_format: TableFormat = TableFormat.DELTA, *, catalog: str | None = None, identifier: str | None = None
    ) -> "RuntimeRail[Lakehouse]":
        if table_format is TableFormat.DELTA and dataset.kind is not DatasetKind.DELTA:
            return Error(BoundaryFault(resource=("not-delta", dataset.ref.relative)))
        if table_format is TableFormat.ICEBERG and (catalog is None or identifier is None):
            return Error(BoundaryFault(resource=("iceberg-needs-catalog", dataset.ref.relative)))
        return Ok(cls(table_uri=str(dataset.ref.path), table_format=table_format, catalog=catalog, identifier=identifier))

    @beartype(conf=FAULT_CONF)
    def run(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        # a committing op rides the runtime `guarded_sync(RetryClass.LAKE_COMMIT, ...)` sync envelope so a
        # concurrent-writer `CommitFailedError`/`CommitFailedException` conflict retries under the one
        # `stamina` policy row before the rail resolves; a read-only op fences through plain `boundary`.
        # Both self-flatten the nested `_apply` rail through `.bind(lambda rail: rail)`.
        subject = f"lake.{self.table_format}.{op.tag}"
        fenced = (
            guarded_sync(RetryClass.LAKE_COMMIT, self._apply, op, data, subject=subject)
            if op.mutating
            else boundary(subject, lambda: self._apply(op, data))
        )
        return fenced.bind(lambda rail: rail)

    def _reject(self, op: LakeOp) -> "RuntimeRail[LakeReceipt]":
        reach = "no portable" if op.tag not in _PORTABLE[self.table_format] else "unhandled"
        return Error(BoundaryFault(boundary=(f"lake.{self.table_format}.{op.tag}", f"{self.table_format} has {reach} {op.tag}")))

    def _apply(self, op: LakeOp, data: pa.Table | None) -> "RuntimeRail[LakeReceipt]":
        match self.table_format, op:
            case TableFormat.DELTA, LakeOp(tag="write", write=(mode, partition_by, evolve, tuning)):
                write_deltalake(
                    self.table_uri,
                    data,
                    mode=mode,
                    partition_by=list(partition_by) or None,
                    schema_mode="merge" if evolve else None,
                    target_file_size=tuning.target_file_size,
                    writer_properties=tuning.writer_properties(),
                    commit_properties=tuning.commit_properties(),
                )
                return self._receipt("write")
            case TableFormat.DELTA, LakeOp(tag="read", read=(version, _columns, predicate)):
                # the SQL-string predicate pushes through the native `deltalake.QueryBuilder` DataFusion
                # SQL surface (register the version-pinned table, `SELECT count(*) WHERE <predicate>`),
                # so the Delta count pushes the predicate identically to the Iceberg `row_filter=str`
                # and Lance `count_rows(filter=str)` arms — `deltalake` owns SQL-over-Delta through
                # DataFusion, so no SQL->pyarrow-DNF lowering owner is minted; the no-predicate count
                # stays the metadata-only `to_pyarrow_dataset().count_rows()` over the version-pinned scan.
                table = DeltaTable(self.table_uri, version=version)
                rows = (
                    QueryBuilder().register("t", table).execute(f"SELECT count(*) AS n FROM t WHERE {predicate}").read_all().column("n")[0].as_py()
                    if predicate
                    else table.to_pyarrow_dataset().count_rows()
                )
                return self._receipt("read", snapshot=rows)
            case TableFormat.DELTA, LakeOp(tag="delete", delete=(predicate,)):
                metrics = DeltaTable(self.table_uri).delete(predicate)
                return self._receipt("delete", removed=_delta_metric(metrics, "num_deleted_rows"))
            case TableFormat.DELTA, LakeOp(tag="update", update=(predicate, updates)):
                metrics = DeltaTable(self.table_uri).update(updates=dict(updates), predicate=predicate)
                return self._receipt("update", snapshot=_delta_metric(metrics, "num_updated_rows"))
            case TableFormat.DELTA, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                clauses = dict(updates)
                merger = DeltaTable(self.table_uri).merge(data, predicate=predicate).when_matched_update(updates=clauses).when_not_matched_insert(updates=clauses)
                (merger.when_not_matched_by_source_delete() if delete_unmatched else merger).execute()
                return self._receipt("merge")
            case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(_adds, drops, renames)) if drops or renames:
                return Error(BoundaryFault(boundary=(f"lake.{self.table_format}.evolve", "delta alter has no portable column drop or rename")))
            case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(adds, _drops, _renames)):
                DeltaTable(self.table_uri).alter.add_columns([Field(name, dtype) for name, dtype in adds])
                return self._receipt("evolve")
            case TableFormat.DELTA, LakeOp(tag="optimize", optimize=(target_size, zorder)):
                opt = DeltaTable(self.table_uri).optimize
                opt.z_order(list(zorder), target_size=target_size) if zorder else opt.compact(target_size=target_size)
                return self._receipt("optimize")
            case TableFormat.DELTA, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                removed = DeltaTable(self.table_uri).vacuum(retention_hours=retention_hours, dry_run=dry_run)
                return self._receipt("vacuum", removed=len(removed))
            case TableFormat.DELTA, LakeOp(tag="changefeed", changefeed=(start, end)):
                rows = DeltaTable(self.table_uri).load_cdf(starting_version=start, ending_version=end).read_all().num_rows
                return self._receipt("changefeed", snapshot=rows)
            case TableFormat.DELTA, LakeOp(tag="restore", restore=(target,)):
                metrics = DeltaTable(self.table_uri).restore(target)
                return self._receipt("restore", removed=_delta_metric(metrics, "num_removed_file"), snapshot=_delta_metric(metrics, "num_restored_file"))
            case TableFormat.ICEBERG, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                txn = self._iceberg().transaction()
                txn.overwrite(data) if mode == "overwrite" else txn.append(data)
                txn.commit_transaction()
                return self._receipt("write")
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(version, columns, predicate)):
                scan = self._iceberg().scan(
                    row_filter=predicate or "true",
                    selected_fields=tuple(columns) or ("*",),
                    snapshot_id=version if isinstance(version, int) else None,
                )
                return self._receipt("read", snapshot=scan.count())
            case TableFormat.ICEBERG, LakeOp(tag="delete", delete=(predicate,)):
                # `Transaction.delete`/`upsert` return `None`/`UpsertResult`, not the `Transaction`, so
                # `commit_transaction` is a separate statement off the bound `txn`, never a chained call.
                txn = self._iceberg().transaction()
                txn.delete(predicate)
                txn.commit_transaction()
                return self._receipt("delete")
            case TableFormat.ICEBERG, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                txn = self._iceberg().transaction()
                txn.upsert(data, join_cols=list(updates.keys()))
                txn.commit_transaction()
                return self._receipt("merge")
            case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(adds, drops, renames)):
                from pyiceberg.types import IcebergType  # noqa: PLC0415

                with self._iceberg().update_schema() as schema:
                    for name, dtype in adds:
                        schema.add_column(name, IcebergType.model_validate(dtype))
                    for column in drops:
                        schema.delete_column(column)
                    for old, new in renames:
                        schema.rename_column(old, new)
                return self._receipt("evolve")
            case TableFormat.ICEBERG, LakeOp(tag="vacuum", vacuum=(retention_hours, _dry_run)):
                self._iceberg().maintenance.expire_snapshots().older_than(_retention(retention_hours)).commit()
                return self._receipt("vacuum")
            case TableFormat.LANCE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, tuning)):
                lance.write_dataset(data, self.table_uri, mode=_LANCE_MODE[mode], max_rows_per_file=tuning.target_file_size or 1024 * 1024)
                return self._receipt("write")
            case TableFormat.LANCE, LakeOp(tag="read", read=(version, _columns, predicate)):
                rows = lance.dataset(self.table_uri, version=version).count_rows(filter=predicate)
                return self._receipt("read", snapshot=rows)
            case TableFormat.LANCE, LakeOp(tag="delete", delete=(predicate,)):
                lance.dataset(self.table_uri).delete(predicate)
                return self._receipt("delete")
            case TableFormat.LANCE, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                # Lance `merge_insert(on)` keys on column name(s), so the join key is the update
                # columns (matching the Iceberg `upsert(join_cols=)` arm), never the Delta SQL `predicate`.
                builder = lance.dataset(self.table_uri).merge_insert(list(updates.keys()))
                builder.when_matched_update_all().when_not_matched_insert_all().execute(data)
                return self._receipt("merge")
            case TableFormat.LANCE, LakeOp(tag="index", index=(column, kind, metric)):
                ds = lance.dataset(self.table_uri)
                # `metric` is meaningful only for the IVF vector families; the scalar/FTS kinds
                # take no metric and ride `create_scalar_index`, routed by `_VECTOR_INDEX` membership.
                ds.create_index(column, index_type=kind, metric=metric) if kind in _VECTOR_INDEX else ds.create_scalar_index(column, index_type=kind)
                return self._receipt("index")
            case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(target_size, _zorder)):
                metrics = lance.dataset(self.table_uri).optimize.compact_files(target_rows_per_fragment=target_size)
                return self._receipt("optimize", snapshot=metrics.fragments_added)
            case TableFormat.LANCE, LakeOp(tag="vacuum", vacuum=(retention_hours, _dry_run)):
                stats = lance.dataset(self.table_uri).cleanup_old_versions(older_than=_age(retention_hours))
                return self._receipt("vacuum", removed=stats.old_versions)
            case _, _:
                return self._reject(op)

    def _iceberg(self) -> "Table":
        from pyiceberg.catalog import load_catalog  # noqa: PLC0415

        return load_catalog(self.catalog).load_table(self.identifier)

    def _snapshot(self) -> tuple[int, int]:
        match self.table_format:
            case TableFormat.DELTA:
                table = DeltaTable(self.table_uri)
                return table.version(), table.get_add_actions().num_rows
            case TableFormat.ICEBERG:
                history = self._iceberg().inspect.snapshots()
                return (history.column("snapshot_id")[-1].as_py() if history.num_rows else 0), 0
            case TableFormat.LANCE:
                return lance.dataset(self.table_uri).version, 0
            case unreachable:
                assert_never(unreachable)

    def _receipt(self, operation: str, *, snapshot: int = 0, removed: int = 0) -> "RuntimeRail[LakeReceipt]":
        version, added = self._snapshot()
        return ContentIdentity.of("lake", f"{self.table_uri}@{version}".encode()).map(
            lambda key: LakeReceipt(
                table_uri=self.table_uri,
                table_format=self.table_format,
                operation=operation,
                version=version,
                files_added=added or snapshot,
                files_removed=removed,
                content_key=key,
            )
        )


# --- [TABLES] ---------------------------------------------------------------------------

_DEFAULT_RETENTION_HOURS: Final[int] = 168

_VECTOR_INDEX: Final[frozenset[str]] = frozenset({"IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"})

# the read-only op tags `LakeOp.mutating` excludes from the `LAKE_COMMIT` retry envelope — every
# other op commits and can hit a concurrent-writer conflict; `read`/`changefeed` only scan.
_READ_ONLY: Final[frozenset[str]] = frozenset({"read", "changefeed"})

# the Delta WriteMode vocabulary projected onto the Lance create|overwrite|append band — the
# create-intent modes (`error`/`ignore`) land on Lance `create` (fail-if-exists), preserving the
# fail-on-existing contract a flat `"append" if mode=="append" else "overwrite"` collapse erased.
_LANCE_MODE: Final[frozendict[WriteMode, LanceMode]] = frozendict(
    {"error": "create", "ignore": "create", "overwrite": "overwrite", "append": "append"}
)

_PORTABLE: Final[frozendict[TableFormat, frozenset[str]]] = frozendict({
    TableFormat.DELTA: frozenset({"write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "restore"}),
    TableFormat.ICEBERG: frozenset({"write", "read", "delete", "merge", "evolve", "vacuum"}),
    TableFormat.LANCE: frozenset({"write", "read", "delete", "merge", "index", "optimize", "vacuum"}),
})


# --- [OPERATIONS] -----------------------------------------------------------------------


def _delta_metric(metrics: dict[str, object], key: str) -> int:
    return int(metrics.get(key, 0))


def _retention(retention_hours: int | None) -> datetime:
    return datetime.now(UTC) - timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)


def _age(retention_hours: int | None) -> timedelta:
    return timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)
```

## [03]-[RESEARCH]

- [STRING_PREDICATE_FILTER]: SETTLED across all three formats — the `Read` op is a metadata-only row-count probe (the column-projected zero-copy read is the `columnar#SCAN` lazy-reader lane), so no arm materializes a table to read `.num_rows`, and all three push the `LakeOp.Read.predicate` SQL string natively with no SQL->pyarrow-DNF lowering owner minted. Lance pushes the string into `LanceDataset.count_rows(filter=)` directly (the same SQL-string filter as `to_table(filter=)`, catalogued aggregate row [06]); Iceberg pushes it into `Table.scan(row_filter: str | BooleanExpression = ALWAYS_TRUE)` which natively accepts a string and parses it through `pyiceberg.expressions.parser.parse(expr) -> BooleanExpression` internally (catalogued scan rows [01],[10]), the inlined arm passing `row_filter=predicate or "true"` straight through (the empty-predicate sentinel `"true"` parsing to `AlwaysTrue`, the `_iceberg_filter`/`AlwaysTrue()` indirection deleted); and Delta — the formerly-open seam — pushes the SQL string through the native `deltalake.QueryBuilder` DataFusion SQL surface (`QueryBuilder().register("t", DeltaTable(uri, version=)).execute(f"SELECT count(*) AS n FROM t WHERE {predicate}").read_all().column("n")[0].as_py()`, catalogued `QueryBuilder` rows [25]/[45]/[59]), reflection-confirmed against `deltalake` `1.6.0` that `QueryBuilder.register(name, table).execute(sql)` returns an `arro3.core.RecordBatchReader` whose `.read_all()` carries the count over the SQL predicate — so `deltalake` itself owns SQL-over-Delta through DataFusion and the predicate is never dropped, the prior premise (that a Delta string-predicate push needs a SQL-string-to-DNF-tuple lowering because `to_pyarrow_table(filters=)` takes pyarrow disjunctive-normal-form tuples) resolved by the better native path the provider owns rather than a minted lowering owner; the no-predicate Delta count stays the metadata-only `to_pyarrow_dataset().count_rows()` over the version-pinned `DeltaTable(uri, version=)` scan, and the `QueryBuilder` version-pin rides the registered version-pinned `DeltaTable`. No SQL-string->pyarrow-Expression/DNF lowering owner exists or is needed in the data tier — each provider's own SQL surface (DataFusion for Delta, the native `row_filter` parser for Iceberg, the `count_rows(filter=)` parser for Lance) owns the predicate parse.
- [LANCE_VERSION]: SETTLED — the `pylance` `lance.dataset(uri, version=)`/`write_dataset(data, uri, mode=, max_rows_per_file=)`/`LanceDataset.{to_table,merge_insert,delete}`/`MergeInsertBuilder.{when_matched_update_all,when_not_matched_insert_all,execute}` surface and the scalar `LanceDataset.version -> int` `@property` the `_snapshot` Lance arm reads are catalogue-confirmed against the folder `pylance` `.api` (`.api/pylance.md` I/O rows [01]-[02], version-and-mutation rows [01]-[04], reflection-verified at `[LANCE_TOPOLOGY]`): the scalar `.version` current-checked-out accessor and the plural `versions()` history rail are distinct confirmed members, so the write/read/delete/merge snapshot reads `.version` as the settled identity with no live-distribution gate remaining, folding the prior `[LANCE_VERSION_ACCESSOR]` block where the catalogue listed only `versions()`. The `write_dataset(max_rows_per_file=)` Parquet-fragment sizing the Lance `Write` arm threads from `tuning.target_file_size` reads I/O row [02] verbatim, so the Lance `Write` tuning is settled fence code, not a knob tail.
- [LANCE_INDEX]: SETTLED — the `Index` op owns both catalogued Lance index families through one arm routed by `_VECTOR_INDEX` membership: the IVF vector entry `LanceDataset.create_index(column, index_type, *, metric, replace)` (`.api/pylance.md` index row [01]) for `VectorIndex` `Literal["IVF_PQ","IVF_HNSW_PQ","IVF_HNSW_SQ"]` over `Metric` `Literal["L2","cosine","dot"]`, and the scalar/FTS entry `LanceDataset.create_scalar_index(column, index_type, *, replace, train)` (`.api/pylance.md` index row [02]) for `ScalarIndex` `Literal["BTREE","BITMAP","LABEL_LIST","ZONEMAP","BLOOMFILTER","RTREE","INVERTED","FTS","NGRAM"]` — the prior `IndexKind` `Literal["IVF_PQ","IVF_HNSW_PQ","BTREE"]` over a single `create_index(metric=)` call is the deleted form, since the catalogue lists `BTREE` only under `create_scalar_index` (which takes no `metric`) and the IVF set's third member is `IVF_HNSW_SQ`, not `BTREE`. `metric` is consumed only by the vector arm; the scalar/FTS arm drops it. The Delta/Iceberg `(format, index)` pairs fall to the catch-all reject because no portable index surface is catalogued for either.
- [DELTA_MERGE_CHAIN]: SETTLED — the `deltalake` `DeltaTable.merge(source, predicate=) -> TableMerger` then `when_matched_update(updates=)`/`when_not_matched_insert(updates=)`/the `delete_unmatched`-gated `when_not_matched_by_source_delete()`/`execute()` builder chain the Delta `merge` arm binds is catalogue-confirmed against the folder `deltalake` `.api` (mutate entrypoint row and implementation-law `TableMerger` chain naming all three clauses `when_matched_update`/`when_not_matched_insert`/`when_not_matched_by_source_delete`); the prior `when_not_matched_insert_all()` spelling is the deleted form — no `_all` Delta-merge clause is catalogued, so the chain binds the catalogued `when_not_matched_insert(updates=)` member, and the previously-dropped third clause is now bound through the `Merge` `delete_unmatched` discriminant selecting `when_not_matched_by_source_delete()` rather than a parallel `MergeDelete` op, so the full catalogued upsert-plus-delete-on-no-match chain is settled fence code.
- [DELTA_WRITER_TUNING]: SETTLED — the `deltalake` `WriterProperties`/`ColumnProperties`/`BloomFilterProperties`/`CommitProperties` types and the `write_deltalake(writer_properties=, target_file_size=, commit_properties=)` parameters the `WriteTuning` policy projects into are catalogue-confirmed against the folder `deltalake` `.api` (public-types rows for the writer-property and commit-property carriers, write entrypoint row), and the exact keyword spellings the projectors bind are confirmed against the live distribution: `WriterProperties(compression=, statistics_truncate_length=, column_properties=)`, `ColumnProperties(bloom_filter_properties=)`, `BloomFilterProperties(set_bloom_filter_enabled=)`, and `CommitProperties(custom_metadata=, max_commit_retries=, app_transactions=)` — `WriteTuning.custom_metadata`/`max_commit_retries` thread the catalogued capability rather than a bare `CommitProperties()`, so `WriteTuning` is settled fence code with no open seam.
- [DELTA_ROW_MUTATION]: SETTLED — the `deltalake` `DeltaTable.delete(predicate) -> dict` and `DeltaTable.update(updates=, predicate=) -> dict` row-mutation methods the `Delete`/`Update` arms bind are catalogue-confirmed against the folder `deltalake` `.api` (mutate entrypoint row), and the `dict` return-metric keys `num_deleted_rows`/`num_updated_rows` the `_delta_metric` projector reads are confirmed against the live distribution docstrings (`update` returns `num_updated_rows`, `delete` returns `num_deleted_rows` and omits the key only on a predicate-less full delete — the metric-dict note on the `.api` implementation-law row). `_delta_metric` isolates the one key per call and reads `0` when the key is absent, so a predicate-less delete folds to a zero receipt count rather than faulting.
- [DELTA_RESTORE]: SETTLED at the entry, RESEARCH at the metric keys — the `deltalake` `DeltaTable.restore(target, *, ignore_missing_files=False, protocol_downgrade_allowed=False, ...) -> dict` time-travel-revert entry the `Restore` op binds is catalogue-confirmed against the folder `deltalake` `.api` (time-travel entrypoint row), so the `int`-version-or-`datetime`-timestamp `target` and the new-revert-commit semantics (`restore` writes a new commit reverting the table rather than mutating history, implementation-law row) are settled fence code; the `dict` return-metric keys `num_removed_file`/`num_restored_file` the `_delta_metric` projector reads are RESEARCH-pending against the live distribution, isolated to the one provisional key per `_delta_metric` call, the revert committing regardless and only the receipt count gating. `Restore` is the second Delta-exclusive tag beside `changefeed`: Iceberg time-travel rollback is the catalogued `ManageSnapshots.rollback_to_snapshot(snapshot_id)` (snapshot row [09]) rather than a version-or-timestamp revert shorthand, so the Iceberg `restore` arm rejects until a catalogued revert-equivalent member lands, and Lance carries no catalogued revert surface.
- [SCHEMA_EVOLVE]: SETTLED — the `Evolve` op binds the catalogued schema-mutation surface on both writing formats with no `NotImplementedError` gate remaining. Delta binds `alter.add_columns([Field(name, dtype), ...])` (`.api/deltalake.md` evolve entrypoint row plus the `Field(name, type)` string-type note); `TableAlterer` exposes only `add_columns`, so the `drops`/`renames` guard arm rejects with a typed `BoundaryFault` and the `(name, type-string)` add tuple constructs `deltalake.schema.Field(name, dtype)` directly because `Field` accepts a primitive type string. Iceberg binds the full `Transaction.update_schema()` context-managed `UpdateSchema` (`.api/pyiceberg.md` evolution rows [07],[11]) chaining `add_column(name, IcebergType.model_validate(dtype))`/`delete_column(column)`/`rename_column(old, new)` over all three `Evolution` clauses inside the autocommitting `with`, where `delete_column(path)` is the catalogued portable column-drop the prior reject claim wrongly denied, and the `(name, type-string)` add binds `pyiceberg.types.IcebergType.model_validate(dtype)` — the Pydantic wrap validator parses a primitive type string to a `StringType()`/`LongType()` instance, the settled string-to-type bridge. Lance rejects `Evolve`, no catalogued column-evolution member.
- [ICEBERG_LANCE_MAINTENANCE]: SETTLED at the bound maintenance arms, RESEARCH-permanent at the genuinely-absent surfaces — the `_PORTABLE` reject-set table is the settled reject-law: `DELTA` rows all ten non-index tags, `ICEBERG` rows `write|read|delete|merge|evolve|vacuum`, `LANCE` rows `write|read|delete|merge|index|optimize|vacuum`, so the `case _, _` arm reads the table and the maintenance gaps are the tags each row omits, never a buried `_PORTABLE_DELTA_ONLY` set forcing every non-Delta op through the catch-all. The Iceberg `Vacuum` arm binds `Table.maintenance.expire_snapshots().older_than(dt).commit()` (`.api/pyiceberg.md` maintenance rows [12]-[13]) over an autocommitting `Transaction`, and the Lance `Optimize`/`Vacuum` arms bind `LanceDataset.optimize.compact_files(target_rows_per_fragment=)` returning `CompactionMetrics.fragments_added` and `cleanup_old_versions(older_than=, retain_versions=)` returning `CleanupStats.old_versions` (`.api/pylance.md` maintenance rows [07]-[10]) — these are settled fence code, the one `retention_hours` axis projecting through `_retention` to a `datetime` cutoff and `_age` to a `timedelta` age. The Iceberg `Optimize` arm stays reject-permanent (`rewrite_data_files` is absent from the Python `pyiceberg` API), Iceberg `remove_orphan_files` is reject-permanent (no public Python member — `ObjectStoreLocationProvider` references orphan removal only in a layout comment), the Iceberg `Restore` arm rejects until a catalogued version-or-timestamp revert lands (only `ManageSnapshots.rollback_to_snapshot(snapshot_id)` is catalogued, snapshot row [09]), and `changefeed`/`restore` reach no non-Delta arm because the `columnar#MATERIALIZE` `DerivedSnapshot._materialize` reads the CDF only through the Delta `load_cdf` surface and the revert commit only through `DeltaTable.restore`.
