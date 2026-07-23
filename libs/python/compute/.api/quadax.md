# [PY_COMPUTE_API_QUADAX]

`quadax` owns JAX-native adaptive numerical quadrature for the compute integration rail: globally-adaptive Gauss-Kronrod, Clenshaw-Curtis, and tanh-sinh integrators over Romberg, fixed-order, and sampled-data rules, each callable-integrand result paired with a `QuadratureInfo` receipt. Every callable-integrand integration stays JIT-compatible and differentiable through the integrand and interval bounds under forward and reverse mode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `quadax`
- package: `quadax`
- import: `quadax`
- owner: `compute`
- rail: quadrature
- capability: globally-adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature, Romberg and tanh-sinh-Romberg integration, fixed-order rule classes, sampled-data trapezoidal/Simpson integration, and JIT-compatible forward-and-reverse-differentiable integrate pipelines over a JAX integrand

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quadrature result receipt

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]               | [CAPABILITY]                                        |
| :-----: | :---------------------------- | :-------------------------- | :-------------------------------------------------- |
|  [01]   | `quadax.utils.QuadratureInfo` | result carrier (NamedTuple) | receipt fields `err`/`neval`/`status`/`info`        |
|  [02]   | `STATUS`                      | decode table                | `dict[int, str]` status-code -> convergence message |

[PUBLIC_TYPE_SCOPE]: fixed-rule quadrature classes

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `AbstractQuadratureRule` | rule base     | abstract base `adaptive_quadrature` dispatches on     |
|  [02]   | `GaussKronrodRule`       | fixed rule    | Gauss-Kronrod node/weight rule, embedded error        |
|  [03]   | `ClenshawCurtisRule`     | fixed rule    | Clenshaw-Curtis (Chebyshev-node) nested-error rule    |
|  [04]   | `TanhSinhRule`           | fixed rule    | tanh-sinh (double-exponential) endpoint-singular rule |
|  [05]   | `NestedRule`             | nested rule   | pairs high-order + embedded lower-order estimate      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: adaptive quadrature
- adaptive integrators share `(fun, interval, args=(), full_output=False, epsabs=None, epsrel=None, max_ninter=50, norm=inf)` -> `(value, QuadratureInfo)`; `adaptive_quadrature` prepends `rule` and adds `**kwargs`, `romberg`/`rombergts` swap `max_ninter` for `divmax=20`

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]              | [RAIL]                                                  |
| :-----: | :---------------------------------------------- | :-------------------------- | :------------------------------------------------------ |
|  [01]   | `adaptive_quadrature(rule, fun, interval, ...)` | polymorphic adaptive driver | rule-parameterized globally-adaptive quadrature         |
|  [02]   | `quadgk(order=21)`                              | Gauss-Kronrod adaptive      | finite/infinite interval; `order ∈ {15,21,31,41,51,61}` |
|  [03]   | `quadcc(order=32)`                              | Clenshaw-Curtis adaptive    | `order ∈ {8,16,32,64,128,256}`                          |
|  [04]   | `quadts(order=61)`                              | tanh-sinh adaptive          | singular endpoints; `order ∈ {41,61,81,101}`            |
|  [05]   | `romberg(divmax=20)`                            | Romberg                     | Richardson-extrapolated trapezoidal                     |
|  [06]   | `rombergts(divmax=20)`                          | tanh-sinh Romberg           | Romberg over tanh-sinh nodes (singular/infinite)        |

[ENTRYPOINT_SCOPE]: fixed-order non-adaptive quadrature
- each takes `(fun, a, b, args=(), norm=inf, n=...)` -> `(value, QuadratureInfo)` over scalar bounds `a, b`, applying one rule at a fixed node count with no panel subdivision for a constant-cost, `vmap`-friendly integral

| [INDEX] | [SURFACE]            | [ENTRY_FAMILY]        | [RAIL]                                      |
| :-----: | :------------------- | :-------------------- | :------------------------------------------ |
|  [01]   | `fixed_quadgk(n=21)` | Gauss-Kronrod fixed   | fixed-order Gauss-Kronrod, no subdivision   |
|  [02]   | `fixed_quadcc(n=32)` | Clenshaw-Curtis fixed | fixed-order Clenshaw-Curtis, no subdivision |
|  [03]   | `fixed_quadts(n=61)` | tanh-sinh fixed       | fixed-order tanh-sinh, no subdivision       |

