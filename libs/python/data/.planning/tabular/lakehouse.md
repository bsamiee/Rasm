# [PY_DATA_LAKEHOUSE]

The transactional table-format interchange owner: one `LakeOp` operation axis crossed with one `TableFormat` provider axis on one `Lakehouse` owner over Delta, Iceberg, Lance, and DuckLake. `Lakehouse.run` folds the write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore lifecycle through the `LakeOp` tagged union and dispatches one `(TableFormat, tag)` arm to a `RuntimeRail[LakeReceipt]` — the operation axis format-agnostic, the format binding a separate discriminant, so a new format is one `TableFormat` row plus its arms, never a parallel Iceberg or Lance owner. The owner commits and reads snapshots over the provider surface; it holds no durable store.

Iceberg's read path is the core-loadable DuckDB `iceberg` extension with `pyiceberg` the catalog-write fallback; Lance carries the multimodal-asset versioning and `create_index` ANN rail; DuckLake rides one `ATTACH 'ducklake:<dsn>'` over the shared `tabular/columnar#SCAN` `DuckDbSession`, the single session every DuckDB-backed arm reuses. `changefeed` is the Delta `load_cdf` and DuckLake `table_changes` feed the `tabular/materialize#MATERIALIZE` `DerivedSnapshot._materialize` consumer reads. Every commit contributes through runtime `ReceiptContributor`, keys by `ContentIdentity`, and — when mutating — rides the `reliability/resilience#RESILIENCE` `RetryClass.LAKE_COMMIT` `guarded_sync` envelope; `open`/`run` admit through `@beartype(conf=FAULT_CONF)`, the shared config the sibling `interop`/`egress`/`columnar` seams bind. Table-protocol governance — deletion vectors, `TableFeatures` — is DECLINED here: the C# `Rasm.Persistence` at-rest owner holds it, never a data-side commit toggle.

## [01]-[INDEX]

- [01]-[LAKEHOUSE]: the transactional table-format lakehouse owner over one `LakeOp` operation axis crossed with one `TableFormat` provider axis.

## [02]-[LAKEHOUSE]

