# [PY_RUNTIME_API_OPENTELEMETRY_API]

`opentelemetry-api` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-api`
- package: `opentelemetry-api`
- import: pending
- owner: `runtime`
- rail: observability
- capability: trace metric and baggage contracts

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `opentelemetry-api`
- Owns: trace metric and baggage contracts
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
