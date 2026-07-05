# [PY_DATA_LAKEHOUSE]

The transactional table-format interchange owner: one `LakeOp` operation axis crossed with one `TableFormat` provider axis on one `Lakehouse` owner, admitting Delta, Iceberg (the core-loadable DuckDB `iceberg` extension the PRIMARY read path, `pyiceberg` the `<3.15` catalog-write fallback), Lance (multimodal/AI-asset versioning with `create_index` ANN), and DuckLake (the SQL-catalog lakehouse-over-Parquet riding one `ATTACH 'ducklake:<dsn>'` over the shared `tabular/columnar#SCAN` `DuckDbSession` rail). `Lakehouse.run` folds the write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore lifecycle through one `LakeOp` tagged union, dispatching one `(TableFormat, tag)` provider arm that returns a `RuntimeRail[LakeReceipt]` directly; `LakeReceipt` is the typed commit receipt — format, version, operation, files-added/removed, content-key. The operation axis is format-agnostic; the format binding is a separate discriminant, so a new format is one `TableFormat` row plus its dispatch arms, never a parallel Iceberg/Lance owner. The reject-law is data, not a buried catch-all: one `_PORTABLE` `Map[TableFormat, frozenset[str]]` rows each format's reachable op tags, and a `(format, op)` outside the row falls to the `case _, _` arm returning `Error(BoundaryFault(boundary=(f"lake.{format}.{op}", ...)))` directly — the deleted form is a `raise` into a `boundary` re-key that discards the typed key through `BoundaryFault.of`; only genuine provider-thrown exceptions cross the `boundary` conversion. Maintenance is provider-portable: Iceberg `Vacuum` binds `Table.maintenance.expire_snapshots().older_than().commit()`, Lance `Optimize`/`Vacuum` bind `LanceDataset.optimize.compact_files()`/`cleanup_old_versions()`, DuckLake `Optimize`/`Vacuum` bind the `ducklake_merge_adjacent_files`/`ducklake_expire_snapshots`+`ducklake_cleanup_old_files` maintenance CALLs, and the one polymorphic `retention_hours` axis projects to a `datetime` cutoff for Iceberg, a `timedelta` age for Lance, and an interval literal for DuckLake. `changefeed` is Delta `load_cdf` AND DuckLake `table_changes` (the two CDC-bearing formats the `tabular/materialize#MATERIALIZE` `DerivedSnapshot._materialize` consumer reads); `restore` writes a revert commit through `DeltaTable.restore` on Delta, rolls back through the catalogued `ManageSnapshots.rollback_to_snapshot` on Iceberg, and re-heads a prior snapshot through `checkout_version`/`restore()` on Lance (`asof=` resolving a timestamp target). Every commit is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[LAKEHOUSE]: the transactional table-format lakehouse owner over one `LakeOp` operation axis crossed with one `TableFormat` provider axis.

## [02]-[LAKEHOUSE]

