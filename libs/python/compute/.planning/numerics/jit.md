# [PY_COMPUTE_JIT]

The one polymorphic JIT owner that collapses the numba LLVM loop-kernel compiler and the jax XLA array-transform compiler into a single backend-discriminated surface. `JitBackend` is one `@tagged_union` whose `Literal` tag is the compile route — numba `njit`/`guvectorize`, numba `vectorize`, or jax `jit` — each case carrying the per-route option payload, dispatched through one table-driven `compile` entry that wraps the kernel, runs the warm trace/specialization probe, and folds the lowered-IR evidence directly off the case. There is no second receipt union: the same tag that selects the compile route folds the compile evidence, so the backend owns its own evidence rather than a parallel mode-tagged carrier mirroring it. The `none` passthrough is the unconditional cp315 floor reachable for every kernel, so a cp315 run without the gated wheels returns the host callable rather than `Error(Import)`. numba is a loop-kernel compiler and jax is an XLA array-transform compiler; both are admitted here as study-kernel accelerators on one owner, distinct from the Array-API backend dispatch in `numerics/array.md#PAYLOAD` where jax rides `array_namespace` as a backend rather than a wrap.

## [01]-[INDEX]

- [01]-[JIT]: numba `njit`/`vectorize`/`guvectorize` and jax `jit` collapsed into one backend-discriminated `JitBackend` owner that folds the lowered-IR compile evidence off the same compile tag — no parallel receipt union.

## [02]-[JIT]

- Owner: `JitBackend` — the numba-and-jax JIT cases on the one accelerator; `Njit(parallel, fastmath, cache, boundscheck)`, `Vectorize(signatures, target, layout)`, `JaxJit(static_argnums, donate_argnums)`, and the `none` passthrough each discriminate the compile route. The numba arm runs `numba.njit(cache=, parallel=, fastmath=, boundscheck=)` for the loop kernel and `numba.vectorize(signatures, target=)` (or `numba.guvectorize(signatures, layout, target=)` when the `layout` column is present) for the scalar-to-ufunc / generalized-ufunc lift; the jax arm runs `jax.jit(static_argnums=, donate_argnums=)` for the XLA array transform. The routes share one dispatch keyed on the tag inside `compile` — the decorator, its options, the warm probe, and the lowered-IR read are the row, built behind the gated import because the decorators reference the resolved `numba`/`jax` — never parallel wrap bodies, never a per-decorator method family, and never a second union restating the same tag.
- Entry: `JitBackend.compile(kernel)` enters one `boundary(f"jit.{self.tag}", ...)` and returns a `RuntimeRail[Jitted]` carrying the compiled callable beside the backend case and the folded `evidence` fact map. The numba njit row warms one specialization, then reads `CPUDispatcher.signatures` for the resolved signature, `parallel_diagnostics`/`inspect_llvm` for the parallel-fusion and lowered-IR evidence the `.api` IMPLEMENTATION_LAW names as the capture surface, and the nopython/parallel/cache verdict off the case options. The numba vectorize row records the ufunc layout signatures and target. The jax jit row traces once through `make_jaxpr` to capture the actual jaxpr text and `eval_shape` to capture the output `ShapeDtypeStruct` — the real XLA trace boundary, not a hardcoded `"traced"` literal — keyed by the `static_argnums`. The `none` passthrough returns the host kernel with empty evidence so a cp315 study composes the same `Jitted` shape whether or not the wheel resolved.
- Receipt: `Jitted` is the typed compile-evidence carrier — the compiled callable, the originating `JitBackend` case, and the lowered-IR `evidence` fact map folded off the compile tag; `Jitted.contribute` emits one `Receipt.of("emitted", "compute.jit", self.backend.tag, ...)` row carrying the route tag and its captured IR/jaxpr facts so the accelerator capture is study evidence, never a generic reported value. A graduated kernel routes its receipt through the solve/study spine, never re-deriving the compile facts downstream.
- Packages: `numba` (`njit`, `vectorize`, `guvectorize`, `CPUDispatcher.signatures`, `CPUDispatcher.parallel_diagnostics`, `CPUDispatcher.inspect_llvm`), `jax` (`jit`, `make_jaxpr`, `eval_shape`), `numpy` (the loop-kernel floor type), `expression` (`tag`, `case`, `tagged_union`), `msgspec` (`Struct`), runtime (`RuntimeRail`, `boundary`, `Receipt`, `ReceiptContributor`).
- Growth: a new loop-kernel compiler is one `JitBackend` case plus one arm in the `compile` dispatch; a new numba option is one column on the `Njit`/`Vectorize` case absorbed by the existing decorator call; a generalized ufunc is the `layout` column already on `Vectorize`; zero new surface, never a per-backend accelerator owner, never a parallel numba-wrap and jax-wrap module, never a per-mode receipt union mirroring the backend tag.
- Boundary: the `none` passthrough runs unconditionally on cp315; `numba`/`llvmlite` and `jax`/`jaxlib` carry no cp315 wheel, so the numba and jax arms are authored against the documented API behind one gated import each with the passthrough as the reachable floor for every kernel. A separate numba accelerator owner beside a jax JIT owner, a per-decorator method family, parallel wrap bodies, a second `JitReceipt` union restating the compile tag, a generic `IReceipt` over the compile evidence, and a jax-as-array-backend arm here (that rides `array_namespace` at array admission, not a kernel wrap) are the deleted forms.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


