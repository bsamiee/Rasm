# [PY_DATA_LAKEHOUSE]

Table-format interchange crosses one `LakeOp` operation axis with one `TableFormat` provider axis on one `Lakehouse` owner over Delta, Iceberg, Lance, DuckLake, and the non-transactional Parquet tree. `Lakehouse.run` folds the ensure/write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore/reference lifecycle through the `LakeOp` tagged union and dispatches one `(TableFormat, tag)` arm to a `RuntimeRail[LakeReceipt]` — the operation axis format-agnostic, the format binding a separate discriminant, so a new format is one `TableFormat` row and its arms, never a parallel Iceberg or Lance owner, and formats reaching fewer operations state that as `_REFUSAL` rows. `Lakehouse` commits and reads snapshots over the provider surface; it holds no durable store.

`Residence` rows the analytics planes this owner writes — the Delta evidence table, the Iceberg alternative, the Parquet cold tail — each answering fits, admit, tenancy, lifetime, and a degradation derived from the reach matrix, so arming a residence is a row and no arm carries a partition or lifetime literal. `Lakehouse.sink` folds a receipt stream through `receipt_frame` onto the schema the `_RECEIPT_COLUMNS` roster generates and commits it through the SAME `(format, tag)` matrix a caller's table rides, both producer facts REQUIRED so a commit clock never stands in for the observation clock and an omitted tenancy never reads as a deliberate one, so the evidence plane inherits reach, veto, retry, span, and snapshot identity whole. `ResidenceRow.ops` splits that plan across a `LakePhase` axis: INGEST arms the plane off its own `TableLayout` and commits one append per drain, MAINTAIN carries the clustering and retention passes the deploy plane schedules, because a clustering pass folded into ingest rewrites every file the plane holds on every drain. Both rosters derive off the reach matrix, so a residence whose format authors no table object plans no arming rather than declaring the absence by hand, and no residence waits on a foreign engine to plant it first. That plane carries NO cardinality ceiling — unbounded dimensionality is the capability a metrics view cap exists to destroy — and its `cap` column is typed `False` so no later pass adds one.

Time travel is one vocabulary both directions of the axis read: `Read`/`Restore` consume a generation, an instant, or a named ref, and `Reference` AUTHORS that name — so a tag-string read reaches a tag this owner minted rather than one a foreign engine happened to leave behind, the same out-of-band step `Ensure`'s arming argument deletes. Iceberg's read path is the core-loadable DuckDB `iceberg` extension with `pyiceberg` the catalog-write fallback; Lance carries the multimodal-asset versioning and `create_index` ANN rail; DuckLake rides one `Attach` row over the shared `tabular/columnar#SCAN` `DuckDbSession`, the single session every DuckDB-backed arm reuses. `Ancestry` is the version-lineage READ over each format's own history surface, projecting a `Generation` edge roster onto the receipt `payload` as the frame `graph/graph#GRAPH` walks and the composition root joins back onto the scan plane by `node` — the name is `Generation` and never `lineage`, which `tabular/columnar#SCAN` `QueryReceipt.lineage_edges` already holds for value attribution. `changefeed` is the Delta `load_cdf` and DuckLake `table_changes` feed the `tabular/materialize#MATERIALIZE` `DerivedSnapshot._materialize` consumer reads, and the receipt carries that feed on its `payload` slot so the consumer composes this owner rather than re-opening `DeltaTable` behind it. Every commit contributes through runtime `ReceiptContributor`, keys by `ContentIdentity`, and — when mutating — rides the `runtime/reliability/resilience#RESILIENCE` `RetryClass.LAKE_COMMIT` `guarded_sync` envelope; `open`/`run`/`run_async` admit through `@beartype(conf=FAULT_CONF)`, the shared config the sibling `interop`/`egress`/`columnar` seams bind. Table-protocol governance — deletion vectors, `TableFeatures` — is DECLINED here: the C# `Rasm.Persistence` at-rest owner holds it, never a data-side commit toggle.

## [01]-[INDEX]

- [02]-[LAKEHOUSE]: `Lakehouse` crosses one `LakeOp` operation axis with one `TableFormat` provider axis, and the `Residence` family rides that same matrix, its evidence plane committing through the `sink` fold.

## [02]-[LAKEHOUSE]

