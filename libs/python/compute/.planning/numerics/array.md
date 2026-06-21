# [PY_COMPUTE_ARRAY]

Backend-agnostic array admission over the Array API standard, woven as one resolver-and-extension rail. `ArrayPayload.admit(source, axes, finite, mode, bound)` is the one entry parameterized over both the operand-source input shape (`ArraySource`: `Live`/`Sparsify`/`SparseFrom`) and the operand-conditioning output shape (`AdmitMode`: `STRICT`/`SANITIZE`/`DENSE_GUARD`), so a numpy floor, a JAX array, a Dask graph, or a pydata-`sparse` tensor admit through one body that never re-resolves the namespace, imports a vendor module, or grows a per-source/per-mode classmethod family. `array_namespace(*arrays)` resolves the backend `xp` once at operation entry; every standard op reads `xp.<op>` and every extension op reads `xpx.<op>(..., xp=xp)`, stacking `array-api-compat` as the resolver tier under `array-api-extra` as the extension tier.

The whole materialize-clean-gate-buffer-identify body is one `railed` `expression.effect.result` chain inside the runtime `boundary("array.admit", ...)` exception-to-fault fence: `FiniteGate` short-circuits a non-finite violation to `Error(BoundaryFault(boundary=...))` on the rail through one masked reduction over the `is_lazy_array` lazy/eager fork, a backend-transfer/coordinate-build/lazy-reduction fault converts to a `BoundaryFault` exactly once, and the railed `ContentIdentity.of` `ContentKey` threads through the same chain so a payload from any backend or any `ArraySource` keys identically to its numpy floor. `NamedAxis` carries the labelled-coordinate cell; the data-branch `xarray`/`dask` `Dataset`/`DataArray` shapes compose as study inputs and are never re-owned.

## [01]-[INDEX]

- [01]-[PAYLOAD]: namespace-dispatched array admission over the one `ArraySource` operand-source axis and the one `AdmitMode` output-shape axis, named axes, the `FiniteGate` masked-admission policy carrying the lazy/eager reduction fork, the `SparseLayout` pydata-sparse construction-and-evidence axis, layout, and railed content identity on one `ArrayPayload` owner, stacking `array-api-compat` resolution under `array-api-extra` extension ops as one `railed` ROP chain.

## [02]-[PAYLOAD]

