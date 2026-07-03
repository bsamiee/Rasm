# [PY_DATA_INTEROP]

The backend-agnostic frame-translation owner plus the pyarrow-free Arrow C Data Interface carrier as one owner over `narwhals`. `FrameInterop` discriminates one `Backend` `StrEnum` axis bound to the live `narwhals.Implementation` vocabulary, lifting any admitted native frame into the agnostic `nw.DataFrame`/`nw.LazyFrame` and lowering it to the requested backend through one `_BACKEND` behavior table pairing each backend tag with its lowering callable and its eager/lazy admission level — so a new backend is one `Backend` row plus one `_BACKEND` row, never a parallel `PolarsAdapter`/`PandasAdapter`/`ArrowAdapter` family and never an `isinstance` dispatch over native frame types. `FrameInterop.schema_of` reads the backend-agnostic `nw.Schema` and the live per-column null-mask off the one frame and folds both into `contract#ADMISSION` `FieldShape`s; `FrameInterop.translate` lowers exactly once into one `_Lowered` carry and binds the railed `ContentIdentity.of` into a `RuntimeRail[FrameTranslation]`; `FrameInterop.c_stream` exports the zero-copy Arrow C stream capsule through `ArrowCStream.of` over `nanoarrow`/`arro3-core`, the one carrier construction every `__arrow_c_stream__`-exporting engine speaks. `InteropReceipt` content-keys every translation hop through runtime `ContentIdentity` and streams through `ReceiptContributor`, so the interop hop carries the same typed evidence the `columnar`/`contract` owners do under the runtime `boundary`/`Receipt` observability stack.

## [01]-[INDEX]

- [01]-[INTEROP]: the dataframe-agnostic backend-translation owner over one `Backend` axis and `_BACKEND` table, the null-mask-accurate schema fold, and the content-keyed `InteropReceipt`.
- [02]-[CARRIER]: the pyarrow-free Arrow C Data Interface carrier, `ArrowCStream.of` the one construction entrypoint over any PyCapsule-exporting object.

## [02]-[INTEROP]

