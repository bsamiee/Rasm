# [PY_ARTIFACTS_API_COLOUR_SCIENCE]

`colour` supplies the complete colorimetry surface for the artifacts colour rail: spectral distribution representation (`SpectralDistribution`, `MultiSpectralDistributions`, `SpectralShape`), RGB colourspace management (`RGB_Colourspace`, `RGB_COLOURSPACES`), XYZ/Lab/LCH/CAM colour appearance model transforms, LUT operators (`LUT1D`, `LUT3D`, `LUT3x1D`, `LUTSequence`), CCTF encoding/decoding, chromatic adaptation, colour difference metrics, correlated-colour-temperature and chromaticity transforms, dataset registries (CMFS/illuminants/light sources), and a universal `convert` gateway that dispatches across all model pairs. The package owner drives colour science entirely through these surfaces and never re-implements a conversion or interpolation that `colour` owns; numeric coordinate arrays it returns feed the `coloraide` engine (CSS/gamut/CVD authoring), the figures owner, and the imaging codec, with `colour` owning the spectral/colorimetric truth and `coloraide` owning the per-color CSS/gamut-map presentation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colour`
- package: `colour-science`
- import: `colour`
- owner: `artifacts`
- rail: colour
- version: `0.4.7`
- license: `BSD-3-Clause`
- entry points: none (library only)
- capability: spectral distribution representation and resampling, XYZ/RGB/Lab/LCH/CAM appearance transforms, universal `convert` model-pair gateway, direct named-RGB→RGB conversion (`RGB_to_RGB`) and primary/adaptation matrices, scene/display transfer functions (`oetf`/`eotf`/`ootf`), measured colour correction (`colour_correction`), chromatic adaptation, CCTF encode/decode, colour difference (`delta_E`), LUT IO (`.cube`/`.clf`/`.csp`), image IO, CCT and chromaticity transforms, dominant/complementary wavelength, whiteness/yellowness and colour-quality indices, and dataset registries for CMFS, illuminants, and light sources

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

`convert` carries the scale kwargs `from_reference_scale, to_reference_scale, **kwargs`; the sRGB fast paths and named-colourspace projections share the tail `illuminant, chromatic_adaptation_transform, apply_cctf_encoding` (`apply_cctf_decoding` on the inverse). `chromatic_adaptation` keys `method=` over `CHROMATIC_ADAPTATION_METHODS` — CIE 1994 / CMCCAT2000 / Fairchild 1990 / Li 2025 / Von Kries / vK20 / Zhai 2018.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]         | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------- | :--------------------- | :--------------------------------------------- |
|  [01]   | `convert(a, source, target, *, …)`                           | universal gateway      | single entry for all model-pair conversions    |
|  [02]   | `chromatic_adaptation(XYZ, XYZ_w, XYZ_wr, method, **kwargs)` | adaptation transform   | registry-keyed adaptation between white points |
|  [03]   | `XYZ_to_sRGB(XYZ, …)`                                        | fast path              | D65-normalised XYZ to sRGB                     |
|  [04]   | `sRGB_to_XYZ(RGB, …)`                                        | fast path              | sRGB to D65-normalised XYZ                     |
|  [05]   | `XYZ_to_RGB(XYZ, colourspace, …)`                            | colourspace projection | XYZ to named RGB space                         |
|  [06]   | `RGB_to_XYZ(RGB, colourspace, …)`                            | colourspace projection | named RGB to XYZ                               |

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

The CAM and perceptual-space rows each take `(XYZ)` (plus model kwargs). The CAM row spans `XYZ_to_CIECAM02` / `XYZ_to_CIECAM16` / `XYZ_to_CAM16` / `XYZ_to_Hunt`, the perceptual row `XYZ_to_Oklab` / `XYZ_to_ICtCp` / `XYZ_to_OSA_UCS` / `XYZ_to_hdr_CIELab`, and the scale-context trio `domain_range_scale(scale)` / `get_domain_range_scale()` / `set_domain_range_scale(scale)`. `describe_conversion_path` carries `(source, target, mode='Short', width=79, padding=3, print_callable=print)` and emits the path description through `print_callable`, returning nothing; `mode='Long'` plus a `print_callable` sink captures the printed lines, and a caller needing the path as data walks `colour.graph.conversion_path(source, target)` instead.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]         | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------- | :--------------------- | :--------------------------------------- |
|  [01]   | `XYZ_to_Lab(XYZ, illuminant)` / `Lab_to_XYZ(Lab, illuminant)`      | Lab projection/inverse | CIE 1976 L\*a\*b\* and its inverse       |
|  [02]   | `XYZ_to_xy(XYZ, illuminant)` / `xy_to_XYZ(xy)` / `XYZ_to_xyY(XYZ)` | chromaticity           | XYZ to/from CIE 1931 `xy`/`xyY`          |
|  [03]   | `XYZ_to_CIECAM02` / … / `XYZ_to_Hunt`                              | CAM appearance         | appearance-model transforms              |
|  [04]   | `XYZ_to_Oklab` / … / `XYZ_to_hdr_CIELab`                           | perceptual space       | uniform perceptual spaces                |
|  [05]   | `domain_range_scale` / `get_…` / `set_…`                           | scale context          | `'reference'`/`'1'`/`'100'` mode control |
|  [06]   | `describe_conversion_path(source, target, …)`                      | graph query            | print the path between two colour models |

[ENTRYPOINT_SCOPE]: RGB-space, transfer-function, and colour-correction operations
- rail: colour

The transfer trio shares `(value, function=…)` with defaults `'ITU-R BT.709'` (oetf) / `'ITU-R BT.1886'` (eotf) / `'ITU-R BT.2100 PQ'` (ootf); `OETFS`/`EOTFS`/`OOTFS` are disjoint registries — `'ITU-R BT.709'` keys only `OETFS`, `'ITU-R BT.1886'` only `EOTFS`, `OOTFS` admits only the `'ITU-R BT.2100 PQ'`/`'ITU-R BT.2100 HLG'` pair — so a kind-curve pairing is proven against the owning registry before dispatch; the correction rows key `method=` over `Cheung 2004`/`Finlayson 2015`/`Vandermonde`. `RGB_to_RGB`/`matrix_RGB_to_RGB` share `(input_colourspace, output_colourspace, chromatic_adaptation_transform='CAT02')` (`RGB_to_RGB` leads with `RGB` and adds `apply_cctf_decoding`/`apply_cctf_encoding`).

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
- rail: colour

The CCT rows share `(coords, method)`; `dominant_wavelength`/`complementary_wavelength` take `(xy, xy_n)`; the index/quality rows key their variant on `method` (`whiteness`/`yellowness` take `(XYZ[, XYZ_0], method)`, `colour_fidelity_index`/`colour_rendering_index` take `(sd_test, method)`).

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
- rail: colour

`matrix_cvd_Machado2009(deficiency, severity)` returns the 3x3 anomalous-trichromacy matrix for a `'Protanomaly'`/`'Deuteranomaly'`/`'Tritanomaly'` deficiency at a `[0, 1]` severity — the physiologically-graded CVD simulation the `derive#DERIVE` `Simulate` arm applies where a binary filter row cannot express severity. `XYZ_to_sd(XYZ, method='Meng 2015', **kwargs)` recovers a reflectance `SpectralDistribution` from an XYZ seed over `'Jakob 2019'`/`'Mallett 2019'`/`'Meng 2015'`/`'Otsu 2018'`/`'Smits 1999'` — the RGB-to-spectral round-trip the `Recover` arm starts a material color plane from. The Munsell pair `munsell_colour_to_xyY(munsell_colour)`/`xyY_to_munsell_colour(xyY)` maps a Munsell notation string to and from CIE xyY — the `Notate` arm's bidirectional discriminant.

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]       | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------ | :------------------- | :---------------------------------------------------- |
|  [01]   | `matrix_cvd_Machado2009(deficiency, severity)`    | CVD matrix           | severity-graded anomalous-trichromacy 3x3 matrix      |
|  [02]   | `XYZ_to_sd(XYZ, method='Meng 2015', …)`           | reflectance recovery | XYZ to a recovered reflectance `SpectralDistribution` |
|  [03]   | `munsell_colour_to_xyY` / `xyY_to_munsell_colour` | Munsell notation     | Munsell name to/from CIE xyY                          |