- Owner: `ArrayPayload` — the dtype/shape/named-axes/finite-policy/layout/identity admission over the Array API standard, parameterized over input shape through `ArraySource` and output shape through `AdmitMode`. `array_namespace(*arrays)` resolves the backend `xp` once at operation entry, so the same admission path accepts a numpy array, a JAX array, a Dask array, or a pydata-`sparse` array and every standard op reads `xp.<op>` while every extension op reads `xpx.<op>(..., xp=xp)`, never a re-resolution inside the body and never a hardcoded vendor import. `NamedAxis` is the labelled-coordinate value object; the data-branch `xarray`/`dask` `Dataset`/`DataArray` shapes compose as study inputs and are never re-catalogued.
- Type axis: `Array` is the one structural array type the whole owner threads — the union of every `array-api-compat` backend the `TypeIs` guards narrow (`NDArray`, `jax.Array`, `da.Array`, `SparseArray`) declared once under `TYPE_CHECKING` so no signature degrades to a bare `object` and the gated `jax`/`sparse`/`dask` symbols never import at runtime. `Mask` is the boolean array a `FiniteGate` projection returns. `ArrayNamespace` is the `Protocol` that types the resolved `xp` the catalogue declares `array_namespace(*xs) -> Namespace`, naming the exact standard members the owner reads (`isnan`/`isinf`/`logical_or`/`any` and the `bool` dtype attribute) so every `xp.<op>` is a typed call rather than a phantom member off a bare `object` — the same `expr: object`-to-`Protocol` collapse `numerics/interval.md#ENCLOSURE` holds. Every `Array` arm structurally carries the `shape`/`dtype`/`device` members `array_namespace` admits across backends, so `_admit` reads `.dtype` directly off the typed operand and `_host_buffer` discriminates residence through the catalogued `is_numpy_array` `TypeIs` predicate rather than a `Device != "cpu"` compare against the opaque Array-API device object.
- Stacking rail: `array-api-compat` is the resolver tier and `array-api-extra` the extension tier directly on top — the canonical kernel does `xp = array_namespace(*arrays)` then threads that one `xp` into both the standard namespace (`xp.isnan`/`xp.isinf`/`xp.any`/`xp.bool`) and every extension call (`xpx.nan_to_num`/`xpx.lazy_apply`), so the admission body is one backend-agnostic rail across numpy/JAX/Dask/sparse with no per-backend branch. The execution-model guard the finite reduction requires rides the same resolution: `is_lazy_array(array)` (true for a JAX-traced or Dask operand) selects the deferred `xpx.lazy_apply` reduction over the eager `xp.any` reduction — the same lazy/eager fork the downstream `jax`/`equinox`/`diffrax` rails consume, established here at the admission boundary rather than re-derived per consumer. The mutate/copy fork (`is_writeable_array` gating a `copy=False` `xpx.at` write) belongs to a transforming consumer, not this read-only admission owner, which never writes the operand.
- Source axis: `ArraySource` is the ONE operand-source request — `Live(array)`, `Sparsify(dense, layout, fill_value)`, `SparseFrom(coords, data, shape, layout, fill_value)` — and `ArraySource.operand` is the one total `match` that materializes the backend `Array` each case stands for: `Live` is the operand verbatim, while both sparse arms share one `COO`-build-then-`SparseLayout.reformat` shape — `Sparsify` lowers a dense source through `sparse.COO.from_numpy(dense, fill_value=fill_value)` (`sparse.asarray` carries no `fill_value`; `from_numpy` is the densify-to-sparse path that sets the implicit dense value), `SparseFrom` builds `sparse.COO(coords, data, shape=shape, fill_value=fill_value)` from the coordinate triple, and each reshapes through `reformat` only when the layout is not COO, never a dense intermediate. `ArrayPayload.admit` consumes any `ArraySource` and never grows a per-source classmethod; a fourth operand source is one `ArraySource` case plus its `operand` arm.
- Mode axis: `AdmitMode` is the ONE bounded output-shape policy that owns how the resolved operand is conditioned before identity — `STRICT` admits the operand verbatim, `SANITIZE` replaces every non-finite cell through `xpx.nan_to_num(array, xp=xp)` (the finite-fill output a downstream solver tolerates without a masked branch), and `DENSE_GUARD` routes the sparse host transfer through `sparse.maybe_densify(max_size, min_density)` so a densification that would blow memory raises a typed boundary the rail converts rather than an OOM. The `DENSE_GUARD` ceiling is the `DenseBound` value object the caller threads on `admit` — a `Meta`-bounded `(max_size, min_density)` pair (a non-positive cap or out-of-unit-interval density is a decode-time rejection) defaulting to the documented `(1000, 0.25)` — so the guard threshold is a parameterized policy rather than the library default hardcoded into the fold. `AdmitMode.condition(xp, array)` is the one total `match` that applies the tag to the resolved operand, so a new output conditioning is one `AdmitMode` row plus its arm, never a boolean `sanitize=`/`guard=` knob and never a parallel `admit_sanitized` entrypoint. The mode and the bound thread alongside the source on one `admit` so the input axis and the output axis stay orthogonal columns on one entry rather than a combinatorial method matrix.
- Finite axis: `FiniteGate` is the ONE bounded finite-admission policy — `REJECT`, `ALLOW_NAN`, `ALLOW_INF` — and the admission body reads it through one masked reduction rather than three branches. Each row's enum value names the *forbidden* class: `REJECT` forbids any non-finite (NaN or ±inf), `ALLOW_NAN` forbids `isinf` (NaN tolerated, inf rejected), `ALLOW_INF` forbids `isnan` (inf tolerated, NaN rejected). `FiniteGate.forbidden(xp, array)` projects the row to its `xp`-namespace boolean `Mask` under a total `match` closed by `assert_never`, and `FiniteGate.violated(xp, array)` reduces that mask through the one lazy/eager fork: `is_lazy_array(array)` routes a Dask/JAX-deferred operand through `xpx.lazy_apply(lambda a: xp.any(self.forbidden(xp, a)), array, shape=(), dtype=xp.bool, xp=xp)` — the mask-then-`any` closed over the traced operand `a`, declaring the scalar `xp.bool` output (the Array API guarantees `bool` on every conformant namespace) so the reduction is one deferred node materialized once through `bool(np.asarray(...))` at this boundary, while an eager backend reduces `xp.any(mask)` and reads `bool(...)` directly. A violation short-circuits the `railed` chain to `Error(BoundaryFault(boundary=(f"non-finite:{gate.value}", str(array.dtype))))` — the `boundary` case's `(subject, detail)` two-tuple naming the offending policy in the subject and the rejecting dtype in the detail — so the class is a first-class fact on the one fault family, never a silent admission. A new finite class is one `FiniteGate` row plus its forbidden-mask arm, never a boolean knob and never a `bool(xp.any(...))` that forces a graph backend to materialize.
- Backend axis: `array_namespace` admits every Array-API backend with zero per-backend admission body; the ungated `numpy` floor and `dask` back the standard on the cp315 core, and the gated companion-band `jax` and pydata-`sparse` backends each expose the `__array_namespace__`/`device`/`to_device` hooks the same path resolves. The pydata-`sparse` backend is the one axis that also *constructs* its operand here, because a sparse tensor has no dense source array to admit — `SparseLayout` is the ONE bounded pydata-sparse policy (`COO`/`GCXS`/`DOK`) whose enum value IS the `sparse` `format=` string, owning both the `Sparsify`/`SparseFrom` construction through `reformat` and the layout recovery from a resolved operand through `SparseLayout.recover(array)` (the concrete-class name lowercased is exactly the `SparseLayout` value, the catalogued discriminant the `sparse` properties surface carries, never a phantom `.format` instance attribute). `fill_value` threads the implicit dense value (the sparse zero a NaN-fill mask treats as present) through the finite reduction so the `FiniteGate` mask reads `sparse.isnan`/`sparse.isinf` over the sparse namespace and a non-finite `fill_value` is itself rejected. The scipy 2-D `sparse` *matrix* construction for linear solves stays on `solvers/linear` (`scipy.sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack`); this owner builds the multi-dimensional pydata-`sparse` *tensor* as an Array-API backend, a distinct concern that meets the linear route only through the admitted operand.
- Entry: `ArrayPayload.admit(source, axes, finite, mode, bound)` wraps the whole materialize-clean-gate-buffer-identify body in the runtime `boundary("array.admit", ...)` exception-to-fault surface and `.bind(lambda rail: rail)`-flattens the inner rail, so a backend transfer fault (`to_device`), a coordinate-build fault (`sparse.COO`), a lazy-reduction fault (`xpx.lazy_apply`), a densification-bound fault (`sparse.maybe_densify`), or a buffer-copy fault converts to a `BoundaryFault` exactly once at this owner — the `boundary(...).bind(lambda rail: rail)` rail-join the sibling `solvers/mesh.md#EXCHANGE` `MeshExchange.run` and `solvers/field.md#FIELD` `FieldQuery.evaluate` hold against this owner as the canonical shape. `_admit` is the module-level `@railed` `effect.result` chain `admit` joins — `@railed` decorates a plain generator function, never a `@classmethod`-wrapped one the builder cannot bind off `cls`, the shape `solvers/mesh.md#EXCHANGE` `_dispatch`/`_field` establish: it resolves the namespace, conditions the operand through `mode.condition`, enforces the finite policy through `FiniteGate.forbidden`/`violated` short-circuiting `Error(BoundaryFault(...))` on a violation, binds the host buffer once through the one `_host_buffer` backend fold — reading `shape`/`count` off that always-concrete `np.ndarray` rather than the possibly-lazy operand whose `size` is `None` — gates a non-empty `axes` against that concrete `buffer.shape` so a labelled-coordinate arity/size mismatch short-circuits onto the rail rather than admitting a lie the data-branch `Dataset` would inherit, and `yield from`-binds the railed `ContentIdentity.of` `ContentKey` over the same buffer under the `CANONICAL_POLICY` default so a canonical-encode fault propagates on the one rail rather than a bare-`ContentKey` assignment or a `match`/`raise RuntimeError(fault)` re-raise. `_host_buffer` is the one backend-discriminated host-transfer fold: a pydata-`sparse` operand densifies through `sparse.asnumpy` (or `maybe_densify` under `DENSE_GUARD`), a numpy operand (the catalogued `is_numpy_array` `TypeIs`) is already the canonical host buffer and reads `np.ascontiguousarray` directly, and every other backend — a lazy JAX/Dask graph or an off-host device array — transfers through the portable `to_device(array, "cpu")` target before `np.ascontiguousarray` — so the canonical contiguous buffer is one fold over the backend's residence rather than an inline ternary, and the contiguous `np.ndarray` it returns passes to `ContentIdentity.of` directly as the PEP 688 `Buffer` the `whole` arm coerces, never a redundant `.tobytes()`.
- Receipt: `ArrayPayload.contribute` returns the one-element `Iterable[Receipt]` the runtime `ReceiptContributor` port declares (the one-element tuple-return and a `yield` both satisfying the port type the corpus carries) — `Receipt.of("compute.array", ("emitted", self.backend, self.facts()))` against the runtime two-argument `of(owner, evidence)` contract over the `(Phase, subject, facts)` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes and never a single-`Receipt` return against the `Iterable[Receipt]` port. `facts()` folds the backend, dtype, finite policy, admit mode, element count, and the `shape` tuple — the count and shape read off the always-concrete host buffer rather than a lazy operand — as native scalars the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `repr`/`str` coerce, merging the `SparseFacts` `density`/`nnz`/`fill_value`/`layout` map through the `dict` `|` union when the operand is sparse so a sparse admission graduates its sparsity evidence beside the dense facts without an imperative mutation branch, the same `facts | {...}` merge the sibling receipts use.
- Packages: `array-api-compat` (`array_namespace`, `is_pydata_sparse_array`, `is_lazy_array`, `is_numpy_array`, `to_device` — the resolver tier, the lazy/eager execution-model guard, the on-host `TypeIs[NDArray]` residence predicate, and the cpu host transfer; `is_writeable_array`/`device`/`size` are the same surface a transforming consumer reaches, `device` not read here because the on-host discriminant is `is_numpy_array` rather than a `Device`-vs-string compare, `size` reserved for a known-shape eager fact while this owner reads the count off the materialized host buffer), `array-api-extra` (`lazy_apply` threaded as `xpx.lazy_apply(fn, array, shape=(), dtype=xp.bool, xp=xp)` for the deferred finite reduction on a graph backend, `nan_to_num` the `SANITIZE` finite-fill conditioning; `default_dtype`/`isclose`/`at` are the available extension ops a transforming consumer threads on the same resolved `xp` when it allocates a precision-bearing result buffer), `numpy` (`asarray`, `ascontiguousarray` — the Array API floor and the canonical C-contiguous host buffer), `sparse` (`COO`, `COO.from_numpy`, `asformat`, `asnumpy`, `maybe_densify`, `format=`, `fill_value`, `density`, `nnz`, `isnan`/`isinf` — the pydata-sparse coordinate and densify-to-sparse construction (`COO.from_numpy` the `fill_value`-carrying dense lowering `asarray` cannot express), the guarded densification, the sparsity evidence, and the sparse-namespace finite mask; `GCXS`/`DOK` are reached through `SparseLayout.reformat`), `jax` and `dask` (admitted Array-API backends `array_namespace` resolves through their `__array_namespace__`/`device`/`to_device` hooks and `is_lazy_array` flags as deferred, never a per-backend admission body), `expression` (`tagged_union`/`tag`/`case` the `ArraySource` ADT, `Error` the in-fence rail short-circuit the `yield from` binds), data-branch `xarray`/`dask` labelled-array shapes, runtime (`RuntimeRail`/`boundary`/`BoundaryFault`/`railed` from `reliability/faults#FAULT` — `railed` the runtime-owned bound `effect.result[Any, BoundaryFault]()` builder the module-level `_admit` generator wears, never re-minted from `expression.effect` here — `ContentIdentity`/`ContentKey` from `evidence/identity#IDENTITY` keyed under the `of` `CANONICAL_POLICY` default rather than a fresh `IdentityPolicy()` allocation, `Receipt`/`ReceiptContributor` from `observability/receipts#RECEIPT`).
- Growth: a new operand source is one `ArraySource` case plus its `operand` arm; a new output conditioning is one `AdmitMode` row plus its `condition` arm; a new backend is admitted by `array_namespace` with zero new surface and rides the existing lazy/eager fork through `is_lazy_array`; a new sparse format is one `SparseLayout` row whose value is the `sparse` `format=` string; a new finite class is one `FiniteGate` row plus its forbidden-mask arm; a new densification knob is one `Meta`-bounded `DenseBound` field; a new layout column is one field on `ArrayPayload`; a new extension op is one `xpx` call threaded on the resolved `xp`, never a re-resolution.
- Boundary: no production tensor runtime, no per-backend admission family, and no per-mode entrypoint family — `array_namespace` collapses the backend selection into one dispatch, `array-api-extra` supplies every extension op on the resolved `xp`, the three finite policies fold into one masked reduction over the lazy/eager fork, the three operand sources fold into one `ArraySource` request, and the three output conditionings fold into one `AdmitMode` policy. A numpy-only admission floor with a separate per-accelerator JIT wrap, a hardcoded vendor `import` in the admission body, a re-resolution of `array_namespace` inside the reduction, a hand-rolled finite-check loop, a `bool(xp.any(...))` eager reduction that forces a JAX/Dask graph to materialize where `is_lazy_array` selects `xpx.lazy_apply`, a three-branch `if finite is REJECT / ALLOW_NAN / ALLOW_INF` ladder, a boolean `allow_nonfinite`/`sanitize` knob, an `admit`/`sparsify`/`sparse_from`/`admit_sanitized` sibling-classmethod family, a `@classmethod @railed` stacking on `_admit` where the builder consumes a plain module-level generator the `cls`-bound descriptor cannot supply (the shape `solvers/mesh.md#EXCHANGE` `_dispatch`/`_field` and `solvers/field.md#FIELD` establish against this owner), a `@staticmethod`-plus-`"ArraySource"`-forward-ref factory where the `@classmethod`-plus-`Self` form binds the subtype once (the sibling `solvers/receipt.md#RECEIPT`/`solvers/field.md#FIELD` factory law), a `match`/`raise RuntimeError(fault)` re-raise or a bare-`ContentKey` assignment off the railed `ContentIdentity.of` where the `railed` `yield from`-bind threads the canonical-encode fault, a four-positional `Receipt.of` against the two-argument `(owner, evidence)` contract, an inline host-transfer ternary where `_host_buffer` folds the backend residence, a `size(conditioned) or 0` count read off a lazy operand whose `size` is `None` where the always-concrete host buffer carries the true `shape`/`count`, a redundant `.tobytes()` on a contiguous buffer the `ContentIdentity.of` `whole` arm coerces, a sparse admission that drops the `density`/`nnz`/`fill_value` evidence, an unguarded `sparse.asnumpy` densification where `DENSE_GUARD` routes `maybe_densify`, a library-default `maybe_densify()` call hardcoding the bound where the `DenseBound` policy carries the caller's ceiling, a `fill_value=` passed to `sparse.asarray` (which has no such keyword) where `COO.from_numpy` carries the implicit dense value, an `xp: object` namespace the body reads `isnan`/`any`/`bool` off where the `ArrayNamespace` `Protocol` types the resolved namespace, a `backend.device != "cpu"` compare against the opaque Array-API `Device` object (or a `getattr(array, "device", ...)` escape) where the catalogued `is_numpy_array` `TypeIs` is the sound on-host residence discriminant, a scipy 2-D sparse-matrix builder duplicated here, a dense intermediate inside `SparseFrom`, an `axes` tuple admitted and folded into the receipt facts with no gate against the materialized `buffer.shape` where a labelled-coordinate arity/size mismatch is a silent lie, a fresh `IdentityPolicy()` allocation passed to `ContentIdentity.of` where the `of` `CANONICAL_POLICY` default already keys the canonical path, and a re-catalogued `xarray` surface are the deleted forms. JAX rides the namespace as a backend, never a wrap; the numba LLVM JIT stays a loop-kernel accelerator on the solver owner, distinct from the Array API admission; scipy 2-D sparse-matrix construction stays on `solvers/linear`.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Any, Literal, Protocol, Self, assert_never

