# [PY_ARTIFACTS_API_PYTHON_PPTX]

`python-pptx` API capture for the `artifacts` office rail. The distribution is locked (`python-pptx` in `uv.lock` under the `artifacts` group) but un-reflectable on this host: it depends on `pillow`, which has no cp315 binary wheel and whose source build fails on the local toolchain (absent libjpeg/zlib headers), so the dependency closure cannot install. Members below remain a TASKLOG gap; the owner names no `python-pptx` member until `pillow` becomes installable (cp315 wheel or provisioned image toolchain) and a reflection pass captures exact spellings.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `python-pptx`
- package: `python-pptx`
- import: `pptx`
- owner: `artifacts`
- rail: office
- installed: locked but not installed on cp315; blocked transitively by `pillow` (no cp315 wheel, source build needs absent libjpeg/zlib headers)
- capability: PowerPoint .pptx presentation construction and editing

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: locked but not installable; blocked transitively by `pillow`

[ENTRYPOINTS]:
- un-reflectable on this host: locked but not installable; blocked transitively by `pillow`

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: locked but not installable; blocked transitively by `pillow`

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `python-pptx`
- Owns: PowerPoint .pptx presentation construction and editing
- Accept: pending reflection capture once `pillow` becomes installable and the dependency closure resolves
- Reject: wrapper-renames and weaker local reimplementation
