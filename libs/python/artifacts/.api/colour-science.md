# [PY_ARTIFACTS_API_COLOUR_SCIENCE]

`colour` owns the artifacts colour rail's colorimetric truth: spectral-distribution representation, colour-model conversion across XYZ/RGB/Lab/CAM appearance spaces through one `convert` gateway, chromatic adaptation, CCTF and broadcast transfer functions, LUT and image IO, colour-difference and colour-quality metrics, and the CMFS/illuminant/light-source dataset registries. It resolves a measured SPD or appearance coordinate to the numeric XYZ/`xy` vector every downstream engine consumes, never re-implementing a conversion or interpolation it owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colour`
- package: `colour-science` (`BSD-3-Clause`)
- import: `colour`
- owner: `artifacts`
- rail: colour
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: spectral distribution and shape family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]     | [CAPABILITY]                                                           |
| :-----: | :--------------------------- | :---------------- | :--------------------------------------------------------------------- |
|  [01]   | `SpectralDistribution`       | spectral owner    | `(data, domain, **kwargs)` — single-channel SPD over a wavelength axis |
|  [02]   | `MultiSpectralDistributions` | multi-channel SPD | `(data, domain, labels, **kwargs)` — multiple SPDs sharing one shape   |
|  [03]   | `SpectralShape`              | shape descriptor  | `(start, end, interval)` — wavelength range and step                   |

[PUBLIC_TYPE_SCOPE]: RGB colourspace and LUT family

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]       | [CAPABILITY]                                                             |
| :-----: | :----------------- | :------------------ | :----------------------------------------------------------------------- |
|  [01]   | `RGB_Colourspace`  | colourspace record  | `(name, primaries, whitepoint, …, cctf_encoding, cctf_decoding)` owner   |
|  [02]   | `RGB_COLOURSPACES` | colourspace catalog | keyed registry of named `RGB_Colourspace` instances                      |
|  [03]   | `LUT1D`            | 1-D LUT operator    | `(table, name, domain, size, comments)` — per-channel lookup             |
|  [04]   | `LUT3D`            | 3-D LUT operator    | `(table, name, domain, size, comments)` — volumetric lookup              |
|  [05]   | `LUT3x1D`          | 3×1D LUT operator   | `(table, name, domain, size, comments)` — three independent 1-D channels |
|  [06]   | `LUTSequence`      | LUT chain           | `(*args: ProtocolLUTSequenceItem)` — ordered LUT pipeline                |

[PUBLIC_TYPE_SCOPE]: interpolation family

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [CAPABILITY]                           |
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

`convert` carries `from_reference_scale, to_reference_scale, **kwargs`; the sRGB fast paths and named-colourspace projections share the tail `illuminant, chromatic_adaptation_transform, apply_cctf_encoding` (`apply_cctf_decoding` on the inverse). `chromatic_adaptation` keys `method=` over `CHROMATIC_ADAPTATION_METHODS`.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]         | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------- | :--------------------- | :--------------------------------------------- |
|  [01]   | `convert(a, source, target, *, …)`                           | universal gateway      | single entry for all model-pair conversions    |
|  [02]   | `chromatic_adaptation(XYZ, XYZ_w, XYZ_wr, method, **kwargs)` | adaptation transform   | registry-keyed adaptation between white points |
|  [03]   | `XYZ_to_sRGB(XYZ, …)`                                        | fast path              | D65-normalised XYZ to sRGB                     |
|  [04]   | `sRGB_to_XYZ(RGB, …)`                                        | fast path              | sRGB to D65-normalised XYZ                     |
|  [05]   | `XYZ_to_RGB(XYZ, colourspace, …)`                            | colourspace projection | XYZ to named RGB space                         |
|  [06]   | `RGB_to_XYZ(RGB, colourspace, …)`                            | colourspace projection | named RGB to XYZ                               |

[ENTRYPOINT_SCOPE]: spectral and CCTF operations

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

