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
- Boundary: no durable store, no schema migration, no global Delta or catalog connection, no blocking commit run inline on an event loop where `run_async` owns the band hop, and no bare `trace.get_tracer(scope)` beside the faults-owned `scoped` stamp that binds the version and semconv triple; the metadata-only `Read` count is not the read lane — column-projected zero-copy reads route to the `tabular/columnar#SCAN` reader, not this commit owner. Reject law is data: `_REFUSAL` rows every `(format, tag)` cell a provider surface cannot portably reach and `_conditional` rows every cell the op's own operands decide, each row carrying its `LakeRefusal` member as the reason the fault reports, so a reject is a table edit and never an arm spending itself on a sentence. `_reach` reads that matrix ahead of the hook point and the retry envelope, `_apply`'s `case _, _` tail answers an admitted cell no arm executes, and every reject returns `Error(LAKE_REFUSED.raised(...))` carrying the operation beside the typed member — never a silent no-op, never a `raise` into a `boundary` that re-keys and discards it, and never a hand-opened `stamina.retry_context` where `guarded_sync` owns the envelope.

```python signature
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


class Capability(StrEnum):
    # the capability SET a caller demands of a handle before it plans, keyed on the `LakeOp` tags the reach matrix
    # already rows. `demand` answers the MISSING subset rather than the first miss, which is what `_reach` could not
    # do: it short-circuits on one cell, so a planner needing travel AND a change feed learned about one of them,
    # fixed it, and came back to learn about the other. Members are the op tags, so a new operation joins the
    # vocabulary by being a `LakeOp` case and no second roster tracks the axis.
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
    # the monotone generation a caller CARRIES into a mutating op, so a commit lands only over the state that caller
    # read. The `ResourceGuard` stays beside it and answers a different question — it is the IN-PROCESS re-entry
    # refusal, and the page already concedes that cross-process coordination is the format's own optimistic-commit
    # protocol — while this fence makes that protocol addressable: `_exclusive` returning the commit's value with no
    # verdict could not distinguish "I hold the fence" from "nobody contended", which is the
    # `docs/laws/scars.md` `[DECISION_UNDERIVABLE_FROM_STATE]` shape. `app_id` is the caller's own writer identity,
    # which the Delta arm binds through `CommitProperties(app_transactions=Transaction(app_id, version))`
    # (`.api/deltalake.md:36`, `:27`) and proves against `transaction_version(app_id)` (`:144`); the object-plane arm
    # binds the same expectation through `PutMode.UpdateVersion` (`libs/python/.api/obstore.md:38`, `:91`) at the
    # `tabular/egress#EGRESS` `_reuse` seam, whose `RemoteIdentity.version` is the generation it compares.
    app_id: str
    expected: int

    def stale(self, held: int) -> "Option[BoundaryFault]":
        # `Nothing` IS holding the fence, so a caller reads a verdict rather than inferring one from a value that
        # looks the same either way; both generations ride the refusal because a stale holder needs to know how far.
        return Nothing if held == self.expected else Some(LAKE_STALE_FENCE.raised(str(self.expected), str(held)))


class Generation(Struct, frozen=True):
    # ONE version-lineage node: a table's own generation identity beside the generation it descends from and the
    # named refs pointing at it. `Ancestry` projects a `Block[Generation]` off each format's OWN history surface —
    # Delta `history(limit)` (`.api/deltalake.md:140`), Iceberg `inspect.snapshots()`, DuckLake `snapshots()`, Lance
    # `versions()` — every one of which this page already reads for a different purpose, so the projection composes
    # and mints no provider. It is DATA crossing the standing counter-edge: `graph/graph#GRAPH` walks it and the
    # composition root joins the `GraphResult.frame` back onto the scan plane by `node`, because `graph` sits at S1
    # composing runtime alone and a `tabular` owner therefore cannot import the rustworkx kernel. The name is
    # `Generation`, never `lineage`: `tabular/columnar#SCAN` `QueryReceipt.lineage_edges` holds that word for VALUE
    # attribution — which source column produced which output column — and one spelling per shared consumer is the
    # branch law (`libs/python/.planning/RULINGS.md` `[02]`, `docs/laws/scars.md` `[STRATA_TWIN]`).
    version: int
    parent: "Posture[int]"
    at: "Posture[datetime]"
    refs: tuple[str, ...] = ()


class LakeRefusal(StrEnum):
    # every refused `(format, op)` cell names its own reason: `_REFUSAL` rows the unconditional cells and
    # `_conditional` the operand-dependent ones, while the member value is the operator-facing evidence
    # `BoundaryFault` carries — so a reject stays a data row and no arm spends itself on a sentence.
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
    # a residence's commit plan splits by PHASE because the two phases run on different clocks: ingest fires per
    # drain and maintenance on the deploy plane's own schedule. Folding clustering into ingest z-orders the whole
    # evidence table on every receipt drain — the provider filters no partition unless one is passed — so a plane
    # taking a drain a second rewrites its entire history a second.
    INGEST = "ingest"
    MAINTAIN = "maintain"


class LakePlane(StrEnum):
    # what a handle's table IS to the evidence spine, declared at `open` and read only by the journal producer leg.
    # `LEDGER` marks the relations the `python:runtime/observability/journal#LEDGER` implementer commits its own
    # landings through: a producer leg there would record a fact whose landing re-enters this same commit, feeding
    # the stream from itself without bound. Residence emptiness cannot discriminate them — a journal commit carries
    # `residence=None` exactly as a caller's commit does — so the ledger's plane rides its own admitted coordinate.
    CALLER = "caller"
    LEDGER = "ledger"


class Residence(StrEnum):
    # analytics residences keyed by CAPABILITY, mirroring how the deploy plane parameterizes its metrics-store
    # family: each row answers fits, admit, tenancy, lifetime, and an honest degradation, so arming a residence
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
    # `(kind, name, target, drop)`: the version-REFERENCE authoring half of the travel vocabulary `Read` already
    # consumes. A named ref is what makes a tag-string read reachable without a foreign engine having authored the
    # tag out of band, which is the same out-of-band step `Ensure`'s arming argument exists to delete. `target` is
    # `None` for the current generation, and `drop` retires a ref rather than a second op naming the inverse.
    reference: tuple[Literal["tag", "branch"], str, int | None, bool] = case()
    # the version-lineage READ half: a bound answers this table's own generation edge roster, which
    # `graph/graph#GRAPH` then WALKS. It is a READING op — it opens no commit and touches no file — so `committing`
    # answers False and the bare boundary rail carries it beside `Read` and `ChangeFeed`.
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
    # pre-flight commit fact — a receipt exists only post-commit, so the veto edge fires this intent shape.
    table_uri: str
    table_format: TableFormat
    operation: str


