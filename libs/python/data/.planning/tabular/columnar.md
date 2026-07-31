# [PY_DATA_COLUMNAR]

The dataset-reference identity owner and the folder's scan base: one polymorphic `DatasetRef` discriminating by source shape, the cross-engine lazy/streaming scan, the request-scoped DuckDB session rail, the typed columnar egress, and the content-keyed query receipt. The folder's scan base above `interop` alone — it imports that one module downward for the `arrow_bytes` serialization and holds zero back-edges, so every other folder composition edge points strictly down into it.

`DuckDbSession` authors the connect-install-attach lifecycle once and is composed downward by `tabular/query`, `tabular/materialize`, `spatial/query`, and `tabular/lakehouse`, each supplying its own `DuckDbExtension` and `Attach` rows rather than a hand-rolled `duckdb.connect()`-plus-install site; `quote_ident`/`quote_literal` are the exported quoting folds every composing owner spells its identifiers and URI literals through, so one escape rule serves the whole DuckDB plane; the same session owns the engine-profiling harvest — DuckDB exposes no scrape surface, so the profiled bracket IS the engine's observability, folded onto the one `QueryReceipt` stream and projected onto the runtime metric spine as `domain="query"` measures. `ScanPlan` sources refs, globs, and wire rows; SQL naming its own sources is `tabular/query#QUERY` `QuerySpec.Sql`'s concern — the standing scan/query boundary. The `predicate_count` fold and its `_PREDICATE_NODES` widening are declared here and imported by `tabular/query#QUERY` `_provenance`, so scan and query receipts count predicates off one source. `arrow_bytes` is the `tabular/interop#INTEROP` whole-table serialization every content key over a table payload rides, imported downward and never re-spelled; `arrow_columns` stays this owner's own admitting half — the one entry converting a producer's declared column roster and sealed arrays into that table. Three wire seams cross the edge as data endpoints: `tabular ← python:artifacts/documents` (the `Corpus` arm over `to_corpus_record` records), `tabular/columnar ← graph/graph` (the `pyarrow` `Table.join` left-outer enrichment of a `GraphResult.frame` node table), and `tabular/columnar ← python:geometry/graduation` (the `EvidenceFrame` and energy `ResultFrame` carriers through `arrow_columns`). Every receipt wires through runtime `ReceiptContributor` and keys by runtime `ContentIdentity`.

## [01]-[INDEX]

- [02]-[DATASET]: the `DatasetRef` identity owner discriminating the columnar source shapes.
- [03]-[SCAN]: the DuckDB session rail, the engine scan plans, the typed egress, the `arrow_columns` admitting fold, and the content-keyed query receipt.

## [02]-[DATASET]

