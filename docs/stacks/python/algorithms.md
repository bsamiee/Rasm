# [PYTHON_ALGORITHMS]

Numeric work is admitted once and routed by operand shape: raw arrays cross into a finite-checked owner at one boundary, the interior is total over admitted operands, and the operand's structure — definite, square, overdetermined, symmetric, sparse-pattern, periodic-grid — selects the owning factorization, never the call site and never a knob riding beside the matrix. Every solve route is one composed `admit -> route -> solve -> witness -> receipt` lifecycle written once; a per-module pipeline re-deriving the chain is the rejected form. `numpy` owns array admission and dense element-wise primitives; `scipy.linalg` owns dense factorization, spectral decomposition, and quadrature; `scipy.sparse.linalg` owns sparse direct and iterative solves; `sympy` owns symbolic derivation lowered to a `numpy` kernel through `lambdify`; `pint` owns dimensional admission and `uncertainties` owns error propagation. Every library refuses its own gates — no constructor checks finiteness, `numpy` arithmetic returns `nan`/`inf` silently under the default `errstate`, a near-singular solve yields a garbage vector with no signal — so admission re-imposes each refused gate as an explicit predicate, and every result leaves as a domain receipt carrying its route, its scale-derived tolerance, and the recomputed true relative residual against the original operator, never a raw `ndarray` or factorization handle. The in-place library kernels — `cho_solve`/`lu_solve` writing into pre-sized scratch, `numpy.errstate` blocks fencing a probe — are this page's named statement exemption.

## [01]-[ROUTE_SPINE]

The operand shape selects the route before any solve; the most specific shape wins, and the table is one lifecycle's route step, never a family of pipelines.

| [INDEX] | [OPERAND_SHAPE]                        | [OWNING_ROUTE]                         | [CONDITIONING_FALLBACK]               |
| :-----: | :------------------------------------- | :------------------------------------- | :------------------------------------ |
|  [01]   | dense symmetric positive-definite      | `scipy.linalg.cho_factor`              | shift `ε·I`, then symmetric `eigh`    |
|  [02]   | dense square general                   | `scipy.linalg.lu_factor`               | rank-revealing `svd` pseudo-inverse   |
|  [03]   | dense overdetermined                   | `scipy.linalg.lstsq` (thin QR)         | `svd` truncated pseudo-inverse        |
|  [04]   | symmetric or Hermitian spectrum        | `scipy.linalg.eigh`                    | generalized `eigh` with `b=` operator |
|  [05]   | nonsymmetric spectrum                  | `scipy.linalg.eig`                     | residual witness, no resort           |
|  [06]   | rank, norm, or condition evidence only | retained `scipy.linalg.svd` factors    | one factorization answers all three   |
|  [07]   | sparse SPD or symmetric, fixed pattern | `scipy.sparse.linalg.splu` (held)      | `cg` while structure unproven         |
|  [08]   | sparse nonsymmetric, fixed pattern     | `scipy.sparse.linalg.splu` (held)      | `gmres` + witness gate                |
|  [09]   | huge sparse or changing pattern        | `scipy.sparse.linalg.gmres`/`bicgstab` | verdict-routed direct `spsolve`       |
|  [10]   | symbolic operator, repeated evaluation | `sympy.lambdify(..., "numpy")` kernel  | dense route on the lowered callable   |