- Owner: `Lakehouse` — the one transactional-table owner over `deltalake`/`pyiceberg`/`lance`/the DuckDB `iceberg`+`ducklake` extension rows; `LakeOp` the tagged-union operation axis (write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore), matched by `match`/`case` so a new table operation is one `LakeOp` case, never a `read_delta`/`write_delta`/`delete_delta`/`compact_delta` method family; `TableFormat` the `StrEnum` provider axis (`DELTA`/`ICEBERG`/`LANCE`/`DUCKLAKE`) the owner dispatches one `(format, tag)` arm against, so the operation axis and the provider axis are two orthogonal discriminants. The writer-tuning axis rides one `WriteTuning` policy `Struct` carried on `Write`, never a parallel `WriteTuned` op or a knob tail; the merge delete-on-no-match axis rides one `delete_unmatched` discriminant on `Merge` selecting the catalogued third `TableMerger.when_not_matched_by_source_delete` clause, never a parallel `MergeDelete` op. `LakeReceipt` is the typed commit receipt — format, version, operation, files-added/removed, content-key — folded by one `_receipt` projector that reads the post-op snapshot identity off one polymorphic `_snapshot` method discriminating `match self.table_format` closed by `assert_never`, never three sibling `_<format>_receipt` factories nor a parallel `_SNAPSHOT` dispatch dict over three module-level `_<format>_snapshot` functions. The reject-law is data, not a buried catch-all: `_PORTABLE` is one `Map[TableFormat, frozenset[str]]` row per format naming the portable op tags, the `case _, _` arm reading it so a `(format, op)` outside the format's portable set is `Error(BoundaryFault(boundary=(...)))`; `changefeed` reaches the Delta `load_cdf` and DuckLake `table_changes` arms the `tabular/materialize#MATERIALIZE` `DerivedSnapshot._materialize` consumer reads, and `restore` reaches the Delta `DeltaTable.restore` revert commit, the Iceberg `ManageSnapshots.rollback_to_snapshot` rollback, and the Lance `checkout_version`/`restore()` re-head.
- Cases: `LakeOp` rows `Write(mode, partition_by, evolve_schema, tuning)` (Delta `write_deltalake` with `mode` ∈ `error|append|overwrite|ignore`, `schema_mode="merge"`, `partition_by`, `target_file_size`, `writer_properties=`/`commit_properties=`/`post_commithook_properties=` projected from `tuning` — the `create_checkpoint`/`cleanup_expired_logs` post-commit hooks mined as `WriteTuning` rows; Iceberg `Transaction.append`/`overwrite` through the `<3.15` pyiceberg catalog-write fallback, a non-empty `partition_by` a TYPED reject naming the table-spec ownership (Iceberg partitioning is table metadata authored at create through `PartitionSpec(PartitionField(source_id, field_id, transform, name))` over the `BucketTransform`/`YearTransform`/`MonthTransform`/`DayTransform` rows — the stated capability row, never a silent discard); DuckLake `INSERT INTO`/`CREATE OR REPLACE TABLE ... AS` over the attached catalog; Lance `write_dataset` whose `mode` is the op's `WriteMode` projected through the `_LANCE_MODE` data row onto the Lance `create|overwrite|append` band — `error` landing on `create` so the fail-on-existing contract survives, `ignore` no-opping on an existing dataset through the write arm's `_lance_exists` guard (Lance owns no native ignore mode) and creating only when absent, rather than a flat `else "overwrite"` collapse silently replacing the dataset — with `max_rows_per_file` from `tuning.target_file_size` and the on-disk file format pinned by the `tuning.data_storage_version` policy row, never the ambient provider default) · `Read(version, columns, predicate)` (the three-format row-count probe over the pinned version — all metadata-only, never a full-table materialize discarded down to `.num_rows`: Delta the version-pinned `DeltaTable(uri, version=)` count — the no-predicate path the metadata-only `to_pyarrow_dataset().count_rows()` over the version-pinned scan, the SQL-predicate path the native `deltalake.QueryBuilder` DataFusion SQL `register(table).execute("SELECT count(*) WHERE <predicate>").read_all()` that pushes the SQL string natively (the `deltalake`-owned SQL-over-Delta surface, so the predicate is never dropped and no SQL->pyarrow-DNF lowering owner is minted); Iceberg the PRIMARY core-loadable DuckDB `iceberg`-extension path — `SELECT count(*) FROM iceberg_scan('<uri>')` with the predicate a `WHERE` clause over the scan relation, the extension row riding the shared `DuckDbSession`; DuckLake the versioned `SELECT count(*) FROM <table> AT (VERSION => ?)` read over the attached catalog; Lance `lance.dataset(uri, version=|asof=).count_rows(filter=)` — an `int` snapshot or `str` tag on `version=`, a timestamp on `asof=` — pushing the SQL-string predicate into the catalogued count, never materializing the table — `columns` is irrelevant to a count and the real column-projected zero-copy read is the `columnar#SCAN` `scan_delta`/`scan_iceberg` lazy-reader lane, not this commit owner) · `Delete(predicate)` (Delta `DeltaTable.delete(predicate) -> dict`; Iceberg `Transaction.delete(delete_filter)`; Lance `LanceDataset.delete(predicate)`; DuckLake the SQL `DELETE FROM ... WHERE`) · `Update(predicate, updates)` (the `updates` a `Map[str, str]` carried immutable on the frozen op, coerced to a plain `dict` only at the delta-rs seam — Delta `DeltaTable.update(updates=dict(updates.items()), predicate=) -> dict`; DuckLake the SQL `UPDATE ... SET ... WHERE`; Iceberg and Lance reject — neither exposes a portable predicate-set row-update outside `merge`) · `Merge(predicate, updates, delete_unmatched)` (the `updates` `Map[str, str]` coerced once into the `dict` both Delta clauses read — Delta `DeltaTable.merge(...) -> TableMerger` then `when_matched_update`/`when_not_matched_insert`/the `delete_unmatched`-gated `when_not_matched_by_source_delete`/`execute`, the full catalogued three-clause builder chain; Iceberg `Transaction.upsert(df, join_cols=list(updates.keys())) -> UpsertResult`; Lance `LanceDataset.merge_insert(list(updates.keys())).when_matched_update_all().when_not_matched_insert_all().execute(data)`; DuckLake the native DuckDB `MERGE INTO` over the registered payload — the `predicate` SQL condition is the Delta/DuckLake merge spelling, while the column-keyed Iceberg/Lance arms derive the join key from the `updates` columns, never the SQL string as a column name) · `Evolve(adds, drops, renames, constraints)` (Delta `alter.add_columns([Field(name, dtype), ...])` over the `adds` clause plus `alter.add_constraint(dict(constraints.items()))` over the mined governance clause, the `drops`/`renames` guard arm rejecting because `TableAlterer` exposes no column drop or rename; Iceberg the full `Transaction.update_schema()` context-managed `UpdateSchema` chaining `add_column(name, IcebergType.model_validate(dtype))`/`delete_column(column)`/`rename_column(old, new)` over all three clauses, the `(name, type-string)` add binding the `IcebergType` Pydantic string-parse; Lance rejects, no catalogued column-evolution member) · `Optimize(target_size, zorder)` (Delta `DeltaTable.optimize.compact`/`z_order`; Lance `LanceDataset.optimize.compact_files(target_rows_per_fragment=)` returning `CompactionMetrics.fragments_added`; the Iceberg arm rejects — `rewrite_data_files` absent from the Python `pyiceberg` API) · `Vacuum(retention_hours, dry_run)` (Delta `DeltaTable.vacuum -> list[str]`; Iceberg `Table.maintenance.expire_snapshots().older_than(_retention(retention_hours)).commit()` over a `datetime` cutoff; Lance `cleanup_old_versions(older_than=_age(retention_hours))` over a `timedelta` age returning `CleanupStats.old_versions`) · `ChangeFeed(start, end)` (Delta `load_cdf` Change Data Feed into an `arro3.core.RecordBatchReader`, the local arm reading `.read_all().num_rows`; DuckLake the `SELECT count(*) FROM table_changes('<table>', start, end)` CDC read; Iceberg/Lance reach no portable arm) · `Index(column, kind, metric)` (Lance — one op owning both Lance index families, routing the IVF vector kinds `IVF_PQ`/`IVF_HNSW_PQ`/`IVF_HNSW_SQ` through `LanceDataset.create_index(column, index_type, metric=)` and the scalar/FTS kinds `BTREE`/`BITMAP`/`LABEL_LIST`/`ZONEMAP`/`BLOOMFILTER`/`RTREE`/`INVERTED`/`FTS`/`NGRAM` through `create_scalar_index(column, index_type)` by `_VECTOR_INDEX` membership, the multimodal/AI-asset retrieval rail the Lance format owns; `metric` is consumed only by the vector arm; Delta/Iceberg reject, no catalogued portable index surface) · `Restore(target)` (Delta `DeltaTable.restore(target) -> dict` writing a revert commit to a prior version or timestamp, the `num_removed_file`/`num_restored_file` metric keys read through `_delta_metric`; Iceberg the catalogued `manage_snapshots().rollback_to_snapshot(snapshot_id).commit()` rollback over an `int` snapshot id, a non-`int` target a typed reject; Lance the `checkout_version`/`restore()` re-head — `lance.dataset(uri, version=target).restore()` over an `int` snapshot, a `datetime` resolved through `asof=` — re-committing the prior version as the new head; DuckLake reaches no portable arm), each binding the exact provider surface the `TableFormat` row selects.
- Entry: `Lakehouse.open` admits a `DatasetRef` plus an explicit `TableFormat` (defaulting to `DELTA`) and returns the frozen owner over the resolved `table_uri` as a `RuntimeRail[Lakehouse]`, cross-validating the format against `dataset.kind` rather than recovering it — `DatasetKind` carries no `LANCE` member, so the format cannot be folded from the kind, and the explicit axis is the one carrier admitting all three providers: a `DELTA` format over a non-`DatasetKind.DELTA` dataset, an `ICEBERG` format missing its `catalog`/`identifier`, and a `DUCKLAKE` format missing its `dsn`/`identifier` each return `Error(BoundaryFault(resource=(...)))` directly — the DuckLake `dsn` is a caller-supplied connection string resolved through the runtime `TransportResource` credential seam, never minted here. `Lakehouse.run` folds one `LakeOp` through one `match (self.table_format, op)` over the portable `(format, op)` cross-product with the `case _, _` arm as total reject, each portable arm returning a `RuntimeRail[LakeReceipt]` directly so a non-portable `(format, op)` is `Error(BoundaryFault(boundary=(...)))` and a provider-thrown exception crosses `boundary` once; time-travel is `LakeOp.Read(version=...)`, never a parallel `read_at_version` entrypoint, and a standalone delete is `LakeOp.Delete(...)`, never a `delete_delta`/`delete_iceberg` family.
- Receipt: the commit contributes an emitted-phase `Receipt.of("lakehouse", ("emitted", subject, facts))` row through `ReceiptContributor` (the runtime two-positional `Receipt.of(owner, evidence)` factory matching the `(phase, subject, facts)` evidence tuple, never a four-positional `Receipt.of(phase, owner, subject, facts)` shape) whose `facts` carries the `version`/`added` counts as native `int` the `dict[str, object]` `EventDict` and its `enc_hook=repr` renderer serialize without a `str()` pre-coerce; the `LakeReceipt` is keyed by `ContentIdentity.of("lake", f"{self.table_uri}@{version}".encode())`, which returns a `RuntimeRail[ContentKey]` the `_receipt` projector threads through `.map(lambda key: LakeReceipt(..., content_key=key))` so the receipt is built inside the rail and a digest fault propagates rather than a `Result` landing in the `content_key: ContentKey` slot — the literal `"lake"` `fmt` namespace and the `(table_uri, monotonic-version)` payload uniquely pin the committed snapshot, so the key is stable across a re-open of an unchanged version without a redundant add-action file-URI enumeration; the snapshot-identity read is one polymorphic `_snapshot` method discriminating `match self.table_format` — Delta `DeltaTable.version()` plus `get_add_actions().num_rows` file count, Iceberg the last `snapshot_id` of `InspectTable.snapshots()`, Lance the scalar `LanceDataset.version` `int` property — folded by one `_receipt` projector, never three sibling `_<format>_snapshot` factories nor a parallel `_SNAPSHOT` dict.
- Packages: `deltalake` (`DeltaTable`/`write_deltalake`/`load_cdf`/`optimize`/`vacuum`/`restore`/`merge`/`TableMerger.{when_matched_update,when_not_matched_insert,when_not_matched_by_source_delete,execute}`/`delete`/`update`/`to_pyarrow_dataset().count_rows()` the no-predicate metadata-only read-count probe/`QueryBuilder.register(name, table).execute(sql).read_all()` the DataFusion SQL surface the predicate-bearing Delta read-count pushes the SQL string through/`alter.add_columns`/`schema.Field`/`get_add_actions`/`version`/`WriterProperties`/`ColumnProperties`/`BloomFilterProperties`/`CommitProperties`), `pyiceberg` (`load_catalog`/`Catalog.load_table`/`Table.transaction`/`Transaction.{append,overwrite,upsert,delete,update_schema,commit_transaction}`/`UpdateSchema.{add_column,delete_column,rename_column}`/`types.IcebergType.model_validate`/`Table.maintenance.expire_snapshots`/`ExpireSnapshots.{older_than,commit}`/`manage_snapshots()`/`ManageSnapshots.{rollback_to_snapshot,create_branch,create_tag}` the catalogued rollback-and-reference surface the `Restore` arm binds/`PartitionSpec`/`PartitionField`/`BucketTransform`/`YearTransform`/`MonthTransform`/`DayTransform` the table-spec authoring vocabulary the partition capability row names/`Table.scan(row_filter=,selected_fields=,snapshot_id=)` over a native `str | BooleanExpression` filter parsed by `expressions.parser.parse`/`DataScan.{count,to_arrow}`/`InspectTable.snapshots`, the one `<3.15` gated arm whose `Table` annotation rides `TYPE_CHECKING`), `pylance` (`lance.dataset(uri, version=, asof=)` the full version-travel axis — an `int` snapshot or `str` tag on `version=`, a timestamp on `asof=` — /`write_dataset(mode=,max_rows_per_file=,data_storage_version=)`/`LanceDataset.{count_rows,merge_insert,delete,create_index,create_scalar_index,version,restore,optimize,cleanup_old_versions}`/`DatasetOptimizer.compact_files(target_rows_per_fragment=)`/`MergeInsertBuilder.{when_matched_update_all,when_not_matched_insert_all,execute}`), `pyarrow` (`Table` the write carrier the `data` param admits, `dataset.Dataset.count_rows()` the Delta read-count), `deltalake` governance rows (`PostCommitHookProperties(create_checkpoint=, cleanup_expired_logs=)` MINED as the `WriteTuning.create_checkpoint`/`cleanup_expired_logs` fields; `TableAlterer.add_constraint` MINED as the `Evolve.constraints` clause; `TableFeatures`/deletion-vector protocol enablement DECLINED — table-protocol governance is the C# Persistence owner's at-rest concern, never a data-side commit toggle), `arro3-core` (the `RecordBatchReader`/`Table` the Delta `load_cdf` change-feed egress returns, `.read_all().num_rows` the local row count), `tabular/columnar#SCAN` (`DuckDbSession`/`DuckDbExtension` — the ONE session rail the DuckLake `ATTACH 'ducklake:'` arms and the Iceberg-extension `iceberg_scan` read path ride, the `DUCKLAKE`/`ICEBERG` extension rows seed data on the shared table; `duckdb` additionally queries any format's Arrow snapshot via `from_arrow` and its native `MERGE INTO` backs the DuckLake merge arm; the `ducklake` SQL surface — `ATTACH 'ducklake:<dsn>'`, `snapshots()`, `table_changes(name, start, end)`, `ducklake_merge_adjacent_files`/`ducklake_expire_snapshots`/`ducklake_cleanup_old_files` CALLs — is the `data/.api/ducklake.md` catalog; `iceberg_scan`/`iceberg_snapshots` the `data/.api/duckdb.md` `[EXTENSIONS]` rows), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `open` factory and the caller-facing `run` submission so a non-`DatasetRef`/`TableFormat` or non-`LakeOp` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the enclosing `boundary`/`guarded_sync` fence rather than an untyped admission, the shared `FAULT_CONF` the sibling `interop`/`egress`/`columnar` admission seams bind; the `_receipt`/`LakeReceipt` projection over the owner's own committed snapshot carries no decorator), runtime (`RuntimeRail`/`BoundaryFault`/`boundary`/`FAULT_CONF` the shared beartype violation-redirect config/`ContentIdentity`/`ReceiptContributor`/`Receipt`, `reliability/resilience#RESILIENCE` `RetryClass.LAKE_COMMIT`/`guarded_sync` the sync commit-conflict retry envelope the mutating arms ride).
- Growth: a new lake operation is one `LakeOp` case absorbed by the `(format, tag)` dispatch; a new write mode is a `Literal` row on `Write`; a new writer-tuning knob is a field on `WriteTuning`; a new Lance vector index kind is a `Literal` row on `VectorIndex` (a scalar/FTS kind on `ScalarIndex`), both absorbed by the one `_VECTOR_INDEX`-routed `Index` arm; a new DuckDB-backed format capability is one `DuckDbExtension` row plus its `(DUCKLAKE, *)`/`(ICEBERG, *)` SQL arm over the shared session; an Iceberg create-with-spec write is one arm authoring `PartitionSpec` rows at table create; the DEFERRED version-reference residue is exactly the named authoring pair — Lance `tags.create`/`create_branch` and Iceberg `ManageSnapshots.create_branch`/`create_tag` — landing as ONE reference-authoring `LakeOp` case with per-format arms when a consumer names it (the read side of the axis is landed: tag-string/`asof` time-travel on `Read`, `checkout_version`/`restore` on `Restore`); a fifth table format (Hudi, Paimon) is one `TableFormat` member plus its `(format, *)` arms on this same owner, never a parallel owner.
- Boundary: no durable store, no schema migration, no global Delta/catalog connection; a `read_delta`/`write_delta`/`delete_delta`/`optimize_delta` family, a per-operation class family, a parallel `WriteTuned` op, three sibling `_<format>_receipt`/`_<format>_snapshot` factories, a `_SNAPSHOT` dispatch dict beside the `match`, a `raise BoundaryFault` reject path into a `boundary` that re-keys it, a parallel `IcebergLakehouse`/`LanceLakehouse` pair, a hand-opened `stamina.retry_context` commit-conflict loop minted on this page where the runtime `guarded_sync(RetryClass.LAKE_COMMIT, ...)` envelope owns the retry/span/lift triplet, a commit-conflict left unretried where the `op.mutating` fence routes every committing op through that envelope, and an undecorated `open`/`run` admitting a caller `DatasetRef`/`TableFormat`/`LakeOp` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `interop`/`egress`/`columnar` admission entrypoints share are the deleted forms. The Iceberg/Lance arms reject the ops their provider surface does not portably reach — the `_PORTABLE` row per format names exactly the reachable tags, so Iceberg rejects `Update`/`Optimize`/`ChangeFeed`/`Index` (`rewrite_data_files` and `load_cdf` absent from the Python `pyiceberg` API), Lance rejects `Update`/`Evolve`/`ChangeFeed` (its `Restore` is portable — `checkout_version`/`restore()` re-head a prior snapshot), and DuckLake rejects `Index`/`Restore`/`Evolve` as `Error(BoundaryFault(boundary=(...)))`, never a silent no-op; a hand-rolled `duckdb.connect()`-plus-install site where the `tabular/columnar#SCAN` `DuckDbSession` owns the lifecycle, and a silently-discarded `partition_by` on a format whose spec is table-owned where the typed capability reject names it, are deleted forms beside the rest; a rollback or change-feed arm lands as the format's tag added to its `_PORTABLE` row plus one dispatch arm before the catch-all, never a new owner.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Iterator
from contextlib import contextmanager
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
    PostCommitHookProperties,
    QueryBuilder,
    WriterProperties,
    write_deltalake,
)
from deltalake.schema import Field
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, field
from sqlglot import exp

