# [PY_DATA_LAKEHOUSE]

The transactional table-format interchange owner: one `LakeOp` operation axis crossed with one `TableFormat` provider axis on one `Lakehouse` owner, admitting Delta, Iceberg (pyiceberg MERGE plus REST catalog), and Lance (multimodal/AI-asset versioning with `create_index` ANN). `Lakehouse.run` folds the write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore lifecycle through one `LakeOp` tagged union, dispatching one `(TableFormat, tag)` provider arm that returns a `RuntimeRail[LakeReceipt]` directly; `LakeReceipt` is the typed commit receipt — format, version, operation, files-added/removed, content-key. The operation axis is format-agnostic; the format binding is a separate discriminant, so a new format is one `TableFormat` row plus its dispatch arm, never a parallel Iceberg/Lance owner. The reject-law is data, not a buried catch-all: one `_PORTABLE` frozen `dict[TableFormat, frozenset[str]]` rows each format's reachable op tags, and a `(format, op)` outside the row falls to the `case _, _` arm returning `Error(BoundaryFault(boundary=(f"lake.{format}.{op}", ...)))` directly — the deleted form is a `raise` into a `boundary` re-key that discards the typed key through `BoundaryFault.of`; only genuine provider-thrown exceptions cross the `boundary` conversion. Maintenance is provider-portable: Iceberg `Vacuum` binds `Table.maintenance.expire_snapshots().older_than().commit()`, Lance `Optimize`/`Vacuum` bind `LanceDataset.optimize.compact_files()`/`cleanup_old_versions()`, and the one polymorphic `retention_hours` axis projects to a `datetime` cutoff for Iceberg and a `timedelta` age for Lance. `changefeed` and `restore` are the two genuinely Delta-exclusive ops (the surface `columnar#MATERIALIZE` `DerivedSnapshot._materialize` reads the change feed through `load_cdf`; `restore` writes a revert commit through `DeltaTable.restore`), reachable on no non-Delta `(format, op)` arm, and a future Iceberg revert or Lance change-feed lands as the format's tag in its `_PORTABLE` row plus one arm before the catch-all without touching the dispatch. Every commit is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[LAKEHOUSE]: the transactional table-format lakehouse owner over one `LakeOp` operation axis crossed with one `TableFormat` provider axis.

## [02]-[LAKEHOUSE]

