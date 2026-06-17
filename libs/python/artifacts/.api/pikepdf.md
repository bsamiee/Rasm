# [PY_ARTIFACTS_API_PIKEPDF]

`pikepdf` API capture for the `artifacts` pdf rail. The distribution is locked (`pikepdf` in `uv.lock` under the `artifacts` group) but un-reflectable on this host: it depends on `pillow`, which has no cp315 binary wheel and whose source build fails on the local toolchain (absent libjpeg/zlib headers), so the dependency closure cannot install. Members below remain a TASKLOG gap; the owner names no `pikepdf` member until `pillow` becomes installable (cp315 wheel or provisioned image toolchain) and a reflection pass captures exact spellings.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pikepdf`
- package: `pikepdf`
- import: `pikepdf`
- owner: `artifacts`
- rail: pdf
- installed: locked but not installed on cp315; blocked transitively by `pillow` (no cp315 wheel, source build needs absent libjpeg/zlib headers)
- capability: qpdf-backed PDF structure repair, linearization, and encryption

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: locked but not installable; blocked transitively by `pillow`

[ENTRYPOINTS]:
- un-reflectable on this host: locked but not installable; blocked transitively by `pillow`

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: locked but not installable; blocked transitively by `pillow`

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pikepdf`
- Owns: qpdf-backed PDF structure repair, linearization, and encryption
- Accept: pending reflection capture once `pillow` becomes installable and the dependency closure resolves
- Reject: wrapper-renames and weaker local reimplementation
