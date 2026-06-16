# [PY_DATA_API_DUCKDB]

`duckdb` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `duckdb`
- package: `duckdb`
- import: pending
- owner: `data`
- rail: query
- capability: in-process SQL and relation execution

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `duckdb`
- Owns: in-process SQL and relation execution
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