# --- [RESIDENCE]

class EvidenceToken(StrEnum):
    # the evidence contract as a closed VOCABULARY rather than five string literals spelled at eight producers, mirrored
    # here as a frozenset and re-typed a third time at `tabular/cost#COST`. Every tabular `contribute` keys its facts
    # dict on these members and `_fact` reads them off the same roster, so a renamed evidence key breaks at the
    # vocabulary instead of leaving the lifted column, the priced plane, and the live series under three spellings
    # with nothing raising. The member VALUE is the wire key a producer writes, which is what makes the roster and
    # the facts map one thing rather than two that agree by convention.
    DOMAIN = "domain"
    KIND = "kind"
    OWNER = "owner"
    SUBJECT = "subject"
    KEY = "key"


# every member is lifted OUT of the open map into its own typed column; a producer spelling one inside `facts` loses
# that column and its pruning. The set DERIVES off the vocabulary, so a sixth lifted key is one member here.
_LIFTED: Final[frozenset[str]] = frozenset(EvidenceToken)


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
        # the transform VALUE is the function name — key identity, so no mapping table stands between them — and the
        # width leads the argument list for the parameterized pair.
        return Block.of_seq(
            quote_ident(column)
            if transform == "identity"
            else f"{transform}({quote_ident(column)})"
            if width is None
            else f"{transform}({width}, {quote_ident(column)})"
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
    # `lifetime` answers BOTH halves — how long a committed row survives AND which owner ends it — because a duration
    # naming no ender leaves a plane every reader assumes somebody else expires.
    lifetime: str
    # `lifetime_hours` carries that window as row data beside the prose naming its ending owner, so the plan holds no
    # literal and a residence keeping evidence longer is one row edit. `None` takes the format's own default window.
    lifetime_hours: int | None
    fits: str
    # Writers that FILL this plane, stated per row because each fill path genuinely differs: two rows commit
    # through a transactional log and the cold row lands a generation through the columnar egress. Leaving this
    # floor column off lets a caller hardcode which entrypoint fills which plane.
    admit: str
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
                planned = (*clustered, LakeOp.Vacuum(self.lifetime_hours, dry_run=False))
            case unreachable:
                assert_never(unreachable)
        return tuple(op for op in planned if _REFUSAL.try_find((self.table_format, op.tag)).is_none())


# mutation-edge hook point: the data composition fold registers the row; a VETO tap gates commit pre-flight.
LAKE_COMMIT_POINT: Final[HookPoint[LakeCommit]] = HookPoint(id="rasm.data.lakehouse.commit", payload=LakeCommit, modality=Modality(veto=None))

# this owner's two names spelled once — the receipt owner label the audit actor also identifies as, and the metric
# segment its content keys, its lifted facts, and its audit verbs all derive from — so a rename cannot leave a
# series, a durable column, and a verb standing under three spellings of one owner.
OWNER: Final[str] = "lakehouse"
DOMAIN: Final[str] = "lake"


class LakeReceipt(Struct, frozen=True):
    table_uri: str
    table_format: TableFormat
    operation: str
    # `Nothing` is a generation nothing answered — an unarmed catalog, or a hive tree that carries no snapshot at all
    # — where the `0` this replaces keyed every such receipt onto ONE content identity.
    version: "Option[int]"
    # per-COMMIT churn, never the snapshot's file roster: Delta answers both off its own log entry and the tree answers
    # whichever generation it landed. Reading a snapshot count here prices one append at the whole table's SIZE.
    # `Nothing` means the provider published no counter — a drifted metrics key, or a format that measures none — so
    # a consumer reading zero files knows the commit really moved none rather than that nobody looked.
    files_added: "Option[int]"
    files_removed: "Option[int]"
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
                    # `domain`/`kind`/`key` are the lifted evidence contract `receipt_frame` reads: the SAME pair this
                    # contributor hands `Metrics.record` beside the key it minted, so a durable row rejoins its series.
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
    # the durable half of a commit, and TWO exclusions carry it. A `LEDGER` plane is the evidence ledger's own
    # relations: a fact recorded there lands through this very commit, so the stream feeds itself without bound.
    # A residence commit is the receipt stream's own store: its fact drains as a receipt the root sinks BACK into
    # that residence, which commits again — the same regress one hop longer, and the reason `contribute` already
    # records no metric for it. Churn is the third gate: a metadata-only op moved no file and evidences nothing.
    # The verb derives from the op tag under the runtime `<domain>.<operation>` grammar, so a new `LakeOp` case
    # reaches the journal with zero rows here, and the meter carries the volume this operation's own provider
    # measured — a format reporting none records its audit line alone rather than an invented charge.
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
    # this slot carries the `remote_store` obstore-backed handle off the admitted ref, so the tree writer reaches an
    # object-plane prefix through the one Rust core, `STORE_RETRY` envelope, and credential resolution every other
    # branch read crosses. `UPath.fs` is the deleted form: it resolves fsspec's OWN `s3fs`/`gcsfs`/`adlfs` backend,
    # a second provider stack whose credentials, retries, and pool no estate config reaches.
    filesystem: Any | None = None
    # in-engine credential rows the DuckDB-backed arms carry: a DuckLake catalog whose `DATA_PATH` is an object-store
    # prefix and an `iceberg_scan` over `s3://` both resolve INSIDE the engine and cross no filesystem handle, so the
    # store bridge above cannot serve them. One row per identity, carried as session data the arms thread through.
    secrets: tuple[SecretRow, ...] = ()
    scope: ScopeKey = DEFAULT_SCOPE
    # the evidence-spine role this handle's table plays; only the durable-evidence ledger's own relations declare
    # `LEDGER`, and the default keeps every caller's table journalled without a keyword at the open site.
    plane: LakePlane = LakePlane.CALLER
    # app-neutral single-writer guard, one per opened owner: two same-process compositions racing one table die at
    # the guard as a typed refusal instead of a late provider commit conflict after the loser's full write. The
    # CROSS-process half is `fence` below, and the two answer different questions rather than one substituting for
    # the other — this one is in-process re-entry, that one is the generation this table last accepted.
    guard: ResourceGuard = field(default_factory=lambda: ResourceGuard("committing"))
    # the monotone generation this writer expects, carried by whichever composition READ the state it is about to
    # commit over. `Nothing` commits exactly as this owner always did, so the door opens without being forced.
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
        # ONE row read admits every format: the row's `kinds` set gates the ref shape and its `needs` roster gates the
        # coordinates, where three hand-written arms checked the ref kind on the Delta arm alone — so a Lance or
        # Iceberg handle opened over a CSV ref passed admission and died inside the provider with a decode fault.
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
        # `sink` IS the evidence plane's one ingest: the receipt stream lands as durable rows through the SAME commit
        # matrix a caller's table rides, so a residence inherits reach, veto, retry, span, and snapshot identity whole
        # and no second write path drifts from them. Both producer facts are REQUIRED: this page's own law says they
        # arrive from whichever composition DRAINED the receipts, and the defaults that stood here contradicted it
        # twice over — `at or datetime.now(UTC)` stamped the COMMIT clock where the observation clock was owed
        # (`libs/.planning/RULINGS.md` `[02]`: an evidence plane settles on its own stamped coordinate, never a
        # storage write clock), and `tenant=""` made an OMITTED tenancy indistinguishable from a deliberately
        # unattributed one at the two grains that read it — `tabular/journal#JOURNAL` `attributed=bool(tenant)` and
        # `tabular/cost#COST`'s slot key. `receipt_frame` already took both required; the defaults lived only here.
        # an EMPTY drain plans nothing: the frame answers the emptiness because the receipt stream is consumed
        # exactly once, and the empty plan's own `Ok(())` is the no-op result — arming and committing zero rows
        # would mint one snapshot per quiet drain, inflating the version history the retention owner walks and
        # reporting a commit rate on a plane nothing wrote.
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
        # `sink_async` exists because the receipt drain IS the async case: a composition draining its own receipt stream
        # on an event loop through the synchronous entrypoint stalls that loop for every commit it makes. Same
        # admission, same frame projection, same plan — `run_async` alone owns the band hop.
        match self._admitted(residence).bind(lambda row: receipt_frame(receipts, tenant=tenant, at=at).map(lambda frame: (row, frame))):
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
        return mismatch.map(lambda refusal: Error(LAKE_REFUSED.raised(residence.value, refusal.value))).default_value(Ok(row))

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
                    self._exclusive(lambda: guarded_sync(RetryClass.LAKE_COMMIT, self._apply, admitted, data, at=LAKE_COMMIT), LAKE_COMMIT)
                    if admitted.committing
                    else boundary(LAKE_READ, lambda: self._apply(admitted, data), catch=_commit_raises())
                ).bind(lambda rail: rail)
            )

    def _exclusive[T](self, commit: "Callable[[], RuntimeRail[T]]", at: "FaultRow[DataLeg]") -> "RuntimeRail[T]":
        # the guard wraps the WHOLE committing envelope — retry attempts included — so a second same-process
        # writer refuses immediately with the typed row rather than queueing behind a commit it will conflict with.
        try:
            with self.guard:
                return commit()
        except BusyResourceError:
            return Error(LAKE_CONTENDED.raised(at.point))

    async def _exclusive_async[T](self, commit: "Callable[[], Awaitable[RuntimeRail[T]]]", at: "FaultRow[DataLeg]") -> "RuntimeRail[T]":
        # sync guard, awaited body: `ResourceGuard` refuses re-entry rather than waiting, so holding it across
        # the awaited band hop is exactly the single-writer semantics — the second writer errors, nothing queues.
        try:
            with self.guard:
                return await commit()
        except BusyResourceError:
            return Error(LAKE_CONTENDED.raised(at.point))

    @beartype(conf=FAULT_CONF)
    async def run_async(self, op: LakeOp, data: pa.Table | None = None) -> "RuntimeRail[LakeReceipt]":
        # every arm below is a BLOCKING native leg — a Delta commit, a Lance compaction, a DuckLake attach — so
        # an async composition reaching the synchronous entrypoint stalls its whole event loop for the duration
        # of a commit. The awaitable leg reads the SAME gate and the SAME envelope selection over one `on_thread`
        # band hop, exactly as the sibling `tabular/egress#EGRESS` owner splits two entrypoints off one row set;
        # `_gated` crosses a `match` closed by `assert_never` here because the fenced leg is awaited. It is also the
        # ONE seat durable evidence lands from, under the runtime producer-seam law: recording suspends, so the
        # synchronous `run` records nothing and a composition wanting commit evidence reaches this leg — which every
        # async composition already does. The record rail BINDS into the verdict, so an armed plane refusing a commit
        # fact surfaces here while a composition that installed none folds to the lawful no-op and costs one block.
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
        # ONE prologue both entrypoints read: reach answers a refused cell before the commit VETO point fires and
        # before the retry envelope opens, then an admitted committing op fires the point pre-flight while `Read`,
        # `ChangeFeed`, and dry-run `Vacuum` pass untouched. The fire rail carries the SUBSCRIBER's fact, so the
        # admitted op is re-mapped back onto it rather than letting a foreign payload ride out as the gate's value.
        # The payload joins reach because it is the same class of bound: a feeding op with no frame is a cell no arm
        # can execute, and letting it through fires a veto point for a mutation that then dies inside the provider
        # with a fault a caller cannot tell from a transport failure.
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
        # the CROSS-PROCESS half the `ResourceGuard` cannot answer: that guard refuses in-process re-entry, and this
        # page already conceded that everything beyond it was "the format's own optimistic-commit protocol" —
        # unaddressable, so `_exclusive` returning the commit's value could not distinguish holding the fence from
        # nobody contending. A carried `Fence` makes it a VERDICT: the writer's own app id resolves the generation
        # this table last accepted from that writer through `transaction_version(app_id)` (`.api/deltalake.md:144`),
        # and a mismatch refuses by name carrying BOTH generations rather than surfacing later as an opaque commit
        # conflict. A handle carrying no fence commits exactly as before, so the door is opened without being forced.
        return self.fence.bind(
            lambda held: held.stale(_transaction_version(self.table_uri, held.app_id)).map(Error)
        ).default_value(Ok(op)) if op.committing else Ok(op)

    def _subject(self, op: LakeOp) -> str:
        return f"lake.{self.table_format}.{op.tag}"

    def _dimensions(self, op: LakeOp) -> dict[str, str]:
        return {"rasm.lake.format": self.table_format.value, "rasm.lake.op": op.tag}

    @beartype(conf=FAULT_CONF)
    def demand(self, capabilities: "Block[Capability]") -> "RuntimeRail[Block[Capability]]":
        # the SET question `_reach` cannot answer: it reads one cell and short-circuits, so a planner needing travel
        # AND a change feed learns about one, repairs it, and comes back to learn about the other. `ACCUMULATE`
        # (`runtime/reliability/faults.md` `traversed`, catalog `libs/python/.api/expression.md:126`) folds every
        # miss into ONE combined fault naming EVERY capability this handle's format cannot serve, so a refusal states
        # the whole gap. An admitted demand answers the set back, which is what makes it a value a planner threads.
        return traversed(capabilities.map(self._served), by=Disposition.ACCUMULATE).map(lambda _held: capabilities)

    def _served(self, capability: Capability) -> "RuntimeRail[Capability]":
        # ONE cell read shared by the demand fold and the per-op gate, so a capability set and a dispatched op can
        # never disagree about what this format reaches.
        return (
            _REFUSAL.try_find((self.table_format, capability.value))
            .map(lambda refusal: Error(LAKE_REFUSED.raised(capability.value, refusal.value)))
            .default_value(Ok(capability))
        )

    def _reach(self, op: LakeOp) -> "RuntimeRail[LakeOp]":
        # one matrix read decides reachability: the unconditional `_REFUSAL` row answers first, the operand-
        # conditional row second, and an admitted op falls through carrying itself onto the commit chain.
        return (
            _REFUSAL.try_find((self.table_format, op.tag))
            .or_else_with(lambda: _conditional(self.table_format, op))
            .map(lambda refusal: Error(LAKE_REFUSED.raised(op.tag, refusal.value)))
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
                return self._receipt(op, added=Posture(declared=0), removed=Posture(declared=0), byte_length=Posture(declared=0))
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
                    # the fence's app transaction rides the SAME commit properties the tuning row already carries, so
                    # `transaction_version(app_id)` reads back the generation THIS writer landed and the next fence
                    # proves against a number the format itself stamped (`.api/deltalake.md:36`, `:27`, `:144`).
                    commit_properties=_fenced_properties(tuning.commit_properties(), self.fence),
                    post_commithook_properties=tuning.hook_properties(),
                )
                # NO delta write, merge, or delete `operationMetrics` key carries bytes — every one is a file count,
                # a row count, or a duration. `get_add_actions` is the only byte truth the format publishes, carrying
                # per-file `size_bytes` off the current snapshot, so the arm differences that roster across the commit
                # it just made — the pre-write read is what makes the difference exact — and reports the volume THIS
                # write landed rather than the whole table's held size.
                table = DeltaTable(self.table_uri)
                return self._receipt(op, byte_length=Posture(declared=_added_bytes(table, prior)), handle=table)
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
                # STRUCTURALLY zero, not a forged one: an empty add-action roster is a snapshot holding no data
                # files, which genuinely accounts for zero bytes — the sum answers NULL over zero rows and the fold
                # is the arithmetic identity, not a stand-in for a measurement the provider withheld.
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
                # `num_output_rows` counts the REWRITTEN OUTPUT FILES — inserted plus updated plus COPIED — so it
                # exceeds the offered batch whenever an untouched row shares a rewritten file. The inserted and
                # updated slots are the honest pair, and they sum to `num_source_rows` by construction.
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
                return self._receipt(op, removed=Posture(declared=len(removed)), quantity=len(removed), unit=LakeUnit.FILES, handle=table)
            case TableFormat.DELTA, LakeOp(tag="changefeed", changefeed=(start, end)):
                # `load_cdf` answers an arro3 `RecordBatchReader`; the zero-copy PyCapsule re-import lands the pyarrow
                # frame this owner hands out, so a change-feed consumer folds the receipt payload instead of re-opening
                # `DeltaTable` behind the owner that already read it. The re-import STAYS pyarrow: the downstream
                # partition split needs `sort_by` and `isin`, neither of which `arro3.compute` spells, so the arro3
                # kernel substitution is refuted for this hop.
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
                    quantity=_delta_metric(metrics, "numRestoredFile").option().default_value(0),
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
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(None, _columns, predicate)):
                # a head read rides `iceberg_scan` with no catalog round-trip, the pyiceberg catalog staying write-only.
                with DuckDbSession(extensions=(DuckDbExtension.ICEBERG,), secrets=self.secrets).connect() as con:
                    where = f" WHERE {predicate}" if predicate else ""
                    rows = con.execute(f"SELECT count(*) FROM iceberg_scan({quote_literal(self.table_uri)}){where}").fetchone()[0]
                return self._receipt(op, quantity=int(rows), unit=LakeUnit.ROWS)
            case TableFormat.ICEBERG, LakeOp(tag="read", read=(version, _columns, predicate)):
                # EVERY travel shape resolves to the one int `snapshot_id` the catalog scan pins, so the
                # `int | str | datetime` vocabulary `Read` declares reaches iceberg exactly as it reaches delta. The
                # resolved id rides `pinned` because a tag or instant names a generation the handle's own HEAD does not
                # carry — reading the head back would key two generations onto one content key.
                table = self._iceberg()
                match _iceberg_snapshot(table, version):
                    case Option(tag="none"):
                        return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.ICEBERG_REF_ABSENT.value))
                    case Option(tag="some", some=snapshot_id):
                        counted = table.scan(row_filter=predicate or "true", snapshot_id=snapshot_id).count()
                        return self._receipt(op, quantity=counted, unit=LakeUnit.ROWS, pinned=Posture(declared=snapshot_id), handle=table)
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
                # ONE travel projection over the provider's own pair — `rollback_to_snapshot(snapshot_id)` for a
                # generation and `rollback_to_timestamp(timestamp_ms)` for an instant — both answering the same builder
                # the single `commit()` closes, so the `int | datetime` operand `Restore` declares reaches iceberg whole
                # instead of refusing the half the provider has served all along.
                manage = self._iceberg().manage_snapshots()
                (manage.rollback_to_snapshot(target) if isinstance(target, int) else manage.rollback_to_timestamp(_millis(target))).commit()
                return self._receipt(op)
            case TableFormat.ICEBERG, LakeOp(tag="reference", reference=(kind, name, target, False)):
                # the catalog's authoring pair takes the SNAPSHOT ID first and the ref name second, inverting the Lance
                # order, and takes no implicit default — so a create over the current head resolves that head here
                # rather than handing the provider a `None` id, and an empty table refuses rather than authoring a ref
                # against generation zero. `_ICEBERG_REFERENCE` rows which builder verb each `(kind, drop)` cell spells,
                # carried as NAME STRINGS so the lazy catalog import stays deferred until this seam resolves it.
                table = self._iceberg()
                match _iceberg_head(table, target):
                    case Option(tag="none"):
                        return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.ICEBERG_REF_ABSENT.value))
                    case Option(tag="some", some=snapshot_id):
                        getattr(table.manage_snapshots(), _ICEBERG_REFERENCE[(kind, False)])(snapshot_id, name).commit()
                        return self._receipt(op, handle=table.refresh())
            case TableFormat.ICEBERG, LakeOp(tag="reference", reference=(kind, name, _target, True)):
                # the retire pair takes the NAME alone, which is why the arity splits into two arms over one row table
                # rather than one arm re-deriving the argument list per cell.
                table = self._iceberg()
                getattr(table.manage_snapshots(), _ICEBERG_REFERENCE[(kind, True)])(name).commit()
                return self._receipt(op, handle=table.refresh())
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
                    max_rows_per_file=tuning.target_file_size or _LANCE_FRAGMENT_ROWS,
                    data_storage_version=tuning.data_storage_version,
                )
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="read", read=(version, _columns, predicate)):
                # this PINNED dataset carries the receipt's version, so a travelling read keys on whichever generation
                # it actually read rather than on whatever head the projector would re-open behind it.
                ds = _lance_travel(self.table_uri, version)
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
                # `restore()` re-heads whichever generation the ONE travel projection opened.
                ds = _lance_travel(self.table_uri, target)
                ds.restore()
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("tag", name, target, False)):
                # `tags.create(tag, reference=)` and `create_branch(branch, reference=)` both take the NAME first and
                # the generation second, and both accept the `int | str` reference vocabulary `Read` already carries —
                # so a tag over the current head passes `None` and one over a pinned generation passes that version.
                lance.dataset(self.table_uri).tags.create(name, target)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("branch", name, target, False)):
                lance.dataset(self.table_uri).create_branch(name, target)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("tag", name, _target, True)):
                lance.dataset(self.table_uri).tags.delete(name)
                return self._receipt(op)
            case TableFormat.LANCE, LakeOp(tag="reference", reference=("branch", name, _target, True)):
                # branch removal is the `branches` manager's, never the dataset's — `Branches.delete(branch)` beside
                # the `list`/`list_ordered` reads, the same split `tags` carries between `create` and `delete`.
                lance.dataset(self.table_uri).branches.delete(name)
                return self._receipt(op)
            case TableFormat.DUCKLAKE, LakeOp(tag="ensure", ensure=layout):
                # `INSERT INTO` on the append arm needs its relation present, and only `overwrite` carries its own
                # `CREATE OR REPLACE`, so an append-planned DuckLake residence arms HERE or nowhere.
                # `CREATE TABLE IF NOT EXISTS … AS SELECT` off the registered zero-row frame lets DuckDB derive every
                # column type from the Arrow schema, and `SET PARTITIONED BY` re-declares the same terms idempotently.
                with self._ducklake(layout.empty()) as (con, table):
                    # both statements ride the `tabular/columnar#SCAN` `DdlStep` carrier, so the verb and the
                    # re-application cost are DECLARED columns rather than an `IF NOT EXISTS` substring a reader
                    # greps for. The identifier positions still ride `quote_ident` because DuckDB parses a name
                    # there and admits no parameter marker; every VALUE binds.
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
                    # an UNARMED catalog answers no head at all, so an open-ended feed over one refuses by name
                    # rather than narrowing itself to the empty range a `0` bound silently produced.
                    match Option.of_optional(end).or_else(_head(con)):
                        case Option(tag="none"):
                            return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.UNARMED.value))
                        case Option(tag="some", some=bound):
                            feed = con.execute("SELECT * FROM table_changes(?, ?, ?)", [self.identifier, start, bound]).to_arrow_table()
                            return self._receipt(op, quantity=feed.num_rows, unit=LakeUnit.ROWS, payload=feed, handle=con)
            case _, LakeOp(tag="ancestry", ancestry=(bound,)):
                # ONE projection over each format's OWN history surface — Delta `history(limit)`
                # (`.api/deltalake.md:140`), Iceberg `inspect.snapshots()`, DuckLake `snapshots()`, Lance
                # `versions()` — every one of which this page already reads for a different purpose, so the roster
                # composes and opens no provider. Reach refuses PARQUET on the standing `PARQUET_NO_TRAVEL` row, so
                # this arm is total over the formats that carry a generation at all. The answer rides `payload` as
                # the edge FRAME `graph/graph#GRAPH` walks, never a second receipt family.
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
                    # every VALUE binds: `.api/duckdb.md:76` rules that a `CALL` of a table function binds its
                    # arguments positionally or by name, `arg => ?` included, and rejects string-interpolated SQL
                    # parameters outright — the f-string this replaces rendered the retention window and the dry-run
                    # flag straight into the statement text, which is the one form that catalog row names by shape.
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
                # this tree APPENDS by GENERATION: `overwrite_or_ignore` overwrites every file whose basename matches,
                # so the default fixed template makes each generation clobber the previous one's `part-0` — the
                # provider's own contract names a per-write basename as what turns that policy into an append. The
                # token is the frame's own content digest, so a retried write is idempotent and two distinct frames
                # stay disjoint, and that SAME digest keys the receipt because a tree holds no snapshot to key on.
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
                # reach refused every travelling and predicate-scoped read, so this is the metadata-only count every
                # sibling `Read` answers: hive segments rejoin through the dataset's own partitioning discovery, and
                # `filesystem` authenticates that listing rather than whatever ambient credentials the run carries.
                counted = pads.dataset(self.table_uri, format="parquet", partitioning="hive", filesystem=self.filesystem).count_rows()
                return self._receipt(op, quantity=counted, unit=LakeUnit.ROWS)
            case _, _:
                return Error(LAKE_REFUSED.raised(op.tag, LakeRefusal.UNARMED.value))

    @contextmanager
    def _ducklake(self, data: pa.Table | None) -> "Iterator[tuple[duckdb.DuckDBPyConnection, str]]":
        # one shared-session bracket owns every DuckLake arm: the `Attach` row carries the extension, the DSN target,
        # and the catalog selection as session DATA the session rail executes, so this bracket adds only the carried
        # payload registration and the quoted identifier that rides out for every arm to reuse. The handle's own
        # secret rows ride in beside the attach, because the session creates them BEFORE the `ATTACH` executes and a
        # catalog whose `DATA_PATH` is an object-store prefix resolves no credential without one.
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
        # the bound is the shared runtime `Depth`, so a walk to convergence is a NAMED case and a max-int spelling is
        # unrepresentable; a bounded roster takes that many newest generations and a fixpoint roster takes the whole
        # history the format holds. Each arm reads its provider's own history columns and nothing more.
        return boundary(LAKE_READ, lambda: self._history(bound), catch=_commit_raises())

    def _history(self, bound: Depth) -> "Block[Generation]":
        match self.table_format:
            case TableFormat.DELTA:
                # `history(limit)` answers newest-first; the parent is the previous version by construction, and the
                # oldest entry a bounded read returns carries an ABSENT parent because this read cannot see past it.
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
                # iceberg publishes the edge OUTRIGHT — `parent_id` beside `snapshot_id` — so no arm infers it.
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
                # PARQUET never reaches here — reach refuses it on the standing `PARQUET_NO_TRAVEL` row — and the
                # DuckLake catalog publishes its own `parent_id` beside each snapshot id.
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
        # version beside the COMMIT's OWN churn — never the snapshot's file roster, which counts every file the table
        # holds: an append of one generation reported the whole residence's file count and total volume as that
        # append's evidence, so the metric spine read a table size as a commit rate and the ledger re-priced the
        # entire plane on every write. `handle` is the provider object the arm already opened, so a receipt costs no
        # second log load and a time-travelling read keys on the version it pinned rather than on HEAD. Formats
        # publishing no per-commit roster report the zeros they can prove, and an arm holding its own evidence overrides.
        # A read PINNING an int version answers THAT snapshot whatever its handle stands at — the iceberg scan takes
        # `snapshot_id` beside a catalog table left at HEAD, so reading the handle back keys a travelling read on a
        # generation it never touched and collapses two versions onto one content key. A read commits nothing, so its
        # churn slots are ABSENT rather than zero: a format publishing no per-commit roster and an operation that made
        # no commit both answered `0` here, indistinguishable from a commit the provider really measured at zero.
        # An UNARMED catalog answers no generation at all, and a receipt over one keys nothing rather than `uri@0`.
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
                # rides the SAME attached connection the arm holds — `snapshots()` is
                # attachment-scoped, so no second `ATTACH`.
                return Posture.of_option(_head(handle)), *blank
            case TableFormat.PARQUET:
                # a tree carries no snapshot: the write arm answers its own file, byte, and identity evidence off
                # each generation it lands, and the read arm reports ABSENT rather than inventing a version.
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
        # `files_added`/`files_removed`/`byte_length` carry the OPERATION's own file and volume evidence — the commit
        # log's churn metrics, each provider's removed-file tally, and the written volume where a writer measures one —
        # while `(quantity, unit)` carry the arm's own measure, so no consumer reading file churn is handed a row count
        # under a field named for files. An arm answering its own evidence overrides the projection; `key` likewise,
        # because a format with no snapshot keys every commit on `uri@0` and collapses every generation onto one identity.
        # `pinned` is that same override on the generation itself: an arm resolving a tag or an instant to a snapshot id
        # names the generation it READ, where the projector would re-read the handle's head and key two apart
        # generations onto one identity. Every override rides `Posture`, because the `None` sentinels this replaces
        # fused "this arm measured nothing, take the projection" with "this arm measured ZERO" — a distinction the
        # page carried by COMMENT discipline alone (`:190` states it for the Delta `ignore` arm) and never by type.
        # `absent` therefore defers to the projector and `declared=0` states a measured zero the projector must not
        # overwrite; a generation nothing answered keys the receipt on the URI alone, never on `uri@0`.
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