from rasm.data.tabular.columnar import DatasetKind, DatasetRef, DuckDbExtension, DuckDbSession
from rasm.runtime.faults import BoundaryFault, FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded_sync

if TYPE_CHECKING:
    import duckdb
    from pyiceberg.table import Table

# --- [TYPES] ----------------------------------------------------------------------------

type WriteMode = Literal["error", "append", "overwrite", "ignore"]
type LanceMode = Literal["create", "overwrite", "append"]
type LanceStorage = Literal["stable", "2.1", "next"]
type Compression = Literal["zstd", "snappy", "gzip", "lz4", "brotli", "uncompressed"]
type VectorIndex = Literal["IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"]
type ScalarIndex = Literal["BTREE", "BITMAP", "LABEL_LIST", "ZONEMAP", "BLOOMFILTER", "RTREE", "INVERTED", "FTS", "NGRAM"]
type IndexKind = VectorIndex | ScalarIndex
type Metric = Literal["L2", "cosine", "dot"]
type Evolution = tuple[tuple[tuple[str, str], ...], tuple[str, ...], tuple[tuple[str, str], ...], Map[str, str]]


class TableFormat(StrEnum):
    DELTA = "delta"
    ICEBERG = "iceberg"
    LANCE = "lance"
    DUCKLAKE = "ducklake"


