# [PY_ARTIFACTS_API_COLOUR_SCIENCE]

`colour` supplies the complete colorimetry surface for the artifacts colour rail: spectral distribution representation (`SpectralDistribution`, `MultiSpectralDistributions`, `SpectralShape`), RGB colourspace management (`RGB_Colourspace`, `RGB_COLOURSPACES`), XYZ/Lab/LCH/CAM colour appearance model transforms, LUT operators (`LUT1D`, `LUT3D`, `LUT3x1D`, `LUTSequence`), CCTF encoding/decoding, chromatic adaptation, colour difference metrics, and a universal `convert` gateway that dispatches across all model pairs. The package owner drives colour science entirely through these surfaces and never re-implements a conversion or interpolation that `colour` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colour`
- package: `colour-science`
- import: `colour`
- owner: `artifacts`
- rail: colour
- asset: runtime library
- installed: `0.4.7` reflected via `/tmp/wfpy-artifacts315/bin/python`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: spectral distribution and shape family
- rail: colour

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]    | [CAPABILITY]                                                           |
| :-----: | :--------------------------- | :---------------- | :--------------------------------------------------------------------- |
|  [01]   | `SpectralDistribution`       | spectral owner    | `(data, domain, **kwargs)` — single-channel SPD over a wavelength axis |
|  [02]   | `MultiSpectralDistributions` | multi-channel SPD | `(data, domain, labels, **kwargs)` — multiple SPDs sharing one shape   |
|  [03]   | `SpectralShape`              | shape descriptor  | `(start, end, interval)` — wavelength range and step                   |

[PUBLIC_TYPE_SCOPE]: RGB colourspace and LUT family
- rail: colour

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]      | [CAPABILITY]                                                             |
| :-----: | :----------------- | :------------------ | :----------------------------------------------------------------------- |
|  [01]   | `RGB_Colourspace`  | colourspace record  | `(name, primaries, whitepoint, …, cctf_encoding, cctf_decoding)` owner   |
|  [02]   | `RGB_COLOURSPACES` | colourspace catalog | keyed registry of named `RGB_Colourspace` instances                      |
|  [03]   | `LUT1D`            | 1-D LUT operator    | `(table, name, domain, size, comments)` — per-channel lookup             |
|  [04]   | `LUT3D`            | 3-D LUT operator    | `(table, name, domain, size, comments)` — volumetric lookup              |
|  [05]   | `LUT3x1D`          | 3×1D LUT operator   | `(table, name, domain, size, comments)` — three independent 1-D channels |
|  [06]   | `LUTSequence`      | LUT chain           | `(*args: ProtocolLUTSequenceItem)` — ordered LUT pipeline                |

[PUBLIC_TYPE_SCOPE]: interpolation family
- rail: colour

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]     | [CAPABILITY]                           |
| :-----: | :----------------------------- | :----------------- | :------------------------------------- |
|  [01]   | `LinearInterpolator`           | linear             | piecewise-linear segment               |
|  [02]   | `SpragueInterpolator`          | quintic            | Sprague five-order spectral            |
|  [03]   | `CubicSplineInterpolator`      | cubic spline       | natural cubic spline                   |
|  [04]   | `PchipInterpolator`            | monotone cubic     | PCHIP shape-preserving                 |
|  [05]   | `KernelInterpolator`           | kernel-based       | configurable kernel function           |
|  [06]   | `NearestNeighbourInterpolator` | nearest-neighbour  | step interpolation                     |
|  [07]   | `NullInterpolator`             | pass-through       | identity, no interpolation             |
|  [08]   | `Extrapolator`                 | boundary extension | wrap, clamp, or constant extrapolation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: universal conversion and adaptation
- rail: colour

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY]         | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------------------------------------------------------- | :--------------------- | :------------------------------------------- |
|  [01]   | `convert(a, source, target, *, from_reference_scale, to_reference_scale, **kwargs)`             | universal gateway      | single entry for all model-pair conversions  |
|  [02]   | `chromatic_adaptation(XYZ, XYZ_w, XYZ_wr, method, **kwargs)`                                    | adaptation transform   | Von Kries / CMCCAT2000 / Li 2025 / Zhai 2018 |
|  [03]   | `XYZ_to_sRGB(XYZ, illuminant, chromatic_adaptation_transform, apply_cctf_encoding)`             | fast path              | D65-normalised XYZ to sRGB                   |
|  [04]   | `sRGB_to_XYZ(RGB, illuminant, chromatic_adaptation_transform, apply_cctf_decoding)`             | fast path              | sRGB to D65-normalised XYZ                   |
|  [05]   | `XYZ_to_RGB(XYZ, colourspace, illuminant, chromatic_adaptation_transform, apply_cctf_encoding)` | colourspace projection | XYZ to named RGB space                       |
|  [06]   | `RGB_to_XYZ(RGB, colourspace, illuminant, chromatic_adaptation_transform, apply_cctf_decoding)` | colourspace projection | named RGB to XYZ                             |

[ENTRYPOINT_SCOPE]: spectral and CCTF operations
- rail: colour

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------ | :---------------- | :---------------------------------------------------- |
|  [01]   | `sd_to_XYZ(sd, cmfs, illuminant, k, method, **kwargs)`  | spectral to XYZ   | ASTM E308 or Integration method                       |
|  [02]   | `cctf_encoding(value, function, **kwargs)`              | OETF/CCTF encode  | method-dispatched gamma/log CCTF encoding             |
|  [03]   | `cctf_decoding(value, function, **kwargs)`              | OETF/CCTF decode  | method-dispatched CCTF decoding                       |
|  [04]   | `delta_E(a, b, method, **kwargs)`                       | colour difference | CIE 2000 and other ΔE metrics                         |
|  [05]   | `read_LUT(path, **kwargs)`                              | LUT import        | `.cube`, `.clf`, `.csp` and other LUT formats         |
|  [06]   | `write_LUT(LUT, path, **kwargs)`                        | LUT export        | serialise a LUT to disk                               |
|  [07]   | `read_image(path, bit_depth, method, **kwargs)`         | image import      | float32/uint8/uint16 image read (Imageio/OpenImageIO) |
|  [08]   | `write_image(image, path, bit_depth, method, **kwargs)` | image export      | multi-bit-depth image write                           |

[ENTRYPOINT_SCOPE]: appearance model and colorimetry operations
- rail: colour

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `XYZ_to_Lab(XYZ, illuminant)`                            | Lab projection | CIE 1976 L\*a\*b\*                             |
|  [02]   | `Lab_to_XYZ(Lab, illuminant)`                            | Lab inverse    | CIE L\*a\*b\* to XYZ                           |
|  [03]   | `XYZ_to_CIECAM02(XYZ, XYZ_w, L_A, Y_b, surround, …)`     | CAM appearance | CIECAM02 appearance model                      |
|  [04]   | `XYZ_to_Oklab(XYZ)` / `Oklab_to_XYZ(Oklab)`              | Oklab          | Oklab perceptual uniform space                 |
|  [05]   | `domain_range_scale(scale)` / `get_domain_range_scale()` | scale context  | `'reference'` / `'1'` / `'100'` mode control   |
|  [06]   | `describe_conversion_path(source, target)`               | graph query    | list conversion path between two colour models |

## [04]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `colour`; 439 public symbols covering spectral, colorimetric, appearance, adaptation, difference, IO, and interpolation
- domain-range scale: three modes — `'reference'` (native), `'1'` (normalised), `'100'` (percentage); use `domain_range_scale` context manager or `set_domain_range_scale` for the call stack
- dispatch: `convert` is the universal entry point; model-specific transforms (`XYZ_to_Lab`, `XYZ_to_CIECAM02`, etc.) are exposed directly and accept the same `**kwargs` for method selection
- method registries: `CHROMATIC_ADAPTATION_METHODS`, `DELTA_E_METHODS`, `CCTF_ENCODINGS`, `CCTF_DECODINGS`, `SD_TO_XYZ_METHODS` enumerate all admitted method strings
- colourspace catalog: `RGB_COLOURSPACES` is a keyed registry; `RGB_Colourspace` instances carry primaries, whitepoint, encoding/decoding CCTFs, and derivable XYZ matrices
- spectral: `SpectralShape(start, end, interval)` drives wavelength grids; SPDs align via `SpectralDistribution.align(shape)`
- LUTs: `LUT1D`/`LUT3D`/`LUT3x1D` compose into a `LUTSequence`; `read_LUT`/`write_LUT` handle `.cube`, `.clf`, and `.csp` formats
- SciPy / Matplotlib features warn and degrade gracefully when those packages are absent

[LOCAL_ADMISSION]:
- colour science logic enters via `convert` or named transform; no wrapper re-exposes individual transform calls.
- domain-range mode is set once at the boundary; interior calls do not toggle it.
- LUT pipelines persist as `LUTSequence` objects; `.cube` files are read via `read_LUT`, never parsed by hand.
- SPD construction uses `SpectralDistribution(data, domain)` with an explicit `SpectralShape`; raw dict intake is the fallback only at the data-ingestion boundary.

[RAIL_LAW]:
- Package: `colour-science`
- Owns: spectral distribution representation, XYZ/RGB/Lab/LCH/CAM colour transforms, CCTF encoding/decoding, chromatic adaptation, LUT IO, and colour difference metrics
- Accept: `convert` or named transforms for colour model conversion; `SpectralDistribution` for SPD work; `RGB_COLOURSPACES` for named-space lookup
- Reject: hand-rolled CCTF, XYZ transform matrices, or interpolation where `colour` owns the operation