# Lance fragment ceiling when the writer policy names no target size: a magnitude a provider call spells inline is
# invisible to `WriteTuning`, unoverridable from a `ResidenceRow`, and unreadable beside the retention window this
# page already names as a `Final` — so the fallback is a constant exactly as the window is.
_LANCE_FRAGMENT_ROWS: Final[int] = 1024 * 1024

_VECTOR_INDEX: Final[frozenset[str]] = frozenset({"IVF_PQ", "IVF_HNSW_PQ", "IVF_HNSW_SQ"})

# which catalog builder verb each `(ref kind, drop)` cell spells. The values are NAME STRINGS resolved at the call
# seam: a row dereferencing `ManageSnapshots.create_tag` here would reify the `lazy` pyiceberg import at module load
# and defeat the deferral every other iceberg surface on this page holds.
_ICEBERG_REFERENCE: Final[Map[tuple[str, bool], str]] = Map.of_seq([
    (("tag", False), "create_tag"),
    (("branch", False), "create_branch"),
    (("tag", True), "remove_tag"),
    (("branch", True), "remove_branch"),
])

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

# --- [ERRORS] ---------------------------------------------------------------------------


def _commit_raises() -> Catch:
    # every committing and reading arm on this owner. Each root is the provider's own declared rail: `DeltaError`
    # (`.api/deltalake.md:52`), the `pyiceberg.exceptions` family whose members share no root of their own
    # (`.api/pyiceberg.md:52-58`), `duckdb.Error` for the DuckLake and Iceberg SQL arms (`.api/duckdb.md:30`), and
    # `pa.ArrowException` beside `OSError` for the Arrow carrier and the object plane. `lance` publishes no exception
    # namespace and refuses an absent dataset with a bare `ValueError` (the shape `_lance_exists` already probes on),
    # so that builtin is named rather than a root the distribution does not carry.
    return (DeltaError, IcebergError, CommitFailedException, duckdb.Error, pa.ArrowException, ValueError, OSError)


