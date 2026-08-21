# [PY_DATA_QUERY]

Relational query owner over one `QuerySpec` axis materializing to uniform Arrow. `QueryEngine` discriminates the `QuerySpec` tagged-union — DuckDB SQL gated through the `sqlglot` parse/qualify/optimize plane, the chained DuckDB relational API, the dataframe-agnostic narwhals surface, the Ibis backend-agnostic expression IR with cross-dialect emission, the ADBC/ConnectorX/Flight SQL remote transport over a `RemoteOp` read/stream/ingest/probe/partition sub-axis, the daft out-of-core/distributed runner, and the in-process `datafusion` federated engine — onto one `pyarrow.Table`. Frontend identity IS the spec shape, never a parallel backend `StrEnum` knob and never a `read`/`stream`/`ingest` name-suffix method family, and no serve-plane servicer is authored here: the federation channel is content-keyed and AT-REST.

`datafusion` Substrait interchange is BIDIRECTIONAL — outbound `Serde.serialize_bytes` mints the portable plan bytes `csharp:Rasm.Persistence/Query/federation` retains as the content-keyed at-rest wire, inbound a Persistence-authored plan arrives as BYTES, admits through the standalone `substrait` plan gate at `_reach`, and only then executes through `Serde.deserialize_bytes` -> `Consumer.from_substrait_plan`, data executing foreign plans and never re-planning them; direction rides the `Federation` two-case discriminant its payload carries, so no runtime XOR gate, rail-returning factory, or unreachable terminal re-derives it. DuckDB's in-process half is a distinct wire: `substrait` attaches SQL TABLE FUNCTIONS alone, so each `PlanWire` half is a `CALL` through `con.execute` off the `_SUBSTRAIT` row — a connection-bound `get_substrait`/`from_substrait` method is not a DuckDB member and never enters. `QueryReceipt`, the `predicate_count` fold, and its `_PREDICATE_NODES` widening are the lower `tabular/columnar#SCAN` owner's, imported rather than re-spelled so scan and query count predicates off one application; this owner extends the receipt with column-level `lineage_edges` off `sqlglot.lineage.lineage` over the qualified SQL and `ibis.to_sql` over the bound expression. Awaitable `run` gates one `_reach`-then-`_bound` prologue over the whole `QuerySpec` axis, then offloads every blocking leg to the `anyio` worker pool through one `_crossed` envelope — in-process arms cross it unretried, remote, streaming, and flight arms delegate the retried-traced-railed leg to `runtime/reliability/resilience#RESILIENCE` under the `REMOTE_DB`/`STREAMING` rows, and a producer-partitioned result crosses it once per leg through `_fanned`. Secret-bearing DSNs and the Ray cluster-address ride caller-supplied `Remote.dsn`/`Streaming.cluster` payloads, the outbound credential minted caller-side through `runtime/execution/admission#SETTINGS` `SecretBoundary`, never `runtime/transport/roots#RESOURCE` `TransportResource`; provenance keeps the redacted scheme-and-host coordinate alone, so no credential reaches a durable receipt. Durable provenance ledgers stay the C# `Rasm.Persistence` Version/Provenance owner consumed at the wire, never a Python owner.

## [01]-[INDEX]

- [02]-[QUERY]: `QueryEngine` folds one `QuerySpec` axis to uniform Arrow, extending the `columnar`-shared receipt with column-level lineage.

