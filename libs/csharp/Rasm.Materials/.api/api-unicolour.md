# [API_UNICOLOUR]

Package: `Wacton.Unicolour` · Version: `7.0.0` · License: MIT (William Acton 2022-2026)  
DLL: `netstandard2.0/Wacton.Unicolour.dll` · Restore proof: `dotnet restore` → resolved 7.0.0, nuget.org  
`<PackageVersion Include="Wacton.Unicolour" Version="7.0.0" />`

---

## [COLOUR_SPACE_ENUM]

`ColourSpace` is the discriminant for all construction and conversion. All 40 members verified.

| [MEMBER]    | [SIGNATURE]                                                    | [USED_BY]                      | [EVIDENCE]    |
| ----------- | -------------------------------------------------------------- | ------------------------------ | ------------- |
| `Rgb`       | `ColourSpace.Rgb`                                              | COLOR_SPACE_AXIS construction  | ilspycmd enum |
| `Rgb255`    | `ColourSpace.Rgb255` — remapped to `Rgb` / 255 on construction | COLOR_SPACE_AXIS               | ilspycmd      |
| `RgbLinear` | `ColourSpace.RgbLinear`                                        | #BSDF_MODEL linear light       | ilspycmd      |
| `Hsb`       | `ColourSpace.Hsb`                                              |                                | ilspycmd      |
| `Hsl`       | `ColourSpace.Hsl`                                              |                                | ilspycmd      |
| `Hwb`       | `ColourSpace.Hwb`                                              |                                | ilspycmd      |
| `Hsi`       | `ColourSpace.Hsi`                                              |                                | ilspycmd      |
| `Xyz`       | `ColourSpace.Xyz`                                              | #PHOTOMETRIC tristimulus       | ilspycmd      |
| `Xyy`       | `ColourSpace.Xyy`                                              | chromaticity construction      | ilspycmd      |
| `Wxy`       | `ColourSpace.Wxy`                                              | dominant wavelength            | ilspycmd      |
| `Lab`       | `ColourSpace.Lab`                                              | delta-E76/94/2000, gamut map   | ilspycmd      |
| `Lchab`     | `ColourSpace.Lchab`                                            | chroma-reduction gamut map     | ilspycmd      |
| `Luv`       | `ColourSpace.Luv`                                              |                                | ilspycmd      |
| `Lchuv`     | `ColourSpace.Lchuv`                                            |                                | ilspycmd      |
| `Hsluv`     | `ColourSpace.Hsluv`                                            | perceptual hue-saturation      | ilspycmd      |
| `Hpluv`     | `ColourSpace.Hpluv`                                            |                                | ilspycmd      |
| `Ypbpr`     | `ColourSpace.Ypbpr`                                            |                                | ilspycmd      |
| `Ycbcr`     | `ColourSpace.Ycbcr`                                            |                                | ilspycmd      |
| `Ycgco`     | `ColourSpace.Ycgco`                                            |                                | ilspycmd      |
| `Yuv`       | `ColourSpace.Yuv`                                              |                                | ilspycmd      |
| `Yiq`       | `ColourSpace.Yiq`                                              |                                | ilspycmd      |
| `Ydbdr`     | `ColourSpace.Ydbdr`                                            |                                | ilspycmd      |
| `Tsl`       | `ColourSpace.Tsl`                                              |                                | ilspycmd      |
| `Xyb`       | `ColourSpace.Xyb`                                              |                                | ilspycmd      |
| `Lms`       | `ColourSpace.Lms`                                              | chromatic adaptation           | ilspycmd      |
| `Ipt`       | `ColourSpace.Ipt`                                              |                                | ilspycmd      |
| `Ictcp`     | `ColourSpace.Ictcp`                                            | DeltaE.Itp (HDR perceptual)    | ilspycmd      |
| `Jzazbz`    | `ColourSpace.Jzazbz`                                           |                                | ilspycmd      |
| `Jzczhz`    | `ColourSpace.Jzczhz`                                           | DeltaE.Z                       | ilspycmd      |
| `Oklab`     | `ColourSpace.Oklab`                                            | DeltaE.Ok, gamut mixing        | ilspycmd      |
| `Oklch`     | `ColourSpace.Oklch`                                            | OklchChromaReduction gamut map | ilspycmd      |
| `Okhsv`     | `ColourSpace.Okhsv`                                            |                                | ilspycmd      |
| `Okhsl`     | `ColourSpace.Okhsl`                                            |                                | ilspycmd      |
| `Okhwb`     | `ColourSpace.Okhwb`                                            |                                | ilspycmd      |
| `Oklrab`    | `ColourSpace.Oklrab`                                           |                                | ilspycmd      |
| `Oklrch`    | `ColourSpace.Oklrch`                                           |                                | ilspycmd      |
| `Cam02`     | `ColourSpace.Cam02`                                            | DeltaE.Cam02                   | ilspycmd      |
| `Cam16`     | `ColourSpace.Cam16`                                            | DeltaE.Cam16                   | ilspycmd      |
| `Hct`       | `ColourSpace.Hct`                                              |                                | ilspycmd      |
| `Munsell`   | `ColourSpace.Munsell`                                          |                                | ilspycmd      |

