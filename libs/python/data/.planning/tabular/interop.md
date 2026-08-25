# [PY_DATA_INTEROP]

`FrameInterop` translates any admitted native dataframe to any backend over `narwhals`, and `ArrowCStream` carries Arrow across the seam pyarrow-free over the Arrow C Data Interface — one owner over the two interchange hops. `FrameInterop` discriminates a single `Backend` axis bound to the live `narwhals.Implementation` vocabulary against one `_BACKEND` behavior table, so a new backend is one `Backend` row plus one `_BACKEND` row, never a parallel adapter family and never an `isinstance` dispatch over native frame types. This is the tier-0 base of the tabular plane: it imports nothing from `rasm.data`, and every folder composition edge points strictly down into it.

Its axis spans the narwhals lazy set — `POLARS`/`PANDAS`/`PYARROW`/`MODIN` eager, `DUCKDB`/`IBIS`/`DASK` lazy — each row's `eager` column selecting both the `from_native` intake level and the lowering head, so a lazy plan survives only into a lazy target and materializes on a lazy→eager hop by contract. `DataLeg` and `DataHook` are DECLARED here for the same reason the tier-0 seat exists: they are the folder's one raise-leg roster under the `runtime/reliability/faults#FAULT` `Leg` contract and its one hook-id roster under the `runtime/observability/hooks#HOOKS` `HookId` shape, every data page anchors its `FaultRow` table and its `HookPoint` rows on a member of one of them, and only a module importing no sibling can be reached by every raiser and every point owner without inverting the codemap order. `FieldShape` is DECLARED here — the `schema_of` minter's page — and `tabular/contract#ADMISSION` and `tabular/profile#PROFILE` import it strictly downward, never a back-edge. `arrow_bytes` is DECLARED here too: the folder's one canonical whole-table Arrow IPC serialization, imported downward by `tabular/columnar#SCAN`, every `tabular` receipt owner keying a frame, and the `gridded/ragged#RAGGED` `arrow`-sink, so one byte stream addresses every content-keyed frame in the package. `ArrowCStream.of` is the one carrier construction both `FrameInterop.c_stream` and the sibling `gridded/ragged#RAGGED` `RaggedArray.c_stream` compose. `cohort_bytes` is the public numeric-cohort projection the compute study emit resolves against, and `wire_bytes` is the transport-band compressed IPC leg whose payload never reaches an identity fold. `InteropReceipt` content-keys each hop through runtime `ContentIdentity` and streams through `ReceiptContributor`, carrying the same typed evidence the `columnar`/`contract` owners do.

## [01]-[INDEX]

- [02]-[INTEROP]: the backend-agnostic translation owner over the seven-row eager/lazy `Backend` axis and `_BACKEND` table, the folder-wide `DataLeg` raise-leg and `DataHook` point-id rosters, this module's `RAISES` table, the locally-declared `FieldShape`/`FieldBreach`/`ShapeSource` and the `ColumnSpec` durable-column declaration with its four derivations, the null-mask schema fold, the folder's `arrow_bytes` serialization, the `cohort_bytes` numeric-cohort projection, and the content-keyed `InteropReceipt`.
- [03]-[CARRIER]: the pyarrow-free Arrow C Data Interface carrier — `ArrowCStream.of` construction, `chunks` streaming consumption, `negotiate` schema-only folding, `device_of` the C Device array row, and the `wire_bytes` transport-band IPC codec over the closed `WireCodec` vocabulary.

## [02]-[INTEROP]