- Owner: `Lakehouse` over the `LakeOp` operation axis (a `tagged_union` matched by `match (self.table_format, op)`) and the `TableFormat` `StrEnum` provider axis, dispatched one `(format, tag)` arm — two orthogonal discriminants, so a new operation is one `LakeOp` case and a new format one `TableFormat` row, never a `read_delta`/`write_delta`/`delete_delta` method family and never a parallel `IcebergLakehouse`/`LanceLakehouse` pair. Writer tuning rides one `WriteTuning` policy `Struct` carried on `Write`, never a parallel `WriteTuned` op or a knob tail; the merge delete-on-no-match rides one `delete_unmatched` discriminant selecting the third `when_not_matched_by_source_delete` clause, never a `MergeDelete` op.
- Owner: `Residence` rows the analytics planes by CAPABILITY — `ResidenceRow` answering the estate residence floor (`fits`, `admit`, `tenancy`, `lifetime`, `degrade`, `cap` typed `False`) beside this owner's own extension of kind, format, partition roster, clustering roster, writer tuning, lifetime window, and an arming `TableLayout`, with `degrade` DERIVED off `_REFUSAL`, so a format losing an op degrades every residence riding it with zero row edits and a hand-written degradation sentence cannot drift from the refusal producing it.
- Owner: `TableLayout` states the authored table spec as DATA — schema, `PartitionTransform`-keyed partition pairs, sort order, properties — and each format arm projects that one declaration onto its own grammar (`bucket[16]` tokens at the iceberg catalog, `bucket(16, col)` SQL at ducklake, bare columns at delta), so a second residence arms through the same row shape and a transform a format cannot spell refuses by name rather than vanishing.
- Owner: `TableFormat.PARQUET` seats the object-plane tree as a FORMAT rather than a writer hanging off the residence family — its `_REFUSAL` cells state the whole non-transactionality, its armed write and read inherit the reach gate, the commit veto, the retry envelope, the span, and the receipt every sibling format rides, and `ResidenceRow.degrade` then derives the cold row's degradation from those cells instead of a hand-kept sentence beside them. Provider-dialect divergence stays a row: `_PARQUET_EXISTING` projects the write mode onto the tree's collision policy and `_PARQUET_CODEC` the writer's codec roster onto its file options, a codec the tree cannot spell refusing by name rather than downgrading.
- Law: `contribute` records the file-churn pair under the owner's own `DOMAIN` segment keyed by operation, so the commit plane projects onto the metric spine beside its tabular siblings.
- Law: a RESIDENCE commit records nothing — metering the plane that stores the receipt stream feeds a series back into the stream it just wrote.
- Law: `contribute` spells `domain` and `kind` into its facts as the SAME pair it hands `Metrics.record`, so a stored evidence row rejoins the series its live twin emitted.
- Law: `quantity` stays receipt-only, its `LakeUnit` varying per arm, so one descriptor never carries four magnitudes.
- Law: a non-committing op moves no files and records nothing, keeping read and changefeed arms off the commit series.
- Entry: `Lakehouse.demand` answers the MISSING subset of a capability set through `traversed(..., by=Disposition.ACCUMULATE)`, so a planner needing travel and a change feed learns about both at once where `_reach` reads one cell and short-circuits. `Lakehouse.open` admits a dataset, format policy, in-engine credential rows, composition scope, an optional `Fence`, and the `LakePlane` role the handle's table plays on the evidence spine through ONE `_ADMIT` row read — the row's `kinds` set gating the ref shape and its `needs` roster the coordinates — where three hand-written arms checked the ref kind on the Delta arm alone and let a Lance or Iceberg handle open over a CSV ref; the admitted `kind` rides the handle, so `_admitted` proves a residence commit holds the SUBJECT its row names rather than merely a table in the right format. `sink` and the awaitable `sink_async` are the evidence plane's one ingest over that admission, folding a receipt stream through `receipt_frame` and the row's INGEST plan — the awaitable leg exists because a composition draining its own receipts on an event loop otherwise stalls it for every commit. `maintain` runs the same row's MAINTAIN plan scoped to a named generation, the entrypoint a scheduled job calls. `run` and the awaitable `run_async` both read ONE `_gated` prologue — the reach matrix, then the `LAKE_COMMIT_POINT` veto for a committing op — and select the same envelope, the awaitable leg adding one `on_thread` band hop because every arm is a blocking native commit an async composition otherwise runs inline on its loop. `Read`, `ChangeFeed`, and dry-run `Vacuum` ride the bare boundary rail; a refused cell answers its typed row ahead of both the hook point and the retry envelope, and the veto's own fact never rides out as the gate's value. Every committing arm additionally crosses the owner's `ResourceGuard` — one per opened owner, composition-bound — so two same-process apps racing one table refuse at the guard with the rostered `LAKE_CONTENDED` row instead of surfacing as a late provider conflict after the loser's full write; the guard wraps the whole retry envelope and nothing queues behind it. The CROSS-process half is `Fence`, a monotone generation the caller carries: the Delta arm stamps it as a `CommitProperties` app transaction and `_fenced` proves it against `transaction_version(app_id)` ahead of the veto, so a stale holder refuses by name carrying both generations rather than learning it from an opaque provider conflict, and a handle carrying no fence commits exactly as before.
- Entry: the composition root's RECEIPT-DRAIN handler is `sink`'s one caller, named here exactly as `tabular/materialize#MATERIALIZE` names `register_data_hooks` as the package's one hook-registration fold: the root draining a scope's receipt stream calls `Lakehouse.sink(receipts, Residence.EVIDENCE, tenant=…, at=…)` carrying the tenancy and instant THAT drain observed, and that call is the residence's whole writer. No page inside this package invokes it — a commit running after the producing context moved stamps whatever tenancy happens to be active — so a composition binding no drain handler writes no evidence plane, which is the stated diagnosis rather than a silent gap. The journal window is the `runtime/observability/journal#LEDGER` port's own plane behind `tabular/journal#JOURNAL`, never a second ingest into this one.
- Receipt: the snapshot identity is one polymorphic `_snapshot` method discriminating `match self.table_format`, folded by one `_receipt` projector — never three sibling `_<format>_snapshot` factories nor a parallel `_SNAPSHOT` dispatch dict — and it reads the provider handle the ARM already opened, so a receipt costs no second log load and a travelling read keys on the version it pinned rather than on head. `LakeReceipt` keys by `ContentIdentity.of(DOMAIN, f"{table_uri}@{version}")`, which returns a rail the projector threads through `.map` so a digest fault propagates rather than a `Result` landing in the `content_key` slot; the `(table_uri, version)` payload pins the committed snapshot stable across a re-open of an unchanged version. `files_added`/`files_removed` carry the COMMIT's own churn off `_COMMIT_METRIC` — the snapshot's file roster counts every file the table holds, so reading it as an append's evidence prices one generation at the whole residence's size — while `byte_length` carries the volume that operation's own provider measures and `(quantity, unit)` the arm's own measure over the closed `LakeUnit` vocabulary, so a row count, a fragment count, and an expired-snapshot count each report as what they are rather than folding into a field named for files. Stock and flow never sum: the ledger slots every fact on the operation, so a `read` row carries the residence's held volume and a `write` row its written volume. `payload` carries the frame an op MOVES — the change feed alone today — because a count-only receipt forces its one row-consuming consumer to re-open the provider this owner already read. `contribute` emits `Receipt.of(OWNER, ("emitted", subject, facts))` whose counts ride as native `int` the `enc_hook=repr` renderer serializes without a pre-coerce.
- Receipt: `quantity` and `matched` split an upsert's LANDED rows from its REDELIVERED ones — Iceberg answers the pair natively off `UpsertResult.rows_inserted`/`rows_updated`, and the Delta arm reads `num_target_rows_inserted`/`num_target_rows_updated` because `num_output_rows` counts the rewritten output files — inserted, updated, and copied together — and so exceeds the offered batch whenever an untouched row shares a rewritten file; a consumer deriving duplicates by subtracting one fused tally from its own batch length reports zero forever.
- Receipt: `ReceiptFact` is the durable evidence row and `receipt_frame` its one projection, folding each receipt through the union's OWN `project()` so a new `Receipt` case reaches the residence with zero edits here. `EvidenceToken` states the evidence contract as a closed vocabulary — `domain`, `kind`, `owner`, `subject`, and `key` reach typed columns while every other fact survives verbatim in the open map, because the residence exists to keep the dimensions a metrics view cap drops. `_RECEIPT_COLUMNS` declares the durable row ONCE and the schema, the commit builder, and the row decoder all derive off it.
- Packages: `deltalake` owns the Delta arms — its `PostCommitHookProperties` and `TableAlterer.add_constraint` are MINED as `WriteTuning` hook fields and the `Evolve.constraints` clause, while `TableFeatures`/deletion-vector protocol enablement is DECLINED as the C# `Rasm.Persistence` at-rest concern; the predicate-bearing Delta read pushes SQL through the native `QueryBuilder` DataFusion surface, no SQL->pyarrow-DNF lowering owner minted. `pyiceberg` is the catalog-write fallback only (its `Table` annotation rides `TYPE_CHECKING`), gated behind the runtime lacking the core-loadable DuckDB `iceberg` read extension; `create_table_if_not_exists` is the idempotent create the `Ensure` arm plants through, `Table.update_spec`/`UpdateSpec.add_field` authoring the partition spec off name-keyed transform TOKENS the provider parses (`bucket[16]`, `truncate[4]`) and `Table.update_sort_order`/`UpdateSortOrder.asc`/`desc` the sort order, both gated on the table's own `spec()`/`sort_order()` reading empty. `pylance` owns the Lance dataset/version-travel/index arms and the predicate-scoped `LanceDataset.update` mutation; `pyarrow` is the write carrier. `tabular/columnar#SCAN` supplies the ONE session rail every DuckLake and Iceberg arm reuses — `DuckDbSession`/`DuckDbExtension`/`Attach` carrying the attach as session data beside the `SecretRow` credential rows the engine's own pushdown readers resolve through, `remote_store` the obstore-backed filesystem handle the tree writer registers, `quote_ident`/`quote_literal` the one escape rule, and `ColumnarEgress.Dataset` with its `emit`/`Landed` half the `PARQUET` write arm commits through, while `tabular/interop#INTEROP` supplies `arrow_bytes`, the folder's one serialization the tree's generation token digests; the `ducklake` and `iceberg` SQL surfaces are `data/.api/duckdb-extensions.md` rows [06] and [04], its `[04]-[DUCKLAKE]` cluster carrying every attach, snapshot, change-feed, and maintenance statement. runtime supplies `RuntimeRail`/`BoundaryFault`/`boundary`/`ContentIdentity`/`ReceiptContributor`/`Receipt` with the `FAULT_CONF`, `RetryClass.LAKE_COMMIT`, and `guarded_sync` the admission and commit rails bind.
- Growth: a new lake operation is one `LakeOp` case absorbed by the `(format, tag)` dispatch, naming its `LakeUnit` on the receipt; a new partition transform is one `PartitionTransform` member with its per-format projection row and, where its grammar carries one, one `_WIDTH_TRANSFORMS` entry, an absent projection row refusing that cell by name; a residence needing an authored table is one `ResidenceRow` `layout` value, the INGEST plan already carrying the arming; a newly counted quantity kind is one `LakeUnit` row; a new write mode a `Literal` row on `Write` beside its `_PARQUET_EXISTING` projection; a new codec a `Compression` row beside its `_PARQUET_CODEC` projection, an absent projection refusing that cell rather than downgrading it; a new writer-tuning knob a `WriteTuning` field; a newly reported commit metric one `_COMMIT_METRIC` row carrying the provider's own key spelling; a new residence lifecycle phase one `LakePhase` member with its `ops` arm, every entrypoint reading the plan it returns; a new Lance vector index kind a `VectorIndex` `Literal` row (a scalar/FTS kind a `ScalarIndex` row), both absorbed by the one `_VECTOR_INDEX`-routed `Index` arm; a new DuckDB-backed capability one `DuckDbExtension` row and its `(DUCKLAKE|ICEBERG, *)` SQL arm; a further table format (Hudi, Paimon) one `TableFormat` member with its `_ADMIT` row, its `_REFUSAL` rows, and its arms on this same owner; a new analytics residence one `_RESIDENCE` row naming its own partition and clustering rosters; a new lifted evidence column one `EvidenceToken` member beside its `_RECEIPT_COLUMNS` row, the lifted set, the Arrow field, the commit builder, and the row decoder all deriving; a new reference kind beyond tag and branch is one `Literal` member with its `_ICEBERG_REFERENCE` create/retire pair, the Lance arms taking it off the same discriminant; a new commit-governance concern is one subscriber the app root attaches on `LAKE_COMMIT_POINT`, zero owner edits; a further evidence-spine role a table can play is one `LakePlane` member the producer leg reads; a new demanded capability is one `Capability` member whose value IS its `LakeOp` tag, the reach matrix already rowing it; a new DDL statement is one `DdlStep` naming its verb, its idempotence, and its bound parameters.
- Law: a caller's commit that MOVED files lands durable evidence on the `python:runtime/observability/journal#LEDGER` plane — one operational `AuditFact` carrying the version arrival beside the file churn as a typed diff, plus a `STORAGE` `MeterFact` over the volume the arm's own provider measured. Two planes are excluded and neither is optional: a `LakePlane.LEDGER` handle holds the evidence ledger's OWN relations, so a fact recorded there lands through this very commit and the stream feeds itself without bound; a residence commit stores the receipt stream, so its fact drains as a receipt the root sinks back into that same residence — the identical regress one hop longer, and the reason `contribute` already meters no residence commit. `residence is None` alone discriminates neither, since a journal commit carries exactly that; the plane the handle admitted at `open` does, and it declares at the ledger rather than being inferred here.
- Boundary: analytics residences carry NO view cap and NO cardinality ceiling — the `cap` column is typed `False`, so the budget a metrics plane needs is unrepresentable here rather than merely discouraged.
- Boundary: no worker, scheduler, or retention executor enters for telemetry — `maintain` is the residence row's MAINTAIN plan and the deploy plane's own scheduled job supplies the cadence, so every expiry rides the residence's own mechanism and a row whose format refuses both passes derives an EMPTY plan rather than answering two refusals a scheduler reads as failure.
- Boundary: `sink` reads no baggage — tenant and observation instant arrive from the composition that drained the receipts, because a commit running after the producing context moved stamps whatever tenancy happens to be active.
- Boundary: no durable store, no schema evolution, no global Delta or catalog connection, no blocking commit run inline on an event loop where `run_async` owns the band hop, and no bare `trace.get_tracer(scope)` beside the faults-owned `scoped` stamp that binds the version and semconv triple; the metadata-only `Read` count is not the read lane — column-projected zero-copy reads route to the `tabular/columnar#SCAN` reader, not this commit owner. Reject law is data: `_REFUSAL` rows every `(format, tag)` cell a provider surface cannot portably reach and `_conditional` rows every cell the op's own operands decide, each row carrying its `LakeRefusal` member as the reason the fault reports, so a reject is a table edit and never an arm spending itself on a sentence. `_reach` reads that matrix ahead of the hook point and the retry envelope, `_apply`'s `case _, _` tail answers an admitted cell no arm executes, and every reject returns `Error(LAKE_REFUSED.raised(...))` carrying the operation beside the typed member — never a silent no-op, never a `raise` into a `boundary` that re-keys and discards it, and never a hand-opened `stamina.retry_context` where `guarded_sync` owns the envelope.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Iterator, Mapping
from contextlib import contextmanager
from copy import replace
from datetime import UTC, date, datetime, timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import lance
import pyarrow as pa
from anyio import BusyResourceError, ResourceGuard
import pyarrow.compute as pc
import pyarrow.dataset as pads
from beartype import beartype
from deltalake import (
    BloomFilterProperties,
    ColumnProperties,
    CommitProperties,
    DeltaTable,
    Transaction,
    PostCommitHookProperties,
    QueryBuilder,
    WriterProperties,
    write_deltalake,
)
from deltalake.exceptions import DeltaError
from deltalake.schema import Field
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field
from msgspec.json import decode as json_decode
from opentelemetry import trace

lazy from pyiceberg.catalog import load_catalog
lazy from pyiceberg.exceptions import CommitFailedException, ValidationError as IcebergError
lazy from pyiceberg.transforms import IdentityTransform
lazy from pyiceberg.types import IcebergType

from rasm.data.tabular.columnar import (
    Attach,
    ColumnarEgress,
    DatasetKind,
    DatasetRef,
    DatasetWrite,
    DdlStep,
    DdlVerb,
    DuckDbExtension,
    DuckDbSession,
    Idempotence,
    SecretRow,
    quote_ident,
    quote_literal,
    remote_store,
)
from rasm.data.tabular.interop import ColumnSpec, DataLeg, arrow_bytes, column_frame, column_rows, column_schema
from rasm.runtime.faults import (
    FAULT_CONF,
    TERMINAL,
    TRANSIENT,
    Catch,
    Depth,
    Disposition,
    FaultRow,
    Posture,
    RuntimeRail,
    async_boundary,
    boundary,
    rostered,
    scoped,
    traversed,
)
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, MeterFact, Party, Resource, Retain, Shifted
from rasm.runtime.metrics import Metrics
from rasm.runtime.lanes import on_thread
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded, guarded_sync

if TYPE_CHECKING:
    import duckdb
    from pyiceberg.table import Table

# --- [TYPES] ----------------------------------------------------------------------------

type WriteMode = Literal["error", "append", "overwrite", "ignore"]
type LanceMode = Literal["create", "overwrite", "append"]
type LanceStorage = Literal["stable", "2.1", "next"]
type Compression = Literal["UNCOMPRESSED", "SNAPPY", "GZIP", "BROTLI", "LZ4", "LZ4_RAW", "ZSTD"]
type VectorIndex = Literal["IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"]
type ScalarIndex = Literal["BTREE", "BITMAP", "LABEL_LIST", "ZONEMAP", "BLOOMFILTER", "RTREE", "INVERTED", "FTS", "NGRAM"]
type IndexKind = VectorIndex | ScalarIndex
type Metric = Literal["L2", "cosine", "dot"]
type Evolution = tuple[tuple[tuple[str, str], ...], tuple[str, ...], tuple[tuple[str, str], ...], Map[str, str]]
type Partition = tuple[tuple[str, str, str], ...]
type PartitionTransform = Literal["identity", "year", "month", "day", "hour", "bucket", "truncate"]
_WIDTH_TRANSFORMS: Final[frozenset[PartitionTransform]] = frozenset({"bucket", "truncate"})
type SortDirection = Literal["asc", "desc"]


class TableFormat(StrEnum):
    DELTA = "delta"
    ICEBERG = "iceberg"
    LANCE = "lance"
    DUCKLAKE = "ducklake"
    PARQUET = "parquet"


class LakeUnit(StrEnum):
    ROWS = "rows"
    FILES = "files"
    FRAGMENTS = "fragments"
    SNAPSHOTS = "snapshots"
    NONE = "none"


