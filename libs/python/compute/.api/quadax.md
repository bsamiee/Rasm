# [PY_COMPUTE_API_QUADAX]

`quadax` owns JAX-native adaptive numerical quadrature for the compute integration rail: globally-adaptive Gauss-Kronrod, Clenshaw-Curtis, and tanh-sinh integrators over Romberg, fixed-order, and sampled-data rules, each callable-integrand result paired with its `QuadratureInfo`. Every callable-integrand integration stays JIT-compatible and differentiable through the integrand and interval bounds under forward and reverse mode.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quadrature result carrier

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]               | [CAPABILITY]                                        |
| :-----: | :---------------------------- | :-------------------------- | :-------------------------------------------------- |
|  [01]   | `quadax.utils.QuadratureInfo` | result carrier (NamedTuple) | carrier fields `err`/`neval`/`status`/`info`        |
|  [02]   | `STATUS`                      | decode table                | `dict[int, str]` status-code -> convergence message |

[PUBLIC_TYPE_SCOPE]: fixed-rule quadrature classes

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `AbstractQuadratureRule` | rule base     | abstract base `adaptive_quadrature` dispatches on     |
|  [02]   | `GaussKronrodRule`       | fixed rule    | Gauss-Kronrod node/weight rule, embedded error        |
|  [03]   | `ClenshawCurtisRule`     | fixed rule    | Clenshaw-Curtis (Chebyshev-node) nested-error rule    |
|  [04]   | `TanhSinhRule`           | fixed rule    | tanh-sinh (double-exponential) endpoint-singular rule |
|  [05]   | `NestedRule`             | nested rule   | pairs high-order + embedded lower-order estimate      |

## [02]-[ENTRYPOINTS]

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
- each takes `(y, *, x=None, dx=1.0, axis=-1)` -> `jax.Array` (cumulative forms add `initial=None`); these integrate already-sampled non-callable data over its abscissae and return a bare array with no `QuadratureInfo`, the rail for discretized field samples rather than a traceable integrand

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY]                 | [RAIL]                                               |
| :-----: | :--------------------- | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `trapezoid`            | sampled trapezoidal            | composite trapezoidal integral of `y` over `x`/`dx`  |
|  [02]   | `cumulative_trapezoid` | sampled cumulative trapezoidal | running trapezoidal integral (`initial` seeds total) |
|  [03]   | `simpson`              | sampled Simpson                | composite Simpson integral of `y` over `x`/`dx`      |
|  [04]   | `cumulative_simpson`   | sampled cumulative Simpson     | running Simpson integral of `y` along `axis`         |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace: `quadax` — integrators, rule classes, and `STATUS` at top level; sampled-data integrators in `quadax.sampled` (top-level re-exported); `romberg`/`rombergts` in `quadax.romberg`; `QuadratureInfo` only in `quadax.utils`; precomputed node/weight tables in `quadax.quad_weights`
- carrier law: every callable-integrand integrator (adaptive and fixed-order) returns a `(value, QuadratureInfo)` pair carrying `err`/`neval`/`status`/`info`, never a bare scalar; the sampled-data family is the sole exception, returning a bare `jax.Array` because pre-sampled data carries no per-call diagnostics
- status law: `QuadratureInfo.status` is a 5-bit integer bitfield decoded through the `STATUS` table (flags combine, so any code 0-31 maps to the union of set flags); convergence reads from the decoded status, never the value, and `full_output=True` widens `info` with per-subinterval diagnostics
- rule law: `AbstractQuadratureRule` is the polymorphic base carrying `integrate`/`norm`; `GaussKronrodRule`/`ClenshawCurtisRule`/`TanhSinhRule` own node/weight construction one family each, and `NestedRule` pairs a high-order rule with an embedded lower-order estimate for the adaptive error signal; the driver composes a rule instance rather than re-deriving nodes per call
- driver law: `adaptive_quadrature(rule, fun, interval, ...)` is the canonical polymorphic adaptive surface discriminating the family by the `rule` instance; `quadgk`/`quadcc`/`quadts` build the matching rule from an `order` keyword and call the driver, taking no `rule` argument, and `fixed_quad{gk,cc,ts}` apply the same rules at a fixed node count `n` over scalar bounds with no subdivision
- selection law: `quadgk` integrates smooth integrands, `quadts` endpoint-singular or infinite-range integrands, `quadcc` oscillatory/Chebyshev-friendly integrands, `romberg`/`rombergts` cheap smooth or singular extrapolated integrals, `fixed_quad*` constant-cost `vmap`-friendly integrals, and `sampled` already-discretized data — one polymorphic integrand passes to whichever integrator the problem selects

[STACKING]:
- `jax`(`.api/jax.md`): the integrand is a pure `Callable[..., jax.Array]`, so `jax.jit` wraps the integrator, `jax.grad`/`jax.jacfwd` differentiate through the integrand and bounds, and `jax.vmap` batches a `fixed_quad*` call across a parameter axis — `fixed_quad*` (constant node count) is the `vmap`-friendly choice because adaptive subdivision introduces data-dependent control flow
- `equinox`(`.api/equinox.md`): the rule classes are `eqx.Module`s, so a quadrature inside a larger model partitions under `eqx.partition`/`eqx.filter_jit` like any JAX leaf and the integrand closes over `eqx.Module` parameters gradients flow into
- `diffrax`(`.api/diffrax.md`), `optimistix`(`.api/optimistix.md`): a parametric integral nested in a `diffrax` right-hand-side or an `optimistix`/`optax` objective differentiates end-to-end because `quadax` preserves the JAX trace, and the `(value, QuadratureInfo)` pair lets the outer solver read `STATUS` before trusting the value
- `compute` solvers/quadrature: `Solve.Iterative` captures the integrator name, `order`/`n`, `epsabs`/`epsrel`, the returned `QuadratureInfo.err`/`neval`, and the decoded `STATUS`, never a bare value, so a consumer gates on convergence rather than re-estimating the error

[LOCAL_ADMISSION]:
- adaptive, fixed-order, and sampled integration routes to `quadax`; solvers/quadrature composes the integrators directly and propagates the `QuadratureInfo` rather than collapsing it to a value
- a pure JAX-traceable integrand routes to `quadax`; a NumPy/SciPy closure is rejected, and `jax.vmap` over `fixed_quad*` (constant node count) is the batched-integral form