- Owner: `DoeDataset` — the `dotnet:Rasm.Compute/Solver/sweep` training-corpus wire admission mirroring the producer record arm-for-arm (`ContentKey`/`Axes`/`Objectives`/`Strategy`/`Points`/row-major `Coordinates`/`Responses`/front-membership `OnFront`/`At`), its `frame` fold the graduation loop's fit ingress keyed by the wire's own content key. `FrameInterop` — the one translation owner over `narwhals`, discriminating a `Backend` `StrEnum` whose value IS the `narwhals.Implementation` member value, so `Backend.implementation` resolves through `Implementation.from_backend` and the axis carries no `ARROW`-named drift (narwhals names `PYARROW`, value `'pyarrow'`). `narwhals.from_native` is the one intake; the lowering target is one `_BACKEND` row. `FieldShape` is declared here carrying the `ShapeSource` posture that names WHICH surface stated its nullability, and `resolve` is its structural-breach fold answering `Option[FieldBreach]` — one keyed divergence, never a joined token stream — the `tabular/contract#ADMISSION` gate accumulates. `arrow_bytes` is declared here as the folder's one whole-table IPC serialization — a schema message ahead of one combined batch — so every content key over a table payload folds identical bytes wherever it is minted. `InteropReceipt` is the typed hop receipt satisfying `ReceiptContributor`. `DataLeg` is declared here as the folder's one raise-leg roster: it names every module in the `data` codemap, satisfies the runtime `Leg` contract structurally, and seats at the tier-0 base so a raiser earlier in the codemap order reaches it without a back-edge. `DataHook` seats beside it on the identical argument: `tabular/materialize#MATERIALIZE` composes `DATA_HOOK_POINTS` out of rows declared at four sibling pages it already imports, so a roster seated there would invert every one of those edges.
- Cases: `Backend` is the closed axis; `_BACKEND` pairs each row with its `_Lowering` — the `lower` head and the `eager` flag — so admission level and lowering head are two columns on one row, never parallel switch arms.
  - A lazy-scan reader is deliberately NOT a column: a backend admits many formats, so a per-engine reader name is incoherent — `tabular/columnar#SCAN` resolves its reader off `DatasetKind`, a disjoint axis this owner never duplicates.
  - One `eager` column serves BOTH source and target for distinct concerns: the SOURCE flag selects `_admit`'s `from_native(eager_only=)` intake, the TARGET flag selects the frame the lowering head receives.
  - Load-bearing invariant: `to_polars`/`to_pandas`/`to_arrow` are `nw.DataFrame`-only, so a lazy-source→eager-target lowering MUST collect first — `_lowered` hands the row's `lower` head `eager if row.eager else admitted` off the existing column. A lazy plan survives only into a lazy target; a lazy→eager hop materializes by contract, and the receipt counts and content-key bytes read the `_eager` projection so the null-mask always reads a materialized frame while a lazy-target output stays lazy.
  - `_lowered` and `_admit` both key `_BACKEND[...]` through the total `Map.__getitem__` — every `Backend` member carries a row by construction, never an inline per-backend `match` and never a `try_find`/`assert_never(target)` fold lying about totality over a live `StrEnum`.
