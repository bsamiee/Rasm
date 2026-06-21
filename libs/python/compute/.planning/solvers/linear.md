# [PY_COMPUTE_LINEAR]

The linear-algebra routes of the one numeric solver. `LinearIntent` discriminates dense linear systems, sparse systems by scheme (direct/factored/Krylov/least-squares), eigen-and-spectral problems, and the autodifferentiable Lineax operator tier that unifies dense, sparse, and iterative solves and least-squares over one general linear operator. A `LinearMap` value object carries a dense matrix, an admitted scipy sparse container, or a matrix-free matvec, AND one `MatrixStructure` policy value (general/symmetric/spd/lower- or upper-triangular/tridiagonal/diagonal) that is the SINGLE structure axis every route reads — it picks the scipy dense LAPACK driver through `scipy.linalg.solve(a, b, assume_a=...)` and the Cholesky/LU factor cell, it picks the lineax `tags` frozenset and lets `lineax.AutoLinearSolver(well_posed=None)` select `LU`/`Cholesky`/`Triangular`/`Diagonal`/`SVD` from those tags, and it lets `LinearMap.lineax_op` lift a diagonal or tridiagonal operand to `lineax.DiagonalLinearOperator`/`lineax.TridiagonalLinearOperator`. One operand shape plus one structure value feeds every route, and the matrix-free path lifts to a `scipy.sparse.linalg.LinearOperator` and a `lineax.FunctionLinearOperator` without a parallel matrix-free owner. Two more policy values complete the bounded vocabulary in place of boolean knobs: `SolveShape` (square/least-squares/min-norm) replaces a `least_squares` flag and selects the dense `solve`-vs-`lstsq` arm, the lineax `LU`/`QR`/`LSMR`/`NormalCG` solver cell, and the sparse `spsolve`-vs-`lsqr` arm; `SpectralMode` (eigenpairs/spectrum) replaces a `spectral` flag and selects `eigh`/`eigsh` against `svds`. Every route folds into the one `SolverReceipt`, and crucially every iterative and operator route folds the backend's *termination reason* — the scipy `info` code through one `_info_status` fold and the `lineax.Solution.result` member name — into `SolveStatus`, so a CG/GMRES non-convergence or a singular factorization is a first-class typed verdict on the receipt, never a silent residual-floor pass and never a raised exception. The dense numpy floor runs unconditionally on cp315; the scipy sparse/eigen bodies and the Lineax tier are the gated companion band.

## [01]-[INDEX]

- [01]-[LINEAR]: dense/sparse/eigen routes over scipy plus the Lineax autodifferentiable operator case, all folded into one `LinearIntent` owner reading one `LinearMap` operand and three bounded policy values (`MatrixStructure`/`SolveShape`/`SpectralMode`) and emitting typed non-convergence.

## [02]-[LINEAR]

