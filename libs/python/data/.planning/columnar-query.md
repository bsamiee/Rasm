# [PY_DATA_COLUMNAR_QUERY]

Typed dataset refs, columnar lazy/streaming scans across engines, and columnar egress. `DatasetRef` is the one polymorphic dataset owner discriminating by source shape; `ScanPlan` is the engine/projection/predicate/partition policy with cases for the Polars LazyFrame plan, the DuckDB relational API, and the PyArrow dataset scanner; `ColumnarEgress` is the typed Arrow/Parquet/IPC export folding one `QueryReceipt` over scan + transform + egress, wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | DATASET   | the dataset ref owner discriminating by source shape           |
|   [2]   | SCAN      | engine scan plans, columnar egress, the query receipt          |

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
- Auto: the Polars path runs `scan_parquet(...).select(projection).filter(predicate).collect(streaming=True)`; the DuckDB path runs the relational API over `duckdb.connect`; the PyArrow path runs `dataset(source).scanner(columns, filter).to_table()`; remote sourcing rides ADBC/ConnectorX where the source is a connection.
- Receipt: the scan contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a `QueryReceipt` keyed by `ContentIdentity` over the egress bytes — never a generic `IReceipt`.
- Packages: `polars` (`scan_parquet`/`LazyFrame.collect`), `duckdb` (`connect`/relational API), `pyarrow` (`dataset`/`Scanner`/`Table`), `adbc-driver-manager`, `connectorx`, `deltalake`, runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`).
- Growth: a new engine is one `ScanPlan` case; a new egress format is one `ColumnarEgress` branch; zero new surface.
- Boundary: no durable query rails, no global DuckDB connection; a generic receipt abstraction and a per-engine egress class family are the deleted forms.

```python signature
from typing import Literal

import pyarrow as pa
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
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


class QueryReceipt(Struct, frozen=True):
    engine: str
    source: str
    columns: int
    predicate_count: int
    row_count: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.Emitted(self.engine, self.source, {"rows": str(self.row_count)})


def execute(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[pa.Table]":
    return boundary(f"scan.{plan.tag}", lambda: _run(plan, dataset))
```

## [4]-[RESEARCH]

- [POLARS_STREAMING]: the Polars `LazyFrame.collect(streaming=True)` and `scan_parquet` projection/predicate-pushdown spellings, the DuckDB relational API arrow export, and the PyArrow `dataset.Scanner` filter expression are verified against `.api/api-polars.md`, `.api/api-duckdb.md`, `.api/api-pyarrow.md` once a cp315 wheel or the marker-floor environment installs the engines (suite TASKLOG `PY_API_002a`).
