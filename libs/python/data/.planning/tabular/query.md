# [PY_DATA_QUERY]

The relational query owner over one `QuerySpec` axis materializing to uniform Arrow. `QueryEngine` discriminates the `QuerySpec` tagged-union axis — DuckDB SQL gated through the `sqlglot` parse/qualify/optimize plane, the chained DuckDB relational API, the dataframe-agnostic narwhals surface, the Ibis backend-agnostic expression IR with cross-dialect emission, the ADBC/ConnectorX/Flight SQL remote transport over a `RemoteOp` read/stream/ingest/probe/partition sub-axis carrying one `Transport` option owner, and the daft out-of-core/distributed runner — onto one `pyarrow.Table` result; the frontend IS the spec shape, never a parallel backend `StrEnum` knob and never a `read`/`stream`/`ingest` name-suffix family. Remote acquisition routes through the runtime `TransportResource`-resolved DSN, the daft Ray runner address through the same seam, the `PARTITION` op fans out through the `execute_partitions` handle and the `partition_sql` planner, and the `Transport` model folds the partition, wire-protocol, TLS/mTLS, three-phase `TimeoutPhase`-keyed timeout, header, message-size, cookie, and queue knobs into one `db_kwargs`/`conn_kwargs` projection keyed by the confirmed `DatabaseOptions`/`StatementOptions` enum values. The result fold is spec-agnostic, so the egress, content-key, and `QueryReceipt` — the `columnar` scan-receipt owner extended with the optional column-level `lineage_edges` projection this owner populates from `sqlglot.find_tables`×`exp.Column` over the qualified SQL and `ibis.to_sql` over the bound expression — are shared with the `columnar` scan owner, never a second receipt rail.

## [01]-[INDEX]

- [01]-[QUERY]: the relational query engine over one `QuerySpec` axis materializing to uniform Arrow, with the `Dialects`-gated `sqlglot`/`ibis` SQL plane, the `RemoteOp` transport sub-axis, the daft elasticity runner, and the column-level lineage receipt projection folded onto the shared `columnar` receipt.