- Owner: `LinearIntent` — the four linear-route cases on the one solver, each reading one `LinearMap` operand. `DenseLa(map, rhs, shape)` runs `scipy.linalg.solve(assume_a)`/`cho_solve`/`lu_solve`/`lstsq` keyed by the operand's `MatrixStructure` and the `SolveShape` policy, with a `np.linalg.solve`/`lstsq` floor; `Sparse(map, rhs, scheme)` runs the `SparseScheme`-selected body over `scipy.sparse.linalg`; `Eigen(map, k, mode)` runs `scipy.linalg.eigh`/`scipy.sparse.linalg.eigsh` for the `EIGENPAIRS` mode and `scipy.sparse.linalg.svds` for the `SPECTRUM` mode with a `np.linalg.eigh` floor; `Operator(map, rhs, shape)` runs the autodifferentiable Lineax route, the `SolveShape` and the operand structure choosing the lineax solver. `solve` returns `RuntimeRail[SolverReceipt]` and `_dispatch` matches the four routes total through `assert_never`.
- Structure axis: `MatrixStructure` is the ONE bounded structure policy — `GENERAL`, `SYMMETRIC`, `SPD`, `LOWER_TRIANGULAR`, `UPPER_TRIANGULAR`, `TRIDIAGONAL`, `DIAGONAL` — carried on every `LinearMap` and read by all three backends through one projection each, never re-discovered per route. The enum *value* IS the scipy `solve(assume_a=...)` driver string (`"gen"`/`"sym"`/`"pos"`/`"lower triangular"`/`"upper triangular"`/`"tridiagonal"`/`"diagonal"`), so the dense route passes `assume_a=m.structure.value` directly and a symmetric or SPD dense system reaches the LAPACK symmetric/Cholesky driver instead of the general LU floor; the one `_LINEAX_TAGS` table projects each structure to the documented `lineax` tag sentinels (`symmetric_tag`, `positive_semidefinite_tag`, `lower_triangular_tag`, `upper_triangular_tag`, `tridiagonal_tag`, `diagonal_tag`) so the dense lineax operator is wrapped once in those tags and `AutoLinearSolver(well_posed=None)` selects `Cholesky`/`Triangular`/`Diagonal`/`LU` from the structure rather than a hard-coded `LU()`. A new structure class is one `MatrixStructure` row plus its `_LINEAX_TAGS` entry, never a per-structure solve method.
- Operand owner: `LinearMap` is the ONE @tagged_union linear-operand value object carrying one `MatrixStructure` field — `Dense(array, structure)` for an `np.ndarray`, `SparseMat(matrix, structure)` for any admitted `scipy.sparse` container, and `Free(matvec, shape, rmatvec, structure)` for a matrix-free linear callable. `LinearMap.scipy_op` projects every case to the one operand the `scipy.sparse.linalg` bodies accept — the dense array and the sparse container pass through unchanged (the Krylov/`eigsh`/`svds` bodies accept a dense array, a sparse container, or a `LinearOperator` interchangeably), and the free case constructs `scipy.sparse.linalg.LinearOperator(shape, matvec, rmatvec)` so a matrix-free operand reaches the same bodies; `LinearMap.lineax_op` projects the diagonal/tridiagonal dense case to `lineax.DiagonalLinearOperator`/`lineax.TridiagonalLinearOperator`, the remaining dense case to `lineax.MatrixLinearOperator(matrix, tags=structure.lineax_tags)`, and the free case to `lineax.FunctionLinearOperator(matvec, input_structure, tags=structure.lineax_tags)`; `LinearMap.residual(x, b)` reads `scipy_op @ x - b` so every route computes its residual through one projection rather than re-casing the operand. A new operand backend is one `LinearMap` case plus its projection arms, never a per-route operand union.
- Scheme owner: `SparseScheme` is the ONE @tagged_union sparse-route discriminant carrying its own per-scheme payload — `Spsolve()` over `scipy.sparse.linalg.spsolve`, `Splu()` over `scipy.sparse.linalg.splu` returning the reusable `SuperLU` factor whose `.solve(b)` back-substitutes, `Factored()` over `scipy.sparse.linalg.factorized` returning the reusable solve closure, `Krylov(kind, rtol, maxiter, preconditioner)` over `scipy.sparse.linalg.cg`/`gmres`/`bicgstab` discriminated by `KrylovKind`, and `Lsqr(atol, btol, conlim)` over `scipy.sparse.linalg.lsqr`. The direct schemes (`Spsolve`/`Splu`/`Factored`) fold into `Direct`; the Krylov scheme reads `(x, info)`, counts true iterations through the scipy `callback` hook, and folds `info` through `_info_status` into the `Iterative` typed status; `Lsqr` reads `(x, istop, itn, r1norm, ...)` and folds `istop` through `_ISTOP` into `LeastSquares`. A new Krylov method is one `KrylovKind` row; a new factorization scheme is one `SparseScheme` case; never a per-scheme method family and never a flag knob.
- Non-convergence law: the scipy Krylov bodies return `(x, info)` where `info == 0` is success, `info > 0` is max-iterations-reached, and `info < 0` is illegal-input/breakdown; the `_info_status` fold maps that integer to the `lineax`/`optimistix` `RESULTS` member-name vocabulary `SolverReceipt` already keys (`"successful"`/`"max_steps_reached"`/`"breakdown"`) so `SolverReceipt.Iterative(residual, steps, tol, result=_info_status(info))` carries `SolveStatus.MAX_STEPS` or `SolveStatus.BREAKDOWN` while the `callback`-counted `steps` feed the iteration slot — never `max(info, 0)` masquerading as an iteration count, never a silent residual-floor `STAGNATION`. The `lsqr` `istop` code folds through `_ISTOP` (`1`/`2`/`4` → `"successful"`, `3` → `"conlim"` → `ILL_CONDITIONED`, `7` → `"max_steps_reached"`). The Lineax `Operator` case calls `lineax.linear_solve(operator, vector, solver, throw=False)` so a non-converged or singular solve returns its `Solution.result` rather than raising; `solution.result.name` is folded through the receipt's `_STATUS` table, and the iteration count and residual read `solution.stats` only as evidence. No domain branch raises on non-convergence; the verdict is always a `SolveStatus` slot on the returned receipt.
- Lineax case: the `Operator` route lifts the `LinearMap` to a structure-tagged `lineax` operator and calls `lineax.linear_solve(operator, vector, solver, throw=False)`; the solver is the `SolveShape`/structure cell — `SQUARE` defers to `lineax.AutoLinearSolver(well_posed=None)` so the operator tags choose `Cholesky` (SPD), `Triangular` (triangular), `Diagonal` (diagonal), or `LU` (general); `LEAST_SQUARES` runs `lineax.QR()` over a rectangular operator; `MIN_NORM` runs `lineax.LSMR()` for the ill-conditioned/under-determined minimum-norm solution; a symmetric-positive-definite matrix-free operand runs `lineax.NormalCG()`. `solution.result.name` folds into the receipt status and `solution.stats` (iteration count, residual) into the payload. The Lineax solve is autodifferentiable, so a downstream `vjp` through the solver reads the implicit-function-theorem adjoint rather than differentiating the iterations — the case folds into the same `_dispatch` rather than standing as a parallel solve entry beside it.
- Entry: `LinearIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the dense case computes the residual norm and condition number, the sparse case folds the per-scheme result and its typed status into `Direct`/`Iterative`/`LeastSquares`, the eigen case reports the spectral residual against the recovered eigenpairs (or the singular-value spectrum for the `SPECTRUM` mode), and the operator case folds the Lineax `Solution.result`/`stats` into `Direct` or `LeastSquares`. `boundary` already converts an unexpected host fault into the runtime fault rail; the *expected* non-convergence is carried inside the success receipt as `SolveStatus`, so the two failure notions stay distinct.
- Construction: `LinearMap` and the sparse-matrix construction helpers compose `scipy.sparse` builders so a caller assembles the operand once — `scipy.sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack` build the banded, identity, tensor, and block operands the FEM and graph-Laplacian routes feed, and `LinearMap.SparseMat(matrix, structure)` accepts any resulting container with its known structure (a symmetric graph Laplacian carries `SYMMETRIC`, a mass-lumped diagonal carries `DIAGONAL`). The construction stays at the operand boundary; the dispatch bodies take only the projected `scipy_op`/`lineax_op` and the structure projections.
- Packages: `scipy` (`linalg.solve`, `linalg.lstsq`, `linalg.eigh`, `linalg.cho_factor`, `linalg.cho_solve`, `linalg.lu_factor`, `linalg.lu_solve`, `linalg.norm`, `sparse.diags_array`, `sparse.eye_array`, `sparse.kron`, `sparse.hstack`, `sparse.vstack`, `sparse.linalg.LinearOperator`, `sparse.linalg.spsolve`, `sparse.linalg.splu`, `sparse.linalg.factorized`, `sparse.linalg.cg`, `sparse.linalg.gmres`, `sparse.linalg.bicgstab`, `sparse.linalg.lsqr`, `sparse.linalg.eigsh`, `sparse.linalg.svds`), `lineax` (`MatrixLinearOperator`, `FunctionLinearOperator`, `DiagonalLinearOperator`, `TridiagonalLinearOperator`, `tridiagonal`, `AutoLinearSolver`, `linear_solve`, `LU`, `QR`, `Cholesky`, `Triangular`, `Diagonal`, `SVD`, `LSMR`, `NormalCG`, `symmetric_tag`, `positive_semidefinite_tag`, `lower_triangular_tag`, `upper_triangular_tag`, `tridiagonal_tag`, `diagonal_tag`, `Solution`, `RESULTS`), `numpy` (`linalg.solve`, `linalg.lstsq`, `linalg.eigh`, `linalg.svdvals`, `linalg.norm`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`, `SolveStatus`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new structure class is one `MatrixStructure` row plus its `assume_a`/`lineax_tags` projection entries; a new Krylov method is one `KrylovKind` row; a new sparse scheme is one `SparseScheme` case folding into an existing receipt factory; a new operand backend is one `LinearMap` case plus its projection arms; a new Lineax solver cell is one `_operator_receipt` `match shape` arm reading the operand structure; a new termination code is one `_info_status` branch or `_ISTOP` row mapping into the existing `SolveStatus` vocabulary. Zero new surface, never a parallel dense/sparse owner, never a free `lineax_solve`, never a parallel matrix-free operand union, never a boolean `least_squares`/`spectral` knob where a policy row carries the modality.
- Boundary: the dense and dense-symmetric numpy floors run unconditionally on cp315; `scipy` and `lineax`/`jaxlib` carry no cp315 wheel, so the scipy sparse/eigen bodies and the Lineax operator case are authored against the documented API with a reachable numpy-floor branch. A per-scheme method family, a separate eigenproblem owner, a free Lineax solve function, four parallel scipy entry points where one Lineax operator folds dense/sparse/iterative solves, a parallel matrix-free operand owner, a hard-coded `lineax.LU()` discarding the operand structure, a `least_squares`/`spectral` boolean knob, a `max(info, 0)` iteration-count fiction, a silent residual-floor verdict for a backend that already adjudicated termination, and a `throw=True` Lineax solve raising on non-convergence are the deleted forms.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union

