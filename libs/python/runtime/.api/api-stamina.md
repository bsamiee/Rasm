# [PY_RUNTIME_API_STAMINA]

`stamina` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `stamina`
- package: `stamina`
- import: pending
- owner: `runtime`
- rail: resilience
- capability: retry policy and backoff

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `stamina`
- Owns: retry policy and backoff
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
