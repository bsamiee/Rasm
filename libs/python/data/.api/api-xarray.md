# [PY_DATA_API_XARRAY]

`xarray` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray`
- package: `xarray`
- import: `import xarray`
- owner: `data`
- rail: array
- capability: labeled N-D datasets

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `xarray`
- Owns: labeled N-D datasets
- Accept: pending decompile capture once a cp315 wheel admits `xarray`
- Reject: wrapper-renames and weaker local reimplementation