from rasm.compute.solvers.receipt import SolverReceipt, SolveStatus
from rasm.runtime.faults import RuntimeRail, boundary


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


class KrylovKind(StrEnum):
    CG = "cg"
    GMRES = "gmres"
    BICGSTAB = "bicgstab"


# --- [CONSTANTS] ---------------------------------------------------------------------------

_TOL: float = 1e-10

# scipy lsqr `istop`: 1/2/4 solved, 3 conlim ill-conditioned, 7 max-iterations.
_ISTOP: FrozenDict[int, str] = FrozenDict(
    {1: "successful", 2: "successful", 3: "conlim", 4: "successful", 7: "max_steps_reached"}
)


# scipy Krylov `info`: 0 converged, >0 max-iterations, <0 illegal-input/breakdown.
def _info_status(info: int) -> str:
    return "successful" if info == 0 else "max_steps_reached" if info > 0 else "breakdown"


# 2-norm condition number from the singular spectrum (the dense direct/eigen conditioning evidence).
def _condition(a: np.ndarray) -> float:
    s = np.linalg.svdvals(np.asarray(a))
    return float(s.max() / s.min()) if s.size and s.min() > 0 else float("inf")


# --- [MODELS] ------------------------------------------------------------------------------

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

    def scipy_op(self) -> object:
        import scipy.sparse.linalg as spla

        match self:
            case LinearMap(tag="dense", dense=(a, _)):
                return a
            case LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return a
            case LinearMap(tag="free", free=(matvec, shape, rmatvec, _)):
                return spla.LinearOperator(shape, matvec=matvec, rmatvec=rmatvec)
            case unreachable:
                assert_never(unreachable)

    def lineax_op(self, vector: np.ndarray) -> object:
        import jax.numpy as jnp
        import lineax as lx

        tags = _LINEAX_TAGS[self.structure](lx)
        match self:
            case LinearMap(tag="dense", dense=(a, MatrixStructure.DIAGONAL)):
                return lx.DiagonalLinearOperator(jnp.asarray(np.diagonal(a)))
            case LinearMap(tag="dense", dense=(a, MatrixStructure.TRIDIAGONAL)):
                d = jnp.asarray(a)
                return lx.tridiagonal(jnp.diagonal(d, -1), jnp.diagonal(d), jnp.diagonal(d, 1))
            case LinearMap(tag="dense", dense=(a, _)):
                return lx.MatrixLinearOperator(jnp.asarray(a), tags=tags)
            case LinearMap(tag="free", free=(matvec, _, _, _)):
                return lx.FunctionLinearOperator(
                    lambda v: jnp.asarray(matvec(np.asarray(v))), jnp.asarray(vector), tags=tags
                )
            case LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return lx.MatrixLinearOperator(jnp.asarray(a.toarray()), tags=tags)
            case unreachable:
                assert_never(unreachable)

    def residual(self, x: np.ndarray, b: np.ndarray) -> float:
        return float(np.linalg.norm(self.scipy_op() @ x - b))


