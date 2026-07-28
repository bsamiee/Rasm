# [PY_DATA_LAKEHOUSE]

Table-format interchange crosses one `LakeOp` operation axis with one `TableFormat` provider axis on one `Lakehouse` owner over Delta, Iceberg, Lance, DuckLake, and the non-transactional Parquet tree. `Lakehouse.run` folds the ensure/write/read/delete/update/merge/evolve/optimize/vacuum/changefeed/index/restore lifecycle through the `LakeOp` tagged union and dispatches one `(TableFormat, tag)` arm to a `RuntimeRail[LakeReceipt]` — the operation axis format-agnostic, the format binding a separate discriminant, so a new format is one `TableFormat` row and its arms, never a parallel Iceberg or Lance owner, and formats reaching fewer operations state that as `_REFUSAL` rows. `Lakehouse` commits and reads snapshots over the provider surface; it holds no durable store.

`Residence` rows the analytics planes this owner writes — the Delta evidence table, the Iceberg alternative, the Parquet cold tail — each answering fits, ingest, tenancy, retention, and a degradation derived from the reach matrix, so arming a residence is a row and no arm carries a partition or retention literal. `Lakehouse.sink` folds a receipt stream through `receipt_frame` onto the pinned `_RECEIPT_SCHEMA` and commits it through the SAME `(format, tag)` matrix a caller's table rides, so the evidence plane inherits reach, veto, retry, span, and snapshot identity whole. `ResidenceRow.ops` splits that plan across a `LakePhase` axis: INGEST arms the plane off its own `TableLayout` and commits one append per drain, MAINTAIN carries the clustering and retention passes the deploy plane schedules, because a clustering pass folded into ingest rewrites every file the plane holds on every drain. Both rosters derive off the reach matrix, so a residence whose format authors no table object plans no arming rather than declaring the absence by hand, and no residence waits on a foreign engine to plant it first. That plane carries NO cardinality ceiling — unbounded dimensionality is the capability a metrics view cap exists to destroy — and its `cap` column is typed `False` so no later pass adds one.

Iceberg's read path is the core-loadable DuckDB `iceberg` extension with `pyiceberg` the catalog-write fallback; Lance carries the multimodal-asset versioning and `create_index` ANN rail; DuckLake rides one `Attach` row over the shared `tabular/columnar#SCAN` `DuckDbSession`, the single session every DuckDB-backed arm reuses. `changefeed` is the Delta `load_cdf` and DuckLake `table_changes` feed the `tabular/materialize#MATERIALIZE` `DerivedSnapshot._materialize` consumer reads, and the receipt carries that feed on its `payload` slot so the consumer composes this owner rather than re-opening `DeltaTable` behind it. Every commit contributes through runtime `ReceiptContributor`, keys by `ContentIdentity`, and — when mutating — rides the `runtime/reliability/resilience#RESILIENCE` `RetryClass.LAKE_COMMIT` `guarded_sync` envelope; `open`/`run`/`run_async` admit through `@beartype(conf=FAULT_CONF)`, the shared config the sibling `interop`/`egress`/`columnar` seams bind. Table-protocol governance — deletion vectors, `TableFeatures` — is DECLINED here: the C# `Rasm.Persistence` at-rest owner holds it, never a data-side commit toggle.

## [01]-[INDEX]

- [02]-[LAKEHOUSE]: `Lakehouse` crosses one `LakeOp` operation axis with one `TableFormat` provider axis, and the `Residence` family rides that same matrix, its evidence plane committing through the `sink` fold.

## [02]-[LAKEHOUSE]

- Owner: `Lakehouse` over the `LakeOp` operation axis (a `tagged_union` matched by `match (self.table_format, op)`) and the `TableFormat` `StrEnum` provider axis, dispatched one `(format, tag)` arm — two orthogonal discriminants, so a new operation is one `LakeOp` case and a new format one `TableFormat` row, never a `read_delta`/`write_delta`/`delete_delta` method family and never a parallel `IcebergLakehouse`/`LanceLakehouse` pair. Writer tuning rides one `WriteTuning` policy `Struct` carried on `Write`, never a parallel `WriteTuned` op or a knob tail; the merge delete-on-no-match rides one `delete_unmatched` discriminant selecting the third `when_not_matched_by_source_delete` clause, never a `MergeDelete` op.
- Owner: `Residence` rows the analytics planes by CAPABILITY — `ResidenceRow` answering the estate residence floor (`fits`, `ingest`, `tenancy`, `retain`, `degrade`, `cap` typed `False`) beside this owner's own extension of kind, format, partition roster, clustering roster, writer tuning, retention window, and an arming `TableLayout`, with `degrade` DERIVED off `_REFUSAL`, so a format losing an op degrades every residence riding it with zero row edits and a hand-written degradation sentence cannot drift from the refusal producing it.
- Owner: `TableLayout` states the authored table spec as DATA — schema, `PartitionTransform`-keyed partition pairs, sort order, properties — and each format arm projects that one declaration onto its own grammar (`bucket[16]` tokens at the iceberg catalog, `bucket(16, col)` SQL at ducklake, bare columns at delta), so a second residence arms through the same row shape and a transform a format cannot spell refuses by name rather than vanishing.
- Owner: `TableFormat.PARQUET` seats the object-plane tree as a FORMAT rather than a writer hanging off the residence family — its `_REFUSAL` cells state the whole non-transactionality, its armed write and read inherit the reach gate, the commit veto, the retry envelope, the span, and the receipt every sibling format rides, and `ResidenceRow.degrade` then derives the cold row's degradation from those cells instead of a hand-kept sentence beside them. Provider-dialect divergence stays a row: `_PARQUET_EXISTING` projects the write mode onto the tree's collision policy and `_PARQUET_CODEC` the writer's codec roster onto its file options, a codec the tree cannot spell refusing by name rather than downgrading.
- Law: `contribute` records the file-churn pair under `domain="lake"` keyed by operation, so the commit plane projects onto the metric spine beside its tabular siblings.
- Law: a RESIDENCE commit records nothing — metering the plane that stores the receipt stream feeds a series back into the stream it just wrote.
- Law: `contribute` spells `domain` and `kind` into its facts as the SAME pair it hands `Metrics.record`, so a stored evidence row rejoins the series its live twin emitted.
- Law: `quantity` stays receipt-only, its `LakeUnit` varying per arm, so one descriptor never carries four magnitudes.
- Law: a non-committing op moves no files and records nothing, keeping read and changefeed arms off the commit series.
- Entry: `Lakehouse.open` admits a dataset, format policy, and composition scope through ONE `_ADMIT` row read — the row's `kinds` set gating the ref shape and its `needs` roster the coordinates — where three hand-written arms checked the ref kind on the Delta arm alone and let a Lance or Iceberg handle open over a CSV ref; the admitted `kind` rides the handle, so `_admitted` proves a residence commit holds the SUBJECT its row names rather than merely a table in the right format. `sink` and the awaitable `sink_async` are the evidence plane's one ingest over that admission, folding a receipt stream through `receipt_frame` and the row's INGEST plan — the awaitable leg exists because a composition draining its own receipts on an event loop otherwise stalls it for every commit. `maintain` runs the same row's MAINTAIN plan scoped to a named generation, the entrypoint a scheduled job calls. `run` and the awaitable `run_async` both read ONE `_gated` prologue — the reach matrix, then the `LAKE_COMMIT_POINT` veto for a committing op — and select the same envelope, the awaitable leg adding one `on_thread` band hop because every arm is a blocking native commit an async composition otherwise runs inline on its loop. `Read`, `ChangeFeed`, and dry-run `Vacuum` ride the bare boundary rail; a refused cell answers its typed row ahead of both the hook point and the retry envelope, and the veto's own fact never rides out as the gate's value.
- Receipt: the snapshot identity is one polymorphic `_snapshot` method discriminating `match self.table_format`, folded by one `_receipt` projector — never three sibling `_<format>_snapshot` factories nor a parallel `_SNAPSHOT` dispatch dict — and it reads the provider handle the ARM already opened, so a receipt costs no second log load and a travelling read keys on the version it pinned rather than on head. `LakeReceipt` keys by `ContentIdentity.of("lake", f"{table_uri}@{version}")`, which returns a rail the projector threads through `.map` so a digest fault propagates rather than a `Result` landing in the `content_key` slot; the `(table_uri, version)` payload pins the committed snapshot stable across a re-open of an unchanged version. `files_added`/`files_removed` carry the COMMIT's own churn off `_COMMIT_METRIC` — the snapshot's file roster counts every file the table holds, so reading it as an append's evidence prices one generation at the whole residence's size — while `byte_length` carries the volume that operation's own provider measures and `(quantity, unit)` the arm's own measure over the closed `LakeUnit` vocabulary, so a row count, a fragment count, and an expired-snapshot count each report as what they are rather than folding into a field named for files. Stock and flow never sum: the ledger slots every fact on the operation, so a `read` row carries the residence's held volume and a `write` row its written volume. `payload` carries the frame an op MOVES — the change feed alone today — because a count-only receipt forces its one row-consuming consumer to re-open the provider this owner already read. `contribute` emits `Receipt.of("lakehouse", ("emitted", subject, facts))` whose counts ride as native `int` the `enc_hook=repr` renderer serializes without a pre-coerce.
- Receipt: `quantity` and `matched` split an upsert's LANDED rows from its REDELIVERED ones — Iceberg answers the pair natively off `UpsertResult.rows_inserted`/`rows_updated`, and the Delta arm reads `num_target_rows_inserted`/`num_target_rows_updated` because `num_output_rows` counts the rewritten output files — inserted, updated, and copied together — and so exceeds the offered batch whenever an untouched row shares a rewritten file; a consumer deriving duplicates by subtracting one fused tally from its own batch length reports zero forever.
- Receipt: `ReceiptFact` is the durable evidence row and `receipt_frame` its one projection, folding each receipt through the union's OWN `project()` so a new `Receipt` case reaches the residence with zero edits here. `_LIFTED` states the evidence contract as data — `domain`, `kind`, `owner`, `subject`, and `key` reach typed columns while every other fact survives verbatim in the open map, because the residence exists to keep the dimensions a metrics view cap drops.
- Packages: `deltalake` owns the Delta arms — its `PostCommitHookProperties` and `TableAlterer.add_constraint` are MINED as `WriteTuning` hook fields and the `Evolve.constraints` clause, while `TableFeatures`/deletion-vector protocol enablement is DECLINED as the C# `Rasm.Persistence` at-rest concern; the predicate-bearing Delta read pushes SQL through the native `QueryBuilder` DataFusion surface, no SQL->pyarrow-DNF lowering owner minted. `pyiceberg` is the catalog-write fallback only (its `Table` annotation rides `TYPE_CHECKING`), gated behind the runtime lacking the core-loadable DuckDB `iceberg` read extension; `create_table_if_not_exists` is the idempotent create the `Ensure` arm plants through, `Table.update_spec`/`UpdateSpec.add_field` authoring the partition spec off name-keyed transform TOKENS the provider parses (`bucket[16]`, `truncate[4]`) and `Table.update_sort_order`/`UpdateSortOrder.asc`/`desc` the sort order, both gated on the table's own `spec()`/`sort_order()` reading empty. `pylance` owns the Lance dataset/version-travel/index arms and the predicate-scoped `LanceDataset.update` mutation; `pyarrow` is the write carrier. `tabular/columnar#SCAN` supplies the ONE session rail every DuckLake and Iceberg arm reuses — `DuckDbSession`/`DuckDbExtension`/`Attach` carrying the attach as session data, `quote_ident`/`quote_literal` the one escape rule, `ColumnarEgress.Dataset` with its `emit`/`Landed` half the `PARQUET` write arm commits through, and `arrow_bytes` the one serialization its generation token digests; the `ducklake` and `iceberg` SQL surfaces are `data/.api/duckdb-extensions.md` rows [06] and [04], its `[04]-[DUCKLAKE]` cluster carrying every attach, snapshot, change-feed, and maintenance statement. runtime supplies `RuntimeRail`/`BoundaryFault`/`boundary`/`ContentIdentity`/`ReceiptContributor`/`Receipt` with the `FAULT_CONF`, `RetryClass.LAKE_COMMIT`, and `guarded_sync` the admission and commit rails bind.
- Growth: a new lake operation is one `LakeOp` case absorbed by the `(format, tag)` dispatch, naming its `LakeUnit` on the receipt; a new partition transform is one `PartitionTransform` member with its per-format projection row and, where its grammar carries one, one `_WIDTH_TRANSFORMS` entry, an absent projection row refusing that cell by name; a residence needing an authored table is one `ResidenceRow` `layout` value, the INGEST plan already carrying the arming; a newly counted quantity kind is one `LakeUnit` row; a new write mode a `Literal` row on `Write` beside its `_PARQUET_EXISTING` projection; a new codec a `Compression` row beside its `_PARQUET_CODEC` projection, an absent projection refusing that cell rather than downgrading it; a new writer-tuning knob a `WriteTuning` field; a newly reported commit metric one `_COMMIT_METRIC` row carrying the provider's own key spelling; a new residence lifecycle phase one `LakePhase` member with its `ops` arm, every entrypoint reading the plan it returns; a new Lance vector index kind a `VectorIndex` `Literal` row (a scalar/FTS kind a `ScalarIndex` row), both absorbed by the one `_VECTOR_INDEX`-routed `Index` arm; a new DuckDB-backed capability one `DuckDbExtension` row and its `(DUCKLAKE|ICEBERG, *)` SQL arm; a further table format (Hudi, Paimon) one `TableFormat` member with its `_ADMIT` row, its `_REFUSAL` rows, and its arms on this same owner; a new analytics residence one `_RESIDENCE` row naming its own partition and clustering rosters; a new lifted evidence column one `_LIFTED` key beside its `_RECEIPT_SCHEMA` field, every producer already spelling it in facts; a new commit-governance concern is one subscriber the app root attaches on `LAKE_COMMIT_POINT`, zero owner edits. DEFERRED: the version-reference authoring pair — Lance `tags.create`/`create_branch` and Iceberg `ManageSnapshots.create_branch`/`create_tag` — lands as ONE reference-authoring `LakeOp` case with per-format arms when a consumer names it; the read side is already landed (tag-string/`asof` time-travel on `Read`, `checkout_version`/`restore` on `Restore`).
- Boundary: analytics residences carry NO view cap and NO cardinality ceiling — the `cap` column is typed `False`, so the budget a metrics plane needs is unrepresentable here rather than merely discouraged.
- Boundary: no worker, scheduler, or retention executor enters for telemetry — `maintain` is the residence row's MAINTAIN plan and the deploy plane's own scheduled job supplies the cadence, so every expiry rides the residence's own mechanism and a row whose format refuses both passes derives an EMPTY plan rather than answering two refusals a scheduler reads as failure.
- Boundary: `sink` reads no baggage — tenant and observation instant arrive from the composition that drained the receipts, because a commit running after the producing context moved stamps whatever tenancy happens to be active.
- Boundary: no durable store, no schema migration, no global Delta or catalog connection, no blocking commit run inline on an event loop where `run_async` owns the band hop, and no bare `trace.get_tracer(scope)` beside the faults-owned `scoped` stamp that binds the version and semconv triple; the metadata-only `Read` count is not the read lane — column-projected zero-copy reads route to the `tabular/columnar#SCAN` reader, not this commit owner. Reject law is data: `_REFUSAL` rows every `(format, tag)` cell a provider surface cannot portably reach and `_conditional` rows every cell the op's own operands decide, each row carrying its `LakeRefusal` member as the reason the fault reports, so a reject is a table edit and never an arm spending itself on a sentence. `_reach` reads that matrix ahead of the hook point and the retry envelope, `_apply`'s `case _, _` tail answers an admitted cell no arm executes, and every reject returns `Error(BoundaryFault(...))` carrying the typed key — never a silent no-op, never a `raise` into a `boundary` that re-keys and discards it, and never a hand-opened `stamina.retry_context` where `guarded_sync` owns the envelope.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Iterator, Mapping
from contextlib import contextmanager
from copy import replace
from datetime import UTC, date, datetime, timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import lance
import pyarrow as pa
import pyarrow.compute as pc
import pyarrow.dataset as pads
from beartype import beartype
from deltalake import (
    BloomFilterProperties,
    ColumnProperties,
    CommitProperties,
    DeltaTable,
    PostCommitHookProperties,
    QueryBuilder,
    WriterProperties,
    write_deltalake,
)
from deltalake.schema import Field
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field
from msgspec.json import decode as json_decode
from opentelemetry import trace