## [02]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner discriminating by the `QuerySpec` tagged-union axis, the single discriminant. `QuerySpec` cases: `Sql`/`Rel`/`Agnostic`/`Ir` in-process, `Remote` over the ADBC/ConnectorX/Flight SQL `RemoteOp` sub-axis, `Streaming` the daft runner, `Flight` the `csharp:Rasm.Persistence/Query/federation` FLIGHT_RESULT_PLANE ticket consumer (SubstraitPlan command bytes -> `GetFlightInfo` -> `DoGet(FlightTicket)` over every returned endpoint), and `Federated` the in-process `datafusion` `SessionContext` over `register_object_store`-backed stores and Arrow-capsule-registered frames, carrying one `Federation` direction — `mint` the outbound plan-minting SQL, `execute` the inbound Persistence-authored Substrait bytes — the two directions of the one `ARCH`-declared `⇄` seam on one case, the minted-or-received bytes stamped onto the result table's schema metadata so the plan rides the wire and keys the receipt.
- Cases: the driver sub-axis is one `_DRIVER` row per `RemoteDriver`, and `DriverKind` is the closed discriminant every per-driver shape derives from — the manager front end takes the caller's `driver`/`entrypoint` shared-library coordinate, Flight SQL alone takes the typed `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` projection, a bundled native driver takes `db_kwargs`/`conn_kwargs` and resolves its own library, a local driver's `connect` admits the URI alone (a `db_kwargs=` keyword is a `TypeError` there), and the loader kind carries no PEP-249 connection to open, wrap, or instrument. Each row's `ns` is a CALL-TIME thunk: a table row dereferencing a `lazy` module attribute reifies every driver proxy at import and defeats the floor gate the roster reads. Statement options ride one `cursor(adbc_stmt_kwargs=)` open per dispatch, so a timeout, queue size, or Substrait pin reaches every op rather than the partition leg alone.
- Law: `_reach` is the one gate over the whole `QuerySpec` axis, read as `run`'s prologue so `_dispatch` is total over admitted specs and no arm re-probes — reject law is data exactly as the sibling `tabular/lakehouse#LAKEHOUSE` matrix states it. `_UNREACHED` derives at import from one `find_spec` per `_PROVIDER_MODULE` row, and that roster derives its driver half from the `_DRIVER` table and carries one row per remaining `lazy`-bound frontend — so a provider the manifest gates below the interpreter floor answers `QUERY_FLOOR.raised(frontend, module)` naming the absent module rather than raising `ModuleNotFoundError` inside an offloaded thread, on the daft, datafusion, and Flight planes exactly as on the remote drivers. `_REMOTE_REFUSAL` rows each `(driver, op)` cell a present provider cannot serve and `_conditional` each cell the transport payload decides, both answering `QUERY_UNREACHED.raised(frontend, reason)` with a typed `RemoteRefusal` whose member value IS the operator-facing evidence. `QueryEngine` honours a caller's `RemoteDriver` choice verbatim: an implicit ADBC-to-ConnectorX swap on a partitioned read overrode an explicit selection AND routed into a floor-absent distribution, so ADBC partition fan-out rides its own native `adbc_execute_partitions` and ConnectorX serves the specs that name it.
- Law: the two INBOUND plan legs — `Federated` execute and `Flight` ticket — admit at that same gate through `_plan_refusal`: the plan's own protobuf model parses the wire, `_REGISTRY` resolves every extension urn it names, and `infer_plan_schema` proves its result shape is knowable before a row is read. Letting the executor's deserializer decide admission answers a foreign payload with a Rust fault indistinguishable from a transport failure, carrying no reason a `PlanRefusal` row states as data, and letting the Flight producer decide it spends a round trip to learn the same thing. That gate is this branch's descriptor-drift owner for the substrait wire and stands DISJOINT from the `runtime/transport/shapes#REGISTRY_AND_DRIFT` `PROTO_VOCABULARY` registry: that registry mints the branch's OWN `channels_pb2` RPC vocabulary and grades its compiled descriptors at boot, where the substrait `Plan` is a FOREIGN vocabulary this branch mints nothing of and the drift it can suffer is the producer's own schema move — which `RETIRED_EXTENSION_SCHEMA` catches per plan, at the only moment a foreign plan exists to grade. A row for it on that registry would name a `channels_pb2` message nothing generates and seat a data-owned plan shape at S0.
- Law: `_bound` is the second prologue leg, resolving each frame-naming arm's input slot ONCE — a spec naming no frame binds the sole bound input, several bound inputs with no name refuse typed, and a named absent frame refuses — so `_dispatch` reads `self.inputs[name]` totally and no arm picks a frame off iteration order. `QueryEngine` holds one admitted `BackendGeneration`; query specs never carry raw contract bytes or repeated admission knobs.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; the awaitable `run` threads the reach-then-bind prologue then folds the `QuerySpec` through `_dispatch`, whose `_body` `match`/`case` closed by `assert_never` destructures operands alone and whose one tail owns the envelope — `_cell` reading the frontend's whole `_Cell` — its three fault anchors, its retry class, and its provider raise set — so the retry law lives at one site and a new frontend inherits it rather than a neighbour's copied call. Its leg anchor GENERATES off one `_celled` declaration per frontend and per `RemoteOp`, so a sixth transport operation lands its row by being a member of the closed roster; the two fan PHASE anchors ride the `Fan` itself, because exactly two of the twelve cells ever answer one and a pair generated per cell seats twenty coordinates no site can raise. `_body` answers the SHAPE its frontend is: one `Leg` for a whole result, or the `Fan` a producer-partitioned one already carries, so `_crossed` puts one envelope on both and `_fanned` spends the plan hop once and redeems every leg CONCURRENTLY under the bound `on_thread` already takes, concatenating in the producer's own order. Draining a fan leg after leg pays the partitioning round trip and collects nothing for it, which is what left the ADBC descriptor set and the Flight endpoint set slower than the unpartitioned read they were selected over; the retry class rides each leg, so a transient on one partition retries that partition rather than re-planning the whole fan. An absent class is the unretried envelope: one `async_boundary` offloading the blocking materialization off the event loop through `on_thread` (the `THREAD_BAND`-bounded hop), its `catch` the frontend's OWN named union rather than a bare `Exception` — `duckdb.Error`, `ibis.IbisError`, `NarwhalsError`, `adbc_driver_manager.Error`, `DaftCoreException`, `flight.FlightError`, and the Arrow core rail are disjoint roots with no shared base, so the cell carries its set as a THUNK and reifies it only where that frontend runs, which is what a module-scope tuple over seven providers could not do behind seven `lazy` binds. The remote, streaming, and flight cells carry `RetryClass.REMOTE_DB`/`RetryClass.STREAMING`, whose `_adbc_transient` hook (ADBC `OperationalError` on `status_code` `TIMEOUT`/`IO`) and `DaftTransientError` tuple retry a genuine transport-transient under runtime backoff — never `RetryClass.RPC`/`WIRE`, whose `_transient` COMPAS module-qualified spellings and `ConnectionError` intra-mesh target catch no ADBC `OperationalError` (which subclasses `DatabaseError`, never `ConnectionError`) or daft Rust fault. Every connection is request-scoped: the DuckDB `Sql`/`Rel` connection rides the shared `tabular/columnar#SCAN` `DuckDbSession().connect()` bracket, the `_ir` backend releases through `try`/`finally` `backend.disconnect()` closing the native `backend.con` the Substrait round-trip drives, the remote drivers ride `with row.ns().connect(...)`, and the Flight client and each per-location peer ride their own `with` bracket.
- Auto: the `tabular/lakehouse#LAKEHOUSE` analytics residence is a SOURCE this axis already reaches, never a lane of its own — `Streaming` scans the cold hive tree distributed under its `hive_partitioning` row, `Federated` registers that tree as a datafusion listing table beside the object store holding it, `Sql`/`Rel` read the evidence table interactively off the caller's `session` policy carrying the `delta` extension row, `Agnostic` folds it dataframe-agnostically, and `Flight` beside `RemoteDriver.FLIGHTSQL` serves the remote query end. That same `session` slot carries the `postgres_scanner` `Attach` row, so the `[TENANT_COST_JOIN]` fold against live tenant, grant, and workload tables is ONE statement rather than a second transport. Selection is a spec shape, so a residence earns no engine-shaped surface and every engine lands the one Arrow frame the residence schema pins.
- Receipt: `_plan_provenance` censuses the admitted plan for both inbound legs — `_plan_rels` walks the plan's whole message tree and recognises a relation by the `Rel` descriptor rather than by the field holding it, so a subquery-nested join counts exactly as a direct child does and `_PLAN_PREDICATES` counts the classes `_PREDICATE_NODES` counts on the SQL legs, where `sqlglot` already walks into subqueries. Relation roster and resolved extension urns both ride the lineage slot under their own edge classes, so one provenance grain spans the SQL and plan wires and a foreign producer's function vocabulary survives the gate that resolved it.
- Receipt: `receipt_of` folds one total `_provenance` match over the `QuerySpec` axis into the shared `tabular/columnar#SCAN` `QueryReceipt.railed` — source, predicate count, and lineage edges — behind one `boundary` fence, because `_provenance` parses caller SQL through `sqlglot` and a malformed or dialect-foreign statement raises past a rail the signature already declares. DSN-sourced provenance keeps the redacted `_endpoint` coordinate, never the credential-bearing DSN. Content keys derive off canonical Arrow bytes except where Substrait plan bytes own identity. Every profiled arm stamps the shared band: DuckDB, Polars, and DataFusion fold portable execution scalars; Daft adds admitted native operator rows. DBAPI span coverage rides the runtime composition-root wrap seam beside `TRAIN`: `dbapi_seams()` declares each admitted connection factory whose distribution this floor resolves, and the root threads those rows through `Instrumentation.dbapi`. ConnectorX exposes no PEP-249 connection object, so its guarded child span and receipt remain its evidence. Flight SQL injects W3C parent context through its admitted option row. `QueryEngine.bench` spends the reach-then-bind prologue ONCE and rounds `_dispatch`, so a refused spec answers its typed row instead of being timed N times as a microsecond success and an admitted plan frontend is timed against the query rather than against its own parse-and-infer admission gate; it refuses mutation INGEST and leaves the process-terminal `JobRun.bounded` envelope to its caller.
- Packages: `duckdb`/`sqlglot` (the parse/qualify/optimize/lineage plane), `narwhals`, `ibis`, `adbc_driver_manager`/`adbc_driver_flightsql`/`adbc_driver_postgresql`/`adbc_driver_snowflake`/`adbc_driver_sqlite` (the five admitted DBAPI transports, the Flight SQL row alone binding the `DatabaseOptions`/`StatementOptions`/`ConnectionOptions`-keyed `db_kwargs`/`conn_kwargs`/`adbc_stmt_kwargs` knobs), `connectorx` (the read-parallel accelerator over ADBC's serial pull, its `read_sql`/`partition_sql`/`get_meta` surface bound behind the `_UNREACHED` floor gate while the manifest marker holds it below the interpreter floor), `daft` (the out-of-core/distributed runner), `datafusion` (the federation `Serde`/`Consumer` Substrait executor — the DuckDB `substrait` extension owns the in-process half through its `CALL` table functions, `pyarrow.substrait` DECLINED as consumer-only: it mints no `Plan` at all, since `serialize_expressions` emits an `ExtendedExpression` and `serialize_schema` a `SubstraitSchema`, so the outbound half of this seam has no producer there; `run_query` resolves each `NamedTable` through a per-call `table_provider` callback rather than a registered catalog and validates nothing ahead of execution, where `_plan_refusal` answers a typed verdict), `substrait` (`proto.Plan`/`ExtensionRegistry`/`infer_plan_schema` — the inbound-plan admission gate the manifest admits this distribution for, and the executor's own deserializer never becomes), `pyarrow.flight` (`FlightClient`/`FlightDescriptor.for_command`/`FlightInfo.endpoints`/`FlightEndpoint.ticket`+`locations`/`FlightCallOptions` — the ticket-redemption plane), `anyio` (`anyio.run` drives the awaitable `run` to completion per bench round on a fresh loop off the serving loop), `beartype` (`@beartype(conf=FAULT_CONF)` on `of`/`run`/`bench`), `tabular/columnar#SCAN` (the shared `DuckDbSession`/`DuckDbExtension`/`QueryReceipt`/`predicate_count` substrate with the `EngineProfile`/`ProfileHarvest`/`ProfileMode` profile band the `datafusion`/`daft` arms harvest onto), `tabular/interop#INTEROP` (`DataLeg`, the roster this page anchors its whole `RAISES` table on), `google.protobuf` (`message.DecodeError` alone, the one raise a malformed plan wire answers), runtime (`RuntimeRail`/`ContentIdentity`/`async_boundary`/`boundary`/`guarded`/`RetryClass`/`on_thread`, `runtime/transport/roots#STORE` `store_handle`/`Config`/`ResourceRef` as the branch's one `from_url` fold every `FederatedStore` row binds through, the credential riding `ref.credentials`, and `runtime/observability/profiles#BENCH` `Bench.run`/`BenchMode`/`BenchmarkReceipt` the query bench lane composes — `DbapiSeam` the declared row shape and `Instrumentation.dbapi` the composition-root wrap `dbapi_seams()` feeds, never a data-altitude activation).
- Growth: a new DuckDB extension or attached catalog on the in-process arms is one row on the caller's `session` policy, zero owner edits; a new query frontend is one `QuerySpec` case; a new SQL dialect target is one `Dialects` member the `SqlGate`/`IrEmit` already thread; a new plan-wire artifact is one `PlanWire` row with its `(serialize, execute)` `_SUBSTRAIT` pair; a new federation direction is one `Federation` case; a new inbound-plan bound is one `PlanRefusal` row read in `_plan_refusal`; a newly predicate-bearing relation kind is one `_PLAN_PREDICATES` member, the generic `_plan_rels` walk already reaching it wherever the algebra seats it; an estate function vocabulary beyond the standard extensions is one `_REGISTRY.register_extension_yaml` call at composition; a new predicate-bearing node is one `_PREDICATE_NODES` row on the lower `tabular/columnar#SCAN` owner the exported `predicate_count` already scans; a new transport operation is one `RemoteOp` row under `assert_never`; a new remote driver is one `_DRIVER` row naming its module, namespace thunk, `DriverKind`, and `db.system.name`, from which the floor roster, the connect projection, the statement projection, the executor, and the instrumentation seam all derive; a new driver-connect dialect is one `DriverKind` member with its arm on `_connect_kwargs`/`_stmt_kwargs`/`_RUN`; a newly-unreachable cell is one `_REMOTE_REFUSAL` row or one `_conditional` arm carrying its `RemoteRefusal` reason; a new typed Flight knob is one `Transport` field folded into `db_kwargs`/`conn_kwargs`/`stmt_kwargs`; a driver-native knob no enum here models is one `Transport.db_options`/`conn_options`/`stmt_options` entry; a new timeout phase is one `TimeoutPhase` row; a new OAuth key is one `OAuthKey` row the `oauth` projection folds; a new daft runner is one `Runner` row; a new lakehouse source is one `LakehouseFormat` row on the `_DAFT_READ` table with its time-travel key; a new federated store backend is one `FederatedStore` row `register_object_store` federates, its scheme, host, and credential all deriving from the ref it carries; a new federated relation over those bytes is one `FederatedTable` row `register_parquet` registers; a new daft shaping verb is one `StreamingPlan` field and one `_SHAPING` row, its position in that `Block` the applied order; a new frontend's envelope is one `_FRONTEND_RAISES` row naming its provider raise thunk plus one `_ENVELOPE` row where it retries, its leg anchor and its posture both deriving through `_celled`; a newly fanning frontend is one `plan_at`/`redeem_at` pair on the `Fan` its own `_body` arm builds; a frontend whose producer partitions its result answers a `Fan` off its own `_body` arm and inherits the plan hop, the concurrent redemption, and the ordered concat untouched; a new ConnectorX planning width is the `_PARTITION_FAN` constant; a new agnostic comparator or aggregator is one `Comparator`+`_COMPARE` row or one `Aggregator` member; a new profiling engine is one `ProfileHarvest` case on the lower `tabular/columnar#SCAN` owner the arm folds its execution scalars through; a new benchmarked frontend inherits the lane free and tunes its default with one `_BENCH_MODE` row; a relational verb composes on the existing chain; zero new surface.
- Boundary: no durable query rail and no global connection; no SQL-string templating or regex rewriting where the `sqlglot` AST owns structure; no hand-rolled Substrait protobuf codec where the extensions own each half, and the `CALL` statements interpolate a function name off the closed `_SUBSTRAIT` row while every value binds as a parameter, so no caller string reaches a statement; no per-setting builder type where the `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` enum value keys the option, and a hand-spelled twin of an option the enum already names is the deleted form; the ADBC partition fan-out rides `Cursor.adbc_execute_partitions` and the result-set-rebinding `adbc_read_partition` on one bracket per descriptor — the rebind clears the cursor's held result and this DBAPI declares `threadsafety=1`, so a concurrent leg owns its own connection and cursor both — the ConnectorX one rides `partition_sql` and fans inside the provider, never a hand-stitched gRPC loop or low-level `AdbcStatement` dance; a free-string dialect bypassing `Dialect.get_or_raise`, a `find_tables`×`exp.Column` cartesian where `sqlglot.lineage.lineage` owns column provenance, and a `register_globals`-leaking `daft.sql` over unbound globals are foreclosed; `of`/`run`/`bench` carry the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `interop`/`egress`/`columnar` admission entrypoints share. Deleted forms: a bare `obstore.store.from_url` federating a store beside the branch `store_handle` fold, which reaches no credential provider and re-dials under a retry policy no page states; a driver swap the caller never asked for, a lazy provider import reached with no floor gate ahead of it, an instrumentation seam row naming a distribution this floor never resolved, a partition op degrading to an unpartitioned single pull because its planning column is absent, an arm picking one of several bound frames off iteration order, a `Flight` redemption reading `endpoints[0]` alone or opening an unauthenticated client where the sibling remote axis carries a whole TLS and OAuth policy, a producer-partitioned fan drained one leg at a time or through a client the planning hop still holds, an `adbc_ingest` answering with its own input frame in place of the driver's reported row count, a mirror `sql`-or-`plan` optional pair whose XOR a factory rail and an unreachable `raise` re-derive, a credential-bearing DSN on a durable receipt, a receipt mint raising past its declared rail on a foreign-dialect statement, a bench lane re-executing a mutation `Remote` INGEST spec, timing a refusal it never gated, or rounding the gated `run` so every round re-pays the admission prologue the lane already spent; a mutable rebind chain shaping a daft plan where `_SHAPING` states the order as rows; a fence, retry class, raise set, or fault anchor spelled inside a dispatch arm where `_cell` owns the envelope; a bare `Exception` catch standing in for a frontend's own named provider roots; a free-string fence subject where every anchor is a rostered `FaultRow` proved against a real module at import; a lint suppression standing in for either — a total `match` over the closed `QuerySpec` family is the doctrine's own dispatch form, so a return-count rule reading it as sprawl is rule pressure the manifest pin answers, never a per-arm silence; a data-side `opentelemetry-instrumentation-dbapi` import where the runtime composition-root `wrap_connect` seam owns the connection-factory patch, a parallel per-engine profile field where `ProfileHarvest` folds every engine onto one `EngineProfile` band, and a data-side metric owner where the `BenchmarkReceipt`/`QueryReceipt` `contribute` projections own every measure.

```python signature
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

from rasm.data.tabular.columnar import DuckDbExtension, DuckDbSession, EngineProfile, ProfileHarvest, ProfileMode, QueryReceipt, predicate_count
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.admission import BackendGeneration
from rasm.runtime.faults import (
    FAULT_CONF,
    TERMINAL,
    TRANSIENT,
    Catch,
    Disposition,
    FaultRow,
    RuntimeRail,
    async_boundary,
    boundary,
    rostered,
    traversed,
)
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import DbapiSeam
from rasm.runtime.profiles import Bench, BenchMode, BenchmarkReceipt
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
# `ibis.parse_sql(text, catalog, dialect)` dereferences `catalog.items()`, so the schema is NOT optional beside the
# text: both ride ONE slot and an `IrEmit(parse=...)` carrying no catalog is unspellable rather than fatal.
type ParseSpec = tuple[str, Schema]
type IngestMode = Literal["append", "create", "replace", "create_append"]
# one blocking materialization the dispatch tail crosses its envelope with. A `FanPlan` answers the legs a
# producer-partitioned result already split into, beside the schema an empty fan reports its shape through.
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


# connect-and-execute dialect per driver row — the ONE closed discriminant every per-driver projection derives
# from, so a sixth driver names its kind rather than adding a boolean beside the row.
class DriverKind(StrEnum):
    MANAGER = "manager"  # driver-manager front end: the shared library is a caller `driver`/`entrypoint` coordinate
    FLIGHT = "flight"  # Flight SQL: the typed option enums key this row's db/conn/stmt projections alone
    NATIVE = "native"  # bundled driver resolving its own library, `connect(uri, db_kwargs=, conn_kwargs=)`
    LOCAL = "local"  # bundled driver whose `connect` admits the URI alone — a `db_kwargs=` keyword is a TypeError
    LOADER = "loader"  # read-parallel loader with no PEP-249 connection to open, wrap, or instrument


class RemoteRefusal(StrEnum):
    # every refused remote cell names its own reason and the member value IS the operator-facing evidence
    # `BoundaryFault` carries: `_REMOTE_REFUSAL` rows the unconditional cells, `_conditional` the
    # transport-dependent ones. An absent distribution answers the separate `_UNREACHED` floor gate under the
    # `import_` tag instead, because a missing provider is a provisioning fact, never a capability bound.
    CONNECTORX_READ_ONLY = "connectorx reaches no write-back; ingest rides an adbc driver"
    CONNECTORX_PARTITION_COLUMN = "connectorx partition planning needs transport.partition_on"
    # every non-Flight driver compiles the refusal into its own binary: postgres returns NOT_IMPLEMENTED from a
    # call-free stub, snowflake from a zero-call body carrying its own static message, sqlite from the shared
    # driver template. Exported-symbol presence proves nothing — each ships the flat-C wrapper, only the body decides.
    SQLITE_NO_PARTITION = "the sqlite driver answers adbc_execute_partitions NOT_IMPLEMENTED"
    POSTGRESQL_NO_PARTITION = "the postgresql driver answers adbc_execute_partitions NOT_IMPLEMENTED"
    SNOWFLAKE_NO_PARTITION = "the snowflake driver answers adbc_execute_partitions NOT_IMPLEMENTED"
    # the native ticket plane is this page's own client, so its bounds state here beside the driver cells: the
    # pyarrow client carries a bearer header and nothing that MINTS one, and the daft runner is process state a
    # per-request switch would re-point under every query already executing on it.
    FLIGHT_NATIVE_OAUTH = "the pyarrow flight plane runs no oauth flow; a token mints caller-side onto transport.authorization"
    STREAMING_RUNNER_FIXED = "the daft runner is a process-wide composition fact; this process resolved the other one"


class PlanRefusal(StrEnum):
    # an INBOUND plan is authored by `csharp:Rasm.Persistence/Query/federation` and arrives as opaque bytes, so its
    # admission is this branch's to state rather than the executor's to discover. Each member value IS the
    # operator-facing evidence, exactly as the remote and lake matrices state theirs.
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


# ops one cursor answers in one call. PARTITION is absent because its result is already SPLIT: its descriptors
# redeem on cursors of their own, so it plans a fan where these four each answer a table.
type CallOp = Literal[RemoteOp.READ, RemoteOp.STREAM, RemoteOp.INGEST, RemoteOp.PROBE]


class TimeoutPhase(StrEnum):
    FETCH = "fetch"
    QUERY = "query"
    UPDATE = "update"


class OAuthFlow(StrEnum):
    CLIENT_CREDENTIALS = "client_credentials"
    TOKEN_EXCHANGE = "token_exchange"


# closed mirror of the driver's `OAUTH_<KEY>` member roster: an open `str` key reflected onto the provider enum
# raises `AttributeError` inside the offloaded thread, where a member lookup refuses at the typed seam.
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


# daft read-scan FRONTEND axis keying `_DAFT_READ` — orthogonal to the `tabular/lakehouse#LAKEHOUSE` `TableFormat`
# transactional-WRITE axis. Both share DELTA/ICEBERG/LANCE, but this read set adds PARQUET/HUDI/SQL with no write
# owner, so a merge would pollute the write axis with read-only members carrying no commit arm — NOT collapsed.
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

# Flight SQL Go driver's embedded OTel tracer joins the caller's trace through this CONNECTION option — a key the
# Python option enums do not spell, verified against the shipped libadbc_driver_flightsql; exporter selection rides
# exporter selection off the standard OTEL_* env family a deployment sets, never a Python-side knob. Flight carries no
# embedded tracer, so it stitches the same parent as a gRPC call header instead — one read, two projections.
_TRACE_PARENT_KEY: Final[str] = "adbc.telemetry.trace_parent"

# default planning width for the ConnectorX partitioned read, which is the one arm whose fan WIDTH is a caller value:
# `partition_sql`/`read_sql` split one statement into that many subqueries. ADBC reads no width at all — the producer
# decides how many partitions its own result carries and `adbc_execute_partitions` takes no such argument — so the
# only caller-side ADBC lever is how many descriptors redeem CONCURRENTLY, which is the branch's own `THREAD_BAND`
# bound rather than a second width here. Spelled inline on a thirty-field policy `Struct` whose every other magnitude
# is a `None`-defaulted caller value, the number was the one policy this owner asserted rather than declared.
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
        # `text` is raw `read`-dialect SQL, so both qualify and lineage parse under `read`; tracing under `write` would mis-parse the source.
        outputs = tuple(sel.alias_or_name for sel in self._qualified(text, self.read).selects)
        roots = (sqlglot_lineage(name, text, schema=self.schema, dialect=self.read) for name in outputs)
        return tuple((str(leaf.name), str(node.name)) for node in roots for leaf in _leaves(node))


class IrEmit(Struct, frozen=True):
    backend_uri: str | None = None
    dialect: Dialects = Dialects.DUCKDB
    wire: PlanWire = PlanWire.SQL
    streaming: bool = False
    # substrait serializers gate optimize-before-serialize on `enable_optimizer`, so an unoptimized plan is a
    # policy value on the emit — a peer engine receiving a DuckDB-optimized plan reads DuckDB's rewrite decisions.
    optimize: bool = True
    parse: ParseSpec | None = None

    def bound(self, expr: IbisTable) -> IbisTable:
        return ibis.parse_sql(self.parse[0], catalog=self.parse[1], dialect=self.dialect.value) if self.parse is not None else expr

    def sql(self, expr: IbisTable) -> str:
        return str(ibis.to_sql(self.bound(expr), dialect=self.dialect.value))

    def edges(self, expr: IbisTable) -> tuple[LineageEdge, ...]:
        return SqlGate(read=self.dialect, write=self.dialect).edges(self.sql(expr))


class Transport(Struct, frozen=True):
    # remote OPERAND and TRANSPORT policy for one `Remote` spec. The typed block mirrors the Flight SQL option
    # enums; `db_options`/`conn_options`/`stmt_options` are the driver-native pass-through a caller keys with its
    # OWN driver enum value, because this page models one driver's vocabulary rather than five. That pass-through
    # WIDENS the surface and never narrows it: each projection seats the raw map first so a typed field wins the
    # key it owns, and a raw entry can add a knob nothing here models but can never unset one this spec resolved.
    partition_on: str | None = None
    partition_num: int = _PARTITION_FAN
    ingest_source: str | None = None
    ingest_mode: IngestMode = "create_append"
    ingest_catalog: str | None = None
    ingest_schema: str | None = None
    ingest_temporary: bool = False
    probe_catalog: str | None = None
    probe_schema: str | None = None
    # provenance parses the remote statement to count predicates and trace lineage; the DuckDB default mis-parses a
    # foreign statement, so the dialect the remote engine speaks is a caller value rather than a baked assumption.
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
        # `OAuthKey` mirrors the driver's own `OAUTH_<KEY>` roster, so the member lookup is total and no caller
        # string reaches `getattr` on the provider enum.
        oauth = {getattr(opts, f"OAUTH_{key.name}").value: value for key, value in self.oauth.items()}
        # the driver-native pass-through seats FIRST on every tier: it exists to reach knobs no typed field models,
        # so a raw key colliding with one this spec already declares is a caller mistake, never an override — seating
        # it last let a stray string silently unset TLS material, an authorization header, an oauth key, or a timeout
        # the typed projection had already resolved, and no signature broke to say so.
        return dict(self.db_options) | {key.value: value for key, value in rows if value is not None} | timeouts | headers | oauth

    def conn_kwargs(self, conn_opts: "type[ConnectionOptions]") -> dict[str, str]:
        session = {f"{conn_opts.OPTION_SESSION_OPTION_PREFIX.value}{key}": value for key, value in self.session_options.items()}
        return dict(self.conn_options) | session

    def stmt_kwargs(self, opts: "type[StatementOptions]") -> dict[str, str]:
        # STATEMENT-tier options key a cursor, never a connection: `adbc.rpc.result_queue_size` and the Substrait
        # pin apply per execution, so they ride `cursor(adbc_stmt_kwargs=)` and every op inherits them.
        queue = {opts.QUEUE_SIZE.value: str(self.queue_size)} if self.queue_size is not None else {}
        substrait = {opts.SUBSTRAIT_VERSION.value: self.substrait_version} if self.substrait_version is not None else {}
        return dict(self.stmt_options) | queue | substrait

    def client_kwargs(self) -> dict[str, Any]:
        # pyarrow's Flight client takes PEM OCTETS where the ADBC option enums take PEM TEXT, so the same caller
        # material serves both planes through one encode rather than a second credential axis on the spec.
        rows = (
            ("tls_root_certs", self.tls_root_certs),
            ("cert_chain", self.mtls_cert_chain),
            ("private_key", self.mtls_private_key),
        )
        pem = {name: value.encode() for name, value in rows if value is not None}
        return pem | {"override_hostname": self.tls_override_hostname, "disable_server_verification": self.tls_skip_verify or None}

    def call_options(self, plane: Any) -> Any:
        # gRPC call headers arrive as `(bytes, bytes)` pairs; the trace parent joins them so a ticket redemption
        # continues the caller's trace exactly as the Flight SQL driver's embedded tracer does off its own option.
        # The configured `authorization` joins them for the SAME reason the driver plane sets it through its own
        # option enum: one spec's credential must reach both planes, or a native redemption reaches the very server
        # every ADBC call authenticates to and arrives anonymous. The flow-minted half has no native equivalent, so
        # `_reach` refuses an `oauth_flow` on this plane by name rather than dropping it here.
        stitched = {"traceparent": parent} if (parent := _trace_parent()) else {}
        credential = {"authorization": self.authorization} if self.authorization is not None else {}
        headers = dict(self.rpc_headers) | credential | stitched
        return plane.FlightCallOptions(
            timeout=self.timeouts.get(TimeoutPhase.QUERY), headers=tuple((k.encode(), v.encode()) for k, v in headers.items())
        )


class Fan(Struct, frozen=True):
    # a producer-PARTITIONED result as one shape: `plan` runs once and answers the legs the producer already split
    # its result into, beside the schema an empty fan reports through. The two fanning frontends — the ADBC partition
    # descriptors and the Flight endpoint set — differ only in what a leg opens, so the concurrency, the
    # envelope-per-leg, and the order-preserving concat state ONCE here rather than twice inside two serial
    # comprehensions that named a fan and drained it one at a time. A fan drained serially spends the producer's
    # partitioning round trip and collects nothing for it, which is strictly slower than the unpartitioned read.
    # The two PHASE anchors ride the fan rather than every frontend's cell: a fan is what has a plan hop and a
    # redemption at all, so generating a `.plan`/`.redeem` pair per frontend seated twenty coordinates no site could
    # ever raise. Only a site that builds a fan can spell them, which is what makes the roster's reach exact.
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
    # cold analytics residences keep their partition columns in path segments, so a scan over one reads them back
    # only under this flag; the `_DAFT_READ` row decides whether the selected reader carries the keyword at all.
    hive_partitioning: bool = False
    disable_pushdowns: bool = False
    version: str | int | None = None
    asof: str | None = None
    block_size: int | None = None
    repartition: int | None = None
    repartition_by: tuple[str, ...] = ()
    io_config: Any = None


class FederatedStore(Struct, frozen=True):
    # one federated object-store registration. The residence is the runtime `ResourceRef` — scheme, root, and the
    # credential provider that root binds ride ONE coordinate — so datafusion's `(schema, host)` registration key and
    # the `store_handle` carry both DERIVE from it and no field here re-spells a residence fact the ref already
    # holds. A bare `obstore.store.from_url` here re-minted the provider's own defaults — no branch retry envelope,
    # no client options, and no credential provider at all — so a federated scan over a private or requester-pays
    # residence arrived anonymous and re-dialed under a policy no page states, while every other remote read in this
    # branch (`columnar`'s `remote_store`, `egress`'s lane, `catalog`'s asset fold, `gridded`'s registry) crosses the
    # same fold. The three obstore construction knobs stay because they are per-CALL provider policy the ref does not
    # carry; the credential is NOT among them, because a store credentialed apart from its residence is two
    # resolutions the one store memo key cannot serve.
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
    # one listing table a plan names over the registered bytes: store registration federates BYTES and names no
    # relation, so a plan over the cold analytics residence resolves nothing until this row lands. `partition_cols`
    # is DECLARED `(column, arrow_type)` rather than sniffed, because a hive path segment carries no type of its own.
    name: str
    path: str
    partition_cols: tuple[tuple[str, str], ...] = ()


@tagged_union(frozen=True)
class Federation:
    # `Federation` carries the ONE `⇄` seam direction structurally: `mint` serializes local SQL outbound, `execute` runs the
    # Persistence-authored bytes inbound. A `(sql | None, plan | None)` pair states the same discriminant three
    # times over — a runtime XOR gate, a factory rail its seven siblings do not return, and an unreachable raise.
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
    # `(direction, stores, tables)`: registering an object store federates its BYTES and names no relation, so a plan
    # over the cold analytics residence needs its listing tables registered too — each `FederatedTable` row naming the
    # hive columns datafusion reads back out of the path rather than the file.
    federated: tuple[Federation, tuple[FederatedStore, ...], tuple[FederatedTable, ...]] = case()
    # csharp:Rasm.Persistence/Query/federation FLIGHT_RESULT_PLANE consumer: SubstraitPlan command bytes submit
    # through GetFlightInfo, EVERY returned FlightEndpoint redeems through DoGet — the producer plans and holds,
    # this side only executes the ticket round-trip; `federated` stays the in-process datafusion executor.
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
    # `session` carries the in-process DuckDB plane's POLICY as caller data — extensions, attached catalogs, and the
    # resolved filesystem handle — never a connection: `DuckDbSession` is a frozen row whose bracket still opens per
    # dispatch under the request-scoped law. Absent it, `Sql`/`Rel` open a bare session, so an evidence-residence read
    # cannot load `delta` and the `postgres_scanner` attach making `[TENANT_COST_JOIN]` one statement has nowhere to be
    # spelled — callers would need a second transport for a join DuckDB already folds.
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
    ) -> "RuntimeRail[BenchmarkReceipt]":
        # query bench lane: one repeated `QuerySpec` timed across engines under the runtime `Bench.run` subjects, the
        # `BenchmarkReceipt` projecting through the standing `domain="bench"` rows at its OWN `contribute` — this page adds
        # zero instrument state. The SAME prologue `run` reads gates the lane, so an unreached provider or a refused
        # `(driver, op)` cell answers once instead of being timed N times as a microsecond "success". Mutation specs never
        # ride the lane: a `Remote` INGEST re-executes an unknowable partial append, so it is a typed refusal, never a
        # benchmarked round. Each round drives the awaitable `run` to completion on a fresh loop through `anyio.run`; a
        # process-terminal caller places this method inside `JobRun.bounded`.
        # `Bench.run` rails its own window — a refused round count and a window measuring nothing arrive typed — so the
        # lane BINDS rather than maps; a map here would nest one rail inside the other the signature already declares.
        # The round body is `_dispatch`, the total-over-admitted-specs entry the `[02]` Law already declares, never
        # `run`: `run`'s first act is the very `_reach`-then-`_bound` prologue this lane just spent, so rounding it
        # re-pays that prologue on every round and warmup — a full `Plan().ParseFromString` parse, an extension-urn
        # resolution loop, and a schema inference on the two plan frontends — folding admission work into both the
        # latency reading and the throughput count as if it were the query.
        return _reach(spec).bind(self._bound).bind(self._benched).bind(
            lambda admitted: Bench.run(
                f"query.{admitted.tag}",
                lambda: anyio.run(self._dispatch, admitted),
                mode=mode or _BENCH_MODE.get(admitted.tag) or BenchMode.LATENCY,
                rounds=rounds,
                warmup=warmup,
            )
        )

    def _benched(self, spec: QuerySpec) -> "RuntimeRail[QuerySpec]":
        match spec:
            case QuerySpec(tag="remote", remote=(_sql, _dsn, _driver, RemoteOp.INGEST, _transport)):
                return Error(BENCH_MUTATION.raised())
            case QuerySpec():
                return Ok(spec)

    @beartype(conf=FAULT_CONF)
    async def run(self, spec: QuerySpec) -> "RuntimeRail[pa.Table]":
        # prologue: `_reach` answers every cell no provider on this interpreter floor serves and `_bound` resolves
        # each frame-naming arm's input slot, so `_dispatch` below is total over admitted specs — no arm re-probes
        # a driver, degrades an op, or picks one of several bound frames off iteration order.
        match _reach(spec).bind(self._bound):
            case Result(tag="error", error=refused):
                return Error(refused)
            case Result(tag="ok", ok=admitted):
                return await self._dispatch(admitted)
            case unreachable:
                assert_never(unreachable)

    def _bound(self, spec: QuerySpec) -> "RuntimeRail[QuerySpec]":
        # admission NORMALIZES the frame slot rather than gating it, so the interior lookup is a total `inputs[name]`.
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

    def _named(self, frame: str | None, point: str) -> "RuntimeRail[str]":
        match frame, tuple(self.inputs):
            case str() as named, bound if named in bound:
                return Ok(named)
            case None, (sole,):
                return Ok(sole)
            case None, _:
                return Error(FRAME_UNRESOLVED.raised(point, _UNNAMED))
            case named, _:
                return Error(FRAME_UNRESOLVED.raised(point, str(named)))

    async def _dispatch(self, spec: QuerySpec) -> "RuntimeRail[pa.Table]":
        # ONE envelope pair over the whole axis. `_body` destructures the spec's own operands and answers the plan —
        # one blocking leg, or the `Fan` a producer-partitioned result already is — and this tail alone decides which
        # fence a leg crosses, reading the subject and the retry class off `_cell`. Arms each re-spelling their own
        # `async_boundary`/`guarded` call put the retry law once per frontend, so a new frontend inherited whichever
        # neighbour it was copied from and the mutation exclusion read as one arm's comment rather than the axis's rule.
        cell, peer = _cell(spec), _peer(spec)
        match self._body(spec):
            case Fan() as fan:
                return await self._fanned(fan, cell, peer)
            case leg:
                return await self._crossed(leg, cell.leg, cell, peer)

    async def _crossed[T](self, leg: Callable[[], T], at: "FaultRow[DataLeg]", cell: "_Cell", peer: "Option[str]") -> "RuntimeRail[T]":
        # ONE envelope carries an unfanned body and every fan leg alike, generic over what the leg materializes so
        # this fan's own plan hop rides it too rather than opening a second spelling of the same fence. `at` is the
        # PHASE anchor — leg, plan, or redeem — and `catch` derives from the cell's own thunk, so the bare `Exception`
        # default this replaces (justified on the page as the only cover for four disjoint provider roots) becomes the
        # named union each frontend actually reaches, reified only where that frontend runs.
        # TWO coordinates ride, answering different questions: `at` is the phase anchor naming WHICH CALL raised, and
        # `on` names WHICH PEER it reached — the breaker arc and the rate bucket key on that alone, so one rostered
        # row serving every destination this axis dials would otherwise fuse two peers into one window and shed every
        # healthy caller of the peer that never went down. `REMOTE_DB` carries a `CIRCUIT` row, so `_keyed` refuses
        # `config` on an unstated peer there; `STREAMING` carries neither stateful row today and its peer is KEPT
        # rather than refused, so the day a row lands the window is already correctly keyed.
        return await cell.retry.map(
            lambda retry: guarded(retry, on_thread, leg, abandon=True, at=at, on=peer)
        ).default_with(lambda: async_boundary(at, lambda: on_thread(leg), catch=cell.raises()))

    async def _fanned(self, fan: Fan, cell: "_Cell", peer: "Option[str]") -> "RuntimeRail[pa.Table]":
        # PLAN hop and every REDEEM leg cross the envelope SEPARATELY, so a transport transient on one partition
        # retries that partition rather than re-planning a fan the producer already materialized, and a permanently
        # refused partition names its own coordinate. The legs run CONCURRENTLY under the one `THREAD_BAND` bound
        # `on_thread` already takes — the fan buys concurrency and never a second bound, exactly as the sibling
        # `runtime/transport/roots#RESOURCE` batch does — because a fan is what a producer splits precisely so its
        # consumer reads the halves at once, and draining it leg after leg pays the partitioning round trip for
        # nothing. Order is the PRODUCER's: the concat reads the legs back in descriptor order whatever order they
        # completed in, so `FlightInfo.ordered` decides nothing here and the result is stable across two runs.
        match await self._crossed(fan.plan, fan.plan_at, cell, peer):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(legs, schema)):
                async with anyio.create_task_group() as group:
                    handles = legs.map(lambda leg: group.start_soon(partial(self._crossed, leg, fan.redeem_at, cell, peer)))
                # ABORT and not ACCUMULATE: a fan is ONE result, so a partial set is no table a caller can read —
                # every leg still runs to completion, since the rails are already settled when the fold reads them.
                return traversed(handles.map(lambda held: held.return_value), by=Disposition.ABORT).map(
                    lambda tables: pa.concat_tables(tables) if tables else schema.empty_table()
                )
            case unreachable:
                assert_never(unreachable)

    def _body(self, spec: QuerySpec) -> "Leg | Fan":
        # operand destructuring alone, one arm per `QuerySpec` case under `assert_never` — the closed-family dispatch
        # the doctrine names, carrying no fence, no retry class, and no subject inside an arm. The two remote arms
        # fused here because they differed by envelope alone, which `_cell` now owns; reach already refused every cell
        # this driver cannot serve on this floor, so the caller's selection rides through verbatim and no arm
        # second-guesses it with a silent swap. Planning is PURE — the fanning arms resolve a row and close over
        # operands, opening no handle and reaching no network until the dispatch tail crosses their legs' envelope.
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
        # `tabular/columnar#SCAN` profiled bracket over the caller's OWN session policy, released once `to_arrow_table`
        # has materialized the relation inside it. `profiling` overrides the row's slot rather than living twice: the
        # daft and datafusion arms read the same axis and neither carries a DuckDB session to hold it.
        with replace(self.session, profiling=self.profiling).profiled() as (con, harvest):
            for name, frame in self.inputs.items():
                con.register(name, frame)
            table = build(con).to_arrow_table()
            return harvest(table)

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

