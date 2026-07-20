# [PY_DATA_COLUMNAR]

The dataset-reference identity owner and the folder's scan base: one polymorphic `DatasetRef` discriminating by source shape, the cross-engine lazy/streaming scan, the request-scoped DuckDB session rail, the typed columnar egress, and the content-keyed query receipt. The pure base of the tabular plane — it imports nothing from `rasm.data`, sits above `interop` alone, and holds zero back-edges, so every folder composition edge points strictly down into it.

`DuckDbSession` authors the connect-install-load lifecycle once and is composed downward by `tabular/query`, `tabular/materialize`, `spatial/query`, and `tabular/lakehouse`, each supplying its own `DuckDbExtension` rows rather than a hand-rolled `duckdb.connect()`-plus-install site; the same session owns the engine-profiling harvest — DuckDB exposes no scrape surface, so the profiled bracket IS the engine's observability, folded onto the one `QueryReceipt` stream and projected onto the runtime metric spine as `domain="query"` measures. `ScanPlan` sources refs, globs, and wire rows; SQL naming its own sources is `tabular/query#QUERY` `QuerySpec.Sql`'s concern — the standing scan/query boundary. The `predicate_count` fold and its `_PREDICATE_NODES` widening are declared here and imported by `query#QUERY` `_provenance`, so scan and query receipts count predicates off one source. `arrow_bytes` is the PUBLIC whole-table serialization fold every content key over a table payload rides. Two wire seams cross the edge as data endpoints: `tabular ← python:artifacts/documents` (the `Corpus` arm over `to_corpus_record` records) and `tabular/columnar ← graph/graph` (the `pyarrow` `Table.join` left-outer enrichment of a `GraphResult.frame` node table). Every receipt wires through runtime `ReceiptContributor` and keys by runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[DATASET]: the `DatasetRef` identity owner discriminating the columnar source shapes.
- [02]-[SCAN]: the DuckDB session rail, the engine scan plans, the typed egress, the public `arrow_bytes` fold, and the content-keyed query receipt.

## [02]-[DATASET]