- Owner: `FrameInterop` — the one backend-agnostic translation owner over `narwhals`, discriminating one `Backend` `StrEnum` axis whose value is the exact `narwhals.Implementation` member value (`POLARS`/`PANDAS`/`PYARROW` the eager admitted rows, `MODIN` the eager pandas-distributed row, `DUCKDB` the lazy-relation row sibling owners admit through the same axis), each row bound through `Implementation.from_backend(self.value)` so the axis carries no `ARROW`-named drift — narwhals carries `PYARROW` (value `'pyarrow'`), so `Backend.PYARROW.value` and `Implementation.PYARROW.name.lower()` agree. `narwhals.from_native` is the one intake lifting any admitted native frame into the agnostic `nw.DataFrame`/`nw.LazyFrame`; the lowering target is one `_BACKEND` row, never branched into parallel adapter classes. `InteropReceipt` is the typed interop receipt — source backend, target backend, row/column counts, content key — satisfying the runtime `ReceiptContributor` Protocol, the same evidence shape the sibling `columnar#SCAN` `QueryReceipt` carries.
- Cases: `Backend` is the one closed backend axis — every row maps to a concrete `narwhals.Implementation` member through `Backend.implementation` (`Implementation.from_backend(self.value)`), so the axis never carries a non-implementation value and `namespace` resolves the native module off that member. `_BACKEND` is the one behavior table pairing each `Backend` with its two-column `_Lowering` row: the `lower` callable projecting the agnostic frame to the native backend (`DataFrame.to_polars`/`to_pandas`/`to_arrow` the eager rows, `to_native` the `DUCKDB`/`MODIN` rows whose native object is the lowered target) and the `eager` admission flag selecting `from_native(..., eager_only=True)` for the materialized rows versus the bare `from_native` for the lazy `DUCKDB` relation — so the source admission level and the lowering head are two columns on one row, never two parallel switch arms and never a module-private dict covering a silent subset. The lazy-scan reader is NOT a column here: a backend admits many formats (`polars` scans CSV/Parquet/IPC/NDJSON alike), so a single per-engine reader name is incoherent; the `columnar#SCAN` plane resolves its reader off `DatasetKind` (format), a disjoint axis this owner never duplicates. The `eager` column is read on BOTH the source and the target row, and the two reads are distinct concerns: the SOURCE `eager` flag selects `_admit`'s `from_native(..., eager_only=)` intake level, while the TARGET `eager` flag selects which frame the lowering head receives — `_lower` passes the `_eager`-collected `nw.DataFrame` to an eager target (`POLARS`/`PANDAS`/`PYARROW`/`MODIN`, whose `to_*`/`to_native` heads are `DataFrame`-shaped) and the bare `admitted` frame to the lazy `DUCKDB` target (whose `to_native` head is total over `DataFrame | LazyFrame` and preserves the deferred relation on the output). This is the load-bearing correctness invariant: `to_polars`/`to_pandas`/`to_arrow` are `nw.DataFrame`-only, so lowering a lazy `DUCKDB`-source admission to an eager target MUST collect first — `_lower(admitted, target)` reading `_eager(admitted) if _BACKEND[target].eager else admitted` does exactly this off the existing column, never an `admitted.to_polars()` that crashes a `nw.LazyFrame` lacking the eager projector and never a per-backend frame-selection branch. The lazy plan survives only into the lazy target; a `DUCKDB`→eager lowering materializes the relation by contract, while a `DUCKDB`→`DUCKDB` lowering keeps it deferred. The receipt's row/column count and the content-key IPC bytes read the `_eager` projection — `nw.LazyFrame.collect()` materializing a lazy `DUCKDB` admission to a `nw.DataFrame` once before `null_count()`/`to_arrow()` (both `DataFrame`-only), so the schema null-mask and the canonical bytes always read a materialized frame while a lazy-target lowered output stays lazy. `_lower` reads the target `_BACKEND` row through the total `Map.__getitem__` and applies its `lower` head to the column-selected frame — the same `_BACKEND[...]` total read `_admit` keys the source row through, since every `Backend` member carries a row by construction, never an inline `match` per backend and never a `try_find`/`assert_never(target)` fold that lies about totality over a live `StrEnum` value.
- Entry: `FrameInterop.translate` lifts a native frame through `_admit` (one `from_native` call whose `eager_only` rides the source `_BACKEND.eager` flag), lowers to the requested `Backend` through `_lower` (the target-`eager`-column frame selection) exactly once into one `_Lowered` carry, and binds the railed `ContentIdentity.of` over the lowered frame's canonical Arrow bytes into a `RuntimeRail[FrameTranslation]` carrying the lowered native frame plus its `InteropReceipt` — the lowering wrapped in one `boundary(f"interop.translate.{source}->{target}", thunk)` so the terminal raise lifts to a `BoundaryFault` exactly once and the railed key threads through `.bind`/`.map` rather than a second fault fence; `FrameInterop.schema_of` reads the backend-agnostic `nw.Schema` through `collect_schema()`, reads the per-column null-count off the live frame through the agnostic `null_count().item(0, name)` single-cell read (never a `.to_native()[name][0]` subscript that drops to a backend-specific frame access and breaks on a `pyarrow.Table`), and folds both into a tuple of `FieldShape`s without touching any backend API directly, the nullability the observed mask (`item(0, name) > 0`) rather than a dtype-kind inference; `FrameInterop.namespace` resolves the native backend namespace module through `Implementation.to_native_namespace`; `FrameInterop.c_stream` admits the native source through `_admit`, collects any lazy admission through `_eager`, and lowers to a `pyarrow.Table` through `to_arrow()` before handing it to `ArrowCStream.of` — the carrier consumes a `narwhals`-lowered `pyarrow.Table` (the one `__arrow_c_stream__`-exporter every backend reaches through the same `to_arrow()` the IPC path already runs), never the raw native frame whose `__arrow_c_stream__` a `pandas`/`modin` source does not reliably export. Every entrypoint returns a `RuntimeRail`, the `boundary` lifting a fault exactly once into `BoundaryFault` and the receipt riding the `Ok` arm; the lowering hop carries no `stamina` retry because a backend `to_*`/`to_native` projection is a pure in-memory CPU transform, not a transient-I/O hop the resilience owner's classes model.
- Receipt: `FrameInterop.translate` folds one `InteropReceipt` keyed by `ContentIdentity.of("interop", lowered.ipc)` over the agnostic frame's canonical Arrow IPC bytes (the `nanoarrow.ArrayStream(...).read_all().serialize()` serialization the `CARRIER` owner and the sibling `gridded/ragged#RAGGED` `arrow`-sink share) — the source-backend, target-backend, row-count, and column-count evidence, never a path-string key and never a generic receipt; `InteropReceipt.contribute` yields one emitted-phase `Receipt.of("frame-interop", ("emitted", subject, facts))` row through the runtime two-argument `Receipt.of(owner, evidence)` form, satisfying the `ReceiptContributor.contribute -> Iterable[Receipt]` Protocol the sibling `egress#EGRESS` `EgressReceipt.contribute` and the `evidence/identity#IDENTITY` `SeedReproduction.contribute` yield through, never a bare single `Receipt` against the streaming Protocol; a re-translation of an unchanged frame between the same backends reuses its key untouched, the interop hop's evidence riding the one receipt stream the `observability/metrics#METRIC`/`execution/lanes#LANE` fold reads alongside the `columnar` scan receipt, never replacing the typed `QueryReceipt`. The structlog trace context and the OTLP span annotation ride the runtime `boundary`/`Receipt` owners — the `faults#FAULT` `_convert` records the fault on whatever span is active (a no-op when none records, since the interop hop mints no span of its own) and the `receipts#RECEIPT` chain injects `trace_id`/`span_id` — so the interop hop carries the full observability stack without minting a second tracer or logger.
- Growth: a new backend is one `Backend` row whose value names its `narwhals.Implementation` member plus one `_BACKEND` row binding its `(lower, eager)` pair; a new interchange protocol is one method on the same owner; a new admission level is one `eager` column value on the existing row; a new receipt slot is one field on `InteropReceipt`; zero new surface. The per-backend adapter family (`PolarsAdapter`/`PandasAdapter`/`ArrowAdapter`) collapses into one owner with a `Backend` axis and a `_BACKEND` table.
- Boundary: no compute (the numeric trio and labelled-array ownership stays in `compute`), no durable store, no engine-specific query rail (`tabular/query#QUERY` owns the relational plane), no lazy-scan execution (`columnar#SCAN` owns the `register_io_source` pushdown); `narwhals` owns only the agnostic frame-translation hop and the schema fold. A per-backend adapter trio, a hand-rolled `isinstance` dispatch over native frame types, a per-backend `match` arm in `translate`/`lower` where one `_BACKEND` row owns the lowering, a second inline `collect_schema()` schema-derivation path beside `schema_of`, a dtype-kind nullability inference where the live `null_count()` mask owns it, a `null_count().to_native()[name][0]` backend-specific subscript where the agnostic `null_count().item(0, name)` read owns the per-cell null-count, a `try_find`/`assert_never(target)` `_lower` fold lying about totality over a live `Backend` value where the total `_BACKEND[target]` keyed read owns the dispatch, an `admitted.to_polars()`/`to_pandas()`/`to_arrow()` lowering a `nw.LazyFrame` (`DUCKDB`-source admission) to an eager target where `_lower` collects through `_eager` first off the target row's `eager` column (the `to_*` heads being `nw.DataFrame`-only), a `c_stream` handing the raw native source frame to `ArrowCStream.of` where the source's `__arrow_c_stream__` a `pandas`/`modin` frame does not reliably export and the `_eager(self._admit(frame)).to_arrow()` lowering produces the universal capsule, a `contribute` returning a bare single `Receipt` against the `ReceiptContributor.contribute -> Iterable[Receipt]` Protocol, an `@override`-decorated `contribute` where the method implements a `Protocol` rather than overriding a base, an `ARROW`-named lower path contradicting the `narwhals.Implementation.PYARROW` member, a `from __future__ import annotations`, a `from builtins import frozendict` table where the `expression` `Map` owns the behavior rows, an interop hop with no `ContentIdentity` key or `ReceiptContributor` receipt where the sibling owners all carry one, and an inline `ArrowCStream(capsule=, schema_repr=)` re-mint where `ArrowCStream.of` owns the construction are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Any, Final

