# [PY_DATA_API_PYARROW]

`pyarrow` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyarrow`
- package: `pyarrow`
- import: `import pyarrow`
- owner: `data`
- rail: columnar
- capability: Arrow memory dataset Parquet and IPC surfaces

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyarrow`
- Owns: Arrow memory dataset Parquet and IPC surfaces
- Accept: pending decompile capture once a cp315 wheel admits `pyarrow`
- Reject: wrapper-renames and weaker local reimplementation