lazy from pyiceberg.catalog import load_catalog
lazy from pyiceberg.transforms import IdentityTransform
lazy from pyiceberg.types import IcebergType

from rasm.data.tabular.columnar import (
    Attach,
    ColumnarEgress,
    DatasetKind,
    DatasetRef,
    DatasetWrite,
    DuckDbExtension,
    DuckDbSession,
    arrow_bytes,
    quote_ident,
    quote_literal,
)
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, async_boundary, boundary, scoped
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.identity import ContentIdentity, ContentKey
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
# `WriterProperties` spells its codec roster UPPERCASE and carries `LZ4_RAW` beside the six an earlier roster named,
# so a lowercase policy value type-checks nowhere against the installed writer even where the Rust core tolerates it.
type Compression = Literal["UNCOMPRESSED", "SNAPPY", "GZIP", "BROTLI", "LZ4", "LZ4_RAW", "ZSTD"]
type VectorIndex = Literal["IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"]
type ScalarIndex = Literal["BTREE", "BITMAP", "LABEL_LIST", "ZONEMAP", "BLOOMFILTER", "RTREE", "INVERTED", "FTS", "NGRAM"]
type IndexKind = VectorIndex | ScalarIndex
type Metric = Literal["L2", "cosine", "dot"]
type Evolution = tuple[tuple[tuple[str, str], ...], tuple[str, ...], tuple[tuple[str, str], ...], Map[str, str]]
# `(column, operator, value)` conjunction the transactional optimizer scopes its rewrite by. A clustering pass over a
# date-partitioned evidence plane touches TODAY and leaves every prior generation alone; an unscoped pass rewrites the
# whole table on every run, which is the cost that turns a maintenance window into a rewrite of all history.
type Partition = tuple[tuple[str, str, str], ...]
# one vocabulary spans every partition transform and each format arm projects it onto its own grammar, so a
# residence declares its layout once and no arm reads a provider-shaped spec object. Iceberg reaches the whole set, ducklake all but
# `truncate`, delta `identity` alone, and lance none — each divergence a refusal row rather than a per-format type.
type PartitionTransform = Literal["identity", "year", "month", "day", "hour", "bucket", "truncate"]
# the transforms whose grammar CARRIES a width — `bucket[16]`, `truncate[4]` — so the width slot is required on
# exactly these and forbidden on every other member. One row set answers both directions, which is what lets the
# layout state the pairing as a fact rather than each format arm discovering a malformed token at its own provider.
_WIDTH_TRANSFORMS: Final[frozenset[PartitionTransform]] = frozenset({"bucket", "truncate"})
type SortDirection = Literal["asc", "desc"]


class TableFormat(StrEnum):
    DELTA = "delta"
    ICEBERG = "iceberg"
    LANCE = "lance"
    DUCKLAKE = "ducklake"
    # this row seats the non-transactional object-plane tree: hive-partitioned Parquet generations under `ColumnarEgress.Dataset`.
    # It earns a format row rather than an escape hatch beside the family because everything it CANNOT do lands as
    # `_REFUSAL` cells, and everything it can — write, read — then rides the identical gate, veto, retry envelope,
    # span, and receipt. A residence carrying its own writer beside this matrix is the second commit path that
    # drifts from all five of those the first time one of them changes.
    PARQUET = "parquet"


class LakeUnit(StrEnum):
    # what an arm's OWN measure counts. `files_added`/`files_removed` are file evidence off the committed
    # snapshot and the provider's file metrics alone; every other quantity an arm reports — rows deleted,
    # rows fed, fragments compacted, snapshots expired — rides `(quantity, unit)`, exactly as the sibling
    # `tabular/egress#EGRESS` receipt splits `byte_length` from `(quantity, unit)`. Folding four measures into
    # a field named for files makes the cost ledger's file-churn read a row count on half the ops.
    ROWS = "rows"
    FILES = "files"
    FRAGMENTS = "fragments"
    SNAPSHOTS = "snapshots"
    NONE = "none"


class LakeRefusal(StrEnum):
    # every refused `(format, op)` cell names its own reason: `_REFUSAL` rows the unconditional cells and
    # `_conditional` the operand-dependent ones, while the member value is the operator-facing evidence
    # `BoundaryFault` carries — so a reject stays a data row and no arm spends itself on a sentence.
    DELTA_NO_INDEX = "delta reaches no vector or scalar index surface"
    DELTA_COLUMN_SURGERY = "delta alter reaches no portable column drop or rename"
    ICEBERG_NO_UPDATE = "pyiceberg reaches no predicate-scoped row update"
    ICEBERG_NO_OPTIMIZE = "pyiceberg reaches no rewrite_data_files compaction"
    ICEBERG_NO_CHANGEFEED = "pyiceberg reaches no change-data feed"
    ICEBERG_NO_INDEX = "iceberg reaches no index surface"
    ICEBERG_PARTITION_SPEC = "partition_by is table-spec-owned; author PartitionSpec at create"
    ICEBERG_WRITE_EXISTS = "error mode forbids a write into an existing table"
    ICEBERG_CONSTRAINTS = "constraint governance is delta alter.add_constraint only"
    ICEBERG_SNAPSHOT_ID = "iceberg rollback takes an int snapshot_id"
    ICEBERG_READ_SNAPSHOT_ID = "iceberg read pins an int snapshot_id; no tag or timestamp travel"
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
    # a residence's commit plan splits by PHASE because the two phases run on different clocks: ingest fires per
    # drain and maintenance on the deploy plane's own schedule. Folding clustering into ingest z-orders the whole
    # evidence table on every receipt drain — the provider filters no partition unless one is passed — so a plane
    # taking a drain a second rewrites its entire history a second.
    INGEST = "ingest"
    MAINTAIN = "maintain"


