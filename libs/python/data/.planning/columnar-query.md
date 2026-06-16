# [PY_DATA_COLUMNAR_QUERY]

Typed dataset refs, columnar lazy/streaming scans across engines, columnar egress, the Delta lakehouse, the relational query engine, and the data-quality gate. `DatasetRef` is the one polymorphic dataset owner discriminating by source shape; `ScanPlan` is the engine/projection/predicate/partition policy with cases for the Polars LazyFrame plan, the DuckDB relational API, and the PyArrow dataset scanner; `ColumnarEgress` is the typed Arrow/Parquet/IPC export folding one `QueryReceipt` over scan + transform + egress; `Lakehouse` folds the Delta write/read/time-travel/optimize/vacuum/changefeed lifecycle through one `LakeOp` axis; `QueryEngine` discriminates one `QuerySpec` tagged-union axis (DuckDB SQL/relational, narwhals dataframe-agnostic) onto a uniform Arrow result; `DataQuality` folds IDS-style `QualityRule` rows into one pandera schema and records a non-enforcing `SchemaClaim`. Every receipt is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                |
| :-----: | :-------- | :---------------------------------------------------- |
|   [1]   | DATASET   | the dataset ref owner discriminating by source shape  |
|   [2]   | SCAN      | engine scan plans, columnar egress, the query receipt |
|   [3]   | LAKEHOUSE | the Delta lakehouse owner: write/read/time-travel/optimize/vacuum/CDF |
|   [4]   | QUERY     | the relational query engine over one `QuerySpec` axis (duckdb sql/rel, narwhals) |
|   [5]   | QUALITY   | the data-quality validation owner over pandera + the schema claim receipt |

## [2]-[DATASET]

- Owner: `DatasetRef` — the one polymorphic dataset owner; `DatasetKind` the closed `StrEnum` discriminating CSV/Parquet/Arrow-IPC/Arrow-dataset/Delta/Pandas-file/Polars-file/.3dm/mesh/HDF source shapes.
- Cases: `DatasetKind` rows `CSV` · `PARQUET` · `ARROW_IPC` · `ARROW_DATASET` · `DELTA` · `PANDAS_FILE` · `POLARS_FILE` · `RHINO_3DM` · `MESH` · `HDF` — matched by `match`/`case`, no Get/List/Scan families.
- Entry: `DatasetRef.of` admits a `ResourceRef` and a `DatasetKind` and returns the frozen owner; the kind is recoverable from the source shape, never a knob.
- Packages: `polars`, `pyarrow`, `pandas`, `rhino3dm`, `meshio`, `trimesh`, `h5py`, runtime (`ResourceRef`/`ContentIdentity`).
- Growth: a new source shape is one `DatasetKind` row; zero new surface.
- Boundary: no product identity, repository, or host document mutation; a `get_csv`/`read_parquet`/`load_delta` family is the deleted form; HDF array persistence is a `DatasetKind` row (file exchange), not a compute concern.

```python signature
from enum import StrEnum

from msgspec import Struct

from rasm.runtime.resources_lanes import ResourceRef


class DatasetKind(StrEnum):
    CSV = "csv"
    PARQUET = "parquet"
    ARROW_IPC = "arrow-ipc"
    ARROW_DATASET = "arrow-dataset"
    DELTA = "delta"
    PANDAS_FILE = "pandas-file"
    POLARS_FILE = "polars-file"
    RHINO_3DM = "rhino-3dm"
    MESH = "mesh"
    HDF = "hdf"


class DatasetRef(Struct, frozen=True):
    ref: ResourceRef
    kind: DatasetKind

    @classmethod
    def of(cls, ref: ResourceRef, kind: DatasetKind) -> "DatasetRef":
        return cls(ref=ref, kind=kind)
```