class Capability(StrEnum):
    ENSURE = "ensure"
    WRITE = "write"
    READ = "read"
    DELETE = "delete"
    UPDATE = "update"
    MERGE = "merge"
    EVOLVE = "evolve"
    OPTIMIZE = "optimize"
    VACUUM = "vacuum"
    CHANGEFEED = "changefeed"
    INDEX = "index"
    RESTORE = "restore"
    REFERENCE = "reference"
    ANCESTRY = "ancestry"


class Fence(Struct, frozen=True):
    app_id: str
    expected: int

    def stale(self, held: int) -> "Option[BoundaryFault]":
        return Nothing if held == self.expected else Some(LAKE_STALE_FENCE.raised(str(self.expected), str(held)))


class Generation(Struct, frozen=True):
    version: int
    parent: "Posture[int]"
    at: "Posture[datetime]"
    refs: tuple[str, ...] = ()


class LakeRefusal(StrEnum):
    DELTA_NO_INDEX = "delta reaches no vector or scalar index surface"
    DELTA_COLUMN_SURGERY = "delta alter reaches no portable column drop or rename"
    DELTA_NO_REFERENCE = "delta names no branch or tag; an int version is its whole reference vocabulary"
    DUCKLAKE_NO_REFERENCE = "ducklake catalogs name a snapshot by id or timestamp alone; no ref vocabulary"
    ICEBERG_NO_UPDATE = "pyiceberg reaches no predicate-scoped row update"
    ICEBERG_NO_OPTIMIZE = "pyiceberg reaches no rewrite_data_files compaction"
    ICEBERG_NO_CHANGEFEED = "pyiceberg reaches no change-data feed"
    ICEBERG_NO_INDEX = "iceberg reaches no index surface"
    ICEBERG_PARTITION_SPEC = "partition_by is table-spec-owned; author PartitionSpec at create"
    ICEBERG_WRITE_EXISTS = "error mode forbids a write into an existing table"
    ICEBERG_CONSTRAINTS = "constraint governance is delta alter.add_constraint only"
    ICEBERG_REF_ABSENT = "the iceberg table resolves no snapshot under that ref, instant, or head"
    LANCE_NO_EVOLVE = "lance reaches no schema-evolution surface"
    LANCE_NO_CHANGEFEED = "lance reaches no change-data feed"
    LANCE_NO_ZORDER = "lance compaction reaches no z-order clustering"
    DUCKLAKE_NO_EVOLVE = "ducklake reaches no schema-evolution surface"
    DUCKLAKE_NO_INDEX = "ducklake reaches no index surface"
    DUCKLAKE_NO_RESTORE = "ducklake reaches no snapshot restore"
    DUCKLAKE_OPTIMIZE_TUNING = "ducklake file merge reaches no target size or z-order"
    DUCKLAKE_READ_TAG = "ducklake travel takes a version int or a timestamp; no tag vocabulary"
    PARQUET_NO_TRANSACTION = "the parquet tree commits no transaction; a correction rewrites its partition whole"
    PARQUET_NO_EVOLVE = "the parquet tree reaches no alter surface; a widened generation carries its own schema"
    PARQUET_NO_OPTIMIZE = "the parquet tree compacts by rewriting a partition through its own write"
    PARQUET_NO_VACUUM = "object-plane lifecycle owns cold-tail expiry; this format holds no snapshot to expire"
    PARQUET_NO_CHANGEFEED = "the parquet tree records no row-level change feed"
    PARQUET_NO_INDEX = "the parquet tree reaches no index surface beyond its own file statistics"
    PARQUET_NO_TRAVEL = "the parquet tree keeps no version history; every read sees the tree as it stands"
    PARQUET_READ_SCOPE = "the tree count takes no version or predicate; a scoped read routes to the columnar scan"
    PARQUET_CODEC = "the tree writer reaches no lz4-raw codec; the transactional formats alone carry it"
    LANCE_NO_PARTITION_FILTER = "lance compaction reaches no partition predicate; it compacts the dataset whole"
    DUCKLAKE_OPTIMIZE_PARTITION = "ducklake file merge reaches no partition predicate"
    PARQUET_NO_TABLE_SPEC = "the parquet tree records no table object; a generation carries its own schema"
    DELTA_PARTITION_TRANSFORM = "delta partitions on columns; every non-identity transform is iceberg-only"
    DUCKLAKE_PARTITION_TRUNCATE = "ducklake partition functions reach year, month, day, hour, and bucket alone"
    LANCE_NO_TABLE_SPEC = "lance authors no partition spec or sort order; the dataset carries neither"
    PARTITION_TRANSFORM_WIDTH = "bucket and truncate each declare a width; every other transform declares none"
    SORT_ORDER_CATALOG_ONLY = "a declared sort order authors at the iceberg catalog alone"
    RESIDENCE_FORMAT_MISMATCH = "the residence row names a table format this handle did not open"
    RESIDENCE_KIND_MISMATCH = "the residence row names a source shape this handle did not open"
    UNARMED = "reach admits the cell yet no arm executes it"


class LakePhase(StrEnum):
    INGEST = "ingest"
    MAINTAIN = "maintain"


class LakePlane(StrEnum):
    CALLER = "caller"
    LEDGER = "ledger"


class Residence(StrEnum):
    EVIDENCE = "evidence"
    TABLE = "table"
    COLD = "cold"


# --- [MODELS] ---------------------------------------------------------------------------


class WriteTuning(Struct, frozen=True):
    compression: Compression = "ZSTD"
    statistics_truncate_length: int | None = None
    target_file_size: int | None = None
    bloom_columns: tuple[str, ...] = ()
    custom_metadata: Map[str, str] = field(default_factory=lambda: Map.of_seq([]))
    max_commit_retries: int | None = None
    create_checkpoint: bool = True
    cleanup_expired_logs: bool = True
    data_storage_version: LanceStorage = "stable"

    def writer_properties(self) -> WriterProperties:
        return WriterProperties(
            compression=self.compression,
            statistics_truncate_length=self.statistics_truncate_length,
            column_properties={
                column: ColumnProperties(bloom_filter_properties=BloomFilterProperties(set_bloom_filter_enabled=True))
                for column in self.bloom_columns
            }
            or None,
        )

    def commit_properties(self) -> CommitProperties:
        return CommitProperties(custom_metadata=dict(self.custom_metadata.items()) or None, max_commit_retries=self.max_commit_retries)

    def hook_properties(self) -> PostCommitHookProperties:
        return PostCommitHookProperties(create_checkpoint=self.create_checkpoint, cleanup_expired_logs=self.cleanup_expired_logs)


@tagged_union(frozen=True)
class LakeOp:
    tag: Literal[
        "ensure", "write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "index", "restore", "reference"
    ] = tag()
    ensure: "TableLayout" = case()
    write: tuple[WriteMode, tuple[str, ...], bool, WriteTuning] = case()
    read: tuple[int | str | datetime | None, tuple[str, ...], str | None] = case()
    delete: tuple[str] = case()
    update: tuple[str, Map[str, str]] = case()
    merge: tuple[str, Map[str, str], bool] = case()
    evolve: Evolution = case()
    optimize: tuple[int | None, tuple[str, ...], Partition] = case()
    vacuum: tuple[int | None, bool] = case()
    changefeed: tuple[int, int | None] = case()
    index: tuple[str, IndexKind, Metric] = case()
    restore: tuple[int | datetime] = case()
    reference: tuple[Literal["tag", "branch"], str, int | None, bool] = case()
    ancestry: tuple[Depth] = case()

    @property
    def committing(self) -> bool:
        match self:
            case LakeOp(tag="read") | LakeOp(tag="changefeed") | LakeOp(tag="ancestry") | LakeOp(tag="vacuum", vacuum=(_, True)):
                return False
            case LakeOp():
                return True

    @property
    def feeding(self) -> bool:
        match self:
            case LakeOp(tag="write") | LakeOp(tag="merge"):
                return True
            case LakeOp():
                return False

    @staticmethod
    def Ensure(layout: "TableLayout") -> "LakeOp":
        return LakeOp(ensure=layout)

    @staticmethod
    def Ancestry(bound: Depth = Depth(fixpoint=None)) -> "LakeOp":
        return LakeOp(ancestry=(bound,))

    @staticmethod
    def Write(
        mode: WriteMode = "error", partition_by: tuple[str, ...] = (), evolve_schema: bool = False, tuning: WriteTuning = WriteTuning()
    ) -> "LakeOp":
        return LakeOp(write=(mode, partition_by, evolve_schema, tuning))

    @staticmethod
    def Read(version: int | str | datetime | None = None, columns: tuple[str, ...] = (), predicate: str | None = None) -> "LakeOp":
        return LakeOp(read=(version, columns, predicate))

    @staticmethod
    def Delete(predicate: str) -> "LakeOp":
        return LakeOp(delete=(predicate,))

    @staticmethod
    def Update(predicate: str, updates: Map[str, str]) -> "LakeOp":
        return LakeOp(update=(predicate, updates))

    @staticmethod
    def Merge(predicate: str, updates: Map[str, str], delete_unmatched: bool = False) -> "LakeOp":
        return LakeOp(merge=(predicate, updates, delete_unmatched))

    @staticmethod
    def Evolve(
        adds: tuple[tuple[str, str], ...] = (),
        drops: tuple[str, ...] = (),
        renames: tuple[tuple[str, str], ...] = (),
        constraints: Map[str, str] | None = None,
    ) -> "LakeOp":
        return LakeOp(evolve=(adds, drops, renames, constraints if constraints is not None else Map.of_seq([])))

    @staticmethod
    def Optimize(target_size: int | None = None, zorder: tuple[str, ...] = (), partition: Partition = ()) -> "LakeOp":
        return LakeOp(optimize=(target_size, zorder, partition))

    @staticmethod
    def Vacuum(retention_hours: int | None = None, dry_run: bool = True) -> "LakeOp":
        return LakeOp(vacuum=(retention_hours, dry_run))

    @staticmethod
    def ChangeFeed(starting_version: int = 0, ending_version: int | None = None) -> "LakeOp":
        return LakeOp(changefeed=(starting_version, ending_version))

    @staticmethod
    def Index(column: str, kind: IndexKind = "IVF_PQ", metric: Metric = "L2") -> "LakeOp":
        return LakeOp(index=(column, kind, metric))

    @staticmethod
    def Restore(target: int | datetime) -> "LakeOp":
        return LakeOp(restore=(target,))

    @staticmethod
    def Reference(kind: Literal["tag", "branch"], name: str, target: int | None = None, drop: bool = False) -> "LakeOp":
        return LakeOp(reference=(kind, name, target, drop))


