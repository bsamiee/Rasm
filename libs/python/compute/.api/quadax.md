# [PY_COMPUTE_API_QUADAX]

`quadax` supplies JAX-native adaptive numerical quadrature for the compute integration rail: globally-adaptive Gauss-Kronrod (`quadgk`), Clenshaw-Curtis (`quadcc`), and tanh-sinh (`quadts`) integrators plus Romberg integration, each returning a value paired with a `QuadratureInfo` receipt; every integration is JIT-compatible and differentiable through the integrand under forward/reverse-mode autodiff.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `quadax`
- package: `quadax`
- import: `quadax`
- owner: `compute`
- rail: quadrature
- namespace: `quadax` (integrators, rule classes, `STATUS`, and the `sampled` integrators re-exported at top level; `QuadratureInfo` lives in `quadax.utils` and is NOT a top-level export); submodules `quadax.adaptive`, `quadax.fixed_order`, `quadax.sampled`, `quadax.quad_weights`, `quadax.utils`
- installed: `0.2.13`
- requires: `jax`, `jaxlib`, `equinox`
- capability: globally-adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature, Romberg and tanh-sinh-Romberg integration, fixed-order rule classes, sampled-data trapezoidal/Simpson integration, and JIT-compatible / forward-and-reverse-differentiable integrate pipelines over a JAX integrand

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quadrature result receipt
- rail: quadrature
- `QuadratureInfo` is a `typing.NamedTuple` in `quadax.utils`, NOT a top-level `quadax` export — import `from quadax.utils import QuadratureInfo`, or accept it positionally as the second tuple element. Its `status` is a bitfield combining `NORMAL_EXIT`/`MAX_NINTER`/`ROUNDOFF`/`BAD_INTEGRAND`/`NO_CONVERGE`, so codes 0-31 decode to the union of set flags; `STATUS` is the top-level re-export of `quadax.utils.STATUS`.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]               | [CAPABILITY]                                        |
| :-----: | :---------------------------- | :-------------------------- | :-------------------------------------------------- |
|  [01]   | `quadax.utils.QuadratureInfo` | result carrier (NamedTuple) | receipt fields `err`/`neval`/`status`/`info`        |
|  [02]   | `STATUS`                      | decode table                | `dict[int, str]` status-code -> convergence message |

[PUBLIC_TYPE_SCOPE]: fixed-rule quadrature classes
- rail: quadrature
- each rule owns the node/weight construction for one quadrature family and is composed by the adaptive integrators; a rule is constructed once and reused across panels. `AbstractQuadratureRule` is the polymorphic base; `adaptive_quadrature` discriminates the family by the rule instance it is given.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `AbstractQuadratureRule` | rule base     | abstract base `adaptive_quadrature` dispatches on     |
|  [02]   | `GaussKronrodRule`       | fixed rule    | Gauss-Kronrod node/weight rule, embedded error        |
|  [03]   | `ClenshawCurtisRule`     | fixed rule    | Clenshaw-Curtis (Chebyshev-node) nested-error rule    |
|  [04]   | `TanhSinhRule`           | fixed rule    | tanh-sinh (double-exponential) endpoint-singular rule |
|  [05]   | `NestedRule`             | nested rule   | pairs high-order + embedded lower-order estimate      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: adaptive quadrature
- rail: quadrature
- each integrator returns `(value, QuadratureInfo)`; `fun` is a JAX-traceable scalar/array integrand and the integration is differentiable through `fun` and the interval bounds.
- `adaptive_quadrature` is the canonical polymorphic driver: one adaptive surface discriminating the quadrature family by the rule instance passed as `rule`. `quadgk` / `quadcc` / `quadts` are named specializations that construct the matching rule and delegate to it. Adaptive integrators share `(fun, interval, args=(), full_output=False, epsabs=None, epsrel=None, max_ninter=50, norm=inf)` and return `(value, QuadratureInfo)`; `adaptive_quadrature` prepends `rule` and adds `**kwargs`; `romberg`/`rombergts` swap `max_ninter` for `divmax=20`.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]              | [RAIL]                                                  |
| :-----: | :---------------------------------------------- | :-------------------------- | :------------------------------------------------------ |
|  [01]   | `adaptive_quadrature(rule, fun, interval, ...)` | polymorphic adaptive driver | rule-parameterized globally-adaptive quadrature         |
|  [02]   | `quadgk(order=21)`                              | Gauss-Kronrod adaptive      | finite/infinite interval; `order ∈ {15,21,31,41,51,61}` |
|  [03]   | `quadcc(order=32)`                              | Clenshaw-Curtis adaptive    | `order ∈ {8,16,32,64,128,256}`                          |
|  [04]   | `quadts(order=61)`                              | tanh-sinh adaptive          | singular endpoints; `order ∈ {41,61,81,101}`            |
|  [05]   | `romberg(divmax=20)`                            | Romberg                     | Richardson-extrapolated trapezoidal                     |
|  [06]   | `rombergts(divmax=20)`                          | tanh-sinh Romberg           | Romberg over tanh-sinh nodes (singular/infinite)        |

