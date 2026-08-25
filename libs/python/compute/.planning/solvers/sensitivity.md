# [PY_COMPUTE_SENSITIVITY]

`Differentiation` is the one automatic-differentiation and sensitivity owner: the `@tagged_union` whose `DiffModeTag` tag discriminates the full derivative algebra — scalar gradient, forward- and reverse-mode Jacobian, Hessian, Jacobian-vector product, vector-Jacobian product, Hessian-vector product, and the finite-difference floor. Every differentiation a study, optimizer, or graduation gate needs is one mode value on this owner, entered through `differentiate` on the union itself, never a `grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp`/`hvp` method family and never a second autodiff surface beside it. This owner never trains a model, fits a network, or carries a gradient-descent loop.

Every mode resolves through one engine stacking `jax`, `equinox`, and `findiff`: the `_SPEC` table keyed on `(DiffTarget, DiffModeTag)` folds each cell to its transform-or-applicator and admissible keyword set, and the gated `DiffEngine` carrier promotes float64 once per solve. Reverse mode reads the implicit-function-theorem adjoint through the autodifferentiable solves `solvers/linear#LINEAR`, `solvers/nonlinear#NONLINEAR`, and `solvers/differential#DIFFERENTIAL` expose, so a sensitivity through a solved system differentiates the converged solution, not the iteration trace. Sensitivity evidence stays on the span — `Derivative` stamps its `attributes` at the mint, no sensitivity `HandoffAxis` case exists, and a crossing that needs derivative evidence rides the solve routes' own graduation.

## [01]-[INDEX]

- [02]-[SENSITIVITY]: the `Differentiation` AD owner discriminating eight `DiffModeTag` cases over the `_SPEC` `(DiffTarget, DiffModeTag)` table, folding JAX/Equinox at float64 and the `findiff` accuracy-scaled floor into one weave-harvested `Derivative` rail.

## [02]-[SENSITIVITY]

