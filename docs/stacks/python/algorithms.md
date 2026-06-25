# [PYTHON_ALGORITHMS]

Numeric work is admitted once and routed by operand shape: raw arrays cross into a finite-checked owner at one boundary, the interior is total over admitted operands, and the operand's structure — definite, square, overdetermined, symmetric, sparse-pattern, periodic-grid — selects the owning factorization, never the call site and never a knob riding beside the matrix. Every solve route is one composed `admit -> route -> solve -> witness -> receipt` lifecycle written once; a per-module pipeline re-deriving the chain is the rejected form. `numpy` is the page's one numeric substrate cited at member depth — array admission, the dtype algebra, every element-wise and reduction ufunc, `einsum` contraction, `linalg`, `fft`, the finiteness predicates, and seeded sampling — while the rail substrate (`expression` carriers, `msgspec` receipt codec, `stamina` retry, `beartype` contract) is cited at depth as every page cites it. The factorization, spectral, quadrature, sparse, symbolic, dimensional, and uncertainty routes are owned by the compute companion band (`scipy.linalg`, `scipy.sparse.linalg`, `sympy`, `pint`, `uncertainties`), named where the implementation choice turns on them and pushed to the compute owner at member depth; this layer states the route law, never the per-member surface. Every library refuses its own gates — no constructor checks finiteness, array arithmetic returns `nan`/`inf` silently, a near-singular solve yields a garbage vector with no signal, an exact-`==` symmetry test fails on an accumulation-built matrix — so admission re-imposes each refused gate as an explicit predicate, and every result leaves as a frozen `msgspec.Struct` receipt carrying its route case, its scale-derived tolerance, and the recomputed true relative residual against the original operator, never a raw `ndarray` or factorization handle. The interior is expression-shaped end to end: the one statement seam is the held-factor close-over at the compute boundary, named where it appears.

## [01]-[ROUTE_SPINE]

The operand shape selects the route before any solve; the most specific shape wins, and the table is one lifecycle's route step, never a family of pipelines.

| [INDEX] | [OPERAND_SHAPE]                        | [OWNING_ROUTE]                                 | [CONDITIONING_FALLBACK]                   |
| :-----: | :------------------------------------- | :--------------------------------------------- | :---------------------------------------- |
|  [01]   | dense symmetric positive-definite      | dense Cholesky factor, held, SPD assumption    | shift `ε·I`, then the symmetric spectrum  |
|  [02]   | dense square general                   | dense pivoted LU factor, held                  | rank-revealing pseudo-inverse             |
|  [03]   | dense overdetermined                   | SVD-based least-squares over the normal form   | truncated SVD pseudo-inverse              |
|  [04]   | symmetric or Hermitian spectrum        | symmetric eigensolver, index-banded subset     | generalized eigensolver on the pair       |
|  [05]   | nonsymmetric spectrum                  | general eigensolver, Schur-pair decode         | residual witness, no resort               |
|  [06]   | rank, norm, or condition evidence only | one retained `numpy.linalg.svd` handle         | the one handle answers all three          |
|  [07]   | sparse SPD or symmetric, fixed pattern | sparse direct factor, held; fill read first    | conjugate-gradient while pattern unproven |
|  [08]   | sparse nonsymmetric, fixed pattern     | sparse direct factor, held                     | restarted Krylov + witness gate           |
|  [09]   | huge sparse or changing pattern        | matrix-free Krylov, incomplete-LU precondition | verdict-routed sparse direct solve        |
|  [10]   | symbolic operator, repeated evaluation | one `"numpy"`-lowered kernel, evaluated dense  | dense route on the lowered callable       |