**NOT in ColourSpace enum (phantom check):** `DisplayP3`, `Rec2020`, `AdobeRGB`, `ProPhoto`, `Aces2065_1`, `AcesCg`, `Oklab` (already listed above). Display primaries are `RgbConfiguration` statics — NOT enum cases. Constructing a Display P3 colour requires `new Configuration(RgbConfiguration.DisplayP3)` then `ColourSpace.Rgb`.

---

## [CONSTRUCTION]

All `Unicolour` construction routes verified from `Wacton.Unicolour.Unicolour`.

| [MEMBER]                       | [SIGNATURE]                                                                                   | [USED_BY]                                         | [EVIDENCE]    |
| ------------------------------ | --------------------------------------------------------------------------------------------- | ------------------------------------------------- | ------------- |
| ctor (space + triple)          | `new Unicolour(ColourSpace, double, double, double, double alpha = 1.0)`                      | COLOR_SPACE_AXIS                                  | ilspycmd ctor |
| ctor (space + tuple3)          | `new Unicolour(ColourSpace, (double first, double second, double third), double alpha = 1.0)` |                                                   | ilspycmd      |
| ctor (space + tuple4 w/ alpha) | `new Unicolour(ColourSpace, (double first, double second, double third, double alpha))`       |                                                   | ilspycmd      |
| ctor (config + space + triple) | `new Unicolour(Configuration, ColourSpace, double, double, double, double alpha = 1.0)`       | COLOR_SPACE_AXIS with Display P3 / Rec2020 config | ilspycmd      |
| ctor (hex)                     | `new Unicolour(string hex)`                                                                   |                                                   | ilspycmd      |
| ctor (config + hex)            | `new Unicolour(Configuration, string hex)`                                                    |                                                   | ilspycmd      |
| ctor (grey)                    | `new Unicolour(ColourSpace, double grey, double alpha = 1.0)`                                 |                                                   | ilspycmd      |
| ctor (chromaticity)            | `new Unicolour(Chromaticity, double luminance = 1.0)`                                         | #PHOTOMETRIC white-point construction             | ilspycmd      |
| ctor (config + chromaticity)   | `new Unicolour(Configuration, Chromaticity, double luminance = 1.0)`                          |                                                   | ilspycmd      |
| ctor (CCT)                     | `new Unicolour(double cct, Locus locus = Locus.Blackbody, double luminance = 1.0)`            | #PHOTOMETRIC blackbody                            | ilspycmd      |
| ctor (Spd)                     | `new Unicolour(Spd)`                                                                          | #PHOTOMETRIC spectral power distribution → XYZ    | ilspycmd      |
| ctor (config + Spd)            | `new Unicolour(Configuration, Spd)`                                                           |                                                   | ilspycmd      |
| ctor (Pigment[] + weights)     | `new Unicolour(Pigment[], double[])`                                                          | Kubelka-Munk pigment mixing                       | ilspycmd      |

---

## [CONVERSION_ACCESSORS]

Lazy-evaluated properties on `Unicolour`; all spelled exactly as below.

