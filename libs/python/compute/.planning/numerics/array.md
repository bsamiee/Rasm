# [PY_COMPUTE_ARRAY]

Backend-agnostic array admission over the Array API standard: `ArrayPayload.admit(source, axes, finite, mode, bound)` is the one entry parameterized over operand source (`ArraySource`) and output conditioning (`AdmitMode`), so a numpy floor, a JAX array, a Dask graph, or a pydata-`sparse` tensor admits through one body that never re-resolves the namespace, imports a vendor module, or grows a per-source/per-mode classmethod family. This owner is read-only admission — the mutate/copy fork belongs to transforming consumers — and it is the boundary where every downstream compute owner's backend and finiteness assumptions are established once.

`array_namespace(*arrays)` resolves the backend `xp` once at entry, stacking `array-api-compat` as the resolver tier under `array-api-extra` as the extension tier (`xp.<op>` / `xpx.<op>(..., xp=xp)`). The body is one `railed` chain inside the `boundary("array.admit", ...)` fence from `reliability/faults#FAULT`, keying the host buffer through `evidence/identity#IDENTITY` `ContentIdentity.of` so a payload from any backend keys identically to its numpy floor. The `Labelled` arm admits `xarray` carriers as branch-tier co-consumption, never a re-owned data interior. The payload graduates on the `array_layout` `HandoffAxis` case, the cross-backend bit-identity proof riding the runtime `ParityReceipt` against the corpus-admitted `array-layout` fixture.

## [01]-[INDEX]

- [01]-[PAYLOAD]: one `ArrayPayload.admit` over the `ArraySource`/`AdmitMode`/`FiniteGate` axes, railed content identity, and the `array_layout` graduation producer.

## [02]-[PAYLOAD]

- Owner: `ArrayPayload` — the input axis (`ArraySource`) and the output axis (`AdmitMode`) are orthogonal columns on one `admit`, never a combinatorial method matrix or a per-mode entrypoint family. `Array` is the one `TYPE_CHECKING` backend union the owner threads so no signature degrades to a bare `object`, and `ArrayNamespace` is the `Protocol` typing the resolved `xp` — the same `object`-to-`Protocol` collapse `numerics/interval.md#ENCLOSURE` holds.
- Cases: the lazy/eager fork (`is_lazy_array` selecting `xpx.lazy_apply` over eager `xp.any`) is established once here at admission and inherited by every downstream `jax`/`equinox`/`diffrax` consumer, never re-derived per consumer. `FiniteGate` rows name the forbidden class and fold to one masked reduction, never a three-branch ladder or a boolean knob. The `DENSE_GUARD` ceiling is the caller-threaded `DenseBound` policy value, never the library default hardcoded into the fold.
- Entry: every fault class — backend transfer, coordinate build, lazy reduction, densification bound, canonical encode — converts to `BoundaryFault` exactly once at this owner; the `boundary(...).bind(lambda rail: rail)` join and the module-level `@railed` generator are the canonical shapes the solver siblings mirror.
- Packages: `is_writeable_array`/`device`/`size` and `xpx.at`/`isclose`/`default_dtype` are reserved surface for transforming consumers — this read-only owner deliberately reads none of them; `jax` and `dask` are admitted as `array_namespace` backends, never wraps; `xarray` carriers are read structurally under `TYPE_CHECKING`, never a runtime import.
- Growth: a new operand source is one `ArraySource` case plus its `operand` arm; a new conditioning is one `AdmitMode` row plus its `condition` arm; a new finite class is one `FiniteGate` row plus its forbidden-mask arm; a new sparse format is one `SparseLayout` row; a new backend rides `array_namespace` with zero new surface.
- Boundary: no production tensor runtime; the numba LLVM JIT stays a loop-kernel accelerator on the solver owner; scipy 2-D sparse-matrix construction stays on `solvers/linear`; the mutate/copy fork (`is_writeable_array` gating `xpx.at`) belongs to transforming consumers of the same resolved `xp`.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Any, Final, Literal, Protocol, Self, assert_never