## [3]-[SCAN]

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `ColumnarEgress` the typed Arrow/Parquet/IPC/lazy-scan export; `QueryReceipt` the one typed fault/receipt fold over scan + transform + egress.
- Cases: `ScanPlan` cases `PolarsLazy(projection, predicate)` (Polars `LazyFrame`/`scan_*`/`collect`) · `DuckDb(sql, projection)` (DuckDB relational API) · `ArrowDataset(filter, columns)` (PyArrow `dataset.Scanner`) — matched by `match`/`case`, each binding the engine that owns it.
- Entry: `ScanPlan.execute` runs the plan and returns a `RuntimeRail[pyarrow.Table]` over the Arrow C Data Interface (zero-copy); `ColumnarEgress.write` emits Arrow/Parquet/IPC keyed by `ContentIdentity`; `QueryReceipt.of` folds the engine/source/columns/predicate-count/row-count/content-key.
- Auto: the Polars path runs `scan_parquet(...).select(projection).filter(predicate).collect(engine="streaming")`; the DuckDB path runs the relational API over `duckdb.connect`; the PyArrow path runs `dataset(source).scanner(columns, filter).to_table()`; remote sourcing rides ADBC/ConnectorX where the source is a connection.
- Receipt: the scan contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a `QueryReceipt` keyed by `ContentIdentity` over the egress bytes — never a generic `IReceipt`.
- Packages: `polars` (`scan_parquet`/`LazyFrame.collect`), `duckdb` (`connect`/relational API), `pyarrow` (`dataset`/`Scanner`/`Table`), `adbc-driver-manager`, `connectorx`, `deltalake`, runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new engine is one `ScanPlan` case; a new egress format is one `ColumnarEgress` branch; zero new surface.
- Boundary: no durable query rails, no global DuckDB connection; a generic receipt abstraction and a per-engine egress class family are the deleted forms.

```python signature
from typing import Literal

import duckdb
import polars as pl
import pyarrow as pa
import pyarrow.dataset as pads
import pyarrow.feather as paf
import pyarrow.parquet as papq
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary


@tagged_union(frozen=True)
class ScanPlan:
    tag: Literal["polars_lazy", "duckdb", "arrow_dataset"] = tag()
    polars_lazy: tuple[tuple[str, ...], str] = case()
    duckdb: tuple[str, tuple[str, ...]] = case()
    arrow_dataset: tuple[str, tuple[str, ...]] = case()

    @staticmethod
    def PolarsLazy(projection: tuple[str, ...], predicate: str) -> "ScanPlan":
        return ScanPlan(polars_lazy=(projection, predicate))

    @staticmethod
    def DuckDb(sql: str, projection: tuple[str, ...]) -> "ScanPlan":
        return ScanPlan(duckdb=(sql, projection))

    @staticmethod
    def ArrowDataset(filter_expr: str, columns: tuple[str, ...]) -> "ScanPlan":
        return ScanPlan(arrow_dataset=(filter_expr, columns))


@tagged_union(frozen=True)
class ColumnarEgress:
    tag: Literal["arrow_ipc", "parquet", "feather"] = tag()
    arrow_ipc: str = case()                                      # target uri
    parquet: tuple[str, str] = case()                            # (target uri, compression)
    feather: str = case()                                        # target uri

    @staticmethod
    def ArrowIpc(target: str) -> "ColumnarEgress":
        return ColumnarEgress(arrow_ipc=target)

    @staticmethod
    def Parquet(target: str, compression: str = "zstd") -> "ColumnarEgress":
        return ColumnarEgress(parquet=(target, compression))

    @staticmethod
    def Feather(target: str) -> "ColumnarEgress":
        return ColumnarEgress(feather=target)

    def write(self, table: pa.Table) -> "RuntimeRail[QueryReceipt]":
        return boundary(f"egress.{self.tag}", lambda: self._emit(table))

    def _emit(self, table: pa.Table) -> "QueryReceipt":
        match self:
            case ColumnarEgress(tag="arrow_ipc", arrow_ipc=target):
                with pa.OSFile(target, "wb") as sink, pa.ipc.new_stream(sink, table.schema) as writer:
                    writer.write_table(table)
            case ColumnarEgress(tag="parquet", parquet=(target, compression)):
                papq.write_table(table, target, compression=compression)
            case ColumnarEgress(tag="feather", feather=target):
                paf.write_feather(table, target)
        return QueryReceipt.of(self.tag, target, table)


class QueryReceipt(Struct, frozen=True):
    engine: str
    source: str
    columns: int
    predicate_count: int
    row_count: int
    content_key: ContentKey

    @classmethod
    def of(
        cls,
        engine: str,
        source: str,
        table: pa.Table,
        *,
        predicate_count: int = 0,
    ) -> "QueryReceipt":
        return cls(
            engine=engine,
            source=source,
            columns=table.num_columns,
            predicate_count=predicate_count,
            row_count=table.num_rows,
            content_key=ContentIdentity.key("query", f"{engine}:{source}".encode()),
        )

    def contribute(self) -> Receipt:
        return Receipt.Emitted(self.engine, self.source, {"rows": str(self.row_count)})


def execute(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[pa.Table]":
    return boundary(f"scan.{plan.tag}", lambda: _run(plan, dataset))


def _run(plan: ScanPlan, dataset: DatasetRef) -> pa.Table:
    source = str(dataset.ref.path)
    match plan:
        case ScanPlan(tag="polars_lazy", polars_lazy=(projection, predicate)):
            lf = pl.scan_parquet(source)
            lf = lf.select(list(projection)) if projection else lf
            lf = lf.filter(pl.sql_expr(predicate)) if predicate else lf
            return lf.collect(engine="streaming").to_arrow()
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            rel = duckdb.connect().sql(sql)
            rel = rel.project(", ".join(projection)) if projection else rel
            return rel.to_arrow_table()
        case ScanPlan(tag="arrow_dataset", arrow_dataset=(filter_expr, columns)):
            ds = pads.dataset(source)
            scan_kwargs: dict[str, object] = {"columns": list(columns) or None}
            if filter_expr:
                scan_kwargs["filter"] = pads.field(filter_expr).is_valid()
            return ds.scanner(**scan_kwargs).to_table()
```

