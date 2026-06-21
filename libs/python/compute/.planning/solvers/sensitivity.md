# [PY_COMPUTE_SENSITIVITY]

`Differentiation` is the one automatic-differentiation and sensitivity owner: the `@tagged_union` whose `DiffModeTag` literal discriminates the full derivative algebra — scalar gradient, forward-mode Jacobian, reverse-mode Jacobian, Hessian, Jacobian-vector product, vector-Jacobian product, Hessian-vector product, and the finite-difference floor. Every differentiation a study, an optimizer, or a graduation gate needs is one mode value on this one owner rather than a `grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp`/`hvp` method family, and the entry is the `differentiate` method on the union itself — there is no separate autodiff surface beside it.

The eight modes resolve through one engine that stacks four admitted libraries as a single rail rather than four flat per-library uses. The frozen `DiffEngine` value object carries the gated `jax`/`equinox` modules with the array/pytree product-read built ONCE per solve behind the rail-wide `jax.config.update("jax_enable_x64", True)` float64 promotion; the `_TRANSFORM`, `_DIRECTIONAL`, `_KEYWORDS`, and `_PRODUCT` data tables fold the `(DiffTarget, DiffModeTag)` pair to its transform constructor or applicator, its admissible keyword set, and its output-shape class; the `_split` fold peels each transform's `has_aux` return shift; and one `_summary` fold mints the shape-aware receipt. The JAX modes route `jax.value_and_grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp` over a plain array function while the `equinox.filter_*` peers route over an Equinox `Module`/PyTree objective whose differentiable leaves are the inexact arrays, the directional tangent riding through as a `Pytree` for the `PYTREE` target rather than an `np.ndarray` flatten — so a parametric model and a raw function differentiate through the same owner selected by one orthogonal `DiffTarget`/`DiffPolicy` row pair.

The finite-difference floor contracts the `findiff` central-difference `coefficients(deriv=1, acc=acc)` `center` stencil over an accuracy-width grid — never a fixed three-point hand-rolled stencil and never an `acc` silently capped — and runs as the gated-band fallback where automatic differentiation is unavailable. Reverse mode reads the implicit-function-theorem adjoint through the autodifferentiable Lineax/Optimistix/Diffrax solves the solver routes expose at float64, so a sensitivity through a solved system differentiates the converged solution rather than the iteration trace.

The typed `DiffReceipt` rides the runtime `ReceiptContributor` port: `contribute` yields the one-element `Iterable[Receipt]` the port declares through the two-argument `Receipt.of(owner, evidence)` contract, and the measured `_dispatch` kernel wears the `@receipted(_REDACTION)` aspect so the receipt stream emits on exit without an inline `Signals.emit`, the differentiated sensitivity graduating on the existing `solver` `HandoffAxis` without a parallel evidence rail. This owner never trains a model, fits a network, or carries a gradient-descent optimizer loop.

## [01]-[INDEX]

- [01]-[SENSITIVITY]: the one `Differentiation` AD owner discriminating eight `DiffModeTag` cases (scalar gradient, forward/reverse Jacobian, Hessian, JVP, VJP, HVP, finite-difference) over the `DiffEngine` carrier folding JAX at float64, the `equinox.filter_*` PyTree peers, and the gated `findiff` accuracy-scaled finite-difference floor as one `@receipted` rail, with the differentiation argument and aux selected by one `DiffPolicy` row, the `(DiffTarget, DiffModeTag)`-keyed `_TRANSFORM`/`_DIRECTIONAL` tables owning the transform-and-applicator dispatch, the `_PRODUCT` table keying each mode to its output-shape class, folding one typed `DiffReceipt` riding the `ReceiptContributor` port and carrying the mode, the `DiffProduct` shape with its `(rows, cols)` extent and Hessian symmetry residual, the differentiated-argument `argnums` witness, the objective value at the differentiation point, the differentiated product, the exactness verdict, the realized accuracy order, and the implicit-adjoint witness.

## [02]-[SENSITIVITY]