@tagged_union(frozen=True)
class SparseScheme:
    tag: Literal["spsolve", "splu", "factored", "krylov", "lsqr"] = tag()
    spsolve: tuple[()] = case()
    splu: tuple[()] = case()
    factored: tuple[()] = case()
    krylov: tuple[KrylovKind, float, int | None, Matvec | None] = case()
    lsqr: tuple[float, float, float] = case()

    @staticmethod
    def Spsolve() -> "SparseScheme":
        return SparseScheme(spsolve=())

    @staticmethod
    def Splu() -> "SparseScheme":
        return SparseScheme(splu=())

    @staticmethod
    def Factored() -> "SparseScheme":
        return SparseScheme(factored=())

    @staticmethod
    def Krylov(
        kind: KrylovKind = KrylovKind.CG, rtol: float = _TOL, maxiter: int | None = None, preconditioner: Matvec | None = None
    ) -> "SparseScheme":
        return SparseScheme(krylov=(kind, rtol, maxiter, preconditioner))

    @staticmethod
    def Lsqr(atol: float = _TOL, btol: float = _TOL, conlim: float = 1e8) -> "SparseScheme":
        return SparseScheme(lsqr=(atol, btol, conlim))


@tagged_union(frozen=True)
class LinearIntent:
    tag: Literal["dense_la", "sparse", "eigen", "operator"] = tag()
    dense_la: tuple[LinearMap, np.ndarray, SolveShape] = case()
    sparse: tuple[LinearMap, np.ndarray, SparseScheme] = case()
    eigen: tuple[LinearMap, int, SpectralMode] = case()
    operator: tuple[LinearMap, np.ndarray, SolveShape] = case()

    @staticmethod
    def DenseLa(matrix: LinearMap, rhs: np.ndarray, shape: SolveShape = SolveShape.SQUARE) -> "LinearIntent":
        return LinearIntent(dense_la=(matrix, rhs, shape))

    @staticmethod
    def Sparse(matrix: LinearMap, rhs: np.ndarray, scheme: SparseScheme = SparseScheme.Spsolve()) -> "LinearIntent":
        return LinearIntent(sparse=(matrix, rhs, scheme))

    @staticmethod
    def Eigen(matrix: LinearMap, k: int, mode: SpectralMode = SpectralMode.EIGENPAIRS) -> "LinearIntent":
        return LinearIntent(eigen=(matrix, k, mode))

    @staticmethod
    def Operator(matrix: LinearMap, rhs: np.ndarray, shape: SolveShape = SolveShape.SQUARE) -> "LinearIntent":
        return LinearIntent(operator=(matrix, rhs, shape))


