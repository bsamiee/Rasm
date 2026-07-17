# [PY_COMPUTE_LINEAR]

Linear-algebra routes of the one numeric solver. `LinearIntent` discriminates dense systems, sparse systems by scheme (direct/factored/Krylov/least-squares), eigen-and-spectral problems, and an autodifferentiable `lineax` operator tier that unifies dense, sparse, iterative, and least-squares solves over one general linear operator. One `LinearMap` value object carries a dense array, an admitted `scipy.sparse` container, or a matrix-free `matvec`, plus one `MatrixStructure` policy value — the single structure axis every route reads and every backend projects once. Two further bounded values retire boolean knobs: `SolveShape` (`SQUARE`/`LEAST_SQUARES`/`MIN_NORM`) selects the solve-vs-least-squares arm across all three backends, and `SpectralMode` (`EIGENPAIRS`/`SPECTRUM`) the eigen-vs-singular arm; the four scattered `(rtol, atol, maxiter)` tuning literals ride one `LinearPolicy`.

Reused axes and seams a rebuild composes without re-derivation: `SolverReceipt` and the shared enum-verdict `verdict` fold home to `solvers/receipt.md#RECEIPT` (`SolveStatus` is folded inside the receipt factories, never imported here), and the `_CEILING` graduation row clears through that page's `graduate` projection. `sparse_receipt` is a PUBLIC cross-module contract `solvers/quadrature.md#QUADRATURE` composes by name for its FEM arm. Gated `lineax` tiers ride the x64 float64 contract every sibling JAX route carries — `solvers/nonlinear.md#NONLINEAR`, `solvers/differential.md#DIFFERENTIAL`, `solvers/sensitivity.md#SENSITIVITY` — and its batched sweep runs the identical per-row residual contraction and worst-code verdict reduce those siblings run, since `lineax.RESULTS` shares their `equinox.Enumeration` base. Isolation is policy data on `_TRAIT`: the gated route declares `HOSTILE` (the x64 flag is process-global native state concurrent in-process solves corrupt) and the scipy bodies `RELEASING`, the runtime `Kernel` crossing deriving band and worker-death retry from the trait row; emission rides the hub `evidence_run` weave for span, fence, and receipt harvest — compute mints zero limiters and no solve retry.

## [01]-[INDEX]

- [01]-[LINEAR]: dense/sparse/eigen/operator routes on one `LinearIntent` reading one `LinearMap` operand and one `MatrixStructure` axis, the gated `lineax` tier folded into `LinearEngine`.

## [02]-[LINEAR]

