# [PY_RUNTIME_API_OPENTELEMETRY_EXPORTER_OTLP_PROTO_HTTP]

`opentelemetry-exporter-otlp-proto-http` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentelemetry-exporter-otlp-proto-http`
- package: `opentelemetry-exporter-otlp-proto-http`
- import: pending
- owner: `runtime`
- rail: observability
- capability: OTLP HTTP export boundary

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `opentelemetry-exporter-otlp-proto-http`
- Owns: OTLP HTTP export boundary
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