[ENTRYPOINT_SCOPE]: fixed-order non-adaptive quadrature
- rail: quadrature
- each fixed integrator applies a single rule at a fixed order with no panel subdivision; it returns `(value, QuadratureInfo)` like the adaptive family but evaluates a known number of nodes for a constant-cost, fully `vmap`-friendly integral. Each takes `(fun, a, b, args=(), norm=inf, n=...)` -> `(value, QuadratureInfo)` over scalar bounds `a, b` (not an `interval`).

| [INDEX] | [SURFACE]            | [ENTRY_FAMILY]        | [RAIL]                                      |
| :-----: | :------------------- | :-------------------- | :------------------------------------------ |
|  [01]   | `fixed_quadgk(n=21)` | Gauss-Kronrod fixed   | fixed-order Gauss-Kronrod, no subdivision   |
|  [02]   | `fixed_quadcc(n=32)` | Clenshaw-Curtis fixed | fixed-order Clenshaw-Curtis, no subdivision |
|  [03]   | `fixed_quadts(n=61)` | tanh-sinh fixed       | fixed-order tanh-sinh, no subdivision       |

[ENTRYPOINT_SCOPE]: sampled-data integration
- rail: quadrature
- defined in the `quadax.sampled` submodule and re-exported at the top-level `quadax` namespace; these integrate already-sampled (non-callable) data over its abscissae and return a bare `jax.Array` — no callable integrand and no `QuadratureInfo` receipt. They are the rail for integrating discretized field samples rather than a JAX-traceable integrand. Each takes `(y, *, x=None, dx=1.0, axis=-1)` (cumulative forms add keyword-only `initial=None`) -> `jax.Array`.

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]                 | [RAIL]                                               |
| :-----: | :--------------------- | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `trapezoid`            | sampled trapezoidal            | composite trapezoidal integral of `y` over `x`/`dx`  |
|  [02]   | `cumulative_trapezoid` | sampled cumulative trapezoidal | running trapezoidal integral (`initial` seeds total) |
|  [03]   | `simpson`              | sampled Simpson                | composite Simpson integral of `y` over `x`/`dx`      |
|  [04]   | `cumulative_simpson`   | sampled cumulative Simpson     | running Simpson integral of `y` along `axis`         |

## [04]-[IMPLEMENTATION_LAW]

[QUADRATURE_TOPOLOGY]:
- namespace: `quadax`; integrators, rule classes, and `STATUS` at top level; sampled-data integrators in `quadax.sampled` (also top-level re-exported); `QuadratureInfo` only in `quadax.utils`; precomputed node/weight tables in `quadax.quad_weights`
- receipt law: every callable-integrand integrator (adaptive and fixed-order) returns a `(value, QuadratureInfo)` pair; the `QuadratureInfo` NamedTuple carries `err`/`neval`/`status`/`info` and is never discarded into a bare scalar return. The sampled-data family is the sole exception: it returns a bare `jax.Array` because pre-sampled data carries no per-call evaluation diagnostics
- status law: `QuadratureInfo.status` is an integer bitfield decoded through the `STATUS` table (flags NORMAL_EXIT/MAX_NINTER/ROUNDOFF/BAD_INTEGRAND/NO_CONVERGE combine, so any code 0-31 maps to the union of its set flags); convergence is read from the decoded status, never inferred from the value. `full_output=True` widens `info` with per-subinterval diagnostics
- rule law: `AbstractQuadratureRule` is the polymorphic base (carrying `integrate`/`norm`); `GaussKronrodRule` / `ClenshawCurtisRule` / `TanhSinhRule` own node/weight construction for one family each, and `NestedRule` pairs a high-order rule with an embedded lower-order estimate for the adaptive error signal; the adaptive driver composes a rule instance rather than re-deriving nodes per call
- driver law: `adaptive_quadrature(rule, fun, interval, ...)` is the canonical polymorphic adaptive surface — one driver discriminating the family by the `rule` instance. `quadgk` / `quadcc` / `quadts` are named convenience wrappers that build the matching rule internally from an `order` keyword and call the driver; they do NOT accept a `rule` argument. `fixed_quad{gk,cc,ts}` apply the same rules at a fixed node count `n` over scalar bounds `a, b` with no subdivision
- selection law: `quadgk` for smooth integrands, `quadts` for endpoint-singular or infinite-range integrands, `quadcc` for oscillatory/Chebyshev-friendly integrands, `romberg`/`rombergts` for cheap smooth or singular extrapolated integrals, the `fixed_quad*` family for constant-cost `vmap`-friendly integrals, and the `sampled` family for already-discretized data; one polymorphic integrand passes to whichever integrator the problem selects
- differentiation law: callable-integrand integration is JIT-compatible and differentiable through the integrand and interval bounds under both forward and reverse mode; the integrand is a pure JAX function, never a host callback