@tagged_union(frozen=True)
class JitBackend:
    tag: Literal["njit", "vectorize", "jax_jit", "none"] = tag()
    njit: tuple[bool, bool, bool, bool] = case()
    vectorize: tuple[tuple[str, ...], Literal["cpu", "parallel"], str] = case()
    jax_jit: tuple[tuple[int, ...], tuple[int, ...]] = case()
    none: tuple[()] = case()

    @staticmethod
    def Njit(*, parallel: bool = False, fastmath: bool = False, cache: bool = True, boundscheck: bool = False) -> "JitBackend":
        return JitBackend(njit=(parallel, fastmath, cache, boundscheck))

    @staticmethod
    def Vectorize(signatures: tuple[str, ...], *, target: Literal["cpu", "parallel"] = "cpu", layout: str = "") -> "JitBackend":
        return JitBackend(vectorize=(signatures, target, layout))

    @staticmethod
    def JaxJit(*, static_argnums: tuple[int, ...] = (), donate_argnums: tuple[int, ...] = ()) -> "JitBackend":
        return JitBackend(jax_jit=(static_argnums, donate_argnums))

    @staticmethod
    def Passthrough() -> "JitBackend":
        return JitBackend(none=())

    def compile(self, kernel: Callable[..., np.ndarray], *probe: object) -> "RuntimeRail[Jitted]":
        return boundary(f"jit.{self.tag}", lambda: self._compiled(kernel, probe))

    def _compiled(self, kernel: Callable[..., np.ndarray], probe: tuple[object, ...]) -> "Jitted":
        match self:
            case JitBackend(tag="njit", njit=(parallel, fastmath, cache, boundscheck)):
                import numba

                fn = numba.njit(cache=cache, parallel=parallel, fastmath=fastmath, boundscheck=boundscheck)(kernel)
                if probe:
                    fn(*probe)
                signature = ", ".join(str(s) for s in fn.signatures) or "<unspecialized>"
                llvm = next(iter(fn.inspect_llvm().values()), "") if fn.signatures else ""
                evidence = {
                    "mode": "llvm",
                    "signature": signature,
                    "parallel": str(parallel),
                    "fastmath": str(fastmath),
                    "cached": str(cache),
                    "ir_lines": str(llvm.count("\n")),
                }
                return Jitted(fn, self, evidence)
            case JitBackend(tag="vectorize", vectorize=(signatures, target, layout)):
                import numba

                fn = (
                    numba.guvectorize(list(signatures), layout, target=target)(kernel)
                    if layout
                    else numba.vectorize(list(signatures), target=target)(kernel)
                )
                evidence = {
                    "mode": "gufunc" if layout else "ufunc",
                    "signature": " | ".join(signatures),
                    "layout": layout or "<elementwise>",
                    "target": target,
                }
                return Jitted(fn, self, evidence)
            case JitBackend(tag="jax_jit", jax_jit=(static_argnums, donate_argnums)):
                import jax

                fn = jax.jit(kernel, static_argnums=static_argnums, donate_argnums=donate_argnums)
                jaxpr = repr(jax.make_jaxpr(kernel, static_argnums=static_argnums)(*probe)) if probe else "<untraced>"
                out = repr(jax.eval_shape(kernel, *probe)) if probe else "<unknown-shape>"
                evidence = {
                    "mode": "xla",
                    "static_argnums": repr(static_argnums),
                    "out_shape": out,
                    "jaxpr_lines": str(jaxpr.count("\n")),
                }
                return Jitted(fn, self, evidence)
            case JitBackend(tag="none", none=()):
                return Jitted(kernel, self, {"mode": "none"})
            case unreachable:
                assert_never(unreachable)


class Jitted(Struct, frozen=True):
    fn: Callable[..., object]
    backend: JitBackend
    evidence: dict[str, str]

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "compute.jit", self.backend.tag, self.evidence)
```

## [03]-[RESEARCH]

- [NUMBA_JIT]: `numba`/`llvmlite` carry the `python_version<'3.15'` marker; the `njit(cache=, parallel=, fastmath=, boundscheck=)`, `vectorize(signatures, target=)`, `guvectorize(signatures, layout, target=)`, and the `CPUDispatcher.signatures`/`parallel_diagnostics`/`inspect_llvm` evidence-capture spellings verify against `compute/.api/numba.md` (IMPLEMENTATION_LAW DISPATCHER_TOPOLOGY names `inspect_llvm`/`parallel_diagnostics` as the IR/parallel-decision capture surface) once the numba wheel resolves on the gated band. `njit` is `jit(nopython=True)`; a decorated kernel becomes a `CPUDispatcher` that specializes one compiled signature per argument-type tuple, so `signatures` reads `<unspecialized>` and `inspect_llvm` is empty until a warm call — the optional `probe` argument forces that one warm specialization so the lowered-IR evidence is non-empty at capture. The `none` passthrough runs unconditionally on cp315.
- [JAX_JIT]: `jax`/`jaxlib` carry the `python_version<'3.15'` marker; the `jit(fun, static_argnums=, donate_argnums=)`, `make_jaxpr(fun, static_argnums=)`, and `eval_shape(fun, *args)` spellings verify against `compute/.api/jax.md` (ENTRYPOINTS [01] compile, vectorization-and-staging [05] `make_jaxpr`, [04] `eval_shape`) on the gated band. `static_argnums` marks compile-time-constant arguments whose change retraces; `make_jaxpr` traces the kernel to the jaxpr without executing and `eval_shape` returns the output `ShapeDtypeStruct` without compute, so the XLA evidence is the real traced jaxpr and output shape rather than a hardcoded literal. The transform requires a pure kernel, the same purity the solver and study kernels already hold. jax rides the Array-API namespace as a backend at `numerics/array.md#PAYLOAD`; here it is admitted only as the XLA kernel-transform arm, never duplicating the array admission.