| [MEMBER]              | [SIGNATURE]                                   | [USED_BY]                              | [EVIDENCE]        |
| --------------------- | --------------------------------------------- | -------------------------------------- | ----------------- |
| `.Rgb`                | `Rgb`                                         | sRGB byte output                       | ilspycmd property |
| `.RgbLinear`          | `RgbLinear`                                   | #BSDF_MODEL linear-light pipeline      | ilspycmd          |
| `.Xyz`                | `Xyz`                                         | #PHOTOMETRIC tristimulus               | ilspycmd          |
| `.Xyy`                | `Xyy`                                         | chromaticity, dominant wavelength      | ilspycmd          |
| `.Lab`                | `Lab`                                         | delta-E, perceptual sort               | ilspycmd          |
| `.Lchab`              | `Lchab`                                       | chroma-reduction gamut mapping         | ilspycmd          |
| `.Oklab`              | `Oklab`                                       | DeltaE.Ok, perceptual interpolation    | ilspycmd          |
| `.Oklch`              | `Oklch`                                       | OklchChromaReduction gamut mapping     | ilspycmd          |
| `.Ictcp`              | `Ictcp`                                       | DeltaE.Itp (HDR), Rec.2100             | ilspycmd          |
| `.Jzczhz`             | `Jzczhz`                                      | DeltaE.Z                               | ilspycmd          |
| `.Cam02`              | `Cam02`                                       | DeltaE.Cam02                           | ilspycmd          |
| `.Cam16`              | `Cam16`                                       | DeltaE.Cam16                           | ilspycmd          |
| `.Lms`                | `Lms`                                         | chromatic adaptation                   | ilspycmd          |
| `.Hsluv`              | `Hsluv`                                       | perceptual hue-uniform                 | ilspycmd          |
| `.Hpluv`              | `Hpluv`                                       |                                        | ilspycmd          |
| `.Hct`                | `Hct`                                         | Material You / dynamic color           | ilspycmd          |
| `.Wxy`                | `Wxy`                                         | dominant wavelength, excitation purity | ilspycmd          |
| `.RelativeLuminance`  | `double`                                      | WCAG contrast                          | ilspycmd          |
| `.Chromaticity`       | `Chromaticity` — `record(double X, double Y)` | white-point comparison                 | ilspycmd          |
| `.DominantWavelength` | `double` (from `.Wxy.W`)                      | spectral locus tagging                 | ilspycmd          |
| `.ExcitationPurity`   | `double` (from `.Wxy.X`)                      | spectral locus tagging                 | ilspycmd          |
| `.Temperature`        | `Temperature`                                 | blackbody CCT readout                  | ilspycmd          |
| `.IsInRgbGamut`       | `bool`                                        | gamut-check gate                       | ilspycmd          |
| `.IsInPointerGamut`   | `bool`                                        | real-surface gamut check               | ilspycmd          |
| `.IsInMacAdamLimits`  | `bool`                                        | spectral limits check                  | ilspycmd          |
| `.IsImaginary`        | `bool`                                        | outside spectral locus                 | ilspycmd          |
| `.Hex`                | `string`                                      |                                        | ilspycmd          |
| `.Alpha`              | `Alpha`                                       |                                        | ilspycmd          |
| `.Configuration`      | `Configuration`                               | config round-trip                      | ilspycmd          |

---

## [DELTA_E]

`DeltaE` enum — all 12 members verified. Called via `unicolour.Difference(reference, DeltaE.X)`.