class Residence(StrEnum):
    # analytics residences keyed by CAPABILITY, mirroring how the deploy plane parameterizes its metrics-store
    # family: each row answers fits, ingest, tenancy, retention, and an honest degradation, so arming a residence
    # is a row and hardcoding one below the family is the deleted form. Subject and format stay separate axes —
    # `DatasetKind` names the source shape and `TableFormat` the commit protocol, this enum the subject.
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
    # Lance on-disk file format as a POLICY row, not the provider default.
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
    tag: Literal["ensure", "write", "read", "delete", "update", "merge", "evolve", "optimize", "vacuum", "changefeed", "index", "restore"] = tag()
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

    @property
    def committing(self) -> bool:
        match self:
            case LakeOp(tag="read") | LakeOp(tag="changefeed") | LakeOp(tag="vacuum", vacuum=(_, True)):
                return False
            case LakeOp():
                return True

    @property
    def feeding(self) -> bool:
        # the two cases whose arms READ the payload frame: every other case carries its whole operand on the union.
        # Stating it here rather than inside each provider arm is what lets the gate answer a missing frame ahead of
        # the veto point and the retry envelope, where an arm reaching `None` answers whatever the provider raises.
        match self:
            case LakeOp(tag="write") | LakeOp(tag="merge"):
                return True
            case LakeOp():
                return False

    @staticmethod
    def Ensure(layout: "TableLayout") -> "LakeOp":
        return LakeOp(ensure=layout)

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


class LakeCommit(Struct, frozen=True):
    # pre-flight commit fact — a receipt exists only post-commit, so the veto edge fires this intent shape.
    table_uri: str
    table_format: TableFormat
    operation: str


# --- [RESIDENCE]

class ReceiptFact(Struct, frozen=True):
    # ONE durable evidence row over the whole receipt stream. Its five lifted columns are the evidence contract every
    # tabular `contribute` spells — `domain` and `kind` the SAME pair that contributor hands `Metrics.record`, `owner`
    # and `subject` its identity, `key` its content key — so a stored row rejoins the series its live twin emitted and
    # a cost slot reconstructs without the producer's process. Everything the contributor did not lift stays in the
    # open `facts` map. Every lifted column is TOTAL: `domain` is a hive path segment and the union's rejected and
    # drained cases carry no domain fact at all, so the producer's metric domain answers first and the receipt owner
    # otherwise — a rejection lands under its producer rather than in a `domain=` directory no predicate names.
    # Unbounded dimensionality IS the residence's capability, so nothing here trims the open map. `fact_keys`
    # carries the sorted key roster beside the map, so a key-existence predicate prunes on file statistics before any
    # value comparison reads the map — the pair lands together or the pruning is a full scan wearing an index's name.
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
    # one authored table SPEC per `Ensure`: schema, partition transforms, sort order, and table properties.
    # Transforms carry a closed vocabulary with the width the parameterized pair needs, so a residence declares
    # `("date", "day", None)` and `("tenant", "bucket", 16)` as DATA and each format arm projects that pair onto its
    # own grammar — where a per-format spec object would fork the layout the moment a second residence arms.
    schema: pa.Schema
    partition_by: tuple[tuple[str, PartitionTransform, int | None], ...] = ()
    sort_by: tuple[tuple[str, SortDirection], ...] = ()
    properties: Map[str, str] = field(default_factory=lambda: Map.of_seq([]))

    @property
    def columns(self) -> tuple[str, ...]:
        # identity-only projection the column-partitioning formats take, transform and width dropped.
        return tuple(column for column, _transform, _width in self.partition_by)

    @property
    def identity_only(self) -> bool:
        return all(transform == "identity" for _column, transform, _width in self.partition_by)

    @property
    def widths_paired(self) -> bool:
        # a width-parameterized transform WITHOUT its width and a plain transform carrying one both render tokens no
        # provider parses — bare `bucket` at the iceberg catalog, `day(4, col)` at ducklake — so the pairing is one
        # layout fact both projections read rather than a malformed spec each grammar discovers inside its own arm.
        return all((width is not None) is (transform in _WIDTH_TRANSFORMS) for _column, transform, width in self.partition_by)

    def iceberg_fields(self) -> "Block[tuple[str, str]]":
        # iceberg's OWN transform grammar carries the width INSIDE the token — `bucket[16]`, `truncate[4]` — and
        # `UpdateSpec.add_field` parses that spelling, so no arm constructs one `Transform` subclass per case.
        return Block.of_seq(
            (column, transform if width is None else f"{transform}[{width}]") for column, transform, width in self.partition_by
        )

    def ducklake_terms(self) -> "Block[str]":
        # ducklake spells a transform as a SQL FUNCTION CALL over the column and identity as the bare column, so the
        # `_DUCKLAKE_PARTITION` row supplies the function name and the width leads its argument list.
        return Block.of_seq(
            quote_ident(column)
            if transform == "identity"
            else f"{_DUCKLAKE_PARTITION[transform]}({quote_ident(column)})"
            if width is None
            else f"{_DUCKLAKE_PARTITION[transform]}({width}, {quote_ident(column)})"
            for column, transform, width in self.partition_by
        )

    def empty(self) -> pa.Table:
        # zero rows carrying the whole schema: DuckDB derives every column type off the registered Arrow frame and
        # Lance seeds an empty dataset from it, so neither arm needs an Arrow-to-provider type table of its own.
        return self.schema.empty_table()


class ResidenceRow(Struct, frozen=True):
    # one residence's whole capability answered in columns. `degrade` is DERIVED off the reach matrix rather than
    # restated, so a format losing an op degrades every residence riding it with zero row edits, and a hand-written
    # degradation sentence cannot drift from the refusal that produces it.
    kind: DatasetKind
    table_format: TableFormat
    domain: str
    partition_by: tuple[str, ...]
    zorder: tuple[str, ...]
    tuning: WriteTuning
    tenancy: str
    retain: str
    # `retain_hours` carries the retention WINDOW as row data beside the prose naming its mechanism, so the plan holds
    # no literal and a residence keeping evidence longer is one row edit. `None` takes the format's own default window.
    retain_hours: int | None
    fits: str
    # Writers that FILL this plane, stated per row because each fill path genuinely differs: two rows commit
    # through a transactional log and the cold row lands a generation through the columnar egress. Leaving this
    # floor column off lets a caller hardcode which entrypoint fills which plane.
    ingest: str
    # `layout` carries the table SPEC this residence arms itself from, so a catalog-governed plane comes into
    # existence through the same matrix that writes it and no composition remembers an out-of-band create. A row
    # leaving it `None` declares that its format needs no authored spec, and `ops` then plans no `Ensure` at all.
    layout: TableLayout | None = None
    # analytics residences carry NO cardinality ceiling, and the type says so: a metrics store demands view caps
    # because a series per value kills it, while this plane exists precisely to hold the dimensions those caps drop.
    # Typed `False`, a later pass cannot helpfully add a budget without deleting the reason the residence exists.
    cap: Literal[False] = False

    @property
    def degrade(self) -> tuple[LakeRefusal, ...]:
        # WHOLLY derived: every capability a residence loses is a refused cell on the format it rides, so a hand-kept
        # sentence beside the matrix cannot survive a format gaining or losing an arm. The non-transactional row
        # degrades hardest and states none of it here — its own cells say it.
        return tuple(refusal for (fmt, _tag), refusal in _REFUSAL.items() if fmt is self.table_format)

    def ops(self, phase: LakePhase, partition: Partition = ()) -> tuple[LakeOp, ...]:
        # ONE plan projection over the phase axis, so a residence's whole lifecycle is row data and neither phase pays
        # its sibling's cost. INGEST arms the plane and commits the append, carrying this row's partition columns and
        # writer tuning. MAINTAIN pairs the clustering pass with the retention pass, both scoped to whichever
        # generation `partition` names. BOTH rosters DERIVE off the reach matrix: a format refusing an op drops it
        # here rather than answering a refusal a scheduler must read as success. Folding clustering into INGEST is
        # deleted — table formats separate layout from write, and an unscoped z-order rewrites every file this plane
        # holds, so a residence taking one drain a second rewrites its whole history a second.
        # `Ensure` leads INGEST rather than riding a phase of its own: every provider's arming is its own
        # if-not-exists surface, so re-running it per drain costs one metadata probe, and the alternative — a create
        # a composition must remember to run once — is exactly the out-of-band step this operation exists to delete.
        match phase:
            case LakePhase.INGEST:
                armed = (LakeOp.Ensure(self.layout),) if self.layout is not None else ()
                planned = (*armed, LakeOp.Write(mode="append", partition_by=self.partition_by, evolve_schema=True, tuning=self.tuning))
            case LakePhase.MAINTAIN:
                clustered = (LakeOp.Optimize(self.tuning.target_file_size, self.zorder, partition),) if self.zorder else ()
                planned = (*clustered, LakeOp.Vacuum(self.retain_hours, dry_run=False))
            case unreachable:
                assert_never(unreachable)
        return tuple(op for op in planned if _REFUSAL.try_find((self.table_format, op.tag)).is_none())


# mutation-edge hook point: the data composition fold registers the row; a VETO tap gates commit pre-flight.
LAKE_COMMIT_POINT: Final[HookPoint[LakeCommit]] = HookPoint(id="rasm.data.lakehouse.commit", payload=LakeCommit, modality=Modality.VETO)