`XYZ_to_CIECAM02`/`XYZ_to_CIECAM16`/`XYZ_to_CAM16`/`XYZ_to_Hunt` span the CAM row and `XYZ_to_Oklab`/`XYZ_to_ICtCp`/`XYZ_to_OSA_UCS`/`XYZ_to_hdr_CIELab` the perceptual row, each taking `(XYZ)` with model kwargs. `describe_conversion_path(source, target, mode='Short', …)` prints through `print_callable` and returns nothing; a caller needing the path as data walks `colour.graph.conversion_path(source, target)`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]         | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------- | :--------------------- | :--------------------------------------- |
|  [01]   | `XYZ_to_Lab(XYZ, illuminant)` / `Lab_to_XYZ(Lab, illuminant)`      | Lab projection/inverse | CIE 1976 L\*a\*b\* and its inverse       |
|  [02]   | `XYZ_to_xy(XYZ, illuminant)` / `xy_to_XYZ(xy)` / `XYZ_to_xyY(XYZ)` | chromaticity           | XYZ to/from CIE 1931 `xy`/`xyY`          |
|  [03]   | `XYZ_to_CIECAM02` / … / `XYZ_to_Hunt`                              | CAM appearance         | appearance-model transforms              |
|  [04]   | `XYZ_to_Oklab` / … / `XYZ_to_hdr_CIELab`                           | perceptual space       | uniform perceptual spaces                |
|  [05]   | `domain_range_scale` / `get_…` / `set_…`                           | scale context          | `'reference'`/`'1'`/`'100'` mode control |
|  [06]   | `describe_conversion_path(source, target, …)`                      | graph query            | print the path between two colour models |

[ENTRYPOINT_SCOPE]: RGB-space, transfer-function, and colour-correction operations

`oetf`/`eotf`/`ootf` share `(value, function=…)` selecting the named broadcast transfer functions over the disjoint registries `OETFS`/`EOTFS`/`OOTFS`, so a kind-curve pairing proves against its owning registry before dispatch. `RGB_to_RGB`/`matrix_RGB_to_RGB` share `(input_colourspace, output_colourspace, chromatic_adaptation_transform='CAT02')`, `RGB_to_RGB` leading with `RGB` and adding `apply_cctf_decoding`/`apply_cctf_encoding`; the correction rows key `method=` over `Cheung 2004`/`Finlayson 2015`/`Vandermonde`.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]       | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------- | :------------------- | :---------------------------------------------- |
|  [01]   | `RGB_to_RGB(RGB, …)`                                       | RGB-space conversion | direct named-RGB→named-RGB, no XYZ round-trip   |
|  [02]   | `matrix_RGB_to_RGB(…)`                                     | RGB matrix           | the 3×3 adaptation-baked RGB→RGB matrix         |
|  [03]   | `normalised_primary_matrix(primaries, whitepoint)`         | RGB matrix           | the NPM from primaries + whitepoint             |
|  [04]   | `oetf(value, …)` / `eotf(value, …)` / `ootf(value, …)`     | transfer function    | scene/display/end-to-end transfer trio          |
|  [05]   | `colour_correction(RGB, M_T, M_R, method='Cheung 2004')`   | colour correction    | apply a measured-vs-reference CCM               |
|  [06]   | `matrix_colour_correction(M_T, M_R, method='Cheung 2004')` | colour correction    | derive the CCM from measured/reference matrices |
|  [07]   | `apply_matrix_colour_correction(RGB, CCM)`                 | colour correction    | apply a derived CCM to an RGB array             |

[ENTRYPOINT_SCOPE]: CCT, wavelength, and colour-quality operations

