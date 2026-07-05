# [PY_DATA_COLUMNAR]

The dataset-reference identity owner and the folder's scan base: one polymorphic owner discriminating by source shape, with the cross-engine lazy/streaming scan, the request-scoped DuckDB session rail, the typed columnar egress, and the content-keyed query receipt. `DatasetRef` is the one dataset owner; `DatasetKind` the closed `StrEnum` discriminating the admitted columnar source shapes, the geometry/HDF rows routed to their real owners. `DuckDbSession` is the ONE request-scoped DuckDB session owner for the folder — connect plus a `DuckDbExtension` policy-row table (`httpfs`/`spatial`/`h3`/`substrait`/`iceberg`/`ducklake` as seed data, install-repository a row property) plus `register_filesystem` threading — composed downward by `tabular/query`, `tabular/materialize`, `spatial/query`, and `tabular/lakehouse`, so the connect-install-load lifecycle is authored once and every consumer supplies extension rows, never a hand-rolled `duckdb.connect()`-plus-install site. `ScanPlan` is the engine/projection/predicate/partition policy with cases for the Polars lazy plan, the `register_io_source` pushdown plugin, the source-scoped DuckDB SQL-over-ref, the PyArrow dataset scanner, the remote glob, the analytical window projection, the `fastexcel` spreadsheet decode, and the artifacts corpus-row wire — `ScanPlan` sources refs, globs, and wire rows; SQL naming its own sources is `tabular/query#QUERY` `QuerySpec.Sql`'s concern, the stated scan/query boundary. `ColumnarEgress` is the typed Arrow/Parquet/IPC export folding one `QueryReceipt` over scan plus transform plus egress; `arrow_bytes` is the PUBLIC canonical whole-table serialization fold every content key over a table payload rides. This page imports nothing from `rasm.data` — it is the pure scan base above `interop` alone. Every receipt is wired through runtime `ReceiptContributor` and keyed by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[DATASET]: the dataset-ref owner discriminating by source shape.
- [02]-[SCAN]: the DuckDB session rail, engine scan plans, columnar egress, the public `arrow_bytes` fold, the content-keyed query receipt.

## [02]-[DATASET]