class LakeCommit(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    operation: str


# --- [RESIDENCE]

class EvidenceToken(StrEnum):
    DOMAIN = "domain"
    KIND = "kind"
    OWNER = "owner"
    SUBJECT = "subject"
    KEY = "key"


_LIFTED: Final[frozenset[str]] = frozenset(EvidenceToken)


class ReceiptFact(Struct, frozen=True):
    at: datetime
    date: date
    domain: str
    kind: str
    owner: str
    phase: str
    subject: str
    tenant: str
    content_key: str
    facts: Map[str, str]
    fact_keys: tuple[str, ...]


class TableLayout(Struct, frozen=True):
    schema: pa.Schema
    partition_by: tuple[tuple[str, PartitionTransform, int | None], ...] = ()
    sort_by: tuple[tuple[str, SortDirection], ...] = ()
    properties: Map[str, str] = field(default_factory=lambda: Map.of_seq([]))

    @property
    def columns(self) -> tuple[str, ...]:
        return tuple(column for column, _transform, _width in self.partition_by)

    @property
    def identity_only(self) -> bool:
        return all(transform == "identity" for _column, transform, _width in self.partition_by)

    @property
    def widths_paired(self) -> bool:
        return all((width is not None) is (transform in _WIDTH_TRANSFORMS) for _column, transform, width in self.partition_by)

    def iceberg_fields(self) -> "Block[tuple[str, str]]":
        return Block.of_seq(
            (column, transform if width is None else f"{transform}[{width}]") for column, transform, width in self.partition_by
        )

    def ducklake_terms(self) -> "Block[str]":
        return Block.of_seq(
            quote_ident(column)
            if transform == "identity"
            else f"{transform}({quote_ident(column)})"
            if width is None
            else f"{transform}({width}, {quote_ident(column)})"
            for column, transform, width in self.partition_by
        )

    def empty(self) -> pa.Table:
        return self.schema.empty_table()


class ResidenceRow(Struct, frozen=True):
    kind: DatasetKind
    table_format: TableFormat
    domain: str
    partition_by: tuple[str, ...]
    zorder: tuple[str, ...]
    tuning: WriteTuning
    tenancy: str
    lifetime: str
    lifetime_hours: int | None
    fits: str
    admit: str
    layout: TableLayout | None = None
    cap: Literal[False] = False

    @property
    def degrade(self) -> tuple[LakeRefusal, ...]:
        return tuple(refusal for (fmt, _tag), refusal in _REFUSAL.items() if fmt is self.table_format)

    def ops(self, phase: LakePhase, partition: Partition = ()) -> tuple[LakeOp, ...]:
        match phase:
            case LakePhase.INGEST:
                armed = (LakeOp.Ensure(self.layout),) if self.layout is not None else ()
                planned = (*armed, LakeOp.Write(mode="append", partition_by=self.partition_by, evolve_schema=True, tuning=self.tuning))
            case LakePhase.MAINTAIN:
                clustered = (LakeOp.Optimize(self.tuning.target_file_size, self.zorder, partition),) if self.zorder else ()
                planned = (*clustered, LakeOp.Vacuum(self.lifetime_hours, dry_run=False))
            case unreachable:
                assert_never(unreachable)
        return tuple(op for op in planned if _REFUSAL.try_find((self.table_format, op.tag)).is_none())


LAKE_COMMIT_POINT: Final[HookPoint[LakeCommit]] = HookPoint(id="rasm.data.lakehouse.commit", payload=LakeCommit, modality=Modality(veto=None))

OWNER: Final[str] = "lakehouse"
DOMAIN: Final[str] = "lake"


class LakeReceipt(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    operation: str
    version: "Option[int]"
    files_added: "Option[int]"
    files_removed: "Option[int]"
    byte_length: int
    quantity: int
    unit: LakeUnit
    matched: int
    content_key: ContentKey
    payload: pa.Table | None = None
    residence: Residence | None = None

    def contribute(self) -> Iterable[Receipt]:
        churn = self.files_added.default_value(0) + self.files_removed.default_value(0)
        if churn and self.residence is None:
            Metrics.record(
                {
                    "rasm.lake.commit.files_added": float(self.files_added.default_value(0)),
                    "rasm.lake.commit.files_removed": float(self.files_removed.default_value(0)),
                },
                domain=DOMAIN,
                kind=self.operation,
            )
        yield Receipt.of(
            OWNER,
            (
                "emitted",
                self.table_uri,
                {
                    "domain": _RESIDENCE[self.residence].domain if self.residence is not None else DOMAIN,
                    "kind": self.operation,
                    "key": self.content_key.hex,
                    "format": self.table_format,
                    "version": self.version.map(str).default_value("unarmed"),
                    "added": self.files_added.default_value(0),
                    "removed": self.files_removed.default_value(0),
                    "bytes": self.byte_length,
                    "quantity": self.quantity,
                    "unit": self.unit,
                },
            ),
        )


def _evidence(receipt: LakeReceipt, plane: LakePlane) -> Block[Fact]:
    churn = receipt.files_added.default_value(0) + receipt.files_removed.default_value(0)
    if plane is not LakePlane.CALLER or receipt.residence is not None or not churn:
        return Block.empty()
    audited = AuditFact(
        action=f"{DOMAIN}.{receipt.operation}",
        actor=Party(kind=Actor.SERVICE, key=OWNER),
        target=Party(kind="table", key=receipt.table_uri),
        retention=Retain.OPERATIONAL,
        change=(
            Assigned(path="/version", next=receipt.version.map(str).default_value("unarmed")),
            Shifted(path="/files", prior=str(receipt.files_removed.default_value(0)), next=str(receipt.files_added.default_value(0))),
        ),
    )
    metered = MeterFact(resource=Resource.STORAGE, quantity=receipt.byte_length, surface=receipt.table_uri)
    return Block.of_seq((audited, metered) if receipt.byte_length else (audited,))


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.tabular.lakehouse")


class Lakehouse(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    kind: DatasetKind
    catalog: str | None = None
    identifier: str | None = None
    dsn: str | None = None
    filesystem: Any | None = None
    secrets: tuple[SecretRow, ...] = ()
    scope: ScopeKey = DEFAULT_SCOPE
    plane: LakePlane = LakePlane.CALLER
    guard: ResourceGuard = field(default_factory=lambda: ResourceGuard("committing"))
    fence: "Option[Fence]" = Nothing

    @classmethod
    @beartype(conf=FAULT_CONF)
    def open(
        cls,
        dataset: DatasetRef,
        table_format: TableFormat = TableFormat.DELTA,
        *,
        catalog: str | None = None,
        identifier: str | None = None,
        dsn: str | None = None,
        secrets: tuple[SecretRow, ...] = (),
        scope: ScopeKey = DEFAULT_SCOPE,
        plane: LakePlane = LakePlane.CALLER,
        fence: "Option[Fence]" = Nothing,
    ) -> "RuntimeRail[Lakehouse]":
        row = _ADMIT[table_format]
        coordinates = {"catalog": catalog, "identifier": identifier, "dsn": dsn}
        missing = tuple(name for name in row.needs if coordinates[name] is None)
        if row.kinds and dataset.kind not in row.kinds:
            return Error(LAKE_ADMIT_KIND.raised(table_format.value, dataset.kind.value, dataset.ref.relative))
        if missing:
            return Error(LAKE_ADMIT_NEEDS.raised(table_format.value, "-".join(missing), dataset.ref.relative))
        return Ok(
            cls(
                table_uri=str(dataset.ref.path),
                table_format=table_format,
                kind=dataset.kind,
                catalog=catalog,
                identifier=identifier,
                dsn=dsn,
                filesystem=remote_store(dataset.ref),
                secrets=secrets,
                scope=scope,
                plane=plane,
                fence=fence,
            )
        )

    @beartype(conf=FAULT_CONF)
    def sink(
        self,
        receipts: Iterable[Receipt],
        residence: Residence = Residence.EVIDENCE,
        *,
        tenant: str,
        at: datetime,
    ) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        return self._admitted(residence).bind(
            lambda row: receipt_frame(receipts, tenant=tenant, at=at).bind(
                lambda frame: self._folded(row.ops(LakePhase.INGEST), residence, frame) if frame.num_rows else Ok(())
            )
        )

    @beartype(conf=FAULT_CONF)
    async def sink_async(
        self,
        receipts: Iterable[Receipt],
        residence: Residence = Residence.EVIDENCE,
        *,
        tenant: str,
        at: datetime,
    ) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        match self._admitted(residence).bind(lambda row: receipt_frame(receipts, tenant=tenant, at=at).map(lambda frame: (row, frame))):
            case Result(tag="error", error=refused):
                return Error(refused)
            case Result(tag="ok", ok=(row, frame)):
                return await self._drained(row.ops(LakePhase.INGEST), residence, frame) if frame.num_rows else Ok(())
            case unreachable:
                assert_never(unreachable)

    @beartype(conf=FAULT_CONF)
    def maintain(self, residence: Residence = Residence.EVIDENCE, *, partition: Partition = ()) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        return self._admitted(residence).bind(lambda row: self._folded(row.ops(LakePhase.MAINTAIN, partition), residence, None))

    def _admitted(self, residence: Residence) -> "RuntimeRail[ResidenceRow]":
        row = _RESIDENCE[residence]
        mismatch = (
            Some(LakeRefusal.RESIDENCE_FORMAT_MISMATCH)
            if row.table_format is not self.table_format
            else Some(LakeRefusal.RESIDENCE_KIND_MISMATCH)
            if row.kind is not self.kind
            else Nothing
        )
        return mismatch.map(lambda refusal: Error(LAKE_REFUSED.raised(residence.value, refusal.value))).default_value(Ok(row))

    def _folded(self, ops: tuple[LakeOp, ...], residence: Residence, frame: pa.Table | None) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        return Block.of_seq(ops).fold(
            lambda landed, op: landed.bind(
                lambda rows: self.run(op, frame if op.tag == "write" else None).map(
                    lambda receipt: (*rows, replace(receipt, residence=residence))
                )
            ),
            Ok(()),
        )

    async def _drained(self, ops: tuple[LakeOp, ...], residence: Residence, frame: pa.Table | None) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        match ops:
            case ():
                return Ok(())
            case (op, *rest):
                match await self.run_async(op, frame if op.tag == "write" else None):
                    case Result(tag="error", error=refused):
                        return Error(refused)
                    case Result(tag="ok", ok=receipt):
                        return (await self._drained(tuple(rest), residence, frame)).map(
                            lambda rows: (replace(receipt, residence=residence), *rows)
                        )
                    case unreachable:
                        assert_never(unreachable)

    @beartype(conf=FAULT_CONF)
    def run(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        subject = self._subject(op)
        with _TRACER.start_as_current_span(subject, attributes=self._dimensions(op)):
            return self._gated(op, data).bind(
                lambda admitted: (
                    self._exclusive(lambda: guarded_sync(RetryClass.LAKE_COMMIT, self._apply, admitted, data, at=LAKE_COMMIT), LAKE_COMMIT)
                    if admitted.committing
                    else boundary(LAKE_READ, lambda: self._apply(admitted, data), catch=_commit_raises())
                ).bind(lambda rail: rail)
            )

    def _exclusive[T](self, commit: "Callable[[], RuntimeRail[T]]", at: "FaultRow[DataLeg]") -> "RuntimeRail[T]":
        try:
            with self.guard:
                return commit()
        except BusyResourceError:
            return Error(LAKE_CONTENDED.raised(at.point))

    async def _exclusive_async[T](self, commit: "Callable[[], Awaitable[RuntimeRail[T]]]", at: "FaultRow[DataLeg]") -> "RuntimeRail[T]":
        try:
            with self.guard:
                return await commit()
        except BusyResourceError:
            return Error(LAKE_CONTENDED.raised(at.point))

    @beartype(conf=FAULT_CONF)
    async def run_async(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        subject = self._subject(op)
        with _TRACER.start_as_current_span(subject, attributes=self._dimensions(op)):
            match self._gated(op, data):
                case Result(tag="error", error=fault):
                    return Error(fault)
                case Result(tag="ok", ok=admitted):
                    fenced = (
                        await self._exclusive_async(
                            lambda: guarded(RetryClass.LAKE_COMMIT, on_thread, self._apply, admitted, data, at=LAKE_COMMIT), LAKE_COMMIT
                        )
                        if admitted.committing
                        else await async_boundary(LAKE_READ, lambda: on_thread(self._apply, admitted, data), catch=_commit_raises())
                    )
                    match fenced.bind(lambda rail: rail):
                        case Result(tag="ok", ok=receipt):
                            return (await Journal.record(_evidence(receipt, self.plane), scope=self.scope)).map(lambda _landed: receipt)
                        case refused:
                            return Error(refused.error)
                case unreachable:
                    assert_never(unreachable)

    def _gated(self, op: LakeOp, data: pa.Table | None) -> "RuntimeRail[LakeOp]":
        if op.feeding and data is None:
            return Error(LAKE_PAYLOAD.raised(op.tag))
        return self._reach(op).bind(self._fenced).bind(
            lambda admitted: Hooks.fire(
                LAKE_COMMIT_POINT.id,
                LakeCommit(table_uri=self.table_uri, table_format=self.table_format, operation=admitted.tag),
                scope=self.scope,
            ).map(lambda _fact: admitted)
            if admitted.committing
            else Ok(admitted)
        )

    def _fenced(self, op: LakeOp) -> "RuntimeRail[LakeOp]":
        return self.fence.bind(
            lambda held: held.stale(_transaction_version(self.table_uri, held.app_id)).map(Error)
        ).default_value(Ok(op)) if op.committing else Ok(op)

    def _subject(self, op: LakeOp) -> str:
        return f"lake.{self.table_format}.{op.tag}"

    def _dimensions(self, op: LakeOp) -> dict[str, str]:
        return {"rasm.lake.format": self.table_format.value, "rasm.lake.op": op.tag}

    @beartype(conf=FAULT_CONF)
    def demand(self, capabilities: "Block[Capability]") -> "RuntimeRail[Block[Capability]]":
        return traversed(capabilities.map(self._served), by=Disposition.ACCUMULATE).map(lambda _held: capabilities)

    def _served(self, capability: Capability) -> "RuntimeRail[Capability]":
        return (
            _REFUSAL.try_find((self.table_format, capability.value))
            .map(lambda refusal: Error(LAKE_REFUSED.raised(capability.value, refusal.value)))
            .default_value(Ok(capability))
        )

    def _reach(self, op: LakeOp) -> "RuntimeRail[LakeOp]":
        return (
            _REFUSAL.try_find((self.table_format, op.tag))
            .or_else_with(lambda: _conditional(self.table_format, op))
            .map(lambda refusal: Error(LAKE_REFUSED.raised(op.tag, refusal.value)))
            .default_value(Ok(op))
        )

    def _apply(self, op: LakeOp, data: pa.Table | None) -> "RuntimeRail[LakeReceipt]":
        match self.table_format, op:
            case TableFormat.DELTA, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)) if DeltaTable.is_deltatable(self.table_uri):
                return self._receipt(op, added=Posture(declared=0), removed=Posture(declared=0), byte_length=Posture(declared=0))
            case TableFormat.DELTA, LakeOp(tag="ensure", ensure=layout):
                table = DeltaTable.create(
                    self.table_uri,
                    layout.schema,
                    mode="ignore",
                    partition_by=list(layout.columns) or None,
                    configuration=dict(layout.properties.items()) or None,
                )
                return self._receipt(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="write", write=(mode, partition_by, evolve, tuning)):
                prior = _add_paths(self.table_uri)
                write_deltalake(
                    self.table_uri,
                    data,
                    mode=mode,
                    partition_by=list(partition_by) or None,
                    schema_mode="merge" if evolve else None,
                    target_file_size=tuning.target_file_size,
                    writer_properties=tuning.writer_properties(),
                    commit_properties=_fenced_properties(tuning.commit_properties(), self.fence),
                    post_commithook_properties=tuning.hook_properties(),
                )
                table = DeltaTable(self.table_uri)
                return self._receipt(op, byte_length=Posture(declared=_added_bytes(table, prior)), handle=table)
            case TableFormat.DELTA, LakeOp(tag="read", read=(version, _columns, predicate)):
                table = DeltaTable(self.table_uri)
                if version is not None:
                    table.load_as_version(version)
                rows = (
                    QueryBuilder().register("t", table).execute(f"SELECT count(*) AS n FROM t WHERE {predicate}").read_all().column("n")[0].as_py()
                    if predicate
                    else table.to_pyarrow_dataset().count_rows()
                )
                actions = pa.table(table.get_add_actions())
                stored = int(pc.sum(actions.column("size_bytes")).as_py() or 0)
                return self._receipt(op, quantity=rows, unit=LakeUnit.ROWS, byte_length=Posture(declared=stored), handle=table)
            case TableFormat.DELTA, LakeOp(tag="delete", delete=(predicate,)):
                table = DeltaTable(self.table_uri)
                metrics = table.delete(predicate)
                deleted = _delta_metric(metrics, "num_deleted_rows").option().default_value(0)
                return self._receipt(op, quantity=deleted, unit=LakeUnit.ROWS, handle=table)
            case TableFormat.DELTA, LakeOp(tag="update", update=(predicate, updates)):
                table = DeltaTable(self.table_uri)
                metrics = table.update(updates=dict(updates.items()), predicate=predicate)
                updated = _delta_metric(metrics, "num_updated_rows").option().default_value(0)
                return self._receipt(op, quantity=updated, unit=LakeUnit.ROWS, handle=table)
            case TableFormat.DELTA, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                clauses = dict(updates.items())
                table = DeltaTable(self.table_uri)
                merger = table.merge(data, predicate=predicate).when_matched_update(updates=clauses).when_not_matched_insert(updates=clauses)
                metrics = (merger.when_not_matched_by_source_delete() if delete_unmatched else merger).execute()
                return self._receipt(
                    op,
                    quantity=_delta_metric(metrics, "num_target_rows_inserted").option().default_value(0),
                    unit=LakeUnit.ROWS,
                    matched=_delta_metric(metrics, "num_target_rows_updated").option().default_value(0),
                    handle=table,
                )
            case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(adds, _drops, _renames, constraints)):
                table = DeltaTable(self.table_uri)
                if adds:
                    table.alter.add_columns([Field(name, dtype) for name, dtype in adds])
                if constraints:
                    table.alter.add_constraint(dict(constraints.items()))
                return self._receipt(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="optimize", optimize=(target_size, zorder, partition)):
                table = DeltaTable(self.table_uri)
                filters = [(column, operator, value) for column, operator, value in partition] or None
                (
                    table.optimize.z_order(list(zorder), partition_filters=filters, target_size=target_size)
                    if zorder
                    else table.optimize.compact(partition_filters=filters, target_size=target_size)
                )
                return self._receipt(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                table = DeltaTable(self.table_uri)
                removed = table.vacuum(retention_hours=retention_hours, dry_run=dry_run)
                return self._receipt(op, removed=Posture(declared=len(removed)), quantity=len(removed), unit=LakeUnit.FILES, handle=table)
            case TableFormat.DELTA, LakeOp(tag="changefeed", changefeed=(start, end)):
                table = DeltaTable(self.table_uri)
                feed = pa.table(table.load_cdf(starting_version=start, ending_version=end).read_all())
                return self._receipt(op, quantity=feed.num_rows, unit=LakeUnit.ROWS, payload=feed, handle=table)
            case TableFormat.DELTA, LakeOp(tag="restore", restore=(target,)):
                table = DeltaTable(self.table_uri)
                metrics = table.restore(target)
                return self._receipt(
                    op,
                    added=_delta_metric(metrics, "numRestoredFile"),
                    removed=_delta_metric(metrics, "numRemovedFile"),
                    quantity=_delta_metric(metrics, "numRestoredFile").option().default_value(0),
                    unit=LakeUnit.FILES,
                    handle=table,
                )
            case TableFormat.ICEBERG, LakeOp(tag="ensure", ensure=layout):
                table = load_catalog(self.catalog).create_table_if_not_exists(
                    self.identifier, layout.schema, properties=dict(layout.properties.items())
                )
                if layout.partition_by and not table.spec().fields:
                    layout.iceberg_fields().fold(lambda spec, row: spec.add_field(*row), table.update_spec()).commit()
                if layout.sort_by and not table.sort_order().fields:
                    Block.of_seq(layout.sort_by).fold(
                        lambda order, row: order.asc(row[0], IdentityTransform()) if row[1] == "asc" else order.desc(row[0], IdentityTransform()),
                        table.update_sort_order(),
                    ).commit()
                return self._receipt(op, handle=table.refresh())
            case TableFormat.ICEBERG, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)):
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                txn = self._iceberg().transaction()
                txn.overwrite(data) if mode == "overwrite" else txn.append(data)
                txn.commit_transaction()
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(None, _columns, predicate)):
                with DuckDbSession(extensions=(DuckDbExtension.ICEBERG,), secrets=self.secrets).connect() as con:
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM iceberg_scan({quote_literal(self.table_uri)}){where}").fetchone()[0]
                return self._receipt(op, quantity=int(rows), unit=LakeUnit.ROWS)
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(version, _columns, predicate)):
                table = self._iceberg()
                match _iceberg_snapshot(table, version):
                    case Option(tag="none"):
                        return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.ICEBERG_REF_ABSENT.value))
                    case Option(tag="some", some=snapshot_id):
                        counted = table.scan(row_filter=predicate or "true", snapshot_id=snapshot_id).count()
                        return self._receipt(op, quantity=counted, unit=LakeUnit.ROWS, pinned=Posture(declared=snapshot_id), handle=table)
            case TableFormat.ICEBERG, LakeOp(tag="delete", delete=(predicate,)):
                txn = self._iceberg().transaction()
                txn.delete(predicate)
                txn.commit_transaction()
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                txn = self._iceberg().transaction()
                outcome = txn.upsert(data, join_cols=list(updates.keys()))
                txn.commit_transaction()
                return self._receipt(op, quantity=outcome.rows_inserted, unit=LakeUnit.ROWS, matched=outcome.rows_updated)
            case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(adds, drops, renames, _constraints)):
                with self._iceberg().update_schema() as schema:
                    for name, dtype in adds:
                        schema.add_column(name, IcebergType.model_validate(dtype))
                    for column in drops:
                        schema.delete_column(column)
                    for old, new in renames:
                        schema.rename_column(old, new)
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="restore", restore=(target,)):
                manage = self._iceberg().manage_snapshots()
                (manage.rollback_to_snapshot(target) if isinstance(target, int) else manage.rollback_to_timestamp(_millis(target))).commit()
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="reference", reference=(kind, name, target, False)):
                table = self._iceberg()
                match _iceberg_head(table, target):
                    case Option(tag="none"):
                        return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.ICEBERG_REF_ABSENT.value))
                    case Option(tag="some", some=snapshot_id):
                        getattr(table.manage_snapshots(), _ICEBERG_REFERENCE[(kind, False)])(snapshot_id, name).commit()
                        return self._receipt(op, handle=table.refresh())
            case TableFormat.ICEBERG, LakeOp(tag="reference", reference=(kind, name, _target, True)):
                table = self._iceberg()
                getattr(table.manage_snapshots(), _ICEBERG_REFERENCE[(kind, True)])(name).commit()
                return self._receipt(op, handle=table.refresh())
            case TableFormat.ICEBERG, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                cutoff = _retention(retention_hours)
                table = self._iceberg()
                aged = sum(1 for committed in table.inspect.snapshots().column("committed_at").to_pylist() if committed < cutoff)
                if not dry_run:
                    table.maintenance.expire_snapshots().older_than(cutoff).commit()
                return self._receipt(op, quantity=aged, unit=LakeUnit.SNAPSHOTS)
            case TableFormat.LANCE, LakeOp(tag="ensure", ensure=_layout) if _lance_exists(self.table_uri):
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="ensure", ensure=layout):
                lance.write_dataset(layout.empty(), self.table_uri, mode="create")
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)) if _lance_exists(self.table_uri):
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, tuning)):
                lance.write_dataset(
                    data,
                    self.table_uri,
                    mode=_LANCE_MODE[mode],
                    max_rows_per_file=tuning.target_file_size or _LANCE_FRAGMENT_ROWS,
                    data_storage_version=tuning.data_storage_version,
                )
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="read", read=(version, _columns, predicate)):
                ds = _lance_travel(self.table_uri, version)
                return self._receipt(op, quantity=ds.count_rows(filter=predicate), unit=LakeUnit.ROWS, handle=ds)
            case TableFormat.LANCE, LakeOp(tag="delete", delete=(predicate,)):
                lance.dataset(self.table_uri).delete(predicate)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="update", update=(predicate, updates)):
                lance.dataset(self.table_uri).update(dict(updates.items()), where=predicate)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                builder = lance.dataset(self.table_uri).merge_insert(list(updates.keys()))
                builder.when_matched_update_all().when_not_matched_insert_all().execute(data)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="index", index=(column, kind, metric)):
                ds = lance.dataset(self.table_uri)
                ds.create_index(column, index_type=kind, metric=metric) if kind in _VECTOR_INDEX else ds.create_scalar_index(column, index_type=kind)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(target_size, _zorder, _partition)):
                metrics = lance.dataset(self.table_uri).optimize.compact_files(target_rows_per_fragment=target_size)
                return self._receipt(op, quantity=metrics.fragments_added, unit=LakeUnit.FRAGMENTS)
            case TableFormat.LANCE, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                ds = lance.dataset(self.table_uri)
                cutoff = datetime.now(UTC).replace(tzinfo=None) - _age(retention_hours)
                aged = sum(1 for row in ds.versions() if row["timestamp"] < cutoff)
                expired = aged if dry_run else ds.cleanup_old_versions(older_than=_age(retention_hours)).old_versions
                return self._receipt(op, quantity=expired, unit=LakeUnit.SNAPSHOTS)
            case TableFormat.LANCE, LakeOp(tag="restore", restore=(target,)):
                ds = _lance_travel(self.table_uri, target)
                ds.restore()
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("tag", name, target, False)):
                lance.dataset(self.table_uri).tags.create(name, target)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("branch", name, target, False)):
                lance.dataset(self.table_uri).create_branch(name, target)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("tag", name, _target, True)):
                lance.dataset(self.table_uri).tags.delete(name)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("branch", name, _target, True)):
                lance.dataset(self.table_uri).branches.delete(name)
                return self._receipt(op)
            case TableFormat.DUCKLAKE, LakeOp(tag="ensure", ensure=layout):
                with self._ducklake(layout.empty()) as (con, table):
                    terms = ", ".join(layout.ducklake_terms())
                    planted = Block.of_seq((
                        DdlStep(
                            verb=DdlVerb.CREATE_TABLE,
                            idempotence=Idempotence.ENSURED,
                            text=f"CREATE TABLE IF NOT EXISTS {table} AS SELECT * FROM payload",
                        ),
                        *(
                            (
                                DdlStep(
                                    verb=DdlVerb.ALTER_TABLE,
                                    idempotence=Idempotence.REPLACED,
                                    text=f"ALTER TABLE {table} SET PARTITIONED BY ({terms})",
                                ),
                            )
                            if terms
                            else ()
                        ),
                    ))
                    for step in planted:
                        step.run(con)
                    return self._receipt(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                with self._ducklake(data) as (con, table):
                    con.execute(
                        f"CREATE OR REPLACE TABLE {table} AS SELECT * FROM payload"
                        if mode == "overwrite"
                        else f"INSERT INTO {table} SELECT * FROM payload"
                    )
                    return self._receipt(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="read", read=(version, _columns, predicate)):
                with self._ducklake(data) as (con, table):
                    at = f" AT ({'VERSION' if isinstance(version, int) else 'TIMESTAMP'} => ?)" if version is not None else ""
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM {table}{at}{where}", [version] if version is not None else []).fetchone()[0]
                    return self._receipt(op, quantity=int(rows), unit=LakeUnit.ROWS, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="delete", delete=(predicate,)):
                with self._ducklake(data) as (con, table):
                    con.execute(f"DELETE FROM {table} WHERE {predicate}")
                    return self._receipt(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="update", update=(predicate, updates)):
                with self._ducklake(data) as (con, table):
                    assignments = ", ".join(f"{quote_ident(column)} = {expr}" for column, expr in updates.items())
                    con.execute(f"UPDATE {table} SET {assignments} WHERE {predicate}")
                    return self._receipt(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                with self._ducklake(data) as (con, table):
                    sets = ", ".join(f"{quote_ident(column)} = payload.{quote_ident(column)}" for column in updates.keys())
                    tail = " WHEN NOT MATCHED BY SOURCE THEN DELETE" if delete_unmatched else ""
                    con.execute(
                        f"MERGE INTO {table} USING payload ON {predicate}"
                        f" WHEN MATCHED THEN UPDATE SET {sets} WHEN NOT MATCHED THEN INSERT BY NAME{tail}"
                    )
                    return self._receipt(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="changefeed", changefeed=(start, end)):
                with self._ducklake(data) as (con, _table):
                    match Option.of_optional(end).or_else(_head(con)):
                        case Option(tag="none"):
                            return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.UNARMED.value))
                        case Option(tag="some", some=bound):
                            feed = con.execute("SELECT * FROM table_changes(?, ?, ?)", [self.identifier, start, bound]).to_arrow_table()
                            return self._receipt(op, quantity=feed.num_rows, unit=LakeUnit.ROWS, payload=feed, handle=con)
            case _, LakeOp(tag="ancestry", ancestry=(bound,)):
                return self._generations(bound).bind(
                    lambda roster: boundary(
                        LAKE_READ, lambda: column_frame(_GENERATION_COLUMNS, roster), catch=_commit_raises()
                    ).bind(lambda frame: self._receipt(op, quantity=len(roster), unit=LakeUnit.SNAPSHOTS, payload=frame))
                )
            case TableFormat.DUCKLAKE, LakeOp(tag="optimize"):
                with self._ducklake(data) as (con, _table):
                    DdlStep(
                        verb=DdlVerb.CALL, idempotence=Idempotence.REPLACED, text="CALL ducklake_merge_adjacent_files('lake')"
                    ).run(con)
                    return self._receipt(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                with self._ducklake(data) as (con, _table):
                    interval = retention_hours or _DEFAULT_RETENTION_HOURS
                    expired = DdlStep(
                        verb=DdlVerb.CALL,
                        idempotence=Idempotence.REPLACED,
                        text="CALL ducklake_expire_snapshots('lake', older_than => now() - to_hours(?), dry_run => ?)",
                        parameters=(interval, dry_run),
                    )
                    expired.run(con)
                    if not dry_run:
                        DdlStep(
                            verb=DdlVerb.CALL,
                            idempotence=Idempotence.REPLACED,
                            text="CALL ducklake_cleanup_old_files('lake', cleanup_all => true)",
                        ).run(con)
                    return self._receipt(op, handle=con)
            case TableFormat.PARQUET, LakeOp(tag="write", write=(mode, partition_by, _evolve, tuning)):
                return ContentIdentity.of(DOMAIN, arrow_bytes(data)).bind(
                    lambda key: ColumnarEgress.Dataset(
                        self.table_uri,
                        partition_by,
                        _PARQUET_CODEC[tuning.compression],
                        _PARQUET_EXISTING[mode],
                        f"part-{key.hex[:16]}-{{i}}.parquet",
                        self.filesystem,
                    )
                    .emit(data)
                    .bind(
                        lambda landed: self._receipt(
                            op,
                            key=key,
                            added=Posture(declared=landed.files),
                            byte_length=Posture(declared=landed.byte_length),
                            quantity=data.num_rows,
                            unit=LakeUnit.ROWS,
                        )
                    )
                )
            case TableFormat.PARQUET, LakeOp(tag="read"):
                counted = pads.dataset(self.table_uri, format="parquet", partitioning="hive", filesystem=self.filesystem).count_rows()
                return self._receipt(op, quantity=counted, unit=LakeUnit.ROWS)
            case _, _:
                return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.UNARMED.value))

    @contextmanager
    def _ducklake(self, data: pa.Table | None) -> "Iterator[tuple[duckdb.DuckDBPyConnection, str]]":
        session = DuckDbSession(
            attach=(Attach(alias="lake", target=f"ducklake:{self.dsn}", kind=DuckDbExtension.DUCKLAKE, current=True),), secrets=self.secrets
        )
        with session.connect() as con:
            if data is not None:
                con.register("payload", data)
            yield con, quote_ident(self.identifier)

    def _iceberg(self) -> "Table":
        return load_catalog(self.catalog).load_table(self.identifier)

    def _generations(self, bound: Depth) -> "RuntimeRail[Block[Generation]]":
        return boundary(LAKE_READ, lambda: self._history(bound), catch=_commit_raises())

    def _history(self, bound: Depth) -> "Block[Generation]":
        match self.table_format:
            case TableFormat.DELTA:
                entries = DeltaTable(self.table_uri).history(_limit(bound))
                return Block.of_seq(
                    Generation(
                        version=int(entry["version"]),
                        parent=Posture(declared=int(entry["version"]) - 1) if int(entry["version"]) else Posture(absent=None),
                        at=Posture.of_option(Option.of_optional(entry.get("timestamp")).map(_instant)),
                    )
                    for entry in entries
                )
            case TableFormat.ICEBERG:
                rows = self._iceberg().inspect.snapshots().to_pylist()
                return _bounded(
                    Block.of_seq(
                        Generation(
                            version=int(row["snapshot_id"]),
                            parent=Posture.of_option(Option.of_optional(row.get("parent_id")).map(int)),
                            at=Posture.of_optional(row.get("committed_at")),
                        )
                        for row in rows
                    ),
                    bound,
                )
            case TableFormat.LANCE:
                rows = lance.dataset(self.table_uri).versions()
                return _bounded(
                    Block.of_seq(
                        Generation(
                            version=int(row["version"]),
                            parent=Posture(declared=int(row["version"]) - 1) if int(row["version"]) > 1 else Posture(absent=None),
                            at=Posture.of_optional(row.get("timestamp")),
                        )
                        for row in rows
                    ),
                    bound,
                )
            case TableFormat.DUCKLAKE | TableFormat.PARQUET:
                with self._ducklake(None) as (con, _table):
                    rows = con.execute("SELECT snapshot_id, parent_id, snapshot_time FROM snapshots()").fetchall()
                return _bounded(
                    Block.of_seq(
                        Generation(
                            version=int(version),
                            parent=Posture.of_option(Option.of_optional(parent).map(int)),
                            at=Posture.of_optional(stamped),
                        )
                        for version, parent, stamped in rows
                    ),
                    bound,
                )
            case unreachable:
                assert_never(unreachable)

    def _snapshot(self, op: LakeOp, handle: Any | None) -> "tuple[Posture[int], Posture[int], Posture[int], int]":
        blank: "tuple[Posture[int], Posture[int], int]" = (Posture(absent=None), Posture(absent=None), 0)
        if op.tag == "read" and isinstance(op.read[0], int):
            return Posture(declared=op.read[0]), *blank
        match self.table_format:
            case TableFormat.DELTA:
                table = handle if handle is not None else DeltaTable(self.table_uri)
                return (Posture(declared=table.version()), *_delta_churn(table, op))
            case TableFormat.ICEBERG:
                history = (handle if handle is not None else self._iceberg()).inspect.snapshots()
                head = Some(history.column("snapshot_id")[-1].as_py()) if history.num_rows else Nothing
                return Posture.of_option(head), *blank
            case TableFormat.LANCE:
                return Posture(declared=(handle if handle is not None else lance.dataset(self.table_uri)).version), *blank
            case TableFormat.DUCKLAKE:
                return Posture.of_option(_head(handle)), *blank
            case TableFormat.PARQUET:
                return Posture(defaulted=(0, "parquet")), *blank
            case unreachable:
                assert_never(unreachable)

    def _receipt(
        self,
        op: LakeOp,
        *,
        key: ContentKey | None = None,
        quantity: int = 0,
        unit: LakeUnit = LakeUnit.NONE,
        matched: int = 0,
        added: "Posture[int]" = Posture(absent=None),
        removed: "Posture[int]" = Posture(absent=None),
        byte_length: "Posture[int]" = Posture(absent=None),
        pinned: "Posture[int]" = Posture(absent=None),
        payload: pa.Table | None = None,
        handle: Any | None = None,
    ) -> "RuntimeRail[LakeReceipt]":
        snapshot, churn_added, churn_removed, churn_bytes = self._snapshot(op, handle)
        version = pinned.option().or_else(snapshot.option())
        keyed = f"{self.table_uri}@{version.map(str).default_value('unarmed')}"
        identity = Ok(key) if key is not None else ContentIdentity.of(DOMAIN, keyed.encode())
        return identity.map(
            lambda resolved: LakeReceipt(
                table_uri=self.table_uri,
                table_format=self.table_format,
                operation=op.tag,
                version=version,
                files_added=added.option().or_else(churn_added.option()),
                files_removed=removed.option().or_else(churn_removed.option()),
                byte_length=byte_length.option().default_value(churn_bytes),
                quantity=quantity,
                unit=unit,
                matched=matched,
                content_key=resolved,
                payload=payload,
            )
        )