CCT rows share `(coords, method)`; `dominant_wavelength`/`complementary_wavelength` take `(xy, xy_n)`; `whiteness`/`yellowness` take `(XYZ[, XYZ_0], method)` and `colour_fidelity_index`/`colour_rendering_index` take `(sd_test, method)`.

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]       | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------- | :------------------- | :------------------------------------------------- |
|  [01]   | `CCT_to_xy` / `xy_to_CCT` / `CCT_to_uv` / `uv_to_CCT` | CCT transform        | CCT to/from `xy`/`uv` chromaticity                 |
|  [02]   | `wavelength_to_XYZ(wavelength, cmfs)`                 | spectral locus       | single-wavelength to XYZ via a CMFS                |
|  [03]   | `dominant_wavelength` / `complementary_wavelength`    | spectral analysis    | dominant/complementary wavelength vs a white point |
|  [04]   | `whiteness` / `yellowness`                            | colour index         | whiteness and yellowness indices                   |
|  [05]   | `colour_fidelity_index` / `colour_rendering_index`    | light-source quality | TM-30 / CIE 2017 fidelity and CIE CRI              |
|  [06]   | `sd_to_aces_relative_exposure_values(sd, illuminant)` | scene-referred       | SPD to ACES2065-1 relative exposure values         |
|  [07]   | `msds_to_XYZ(msds, ...)`                              | scene-referred       | multi-spectral batch SPD to XYZ                    |

[ENTRYPOINT_SCOPE]: CVD, reflectance recovery, and Munsell notation

`matrix_cvd_Machado2009(deficiency, severity)` returns the 3×3 anomalous-trichromacy matrix for a `'Protanomaly'`/`'Deuteranomaly'`/`'Tritanomaly'` deficiency at a `[0, 1]` severity — physiologically-graded CVD a binary filter cannot express. `XYZ_to_sd(XYZ, method='Meng 2015', **kwargs)` recovers a reflectance `SpectralDistribution` from an XYZ seed over `'Gaussian'`/`'Jakob 2019'`/`'Mallett 2019'`/`'Meng 2015'`/`'Otsu 2018'`/`'Smits 1999'`. `munsell_colour_to_xyY`/`xyY_to_munsell_colour` map a Munsell notation string to and from CIE xyY.

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]       | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------ | :------------------- | :---------------------------------------------------- |
|  [01]   | `matrix_cvd_Machado2009(deficiency, severity)`    | CVD matrix           | severity-graded anomalous-trichromacy 3×3 matrix      |
|  [02]   | `XYZ_to_sd(XYZ, method='Meng 2015', …)`           | reflectance recovery | XYZ to a recovered reflectance `SpectralDistribution` |
|  [03]   | `munsell_colour_to_xyY` / `xyY_to_munsell_colour` | Munsell notation     | Munsell name to/from CIE xyY                          |

[ENTRYPOINT_SCOPE]: dataset registries

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY]         | [CAPABILITY]                                                                             |
| :-----: | :------------------ | :--------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `MSDS_CMFS`         | CMFS registry          | keyed colour-matching-function multi-SPDs (e.g. `'CIE 1931 2 Degree Standard Observer'`) |
|  [02]   | `SDS_ILLUMINANTS`   | illuminant SPDs        | keyed illuminant spectral distributions (`'D65'`, `'A'`, `'FL2'`, …)                     |
|  [03]   | `CCS_ILLUMINANTS`   | illuminant whitepoints | keyed `xy` chromaticity whitepoints per standard observer                                |
|  [04]   | `SDS_LIGHT_SOURCES` | light-source SPDs      | keyed real light-source spectral distributions                                           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `convert` is the universal entry keyed on `source`/`target` model strings; the named transforms (`XYZ_to_Lab`, `XYZ_to_CIECAM02`, `CCT_to_xy`) are direct rows sharing the same `method=`/`**kwargs` axis, never a per-method wrapper. Appearance models `ZCAM`/`Hellwig 2022`/`Kim 2009`/`RLAB`/`Hunt` register as `convert` model nodes, named as convert targets rather than per-model wrappers.
- Method selection is a registry-key row: `CHROMATIC_ADAPTATION_METHODS`, `DELTA_E_METHODS`, `CCTF_ENCODINGS`, `CCTF_DECODINGS`, `SD_TO_XYZ_METHODS`, and the disjoint transfer maps `OETFS`/`EOTFS`/`OOTFS` enumerate the admitted strings; `cctf_encoding`/`cctf_decoding` are the generic gamma/log rows while `oetf`/`eotf`/`ootf` select the named broadcast functions.
- Domain-range scale binds once at the boundary through the `domain_range_scale` context manager or `set_domain_range_scale`; interior calls never toggle it. Modes: `'reference'`, `'1'`, `'100'`.
- `RGB_to_RGB` bakes chromatic adaptation and optional CCTF into one call, so an HDR or wide-gamut RGB→RGB conversion is one call rather than a hand-chained XYZ round-trip; `colour_correction` derives and applies a measured-to-reference CCM keyed by `method=`.
- Named registries are looked up, never reconstructed: `RGB_COLOURSPACES` carries primaries, whitepoint, encode/decode CCTFs, and derivable XYZ matrices; `MSDS_CMFS`/`SDS_ILLUMINANTS`/`CCS_ILLUMINANTS`/`SDS_LIGHT_SOURCES` carry the CMFS, illuminant, and light-source datasets.
- `SpectralShape(start, end, interval)` drives wavelength grids; SPDs align via `SpectralDistribution.align(shape)` and interpolate through the registered interpolator family, `.copy()` cloning before a mutating `align`; `.domain`/`.values` expose the wavelength and sample axes. LUTs compose into a `LUTSequence`; `read_LUT`/`write_LUT` own `.cube`/`.clf`/`.csp`.
- SciPy and Matplotlib are optional accelerators: interpolation degrades and `plot_*` surfaces raise `ColourUsageWarning` when absent.

