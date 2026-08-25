# [PY_COMPUTE_LINEAR]

Linear-algebra routes of the one numeric solver. `LinearIntent` discriminates dense systems, sparse systems by scheme (direct/factored/Krylov/least-squares), eigen-and-spectral problems, and an autodifferentiable `lineax` operator tier unifying every solve over one general linear operator. One `LinearMap` value object carries a dense array, an admitted `scipy.sparse` container, or a matrix-free `matvec`, with one `MatrixStructure` policy value — the single structure axis every route reads and every backend projects once. Two bounded values retire boolean knobs: `SolveShape` (`SQUARE`/`LEAST_SQUARES`/`MIN_NORM`) selects the solve-vs-least-squares arm across all three backends, and `SpectralMode` (`EIGENPAIRS`/`SPECTRUM`) the eigen-vs-singular arm; tuning literals ride one `LinearPolicy`.

Reused axes and seams a rebuild composes without re-derivation: `Solve` and the shared enum-verdict `verdict` fold home to `solvers/solve#SOLVE` (`SolveStatus` is folded inside the `Solve` factories, never imported here), and `_CEILING` is the family default ceiling a solver-axis crossing composes at that page's `graduate` projection beside the `Solve`-projected ledger. `sparse_solve` is a PUBLIC cross-module contract `solvers/quadrature#QUADRATURE` composes by name for its FEM arm. Gated `lineax` tiers ride the x64 float64 contract every sibling JAX route carries — `solvers/nonlinear#NONLINEAR`, `solvers/differential#DIFFERENTIAL`, `solvers/sensitivity#SENSITIVITY` — and its batched sweep runs the identical per-row residual contraction and worst-code verdict reduce those siblings run, since `lineax.RESULTS` shares their `equinox.Enumeration` base. Isolation is policy data on `_TRAIT`: the gated route declares `HOSTILE` (the x64 flag is process-global native state concurrent in-process solves corrupt) and the scipy bodies `RELEASING`, the runtime `Kernel` crossing deriving band and worker-death retry from the trait row; the hub `evidence_run` weave owns span and fence, each `Solve` stamping its `attributes` there — compute mints zero limiters and no solve retry. The `[03]-[EXCHANGE]` sparse containers mirror the C# `Tensor/factor#SPARSE_SOLVE` exchange convention as hand-copied wire law, so a layout edit there lands its ripple here in the same change.

## [01]-[INDEX]

- [02]-[LINEAR]: dense/sparse/eigen/operator routes on one `LinearIntent` reading one `LinearMap` operand and one `MatrixStructure` axis, the gated `lineax` tier folded into `LinearEngine`.
- [03]-[EXCHANGE]: the two-container sparse exchange with the C# factor lane — `.mtx` over `scipy.io` and the scipy-convention HDF5 group over `h5py` — on one `SparseExchange` owner, its write twins landing the operational audit and storage charge.

## [02]-[LINEAR]