# --- [TABLES] ------------------------------------------------------------------------------

# Structure -> lineax tag frozenset (deferred behind the gated `lineax` import; keyed by the cp315-clean enum).
_LINEAX_TAGS: FrozenDict[MatrixStructure, Callable[[object], frozenset]] = FrozenDict(
    {
        MatrixStructure.GENERAL: lambda lx: frozenset(),
        MatrixStructure.SYMMETRIC: lambda lx: frozenset({lx.symmetric_tag}),
        MatrixStructure.SPD: lambda lx: frozenset({lx.symmetric_tag, lx.positive_semidefinite_tag}),
        MatrixStructure.LOWER_TRIANGULAR: lambda lx: frozenset({lx.lower_triangular_tag}),
        MatrixStructure.UPPER_TRIANGULAR: lambda lx: frozenset({lx.upper_triangular_tag}),
        MatrixStructure.TRIDIAGONAL: lambda lx: frozenset({lx.tridiagonal_tag}),
        MatrixStructure.DIAGONAL: lambda lx: frozenset({lx.diagonal_tag}),
    }
)


# --- [OPERATIONS] --------------------------------------------------------------------------

def solve(intent: LinearIntent) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: LinearIntent) -> SolverReceipt:
    match intent:
        case LinearIntent(tag="dense_la", dense_la=(m, b, shape)):
            return _dense_receipt(m, b, shape)
        case LinearIntent(tag="sparse", sparse=(m, b, scheme)):
            return _sparse_receipt(m, b, scheme)
        case LinearIntent(tag="eigen", eigen=(m, k, mode)):
            return _eigen_receipt(m, k, mode)
        case LinearIntent(tag="operator", operator=(m, b, shape)):
            return _operator_receipt(m, b, shape)
        case unreachable:
            assert_never(unreachable)