- Entry: `translate` lifts through `_admit`, lowers once into one `_Lowered` carry, and binds `ContentIdentity.of` over the lowered frame's canonical Arrow bytes into a `RuntimeRail[FrameTranslation]` — the lowering wrapped in one `boundary(...)` so a terminal raise lifts to `BoundaryFault` exactly once and the key threads through `.bind`/`.map`, never a second fault fence. `schema_of` reads the agnostic `nw.Schema` through `collect_schema()` and the per-column null count through `null_count().item(0, name)` — never a `.to_native()[name][0]` subscript that breaks on a `pyarrow.Table` — so nullability is the observed mask, not a dtype-kind inference. `namespace` resolves through `Implementation.to_native_namespace`. `c_stream` lowers to a `pyarrow.Table` through `_eager(self._admit(frame)).to_arrow()` before `ArrowCStream.of` — the one `__arrow_c_stream__`-exporter every backend reaches — never the raw native frame a `pandas`/`modin` source does not reliably export. No hop carries a `stamina` retry: a `to_*`/`to_native` projection is a pure in-memory transform, not a transient-I/O hop.
- Receipt: `translate` keys one `InteropReceipt` by `ContentIdentity.of("interop", lowered.ipc)` over the `arrow_bytes` stream this owner declares, carrying source/target backend and row/column counts, never a path-string key. `ArrowCStream` stays interchange-only and NEVER a key byte source: a consume-once capsule is not a deterministic byte stream, so every key in the folder rides `arrow_bytes` instead.
- Law: `ColumnSpec` is the folder's ONE durable-column declaration: `column_schema`, `column_frame`, `column_struct`, and `column_rows` are four DERIVATIONS off one roster, so `tabular/lakehouse#LAKEHOUSE`, `tabular/journal#JOURNAL`, and `tabular/cost#COST` each declare a plane's columns once instead of transcribing them into an Arrow schema, a builder projection, a hand-written struct, and a row-wise string-subscript reader. It stays distinct from `FieldShape`: that row is the structural PROBE a contract compares and carries the `ShapeSource` posture, this row is the GENERATOR a durable plane builds from and carries an Arrow type and a producer projection — fusing them would erase the posture on one side and the Arrow binding on the other.
- Law: `cohort_bytes` is the folder's ONE numeric-cohort projection — axis and objective names beside row-major coordinate and response vectors build the Arrow training table through the same strided fold `DoeDataset.frame` composes, then ride `arrow_bytes` — so a sibling emits canonical cohort bytes without importing a single pyarrow construction member, and the compute study emit resolves against this public fold. `DoeDataset.frame` is that fold carrying the wire's own residue — the `on_front` boolean column and the `content_key`/`strategy`/`at`/`points` schema metadata the C# `ArrowBatch.Landing(LakeDataset.Doe, …)` emit carries — so both ends declare ONE Arrow schema and a maskless training table beside the masked producer batch is the two-schema fork this fold deletes; `on_front` is the reserved mask column no axis or objective may claim. Extent admission is exact: a coordinate, response, or mask vector off its `points x names` (mask: `points`) extent refuses on the rail before any column builds.
- Growth: a new backend is one `Backend` row naming its `Implementation` member plus one `_BACKEND` `(lower, eager)` row (`PYSPARK`/`SQLFRAME` land this way); a new interchange protocol is one method; a new admission level is one `eager` value; a new structural attribute is one `FieldShape` column read once by `schema_of`; a new structural divergence is one `BreachKind` member with its arm on `resolve`; a new durable column on any composing plane is one `ColumnSpec` row whose four derivations follow; a new receipt slot is one `InteropReceipt` field; a new content-keying consumer imports `arrow_bytes` and adds nothing; a new cohort column family is one name tuple on the same `cohort_bytes` call; a new data module is one `DataLeg` member whose value names it, its rows landing at that module's own page; a new package hook point is one `DataHook` member whose value names it, its row landing at the firing owner's page; a new refusal law here is one `FaultRow` on `RAISES`; zero new surface.
- Boundary: no compute (the numeric and labelled-array ownership stays in `compute`), no durable store, no query rail (`tabular/query#QUERY` owns the relational plane), no lazy-scan execution (`tabular/columnar#SCAN` owns the `register_io_source` pushdown); `narwhals` owns only the frame-translation hop and the schema fold; `DoeDataset` is the WIRE ADMISSION only — the fit itself is the `compute` companion's and the graduated ONNX crosses back over `GraduationEvidence`, never a training loop here. Rejected forms: a per-backend `PolarsAdapter`/`PandasAdapter` trio or `isinstance` dispatch where one `_BACKEND` row owns lowering; a `FieldShape` re-declared on a consumer page where this minter owns it; a second `collect_schema()` path beside `schema_of`; a bare receipt-less lowering entrypoint beside `translate`, whose `FrameTranslation` already carries the lowered frame with its key; a second whole-table serialization beside `arrow_bytes` — `nanoarrow.ArrayStream(table).read_all().serialize()` is the falsified twin, emitting a bare batch message with NO schema, so `pyarrow.ipc.open_stream` refuses those bytes outright and two frames differing in schema alone mint one identical key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Buffer, Callable, Iterable, Iterator
from enum import StrEnum
from typing import Any, Final

import nanoarrow
import nanoarrow.device
import narwhals as nw
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block, Map
from msgspec import Struct, defstruct
from narwhals.exceptions import NarwhalsError
from nanoarrow._utils import NanoarrowException

lazy import pyarrow as pa
lazy from arro3.io import exceptions as arro3_exceptions, read_ipc_stream, write_ipc_stream

from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------