import array_api_extra as xpx
import numpy as np
import sparse
from array_api_compat import array_namespace, is_lazy_array, is_numpy_array, is_pydata_sparse_array, to_device
from expression import Error, case, tag, tagged_union
from msgspec import Meta, Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, railed
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # the gated backend array types `array-api-compat` narrows through its `TypeIs` guards, unioned
    # into the one `Array` the owner threads so no signature degrades to a bare `object`; the
    # `jax`/`sparse`/`dask` wheels are companion-gated and never import at runtime, so the union is a
    # `TYPE_CHECKING`-only alias whose every arm structurally carries the `shape`/`dtype`/`device`
    # members `array_namespace` admits, read directly off the operand rather than a second Protocol.
    import dask.array as da
    import jax
    from numpy.typing import NDArray
    from sparse import SparseArray

    type Array = NDArray[Any] | jax.Array | da.Array | SparseArray
    type Mask = Array

    class ArrayNamespace(Protocol):
        # the resolved `xp` the catalogue declares `array_namespace(*xs) -> Namespace`; naming the
        # exact standard members the owner reads keeps every `xp.<op>` a typed call, not a phantom
        # attribute off a bare `object`, the `expr: object`-to-`Protocol` collapse the siblings hold.
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
        # lazy/eager fork: a Dask/JAX-deferred operand builds the mask-then-`any` as one deferred
        # node through `xpx.lazy_apply` declaring the scalar `xp.bool` output (the Array API
        # guarantees `bool` on every conformant namespace), materialized once here at the admission
        # boundary rather than a `bool(xp.any(...))` that forces the whole operand graph eager; an
        # eager backend reduces `xp.any` and reads `bool` directly.
        if is_lazy_array(array):
            reduced = xpx.lazy_apply(lambda a: xp.any(self.forbidden(xp, a)), array, shape=(), dtype=xp.bool, xp=xp)
            return bool(np.asarray(reduced))
        return bool(xp.any(self.forbidden(xp, array)))