The appearance roster the universal `convert` gateway spans beside CIECAM02/CAM16: `XYZ_to_ZCAM`/`XYZ_to_Hellwig2022`/`XYZ_to_Kim2009`/`XYZ_to_RLAB`/`XYZ_to_Hunt` register `'ZCAM'`/`'Hellwig 2022'`/`'Kim 2009'`/`'RLAB'`/`'Hunt'` as `convert` model nodes, so a derive `ColorModel` names them as convert targets, never a per-model wrapper.

[ENTRYPOINT_SCOPE]: dataset registries
- rail: colour

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY]         | [CAPABILITY]                                                                             |
| :-----: | :------------------ | :--------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `MSDS_CMFS`         | CMFS registry          | keyed colour-matching-function multi-SPDs (e.g. `'CIE 1931 2 Degree Standard Observer'`) |
|  [02]   | `SDS_ILLUMINANTS`   | illuminant SPDs        | keyed illuminant spectral distributions (`'D65'`, `'A'`, `'FL2'`, …)                     |
|  [03]   | `CCS_ILLUMINANTS`   | illuminant whitepoints | keyed `xy` chromaticity whitepoints per standard observer                                |
|  [04]   | `SDS_LIGHT_SOURCES` | light-source SPDs      | keyed real light-source spectral distributions                                           |