# --- [TABLES] ---------------------------------------------------------------------------

_DEFAULT_RETENTION_HOURS: Final[int] = 168

_LANCE_FRAGMENT_ROWS: Final[int] = 1024 * 1024

_VECTOR_INDEX: Final[frozenset[str]] = frozenset({"IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"})

_ICEBERG_REFERENCE: Final[Map[tuple[str, bool], str]] = Map.of_seq([
    (("tag", False), "create_tag"),
    (("branch", False), "create_branch"),
    (("tag", True), "remove_tag"),
    (("branch", True), "remove_branch"),
])

_LANCE_MODE: Final[Map[WriteMode, LanceMode]] = Map.of_seq([
    ("error", "create"),
    ("ignore", "create"),
    ("overwrite", "overwrite"),
    ("append", "append"),
])

_COMMIT_METRIC: Final[Map[str, tuple[str, str, str | None]]] = Map.of_seq([
    ("write", ("num_added_files", "num_removed_files", None)),
    ("delete", ("num_added_files", "num_removed_files", None)),
    ("update", ("num_added_files", "num_removed_files", None)),
    ("merge", ("num_target_files_added", "num_target_files_removed", None)),
    ("optimize", ("numFilesAdded", "numFilesRemoved", "filesAdded")),
])

# --- [ERRORS] ---------------------------------------------------------------------------


