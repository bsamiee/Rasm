# [PY_COMPUTE_API_UNCERTAINTIES]

`uncertainties` owns first-order linear error propagation with automatic correlation tracking for the compute uncertainty rail. `ufloat` mints an independent `Variable`, and every arithmetic result is a derived `UFloat` (the public alias of `AffineScalarFunc`) carrying a `derivatives` chain-rule gradient against its source variables, so two results sharing a `Variable` stay correlated with no side covariance table. A derived `std_dev` and its `error_components()` variance split feed the study receipt, and a bare `math`/NumPy call that drops the propagation graph is the boundary reject signal.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uncertainties`
- package: `uncertainties`
- module: `uncertainties`
- namespaces: `uncertainties.umath`, `uncertainties.unumpy`, `uncertainties.unumpy.ulinalg`
- owner: `compute`
- rail: uncertainty
- capability: linear error propagation with automatic correlation tracking — scalar `UFloat` algebra, covariance/correlation reconstruction, arbitrary-function lifting via `wrap`, a `umath` scalar-math mirror, and a `unumpy` NumPy-array/matrix surface with an `ulinalg` linear-algebra path

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: uncertainty value types

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [CAPABILITY]                                                                      |
| :-----: | :-------------- | :---------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `UFloat`        | scalar quantity   | nominal value plus linear uncertainty; public alias of `AffineScalarFunc`         |
|  [02]   | `Variable`      | independent input | `UFloat`/`AffineScalarFunc` subclass with a settable `tag`; what `ufloat` returns |
|  [03]   | `unumpy.matrix` | uncertain matrix  | NumPy `matrix` subclass of `UFloat` with `.I` uncertainty-propagating inverse     |

[PUBLIC_TYPE_SCOPE]: `UFloat` members

| [INDEX] | [MEMBER]              | [KIND]   | [CAPABILITY]                                                                                   |
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
- `correlated_values(nom_values, covariance_mat, tags=None)`, `correlated_values_norm(values_with_std_dev, correlation_mat, tags=None)`: the cohort constructors.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `ufloat(nominal_value, std_dev=None, tag=None)`           | construct      | one independent uncertain scalar (`Variable`)           |
|  [02]   | `ufloat_fromstr(representation, tag=None)`                | construct      | parse `"x+/-u"` / `"x(u)"` shorthand string             |
|  [03]   | `correlated_values`                                       | construct      | cohort from a covariance matrix (`curve_fit` `pcov`)    |
|  [04]   | `correlated_values_norm`                                  | construct      | cohort from `(value, std)` pairs + a correlation matrix |
|  [05]   | `covariance_matrix(nums_with_uncert)`                     | extract        | covariance matrix across uncertain values               |
|  [06]   | `correlation_matrix(nums_with_uncert)`                    | extract        | correlation matrix across uncertain values              |
|  [07]   | `nominal_value(x)` / `std_dev(x)`                         | extract        | scalar accessors over any number, uncertain or plain    |
|  [08]   | `wrap(f, derivatives_args=None, derivatives_kwargs=None)` | adapt          | lift a numeric `f` into propagation                     |
|  [09]   | `nan_if_exception(f)`                                     | adapt          | `nan`-returning derivative wrapper at a singular point  |

[ENTRYPOINT_SCOPE]: array and matrix construction (`uncertainties.unumpy`)

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `unumpy.uarray(nominal_values, std_devs=None)`  | construct      | object-dtype `ndarray` of `UFloat` from two arrays   |
|  [02]   | `unumpy.umatrix(nominal_values, std_devs=None)` | construct      | uncertain `unumpy.matrix`                            |
|  [03]   | `unumpy.nominal_values(arr)`                    | extract        | array of central values (NumPy-ufunc compatible)     |
|  [04]   | `unumpy.std_devs(arr)`                          | extract        | array of standard deviations                         |
|  [05]   | `unumpy.ulinalg.inv(m)`                         | linalg         | uncertainty-propagating matrix inverse               |
|  [06]   | `unumpy.ulinalg.pinv(m)`                        | linalg         | uncertainty-propagating Moore-Penrose pseudo-inverse |

[ENTRYPOINT_SCOPE]: uncertainty-propagating math (`uncertainties.umath`, `uncertainties.unumpy`)
- `umath` mirrors `math` names; `unumpy` elementwise math mirrors NumPy names (`arccos`/`arcsin`/`arctan`/`arctan2`/`arccosh`/`arctanh`), not the `math` spellings; both dispatch to the plain library when no argument carries uncertainty.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `umath.sqrt` / `exp` / `log` / `log10` / `log1p` / `expm1`        | scalar math    | drop-in `math` functions over `UFloat`          |
|  [02]   | `umath.sin` / `cos` / `tan` / `sinh` / `cosh` / `tanh`            | scalar math    | trigonometric / hyperbolic propagation          |
|  [03]   | `umath.asin` / `acos` / `atan` / `atan2` / `hypot`                | scalar math    | inverse-trig, `atan2`, Euclidean norm           |
|  [04]   | `umath.asinh` / `acosh` / `atanh` / `pow` / `fabs`                | scalar math    | inverse-hyperbolic, power, absolute value       |
|  [05]   | `umath.erf` / `erfc` / `gamma` / `lgamma` / `factorial`           | scalar math    | special-function propagation                    |
|  [06]   | `umath.fsum` / `frexp` / `modf` / `ldexp` / `copysign`            | scalar math    | summation and float decomposition/composition   |
|  [07]   | `umath.ceil` / `floor` / `trunc` / `fmod` / `degrees` / `radians` | scalar math    | rounding, modulus, angle conversion             |
|  [08]   | `unumpy.exp` / `log` / `sqrt` / `sin` / `arctan2` / `hypot`       | array math     | elementwise propagation over `uarray`/`umatrix` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- value graph: `ufloat` returns a `Variable`; every arithmetic result is an `AffineScalarFunc`/`UFloat` carrying a `derivatives` map of linear sensitivities to the originating `Variable` atoms. Correlation is the chain rule on that map — sharing a `Variable` is shared dependence, never a side covariance table.
- cohort reconstruction: `correlated_values(nom, cov)` and `correlated_values_norm((val,std)..., corr)` rebuild a correlated `UFloat` cohort from a matrix; `covariance_matrix`/`correlation_matrix` are the exact inverse. One polymorphic pair owns both directions — no per-rank helper.
- function lifting: `wrap` lifts an arbitrary numeric callable, using supplied analytic partials or finite-difference estimation; `nan_if_exception` guards a derivative at a singular argument.
- array thread: `unumpy.uarray` produces an object-dtype `ndarray` that NumPy ufuncs and `unumpy` math thread propagation through, folding the same array the compute array rail carries under both units and uncertainty without a parallel container.

[STACKING]:
- `pint` (`.api/pint.md`): `Quantity.plus_minus`/`UnitRegistry.Measurement` build a `pint.Measurement` over a `UFloat` magnitude, so a unit-bearing quantity carries correlation in one carrier with no side error bookkeeping.
- `scipy` (`.api/scipy.md`) / `numpy` (`.api/numpy.md`, substrate tier): `correlated_values(popt, pcov, tags=...)` lifts a `scipy.optimize.curve_fit` `(popt, pcov)` into a correlated `UFloat` cohort that auto-propagates the fit covariance; `unumpy.nominal_values`/`std_devs` split it back into NumPy arrays.
- `arviz` (`.api/arviz.md`) / `pandera` (`libs/python/data/.api/pandera.md`): a derived `std_dev` and its `error_components()` variance split are the uncertainty claim on the study receipt.
- `compute` uncertainty rail: study math routes through `umath`/`unumpy`, never bare `math`/NumPy, so the propagation graph threads unbroken from `ufloat` input to receipt.

[LOCAL_ADMISSION]:
- import: `uncertainties`/`umath` at boundary scope; `unumpy` only on an array path (it pulls `numpy`).
- entry: measurement-error inputs enter as `ufloat` (independent) or `correlated_values` (correlated cohort).
- evidence: derived results expose `nominal_value`, `std_dev`, and `error_components()`, which join the study receipt with correlation provenance.
- array/matrix: array payloads use `unumpy.uarray`; matrix study math uses `unumpy.umatrix` with `ulinalg.inv`/`pinv`.

[RAIL_LAW]:
- Package: `uncertainties`
- Owns: first-order linear error propagation, automatic correlation tracking via the `derivatives` chain rule, covariance/correlation reconstruction, arbitrary-function lifting, and NumPy array/matrix uncertainty wrapping for the uncertainty rail
- Accept: a study result carrying a propagated `std_dev`, an `error_components()` split, and correlation provenance through shared `Variable` atoms or a `correlated_values` cohort
- Reject: manual error-bar arithmetic, uncorrelated re-derivation of propagated variance, a side covariance table parallel to the `UFloat` graph, and bare `math`/NumPy calls that drop the uncertainty graph
