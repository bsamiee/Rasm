# [PY_COMPUTE_ARRAY]

Backend-agnostic array admission over the Array API standard. `ArrayPayload` admits dtype, shape, named axes, finite policy, layout, and content identity from any Array-API-conformant array, resolving the backend namespace through `array_namespace` so a numpy floor, a JAX array, a Dask graph, or a pydata-`sparse` tensor admit through one path and dispatch through the resolved `xp`. `ArrayOp` is the ONE admission request discriminating where the operand comes from — `Live` admits an already-materialized backend array, `Sparsify` lowers a dense source to a pydata-`sparse` tensor, `SparseFrom` builds the format-discriminated container from a coordinate triple without a dense intermediate — so a single `ArrayPayload.admit` body materializes the operand and admits it, never a `admit`/`sparsify`/`sparse_from` sibling family fanning the one concern across three classmethods. `FinitePolicy` is the ONE bounded finite-admission vocabulary every backend reads through one masked-reduction projection — `REJECT` fails any non-finite element, `ALLOW_NAN` admits NaN but rejects ±inf, `ALLOW_INF` admits ±inf but rejects NaN — so a probabilistic study admitting masked NaN and an unbounded-objective trace admitting inf both ride the same admission body rather than a boolean `allow_nonfinite` knob. `NamedAxis` carries the labelled-coordinate cell composed from the data-branch labelled-array shapes. The construction keys identity through the one runtime `ContentIdentity` over the canonical buffer, and every materialization that touches a backend transfer or buffer copy rides the runtime `boundary` exception-to-fault surface so no array-library exception escapes domain flow.

## [01]-[INDEX]

- [01]-[PAYLOAD]: namespace-dispatched array admission over the one `ArrayOp` operand-source request, named axes, the `FinitePolicy` masked-admission axis, the `SparseLayout` pydata-sparse construction axis, layout, and content identity on one `ArrayPayload` owner.

## [02]-[PAYLOAD]

