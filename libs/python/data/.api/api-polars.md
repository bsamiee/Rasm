# [PY_DATA_API_POLARS]

`polars` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `polars`
- package: `polars`
- import: `import polars`
- owner: `data`
- rail: columnar
- capability: eager and lazy columnar frames

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `polars`
- Owns: eager and lazy columnar frames
- Accept: pending decompile capture once a cp315 wheel admits `polars`
- Reject: wrapper-renames and weaker local reimplementation
