# [PY_DATA_COLUMNAR]

The dataset-reference identity owner: one polymorphic owner discriminating by source shape, with the cross-engine lazy/streaming scan, the typed columnar egress, and the content-keyed query receipt. `DatasetRef` is the one dataset owner; `DatasetKind` the closed `StrEnum` discriminating the admitted columnar source shapes, the geometry/HDF rows routed to their real owners. `ScanPlan` is the engine/projection/predicate/partition policy with cases for the Polars lazy plan, the `register_io_source` pushdown plugin, the DuckDB relational API, the PyArrow dataset scanner, the remote glob, the analytical window projection, the `fastexcel` spreadsheet decode, and the artifacts corpus-row wire; `ColumnarEgress` is the typed Arrow/Parquet/IPC export folding one `QueryReceipt` over scan plus transform plus egress. Every receipt is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[DATASET]: the dataset-ref owner discriminating by source shape.
- [02]-[SCAN]: engine scan plans, columnar egress, the content-keyed query receipt.
- [03]-[MATERIALIZE]: the incremental CDC-materialization owner — partition-delta recompute keyed by content identity.

## [02]-[DATASET]

- Owner: `DatasetRef` — the one polymorphic dataset owner; `DatasetKind` the closed `StrEnum` discriminating the columnar-plane source shapes the scan owner reads end-to-end. The geometry/HDF source shapes leave the axis: `.3dm` reading is a `geometry` concern reaching the columnar plane only through the settled `spatial/mesh ⇄ python:geometry` Arrow point-record seam, the unstructured-mesh file row lands in `spatial/mesh`, and the chunked HDF field row lands in `gridded/field` — `DatasetKind` carries no eager row that has no `ScanPlan` arm.
- Cases: `DatasetKind` rows `CSV` · `PARQUET` · `ARROW_IPC` · `ARROW_DATASET` · `NDJSON` · `DELTA` · `ICEBERG` · `EXCEL` · `PANDAS_FILE` · `POLARS_FILE`, matched by `match`/`case`, never a Get/List/Scan family. Each lazy-scannable row carries its `polars` reader on the `scan_reader` behavior column (the `POLICY_VALUES` law — the vocabulary owns its reader, so a `PolarsLazy`/`IoSource` plan resolves `kind.scan_reader` rather than a module-private dict covering a silent subset); `ARROW_DATASET`/`EXCEL`/`PANDAS_FILE`/`POLARS_FILE` carry `scan_reader is None` so the lazy arms reject them at the named `scan.polars` boundary that converts the explicit reader-absence error to a `BoundaryFault`, never a silent `KeyError`. `EXCEL` is the spreadsheet wire row the `ScanPlan.Excel` arm decodes through `fastexcel` calamine; `RHINO_3DM`/`MESH`/`HDF` are the dropped rows whose reads route to their real owners.
- Entry: `DatasetRef.of` admits a `ResourceRef` and a `DatasetKind` and returns the frozen owner; the kind is recoverable from the source shape, never a knob.
- Packages: `polars`, `pyarrow`, `pandas`, `fastexcel` (the calamine xlsx/xlsm/xlsb/xls/ods reader the `Excel` arm decodes through the lazy `ExcelSheet`+`to_arrow_with_errors` pyarrow path on this pyarrow-resident plane, the `__arrow_c_array__` PyCapsule the pyarrow-free interop carrier rides elsewhere; a producer, never a `ScanPlan` engine backend), runtime (`ResourceRef`/`ContentIdentity`).
- Growth: a new columnar source shape is one `DatasetKind` row plus its `_SCAN_READER` reader entry when lazy-scannable (absent when the kind reaches the plane through a dedicated `ScanPlan` arm); a spreadsheet source is the `EXCEL` row the `Excel` arm already reads; a geometry/raster source is a row on its real owner's axis, never re-admitted here; zero new surface.
- Boundary: no product identity, repository, or host-document mutation; a `get_csv`/`read_parquet`/`load_delta`/`read_excel` method family is the deleted form; `fastexcel` is a `DatasetKind`-plus-capsule-ingest producer, never a `ScanPlan` engine backend row; a `RHINO_3DM`/`MESH`/`HDF` row with no scan arm forcing an eager read inside the columnar plane is the deleted form, the geometry read routing through the existing `spatial/mesh ⇄ python:geometry` seam rather than a new `columnar → geometry` edge.

```python signature
from enum import StrEnum
from typing import Final

from msgspec import Struct

from rasm.runtime.roots import ResourceRef

_SCAN_READER: Final[dict[str, str]] = {
    "csv": "scan_csv",
    "parquet": "scan_parquet",
    "arrow-ipc": "scan_ipc",
    "ndjson": "scan_ndjson",
    "delta": "scan_delta",
    "iceberg": "scan_iceberg",
}


class DatasetKind(StrEnum):
    CSV = "csv"
    PARQUET = "parquet"
    ARROW_IPC = "arrow-ipc"
    ARROW_DATASET = "arrow-dataset"
    NDJSON = "ndjson"
    DELTA = "delta"
    ICEBERG = "iceberg"
    EXCEL = "excel"
    PANDAS_FILE = "pandas-file"
    POLARS_FILE = "polars-file"

    @property
    def scan_reader(self) -> str | None:
        return _SCAN_READER.get(self.value)


class DatasetRef(Struct, frozen=True):
    ref: ResourceRef
    kind: DatasetKind

    @classmethod
    def of(cls, ref: ResourceRef, kind: DatasetKind) -> "DatasetRef":
        return cls(ref=ref, kind=kind)
```

