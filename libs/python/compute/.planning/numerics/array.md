# [PY_COMPUTE_ARRAY]

Backend-agnostic array admission over the Array API standard. `ArrayPayload` admits dtype, shape, named axes, finite policy, layout, and content identity from any Array-API-conformant array, resolving the backend namespace through `array_namespace` so a numpy floor, a JAX array, a Dask graph, or a Sparse matrix admit through one path and dispatch through the resolved `xp`. `NamedAxis` carries the labelled-coordinate cell composed from the data-branch labelled-array shapes. Admission rejects a non-finite array under a `REJECT` policy on the resilience rail, and keys identity through the one runtime `ContentIdentity` over the canonical buffer.

## [1]-[INDEX]

- [1]-[PAYLOAD]: namespace-dispatched array admission, named axes, finite policy, and content identity on one `ArrayPayload` owner.

## [2]-[PAYLOAD]

- Owner: `ArrayPayload` — the dtype/shape/named-axes/finite-policy/layout/identity admission over the Array API standard; `array_namespace(array)` resolves the backend `xp`, so the same admission path accepts a numpy array, a JAX array, a Dask array, or a Sparse array and every solver route dispatches through the resolved namespace. `NamedAxis` is the labelled-coordinate value object; the data-branch `xarray`/`dask` `Dataset`/`DataArray` shapes compose as study inputs and are never re-catalogued.
- Entry: `ArrayPayload.admit` resolves the namespace, asserts dtype and shape against the Array API inspection surface, enforces the finite policy through `xp.isfinite` reduced over the array, and returns a `RuntimeRail[ArrayPayload]`; a non-finite array under `FinitePolicy.REJECT` returns `Error(BoundaryFault(boundary=...))` carrying the offending dtype. The identity keys through `ContentIdentity.of` over the canonical contiguous buffer (`np.asarray(...).tobytes()` after a host transfer through `xp.to_device`/`np.from_dlpack`), so a payload admitted from any backend keys identically to its numpy floor.
- Packages: `array-api-compat` (`array_namespace`, `is_*_array`, `device`, `to_device`), `array-api-extra` (`at`, `atleast_nd` namespace-portable helpers), `numpy` (`asarray`, `from_dlpack`, the Array API floor), `jax` and `sparse` (admitted Array-API backends `array_namespace` resolves through their `__array_namespace__`/`device`/`to_device` hooks, never a per-backend admission body), data-branch `xarray`/`dask` labelled-array shapes, runtime (`RuntimeRail`, `BoundaryFault`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new backend is admitted by `array_namespace` with zero new surface; a new layout is one column on `ArrayPayload`; a new finite policy is one `FinitePolicy` row.
- Boundary: no production tensor runtime and no per-backend admission family — `array_namespace` collapses the backend selection into one dispatch. A numpy-only admission floor with a separate per-accelerator JIT wrap, a hand-rolled finite-check loop, and a re-catalogued `xarray` surface are the deleted forms. JAX rides the namespace as a backend, never a wrap; the numba LLVM JIT stays a loop-kernel accelerator on the solver owner, distinct from the Array API admission.

```python signature
from enum import StrEnum

import numpy as np
from array_api_compat import array_namespace, device, to_device
from expression import Error, Ok
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.receipts import Receipt


class FinitePolicy(StrEnum):
    REJECT = "reject"
    ALLOW_NAN = "allow-nan"
    ALLOW_INF = "allow-inf"


class NamedAxis(Struct, frozen=True):
    name: str
    size: int


class ArrayPayload(Struct, frozen=True):
    backend: str
    dtype: str
    shape: tuple[int, ...]
    axes: tuple[NamedAxis, ...]
    finite: FinitePolicy
    content_key: ContentKey

    @classmethod
    def admit(cls, array: object, axes: tuple[NamedAxis, ...], finite: FinitePolicy) -> "RuntimeRail[ArrayPayload]":
        xp = array_namespace(array)
        if finite is FinitePolicy.REJECT and not bool(xp.all(xp.isfinite(array))):
            return Error(BoundaryFault(boundary=("non-finite", str(array.dtype))))
        host = np.asarray(to_device(array, "cpu")) if device(array) != "cpu" else np.asarray(array)
        return Ok(
            cls(
                backend=xp.__name__,
                dtype=str(array.dtype),
                shape=tuple(array.shape),
                axes=axes,
                finite=finite,
                content_key=ContentIdentity.of("array", host.tobytes(), IdentityPolicy()),
            )
        )

    def contribute(self) -> Receipt:
        facts = {"backend": self.backend, "dtype": self.dtype, "shape": repr(self.shape)}
        return Receipt.of("emitted", "compute.array", self.backend, facts)
```

## [3]-[RESEARCH]

- [ARRAY_API_NAMESPACE]: `array-api-compat` and `array-api-extra` resolve on the cp315 core (pure-Python, cp315-clean); the `array_namespace`/`device`/`to_device`/`is_*_array` and `array_api_extra.at`/`atleast_nd` spellings verify against the `.api` catalogue under a uv-sync reflection pass. The numpy floor backs the standard on cp315; the `jax`, `dask`, and `sparse` (pydata `COO`/`GCXS`/`DOK`) backends each expose the `__array_namespace__`/`device`/`to_device` Array-API hooks `compute/.api/sparse.md` catalogues, so `array_namespace` admits them through the one path at their own deploy posture rather than a per-backend admission body.