- Owner: `LinearIntent` — the four route cases on the one solver, each reading one `LinearMap` operand; `Eigen` carries the `EigenScheme` sparse-eigen row (`ARPACK`/`LOBPCG`/`SHIFT_INVERT`) and its `sigma` shift. `LinearIntent.solve(lane)` is the one `async` method, the inner `match self` dispatching all four routes through `assert_never` — identical in shape to `NonlinearIntent.solve`/`DifferentialIntent.solve`/`FieldQuery.evaluate`, never a free `solve(intent)` beside a free `_dispatch`.
- Cases: `LinearMap` is the ONE `@tagged_union` operand carrying one `MatrixStructure` field, exposing four total `match` projections so every route reads ONE projection rather than a raw `self.dense[0]` that raises on a mis-routed operand — `scipy_op` (the sparse-linalg operand), `dense_array` (the LAPACK operand), `matrix` (the actual sparse container the `SuperLU` factorizations need, since a factor admits no `LinearOperator`), and `residual`. Its lineax-operator projection is NOT on `LinearMap` — it lives on `LinearEngine.operator` so the value object never imports the gated `jnp`/`lx`, and the sparse case there stays matrix-free (never `a.toarray()`) so a FEM/graph-Laplacian operand stays sparse through the differentiable solve. `LinearPolicy` is the ONE frozen tuning value over every route (`tol`/`maxiter`/`preconditioner`): the scheme discriminant carries the METHOD, this policy the TUNING, so a re-tuned solve is one value, not a re-spelled `Krylov(kind, rtol, maxiter, M)`; the multi-RHS sweep discriminates on the RHS rank itself, never a `batched` knob. `SparseScheme` is the ONE sparse-route discriminant, its Krylov member indexing the full `KrylovKind` family whose enum value IS its `scipy.sparse.linalg` callable name.
- Law: only the dense route holds a singular spectrum, so `condition` is measured there alone — `_condition` answers `None` on an empty or rank-deficient spectrum where no finite ratio exists, and every sparse, ARPACK-stalled, and lineax-operator arm constructs its `Solve` without the slot. The declared-residual ledger then omits an unmeasured slot and the hub refuses a conditioning ceiling by key coverage; a `float("nan")` or `float("inf")` in the slot is the deleted form, since either enters the ledger as a value and breaches the hub's finiteness refinement on every sparse crossing instead of grading.
- Entry: `LinearIntent.solve` composes `lane.offload` on the `_TRAIT` family row under the hub `evidence_run` weave, threading the caller's composition `ScopeKey` onto the weave and `graduates` onto the crossing so an embedded composition's lifecycle and admission facts key to it; both default `DEFAULT_SCOPE`. `boundary` converts an unexpected host fault to the runtime fault rail; the *expected* non-convergence is carried inside the `Solve` as `SolveStatus`, so the two failure notions stay distinct.
- Auto: structure values drive backend selection with no per-route branch. `MatrixStructure` values ARE the scipy `solve(assume_a=...)` driver strings, so a symmetric or SPD dense system reaches the LAPACK symmetric/Cholesky driver instead of the general LU floor. For the lineax tier the `_TAG_NAMES` projection resolves the structure to a `frozenset` of documented tags that `AutoLinearSolver(well_posed=True)` reads to pick `Cholesky` → `Triangular` → `Tridiagonal` → `Diagonal` → `LU` — `well_posed=True` is load-bearing: `well_posed=None` is the rank-deficient least-squares SVD path `MIN_NORM`/`LSMR` owns and discards the structure, so the square route never passes it. A matrix-free SPD operand routes `lineax.Normal(lineax.CG(...))`, the documented normal-equations composite, NEVER the deprecated `lineax.NormalCG`.
- Output: every route folds its actual solution into `Solve` — dense/sparse/operator vectors, singular values, or eigenvalue/eigenvector pairs — beside the backend's termination reason. Scipy `info` folds through `_info_status`, `lsqr`/`lsmr` `istop` through `_ISTOP`, and `lineax.Solution.result` through the `Solve`-owned `verdict`; the batched sweep carries the worst code by `jnp.max`.
- Packages: `scipy` (`linalg.solve`/`lstsq`/`eigh`/`svdvals`/`norm`, the `sparse.diags_array`/`eye_array`/`kron`/`hstack`/`vstack` operand builders, `sparse.linalg` `LinearOperator`/`spsolve`/`splu`/`spilu`/`factorized`/`eigsh`/`svds`/`lobpcg`/`minres`, the Krylov family `cg`/`minres`/`gmres`/`bicgstab`/`qmr`/`tfqmr`/`lgmres`/`gcrotmk`, `lsqr`/`lsmr`); `lineax` (`MatrixLinearOperator`/`FunctionLinearOperator`/`DiagonalLinearOperator`/`TridiagonalLinearOperator`, `AutoLinearSolver`/`linear_solve`/`QR`/`LSMR`/`CG`/`Normal`, the six structure tags, `Solution`/`RESULTS` — `AutoLinearSolver(well_posed=True)` owns direct-solver selection so `LU`/`Cholesky`/`Triangular`/`Tridiagonal`/`Diagonal` are never named and `NormalCG` is deleted); `equinox` (`filter_vmap` the batched multi-RHS sweep); `jax` (`config.update` floating the gated solve to float64, `ShapeDtypeStruct` the domain-sized `FunctionLinearOperator` input, `numpy.diagonal`/`asarray`/`linalg.norm`/`max`); `numpy` (dense floors); `jaxtyping` (`Float[Array, ...]` on the gated residual, checked through `beartype(conf=FAULT_CONF)`); `solvers/solve#SOLVE`, hub (`EvidenceScope`/`evidence_run`), `msgspec` (`Struct` for `LinearPolicy`), `dataclasses` (frozen `LinearEngine`), `expression.collections` (`Map` the table rail), runtime (`RuntimeRail`/`FAULT_CONF`/`LanePolicy`/`Kernel`/`KernelTrait`, and `ScopeKey`/`DEFAULT_SCOPE` for the composition key both entries thread).
- Growth: a new structure class is one `MatrixStructure` row with its `_TAG_NAMES` entry (the `assume_a` driver is the value itself); a new Krylov method one `KrylovKind` row (the value resolves the callable through `getattr(spla, kind.value)`); a new sparse scheme one `SparseScheme` case; a new operand backend one `LinearMap` case with its `scipy_op`/`dense_array`/`matrix`/`krylov_preconditioner`/`LinearEngine.operator` arms; a new lineax solver cell one `LinearEngine.solver` `match shape` arm; a new tuning axis one `LinearPolicy` field; a new termination code one `_info_status` branch or `_ISTOP` row; a new sparse-eigen method one `EigenScheme` row with its `_eigen_solve` arm. Never a parallel dense/sparse owner, a free `lineax_solve`, a parallel matrix-free operand union, a boolean `least_squares`/`spectral` knob, or a Python loop over a multi-RHS stack.
- Boundary: operand construction stays at the boundary — the `scipy.sparse` builders assemble the banded/identity/tensor/block operands the FEM and graph-Laplacian routes feed, and `SparseMat` accepts any container with its known structure; the dispatch bodies take only the projected `scipy_op`/`operator` and the structure. Batched and lineax residuals contract over the operator's OWN `.mv`, never `scipy_op @ x` re-entering the scipy rail off a JAX solve.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import numpy as np
from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from jaxtyping import Array, Float, jaxtyped
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, Graduation, evidence_run
from rasm.compute.solvers.solve import Provider, Solve, graduate, verdict
from rasm.runtime.faults import FAULT_CONF, TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import scipy.linalg as sla
lazy import scipy.sparse as sp
lazy import scipy.sparse.linalg as spla

