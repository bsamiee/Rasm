# [PY_COMPUTE_LINEAR]

The linear-algebra routes of the one numeric solver. `LinearIntent` discriminates dense systems, sparse systems by scheme (direct/factored/Krylov/least-squares), eigen-and-spectral problems, and the autodifferentiable `lineax` operator tier that unifies dense, sparse, and iterative solves and least-squares over one general linear operator. One `LinearMap` value object carries a dense `np.ndarray`, an admitted `scipy.sparse` container, or a matrix-free `matvec`, plus one `MatrixStructure` policy value (`GENERAL`/`SYMMETRIC`/`SPD`/`LOWER_TRIANGULAR`/`UPPER_TRIANGULAR`/`TRIDIAGONAL`/`DIAGONAL`) that is the SINGLE structure axis every route reads.

That structure value drives all three backends. The enum value IS the `assume_a` string, so it picks the dense LAPACK driver through `scipy.linalg.solve(a, b, assume_a=...)`; it resolves the `lineax` `tags` frozenset that `lineax.AutoLinearSolver(well_posed=True)` reads to select `Cholesky`/`Triangular`/`Tridiagonal`/`Diagonal`/`LU`; and it lets the `LinearEngine.operator` projection lift a diagonal or tridiagonal operand to `lineax.DiagonalLinearOperator`/`lineax.TridiagonalLinearOperator(diagonal, lower_diagonal, upper_diagonal)`. One operand shape plus one structure value feeds every route, and the matrix-free path lifts to a `scipy.sparse.linalg.LinearOperator` and a `lineax.FunctionLinearOperator` without a parallel matrix-free owner.

Two more policy values complete the bounded vocabulary in place of boolean knobs. `SolveShape` (`SQUARE`/`LEAST_SQUARES`/`MIN_NORM`) replaces a `least_squares` flag and selects the dense `solve`-vs-`lstsq` arm, the `lineax` `AutoLinearSolver`/`QR`/`LSMR`/`Normal(CG)` solver cell, and the sparse `spsolve`-vs-`lsqr` arm; `SpectralMode` (`EIGENPAIRS`/`SPECTRUM`) replaces a `spectral` flag and selects `eigh`/`eigsh` against `svds`/`svdvals`. The four tuning axes that scatter as `(rtol, atol, maxiter)` literals across a naive linear page ride one `LinearPolicy` value object instead: the residual tolerance, the Krylov `maxiter` cap, the optional matrix-free `preconditioner`, and the `batched` multi-RHS sweep flag that vmaps one operator over a stack of right-hand sides through `equinox.filter_vmap` as one compiled differentiable solve.

The `lineax` tier is woven as one rail, not a flat per-library call: the frozen `LinearEngine` value object folds the gated `jax`/`jnp`/`lx` modules behind `LinearEngine.gated()`, which runs `jax.config.update("jax_enable_x64", True)` before the solve — the `1e-10` `(rtol, atol)` is below float32 eps (`~1.2e-7`) so without the float64 promotion the termination criterion is unsatisfiable and a downcast operand loses the precision the receipt residual adjudicates against, the same x64 contract every sibling JAX route (`solvers/nonlinear.md#NONLINEAR`, `solvers/differential.md#DIFFERENTIAL`, `solvers/sensitivity.md#SENSITIVITY`) carries. The carrier owns the structure-tagged operator build, the `SolveShape`/structure solver cell, and the lineax-rail residual `‖operator.mv(x) - b‖` over the operator's own `.mv`, so `LinearMap` stays a core value object carrying no gated import and the differentiable route never re-enters the scipy operand projection to score a JAX solve.

Every route folds into the one `SolverReceipt`, and every iterative and operator route folds the backend's *termination reason* — the scipy `info`/`istop` code through one `_info_status`/`_ISTOP` fold and the `lineax.Solution.result` member name — into `SolveStatus`, so a CG/GMRES non-convergence or a singular factorization is a first-class typed verdict on the receipt, never a silent residual-floor pass and never a raised exception. `LinearIntent.solve` is the one entry method on the union (matching `NonlinearIntent.solve`/`DifferentialIntent.solve`/`FieldQuery.evaluate`), and the receipt's `@receipted` `SolverReceipt.contribute` owns the observability emission rather than an inline `Signals.emit`. The dense numpy floor runs unconditionally on runtime; the scipy sparse/eigen bodies and the `lineax` tier are the worker lane.

## [01]-[INDEX]

- [01]-[LINEAR]: dense/sparse/eigen routes over scipy plus the `lineax` autodifferentiable operator case, all folded into one `LinearIntent` owner reading one `LinearMap` operand, three bounded policy values (`MatrixStructure`/`SolveShape`/`SpectralMode`), and one `LinearPolicy` tuning value, the gated `jnp`/`lx` modules folded into one `LinearEngine` carrier floating the rail to float64, with batched multi-RHS sweeps through `equinox.filter_vmap` and typed non-convergence on the receipt.

## [02]-[LINEAR]