def _dense_receipt(m: LinearMap, b: np.ndarray, shape: SolveShape) -> SolverReceipt:
    a = np.asarray(m.dense[0])
    if shape is not SolveShape.SQUARE or a.shape[0] != a.shape[1]:
        x, residuals, rank, _ = np.linalg.lstsq(a, b, rcond=None)
        residual = float(residuals[0]) if residuals.size else float(np.linalg.norm(a @ x - b))
        return SolverReceipt.LeastSquares(residual, int(rank), 0)
    try:
        import scipy.linalg as sla

        if m.structure is MatrixStructure.SPD:
            x = sla.cho_solve(sla.cho_factor(a), b)
        else:
            x = sla.solve(a, b, assume_a=m.structure.value)
    except ImportError:
        x = np.linalg.solve(a, b)
    return SolverReceipt.Direct(float(np.linalg.norm(a @ x - b)), _condition(a))


def _sparse_receipt(m: LinearMap, b: np.ndarray, scheme: SparseScheme) -> SolverReceipt:
    import scipy.sparse.linalg as spla

    a = m.sparse_mat[0]
    match scheme:
        case SparseScheme(tag="spsolve"):
            return SolverReceipt.Direct(m.residual(spla.spsolve(a, b), b), float("nan"))
        case SparseScheme(tag="splu"):
            return SolverReceipt.Direct(m.residual(spla.splu(a).solve(b), b), float("nan"))
        case SparseScheme(tag="factored"):
            return SolverReceipt.Direct(m.residual(spla.factorized(a)(b), b), float("nan"))
        case SparseScheme(tag="krylov", krylov=(kind, rtol, maxiter, precond)):
            op = m.scipy_op()
            pre = spla.LinearOperator(op.shape, matvec=precond) if precond is not None else None
            krylov = {KrylovKind.CG: spla.cg, KrylovKind.GMRES: spla.gmres, KrylovKind.BICGSTAB: spla.bicgstab}[kind]
            steps: list[int] = []
            x, info = krylov(op, b, rtol=rtol, maxiter=maxiter, M=pre, callback=lambda _: steps.append(1))
            return SolverReceipt.Iterative(m.residual(x, b), len(steps), rtol, result=_info_status(int(info)))
        case SparseScheme(tag="lsqr", lsqr=(atol, btol, conlim)):
            x, istop, itn, r1norm, *_ = spla.lsqr(m.scipy_op(), b, atol=atol, btol=btol, conlim=conlim)
            return SolverReceipt.LeastSquares(float(r1norm), 0, int(itn), result=_ISTOP.get(int(istop), "other"))
        case unreachable:
            assert_never(unreachable)


