# [PY_ARTIFACTS_API_VTK]

`vtk` API capture for the `artifacts` visuals rail. The distribution is intentionally marker-gated to `python_version<'3.13'` in the manifest (`vtk>=9.6.2; python_version<'3.13'`) because VTK publishes no cp315 wheel and its native build is non-viable on the local toolchain, so it is correctly excluded from the active cp315 resolution and is un-reflectable on this host. Members below remain a TASKLOG gap; the owner names no `vtk` member until the gated sub-3.13 companion interpreter resolves it and a reflection pass captures exact spellings, or VTK publishes a cp315 wheel.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vtk`
- package: `vtk`
- import: `vtk`
- owner: `artifacts`
- rail: visuals
- installed: intentionally absent on cp315; marker-gated `python_version<'3.13'`; no cp315 wheel and native build non-viable locally
- capability: 3D scientific visualization toolkit pipelines and renderers

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: marker-gated `python_version<'3.13'`; resolves only on the sub-3.13 companion interpreter

[ENTRYPOINTS]:
- un-reflectable on this host: marker-gated `python_version<'3.13'`; resolves only on the sub-3.13 companion interpreter

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: marker-gated `python_version<'3.13'`; resolves only on the sub-3.13 companion interpreter

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `vtk`
- Owns: 3D scientific visualization toolkit pipelines and renderers
- Accept: pending reflection capture on the gated sub-3.13 companion interpreter, or once VTK publishes a cp315 wheel
- Reject: wrapper-renames and weaker local reimplementation