| [MEMBER]            | [SIGNATURE]                             | [USED_BY]                         | [EVIDENCE]                   |
| ------------------- | --------------------------------------- | --------------------------------- | ---------------------------- |
| `Cie76`             | `DeltaE.Cie76`                          | baseline perceptual diff          | ilspycmd                     |
| `Cie94`             | `DeltaE.Cie94`                          | graphic-arts weighting            | ilspycmd                     |
| `Cie94Textiles`     | `DeltaE.Cie94Textiles`                  |                                   | ilspycmd                     |
| `Ciede2000`         | `DeltaE.Ciede2000`                      | industry-standard appearance diff | #BSDF_MODEL appearance match | ilspycmd |
| `CmcAcceptability`  | `DeltaE.CmcAcceptability`               |                                   | ilspycmd                     |
| `CmcPerceptibility` | `DeltaE.CmcPerceptibility`              |                                   | ilspycmd                     |
| `Itp`               | `DeltaE.Itp` — DeltaEItp × 720 on ICtCp | HDR perceptual diff               | ilspycmd                     |
| `Z`                 | `DeltaE.Z` — on JzCzhz                  | HDR perceptual diff (Safdar 2021) | ilspycmd                     |
| `Hyab`              | `DeltaE.Hyab`                           |                                   | ilspycmd                     |
| `Ok`                | `DeltaE.Ok` — Euclidean on Oklab        | perceptual mixing                 | ilspycmd                     |
| `Cam02`             | `DeltaE.Cam02`                          | appearance model                  | ilspycmd                     |
| `Cam16`             | `DeltaE.Cam16` — 1.41 × d^0.63          | appearance model                  | ilspycmd                     |

Entry point: `double Unicolour.Difference(Unicolour reference, DeltaE deltaE)` and `double Unicolour.Contrast(Unicolour other)` (WCAG).

---

## [CONFIGURATION]

| [MEMBER]                     | [SIGNATURE]                                                                                                      | [USED_BY]            | [EVIDENCE] |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------- | -------------------- | ---------- |
| `Configuration.Default`      | `static Configuration` — sRGB/D65/Rec601/StandardRgb CAM                                                         | baseline             | ilspycmd   |
| `Configuration..ctor`        | `(RgbConfiguration?, XyzConfiguration?, YbrConfiguration?, CamConfiguration?, DynamicRange?, IccConfiguration?)` | custom working space | ilspycmd   |
| `Configuration.Rgb`          | `RgbConfiguration`                                                                                               |                      | ilspycmd   |
| `Configuration.Xyz`          | `XyzConfiguration`                                                                                               |                      | ilspycmd   |
| `Configuration.DynamicRange` | `DynamicRange`                                                                                                   | HDR/SDR tone policy  | ilspycmd   |

---

## [RGB_CONFIGURATION]

Static presets on `RgbConfiguration`. All names verified — note exact capitalization.

| [MEMBER]                 | [SIGNATURE]                                                                                                                               | [USED_BY]                    | [EVIDENCE]             |
| ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------- | ---------------------- |
| `StandardRgb`            | `static RgbConfiguration`                                                                                                                 | sRGB / COLOR_SPACE_AXIS.srgb | ilspycmd               |
| `DisplayP3`              | `static RgbConfiguration`                                                                                                                 | COLOR_SPACE_AXIS.display-p3  | ilspycmd               |
| `Rec2020`                | `static RgbConfiguration`                                                                                                                 | COLOR_SPACE_AXIS.rec2020     | ilspycmd               |
| `Rec2100Pq`              | `static RgbConfiguration`                                                                                                                 | #PHOTOMETRIC PQ transfer     | ilspycmd               |
| `Rec2100Hlg`             | `static RgbConfiguration`                                                                                                                 | #PHOTOMETRIC HLG transfer    | ilspycmd               |
| `A98`                    | `static RgbConfiguration`                                                                                                                 | Adobe RGB (a98-rgb)          | ilspycmd               |
| `ProPhoto`               | `static RgbConfiguration`                                                                                                                 | ProPhoto / ROMM RGB          | ilspycmd               |
| `Aces20651`              | `static RgbConfiguration`                                                                                                                 | ACES 2065-1 (scene-linear)   | #BSDF_MODEL ACES scene | ilspycmd |
| `Acescg`                 | `static RgbConfiguration`                                                                                                                 | ACEScg (scene-linear AP1)    | #BSDF_MODEL shading    | ilspycmd |
| `Acescct`                | `static RgbConfiguration`                                                                                                                 | ACEScct log                  | ilspycmd               |
| `Acescc`                 | `static RgbConfiguration`                                                                                                                 | ACEScc log                   | ilspycmd               |
| `Rec709`                 | `static RgbConfiguration`                                                                                                                 |                              | ilspycmd               |
| `RgbConfiguration..ctor` | `(Chromaticity r, Chromaticity g, Chromaticity b, WhitePoint, Func<double,double> fromLinear, Func<double,double> toLinear, string name)` | custom primaries             | ilspycmd               |

