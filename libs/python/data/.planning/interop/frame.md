# [PY_DATA_FRAME]

The backend-agnostic frame-translation owner: one `FrameInterop` owner over `narwhals` keyed by a single `narwhals.Implementation` backend axis, plus the pyarrow-free Arrow C Data Interface zero-copy carrier over the Arrow PyCapsule protocol. `FrameInterop.translate` lifts any admitted native frame into the agnostic `nw.DataFrame` and lowers it to the requested backend through one `_LOWER` dispatch row; `FrameInterop.schema_of` reads the backend-agnostic schema and the live per-column null-mask and folds both into `contracts` `FieldShape`s; `ArrowCStream` carries the Arrow C Data Interface stream capsule over `arro3-core`/`nanoarrow`, so polars/pandas/duckdb exchange Arrow without pyarrow over the protocol every PyCapsule-exporting engine already speaks. The backend axis lives on one interop owner; no parallel polars/pandas/pyarrow adapter types.

## [1]-[INDEX]

- `[2]-[INTEROP]`: dataframe-agnostic backend translation and the pyarrow-free Arrow C-data carrier.

## [2]-[INTEROP]

- Owner: `FrameInterop` — one backend-agnostic translation owner over `narwhals`, the backend discriminated by a single `narwhals.Implementation` axis (POLARS/PANDAS/PYARROW the admitted rows). `narwhals.from_native` lifts any admitted native frame into the agnostic `nw.DataFrame`; `narwhals.to_native` lowers it back to the requested backend; the backend is never branched into parallel adapter classes. `ArrowCStream` is the Arrow C Data Interface zero-copy carrier over the Arrow PyCapsule stream protocol, one row on the same interop owner.
- Entry: `FrameInterop.translate` lifts a native frame through `narwhals.from_native(..., eager_only=True)`, then lowers to the requested `Backend` row via the `_LOWER` dispatch table (`to_polars`/`to_pandas`/`to_arrow`), returning a `RuntimeRail` of the native frame in the target backend; `FrameInterop.schema_of` reads the backend-agnostic `nw.Schema` through `collect_schema()`, reads the per-column null-mask off the live frame, and folds both into a tuple of `FieldShape`s without touching any backend API directly; `FrameInterop.c_stream` exports the zero-copy Arrow C stream capsule through the PyCapsule protocol over `arro3-core`/`nanoarrow`.
- Auto: nullability is the observed null-mask read by `null_count()`, never inferred from dtype kind; the Arrow C-data carrier consumes any `__arrow_c_stream__`-exporting object, so the pyarrow-free Arrow interchange spans every PyCapsule-exporting backend without the Arrow C++ source build.
- Packages: `narwhals` (`from_native`/`to_native`/`from_arrow`/`Implementation`/`Schema`/`DataFrame.{to_polars,to_pandas,to_arrow,collect_schema,implementation,to_native,null_count}`), `arro3-core` (the pyarrow-free Arrow C-data bridge), `nanoarrow` (`c_array_stream`/`ArrayStream`), runtime (`RuntimeRail`/`boundary`/`BoundaryFault`).
- Growth: a new backend is one `Backend` enum row plus one `_LOWER` dispatch row; a new interchange protocol is one method on the same owner; zero new surface. The per-backend adapter family (`PolarsAdapter`/`PandasAdapter`/`ArrowAdapter`) collapses into one owner with a backend axis.
- Boundary: no compute (the numeric trio and labelled-array ownership stays in `compute`), no durable store, no engine-specific query rail; `narwhals` owns only the agnostic frame-translation hop. A per-backend adapter trio, a hand-rolled `isinstance` dispatch over native frame types, and a second schema-derivation path are the deleted forms.

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from builtins import frozendict
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final

import narwhals as nw
from msgspec import Struct

from rasm.data.contracts.admission import FieldShape
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Callable


# --- [TYPES] ---------------------------------------------------------------------------
class Backend(StrEnum):
    POLARS = "polars"
    PANDAS = "pandas"
    PYARROW = "pyarrow"

    @property
    def implementation(self) -> nw.Implementation:
        return nw.Implementation.from_backend(self.value)


# --- [MODELS] --------------------------------------------------------------------------
class FrameInterop(Struct, frozen=True):
    source: Backend

    def translate(self, frame: Any, target: Backend) -> RuntimeRail[Any]:
        return boundary(f"interop.translate.{self.source}->{target}", lambda: _lower(nw.from_native(frame, eager_only=True), target))

    def schema_of(self, frame: Any) -> RuntimeRail[tuple[FieldShape, ...]]:
        return boundary(f"interop.schema.{self.source}", lambda: _shapes(nw.from_native(frame, eager_only=True)))

    def namespace(self) -> RuntimeRail[Any]:
        return boundary(f"interop.namespace.{self.source}", self.source.implementation.to_native_namespace)

    def c_stream(self, frame: Any) -> RuntimeRail[ArrowCStream]:
        return boundary(f"interop.c_stream.{self.source}", lambda: _export_c_stream(frame))


# --- [OPERATIONS] ----------------------------------------------------------------------
def _shapes(frame: nw.DataFrame[Any]) -> tuple[FieldShape, ...]:
    schema = frame.collect_schema()
    nulls = frame.null_count().to_native()
    null_by_field = {name: bool(int(nulls[name][0])) for name in schema.names()}
    return tuple(
        FieldShape(field=name, logical_type=str(dtype), nullable=null_by_field.get(name, False), source_evidence=f"narwhals:{dtype!r}")
        for name, dtype in schema.items()
    )


_LOWER: Final[frozendict[Backend, Callable[[nw.DataFrame[Any]], Any]]] = frozendict({
    Backend.POLARS: lambda f: f.to_polars(),
    Backend.PANDAS: lambda f: f.to_pandas(),
    Backend.PYARROW: lambda f: f.to_arrow(),
})


def _lower(frame: nw.DataFrame[Any], target: Backend) -> Any:
    return _LOWER[target](frame)


# --- [BOUNDARIES] ----------------------------------------------------------------------
class ArrowCStream(Struct, frozen=True):
    capsule: Any
    schema_repr: str


def _export_c_stream(frame: Any) -> ArrowCStream:
    import nanoarrow  # noqa: PLC0415

    stream = nanoarrow.c_array_stream(frame)
    return ArrowCStream(capsule=stream.__arrow_c_stream__(), schema_repr=repr(stream.schema))
```
