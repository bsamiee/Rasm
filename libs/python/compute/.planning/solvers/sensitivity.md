# [PY_COMPUTE_SENSITIVITY]

The one automatic-differentiation and sensitivity owner. `Differentiation` is the `@tagged_union` whose `DiffModeTag` literal discriminates the full derivative algebra — scalar gradient, forward-mode Jacobian, reverse-mode Jacobian, Hessian, Jacobian-vector product, vector-Jacobian product, Hessian-vector product, and the finite-difference floor — so every differentiation a study, an optimizer, or a graduation gate needs is one mode value on one owner rather than a `grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp`/`hvp` method family. There is no separate autodiff surface beside this one: `Differentiation` IS the AD owner, and the entry is the `differentiate` method on the union. The JAX modes route `jax.value_and_grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp` over a plain array function and the `equinox.filter_*` peers over an Equinox `Module`/PyTree objective whose differentiable leaves are the inexact arrays, so a parametric model and a raw function differentiate through the same owner with the differentiation argument, holomorphic flag (threaded only into the transforms whose `.api` signature carries it), and aux-carrying selected by one orthogonal `DiffPolicy` row rather than a per-mode keyword spray. The finite-difference floor contracts the `findiff` central-difference `coefficients(deriv=1, acc=acc)` `center` stencil over an accuracy-width grid — never a fixed three-point hand-rolled stencil and never an `acc` silently capped — and runs as the gated-band fallback where automatic differentiation is unavailable. Reverse mode reads the implicit-function-theorem adjoint through the autodifferentiable Lineax/Optimistix/Diffrax solves the solver routes expose, so a sensitivity through a solved system differentiates the converged solution rather than the iteration trace. This owner never trains a model, fits a network, or carries a gradient-descent optimizer loop.

## [01]-[INDEX]

- [01]-[SENSITIVITY]: the one `Differentiation` AD owner discriminating eight `DiffModeTag` cases (scalar gradient, forward/reverse Jacobian, Hessian, JVP, VJP, HVP, finite-difference) over JAX, the `equinox.filter_*` PyTree peers, and the gated `findiff` accuracy-scaled finite-difference floor, with the differentiation argument and aux selected by one `DiffPolicy` row, folding one typed `DiffReceipt` carrying the mode, the objective value at the differentiation point, the differentiated product, the exactness verdict, the accuracy order, and the implicit-adjoint witness.

## [02]-[SENSITIVITY]

