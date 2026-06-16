# [PY_RUNTIME_API_ANYIO]

`anyio` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `anyio`
- package: `anyio`
- import: pending
- owner: `runtime`
- rail: concurrency
- capability: structured concurrency and process boundaries

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `anyio`
- Owns: structured concurrency and process boundaries
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