## [4]-[LAKEHOUSE]

- Owner: `Lakehouse` — the one Delta-lakehouse owner over `deltalake.DeltaTable`/`write_deltalake`; `LakeOp` the tagged-union operation axis (write/append/overwrite/read/time-travel/optimize/vacuum/changefeed/schema-evolution/merge), matched by `match`/`case` so a new table operation is one `LakeOp` case, never a `read_delta`/`write_delta`/`compact_delta` method family. `LakeReceipt` is the typed Delta-commit receipt — version, operation, files-added/removed, content-key — never a generic `IReceipt`.
- Cases: `LakeOp` rows `Write(mode, partition_by)` (`write_deltalake` with `mode` ∈ `error|append|overwrite|ignore`, `schema_mode="merge"` carrying additive schema evolution) · `Read(version, columns, predicate)` (`load_as_version` + `to_pyarrow_table`, zero-copy Arrow) · `Optimize(target_size, zorder)` (`DeltaTable.optimize.compact`/`z_order`) · `Vacuum(retention_hours, dry_run)` (`DeltaTable.vacuum`) · `ChangeFeed(start, end)` (`load_cdf` Change Data Feed → `RecordBatchReader`) · `Merge(predicate, updates)` (`DeltaTable.merge` upsert) — each binding the exact `deltalake` 1.6 surface that owns it.
- Entry: `Lakehouse.open` admits a `DatasetRef` of `DatasetKind.DELTA` and returns the frozen owner over the resolved `table_uri`; `Lakehouse.run` folds one `LakeOp` through `match`/`case` and returns a `RuntimeRail[LakeReceipt]`; time-travel is `LakeOp.Read(version=...)`, never a parallel `read_at_version` entrypoint.
- Auto: `optimize` is a property exposing `compact()`/`z_order(columns)`; the changefeed rides `load_cdf(starting_version, ending_version)` returning a `pyarrow.RecordBatchReader` with the `_change_type`/`_commit_version`/`_commit_timestamp` CDF columns; schema evolution rides `schema_mode="merge"` on append — no DDL migration, this is portable interchange.
- Receipt: the commit contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a `LakeReceipt` keyed by `ContentIdentity` over the snapshot version + add-action file URIs — never a generic receipt.
- Packages: `deltalake` (`DeltaTable`/`write_deltalake`/`load_cdf`/`optimize`/`vacuum`/`merge`, REFLECTED 1.6.0), `pyarrow` (`Table`/`RecordBatchReader`), `duckdb` (queries the Delta snapshot via `from_arrow`), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new lake operation is one `LakeOp` case; a new write mode is a `Literal` row on `Write`; zero new surface; no parallel Iceberg/Hudi owner — a second table format is a `LakeOp`-sibling table-format axis on this same owner when admitted.
- Boundary: no durable store, no schema migration, no global Delta connection; a `read_delta`/`write_delta`/`optimize_delta` family and a per-operation class family are the deleted forms.