# --- [TYPES] ----------------------------------------------------------------------------

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
    ARPACK = "arpack"
    LOBPCG = "lobpcg"
    SHIFT_INVERT = "shift_invert"


class KrylovKind(StrEnum):
    CG = "cg"
    MINRES = "minres"
    GMRES = "gmres"
    BICGSTAB = "bicgstab"
    QMR = "qmr"
    TFQMR = "tfqmr"
    LGMRES = "lgmres"
    GCROTMK = "gcrotmk"


# --- [CONSTANTS] ------------------------------------------------------------------------

_TOL: float = 1e-10

_CEILING: Final[Map[str, float]] = Map.of_seq([("residual", _TOL)])

_EIGEN_SEED: Final[int] = 0

_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([
    ("dense_la", KernelTrait.RELEASING),
    ("sparse", KernelTrait.RELEASING),
    ("eigen", KernelTrait.RELEASING),
    ("operator", KernelTrait.HOSTILE),
])

_ISTOP: Final[Map[int, str]] = Map.of_seq([
    (1, "successful"),
    (2, "successful"),
    (3, "conlim"),
    (4, "successful"),
    (5, "successful"),
    (7, "max_steps_reached"),
])

_TAG_NAMES: Final[Map[MatrixStructure, tuple[str, ...]]] = Map.of_seq([
    (MatrixStructure.GENERAL, ()),
    (MatrixStructure.SYMMETRIC, ("symmetric_tag",)),
    (MatrixStructure.SPD, ("symmetric_tag", "positive_semidefinite_tag")),
    (MatrixStructure.LOWER_TRIANGULAR, ("lower_triangular_tag",)),
    (MatrixStructure.UPPER_TRIANGULAR, ("upper_triangular_tag",)),
    (MatrixStructure.TRIDIAGONAL, ("tridiagonal_tag",)),
    (MatrixStructure.DIAGONAL, ("diagonal_tag",)),
])


def _info_status(info: int) -> str:
    return "successful" if info == 0 else "max_steps_reached" if info > 0 else "breakdown"


def _tags(structure: MatrixStructure, lx: object) -> frozenset:
    return frozenset(getattr(lx, name) for name in _TAG_NAMES[structure])


def _condition(s: np.ndarray) -> float | None:
    return float(s.max() / s.min()) if s.size and s.min() > 0 else None


def _spd_free(m: "LinearMap") -> bool:
    match m:
        case LinearMap(tag="free", free=(_, _, _, MatrixStructure.SPD)):
            return True
        case _:
            return False


# --- [TABLES] ---------------------------------------------------------------------------

MTX_READ: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.LINEAR, point="read_mtx", arm="boundary", defect="mtx-read", retriability=TERMINAL
)
MTX_WRITE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.LINEAR, point="write_mtx", arm="boundary", defect="mtx-write", retriability=TERMINAL
)
ARCHIVE_READ: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.LINEAR, point="read_archive", arm="boundary", defect="archive-read", retriability=TERMINAL
)
ARCHIVE_WRITE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.LINEAR, point="write_archive", arm="boundary", defect="archive-write", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([MTX_READ, MTX_WRITE, ARCHIVE_READ, ARCHIVE_WRITE]))


# --- [MODELS] ---------------------------------------------------------------------------