# this module's whole raise roster. Reach refusals, admission mismatches, and the stale-fence verdict are TERMINAL —
# no re-issue of the same op against the same state clears them — while the fenced commit legs are TRANSIENT, since a
# commit conflict, a catalog round trip, or an object-plane read may clear on a re-offer. Near-identical defects
# collapse into ONE parameterized row: every reach refusal shares `LAKE_REFUSED` and names its own typed member.
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

# `_REFUSAL` is the `(format, tag)` reach matrix: an absent cell is reachable and carries an executing arm, a present
# cell refuses with its own row reason. The matrix outranks the arms, so a fifth format lands its unreachable cells as
# rows and its reachable cells as arms, and `_apply`'s tail catches an admitted cell no arm executes.
_REFUSAL: Final[Map[tuple[TableFormat, str], LakeRefusal]] = Map.of_seq([
    ((TableFormat.DELTA, "index"), LakeRefusal.DELTA_NO_INDEX),
    ((TableFormat.DELTA, "reference"), LakeRefusal.DELTA_NO_REFERENCE),
    ((TableFormat.DUCKLAKE, "reference"), LakeRefusal.DUCKLAKE_NO_REFERENCE),
    # the tree keeps no version history, so it names none — the same bound its refused `restore` states.
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
    # a hive tree keeps no generation roster to walk, so the version-lineage read refuses on the same travel row
    # every other travelling op does rather than answering an empty edge set a consumer would read as a root.
    ((TableFormat.PARQUET, "ancestry"), LakeRefusal.PARQUET_NO_TRAVEL),
    # a hive tree holds no table OBJECT to author: its directory appears with the first file the write lands, so the
    # COLD residence derives an `Ensure`-free INGEST plan off this cell rather than declaring one by hand.
    ((TableFormat.PARQUET, "ensure"), LakeRefusal.PARQUET_NO_TABLE_SPEC),
])

