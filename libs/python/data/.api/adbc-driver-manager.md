# [PY_DATA_API_ADBC_DRIVER_MANAGER]

`adbc-driver-manager` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `adbc-driver-manager`
- package: `adbc-driver-manager`
- import: `import adbc_driver_manager`
- owner: `data`
- rail: query
- capability: Arrow database connectivity

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `adbc-driver-manager`
- Owns: Arrow database connectivity
- Accept: pending decompile capture once a cp315 wheel admits `adbc_driver_manager`
- Reject: wrapper-renames and weaker local reimplementation
