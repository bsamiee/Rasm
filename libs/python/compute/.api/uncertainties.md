# [PY_COMPUTE_API_UNCERTAINTIES]

`uncertainties` supplies first-order (linear) error propagation over correlated quantities for the compute uncertainty rail. `ufloat` and `correlated_values` attach a standard deviation and covariance structure to study inputs; arithmetic tracks correlation automatically and derived `UFloat` values expose `nominal_value` and `std_dev`. The `uncertainties.unumpy` submodule wraps NumPy arrays of uncertain quantities through `uarray`, `nominal_values`, and `std_devs`, and `uncertainties.umath` supplies math functions that propagate uncertainty.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uncertainties`
- package: `uncertainties`
- module: `uncertainties`; submodules `uncertainties.unumpy`, `uncertainties.unumpy.ulinalg`, `uncertainties.umath`
- asset: pure Python
- owner: `compute`
- rail: uncertainty

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: uncertainty value types
- rail: uncertainty

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [ROLE]                                       |
| :-----: | :-------------- | :---------------- | :------------------------------------------- |
|  [01]   | `UFloat`        | scalar quantity   | nominal value plus linear uncertainty        |
|  [02]   | `Variable`      | independent input | `UFloat` subclass with a settable `tag`      |
|  [03]   | `unumpy.matrix` | uncertain matrix  | NumPy `matrix` of `UFloat` with `.I` inverse |

[PUBLIC_TYPE_SCOPE]: `UFloat` members
- rail: uncertainty

| [INDEX] | [MEMBER]              | [KIND]   | [ROLE]                                     |
| :-----: | :-------------------- | :------- | :----------------------------------------- |
|  [01]   | `nominal_value` / `n` | property | central value of the quantity              |
|  [02]   | `std_dev` / `s`       | property | standard deviation of the quantity         |
|  [03]   | `derivatives`         | property | partials with respect to each `Variable`   |
|  [04]   | `error_components()`  | method   | per-variable uncertainty contributions     |
|  [05]   | `std_score(value)`    | method   | deviation of `value` in units of `std_dev` |
|  [06]   | `format(format_spec)` | method   | shorthand-aware formatted text             |
|  [07]   | `tag`                 | property | identity label, `Variable` only            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scalar construction and correlation
- rail: uncertainty

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------------------------------------------------ | :------------- | :------------------------------------- |
|  [01]   | `ufloat(nominal_value, std_dev=None, tag=None)`                           | construct      | one independent uncertain scalar       |
|  [02]   | `ufloat_fromstr(representation, tag=None)`                                | construct      | parse `"x+/-u"` shorthand string       |
|  [03]   | `correlated_values(nom_values, covariance_mat, tags=None)`                | construct      | quantities from a covariance matrix    |
|  [04]   | `correlated_values_norm(values_with_std_dev, correlation_mat, tags=None)` | construct      | quantities from a correlation matrix   |
|  [05]   | `covariance_matrix(nums_with_uncert)`                                     | extract        | covariance across uncertain values     |
|  [06]   | `correlation_matrix(nums_with_uncert)`                                    | extract        | correlation across uncertain values    |
|  [07]   | `nominal_value(x)` / `std_dev(x)`                                         | extract        | scalar accessors over any number       |
|  [08]   | `wrap(f, derivatives_args=None, derivatives_kwargs=None)`                 | adapt          | propagate through a plain function     |
|  [09]   | `nan_if_exception(f)`                                                     | adapt          | derivative that returns `nan` on raise |

[ENTRYPOINT_SCOPE]: array and matrix construction (`uncertainties.unumpy`)
- rail: uncertainty

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `unumpy.uarray(nominal_values, std_devs=None)`  | construct      | array of `UFloat` from two arrays      |
|  [02]   | `unumpy.umatrix(nominal_values, std_devs=None)` | construct      | uncertain NumPy `matrix`               |
|  [03]   | `unumpy.nominal_values(arr)`                    | extract        | array of central values                |
|  [04]   | `unumpy.std_devs(arr)`                          | extract        | array of standard deviations           |
|  [05]   | `unumpy.ulinalg.inv(m)`                         | linalg         | uncertainty-propagating inverse        |
|  [06]   | `unumpy.ulinalg.pinv(m)`                        | linalg         | uncertainty-propagating pseudo-inverse |

[ENTRYPOINT_SCOPE]: uncertainty-propagating math (`uncertainties.umath`, `uncertainties.unumpy`)
- rail: uncertainty

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `umath.sqrt` / `exp` / `log` / `log10`    | scalar math    | drop-in `math` functions on `UFloat` |
|  [02]   | `umath.sin` / `cos` / `tan` / `atan2`     | scalar math    | trigonometric propagation            |
|  [03]   | `umath.asin` / `acos` / `atan` / `hypot`  | scalar math    | inverse-trig and Euclidean norm      |
|  [04]   | `umath.erf` / `erfc` / `gamma` / `lgamma` | scalar math    | special-function propagation         |
|  [05]   | `umath.fsum` / `factorial` / `frexp`      | scalar math    | summation and decomposition          |
|  [06]   | `unumpy.exp` / `log` / `sqrt` / `sin`     | array math     | elementwise propagation over arrays  |

## [04]-[IMPLEMENTATION_LAW]

[UNCERTAINTY_TOPOLOGY]:
- namespace: `uncertainties` (scalars), `uncertainties.unumpy` (arrays/matrices), `uncertainties.umath` (scalar math), `uncertainties.unumpy.ulinalg` (matrix linalg)
- `ufloat` returns a `Variable`; arithmetic over `Variable` and `UFloat` returns `UFloat` (the `AffineScalarFunc` alias) carrying linear dependence on the originating variables
- correlation is tracked through the chain rule on `derivatives`; two results sharing a `Variable` stay correlated automatically without a covariance argument
- `correlated_values` and `correlated_values_norm` reconstruct correlated `UFloat` quantities from a covariance or correlation matrix and optional `tags`; `covariance_matrix` and `correlation_matrix` recover those matrices from a list of results
- `umath` and `unumpy` math functions mirror the standard-library `math`/NumPy names and dispatch to plain functions when no argument carries uncertainty
- `wrap` lifts an arbitrary numeric function into the propagation system; supplied or numerically estimated partial derivatives drive the linear error term

[LOCAL_ADMISSION]:
- Study inputs that carry measurement error enter as `ufloat` or `correlated_values`; the correlation structure stays inside the `UFloat` graph rather than a separate bookkeeping table.
- Derived results expose `nominal_value` and `std_dev`; the propagated `std_dev` and its `error_components()` join the study receipt.
- Array-shaped study payloads use `unumpy.uarray`; matrix study math uses `unumpy.umatrix` with `ulinalg.inv`/`pinv`.
- Uncertainty math uses `umath`/`unumpy` functions, never the bare `math`/NumPy functions, so propagation is preserved.

[RAIL_LAW]:
- Package: `uncertainties`
- Owns: linear error propagation, automatic correlation tracking, and array/matrix uncertainty wrapping for the uncertainty rail
- Accept: a study result carrying a propagated `std_dev`, `error_components`, and correlation provenance
- Reject: manual error-bar arithmetic, uncorrelated re-derivation of propagated variance, and bare `math`/NumPy calls that drop the uncertainty graph
