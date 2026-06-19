# [PY_DATA_DATASET]

The dataset-reference identity owner: one polymorphic owner discriminating by source shape, with the cross-engine lazy/streaming scan, the typed columnar egress, and the content-keyed query receipt. `DatasetRef` is the one dataset owner; `DatasetKind` the closed `StrEnum` discriminating the admitted source shapes. `ScanPlan` is the engine/projection/predicate/partition policy with cases for the Polars lazy plan, the DuckDB relational API, and the PyArrow dataset scanner; `ColumnarEgress` is the typed Arrow/Parquet/IPC export folding one `QueryReceipt` over scan plus transform plus egress. Every receipt is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[DATASET]`: the dataset-ref owner discriminating by source shape.
- `[3]-[SCAN]`: engine scan plans, columnar egress, the content-keyed query receipt.
- `[4]-[MATERIALIZE]`: the incremental CDC-materialization owner — partition-delta recompute keyed by content identity.

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

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `WindowFunction` the analytical window-verb row carrying its `DuckDBPyRelation` window spelling; `ColumnarEgress` the typed Arrow/Parquet/IPC export; `QueryReceipt` the one typed fault/receipt fold over scan plus transform plus egress.
- Cases: `ScanPlan` cases `PolarsLazy(projection, predicate)` (Polars `LazyFrame`/`scan_*`/`collect`) · `DuckDb(sql, projection)` (DuckDB relational API) · `ArrowDataset(predicate, columns)` (PyArrow `dataset.Scanner`, the predicate a pre-built `pyarrow.dataset.Expression` policy value the body never re-parses from a string) · `RemoteGlob(glob, predicate, partition_keys)` (DuckDB `read_parquet(file_glob)` over a request-scoped connection that self-loads the `httpfs` extension and Hive-partition-prunes by the predicate) · `Window(partitions, order, functions)` (the `DuckDBPyRelation` analytical window-function projection over the `WindowFunction` verb rows), matched by `match`/`case` closed by `assert_never`, each binding the engine that owns it.
- Entry: `execute(plan, dataset)` runs the plan and returns a `RuntimeRail[pyarrow.Table]` over the Arrow C Data Interface (zero-copy); `ColumnarEgress.write` emits Arrow/Parquet/IPC keyed by `ContentIdentity`; `QueryReceipt.of` folds the engine/source/columns/predicate-count/row-count/content-key.
- Auto: the Polars path selects the lazy reader off `dataset.kind` through one in-arm `scan` table (`scan_csv`/`scan_parquet`/`scan_ipc`/`scan_delta`), then runs `.select(projection).filter(predicate).collect(engine="streaming")` — the reader is the dataset kind, never a hardcoded format; the DuckDB path runs the relational API over `duckdb.connect`; the PyArrow path runs `dataset(source).scanner(columns, filter).to_table()` with a pre-built `Expression` predicate; the `RemoteGlob` path opens a request-scoped `duckdb.connect()`, runs `INSTALL httpfs; LOAD httpfs` once on that connection (the per-connection extension-load pattern the `geospatial` `SpatialEngine` establishes for `spatial`/`h3`, never a pre-loaded extension — httpfs is NOT loaded on a sibling path), `read_parquet(glob)` against the remote glob with the predicate pushed into the relational filter so only the partition files the predicate touches are pulled, and `register_filesystem(dataset.ref.path.fs)` registers the `fsspec` `AbstractFileSystem` the `ResourceRef` `UPath` resolves (the `s3`/`gcs` credentials baked into the `storage_options` the runtime `roots#RESOURCE` `ResourceRoot.admit` resolves through `url_to_fs`) so the `s3`/`gcs` glob authenticates through the one runtime-owned filesystem, never a second credential owner and never the `http`/`ssh` `TransportResource` (which fetches generic artifacts, not a cloud-store filesystem); the `Window` path builds the `row_number`/`rank`/`lag`/`first_value`/… projection from the `WindowFunction` rows over the partition/order spec, never ten enumerated arms; remote sourcing rides ADBC where the source is a connection (ConnectorX rides the `<3.15` gated band, never a module-top import). `polars` is on `banned-module-level-imports`, so the reader table builds inside the polars arm under `# noqa: PLC0415`. `engine="streaming"` is the streaming spelling, never the `collect(streaming=True)` flag. The DuckDB connection stays request-scoped via `duckdb.connect()`, never a module global.
- Receipt: the scan contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces a `QueryReceipt` keyed by `ContentIdentity` over the egress bytes, never a generic receipt.
- Packages: `polars` (`scan_parquet`/`LazyFrame.collect`), `duckdb` (`connect`/relational API/`read_parquet(file_glob)`/`install_extension`/`load_extension`/`register_filesystem`/the `DuckDBPyRelation` window-function family `row_number`/`rank`/`dense_rank`/`lag`/`lead`/`first_value`/`last_value`/`n_tile`/`cume_dist`/`percent_rank`), `pyarrow` (`dataset`/`Scanner`/`Table`), `adbc-driver-manager`, `connectorx`, `deltalake`, runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`/`ResourceRef` — the `RemoteGlob` arm registers the `ResourceRef.path.fs` `fsspec` filesystem the runtime `roots#RESOURCE` owner resolves).
- Growth: a new engine is one `ScanPlan` case; a new remote source is one `ScanPlan` case; a new window verb is one `WindowFunction` row; a new egress format is one `ColumnarEgress` branch; zero new surface.
- Boundary: no durable query rails, no global DuckDB connection; a generic receipt abstraction, a per-engine egress class family, a `scan_remote`/`scan_glob`/`window_rank` method family, a second SQL engine or second transport owner, and a pre-loaded-httpfs assumption are the deleted forms.