# ducklake's partition-function grammar: `identity` is the BARE column and every other term a SQL call over it,
# `bucket` alone leading with its width. The table this replaces mapped every key onto ITSELF, so it stated which
# transforms the engine reaches and nothing more — a five-row mirror of a vocabulary the transform's own value
# already spells, where a renamed member left the lookup resolving under two names with nothing raising. A membership
# SET says the same thing with zero mapping, and `ducklake_terms` spells the function name off the transform itself.
# `truncate` is absent because the engine refuses that function by name, which is what `_conditional` reads to refuse
# the cell rather than silently dropping a declared transform.
_DUCKLAKE_TRANSFORMS: Final[frozenset[PartitionTransform]] = frozenset({"year", "month", "day", "hour", "bucket"})


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

# ONE declaration of the durable evidence row as `tabular/interop#INTEROP` `ColumnSpec` rows: the Arrow schema, the
# commit builder, and the row decoder are three DERIVATIONS off this roster, where a twelfth column used to be four
# separate edits — a `pa.field`, a `ReceiptFact` slot, a `_framed` dict key, and a `receipt_facts` string subscript —
# any three of which still compiled with the fourth missed. `facts` is a MAP rather than a widened column set: one
# column per fact key would grow the schema on every producer edit and force a migration to hold a dimension this
# plane exists to keep, and `fact_keys` beside it is what a key-existence predicate prunes on before the map opens.
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

