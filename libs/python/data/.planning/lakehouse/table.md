# [PY_DATA_TABLE]

The transactional table-format interchange owner: one `LakeOp` axis over one `Lakehouse` owner, the table-format binding admitting Delta now and a sibling table-format row when Iceberg or Lance is admitted. `Lakehouse` folds the write/read/time-travel/optimize/vacuum/changefeed/schema-evolution/merge lifecycle through one `LakeOp` tagged union; `LakeReceipt` is the typed commit receipt — version, operation, files-added/removed, content-key. Every commit is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[LAKEHOUSE]`: the transactional table-format lakehouse owner over one `LakeOp` axis.

## [2]-[LAKEHOUSE]

- Owner: `Lakehouse` — the one transactional-table owner over `deltalake.DeltaTable`/`write_deltalake`; `LakeOp` the tagged-union operation axis (write/append/overwrite/read/time-travel/optimize/vacuum/changefeed/schema-evolution/merge), matched by `match`/`case` so a new table operation is one `LakeOp` case, never a `read_delta`/`write_delta`/`compact_delta` method family. `LakeReceipt` is the typed commit receipt — version, operation, files-added/removed, content-key.
- Cases: `LakeOp` rows `Write(mode, partition_by, evolve_schema)` (`write_deltalake` with `mode` ∈ `error|append|overwrite|ignore`, `schema_mode="merge"` carrying additive schema evolution) · `Read(version, columns, predicate)` (`load_as_version` + `to_pyarrow_table`, zero-copy Arrow) · `Optimize(target_size, zorder)` (`DeltaTable.optimize.compact`/`z_order`) · `Vacuum(retention_hours, dry_run)` (`DeltaTable.vacuum`) · `ChangeFeed(start, end)` (`load_cdf` Change Data Feed into a `RecordBatchReader`) · `Merge(predicate, updates)` (`DeltaTable.merge` upsert), each binding the exact `deltalake` surface that owns it.
- Entry: `Lakehouse.open` admits a `DatasetRef` of `DatasetKind.DELTA` and returns the frozen owner over the resolved `table_uri`; `Lakehouse.run` folds one `LakeOp` through `match`/`case` closed by `assert_never` and returns a `RuntimeRail[LakeReceipt]`; time-travel is `LakeOp.Read(version=...)`, never a parallel `read_at_version` entrypoint.
- Auto: `optimize` is a property exposing `compact()`/`z_order(columns)`; the changefeed rides `load_cdf(starting_version, ending_version)` returning a `pyarrow.RecordBatchReader` with the `_change_type`/`_commit_version`/`_commit_timestamp` CDF columns; schema evolution rides `schema_mode="merge"` on append, with no DDL migration because this is portable interchange.
- Receipt: the commit contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces a `LakeReceipt` keyed by `ContentIdentity` over the snapshot version plus add-action file URIs.
- Packages: `deltalake` (`DeltaTable`/`write_deltalake`/`load_cdf`/`optimize`/`vacuum`/`merge`), `pyarrow` (`Table`/`RecordBatchReader`), `duckdb` (queries the Delta snapshot via `from_arrow`), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new lake operation is one `LakeOp` case; a new write mode is a `Literal` row on `Write`; a second table format (Iceberg via `pyiceberg`, Lance) is a `TableFormat`-axis row on this same owner dispatching `_apply` to the format provider, never a parallel Iceberg/Hudi owner.
- Boundary: no durable store, no schema migration, no global Delta connection; a `read_delta`/`write_delta`/`optimize_delta` family and a per-operation class family are the deleted forms.

```python signature
from datetime import datetime
from typing import Literal, assert_never

import pyarrow as pa
from deltalake import CommitProperties, DeltaTable, write_deltalake
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.data.columnar.dataset import DatasetKind, DatasetRef
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

type WriteMode = Literal["error", "append", "overwrite", "ignore"]


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
            {"op": self.operation, "version": str(self.version), "added": str(self.files_added)},
        )


class Lakehouse(Struct, frozen=True):
    table_uri: str

    @classmethod
    def open(cls, dataset: DatasetRef) -> "RuntimeRail[Lakehouse]":
        if dataset.kind is not DatasetKind.DELTA:
            return Error(BoundaryFault(resource=("not-delta", dataset.ref.relative)))
        return Ok(cls(table_uri=str(dataset.ref.path)))

    def run(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        return boundary(f"lake.{op.tag}", lambda: self._apply(op, data))

    def _apply(self, op: LakeOp, data: pa.Table | None) -> LakeReceipt:
        match op:
            case LakeOp(tag="write", write=(mode, partition_by, evolve)):
                write_deltalake(
                    self.table_uri,
                    data,
                    mode=mode,
                    partition_by=list(partition_by) or None,
                    schema_mode="merge" if evolve else None,
                    commit_properties=CommitProperties(),
                )
                return self._receipt("write")
            case LakeOp(tag="read", read=(version, columns, predicate)):
                table = DeltaTable(self.table_uri)
                if version is not None:
                    table.load_as_version(version)
                arrow = table.to_pyarrow_table(columns=list(columns) or None, filters=None)
                return self._receipt("read", snapshot=arrow.num_rows)
            case LakeOp(tag="optimize", optimize=(target_size, zorder)):
                table = DeltaTable(self.table_uri)
                _ = (
                    table.optimize.z_order(list(zorder), target_size=target_size)
                    if zorder
                    else table.optimize.compact(target_size=target_size)
                )
                return self._receipt("optimize")
            case LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                removed = DeltaTable(self.table_uri).vacuum(retention_hours=retention_hours, dry_run=dry_run)
                return self._receipt("vacuum", removed=len(removed))
            case LakeOp(tag="changefeed", changefeed=(start, end)):
                reader = DeltaTable(self.table_uri).load_cdf(starting_version=start, ending_version=end)
                return self._receipt("changefeed", snapshot=reader.read_all().num_rows)
            case LakeOp(tag="merge", merge=(predicate, updates)):
                merger = DeltaTable(self.table_uri).merge(data, predicate=predicate)
                merger.when_matched_update(updates=updates).when_not_matched_insert_all().execute()
                return self._receipt("merge")
            case unreachable:
                assert_never(unreachable)

    def _receipt(self, operation: str, *, snapshot: int = 0, removed: int = 0) -> LakeReceipt:
        table = DeltaTable(self.table_uri)
        actions = table.get_add_actions().to_pydict()
        key = ContentIdentity.of("delta", f"{self.table_uri}@{table.version()}".encode())
        return LakeReceipt(
            table_uri=self.table_uri,
            operation=operation,
            version=table.version(),
            files_added=len(actions.get("path", [])),
            files_removed=removed,
            content_key=key,
        )
```