```python signature
import duckdb
import pyarrow as pa
import pyarrow.dataset as pads
import pyarrow.feather as paf
import pyarrow.parquet as papq
from enum import StrEnum
from typing import Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class WindowFunction(StrEnum):
    ROW_NUMBER = "row_number"
    RANK = "rank"
    DENSE_RANK = "dense_rank"
    LAG = "lag"
    LEAD = "lead"
    FIRST_VALUE = "first_value"
    LAST_VALUE = "last_value"
    N_TILE = "n_tile"
    CUME_DIST = "cume_dist"
    PERCENT_RANK = "percent_rank"

    def projection(self, alias: str, over: str, *, args: str = "") -> str:
        return f"{self.value}({args}) {over} AS {alias}"


@tagged_union(frozen=True)
class ScanPlan:
    tag: Literal["polars_lazy", "duckdb", "arrow_dataset", "remote_glob", "window"] = tag()
    polars_lazy: tuple[tuple[str, ...], str] = case()
    duckdb: tuple[str, tuple[str, ...]] = case()
    arrow_dataset: tuple[pads.Expression | None, tuple[str, ...]] = case()
    remote_glob: tuple[str, str, tuple[str, ...]] = case()
    window: tuple[tuple[str, ...], tuple[str, ...], tuple[tuple[WindowFunction, str], ...]] = case()

    @staticmethod
    def PolarsLazy(projection: tuple[str, ...], predicate: str) -> "ScanPlan":
        return ScanPlan(polars_lazy=(projection, predicate))

    @staticmethod
    def DuckDb(sql: str, projection: tuple[str, ...]) -> "ScanPlan":
        return ScanPlan(duckdb=(sql, projection))

    @staticmethod
    def ArrowDataset(predicate: pads.Expression | None, columns: tuple[str, ...]) -> "ScanPlan":
        return ScanPlan(arrow_dataset=(predicate, columns))

    @staticmethod
    def RemoteGlob(glob: str, predicate: str = "", partition_keys: tuple[str, ...] = ()) -> "ScanPlan":
        return ScanPlan(remote_glob=(glob, predicate, partition_keys))

    @staticmethod
    def Window(partitions: tuple[str, ...], order: tuple[str, ...], functions: tuple[tuple[WindowFunction, str], ...]) -> "ScanPlan":
        return ScanPlan(window=(partitions, order, functions))


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
            content_key=ContentIdentity.of("query", f"{engine}:{source}".encode()),
        )

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", self.engine, self.source, {"rows": str(self.row_count)})


def execute(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[pa.Table]":
    return boundary(f"scan.{plan.tag}", lambda: _run(plan, dataset))


def _run(plan: ScanPlan, dataset: DatasetRef) -> pa.Table:
    source = str(dataset.ref.path)
    match plan:
        case ScanPlan(tag="polars_lazy", polars_lazy=(projection, predicate)):
            import polars as pl  # noqa: PLC0415

            scan = {
                DatasetKind.CSV: pl.scan_csv, DatasetKind.PARQUET: pl.scan_parquet,
                DatasetKind.ARROW_IPC: pl.scan_ipc, DatasetKind.DELTA: pl.scan_delta,
            }
            lf = scan[dataset.kind](source)
            lf = lf.select(list(projection)) if projection else lf
            lf = lf.filter(pl.sql_expr(predicate)) if predicate else lf
            return lf.collect(engine="streaming").to_arrow()
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            rel = duckdb.connect().sql(sql)
            rel = rel.project(", ".join(projection)) if projection else rel
            return rel.to_arrow_table()
        case ScanPlan(tag="arrow_dataset", arrow_dataset=(predicate, columns)):
            return pads.dataset(source).scanner(columns=list(columns) or None, filter=predicate).to_table()
        case ScanPlan(tag="remote_glob", remote_glob=(glob, predicate, _partition_keys)):
            con = duckdb.connect()
            con.install_extension("httpfs")
            con.load_extension("httpfs")
            con.register_filesystem(dataset.ref.path.fs)
            rel = con.read_parquet(glob)
            rel = rel.filter(predicate) if predicate else rel
            return rel.to_arrow_table()
        case ScanPlan(tag="window", window=(partitions, order, functions)):
            over = f"OVER (PARTITION BY {', '.join(partitions)} ORDER BY {', '.join(order)})"
            projection = ", ".join(verb.projection(alias, over) for verb, alias in functions)
            return duckdb.connect().sql(f"SELECT *, {projection} FROM read_parquet('{source}')").to_arrow_table()
        case unreachable:
            assert_never(unreachable)
```