import nanoarrow
import narwhals as nw
from beartype import beartype
from expression.collections import Map
from msgspec import Struct

from rasm.data.tabular.contract import FieldShape
from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------


class Backend(StrEnum):
    POLARS = "polars"
    PANDAS = "pandas"
    PYARROW = "pyarrow"
    MODIN = "modin"
    DUCKDB = "duckdb"

    @property
    def implementation(self) -> nw.Implementation:
        return nw.Implementation.from_backend(self.value)


# --- [MODELS] ---------------------------------------------------------------------------


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
            "frame-interop", ("emitted", f"{self.source}->{self.target}", {"rows": self.rows, "columns": self.columns, "key": self.content_key.hex})
        )


# `frame` is the backend-native object — `polars.DataFrame`/`pandas.DataFrame`/`pyarrow.Table`/
# duckdb relation — whose concrete type the banned module-top imports forbid naming and the
# `Map[Backend, _Lowering]` dispatch erases per row; `Any` is the honest wire floor, not weak typing.
class FrameTranslation(Struct, frozen=True):
    frame: Any
    receipt: InteropReceipt


# lowered exactly once: the native target frame, the agnostic frame the receipt counts, and
# the canonical Arrow IPC bytes the content key reads — so `translate` never re-lowers per rail arm.
class _Lowered(Struct, frozen=True):
    frame: Any
    agnostic: nw.DataFrame[Any]
    ipc: bytes