def _commit_raises() -> Catch:
    return (DeltaError, IcebergError, CommitFailedException, duckdb.Error, pa.ArrowException, ValueError, OSError)


LAKE_REFUSED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="reach", arm="boundary", defect="cell-refused", retriability=TERMINAL, slots=("operation", "reason")
)
LAKE_ADMIT_KIND: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="open.kind", arm="config", defect="ref-kind-refused", retriability=TERMINAL, slots=("format", "kind", "ref")
)
LAKE_ADMIT_NEEDS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="open.needs", arm="config", defect="coordinates-absent", retriability=TERMINAL, slots=("format", "missing", "ref")
)
LAKE_PAYLOAD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="gate", arm="config", defect="payload-absent", retriability=TERMINAL, slots=("operation",)
)
LAKE_CONTENDED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="fence", arm="resource", defect="concurrent-commit", retriability=TRANSIENT, slots=("operation",)
)
LAKE_STALE_FENCE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="fence.generation", arm="config", defect="stale-fence", retriability=TERMINAL, slots=("expected", "held")
)
LAKE_COMMIT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="commit", arm="boundary", defect="commit-leg", retriability=TRANSIENT
)
LAKE_READ: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="read", arm="boundary", defect="read-leg", retriability=TRANSIENT
)
LAKE_RECEIPTS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="receipts", arm="boundary", defect="receipt-frame", retriability=TERMINAL
)
LAKE_RECEIPTS_DECODE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.LAKEHOUSE, point="receipts.decode", arm="boundary", defect="receipt-decode", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    LAKE_REFUSED,
    LAKE_ADMIT_KIND,
    LAKE_ADMIT_NEEDS,
    LAKE_PAYLOAD,
    LAKE_CONTENDED,
    LAKE_STALE_FENCE,
    LAKE_COMMIT,
    LAKE_READ,
    LAKE_RECEIPTS,
    LAKE_RECEIPTS_DECODE,
]))