- Owner: `ArrayPayload` — the dtype/shape/named-axes/finite-policy/layout/identity admission over the Array API standard; `array_namespace(array)` resolves the backend `xp`, so the same admission path accepts a numpy array, a JAX array, a Dask array, or a pydata-`sparse` array and every solver route dispatches through the resolved namespace. `NamedAxis` is the labelled-coordinate value object; the data-branch `xarray`/`dask` `Dataset`/`DataArray` shapes compose as study inputs and are never re-catalogued.
- Request axis: `ArrayOp` is the ONE admission request — `Live(array)`, `Sparsify(dense, layout, fill_value)`, `SparseFrom(coords, data, shape, layout, fill_value)` — and `ArrayOp.operand` is the one total `match` that materializes the backend array each case stands for: `Live` is the operand verbatim, `Sparsify` lowers through `sparse.asarray(dense, format=layout.value, fill_value=fill_value)`, `SparseFrom` builds `sparse.COO(coords, data, shape=shape, fill_value=fill_value)` and reshapes through `asformat(layout.value)` only when the layout is not COO, never a dense intermediate. `ArrayPayload.admit` consumes any `ArrayOp` and never grows a per-source classmethod; a fourth operand source is one `ArrayOp` case plus its `operand` arm.
- Finite axis: `FinitePolicy` is the ONE bounded finite-admission policy — `REJECT`, `ALLOW_NAN`, `ALLOW_INF` — and the admission body reads it through one masked reduction rather than three branches. Each row carries the predicate that defines the *forbidden* class as the enum value: `REJECT` forbids `~isfinite` (any NaN or ±inf), `ALLOW_NAN` forbids `isinf` (NaN tolerated, inf rejected), `ALLOW_INF` forbids `isnan` (inf tolerated, NaN rejected). `FinitePolicy.forbidden(xp, array)` projects the row to its `xp`-namespace boolean mask under a total `match` closed by `assert_never`, the entry reduces `xp.any(mask)` once, and a violation returns `Error(BoundaryFault(boundary=(f"non-finite:{finite.value}", str(array.dtype))))` — the `boundary` case's `(subject, detail)` two-tuple naming the offending policy in the subject and the rejecting dtype in the detail, so the class is a first-class fact on the one fault family, never a silent admission. A clean array under any policy admits identically; the projection runs through the resolved `xp` so a JAX or sparse array reduces through its own `isnan`/`isinf` namespace member, never a hardcoded numpy check. A new finite class is one `FinitePolicy` row plus its forbidden-mask arm, never a boolean knob.
- Backend axis: `array_namespace` admits every Array-API backend with zero per-backend admission body; the ungated `numpy` floor and `dask` back the standard on the cp315 core, and the gated companion-band `jax` and pydata-`sparse` backends each expose the `__array_namespace__`/`device`/`to_device` hooks the same path resolves. The pydata-`sparse` backend is the one axis that also *constructs* its operand here, because a sparse tensor has no dense source array to admit — `SparseLayout` is the ONE bounded pydata-sparse construction policy (`COO`/`GCXS`/`DOK`) whose enum value IS the `sparse` `format=` string, and the `Sparsify`/`SparseFrom` cases of `ArrayOp` carry that construction so the result admits through the same body the `Live` case does. `fill_value` threads the implicit dense value (the sparse zero a NaN-fill mask treats as present) through the finite reduction so the `FinitePolicy` mask reads `sparse.isnan`/`sparse.isinf` over the sparse namespace and a non-finite `fill_value` is itself rejected. The scipy 2-D `sparse` *matrix* construction for linear solves stays on `solvers/linear` (`scipy.sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack`); this owner builds the multi-dimensional pydata-`sparse` *tensor* as an Array-API backend, a distinct concern that meets the linear route only through the admitted operand.
- Entry: `ArrayPayload.admit(op, axes, finite)` wraps the whole materialize-and-admit body in the runtime `boundary("array.admit", ...)` exception-to-fault surface and joins the inner rail through `bind`, so a backend transfer fault (`to_device`), a coordinate-build fault (`sparse.COO`), or a buffer-copy fault (`tobytes`) converts to a `BoundaryFault` exactly once at this owner rather than escaping domain flow; `_admit` resolves the namespace, enforces the finite policy through the one `FinitePolicy.forbidden` masked reduction, and returns `RuntimeRail[ArrayPayload]`. The identity keys through `ContentIdentity.of("array", host.tobytes(), IdentityPolicy())` over the canonical contiguous buffer — a host transfer through `to_device(..., "cpu")` when the operand is off-host, with a pydata-`sparse` operand densified through `sparse.asnumpy` only at the identity boundary — so a payload admitted from any backend or any `ArrayOp` case keys identically to its numpy floor through the one `admit` body, never a parallel sparse admission.
- Receipt: `ArrayPayload.contribute` emits one `Receipt.of("emitted", "compute.array", backend, facts)` carrying the backend, dtype, shape, and finite policy, merging the `SparseLayout` fact through the `dict` `|` union (the same `facts | {...}` merge the sibling receipts use) when the operand is sparse, so a sparse admission graduates its format as evidence beside the dense facts without an imperative mutation branch.
- Packages: `array-api-compat` (`array_namespace`, `is_pydata_sparse_array`, `device`, `to_device`), `numpy` (`asarray`, the Array API floor), `sparse` (`asarray`, `COO`, `GCXS`, `DOK`, `asnumpy`, `format=`/`asformat`, `fill_value`, `density`, `isnan`/`isinf` — the pydata-sparse construction and backend; `GCXS`/`DOK` are reached through the `asformat(layout.value)` reshape, and an already-formatted operand recovers its layout from the concrete class through `SparseLayout(type(array).__name__.lower())` — the `COO`/`GCXS`/`DOK` class name lowercased is exactly the `SparseLayout` value, the catalogued discriminant the `sparse` properties surface carries, never a phantom `.format` instance attribute), `jax` and `dask` (admitted Array-API backends `array_namespace` resolves through their `__array_namespace__`/`device`/`to_device` hooks, never a per-backend admission body), data-branch `xarray`/`dask` labelled-array shapes, runtime (`RuntimeRail`/`boundary`/`BoundaryFault` from `runtime/faults`, `ContentIdentity`/`ContentKey`/`IdentityPolicy` from `runtime/content_identity`, `Receipt`/`ReceiptContributor` from `runtime/receipts`).
- Growth: a new operand source is one `ArrayOp` case plus its `operand` arm; a new backend is admitted by `array_namespace` with zero new surface; a new sparse format is one `SparseLayout` row whose value is the `sparse` `format=` string; a new finite class is one `FinitePolicy` row plus its forbidden-mask arm; a new layout is one column on `ArrayPayload`.
- Boundary: no production tensor runtime and no per-backend admission family — `array_namespace` collapses the backend selection into one dispatch, the three finite policies fold into one masked reduction, and the three operand sources fold into one `ArrayOp` request. A numpy-only admission floor with a separate per-accelerator JIT wrap, a hand-rolled finite-check loop, a three-branch `if finite is REJECT / ALLOW_NAN / ALLOW_INF` ladder, a boolean `allow_nonfinite` knob, an `admit`/`sparsify`/`sparse_from` sibling-classmethod family, a raw `Error(BoundaryFault(...))` body that lets a transfer exception escape the rail, a scipy 2-D sparse-matrix builder duplicated here, a dense intermediate inside `SparseFrom`, and a re-catalogued `xarray` surface are the deleted forms. JAX rides the namespace as a backend, never a wrap; the numba LLVM JIT stays a loop-kernel accelerator on the solver owner, distinct from the Array API admission; scipy 2-D sparse-matrix construction stays on `solvers/linear`.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
import sparse
from array_api_compat import array_namespace, device, is_pydata_sparse_array, to_device
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