Recover the fallback from the route value and record it on the receipt, never a caller-named entrypoint: a fill ratio (symbolic factor nonzeros over input nonzeros, read from the held sparse factor before the numeric sweep) routes direct versus iterative, and the regularized least-squares case (the design matrix stacked over a `√λ`-scaled identity) is the derived consequence of the conditioning budget exceeding the inverse cap. `scipy.linalg` and `scipy.sparse.linalg` are distinct namespaces, so hybrid routing is integrator-authored and carries its own residual validation. The fallback is a rebind onto the same lifecycle: primary and conditioning routes converge on the one witness gate, and the receipt records the taken path. The per-member factorization surface — the held-factor tuple, the assumption flag, the index-band, the incomplete-LU operator — is the compute owner's depth; this table fixes which substrate and which algebraic shape the operand selects.

[ROUTE_UNION]:
- Law: the `Route` `StrEnum` is the one closed vocabulary over every operand shape the route table lists — dense, sparse, iterative, symbolic — and it is the receipt's route discriminant, so a new substrate lands as one member and the receipt carries every taken path; the dense `@tagged_union` `FactorRoute(frozen=True)` whose `case()` payload is the admitted dense operator projects the dense subset of that vocabulary, the sparse and symbolic sections threading their own handle and callable. The dense dispatch is table-driven — the `OPERAND` projection, the `CONDITION` and `RANK_GATE` admission gates, and the per-route `Solver` are `frozendict` rows keyed on the route, never enumerated `match` arms — so a new dense factorization is one `Route` member plus one row in each table, every consumer untouched.
- Law: the per-route solver is one entry in a route-keyed `frozendict` policy table — the `scipy.linalg` factorization the route table names is the cell value, applied at the compute boundary that owns its member surface — and the interior threads only the admitted operator, the policy-derived conditioning convention, and the witness gate; the factorization's member surface is the compute owner's depth, this dispatch holds only the route key and the rail.
- Law: the route owns its conditioning convention as one frozen policy value derived from operand scale (a rank tolerance, a regularization budget); a bare literal threaded through the signature is the rejected form.
- Reject: a `compute_vectors: bool`, a `mode: str`, or a `sym: bool` parameter riding beside the matrix — a parallel knob re-describing the input is arity smuggled back; sibling `solve_spd`/`solve_general`/`solve_lstsq` functions where one routed dispatch discriminates on the operand value.
- Boundary: the element carrier is `numpy` `float64`; admission narrows the dtype and asserts C-contiguity once, and the interior never re-checks either.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from builtins import frozendict
from collections.abc import Callable
from enum import StrEnum
from typing import Literal

import numpy as np
from expression import Error, Ok, Result, case, tag, tagged_union

# --- [TYPES] ----------------------------------------------------------------------------
type SolveFault = Literal["<nan>", "<inf>", "<singular>", "<rank-deficient>", "<non-finite>", "<residual-exceeded>", "<contract>"]
type Solver = Callable[[np.ndarray, np.ndarray], tuple[np.ndarray, int]]
type Gate = Callable[[np.ndarray, np.ndarray, int], Result[np.ndarray, SolveFault]]


class Route(StrEnum):
    DEFINITE = "definite"
    SQUARE = "square"
    OVERDETERMINED = "overdetermined"
    SPECTRAL = "spectral"
    NONSYMMETRIC = "nonsymmetric"
    EVIDENCE = "evidence"
    SPARSE_DIRECT = "sparse-direct"
    SPARSE_ITERATIVE = "sparse-iterative"
    SYMBOLIC = "symbolic"


@tagged_union(frozen=True)
class FactorRoute:
    tag: Route = tag()
    definite: np.ndarray = case()
    square: np.ndarray = case()
    overdetermined: np.ndarray = case()
    spectral: np.ndarray = case()

# --- [OPERATIONS] -----------------------------------------------------------------------
def ranked(a: np.ndarray, x: np.ndarray, rank: int, /) -> Result[np.ndarray, SolveFault]:
    return Ok(x) if rank >= a.shape[1] else Error("<rank-deficient>")


PASS: Gate = lambda _a, x, _rank: Ok(x)
RANK_GATE: frozendict[Route, Gate] = frozendict({Route.OVERDETERMINED: ranked})
CONDITION: frozendict[Route, bool] = frozendict({Route.SQUARE: True, Route.DEFINITE: True})


