# [PY_COMPUTE_API_QUADAX]

`quadax` supplies JAX-native adaptive numerical quadrature for the compute integration rail: globally-adaptive Gauss-Kronrod (`quadgk`), Clenshaw-Curtis (`quadcc`), and tanh-sinh (`quadts`) integrators plus Romberg integration, each returning a value paired with a `QuadratureInfo` receipt; every integration is JIT-compatible and differentiable through the integrand under forward/reverse-mode autodiff.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `quadax`
- package: `quadax`
- import: `quadax`
- owner: `compute`
- rail: quadrature
- installed: `0.2.13` authored from ledger ([04]-sourced; `assay api` resolution blocked by the `opentelemetry-proto` `protobuf>=5,<7` ceiling against the workspace `protobuf>=7.35` floor — a workspace resolution conflict, not a quadax or interpreter fault); license MIT; pure-Python but JAX-dependent, marker-gated `python_version<'3.15'` (jaxlib ships no cp315 wheel) => GATED
- capability: globally-adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature, Romberg integration, fixed-rule quadrature classes, and JIT-compatible integrate-and-differentiate pipelines over a JAX integrand

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quadrature result receipt
- rail: quadrature

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                                                 |
| :-----: | :--------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `QuadratureInfo` | result carrier | per-integration receipt: `err` (estimated error), `neval` (integrand evaluations), `status` (convergence code), `info` (solver diagnostics)  |
|  [02]   | `STATUS`         | decode table   | `dict[int, str]` mapping `QuadratureInfo.status` codes to human-readable convergence messages; consumed to interpret the receipt status code |

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

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY]              | [RAIL]                                                                                                                   |
| :-----: | :-------------------------------------------------------------------------- | :-------------------------- | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `adaptive_quadrature(rule, fun, interval, ...)` → `(value, QuadratureInfo)` | polymorphic adaptive driver | rule-parameterized globally-adaptive quadrature; the rule selects the family that `quadgk`/`quadcc`/`quadts` delegate to |
|  [02]   | `quadgk(fun, interval, ...)` → `(value, QuadratureInfo)`                    | Gauss-Kronrod adaptive      | globally-adaptive Gauss-Kronrod over a finite/infinite interval                                                          |
|  [03]   | `quadcc(fun, interval, ...)` → `(value, QuadratureInfo)`                    | Clenshaw-Curtis adaptive    | globally-adaptive Clenshaw-Curtis quadrature                                                                             |
|  [04]   | `quadts(fun, interval, ...)` → `(value, QuadratureInfo)`                    | tanh-sinh adaptive          | globally-adaptive tanh-sinh quadrature for singular endpoints                                                            |
|  [05]   | `romberg(fun, interval, ...)` → `(value, QuadratureInfo)`                   | Romberg                     | Richardson-extrapolated trapezoidal (Romberg) integration                                                                |
|  [06]   | `rombergts(fun, interval, ...)` → `(value, QuadratureInfo)`                 | tanh-sinh Romberg           | Romberg extrapolation over tanh-sinh nodes for endpoint-singular/infinite-range integrands                               |

[ENTRYPOINT_SCOPE]: fixed-order non-adaptive quadrature
- rail: quadrature
- each fixed integrator applies a single rule at a fixed order with no panel subdivision; it returns `(value, QuadratureInfo)` like the adaptive family but evaluates a known number of nodes for a constant-cost, fully `vmap`-friendly integral.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]        | [RAIL]                                          |
| :-----: | :------------------------------------------------------------- | :-------------------- | :---------------------------------------------- |
|  [01]   | `fixed_quadgk(fun, interval, ...)` → `(value, QuadratureInfo)` | Gauss-Kronrod fixed   | fixed-order Gauss-Kronrod with no subdivision   |
|  [02]   | `fixed_quadcc(fun, interval, ...)` → `(value, QuadratureInfo)` | Clenshaw-Curtis fixed | fixed-order Clenshaw-Curtis with no subdivision |
|  [03]   | `fixed_quadts(fun, interval, ...)` → `(value, QuadratureInfo)` | tanh-sinh fixed       | fixed-order tanh-sinh with no subdivision       |

[ENTRYPOINT_SCOPE]: sampled-data integration
- rail: quadrature
- from the `quadax.sampled` submodule; these integrate already-sampled (non-callable) data over its abscissae and return a bare array — no callable integrand and no `QuadratureInfo` receipt. They are the rail for integrating discretized field samples rather than a JAX-traceable integrand.

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]                 | [RAIL]                                                                                |
| :-----: | :------------------------------------------------------ | :----------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `trapezoid(y, x, dx, axis)` → array                     | sampled trapezoidal            | composite trapezoidal integral of samples `y` over `x` (or spacing `dx`) along `axis` |
|  [02]   | `cumulative_trapezoid(y, x, dx, axis, initial)` → array | sampled cumulative trapezoidal | running trapezoidal integral of `y` along `axis`                                      |
|  [03]   | `simpson(y, x, dx, axis)` → array                       | sampled Simpson                | composite Simpson integral of samples `y` over `x` (or spacing `dx`) along `axis`     |
|  [04]   | `cumulative_simpson(y, x, dx, axis, initial)` → array   | sampled cumulative Simpson     | running Simpson integral of `y` along `axis`                                          |