# --- [TYPES] -------------------------------------------------------------------------------

class FinitePolicy(StrEnum):
    REJECT = "reject"
    ALLOW_NAN = "allow-nan"
    ALLOW_INF = "allow-inf"

    def forbidden(self, xp: object, array: object) -> object:
        match self:
            case FinitePolicy.REJECT:
                return xp.logical_not(xp.isfinite(array))
            case FinitePolicy.ALLOW_NAN:
                return xp.isinf(array)
            case FinitePolicy.ALLOW_INF:
                return xp.isnan(array)
            case unreachable:
                assert_never(unreachable)


class SparseLayout(StrEnum):
    COO = "coo"
    GCXS = "gcxs"
    DOK = "dok"


# --- [MODELS] ------------------------------------------------------------------------------

class NamedAxis(Struct, frozen=True):
    name: str
    size: int


@tagged_union(frozen=True)
class ArrayOp:
    tag: Literal["live", "sparsify", "sparse_from"] = tag()
    live: object = case()
    sparsify: tuple[object, SparseLayout, float] = case()
    sparse_from: tuple[object, object, tuple[int, ...], SparseLayout, float] = case()

    @staticmethod
    def Live(array: object) -> "ArrayOp":
        return ArrayOp(live=array)

    @staticmethod
    def Sparsify(dense: object, layout: SparseLayout = SparseLayout.COO, fill_value: float = 0.0) -> "ArrayOp":
        return ArrayOp(sparsify=(dense, layout, fill_value))

    @staticmethod
    def SparseFrom(
        coords: object, data: object, shape: tuple[int, ...], layout: SparseLayout = SparseLayout.COO, fill_value: float = 0.0
    ) -> "ArrayOp":
        return ArrayOp(sparse_from=(coords, data, shape, layout, fill_value))

    def operand(self) -> object:
        match self:
            case ArrayOp(tag="live", live=array):
                return array
            case ArrayOp(tag="sparsify", sparsify=(dense, layout, fill_value)):
                return sparse.asarray(dense, format=layout.value, fill_value=fill_value)
            case ArrayOp(tag="sparse_from", sparse_from=(coords, data, shape, layout, fill_value)):
                built = sparse.COO(coords, data, shape=shape, fill_value=fill_value)
                return built if layout is SparseLayout.COO else built.asformat(layout.value)
            case unreachable:
                assert_never(unreachable)