# --- [MODELS] ---------------------------------------------------------------------------


class WriteTuning(Struct, frozen=True):
    compression: Compression = "zstd"
    statistics_truncate_length: int | None = None
    target_file_size: int | None = None
    bloom_columns: tuple[str, ...] = ()
    custom_metadata: Map[str, str] = field(default_factory=lambda: Map.of_seq([]))
    max_commit_retries: int | None = None
    create_checkpoint: bool = True
    cleanup_expired_logs: bool = True
    # the Lance on-disk file format pinned as a POLICY row, never an ambient provider default.
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
    tag: Literal["write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "index", "restore"] = tag()
    write: tuple[WriteMode, tuple[str, ...], bool, WriteTuning] = case()
    read: tuple[int | str | datetime | None, tuple[str, ...], str | None] = case()
    delete: tuple[str] = case()
    update: tuple[str, Map[str, str]] = case()
    merge: tuple[str, Map[str, str], bool] = case()
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
    # the DuckLake catalog DSN — caller-resolved through the runtime `TransportResource` credential
    # seam and carried as data; the `ATTACH 'ducklake:<dsn>'` arm reads it, never a minted credential.
    dsn: str | None = None

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
    ) -> "RuntimeRail[Lakehouse]":
        if table_format is TableFormat.DELTA and dataset.kind is not DatasetKind.DELTA:
            return Error(BoundaryFault(resource=("not-delta", dataset.ref.relative)))
        if table_format is TableFormat.ICEBERG and (catalog is None or identifier is None):
            return Error(BoundaryFault(resource=("iceberg-needs-catalog", dataset.ref.relative)))
        if table_format is TableFormat.DUCKLAKE and (dsn is None or identifier is None):
            return Error(BoundaryFault(resource=("ducklake-needs-dsn", dataset.ref.relative)))
        return Ok(cls(table_uri=str(dataset.ref.path), table_format=table_format, catalog=catalog, identifier=identifier, dsn=dsn))

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
                    post_commithook_properties=tuning.hook_properties(),
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
                metrics = DeltaTable(self.table_uri).update(updates=dict(updates.items()), predicate=predicate)
                return self._receipt("update", snapshot=_delta_metric(metrics, "num_updated_rows"))
            case TableFormat.DELTA, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                clauses = dict(updates.items())
                merger = (
                    DeltaTable(self.table_uri)
                    .merge(data, predicate=predicate)
                    .when_matched_update(updates=clauses)
                    .when_not_matched_insert(updates=clauses)
                )
                (merger.when_not_matched_by_source_delete() if delete_unmatched else merger).execute()
                return self._receipt("merge")
            case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(_adds, drops, renames, _constraints)) if drops or renames:
                return Error(BoundaryFault(boundary=(f"lake.{self.table_format}.evolve", "delta alter has no portable column drop or rename")))
            case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(adds, _drops, _renames, constraints)):
                alterer = DeltaTable(self.table_uri).alter
                if adds:
                    alterer.add_columns([Field(name, dtype) for name, dtype in adds])
                if constraints:
                    # the mined governance clause: named SQL invariants enforced at every commit.
                    alterer.add_constraint(dict(constraints.items()))
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
                return self._receipt(
                    "restore", removed=_delta_metric(metrics, "num_removed_file"), snapshot=_delta_metric(metrics, "num_restored_file")
                )
            case TableFormat.ICEBERG, LakeOp(tag="write", write=(_mode, partition_by, _evolve, _tuning)) if partition_by:
                # Iceberg partitioning is TABLE-SPEC metadata authored at create (`PartitionSpec` over
                # `PartitionField` + `BucketTransform`/`YearTransform` rows) — the stated capability
                # row; a per-write partition_by cannot re-author the spec, so the discard is typed.
                return Error(BoundaryFault(boundary=("lake.iceberg.write", "partition_by is table-spec-owned; author PartitionSpec at create")))
            case TableFormat.ICEBERG, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                # `_iceberg()` catalog-loads the table (it exists), so `error` fails typed and `ignore`
                # no-ops; `overwrite`/`append` commit through the transaction per the WriteMode contract.
                if mode == "error":
                    return Error(BoundaryFault(boundary=("lake.iceberg.write", "error mode forbids a write into an existing table")))
                if mode == "ignore":
                    return self._receipt("write")
                txn = self._iceberg().transaction()
                txn.overwrite(data) if mode == "overwrite" else txn.append(data)
                txn.commit_transaction()
                return self._receipt("write")
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(version, _columns, predicate)):
                # the PRIMARY read path is the core-loadable DuckDB `iceberg` extension over the shared
                # session rail — `iceberg_scan` reads the table files with no catalog round-trip; the
                # pyiceberg catalog stays the `<3.15` WRITE fallback, never the read hot path.
                if isinstance(version, int):
                    scan = self._iceberg().scan(row_filter=predicate or "true", snapshot_id=version)
                    return self._receipt("read", snapshot=scan.count())
                with DuckDbSession(extensions=(DuckDbExtension.ICEBERG,)).connect() as con:
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM iceberg_scan({_quote_literal(self.table_uri)}){where}").fetchone()[0]
                return self._receipt("read", snapshot=int(rows))
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
            case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(_adds, _drops, _renames, constraints)) if constraints:
                return Error(BoundaryFault(boundary=("lake.iceberg.evolve", "constraint governance is Delta alter.add_constraint only")))
            case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(adds, drops, renames, _constraints)):
                from pyiceberg.types import IcebergType  # noqa: PLC0415

                with self._iceberg().update_schema() as schema:
                    for name, dtype in adds:
                        schema.add_column(name, IcebergType.model_validate(dtype))
                    for column in drops:
                        schema.delete_column(column)
                    for old, new in renames:
                        schema.rename_column(old, new)
                return self._receipt("evolve")
            case TableFormat.ICEBERG, LakeOp(tag="restore", restore=(target,)):
                if not isinstance(target, int):
                    return Error(BoundaryFault(boundary=("lake.iceberg.restore", "iceberg rollback takes an int snapshot_id")))
                self._iceberg().manage_snapshots().rollback_to_snapshot(target).commit()
                return self._receipt("restore")
            case TableFormat.ICEBERG, LakeOp(tag="vacuum", vacuum=(retention_hours, _dry_run)):
                self._iceberg().maintenance.expire_snapshots().older_than(_retention(retention_hours)).commit()
                return self._receipt("vacuum")
            case TableFormat.LANCE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, tuning)):
                # Lance has no native ignore mode: `ignore` no-ops on an existing dataset and otherwise
                # creates, so the existence probe short-circuits to the current snapshot before the write.
                if mode == "ignore" and _lance_exists(self.table_uri):
                    return self._receipt("write")
                lance.write_dataset(
                    data,
                    self.table_uri,
                    mode=_LANCE_MODE[mode],
                    max_rows_per_file=tuning.target_file_size or 1024 * 1024,
                    data_storage_version=tuning.data_storage_version,
                )
                return self._receipt("write")
            case TableFormat.LANCE, LakeOp(tag="read", read=(version, _columns, predicate)):
                # version-travel is the full pylance axis: an `int` snapshot or `str` tag rides
                # `version=`, a `datetime` resolves through `asof=` to the latest version before it.
                ds = lance.dataset(self.table_uri, asof=version) if isinstance(version, datetime) else lance.dataset(self.table_uri, version=version)
                return self._receipt("read", snapshot=ds.count_rows(filter=predicate))
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
            case TableFormat.LANCE, LakeOp(tag="restore", restore=(target,)):
                # `restore()` re-commits a prior snapshot as the new head — the Lance revert
                # mirroring the Delta revert commit; an `int` pins `version=`, a `datetime` resolves
                # through `asof=` to the latest version at or before it.
                ds = lance.dataset(self.table_uri, version=target) if isinstance(target, int) else lance.dataset(self.table_uri, asof=target)
                ds.restore()
                return self._receipt("restore")
            case TableFormat.DUCKLAKE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                with self._ducklake() as con:
                    con.register("payload", data)
                    table = _quote_ident(self.identifier)
                    statement = (
                        f"CREATE OR REPLACE TABLE {table} AS SELECT * FROM payload"
                        if mode == "overwrite"
                        else f"INSERT INTO {table} SELECT * FROM payload"
                    )
                    con.execute(statement)
                    return self._receipt("write", con=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="read", read=(version, _columns, predicate)):
                with self._ducklake() as con:
                    at = " AT (VERSION => ?)" if isinstance(version, int) else ""
                    where = f" WHERE {predicate}" if predicate else ""
                    table = _quote_ident(self.identifier)
                    rows = con.execute(f"SELECT count(*) FROM {table}{at}{where}", [version] if isinstance(version, int) else []).fetchone()[0]
                    return self._receipt("read", snapshot=int(rows), con=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="delete", delete=(predicate,)):
                with self._ducklake() as con:
                    con.execute(f"DELETE FROM {_quote_ident(self.identifier)} WHERE {predicate}")
                    return self._receipt("delete", con=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="update", update=(predicate, updates)):
                with self._ducklake() as con:
                    assignments = ", ".join(f"{_quote_ident(column)} = {expr}" for column, expr in updates.items())
                    con.execute(f"UPDATE {_quote_ident(self.identifier)} SET {assignments} WHERE {predicate}")
                    return self._receipt("update", con=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                # DuckDB owns native MERGE INTO; the update columns drive both clauses and the
                # `delete_unmatched` discriminant appends the not-matched-by-source delete clause.
                with self._ducklake() as con:
                    con.register("payload", data)
                    sets = ", ".join(f"{_quote_ident(column)} = payload.{_quote_ident(column)}" for column in updates.keys())
                    tail = " WHEN NOT MATCHED BY SOURCE THEN DELETE" if delete_unmatched else ""
                    con.execute(
                        f"MERGE INTO {_quote_ident(self.identifier)} USING payload ON {predicate}"
                        f" WHEN MATCHED THEN UPDATE SET {sets} WHEN NOT MATCHED THEN INSERT BY NAME{tail}"
                    )
                    return self._receipt("merge", con=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="changefeed", changefeed=(start, end)):
                with self._ducklake() as con:
                    rows = con.execute("SELECT count(*) FROM table_changes(?, ?, ?)", [self.identifier, start, end]).fetchone()[0]
                    return self._receipt("changefeed", snapshot=int(rows), con=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="optimize"):
                with self._ducklake() as con:
                    con.execute("CALL ducklake_merge_adjacent_files('lake')")
                    return self._receipt("optimize", con=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                with self._ducklake() as con:
                    interval = retention_hours or _DEFAULT_RETENTION_HOURS
                    con.execute(f"CALL ducklake_expire_snapshots('lake', older_than => now() - INTERVAL '{interval} hours', dry_run => {dry_run})")
                    if not dry_run:
                        con.execute("CALL ducklake_cleanup_old_files('lake', cleanup_all => true)")
                    return self._receipt("vacuum", con=con)
            case _, _:
                return self._reject(op)

    @contextmanager
    def _ducklake(self) -> "Iterator[duckdb.DuckDBPyConnection]":
        # one shared-session bracket per op: the `DUCKLAKE` extension row loads through the
        # `tabular/columnar#SCAN` rail, the caller-resolved DSN attaches, the catalog becomes current.
        with DuckDbSession(extensions=(DuckDbExtension.DUCKLAKE,)).connect() as con:
            con.execute(f"ATTACH {_quote_literal(f'ducklake:{self.dsn}')} AS lake")
            con.execute("USE lake")
            yield con

    def _iceberg(self) -> "Table":
        from pyiceberg.catalog import load_catalog  # noqa: PLC0415

        return load_catalog(self.catalog).load_table(self.identifier)

    def _snapshot(self, con: "duckdb.DuckDBPyConnection | None" = None) -> tuple[int, int]:
        match self.table_format:
            case TableFormat.DELTA:
                table = DeltaTable(self.table_uri)
                return table.version(), table.get_add_actions().num_rows
            case TableFormat.ICEBERG:
                history = self._iceberg().inspect.snapshots()
                return (history.column("snapshot_id")[-1].as_py() if history.num_rows else 0), 0
            case TableFormat.LANCE:
                return lance.dataset(self.table_uri).version, 0
            case TableFormat.DUCKLAKE:
                # the snapshot read rides the SAME attached connection the arm holds — the catalog
                # `snapshots()` function is attachment-scoped, so no second ATTACH is opened.
                row = con.execute("SELECT max(snapshot_id) FROM snapshots()").fetchone()
                return int(row[0] or 0), 0
            case unreachable:
                assert_never(unreachable)

    def _receipt(
        self, operation: str, *, snapshot: int = 0, removed: int = 0, con: "duckdb.DuckDBPyConnection | None" = None
    ) -> "RuntimeRail[LakeReceipt]":
        version, added = self._snapshot(con)
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

# the Delta WriteMode vocabulary projected onto the Lance create|overwrite|append band: `error`
# lands on `create` (fail-if-exists); `ignore` also creates when absent but no-ops on an existing
# dataset through the write arm's `_lance_exists` guard, since Lance owns no native ignore mode.
_LANCE_MODE: Final[Map[WriteMode, LanceMode]] = Map.of_seq([
    ("error", "create"),
    ("ignore", "create"),
    ("overwrite", "overwrite"),
    ("append", "append"),
])

_PORTABLE: Final[Map[TableFormat, frozenset[str]]] = Map.of_seq([
    (TableFormat.DELTA, frozenset({"write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "restore"})),
    (TableFormat.ICEBERG, frozenset({"write", "read", "delete", "merge", "evolve", "vacuum", "restore"})),
    (TableFormat.LANCE, frozenset({"write", "read", "delete", "merge", "index", "optimize", "vacuum", "restore"})),
    (TableFormat.DUCKLAKE, frozenset({"write", "read", "delete", "update", "merge", "changefeed", "optimize", "vacuum"})),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _quote_ident(name: str | None) -> str:
    # each dotted part routes through sqlglot's dialect-correct identifier quoting, staying qualified
    # so a caller table/column name can never break out of its identifier position into injectable SQL.
    return ".".join(exp.Identifier(this=part, quoted=True).sql(dialect="duckdb") for part in (name or "").split("."))


def _quote_literal(value: str) -> str:
    # single-quoted SQL string literal via sqlglot — the URI/DSN value positions.
    return exp.Literal.string(value).sql(dialect="duckdb")


def _lance_exists(uri: str) -> bool:
    # existence probe backing the `ignore` no-op — `lance.dataset` raises `ValueError` when absent.
    try:
        lance.dataset(uri)
    except ValueError:
        return False
    return True


def _delta_metric(metrics: dict[str, object], key: str) -> int:
    return int(metrics.get(key, 0))


def _retention(retention_hours: int | None) -> datetime:
    return datetime.now(UTC) - timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)


def _age(retention_hours: int | None) -> timedelta:
    return timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)
```
