# [PY_DATA_DATASET]

The dataset-reference identity owner: one polymorphic owner discriminating by source shape, with the cross-engine lazy/streaming scan, the typed columnar egress, and the content-keyed query receipt. `DatasetRef` is the one dataset owner; `DatasetKind` the closed `StrEnum` discriminating the admitted source shapes. `ScanPlan` is the engine/projection/predicate/partition policy with cases for the Polars lazy plan, the DuckDB relational API, and the PyArrow dataset scanner; `ColumnarEgress` is the typed Arrow/Parquet/IPC export folding one `QueryReceipt` over scan plus transform plus egress. Every receipt is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[DATASET]`: the dataset-ref owner discriminating by source shape.
- `[3]-[SCAN]`: engine scan plans, columnar egress, the content-keyed query receipt.

## [2]-[DATASET]

- Owner: `DatasetRef` — the one polymorphic dataset owner; `DatasetKind` the closed `StrEnum` discriminating CSV/Parquet/Arrow-IPC/Arrow-dataset/Delta/Pandas-file/Polars-file/.3dm/mesh/HDF source shapes.
- Cases: `DatasetKind` rows `CSV` · `PARQUET` · `ARROW_IPC` · `ARROW_DATASET` · `DELTA` · `PANDAS_FILE` · `POLARS_FILE` · `RHINO_3DM` · `MESH` · `HDF`, matched by `match`/`case`, never a Get/List/Scan family.
- Entry: `DatasetRef.of` admits a `ResourceRef` and a `DatasetKind` and returns the frozen owner; the kind is recoverable from the source shape, never a knob.
- Packages: `polars`, `pyarrow`, `pandas`, `rhino3dm`, `meshio`, `trimesh`, `h5py`, runtime (`ResourceRef`/`ContentIdentity`).
- Growth: a new source shape is one `DatasetKind` row; zero new surface.
- Boundary: no product identity, repository, or host-document mutation; a `get_csv`/`read_parquet`/`load_delta` family is the deleted form; HDF array persistence is a `DatasetKind` row (file exchange), not a compute concern.

```python signature
from enum import StrEnum

from msgspec import Struct

from rasm.runtime.roots import ResourceRef


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

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `ColumnarEgress` the typed Arrow/Parquet/IPC export; `QueryReceipt` the one typed fault/receipt fold over scan plus transform plus egress.
- Cases: `ScanPlan` cases `PolarsLazy(projection, predicate)` (Polars `LazyFrame`/`scan_*`/`collect`) · `DuckDb(sql, projection)` (DuckDB relational API) · `ArrowDataset(predicate, columns)` (PyArrow `dataset.Scanner`, the predicate a pre-built `pyarrow.dataset.Expression` policy value the body never re-parses from a string), matched by `match`/`case`, each binding the engine that owns it.
- Entry: `execute(plan, dataset)` runs the plan and returns a `RuntimeRail[pyarrow.Table]` over the Arrow C Data Interface (zero-copy); `ColumnarEgress.write` emits Arrow/Parquet/IPC keyed by `ContentIdentity`; `QueryReceipt.of` folds the engine/source/columns/predicate-count/row-count/content-key.
- Auto: the Polars path selects the lazy reader off `dataset.kind` through one `_POLARS_SCAN` table (`scan_csv`/`scan_parquet`/`scan_ipc`/`scan_delta`), then runs `.select(projection).filter(predicate).collect(engine="streaming")` — the reader is the dataset kind, never a hardcoded format; the DuckDB path runs the relational API over `duckdb.connect`; the PyArrow path runs `dataset(source).scanner(columns, filter).to_table()` with a pre-built `Expression` predicate; remote sourcing rides ADBC/ConnectorX where the source is a connection. `engine="streaming"` is the streaming spelling, never the `collect(streaming=True)` flag.
- Receipt: the scan contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a `QueryReceipt` keyed by `ContentIdentity` over the egress bytes, never a generic receipt.
- Packages: `polars` (`scan_parquet`/`LazyFrame.collect`), `duckdb` (`connect`/relational API), `pyarrow` (`dataset`/`Scanner`/`Table`), `adbc-driver-manager`, `connectorx`, `deltalake`, runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new engine is one `ScanPlan` case; a new egress format is one `ColumnarEgress` branch; zero new surface.
- Boundary: no durable query rails, no global DuckDB connection; a generic receipt abstraction and a per-engine egress class family are the deleted forms.

```python signature
import duckdb
import polars as pl
import pyarrow as pa
import pyarrow.dataset as pads
import pyarrow.feather as paf
import pyarrow.parquet as papq
from typing import Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability.receipts import Receipt
from rasm.runtime.faults import RuntimeRail, boundary


@tagged_union(frozen=True)
class ScanPlan:
    tag: Literal["polars_lazy", "duckdb", "arrow_dataset"] = tag()
    polars_lazy: tuple[tuple[str, ...], str] = case()
    duckdb: tuple[str, tuple[str, ...]] = case()
    arrow_dataset: tuple[pads.Expression | None, tuple[str, ...]] = case()

    @staticmethod
    def PolarsLazy(projection: tuple[str, ...], predicate: str) -> "ScanPlan":
        return ScanPlan(polars_lazy=(projection, predicate))

    @staticmethod
    def DuckDb(sql: str, projection: tuple[str, ...]) -> "ScanPlan":
        return ScanPlan(duckdb=(sql, projection))

    @staticmethod
    def ArrowDataset(predicate: pads.Expression | None, columns: tuple[str, ...]) -> "ScanPlan":
        return ScanPlan(arrow_dataset=(predicate, columns))


@tagged_union(frozen=True)
class ColumnarEgress:
    tag: Literal["arrow_ipc", "parquet", "feather"] = tag()
    arrow_ipc: str = case()
    parquet: tuple[str, str] = case()
    feather: str = case()

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
            case unreachable:
                assert_never(unreachable)
        return QueryReceipt.of(self.tag, target, table)


class QueryReceipt(Struct, frozen=True):
    engine: str
    source: str
    columns: int
    predicate_count: int
    row_count: int
    content_key: ContentKey

    @classmethod
    def of(cls, engine: str, source: str, table: pa.Table, *, predicate_count: int = 0) -> "QueryReceipt":
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


_POLARS_SCAN: dict[DatasetKind, "object"] = {
    DatasetKind.CSV: pl.scan_csv,
    DatasetKind.PARQUET: pl.scan_parquet,
    DatasetKind.ARROW_IPC: pl.scan_ipc,
    DatasetKind.DELTA: pl.scan_delta,
}


def execute(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[pa.Table]":
    return boundary(f"scan.{plan.tag}", lambda: _run(plan, dataset))


def _run(plan: ScanPlan, dataset: DatasetRef) -> pa.Table:
    source = str(dataset.ref.path)
    match plan:
        case ScanPlan(tag="polars_lazy", polars_lazy=(projection, predicate)):
            lf = _POLARS_SCAN[dataset.kind](source)
            lf = lf.select(list(projection)) if projection else lf
            lf = lf.filter(pl.sql_expr(predicate)) if predicate else lf
            return lf.collect(engine="streaming").to_arrow()
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            rel = duckdb.connect().sql(sql)
            rel = rel.project(", ".join(projection)) if projection else rel
            return rel.to_arrow_table()
        case ScanPlan(tag="arrow_dataset", arrow_dataset=(predicate, columns)):
            return pads.dataset(source).scanner(columns=list(columns) or None, filter=predicate).to_table()
        case unreachable:
            assert_never(unreachable)
```