- Owner: `Differentiation` — the ONE `@tagged_union` AD owner; the `Literal` tag IS the differentiation mode, read directly through `.tag`. The eight cases are the bounded derivative algebra — `Gradient()` over `jax.value_and_grad` (the scalar-objective value-and-gradient pair in one pass), `ForwardJacobian()` over `jax.jacfwd`, `ReverseJacobian()` over `jax.jacrev`, `Hessian()` over `jax.hessian`, `Jvp(tangent)` over `jax.jvp`, `Vjp(cotangent)` over `jax.vjp`, `Hvp(vector)` composing `jax.jvp(jax.grad(fn), (x,), (vector,))` (the Hessian-vector product without materializing the Hessian), and `FiniteDifference(step, acc)` over the `findiff` floor — and the mode decides the engine, the differentiated product, the objective value witness, and the exactness verdict. The eight `@classmethod` factories returning `Self` are the canonical tagged-union constructors binding the subtype once, never a `@staticmethod`-plus-`"Differentiation"`-forward-reference re-spelled eight times. The single entry `mode.differentiate(fn, x)` is the method on the union — the mode IS the owner — folding every case into one `_dispatch`, never a `value_and_grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp`/`hvp` method family. A new differentiation engine is one `Differentiation` case plus one `_TRANSFORM` row; the scalar `Gradient`, the directional `Jvp`/`Vjp`/`Hvp` projections, and the full Jacobian/Hessian share the one dispatch.
- Engine carrier: `DiffEngine` is the ONE frozen value object folding the gated `jax`/`equinox` modules so the five former operation bodies (`_autodiff`/`_jvp`/`_vjp`/`_hvp`/`_finite_difference`) collapse into two table-keyed rails — the repeated `import jax`/`import equinox` pair, the repeated array/pytree product-read ternary, and the repeated `DiffStatus.EXACT, accuracy=0, implicit=True` summary tail live once on the carrier's `flatten` and the two rail bodies rather than three-to-five times across sibling helper functions. `DiffEngine.gated()` imports `jax`/`equinox` once behind the gated band, runs the rail-wide `jax.config.update("jax_enable_x64", True)` float64 promotion (a reverse-mode adjoint pulled back through a `lineax`/`optimistix`/`diffrax` solve assumes float64, and the x32 default silently degrades the implicit-function-theorem gradient), and returns the populated carrier; `_dispatch` builds it ONCE and threads the single carrier into both rails rather than a second `gated()` call per rail. The `_transformed` rail folds `_TRANSFORM[(target, mode.tag)]` against it and the `_directional` rail folds `_DIRECTIONAL[(target, mode.tag)]`, so the JAX flow (mode → transform/applicator → `flatten` → `_summary`) is one integrated rail rather than a flat per-library call. The finite-difference mode bypasses the carrier through the `findiff` floor, the one mode reachable on cp315 where `jax` is absent.
- Policy axis: `DiffPolicy` is the ONE orthogonal differentiation-argument row carried on `differentiate` alongside the mode — `argnums` (the positional argument differentiated), `holomorphic` (complex-holomorphic differentiation), `allow_int` (integer-input differentiation), and `has_aux` (the objective returns a `(value, aux)` pair). The keyword projection is data, not a per-mode spray: the `_KEYWORDS` table names, per `DiffModeTag`, exactly the `DiffPolicy` fields the matching transform's `.api` signature accepts — `value_and_grad(fun, argnums, has_aux)` takes `argnums`/`has_aux` and rejects `holomorphic`; `jacfwd(fun, argnums, has_aux, holomorphic)` is the one full-Jacobian transform carrying `holomorphic`; `jacrev(fun, argnums, has_aux, allow_int)` carries `allow_int` not `holomorphic`; `hessian(fun, argnums, has_aux)` carries neither — so each transform receives only the keywords it accepts, never a `holomorphic=` that `value_and_grad`/`jacrev`/`hessian` raise on. `has_aux=True` shifts the transform return shape, so the `_split` fold peels `value_and_grad`'s `((value, aux), grad)` and the Jacobian/Hessian transforms' `(product, aux)` against the `(mode-carries-value, has_aux)` axis pair and lands the witness on the receipt's `aux` flag; the `equinox.filter_*` peers carry `has_aux` ALONE (no integer `argnums`/`holomorphic`/`allow_int`) and differentiate by the first argument's array-leaf `filter_spec`, so the `PYTREE` column projects only `has_aux` and never threads `argnums` the filter transform would reject. The directional `jvp`/`vjp`/`hvp` cells hold `has_aux=False` by construction, since a `(value, aux)` head shifts the directional return tuple the cell unpacks. A study sweeping the differentiation argument sets `DiffPolicy.argnums`, never a second entry.
- Leaf axis: `DiffTarget` is the ONE bounded objective-shape policy carried on `differentiate` — `ARRAY` for a plain `np.ndarray`→`np.ndarray` function differentiated through the bare `jax.*` transforms, and `PYTREE` for an Equinox `Module`/PyTree objective differentiated through the `equinox.filter_*` peers, where `equinox.partition`/`is_inexact_array` split the inexact-array design leaves the transform differentiates from the static rest. The `_TRANSFORM` table is keyed on `(DiffTarget, DiffModeTag)` and is the SINGLE dispatch owner for both the transform modes and the directional modes: the analytic-transform rows project the pair to its constructor — `(ARRAY, gradient)→jax.value_and_grad`, `(PYTREE, gradient)→equinox.filter_value_and_grad`, `(ARRAY, reverse_jacobian)→jax.jacrev`, `(PYTREE, reverse_jacobian)→equinox.filter_jacrev` — while the directional rows project to the applicator closure that calls `jax.jvp`/`eqx.filter_jvp`, `jax.vjp`/`eqx.filter_vjp`, and the forward-over-reverse `jax.jvp(jax.grad(fn), ...)` HVP composition, so JVP/VJP/HVP are three table rows rather than three sibling helper bodies. The array and the PyTree path are two columns on each table rather than two parallel owners. The directional input is parameterized over the target shape: the `Jvp`/`Vjp`/`Hvp` case payload is a `Pytree`, so the `ARRAY` applicator cells lift the primal and tangent through `jax.numpy.asarray` while the `PYTREE` cells thread the structured tangent straight into `eqx.filter_jvp`/`eqx.filter_vjp` — never a `jax.numpy.asarray` over a PyTree tangent, which flattens a non-array leaf. The PyTree product flattens through `DiffEngine.flatten`, which keeps the `equinox.is_inexact_array` (differentiated) leaves via `equinox.partition` and concatenates their `np.ravel`ed values over the cataloged `jax.tree_util.tree_leaves` surface (the array branch a bare `np.asarray`). The finite-difference floor and the HVP composition are `ARRAY`-only (a PyTree leaf has no uniform grid; the HVP scalar-reduces through `jax.grad` over the array primal), and a `PYTREE` request for either carries `DiffStatus.UNSUPPORTED` on the receipt rather than raising.
- Implicit-adjoint loop: when the differentiated function is itself a solve, reverse mode reads the adjoint through the solver rather than through the iterations. The Lineax `linear_solve`, the Optimistix `root_find`/`minimise`/`fixed_point`/`least_squares` (each carrying `optimistix.ImplicitAdjoint` by default, one `lineax` linear solve per backward pass), and the Diffrax adjoint solve carry implicit-function-theorem adjoints, so `jax.vjp`/`jax.jacrev`/`jax.value_and_grad` over a function that calls them pulls back through the converged solution; `solvers/linear.md#LINEAR`, `solvers/nonlinear.md#NONLINEAR`, and `solvers/differential.md#DIFFERENTIAL` expose those autodifferentiable solves, and this owner consumes the adjoint they carry. `optimization/design.md#DESIGN` reads this owner's `Gradient`/reverse-mode gradient over an inner-solve objective for inverse design, and `experiments/study.md#STUDY` reads this owner's per-axis Jacobian for the derivative-based global-sensitivity (DGSM) screen — both consume the gradient, never re-implement it here.
- Entry: `mode.differentiate(fn, x)` fences the `@receipted(_REDACTION)`-decorated `_dispatch` kernel in one `boundary(f"diff.{self.tag}", ...)` and returns `RuntimeRail[DiffReceipt]` carrying the mode, the objective `value` at the differentiation point (the `Gradient` scalar value-and-gradient pair, and the singleton-output `ARRAY` `Jvp` primal; `None` for the `Vjp`/`Hvp` projections, the vector-output `Jvp`, and the matrix-valued Jacobian/Hessian modes), the differentiated product summary (the Jacobian/Hessian Frobenius norm, the gradient/JVP/HVP max-component magnitude), the `DiffStatus` verdict (the JAX modes `EXACT`, the finite-difference mode `TRUNCATION_BOUNDED`, a non-finite product `NONFINITE`, an unsupported pairing `UNSUPPORTED`), the realized accuracy order, and the implicit-adjoint witness flag. The aspect harvests the `DiffReceipt.contribute` stream on exit, so receipt production is a decorator rail rather than an inline `Signals.emit` — matching every sibling solver route. `_dispatch` builds one `DiffEngine.gated()` carrier for the JAX modes (the finite-difference floor takes none), folds the `_TRANSFORM`-selected transform (the gradient/Jacobian/Hessian rail) or the `_DIRECTIONAL`-selected applicator (the JVP/VJP/HVP rail) under the `_KEYWORDS`-projected `DiffPolicy` keywords, peels the transform return through the `_split` fold so `has_aux`'s `(value, aux)`/`(product, aux)` shape shift lands on the receipt's `value`/`aux` witnesses rather than crashing the `float(out[0])` read, and reads the product back through `DiffEngine.flatten` (array target a bare `np.asarray`, pytree target the `equinox.partition`/`jax.tree_util.tree_leaves` leaf concatenation); the finite-difference body assembles the Jacobian column-by-column from the `findiff` central-difference `coefficients(deriv=1, acc=acc)` `center` stencil weights/offsets contracted over the per-axis sample grid and divided by `step`. `boundary` converts an unexpected host fault into the runtime fault rail; an unsupported mode/target pairing (a `PYTREE` finite-difference or HVP, whose `_DIRECTIONAL` cell is absent) is carried inside the success receipt as `DiffStatus.UNSUPPORTED`, so the two failure notions stay distinct. One `_summary` fold builds the receipt for every mode, keyed on both the `DiffStatus` the body resolves and the `_PRODUCT[mode]` output-shape class that reshapes the flat product to its `(rows, cols)` extent and mints the Hessian symmetry residual — never a parallel inline receipt construction per mode.
- Receipt: `DiffReceipt` is the typed AD receipt — never a generic reported-value abstraction — carrying the mode tag, the `DiffProduct` output-shape class, the `argnums` differentiated-argument witness (the index a study sweeping `DiffPolicy.argnums` reads back to attribute each per-axis receipt to its argument), the objective `value` witness, the `aux` has_aux witness flag, the differentiated product as a flat `tuple[float, ...]` (the gradient for the scalar/directional modes, the flattened Jacobian/Hessian for the full modes), the `(rows, cols)` matrix extent the shape resolves, the `max_magnitude`/`frobenius` product summaries, the Hessian `symmetry` residual `‖H - Hᵀ‖_∞` (`0.0` off the Hessian mode), the `DiffStatus` verdict, the realized `accuracy` order, and the `implicit_adjoint` witness — so the product is parameterized over both input argument and output shape, not erased to one norm pair. `DiffStatus` is a value object with behavior: its `exact` predicate tests membership in the `_EXACT` `frozenset` so the autodiff-vs-truncation witness lives once on the vocabulary, mirroring the sibling `SolveStatus.converged`. `DiffReceipt.contribute` implements the runtime `ReceiptContributor` port — it returns the one-element `Iterable[Receipt]` the `contribute(self) -> Iterable[Receipt]` port declares (the same one-element-tuple shape the sibling `solvers/receipt.md#RECEIPT` `SolverReceipt.contribute` yields), minting through the two-argument `Receipt.of(owner, evidence)` contract as `Receipt.of("compute.differentiation", ("emitted", self.mode, facts))`, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes; the kernel that produces it wears `@receipted(_REDACTION)` while `contribute` itself stays undecorated, exactly as the runtime owner declares. The facts ride as native `float`/`int`/`bool`/`DiffProduct`/`DiffStatus` through the runtime `Signals` `msgspec` `Encoder(enc_hook=repr, order="deterministic")` rather than a `str()` coerce, so the `DiffStatus` verdict, the `exact` flag, the `(rows, cols)` extent, the `symmetry` residual, the `accuracy` order, and the finite-difference step reach the C# graduation gate as numeric evidence through the existing `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION`. No new handoff axis — the differentiated sensitivity crosses on the `solver` axis already present.
- Packages: `jax` (`value_and_grad`, `grad`, `jacfwd`, `jacrev`, `hessian`, `jvp`, `vjp`, `tree_util.tree_leaves`, `numpy.asarray`, `config.update("jax_enable_x64", True)` — the rail-wide float64 promotion the gated carrier runs so the implicit adjoint pulls back at double precision), `equinox` (`filter_value_and_grad`, `filter_grad`, `filter_jacfwd`, `filter_jacrev`, `filter_hessian`, `filter_jvp`, `filter_vjp`, `partition`, `is_inexact_array`), `findiff` (`coefficients(deriv, acc=)` — the cataloged raw-coefficient surface whose `center` entry carries the accuracy-scaled central-difference `coefficients` weights, `offsets`, and the realized `accuracy` order the floor reads onto the receipt, never a fixed three-point hand-rolled stencil and never `min(acc, 2)`), `optimistix`/`lineax`/`diffrax` (the implicit-adjoint solves consumed transitively through the solver routes, never imported here), `numpy` (`asarray`, `eye`, `ravel`, `stack`, `column_stack`, `tensordot`, `concatenate`, `atleast_2d`, `linalg.norm`, `all`, `isfinite`), `expression` (`tag`, `case`, `tagged_union` for the `Differentiation` union; `expression.collections.Map` for the empty `Redaction.classified` policy), `dataclasses` (`dataclass(frozen=True, slots=True)` for the `DiffPolicy`/`DiffEngine` value objects), `msgspec` (`Struct` for the `DiffReceipt` record), `beartype` (`FrozenDict` for the `_TRANSFORM`/`_DIRECTIONAL`/`_KEYWORDS`/`_PRODUCT` tables, matching the sibling solver routes), `graduation/handoff.md#GRADUATION` (the `solver` axis the sensitivity graduates on), `experiments/study.md#STUDY` (the DGSM consumer reading this Jacobian), runtime (`RuntimeRail`, `boundary`, the `Receipt`/`ReceiptContributor` port satisfied structurally, the `Redaction` empty-`classified` policy and the `@receipted` aspect the `_dispatch` kernel wears, plus `Signals` whose `msgspec` encoder carries the receipt's native scalars).
- Growth: a new analytic-transform mode is one `Differentiation` case plus one `_TRANSFORM` row plus one `_PRODUCT` row naming its output shape, plus, when its `.api` keyword surface differs, one `_KEYWORDS` row; a new directional mode is one `Differentiation` case plus one `_DIRECTIONAL` applicator row plus one `_PRODUCT` row; a new objective shape is one `DiffTarget` member plus its `_TRANSFORM`/`_DIRECTIONAL` column; a new differentiation argument or aux mode is one `DiffPolicy` field plus its `_KEYWORDS` membership; a new product geometry (a sparse or block-structured Jacobian summary) is one `DiffProduct` member plus its `_summary` reshape arm; zero new surface, no parallel autodiff page, no `value_and_grad`/`jacrev`/`hessian`/`jvp`/`vjp`/`grad`/`hvp` method family, no per-mode helper file, no hand-rolled stencil beside the `findiff` coefficient surface.
- Boundary: classical sensitivity and adjoint analysis only — implicit-function-theorem adjoints, scalar gradients, forward/reverse Jacobians, Hessians, directional JVP/VJP/HVP products, and the finite-difference floor are in-scope; this owner never trains a model, fits a network, carries a gradient-descent optimizer loop (that is `optimization/design.md#DESIGN` reading this owner's gradient), or runs a sampled global-sensitivity design (that is `experiments/study.md#STUDY` over SALib, which reads this owner's Jacobian for its DGSM derivative screen). `jax`/`jaxlib`/`equinox` carry no cp315 wheel and `findiff` is gated `python_version<'3.15'` through its `scipy`/`numpy` dependencies, so the JAX modes and the finite-difference floor are both authored against the documented API on the gated companion band; the `findiff`-coefficient central-difference contraction is the reachable floor where JAX is absent. A parallel autodiff owner beside the solver, a `grad`/`jacobian`/`hessian`/`jvp`/`vjp` method family, a per-mode `_*` helper body, a `getattr`-on-tag dispatch in place of the total `match`/`assert_never`, an unconditional `argnums=`/`holomorphic=` keyword spray onto a transform whose `.api` signature rejects it (the `equinox.filter_*` peers carry `has_aux` alone; `value_and_grad`/`jacrev`/`hessian` reject `holomorphic`), a `@staticmethod`-plus-`"Differentiation"`-forward-ref factory where the `@classmethod`-plus-`Self` form binds the subtype, a fixed three-point hand-rolled stencil duplicating `findiff.coefficients`, an `acc` silently capped to 2 discarding the policy's accuracy order, a `numpy.asarray` flattening a JAX PyTree, a `jax.numpy.asarray` over a PYTREE directional tangent where the structured tree threads straight into `eqx.filter_jvp`/`eqx.filter_vjp`, a gated solve left on the JAX default float32 where the `jax.config.update("jax_enable_x64", True)` floor belongs and the reverse-mode adjoint is silently degraded below double precision, a second `DiffEngine.gated()` per rail where `_dispatch` builds one carrier threaded into both, an inline `Signals.emit` in the dispatch body where the `@receipted(_REDACTION)` aspect owns egress, a flat-`tuple` product erasing the Jacobian/Hessian `(rows, cols)` extent and the Hessian symmetry residual the `DiffProduct` shape carries, a boolean `reverse`/`exact` knob where the mode value carries the modality, a `float(out[0])` value read that crashes on the `((value, aux), grad)` shape `value_and_grad` returns under `has_aux` where `_split` peels the triple, a per-mode inline receipt construction beside `_summary`, a `contribute` returning one bare `Receipt` against the `Iterable[Receipt]` port the siblings yield a one-element tuple for, a four-positional `Receipt.of("emitted", owner, subject, facts)` call against the runtime two-argument `of(owner, evidence)` contract, a `str()`-coerced fact dict where the runtime encoder carries native scalars, and a generic `IReceipt`/reported-value abstraction discarding the mode/value/exactness/shape/adjoint evidence are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from enum import StrEnum
from typing import Literal, Self, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted


# --- [TYPES] -------------------------------------------------------------------------------

# A plain array objective is `np.ndarray -> np.ndarray`; a PyTree objective is `Pytree -> Pytree`
# (an `equinox.Module`/structured tree). `Pytree` stays `object` because the directional tangent and
# the differentiation point are tracked through the transform as arbitrary pytrees — never narrowed to
# `np.ndarray`, which would flatten a structured leaf, the same vocabulary the sibling solver routes carry.
type ArrayFn = Callable[[np.ndarray], np.ndarray]
type Pytree = object
type DiffModeTag = Literal[
    "gradient", "forward_jacobian", "reverse_jacobian", "hessian", "jvp", "vjp", "hvp", "finite_difference"
]
# A directional applicator reads the gated carrier, the `(primal, tangent)` pytree pair, and the policy,
# returning the (value, product) pair the summary folds — the JVP/VJP/HVP modes as table cells.
type Applicator = Callable[["DiffEngine", Callable, tuple[Pytree, Pytree], "DiffPolicy"], tuple[float | None, object]]
# A transform constructor selector reads the gated jax/equinox modules off the carrier and returns the
# `jax.*`/`equinox.filter_*` transform the gradient/Jacobian/Hessian modes call under projected keywords.
type TransformPick = Callable[["DiffEngine"], Callable[..., object]]


class DiffTarget(StrEnum):
    ARRAY = "array"
    PYTREE = "pytree"


# The product geometry the mode emits — the OUTPUT-shape discriminant the receipt folds: a `SCALAR`
# gradient/directional vector, a 2-D `JACOBIAN`, or a square `HESSIAN`. The summary keys its shape-aware
# facts (rows/cols, the Hessian symmetry residual) off this rather than collapsing every mode to one
# flat Frobenius+max, so the differentiated product is parameterized over output shape, not erased to a tuple.
class DiffProduct(StrEnum):
    SCALAR = "scalar"  # gradient / JVP / VJP / HVP — a flat vector, no matrix structure
    JACOBIAN = "jacobian"  # forward/reverse Jacobian / finite-difference — an (out, in) matrix
    HESSIAN = "hessian"  # second derivative — a square (in, in) matrix carrying a symmetry residual


# The exactness vocabulary, a value object with behavior mirroring the sibling `SolveStatus.converged`:
# `exact` tests membership in `_EXACT` so a consumer reads the autodiff-vs-truncation witness once on the
# vocabulary rather than re-spelling `status is DiffStatus.EXACT` at every call site.
class DiffStatus(StrEnum):
    EXACT = "exact"
    TRUNCATION_BOUNDED = "truncation_bounded"
    UNSUPPORTED = "unsupported"
    NONFINITE = "nonfinite"

    @property
    def exact(self) -> bool:
        return self in _EXACT


# --- [CONSTANTS] ---------------------------------------------------------------------------

# The lone exact-derivative class the `DiffStatus.exact` predicate folds: the JAX/Equinox autodiff modes
# carry machine-exact derivatives, the finite-difference floor carries `TRUNCATION_BOUNDED`. The membership
# lives once on the vocabulary so every consumer reads the same value-object behavior.
_EXACT: frozenset[DiffStatus] = frozenset({DiffStatus.EXACT})

# Field-redaction policy the `@receipted` aspect binds; the AD facts carry no secret, so the classification
# `Map` is empty and every fact reaches the line natively — the one policy object the aspect threads, never
# a per-call construction, exactly as the sibling solver routes bind it.
_REDACTION: Redaction = Redaction(classified=Map.empty())

# Per-mode keyword admission: each `DiffModeTag` names exactly the `DiffPolicy` fields the matching
# transform's `.api` signature accepts, so the projection threads `holomorphic` only into `jacfwd`,
# `allow_int` only into `jacrev`, and `has_aux`/`argnums` where the bare jax transforms carry them —
# never a `holomorphic=` onto `value_and_grad`/`jacrev`/`hessian`, which raise on it. The PYTREE column
# overrides every analytic mode to `("has_aux",)` because the `equinox.filter_*` peers differentiate by
# the first argument's array-leaf filter and accept `has_aux` alone (no integer `argnums`/`holomorphic`).
_KEYWORDS: FrozenDict[DiffModeTag, tuple[str, ...]] = FrozenDict(
    {
        "gradient": ("argnums", "has_aux"),
        "forward_jacobian": ("argnums", "has_aux", "holomorphic"),
        "reverse_jacobian": ("argnums", "has_aux", "allow_int"),
        "hessian": ("argnums", "has_aux"),
        # the inner `jax.grad` of the forward-over-reverse HVP composition carries the full grad keyword
        # surface (`argnums`/`holomorphic`/`allow_int`) but never `has_aux`: a (grad, aux) pair would
        # break the outer `jax.jvp` composition, so the HVP grad differentiates the bare scalar objective.
        "hvp": ("argnums", "holomorphic", "allow_int"),
    }
)

# Per-mode OUTPUT-shape class: the differentiated product geometry the `_summary` fold reads to mint
# shape-aware facts. The scalar/directional modes carry a flat vector, the full-Jacobian and the
# finite-difference floor a 2-D matrix, the Hessian a square matrix carrying its symmetry residual. A new
# mode names its product class in one row, never an inline `if mode == "hessian"` reshape in the summary.
_PRODUCT: FrozenDict[DiffModeTag, DiffProduct] = FrozenDict(
    {
        "gradient": DiffProduct.SCALAR,
        "forward_jacobian": DiffProduct.JACOBIAN,
        "reverse_jacobian": DiffProduct.JACOBIAN,
        "hessian": DiffProduct.HESSIAN,
        "jvp": DiffProduct.SCALAR,
        "vjp": DiffProduct.SCALAR,
        "hvp": DiffProduct.SCALAR,
        "finite_difference": DiffProduct.JACOBIAN,
    }
)


# --- [MODELS] ------------------------------------------------------------------------------

@dataclass(frozen=True, slots=True)
class DiffPolicy:
    argnums: int = 0  # positional argument differentiated; threaded to the bare jax transforms only
    holomorphic: bool = False  # complex-holomorphic differentiation; jax.grad/jacfwd alone carry it
    allow_int: bool = False  # integer-input differentiation; jax.jacrev/grad alone carry it
    has_aux: bool = False  # objective returns (value, aux); _split peels the shifted return, aux flag rides the receipt

    # The PYTREE column collapses to `has_aux` alone (the `equinox.filter_*` peers reject integer
    # `argnums`/`holomorphic`/`allow_int`); the ARRAY column reads `_KEYWORDS` total over the keyed
    # modes and defaults the directional `jvp`/`vjp` and the `finite_difference` floor — which never
    # project keywords — to the empty set, so `projected` stays total over `DiffModeTag` rather than
    # raising `KeyError` on an unkeyed mode were a future cell to read it.
    def projected(self, mode: DiffModeTag, target: DiffTarget) -> dict[str, object]:
        names = ("has_aux",) if target is DiffTarget.PYTREE else _KEYWORDS.get(mode, ())
        return {name: getattr(self, name) for name in names}


# The directional cases carry a `Pytree` tangent/cotangent/vector, not an `np.ndarray`: a PYTREE-target
# JVP/VJP rides a structured tangent the transform tracks per-leaf, so narrowing the payload to `np.ndarray`
# would force a flatten the boundary forbids. The ARRAY target lifts its tangent through `jnp.asarray` at the
# applicator cell; the PYTREE target keeps the tangent a tree.
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

    # The mode IS the owner: one entry method fences the `@receipted`-decorated `_dispatch` kernel in one
    # `boundary`, so a host fault rails and the harvested `DiffReceipt` stream emits on the same exit —
    # never an inline `Signals.emit`, matching the sibling solver routes' decorator-rail egress.
    def differentiate(
        self, fn: Callable, x: Pytree, target: DiffTarget = DiffTarget.ARRAY, policy: DiffPolicy = DiffPolicy()
    ) -> "RuntimeRail[DiffReceipt]":
        return boundary(f"diff.{self.tag}", lambda: _dispatch(fn, x, self, target, policy))


# The gated jax/equinox modules folded into one value object with behavior built ONCE per solve: the
# former five operation bodies read `engine.jax`/`engine.eqx` off the carrier rather than each repeating
# `import jax`/`import equinox`, so the gated import and the float64 promotion fire once and both the
# transform rail and the directional rail thread the same carrier. `gated()` runs the rail-wide
# `jax.config.update("jax_enable_x64", True)` precondition the JAX siblings hold — a reverse-mode adjoint
# pulled back through a `lineax`/`optimistix`/`diffrax` solve assumes float64, and the x32 default silently
# degrades the implicit-function-theorem gradient. `flatten` owns the array/pytree product-read fork once
# for every mode through the per-leaf `tree_util.tree_leaves` over the inexact-array partition, never a bare
# `numpy.asarray` over a structured pytree.
@dataclass(frozen=True, slots=True)
class DiffEngine:
    jax: object
    eqx: object

    @classmethod
    def gated(cls) -> Self:
        import equinox as eqx
        import jax

        jax.config.update("jax_enable_x64", True)
        return cls(jax=jax, eqx=eqx)

    def flatten(self, tree: object, target: DiffTarget) -> np.ndarray:
        if target is DiffTarget.ARRAY:
            return np.asarray(tree)
        inexact, _ = self.eqx.partition(tree, self.eqx.is_inexact_array)
        leaves = self.jax.tree_util.tree_leaves(inexact)
        return np.concatenate([np.ravel(np.asarray(leaf)) for leaf in leaves]) if leaves else np.asarray([])


# The typed AD receipt — never a generic reported-value abstraction. Beyond the flat `product`, it carries
# the OUTPUT-shape evidence the `_PRODUCT` class resolves: `shape` is the product geometry, `rows`/`cols` the
# matrix extent (`(1, n)` for a scalar/directional vector, `(out, in)` for a Jacobian, `(n, n)` for a
# Hessian), `symmetry` the Hessian off-diagonal residual `‖H - Hᵀ‖_∞` (`0.0` for the non-Hessian modes), and
# `aux` the has_aux witness flag (the objective returned a `(value, aux)` pair the transform peeled), so the
# differentiated product is parameterized over output shape rather than collapsed to one norm pair.
class DiffReceipt(Struct, frozen=True):
    mode: DiffModeTag
    target: DiffTarget
    shape: DiffProduct
    argnums: int  # the differentiated positional argument; the witness a study sweeping `DiffPolicy.argnums` reads back to attribute each per-axis Jacobian receipt to its argument (the `PYTREE` filter modes report 0, the first-arg leaf filter)
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

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "mode": self.mode,
            "target": self.target,
            "shape": self.shape,
            "argnums": self.argnums,
            "value": self.value,
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
        }
        return (Receipt.of("compute.differentiation", ("emitted", self.mode, facts)),)