## [04]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `colour`; 408 public `__all__` symbols covering spectral, colorimetric, appearance, adaptation, difference, CCT, chromaticity, IO, dataset, and interpolation domains
- domain-range scale: three modes — `'reference'` (native), `'1'` (normalised), `'100'` (percentage); use the `domain_range_scale` context manager at the boundary or `set_domain_range_scale` for the call stack; interior calls never toggle it
- dispatch: `convert` is the universal entry point keyed on `source`/`target` model strings; the named transforms (`XYZ_to_Lab`, `XYZ_to_CIECAM02`, `CCT_to_xy`, …) are the direct rows and accept the same `**kwargs`/`method=` axis for method selection — never a per-method wrapper
- method registries: `CHROMATIC_ADAPTATION_METHODS`, `DELTA_E_METHODS`, `CCTF_ENCODINGS`, `CCTF_DECODINGS`, `SD_TO_XYZ_METHODS`, and the transfer-function maps `OETFS`/`EOTFS`/`OOTFS` enumerate the admitted method strings; the `function=`/`method=` argument is a registry-key row, never a parallel function
- transfer vs CCTF: `cctf_encoding`/`cctf_decoding` are the generic gamma/log CCTF rows, while `oetf`/`eotf`/`ootf` select the named broadcast transfer functions (`'ITU-R BT.709'`, `'ITU-R BT.1886'`, `'ITU-R BT.2100 PQ'`, …) — distinct registries, both `function=` rows; `RGB_to_RGB` carries `chromatic_adaptation_transform=` and optional `apply_cctf_decoding`/`apply_cctf_encoding`, so an HDR or wide-gamut RGB→RGB conversion is one call, never a hand-chained XYZ round-trip; `colour_correction` derives and applies a measured-to-reference CCM keyed by `method=` (`Cheung 2004`/`Finlayson 2015`/`Vandermonde`)
- colourspace catalog: `RGB_COLOURSPACES` is a keyed registry; `RGB_Colourspace` instances carry primaries, whitepoint, encoding/decoding CCTFs, and derivable XYZ matrices
- dataset registries: `MSDS_CMFS`, `SDS_ILLUMINANTS`, `CCS_ILLUMINANTS`, `SDS_LIGHT_SOURCES` are keyed dataset maps; CMFS and illuminant SPDs are looked up by name, never reconstructed
- spectral: `SpectralShape(start, end, interval)` drives wavelength grids; SPDs align via `SpectralDistribution.align(shape)` and interpolate/extrapolate through the registered interpolator family (`SpragueInterpolator` for uniform CIE work, `Extrapolator` for boundary extension); `SpectralDistribution.domain`/`.values` expose the wavelength and sample `ndarray` axes, and `.copy()` clones before a mutating `align`
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
