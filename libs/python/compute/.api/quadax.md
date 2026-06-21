# [PY_COMPUTE_API_QUADAX]

`quadax` supplies JAX-native adaptive numerical quadrature for the compute integration rail: globally-adaptive Gauss-Kronrod (`quadgk`), Clenshaw-Curtis (`quadcc`), and tanh-sinh (`quadts`) integrators plus Romberg integration, each returning a value paired with a `QuadratureInfo` receipt; every integration is JIT-compatible and differentiable through the integrand under forward/reverse-mode autodiff.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `quadax`
- package: `quadax`
- import: `quadax`
- owner: `compute`
- rail: quadrature
- namespace: `quadax` (integrators, rule classes, `STATUS`, and the `sampled` integrators re-exported at top level; `QuadratureInfo` lives in `quadax.utils` and is NOT a top-level export); submodules `quadax.adaptive`, `quadax.fixed_order`, `quadax.sampled`, `quadax.quad_weights`, `quadax.utils`
- installed: `0.2.13`; license MIT; wheel `py3-none-any` (pure-Python source) but `jax`/`jaxlib`/`equinox`-dependent at runtime
- gate: `[GATED]` `; python_version<'3.15'` — pure-Python itself, but `jaxlib` ships no cp315 wheel, so the JAX-traceable quadrature surface runs only on the companion interpreter band, never the cp315 core
- requires: `jax`, `jaxlib`, `equinox`
- capability: globally-adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature, Romberg and tanh-sinh-Romberg integration, fixed-order rule classes, sampled-data trapezoidal/Simpson integration, and JIT-compatible / forward-and-reverse-differentiable integrate pipelines over a JAX integrand

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quadrature result receipt
- rail: quadrature

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                                                 |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `quadax.utils.QuadratureInfo` | result carrier (NamedTuple) | per-integration receipt with fields `err` (estimated error), `neval` (integrand evaluations), `status` (convergence code), `info` (solver diagnostics); a `typing.NamedTuple` defined in `quadax.utils`, NOT a top-level `quadax` export — import via `from quadax.utils import QuadratureInfo` if the type is named, else accept it positionally as the second tuple element |
|  [02]   | `STATUS`                 | decode table   | `dict[int, str]` mapping `QuadratureInfo.status` codes to human-readable convergence messages; top-level re-export of `quadax.utils.STATUS`; the status is a bitfield combining NORMAL_EXIT/MAX_NINTER/ROUNDOFF/BAD_INTEGRAND/NO_CONVERGE flags, so codes 0-31 each decode to the union of set flags |

[PUBLIC_TYPE_SCOPE]: fixed-rule quadrature classes
- rail: quadrature
- each rule owns the node/weight construction for one quadrature family and is composed by the adaptive integrators; a rule is constructed once and reused across panels. `AbstractQuadratureRule` is the polymorphic base; `adaptive_quadrature` discriminates the family by the rule instance it is given.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                                                        |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `AbstractQuadratureRule` | rule base     | abstract base for fixed-order node/weight rules; the type `adaptive_quadrature` dispatches on       |
|  [02]   | `GaussKronrodRule`       | fixed rule    | Gauss-Kronrod node/weight rule with embedded error estimate                                         |
|  [03]   | `ClenshawCurtisRule`     | fixed rule    | Clenshaw-Curtis (Chebyshev-node) rule with nested error estimate                                    |
|  [04]   | `TanhSinhRule`           | fixed rule    | tanh-sinh (double-exponential) rule for endpoint-singular integrands                                |
|  [05]   | `NestedRule`             | nested rule   | nested fixed rule pairing a high-order and embedded lower-order estimate for adaptive error control |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: adaptive quadrature
- rail: quadrature
- each integrator returns `(value, QuadratureInfo)`; `fun` is a JAX-traceable scalar/array integrand and the integration is differentiable through `fun` and the interval bounds.
- `adaptive_quadrature` is the canonical polymorphic driver: one adaptive surface discriminating the quadrature family by the rule instance passed as `rule`. `quadgk` / `quadcc` / `quadts` are named specializations that construct the matching rule and delegate to it.

| [INDEX] | [SURFACE]                                                                                                                  | [ENTRY_FAMILY]              | [RAIL]                                                                                                                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------- | :-------------------------- | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `adaptive_quadrature(rule, fun, interval, args=(), full_output=False, epsabs=None, epsrel=None, max_ninter=50, norm=inf, **kwargs)` → `(value, QuadratureInfo)` | polymorphic adaptive driver | rule-parameterized globally-adaptive quadrature; `rule` is an `AbstractQuadratureRule` instance selecting the family     |
|  [02]   | `quadgk(fun, interval, args=(), full_output=False, epsabs=None, epsrel=None, max_ninter=50, order=21, norm=inf)` → `(value, QuadratureInfo)` | Gauss-Kronrod adaptive      | globally-adaptive Gauss-Kronrod over a finite/infinite interval; `order ∈ {15,21,31,41,51,61}` Kronrod nodes             |
|  [03]   | `quadcc(fun, interval, ..., order=32, norm=inf)` → `(value, QuadratureInfo)`                                               | Clenshaw-Curtis adaptive    | globally-adaptive Clenshaw-Curtis quadrature; `order ∈ {8,16,32,64,128,256}`                                             |
|  [04]   | `quadts(fun, interval, ..., order=61, norm=inf)` → `(value, QuadratureInfo)`                                               | tanh-sinh adaptive          | globally-adaptive tanh-sinh quadrature for singular endpoints; `order ∈ {41,61,81,101}`                                  |
|  [05]   | `romberg(fun, interval, args=(), full_output=False, epsabs=None, epsrel=None, divmax=20, norm=inf)` → `(value, QuadratureInfo)` | Romberg                  | Richardson-extrapolated trapezoidal (Romberg) integration; `divmax` caps the extrapolation table depth                   |
|  [06]   | `rombergts(fun, interval, ..., divmax=20, norm=inf)` → `(value, QuadratureInfo)`                                           | tanh-sinh Romberg           | Romberg extrapolation over tanh-sinh nodes for endpoint-singular/infinite-range integrands                               |