# the version-lineage EDGE roster as columns, so `Ancestry` lands its answer on the same `payload` slot the change
# feed rides and `graph/graph#GRAPH` walks it as the frame the standing counter-edge already carries. `node` and
# `parent` are the two columns the walk keys on, so the `GraphResult.frame` it answers left-joins back onto the scan
# plane by `node` with zero new seam. An absent parent is the ROOT generation, and a `-1` there would be a node the
# walk follows into nothing.
_GENERATION_COLUMNS: Final[Block[ColumnSpec[Generation, object]]] = Block.of_seq([
    ColumnSpec(name="node", arrow=pa.int64(), kind=int, lift=lambda row: row.version),
    ColumnSpec(name="parent", arrow=pa.int64(), kind=int, nullable=True, lift=lambda row: row.parent.option().to_optional()),
    ColumnSpec(name="at", arrow=pa.timestamp("us", tz="UTC"), kind=datetime, nullable=True, lift=lambda row: row.at.option().to_optional()),
    ColumnSpec(name="refs", arrow=pa.list_(pa.string()), kind=tuple, lift=lambda row: list(row.refs)),
])

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
            lifetime="a committed row survives to the table's own log-retention window; the MAINTAIN plan's Vacuum expires it, on the cadence the deploy plane schedules that plan",
            lifetime_hours=None,
            fits="mutable evidence carrying time travel and a change feed",
            admit="`Lakehouse.sink` appends one `write_deltalake` commit per receipt drain through the INGEST plan",
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
            lifetime="a committed snapshot survives to the window the format's own table properties declare; the MAINTAIN plan's Vacuum expires it on the deploy plane's cadence",
            lifetime_hours=None,
            fits="catalog-governed multi-engine read where a foreign engine holds the catalog",
            admit="`Lakehouse.sink` appends through the catalog transaction the INGEST plan's own `Ensure` authored",
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
            lifetime="a landed generation survives to the object-plane lifecycle rule the deploy plane sets, and that plane is its sole ender — the MAINTAIN plan derives EMPTY off the refusal rows, so this owner expires nothing",
            lifetime_hours=None,
            fits="cold tail, cheapest per byte, whole-partition batch scan",
            admit="`Lakehouse.sink` lands one `ColumnarEgress.Dataset` generation per drain; the tree carries no log to commit to",
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


