# [PY_DATA_API_PANDAS]

`pandas` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pandas`
- package: `pandas`
- import: `import pandas`
- owner: `data`
- rail: columnar
- capability: compatibility tabular frames

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pandas`
- Owns: compatibility tabular frames
- Accept: pending decompile capture once a cp315 wheel admits `pandas`
- Reject: wrapper-renames and weaker local reimplementation