[ENTRYPOINT_SCOPE]: fixed-order non-adaptive quadrature
- rail: quadrature
- each fixed integrator applies a single rule at a fixed order with no panel subdivision; it returns `(value, QuadratureInfo)` like the adaptive family but evaluates a known number of nodes for a constant-cost, fully `vmap`-friendly integral.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]        | [RAIL]                                          |
| :-----: | :--------------------------------------------------- | :-------------------- | :---------------------------------------------- |
|  [01]   | `fixed_quadgk(fun, a, b, args=(), norm=inf, n=21)` → `(value, QuadratureInfo)` | Gauss-Kronrod fixed   | fixed-order Gauss-Kronrod with no subdivision; takes scalar bounds `a, b` (not an `interval`) and node count `n` |
|  [02]   | `fixed_quadcc(fun, a, b, args=(), norm=inf, n=32)` → `(value, QuadratureInfo)` | Clenshaw-Curtis fixed | fixed-order Clenshaw-Curtis with no subdivision |
|  [03]   | `fixed_quadts(fun, a, b, args=(), norm=inf, n=61)` → `(value, QuadratureInfo)` | tanh-sinh fixed       | fixed-order tanh-sinh with no subdivision       |

[ENTRYPOINT_SCOPE]: sampled-data integration
- rail: quadrature
- defined in the `quadax.sampled` submodule and re-exported at the top-level `quadax` namespace; these integrate already-sampled (non-callable) data over its abscissae and return a bare `jax.Array` — no callable integrand and no `QuadratureInfo` receipt. `x`/`dx`/`axis`/`initial` are keyword-only. They are the rail for integrating discretized field samples rather than a JAX-traceable integrand.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]                 | [RAIL]                                                                                |
| :-----: | :--------------------------------------------------------------------- | :----------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `trapezoid(y, *, x=None, dx=1.0, axis=-1)` → `jax.Array`               | sampled trapezoidal            | composite trapezoidal integral of samples `y` over abscissae `x` (or spacing `dx`) along `axis` |
|  [02]   | `cumulative_trapezoid(y, *, x=None, dx=1.0, axis=-1, initial=None)` → `jax.Array` | sampled cumulative trapezoidal | running trapezoidal integral of `y` along `axis` (`initial` seeds the running total)  |
|  [03]   | `simpson(y, *, x=None, dx=1.0, axis=-1)` → `jax.Array`                 | sampled Simpson                | composite Simpson integral of samples `y` over `x` (or spacing `dx`) along `axis`     |
|  [04]   | `cumulative_simpson(y, *, x=None, dx=1.0, axis=-1, initial=None)` → `jax.Array` | sampled cumulative Simpson     | running Simpson integral of `y` along `axis`                                          |

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
- pure-Python wheel but JAX-dependent: gated `python_version<'3.15'` because jaxlib ships no cp315 wheel; the package runs only in the marker-gated band, never on the cp315 core
- `QuadratureInfo` is imported from `quadax.utils` when the receipt is named in a signature/annotation; it is not exposed on the top-level `quadax` namespace
- the integrand is the differentiable surface — gradients flow through `fun` and the interval bounds under forward and reverse mode, so solvers/field passes a JAX-traceable integrand and never a NumPy/SciPy closure; `jax.vmap` over `fixed_quad*` (constant node count) is the batched-integral form
- adaptive tolerances (`epsabs`, `epsrel`), `max_ninter` (adaptive) / `divmax` (Romberg) limits, and `order` (adaptive) / `n` (fixed) node counts are integrator keyword arguments; convergence is read from the decoded `QuadratureInfo.status` bitfield, never inferred from the value alone

[RAIL_LAW]:
- Package: `quadax`
- Owns: JAX-native adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature via the polymorphic `adaptive_quadrature` driver, Romberg / tanh-sinh-Romberg integration, fixed-order `fixed_quad*` integrators, the `AbstractQuadratureRule` rule hierarchy, the `QuadratureInfo` receipt and `STATUS` decode table, and the `sampled`-data trapezoidal/Simpson integrators
- Accept: `adaptive_quadrature` as the canonical rule-parameterized adaptive entry (with `quadgk` / `quadcc` / `quadts` / `romberg` / `rombergts` and the `fixed_quad{gk,cc,ts}` family as named specializations), the `(value, QuadratureInfo)` receipt pair decoded through `STATUS`, rule classes composed by the driver, a pure JAX-traceable integrand for differentiable integration, and the `sampled` family (`trapezoid` / `cumulative_trapezoid` / `simpson` / `cumulative_simpson`) for already-discretized field samples
- Reject: `scipy.integrate.quad` or other host-callback quadrature when a JIT-compatible, integrand-differentiable integral is required; discarding the `QuadratureInfo` receipt or inferring convergence from the value instead of the decoded `STATUS`; treating `QuadratureInfo` as a top-level `quadax` export; a parallel local quadrature kernel duplicating the admitted rule classes; a hand-rolled trapezoidal/Simpson kernel when the `sampled` family already owns sampled-data integration
- License: MIT; gated `; python_version<'3.15'` (jaxlib has no cp315 wheel)
- Usage: deferred consumer `[INTERPAX_QUADAX_USAGE]` ([BLOCKED]) in compute solvers/quadrature and solvers/field