OPERAND: frozendict[Route, Callable[[FactorRoute], np.ndarray]] = frozendict({
    Route.DEFINITE: lambda r: r.definite,
    Route.SQUARE: lambda r: r.square,
    Route.OVERDETERMINED: lambda r: r.overdetermined,
    Route.SPECTRAL: lambda r: r.spectral,
})


def solved(route: FactorRoute, solvers: frozendict[Route, Solver], b: np.ndarray, cap: float, /) -> Result[np.ndarray, SolveFault]:
    operand, gate = OPERAND[route.tag](route), RANK_GATE.get(route.tag, PASS)
    gated = (lambda m: conditioned(m, cap)) if CONDITION.get(route.tag, False) else Ok
    return admitted(operand).bind(gated).bind(
        lambda m: gate(m, *solvers[route.tag](m, b)).bind(lambda x: attested(m, x, b, cap))
    )
```

## [02]-[ADMISSION_GATES]

[FINITE_ADMISSION]:
- Law: gate every operand on one all-finite predicate over the flat array before factoring — `numpy.isfinite(a).all()` in a single vectorized pass — and split the typed rejection into a NaN-any then Inf-any cause only when the failure is real; no factorization rejects `nan`/`inf`, a non-finite entry propagates silently into a corrupted factor, and a Python-level per-element loop forfeits the one-pass vectorized admission the contiguous layout grants.
- Law: admission narrows the dtype to `float64` and forces C-contiguity once through `numpy.ascontiguousarray`, because a strided or object-dtype operand silently degrades every downstream BLAS call to a slow Python path; the narrowed array is the admitted operator the whole lifecycle threads.
- Reject: trusting a constructor; re-checking finiteness inside the interior; `numpy.nan_to_num` substituting a value for a non-finite entry instead of rejecting it.

[SYMMETRY_AND_SINGULAR]:
- Law: force symmetry with `(a + a.T) * 0.5` (`a.conj().T` for Hermitian) before the symmetric route; an exact float `==` symmetry test fails on an accumulation-built matrix, and a one-ULP mirror difference routes to the nonsymmetric path whose spectrum acquires spurious complex pairs. The SPD route reads only one triangle, so symmetrize first and never substitute a separate eigenvalue probe for the guarantee.
- Law: probe singularity through `numpy.linalg.cond(a)` against a scale-relative cap, never a determinant — a pivoted LU on a singular operand returns a finite garbage factor and never raises, and the determinant underflows to `0.0` with no signal. Read a genuinely needed determinant as the streaming log form `2·Σ log|L_ii|` from the Cholesky factor or `Σ log|U_ii|` from the LU factor; the plain product underflows and its logarithm is `-inf`.
- Law: read the least-squares solution rank against the expected column count and reject a deficiency; a rank-deficient design matrix returns a minimum-norm solution silently, and the returned rank is the only signal. The SVD-based least-squares route is the admitted form over the normal equations in near-dependent columns, because the normal equations square the condition number.
- Boundary: guard `numpy.linalg.cond` against `+inf` before gating; it is `+inf` for a rank-deficient operator and a comparison against it always fails closed.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Literal

import numpy as np
from expression import Error, Ok, Result

# --- [TYPES] ----------------------------------------------------------------------------
type SolveFault = Literal["<nan>", "<inf>", "<singular>", "<rank-deficient>", "<non-finite>", "<residual-exceeded>", "<contract>"]

# --- [OPERATIONS] -----------------------------------------------------------------------
def admitted(a: np.ndarray, /) -> Result[np.ndarray, SolveFault]:
    m = np.ascontiguousarray(a, dtype=np.float64)
    flat = m.reshape(-1)
    return Ok(m) if np.isfinite(flat).all() else Error("<nan>") if np.isnan(flat).any() else Error("<inf>")


def conditioned(a: np.ndarray, cap: float, /) -> Result[np.ndarray, SolveFault]:
    kappa = float(np.linalg.cond(a))
    return Error("<singular>") if not np.isfinite(kappa) or kappa > cap else Ok(a)
```