- Owner: `Differentiation` — the one `@tagged_union` AD owner; the `Literal` tag IS the mode, read through `.tag`. Eight `@classmethod` factories return `Self`, binding the subtype once. `differentiate` is the single entry on the union, folding every case into one `_dispatch` — the mode IS the owner, never a per-transform method family. Orthogonal `DiffPolicy` (differentiation argument) and `DiffTarget` (objective shape) rows ride `differentiate` beside the mode.
- Cases: the `_SPEC` table keyed on `(DiffTarget, DiffModeTag)` is the single dispatch owner — `ARRAY` selects the bare `jax.*` transform, `PYTREE` the `equinox.filter_*` peer over an Equinox `Module`, two columns on one table rather than parallel owners. Keyword projection is data: each `DiffSpec.keywords` names exactly the `DiffPolicy` fields the cell's transform admits — `holomorphic` rides `jacfwd` alone, `allow_int` rides `jacrev` alone, and the `filter_*` peers carry `("has_aux",)` alone (they differentiate the first argument's inexact-array leaves and reject integer `argnums`), so no transform receives a keyword it raises on. Directional `jvp`/`vjp`/`hvp` cells are three applicator rows: the `Jvp` tangent and `Hvp` vector live in the input space, the `Vjp` cotangent in the output space, all three carried as one `Pytree` payload the `ARRAY` cells lift through `jnp.asarray` and the `PYTREE` cells thread whole — an `asarray` over a PyTree leaf flattens it, the forbidden form. `has_aux=True` shifts the transform return, peeled by the `_split` fold onto the `Derivative` `value`/`aux` witnesses. Finite-difference floor and HVP are `ARRAY`-only, so the `(PYTREE, finite_difference)` and `(PYTREE, hvp)` cells are absent — that one membership miss IS the `DiffStatus.UNSUPPORTED` verdict, carried on the `Derivative` rather than raised.
- Entry: `mode.differentiate(lane, fn, x)` is `async`, offloading `_dispatch` under the `HOSTILE` trait (the `jax_enable_x64` mutation is process-global), isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing owner, the hub `evidence_run` weave owning span and fence. `_dispatch` resolves the `_SPEC[(target, mode.tag)]` row first — an absent cell is the sole UNSUPPORTED verdict — then a total mode-tag `match` routes the finite-difference short-circuit (before any `gated()` carrier build), the directional rail, or the transform rail, each reading the product back through `DiffEngine.flatten`. Finite-difference floor contracts the `findiff` `coefficients(deriv=1, acc=acc)` `center` stencil over an accuracy-width grid and records the realized order — never a fixed three-point stencil, never an `acc` capped. `boundary` converts an unexpected host fault to the fault rail; the UNSUPPORTED pairing rides inside the `Derivative`, so a fault and an unsupported mode stay distinct notions.
- Output: `DiffProduct` is the output-shape discriminant — `SCALAR` (gradient/directional vector), `JACOBIAN` (2-D), `HESSIAN` (square, carrying the symmetry residual). `_PRODUCT` table keys each mode to its class and `_summary` reads it to resolve the `(rows, cols)` extent and the Hessian `‖H − Hᵀ‖_∞` residual, so the product is parameterized over output shape rather than erased to one norm pair; `_summary` reads it for every mode including the UNSUPPORTED pairing that carries no `_SPEC` row.
- Output: `Derivative` is the typed AD result carrying the mode, shape, the `argnums` differentiated-argument witness (the intrinsic `∂y/∂xᵢ` provenance a reader recovers without the call site, `0` on the single-argument `PYTREE` filter default), the objective `value`, the `aux` flag, the flat product, the `(rows, cols)` extent, the `max_magnitude`/`frobenius` norms, the Hessian `symmetry` residual (`0.0` off the Hessian mode), the `DiffStatus` verdict, the realized `accuracy`, and the `implicit_adjoint` flag — parameterized over both input argument and output shape. `DiffStatus` is a value object with behavior: its `exact` predicate tests the `_EXACT` frozenset, mirroring the sibling `SolveStatus.converged`. Facts ride as native scalars — verdict, extent, residual, accuracy, step — so the span carries the full numeric evidence and no sensitivity `HandoffAxis` case exists.
- Packages: `jax` (the transforms and the rail-wide `config.update("jax_enable_x64", True)` float64 promotion the gated carrier runs so the implicit adjoint pulls back at double precision), `equinox` (the `filter_*` peers, `partition`/`is_inexact_array` splitting the differentiated inexact-array leaves), `findiff` (`coefficients(deriv, acc=)` — the raw central-difference surface whose `center` entry carries the accuracy-scaled weights, offsets, and realized order the floor reads onto the `Derivative`), `numpy`, `expression`, `dataclasses`, and `msgspec` per the fence imports; `optimistix`/`lineax`/`diffrax` supply the implicit adjoints transitively through the solver routes, never imported here.
- Growth: a new analytic mode is one `Differentiation` case, one `_SPEC` row, and one `_PRODUCT` row; a new directional mode is one case, one `_SPEC` applicator, and one `_PRODUCT` row; a new objective shape is one `DiffTarget` member with its `_SPEC` column; a new differentiation argument is one `DiffPolicy` field with its membership on the affected `keywords` tuples; a new product geometry is one `DiffProduct` member with its `_summary` arm.
- Boundary: the implicit-adjoint solves are the solver routes' — `solvers/linear#LINEAR`, `solvers/nonlinear#NONLINEAR`, and `solvers/differential#DIFFERENTIAL` carry the autodifferentiable adjoints (each `optimistix` solve `ImplicitAdjoint` by default, one `lineax` solve per backward pass) this owner consumes, so reverse mode over a function that solves pulls back through the converged solution. `optimization/design#DESIGN` reads this owner's `Gradient`/reverse-mode gradient over an inner-solve objective for inverse design (its `filter_value_and_grad` pass is the `(PYTREE, gradient)` cell). `experiments/study#STUDY` owns a DISJOINT sampled-DGSM rail over `SALib.analyze.dgsm` and never calls this owner — the shared concept is the `∂y/∂x` field, not a wire (the `[DGSM_SEAM]` row carries the boundary).

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, Self, assert_never, cast

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from opentelemetry import trace

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait, Wire

lazy from findiff import coefficients


# --- [TYPES] ----------------------------------------------------------------------------

type ArrayFn = Callable[[np.ndarray], np.ndarray]
type Pytree = object
type DiffModeTag = Literal["gradient", "forward_jacobian", "reverse_jacobian", "hessian", "jvp", "vjp", "hvp", "finite_difference"]
type Applicator = Callable[["DiffEngine", Callable, tuple[Pytree, Pytree], dict[str, object]], tuple[float | None, object]]
type TransformPick = Callable[["DiffEngine"], Callable[..., object]]


class DiffTarget(StrEnum):
    ARRAY = "array"
    PYTREE = "pytree"


class DiffProduct(StrEnum):
    SCALAR = "scalar"
    JACOBIAN = "jacobian"
    HESSIAN = "hessian"