- Owner: `Lakehouse` over the `LakeOp` operation axis (a `tagged_union` matched by `match (self.table_format, op)`) and the `TableFormat` `StrEnum` provider axis, dispatched one `(format, tag)` arm — two orthogonal discriminants, so a new operation is one `LakeOp` case and a new format one `TableFormat` row, never a `read_delta`/`write_delta`/`delete_delta` method family and never a parallel `IcebergLakehouse`/`LanceLakehouse` pair. Writer tuning rides one `WriteTuning` policy `Struct` carried on `Write`, never a parallel `WriteTuned` op or a knob tail; the merge delete-on-no-match rides one `delete_unmatched` discriminant selecting the third `when_not_matched_by_source_delete` clause, never a `MergeDelete` op.
- Entry: `Lakehouse.open` admits a `DatasetRef` plus an explicit `TableFormat` (default `DELTA`) and cross-validates it against `dataset.kind` rather than recovering it — `DatasetKind` carries no `LANCE` member, so the format cannot be folded from the kind and the explicit axis is the one carrier admitting every provider; a `DELTA` over a non-`DELTA` dataset, an `ICEBERG` missing `catalog`/`identifier`, and a `DUCKLAKE` missing `dsn`/`identifier` each return `Error(BoundaryFault(resource=...))` directly. `run` folds one `LakeOp` through the `(format, op)` cross-product with a `case _, _` total reject, each portable arm returning a `RuntimeRail[LakeReceipt]` so a provider-thrown exception crosses `boundary` once; time-travel is `Read(version=...)` and a standalone delete is `Delete(...)`, never a parallel `read_at_version` or `delete_iceberg` entrypoint.
- Receipt: the snapshot identity is one polymorphic `_snapshot` method discriminating `match self.table_format`, folded by one `_receipt` projector — never three sibling `_<format>_snapshot` factories nor a parallel `_SNAPSHOT` dispatch dict. `LakeReceipt` keys by `ContentIdentity.of("lake", f"{table_uri}@{version}")`, which returns a rail the projector threads through `.map` so a digest fault propagates rather than a `Result` landing in the `content_key` slot; the `(table_uri, version)` payload pins the committed snapshot stable across a re-open of an unchanged version. `contribute` emits `Receipt.of("lakehouse", ("emitted", subject, facts))` whose `version`/`added` ride as native `int` the `enc_hook=repr` renderer serializes without a pre-coerce.
- Packages: `deltalake` owns the Delta arms — its `PostCommitHookProperties` and `TableAlterer.add_constraint` are MINED as `WriteTuning` hook fields and the `Evolve.constraints` clause, while `TableFeatures`/deletion-vector protocol enablement is DECLINED as the C# `Rasm.Persistence` at-rest concern; the predicate-bearing Delta read pushes SQL through the native `QueryBuilder` DataFusion surface, no SQL->pyarrow-DNF lowering owner minted. `pyiceberg` is the catalog-write fallback only (its `Table` annotation rides `TYPE_CHECKING`), gated behind the runtime lacking the core-loadable DuckDB `iceberg` read extension; `PartitionSpec`/`PartitionField` plus the `Bucket`/`Year`/`Month`/`Day` transforms are the table-spec authoring vocabulary the partition capability names. `pylance` owns the Lance dataset/version-travel/index arms and `pyarrow` the write carrier. `tabular/columnar#SCAN` `DuckDbSession`/`DuckDbExtension` is the ONE session rail every DuckLake `ATTACH 'ducklake:'` and Iceberg `iceberg_scan` arm reuses; the `ducklake` and `iceberg_scan` SQL surfaces are the `data/.api/ducklake.md` and `data/.api/duckdb.md` catalogs. runtime supplies `RuntimeRail`/`BoundaryFault`/`boundary`/`ContentIdentity`/`ReceiptContributor`/`Receipt` plus the `FAULT_CONF`, `RetryClass.LAKE_COMMIT`, and `guarded_sync` the admission and commit rails bind.
- Growth: a new lake operation is one `LakeOp` case absorbed by the `(format, tag)` dispatch; a new write mode a `Literal` row on `Write`; a new writer-tuning knob a `WriteTuning` field; a new Lance vector index kind a `VectorIndex` `Literal` row (a scalar/FTS kind a `ScalarIndex` row), both absorbed by the one `_VECTOR_INDEX`-routed `Index` arm; a new DuckDB-backed capability one `DuckDbExtension` row plus its `(DUCKLAKE|ICEBERG, *)` SQL arm; a fifth table format (Hudi, Paimon) one `TableFormat` member plus its arms on this same owner. DEFERRED: the version-reference authoring pair — Lance `tags.create`/`create_branch` and Iceberg `ManageSnapshots.create_branch`/`create_tag` — lands as ONE reference-authoring `LakeOp` case with per-format arms when a consumer names it; the read side is already landed (tag-string/`asof` time-travel on `Read`, `checkout_version`/`restore` on `Restore`).
- Boundary: no durable store, no schema migration, no global Delta or catalog connection; the metadata-only `Read` count is not the read lane — column-projected zero-copy reads route to the `tabular/columnar#SCAN` reader, not this commit owner. The reject-law is data: `_PORTABLE` names each format's reachable tags and the `case _, _` arm returns `Error(BoundaryFault(...))` carrying the typed key, never a `raise` into a `boundary` that re-keys and discards it, and never a hand-opened `stamina.retry_context` where `guarded_sync` owns the envelope. Each format rejects the ops its provider surface cannot portably reach as a typed fault, never a silent no-op: Iceberg `Update`/`Optimize`/`ChangeFeed`/`Index` (`rewrite_data_files` and `load_cdf` absent from `pyiceberg`), Lance `Update`/`Evolve`/`ChangeFeed` (its `Restore` is portable), DuckLake `Index`/`Restore`/`Evolve`.

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
    # Lance on-disk file format as a POLICY row, not the provider default.
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
    # DuckLake catalog DSN — caller-resolved through the runtime `TransportResource` seam, never minted here.
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
        # both branches self-flatten the nested `_apply` rail through `.bind(lambda rail: rail)`.
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
                # the predicate path pushes SQL through `QueryBuilder`; else the metadata-only `to_pyarrow_dataset().count_rows()`.
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
                    # mined governance clause — named SQL invariants enforced at every commit.
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
                # Iceberg partitioning is table-spec metadata authored at create; a per-write
                # `partition_by` is a typed reject, not a silent discard.
                return Error(BoundaryFault(boundary=("lake.iceberg.write", "partition_by is table-spec-owned; author PartitionSpec at create")))
            case TableFormat.ICEBERG, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                # `_iceberg()` loads an existing table, so `error` fails typed, `ignore` no-ops, `overwrite`/`append` commit.
                if mode == "error":
                    return Error(BoundaryFault(boundary=("lake.iceberg.write", "error mode forbids a write into an existing table")))
                if mode == "ignore":
                    return self._receipt("write")
                txn = self._iceberg().transaction()
                txn.overwrite(data) if mode == "overwrite" else txn.append(data)
                txn.commit_transaction()
                return self._receipt("write")
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(version, _columns, predicate)):
                # `iceberg_scan` reads with no catalog round-trip; the pyiceberg catalog is write-only.
                if isinstance(version, int):
                    scan = self._iceberg().scan(row_filter=predicate or "true", snapshot_id=version)
                    return self._receipt("read", snapshot=scan.count())
                with DuckDbSession(extensions=(DuckDbExtension.ICEBERG,)).connect() as con:
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM iceberg_scan({_quote_literal(self.table_uri)}){where}").fetchone()[0]
                return self._receipt("read", snapshot=int(rows))
            case TableFormat.ICEBERG, LakeOp(tag="delete", delete=(predicate,)):
                # `Transaction.delete`/`upsert` return `None`/`UpsertResult`, not the `Transaction`, so
                # `commit_transaction` is a separate statement off `txn`, never chained.
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
                # `ignore` short-circuits to the current snapshot when the dataset exists (`_lance_exists`).
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
                # an `int`/`str` tag rides `version=`; a `datetime` resolves through `asof=`.
                ds = lance.dataset(self.table_uri, asof=version) if isinstance(version, datetime) else lance.dataset(self.table_uri, version=version)
                return self._receipt("read", snapshot=ds.count_rows(filter=predicate))
            case TableFormat.LANCE, LakeOp(tag="delete", delete=(predicate,)):
                lance.dataset(self.table_uri).delete(predicate)
                return self._receipt("delete")
            case TableFormat.LANCE, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                # `merge_insert(on)` keys on column name(s), so the join key is the update columns, not the `predicate`.
                builder = lance.dataset(self.table_uri).merge_insert(list(updates.keys()))
                builder.when_matched_update_all().when_not_matched_insert_all().execute(data)
                return self._receipt("merge")
            case TableFormat.LANCE, LakeOp(tag="index", index=(column, kind, metric)):
                ds = lance.dataset(self.table_uri)
                # `metric` binds only the IVF vector families; scalar/FTS ride `create_scalar_index`, routed by `_VECTOR_INDEX`.
                ds.create_index(column, index_type=kind, metric=metric) if kind in _VECTOR_INDEX else ds.create_scalar_index(column, index_type=kind)
                return self._receipt("index")
            case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(target_size, _zorder)):
                metrics = lance.dataset(self.table_uri).optimize.compact_files(target_rows_per_fragment=target_size)
                return self._receipt("optimize", snapshot=metrics.fragments_added)
            case TableFormat.LANCE, LakeOp(tag="vacuum", vacuum=(retention_hours, _dry_run)):
                stats = lance.dataset(self.table_uri).cleanup_old_versions(older_than=_age(retention_hours))
                return self._receipt("vacuum", removed=stats.old_versions)
            case TableFormat.LANCE, LakeOp(tag="restore", restore=(target,)):
                # `restore()` re-heads a prior snapshot; `int` pins `version=`, `datetime` via `asof=`.
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
                # DuckDB owns native `MERGE INTO`; update columns drive both clauses, `delete_unmatched` appends the by-source delete.
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
        # one shared-session bracket per op — extension loads, DSN attaches, catalog current.
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
                # rides the SAME attached connection the arm holds — `snapshots()` is
                # attachment-scoped, so no second `ATTACH`.
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

# the tags `mutating` excludes from `LAKE_COMMIT` — `read`/`changefeed` only scan; every other op can conflict.
_READ_ONLY: Final[frozenset[str]] = frozenset({"read", "changefeed"})

# the Delta WriteMode projected onto Lance `create|overwrite|append`: `error`/`ignore`->`create`; `ignore`
# no-ops on an existing dataset via `_lance_exists`, Lance owning no native ignore mode.
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
    # each dotted part routes through sqlglot identifier quoting, so a caller name can't inject SQL.
    return ".".join(exp.Identifier(this=part, quoted=True).sql(dialect="duckdb") for part in (name or "").split("."))


def _quote_literal(value: str) -> str:
    # single-quoted SQL string literal via sqlglot for the URI/DSN positions.
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