## [03]-[SCAN]

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `WindowFunction` the analytical window-verb row carrying its `DuckDBPyRelation` window spelling; `ExcelSpec` the named decode-policy `Struct` the `Excel` case carries (the windowing, dtype, selection, and whitespace axes as one frozen field set); `ColumnarEgress` the typed Arrow/Parquet/IPC export; `QueryReceipt` the one typed receipt fold over scan plus transform plus egress, carrying the optional column-level `lineage_edges` projection the `tabular/query#QUERY` `QueryEngine` populates from `sqlglot.find_tables`/`ibis.to_sql` and the scan path leaves empty. The eager-reader cases (`PolarsLazy` plus the geometry/HDF rows that owned no arm) collapse into the lazy `IoSource` pushdown plane and the two wire-ingest arms, so every `ScanPlan` case terminates in the same `RuntimeRail[pa.Table]` over the Arrow C Data Interface.
- Cases: `ScanPlan` cases `PolarsLazy(projection, predicate)` (Polars `LazyFrame`/`scan_*`/`collect`) · `IoSource(projection, predicate)` (the `polars.io.plugins.register_io_source` lazy plugin lifting a Rasm `DatasetRef` into a `LazyFrame` with predicate/projection pushdown, the generator callable yielding `pa.RecordBatch` windows the plugin folds into the lazy graph) · `DuckDb(sql, projection)` (DuckDB relational API) · `ArrowDataset(predicate, columns)` (PyArrow `dataset.dataset(source).scanner(columns=, filter=).to_table()`, the predicate a pre-built `pyarrow.dataset.Expression` policy value the body never re-parses from a string) · `RemoteGlob(glob, predicate, partition_keys)` (DuckDB `read_parquet(file_glob)` over a request-scoped connection that self-loads the `httpfs` extension and Hive-partition-prunes by the predicate) · `Window(partitions, order, functions)` (the `DuckDBPyRelation.read_parquet(...).select(StarExpression(), *window_exprs)` analytical window-function projection over the `WindowFunction` verb rows, each verb emitting one `duckdb.SQLExpression(...).alias(...)` window node — the parquet path and the projection both expression-parameterized, never an f-string `project(...)` interpolation) · `Excel(spec)` (the `fastexcel` calamine reader materializing one sheet or named table through the lazy `ExcelSheet`+`to_arrow_with_errors` decode under the named `ExcelSpec` field set, the per-cell `CellErrors` and sheet evidence stamped onto the one-batch `pa.Table`) · `Corpus(rows)` (the artifacts `documents` `to_corpus_row` flat-record wire lifted into a `pa.Table` through `pa.Table.from_pylist` on this pyarrow-resident plane, the resulting Arrow table riding the agnostic `interop#INTEROP` `FrameInterop.c_stream` carrier zero-copy at the downstream hop, the data-side endpoint of the `tabular ← python:artifacts/documents [WIRE]` seam), matched by `match`/`case` closed by `assert_never`, each binding the engine or wire that owns it.
- Entry: `execute(plan, dataset)` runs the plan and returns a `RuntimeRail[pa.Table]` over the Arrow C Data Interface (zero-copy) for the egress hop; `scan(plan, dataset)` maps the same materialization into a `RuntimeRail[tuple[pa.Table, QueryReceipt]]`, folding `QueryReceipt.of(plan.tag, source, table, predicate_count=plan.predicate_count)` so the scan-only path carries the same receipt the egress path stamps, never a second receipt rail; `ColumnarEgress.write(table, predicate_count=)` emits Arrow/Parquet/IPC keyed by `ContentIdentity` threading the plan's predicate count; `QueryReceipt.of` folds the engine/source/columns/predicate-count/row-count/content-key. `ScanPlan.predicate_count` is the one derived projection over the case axis — the polars/io-source predicate-string presence, the arrow-dataset `Expression` presence, the remote-glob predicate presence, and the DuckDB `sqlglot.parse_one(sql).find_all(exp.Where, exp.Having, exp.Qualify, exp.Join)` node count — sharing the `query#QUERY` `_PREDICATE_NODES` widening so the scan and query receipts count predicates identically, never a hardcoded `0`. The `IoSource` arm builds its `LazyFrame` through one `register_io_source` registration whose generator reads the same `DatasetKind.scan_reader` the `PolarsLazy` arm reads, so the plugin-pushed and the direct-lazy scan over one `DatasetRef` fold the byte-identical `QueryReceipt` off the shared reader; the distributed out-of-core runner is the `query#QUERY` `QuerySpec.Streaming` daft case, not a columnar scan arm — `daft` never enters this owner's axis.
- Auto: the Polars path selects the lazy reader off `dataset.kind` through the `DatasetKind.scan_reader` reader column (the `scan_csv`/`scan_parquet`/`scan_ipc`/`scan_ndjson`/`scan_delta`/`scan_iceberg` confirmed rows, and the non-lazy `EXCEL`/`PANDAS_FILE`/`POLARS_FILE`/`ARROW_DATASET` kinds raising the explicit reader-absence error the `execute` boundary converts to a `BoundaryFault` rather than a silent `KeyError`), then runs `.select(projection).filter(predicate).collect(engine="streaming")` — the reader is the dataset-kind row, never a hardcoded format and never a module-private dict covering a silent subset; the `IoSource` path registers one `register_io_source` plugin whose `with_columns`/`predicate` callback receives the polars-pushed projection and predicate and forwards them into the same `DatasetKind.scan_reader` so pushdown crosses the plugin boundary, then `collect(engine="streaming")` materializes to Arrow — the generator is the dataset-kind row, never a per-format plugin; the DuckDB path runs the relational API over `duckdb.connect`; the PyArrow path runs `dataset(source).scanner(columns, filter).to_table()` with a pre-built `Expression` predicate (`pyarrow.dataset` `compute.field`/`compute.scalar` pushdown nodes); the `RemoteGlob` path opens a request-scoped `duckdb.connect()`, runs `INSTALL httpfs; LOAD httpfs` once on that connection (the per-connection extension-load pattern the `geospatial` `SpatialEngine` establishes for `spatial`/`h3`, never a pre-loaded extension — httpfs is NOT loaded on a sibling path), `read_parquet(glob, hive_partitioning=bool(partition_keys))` against the remote glob (the `partition_keys` presence toggling DuckDB Hive-partition-column discovery so the partition columns project into the relation, never the dropped `_partition_keys` discard) with the predicate pushed into the relational filter so only the partition files the predicate touches are pulled, and `register_filesystem(dataset.ref.path.fs)` registers the `fsspec` `AbstractFileSystem` the `ResourceRef` `UPath` resolves (the `s3`/`gcs` credentials baked into the `storage_options` the runtime `roots#RESOURCE` `ResourceRoot.admit` resolves through `url_to_fs`) so the `s3`/`gcs` glob authenticates through the one runtime-owned filesystem, never a second credential owner and never the `http`/`ssh` `TransportResource` (which fetches generic artifacts, not a cloud-store filesystem); the `Window` path builds the `row_number`/`rank`/`lag`/`first_value`/… projection from the `WindowFunction` rows over the partition/order spec as one `(duckdb.StarExpression(), *window_nodes)` `.select(*projection)` where each `WindowFunction.expression` emits a `duckdb.SQLExpression` OVER node aliased through `.alias`, never ten enumerated arms and never an f-string `project("*, ...")` interpolation; the `Excel` path opens `fastexcel.read_excel(source)` once, routes `sheet`-versus-`table` through the one polymorphic `load_sheet`/`load_table` surface (`load_sheet` when `table` is absent, `load_table` when a named table is named — never a per-sheet `load_by_a`/`load_by_b` family), threads the whole `ExcelSpec` field set — `header_row`/`column_names`/`skip_rows`/`n_rows`/`schema_sample_rows`/`use_columns`/`dtypes`/`dtype_coercion`/`skip_whitespace_tail_rows`/`whitespace_as_null` — as one named kwargs plan both load members read (the 7-positional tuple collapsed into the `ExcelSpec` `Struct` so a new decode knob is one field, never a tuple-arity break), decodes through the lazy `ExcelSheet`/`ExcelTable` and `to_arrow_with_errors()` so the per-cell `CellErrors` are captured rather than silently nulled, and lands the one-batch `pa.Table` with the sheet `name`/`total_height`/`visible` and the cell-error count stamped as Arrow schema metadata so the decode evidence rides the uniform return into the receipt — `use_columns`/`dtypes` prune and type at decode, never a post-load projection or cast pass; the `Corpus` path lifts the artifacts-owned `to_corpus_row` flat records (a sequence of `Mapping[str, Any]` arriving at the wire, never re-derived from the document recovery) into a `pa.Table` through `pa.Table.from_pylist`, terminating in the same uniform `pa.Table` every arm returns so the zero-copy hop downstream rides the `interop#INTEROP` `FrameInterop.c_stream` carrier rather than a second Arrow path. Remote sourcing rides ADBC where the source is a connection (ConnectorX rides the `<3.15` gated band, never a module-top import). `polars` and `fastexcel` are on `banned-module-level-imports`, so the polars arm binds `pl` and `read_excel` binds inside the Excel arm under `# noqa: PLC0415`; the `DatasetKind.scan_reader` reader column resolves the bound `pl` member by name (`getattr(pl, kind.scan_reader)`) rather than capturing an unbound module-top handle. `engine="streaming"` is the streaming spelling, never the `collect(streaming=True)` flag. The DuckDB connection stays request-scoped via `duckdb.connect()`, never a module global.
- Receipt: the scan contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces a `QueryReceipt` keyed by `ContentIdentity` over the egress bytes, never a generic receipt; the `scan` entrypoint folds `QueryReceipt.of(..., predicate_count=plan.predicate_count)` so every arm carries its real predicate count — the polars/io-source/remote-glob predicate-string presence and the DuckDB `find_all` node count over the shared `query#QUERY` `_PREDICATE_NODES` widening — never the hardcoded `0`; the `IoSource` and `PolarsLazy` arms over one `DatasetRef` produce the byte-identical receipt off the shared `DatasetKind.scan_reader` reader, and the `Excel`/`Corpus` wire arms key by `ContentIdentity` over the decoded Arrow bytes so a re-ingest of an unchanged workbook or corpus reuses its key untouched; the `Excel` arm carries its `CellErrors` count and sheet `name`/`total_height`/`visible` as Arrow schema metadata so the decode evidence rides the uniform `pa.Table` into the receipt rather than vanishing at the bare return.
- Packages: `polars` (`scan_csv`/`scan_parquet`/`scan_ipc`/`scan_ndjson`/`scan_delta`/`scan_iceberg`/`LazyFrame.select`/`filter`/`collect(engine=)`/`collect_schema`/`sql_expr`/`io.plugins.register_io_source`), `duckdb` (`connect`/Relational API/`read_parquet(file_glob, hive_partitioning=)`/`DuckDBPyRelation.select`/`project`/`filter`/`to_arrow_table`/`install_extension`/`load_extension`/`register_filesystem`/the `SQLExpression`/`StarExpression`/`Expression.alias` window-node builders carrying the `row_number`/`rank`/`dense_rank`/`lag`/`lead`/`first_value`/`last_value`/`n_tile`/`cume_dist`/`percent_rank` verb spelling), `pyarrow` (`dataset.dataset(...).scanner(columns=, filter=).to_table()`/`Table`/`Table.from_pylist`/`Table.from_batches`/`Table.replace_schema_metadata`/`RecordBatch`/`compute.field`/`compute.scalar`/`compute.unique`/`compute.equal`), `sqlglot` (`parse_one`/`find_all(exp.Where, exp.Having, exp.Qualify, exp.Join)` — the shared `query#QUERY` `_PREDICATE_NODES` predicate count over the DuckDB SQL arm), `fastexcel` (`read_excel`/`ExcelReader.{load_sheet,load_table,table_names,sheet_names}`/`ExcelSheet.{to_arrow,to_arrow_with_errors,__arrow_c_array__,name,total_height,visible,selected_columns}`/`CellErrors`), `adbc-driver-manager`, `connectorx`, `deltalake`, `tabular/interop` (`ArrowCStream`/`FrameInterop` — the agnostic carrier the `Corpus` arm rides), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`/`ResourceRef` — the `RemoteGlob` arm registers the `ResourceRef.path.fs` `fsspec` filesystem the runtime `roots#RESOURCE` owner resolves).
- Growth: a new engine is one `ScanPlan` case; a new remote source is one `ScanPlan` case; a new lazy-pushdown source is one `DatasetKind.scan_reader` row the polars arms and the `IoSource` plugin already forward; a new window verb is one `WindowFunction` row; a new spreadsheet decode knob is one `ExcelSpec` field; a new corpus wire field is one column the artifacts producer adds and `from_pylist` already folds; a new egress format is one `ColumnarEgress` branch; zero new surface.
- Boundary: no durable query rails, no global DuckDB connection; a generic receipt abstraction, a per-engine egress class family, a `scan_remote`/`scan_glob`/`window_rank`/`read_excel`/`ingest_corpus` method family, a second SQL engine or second transport owner, a `fastexcel` `ScanPlan` backend row (it is a `DatasetKind`-plus-capsule producer, never an engine), a re-minted document-recovery decoder where the `Corpus` arm consumes the artifacts `to_corpus_row` record at the wire, a second Arrow path where the `interop#INTEROP` carrier owns the zero-copy hop, a per-format polars IO plugin where one `register_io_source` reads `dataset.kind`, an eager geometry/HDF read forced inside the columnar plane, and a pre-loaded-httpfs assumption are the deleted forms.