class DataLeg(StrEnum):
    INTEROP = "data.tabular.interop"
    COLUMNAR = "data.tabular.columnar"
    LAKEHOUSE = "data.tabular.lakehouse"
    TABULAR_QUERY = "data.tabular.query"
    MATERIALIZE = "data.tabular.materialize"
    CONTRACT = "data.tabular.contract"
    PROFILE = "data.tabular.profile"
    EGRESS = "data.tabular.egress"
    COST = "data.tabular.cost"
    JOURNAL = "data.tabular.journal"
    GEOSPATIAL = "data.spatial.geospatial"
    SPATIAL_QUERY = "data.spatial.query"
    GRID = "data.spatial.grid"
    CATALOG = "data.spatial.catalog"
    MESH = "data.spatial.mesh"
    CUBE = "data.spatial.cube"
    STORE = "data.gridded.store"
    VIRTUAL = "data.gridded.virtual"
    RAGGED = "data.gridded.ragged"
    FIELD = "data.gridded.field"
    ENSEMBLE = "data.gridded.ensemble"
    GRAPH = "data.graph.graph"
    NETWORK = "data.graph.network"
    IMPACT = "data.impact.impact"
    DECLARATION = "data.impact.declaration"
    INVENTORY = "data.impact.inventory"
    SOLVE = "data.impact.solve"
    SCENARIO = "data.impact.scenario"


class DataHook(StrEnum):
    LAKE_COMMIT = "rasm.data.lakehouse.commit"
    EGRESS_PUT = "rasm.data.egress.put"
    EGRESS_DELETE = "rasm.data.egress.delete"
    EGRESS_COPY = "rasm.data.egress.copy"
    EGRESS_RENAME = "rasm.data.egress.rename"
    MATERIALIZE_REFRESH = "rasm.data.materialize.refresh"
    CONTRACT_VERDICT = "rasm.data.contract.verdict"


class ShapeSource(StrEnum):
    DECLARED = "declared"
    OBSERVED = "observed"


class BreachKind(StrEnum):
    ABSENT = "absent"
    LOGICAL_TYPE = "logical-type"
    NULLABILITY = "nullability"


class Backend(StrEnum):
    POLARS = "polars"
    PANDAS = "pandas"
    PYARROW = "pyarrow"
    MODIN = "modin"
    DUCKDB = "duckdb"
    IBIS = "ibis"
    DASK = "dask"

    @property
    def implementation(self) -> nw.Implementation:
        return nw.Implementation.from_backend(self.value)


# --- [CONSTANTS] ------------------------------------------------------------------------

_NARWHALS_RAISES: Final[Catch] = (NarwhalsError, TypeError)
_CARRIER_RAISES: Final[Catch] = (NanoarrowException, TypeError, ValueError)

# --- [MODELS] ---------------------------------------------------------------------------


class FieldBreach(Struct, frozen=True):
    field: str
    kind: BreachKind
    declared: str
    observed: str

    @property
    def spelled(self) -> str:
        return f"{self.field}:{self.kind.value}:{self.declared}!={self.observed}"


class ColumnSpec[R, T](Struct, frozen=True):
    name: str
    arrow: "pa.DataType"
    kind: type[T]
    lift: Callable[[R], T]
    nullable: bool = False


class FieldShape(Struct, frozen=True):
    field: str
    logical_type: str
    nullable: bool
    source: ShapeSource

    def resolve(self, live: "Map[str, FieldShape]") -> Option[FieldBreach]:
        match live.try_find(self.field):
            case Option(tag="none"):
                return Some(FieldBreach(field=self.field, kind=BreachKind.ABSENT, declared=self.logical_type, observed=""))
            case Option(tag="some", some=FieldShape(logical_type=actual)) if actual != self.logical_type:
                return Some(FieldBreach(field=self.field, kind=BreachKind.LOGICAL_TYPE, declared=self.logical_type, observed=actual))
            case Option(tag="some", some=FieldShape(nullable=True, source=observed)) if not self.nullable:
                return Some(FieldBreach(field=self.field, kind=BreachKind.NULLABILITY, declared=self.source.value, observed=observed.value))
            case _:
                return Nothing


class _Lowering(Struct, frozen=True):
    lower: Callable[[nw.DataFrame[Any] | nw.LazyFrame[Any]], Any]
    eager: bool


class InteropReceipt(Struct, frozen=True):
    source: Backend
    target: Backend
    rows: int
    columns: int
    content_key: ContentKey

    @classmethod
    def of(cls, source: Backend, target: Backend, frame: nw.DataFrame[Any], key: ContentKey) -> "InteropReceipt":
        return cls(source=source, target=target, rows=frame.shape[0], columns=len(frame.columns), content_key=key)

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "frame-interop",
            (
                "emitted",
                f"{self.source}->{self.target}",
                {"domain": "interop", "kind": self.target, "key": self.content_key.hex, "rows": self.rows, "columns": self.columns},
            ),
        )