# --- [TABLES] ---------------------------------------------------------------------------

# one behavior row per backend: the lowering head and the eager/lazy admission flag. DUCKDB
# lowers to its native relation through `to_native` and admits lazy (its deferred plan survives);
# the materialized rows lower through their named `to_*` projector and admit eager.
_BACKEND: Final[Map[Backend, _Lowering]] = Map.of_seq([
    (Backend.POLARS, _Lowering(lambda f: f.to_polars(), True)),
    (Backend.PANDAS, _Lowering(lambda f: f.to_pandas(), True)),
    (Backend.PYARROW, _Lowering(lambda f: f.to_arrow(), True)),
    (Backend.MODIN, _Lowering(lambda f: f.to_native(), True)),
    (Backend.DUCKDB, _Lowering(lambda f: f.to_native(), False)),
])

# --- [SERVICES] -------------------------------------------------------------------------


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

    def lower(self, frame: Any, target: Backend) -> Any:
        return self._lower(self._admit(frame), target)

    def _admit(self, frame: Any) -> nw.DataFrame[Any] | nw.LazyFrame[Any]:
        return nw.from_native(frame, eager_only=_BACKEND[self.source].eager)

    def _lower(self, admitted: nw.DataFrame[Any] | nw.LazyFrame[Any], target: Backend) -> Any:
        row = _BACKEND[target]
        return row.lower(_eager(admitted) if row.eager else admitted)

    def _lowered(self, frame: Any, target: Backend) -> "_Lowered":
        admitted = self._admit(frame)
        eager = _eager(admitted)
        ipc = nanoarrow.ArrayStream(eager.to_arrow()).read_all().serialize()
        # reuse the already-collected `eager` for an eager target (no second `.collect()` of a
        # lazy DUCKDB relation); the lazy DUCKDB target keeps `admitted` so `to_native` preserves it.
        row = _BACKEND[target]
        return _Lowered(frame=row.lower(eager if row.eager else admitted), agnostic=eager, ipc=ipc)


# --- [OPERATIONS] -----------------------------------------------------------------------


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