## [03]-[TOLERANCE_AND_HELD_HANDLE]

[SCALE_DERIVED_TOLERANCE]:
- Law: derive every threshold from operator and right-hand-side scale — `σ_max` from the SVD, `‖A‖_F` via `numpy.linalg.norm`, `‖b‖∞` — and carry it as one named `frozendict` policy row on the receipt; a bare per-module absolute literal in `1e-4..1e-8` is unreplayable and uncomparable across operators, the rejected form.
- Law: recompute domain rank as `count(σ_i > ε_rank · σ_max)` with a caller-supplied relative `ε_rank`; the LAPACK default rank cut misclassifies significant singular values for an operator with `‖A‖₂ ~ 10⁶`.
- Reject: a fresh absolute literal per call site — the conditioning policy is one row keyed by the route case, derived once from the admitted operator's scale, never re-typed beside each solve.

[HELD_HANDLE]:
- Law: retain one factorization handle for streamed right-hand sides — the held factor the route names returns its factor tuple once and each solve is one triangular back-substitution; the held factorization enters the interior as one `Solve` closure bound at the compute boundary, and a loop of fresh factorizations is cubic per call where the held handle is quadratic.
- Law: retain one thin `numpy.linalg.svd` factorization for rank, 2-norm (`s[0]`), and condition (`s[0]/s[-1]`); each separate query otherwise rebuilds the cubic decomposition, while the induced `numpy.linalg.norm` reductions stay quadratic, touching no decomposition.
- Law: form the iterative-refinement residual against the original operator in working precision (`b - a @ x`), never against reconstructed factors — the factors carry exactly the rounding error the correction exists to cancel — and thread the correction as one `expression.Block.fold` over a fixed budget, never a mutating loop.
- Reject: `numpy.linalg.inv` in a hot loop — it factors plus solves against an identity; solve against the held handle with reused buffers.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Literal

import numpy as np
from expression import Error, Ok, Result
from expression.collections import Block

# --- [TYPES] ----------------------------------------------------------------------------
type RefineFault = Literal["<non-finite-residual>", "<stalled>"]
type Solve = Callable[[np.ndarray], np.ndarray]

# --- [OPERATIONS] -----------------------------------------------------------------------
def refined(a: np.ndarray, held: Solve, b: np.ndarray, x: np.ndarray, tol: float, cap: int, /) -> Result[np.ndarray, RefineFault]:
    b_inf = max(float(np.linalg.norm(b, np.inf)), np.finfo(np.float64).tiny)
    relative = lambda v: float(np.linalg.norm(b - a @ v, np.inf)) / b_inf

    def correct(state: np.ndarray, _: int, /) -> np.ndarray:
        return state if relative(state) <= tol else state + held(b - a @ state)

    settled = Block.range(cap).fold(correct, x)
    residual = relative(settled)
    return Error("<non-finite-residual>") if not np.isfinite(residual) else Ok(settled) if residual <= tol else Error("<stalled>")
