# [PY_COMPUTE_ARRAY]

Backend-agnostic array admission over the Array API standard: `ArrayPayload.admit(source, axes, finite, mode, bound)` is the one entry parameterized over operand source (`ArraySource`) and output conditioning (`AdmitMode`), so a numpy floor, a JAX array, a Dask graph, or a pydata-`sparse` tensor admits through one body that never re-resolves the namespace, imports a vendor module, or grows a per-source/per-mode classmethod family. This owner is read-only admission — the mutate/copy fork belongs to transforming consumers — and it is the boundary where every downstream compute owner's backend and finiteness assumptions are established once.

`array_namespace(*arrays)` resolves the backend `xp` once at entry, stacking `array-api-compat` as the resolver tier under `array-api-extra` as the extension tier (`xp.<op>` / `xpx.<op>(..., xp=xp)`). Its body is one `returns_result` chain inside the rostered `boundary(ADMIT, ..., catch=...)` fence from `runtime/reliability/faults#FAULT` — the fence names the resolver, carrier, and densify-bound raise surface it reaches and nothing wider — keying the host buffer through `runtime/evidence/identity#IDENTITY` `ContentIdentity.of` so a payload from any backend keys identically to its numpy floor. Its `Labelled` arm admits `xarray` carriers as branch-tier co-consumption, never a re-owned data interior. Payloads graduate on the `array_layout` `HandoffAxis` case, the cross-backend bit-identity proof riding the runtime `Parity` against the corpus-admitted `array-layout` fixture.

## [01]-[INDEX]

- [02]-[PAYLOAD]: one `ArrayPayload.admit` over the `ArraySource`/`AdmitMode`/`FiniteGate` axes, result-typed content identity, and the `array_layout` graduation producer.

## [02]-[PAYLOAD]

- Owner: `ArrayPayload` — the input axis (`ArraySource`) and the output axis (`AdmitMode`) are orthogonal columns on one `admit`, never a combinatorial method matrix or a per-mode entrypoint family. `Array` is the one `TYPE_CHECKING` backend union the owner threads so no signature degrades to a bare `object`, and `ArrayNamespace` is the `Protocol` typing the resolved `xp` — the same `object`-to-`Protocol` collapse `numerics/interval#ENCLOSURE` holds.
- Cases: the lazy/eager fork (`is_lazy_array` selecting `xpx.lazy_apply` over eager `xp.any`) is established once here at admission and inherited by every downstream `jax`/`equinox`/`diffrax` consumer, never re-derived per consumer. `FiniteGate` rows name the forbidden class and fold to one masked reduction, never a three-branch ladder or a boolean knob. `DENSE_GUARD`'s ceiling is the caller-threaded `DenseBound` policy value, never the library default hardcoded into the fold.
- Entry: every fault class — backend transfer, coordinate build, lazy reduction, densification bound, canonical encode — converts to `BoundaryFault` exactly once at this owner through the `RAISES` roster, so the two interior refusals derive their subject from the leg instead of spelling a discriminant into it and the fence catches its named provider set alone; the `boundary(...).bind(lambda held: held)` join and the module-level `@returns_result` generator are the canonical shapes the solver siblings mirror. The whole join rides the hub `evidence_run` weave under the caller's composition key, so admission — the one kernel every producer in the package crosses — reports its own lifecycle facts and resource band exactly as its siblings do rather than standing outside the branch's universal evidence floor.
- Packages: `is_writeable_array`/`device`/`size` and `xpx.at`/`isclose`/`default_dtype` are reserved surface for transforming consumers — this read-only owner deliberately reads none of them; `jax` and `dask` are admitted as `array_namespace` backends, never wraps; `xarray` carriers are read structurally under `TYPE_CHECKING`, never a runtime import.
- Growth: a new refusal is one `RAISES` row whose `slots` name its coordinates; a new operand source is one `ArraySource` case with its `operand` arm; a new conditioning is one `AdmitMode` row with its `condition` arm; a new finite class is one `FiniteGate` row with its forbidden-mask arm; a new sparse format is one `SparseLayout` row; a new backend rides `array_namespace` with zero new surface.
- Boundary: no production tensor runtime; the numba LLVM JIT stays a loop-kernel accelerator on the solver owner; scipy 2-D sparse-matrix construction stays on `solvers/linear`; the mutate/copy fork (`is_writeable_array` gating `xpx.at`) belongs to transforming consumers of the same resolved `xp`.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Any, Final, Literal, Protocol, Self, assert_never

