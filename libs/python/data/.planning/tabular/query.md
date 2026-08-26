# [PY_DATA_QUERY]

Relational query owner over one `QuerySpec` axis materializing to uniform Arrow. `QueryEngine` discriminates the `QuerySpec` tagged-union — DuckDB SQL gated through the `sqlglot` parse/qualify/optimize plane, the chained DuckDB relational API, the dataframe-agnostic narwhals surface, the Ibis backend-agnostic expression IR with cross-dialect emission, the ADBC/ConnectorX/Flight SQL remote transport over a `RemoteOp` read/stream/ingest/probe/partition sub-axis, the daft out-of-core/distributed runner, and the in-process `datafusion` federated engine — onto one `pyarrow.Table`. Frontend identity IS the spec shape, never a parallel backend `StrEnum` knob and never a `read`/`stream`/`ingest` name-suffix method family, and no serve-plane servicer is authored here: the federation channel is content-keyed and AT-REST.

`datafusion` Substrait interchange is BIDIRECTIONAL — outbound `Serde.serialize_bytes` mints the portable plan bytes `dotnet:Rasm.Persistence/Query/federation` retains as the content-keyed at-rest wire, inbound a Persistence-authored plan arrives as BYTES, admits through the standalone `substrait` plan gate at `_reach`, and only then executes through `Serde.deserialize_bytes` -> `Consumer.from_substrait_plan`, data executing foreign plans and never re-planning them; direction rides the `Federation` two-case discriminant its payload carries, so no runtime XOR gate, result-returning factory, or unreachable terminal re-derives it. DuckDB's in-process half is a distinct wire: `substrait` attaches SQL TABLE FUNCTIONS alone, so each `PlanWire` half is a `CALL` through `con.execute` off the `_SUBSTRAIT` row — a connection-bound `get_substrait`/`from_substrait` method is not a DuckDB member and never enters. `QueryCensus`, the `predicate_count` fold, and its `_PREDICATE_NODES` widening are the lower `tabular/columnar#SCAN` owner's, imported rather than re-spelled so scan and query count predicates off one application; this owner extends the result with column-level `lineage_edges` off `sqlglot.lineage.lineage` over the qualified SQL and `ibis.to_sql` over the bound expression. Awaitable `run` gates one `_reach`-then-`_bound` prologue over the whole `QuerySpec` axis, then offloads every blocking leg to the `anyio` worker pool through one `_crossed` envelope — in-process arms cross it unretried, remote, streaming, and flight arms delegate the retried-traced-result leg to `runtime/reliability/resilience#RESILIENCE` under the `REMOTE_DB`/`STREAMING` rows, and a producer-partitioned result crosses it once per leg through `_fanned`. Secret-bearing DSNs and the Ray cluster-address ride caller-supplied `Remote.dsn`/`Streaming.cluster` payloads, the outbound credential minted caller-side through `runtime/execution/admission#SETTINGS` `SecretBoundary`, never `runtime/transport/roots#RESOURCE` `TransportResource`; provenance keeps the redacted scheme-and-host coordinate alone, so no credential reaches a durable result. Durable provenance ledgers stay the C# `Rasm.Persistence` Version/Provenance owner consumed at the wire, never a Python owner.

## [01]-[INDEX]

- [02]-[QUERY]: `QueryEngine` folds one `QuerySpec` axis to uniform Arrow, extending the `columnar`-shared result with column-level lineage.

