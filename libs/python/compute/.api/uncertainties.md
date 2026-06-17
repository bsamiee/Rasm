# [PY_COMPUTE_API_UNCERTAINTIES]

`uncertainties` supplies first-order (linear) error propagation over correlated quantities for the compute uncertainty rail. The package owner attaches a standard deviation and correlation structure to study inputs and propagates them through derived results; it never re-implements error-propagation arithmetic the package owns. Member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uncertainties`
- package: `uncertainties`
- import: `uncertainties`; submodule `uncertainties.unumpy`
- owner: `compute`
- rail: uncertainty
- installed: ABSENT on cp315 (requires-python `>=3.15`, no cp315 wheel; sdist build blocked by manifest gaps 1+2)
- capability: linear (first-order) uncertainty propagation with automatic correlation tracking, plus NumPy-array uncertainty wrapping

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: uncertainty owners
- rail: uncertainty

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]      | [CAPABILITY]                        |
| :-----: | :-------------------------------- | :------------------ | :---------------------------------- |
|   [1]   | `uncertainties.ufloat`            | value constructor   | a number with a standard deviation  |
|   [2]   | `uncertainties.UFloat`            | value carrier       | nominal-value-plus-std-dev quantity |
|   [3]   | `uncertainties.unumpy`            | array submodule     | uncertainty-bearing NumPy arrays    |
|   [4]   | `uncertainties.correlated_values` | correlation builder | quantities from a covariance matrix |

[ENTRYPOINTS]:
- UN_REFLECTED: exact attribute/method spellings (`UFloat.nominal_value`, `UFloat.std_dev`, `uncertainties.covariance_matrix`, `unumpy.uarray`) and verified signatures require a reflectable install to capture; type names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- construction: study inputs carrying measurement error enter as `ufloat` or `correlated_values`; correlation is tracked automatically through arithmetic.
- propagation: derived results expose `nominal_value` and `std_dev`; the uncertainty claim joins the study receipt.
- boundary: uncertainty claims are offline study evidence; product error policy stays in C# owners after graduation.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `uncertainties`
- Owns: linear error propagation, correlation tracking, and array uncertainty wrapping for the uncertainty rail
- Accept: a study result carrying a propagated `std_dev` and correlation provenance
- Reject: manual error-bar arithmetic; uncorrelated re-derivation of propagated variance; wrapper-renames of `UFloat` operations
