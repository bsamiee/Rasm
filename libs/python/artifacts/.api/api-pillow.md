# [PY_ARTIFACTS_API_PILLOW]

`pillow` API capture for the `artifacts` image rail. The distribution is locked (`pillow` in `uv.lock` under the `artifacts` group) but un-reflectable on this host: PyPI publishes no cp315 (Python 3.15) binary wheel, and the source build fails on the local Nix toolchain — the build env points `AR` at a non-existent `llvm-ar` store path, and after that override the build still requires system `jpeg`/`zlib` development headers that are absent. Pillow is the single irreducible image-toolchain gap; its un-buildability transitively blocks every `artifacts` distribution that depends on it. Members below remain a TASKLOG gap; the owner names no `pillow` member until a cp315 wheel publishes or the host provisions the C image-library toolchain (libjpeg/zlib headers and a working archiver) and a reflection pass captures exact spellings.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pillow`
- package: `pillow`
- import: `PIL`
- owner: `artifacts`
- rail: image
- installed: locked but not installed on cp315; no cp315 binary wheel on PyPI, and the source build fails on the local toolchain (missing `llvm-ar`, then absent libjpeg/zlib headers)
- capability: raster image decode, encode, transform, and composition

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- un-reflectable on this host: locked but not installable; no cp315 wheel and the source build needs absent libjpeg/zlib headers

[ENTRYPOINTS]:
- un-reflectable on this host: locked but not installable; no cp315 wheel and the source build needs absent libjpeg/zlib headers

[IMPLEMENTATION_LAW]:
- un-reflectable on this host: locked but not installable; no cp315 wheel and the source build needs absent libjpeg/zlib headers

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pillow`
- Owns: raster image decode, encode, transform, and composition
- Accept: pending reflection capture once a cp315 wheel publishes or the host provisions the libjpeg/zlib toolchain that lets the source build succeed
- Reject: wrapper-renames and weaker local reimplementation