_REFUSAL: Final[Map[tuple[TableFormat, str], LakeRefusal]] = Map.of_seq([
    ((TableFormat.DELTA, "index"), LakeRefusal.DELTA_NO_INDEX),
    ((TableFormat.DELTA, "reference"), LakeRefusal.DELTA_NO_REFERENCE),
    ((TableFormat.DUCKLAKE, "reference"), LakeRefusal.DUCKLAKE_NO_REFERENCE),
    ((TableFormat.PARQUET, "reference"), LakeRefusal.PARQUET_NO_TRAVEL),
    ((TableFormat.ICEBERG, "update"), LakeRefusal.ICEBERG_NO_UPDATE),
    ((TableFormat.ICEBERG, "optimize"), LakeRefusal.ICEBERG_NO_OPTIMIZE),
    ((TableFormat.ICEBERG, "changefeed"), LakeRefusal.ICEBERG_NO_CHANGEFEED),
    ((TableFormat.ICEBERG, "index"), LakeRefusal.ICEBERG_NO_INDEX),
    ((TableFormat.LANCE, "evolve"), LakeRefusal.LANCE_NO_EVOLVE),
    ((TableFormat.LANCE, "changefeed"), LakeRefusal.LANCE_NO_CHANGEFEED),
    ((TableFormat.DUCKLAKE, "evolve"), LakeRefusal.DUCKLAKE_NO_EVOLVE),
    ((TableFormat.DUCKLAKE, "index"), LakeRefusal.DUCKLAKE_NO_INDEX),
    ((TableFormat.DUCKLAKE, "restore"), LakeRefusal.DUCKLAKE_NO_RESTORE),
    ((TableFormat.PARQUET, "delete"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "update"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "merge"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "evolve"), LakeRefusal.PARQUET_NO_EVOLVE),
    ((TableFormat.PARQUET, "optimize"), LakeRefusal.PARQUET_NO_OPTIMIZE),
    ((TableFormat.PARQUET, "vacuum"), LakeRefusal.PARQUET_NO_VACUUM),
    ((TableFormat.PARQUET, "changefeed"), LakeRefusal.PARQUET_NO_CHANGEFEED),
    ((TableFormat.PARQUET, "index"), LakeRefusal.PARQUET_NO_INDEX),
    ((TableFormat.PARQUET, "restore"), LakeRefusal.PARQUET_NO_TRAVEL),
    ((TableFormat.PARQUET, "ancestry"), LakeRefusal.PARQUET_NO_TRAVEL),
    ((TableFormat.PARQUET, "ensure"), LakeRefusal.PARQUET_NO_TABLE_SPEC),
])

_DUCKLAKE_TRANSFORMS: Final[frozenset[PartitionTransform]] = frozenset({"year", "month", "day", "hour", "bucket"})


class _Admission(Struct, frozen=True):
    kinds: frozenset[DatasetKind] = frozenset()
    needs: tuple[str, ...] = ()


_ADMIT: Final[Map[TableFormat, _Admission]] = Map.of_seq([
    (TableFormat.DELTA, _Admission(kinds=frozenset({DatasetKind.DELTA, DatasetKind.RECEIPTS}))),
    (TableFormat.ICEBERG, _Admission(kinds=frozenset({DatasetKind.ICEBERG}), needs=("catalog", "identifier"))),
    (TableFormat.LANCE, _Admission()),
    (TableFormat.DUCKLAKE, _Admission(needs=("dsn", "identifier"))),
    (TableFormat.PARQUET, _Admission(kinds=frozenset({DatasetKind.PARQUET}))),
])

_PARQUET_EXISTING: Final[Map[WriteMode, DatasetWrite]] = Map.of_seq([
    ("error", "error"),
    ("ignore", "overwrite_or_ignore"),
    ("append", "overwrite_or_ignore"),
    ("overwrite", "delete_matching"),
])

_PARQUET_CODEC: Final[Map[Compression, str]] = Map.of_seq([
    ("UNCOMPRESSED", "none"),
    ("SNAPPY", "snappy"),
    ("GZIP", "gzip"),
    ("BROTLI", "brotli"),
    ("LZ4", "lz4"),
    ("ZSTD", "zstd"),
])

_RECEIPT_COLUMNS: Final[Block[ColumnSpec[ReceiptFact, object]]] = Block.of_seq([
    ColumnSpec(name="at", arrow=pa.timestamp("us", tz="UTC"), kind=datetime, lift=lambda row: row.at),
    ColumnSpec(name="date", arrow=pa.date32(), kind=date, lift=lambda row: row.date),
    ColumnSpec(name="domain", arrow=pa.string(), kind=str, lift=lambda row: row.domain),
    ColumnSpec(name="kind", arrow=pa.string(), kind=str, lift=lambda row: row.kind),
    ColumnSpec(name="owner", arrow=pa.string(), kind=str, lift=lambda row: row.owner),
    ColumnSpec(name="phase", arrow=pa.string(), kind=str, lift=lambda row: row.phase),
    ColumnSpec(name="subject", arrow=pa.string(), kind=str, lift=lambda row: row.subject),
    ColumnSpec(name="tenant", arrow=pa.string(), kind=str, lift=lambda row: row.tenant),
    ColumnSpec(name="content_key", arrow=pa.string(), kind=str, lift=lambda row: row.content_key),
    ColumnSpec(name="facts", arrow=pa.map_(pa.string(), pa.string()), kind=Map, lift=lambda row: list(row.facts.items())),
    ColumnSpec(name="fact_keys", arrow=pa.list_(pa.string()), kind=tuple, lift=lambda row: list(row.fact_keys)),
])
_RECEIPT_SCHEMA: Final[pa.Schema] = column_schema(_RECEIPT_COLUMNS)

_GENERATION_COLUMNS: Final[Block[ColumnSpec[Generation, object]]] = Block.of_seq([
    ColumnSpec(name="node", arrow=pa.int64(), kind=int, lift=lambda row: row.version),
    ColumnSpec(name="parent", arrow=pa.int64(), kind=int, nullable=True, lift=lambda row: row.parent.option().to_optional()),
    ColumnSpec(name="at", arrow=pa.timestamp("us", tz="UTC"), kind=datetime, nullable=True, lift=lambda row: row.at.option().to_optional()),
    ColumnSpec(name="refs", arrow=pa.list_(pa.string()), kind=tuple, lift=lambda row: list(row.refs)),
])