- Owner: `LinearIntent` — the four route cases on the one solver, each reading one `LinearMap` operand; `Eigen` additionally carries the `EigenScheme` sparse-eigen row (`ARPACK`/`LOBPCG`/`SHIFT_INVERT`) and its `sigma` shift. `LinearIntent.solve(lane)` is the one `async` method, the inner `match self` dispatching all four routes through `assert_never` — identical in shape to `NonlinearIntent.solve`/`DifferentialIntent.solve`/`FieldQuery.evaluate`, never a free `solve(intent)` beside a free `_dispatch`.
- Cases: `LinearMap` is the ONE `@tagged_union` operand carrying one `MatrixStructure` field, exposing four total `match` projections so every route reads ONE projection rather than a raw `self.dense[0]` that raises on a mis-routed operand — `scipy_op` (the sparse-linalg operand), `dense_array` (the LAPACK operand), `matrix` (the actual sparse container the `SuperLU` factorizations need, since a factor admits no `LinearOperator`), and `residual`. Its lineax-operator projection is NOT on `LinearMap` — it lives on `LinearEngine.operator` so the value object never imports the gated `jnp`/`lx`, and the sparse case there stays matrix-free (never `a.toarray()`) so a FEM/graph-Laplacian operand stays sparse through the differentiable solve. `LinearPolicy` is the ONE frozen tuning value over every route (`tol`/`maxiter`/`preconditioner`/`batched`): the scheme discriminant carries the METHOD, this policy the TUNING, so a re-tuned solve is one value, not a re-spelled `Krylov(kind, rtol, maxiter, M)`. `SparseScheme` is the ONE sparse-route discriminant, its Krylov member indexing the full `KrylovKind` family whose enum value IS its `scipy.sparse.linalg` callable name.
- Entry: `LinearIntent.solve` composes `lane.offload` on the `_TRAIT` family row under the hub `evidence_run` weave. `boundary` converts an unexpected host fault to the runtime fault rail; the *expected* non-convergence is carried inside the success receipt as `SolveStatus`, so the two failure notions stay distinct.
- Auto: structure values drive backend selection with no per-route branch. `MatrixStructure` values ARE the scipy `solve(assume_a=...)` driver strings, so a symmetric or SPD dense system reaches the LAPACK symmetric/Cholesky driver instead of the general LU floor. For the lineax tier the `_TAG_NAMES` projection resolves the structure to a `frozenset` of documented tags that `AutoLinearSolver(well_posed=True)` reads to pick `Cholesky` → `Triangular` → `Tridiagonal` → `Diagonal` → `LU` — `well_posed=True` is load-bearing: `well_posed=None` is the rank-deficient least-squares SVD path `MIN_NORM`/`LSMR` owns and discards the structure, so the square route never passes it. A matrix-free SPD operand routes `lineax.Normal(lineax.CG(...))`, the documented normal-equations composite, NEVER the deprecated `lineax.NormalCG`.
- Output: every route folds into the one `SolverReceipt`, and every iterative/operator route folds the backend's *termination reason* into a typed `SolveStatus` — the scipy `info` through `_info_status`, the `lsqr`/`lsmr` `istop` through the shared `_ISTOP`, and the `lineax.Solution.result` member name through the receipt-owned `verdict` fold. `lineax.linear_solve(..., throw=False)` returns its verdict rather than raising, so a CG/GMRES non-convergence or singular factorization is a first-class verdict, never a silent residual-floor pass and never a raise. Its batched sweep carries the true aggregate verdict (worst column by `jnp.max` over the per-row codes) rather than a `result=None` fiction.
- Packages: `scipy` (`linalg.solve`/`lstsq`/`eigh`/`svdvals`/`norm`, the `sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack` operand builders, `sparse.linalg` `LinearOperator`/`spsolve`/`splu`/`spilu`/`factorized`/`eigsh`/`svds`/`lobpcg`/`minres`, the Krylov family `cg`/`minres`/`gmres`/`bicgstab`/`qmr`/`tfqmr`/`lgmres`/`gcrotmk`, `lsqr`/`lsmr`); `lineax` (`MatrixLinearOperator`/`FunctionLinearOperator`/`DiagonalLinearOperator`/`TridiagonalLinearOperator`, `AutoLinearSolver`/`linear_solve`/`QR`/`LSMR`/`CG`/`Normal`, the six structure tags, `Solution`/`RESULTS` — `AutoLinearSolver(well_posed=True)` owns direct-solver selection so `LU`/`Cholesky`/`Triangular`/`Tridiagonal`/`Diagonal` are never named and `NormalCG` is deleted); `equinox` (`filter_vmap` the batched multi-RHS sweep); `jax` (`config.update` floating the gated solve to float64, `ShapeDtypeStruct` the domain-sized `FunctionLinearOperator` input, `numpy.diagonal`/`asarray`/`linalg.norm`/`max`); `numpy` (dense floors); `jaxtyping` (`Float[Array, ...]` on the gated residual, checked through `beartype(conf=FAULT_CONF)`); `solvers/receipt.md#RECEIPT`, hub (`EvidenceScope`/`evidence_run`), `msgspec` (`Struct` for `LinearPolicy`), `dataclasses` (frozen `LinearEngine`), `expression.collections` (`Map` the table rail), runtime (`RuntimeRail`/`FAULT_CONF`/`LanePolicy`/`Kernel`/`KernelTrait`).
- Growth: a new structure class is one `MatrixStructure` row plus its `_TAG_NAMES` entry (the `assume_a` driver is the value itself); a new Krylov method one `KrylovKind` row (the value resolves the callable through `getattr(spla, kind.value)`); a new sparse scheme one `SparseScheme` case; a new operand backend one `LinearMap` case plus its `scipy_op`/`dense_array`/`matrix`/`krylov_preconditioner`/`LinearEngine.operator` arms; a new lineax solver cell one `LinearEngine.solver` `match shape` arm; a new tuning axis one `LinearPolicy` field; a new termination code one `_info_status` branch or `_ISTOP` row; a new sparse-eigen method one `EigenScheme` row plus its `_eigen_receipt` arm. Never a parallel dense/sparse owner, a free `lineax_solve`, a parallel matrix-free operand union, a boolean `least_squares`/`spectral` knob, or a Python loop over a multi-RHS stack.
- Boundary: operand construction stays at the boundary — the `scipy.sparse` builders assemble the banded/identity/tensor/block operands the FEM and graph-Laplacian routes feed, and `SparseMat` accepts any container with its known structure; the dispatch bodies take only the projected `scipy_op`/`operator` and the structure. Batched and lineax residuals contract over the operator's OWN `.mv`, never `scipy_op @ x` re-entering the scipy rail off a JAX solve.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import numpy as np
from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Map
from jaxtyping import Array, Float, jaxtyped
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.solvers.receipt import SolverReceipt, verdict
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

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