def _fenced_properties(properties: "CommitProperties | None", fence: "Option[Fence]") -> "CommitProperties | None":
    # a handle carrying no fence commits under the tuning row's own properties untouched; a fenced one stamps its
    # writer identity and the generation it is advancing to, which is what makes the next `_fenced` read exact.
    return fence.map(
        lambda held: CommitProperties(
            custom_metadata=properties.custom_metadata if properties else None,
            max_commit_retries=properties.max_commit_retries if properties else None,
            app_transactions=[Transaction(app_id=held.app_id, version=held.expected + 1)],
        )
    ).default_value(properties)


def _transaction_version(uri: str, app_id: str) -> int:
    # the generation this table last accepted FROM this writer; `-1` is delta-rs's own "this app has never committed
    # here" answer, which is a real coordinate a fresh writer's `Fence(expected=-1)` matches rather than a sentinel
    # this page invents (`.api/deltalake.md:144`).
    return DeltaTable(uri).transaction_version(app_id)


def _limit(bound: Depth) -> int:
    # Delta's `history(limit)` takes a COUNT, and a fixpoint roster asks for the whole log — the format publishes no
    # unbounded spelling, so the fixpoint case reads the current version count, never a max-int a caller could not
    # survive. `Depth` refuses a bound below one at its own ingress, so the bounded arm needs no floor guard here.
    match bound:
        case Depth(tag="bounded", bounded=held):
            return held
        case Depth(tag="fixpoint"):
            return 0
        case _ as unreachable:
            assert_never(unreachable)


def _bounded(roster: "Block[Generation]", bound: Depth) -> "Block[Generation]":
    # ONE bound application for every format whose history surface takes no limit argument: newest generations first,
    # so a bounded roster is the same prefix Delta's own `history(limit)` answers and the two arms cannot disagree.
    ordered = roster.sort_with(lambda row: -row.version)
    return ordered if bound.tag == "fixpoint" else ordered.take(min(_limit(bound), len(ordered)))


def _instant(held: object) -> datetime:
    # Delta stamps its log entries in EPOCH MILLISECONDS; every other format on this page publishes a real timestamp.
    return datetime.fromtimestamp(int(held) / 1000.0, UTC) if isinstance(held, int) else held


def _head(con: "duckdb.DuckDBPyConnection") -> Option[int]:
    # ONE catalog-head read: the snapshot projector and the open-ended change feed both resolve the current
    # DuckLake snapshot off the attachment-scoped `snapshots()` view, never two spellings of one statement.
    # `max(snapshot_id)` over an EMPTY `snapshots()` view answers NULL, and the `or 0` this deletes read that as
    # generation ZERO — a forgery of IDENTITY, not of a measure: `_receipt` then keyed every pre-first-commit
    # DuckLake receipt as `uri@0`, collapsing every one of them onto a single content key, and the same read fed
    # `changefeed`'s open-ended end bound where 0 silently narrows the feed to nothing.
    row = con.execute("SELECT max(snapshot_id) FROM snapshots()").fetchone()
    return Option.of_optional(row[0]).map(int)