class AdmitMode(StrEnum):
    STRICT = "strict"            # admit the resolved operand verbatim under the finite gate
    SANITIZE = "sanitize"        # replace every non-finite cell through `xpx.nan_to_num` before the gate
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
        # the format-discriminated reshape: COO is the build floor, the other rows recover through
        # the catalogued `asformat(format=)` reshape rather than a per-class constructor.
        return array if self is SparseLayout.COO else array.asformat(self.value)

    @staticmethod
    def recover(array: "SparseArray") -> "SparseLayout":
        # the concrete-class name lowercased IS the `format=` value the `sparse` surface carries;
        # there is no `.format` instance attribute to read.
        return SparseLayout(type(array).__name__.lower())


# --- [MODELS] ------------------------------------------------------------------------------

class NamedAxis(Struct, frozen=True, gc=False):
    name: str
    size: int


class DenseBound(Struct, frozen=True, gc=False):
    # the `DENSE_GUARD` densification ceiling threaded onto `maybe_densify(max_size, min_density)`, so
    # the host-transfer bound is a caller-parameterized policy value rather than the library default
    # hardcoded into the fold. `Meta`-bounded: a non-positive cap or an out-of-unit-interval density
    # is a decode-time rejection, never a silent blow-through.
    max_size: Annotated[int, Meta(gt=0)] = 1000
    min_density: Annotated[float, Meta(ge=0.0, le=1.0)] = 0.25