class EigenScheme(StrEnum):
    # ARPACK `eigsh` default, `lobpcg` for a large SPD operand, shift-invert for interior modes near `sigma`.
    ARPACK = "arpack"
    LOBPCG = "lobpcg"
    SHIFT_INVERT = "shift_invert"


# enum value IS the `scipy.sparse.linalg` callable name, so `getattr(spla, kind.value)` resolves the
# body with no identity table; the whole family shares one `(A, b, *, rtol, atol, maxiter, M, callback)`
# signature, so a new method is one row.
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

# family default graduation ceiling; a caller's tighter row overrides at the `graduate` projection.
_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", _TOL)])

# deterministic `lobpcg` initial-block seed; provenance is data, not an ambient `default_rng()` the
# receipt cannot reproduce.
_EIGEN_SEED: Final[int] = 0

# family trait rows: gated lineax is HOSTILE (the x64 flag is process-global native state); the GIL-releasing
# scipy bodies are RELEASING; isolation, band, and worker-death retry derive at the runtime crossing owner.
_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([
    ("dense_la", KernelTrait.RELEASING),
    ("sparse", KernelTrait.RELEASING),
    ("eigen", KernelTrait.RELEASING),
    ("operator", KernelTrait.HOSTILE),
])

# scipy lsqr/lsmr `istop`: 1/2/4/5 solved, 3 conlim ill-conditioned, 7 max-iterations — one shared table.
_ISTOP: Final[Map[int, str]] = Map.of_seq([
    (1, "successful"),
    (2, "successful"),
    (3, "conlim"),
    (4, "successful"),
    (5, "successful"),
    (7, "max_steps_reached"),
])

# Structure -> lineax tag-attribute names; `_tags` resolves them against the gated module into a
# frozenset, one data row per structure.
_TAG_NAMES: Final[Map[MatrixStructure, tuple[str, ...]]] = Map.of_seq([
    (MatrixStructure.GENERAL, ()),
    (MatrixStructure.SYMMETRIC, ("symmetric_tag",)),
    (MatrixStructure.SPD, ("symmetric_tag", "positive_semidefinite_tag")),
    (MatrixStructure.LOWER_TRIANGULAR, ("lower_triangular_tag",)),
    (MatrixStructure.UPPER_TRIANGULAR, ("upper_triangular_tag",)),
    (MatrixStructure.TRIDIAGONAL, ("tridiagonal_tag",)),
    (MatrixStructure.DIAGONAL, ("diagonal_tag",)),
])


# scipy Krylov `info`: 0 converged, >0 max-iterations, <0 illegal-input/breakdown.
def _info_status(info: int) -> str:
    return "successful" if info == 0 else "max_steps_reached" if info > 0 else "breakdown"


def _tags(structure: MatrixStructure, lx: object) -> frozenset:
    return frozenset(getattr(lx, name) for name in _TAG_NAMES[structure])


# 2-norm condition number from the singular spectrum.
def _condition(s: np.ndarray) -> float:
    return float(s.max() / s.min()) if s.size and s.min() > 0 else float("inf")


# A matrix-free SPD operand has no factorable matrix, so its SQUARE solve routes `Normal(CG)`; dense/
# sparse SPD keeps the tag-dispatched Cholesky. Never a stringly `m.tag` compare.
def _spd_free(m: "LinearMap") -> bool:
    match m:
        case LinearMap(tag="free", free=(_, _, _, MatrixStructure.SPD)):
            return True
        case _:
            return False


# --- [MODELS] ------------------------------------------------------------------------------


