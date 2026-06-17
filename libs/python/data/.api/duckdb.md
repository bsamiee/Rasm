# [PY_DATA_API_DUCKDB]

`duckdb` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `duckdb`
- package: `duckdb`
- import: `import duckdb`
- owner: `data`
- rail: query
- capability: in-process SQL and relation execution

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `duckdb`
- Owns: in-process SQL and relation execution
- Accept: pending decompile capture once a cp315 wheel admits `duckdb`
- Reject: wrapper-renames and weaker local reimplementation