## [02]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner discriminating by the `QuerySpec` tagged-union axis, the single discriminant. `QuerySpec` cases: `Sql`/`Rel`/`Agnostic`/`Ir` in-process, `Remote` over the ADBC/ConnectorX/Flight SQL `RemoteOp` sub-axis, `Streaming` the daft runner, `Flight` the `dotnet:Rasm.Persistence/Query/federation` FLIGHT_RESULT_PLANE ticket consumer (SubstraitPlan command bytes -> `GetFlightInfo` -> `DoGet(FlightTicket)` over every returned endpoint), and `Federated` the in-process `datafusion` `SessionContext` over `register_object_store`-backed stores and Arrow-capsule-registered frames, carrying one `Federation` direction — `mint` the outbound plan-minting SQL, `execute` the inbound Persistence-authored Substrait bytes — the two directions of the one `ARCH`-declared `⇄` boundary on one case, the minted-or-received bytes stamped onto the result table's schema metadata so the plan rides the wire and keys the result.
- Cases: the driver sub-axis is one `_DRIVER` row per `RemoteDriver`, and `DriverKind` is the closed discriminant every per-driver shape derives from — the manager front end takes the caller's `driver`/`entrypoint` shared-library coordinate, Flight SQL alone takes the typed `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` projection, a bundled native driver takes `db_kwargs`/`conn_kwargs` and resolves its own library, a local driver's `connect` admits the URI alone (a `db_kwargs=` keyword is a `TypeError` there), and the loader kind carries no PEP-249 connection to open, wrap, or instrument. Each row's `ns` is a CALL-TIME thunk: a table row dereferencing a `lazy` module attribute reifies every driver proxy at import and defeats the floor gate the roster reads. Statement options ride one `cursor(adbc_stmt_kwargs=)` open per dispatch, so a timeout, queue size, or Substrait pin reaches every op rather than the partition leg alone.
- Law: `_reach` is the one gate over the whole `QuerySpec` axis, read as `run`'s prologue so `_dispatch` is total over admitted specs and no arm re-probes — reject law is data exactly as the sibling `tabular/lakehouse#LAKEHOUSE` matrix states it. `_UNREACHED` derives at import from one `find_spec` per `_PROVIDER_MODULE` row, and that roster derives its driver half from the `_DRIVER` table and carries one row per remaining `lazy`-bound frontend — so a provider the manifest gates below the interpreter floor answers `QUERY_FLOOR.raised(frontend, module)` naming the absent module rather than raising `ModuleNotFoundError` inside an offloaded thread, on the daft, datafusion, and Flight planes exactly as on the remote drivers. `_REMOTE_REFUSAL` rows each `(driver, op)` cell a present provider cannot serve and `_conditional` each cell the transport payload decides, both answering `QUERY_UNREACHED.raised(frontend, reason)` with a typed `RemoteRefusal` whose member value IS the operator-facing evidence. `QueryEngine` honours a caller's `RemoteDriver` choice verbatim: an implicit ADBC-to-ConnectorX swap on a partitioned read overrode an explicit selection AND routed into a floor-absent distribution, so ADBC partition fan-out rides its own native `adbc_execute_partitions` and ConnectorX serves the specs that name it.
- Law: `_bound` is the second prologue leg, resolving each frame-naming arm's input slot ONCE — a spec naming no frame binds the sole bound input, several bound inputs with no name refuse typed, and a named absent frame refuses — so `_dispatch` reads `self.inputs[name]` totally and no arm picks a frame off iteration order. `QueryEngine` holds one admitted `BackendGeneration`; query specs never carry raw contract bytes or repeated admission knobs.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; the awaitable `run` threads the reach-then-bind prologue then folds the `QuerySpec` through `_dispatch`, whose `_body` `match`/`case` closed by `assert_never` destructures operands alone and whose one tail owns the envelope — `_cell` reading the frontend's whole `_Cell` — its three fault anchors, its retry class, and its provider raise set — so the retry law lives at one site and a new frontend inherits it rather than a neighbour's copied call. Its leg anchor GENERATES off one `_celled` declaration per frontend and per `RemoteOp`, so a sixth transport operation lands its row by being a member of the closed roster; the two fan PHASE anchors ride the `Fan` itself, because exactly two of the twelve cells ever answer one and a pair generated per cell seats twenty coordinates no site can raise. `_body` answers the SHAPE its frontend is: one `Leg` for a whole result, or the `Fan` a producer-partitioned one already carries, so `_crossed` puts one envelope on both and `_fanned` spends the plan hop once and redeems every leg CONCURRENTLY under the bound `on_thread` already takes, concatenating in the producer's own order. Draining a fan leg after leg pays the partitioning round trip and collects nothing for it, which is what left the ADBC descriptor set and the Flight endpoint set slower than the unpartitioned read they were selected over; the retry class rides each leg, so a transient on one partition retries that partition rather than re-planning the whole fan. An absent class is the unretried envelope: one `async_boundary` offloading the blocking materialization off the event loop through `on_thread` (the `THREAD_BAND`-bounded hop), its `catch` the frontend's OWN named union rather than a bare `Exception` — `duckdb.Error`, `ibis.IbisError`, `NarwhalsError`, `adbc_driver_manager.Error`, `DaftCoreException`, `flight.FlightError`, and the Arrow core error are disjoint roots with no shared base, so the cell carries its set as a THUNK and reifies it only where that frontend runs, which is what a module-scope tuple over seven providers could not do behind seven `lazy` binds. The remote, streaming, and flight cells carry `RetryClass.REMOTE_DB`/`RetryClass.STREAMING`, whose `_adbc_transient` hook (ADBC `OperationalError` on `status_code` `TIMEOUT`/`IO`) and `DaftTransientError` tuple retry a genuine transport-transient under runtime backoff — never `RetryClass.RPC`/`WIRE`, whose `_transient` COMPAS module-qualified spellings and `ConnectionError` intra-mesh target catch no ADBC `OperationalError` (which subclasses `DatabaseError`, never `ConnectionError`) or daft Rust fault. Every connection is request-scoped: the DuckDB `Sql`/`Rel` connection rides the shared `tabular/columnar#SCAN` `DuckDbSession().connect()` bracket, the `_ir` backend releases through `try`/`finally` `backend.disconnect()` closing the native `backend.con` the Substrait round-trip drives, the remote drivers ride `with row.ns().connect(...)`, and the Flight client and each per-location peer ride their own `with` bracket.
- Auto: the `tabular/lakehouse#LAKEHOUSE` analytics store is a SOURCE this axis already reaches, never a lane of its own — `Streaming` scans the cold hive tree distributed under its `hive_partitioning` row, `Federated` registers that tree as a datafusion listing table beside the object store holding it, `Sql`/`Rel` read the evidence table interactively off the caller's `session` policy carrying the `delta` extension row, `Agnostic` folds it dataframe-agnostically, and `Flight` beside `RemoteDriver.FLIGHTSQL` serves the remote query end. That same `session` slot carries the `postgres_scanner` `Attach` row, so the `[TENANT_COST_JOIN]` fold against live tenant, grant, and workload tables is ONE statement rather than a second transport. Selection is a spec shape, so a store earns no engine-shaped surface and every engine lands the one Arrow frame the store schema pins.
- Result: `_plan_provenance` censuses the admitted plan for both inbound legs — `_plan_rels` walks the plan's whole message tree and recognises a relation by the `Rel` descriptor rather than by the field holding it, so a subquery-nested join counts exactly as a direct child does and `_PLAN_PREDICATES` counts the classes `_PREDICATE_NODES` counts on the SQL legs, where `sqlglot` already walks into subqueries. Relation roster and resolved extension urns both ride the lineage slot under their own edge classes, so one provenance grain spans the SQL and plan wires and a foreign producer's function vocabulary survives the gate that resolved it.
- Result: `run` folds one total `_provenance` match over the `QuerySpec` axis into the shared `tabular/columnar#SCAN` `QueryCensus.derived` — source, predicate count, and lineage edges — behind one `boundary` fence, because `_provenance` parses caller SQL through `sqlglot` and a malformed or dialect-foreign statement raises past a result the signature already declares. DSN-sourced provenance keeps the redacted `_endpoint` coordinate, never the credential-bearing DSN. Content keys derive off canonical Arrow bytes except where Substrait plan bytes own identity. Every profiled arm stamps the shared band: DuckDB, Polars, and DataFusion fold portable execution scalars; Daft adds admitted native operator rows. DBAPI span coverage rides the runtime composition-root wrap boundary beside `TRAIN`: `dbapi_drivers()` declares each admitted connection factory whose distribution this floor resolves, and the root threads those rows through `Instrumentation.dbapi`. ConnectorX exposes no PEP-249 connection object, so its guarded child span and result remain its evidence. Flight SQL injects W3C parent context through its admitted option row. `QueryEngine.bench` spends the reach-then-bind prologue ONCE and rounds `_dispatch`, so a refused spec answers its typed row instead of being timed N times as a microsecond success and an admitted plan frontend is timed against the query rather than against its own parse-and-infer admission gate; it refuses mutation INGEST and leaves the process-terminal `JobRun.bounded` envelope to its caller.
- Packages: `duckdb`/`sqlglot` (the parse/qualify/optimize/lineage plane), `narwhals`, `ibis`, `adbc_driver_manager`/`adbc_driver_flightsql`/`adbc_driver_postgresql`/`adbc_driver_snowflake`/`adbc_driver_sqlite` (the five admitted DBAPI transports, the Flight SQL row alone binding the `DatabaseOptions`/`StatementOptions`/`ConnectionOptions`-keyed `db_kwargs`/`conn_kwargs`/`adbc_stmt_kwargs` knobs), `connectorx` (the read-parallel accelerator over ADBC's serial pull, its `read_sql`/`partition_sql`/`get_meta` surface bound behind the `_UNREACHED` floor gate while the manifest marker holds it below the interpreter floor), `daft` (the out-of-core/distributed runner), `datafusion` (the federation `Serde`/`Consumer` Substrait executor — the DuckDB `substrait` extension owns the in-process half through its `CALL` table functions, `pyarrow.substrait` DECLINED as consumer-only: it mints no `Plan` at all, since `serialize_expressions` emits an `ExtendedExpression` and `serialize_schema` a `SubstraitSchema`, so the outbound half of this boundary has no producer there; `run_query` resolves each `NamedTable` through a per-call `table_provider` callback rather than a registered catalog and validates nothing ahead of execution, where `_plan_refusal` answers a typed verdict), `substrait` (`proto.Plan`/`ExtensionRegistry`/`infer_plan_schema` — the inbound-plan admission gate the manifest admits this distribution for, and the executor's own deserializer never becomes), `pyarrow.flight` (`FlightClient`/`FlightDescriptor.for_command`/`FlightInfo.endpoints`/`FlightEndpoint.ticket`+`locations`/`FlightCallOptions` — the ticket-redemption plane), `anyio` (`anyio.run` drives the awaitable `run` to completion per bench round on a fresh loop off the serving loop), `beartype` (`@beartype(conf=FAULT_CONF)` on `of`/`run`/`bench`), `tabular/columnar#SCAN` (the shared `DuckDbSession`/`DuckDbExtension`/`QueryCensus`/`predicate_count` substrate with the `EngineProfile`/`ProfileHarvest`/`ProfileMode` profile band the `datafusion`/`daft` arms harvest onto), `tabular/interop#INTEROP` (`DataLeg`, the roster this page anchors its whole `RAISES` table on), `google.protobuf` (`message.DecodeError` alone, the one raise a malformed plan wire answers), runtime (`RuntimeResult`/`ContentIdentity`/`async_boundary`/`boundary`/`guarded`/`RetryClass`/`on_thread`, `runtime/transport/roots#STORE` `store_handle`/`Config`/`ResourceRef` as the branch's one `from_url` fold every `FederatedStore` row binds through, the credential riding `ref.credentials`, and `runtime/observability/profiles#BENCH` `Bench.run`/`BenchMode`/`Benchmark` the query bench lane composes — `DbapiDriver` the declared row shape and `Instrumentation.dbapi` the composition-root wrap `dbapi_drivers()` feeds, never a data-altitude activation).
- Growth: a new DuckDB extension or attached catalog on the in-process arms is one row on the caller's `session` policy, zero owner edits; a new query frontend is one `QuerySpec` case; a new SQL dialect target is one `Dialects` member the `SqlGate`/`IrEmit` already thread; a new plan-wire artifact is one `PlanWire` row with its `(serialize, execute)` `_SUBSTRAIT` pair; a new federation direction is one `Federation` case; a new inbound-plan bound is one `PlanRefusal` row read in `_plan_refusal`; a newly predicate-bearing relation kind is one `_PLAN_PREDICATES` member, the generic `_plan_rels` walk already reaching it wherever the algebra seats it; a repo function vocabulary beyond the standard extensions is one `_REGISTRY.register_extension_yaml` call at composition; a new predicate-bearing node is one `_PREDICATE_NODES` row on the lower `tabular/columnar#SCAN` owner the exported `predicate_count` already scans; a new transport operation is one `RemoteOp` row under `assert_never`; a new remote driver is one `_DRIVER` row naming its module, namespace thunk, `DriverKind`, and `db.system.name`, from which the floor roster, the connect projection, the statement projection, the executor, and the instrumentation boundary all derive; a new driver-connect dialect is one `DriverKind` member with its arm on `_connect_kwargs`/`_stmt_kwargs`/`_RUN`; a newly-unreachable cell is one `_REMOTE_REFUSAL` row or one `_conditional` arm carrying its `RemoteRefusal` reason; a new typed Flight knob is one `Transport` field folded into `db_kwargs`/`conn_kwargs`/`stmt_kwargs`; a driver-native knob no enum here models is one `Transport.db_options`/`conn_options`/`stmt_options` entry; a new timeout phase is one `TimeoutPhase` row; a new OAuth key is one `OAuthKey` row the `oauth` projection folds; a new daft runner is one `Runner` row; a new lakehouse source is one `LakehouseFormat` row on the `_DAFT_READ` table with its time-travel key; a new federated store backend is one `FederatedStore` row `register_object_store` federates, its scheme, host, and credential all deriving from the ref it carries; a new federated relation over those bytes is one `FederatedTable` row `register_parquet` registers; a new daft shaping verb is one `StreamingPlan` field and one `_SHAPING` row, its position in that `Block` the applied order; a new frontend's envelope is one `_FRONTEND_RAISES` row naming its provider raise thunk plus one `_ENVELOPE` row where it retries, its leg anchor and its posture both deriving through `_celled`; a newly fanning frontend is one `plan_at`/`redeem_at` pair on the `Fan` its own `_body` arm builds; a frontend whose producer partitions its result answers a `Fan` off its own `_body` arm and inherits the plan hop, the concurrent redemption, and the ordered concat untouched; a new ConnectorX planning width is the `_PARTITION_FAN` constant; a new agnostic comparator or aggregator is one `Comparator`+`_COMPARE` row or one `Aggregator` member; a new profiling engine is one `ProfileHarvest` case on the lower `tabular/columnar#SCAN` owner the arm folds its execution scalars through; a new benchmarked frontend inherits the lane free and tunes its default with one `_BENCH_MODE` row; a relational verb composes on the existing chain; zero new surface.

```python
import operator
from collections.abc import Callable, Iterator, Mapping
from contextlib import contextmanager
from copy import replace
from enum import StrEnum
from functools import partial
from importlib.util import find_spec
from time import perf_counter
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never
from urllib.parse import urlsplit, urlunsplit

import anyio
import duckdb
import ibis
import narwhals as nw
import pyarrow as pa
import sqlglot
from google.protobuf import message as protobuf_message
from narwhals.exceptions import NarwhalsError
from sqlglot.errors import SqlglotError
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from google.protobuf.message import DecodeError
from ibis.backends import BaseBackend
from ibis.expr.types import Table as IbisTable
from msgspec import Struct, field
from narwhals import Expr as NwExpr
from opentelemetry import propagate
from sqlglot import Dialects, ErrorLevel, exp
from sqlglot.lineage import Node as LineageNode
from sqlglot.lineage import lineage as sqlglot_lineage
from sqlglot.optimizer import optimize as sqlglot_optimize
from sqlglot.optimizer.qualify import qualify as sqlglot_qualify
from substrait import proto as substrait_proto
from substrait.extension_registry import ExtensionRegistry
from substrait.type_inference import infer_plan_schema

lazy import adbc_driver_manager as adbc_manager
lazy import connectorx as cx
lazy import daft
lazy import pyarrow.flight as flight
lazy from adbc_driver_flightsql import ConnectionOptions, DatabaseOptions, StatementOptions
lazy from adbc_driver_flightsql import dbapi as flightsql_dbapi
lazy from adbc_driver_manager import dbapi as adbc_dbapi
lazy from adbc_driver_postgresql import dbapi as postgres_dbapi
lazy from adbc_driver_snowflake import dbapi as snowflake_dbapi
lazy from adbc_driver_sqlite import dbapi as sqlite_dbapi
lazy from datafusion import SessionContext
lazy from datafusion import substrait as dfs

from rasm.data.tabular.columnar import DuckDbExtension, DuckDbSession, EngineProfile, ProfileHarvest, ProfileMode, QueryCensus, predicate_count
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.admission import BackendGeneration
from rasm.runtime.faults import (
    FAULT_CONF,
    TERMINAL,
    TRANSIENT,
    Catch,
    Disposition,
    FaultRow,
    RuntimeResult,
    async_boundary,
    boundary,
    rostered,
    traversed,
)
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import DbapiDriver
from rasm.runtime.profiles import Bench, BenchMode, Benchmark
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import Config, ResourceRef, origin, store_handle

if TYPE_CHECKING:
    from obstore.store import ClientConfig, ObjectStore, RetryConfig

# --- [TYPES] ----------------------------------------------------------------------------

type Schema = Mapping[str, Mapping[str, str]]
type LineageEdge = tuple[str, str]
type Frames = Mapping[str, Any]
type Predicate = tuple[str, "Comparator", Any]
type AggExpr = tuple[str, "Aggregator"]
type ParseSpec = tuple[str, Schema]
type IngestMode = Literal["append", "create", "replace", "create_append"]
type Leg = Callable[[], pa.Table]
type FanPlan = Callable[[], tuple[Block[Leg], pa.Schema]]


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
    POSTGRESQL = "postgresql"
    SNOWFLAKE = "snowflake"
    SQLITE = "sqlite"


class DriverKind(StrEnum):
    MANAGER = "manager"
    FLIGHT = "flight"
    NATIVE = "native"
    LOCAL = "local"
    LOADER = "loader"


class RemoteRefusal(StrEnum):
    CONNECTORX_READ_ONLY = "connectorx reaches no write-back; ingest rides an adbc driver"
    CONNECTORX_PARTITION_COLUMN = "connectorx partition planning needs transport.partition_on"
    SQLITE_NO_PARTITION = "the sqlite driver answers adbc_execute_partitions NOT_IMPLEMENTED"
    POSTGRESQL_NO_PARTITION = "the postgresql driver answers adbc_execute_partitions NOT_IMPLEMENTED"
    SNOWFLAKE_NO_PARTITION = "the snowflake driver answers adbc_execute_partitions NOT_IMPLEMENTED"
    FLIGHT_NATIVE_OAUTH = "the pyarrow flight plane runs no oauth flow; a token mints caller-side onto transport.authorization"
    STREAMING_RUNNER_FIXED = "the daft runner is a process-wide composition fact; this process resolved the other one"


class PlanRefusal(StrEnum):
    UNPARSEABLE = "the wire bytes decode as no substrait Plan"
    NO_RELATION = "the plan carries no root relation to execute"
    RETIRED_EXTENSION_SCHEMA = "the plan declares functions against the retired extension-uri schema this parser drops"
    UNKNOWN_EXTENSION = "the plan names an extension urn this registry resolves nowhere"
    UNTYPED_OUTPUT = "the plan's output schema does not infer, so its result shape is unknowable before execution"


class RemoteOp(StrEnum):
    READ = "read"
    STREAM = "stream"
    INGEST = "ingest"
    PROBE = "probe"
    PARTITION = "partition"


type CallOp = Literal[RemoteOp.READ, RemoteOp.STREAM, RemoteOp.INGEST, RemoteOp.PROBE]


class TimeoutPhase(StrEnum):
    FETCH = "fetch"
    QUERY = "query"
    UPDATE = "update"


class OAuthFlow(StrEnum):
    CLIENT_CREDENTIALS = "client_credentials"
    TOKEN_EXCHANGE = "token_exchange"


class OAuthKey(StrEnum):
    AUTH_URI = "auth_uri"
    TOKEN_URI = "token_uri"
    REDIRECT_URI = "redirect_uri"
    SCOPE = "scope"
    CLIENT_ID = "client_id"
    CLIENT_SECRET = "client_secret"
    EXCHANGE_SUBJECT_TOKEN = "exchange_subject_token"
    EXCHANGE_SUBJECT_TOKEN_TYPE = "exchange_subject_token_type"
    EXCHANGE_ACTOR_TOKEN = "exchange_actor_token"
    EXCHANGE_ACTOR_TOKEN_TYPE = "exchange_actor_token_type"
    EXCHANGE_REQUESTED_TOKEN_TYPE = "exchange_requested_token_type"
    EXCHANGE_SCOPE = "exchange_scope"
    EXCHANGE_AUD = "exchange_aud"
    EXCHANGE_RESOURCE = "exchange_resource"


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


class PlanWire(StrEnum):
    SQL = "sql"
    SUBSTRAIT = "substrait"
    SUBSTRAIT_JSON = "substrait_json"


# --- [CONSTANTS] ------------------------------------------------------------------------

_SCOPE: Final[str] = "rasm.data.tabular.query"

_TRACE_PARENT_KEY: Final[str] = "adbc.telemetry.trace_parent"

_PARTITION_FAN: Final[int] = 4

_PLAN_META: Final[bytes] = b"substrait.plan"


# --- [MODELS] ---------------------------------------------------------------------------


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
        outputs = tuple(sel.alias_or_name for sel in self._qualified(text, self.read).selects)
        roots = (sqlglot_lineage(name, text, schema=self.schema, dialect=self.read) for name in outputs)
        return tuple((str(leaf.name), str(node.name)) for node in roots for leaf in _leaves(node))


class IrEmit(Struct, frozen=True):
    backend_uri: str | None = None
    dialect: Dialects = Dialects.DUCKDB
    wire: PlanWire = PlanWire.SQL
    streaming: bool = False
    optimize: bool = True
    parse: ParseSpec | None = None

    def bound(self, expr: IbisTable) -> IbisTable:
        return ibis.parse_sql(self.parse[0], catalog=self.parse[1], dialect=self.dialect.value) if self.parse is not None else expr

    def sql(self, expr: IbisTable) -> str:
        return str(ibis.to_sql(self.bound(expr), dialect=self.dialect.value))

    def edges(self, expr: IbisTable) -> tuple[LineageEdge, ...]:
        return SqlGate(read=self.dialect, write=self.dialect).edges(self.sql(expr))


class Transport(Struct, frozen=True):
    partition_on: str | None = None
    partition_num: int = _PARTITION_FAN
    ingest_source: str | None = None
    ingest_mode: IngestMode = "create_append"
    ingest_catalog: str | None = None
    ingest_schema: str | None = None
    ingest_temporary: bool = False
    probe_catalog: str | None = None
    probe_schema: str | None = None
    dialect: Dialects = Dialects.DUCKDB
    driver: str | None = None
    entrypoint: str | None = None
    autocommit: bool = False
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
    oauth: Mapping[OAuthKey, str] = field(default_factory=dict)
    session_options: Mapping[str, str] = field(default_factory=dict)
    substrait_version: str | None = None
    db_options: Mapping[str, str] = field(default_factory=dict)
    conn_options: Mapping[str, str] = field(default_factory=dict)
    stmt_options: Mapping[str, str] = field(default_factory=dict)

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
        oauth = {getattr(opts, f"OAUTH_{key.name}").value: value for key, value in self.oauth.items()}
        return dict(self.db_options) | {key.value: value for key, value in rows if value is not None} | timeouts | headers | oauth

    def conn_kwargs(self, conn_opts: "type[ConnectionOptions]") -> dict[str, str]:
        session = {f"{conn_opts.OPTION_SESSION_OPTION_PREFIX.value}{key}": value for key, value in self.session_options.items()}
        return dict(self.conn_options) | session

    def stmt_kwargs(self, opts: "type[StatementOptions]") -> dict[str, str]:
        queue = {opts.QUEUE_SIZE.value: str(self.queue_size)} if self.queue_size is not None else {}
        substrait = {opts.SUBSTRAIT_VERSION.value: self.substrait_version} if self.substrait_version is not None else {}
        return dict(self.stmt_options) | queue | substrait

    def client_kwargs(self) -> dict[str, Any]:
        rows = (
            ("tls_root_certs", self.tls_root_certs),
            ("cert_chain", self.mtls_cert_chain),
            ("private_key", self.mtls_private_key),
        )
        pem = {name: value.encode() for name, value in rows if value is not None}
        return pem | {"override_hostname": self.tls_override_hostname, "disable_server_verification": self.tls_skip_verify or None}

    def call_options(self, plane: Any) -> Any:
        stitched = {"traceparent": parent} if (parent := _trace_parent()) else {}
        credential = {"authorization": self.authorization} if self.authorization is not None else {}
        headers = dict(self.rpc_headers) | credential | stitched
        return plane.FlightCallOptions(
            timeout=self.timeouts.get(TimeoutPhase.QUERY), headers=tuple((k.encode(), v.encode()) for k, v in headers.items())
        )


class Fan(Struct, frozen=True):
    plan: FanPlan
    plan_at: "FaultRow[DataLeg]"
    redeem_at: "FaultRow[DataLeg]"


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
    hive_partitioning: bool = False
    disable_pushdowns: bool = False
    version: str | int | None = None
    asof: str | None = None
    block_size: int | None = None
    repartition: int | None = None
    repartition_by: tuple[str, ...] = ()
    io_config: Any = None


class FederatedStore(Struct, frozen=True):
    ref: ResourceRef
    config: Config | None = None
    client_options: "ClientConfig | None" = None
    retry_config: "RetryConfig | None" = None

    def handle(self) -> "ObjectStore":
        return store_handle(
            self.ref.root,
            config=self.config,
            client_options=self.client_options,
            retry_config=self.retry_config,
            provider=self.ref.credentials,
        )


class FederatedTable(Struct, frozen=True):
    name: str
    path: str
    partition_cols: tuple[tuple[str, str], ...] = ()


@tagged_union(frozen=True)
class Federation:
    tag: Literal["mint", "execute"] = tag()
    mint: str = case()
    execute: bytes = case()


@tagged_union(frozen=True)
class QuerySpec:
    tag: Literal["sql", "rel", "agnostic", "ir", "remote", "streaming", "federated", "flight"] = tag()
    sql: tuple[str, SqlGate | None] = case()
    rel: tuple[str | None, str | None, tuple[str, ...], tuple[str, ...]] = case()
    agnostic: tuple[str | None, tuple[str, ...], tuple[Predicate, ...], tuple[str, ...], tuple[AggExpr, ...]] = case()
    ir: tuple[IbisTable, IrEmit] = case()
    remote: tuple[str, str, RemoteDriver, RemoteOp, Transport] = case()
    streaming: tuple[StreamingPlan, Runner, str | None] = case()
    federated: tuple[Federation, tuple[FederatedStore, ...], tuple[FederatedTable, ...]] = case()
    flight: tuple[bytes, str, Transport] = case()

    @staticmethod
    def Sql(text: str, gate: SqlGate | None = None) -> "QuerySpec":
        return QuerySpec(sql=(text, gate))

    @staticmethod
    def Rel(filter_expr: str | None, project: tuple[str, ...], group_by: tuple[str, ...] = (), frame: str | None = None) -> "QuerySpec":
        return QuerySpec(rel=(frame, filter_expr, project, group_by))

    @staticmethod
    def Agnostic(
        select: tuple[str, ...] = (),
        predicates: tuple[Predicate, ...] = (),
        group_by: tuple[str, ...] = (),
        aggs: tuple[AggExpr, ...] = (),
        frame: str | None = None,
    ) -> "QuerySpec":
        return QuerySpec(agnostic=(frame, select, predicates, group_by, aggs))

    @staticmethod
    def Ir(expr: IbisTable, emit: IrEmit = IrEmit()) -> "QuerySpec":
        return QuerySpec(ir=(expr, emit))

    @staticmethod
    def Remote(
        sql: str,
        dsn: str,
        driver: RemoteDriver = RemoteDriver.ADBC,
        op: RemoteOp = RemoteOp.READ,
        transport: Transport = Transport(),
    ) -> "QuerySpec":
        return QuerySpec(remote=(sql, dsn, driver, op, transport))

    @staticmethod
    def Streaming(plan: StreamingPlan, runner: Runner = Runner.NATIVE, cluster: str | None = None) -> "QuerySpec":
        return QuerySpec(streaming=(plan, runner, cluster))

    @staticmethod
    def Flight(plan: bytes, dsn: str, transport: Transport = Transport()) -> "QuerySpec":
        return QuerySpec(flight=(plan, dsn, transport))

    @staticmethod
    def Federated(
        direction: Federation, stores: tuple[FederatedStore, ...] = (), tables: tuple[FederatedTable, ...] = ()
    ) -> "QuerySpec":
        return QuerySpec(federated=(direction, stores, tables))


# --- [SERVICES] -------------------------------------------------------------------------


class QueryEngine(Struct, frozen=True):
    generation: BackendGeneration
    inputs: Frames
    session: DuckDbSession = DuckDbSession()
    profiling: ProfileMode = ProfileMode.OFF

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(
        cls,
        generation: BackendGeneration,
        inputs: Frames,
        *,
        session: DuckDbSession = DuckDbSession(),
        profiling: ProfileMode = ProfileMode.OFF,
    ) -> "QueryEngine":
        return cls(generation=generation, inputs=inputs, session=session, profiling=profiling)

    @beartype(conf=FAULT_CONF)
    def bench(
        self, spec: QuerySpec, *, mode: BenchMode | None = None, rounds: int = 32, warmup: int = 4
    ) -> "RuntimeResult[Benchmark]":
        return _reach(spec).bind(self._bound).bind(self._benched).bind(
            lambda admitted: Bench.run(
                f"query.{admitted.tag}",
                lambda: anyio.run(self._dispatch, admitted),
                mode=mode or _BENCH_MODE.get(admitted.tag) or BenchMode.LATENCY,
                rounds=rounds,
                warmup=warmup,
            )
        )

    def _benched(self, spec: QuerySpec) -> "RuntimeResult[QuerySpec]":
        match spec:
            case QuerySpec(tag="remote", remote=(_sql, _dsn, _driver, RemoteOp.INGEST, _transport)):
                return Error(BENCH_MUTATION.raised())
            case QuerySpec():
                return Ok(spec)

    @beartype(conf=FAULT_CONF)
    async def run(self, spec: QuerySpec) -> "RuntimeResult[tuple[pa.Table, QueryCensus]]":
        def completed(admitted: QuerySpec, table: pa.Table, fold: "Provenance") -> "RuntimeResult[tuple[pa.Table, QueryCensus]]":
            source, predicates, edges = fold
            census = (
                _wire(admitted, table)
                .map(
                    lambda wire: ContentIdentity.of("query.plan", wire).map(
                        lambda key: QueryCensus.of(
                            admitted.tag,
                            source,
                            table,
                            key,
                            predicate_count=predicates,
                            lineage_edges=edges,
                            mode=self.profiling,
                            profile=EngineProfile.from_table(table),
                        )
                    )
                )
                .default_with(
                    lambda: QueryCensus.derived(
                        admitted.tag,
                        source,
                        table,
                        predicate_count=predicates,
                        lineage_edges=edges,
                        mode=self.profiling,
                    )
                )
            )
            return census.map(lambda result: (table, result))

        match _reach(spec).bind(self._bound):
            case Result(tag="error", error=refused):
                return Error(refused)
            case Result(tag="ok", ok=admitted):
                match await self._dispatch(admitted):
                    case Result(tag="error", error=refused):
                        return Error(refused)
                    case Result(tag="ok", ok=table):
                        return boundary(QUERY_PROVENANCE, lambda: _provenance(admitted), catch=(SqlglotError, ibis.common.exceptions.IbisError, protobuf_message.DecodeError, ValueError)).bind(
                            lambda fold: completed(admitted, table, fold)
                        )
                    case unreachable:
                        assert_never(unreachable)
            case unreachable:
                assert_never(unreachable)

    def _bound(self, spec: QuerySpec) -> "RuntimeResult[QuerySpec]":
        match spec:
            case QuerySpec(tag="rel", rel=(frame, flt, project, group_by)):
                return self._named(frame, "rel").map(lambda name: QuerySpec(rel=(name, flt, project, group_by)))
            case QuerySpec(tag="agnostic", agnostic=(frame, select, predicates, group_by, aggs)):
                return self._named(frame, "agnostic").map(lambda name: QuerySpec(agnostic=(name, select, predicates, group_by, aggs)))
            case QuerySpec(tag="remote", remote=(sql, dsn, driver, RemoteOp.INGEST as op, transport)):
                return self._named(transport.ingest_source, "remote.ingest").map(
                    lambda name: QuerySpec(remote=(sql, dsn, driver, op, replace(transport, ingest_source=name)))
                )
            case QuerySpec():
                return Ok(spec)

    def _named(self, frame: str | None, point: str) -> "RuntimeResult[str]":
        match frame, tuple(self.inputs):
            case str() as named, bound if named in bound:
                return Ok(named)
            case None, (sole,):
                return Ok(sole)
            case None, _:
                return Error(FRAME_UNRESOLVED.raised(point, _UNNAMED))
            case named, _:
                return Error(FRAME_UNRESOLVED.raised(point, str(named)))

    async def _dispatch(self, spec: QuerySpec) -> "RuntimeResult[pa.Table]":
        cell, peer = _cell(spec), _peer(spec)
        match self._body(spec):
            case Fan() as fan:
                return await self._fanned(fan, cell, peer)
            case leg:
                return await self._crossed(leg, cell.leg, cell, peer)

    async def _crossed[T](self, leg: Callable[[], T], at: "FaultRow[DataLeg]", cell: "_Cell", peer: "Option[str]") -> "RuntimeResult[T]":
        return await cell.retry.map(
            lambda retry: guarded(retry, on_thread, leg, abandon=True, at=at, on=peer)
        ).default_with(lambda: async_boundary(at, lambda: on_thread(leg), catch=cell.raises()))

    async def _fanned(self, fan: Fan, cell: "_Cell", peer: "Option[str]") -> "RuntimeResult[pa.Table]":
        match await self._crossed(fan.plan, fan.plan_at, cell, peer):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(legs, schema)):
                async with anyio.create_task_group() as group:
                    handles = legs.map(lambda leg: group.start_soon(partial(self._crossed, leg, fan.redeem_at, cell, peer)))
                return traversed(handles.map(lambda held: held.return_value), by=Disposition.ABORT).map(
                    lambda tables: pa.concat_tables(tables) if tables else schema.empty_table()
                )
            case unreachable:
                assert_never(unreachable)

    def _body(self, spec: QuerySpec) -> "Leg | Fan":
        match spec:
            case QuerySpec(tag="sql", sql=(text, gate)):
                return lambda: self._duckdb(lambda con: con.sql(gate.transpile(text) if gate else text))
            case QuerySpec(tag="rel", rel=(frame, flt, project, group_by)):
                return lambda: self._duckdb(lambda con: _relation(con, self.inputs[frame], flt, project, group_by))
            case QuerySpec(tag="agnostic", agnostic=(frame, select, predicates, group_by, aggs)):
                return lambda: _agnostic(self.inputs[frame], select, predicates, group_by, aggs)
            case QuerySpec(tag="ir", ir=(expr, emit)):
                return lambda: self._ir(expr, emit)
            case QuerySpec(tag="remote", remote=(sql, dsn, driver, op, transport)):
                return _remote(driver, sql, dsn, op, transport, self.inputs)
            case QuerySpec(tag="streaming", streaming=(plan, runner, cluster)):
                return lambda: _stream(plan, runner, cluster, self.inputs, self.profiling)
            case QuerySpec(tag="federated", federated=(direction, stores, tables)):
                return lambda: _federated(direction, stores, tables, self.inputs, self.profiling)
            case QuerySpec(tag="flight", flight=(plan, dsn, transport)):
                return Fan(plan=lambda: _flight_plan(plan, dsn, transport), plan_at=FLIGHT_PLAN, redeem_at=FLIGHT_REDEEM)
            case unreachable:
                assert_never(unreachable)

    def _duckdb(self, build: Callable[[duckdb.DuckDBPyConnection], duckdb.DuckDBPyRelation]) -> pa.Table:
        with replace(self.session, profiling=self.profiling).profiled() as (con, harvest):
            for name, frame in self.inputs.items():
                con.register(name, frame)
            table = build(con).to_arrow_table()
            return harvest(table)

    def _ir(self, expr: IbisTable, emit: IrEmit) -> pa.Table:
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

# --- [ERRORS] ---------------------------------------------------------------------------


def _duck_raises() -> Catch:
    return (duckdb.Error, pa.ArrowException, OSError)


def _agnostic_raises() -> Catch:
    return (NarwhalsError, TypeError, pa.ArrowException, OSError)


def _ir_raises() -> Catch:
    return (ibis.common.exceptions.IbisError, duckdb.Error, pa.ArrowException, OSError)


def _remote_raises() -> Catch:
    return (adbc_manager.Error, RuntimeError, ValueError, pa.ArrowException, OSError)


def _streaming_raises() -> Catch:
    return (daft.exceptions.DaftCoreException, pa.ArrowException, OSError)


def _federated_raises() -> Catch:
    return (ValueError, protobuf_message.DecodeError, pa.ArrowException, OSError)


def _flight_raises() -> Catch:
    return (flight.FlightError, pa.ArrowException, OSError)


_FRONTEND_RAISES: Final[Map[str, Callable[[], Catch]]] = Map.of_seq([
    ("sql", _duck_raises),
    ("rel", _duck_raises),
    ("agnostic", _agnostic_raises),
    ("ir", _ir_raises),
    ("streaming", _streaming_raises),
    ("federated", _federated_raises),
    ("flight", _flight_raises),
])


class _Cell(Struct, frozen=True):
    leg: FaultRow[DataLeg]
    retry: "Option[RetryClass]"
    raises: Callable[[], Catch]


def _celled(point: str, retry: "Option[RetryClass]", raises: Callable[[], Catch]) -> _Cell:
    posture = TRANSIENT if retry.is_some() else TERMINAL
    return _Cell(
        leg=FaultRow(leg=DataLeg.TABULAR_QUERY, point=point, arm="boundary", defect="frontend-leg", retriability=posture),
        retry=retry,
        raises=raises,
    )


QUERY_FLOOR: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="reach.floor", arm="import_", defect="distribution-absent", retriability=TERMINAL, slots=("frontend", "module")
)
QUERY_UNREACHED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="reach.cell", arm="boundary", defect="cell-refused", retriability=TERMINAL, slots=("frontend", "reason")
)
FRAME_UNRESOLVED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="bind", arm="config", defect="frame-unresolved", retriability=TERMINAL, slots=("frontend", "frame")
)
BENCH_MUTATION: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="bench", arm="config", defect="mutation-spec-excluded", retriability=TERMINAL
)
_UNNAMED: Final[str] = "unnamed"

FLIGHT_PLAN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="flight.plan", arm="boundary", defect="fan-plan", retriability=TRANSIENT
)
FLIGHT_REDEEM: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="flight.redeem", arm="boundary", defect="fan-redeem", retriability=TRANSIENT
)
PARTITION_PLAN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="remote.partition.plan", arm="boundary", defect="fan-plan", retriability=TRANSIENT
)
PARTITION_REDEEM: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="remote.partition.redeem", arm="boundary", defect="fan-redeem", retriability=TRANSIENT
)

QUERY_PROVENANCE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="provenance", arm="boundary", defect="provenance-parse", retriability=TERMINAL
)
_ENVELOPE: Final[Map[str, RetryClass]] = Map.of_seq([
    ("remote", RetryClass.REMOTE_DB),
    ("streaming", RetryClass.STREAMING),
    ("flight", RetryClass.REMOTE_DB),
])

_CELLS: Final[Map[str, _Cell]] = Map.of_seq(
    (tag, _celled(tag, _ENVELOPE.try_find(tag), raises)) for tag, raises in _FRONTEND_RAISES.items()
)

_REMOTE_CELLS: Final[Map[RemoteOp, _Cell]] = Map.of_seq(
    (op, _celled(f"remote.{op.value}", Nothing if op is RemoteOp.INGEST else Some(RetryClass.REMOTE_DB), _remote_raises))
    for op in RemoteOp
)

RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    QUERY_FLOOR,
    QUERY_UNREACHED,
    FRAME_UNRESOLVED,
    BENCH_MUTATION,
    QUERY_PROVENANCE,
    FLIGHT_PLAN,
    FLIGHT_REDEEM,
    PARTITION_PLAN,
    PARTITION_REDEEM,
    *(cell.leg for cell in (*_CELLS.values(), *_REMOTE_CELLS.values())),
]))


def _peer(spec: QuerySpec) -> "Option[str]":
    match spec:
        case QuerySpec(tag="remote", remote=(_, dsn, _, _, _)) | QuerySpec(tag="flight", flight=(_, dsn, _)):
            return Some(origin(dsn))
        case QuerySpec(tag="streaming", streaming=(_plan, runner, cluster)):
            return Some(cluster or runner.value)
        case QuerySpec():
            return Nothing


def _cell(spec: QuerySpec) -> "_Cell":
    match spec:
        case QuerySpec(tag="remote", remote=(_sql, _dsn, _driver, op, _transport)):
            return _REMOTE_CELLS[op]
        case QuerySpec(tag=tag):
            return _CELLS[tag]


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


def _relation(
    con: duckdb.DuckDBPyConnection, frame: Any, flt: str | None, project: tuple[str, ...], group_by: tuple[str, ...]
) -> duckdb.DuckDBPyRelation:
    rel = con.from_arrow(frame)
    rel = rel.filter(flt) if flt else rel
    return rel.aggregate(", ".join(project), ", ".join(group_by)) if group_by else rel.project(", ".join(project))


def _agnostic(
    frame: Any, select: tuple[str, ...], predicates: tuple[Predicate, ...], group_by: tuple[str, ...], aggs: tuple[AggExpr, ...]
) -> pa.Table:
    lf = nw.from_native(frame).lazy()
    lf = lf.filter(*(_predicate(p) for p in predicates)) if predicates else lf
    shaped = lf.group_by(*group_by).agg(*(_aggregation(a) for a in aggs)) if group_by else lf.select(*select)
    return shaped.collect().to_arrow()


# --- [SUBSTRAIT]

_SUBSTRAIT: Final[Map[PlanWire, tuple[str, str]]] = Map.of_seq([
    (PlanWire.SUBSTRAIT, ("get_substrait", "from_substrait")),
    (PlanWire.SUBSTRAIT_JSON, ("get_substrait_json", "from_substrait_json")),
])

_REGISTRY: Final[ExtensionRegistry] = ExtensionRegistry(load_default_extensions=True)


def _plan_refusal(wire: bytes) -> "Option[PlanRefusal]":
    plan = substrait_proto.Plan()
    try:
        plan.ParseFromString(wire)
    except DecodeError:
        return Some(PlanRefusal.UNPARSEABLE)
    if not any(relation.HasField("root") for relation in plan.relations):
        return Some(PlanRefusal.NO_RELATION)
    if plan.extensions and not plan.extension_urns:
        return Some(PlanRefusal.RETIRED_EXTENSION_SCHEMA)
    if any(_REGISTRY.lookup_urn(named.urn) is None for named in plan.extension_urns):
        return Some(PlanRefusal.UNKNOWN_EXTENSION)
    try:
        infer_plan_schema(plan)
    except Exception:
        return Some(PlanRefusal.UNTYPED_OUTPUT)
    return Nothing


def _ir_plan(backend: BaseBackend, expr: IbisTable, emit: IrEmit) -> pa.Table:
    con = backend.con
    DuckDbExtension.SUBSTRAIT.load(con)
    serialize, execute = _SUBSTRAIT[emit.wire]
    plan = con.execute(
        f"CALL {serialize}(?, enable_optimizer => ?)", [str(ibis.to_sql(expr, dialect=Dialects.DUCKDB.value)), emit.optimize]
    ).fetchone()[0]
    executed = con.execute(f"CALL {execute}(?)", [plan])
    return executed.to_arrow_reader().read_all() if emit.streaming else executed.to_arrow_table()


# --- [REMOTE]


@contextmanager
def _opened(row: "_Driver", dsn: str, transport: Transport) -> "Iterator[tuple[adbc_dbapi.Connection, adbc_dbapi.Cursor]]":
    with row.ns().connect(**_connect_kwargs(row.kind, transport, dsn)) as conn, conn.cursor(adbc_stmt_kwargs=_stmt_kwargs(row.kind, transport)) as cur:
        yield conn, cur


def _partition_plan(row: "_Driver", sql: str, dsn: str, transport: Transport) -> "tuple[Block[Leg], pa.Schema]":
    with _opened(row, dsn, transport) as (_conn, cur):
        descriptors, schema = cur.adbc_execute_partitions(sql)
    return Block.of_seq(descriptors).map(lambda token: _partition_leg(row, dsn, transport, token)), schema


def _partition_leg(row: "_Driver", dsn: str, transport: Transport, token: bytes) -> Leg:
    def redeemed() -> pa.Table:
        with _opened(row, dsn, transport) as (_conn, cur):
            cur.adbc_read_partition(token)
            return cur.fetch_arrow_table()

    return redeemed


def _dbapi_op(conn: "adbc_dbapi.Connection", cur: "adbc_dbapi.Cursor", sql: str, op: CallOp, transport: Transport, frames: Frames) -> pa.Table:
    match op:
        case RemoteOp.READ:
            cur.execute(sql)
            return cur.fetch_arrow_table()
        case RemoteOp.STREAM:
            cur.execute(sql)
            return cur.fetch_record_batch().read_all()
        case RemoteOp.INGEST:
            written = cur.adbc_ingest(
                sql,
                frames[transport.ingest_source],
                mode=transport.ingest_mode,
                catalog_name=transport.ingest_catalog,
                db_schema_name=transport.ingest_schema,
                temporary=transport.ingest_temporary,
            )
            return pa.table({"table": [sql], "mode": [transport.ingest_mode], "rows": [written]})
        case RemoteOp.PROBE:
            schema = conn.adbc_get_table_schema(sql, catalog_filter=transport.probe_catalog, db_schema_filter=transport.probe_schema)
            return pa.table({"field": schema.names, "type": [str(t) for t in schema.types]})
        case unreachable:
            assert_never(unreachable)


def _trace_parent() -> str | None:
    carrier: dict[str, str] = {}
    propagate.inject(carrier)
    return carrier.get("traceparent")


def _connect_kwargs(kind: DriverKind, transport: Transport, dsn: str) -> dict[str, Any]:
    match kind:
        case DriverKind.MANAGER:
            return {
                "driver": transport.driver,
                "entrypoint": transport.entrypoint,
                "uri": dsn,
                "db_kwargs": dict(transport.db_options) or None,
                "conn_kwargs": dict(transport.conn_options) or None,
                "autocommit": transport.autocommit,
            }
        case DriverKind.FLIGHT:
            stitched = {_TRACE_PARENT_KEY: parent} if (parent := _trace_parent()) else {}
            return {
                "uri": dsn,
                "db_kwargs": transport.db_kwargs(DatabaseOptions) or None,
                "conn_kwargs": (transport.conn_kwargs(ConnectionOptions) | stitched) or None,
            }
        case DriverKind.NATIVE:
            return {"uri": dsn, "db_kwargs": dict(transport.db_options) or None, "conn_kwargs": dict(transport.conn_options) or None}
        case DriverKind.LOCAL | DriverKind.LOADER:
            return {"uri": dsn}
        case unreachable:
            assert_never(unreachable)


def _stmt_kwargs(kind: DriverKind, transport: Transport) -> dict[str, str] | None:
    rows = transport.stmt_kwargs(StatementOptions) if kind is DriverKind.FLIGHT else dict(transport.stmt_options)
    return rows or None


def _dbapi(row: "_Driver", sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> "Leg | Fan":
    if op is RemoteOp.PARTITION:
        return Fan(plan=lambda: _partition_plan(row, sql, dsn, transport), plan_at=PARTITION_PLAN, redeem_at=PARTITION_REDEEM)

    def called() -> pa.Table:
        with _opened(row, dsn, transport) as (conn, cur):
            return _dbapi_op(conn, cur, sql, op, transport, frames)

    return called


def _connectorx(row: "_Driver", sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> "Leg | Fan":
    def called() -> pa.Table:
        ns = row.ns()
        if op is RemoteOp.PROBE:
            return pa.Table.from_pandas(ns.get_meta(dsn, sql, protocol=transport.protocol))
        queries = ns.partition_sql(dsn, sql, transport.partition_on, transport.partition_num) if op is RemoteOp.PARTITION else sql
        result = ns.read_sql(
            dsn,
            queries,
            return_type="arrow_stream" if op is RemoteOp.STREAM else "arrow",
            protocol=transport.protocol,
            partition_on=transport.partition_on if isinstance(queries, str) else None,
            partition_num=transport.partition_num if isinstance(queries, str) and transport.partition_on else None,
        )
        return result.read_all() if op is RemoteOp.STREAM else result

    return called


type DriverRun = Callable[["_Driver", str, str, RemoteOp, Transport, Frames], "Leg | Fan"]

_RUN: Final[Map[DriverKind, DriverRun]] = Map.of_seq([
    (DriverKind.MANAGER, _dbapi),
    (DriverKind.FLIGHT, _dbapi),
    (DriverKind.NATIVE, _dbapi),
    (DriverKind.LOCAL, _dbapi),
    (DriverKind.LOADER, _connectorx),
])


class _Driver(Struct, frozen=True):
    module: str
    ns: Callable[[], Any]
    kind: DriverKind
    system: str


_DRIVER: Final[Map[RemoteDriver, _Driver]] = Map.of_seq([
    (RemoteDriver.ADBC, _Driver(module="adbc_driver_manager", ns=lambda: adbc_dbapi, kind=DriverKind.MANAGER, system="other_sql")),
    (RemoteDriver.CONNECTORX, _Driver(module="connectorx", ns=lambda: cx, kind=DriverKind.LOADER, system="other_sql")),
    (RemoteDriver.FLIGHTSQL, _Driver(module="adbc_driver_flightsql", ns=lambda: flightsql_dbapi, kind=DriverKind.FLIGHT, system="other_sql")),
    (RemoteDriver.POSTGRESQL, _Driver(module="adbc_driver_postgresql", ns=lambda: postgres_dbapi, kind=DriverKind.NATIVE, system="postgresql")),
    (RemoteDriver.SNOWFLAKE, _Driver(module="adbc_driver_snowflake", ns=lambda: snowflake_dbapi, kind=DriverKind.NATIVE, system="snowflake")),
    (RemoteDriver.SQLITE, _Driver(module="adbc_driver_sqlite", ns=lambda: sqlite_dbapi, kind=DriverKind.LOCAL, system="sqlite")),
])


def _remote(driver: RemoteDriver, sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> "Leg | Fan":
    row = _DRIVER[driver]
    return _RUN[row.kind](row, sql, dsn, op, transport, frames)


# --- [FLIGHT]


def _flight_plan(plan: bytes, dsn: str, transport: Transport) -> "tuple[Block[Leg], pa.Schema]":
    options = transport.call_options(flight)
    with _flight_client(dsn, transport) as client:
        info = client.get_flight_info(flight.FlightDescriptor.for_command(plan), options)
        endpoints, schema = tuple(info.endpoints), info.schema
    return Block.of_seq(endpoints).map(lambda endpoint: _flight_leg(_located(endpoint, dsn), endpoint.ticket, transport)), schema


def _flight_client(location: str, transport: Transport) -> Any:
    return flight.FlightClient(location, **transport.client_kwargs())


def _located(endpoint: Any, dsn: str) -> str:
    locations = tuple(endpoint.locations)
    return locations[0].uri.decode() if locations else dsn


def _flight_leg(location: str, ticket: Any, transport: Transport) -> Leg:
    def redeemed() -> pa.Table:
        with _flight_client(location, transport) as peer:
            return peer.do_get(ticket, transport.call_options(flight)).read_all()

    return redeemed


# --- [REACH]

_PROVIDER_MODULE: Final[Map[str, str]] = Map.of_seq([
    *((driver.value, row.module) for driver, row in _DRIVER.items()),
    ("streaming", "daft"),
    ("federated", "datafusion"),
    ("flight", "pyarrow.flight"),
])

_UNREACHED: Final[Map[str, str]] = Map.of_seq(
    (coordinate, module) for coordinate, module in _PROVIDER_MODULE.items() if find_spec(module) is None
)


def _floor(coordinate: str, point: str) -> "Option[RuntimeResult[QuerySpec]]":
    return _UNREACHED.try_find(coordinate).map(lambda module: Error(QUERY_FLOOR.raised(point, module)))

_REMOTE_REFUSAL: Final[Map[tuple[RemoteDriver, RemoteOp], RemoteRefusal]] = Map.of_seq([
    ((RemoteDriver.CONNECTORX, RemoteOp.INGEST), RemoteRefusal.CONNECTORX_READ_ONLY),
    ((RemoteDriver.SQLITE, RemoteOp.PARTITION), RemoteRefusal.SQLITE_NO_PARTITION),
    ((RemoteDriver.POSTGRESQL, RemoteOp.PARTITION), RemoteRefusal.POSTGRESQL_NO_PARTITION),
    ((RemoteDriver.SNOWFLAKE, RemoteOp.PARTITION), RemoteRefusal.SNOWFLAKE_NO_PARTITION),
])


def _conditional(driver: RemoteDriver, op: RemoteOp, transport: Transport) -> "Option[RemoteRefusal]":
    return (
        Some(RemoteRefusal.CONNECTORX_PARTITION_COLUMN)
        if driver is RemoteDriver.CONNECTORX and op is RemoteOp.PARTITION and transport.partition_on is None
        else Nothing
    )


def _reach(spec: QuerySpec) -> "RuntimeResult[QuerySpec]":
    match spec:
        case QuerySpec(tag="remote", remote=(_sql, _dsn, driver, op, transport)):
            point = f"remote.{op.value}"
            return (
                _floor(driver.value, point)
                .or_else_with(
                    lambda: _REMOTE_REFUSAL.try_find((driver, op))
                    .or_else_with(lambda: _conditional(driver, op, transport))
                    .map(lambda refusal: Error(QUERY_UNREACHED.raised(point, refusal.value)))
                )
                .default_value(Ok(spec))
            )
        case QuerySpec(tag="flight", flight=(_wire, _dsn, transport)) if transport.oauth_flow is not None:
            return Error(QUERY_UNREACHED.raised("flight", RemoteRefusal.FLIGHT_NATIVE_OAUTH.value))
        case QuerySpec(tag="streaming", streaming=(_plan, runner, _cluster)):
            return (
                _floor("streaming", "streaming")
                .or_else_with(
                    lambda: Some(Error(QUERY_UNREACHED.raised("streaming", RemoteRefusal.STREAMING_RUNNER_FIXED.value)))
                    if daft.get_or_infer_runner_type() != runner.value
                    else Nothing
                )
                .default_value(Ok(spec))
            )
        case (
            QuerySpec(tag="federated", federated=(Federation(tag="execute", execute=wire), _, _))
            | QuerySpec(tag="flight", flight=(wire, _, _))
        ):
            return (
                _floor(spec.tag, spec.tag)
                .or_else_with(lambda: _plan_refusal(wire).map(lambda refusal: Error(QUERY_UNREACHED.raised(spec.tag, refusal.value))))
                .default_value(Ok(spec))
            )
        case QuerySpec(tag=tag):
            return _floor(tag, tag).default_value(Ok(spec))


def dbapi_drivers() -> tuple[DbapiDriver, ...]:
    return (
        DbapiDriver(name=_SCOPE, connect_module=duckdb, connect_method_name="connect", database_system="duckdb"),
        *(
            DbapiDriver(name=_SCOPE, connect_module=row.ns(), connect_method_name="connect", database_system=row.system)
            for driver, row in _DRIVER.items()
            if row.kind is not DriverKind.LOADER and _UNREACHED.try_find(driver.value).is_none()
        ),
    )


# --- [STREAMING]

_DAFT_READ: Final[Map[LakehouseFormat, tuple[str, str | None, bool]]] = Map.of_seq([
    (LakehouseFormat.PARQUET, ("read_parquet", None, True)),
    (LakehouseFormat.ICEBERG, ("read_iceberg", "snapshot_id", False)),
    (LakehouseFormat.DELTA, ("read_deltalake", "version", False)),
    (LakehouseFormat.HUDI, ("read_hudi", None, False)),
    (LakehouseFormat.LANCE, ("read_lance", "version", False)),
    (LakehouseFormat.SQL, ("read_sql", None, False)),
])


def _daft_scan(plan: StreamingPlan) -> Any:
    name, version_key, hive = _DAFT_READ[plan.fmt]
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
    layout = {"hive_partitioning": True} if hive and plan.hive_partitioning else {}
    return reader(plan.source, io_config=plan.io_config, **travel, **layout)


class _Shape(Struct, frozen=True):
    named: str
    applies: Callable[[StreamingPlan], bool]
    apply: Callable[[Any, StreamingPlan], Any]


_SHAPING: Final[Block[_Shape]] = Block.of_seq([
    _Shape("where", lambda plan: bool(plan.predicate), lambda frame, plan: frame.where(plan.predicate)),
    _Shape(
        "with_columns",
        lambda plan: bool(plan.with_columns),
        lambda frame, plan: frame.with_columns({name: daft.sql_expr(expr) for name, expr in plan.with_columns.items()}),
    ),
    _Shape(
        "agg",
        lambda plan: bool(plan.group_by),
        lambda frame, plan: frame.groupby(*plan.group_by).agg(*(daft.sql_expr(expr) for expr in plan.agg)),
    ),
    _Shape("explode", lambda plan: bool(plan.explode), lambda frame, plan: frame.explode(*plan.explode)),
    _Shape("distinct", lambda plan: plan.distinct is not None, lambda frame, plan: frame.distinct(*plan.distinct)),
    _Shape("sample", lambda plan: plan.sample is not None, lambda frame, plan: frame.sample(plan.sample)),
    _Shape("sort", lambda plan: bool(plan.sort), lambda frame, plan: frame.sort(list(plan.sort), desc=plan.sort_desc)),
    _Shape("select", lambda plan: bool(plan.project), lambda frame, plan: frame.select(*plan.project)),
    _Shape("limit", lambda plan: plan.limit is not None, lambda frame, plan: frame.limit(plan.limit)),
    _Shape("repartition", lambda plan: bool(plan.repartition), lambda frame, plan: frame.repartition(plan.repartition, *plan.repartition_by)),
])


_DURATION_UNIT: Final[Map[str, float]] = Map.of_seq([("ns", 1e-9), ("us", 1e-6), ("µs", 1e-6), ("ms", 1e-3), ("s", 1.0)])


def _daft_seconds(duration: Mapping[str, Any] | None) -> float:
    return 0.0 if not duration else float(duration["value"]) * (_DURATION_UNIT.get(str(duration.get("unit") or "s")) or 1.0)


def _daft_operators(metrics: Any) -> tuple[tuple[str, float, int], ...]:
    return tuple(
        (str(row["name"]), _daft_seconds(stats.get("duration")), int((stats.get("rows.out") or {"value": 0.0})["value"]))
        for row in metrics.to_pylist()
        for stats in (dict(row["stats"] or ()),)
    )


def _stream(plan: StreamingPlan, runner: Runner, cluster: str | None, frames: Frames, profiling: ProfileMode = ProfileMode.OFF) -> pa.Table:
    if runner is Runner.RAY:
        daft.set_runner_ray(address=cluster, noop_if_initialized=True)
    scan = _daft_scan(plan)
    bound = (
        daft.sql(plan.sql, register_globals=False, **{name: daft.from_arrow(frame) for name, frame in frames.items()} | {"this": scan})
        if plan.sql and plan.fmt is not LakehouseFormat.SQL
        else scan
    )
    shaped = _SHAPING.fold(lambda frame, row: row.apply(frame, plan) if row.applies(plan) else frame, bound)
    if profiling is ProfileMode.OFF:
        return shaped.to_arrow()
    started = perf_counter()
    materialized = shaped.collect()
    latency_s = perf_counter() - started
    table = materialized.to_arrow()
    metrics = materialized.metrics
    operators = _daft_operators(metrics) if metrics is not None else ()
    return EngineProfile.of(
        ProfileHarvest(daft=(latency_s, table.num_rows, table.nbytes, materialized.num_partitions() or 1, operators))
    ).stamp(table)


# --- [FEDERATION]


def _federated(
    direction: Federation,
    stores: tuple[FederatedStore, ...],
    tables: tuple[FederatedTable, ...],
    frames: Frames,
    profiling: ProfileMode = ProfileMode.OFF,
) -> pa.Table:
    ctx = SessionContext()
    for store in stores:
        ctx.register_object_store(store.ref.scheme, store.handle(), store.ref.root)
    for table_row in tables:
        ctx.register_parquet(table_row.name, table_row.path, table_partition_cols=list(table_row.partition_cols))
    for name, frame in frames.items():
        ctx.from_arrow(frame, name=name)
    match direction:
        case Federation(tag="execute", execute=plan):
            bound, wire = ctx.create_dataframe_from_logical_plan(dfs.Consumer.from_substrait_plan(ctx, dfs.Serde.deserialize_bytes(plan))), plan
        case Federation(tag="mint", mint=text):
            bound, wire = ctx.sql(text), dfs.Serde.serialize_bytes(text, ctx)
        case unreachable:
            assert_never(unreachable)
    started = perf_counter()
    batches = [batch.to_pyarrow() for batch in bound.execute_stream()]
    latency_s = perf_counter() - started
    table = (pa.Table.from_batches(batches) if batches else pa.table({})).replace_schema_metadata({_PLAN_META: wire})
    if profiling is ProfileMode.OFF:
        return table
    return EngineProfile.of(ProfileHarvest(scalar=(latency_s, table.num_rows, table.nbytes))).stamp(table)


type Provenance = tuple[str, int, tuple[LineageEdge, ...]]

_PLAN_PREDICATES: Final[frozenset[str]] = frozenset({"filter", "join", "hash_join", "merge_join", "nested_loop_join"})

_REL: Final = substrait_proto.Rel.DESCRIPTOR


def _plan_rels(plan: "substrait_proto.Plan") -> "Iterator[str]":
    def walked(node: Any) -> "Iterator[str]":
        if node.DESCRIPTOR is _REL:
            kind = node.WhichOneof("rel_type")
            if kind is not None:
                yield kind
                yield from walked(getattr(node, kind))
            return
        for field in node.DESCRIPTOR.fields:
            if field.message_type is None or field.message_type.GetOptions().map_entry:
                continue
            held = getattr(node, field.name)
            for child in held if field.is_repeated else ((held,) if node.HasField(field.name) else ()):
                yield from walked(child)

    for entry in plan.relations:
        yield from walked(entry.root.input if entry.WhichOneof("rel_type") == "root" else entry.rel)


def _plan_provenance(wire: bytes) -> Provenance:
    plan = substrait_proto.Plan()
    plan.ParseFromString(wire)
    rels = tuple(_plan_rels(plan))
    edges = (*(("substrait", kind) for kind in rels), *(("substrait-urn", named.urn) for named in plan.extension_urns))
    return "substrait-plan", sum(1 for kind in rels if kind in _PLAN_PREDICATES), edges


def _endpoint(dsn: str) -> str:
    parsed = urlsplit(dsn)
    match Option.of_optional(parsed.hostname):
        case Option(tag="none"):
            return dsn if not parsed.netloc else urlunsplit((parsed.scheme, parsed.netloc.rpartition("@")[2], parsed.path, "", ""))
        case Option(tag="some", some=hostname):
            host = f"{hostname}:{parsed.port}" if parsed.port else hostname
            return dsn if not parsed.netloc else urlunsplit((parsed.scheme, host, parsed.path, "", ""))


def _provenance(spec: QuerySpec) -> Provenance:
    match spec:
        case QuerySpec(tag="sql", sql=(text, gate)):
            return text, predicate_count(text), (gate or SqlGate()).edges(text)
        case QuerySpec(tag="ir", ir=(expr, emit)):
            text = emit.sql(expr)
            return text, predicate_count(text), SqlGate(read=emit.dialect, write=emit.dialect).edges(text)
        case QuerySpec(tag="rel", rel=(_frame, flt, _project, _group_by)):
            return spec.tag, int(flt is not None), ()
        case QuerySpec(tag="agnostic", agnostic=(_frame, _select, predicates, _group_by, _aggs)):
            return spec.tag, len(predicates), ()
        case QuerySpec(tag="remote", remote=(_subject, dsn, _driver, RemoteOp.INGEST | RemoteOp.PROBE, _transport)):
            return _endpoint(dsn), 0, ()
        case QuerySpec(tag="remote", remote=(sql, dsn, _driver, _op, transport)):
            gate = SqlGate(read=transport.dialect, write=transport.dialect)
            return _endpoint(dsn), predicate_count(sql), gate.edges(sql)
        case QuerySpec(tag="streaming", streaming=(plan, _runner, _cluster)):
            return (
                plan.source,
                predicate_count(plan.sql) if plan.sql else int(plan.predicate is not None),
                SqlGate().edges(plan.sql) if plan.sql else (),
            )
        case QuerySpec(tag="federated", federated=(Federation(tag="mint", mint=text), _stores, _tables)):
            return text, predicate_count(text), SqlGate().edges(text)
        case QuerySpec(tag="federated", federated=(Federation(tag="execute", execute=wire), _stores, _tables)):
            return _plan_provenance(wire)
        case QuerySpec(tag="flight", flight=(plan, dsn, _transport)):
            _source, predicates, edges = _plan_provenance(plan)
            return _endpoint(dsn), predicates, edges
        case unreachable:
            assert_never(unreachable)


def _wire(spec: QuerySpec, table: pa.Table) -> "Option[bytes]":
    match spec:
        case QuerySpec(tag="federated"):
            return Option.of_optional((table.schema.metadata or {}).get(_PLAN_META))
        case QuerySpec(tag="flight", flight=(plan, _dsn, _transport)):
            return Some(plan)
        case QuerySpec():
            return Nothing


```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
