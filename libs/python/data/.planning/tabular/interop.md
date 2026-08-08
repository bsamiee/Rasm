# [PY_DATA_INTEROP]

`FrameInterop` translates any admitted native dataframe to any backend over `narwhals`, and `ArrowCStream` carries Arrow across the seam pyarrow-free over the Arrow C Data Interface — one owner over the two interchange hops. `FrameInterop` discriminates a single `Backend` axis bound to the live `narwhals.Implementation` vocabulary against one `_BACKEND` behavior table, so a new backend is one `Backend` row plus one `_BACKEND` row, never a parallel adapter family and never an `isinstance` dispatch over native frame types. This is the tier-0 base of the tabular plane: it imports nothing from `rasm.data`, and every folder composition edge points strictly down into it.

Its axis spans the narwhals lazy set — `POLARS`/`PANDAS`/`PYARROW`/`MODIN` eager, `DUCKDB`/`IBIS`/`DASK` lazy — each row's `eager` column selecting both the `from_native` intake level and the lowering head, so a lazy plan survives only into a lazy target and materializes on a lazy→eager hop by contract. `FieldShape` is DECLARED here — the `schema_of` minter's page — and `tabular/contract#ADMISSION` and `tabular/profile#PROFILE` import it strictly downward, never a back-edge. `arrow_bytes` is DECLARED here too: the folder's one canonical whole-table Arrow IPC serialization, imported downward by `tabular/columnar#SCAN`, every `tabular` receipt owner keying a frame, and the `gridded/ragged#RAGGED` `arrow`-sink, so one byte stream addresses every content-keyed frame in the package. `ArrowCStream.of` is the one carrier construction both `FrameInterop.c_stream` and the sibling `gridded/ragged#RAGGED` `RaggedArray.c_stream` compose. `cohort_bytes` is the public numeric-cohort projection the compute study emit resolves against, and `wire_bytes` is the transport-band compressed IPC leg whose payload never reaches an identity fold. `InteropReceipt` content-keys each hop through runtime `ContentIdentity` and streams through `ReceiptContributor`, carrying the same typed evidence the `columnar`/`contract` owners do.

## [01]-[INDEX]

- [02]-[INTEROP]: the backend-agnostic translation owner over the seven-row eager/lazy `Backend` axis and `_BACKEND` table, the locally-declared `FieldShape`, the null-mask schema fold, the folder's `arrow_bytes` serialization, the `cohort_bytes` numeric-cohort projection, and the content-keyed `InteropReceipt`.
- [03]-[CARRIER]: the pyarrow-free Arrow C Data Interface carrier — `ArrowCStream.of` construction, `chunks` streaming consumption, `negotiate` schema-only folding, `device_of` the C Device array row, and the `wire_bytes` transport-band IPC codec over the closed `WireCodec` vocabulary.

## [02]-[INTEROP]

- Owner: `DoeDataset` — the `csharp:Rasm.Compute/Solver/sweep` training-corpus wire admission (`ContentKey`/`Axes`/`Objectives`/`Strategy`/`Points`/row-major `Coordinates`/`Responses`/`At`), its `frame` fold the graduation loop's fit ingress keyed by the wire's own content key. `FrameInterop` — the one translation owner over `narwhals`, discriminating a `Backend` `StrEnum` whose value IS the `narwhals.Implementation` member value, so `Backend.implementation` resolves through `Implementation.from_backend` and the axis carries no `ARROW`-named drift (narwhals names `PYARROW`, value `'pyarrow'`). `narwhals.from_native` is the one intake; the lowering target is one `_BACKEND` row. `FieldShape` is declared here and `resolve` is its structural-breach fold the `tabular/contract#ADMISSION` gate reads. `arrow_bytes` is declared here as the folder's one whole-table IPC serialization — a schema message ahead of one combined batch — so every content key over a table payload folds identical bytes wherever it is minted. `InteropReceipt` is the typed hop receipt satisfying `ReceiptContributor`.
- Cases: `Backend` is the closed axis; `_BACKEND` pairs each row with its `_Lowering` — the `lower` head and the `eager` flag — so admission level and lowering head are two columns on one row, never parallel switch arms.
  - A lazy-scan reader is deliberately NOT a column: a backend admits many formats, so a per-engine reader name is incoherent — `tabular/columnar#SCAN` resolves its reader off `DatasetKind`, a disjoint axis this owner never duplicates.
  - One `eager` column serves BOTH source and target for distinct concerns: the SOURCE flag selects `_admit`'s `from_native(eager_only=)` intake, the TARGET flag selects the frame the lowering head receives.
  - Load-bearing invariant: `to_polars`/`to_pandas`/`to_arrow` are `nw.DataFrame`-only, so a lazy-source→eager-target lowering MUST collect first — `_lowered` hands the row's `lower` head `eager if row.eager else admitted` off the existing column. A lazy plan survives only into a lazy target; a lazy→eager hop materializes by contract, and the receipt counts and content-key bytes read the `_eager` projection so the null-mask always reads a materialized frame while a lazy-target output stays lazy.
  - `_lowered` and `_admit` both key `_BACKEND[...]` through the total `Map.__getitem__` — every `Backend` member carries a row by construction, never an inline per-backend `match` and never a `try_find`/`assert_never(target)` fold lying about totality over a live `StrEnum`.
