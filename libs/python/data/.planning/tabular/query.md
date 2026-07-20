# [PY_DATA_QUERY]

Relational query owner over one `QuerySpec` axis materializing to uniform Arrow. `QueryEngine` discriminates the `QuerySpec` tagged-union — DuckDB SQL gated through the `sqlglot` parse/qualify/optimize plane, the chained DuckDB relational API, the dataframe-agnostic narwhals surface, the Ibis backend-agnostic expression IR with cross-dialect emission, the ADBC/ConnectorX/Flight SQL remote transport over a `RemoteOp` read/stream/ingest/probe/partition sub-axis, the daft out-of-core/distributed runner, and the in-process `datafusion` federated engine — onto one `pyarrow.Table`. Frontend identity IS the spec shape, never a parallel backend `StrEnum` knob and never a `read`/`stream`/`ingest` name-suffix method family, and no serve-plane servicer is authored here: the federation channel is content-keyed and AT-REST.

`datafusion` Substrait interchange is BIDIRECTIONAL — outbound `Serde.serialize_bytes` mints the portable plan bytes `csharp:Rasm.Persistence/Query/federation` retains as the content-keyed at-rest wire, inbound a Persistence-authored plan arrives as BYTES and executes through `Serde.deserialize_bytes` -> `Consumer.from_substrait_plan`, data executing foreign plans and never re-planning them. `QueryReceipt`, the `predicate_count` fold, and its `_PREDICATE_NODES` widening are the lower `columnar#SCAN` owner's, imported and called rather than re-spelled so scan and query count predicates off one application; this owner extends the receipt with the column-level `lineage_edges` projection it populates from `sqlglot.lineage.lineage` over the qualified SQL and `ibis.to_sql` over the bound expression. Awaitable `run` offloads every blocking leg to the `anyio` worker pool — in-process arms ride `_local`, remote and streaming arms delegate the retried-traced-railed leg to `reliability/resilience#RESILIENCE` under the `REMOTE_DB`/`STREAMING` rows. A secret-bearing DSN and the Ray cluster-address are caller-supplied `Remote.dsn`/`Streaming.cluster` case payloads, the outbound credential minted caller-side through `execution/admission#SETTINGS` `SecretBoundary`, never `transport/roots#RESOURCE` `TransportResource` (the `http`/`ssh` artifact-fetch union, not a database-DSN resolver). Durable provenance ledgers stay the C# `Rasm.Persistence` Version/Provenance owner consumed at the wire, never a Python owner.

## [01]-[INDEX]

- [01]-[QUERY]: the relational query engine over one `QuerySpec` axis materializing to uniform Arrow, with the `columnar`-shared receipt extended by column-level lineage.