```

## [04]-[SPARSE_AND_ITERATIVE]

[SPARSE_ROUTE]:
- Law: a fixed-pattern sparse operator factors once through the `scipy.sparse.linalg` direct factor and reuses the held object for every right-hand side; the held factor's nonzero count over the input nonzero count is the fill ratio read before the numeric sweep, and a fill ratio above the budget routes to an iterative solve instead.
- Law: an iterative solve — conjugate-gradient for SPD, a restarted-Krylov mirror otherwise — carries its convergence criterion as the call's relative and absolute residual bounds and its iteration ceiling, returns an `(x, info)` verdict pair, and a nonzero `info` is a non-convergence fault routed to the verdict-gated direct fallback, never a silently accepted partial iterate; this `(x, info)` contract is where the choice turns, so the page fixes it while the per-solver member rides the compute owner.
- Law: a preconditioner is the matrix-free linear operator passed through the solver's preconditioner slot; an incomplete-LU factor lowered to that operator is the standard accelerant and rides the route value, never a global default.
- Reject: rebuilding the sparse factorization per right-hand side; accepting an iterative iterate without checking the verdict code; densifying a sparse operator to reuse the dense route.

[TERMINAL_PARTITION]:
- Law: partition the solver terminus through an `@tagged_union` `SolveTerminal` over `admitted | exhausted | broken`, mapping the `(x, info)` verdict onto one case — converged to `admitted`, count-exhausted to `exhausted` carrying the partial iterate and the binding budget, breakdown to `broken` — so the relaxed-criterion or direct-solve retry reads the residual budget off the value rather than re-running the solve.
- Law: the breakdown semantics are a solver-kind property, not a call-site flag: the same near-zero inner-product denominator throws in one solver, cancels the iterate in a second, and substitutes `1` and continues in a third, so the terminus case is read from the solver identity and the witness gate is the only signal that survives the substitution path.
- Reject: collapsing count-exhaustion into the failure rail — it destroys the caller's seeded-direct retry; accepting the substitution iterate under a converged verdict without the independent residual witness.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Literal, assert_never

import numpy as np
from expression import Error, Ok, Result, case, tag, tagged_union

# --- [TYPES] ----------------------------------------------------------------------------
type IterFault = Literal["<broken>", "<residual-exceeded>"]


@tagged_union(frozen=True)
class SolveTerminal:
    tag: Literal["admitted", "exhausted", "broken"] = tag()
    admitted: np.ndarray = case()
    exhausted: tuple[np.ndarray, int] = case()
    broken: int = case()

# --- [OPERATIONS] -----------------------------------------------------------------------
def settled(verdict: SolveTerminal, witness: float, cap: float, /) -> Result[np.ndarray, IterFault]:
    passed = np.isfinite(witness) and witness <= cap
    match verdict:
        case SolveTerminal(tag="admitted"):
            return Ok(verdict.admitted) if passed else Error("<residual-exceeded>")
        case SolveTerminal(tag="exhausted"):
            partial, _budget = verdict.exhausted
            return Ok(partial) if passed else Error("<residual-exceeded>")
        case SolveTerminal(tag="broken"):
            return Error("<broken>")
        case never:
            assert_never(never)
```

## [05]-[WITNESS_AND_RECEIPT]

[RESIDUAL_WITNESS]:
- Law: every solve — dense, sparse, direct, iterative, primary, fallback — converges on one witness gate that recomputes the true relative residual `‖b - A·x‖ / ‖b‖` against the original operator and compares it to the scale-derived cap; this single signal survives preconditioning, breakdown substitution, cancellation, and provider divergence, and a solve that returns without passing the gate is the rejected form.
- Law: the witness reads the residual in the operator's own working precision and rejects a non-finite residual before comparing it to the tolerance; a `nan` residual compared with `<=` silently passes, so the finite check precedes the magnitude check.

[TYPED_RECEIPT]:
- Law: every result leaves as one frozen `msgspec.Struct` `SolveReceipt` carrying the route case as a `Route` member, the scale-derived tolerance, and the recomputed residual — the numeric evidence this layer owns; it never carries the operator, the factorization handle, or an eagerly decoded solution array, because the numeric block is large and its decode is the consumer's choice. The solution defers as a `Raw` field whose octet capture and zero-copy reconstruction are the `boundaries.md` wire owner's mechanic, composed here, not taught here — this card fixes only which evidence the receipt holds.
- Law: the egress weave is the `aspected` factory `surfaces-and-dispatch.md` owns, applied over the pure `witnessed` core: `stamina.retry` on the transient provider fault and the runtime span enter as `*composed` weaves already built at their owners, and the factory's fixed contract arm lifts a `BeartypeCallHintViolation` through `lifted` — the witness re-spells none of them, and a failing weave folds onto the `SolveFault` rail rather than escaping. No `numpy.linalg.LinAlgError` capture rides this weave: the `<singular>` cap-against-`cond` gate at admission already precludes the near-singular factorization that raises it, so a re-catch at egress would re-impose a gate the interior owns once.
- Boundary: the span attribute map is the receipt's scalar projection — route, tolerance, residual — never the `Raw` solution bytes, and the projection is the exact `str | float` shape `runtime.md`'s emission weave accepts; the span and structured-emission mechanics that consume it are `runtime.md`'s, this layer states only what crosses.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import numpy as np
import stamina
from expression import Error, Ok, Result
from msgspec import Raw, Struct