[ENTRYPOINT_SCOPE]: sampled-data integration
- each takes `(y, *, x=None, dx=1.0, axis=-1)` -> `jax.Array` (cumulative forms add `initial=None`); these integrate already-sampled non-callable data over its abscissae and return a bare array with no `QuadratureInfo` receipt, the rail for discretized field samples rather than a traceable integrand

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]                 | [RAIL]                                               |
| :-----: | :--------------------- | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `trapezoid`            | sampled trapezoidal            | composite trapezoidal integral of `y` over `x`/`dx`  |
|  [02]   | `cumulative_trapezoid` | sampled cumulative trapezoidal | running trapezoidal integral (`initial` seeds total) |
|  [03]   | `simpson`              | sampled Simpson                | composite Simpson integral of `y` over `x`/`dx`      |
|  [04]   | `cumulative_simpson`   | sampled cumulative Simpson     | running Simpson integral of `y` along `axis`         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace: `quadax` — integrators, rule classes, and `STATUS` at top level; sampled-data integrators in `quadax.sampled` (top-level re-exported); `romberg`/`rombergts` in `quadax.romberg`; `QuadratureInfo` only in `quadax.utils`; precomputed node/weight tables in `quadax.quad_weights`
- receipt law: every callable-integrand integrator (adaptive and fixed-order) returns a `(value, QuadratureInfo)` pair carrying `err`/`neval`/`status`/`info`, never a bare scalar; the sampled-data family is the sole exception, returning a bare `jax.Array` because pre-sampled data carries no per-call diagnostics
- status law: `QuadratureInfo.status` is a 5-bit integer bitfield decoded through the `STATUS` table (flags combine, so any code 0-31 maps to the union of set flags); convergence reads from the decoded status, never the value, and `full_output=True` widens `info` with per-subinterval diagnostics
- rule law: `AbstractQuadratureRule` is the polymorphic base carrying `integrate`/`norm`; `GaussKronrodRule`/`ClenshawCurtisRule`/`TanhSinhRule` own node/weight construction one family each, and `NestedRule` pairs a high-order rule with an embedded lower-order estimate for the adaptive error signal; the driver composes a rule instance rather than re-deriving nodes per call
- driver law: `adaptive_quadrature(rule, fun, interval, ...)` is the canonical polymorphic adaptive surface discriminating the family by the `rule` instance; `quadgk`/`quadcc`/`quadts` build the matching rule from an `order` keyword and call the driver, taking no `rule` argument, and `fixed_quad{gk,cc,ts}` apply the same rules at a fixed node count `n` over scalar bounds with no subdivision
- selection law: `quadgk` integrates smooth integrands, `quadts` endpoint-singular or infinite-range integrands, `quadcc` oscillatory/Chebyshev-friendly integrands, `romberg`/`rombergts` cheap smooth or singular extrapolated integrals, `fixed_quad*` constant-cost `vmap`-friendly integrals, and `sampled` already-discretized data — one polymorphic integrand passes to whichever integrator the problem selects

[STACKING]:
- `jax`(`.api/jax.md`): the integrand is a pure `Callable[..., jax.Array]`, so `jax.jit` wraps the integrator, `jax.grad`/`jax.jacfwd` differentiate through the integrand and bounds, and `jax.vmap` batches a `fixed_quad*` call across a parameter axis — `fixed_quad*` (constant node count) is the `vmap`-friendly choice because adaptive subdivision introduces data-dependent control flow
- `equinox`(`.api/equinox.md`): the rule classes are `eqx.Module`s, so a quadrature inside a larger model partitions under `eqx.partition`/`eqx.filter_jit` like any JAX leaf and the integrand closes over `eqx.Module` parameters gradients flow into
- `diffrax`(`.api/diffrax.md`), `optimistix`(`.api/optimistix.md`): a parametric integral nested in a `diffrax` right-hand-side or an `optimistix`/`optax` objective differentiates end-to-end because `quadax` preserves the JAX trace, and the `(value, QuadratureInfo)` pair lets the outer solver read `STATUS` before trusting the value
- `compute` solvers/quadrature: the algorithm receipt captures the integrator name, `order`/`n`, `epsabs`/`epsrel`, the returned `QuadratureInfo.err`/`neval`, and the decoded `STATUS`, never a bare value, so a consumer gates on convergence rather than re-estimating the error

[LOCAL_ADMISSION]:
- adaptive, fixed-order, and sampled integration routes to `quadax`; solvers/quadrature composes the integrators directly and propagates the `QuadratureInfo` receipt rather than collapsing it to a value
- a pure JAX-traceable integrand routes to `quadax`; a NumPy/SciPy closure is rejected, and `jax.vmap` over `fixed_quad*` (constant node count) is the batched-integral form

[RAIL_LAW]:
- Package: `quadax`
- Owns: JAX-native adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature via the polymorphic `adaptive_quadrature` driver, Romberg / tanh-sinh-Romberg integration, fixed-order `fixed_quad*` integrators, the `AbstractQuadratureRule` hierarchy, the `QuadratureInfo` receipt and `STATUS` decode table, and the `sampled`-data trapezoidal/Simpson integrators
- Accept: `adaptive_quadrature` as the canonical rule-parameterized adaptive entry (`quadgk`/`quadcc`/`quadts`/`romberg`/`rombergts` and `fixed_quad{gk,cc,ts}` as named specializations), the `(value, QuadratureInfo)` pair decoded through `STATUS`, rule classes composed by the driver, a pure JAX-traceable integrand, and the `sampled` family for already-discretized field samples
- Reject: `scipy.integrate.quad` or other host-callback quadrature where a JIT-compatible, integrand-differentiable integral is required; discarding the `QuadratureInfo` receipt or inferring convergence from the value; a parallel local quadrature kernel duplicating the admitted rules or a hand-rolled trapezoidal/Simpson kernel the `sampled` family owns