# --- [TABLES] ------------------------------------------------------------------------------

# (target, mode) -> transform-constructor selector, read off the gated carrier. The ARRAY column uses
# the bare jax.* transforms; the PYTREE column uses the equinox.filter_* peers so an Equinox Module
# objective differentiates only its inexact-array leaves. `gradient` selects the value-and-gradient
# pair; the Jacobian/Hessian columns are the full-derivative transforms.
_TRANSFORM: FrozenDict[tuple[DiffTarget, DiffModeTag], TransformPick] = FrozenDict(
    {
        (DiffTarget.ARRAY, "gradient"): lambda e: e.jax.value_and_grad,
        (DiffTarget.ARRAY, "forward_jacobian"): lambda e: e.jax.jacfwd,
        (DiffTarget.ARRAY, "reverse_jacobian"): lambda e: e.jax.jacrev,
        (DiffTarget.ARRAY, "hessian"): lambda e: e.jax.hessian,
        (DiffTarget.PYTREE, "gradient"): lambda e: e.eqx.filter_value_and_grad,
        (DiffTarget.PYTREE, "forward_jacobian"): lambda e: e.eqx.filter_jacfwd,
        (DiffTarget.PYTREE, "reverse_jacobian"): lambda e: e.eqx.filter_jacrev,
        (DiffTarget.PYTREE, "hessian"): lambda e: e.eqx.filter_hessian,
    }
)


