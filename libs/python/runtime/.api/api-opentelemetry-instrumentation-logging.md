# [PY_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_LOGGING]

`opentelemetry-instrumentation-logging` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-instrumentation-logging`
- package: `opentelemetry-instrumentation-logging`
- import: pending
- owner: `runtime`
- rail: observability
- capability: stdlib logging bridge

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `opentelemetry-instrumentation-logging`
- Owns: stdlib logging bridge
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