- Entry: `translate` lifts through `_admit`, lowers once into one `_Lowered` carry, and binds `ContentIdentity.of` over the lowered frame's canonical Arrow bytes into a `RuntimeRail[FrameTranslation]` — the lowering wrapped in one `boundary(...)` so a terminal raise lifts to `BoundaryFault` exactly once and the key threads through `.bind`/`.map`, never a second fault fence. `schema_of` reads the agnostic `nw.Schema` through `collect_schema()` and the per-column null count through `null_count().item(0, name)` — never a `.to_native()[name][0]` subscript that breaks on a `pyarrow.Table` — so nullability is the observed mask, not a dtype-kind inference. `namespace` resolves through `Implementation.to_native_namespace`. `c_stream` lowers to a `pyarrow.Table` through `_eager(self._admit(frame)).to_arrow()` before `ArrowCStream.of` — the one `__arrow_c_stream__`-exporter every backend reaches — never the raw native frame a `pandas`/`modin` source does not reliably export. No hop carries a `stamina` retry: a `to_*`/`to_native` projection is a pure in-memory transform, not a transient-I/O hop.
- Receipt: `translate` keys one `InteropReceipt` by `ContentIdentity.of("interop", lowered.ipc)` over the `arrow_bytes` stream this owner declares, carrying source/target backend and row/column counts, never a path-string key. `ArrowCStream` stays interchange-only and NEVER a key byte source: a consume-once capsule is not a deterministic byte stream, so every key in the folder rides `arrow_bytes` instead.
- Law: `cohort_bytes` is the folder's ONE numeric-cohort projection — axis and objective names beside row-major coordinate and response vectors build the Arrow training table through the same strided fold `DoeDataset.frame` composes, then ride `arrow_bytes` — so a sibling emits canonical cohort bytes without importing a single pyarrow construction member, and the compute study emit resolves against this public fold. Extent admission is exact: a coordinate or response vector off its `points x names` extent refuses on the rail before any column builds.
- Growth: a new backend is one `Backend` row naming its `Implementation` member plus one `_BACKEND` `(lower, eager)` row (`PYSPARK`/`SQLFRAME` land this way); a new interchange protocol is one method; a new admission level is one `eager` value; a new structural attribute is one `FieldShape` column read once by `schema_of`; a new receipt slot is one `InteropReceipt` field; a new content-keying consumer imports `arrow_bytes` and adds nothing; a new cohort column family is one name tuple on the same `cohort_bytes` call; zero new surface.
- Boundary: no compute (the numeric and labelled-array ownership stays in `compute`), no durable store, no query rail (`tabular/query#QUERY` owns the relational plane), no lazy-scan execution (`tabular/columnar#SCAN` owns the `register_io_source` pushdown); `narwhals` owns only the frame-translation hop and the schema fold; `DoeDataset` is the WIRE ADMISSION only — the fit itself is the `compute` companion's and the graduated ONNX crosses back over `GraduationEvidence`, never a training loop here. Rejected forms: a per-backend `PolarsAdapter`/`PandasAdapter` trio or `isinstance` dispatch where one `_BACKEND` row owns lowering; a `FieldShape` re-declared on a consumer page where this minter owns it; a second `collect_schema()` path beside `schema_of`; a bare receipt-less lowering entrypoint beside `translate`, whose `FrameTranslation` already carries the lowered frame with its key; a second whole-table serialization beside `arrow_bytes` — `nanoarrow.ArrayStream(table).read_all().serialize()` is the falsified twin, emitting a bare batch message with NO schema, so `pyarrow.ipc.open_stream` refuses those bytes outright and two frames differing in schema alone mint one identical key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Buffer, Callable, Iterable, Iterator
from enum import StrEnum
from typing import Any, Final