# (target, mode) -> directional applicator, the JVP/VJP/HVP modes as table cells rather than three
# sibling helper bodies. The ARRAY rows lift the primal and tangent through `jnp.asarray` and call the bare
# jax directional transforms; the PYTREE rows keep the primal and tangent as structured trees and call the
# equinox.filter_* peers — a `jnp.asarray` over a PYTREE tangent would flatten a non-array leaf, the
# forbidden form, so only the ARRAY cells adopt and the PYTREE cells thread the tree straight through. The
# HVP composition jvp(grad(fn), (x,), (v,)) is the second-order directional derivative without materializing
# the Hessian — ARRAY-only, since it scalar-reduces through jax.grad over the array primal, so the PYTREE
# cell is absent and the dispatch returns UNSUPPORTED. The directional cells hold has_aux=False by
# construction: a (value, aux) head would shift the jvp/vjp return tuple the cell unpacks, so directional aux
# is out of scope and rides only the value-and-grad/Jacobian transform rail through `_split`.
_DIRECTIONAL: FrozenDict[tuple[DiffTarget, DiffModeTag], Applicator] = FrozenDict(
    {
        (DiffTarget.ARRAY, "jvp"): lambda e, fn, v, p: _read_array_jvp(
            e.jax.jvp(fn, (e.jax.numpy.asarray(v[0]),), (e.jax.numpy.asarray(v[1]),))
        ),
        (DiffTarget.PYTREE, "jvp"): lambda e, fn, v, p: (None, e.eqx.filter_jvp(fn, (v[0],), (v[1],))[1]),
        (DiffTarget.ARRAY, "vjp"): lambda e, fn, v, p: (None, e.jax.vjp(fn, e.jax.numpy.asarray(v[0]))[1](e.jax.numpy.asarray(v[1]))[0]),
        (DiffTarget.PYTREE, "vjp"): lambda e, fn, v, p: (None, e.eqx.filter_vjp(fn, v[0])[1](v[1])[0]),
        (DiffTarget.ARRAY, "hvp"): lambda e, fn, v, p: (
            None,
            e.jax.jvp(e.jax.grad(fn, **p.projected("hvp", DiffTarget.ARRAY)), (e.jax.numpy.asarray(v[0]),), (e.jax.numpy.asarray(v[1]),))[1],
        ),
    }
)