class LakeReceipt(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    operation: str
    version: int
    # per-COMMIT churn, never the snapshot's file roster: Delta answers both off its own log entry and the tree answers
    # whichever generation it landed. Reading a snapshot count here prices one append at the whole table's SIZE.
    files_added: int
    files_removed: int
    # `byte_length` carries the volume THIS operation accounts for as its own provider measures it — the tree's landed
    # generation, the optimizer's rewritten files, a Delta read's pinned snapshot. Stock and flow never sum because the
    # ledger slots every fact on its operation, so `read` and `write` are two slots: residence storage answers off the
    # read and write spend off the write. Delta appends measure no bytes and report zero, never an invented number.
    byte_length: int
    quantity: int
    unit: LakeUnit
    # `matched` splits an upsert's landed rows from its REDELIVERED ones. `quantity` alone cannot: an upsert answers
    # one output tally covering inserted, updated, and copied rows, so a caller subtracting it from its offered batch
    # reports zero duplicates forever and clamps negative on a copy-heavy rewrite. Formats answering no split report 0.
    matched: int
    content_key: ContentKey
    # `payload` carries the frame an op MOVES, filled where a provider answers rows rather than a count — the change feed
    # alone today. Its one consumer therefore composes this owner instead of re-opening `DeltaTable` behind
    # it, which is the duplicated-provider form a count-only receipt forces on anything needing the rows.
    payload: pa.Table | None = None
    # set by `sink` alone: this commit WROTE the evidence plane rather than a caller's table.
    residence: Residence | None = None

    def contribute(self) -> Iterable[Receipt]:
        # commit-plane metric: file churn per commit lands on the metric spine under domain="lake" keyed by operation,
        # matching every sibling tabular contributor. `quantity` stays receipt-only — its `LakeUnit` varies per op, so
        # one instrument over it would export four dimensions of magnitude under one descriptor; the table uri stays
        # receipt-only as unbounded cardinality. A non-committing op moves no files and records nothing, and a
        # RESIDENCE commit records nothing at all: metering the plane that stores the receipt stream feeds a series
        # back into the very stream it just wrote, so the evidence write stays evidence and never a second source.
        if (self.files_added or self.files_removed) and self.residence is None:
            Metrics.record(
                {"rasm.lake.commit.files_added": float(self.files_added), "rasm.lake.commit.files_removed": float(self.files_removed)},
                domain="lake",
                kind=self.operation,
            )
        yield Receipt.of(
            "lakehouse",
            (
                "emitted",
                self.table_uri,
                {
                    # `domain`/`kind`/`key` are the lifted evidence contract `receipt_frame` reads: the SAME pair this
                    # contributor hands `Metrics.record` beside the key it minted, so a durable row rejoins its series.
                    "domain": _RESIDENCE[self.residence].domain if self.residence is not None else "lake",
                    "kind": self.operation,
                    "key": self.content_key.hex,
                    "format": self.table_format,
                    "version": self.version,
                    "added": self.files_added,
                    "removed": self.files_removed,
                    "bytes": self.byte_length,
                    "quantity": self.quantity,
                    "unit": self.unit,
                },
            ),
        )


# --- [SERVICES] -------------------------------------------------------------------------

# faults-owned scope stamp: `scoped` binds the version and semconv triple, so no page re-spells the pin.
_TRACER: Final = scoped(trace.get_tracer, "rasm.data.tabular.lakehouse")


class Lakehouse(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    # `kind` carries the admitted ref's own source shape, so a residence commit proves the handle holds the SUBJECT its
    # row names and not merely a table in the right format. `DatasetKind.RECEIPTS` makes that provable: without it a
    # `sink` guarded on format alone writes the evidence stream into any Delta table a caller happened to open.
    kind: DatasetKind
    catalog: str | None = None
    identifier: str | None = None
    # DuckLake catalog DSN — caller-resolved through the runtime `TransportResource` seam, never minted here.
    dsn: str | None = None
    # this slot carries the runtime-resolved `fsspec` handle off the admitted ref, so the tree writer reaches an
    # object-plane prefix under the same credentials every other runtime leg uses; a handle re-minted here would
    # authenticate against whatever ambient environment the commit happens to run in.
    filesystem: Any | None = None
    scope: ScopeKey = DEFAULT_SCOPE

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
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeRail[Lakehouse]":
        # ONE row read admits every format: the row's `kinds` set gates the ref shape and its `needs` roster gates the
        # coordinates, where three hand-written arms checked the ref kind on the Delta arm alone — so a Lance or
        # Iceberg handle opened over a CSV ref passed admission and died inside the provider with a decode fault.
        row = _ADMIT[table_format]
        coordinates = {"catalog": catalog, "identifier": identifier, "dsn": dsn}
        missing = tuple(name for name in row.needs if coordinates[name] is None)
        if row.kinds and dataset.kind not in row.kinds:
            return Error(BoundaryFault(resource=(f"{table_format.value}-refuses-{dataset.kind.value}", dataset.ref.relative)))
        if missing:
            return Error(BoundaryFault(resource=(f"{table_format.value}-needs-{'-'.join(missing)}", dataset.ref.relative)))
        return Ok(
            cls(
                table_uri=str(dataset.ref.path),
                table_format=table_format,
                kind=dataset.kind,
                catalog=catalog,
                identifier=identifier,
                dsn=dsn,
                filesystem=dataset.ref.path.fs,
                scope=scope,
            )
        )

    @beartype(conf=FAULT_CONF)
    def sink(
        self,
        receipts: Iterable[Receipt],
        residence: Residence = Residence.EVIDENCE,
        *,
        tenant: str = "",
        at: datetime | None = None,
    ) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        # `sink` IS the evidence plane's one ingest: the receipt stream lands as durable rows through the SAME commit
        # matrix a caller's table rides, so a residence inherits reach, veto, retry, span, and snapshot identity whole
        # and no second write path drifts from them. `tenant` and `at` arrive from whichever composition DRAINED the
        # receipts rather than being read here: a commit running after the producing context moved stamps whatever
        # tenancy happened to be active, silently attributing one tenant's evidence to the next.
        # an EMPTY drain plans nothing: the frame answers the emptiness because the receipt stream is consumed
        # exactly once, and the empty plan's own `Ok(())` is the no-op result — arming and committing zero rows
        # would mint one snapshot per quiet drain, inflating the version history the retention owner walks and
        # reporting a commit rate on a plane nothing wrote.
        return self._admitted(residence).bind(
            lambda row: receipt_frame(receipts, tenant=tenant, at=at or datetime.now(UTC)).bind(
                lambda frame: self._folded(row.ops(LakePhase.INGEST), residence, frame) if frame.num_rows else Ok(())
            )
        )

    @beartype(conf=FAULT_CONF)
    async def sink_async(
        self,
        receipts: Iterable[Receipt],
        residence: Residence = Residence.EVIDENCE,
        *,
        tenant: str = "",
        at: datetime | None = None,
    ) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        # `sink_async` exists because the receipt drain IS the async case: a composition draining its own receipt stream
        # on an event loop through the synchronous entrypoint stalls that loop for every commit it makes. Same
        # admission, same frame projection, same plan — `run_async` alone owns the band hop.
        stamped = at or datetime.now(UTC)
        match self._admitted(residence).bind(lambda row: receipt_frame(receipts, tenant=tenant, at=stamped).map(lambda frame: (row, frame))):
            case Result(tag="error", error=refused):
                return Error(refused)
            case Result(tag="ok", ok=(row, frame)):
                # same empty-drain answer the synchronous half gives: a quiet window plans nothing rather than
                # committing a zero-row snapshot the retention owner then walks.
                return await self._drained(row.ops(LakePhase.INGEST), residence, frame) if frame.num_rows else Ok(())
            case unreachable:
                assert_never(unreachable)

    @beartype(conf=FAULT_CONF)
    def maintain(self, residence: Residence = Residence.EVIDENCE, *, partition: Partition = ()) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        # `maintain` runs the residence's OTHER phase on the deploy plane's own cadence rather than per drain: clustering
        # scoped to whichever generation `partition` names, then this row's retention window. `[04.0]`'s no-new-executor
        # law points exactly here — one plan a scheduled job calls, never a worker, scheduler, or retention loop minted
        # inside this owner.
        return self._admitted(residence).bind(lambda row: self._folded(row.ops(LakePhase.MAINTAIN, partition), residence, None))

    def _admitted(self, residence: Residence) -> "RuntimeRail[ResidenceRow]":
        # ONE residence admission both ingest legs and the maintenance leg read: the row's format AND its source shape
        # must be the handle's own, so an evidence commit proves the subject it writes into rather than only its format.
        row = _RESIDENCE[residence]
        mismatch = (
            Some(LakeRefusal.RESIDENCE_FORMAT_MISMATCH)
            if row.table_format is not self.table_format
            else Some(LakeRefusal.RESIDENCE_KIND_MISMATCH)
            if row.kind is not self.kind
            else Nothing
        )
        return mismatch.map(lambda refusal: Error(BoundaryFault(boundary=(f"lake.{residence.value}", refusal)))).default_value(Ok(row))

    def _folded(self, ops: tuple[LakeOp, ...], residence: Residence, frame: pa.Table | None) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        # every residence plan runs SEQUENTIALLY: a later op reads the generation the earlier one committed, so the
        # fold threads the rail rather than gathering commits. The write op alone carries the frame; a maintenance op
        # moves no payload, and handing it one would commit the evidence twice.
        return Block.of_seq(ops).fold(
            lambda landed, op: landed.bind(
                lambda rows: self.run(op, frame if op.tag == "write" else None).map(
                    lambda receipt: (*rows, replace(receipt, residence=residence))
                )
            ),
            Ok(()),
        )

    async def _drained(self, ops: tuple[LakeOp, ...], residence: Residence, frame: pa.Table | None) -> "RuntimeRail[tuple[LakeReceipt, ...]]":
        # `_drained` twins `_folded` on the awaitable leg: `Block.fold` threads no awaitable, so tail recursion IS the
        # fold and every arm still returns a rail — no mutable accumulator and no gather over commits that must order.
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
        # Table format and op ride the commit span as dimensions — that span is the transactional leg's trace
        # surface, and the fence marks a failed leg's span. The tail self-flattens the nested `_apply` rail.
        subject = self._subject(op)
        with _TRACER.start_as_current_span(subject, attributes=self._dimensions(op)):
            return self._gated(op, data).bind(
                lambda admitted: (
                    guarded_sync(RetryClass.LAKE_COMMIT, self._apply, admitted, data, subject=subject)
                    if admitted.committing
                    else boundary(subject, lambda: self._apply(admitted, data))
                ).bind(lambda rail: rail)
            )

    @beartype(conf=FAULT_CONF)
    async def run_async(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        # every arm below is a BLOCKING native leg — a Delta commit, a Lance compaction, a DuckLake attach — so
        # an async composition reaching the synchronous entrypoint stalls its whole event loop for the duration
        # of a commit. The awaitable leg reads the SAME gate and the SAME envelope selection over one `on_thread`
        # band hop, exactly as the sibling `tabular/egress#EGRESS` owner splits two entrypoints off one row set;
        # `_gated` crosses a `match` closed by `assert_never` here because the fenced leg is awaited.
        subject = self._subject(op)
        with _TRACER.start_as_current_span(subject, attributes=self._dimensions(op)):
            match self._gated(op, data):
                case Result(tag="error", error=fault):
                    return Error(fault)
                case Result(tag="ok", ok=admitted):
                    fenced = (
                        await guarded(RetryClass.LAKE_COMMIT, on_thread, self._apply, admitted, data, subject=subject)
                        if admitted.committing
                        else await async_boundary(subject, lambda: on_thread(self._apply, admitted, data))
                    )
                    return fenced.bind(lambda rail: rail)
                case unreachable:
                    assert_never(unreachable)

    def _gated(self, op: LakeOp, data: pa.Table | None) -> "RuntimeRail[LakeOp]":
        # ONE prologue both entrypoints read: reach answers a refused cell before the commit VETO point fires and
        # before the retry envelope opens, then an admitted committing op fires the point pre-flight while `Read`,
        # `ChangeFeed`, and dry-run `Vacuum` pass untouched. The fire rail carries the SUBSCRIBER's fact, so the
        # admitted op is re-mapped back onto it rather than letting a foreign payload ride out as the gate's value.
        # The payload joins reach because it is the same class of bound: a feeding op with no frame is a cell no arm
        # can execute, and letting it through fires a veto point for a mutation that then dies inside the provider
        # with a fault a caller cannot tell from a transport failure.
        if op.feeding and data is None:
            return Error(BoundaryFault(boundary=(self._subject(op), f"{op.tag} carries no payload frame")))
        return self._reach(op).bind(
            lambda admitted: Hooks.fire(
                LAKE_COMMIT_POINT.id,
                LakeCommit(table_uri=self.table_uri, table_format=self.table_format, operation=admitted.tag),
                scope=self.scope,
            ).map(lambda _fact: admitted)
            if admitted.committing
            else Ok(admitted)
        )

    def _subject(self, op: LakeOp) -> str:
        return f"lake.{self.table_format}.{op.tag}"

    def _dimensions(self, op: LakeOp) -> dict[str, str]:
        return {"rasm.lake.format": self.table_format.value, "rasm.lake.op": op.tag}

    def _reach(self, op: LakeOp) -> "RuntimeRail[LakeOp]":
        # one matrix read decides reachability: the unconditional `_REFUSAL` row answers first, the operand-
        # conditional row second, and an admitted op falls through carrying itself onto the commit chain.
        return (
            _REFUSAL.try_find((self.table_format, op.tag))
            .or_else_with(lambda: _conditional(self.table_format, op))
            .map(lambda refusal: Error(BoundaryFault(boundary=(self._subject(op), refusal))))
            .default_value(Ok(op))
        )

    def _apply(self, op: LakeOp, data: pa.Table | None) -> "RuntimeRail[LakeReceipt]":
        # reach already refused every unreachable and operand-conditional cell, so each arm below is provider
        # work alone and the tail answers an admitted cell no arm executes.
        match self.table_format, op:
            case TableFormat.DELTA, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)) if DeltaTable.is_deltatable(self.table_uri):
                # `ignore` no-ops onto the current snapshot exactly as the Iceberg and Lance arms do. Without the
                # short-circuit the writer commits nothing while the projector still reads the log's newest entry,
                # so a no-op write would report the PREVIOUS append's churn as its own — the churn slots are stated
                # zero here rather than inherited from a commit this call never made.
                return self._receipt(op, added=0, removed=0, byte_length=0)
            case TableFormat.DELTA, LakeOp(tag="ensure", ensure=layout):
                # `mode="ignore"` IS delta-rs's own if-not-exists: an existing log returns its table untouched, so the
                # INGEST plan re-runs this per drain with no probe of its own. Delta partitions on COLUMNS, which is
                # why reach refuses every non-identity transform here, and table properties ride `configuration`.
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
                    commit_properties=tuning.commit_properties(),
                    post_commithook_properties=tuning.hook_properties(),
                )
                # NO delta write, merge, or delete `operationMetrics` key carries bytes — every one is a file count,
                # a row count, or a duration. `get_add_actions` is the only byte truth the format publishes, carrying
                # per-file `size_bytes` off the current snapshot, so the arm differences that roster across the commit
                # it just made — the pre-write read is what makes the difference exact — and reports the volume THIS
                # write landed rather than the whole table's held size.
                table = DeltaTable(self.table_uri)
                return self._receipt(op, byte_length=_added_bytes(table, prior), handle=table)
            case TableFormat.DELTA, LakeOp(tag="read", read=(version, _columns, predicate)):
                # Predicate path pushes SQL through `QueryBuilder`; else metadata-only `to_pyarrow_dataset().count_rows()`.
                # `DeltaTable(version=)` admits an int alone, while `load_as_version` spans the int snapshot, the
                # string tag, and the timestamp — so every shape `Read` declares travels through the one member.
                table = DeltaTable(self.table_uri)
                if version is not None:
                    table.load_as_version(version)
                rows = (
                    QueryBuilder().register("t", table).execute(f"SELECT count(*) AS n FROM t WHERE {predicate}").read_all().column("n")[0].as_py()
                    if predicate
                    else table.to_pyarrow_dataset().count_rows()
                )
                # reads are the STATE op, so this byte slot answers the PINNED snapshot's on-disk volume — Delta's one
                # published storage number. It never mixes with a write's written volume because the ledger slots every
                # fact on its operation, so stock and flow stay two rows. `get_add_actions` answers an ARRO3 `Table`,
                # so this frame crosses the PyCapsule into pyarrow ONCE here: `pyarrow.compute` refuses an arro3
                # `ChunkedArray` outright, and a fold reaching one fails the receipt AFTER its read already succeeded.
                actions = pa.table(table.get_add_actions())
                stored = int(pc.sum(actions.column("size_bytes")).as_py() or 0)
                return self._receipt(op, quantity=rows, unit=LakeUnit.ROWS, byte_length=stored, handle=table)
            case TableFormat.DELTA, LakeOp(tag="delete", delete=(predicate,)):
                table = DeltaTable(self.table_uri)
                metrics = table.delete(predicate)
                return self._receipt(op, quantity=_delta_metric(metrics, "num_deleted_rows"), unit=LakeUnit.ROWS, handle=table)
            case TableFormat.DELTA, LakeOp(tag="update", update=(predicate, updates)):
                table = DeltaTable(self.table_uri)
                metrics = table.update(updates=dict(updates.items()), predicate=predicate)
                return self._receipt(op, quantity=_delta_metric(metrics, "num_updated_rows"), unit=LakeUnit.ROWS, handle=table)
            case TableFormat.DELTA, LakeOp(tag="merge", merge=(predicate, updates, delete_unmatched)):
                clauses = dict(updates.items())
                table = DeltaTable(self.table_uri)
                merger = table.merge(data, predicate=predicate).when_matched_update(updates=clauses).when_not_matched_insert(updates=clauses)
                metrics = (merger.when_not_matched_by_source_delete() if delete_unmatched else merger).execute()
                # `num_output_rows` counts the REWRITTEN OUTPUT FILES — inserted plus updated plus COPIED — so it
                # exceeds the offered batch whenever an untouched row shares a rewritten file. The inserted and
                # updated slots are the honest pair, and they sum to `num_source_rows` by construction.
                return self._receipt(
                    op,
                    quantity=_delta_metric(metrics, "num_target_rows_inserted"),
                    unit=LakeUnit.ROWS,
                    matched=_delta_metric(metrics, "num_target_rows_updated"),
                    handle=table,
                )
            case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(adds, _drops, _renames, constraints)):
                table = DeltaTable(self.table_uri)
                if adds:
                    table.alter.add_columns([Field(name, dtype) for name, dtype in adds])
                if constraints:
                    # mined governance clause — named SQL invariants enforced at every commit.
                    table.alter.add_constraint(dict(constraints.items()))
                return self._receipt(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="optimize", optimize=(target_size, zorder, partition)):
                # `partition_filters` scopes the rewrite: a clustering pass over the evidence plane's own `(domain, date)`
                # layout touches the named generation, where an unfiltered pass rewrites every file the table holds.
                table = DeltaTable(self.table_uri)
                filters = [(column, operator, value) for column, operator, value in partition] or None
                (
                    table.optimize.z_order(list(zorder), partition_filters=filters, target_size=target_size)
                    if zorder
                    else table.optimize.compact(partition_filters=filters, target_size=target_size)
                )
                return self._receipt(op, handle=table)
            case TableFormat.DELTA, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                # `vacuum` answers the removed FILE paths, so the file slot and the op measure genuinely coincide here.
                table = DeltaTable(self.table_uri)
                removed = table.vacuum(retention_hours=retention_hours, dry_run=dry_run)
                return self._receipt(op, removed=len(removed), quantity=len(removed), unit=LakeUnit.FILES, handle=table)
            case TableFormat.DELTA, LakeOp(tag="changefeed", changefeed=(start, end)):
                # `load_cdf` answers an arro3 `RecordBatchReader`; the zero-copy PyCapsule re-import lands the pyarrow
                # frame this owner hands out, so a change-feed consumer folds the receipt payload instead of re-opening
                # `DeltaTable` behind the owner that already read it.
                table = DeltaTable(self.table_uri)
                feed = pa.table(table.load_cdf(starting_version=start, ending_version=end).read_all())
                return self._receipt(op, quantity=feed.num_rows, unit=LakeUnit.ROWS, payload=feed, handle=table)
            case TableFormat.DELTA, LakeOp(tag="restore", restore=(target,)):
                # `restore` answers CAMEL-cased SINGULAR keys off the Rust layer where every sibling roster is snake or
                # camel-plural, and it answers them on its own return rather than through the commit log — so the arm
                # holds both churn slots here and `_COMMIT_METRIC` rows it nowhere.
                table = DeltaTable(self.table_uri)
                metrics = table.restore(target)
                return self._receipt(
                    op,
                    added=_delta_metric(metrics, "numRestoredFile"),
                    removed=_delta_metric(metrics, "numRemovedFile"),
                    quantity=_delta_metric(metrics, "numRestoredFile"),
                    unit=LakeUnit.FILES,
                    handle=table,
                )
            case TableFormat.ICEBERG, LakeOp(tag="ensure", ensure=layout):
                # catalog-governed arming: `create_table_if_not_exists` is the one idempotent create, and the spec
                # lands AFTER it because a partition field binds the SOURCE FIELD ID the catalog assigns while
                # converting the Arrow schema — resolving that id here would re-derive the assignment. Each update
                # gates on the table's OWN current spec being empty, so re-running the plan per drain re-authors
                # nothing rather than resting on the provider's duplicate handling.
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
                # `_iceberg()` loads an existing table, so `ignore` no-ops onto the current snapshot.
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="write", write=(mode, _partition_by, _evolve, _tuning)):
                txn = self._iceberg().transaction()
                txn.overwrite(data) if mode == "overwrite" else txn.append(data)
                txn.commit_transaction()
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(int() as version, _columns, predicate)):
                # a snapshot-pinned read goes through the catalog scan; every other read rides `iceberg_scan`
                # with no catalog round-trip, the pyiceberg catalog staying write-only.
                table = self._iceberg()
                counted = table.scan(row_filter=predicate or "true", snapshot_id=version).count()
                return self._receipt(op, quantity=counted, unit=LakeUnit.ROWS, handle=table)
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(_version, _columns, predicate)):
                with DuckDbSession(extensions=(DuckDbExtension.ICEBERG,)).connect() as con:
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM iceberg_scan({quote_literal(self.table_uri)}){where}").fetchone()[0]
                return self._receipt(op, quantity=int(rows), unit=LakeUnit.ROWS)
            case TableFormat.ICEBERG, LakeOp(tag="delete", delete=(predicate,)):
                # `Transaction.delete`/`upsert` return `None`/`UpsertResult`, not the `Transaction`, so
                # `commit_transaction` is a separate statement off `txn`, never chained.
                txn = self._iceberg().transaction()
                txn.delete(predicate)
                txn.commit_transaction()
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                txn = self._iceberg().transaction()
                # `UpsertResult` answers the insert/update split natively where delta-rs answers only a fused output
                # tally, so this arm fills both slots off the provider rather than deriving either.
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
                self._iceberg().manage_snapshots().rollback_to_snapshot(target).commit()
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                # real dry-run: the expirable set projects off the snapshots metadata table because the provider's
                # expire builder carries no preview, and the count lands BEFORE the wet leg commits, so both legs
                # report honest removed evidence and the default Vacuum() succeeds on every format.
                cutoff = _retention(retention_hours)
                table = self._iceberg()
                aged = sum(1 for committed in table.inspect.snapshots().column("committed_at").to_pylist() if committed < cutoff)
                if not dry_run:
                    table.maintenance.expire_snapshots().older_than(cutoff).commit()
                return self._receipt(op, quantity=aged, unit=LakeUnit.SNAPSHOTS)
            case TableFormat.LANCE, LakeOp(tag="ensure", ensure=_layout) if _lance_exists(self.table_uri):
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="ensure", ensure=layout):
                # a zero-row `create` seeds the schema and nothing else — reach refuses every partition and sort
                # operand on this format, so the empty frame IS the whole arming a Lance dataset admits.
                lance.write_dataset(layout.empty(), self.table_uri, mode="create")
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="write", write=("ignore", _partition_by, _evolve, _tuning)) if _lance_exists(self.table_uri):
                # `ignore` short-circuits to the current snapshot when the dataset already exists.
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="write", write=(mode, _partition_by, _evolve, tuning)):
                lance.write_dataset(
                    data,
                    self.table_uri,
                    mode=_LANCE_MODE[mode],
                    max_rows_per_file=tuning.target_file_size or 1024 * 1024,
                    data_storage_version=tuning.data_storage_version,
                )
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="read", read=(version, _columns, predicate)):
                # an `int`/`str` tag rides `version=`; a `datetime` resolves through `asof=`.
                ds = lance.dataset(self.table_uri, asof=version) if isinstance(version, datetime) else lance.dataset(self.table_uri, version=version)
                # this PINNED dataset carries the receipt's version, so a travelling read keys on whichever generation
                # it actually read rather than on whatever head the projector would re-open behind it.
                return self._receipt(op, quantity=ds.count_rows(filter=predicate), unit=LakeUnit.ROWS, handle=ds)
            case TableFormat.LANCE, LakeOp(tag="delete", delete=(predicate,)):
                lance.dataset(self.table_uri).delete(predicate)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="update", update=(predicate, updates)):
                lance.dataset(self.table_uri).update(dict(updates.items()), where=predicate)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="merge", merge=(_predicate, updates, _delete_unmatched)):
                # `merge_insert(on)` keys on column name(s), so the join key is the update columns, not the `predicate`.
                builder = lance.dataset(self.table_uri).merge_insert(list(updates.keys()))
                builder.when_matched_update_all().when_not_matched_insert_all().execute(data)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="index", index=(column, kind, metric)):
                ds = lance.dataset(self.table_uri)
                # `metric` binds only the IVF vector families; scalar/FTS ride `create_scalar_index`, routed by `_VECTOR_INDEX`.
                ds.create_index(column, index_type=kind, metric=metric) if kind in _VECTOR_INDEX else ds.create_scalar_index(column, index_type=kind)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(target_size, _zorder, _partition)):
                metrics = lance.dataset(self.table_uri).optimize.compact_files(target_rows_per_fragment=target_size)
                return self._receipt(op, quantity=metrics.fragments_added, unit=LakeUnit.FRAGMENTS)
            case TableFormat.LANCE, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                # real dry-run: version history older than the retention window counts off `versions()` — timestamps
                # arrive naive-UTC, so the cutoff strips its zone — while the wet leg reports the provider's own
                # `old_versions` tally, so removed evidence stays honest on both legs.
                ds = lance.dataset(self.table_uri)
                cutoff = datetime.now(UTC).replace(tzinfo=None) - _age(retention_hours)
                aged = sum(1 for row in ds.versions() if row["timestamp"] < cutoff)
                expired = aged if dry_run else ds.cleanup_old_versions(older_than=_age(retention_hours)).old_versions
                return self._receipt(op, quantity=expired, unit=LakeUnit.SNAPSHOTS)
            case TableFormat.LANCE, LakeOp(tag="restore", restore=(target,)):
                # `restore()` re-heads a prior snapshot; `int` pins `version=`, `datetime` via `asof=`.
                ds = lance.dataset(self.table_uri, version=target) if isinstance(target, int) else lance.dataset(self.table_uri, asof=target)
                ds.restore()
                return self._receipt(op)
            case TableFormat.DUCKLAKE, LakeOp(tag="ensure", ensure=layout):
                # `INSERT INTO` on the append arm needs its relation present, and only `overwrite` carries its own
                # `CREATE OR REPLACE`, so an append-planned DuckLake residence arms HERE or nowhere.
                # `CREATE TABLE IF NOT EXISTS … AS SELECT` off the registered zero-row frame lets DuckDB derive every
                # column type from the Arrow schema, and `SET PARTITIONED BY` re-declares the same terms idempotently.
                with self._ducklake(layout.empty()) as (con, table):
                    con.execute(f"CREATE TABLE IF NOT EXISTS {table} AS SELECT * FROM payload")
                    terms = ", ".join(layout.ducklake_terms())
                    if terms:
                        con.execute(f"ALTER TABLE {table} SET PARTITIONED BY ({terms})")
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
                    # catalog travel spells `AT (VERSION => n)` for a snapshot int and `AT (TIMESTAMP => ts)` for a
                    # datetime; reach refused the tag shape, so the operand's own type picks the key and the value
                    # still binds as a parameter.
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
                # DuckDB owns native `MERGE INTO`; update columns drive both clauses, `delete_unmatched` appends the by-source delete.
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
                    # `table_changes(table, start, end)` takes two BOUND snapshot ids, so an open-ended request resolves
                    # its end to the catalog head off the same `snapshots()` view `_snapshot` reads — a NULL bound is not
                    # what a Delta `ending_version=None` means by "latest". Rows ride the receipt, never a count alone.
                    feed = con.execute(
                        "SELECT * FROM table_changes(?, ?, ?)", [self.identifier, start, end if end is not None else _head(con)]
                    ).to_arrow_table()
                    return self._receipt(op, quantity=feed.num_rows, unit=LakeUnit.ROWS, payload=feed, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="optimize"):
                with self._ducklake(data) as (con, _table):
                    con.execute("CALL ducklake_merge_adjacent_files('lake')")
                    return self._receipt(op, handle=con)
            case TableFormat.DUCKLAKE, LakeOp(tag="vacuum", vacuum=(retention_hours, dry_run)):
                with self._ducklake(data) as (con, _table):
                    interval = retention_hours or _DEFAULT_RETENTION_HOURS
                    con.execute(f"CALL ducklake_expire_snapshots('lake', older_than => now() - INTERVAL '{interval} hours', dry_run => {dry_run})")
                    if not dry_run:
                        con.execute("CALL ducklake_cleanup_old_files('lake', cleanup_all => true)")
                    return self._receipt(op, handle=con)
            case TableFormat.PARQUET, LakeOp(tag="write", write=(mode, partition_by, _evolve, tuning)):
                # this tree APPENDS by GENERATION: `overwrite_or_ignore` overwrites every file whose basename matches,
                # so the default fixed template makes each generation clobber the previous one's `part-0` — the
                # provider's own contract names a per-write basename as what turns that policy into an append. The
                # token is the frame's own content digest, so a retried write is idempotent and two distinct frames
                # stay disjoint, and that SAME digest keys the receipt because a tree holds no snapshot to key on.
                return ContentIdentity.of("lake", arrow_bytes(data)).bind(
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
                            op, key=key, added=landed.files, byte_length=landed.byte_length, quantity=data.num_rows, unit=LakeUnit.ROWS
                        )
                    )
                )
            case TableFormat.PARQUET, LakeOp(tag="read"):
                # reach refused every travelling and predicate-scoped read, so this is the metadata-only count every
                # sibling `Read` answers: hive segments rejoin through the dataset's own partitioning discovery, and
                # `filesystem` authenticates that listing rather than whatever ambient credentials the run carries.
                counted = pads.dataset(self.table_uri, format="parquet", partitioning="hive", filesystem=self.filesystem).count_rows()
                return self._receipt(op, quantity=counted, unit=LakeUnit.ROWS)
            case _, _:
                return Error(BoundaryFault(boundary=(self._subject(op), LakeRefusal.UNARMED)))

    @contextmanager
    def _ducklake(self, data: pa.Table | None) -> "Iterator[tuple[duckdb.DuckDBPyConnection, str]]":
        # one shared-session bracket owns every DuckLake arm: the `Attach` row carries the extension, the DSN target,
        # and the catalog selection as session DATA the session rail executes, so this bracket adds only the carried
        # payload registration and the quoted identifier that rides out for every arm to reuse.
        session = DuckDbSession(attach=(Attach(alias="lake", target=f"ducklake:{self.dsn}", kind=DuckDbExtension.DUCKLAKE, current=True),))
        with session.connect() as con:
            if data is not None:
                con.register("payload", data)
            yield con, quote_ident(self.identifier)

    def _iceberg(self) -> "Table":
        return load_catalog(self.catalog).load_table(self.identifier)

    def _snapshot(self, op: LakeOp, handle: Any | None) -> tuple[int, int, int, int]:
        # version beside the COMMIT's OWN churn — never the snapshot's file roster, which counts every file the table
        # holds: an append of one generation reported the whole residence's file count and total volume as that
        # append's evidence, so the metric spine read a table size as a commit rate and the ledger re-priced the
        # entire plane on every write. `handle` is the provider object the arm already opened, so a receipt costs no
        # second log load and a time-travelling read keys on the version it pinned rather than on HEAD. Formats
        # publishing no per-commit roster report the zeros they can prove, and an arm holding its own evidence overrides.
        # A read PINNING an int version answers THAT snapshot whatever its handle stands at — the iceberg scan takes
        # `snapshot_id` beside a catalog table left at HEAD, so reading the handle back keys a travelling read on a
        # generation it never touched and collapses two versions onto one content key. A read commits nothing, so its
        # churn slots are zero by construction rather than by an arm that remembers to say so.
        if op.tag == "read" and isinstance(op.read[0], int):
            return op.read[0], 0, 0, 0
        match self.table_format:
            case TableFormat.DELTA:
                table = handle if handle is not None else DeltaTable(self.table_uri)
                return (table.version(), *_delta_churn(table, op))
            case TableFormat.ICEBERG:
                history = (handle if handle is not None else self._iceberg()).inspect.snapshots()
                return (history.column("snapshot_id")[-1].as_py() if history.num_rows else 0), 0, 0, 0
            case TableFormat.LANCE:
                return (handle if handle is not None else lance.dataset(self.table_uri)).version, 0, 0, 0
            case TableFormat.DUCKLAKE:
                # rides the SAME attached connection the arm holds — `snapshots()` is
                # attachment-scoped, so no second `ATTACH`.
                return _head(handle), 0, 0, 0
            case TableFormat.PARQUET:
                # a tree carries no snapshot: the write arm answers its own file, byte, and identity evidence off
                # each generation it lands, and the read arm reports none rather than inventing a version.
                return 0, 0, 0, 0
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
        added: int | None = None,
        removed: int | None = None,
        byte_length: int | None = None,
        payload: pa.Table | None = None,
        handle: Any | None = None,
    ) -> "RuntimeRail[LakeReceipt]":
        # `files_added`/`files_removed`/`byte_length` carry the OPERATION's own file and volume evidence — the commit
        # log's churn metrics, each provider's removed-file tally, and the written volume where a writer measures one —
        # while `(quantity, unit)` carry the arm's own measure, so no consumer reading file churn is handed a row count
        # under a field named for files. An arm answering its own evidence overrides the projection; `key` likewise,
        # because a format with no snapshot keys every commit on `uri@0` and collapses every generation onto one identity.
        version, churn_added, churn_removed, churn_bytes = self._snapshot(op, handle)
        identity = Ok(key) if key is not None else ContentIdentity.of("lake", f"{self.table_uri}@{version}".encode())
        return identity.map(
            lambda resolved: LakeReceipt(
                table_uri=self.table_uri,
                table_format=self.table_format,
                operation=op.tag,
                version=version,
                files_added=churn_added if added is None else added,
                files_removed=churn_removed if removed is None else removed,
                byte_length=churn_bytes if byte_length is None else byte_length,
                quantity=quantity,
                unit=unit,
                matched=matched,
                content_key=resolved,
                payload=payload,
            )
        )