---

## [XYZ_CONFIGURATION]

| [MEMBER]                                            | [SIGNATURE]                                                            | [USED_BY]                 | [EVIDENCE] |
| --------------------------------------------------- | ---------------------------------------------------------------------- | ------------------------- | ---------- |
| `XyzConfiguration.D65`                              | `static XyzConfiguration` — Illuminant.D65, Observer.Degree2, Bradford | default                   | ilspycmd   |
| `XyzConfiguration.D50`                              | `static XyzConfiguration` — Illuminant.D50, Observer.Degree2, Bradford | ICC profile               | ilspycmd   |
| `XyzConfiguration..ctor(Illuminant, Observer, ...)` | custom illuminant/observer                                             | #PHOTOMETRIC custom white | ilspycmd   |
| `XyzConfiguration.WhitePoint`                       | `WhitePoint`                                                           |                           | ilspycmd   |
| `XyzConfiguration.Observer`                         | `Observer`                                                             |                           | ilspycmd   |

---

## [DYNAMIC_RANGE]

| [MEMBER]                      | [SIGNATURE]                                                                                                   | [USED_BY]        | [EVIDENCE] |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------- | ---------------- | ---------- |
| `DynamicRange.Standard`       | `static DynamicRange` — white 100 cd/m², max 100, min 0.1                                                     | SDR              | ilspycmd   |
| `DynamicRange.High`           | `static DynamicRange` — white 203 cd/m², max 1000, min 0                                                      | HDR default      | ilspycmd   |
| `DynamicRange..ctor`          | `(double whiteLuminance, double maxLuminance, double minLuminance, double hlgWhiteLevel = 0.75, string name)` | custom HDR range | ilspycmd   |
| `DynamicRange.WhiteLuminance` | `double`                                                                                                      |                  | ilspycmd   |
| `DynamicRange.MaxLuminance`   | `double`                                                                                                      |                  | ilspycmd   |

`Rec2100Pq` and `Rec2100Hlg` use the `FromLinear`/`ToLinear` delegates that call into internal `Pq` and `Hlg` OETF/EOTF kernels, scaled by `DynamicRange.WhiteLuminance / 203` at construction. The tone-curve math is inside the library; the consumer sets `DynamicRange` in `Configuration` only.

---

## [SPECTRAL_SPD]

`Spd` extends `SpectralCoefficients` — power distribution → XYZ via `new Unicolour(config, Spd)`.

| [MEMBER]      | [SIGNATURE]                                                                                | [USED_BY]                          | [EVIDENCE]              |
| ------------- | ------------------------------------------------------------------------------------------ | ---------------------------------- | ----------------------- |
| `Spd..ctor`   | `(int start, int interval, params double[] coefficients)` — interval must be 0, 1, or 5 nm | #PHOTOMETRIC custom illuminant SPD | ilspycmd                |
| `Spd.D65`     | `static Spd` — 300–830 nm, 1 nm interval                                                   | D65 reference illuminant           | ilspycmd                |
| `Spd.IsValid` | `bool` — interval ∈ {0, 1, 5}                                                              | validation                         | ilspycmd                |
| `Xyz.FromSpd` | internal; called by `Unicolour(config, Spd)` ctor                                          |                                    | ilspycmd Unicolour ctor |

**SPD construction path:** `new Unicolour(Configuration, Spd)` → internal `SpdToXyzTuple` → `Xyz.FromSpd(spd, xyzConfig.Observer, xyzConfig.WhitePoint)` → XYZ triplet. The reflectance → XYZ path uses `Pigment[]`/`KubelkaMunk` with `Spd` illuminant, not the `Spd` ctor directly.

---

## [GAMUT_MAP]