class FrameTranslation(Struct, frozen=True):
    frame: Any
    receipt: InteropReceipt


class _Lowered(Struct, frozen=True):
    frame: Any
    agnostic: nw.DataFrame[Any]
    ipc: Buffer


# --- [ERRORS] ---------------------------------------------------------------------------


def _arrow_raises() -> Catch:
    return (pa.ArrowException, OSError)


def _wire_raises() -> Catch:
    return (arro3_exceptions.BaseError, OSError, ValueError)


DOE_EXTENT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="doe.extent", arm="config", defect="mask-extent", retriability=TERMINAL, slots=("mask", "points")
)
DOE_RESERVED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="doe.reserved", arm="config", defect="field-reserved", retriability=TERMINAL, slots=("column",)
)
DOE_BUILD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="doe.build", arm="boundary", defect="doe-build", retriability=TERMINAL
)
COHORT_EXTENT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="cohort", arm="config", defect="cohort-extent", retriability=TERMINAL, slots=("coordinates", "responses", "expected")
)
COHORT_BUILD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="cohort.build", arm="boundary", defect="cohort-build", retriability=TERMINAL
)
FRAME_LOWER: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="translate", arm="boundary", defect="lowering", retriability=TERMINAL
)
FRAME_SCHEMA: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="schema", arm="boundary", defect="schema-fold", retriability=TERMINAL
)
FRAME_NAMESPACE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="namespace", arm="import_", defect="namespace-absent", retriability=TERMINAL
)
CARRIER_EXPORT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="carrier", arm="boundary", defect="capsule-export", retriability=TERMINAL
)
CARRIER_UNNAMED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="carrier.field", arm="boundary", defect="unnamed-child", retriability=TERMINAL, slots=("ordinal",)
)
WIRE_CODEC: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INTEROP, point="wire", arm="boundary", defect="ipc-codec", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    DOE_EXTENT,
    DOE_RESERVED,
    DOE_BUILD,
    COHORT_EXTENT,
    COHORT_BUILD,
    FRAME_LOWER,
    FRAME_SCHEMA,
    FRAME_NAMESPACE,
    CARRIER_EXPORT,
    CARRIER_UNNAMED,
    WIRE_CODEC,
]))

# --- [TABLES] ---------------------------------------------------------------------------

_BACKEND: Final[Map[Backend, _Lowering]] = Map.of_seq([
    (Backend.POLARS, _Lowering(lambda f: f.to_polars(), True)),
    (Backend.PANDAS, _Lowering(lambda f: f.to_pandas(), True)),
    (Backend.PYARROW, _Lowering(lambda f: f.to_arrow(), True)),
    (Backend.MODIN, _Lowering(lambda f: f.to_native(), True)),
    (Backend.DUCKDB, _Lowering(lambda f: f.to_native(), False)),
    (Backend.IBIS, _Lowering(lambda f: f.to_native(), False)),
    (Backend.DASK, _Lowering(lambda f: f.to_native(), False)),
])

# --- [SERVICES] -------------------------------------------------------------------------


class DoeDataset(Struct, frozen=True):
    content_key: str
    axes: tuple[str, ...]
    objectives: tuple[str, ...]
    strategy: str
    points: int
    coordinates: tuple[float, ...]
    responses: tuple[float, ...]
    on_front: tuple[bool, ...]
    at: str

    def frame(self) -> RuntimeRail["Any"]:
        """The ONE Doe Arrow schema both ends declare: cohort columns, the `on_front` mask, wire provenance as metadata."""
        if len(self.on_front) != self.points:
            return Error(DOE_EXTENT.raised(str(len(self.on_front)), str(self.points)))
        if "on_front" in self.axes or "on_front" in self.objectives:
            return Error(DOE_RESERVED.raised("on_front"))
        return _cohort_table(self.axes, self.objectives, self.coordinates, self.responses, self.points).bind(
            lambda table: boundary(DOE_BUILD, lambda: self._labelled(table), catch=_arrow_raises())
        )

    def _labelled(self, table: "pa.Table") -> "pa.Table":
        return table.append_column(
            pa.field("on_front", pa.bool_(), nullable=False), pa.array(self.on_front, type=pa.bool_())
        ).replace_schema_metadata({"content_key": self.content_key, "strategy": self.strategy, "at": self.at, "points": str(self.points)})