Recover the fallback from the route value and record it on the receipt, never a caller-named entrypoint: a fill ratio (symbolic factor nonzeros over input nonzeros, read from the `splu` object's `nnz` before the numeric sweep) routes direct versus iterative, and the regularized least-squares case (the design matrix stacked over a `√λ`-scaled identity under `lstsq`) is the derived consequence of the conditioning budget exceeding the inverse cap. `scipy.linalg` and `scipy.sparse.linalg` are distinct namespaces, so hybrid routing is integrator-authored and carries its own residual validation. The fallback is a rebind onto the same lifecycle: primary and conditioning routes converge on the one witness gate, and the receipt records the taken path.

[ROUTE_UNION]:
- Law: discriminate the factorization on the operand through a `@tagged_union` `FactorRoute` carrying its mode, symmetry assumption, and vector demand as case data recoverable by matching the value; the `match` over its cases is the totality proof with `assert_never` on the unreachable arm.
- Law: the route owns the conditioning convention — a rank tolerance, a regularization budget — as one frozen policy value on the case, derived from operand scale, never a bare literal threaded through the signature.
- Reject: a `compute_vectors: bool`, a `mode: str`, or a `sym: bool` parameter riding beside the matrix; a parallel knob re-describing the input is arity smuggled back; sibling `solve_spd`/`solve_general`/`solve_lstsq` functions where one routed dispatch discriminates on the operand.
- Boundary: the element carrier is `numpy` `float64`; admission narrows the dtype once at the boundary and the interior never re-checks it.

```python conceptual
from dataclasses import dataclass
from typing import Literal, assert_never

import numpy as np
import scipy.linalg as sla
from expression import Error, Ok, Result, case, tag, tagged_union

type SolveFault = Literal["<non-finite>", "<singular>", "<residual-exceeded>"]


@tagged_union(frozen=True)
class FactorRoute:
    tag: Literal["definite", "square", "overdetermined", "spectral"] = tag()
    definite: np.ndarray = case()
    square: np.ndarray = case()
    overdetermined: np.ndarray = case()
    spectral: np.ndarray = case()


def solved(route: FactorRoute, b: np.ndarray, cap: float, /) -> Result[np.ndarray, SolveFault]:
    match route:
        case FactorRoute(tag="definite"):
            return admitted(route.definite).bind(lambda a: witnessed(a, sla.cho_solve(sla.cho_factor(a), b), b, cap))
        case FactorRoute(tag="square"):
            return admitted(route.square).bind(lambda a: witnessed(a, sla.lu_solve(sla.lu_factor(a), b), b, cap))
        case FactorRoute(tag="overdetermined"):
            return admitted(route.overdetermined).bind(lambda a: witnessed(a, sla.lstsq(a, b)[0], b, cap))
        case FactorRoute(tag="spectral"):
            return admitted(route.spectral).map(lambda a: sla.eigh(a, eigvals_only=False)[0])
        case unreachable:
            assert_never(unreachable)
```

## [02]-[ADMISSION_GATES]

[FINITE_ADMISSION]:
- Law: gate every operand on one all-finite predicate over the flat array before factoring — `numpy.isfinite(a).all()` in a single vectorized pass, with a NaN-any then Inf-any split when the typed rejection must name its cause; no factorization rejects `nan`/`inf`, a non-finite entry propagates silently into a corrupted factor, and a Python-level per-element loop forfeits the one-pass vectorized admission the contiguous layout grants.
- Law: admission narrows dtype to `float64` and asserts C-contiguity once (`numpy.ascontiguousarray`), because a strided or object-dtype operand silently degrades every downstream BLAS call to a slow Python path.
- Reject: trusting a constructor; re-checking finiteness inside the interior; `numpy.nan_to_num` substituting a value for a non-finite entry instead of rejecting it.

```python conceptual
import numpy as np
from expression import Error, Ok, Result
from typing import Literal

type AdmitFault = Literal["<nan>", "<inf>"]


def admitted(a: np.ndarray, /) -> Result[np.ndarray, AdmitFault]:
    flat = np.ascontiguousarray(a, dtype=np.float64).reshape(-1)
    return Ok(a) if np.isfinite(flat).all() else Error("<nan>") if np.isnan(flat).any() else Error("<inf>")
```

[SYMMETRY_FORCING]:
- Law: force symmetry with `(a + a.T) * 0.5` (`a.conj().T` for Hermitian) before the symmetric route; an exact float `==` symmetry test fails on an accumulation-built matrix, and a one-ULP mirror difference silently routes to the nonsymmetric path whose spectrum acquires spurious complex pairs.
- Law: the SPD route reads only one triangle — `cho_factor` defaults to the upper — so symmetrize first and never substitute a separate eigenvalue probe for the symmetry guarantee.
- Reject: an in-place self-averaging loop; it mutates the backing array while reading mirror entries, corrupting the average.

[SINGULAR_AND_ZERO_NORM]:
- Law: probe singularity through the conditioning estimate, not a determinant — `numpy.linalg.cond(a)` against a scale-relative cap, because `lu_factor` on a singular operand returns a finite garbage factor and never raises, and the determinant underflows to `0.0` with no signal.
- Law: read the streaming log-determinant as `2·Σ log|L_ii|` from the Cholesky factor or `Σ log|U_ii|` from the LU factor when a determinant is genuinely needed; the plain product underflows and its logarithm is `-inf`.
- Law: check the least-squares residual rank from `lstsq`'s returned `rank` against the expected column count; a rank-deficient design matrix returns a minimum-norm solution silently, and the rank is the only signal.
- Use: `scipy.linalg.lstsq` (LAPACK `gelsd`, SVD-based) over the normal-equations route in near-dependent columns; the normal equations square the condition number.

## [03]-[DENSE_FACTOR_LAW]

[RANK_AND_TOLERANCE]:
- Law: derive every threshold from operator and right-hand-side scale — `σ_max` from the SVD, `‖A‖_F` via `numpy.linalg.norm`, `‖b‖∞` — and carry it as one named policy value on the receipt; a bare per-module absolute literal in `1e-4..1e-8` is the rejected form, unreplayable and uncomparable across operators.
- Law: recompute domain rank as `count(σ_i > ε_rank · σ_max)` with a caller-supplied relative `ε_rank`; the LAPACK default rank cut misclassifies significant singular values for an operator with `‖A‖₂ ~ 10⁶`.
- Boundary: guard `numpy.linalg.cond` against `+inf` before gating; it is `+inf` for a rank-deficient operator and a comparison against it always fails closed.

[HELD_HANDLE]:
- Law: retain one factorization handle for streamed right-hand sides — `cho_factor`/`lu_factor` returns the factor tuple once and each solve is one `cho_solve`/`lu_solve` triangular pass; a loop of fresh factorizations is the rejected form, cubic per call where the held handle is quadratic.
- Law: retain one `scipy.linalg.svd(a, full_matrices=False)` factorization for rank, 2-norm (`s[0]`), and condition (`s[0]/s[-1]`); each separate query otherwise rebuilds the cubic decomposition.
- Law: form the iterative-refinement residual against the original operator in working precision (`b - a @ x`), never against reconstructed factors — the factors carry exactly the rounding error the correction exists to cancel.
- Reject: `numpy.linalg.inv` in a hot loop — it factors plus solves against an identity; solve against the held handle with reused buffers.

```python conceptual
import numpy as np
import scipy.linalg as sla
from expression import Error, Ok, Result
from typing import Literal

type RefineFault = Literal["<non-finite-residual>", "<stalled>"]


def refined(a: np.ndarray, b: np.ndarray, x: np.ndarray, tol: float, cap: int, /) -> Result[np.ndarray, RefineFault]:
    factors = sla.lu_factor(a)
    b_norm = float(np.linalg.norm(b, np.inf))

    def step(state: np.ndarray, _: int, /) -> np.ndarray:
        return state + sla.lu_solve(factors, b - a @ state)

    seed = (x, float(np.linalg.norm(b - a @ x, np.inf)) / b_norm)
    refined_x = next(
        (cursor for cursor, _ in ((step(x, i), 0) for i in range(cap)) if float(np.linalg.norm(b - a @ cursor, np.inf)) / b_norm <= tol),
        step(x, cap - 1),
    )
    residual = float(np.linalg.norm(b - a @ refined_x, np.inf)) / b_norm
    return Ok(refined_x) if np.isfinite(residual) and residual <= tol else Error("<stalled>")
```

## [04]-[SPARSE_AND_ITERATIVE]

[SPARSE_ROUTE]:
- Law: a fixed-pattern sparse operator factors once through `scipy.sparse.linalg.splu` and reuses the held object for every right-hand side; the `splu` result's `nnz` over the input `nnz` is the fill ratio read before the numeric sweep, and a fill ratio above the budget routes to an iterative solve instead.
- Law: an iterative solve — `cg` for SPD, `gmres`/`bicgstab` otherwise — carries its convergence criterion as the call's `rtol`/`atol` and `maxiter`, returns an `(x, info)` pair, and `info != 0` is a non-convergence fault routed to the verdict-gated direct fallback, never a silently accepted partial iterate.
- Law: a preconditioner is a `scipy.sparse.linalg.LinearOperator` passed as `M=`; an incomplete-LU preconditioner (`spilu` lowered through `LinearOperator`) is the standard accelerant and rides the route value, never a global default.
- Reject: rebuilding the sparse factorization per right-hand side; accepting an iterative iterate without checking `info`; densifying a sparse operator to reuse the dense route.

[RESIDUAL_WITNESS]:
- Law: every solve — dense, sparse, direct, iterative, primary, fallback — converges on one witness gate that recomputes the true relative residual `‖b - A·x‖ / ‖b‖` against the original operator and compares it to the scale-derived cap; this single signal survives preconditioning, breakdown substitution, cancellation, and provider divergence, and a solve that returns without passing the gate is the rejected form.
- Law: the witness reads the residual in the operator's own working precision and rejects a non-finite residual before comparing it to the tolerance; a `nan` residual compared with `<=` silently passes.
- Boundary: the receipt carries the residual value, the route taken, and the tolerance policy; it never carries the operator, the factorization, or the solution array's bytes.

```python conceptual
import numpy as np
from expression import Error, Ok, Result
from typing import Literal

type WitnessFault = Literal["<non-finite-residual>", "<residual-exceeded>"]


def witnessed(a: np.ndarray, x: np.ndarray, b: np.ndarray, cap: float, /) -> Result[np.ndarray, WitnessFault]:
    residual = float(np.linalg.norm(b - a @ x)) / max(float(np.linalg.norm(b)), np.finfo(np.float64).tiny)
    return Error("<non-finite-residual>") if not np.isfinite(residual) else Ok(x) if residual <= cap else Error("<residual-exceeded>")
```

## [05]-[SYMBOLIC_UNITS_UNCERTAINTY]

[SYMBOLIC_LOWERING]:
- Law: `sympy` owns the symbolic derivation — differentiation, simplification, common-subexpression elimination through `sympy.cse` — and the closed-form result lowers to a vectorized `numpy` kernel through `sympy.lambdify(args, expr, "numpy")` exactly once at the boundary; the interior calls the lowered callable, never re-evaluates the symbolic tree per sample.
- Law: a derivative needed at runtime is derived symbolically once (`sympy.diff`) and lowered with the primal in one `lambdify` of a tuple, so the gradient and value share one compiled kernel and one common-subexpression pass.
- Reject: `sympy.evalf` in a per-sample loop; re-deriving the same symbolic expression at every call; a hand-coded Jacobian where `sympy.diff` lowered through `lambdify` is exact.

[DIMENSIONAL_AND_ERROR]:
- Law: `pint` admits dimensioned input at the boundary — one module-level `pint.UnitRegistry`, quantities carry their unit, and the boundary strips to the canonical base unit's magnitude before the array crosses into the finite-admission gate; the interior is unit-free `float64` and the receipt re-attaches the unit on egress.
- Law: `uncertainties.ufloat` (or `unumpy` arrays) carries correlated error through a derivation when error propagation is the deliverable; it is a boundary-and-receipt concern, never threaded through the dense factorization hot path where it defeats BLAS.
- Reject: stringly-typed unit suffixes on field names; manual error-propagation arithmetic where `uncertainties` derives the partials; mixing a dimensioned quantity into a BLAS call.

## [06]-[RESEARCH]

- [NUMERIC_CATALOGUE]: the `compute` package `.api` catalogues for `numpy`, `scipy`, `sympy`, `pint`, and `uncertainties` are capture-pending; the dense and sparse LAPACK route spellings here are the stable public surface and the catalogue capture confirms the exact return-tuple arity and the `info`/`rank` field names per provider version.
