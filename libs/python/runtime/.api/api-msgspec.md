# [PY_RUNTIME_API_MSGSPEC]

`msgspec` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msgspec`
- package: `msgspec`
- import: pending
- owner: `runtime`
- rail: serialization
- capability: wire structs and fast codecs

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `msgspec`
- Owns: wire structs and fast codecs
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