def receipt_frame(receipts: Iterable[Receipt], *, tenant: str, at: datetime) -> "RuntimeRail[pa.Table]":
    # `receipt_frame` IS the residence's one projection, folding through each receipt's OWN `project()` rather than
    # re-matching its cases: new `Receipt` cases therefore reach the evidence plane with zero edits here, and whichever
    # phase that union names becomes this row's phase. `_LIFTED` names the evidence contract as data — every key a
    # contributor promises lands in its typed column and every other key survives verbatim in the open map, so this
    # residence never trims a dimension and never guesses which dimension a producer considered important.
    return boundary(LAKE_RECEIPTS, lambda: _framed(Block.of_seq(_fact(receipt, tenant, at) for receipt in receipts)), catch=_commit_raises())


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
    # one rendering rule for every lifted column: an absent fact and a `None` fact both read empty, never `"None"`.
    held = facts.get(key)
    return "" if held is None else str(held)


def receipt_facts(table: pa.Table) -> "RuntimeRail[tuple[ReceiptFact, ...]]":
    # `receipt_facts` inverts `receipt_frame` exactly, so schema knowledge stays with the schema: consumers scanning
    # this residence through duckdb, daft, datafusion, polars, or Flight SQL land the SAME Arrow frame and decode it
    # here, which is what makes the engine tier a selection rather than one reconstruction path per engine.
    # the READ half of the same roster: every column resolves through its spec's own name and each row assembles
    # into the struct that roster generated, so the row-wise `row["at"]` string subscripts this deletes — a fourth
    # transcription of one declaration, and the one no compiler could ever check — are unspellable.
    return boundary(LAKE_RECEIPTS_DECODE, lambda: column_rows(table, _RECEIPT_COLUMNS, ReceiptFact), catch=_commit_raises())


def _framed(rows: "Block[ReceiptFact]") -> pa.Table:
    # column-wise construction bound to the schema the SAME roster generated, matching the sibling
    # `tabular/cost#COST` priced frame: a row-wise build would infer the map and list types per batch and hand the
    # writer a schema that drifts with the widest row it happened to see.
    return column_frame(_RECEIPT_COLUMNS, rows)


def _lance_exists(uri: str) -> bool:
    # existence probe backing the `ignore` no-op — `lance.dataset` raises `ValueError` when absent.
    try:
        lance.dataset(uri)
    except ValueError:
        return False
    return True


def _lance_travel(uri: str, target: int | str | datetime | None) -> Any:
    # ONE travel opening for every arm that pins a generation: an instant rides `asof=` and every other operand rides
    # `version=`, which spans the int snapshot and the tag string alike. Spelled twice as inline `isinstance` branches
    # the rule inverted between the read arm and the restore arm, so a tag reached `version=` on one and `asof=` on the
    # other — a divergence neither arm stated and the next travel operand would widen.
    return lance.dataset(uri, asof=target) if isinstance(target, datetime) else lance.dataset(uri, version=target)


def _millis(moment: datetime) -> int:
    # iceberg's travel and rollback members take epoch MILLISECONDS; seconds resolve to 1970 and answer the table's
    # first generation for every request rather than raising.
    return int(moment.timestamp() * 1000)


def _iceberg_snapshot(table: "Table", version: int | str | datetime) -> "Option[int]":
    # ONE travel resolution over every shape `Read` declares, each answering the int `snapshot_id` the catalog scan
    # pins: an int IS that id, a string names a tag or branch `snapshot_by_name` resolves, and an instant resolves
    # through `snapshot_as_of_timestamp`. `Nothing` is a ref the table does not hold — the arm's own refusal, never a
    # silent fall back to HEAD under a request that named a generation.
    match version:
        case int() as snapshot_id:
            return Some(snapshot_id)
        case str() as named:
            return Option.of_optional(table.snapshot_by_name(named)).map(lambda snapshot: snapshot.snapshot_id)
        case moment:
            return Option.of_optional(table.snapshot_as_of_timestamp(_millis(moment))).map(lambda snapshot: snapshot.snapshot_id)


def _iceberg_head(table: "Table", target: int | None) -> "Option[int]":
    # a reference authored over the CURRENT generation resolves that generation here, because the catalog's
    # authoring pair takes an explicit snapshot id and no default; an empty table carries no head, which refuses
    # rather than authoring a ref against generation zero.
    return Some(target) if target is not None else Option.of_optional(table.current_snapshot()).map(lambda snapshot: snapshot.snapshot_id)


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


def _delta_metric(metrics: dict[str, object], key: str) -> "Posture[int]":
    # a key the commit entry does NOT publish is `absent`, never zero. The `metrics.get(key, 0)` this deletes covered
    # only the DRIFT case — `_churn` already defaults the whole map empty for the operations publishing a NULL
    # metrics map — so a delta-rs release renaming `num_target_rows_inserted` or `num_target_rows_updated` read ZERO
    # inserted and ZERO updated rows with nothing raising, and the upsert receipt reported a landing that never
    # happened (`docs/laws/scars.md` `[ASSERTED_VALUE]`). Absent now names the drifted key at the receipt.
    return Posture.of_option(Option.of_optional(metrics.get(key)).map(int))


def _delta_churn(table: DeltaTable, op: LakeOp) -> "tuple[Posture[int], Posture[int], int]":
    # per-commit churn off the table's own log entry, read ONLY for a committing op: `history(1)` on a read or a
    # change feed answers the PREVIOUS write's numbers, so a metadata op would report a commit it never made. A
    # metadata op's churn is ABSENT — it made no commit to count — where the deleted zero triple claimed it made one
    # that moved nothing.
    absent: "tuple[Posture[int], Posture[int], int]" = (Posture(absent=None), Posture(absent=None), 0)
    return (
        _COMMIT_METRIC.try_find(op.tag).map(lambda keys: _churn(table.history(1), keys)).default_value(absent)
        if op.committing
        else absent
    )


def _churn(entries: list[dict[str, Any]], keys: tuple[str, str, str | None]) -> "tuple[Posture[int], Posture[int], int]":
    # a commit entry carries `operationMetrics` as NULL for the operations publishing none — a schema alter is one —
    # so the map defaults empty and every key answers ABSENT rather than raising on a `None` subscript. The two file
    # counters ride their posture out to the receipt, where an absent one names a drifted provider key instead of
    # reporting a commit that moved no files.
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