```python signature
from datetime import datetime
from typing import Literal

import pyarrow as pa
from deltalake import CommitProperties, DeltaTable, write_deltalake
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import BoundaryFault, RuntimeRail, boundary

type WriteMode = Literal["error", "append", "overwrite", "ignore"]


@tagged_union(frozen=True)
class LakeOp:
    tag: Literal[
        "write", "read", "optimize", "vacuum", "changefeed", "merge"
    ] = tag()
    write: tuple[WriteMode, tuple[str, ...], bool] = case()       # (mode, partition_by, evolve_schema)
    read: tuple[int | str | datetime | None, tuple[str, ...], str | None] = case()  # (version, columns, predicate)
    optimize: tuple[int | None, tuple[str, ...]] = case()         # (target_size, zorder_columns)
    vacuum: tuple[int | None, bool] = case()                      # (retention_hours, dry_run)
    changefeed: tuple[int, int | None] = case()                   # (starting_version, ending_version)
    merge: tuple[str, dict[str, str]] = case()                    # (predicate, updates)

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
        return Receipt.Emitted(
            "lakehouse",
            self.table_uri,
            {"op": self.operation, "version": str(self.version), "added": str(self.files_added)},
        )


class Lakehouse(Struct, frozen=True):
    table_uri: str

    @classmethod
    def open(cls, dataset: DatasetRef) -> "RuntimeRail[Lakehouse]":
        if dataset.kind is not DatasetKind.DELTA:
            return Error(BoundaryFault.Resource("not-delta", dataset.ref.relative))
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
                metrics = (
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

    def _receipt(self, operation: str, *, snapshot: int = 0, removed: int = 0) -> LakeReceipt:
        table = DeltaTable(self.table_uri)
        actions = table.get_add_actions().to_pydict()
        key = ContentIdentity.key("delta", f"{self.table_uri}@{table.version()}".encode())
        return LakeReceipt(
            table_uri=self.table_uri,
            operation=operation,
            version=table.version(),
            files_added=len(actions.get("path", [])),
            files_removed=removed,
            content_key=key,
        )
```

## [5]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner over `duckdb`/`narwhals` discriminating by the `QuerySpec` tagged-union axis (the single discriminant; the frontend IS the spec shape, never a parallel `StrEnum` backend knob). `QuerySpec` cases `Sql` (relational SQL) · `Rel` (the chained relational API) · `Agnostic` (the dataframe-agnostic expression surface). A new query frontend is one `QuerySpec` case, never a `sql_query`/`rel_query`/`nw_query` method family.
- Cases: `QuerySpec.Sql(text)` runs `duckdb.connect().sql(text)` over registered Arrow/Delta inputs and binds the result with `to_arrow_table()` (zero-copy Arrow) · `Rel(filter, project, group_by)` chains `from_arrow(...).filter(...).project(...).aggregate(...)` returning a `DuckDBPyRelation`, terminal `to_arrow_table()` · `Agnostic(select, filter, group_by)` admits any native frame via `narwhals.from_native`, composes `select`/`filter`/`group_by`/`with_columns` against `narwhals.col`/`narwhals.Expr`, and lands native via `to_native` or `.to_arrow()` — one query authored once runs on polars, pandas, pyarrow, or duckdb relations underneath.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; `QueryEngine.run` folds the `QuerySpec` through `match`/`case` (total over the three cases, no fallthrough) and returns a `RuntimeRail[pa.Table]`; the DuckDB connection is request-scoped, never a module global.
- Auto: Arrow result binding is uniform — every case terminates in a `pyarrow.Table` via `DuckDBPyRelation.to_arrow_table` or `narwhals.DataFrame.to_arrow`, so the egress, content-key, and `QueryReceipt` fold are spec-agnostic; the narwhals path keeps the query lazy (`narwhals.LazyFrame`) until `collect()`, preserving polars/duckdb predicate-pushdown.
- Receipt: a run contributes a `QueryReceipt` (the existing SCAN receipt owner) keyed by `ContentIdentity` over the result bytes; no new receipt rail.
- Packages: `duckdb` (`connect`/`sql`/`from_arrow`/`DuckDBPyRelation.filter`/`project`/`aggregate`/`to_arrow_table`, REFLECTED 1.5.3), `narwhals` (`from_native`/`to_native`/`col`/`Expr`/`LazyFrame.collect`/`DataFrame.to_arrow`, REFLECTED 2.22.1), `pyarrow` (`Table`), runtime (`RuntimeRail`/`ContentIdentity`).
- Growth: a new query frontend is one `QuerySpec` case; a new relational verb composes on the existing chain; zero new surface.
- Boundary: no durable query rail, no global connection, no SQL-string templating engine, no parallel `StrEnum` backend discriminant duplicating the `QuerySpec` tag; a per-frontend query class family and a generic dataframe wrapper are the deleted forms.