_RESIDENCE: Final[Map[Residence, ResidenceRow]] = Map.of_seq([
    (
        Residence.EVIDENCE,
        ResidenceRow(
            kind=DatasetKind.RECEIPTS,
            table_format=TableFormat.DELTA,
            domain="telemetry",
            partition_by=("domain", "date"),
            zorder=("tenant", "content_key"),
            tuning=WriteTuning(compression="ZSTD", statistics_truncate_length=64, bloom_columns=("tenant", "content_key", "kind")),
            tenancy="tenant leads the clustering key, so a single-tenant predicate prunes files before a row is read",
            lifetime="a committed row survives to the table's own log-retention window; the MAINTAIN plan's Vacuum expires it, on the cadence the deploy plane schedules that plan",
            lifetime_hours=None,
            fits="mutable evidence carrying time travel and a change feed",
            admit="`Lakehouse.sink` appends one `write_deltalake` commit per receipt drain through the INGEST plan",
            layout=TableLayout(
                schema=_RECEIPT_SCHEMA, partition_by=(("domain", "identity", None), ("date", "identity", None))
            ),
        ),
    ),
    (
        Residence.TABLE,
        ResidenceRow(
            kind=DatasetKind.ICEBERG,
            table_format=TableFormat.ICEBERG,
            domain="telemetry",
            partition_by=(),
            zorder=(),
            tuning=WriteTuning(compression="ZSTD"),
            tenancy="tenant buckets in the spec `Ensure` authors, so a tenant predicate prunes without a directory per tenant",
            lifetime="a committed snapshot survives to the window the format's own table properties declare; the MAINTAIN plan's Vacuum expires it on the deploy plane's cadence",
            lifetime_hours=None,
            fits="catalog-governed multi-engine read where a foreign engine holds the catalog",
            admit="`Lakehouse.sink` appends through the catalog transaction the INGEST plan's own `Ensure` authored",
            layout=TableLayout(
                schema=_RECEIPT_SCHEMA,
                partition_by=(("domain", "identity", None), ("at", "day", None), ("tenant", "bucket", 16)),
                sort_by=(("tenant", "asc"), ("content_key", "asc")),
            ),
        ),
    ),
    (
        Residence.COLD,
        ResidenceRow(
            kind=DatasetKind.PARQUET,
            table_format=TableFormat.PARQUET,
            domain="telemetry",
            partition_by=("domain", "date"),
            zorder=(),
            tuning=WriteTuning(compression="ZSTD"),
            tenancy="tenant stays a row column; a hive segment per tenant fragments the tree past any pruning it buys",
            lifetime="a landed generation survives to the object-plane lifecycle rule the deploy plane sets, and that plane is its sole ender — the MAINTAIN plan derives EMPTY off the refusal rows, so this owner expires nothing",
            lifetime_hours=None,
            fits="cold tail, cheapest per byte, whole-partition batch scan",
            admit="`Lakehouse.sink` lands one `ColumnarEgress.Dataset` generation per drain; the tree carries no log to commit to",
        ),
    ),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _conditional(table_format: TableFormat, op: LakeOp) -> Option[LakeRefusal]:
    match table_format, op:
        case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(_adds, drops, renames, _constraints)) if drops or renames:
            return Some(LakeRefusal.DELTA_COLUMN_SURGERY)
        case _, LakeOp(tag="ensure", ensure=layout) if not layout.widths_paired:
            return Some(LakeRefusal.PARTITION_TRANSFORM_WIDTH)
        case _, LakeOp(tag="ensure", ensure=layout) if layout.sort_by and table_format is not TableFormat.ICEBERG:
            return Some(LakeRefusal.SORT_ORDER_CATALOG_ONLY)
        case TableFormat.DELTA, LakeOp(tag="ensure", ensure=layout) if not layout.identity_only:
            return Some(LakeRefusal.DELTA_PARTITION_TRANSFORM)
        case TableFormat.LANCE, LakeOp(tag="ensure", ensure=layout) if layout.partition_by:
            return Some(LakeRefusal.LANCE_NO_TABLE_SPEC)
        case TableFormat.DUCKLAKE, LakeOp(tag="ensure", ensure=layout) if any(
            transform not in _DUCKLAKE_TRANSFORMS and transform != "identity" for _column, transform, _width in layout.partition_by
        ):
            return Some(LakeRefusal.DUCKLAKE_PARTITION_TRUNCATE)
        case TableFormat.ICEBERG, LakeOp(tag="write", write=(_mode, partition_by, _evolve, _tuning)) if partition_by:
            return Some(LakeRefusal.ICEBERG_PARTITION_SPEC)
        case TableFormat.ICEBERG, LakeOp(tag="write", write=("error", _partition_by, _evolve, _tuning)):
            return Some(LakeRefusal.ICEBERG_WRITE_EXISTS)
        case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(_adds, _drops, _renames, constraints)) if constraints:
            return Some(LakeRefusal.ICEBERG_CONSTRAINTS)
        case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(_target_size, zorder, _partition)) if zorder:
            return Some(LakeRefusal.LANCE_NO_ZORDER)
        case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(_target_size, _zorder, partition)) if partition:
            return Some(LakeRefusal.LANCE_NO_PARTITION_FILTER)
        case TableFormat.DUCKLAKE, LakeOp(tag="optimize", optimize=(_target_size, _zorder, partition)) if partition:
            return Some(LakeRefusal.DUCKLAKE_OPTIMIZE_PARTITION)
        case TableFormat.DUCKLAKE, LakeOp(tag="optimize", optimize=(target_size, zorder, _partition)) if target_size is not None or zorder:
            return Some(LakeRefusal.DUCKLAKE_OPTIMIZE_TUNING)
        case TableFormat.DUCKLAKE, LakeOp(tag="read", read=(str(), _columns, _predicate)):
            return Some(LakeRefusal.DUCKLAKE_READ_TAG)
        case TableFormat.PARQUET, LakeOp(tag="read", read=(version, _columns, predicate)) if version is not None or predicate:
            return Some(LakeRefusal.PARQUET_READ_SCOPE)
        case TableFormat.PARQUET, LakeOp(tag="write", write=(_mode, _partition_by, _evolve, tuning)) if (
            _PARQUET_CODEC.try_find(tuning.compression).is_none()
        ):
            return Some(LakeRefusal.PARQUET_CODEC)
        case _, _:
            return Nothing


def _fenced_properties(properties: "CommitProperties | None", fence: "Option[Fence]") -> "CommitProperties | None":
    return fence.map(
        lambda held: CommitProperties(
            custom_metadata=properties.custom_metadata if properties else None,
            max_commit_retries=properties.max_commit_retries if properties else None,
            app_transactions=[Transaction(app_id=held.app_id, version=held.expected + 1)],
        )
    ).default_value(properties)


def _transaction_version(uri: str, app_id: str) -> int:
    return DeltaTable(uri).transaction_version(app_id)


def _limit(bound: Depth) -> int:
    match bound:
        case Depth(tag="bounded", bounded=held):
            return held
        case Depth(tag="fixpoint"):
            return 0
        case _ as unreachable:
            assert_never(unreachable)


def _bounded(roster: "Block[Generation]", bound: Depth) -> "Block[Generation]":
    ordered = roster.sort_with(lambda row: -row.version)
    return ordered if bound.tag == "fixpoint" else ordered.take(min(_limit(bound), len(ordered)))


def _instant(held: object) -> datetime:
    return datetime.fromtimestamp(int(held) / 1000.0, UTC) if isinstance(held, int) else held


def _head(con: "duckdb.DuckDBPyConnection") -> Option[int]:
    row = con.execute("SELECT max(snapshot_id) FROM snapshots()").fetchone()
    return Option.of_optional(row[0]).map(int)


def receipt_frame(receipts: Iterable[Receipt], *, tenant: str, at: datetime) -> "RuntimeRail[pa.Table]":
    return boundary(LAKE_RECEIPTS, lambda: _framed(Block.of_seq(_fact(receipt, tenant, at) for receipt in receipts)), catch=_commit_raises())


def _fact(receipt: Receipt, tenant: str, at: datetime) -> ReceiptFact:
    _level, phase, facts = receipt.project()
    held = {key: str(value) for key, value in facts.items() if key not in _LIFTED and value is not None}
    return ReceiptFact(
        at=at,
        date=at.date(),
        domain=_column(facts, EvidenceToken.DOMAIN) or _column(facts, EvidenceToken.OWNER),
        kind=_column(facts, EvidenceToken.KIND) or phase,
        owner=_column(facts, EvidenceToken.OWNER),
        phase=phase,
        subject=_column(facts, EvidenceToken.SUBJECT),
        tenant=tenant,
        content_key=_column(facts, EvidenceToken.KEY),
        facts=Map.of_seq(sorted(held.items())),
        fact_keys=tuple(sorted(held)),
    )


def _column(facts: Mapping[str, object], key: EvidenceToken) -> str:
    held = facts.get(key)
    return "" if held is None else str(held)


def receipt_facts(table: pa.Table) -> "RuntimeRail[tuple[ReceiptFact, ...]]":
    return boundary(LAKE_RECEIPTS_DECODE, lambda: column_rows(table, _RECEIPT_COLUMNS, ReceiptFact), catch=_commit_raises())


def _framed(rows: "Block[ReceiptFact]") -> pa.Table:
    return column_frame(_RECEIPT_COLUMNS, rows)


def _lance_exists(uri: str) -> bool:
    try:
        lance.dataset(uri)
    except ValueError:
        return False
    return True


def _lance_travel(uri: str, target: int | str | datetime | None) -> Any:
    return lance.dataset(uri, asof=target) if isinstance(target, datetime) else lance.dataset(uri, version=target)


def _millis(moment: datetime) -> int:
    return int(moment.timestamp() * 1000)


def _iceberg_snapshot(table: "Table", version: int | str | datetime) -> "Option[int]":
    match version:
        case int() as snapshot_id:
            return Some(snapshot_id)
        case str() as named:
            return Option.of_optional(table.snapshot_by_name(named)).map(lambda snapshot: snapshot.snapshot_id)
        case moment:
            return Option.of_optional(table.snapshot_as_of_timestamp(_millis(moment))).map(lambda snapshot: snapshot.snapshot_id)


def _iceberg_head(table: "Table", target: int | None) -> "Option[int]":
    return Some(target) if target is not None else Option.of_optional(table.current_snapshot()).map(lambda snapshot: snapshot.snapshot_id)


def _add_paths(uri: str) -> frozenset[str]:
    return frozenset(pa.table(DeltaTable(uri).get_add_actions()).column("path").to_pylist()) if DeltaTable.is_deltatable(uri) else frozenset()


def _added_bytes(table: "DeltaTable", prior: frozenset[str]) -> int:
    actions = pa.table(table.get_add_actions())
    return sum(
        size
        for path, size in zip(actions.column("path").to_pylist(), actions.column("size_bytes").to_pylist(), strict=True)
        if path not in prior
    )


def _delta_metric(metrics: dict[str, object], key: str) -> "Posture[int]":
    return Posture.of_option(Option.of_optional(metrics.get(key)).map(int))


def _delta_churn(table: DeltaTable, op: LakeOp) -> "tuple[Posture[int], Posture[int], int]":
    absent: "tuple[Posture[int], Posture[int], int]" = (Posture(absent=None), Posture(absent=None), 0)
    return (
        _COMMIT_METRIC.try_find(op.tag).map(lambda keys: _churn(table.history(1), keys)).default_value(absent)
        if op.committing
        else absent
    )


def _churn(entries: list[dict[str, Any]], keys: tuple[str, str, str | None]) -> "tuple[Posture[int], Posture[int], int]":
    metrics = (entries[0].get("operationMetrics") or {}) if entries else {}
    added, removed, volume = keys
    return _delta_metric(metrics, added), _delta_metric(metrics, removed), _delta_volume(metrics, volume)


def _delta_volume(metrics: dict[str, object], key: str | None) -> int:
    held = metrics.get(key) if key is not None else None
    return int(json_decode(held)["totalSize"]) if isinstance(held, str) else 0


def _retention(retention_hours: int | None) -> datetime:
    return datetime.now(UTC) - timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)


def _age(retention_hours: int | None) -> timedelta:
    return timedelta(hours=retention_hours or _DEFAULT_RETENTION_HOURS)
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