# bench-mode default per `QuerySpec` tag — scan-bound frontends (streaming/remote/federated/flight) count rounds over the
# window, point frontends (sql/rel/agnostic/ir) time each round; a caller override on `bench(mode=)` wins over the row.
# retry class per `QuerySpec` tag; an ABSENT tag is the unretried envelope. The in-process arms cross unretried
# because `duckdb.Error`, `ibis.IbisError`, narwhals, and pyarrow are disjoint exception roots no transient predicate
# spans, so a narrowed catch would let one arm's taxonomy escape the fence while a broad retry would replay a
# deterministic failure. `_cell` reads this table beside its one operand-conditional override.
# --- [ERRORS] ---------------------------------------------------------------------------


def _duck_raises() -> Catch:
    # every in-process DuckDB leg: `duckdb.Error` is the DB-API base the whole engine hierarchy descends from
    # (`.api/duckdb.md:30`), and the Arrow materialization under it carries the core rail beside `OSError`.
    return (duckdb.Error, pa.ArrowException, OSError)


def _agnostic_raises() -> Catch:
    # `NarwhalsError` roots the whole narwhals rail including `ColumnNotFoundError` (`.api/narwhals.md` failure rows);
    # `from_native` handed a non-frame refuses as a bare `TypeError` outside that root.
    return (NarwhalsError, TypeError, pa.ArrowException, OSError)