```python signature
from typing import Any, Literal

import duckdb
import narwhals as nw
import pyarrow as pa
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.rails_resilience import RuntimeRail, boundary


@tagged_union(frozen=True)
class QuerySpec:
    tag: Literal["sql", "rel", "agnostic"] = tag()
    sql: str = case()                                            # raw SQL over registered inputs
    rel: tuple[str | None, tuple[str, ...], tuple[str, ...]] = case()  # (filter, project, group_by)
    agnostic: tuple[tuple[str, ...], str | None, tuple[str, ...]] = case()  # (select, filter_expr, group_by)

    @staticmethod
    def Sql(text: str) -> "QuerySpec":
        return QuerySpec(sql=text)

    @staticmethod
    def Rel(filter_expr: str | None, project: tuple[str, ...], group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(rel=(filter_expr, project, group_by))

    @staticmethod
    def Agnostic(select: tuple[str, ...], filter_expr: str | None = None, group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(agnostic=(select, filter_expr, group_by))


class QueryEngine(Struct, frozen=True):
    inputs: dict[str, Any]   # name -> Arrow table / native frame, registered request-scoped

    @classmethod
    def of(cls, inputs: dict[str, Any]) -> "QueryEngine":
        return cls(inputs=inputs)

    def run(self, spec: QuerySpec) -> "RuntimeRail[pa.Table]":
        return boundary(f"query.{spec.tag}", lambda: self._dispatch(spec))

    def _dispatch(self, spec: QuerySpec) -> pa.Table:
        match spec:
            case QuerySpec(tag="sql", sql=text):
                con = duckdb.connect()
                for name, frame in self.inputs.items():
                    con.register(name, frame)
                return con.sql(text).to_arrow_table()
            case QuerySpec(tag="rel", rel=(flt, project, group_by)):
                con = duckdb.connect()
                name, frame = next(iter(self.inputs.items()))
                rel = con.from_arrow(frame)
                rel = rel.filter(flt) if flt else rel
                rel = rel.aggregate(", ".join(project), ", ".join(group_by)) if group_by else rel.project(", ".join(project))
                return rel.to_arrow_table()
            case QuerySpec(tag="agnostic", agnostic=(select, flt, group_by)):
                name, frame = next(iter(self.inputs.items()))
                lf = nw.from_native(frame).lazy()
                lf = lf.filter(nw.col(flt)) if flt else lf
                if group_by:
                    lf = lf.group_by(*group_by).agg(*(nw.col(c) for c in select))
                else:
                    lf = lf.select(*select)
                return nw.to_native(lf.collect()).to_arrow()
```

## [6]-[QUALITY]

