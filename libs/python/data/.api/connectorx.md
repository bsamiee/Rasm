# [PY_DATA_API_CONNECTORX]

`connectorx` API capture placeholder for `data`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `connectorx`
- package: `connectorx`
- import: `import connectorx`
- owner: `data`
- rail: query
- capability: database-to-frame reads

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[ENTRYPOINTS]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: no cp315 wheel; distribution absent from the >=3.15 lock

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `connectorx`
- Owns: database-to-frame reads
- Accept: pending decompile capture once a cp315 wheel admits `connectorx`
- Reject: wrapper-renames and weaker local reimplementation