- Owner: `LinearIntent` — the four linear-route cases on the one solver, each reading one `LinearMap` operand. `DenseLa(map, rhs, shape)` runs `scipy.linalg.solve(a, b, assume_a=m.structure.value)` for the square arm — the structure value alone selecting the LAPACK driver, the SPD `"pos"` value reaching the Cholesky driver with no `cho_*` special case — and `scipy.linalg.lstsq` for the `SolveShape`/non-square arm, with a `np.linalg.solve`/`lstsq` floor; `Sparse(map, rhs, scheme)` runs the `SparseScheme`-selected body over `scipy.sparse.linalg`; `Eigen(map, k, mode)` runs `scipy.linalg.eigh`/`scipy.sparse.linalg.eigsh` for the `EIGENPAIRS` mode and `scipy.sparse.linalg.svds`/`np.linalg.svdvals` for the `SPECTRUM` mode, with a mode-respecting `np.linalg.eigh`/`np.linalg.svdvals` floor; `Operator(map, rhs, shape, policy)` runs the autodifferentiable `lineax` route, the `SolveShape` and the operand structure choosing the `lineax` solver and `LinearPolicy.batched` arming the multi-RHS sweep. `LinearIntent.solve` is the one method on the union — it enters `boundary(f"solve.{self.tag}", ...)` and the inner `match self` dispatches the four routes total through `assert_never`, identical in shape to `NonlinearIntent.solve`/`DifferentialIntent.solve`/`FieldQuery.evaluate`, never a free `solve(intent)` function beside a free `_dispatch`.
- Policy axis: `LinearPolicy` is the ONE frozen tuning value object over every route — `tol` (the Krylov `rtol`/`lsqr` `atol`/`lineax` `(rtol, atol)`, defaulting `_TOL`), `maxiter` (the iteration cap, the Krylov `maxiter=` and the `lineax` `LSMR`/`CG` `max_steps=`), `preconditioner` (the optional matrix-free `M=` Krylov preconditioner), and `batched` (the multi-RHS sweep flag). A Krylov scheme reads `policy.tol`/`policy.maxiter`/`policy.preconditioner`, and the operator route threads `policy.tol`/`policy.maxiter` into the iterative `lineax` cell plus `policy.batched` for the sweep, rather than four `(rtol, atol, maxiter)` literals re-spelled per scheme. A new tuning axis is one `LinearPolicy` field threaded into the scheme/operator cell, never a fifth positional argument or a second entry point.
- Structure axis: `MatrixStructure` is the ONE bounded structure policy — `GENERAL`, `SYMMETRIC`, `SPD`, `LOWER_TRIANGULAR`, `UPPER_TRIANGULAR`, `TRIDIAGONAL`, `DIAGONAL` — carried on every `LinearMap` and read by all three backends through one projection each, never re-discovered per route. The enum *value* IS the scipy `solve(assume_a=...)` driver string (`"gen"`/`"sym"`/`"pos"`/`"lower triangular"`/`"upper triangular"`/`"tridiagonal"`/`"diagonal"`), so the dense route passes `assume_a=m.structure.value` directly and a symmetric or SPD dense system reaches the LAPACK symmetric/Cholesky driver instead of the general LU floor; the one `_TAG_NAMES` table projects each structure to the documented `lineax` tag-attribute names (`symmetric_tag`, `positive_semidefinite_tag`, `lower_triangular_tag`, `upper_triangular_tag`, `tridiagonal_tag`, `diagonal_tag`), resolved once against the gated module into a `frozenset`, so the dense lineax operator is wrapped once in those tags and `AutoLinearSolver(well_posed=True)` reads them to pick `Cholesky` (PSD) → `Triangular` (triangular) → `Tridiagonal` (tridiagonal) → `Diagonal` (diagonal) → `LU` (general square) rather than a hard-coded `LU()` — `well_posed=True` is the load-bearing argument: the catalog `well_posed=None` is the rank-deficient least-squares SVD path and would discard the structure, so the square route never passes it. A new structure class is one `MatrixStructure` row plus its `_TAG_NAMES` entry, never a per-structure solve method.
- Operand owner: `LinearMap` is the ONE `@tagged_union` linear-operand value object carrying one `MatrixStructure` field — `Dense(array, structure)` for an `np.ndarray`, `SparseMat(matrix, structure)` for any admitted `scipy.sparse` container, and `Free(matvec, shape, rmatvec, structure)` for a matrix-free linear callable. The operand exposes four total `match self` projections so every route reads ONE projection rather than a raw `self.dense[0]`/`self.sparse_mat[0]` attribute that raises `AttributeError` on a mis-routed operand: `LinearMap.scipy_op` projects every case to the one operand the `scipy.sparse.linalg` bodies accept — the dense array and the sparse container pass through unchanged (the Krylov/`eigsh`/`svds` bodies accept a dense array, a sparse container, or a `LinearOperator` interchangeably), and the free case constructs `scipy.sparse.linalg.LinearOperator(shape, matvec, rmatvec)` so a matrix-free operand reaches the same bodies; `LinearMap.dense_array` is the dense LAPACK projection (dense passes through, a sparse container densifies once, a `Free` materialises its action against the identity columns) the `DenseLa` route reads in place of `m.dense[0]`; `LinearMap.matrix` is the actual sparse container the direct-factorization schemes (`spsolve`/`splu`/`spilu`/`factorized`) require (`SparseMat` returns its container, `Dense`/`Free` lift to CSR) since a `SuperLU` factor admits no `LinearOperator`; `LinearMap.residual(x, b)` reads `scipy_op @ x - b` so every route computes its residual through one projection. The `lineax`-operator projection is NOT on `LinearMap` — it lives on `LinearEngine.operator(m)` so the value object never imports the gated `jnp`/`lx`: that carrier method projects the diagonal dense case to `lineax.DiagonalLinearOperator(diag)` and the tridiagonal dense case to `lineax.TridiagonalLinearOperator(diagonal, lower_diagonal, upper_diagonal)` — the constructor taking the three diagonals (sized `n`, `n-1`, `n-1`) extracted via `jnp.diagonal(d, 0)`/`jnp.diagonal(d, -1)`/`jnp.diagonal(d, 1)`, never the `lineax.tridiagonal(operator)` EXTRACTOR which reads three diagonals OUT of a built operator — the remaining dense case to `lineax.MatrixLinearOperator(matrix, tags=_tags(structure, lx))`, the free case to `lineax.FunctionLinearOperator(matvec, jax.ShapeDtypeStruct((n,), float64), tags=_tags(structure, lx))` with the DOMAIN-sized input structure, and the sparse case to a matrix-free `lineax.FunctionLinearOperator(lambda v: a @ v, jax.ShapeDtypeStruct((a.shape[1],), float64), tags=_tags(structure, lx))` rather than an `a.toarray()` densification, so a FEM or graph-Laplacian operand stays sparse through the differentiable solve instead of materialising a dense `n*n`. A new operand backend is one `LinearMap` case plus its `scipy_op`/`dense_array`/`matrix` arms plus its `LinearEngine.operator` arm, never a per-route operand union.
- Scheme owner: `SparseScheme` is the ONE `@tagged_union` sparse-route discriminant carrying its own per-scheme payload — `Spsolve()` over `scipy.sparse.linalg.spsolve`, `Splu()` over `scipy.sparse.linalg.splu` returning the reusable `SuperLU` factor whose `.solve(b)` back-substitutes, `Spilu(drop_tol, fill_factor)` over `scipy.sparse.linalg.spilu` returning the *incomplete* `SuperLU` factor with the same `.solve(b)` contract, `Factored()` over `scipy.sparse.linalg.factorized` returning the reusable solve closure, `Krylov(kind)` over the full catalogued Krylov family (`cg`/`minres`/`gmres`/`bicgstab`/`qmr`/`tfqmr`/`lgmres`/`gcrotmk`) discriminated by `KrylovKind`, and `Lsqr(conlim)`/`Lsmr(conlim)` over `scipy.sparse.linalg.lsqr`/`lsmr`. The Krylov tolerance, iteration cap, and preconditioner ride the orthogonal `LinearPolicy` rather than the case payload, so the scheme discriminates the *method* and the policy carries the *tuning* — a re-tuned solve is one `LinearPolicy` value, not a re-spelled `Krylov(kind, rtol, maxiter, M)`. The direct schemes (`Spsolve`/`Splu`/`Spilu`/`Factored`) fold into `Direct` through the one `SuperLU.solve` back-substitution; the Krylov scheme reads `(x, info)`, sources its `M=` accelerator through `LinearMap.krylov_preconditioner` — an explicit `policy.preconditioner` wins; a factorable `SparseMat`/`Dense` operand falls to an `spilu` incomplete factor (the canonical FEM/graph-Laplacian ILU accelerator, never a parallel preconditioner knob); a matrix-free `Free` operand has no factorable matrix, so it runs unpreconditioned (`M=None`) rather than forcing `spilu` on a `LinearOperator` it cannot factor, the explicit preconditioner being the matrix-free accelerator path — counts true iterations through the scipy `callback` hook — `gmres` alone passing `callback_type="x"` so its callback fires once per OUTER iteration with the iterate rather than the `"pr_norm"` per-inner-step default, keeping the count comparable to the cg/bicgstab per-iteration callback — and folds `info` through `_info_status` into the `Iterative` typed status; `Lsqr`/`Lsmr` read `(x, istop, itn, r1norm, ...)` and fold `istop` through the shared `_ISTOP` into `LeastSquares`. A new Krylov method is one `KrylovKind` row; a new factorization scheme is one `SparseScheme` case; never a per-scheme method family and never a flag knob.
- Non-convergence law: the scipy Krylov bodies return `(x, info)` where `info == 0` is success, `info > 0` is max-iterations-reached, and `info < 0` is illegal-input/breakdown; the `_info_status` fold maps that integer to the `lineax`/`optimistix` `RESULTS` member-name vocabulary `SolverReceipt` already keys (`"successful"`/`"max_steps_reached"`/`"breakdown"`) so `SolverReceipt.Iterative(residual, steps, tol, result=_info_status(info))` carries `SolveStatus.MAX_STEPS` or `SolveStatus.BREAKDOWN` while the `callback`-counted `steps` feed the iteration slot — never `max(info, 0)` masquerading as an iteration count, never a silent residual-floor `STAGNATION`. The shared `lsqr`/`lsmr` `istop` code folds through `_ISTOP` (`1`/`2`/`4`/`5` → `"successful"`, `3` → `"conlim"` → `ILL_CONDITIONED`, `7` → `"max_steps_reached"`). The Lineax `Operator` case calls `lineax.linear_solve(operator, vector, solver, throw=False)` so a non-converged or singular solve returns its `Solution.result` rather than raising; the `RESULTS` member name (`LinearEngine.verdict` inverting `RESULTS._name_to_item` off the `Solution.result._value` code, since the `EnumerationItem` carries no `.name`) is folded through the receipt's `_STATUS` table, and the iteration count and residual read `solution.stats` only as evidence. No domain branch raises on non-convergence; the verdict is always a `SolveStatus` slot on the returned receipt.
- Lineax case: the `Operator` route lifts the `LinearMap` to a structure-tagged `lineax` operator through `LinearEngine.operator(m)` — the `input_structure` riding the operand's column count, so the RHS is never consulted to size the operator domain — and calls `lineax.linear_solve(operator, vector, solver, throw=False)` on the float64-floated carrier. The solver is the `LinearEngine.solver(shape, structure, *, spd_free, tol, maxiter)` cell: `SQUARE` defers to `lineax.AutoLinearSolver(well_posed=True)` so the operator tags choose `Cholesky` (SPD), `Triangular` (triangular), `Tridiagonal` (tridiagonal), `Diagonal` (diagonal), or `LU` (general square); `LEAST_SQUARES` runs `lineax.QR()` over an over-determined rectangular operator; `MIN_NORM` runs `lineax.LSMR(rtol, atol, max_steps)` for the ill-conditioned/under-determined minimum-norm solution; a matrix-free SPD operand (the structural `_spd_free` predicate, never a stringly `m.tag == "free"` compare) runs `lineax.Normal(lineax.CG(rtol, atol, max_steps))` — the documented normal-equations composite, NEVER the deprecated `lineax.NormalCG` slated for removal. `LinearPolicy.maxiter` threads into the iterative `LSMR`/`CG` cells as `max_steps=`; the direct `QR`/`AutoLinearSolver` cells carry no iteration cap. When `LinearPolicy.batched` is set, the right-hand side's leading axis is a stack of vectors and the whole solve maps through `equinox.filter_vmap(..., in_axes=(None, 0))` as ONE compiled differentiable solve over the one operator and the RHS stack, a second `filter_vmap` contracting the per-row residual through `LinearEngine.residual` (the operator's own `.mv`, never `LinearMap.scipy_op @ x` re-entering the scipy rail off a JAX solve) before `jnp.max` folds the worst column — never a Python `max(...)` over a `zip` of host arrays, the same vectorized per-row contraction `solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` run. The batched `Solution.result` reduces to its single worst-case termination member through `LinearEngine.verdict(result, batched=True)` — `jnp.max` over the per-row `_value` codes (the zero `successful`, the `equinox.Enumeration` zero-code convention every `RESULTS` subclass shares, making `max == 0` iff every column converged) plus the `RESULTS._name_to_item` name inversion, the identical reduction the sibling `solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` batched paths run because `lineax.RESULTS` is the same `equinox.Enumeration` base; `lineax.RESULTS.promote` is inheritance-widening (it lifts a member from a parent `Enumeration` to a subclass and raises on a same-class member), NOT a cross-vmap combine — exactly as `optimistix.RESULTS.promote`/`diffrax.RESULTS.promote` are NOT batch combines either — so the batched sweep carries its true aggregate verdict rather than a `result=None` residual-floor fiction, the single-point path inverting the one `int(Solution.result._value)` through `LinearEngine.verdict(result, batched=False)` (the `EnumerationItem` carries no `.name`) — one compiled solve over the whole sweep, never a Python loop over right-hand sides. That resolved member name folds into the receipt status and `solution.stats` (iteration count, residual norms) into the payload. The solve is autodifferentiable, so a downstream `vjp` through it reads the implicit-function-theorem adjoint rather than differentiating the iterations — the case folds into the same `solve` `match` rather than standing as a parallel solve entry beside it.
- Entry: `LinearIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the dense case computes the residual norm and condition number, the sparse case folds the per-scheme result and its typed status into `Direct`/`Iterative`/`LeastSquares`, the eigen case reports the spectral residual against the recovered eigenpairs (or the singular-value spectrum for the `SPECTRUM` mode), and the operator case folds the `lineax` `Solution.result`/`stats` into `Direct` or `LeastSquares`. `boundary` already converts an unexpected host fault into the runtime fault rail; the *expected* non-convergence is carried inside the success receipt as `SolveStatus`, so the two failure notions stay distinct. Emission rides the runtime `@receipted` aspect the measured `_dispatch` kernel wears, so the receipt stream emits on exit rather than an inline `Signals.emit` threaded through each route body — matching every sibling solver route.
- Construction: `LinearMap` and the sparse-matrix construction helpers compose `scipy.sparse` builders so a caller assembles the operand once — `scipy.sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack` build the banded, identity, tensor, and block operands the FEM and graph-Laplacian routes feed, and `LinearMap.SparseMat(matrix, structure)` accepts any resulting container with its known structure (a symmetric graph Laplacian carries `SYMMETRIC`, a mass-lumped diagonal carries `DIAGONAL`). The construction stays at the operand boundary; the dispatch bodies take only the projected `LinearMap.scipy_op`/`LinearEngine.operator` and the structure projections.
- Packages: `scipy` (`linalg.solve`, `linalg.lstsq`, `linalg.eigh`, `linalg.norm`, `sparse.diags_array`, `sparse.eye_array`, `sparse.kron`, `sparse.hstack`, `sparse.vstack`, `sparse.linalg.LinearOperator`, `sparse.linalg.spsolve`, `sparse.linalg.splu`, `sparse.linalg.spilu`, `sparse.linalg.factorized`, the full Krylov family `sparse.linalg.cg`/`minres`/`gmres`/`bicgstab`/`qmr`/`tfqmr`/`lgmres`/`gcrotmk`, `sparse.linalg.lsqr`, `sparse.linalg.lsmr`, `sparse.linalg.eigsh`, `sparse.linalg.svds`), `lineax` (`MatrixLinearOperator`, `FunctionLinearOperator`, `DiagonalLinearOperator`, `TridiagonalLinearOperator`, `AutoLinearSolver`, `linear_solve`, `QR`, `LSMR`, `CG`, `Normal`, `symmetric_tag`, `positive_semidefinite_tag`, `lower_triangular_tag`, `upper_triangular_tag`, `tridiagonal_tag`, `diagonal_tag`, `Solution`, `RESULTS` — `AutoLinearSolver(well_posed=True)` owns the `LU`/`Cholesky`/`Triangular`/`Tridiagonal`/`Diagonal` selection from tags, so those direct solvers are never named directly and the deprecated `NormalCG` is the deleted spelling), `equinox` (`filter_vmap` — the `LinearPolicy.batched` multi-RHS sweep over one compiled differentiable solve, the documented `filter_vmap(fun, *, in_axes=...)` surface), `jax` (`config.update("jax_enable_x64", True)` floating the gated `lineax` solve to float64 so the `1e-10` tolerance is reachable rather than silently clamped at float32 eps, `ShapeDtypeStruct((n,), float64)` declaring the domain-sized `FunctionLinearOperator` `input_structure` off the operand's column count, `numpy.diagonal`/`numpy.asarray` for the per-leaf operand lift and the diagonal/tridiagonal extraction, `numpy.linalg.norm` contracting the lineax-rail residual over the operator's `.mv` and `numpy.max` folding the batched per-row residual stack inside `filter_vmap`), `numpy` (`linalg.solve`, `linalg.lstsq`, `linalg.eigh`, `linalg.svdvals`, `linalg.norm`, `diagonal`), `solvers/receipt.md#RECEIPT` (`SolverReceipt` — `SolveStatus` is folded inside the receipt factories, never imported here), `msgspec` (`Struct` for the `LinearPolicy` tuning value object, matching every sibling policy), `dataclasses` (`dataclass(frozen=True, slots=True)` for the `LinearEngine` carrier holding live gated module handles), `expression.collections` (`Map` for the empty `Redaction.classified` policy), runtime (`RuntimeRail`, `boundary`, `Redaction` the empty-`classified` policy the `@receipted` aspect binds, `receipted` the aspect the measured `_dispatch` kernel wears).
- Growth: a new structure class is one `MatrixStructure` row plus its `_TAG_NAMES` projection entry (the `assume_a` driver is the enum value itself); a new Krylov method is one `KrylovKind` member whose value IS its `scipy.sparse.linalg` callable name (resolved through `getattr(spla, kind.value)`, no parallel identity table); a new sparse scheme is one `SparseScheme` case folding into an existing receipt factory; a new operand backend is one `LinearMap` case plus its `scipy_op`/`dense_array`/`matrix`/`krylov_preconditioner` arms plus its `LinearEngine.operator` arm; a new `lineax` solver cell is one `LinearEngine.solver` `match shape` arm reading the operand structure; a new tuning axis is one `LinearPolicy` field; a new termination code is one `_info_status` branch or `_ISTOP` row mapping into the existing `SolveStatus` vocabulary. Zero new surface, never a parallel dense/sparse owner, never a free `lineax_solve`, never a parallel matrix-free operand union, never a boolean `least_squares`/`spectral` knob where a policy row carries the modality, never a Python loop over a multi-RHS stack where `filter_vmap` vectorises one compiled solve.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, Self, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Redaction, receipted

# --- [TYPES] -------------------------------------------------------------------------------

type Matvec = Callable[[np.ndarray], np.ndarray]


class MatrixStructure(StrEnum):
    GENERAL = "gen"
    SYMMETRIC = "sym"
    SPD = "pos"
    LOWER_TRIANGULAR = "lower triangular"
    UPPER_TRIANGULAR = "upper triangular"
    TRIDIAGONAL = "tridiagonal"
    DIAGONAL = "diagonal"


class SolveShape(StrEnum):
    SQUARE = "square"
    LEAST_SQUARES = "least_squares"
    MIN_NORM = "min_norm"


class SpectralMode(StrEnum):
    EIGENPAIRS = "eigenpairs"
    SPECTRUM = "spectrum"


# The enum value IS the `scipy.sparse.linalg` callable name (mirroring `MatrixStructure.value` as the
# `assume_a` driver), so `getattr(spla, kind.value)` resolves the body with no parallel identity table.
# The full catalogued Krylov family — symmetric `cg`/`minres`, general-square `gmres`/`bicgstab`/`qmr`/
# `tfqmr`, and the augmented/recycling `lgmres`/`gcrotmk` for slow-converging systems — all share the
# `(A, b, *, rtol, atol, maxiter, M, callback)` signature, so a new method is one row, never a body.
class KrylovKind(StrEnum):
    CG = "cg"
    MINRES = "minres"
    GMRES = "gmres"
    BICGSTAB = "bicgstab"
    QMR = "qmr"
    TFQMR = "tfqmr"
    LGMRES = "lgmres"
    GCROTMK = "gcrotmk"


# --- [CONSTANTS] ---------------------------------------------------------------------------

_TOL: float = 1e-10

# Field-redaction policy the `@receipted` aspect binds; the solver facts are native residual/condition
# scalars carrying no secret, so the classification map is the empty persistent `Map`, never a mutable
# `{}` the FP rail rejects for domain state — the sibling `solvers/nonlinear.md#NONLINEAR` route's `_REDACTION`.
_REDACTION: Redaction = Redaction(classified=Map.empty())

# scipy lsqr/lsmr `istop`: 1/2/4/5 solved (lsmr adds 5, machine-precision exact), 3 conlim
# ill-conditioned, 7 max-iterations — one shared table over both least-squares solvers.
_ISTOP: FrozenDict[int, str] = FrozenDict(
    {1: "successful", 2: "successful", 3: "conlim", 4: "successful", 5: "successful", 7: "max_steps_reached"}
)

# Structure -> lineax tag-attribute names; `_tags` resolves them against the gated module into a
# 7 structures stay one data row each rather than 7 closures.
_TAG_NAMES: FrozenDict[MatrixStructure, tuple[str, ...]] = FrozenDict(
    {
        MatrixStructure.GENERAL: (),
        MatrixStructure.SYMMETRIC: ("symmetric_tag",),
        MatrixStructure.SPD: ("symmetric_tag", "positive_semidefinite_tag"),
        MatrixStructure.LOWER_TRIANGULAR: ("lower_triangular_tag",),
        MatrixStructure.UPPER_TRIANGULAR: ("upper_triangular_tag",),
        MatrixStructure.TRIDIAGONAL: ("tridiagonal_tag",),
        MatrixStructure.DIAGONAL: ("diagonal_tag",),
    }
)


# scipy Krylov `info`: 0 converged, >0 max-iterations, <0 illegal-input/breakdown.
def _info_status(info: int) -> str:
    return "successful" if info == 0 else "max_steps_reached" if info > 0 else "breakdown"


def _tags(structure: MatrixStructure, lx: object) -> frozenset:
    return frozenset(getattr(lx, name) for name in _TAG_NAMES[structure])


# 2-norm condition number from the singular spectrum (the dense direct/eigen conditioning evidence).
def _condition(s: np.ndarray) -> float:
    return float(s.max() / s.min()) if s.size and s.min() > 0 else float("inf")


# A matrix-free SPD operand has no factorable matrix, so its SQUARE solve routes `Normal(CG)`; a dense
# or sparse SPD operand keeps the tag-dispatched Cholesky. Structural over the union, never a stringly
# `m.tag == "free"` band string.
def _spd_free(m: "LinearMap") -> bool:
    match m:
        case LinearMap(tag="free", free=(_, _, _, MatrixStructure.SPD)):
            return True
        case _:
            return False


# --- [MODELS] ------------------------------------------------------------------------------

# One tuning value object over every route: the Krylov/lsqr/lineax tolerance, the Krylov iteration
# cap, the optional matrix-free preconditioner, and the multi-RHS sweep flag. The scheme discriminant
# carries the METHOD; this policy carries the TUNING, so a re-tuned solve is one value, not a
# re-spelled Krylov(kind, rtol, maxiter, M) case payload. `Struct` matches every sibling policy
# (`SolverPolicy`/`NonlinearPolicy`/`IntegratePolicy`/`QuadPolicy`) so the intent-carried tuning is one
# wire-encodable shape across the corpus; `LinearEngine` stays a `dataclass` because it holds live gated
# module handles, not domain state.
class LinearPolicy(Struct, frozen=True):
    tol: float = _TOL
    maxiter: int | None = None
    preconditioner: Matvec | None = None
    batched: bool = False


@tagged_union(frozen=True)
class LinearMap:
    tag: Literal["dense", "sparse_mat", "free"] = tag()
    dense: tuple[np.ndarray, MatrixStructure] = case()
    sparse_mat: tuple[object, MatrixStructure] = case()
    free: tuple[Matvec, tuple[int, int], Matvec | None, MatrixStructure] = case()

    @staticmethod
    def Dense(array: np.ndarray, structure: MatrixStructure = MatrixStructure.GENERAL) -> "LinearMap":
        return LinearMap(dense=(array, structure))

    @staticmethod
    def SparseMat(matrix: object, structure: MatrixStructure = MatrixStructure.GENERAL) -> "LinearMap":
        return LinearMap(sparse_mat=(matrix, structure))

    @staticmethod
    def Free(
        matvec: Matvec, shape: tuple[int, int], rmatvec: Matvec | None = None,
        structure: MatrixStructure = MatrixStructure.GENERAL,
    ) -> "LinearMap":
        return LinearMap(free=(matvec, shape, rmatvec, structure))

    @property
    def structure(self) -> MatrixStructure:
        return getattr(self, self.tag)[-1]

    # The one operand the `scipy.sparse.linalg` bodies accept: dense array / sparse container pass
    # through; the free case lifts matrix-free. No gated import — `lineax` lift lives on LinearEngine.
    def scipy_op(self) -> object:
        import scipy.sparse.linalg as spla

        match self:
            case LinearMap(tag="dense", dense=(a, _)) | LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return a
            case LinearMap(tag="free", free=(matvec, shape, rmatvec, _)):
                return spla.LinearOperator(shape, matvec=matvec, rmatvec=rmatvec)
            case _ as unreachable:
                assert_never(unreachable)

    # The dense LAPACK route reads one projection rather than a raw `self.dense[0]` that raises on a
    # mis-routed sparse/free operand: a dense array passes through, a sparse container densifies once,
    # and a matrix-free operator materialises its action against the identity columns (the only dense
    # form a matvec admits). Total over the union — no `AttributeError` control flow on a wrong tag.
    def dense_array(self) -> np.ndarray:
        match self:
            case LinearMap(tag="dense", dense=(a, _)):
                return np.asarray(a)
            case LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return np.asarray(a.toarray())
            case LinearMap(tag="free", free=(matvec, (rows, cols), _, _)):
                return np.column_stack([matvec(col) for col in np.eye(cols)])
            case _ as unreachable:
                assert_never(unreachable)

    # The actual sparse container the direct-factorization schemes (`spsolve`/`splu`/`spilu`/factorized)
    # require: a `SparseMat` returns its container, a `Dense` lifts to CSR, and a matrix-free `Free`
    # materialises its action to CSR (the only factorable form of a matvec) — total over the union, so
    # a mis-routed operand densifies once rather than raising a raw `AttributeError`.
    def matrix(self) -> object:
        import scipy.sparse as sp

        match self:
            case LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return a
            case _:
                return sp.csr_array(self.dense_array())

    # The Krylov `M=` accelerator selection folded once: an explicit matrix-free preconditioner wins; a
    # factorable operand (`SparseMat`/`Dense`) falls to its `spilu` incomplete-factor ILU; a matrix-free
    # `Free` operand has no factorable matrix, so it runs unpreconditioned (`M=None`) rather than forcing
    # `spilu` on a `LinearOperator` it cannot factor.
    def krylov_preconditioner(self, explicit: Matvec | None, spla: object) -> object | None:
        match self:
            case _ if explicit is not None:
                return spla.LinearOperator(self.scipy_op().shape, matvec=explicit)
            case LinearMap(tag="free"):
                return None
            case _:
                factor = spla.spilu(self.matrix())
                return spla.LinearOperator(factor.shape, matvec=factor.solve)

    def residual(self, x: np.ndarray, b: np.ndarray) -> float:
        return float(np.linalg.norm(self.scipy_op() @ x - b))


# The gated jnp/lx modules folded into one value object with behavior: `operator` and `solver` read
# `self.lx`/`self.jnp` off the carrier rather than each helper re-importing — the SolveEngine.gated()/
# DiffEngine.gated() discipline the sibling JAX routes run. `gated()` floats the rail to float64 and
# imports once behind the band, so the lineax flow (operator -> solve -> read) is one rail and
@dataclass(frozen=True, slots=True)
class LinearEngine:
    jax: object
    jnp: object
    lx: object

    @classmethod
    def gated(cls) -> Self:
        import jax
        import jax.numpy as jnp
        import lineax as lx

        jax.config.update("jax_enable_x64", True)  # 1e-10 (rtol, atol) is below float32 eps; JAX defaults to float32
        return cls(jax=jax, jnp=jnp, lx=lx)

    # The SINGLE LinearMap -> lineax-operator projection: diagonal/tridiagonal built directly, the rest
    # structure-tagged. `FunctionLinearOperator` takes `input_structure` describing the operator's DOMAIN
    # (the `x` vector, sized by the operand's column count), so the structure is a `jax.ShapeDtypeStruct`
    # over `shape[1]` — never the RHS vector, which is the codomain (sized `m`) and mis-sizes the domain
    # of a non-square least-squares/min-norm operand. The sparse case wraps matrix-free (never a.toarray())
    # so a FEM/graph-Laplacian operand stays sparse through the differentiable solve.
    def operator(self, m: LinearMap) -> object:
        jnp, lx, tags = self.jnp, self.lx, _tags(m.structure, self.lx)
        match m:
            case LinearMap(tag="dense", dense=(a, MatrixStructure.DIAGONAL)):
                return lx.DiagonalLinearOperator(jnp.asarray(np.diagonal(a)))
            case LinearMap(tag="dense", dense=(a, MatrixStructure.TRIDIAGONAL)):
                d = jnp.asarray(a)
                return lx.TridiagonalLinearOperator(jnp.diagonal(d, 0), jnp.diagonal(d, -1), jnp.diagonal(d, 1))
            case LinearMap(tag="dense", dense=(a, _)):
                return lx.MatrixLinearOperator(jnp.asarray(a), tags=tags)
            case LinearMap(tag="free", free=(matvec, (_, n), _, _)):
                domain = self.jax.ShapeDtypeStruct((n,), jnp.float64)
                return lx.FunctionLinearOperator(lambda v: jnp.asarray(matvec(np.asarray(v))), domain, tags=tags)
            case LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                domain = self.jax.ShapeDtypeStruct((a.shape[1],), jnp.float64)
                return lx.FunctionLinearOperator(lambda v: jnp.asarray(a @ np.asarray(v)), domain, tags=tags)
            case _ as unreachable:
                assert_never(unreachable)

    # The SINGLE SolveShape/structure -> solver cell. `well_posed=True` reads the operator tags to pick
    # Cholesky/Triangular/Tridiagonal/Diagonal/LU; the well_posed=None SVD path is the rank-deficient
    # route MIN_NORM/LSMR owns, so SQUARE never passes it. An SPD matrix-free operand routes Normal(CG).
    # The iterative cells thread `policy.maxiter` as `max_steps=`; the direct QR/AutoLinearSolver cells
    # carry no iteration cap.
    def solver(self, shape: SolveShape, structure: MatrixStructure, *, spd_free: bool, tol: float, maxiter: int | None) -> object:
        lx = self.lx
        match shape:
            case SolveShape.LEAST_SQUARES:
                return lx.QR()
            case SolveShape.MIN_NORM:
                return lx.LSMR(rtol=tol, atol=tol, max_steps=maxiter)
            case SolveShape.SQUARE if spd_free:
                return lx.Normal(lx.CG(rtol=tol, atol=tol, max_steps=maxiter))
            case SolveShape.SQUARE:
                return lx.AutoLinearSolver(well_posed=True)
            case _ as unreachable:
                assert_never(unreachable)

    # The lineax-rail residual `‖operator.mv(x) - b‖` over the operator's OWN `.mv`, returning the
    # traced `jnp` scalar — never `LinearMap.scipy_op @ x` which re-enters the scipy rail off a JAX
    # solve. Stays inside `filter_vmap`: the batched sweep contracts per-row through this scalar then
    # folds `jnp.max`, where a Python `max(...)` over a zip and a `float()` on a `Tracer` both raise.
    def residual(self, operator: object, x: object, b: object) -> object:
        return self.jnp.linalg.norm(operator.mv(x) - b)

    def verdict(self, result: object, *, batched: bool) -> str:
        # the `lineax.RESULTS` member NAME the receipt's `_STATUS` table keys on, recovered by inverting the
        # class `_name_to_item` code->item map — the lineax `RESULTS` is an `equinox.Enumeration` whose item
        # carries ONLY `_value`/`_enumeration` (no `.name`, `RESULTS[item]` yields the human MESSAGE), the same
        # `optimization/design.md#DESIGN` `_result_names` inversion the sibling solver carriers run. The batched
        # multi-RHS path reduces the worst-case verdict by `jnp.max` over the per-row `_value` codes — the zero
        # `successful` (the `equinox.Enumeration` zero-code convention every `RESULTS` subclass shares) makes
        # `max == 0` iff every column converged — exactly the `solvers/nonlinear.md#NONLINEAR`/`solvers/
        # differential.md#DIFFERENTIAL` reduction; `lineax.RESULTS.promote` is NOT a vmap combine (it widens a
        # member from a parent `Enumeration` to a subclass and raises on a same-class member), so the reduction
        # is the code max, never `promote(result)`, and the single path inverts the one `int(result._value)`.
        code = int(self.jnp.max(result._value)) if batched else int(result._value)
        names = {int(item._value): name for name, item in self.lx.RESULTS._name_to_item.items()}
        return names.get(code, "")


# The scheme discriminates the METHOD; tolerance/maxiter/preconditioner ride the orthogonal
# LinearPolicy. `Krylov` carries only its KrylovKind; `Lsqr` only its conlim (the one knob with no
# LinearPolicy peer). A re-tuned solve is one LinearPolicy value, never a re-spelled case payload.
@tagged_union(frozen=True)
class SparseScheme:
    tag: Literal["spsolve", "splu", "spilu", "factored", "krylov", "lsqr", "lsmr"] = tag()
    spsolve: tuple[()] = case()
    splu: tuple[()] = case()
    spilu: tuple[float, float] = case()
    factored: tuple[()] = case()
    krylov: tuple[KrylovKind] = case()
    lsqr: tuple[float] = case()
    lsmr: tuple[float] = case()

    @staticmethod
    def Spsolve() -> "SparseScheme":
        return SparseScheme(spsolve=())

    @staticmethod
    def Splu() -> "SparseScheme":
        return SparseScheme(splu=())

    @staticmethod
    def Spilu(drop_tol: float = 1e-4, fill_factor: float = 10.0) -> "SparseScheme":
        return SparseScheme(spilu=(drop_tol, fill_factor))

    @staticmethod
    def Factored() -> "SparseScheme":
        return SparseScheme(factored=())

    @staticmethod
    def Krylov(kind: KrylovKind = KrylovKind.CG) -> "SparseScheme":
        return SparseScheme(krylov=(kind,))

    @staticmethod
    def Lsqr(conlim: float = 1e8) -> "SparseScheme":
        return SparseScheme(lsqr=(conlim,))

    @staticmethod
    def Lsmr(conlim: float = 1e8) -> "SparseScheme":
        return SparseScheme(lsmr=(conlim,))


@tagged_union(frozen=True)
class LinearIntent:
    tag: Literal["dense_la", "sparse", "eigen", "operator"] = tag()
    dense_la: tuple[LinearMap, np.ndarray, SolveShape] = case()
    sparse: tuple[LinearMap, np.ndarray, SparseScheme, LinearPolicy] = case()
    eigen: tuple[LinearMap, int, SpectralMode] = case()
    operator: tuple[LinearMap, np.ndarray, SolveShape, LinearPolicy] = case()

    @staticmethod
    def DenseLa(matrix: LinearMap, rhs: np.ndarray, shape: SolveShape = SolveShape.SQUARE) -> "LinearIntent":
        return LinearIntent(dense_la=(matrix, rhs, shape))

    @staticmethod
    def Sparse(
        matrix: LinearMap, rhs: np.ndarray, scheme: SparseScheme = SparseScheme.Spsolve(), policy: LinearPolicy = LinearPolicy()
    ) -> "LinearIntent":
        return LinearIntent(sparse=(matrix, rhs, scheme, policy))

    @staticmethod
    def Eigen(matrix: LinearMap, k: int, mode: SpectralMode = SpectralMode.EIGENPAIRS) -> "LinearIntent":
        return LinearIntent(eigen=(matrix, k, mode))

    @staticmethod
    def Operator(
        matrix: LinearMap, rhs: np.ndarray, shape: SolveShape = SolveShape.SQUARE, policy: LinearPolicy = LinearPolicy()
    ) -> "LinearIntent":
        return LinearIntent(operator=(matrix, rhs, shape, policy))

    def solve(self) -> "RuntimeRail[SolverReceipt]":
        return boundary(f"solve.{self.tag}", lambda: _dispatch(self))


# --- [OPERATIONS] --------------------------------------------------------------------------


# `@receipted(_REDACTION)` wraps the one measured kernel that returns the `SolverReceipt`: the aspect
# harvests its `SolverReceipt.contribute` stream and emits on exit, so receipt production stays a decorator
# rail rather than an inline `Signals.emit` threaded through each route — matching every sibling solver
# route. `solve` fences this kernel in one `boundary`, so a host fault rails and the receipt emits on the
# same exit.
@receipted(_REDACTION)
def _dispatch(intent: LinearIntent) -> SolverReceipt:
    match intent:
        case LinearIntent(tag="dense_la", dense_la=(m, b, shape)):
            return _dense_receipt(m, b, shape)
        case LinearIntent(tag="sparse", sparse=(m, b, scheme, policy)):
            return _sparse_receipt(m, b, scheme, policy)
        case LinearIntent(tag="eigen", eigen=(m, k, mode)):
            return _eigen_receipt(m, k, mode)
        case LinearIntent(tag="operator", operator=(m, b, shape, policy)):
            return _operator_receipt(m, b, shape, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _dense_receipt(m: LinearMap, b: np.ndarray, shape: SolveShape) -> SolverReceipt:
    a = m.dense_array()
    if shape is not SolveShape.SQUARE or a.shape[0] != a.shape[1]:
        x, residuals, rank, _ = np.linalg.lstsq(a, b, rcond=None)
        residual = float(residuals[0]) if residuals.size else float(np.linalg.norm(a @ x - b))
        return SolverReceipt.LeastSquares(residual, int(rank), 0)
    try:
        import scipy.linalg as sla

        # `assume_a=m.structure.value` IS the LAPACK driver selector: `"pos"` routes the SPD system
        # onto the Cholesky `?posv` driver, `"sym"` onto `?sysv`, the triangular/tridiagonal/diagonal
        # values onto their banded drivers — one structure projection, no SPD-special-case `cho_*` pair.
        x = sla.solve(a, b, assume_a=m.structure.value)
    except ImportError:
        x = np.linalg.solve(a, b)
    return SolverReceipt.Direct(float(np.linalg.norm(a @ x - b)), _condition(np.linalg.svdvals(a)))


def _sparse_receipt(m: LinearMap, b: np.ndarray, scheme: SparseScheme, policy: LinearPolicy) -> SolverReceipt:
    import scipy.sparse.linalg as spla

    # The direct-factorization schemes back-substitute through a `SuperLU`/solve-closure that only an
    # actual sparse container admits, so they read `m.matrix()` (the SparseMat-or-Dense projection that
    # raises a typed mis-route, never an `AttributeError`); the Krylov/lsqr schemes read the matrix-free
    # `m.scipy_op()` so a `Free` FEM operand reaches them without ever materialising a matrix.
    match scheme:
        case SparseScheme(tag="spsolve"):
            return SolverReceipt.Direct(m.residual(spla.spsolve(m.matrix(), b), b), float("nan"))
        # `splu` is the exact factor; `spilu(drop_tol, fill_factor)` is the incomplete factor — both
        # return a `SuperLU` whose `.solve(b)` back-substitutes, so the direct fold is one arm.
        case SparseScheme(tag="splu"):
            return SolverReceipt.Direct(m.residual(spla.splu(m.matrix()).solve(b), b), float("nan"))
        case SparseScheme(tag="spilu", spilu=(drop_tol, fill_factor)):
            return SolverReceipt.Direct(m.residual(spla.spilu(m.matrix(), drop_tol=drop_tol, fill_factor=fill_factor).solve(b), b), float("nan"))
        case SparseScheme(tag="factored"):
            return SolverReceipt.Direct(m.residual(spla.factorized(m.matrix())(b), b), float("nan"))
        case SparseScheme(tag="krylov", krylov=(kind,)):
            op = m.scipy_op()
            # The Krylov `M=` accelerator: an explicit matrix-free `policy.preconditioner` wins; absent
            # one, the `spilu` incomplete factor of the operand becomes the matrix-free `M` ILU the FEM/
            # graph-Laplacian assembled route wants — but a matrix-free `Free` operand has no factorable
            # matrix, so it falls through to `M=None` (unpreconditioned) rather than forcing `spilu` on a
            # `LinearOperator` it cannot factor; the explicit `policy.preconditioner` is the matrix-free
            # accelerator path for that case.
            pre = m.krylov_preconditioner(policy.preconditioner, spla)
            steps: list[int] = []
            # `gmres` alone takes `callback_type`; `"x"` fires the callback once per OUTER iteration
            # with the iterate, so `len(steps)` is the true iteration count comparable to the
            # cg/bicgstab per-iteration callback rather than the `"pr_norm"` per-inner-step default.
            extra = {"callback_type": "x"} if kind is KrylovKind.GMRES else {}
            x, info = getattr(spla, kind.value)(
                op, b, rtol=policy.tol, maxiter=policy.maxiter, M=pre, callback=lambda *_: steps.append(1), **extra
            )
            return SolverReceipt.Iterative(m.residual(x, b), len(steps), policy.tol, result=_info_status(int(info)))
        # `lsqr` and `lsmr` are the two catalogued sparse least-squares solvers — both return
        # `(x, istop, itn, normr, ...)` with the same `istop` vocabulary, so one or-pattern folds both.
        case SparseScheme(tag="lsqr", lsqr=(conlim,)) | SparseScheme(tag="lsmr", lsmr=(conlim,)):
            x, istop, itn, r1norm, *_ = getattr(spla, scheme.tag)(m.scipy_op(), b, atol=policy.tol, btol=policy.tol, conlim=conlim)
            return SolverReceipt.LeastSquares(float(r1norm), 0, int(itn), result=_ISTOP.get(int(istop), "other"))
        case _ as unreachable:
            assert_never(unreachable)


# `mode` is honoured on both bands: SPECTRUM reads the singular spectrum (dense `svdvals` / sparse
# `svds`), EIGENPAIRS the symmetric eigenpairs (`eigh`/`eigsh`), so a dense SPECTRUM never silently
# returns eigenpairs. One total `match (m, mode)` over the structural `LinearMap.Dense` tag closed by
# `assert_never`, identical in shape to `_dispatch`/`LinearEngine.operator` — the dense column matches
# the structural `LinearMap(tag="dense", ...)` directly, never a stringly `m.tag == "dense"` band string.
def _eigen_receipt(m: LinearMap, k: int, mode: SpectralMode) -> SolverReceipt:
    match (m, mode):
        case (LinearMap(tag="dense", dense=(a, _)), SpectralMode.SPECTRUM):
            s = np.linalg.svdvals(np.asarray(a))
            return SolverReceipt.Eigen(0.0, int(s.size), _condition(s))
        case (LinearMap(tag="dense", dense=(a, _)), SpectralMode.EIGENPAIRS):
            dense = np.asarray(a)
            w, v = np.linalg.eigh(dense)
            return SolverReceipt.Eigen(float(np.linalg.norm(dense @ v - v * w)), int(w.size), _condition(np.linalg.svdvals(dense)))
        case (_, SpectralMode.SPECTRUM):
            import scipy.sparse.linalg as spla

            op = m.scipy_op()
            u, s, vt = spla.svds(op, k=k)
            return SolverReceipt.Eigen(float(np.linalg.norm(op @ vt.conj().T - u * s)), int(s.size), _condition(np.asarray(s)))
        case (_, SpectralMode.EIGENPAIRS):
            import scipy.sparse.linalg as spla

            op = m.scipy_op()
            w, v = spla.eigsh(op, k=k)
            return SolverReceipt.Eigen(float(np.linalg.norm(op @ v - v * w)), int(k), float("nan"))
        case _ as unreachable:
            assert_never(unreachable)


# One float64-floated lineax rail: `LinearEngine.gated()` imports jnp/lx once and promotes to float64
# (the 1e-10 tolerance is below float32 eps), `operator` builds the structure-tagged operator, `solver`
# picks the SolveShape/structure cell, and `linear_solve(..., throw=False)` returns a typed verdict.
# Batched: vmap one operator over a RHS stack through `equinox.filter_vmap(in_axes=(None, 0))` as one
# compiled differentiable solve, a second `filter_vmap` contracting the per-row `e.residual` (the
# operator's own `.mv`) before `jnp.max` folds the worst column — never a Python `max(...)` over a zip
# or a `float()` on a `Tracer`. `e.verdict` reduces the batched `Solution.result` to its worst-case
# member by `jnp.max` over the per-row `_value` codes plus the `RESULTS._name_to_item` name inversion —
# the same reduction `solvers/nonlinear.md#NONLINEAR`/`solvers/differential.md#DIFFERENTIAL` run, since
# `lineax.RESULTS` is the same `equinox.Enumeration` base whose `promote` is inheritance-widening (raising
# on a same-class member), NOT a vmap combine — so the batched solve carries its true aggregate verdict
# rather than a `result=None` residual-floor fiction; the single-point path inverts the one
# `int(Solution.result._value)` (the `EnumerationItem` carries no `.name`).
def _operator_receipt(m: LinearMap, b: np.ndarray, shape: SolveShape, policy: LinearPolicy) -> SolverReceipt:
    import equinox as eqx

    e = LinearEngine.gated()
    operator = e.operator(m)  # input_structure rides the operand's column count, so no RHS row is needed to build it
    solver = e.solver(shape, m.structure, spd_free=_spd_free(m), tol=policy.tol, maxiter=policy.maxiter)
    run = lambda op, v: e.lx.linear_solve(op, v, solver, throw=False)
    # lineax direct solvers (LU/Cholesky/QR/Diagonal/Tridiagonal) return `stats == {}`, so `num_steps`
    # reads through `.get(..., 0)` — unlike the always-iterative optimistix/diffrax siblings keying it
    # directly — and `0` is the truthful iteration count for a one-shot factorization.
    if policy.batched:
        stack = e.jnp.asarray(b)
        solution = eqx.filter_vmap(run, in_axes=(None, 0))(operator, stack)
        per_row = eqx.filter_vmap(lambda v, rhs: e.residual(operator, v, rhs), in_axes=(0, 0))(solution.value, stack)
        status, iterations = e.verdict(solution.result, batched=True), int(np.asarray(solution.stats.get("num_steps", 0)).max())
        residual = float(e.jnp.max(per_row))
    else:
        rhs = e.jnp.asarray(b)
        solution = run(operator, rhs)
        status, iterations = e.verdict(solution.result, batched=False), int(solution.stats.get("num_steps", 0))
        residual = float(e.residual(operator, solution.value, rhs))
    # `Solution` exposes no rank; the rank slot stays 0 (unknown), matching the `lsqr` arm — never
    # `x.size`, the solution dimension, not the operator rank the slot names.
    return (
        SolverReceipt.LeastSquares(residual, 0, iterations, result=status)
        if shape is not SolveShape.SQUARE
        else SolverReceipt.Direct(residual, float("nan"), result=status)
    )
```

## [03]-[RESEARCH]

- [NONCONVERGENCE_RAIL]: the FIX law is that an iterative or operator solve never silently returns a wrong answer and never raises on expected non-convergence. The scipy Krylov `info` code and `lsqr` `istop` code fold through `_info_status`/`_ISTOP` into the `RESULTS` member-name vocabulary the receipt keys, so `SolveStatus.MAX_STEPS`/`BREAKDOWN`/`ILL_CONDITIONED` ride out on the receipt; the lineax route uses `throw=False` and folds the member name through `LinearEngine.verdict` (the `RESULTS._name_to_item` inversion, since the `EnumerationItem` carries no `.name`). The unexpected host fault (a shape mismatch, a non-finite operand the backend rejects) still rises on the `boundary` runtime rail, keeping the expected-non-convergence verdict and the unexpected-fault rail distinct rather than collapsing both into one.
