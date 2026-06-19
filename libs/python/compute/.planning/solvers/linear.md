# [PY_COMPUTE_LINEAR]

The linear-algebra routes of the one numeric solver. `LinearIntent` discriminates dense linear systems, sparse systems by Krylov scheme, and eigenproblems over `scipy.linalg`/`scipy.sparse.linalg` with a numpy floor, plus the Lineax tier that unifies dense, sparse, and iterative solves and least-squares over a general linear operator as one autodifferentiable surface. Every route folds into the one `SolverReceipt`; the dense and dense-symmetric paths run unconditionally on the numpy floor, the scipy sparse and eigen paths gate on the scipy wheel, and the Lineax tier gates on the jaxlib floor. Lineax closes the loop with the differentiation owner because its solves are differentiable through the implicit function theorem.

## [01]-[INDEX]

- [01]-[LINEAR]: dense/sparse/eigen routes over scipy plus the Lineax autodifferentiable operator case, all folded into one `LinearIntent` owner.

## [02]-[LINEAR]

- Owner: `LinearIntent` — the four linear-route cases on the one solver; `DenseLa(matrix, rhs)` over `scipy.linalg.solve`/`lstsq` with a `np.linalg.solve`/`lstsq` floor, `Sparse(matrix, rhs, scheme)` over `scipy.sparse.linalg.spsolve`/`cg`/`gmres`/`lsqr`, `Eigen(matrix, k)` over `scipy.linalg.eigh`/`scipy.sparse.linalg.eigsh` with a `np.linalg.eigvalsh` floor, and `Operator(matrix, rhs, least_squares)` for the autodifferentiable Lineax route. `SparseScheme` selects the Krylov solver, `solve` returns `RuntimeRail[SolverReceipt]`, and `_dispatch` matches the four routes total.
- Lineax case: the `Operator` route lifts a dense matrix or a matrix-free function into one `lineax.MatrixLinearOperator`/`FunctionLinearOperator`, and `lineax.linear_solve(operator, vector, solver)` resolves the system with one of `lineax.LU`/`lineax.QR`/`lineax.CG`/`lineax.GMRES`/`lineax.NormalCG`, folding the `lineax.Solution.stats` (iteration count and residual) into `SolverReceipt`; the least-squares variant runs `lineax.QR()` over a rectangular operator. The Lineax solve is autodifferentiable, so a downstream `vjp` through the solver reads the implicit-function-theorem adjoint rather than differentiating the iterations — the case folds into the same `_dispatch` rather than standing as a parallel solve entry beside it.
- Entry: `LinearIntent.solve` enters one `boundary(f"solve.{intent.tag}", ...)`; the dense case computes the residual norm and condition number, the sparse case folds the per-scheme `(x, info)` into `Direct`/`Iterative`/`LeastSquares`, the eigen case reports the spectral residual against the recovered eigenpairs, and the operator case folds the Lineax `Solution.stats` into `Direct` or `LeastSquares`.
- Packages: `scipy` (`linalg.solve`, `linalg.lstsq`, `linalg.eigh`, `sparse.linalg.spsolve`, `sparse.linalg.cg`, `sparse.linalg.gmres`, `sparse.linalg.lsqr`, `sparse.linalg.eigsh`), `lineax` (`MatrixLinearOperator`, `FunctionLinearOperator`, `linear_solve`, `LU`, `QR`, `CG`, `GMRES`, `NormalCG`, `Solution`), `numpy` (`linalg.solve`, `linalg.lstsq`, `linalg.eigvalsh`, `linalg.norm`, `linalg.cond`), `solvers/receipt.md#RECEIPT` (`SolverReceipt`, `SolveMethod`), runtime (`RuntimeRail`, `boundary`).
- Growth: a new Krylov scheme is one `SparseScheme` row; a new Lineax solver is one `match` arm folding `Solution.stats`; a new linear route is one `LinearIntent` case; zero new surface, never a parallel dense/sparse owner and never a free `lineax_solve` beside the owner.
- Boundary: the dense and dense-symmetric numpy floors run unconditionally on cp315; `scipy` and `lineax`/`jaxlib` carry no cp315 wheel, so the scipy sparse/eigen bodies and the Lineax operator case are authored against the documented API with a reachable numpy-floor branch. A per-scheme method family, a separate eigenproblem owner, a free Lineax solve function, and four parallel scipy entry points where one Lineax operator abstraction folds dense, sparse, and iterative solves are the deleted forms.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union

from rasm.compute.solvers.receipt import SolverReceipt
from rasm.runtime.faults import RuntimeRail, boundary


class SparseScheme(StrEnum):
    SPSOLVE = "spsolve"
    CG = "cg"
    GMRES = "gmres"
    LSQR = "lsqr"