- Owner: `Differentiation` — the ONE `@tagged_union` AD owner; the `Literal` tag IS the differentiation mode, read directly through `.tag`. The eight cases are the bounded derivative algebra — `Gradient()` over `jax.value_and_grad` (the scalar-objective value-and-gradient pair in one pass), `ForwardJacobian()` over `jax.jacfwd`, `ReverseJacobian()` over `jax.jacrev`, `Hessian()` over `jax.hessian`, `Jvp(tangent)` over `jax.jvp`, `Vjp(cotangent)` over `jax.vjp`, `Hvp(vector)` composing `jax.jvp(jax.grad(fn), (x,), (vector,))` (the Hessian-vector product without materializing the Hessian), and `FiniteDifference(step, acc)` over the `findiff` floor — and the mode decides the engine, the differentiated product, the objective value witness, and the exactness verdict. The single entry `mode.differentiate(fn, x)` is the method on the union — the mode IS the owner — folding every case into one `_dispatch`, never a `value_and_grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp`/`hvp` method family. A new differentiation engine is one `Differentiation` case plus one `_TRANSFORM` row; the scalar `Gradient`, the directional `Jvp`/`Vjp`/`Hvp` projections, and the full Jacobian/Hessian share the one dispatch.
- Policy axis: `DiffPolicy` is the ONE orthogonal differentiation-argument row carried on `differentiate` alongside the mode — `argnums` (the positional argument differentiated), `holomorphic` (complex-holomorphic differentiation), and `has_aux` (the objective returns a `(value, aux)` pair whose aux rides the receipt facts) — so the JAX `argnums`/`holomorphic`/`has_aux` keywords are one policy value threaded through the `_TRANSFORM`-selected transform rather than a per-mode keyword spray or six hardcoded defaults. A study sweeping the differentiation argument sets `DiffPolicy.argnums`, never a second entry.
- Leaf axis: `DiffTarget` is the ONE bounded objective-shape policy carried on `differentiate` — `ARRAY` for a plain `np.ndarray`→`np.ndarray` function differentiated through the bare `jax.*` transforms, and `PYTREE` for an Equinox `Module`/PyTree objective differentiated through the `equinox.filter_*` peers, where `equinox.partition`/`is_inexact_array` split the inexact-array design leaves the transform differentiates from the static rest. The `_TRANSFORM` table is keyed on `(DiffTarget, mode_tag)` and projects each pair to its transform constructor — `(ARRAY, gradient)→jax.value_and_grad`, `(PYTREE, gradient)→equinox.filter_value_and_grad`, `(ARRAY, reverse_jacobian)→jax.jacrev`, `(PYTREE, reverse_jacobian)→equinox.filter_jacrev`, and so through every mode — so the array and the PyTree path are two columns on one table rather than two parallel owners. The PyTree product flattens through `_flatten`, which keeps the `equinox.is_inexact_array` (differentiated) leaves via `equinox.partition` and concatenates their `np.ravel`ed values over the cataloged `jax.tree_util.tree_leaves` surface. The finite-difference floor is `ARRAY`-only (a PyTree leaf has no uniform grid), and a `PYTREE` finite-difference request carries `DiffStatus.UNSUPPORTED` on the receipt rather than raising.
- Implicit-adjoint loop: when the differentiated function is itself a solve, reverse mode reads the adjoint through the solver rather than through the iterations. The Lineax `linear_solve`, the Optimistix `root_find`/`minimise`/`fixed_point`/`least_squares` (each carrying `optimistix.ImplicitAdjoint` by default), and the Diffrax adjoint solve carry implicit-function-theorem adjoints, so `jax.vjp`/`jax.jacrev`/`jax.value_and_grad` over a function that calls them pulls back through the converged solution; `solvers/linear.md#LINEAR`, `solvers/nonlinear.md#NONLINEAR`, and `solvers/differential.md#DIFFERENTIAL` expose those autodifferentiable solves, and this owner consumes the adjoint they carry. `optimization/design.md#DESIGN` reads this owner's `Gradient`/reverse-mode gradient over an inner-solve objective for inverse design, and `experiments/study.md#STUDY` reads this owner's per-axis Jacobian for the derivative-based global-sensitivity (DGSM) screen — both consume the gradient, never re-implement it here.
- Entry: `mode.differentiate(fn, x)` returns `RuntimeRail[DiffReceipt]` carrying the mode, the objective `value` at the differentiation point (present for the value-carrying `Gradient`/`Jvp` modes, `None` for the projection-only modes), the differentiated product summary (the Jacobian/Hessian Frobenius norm, the gradient/JVP/HVP max-component magnitude), the `DiffStatus` verdict (the JAX modes `EXACT`, the finite-difference mode `TRUNCATION_BOUNDED`, a non-finite product `NONFINITE`, an unsupported pairing `UNSUPPORTED`), the realized accuracy order, and the implicit-adjoint witness flag. The autodiff body calls the `_TRANSFORM`-selected transform under the `DiffPolicy` keywords — `holomorphic` threading only into `jax.jacfwd`, the one transform whose `.api` signature carries it, never into `value_and_grad`/`jacrev`/`hessian` which reject it — and reads the product back through `np.asarray` (array target) or `_flatten` (pytree target); the finite-difference body assembles the Jacobian column-by-column from the `findiff` central-difference `coefficients(deriv=1, acc=acc)` `center` stencil weights/offsets contracted over the per-axis sample grid and divided by `step`. `boundary` converts an unexpected host fault into the runtime fault rail; an unsupported mode/target pairing is carried inside the success receipt as `DiffStatus.UNSUPPORTED`, so the two failure notions stay distinct. One `_summary` fold builds the receipt for every mode, keyed on the `DiffStatus` the body resolves — never a parallel inline receipt construction per mode.
- Receipt: `DiffReceipt` is the typed AD receipt — never a generic reported-value abstraction — carrying the mode tag, the objective `value` witness, the differentiated product as a flat `tuple[float, ...]` (the gradient for the scalar/directional modes, the flattened Jacobian/Hessian for the full modes), the `max_magnitude`/`frobenius` product summaries, the `DiffStatus` verdict, the realized `accuracy` order, and the `implicit_adjoint` witness. `DiffReceipt.contribute` emits one `Receipt.of("emitted", ...)` row; the `DiffStatus` verdict, the `accuracy` order, and the finite-difference step are the evidence the C# graduation gate reads through the existing `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION` to admit or reject a sensitivity for kernel handoff. No new handoff axis — the differentiated sensitivity crosses on the `solver` axis already present.
- Packages: `jax` (`value_and_grad`, `grad`, `jacfwd`, `jacrev`, `hessian`, `jvp`, `vjp`, `tree_util.tree_leaves`, `numpy.asarray`), `equinox` (`filter_value_and_grad`, `filter_grad`, `filter_jacfwd`, `filter_jacrev`, `filter_hessian`, `filter_jvp`, `filter_vjp`, `partition`, `is_inexact_array`), `findiff` (`coefficients(deriv, acc=)` — the cataloged raw-coefficient surface whose `center` entry carries the accuracy-scaled central-difference weights and offsets the floor contracts, never a fixed three-point hand-rolled stencil and never `min(acc, 2)`), `optimistix`/`lineax`/`diffrax` (the implicit-adjoint solves consumed transitively through the solver routes, never imported here), `numpy` (`asarray`, `eye`, `ravel`, `stack`, `column_stack`, `tensordot`, `concatenate`, `linalg.norm`, `all`, `isfinite`), `solvers/receipt.md#RECEIPT` (the solver evidence the gradient annotates), `graduation/handoff.md#GRADUATION` (the `solver` axis the sensitivity graduates on), `experiments/study.md#STUDY` (the DGSM consumer reading this Jacobian), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new differentiation engine is one `Differentiation` case plus one `_TRANSFORM` row; a new objective shape is one `DiffTarget` member plus its `_TRANSFORM` column; a new differentiation argument or aux mode is one `DiffPolicy` field threaded through the existing transform call; zero new surface, no parallel autodiff page, no `value_and_grad`/`jacrev`/`hessian`/`jvp`/`vjp`/`grad`/`hvp` method family, no per-mode helper file, no hand-rolled stencil beside the `findiff` coefficient surface.
- Boundary: classical sensitivity and adjoint analysis only — implicit-function-theorem adjoints, scalar gradients, forward/reverse Jacobians, Hessians, directional JVP/VJP/HVP products, and the finite-difference floor are in-scope; this owner never trains a model, fits a network, carries a gradient-descent optimizer loop (that is `optimization/design.md#DESIGN` reading this owner's gradient), or runs a sampled global-sensitivity design (that is `experiments/study.md#STUDY` over SALib, which reads this owner's Jacobian for its DGSM derivative screen). `jax`/`jaxlib`/`equinox` carry no cp315 wheel and `findiff` is gated `python_version<'3.15'` through its `scipy`/`numpy` dependencies, so the JAX modes and the finite-difference floor are both authored against the documented API on the gated companion band; the `findiff`-coefficient central-difference contraction is the reachable floor where JAX is absent. A parallel autodiff owner beside the solver, a `grad`/`jacobian`/`hessian`/`jvp`/`vjp` method family, a per-mode `_*` helper body, a `getattr`-on-tag dispatch in place of the total `match`/`assert_never`, a fixed three-point hand-rolled stencil duplicating `findiff.coefficients`, an `acc` silently capped to 2 discarding the policy's accuracy order, a `numpy.asarray` flattening a JAX PyTree, a boolean `reverse`/`exact` knob where the mode value carries the modality, a per-mode inline receipt construction beside `_summary`, and a generic `IReceipt`/reported-value abstraction discarding the mode/value/exactness/adjoint evidence are the deleted forms.