- Owner: `DatasetRef` — the one polymorphic dataset owner; `DatasetKind` the closed `StrEnum` over the columnar-plane source shapes the scan owner reads end-to-end. The geometry/HDF shapes leave the axis by ownership ruling: `.3dm` reading is a `geometry` concern reaching the columnar plane only through the settled `spatial/mesh ⇄ python:geometry` Arrow point-record seam, the unstructured-mesh file lands in `spatial/mesh`, and the chunked HDF field lands in `gridded/field`. `DatasetKind` carries no row without a live `ScanPlan` arm.
- Cases: matched by `match`/`case`, never a `Get`/`List`/`Scan` family. `ARROW_DATASET` and `EXCEL` carry `scan_reader is None` because each is read by a dedicated non-polars arm (`ScanPlan.ArrowDataset` over the PyArrow scanner, `ScanPlan.Excel` over the `fastexcel` calamine decode), so the lazy `PolarsLazy`/`IoSource` arms reject those refs at the `scan.polars` boundary that converts the explicit reader-absence to a `BoundaryFault`, never a silent `KeyError`. Each lazy-scannable row carries its `polars` reader on the `scan_reader` behavior column so a plan resolves `kind.scan_reader` rather than a module-private dict over a silent subset.
- Entry: `DatasetRef.of` admits a `ResourceRef` and a `DatasetKind` and returns the frozen owner; the kind is recoverable from the source shape, never a knob.
- Packages: `polars`, `pyarrow`, `fastexcel` (the calamine reader the `Excel` arm decodes through — a `DatasetKind`-plus-capsule producer, never a `ScanPlan` engine backend), `beartype` (`@beartype(conf=FAULT_CONF)` the public admission contract on `DatasetRef.of` so a bad argument raises the `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail, the shared `FAULT_CONF` the sibling data admission seams bind), runtime (`ResourceRef`/`ContentIdentity`/`FAULT_CONF`).
- Growth: a new columnar source shape is one `DatasetKind` row plus its `_SCAN_READER` entry when lazy-scannable (absent when a dedicated `ScanPlan` arm reads it); a geometry/raster source is a row on its real owner's axis, never re-admitted here; zero new surface.
- Boundary: no product identity, repository, or host-document mutation; a `get_csv`/`read_parquet`/`load_delta`/`read_excel` method family is the deleted form; a `RHINO_3DM`/`MESH`/`HDF` `DatasetKind` row forcing an eager in-plane read is the deleted form.

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

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `WindowFunction` the analytical window-verb row carrying its OVER-node spelling; `ExcelSpec` the named decode-policy `Struct` the `Excel` case carries; `ColumnarEgress` the typed Arrow/Parquet/IPC export; `QueryReceipt` the one receipt fold over scan plus transform plus egress, carrying the optional column-level `lineage_edges` the `tabular/query#QUERY` `QueryEngine` populates (`sqlglot.lineage` over the qualified SQL, `ibis.to_sql` over the bound expression) and the scan path leaves empty, plus the optional `EngineProfile` the profiled bracket harvests. Every case terminates in the same `RuntimeRail[pa.Table]` over the Arrow C Data Interface.
- Cases: closed by `assert_never`, each binding the engine or wire that owns it. `ArrowDataset` takes a pre-built `pyarrow.dataset.Expression` predicate the body never re-parses from a string. `IoSource` lifts a `DatasetRef` into a `LazyFrame` through `register_io_source` reading the same `DatasetKind.scan_reader` the `PolarsLazy` arm reads, so the plugin-pushed and direct-lazy scans over one ref fold the byte-identical receipt. The distributed out-of-core runner is the `query#QUERY` `QuerySpec.Streaming` daft case, never a scan arm; a connection-sourced remote read is `QuerySpec.Remote`, never a scan arm — this owner sources files, globs, and the two wire-ingest rows, never a database connection.
- Entry: `execute(plan, dataset)` returns `RuntimeRail[pa.Table]` over the Arrow C Data Interface for the egress hop; `scan(plan, dataset)` binds the same materialization into `RuntimeRail[tuple[pa.Table, QueryReceipt]]`, threading `QueryReceipt.railed(..., predicate_count=plan.predicate_count)` so the scan-only path carries the egress path's receipt. `ScanPlan.predicate_count` is the one derived projection over the case axis — the `DuckDb` arm calling the exported `predicate_count(sql)` fold `query#QUERY` `_provenance` shares, so scan and query count predicates identically, never a hardcoded `0`. `QueryReceipt.railed` derives the content key off the canonical Arrow bytes through the railed `ContentIdentity.of` and `.map`s the resolved key into the receipt; `QueryReceipt.of` is the plain factory taking the already-resolved key.
- Auto: the polars path selects its lazy reader off `dataset.kind` through `DatasetKind.scan_reader`, then runs `.select().filter().collect(engine="streaming")` — `engine="streaming"` the streaming spelling, never the `collect(streaming=True)` flag. The `DuckDb` arm binds the admitted ref as the one `source` view through the `_DUCK_READER` `DatasetKind`-keyed row, so the SQL is source-scoped by construction. `RemoteGlob` opens `DuckDbSession(extensions=(DuckDbExtension.HTTPFS,), filesystem=dataset.ref.path.fs)` and `register_filesystem` threads the runtime-resolved `fsspec` handle so `s3`/`gcs` globs authenticate through the one runtime `roots#RESOURCE` filesystem, never a second credential owner and never the `http`/`ssh` `TransportResource`. `fastexcel` is on the manifest `banned-module-level-imports` row and `polars` binds function-local by this page's lazy-boundary law, so `scan_reader` resolves the bound `pl` member by name (`getattr(pl, kind.scan_reader)`) rather than an unbound module-top handle. Every DuckDB touch rides one request-scoped `DuckDbSession.profiled()` bracket, the extension policy rows loaded once per connection so a consumer names WHAT it needs, never HOW to load it; a `ProfileMode` beyond `OFF` arms the engine's own JSON profiling — `SET enable_profiling='json'` and `SET profiling_mode`, then `con.get_profiling_information()` returns the last query's structured JSON string in-process — and the harvest thunk folds the lowercase profile keys (`cpu_time`/`latency`/`rows_returned`/`result_set_size`/`blocked_thread_time`/`total_bytes_read`/`total_bytes_written` plus the per-operator `operator_type`/`operator_timing`/`operator_cardinality` tree) through `ProfileHarvest.Duckdb` into one `EngineProfile` the table carries as `rasm.query.profile` schema metadata, so the profile rides the uniform `pa.Table` exactly as the Excel decode evidence does and no temp-file sink races the request-scoped bracket. Engine-profile parity holds beyond DuckDB: the polars arms swap `collect(engine="streaming")` for `LazyFrame.profile()` under an armed mode and fold the `(node, start, end)` microsecond timing frame through `ProfileHarvest.Polars`, the `query#QUERY` `datafusion` arm folds the wall-latency execution scalars its python `ExecutionPlan` exposes with no per-operator metric surface through `ProfileHarvest.Datafusion`, and the `daft` arm folds those scalars plus its materialized `DataFrame.metrics` per-operator rows through `ProfileHarvest.Daft` — `EngineProfile.of` the ONE polymorphic entrypoint discriminating the harvest value shape, so every engine lands the identical band with per-engine adapters at the arm and never a parallel receipt field.
- Receipt: the scan contributes an emitted-phase `Receipt.of(owner, ("emitted", subject, facts))` row through `ReceiptContributor` (the two-argument owner-plus-evidence factory, the `(Phase, subject, facts)` triple, never a four-positional call) and produces a `QueryReceipt` keyed by `ContentIdentity` over the canonical `arrow_bytes`, never the `engine:source` string a content change cannot move. The `Excel`/`Corpus` wire arms key over their decoded Arrow bytes so a re-ingest of an unchanged workbook or corpus reuses its key, and the `Excel` arm stamps its path-shaped decode evidence as Arrow schema metadata so it rides the uniform `pa.Table` into the receipt rather than vanishing at the bare return. Receipts stay truth and instruments stay projections: a profile-bearing `contribute` records the engine latency and row count onto the runtime `Metrics.record` mapping arm under `domain="query"`, keyed by the engine tag, and an unprofiled receipt records nothing.
- Packages: `polars`, `duckdb`, `pyarrow`, `sqlglot`, `fastexcel`, `tabular/interop` (the `ArrowCStream`/`FrameInterop` carrier the `Corpus` arm rides zero-copy), `beartype` (`@beartype(conf=FAULT_CONF)` on the `execute`/`scan` entrypoints; the `QueryReceipt`/`ScanPlan`/`ColumnarEgress` staticmethods over already-admitted values carry no decorator), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`/`FAULT_CONF`/`ResourceRef`, and `Metrics.record` as the one instrument projection port — the DBAPI span train (`PsycopgInstrumentor`/`SQLite3Instrumentor`) activates at the runtime composition root, never at data altitude). The `DuckDbSession` owner authors the connect-install-register lifecycle once with the `DuckDbExtension` rows as seed data. The `pyarrow` `Table.join` left-outer join is the data-side endpoint of the `tabular/columnar ← graph/graph [WIRE]` seam, enriching a `GraphResult.frame` node-index-keyed table by the stable `node` key.
- Growth: a new engine is one `ScanPlan` case; a new DuckDB extension is one `DuckDbExtension` row (repository a row property) every session consumer names for free; a new lazy-pushdown source is one `DatasetKind.scan_reader` row the polars arms and the `IoSource` plugin already forward; a new DuckDB-readable ref kind is one `_DUCK_READER` row; a new window verb is one `WindowFunction` row; a new decode knob is one `ExcelSpec` field; a new corpus wire field is one column `from_pylist` already folds; a new egress format is one `ColumnarEgress` branch; a new profiling engine is one `ProfileHarvest` case plus its per-arm adapter classmethod folding into the shared `EngineProfile`; a new harvested profile fact is one `EngineProfile` field the band decoder already ignores-or-fills; a new query instrument one measure name in `contribute` and one `InstrumentSpec` row on the runtime metrics owner; zero new surface.
- Boundary: no durable query rails, no global DuckDB connection, the `DuckDbSession` bracket request-scoped by law; a free-form `con.sql(sql)` scan arm binding no admitted source where the `DuckDb` case binds the ref's `source` view and `QuerySpec.Sql` owns self-sourced SQL; a `scan_remote`/`scan_glob`/`window_rank`/`read_excel`/`ingest_corpus` method family, a generic receipt abstraction, a per-engine egress class family, a second SQL engine or second transport owner are the deleted forms; a `fastexcel` `ScanPlan` backend row where it is a `DatasetKind`-plus-capsule producer; a per-format polars IO plugin where one `register_io_source` reads `dataset.kind`; a local graph node-table owner or a `graph`-named `ScanPlan` arm where the `graph/graph#GRAPH` `GraphResult.frame` node table left-joins through the existing `pyarrow` `Table.join`; a pre-loaded-httpfs assumption; and an undecorated `execute`/`scan` admitting a caller argument without the `@beartype(conf=FAULT_CONF)` public-seam contract.

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
from msgspec.json import Decoder as JsonDecoder
from msgspec.json import encode as json_encode

from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # `polars` binds function-local by the lazy-boundary law; the type-only import keeps `LazyFrame`/`Expr` precise.
    import polars as pl
    from polars import Expr, LazyFrame

type CorpusRow = Mapping[str, Any]
# the closed `fastexcel` per-column dtype vocabulary the calamine decoder honors, never a free-string dtype it rejects late.
type ExcelDType = Literal["null", "int", "float", "string", "boolean", "datetime", "date", "duration"]

# the predicate-bearing node widening `tabular/query#QUERY` imports so scan-receipt and query-receipt counts read identical node classes.
_PREDICATE_NODES: Final[tuple[type[exp.Expression], ...]] = (exp.Where, exp.Having, exp.Qualify, exp.Join)


def predicate_count(text: str) -> int:
    # the one predicate-counting fold this owner exports beside `_PREDICATE_NODES`; `query#QUERY` `_provenance` and the `duckdb` arm both call it.
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
        # the install source is a ROW property: `h3`/`substrait` ride the community repository, the rest core.
        return "community" if self in _COMMUNITY else None

    def load(self, con: duckdb.DuckDBPyConnection) -> None:
        con.install_extension(self.value, repository=self.repository)
        con.load_extension(self.value)


_COMMUNITY: Final[frozenset[DuckDbExtension]] = frozenset({DuckDbExtension.H3, DuckDbExtension.SUBSTRAIT})

# the DuckDB reader per lazily-bindable `DatasetKind`; the `DuckDb` arm binds the admitted ref as the one `source` view.
_DUCK_READER: Final[Map[str, str]] = Map.of_seq([("parquet", "read_parquet"), ("csv", "read_csv"), ("ndjson", "read_json")])


class ProfileMode(StrEnum):
    OFF = "off"
    STANDARD = "standard"
    DETAILED = "detailed"


# decode targets over DuckDB's lowercase JSON profile keys; unknown keys drop at decode, so a DuckDB key addition costs nothing.
class _ProfileNode(Struct, frozen=True):
    operator_type: str = ""
    operator_timing: float = 0.0
    operator_cardinality: int = 0
    children: tuple["_ProfileNode", ...] = ()


class _ProfileRoot(Struct, frozen=True):
    cpu_time: float = 0.0
    latency: float = 0.0
    rows_returned: int = 0
    result_set_size: int = 0
    blocked_thread_time: float = 0.0
    total_bytes_read: int = 0
    total_bytes_written: int = 0
    children: tuple[_ProfileNode, ...] = ()


@tagged_union(frozen=True)
class ProfileHarvest:
    # the input-shape discriminant every engine's native profile arrives as, so `EngineProfile.of` is ONE polymorphic
    # entrypoint keyed by value shape, never an `of_duckdb`/`of_polars`/`of_daft` name-suffix family: `duckdb` the
    # `get_profiling_information()` JSON string, `polars` the `LazyFrame.profile()` node-timing frame, `datafusion` the
    # wall-latency execution scalars its python `ExecutionPlan` exposes with no per-operator metric surface, `daft` those
    # scalars plus the materialized `DataFrame.metrics` per-operator rows `(name, seconds, cardinality)`, `band` the
    # already-decoded `EngineProfile` msgspec-JSON riding the table's own metadata so a railed re-read round-trips.
    tag: Literal["duckdb", "polars", "datafusion", "daft", "band"] = tag()
    duckdb: str = case()
    polars: Any = case()
    datafusion: tuple[float, int, int] = case()
    daft: tuple[float, int, int, int, tuple[tuple[str, float, int], ...]] = case()
    band: bytes = case()

    @staticmethod
    def Duckdb(raw: str) -> "ProfileHarvest":
        return ProfileHarvest(duckdb=raw)

    @staticmethod
    def Polars(timings: Any) -> "ProfileHarvest":
        return ProfileHarvest(polars=timings)

    @staticmethod
    def Datafusion(latency_s: float, rows: int, nbytes: int) -> "ProfileHarvest":
        return ProfileHarvest(datafusion=(latency_s, rows, nbytes))

    @staticmethod
    def Daft(latency_s: float, rows: int, nbytes: int, partitions: int, operators: tuple[tuple[str, float, int], ...] = ()) -> "ProfileHarvest":
        return ProfileHarvest(daft=(latency_s, rows, nbytes, partitions, operators))

    @staticmethod
    def Band(stamped: bytes) -> "ProfileHarvest":
        return ProfileHarvest(band=stamped)


class EngineProfile(Struct, frozen=True):
    cpu_time_s: float
    latency_s: float
    rows_returned: int
    result_set_size: int
    blocked_thread_s: float
    bytes_read: int
    bytes_written: int
    operators: tuple[tuple[str, float, int], ...]
    partitions: int = 0

    @classmethod
    def of(cls, harvest: ProfileHarvest) -> "EngineProfile":
        # the one decoded band shape, per-engine adapters at the arm — never parallel receipt fields; each case folds its
        # engine's native profile spelling into the identical `EngineProfile`, closed by `assert_never`.
        match harvest:
            case ProfileHarvest(tag="duckdb", duckdb=raw):
                return cls._duckdb(raw)
            case ProfileHarvest(tag="polars", polars=timings):
                return cls._polars(timings)
            case ProfileHarvest(tag="datafusion", datafusion=(latency_s, rows, nbytes)):
                return cls(latency_s, latency_s, rows, nbytes, 0.0, 0, 0, ())
            case ProfileHarvest(tag="daft", daft=(latency_s, rows, nbytes, partitions, operators)):
                return cls(latency_s, latency_s, rows, nbytes, 0.0, 0, 0, operators, partitions)
            case ProfileHarvest(tag="band", band=stamped):
                return _PROFILE_BAND_DECODER.decode(stamped)
            case unreachable:
                assert_never(unreachable)

    @classmethod
    def _duckdb(cls, raw: str) -> "EngineProfile":
        # `get_profiling_information()` returns the profile as a JSON string the `_ProfileRoot` decoder reads directly,
        # unknown optimizer-timing keys dropping at decode so a DuckDB key addition costs nothing.
        root = _PROFILE_DECODER.decode(raw)

        def walked(node: _ProfileNode) -> Iterator[tuple[str, float, int]]:
            # plan trees are bounded-depth; native recursion to data depth is the lawful walk.
            yield (node.operator_type, node.operator_timing, node.operator_cardinality)
            for child in node.children:
                yield from walked(child)

        return cls(
            cpu_time_s=root.cpu_time,
            latency_s=root.latency,
            rows_returned=root.rows_returned,
            result_set_size=root.result_set_size,
            blocked_thread_s=root.blocked_thread_time,
            bytes_read=root.total_bytes_read,
            bytes_written=root.total_bytes_written,
            operators=tuple(row for child in root.children for row in walked(child)),
        )

    @classmethod
    def _polars(cls, timings: Any) -> "EngineProfile":
        # `LazyFrame.profile()` returns `(result, timings)`; the timing frame is the canonical `(node, start, end)`
        # microsecond spans — operators fold node-by-node, latency is the span envelope, cpu the summed node durations.
        rows = timings.to_dicts()
        operators = tuple((str(row["node"]), (row["end"] - row["start"]) / 1_000_000.0, 0) for row in rows)
        starts = tuple(row["start"] for row in rows)
        ends = tuple(row["end"] for row in rows)
        latency_s = (max(ends) - min(starts)) / 1_000_000.0 if rows else 0.0
        return cls(sum(dur for _, dur, _ in operators), latency_s, 0, 0, 0.0, 0, 0, operators)

    def stamp(self, table: pa.Table) -> pa.Table:
        # the profile rides the uniform table as its own metadata band — the Excel decode-evidence pattern — so every
        # engine arm returns a table-shaped result and `sibling` query arms never touch the private band key.
        return table.replace_schema_metadata({**(table.schema.metadata or {}), _PROFILE_META: json_encode(self)})

    @classmethod
    def from_table(cls, table: pa.Table) -> "EngineProfile | None":
        # the band read every railed and federated receipt shares — the stamped `EngineProfile` JSON round-trips through
        # `ProfileHarvest.Band`, never re-parsed as the DuckDB-native `_ProfileRoot`.
        stamped = (table.schema.metadata or {}).get(_PROFILE_META)
        return cls.of(ProfileHarvest.Band(stamped)) if stamped else None


_PROFILE_DECODER: Final[JsonDecoder[_ProfileRoot]] = JsonDecoder(_ProfileRoot)
# the band decoder round-trips the STAMPED `EngineProfile` (its own JSON shape), distinct from the DuckDB-native
# `_ProfileRoot` decoder — a railed re-read decodes the decoded profile, never re-parsing raw engine bytes as `_ProfileRoot`.
_PROFILE_BAND_DECODER: Final[JsonDecoder[EngineProfile]] = JsonDecoder(EngineProfile)
_PROFILE_META: Final[bytes] = b"rasm.query.profile"


class DuckDbSession(Struct, frozen=True):
    extensions: tuple[DuckDbExtension, ...] = ()
    filesystem: Any | None = None
    profiling: ProfileMode = ProfileMode.OFF

    @contextmanager
    def connect(self) -> Iterator[duckdb.DuckDBPyConnection]:
        # request-scoped by law — one bracket per run; the deduplicated extension union loads once per connection and the runtime `fsspec` handle registers.
        with duckdb.connect() as con:
            for extension in dict.fromkeys(self.extensions):
                extension.load(con)
            if self.filesystem is not None:
                con.register_filesystem(self.filesystem)
            yield con

    @contextmanager
    def profiled(self) -> Iterator[tuple[duckdb.DuckDBPyConnection, Callable[[], EngineProfile | None]]]:
        # DuckDB owns its profiling harvest in-process: `get_profiling_information()` returns the last query's structured
        # JSON as a string the harvest thunk reads AFTER the profiled query, so no temp-file sink races the bracket.
        with self.connect() as con:
            if self.profiling is not ProfileMode.OFF:
                con.execute("SET enable_profiling='json'")
                con.execute(f"SET profiling_mode='{self.profiling.value}'")

            def harvest() -> EngineProfile | None:
                return EngineProfile.of(ProfileHarvest.Duckdb(con.get_profiling_information())) if self.profiling is not ProfileMode.OFF else None

            yield con, harvest


def _profiled_table(table: pa.Table, profile: EngineProfile | None) -> pa.Table:
    # profile rides the uniform pa.Table through `EngineProfile.stamp` — the Excel decode-evidence pattern — so `_run` stays table-shaped.
    return table if profile is None else profile.stamp(table)


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
    profile: EngineProfile | None = None

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
        profile: EngineProfile | None = None,
    ) -> "QueryReceipt":
        return cls(
            engine=engine,
            source=source,
            columns=table.num_columns,
            predicate_count=predicate_count,
            row_count=table.num_rows,
            content_key=content_key,
            lineage_edges=lineage_edges,
            profile=profile,
        )

    @classmethod
    def railed(
        cls,
        engine: str,
        source: str,
        table: pa.Table,
        *,
        predicate_count: int = 0,
        lineage_edges: tuple[tuple[str, str], ...] = (),
    ) -> "RuntimeRail[QueryReceipt]":
        # content identity over the canonical Arrow bytes, never the `engine:source` string; the railed `ContentIdentity.of` threads
        # through `.map`, and the profile decodes off the table's own metadata band so every railed caller inherits the harvest.
        harvested = EngineProfile.from_table(table)
        return ContentIdentity.of("query", arrow_bytes(table)).map(
            lambda key: cls.of(engine, source, table, key, predicate_count=predicate_count, lineage_edges=lineage_edges, profile=harvested)
        )

    def contribute(self) -> Iterable[Receipt]:
        # receipts stay truth, instruments stay projections: only a profile-bearing receipt records the query measures.
        facts: dict[str, object] = {"rows": self.row_count, "lineage": len(self.lineage_edges)}
        if self.profile is not None:
            Metrics.record(
                {"rasm.query.engine.duration": self.profile.latency_s * 1000.0, "rasm.query.rows": float(self.row_count)},
                domain="query",
                kind=self.engine,
            )
            facts |= {"cpu_s": self.profile.cpu_time_s, "latency_s": self.profile.latency_s, "blocked_s": self.profile.blocked_thread_s}
        return (Receipt.of("query", ("emitted", self.source, facts)),)