import array_api_extra as xpx
import numpy as np
import sparse
from array_api_compat import array_namespace, is_lazy_array, is_numpy_array, is_pydata_sparse_array, to_device
from expression import Error, case, tag, tagged_union
from expression.collections import Map
from msgspec import Meta, Struct

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, railed
from rasm.runtime.receipts import Receipt
from rasm.runtime.reproduction import ParityReceipt

if TYPE_CHECKING:
    # `jax`/`sparse`/`dask`/`xarray` are companion-gated and never import at runtime; every `Array` arm structurally carries the
    # `shape`/`dtype`/`device` members `array_namespace` admits, read directly off the operand rather than a second Protocol.
    import dask.array as da
    import jax
    import xarray as xr
    from numpy.typing import NDArray
    from sparse import SparseArray

    type Array = NDArray[Any] | jax.Array | da.Array | SparseArray
    type Mask = Array
    type LabelledCarrier = xr.DataArray | xr.Dataset

    class ArrayNamespace(Protocol):
        # names the exact standard members the owner reads, so every `xp.<op>` is a typed call rather than a phantom off a bare `object`.
        __name__: str
        bool: object

        def isnan(self, x: "Array", /) -> "Mask": ...
        def isinf(self, x: "Array", /) -> "Mask": ...
        def logical_or(self, x1: "Mask", x2: "Mask", /) -> "Mask": ...
        def any(self, x: "Mask", /) -> "Array": ...


# --- [TYPES] -------------------------------------------------------------------------------


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
        # a deferred operand builds mask-then-`any` as one `xpx.lazy_apply` node (scalar `xp.bool` output) materialized once here —
        # a `bool(xp.any(...))` on a graph backend forces the whole operand graph eager, the deleted form.
        if is_lazy_array(array):
            reduced = xpx.lazy_apply(lambda a: xp.any(self.forbidden(xp, a)), array, shape=(), dtype=xp.bool, xp=xp)
            return bool(np.asarray(reduced))
        return bool(xp.any(self.forbidden(xp, array)))


class AdmitMode(StrEnum):
    STRICT = "strict"  # admit the resolved operand verbatim under the finite gate
    SANITIZE = "sanitize"  # replace every non-finite cell through `xpx.nan_to_num` before the gate
    DENSE_GUARD = "dense-guard"  # route the sparse host transfer through `maybe_densify` under `DenseBound`

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
        # COO is the build floor; other rows recover through `asformat(format=)`, never a per-class constructor.
        return array if self is SparseLayout.COO else array.asformat(self.value)

    @staticmethod
    def recover(array: "SparseArray") -> "SparseLayout":
        # the concrete-class name lowercased IS the `format=` value; there is no `.format` instance attribute to read.
        return SparseLayout(type(array).__name__.lower())


# --- [CONSTANTS] -----------------------------------------------------------------------------

# the `array_layout` family's default graduation ceiling; bit-identity admits zero parity delta.
_LAYOUT_CEILING: Final[Map[str, float]] = Map.of_seq([("parity_delta", 0.0)])

# --- [MODELS] ------------------------------------------------------------------------------


class NamedAxis(Struct, frozen=True, gc=False):
    name: str
    size: int


class DenseBound(Struct, frozen=True, gc=False):
    # `Meta`-bounded: a non-positive cap or an out-of-unit-interval density is a decode-time rejection, never a silent blow-through.
    max_size: Annotated[int, Meta(gt=0)] = 1000
    min_density: Annotated[float, Meta(ge=0.0, le=1.0)] = 0.25