# --- [TABLES] ---------------------------------------------------------------------------

_DEFAULT_RETENTION_HOURS: Final[int] = 168

_VECTOR_INDEX: Final[frozenset[str]] = frozenset({"IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"})

# Delta WriteMode projected onto Lance `create|overwrite|append`: `error`/`ignore`->`create`; `ignore`
# no-ops on an existing dataset via `_lance_exists`, Lance owning no native ignore mode.
_LANCE_MODE: Final[Map[WriteMode, LanceMode]] = Map.of_seq([
    ("error", "create"),
    ("ignore", "create"),
    ("overwrite", "overwrite"),
    ("append", "append"),
])

# `_COMMIT_METRIC` rows the Delta commit log's OWN churn keys per operation as `(added, removed, volume)`. delta-rs
# spells one metric roster per operation and those spellings genuinely diverge — snake for write/delete/update, a
# `num_target_*` pair for merge, camelCase for optimize — so divergence is a row and no arm carries a second copy of
# any key. Slot three names whichever JSON-encoded file summary carries `totalSize`, which optimize alone publishes:
# write commits report files and rows and NO byte volume, so a Delta append's byte evidence is honestly zero rather
# than a number invented for it. Operations absent here answer their own churn on their arm (restore, vacuum) or move
# no files at all (schema alters publish a NULL metrics map), and every absent cell reports zero rather than a guess.
_COMMIT_METRIC: Final[Map[str, tuple[str, str, str | None]]] = Map.of_seq([
    ("write", ("num_added_files", "num_removed_files", None)),
    ("delete", ("num_added_files", "num_removed_files", None)),
    ("update", ("num_added_files", "num_removed_files", None)),
    ("merge", ("num_target_files_added", "num_target_files_removed", None)),
    ("optimize", ("numFilesAdded", "numFilesRemoved", "filesAdded")),
])

