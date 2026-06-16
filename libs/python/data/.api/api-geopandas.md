# [PY_DATA_API_GEOPANDAS]

`geopandas` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `geopandas`
- package: `geopandas`
- import: `import geopandas`
- owner: `data`
- rail: geospatial
- capability: geospatial tabular data

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `geopandas`
- Owns: geospatial tabular data
- Accept: pending decompile capture once a cp315 wheel admits `geopandas`
- Reject: wrapper-renames and weaker local reimplementation