def _ir_raises() -> Catch:
    # the ibis leg drives a real backend connection, so its own root sits beside the engine the emit binds.
    return (ibis.common.exceptions.IbisError, duckdb.Error, pa.ArrowException, OSError)


def _remote_raises() -> Catch:
    # `adbc_driver_manager.Error` is the DB-API base every admitted driver's exceptions descend from. ConnectorX
    # publishes NO exception namespace of its own — a Rust fault surfaces as a builtin `RuntimeError`/`ValueError`
    # (its distribution sits below the interpreter floor here, so the shape is taken from its documented rail and
    # the floor gate refuses the frontend before any of it runs) — so both roots are named.
    return (adbc_manager.Error, RuntimeError, ValueError, pa.ArrowException, OSError)


def _streaming_raises() -> Catch:
    # `DaftCoreException` roots every daft fault and subclasses `ValueError` (`.api/daft.md:46`); its transient
    # branch is the retry discriminant `RetryClass.STREAMING` already targets, so this fence catches the whole tree.
    return (daft.exceptions.DaftCoreException, pa.ArrowException, OSError)


def _federated_raises() -> Catch:
    # datafusion publishes no exception namespace at all — a plan or execution fault surfaces as a bare `ValueError`
    # off its Rust core (probed against the installed distribution) — and the plan wire decodes through protobuf.
    return (ValueError, protobuf_message.DecodeError, pa.ArrowException, OSError)


