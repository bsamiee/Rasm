# [PY_ARTIFACTS_API_PYVISTA]

`pyvista` API capture for the `artifacts` visuals rail. The distribution is intentionally marker-gated to `python_version<'3.13'` in the manifest (`pyvista>=0.48.4; python_version<'3.13'`) because it rides the `vtk` native-interpreter floor that publishes no cp315 wheel, so it is correctly excluded from the active cp315 resolution and is un-reflectable on this host. Members below remain a TASKLOG gap; the owner names no `pyvista` member until the gated sub-3.13 companion interpreter resolves it and a reflection pass captures exact spellings, or `vtk` publishes a cp315 wheel that lifts the floor.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyvista`
- package: `pyvista`
- import: `pyvista`
- owner: `artifacts`
- rail: visuals
- installed: intentionally absent on cp315; marker-gated `python_version<'3.13'` riding the `vtk` native floor
- capability: VTK-backed 3D scientific visualization and mesh plotting

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: marker-gated `python_version<'3.13'`; resolves only on the sub-3.13 companion interpreter

[ENTRYPOINTS]:
- un-reflectable on this host: marker-gated `python_version<'3.13'`; resolves only on the sub-3.13 companion interpreter

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: marker-gated `python_version<'3.13'`; resolves only on the sub-3.13 companion interpreter

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyvista`
- Owns: VTK-backed 3D scientific visualization and mesh plotting
- Accept: pending reflection capture on the gated sub-3.13 companion interpreter, or once `vtk` publishes a cp315 wheel
- Reject: wrapper-renames and weaker local reimplementation