import nanoarrow
import nanoarrow.device
import narwhals as nw
from beartype import beartype
from expression.collections import Map
from msgspec import Struct

# the whole-table IPC fold and the DoeDataset build are the two pyarrow legs this page holds; the module-scope
# `lazy` bind states that deferral as law where a function-local import carrying a suppression states it nowhere.
lazy import pyarrow as pa

from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------


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


# --- [MODELS] ---------------------------------------------------------------------------


# `FieldShape` is the structural field this minter DECLARES; `schema_of` builds it off the live null-mask.
class FieldShape(Struct, frozen=True):
    field: str
    logical_type: str
    nullable: bool = True
    source_evidence: str = ""

    def resolve(self, live: "Map[str, FieldShape]") -> str:
        found = live.get(self.field)
        match found:
            case None:
                return f"{self.field}:absent"
            case FieldShape(logical_type=actual) if actual != self.logical_type:
                return f"{self.field}:{self.logical_type}!={actual}"
            case FieldShape(nullable=True) if not self.nullable:
                return f"{self.field}:nullable"
            case _:
                return ""


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
        # no `Metrics.record` here: a translation hop moves no measured resource — it reads one in-memory frame and
        # writes another — so this receipt carries evidence alone and the metric spine stays free of a series whose
        # every value is the calling owner's own span already timing it, the same stated abstention `tabular/cost#COST`
        # carries. `domain`/`kind`/`key` are the lifted evidence contract the `tabular/lakehouse#LAKEHOUSE` residence
        # reads, and `domain` is that plane's partition column — a contributor omitting it lands every translation row
        # in one nameless partition no predicate ever prunes. The TARGET backend keys the row: a translation is named
        # by where it lands, and the source survives in the subject beside it.
        yield Receipt.of(
            "frame-interop",
            (
                "emitted",
                f"{self.source}->{self.target}",
                {"domain": "interop", "kind": self.target, "key": self.content_key.hex, "rows": self.rows, "columns": self.columns},
            ),
        )


# backend-native object the `Map[Backend, _Lowering]` dispatch erases per row; `Any` is the honest wire floor, not weak typing.
class FrameTranslation(Struct, frozen=True):
    frame: Any
    receipt: InteropReceipt


# lowered once so `translate` never re-lowers per rail arm: native frame, agnostic frame (receipt counts), IPC bytes (key).
class _Lowered(Struct, frozen=True):
    frame: Any
    agnostic: nw.DataFrame[Any]
    ipc: Buffer


# --- [TABLES] ---------------------------------------------------------------------------

# one row per backend: the lowering head plus the eager/lazy admission flag.
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


# csharp:Rasm.Compute/Solver/sweep DoeDataset wire — the graduation loop's training leg (C# sweep -> DoeDataset ->
# Python fit -> graduated ONNX -> neural-field surrogate). Coordinates are row-major (points x axes) and
# Responses row-major (points x objectives); admission is exact-extent, and the frame keys by the wire's OWN
# content key so the fit's provenance joins the C# receipt spine — never a re-hash.
class DoeDataset(Struct, frozen=True):
    content_key: str
    axes: tuple[str, ...]
    objectives: tuple[str, ...]
    strategy: str
    points: int
    coordinates: tuple[float, ...]
    responses: tuple[float, ...]
    at: str

    def frame(self) -> RuntimeRail["Any"]:
        """One Arrow training table: axis columns then objective columns, rows the design points — the shared cohort fold."""
        return boundary(f"interop.doe.{self.content_key}", lambda: _cohort_table(self.axes, self.objectives, self.coordinates, self.responses, self.points))