# `_REFUSAL` is the `(format, tag)` reach matrix: an absent cell is reachable and carries an executing arm, a present
# cell refuses with its own row reason. The matrix outranks the arms, so a fifth format lands its unreachable cells as
# rows and its reachable cells as arms, and `_apply`'s tail catches an admitted cell no arm executes.
_REFUSAL: Final[Map[tuple[TableFormat, str], LakeRefusal]] = Map.of_seq([
    ((TableFormat.DELTA, "index"), LakeRefusal.DELTA_NO_INDEX),
    ((TableFormat.ICEBERG, "update"), LakeRefusal.ICEBERG_NO_UPDATE),
    ((TableFormat.ICEBERG, "optimize"), LakeRefusal.ICEBERG_NO_OPTIMIZE),
    ((TableFormat.ICEBERG, "changefeed"), LakeRefusal.ICEBERG_NO_CHANGEFEED),
    ((TableFormat.ICEBERG, "index"), LakeRefusal.ICEBERG_NO_INDEX),
    ((TableFormat.LANCE, "evolve"), LakeRefusal.LANCE_NO_EVOLVE),
    ((TableFormat.LANCE, "changefeed"), LakeRefusal.LANCE_NO_CHANGEFEED),
    ((TableFormat.DUCKLAKE, "evolve"), LakeRefusal.DUCKLAKE_NO_EVOLVE),
    ((TableFormat.DUCKLAKE, "index"), LakeRefusal.DUCKLAKE_NO_INDEX),
    ((TableFormat.DUCKLAKE, "restore"), LakeRefusal.DUCKLAKE_NO_RESTORE),
    # these cells carry the non-transactional tree's WHOLE class rather than a sentence beside the residence family —
    # which is what makes `ResidenceRow.degrade` derive it and `write`/`read` the only cells this format arms.
    ((TableFormat.PARQUET, "delete"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "update"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "merge"), LakeRefusal.PARQUET_NO_TRANSACTION),
    ((TableFormat.PARQUET, "evolve"), LakeRefusal.PARQUET_NO_EVOLVE),
    ((TableFormat.PARQUET, "optimize"), LakeRefusal.PARQUET_NO_OPTIMIZE),
    ((TableFormat.PARQUET, "vacuum"), LakeRefusal.PARQUET_NO_VACUUM),
    ((TableFormat.PARQUET, "changefeed"), LakeRefusal.PARQUET_NO_CHANGEFEED),
    ((TableFormat.PARQUET, "index"), LakeRefusal.PARQUET_NO_INDEX),
    ((TableFormat.PARQUET, "restore"), LakeRefusal.PARQUET_NO_TRAVEL),
    # a hive tree holds no table OBJECT to author: its directory appears with the first file the write lands, so the
    # COLD residence derives an `Ensure`-free INGEST plan off this cell rather than declaring one by hand.
    ((TableFormat.PARQUET, "ensure"), LakeRefusal.PARQUET_NO_TABLE_SPEC),
])