- Owner: `DatasetRef` — the one polymorphic dataset owner; `DatasetKind` the closed `StrEnum` discriminating the columnar-plane source shapes the scan owner reads end-to-end. The geometry/HDF source shapes leave the axis: `.3dm` reading is a `geometry` concern reaching the columnar plane only through the settled `spatial/mesh ⇄ python:geometry` Arrow point-record seam, the unstructured-mesh file row lands in `spatial/mesh`, and the chunked HDF field row lands in `gridded/field` — `DatasetKind` carries no eager row that has no `ScanPlan` arm.
- Cases: `DatasetKind` rows `CSV` · `PARQUET` · `ARROW_IPC` · `ARROW_DATASET` · `NDJSON` · `DELTA` · `ICEBERG` · `EXCEL`, matched by `match`/`case`, never a Get/List/Scan family — every row reaches the plane through a real arm, never a dead row whose only behavior is rejection. Each lazy-scannable row carries its `polars` reader on the `scan_reader` behavior column (the `POLICY_VALUES` law — the vocabulary owns its reader, so a `PolarsLazy`/`IoSource` plan resolves `kind.scan_reader` rather than a module-private dict covering a silent subset); `ARROW_DATASET` and `EXCEL` carry `scan_reader is None` because each is read by a dedicated non-polars arm (`ScanPlan.ArrowDataset` over the PyArrow dataset scanner, `ScanPlan.Excel` over the `fastexcel` calamine decode), so the lazy `PolarsLazy`/`IoSource` arms reject an `ARROW_DATASET`/`EXCEL` `DatasetRef` at the named `scan.polars` boundary that converts the explicit reader-absence error to a `BoundaryFault`, never a silent `KeyError`. `RHINO_3DM`/`MESH`/`HDF` and a `pandas`/`polars` native-pickle file are the dropped shapes whose reads route to their real owners (`spatial/mesh`, `gridded/field`, or a caller's own deserializer), never a `DatasetKind` row carrying no arm.
- Entry: `DatasetRef.of` admits a `ResourceRef` and a `DatasetKind` and returns the frozen owner; the kind is recoverable from the source shape, never a knob.
- Packages: `polars`, `pyarrow`, `fastexcel` (the calamine xlsx/xlsm/xlsb/xls/ods reader the `Excel` arm decodes through the lazy `ExcelSheet`+`to_arrow_with_errors` sheet path and the lazy `ExcelTable`+`to_arrow` table path on this pyarrow-resident plane, the `__arrow_c_array__` PyCapsule the pyarrow-free interop carrier rides elsewhere; a producer, never a `ScanPlan` engine backend), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `DatasetRef.of` factory so a non-`ResourceRef`/non-`DatasetKind` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the caller's enclosing fence, the shared `FAULT_CONF` the sibling data admission seams bind), runtime (`ResourceRef`/`ContentIdentity`/`FAULT_CONF` the shared beartype violation-redirect config).
- Growth: a new columnar source shape is one `DatasetKind` row plus its `_SCAN_READER` reader entry when lazy-scannable (absent when the kind reaches the plane through a dedicated `ScanPlan` arm); a spreadsheet source is the `EXCEL` row the `Excel` arm already reads; a geometry/raster source is a row on its real owner's axis, never re-admitted here; zero new surface.
- Boundary: no product identity, repository, or host-document mutation; a `get_csv`/`read_parquet`/`load_delta`/`read_excel` method family is the deleted form; `fastexcel` is a `DatasetKind`-plus-capsule-ingest producer, never a `ScanPlan` engine backend row; a `RHINO_3DM`/`MESH`/`HDF` row with no scan arm forcing an eager read inside the columnar plane is the deleted form, the geometry read routing through the existing `spatial/mesh ⇄ python:geometry` seam rather than a new `columnar → geometry` edge; an undecorated `DatasetRef.of` admitting a caller `ResourceRef`/`DatasetKind` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling data admission factories share is the deleted form.

```python signature
from enum import StrEnum
from typing import Final

from beartype import beartype
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.faults import FAULT_CONF
from rasm.runtime.roots import ResourceRef

_SCAN_READER: Final[Map[str, str]] = Map.of_seq([
    ("csv", "scan_csv"),
    ("parquet", "scan_parquet"),
    ("arrow-ipc", "scan_ipc"),
    ("ndjson", "scan_ndjson"),
    ("delta", "scan_delta"),
    ("iceberg", "scan_iceberg"),
])


class DatasetKind(StrEnum):
    CSV = "csv"
    PARQUET = "parquet"
    ARROW_IPC = "arrow-ipc"
    ARROW_DATASET = "arrow-dataset"
    NDJSON = "ndjson"
    DELTA = "delta"
    ICEBERG = "iceberg"
    EXCEL = "excel"

    @property
    def scan_reader(self) -> str | None:
        return _SCAN_READER.get(self.value)


class DatasetRef(Struct, frozen=True):
    ref: ResourceRef
    kind: DatasetKind

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, ref: ResourceRef, kind: DatasetKind) -> "DatasetRef":
        return cls(ref=ref, kind=kind)
```

## [03]-[SCAN]

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `WindowFunction` the analytical window-verb row carrying its `DuckDBPyRelation` window spelling; `ExcelSpec` the named decode-policy `Struct` the `Excel` case carries (the windowing, dtype, selection, and whitespace axes as one frozen field set); `ColumnarEgress` the typed Arrow/Parquet/IPC export; `QueryReceipt` the one typed receipt fold over scan plus transform plus egress, carrying the optional column-level `lineage_edges` projection the `tabular/query#QUERY` `QueryEngine` populates from the `sqlglot.lineage.lineage` column-provenance engine over the qualified SQL (and `ibis.to_sql` over the bound expression) and the scan path leaves empty. The eager-reader cases (`PolarsLazy` plus the geometry/HDF rows that owned no arm) collapse into the lazy `IoSource` pushdown plane and the two wire-ingest arms, so every `ScanPlan` case terminates in the same `RuntimeRail[pa.Table]` over the Arrow C Data Interface.
- Cases: `ScanPlan` cases `PolarsLazy(projection, predicate)` (Polars `LazyFrame`/`scan_*`/`collect`) · `IoSource(projection, predicate)` (the `polars.io.plugins.register_io_source` lazy plugin lifting a Rasm `DatasetRef` into a `LazyFrame` with predicate/projection pushdown, the generator callable yielding polars `DataFrame` windows — the plugin contract's `Iterator[DataFrame]`, never `pa.RecordBatch` — the plugin folds into the lazy graph) · `DuckDb(sql, projection)` (source-scoped DuckDB SQL-over-ref: the session binds the admitted `DatasetRef` as the one `source` relation view keyed by `DatasetKind` through the `_DUCK_READER` row, and the SQL names `source` alone — free-form SQL naming its own sources is `query#QUERY` `QuerySpec.Sql`'s concern, the stated scan/query boundary) · `ArrowDataset(predicate, columns)` (PyArrow `dataset.dataset(source).scanner(columns=, filter=).to_table()`, the predicate a pre-built `pyarrow.dataset.Expression` policy value the body never re-parses from a string) · `RemoteGlob(glob, predicate, partition_keys)` (DuckDB `read_parquet(file_glob)` over one `DuckDbSession` carrying the `HTTPFS` extension row and the `ResourceRef.path.fs` filesystem, Hive-partition-pruned by the predicate) · `Window(partitions, order, functions)` (the `DuckDBPyRelation.read_parquet(...).select(StarExpression(), *window_exprs)` analytical window-function projection over the `(WindowFunction, alias, args)` verb-row triples — each triple emitting one `duckdb.SQLExpression(...).alias(...)` window node whose `args` carries the verb's call arguments so `lag(col, 1)`/`ntile(4)`/`first_value(col)` reach the OVER node the no-arg `row_number()`/`rank()` rows leave empty — the parquet path and the projection both expression-parameterized, never an f-string `project(...)` interpolation) · `Excel(spec)` (the `fastexcel` calamine reader materializing one sheet or named table under the named `ExcelSpec` field set — the sheet path through the lazy `ExcelSheet`+`to_arrow_with_errors` error-capturing decode, the table path through the lazy `ExcelTable`+`to_arrow` export since `ExcelTable` carries no error-capture member — each path's own evidence stamped onto the one-batch `pa.Table`) · `Corpus(rows)` (the artifacts `documents/model#NODE` `to_corpus_record` flat-`dict` records — `Mapping[str, Any]` rows, the producer's `msgspec.to_builtins` lowering of its typed `CorpusRow` Struct, never the raw Struct `from_pylist` rejects — lifted into a `pa.Table` through `pa.Table.from_pylist` on this pyarrow-resident plane, the resulting Arrow table riding the agnostic `interop#INTEROP` `FrameInterop.c_stream` carrier zero-copy at the downstream hop, the data-side endpoint of the `tabular ← python:artifacts/documents [WIRE]` seam), matched by `match`/`case` closed by `assert_never`, each binding the engine or wire that owns it.
- Entry: `execute(plan, dataset)` runs the plan and returns a `RuntimeRail[pa.Table]` over the Arrow C Data Interface (zero-copy) for the egress hop; `scan(plan, dataset)` binds the same materialization into a `RuntimeRail[tuple[pa.Table, QueryReceipt]]`, threading the railed `QueryReceipt.railed(plan.tag, source, table, predicate_count=plan.predicate_count)` so the scan-only path carries the same receipt the egress path stamps, never a second receipt rail; `ColumnarEgress.write(table, predicate_count=)` emits Arrow/Parquet/IPC then binds `QueryReceipt.railed`, threading the plan's predicate count. `QueryReceipt.railed` derives the content key off the canonical Arrow bytes through the railed `ContentIdentity.of` and `.map`s the resolved `ContentKey` into the receipt — `QueryReceipt.of` is the plain factory taking the already-resolved key, never collapsing a `RuntimeRail[ContentKey]` into the `content_key` field. `ScanPlan.predicate_count` is the one derived projection over the case axis — the polars/io-source predicate-string presence, the arrow-dataset `Expression` presence, the remote-glob predicate presence, and the DuckDB arm calling the exported `predicate_count(sql)` fold (the one `parse_one().find_all(*_PREDICATE_NODES)` application this lower owner homes beside the `_PREDICATE_NODES` widening it declares, the module-level `predicate_count` function `query#QUERY` `_provenance` imports and calls rather than re-spelling the byte-identical fold — the shared correspondence is the FOLD, not only the node tuple) so the scan and query receipts count predicates identically off one fold, never a hardcoded `0` and never a re-declared `find_all` application. The `IoSource` arm builds its `LazyFrame` through one `register_io_source` registration whose generator reads the same `DatasetKind.scan_reader` the `PolarsLazy` arm reads, so the plugin-pushed and the direct-lazy scan over one `DatasetRef` fold the byte-identical `QueryReceipt` off the shared reader; the distributed out-of-core runner is the `query#QUERY` `QuerySpec.Streaming` daft case, not a columnar scan arm — `daft` never enters this owner's axis.
- Auto: the Polars path selects the lazy reader off `dataset.kind` through the `DatasetKind.scan_reader` reader column (the `scan_csv`/`scan_parquet`/`scan_ipc`/`scan_ndjson`/`scan_delta`/`scan_iceberg` confirmed rows, and the dedicated-arm `ARROW_DATASET`/`EXCEL` kinds raising the explicit reader-absence error the `execute` boundary converts to a `BoundaryFault` rather than a silent `KeyError`), then runs `.select(projection).filter(predicate).collect(engine="streaming")` — the reader is the dataset-kind row, never a hardcoded format and never a module-private dict covering a silent subset; the `IoSource` path registers one `register_io_source` plugin whose `(with_columns, predicate, n_rows, batch_size)` callback receives the polars-pushed projection and the polars `Expr` predicate and forwards them into the same `DatasetKind.scan_reader` so pushdown crosses the plugin boundary, the generator yielding `frame.slice`-windowed polars `DataFrame` chunks (the `Iterator[DataFrame]` the plugin folds, never `pa.RecordBatch`) and the outer `collect(engine="streaming").to_arrow()` materializing the assembled lazy graph — the generator is the dataset-kind row, never a per-format plugin; the DuckDB path opens one `DuckDbSession` (no extension rows), binds the admitted ref as the `source` relation view through the `_DUCK_READER` `DatasetKind`-keyed row (`read_parquet`/`read_csv`/`read_json`), and runs the source-scoped SQL over that view — the arm binds the admitted source by construction, never a free-form `con.sql(sql)` over sources the SQL names itself; the PyArrow path runs `dataset(source).scanner(columns, filter).to_table()` with a pre-built `Expression` predicate (`pyarrow.dataset` `compute.field`/`compute.scalar` pushdown nodes); the `RemoteGlob` path opens one `DuckDbSession(extensions=(DuckDbExtension.HTTPFS,), filesystem=dataset.ref.path.fs)` — the session loads the deduplicated extension union once per connection and `register_filesystem` threads the `fsspec` `AbstractFileSystem` the `ResourceRef.path` `UPath` exposes off its scheme-resolved `.fs` accessor (the `s3`/`gcs` filesystem the runtime `roots#RESOURCE` owner's `UPath(protocol=)` resolution configures) so the `s3`/`gcs` glob authenticates through the one runtime-owned filesystem, never a second credential owner and never the `http`/`ssh` `TransportResource` — then `read_parquet(glob, hive_partitioning=bool(partition_keys))` against the remote glob (the `partition_keys` presence toggling DuckDB Hive-partition-column discovery so the partition columns project into the relation) with the predicate pushed into the relational filter so only the partition files the predicate touches are pulled; the `Window` path builds the `row_number`/`rank`/`lag`/`first_value`/… projection from the `(WindowFunction, alias, args)` triples over the partition/order spec as one `(duckdb.StarExpression(), *window_nodes)` `.select(*projection)` where each `WindowFunction.expression(alias, partitions, order, args=args)` emits a `duckdb.SQLExpression` OVER node aliased through `.alias` — the `args` column carrying the verb's call arguments (`lag(col, 1)`/`ntile(4)`) so an argument-bearing verb is reachable while the no-arg `row_number()`/`rank()` rows pass `""`, never ten enumerated arms and never an f-string `project("*, ...")` interpolation; the `Excel` path opens `fastexcel.read_excel(source)` once, routes `sheet`-versus-`table` through the one polymorphic `load_sheet`/`load_table` surface (`load_sheet` when `table` is absent, `load_table` when a named table is named — never a per-sheet `load_by_a`/`load_by_b` family), threads the whole `ExcelSpec` field set — `header_row`/`column_names`/`skip_rows`/`n_rows`/`schema_sample_rows`/`use_columns`/`dtypes`/`dtype_coercion`/`skip_whitespace_tail_rows`/`whitespace_as_null` — as one named kwargs plan both load members read (the 7-positional tuple collapsed into the `ExcelSpec` `Struct` so a new decode knob is one field, never a tuple-arity break), decodes the sheet path through the lazy `ExcelSheet` and `to_arrow_with_errors()` so the per-cell `CellErrors` are captured rather than silently nulled, decodes the table path through the lazy `ExcelTable` and `to_arrow()` (the `to_arrow_with_errors`/`visible` members are `ExcelSheet`-only, so the table path omits the error and visibility keys rather than faking them), and lands the one-batch `pa.Table` with the shared `total_height` plus the path-shaped evidence — sheet `name`/`visible`/cell-error count, or table `name`/`sheet_name`/`offset` — stamped as Arrow schema metadata so the decode evidence rides the uniform return into the receipt — `use_columns`/`dtypes` prune and type at decode, never a post-load projection or cast pass; the `Corpus` path lifts the artifacts-owned `to_corpus_record` flat-`dict` records (a sequence of `Mapping[str, Any]` arriving at the wire — the producer's `msgspec.to_builtins` lowering of its `CorpusRow` Struct, never the Struct itself which `from_pylist` rejects with `AttributeError`, never re-derived from the document recovery) into a `pa.Table` through `pa.Table.from_pylist`, terminating in the same uniform `pa.Table` every arm returns so the zero-copy hop downstream rides the `interop#INTEROP` `FrameInterop.c_stream` carrier rather than a second Arrow path. A connection-sourced remote read is the `query#QUERY` `QuerySpec.Remote` ADBC/ConnectorX/Flight SQL axis, never a `ScanPlan` arm — this owner sources files, globs, and the two wire-ingest rows, never a database connection. `fastexcel` is on the manifest `banned-module-level-imports` row and `polars` binds function-local by this page's lazy-boundary law (the heavy frame engine loads only on the arm that runs), so the polars arms bind `pl` and `read_excel` binds inside the Excel arm under `# noqa: PLC0415`; the `DatasetKind.scan_reader` reader column resolves the bound `pl` member by name (`getattr(pl, kind.scan_reader)`) rather than capturing an unbound module-top handle. `engine="streaming"` is the streaming spelling, never the `collect(streaming=True)` flag. Every DuckDB touch rides one `DuckDbSession.connect()` bracket — request-scoped by law, never a module global, the extension policy rows loaded once per connection through `install_extension(value, repository=row)`-then-`load_extension` so a consumer names WHAT it needs (`HTTPFS`/`SPATIAL`/`H3`/`SUBSTRAIT`/`ICEBERG`/`DUCKLAKE` rows) and never HOW to load it.
- Receipt: the scan contributes an emitted-phase `Receipt.of(owner, ("emitted", subject, facts))` row through `ReceiptContributor` (the two-argument owner-plus-evidence factory the receipts owner declares, the `(Phase, subject, facts)` triple routing the `fact` case — never a four-positional call) and produces a `QueryReceipt` keyed by `ContentIdentity` over the canonical Arrow bytes the `arrow_bytes` whole-table serialization folds, never a generic receipt and never the `engine:source` string a content-change cannot move; the `scan` entrypoint threads `QueryReceipt.railed(..., predicate_count=plan.predicate_count)` so every arm carries its real predicate count — the polars/io-source/remote-glob predicate-string presence and the DuckDB arm's `predicate_count(sql)` call into the one exported `find_all(*_PREDICATE_NODES)` fold this owner homes and `query#QUERY` shares — never the hardcoded `0`; the `IoSource` and `PolarsLazy` arms over one `DatasetRef` produce the byte-identical receipt off the shared `DatasetKind.scan_reader` reader and identical Arrow output, and the `Excel`/`Corpus` wire arms key by `ContentIdentity` over the decoded Arrow bytes so a re-ingest of an unchanged workbook or corpus reuses its key untouched; the `Excel` arm carries its path-shaped decode evidence — the sheet path its `CellErrors` count and `name`/`visible`, the table path its `name`/`sheet_name`/`offset`, both the shared `total_height` — as Arrow schema metadata so the decode evidence rides the uniform `pa.Table` into the receipt rather than vanishing at the bare return.
- Packages: `polars` (`scan_csv`/`scan_parquet`/`scan_ipc`/`scan_ndjson`/`scan_delta`/`scan_iceberg`/`LazyFrame.select`/`filter`/`collect(engine=)`/`collect_schema`/`sql_expr`/`io.plugins.register_io_source`), `duckdb` (`connect`/Relational API/`read_parquet(file_glob, hive_partitioning=)`/`read_csv`/`read_json`/`DuckDBPyRelation.select`/`project`/`filter`/`to_arrow_table`/`install_extension(extension, repository=)`/`load_extension`/`register_filesystem`/`register` — the `DuckDbSession` owner authors the connect-install-register lifecycle ONCE with the `DuckDbExtension` rows as seed data, `httpfs`/`spatial`/`iceberg`/`ducklake` core-repository installs and `h3`/`substrait` community-repository installs, the repository a row property; plus the `SQLExpression`/`StarExpression`/`Expression.alias` window-node builders carrying the `row_number`/`rank`/`dense_rank`/`lag`/`lead`/`first_value`/`last_value`/`n_tile`/`cume_dist`/`percent_rank` verb spelling), `pyarrow` (`dataset.dataset(...).scanner(columns=, filter=).to_table()`/`Table`/`Table.from_pylist`/`Table.from_batches`/`Table.replace_schema_metadata`/`RecordBatch`/`compute.field`/`compute.scalar`/`Table.join(right, keys, join_type="left outer", ..., filter_expression=)` — the last the left-outer hash join the `graph/graph#GRAPH` `GraphResult.frame` node-index-keyed `pa.Table` enriches a node-attribute table through by the stable `node` key, the data-side endpoint of the `tabular/columnar ← graph/graph [WIRE]` seam), `sqlglot` (`parse_one`/`find_all(*_PREDICATE_NODES)` — the `_PREDICATE_NODES` `(exp.Where, exp.Having, exp.Qualify, exp.Join)` widening and the `predicate_count(text)` fold over it this owner declares and exports, the DuckDB SQL arm and `query#QUERY` `_provenance` both calling the one exported fold rather than re-spelling the `parse_one().find_all()` application), `fastexcel` (`read_excel`/`ExcelReader.{load_sheet,load_table,table_names,sheet_names}`/`ExcelSheet.{to_arrow,to_arrow_with_errors,__arrow_c_array__,name,total_height,visible,selected_columns}`/`ExcelTable.{to_arrow,name,sheet_name,offset,total_height}` — the table block exposes neither `to_arrow_with_errors` nor `visible`, so the table path is the plain `to_arrow` export/`CellErrors`), `tabular/interop` (`ArrowCStream`/`FrameInterop` — the agnostic carrier the `Corpus` arm rides), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `execute`/`scan` entrypoints so a non-`ScanPlan`/non-`DatasetRef` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the enclosing `boundary` fence rather than an untyped admission, the shared `FAULT_CONF` the sibling data admission seams bind; the `QueryReceipt.of`/`railed` projection and the `ScanPlan`/`ColumnarEgress` case staticmethods over already-admitted values carry no decorator), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`/`FAULT_CONF` the shared beartype violation-redirect config/`ResourceRef` — the `RemoteGlob` arm registers the `ResourceRef.path.fs` `fsspec` filesystem the runtime `roots#RESOURCE` owner resolves).
- Growth: a new engine is one `ScanPlan` case; a new remote source is one `ScanPlan` case; a new DuckDB extension is one `DuckDbExtension` row (repository a row property) every downstream session consumer names for free; a new lazy-pushdown source is one `DatasetKind.scan_reader` row the polars arms and the `IoSource` plugin already forward; a new DuckDB-readable ref kind is one `_DUCK_READER` row; a new window verb is one `WindowFunction` row; a new spreadsheet decode knob is one `ExcelSpec` field; a new corpus wire field is one column the artifacts producer adds and `from_pylist` already folds; a new egress format is one `ColumnarEgress` branch; zero new surface.
- Boundary: no durable query rails, no global DuckDB connection; a hand-rolled `duckdb.connect()`-plus-`install_extension`/`load_extension` site anywhere in the folder where the one `DuckDbSession` owner authors the lifecycle and consumers supply extension rows, a free-form `con.sql(sql)` scan arm binding no admitted source where the `DuckDb` case binds the ref's `source` view and `QuerySpec.Sql` owns self-sourced SQL, a generic receipt abstraction, a per-engine egress class family, a `scan_remote`/`scan_glob`/`window_rank`/`read_excel`/`ingest_corpus` method family, a second SQL engine or second transport owner, a `fastexcel` `ScanPlan` backend row (it is a `DatasetKind`-plus-capsule producer, never an engine), a re-minted document-recovery decoder where the `Corpus` arm consumes the artifacts `to_corpus_record` flat-`dict` record at the wire, a `pa.Table.from_pylist` over the raw `CorpusRow` `msgspec.Struct` where the producer's `to_corpus_record` `to_builtins` mapping is the `from_pylist`-ingestible shape (a `Struct` exposes no `.keys()` and `from_pylist` raises), a second Arrow path where the `interop#INTEROP` carrier owns the zero-copy hop, a per-format polars IO plugin where one `register_io_source` reads `dataset.kind`, an eager geometry/HDF read forced inside the columnar plane, a local graph node-table owner or a `graph`-named `ScanPlan` arm where the `graph/graph#GRAPH` `GraphResult.frame` produces the node-index-keyed `pa.Table` and the existing `pyarrow` `Table.join` left-joins it as enrichment, a pre-loaded-httpfs assumption, and an undecorated `execute`/`scan` admitting a caller `ScanPlan`/`DatasetRef` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling data admission entrypoints share are the deleted forms.

```python signature
import duckdb
import pyarrow as pa
import pyarrow.dataset as pads
import pyarrow.feather as paf
import pyarrow.parquet as papq
import sqlglot
from collections.abc import Buffer, Callable, Iterable, Iterator, Mapping
from contextlib import contextmanager
from enum import StrEnum
from sqlglot import exp
from types import ModuleType
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # `polars` binds function-local by the lazy-boundary law — the heavy frame engine loads only
    # on the arm that runs; the type-only import keeps `LazyFrame`/`Expr`/the handle precise.
    import polars as pl
    from polars import Expr, LazyFrame

type CorpusRow = Mapping[str, Any]
# the `fastexcel` per-column dtype vocabulary mapped at this edge; `dtypes` admits the closed
# `DType` set the calamine decoder honors, never a free-string dtype the reader rejects late.
type ExcelDType = Literal["null", "int", "float", "string", "boolean", "datetime", "date", "duration"]

# the predicate-bearing node widening this lower owner declares once; `tabular/query#QUERY`
# imports it so the scan-receipt and query-receipt predicate counts read identical node classes.
_PREDICATE_NODES: Final[tuple[type[exp.Expression], ...]] = (exp.Where, exp.Having, exp.Qualify, exp.Join)


def predicate_count(text: str) -> int:
    # the one predicate-counting FOLD this lower owner exports beside the `_PREDICATE_NODES` widening
    # it already owns: one variadic `find_all(*_PREDICATE_NODES)` over the parsed SQL. `tabular/query#QUERY`
    # `_provenance` imports and calls it rather than re-spelling the byte-identical `parse_one().find_all()`
    # application, so the scan-receipt and query-receipt counts share the fold AND the node set (the
    # DERIVED_LOGIC/ONE_HOP correspondence is the application, not only the constant) — the `ScanPlan`
    # `duckdb` arm calls it too, so the property, the query provenance, and the wire all read one source.
    return len(tuple(sqlglot.parse_one(text).find_all(*_PREDICATE_NODES)))


class DuckDbExtension(StrEnum):
    HTTPFS = "httpfs"
    SPATIAL = "spatial"
    H3 = "h3"
    SUBSTRAIT = "substrait"
    ICEBERG = "iceberg"
    DUCKLAKE = "ducklake"

    @property
    def repository(self) -> str | None:
        # the install source is a ROW property: `h3`/`substrait` ride the community repository,
        # the rest install from core — a consumer names the row, never the load mechanics.
        return "community" if self in _COMMUNITY else None

    def load(self, con: duckdb.DuckDBPyConnection) -> None:
        con.install_extension(self.value, repository=self.repository)
        con.load_extension(self.value)


_COMMUNITY: Final[frozenset[DuckDbExtension]] = frozenset({DuckDbExtension.H3, DuckDbExtension.SUBSTRAIT})

# the DuckDB reader per lazily-bindable `DatasetKind` — the `DuckDb` scan arm binds the admitted
# ref as the one `source` view through this row, so the SQL is source-scoped by construction.
_DUCK_READER: Final[Map[str, str]] = Map.of_seq([("parquet", "read_parquet"), ("csv", "read_csv"), ("ndjson", "read_json")])


class DuckDbSession(Struct, frozen=True):
    # the ONE request-scoped DuckDB session owner for the folder: `query`, `spatial/query`, and
    # `lakehouse` compose it downward with their own extension rows (the ibis-owned `_ir_plan`
    # connection loads its row via `DuckDbExtension.load`); `materialize` rides it transitively.
    extensions: tuple[DuckDbExtension, ...] = ()
    filesystem: Any | None = None

    @contextmanager
    def connect(self) -> Iterator[duckdb.DuckDBPyConnection]:
        # request-scoped by law — one bracket per run, released on every exit, never a module
        # global; the deduplicated extension union loads once per connection and the runtime-
        # resolved `fsspec` handle registers so cloud reads authenticate through one owner.
        with duckdb.connect() as con:
            for extension in dict.fromkeys(self.extensions):
                extension.load(con)
            if self.filesystem is not None:
                con.register_filesystem(self.filesystem)
            yield con


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

    def expression(self, alias: str, partitions: tuple[str, ...], order: tuple[str, ...], *, args: str = "") -> duckdb.Expression:
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
    dtypes: Map[str | int, ExcelDType] | None = None
    dtype_coercion: Literal["coerce", "strict"] = "coerce"
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
    window: tuple[tuple[str, ...], tuple[str, ...], tuple[tuple[WindowFunction, str, str], ...]] = case()
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
    def Window(partitions: tuple[str, ...], order: tuple[str, ...], functions: tuple[tuple[WindowFunction, str, str], ...]) -> "ScanPlan":
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
                return predicate_count(sql)
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
        return boundary(f"egress.{self.tag}", lambda: self._emit(table)).bind(
            lambda target: QueryReceipt.railed(self.tag, target, table, predicate_count=predicate_count)
        )

    def _emit(self, table: pa.Table) -> str:
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
        return target


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
        cls,
        engine: str,
        source: str,
        table: pa.Table,
        content_key: ContentKey,
        *,
        predicate_count: int = 0,
        lineage_edges: tuple[tuple[str, str], ...] = (),
    ) -> "QueryReceipt":
        return cls(
            engine=engine,
            source=source,
            columns=table.num_columns,
            predicate_count=predicate_count,
            row_count=table.num_rows,
            content_key=content_key,
            lineage_edges=lineage_edges,
        )

    @classmethod
    def railed(
        cls, engine: str, source: str, table: pa.Table, *, predicate_count: int = 0, lineage_edges: tuple[tuple[str, str], ...] = ()
    ) -> "RuntimeRail[QueryReceipt]":
        # content identity over the canonical Arrow bytes, never the `engine:source` string —
        # an unchanged table re-egressed reuses its key, and the railed `ContentIdentity.of`
        # threads through `.map` rather than collapsing a `RuntimeRail[ContentKey]` into a field.
        return ContentIdentity.of("query", arrow_bytes(table)).map(
            lambda key: cls.of(engine, source, table, key, predicate_count=predicate_count, lineage_edges=lineage_edges)
        )

    def contribute(self) -> Iterable[Receipt]:
        return (Receipt.of("query", ("emitted", self.source, {"rows": self.row_count, "lineage": len(self.lineage_edges)})),)


@beartype(conf=FAULT_CONF)
def execute(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[pa.Table]":
    return boundary(f"scan.{plan.tag}", lambda: _run(plan, dataset))


@beartype(conf=FAULT_CONF)
def scan(plan: ScanPlan, dataset: DatasetRef) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
    return execute(plan, dataset).bind(
        lambda table: QueryReceipt.railed(plan.tag, str(dataset.ref.path), table, predicate_count=plan.predicate_count).map(
            lambda receipt: (table, receipt)
        )
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

            lf = pl.io.plugins.register_io_source(io_source=_io_source(dataset, source), schema=_scan_lazy(pl, dataset.kind, source).collect_schema())
            return _pushed(pl, lf, projection, predicate).collect(engine="streaming").to_arrow()
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            # source-scoped SQL-over-ref: the session binds the admitted ref as the one `source`
            # view keyed by `DatasetKind`, so the SQL never names its own sources (that is
            # `QuerySpec.Sql`'s concern); the bracket releases the connection on every exit.
            reader = _DUCK_READER.get(dataset.kind.value)
            if reader is None:
                raise ValueError(f"{dataset.kind.value} carries no DuckDB relation reader")
            with DuckDbSession().connect() as con:
                getattr(con, reader)(source).create_view("source")
                rel = con.sql(sql)
                return (rel.project(", ".join(projection)) if projection else rel).to_arrow_table()
        case ScanPlan(tag="arrow_dataset", arrow_dataset=(predicate, columns)):
            return pads.dataset(source).scanner(columns=list(columns) or None, filter=predicate).to_table()
        case ScanPlan(tag="remote_glob", remote_glob=(glob, predicate, partition_keys)):
            with DuckDbSession(extensions=(DuckDbExtension.HTTPFS,), filesystem=dataset.ref.path.fs).connect() as con:
                rel = con.read_parquet(glob, hive_partitioning=bool(partition_keys))
                return (rel.filter(predicate) if predicate else rel).to_arrow_table()
        case ScanPlan(tag="window", window=(partitions, order, functions)):
            projection = (duckdb.StarExpression(), *(verb.expression(alias, partitions, order, args=args) for verb, alias, args in functions))
            with DuckDbSession().connect() as con:
                return con.read_parquet(source).select(*projection).to_arrow_table()
        case ScanPlan(tag="excel", excel=spec):
            import fastexcel  # noqa: PLC0415

            reader = fastexcel.read_excel(source)
            kwargs = {
                "header_row": spec.header_row,
                "column_names": list(spec.column_names) or None,
                "skip_rows": spec.skip_rows,
                "n_rows": spec.n_rows,
                "schema_sample_rows": spec.schema_sample_rows,
                "dtype_coercion": spec.dtype_coercion,
                "use_columns": list(spec.use_columns) or None,
                "dtypes": dict(spec.dtypes.items()) if spec.dtypes is not None else None,
                "skip_whitespace_tail_rows": spec.skip_whitespace_tail_rows,
                "whitespace_as_null": spec.whitespace_as_null,
            }
            # the decode evidence is CAPABILITY-SHAPED: `to_arrow_with_errors`/`visible` are
            # `ExcelSheet`-only (ExcelTable exposes neither), so the table path exports through
            # `to_arrow()` and stamps its own `sheet_name`/`offset` evidence instead.
            if spec.table is not None:
                block = reader.load_table(spec.table, **kwargs)
                batch = block.to_arrow()
                evidence = {b"excel.table": block.name.encode(), b"excel.sheet": block.sheet_name.encode(), b"excel.offset": str(block.offset).encode()}
            else:
                block = reader.load_sheet(spec.sheet, **kwargs)
                batch, errors = block.to_arrow_with_errors()
                evidence = {
                    b"excel.sheet": block.name.encode(),
                    b"excel.cell_errors": str(0 if errors is None else len(errors.errors)).encode(),
                    b"excel.visible": str(block.visible).encode(),
                }
            return pa.Table.from_batches([batch]).replace_schema_metadata({b"excel.total_height": str(block.total_height).encode(), **evidence})
        case ScanPlan(tag="corpus", corpus=(rows,)):
            # `rows` are the artifacts `documents/model#NODE` `to_corpus_record` flat `dict` mappings
            # (its `msgspec.to_builtins` lowering of the typed `CorpusRow` Struct), so `from_pylist`
            # reads `.keys()` per row — a raw `CorpusRow` Struct here raises `AttributeError`.
            return pa.Table.from_pylist(list(rows))
        case unreachable:
            assert_never(unreachable)


def _scan_lazy(pl: "ModuleType", kind: DatasetKind, source: str) -> "LazyFrame":
    reader = kind.scan_reader
    if reader is None:
        raise ValueError(f"{kind.value} carries no lazy scan reader")
    return getattr(pl, reader)(source)


def _pushed(pl: "ModuleType", lf: "LazyFrame", projection: tuple[str, ...], predicate: str) -> "LazyFrame":
    lf = lf.select(list(projection)) if projection else lf
    return lf.filter(pl.sql_expr(predicate)) if predicate else lf


def _io_source(dataset: DatasetRef, source: str) -> "Callable[[list[str] | None, Expr | None, int | None, int | None], Iterator[pl.DataFrame]]":
    # `register_io_source` contract: the generator yields polars `DataFrame` windows (NOT
    # `pa.RecordBatch`), `predicate` arrives as a polars `Expr`, and projection/predicate/row-cap
    # push through `with_columns`/`filter`/`head` so the plugin folds a pushed-down lazy scan.
    def generator(with_columns: list[str] | None, predicate: "Expr | None", n_rows: int | None, batch_size: int | None) -> "Iterator[pl.DataFrame]":
        import polars as pl  # noqa: PLC0415

        lf = _scan_lazy(pl, dataset.kind, source)
        lf = lf.select(with_columns) if with_columns else lf
        lf = lf.filter(predicate) if predicate is not None else lf
        lf = lf.head(n_rows) if n_rows is not None else lf
        frame = lf.collect(engine="streaming")
        step = batch_size or 65536
        yield from (frame.slice(offset, step) for offset in range(0, max(frame.height, 1), step))

    return generator


def arrow_bytes(table: pa.Table) -> Buffer:
    # canonical whole-table bytes — the ONE serialization derivation: `combine_chunks` coalesces
    # every column so the single `RecordBatch` serializes to a buffer-protocol `pa.Buffer` the
    # `ContentIdentity` `whole` arm folds; an empty table keys off `b""`, its stable degenerate.
    # PUBLIC branch-consumer export: geometry energy/simulate keys its result frames through it at
    # the consumer edge and impact/impact composes the Assessment crossing over it — never narrowed.
    batches = table.combine_chunks().to_batches()
    return batches[0].serialize() if batches else b""
```