```python signature
import duckdb
import pyarrow as pa
import pyarrow.dataset as pads
import pyarrow.feather as paf
import pyarrow.parquet as papq
import sqlglot
from collections.abc import Callable, Iterator, Mapping
from enum import StrEnum
from sqlglot import exp
from typing import Any, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

type CorpusRow = Mapping[str, Any]


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

    def expression(self, alias: str, partitions: tuple[str, ...], order: tuple[str, ...], *, args: str = "") -> Any:
        partition_by = f"PARTITION BY {', '.join(partitions)} " if partitions else ""
        order_by = f"ORDER BY {', '.join(order)}" if order else ""
        return duckdb.SQLExpression(f"{self.value}({args}) OVER ({partition_by}{order_by})").alias(alias)


class ExcelSpec(Struct, frozen=True):
    sheet: int | str = 0
    table: str | None = None
    header_row: int | None = 0
    column_names: tuple[str, ...] = ()
    skip_rows: int | None = None
    n_rows: int | None = None
    schema_sample_rows: int = 1000
    use_columns: tuple[str, ...] = ()
    dtypes: Mapping[str | int, str] | None = None
    dtype_coercion: str = "coerce"
    skip_whitespace_tail_rows: bool = False
    whitespace_as_null: bool = False


@tagged_union(frozen=True)
class ScanPlan:
    tag: Literal["polars_lazy", "io_source", "duckdb", "arrow_dataset", "remote_glob", "window", "excel", "corpus"] = tag()
    polars_lazy: tuple[tuple[str, ...], str] = case()
    io_source: tuple[tuple[str, ...], str] = case()
    duckdb: tuple[str, tuple[str, ...]] = case()
    arrow_dataset: tuple[pads.Expression | None, tuple[str, ...]] = case()
    remote_glob: tuple[str, str, tuple[str, ...]] = case()
    window: tuple[tuple[str, ...], tuple[str, ...], tuple[tuple[WindowFunction, str], ...]] = case()
    excel: ExcelSpec = case()
    corpus: tuple[tuple[CorpusRow, ...]] = case()

    @staticmethod
    def PolarsLazy(projection: tuple[str, ...], predicate: str) -> "ScanPlan":
        return ScanPlan(polars_lazy=(projection, predicate))

    @staticmethod
    def IoSource(projection: tuple[str, ...] = (), predicate: str = "") -> "ScanPlan":
        return ScanPlan(io_source=(projection, predicate))

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

    @staticmethod
    def Excel(spec: ExcelSpec = ExcelSpec()) -> "ScanPlan":
        return ScanPlan(excel=spec)

    @staticmethod
    def Corpus(rows: tuple[CorpusRow, ...]) -> "ScanPlan":
        return ScanPlan(corpus=(rows,))

    @property
    def predicate_count(self) -> int:
        match self:
            case ScanPlan(tag="polars_lazy", polars_lazy=(_, predicate)) | ScanPlan(tag="io_source", io_source=(_, predicate)):
                return int(bool(predicate))
            case ScanPlan(tag="arrow_dataset", arrow_dataset=(predicate, _)):
                return int(predicate is not None)
            case ScanPlan(tag="remote_glob", remote_glob=(_, predicate, _)):
                return int(bool(predicate))
            case ScanPlan(tag="duckdb", duckdb=(sql, _)):
                return len(tuple(sqlglot.parse_one(sql).find_all(exp.Where, exp.Having, exp.Qualify, exp.Join)))
            case _:
                return 0


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

    def write(self, table: pa.Table, *, predicate_count: int = 0) -> "RuntimeRail[QueryReceipt]":
        return boundary(f"egress.{self.tag}", lambda: self._emit(table, predicate_count))

    def _emit(self, table: pa.Table, predicate_count: int) -> "QueryReceipt":
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
        return QueryReceipt.of(self.tag, target, table, predicate_count=predicate_count)


class QueryReceipt(Struct, frozen=True):
    engine: str
    source: str
    columns: int
    predicate_count: int
    row_count: int
    content_key: ContentKey
    lineage_edges: tuple[tuple[str, str], ...] = ()

    @classmethod
    def of(
        cls, engine: str, source: str, table: pa.Table, *, predicate_count: int = 0,
        lineage_edges: tuple[tuple[str, str], ...] = (),
    ) -> "QueryReceipt":
        return cls(
            engine=engine,
            source=source,
            columns=table.num_columns,
            predicate_count=predicate_count,
            row_count=table.num_rows,
            content_key=ContentIdentity.of("query", f"{engine}:{source}".encode()),
            lineage_edges=lineage_edges,
        )

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted", self.engine, self.source,
            {"rows": str(self.row_count), "lineage": str(len(self.lineage_edges))},
        )


def execute(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[pa.Table]":
    return boundary(f"scan.{plan.tag}", lambda: _run(plan, dataset))


def scan(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
    return execute(plan, dataset).map(
        lambda table: (table, QueryReceipt.of(plan.tag, str(dataset.ref.path), table, predicate_count=plan.predicate_count))
    )


def _run(plan: ScanPlan, dataset: DatasetRef) -> pa.Table:
    source = str(dataset.ref.path)
    match plan:
        case ScanPlan(tag="polars_lazy", polars_lazy=(projection, predicate)):
            import polars as pl  # noqa: PLC0415

            lf = _scan_lazy(pl, dataset.kind, source)
            return _pushed(pl, lf, projection, predicate).collect(engine="streaming").to_arrow()
        case ScanPlan(tag="io_source", io_source=(projection, predicate)):
            import polars as pl  # noqa: PLC0415

            lf = pl.io.plugins.register_io_source(
                io_source=_io_source(dataset, source), schema=_scan_lazy(pl, dataset.kind, source).collect_schema(),
            )
            return _pushed(pl, lf, projection, predicate).collect(engine="streaming").to_arrow()
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            rel = duckdb.connect().sql(sql)
            rel = rel.project(", ".join(projection)) if projection else rel
            return rel.to_arrow_table()
        case ScanPlan(tag="arrow_dataset", arrow_dataset=(predicate, columns)):
            return pads.dataset(source).scanner(columns=list(columns) or None, filter=predicate).to_table()
        case ScanPlan(tag="remote_glob", remote_glob=(glob, predicate, partition_keys)):
            con = duckdb.connect()
            con.install_extension("httpfs")
            con.load_extension("httpfs")
            con.register_filesystem(dataset.ref.path.fs)
            rel = con.read_parquet(glob, hive_partitioning=bool(partition_keys))
            rel = rel.filter(predicate) if predicate else rel
            return rel.to_arrow_table()
        case ScanPlan(tag="window", window=(partitions, order, functions)):
            projection = (duckdb.StarExpression(), *(verb.expression(alias, partitions, order) for verb, alias in functions))
            return duckdb.connect().read_parquet(source).select(*projection).to_arrow_table()
        case ScanPlan(tag="excel", excel=spec):
            import fastexcel  # noqa: PLC0415

            reader = fastexcel.read_excel(source)
            kwargs = {
                "header_row": spec.header_row, "column_names": list(spec.column_names) or None,
                "skip_rows": spec.skip_rows, "n_rows": spec.n_rows, "schema_sample_rows": spec.schema_sample_rows,
                "dtype_coercion": spec.dtype_coercion, "use_columns": list(spec.use_columns) or None, "dtypes": spec.dtypes,
                "skip_whitespace_tail_rows": spec.skip_whitespace_tail_rows, "whitespace_as_null": spec.whitespace_as_null,
            }
            block = reader.load_table(spec.table, **kwargs) if spec.table is not None else reader.load_sheet(spec.sheet, **kwargs)
            batch, errors = block.to_arrow_with_errors()
            return pa.Table.from_batches([batch]).replace_schema_metadata({
                b"excel.sheet": block.name.encode(), b"excel.cell_errors": str(0 if errors is None else len(errors)).encode(),
                b"excel.total_height": str(block.total_height).encode(), b"excel.visible": str(block.visible).encode(),
            })
        case ScanPlan(tag="corpus", corpus=(rows,)):
            return pa.Table.from_pylist(list(rows))
        case unreachable:
            assert_never(unreachable)


def _scan_lazy(pl: Any, kind: DatasetKind, source: str) -> Any:
    reader = kind.scan_reader
    if reader is None:
        raise ValueError(f"{kind.value} carries no lazy scan reader")
    return getattr(pl, reader)(source)


def _pushed(pl: Any, lf: Any, projection: tuple[str, ...], predicate: str) -> Any:
    lf = lf.select(list(projection)) if projection else lf
    return lf.filter(pl.sql_expr(predicate)) if predicate else lf


def _io_source(dataset: DatasetRef, source: str) -> Callable[[list[str] | None, Any, int | None, int | None], Iterator[pa.RecordBatch]]:
    def generator(
        with_columns: list[str] | None, predicate: Any, n_rows: int | None, batch_size: int | None,
    ) -> Iterator[pa.RecordBatch]:
        import polars as pl  # noqa: PLC0415

        lf = _scan_lazy(pl, dataset.kind, source)
        lf = lf.select(with_columns) if with_columns else lf
        lf = lf.filter(predicate) if predicate is not None else lf
        lf = lf.head(n_rows) if n_rows is not None else lf
        yield from lf.collect(engine="streaming").to_arrow().to_batches(max_chunksize=batch_size or 65536)

    return generator
```