## [4]-[MATERIALIZE]

- Owner: `DerivedSnapshot` — the one incremental CDC-materialization owner folding the `lakehouse` change feed, the `query` engine, and `ContentIdentity` into a partition-delta recompute; `PartitionBundle` the per-partition content-keyed Arrow bundle. The derived view composes the `lakehouse#LAKEHOUSE` `Lakehouse` owner for the source `table_uri` identity and reads the Change Data Feed between two snapshot versions through the same `deltalake.load_cdf` surface the `lakehouse` `ChangeFeed` op binds, derives the changed-partition set from the CDF `_commit_version` range, routes only the changed rows through the `query/relational#QUERY` `QueryEngine`, and re-keys only the touched partition bundles — an unchanged partition's content-key is reused untouched. A full re-scan is the deleted form.
- Entry: `DerivedSnapshot.refresh` admits the source `Lakehouse`, a `start`/`end` version range, a `QuerySpec` transform (carried on the owner), and the prior `tuple[PartitionBundle, ...]`; it reads the CDF over the `Lakehouse.table_uri`, partitions the CDF frame by the `partition_by` keys, recomputes each changed partition through `QueryEngine.run` over the changed rows, and folds one `tuple[PartitionBundle, ...]` where every untouched partition carries its prior `ContentKey` by reference; the return is a `RuntimeRail[tuple[PartitionBundle, ...]]`.
- Auto: the changed-partition set is exactly the distinct partition values present in the CDF `_change_type`-filtered frame over the `_commit_version` range, so an unchanged partition never re-keys; the recompute stays lazy — the `QueryEngine.Rel`/`Sql` path pushes the delta predicate into the DuckDB relation rather than materializing the full table; each recomputed partition keys by `ContentIdentity.of` over its Arrow bytes, and the parent snapshot key folds the partition `ContentKey`s through the Merkle `tuple[ContentKey, ...]` `ContentIdentity.of` source so a single changed partition flips the snapshot key while the unchanged children stay byte-stable.
- Receipt: the refresh folds the shared `QueryReceipt` over the recomputed delta and contributes an emitted-phase `Receipt.of` through `ReceiptContributor`; no new receipt rail.
- Packages: `deltalake` (the `load_cdf` change feed with `_change_type`/`_commit_version`, composed through `lakehouse`), `duckdb` (the partition-delta recompute, composed through `query`), `pyarrow` (`Table`/`RecordBatchReader`/`compute` partition grouping), runtime (`ContentIdentity`/`ContentKey`/`RuntimeRail`/`ReceiptContributor`).
- Growth: a new transform is a different `QuerySpec`; a new partition strategy is one `partition_by` tuple; a second source format is the `lakehouse` `TableFormat` axis with zero change here; zero new surface.
- Boundary: composes the `lakehouse` `ChangeFeed` op and the `query` `QueryEngine`, never re-minting either; no full re-scan, no parallel materialization module, no durable derived store, no second CDF reader; a per-partition recompute class family and a re-derived change feed are the deleted forms.