import array_api_extra as xpx
import numpy as np
import sparse
from array_api_compat import array_namespace, is_lazy_array, is_numpy_array, is_pydata_sparse_array, to_device
from expression import Error, Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Meta, Struct

from opentelemetry import trace

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, Graduation, HandoffAxis, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeResult, boundary, returns_result, rostered
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.reproduction import Parity

if TYPE_CHECKING:
    import dask.array as da
    import jax
    import xarray as xr
    from numpy.typing import NDArray
    from sparse import SparseArray

    type Array = NDArray[Any] | jax.Array | da.Array | SparseArray
    type Mask = Array
    type LabelledCarrier = xr.DataArray | xr.Dataset

    class ArrayNamespace(Protocol):
        __name__: str
        bool: object

        def isnan(self, x: "Array", /) -> "Mask": ...
        def isinf(self, x: "Array", /) -> "Mask": ...
        def logical_or(self, x1: "Mask", x2: "Mask", /) -> "Mask": ...
        def any(self, x: "Mask", /) -> "Array": ...


# --- [TYPES] ----------------------------------------------------------------------------


class FiniteGate(StrEnum):
    REJECT = "reject"
    ALLOW_NAN = "allow-nan"
    ALLOW_INF = "allow-inf"

    def forbidden(self, xp: "ArrayNamespace", array: "Array") -> "Mask":
        match self:
            case FiniteGate.REJECT:
                return xp.logical_or(xp.isnan(array), xp.isinf(array))
            case FiniteGate.ALLOW_NAN:
                return xp.isinf(array)
            case FiniteGate.ALLOW_INF:
                return xp.isnan(array)
            case _ as unreachable:
                assert_never(unreachable)

    def violated(self, xp: "ArrayNamespace", array: "Array") -> bool:
        if is_lazy_array(array):
            reduced = xpx.lazy_apply(lambda a: xp.any(self.forbidden(xp, a)), array, shape=(), dtype=xp.bool, xp=xp)
            return bool(np.asarray(reduced))
        return bool(xp.any(self.forbidden(xp, array)))


class AdmitMode(StrEnum):
    STRICT = "strict"
    SANITIZE = "sanitize"
    DENSE_GUARD = "dense-guard"

    def condition(self, xp: "ArrayNamespace", array: "Array") -> "Array":
        match self:
            case AdmitMode.STRICT | AdmitMode.DENSE_GUARD:
                return array
            case AdmitMode.SANITIZE:
                return xpx.nan_to_num(array, xp=xp)
            case _ as unreachable:
                assert_never(unreachable)


class SparseLayout(StrEnum):
    COO = "coo"
    GCXS = "gcxs"
    DOK = "dok"

    def reformat(self, array: "SparseArray") -> "SparseArray":
        return array if self is SparseLayout.COO else array.asformat(self.value)

    @staticmethod
    def recover(array: "SparseArray") -> "SparseLayout":
        return SparseLayout(type(array).__name__.lower())


# --- [CONSTANTS] ------------------------------------------------------------------------

_LAYOUT_CEILING: Final[Map[str, float]] = Map.of_seq([("parity_delta", 0.0)])

# --- [TABLES] ---------------------------------------------------------------------------