[QUADRATURE_STACKING]:
- jax ↔ quadax: the integrand is a pure `Callable[..., jax.Array]`; `jax.jit` wraps the integrator, `jax.grad`/`jax.jacfwd` differentiate the result through the integrand and bounds, and `jax.vmap` batches a `fixed_quad*` call across a parameter axis for a constant-cost vectorized integral — the `fixed_quad*` family (constant node count) is the `vmap`-friendly choice because adaptive subdivision introduces data-dependent control flow.
- equinox ↔ quadax: `quadax` is built on `equinox` (the rule classes are `eqx.Module`s); a quadrature inside a larger `eqx.Module` model is partitioned with `eqx.partition`/`eqx.filter_jit` exactly like any other JAX leaf, and the integrand can close over `eqx.Module` parameters that gradients flow into.
- diffrax/optimistix ↔ quadax: a parametric integral nested in a differential-equation right-hand-side (`diffrax`) or an optimization objective (`optimistix`/`optax`) is differentiated end-to-end because `quadax` preserves the JAX trace; the `(value, QuadratureInfo)` pair lets the outer solver read convergence (`STATUS`) before trusting the value in its own residual.
- receipt rail: the algorithm receipt captures the integrator name, `order`/`n`, `epsabs`/`epsrel`, the returned `QuadratureInfo.err`/`neval`, and the decoded `STATUS` string — never a bare value, so a downstream consumer can gate on convergence rather than re-estimating the error.

[LOCAL_ADMISSION]:
- quadax is admitted as the compute quadrature owner; solvers/quadrature composes the adaptive integrators directly and propagates the `QuadratureInfo` receipt rather than collapsing it to a value
- `QuadratureInfo` is imported from `quadax.utils` when the receipt is named in a signature/annotation; it is not exposed on the top-level `quadax` namespace
- the integrand is the differentiable surface — gradients flow through `fun` and the interval bounds under forward and reverse mode, so solvers/field passes a JAX-traceable integrand and never a NumPy/SciPy closure; `jax.vmap` over `fixed_quad*` (constant node count) is the batched-integral form
- adaptive tolerances (`epsabs`, `epsrel`), `max_ninter` (adaptive) / `divmax` (Romberg) limits, and `order` (adaptive) / `n` (fixed) node counts are integrator keyword arguments; convergence is read from the decoded `QuadratureInfo.status` bitfield, never inferred from the value alone

[RAIL_LAW]:
- Package: `quadax`
- Owns: JAX-native adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature via the polymorphic `adaptive_quadrature` driver, Romberg / tanh-sinh-Romberg integration, fixed-order `fixed_quad*` integrators, the `AbstractQuadratureRule` rule hierarchy, the `QuadratureInfo` receipt and `STATUS` decode table, and the `sampled`-data trapezoidal/Simpson integrators
- Accept: `adaptive_quadrature` as the canonical rule-parameterized adaptive entry (with `quadgk` / `quadcc` / `quadts` / `romberg` / `rombergts` and the `fixed_quad{gk,cc,ts}` family as named specializations), the `(value, QuadratureInfo)` receipt pair decoded through `STATUS`, rule classes composed by the driver, a pure JAX-traceable integrand for differentiable integration, and the `sampled` family (`trapezoid` / `cumulative_trapezoid` / `simpson` / `cumulative_simpson`) for already-discretized field samples
- Reject: `scipy.integrate.quad` or other host-callback quadrature when a JIT-compatible, integrand-differentiable integral is required; discarding the `QuadratureInfo` receipt or inferring convergence from the value instead of the decoded `STATUS`; treating `QuadratureInfo` as a top-level `quadax` export; a parallel local quadrature kernel duplicating the admitted rule classes; a hand-rolled trapezoidal/Simpson kernel when the `sampled` family already owns sampled-data integration
- Usage: deferred consumer `[INTERPAX_QUADAX_USAGE]` ([BLOCKED]) in compute solvers/quadrature and solvers/field