class SparseFacts(Struct, frozen=True, gc=False):
    # the sparse-array evidence the fold graduates beside the dense facts: the layout, the implicit
    # dense `fill_value`, the stored-element count, and the occupancy `density`, projected off the
    # `sparse` properties surface so a sparse admission's sparsity is first-class receipt evidence.
    layout: SparseLayout
    fill_value: float
    nnz: int
    density: float

    @staticmethod
    def of(array: "SparseArray") -> "SparseFacts":
        return SparseFacts(
            layout=SparseLayout.recover(array),
            fill_value=float(array.fill_value),
            nnz=int(array.nnz),
            density=float(array.density),
        )

    def as_map(self) -> dict[str, object]:
        return {"layout": self.layout.value, "fill_value": self.fill_value, "nnz": self.nnz, "density": self.density}


@tagged_union(frozen=True)
class ArraySource:
    tag: Literal["live", "sparsify", "sparse_from"] = tag()
    live: "Array" = case()
    sparsify: tuple["Array", SparseLayout, float] = case()
    sparse_from: tuple["Array", "Array", tuple[int, ...], SparseLayout, float] = case()

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
            case _ as unreachable:
                assert_never(unreachable)


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
        # `_admit` is the module-level `@railed` chain; `admit` enters one `array.admit` fence and
        # `.bind`-flattens the inner rail so a backend-transfer/coordinate-build/lazy-reduction/
        # densification-bound/canonical-encode fault converts to a `BoundaryFault` exactly once — the
        # `boundary(...).bind(lambda rail: rail)` join `solvers/mesh.md#EXCHANGE`/`solvers/field.md#FIELD`
        # mirror against this owner, never a `@classmethod @railed` stacking the builder cannot bind.
        return boundary("array.admit", lambda: _admit(source.operand(), axes, finite, mode, bound)).bind(lambda rail: rail)

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

