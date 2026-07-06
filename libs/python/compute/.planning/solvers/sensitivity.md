# [PY_COMPUTE_SENSITIVITY]

`Differentiation` is the one automatic-differentiation and sensitivity owner: the `@tagged_union` whose `DiffModeTag` literal discriminates the full derivative algebra — scalar gradient, forward-mode Jacobian, reverse-mode Jacobian, Hessian, Jacobian-vector product, vector-Jacobian product, Hessian-vector product, and the finite-difference floor. Every differentiation a study, an optimizer, or a graduation gate needs is one mode value on this one owner rather than a `grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp`/`hvp` method family, and the entry is the `differentiate` method on the union itself — there is no separate autodiff surface beside it.

The eight modes resolve through one engine that stacks four admitted libraries as a single rail rather than four flat per-library uses. The frozen `DiffEngine` value object carries the gated `jax`/`equinox` modules with the array/pytree product-read built ONCE per solve behind the rail-wide `jax.config.update("jax_enable_x64", True)` float64 promotion; the one `_SPEC` table folds the `(DiffTarget, DiffModeTag)` pair to a `DiffSpec` value object carrying the cell's transform constructor or applicator and its admissible keyword set, while the `_PRODUCT` table keys each mode to its output-shape class; the `_split` fold peels each transform's `has_aux` return shift; and one `_summary` fold mints the shape-aware receipt. The JAX modes route `jax.value_and_grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp` over a plain array function while the `equinox.filter_*` peers route over an Equinox `Module`/PyTree objective whose differentiable leaves are the inexact arrays, the directional tangent riding through as a `Pytree` for the `PYTREE` target rather than an `np.ndarray` flatten — so a parametric model and a raw function differentiate through the same owner selected by one orthogonal `DiffTarget`/`DiffPolicy` row pair.

The finite-difference floor contracts the `findiff` central-difference `coefficients(deriv=1, acc=acc)` `center` stencil over an accuracy-width grid — never a fixed three-point hand-rolled stencil and never an `acc` silently capped — and runs as the worker fallback where automatic differentiation is unavailable. Reverse mode reads the implicit-function-theorem adjoint through the autodifferentiable Lineax/Optimistix/Diffrax solves the solver routes expose at float64, so a sensitivity through a solved system differentiates the converged solution rather than the iteration trace.

The typed `DiffReceipt` rides the runtime `ReceiptContributor` port: `contribute` narrows the port's `Iterable[Receipt]` to the concrete one-element `tuple[Receipt, ...]` the sibling `SolverReceipt.contribute` yields, minting through the two-argument `Receipt.of(owner, evidence)` contract, and the weave's `@receipted(REDACTION)` harvest streams the receipt on exit without an inline `Signals.emit`, the differentiated sensitivity graduating on the existing `solver` `HandoffAxis` without a parallel evidence rail. This owner never trains a model, fits a network, or carries a gradient-descent optimizer loop.

## [01]-[INDEX]

- [01]-[SENSITIVITY]: the one `Differentiation` AD owner discriminating eight `DiffModeTag` cases (scalar gradient, forward/reverse Jacobian, Hessian, JVP, VJP, HVP, finite-difference) over the `DiffEngine` carrier folding JAX at float64, the `equinox.filter_*` PyTree peers, and the gated `findiff` accuracy-scaled finite-difference floor as one weave-harvested rail, with the differentiation argument and aux selected by one `DiffPolicy` row, the one `(DiffTarget, DiffModeTag)`-keyed `_SPEC` table folding each cell to a `DiffSpec` carrying its transform-or-applicator and keyword surface, the `_PRODUCT` table keying each mode to its output-shape class, folding one typed `DiffReceipt` riding the `ReceiptContributor` port and carrying the mode, the `DiffProduct` shape with its `(rows, cols)` extent and Hessian symmetry residual, the objective value at the differentiation point, the differentiated product, the differentiated-argument `argnums` witness (the `int | tuple[int, ...]` index the JAX transform read, intrinsic product provenance), the exactness verdict, the realized accuracy order, and the implicit-adjoint witness.

## [02]-[SENSITIVITY]