class FrameInterop(Struct, frozen=True):
    source: Backend

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, source: Backend) -> "FrameInterop":
        return cls(source=source)

    def translate(self, frame: Any, target: Backend) -> RuntimeRail[FrameTranslation]:
        return boundary(FRAME_LOWER, lambda: self._lowered(frame, target), catch=(*_NARWHALS_RAISES, *_arrow_raises())).bind(
            lambda lowered: ContentIdentity.of("interop", lowered.ipc).map(
                lambda key: FrameTranslation(frame=lowered.frame, receipt=InteropReceipt.of(self.source, target, lowered.agnostic, key))
            )
        )

    def schema_of(self, frame: Any) -> RuntimeRail[tuple[FieldShape, ...]]:
        return boundary(FRAME_SCHEMA, lambda: _shapes(_eager(self._admit(frame))), catch=_NARWHALS_RAISES)

    def namespace(self) -> RuntimeRail[Any]:
        return boundary(FRAME_NAMESPACE, self.source.implementation.to_native_namespace, catch=(ImportError, *_NARWHALS_RAISES))

    def c_stream(self, frame: Any) -> RuntimeRail["ArrowCStream"]:
        raises = (*_NARWHALS_RAISES, *_CARRIER_RAISES, *_arrow_raises())
        return boundary(CARRIER_EXPORT, lambda: ArrowCStream.of(_eager(self._admit(frame)).to_arrow()), catch=raises)

    def _admit(self, frame: Any) -> nw.DataFrame[Any] | nw.LazyFrame[Any]:
        return nw.from_native(frame, eager_only=_BACKEND[self.source].eager)

    def _lowered(self, frame: Any, target: Backend) -> "_Lowered":
        admitted = self._admit(frame)
        eager = _eager(admitted)
        ipc = arrow_bytes(eager.to_arrow())
        row = _BACKEND[target]
        return _Lowered(frame=row.lower(eager if row.eager else admitted), agnostic=eager, ipc=ipc)


# --- [OPERATIONS] -----------------------------------------------------------------------


def arrow_bytes(table: "pa.Table") -> Buffer:
    sink = pa.BufferOutputStream()
    with pa.ipc.new_stream(sink, table.schema) as writer:
        writer.write_table(table.combine_chunks())
    return sink.getvalue()


def _cohort_table(
    axes: tuple[str, ...], objectives: tuple[str, ...], coordinates: "Iterable[float]", responses: "Iterable[float]", points: int
) -> RuntimeRail["pa.Table"]:
    xs, ys = tuple(coordinates), tuple(responses)
    if len(xs) != points * len(axes) or len(ys) != points * len(objectives):
        return Error(COHORT_EXTENT.raised(str(len(xs)), str(len(ys)), f"{points}x{len(axes)}x{len(objectives)}"))
    columns = {axis: pa.array(xs[index :: len(axes)]) for index, axis in enumerate(axes)} | {
        objective: pa.array(ys[index :: len(objectives)]) for index, objective in enumerate(objectives)
    }
    return boundary(COHORT_BUILD, lambda: pa.table(columns), catch=_arrow_raises())


def cohort_bytes(
    axes: tuple[str, ...], objectives: tuple[str, ...], coordinates: "Iterable[float]", responses: "Iterable[float]", points: int
) -> RuntimeRail[Buffer]:
    return _cohort_table(axes, objectives, coordinates, responses, points).bind(
        lambda table: boundary(COHORT_BUILD, lambda: arrow_bytes(table), catch=_arrow_raises())
    )


def column_schema(specs: "Block[ColumnSpec[Any, Any]]") -> "pa.Schema":
    return pa.schema([pa.field(spec.name, spec.arrow, nullable=spec.nullable) for spec in specs])


def column_frame[R](specs: "Block[ColumnSpec[R, Any]]", rows: "Block[R]") -> "pa.Table":
    return pa.Table.from_pydict({spec.name: [spec.lift(row) for row in rows] for spec in specs}, schema=column_schema(specs))


def column_struct(name: str, specs: "Block[ColumnSpec[Any, Any]]") -> "type[Struct]":
    return defstruct(name, [(spec.name, spec.kind) for spec in specs], frozen=True)


