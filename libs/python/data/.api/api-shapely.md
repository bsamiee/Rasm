# [PY_DATA_API_SHAPELY]

`shapely` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `shapely`
- package: `shapely`
- import: `import shapely`
- owner: `data`
- rail: geospatial
- capability: geometry predicates and operations

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `shapely`
- Owns: geometry predicates and operations
- Accept: pending decompile capture once a cp315 wheel admits `shapely`
- Reject: wrapper-renames and weaker local reimplementation