```python signature
import pyarrow as pa
import pyarrow.compute as pc
from deltalake import DeltaTable
from msgspec import Struct

from rasm.data.lakehouse.table import Lakehouse
from rasm.data.query.relational import QueryEngine, QuerySpec
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class PartitionBundle(Struct, frozen=True):
    partition: str
    rows: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "derived-snapshot", self.partition, {"rows": str(self.rows)})


class DerivedSnapshot(Struct, frozen=True):
    partition_by: tuple[str, ...]
    transform: QuerySpec

    def refresh(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...],
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        return boundary("derived.refresh", lambda: self._materialize(source, start, end, prior))

    def _materialize(
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...],
    ) -> tuple[PartitionBundle, ...]:
        reader = DeltaTable(source.table_uri).load_cdf(starting_version=start, ending_version=end)
        cdf = reader.read_all()
        key_col = self.partition_by[0]
        changed = pc.unique(cdf.column(key_col)).to_pylist()
        prior_by_key = {b.partition: b for b in prior}
        recomputed = {
            str(value): self._recompute(cdf.filter(pc.equal(cdf.column(key_col), value)), str(value))
            for value in changed
        }
        return tuple(
            recomputed.get(b.partition, b) for b in prior
        ) + tuple(recomputed[k] for k in recomputed if k not in prior_by_key)

    def _recompute(self, delta: pa.Table, partition: str) -> PartitionBundle:
        result = QueryEngine.of({"delta": delta}).run(self.transform).default_value(delta)
        return PartitionBundle(
            partition=partition, rows=result.num_rows,
            content_key=ContentIdentity.of("partition", result.to_batches()[0].serialize() if result.num_rows else b""),
        )


def snapshot_key(bundles: tuple[PartitionBundle, ...]) -> ContentKey:
    return ContentIdentity.of("derived-snapshot", tuple(b.content_key for b in bundles))
```

## [5]-[RESEARCH]

- [CDF_PARTITION_GROUP]: the `deltalake` `DeltaTable.load_cdf(starting_version=, ending_version=)` reader and its `_change_type`/`_commit_version` CDF columns the `_materialize` partition-grouping reads are catalogue-confirmed against the folder `deltalake` `.api`; the `pyarrow.compute` `unique`/`equal` partition-grouping and the `RecordBatch.serialize` content-key source are stdlib-Arrow members, so the one open seam is the `QueryEngine.run` rail unwrap (`Result.default_value`) the `_recompute` uses to lower the delta-query rail into the partition bundle — the unwrap reuses the prior delta on a query fault rather than propagating, an intentional reuse-on-empty-delta policy the refresh boundary owns.