## [02]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner over `duckdb`/`narwhals`/`ibis`/`sqlglot`/ADBC/`daft` discriminating by the `QuerySpec` tagged-union axis (the single discriminant; the frontend IS the spec shape). `QuerySpec` cases `Sql` (relational SQL gated through the `sqlglot` plane) · `Rel` (the chained relational API) · `Agnostic` (the dataframe-agnostic expression surface) · `Ir` (the Ibis backend-agnostic expression IR with cross-dialect emission/ingest) · `Remote` (ADBC/ConnectorX/Flight SQL transport over the `RemoteOp` read/stream/ingest/probe/partition sub-axis) · `Streaming` (the daft out-of-core/distributed runner). A new query frontend is one `QuerySpec` case; a new SQL sub-mode is one `SqlGate`/`IrEmit` field on the existing case; a new transport operation is one `RemoteOp` row; a new remote driver is one `RemoteDriver` row on the driver table; a new transport knob is one `Transport` field folded into the option-keyed `db_kwargs`/`conn_kwargs` projection; a new timeout phase is one `TimeoutPhase` row; a new runner is one `Runner` row — never a `sql_query`/`rel_query`/`nw_query`/`ibis_query`/`stream_query` method family.
- Cases: `QuerySpec.Sql(text, gate)` runs `con.sql(text)` over the `QueryEngine._bound()` connection that `register`s every input frame, binding the result with `to_arrow_table()` (zero-copy Arrow), the optional `SqlGate` gating `text` through `sqlglot.parse_one(dialect=Dialect.get_or_raise(read), error_level=errors)`/`qualify`/`optimizer.optimize`/`Expr.sql(dialect=get_or_raise(write))` under the `ErrorLevel`-keyed parser policy before DuckDB sees it and folding the parse evidence into the receipt · `Rel(filter, project, group_by)` chains `from_arrow(...).filter(...).project(...).aggregate(...)` returning a `DuckDBPyRelation`, terminal `to_arrow_table()` · `Agnostic(select, filter, group_by)` admits any native frame via `narwhals.from_native(...).lazy()`, composes `filter(*col-predicates)`/`group_by`/`agg`/`select` against `narwhals.col`/`narwhals.Expr` (a grouped select folds each selected column through a `sum` aggregation expression, never a bare column reference `group_by` rejects), and lands Arrow through `narwhals.DataFrame.to_arrow` · `Ir(expr, emit)` binds one `ibis` expression — either the supplied `IbisTable` or, when `IrEmit.parse` carries a SQL string, the `ibis.parse_sql(parse, catalog, dialect)` round-trip that admits raw SQL back into the backend-agnostic expression tree — and the `PlanWire` row folds egress: `SQL` materializes through `BaseBackend.to_pyarrow(expr)` (or `to_pyarrow_batches(expr).read_all()` under `IrEmit.streaming` for the incremental record-batch path) against `ibis.connect(backend_uri)` (DuckDB the default when `backend_uri` is `None`) and emits the cross-dialect portable SQL plan through `ibis.to_sql(expr, dialect)` for the wire, while `SUBSTRAIT`/`SUBSTRAIT_JSON` round-trip the compiled `SELECT` through the admitted `duckdb-substrait` extension (`con.install_extension("substrait", repository="community")`/`load_extension`/`get_substrait`/`get_substrait_json` emitting the portable binary/JSON Substrait protobuf plan, `from_substrait`/`from_substrait_json` executing it back to Arrow) — the portable relational-algebra artifact the C# `Rasm.Persistence` federation seam decodes, denser than a dialect SQL string; the same lazy `sqlglot`-compiled expression compiles to DuckDB/DataFusion/Polars/remote SQL with no rewrite · `Remote(sql, dsn, driver, op, transport)` opens the remote connection over the `TransportResource`-resolved `dsn` string (arriving pre-resolved through the runtime `roots` seam) and dispatches the `RemoteOp` sub-axis (`READ` whole table, `STREAM` record-batch reader, `INGEST` Arrow write-back, `PROBE` schema introspection, `PARTITION` distributed fan-out — `_adbc_dispatch` answers all five uniformly for the ADBC and Flight SQL drivers, the `PARTITION` arm driving the low-level `AdbcStatement.execute_partitions` handle and reading each descriptor as Arrow record batches under the `StatementOptions.QUEUE_SIZE` read-ahead, ConnectorX answering `PROBE` through `get_meta`, `PARTITION` through the `partition_sql` explicit planner fed back as the `read_sql` per-partition query list, and the read ops through `read_sql`) across the `RemoteDriver` table — ADBC the cp315-core default, ConnectorX the `<3.15`-gated parallel-partitioned fast-path auto-selected when the `Transport.partition_on` column is supplied (the `partition_num`/`protocol` wire-tuning travelling on the same row), Flight SQL the Arrow-native driver whose `Transport.db_kwargs`/`conn_kwargs` fold the confirmed `DatabaseOptions` transport axis (`AUTHORIZATION_HEADER`/`AUTHORITY`/`TLS_ROOT_CERTS`/`TLS_OVERRIDE_HOSTNAME`/`MTLS_CERT_CHAIN`/`MTLS_PRIVATE_KEY`/`TLS_SKIP_VERIFY`/`WITH_BLOCK`/`WITH_COOKIE_MIDDLEWARE`/`WITH_MAX_MSG_SIZE`/the three `TIMEOUT_*` phases keyed by `TimeoutPhase`/`RPC_CALL_HEADER_PREFIX`/the OAuth axis — `OAUTH_FLOW` keyed by the `OAuthFlow` row plus the `OAUTH_*` client-credentials and RFC 8693 token-exchange keys folded from the `Transport.oauth` mapping), the `StatementOptions.QUEUE_SIZE` read-ahead and `SUBSTRAIT_VERSION`, and the `ConnectionOptions.OPTION_SESSION_OPTION_PREFIX` session-option keys into one option-keyed dict — all terminating in the uniform `pyarrow.Table` (or a `RecordBatchReader` collapsed to a table on the `STREAM` op) · `Streaming(plan, runner)` builds a lazy `daft.DataFrame` from `daft.read_*` (the `_DAFT_READ` row keyed by `LakehouseFormat` carrying the per-format time-travel key — `snapshot_id` for Iceberg, `version` for Delta/Lance, `asof`/`block_size` the Lance scan axes — threading `io_config` for object-store credentials, the SQL row threading `read_sql(partition_col=, num_partitions=, partition_bound_strategy=, disable_pushdowns_to_sql=)` native partitioning), binds every registered input through `daft.from_arrow` into `daft.sql(plan.sql, register_globals=False, **frames)` when `plan.sql` reshapes a lakehouse scan, and pushes the `plan.predicate`/`with_columns`/`group_by`×`agg`/`explode`/`distinct`/`sample`/`sort`/`project`/`limit`/`repartition` shaping (each `daft.sql_expr`-compiled where the verb takes an expression) into the lazy plan before triggering `DataFrame.to_arrow` under the `Runner` row (`NATIVE` single-node out-of-core, `RAY` distributed against the `TransportResource`-resolved cluster-address string `QueryEngine.run` threads), the polars streaming engine's out-of-core successor.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; `QueryEngine.run` folds the `QuerySpec` through `match`/`case` closed by `assert_never` (the totality proof — a new case forces a new arm) and returns a `RuntimeRail[pa.Table]`; the DuckDB connection is request-scoped, never a module global; the Ibis, remote, and daft connections acquire and release per run.
- Auto: Arrow result binding is uniform — every case terminates in a `pyarrow.Table` via `DuckDBPyRelation.to_arrow_table`, `narwhals.DataFrame.to_arrow`, `ibis.BaseBackend.to_pyarrow`, `Cursor.fetch_arrow_table`/`Cursor.fetch_record_batch().read_all()`/`Cursor.adbc_ingest`/`Connection.adbc_get_table_schema`, `connectorx.read_sql(return_type="arrow")`/`connectorx.get_meta`, or `daft.DataFrame.to_arrow`, so the egress, content-key, and `QueryReceipt` fold are spec-agnostic; the narwhals path keeps the query lazy (`narwhals.LazyFrame`) until `collect()`; the Ibis path stays an unbound expression until `to_pyarrow`/`to_pyarrow_batches`, so predicate pushdown crosses the backend, `ibis.to_sql(expr, dialect)` emits the portable SQL plan without connecting, and the `PlanWire.SUBSTRAIT`/`SUBSTRAIT_JSON` rows lift the compiled plan to a portable Substrait protobuf the C# federation seam decodes; the daft path stays a lazy logical plan until the `to_arrow` sink, so partition-parallel pushdown owns the cost; Ibis (analytical-intent portability across 20+ backends), narwhals (input-type agnosticism), and daft (where-and-how-big-code-runs scale) are distinct axes all carried on the owner. The `RemoteOp` sub-axis is a closed-enum value the `_adbc_dispatch` fold matches once over `assert_never` for the two ADBC-handle drivers, never a `read`/`stream`/`ingest`/`probe`/`partition` method family; the `RemoteDriver` row carries its connect-and-dispatch closure so the three drivers collapse to one table lookup over the shared `(sql, dsn, op, transport, frames)` signature, never three `if driver ==` arms; ConnectorX auto-selecting when `Transport.partition_on` is supplied (the `_dispatch` arm rewriting an ADBC request to ConnectorX) is its sole reason to exist over ADBC's serial one-shot, the `PARTITION` op is the distributed fan-out the `_partitions` fold drives off the `execute_partitions` handle on the ADBC/Flight SQL path and the `partition_sql` planner on the ConnectorX path, and the `Transport` model is the one option owner folding partition, wire-protocol, TLS/mTLS, the three-phase `TimeoutPhase`-keyed timeout, header, message-size, cookie, queue, OAuth-flow, session-option, and Substrait-version knobs into `db_kwargs`/`conn_kwargs` keyed by the confirmed `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` enum values rather than scattered `inputs[...]` reads. `ibis`, `sqlglot`, and `adbc-driver-manager` are cp315-clean and import module-top, while `connectorx`, `adbc-driver-flightsql`, and `daft` ride their gated arms importing the dist function-local under `# noqa: PLC0415` (ConnectorX on the `python_version<'3.15'` band, Flight SQL and daft as heavyweight engines), never a module-top import on this cp315-core page.
- Receipt: `receipt_of(spec, table)` folds one total `_provenance` match over the `QuerySpec` axis into the shared `columnar#SCAN` `QueryReceipt.of` — source, predicate count (the `_PREDICATE_NODES` `exp.Where`/`exp.Having`/`exp.Qualify`/`exp.Join` node count over the parsed SQL through one variadic `find_all`, or the filter presence on the relational/agnostic/no-SQL streaming arms), and `lineage_edges` (the column-level source→derived edges from `SqlGate.edges`/`IrEmit.edges`, extracted on every SQL-carrying arm — the `Sql` text, the `Ir` `ibis.to_sql` emission, the `Remote` transport SQL, and the `Streaming` `plan.sql` reshape — and `()` only on the relational/agnostic arms that carry no SQL string) — keyed by `ContentIdentity` inside `QueryReceipt.of` and contributed through runtime `ReceiptContributor`; the durable provenance ledger stays the C# `Rasm.Persistence` Version/Provenance owner consumed at the wire, never a Python versioning/provenance/ledger owner. No new receipt rail, and `_provenance` is one total fold over the spec yielding `(source, predicate_count, lineage_edges)` together, never two parallel spec walks.
- Packages: `duckdb` (`connect`/`register`/`sql`/`from_arrow`/`DuckDBPyRelation.filter`/`project`/`aggregate`/`to_arrow_table`/`install_extension(repository=)`/`load_extension`), `duckdb-substrait` (the connection-bound `get_substrait`/`get_substrait_json`/`from_substrait`/`from_substrait_json` round-trip over the loaded `substrait` community extension), `sqlglot` (`parse_one(dialect=, error_level=)`/`Expr.sql(dialect=)`/`optimizer.optimize`/`optimizer.qualify.qualify(infer_schema=, validate_qualify_columns=)`/`exp.Column`/`exp.Where`/`exp.Having`/`exp.Qualify`/`exp.Join`/`find_all(*types)`/`find_tables`/`Dialect.get_or_raise`/`Dialects`/`ErrorLevel`), `narwhals` (`from_native`/`DataFrame.lazy`/`col`/`Expr`/`LazyFrame.collect`/`DataFrame.to_arrow`), `ibis-framework` (`connect`/`duckdb.connect`/`parse_sql(catalog=, dialect=)`/`BaseBackend.to_pyarrow`/`to_pyarrow_batches`/`to_sql`, the backend-agnostic expression IR), `adbc-driver-manager` (`dbapi.connect`/`Cursor.execute`/`fetch_arrow_table`/`fetch_record_batch`/`adbc_ingest`/`Connection.adbc_get_table_schema`/`AdbcStatement.set_sql_query`/`set_options`/`execute_partitions`), `adbc-driver-flightsql` (`dbapi.connect(db_kwargs=, conn_kwargs=)`/`DatabaseOptions.{AUTHORIZATION_HEADER,AUTHORITY,TLS_ROOT_CERTS,TLS_OVERRIDE_HOSTNAME,MTLS_CERT_CHAIN,MTLS_PRIVATE_KEY,TLS_SKIP_VERIFY,WITH_BLOCK,WITH_COOKIE_MIDDLEWARE,WITH_MAX_MSG_SIZE,TIMEOUT_FETCH,TIMEOUT_QUERY,TIMEOUT_UPDATE,RPC_CALL_HEADER_PREFIX,OAUTH_FLOW,OAUTH_*}`/`OAuthFlowType.{CLIENT_CREDENTIALS,TOKEN_EXCHANGE}`/`ConnectionOptions.OPTION_SESSION_OPTION_PREFIX`/`StatementOptions.{QUEUE_SIZE,SUBSTRAIT_VERSION}`), `connectorx` (`read_sql(return_type="arrow"|"arrow_stream", protocol=, partition_on=, partition_num=)`/`partition_sql(conn, query, partition_on, partition_num)`/`get_meta(conn, query, protocol=)`, the `<3.15` gated partitioned arm), `daft` (`read_parquet`/`read_iceberg(snapshot_id=)`/`read_deltalake(version=)`/`read_hudi`/`read_lance(version=, asof=, block_size=)`/`read_sql(partition_col=, num_partitions=, partition_bound_strategy=, disable_pushdowns_to_sql=)`/`from_arrow`/`sql(register_globals=)`/`sql_expr`/`DataFrame.where`/`with_columns`/`groupby`/`agg`/`explode`/`distinct`/`sample`/`sort(desc=)`/`select`/`limit`/`repartition(num, *by)`/`set_runner_native`/`set_runner_ray`/`DataFrame.to_arrow`), `pyarrow` (`Table`/`Table.from_pandas`/`Table.from_batches`/`RecordBatchReader`/`concat_tables`/`table`), runtime (`RuntimeRail`/`boundary`/`ContentIdentity`/`ReceiptContributor`, all reached through the `columnar` `QueryReceipt`), `tabular/columnar` (`QueryReceipt`).
- Growth: a new query frontend is one `QuerySpec` case; a new SQL dialect target is one `Dialects` member the `SqlGate`/`IrEmit` already thread; a new plan-wire artifact is one `PlanWire` row the `_ir_plan` fold already matches; a new predicate-bearing node is one `_PREDICATE_NODES` row the variadic `find_all` already scans; a new transport operation is one `RemoteOp` row the `_adbc_dispatch` fold already matches under `assert_never`; a new remote driver is one `RemoteDriver` row on the `_REMOTE` table binding its connect closure; a new transport knob is one `Transport` field folded into `db_kwargs`/`conn_kwargs`; a new timeout phase is one `TimeoutPhase` row keyed to its `TIMEOUT_*` member; a new OAuth key is one `Transport.oauth` entry the `OAUTH_*` projection already folds; a new session option is one `Transport.session_options` entry; a new daft runner is one `Runner` row binding a `TransportResource`-resolved address; a new lakehouse source is one `LakehouseFormat` row on the `_DAFT_READ` table carrying its time-travel key; a new daft shaping verb is one `StreamingPlan` field threaded into the `_stream` fold; a new relational verb composes on the existing chain; zero new surface.
- Boundary: no durable query rail, no global connection, no SQL-string templating engine, no regex SQL rewriting where the `sqlglot` AST owns structure, no hand-rolled Substrait protobuf codec where the `duckdb-substrait` extension owns encode/decode, no parallel plan model per encoding where one `PlanWire` row keys binary-versus-JSON, no parallel `StrEnum` backend discriminant duplicating the `QuerySpec` tag, no second Ray runner owner, no second `QueryReceipt` declaration shadowing the `columnar` owner, no per-setting builder type where the `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` enum value keys the option; remote-connection acquisition, OAuth credential identity, and the Ray cluster address route through the runtime `TransportResource`, never a second transport owner or a Python-minted credential; a per-frontend query class family, a `read`/`stream`/`ingest`/`probe`/`partition` remote method family, three `if driver ==` arms, three parallel `timeout_fetch`/`timeout_query`/`timeout_update` fields where one `TimeoutPhase`-keyed row folds them, parallel `oauth_*` fields where one `Transport.oauth` mapping folds them, a hand-stitched gRPC partition loop where `execute_partitions`/`partition_sql` own the fan-out, a `register_globals`-leaking `daft.sql` over unbound globals, scattered `inputs[...]` option reads where the `Transport` model owns the knobs, a backend-specific SQL string inside an Ibis pipeline, a free-string dialect bypassing `Dialect.get_or_raise`, a daft Ray runner as a second owner, and a generic dataframe wrapper are the deleted forms.