- Owner: `ArrowCStream` — the pyarrow-free Arrow C Data Interface zero-copy carrier over the Arrow PyCapsule stream protocol; `ArrowCStream.of` the one construction classmethod over any `__arrow_c_stream__`-exporting object, wrapping it through `nanoarrow.ArrayStream` and reading the `.schema` the low-level `nanoarrow.c_array_stream` does not expose, so polars/pandas/duckdb and the `gridded/ragged#RAGGED` awkward-lowered `pyarrow.Table` (the `ak.to_arrow_table` native `__arrow_c_stream__`-exporter, not the `ak.to_arrow` `pyarrow.Array` which exports only `__arrow_c_array__`) all exchange Arrow without pyarrow over the protocol every PyCapsule-exporting engine already speaks. The carrier is one frozen `Struct` carrying the capsule plus the schema repr, never a parallel per-producer wrapper.
- Entry: `ArrowCStream.of(exporter)` is the one construction entrypoint both this owner's `FrameInterop.c_stream` and the sibling `gridded/ragged#RAGGED` `RaggedArray.c_stream` compose — it accepts any `__arrow_c_stream__`-exporting object (a `narwhals`-lowered `pyarrow.Table`, an `ak.to_arrow_table` `pyarrow.Table`, an `arro3.core.Table`, a `nanoarrow.Array`), wraps it through `nanoarrow.ArrayStream`, reads the `ArrayStream.schema` repr as the carried schema evidence FIRST, then exports the stream's `__arrow_c_stream__` capsule — the order load-bearing because `__arrow_c_stream__()` moves the Arrow C stream out of the wrapper (a stream capsule is consume-once), so the schema repr must be captured before the export and the carried capsule is a single-handoff transport, never re-read. The carrier construction is one shape both folders compose, never a per-folder `ArrowCStream(capsule=, schema_repr=)` re-mint and never a second `nanoarrow.ArrayStream` wrap at the consumer seam.
- Auto: the capsule, not pyarrow's compute, crosses the seam, so the carrier stays pyarrow-free while spanning every PyCapsule-exporting backend; `nanoarrow.ArrayStream` reads the `.schema` the C-level `c_array_stream` does not, so the schema repr rides the carrier without a second introspection pass; the `arro3-core` PyCapsule bridge is the alternate zero-copy reader the carrier crosses on the consumer side, both reading the canonical Arrow C-data layout without the Arrow C++ source build.
- Packages: `nanoarrow` (`ArrayStream(obj)`/`ArrayStream.schema`/`ArrayStream.__arrow_c_stream__`/`c_array_stream` the carrier wrap), `arro3-core` (the alternate PyCapsule reader the carrier crosses), `msgspec` (`Struct` the frozen carrier owner).
- Growth: a new PyCapsule-exporting producer is admitted free by `ArrowCStream.of` over its `__arrow_c_stream__` with zero carrier change; a new carried evidence field is one column on `ArrowCStream`; zero new surface.
- Boundary: no Arrow compute, no pyarrow import, no IPC serialization (the `ragged#RAGGED` `arrow`-sink owns the IPC bytes); an inline `ArrowCStream(capsule=, schema_repr=)` re-mint at a consumer seam where `ArrowCStream.of` owns the construction, a second `nanoarrow.ArrayStream` wrap beside the carrier, and a pyarrow round-trip where the capsule crosses directly are the deleted forms.

```python signature
import nanoarrow
from msgspec import Struct


class ArrowCStream(Struct, frozen=True):
    capsule: Any
    schema_repr: str

    @classmethod
    def of(cls, exporter: Any) -> "ArrowCStream":
        # read `schema` BEFORE exporting the capsule: `__arrow_c_stream__()` moves the Arrow C
        # stream out of the wrapper (a stream capsule is consume-once), so the schema repr is
        # captured first and the carried capsule is a single-handoff transport, never re-read.
        stream = nanoarrow.ArrayStream(exporter)
        schema_repr = repr(stream.schema)
        return cls(capsule=stream.__arrow_c_stream__(), schema_repr=schema_repr)
```

## [04]-[RESEARCH]