- Owner: `Differentiation` — the ONE `@tagged_union` AD owner; the `Literal` tag IS the differentiation mode, read directly through `.tag`. The eight cases are the bounded derivative algebra — `Gradient()` over `jax.value_and_grad` (the scalar-objective value-and-gradient pair in one pass), `ForwardJacobian()` over `jax.jacfwd`, `ReverseJacobian()` over `jax.jacrev`, `Hessian()` over `jax.hessian`, `Jvp(tangent)` over `jax.jvp`, `Vjp(cotangent)` over `jax.vjp`, `Hvp(vector)` composing `jax.jvp(jax.grad(fn), (x,), (vector,))` (the Hessian-vector product without materializing the Hessian), and `FiniteDifference(step, acc)` over the `findiff` floor — and the mode decides the engine, the differentiated product, the objective value witness, and the exactness verdict. The eight `@classmethod` factories returning `Self` are the canonical tagged-union constructors binding the subtype once, never a `@staticmethod`-plus-`"Differentiation"`-forward-reference re-spelled eight times. The single entry `mode.differentiate(fn, x)` is the method on the union — the mode IS the owner — folding every case into one `_dispatch`, never a `value_and_grad`/`jacfwd`/`jacrev`/`hessian`/`jvp`/`vjp`/`hvp` method family. A new differentiation engine is one `Differentiation` case plus one `_SPEC` row; the scalar `Gradient`, the directional `Jvp`/`Vjp`/`Hvp` projections, and the full Jacobian/Hessian share the one dispatch.
- Policy axis: `DiffPolicy` is the ONE orthogonal differentiation-argument row carried on `differentiate` alongside the mode — `argnums` (the positional argument differentiated, `int | tuple[int, ...]` matching the JAX transform contract: a bare `int` differentiates one argument and the transform returns the single product, a `tuple[int, ...]` differentiates several and the transform returns the per-argument product tuple `_summary` concatenates into one flat product keyed by the recorded index set), `holomorphic` (complex-holomorphic differentiation), `allow_int` (integer-input differentiation), and `has_aux` (the objective returns a `(value, aux)` pair). The keyword projection is data, not a per-mode spray: each `DiffSpec.keywords` tuple names exactly the `DiffPolicy` fields its cell's transform `.api` signature accepts, and `DiffPolicy.projected(keywords)` reads them off that row — `value_and_grad(fun, argnums, has_aux)` takes `argnums`/`has_aux` and rejects `holomorphic`; `jacfwd(fun, argnums, has_aux, holomorphic)` is the one full-Jacobian transform carrying `holomorphic`; `jacrev(fun, argnums, has_aux, allow_int)` carries `allow_int` not `holomorphic`; `hessian(fun, argnums, has_aux)` carries neither — so each transform receives only the keywords it accepts, never a `holomorphic=` that `value_and_grad`/`jacrev`/`hessian` raise on. `has_aux=True` shifts the transform return shape, so the `_split` fold peels `value_and_grad`'s `((value, aux), grad)` and the Jacobian/Hessian transforms' `(product, aux)` against the `(mode-carries-value, has_aux)` axis pair and lands the witness on the receipt's `aux` flag; the `equinox.filter_*` peers carry `has_aux` ALONE (no integer `argnums`/`holomorphic`/`allow_int`) and differentiate by the first argument's array-leaf `filter_spec`, so the `PYTREE` cells carry `("has_aux",)` and never thread `argnums` the filter transform would reject. The directional `jvp`/`vjp` cells carry an empty `keywords` tuple by construction, since a `(value, aux)` head shifts the directional return tuple the cell unpacks; the `hvp` cell carries the inner-`grad` keyword surface its applicator threads. A multi-argument objective differentiated by several inputs sets `DiffPolicy.argnums=(0, 1, ...)` in one call — the transform's per-argument product tuple folds to one flat product with the index set on the receipt — never a re-entry per argument and never a second entry beside the mode.
- Leaf axis: `DiffTarget` is the ONE bounded objective-shape policy carried on `differentiate` — `ARRAY` for a plain `np.ndarray`→`np.ndarray` function differentiated through the bare `jax.*` transforms, and `PYTREE` for an Equinox `Module`/PyTree objective differentiated through the `equinox.filter_*` peers, where `equinox.partition`/`is_inexact_array` split the inexact-array design leaves the transform differentiates from the static rest. The `_SPEC` table is keyed on `(DiffTarget, DiffModeTag)` and is the SINGLE dispatch owner for the transform modes, the directional modes, and the finite-difference floor alike: the analytic cells carry a `DiffSpec` whose `apply` projects the pair to its constructor — `(ARRAY, gradient)→jax.value_and_grad`, `(PYTREE, gradient)→equinox.filter_value_and_grad`, `(ARRAY, reverse_jacobian)→jax.jacrev`, `(PYTREE, reverse_jacobian)→equinox.filter_jacrev` — while the directional cells carry the applicator closure that calls `jax.jvp`/`eqx.filter_jvp`, `jax.vjp`/`eqx.filter_vjp`, and the forward-over-reverse `jax.jvp(jax.grad(fn), ...)` HVP composition, so JVP/VJP/HVP are three table rows rather than three sibling helper bodies. The array and the PyTree path are two columns on the one table rather than two parallel owners. The directional input is parameterized over the target shape and rides the space its transform reads: the `Jvp` tangent and `Hvp` vector live in the INPUT space (`jax.jvp`/`jax.grad` consume an input-shaped perturbation) while the `Vjp` cotangent lives in the OUTPUT space (`vjp_fn` consumes an output-shaped cotangent), all three carried as one `Pytree` case payload. The `ARRAY` applicator cells lift that payload through `jax.numpy.asarray` while the `PYTREE` cells thread the structured tree straight into `eqx.filter_jvp`/`eqx.filter_vjp` — never a `jax.numpy.asarray` over a PyTree leaf, which flattens a non-array leaf. The PyTree product flattens through `DiffEngine.flatten`, which keeps the `equinox.is_inexact_array` (differentiated) leaves via `equinox.partition` and concatenates their `np.ravel`ed values over the cataloged `jax.tree_util.tree_leaves` surface (the array branch a bare `np.asarray`). The finite-difference floor and the HVP composition are `ARRAY`-only (a PyTree leaf has no uniform grid; the HVP scalar-reduces through `jax.grad` over the array primal), so the `(PYTREE, finite_difference)` and `(PYTREE, hvp)` cells are absent from `_SPEC` and that one membership miss carries `DiffStatus.UNSUPPORTED` on the receipt rather than raising.
- Implicit-adjoint loop: when the differentiated function is itself a solve, reverse mode reads the adjoint through the solver rather than through the iterations. The Lineax `linear_solve`, the Optimistix `root_find`/`minimise`/`fixed_point`/`least_squares` (each carrying `optimistix.ImplicitAdjoint` by default, one `lineax` linear solve per backward pass), and the Diffrax adjoint solve carry implicit-function-theorem adjoints, so `jax.vjp`/`jax.jacrev`/`jax.value_and_grad` over a function that calls them pulls back through the converged solution; `solvers/linear.md#LINEAR`, `solvers/nonlinear.md#NONLINEAR`, and `solvers/differential.md#DIFFERENTIAL` expose those autodifferentiable solves, and this owner consumes the adjoint they carry. `optimization/design.md#DESIGN` reads this owner's `Gradient`/reverse-mode gradient over an inner-solve objective for inverse design (its `equinox.filter_value_and_grad(Objective.cost, has_aux=True)` pass is exactly the `(PYTREE, gradient)` cell this owner folds). `experiments/study.md#STUDY` owns a DISJOINT sampled-DGSM rail over `SALib.analyze.dgsm` and never calls this owner — the shared concept is the `∂y/∂x` derivative field, not a wire (the `[DGSM_SEAM]` row carries the boundary).
- Entry: `mode.differentiate(lane, fn, x)` is `async`, composing `lane.offload(_dispatch, ..., modality=Modality.PROCESS, retry=RetryClass.OCCT)` under the hub `evidence_run` weave — the x64-gated AD family pins the PROCESS modality, the retry wraps the isolation leg only, and the weave owns span, fence, and the `@receipted(REDACTION)` receipt harvest — and returns `RuntimeRail[DiffReceipt]` carrying the mode, the objective `value` at the differentiation point (the `Gradient` scalar value-and-gradient pair, and the singleton-output `ARRAY` `Jvp` primal; `None` for the `Vjp`/`Hvp` projections, the vector-output `Jvp`, and the matrix-valued Jacobian/Hessian modes), the differentiated product summary (the Jacobian/Hessian Frobenius norm, the gradient/JVP/HVP max-component magnitude), the `DiffStatus` verdict (the JAX modes `EXACT`, the finite-difference mode `TRUNCATION_BOUNDED`, a non-finite product `NONFINITE`, an unsupported pairing `UNSUPPORTED`), the realized accuracy order, and the implicit-adjoint witness flag. The aspect harvests the `DiffReceipt.contribute` stream on exit, so receipt production is a decorator rail rather than an inline `Signals.emit` — matching every sibling solver route. `_dispatch` resolves the one `_SPEC[(target, mode.tag)]` row first — an absent cell is the single UNSUPPORTED verdict — then routes on a total `match` over the mode tag: the finite-difference floor short-circuits before any `gated()` call, and the JAX-requiring arms build one `DiffEngine.gated()` carrier and thread it plus the resolved `spec` into `_transformed` (the gradient/Jacobian/Hessian rail, calling `spec.apply(engine)` under the `spec.keywords`-projected `DiffPolicy` keywords and peeling the `has_aux` `(value, aux)`/`(product, aux)` shape shift through the `_split` fold so it lands on the receipt's `value`/`aux` witnesses rather than crashing the `float(out[0])` read) or `_directional` (the JVP/VJP/HVP rail, calling `spec.apply` over the carrier, the `(primal, tangent)` pair, and the projected keywords). Both rails read the product back through `DiffEngine.flatten` (the per-argument `_leaf` read — an array target a bare `np.asarray`, a pytree target the `equinox.partition`/`jax.tree_util.tree_leaves` leaf concatenation — wrapped once by the tuple-`argnums` block fold that concatenates the transform's per-argument product tuple); the finite-difference body assembles the Jacobian column-by-column from the `findiff` central-difference `coefficients(deriv=1, acc=acc)` `center` stencil weights/offsets contracted over the per-axis sample grid, divided by `step`, and stacked through `np.stack(axis=1)`. `boundary` converts an unexpected host fault into the runtime fault rail; the unsupported mode/target pairing (a `PYTREE` finite-difference or HVP, whose `_SPEC` cell is absent) is carried inside the success receipt as `DiffStatus.UNSUPPORTED`, so the two failure notions stay distinct. One `_summary` fold builds the receipt for every mode, keyed on both the `DiffStatus` the body resolves and the `_PRODUCT[mode]` output-shape class that resolves the product's `(rows, cols)` extent (the 2-D Jacobian/Hessian as-is, the scalar/directional vector lifted to `(1, n)`) and mints the Hessian symmetry residual — never a parallel inline receipt construction per mode.
- Receipt: `DiffReceipt` is the typed AD receipt — never a generic reported-value abstraction — carrying the mode tag, the `DiffProduct` output-shape class, the `argnums` differentiated-argument witness (the positional index — or the `tuple[int, ...]` index set — the JAX transform differentiated by, the intrinsic provenance of WHICH argument the product is `∂y/∂xᵢ` against, self-justified on the receipt exactly as `accuracy`/`symmetry`/`implicit_adjoint` are: a reader of one emitted receipt knows the differentiated argument without re-deriving it from the call site, and the field is `0` on the single-argument default the `(PYTREE, *)` filter cells always take), the objective `value` witness, the `aux` has_aux witness flag, the differentiated product as a flat `tuple[float, ...]` (the gradient for the scalar/directional modes, the flattened Jacobian/Hessian for the full modes), the `(rows, cols)` matrix extent the shape resolves, the `max_magnitude`/`frobenius` product summaries, the Hessian `symmetry` residual `‖H - Hᵀ‖_∞` (`0.0` off the Hessian mode), the `DiffStatus` verdict, the realized `accuracy` order, and the `implicit_adjoint` witness — so the product is parameterized over both input argument and output shape, not erased to one norm pair. `DiffStatus` is a value object with behavior: its `exact` predicate tests membership in the `_EXACT` `frozenset` so the autodiff-vs-truncation witness lives once on the vocabulary, mirroring the sibling `SolveStatus.converged`. `DiffReceipt.contribute` implements the runtime `ReceiptContributor` port structurally, narrowing the port's `Iterable[Receipt]` to the concrete one-element `tuple[Receipt, ...]` return the sibling `solvers/receipt.md#RECEIPT` `SolverReceipt.contribute` and `graduation/handoff.md#GRADUATION` `GraduationReceipt.contribute` both yield, minting through the two-argument `Receipt.of(owner, evidence)` contract as `Receipt.of("compute.differentiation", ("emitted", self.mode, facts))`, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes; the weave's `@receipted(REDACTION)` harvest streams it while `contribute` itself stays undecorated, exactly as the runtime owner declares. The facts ride as native `float`/`int`/`bool`/`DiffProduct`/`DiffStatus` through the runtime `Signals` `msgspec` `Encoder(enc_hook=repr, order="deterministic")` rather than a `str()` coerce, so the `DiffStatus` verdict, the `exact` flag, the `(rows, cols)` extent, the `symmetry` residual, the `accuracy` order, and the finite-difference step reach the C# graduation gate as numeric evidence through the existing `solver` `HandoffAxis` case at `graduation/handoff.md#GRADUATION`. No new handoff axis — the differentiated sensitivity crosses on the `solver` axis already present.
- Packages: `jax` (`value_and_grad`, `grad`, `jacfwd`, `jacrev`, `hessian`, `jvp`, `vjp`, `tree_util.tree_leaves`, `numpy.asarray`, `config.update("jax_enable_x64", True)` — the rail-wide float64 promotion the gated carrier runs so the implicit adjoint pulls back at double precision), `equinox` (`filter_value_and_grad`, `filter_grad`, `filter_jacfwd`, `filter_jacrev`, `filter_hessian`, `filter_jvp`, `filter_vjp`, `partition`, `is_inexact_array`), `findiff` (`coefficients(deriv, acc=)` — the cataloged raw-coefficient surface whose `center` entry carries the accuracy-scaled central-difference `coefficients` weights, `offsets`, and the realized `accuracy` order the floor reads onto the receipt, never a fixed three-point hand-rolled stencil and never `min(acc, 2)`), `optimistix`/`lineax`/`diffrax` (the implicit-adjoint solves consumed transitively through the solver routes, never imported here), `numpy` (`asarray`, `eye`, `ravel`, `reshape`, `stack`, `tensordot`, `concatenate`, `linalg.norm`, `all`, `isfinite`), `expression` (`tag`, `case`, `tagged_union` for the `Differentiation` union; `expression.collections.Map` for the empty `Redaction.classified` policy), `dataclasses` (`dataclass(frozen=True, slots=True)` for the `DiffPolicy`/`DiffSpec`/`DiffEngine` value objects), `msgspec` (`Struct` for the `DiffReceipt` record), `expression.collections` (`Map` for the `_SPEC`/`_PRODUCT` tables, matching the sibling solver routes), `typing` (`cast` narrowing `DiffSpec.apply` to the kind-implied `TransformPick`/`Applicator` at the rail call site), `graduation/handoff.md#GRADUATION` (the `solver` axis the sensitivity graduates on), `experiments/study.md#STUDY` (the DGSM consumer reading this Jacobian), runtime (`RuntimeRail`, `boundary`, the `Receipt`/`ReceiptContributor` port satisfied structurally, the hub-exported `REDACTION` riding the weave harvest, plus `Signals` whose `msgspec` encoder carries the receipt's native scalars).
- Growth: a new analytic-transform mode is one `Differentiation` case plus one `_SPEC` row carrying its transform selector and keyword tuple plus one `_PRODUCT` row naming its output shape; a new directional mode is one `Differentiation` case plus one `_SPEC` applicator row plus one `_PRODUCT` row; a new objective shape is one `DiffTarget` member plus its `_SPEC` column; a new differentiation argument or aux mode is one `DiffPolicy` field plus its membership on the affected `_SPEC` rows' `keywords` tuples; a new product geometry (a sparse or block-structured Jacobian summary) is one `DiffProduct` member plus its `_summary` reshape arm; zero new surface, no parallel autodiff page, no `value_and_grad`/`jacrev`/`hessian`/`jvp`/`vjp`/`grad`/`hvp` method family, no per-mode helper file, no parallel transform/applicator/keyword table triple, no hand-rolled stencil beside the `findiff` coefficient surface.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from enum import StrEnum
from typing import Final, Literal, Self, assert_never, cast

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass


# --- [TYPES] -------------------------------------------------------------------------------

# A plain array objective is `np.ndarray -> np.ndarray`; a PyTree objective is `Pytree -> Pytree`
# (an `equinox.Module`/structured tree). `Pytree` stays `object` because the directional tangent and
# the differentiation point ride through the transform as arbitrary pytrees, never narrowed to
# `np.ndarray`, which would flatten a structured leaf, the same vocabulary the sibling solver routes carry.
type ArrayFn = Callable[[np.ndarray], np.ndarray]
type Pytree = object
type DiffModeTag = Literal["gradient", "forward_jacobian", "reverse_jacobian", "hessian", "jvp", "vjp", "hvp", "finite_difference"]
# A directional applicator reads the gated carrier, the objective, the `(primal, tangent)` pytree pair, and
# the projected `DiffPolicy` keywords (the HVP cell threads them into its inner `jax.grad`; the JVP/VJP cells
# ignore them), returning the `(value, product)` pair the summary folds.
type Applicator = Callable[["DiffEngine", Callable, tuple[Pytree, Pytree], dict[str, object]], tuple[float | None, object]]
# A transform-constructor selector reads the gated jax/equinox modules off the carrier and returns the
# `jax.*`/`equinox.filter_*` transform the gradient/Jacobian/Hessian modes call under the spec's keywords.
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

# the family modality row: `DiffEngine.gated()` mutates the process-global x64 flag, so the AD family
# pins PROCESS; policy DATA beside the tables, never a per-page literal.
_MODALITY: Final[Modality] = Modality.PROCESS

# Per-mode OUTPUT-shape class: the differentiated product geometry the `_summary` fold reads to mint
# shape-aware facts, target-independent because a Jacobian is a Jacobian whether the objective is an array
# or a PyTree. The scalar/directional modes carry a flat vector, the full-Jacobian and the finite-difference
# floor a 2-D matrix, the Hessian a square matrix carrying its symmetry residual. `_summary` reads this for
# every mode including the UNSUPPORTED pairing that has no `_SPEC` row, so the shape vocabulary lives here
# rather than only on the spec; a new mode names its product class in one row, never an inline reshape.
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


# --- [MODELS] ------------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class DiffPolicy:
    argnums: int | tuple[int, ...] = (
        0  # positional argument(s) differentiated; the bare jax transform contract — `int` returns one product, `tuple[int, ...]` returns the per-argument product tuple `_summary` concatenates. Threaded to the bare jax transforms only (the `equinox.filter_*` peers reject it)
    )
    holomorphic: bool = False  # complex-holomorphic differentiation; jax.grad/jacfwd alone carry it
    allow_int: bool = False  # integer-input differentiation; jax.jacrev/grad alone carry it
    has_aux: bool = False  # objective returns (value, aux); _split peels the shifted return, aux flag rides the receipt

    # The spec's `keywords` tuple names exactly the `DiffPolicy` fields the cell's transform admits, so the
    # projection reads them off the row rather than re-branching on target: the PYTREE cells carry
    # `("has_aux",)` (the `equinox.filter_*` peers reject integer `argnums`/`holomorphic`/`allow_int`), the
    # ARRAY analytic cells carry the bare-jax keyword tuple, and the directional/finite-difference cells
    # carry `()`. The projection is total over any tuple the row supplies — no `KeyError` on an unkeyed mode.
    def projected(self, keywords: tuple[str, ...]) -> dict[str, object]:
        return {name: getattr(self, name) for name in keywords}


# The ONE dispatch row each `(DiffTarget, DiffModeTag)` cell resolves to, collapsing the former
# `_TRANSFORM`/`_DIRECTIONAL`/`_KEYWORDS` table triple into one value object: `keywords` is the
# `DiffPolicy`-field projection the cell's transform admits, and `apply` is the cell's callable — a
# `TransformPick` for the analytic gradient/Jacobian/Hessian cells, an `Applicator` for the directional
# JVP/VJP/HVP cells, and `None` for the finite-difference cell (whose `_finite_difference` body reads
# `step`/`acc` off the matched case rather than a carrier callable). The rail is selected by `_dispatch`'s
# total `match` over the mode tag — the tag IS the kind discriminant, so the spec carries no redundant
# `kind` field — and a `(target, mode)` pairing absent from `_SPEC` (a `PYTREE` HVP or `PYTREE`
# finite-difference) is the single UNSUPPORTED source, so adding a mode is one `_SPEC` row.
@dataclass(frozen=True, slots=True)
class DiffSpec:
    keywords: tuple[str, ...]
    apply: TransformPick | Applicator | None


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

    # The mode IS the owner: the one `async` entry composes the process lane (the x64 mutation is
    # process-global native state) under the hub weave — span, fence, and the `@receipted(REDACTION)`
    # harvest of the `DiffReceipt` are composed, never an inline `Signals.emit`. Worker death rides
    # `retry=RetryClass.OCCT` on the isolation leg only; the deterministic derivative never retries.
    async def differentiate(
        self, lane: LanePolicy, fn: Callable, x: Pytree, target: DiffTarget = DiffTarget.ARRAY, policy: DiffPolicy = DiffPolicy()
    ) -> "RuntimeRail[DiffReceipt]":
        async def dispatch() -> RuntimeRail[DiffReceipt]:
            return await lane.offload(_dispatch, fn, x, self, target, policy, modality=_MODALITY, retry=RetryClass.OCCT)

        return await evidence_run(EvidenceScope.SENSITIVITY, f"diff.{self.tag}", dispatch)


# The gated jax/equinox modules folded into one value object with behavior built ONCE per solve: the
# transform and directional rails read `engine.jax`/`engine.eqx` off the carrier rather than each repeating
# `import jax`/`import equinox`, so the gated import and the float64 promotion fire once and both rails
# thread the carrier `_dispatch` constructs. `gated()` runs the rail-wide
# `jax.config.update("jax_enable_x64", True)` precondition the JAX siblings hold — a reverse-mode adjoint
# pulled back through a `lineax`/`optimistix`/`diffrax` solve assumes float64, and the x32 default silently
# degrades the implicit-function-theorem gradient. `flatten` owns the array/pytree product-read fork once
# for every mode: the `_leaf` primitive reads one product (the per-leaf `tree_util.tree_leaves` over the
# inexact-array partition for a pytree, a bare `np.asarray` for an array — never a `numpy.asarray` over a
# structured pytree), and `flatten` wraps it with the tuple-`argnums` block fold that concatenates the
# transform's per-argument product tuple into one flat product keyed by the index set on the receipt.
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

    # A bare `int` argnums yields one product `_leaf` reads; a `tuple[int, ...]` argnums yields the
    # transform's per-argument product tuple, concatenated leaf-by-leaf into one flat product keyed by the
    # index set on the receipt — the multi-argument Jacobian `[∂y/∂x₀ | ∂y/∂x₁ | ...]` stacked column-blockwise.
    # The per-argument read is the one array/pytree leaf fork; the tuple fold rides on top of it once.
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
    argnums: (
        int | tuple[int, ...]
    )  # the differentiated positional argument(s) the JAX transform read — intrinsic product provenance carried on the receipt that holds the product, self-describing like `accuracy`/`implicit_adjoint`, not a demultiplex key any consumer reads back (the `PYTREE` filter modes report `0`, the first-arg leaf filter)
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

# The ONE `(target, mode)` dispatch table — every analytic transform, directional applicator, finite-difference
# floor, and per-cell keyword surface on one owner. The ARRAY analytic cells select the bare `jax.*` transform
# under the bare-jax keyword tuple (`gradient`→value-and-gradient, the Jacobian/Hessian columns the full
# transforms); the PYTREE analytic cells select the `equinox.filter_*` peer carrying `("has_aux",)` alone (the
# peers differentiate the first argument's inexact-array leaves and reject integer `argnums`/`holomorphic`).
# The directional cells are the JVP/VJP/HVP applicators: the ARRAY rows lift primal and tangent through
# `jnp.asarray` and call the bare jax directional transforms, the PYTREE rows thread the structured tree
# straight into the `filter_*` peer (a `jnp.asarray` over a PYTREE tangent would flatten a non-array leaf, the
# forbidden form). The HVP composition `jvp(grad(fn), (x,), (v,))` is the forward-over-reverse second-order
# directional derivative materializing no Hessian — ARRAY-only, scalar-reducing through `jax.grad` over the
# array primal, with its `keywords` the inner-`grad` surface the applicator itself threads (never `has_aux`,
# whose `(grad, aux)` head breaks the outer `jvp`). The `(ARRAY, finite_difference)` cell carries `apply=None`
# so the mode-tag match routes it to `_finite_difference` before any carrier build. A `(target, mode)` pairing
# absent here (a `PYTREE` HVP or `PYTREE` finite-difference) is the single UNSUPPORTED source — both
# UNSUPPORTED notions fold to one membership miss rather than two separate guards. The directional cells hold
# no keyword tuple because a `(value, aux)` head would shift the `jvp`/`vjp` return tuple the cell unpacks.
_SPEC: Map[tuple[DiffTarget, DiffModeTag], DiffSpec] = Map.of_seq([
    ((DiffTarget.ARRAY, "gradient"), DiffSpec(("argnums", "has_aux"), lambda e: e.jax.value_and_grad)),
    ((DiffTarget.ARRAY, "forward_jacobian"), DiffSpec(("argnums", "has_aux", "holomorphic"), lambda e: e.jax.jacfwd)),
    ((DiffTarget.ARRAY, "reverse_jacobian"), DiffSpec(("argnums", "has_aux", "allow_int"), lambda e: e.jax.jacrev)),
    ((DiffTarget.ARRAY, "hessian"), DiffSpec(("argnums", "has_aux"), lambda e: e.jax.hessian)),
    ((DiffTarget.PYTREE, "gradient"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_value_and_grad)),
    ((DiffTarget.PYTREE, "forward_jacobian"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_jacfwd)),
    ((DiffTarget.PYTREE, "reverse_jacobian"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_jacrev)),
    ((DiffTarget.PYTREE, "hessian"), DiffSpec(("has_aux",), lambda e: e.eqx.filter_hessian)),
    ((DiffTarget.ARRAY, "jvp"), DiffSpec( (), lambda e, fn, v, kw: _read_array_jvp(e.jax.jvp(fn, (e.jax.numpy.asarray(v[0]),), (e.jax.numpy.asarray(v[1]),))) )),
    ((DiffTarget.PYTREE, "jvp"), DiffSpec((), lambda e, fn, v, kw: (None, e.eqx.filter_jvp(fn, (v[0],), (v[1],))[1]))),
    ((DiffTarget.ARRAY, "vjp"), DiffSpec((), lambda e, fn, v, kw: (None, e.jax.vjp(fn, e.jax.numpy.asarray(v[0]))[1](e.jax.numpy.asarray(v[1]))[0]))),
    ((DiffTarget.PYTREE, "vjp"), DiffSpec((), lambda e, fn, v, kw: (None, e.eqx.filter_vjp(fn, v[0])[1](v[1])[0]))),
    ((DiffTarget.ARRAY, "hvp"), DiffSpec( ("argnums", "holomorphic", "allow_int"), lambda e, fn, v, kw: (None, e.jax.jvp(e.jax.grad(fn, **kw), (e.jax.numpy.asarray(v[0]),), (e.jax.numpy.asarray(v[1]),))[1]), )),
    ((DiffTarget.ARRAY, "finite_difference"), DiffSpec((), None)),
])


# --- [OPERATIONS] --------------------------------------------------------------------------


# the one measured kernel returning the `DiffReceipt` — module-level and import-resolvable, so it
# crosses the process lane as spec data plus operands; the weave's `@receipted(REDACTION)` harvest
# streams the receipt. `_dispatch` resolves the one `_SPEC` row first: an absent `(target, mode.tag)`
# cell is the single UNSUPPORTED verdict (a `PYTREE` HVP or `PYTREE` finite-difference). One total
# `match` over the mode tag then selects the rail — the finite-difference floor short-circuits to
# `_finite_difference` BEFORE any carrier build; the one `DiffEngine.gated()` carrier threads the
# gated import + float64 promotion once, and the directional tangent rides through as a pytree.
def _dispatch(fn: Callable, x: Pytree, mode: Differentiation, target: DiffTarget, policy: DiffPolicy) -> DiffReceipt:
    if (spec := _SPEC.try_find((target, mode.tag)).to_optional()) is None:
        return _summary(np.asarray([]), mode.tag, target, status=DiffStatus.UNSUPPORTED, accuracy=0, implicit=False)
    match mode:
        case Differentiation(tag="finite_difference", finite_difference=(step, acc)):
            return _finite_difference(fn, x, step, acc)
        case Differentiation(tag="jvp", jvp=tangent) | Differentiation(tag="vjp", vjp=tangent) | Differentiation(tag="hvp", hvp=tangent):
            return _directional(DiffEngine.gated(), spec, fn, (x, tangent), mode.tag, target, policy)
        case Differentiation(tag="gradient" | "forward_jacobian" | "reverse_jacobian" | "hessian"):
            return _transformed(DiffEngine.gated(), spec, fn, x, mode.tag, target, policy)
        case _ as unreachable:
            assert_never(unreachable)


# One receipt fold for every mode, keyed on the OUTPUT-shape class: the product flattens, the status is the
# caller's verdict refined to NONFINITE on a non-finite product, the optional primal value rides the
# value-carrying modes, and the `_PRODUCT[mode]` geometry resolves the matrix extent and the Hessian
# symmetry residual — the 2-D product as-is for a JACOBIAN `(out, in)` and a HESSIAN `(n, n)` (whose
# off-diagonal residual `‖H - Hᵀ‖_∞` is the symmetry witness), a `(1, n)` lift for a SCALAR/directional
# vector. The shape-aware extent is parameterized over output, not erased to one norm pair.
def _summary(
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
) -> DiffReceipt:
    arr = np.asarray(product, dtype=float)
    flat = np.ravel(arr)
    shape = _PRODUCT[mode]
    verdict = DiffStatus.NONFINITE if not np.all(np.isfinite(flat)) else status
    # A JACOBIAN/HESSIAN product arrives 2-D; a scalar/directional vector arrives 1-D and lifts to `(1, n)`.
    matrix = arr if arr.ndim >= 2 else flat.reshape(1, -1)
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


# The gradient/Jacobian/Hessian rail over the prebuilt carrier: read the transform selector off the spec's
# `apply`, call it under the spec's `keywords` projection (the PYTREE cells carrying `has_aux` alone), read the
# product back through the carrier's flatten — the ARRAY branch keeps the 2-D Jacobian/Hessian extent through
# `np.asarray` so `_summary` recovers `(rows, cols)`, the PYTREE branch concatenates the inexact leaves to a
# flat vector. The keyword projection — not a per-mode `if` — keeps `holomorphic`/`allow_int` off a transform
# that rejects them, and `_split` peels the value/aux/product triple off the transform's has_aux return shape.
def _transformed(
    engine: DiffEngine, spec: DiffSpec, fn: Callable, x: Pytree, mode: DiffModeTag, target: DiffTarget, policy: DiffPolicy
) -> DiffReceipt:
    transform = cast(TransformPick, spec.apply)(engine)
    primal = engine.jax.numpy.asarray(x) if target is DiffTarget.ARRAY else x
    out = transform(fn, **policy.projected(spec.keywords))(primal)
    # value_and_grad returns `((value, aux), grad)` under has_aux else `(value, grad)`; the Jacobian/Hessian
    # transforms return `(product, aux)` under has_aux else the bare product. One `_split` fold peels the
    # `(value, product, aux)` triple off the mode and the policy's `has_aux` flag rather than per-mode reshaping.
    value, product, aux = _split(out, mode, policy.has_aux)
    return _summary(
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


# The JVP/VJP/HVP rail over the prebuilt carrier `_dispatch` threads in: the spec's `apply` applicator returns
# the (value, product) pair under the spec's projected keywords (the HVP cell threads them into its inner
# `jax.grad`, the JVP/VJP cells ignore them), and the product reads back through the carrier's flatten. The
# UNSUPPORTED-pairing short-circuit lives in `_dispatch` so this rail never receives an absent cell and never
# re-imports jax; the tangent stays a pytree for the PYTREE target.
def _directional(
    engine: DiffEngine, spec: DiffSpec, fn: Callable, primals: tuple[Pytree, Pytree], mode: DiffModeTag, target: DiffTarget, policy: DiffPolicy
) -> DiffReceipt:
    value, product = cast(Applicator, spec.apply)(engine, fn, primals, policy.projected(spec.keywords))
    # `jax.jvp`/`vjp` carry no `argnums` keyword — a directional derivative is against the one `(primal, tangent)`
    # pair, so the product is single-block and the witness is `0` regardless of `policy.argnums`.
    return _summary(engine.flatten(product, target), mode, target, status=DiffStatus.EXACT, accuracy=0, implicit=True, value=value)


# `jax.jvp` returns the (primal_out, tangent_out) pair over an array objective; the scalar primal rides
# the receipt value when the output is a singleton, the tangent is the directional product. The PYTREE
# JVP keeps `value=None` because a pytree primal output has no single scalar witness.
def _read_array_jvp(out_tangent: tuple[object, object]) -> tuple[float | None, object]:
    out, tangent = out_tangent
    value = float(np.asarray(out)) if np.asarray(out).size == 1 else None
    return value, tangent


# The finite-difference floor is ARRAY-only — a pytree leaf has no uniform grid, so the `(PYTREE,
# finite_difference)` cell is absent from `_SPEC` and `_dispatch` carries that pairing as UNSUPPORTED before
# needs no jaxlib backend. The Jacobian columns are the central-difference contraction of the findiff
# `coefficients(deriv=1, acc=acc)` stencil — the cataloged raw-coefficient surface — over the per-axis sample
# grid: the stencil weights/offsets honor the requested accuracy order directly, never an `acc` capped to 2
# and never a fixed three-point hand-rolled stencil. `coefficients` returns the central scheme as the `center`
# entry carrying its `coefficients` weights, `offsets`, and an `accuracy` entry; the receipt records that
# realized order (an odd `acc` request the central scheme rounds to even reaches it truthfully) rather than
# the requested `acc`. Each axis derivative is the weight-contracted finite difference divided by `step`,
# raveled so a scalar- and vector-output objective both stack to a `(out, in)` Jacobian through `np.stack`.
def _finite_difference(fn: ArrayFn, x: object, step: float, acc: int) -> DiffReceipt:
    from findiff import coefficients

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
        jac, "finite_difference", DiffTarget.ARRAY, status=DiffStatus.TRUNCATION_BOUNDED, accuracy=int(center["accuracy"]), implicit=False
    )
```