```python signature
from collections.abc import Callable, Mapping
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import duckdb
import ibis
import narwhals as nw
import pyarrow as pa
import sqlglot
from adbc_driver_manager import dbapi
from expression import case, tag, tagged_union
from ibis.expr.types import Table as IbisTable
from msgspec import Struct
from sqlglot import Dialects, ErrorLevel, exp
from sqlglot.optimizer import optimize as sqlglot_optimize
from sqlglot.optimizer.qualify import qualify as sqlglot_qualify

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from adbc_driver_flightsql import ConnectionOptions, DatabaseOptions, StatementOptions


# --- [TYPES] ----------------------------------------------------------------------------

type Schema = Mapping[str, Mapping[str, str]]
type LineageEdge = tuple[str, str]
type Frames = Mapping[str, Any]


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
        return sqlglot_qualify(tree, schema=self.schema, dialect=dialect, infer_schema=self.schema is None, validate_qualify_columns=self.schema is not None)

    def transpile(self, text: str) -> str:
        qualified = self._qualified(text, self.read)
        gated = sqlglot_optimize(qualified, schema=self.schema, dialect=self.read) if self.optimize and self.schema is not None else qualified
        return gated.sql(dialect=sqlglot.Dialect.get_or_raise(self.write))

    def edges(self, text: str) -> tuple[LineageEdge, ...]:
        tree = self._qualified(text, self.write)
        sources = tuple(table.alias_or_name for table in sqlglot.find_tables(tree))
        return tuple(
            (source, str(col.alias_or_name))
            for col in tree.find_all(exp.Column)
            for source in (sources if col.table == "" else (str(col.table),))
        )


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
    timeouts: Mapping[TimeoutPhase, float] = {}
    rpc_headers: Mapping[str, str] = {}
    oauth_flow: OAuthFlow | None = None
    oauth: Mapping[str, str] = {}
    session_options: Mapping[str, str] = {}
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
    with_columns: Mapping[str, str] = {}
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
    tag: Literal["sql", "rel", "agnostic", "ir", "remote", "streaming"] = tag()
    sql: tuple[str, SqlGate | None] = case()
    rel: tuple[str | None, tuple[str, ...], tuple[str, ...]] = case()
    agnostic: tuple[tuple[str, ...], str | None, tuple[str, ...]] = case()
    ir: tuple[IbisTable, IrEmit] = case()
    remote: tuple[str, str, RemoteDriver, RemoteOp, Transport] = case()
    streaming: tuple[StreamingPlan, Runner] = case()

    @staticmethod
    def Sql(text: str, gate: SqlGate | None = None) -> "QuerySpec":
        return QuerySpec(sql=(text, gate))

    @staticmethod
    def Rel(filter_expr: str | None, project: tuple[str, ...], group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(rel=(filter_expr, project, group_by))

    @staticmethod
    def Agnostic(select: tuple[str, ...], filter_expr: str | None = None, group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(agnostic=(select, filter_expr, group_by))

    @staticmethod
    def Ir(expr: IbisTable, emit: IrEmit = IrEmit()) -> "QuerySpec":
        return QuerySpec(ir=(expr, emit))

    @staticmethod
    def Remote(
        sql: str, dsn: str, driver: RemoteDriver = RemoteDriver.ADBC,
        op: RemoteOp = RemoteOp.READ, transport: Transport = Transport(),
    ) -> "QuerySpec":
        return QuerySpec(remote=(sql, dsn, driver, op, transport))

    @staticmethod
    def Streaming(plan: StreamingPlan, runner: Runner = Runner.NATIVE) -> "QuerySpec":
        return QuerySpec(streaming=(plan, runner))


# --- [SERVICES] -------------------------------------------------------------------------

class QueryEngine(Struct, frozen=True):
    inputs: dict[str, Any]

    @classmethod
    def of(cls, inputs: dict[str, Any]) -> "QueryEngine":
        return cls(inputs=inputs)

    def run(self, spec: QuerySpec, cluster: str | None = None) -> "RuntimeRail[pa.Table]":
        return boundary(f"query.{spec.tag}", lambda: self._dispatch(spec, cluster))

    def _bound(self) -> duckdb.DuckDBPyConnection:
        con = duckdb.connect()
        for name, frame in self.inputs.items():
            con.register(name, frame)
        return con

    def _dispatch(self, spec: QuerySpec, cluster: str | None) -> pa.Table:  # noqa: PLR0911
        match spec:
            case QuerySpec(tag="sql", sql=(text, gate)):
                return self._bound().sql(gate.transpile(text) if gate else text).to_arrow_table()
            case QuerySpec(tag="rel", rel=(flt, project, group_by)):
                rel = next(iter(self.inputs.values())).pipe(self._bound().from_arrow)
                rel = rel.filter(flt) if flt else rel
                shaped = rel.aggregate(", ".join(project), ", ".join(group_by)) if group_by else rel.project(", ".join(project))
                return shaped.to_arrow_table()
            case QuerySpec(tag="agnostic", agnostic=(select, flt, group_by)):
                lf = nw.from_native(next(iter(self.inputs.values()))).lazy()
                lf = lf.filter(*(nw.col(name) for name in flt.split(","))) if flt else lf
                shaped = lf.group_by(*group_by).agg(*(nw.col(c).sum() for c in select)) if group_by else lf.select(*select)
                return shaped.collect().to_arrow()
            case QuerySpec(tag="ir", ir=(expr, emit)):
                backend = ibis.connect(emit.backend_uri) if emit.backend_uri else ibis.duckdb.connect()
                bound = emit.bound(expr)
                return _ir_plan(backend, bound, emit) if emit.wire is not PlanWire.SQL else (
                    backend.to_pyarrow_batches(bound).read_all() if emit.streaming else backend.to_pyarrow(bound)
                )
            case QuerySpec(tag="remote", remote=(sql, dsn, driver, op, transport)):
                selected = RemoteDriver.CONNECTORX if transport.partition_on and driver is RemoteDriver.ADBC else driver
                return _REMOTE[selected](sql, dsn, op, transport, self.inputs)
            case QuerySpec(tag="streaming", streaming=(plan, runner)):
                return _stream(plan, runner, cluster, self.inputs)
            case unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------

_RPC_QUEUE_SIZE: Final[str] = "adbc.rpc.result_queue_size"


def _partitions(conn: dbapi.Connection, sql: str, transport: Transport) -> pa.Table:
    stmt = conn.adbc_statement
    stmt.set_sql_query(sql)
    if transport.queue_size is not None:
        stmt.set_options(**{_RPC_QUEUE_SIZE: str(transport.queue_size)})
    descriptors, schema = stmt.execute_partitions()
    tables = (conn.adbc_read_partition(token).read_all() for token in descriptors)
    return pa.concat_tables(tables) if descriptors else schema.empty_table()


def _ir_plan(backend: Any, expr: IbisTable, emit: IrEmit) -> pa.Table:
    con = backend.con
    con.install_extension("substrait", repository="community")
    con.load_extension("substrait")
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
    queries = cx.partition_sql(dsn, sql, transport.partition_on, transport.partition_num) if op is RemoteOp.PARTITION and transport.partition_on else sql
    result = cx.read_sql(
        dsn, queries, return_type="arrow_stream" if op is RemoteOp.STREAM else "arrow", protocol=transport.protocol,
        partition_on=transport.partition_on if isinstance(queries, str) else None,
        partition_num=transport.partition_num if isinstance(queries, str) and transport.partition_on else None,
    )
    return result.read_all() if op is RemoteOp.STREAM else result


def _flightsql(sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    from adbc_driver_flightsql import ConnectionOptions, DatabaseOptions, StatementOptions  # noqa: PLC0415
    from adbc_driver_flightsql import dbapi as flight  # noqa: PLC0415

    with flight.connect(
        dsn, db_kwargs=transport.db_kwargs(DatabaseOptions) or None,
        conn_kwargs=transport.conn_kwargs(StatementOptions, ConnectionOptions) or None,
    ) as conn:
        return _adbc_dispatch(conn, sql, op, transport, frames)


_REMOTE: Final[dict[RemoteDriver, Callable[[str, str, RemoteOp, Transport, Frames], pa.Table]]] = {
    RemoteDriver.ADBC: _adbc,
    RemoteDriver.CONNECTORX: _connectorx,
    RemoteDriver.FLIGHTSQL: _flightsql,
}


_DAFT_READ: Final[dict[LakehouseFormat, tuple[str, str | None]]] = {
    LakehouseFormat.PARQUET: ("read_parquet", None),
    LakehouseFormat.ICEBERG: ("read_iceberg", "snapshot_id"),
    LakehouseFormat.DELTA: ("read_deltalake", "version"),
    LakehouseFormat.HUDI: ("read_hudi", None),
    LakehouseFormat.LANCE: ("read_lance", "version"),
    LakehouseFormat.SQL: ("read_sql", None),
}


def _daft_scan(daft: Any, plan: StreamingPlan) -> Any:
    name, version_key = _DAFT_READ[plan.fmt]
    reader = getattr(daft, name)
    if plan.fmt is LakehouseFormat.SQL:
        return reader(
            plan.sql, plan.conn, partition_col=plan.partition_col, num_partitions=plan.num_partitions,
            partition_bound_strategy=plan.partition_strategy, disable_pushdowns_to_sql=plan.disable_pushdowns,
        )
    travel = {version_key: plan.version} if version_key and plan.version is not None else {}
    travel |= {"asof": plan.asof} if plan.fmt is LakehouseFormat.LANCE and plan.asof else {}
    travel |= {"block_size": plan.block_size} if plan.fmt is LakehouseFormat.LANCE and plan.block_size is not None else {}
    return reader(plan.source, io_config=plan.io_config, **travel)


def _stream(plan: StreamingPlan, runner: Runner, cluster: str | None, frames: Frames) -> pa.Table:
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
    return shaped.to_arrow()


# --- [RECEIPTS] -------------------------------------------------------------------------

type Provenance = tuple[str, int, tuple[LineageEdge, ...]]


_PREDICATE_NODES: Final[tuple[type[exp.Expression], ...]] = (exp.Where, exp.Having, exp.Qualify, exp.Join)


def _predicates(text: str) -> int:
    return len(tuple(sqlglot.parse_one(text).find_all(*_PREDICATE_NODES)))


def _provenance(spec: QuerySpec) -> Provenance:
    match spec:
        case QuerySpec(tag="sql", sql=(text, gate)):
            return text, _predicates(text), (gate or SqlGate()).edges(text)
        case QuerySpec(tag="ir", ir=(expr, emit)):
            text = emit.sql(expr)
            return text, _predicates(text), SqlGate(read=emit.dialect, write=emit.dialect).edges(text)
        case QuerySpec(tag="rel", rel=(flt, _, _)) | QuerySpec(tag="agnostic", agnostic=(_, flt, _)):
            return spec.tag, int(flt is not None), ()
        case QuerySpec(tag="remote", remote=(sql, dsn, _, _, _)):
            return dsn, _predicates(sql), SqlGate().edges(sql)
        case QuerySpec(tag="streaming", streaming=(plan, _)):
            return plan.source, _predicates(plan.sql) if plan.sql else int(plan.predicate is not None), SqlGate().edges(plan.sql) if plan.sql else ()
        case unreachable:
            assert_never(unreachable)


def receipt_of(spec: QuerySpec, table: pa.Table) -> QueryReceipt:
    source, predicates, edges = _provenance(spec)
    return QueryReceipt.of(spec.tag, source, table, predicate_count=predicates, lineage_edges=edges)
```

