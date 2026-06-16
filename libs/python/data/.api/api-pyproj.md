# [PY_DATA_API_PYPROJ]

`pyproj` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyproj`
- package: `pyproj`
- import: `import pyproj`
- owner: `data`
- rail: geospatial
- capability: CRS transforms and geodesy

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyproj`
- Owns: CRS transforms and geodesy
- Accept: pending decompile capture once a cp315 wheel admits `pyproj`
- Reject: wrapper-renames and weaker local reimplementation
