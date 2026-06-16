# [PY_RUNTIME_API_OPENTELEMETRY_SDK]

`opentelemetry-sdk` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-sdk`
- package: `opentelemetry-sdk`
- import: pending
- owner: `runtime`
- rail: observability
- capability: trace metric and log providers

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `opentelemetry-sdk`
- Owns: trace metric and log providers
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