## [03]-[RESEARCH]

- [SQLGLOT_AST_ACCESSORS]: the `sqlglot` `parse_one(dialect=, error_level=)`/`Expr.sql(dialect=)`/`optimizer.optimize(schema=, dialect=)`/`optimizer.qualify.qualify(schema=, dialect=)`/`Dialect.get_or_raise`/`Dialects`/`ErrorLevel`/`exp.Column`/`find_all(*expression_types)`/`find_tables(expression) -> set[Table]` surface the `SqlGate.transpile`/`edges`/`_predicates` path transcribes is catalogue-confirmed against the folder `sqlglot` `.api` — `find_all` confirmed variadic over node types, so the `_PREDICATE_NODES` multi-type predicate scan is one `find_all` call, and `ErrorLevel.{IGNORE,WARN,RAISE,IMMEDIATE}` threaded through `parse_one(error_level=)` is the confirmed parser-error policy row the `SqlGate.errors` field keys; `to_sql(expr, dialect)`/`ibis.duckdb.connect()`/`BaseBackend.to_pyarrow(expr)`/`parse_sql(sqlstring, catalog, dialect)` the `IrEmit`/`Ir` path binds are catalogue-confirmed against the folder `ibis-framework` `.api` — `ibis.duckdb.connect()` the confirmed default-backend accessor, `ibis.to_sql(expr, dialect)` the confirmed module-level emitter, and `ibis.parse_sql(sqlstring, catalog, dialect)` the confirmed SQL→expression ingest row [08] the `IrEmit.bound`/`parse` round-trip drives; the `duckdb-substrait` `con.install_extension("substrait", repository="community")`/`load_extension("substrait")`/`get_substrait`/`get_substrait_json(query) -> DuckDBPyRelation`/`from_substrait(proto)`/`from_substrait_json(json)` round-trip the `PlanWire.SUBSTRAIT`/`SUBSTRAIT_JSON` `_ir_plan` fold binds is catalogue-confirmed against the folder `duckdb-substrait` `.api` (the four connection-bound methods plus the `.fetchone()[0]` plan-artifact read), and the `qualify(infer_schema=, validate_qualify_columns=)` policy the `SqlGate._qualified` fold threads is catalogue-confirmed against the `sqlglot` `.api` row [06]. Two open seams on the Ibis-IR path: `BaseBackend.to_pyarrow_batches(expr) -> RecordBatchReader` the `IrEmit.streaming` arm reads (the `.api` lists `to_pyarrow`/`to_pandas`/`to_polars` but not the batched `to_pyarrow_batches` row) and `BaseBackend.con` (the native `DuckDBPyConnection` accessor the `_ir_plan` substrait round-trip drives off the ibis DuckDB backend, not enumerated on the `.api` execution-interface rows) confirm against the live distribution before the streaming and Substrait arms treat them as settled, leaving `IrEmit.streaming` and `_ir_plan` marked RESEARCH items while `to_pyarrow`/`to_sql`/`parse_sql`/`connect`/`duckdb.connect` and the whole `duckdb-substrait` four-method surface stay confirmed. The one open seam is the per-node AST accessor the `SqlGate.edges` source→target fold reads on the `find_all(exp.Column)`/`find_tables` nodes — `Column.alias_or_name`, `Column.table`, `Table.alias_or_name` — and the `exp.Where`/`exp.Having`/`exp.Qualify`/`exp.Join` node classes the `_PREDICATE_NODES` predicate count names, which are core `sqlglot.expressions` classes on the catalogue-confirmed traversal but not individually listed in the `.api`; the accessor and node-class read confirms against the live distribution before the column-level edge extraction and the widened predicate count treat them as settled, leaving `SqlGate.edges` and `_PREDICATE_NODES` marked RESEARCH items. The `parse_one`/`qualify`/`optimize`/`Dialect.get_or_raise`/`find_tables`/`to_sql` plane is settled, and the removed `sqlglot.lineage.lineage`/`Node.walk` traversal is replaced by the confirmed `find_tables`×`find_all(exp.Column)` cartesian, so no fence member contradicts a sibling RESEARCH item.
- [REMOTE_PARTITION_DEEPEN]: the `adbc-driver-manager` `dbapi.connect(uri=)`/`Cursor.execute`/`fetch_arrow_table`/`fetch_record_batch`/`adbc_ingest(table_name, data, mode)`/`Connection.adbc_get_table_schema(table_name)` surface the `_adbc`/`_adbc_dispatch` `READ`/`STREAM`/`INGEST`/`PROBE` arms transcribe is catalogue-confirmed against the folder `adbc-driver-manager` `.api` (the `PROBE` arm reading `Schema.names`/`Schema.types` off the returned `pyarrow.Schema`); the low-level `AdbcStatement.set_sql_query(query)`/`set_options(**kwargs)`/`execute_partitions() -> (stream, rows_affected)` handle surface the `PARTITION` arm's `_partitions` fold binds is catalogue-confirmed against the same `.api` `_lib` handle layer, and `adbc-driver-flightsql` confirms `execute_partitions`/`adbc_execute_partitions` as the partition capability; the `<3.15`-gated `connectorx` `read_sql(conn, query, return_type=, protocol=, partition_on=, partition_num=)` partitioned/streaming surface (`arrow`/`arrow_stream` egress), the `partition_sql(conn, query, partition_on, partition_num) -> list[str]` explicit planner the `PARTITION` arm calls and feeds back as the `read_sql` `query` list, and the `get_meta(conn, query, protocol=)` schema-probe surface the `_connectorx` `PROBE` arm binds (its `pd.DataFrame` lifted through `pyarrow.Table.from_pandas`) are catalogue-confirmed against the folder `connectorx` `.api`; the `adbc-driver-flightsql` `dbapi.connect(uri, db_kwargs, conn_kwargs)` and the `DatabaseOptions.{AUTHORIZATION_HEADER,AUTHORITY,TLS_ROOT_CERTS,TLS_OVERRIDE_HOSTNAME,MTLS_CERT_CHAIN,MTLS_PRIVATE_KEY,TLS_SKIP_VERIFY,WITH_BLOCK,WITH_COOKIE_MIDDLEWARE,WITH_MAX_MSG_SIZE,TIMEOUT_FETCH,TIMEOUT_QUERY,TIMEOUT_UPDATE,RPC_CALL_HEADER_PREFIX,OAUTH_FLOW,OAUTH_*}`/`OAuthFlowType.{CLIENT_CREDENTIALS,TOKEN_EXCHANGE}`/`ConnectionOptions.OPTION_SESSION_OPTION_PREFIX`/`StatementOptions.{QUEUE_SIZE,SUBSTRAIT_VERSION}` enum-value keys the `Transport.db_kwargs`/`conn_kwargs` projection folds — `RPC_CALL_HEADER_PREFIX` suffixed with each header name, the three `TIMEOUT_*` phases keyed by the `TimeoutPhase` row, the OAuth axis keyed by the `OAuthFlow` row and the `getattr(opts, f"OAUTH_{key.upper()}")` projection over the `Transport.oauth` mapping, `QUEUE_SIZE`/`SUBSTRAIT_VERSION` threaded as `conn_kwargs` statement options, `OPTION_SESSION_OPTION_PREFIX` suffixed with each session-option key, `TLS_SKIP_VERIFY`/`WITH_BLOCK`/`WITH_COOKIE_MIDDLEWARE` emitting the literal `"true"` — are all catalogue-confirmed against the folder `adbc-driver-flightsql` `.api` (the 29 `DatabaseOptions` rows including the 15 `OAUTH_*` keys, the two `OAuthFlowType` values, the `ConnectionOptions`/`StatementOptions` rows). One open seam: the DBAPI `Connection`-to-handle accessors the `_partitions` fold reads — `Connection.adbc_statement` (the `AdbcStatement` handle accessor), the exact `AdbcStatement.execute_partitions() -> (descriptors, schema)` return arity the `.api` summarizes as `(stream, rows_affected)`, `Connection.adbc_read_partition(token) -> RecordBatchReader`, and the `_RPC_QUEUE_SIZE = "adbc.rpc.result_queue_size"` constant the generic-ADBC `_partitions` path passes to `set_options` (the canonical RPC key, equal to the flightsql `StatementOptions.QUEUE_SIZE` value but not enumerated on the `adbc-driver-manager` `StatementOptions` `.api` row that lists only ingest/bind/incremental) — name the handle-access and per-descriptor endpoint-read members that neither `.api` lists as a row, so they confirm against the live distribution before the `PARTITION` fan-out treats them as settled, leaving `_partitions` a marked RESEARCH item while `set_sql_query`/`set_options`/`execute_partitions` stay confirmed against the `_lib` handle table; plus the cp315-core `dsn` derivation from the runtime `TransportResource`, the `Remote` arm receiving a resolved DSN string and the daft `RAY` runner a resolved cluster address through `TransportResource.acquire`, the projection landing on the runtime `roots` seam rather than a second transport owner. The serial `dbapi.connect`/`fetch_arrow_table`/`fetch_record_batch`/`adbc_ingest`/`adbc_get_table_schema`/`get_meta`/`partition_sql` transport surface and the whole `Transport` option-keyed `db_kwargs`/`conn_kwargs` projection (transport, OAuth, session, and Substrait-version axes) are settled.
- [DAFT_ELASTICITY]: the `daft` `read_parquet`/`read_iceberg(snapshot_id=)`/`read_deltalake(version=)`/`read_hudi`/`read_lance(version=, asof=)`/`read_sql(sql, conn, partition_col=, num_partitions=, partition_bound_strategy=)`/`from_arrow`/`sql(sql, register_globals=, **bindings)`/`DataFrame.where`/`distinct`/`sort`/`select`/`limit`/`repartition`/`set_runner_native`/`set_runner_ray(address=)`/`DataFrame.to_arrow() -> pyarrow.Table` surface the `_stream`/`_daft_scan`/`_DAFT_READ` path transcribes is catalogue-confirmed against the folder `daft` `.api` (cp310-abi3, importing on cp315 without source-build per the catalogue install note; `to_arrow` confirmed to land a `pyarrow.Table` directly, so the uniform-Arrow contract treats the daft egress as a settled `pyarrow.Table` with no `arro3`/`nanoarrow` capsule intermediary); the `read_sql(partition_col=, num_partitions=, partition_bound_strategy=)` native partition pushdown replaces the prior ConnectorX-only partition story for the daft engine, the `_DAFT_READ` row's per-format time-travel key (`snapshot_id` for Iceberg, `version` for Delta/Lance, `asof` the confirmed Lance timestamp axis) and the per-reader `io_config` thread the catalogue-confirmed object-store and snapshot arguments, and `daft.sql(plan.sql, register_globals=False, **{name: daft.from_arrow(frame) ...} | {"this": scan})` seals the binding to the registered input frames plus the lakehouse scan so the SQL transform resolves them rather than leaking an unregistered global, with the `plan.predicate`/`with_columns`/`group_by`×`agg`/`explode`/`distinct`/`sample`/`sort`/`project`/`limit`/`repartition` shaping pushed into the lazy plan through `DataFrame.where`/`with_columns`/`groupby`/`agg`/`explode`/`distinct`/`sample`/`sort(desc=)`/`select`/`limit`/`repartition(num, *partition_by)` before the `to_arrow` sink — `distinct=None` skips dedup, `distinct=()` dedups all columns, `distinct=(cols)` dedups on keys, the one polymorphic dedup row, and each expression-taking verb compiling its string through `daft.sql_expr`. The expanded shaping surface (`with_columns`/`groupby`/`agg`/`explode`/`sample`/`sort(desc=)`/`repartition(num, *partition_by)`/`sql_expr`), the `read_lance(block_size=)` scan tuning, and the `read_sql(disable_pushdowns_to_sql=)` pushdown control are all catalogue-confirmed against the folder `daft` `.api` (the `DataFrame` transform table rows [01]-[16] and the expression-factory `sql_expr` row). The one open seam is the cp315-core daft `RAY` runner cluster address derivation from the runtime `TransportResource`, the `Streaming` arm receiving the resolved address through `QueryEngine.run` rather than a second transport owner; the lazy-plan/runner/read/time-travel/`io_config`/`from_arrow`/`sql`-binding/full-shaping/`to_arrow` surface is settled.