# ducklake's partition-function grammar: `identity` is the BARE column and every other term a SQL call over it,
# `bucket` alone leading with its width. `truncate` has no row because the engine refuses that function by name,
# which is what `_conditional` reads to refuse the cell rather than silently dropping a declared transform.
_DUCKLAKE_PARTITION: Final[Map[PartitionTransform, str]] = Map.of_seq([
    ("year", "year"),
    ("month", "month"),
    ("day", "day"),
    ("hour", "hour"),
    ("bucket", "bucket"),
])


class _Admission(Struct, frozen=True):
    # co-located with its table: `kinds` empty states that the ref's source shape is NOT this format's discriminant —
    # a Lance directory and a DuckLake catalog carry no columnar scan shape — while `needs` names the coordinates
    # `open` cannot construct the handle without, so a fifth format lands one row instead of a fourth `if`.
    kinds: frozenset[DatasetKind] = frozenset()
    needs: tuple[str, ...] = ()


_ADMIT: Final[Map[TableFormat, _Admission]] = Map.of_seq([
    (TableFormat.DELTA, _Admission(kinds=frozenset({DatasetKind.DELTA, DatasetKind.RECEIPTS}))),
    (TableFormat.ICEBERG, _Admission(kinds=frozenset({DatasetKind.ICEBERG}), needs=("catalog", "identifier"))),
    (TableFormat.LANCE, _Admission()),
    (TableFormat.DUCKLAKE, _Admission(needs=("dsn", "identifier"))),
    (TableFormat.PARQUET, _Admission(kinds=frozenset({DatasetKind.PARQUET}))),
])

# `WriteMode` projected onto the tree writer's collision policy: `append`/`ignore` land a generation beside the
# existing tree, `overwrite` clears every touched partition first, and `error` refuses a populated tree outright.
_PARQUET_EXISTING: Final[Map[WriteMode, DatasetWrite]] = Map.of_seq([
    ("error", "error"),
    ("ignore", "overwrite_or_ignore"),
    ("append", "overwrite_or_ignore"),
    ("overwrite", "delete_matching"),
])

# `Compression` projected onto the tree writer's OWN codec vocabulary, the third row table stating a provider
# dialect divergence beside `_LANCE_MODE` and `_PARQUET_EXISTING`. The writer roster is the transactional
# formats' spelling: the tree's file options reject `UNCOMPRESSED` outright and spell it `none`, and they reach
# no lz4-raw codec at all — `lz4` there writes the plain LZ4 parquet codec, so a row mapping `LZ4_RAW` onto it
# answers a policy nobody selected. The absent row is what `_conditional` reads to refuse that cell by name.
_PARQUET_CODEC: Final[Map[Compression, str]] = Map.of_seq([
    ("UNCOMPRESSED", "none"),
    ("SNAPPY", "snappy"),
    ("GZIP", "gzip"),
    ("BROTLI", "brotli"),
    ("LZ4", "lz4"),
    ("ZSTD", "zstd"),
])

# `_RECEIPT_SCHEMA` pins the durable evidence schema, so every writer and every engine reading the residence agrees
# byte-for-byte. `facts` is a MAP rather than a widened column set: one column per fact key would grow the schema on
# every producer edit and force a migration to hold a dimension this plane exists to keep, and `fact_keys` beside it
# is what a key-existence predicate prunes on before the map opens.
_RECEIPT_SCHEMA: Final[pa.Schema] = pa.schema([
    pa.field("at", pa.timestamp("us", tz="UTC"), nullable=False),
    pa.field("date", pa.date32(), nullable=False),
    pa.field("domain", pa.string(), nullable=False),
    pa.field("kind", pa.string(), nullable=False),
    pa.field("owner", pa.string(), nullable=False),
    pa.field("phase", pa.string(), nullable=False),
    pa.field("subject", pa.string(), nullable=False),
    pa.field("tenant", pa.string(), nullable=False),
    pa.field("content_key", pa.string(), nullable=False),
    pa.field("facts", pa.map_(pa.string(), pa.string()), nullable=False),
    pa.field("fact_keys", pa.list_(pa.string()), nullable=False),
])

# `_LIFTED` names the evidence contract as data: every key a tabular `contribute` promises, lifted out of the open map
# into its own typed column. Producers spelling one of these inside `facts` lose that typed column and its pruning.
_LIFTED: Final[frozenset[str]] = frozenset({"domain", "kind", "owner", "subject", "key"})