- [NARWHALS_SURFACE]: the `narwhals` `from_native(..., eager_only=)`/`to_native`/`Implementation.{from_backend,to_native_namespace,name}`/`Schema.{names,items}`/`DataFrame.{to_polars,to_pandas,to_arrow,collect_schema,null_count,item,shape,columns,to_native}`/`LazyFrame.{collect,collect_schema,to_native}` member surface the `translate`/`lower`/`schema_of`/`namespace`/`_eager` folds transcribe is catalogue-confirmed against the folder `narwhals` `.api` (intake/export ENTRYPOINTS [01]-[02], `Implementation.from_backend`/`to_native_namespace`/`name` PUBLIC_TYPES backend-enum [02]-[05], `DataFrame.collect_schema`/`null_count`/`item`/`to_polars`/`to_pandas`/`to_arrow`/`shape`/`columns` DataFrame ENTRYPOINTS [12]/[29]/[21]/[34]/[26], `LazyFrame.collect`/`collect_schema`/`to_native` LazyFrame ENTRYPOINTS [01]/[18]/[19]). The `_shapes` null-mask read stays inside `narwhals` through `null_count().item(0, name)` — the agnostic single-cell read (DataFrame ENTRYPOINTS [21]) over the one-row null-count frame `null_count()` returns (DataFrame ENTRYPOINTS [29]) — never a `null_count().to_native()[name][0]` subscript that drops to a backend frame and lacks a uniform scalar-extraction semantics across `polars`/`pandas`/`pyarrow`. The `Backend` axis binds the real `Implementation` members `POLARS`/`PANDAS`/`PYARROW`/`MODIN`/`DUCKDB` (PUBLIC_TYPES backend-enum [01], `PYARROW.value == 'pyarrow'` and `PYARROW.name.lower() == 'pyarrow'`, never an `ARROW` member), so `Backend.PYARROW.value` and the implementation member agree and an `ARROW`-named lower path is the floor-violating form; the `eager_only=` admission flag the `_BACKEND.eager` column threads is the confirmed `from_native` intake knob (ENTRYPOINTS [01]), the `DUCKDB` lazy row admitting through the bare `from_native` and the materialized rows through `eager_only=True`. The `_eager` collect-if-lazy projection binds `LazyFrame.collect()` (LazyFrame ENTRYPOINTS [01], returning a `DataFrame`) because `null_count()`/`to_arrow()` are `DataFrame`-only members the lazy `DUCKDB` admission lacks, so the schema null-mask and the content-key IPC bytes read a materialized frame; the `isinstance(frame, nw.LazyFrame)` guard is the confirmed `DataFrame`-versus-`LazyFrame` discriminant (PUBLIC_TYPES frame-types [01]-[02]). The `_BACKEND.lower` heads — `to_polars`/`to_pandas`/`to_arrow` the eager rows, `to_native` the `MODIN`/`DUCKDB` rows whose native object is the lowered target — are the confirmed export members, the `to_native` head returning the backend-native frame/relation `narwhals` wraps. `to_polars`/`to_pandas`/`to_arrow` are `nw.DataFrame`-only (DataFrame ENTRYPOINTS [34], absent from the `LazyFrame` surface ENTRYPOINTS [01]-[19], whose only export is `to_native`), so `_lower` MUST hand an eager-target head a collected `nw.DataFrame`: `_lower(admitted, target)` reads `_eager(admitted) if _BACKEND[target].eager else admitted` off the target row's existing `eager` column, collecting a lazy `DUCKDB`-source admission before an eager `to_*`/`to_native` projector and passing the bare `admitted` only to the lazy `DUCKDB` target whose `to_native` is total over `DataFrame | LazyFrame` and preserves the deferred relation — the prior `admitted`-unconditional lowering crashing `LazyFrame.to_polars()` on a `DUCKDB`→`POLARS` translate (the live `contract#ADMISSION` `FrameAdmission.enforce` and `contract#COLLECTION` `FrameCovenant._lower` both lowering a `DUCKDB`-backed member to `Backend.POLARS`) is the deleted form. `c_stream` likewise lowers through `_eager(self._admit(frame)).to_arrow()` so the carrier receives a `narwhals`-lowered `pyarrow.Table` rather than the raw source frame whose `__arrow_c_stream__` a `pandas`/`modin` source does not reliably export. Settled fence code.
- [BACKEND_TABLE]: the `_BACKEND` `Map[Backend, _Lowering]` behavior table pairs the lowering head and the eager/lazy admission flag on one row, mirroring the sibling `egress#EGRESS` `_ROUTE` `Map[str, _Row]` data-table dispatch — the `lower` callable and the `eager` admission flag the two columns one row carries. A lazy-scan reader column is deliberately absent: the reader is a function of file FORMAT (`scan_csv`/`scan_parquet`/`scan_ipc`/`scan_ndjson`), not of backend ENGINE, so a single per-`Backend` reader name cannot exist (`Backend.POLARS` scans every format) and the `columnar#SCAN` plane keys its reader off the `DatasetKind` format axis through its own `_SCAN_READER: dict[str, str]` — a disjoint vocabulary this owner has no row in and never duplicates with an arbitrary per-engine literal. The `lower` head dispatch is the total `_BACKEND[target]` keyed read — the same `Map.__getitem__` `_admit` keys the source row through, total by construction because every `Backend` member carries a row — never an inline per-backend `match` and never a `try_find`/`assert_never(target)` fold that would mistype a populated `Backend` value as `Never`. Settled fence code.
- [INTEROP_RECEIPT]: `FrameInterop.translate` mints one `InteropReceipt` keyed by `ContentIdentity.of("interop", lowered.ipc)` over the agnostic frame's canonical Arrow IPC bytes — the same `ContentIdentity` content-keying the sibling `columnar#SCAN` `QueryReceipt.of`, `contract#QUALITY` `ContractClaim`, and `egress#EGRESS` `EgressReceipt` carry. The runtime `ContentIdentity.of(fmt, source, policy=CANONICAL_POLICY, *, view, seed) -> RuntimeRail[ContentKey]` returns a railed key over the `Buffer` source-shape (the `bytes` IPC payload the `whole` `IdentitySource` modality claims through its `Buffer()` arm, `evidence/identity#IDENTITY` PUBLIC_TYPES `Source = Buffer | Iterable[bytes] | tuple[ContentKey, ...] | Struct`), so the `boundary`-lowered `_Lowered.ipc` `bytes` feeds the whole-payload digest directly; `translate` binds the `boundary(...)` lowering rail into the `ContentIdentity.of` rail through `.bind` and `.map`s the resolved `ContentKey` into the receipt, so the lowering and the key derivation are two railed hops the consumer's `match` resolves once, never a fabricated degenerate key and never a second fault fence. `InteropReceipt` satisfies the runtime `observability/receipts#RECEIPT` `ReceiptContributor` Protocol through `contribute`, emitting an emitted-phase `Receipt.of("frame-interop", ("emitted", subject, facts))` row — the two-argument `Receipt.of(owner, evidence)` factory routing the `(phase, subject, facts)` triple the receipts owner's `of` match decomposes, never the four-positional `Receipt.of(phase, owner, subject, facts)` shape the owner does not expose — the native `rows`/`columns` counts and the `content_key.hex` riding the `dict[str, object]` facts map without a `str()` coerce (the receipts `Encoder(enc_hook=repr)` serializes scalars natively), so the interop hop's evidence rides the one receipt stream the `observability/metrics#METRIC`/`execution/lanes#LANE` fold reads. The `ContentIdentity.of` railed derivation over the `bytes` IPC source, the `.bind`/`.map` rail thread, and the `ReceiptContributor` two-argument contribution are settled against the `egress#EGRESS`/`contract#QUALITY` sibling receipts that already carry the two-argument `Receipt.of` form.
- [ARROW_C_DATA]: the `nanoarrow` `ArrayStream(obj)`/`ArrayStream.__arrow_c_stream__`/`ArrayStream.schema`/`c_array_stream` surface and the `arro3-core` PyCapsule bridge the `ArrowCStream.of` carrier binds are catalogue-confirmed against the folder `nanoarrow` `.api` (`ArrayStream` PUBLIC_TYPES [02], `array_stream`/`ArrayStream` construction ENTRYPOINTS [02]/[05], `ArrayStream.schema` metadata ENTRYPOINTS [18], the `__arrow_c_stream__` PyCapsule export IMPLEMENTATION_LAW) and the folder `arro3-core` `.api` (the `ArrowStreamExportable` `__arrow_c_stream__` protocol the carrier crosses). `ArrowCStream.of` is the one public construction classmethod — `nanoarrow.ArrayStream(exporter)` wraps any `__arrow_c_stream__`-exporting object and reads the `.schema` the C-level `c_array_stream` does not, so the carrier construction is one shape both this owner's `FrameInterop.c_stream` and the sibling `gridded/ragged#RAGGED` `RaggedArray.c_stream` compose, never a per-folder re-mint. Settled fence code.
- [INTEROP_STREAM]: the `ArrowCStream.of(exporter)` public classmethod is the one carrier-construction shape the `gridded/ragged#RAGGED` `[INTEROP_CONSTRUCTION]` ripple consumes — `ragged` composes `ArrowCStream.of(_arrow(array))` over its `ak.to_arrow_table` `pyarrow.Table` (the native `__arrow_c_stream__`-exporter) and the `columnar#SCAN` `Corpus` arm rides the same carrier off its `pa.Table`, so the construction lives once on the `ArrowCStream(Struct, frozen=True)` owner as the `of` classmethod over the one `nanoarrow.ArrayStream`/`.schema` read and is authored nowhere downstream. Settled fence code.