## [04]-[IMPLEMENTATION_LAW]

[QUADRATURE_TOPOLOGY]:
- namespace: `quadax`; integrators and rule classes at top level, sampled-data integrators in the `quadax.sampled` submodule
- receipt law: every callable-integrand integrator (adaptive and fixed-order) returns a `(value, QuadratureInfo)` pair; the `QuadratureInfo` receipt carries the estimated error, evaluation count, and convergence status and is never discarded into a bare scalar return. The sampled-data family is the sole exception: it returns a bare array because pre-sampled data carries no per-call evaluation diagnostics
- status law: `QuadratureInfo.status` is an integer convergence code decoded through the `STATUS` table; convergence is read from the decoded status, never inferred from the value
- rule law: `AbstractQuadratureRule` is the polymorphic base; `GaussKronrodRule` / `ClenshawCurtisRule` / `TanhSinhRule` / `NestedRule` own node/weight construction for one family each; the adaptive driver composes a rule rather than re-deriving nodes per call
- driver law: `adaptive_quadrature(rule, fun, interval, ...)` is the canonical polymorphic adaptive surface — one driver discriminating the family by the `rule` instance; `quadgk` / `quadcc` / `quadts` construct the matching rule and delegate to it; `fixed_quad{gk,cc,ts}` apply the same rules at a fixed order with no subdivision
- selection law: `quadgk` for smooth integrands, `quadts` for endpoint-singular or infinite-range integrands, `quadcc` for oscillatory/Chebyshev-friendly integrands, `romberg`/`rombergts` for cheap smooth or singular extrapolated integrals, the `fixed_quad*` family for constant-cost `vmap`-friendly integrals, and the `sampled` family for already-discretized data; one polymorphic integrand passes to whichever integrator the problem selects
- differentiation law: callable-integrand integration is JIT-compatible and differentiable through the integrand and interval bounds; the integrand is a pure JAX function, never a host callback

[LOCAL_ADMISSION]:
- quadax is admitted as the compute quadrature owner; solvers/quadrature composes the adaptive integrators directly and propagates the `QuadratureInfo` receipt rather than collapsing it to a value
- pure-Python wheel but JAX-dependent: gated `python_version<'3.15'` because jaxlib ships no cp315 wheel; the package runs only in the marker-gated band, never on the cp315 core
- the integrand is the differentiable surface — gradients flow through `fun` and the interval bounds, so solvers/field passes a JAX-traceable integrand and never a NumPy/SciPy closure
- adaptive tolerances (`epsabs`, `epsrel`) and panel/order limits are integrator keyword arguments; convergence is read from `QuadratureInfo.status`, never inferred from the value alone

[RAIL_LAW]:
- Package: `quadax`
- Owns: JAX-native adaptive Gauss-Kronrod / Clenshaw-Curtis / tanh-sinh quadrature via the polymorphic `adaptive_quadrature` driver, Romberg / tanh-sinh-Romberg integration, fixed-order `fixed_quad*` integrators, the `AbstractQuadratureRule` rule hierarchy, the `QuadratureInfo` receipt and `STATUS` decode table, and the `sampled`-data trapezoidal/Simpson integrators
- Accept: `adaptive_quadrature` as the canonical rule-parameterized adaptive entry (with `quadgk` / `quadcc` / `quadts` / `romberg` / `rombergts` and the `fixed_quad{gk,cc,ts}` family as named specializations), the `(value, QuadratureInfo)` receipt pair decoded through `STATUS`, rule classes composed by the driver, a pure JAX-traceable integrand for differentiable integration, and the `sampled` family (`trapezoid` / `cumulative_trapezoid` / `simpson` / `cumulative_simpson`) for already-discretized field samples
- Reject: `scipy.integrate.quad` or other host-callback quadrature when a JIT-compatible, integrand-differentiable integral is required; discarding the `QuadratureInfo` receipt or inferring convergence from the value instead of the decoded `STATUS`; a parallel local quadrature kernel duplicating the admitted rule classes; a hand-rolled trapezoidal/Simpson kernel when the `sampled` family already owns sampled-data integration
- Usage: deferred consumer `[INTERPAX_QUADAX_USAGE]` ([BLOCKED]) in compute solvers/quadrature and solvers/field