- Owner: `Lakehouse` — the one transactional-table owner over `deltalake`/`pyiceberg`/`lance`; `LakeOp` the tagged-union operation axis (write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore), matched by `match`/`case` so a new table operation is one `LakeOp` case, never a `read_delta`/`write_delta`/`delete_delta`/`compact_delta` method family; `TableFormat` the `StrEnum` provider axis (`DELTA`/`ICEBERG`/`LANCE`) the owner dispatches one `(format, tag)` arm against, so the operation axis and the provider axis are two orthogonal discriminants. The writer-tuning axis rides one `WriteTuning` policy `Struct` carried on `Write`, never a parallel `WriteTuned` op or a knob tail; the merge delete-on-no-match axis rides one `delete_unmatched` discriminant on `Merge` selecting the catalogued third `TableMerger.when_not_matched_by_source_delete` clause, never a parallel `MergeDelete` op. `LakeReceipt` is the typed commit receipt — format, version, operation, files-added/removed, content-key — folded by one `_receipt` projector that reads the post-op snapshot identity off one polymorphic `_snapshot` method discriminating `match self.table_format` closed by `assert_never`, never three sibling `_<format>_receipt` factories nor a parallel `_SNAPSHOT` dispatch dict over three module-level `_<format>_snapshot` functions. The reject-law is data, not a buried catch-all: `_PORTABLE` is one frozen `dict[TableFormat, frozenset[str]]` row per format naming the portable op tags, the `case _, _` arm reading it so a `(format, op)` outside the format's portable set is `Error(BoundaryFault(boundary=(...)))` and `changefeed`/`restore` are the two genuinely Delta-exclusive tags the `columnar#MATERIALIZE` `DerivedSnapshot._materialize` and the time-travel revert consume through `load_cdf`/`DeltaTable.restore`.
- Cases: `LakeOp` rows `Write(mode, partition_by, evolve_schema, tuning)` (Delta `write_deltalake` with `mode` ∈ `error|append|overwrite|ignore`, `schema_mode="merge"`, `partition_by`, `target_file_size`, `writer_properties=` and `commit_properties=` projected from `tuning`; Iceberg `Transaction.append`/`overwrite`; Lance `write_dataset` with `mode` ∈ `create|overwrite|append` and `max_rows_per_file` from `tuning.target_file_size`) · `Read(version, columns, predicate)` (Delta `DeltaTable(uri, version=).to_pyarrow_table(columns=)` pushing the column projection, the predicate routed through the open Delta-DNF seam; Iceberg `Table.scan(row_filter=, selected_fields=, snapshot_id=).count()` binding the SQL-string predicate through `row_filter` (which natively accepts `str | BooleanExpression` and parses the string internally) and never dropping it, the catalogued `DataScan.count` row-count path over `.to_arrow().num_rows`; Lance `lance.dataset(uri, version=).to_table(columns, filter)` with the predicate riding `filter` directly, all zero-copy Arrow) · `Delete(predicate)` (Delta `DeltaTable.delete(predicate) -> dict`; Iceberg `Transaction.delete(delete_filter)`; Lance `LanceDataset.delete(predicate)`) · `Update(predicate, set)` (Delta `DeltaTable.update(updates=, predicate=) -> dict`; Iceberg and Lance reject — neither exposes a portable predicate-set row-update outside `merge`) · `Merge(predicate, updates, delete_unmatched)` (Delta `DeltaTable.merge(...) -> TableMerger` then `when_matched_update`/`when_not_matched_insert`/the `delete_unmatched`-gated `when_not_matched_by_source_delete`/`execute`, the full catalogued three-clause builder chain; Iceberg `Transaction.upsert(df, join_cols) -> UpsertResult`; Lance `LanceDataset.merge_insert(on).when_matched_update_all().when_not_matched_insert_all().execute(data)`) · `Evolve(adds, drops, renames)` (Delta `alter.add_columns([Field(name, dtype), ...])` over the `adds` clause, the `drops`/`renames` guard arm rejecting because `TableAlterer` exposes no column drop or rename; Iceberg the full `Transaction.update_schema()` context-managed `UpdateSchema` chaining `add_column(name, IcebergType.model_validate(dtype))`/`delete_column(column)`/`rename_column(old, new)` over all three clauses, the `(name, type-string)` add binding the `IcebergType` Pydantic string-parse; Lance rejects, no catalogued column-evolution member) · `Optimize(target_size, zorder)` (Delta `DeltaTable.optimize.compact`/`z_order`; Lance `LanceDataset.optimize.compact_files(target_rows_per_fragment=)` returning `CompactionMetrics.fragments_added`; the Iceberg arm rejects — `rewrite_data_files` absent from the Python `pyiceberg` API) · `Vacuum(retention_hours, dry_run)` (Delta `DeltaTable.vacuum -> list[str]`; Iceberg `Table.maintenance.expire_snapshots().older_than(_retention(retention_hours)).commit()` over a `datetime` cutoff; Lance `cleanup_old_versions(older_than=_age(retention_hours))` over a `timedelta` age returning `CleanupStats.old_versions`) · `ChangeFeed(start, end)` (Delta `load_cdf` Change Data Feed into a `RecordBatchReader`; non-Delta formats reach no portable arm) · `Index(column, kind, metric)` (Lance `LanceDataset.create_index(column, index_type, metric=)` ANN/scalar index — the multimodal/AI-asset rail the Lance format owns; Delta/Iceberg reject, no catalogued portable index surface) · `Restore(target)` (Delta `DeltaTable.restore(target) -> dict` writing a revert commit to a prior version or timestamp, the `num_removed_file`/`num_restored_file` metric keys read through `_delta_metric`; the Iceberg/Lance arms reach no portable arm — Iceberg time-travel rollback is `ManageSnapshots.rollback_to_snapshot` outside the `_PORTABLE` row until a catalogued revert-equivalent member lands), each binding the exact provider surface the `TableFormat` row selects.
- Entry: `Lakehouse.open` admits a `DatasetRef` and the `TableFormat` recovered from `DatasetKind` and returns the frozen owner over the resolved `table_uri`; `Lakehouse.run` folds one `LakeOp` through one `match (self.table_format, op)` over the portable `(format, op)` cross-product with the `case _, _` arm as total reject, each portable arm returning a `RuntimeRail[LakeReceipt]` directly so a non-portable `(format, op)` is `Error(BoundaryFault(boundary=(...)))` and a provider-thrown exception crosses `boundary` once; time-travel is `LakeOp.Read(version=...)`, never a parallel `read_at_version` entrypoint, and a standalone delete is `LakeOp.Delete(...)`, never a `delete_delta`/`delete_iceberg` family.
- Auto: `run` wraps `boundary` once around one `match (self.table_format, op)` and self-flattens the nested rail through `.bind(lambda rail: rail)` so the provider exception and the typed reject share one `RuntimeRail[LakeReceipt]` — a Delta operation, an Iceberg `Transaction`, a Lance `LanceDataset` all fold into the same carrier; a non-portable pair falls to the `case _, _` arm calling `_reject(op)`, which reads `op.tag not in _PORTABLE[self.table_format]` and returns `Error(BoundaryFault(boundary=(f"lake.{self.table_format}.{op.tag}", ...)))` directly so the typed key survives — the deleted form is a `raise` into the `boundary` re-key that overwrites it via `BoundaryFault.of` — and only a provider-thrown exception routes through `boundary`; `_PORTABLE` rows `DELTA` over the ten non-index tags, `ICEBERG` over `write|read|delete|merge|evolve|vacuum`, and `LANCE` over `write|read|delete|merge|index|optimize|vacuum`, so `changefeed` and `restore` are the two Delta-exclusive tags and `optimize` stays Iceberg-exclusive-of-Delta only because `rewrite_data_files` is absent from the Python `pyiceberg` API; the one `Vacuum.retention_hours` axis projects through `_retention` to a `datetime.now(UTC) - timedelta(hours=...)` cutoff the Iceberg `expire_snapshots().older_than()` consumes and through `_age` to a `timedelta(hours=...)` the Lance `cleanup_old_versions(older_than=)` consumes, both defaulting to `_DEFAULT_RETENTION_HOURS` when the axis is `None`; the Iceberg `evolve` arm opens `update_schema()` as a context manager and folds `add_column`/`delete_column`/`rename_column` over the three `Evolution` clauses inside the autocommitting `with`, the Delta `evolve` arm binds `alter.add_columns` over the `adds` clause with a guard arm rejecting `drops`/`renames`; `WriteTuning.writer_properties` projects to `deltalake.WriterProperties` with nested `ColumnProperties`/`BloomFilterProperties`; Delta `optimize` is a property exposing `compact(target_size=)`/`z_order(columns, target_size=)`; `restore` binds `DeltaTable.restore(target)` over an `int` version or a `datetime` timestamp, writing a new revert commit rather than mutating history, the `num_removed_file`/`num_restored_file` metric keys read through `_delta_metric`; the changefeed rides `load_cdf(starting_version, ending_version)` returning a `pyarrow.RecordBatchReader` with the `_change_type`/`_commit_version`/`_commit_timestamp` CDF columns the `columnar#MATERIALIZE` `DerivedSnapshot._materialize` partition-groups; Iceberg writes flow through `load_catalog`-acquired `Transaction` (`load_catalog` the single polymorphic catalog entry, REST discriminated by `uri` scheme), the Iceberg read binds `Table.scan(...).count()` over the catalogued `DataScan.count` row, and the Iceberg snapshot identity reads the last `snapshot_id` of `InspectTable.snapshots()` over `table.inspect`; `lance` is the `pylance` dist module (`import lance`, never `import pylance`); `pyiceberg` rides the `python_version<'3.15'` gated band so its arm imports the dist function-local under `# noqa: PLC0415`, never a module-top import on this cp315-core page, while `deltalake` (abi3) and `lance`/`pylance` (abi3) import module-top.
- Receipt: the commit contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces a `LakeReceipt` keyed by `ContentIdentity.of` over the format tag plus snapshot version plus add-action/data-file URIs; the snapshot-identity read is one polymorphic `_snapshot` method discriminating `match self.table_format` — Delta `DeltaTable.version()` plus `get_add_actions().num_rows` file count, Iceberg the last `snapshot_id` of `InspectTable.snapshots()`, Lance the scalar `LanceDataset.version` `int` property — folded by one `_receipt` projector, never three sibling `_<format>_snapshot` factories nor a parallel `_SNAPSHOT` dict.
- Packages: `deltalake` (`DeltaTable`/`write_deltalake`/`load_cdf`/`optimize`/`vacuum`/`restore`/`merge`/`TableMerger.{when_matched_update,when_not_matched_insert,when_not_matched_by_source_delete,execute}`/`delete`/`update`/`alter.add_columns`/`schema.Field`/`get_add_actions`/`version`/`WriterProperties`/`ColumnProperties`/`BloomFilterProperties`/`CommitProperties`), `pyiceberg` (`load_catalog`/`Catalog.load_table`/`Table.transaction`/`Transaction.{append,overwrite,upsert,delete,update_schema,commit_transaction}`/`UpdateSchema.{add_column,delete_column,rename_column}`/`types.IcebergType.model_validate`/`Table.maintenance.expire_snapshots`/`ExpireSnapshots.{older_than,commit}`/`Table.scan(row_filter=,selected_fields=,snapshot_id=)` over a native `str | BooleanExpression` filter parsed by `expressions.parser.parse`/`DataScan.{count,to_arrow}`/`InspectTable.snapshots`, the one `<3.15` gated arm whose `Table`/`DataScan` annotations ride `TYPE_CHECKING`), `pylance` (`lance.dataset`/`write_dataset(mode=,max_rows_per_file=)`/`LanceDataset.{to_table,merge_insert,delete,create_index,version,optimize,cleanup_old_versions}`/`DatasetOptimizer.compact_files(target_rows_per_fragment=)`/`MergeInsertBuilder.{when_matched_update_all,when_not_matched_insert_all,execute}`), `pyarrow` (`Table`/`RecordBatchReader`), `duckdb` (queries any format's Arrow snapshot via `from_arrow`), runtime (`RuntimeRail`/`BoundaryFault`/`boundary`/`ContentIdentity`/`ReceiptContributor`/`Receipt`).
- Growth: a new lake operation is one `LakeOp` case absorbed by the `(format, tag)` dispatch; a new write mode is a `Literal` row on `Write`; a new writer-tuning knob is a field on `WriteTuning`; a new Lance index kind is a `Literal` row on `Index`; a fourth table format (Hudi, Paimon) is one `TableFormat` member plus its `(format, *)` arms on this same owner, never a parallel owner.
- Boundary: no durable store, no schema migration, no global Delta/catalog connection; a `read_delta`/`write_delta`/`delete_delta`/`optimize_delta` family, a per-operation class family, a parallel `WriteTuned` op, three sibling `_<format>_receipt`/`_<format>_snapshot` factories, a `_SNAPSHOT` dispatch dict beside the `match`, a `raise BoundaryFault` reject path into a `boundary` that re-keys it, and a parallel `IcebergLakehouse`/`LanceLakehouse` pair are the deleted forms. The Iceberg/Lance arms reject the ops their provider surface does not portably reach — the `_PORTABLE` row per format names exactly the reachable tags, so Iceberg rejects `Update`/`Optimize`/`ChangeFeed`/`Index`/`Restore` (`rewrite_data_files`, `load_cdf`, and a version-or-timestamp revert all absent from the Python `pyiceberg` API) and Lance rejects `Update`/`Evolve`/`ChangeFeed`/`Restore` as `Error(BoundaryFault(boundary=(...)))`, never a silent no-op; a rollback or change-feed arm lands as the format's tag added to its `_PORTABLE` row plus one dispatch arm before the catch-all, never a new owner.

```python signature
from datetime import UTC, datetime, timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Literal, assert_never

import lance
import pyarrow as pa
from deltalake import (
    BloomFilterProperties,
    ColumnProperties,
    CommitProperties,
    DeltaTable,
    WriterProperties,
    write_deltalake,
)
from deltalake.schema import Field
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.data.tabular.columnar import DatasetKind, DatasetRef
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from pyiceberg.table import DataScan, Table

type WriteMode = Literal["error", "append", "overwrite", "ignore"]
type Compression = Literal["zstd", "snappy", "gzip", "lz4", "brotli", "uncompressed"]
type IndexKind = Literal["IVF_PQ", "IVF_HNSW_PQ", "BTREE"]
type Metric = Literal["L2", "cosine", "dot"]
type Evolution = tuple[tuple[tuple[str, str], ...], tuple[str, ...], tuple[tuple[str, str], ...]]


class TableFormat(StrEnum):
    DELTA = "delta"
    ICEBERG = "iceberg"
    LANCE = "lance"


class WriteTuning(Struct, frozen=True):
    compression: Compression = "zstd"
    statistics_truncate_length: int | None = None
    target_file_size: int | None = None
    bloom_columns: tuple[str, ...] = ()
    custom_metadata: dict[str, str] = {}
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
    update: tuple[str, dict[str, str]] = case()
    merge: tuple[str, dict[str, str], bool] = case()
    evolve: Evolution = case()
    optimize: tuple[int | None, tuple[str, ...]] = case()
    vacuum: tuple[int | None, bool] = case()
    changefeed: tuple[int, int | None] = case()
    index: tuple[str, IndexKind, Metric] = case()
    restore: tuple[int | datetime] = case()

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
    def Update(predicate: str, set: dict[str, str]) -> "LakeOp":
        return LakeOp(update=(predicate, set))

    @staticmethod
    def Merge(predicate: str, updates: dict[str, str], delete_unmatched: bool = False) -> "LakeOp":
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

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "lakehouse",
            self.table_uri,
            {"format": self.table_format, "op": self.operation, "version": str(self.version), "added": str(self.files_added)},
        )


class Lakehouse(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    catalog: str | None = None
    identifier: str | None = None

    @classmethod
    def open(
        cls, dataset: DatasetRef, table_format: TableFormat = TableFormat.DELTA, *, catalog: str | None = None, identifier: str | None = None
    ) -> "RuntimeRail[Lakehouse]":
        if table_format is TableFormat.DELTA and dataset.kind is not DatasetKind.DELTA:
            return Error(BoundaryFault(resource=("not-delta", dataset.ref.relative)))
        if table_format is TableFormat.ICEBERG and (catalog is None or identifier is None):
            return Error(BoundaryFault(resource=("iceberg-needs-catalog", dataset.ref.relative)))
        return Ok(cls(table_uri=str(dataset.ref.path), table_format=table_format, catalog=catalog, identifier=identifier))

    def run(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        return boundary(f"lake.{self.table_format}.{op.tag}", lambda: self._apply(op, data)).bind(lambda rail: rail)

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
            case TableFormat.DELTA, LakeOp(tag="read", read=(version, columns, _predicate)):
                rows = DeltaTable(self.table_uri, version=version).to_pyarrow_table(columns=list(columns) or None).num_rows
                return self._receipt("read", snapshot=rows)
            case TableFormat.DELTA, LakeOp(tag="delete", delete=(predicate,)):
                metrics = DeltaTable(self.table_uri).delete(predicate)
                return self._receipt("delete", removed=_delta_metric(metrics, "num_deleted_rows"))
            case TableFormat.DELTA, LakeOp(tag="update", update=(predicate, updates)):
                metrics = DeltaTable(self.table_uri).update(updates=updates, predicate=predicate)
                return self._receipt("update", snapshot=_delta_metric(metrics, "num_updated_rows"))
            case TableFormat.DELTA, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                merger = DeltaTable(self.table_uri).merge(data, predicate=predicate).when_matched_update(updates=updates).when_not_matched_insert(updates=updates)
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
                rows = self._iceberg_scan(version if isinstance(version, int) else None, columns, predicate).count()
                return self._receipt("read", snapshot=rows)
            case TableFormat.ICEBERG, LakeOp(tag="delete", delete=(predicate,)):
                self._iceberg().transaction().delete(predicate).commit_transaction()
                return self._receipt("delete")
            case TableFormat.ICEBERG, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                self._iceberg().transaction().upsert(data, join_cols=list(updates.keys())).commit_transaction()
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
                lance.write_dataset(data, self.table_uri, mode="append" if mode == "append" else "overwrite", max_rows_per_file=tuning.target_file_size or 1024 * 1024)
                return self._receipt("write")
            case TableFormat.LANCE, LakeOp(tag="read", read=(version, columns, predicate)):
                rows = lance.dataset(self.table_uri, version=version).to_table(columns=list(columns) or None, filter=predicate).num_rows
                return self._receipt("read", snapshot=rows)
            case TableFormat.LANCE, LakeOp(tag="delete", delete=(predicate,)):
                lance.dataset(self.table_uri).delete(predicate)
                return self._receipt("delete")
            case TableFormat.LANCE, LakeOp(tag="merge", merge=(predicate, _updates, _delete_unmatched)):
                lance.dataset(self.table_uri).merge_insert(predicate).when_matched_update_all().when_not_matched_insert_all().execute(data)
                return self._receipt("merge")
            case TableFormat.LANCE, LakeOp(tag="index", index=(column, kind, metric)):
                lance.dataset(self.table_uri).create_index(column, index_type=kind, metric=metric)
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

    def _iceberg_scan(self, version: int | None, columns: tuple[str, ...], predicate: str | None) -> "DataScan":
        return self._iceberg().scan(
            row_filter=predicate or "true",
            selected_fields=tuple(columns) or ("*",),
            snapshot_id=version,
        )

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
        return Ok(
            LakeReceipt(
                table_uri=self.table_uri,
                table_format=self.table_format,
                operation=operation,
                version=version,
                files_added=added or snapshot,
                files_removed=removed,
                content_key=ContentIdentity.of(self.table_format, f"{self.table_uri}@{version}".encode()),
            )
        )


_PORTABLE: dict[TableFormat, frozenset[str]] = {
    TableFormat.DELTA: frozenset({"write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "restore"}),
    TableFormat.ICEBERG: frozenset({"write", "read", "delete", "merge", "evolve", "vacuum"}),
    TableFormat.LANCE: frozenset({"write", "read", "delete", "merge", "index", "optimize", "vacuum"}),
}

_DEFAULT_RETENTION_HOURS = 168


def _delta_metric(metrics: dict[str, object], key: str) -> int:
    return int(metrics.get(key, 0))


def _retention(retention_hours: int | None) -> datetime:
    return datetime.now(UTC) - timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)


def _age(retention_hours: int | None) -> timedelta:
    return timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)
```

## [03]-[RESEARCH]

- [ICEBERG_SNAPSHOT]: SETTLED — the `pyiceberg` `load_catalog`/`Catalog.load_table`/`Table.transaction`/`Transaction.{append,overwrite,upsert,delete,commit_transaction}`/`Table.scan(row_filter=, selected_fields=, snapshot_id=)`/`DataScan.count()` surface and the `Table.inspect.snapshots()` metadata-table accessor the `_iceberg_scan`/`_snapshot` Iceberg arm binds are catalogue-confirmed against the folder `pyiceberg` `.api` (`.api/pyiceberg.md` scan row [01], egress/scan rows [03],[09], write rows [01]-[04],[06], and inspection row [10]); the `row_filter=predicate or "true"` no-predicate sentinel reads the native `str | BooleanExpression` scan parameter (scan rows [01],[10]) where `"true"` parses to `AlwaysTrue`, the selected-fields/snapshot-id projection reads scan row [01] verbatim with `_iceberg_scan` typed `int | None` over the catalogued `snapshot_id` parameter — the `Read.version` `str`/`datetime` forms are Delta/Lance time-travel spellings the Iceberg arm narrows to `None` at the call site (`version if isinstance(version, int) else None`) because the catalogued `snapshot_id` accepts only an `int` snapshot id, the erased `object` version parameter being the deleted form — the Iceberg `Read` row-count binds the catalogued `DataScan.count()` (scan row [09]) over a materializing `.to_arrow().num_rows`, and the `_snapshot` Iceberg arm reads the current snapshot id from the last `snapshot_id` row of the catalogued `InspectTable.snapshots()` Arrow table (the inspection surface the catalogue's reject-law mandates over `raw metadata-file access outside InspectTable`), so the `Table.metadata.current_snapshot_id` private-attribute read is the deleted form. `pyiceberg` rides the `python_version<'3.15'` gated band, so `load_catalog`/`IcebergType` import function-local under `# noqa: PLC0415`; a module-top `pyiceberg` import on this cp315-core page is the floor-violating form.
- [STRING_PREDICATE_FILTER]: SETTLED for Iceberg and Lance, RESEARCH-open for Delta — the `LakeOp.Read.predicate` string rides the Lance read directly (`LanceDataset.to_table(filter=)` accepts a SQL-string filter, catalogued mutation/scan row [04]) and the Iceberg read directly because `Table.scan(row_filter: str | BooleanExpression = ALWAYS_TRUE)` natively accepts a string and parses it through `pyiceberg.expressions.parser.parse(expr) -> BooleanExpression` internally (catalogued scan rows [01],[10]). The `_iceberg_filter`/`AlwaysTrue()` indirection is the deleted form: `_iceberg_scan` passes `row_filter=predicate or "true"` straight through, the empty-predicate sentinel `"true"` parsing to `AlwaysTrue`, so the predicate is never dropped and no `NotImplementedError` gate remains. The Delta read still drops the predicate because `DeltaTable.to_pyarrow_table(filters=)` takes pyarrow disjunctive-normal-form filter tuples, not the op's SQL string (catalogued read-egress row), so a Delta string-predicate push is the one open seam — a SQL-string to DNF-tuple lowering, not a parser the `pyiceberg` surface already owns.
- [LANCE_VERSION]: SETTLED — the `pylance` `lance.dataset(uri, version=)`/`write_dataset(data, uri, mode=, max_rows_per_file=)`/`LanceDataset.{to_table,merge_insert,delete}`/`MergeInsertBuilder.{when_matched_update_all,when_not_matched_insert_all,execute}` surface and the scalar `LanceDataset.version -> int` `@property` the `_snapshot` Lance arm reads are catalogue-confirmed against the folder `pylance` `.api` (`.api/pylance.md` I/O rows [01]-[02], version-and-mutation rows [01]-[04], reflection-verified at `[LANCE_TOPOLOGY]`): the scalar `.version` current-checked-out accessor and the plural `versions()` history rail are distinct confirmed members, so the write/read/delete/merge snapshot reads `.version` as the settled identity with no live-distribution gate remaining, folding the prior `[LANCE_VERSION_ACCESSOR]` block where the catalogue listed only `versions()`. The `write_dataset(max_rows_per_file=)` Parquet-fragment sizing the Lance `Write` arm threads from `tuning.target_file_size` reads I/O row [02] verbatim, so the Lance `Write` tuning is settled fence code, not a knob tail.
- [LANCE_INDEX]: SETTLED — the `pylance` `LanceDataset.create_index(column, index_type, *, metric, replace)` ANN/scalar-index entry the `Index` op binds is catalogue-confirmed against the folder `pylance` `.api` (mutation row [05], topology row); the `IndexKind` `Literal["IVF_PQ","IVF_HNSW_PQ","BTREE"]` and `Metric` `Literal["L2","cosine","dot"]` axes transcribe the catalogued `index_type`/`metric` value sets verbatim (`[LANCE_TOPOLOGY]` index row), so the Lance `Index` arm is settled fence code and the Delta/Iceberg `(format, index)` pairs fall to the catch-all reject because no portable index surface is catalogued for either.
- [DELTA_MERGE_CHAIN]: SETTLED — the `deltalake` `DeltaTable.merge(source, predicate=) -> TableMerger` then `when_matched_update(updates=)`/`when_not_matched_insert(updates=)`/the `delete_unmatched`-gated `when_not_matched_by_source_delete()`/`execute()` builder chain the Delta `merge` arm binds is catalogue-confirmed against the folder `deltalake` `.api` (mutate entrypoint row and implementation-law `TableMerger` chain naming all three clauses `when_matched_update`/`when_not_matched_insert`/`when_not_matched_by_source_delete`); the prior `when_not_matched_insert_all()` spelling is the deleted form — no `_all` Delta-merge clause is catalogued, so the chain binds the catalogued `when_not_matched_insert(updates=)` member, and the previously-dropped third clause is now bound through the `Merge` `delete_unmatched` discriminant selecting `when_not_matched_by_source_delete()` rather than a parallel `MergeDelete` op, so the full catalogued upsert-plus-delete-on-no-match chain is settled fence code.
- [DELTA_WRITER_TUNING]: SETTLED — the `deltalake` `WriterProperties`/`ColumnProperties`/`BloomFilterProperties`/`CommitProperties` types and the `write_deltalake(writer_properties=, target_file_size=, commit_properties=)` parameters the `WriteTuning` policy projects into are catalogue-confirmed against the folder `deltalake` `.api` (public-types rows for the writer-property and commit-property carriers, write entrypoint row), and the exact keyword spellings the projectors bind are confirmed against the live distribution: `WriterProperties(compression=, statistics_truncate_length=, column_properties=)`, `ColumnProperties(bloom_filter_properties=)`, `BloomFilterProperties(set_bloom_filter_enabled=)`, and `CommitProperties(custom_metadata=, max_commit_retries=, app_transactions=)` — `WriteTuning.custom_metadata`/`max_commit_retries` thread the catalogued capability rather than a bare `CommitProperties()`, so `WriteTuning` is settled fence code with no open seam.
- [DELTA_ROW_MUTATION]: SETTLED — the `deltalake` `DeltaTable.delete(predicate) -> dict` and `DeltaTable.update(updates=, predicate=) -> dict` row-mutation methods the `Delete`/`Update` arms bind are catalogue-confirmed against the folder `deltalake` `.api` (mutate entrypoint row), and the `dict` return-metric keys `num_deleted_rows`/`num_updated_rows` the `_delta_metric` projector reads are confirmed against the live distribution docstrings (`update` returns `num_updated_rows`, `delete` returns `num_deleted_rows` and omits the key only on a predicate-less full delete — the metric-dict note on the `.api` implementation-law row). `_delta_metric` isolates the one key per call and reads `0` when the key is absent, so a predicate-less delete folds to a zero receipt count rather than faulting.
- [DELTA_RESTORE]: SETTLED at the entry, RESEARCH at the metric keys — the `deltalake` `DeltaTable.restore(target, *, ignore_missing_files=False, protocol_downgrade_allowed=False, ...) -> dict` time-travel-revert entry the `Restore` op binds is catalogue-confirmed against the folder `deltalake` `.api` (time-travel entrypoint row), so the `int`-version-or-`datetime`-timestamp `target` and the new-revert-commit semantics (`restore` writes a new commit reverting the table rather than mutating history, implementation-law row) are settled fence code; the `dict` return-metric keys `num_removed_file`/`num_restored_file` the `_delta_metric` projector reads are RESEARCH-pending against the live distribution, isolated to the one provisional key per `_delta_metric` call, the revert committing regardless and only the receipt count gating. `Restore` is the second Delta-exclusive tag beside `changefeed`: Iceberg time-travel rollback is the catalogued `ManageSnapshots.rollback_to_snapshot(snapshot_id)` (snapshot row [09]) rather than a version-or-timestamp revert shorthand, so the Iceberg `restore` arm rejects until a catalogued revert-equivalent member lands, and Lance carries no catalogued revert surface.
- [SCHEMA_EVOLVE]: SETTLED — the `Evolve` op binds the catalogued schema-mutation surface on both writing formats with no `NotImplementedError` gate remaining. Delta binds `alter.add_columns([Field(name, dtype), ...])` (`.api/deltalake.md` evolve entrypoint row plus the `Field(name, type)` string-type note); `TableAlterer` exposes only `add_columns`, so the `drops`/`renames` guard arm rejects with a typed `BoundaryFault` and the `(name, type-string)` add tuple constructs `deltalake.schema.Field(name, dtype)` directly because `Field` accepts a primitive type string. Iceberg binds the full `Transaction.update_schema()` context-managed `UpdateSchema` (`.api/pyiceberg.md` evolution rows [07],[11]) chaining `add_column(name, IcebergType.model_validate(dtype))`/`delete_column(column)`/`rename_column(old, new)` over all three `Evolution` clauses inside the autocommitting `with`, where `delete_column(path)` is the catalogued portable column-drop the prior reject claim wrongly denied, and the `(name, type-string)` add binds `pyiceberg.types.IcebergType.model_validate(dtype)` — the Pydantic wrap validator parses a primitive type string to a `StringType()`/`LongType()` instance, the settled string-to-type bridge. Lance rejects `Evolve`, no catalogued column-evolution member.
- [ICEBERG_LANCE_MAINTENANCE]: SETTLED at the bound maintenance arms, RESEARCH-permanent at the genuinely-absent surfaces — the `_PORTABLE` reject-set table is the settled reject-law: `DELTA` rows all ten non-index tags, `ICEBERG` rows `write|read|delete|merge|evolve|vacuum`, `LANCE` rows `write|read|delete|merge|index|optimize|vacuum`, so the `case _, _` arm reads the table and the maintenance gaps are the tags each row omits, never a buried `_PORTABLE_DELTA_ONLY` set forcing every non-Delta op through the catch-all. The Iceberg `Vacuum` arm binds `Table.maintenance.expire_snapshots().older_than(dt).commit()` (`.api/pyiceberg.md` maintenance rows [12]-[13]) over an autocommitting `Transaction`, and the Lance `Optimize`/`Vacuum` arms bind `LanceDataset.optimize.compact_files(target_rows_per_fragment=)` returning `CompactionMetrics.fragments_added` and `cleanup_old_versions(older_than=, retain_versions=)` returning `CleanupStats.old_versions` (`.api/pylance.md` maintenance rows [07]-[10]) — these are settled fence code, the one `retention_hours` axis projecting through `_retention` to a `datetime` cutoff and `_age` to a `timedelta` age. The Iceberg `Optimize` arm stays reject-permanent (`rewrite_data_files` is absent from the Python `pyiceberg` API), Iceberg `remove_orphan_files` is reject-permanent (no public Python member — `ObjectStoreLocationProvider` references orphan removal only in a layout comment), the Iceberg `Restore` arm rejects until a catalogued version-or-timestamp revert lands (only `ManageSnapshots.rollback_to_snapshot(snapshot_id)` is catalogued, snapshot row [09]), and `changefeed`/`restore` reach no non-Delta arm because the `columnar#MATERIALIZE` `DerivedSnapshot._materialize` reads the CDF only through the Delta `load_cdf` surface and the revert commit only through `DeltaTable.restore`.