- Owner: `DataQuality` — the one data-quality validation owner over `pandera.polars`; `QualityRule` the row family modeling one column claim (dtype/nullable/unique/required + a closed `CheckKind` predicate set), folded into a `pandera.polars.DataFrameSchema`. A new validation is one `QualityRule` row, never a `validate_nullable`/`validate_range`/`validate_unique` method family. `SchemaClaim` is the receipt that RECORDS the contract and its failure cases without raising — it never enforces; enforcement is the caller's `match` on `SchemaClaim.status`.
- Cases: `CheckKind` rows `GE`/`LE`/`GT`/`LT`/`EQ` (`pandera.Check.ge`/`le`/`gt`/`lt`/`equal_to`) · `IN_RANGE(lo, hi)` (`Check.in_range`) · `ISIN(values)` (`Check.isin`) · `UNIQUE`/`MONOTONIC` (column flags / `Check.is_monotonic`) — matched by `match`/`case` into the concrete `pandera.Check`, so the IDS-style rule vocabulary collapses one switch, not a per-check builder.
- Entry: `DataQuality.of` folds a tuple of `QualityRule` into one `DataFrameSchema`; `DataQuality.validate` runs `schema.validate(frame, lazy=lazy)` and returns a `RuntimeRail[SchemaClaim]` — `lazy=True` collects ALL failure cases (the `SchemaErrors.failure_cases` frame), `lazy=False` short-circuits on the first `SchemaError`. Both land in one `SchemaClaim`; the rail is `Ok` even on validation failure because the claim records-but-does-not-enforce.
- Auto: a passing validation yields `SchemaClaim(status=PASSED, failure_count=0)`; a failing lazy validation captures `SchemaErrors.failure_cases` row count and the failing column/check pairs into `SchemaClaim.failures`; the polars backend keeps the frame lazy so validation pushes into the scan.
- Receipt: `SchemaClaim` contributes a `Receipt.emitted` row through `ReceiptContributor` keyed by `ContentIdentity` over the schema fingerprint — it is the data-contract evidence, never a generic receipt, and it never replaces the typed `QueryReceipt`.
- Packages: `pandera` (`pandera.polars.DataFrameSchema`/`Column`/`Check`/`errors.SchemaError`/`SchemaErrors`, REFLECTED 0.31.1), `polars` (`LazyFrame`), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new check is one `CheckKind` row; a new column claim is one `QualityRule`; zero new surface.
- Boundary: no raising in domain logic, no global schema registry, no coercion side effects (`coerce=False`); a per-check validator family and an exception-driven gate are the deleted forms.

