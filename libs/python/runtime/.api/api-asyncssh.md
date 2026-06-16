# [PY_RUNTIME_API_ASYNCSSH]

`asyncssh` API capture placeholder for `runtime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `asyncssh`
- package: `asyncssh`
- import: pending
- owner: `runtime`
- rail: transport
- capability: SSH SFTP and process transport

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `asyncssh`
- Owns: SSH SFTP and process transport
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