# Method rides the scheme discriminant, tuning rides here, so a re-tuned solve is one value. `Struct`
# (wire-encodable, matching every sibling policy) — `LinearEngine` stays a `dataclass` because it holds
# live gated module handles, not domain state.
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
        matvec: Matvec, shape: tuple[int, int], rmatvec: Matvec | None = None, structure: MatrixStructure = MatrixStructure.GENERAL
    ) -> "LinearMap":
        return LinearMap(free=(matvec, shape, rmatvec, structure))

    @property
    def structure(self) -> MatrixStructure:
        # total match over the closed union, `assert_never`-closed.
        match self:
            case (
                LinearMap(tag="dense", dense=(*_, MatrixStructure() as structure))
                | LinearMap(tag="sparse_mat", sparse_mat=(*_, MatrixStructure() as structure))
                | LinearMap(tag="free", free=(*_, MatrixStructure() as structure))
            ):
                return structure
            case _ as unreachable:
                assert_never(unreachable)

    # one operand the `scipy.sparse.linalg` bodies accept: dense/sparse pass through, free lifts
    # matrix-free. No gated import — the lineax lift lives on `LinearEngine`.
    def scipy_op(self) -> object:
        import scipy.sparse.linalg as spla

        match self:
            case LinearMap(tag="dense", dense=(a, _)) | LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return a
            case LinearMap(tag="free", free=(matvec, shape, rmatvec, _)):
                return spla.LinearOperator(shape, matvec=matvec, rmatvec=rmatvec)
            case _ as unreachable:
                assert_never(unreachable)

    # One projection rather than a raw `self.dense[0]` that raises on a mis-routed operand: dense passes
    # through, sparse densifies once, a matvec materialises against the identity columns. Total over the union.
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

    # sparse container the direct-factorization schemes require (a `SuperLU` factor admits no
    # `LinearOperator`): `SparseMat` returns its container, `Dense`/`Free` lift to CSR.
    def matrix(self) -> object:
        import scipy.sparse as sp

        match self:
            case LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return a
            case _:
                return sp.csr_array(self.dense_array())

    # Krylov `M=` selection folded once: an explicit preconditioner wins; a factorable operand falls to
    # its `spilu` ILU; a matrix-free `Free` has no factorable matrix, so runs unpreconditioned (`M=None`).
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


# gated jnp/lx modules folded into one carrier read off `self.lx`/`self.jnp` rather than re-imported
# per helper, matching the sibling JAX-route `.gated()` discipline. `gated()` imports once behind the band
# and floats the rail to float64.
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

    # single `LinearMap` -> lineax-operator projection, structure-tagged; the `Tridiagonal` constructor
    # takes the three diagonals via `jnp.diagonal`, never the `lineax.tridiagonal` extractor reading them from
    # a built operator. `input_structure` sizes the DOMAIN by the operand's column count (`shape[1]`), not the
    # RHS codomain which mis-sizes a non-square operand. The sparse case wraps matrix-free (never `a.toarray()`).
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

    # single `SolveShape`/structure -> solver cell. `well_posed=True` reads the operator tags to pick
    # Cholesky/Triangular/Tridiagonal/Diagonal/LU; `well_posed=None` is the rank-deficient SVD path
    # `MIN_NORM`/`LSMR` owns, so `SQUARE` never passes it.
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

    # lineax-rail residual over the operator's OWN `.mv`, returning the traced `jnp` scalar — never
    # `scipy_op @ x` re-entering the scipy rail off a JAX solve. Stays inside `filter_vmap` (a `float()` on
    # a `Tracer` raises); the jaxtyping contract rails a rank/dtype breach at the boundary, not mid-solve.
    @jaxtyped(typechecker=beartype(conf=FAULT_CONF))
    def residual(self, operator: object, x: Float[Array, "..."], b: Float[Array, "..."]) -> Float[Array, ""]:
        return self.jnp.linalg.norm(operator.mv(x) - b)

    def verdict(self, result: object) -> str:
        # one-row composition of the receipt-owned verdict fold, parameterized by the gated handle and
        # `lineax.RESULTS`: the zero-code `equinox.Enumeration` makes `max == 0` iff every column converged;
        # `RESULTS.promote` is inheritance-widening, never a vmap combine.
        return verdict(self.jnp, self.lx.RESULTS, result)


