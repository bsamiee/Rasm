# [PY_COMPUTE_API_UNCERTAINTIES]

`uncertainties` API capture placeholder for `compute`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `uncertainties`
- package: `uncertainties`
- import: pending
- owner: `compute`
- rail: uncertainty
- capability: uncertainty propagation

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `uncertainties`
- Owns: uncertainty propagation
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