def _flight_raises() -> Catch:
    # `flight.FlightError` roots the eight wire leaves (`.api/pyarrow.md` flight rows) and derives from `Exception`
    # directly, never from `ArrowException`, so the core rail is named beside it for the concat and schema legs.
    return (flight.FlightError, pa.ArrowException, OSError)


# one declaration per admitted frontend, its raise set a THUNK so a `lazy`-bound distribution reifies only where its
# own frontend runs. The tags are the `QuerySpec` case names, which is what makes `_CELLS` total over the axis.
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
    # the whole envelope for one frontend as DATA: three fault anchors, the retry class, and the provider raise set.
    # The two fan PHASE anchors ride the `Fan` itself: a fanning frontend crosses the envelope twice and each phase
    # names its own coordinate, but only two of these twelve cells ever fan, so generating a pair per cell seats
    # twenty coordinates no site can raise.
    leg: FaultRow[DataLeg]
    retry: "Option[RetryClass]"
    raises: Callable[[], Catch]


def _celled(point: str, retry: "Option[RetryClass]", raises: Callable[[], Catch]) -> _Cell:
    # ONE generator per frontend: the leg anchor DERIVES off the frontend's own point, so a new frontend lands its
    # anchor by joining `_FRONTEND_RAISES`. A frontend nothing re-offers declares TERMINAL. The two FAN phases are
    # deliberately not generated here — a fan carries them, because only two of the twelve cells ever answer one.
    posture = TRANSIENT if retry.is_some() else TERMINAL
    return _Cell(
        leg=FaultRow(leg=DataLeg.TABULAR_QUERY, point=point, arm="boundary", defect="frontend-leg", retriability=posture),
        retry=retry,
        raises=raises,
    )


# the refusals this page raises OUTSIDE a fence, each a parameterized row taking its coordinates rather than one row
# per site: `QUERY_FLOOR` names the frontend and the module the interpreter cannot resolve, `QUERY_UNREACHED` the
# frontend and the typed `RemoteRefusal`/`PlanRefusal` value the matrix answered.
QUERY_FLOOR: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="reach.floor", arm="import_", defect="distribution-absent", retriability=TERMINAL, slots=("frontend", "module")
)
QUERY_UNREACHED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="reach.cell", arm="boundary", defect="cell-refused", retriability=TERMINAL, slots=("frontend", "reason")
)
# ONE parameterized row, not two: both failures are the SAME resolution law — this spec names no frame the engine
# holds — and a caller repairs either identically, by naming a bound one. The discriminant is recoverable from the
# VALUE rather than from a second row: `frame` carries `unnamed` where several inputs are bound and none was named,
# and the requested spelling where one was named and no input carries it.
FRAME_UNRESOLVED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="bind", arm="config", defect="frame-unresolved", retriability=TERMINAL, slots=("frontend", "frame")
)
BENCH_MUTATION: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.TABULAR_QUERY, point="bench", arm="config", defect="mutation-spec-excluded", retriability=TERMINAL
)
# the coordinate an ambiguous bind reports in place of a name nobody supplied; a bound frame can never spell it,
# because `_bound` normalizes the slot to a real input name before the interior ever reads it.
_UNNAMED: Final[str] = "unnamed"

# the two fanning frontends' phase anchors, hand-rostered because exactly two sites build a `Fan`: the Flight
# endpoint set and the ADBC partition descriptors. A transport transient on one partition retries THAT partition
# under its own coordinate rather than re-planning a fan the producer already materialized.
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

# every frontend's cell GENERATED off one declaration: the three fault anchors a frontend can raise under — its whole
# leg, its plan hop, and one redeemed partition — plus the retry class and the PROVIDER raise set that frontend's
# thunk reaches. The set rides a thunk rather than a tuple so a `lazy`-bound distribution reifies only when its own
# frontend runs: naming `daft`'s root in a module-scope tuple would load the whole out-of-core runner for a DuckDB
# query. A frontend carrying no `_ENVELOPE` class declares TERMINAL — nothing here re-offers it — and the two fan
# phases inherit their leg's posture, since a partition and its plan fail for one reason.
_CELLS: Final[Map[str, _Cell]] = Map.of_seq(
    (tag, _celled(tag, _ENVELOPE.try_find(tag), raises)) for tag, raises in _FRONTEND_RAISES.items()
)

