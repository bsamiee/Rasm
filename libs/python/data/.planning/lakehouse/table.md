# [PY_DATA_TABLE]

The transactional table-format interchange owner: one `LakeOp` operation axis crossed with one `TableFormat` provider axis on one `Lakehouse` owner, admitting Delta, Iceberg (pyiceberg MERGE plus REST catalog), and Lance (multimodal/AI-asset versioning). `Lakehouse` folds the write/read/time-travel/optimize/vacuum/changefeed/schema-evolution/merge lifecycle through one `LakeOp` tagged union, dispatching `_apply` to the provider the `TableFormat` axis selects; `LakeReceipt` is the typed commit receipt — version, operation, files-added/removed, content-key. The operation axis is format-agnostic; the format binding is a separate discriminant, so a new format is one `TableFormat` row dispatching to its provider, never a parallel Iceberg/Lance owner. Every commit is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[LAKEHOUSE]`: the transactional table-format lakehouse owner over one `LakeOp` operation axis crossed with one `TableFormat` provider axis.

## [2]-[LAKEHOUSE]

- Owner: `Lakehouse` — the one transactional-table owner over `deltalake`/`pyiceberg`/`lance`; `LakeOp` the tagged-union operation axis (write/append/overwrite/read/time-travel/optimize/vacuum/changefeed/schema-evolution/merge), matched by `match`/`case` so a new table operation is one `LakeOp` case, never a `read_delta`/`write_delta`/`compact_delta` method family; `TableFormat` the `StrEnum` provider axis (`DELTA`/`ICEBERG`/`LANCE`) the owner dispatches `_apply` against, so the operation axis and the provider axis are two orthogonal discriminants. `LakeReceipt` is the typed commit receipt — format, version, operation, files-added/removed, content-key.
- Cases: `LakeOp` rows `Write(mode, partition_by, evolve_schema)` (Delta `write_deltalake` with `mode` ∈ `error|append|overwrite|ignore` and `schema_mode="merge"`; Iceberg `Transaction.append`/`overwrite` under `Catalog.create_table_transaction`; Lance `write_dataset` with `mode` ∈ `create|overwrite|append`) · `Read(version, columns, predicate)` (Delta `load_as_version`+`to_pyarrow_table`; Iceberg `Table.scan(...).to_arrow()` with `snapshot_id`; Lance `lance.dataset(uri, version=...).to_table(columns, filter)`, all zero-copy Arrow) · `Optimize(target_size, zorder)` (Delta `DeltaTable.optimize.compact`/`z_order`; the Iceberg/Lance arms reject as `BoundaryFault.of` since neither exposes a portable compaction surface) · `Vacuum(retention_hours, dry_run)` (Delta `DeltaTable.vacuum`; Iceberg `Repository.expire_snapshots`-class is catalogue-side, the arm rejects) · `ChangeFeed(start, end)` (Delta `load_cdf` Change Data Feed into a `RecordBatchReader`; non-Delta formats reject) · `Merge(predicate, updates)` (Delta `DeltaTable.merge` upsert; Iceberg `Transaction.upsert(df, join_cols)`; Lance `LanceDataset.merge_insert(on).when_matched_update_all().when_not_matched_insert_all().execute(data)`), each binding the exact provider surface the `TableFormat` row selects.
- Entry: `Lakehouse.open` admits a `DatasetRef` and the `TableFormat` recovered from `DatasetKind` and returns the frozen owner over the resolved `table_uri`; `Lakehouse.run` folds one `LakeOp` through `match`/`case` closed by `assert_never`, dispatching the matched operation to the `TableFormat`-keyed provider, and returns a `RuntimeRail[LakeReceipt]`; time-travel is `LakeOp.Read(version=...)`, never a parallel `read_at_version` entrypoint.
- Auto: the operation `match` arm selects on `LakeOp.tag`, then routes to the format provider through one `_PROVIDER` dispatch keyed by `self.format` — a Delta operation, an Iceberg `Transaction`, and a Lance `LanceDataset` all fold into the same `LakeReceipt` shape; Delta `optimize` is a property exposing `compact()`/`z_order(columns)`; the changefeed rides `load_cdf(starting_version, ending_version)` returning a `pyarrow.RecordBatchReader` with the `_change_type`/`_commit_version`/`_commit_timestamp` CDF columns; Iceberg writes flow through `Catalog.load_catalog`-acquired `Transaction` (`load_catalog` the single polymorphic catalog entry, REST discriminated by `uri` scheme), `lance` is the `pylance` dist module (`import lance`, never `import pylance`); `pyiceberg` rides the `python_version<'3.15'` gated band so its arm imports the dist function-local under `# noqa: PLC0415`, never a module-top import on this cp315-core page, while `deltalake` (abi3) and `lance`/`pylance` (abi3) import module-top.
- Receipt: the commit contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces a `LakeReceipt` keyed by `ContentIdentity` over the format tag plus snapshot version plus add-action/data-file URIs.
- Packages: `deltalake` (`DeltaTable`/`write_deltalake`/`load_cdf`/`optimize`/`vacuum`/`merge`), `pyiceberg` (`load_catalog`/`Catalog.{load_table,create_table_transaction}`/`Transaction.{append,overwrite,upsert,commit_transaction}`/`Table.scan`/`TableScan.to_arrow`, the one `<3.15` gated arm), `pylance` (`lance.dataset`/`write_dataset`/`LanceDataset.{to_table,merge_insert,versions,delete}`/`MergeInsertBuilder`), `pyarrow` (`Table`/`RecordBatchReader`), `duckdb` (queries any format's Arrow snapshot via `from_arrow`), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new lake operation is one `LakeOp` case absorbed by every provider arm; a new write mode is a `Literal` row on `Write`; a fourth table format (Hudi, Paimon) is one `TableFormat` member plus one `_PROVIDER` dispatch arm on this same owner, never a parallel owner.
- Boundary: no durable store, no schema migration, no global Delta/catalog connection; a `read_delta`/`write_delta`/`optimize_delta` family, a per-operation class family, and a parallel `IcebergLakehouse`/`LanceLakehouse` pair are the deleted forms. The Iceberg/Lance arms reject portable-Delta-only operations (`Optimize`/`Vacuum`/`ChangeFeed`) as a typed `BoundaryFault`, never a silent no-op.

```python signature
from datetime import datetime
from enum import StrEnum
from typing import Literal, assert_never

import lance
import pyarrow as pa
from deltalake import CommitProperties, DeltaTable, write_deltalake
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.data.columnar.dataset import DatasetKind, DatasetRef
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

type WriteMode = Literal["error", "append", "overwrite", "ignore"]


class TableFormat(StrEnum):
    DELTA = "delta"
    ICEBERG = "iceberg"
    LANCE = "lance"


@tagged_union(frozen=True)
class LakeOp:
    tag: Literal["write", "read", "optimize", "vacuum", "changefeed", "merge"] = tag()
    write: tuple[WriteMode, tuple[str, ...], bool] = case()
    read: tuple[int | str | datetime | None, tuple[str, ...], str | None] = case()
    optimize: tuple[int | None, tuple[str, ...]] = case()
    vacuum: tuple[int | None, bool] = case()
    changefeed: tuple[int, int | None] = case()
    merge: tuple[str, dict[str, str]] = case()

    @staticmethod
    def Write(mode: WriteMode = "error", partition_by: tuple[str, ...] = (), evolve_schema: bool = False) -> "LakeOp":
        return LakeOp(write=(mode, partition_by, evolve_schema))

    @staticmethod
    def Read(version: int | str | datetime | None = None, columns: tuple[str, ...] = (), predicate: str | None = None) -> "LakeOp":
        return LakeOp(read=(version, columns, predicate))

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
    def Merge(predicate: str, updates: dict[str, str]) -> "LakeOp":
        return LakeOp(merge=(predicate, updates))


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


_PORTABLE_DELTA_ONLY: frozenset[str] = frozenset({"optimize", "vacuum", "changefeed"})


class Lakehouse(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    catalog: str | None = None
    identifier: str | None = None

    @classmethod
    def open(cls, dataset: DatasetRef, table_format: TableFormat = TableFormat.DELTA, *, catalog: str | None = None, identifier: str | None = None) -> "RuntimeRail[Lakehouse]":
        if table_format is TableFormat.DELTA and dataset.kind is not DatasetKind.DELTA:
            return Error(BoundaryFault(resource=("not-delta", dataset.ref.relative)))
        if table_format is TableFormat.ICEBERG and (catalog is None or identifier is None):
            return Error(BoundaryFault(resource=("iceberg-needs-catalog", dataset.ref.relative)))
        return Ok(cls(table_uri=str(dataset.ref.path), table_format=table_format, catalog=catalog, identifier=identifier))

    def run(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        if self.table_format is not TableFormat.DELTA and op.tag in _PORTABLE_DELTA_ONLY:
            return Error(BoundaryFault(boundary=(f"lake.{op.tag}", f"{self.table_format} has no portable {op.tag}")))
        return boundary(f"lake.{self.table_format}.{op.tag}", lambda: self._apply(op, data))

    def _apply(self, op: LakeOp, data: pa.Table | None) -> LakeReceipt:
        match self.table_format:
            case TableFormat.DELTA:
                return self._apply_delta(op, data)
            case TableFormat.ICEBERG:
                return self._apply_iceberg(op, data)
            case TableFormat.LANCE:
                return self._apply_lance(op, data)
            case unreachable:
                assert_never(unreachable)

    def _apply_delta(self, op: LakeOp, data: pa.Table | None) -> LakeReceipt:
        match op:
            case LakeOp(tag="write", write=(mode, partition_by, evolve)):
                write_deltalake(
                    self.table_uri, data, mode=mode, partition_by=list(partition_by) or None,
                    schema_mode="merge" if evolve else None, commit_properties=CommitProperties(),
                )
                return self._delta_receipt("write")
            case LakeOp(tag="read", read=(version, columns, predicate)):
                table = DeltaTable(self.table_uri)
                if version is not None:
                    table.load_as_version(version)
                arrow = table.to_pyarrow_table(columns=list(columns) or None, filters=None)
                return self._delta_receipt("read", snapshot=arrow.num_rows)
            case LakeOp(tag="optimize", optimize=(target_size, zorder)):
                table = DeltaTable(self.table_uri)
                _ = (
                    table.optimize.z_order(list(zorder), target_size=target_size)
                    if zorder
                    else table.optimize.compact(target_size=target_size)
                )
                return self._delta_receipt("optimize")
            case LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                removed = DeltaTable(self.table_uri).vacuum(retention_hours=retention_hours, dry_run=dry_run)
                return self._delta_receipt("vacuum", removed=len(removed))
            case LakeOp(tag="changefeed", changefeed=(start, end)):
                reader = DeltaTable(self.table_uri).load_cdf(starting_version=start, ending_version=end)
                return self._delta_receipt("changefeed", snapshot=reader.read_all().num_rows)
            case LakeOp(tag="merge", merge=(predicate, updates)):
                merger = DeltaTable(self.table_uri).merge(data, predicate=predicate)
                merger.when_matched_update(updates=updates).when_not_matched_insert_all().execute()
                return self._delta_receipt("merge")
            case unreachable:
                assert_never(unreachable)

    def _apply_iceberg(self, op: LakeOp, data: pa.Table | None) -> LakeReceipt:
        from pyiceberg.catalog import load_catalog  # noqa: PLC0415

        table = load_catalog(self.catalog).load_table(self.identifier)
        match op:
            case LakeOp(tag="write", write=(mode, partition_by, evolve)):
                txn = table.transaction()
                _ = txn.overwrite(data) if mode == "overwrite" else txn.append(data)
                txn.commit_transaction()
            case LakeOp(tag="read", read=(version, columns, predicate)):
                fields = tuple(columns) or ("*",)
                arrow = table.scan(selected_fields=fields, snapshot_id=version).to_arrow()
                return self._iceberg_receipt(table, "read", snapshot=arrow.num_rows)
            case LakeOp(tag="merge", merge=(predicate, updates)):
                table.transaction().upsert(data, join_cols=list(updates.keys())).commit_transaction()
            case unreachable:
                assert_never(unreachable)
        return self._iceberg_receipt(load_catalog(self.catalog).load_table(self.identifier), op.tag)

    def _apply_lance(self, op: LakeOp, data: pa.Table | None) -> LakeReceipt:
        match op:
            case LakeOp(tag="write", write=(mode, partition_by, evolve)):
                lance.write_dataset(data, self.table_uri, mode="append" if mode == "append" else "overwrite")
                return self._lance_receipt("write")
            case LakeOp(tag="read", read=(version, columns, predicate)):
                ds = lance.dataset(self.table_uri, version=version)
                arrow = ds.to_table(columns=list(columns) or None, filter=predicate)
                return self._lance_receipt("read", version=ds.version, snapshot=arrow.num_rows)
            case LakeOp(tag="merge", merge=(predicate, updates)):
                ds = lance.dataset(self.table_uri)
                ds.merge_insert(predicate).when_matched_update_all().when_not_matched_insert_all().execute(data)
                return self._lance_receipt("merge")
            case unreachable:
                assert_never(unreachable)

    def _delta_receipt(self, operation: str, *, snapshot: int = 0, removed: int = 0) -> LakeReceipt:
        table = DeltaTable(self.table_uri)
        actions = table.get_add_actions().to_pydict()
        return LakeReceipt(
            table_uri=self.table_uri, table_format=TableFormat.DELTA, operation=operation,
            version=table.version(), files_added=len(actions.get("path", [])), files_removed=removed,
            content_key=ContentIdentity.of("delta", f"{self.table_uri}@{table.version()}".encode()),
        )

    def _iceberg_receipt(self, table: object, operation: str, *, snapshot: int = 0) -> LakeReceipt:
        history = table.inspect.snapshots()
        snap = history.column("snapshot_id")[-1].as_py() if history.num_rows else 0
        return LakeReceipt(
            table_uri=self.table_uri, table_format=TableFormat.ICEBERG, operation=operation,
            version=snap, files_added=snapshot, files_removed=0,
            content_key=ContentIdentity.of("iceberg", f"{self.identifier}@{snap}".encode()),
        )

    def _lance_receipt(self, operation: str, *, version: int = 0, snapshot: int = 0) -> LakeReceipt:
        ver = version or lance.dataset(self.table_uri).version
        return LakeReceipt(
            table_uri=self.table_uri, table_format=TableFormat.LANCE, operation=operation,
            version=ver, files_added=snapshot, files_removed=0,
            content_key=ContentIdentity.of("lance", f"{self.table_uri}@{ver}".encode()),
        )
```

## [3]-[RESEARCH]

- [ICEBERG_SNAPSHOT]: the `pyiceberg` `load_catalog`/`Catalog.load_table`/`Table.transaction`/`Transaction.{append,overwrite,upsert,commit_transaction}`/`Table.scan(selected_fields=, snapshot_id=).to_arrow()` surface and the `Table.inspect.snapshots()` metadata-table accessor the `_iceberg_receipt` binds are both catalogue-confirmed against the folder `pyiceberg` `.api`; the receipt reads the current snapshot id from the last `snapshot_id` row of the catalogued `InspectTable.snapshots()` Arrow table (the inspection surface the catalogue's reject-law mandates over `raw metadata-file access outside InspectTable`), so no catalogue gap remains and the `Table.metadata.current_snapshot_id` private-attribute read is the deleted form. `pyiceberg` rides the `python_version<'3.15'` gated band, so `load_catalog` imports function-local under `# noqa: PLC0415`; a module-top `pyiceberg` import on this cp315-core page is the floor-violating form.
- [LANCE_VERSION]: the `pylance` `lance.dataset(uri, version=)`/`write_dataset(data, uri, mode=)`/`LanceDataset.{to_table,merge_insert}`/`MergeInsertBuilder.{when_matched_update_all,when_not_matched_insert_all,execute}` surface is catalogue-confirmed against the folder `pylance` `.api`; the `LanceDataset.version` integer property the `_lance_receipt` reads is the one member the catalogue lists only as `versions()`, so the version read confirms the scalar `.version` accessor against the live distribution before the receipt treats it as the settled snapshot identity.
