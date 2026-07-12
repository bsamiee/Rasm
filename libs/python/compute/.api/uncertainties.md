# [PY_COMPUTE_API_UNCERTAINTIES]

`uncertainties` supplies first-order (linear) error propagation over correlated quantities for the compute uncertainty rail. `ufloat` returns an independent `Variable`; arithmetic over `Variable`/`UFloat` yields a derived `UFloat` (the public alias for the internal `AffineScalarFunc`) whose `derivatives` mapping is the chain-rule gradient against the originating variables, so two results sharing a `Variable` stay correlated with no covariance bookkeeping. The library STACKS into one dense uncertainty rail with its siblings: `pint.Measurement` carries the magnitude-plus-error term while `uncertainties` owns the correlation graph behind it; `correlated_values` reconstructs the `UFloat` cohort from a `scipy`/`numpy` covariance matrix (the same matrix a least-squares `scipy.optimize.curve_fit` returns as `pcov`) and `covariance_matrix` recovers it for a downstream sampler; `unumpy.uarray` wraps the identical `numpy` array the array rail folds, threading propagation through NumPy ufuncs; and the propagated `std_dev` plus `error_components()` join the `arviz`/`pandera` study receipt as a captured uncertainty claim. It never re-implements the propagation algebra the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uncertainties`
- package: `uncertainties`
- version: `3.2.3`
- license: BSD-3-Clause (Revised BSD)
- import: `uncertainties`; submodules `uncertainties.umath`, `uncertainties.unumpy`, `uncertainties.unumpy.ulinalg`
- owner: `compute`
- rail: uncertainty
- capability: linear error propagation with automatic correlation tracking — scalar `UFloat` algebra, covariance/correlation reconstruction, arbitrary-function lifting via `wrap`, a `umath` scalar-math mirror, and a `unumpy` NumPy-array/matrix surface with an `ulinalg` linear-algebra path

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: uncertainty value types
- rail: uncertainty
- `UFloat is uncertainties.core.AffineScalarFunc` (one object, two names); `ufloat()` returns a `Variable`, and any arithmetic combination returns a derived `AffineScalarFunc`/`UFloat`

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [ROLE]                                                                            |
| :-----: | :-------------- | :---------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `UFloat`        | scalar quantity   | nominal value plus linear uncertainty; public alias of `AffineScalarFunc`         |
|  [02]   | `Variable`      | independent input | `UFloat`/`AffineScalarFunc` subclass with a settable `tag`; what `ufloat` returns |
|  [03]   | `unumpy.matrix` | uncertain matrix  | NumPy `matrix` subclass of `UFloat` with `.I` uncertainty-propagating inverse     |

[PUBLIC_TYPE_SCOPE]: `UFloat` members
- rail: uncertainty

| [INDEX] | [MEMBER]              | [KIND]   | [ROLE]                                                                                         |
| :-----: | :-------------------- | :------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `nominal_value` / `n` | property | central value of the quantity                                                                  |
|  [02]   | `std_dev` / `s`       | property | standard deviation; `0` for an exact value                                                     |
|  [03]   | `derivatives`         | property | `{Variable: partial}` chain-rule gradient against each source                                  |
|  [04]   | `error_components()`  | method   | `{Variable: contribution}` per-variable uncertainty split that sums in quadrature to `std_dev` |
|  [05]   | `std_score(value)`    | method   | deviation of `value` from the nominal in units of `std_dev`                                    |
|  [06]   | `format(format_spec)` | method   | shorthand-aware text (`{:.2u}` -> `2.00+/-0.10`, `:S`/`:P`/`:L` styles)                        |
|  [07]   | `tag`                 | property | identity label; settable on `Variable`                                                         |
|  [08]   | `dtype`               | property | NumPy-object dtype tag enabling `unumpy` array participation                                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: scalar construction and correlation
- rail: uncertainty
- `correlated_values` / `correlated_values_norm` are the cohort entry: one call reconstructs N correlated `UFloat` from a covariance (or correlation+std) matrix, the polymorphic inverse of `covariance_matrix` / `correlation_matrix`

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [RAIL]                                                                                                   |
| :-----: | :------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `ufloat(nominal_value, std_dev=None, tag=None)`                           | construct      | one independent uncertain scalar (`Variable`)                                                            |
|  [02]   | `ufloat_fromstr(representation, tag=None)`                                | construct      | parse `"x+/-u"` / `"x(u)"` shorthand string                                                              |
|  [03]   | `correlated_values(nom_values, covariance_mat, tags=None)`                | construct      | `UFloat` cohort from a covariance matrix (e.g. `curve_fit` `pcov`)                                       |
|  [04]   | `correlated_values_norm(values_with_std_dev, correlation_mat, tags=None)` | construct      | `UFloat` cohort from `(value, std)` pairs + a correlation matrix                                         |
|  [05]   | `covariance_matrix(nums_with_uncert)`                                     | extract        | covariance matrix across uncertain values                                                                |
|  [06]   | `correlation_matrix(nums_with_uncert)`                                    | extract        | correlation matrix across uncertain values                                                               |
|  [07]   | `nominal_value(x)` / `std_dev(x)`                                         | extract        | scalar accessors over any number, uncertain or plain                                                     |
|  [08]   | `wrap(f, derivatives_args=None, derivatives_kwargs=None)`                 | adapt          | lift a plain numeric `f` into propagation; supply analytic partials or let them be estimated numerically |
|  [09]   | `nan_if_exception(f)`                                                     | adapt          | derivative wrapper returning `nan` instead of raising at a singular point                                |

[ENTRYPOINT_SCOPE]: array and matrix construction (`uncertainties.unumpy`)
- rail: uncertainty

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :---------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `unumpy.uarray(nominal_values, std_devs=None)`  | construct      | object-dtype `ndarray` of `UFloat` from two arrays   |
|  [02]   | `unumpy.umatrix(nominal_values, std_devs=None)` | construct      | uncertain `unumpy.matrix`                            |
|  [03]   | `unumpy.nominal_values(arr)`                    | extract        | array of central values (NumPy-ufunc compatible)     |
|  [04]   | `unumpy.std_devs(arr)`                          | extract        | array of standard deviations                         |
|  [05]   | `unumpy.ulinalg.inv(m)`                         | linalg         | uncertainty-propagating matrix inverse               |
|  [06]   | `unumpy.ulinalg.pinv(m)`                        | linalg         | uncertainty-propagating Moore-Penrose pseudo-inverse |

[ENTRYPOINT_SCOPE]: uncertainty-propagating math (`uncertainties.umath`, `uncertainties.unumpy`)
- rail: uncertainty
- `umath` (40 functions) mirrors `math` names; `unumpy` elementwise math mirrors NumPy names (`arccos`/`arcsin`/`arctan`/`arctan2`/`arccosh`/`arctanh`), not the `math` spellings; both dispatch to the plain library when no argument carries uncertainty

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :---------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `umath.sqrt` / `exp` / `log` / `log10` / `log1p` / `expm1`        | scalar math    | drop-in `math` functions over `UFloat`                |
|  [02]   | `umath.sin` / `cos` / `tan` / `sinh` / `cosh` / `tanh`            | scalar math    | trigonometric / hyperbolic propagation                |
|  [03]   | `umath.asin` / `acos` / `atan` / `atan2` / `hypot`                | scalar math    | inverse-trig, two-argument arctangent, Euclidean norm |
|  [04]   | `umath.asinh` / `acosh` / `atanh` / `pow` / `fabs`                | scalar math    | inverse-hyperbolic, power, absolute value             |
|  [05]   | `umath.erf` / `erfc` / `gamma` / `lgamma` / `factorial`           | scalar math    | special-function propagation                          |
|  [06]   | `umath.fsum` / `frexp` / `modf` / `ldexp` / `copysign`            | scalar math    | summation and float decomposition/composition         |
|  [07]   | `umath.ceil` / `floor` / `trunc` / `fmod` / `degrees` / `radians` | scalar math    | rounding, modulus, angle conversion                   |
|  [08]   | `unumpy.exp` / `log` / `sqrt` / `sin` / `arctan2` / `hypot`       | array math     | elementwise propagation over `uarray`/`umatrix`       |

## [04]-[IMPLEMENTATION_LAW]

[UNCERTAINTY_TOPOLOGY]:
- namespace: `uncertainties` (scalars + correlation), `uncertainties.umath` (scalar math, `math` names), `uncertainties.unumpy` (array/matrix + NumPy-name math), `uncertainties.unumpy.ulinalg` (matrix linalg)
- value graph: `ufloat` returns a `Variable`; every arithmetic result is an `AffineScalarFunc`/`UFloat` carrying a `derivatives` map of linear sensitivities to the originating `Variable` atoms. Correlation is the chain rule on that map — sharing a `Variable` is shared dependence, never a side covariance table.
- cohort reconstruction: `correlated_values(nom, cov)` and `correlated_values_norm((val,std)..., corr)` rebuild a correlated `UFloat` cohort from a matrix; `covariance_matrix`/`correlation_matrix` are the exact inverse, recovering the matrix from a list of results. One polymorphic pair owns both directions — no per-rank helper.
- function lifting: `wrap(f, derivatives_args=..., derivatives_kwargs=...)` lifts an arbitrary numeric callable; analytic partials when supplied, finite-difference estimation otherwise. `nan_if_exception` guards a derivative at a singular argument.
- array thread: `unumpy.uarray` produces an object-dtype `ndarray`; NumPy ufuncs and `unumpy` math thread propagation through it, so the array rail folds the same array under units (`pint`) and uncertainty (`uncertainties`) without a parallel container.

[INTEGRATION_STACK]:
- pint: `pint.Measurement` is the magnitude-plus-error carrier at the units edge; `uncertainties` owns the correlation graph that backs it, so a unit-bearing uncertain quantity is a `pint.Quantity` over a `UFloat` magnitude — no separate error bookkeeping.
- scipy/numpy: a least-squares `scipy.optimize.curve_fit` returns `(popt, pcov)`; `correlated_values(popt, pcov, tags=...)` lifts the fit straight into a correlated `UFloat` cohort whose downstream arithmetic auto-propagates the fit covariance. `unumpy.nominal_values`/`std_devs` then split the cohort back into NumPy arrays for plotting or a `scipy` consumer.
- arviz/receipt: a derived result's `std_dev` and `error_components()` (the per-source variance split) are captured as the study's uncertainty claim, joining the same receipt the `arviz`/`pandera` rail folds.

[LOCAL_ADMISSION]:
- import: `uncertainties`/`umath` at boundary scope; `unumpy` only on an array path (it pulls `numpy`).
- entry: study inputs with measurement error enter as `ufloat` (independent) or `correlated_values` (correlated cohort); the correlation structure lives inside the `UFloat` graph, never a side table.
- evidence: derived results expose `nominal_value`, `std_dev`, and `error_components()`; the propagated `std_dev` and its variance split join the study receipt with correlation provenance.
- array/matrix: array-shaped payloads use `unumpy.uarray`; matrix study math uses `unumpy.umatrix` with `ulinalg.inv`/`pinv`.
- boundary: uncertainty math uses `umath`/`unumpy` functions, never bare `math`/NumPy calls, so the propagation graph is never dropped mid-pipeline.

[RAIL_LAW]:
- Package: `uncertainties`
- Owns: first-order linear error propagation, automatic correlation tracking via the `derivatives` chain rule, covariance/correlation reconstruction, arbitrary-function lifting, and NumPy array/matrix uncertainty wrapping for the uncertainty rail
- Accept: a study result carrying a propagated `std_dev`, an `error_components()` split, and correlation provenance through shared `Variable` atoms or a `correlated_values` cohort
- Reject: manual error-bar arithmetic, uncorrelated re-derivation of propagated variance, a side covariance table parallel to the `UFloat` graph, and bare `math`/NumPy calls that drop the uncertainty graph
