# [PY_DATA_COLUMNAR]

The dataset-reference identity owner and the folder's scan base: one polymorphic `DatasetRef` discriminating by source shape, the cross-engine lazy/streaming scan, the request-scoped DuckDB session rail, the typed columnar egress, and the content-keyed query receipt. The folder's scan base above `interop` alone — it imports that one module downward for the `arrow_bytes` serialization and holds zero back-edges, so every other folder composition edge points strictly down into it.

`DuckDbSession` authors the connect-install-attach lifecycle once and is composed downward by `tabular/query`, `tabular/materialize`, `spatial/query`, and `tabular/lakehouse`, each supplying its own `DuckDbExtension` and `Attach` rows rather than a hand-rolled `duckdb.connect()`-plus-install site; `quote_ident`/`quote_literal` are the exported quoting folds every composing owner spells its identifiers and URI literals through, so one escape rule serves the whole DuckDB plane; the same session owns the engine-profiling harvest — DuckDB exposes no scrape surface, so the profiled bracket IS the engine's observability, folded onto the one `QueryReceipt` stream and projected onto the runtime metric spine as `domain="query"` measures. `ScanPlan` sources refs, globs, and wire rows; SQL naming its own sources is `tabular/query#QUERY` `QuerySpec.Sql`'s concern — the standing scan/query boundary. The `predicate_count` fold and its `_PREDICATE_NODES` widening are declared here and imported by `tabular/query#QUERY` `_provenance`, so scan and query receipts count predicates off one source. `arrow_bytes` is the `tabular/interop#INTEROP` whole-table serialization every content key over a table payload rides, imported downward and never re-spelled; `arrow_columns` stays this owner's own admitting half — the one entry converting a producer's declared column roster and sealed arrays into that table — and `admit_evidence` is the geometry frame-family admission above it, landing subject and producer-key join columns on the scan plane. Three wire seams cross the edge as data endpoints: `tabular ← python:artifacts/documents` (the `Corpus` arm over `to_corpus_record` records), `tabular/columnar ← graph/graph` (the `pyarrow` `Table.join` left-outer enrichment of a `GraphResult.frame` node table), and `tabular/columnar ← python:geometry/graduation` (the `EvidenceFrame` and energy `ResultFrame` carriers through `arrow_columns`). Every receipt wires through runtime `ReceiptContributor` and keys by runtime `ContentIdentity`.

## [01]-[INDEX]

- [02]-[DATASET]: the `DatasetRef` identity owner discriminating the columnar source shapes.
- [03]-[SCAN]: the DuckDB session rail, the engine scan plans with their awaitable twin, the typed egress, the `arrow_columns` admitting fold, and the content-keyed query receipt.

## [02]-[DATASET]