# the remote sub-axis carries its OP in the coordinate exactly as `_reach` does, so a fault and the refusal it would
# have replaced name one point rather than a bare `query.remote` covering five operations. The rows GENERATE off the
# closed `RemoteOp` roster, so a sixth operation lands its whole anchor set by being a member.
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
    # WHICH PEER this spec reaches, at the grain a breaker arc and a rate bucket are written at: an endpoint that can
    # go DOWN or pace, never the frontend that dialed it. Every url- or DSN-shaped destination crosses the branch's
    # ONE `roots` `origin` fold, which keys on `scheme://host:port` off `hostname`/`port` and never the raw netloc:
    # a raw DSN publishes its password onto the `rasm.peer` span attribute, and a path-bearing coordinate splits one
    # origin's window across every database it reads so no arc ever reaches its trip. `_endpoint` stays the RECEIPT's
    # provenance spelling — it keeps the path a reader needs to tell two databases apart — and the two never merge:
    # provenance wants the finer coordinate and a failure window wants the coarser one. The streaming leg answers its
    # cluster, else the process-wide runner it was pinned to. An in-process frontend reaches no peer and carries no
    # stateful class, so `Nothing` is its key.
    match spec:
        case QuerySpec(tag="remote", remote=(_sql, dsn, _driver, _op, _transport)) | QuerySpec(tag="flight", flight=(_wire, dsn, _transport)):
            return Some(origin(dsn))
        case QuerySpec(tag="streaming", streaming=(_plan, runner, cluster)):
            return Some(cluster or runner.value)
        case QuerySpec():
            return Nothing


def _cell(spec: QuerySpec) -> "_Cell":
    # fault anchors, retry class, and provider raise set for one ADMITTED spec. The mutation cell alone carries no
    # retry class: an ingest timeout leaves an unknowable partial append, so a blind `REMOTE_DB` replay duplicates
    # rows where the caller re-issues under its own dedupe, and the unretried envelope never abandons its band slot
    # either, so a deadline-tripped mutation stays observed to completion.
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

# serialize/execute table-function pair per plan wire. `substrait` attaches SQL TABLE FUNCTIONS alone — no
# connection-bound method, no extension-owned class — so each half is a `CALL` through `con.execute`: SUBSTRAIT
# carries the protobuf `Plan` BLOB `datafusion`'s `Consumer` ingests unchanged, SUBSTRAIT_JSON its inspectable
# VARCHAR twin over the identical logical plan.
_SUBSTRAIT: Final[Map[PlanWire, tuple[str, str]]] = Map.of_seq([
    (PlanWire.SUBSTRAIT, ("get_substrait", "from_substrait")),
    (PlanWire.SUBSTRAIT_JSON, ("get_substrait_json", "from_substrait_json")),
])

# this registry holds the process-wide function vocabulary an inbound plan resolves against, default-loaded with the
# standard extension set. An estate function set beyond that registers here ONCE through `register_extension_yaml`,
# so a plan naming it admits everywhere rather than at whichever call site remembered to widen its own copy.
_REGISTRY: Final[ExtensionRegistry] = ExtensionRegistry(load_default_extensions=True)


def _plan_refusal(wire: bytes) -> "Option[PlanRefusal]":
    # `_reach` reads this ONE inbound-plan gate ahead of the executor: the plan's own protobuf model parses the
    # bytes, the registry resolves every extension urn the plan names, and schema inference proves the result shape
    # is knowable before a row is read. Letting `Serde.deserialize_bytes` be the admission authority hands a caller
    # a Rust fault wearing a transport failure's clothes, carrying no reason a matrix row states as data — the
    # AUTHORITY inversion the reach discipline forecloses everywhere else on this page. Inference raises a bare
    # `Exception` for every unhandled rel and type shape alike, so the catch is as wide as the surface it fences.
    plan = substrait_proto.Plan()
    try:
        plan.ParseFromString(wire)
    except DecodeError:
        return Some(PlanRefusal.UNPARSEABLE)
    # a plan carries `PlanRel` entries and exactly the ROOT ones name an output an executor can return; an entry
    # seated on `rel` alone is an anonymous sub-plan with no entry point. One predicate answers both the empty plan
    # and the root-less one, and it answers them HERE rather than through the inference row below, which does refuse
    # a root-less plan but names a schema that would not infer where the fact is a missing entry point.
    if not any(relation.HasField("root") for relation in plan.relations):
        return Some(PlanRefusal.NO_RELATION)
    # SCHEMA SKEW, not an absence: substrait moved the extension space off `extension_uris` (field 1, `SimpleExtensionURI`)
    # onto `extension_urns` (field 8, `SimpleExtensionURN`) and moved each declaration's back-reference off
    # `extension_uri_reference` (field 1) onto `extension_urn_reference` (field 4). proto3 files a retired field into the
    # unknown set rather than raising, so a producer still minting the URI-era schema parses clean, reads `extension_urns`
    # EMPTY, and reads every declaration's reference as the 0 default. Treating that as an extension-free plan lets the
    # resolution check below pass VACUOUSLY and lands zero provenance edges — a false admission wearing a clean gate's
    # clothes. Declarations without declared spaces is the exact signature, so it refuses on its own reason here.
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
    # ibis backends own the connection; the extension load names WHAT it needs through the
    # `tabular/columnar#SCAN` `DuckDbExtension.SUBSTRAIT` row (community repository a row property), never HOW.
    # Only the function name interpolates, off the closed `_SUBSTRAIT` row — the emitted SQL, the optimizer
    # gate, and the plan payload all bind as parameters, so no caller string reaches either statement.
    # DuckDB's OWN parser reads the serializer argument, so this leg pins `Dialects.DUCKDB` whatever
    # `emit.dialect` says: substrait carries the plan dialect-portable by construction, and `emit.dialect`
    # governs the SQL wire and the lineage trace alone — a foreign dialect mis-parses before a plan is minted.
    con = backend.con
    DuckDbExtension.SUBSTRAIT.load(con)
    serialize, execute = _SUBSTRAIT[emit.wire]
    plan = con.execute(
        f"CALL {serialize}(?, enable_optimizer => ?)", [str(ibis.to_sql(expr, dialect=Dialects.DUCKDB.value)), emit.optimize]
    ).fetchone()[0]
    # `streaming` threads the same axis the SQL wire threads, so a plan wire never silently drops the caller's
    # incremental request; `to_arrow_reader`/`to_arrow_table` are the live spellings the deprecated
    # `fetch_record_batch`/`fetch_arrow_table` twins name.
    executed = con.execute(f"CALL {execute}(?)", [plan])
    return executed.to_arrow_reader().read_all() if emit.streaming else executed.to_arrow_table()


# --- [REMOTE]


@contextmanager
def _opened(row: "_Driver", dsn: str, transport: Transport) -> "Iterator[tuple[adbc_dbapi.Connection, adbc_dbapi.Cursor]]":
    # ONE connect-and-cursor bracket serves every DBAPI leg, so the kind-keyed connect projection and the single
    # `adbc_stmt_kwargs` open are spelled once rather than at each of the call, plan, and redeem sites.
    with row.ns().connect(**_connect_kwargs(row.kind, transport, dsn)) as conn, conn.cursor(adbc_stmt_kwargs=_stmt_kwargs(row.kind, transport)) as cur:
        yield conn, cur


def _partition_plan(row: "_Driver", sql: str, dsn: str, transport: Transport) -> "tuple[Block[Leg], pa.Schema]":
    # PLAN half: one connection collects the opaque descriptors and CLOSES. A descriptor is self-contained — a
    # distributed driver fans them across workers, each redeeming on handles of its own — so a leg carries the token
    # and opens its own bracket rather than holding this one across the fold. The schema rides out beside the legs
    # because an empty descriptor set is an honest empty result whose shape only the plan knows.
    with _opened(row, dsn, transport) as (_conn, cur):
        descriptors, schema = cur.adbc_execute_partitions(sql)
    return Block.of_seq(descriptors).map(lambda token: _partition_leg(row, dsn, transport, token)), schema


def _partition_leg(row: "_Driver", dsn: str, transport: Transport, token: bytes) -> Leg:
    # ONE descriptor on handles no sibling shares. `adbc_read_partition` RETURNS NONE and rebinds the CALLING cursor,
    # clearing whatever result it held, so a second descriptor on one cursor cancels the first read and the whole fan
    # is forced serial there; this DBAPI further declares `threadsafety=1`, under which threads share the module and
    # never a connection, so a concurrently-redeeming leg owns both. The bracket carries the same kind-keyed statement
    # options the plan hop took, so a queue size or timeout reaches every redemption rather than the planning call alone.
    def redeemed() -> pa.Table:
        with _opened(row, dsn, transport) as (_conn, cur):
            cur.adbc_read_partition(token)
            return cur.fetch_arrow_table()

    return redeemed


def _dbapi_op(conn: "adbc_dbapi.Connection", cur: "adbc_dbapi.Cursor", sql: str, op: CallOp, transport: Transport, frames: Frames) -> pa.Table:
    # `Remote`'s first slot carries the op's SUBJECT, not always a statement: READ/STREAM/PARTITION bind it as
    # SQL text, INGEST and PROBE bind it as the target table name the driver resolves through its own catalog.
    match op:
        case RemoteOp.READ:
            cur.execute(sql)
            return cur.fetch_arrow_table()
        case RemoteOp.STREAM:
            cur.execute(sql)
            return cur.fetch_record_batch().read_all()
        case RemoteOp.INGEST:
            # `adbc_ingest` ANSWERS the driver's own inserted-row count (`-1` where the driver cannot report it),
            # and that count is the operation's evidence — echoing the input frame back reports the payload the
            # caller already holds and keys the receipt on bytes the remote never acknowledged.
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
    # `propagate.inject` writes `traceparent` into the carrier only under a RECORDING span, so an unsampled
    # or span-less call stitches nothing and the driver traces under its own root.
    carrier: dict[str, str] = {}
    propagate.inject(carrier)
    return carrier.get("traceparent")


def _connect_kwargs(kind: DriverKind, transport: Transport, dsn: str) -> dict[str, Any]:
    # per-kind connect projection, LIVE-shaped: the manager takes the shared-library coordinate beside the URI, a
    # bundled native driver takes the two kwarg maps, and the local driver's `connect(uri, **kwargs)` forwards
    # every extra keyword into `Connection.__init__` — so a `db_kwargs=` there is a TypeError, not a no-op.
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
            # an embedded Go tracer reads the trace parent off a CONNECTION option the Python enums do
            # not spell, so the stitched row joins `conn_kwargs` and a span-less call adds nothing.
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
    # typed statement enums belong to the Flight SQL package alone; every other driver takes the caller's own
    # driver-native rows, so no leg dereferences a distribution the floor gate never proved present.
    rows = transport.stmt_kwargs(StatementOptions) if kind is DriverKind.FLIGHT else dict(transport.stmt_options)
    return rows or None


