# [PY_COMPUTE_API_CATALOGUE]

`compute` API catalogue pages carry decompiled/reflected external-package surface plus package-rail admission law. One distribution (`sympy`) is reflectable on the cp315 interpreter and carries a member-complete surface; the other eleven are absent from the cp315 lock (requires-python `>=3.15` with no cp315 wheels, manifest gaps 1+2) and carry stable submodule/type facts with member spellings marked UN_REFLECTED until a reflectable install exists.

## [1]-[PACKAGE_PAGES]

[ARRAYS]:
- rail: arrays
- pages:
  - [api-numpy.md](api-numpy.md)
  - [api-xarray.md](api-xarray.md)

[SOLVERS]:
- rail: solvers
- pages:
  - [api-scipy.md](api-scipy.md)

[SYMBOLIC]:
- rail: symbolic
- pages:
  - [api-sympy.md](api-sympy.md)

[STUDIES]:
- rail: studies
- pages:
  - [api-dask.md](api-dask.md)

[UNITS]:
- rail: units
- pages:
  - [api-pint.md](api-pint.md)

[UNCERTAINTY]:
- rail: uncertainty
- pages:
  - [api-uncertainties.md](api-uncertainties.md)

[ACCELERATOR]:
- rail: accelerator
- pages:
  - [api-jax.md](api-jax.md)
  - [api-numba.md](api-numba.md)

[MODEL]:
- rail: model
- pages:
  - [api-onnx.md](api-onnx.md)
  - [api-onnxruntime.md](api-onnxruntime.md)
  - [api-scikit-learn.md](api-scikit-learn.md)

## [2]-[REFLECTION_STATUS]

| [INDEX] | [DISTRIBUTION] | [RAIL] | [REFLECTED] |
| :-----: | :------------- | :----- | :---------- |
| [1] | `sympy` | symbolic | `1.14.0` member-complete via `assay api query --key sympy` |
| [2] | `numpy` | arrays | ABSENT on cp315; submodule/type facts only |
| [3] | `xarray` | arrays | ABSENT on cp315; submodule/type facts only |
| [4] | `scipy` | solvers | ABSENT on cp315; submodule/type facts only |
| [5] | `dask` | studies | ABSENT on cp315; submodule/type facts only |
| [6] | `pint` | units | ABSENT on cp315; type facts only |
| [7] | `uncertainties` | uncertainty | ABSENT on cp315; type facts only |
| [8] | `jax` | accelerator | ABSENT on cp315 (marker `<'3.15'`); transform facts only |
| [9] | `numba` | accelerator | ABSENT on cp315 (marker `<'3.15'`); decorator facts only |
| [10] | `onnx` | model | ABSENT on cp315 (marker `<'3.15'`); message/submodule facts only |
| [11] | `onnxruntime` | model | ABSENT on cp315 (marker `<'3.15'`); type facts only |
| [12] | `scikit-learn` | model | ABSENT on cp315 (marker `<'3.15'`); submodule/type facts only |

## [3]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- API pages carry external package API facts and package-rail admission records.
- Planning pages carry owner boundaries and source-transcription law.
- README pages route catalogues without duplicating member tables.

[REFLECTION_LAW]:
- A reflectable distribution carries a member-complete surface with exact spellings captured via `assay api query`.
- An absent distribution carries only stable submodule/type/import facts; every member spelling stays an UN_REFLECTED entry until a reflectable install captures it, never an invented signature.
- The un-reflected member surface for the eleven absent distributions is the open TASKLOG gap, closed when a cp315-compatible install or a lowered requires-python floor (manifest gap 1) admits reflection.