- Owner: `DatasetRef` — the one polymorphic dataset owner; `DatasetKind` the closed `StrEnum` over the columnar-plane source shapes the scan owner reads end-to-end. The geometry/HDF shapes leave the axis by ownership ruling: `.3dm` reading is a `geometry` concern reaching the columnar plane only through the settled `spatial/mesh ⇄ python:geometry` Arrow point-record seam, the unstructured-mesh file lands in `spatial/mesh`, and the chunked HDF field lands in `gridded/field`. `DatasetKind` carries no row without a live `ScanPlan` arm.
- Cases: matched by `match`/`case`, never a `Get`/`List`/`Scan` family. `ARROW_DATASET` and `EXCEL` carry `scan_reader is None` because each is read by a dedicated non-polars arm (`ScanPlan.ArrowDataset` over the PyArrow scanner, `ScanPlan.Excel` over the `fastexcel` calamine decode), so the lazy `PolarsLazy`/`IoSource` arms reject those refs at the `scan.polars` boundary that converts the explicit reader-absence to a `BoundaryFault`, never a silent `KeyError`. Each lazy-scannable row carries its `polars` reader on the `scan_reader` behavior column so a plan resolves `kind.scan_reader` rather than a module-private dict over a silent subset.
- Entry: `DatasetRef.of` admits a `ResourceRef` and a `DatasetKind` and returns the frozen owner; the kind is recoverable from the source shape, never a knob.
- Packages: `polars`, `pyarrow`, `fastexcel` (the calamine reader the `Excel` arm decodes through — a `DatasetKind`-plus-capsule producer, never a `ScanPlan` engine backend), `beartype` (`@beartype(conf=FAULT_CONF)` the public admission contract on `DatasetRef.of` so a bad argument raises the `BeartypeCallHintViolation` root the `runtime/reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail, the shared `FAULT_CONF` the sibling data admission seams bind), runtime (`ResourceRef`/`ContentIdentity`/`FAULT_CONF`).
- Growth: a new columnar source shape is one `DatasetKind` row plus its `_SCAN_READER` entry when lazy-scannable (absent when a dedicated `ScanPlan` arm reads it); a geometry/raster source is a row on its real owner's axis, never re-admitted here; zero new surface.
- Boundary: no product identity, repository, or host-document mutation; a `get_csv`/`read_parquet`/`load_delta`/`read_excel` method family is the deleted form; a `RHINO_3DM`/`MESH`/`HDF` `DatasetKind` row forcing an eager in-plane read is the deleted form. This axis names a SOURCE SHAPE, so a residence SUBJECT is one `tabular/lakehouse#LAKEHOUSE` residence row pairing a kind with a `TableFormat` and a second kind per subject is the deleted form; `RECEIPTS` earns its row on shape alone, the residence family fixing the column roster a scan would otherwise take from the caller.

```python
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
    ("receipts", "scan_delta"),
])


class DatasetKind(StrEnum):
    CSV = "csv"
    PARQUET = "parquet"
    ARROW_IPC = "arrow-ipc"
    ARROW_DATASET = "arrow-dataset"
    NDJSON = "ndjson"
    DELTA = "delta"
    ICEBERG = "iceberg"
    RECEIPTS = "receipts"
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

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `WindowFunction` the analytical window-verb row carrying its OVER-node spelling; `ExcelSpec` the named decode-policy `Struct` the `Excel` case carries; `Attach` the attached-catalog row `DuckDbSession` folds into its own `ATTACH` statement; `SecretRow` over the closed `SecretType` vocabulary the same session issues once per connection as a `CREATE SECRET … PROVIDER credential_chain` `DdlStep`, and `remote_store` the one obstore-backed filesystem handle it registers; `ColumnarEgress` the typed export over single-file Arrow/Parquet/IPC targets and the hive-partitioned Parquet tree alike; `QueryReceipt` the one receipt fold over scan plus transform plus egress, carrying the optional column-level `lineage_edges` the `tabular/query#QUERY` `QueryEngine` populates (`sqlglot.lineage` over the qualified SQL, `ibis.to_sql` over the bound expression) and the scan path leaves empty, plus the `mode` the caller armed beside the `Option[EngineProfile]` the profiled bracket harvested, two facts because an unstamped table under an armed mode and a declared abstention are different runs; `DdlStep` is the session's ONE statement carrier over the closed `DdlVerb`/`Idempotence` columns, so every `CREATE SECRET`, `ATTACH`, and `USE` names its verb and its re-application cost instead of hiding both inside an f-string. Every case terminates in the same `RuntimeRail[pa.Table]` over the Arrow C Data Interface.
- Cases: closed by `assert_never`, each binding the engine or wire that owns it. `ArrowDataset` takes a pre-built `pyarrow.dataset.Expression` predicate the body never re-parses from a string. `IoSource` lifts a `DatasetRef` into a `LazyFrame` through `register_io_source` reading the same `DatasetKind.scan_reader` the `PolarsLazy` arm reads, so the plugin-pushed and direct-lazy scans over one ref fold the byte-identical receipt. The distributed out-of-core runner is the `tabular/query#QUERY` `QuerySpec.Streaming` daft case, never a scan arm; a connection-sourced remote read is `QuerySpec.Remote`, never a scan arm — this owner sources files, globs, and the two wire-ingest rows, never a database connection.
- Entry: `execute(plan, dataset)` returns `RuntimeRail[pa.Table]` over the Arrow C Data Interface for the egress hop; `scan(plan, dataset)` binds the same materialization into `RuntimeRail[tuple[pa.Table, QueryReceipt]]`, threading `QueryReceipt.railed(..., predicate_count=plan.predicate_count)` so the scan-only path carries the egress path's receipt. `ScanPlan.predicate_count` is the one derived projection over the case axis — the `DuckDb` arm calling the exported `predicate_count(sql)` fold `tabular/query#QUERY` `_provenance` shares, so scan and query count predicates identically, never a hardcoded `0`. `QueryReceipt.railed` derives the content key off the canonical Arrow bytes through the railed `ContentIdentity.of` and `.map`s the resolved key into the receipt; `QueryReceipt.of` is the plain factory taking the already-resolved key.
- Entry: `scan_async` is the awaitable twin over one `on_thread` band hop, splitting two entrypoints off one body exactly as the sibling `tabular/lakehouse#LAKEHOUSE` and `tabular/egress#EGRESS` owners do — a blocking native scan never runs inline on a caller's loop, and the hop stops being hand-rolled at every async consumer. It is also the one seat this owner lands durable evidence from: the profiled bracket's engine seconds cross onto the `python:runtime/observability/journal#LEDGER` plane as a `COMPUTE` `MeterFact` in the millisecond unit the live duration series already exports, and an unprofiled receipt meters nothing, exactly as it records no series. Recording suspends by law, so the synchronous `scan` and `execute` carry no producer leg and `contribute` stays the pure receipt projection.
- Entry: `ColumnarEgress` splits its write in two — `emit` lands the bytes and answers the `Landed` file-and-byte evidence, `write` composes that half with the scan-plane `QueryReceipt`. An evidence-plane commit takes `emit`, because a `QueryReceipt` minted for it enters the receipt stream as a query-domain row and prices a residence's own storage as a query.
- Auto: the polars path selects its lazy reader off `dataset.kind` through `DatasetKind.scan_reader`, then runs `.select().filter().collect(engine="streaming")` — `engine="streaming"` the streaming spelling, never the `collect(streaming=True)` flag. `ScanPlan.DuckDb` binds the admitted ref as the one `source` view through the `_DUCK_READER` `DatasetKind`-keyed row — a SQL TABLE FUNCTION beside the extension it needs, because DuckDB exposes only a few readers as connection methods and the analytics residence's own reader is `delta_scan` — so the SQL is source-scoped by construction and the evidence table answers an interactive read. `RemoteGlob` opens `DuckDbSession(extensions=(DuckDbExtension.HTTPFS, …), filesystem=remote_store(dataset.ref))` so `register_filesystem` threads an `obstore.fsspec.FsspecStore` over the SAME Rust core, `STORE_RETRY` envelope, and credential resolution every other remote read in the branch crosses — `UPath.fs` is the deleted form, resolving fsspec's own `s3fs`/`gcsfs`/`adlfs` backend as a second provider stack the estate's pinned retry config never reaches. Cold `polars`, `fastexcel`, and `obstore.fsspec` imports defer at module scope. Every engine harvests its OWN payload through the case that carries it — DuckDB the profiling JSON its connection answers, polars its node-timing frame, Daft its `DataFrame.metrics` rows — and `ProfileHarvest.scalar` stays the portable floor an engine publishing no payload rides; each case opens on the UNINSTRUMENTED floor for the facts its engine measures nowhere and overrides only what it really answers, so aliasing latency onto cpu is unspellable rather than merely discouraged. The band splits absence in two: an engine that publishes no such instrument at all carries `Posture(defaulted=(0, <engine>))` — a permanent property, PROVEN — while a key DuckDB's `custom_profiling_settings` narrowed away carries `Posture(absent=None)`, an unknown the `withheld` census names. A withheld measure is never a proven zero, which is exactly what the deleted `root.get(key, 0)` fallback made it.
- Law: `admit_evidence` is the geometry frame-family admission row over the standing seam — the wire-carried subject discriminant and the PRODUCER's `ContentKey` land as lead columns on the admitted table, so cross-model deviation trends, quality rollups, and section-property queries join scan- and lake-side frames on `content_key` with zero receipt parsing, and the receipt subject spells `{subject}:{key}` under the same `domain="query"` projection every scan row rides. The subject stays an opaque wire literal here: data dispatches on no member, so a mirrored `GeometrySubject` roster would be an unread copy the geometry owner already censuses — a new frame family admits with zero edits on this page.
- Receipt: a measure the engine WITHHELD contributes no fact and no metric point — `libs/.planning/RULINGS.md` `[02]` rules an unmeasured instrument reads UNMEASURED, never zero — and the `withheld` census rides beside the stated ones so a reader sees WHICH axes the engine declined rather than inferring it from a fact that is simply not there. The scan contributes an emitted-phase `Receipt.of(owner, ("emitted", subject, facts))` row through `ReceiptContributor` (the two-argument owner-plus-evidence factory, the `(Phase, subject, facts)` triple, never a four-positional call) and produces a `QueryReceipt` keyed by `ContentIdentity` over the canonical `arrow_bytes`, never the `engine:source` string a content change cannot move. The `Excel`/`Corpus` wire arms key over their decoded Arrow bytes so a re-ingest of an unchanged workbook or corpus reuses its key, and the `Excel` arm stamps its path-shaped decode evidence as Arrow schema metadata so it rides the uniform `pa.Table` into the receipt rather than vanishing at the bare return. Receipts stay truth and instruments stay projections: a profile-bearing `contribute` records the engine latency and row count onto the runtime `Metrics.record` mapping arm under `domain="query"`, keyed by the engine tag, and an unprofiled receipt records nothing.
- Packages: `polars`, `duckdb`, `pyarrow`, `sqlglot`, `fastexcel`, `obstore` (`fsspec.FsspecStore` — the sanctioned filesystem-interface bridge over the branch object-store core, reached exactly where DuckDB demands a filesystem handle and never as a second IO owner), `tabular/interop` (`arrow_bytes`, the folder's one whole-table IPC serialization this owner keys every receipt through and never re-spells), `beartype` (`@beartype(conf=FAULT_CONF)` on the `execute`/`scan` entrypoints; the `QueryReceipt`/`ScanPlan`/`ColumnarEgress` staticmethods over already-admitted values carry no decorator), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`/`FAULT_CONF`/`ResourceRef`/`async_boundary`/`on_thread`, `Journal.record` as the durable evidence port and `Metrics.record` as the one instrument projection port — the DBAPI span train (`PsycopgInstrumentor`/`SQLite3Instrumentor`) activates at the runtime composition root, never at data altitude). The `DuckDbSession` owner authors the connect-install-register lifecycle once with the `DuckDbExtension` rows as seed data. The `pyarrow` `Table.join` left-outer join is the data-side endpoint of the `tabular/columnar ← graph/graph [WIRE]` seam, enriching a `GraphResult.frame` node-index-keyed table by the stable `node` key.
- Growth: a new engine is one `ScanPlan` case; a new DuckDB extension is one `DuckDbExtension` row (repository and attach type both row properties) every session consumer names for free; a new attachable catalog is one `_ATTACH_TYPE` row its `Attach` value then spells for free; a new object-store scheme is one `_SCHEME_EXTENSION` row every remote scan reads; a new in-engine credential type is one `SecretType` member with its `_SECRET_EXTENSION` row, the session's extension union and its DDL both deriving; a bespoke object-store credential provider is the existing `remote_store` argument, never a second store construction; a new lazy-pushdown source is one `DatasetKind.scan_reader` row the polars arms and the `IoSource` plugin already forward; a new DuckDB-readable ref kind is one `_DUCK_READER` row naming its table function beside the extension that function needs; a new window verb is one `WindowFunction` row; a new decode knob is one `ExcelSpec` field; a new corpus wire field is one column `from_pylist` already folds; a new egress format is one `ColumnarEgress` branch answering its own `Landed` evidence; a new native profile shape is one `ProfileHarvest` case; a new harvested profile fact is one `Posture`-shaped `EngineProfile` field, its band projection and its withheld census both deriving off the declaration; a new session statement kind is one `DdlVerb` member and one `DdlStep` its owner returns; a new query instrument is one measure name in `contribute` and one `InstrumentSpec` row on the runtime metrics owner.
- Boundary: no durable query rails, no global DuckDB connection, the `DuckDbSession` bracket request-scoped by law; an interpolated DDL string beside the `DdlStep` carrier — an unnamed verb, an idempotence spelled as an `IF NOT EXISTS` substring, and values joined into the text where `.api/duckdb.md` binds them — a hand-written `ATTACH` beside the `Attach` session row, and a page-private SQL-quoting twin beside the exported `quote_ident`/`quote_literal` folds are the deleted forms, both drifting the escape and dialect rule the moment one owner meets a name the other never tested; a free-form `con.sql(sql)` scan arm binding no admitted source where the `DuckDb` case binds the ref's `source` view and `QuerySpec.Sql` owns self-sourced SQL; a `scan_remote`/`scan_glob`/`window_rank`/`read_excel`/`ingest_corpus` method family, a generic receipt abstraction, a per-engine egress class family, a second SQL engine or second transport owner are the deleted forms; a `fastexcel` `ScanPlan` backend row where it is a `DatasetKind`-plus-capsule producer; a per-format polars IO plugin where one `register_io_source` reads `dataset.kind`; a local graph node-table owner or a `graph`-named `ScanPlan` arm where the `graph/graph#GRAPH` `GraphResult.frame` node table left-joins through the existing `pyarrow` `Table.join`; a pre-loaded-httpfs assumption; a `UPath.fs` handle handed to `register_filesystem` where `remote_store` bridges the one branch store, which second provider stack authenticates, retries, and pools outside every envelope the estate pins; key material spelled into a `CREATE SECRET` statement where `PROVIDER credential_chain` reads the provider's own chain, and a `CREATE OR REPLACE SECRET` whose silent replacement hides a caller's colliding row; a whole-table serialization re-spelled here beside the imported `tabular/interop#INTEROP` `arrow_bytes`, which two spellings of one byte stream fork into two content keys for one frame; and an undecorated `execute`/`scan` admitting a caller argument without the `@beartype(conf=FAULT_CONF)` public-seam contract.

```python
import duckdb
import numpy as np
import pyarrow as pa
import pyarrow.dataset as pads
import pyarrow.feather as paf
import pyarrow.parquet as papq
import sqlglot
from collections.abc import Callable, Iterable, Iterator, Mapping
from contextlib import contextmanager
from enum import StrEnum
from itertools import chain
from sqlglot import exp
from types import ModuleType
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

lazy import fastexcel
lazy import polars as pl
lazy from obstore.fsspec import FsspecStore

from beartype import beartype
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import decode as json_decode
from msgspec.json import encode as json_encode

from rasm.data.tabular.interop import DataLeg, arrow_bytes
from rasm.runtime.faults import FAULT_CONF, TERMINAL, TRANSIENT, Catch, FaultRow, Posture, RuntimeRail, async_boundary, boundary, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Journal, MeterFact, Resource
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.roots import STORE_RETRY, ResourceRef

if TYPE_CHECKING:
    from polars import Expr, LazyFrame

type CorpusRow = Mapping[str, Any]
type ExcelDType = Literal["null", "int", "float", "string", "boolean", "datetime", "date", "duration"]
type DatasetWrite = Literal["error", "overwrite_or_ignore", "delete_matching"]

_PREDICATE_NODES: Final[tuple[type[exp.Expression], ...]] = (exp.Where, exp.Having, exp.Qualify, exp.Join)


def predicate_count(text: str) -> int:
    return len(tuple(sqlglot.parse_one(text).find_all(*_PREDICATE_NODES)))


def quote_ident(name: str | None) -> str:
    return ".".join(exp.Identifier(this=part, quoted=True).sql(dialect="duckdb") for part in (name or "").split("."))


def quote_literal(value: str) -> str:
    return exp.Literal.string(value).sql(dialect="duckdb")


# --- [ERRORS] ---------------------------------------------------------------------------


def _scan_raises() -> Catch:
    return (duckdb.Error, pl.exceptions.PolarsError, fastexcel.FastExcelError, pa.ArrowException, OSError)


def _write_raises() -> Catch:
    return (pa.ArrowException, OSError)


SCAN_RUN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COLUMNAR, point="scan", arm="boundary", defect="scan-run", retriability=TRANSIENT
)
EGRESS_WRITE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COLUMNAR, point="egress", arm="boundary", defect="egress-write", retriability=TRANSIENT
)
EVIDENCE_ADMIT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.COLUMNAR, point="evidence", arm="boundary", defect="evidence-admit", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([SCAN_RUN, EGRESS_WRITE, EVIDENCE_ADMIT]))

# --- [TYPES] ----------------------------------------------------------------------------


class DdlVerb(StrEnum):
    CREATE_SECRET = "create-secret"
    ATTACH = "attach"
    USE = "use"
    PRAGMA = "pragma"
    CREATE_TABLE = "create-table"
    ALTER_TABLE = "alter-table"
    CALL = "call"


class Idempotence(StrEnum):
    ONCE = "once"
    ENSURED = "ensured"
    REPLACED = "replaced"


class DdlStep(Struct, frozen=True):
    verb: DdlVerb
    idempotence: Idempotence
    text: str
    parameters: tuple[object, ...] = ()

    def run(self, con: duckdb.DuckDBPyConnection) -> None:
        con.execute(self.text, list(self.parameters))


class DuckDbExtension(StrEnum):
    HTTPFS = "httpfs"
    SPATIAL = "spatial"
    H3 = "h3"
    SUBSTRAIT = "substrait"
    ICEBERG = "iceberg"
    DUCKLAKE = "ducklake"
    DELTA = "delta"
    AWS = "aws"
    AZURE = "azure"
    POSTGRES_SCANNER = "postgres_scanner"

    @property
    def repository(self) -> str | None:
        return "community" if self in _COMMUNITY else None

    @property
    def attach_type(self) -> str | None:
        return _ATTACH_TYPE.get(self.value)

    def load(self, con: duckdb.DuckDBPyConnection) -> None:
        con.install_extension(self.value, repository=self.repository)
        con.load_extension(self.value)


_COMMUNITY: Final[frozenset[DuckDbExtension]] = frozenset({DuckDbExtension.H3, DuckDbExtension.SUBSTRAIT})

_ATTACH_TYPE: Final[Map[str, str]] = Map.of_seq([("postgres_scanner", "postgres")])

_SCHEME_EXTENSION: Final[Map[str, DuckDbExtension]] = Map.of_seq([
    ("s3", DuckDbExtension.AWS),
    ("s3a", DuckDbExtension.AWS),
    ("gs", DuckDbExtension.AWS),
    ("az", DuckDbExtension.AZURE),
    ("abfs", DuckDbExtension.AZURE),
    ("abfss", DuckDbExtension.AZURE),
])


class SecretType(StrEnum):
    S3 = "s3"
    GCS = "gcs"
    R2 = "r2"
    AZURE = "azure"

    @property
    def extensions(self) -> tuple[DuckDbExtension, ...]:
        return _SECRET_EXTENSION[self.value]


_SECRET_EXTENSION: Final[Map[str, tuple[DuckDbExtension, ...]]] = Map.of_seq([
    ("s3", (DuckDbExtension.HTTPFS, DuckDbExtension.AWS)),
    ("gcs", (DuckDbExtension.HTTPFS, DuckDbExtension.AWS)),
    ("r2", (DuckDbExtension.HTTPFS, DuckDbExtension.AWS)),
    ("azure", (DuckDbExtension.AZURE,)),
])


class SecretRow(Struct, frozen=True):
    kind: SecretType
    name: str
    scope: str | None = None

    def step(self) -> DdlStep:
        scoped = f", SCOPE {quote_literal(self.scope)}" if self.scope is not None else ""
        return DdlStep(
            verb=DdlVerb.CREATE_SECRET,
            idempotence=Idempotence.ONCE,
            text=f"CREATE SECRET {quote_ident(self.name)} (TYPE {self.kind.value}, PROVIDER credential_chain{scoped})",
        )


class _DuckReader(Struct, frozen=True):
    function: str
    extension: DuckDbExtension | None = None


_DUCK_READER: Final[Map[str, _DuckReader]] = Map.of_seq([
    ("parquet", _DuckReader("read_parquet")),
    ("csv", _DuckReader("read_csv")),
    ("ndjson", _DuckReader("read_json")),
    ("delta", _DuckReader("delta_scan", DuckDbExtension.DELTA)),
    ("receipts", _DuckReader("delta_scan", DuckDbExtension.DELTA)),
    ("iceberg", _DuckReader("iceberg_scan", DuckDbExtension.ICEBERG)),
])


class ProfileMode(StrEnum):
    OFF = "off"
    STANDARD = "standard"
    DETAILED = "detailed"


@tagged_union(frozen=True)
class ProfileHarvest:
    tag: Literal["scalar", "duckdb", "polars", "daft", "band"] = tag()
    scalar: tuple[float, int, int] = case()
    duckdb: bytes = case()
    polars: tuple[tuple[tuple[str, int, int], ...], int, int] = case()
    daft: tuple[float, int, int, int, tuple[tuple[str, float, int], ...]] = case()
    band: bytes = case()


class OperatorSpan(Struct, frozen=True):
    name: Posture[str]
    seconds: Posture[float]
    cardinality: Posture[int]


class EngineProfile(Struct, frozen=True):
    cpu_time_s: Posture[float]
    latency_s: Posture[float]
    rows_returned: Posture[int]
    result_set_size: Posture[int]
    blocked_thread_s: Posture[float]
    bytes_read: Posture[int]
    bytes_written: Posture[int]
    operators: tuple[OperatorSpan, ...]
    partitions: Posture[int] = Posture(absent=None)

    @classmethod
    def of(cls, harvest: ProfileHarvest) -> "EngineProfile":
        match harvest:
            case ProfileHarvest(tag="scalar", scalar=(latency_s, rows, nbytes)):
                return cls(**(_uninstrumented("scalar") | _volume(latency_s, rows, nbytes) | {"operators": ()}))
            case ProfileHarvest(tag="duckdb", duckdb=payload):
                return _duck_profile(payload)
            case ProfileHarvest(tag="polars", polars=(nodes, rows, nbytes)):
                latest = max((end for _node, _start, end in nodes), default=None)
                return cls(**(
                    _uninstrumented("polars")
                    | {
                        "latency_s": Posture.of_option(Option.of_optional(latest).map(_seconds)),
                        "rows_returned": Posture(declared=rows),
                        "result_set_size": Posture(declared=nbytes),
                        "operators": tuple(
                            OperatorSpan(
                                name=Posture(declared=node),
                                seconds=Posture(declared=_seconds(end - start)),
                                cardinality=Posture(defaulted=(0, "polars")),
                            )
                            for node, start, end in nodes
                        ),
                    }
                ))
            case ProfileHarvest(tag="daft", daft=(latency_s, rows, nbytes, partitions, operators)):
                return cls(**(
                    _uninstrumented("daft")
                    | _volume(latency_s, rows, nbytes)
                    | {
                        "operators": tuple(
                            OperatorSpan(name=Posture(declared=node), seconds=Posture(declared=span), cardinality=Posture(declared=out))
                            for node, span, out in operators
                        ),
                        "partitions": Posture(declared=partitions),
                    }
                ))
            case ProfileHarvest(tag="band", band=stamped):
                return _unbanded(stamped)
            case unreachable:
                assert_never(unreachable)

    @property
    def withheld(self) -> tuple[str, ...]:
        return tuple(
            name
            for name in EngineProfile.__struct_fields__
            if isinstance(held := getattr(self, name), Posture) and held.tag == "absent"
        )

    def stamp(self, table: pa.Table) -> pa.Table:
        return table.replace_schema_metadata({**(table.schema.metadata or {}), _PROFILE_META: json_encode(_banded(self))})

    @classmethod
    def from_table(cls, table: pa.Table) -> "Option[EngineProfile]":
        return Option.of_optional((table.schema.metadata or {}).get(_PROFILE_META)).map(lambda stamped: cls.of(ProfileHarvest(band=stamped)))


_PROFILE_META: Final[bytes] = b"rasm.query.profile"
_BANDED: Final[tuple[str, ...]] = tuple(name for name in EngineProfile.__struct_fields__ if name != "operators")
_MICROS: Final[float] = 1_000_000.0

_PROFILING_MODE: Final[Map[str, str]] = Map.of_seq([("standard", "standard"), ("detailed", "detailed")])

_DUCK_ROOT: Final[Map[str, tuple[str, Callable[[Any], Any]]]] = Map.of_seq([
    ("latency", ("latency_s", float)),
    ("cpu_time", ("cpu_time_s", float)),
    ("blocked_thread_time", ("blocked_thread_s", float)),
    ("rows_returned", ("rows_returned", int)),
    ("result_set_size", ("result_set_size", int)),
    ("total_bytes_read", ("bytes_read", int)),
    ("total_bytes_written", ("bytes_written", int)),
])


def _seconds(micros: float) -> float:
    return micros / _MICROS


def _spelled(held: "Posture[Any]") -> "list[Any] | None":
    match held:
        case Posture(tag="absent"):
            return None
        case Posture(tag="declared", declared=value):
            return [value, None]
        case Posture(tag="defaulted", defaulted=(value, source)):
            return [value, source]
        case _ as unreachable:
            assert_never(unreachable)


def _readmitted(row: "list[Any] | None") -> "Posture[Any]":
    match row:
        case None:
            return Posture(absent=None)
        case [value, None]:
            return Posture(declared=value)
        case [value, source]:
            return Posture(defaulted=(value, source))
        case _ as unreachable:
            assert_never(unreachable)


def _banded(profile: "EngineProfile") -> "dict[str, Any]":
    return {name: _spelled(getattr(profile, name)) for name in _BANDED} | {
        "operators": [[_spelled(span.name), _spelled(span.seconds), _spelled(span.cardinality)] for span in profile.operators]
    }


def _unbanded(payload: bytes) -> "EngineProfile":
    banded = json_decode(payload)
    return EngineProfile(
        **{name: _readmitted(banded[name]) for name in _BANDED},
        operators=tuple(
            OperatorSpan(name=_readmitted(node), seconds=_readmitted(span), cardinality=_readmitted(rows))
            for node, span, rows in banded["operators"]
        ),
    )


def _stated(profile: "EngineProfile") -> "dict[str, object]":
    volume = profile.bytes_read.option().map2(lambda read, written: read + written, profile.bytes_written.option())
    return (
        profile.cpu_time_s.option().map(lambda held: {"cpu_s": held}).default_value({})
        | profile.latency_s.option().map(lambda held: {"latency_s": held}).default_value({})
        | profile.blocked_thread_s.option().map(lambda held: {"blocked_s": held}).default_value({})
        | volume.map(lambda held: {"bytes": held}).default_value({})
    )


def _uninstrumented(engine: str) -> "dict[str, Posture[Any]]":
    return {
        "cpu_time_s": Posture(defaulted=(0.0, engine)),
        "blocked_thread_s": Posture(defaulted=(0.0, engine)),
        "bytes_read": Posture(defaulted=(0, engine)),
        "bytes_written": Posture(defaulted=(0, engine)),
        "partitions": Posture(defaulted=(1, engine)),
    }


def _volume(latency_s: float, rows: int, nbytes: int) -> "dict[str, Posture[Any]]":
    return {
        "latency_s": Posture(declared=latency_s),
        "rows_returned": Posture(declared=rows),
        "result_set_size": Posture(declared=nbytes),
    }


def _duck_profile(payload: bytes) -> "EngineProfile":
    root = json_decode(payload)
    return EngineProfile(
        **{slot: _probed(root, key, cast) for key, (slot, cast) in _DUCK_ROOT.items()},
        operators=tuple(_duck_operators(root)),
        partitions=Posture(defaulted=(1, "duckdb")),
    )


def _probed[T](node: Mapping[str, Any], key: str, cast: Callable[[Any], T]) -> "Posture[T]":
    return Posture.of_option(Option.of_optional(node.get(key)).map(cast))


def _duck_operators(node: Mapping[str, Any]) -> "Iterator[OperatorSpan]":
    return chain.from_iterable(
        chain(
            (
                OperatorSpan(
                    name=_probed(child, "operator_name", str),
                    seconds=_probed(child, "operator_timing", float),
                    cardinality=_probed(child, "operator_cardinality", int),
                ),
            ),
            _duck_operators(child),
        )
        for child in node.get("children", ())
    )


class Attach(Struct, frozen=True):
    alias: str
    target: str
    kind: DuckDbExtension | None = None
    read_only: bool = False
    current: bool = False

    def steps(self) -> "Block[DdlStep]":
        clauses = ", ".join(
            row for row in (f"TYPE {self.kind.attach_type}" if self.kind and self.kind.attach_type else "", "READ_ONLY" if self.read_only else "") if row
        )
        attached = DdlStep(
            verb=DdlVerb.ATTACH,
            idempotence=Idempotence.ONCE,
            text=f"ATTACH {quote_literal(self.target)} AS {quote_ident(self.alias)}" + (f" ({clauses})" if clauses else ""),
        )
        selected = DdlStep(verb=DdlVerb.USE, idempotence=Idempotence.REPLACED, text=f"USE {quote_ident(self.alias)}")
        return Block.of_seq((attached, selected) if self.current else (attached,))


class DuckDbSession(Struct, frozen=True):
    extensions: tuple[DuckDbExtension, ...] = ()
    attach: tuple[Attach, ...] = ()
    secrets: tuple[SecretRow, ...] = ()
    filesystem: Any | None = None
    profiling: ProfileMode = ProfileMode.OFF

    @contextmanager
    def connect(self) -> Iterator[duckdb.DuckDBPyConnection]:
        with duckdb.connect() as con:
            for extension in dict.fromkeys((
                *self.extensions,
                *(row.kind for row in self.attach if row.kind is not None),
                *(needed for row in self.secrets for needed in row.kind.extensions),
            )):
                extension.load(con)
            for secret in self.secrets:
                secret.step().run(con)
            if self.filesystem is not None:
                con.register_filesystem(self.filesystem)
            for row in self.attach:
                for step in row.steps():
                    step.run(con)
            yield con

    @contextmanager
    def profiled(self) -> Iterator[tuple[duckdb.DuckDBPyConnection, Callable[[pa.Table], pa.Table]]]:
        with self.connect() as con:
            if self.profiling is not ProfileMode.OFF:
                con.execute("PRAGMA enable_profiling = 'no_output'")
                con.execute(f"PRAGMA profiling_mode = {quote_literal(_PROFILING_MODE[self.profiling.value])}")

            def harvest(table: pa.Table) -> pa.Table:
                return (
                    table
                    if self.profiling is ProfileMode.OFF
                    else EngineProfile.of(ProfileHarvest(duckdb=con.get_profiling_information().encode())).stamp(table)
                )

            yield con, harvest

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
    corpus: tuple[CorpusRow, ...] = case()

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
        return ScanPlan(corpus=rows)

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
            case ScanPlan(tag="window") | ScanPlan(tag="excel") | ScanPlan(tag="corpus"):
                return 0
            case unreachable:
                assert_never(unreachable)


class Landed(Struct, frozen=True):
    target: str
    files: int
    byte_length: int = 0


@tagged_union(frozen=True)
class ColumnarEgress:
    tag: Literal["arrow_ipc", "parquet", "feather", "dataset"] = tag()
    arrow_ipc: str = case()
    parquet: tuple[str, str] = case()
    feather: str = case()
    dataset: tuple[str, tuple[str, ...], str, DatasetWrite, str, Any] = case()

    @staticmethod
    def ArrowIpc(target: str) -> "ColumnarEgress":
        return ColumnarEgress(arrow_ipc=target)

    @staticmethod
    def Parquet(target: str, compression: str = "zstd") -> "ColumnarEgress":
        return ColumnarEgress(parquet=(target, compression))

    @staticmethod
    def Feather(target: str) -> "ColumnarEgress":
        return ColumnarEgress(feather=target)

    @staticmethod
    def Dataset(
        base_dir: str,
        partition_by: tuple[str, ...] = (),
        compression: str = "zstd",
        existing: DatasetWrite = "overwrite_or_ignore",
        basename: str = "part-{i}.parquet",
        filesystem: Any | None = None,
    ) -> "ColumnarEgress":
        return ColumnarEgress(dataset=(base_dir, partition_by, compression, existing, basename, filesystem))

    def emit(self, table: pa.Table) -> "RuntimeRail[Landed]":
        return boundary(EGRESS_WRITE, lambda: self._emit(table), catch=_write_raises())

    def write(self, table: pa.Table, *, predicate_count: int = 0) -> "RuntimeRail[QueryReceipt]":
        return self.emit(table).bind(lambda landed: QueryReceipt.railed(self.tag, landed.target, table, predicate_count=predicate_count))

    def _emit(self, table: pa.Table) -> "Landed":
        match self:
            case ColumnarEgress(tag="arrow_ipc", arrow_ipc=target):
                with pa.OSFile(target, "wb") as sink, pa.ipc.new_stream(sink, table.schema) as writer:
                    writer.write_table(table)
                return Landed(target=target, files=1)
            case ColumnarEgress(tag="parquet", parquet=(target, compression)):
                papq.write_table(table, target, compression=compression)
                return Landed(target=target, files=1)
            case ColumnarEgress(tag="feather", feather=target):
                paf.write_feather(table, target)
                return Landed(target=target, files=1)
            case ColumnarEgress(tag="dataset", dataset=(target, partition_by, compression, existing, basename, filesystem)):
                visited: list[Any] = []
                pads.write_dataset(
                    table,
                    target,
                    format="parquet",
                    partitioning=list(partition_by) or None,
                    partitioning_flavor="hive" if partition_by else None,
                    existing_data_behavior=existing,
                    basename_template=basename,
                    filesystem=filesystem,
                    file_options=pads.ParquetFileFormat().make_write_options(compression=compression),
                    file_visitor=visited.append,
                )
                return Landed(target=target, files=len(visited), byte_length=sum(written.size for written in visited))
            case unreachable:
                assert_never(unreachable)


class QueryReceipt(Struct, frozen=True):
    engine: str
    source: str
    columns: int
    predicate_count: int
    row_count: int
    content_key: ContentKey
    lineage_edges: tuple[tuple[str, str], ...] = ()
    mode: ProfileMode = ProfileMode.OFF
    profile: "Option[EngineProfile]" = Nothing

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
        mode: ProfileMode = ProfileMode.OFF,
        profile: "Option[EngineProfile]" = Nothing,
    ) -> "QueryReceipt":
        return cls(
            engine=engine,
            source=source,
            columns=table.num_columns,
            predicate_count=predicate_count,
            row_count=table.num_rows,
            content_key=content_key,
            lineage_edges=lineage_edges,
            mode=mode,
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
        mode: ProfileMode = ProfileMode.OFF,
    ) -> "RuntimeRail[QueryReceipt]":
        harvested = EngineProfile.from_table(table)
        return ContentIdentity.of("query", arrow_bytes(table)).map(
            lambda key: cls.of(
                engine, source, table, key, predicate_count=predicate_count, lineage_edges=lineage_edges, mode=mode, profile=harvested
            )
        )

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "domain": "query",
            "kind": self.engine,
            "key": self.content_key.hex,
            "rows": self.row_count,
            "lineage": len(self.lineage_edges),
            "profiling": self.mode,
        }
        match self.profile:
            case Option(tag="some", some=profile):
                Metrics.record(
                    {"rasm.query.rows": float(self.row_count)}
                    | profile.latency_s.option().map(lambda held: {"rasm.query.engine.duration": held * 1000.0}).default_value({}),
                    domain="query",
                    kind=self.engine,
                )
                facts |= _stated(profile) | ({} if not profile.withheld else {"withheld": ",".join(profile.withheld)})
            case Option(tag="none"):
                pass
        return (Receipt.of("query", ("emitted", self.source, facts)),)


@beartype(conf=FAULT_CONF)
def execute(plan: ScanPlan, dataset: DatasetRef, profiling: ProfileMode = ProfileMode.OFF) -> "RuntimeRail[pa.Table]":
    return boundary(SCAN_RUN, lambda: _run(plan, dataset, profiling), catch=_scan_raises())


@beartype(conf=FAULT_CONF)
def scan(plan: ScanPlan, dataset: DatasetRef, profiling: ProfileMode = ProfileMode.OFF) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
    return execute(plan, dataset, profiling).bind(
        lambda table: QueryReceipt.railed(plan.tag, str(dataset.ref.path), table, predicate_count=plan.predicate_count, mode=profiling).map(
            lambda receipt: (table, receipt)
        )
    )


def _metered(receipt: QueryReceipt) -> "Block[MeterFact]":
    return Block.of_seq(
        receipt.profile.bind(lambda profile: profile.latency_s.option())
        .map(lambda held: (MeterFact(resource=Resource.COMPUTE, quantity=round(held * 1000.0), surface=receipt.engine),))
        .default_value(())
    )


@beartype(conf=FAULT_CONF)
async def scan_async(
    plan: ScanPlan, dataset: DatasetRef, profiling: ProfileMode = ProfileMode.OFF, *, scope: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
    railed = await async_boundary(SCAN_RUN, lambda: on_thread(scan, plan, dataset, profiling), catch=_scan_raises())
    match railed.bind(lambda rail: rail):
        case Result(tag="ok", ok=(table, receipt)):
            return (await Journal.record(_metered(receipt), scope=scope)).map(lambda _landed: (table, receipt))
        case refused:
            return Error(refused.error)


def _run(plan: ScanPlan, dataset: DatasetRef, profiling: ProfileMode = ProfileMode.OFF) -> pa.Table:
    source = str(dataset.ref.path)
    match plan:
        case ScanPlan(tag="polars_lazy", polars_lazy=(projection, predicate)):
            lf = _scan_lazy(pl, dataset.kind, source)
            return _polars_collect(_pushed(pl, lf, projection, predicate), profiling)
        case ScanPlan(tag="io_source", io_source=(projection, predicate)):
            lf = pl.io.plugins.register_io_source(io_source=_io_source(dataset, source), schema=_scan_lazy(pl, dataset.kind, source).collect_schema())
            return _polars_collect(_pushed(pl, lf, projection, predicate), profiling)
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            row = _DUCK_READER.get(dataset.kind.value)
            if row is None:
                raise ValueError(f"{dataset.kind.value} carries no DuckDB relation reader")
            with DuckDbSession(extensions=(row.extension,) if row.extension else (), profiling=profiling).profiled() as (con, harvest):
                con.sql(f"SELECT * FROM {row.function}({quote_literal(source)})").create_view("source")
                rel = con.sql(sql)
                return harvest((rel.project(", ".join(projection)) if projection else rel).to_arrow_table())
        case ScanPlan(tag="arrow_dataset", arrow_dataset=(predicate, columns)):
            return pads.dataset(source).scanner(columns=list(columns) or None, filter=predicate).to_table()
        case ScanPlan(tag="remote_glob", remote_glob=(glob, predicate, partition_keys)):
            providers = _SCHEME_EXTENSION.try_find(dataset.ref.scheme).to_list()
            with DuckDbSession(
                extensions=(DuckDbExtension.HTTPFS, *providers), filesystem=remote_store(dataset.ref), profiling=profiling
            ).profiled() as (con, harvest):
                rel = con.read_parquet(glob, hive_partitioning=bool(partition_keys))
                return harvest((rel.filter(predicate) if predicate else rel).to_arrow_table())
        case ScanPlan(tag="window", window=(partitions, order, functions)):
            projection = (duckdb.StarExpression(), *(verb.expression(alias, partitions, order, args=args) for verb, alias, args in functions))
            with DuckDbSession(profiling=profiling).profiled() as (con, harvest):
                return harvest(con.read_parquet(source).select(*projection).to_arrow_table())
        case ScanPlan(tag="excel", excel=spec):
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
        case ScanPlan(tag="corpus", corpus=rows):
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
    if profiling is ProfileMode.OFF:
        return lf.collect(engine="streaming").to_arrow()
    frame, nodes = lf.profile(engine="streaming")
    table = frame.to_arrow()
    spans = tuple((str(node), int(start), int(end)) for node, start, end in nodes.iter_rows())
    return EngineProfile.of(ProfileHarvest(polars=(spans, table.num_rows, table.nbytes))).stamp(table)


def _io_source(dataset: DatasetRef, source: str) -> "Callable[[list[str] | None, Expr | None, int | None, int | None], Iterator[pl.DataFrame]]":
    def generator(with_columns: list[str] | None, predicate: "Expr | None", n_rows: int | None, batch_size: int | None) -> "Iterator[pl.DataFrame]":
        lf = _scan_lazy(pl, dataset.kind, source)
        lf = lf.select(with_columns) if with_columns else lf
        lf = lf.filter(predicate) if predicate is not None else lf
        lf = lf.head(n_rows) if n_rows is not None else lf
        frame = lf.collect(engine="streaming")
        step = batch_size or 65536
        yield from (frame.slice(offset, step) for offset in range(0, max(frame.height, 1), step))

    return generator


def remote_store(ref: ResourceRef) -> Any:
    return FsspecStore(ref.scheme, retry_config=STORE_RETRY, credential_provider=ref.credentials)


def arrow_columns(columns: tuple[str, ...], table: Mapping[str, np.ndarray]) -> pa.Table:
    return pa.table({name: table[name] for name in columns})


def admit_evidence(
    subject: str, key: ContentKey, columns: tuple[str, ...], table: Mapping[str, np.ndarray]
) -> "RuntimeRail[tuple[pa.Table, QueryReceipt]]":
    def build() -> pa.Table:
        admitted = arrow_columns(columns, table)
        rows = admitted.num_rows
        keyed = admitted.add_column(0, "content_key", pa.array([key.hex] * rows, type=pa.string()))
        return keyed.add_column(0, "subject", pa.array([subject] * rows, type=pa.string()))

    return boundary(EVIDENCE_ADMIT, build, catch=_write_raises()).bind(
        lambda admitted: QueryReceipt.railed("evidence", f"{subject}:{key.hex}", admitted).map(lambda receipt: (admitted, receipt))
    )
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
