# [PY_COMPUTE_API_XARRAY]

`xarray` supplies labelled, named-axis n-dimensional arrays and datasets for the compute named-axis rail. The package owner attaches study dimension names, coordinate labels, and free-dimension evidence to an admitted array so study plans address axes by name rather than position; it never re-implements the labelled-array model xarray owns. Member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray`
- package: `xarray`
- import: `xarray` (lint alias `xr`)
- owner: `compute`
- rail: arrays
- installed: ABSENT on cp315 (requires-python `>=3.15`, no cp315 wheel; sdist build blocked by manifest gaps 1+2)
- capability: labelled n-dimensional arrays — named dimensions, coordinate indexes, dataset grouping, label-based selection, and a dask-backed lazy/chunked path

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: labelled-array owners
- rail: arrays

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `xarray.DataArray` | labelled array | array plus named dims and coords |
| [2] | `xarray.Dataset` | labelled collection | named DataArrays sharing dimensions |
| [3] | `xarray.Variable` | dim-aware buffer | array with attached dimension names |
| [4] | `xarray.Coordinates` | coordinate index | label index for a dimension |

[ENTRYPOINTS]:
- UN_REFLECTED: exact method spellings (`DataArray.sel`, `DataArray.isel`, `Dataset.groupby`, `DataArray.chunk`, `DataArray.dims`, `DataArray.coords`) and verified signatures require a reflectable install to capture; type names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- naming: an admitted `ndarray` is wrapped in a `DataArray`/`Dataset` carrying study dimension names and coordinate labels; the axis record is consumed by study plans.
- selection: label-based `sel`/`isel` address study axes by name; free-dimension evidence is captured from the dimension set.
- chunking: `chunk` opts an array into the dask-backed lazy path (see `.api/api-dask.md`).
- boundary: named axes are study evidence, not wire vocabulary.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `xarray`
- Owns: labelled named-axis arrays, coordinate indexes, and free-dimension evidence for the array rail
- Accept: an admitted array wrapped with study dimension names and coordinate labels
- Reject: positional-only axis handling where names exist; wrapper-renames of label selection; treating axes as wire vocabulary