## [04]-[MATERIALIZE]

- Owner: `DerivedSnapshot` — the one incremental CDC-materialization owner folding the `lakehouse` change feed, the `query` engine, and `ContentIdentity` into a partition-delta recompute; `PartitionBundle` the per-partition content-keyed Arrow bundle. The derived view composes the `lakehouse#LAKEHOUSE` `Lakehouse` owner for the source `table_uri` identity and reads the Change Data Feed between two snapshot versions through the same `deltalake.load_cdf` surface the `lakehouse` `ChangeFeed` op binds, derives the changed-partition set from the CDF `_commit_version` range, routes only the changed rows through the `tabular/query#QUERY` `QueryEngine`, and re-keys only the touched partition bundles — an unchanged partition's content-key is reused untouched. A full re-scan is the deleted form.
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

from rasm.data.tabular.lakehouse import Lakehouse, TableFormat
from rasm.data.tabular.query import QueryEngine, QuerySpec
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
        self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]
    ) -> "RuntimeRail[tuple[PartitionBundle, ...]]":
        return boundary("derived.refresh", lambda: self._materialize(source, start, end, prior))

    def _materialize(self, source: Lakehouse, start: int, end: int | None, prior: tuple[PartitionBundle, ...]) -> tuple[PartitionBundle, ...]:
        if source.table_format is not TableFormat.DELTA:
            raise ValueError(f"change-feed materialization is Delta-exclusive; {source.table_format.value} carries no load_cdf")
        cdf = DeltaTable(source.table_uri).load_cdf(starting_version=start, ending_version=end).read_all()
        touched = cdf.filter(pc.field("_change_type") != "delete")
        key_col = self.partition_by[0]
        changed = pc.unique(touched.column(key_col)).to_pylist()
        prior_by_key = {b.partition: b for b in prior}
        recomputed = {str(value): self._recompute(touched.filter(pc.equal(touched.column(key_col), value)), str(value)) for value in changed}
        return tuple(recomputed.get(b.partition, b) for b in prior) + tuple(recomputed[k] for k in recomputed if k not in prior_by_key)

    def _recompute(self, delta: pa.Table, partition: str) -> PartitionBundle:
        result = QueryEngine.of({"delta": delta}).run(self.transform).default_value(delta)
        return PartitionBundle(
            partition=partition,
            rows=result.num_rows,
            content_key=ContentIdentity.of("partition", result.to_batches()[0].serialize() if result.num_rows else b""),
        )