# scheme discriminates the METHOD; tuning rides the orthogonal `LinearPolicy`. `Lsqr`/`Lsmr` carry
# `conlim`, the one knob with no `LinearPolicy` peer. A re-tuned solve is one value, never a case payload.
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
    eigen: tuple[LinearMap, int, SpectralMode, EigenScheme, float | None] = case()
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
    def Eigen(
        matrix: LinearMap, k: int, mode: SpectralMode = SpectralMode.EIGENPAIRS, scheme: EigenScheme = EigenScheme.ARPACK, sigma: float | None = None
    ) -> "LinearIntent":
        return LinearIntent(eigen=(matrix, k, mode, scheme, sigma))

    @staticmethod
    def Operator(matrix: LinearMap, rhs: np.ndarray, shape: SolveShape = SolveShape.SQUARE, policy: LinearPolicy = LinearPolicy()) -> "LinearIntent":
        return LinearIntent(operator=(matrix, rhs, shape, policy))

    async def solve(self, lane: LanePolicy) -> "RuntimeRail[SolverReceipt]":
        # composes the runtime crossing on the family trait row — isolation, band, and worker-death retry all derive
        # from the Kernel value, wrapping the isolation leg, never the solve. The weave owns span/fence/harvest.
        async def dispatch() -> RuntimeRail[SolverReceipt]:
            return await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self)

        return await evidence_run(EvidenceScope.LINEAR, f"solve.{self.tag}", dispatch)


# --- [OPERATIONS] --------------------------------------------------------------------------


# one measured kernel — module-level and import-resolvable, so it crosses the process lane as spec
# data plus operands.
def _dispatch(intent: LinearIntent) -> SolverReceipt:
    match intent:
        case LinearIntent(tag="dense_la", dense_la=(m, b, shape)):
            return _dense_receipt(m, b, shape)
        case LinearIntent(tag="sparse", sparse=(m, b, scheme, policy)):
            return sparse_receipt(m, b, scheme, policy)
        case LinearIntent(tag="eigen", eigen=(m, k, mode, scheme, sigma)):
            return _eigen_receipt(m, k, mode, scheme, sigma)
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

        # `assume_a=m.structure.value` IS the LAPACK driver selector: `"pos"` reaches the Cholesky `?posv`
        # driver, `"sym"` `?sysv` — no SPD-special-case `cho_*` pair.
        x = sla.solve(a, b, assume_a=m.structure.value)
    except ImportError:
        x = np.linalg.solve(a, b)
    return SolverReceipt.Direct(float(np.linalg.norm(a @ x - b)), _condition(np.linalg.svdvals(a)))


# PUBLIC: `solvers/quadrature.md#QUADRATURE` composes this by name for its FEM arm, never a private `_sparse_receipt`.
def sparse_receipt(m: LinearMap, b: np.ndarray, scheme: SparseScheme, policy: LinearPolicy) -> SolverReceipt:
    import scipy.sparse.linalg as spla

    # Direct schemes read `m.matrix()` (only a sparse container admits a `SuperLU`); Krylov/lsqr read the
    # matrix-free `m.scipy_op()` so a `Free` FEM operand reaches them without materialising a matrix.
    match scheme:
        case SparseScheme(tag="spsolve"):
            return SolverReceipt.Direct(m.residual(spla.spsolve(m.matrix(), b), b), float("nan"))
        # `splu` exact factor, `spilu` incomplete — both return a `SuperLU` whose `.solve(b)` back-substitutes.
        case SparseScheme(tag="splu"):
            return SolverReceipt.Direct(m.residual(spla.splu(m.matrix()).solve(b), b), float("nan"))
        case SparseScheme(tag="spilu", spilu=(drop_tol, fill_factor)):
            return SolverReceipt.Direct(m.residual(spla.spilu(m.matrix(), drop_tol=drop_tol, fill_factor=fill_factor).solve(b), b), float("nan"))
        case SparseScheme(tag="factored"):
            return SolverReceipt.Direct(m.residual(spla.factorized(m.matrix())(b), b), float("nan"))
        case SparseScheme(tag="krylov", krylov=(kind,)):
            op = m.scipy_op()
            pre = m.krylov_preconditioner(policy.preconditioner, spla)
            steps: list[int] = []
            # `gmres` alone takes `callback_type`; `"x"` fires once per OUTER iteration so `len(steps)` is
            # comparable to the cg/bicgstab per-iteration count, not the `"pr_norm"` per-inner default.
            extra = {"callback_type": "x"} if kind is KrylovKind.GMRES else {}
            x, info = getattr(spla, kind.value)(op, b, rtol=policy.tol, maxiter=policy.maxiter, M=pre, callback=lambda *_: steps.append(1), **extra)
            return SolverReceipt.Iterative(m.residual(x, b), len(steps), policy.tol, result=_info_status(int(info)))
        # `lsqr`/`lsmr` both return `(x, istop, itn, normr, ...)` with the same `istop` vocabulary, one or-pattern.
        case SparseScheme(tag="lsqr", lsqr=(conlim,)) | SparseScheme(tag="lsmr", lsmr=(conlim,)):
            x, istop, itn, r1norm, *_ = getattr(spla, scheme.tag)(m.scipy_op(), b, atol=policy.tol, btol=policy.tol, conlim=conlim)
            return SolverReceipt.LeastSquares(float(r1norm), 0, int(itn), result=_ISTOP.try_find(int(istop)).default_value("other"))
        case _ as unreachable:
            assert_never(unreachable)