| [MEMBER]                                                                  | [SIGNATURE]                           | [USED_BY]                  | [EVIDENCE]       |
| ------------------------------------------------------------------------- | ------------------------------------- | -------------------------- | ---------------- |
| `GamutMap.RgbClipping`                                                    | clip each channel to [0, 1]           | fast                       | ilspycmd         |
| `GamutMap.OklchChromaReduction`                                           | reduce Oklch chroma until in gamut    | perceptual quality         | COLOR_SPACE_AXIS | ilspycmd |
| `GamutMap.WxyPurityReduction`                                             | reduce excitation purity toward white | spectral                   | ilspycmd         |
| `Unicolour.Mix(other, ColourSpace, amount, HueSpan, premultiplyAlpha)`    | `Unicolour`                           | interpolation in any space | ilspycmd         |
| `Unicolour.Palette(other, ColourSpace, count, HueSpan, premultiplyAlpha)` | `IEnumerable<Unicolour>`              |                            | ilspycmd         |

Gamut mapping is exposed only through `GamutMapping.ToRgbGamut` (internal static). Public access is via `Unicolour.IsInRgbGamut` check + `Mix`/`Palette` for interpolated output, or by constructing with a mapped space. No public `MapToGamut` instance method exists — the three `GamutMap` rows feed only the internal `GamutMapping` kernel.

---

## [CHROMATICITY_WHITE_POINT]

| [MEMBER]                                 | [SIGNATURE]                                                | [USED_BY]                           | [EVIDENCE]                |
| ---------------------------------------- | ---------------------------------------------------------- | ----------------------------------- | ------------------------- |
| `Chromaticity`                           | `record(double X, double Y)` with `.U`, `.V`, `.Uv`, `.Xy` | white-point and dominant-wavelength | ilspycmd                  |
| `Chromaticity.FromUv(u, v)`              | `static Chromaticity`                                      | UCS conversion                      | ilspycmd                  |
| `WhitePoint`                             | class — wraps `Chromaticity`                               | illuminant white point              | ilspycmd                  |
| `Illuminant.D65` / `Illuminant.D50`      | `static Illuminant`                                        | standard illuminants                | ilspycmd XyzConfiguration |
| `Observer.Degree2` / `Observer.Degree10` | `static Observer`                                          | CIE observer                        | ilspycmd XyzConfiguration |

---

## [NOT_COVERED]

The following concerns fall outside Wacton.Unicolour 7.0.0 and must be authored as local kernels or routed through another admitted package:

| [CONCERN]                                                | [REASON]                                                                                                                                                                                                                                                         | [OWNER_ACTION]                                                                                         |
| -------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| RGB → SPD spectral upsampling                            | No upsampling kernel; `Spd` only accepts measured/tabulated coefficients. The Pigment path does Kubelka-Munk reflectance mixing, not scene radiance upsampling.                                                                                                  | Author-kernel: Smits (1999) or Jakob-Hanika (2019) coefficient table for appearance engine upsampling. |
| ICC profile parsing and PCS round-trip                   | `IccConfiguration` is present but the internal `Icc` namespace parses ICC v2/v4 profiles — the public API surface for `IccConfiguration` is limited to passing a profile path; full PCS transform inspection is internal. Not suitable as a general ICC library. | Admit `Cms.Net` or `lcms2` P/Invoke for full ICC PCS pipeline if needed.                               |
| ACES RRT / ODT tone-map operators                        | `Aces20651` and `Acescg` are primaries + linear transfer only. No RRT (Reference Rendering Transform) or Output Device Transform is implemented. The ACES pipeline (IDT → RRT → ODT) is entirely absent.                                                         | Author-kernel: ACES CTL-equivalent RRT/ODT math or admit a dedicated tone-mapping package.             |
| Scene-referred HDR tone mapping (ACES, Reinhard, Filmic) | `DynamicRange` controls PQ/HLG OETF/EOTF parameters only; no tone-mapping curve (exposure, filmic shoulder, global-operator) is provided.                                                                                                                        | Author-kernel in the appearance engine.                                                                |
| Spectral rendering CMFs beyond CIE 1931/1964             | Only `Observer.Degree2` (CIE 1931) and `Observer.Degree10` (CIE 1964) are provided. No physiologically-based `Observer` (e.g., Stockman-Sharpe 2°) or custom CMF injection.                                                                                      | Author-kernel if physiological primaries are required for the rendering observer.                      |