class FrameInterop(Struct, frozen=True):
    source: Backend

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, source: Backend) -> "FrameInterop":
        return cls(source=source)

    def translate(self, frame: Any, target: Backend) -> RuntimeRail[FrameTranslation]:
        return boundary(f"interop.translate.{self.source}->{target}", lambda: self._lowered(frame, target)).bind(
            lambda lowered: ContentIdentity.of("interop", lowered.ipc).map(
                lambda key: FrameTranslation(frame=lowered.frame, receipt=InteropReceipt.of(self.source, target, lowered.agnostic, key))
            )
        )

    def schema_of(self, frame: Any) -> RuntimeRail[tuple[FieldShape, ...]]:
        return boundary(f"interop.schema.{self.source}", lambda: _shapes(_eager(self._admit(frame))))

    def namespace(self) -> RuntimeRail[Any]:
        return boundary(f"interop.namespace.{self.source}", self.source.implementation.to_native_namespace)

    def c_stream(self, frame: Any) -> RuntimeRail["ArrowCStream"]:
        return boundary(f"interop.c_stream.{self.source}", lambda: ArrowCStream.of(_eager(self._admit(frame)).to_arrow()))

    def _admit(self, frame: Any) -> nw.DataFrame[Any] | nw.LazyFrame[Any]:
        return nw.from_native(frame, eager_only=_BACKEND[self.source].eager)

    def _lowered(self, frame: Any, target: Backend) -> "_Lowered":
        admitted = self._admit(frame)
        eager = _eager(admitted)
        ipc = arrow_bytes(eager.to_arrow())
        # reuse the collected `eager` for an eager target; a lazy target keeps `admitted` to preserve the deferred plan.
        row = _BACKEND[target]
        return _Lowered(frame=row.lower(eager if row.eager else admitted), agnostic=eager, ipc=ipc)


# --- [OPERATIONS] -----------------------------------------------------------------------


def arrow_bytes(table: "pa.Table") -> Buffer:
    # canonical whole-table IPC STREAM bytes — the ONE serialization the `ContentIdentity` `whole` arm folds and every
    # foreign reader opens, declared at the folder floor so the scan plane, the commit plane, the cost ledger, and the
    # ragged Arrow sink all key one byte stream. `RecordBatch.serialize()` and the `nanoarrow.ArrayStream(...).read_all()
    # .serialize()` twin are the rejected forms: each emits a bare batch message carrying NO schema message, so
    # `ipc.open_stream` refuses it outright, only `read_record_batch(obj, schema)` decodes it, and two frames differing
    # in schema alone key IDENTICALLY. `combine_chunks` coalesces every column first so the stream carries one batch;
    # an empty table keys off `b""`.
    sink = pa.BufferOutputStream()
    with pa.ipc.new_stream(sink, table.schema) as writer:
        writer.write_table(table.combine_chunks())
    return sink.getvalue()


def _cohort_table(
    axes: tuple[str, ...], objectives: tuple[str, ...], coordinates: "Iterable[float]", responses: "Iterable[float]", points: int
) -> "pa.Table":
    # ONE strided cohort build: row-major (points x names) vectors stride into named columns, axis columns ahead
    # of objective columns; exact-extent admission refuses before any column builds. `DoeDataset.frame` and the
    # public `cohort_bytes` projection both fold through here, so the C# sweep wire and the compute study emit
    # produce one table shape and one byte stream for identical content.
    xs, ys = tuple(coordinates), tuple(responses)
    if len(xs) != points * len(axes) or len(ys) != points * len(objectives):
        msg = f"<cohort-extent:{len(xs)}x{len(ys)}!={points}x{len(axes)}x{len(objectives)}>"
        raise ValueError(msg)
    columns = {axis: pa.array(xs[index :: len(axes)]) for index, axis in enumerate(axes)} | {
        objective: pa.array(ys[index :: len(objectives)]) for index, objective in enumerate(objectives)
    }
    return pa.table(columns)


def cohort_bytes(
    axes: tuple[str, ...], objectives: tuple[str, ...], coordinates: "Iterable[float]", responses: "Iterable[float]", points: int
) -> RuntimeRail[Buffer]:
    # the public cohort-to-bytes projection: a sibling emits canonical Arrow cohort bytes with zero pyarrow
    # construction imports; the caller keys the returned bytes through its own `ContentIdentity` mint.
    return boundary("interop.cohort", lambda: arrow_bytes(_cohort_table(axes, objectives, coordinates, responses, points)))


def _eager(frame: nw.DataFrame[Any] | nw.LazyFrame[Any]) -> nw.DataFrame[Any]:
    return frame.collect() if isinstance(frame, nw.LazyFrame) else frame