class DiffStatus(StrEnum):
    EXACT = "exact"
    TRUNCATION_BOUNDED = "truncation_bounded"
    UNSUPPORTED = "unsupported"
    NONFINITE = "nonfinite"

    @property
    def exact(self) -> bool:
        return self in _EXACT


# --- [CONSTANTS] ------------------------------------------------------------------------

_EXACT: frozenset[DiffStatus] = frozenset({DiffStatus.EXACT})

_PRODUCT: Map[DiffModeTag, DiffProduct] = Map.of_seq([
    ("gradient", DiffProduct.SCALAR),
    ("forward_jacobian", DiffProduct.JACOBIAN),
    ("reverse_jacobian", DiffProduct.JACOBIAN),
    ("hessian", DiffProduct.HESSIAN),
    ("jvp", DiffProduct.SCALAR),
    ("vjp", DiffProduct.SCALAR),
    ("hvp", DiffProduct.SCALAR),
    ("finite_difference", DiffProduct.JACOBIAN),
])


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class DiffPolicy:
    argnums: int | tuple[int, ...] = 0
    holomorphic: bool = False
    allow_int: bool = False
    has_aux: bool = False

    def projected(self, keywords: tuple[str, ...]) -> dict[str, object]:
        return {name: getattr(self, name) for name in keywords}


@dataclass(frozen=True, slots=True)
class DiffSpec:
    keywords: tuple[str, ...]
    apply: TransformPick | Applicator | None