- Owner: `DatasetRef` — the one polymorphic dataset owner; `DatasetKind` the closed `StrEnum` over the columnar-plane source shapes the scan owner reads end-to-end. The geometry/HDF shapes leave the axis by ownership ruling: `.3dm` reading is a `geometry` concern reaching the columnar plane only through the settled `spatial/mesh ⇄ python:geometry` Arrow point-record seam, the unstructured-mesh file lands in `spatial/mesh`, and the chunked HDF field lands in `gridded/field`. `DatasetKind` carries no row without a live `ScanPlan` arm.
- Cases: matched by `match`/`case`, never a `Get`/`List`/`Scan` family. `ARROW_DATASET` and `EXCEL` carry `scan_reader is None` because each is read by a dedicated non-polars arm (`ScanPlan.ArrowDataset` over the PyArrow scanner, `ScanPlan.Excel` over the `fastexcel` calamine decode), so the lazy `PolarsLazy`/`IoSource` arms reject those refs at the `scan.polars` boundary that converts the explicit reader-absence to a `BoundaryFault`, never a silent `KeyError`. Each lazy-scannable row carries its `polars` reader on the `scan_reader` behavior column so a plan resolves `kind.scan_reader` rather than a module-private dict over a silent subset.
- Entry: `DatasetRef.of` admits a `ResourceRef` and a `DatasetKind` and returns the frozen owner; the kind is recoverable from the source shape, never a knob.
- Packages: `polars`, `pyarrow`, `fastexcel` (the calamine reader the `Excel` arm decodes through — a `DatasetKind`-plus-capsule producer, never a `ScanPlan` engine backend), `beartype` (`@beartype(conf=FAULT_CONF)` the public admission contract on `DatasetRef.of` so a bad argument raises the `BeartypeCallHintViolation` root the `runtime/reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail, the shared `FAULT_CONF` the sibling data admission seams bind), runtime (`ResourceRef`/`ContentIdentity`/`FAULT_CONF`).
- Growth: a new columnar source shape is one `DatasetKind` row plus its `_SCAN_READER` entry when lazy-scannable (absent when a dedicated `ScanPlan` arm reads it); a geometry/raster source is a row on its real owner's axis, never re-admitted here; zero new surface.
- Boundary: no product identity, repository, or host-document mutation; a `get_csv`/`read_parquet`/`load_delta`/`read_excel` method family is the deleted form; a `RHINO_3DM`/`MESH`/`HDF` `DatasetKind` row forcing an eager in-plane read is the deleted form. This axis names a SOURCE SHAPE, so a residence SUBJECT is one `tabular/lakehouse#LAKEHOUSE` residence row pairing a kind with a `TableFormat` and a second kind per subject is the deleted form; `RECEIPTS` earns its row on shape alone, the residence family fixing the column roster a scan would otherwise take from the caller.

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
    # the branch evidence residence: a Delta table whose schema the `tabular/lakehouse#LAKEHOUSE` residence family
    # OWNS rather than a caller declaring it, so a ref carrying this kind names a known column roster and admits
    # only under `TableFormat.DELTA`. It stays a source shape here — WHICH subject a residence holds is the
    # residence row's, never a second `DatasetKind` per subject.
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

- Owner: `ScanPlan` — the engine/projection/predicate/partition policy tagged union; `WindowFunction` the analytical window-verb row carrying its OVER-node spelling; `ExcelSpec` the named decode-policy `Struct` the `Excel` case carries; `Attach` the attached-catalog row `DuckDbSession` folds into its own `ATTACH` statement; `SecretRow` over the closed `SecretType` vocabulary the same session issues once per connection as `CREATE SECRET … PROVIDER credential_chain` DDL, and `remote_store` the one obstore-backed filesystem handle it registers; `ColumnarEgress` the typed export over single-file Arrow/Parquet/IPC targets and the hive-partitioned Parquet tree alike; `QueryReceipt` the one receipt fold over scan plus transform plus egress, carrying the optional column-level `lineage_edges` the `tabular/query#QUERY` `QueryEngine` populates (`sqlglot.lineage` over the qualified SQL, `ibis.to_sql` over the bound expression) and the scan path leaves empty, plus the optional `EngineProfile` the profiled bracket harvests. Every case terminates in the same `RuntimeRail[pa.Table]` over the Arrow C Data Interface.
- Cases: closed by `assert_never`, each binding the engine or wire that owns it. `ArrowDataset` takes a pre-built `pyarrow.dataset.Expression` predicate the body never re-parses from a string. `IoSource` lifts a `DatasetRef` into a `LazyFrame` through `register_io_source` reading the same `DatasetKind.scan_reader` the `PolarsLazy` arm reads, so the plugin-pushed and direct-lazy scans over one ref fold the byte-identical receipt. The distributed out-of-core runner is the `tabular/query#QUERY` `QuerySpec.Streaming` daft case, never a scan arm; a connection-sourced remote read is `QuerySpec.Remote`, never a scan arm — this owner sources files, globs, and the two wire-ingest rows, never a database connection.
- Entry: `execute(plan, dataset)` returns `RuntimeRail[pa.Table]` over the Arrow C Data Interface for the egress hop; `scan(plan, dataset)` binds the same materialization into `RuntimeRail[tuple[pa.Table, QueryReceipt]]`, threading `QueryReceipt.railed(..., predicate_count=plan.predicate_count)` so the scan-only path carries the egress path's receipt. `ScanPlan.predicate_count` is the one derived projection over the case axis — the `DuckDb` arm calling the exported `predicate_count(sql)` fold `tabular/query#QUERY` `_provenance` shares, so scan and query count predicates identically, never a hardcoded `0`. `QueryReceipt.railed` derives the content key off the canonical Arrow bytes through the railed `ContentIdentity.of` and `.map`s the resolved key into the receipt; `QueryReceipt.of` is the plain factory taking the already-resolved key.
- Entry: `ColumnarEgress` splits its write in two — `emit` lands the bytes and answers the `Landed` file-and-byte evidence, `write` composes that half with the scan-plane `QueryReceipt`. An evidence-plane commit takes `emit`, because a `QueryReceipt` minted for it enters the receipt stream as a query-domain row and prices a residence's own storage as a query.
- Auto: the polars path selects its lazy reader off `dataset.kind` through `DatasetKind.scan_reader`, then runs `.select().filter().collect(engine="streaming")` — `engine="streaming"` the streaming spelling, never the `collect(streaming=True)` flag. `ScanPlan.DuckDb` binds the admitted ref as the one `source` view through the `_DUCK_READER` `DatasetKind`-keyed row — a SQL TABLE FUNCTION beside the extension it needs, because DuckDB exposes only a few readers as connection methods and the analytics residence's own reader is `delta_scan` — so the SQL is source-scoped by construction and the evidence table answers an interactive read. `RemoteGlob` opens `DuckDbSession(extensions=(DuckDbExtension.HTTPFS, …), filesystem=remote_store(dataset.ref))` so `register_filesystem` threads an `obstore.fsspec.FsspecStore` over the SAME Rust core, `STORE_RETRY` envelope, and credential resolution every other remote read in the branch crosses — `UPath.fs` is the deleted form, resolving fsspec's own `s3fs`/`gcsfs`/`adlfs` backend as a second provider stack the estate's pinned retry config never reaches. Cold `polars`, `fastexcel`, and `obstore.fsspec` imports defer at module scope. Every engine harvests its OWN payload through the case that carries it — DuckDB the profiling JSON its connection answers, polars its node-timing frame, Daft its `DataFrame.metrics` rows — and `ProfileHarvest.scalar` stays the portable floor an engine publishing no payload rides; each case answers zero for the facts its engine measures nowhere rather than aliasing latency onto them.
- Receipt: the scan contributes an emitted-phase `Receipt.of(owner, ("emitted", subject, facts))` row through `ReceiptContributor` (the two-argument owner-plus-evidence factory, the `(Phase, subject, facts)` triple, never a four-positional call) and produces a `QueryReceipt` keyed by `ContentIdentity` over the canonical `arrow_bytes`, never the `engine:source` string a content change cannot move. The `Excel`/`Corpus` wire arms key over their decoded Arrow bytes so a re-ingest of an unchanged workbook or corpus reuses its key, and the `Excel` arm stamps its path-shaped decode evidence as Arrow schema metadata so it rides the uniform `pa.Table` into the receipt rather than vanishing at the bare return. Receipts stay truth and instruments stay projections: a profile-bearing `contribute` records the engine latency and row count onto the runtime `Metrics.record` mapping arm under `domain="query"`, keyed by the engine tag, and an unprofiled receipt records nothing.
- Packages: `polars`, `duckdb`, `pyarrow`, `sqlglot`, `fastexcel`, `obstore` (`fsspec.FsspecStore` — the sanctioned filesystem-interface bridge over the branch object-store core, reached exactly where DuckDB demands a filesystem handle and never as a second IO owner), `tabular/interop` (`arrow_bytes`, the folder's one whole-table IPC serialization this owner keys every receipt through and never re-spells), `beartype` (`@beartype(conf=FAULT_CONF)` on the `execute`/`scan` entrypoints; the `QueryReceipt`/`ScanPlan`/`ColumnarEgress` staticmethods over already-admitted values carry no decorator), runtime (`RuntimeRail`/`ContentIdentity`/`ReceiptContributor`/`FAULT_CONF`/`ResourceRef`, and `Metrics.record` as the one instrument projection port — the DBAPI span train (`PsycopgInstrumentor`/`SQLite3Instrumentor`) activates at the runtime composition root, never at data altitude). The `DuckDbSession` owner authors the connect-install-register lifecycle once with the `DuckDbExtension` rows as seed data. The `pyarrow` `Table.join` left-outer join is the data-side endpoint of the `tabular/columnar ← graph/graph [WIRE]` seam, enriching a `GraphResult.frame` node-index-keyed table by the stable `node` key.
- Growth: a new engine is one `ScanPlan` case; a new DuckDB extension is one `DuckDbExtension` row (repository and attach type both row properties) every session consumer names for free; a new attachable catalog is one `_ATTACH_TYPE` row its `Attach` value then spells for free; a new object-store scheme is one `_SCHEME_EXTENSION` row every remote scan reads; a new in-engine credential type is one `SecretType` member with its `_SECRET_EXTENSION` row, the session's extension union and its DDL both deriving; a bespoke object-store credential provider is the existing `remote_store` argument, never a second store construction; a new lazy-pushdown source is one `DatasetKind.scan_reader` row the polars arms and the `IoSource` plugin already forward; a new DuckDB-readable ref kind is one `_DUCK_READER` row naming its table function beside the extension that function needs; a new window verb is one `WindowFunction` row; a new decode knob is one `ExcelSpec` field; a new corpus wire field is one column `from_pylist` already folds; a new egress format is one `ColumnarEgress` branch answering its own `Landed` evidence; a new native profile shape is one `ProfileHarvest` case; a new harvested profile fact is one `EngineProfile` field; a new query instrument is one measure name in `contribute` and one `InstrumentSpec` row on the runtime metrics owner.
- Boundary: no durable query rails, no global DuckDB connection, the `DuckDbSession` bracket request-scoped by law; a hand-written `ATTACH` beside the `Attach` session row and a page-private SQL-quoting twin beside the exported `quote_ident`/`quote_literal` folds are the deleted forms, both drifting the escape and dialect rule the moment one owner meets a name the other never tested; a free-form `con.sql(sql)` scan arm binding no admitted source where the `DuckDb` case binds the ref's `source` view and `QuerySpec.Sql` owns self-sourced SQL; a `scan_remote`/`scan_glob`/`window_rank`/`read_excel`/`ingest_corpus` method family, a generic receipt abstraction, a per-engine egress class family, a second SQL engine or second transport owner are the deleted forms; a `fastexcel` `ScanPlan` backend row where it is a `DatasetKind`-plus-capsule producer; a per-format polars IO plugin where one `register_io_source` reads `dataset.kind`; a local graph node-table owner or a `graph`-named `ScanPlan` arm where the `graph/graph#GRAPH` `GraphResult.frame` node table left-joins through the existing `pyarrow` `Table.join`; a pre-loaded-httpfs assumption; a `UPath.fs` handle handed to `register_filesystem` where `remote_store` bridges the one branch store, which second provider stack authenticates, retries, and pools outside every envelope the estate pins; key material spelled into a `CREATE SECRET` statement where `PROVIDER credential_chain` reads the provider's own chain, and a `CREATE OR REPLACE SECRET` whose silent replacement hides a caller's colliding row; a whole-table serialization re-spelled here beside the imported `tabular/interop#INTEROP` `arrow_bytes`, which two spellings of one byte stream fork into two content keys for one frame; and an undecorated `execute`/`scan` admitting a caller argument without the `@beartype(conf=FAULT_CONF)` public-seam contract.

```python signature
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
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.json import Decoder as JsonDecoder
from msgspec.json import decode as json_decode
from msgspec.json import encode as json_encode

from rasm.data.tabular.interop import arrow_bytes
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import STORE_RETRY, ResourceRef

if TYPE_CHECKING:
    from polars import Expr, LazyFrame

type CorpusRow = Mapping[str, Any]
# the closed `fastexcel` per-column dtype vocabulary the calamine decoder honors, never a free-string dtype it rejects late.
type ExcelDType = Literal["null", "int", "float", "string", "boolean", "datetime", "date", "duration"]
# `write_dataset` closes its collision policy here: `error` refuses a populated tree, `overwrite_or_ignore` OVERWRITES
# every file whose basename matches and ignores the rest, `delete_matching` clears every touched partition first.
# Appending therefore needs `overwrite_or_ignore` PLUS a per-generation `basename`, which is the provider's own stated
# contract — under the fixed default template that policy makes each generation clobber the previous one's `part-0`.
type DatasetWrite = Literal["error", "overwrite_or_ignore", "delete_matching"]

# the predicate-bearing node widening `tabular/query#QUERY` imports so scan-receipt and query-receipt counts read identical node classes.
_PREDICATE_NODES: Final[tuple[type[exp.Expression], ...]] = (exp.Where, exp.Having, exp.Qualify, exp.Join)


def predicate_count(text: str) -> int:
    # the one predicate-counting fold this owner exports beside `_PREDICATE_NODES`; `tabular/query#QUERY` `_provenance` and the `duckdb` arm both call it.
    return len(tuple(sqlglot.parse_one(text).find_all(*_PREDICATE_NODES)))


def quote_ident(name: str | None) -> str:
    # the session base owns SQL quoting for every DuckDB-composing owner: each dotted part routes through sqlglot
    # identifier quoting, so a caller-supplied table, column, or catalog alias cannot inject SQL. A per-page private
    # twin drifts the escape rule the moment one owner meets a name the other never tested.
    return ".".join(exp.Identifier(this=part, quoted=True).sql(dialect="duckdb") for part in (name or "").split("."))


def quote_literal(value: str) -> str:
    # single-quoted SQL string literal for the URI, DSN, and attach-target positions no parameter binding reaches.
    return exp.Literal.string(value).sql(dialect="duckdb")


class DuckDbExtension(StrEnum):
    HTTPFS = "httpfs"
    SPATIAL = "spatial"
    H3 = "h3"
    SUBSTRAIT = "substrait"
    ICEBERG = "iceberg"
    DUCKLAKE = "ducklake"
    # transactional-log reader over a Delta table, and the row that makes the analytics evidence residence
    # interactively queryable: `delta_scan` is a SQL table function like every other reader here, never a
    # connection method, so it reaches DuckDB only through a row naming the function beside the extension.
    DELTA = "delta"
    # object-plane credential rows, and the two carry DIFFERENT halves: `httpfs` registers the s3/gcs/r2 secret
    # types and their protocol while `aws` supplies the `credential_chain` PROVIDER over them, whereas `azure`
    # registers its own protocol AND its own secret type, so an azure glob needs this row where an s3 glob needs
    # `httpfs` beside it. A remote scan opens the scheme's own row rather than assuming ambient credentials.
    AWS = "aws"
    AZURE = "azure"
    # the operational-store attach: the evidence residence joins tenant, grant, and workload dimension tables
    # living in PostgreSQL inside ONE DuckDB statement, so `[TENANT_COST_JOIN]` folds without a second transport.
    POSTGRES_SCANNER = "postgres_scanner"

    @property
    def repository(self) -> str | None:
        # the install source is a ROW property: `h3`/`substrait` ride the community repository, the rest core.
        return "community" if self in _COMMUNITY else None

    @property
    def attach_type(self) -> str | None:
        # the `ATTACH … (TYPE <t>)` keyword an attachable extension answers, which is NOT the extension name:
        # `postgres_scanner` attaches as `postgres`. An absent row means the extension attaches through its own
        # target-string dialect — DuckLake's `ducklake:<backend>:<dsn>` prefix — or carries no attach surface.
        return _ATTACH_TYPE.get(self.value)

    def load(self, con: duckdb.DuckDBPyConnection) -> None:
        con.install_extension(self.value, repository=self.repository)
        con.load_extension(self.value)


_COMMUNITY: Final[frozenset[DuckDbExtension]] = frozenset({DuckDbExtension.H3, DuckDbExtension.SUBSTRAIT})

_ATTACH_TYPE: Final[Map[str, str]] = Map.of_seq([("postgres_scanner", "postgres")])

# object-store scheme to its credential-provider extension: `RemoteGlob` reads the ref's OWN scheme rather than
# loading every cloud provider on every glob, so a `file://` scan installs nothing and an `s3://` scan installs `aws`.
_SCHEME_EXTENSION: Final[Map[str, DuckDbExtension]] = Map.of_seq([
    ("s3", DuckDbExtension.AWS),
    ("s3a", DuckDbExtension.AWS),
    ("gs", DuckDbExtension.AWS),
    ("az", DuckDbExtension.AZURE),
    ("abfs", DuckDbExtension.AZURE),
    ("abfss", DuckDbExtension.AZURE),
])


class SecretType(StrEnum):
    # the DuckDB secret TYPES the provider extensions register. The two halves differ: `httpfs` registers `s3`,
    # `gcs`, and `r2` beside their protocol while `aws` supplies the `credential_chain` PROVIDER over all three,
    # whereas `azure` registers its own protocol AND its own type — `duckdb_secret_types()` names no azure type
    # until that extension loads. A row therefore names the extensions it needs rather than a caller listing them.
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
    # one IN-ENGINE credential as session DATA, the complementary half of the `FsspecStore` handle: that handle
    # serves every read crossing a Python filesystem object, while DuckDB's own pushdown readers — `delta_scan`,
    # `iceberg_scan`, `read_parquet` over an `s3://` uri — resolve inside the engine and cross none. `PROVIDER
    # credential_chain` walks the provider's OWN default chain (environment, config file, instance metadata), so no
    # key material is ever spelled into SQL, and `scope` binds one secret to one prefix where a session reads two
    # buckets under different identities. `CREATE SECRET` validates EAGERLY — a chain resolving nothing raises
    # `Secret Validation Failure` naming the link it stopped on, at session setup as a provisioning fact rather than
    # at scan time as a read failure — and the bare verb refuses a duplicate name outright, so two rows colliding on
    # one identifier fail loudly here instead of one silently replacing the other under `OR REPLACE`.
    kind: SecretType
    name: str
    scope: str | None = None

    def statement(self) -> str:
        scoped = f", SCOPE {quote_literal(self.scope)}" if self.scope is not None else ""
        return f"CREATE SECRET {quote_ident(self.name)} (TYPE {self.kind.value}, PROVIDER credential_chain{scoped})"


class _DuckReader(Struct, frozen=True):
    # DuckDB binds every source shape through a SQL TABLE FUNCTION, and only a few of those additionally hang off the
    # connection as a method — so this row names the FUNCTION and the arm binds `SELECT * FROM <fn>(<uri>)`. Naming the
    # connection method instead admits parquet, csv, and json alone and leaves the analytics evidence residence, whose
    # reader is `delta_scan`, with no DuckDB arm at all while its extension sits rowed on the session beside it.
    function: str
    extension: DuckDbExtension | None = None


# reader row per DuckDB-bindable `DatasetKind`; the `DuckDb` arm binds the admitted ref as the one `source` view and
# loads whatever extension the row names, so a fourth source shape is a row rather than a second session-shaping site.
_DUCK_READER: Final[Map[str, _DuckReader]] = Map.of_seq([
    ("parquet", _DuckReader("read_parquet")),
    ("csv", _DuckReader("read_csv")),
    ("ndjson", _DuckReader("read_json")),
    ("delta", _DuckReader("delta_scan", DuckDbExtension.DELTA)),
    # `RECEIPTS` rides the Delta reader under its own subject kind, so an interactive read of the receipt stream is the
    # same one-view binding every other source gets and "duckdb owns local interactive query" holds over the residence.
    ("receipts", _DuckReader("delta_scan", DuckDbExtension.DELTA)),
    ("iceberg", _DuckReader("iceberg_scan", DuckDbExtension.ICEBERG)),
])


class ProfileMode(StrEnum):
    OFF = "off"
    STANDARD = "standard"
    DETAILED = "detailed"


@tagged_union(frozen=True)
class ProfileHarvest:
    # one shape axis, each case carrying its PROVIDER's own payload untranslated so the decode lives in `of` alone:
    # portable execution scalars, DuckDB's profiling JSON text, polars' node-timing rows, Daft's operator rows, and
    # a stamped band. Each native case drops every fact its own payload already answers — DuckDB's text carries rows,
    # bytes, cpu, blocked time, and IO whole, so that case takes the text and nothing beside it.
    tag: Literal["scalar", "duckdb", "polars", "daft", "band"] = tag()
    scalar: tuple[float, int, int] = case()
    duckdb: bytes = case()
    polars: tuple[tuple[tuple[str, int, int], ...], int, int] = case()
    daft: tuple[float, int, int, int, tuple[tuple[str, float, int], ...]] = case()
    band: bytes = case()


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
        # one decoded band shape; every admitted harvest shape folds into the identical `EngineProfile`.
        match harvest:
            case ProfileHarvest(tag="scalar", scalar=(latency_s, rows, nbytes)):
                return cls(latency_s, latency_s, rows, nbytes, 0.0, 0, 0, ())
            case ProfileHarvest(tag="duckdb", duckdb=payload):
                return _duck_profile(payload)
            case ProfileHarvest(tag="polars", polars=(nodes, rows, nbytes)):
                # polars publishes node spans in MICROSECONDS off one monotonic origin, so query latency reads off
                # its last span's end and each operator's cost off its own width. Zeros here are PROVEN: polars
                # reports no cpu, blocked, or IO fact, and aliasing latency onto them invents evidence.
                return cls(
                    0.0,
                    max((end for _node, _start, end in nodes), default=0) / _MICROS,
                    rows,
                    nbytes,
                    0.0,
                    0,
                    0,
                    tuple((node, (end - start) / _MICROS, 0) for node, start, end in nodes),
                )
            case ProfileHarvest(tag="daft", daft=(latency_s, rows, nbytes, partitions, operators)):
                return cls(latency_s, latency_s, rows, nbytes, 0.0, 0, 0, operators, partitions)
            case ProfileHarvest(tag="band", band=stamped):
                return _PROFILE_BAND_DECODER.decode(stamped)
            case unreachable:
                assert_never(unreachable)

    def stamp(self, table: pa.Table) -> pa.Table:
        # the profile rides the uniform table as its own metadata band — the Excel decode-evidence pattern — so every
        # engine arm returns a table-shaped result and `sibling` query arms never touch the private band key.
        return table.replace_schema_metadata({**(table.schema.metadata or {}), _PROFILE_META: json_encode(self)})

    @classmethod
    def from_table(cls, table: pa.Table) -> "EngineProfile | None":
        # the band read every railed and federated receipt shares.
        stamped = (table.schema.metadata or {}).get(_PROFILE_META)
        return cls.of(ProfileHarvest(band=stamped)) if stamped else None


_PROFILE_BAND_DECODER: Final[JsonDecoder[EngineProfile]] = JsonDecoder(EngineProfile)
_PROFILE_META: Final[bytes] = b"rasm.query.profile"
_MICROS: Final[float] = 1_000_000.0

# `profiling_mode` PRAGMA value per band. `enable_profiling` takes the OUTPUT target, and its connection-method
# spelling `enable_profiling()` defaults to the query-graph renderer, printing a box-drawn plan into the host's
# console on every statement — which is why `no_output` is the arming here: DuckDB keeps the payload inside the
# connection and `get_profiling_information()` answers it.
_PROFILING_MODE: Final[Map[str, str]] = Map.of_seq([("standard", "standard"), ("detailed", "detailed")])

# metric keys DuckDB publishes ONLY at its root node, so this roster keeps the operator walk from re-reading them
# per child and a childless plan still answers them. `latency`/`blocked_thread_time`/`cpu_time` carry SECONDS as
# floats, `result_set_size`/`total_bytes_*` bytes, `rows_returned` a row count.
_DUCK_ROOT: Final[tuple[str, ...]] = ("latency", "cpu_time", "blocked_thread_time", "rows_returned", "result_set_size", "total_bytes_read", "total_bytes_written")


def _duck_profile(payload: bytes) -> "EngineProfile":
    # `get_profiling_information()` answers JSON TEXT, never a mapping — reading it as a dict walks a string's
    # characters. Profiling unarmed answers `{"result": "disabled"}` carrying no metric key at all, and
    # `custom_profiling_settings` narrows the published set at BOTH altitudes, so key ABSENCE is the normal case:
    # every read probes with `.get` and an unarmed connection yields the zero profile rather than a `KeyError`.
    root = json_decode(payload)
    latency, cpu, blocked, rows, nbytes, read, written = (root.get(key, 0) for key in _DUCK_ROOT)
    return EngineProfile(
        cpu_time_s=float(cpu),
        latency_s=float(latency),
        rows_returned=int(rows),
        result_set_size=int(nbytes),
        blocked_thread_s=float(blocked),
        bytes_read=int(read),
        bytes_written=int(written),
        operators=tuple(_duck_operators(root)),
    )


def _duck_operators(node: Mapping[str, Any]) -> "Iterator[tuple[str, float, int]]":
    # DuckDB hangs its physical plan as a CHILD tree under the metric root: that root names no operator, and each
    # descendant carries `operator_name` beside its own timing and cardinality. Native recursion holds to DATA depth —
    # plans nest as deep as a statement's own algebra and never as deep as its input — and this lazy `chain` flatten
    # keeps pre-order, so an operator row reads positionally against the engine's own `EXPLAIN ANALYZE` render.
    return chain.from_iterable(
        chain(
            ((str(child.get("operator_name", "")), float(child.get("operator_timing", 0.0)), int(child.get("operator_cardinality", 0))),),
            _duck_operators(child),
        )
        for child in node.get("children", ())
    )


class Attach(Struct, frozen=True):
    # one attached catalog as session DATA. `kind` carries the extension serving the attach, so a caller naming an
    # attach never separately lists its extension row and no arm hand-writes an `ATTACH` string; the statement is
    # `TYPE`-clause where the row answers `attach_type` (postgres_scanner) and bare where the target string carries
    # its own dialect (DuckLake's `ducklake:<backend>:<dsn>` prefix). `current` selects the attached catalog so a
    # catalog-scoped surface — DuckLake's `snapshots()`, `table_changes()` — resolves without a second statement.
    alias: str
    target: str
    kind: DuckDbExtension | None = None
    read_only: bool = False
    current: bool = False

    def statement(self) -> str:
        clauses = ", ".join(
            row for row in (f"TYPE {self.kind.attach_type}" if self.kind and self.kind.attach_type else "", "READ_ONLY" if self.read_only else "") if row
        )
        return f"ATTACH {quote_literal(self.target)} AS {quote_ident(self.alias)}" + (f" ({clauses})" if clauses else "")


class DuckDbSession(Struct, frozen=True):
    extensions: tuple[DuckDbExtension, ...] = ()
    attach: tuple[Attach, ...] = ()
    # `secrets` are the in-engine credential rows the pushdown readers resolve through; `filesystem` is the
    # `obstore.fsspec.FsspecStore` handle every read crossing a Python filesystem object rides. The two are
    # complementary halves of ONE object-plane identity, never alternatives: a `delta_scan` over `s3://` never
    # touches the handle and a polars-plugin read never sees the secret.
    secrets: tuple[SecretRow, ...] = ()
    filesystem: Any | None = None
    profiling: ProfileMode = ProfileMode.OFF

    @contextmanager
    def connect(self) -> Iterator[duckdb.DuckDBPyConnection]:
        # request-scoped by law — one bracket per run; the deduplicated extension union loads once per connection, the
        # secret rows create once inside it, and the obstore-backed filesystem handle registers.
        # Each attach row's own extension AND each secret row's own extensions join that union, so neither an attach
        # nor a credential depends on a caller having listed its provider — the ordering is load, secret, register,
        # attach, select, because a `CREATE SECRET` resolves no type before its extension is in the engine, an
        # `ATTACH` over an object-store data path resolves no credential before its secret exists, and a catalog
        # cannot go current before it is attached.
        with duckdb.connect() as con:
            for extension in dict.fromkeys((
                *self.extensions,
                *(row.kind for row in self.attach if row.kind is not None),
                *(needed for row in self.secrets for needed in row.kind.extensions),
            )):
                extension.load(con)
            for secret in self.secrets:
                con.execute(secret.statement())
            if self.filesystem is not None:
                con.register_filesystem(self.filesystem)
            for row in self.attach:
                con.execute(row.statement())
                if row.current:
                    con.execute(f"USE {quote_ident(row.alias)}")
            yield con

    @contextmanager
    def profiled(self) -> Iterator[tuple[duckdb.DuckDBPyConnection, Callable[[pa.Table], pa.Table]]]:
        # DuckDB's OWN payload is the harvest: cpu, blocked-thread, IO volume, and the whole operator tree come off
        # `get_profiling_information()`, so no wall-clock scalar stands in for evidence the engine already measures.
        # Both PRAGMA values interpolate as LITERALS because DuckDB binds no parameter inside a PRAGMA, which is why
        # `profiling_mode` arrives off the closed `_PROFILING_MODE` row rather than a caller string. `harvest` reads
        # this connection AFTER its arm materializes, so each payload describes that arm's own query rather than
        # whatever statement the bracket last touched.
        with self.connect() as con:
            if self.profiling is not ProfileMode.OFF:
                con.execute("PRAGMA enable_profiling = 'no_output'")
                con.execute(f"PRAGMA profiling_mode = {quote_literal(_PROFILING_MODE[self.profiling.value])}")

            def harvest(table: pa.Table) -> pa.Table:
                # OFF short-circuits whole: no payload read, no decode, no stamp — the disabled path is free.
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
    # what a write actually LANDED, answered by the write itself rather than re-derived by a caller stat walk. The
    # partitioned arm reads both numbers off the provider's own per-file callback; the single-file arms answer the
    # count they land by construction and report NO byte evidence, because a provider that measures nothing reports
    # nothing rather than the in-memory `nbytes` a compressed file never equals.
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
        # this row owns the MULTI-file hive-partitioned write beside the single-file `Parquet` row: one directory
        # tree whose partition columns leave the files and become path segments, which is what makes a
        # partition-pruned scan read one segment instead of every file. `filesystem` takes the runtime-resolved
        # handle, so one case writes a local tree and an object-plane prefix without a second egress family. An
        # APPENDING caller passes its own generation token in `basename`: the default template collides with every
        # prior generation under `overwrite_or_ignore`, so appending stays a caller value and the default stays one-shot.
        return ColumnarEgress(dataset=(base_dir, partition_by, compression, existing, basename, filesystem))

    def emit(self, table: pa.Table) -> "RuntimeRail[Landed]":
        # this half writes ALONE and answers what it landed. A residence commit takes it: a `QueryReceipt` minted for
        # an evidence write enters the evidence stream as a query-domain row and prices the residence's own storage
        # as a query, and the file and byte evidence a cost plane reads has no other producer.
        return boundary(f"egress.{self.tag}", lambda: self._emit(table))

    def write(self, table: pa.Table, *, predicate_count: int = 0) -> "RuntimeRail[QueryReceipt]":
        return self.emit(table).bind(lambda landed: QueryReceipt.railed(self.tag, landed.target, table, predicate_count=predicate_count))

    def _emit(self, table: pa.Table) -> "Landed":
        match self:
            case ColumnarEgress(tag="arrow_ipc", arrow_ipc=target):
                with pa.OSFile(target, "wb") as sink, pa.ipc.new_stream(sink, table.schema) as writer:
                    writer.write_table(table)
                return Landed(target=target, files=1)
            case ColumnarEgress(tag="parquet", parquet=(target, compression)):
                # `write_table` carries no metadata collector, so the single-file arms answer file evidence alone —
                # a stat round-trip against a remote object plane buys one number at the cost of a second request.
                papq.write_table(table, target, compression=compression)
                return Landed(target=target, files=1)
            case ColumnarEgress(tag="feather", feather=target):
                paf.write_feather(table, target)
                return Landed(target=target, files=1)
            case ColumnarEgress(tag="dataset", dataset=(target, partition_by, compression, existing, basename, filesystem)):
                # compression rides `file_options` here, never a `compression=` keyword — `write_dataset` carries
                # none, so a caller-facing knob spelled as one would silently write uncompressed. `file_visitor` is
                # this provider's ONE surface answering what a partitioned write actually landed: each visited
                # `WrittenFile` carries `path` beside `size`, the file's real on-disk byte count, so this arm reads
                # a measurement rather than a derivation. `metadata` row-group `total_byte_size` beside it is the
                # UNCOMPRESSED column volume and reads identically under every codec, so a residence sized off
                # that number prices a zstd tail at its raw width and never moves when the codec policy does.
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
        # `domain`/`kind`/`key` carry the lifted evidence contract the `tabular/lakehouse#LAKEHOUSE` residence reads,
        # spelling the SAME pair handed `Metrics.record` beside the minted key, so a stored row rejoins the series it
        # emitted. Profile-band volume rides `bytes` for the same reason: a quantity the live cost arm reads and the
        # receipt drops prices every reconstruction of that window at zero.
        facts: dict[str, object] = {
            "domain": "query",
            "kind": self.engine,
            "key": self.content_key.hex,
            "rows": self.row_count,
            "lineage": len(self.lineage_edges),
        }
        if self.profile is not None:
            Metrics.record(
                {"rasm.query.engine.duration": self.profile.latency_s * 1000.0, "rasm.query.rows": float(self.row_count)},
                domain="query",
                kind=self.engine,
            )
            facts |= {
                "cpu_s": self.profile.cpu_time_s,
                "latency_s": self.profile.latency_s,
                "blocked_s": self.profile.blocked_thread_s,
                "bytes": self.profile.bytes_read + self.profile.bytes_written,
            }
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
            lf = _scan_lazy(pl, dataset.kind, source)
            return _polars_collect(_pushed(pl, lf, projection, predicate), profiling)
        case ScanPlan(tag="io_source", io_source=(projection, predicate)):
            lf = pl.io.plugins.register_io_source(io_source=_io_source(dataset, source), schema=_scan_lazy(pl, dataset.kind, source).collect_schema())
            return _polars_collect(_pushed(pl, lf, projection, predicate), profiling)
        case ScanPlan(tag="duckdb", duckdb=(sql, projection)):
            # source-scoped by construction: the row's own table function binds the admitted ref as the one `source`
            # view and that same row names the extension the function needs, so the SQL never self-sources, no arm
            # hand-loads a provider, and the uri rides `quote_literal` rather than string-joined into a statement.
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
            # the ref's OWN scheme picks its credential-provider extension off `_SCHEME_EXTENSION`, so an `s3://`
            # glob installs `aws` and a `file://` glob installs neither — a fixed cloud-provider set would install
            # both on every scan and still miss the next scheme. The filesystem handle is the obstore-backed
            # `remote_store` bridge, never `UPath.fs`: one store, one credential resolution, one retry envelope.
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
        case ScanPlan(tag="corpus", corpus=rows):
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
    # OFF short-circuits whole: no profile frame, no decode, no stamp — the disabled path is free. `profile` runs the
    # SAME streaming engine `collect` does and answers `(result, nodes)`, so profiling costs the plan's node timings
    # and nothing else; a wall-clock bracket around `collect` would report the same latency with no operator evidence.
    # One band spans every engine, so node rows land as operator rows while facts polars publishes nowhere — cpu,
    # blocked time, IO volume — stay zero rather than borrowing the latency reading.
    if profiling is ProfileMode.OFF:
        return lf.collect(engine="streaming").to_arrow()
    frame, nodes = lf.profile(engine="streaming")
    table = frame.to_arrow()
    spans = tuple((str(node), int(start), int(end)) for node, start, end in nodes.iter_rows())
    return EngineProfile.of(ProfileHarvest(polars=(spans, table.num_rows, table.nbytes))).stamp(table)


def _io_source(dataset: DatasetRef, source: str) -> "Callable[[list[str] | None, Expr | None, int | None, int | None], Iterator[pl.DataFrame]]":
    # `register_io_source` contract: the generator yields polars `DataFrame` windows (NOT `pa.RecordBatch`), `predicate`
    # arrives as a polars `Expr`, and projection/predicate/row-cap push through `with_columns`/`filter`/`head`.
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
    # DuckDB's `register_filesystem` takes an fsspec `AbstractFileSystem`, and `UPath.fs` resolves FSSPEC's own
    # backend (`s3fs`/`gcsfs`/`adlfs`) — a SECOND object-store stack beside the `obstore` Rust core every other remote
    # read in the branch crosses, carrying its own credential resolution, its own retry envelope, and its own
    # connection pool, none of which the estate-pinned `STORE_RETRY` config reaches. `obstore.fsspec.FsspecStore` is
    # the sanctioned bridge for exactly this case: one store handle wearing the filesystem interface DuckDB demands,
    # so a `RemoteGlob` authenticates, retries, and pools identically to the egress and acquisition legs. The
    # `credential_provider=` is obstore's OWN constructor keyword and stays — but its value is the ref's own column,
    # never a second parameter beside the ref: a composition holding a bespoke provider
    # (`obstore.auth.planetary_computer`, an Earthdata token) stamps it on the residence coordinate, so this bridge
    # authenticates off the same column the egress and acquisition legs read and neither caller fills a field twice.
    return FsspecStore(ref.scheme, retry_config=STORE_RETRY, credential_provider=ref.credentials)


def arrow_columns(columns: tuple[str, ...], table: Mapping[str, np.ndarray]) -> pa.Table:
    # data-side endpoint of the `[BOUNDARY]: arrow_bytes` seams the geometry ARCHITECTURE ledger declares: a producer
    # hands its DECLARED column order beside the name-keyed arrays it already sealed — the geometry `EvidenceFrame`
    # and energy `ResultFrame` carriers both — so ONE Arrow entry admits every columnar crossing and no producer frame
    # type enters this tier as a foreign shape. Order reads the ROSTER, never the mapping: the roster is what the
    # producer declared, and taking the mapping's own order re-schemas the table whenever a producer rebuilds it.
    # Producers catch a width mismatch at their own admission fence; this fold converts an already-rectangular table.
    return pa.table({name: table[name] for name in columns})
```

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
