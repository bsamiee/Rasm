# [PY_COMPUTE_API_NUMPY]

`numpy` API capture placeholder for `compute`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpy`
- package: `numpy`
- import: pending
- owner: `compute`
- rail: arrays
- capability: array dtype and numeric primitives

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `numpy`
- Owns: array dtype and numeric primitives
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