@beartype(conf=FAULT_CONF)
def execute(plan: ScanPlan, dataset: DatasetRef, profiling: ProfileMode = ProfileMode.OFF) -> "RuntimeRail[pa.Table]":
    return boundary(f"scan.{plan.tag}", lambda: _run(plan, dataset, profiling))


@beartype(conf=FAULT_CONF)
def scan(plan: ScanPlan, dataset: DatasetRef, profiling: ProfileMode = ProfileMode.OFF) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
    return execute(plan, dataset, profiling).bind(
        lambda table: QueryReceipt.railed(plan.tag, str(dataset.ref.path), table, predicate_count=plan.predicate_count).map(
            lambda receipt: (table, receipt)
        )
    )


def _run(plan: ScanPlan, dataset: DatasetRef, profiling: ProfileMode = ProfileMode.OFF) -> pa.Table:
    source = str(dataset.ref.path)
    match plan:
        case ScanPlan(tag="polars_lazy", polars_lazy=(projection, predicate)):
            import polars as pl  # noqa: PLC0415

            lf = _scan_lazy(pl, dataset.kind, source)
            return _polars_collect(_pushed(pl, lf, projection, predicate), profiling)
        case ScanPlan(tag="io_source", io_source=(projection, predicate)):
            import polars as pl  # noqa: PLC0415

            lf = pl.io.plugins.register_io_source(io_source=_io_source(dataset, source), schema=_scan_lazy(pl, dataset.kind, source).collect_schema())
            return _polars_collect(_pushed(pl, lf, projection, predicate), profiling)
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            # source-scoped by construction: the session binds the admitted ref as the one `source` view, so the SQL never self-sources.
            reader = _DUCK_READER.get(dataset.kind.value)
            if reader is None:
                raise ValueError(f"{dataset.kind.value} carries no DuckDB relation reader")
            with DuckDbSession(profiling=profiling).profiled() as (con, harvest):
                getattr(con, reader)(source).create_view("source")
                rel = con.sql(sql)
                return _profiled_table((rel.project(", ".join(projection)) if projection else rel).to_arrow_table(), harvest())
        case ScanPlan(tag="arrow_dataset", arrow_dataset=(predicate, columns)):
            return pads.dataset(source).scanner(columns=list(columns) or None, filter=predicate).to_table()
        case ScanPlan(tag="remote_glob", remote_glob=(glob, predicate, partition_keys)):
            with DuckDbSession(extensions=(DuckDbExtension.HTTPFS,), filesystem=dataset.ref.path.fs, profiling=profiling).profiled() as (con, harvest):
                rel = con.read_parquet(glob, hive_partitioning=bool(partition_keys))
                return _profiled_table((rel.filter(predicate) if predicate else rel).to_arrow_table(), harvest())
        case ScanPlan(tag="window", window=(partitions, order, functions)):
            projection = (duckdb.StarExpression(), *(verb.expression(alias, partitions, order, args=args) for verb, alias, args in functions))
            with DuckDbSession(profiling=profiling).profiled() as (con, harvest):
                return _profiled_table(con.read_parquet(source).select(*projection).to_arrow_table(), harvest())
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
            # capability-shaped decode evidence: `to_arrow_with_errors`/`visible` are `ExcelSheet`-only, so the table
            # path exports through `to_arrow()` and stamps its own `sheet_name`/`offset` instead.
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
            # the artifacts `to_corpus_record` flat-`dict` rows (its `msgspec.to_builtins` lowering of the `CorpusRow`
            # Struct), so `from_pylist` reads `.keys()` per row — a raw Struct raises `AttributeError`.
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