# `mode` honoured on both bands: SPECTRUM the singular spectrum, EIGENPAIRS the symmetric eigenpairs.
# `ArpackNoConvergence` CARRIES the converged pairs, folded with `result="max_steps_reached"` rather than
# discarded — a boundary-kernel catch, not domain control flow.
def _eigen_receipt(m: LinearMap, k: int, mode: SpectralMode, scheme: EigenScheme, sigma: float | None) -> SolverReceipt:
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
            try:
                match scheme:
                    case EigenScheme.LOBPCG:
                        # seeded orthonormal block (`_EIGEN_SEED` makes it deterministic); `largest=False`
                        # recovers the low modes a FEM operand wants.
                        block = np.linalg.qr(np.random.default_rng(_EIGEN_SEED).standard_normal((op.shape[0], k)))[0]
                        w, v = spla.lobpcg(op, block, largest=False)
                    case EigenScheme.SHIFT_INVERT:
                        # interior modes near `sigma`: a factorable operand lets scipy factor `(A - σI)`
                        # internally; a matrix-free operand supplies the inverse action as `OPinv` via
                        # `minres(op, rhs, shift=σ)` off its own matvec.
                        opinv = None
                        if m.tag == "free":
                            opinv = spla.LinearOperator(op.shape, matvec=lambda rhs: spla.minres(op, rhs, shift=sigma or 0.0)[0])
                        w, v = spla.eigsh(op, k=k, sigma=sigma, OPinv=opinv)
                    case EigenScheme.ARPACK:
                        w, v = spla.eigsh(op, k=k)
                    case _ as unreachable:
                        assert_never(unreachable)
            except spla.ArpackNoConvergence as stalled:
                w, v = stalled.eigenvalues, stalled.eigenvectors
                partial = float(np.linalg.norm(op @ v - v * w)) if w.size else float("inf")
                return SolverReceipt.Eigen(partial, int(w.size), float("nan"), result="max_steps_reached")
            return SolverReceipt.Eigen(float(np.linalg.norm(op @ v - v * w)), int(np.asarray(w).size), float("nan"))
        case _ as unreachable:
            assert_never(unreachable)


# One float64-floated lineax rail; `linear_solve(..., throw=False)` returns a typed verdict rather than
# raising. Batched vmaps one operator over the RHS stack through `filter_vmap(in_axes=(None, 0))` as one
# compiled solve, a second contracting the per-row residual.
def _operator_receipt(m: LinearMap, b: np.ndarray, shape: SolveShape, policy: LinearPolicy) -> SolverReceipt:
    import equinox as eqx

    e = LinearEngine.gated()
    operator = e.operator(m)  # input_structure rides the operand's column count, so no RHS is needed to build it
    solver = e.solver(shape, m.structure, spd_free=_spd_free(m), tol=policy.tol, maxiter=policy.maxiter)
    run = lambda op, v: e.lx.linear_solve(op, v, solver, throw=False)
    # lineax direct solvers return `stats == {}`, so `num_steps` reads through `.get(..., 0)`; `0` is the
    # truthful iteration count for a one-shot factorization.
    if policy.batched:
        stack = e.jnp.asarray(b)
        solution = eqx.filter_vmap(run, in_axes=(None, 0))(operator, stack)
        per_row = eqx.filter_vmap(lambda v, rhs: e.residual(operator, v, rhs), in_axes=(0, 0))(solution.value, stack)
        status, iterations = e.verdict(solution.result), int(np.asarray(solution.stats.get("num_steps", 0)).max())
        residual = float(e.jnp.max(per_row))
    else:
        rhs = e.jnp.asarray(b)
        solution = run(operator, rhs)
        status, iterations = e.verdict(solution.result), int(solution.stats.get("num_steps", 0))
        residual = float(e.residual(operator, solution.value, rhs))
    # `Solution` exposes no rank; the slot stays 0 (unknown), never `x.size` (the solution dimension, not
    # operator rank the slot names).
    return (
        SolverReceipt.LeastSquares(residual, 0, iterations, result=status)
        if shape is not SolveShape.SQUARE
        else SolverReceipt.Direct(residual, float("nan"), result=status)
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