```python signature
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


# --- [TYPES] -------------------------------------------------------------------------------

type ArrayFn = Callable[[np.ndarray], np.ndarray]
type DiffModeTag = Literal[
    "gradient", "forward_jacobian", "reverse_jacobian", "hessian", "jvp", "vjp", "hvp", "finite_difference"
]


class DiffTarget(StrEnum):
    ARRAY = "array"
    PYTREE = "pytree"


class DiffStatus(StrEnum):
    EXACT = "exact"
    TRUNCATION_BOUNDED = "truncation_bounded"
    UNSUPPORTED = "unsupported"
    NONFINITE = "nonfinite"


# --- [MODELS] ------------------------------------------------------------------------------

@dataclass(frozen=True, slots=True)
class DiffPolicy:
    argnums: int = 0  # the positional argument differentiated, threaded to every jax/equinox transform
    holomorphic: bool = False  # complex-holomorphic differentiation for the analytic modes
    has_aux: bool = False  # objective returns (value, aux); aux rides the receipt facts


@tagged_union(frozen=True)
class Differentiation:
    tag: DiffModeTag = tag()
    gradient: tuple[()] = case()
    forward_jacobian: tuple[()] = case()
    reverse_jacobian: tuple[()] = case()
    hessian: tuple[()] = case()
    jvp: np.ndarray = case()
    vjp: np.ndarray = case()
    hvp: np.ndarray = case()
    finite_difference: tuple[float, int] = case()

    @staticmethod
    def Gradient() -> "Differentiation":
        return Differentiation(gradient=())

    @staticmethod
    def ForwardJacobian() -> "Differentiation":
        return Differentiation(forward_jacobian=())

    @staticmethod
    def ReverseJacobian() -> "Differentiation":
        return Differentiation(reverse_jacobian=())

    @staticmethod
    def Hessian() -> "Differentiation":
        return Differentiation(hessian=())

    @staticmethod
    def Jvp(tangent: np.ndarray) -> "Differentiation":
        return Differentiation(jvp=tangent)

    @staticmethod
    def Vjp(cotangent: np.ndarray) -> "Differentiation":
        return Differentiation(vjp=cotangent)

    @staticmethod
    def Hvp(vector: np.ndarray) -> "Differentiation":
        return Differentiation(hvp=vector)

    @staticmethod
    def FiniteDifference(step: float = 1e-6, acc: int = 2) -> "Differentiation":
        return Differentiation(finite_difference=(step, acc))

    def differentiate(
        self, fn: Callable, x: object, target: DiffTarget = DiffTarget.ARRAY, policy: DiffPolicy = DiffPolicy()
    ) -> "RuntimeRail[DiffReceipt]":
        return boundary(f"diff.{self.tag}", lambda: _dispatch(fn, x, self, target, policy))


class DiffReceipt(Struct, frozen=True):
    mode: DiffModeTag
    target: DiffTarget
    value: float | None
    product: tuple[float, ...]
    max_magnitude: float
    frobenius: float
    accuracy: int
    status: DiffStatus
    implicit_adjoint: bool

    def contribute(self) -> Receipt:
        facts = {
            "mode": self.mode,
            "target": self.target,
            "value": "none" if self.value is None else f"{self.value:.3e}",
            "max_magnitude": f"{self.max_magnitude:.3e}",
            "frobenius": f"{self.frobenius:.3e}",
            "accuracy": str(self.accuracy),
            "status": self.status,
            "implicit_adjoint": str(self.implicit_adjoint),
        }
        return Receipt.of("emitted", "compute.differentiation", self.mode, facts)


# --- [TABLES] ------------------------------------------------------------------------------

# (target, mode) -> transform constructor, deferred behind the gated jax/equinox imports. The array
# column uses the bare jax.* transforms; the pytree column uses the equinox.filter_* peers so an
# Equinox Module objective differentiates only its inexact-array leaves. `gradient` selects the
# value-and-gradient pair; the Jacobian/Hessian columns are the full-derivative transforms.
def _transform(target: DiffTarget, tag: DiffModeTag, jax: object, eqx: object) -> Callable:
    table = {
        (DiffTarget.ARRAY, "gradient"): jax.value_and_grad,
        (DiffTarget.ARRAY, "forward_jacobian"): jax.jacfwd,
        (DiffTarget.ARRAY, "reverse_jacobian"): jax.jacrev,
        (DiffTarget.ARRAY, "hessian"): jax.hessian,
        (DiffTarget.PYTREE, "gradient"): eqx.filter_value_and_grad,
        (DiffTarget.PYTREE, "forward_jacobian"): eqx.filter_jacfwd,
        (DiffTarget.PYTREE, "reverse_jacobian"): eqx.filter_jacrev,
        (DiffTarget.PYTREE, "hessian"): eqx.filter_hessian,
    }
    return table[(target, tag)]


# --- [OPERATIONS] --------------------------------------------------------------------------

def _dispatch(fn: Callable, x: object, mode: Differentiation, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    match mode:
        case Differentiation(tag="finite_difference", finite_difference=(step, acc)):
            return _finite_difference(fn, np.asarray(x), step, acc, target)
        case Differentiation(tag="gradient" | "forward_jacobian" | "reverse_jacobian" | "hessian"):
            return _autodiff(fn, x, mode.tag, target, policy)
        case Differentiation(tag="jvp", jvp=tangent):
            return _jvp(fn, x, np.asarray(tangent), target, policy)
        case Differentiation(tag="vjp", vjp=cotangent):
            return _vjp(fn, x, np.asarray(cotangent), target, policy)
        case Differentiation(tag="hvp", hvp=vector):
            return _hvp(fn, x, np.asarray(vector), target, policy)
        case unreachable:
            assert_never(unreachable)


# One receipt fold for every mode: the product flattens, the status is the caller's verdict refined to
# NONFINITE on a non-finite product, and the optional primal value rides the value-carrying modes.
def _summary(
    product: np.ndarray, mode: DiffModeTag, target: DiffTarget, *,
    status: DiffStatus, accuracy: int, implicit: bool, value: float | None = None,
) -> DiffReceipt:
    flat = np.ravel(np.asarray(product))
    verdict = DiffStatus.NONFINITE if not np.all(np.isfinite(flat)) else status
    return DiffReceipt(
        mode=mode,
        target=target,
        value=value,
        product=tuple(map(float, flat)),
        max_magnitude=float(np.linalg.norm(flat, np.inf)) if flat.size else 0.0,
        frobenius=float(np.linalg.norm(flat)),
        accuracy=accuracy,
        status=status if status is DiffStatus.UNSUPPORTED else verdict,
        implicit_adjoint=implicit,
    )


# `holomorphic` is honored only by the transforms whose `.api` signature carries it — `jax.grad`
# and `jax.jacfwd`; `value_and_grad`, `jacrev`, and `hessian` reject the keyword, so the policy flag
# threads into the forward-Jacobian transform alone and never a transform that would raise on it. The
# pytree product flattens through the cataloged `jax.tree_util` leaf surface, never a non-cataloged
# `flatten_util.ravel_pytree`; `equinox.is_inexact_array` keeps the static leaves out of the summary.
def _autodiff(fn: Callable, x: object, mode: DiffModeTag, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    import equinox as eqx
    import jax

    transform = _transform(target, mode, jax, eqx)
    kw: dict[str, object] = {"argnums": policy.argnums, "has_aux": policy.has_aux}
    if mode == "forward_jacobian":
        kw["holomorphic"] = policy.holomorphic
    primal = jax.numpy.asarray(x) if target is DiffTarget.ARRAY else x
    out = transform(fn, **kw)(primal)
    value, product = (float(out[0]), out[1]) if mode == "gradient" else (None, out)
    leaves = _flatten(product, jax, eqx) if target is DiffTarget.PYTREE else product
    return _summary(np.asarray(leaves), mode, target, status=DiffStatus.EXACT, accuracy=0, implicit=True, value=value)


# One pytree-flattening fold for every pytree product: keep the inexact-array (differentiated) leaves,
# concatenate their ravels into the flat product the receipt records. Routes through the cataloged
# `jax.tree_util` leaf surface and the `equinox.is_inexact_array` leaf predicate, never a bespoke ravel.
def _flatten(tree: object, jax: object, eqx: object) -> np.ndarray:
    inexact, _ = eqx.partition(tree, eqx.is_inexact_array)
    leaves = jax.tree_util.tree_leaves(inexact)
    return np.concatenate([np.ravel(np.asarray(leaf)) for leaf in leaves]) if leaves else np.asarray([])


def _jvp(fn: Callable, x: object, tangent: np.ndarray, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    import equinox as eqx
    import jax

    primal_jvp = eqx.filter_jvp if target is DiffTarget.PYTREE else jax.jvp
    out, out_tangent = primal_jvp(fn, (jax.numpy.asarray(x),), (jax.numpy.asarray(tangent),))
    flat = _flatten(out_tangent, jax, eqx) if target is DiffTarget.PYTREE else np.asarray(out_tangent)
    value = float(np.asarray(out)) if target is DiffTarget.ARRAY and np.asarray(out).size == 1 else None
    return _summary(flat, "jvp", target, status=DiffStatus.EXACT, accuracy=0, implicit=True, value=value)


def _vjp(fn: Callable, x: object, cotangent: np.ndarray, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    import equinox as eqx
    import jax

    primal_vjp = eqx.filter_vjp if target is DiffTarget.PYTREE else jax.vjp
    _, pullback = primal_vjp(fn, jax.numpy.asarray(x))
    (grad,) = pullback(jax.numpy.asarray(cotangent))
    flat = _flatten(grad, jax, eqx) if target is DiffTarget.PYTREE else np.asarray(grad)
    return _summary(flat, "vjp", target, status=DiffStatus.EXACT, accuracy=0, implicit=True)


# Hessian-vector product without materializing the Hessian: the forward-over-reverse composition
# jvp(grad(fn), (x,), (v,)) is the second-order directional derivative jax documents for hvp. The
# composition is ARRAY-only (it scalar-reduces through jax.grad over the array primal); a PYTREE HVP
# carries DiffStatus.UNSUPPORTED on the receipt rather than raising, mirroring the finite-difference floor.
def _hvp(fn: Callable, x: object, vector: np.ndarray, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    import jax

    if target is DiffTarget.PYTREE:
        return _summary(np.asarray([]), "hvp", target, status=DiffStatus.UNSUPPORTED, accuracy=0, implicit=False)
    grad_fn = jax.grad(fn, argnums=policy.argnums, holomorphic=policy.holomorphic)
    _, hv = jax.jvp(grad_fn, (jax.numpy.asarray(x),), (jax.numpy.asarray(vector),))
    return _summary(np.asarray(hv), "hvp", target, status=DiffStatus.EXACT, accuracy=0, implicit=True)


# The finite-difference floor is ARRAY-only — a pytree leaf has no uniform grid. A PYTREE request
# carries DiffStatus.UNSUPPORTED inside the success receipt (a distinct verdict, never a raise), so the
# unsupported-pairing notion stays separate from the host-fault rail `boundary` owns.
def _finite_difference(fn: ArrayFn, x: np.ndarray, step: float, acc: int, target: DiffTarget) -> DiffReceipt:
    if target is DiffTarget.PYTREE:
        return _summary(
            np.asarray([]), "finite_difference", target,
            status=DiffStatus.UNSUPPORTED, accuracy=acc, implicit=False,
        )
    jac = _findiff_jacobian(fn, x, step, acc)
    return _summary(
        np.ravel(jac), "finite_difference", DiffTarget.ARRAY,
        status=DiffStatus.TRUNCATION_BOUNDED, accuracy=acc, implicit=False,
    )


# The Jacobian columns are the central-difference contraction of the findiff `coefficients(deriv=1,
# acc=acc)` stencil — the cataloged raw-coefficient surface — over the per-axis sample grid: the
# stencil weights/offsets honor the requested accuracy order directly, never an `acc` capped to 2 and
# never a fixed three-point hand-rolled stencil. `coefficients` returns the central scheme as the
# `center` entry carrying its `coefficients` weights and `offsets`; the derivative along each axis is
# the weight-contracted finite difference divided by `step`, stacked into the Jacobian columns.
def _findiff_jacobian(fn: ArrayFn, x: np.ndarray, step: float, acc: int) -> np.ndarray:
    from findiff import coefficients

    center = coefficients(deriv=1, acc=acc)["center"]
    weights = np.asarray(center["coefficients"])
    offsets = np.asarray(center["offsets"])
    basis = np.eye(x.size)
    columns = [
        np.tensordot(weights, np.stack([fn(x + o * step * basis[axis]) for o in offsets]), axes=(0, 0)) / step
        for axis in range(x.size)
    ]
    return np.column_stack(columns)
```

## [03]-[RESEARCH]

- [JAX_AD]: the `jax.value_and_grad`/`grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp` and `jax.tree_util.tree_leaves` spellings carry the `python_version<'3.15'` marker (no jaxlib cp315 wheel). The `.api` signatures bound the keyword surface exactly: `value_and_grad(fun, argnums=0, has_aux=False)` returns the `(value, gradient)` pair for a scalar objective in one pass and carries no `holomorphic`; `grad(fun, argnums=0, has_aux=False, holomorphic=False, allow_int=False)` the bare gradient composed under the HVP; `jacfwd(fun, argnums=0, has_aux=False, holomorphic=False)` the forward-mode Jacobian (the one full-Jacobian transform carrying `holomorphic`); `jacrev(fun, argnums=0, has_aux=False, allow_int=False)` the reverse-mode Jacobian, which carries `allow_int` not `holomorphic`; `hessian(fun, argnums=0, has_aux=False)` the second derivative via `jacfwd(jacrev(...))` and likewise no `holomorphic`; `jvp(fun, primals, tangents, has_aux=False)` the value-and-tangent pair (forward directional product); and `vjp(fun, *primals, has_aux=False)` the `(value, pullback)` pair whose pullback applies the cotangent (reverse directional product). The HVP mode is the documented forward-over-reverse composition `jvp(grad(fun), (x,), (v,))`, materializing no Hessian. `argnums`/`has_aux` thread from `DiffPolicy` into every transform; `holomorphic` threads only into `jacfwd` (and the HVP-composed `grad`), never into a transform whose signature rejects it. The pytree product flattens through `jax.tree_util.tree_leaves` over the inexact-array partition, never `jax.flatten_util.ravel_pytree`. All transformed functions must be pure JAX functions; the reverse modes read the implicit-function-theorem adjoint through the autodifferentiable solver routes rather than re-deriving it. The bodies verify against the `.api` catalogue once the jaxlib wheel resolves.
- [EQUINOX_FILTER]: the `equinox.filter_value_and_grad`/`filter_jacfwd`/`filter_jacrev`/`filter_hessian`/`filter_jvp`/`filter_vjp` peers differentiate w.r.t. the inexact-array leaves of an `equinox.Module`/PyTree objective only, leaving static leaves untouched; `equinox.partition`/`combine` split the differentiable design leaves from the static rest and `equinox.is_inexact_array` is the leaf predicate. They require `jax` and carry the same `python_version<'3.15'` marker. The `_TRANSFORM` table makes the `ARRAY` and `PYTREE` paths two columns on one owner: a raw function differentiates through the bare `jax.*` transform, a parametric model through the `filter_*` peer, never two parallel surfaces.
- [FINDIFF_FLOOR]: `findiff` is pure-Python (`py3-none-any`) but gated `python_version<'3.15'` through its `scipy`/`numpy` dependencies. The finite-difference floor reads the cataloged raw-coefficient surface `coefficients(deriv=1, acc=acc)`, whose `center` entry carries the central-difference `coefficients` weights and `offsets` for accuracy order `acc` directly — never an `acc` clamped to 2 and never a fixed three-point hand-rolled stencil. The per-axis Jacobian column is the weight-contraction of `fn` sampled at `x + offset*step*e_axis` over those center offsets, divided by `step`; this is the stencil applied once at the differentiation point rather than constructing and discarding a full `Diff` operator over a padded sample line. (`Diff(axis, grid, periodic, acc, scheme, compact)` and its `operator.matrix(shape)` sparse representation remain the cataloged operator algebra for a full-grid solve, but the point-Jacobian floor needs only the coefficient stencil.) The floor is `ARRAY`-only because a PyTree leaf has no uniform grid; a `PYTREE` finite-difference request carries `DiffStatus.UNSUPPORTED` on the receipt rather than raising.
- [IMPLICIT_ADJOINT]: the implicit-function-theorem adjoint is carried by the Lineax `linear_solve`, the Optimistix `root_find`/`minimise`/`fixed_point`/`least_squares` (default `optimistix.ImplicitAdjoint`), and the Diffrax adjoint solve exposed through `solvers/linear.md#LINEAR`, `solvers/nonlinear.md#NONLINEAR`, and `solvers/differential.md#DIFFERENTIAL`; this owner reads the adjoint through `jax.value_and_grad`/`jax.vjp`/`jax.jacrev` over those solves rather than re-deriving it. `optimization/design.md#DESIGN` is the consumer driving an objective to a stationary point on this owner's `Gradient`/reverse-mode gradient; this owner only differentiates, never optimizes.
- [DGSM_SEAM]: `experiments/study.md#STUDY` owns sampled global sensitivity over SALib — Sobol, Morris, FAST, RBD-FAST, delta, PAWN, and the derivative-based DGSM screen. The DGSM analyzer (`SALib.analyze.dgsm`) consumes per-axis model derivatives; the `ForwardJacobian`/`ReverseJacobian` product this owner emits is exactly that derivative field, so `Study`'s `Dgsm` method reads this Jacobian rather than re-deriving a finite-difference gradient. This is a consumer seam, never a re-implementation: local autodiff sensitivity stays here, sampled-design global sensitivity stays in `Study`, and the two meet at the Jacobian the study reads.