class SparseFacts(Struct, frozen=True, gc=False):
    # sparsity as first-class receipt evidence beside the dense facts, projected off the `sparse` properties surface.
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
        # one case for both carrier shapes, discriminated by `labelled_target` — never a per-carrier sibling case.
        return cls(labelled=(carrier, name))

    def labelled_target(self) -> "xr.DataArray":
        # a multi-variable nameless `Dataset` raises HERE inside the admission fence, never passing the carrier on to fail later
        # on a phantom `.data`; structural `data_vars` read, no runtime `xarray` import.
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
                # `sparse.asarray` carries no `fill_value`; `COO.from_numpy(dense, fill_value=)` is the
                # densify-to-sparse path that sets the implicit dense value, then `reformat` to layout.
                return layout.reformat(sparse.COO.from_numpy(dense, fill_value=fill_value))
            case ArraySource(tag="sparse_from", sparse_from=(coords, data, shape, layout, fill_value)):
                return layout.reformat(sparse.COO(coords, data, shape=shape, fill_value=fill_value))
            case ArraySource(tag="labelled"):
                return self.labelled_target().data
            case _ as unreachable:
                assert_never(unreachable)

    def axes_of(self) -> tuple[NamedAxis, ...]:
        # dims/sizes off the resolved DataArray become the `NamedAxis` rows the admission gate checks; other sources yield ().
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
    sparse_facts: SparseFacts | None
    content_key: ContentKey

    # --- [OPERATIONS] ----------------------------------------------------------------------

    @classmethod
    def admit(
        cls,
        source: ArraySource,
        axes: tuple[NamedAxis, ...],
        finite: FiniteGate,
        mode: AdmitMode = AdmitMode.STRICT,
        bound: DenseBound = DenseBound(),
    ) -> "RuntimeRail[ArrayPayload]":
        # a labelled source with no caller axes derives them from its own coords through `axes_of`.
        return boundary("array.admit", lambda: _admit(source.operand(), axes or source.axes_of(), finite, mode, bound)).bind(lambda rail: rail)

    def graduates(self, parity: ParityReceipt) -> "RuntimeRail[GraduationReceipt]":
        # verification folds to the `parity_delta` residual the hub clears against the family ceiling; a caller's tighter row overrides.
        ledger = {"parity_delta": 0.0 if parity.verified else 1.0}
        return GraduationReceipt.graduates("compute.array", HandoffAxis(array_layout=self.backend), self.content_key, ledger, dict(_LAYOUT_CEILING.items()))

    def facts(self) -> dict[str, object]:
        base: dict[str, object] = {
            "backend": self.backend,
            "dtype": self.dtype,
            "shape": self.shape,
            "count": self.count,
            "finite": self.finite.value,
            "mode": self.mode.value,
        }
        return base | (self.sparse_facts.as_map() if self.sparse_facts is not None else {})

    def contribute(self) -> Iterable[Receipt]:
        return (Receipt.of("compute.array", ("emitted", self.backend, self.facts())),)


# --- [OPERATIONS] --------------------------------------------------------------------------


# `@railed` decorates a module-level generator — the builder cannot bind a `@classmethod`-wrapped one off `cls`.
@railed
def _admit(array: "Array", axes: tuple[NamedAxis, ...], finite: FiniteGate, mode: AdmitMode, bound: DenseBound) -> ArrayPayload:
    xp = array_namespace(array)
    conditioned = mode.condition(xp, array)
    if finite.violated(xp, conditioned):
        # `yield from Error(...)` raises `EffectError` so the builder short-circuits; a bare `yield Error(...)` binds the fault as `Ok`.
        yield from Error(BoundaryFault(boundary=(f"non-finite:{finite.value}", str(conditioned.dtype))))
    sparse_in = is_pydata_sparse_array(conditioned)
    # `shape`/`count` read off the always-concrete host buffer — a lazy operand's `size` is `None`, so `size(conditioned) or 0` records a `0` lie.
    buffer = _host_buffer(conditioned, sparse_in, mode, bound)
    # a non-empty `axes` whose sizes disagree with the concrete buffer is a lie the data-branch `Dataset` would inherit; short-circuit it.
    if axes and tuple(axis.size for axis in axes) != buffer.shape:
        yield from Error(BoundaryFault(boundary=(f"axes-shape:{len(axes)}", str(buffer.shape))))
    key: ContentKey = yield from ContentIdentity.of("array", buffer)
    return ArrayPayload(
        backend=xp.__name__,
        dtype=str(conditioned.dtype),
        shape=buffer.shape,
        count=buffer.size,
        axes=axes,
        finite=finite,
        mode=mode,
        sparse_facts=SparseFacts.of(conditioned) if sparse_in else None,
        content_key=key,
    )


# one backend-residence fold to the canonical C-contiguous host buffer, passed to `ContentIdentity.of` as the PEP 688 `Buffer`
# (no redundant `.tobytes()`). Residence reads the `is_numpy_array` `TypeIs` — a `Device != "cpu"` compare mis-evaluates the opaque
# Array-API `Device` object, which is not a string.
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