NON_FINITE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.ARRAY, point="finite", arm="config", defect="non-finite", retriability=TERMINAL, slots=("gate", "dtype")
)
AXES_SHAPE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.ARRAY, point="axes", arm="config", defect="axes-shape", retriability=TERMINAL, slots=("declared", "buffer")
)
ADMIT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.ARRAY, point="admit", arm="config", defect="admission", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([NON_FINITE, AXES_SHAPE, ADMIT]))

_ADMIT_CATCH: Final[tuple[type[BaseException], ...]] = (KeyError, TypeError, ValueError)

# --- [MODELS] ---------------------------------------------------------------------------


class NamedAxis(Struct, frozen=True, gc=False):
    name: str
    size: int


class DenseBound(Struct, frozen=True, gc=False):
    max_size: Annotated[int, Meta(gt=0)] = 1000
    min_density: Annotated[float, Meta(ge=0.0, le=1.0)] = 0.25


class SparseFacts(Struct, frozen=True, gc=False):
    layout: SparseLayout
    fill_value: float
    nnz: int
    density: float

    @staticmethod
    def of(array: "SparseArray") -> "SparseFacts":
        return SparseFacts(layout=SparseLayout.recover(array), fill_value=float(array.fill_value), nnz=int(array.nnz), density=float(array.density))

    def as_map(self) -> dict[str, object]:
        return {"layout": self.layout.value, "fill_value": self.fill_value, "nnz": self.nnz, "density": self.density}


@tagged_union(frozen=True)
class ArraySource:
    tag: Literal["live", "sparsify", "sparse_from", "labelled"] = tag()
    live: "Array" = case()
    sparsify: tuple["Array", SparseLayout, float] = case()
    sparse_from: tuple["Array", "Array", tuple[int, ...], SparseLayout, float] = case()
    labelled: tuple["LabelledCarrier", str | None] = case()

    @classmethod
    def Live(cls, array: "Array") -> Self:
        return cls(live=array)

    @classmethod
    def Sparsify(cls, dense: "Array", layout: SparseLayout = SparseLayout.COO, fill_value: float = 0.0) -> Self:
        return cls(sparsify=(dense, layout, fill_value))

    @classmethod
    def SparseFrom(
        cls, coords: "Array", data: "Array", shape: tuple[int, ...], layout: SparseLayout = SparseLayout.COO, fill_value: float = 0.0
    ) -> Self:
        return cls(sparse_from=(coords, data, shape, layout, fill_value))

    @classmethod
    def Labelled(cls, carrier: "LabelledCarrier", name: str | None = None) -> Self:
        return cls(labelled=(carrier, name))

    def labelled_target(self) -> "xr.DataArray":
        carrier, name = self.labelled
        if not hasattr(carrier, "data_vars"):
            return carrier
        if name is not None:
            return carrier[name]
        names = tuple(carrier.data_vars)
        if len(names) == 1:
            return carrier[names[0]]
        raise ValueError(f"labelled: nameless Dataset carries {len(names)} variables; name the one to admit")

    def operand(self) -> "Array":
        match self:
            case ArraySource(tag="live", live=array):
                return array
            case ArraySource(tag="sparsify", sparsify=(dense, layout, fill_value)):
                return layout.reformat(sparse.COO.from_numpy(dense, fill_value=fill_value))
            case ArraySource(tag="sparse_from", sparse_from=(coords, data, shape, layout, fill_value)):
                return layout.reformat(sparse.COO(coords, data, shape=shape, fill_value=fill_value))
            case ArraySource(tag="labelled"):
                return self.labelled_target().data
            case _ as unreachable:
                assert_never(unreachable)

    def axes_of(self) -> tuple[NamedAxis, ...]:
        match self:
            case ArraySource(tag="labelled"):
                target = self.labelled_target()
                return tuple(NamedAxis(name=str(dim), size=int(size)) for dim, size in zip(target.dims, target.shape, strict=True))
            case _:
                return ()


