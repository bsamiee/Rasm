# [PY_DATA_API_RASTERIO]

`rasterio` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rasterio`
- package: `rasterio`
- import: `import rasterio`
- owner: `data`
- rail: geospatial
- capability: raster geospatial file I/O

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `rasterio`
- Owns: raster geospatial file I/O
- Accept: pending decompile capture once a cp315 wheel admits `rasterio`
- Reject: wrapper-renames and weaker local reimplementation
