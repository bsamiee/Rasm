# [PY_RUNTIME_API_FSSPEC]

`fsspec` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fsspec`
- package: `fsspec`
- import: pending
- owner: `runtime`
- rail: resources
- capability: filesystem protocol dispatch

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `fsspec`
- Owns: filesystem protocol dispatch
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