[LOCAL_ADMISSION]:
- Colour-model conversion enters via `convert` or a named transform; method selection is the `method=`/registry-key row, never a wrapper re-exposing an individual transform.
- Domain-range mode sets once at the boundary; interior calls hold it fixed.
- LUT pipelines persist as `LUTSequence` objects; `.cube`/`.clf`/`.csp` files read through `read_LUT`.
- SPD construction uses `SpectralDistribution(data, domain)` with an explicit `SpectralShape`; raw dict intake is the data-ingestion-boundary fallback.

[STACKING]:
- `coloraide`(`.api/coloraide.md`): the numeric XYZ/`xy` vector is the seam — a measured XYZ from `sd_to_XYZ` or an appearance coordinate enters `coloraide` as `Color(('xyz-d65', coords))` for the per-color gamut/CSS/CVD/palette legs `coloraide` owns; `colour` never parses CSS strings, `coloraide` never runs spectral sd→XYZ integration.
- imaging codec: `read_image`/`write_image` route to Imageio/OpenImageIO float buffers; the multi-bit-depth pixel array feeds the imaging owner, the colour transform running on the array before encode, never on a serialized blob.
- figures owner: chromaticity (`XYZ_to_xy`), spectral locus (`wavelength_to_XYZ`), and CCT (`CCT_to_xy`) coordinates feed the plot owner as numeric arrays; `plot_*` stays Matplotlib-gated at the figures owner, never invoked in the colour rail.
- universal rail (`libs/python/.api`): numeric XYZ/`xy`/Lab ndarrays project into one `derive#DERIVE` `ColorModel`/`Derivation` case; `numpy` materializes the coordinate arrays and the typing rail refines their rank, dtype, and finiteness.

[RAIL_LAW]:
- Package: `colour-science`
- Owns: spectral-distribution representation, XYZ/RGB/Lab/LCH/CAM colour transforms, CCTF encode/decode, broadcast transfer functions, chromatic adaptation, CCT and chromaticity transforms, dominant/complementary wavelength, whiteness/yellowness and colour-quality indices, reflectance recovery, Munsell notation, LUT IO, image IO, dataset registries, and colour-difference metrics.
- Accept: `convert` or named transforms for colour-model conversion; `SpectralDistribution`/`MultiSpectralDistributions` for SPD work; `RGB_COLOURSPACES`/`MSDS_CMFS`/`SDS_ILLUMINANTS` for named-registry lookup; XYZ/`xy` arrays handed to `coloraide` for CSS/gamut presentation.
- Reject: a hand-rolled CCTF, XYZ transform matrix, CCT estimation, or interpolation where `colour` owns the operation; `coloraide` for spectral sd→XYZ integration or `colour` for CSS string parsing; a per-method wrapper where `method=` selects a registry row; a Matplotlib `plot_*` surface inside the colour rail.