# the one `railed` `effect.result` chain `admit` joins through `.bind` inside the `array.admit` fence:
# resolve the namespace, condition the operand by the output mode, short-circuit a finite violation
# onto the rail, fold the host buffer, and `yield from`-bind the railed `ContentIdentity.of` key so a
# canonical-encode `Error` propagates on the one rail. `@railed` decorates a module-level function, the
# canonical shape `solvers/mesh.md#EXCHANGE` `_dispatch`/`_field` and `solvers/field.md#FIELD` hold —
# the builder consumes a plain generator, never a `@classmethod`-wrapped one it cannot bind off `cls`.
@railed
def _admit(array: "Array", axes: tuple[NamedAxis, ...], finite: FiniteGate, mode: AdmitMode, bound: DenseBound) -> ArrayPayload:
    xp = array_namespace(array)
    conditioned = mode.condition(xp, array)
    if finite.violated(xp, conditioned):
        # `yield from Error(...)` invokes `Result.__iter__`, which raises `EffectError` on the `Error`
        # case so the builder short-circuits the chain — a bare `yield Error(...)` binds the fault as
        # an `Ok` value, the deleted form.
        yield from Error(BoundaryFault(boundary=(f"non-finite:{finite.value}", str(conditioned.dtype))))
    sparse_in = is_pydata_sparse_array(conditioned)
    # the host buffer is the one always-concrete materialization: `shape`/`count` read off it rather
    # than off a lazy operand whose `array_api_compat.size` is `None` and whose `.shape` carries unknown
    # entries — `size(conditioned) or 0` would record a `0` count, the lie.
    buffer = _host_buffer(conditioned, sparse_in, mode, bound)
    # labelled axes are gated against the materialized shape: a non-empty `axes` whose per-axis sizes
    # disagree with the concrete buffer is a lie that would mis-shape the data-branch `Dataset` it feeds,
    # short-circuited onto the rail; an empty `axes` is the unlabelled path and clears.
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


# one backend-residence fold returning the canonical C-contiguous host buffer: a sparse tensor densifies
# (guarded through `maybe_densify(max_size, min_density)` under `DENSE_GUARD` so a memory blow is a typed
# boundary, not an OOM, the ceiling carried by the caller's `DenseBound`), an off-host lazy/device array
# transfers to cpu first, an on-host eager array reads its contiguous buffer directly. The returned
# `ndarray` is the PEP 688 `Buffer` `ContentIdentity.of` coerces — no redundant `.tobytes()`. Residence
# reads the catalogued `is_numpy_array` `TypeIs[NDArray]` predicate, not a `Device != "cpu"` compare an
# opaque Array-API `Device` object (the device is not a string) would mis-evaluate.
def _host_buffer(array: "Array", sparse_in: bool, mode: AdmitMode, bound: DenseBound) -> np.ndarray:
    if sparse_in:
        densified = array.maybe_densify(max_size=bound.max_size, min_density=bound.min_density) if mode is AdmitMode.DENSE_GUARD else array
        return sparse.asnumpy(densified)
    return np.ascontiguousarray(array if is_numpy_array(array) else to_device(array, "cpu"))