```python signature
from enum import StrEnum
from typing import Any, Literal

import pandera.polars as pap
import polars as pl
from pandera import Check
from pandera.errors import SchemaError, SchemaErrors
from expression import Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary


@tagged_union(frozen=True)
class CheckKind:
    tag: Literal["cmp", "in_range", "isin", "unique", "monotonic"] = tag()
    cmp: tuple[Literal["ge", "le", "gt", "lt", "eq"], float] = case()
    in_range: tuple[float, float] = case()
    isin: tuple[Any, ...] = case()
    unique: bool = case()
    monotonic: bool = case()

    def to_check(self) -> Check | None:
        match self:
            case CheckKind(tag="cmp", cmp=("ge", v)):
                return Check.ge(v)
            case CheckKind(tag="cmp", cmp=("le", v)):
                return Check.le(v)
            case CheckKind(tag="cmp", cmp=("gt", v)):
                return Check.gt(v)
            case CheckKind(tag="cmp", cmp=("lt", v)):
                return Check.lt(v)
            case CheckKind(tag="cmp", cmp=("eq", v)):
                return Check.equal_to(v)
            case CheckKind(tag="in_range", in_range=(lo, hi)):
                return Check.in_range(lo, hi)
            case CheckKind(tag="isin", isin=values):
                return Check.isin(list(values))
            case CheckKind(tag="monotonic", monotonic=True):
                return Check.is_monotonic()
            case _:
                return None  # unique is a column flag, not a Check


class QualityRule(Struct, frozen=True):
    column: str
    dtype: Any                       # a polars dtype, e.g. pl.Int64
    checks: tuple[CheckKind, ...] = ()
    nullable: bool = False
    required: bool = True

    def to_column(self) -> pap.Column:
        return pap.Column(
            self.dtype,
            checks=[c.to_check() for c in self.checks if c.to_check() is not None],
            nullable=self.nullable,
            unique=any(c.tag == "unique" and c.unique for c in self.checks),
            required=self.required,
            coerce=False,
        )


class ClaimStatus(StrEnum):
    PASSED = "passed"
    FAILED = "failed"


class SchemaClaim(Struct, frozen=True):
    status: ClaimStatus
    columns: int
    failure_count: int
    failures: tuple[tuple[str, str], ...]   # (column, check) pairs, recorded not enforced
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.Emitted(
            "data-quality",
            f"schema[{self.columns}]",
            {"status": self.status, "failures": str(self.failure_count)},
        )


class DataQuality(Struct, frozen=True):
    rules: tuple[QualityRule, ...]

    @classmethod
    def of(cls, *rules: QualityRule) -> "DataQuality":
        return cls(rules=rules)

    def _schema(self) -> pap.DataFrameSchema:
        return pap.DataFrameSchema({r.column: r.to_column() for r in self.rules}, strict=False, coerce=False)

    def validate(self, frame: pl.LazyFrame, *, lazy: bool = True) -> "RuntimeRail[SchemaClaim]":
        return boundary("quality.validate", lambda: self._validate(frame, lazy))

    def _validate(self, frame: pl.LazyFrame, lazy: bool) -> SchemaClaim:
        key = ContentIdentity.key("schema", repr(self._schema()).encode())
        try:
            self._schema().validate(frame, lazy=lazy)
            return SchemaClaim(ClaimStatus.PASSED, len(self.rules), 0, (), key)
        except (SchemaErrors, SchemaError) as fault:
            cases = getattr(fault, "failure_cases", None)
            pairs = (
                tuple((str(c), str(k)) for c, k in cases.select(["column", "check"]).iter_rows())
                if cases is not None
                else ((str(getattr(fault, "schema", "?")), str(getattr(fault, "check", "?"))),)
            )
            return SchemaClaim(ClaimStatus.FAILED, len(self.rules), len(pairs), pairs, key)
```

## [7]-[RESEARCH]

- [POLARS_STREAMING]: the Polars `LazyFrame.collect(engine="streaming")` and `scan_parquet` projection/predicate-pushdown spellings and the PyArrow `dataset.Scanner` filter expression verify against `.api/api-polars.md`, `.api/api-pyarrow.md` once the marker-floor environment installs them (pyarrow is GATED on 3.15; polars is reflected 1.41.2). The legacy `collect(streaming=True)` knob was deprecated in polars 1.25 and removed from the signature — `engine="streaming"` is the reflected spelling. The DuckDB relational arrow export is now reflected — see [REFLECTED_ENGINES].
- [REFLECTED_ENGINES]: `deltalake` 1.6.0, `duckdb` 1.5.3, `narwhals` 2.22.1, and `pandera` 0.31.1 are INSTALLED and reflected in this environment; the LAKEHOUSE, QUERY, and QUALITY fences use reflected spellings. The `.api/api-deltalake.md`, `.api/api-duckdb.md`, `.api/api-pandera.md` catalogues, and the charter CATALOGUE_PENDING note, still record these as "un-reflectable / no cp315 wheel" — that capture is STALE and reconciles in ALIGN (the `narwhals` distribution has no `.api` row yet). `DeltaTable.optimize` is a property exposing `.compact(...)`/`.z_order(...)`, not a method; `load_cdf` returns a `pyarrow.RecordBatchReader`; `DuckDBPyConnection.sql(...).to_arrow_table()` is the zero-copy result binding; `narwhals.from_native(...).lazy()` keeps the query lazy until `.collect()`.
- [PANDERA_BACKEND]: `pandera.polars.DataFrameSchema.validate(frame, lazy=True)` raises `pandera.errors.SchemaErrors` carrying a `.failure_cases` frame with `column`/`check` columns; `lazy=False` raises `SchemaError` on the first failure. `DataQuality` rails this exactly once at `boundary` and records the failure cases into `SchemaClaim` rather than re-raising — the claim is verified-by-stability evidence, the C# native-BLAS posture.
