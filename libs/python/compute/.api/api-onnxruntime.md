# [PY_COMPUTE_API_ONNXRUNTIME]

`onnxruntime` API capture placeholder for `compute`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `onnxruntime`
- package: `onnxruntime`
- import: pending
- owner: `compute`
- rail: pending
- capability: model validation runtime candidate

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- pending

[ENTRYPOINTS]:
- pending

[IMPLEMENTATION_LAW]:
- pending

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `onnxruntime`
- Owns: model validation runtime candidate
- Accept: pending package-owner capture
- Reject: wrapper-renames and weaker local reimplementation
