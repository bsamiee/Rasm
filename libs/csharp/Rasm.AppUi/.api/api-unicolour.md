# [RASM_APPUI_API_UNICOLOUR]

`Wacton.Unicolour` supplies a 40-space colour model via the `Unicolour` type, with construction from any `ColourSpace` discriminant, lazy-evaluated conversion accessors for all spaces, perceptual delta-E metrics, gamut mapping, mixing, spectral SPD intake, and configurable working spaces through `Configuration`, `RgbConfiguration`, and `XyzConfiguration`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wacton.Unicolour`
- package: `Wacton.Unicolour`
- assembly: `Wacton.Unicolour`
- namespace: `Wacton.Unicolour`
- asset: netstandard2.0
- rail: colour

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — RGB and hue-based spaces
- rail: colour

`ColourSpace` is the discriminant for all construction and conversion. Display primaries (`DisplayP3`, `Rec2020`, `AdobeRGB`) are `RgbConfiguration` statics, not enum cases.

| [INDEX] | [MEMBER]    | [USED_BY]                     |
| :-----: | :---------- | :---------------------------- |
|   [1]   | `Rgb`       | COLOR_SPACE_AXIS construction |
|   [2]   | `Rgb255`    | COLOR_SPACE_AXIS              |
|   [3]   | `RgbLinear` | #BSDF_MODEL linear light      |
|   [4]   | `Hsb`       |                               |
|   [5]   | `Hsl`       |                               |
|   [6]   | `Hwb`       |                               |
|   [7]   | `Hsi`       |                               |
|   [8]   | `Hsluv`     | perceptual hue-saturation     |
|   [9]   | `Hpluv`     |                               |
|  [10]   | `Okhsv`     |                               |
|  [11]   | `Okhsl`     |                               |
|  [12]   | `Okhwb`     |                               |

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — XYZ, Lab, and appearance model spaces
- rail: colour

| [INDEX] | [MEMBER]  | [USED_BY]                    |
| :-----: | :-------- | :--------------------------- |
|   [1]   | `Xyz`     | #PHOTOMETRIC tristimulus     |
|   [2]   | `Xyy`     | chromaticity construction    |
|   [3]   | `Wxy`     | dominant wavelength          |
|   [4]   | `Lab`     | delta-E76/94/2000, gamut map |
|   [5]   | `Lchab`   | chroma-reduction gamut map   |
|   [6]   | `Luv`     |                              |
|   [7]   | `Lchuv`   |                              |
|   [8]   | `Lms`     | chromatic adaptation         |
|   [9]   | `Ipt`     |                              |
|  [10]   | `Ictcp`   | DeltaE.Itp (HDR perceptual)  |
|  [11]   | `Jzazbz`  |                              |
|  [12]   | `Jzczhz`  | DeltaE.Z                     |
|  [13]   | `Cam02`   | DeltaE.Cam02                 |
|  [14]   | `Cam16`   | DeltaE.Cam16                 |
|  [15]   | `Hct`     |                              |
|  [16]   | `Munsell` |                              |

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — Oklab, Okl-derived, and video spaces
- rail: colour

| [INDEX] | [MEMBER] | [USED_BY]                      |
| :-----: | :------- | :----------------------------- |
|   [1]   | `Oklab`  | DeltaE.Ok, gamut mixing        |
|   [2]   | `Oklch`  | OklchChromaReduction gamut map |
|   [3]   | `Oklrab` |                                |
|   [4]   | `Oklrch` |                                |
|   [5]   | `Xyb`    |                                |
|   [6]   | `Ypbpr`  |                                |
|   [7]   | `Ycbcr`  |                                |
|   [8]   | `Ycgco`  |                                |
|   [9]   | `Yuv`    |                                |
|  [10]   | `Yiq`    |                                |
|  [11]   | `Ydbdr`  |                                |
|  [12]   | `Tsl`    |                                |

[PUBLIC_TYPE_SCOPE]: Unicolour — construction overloads
- rail: colour

| [INDEX] | [SIGNATURE]                                                                                   | [USED_BY]                                         |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------------------------------------------ |
|   [1]   | `new Unicolour(ColourSpace, double, double, double, double alpha = 1.0)`                      | COLOR_SPACE_AXIS                                  |
|   [2]   | `new Unicolour(ColourSpace, (double first, double second, double third), double alpha = 1.0)` |                                                   |
|   [3]   | `new Unicolour(ColourSpace, (double first, double second, double third, double alpha))`       |                                                   |
|   [4]   | `new Unicolour(Configuration, ColourSpace, double, double, double, double alpha = 1.0)`       | COLOR_SPACE_AXIS with Display P3 / Rec2020 config |
|   [5]   | `new Unicolour(string hex)`                                                                   |                                                   |
|   [6]   | `new Unicolour(Configuration, string hex)`                                                    |                                                   |
|   [7]   | `new Unicolour(ColourSpace, double grey, double alpha = 1.0)`                                 |                                                   |
|   [8]   | `new Unicolour(Chromaticity, double luminance = 1.0)`                                         | #PHOTOMETRIC white-point construction             |
|   [9]   | `new Unicolour(Configuration, Chromaticity, double luminance = 1.0)`                          |                                                   |
|  [10]   | `new Unicolour(double cct, Locus locus = Locus.Blackbody, double luminance = 1.0)`            | #PHOTOMETRIC blackbody                            |
|  [11]   | `new Unicolour(Spd)`                                                                          | #PHOTOMETRIC spectral power distribution → XYZ    |
|  [12]   | `new Unicolour(Configuration, Spd)`                                                           |                                                   |
|  [13]   | `new Unicolour(Pigment[], double[])`                                                          | Kubelka-Munk pigment mixing                       |

[PUBLIC_TYPE_SCOPE]: Unicolour — conversion accessors (lazy-evaluated)
- rail: colour

| [INDEX] | [MEMBER]             | [TYPE]      | [USED_BY]                              |
| :-----: | :------------------- | :---------- | :------------------------------------- |
|   [1]   | `.Rgb`               | `Rgb`       | sRGB byte output                       |
|   [2]   | `.RgbLinear`         | `RgbLinear` | #BSDF_MODEL linear-light pipeline      |
|   [3]   | `.Xyz`               | `Xyz`       | #PHOTOMETRIC tristimulus               |
|   [4]   | `.Xyy`               | `Xyy`       | chromaticity, dominant wavelength      |
|   [5]   | `.Lab`               | `Lab`       | delta-E, perceptual sort               |
|   [6]   | `.Lchab`             | `Lchab`     | chroma-reduction gamut mapping         |
|   [7]   | `.Oklab`             | `Oklab`     | DeltaE.Ok, perceptual interpolation    |
|   [8]   | `.Oklch`             | `Oklch`     | OklchChromaReduction gamut mapping     |
|   [9]   | `.Ictcp`             | `Ictcp`     | DeltaE.Itp (HDR), Rec.2100             |
|  [10]   | `.Jzczhz`            | `Jzczhz`    | DeltaE.Z                               |
|  [11]   | `.Cam02`             | `Cam02`     | DeltaE.Cam02                           |
|  [12]   | `.Cam16`             | `Cam16`     | DeltaE.Cam16                           |
|  [13]   | `.Lms`               | `Lms`       | chromatic adaptation                   |
|  [14]   | `.Hsluv`             | `Hsluv`     | perceptual hue-uniform                 |
|  [15]   | `.Hct`               | `Hct`       | Material You / dynamic color           |
|  [16]   | `.Wxy`               | `Wxy`       | dominant wavelength, excitation purity |
|  [17]   | `.RelativeLuminance` | `double`    | WCAG contrast                          |
|  [18]   | `.IsInRgbGamut`      | `bool`      | gamut-check gate                       |

[PUBLIC_TYPE_SCOPE]: Unicolour — scalar and metadata accessors
- rail: colour

| [INDEX] | [MEMBER]              | [TYPE]          | [USED_BY]                |
| :-----: | :-------------------- | :-------------- | :----------------------- |
|   [1]   | `.Chromaticity`       | `Chromaticity`  | white-point comparison   |
|   [2]   | `.DominantWavelength` | `double`        | spectral locus tagging   |
|   [3]   | `.ExcitationPurity`   | `double`        | spectral locus tagging   |
|   [4]   | `.Temperature`        | `Temperature`   | blackbody CCT readout    |
|   [5]   | `.IsInPointerGamut`   | `bool`          | real-surface gamut check |
|   [6]   | `.IsInMacAdamLimits`  | `bool`          | spectral limits check    |
|   [7]   | `.IsImaginary`        | `bool`          | outside spectral locus   |
|   [8]   | `.Hex`                | `string`        |                          |
|   [9]   | `.Alpha`              | `Alpha`         |                          |
|  [10]   | `.Configuration`      | `Configuration` | config round-trip        |

[PUBLIC_TYPE_SCOPE]: DeltaE enum — all 12 members
- rail: colour

Called via `unicolour.Difference(reference, DeltaE.X)`.

| [INDEX] | [MEMBER]            | [USED_BY]                         |
| :-----: | :------------------ | :-------------------------------- |
|   [1]   | `Cie76`             | baseline perceptual diff          |
|   [2]   | `Cie94`             | graphic-arts weighting            |
|   [3]   | `Cie94Textiles`     |                                   |
|   [4]   | `Ciede2000`         | industry-standard appearance diff |
|   [5]   | `CmcAcceptability`  |                                   |
|   [6]   | `CmcPerceptibility` |                                   |
|   [7]   | `Itp`               | HDR perceptual diff               |
|   [8]   | `Z`                 | HDR perceptual diff (Safdar 2021) |
|   [9]   | `Hyab`              |                                   |
|  [10]   | `Ok`                | perceptual mixing                 |
|  [11]   | `Cam02`             | appearance model                  |
|  [12]   | `Cam16`             | appearance model                  |

[PUBLIC_TYPE_SCOPE]: Configuration, RgbConfiguration, XyzConfiguration, DynamicRange
- rail: colour

| [INDEX] | [SYMBOL]                       | [SIGNATURE]                                                                                                                               | [USED_BY]                   |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------- |
|   [1]   | `Configuration.Default`        | `static Configuration` — sRGB/D65/Rec601/StandardRgb CAM                                                                                  | baseline                    |
|   [2]   | `Configuration..ctor`          | `(RgbConfiguration?, XyzConfiguration?, YbrConfiguration?, CamConfiguration?, DynamicRange?, IccConfiguration?)`                          | custom working space        |
|   [3]   | `RgbConfiguration.StandardRgb` | `static RgbConfiguration`                                                                                                                 | sRGB                        |
|   [4]   | `RgbConfiguration.DisplayP3`   | `static RgbConfiguration`                                                                                                                 | COLOR_SPACE_AXIS.display-p3 |
|   [5]   | `RgbConfiguration.Rec2020`     | `static RgbConfiguration`                                                                                                                 | COLOR_SPACE_AXIS.rec2020    |
|   [6]   | `RgbConfiguration.Rec2100Pq`   | `static RgbConfiguration`                                                                                                                 | #PHOTOMETRIC PQ transfer    |
|   [7]   | `RgbConfiguration.A98`         | `static RgbConfiguration`                                                                                                                 | Adobe RGB                   |
|   [8]   | `RgbConfiguration.ProPhoto`    | `static RgbConfiguration`                                                                                                                 | ProPhoto / ROMM RGB         |
|   [9]   | `RgbConfiguration.Aces20651`   | `static RgbConfiguration`                                                                                                                 | ACES 2065-1                 |
|  [10]   | `RgbConfiguration.Acescg`      | `static RgbConfiguration`                                                                                                                 | ACEScg scene-linear AP1     |
|  [11]   | `RgbConfiguration..ctor`       | `(Chromaticity r, Chromaticity g, Chromaticity b, WhitePoint, Func<double,double> fromLinear, Func<double,double> toLinear, string name)` | custom primaries            |
|  [12]   | `XyzConfiguration.D65`         | `static XyzConfiguration` — Illuminant.D65, Observer.Degree2, Bradford                                                                    | default                     |
|  [13]   | `XyzConfiguration.D50`         | `static XyzConfiguration` — Illuminant.D50, Observer.Degree2, Bradford                                                                    | ICC profile                 |
|  [14]   | `DynamicRange.Standard`        | `static DynamicRange` — white 100 cd/m², max 100, min 0.1                                                                                 | SDR                         |
|  [15]   | `DynamicRange.High`            | `static DynamicRange` — white 203 cd/m², max 1000, min 0                                                                                  | HDR default                 |
|  [16]   | `DynamicRange..ctor`           | `(double whiteLuminance, double maxLuminance, double minLuminance, double hlgWhiteLevel = 0.75, string name)`                             | custom HDR range            |

[PUBLIC_TYPE_SCOPE]: Spd, GamutMap, Chromaticity, WhitePoint
- rail: colour

| [INDEX] | [SYMBOL]                        | [SIGNATURE]                                                                                | [USED_BY]                        |
| :-----: | :------------------------------ | :----------------------------------------------------------------------------------------- | :------------------------------- |
|   [1]   | `Spd..ctor`                     | `(int start, int interval, params double[] coefficients)` — interval must be 0, 1, or 5 nm | #PHOTOMETRIC custom SPD          |
|   [2]   | `Spd.D65`                       | `static Spd` — 300–830 nm, 1 nm interval                                                   | D65 reference illuminant         |
|   [3]   | `GamutMap.RgbClipping`          | clip each channel to [0, 1]                                                                | fast                             |
|   [4]   | `GamutMap.OklchChromaReduction` | reduce Oklch chroma until in gamut                                                         | perceptual quality               |
|   [5]   | `GamutMap.WxyPurityReduction`   | reduce excitation purity toward white                                                      | spectral                         |
|   [6]   | `Unicolour.Mix`                 | `(other, ColourSpace, amount, HueSpan, premultiplyAlpha)` → `Unicolour`                    | interpolation in any space       |
|   [7]   | `Unicolour.Palette`             | `(other, ColourSpace, count, HueSpan, premultiplyAlpha)` → `IEnumerable<Unicolour>`        |                                  |
|   [8]   | `Chromaticity`                  | `record(double X, double Y)` with `.U`, `.V`, `.Uv`, `.Xy`                                 | white-point, dominant-wavelength |
|   [9]   | `Chromaticity.FromUv(u, v)`     | `static Chromaticity`                                                                      | UCS conversion                   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Unicolour — primary operations
- rail: colour

| [INDEX] | [SURFACE]                                             | [CALL_SHAPE]                             | [RAIL]                            |
| :-----: | :---------------------------------------------------- | :--------------------------------------- | :-------------------------------- |
|   [1]   | `new Unicolour(ColourSpace, double, double, double)`  | ctor                                     | colour construction               |
|   [2]   | `new Unicolour(Configuration, ColourSpace, ...)`      | ctor                                     | custom working-space construction |
|   [3]   | `new Unicolour(string hex)`                           | ctor                                     | hex intake                        |
|   [4]   | `new Unicolour(Spd)`                                  | ctor                                     | spectral intake                   |
|   [5]   | `new Unicolour(Pigment[], double[])`                  | ctor                                     | Kubelka-Munk mix                  |
|   [6]   | `Unicolour.Difference(Unicolour, DeltaE)`             | instance call → `double`                 | perceptual delta                  |
|   [7]   | `Unicolour.Contrast(Unicolour)`                       | instance call → `double`                 | WCAG contrast                     |
|   [8]   | `Unicolour.Mix(Unicolour, ColourSpace, double, ...)`  | instance call → `Unicolour`              | interpolation                     |
|   [9]   | `Unicolour.Palette(Unicolour, ColourSpace, int, ...)` | instance call → `IEnumerable<Unicolour>` | palette generation                |

## [4]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `Wacton.Unicolour`
- primary type: `Unicolour` — immutable, lazy-evaluated per colour-space accessor
- construction discriminant: `ColourSpace` enum (40 members)
- working-space policy: `Configuration` (holds `RgbConfiguration`, `XyzConfiguration`, `DynamicRange`, `CamConfiguration`)
- display primaries (DisplayP3, Rec2020, AdobeRGB) are `RgbConfiguration` statics, not `ColourSpace` cases

[GAMUT_LAW]:
- gamut mapping is internal only; public access is `IsInRgbGamut` check + `Mix`/`Palette`
- no public `MapToGamut` instance method; `GamutMap` enum feeds the internal `GamutMapping` kernel only

[SPD_LAW]:
- `new Unicolour(config, Spd)` → internal `SpdToXyzTuple` → `Xyz.FromSpd`
- reflectance mixing uses `Pigment[]`/`KubelkaMunk` with `Spd` illuminant, not the `Spd` ctor

[RAIL_LAW]:
- Package: `Wacton.Unicolour`
- Owns: 40-space colour model, construction, conversion, delta-E, gamut mapping, spectral intake, mixing
- Accept: colour construction from any `ColourSpace`, working-space configuration, perceptual metrics
- Reject: ICC full PCS pipeline, ACES RRT/ODT tone mapping, scene-referred HDR tone mapping

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