def _dbapi(row: "_Driver", sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> "Leg | Fan":
    # ONE PEP-249 planner over every admitted driver, answering the shape its OP is rather than a table: PARTITION
    # alone carries a result the producer already SPLIT, so it answers the fan its descriptors are, and every other
    # op is one call inside one bracket. The row's kind decides both projections at whichever bracket opens.
    if op is RemoteOp.PARTITION:
        return Fan(plan=lambda: _partition_plan(row, sql, dsn, transport), plan_at=PARTITION_PLAN, redeem_at=PARTITION_REDEEM)

    def called() -> pa.Table:
        with _opened(row, dsn, transport) as (conn, cur):
            return _dbapi_op(conn, cur, sql, op, transport, frames)

    return called


def _connectorx(row: "_Driver", sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> "Leg | Fan":
    # LOADER fans INSIDE `read_sql`, which drives its own subquery set concurrently off `partition_on`/
    # `partition_num`, so its PARTITION cell answers ONE leg where the DBAPI kinds answer a fan — nesting a second
    # fold over a provider that already parallelized multiplies the connection count against one bound.
    def called() -> pa.Table:
        ns = row.ns()
        if op is RemoteOp.PROBE:
            return pa.Table.from_pandas(ns.get_meta(dsn, sql, protocol=transport.protocol))
        # reach proved `partition_on` present on the PARTITION cell, so the planner emits explicit per-partition
        # subqueries there and every other op rides the one statement under `read_sql`'s own partition carry.
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
    # `ns` stays a CALL-TIME thunk: a table row spelling `adbc_dbapi.connect` dereferences the lazy proxy at
    # module import, reifying every driver whether a spec names it or not and defeating the floor gate the
    # `_PROVIDER_MODULE` roster derives from this same table.
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
    # PLAN half of the FederationFlight round trip: the plan bytes ride a command `FlightDescriptor` into
    # `GetFlightInfo`, which answers the endpoint set the producer partitioned its result into beside that result's
    # schema. Redeeming `endpoints[0]` alone answers a silent fraction and redeeming the set one endpoint after the
    # next spends the planning round trip to buy nothing, so each endpoint leaves here as its own leg. The planning
    # client closes with this hop: a leg dials its own, since a `FlightClient` declares no concurrent-use contract
    # and the located arm already opened one per peer.
    options = transport.call_options(flight)
    with _flight_client(dsn, transport) as client:
        info = client.get_flight_info(flight.FlightDescriptor.for_command(plan), options)
        endpoints, schema = tuple(info.endpoints), info.schema
    return Block.of_seq(endpoints).map(lambda endpoint: _flight_leg(_located(endpoint, dsn), endpoint.ticket, transport)), schema


def _flight_client(location: str, transport: Transport) -> Any:
    return flight.FlightClient(location, **transport.client_kwargs())


def _located(endpoint: Any, dsn: str) -> str:
    # EMPTY locations mean "this server", so the leg dials the spec's own coordinate while a located endpoint names
    # whichever peer serves it. ONE projection off a fact the endpoint already carries, so the fan holds no two-arm
    # split and every leg reaches its peer through a bracket carrying the SAME caller TLS material.
    locations = tuple(endpoint.locations)
    return locations[0].uri.decode() if locations else dsn


def _flight_leg(location: str, ticket: Any, transport: Transport) -> Leg:
    def redeemed() -> pa.Table:
        with _flight_client(location, transport) as peer:
            return peer.do_get(ticket, transport.call_options(flight)).read_all()

    return redeemed


# --- [REACH]

# module each LAZILY-BOUND provider imports, keyed by the reach coordinate selecting it: a `RemoteDriver` value
# on the remote sub-axis, a `QuerySpec` tag where one frontend's whole arm rides one distribution. The driver
# half DERIVES from `_DRIVER`, so a new driver row lands its floor coordinate with no second edit, and the tail
# carries one row per remaining `lazy` name — gating the driver sub-axis alone would leave
# `streaming`/`federated`/`flight` open the moment a marker lands on theirs. `(_PROVIDER_MODULE, _UNREACHED)`
# pairs into the interpreter-floor gate: a manifest marker holding a distribution below the running floor leaves
# its module unresolvable, so each row derives ONCE at import through `find_spec` and every spec naming that
# provider refuses typed at reach — never a per-call probe, and never a `ModuleNotFoundError` surfacing from
# inside the offloaded thread the lazy proxy binds in.
_PROVIDER_MODULE: Final[Map[str, str]] = Map.of_seq([
    *((driver.value, row.module) for driver, row in _DRIVER.items()),
    ("streaming", "daft"),
    ("federated", "datafusion"),
    ("flight", "pyarrow.flight"),
])

_UNREACHED: Final[Map[str, str]] = Map.of_seq(
    (coordinate, module) for coordinate, module in _PROVIDER_MODULE.items() if find_spec(module) is None
)


def _floor(coordinate: str, point: str) -> "Option[RuntimeRail[QuerySpec]]":
    # one floor read serving both `_reach` arms, so the remote sub-axis and a whole-arm frontend answer the
    # absent distribution through one row shape rather than two spellings of the same lookup.
    return _UNREACHED.try_find(coordinate).map(lambda module: Error(QUERY_FLOOR.raised(point, module)))

# `(driver, op)` reach matrix: an absent cell is reachable and carries an executing arm, a present cell refuses
# with its own row reason. The matrix outranks the arms, so a fourth driver lands its unreachable cells as rows.
_REMOTE_REFUSAL: Final[Map[tuple[RemoteDriver, RemoteOp], RemoteRefusal]] = Map.of_seq([
    ((RemoteDriver.CONNECTORX, RemoteOp.INGEST), RemoteRefusal.CONNECTORX_READ_ONLY),
    ((RemoteDriver.SQLITE, RemoteOp.PARTITION), RemoteRefusal.SQLITE_NO_PARTITION),
    ((RemoteDriver.POSTGRESQL, RemoteOp.PARTITION), RemoteRefusal.POSTGRESQL_NO_PARTITION),
    ((RemoteDriver.SNOWFLAKE, RemoteOp.PARTITION), RemoteRefusal.SNOWFLAKE_NO_PARTITION),
])


def _conditional(driver: RemoteDriver, op: RemoteOp, transport: Transport) -> "Option[RemoteRefusal]":
    # operand-conditional cell: reach turns on the transport payload a `(driver, op)` key cannot see, so the row
    # still answers a typed reason instead of collapsing a partition request onto one unpartitioned serial pull.
    return (
        Some(RemoteRefusal.CONNECTORX_PARTITION_COLUMN)
        if driver is RemoteDriver.CONNECTORX and op is RemoteOp.PARTITION and transport.partition_on is None
        else Nothing
    )


def _reach(spec: QuerySpec) -> "RuntimeRail[QuerySpec]":
    # one matrix read per spec decides reachability: the floor row answers a provider whose distribution this
    # interpreter cannot resolve, then the unconditional cell, then the operand-conditional one; an admitted
    # spec falls through carrying itself onto the dispatch. A frontend whose whole arm rides one lazily-bound
    # distribution answers the floor row under its own tag, so ONE gate spans the axis and an in-process arm
    # binding only module-level imports carries no coordinate and falls straight through.
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
            # the flow lives inside the Flight SQL DRIVER, which mints a bearer off its own option roster; the native
            # client this leg opens carries a header and no minting seam at all, so an oauth-configured spec refuses
            # here rather than redeeming a ticket anonymously against a server the same credential set authenticates
            # to on every ADBC call.
            return Error(QUERY_UNREACHED.raised("flight", RemoteRefusal.FLIGHT_NATIVE_OAUTH.value))
        case QuerySpec(tag="streaming", streaming=(_plan, runner, _cluster)):
            # floor row FIRST — the runner probe below binds the lazy distribution, so an absent `daft` answers its
            # import fact rather than raising inside the very gate that exists to foreclose it. Then the runner: it
            # is PROCESS-WIDE with no call-local form, so honouring a divergent selection re-points the runner under
            # every query already executing on it and races two concurrent specs for one global. The composition
            # resolves it once and a spec naming the other refuses by name, the same verbatim-selection law the
            # remote arms hold — an implicit swap answers a request nobody made.
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
            QuerySpec(tag="federated", federated=(Federation(tag="execute", execute=wire), _stores, _tables))
            | QuerySpec(tag="flight", flight=(wire, _dsn, _transport))
        ):
            # these two legs are the ONLY specs carrying a payload this branch did not author, so both join every
            # other capability bound at one gate: floor row first, then the plan's own admission. A plan refused
            # here costs nothing; the same plan refused at the Flight producer costs a round trip and comes back as
            # a remote error naming no local reason.
            return (
                _floor(spec.tag, spec.tag)
                .or_else_with(lambda: _plan_refusal(wire).map(lambda refusal: Error(QUERY_UNREACHED.raised(spec.tag, refusal.value))))
                .default_value(Ok(spec))
            )
        case QuerySpec(tag=tag):
            return _floor(tag, tag).default_value(Ok(spec))


# data-side endpoint of the runtime PEP-249 wrap seam: this plane admits the driver modules, so it declares the
# `DbapiSeam` rows and the composition root threads each through `Instrumentation.dbapi` — zero data-side
# instrumentor import, db-semconv query spans landing beside the `QueryReceipt.profile` band. The roster derives
# from `_DRIVER` under the SAME floor gate the dispatch reads, because a seam row naming a distribution this
# interpreter never resolved crashes the composition root on the very import the gate exists to foreclose; the
# LOADER kind is excluded by shape (`read_sql` exposes no PEP-249 connection to wrap). The Flight SQL row is
# additive beside the driver's embedded Go tracer, which the `call_options` trace-parent header stitches.
def dbapi_seams() -> tuple[DbapiSeam, ...]:
    return (
        DbapiSeam(name=_SCOPE, connect_module=duckdb, connect_method_name="connect", database_system="duckdb"),
        *(
            DbapiSeam(name=_SCOPE, connect_module=row.ns(), connect_method_name="connect", database_system=row.system)
            for driver, row in _DRIVER.items()
            if row.kind is not DriverKind.LOADER and _UNREACHED.try_find(driver.value).is_none()
        ),
    )


# --- [STREAMING]

# (reader, time-travel key, hive-partition support) per lakehouse frontend. The third column is load-bearing for the
# cold analytics residence: its partition columns live in PATH SEGMENTS, so a reader without the flag returns rows
# missing the very columns a `(domain, date)` predicate prunes on. Only the file-shaped reader carries it — a table
# format reads its partitioning out of its own metadata, so the flag has no meaning and no keyword there.
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
    # layout is a separate axis from travel: a hive tree names its partition columns in the path, and a reader whose
    # row does not carry the flag never receives the keyword rather than receiving it as False.
    layout = {"hive_partitioning": True} if hive and plan.hive_partitioning else {}
    return reader(plan.source, io_config=plan.io_config, **travel, **layout)


class _Shape(Struct, frozen=True):
    # one daft shaping verb: the name the row reads by, the operand predicate deciding whether the plan carries it,
    # and the projection applying it. Both callables dereference the `lazy` daft proxy at CALL time, so the table
    # itself reifies no provider attribute at import.
    named: str
    applies: Callable[[StreamingPlan], bool]
    apply: Callable[[Any, StreamingPlan], Any]


# the daft shaping chain as ORDERED data: the row's position in this `Block` IS the applied order, which is the fact
# a sequence of rebound locals encoded and stated nowhere. Order is load-bearing — `groupby/agg` consumes the columns
# `with_columns` mints, `limit` after `sort` is a top-k where the reverse is an arbitrary slice — so a new verb lands
# at its position rather than wherever a hand-inserted rebind happened to go.
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


# daft `stats`-map durations arrive unit-tagged; the row folds the value to seconds off the unit, never a fixed divisor.
_DURATION_UNIT: Final[Map[str, float]] = Map.of_seq([("ns", 1e-9), ("us", 1e-6), ("µs", 1e-6), ("ms", 1e-3), ("s", 1.0)])


def _daft_seconds(duration: Mapping[str, Any] | None) -> float:
    return 0.0 if not duration else float(duration["value"]) * (_DURATION_UNIT.get(str(duration.get("unit") or "s")) or 1.0)


def _daft_operators(metrics: Any) -> tuple[tuple[str, float, int], ...]:
    # daft's materialized `DataFrame.metrics` RecordBatch carries one row per physical operator whose `stats` map holds a
    # `duration` {value, unit} struct and a `rows.out` cardinality struct, folded to the shared `(name, seconds, rows)` band.
    # `stats` binds through a single-element generator clause, never a walrus in a condition true by construction.
    return tuple(
        (str(row["name"]), _daft_seconds(stats.get("duration")), int((stats.get("rows.out") or {"value": 0.0})["value"]))
        for row in metrics.to_pylist()
        for stats in (dict(row["stats"] or ()),)
    )


def _stream(plan: StreamingPlan, runner: Runner, cluster: str | None, frames: Frames, profiling: ProfileMode = ProfileMode.OFF) -> pa.Table:
    # the runner is PROCESS-WIDE state with no call-local form: a bare per-request switch re-points it under every
    # query already executing on it, and two concurrent specs naming different runners race one global. `_reach` has
    # already proved this process resolved the runner TYPE this spec names, so the ray arm binds only its address and
    # binds it idempotently — `noop_if_initialized` is daft's own answer for a runner already standing — while the
    # native arm sets nothing, `set_runner_native` being exactly the re-point that tears a live ray runner down.
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
    # daft exposes structured per-operator execution statistics off `DataFrame.metrics` once materialized — a RecordBatch
    # of (id, name, type, category, duration, stats) rows the `_daft_operators` fold lands on the shared operator band;
    # `into_partitions` is a Ray-only repartition and a documented no-op on the native runner, so `collect` materializes,
    # wall latency brackets the collect, and grain reads the real `num_partitions()` (`None` on the native single partition).
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
    # one in-process datafusion session per run: each `FederatedStore` row federates a remote object store through
    # the branch's `store_handle` fold off its ref's own columns — retry envelope and credential carried, never a
    # second `from_url` spelling and never a provider beside the coordinate — each `FederatedTable` row registers a listing table a plan names over those bytes, and every input
    # registers by name through `from_arrow(name=)`. Store registration federates BYTES and names no relation, so a
    # plan over the cold analytics residence resolves nothing until its table row lands. The wire bytes stamp the
    # result table's schema metadata, so the plan RIDES the table to the Persistence consumer.
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
    # `execute_stream` drains as a RecordBatchStream unwrapped through `RecordBatch.to_pyarrow()` — incremental, never one giant collect.
    started = perf_counter()
    batches = [batch.to_pyarrow() for batch in bound.execute_stream()]
    latency_s = perf_counter() - started
    table = (pa.Table.from_batches(batches) if batches else pa.table({})).replace_schema_metadata({_PLAN_META: wire})
    if profiling is ProfileMode.OFF:
        return table
    # datafusion's python `ExecutionPlan` exposes only `display`/`display_indent`/`partition_count` — no structured
    # per-operator metric accessor — so the adapter harvests wall latency around the drain plus result cardinality and
    # byte size through the scalar harvest, merged onto the plan-bearing metadata band by `EngineProfile.stamp`.
    return EngineProfile.of(ProfileHarvest(scalar=(latency_s, table.num_rows, table.nbytes))).stamp(table)


# --- [RECEIPTS] -------------------------------------------------------------------------

type Provenance = tuple[str, int, tuple[LineageEdge, ...]]

# predicate-bearing Substrait relation kinds, the plan-side twin of the `tabular/columnar#SCAN` `_PREDICATE_NODES`
# widening: a foreign plan's receipt counts the same predicate classes a local statement does, so one provenance
# grain spans both wires and a board comparing them is not comparing two different measures.
_PLAN_PREDICATES: Final[frozenset[str]] = frozenset({"filter", "join", "hash_join", "merge_join", "nested_loop_join"})

# `_REL` pins the descriptor identity the walk recognises a relation BY, wherever in the message tree it sits.
_REL: Final = substrait_proto.Rel.DESCRIPTOR


def _plan_rels(plan: "substrait_proto.Plan") -> "Iterator[str]":
    # ONE depth-first walk over the plan's WHOLE message tree, generic on two axes: `Rel` is a protobuf `oneof`, so
    # `WhichOneof` names each node, and a relation is recognised by its own descriptor rather than by the field that
    # holds it — so a relation nested under an `Expression.Subquery` counts exactly as a direct `input`/`left`/
    # `inputs` child does. `sqlglot.find_all` walks a subquery's predicates on the SQL legs, so a `Rel`-typed-field
    # roster alone reports a plan whose only join sits inside a scalar subquery as carrying zero predicates,
    # breaking the very parity this census claims. A relation kind the spec adds upstream walks free.
    # `HasField` gates each unset singular message: the algebra is mutually recursive, so descending into a default
    # sub-message would walk `Rel`->`Expression`->`Rel` forever. Map entries carry keys, never messages.
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
    # `_reach` already proved these bytes parse, so the census reads the admitted plan rather than reporting the
    # placeholder a foreign leg would otherwise carry: predicate-bearing relations count the SAME classes the SQL
    # legs count through `_PREDICATE_NODES`, and the relation roster rides the lineage slot so an inbound receipt
    # carries the plan's real shape. A parse re-run here is the cost of keeping the gate free of receipt concerns.
    # Extension urns ride that same slot under their own edge class: reach RESOLVED every one of them to admit the
    # plan, and a foreign producer's function vocabulary is the one provenance fact no relation kind carries — an
    # urn the estate later retires reads off the receipts that already used it.
    plan = substrait_proto.Plan()
    plan.ParseFromString(wire)
    rels = tuple(_plan_rels(plan))
    edges = (*(("substrait", kind) for kind in rels), *(("substrait-urn", named.urn) for named in plan.extension_urns))
    return "substrait-plan", sum(1 for kind in rels if kind in _PLAN_PREDICATES), edges


def _endpoint(dsn: str) -> str:
    # a DSN carries `user:password` material and a receipt is DURABLE evidence contributed onward, so provenance
    # keeps the scheme-host-path coordinate and drops the userinfo, query, and fragment with it; a DSN carrying
    # no `//` authority (a local file or `:memory:`) has no credential band to strip and rides verbatim.
    # an authority-less DSN rides verbatim, and so does one whose authority carries no HOSTNAME: `parsed.hostname or ""`
    # collapsed every such DSN onto `""` or `":5432"`, merging distinct endpoints onto ONE durable provenance
    # coordinate — the `netloc` this arm falls back to is the authority the producer actually wrote, credential band
    # stripped by `partition`, so two endpoints stay two coordinates.
    parsed = urlsplit(dsn)
    match Option.of_optional(parsed.hostname):
        case Option(tag="none"):
            return dsn if not parsed.netloc else urlunsplit((parsed.scheme, parsed.netloc.rpartition("@")[2], parsed.path, "", ""))
        case Option(tag="some", some=hostname):
            host = f"{hostname}:{parsed.port}" if parsed.port else hostname
            return dsn if not parsed.netloc else urlunsplit((parsed.scheme, host, parsed.path, "", ""))


# imported `tabular/columnar#SCAN` fold — applied here, never re-spelling the byte-identical `find_all(*_PREDICATE_NODES)`.
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
            # `Remote`'s first slot carries the op's SUBJECT, and on these two it is a TARGET TABLE NAME the driver
            # resolves through its own catalog rather than a statement — the same split `_dbapi_op` dispatches on.
            # Parsing a bare identifier as SQL reports lineage the operation never expressed, and a name the parser
            # refuses raises past a fence that fails the receipt AFTER the remote already committed.
            return _endpoint(dsn), 0, ()
        case QuerySpec(tag="remote", remote=(sql, dsn, _driver, _op, transport)):
            # each remote engine's own dialect parses its statement; a DuckDB default silently mis-reads a
            # Snowflake or Postgres statement and reports lineage the query never expressed.
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
            # ticket redemption executes a producer-held plan, so the endpoint names the source while the plan's own
            # census carries the shape — an endpoint-only provenance reports every remote query as one opaque leg.
            _source, predicates, edges = _plan_provenance(plan)
            return _endpoint(dsn), predicates, edges
        case unreachable:
            assert_never(unreachable)


def _wire(spec: QuerySpec, table: pa.Table) -> "Option[bytes]":
    # FEDERATED and FLIGHT arms key by the PLAN BYTES — the stamped schema metadata or the ticket-minting command — so the
    # in-process execution and the ticket redemption of one Persistence plan share one reuse-ledger dedupe key. An absent
    # stamp is `Nothing` rather than empty bytes, because a zero-length key collapses every unstamped result onto one slot.
    match spec:
        case QuerySpec(tag="federated"):
            return Option.of_optional((table.schema.metadata or {}).get(_PLAN_META))
        case QuerySpec(tag="flight", flight=(plan, _dsn, _transport)):
            return Some(plan)
        case QuerySpec():
            return Nothing


def receipt_of(spec: QuerySpec, table: pa.Table, mode: ProfileMode = ProfileMode.OFF) -> "RuntimeRail[QueryReceipt]":
    # `_provenance` drives `sqlglot` over caller SQL, so a malformed or dialect-foreign statement raises past the
    # rail this signature declares — one `boundary` fence converts it once, and a receipt mint never crashes the
    # run that already produced its table.
    # `mode` rides the caller's OWN arming so the receipt separates "nobody asked to profile" from "the engine
    # published no band"; an unarmed caller keeps the `OFF` default and its receipt says so.
    raises = (SqlglotError, ibis.common.exceptions.IbisError, protobuf_message.DecodeError, ValueError)
    return boundary(QUERY_PROVENANCE, lambda: _provenance(spec), catch=raises).bind(lambda fold: _keyed(spec, table, fold, mode))


def _keyed(spec: QuerySpec, table: pa.Table, fold: Provenance, mode: ProfileMode) -> "RuntimeRail[QueryReceipt]":
    source, predicates, edges = fold
    # plan-keyed arms carry the profile band too — the federated arm's datafusion harvest rides the same table
    # metadata the plan wire rides, so a plan-keyed receipt is profile-bearing exactly as a railed one; every
    # other arm keys by the canonical Arrow result bytes through the railed `ContentIdentity.of`.
    return (
        _wire(spec, table)
        .map(
            lambda wire: ContentIdentity.of("query.plan", wire).map(
                lambda key: QueryReceipt.of(
                    spec.tag,
                    source,
                    table,
                    key,
                    predicate_count=predicates,
                    lineage_edges=edges,
                    mode=mode,
                    profile=EngineProfile.from_table(table),
                )
            )
        )
        .default_with(lambda: QueryReceipt.railed(spec.tag, source, table, predicate_count=predicates, lineage_edges=edges, mode=mode))
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