class ArrayPayload(Struct, frozen=True):
    backend: str
    dtype: str
    shape: tuple[int, ...]
    count: int
    axes: tuple[NamedAxis, ...]
    finite: FiniteGate
    mode: AdmitMode
    sparse_facts: Option[SparseFacts]
    content_key: ContentKey

    # --- [OPERATIONS] -------------------------------------------------------------------

    @classmethod
    def admit(
        cls,
        source: ArraySource,
        axes: tuple[NamedAxis, ...],
        finite: FiniteGate,
        mode: AdmitMode = AdmitMode.STRICT,
        bound: DenseBound = DenseBound(),
        *,
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeResult[ArrayPayload]":
        def held() -> "RuntimeResult[ArrayPayload]":
            return boundary(
                ADMIT, lambda: _admit(source.operand(), axes or source.axes_of(), finite, mode, bound), catch=_ADMIT_CATCH
            ).bind(lambda held: held)

        facts = {"source": source.tag, "mode": mode.value, "finite": finite.value}
        return evidence_run(EvidenceScope.ARRAY, f"array.{source.tag}", held, facts=facts, composition=composition)

    def graduates(self, parity: Parity, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeResult[Graduation]":
        ledger = {"parity_delta": 0.0 if parity.verified else 1.0}
        return Graduation.graduates(
            EvidenceScope.ARRAY.value,
            HandoffAxis(array_layout=self.backend),
            self.content_key,
            ledger,
            dict(_LAYOUT_CEILING.items()),
            composition=composition,
        )

    def facts(self) -> dict[str, object]:
        base: dict[str, object] = {
            "backend": self.backend,
            "dtype": self.dtype,
            "shape": self.shape,
            "count": self.count,
            "finite": self.finite.value,
            "mode": self.mode.value,
        }
        return base | self.sparse_facts.map(SparseFacts.as_map).default_value({})

    @property
    def attributes(self) -> dict[str, str | bool | int | float]:
        scalars = {name: value for name, value in self.facts().items() if isinstance(value, str | bool | int | float)}
        return {"key": self.content_key.hex, **scalars}

    def _noted(self) -> "ArrayPayload":
        trace.get_current_span().set_attributes(self.attributes)
        return self


# --- [OPERATIONS] -----------------------------------------------------------------------


@returns_result
def _admit(array: "Array", axes: tuple[NamedAxis, ...], finite: FiniteGate, mode: AdmitMode, bound: DenseBound) -> ArrayPayload:
    xp = array_namespace(array)
    conditioned = mode.condition(xp, array)
    if finite.violated(xp, conditioned):
        yield from Error(NON_FINITE.raised(finite.value, str(conditioned.dtype)))
    sparse_in = is_pydata_sparse_array(conditioned)
    buffer = _host_buffer(conditioned, sparse_in, mode, bound)
    if axes and tuple(axis.size for axis in axes) != buffer.shape:
        yield from Error(AXES_SHAPE.raised(str(tuple(axis.size for axis in axes)), str(buffer.shape)))
    key: ContentKey = yield from ContentIdentity.of("array", buffer)
    return ArrayPayload(
        backend=xp.__name__,
        dtype=str(conditioned.dtype),
        shape=buffer.shape,
        count=buffer.size,
        axes=axes,
        finite=finite,
        mode=mode,
        sparse_facts=Some(SparseFacts.of(conditioned)) if sparse_in else Nothing,
        content_key=key,
    )._noted()


def _host_buffer(array: "Array", sparse_in: bool, mode: AdmitMode, bound: DenseBound) -> np.ndarray:
    if sparse_in:
        densified = array.maybe_densify(max_size=bound.max_size, min_density=bound.min_density) if mode is AdmitMode.DENSE_GUARD else array
        return sparse.asnumpy(densified)
    return np.ascontiguousarray(array if is_numpy_array(array) else to_device(array, "cpu"))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