def _polars_collect(lf: "LazyFrame", profiling: ProfileMode) -> pa.Table:
    # OFF stays the byte-identical streaming collect; an armed mode swaps to `LazyFrame.profile()`, whose `(result, timings)`
    # node-timing frame folds through the `ProfileHarvest.Polars` adapter onto the shared band exactly as the DuckDB sink does.
    if profiling is ProfileMode.OFF:
        return lf.collect(engine="streaming").to_arrow()
    result, timings = lf.profile()
    return _profiled_table(result.to_arrow(), EngineProfile.of(ProfileHarvest.Polars(timings)))


def _io_source(dataset: DatasetRef, source: str) -> "Callable[[list[str] | None, Expr | None, int | None, int | None], Iterator[pl.DataFrame]]":
    # `register_io_source` contract: the generator yields polars `DataFrame` windows (NOT `pa.RecordBatch`), `predicate`
    # arrives as a polars `Expr`, and projection/predicate/row-cap push through `with_columns`/`filter`/`head`.
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
    # canonical whole-table bytes — the ONE serialization: `combine_chunks` coalesces every column so a single `RecordBatch`
    # serializes to the `pa.Buffer` the `ContentIdentity` `whole` arm folds; an empty table keys off `b""`. Public fold — never narrowed.
    batches = table.combine_chunks().to_batches()
    return batches[0].serialize() if batches else b""
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