```

## [03]-[RESEARCH]

- [ARRAY_API_STACK]: `array-api-compat` is the resolver tier and `array-api-extra` the extension tier directly on top, both ungated pure-Python on the cp315 core; the canonical rail `xp = array_namespace(*arrays)` then `xpx.<op>(..., xp=xp)` is the documented stack in `compute/.api/array-api-compat.md` `[04]-[STACKING]` and `compute/.api/array-api-extra.md` `[04]-[STACKING]`, so this owner resolves the namespace once and threads the one `xp` into both the standard and extension calls rather than re-resolving or importing a vendor namespace. `array_namespace`/`to_device`/`is_pydata_sparse_array`/`is_lazy_array`/`size` verify against `compute/.api/array-api-compat.md` ENTRYPOINTS [01]/[04]/[05] and backend-predicate [08]/[09]; the wired `xpx.lazy_apply(func, *args, shape=, dtype=, xp=)` and `xpx.nan_to_num(x, *, fill_value=0.0, xp=)` verify against `compute/.api/array-api-extra.md` ENTRYPOINTS [13]/[14], and the lazy-reduction output dtype is the standard `xp.bool` the Array API guarantees on every conformant namespace (`xpx.default_dtype(xp, kind=...)` ENTRYPOINTS [08] is the catalogued backend-canonical query for a *precision-bearing* `'real floating'`/`'integral'` result buffer, not the boolean mask, so it is reserved for a transforming consumer; `isclose`/`at` [10]/[01] are the same consumer-threaded extension surface on the resolved `xp`). The `numpy>=2.0` floor and `dask` are ungated cp315 core (numpy 2.x ships cp315 wheels, dask is pure Python), while `jax` (`python_version<'3.15'`, gated by the jaxlib cp315 floor) and `sparse` (`python_version<'3.15'`, gated by the numba/llvmlite cp315 floor) are the structurally gated companion band, each exposing the same `__array_namespace__`/`device`/`to_device` Array-API hooks `array_namespace` admits through the one path at its own manifest gate.
- [INPUT_OUTPUT_AXES]: the owner is parameterized over both the operand-source input shape (`ArraySource`) and the output-conditioning shape (`AdmitMode`), the two orthogonal columns on one `admit` rather than a combinatorial method matrix. `AdmitMode.SANITIZE` realizes the `xpx.nan_to_num` cleaning op the catalogue documents at ENTRYPOINTS [14] as the finite-fill conditioning a downstream solver tolerates, and `AdmitMode.DENSE_GUARD` realizes the `sparse.maybe_densify(max_size, min_density)` guarded densification the `compute/.api/sparse.md` `[04]-[LOCAL_ADMISSION]` names as "the correct boundary for handing a result to a dense consumer" so a densification that would blow memory raises a typed boundary the rail converts rather than an OOM. `condition` is one total `match` closed by `assert_never`, so a fourth output shape is one row plus one arm, never a `sanitize=`/`guard=` boolean knob nor a parallel `admit_sanitized` entrypoint — the same input-and-output parameterization the runtime `evidence/identity#IDENTITY` `ContentIdentity.of` holds over its `view` projection.
- [LAZY_EAGER_FORK]: `is_lazy_array(array)` is true for a Dask and a JAX-deferred operand per `compute/.api/array-api-compat.md` ENTRYPOINTS [09], and the `[04]-[STACKING]` row binds it to `xpx.lazy_apply` for the deferred path against the direct eager `xp` op. `FiniteGate.violated` reads that guard: a lazy operand builds the `xp.any(forbidden(...))` reduction as one deferred node through `xpx.lazy_apply(fn, array, shape=(), dtype=xp.bool, xp=xp)` declaring the scalar `xp.bool` output per `compute/.api/array-api-extra.md` `[04]-[ARRAY_API_EXTRA_TOPOLOGY]`/`[LOCAL_ADMISSION]` ("`lazy_apply` is required when `func` uses control flow incompatible with graph tracing"), materialized once at this admission boundary, while an eager backend reduces `xp.any` and reads `bool(...)` directly — a `bool(xp.any(...))` over a Dask or JAX-traced array forces the whole operand graph eager or raises a `TracerBoolConversionError`, the deleted form. `is_writeable_array` gates the `copy=False` `xpx.at` write per `compute/.api/array-api-compat.md` `[04]-[STACKING]`, established here at the admission boundary as the same lazy/eager and mutate/copy forks the downstream `jax`/`diffrax`/`equinox` rails consume.
- [FINITE_GATE]: the three `FiniteGate` rows fold into one masked reduction because the Array API guarantees `isnan`/`isinf` on every conformant namespace; the forbidden-mask projection reads the resolved `xp`'s member under a total `match` closed by `assert_never`, so `REJECT` rejects any non-finite (`isnan | isinf`), `ALLOW_NAN` rejects only inf, and `ALLOW_INF` rejects only NaN, with no per-policy branch in the entry body and no boolean knob. The mask reduces through the lazy/eager fork once and the violating policy/dtype land on the `boundary` case's `(subject, detail)` two-tuple — `subject=f"non-finite:{policy}"`, `detail=dtype` — matching the `reliability/faults#FAULT` `BoundaryFault.boundary: tuple[str, str]` shape exactly rather than a three-tuple the fault family does not admit. This is a domain rejection minted directly on the fault family, yielded onto the `railed` chain as `Error(BoundaryFault(...))` (the same in-fence `Error` form the `quantity.md` `convert` pre-check documents), distinct from the exception-to-fault conversion `boundary` owns for a raising transfer.
- [RAIL_BOUNDARY]: the materialize-clean-gate-buffer-identify body rides `boundary("array.admit", ...)` from `reliability/faults` so a `to_device` transfer, a `sparse.COO` build, an `xpx.lazy_apply` lazy reduction, a `sparse.maybe_densify` densification bound, or a buffer copy that raises converts to a `BoundaryFault` exactly once at this owner; `ArrayPayload.admit` joins the inner `RuntimeRail` the thunk returns through `bind`, keeping the entry a single rail. `_admit` is the `railed` `effect.result[Any, BoundaryFault]()` builder the faults owner exposes for the free-form interleaved-bind past the three-level threshold: a finite violation `yield from Error(...)` invokes `Result.__iter__` raising `EffectError` so the chain short-circuits to the fault (a bare `yield Error(...)` would bind it as an `Ok` value, the deleted form), and the railed `ContentIdentity.of` `ContentKey` threads through `key = yield from ContentIdentity.of(...)` per the `evidence/identity#IDENTITY` `ContentIdentity.of(fmt, source, policy, *, view="value") -> RuntimeRail[ContentKey]` contract, so the canonical-encode fault rides the one rail rather than the prior `match ContentIdentity.of(...)`/`raise RuntimeError(fault)` re-raise — the `railed`-over-`match` collapse the runtime `reliability/faults#FAULT` owner names as the capability `traversed` cannot express. The `ContentIdentity.of` `source` argument is the contiguous `np.ndarray` `_host_buffer` returns, bound once and admitted directly as the PEP 688 `Buffer` the `IdentitySource.lift` `whole` arm coerces through `bytes(...)`, so the redundant `.tobytes()` is dropped and `shape`/`count` read off that always-concrete buffer rather than a lazy operand whose `array_api_compat.size` is `None` (the `size(conditioned) or 0` count lie the deleted form). `_host_buffer` folds the backend residence — `sparse.asnumpy`/`maybe_densify` for a sparse tensor, `np.ascontiguousarray` direct for a numpy operand (the catalogued `is_numpy_array` `TypeIs`), `to_device(array, "cpu")` for every other backend (a lazy JAX/Dask graph or an off-host device array) — discriminating on the typed `is_numpy_array` predicate rather than a `backend.device != "cpu"` compare against the opaque Array-API `Device` object (the device is not a string, so the compare mis-evaluates a non-numpy backend), so the canonical contiguous identity buffer is one typed fold rather than an inline ternary.
- [SPARSE_CONSTRUCTION_AND_EVIDENCE]: pydata-`sparse` is the gated companion band (`python_version<'3.15'`, gated by the numba/llvmlite cp315 floor, not numpy which ships cp315 wheels), so the `ArraySource.Sparsify`/`ArraySource.SparseFrom` operand bodies are authored against the documented `COO`/`COO.from_numpy`/`asformat`/`asnumpy`/`maybe_densify`/`fill_value`/`density`/`nnz`/`isnan`/`isinf` surface in `compute/.api/sparse.md` PUBLIC_TYPES/ENTRYPOINTS. Both sparse arms build a `COO` carrying the implicit-dense `fill_value` then `reformat` — `Sparsify` lowers a dense source through `COO.from_numpy(dense, fill_value=)` and `SparseFrom` through `COO(coords, data, shape=, fill_value=)`; `sparse.asarray(obj, *, dtype, format, copy)` (ENTRYPOINTS [05]) carries no `fill_value` keyword, so the `fill_value`-bearing densify-to-sparse path is `COO.from_numpy`, not `asarray`. `SparseLayout` enum values are the verified `format=`/`asformat` construction strings (`'coo'`/`'gcxs'`/`'dok'`) the creation/`asformat` path consumes, `SparseLayout.reformat` is the `asformat`-discriminated reshape, and `SparseLayout.recover` reads the `COO`/`GCXS`/`DOK` concrete-class name lowercased since the catalogued properties surface carries no `.format` instance attribute. `SparseFacts.of` projects the `density`/`nnz`/`fill_value` the `[04]-[LOCAL_ADMISSION]` "a sparse-array fold captures the format, `nnz`/`density`, and `fill_value` as the array claim" row mandates, graduated through the `facts()` `dict` `|` union so the sparsity evidence rides the receipt beside the dense facts. Because `sparse` satisfies `array-api-compat` per `compute/.api/sparse.md` `[04]-[INTEGRATION_STACK]`, the same `FiniteGate.forbidden` mask and the resolved-`xp` reduction fold a sparse array beside numpy/jax/dask with no format-specific branch. This is the multi-dimensional pydata-sparse *tensor* backend axis — distinct from the scipy 2-D sparse *matrix* construction (`scipy.sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack`) that `solvers/linear` owns for the linear-solve operand; there is no separate `numerics/sparse` page, and the two meet only through the admitted operand the linear route reads.