# --- [MODELS] ---------------------------------------------------------------------------
class SolveReceipt(Struct, frozen=True, gc=False):
    route: Route
    tolerance: float
    residual: float
    solution: Raw

# --- [OPERATIONS] -----------------------------------------------------------------------
def attested(a: np.ndarray, x: np.ndarray, b: np.ndarray, cap: float, /) -> Result[np.ndarray, SolveFault]:
    residual = float(np.linalg.norm(b - a @ x)) / max(float(np.linalg.norm(b)), np.finfo(np.float64).tiny)
    return Error("<non-finite>") if not np.isfinite(residual) else Ok(x) if residual <= cap else Error("<residual-exceeded>")


@aspected(stamina.retry(on=ConnectionError, attempts=3), lifted=lambda _v: "<contract>")
def witnessed(route: Route, a: np.ndarray, x: np.ndarray, b: np.ndarray, cap: float, /) -> Result[SolveReceipt, SolveFault]:
    block = lambda x_ok: np.ascontiguousarray(x_ok, dtype=np.float64).tobytes()
    residual = lambda x_ok: float(np.linalg.norm(b - a @ x_ok)) / max(float(np.linalg.norm(b)), np.finfo(np.float64).tiny)
    return attested(a, x, b, cap).map(lambda x_ok: SolveReceipt(route, cap, residual(x_ok), Raw(block(x_ok))))
```

## [06]-[SYMBOLIC_UNITS_UNCERTAINTY]

[SYMBOLIC_LOWERING]:
- Law: the `sympy` symbolic-derivation route — differentiation, simplification, common-subexpression elimination, then lowering to a `"numpy"`-backend vectorized callable — produces one `Kernel` exactly once at the compute boundary, and this layer admits that produced callable into the numeric interior under a finite-sample gate; the symbolic member surface is the compute owner's depth, and the interior calls the lowered callable and never re-evaluates the symbolic tree per sample. The `"numpy"` lowering target is named because it fixes the backend the admitted kernel runs on.
- Law: a runtime derivative is lowered with its primal in one tuple-lowering so value and gradient share one compiled kernel and one subexpression pass — the lowering boundary's choice, named here only because it changes what crosses — and the admitted callable then enters the dense route as an ordinary operator.
- Reject: symbolic per-sample evaluation in a loop; re-deriving the symbolic expression per call; admitting the lowered kernel without the finite-sample gate that rejects a `nan`/`inf`-producing derivation before it poisons the interior.

[DIMENSIONAL_AND_ERROR]:
- Law: the `pint` units route admits dimensioned input at the boundary — one module-level registry, a single strip to the canonical base-unit magnitude — before the array crosses the finite-admission gate; the interior is unit-free `float64` and the receipt re-attaches the unit on egress, the registry and the magnitude-strip being the compute boundary's surface, named here only as the admission choice.
- Law: the `uncertainties` route carries correlated error when propagation is the deliverable — a fit covariance lifted into auto-propagating arithmetic — strictly as a boundary-and-receipt concern, never threaded through the dense factorization hot path where the object dtype defeats BLAS.
- Reject: stringly-typed unit suffixes on field names; manual error-propagation arithmetic where the route derives the partials from its own shared-variable graph; mixing a dimensioned quantity into a BLAS call.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Literal

import numpy as np
from expression import Error, Ok, Result

# --- [TYPES] ----------------------------------------------------------------------------
type KernelFault = Literal["<non-finite-kernel>", "<wrong-arity>"]
type Kernel = Callable[..., tuple[np.ndarray, np.ndarray]]

# --- [OPERATIONS] -----------------------------------------------------------------------
def admitted_kernel(lowered: Kernel, arity: int, /) -> Result[Kernel, KernelFault]:
    probe = (np.zeros(1) for _ in range(arity))
    sample = lowered(*probe)
    return (
        Error("<wrong-arity>") if len(sample) != 2
        else Ok(lowered) if all(np.isfinite(np.asarray(part)).all() for part in sample)
        else Error("<non-finite-kernel>")
    )
```

