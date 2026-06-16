# [PY_DATA_API_DASK]

`dask` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dask`
- package: `dask`
- import: `import dask`
- owner: `data`
- rail: scale
- capability: partitioned data and graph execution

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `dask`
- Owns: partitioned data and graph execution
- Accept: pending decompile capture once a cp315 wheel admits `dask`
- Reject: wrapper-renames and weaker local reimplementation
