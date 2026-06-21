# [PY_ARTIFACTS_API_COLOUR_SCIENCE]

`colour` supplies the complete colorimetry surface for the artifacts colour rail: spectral distribution representation (`SpectralDistribution`, `MultiSpectralDistributions`, `SpectralShape`), RGB colourspace management (`RGB_Colourspace`, `RGB_COLOURSPACES`), XYZ/Lab/LCH/CAM colour appearance model transforms, LUT operators (`LUT1D`, `LUT3D`, `LUT3x1D`, `LUTSequence`), CCTF encoding/decoding, chromatic adaptation, colour difference metrics, correlated-colour-temperature and chromaticity transforms, dataset registries (CMFS/illuminants/light sources), and a universal `convert` gateway that dispatches across all model pairs. The package owner drives colour science entirely through these surfaces and never re-implements a conversion or interpolation that `colour` owns; numeric coordinate arrays it returns feed the `coloraide` engine (CSS/gamut/CVD authoring), the figures owner, and the imaging codec, with `colour` owning the spectral/colorimetric truth and `coloraide` owning the per-color CSS/gamut-map presentation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colour`
- package: `colour-science`
- import: `colour`
- owner: `artifacts`
- rail: colour
- installed: `0.4.7` reflected via `import colour; colour.__version__` on cp313
- license: `BSD-3-Clause`
- wheel: `colour_science-0.4.7-py3-none-any.whl` — pure Python, no native build; runtime dep `numpy`
- marker: `python_requires <3.15,>=3.11`; SciPy and Matplotlib are optional accelerators — interpolation/optimisation and `plot_*` surfaces warn and degrade (or are absent) when those packages are not installed
- entry points: none (library only)
- capability: spectral distribution representation and resampling, XYZ/RGB/Lab/LCH/CAM appearance transforms, universal `convert` model-pair gateway, chromatic adaptation, CCTF encode/decode, colour difference (`delta_E`), LUT IO (`.cube`/`.clf`/`.csp`), image IO, CCT and chromaticity transforms, dominant/complementary wavelength, whiteness/yellowness and colour-quality indices, and dataset registries for CMFS, illuminants, and light sources

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
|  [01]   | `XYZ_to_Lab(XYZ, illuminant)` / `Lab_to_XYZ(Lab, illuminant)`                              | Lab projection/inverse | CIE 1976 L\*a\*b\* and its inverse                     |
|  [02]   | `XYZ_to_xy(XYZ, illuminant)` / `xy_to_XYZ(xy)` / `XYZ_to_xyY(XYZ)`                          | chromaticity   | XYZ to/from CIE 1931 `xy`/`xyY` chromaticity            |
|  [03]   | `XYZ_to_CIECAM02(...)` / `XYZ_to_CIECAM16(...)` / `XYZ_to_CAM16(...)` / `XYZ_to_Hunt(...)`  | CAM appearance | CIECAM02 / CIECAM16 / CAM16 / Hunt appearance models    |
|  [04]   | `XYZ_to_Oklab(XYZ)` / `XYZ_to_ICtCp(XYZ)` / `XYZ_to_OSA_UCS(XYZ)` / `XYZ_to_hdr_CIELab(XYZ)`| perceptual space | Oklab, ICtCp, OSA-UCS, and HDR-CIELab uniform spaces  |
|  [05]   | `domain_range_scale(scale)` / `get_domain_range_scale()` / `set_domain_range_scale(scale)` | scale context  | `'reference'` / `'1'` / `'100'` mode control            |
|  [06]   | `describe_conversion_path(source, target)`                                                 | graph query    | list conversion path between two colour models          |

[ENTRYPOINT_SCOPE]: CCT, wavelength, and colour-quality operations
- rail: colour

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]       | [CAPABILITY]                                                              |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------- | :----------------------------------------------------------------------- |
|  [01]   | `CCT_to_xy(CCT, method)` / `xy_to_CCT(xy, method)` / `CCT_to_uv(...)` / `uv_to_CCT(...)`  | CCT transform        | correlated colour temperature to/from `xy`/`uv` chromaticity, method-dispatched |
|  [02]   | `wavelength_to_XYZ(wavelength, cmfs)`                                                     | spectral locus       | single-wavelength to XYZ via a CMFS                                      |
|  [03]   | `dominant_wavelength(xy, xy_n)` / `complementary_wavelength(xy, xy_n)`                    | spectral analysis    | dominant / complementary wavelength of a chromaticity vs a white point   |
|  [04]   | `whiteness(XYZ, XYZ_0, method)` / `yellowness(XYZ, method)`                               | colour index         | whiteness and yellowness indices, method-dispatched                      |
|  [05]   | `colour_fidelity_index(sd_test, method)` / `colour_rendering_index(sd_test, method)`      | light-source quality | TM-30 / CIE 2017 colour fidelity and CIE colour rendering index          |
|  [06]   | `sd_to_aces_relative_exposure_values(sd, illuminant)` / `msds_to_XYZ(msds, ...)`          | scene-referred       | SPD to ACES2065-1 RAE; multi-spectral batch SPD to XYZ                   |

[ENTRYPOINT_SCOPE]: dataset registries
- rail: colour

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY]         | [CAPABILITY]                                                                                  |
| :-----: | :------------------ | :--------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `MSDS_CMFS`         | CMFS registry          | keyed colour-matching-function multi-SPDs (e.g. `'CIE 1931 2 Degree Standard Observer'`)      |
|  [02]   | `SDS_ILLUMINANTS`   | illuminant SPDs        | keyed illuminant spectral distributions (`'D65'`, `'A'`, `'FL2'`, …)                          |
|  [03]   | `CCS_ILLUMINANTS`   | illuminant whitepoints | keyed `xy` chromaticity whitepoints per standard observer                                     |
|  [04]   | `SDS_LIGHT_SOURCES` | light-source SPDs      | keyed real light-source spectral distributions                                                |

## [04]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `colour`; 408 public `__all__` symbols covering spectral, colorimetric, appearance, adaptation, difference, CCT, chromaticity, IO, dataset, and interpolation domains
- domain-range scale: three modes — `'reference'` (native), `'1'` (normalised), `'100'` (percentage); use the `domain_range_scale` context manager at the boundary or `set_domain_range_scale` for the call stack; interior calls never toggle it
- dispatch: `convert` is the universal entry point keyed on `source`/`target` model strings; the named transforms (`XYZ_to_Lab`, `XYZ_to_CIECAM02`, `CCT_to_xy`, …) are the direct rows and accept the same `**kwargs`/`method=` axis for method selection — never a per-method wrapper
- method registries: `CHROMATIC_ADAPTATION_METHODS`, `DELTA_E_METHODS`, `CCTF_ENCODINGS`, `CCTF_DECODINGS`, `SD_TO_XYZ_METHODS` enumerate the admitted method strings; the `method=` argument is a registry-key row, never a parallel function
- colourspace catalog: `RGB_COLOURSPACES` is a keyed registry; `RGB_Colourspace` instances carry primaries, whitepoint, encoding/decoding CCTFs, and derivable XYZ matrices
- dataset registries: `MSDS_CMFS`, `SDS_ILLUMINANTS`, `CCS_ILLUMINANTS`, `SDS_LIGHT_SOURCES` are keyed dataset maps; CMFS and illuminant SPDs are looked up by name, never reconstructed
- spectral: `SpectralShape(start, end, interval)` drives wavelength grids; SPDs align via `SpectralDistribution.align(shape)` and interpolate/extrapolate through the registered interpolator family (`SpragueInterpolator` for uniform CIE work, `Extrapolator` for boundary extension)
- LUTs: `LUT1D`/`LUT3D`/`LUT3x1D` compose into a `LUTSequence`; `read_LUT`/`write_LUT` handle `.cube`, `.clf`, and `.csp` formats
- SciPy / Matplotlib are optional accelerators — interpolation/optimisation degrades and `plot_*` surfaces are absent (with a `ColourUsageWarning`) when they are not installed

[LOCAL_ADMISSION]:
- colour science logic enters via `convert` or a named transform; no wrapper re-exposes individual transform calls; method selection is the `method=`/registry-key row.
- domain-range mode is set once at the boundary; interior calls do not toggle it.
- LUT pipelines persist as `LUTSequence` objects; `.cube` files are read via `read_LUT`, never parsed by hand.
- SPD construction uses `SpectralDistribution(data, domain)` with an explicit `SpectralShape`; raw dict intake is the fallback only at the data-ingestion boundary.

[INTEGRATION_STACK]:
- `colour` ↔ `coloraide`: `colour` owns spectral/colorimetric truth (SPD → XYZ via `sd_to_XYZ`, CAM appearance models, CCT, named CIE RGB primaries, ICC-grade chromatic adaptation, LUT IO); `coloraide` owns per-color CSS/named parsing, gamut mapping, and CVD presentation. The seam is the numeric XYZ/`xy` vector: `colour` resolves a measured SPD or an appearance-model coordinate to XYZ, the figures/imaging owner converts to `coloraide`'s `('xyz-d65', coords)` input shape for gamut-mapped CSS output. Neither library re-implements the other's domain — `colour` is never used for CSS string parsing, `coloraide` is never used for spectral sd→XYZ integration.
- `colour` → imaging codec: `read_image`/`write_image` route to Imageio/OpenImageIO float buffers; the multi-bit-depth pixel array feeds the imaging owner directly; the colour transform runs on the array before encode, never on a serialized blob.
- `colour` → figures owner: chromaticity (`XYZ_to_xy`), spectral locus (`wavelength_to_XYZ`), and CCT (`CCT_to_xy`) coordinates feed the plot owner as numeric arrays; `plot_*` surfaces are deferred to the figures owner (Matplotlib-gated), never invoked inside the colour rail.

[RAIL_LAW]:
- Package: `colour-science`
- Owns: spectral distribution representation, XYZ/RGB/Lab/LCH/CAM colour transforms, CCTF encoding/decoding, chromatic adaptation, CCT and chromaticity transforms, dominant/complementary wavelength, whiteness/yellowness and colour-quality indices, LUT IO, image IO, dataset registries, and colour difference metrics
- Accept: `convert` or named transforms for colour-model conversion; `SpectralDistribution`/`MultiSpectralDistributions` for SPD work; `RGB_COLOURSPACES`/`MSDS_CMFS`/`SDS_ILLUMINANTS` for named-registry lookup; XYZ/`xy` arrays handed to `coloraide` for CSS/gamut presentation
- Reject: hand-rolled CCTF, XYZ transform matrices, CCT estimation, or interpolation where `colour` owns the operation; using `coloraide` for spectral sd→XYZ integration or `colour` for CSS string parsing; a per-method wrapper where `method=` selects a registry row; invoking a Matplotlib `plot_*` surface inside the colour rail