def _eigen_receipt(m: LinearMap, k: int, mode: SpectralMode) -> SolverReceipt:
    if m.tag == "dense":
        a = np.asarray(m.dense[0])
        w, v = np.linalg.eigh(a)
        residual = float(np.linalg.norm(a @ v - v * w))
        return SolverReceipt.Eigen(residual, int(w.size), _condition(a))
    import scipy.sparse.linalg as spla

    op = m.scipy_op()
    if mode is SpectralMode.SPECTRUM:
        u, s, vt = spla.svds(op, k=k)
        residual = float(np.linalg.norm(op @ vt.conj().T - u * s))
        return SolverReceipt.Eigen(residual, int(s.size), float(s.max() / s.min()))
    w, v = spla.eigsh(op, k=k)
    return SolverReceipt.Eigen(float(np.linalg.norm(op @ v - v * w)), int(k), float("nan"))


def _operator_receipt(m: LinearMap, b: np.ndarray, shape: SolveShape) -> SolverReceipt:
    import jax.numpy as jnp
    import lineax as lx

    operator = m.lineax_op(b)
    spd_free = m.structure is MatrixStructure.SPD and m.tag == "free"
    match shape:
        case SolveShape.LEAST_SQUARES:
            solver = lx.QR()
        case SolveShape.MIN_NORM:
            solver = lx.LSMR(rtol=_TOL, atol=_TOL)
        case SolveShape.SQUARE:
            solver = lx.NormalCG(rtol=_TOL, atol=_TOL) if spd_free else lx.AutoLinearSolver(well_posed=None)
        case unreachable:
            assert_never(unreachable)
    solution = lx.linear_solve(operator, jnp.asarray(b), solver, throw=False)
    x = np.asarray(solution.value)
    status = solution.result.name
    iterations = int(solution.stats.get("num_steps", 0))
    return (
        SolverReceipt.LeastSquares(m.residual(x, b), int(x.shape[0]), iterations, result=status)
        if shape is not SolveShape.SQUARE
        else SolverReceipt.Direct(m.residual(x, b), float("nan"), result=status)
    )