## [07]-[OWNED_BUILDS]

The constant-coefficient periodic operators and the seeded sampler have no shared-substrate solver surface; they are composed from `numpy.fft`, the seeded `numpy.random.Generator`, and the rails, with the route owning the policy the providers refuse.

[SPECTRAL_OPERATOR]:
- Law: collapse every constant-coefficient periodic operator to one symbol applied pointwise to the forward `numpy.fft.rfft`; the symbol is a `frozendict` policy row keyed by a closed `StrEnum` operator vocabulary, composed by pointwise multiplication before a single `irfft`, and a new operator is a new row, never a new code path.
- Law: derive the split-spectrum wavenumber once at grid construction through `numpy.fft.rfftfreq`, scaled by `2π/extent`; hand-indexing the bin number applies an aliased symbol past the half length silently, and the Nyquist bin is zeroed for odd-order symbols.
- Boundary: mark the result inadmissible on a non-finite inverse; a real-symbol operator owes a finite real field, and a `nan` diagnoses broken Hermitian symmetry in the assembled spectrum.

[SEEDED_SAMPLING]:
- Law: draw stochastic samples through one explicit `numpy.random.default_rng(seed)` over a state-serializable generator for checkpoint-resume, deriving independent parallel streams through `SeedSequence.spawn`; the default entropy source is non-deterministic regardless of seeding, so the seed and the spawned child index ride the receipt as the replay key.
- Boundary: a stochastic estimate leaves as a replicate family whose cross-replicate spread is the receipt's variance evidence; a single draw carries no recoverable variance.

```python conceptual
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from builtins import frozendict
from collections.abc import Callable
from enum import StrEnum
from typing import Literal

import numpy as np
from expression import Error, Ok, Result

# --- [TYPES] ----------------------------------------------------------------------------
type SpectralFault = Literal["<broken-symmetry>"]


class Operator(StrEnum):
    LAPLACIAN = "laplacian"
    GRADIENT = "gradient"
    HELMHOLTZ = "helmholtz"

# --- [CONSTANTS] ------------------------------------------------------------------------
SYMBOL: frozendict[Operator, Callable[[np.ndarray], np.ndarray]] = frozendict({
    Operator.LAPLACIAN: lambda k: -(k**2),
    Operator.GRADIENT: lambda k: 1j * k,
    Operator.HELMHOLTZ: lambda k: 1.0 / (1.0 + k**2),
})

# --- [OPERATIONS] -----------------------------------------------------------------------
def driven(field: np.ndarray, extent: float, operator: Operator, /) -> Result[np.ndarray, SpectralFault]:
    k = np.fft.rfftfreq(field.size, d=extent / field.size) * (2.0 * np.pi)
    spectrum = np.fft.rfft(field) * SYMBOL[operator](k)
    spectrum[-1] = 0.0 if field.size % 2 == 0 else spectrum[-1]
    inverted = np.fft.irfft(spectrum, n=field.size)
    return Ok(inverted) if np.isfinite(inverted).all() else Error("<broken-symmetry>")
```