class LinearPolicy(Struct, frozen=True):
    tol: float = _TOL
    maxiter: int | None = None
    preconditioner: Matvec | None = None


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
        match self:
            case (
                LinearMap(tag="dense", dense=(*_, MatrixStructure() as structure))
                | LinearMap(tag="sparse_mat", sparse_mat=(*_, MatrixStructure() as structure))
                | LinearMap(tag="free", free=(*_, MatrixStructure() as structure))
            ):
                return structure
            case _ as unreachable:
                assert_never(unreachable)

    def scipy_op(self) -> object:
        match self:
            case LinearMap(tag="dense", dense=(a, _)) | LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return a
            case LinearMap(tag="free", free=(matvec, shape, rmatvec, _)):
                return spla.LinearOperator(shape, matvec=matvec, rmatvec=rmatvec)
            case _ as unreachable:
                assert_never(unreachable)

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

    def matrix(self) -> object:
        match self:
            case LinearMap(tag="sparse_mat", sparse_mat=(a, _)):
                return a
            case _:
                return sp.csr_array(self.dense_array())

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


@dataclass(frozen=True, slots=True)
class LinearEngine:
    jax: object
    jnp: object
    lx: object
    eqx: object

    @classmethod
    def gated(cls) -> Self:
        import jax

        jax.config.update("jax_enable_x64", True)
        import equinox as eqx
        import jax.numpy as jnp
        import lineax as lx

        return cls(jax=jax, jnp=jnp, lx=lx, eqx=eqx)

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

    @jaxtyped(typechecker=beartype(conf=FAULT_CONF))
    def residual(self, operator: object, x: Float[Array, "..."], b: Float[Array, "..."]) -> Float[Array, ""]:
        return self.jnp.linalg.norm(operator.mv(x) - b)

    def verdict(self, result: object) -> str:
        return verdict(self.jnp, self.lx.RESULTS, result)


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

    async def solve(
        self, lane: LanePolicy, key: ContentKey, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[Solve[np.ndarray | tuple[np.ndarray, np.ndarray]]]":
        async def dispatch() -> "RuntimeRail[Solve[np.ndarray | tuple[np.ndarray, np.ndarray]]]":
            return await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self, key)

        return await evidence_run(EvidenceScope.LINEAR, f"solve.{self.tag}", dispatch, facts={"route": self.tag}, composition=composition)

    def graduates(
        self, solve: "Solve[np.ndarray | tuple[np.ndarray, np.ndarray]]", ceiling: dict[str, float] | None = None,
        *, composition: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeRail[Graduation]":
        return graduate(
            EvidenceScope.LINEAR.value, f"solve.{self.tag}", solve.content_key, solve, ceiling or dict(_CEILING.items()),
            composition=composition,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dispatch(intent: LinearIntent, key: ContentKey) -> "Solve[np.ndarray | tuple[np.ndarray, np.ndarray]]":
    match intent:
        case LinearIntent(tag="dense_la", dense_la=(m, b, shape)):
            return _dense_solve(key, m, b, shape)
        case LinearIntent(tag="sparse", sparse=(m, b, scheme, policy)):
            return sparse_solve(key, m, b, scheme, policy)
        case LinearIntent(tag="eigen", eigen=(m, k, mode, scheme, sigma)):
            return _eigen_solve(key, m, k, mode, scheme, sigma)
        case LinearIntent(tag="operator", operator=(m, b, shape, policy)):
            return _operator_solve(key, m, b, shape, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _dense_solve(key: ContentKey, m: LinearMap, b: np.ndarray, shape: SolveShape) -> "Solve[np.ndarray]":
    a = m.dense_array()
    if shape is not SolveShape.SQUARE or a.shape[0] != a.shape[1]:
        x, residuals, rank, _ = np.linalg.lstsq(a, b, rcond=None)
        residual = float(np.max(residuals)) if residuals.size else float(np.linalg.norm(a @ x - b))
        return Solve.LeastSquares(x, key, residual, int(rank), 0, Provider.GATED)
    try:
        x = sla.solve(a, b, assume_a=m.structure.value)
    except ImportError:
        x = np.linalg.solve(a, b)
    return Solve.Direct(x, key, float(np.linalg.norm(a @ x - b)), _condition(np.linalg.svdvals(a)))


def sparse_solve(key: ContentKey, m: LinearMap, b: np.ndarray, scheme: SparseScheme, policy: LinearPolicy) -> "Solve[np.ndarray]":
    match scheme:
        case SparseScheme(tag="spsolve"):
            x = np.asarray(spla.spsolve(m.matrix(), b))
            return Solve.Direct(x, key, m.residual(x, b))
        case SparseScheme(tag="splu"):
            x = np.asarray(spla.splu(m.matrix()).solve(b))
            return Solve.Direct(x, key, m.residual(x, b))
        case SparseScheme(tag="spilu", spilu=(drop_tol, fill_factor)):
            x = np.asarray(spla.spilu(m.matrix(), drop_tol=drop_tol, fill_factor=fill_factor).solve(b))
            return Solve.Direct(x, key, m.residual(x, b))
        case SparseScheme(tag="factored"):
            x = np.asarray(spla.factorized(m.matrix())(b))
            return Solve.Direct(x, key, m.residual(x, b))
        case SparseScheme(tag="krylov", krylov=(kind,)):
            op = m.scipy_op()
            pre = m.krylov_preconditioner(policy.preconditioner, spla)
            steps: list[int] = []
            extra = {"callback_type": "x"} if kind is KrylovKind.GMRES else {}
            x, info = getattr(spla, kind.value)(op, b, rtol=policy.tol, maxiter=policy.maxiter, M=pre, callback=lambda *_: steps.append(1), **extra)
            return Solve.Iterative(np.asarray(x), key, m.residual(x, b), len(steps), Provider.GATED, policy.tol, result=_info_status(int(info)))
        case SparseScheme(tag="lsqr", lsqr=(conlim,)) | SparseScheme(tag="lsmr", lsmr=(conlim,)):
            x, istop, itn, r1norm, *_ = getattr(spla, scheme.tag)(m.scipy_op(), b, atol=policy.tol, btol=policy.tol, conlim=conlim)
            return Solve.LeastSquares(
                np.asarray(x), key, float(r1norm), 0, int(itn), Provider.GATED,
                result=_ISTOP.try_find(int(istop)).default_value("other"),
            )
        case _ as unreachable:
            assert_never(unreachable)


def _eigen_solve(
    key: ContentKey, m: LinearMap, k: int, mode: SpectralMode, scheme: EigenScheme, sigma: float | None
) -> "Solve[np.ndarray | tuple[np.ndarray, np.ndarray]]":
    match (m, mode):
        case (LinearMap(tag="dense", dense=(a, _)), SpectralMode.SPECTRUM):
            s = np.linalg.svdvals(np.asarray(a))
            return Solve.Eigen(s, key, 0.0, int(s.size), _condition(s))
        case (LinearMap(tag="dense", dense=(a, _)), SpectralMode.EIGENPAIRS):
            dense = np.asarray(a)
            w, v = np.linalg.eigh(dense)
            return Solve.Eigen((w, v), key, float(np.linalg.norm(dense @ v - v * w)), int(w.size), _condition(np.linalg.svdvals(dense)))
        case (_, SpectralMode.SPECTRUM):
            op = m.scipy_op()
            u, s, vt = spla.svds(op, k=k)
            return Solve.Eigen(np.asarray(s), key, float(np.linalg.norm(op @ vt.conj().T - u * s)), int(s.size), _condition(np.asarray(s)))
        case (_, SpectralMode.EIGENPAIRS):
            op = m.scipy_op()
            try:
                match scheme:
                    case EigenScheme.LOBPCG:
                        block = np.linalg.qr(np.random.default_rng(_EIGEN_SEED).standard_normal((op.shape[0], k)))[0]
                        w, v = spla.lobpcg(op, block, largest=False)
                    case EigenScheme.SHIFT_INVERT:
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
                return Solve.Eigen((np.asarray(w), np.asarray(v)), key, partial, int(w.size), result="max_steps_reached")
            return Solve.Eigen((np.asarray(w), np.asarray(v)), key, float(np.linalg.norm(op @ v - v * w)), int(np.asarray(w).size))
        case _ as unreachable:
            assert_never(unreachable)


def _operator_solve(key: ContentKey, m: LinearMap, b: np.ndarray, shape: SolveShape, policy: LinearPolicy) -> "Solve[np.ndarray]":
    e = LinearEngine.gated()
    operator = e.operator(m)
    solver = e.solver(shape, m.structure, spd_free=_spd_free(m), tol=policy.tol, maxiter=policy.maxiter)
    run = lambda op, v: e.lx.linear_solve(op, v, solver, throw=False)
    if b.ndim == 2:
        stack = e.jnp.asarray(b)
        solution = e.eqx.filter_vmap(run, in_axes=(None, 0))(operator, stack)
        per_row = e.eqx.filter_vmap(lambda v, rhs: e.residual(operator, v, rhs), in_axes=(0, 0))(solution.value, stack)
        status, iterations = e.verdict(solution.result), int(np.asarray(solution.stats.get("num_steps", 0)).max())
        residual = float(e.jnp.max(per_row))
    else:
        rhs = e.jnp.asarray(b)
        solution = run(operator, rhs)
        status, iterations = e.verdict(solution.result), int(solution.stats.get("num_steps", 0))
        residual = float(e.residual(operator, solution.value, rhs))
    return (
        Solve.LeastSquares(np.asarray(solution.value), key, residual, 0, iterations, Provider.GATED, result=status)
        if shape is not SolveShape.SQUARE
        else Solve.Direct(np.asarray(solution.value), key, residual, result=status)
    )
```

## [03]-[EXCHANGE]

- Owner: `SparseExchange` — the two-container correspondence with `dotnet:Rasm.Compute/Tensor/factor#SPARSE_SOLVE` `ReadArchive`/`WriteArchive`, hand-copied as a deliberate non-import mirror per estate law: `.mtx` through `scipy.io` is the SuiteSparse interop surface, and the scipy-convention HDF5 group (`indptr`/`indices` int32, `values` float64, `permutation` int32, `shape`/`format` group attributes) carries the `kind`/`ordering`/`fill`/`frobenius`/`symmetric` reproduction metadata `.mtx` drops. Both containers read and write here because the .NET lane landed both directions of each: a reproduction artifact from a failed C# factorization re-factors python-side under its recorded policy, and a python-authored operand crosses back through the same two doors.
- Cases: `ExchangeMeta` types the archive's metadata attributes; `ordering` carries the CSparse `ColumnOrdering` ordinal the producer wrote (`0` Natural, `1` MinimumDegreeAtPlusA, `2` MinimumDegreeStS, `3` MinimumDegreeAtA — decompile-proven declaration order) and `permc` projects it onto the `splu` `permc_spec` vocabulary, `MinimumDegreeStS` landing on `COLAMD` as the closest scipy ordering with the divergence stated here rather than hidden.
- Law: both write legs carry the `composition` custody key and an awaitable twin — `write_mtx_async`/`write_archive_async` over the one `_written` half — because the sync bodies are pure `h5py`/`scipy.io` calls opening no loop while recording suspends. Each twin lands one `OPERATIONAL` `AuditFact` naming the destination beside a `STORAGE` `MeterFact` over the bytes written, through one `_evidence` fold so the two containers cannot drift into two vocabularies for one crossing; the container token rides the diff rather than the verb, since both legs write the same kind of thing to two encodings. `OPERATIONAL` is earned: a sparse operand is a reproduction artifact re-derivable from the factorization that produced it. The record rail BINDS into the verdict, and a refused write records nothing since there is no artifact to name. Reads land no evidence — an exchange plane recording each `read_mtx` prices the read path for rows no reconstruction ever reads.
- Entry: `read_mtx`/`write_mtx` and `read_archive`/`write_archive` are the four legs of one owner, each write answering the container's byte extent; the HDF5 leg is pinned to `/A`, requires every dataset and metadata attribute, and gates explicit little-endian int32 indices/permutation, little-endian float64 values, little-endian int64 shape/scalars, and the PureHDF-compatible uint8 `symmetric` scalar before every inbound leg re-runs `_admit` — extent congruence, monotone pointer run, index bounds, one vectorized finiteness pass — because both routes end at admission exactly as both C# routes end at `Ingest`.
- Auto: `.mtx` writes pin `symmetry="general"` because the pinned .NET peer writer exposes no symmetry parameter and its reader exposes no header metadata; both branches exchange operand values, never a structure hint only one peer can recover. Factor structure and reproduction policy ride the HDF5 sibling.
- Output: reads land a `LinearMap.SparseMat` the `[02]-[LINEAR]` routes consume directly — the exchange is solver currency, never a gridded field, so no gridded-plane page owns any of this.
- Packages: `scipy` (`io.mmread`/`io.mmwrite`/`io.mminfo`, `sparse.csr_array`/`csc_array`, `sparse.linalg.norm`); `h5py` (module-top; `File`, `create_group`/`create_dataset`, attribute IO) under the compute-tier `.api/h5py.md` admission.
- Growth: a new archive attribute is one `ExchangeMeta` field with its wire spelling; a new container format is one read/write leg pair with its awaitable twin over the shared `_written` half and one container token, never a sibling exchange surface; zero new knob on the solve routes.
- Boundary: no ledger, custody, or retention window is minted here — the plane arrives bound at the composition root and this owner declares a `Retain` class alone. The int32 index pin is the exchange law — the C# reader declares int32 dataset reads, so an operand whose `nnz` or pointer length exceeds int32 refuses typed at write rather than emitting a container the peer mis-reads; the group convention layout is the C# fence's law and this mirror re-derives none of it; factorization policy is recorded evidence — python re-factors through its own `SparseScheme` under the projected ordering and never claims byte-identical factors.

```python
import h5py
import scipy.io as sio
import scipy.sparse as sp
from expression import Error, Result
from expression.collections import Block

from rasm.compute.graduation.handoff import EVIDENCE_DOMAIN
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, MeterFact, Party, Resource, Retain
from rasm.runtime.roots import ResourceRef

_INT32_CEIL: Final[int] = 2**31 - 1

_PERMC: Final[Map[int, str]] = Map.of_seq([(0, "NATURAL"), (1, "MMD_AT_PLUS_A"), (2, "COLAMD"), (3, "MMD_ATA")])

def _evidence(container: str, ref: "ResourceRef", written: int) -> "Block[Fact]":
    audited = AuditFact(
        action=f"{EVIDENCE_DOMAIN}.exchange",
        actor=Party(kind=Actor.SERVICE, key=EvidenceScope.LINEAR.value),
        target=Party(kind="artifact", key=str(ref.path)),
        retention=Retain.OPERATIONAL,
        change=(Assigned(path="/container", next=container),),
    )
    return Block.of_seq((audited, MeterFact(resource=Resource.STORAGE, quantity=written, surface=str(ref.path))))


async def _written(container: str, ref: "ResourceRef", settled: "RuntimeRail[int]", composition: ScopeKey) -> "RuntimeRail[int]":
    match settled:
        case Result(tag="ok", ok=written):
            return (await Journal.record(_evidence(container, ref, written), scope=composition)).map(lambda _landed: written)
        case refused:
            return Error(refused.error)


class ExchangeMeta(Struct, frozen=True):
    kind: str
    ordering: int
    fill: int
    frobenius: float
    symmetric: bool

    @property
    def permc(self) -> str:
        return _PERMC.try_find(self.ordering).default_value("COLAMD")


class SparseExchange:
    @staticmethod
    def read_mtx(ref: "ResourceRef") -> "RuntimeRail[LinearMap]":
        def read() -> LinearMap:
            operand = sp.csr_array(sio.mmread(str(ref.path), spmatrix=False))
            return LinearMap.SparseMat(_admit(operand, tuple(map(int, operand.shape))), MatrixStructure.GENERAL)

        return boundary(MTX_READ, read, catch=(ValueError, OSError))

    @staticmethod
    def write_mtx(ref: "ResourceRef", m: LinearMap) -> "RuntimeRail[int]":
        def write() -> int:
            sio.mmwrite(str(ref.path), sp.coo_array(m.matrix()), symmetry="general")
            return ref.path.stat().st_size

        return boundary(MTX_WRITE, write, catch=(ValueError, OSError))

    @staticmethod
    async def write_mtx_async(ref: "ResourceRef", m: LinearMap, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[int]":
        return await _written("mtx", ref, SparseExchange.write_mtx(ref, m), composition)

    @staticmethod
    def read_archive(ref: "ResourceRef") -> "RuntimeRail[tuple[LinearMap, ExchangeMeta, np.ndarray]]":
        def read() -> tuple[LinearMap, ExchangeMeta, np.ndarray]:
            with h5py.File(str(ref.path), "r") as file:
                node = file["A"]
                shape_wire = _wire_array(node.attrs["shape"], "<i8", "A.shape")
                shape = tuple(int(extent) for extent in shape_wire)
                if len(shape) != 2:
                    raise ValueError(f"exchange shape rank: {shape}")
                wire_format = str(node.attrs["format"])
                if wire_format not in ("csr", "csc"):
                    raise ValueError(f"exchange format: {wire_format}")
                ctor = sp.csr_array if wire_format == "csr" else sp.csc_array
                operand = ctor(
                    (
                        _wire_array(node["values"], "<f8", "A/values"),
                        _wire_array(node["indices"], "<i4", "A/indices"),
                        _wire_array(node["indptr"], "<i4", "A/indptr"),
                    ),
                    shape=shape,
                )
                meta = ExchangeMeta(
                    kind=str(node.attrs["kind"]),
                    ordering=int(_wire_scalar(node.attrs["ordering"], "<i8", "A.ordering")),
                    fill=int(_wire_scalar(node.attrs["fill"], "<i8", "A.fill")),
                    frobenius=float(_wire_scalar(node.attrs["frobenius"], "<f8", "A.frobenius")),
                    symmetric=_wire_bool(node.attrs["symmetric"], "A.symmetric"),
                )
                if (
                    meta.kind not in ("spd", "ldl", "lu", "qr")
                    or meta.ordering not in range(4)
                    or meta.fill < 0
                    or not np.isfinite(meta.frobenius)
                    or meta.frobenius < 0.0
                    or meta.symmetric != (meta.kind in ("spd", "ldl"))
                ):
                    raise ValueError(f"exchange metadata: {meta}")
                permutation = _wire_array(node["permutation"], "<i4", "A/permutation")
                if permutation.shape != (shape[1],) or set(map(int, permutation)) != set(range(shape[1])):
                    raise ValueError(f"exchange permutation: {permutation.shape} for {shape}")
            structure = MatrixStructure.SYMMETRIC if meta.symmetric else MatrixStructure.GENERAL
            return LinearMap.SparseMat(_admit(operand, shape), structure), meta, permutation

        return boundary(ARCHIVE_READ, read, catch=(KeyError, OSError, OverflowError, TypeError, ValueError))

    @staticmethod
    def write_archive(ref: "ResourceRef", m: LinearMap, meta: ExchangeMeta, permutation: np.ndarray) -> "RuntimeRail[int]":
        def write() -> int:
            operand = sp.csc_array(m.matrix())
            applied = np.asarray(permutation)
            held_meta = (
                meta.kind in ("spd", "ldl", "lu", "qr")
                and meta.ordering in range(4)
                and meta.fill >= 0
                and np.isfinite(meta.frobenius)
                and meta.frobenius >= 0.0
                and meta.symmetric == (meta.kind in ("spd", "ldl"))
            )
            held_permutation = applied.shape == (operand.shape[1],) and set(map(int, applied)) == set(range(operand.shape[1]))
            _admit(operand, tuple(map(int, operand.shape)))
            if not held_meta or not held_permutation:
                raise ValueError(f"exchange write roster: {meta}; permutation={applied.shape}")
            if max(operand.shape) > _INT32_CEIL or operand.nnz > _INT32_CEIL or operand.indptr.size > _INT32_CEIL:
                raise OverflowError(f"exchange int32 ceiling: shape={operand.shape}; nnz={operand.nnz}")
            with h5py.File(str(ref.path), "x") as file:
                node = file.create_group("A")
                node.create_dataset("indptr", data=operand.indptr.astype("<i4"))
                node.create_dataset("indices", data=operand.indices.astype("<i4"))
                node.create_dataset("values", data=operand.data.astype("<f8"))
                node.create_dataset("permutation", data=applied.astype("<i4"))
                node.attrs["shape"] = np.asarray(operand.shape, dtype="<i8")
                node.attrs["format"] = "csc"
                node.attrs["kind"] = meta.kind
                node.attrs["ordering"] = np.asarray(meta.ordering, dtype="<i8")
                node.attrs["fill"] = np.asarray(meta.fill, dtype="<i8")
                node.attrs["frobenius"] = np.asarray(meta.frobenius, dtype="<f8")
                node.attrs["symmetric"] = np.uint8(meta.symmetric)
            return ref.path.stat().st_size

        return boundary(ARCHIVE_WRITE, write, catch=(OSError, OverflowError, TypeError, ValueError))

    @staticmethod
    async def write_archive_async(
        ref: "ResourceRef", m: LinearMap, meta: ExchangeMeta, permutation: np.ndarray, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[int]":
        return await _written("archive", ref, SparseExchange.write_archive(ref, m, meta, permutation), composition)


def _wire_array(source: object, dtype: str, coordinate: str) -> np.ndarray:
    value = np.asarray(source)
    expected = np.dtype(dtype)
    if (
        value.dtype.kind != expected.kind
        or value.dtype.itemsize != expected.itemsize
        or value.dtype.byteorder not in ("<", "=")
        or (value.dtype.byteorder == "=" and not np.little_endian)
    ):
        raise TypeError(f"exchange dtype {coordinate}: {value.dtype.str} != {dtype}")
    return value


def _wire_scalar(source: object, dtype: str, coordinate: str) -> object:
    value = _wire_array(source, dtype, coordinate)
    if value.ndim != 0:
        raise TypeError(f"exchange scalar {coordinate}: {value.shape}")
    return value.item()


def _wire_bool(source: object, coordinate: str) -> bool:
    value = np.asarray(source)
    if value.ndim != 0 or value.dtype.kind != "u" or value.dtype.itemsize != 1 or int(value.item()) not in (0, 1):
        raise TypeError(f"exchange bool {coordinate}: {value.dtype.str}{value.shape}")
    return bool(value.item())


def _admit(operand: object, shape: tuple[int, int]) -> object:
    if tuple(operand.shape) != shape:
        raise ValueError(f"exchange extents: {operand.shape} != {shape}")
    pointers, indices = operand.indptr, operand.indices
    minor = operand.shape[1] if operand.format == "csr" else operand.shape[0]
    if pointers[0] != 0 or pointers[-1] != operand.nnz or (np.diff(pointers) < 0).any():
        raise ValueError("exchange pointer run: non-monotone indptr")
    if indices.size and (indices.max() >= minor or indices.min() < 0):
        raise ValueError("exchange index bounds")
    if not np.isfinite(operand.data).all():
        raise ValueError("exchange values: non-finite entry")
    return operand
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