class ArrayPayload(Struct, frozen=True):
    backend: str
    dtype: str
    shape: tuple[int, ...]
    axes: tuple[NamedAxis, ...]
    finite: FinitePolicy
    layout: SparseLayout | None
    content_key: ContentKey

    # --- [OPERATIONS] ----------------------------------------------------------------------

    @classmethod
    def admit(cls, op: ArrayOp, axes: tuple[NamedAxis, ...], finite: FinitePolicy) -> "RuntimeRail[ArrayPayload]":
        return boundary("array.admit", lambda: cls._admit(op.operand(), axes, finite)).bind(lambda outcome: outcome)

    @classmethod
    def _admit(cls, array: object, axes: tuple[NamedAxis, ...], finite: FinitePolicy) -> "RuntimeRail[ArrayPayload]":
        xp = array_namespace(array)
        if bool(xp.any(finite.forbidden(xp, array))):
            return Error(BoundaryFault(boundary=(f"non-finite:{finite.value}", str(array.dtype))))
        sparse_in = is_pydata_sparse_array(array)
        host = sparse.asnumpy(array) if sparse_in else np.asarray(array if device(array) == "cpu" else to_device(array, "cpu"))
        return Ok(
            cls(
                backend=xp.__name__,
                dtype=str(array.dtype),
                shape=tuple(array.shape),
                axes=axes,
                finite=finite,
                layout=SparseLayout(type(array).__name__.lower()) if sparse_in else None,
                content_key=ContentIdentity.of("array", host.tobytes(), IdentityPolicy()),
            )
        )

    def contribute(self) -> Receipt:
        sparse_facts = {"layout": self.layout.value} if self.layout is not None else {}
        facts = {"backend": self.backend, "dtype": self.dtype, "shape": repr(self.shape), "finite": self.finite.value} | sparse_facts
        return Receipt.of("emitted", "compute.array", self.backend, facts)
```

## [03]-[RESEARCH]

- [ARRAY_API_NAMESPACE]: `array-api-compat` resolves on the cp315 core (pure-Python, cp315-clean); the `array_namespace`/`device`/`to_device`/`is_pydata_sparse_array` spellings verify against `compute/.api/array-api-compat.md` under a uv-sync reflection pass. The `numpy>=2.0` floor and `dask` are ungated cp315 core (numpy 2.x ships cp315 wheels, dask is pure Python), so they back the standard on the cp315 core; `jax` (`python_version<'3.15'`, gated by the jaxlib cp315 floor) and `sparse` (`python_version<'3.15'`, gated by the numba/llvmlite cp315 floor) are the structurally gated companion band, each exposing the same `__array_namespace__`/`device`/`to_device` Array-API hooks `compute/.api/sparse.md` catalogues, so `array_namespace` admits every backend through the one path at its own manifest gate rather than a per-backend admission body — the gated band lands when its stack ships stable cp315 wheels, never a design defect in this owner.
- [FINITE_POLICY]: the three `FinitePolicy` rows fold into one masked reduction because the Array API guarantees `isfinite`/`isnan`/`isinf` on every conformant namespace; the forbidden-mask projection reads the resolved `xp`'s member under a total `match` closed by `assert_never`, so `REJECT` rejects any non-finite, `ALLOW_NAN` rejects only inf, and `ALLOW_INF` rejects only NaN, with no per-policy branch in the entry body and no boolean knob. The mask reduces through `xp.any` once and the violating policy/dtype land on the `boundary` case's `(subject, detail)` two-tuple — `subject=f"non-finite:{policy}"`, `detail=dtype` — matching the `runtime/reliability/faults` `BoundaryFault.boundary: tuple[str, str]` shape exactly rather than a three-tuple the fault family does not admit.
- [RAIL_BOUNDARY]: the materialize-and-admit body rides `boundary("array.admit", ...)` from `runtime/faults` so a `to_device` transfer, a `sparse.COO` build, or a `tobytes` buffer copy that raises converts to a `BoundaryFault` exactly once at this owner; `ArrayPayload.admit` joins the inner `RuntimeRail` the thunk returns through `bind`, keeping the entry a single rail rather than a raw `Error(BoundaryFault(...))` that lets an array-library exception escape domain flow.
- [SPARSE_CONSTRUCTION]: pydata-`sparse` is the gated companion band (`python_version<'3.15'`, gated by the numba/llvmlite cp315 floor, not numpy which ships cp315 wheels), so the `ArrayOp.Sparsify`/`ArrayOp.SparseFrom` operand bodies are authored against the documented `sparse.asarray(format=)`/`COO`/`asformat`/`asnumpy`/`fill_value`/`density` surface in `compute/.api/sparse.md`; `SparseLayout` enum values are the verified `format=` construction strings (`'coo'`/`'gcxs'`/`'dok'`) the creation/`asformat` path consumes, and the same lowercased token is the `COO`/`GCXS`/`DOK` concrete-class name the layout-recovery reads, since the catalogued properties surface carries no `.format` instance attribute. This is the multi-dimensional pydata-sparse *tensor* backend axis — distinct from the scipy 2-D sparse *matrix* construction (`scipy.sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack`) that `solvers/linear` owns for the linear-solve operand; there is no separate `numerics/sparse` page, and the two meet only through the admitted operand the linear route reads.