```

## [03]-[RESEARCH]

- [SCIPY_LINALG]: the `scipy.linalg.{solve,lstsq,eigh,cho_factor,cho_solve,lu_factor,lu_solve}` and `scipy.sparse.linalg.{spsolve,splu,factorized,cg,gmres,bicgstab,lsqr,eigsh,svds,LinearOperator}` spellings carry the `python_version<'3.15'` marker; the bodies are authored against the documented API and verify against the `.api` catalogue once the scipy wheel resolves. The numpy floor (`_dense_receipt` `np.linalg.solve`/`lstsq` with the `np.linalg.svdvals` conditioning, the `_eigen_receipt` dense arm over `np.linalg.eigh`) runs unconditionally on cp315 and is reached via the `ImportError` branch when scipy is absent, so the dense route is never band-gated. `scipy.linalg.solve(a, b, assume_a=...)` routes the symmetric/SPD/triangular/tridiagonal/diagonal dense system onto the matching LAPACK driver — the `MatrixStructure` value IS the `assume_a` string by construction (`"gen"`/`"sym"`/`"pos"`/`"lower triangular"`/`"upper triangular"`/`"tridiagonal"`/`"diagonal"`), and the SPD case factors once through `cho_factor`/`cho_solve` so a Cholesky solve replaces the general LU path without a parallel method. The Krylov `(x, info)` contract and the `lsqr` `(x, istop, itn, r1norm, ...)` tuple are the documented return shapes; `info == 0` is the only success code, `info > 0` is max-iterations, `info < 0` is illegal input — so `_info_status` is the typed-status boundary, not a residual heuristic. The Krylov, eigsh, and svds bodies accept a dense array, a sparse container, or a `LinearOperator` directly; `LinearOperator(shape, matvec, rmatvec)` lifts the matrix-free `LinearMap.Free` matvec so `cg`/`gmres`/`bicgstab`/`lsqr`/`eigsh`/`svds` solve a problem that never materializes the matrix. `splu` returns a `SuperLU` object whose `.solve(b)` back-substitutes, and `factorized` returns a reusable `solve(b)` closure — both are direct-solve evidence folding into `SolverReceipt.Direct`. `svds` returns `(u, s, vt)`; the `SPECTRUM` eigen mode reports the singular-value spectrum and its conditioning.
- [LINEAX_OPERATOR]: `lineax` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `MatrixLinearOperator`/`FunctionLinearOperator`/`DiagonalLinearOperator`/`TridiagonalLinearOperator`/`AutoLinearSolver`/`linear_solve`/`LU`/`QR`/`Cholesky`/`Triangular`/`Diagonal`/`SVD`/`LSMR`/`NormalCG`/`Solution`/`RESULTS` spellings and the `{symmetric,positive_semidefinite,lower_triangular,upper_triangular,tridiagonal,diagonal}_tag` sentinels verify against the `.api` catalogue under a uv-sync reflection pass on that band. The operand structure is the load-bearing input: `MatrixLinearOperator(matrix, tags=...)` and `FunctionLinearOperator(fn, input_structure, tags=...)` carry the `MatrixStructure`-projected tag frozenset, and `AutoLinearSolver(well_posed=None)` reads those tags to pick `Cholesky` (SPD), `Triangular` (triangular), `Diagonal` (diagonal), or `LU` (general) — so a structured dense or matrix-free system reaches its specialized solver without a hard-coded `LU()`. A genuinely diagonal or tridiagonal operand is built directly as `DiagonalLinearOperator` or through the `lineax.tridiagonal(lower, mid, upper)` constructor (extracting the three diagonals via `jnp.diagonal(d, -1|0|1)` in the documented lower/main/upper order) rather than a dense matrix. `linear_solve(operator, vector, solver, throw=False)` returns a `Solution` whose `.result` is a `RESULTS` member (`successful`/`max_steps_reached`/`singular`/`breakdown`/`stagnation`/`conlim`/`nonfinite_input`) and whose `.stats` carries the iteration count and residual norms; `throw=False` converts the default raising contract into a typed verdict the receipt folds through `solution.result.name`, which the `_STATUS` table in `solvers/receipt.md#RECEIPT` already maps one-to-one. The `SolveShape` policy picks the rectangular/under-determined cell — `QR` for least-squares, `LSMR` for the minimum-norm/ill-conditioned solve, `NormalCG` for an SPD matrix-free normal-equations solve — in place of a `least_squares` boolean. The `_LINEAX_TAGS` table defers every `lineax` tag reference behind a `Callable[[lx], frozenset]` so the cp315-clean `MatrixStructure` enum keys it without importing the gated package at module load. The Lineax solve is autodifferentiable, so `solvers/sensitivity.md#SENSITIVITY` reads the implicit-function-theorem adjoint through it rather than through the iterations.
- [NONCONVERGENCE_RAIL]: the FIX law is that an iterative or operator solve never silently returns a wrong answer and never raises on expected non-convergence. The scipy Krylov `info` code and `lsqr` `istop` code fold through `_info_status`/`_ISTOP` into the `RESULTS` member-name vocabulary the receipt keys, so `SolveStatus.MAX_STEPS`/`BREAKDOWN`/`ILL_CONDITIONED` ride out on the receipt; the lineax route uses `throw=False` and folds `solution.result.name`. The unexpected host fault (a shape mismatch, a non-finite operand the backend rejects) still rises on the `boundary` runtime rail, keeping the expected-non-convergence verdict and the unexpected-fault rail distinct rather than collapsing both into one.