# `_RESIDENCE` rows the residence family: capability columns plus a DERIVED degradation, exactly as the deploy plane's
# metrics-store family is rowed. Partition columns are ROW DATA, so a residence partitioning on a different pair is one
# edit and no arm carries a literal. `EVIDENCE` clusters on `(tenant, content_key)` because single-tenant incident
# reads and content-key joins are the two queries this plane exists to answer, and Delta clusters on its own pass.
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
            retain="the MAINTAIN plan's Vacuum against the table's own log-retention window",
            retain_hours=None,
            fits="mutable evidence carrying time travel and a change feed",
            ingest="`Lakehouse.sink` appends one `write_deltalake` commit per receipt drain through the INGEST plan",
            # delta arms from the pinned receipt schema and its own hive columns; `identity` is the only transform
            # this format reaches, so the layout states the pair the write already partitions on and nothing more.
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
            # write refuses a partition roster on this format — layout is the table spec `Ensure` authors — so this
            # row states an empty roster rather than handing `Write` an operand reach would refuse.
            partition_by=(),
            zorder=(),
            tuning=WriteTuning(compression="ZSTD"),
            tenancy="tenant buckets in the spec `Ensure` authors, so a tenant predicate prunes without a directory per tenant",
            retain="the MAINTAIN plan's Vacuum expiring snapshots older than the window",
            retain_hours=None,
            fits="catalog-governed multi-engine read where a foreign engine holds the catalog",
            ingest="`Lakehouse.sink` appends through the catalog transaction the INGEST plan's own `Ensure` authored",
            # this row is why the operation exists: a catalog-governed residence stayed DECLARABLE and unarmable,
            # waiting on a foreign engine to plant it. Iceberg reaches the full transform set, so the spec bucket-hashes
            # tenant rather than pathing one directory per tenant, and days the timestamp its own predicates scan.
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
            retain="object-plane lifecycle at the deploy plane; the MAINTAIN plan derives EMPTY off the refusal rows",
            retain_hours=None,
            fits="cold tail, cheapest per byte, whole-partition batch scan",
            ingest="`Lakehouse.sink` lands one `ColumnarEgress.Dataset` generation per drain; the tree carries no log to commit to",
        ),
    ),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _conditional(table_format: TableFormat, op: LakeOp) -> Option[LakeRefusal]:
    # operand-conditional cells: reach turns on the op's own payload, which a `(format, tag)` key cannot see,
    # so each row still answers with a typed `LakeRefusal` and never an arm-level sentence.
    match table_format, op:
        case TableFormat.DELTA, LakeOp(tag="evolve", evolve=(_adds, drops, renames, _constraints)) if drops or renames:
            return Some(LakeRefusal.DELTA_COLUMN_SURGERY)
        case _, LakeOp(tag="ensure", ensure=layout) if not layout.widths_paired:
            # a layout fact, not a format one: an unpaired width renders a token no grammar parses, so the reach gate
            # answers it ahead of every arm rather than letting whichever provider the residence rides fail on it.
            return Some(LakeRefusal.PARTITION_TRANSFORM_WIDTH)
        case _, LakeOp(tag="ensure", ensure=layout) if layout.sort_by and table_format is not TableFormat.ICEBERG:
            # a declared sort order that lands nowhere silently drops the read policy a residence declared, so every
            # format but the catalog one refuses the operand rather than planting a table missing half its spec.
            return Some(LakeRefusal.SORT_ORDER_CATALOG_ONLY)
        case TableFormat.DELTA, LakeOp(tag="ensure", ensure=layout) if not layout.identity_only:
            return Some(LakeRefusal.DELTA_PARTITION_TRANSFORM)
        case TableFormat.LANCE, LakeOp(tag="ensure", ensure=layout) if layout.partition_by:
            return Some(LakeRefusal.LANCE_NO_TABLE_SPEC)
        case TableFormat.DUCKLAKE, LakeOp(tag="ensure", ensure=layout) if any(
            transform not in _DUCKLAKE_PARTITION and transform != "identity" for _column, transform, _width in layout.partition_by
        ):
            return Some(LakeRefusal.DUCKLAKE_PARTITION_TRUNCATE)
        case TableFormat.ICEBERG, LakeOp(tag="write", write=(_mode, partition_by, _evolve, _tuning)) if partition_by:
            return Some(LakeRefusal.ICEBERG_PARTITION_SPEC)
        case TableFormat.ICEBERG, LakeOp(tag="write", write=("error", _partition_by, _evolve, _tuning)):
            return Some(LakeRefusal.ICEBERG_WRITE_EXISTS)
        case TableFormat.ICEBERG, LakeOp(tag="evolve", evolve=(_adds, _drops, _renames, constraints)) if constraints:
            return Some(LakeRefusal.ICEBERG_CONSTRAINTS)
        case TableFormat.ICEBERG, LakeOp(tag="restore", restore=(target,)) if not isinstance(target, int):
            return Some(LakeRefusal.ICEBERG_SNAPSHOT_ID)
        case TableFormat.ICEBERG, LakeOp(tag="read", read=(version, _columns, _predicate)) if version is not None and not isinstance(version, int):
            # `iceberg_scan` reads the current snapshot and the catalog scan pins an int `snapshot_id`, so a tag
            # or timestamp has no arm — admitting one silently reads HEAD under a pinned request.
            return Some(LakeRefusal.ICEBERG_READ_SNAPSHOT_ID)
        case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(_target_size, zorder, _partition)) if zorder:
            return Some(LakeRefusal.LANCE_NO_ZORDER)
        case TableFormat.LANCE, LakeOp(tag="optimize", optimize=(_target_size, _zorder, partition)) if partition:
            return Some(LakeRefusal.LANCE_NO_PARTITION_FILTER)
        case TableFormat.DUCKLAKE, LakeOp(tag="optimize", optimize=(_target_size, _zorder, partition)) if partition:
            return Some(LakeRefusal.DUCKLAKE_OPTIMIZE_PARTITION)
        case TableFormat.DUCKLAKE, LakeOp(tag="optimize", optimize=(target_size, zorder, _partition)) if target_size is not None or zorder:
            # `ducklake_merge_adjacent_files` takes neither a target size nor a clustering key, so both operands
            # would vanish into a bare compaction the caller believes it tuned.
            return Some(LakeRefusal.DUCKLAKE_OPTIMIZE_TUNING)
        case TableFormat.DUCKLAKE, LakeOp(tag="read", read=(str(), _columns, _predicate)):
            return Some(LakeRefusal.DUCKLAKE_READ_TAG)
        case TableFormat.PARQUET, LakeOp(tag="read", read=(version, _columns, predicate)) if version is not None or predicate:
            # a tree has no version to pin and the metadata count reaches no SQL predicate, so admitting either
            # operand silently answers a count over the WHOLE tree under a request that scoped it.
            return Some(LakeRefusal.PARQUET_READ_SCOPE)
        case TableFormat.PARQUET, LakeOp(tag="write", write=(_mode, _partition_by, _evolve, tuning)) if (
            _PARQUET_CODEC.try_find(tuning.compression).is_none()
        ):
            # `_PARQUET_CODEC` IS this format's codec reach: an unrowed policy value either raises an opaque Arrow
            # fault a caller reads as a write failure, or — worse — downgrades onto a neighbouring codec unasked.
            return Some(LakeRefusal.PARQUET_CODEC)
        case _, _:
            return Nothing


def _head(con: "duckdb.DuckDBPyConnection") -> int:
    # ONE catalog-head read: the snapshot projector and the open-ended change feed both resolve the current
    # DuckLake snapshot off the attachment-scoped `snapshots()` view, never two spellings of one statement.
    row = con.execute("SELECT max(snapshot_id) FROM snapshots()").fetchone()
    return int(row[0] or 0)


def receipt_frame(receipts: Iterable[Receipt], *, tenant: str, at: datetime) -> "RuntimeRail[pa.Table]":
    # `receipt_frame` IS the residence's one projection, folding through each receipt's OWN `project()` rather than
    # re-matching its cases: new `Receipt` cases therefore reach the evidence plane with zero edits here, and whichever
    # phase that union names becomes this row's phase. `_LIFTED` names the evidence contract as data — every key a
    # contributor promises lands in its typed column and every other key survives verbatim in the open map, so this
    # residence never trims a dimension and never guesses which dimension a producer considered important.
    return boundary("lake.receipts", lambda: _framed(tuple(_fact(receipt, tenant, at) for receipt in receipts)))


def _fact(receipt: Receipt, tenant: str, at: datetime) -> ReceiptFact:
    # every lifted column is TOTAL by construction. `domain` is a hive PATH SEGMENT, so an empty value writes a
    # `domain=` directory no predicate names and no compaction reaches — and this receipt union's rejected and drained
    # cases carry no `domain`, `kind`, or `key` fact at all, which is exactly the fault and drain evidence an incident
    # reconstruction reads. Producers' recorded metric domain answers first and the receipt OWNER answers otherwise, so
    # every partition value names a real producer; `kind` falls back to whichever phase the union always projects,
    # making "count rejections per producer per day" a partition-pruned scan rather than a full read. `None` facts drop
    # instead of rendering — `str(None)` writes the literal `"None"`, indistinguishable from a producer recording that
    # text and poisoning a clustering key with it.
    _level, phase, facts = receipt.project()
    held = {key: str(value) for key, value in facts.items() if key not in _LIFTED and value is not None}
    return ReceiptFact(
        at=at,
        date=at.date(),
        domain=_column(facts, "domain") or _column(facts, "owner"),
        kind=_column(facts, "kind") or phase,
        owner=_column(facts, "owner"),
        phase=phase,
        subject=_column(facts, "subject"),
        tenant=tenant,
        content_key=_column(facts, "key"),
        facts=Map.of_seq(sorted(held.items())),
        fact_keys=tuple(sorted(held)),
    )


def _column(facts: Mapping[str, object], key: str) -> str:
    # one rendering rule for every lifted column: an absent fact and a `None` fact both read empty, never `"None"`.
    held = facts.get(key)
    return "" if held is None else str(held)


def receipt_facts(table: pa.Table) -> "RuntimeRail[tuple[ReceiptFact, ...]]":
    # `receipt_facts` inverts `receipt_frame` exactly, so schema knowledge stays with the schema: consumers scanning
    # this residence through duckdb, daft, datafusion, polars, or Flight SQL land the SAME Arrow frame and decode it
    # here, which is what makes the engine tier a selection rather than one reconstruction path per engine.
    return boundary(
        "lake.receipts.decode",
        lambda: tuple(
            ReceiptFact(
                at=row["at"],
                date=row["date"],
                domain=row["domain"],
                kind=row["kind"],
                owner=row["owner"],
                phase=row["phase"],
                subject=row["subject"],
                tenant=row["tenant"],
                content_key=row["content_key"],
                facts=Map.of_seq(row["facts"]),
                fact_keys=tuple(row["fact_keys"]),
            )
            for row in table.select(_RECEIPT_SCHEMA.names).to_pylist()
        ),
    )


def _framed(rows: tuple[ReceiptFact, ...]) -> pa.Table:
    # column-wise construction against the pinned schema, matching the sibling `tabular/cost#COST` priced frame: a
    # row-wise build would infer the map and list types per batch and hand the writer a schema that drifts with the
    # widest row it happened to see.
    return pa.table(
        {
            "at": [row.at for row in rows],
            "date": [row.date for row in rows],
            "domain": [row.domain for row in rows],
            "kind": [row.kind for row in rows],
            "owner": [row.owner for row in rows],
            "phase": [row.phase for row in rows],
            "subject": [row.subject for row in rows],
            "tenant": [row.tenant for row in rows],
            "content_key": [row.content_key for row in rows],
            "facts": [list(row.facts.items()) for row in rows],
            "fact_keys": [list(row.fact_keys) for row in rows],
        },
        schema=_RECEIPT_SCHEMA,
    )


def _lance_exists(uri: str) -> bool:
    # existence probe backing the `ignore` no-op — `lance.dataset` raises `ValueError` when absent.
    try:
        lance.dataset(uri)
    except ValueError:
        return False
    return True


def _add_paths(uri: str) -> frozenset[str]:
    # the live add-action roster a write differences its own commit against, read BEFORE that commit. An absent table
    # has no roster, so a first commit's whole addition is its own volume.
    return frozenset(pa.table(DeltaTable(uri).get_add_actions()).column("path").to_pylist()) if DeltaTable.is_deltatable(uri) else frozenset()


def _added_bytes(table: "DeltaTable", prior: frozenset[str]) -> int:
    # `get_add_actions` answers an `arro3.core.Table`, NOT a `pyarrow.Table`, so the column crosses through `pa.table`
    # before any pyarrow read. It rosters every file the CURRENT snapshot holds, so the volume this commit landed is
    # the roster DIFFERENCE across it — a path absent from the prior roster is a file this write added. The key is the
    # partition-relative `path` the log itself records: a bare basename repeats under every partition directory, so
    # matching on one prices an unpartitioned append at the whole table and a partitioned one at zero.
    actions = pa.table(table.get_add_actions())
    return sum(
        size
        for path, size in zip(actions.column("path").to_pylist(), actions.column("size_bytes").to_pylist(), strict=True)
        if path not in prior
    )


def _delta_metric(metrics: dict[str, object], key: str) -> int:
    return int(metrics.get(key, 0))


def _delta_churn(table: DeltaTable, op: LakeOp) -> tuple[int, int, int]:
    # per-commit churn off the table's own log entry, read ONLY for a committing op: `history(1)` on a read or a
    # change feed answers the PREVIOUS write's numbers, so a metadata op would report a commit it never made.
    return (
        _COMMIT_METRIC.try_find(op.tag).map(lambda keys: _churn(table.history(1), keys)).default_value((0, 0, 0))
        if op.committing
        else (0, 0, 0)
    )


def _churn(entries: list[dict[str, Any]], keys: tuple[str, str, str | None]) -> tuple[int, int, int]:
    # a commit entry carries `operationMetrics` as NULL for the operations publishing none — a schema alter is one —
    # so the map defaults empty and every key reads zero rather than raising on a `None` subscript.
    metrics = (entries[0].get("operationMetrics") or {}) if entries else {}
    added, removed, volume = keys
    return _delta_metric(metrics, added), _delta_metric(metrics, removed), _delta_volume(metrics, volume)


def _delta_volume(metrics: dict[str, object], key: str | None) -> int:
    # optimize entries publish their added-file summary as a JSON STRING carrying `totalSize` beside the avg, min, and
    # max this receipt has no slot for, so volume decodes rather than reads — `int()` over that string raises, and a
    # missing row means the operation genuinely measured no bytes.
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