@tagged_union(frozen=True)
class Differentiation:
    tag: DiffModeTag = tag()
    gradient: tuple[()] = case()
    forward_jacobian: tuple[()] = case()
    reverse_jacobian: tuple[()] = case()
    hessian: tuple[()] = case()
    jvp: Pytree = case()
    vjp: Pytree = case()
    hvp: Pytree = case()
    finite_difference: tuple[float, int] = case()

    @classmethod
    def Gradient(cls) -> Self:
        return cls(gradient=())

    @classmethod
    def ForwardJacobian(cls) -> Self:
        return cls(forward_jacobian=())

    @classmethod
    def ReverseJacobian(cls) -> Self:
        return cls(reverse_jacobian=())

    @classmethod
    def Hessian(cls) -> Self:
        return cls(hessian=())

    @classmethod
    def Jvp(cls, tangent: Pytree) -> Self:
        return cls(jvp=tangent)

    @classmethod
    def Vjp(cls, cotangent: Pytree) -> Self:
        return cls(vjp=cotangent)

    @classmethod
    def Hvp(cls, vector: Pytree) -> Self:
        return cls(hvp=vector)

    @classmethod
    def FiniteDifference(cls, step: float = 1e-6, acc: int = 2) -> Self:
        return cls(finite_difference=(step, acc))

    async def differentiate(
        self, lane: LanePolicy, fn: Callable, x: Pytree, key: ContentKey, target: DiffTarget = DiffTarget.ARRAY,
        policy: DiffPolicy = DiffPolicy(), *, composition: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeRail[Derivative]":
        async def dispatch() -> RuntimeRail[Derivative]:
            return await lane.offload(Kernel.of(_dispatch, KernelTrait.HOSTILE, wire=Wire.SHARED_MEMORY), fn, x, self, target, policy, key)

        facts = {"mode": self.tag, "target": target.value, "aux": policy.has_aux}
        return await evidence_run(EvidenceScope.SENSITIVITY, f"diff.{self.tag}", dispatch, facts=facts, composition=composition)


@dataclass(frozen=True, slots=True)
class DiffEngine:
    jax: object
    eqx: object

    @classmethod
    def gated(cls) -> Self:
        import jax

        jax.config.update("jax_enable_x64", True)

        import equinox as eqx

        return cls(jax=jax, eqx=eqx)

    def flatten(self, product: object, target: DiffTarget, argnums: int | tuple[int, ...] = 0) -> np.ndarray:
        if isinstance(argnums, tuple):
            blocks = [self._leaf(part, target) for part in product]
            return np.concatenate(blocks) if blocks else np.asarray([])
        return self._leaf(product, target)

    def _leaf(self, tree: object, target: DiffTarget) -> np.ndarray:
        if target is DiffTarget.ARRAY:
            return np.asarray(tree)
        inexact, _ = self.eqx.partition(tree, self.eqx.is_inexact_array)
        leaves = self.jax.tree_util.tree_leaves(inexact)
        return np.concatenate([np.ravel(np.asarray(leaf)) for leaf in leaves]) if leaves else np.asarray([])


class Derivative(Struct, frozen=True):
    content_key: ContentKey
    mode: DiffModeTag
    target: DiffTarget
    shape: DiffProduct
    argnums: int | tuple[int, ...]
    value: float | None
    aux: bool
    product: tuple[float, ...]
    rows: int
    cols: int
    max_magnitude: float
    frobenius: float
    symmetry: float
    accuracy: int
    status: DiffStatus
    implicit_adjoint: bool

    @property
    def attributes(self) -> dict[str, str | bool | int | float]:
        return {
            "key": self.content_key.hex,
            "mode": self.mode,
            "target": self.target,
            "shape": self.shape,
            "argnums": str(self.argnums),
            "aux": self.aux,
            "rows": self.rows,
            "cols": self.cols,
            "max_magnitude": self.max_magnitude,
            "frobenius": self.frobenius,
            "symmetry": self.symmetry,
            "accuracy": self.accuracy,
            "exact": self.status.exact,
            "status": self.status,
            "implicit_adjoint": self.implicit_adjoint,
            **({"value": self.value} if self.value is not None else {}),
        }

    def _noted(self) -> "Derivative":
        trace.get_current_span().set_attributes(self.attributes)
        return self


# --- [TABLES] ---------------------------------------------------------------------------

_SPEC: Map[tuple[DiffTarget, DiffModeTag], DiffSpec] = Map.of_seq([
    ((DiffTarget.ARRAY, "gradient"), DiffSpec(("argnums", "has_aux"), lambda e: e.jax.value_and_grad)),
    ((DiffTarget.ARRAY, "forward_jacobian"), DiffSpec(("argnums", "has_aux", "holomorphic"), lambda e: e.jax.jacfwd)),
    ((DiffTarget.ARRAY, "reverse_jacobian"), DiffSpec(("argnums", "has_aux", "allow_int"), lambda e: e.jax.jacrev)),
    ((DiffTarget.ARRAY, "hessian"), DiffSpec(("argnums", "has_aux"), lambda e: e.jax.hessian)),
    ((DiffTarget.PYTREE, "gradient"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_value_and_grad)),
    ((DiffTarget.PYTREE, "forward_jacobian"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_jacfwd)),
    ((DiffTarget.PYTREE, "reverse_jacobian"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_jacrev)),
    ((DiffTarget.PYTREE, "hessian"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_hessian)),
    (
        (DiffTarget.ARRAY, "jvp"),
        DiffSpec((), lambda e, fn, v, kw: _read_array_jvp(e.jax.jvp(fn, (e.jax.numpy.asarray(v[0]),), (e.jax.numpy.asarray(v[1]),)))),
    ),
    ((DiffTarget.PYTREE, "jvp"), DiffSpec((), lambda e, fn, v, kw: (None, e.eqx.filter_jvp(fn, (v[0],), (v[1],))[1]))),
    ((DiffTarget.ARRAY, "vjp"), DiffSpec((), lambda e, fn, v, kw: (None, e.jax.vjp(fn, e.jax.numpy.asarray(v[0]))[1](e.jax.numpy.asarray(v[1]))[0]))),
    ((DiffTarget.PYTREE, "vjp"), DiffSpec((), lambda e, fn, v, kw: (None, e.eqx.filter_vjp(fn, v[0])[1](v[1])[0]))),
    (
        (DiffTarget.ARRAY, "hvp"),
        DiffSpec(
            ("argnums", "holomorphic", "allow_int"),
            lambda e, fn, v, kw: (None, e.jax.jvp(e.jax.grad(fn, **kw), (e.jax.numpy.asarray(v[0]),), (e.jax.numpy.asarray(v[1]),))[1]),
        ),
    ),
    ((DiffTarget.ARRAY, "finite_difference"), DiffSpec((), None)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dispatch(fn: Callable, x: Pytree, mode: Differentiation, target: DiffTarget, policy: DiffPolicy, key: ContentKey) -> Derivative:
    if (spec := _SPEC.try_find((target, mode.tag)).to_optional()) is None:
        return _summary(key, np.asarray([]), mode.tag, target, status=DiffStatus.UNSUPPORTED, accuracy=0, implicit=False)
    match mode:
        case Differentiation(tag="finite_difference", finite_difference=(step, acc)):
            return _finite_difference(key, fn, x, step, acc)
        case Differentiation(tag="jvp", jvp=tangent) | Differentiation(tag="vjp", vjp=tangent) | Differentiation(tag="hvp", hvp=tangent):
            return _directional(key, DiffEngine.gated(), spec, fn, (x, tangent), mode.tag, target, policy)
        case Differentiation(tag="gradient" | "forward_jacobian" | "reverse_jacobian" | "hessian"):
            return _transformed(key, DiffEngine.gated(), spec, fn, x, mode.tag, target, policy)
        case _ as unreachable:
            assert_never(unreachable)


def _summary(
    key: ContentKey,
    product: np.ndarray,
    mode: DiffModeTag,
    target: DiffTarget,
    *,
    status: DiffStatus,
    accuracy: int,
    implicit: bool,
    argnums: int | tuple[int, ...] = 0,
    value: float | None = None,
    aux: object = None,
) -> Derivative:
    arr = np.asarray(product, dtype=float)
    flat = np.ravel(arr)
    shape = _PRODUCT[mode]
    verdict = DiffStatus.NONFINITE if not np.all(np.isfinite(flat)) else status
    matrix = arr if arr.ndim >= 2 else flat.reshape(1, -1)
    symmetry = float(np.linalg.norm(matrix - matrix.T, np.inf)) if shape is DiffProduct.HESSIAN and matrix.shape[0] == matrix.shape[1] else 0.0
    return Derivative(
        content_key=key,
        mode=mode,
        target=target,
        shape=shape,
        argnums=argnums,
        value=value,
        aux=aux is not None,
        product=tuple(map(float, flat)),
        rows=int(matrix.shape[0]),
        cols=int(matrix.shape[1]),
        max_magnitude=float(np.linalg.norm(flat, np.inf)) if flat.size else 0.0,
        frobenius=float(np.linalg.norm(flat)),
        symmetry=symmetry,
        accuracy=accuracy,
        status=status if status is DiffStatus.UNSUPPORTED else verdict,
        implicit_adjoint=implicit,
    )._noted()


def _transformed(
    key: ContentKey, engine: DiffEngine, spec: DiffSpec, fn: Callable, x: Pytree, mode: DiffModeTag, target: DiffTarget, policy: DiffPolicy
) -> Derivative:
    transform = cast(TransformPick, spec.apply)(engine)
    primal = engine.jax.numpy.asarray(x) if target is DiffTarget.ARRAY else x
    out = transform(fn, **policy.projected(spec.keywords))(primal)
    value, product, aux = _split(out, mode, policy.has_aux)
    return _summary(
        key,
        engine.flatten(product, target, policy.argnums),
        mode,
        target,
        status=DiffStatus.EXACT,
        accuracy=0,
        implicit=True,
        argnums=policy.argnums,
        value=value,
        aux=aux,
    )


def _split(out: object, mode: DiffModeTag, has_aux: bool) -> tuple[float | None, object, object]:
    match (mode == "gradient", has_aux):
        case (True, True):
            (value, aux), product = out
            return float(np.asarray(value)), product, aux
        case (True, False):
            value, product = out
            return float(np.asarray(value)), product, None
        case (False, True):
            product, aux = out
            return None, product, aux
        case _:
            return None, out, None


def _directional(
    key: ContentKey, engine: DiffEngine, spec: DiffSpec, fn: Callable, primals: tuple[Pytree, Pytree], mode: DiffModeTag,
    target: DiffTarget, policy: DiffPolicy,
) -> Derivative:
    value, product = cast(Applicator, spec.apply)(engine, fn, primals, policy.projected(spec.keywords))
    return _summary(key, engine.flatten(product, target), mode, target, status=DiffStatus.EXACT, accuracy=0, implicit=True, value=value)


def _read_array_jvp(out_tangent: tuple[object, object]) -> tuple[float | None, object]:
    out, tangent = out_tangent
    value = float(np.asarray(out)) if np.asarray(out).size == 1 else None
    return value, tangent


def _finite_difference(key: ContentKey, fn: ArrayFn, x: object, step: float, acc: int) -> Derivative:
    point = np.asarray(x)
    center = coefficients(deriv=1, acc=acc)["center"]
    weights, offsets = np.asarray(center["coefficients"]), np.asarray(center["offsets"])
    basis = np.eye(point.size)
    columns = [
        np.ravel(np.tensordot(weights, np.stack([fn(point + o * step * basis[axis]) for o in offsets]), axes=(0, 0)) / step)
        for axis in range(point.size)
    ]
    jac = np.stack(columns, axis=1)
    return _summary(
        key, jac, "finite_difference", DiffTarget.ARRAY, status=DiffStatus.TRUNCATION_BOUNDED, accuracy=int(center["accuracy"]),
        implicit=False,
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