## [02]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner discriminating by the `QuerySpec` tagged-union axis, the single discriminant. `QuerySpec` cases: `Sql`/`Rel`/`Agnostic`/`Ir` in-process, `Remote` over the ADBC/ConnectorX/Flight SQL `RemoteOp` sub-axis, `Streaming` the daft runner, `Flight` the `csharp:Rasm.Persistence/Query/federation` FLIGHT_RESULT_PLANE ticket consumer (SubstraitPlan command bytes -> `GetFlightInfo` -> `DoGet(FlightTicket)`), and `Federated` the in-process `datafusion` `SessionContext` over `register_object_store`-backed stores and Arrow-capsule-registered frames, carrying EITHER the outbound plan-minting `sql` OR the inbound Persistence-authored Substrait `plan` bytes — the two directions of the one `ARCH`-declared `⇄` seam on one case, the minted-or-received bytes stamped onto the result table's schema metadata so the plan rides the wire and keys the receipt.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; the awaitable `run` folds the `QuerySpec` through `match`/`case` closed by `assert_never`, returning `RuntimeRail[pa.Table]`. In-process `Sql`/`Rel`/`Agnostic`/`Ir` arms ride `_local` — one `async_boundary` offloading the blocking materialization off the event loop through `on_thread` (the `THREAD_BAND`-bounded hop), the broad `Exception` default deliberate because `duckdb.Error`, `ibis.IbisError`, narwhals, and pyarrow are disjoint exception roots with no shared base, so a narrowed catch lets one arm's taxonomy escape the fence. Remote and streaming arms delegate to `guarded(RetryClass.REMOTE_DB)`/`guarded(RetryClass.STREAMING)`, whose `_adbc_transient` hook (ADBC `OperationalError` on `status_code` `TIMEOUT`/`IO`) and `DaftTransientError` tuple retry a genuine transport-transient under runtime backoff — never `RetryClass.RPC`/`WIRE`, whose `_transient` COMPAS module-qualified spellings and `ConnectionError` intra-mesh target catch no ADBC `OperationalError` (which subclasses `DatabaseError`, never `ConnectionError`) or daft Rust fault. Every connection is request-scoped: the DuckDB `Sql`/`Rel` connection rides the shared `columnar#SCAN` `DuckDbSession().connect()` bracket, the `_ir` backend releases through `try`/`finally` `backend.disconnect()` closing the native `backend.con` the Substrait round-trip drives, and the remote drivers ride `with dbapi.connect(...)`.
- Receipt: `receipt_of` folds one total `_provenance` match over the `QuerySpec` axis into the shared `columnar#SCAN` `QueryReceipt.railed` — source, `predicate_count` (the lower owner's exported `parse_one().find_all(*_PREDICATE_NODES)` fold, applied not re-spelled, so scan and query share the application not only the node tuple), and `lineage_edges` (the column-level source→derived edges `sqlglot.lineage.lineage` traces per qualified output column, walked to source-column leaves through `_leaves` so an unqualified projection resolves to its real physical source, extracted on every SQL-carrying arm and `()` on the relational/agnostic arms). Content keys derive off the canonical Arrow bytes through the railed `ContentIdentity.of` — EXCEPT the `Federated` and `Flight` arms, whose identity IS the Substrait plan bytes (the result table's stamped schema metadata, or the `Flight` case's ticket-minting command payload): `ContentIdentity.of("query.plan", wire)` keys the receipt so the `Rasm.Persistence` reuse ledger gains a recompute-dedupe key, the received, minted, and ticket-redeemed bytes of one plan keying identically because retention-never-re-serialization is the channel law. `_provenance` is one total fold yielding `(source, predicate_count, lineage_edges)` together, never two parallel spec walks. Engine-profile evidence rides the shared `QueryReceipt.profile` band across every engine: the `Sql`/`Rel` DuckDB arms and the polars scan arms harvest through the `columnar#SCAN` profiled bracket, the `Federated` datafusion arm folds the wall-latency execution scalars its python `ExecutionPlan` exposes with no per-operator metric accessor, and the `Streaming` daft arm folds those scalars plus its materialized `DataFrame.metrics` per-operator rows through `ProfileHarvest.Datafusion`/`ProfileHarvest.Daft` onto the same band — `receipt_of`'s plan-keyed arms reading it off the table metadata through `EngineProfile.from_table` exactly as the railed arms do — the instrument projection firing at the receipt's own `contribute`, so this page adds no metric state. DBAPI span coverage rides the runtime composition-root wrap seam beside `TRAIN`: the generic `opentelemetry-instrumentation-dbapi` `wrap_connect` patches every PEP-249 connection factory this plane opens — the `columnar#SCAN` `DuckDbSession.connect` `duckdb.connect()` factory, the `_adbc` `dbapi.connect` factory, and the `_flightsql` `flight.connect` factory — so duckdb and ADBC legs emit db-semconv query spans beside the profile band with zero data-side instrumentor import; ConnectorX stays outside the seam because its `read_sql` accelerator exposes no PEP-249 connection object to wrap, so its evidence stays the guarded child span and the receipt alone. The Flight SQL arm additionally carries driver-native spans, its embedded Go tracer stitched into the caller's trace through the `_TRACE_PARENT_KEY` connection option and exported under the deployment's `OTEL_*` env family. `QueryEngine.bench` is the repeatable-benchmark leg — one `QuerySpec` timed across engines under the runtime `Bench.run` subjects, the `BenchmarkReceipt` projecting `domain="bench"` at its own `contribute`, a mutation `Remote` INGEST spec refused at the lane and a process-terminal run riding the runtime `JobRun.bounded` envelope.
- Packages: `duckdb`/`sqlglot` (the parse/qualify/optimize/lineage plane), `narwhals`, `ibis`, `adbc_driver_manager`/`adbc_driver_flightsql` (the DBAPI transport and the `DatabaseOptions`/`StatementOptions`/`ConnectionOptions`-keyed `db_kwargs`/`conn_kwargs` knobs), `connectorx` (the read-parallel accelerator over ADBC's serial pull), `daft` (the out-of-core/distributed runner), `datafusion` (the federation `Serde`/`Consumer` Substrait executor — `duckdb-substrait` owns the DuckDB half, `pyarrow.substrait` DECLINED), `obstore` (`from_url` object-store federation), `anyio` (`anyio.run` drives the awaitable `run` to completion per bench round on a fresh loop off the serving loop), `beartype` (`@beartype(conf=FAULT_CONF)` on `of`/`run`/`bench`), `columnar#SCAN` (the shared `DuckDbSession`/`DuckDbExtension`/`QueryReceipt`/`predicate_count` substrate plus the `EngineProfile`/`ProfileHarvest`/`ProfileMode` profile band the `datafusion`/`daft` arms harvest onto), runtime (`RuntimeRail`/`ContentIdentity`/`async_boundary`/`guarded`/`RetryClass`/`on_thread`, and `observability/profiles#BENCH` `Bench.run`/`BenchMode`/`BenchmarkReceipt` the query bench lane composes — the DBAPI span train `wrap_connect` seam activates at the runtime composition root, never at data altitude).
- Growth: a new query frontend is one `QuerySpec` case; a new SQL dialect target is one `Dialects` member the `SqlGate`/`IrEmit` already thread; a new plan-wire artifact is one `PlanWire` row; a new predicate-bearing node is one `_PREDICATE_NODES` row on the lower `columnar#SCAN` owner the exported `predicate_count` already scans; a new transport operation is one `RemoteOp` row under `assert_never`; a new remote driver is one `RemoteDriver` row on the `_REMOTE` table; a new transport knob is one `Transport` field folded into `db_kwargs`/`conn_kwargs`; a new timeout phase is one `TimeoutPhase` row; a new OAuth key is one `Transport.oauth` entry the `OAUTH_*` projection folds; a new daft runner is one `Runner` row; a new lakehouse source is one `LakehouseFormat` row on the `_DAFT_READ` table with its time-travel key; a new federated store backend is one `(scheme, url, host)` row `register_object_store` federates; a new daft shaping verb is one `StreamingPlan` field; a new agnostic comparator or aggregator is one `Comparator`+`_COMPARE` row or one `Aggregator` member; a new profiling engine is one `ProfileHarvest` case on the lower `columnar#SCAN` owner the arm folds its execution scalars through; a new benchmarked frontend inherits the lane free and tunes its default with one `_BENCH_MODE` row; a new wrapped connection factory is one composition-root `wrap_connect` row on the runtime seam, named here and threaded there; a relational verb composes on the existing chain; zero new surface.
- Boundary: no durable query rail and no global connection; no SQL-string templating or regex rewriting where the `sqlglot` AST owns structure; no hand-rolled Substrait protobuf codec where the extensions own each half; no per-setting builder type where the `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` enum value keys the option; the ADBC partition fan-out rides `Cursor.adbc_execute_partitions`/`adbc_read_partition`/`partition_sql`, never a hand-stitched gRPC loop or low-level `AdbcStatement` dance; a free-string dialect bypassing `Dialect.get_or_raise`, a `find_tables`×`exp.Column` cartesian where `sqlglot.lineage.lineage` owns column provenance, and a `register_globals`-leaking `daft.sql` over unbound globals are foreclosed; `of`/`run`/`bench` carry the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `interop`/`egress`/`columnar` admission entrypoints share. A bench lane re-executing a mutation `Remote` INGEST spec, a data-side `opentelemetry-instrumentation-dbapi` import where the runtime composition-root `wrap_connect` seam owns the connection-factory patch, a parallel per-engine profile field where `ProfileHarvest` folds every engine onto one `EngineProfile` band, and a data-side metric owner where the `BenchmarkReceipt`/`QueryReceipt` `contribute` projections own every measure are the deleted forms.

```python signature
import operator
from collections.abc import Callable, Mapping
from enum import StrEnum
from time import perf_counter
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import anyio
import duckdb
import ibis
import narwhals as nw
import pyarrow as pa
import sqlglot
from adbc_driver_manager import dbapi
from beartype import beartype
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map
from ibis.expr.types import Table as IbisTable
from msgspec import Struct, field
from narwhals import Expr as NwExpr
from obstore.store import from_url
from opentelemetry import propagate
from sqlglot import Dialects, ErrorLevel, exp
from sqlglot.lineage import Node as LineageNode
from sqlglot.lineage import lineage as sqlglot_lineage
from sqlglot.optimizer import optimize as sqlglot_optimize
from sqlglot.optimizer.qualify import qualify as sqlglot_qualify

from rasm.data.tabular.columnar import DuckDbExtension, DuckDbSession, EngineProfile, ProfileHarvest, ProfileMode, QueryReceipt, predicate_count
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.lanes import on_thread
from rasm.runtime.profiles import Bench, BenchMode, BenchmarkReceipt
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from adbc_driver_flightsql import ConnectionOptions, DatabaseOptions, StatementOptions


# --- [TYPES] ----------------------------------------------------------------------------

type Schema = Mapping[str, Mapping[str, str]]
type LineageEdge = tuple[str, str]
type Frames = Mapping[str, Any]
type Predicate = tuple[str, "Comparator", Any]
type AggExpr = tuple[str, "Aggregator"]


class Comparator(StrEnum):
    GT = "gt"
    GE = "ge"
    LT = "lt"
    LE = "le"
    EQ = "eq"
    NE = "ne"
    IN = "in_"


class Aggregator(StrEnum):
    SUM = "sum"
    MEAN = "mean"
    MIN = "min"
    MAX = "max"
    COUNT = "count"
    NUNIQUE = "n_unique"
    MEDIAN = "median"
    STD = "std"


class RemoteDriver(StrEnum):
    ADBC = "adbc"
    CONNECTORX = "connectorx"
    FLIGHTSQL = "flightsql"


class RemoteOp(StrEnum):
    READ = "read"
    STREAM = "stream"
    INGEST = "ingest"
    PROBE = "probe"
    PARTITION = "partition"


class TimeoutPhase(StrEnum):
    FETCH = "fetch"
    QUERY = "query"
    UPDATE = "update"


class OAuthFlow(StrEnum):
    CLIENT_CREDENTIALS = "client_credentials"
    TOKEN_EXCHANGE = "token_exchange"


class Runner(StrEnum):
    NATIVE = "native"
    RAY = "ray"


# daft read-scan FRONTEND axis keying `_DAFT_READ` — orthogonal to the `lakehouse#LAKEHOUSE` `TableFormat`
# transactional-WRITE axis. Both share DELTA/ICEBERG/LANCE, but this read set adds PARQUET/HUDI/SQL with no write
# owner, so a merge would pollute the write axis with read-only members carrying no commit arm — NOT collapsed.
class LakehouseFormat(StrEnum):
    PARQUET = "parquet"
    ICEBERG = "iceberg"
    DELTA = "delta"
    HUDI = "hudi"
    LANCE = "lance"
    SQL = "sql"


# --- [MODELS] ---------------------------------------------------------------------------


class PlanWire(StrEnum):
    SQL = "sql"
    SUBSTRAIT = "substrait"
    SUBSTRAIT_JSON = "substrait_json"


class SqlGate(Struct, frozen=True):
    read: Dialects = Dialects.DUCKDB
    write: Dialects = Dialects.DUCKDB
    schema: Schema | None = None
    optimize: bool = True
    errors: ErrorLevel = ErrorLevel.RAISE

    def _qualified(self, text: str, dialect: Dialects) -> exp.Expression:
        tree = sqlglot.parse_one(text, dialect=sqlglot.Dialect.get_or_raise(dialect), error_level=self.errors)
        return sqlglot_qualify(
            tree, schema=self.schema, dialect=dialect, infer_schema=self.schema is None, validate_qualify_columns=self.schema is not None
        )

    def transpile(self, text: str) -> str:
        qualified = self._qualified(text, self.read)
        gated = sqlglot_optimize(qualified, schema=self.schema, dialect=self.read) if self.optimize and self.schema is not None else qualified
        return gated.sql(dialect=sqlglot.Dialect.get_or_raise(self.write))

    def edges(self, text: str) -> tuple[LineageEdge, ...]:
        # `text` is raw `read`-dialect SQL, so both qualify and lineage parse under `read`; tracing under `write` would mis-parse the source.
        outputs = tuple(sel.alias_or_name for sel in self._qualified(text, self.read).selects)
        roots = (sqlglot_lineage(name, text, schema=self.schema, dialect=self.read) for name in outputs)
        return tuple((str(leaf.name), str(node.name)) for node in roots for leaf in _leaves(node))


class IrEmit(Struct, frozen=True):
    backend_uri: str | None = None
    dialect: Dialects = Dialects.DUCKDB
    wire: PlanWire = PlanWire.SQL
    streaming: bool = False
    parse: str | None = None
    catalog: Mapping[str, Mapping[str, str]] | None = None

    def bound(self, expr: IbisTable | None) -> IbisTable:
        return ibis.parse_sql(self.parse, catalog=self.catalog, dialect=self.dialect.value) if self.parse is not None else expr

    def sql(self, expr: IbisTable) -> str:
        return ibis.to_sql(self.bound(expr), dialect=self.dialect.value)

    def edges(self, expr: IbisTable) -> tuple[LineageEdge, ...]:
        return SqlGate(read=self.dialect, write=self.dialect).edges(self.sql(expr))


class Transport(Struct, frozen=True):
    partition_on: str | None = None
    partition_num: int = 4
    protocol: str | None = None
    queue_size: int | None = None
    authorization: str | None = None
    authority: str | None = None
    tls_root_certs: str | None = None
    tls_override_hostname: str | None = None
    mtls_cert_chain: str | None = None
    mtls_private_key: str | None = None
    tls_skip_verify: bool = False
    block: bool = False
    max_msg_size: int | None = None
    cookie_middleware: bool = False
    timeouts: Mapping[TimeoutPhase, float] = field(default_factory=dict)
    rpc_headers: Mapping[str, str] = field(default_factory=dict)
    oauth_flow: OAuthFlow | None = None
    oauth: Mapping[str, str] = field(default_factory=dict)
    session_options: Mapping[str, str] = field(default_factory=dict)
    substrait_version: str | None = None

    def db_kwargs(self, opts: "type[DatabaseOptions]") -> dict[str, str]:
        timeout_keys = {TimeoutPhase.FETCH: opts.TIMEOUT_FETCH, TimeoutPhase.QUERY: opts.TIMEOUT_QUERY, TimeoutPhase.UPDATE: opts.TIMEOUT_UPDATE}
        rows = (
            (opts.AUTHORIZATION_HEADER, self.authorization),
            (opts.AUTHORITY, self.authority),
            (opts.TLS_ROOT_CERTS, self.tls_root_certs),
            (opts.TLS_OVERRIDE_HOSTNAME, self.tls_override_hostname),
            (opts.MTLS_CERT_CHAIN, self.mtls_cert_chain),
            (opts.MTLS_PRIVATE_KEY, self.mtls_private_key),
            (opts.TLS_SKIP_VERIFY, "true" if self.tls_skip_verify else None),
            (opts.WITH_BLOCK, "true" if self.block else None),
            (opts.WITH_COOKIE_MIDDLEWARE, "true" if self.cookie_middleware else None),
            (opts.WITH_MAX_MSG_SIZE, str(self.max_msg_size) if self.max_msg_size is not None else None),
            (opts.OAUTH_FLOW, self.oauth_flow.value if self.oauth_flow is not None else None),
        )
        headers = {f"{opts.RPC_CALL_HEADER_PREFIX.value}{name}": value for name, value in self.rpc_headers.items()}
        timeouts = {timeout_keys[phase].value: str(seconds) for phase, seconds in self.timeouts.items()}
        oauth = {getattr(opts, f"OAUTH_{key.upper()}").value: value for key, value in self.oauth.items()}
        return {key.value: value for key, value in rows if value is not None} | timeouts | headers | oauth

    def conn_kwargs(self, opts: "type[StatementOptions]", conn_opts: "type[ConnectionOptions]") -> dict[str, str]:
        queue = {opts.QUEUE_SIZE.value: str(self.queue_size)} if self.queue_size is not None else {}
        substrait = {opts.SUBSTRAIT_VERSION.value: self.substrait_version} if self.substrait_version is not None else {}
        session = {f"{conn_opts.OPTION_SESSION_OPTION_PREFIX.value}{key}": value for key, value in self.session_options.items()}
        return queue | substrait | session


class StreamingPlan(Struct, frozen=True):
    fmt: LakehouseFormat
    source: str
    sql: str | None = None
    conn: str | None = None
    project: tuple[str, ...] = ()
    with_columns: Mapping[str, str] = field(default_factory=dict)
    predicate: str | None = None
    group_by: tuple[str, ...] = ()
    agg: tuple[str, ...] = ()
    explode: tuple[str, ...] = ()
    sort: tuple[str, ...] = ()
    sort_desc: bool = False
    distinct: tuple[str, ...] | None = None
    sample: float | None = None
    limit: int | None = None
    partition_col: str | None = None
    num_partitions: int | None = None
    partition_strategy: str = "min-max"
    disable_pushdowns: bool = False
    version: str | int | None = None
    asof: str | None = None
    block_size: int | None = None
    repartition: int | None = None
    repartition_by: tuple[str, ...] = ()
    io_config: Any = None


@tagged_union(frozen=True)
class QuerySpec:
    tag: Literal["sql", "rel", "agnostic", "ir", "remote", "streaming", "federated", "flight"] = tag()
    sql: tuple[str, SqlGate | None] = case()
    rel: tuple[str | None, tuple[str, ...], tuple[str, ...]] = case()
    agnostic: tuple[tuple[str, ...], tuple[Predicate, ...], tuple[str, ...], tuple[AggExpr, ...]] = case()
    ir: tuple[IbisTable, IrEmit] = case()
    remote: tuple[str, str, RemoteDriver, RemoteOp, Transport] = case()
    streaming: tuple[StreamingPlan, Runner, str | None] = case()
    federated: tuple[str | None, bytes | None, tuple[tuple[str, str, str | None], ...]] = case()
    # csharp:Rasm.Persistence/Query/federation FLIGHT_RESULT_PLANE consumer: SubstraitPlan command bytes submit
    # through GetFlightInfo, the returned ReplayKey FlightTicket redeems through DoGet — the producer plans and
    # holds, this side only executes the ticket round-trip; `federated` stays the in-process datafusion executor.
    flight: tuple[bytes, str] = case()

    @staticmethod
    def Sql(text: str, gate: SqlGate | None = None) -> "QuerySpec":
        return QuerySpec(sql=(text, gate))

    @staticmethod
    def Rel(filter_expr: str | None, project: tuple[str, ...], group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(rel=(filter_expr, project, group_by))

    @staticmethod
    def Agnostic(
        select: tuple[str, ...] = (), predicates: tuple[Predicate, ...] = (), group_by: tuple[str, ...] = (), aggs: tuple[AggExpr, ...] = ()
    ) -> "QuerySpec":
        return QuerySpec(agnostic=(select, predicates, group_by, aggs))

    @staticmethod
    def Ir(expr: IbisTable, emit: IrEmit = IrEmit()) -> "QuerySpec":
        return QuerySpec(ir=(expr, emit))

    @staticmethod
    def Remote(
        sql: str, dsn: str, driver: RemoteDriver = RemoteDriver.ADBC, op: RemoteOp = RemoteOp.READ, transport: Transport = Transport()
    ) -> "QuerySpec":
        return QuerySpec(remote=(sql, dsn, driver, op, transport))

    @staticmethod
    def Streaming(plan: StreamingPlan, runner: Runner = Runner.NATIVE, cluster: str | None = None) -> "QuerySpec":
        return QuerySpec(streaming=(plan, runner, cluster))

    @staticmethod
    def Flight(plan: bytes, dsn: str) -> "QuerySpec":
        return QuerySpec(flight=(plan, dsn))

    @staticmethod
    def Federated(
        sql: str | None = None, plan: bytes | None = None, stores: tuple[tuple[str, str, str | None], ...] = ()
    ) -> "RuntimeRail[QuerySpec]":
        # exactly one of `sql` (outbound plan-minting) or `plan` (inbound Persistence-authored bytes) drives the
        # run — the ONE XOR enforcement point, so `_federated`/`_provenance` read a settled direction.
        if (sql is None) == (plan is None):
            return Error(BoundaryFault(config=("query.federated", "sql-xor-plan")))
        return Ok(QuerySpec(federated=(sql, plan, stores)))


# --- [SERVICES] -------------------------------------------------------------------------


class QueryEngine(Struct, frozen=True):
    inputs: Frames
    profiling: ProfileMode = ProfileMode.OFF

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, inputs: Frames, *, profiling: ProfileMode = ProfileMode.OFF) -> "QueryEngine":
        return cls(inputs=inputs, profiling=profiling)

    @beartype(conf=FAULT_CONF)
    def bench(
        self, spec: QuerySpec, *, mode: BenchMode | None = None, rounds: int = 32, warmup: int = 4
    ) -> "RuntimeRail[BenchmarkReceipt]":
        # the query bench lane: one repeated `QuerySpec` timed across engines under the runtime `Bench.run` subjects, the
        # `BenchmarkReceipt` projecting through the standing `domain="bench"` rows at its OWN `contribute` — this page adds
        # zero instrument state. Mutation specs never ride the lane: a `Remote` INGEST re-executes an unknowable partial
        # append, so it is a typed refusal, never a benchmarked round. `Bench.run` is sync and process-terminal — it rides
        # the runtime `JobRun.bounded` envelope, so each round drives the awaitable `run` to completion on a fresh loop
        # through `anyio.run` off the serving loop; throughput times scan-bound specs, latency point queries.
        if spec.tag == "remote" and spec.remote[3] is RemoteOp.INGEST:
            return Error(BoundaryFault(config=("query.bench", "mutation-spec-excluded")))
        selected = mode or _BENCH_MODE.get(spec.tag) or BenchMode.LATENCY
        return Ok(Bench.run(f"query.{spec.tag}", lambda: anyio.run(self.run, spec), mode=selected, rounds=rounds, warmup=warmup))

    @beartype(conf=FAULT_CONF)
    async def run(self, spec: QuerySpec) -> "RuntimeRail[pa.Table]":  # noqa: PLR0911
        match spec:
            case QuerySpec(tag="sql", sql=(text, gate)):
                return await self._local("sql", lambda: self._duckdb(lambda con: con.sql(gate.transpile(text) if gate else text)))
            case QuerySpec(tag="rel", rel=(flt, project, group_by)):
                return await self._local("rel", lambda: self._duckdb(lambda con: self._relation(con, flt, project, group_by)))
            case QuerySpec(tag="agnostic", agnostic=(select, predicates, group_by, aggs)):
                return await self._local("agnostic", lambda: self._agnostic(select, predicates, group_by, aggs))
            case QuerySpec(tag="ir", ir=(expr, emit)):
                return await self._local("ir", lambda: self._ir(expr, emit))
            case QuerySpec(tag="remote", remote=(sql, dsn, driver, RemoteOp.INGEST as op, transport)):
                # ingest mutates the remote table — a transport timeout leaves an unknowable partial append, so the arm crosses
                # unretried and the caller re-issues under its own dedupe, never a blind REMOTE_DB replay duplicating rows; the
                # hop never abandons its band slot either, so a deadline-tripped mutation stays observed to completion.
                return await async_boundary("query.ingest", lambda: on_thread(_REMOTE[driver], sql, dsn, op, transport, self.inputs))
            case QuerySpec(tag="remote", remote=(sql, dsn, driver, op, transport)):
                divert = transport.partition_on is not None and driver is RemoteDriver.ADBC and op in _CX_OPS
                selected = RemoteDriver.CONNECTORX if divert else driver
                return await guarded(
                    RetryClass.REMOTE_DB, on_thread, _REMOTE[selected], sql, dsn, op, transport, self.inputs, abandon=True, subject="query.remote"
                )
            case QuerySpec(tag="streaming", streaming=(plan, runner, cluster)):
                return await guarded(
                    RetryClass.STREAMING, on_thread, _stream, plan, runner, cluster, self.inputs, self.profiling, abandon=True, subject="query.streaming"
                )
            case QuerySpec(tag="federated", federated=(sql, plan, stores)):
                return await self._local("federated", lambda: _federated(sql, plan, stores, self.inputs, self.profiling))
            case QuerySpec(tag="flight", flight=(plan, dsn)):
                return await guarded(RetryClass.REMOTE_DB, on_thread, _flight_ticket, plan, dsn, abandon=True, subject="query.flight")
            case unreachable:
                assert_never(unreachable)

    async def _local(self, tag: str, run: Callable[[], pa.Table]) -> "RuntimeRail[pa.Table]":
        return await async_boundary(f"query.{tag}", lambda: on_thread(run))

    def _duckdb(self, build: Callable[[duckdb.DuckDBPyConnection], duckdb.DuckDBPyRelation]) -> pa.Table:
        # `columnar#SCAN` profiled bracket, released once `to_arrow_table` has materialized the relation inside it; an armed
        # `ProfileMode` harvests the DuckDB JSON tree AFTER materialization and stamps it onto the table, so the `Sql`/`Rel`
        # query arms carry the identical profile band the columnar DuckDB scan arms do — parity holds through the query plane,
        # never the scan alone. `receipt_of`'s railed arm reads the band back off the table exactly as every scan receipt does.
        with DuckDbSession(profiling=self.profiling).profiled() as (con, harvest):
            for name, frame in self.inputs.items():
                con.register(name, frame)
            table = build(con).to_arrow_table()
            profile = harvest()
            return table if profile is None else profile.stamp(table)

    def _relation(
        self, con: duckdb.DuckDBPyConnection, flt: str | None, project: tuple[str, ...], group_by: tuple[str, ...]
    ) -> duckdb.DuckDBPyRelation:
        rel = con.from_arrow(next(iter(self.inputs.values())))
        rel = rel.filter(flt) if flt else rel
        return rel.aggregate(", ".join(project), ", ".join(group_by)) if group_by else rel.project(", ".join(project))

    def _agnostic(self, select: tuple[str, ...], predicates: tuple[Predicate, ...], group_by: tuple[str, ...], aggs: tuple[AggExpr, ...]) -> pa.Table:
        lf = nw.from_native(next(iter(self.inputs.values()))).lazy()
        lf = lf.filter(*(_predicate(p) for p in predicates)) if predicates else lf
        shaped = lf.group_by(*group_by).agg(*(_aggregation(a) for a in aggs)) if group_by else lf.select(*select)
        return shaped.collect().to_arrow()

    def _ir(self, expr: IbisTable, emit: IrEmit) -> pa.Table:
        # `disconnect()` closes the native `backend.con` the Substrait round-trip drives on every exit, so a remote `backend_uri` never leaks.
        backend = ibis.connect(emit.backend_uri) if emit.backend_uri else ibis.duckdb.connect()
        try:
            bound = emit.bound(expr)
            return (
                _ir_plan(backend, bound, emit)
                if emit.wire is not PlanWire.SQL
                else (backend.to_pyarrow_batches(bound).read_all() if emit.streaming else backend.to_pyarrow(bound))
            )
        finally:
            backend.disconnect()


# --- [OPERATIONS] -----------------------------------------------------------------------

_QUEUE_SIZE_KEY: Final[str] = "adbc.rpc.result_queue_size"

# Flight SQL Go driver's embedded OTel tracer joins the caller's trace through this connection option — a key the Python
# option enums do not spell, verified against the shipped libadbc_driver_flightsql; exporter selection rides the standard
# OTEL_* env family the deployment sets, never a Python-side knob.
_TRACE_PARENT_KEY: Final[str] = "adbc.telemetry.trace_parent"


# the bench-mode default per `QuerySpec` tag — scan-bound frontends (streaming/remote/federated/flight) count rounds over
# the window, point frontends (sql/rel/agnostic/ir) time each round; a caller override on `bench(mode=)` wins over the row.
_BENCH_MODE: Final[Map[str, BenchMode]] = Map.of_seq([
    ("sql", BenchMode.LATENCY),
    ("rel", BenchMode.LATENCY),
    ("agnostic", BenchMode.LATENCY),
    ("ir", BenchMode.LATENCY),
    ("remote", BenchMode.THROUGHPUT),
    ("streaming", BenchMode.THROUGHPUT),
    ("federated", BenchMode.THROUGHPUT),
    ("flight", BenchMode.THROUGHPUT),
])


_COMPARE: Final[Map[Comparator, Callable[[NwExpr, Any], NwExpr]]] = Map.of_seq([
    (Comparator.GT, operator.gt),
    (Comparator.GE, operator.ge),
    (Comparator.LT, operator.lt),
    (Comparator.LE, operator.le),
    (Comparator.EQ, operator.eq),
    (Comparator.NE, operator.ne),
    (Comparator.IN, lambda col, value: col.is_in(value)),
])


def _predicate(pred: Predicate) -> NwExpr:
    column, comparator, value = pred
    return _COMPARE[comparator](nw.col(column), value)


def _aggregation(spec: AggExpr) -> NwExpr:
    column, aggregator = spec
    return getattr(nw.col(column), aggregator.value)().alias(column)


def _leaves(node: LineageNode) -> tuple[LineageNode, ...]:
    return (node,) if not node.downstream else tuple(leaf for child in node.downstream for leaf in _leaves(child))


def _partitions(conn: dbapi.Connection, sql: str, transport: Transport) -> pa.Table:
    stmt_kwargs = {_QUEUE_SIZE_KEY: str(transport.queue_size)} if transport.queue_size is not None else None
    with conn.cursor(adbc_stmt_kwargs=stmt_kwargs) as cur:
        descriptors, schema = cur.adbc_execute_partitions(sql)
        readers = (cur.adbc_read_partition(token).read_all() for token in descriptors)
        return pa.concat_tables(readers) if descriptors else schema.empty_table()


def _ir_plan(backend: Any, expr: IbisTable, emit: IrEmit) -> pa.Table:
    # ibis backends own the connection; the extension load names WHAT it needs through the
    # `columnar#SCAN` `DuckDbExtension.SUBSTRAIT` row (community repository a row property), never HOW.
    con = backend.con
    DuckDbExtension.SUBSTRAIT.load(con)
    select = ibis.to_sql(expr, dialect=Dialects.DUCKDB.value)
    if emit.wire is PlanWire.SUBSTRAIT_JSON:
        plan = con.get_substrait_json(select).fetchone()[0]
        return con.from_substrait_json(plan).to_arrow_table()
    plan = con.get_substrait(select).fetchone()[0]
    return con.from_substrait(plan).to_arrow_table()


def _adbc_dispatch(conn: dbapi.Connection, sql: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    with conn.cursor() as cur:
        match op:
            case RemoteOp.READ:
                cur.execute(sql)
                return cur.fetch_arrow_table()
            case RemoteOp.STREAM:
                cur.execute(sql)
                return cur.fetch_record_batch().read_all()
            case RemoteOp.INGEST:
                frame = next(iter(frames.values()))
                cur.adbc_ingest(sql, frame, mode="create_append")
                return frame if isinstance(frame, pa.Table) else pa.table(frame)
            case RemoteOp.PROBE:
                schema = conn.adbc_get_table_schema(sql)
                return pa.table({"field": schema.names, "type": [str(t) for t in schema.types]})
            case RemoteOp.PARTITION:
                return _partitions(conn, sql, transport)
            case unreachable:
                assert_never(unreachable)


def _adbc(sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    with dbapi.connect(uri=dsn) as conn:
        return _adbc_dispatch(conn, sql, op, transport, frames)


def _connectorx(sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    import connectorx as cx  # noqa: PLC0415

    if op is RemoteOp.PROBE:
        return pa.Table.from_pandas(cx.get_meta(dsn, sql, protocol=transport.protocol))
    queries = (
        cx.partition_sql(dsn, sql, transport.partition_on, transport.partition_num) if op is RemoteOp.PARTITION and transport.partition_on else sql
    )
    result = cx.read_sql(
        dsn,
        queries,
        return_type="arrow_stream" if op is RemoteOp.STREAM else "arrow",
        protocol=transport.protocol,
        partition_on=transport.partition_on if isinstance(queries, str) else None,
        partition_num=transport.partition_num if isinstance(queries, str) and transport.partition_on else None,
    )
    return result.read_all() if op is RemoteOp.STREAM else result


def _flight_ticket(plan: bytes, dsn: str) -> pa.Table:
    """FederationFlight ticket round-trip: the plan bytes ride a command FlightDescriptor into GetFlightInfo and
    the returned ticket (the producer's ReplayKey) redeems through DoGet as zero-copy record batches."""
    import pyarrow.flight as flight  # noqa: PLC0415

    client = flight.FlightClient(dsn)
    info = client.get_flight_info(flight.FlightDescriptor.for_command(plan))
    return client.do_get(info.endpoints[0].ticket).read_all()


def _flightsql(sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    from adbc_driver_flightsql import ConnectionOptions, DatabaseOptions, StatementOptions  # noqa: PLC0415
    from adbc_driver_flightsql import dbapi as flight  # noqa: PLC0415

    # driver-native trace stitching: the global W3C propagator writes `traceparent` into the carrier only under a
    # recording span, so an unsampled or span-less call adds no option and the driver traces under its own root.
    carrier: dict[str, str] = {}
    propagate.inject(carrier)
    stitched = {_TRACE_PARENT_KEY: parent} if (parent := carrier.get("traceparent")) else {}
    conn_kwargs = {**(transport.conn_kwargs(StatementOptions, ConnectionOptions) or {}), **stitched}
    with flight.connect(dsn, db_kwargs=transport.db_kwargs(DatabaseOptions) or None, conn_kwargs=conn_kwargs or None) as conn:
        return _adbc_dispatch(conn, sql, op, transport, frames)


_REMOTE: Final[Map[RemoteDriver, Callable[[str, str, RemoteOp, Transport, Frames], pa.Table]]] = Map.of_seq([
    (RemoteDriver.ADBC, _adbc),
    (RemoteDriver.CONNECTORX, _connectorx),
    (RemoteDriver.FLIGHTSQL, _flightsql),
])


# INGEST stays ADBC (ConnectorX is read-only), PROBE on ADBC's native `adbc_get_table_schema`; ConnectorX accelerates the rest.
_CX_OPS: Final[frozenset[RemoteOp]] = frozenset({RemoteOp.READ, RemoteOp.STREAM, RemoteOp.PARTITION})


_DAFT_READ: Final[Map[LakehouseFormat, tuple[str, str | None]]] = Map.of_seq([
    (LakehouseFormat.PARQUET, ("read_parquet", None)),
    (LakehouseFormat.ICEBERG, ("read_iceberg", "snapshot_id")),
    (LakehouseFormat.DELTA, ("read_deltalake", "version")),
    (LakehouseFormat.HUDI, ("read_hudi", None)),
    (LakehouseFormat.LANCE, ("read_lance", "version")),
    (LakehouseFormat.SQL, ("read_sql", None)),
])


def _daft_scan(daft: Any, plan: StreamingPlan) -> Any:
    name, version_key = _DAFT_READ[plan.fmt]
    reader = getattr(daft, name)
    if plan.fmt is LakehouseFormat.SQL:
        return reader(
            plan.sql,
            plan.conn,
            partition_col=plan.partition_col,
            num_partitions=plan.num_partitions,
            partition_bound_strategy=plan.partition_strategy,
            disable_pushdowns_to_sql=plan.disable_pushdowns,
        )
    travel = {version_key: plan.version} if version_key and plan.version is not None else {}
    travel |= {"asof": plan.asof} if plan.fmt is LakehouseFormat.LANCE and plan.asof else {}
    travel |= {"block_size": plan.block_size} if plan.fmt is LakehouseFormat.LANCE and plan.block_size is not None else {}
    return reader(plan.source, io_config=plan.io_config, **travel)


# daft `stats`-map durations arrive unit-tagged; the row folds the value to seconds off the unit, never a fixed divisor.
_DURATION_UNIT: Final[Map[str, float]] = Map.of_seq([("ns", 1e-9), ("us", 1e-6), ("µs", 1e-6), ("ms", 1e-3), ("s", 1.0)])


def _daft_seconds(duration: Mapping[str, Any] | None) -> float:
    return 0.0 if not duration else float(duration["value"]) * (_DURATION_UNIT.get(str(duration.get("unit") or "s")) or 1.0)


def _daft_operators(metrics: Any) -> tuple[tuple[str, float, int], ...]:
    # daft's materialized `DataFrame.metrics` RecordBatch carries one row per physical operator whose `stats` map holds a
    # `duration` {value, unit} struct and a `rows.out` cardinality struct, folded to the shared `(name, seconds, rows)` band.
    return tuple(
        (str(row["name"]), _daft_seconds(stats.get("duration")), int((stats.get("rows.out") or {"value": 0.0})["value"]))
        for row in metrics.to_arrow_table().to_pylist()
        if (stats := dict(row["stats"] or ())) is not None
    )


def _stream(plan: StreamingPlan, runner: Runner, cluster: str | None, frames: Frames, profiling: ProfileMode = ProfileMode.OFF) -> pa.Table:
    import daft  # noqa: PLC0415

    daft.set_runner_ray(address=cluster) if runner is Runner.RAY else daft.set_runner_native()
    scan = _daft_scan(daft, plan)
    bound = (
        daft.sql(plan.sql, register_globals=False, **{name: daft.from_arrow(frame) for name, frame in frames.items()} | {"this": scan})
        if plan.sql and plan.fmt is not LakehouseFormat.SQL
        else scan
    )
    shaped = bound.where(plan.predicate) if plan.predicate else bound
    shaped = shaped.with_columns({name: daft.sql_expr(expr) for name, expr in plan.with_columns.items()}) if plan.with_columns else shaped
    shaped = shaped.groupby(*plan.group_by).agg(*(daft.sql_expr(expr) for expr in plan.agg)) if plan.group_by else shaped
    shaped = shaped.explode(*plan.explode) if plan.explode else shaped
    shaped = shaped.distinct(*plan.distinct) if plan.distinct is not None else shaped
    shaped = shaped.sample(plan.sample) if plan.sample is not None else shaped
    shaped = shaped.sort(list(plan.sort), desc=plan.sort_desc) if plan.sort else shaped
    shaped = shaped.select(*plan.project) if plan.project else shaped
    shaped = shaped.limit(plan.limit) if plan.limit is not None else shaped
    shaped = shaped.repartition(plan.repartition, *plan.repartition_by) if plan.repartition else shaped
    if profiling is ProfileMode.OFF:
        return shaped.to_arrow()
    # daft exposes structured per-operator execution statistics off `DataFrame.metrics` once materialized — a RecordBatch
    # of (id, name, type, category, duration, stats) rows the `_daft_operators` fold lands on the shared operator band;
    # `into_partitions` is a Ray-only repartition and a documented no-op on the native runner, so `collect` materializes,
    # wall latency brackets the collect, and grain reads the real `num_partitions()` (`None` on the native single partition).
    started = perf_counter()
    materialized = shaped.collect()
    latency_s = perf_counter() - started
    table = materialized.to_arrow()
    metrics = shaped.metrics
    operators = _daft_operators(metrics) if metrics is not None else ()
    return EngineProfile.of(ProfileHarvest.Daft(latency_s, table.num_rows, table.nbytes, shaped.num_partitions() or 1, operators)).stamp(table)


_PLAN_META: Final[bytes] = b"substrait.plan"


def _federated(
    sql: str | None, plan: bytes | None, stores: tuple[tuple[str, str, str | None], ...], frames: Frames, profiling: ProfileMode = ProfileMode.OFF
) -> pa.Table:
    from datafusion import SessionContext  # noqa: PLC0415
    from datafusion import substrait as dfs  # noqa: PLC0415

    # one in-process datafusion session per run: `(scheme, url, host)` rows federate remote object stores
    # through obstore `from_url`, every input registering by name through `from_arrow(name=)`. The wire bytes
    # stamp the result table's schema metadata, so the plan RIDES the table to the Persistence consumer.
    ctx = SessionContext()
    for scheme, url, host in stores:
        ctx.register_object_store(scheme, from_url(url), host)
    for name, frame in frames.items():
        ctx.from_arrow(frame, name=name)
    if plan is not None:
        bound = ctx.create_dataframe_from_logical_plan(dfs.Consumer.from_substrait_plan(ctx, dfs.Serde.deserialize_bytes(plan)))
        wire = plan
    elif sql is not None:
        bound = ctx.sql(sql)
        wire = dfs.Serde.serialize_bytes(sql, ctx)
    else:  # unreachable: the `Federated` XOR gate is the single enforcement point — this terminal only narrows `sql`
        raise AssertionError("federated: sql-xor-plan breached past the Federated gate")
    # `execute_stream` drains as a RecordBatchStream unwrapped through `RecordBatch.to_pyarrow()` — incremental, never one giant collect.
    started = perf_counter()
    batches = [batch.to_pyarrow() for batch in bound.execute_stream()]
    latency_s = perf_counter() - started
    table = (pa.Table.from_batches(batches) if batches else pa.table({})).replace_schema_metadata({_PLAN_META: wire})
    if profiling is ProfileMode.OFF:
        return table
    # datafusion's python `ExecutionPlan` exposes only `display`/`display_indent`/`partition_count` — no structured
    # per-operator metric accessor — so the adapter harvests wall latency around the drain plus result cardinality and
    # byte size through `ProfileHarvest.Datafusion`, merged onto the plan-bearing metadata band by `EngineProfile.stamp`.
    return EngineProfile.of(ProfileHarvest.Datafusion(latency_s, table.num_rows, table.nbytes)).stamp(table)


# --- [RECEIPTS] -------------------------------------------------------------------------

type Provenance = tuple[str, int, tuple[LineageEdge, ...]]


# imported `columnar#SCAN` fold — applied here, never re-spelling the byte-identical `find_all(*_PREDICATE_NODES)`.
def _provenance(spec: QuerySpec) -> Provenance:
    match spec:
        case QuerySpec(tag="sql", sql=(text, gate)):
            return text, predicate_count(text), (gate or SqlGate()).edges(text)
        case QuerySpec(tag="ir", ir=(expr, emit)):
            text = emit.sql(expr)
            return text, predicate_count(text), SqlGate(read=emit.dialect, write=emit.dialect).edges(text)
        case QuerySpec(tag="rel", rel=(flt, _, _)):
            return spec.tag, int(flt is not None), ()
        case QuerySpec(tag="agnostic", agnostic=(_, predicates, _, _)):
            return spec.tag, len(predicates), ()
        case QuerySpec(tag="remote", remote=(sql, dsn, _, _, _)):
            return dsn, predicate_count(sql), SqlGate().edges(sql)
        case QuerySpec(tag="streaming", streaming=(plan, _, _)):
            return (
                plan.source,
                predicate_count(plan.sql) if plan.sql else int(plan.predicate is not None),
                SqlGate().edges(plan.sql) if plan.sql else (),
            )
        case QuerySpec(tag="federated", federated=(sql, _, _)):
            # outbound carries lineage/predicates off the minting SQL; the inbound plan-bytes leg carries none (foreign provenance).
            return ("substrait-plan" if sql is None else sql, predicate_count(sql) if sql else 0, SqlGate().edges(sql) if sql else ())
        case QuerySpec(tag="flight", flight=(_, dsn)):
            # ticket redemption executes a producer-held plan — provenance is the endpoint alone, no local SQL to count or trace.
            return dsn, 0, ()
        case unreachable:
            assert_never(unreachable)


def receipt_of(spec: QuerySpec, table: pa.Table) -> "RuntimeRail[QueryReceipt]":
    # FEDERATED and FLIGHT arms key by the PLAN BYTES — the stamped schema metadata or the ticket-minting command — so the
    # in-process execution and the ticket redemption of one Persistence plan share one reuse-ledger dedupe key; every other
    # arm keys by the canonical Arrow result bytes through the railed `ContentIdentity.of`.
    source, predicates, edges = _provenance(spec)
    wire = (table.schema.metadata or {}).get(_PLAN_META, b"") if spec.tag == "federated" else spec.flight[0] if spec.tag == "flight" else None
    if wire is not None:
        # the plan-keyed arms carry the profile band too — the federated arm's datafusion harvest rides the same table
        # metadata the plan wire rides, so a plan-keyed receipt is profile-bearing exactly as a railed one.
        harvested = EngineProfile.from_table(table)
        return ContentIdentity.of("query.plan", wire).map(
            lambda key: QueryReceipt.of(spec.tag, source, table, key, predicate_count=predicates, lineage_edges=edges, profile=harvested)
        )
    return QueryReceipt.railed(spec.tag, source, table, predicate_count=predicates, lineage_edges=edges)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