def snapshot_key(bundles: tuple[PartitionBundle, ...]) -> ContentKey:
    return ContentIdentity.of("derived-snapshot", tuple(b.content_key for b in bundles))
```

## [05]-[RESEARCH]

- [POLARS_IO_SOURCE]: the `polars` lazy IO surface the `IoSource` arm transcribes is split between confirmed and open. The `scan_csv`/`scan_parquet`/`scan_ipc`/`scan_ndjson`/`scan_delta`/`scan_iceberg` lazy readers the `DatasetKind.scan_reader` column names, `LazyFrame.select`/`filter`/`collect(engine="streaming")`/`collect_schema`, and the `DataFrame.to_arrow`/`pa.Table.to_batches(max_chunksize=)` chunking the generator yields are catalogue-confirmed against the folder `polars` `.api` (`scan_csv`/`scan_parquet`/`scan_ipc` lazy-IO row [09], `scan_ndjson`/`scan_delta`/`scan_iceberg` row [10], `select`/`filter`/`collect`/`collect_schema` execution rows, `to_arrow` interop row). Two open seams: `polars.io.plugins.register_io_source(io_source=, schema=)` with the four-argument generator contract `(with_columns, predicate, n_rows, batch_size) -> Iterator[pa.RecordBatch]` the `_io_source` plugin binds — the plugin-registration entrypoint and its pushdown-callback signature are not enumerated as a row on the folder `polars` `.api`; and `polars.sql_expr(predicate) -> Expr` the `_pushed` fold lifts the SQL predicate string into the lazy filter — `sql_expr` is not a `.api` row (the SQL surface enumerates `sql(query)`/`SQLContext` only), so both `register_io_source` and `sql_expr` confirm against the live distribution before the pushdown treats them as settled, leaving `IoSource`, `_io_source`, and the `_pushed` SQL-string predicate lift marked RESEARCH items while the `DatasetKind.scan_reader` reader column and the `LazyFrame` projection/collection chain stay confirmed. The `PolarsLazy` and `IoSource` arms over one `DatasetRef` collapse onto the shared `DatasetKind.scan_reader` reader so the plugin-pushed and direct-lazy receipts agree; the distributed runner stays the `query#QUERY` daft case off this owner's axis; the dropped `RHINO_3DM`/`MESH`/`HDF` rows route to `spatial/mesh` and `gridded/field` through the settled `spatial/mesh ⇄ python:geometry` Arrow seam, no member contradicting a sibling item.
- [DUCKDB_WINDOW_EXPRESSION]: the `Window` arm replaces the prior f-string `project("*, row_number() OVER (...) AS ...")` SQL interpolation with the expression-node form `DuckDBPyRelation.select(duckdb.StarExpression(), *window_nodes)` where each `WindowFunction.expression` emits `duckdb.SQLExpression(f"{verb}({args}) OVER (PARTITION BY ... ORDER BY ...)").alias(alias)`. `duckdb.SQLExpression` and `duckdb.StarExpression` are catalogue-confirmed against the folder `duckdb` `.api` `Expression` row [02] (the nine expression builders `ColumnExpression`/`ConstantExpression`/`FunctionExpression`/`CaseExpression`/`LambdaExpression`/`SQLExpression`/`StarExpression`/`DefaultExpression`/`CoalesceOperator`), so the expression-node projection over `parse`-free `SQLExpression` fragments is settled over an f-string `project()` interpolation. Two open seams: `duckdb.Expression.alias(name) -> Expression` the per-verb window node aliases through and the `DuckDBPyRelation.select(*expressions: Expression)` overload accepting `Expression` objects positionally (the `.api` lists `DuckDBPyRelation.select`/`project` as transforms and `Expression` as a node type but enumerates neither the `Expression.alias` member nor the `Expression`-argument `select` form as a row), plus the `duckdb.read_parquet(file_glob, hive_partitioning=)` keyword the `RemoteGlob` arm toggles off `partition_keys` presence (the `.api` `read_parquet(file_glob, ...)` row [file readers] names the reader but not the `hive_partitioning` kwarg), so the `Expression.alias`/`select(*Expression)`/`hive_partitioning` members confirm against the live distribution before the window expression-node projection and the Hive-partition discovery treat them as settled, leaving the `Window` `select(*projection)` node-build and the `RemoteGlob` `hive_partitioning` toggle marked RESEARCH items while the `SQLExpression`/`StarExpression` builders, `read_parquet(file_glob)`, `DuckDBPyRelation.filter`/`to_arrow_table`, and the `sqlglot.parse_one(...).find_all(exp.Where, exp.Having, exp.Qualify, exp.Join)` predicate count (settled against the `query#QUERY` `_PREDICATE_NODES` surface) stay confirmed. No member contradicts a sibling RESEARCH item — the `_PREDICATE_NODES` node classes match the `query` owner's catalogue-confirmed `sqlglot` traversal.
- [FASTEXCEL_INGEST]: the `fastexcel` `read_excel(source)`/`ExcelReader.load_sheet(idx_or_name, header_row=, column_names=, skip_rows=, n_rows=, schema_sample_rows=, dtype_coercion=, use_columns=, dtypes=, skip_whitespace_tail_rows=, whitespace_as_null=, eager=)`/`load_table(name, ...)`/`ExcelSheet.to_arrow_with_errors() -> tuple[pa.RecordBatch, CellErrors | None]`/`ExcelSheet.{name,total_height,visible,selected_columns}` surface the `Excel` arm transcribes is catalogue-confirmed against the folder `fastexcel` `.api` (the `read_excel` open row [03][01], the polymorphic `load_sheet`/`load_table` rows [03][01]/[03][05] with the full lazy-`ExcelSheet` kwargs axis, `to_arrow_with_errors` row [03/ExcelSheet][02], the `name`/`total_height`/`visible`/`selected_columns` property rows [08]/[11]/[14]/[12]). `load_sheet` versus `load_table` is the confirmed sheet-versus-named-table distinction over one kernel, the `ExcelSpec` `Struct` collapses the 7-positional case tuple into named fields so a new decode knob is one field, the `dtypes` `DTypeMap` and `dtype_coercion="coerce"` are the confirmed per-column type override and mixed-column policy, `use_columns` the confirmed name/index/letter/predicate selection, the lazy `ExcelSheet`+`to_arrow_with_errors()` the confirmed per-cell-error-capturing decode, and `pa.Table.from_batches([batch])` the stdlib-Arrow one-batch lift — `fastexcel` is a `DatasetKind.EXCEL`-plus-capsule producer, never a `ScanPlan` engine backend. The one open seam is the evidence stamp: `pyarrow.Table.replace_schema_metadata(mapping)` (not enumerated on the folder `pyarrow` `.api`, which lists `cast`/`combine_chunks` but no metadata-replace row) and the `CellErrors` count accessor (`len(errors)` over the `CellErrors | None` second tuple element — the `.api` names `CellErrors` as a collection but enumerates no length or iteration member), so both confirm against the live distribution before the schema-metadata decode-evidence stamp treats them as settled, leaving the `Excel` evidence-stamp a marked RESEARCH item while the whole `read_excel`/`load_sheet`/`load_table`/`to_arrow_with_errors`/property decode surface stays confirmed.
- [CORPUS_WIRE]: the `Corpus` arm is the data-side endpoint of the `tabular ← python:artifacts/documents [WIRE]` seam (`data/ARCHITECTURE.md` L39), consuming the artifacts-owned `to_corpus_row` flat record at the wire — a sequence of `Mapping[str, Any]` rows — and lifting it through the catalogue-confirmed `pyarrow` `Table.from_pylist` row [03][06], then carrying it zero-copy through the settled `interop#INTEROP` `ArrowCStream` PyCapsule carrier. The `pa.Table.from_pylist` lift and the `ArrowCStream` carrier are settled fence code; the one open seam is the exact `to_corpus_row` column shape (the `DocumentNode`/`TableNode`/symbol field names and dtypes the artifacts `INGEST`/`OFFICE_INGEST`/`LENS_TABLE_ARM` cards emit), which is owned by the `python:artifacts/documents` producing endpoint and confirms against that folder's authored counterpart before the `from_pylist` schema treats the column names as settled — the `Corpus` arm never re-mints the document recovery, and the content-key consumption is a separate runtime ripple, not conflated with this WIRE seam, leaving the `to_corpus_row` column shape a marked RESEARCH item bound to the artifacts pass.
- [CDF_PARTITION_GROUP]: the `deltalake` `DeltaTable.load_cdf(starting_version=, ending_version=)` reader and its `_change_type`/`_commit_version` CDF columns the `_materialize` partition-grouping reads are catalogue-confirmed against the folder `deltalake` `.api`; the `_materialize` arm guards `source.table_format is TableFormat.DELTA` before binding `load_cdf` (the `lakehouse#LAKEHOUSE` `_PORTABLE` row proves `changefeed` is the Delta-exclusive tag, so an Iceberg/Lance source raises rather than calling an absent `load_cdf`), filters the raw CDF to the touched rows through `cdf.filter(pc.field("_change_type") != "delete")` so a deleted partition never re-keys as changed (the prose-promised `_change_type`-filtered changed set, never the raw-CDF over-recompute), and groups the touched frame by the `partition_by[0]` key. The `pyarrow.compute` `field`/`unique`/`equal` and the `Table.filter(Expression)` selection are catalogue-confirmed against the folder `pyarrow` `.api` (`compute.field(name)` row [04], `filter(mask)` row [02], `unique`/`equal` compute kernels), and the `RecordBatch.serialize` content-key source is a stdlib-Arrow member, so the one open seam is the `QueryEngine.run` rail unwrap (`Result.default_value`) the `_recompute` uses to lower the delta-query rail into the partition bundle — the unwrap reuses the prior delta on a query fault rather than propagating, an intentional reuse-on-empty-delta policy the refresh boundary owns. The `DerivedSnapshot` composes the `Lakehouse` owner for the `table_uri` identity and reads the CDF through the one `load_cdf` surface the `lakehouse` `ChangeFeed` op binds, never a second CDF differ.