def column_rows[S: Struct](table: "pa.Table", specs: "Block[ColumnSpec[Any, Any]]", shape: type[S]) -> "Block[S]":
    names = tuple(spec.name for spec in specs)
    columns = tuple(table.column(spec.name).to_pylist() for spec in specs)
    return Block.of_seq(shape(**dict(zip(names, values, strict=True))) for values in zip(*columns, strict=True))


def _eager(frame: nw.DataFrame[Any] | nw.LazyFrame[Any]) -> nw.DataFrame[Any]:
    return frame.collect() if isinstance(frame, nw.LazyFrame) else frame


def _shapes(frame: nw.DataFrame[Any]) -> tuple[FieldShape, ...]:
    schema = frame.collect_schema()
    nulls = frame.null_count()
    return tuple(
        FieldShape(field=name, logical_type=str(dtype), nullable=nulls.item(0, name) > 0, source=ShapeSource.OBSERVED)
        for name, dtype in schema.items()
    )
```

## [03]-[CARRIER]

- Owner: `ArrowCStream` — the pyarrow-free Arrow C Data Interface zero-copy carrier over the PyCapsule stream protocol; one frozen `Struct` carrying the capsule plus the schema repr, never a per-producer wrapper. `ArrowCStream.of` wraps any `__arrow_c_stream__`-exporter through `nanoarrow.ArrayStream` and reads the `.schema` the low-level `c_array_stream` does not expose, so polars/pandas/duckdb and the `gridded/ragged#RAGGED` `ak.to_arrow_table` `pyarrow.Table` (the native `__arrow_c_stream__`-exporter, not `ak.to_arrow` which exports only `__arrow_c_array__`) exchange Arrow without pyarrow. Three siblings complete the depth: `chunks` consumes chunk-by-chunk through `nanoarrow.c_array_stream` with no full materialization; `negotiate` reads the schema WITHOUT moving a batch and folds the child fields onto the rail as a `FieldShape` tuple, refusing a nameless child at its ordinal rather than keying it `""`; `device_of` is the C Device Data Interface array row over `nanoarrow.device.c_device_array`, array-level by construction since no device-array-STREAM constructor exists.
- Entry: `ArrowCStream.of(exporter)` is the one construction both this owner's `FrameInterop.c_stream` and the sibling `gridded/ragged#RAGGED` `RaggedArray.c_stream` compose. It reads the `ArrayStream.schema` repr FIRST, then exports the `__arrow_c_stream__` capsule — the order load-bearing because `__arrow_c_stream__()` moves the Arrow C stream out of the wrapper (a stream capsule is consume-once), so the schema repr is captured before the export and the carried capsule is a single-handoff transport, never re-read and never a second `nanoarrow.ArrayStream` wrap at the consumer seam.
- Auto: `chunks` yields the C-level `CArray` chunk holders directly off the `CArrayStream` iterator so back-pressure lives at the chunk grain — the stream owns a C resource and closes on iterator exhaustion. `negotiate` reads `Schema.fields` (each child carrying `name`/`type`/`nullable`), and it is total over the struct-top schema in SHAPE alone: a child whose `name` is empty carries no roster key, so it refuses at its ordinal — two nameless children would otherwise collide on one slot of the `field -> FieldShape` map the contract gate builds and the gate would report one column's breach under another's declaration. `device_of` resolves the device kind off the held `CDeviceArray` — `DEVICE_CPU` for a host-resident exporter, a non-CPU device surfacing as the holder's own attribute rather than a silent copy to host. `arro3-core` is the alternate zero-copy PyCapsule reader the consumer side crosses on, both reading the canonical Arrow C-data layout without the Arrow C++ source build.
- Law: `wire_bytes` is the folder's one TRANSPORT-band IPC serialization — `arro3.io.write_ipc_stream(data, sink, compression=)` over the closed `WireCodec` vocabulary (`LZ4`/`ZSTD`/`NONE`), pyarrow-free on the slim carrier leg, and `wire_table` its `read_ipc_stream(...).read_all()` inverse — for frames crossing to siblings, .NET peers, or object storage where wire volume matters. It NEVER reaches an identity fold: content keys derive from the `[02]` `arrow_bytes` fold alone, pinned uncompressed by the folder key law, because the arro3 stream is not byte-identical to the canonical serialization even at `NONE` — a `wire_bytes` payload keyed anywhere forks one frame into two keys. Every codec's framing is standard Arrow IPC body compression, so a pyarrow or C# Arrow reader opens the stream with zero re-encode.
- Packages: `nanoarrow` (`ArrayStream`, `c_array_stream` the chunked iterator, `Schema.fields`/`name`/`type`/`nullable`), `nanoarrow.device` (`c_device_array`/`CDeviceArray`/`DeviceType`/`DEVICE_CPU` — the C Device Data Interface submodule, no top-level spellings), `arro3-core` (the alternate PyCapsule reader), `arro3-io` (`write_ipc_stream`/`read_ipc_stream` the transport-band codec pair), `msgspec` (`Struct` the frozen owner).
- Growth: a new PyCapsule-exporting producer is admitted free by `of`/`chunks`/`negotiate` over its `__arrow_c_stream__`/`__arrow_c_schema__` with zero carrier change; a non-CPU device row is one `DeviceType` the held `CDeviceArray` already reports; a new carried evidence field is one `ArrowCStream` column; a new transport codec is one `WireCodec` member; zero new surface.
- Boundary: no Arrow compute, no pyarrow on the carrier path — the capsule crosses through `nanoarrow` alone and the wire codec through `arro3.io`, so the module's `lazy pyarrow` bind stays the `[02]` owner's and a carrier hop pays none of it — no canonical serialization (the `[02]-[INTEROP]` owner's `arrow_bytes` fold holds the whole-table key stream), no content-key mint (a consume-once capsule is never a key source, and a `wire_bytes` payload never is either). Rejected forms: an inline `ArrowCStream(capsule=, schema_repr=)` re-mint where `of` owns construction; a `read_all()` materialization inside `chunks`; a data-moving probe where `negotiate` reads the schema alone; a `child.name or ""` fallback keying every nameless child onto one roster slot; a top-level `nanoarrow.CDeviceArray` spelling where `nanoarrow.device` is the import home; a device-STREAM constructor claim no member backs; a pyarrow round-trip where the capsule crosses directly; a pyarrow `ipc.IpcWriteOptions(compression=)` twin beside `wire_bytes` — two transport serializations drift independently and the slim leg already owns the codec.

```python signature
class ArrowCStream(Struct, frozen=True):
    capsule: Any
    schema_repr: str

    @classmethod
    def of(cls, exporter: Any) -> "ArrowCStream":
        stream = nanoarrow.ArrayStream(exporter)
        schema_repr = repr(stream.schema)
        return cls(capsule=stream.__arrow_c_stream__(), schema_repr=schema_repr)

    @staticmethod
    def chunks(exporter: Any) -> Iterator[Any]:
        yield from nanoarrow.c_array_stream(exporter)

    @staticmethod
    def negotiate(exporter: Any) -> RuntimeRail[tuple[FieldShape, ...]]:
        return boundary(CARRIER_EXPORT, lambda: nanoarrow.ArrayStream(exporter).schema, catch=_CARRIER_RAISES).bind(
            lambda schema: traversed(Block.of_seq([_declared(ordinal, child) for ordinal, child in enumerate(schema.fields)])).map(tuple)
        )

    @staticmethod
    def device_of(exporter: Any) -> Any:
        return nanoarrow.device.c_device_array(exporter)


class WireCodec(StrEnum):
    LZ4 = "LZ4"
    ZSTD = "ZSTD"
    NONE = "none"


def _declared(ordinal: int, child: Any) -> RuntimeRail[FieldShape]:
    return (
        Ok(FieldShape(field=child.name, logical_type=str(child.type), nullable=child.nullable, source=ShapeSource.DECLARED))
        if child.name
        else Error(CARRIER_UNNAMED.raised(str(ordinal)))
    )


def wire_bytes(exporter: Any, codec: WireCodec = WireCodec.LZ4) -> RuntimeRail[bytes]:
    def emit() -> bytes:
        sink = io.BytesIO()
        write_ipc_stream(exporter, sink, compression=None if codec is WireCodec.NONE else codec.value)
        return sink.getvalue()

    return boundary(WIRE_CODEC, emit, catch=_wire_raises())


def wire_table(payload: bytes) -> RuntimeRail[Any]:
    def emit() -> Any:
        return read_ipc_stream(io.BytesIO(payload)).read_all()

    return boundary(WIRE_CODEC, emit, catch=_wire_raises())
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