def _shapes(frame: nw.DataFrame[Any]) -> tuple[FieldShape, ...]:
    schema = frame.collect_schema()
    nulls = frame.null_count()
    return tuple(
        FieldShape(field=name, logical_type=str(dtype), nullable=nulls.item(0, name) > 0, source_evidence=f"narwhals:{dtype!r}")
        for name, dtype in schema.items()
    )
```

## [03]-[CARRIER]

- Owner: `ArrowCStream` — the pyarrow-free Arrow C Data Interface zero-copy carrier over the PyCapsule stream protocol; one frozen `Struct` carrying the capsule plus the schema repr, never a per-producer wrapper. `ArrowCStream.of` wraps any `__arrow_c_stream__`-exporter through `nanoarrow.ArrayStream` and reads the `.schema` the low-level `c_array_stream` does not expose, so polars/pandas/duckdb and the `gridded/ragged#RAGGED` `ak.to_arrow_table` `pyarrow.Table` (the native `__arrow_c_stream__`-exporter, not `ak.to_arrow` which exports only `__arrow_c_array__`) exchange Arrow without pyarrow. Three siblings complete the depth: `chunks` consumes chunk-by-chunk through `nanoarrow.c_array_stream` with no full materialization; `negotiate` reads the schema WITHOUT moving a batch and folds the child fields into a `FieldShape` tuple; `device_of` is the C Device Data Interface array row over `nanoarrow.device.c_device_array`, array-level by construction since no device-array-STREAM constructor exists.
- Entry: `ArrowCStream.of(exporter)` is the one construction both this owner's `FrameInterop.c_stream` and the sibling `gridded/ragged#RAGGED` `RaggedArray.c_stream` compose. It reads the `ArrayStream.schema` repr FIRST, then exports the `__arrow_c_stream__` capsule — the order load-bearing because `__arrow_c_stream__()` moves the Arrow C stream out of the wrapper (a stream capsule is consume-once), so the schema repr is captured before the export and the carried capsule is a single-handoff transport, never re-read and never a second `nanoarrow.ArrayStream` wrap at the consumer seam.
- Auto: `chunks` yields the C-level `CArray` chunk holders directly off the `CArrayStream` iterator so back-pressure lives at the chunk grain — the stream owns a C resource and closes on iterator exhaustion. `negotiate` reads `Schema.fields` (each child carrying `name`/`type`/`nullable`) so it is total over the struct-top schema. `device_of` resolves the device kind off the held `CDeviceArray` — `DEVICE_CPU` for a host-resident exporter, a non-CPU device surfacing as the holder's own attribute rather than a silent copy to host. `arro3-core` is the alternate zero-copy PyCapsule reader the consumer side crosses on, both reading the canonical Arrow C-data layout without the Arrow C++ source build.
- Law: `wire_bytes` is the folder's one TRANSPORT-band IPC serialization — `arro3.io.write_ipc_stream(data, sink, compression=)` over the closed `WireCodec` vocabulary (`LZ4`/`ZSTD`/`NONE`), pyarrow-free on the slim carrier leg, and `wire_table` its `read_ipc_stream(...).read_all()` inverse — for frames crossing to siblings, C# peers, or object storage where wire volume matters. It NEVER reaches an identity fold: content keys derive from the `[02]` `arrow_bytes` fold alone, pinned uncompressed by the folder key law, because the arro3 stream is not byte-identical to the canonical serialization even at `NONE` — a `wire_bytes` payload keyed anywhere forks one frame into two keys. Every codec's framing is standard Arrow IPC body compression, so a pyarrow or C# Arrow reader opens the stream with zero re-encode.
- Packages: `nanoarrow` (`ArrayStream`, `c_array_stream` the chunked iterator, `Schema.fields`/`name`/`type`/`nullable`), `nanoarrow.device` (`c_device_array`/`CDeviceArray`/`DeviceType`/`DEVICE_CPU` — the C Device Data Interface submodule, no top-level spellings), `arro3-core` (the alternate PyCapsule reader), `arro3-io` (`write_ipc_stream`/`read_ipc_stream` the transport-band codec pair), `msgspec` (`Struct` the frozen owner).
- Growth: a new PyCapsule-exporting producer is admitted free by `of`/`chunks`/`negotiate` over its `__arrow_c_stream__`/`__arrow_c_schema__` with zero carrier change; a non-CPU device row is one `DeviceType` the held `CDeviceArray` already reports; a new carried evidence field is one `ArrowCStream` column; a new transport codec is one `WireCodec` member; zero new surface.
- Boundary: no Arrow compute, no pyarrow on the carrier path — the capsule crosses through `nanoarrow` alone and the wire codec through `arro3.io`, so the module's `lazy pyarrow` bind stays the `[02]` owner's and a carrier hop pays none of it — no canonical serialization (the `[02]-[INTEROP]` owner's `arrow_bytes` fold holds the whole-table key stream), no content-key mint (a consume-once capsule is never a key source, and a `wire_bytes` payload never is either). Rejected forms: an inline `ArrowCStream(capsule=, schema_repr=)` re-mint where `of` owns construction; a `read_all()` materialization inside `chunks`; a data-moving probe where `negotiate` reads the schema alone; a top-level `nanoarrow.CDeviceArray` spelling where `nanoarrow.device` is the import home; a device-STREAM constructor claim no member backs; a pyarrow round-trip where the capsule crosses directly; a pyarrow `ipc.IpcWriteOptions(compression=)` twin beside `wire_bytes` — two transport serializations drift independently and the slim leg already owns the codec.

```python signature
class ArrowCStream(Struct, frozen=True):
    capsule: Any
    schema_repr: str

    @classmethod
    def of(cls, exporter: Any) -> "ArrowCStream":
        # read `schema` BEFORE exporting: `__arrow_c_stream__()` moves the stream out (consume-once) — schema captured first, capsule single-handoff.
        stream = nanoarrow.ArrayStream(exporter)
        schema_repr = repr(stream.schema)
        return cls(capsule=stream.__arrow_c_stream__(), schema_repr=schema_repr)

    @staticmethod
    def chunks(exporter: Any) -> Iterator[Any]:
        # chunked over the C-level stream: each `CArray` crosses without whole-stream materialization —
        # `read_all()` here is the deleted form.
        yield from nanoarrow.c_array_stream(exporter)

    @staticmethod
    def negotiate(exporter: Any) -> tuple[FieldShape, ...]:
        # schema-only, no batch moves; nullability is the DECLARED flag, so the observed mask comes from `schema_of`.
        schema = nanoarrow.ArrayStream(exporter).schema
        return tuple(
            FieldShape(field=child.name or "", logical_type=str(child.type), nullable=child.nullable, source_evidence="arrow-c-schema")
            for child in schema.fields
        )

    @staticmethod
    def device_of(exporter: Any) -> Any:
        # C Device Data Interface array row: a `CDeviceArray` with `__arrow_c_device_array__`, host resolving `DEVICE_CPU`; array-level only, no device-stream constructor.
        return nanoarrow.device.c_device_array(exporter)


class WireCodec(StrEnum):
    # transport-band body compression; the member value IS the `arro3.io` compression spelling, `NONE` mapping to None.
    LZ4 = "LZ4"
    ZSTD = "ZSTD"
    NONE = "none"


def wire_bytes(exporter: Any, codec: WireCodec = WireCodec.LZ4) -> RuntimeRail[bytes]:
    # transport-band ONLY: standard Arrow IPC body compression a pyarrow or C# Arrow reader opens directly;
    # never a key source — content identity stays the uncompressed `arrow_bytes` fold by the folder key law.
    def emit() -> bytes:
        import io  # ruff:ignore[import-outside-top-level]

        from arro3.io import write_ipc_stream  # ruff:ignore[import-outside-top-level]

        sink = io.BytesIO()
        write_ipc_stream(exporter, sink, compression=None if codec is WireCodec.NONE else codec.value)
        return sink.getvalue()

    return boundary(f"interop.wire.{codec.name.lower()}", emit)


def wire_table(payload: bytes) -> RuntimeRail[Any]:
    # the inverse leg: decompression rides the IPC reader off the stream's own body-compression header, so one
    # entry serves every `WireCodec` member and the caller never re-states which codec the producer chose.
    def emit() -> Any:
        import io  # ruff:ignore[import-outside-top-level]

        from arro3.io import read_ipc_stream  # ruff:ignore[import-outside-top-level]

        return read_ipc_stream(io.BytesIO(payload)).read_all()

    return boundary("interop.wire.read", emit)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