@tagged_union(frozen=True)
class LinearIntent:
    tag: Literal["dense_la", "sparse", "eigen", "operator"] = tag()
    dense_la: tuple[np.ndarray, np.ndarray] = case()
    sparse: tuple[object, np.ndarray, SparseScheme] = case()
    eigen: tuple[object, int] = case()
    operator: tuple[np.ndarray, np.ndarray, bool] = case()

    @staticmethod
    def DenseLa(matrix: np.ndarray, rhs: np.ndarray) -> "LinearIntent":
        return LinearIntent(dense_la=(matrix, rhs))

    @staticmethod
    def Sparse(matrix: object, rhs: np.ndarray, scheme: SparseScheme = SparseScheme.SPSOLVE) -> "LinearIntent":
        return LinearIntent(sparse=(matrix, rhs, scheme))

    @staticmethod
    def Eigen(matrix: object, k: int) -> "LinearIntent":
        return LinearIntent(eigen=(matrix, k))

    @staticmethod
    def Operator(matrix: np.ndarray, rhs: np.ndarray, *, least_squares: bool = False) -> "LinearIntent":
        return LinearIntent(operator=(matrix, rhs, least_squares))


def solve(intent: LinearIntent) -> "RuntimeRail[SolverReceipt]":
    return boundary(f"solve.{intent.tag}", lambda: _dispatch(intent))


def _dispatch(intent: LinearIntent) -> SolverReceipt:
    match intent:
        case LinearIntent(tag="dense_la", dense_la=(a, b)):
            return _dense_receipt(a, b)
        case LinearIntent(tag="sparse", sparse=(a, b, scheme)):
            return _sparse_receipt(a, b, scheme)
        case LinearIntent(tag="eigen", eigen=(a, k)):
            return _eigen_receipt(a, k)
        case LinearIntent(tag="operator", operator=(a, b, least_squares)):
            return _operator_receipt(a, b, least_squares)
        case unreachable:
            assert_never(unreachable)


def _dense_receipt(a: np.ndarray, b: np.ndarray) -> SolverReceipt:
    if a.shape[0] == a.shape[1]:
        x = np.linalg.solve(a, b)
        return SolverReceipt.Direct(float(np.linalg.norm(a @ x - b)), float(np.linalg.cond(a)))
    x, residuals, rank, _ = np.linalg.lstsq(a, b, rcond=None)
    residual = float(residuals[0]) if residuals.size else float(np.linalg.norm(a @ x - b))
    return SolverReceipt.LeastSquares(residual, int(rank), 0)


def _sparse_receipt(a: object, b: np.ndarray, scheme: SparseScheme) -> SolverReceipt:
    import scipy.sparse.linalg as spla

    tol = 1e-10
    match scheme:
        case SparseScheme.SPSOLVE:
            x = spla.spsolve(a, b)
            return SolverReceipt.Direct(float(np.linalg.norm(a @ x - b)), float("nan"))
        case SparseScheme.CG:
            x, info = spla.cg(a, b, rtol=tol)
            return SolverReceipt.Iterative(float(np.linalg.norm(a @ x - b)), max(info, 0), tol)
        case SparseScheme.GMRES:
            x, info = spla.gmres(a, b, rtol=tol)
            return SolverReceipt.Iterative(float(np.linalg.norm(a @ x - b)), max(info, 0), tol)
        case SparseScheme.LSQR:
            x, istop, itn, r1norm, *_ = spla.lsqr(a, b, atol=tol, btol=tol)
            return SolverReceipt.LeastSquares(float(r1norm), int(istop), int(itn))
        case unreachable:
            assert_never(unreachable)


def _eigen_receipt(a: object, k: int) -> SolverReceipt:
    if isinstance(a, np.ndarray):
        w = np.linalg.eigvalsh(a)
        return SolverReceipt.Eigen(float(np.spacing(np.abs(w).max())), int(w.size), float(np.linalg.cond(a)))
    import scipy.sparse.linalg as spla

    w, v = spla.eigsh(a, k=k)
    residual = float(np.linalg.norm(a @ v - v * w))
    return SolverReceipt.Eigen(residual, int(k), float("nan"))


def _operator_receipt(a: np.ndarray, b: np.ndarray, least_squares: bool) -> SolverReceipt:
    import lineax as lx

    operator = lx.MatrixLinearOperator(a)
    solution = lx.linear_solve(operator, b, lx.QR() if least_squares else lx.LU())
    residual = float(np.linalg.norm(np.asarray(a) @ np.asarray(solution.value) - b))
    iterations = int(solution.stats.get("num_steps", 0))
    return SolverReceipt.LeastSquares(residual, int(a.shape[1]), iterations) if least_squares else SolverReceipt.Direct(residual, float("nan"))
```

## [03]-[RESEARCH]

- [SCIPY_LINALG]: the `scipy.linalg.{solve,lstsq,eigh}` and `scipy.sparse.linalg.{spsolve,cg,gmres,lsqr,eigsh}` spellings carry the `python_version<'3.15'` marker; the bodies are authored against the documented API and verify against the `.api` catalogue once the scipy wheel resolves. The numpy floor (`_dense_receipt`, the `_eigen_receipt` numpy arm) runs unconditionally on cp315.
- [LINEAX_OPERATOR]: `lineax` resolves on the gated `python_version<'3.15'` band riding the jaxlib floor; the `MatrixLinearOperator`/`FunctionLinearOperator`/`linear_solve`/`LU`/`QR`/`CG`/`GMRES`/`NormalCG`/`Solution.stats` spellings verify against the `.api` catalogue under a uv-sync reflection pass on that band. The Lineax solve is autodifferentiable, so `solvers/sensitivity.md#SENSITIVITY` reads the implicit-function-theorem adjoint through it rather than through the iterations.