# --- [OPERATIONS] --------------------------------------------------------------------------

# `@receipted(_REDACTION)` wraps the measured dispatch and emits its `DiffReceipt.contribute` stream on
# exit; the entry `differentiate` fences it in one `boundary` so the aspect fires on the contributor while a
# jax/equinox/findiff raise still rails. The aspect decorates this kernel — never the receipt's own
# `contribute` — matching the runtime owner's "wraps a ReceiptContributor-returning op". The JAX modes build
# ONE `DiffEngine` (the gated import + float64 promotion fire once) threaded into both rails; the
# finite-difference floor takes none — it is the one mode reachable on cp315 where jax is absent. The
# directional tangent rides through as a pytree (`mode.<tag>` is the carried `Pytree`), never `np.asarray`-d.
@receipted(_REDACTION)
def _dispatch(fn: Callable, x: Pytree, mode: Differentiation, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    match mode:
        case Differentiation(tag="finite_difference", finite_difference=(step, acc)):
            return _finite_difference(fn, x, step, acc, target)
        case Differentiation(tag="gradient" | "forward_jacobian" | "reverse_jacobian" | "hessian"):
            return _transformed(DiffEngine.gated(), fn, x, mode.tag, target, policy)
        case (
            Differentiation(tag="jvp", jvp=tangent)
            | Differentiation(tag="vjp", vjp=tangent)
            | Differentiation(tag="hvp", hvp=tangent)
        ):
            return _directional(fn, x, mode.tag, tangent, target, policy)
        case unreachable:
            assert_never(unreachable)


# One receipt fold for every mode, keyed on the OUTPUT-shape class: the product flattens, the status is the
# caller's verdict refined to NONFINITE on a non-finite product, the optional primal value rides the
# value-carrying modes, and the `_PRODUCT[mode]` geometry resolves the matrix extent and the Hessian
# symmetry residual — `(out, in)` for a JACOBIAN reshaped from the flat product, `(n, n)` for a HESSIAN
# whose off-diagonal residual `‖H - Hᵀ‖_∞` is the symmetry witness, `(1, n)` for a SCALAR/directional
# vector. The shape-aware extent is parameterized over output, not erased to one norm pair.
def _summary(
    product: np.ndarray, mode: DiffModeTag, target: DiffTarget, *,
    status: DiffStatus, accuracy: int, implicit: bool, argnums: int = 0, value: float | None = None, aux: object = None,
) -> DiffReceipt:
    flat = np.ravel(np.asarray(product, dtype=float))
    shape = _PRODUCT[mode]
    verdict = DiffStatus.NONFINITE if not np.all(np.isfinite(flat)) else status
    matrix = np.atleast_2d(np.asarray(product, dtype=float)) if shape is not DiffProduct.SCALAR else flat.reshape(1, -1)
    symmetry = float(np.linalg.norm(matrix - matrix.T, np.inf)) if shape is DiffProduct.HESSIAN and matrix.shape[0] == matrix.shape[1] else 0.0
    return DiffReceipt(
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
    )


# The gradient/Jacobian/Hessian rail over the prebuilt carrier: pick the transform off it, call it under the
# keywords the mode's `.api` signature admits (the PYTREE column projecting `has_aux` alone), read the
# product back through the carrier's flatten — the ARRAY branch keeps the 2-D Jacobian/Hessian extent through
# `np.asarray` so `_summary` recovers `(rows, cols)`, the PYTREE branch concatenates the inexact leaves to a
# flat vector. The keyword projection — not a per-mode `if` — keeps `holomorphic`/`allow_int` off a transform
# that rejects them, and `_split` peels the value/aux/product triple off the transform's has_aux return shape.
def _transformed(engine: DiffEngine, fn: Callable, x: Pytree, mode: DiffModeTag, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    transform = _TRANSFORM[(target, mode)](engine)
    primal = engine.jax.numpy.asarray(x) if target is DiffTarget.ARRAY else x
    out = transform(fn, **policy.projected(mode, target))(primal)
    # value_and_grad returns `((value, aux), grad)` under has_aux else `(value, grad)`; the Jacobian/Hessian
    # transforms return `(product, aux)` under has_aux else the bare product. One `_split` fold peels the
    # `(value, product, aux)` triple off the mode and the policy's `has_aux` flag rather than per-mode reshaping.
    value, product, aux = _split(out, mode, policy.has_aux)
    return _summary(engine.flatten(product, target), mode, target, status=DiffStatus.EXACT, accuracy=0, implicit=True, argnums=policy.argnums, value=value, aux=aux)


# The transform-return peel: a `gradient` head is the scalar value (or its `(value, aux)` pair under has_aux),
# the rest is the gradient; a Jacobian/Hessian head is the bare product (or its `(product, aux)` pair). One
# fold over the `(mode-carries-value, has_aux)` axes rather than a per-mode reshape inside `_transformed`.
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


# The JVP/VJP/HVP rail: the `(target, mode)` applicator off `_DIRECTIONAL` returns the (value, product)
# pair; an absent cell (the PYTREE HVP, which scalar-reduces through jax.grad over an array primal)
# carries DiffStatus.UNSUPPORTED on the receipt rather than raising, mirroring the finite-difference
# floor so the unsupported-pairing notion stays separate from the host-fault rail `boundary` owns. The
# tangent stays a pytree for the PYTREE target; the engine is built here because the UNSUPPORTED cell
# short-circuits before any gated import is needed.
def _directional(fn: Callable, x: Pytree, mode: DiffModeTag, tangent: Pytree, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    applicator = _DIRECTIONAL.get((target, mode))
    if applicator is None:
        return _summary(np.asarray([]), mode, target, status=DiffStatus.UNSUPPORTED, accuracy=0, implicit=False)
    engine = DiffEngine.gated()
    value, product = applicator(engine, fn, (x, tangent), policy)
    return _summary(engine.flatten(product, target), mode, target, status=DiffStatus.EXACT, accuracy=0, implicit=True, argnums=policy.argnums, value=value)


# `jax.jvp` returns the (primal_out, tangent_out) pair over an array objective; the scalar primal rides
# the receipt value when the output is a singleton, the tangent is the directional product. The PYTREE
# JVP keeps `value=None` because a pytree primal output has no single scalar witness.
def _read_array_jvp(out_tangent: tuple[object, object]) -> tuple[float | None, object]:
    out, tangent = out_tangent
    value = float(np.asarray(out)) if np.asarray(out).size == 1 else None
    return value, tangent


# The finite-difference floor is ARRAY-only — a pytree leaf has no uniform grid. A PYTREE request
# carries DiffStatus.UNSUPPORTED inside the success receipt (a distinct verdict, never a raise), so the
# unsupported-pairing notion stays separate from the host-fault rail `boundary` owns. The one mode
# reachable on cp315 where jax is absent: the findiff coefficient surface needs no jaxlib backend.
def _finite_difference(fn: ArrayFn, x: object, step: float, acc: int, target: DiffTarget) -> DiffReceipt:
    if target is DiffTarget.PYTREE:
        return _summary(
            np.asarray([]), "finite_difference", target,
            status=DiffStatus.UNSUPPORTED, accuracy=acc, implicit=False,
        )
    jac, realized = _findiff_jacobian(fn, np.asarray(x), step, acc)
    return _summary(
        jac, "finite_difference", DiffTarget.ARRAY,
        status=DiffStatus.TRUNCATION_BOUNDED, accuracy=realized, implicit=False,
    )


# The Jacobian columns are the central-difference contraction of the findiff `coefficients(deriv=1,
# acc=acc)` stencil — the cataloged raw-coefficient surface — over the per-axis sample grid: the
# stencil weights/offsets honor the requested accuracy order directly, never an `acc` capped to 2 and
# never a fixed three-point hand-rolled stencil. `coefficients` returns the central scheme as the
# `center` entry carrying its `coefficients` weights, `offsets`, and an `accuracy` entry; the receipt
# records that realized order (an odd `acc` request the central scheme rounds to even reaches the
# receipt truthfully) rather than the requested `acc`. The derivative along each axis is the
# weight-contracted finite difference divided by `step`, stacked into the Jacobian columns.
def _findiff_jacobian(fn: ArrayFn, x: np.ndarray, step: float, acc: int) -> tuple[np.ndarray, int]:
    from findiff import coefficients

    center = coefficients(deriv=1, acc=acc)["center"]
    weights = np.asarray(center["coefficients"])
    offsets = np.asarray(center["offsets"])
    basis = np.eye(x.size)
    columns = [
        np.tensordot(weights, np.stack([fn(x + o * step * basis[axis]) for o in offsets]), axes=(0, 0)) / step
        for axis in range(x.size)
    ]
    return np.column_stack(columns), int(center["accuracy"])
```

## [03]-[RESEARCH]

- [JAX_AD]: the `jax.value_and_grad`/`grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp` and `jax.tree_util.tree_leaves` spellings carry the `python_version<'3.15'` marker (no jaxlib cp315 wheel). The `.api` signatures bound the keyword surface exactly: `value_and_grad(fun, argnums=0, has_aux=False)` returns the `(value, gradient)` pair for a scalar objective in one pass and carries no `holomorphic`; `grad(fun, argnums=0, has_aux=False, holomorphic=False, allow_int=False)` the bare gradient composed under the HVP; `jacfwd(fun, argnums=0, has_aux=False, holomorphic=False)` the forward-mode Jacobian (the one full-Jacobian transform carrying `holomorphic`); `jacrev(fun, argnums=0, has_aux=False, allow_int=False)` the reverse-mode Jacobian, which carries `allow_int` not `holomorphic`; `hessian(fun, argnums=0, has_aux=False)` the second derivative via `jacfwd(jacrev(...))` and likewise no `holomorphic`; `jvp(fun, primals, tangents, has_aux=False)` the value-and-tangent pair (forward directional product); and `vjp(fun, *primals, has_aux=False)` the `(value, pullback)` pair whose pullback applies the cotangent (reverse directional product). The HVP mode is the documented forward-over-reverse composition `jvp(grad(fun), (x,), (v,))`, materializing no Hessian, with the inner `grad` carrying the full `argnums`/`holomorphic`/`allow_int` surface but never `has_aux` (a `(grad, aux)` pair breaks the outer `jvp`). The keyword surface is the data-driven `_KEYWORDS` table, one row per mode naming exactly the `DiffPolicy` fields that mode's transform admits — `gradient`→`argnums`/`has_aux`, `forward_jacobian`→`+holomorphic`, `reverse_jacobian`→`+allow_int`, `hessian`→`argnums`/`has_aux`, `hvp`→`argnums`/`holomorphic`/`allow_int` — so the projection threads `holomorphic` into `jacfwd` and the HVP `grad` alone and never onto `value_and_grad`/`jacrev`/`hessian`, which raise on it, rather than a per-mode `if mode == ...` keyword patch. The pytree product flattens through `jax.tree_util.tree_leaves` over the inexact-array partition, never `jax.flatten_util.ravel_pytree`. All transformed functions must be pure JAX functions; the reverse modes read the implicit-function-theorem adjoint through the autodifferentiable solver routes rather than re-deriving it. The bodies verify against the `.api` catalogue once the jaxlib wheel resolves.
- [EQUINOX_FILTER]: the `equinox.filter_value_and_grad`/`filter_jacfwd`/`filter_jacrev`/`filter_hessian`/`filter_jvp`/`filter_vjp` peers differentiate w.r.t. the inexact-array leaves of an `equinox.Module`/PyTree objective only, leaving static leaves untouched; `equinox.partition`/`combine` split the differentiable design leaves from the static rest and `equinox.is_inexact_array` is the leaf predicate. The `.api` confirms these peers differentiate by the first argument's array-leaf `filter_spec` and accept `has_aux` ALONE — `filter_value_and_grad(fun, *, has_aux=False)`, `filter_jacfwd(fun, *, has_aux=False)`, and so through the family carry no integer `argnums`/`holomorphic`/`allow_int` — so the `PYTREE` column of `DiffPolicy.projected` collapses to `("has_aux",)` and never threads an `argnums` the filter transform would reject. They require `jax` and carry the same `python_version<'3.15'` marker. The `_TRANSFORM` table makes the `ARRAY` and `PYTREE` analytic paths two columns on one owner and the `_DIRECTIONAL` table makes the JVP/VJP/HVP paths two more (the array `jax.jvp`/`jax.vjp` and the pytree `filter_jvp`/`filter_vjp` as applicator cells, the HVP composition array-only): a raw function differentiates through the bare `jax.*` transform, a parametric model through the `filter_*` peer, never two parallel surfaces and never a per-mode helper body.
- [FINDIFF_FLOOR]: `findiff` is pure-Python (`py3-none-any`) but gated `python_version<'3.15'` through its `scipy`/`numpy` dependencies. The finite-difference floor reads the cataloged raw-coefficient surface `coefficients(deriv=1, acc=acc)`, whose `center` entry carries the central-difference `coefficients` weights, `offsets`, and the realized `accuracy` order for the requested `acc` — never an `acc` clamped to 2 and never a fixed three-point hand-rolled stencil. The receipt's `accuracy` field reads `center["accuracy"]` rather than echoing the requested `acc`, so a central-scheme rounding (an odd request the symmetric stencil realizes at the next even order) reaches the receipt as the order actually delivered. The per-axis Jacobian column is the weight-contraction of `fn` sampled at `x + offset*step*e_axis` over those center offsets, divided by `step`; this is the stencil applied once at the differentiation point rather than constructing and discarding a full `Diff` operator over a padded sample line. (`Diff(axis, grid, periodic, acc, scheme, compact)` and its `operator.matrix(shape)` sparse representation remain the cataloged operator algebra for a full-grid solve, but the point-Jacobian floor needs only the coefficient stencil.) The floor is `ARRAY`-only because a PyTree leaf has no uniform grid; a `PYTREE` finite-difference request carries `DiffStatus.UNSUPPORTED` on the receipt rather than raising.
- [IMPLICIT_ADJOINT]: the implicit-function-theorem adjoint is carried by the Lineax `linear_solve`, the Optimistix `root_find`/`minimise`/`fixed_point`/`least_squares` (default `optimistix.ImplicitAdjoint`), and the Diffrax adjoint solve exposed through `solvers/linear.md#LINEAR`, `solvers/nonlinear.md#NONLINEAR`, and `solvers/differential.md#DIFFERENTIAL`; this owner reads the adjoint through `jax.value_and_grad`/`jax.vjp`/`jax.jacrev` over those solves rather than re-deriving it. `optimization/design.md#DESIGN` is the consumer driving an objective to a stationary point on this owner's `Gradient`/reverse-mode gradient; this owner only differentiates, never optimizes.
- [DGSM_SEAM]: `experiments/study.md#STUDY` owns sampled global sensitivity over SALib — Sobol, Morris, FAST, RBD-FAST, delta, PAWN, and the derivative-based DGSM screen. The DGSM analyzer (`SALib.analyze.dgsm`) consumes per-axis model derivatives; the `ForwardJacobian`/`ReverseJacobian` product this owner emits is exactly that derivative field, so `Study`'s `Dgsm` method reads this Jacobian rather than re-deriving a finite-difference gradient. A study sweeping `DiffPolicy.argnums` across a multi-argument objective attributes each emitted receipt to its differentiated argument through the receipt's `argnums` witness, so the swept Jacobian fields stay addressable per argument rather than collapsing to one indistinguishable stream. This is a consumer seam, never a re-implementation: local autodiff sensitivity stays here, sampled-design global sensitivity stays in `Study`, and the two meet at the Jacobian the study reads.
