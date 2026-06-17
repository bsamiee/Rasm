# [PY_DATA_API_DELTALAKE]

`deltalake` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `deltalake`
- package: `deltalake`
- import: `import deltalake`
- owner: `data`
- rail: columnar
- capability: Delta table exchange

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `deltalake`
- Owns: Delta table exchange
- Accept: pending decompile capture once a cp315 wheel admits `deltalake`
- Reject: wrapper-renames and weaker local reimplementation
