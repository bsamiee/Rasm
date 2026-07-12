# [RASM_API_UNICOLOUR]

`Wacton.Unicolour` (MIT, pure-managed `netstandard2.0`, zero dependencies, ALC-safe) owns an immutable 40-space colour model through `Unicolour`: any `ColourSpace` constructs it, lazy accessors project every space, `DeltaE` measures perceptual distance, gamut maps bound output, `HueSpan` governs mixing and palettes, `Locus` admits spectral SPD and blackbody/CCT inputs, and `Configuration` composes `RgbConfiguration`, `XyzConfiguration`, `YbrConfiguration`, `CamConfiguration`, `DynamicRange`, and `IccConfiguration` working-space policy. `Wacton.Unicolour.Datasets` (`netstandard2.0`) adds named colour and ColorChecker reference sets plus perceptual colourmaps over the same model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wacton.Unicolour`

- package / license: `Wacton.Unicolour` / MIT
- assembly: `Wacton.Unicolour`
- namespace: `Wacton.Unicolour`
- asset: `netstandard2.0` (sole lib; bound under both the net10 AppUi and Materials consumers; zero deps, ALC-safe)
- rail: colour

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — RGB and hue-based spaces

- rail: colour

`ColourSpace` is the discriminant for all construction and conversion. Display primaries (`DisplayP3`, `Rec2020`, `AdobeRGB`) are `RgbConfiguration` statics, not enum cases.

| [INDEX] | [MEMBER]    | [USED_BY]                     |
| :-----: | :---------- | :---------------------------- |
|  [01]   | `Rgb`       | COLOR_SPACE_AXIS construction |
|  [02]   | `Rgb255`    | COLOR_SPACE_AXIS              |
|  [03]   | `RgbLinear` | #BSDF_MODEL linear light      |
|  [04]   | `Hsb`       | —                             |
|  [05]   | `Hsl`       | —                             |
|  [06]   | `Hwb`       | —                             |
|  [07]   | `Hsi`       | —                             |
|  [08]   | `Hsluv`     | perceptual hue-saturation     |
|  [09]   | `Hpluv`     | —                             |
|  [10]   | `Okhsv`     | —                             |
|  [11]   | `Okhsl`     | —                             |
|  [12]   | `Okhwb`     | —                             |

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — XYZ, Lab, and appearance model spaces

- rail: colour

`Lab` drives delta-E76/94/2000 and gamut mapping.

| [INDEX] | [MEMBER]  | [USED_BY]                   |
| :-----: | :-------- | :-------------------------- |
|  [01]   | `Xyz`     | #PHOTOMETRIC tristimulus    |
|  [02]   | `Xyy`     | chromaticity construction   |
|  [03]   | `Wxy`     | dominant wavelength         |
|  [04]   | `Lab`     | perceptual comparison       |
|  [05]   | `Lchab`   | chroma-reduction gamut map  |
|  [06]   | `Luv`     | —                           |
|  [07]   | `Lchuv`   | —                           |
|  [08]   | `Lms`     | chromatic adaptation        |
|  [09]   | `Ipt`     | —                           |
|  [10]   | `Ictcp`   | DeltaE.Itp (HDR perceptual) |
|  [11]   | `Jzazbz`  | —                           |
|  [12]   | `Jzczhz`  | DeltaE.Z                    |
|  [13]   | `Cam02`   | DeltaE.Cam02                |
|  [14]   | `Cam16`   | DeltaE.Cam16                |
|  [15]   | `Hct`     | —                           |
|  [16]   | `Munsell` | —                           |

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — Oklab, Okl-derived, and video spaces

- rail: colour

`Oklab` drives `DeltaE.Ok` and gamut interpolation.

| [INDEX] | [MEMBER] | [USED_BY]                      |
| :-----: | :------- | :----------------------------- |
|  [01]   | `Oklab`  | perceptual workflow            |
|  [02]   | `Oklch`  | OklchChromaReduction gamut map |
|  [03]   | `Oklrab` | —                              |
|  [04]   | `Oklrch` | —                              |
|  [05]   | `Xyb`    | —                              |
|  [06]   | `Ypbpr`  | —                              |
|  [07]   | `Ycbcr`  | —                              |
|  [08]   | `Ycgco`  | —                              |
|  [09]   | `Yuv`    | —                              |
|  [10]   | `Yiq`    | —                              |
|  [11]   | `Ydbdr`  | —                              |
|  [12]   | `Tsl`    | —                              |

[PUBLIC_TYPE_SCOPE]: Unicolour — construction overloads

- rail: colour

Configured colour-space construction selects Display P3 or Rec2020 through `Configuration`, and `Spd` construction projects spectral power distribution to XYZ.

| [INDEX] | [SIGNATURE]                                                                                   | [USED_BY]                             |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `new Unicolour(ColourSpace, double, double, double, double alpha = 1.0)`                      | COLOR_SPACE_AXIS                      |
|  [02]   | `new Unicolour(ColourSpace, (double first, double second, double third), double alpha = 1.0)` | —                                     |
|  [03]   | `new Unicolour(ColourSpace, (double first, double second, double third, double alpha))`       | —                                     |
|  [04]   | `new Unicolour(Configuration, ColourSpace, double, double, double, double alpha = 1.0)`       | COLOR_SPACE_AXIS                      |
|  [05]   | `new Unicolour(string hex)`                                                                   | —                                     |
|  [06]   | `new Unicolour(Configuration, string hex)`                                                    | —                                     |
|  [07]   | `new Unicolour(ColourSpace, double grey, double alpha = 1.0)`                                 | —                                     |
|  [08]   | `new Unicolour(Chromaticity, double luminance = 1.0)`                                         | #PHOTOMETRIC white-point construction |
|  [09]   | `new Unicolour(Configuration, Chromaticity, double luminance = 1.0)`                          | —                                     |
|  [10]   | `new Unicolour(double cct, Locus locus = Locus.Blackbody, double luminance = 1.0)`            | #PHOTOMETRIC blackbody                |
|  [11]   | `new Unicolour(Spd)`                                                                          | #PHOTOMETRIC spectral intake          |
|  [12]   | `new Unicolour(Configuration, Spd)`                                                           | —                                     |
|  [13]   | `new Unicolour(Pigment[], double[])`                                                          | Kubelka-Munk pigment mixing           |

[PUBLIC_TYPE_SCOPE]: Unicolour — conversion accessors (lazy-evaluated)

- rail: colour

`.Xyy` carries chromaticity and dominant-wavelength projection. `.Lab` owns delta-E input and perceptual sorting. `.Oklab` owns `DeltaE.Ok` input and perceptual interpolation. `.Ictcp` joins `DeltaE.Itp` to Rec.2100. `.Wxy` carries dominant-wavelength and excitation-purity projection. `Hct` serves Material callers and dynamic colour.

| [INDEX] | [MEMBER]                         | [TYPE]                 | [USED_BY]                     |
| :-----: | :------------------------------- | :--------------------- | :---------------------------- |
|  [01]   | `.Rgb`                           | `Rgb`                  | configured RGB projection     |
|  [02]   | `.RgbLinear`                     | `RgbLinear`            | #BSDF_MODEL linear-light rail |
|  [03]   | `.Xyz`                           | `Xyz`                  | #PHOTOMETRIC tristimulus      |
|  [04]   | `.Xyy`                           | `Xyy`                  | chromaticity projection       |
|  [05]   | `.Lab`                           | `Lab`                  | perceptual comparison         |
|  [06]   | `.Lchab`                         | `Lchab`                | chroma-reduction gamut map    |
|  [07]   | `.Oklab`                         | `Oklab`                | perceptual workflow           |
|  [08]   | `.Oklch`                         | `Oklch`                | Oklch chroma reduction        |
|  [09]   | `.Ictcp`                         | `Ictcp`                | HDR perceptual workflow       |
|  [10]   | `.Jzczhz`                        | `Jzczhz`               | `DeltaE.Z`                    |
|  [11]   | `.Cam02`                         | `Cam02`                | `DeltaE.Cam02`                |
|  [12]   | `.Cam16`                         | `Cam16`                | `DeltaE.Cam16`                |
|  [13]   | `.Lms`                           | `Lms`                  | chromatic adaptation          |
|  [14]   | `GetRepresentation(ColourSpace)` | `ColourRepresentation` | selected-space projection     |
|  [15]   | `.Hsluv`                         | `Hsluv`                | perceptual hue uniformity     |
|  [16]   | `.Hct`                           | `Hct`                  | dynamic colour                |
|  [17]   | `.Wxy`                           | `Wxy`                  | spectral-locus projection     |
|  [18]   | `.RelativeLuminance`             | `double`               | WCAG contrast                 |
|  [19]   | `.IsInRgbGamut`                  | `bool`                 | gamut-check gate              |

[PUBLIC_TYPE_SCOPE]: Unicolour — scalar and metadata accessors

- rail: colour

| [INDEX] | [MEMBER]              | [TYPE]          | [USED_BY]                |
| :-----: | :-------------------- | :-------------- | :----------------------- |
|  [01]   | `.Chromaticity`       | `Chromaticity`  | white-point comparison   |
|  [02]   | `.DominantWavelength` | `double`        | spectral locus tagging   |
|  [03]   | `.ExcitationPurity`   | `double`        | spectral locus tagging   |
|  [04]   | `.Temperature`        | `Temperature`   | blackbody CCT readout    |
|  [05]   | `.IsInPointerGamut`   | `bool`          | real-surface gamut check |
|  [06]   | `.IsInMacAdamLimits`  | `bool`          | spectral limits check    |
|  [07]   | `.IsImaginary`        | `bool`          | outside spectral locus   |
|  [08]   | `.Hex`                | `string`        | —                        |
|  [09]   | `.Alpha`              | `Alpha`         | —                        |
|  [10]   | `.Configuration`      | `Configuration` | config round-trip        |

[PUBLIC_TYPE_SCOPE]: DeltaE enum — metric vocabulary

- rail: colour

Called via `unicolour.Difference(reference, DeltaE.X)`.

| [INDEX] | [MEMBER]            | [USED_BY]                         |
| :-----: | :------------------ | :-------------------------------- |
|  [01]   | `Cie76`             | baseline perceptual diff          |
|  [02]   | `Cie94`             | graphic-arts weighting            |
|  [03]   | `Cie94Textiles`     | —                                 |
|  [04]   | `Ciede2000`         | industry-standard appearance diff |
|  [05]   | `CmcAcceptability`  | —                                 |
|  [06]   | `CmcPerceptibility` | —                                 |
|  [07]   | `Itp`               | HDR perceptual diff               |
|  [08]   | `Z`                 | JzCzhz perceptual diff            |
|  [09]   | `Hyab`              | —                                 |
|  [10]   | `Ok`                | perceptual mixing                 |
|  [11]   | `Cam02`             | appearance model                  |
|  [12]   | `Cam16`             | appearance model                  |

[PUBLIC_TYPE_SCOPE]: Configuration, RgbConfiguration, XyzConfiguration, DynamicRange

- rail: colour

`Configuration` carries RGB, XYZ, YBR, CAM, dynamic-range, and ICC working-space policy. RGB custom construction uses chromaticity primaries, a white point, transfer delegates, and a name; dynamic-range custom construction carries white, maximum, minimum, HLG white-level, and name values.

`Configuration.Default` binds its RGB and XYZ axes to sRGB and D65.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]       | [CAPABILITY]         |
| :-----: | :---------------------- | :----------------- | :------------------- |
|  [01]   | `Configuration.Default` | static preset      | baseline policy      |
|  [02]   | `Configuration..ctor`   | configuration ctor | custom working space |

`RgbConfiguration.ProPhoto` names the ROMM RGB preset.

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :----------------------------- | :--------------- | :------------------------ |
|  [01]   | `RgbConfiguration.StandardRgb` | static preset    | sRGB                      |
|  [02]   | `RgbConfiguration.DisplayP3`   | static preset    | COLOR_SPACE_AXIS display  |
|  [03]   | `RgbConfiguration.Rec2020`     | static preset    | COLOR_SPACE_AXIS wide RGB |
|  [04]   | `RgbConfiguration.Rec2100Pq`   | static preset    | #PHOTOMETRIC PQ transfer  |
|  [05]   | `RgbConfiguration.A98`         | static preset    | Adobe RGB                 |
|  [06]   | `RgbConfiguration.ProPhoto`    | static preset    | ROMM RGB                  |
|  [07]   | `RgbConfiguration.Aces20651`   | static preset    | ACES 2065-1               |
|  [08]   | `RgbConfiguration.Acescg`      | static preset    | ACEScg scene-linear AP1   |
|  [09]   | `RgbConfiguration..ctor`       | custom primaries | RGB working space         |

`XyzConfiguration` binds the working white.

| [INDEX] | [SURFACE]              | [CALL_SHAPE] | [CAPABILITY]      |
| :-----: | :--------------------- | :----------- | :---------------- |
|  [01]   | `XyzConfiguration.D65` | D65 preset   | default white     |
|  [02]   | `XyzConfiguration.D50` | D50 preset   | ICC profile white |

`DynamicRange` binds the luminance span.

| [INDEX] | [SURFACE]               | [CALL_SHAPE] | [CAPABILITY]    |
| :-----: | :---------------------- | :----------- | :-------------- |
|  [01]   | `DynamicRange.Standard` | SDR preset   | standard range  |
|  [02]   | `DynamicRange.High`     | HDR preset   | HDR default     |
|  [03]   | `DynamicRange..ctor`    | range ctor   | custom HDR span |

[PUBLIC_TYPE_SCOPE]: Spd, GamutMap, Chromaticity, WhitePoint

- rail: colour

`Spd` construction accepts measured coefficients at 0, 1, or 5 nm intervals.

| [INDEX] | [SURFACE]   | [CALL_SHAPE]         | [CAPABILITY]             |
| :-----: | :---------- | :------------------- | :----------------------- |
|  [01]   | `Spd..ctor` | coefficient ctor     | #PHOTOMETRIC custom SPD  |
|  [02]   | `Spd.D65`   | static SPD reference | D65 reference illuminant |

Each `GamutMap` value binds one RGB-gamut policy.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :--------------- | :----------------------- |
|  [01]   | `GamutMap.RgbClipping`          | channel clipping | fast gamut clamp         |
|  [02]   | `GamutMap.OklchChromaReduction` | chroma reduction | perceptual gamut mapping |
|  [03]   | `GamutMap.WxyPurityReduction`   | purity reduction | spectral gamut mapping   |

`Unicolour` interpolation returns one colour or a generated sequence.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :------------------ | :---------------- | :------------------------ |
|  [01]   | `Unicolour.Mix`     | interpolation     | single mixed colour       |
|  [02]   | `Unicolour.Palette` | interpolation set | generated colour sequence |

[PUBLIC_TYPE_SCOPE]: blend, simulation, and ICC carriers

- rail: colour

Each public carrier owns one colour operation or configuration policy.

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                   |
| :-----: | :----------------------------- | :--------------------------------------------- |
|  [01]   | `BlendMode`                    | `Unicolour.Blend` discriminant                 |
|  [02]   | `Cvd`                          | `Simulate` discriminant                        |
|  [03]   | `Icc`                          | ICC profile carrier                            |
|  [04]   | `IccConfiguration`             | ICC-backed configuration                       |
|  [05]   | `YbrConfiguration.Jpeg`        | JPEG YBR policy                                |
|  [06]   | `YbrConfiguration.Rec601`      | Rec.601 YBR policy                             |
|  [07]   | `YbrConfiguration.Rec709`      | Rec.709 YBR policy                             |
|  [08]   | `YbrConfiguration.Rec2020`     | Rec.2020 YBR policy                            |
|  [09]   | `CamConfiguration.StandardRgb` | standard-RGB CAM policy                        |
|  [10]   | `CamConfiguration.Hct`         | HCT CAM policy                                 |
|  [11]   | `Configuration`                | `ConvertToConfiguration(Configuration)` target |

[ICC_MEMBERS]:

- Owner: `Icc`
- Members:
    - `Channels`
    - `Profile`
    - `Header`
    - `Tags`

`Chromaticity` surfaces own white-point and wavelength geometry plus UCS conversion.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]       | [CAPABILITY]          |
| :-----: | :-------------------------- | :----------------- | :-------------------- |
|  [01]   | `Chromaticity`              | chromaticity value | chromaticity geometry |
|  [02]   | `Chromaticity.FromUv(u, v)` | static conversion  | UCS conversion        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Unicolour — primary operations

- rail: colour

`Unicolour` constructors own each registered construction route.

| [INDEX] | [SURFACE]                                            | [RAIL]                            |
| :-----: | :--------------------------------------------------- | :-------------------------------- |
|  [01]   | `new Unicolour(ColourSpace, double, double, double)` | colour construction               |
|  [02]   | `new Unicolour(Configuration, ColourSpace,...)`      | custom working-space construction |
|  [03]   | `new Unicolour(string hex)`                          | hex intake                        |
|  [04]   | `new Unicolour(Spd)`                                 | spectral intake                   |
|  [05]   | `new Unicolour(Pigment[], double[])`                 | Kubelka-Munk mix                  |

Each operation binds one colour rail, and its keyed record carries the exact signature and return type.

| [INDEX] | [SURFACE]    | [RAIL]                       |
| :-----: | :----------- | :--------------------------- |
|  [01]   | `Difference` | perceptual delta             |
|  [02]   | `Contrast`   | WCAG contrast                |
|  [03]   | `Mix`        | hue-aware interpolation      |
|  [04]   | `Palette`    | hue-aware palette generation |

[DIFFERENCE]:

- Signature: `double Difference(Unicolour reference, DeltaE deltaE)`

[CONTRAST]:

- Signature: `double Contrast(Unicolour other)`

[MIX]:

- Signature: `Unicolour Mix(Unicolour other, ColourSpace colourSpace, double amount = 0.5, HueSpan hueSpan = HueSpan.Shorter, bool premultiplyAlpha = true)`

[PALETTE]:

- Signature: `IEnumerable<Unicolour> Palette(Unicolour other, ColourSpace colourSpace, int count, HueSpan hueSpan = HueSpan.Shorter, bool premultiplyAlpha = true)`

`HueSpan` enum (mix/palette hue-traversal axis): `Shorter`, `Longer`, `Increasing`, `Decreasing`. `Locus` enum (CCT-construction radiator): `Blackbody`, `Daylight`.

## [04]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:

- namespace: `Wacton.Unicolour`
- primary type: `Unicolour` — immutable, lazy-evaluated per colour-space accessor
- construction discriminant: `ColourSpace` enum
- working-space policy: `Configuration` (holds `Rgb`, `Xyz`, `Ybr`, `Cam`, `DynamicRange`, `Icc` slots)
- display primaries (DisplayP3, Rec2020, AdobeRGB) are `RgbConfiguration` statics, not `ColourSpace` cases

[INTEGRATION]:

- AppUi boundary: `Unicolour` is the canonical colour value; map to/from Avalonia `Color`/`HsvColor`
  (`api-avalonia-color.md`) only at the view edge — convert outbound values to `Configuration.Default`, read `.Rgb.Byte255`, and construct
  `new Unicolour(ColourSpace.Rgb255, r, g, b)` inbound, keeping all perceptual maths in `Unicolour`.
- Accessibility: `Contrast(other)` is the WCAG ratio and `RelativeLuminance` the WCAG luminance;
  drive theme contrast gates off these rather than re-deriving luminance from Avalonia brushes.
- Palette pipelines: `Mix`/`Palette` over `ColourSpace.Oklab`/`Oklch` with `HueSpan` produce
  perceptually-even theme ramps; project each result through `ConvertToConfiguration(Configuration.Default).Rgb.Byte255`
  before binding the swatch `ItemsSource`.
- Materials/BSDF: the Materials consumer composes the spectral/appearance surface
  (`Spd`, `Pigment[]`/`KubelkaMunk`, `DeltaE.Ciede2000`, `Configuration` working spaces) for
  #PHOTOMETRIC and #BSDF_MODEL pages; AppUi reuses the same model for UI colour without a
  second colour library.

[GAMUT_LAW]:

- public gamut mapping is `Unicolour.MapToRgbGamut(GamutMap gamutMap = GamutMap.OklchChromaReduction)`, `Unicolour.MapToPointerGamut()`, and `Unicolour.MapToMacAdamLimits()`
- `GamutMap` is accepted only by `MapToRgbGamut`; Pointer and MacAdam mapping use zero-argument surfaces

[SPD_LAW]:

- `new Unicolour(config, Spd)` uses the internal SPD-to-XYZ path
- reflectance mixing uses `Pigment[]`/`KubelkaMunk` with `Spd` illuminant, not the `Spd` ctor

[RAIL_LAW]:

- Package: `Wacton.Unicolour`
- Owns: 40-space colour model, construction, conversion, delta-E, gamut mapping, spectral intake, mixing
- Accept: colour construction from any `ColourSpace`, working-space configuration, perceptual metrics
- Reject: ICC full PCS pipeline, ACES RRT/ODT tone mapping, scene-referred HDR tone mapping

---

## [05]-[CONSTRUCTION]

`Wacton.Unicolour.Unicolour` owns colour construction. Alpha defaults to `1.0` where the overload exposes it, and configuration variants select the working RGB/XYZ/dynamic-range policy before conversion.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]                 | [CAPABILITY]                             |
| :-----: | :--------------------------------------------- | :--------------------------- | :--------------------------------------- |
|  [01]   | `Unicolour(ColourSpace, …)`                    | space triple ctor            | COLOR_SPACE_AXIS construction            |
|  [02]   | `Unicolour(ColourSpace, …)`                    | space tuple ctor             | tuple-based colour construction          |
|  [03]   | `Unicolour(Configuration, ColourSpace, …)`     | configured space ctor        | custom working-space construction        |
|  [04]   | `ConvertToConfiguration(Configuration config)` | instance conversion          | remaps a colour to another configuration |
|  [05]   | `Unicolour(string hex)`                        | hex ctor                     | hex intake                               |
|  [06]   | `Unicolour(Configuration, string hex)`         | configured hex ctor          | configured hex intake                    |
|  [07]   | `Unicolour(ColourSpace, double grey, …)`       | grey ctor                    | single-channel construction              |
|  [08]   | `Unicolour(Chromaticity, …)`                   | chromaticity ctor            | #PHOTOMETRIC white-point construction    |
|  [09]   | `Unicolour(Configuration, Chromaticity, …)`    | configured chromaticity ctor | custom white-point construction          |
|  [10]   | `Unicolour(double cct, Locus, …)`              | CCT ctor                     | #PHOTOMETRIC blackbody                   |
|  [11]   | `Unicolour(Spd)`                               | SPD ctor                     | spectral power distribution to XYZ       |
|  [12]   | `Unicolour(Configuration, Spd)`                | configured SPD ctor          | custom spectral intake                   |
|  [13]   | `Unicolour(Pigment[], double[])`               | pigment mix ctor             | Kubelka-Munk pigment mixing              |

---

## [06]-[CONVERSION_ACCESSORS]

`Unicolour` lazy-evaluates every registered accessor. `.Xyy` carries chromaticity and dominant-wavelength projection. `.Lab` owns delta-E input and perceptual sorting. `.Oklab` owns `DeltaE.Ok` input and perceptual interpolation. `.Ictcp` joins `DeltaE.Itp` to Rec.2100. `.Wxy` carries dominant-wavelength and excitation-purity projection. `Chromaticity` is `record(double X, double Y)`. `.DominantWavelength` projects `.Wxy.W`, and `.ExcitationPurity` projects `.Wxy.X`. `Hct` serves Material callers and dynamic colour.

| [INDEX] | [MEMBER]              | [TYPE]          | [USED_BY]                     |
| :-----: | :-------------------- | :-------------- | :---------------------------- |
|  [01]   | `.Rgb`                | `Rgb`           | configured RGB projection     |
|  [02]   | `.RgbLinear`          | `RgbLinear`     | #BSDF_MODEL linear-light rail |
|  [03]   | `.Xyz`                | `Xyz`           | #PHOTOMETRIC tristimulus      |
|  [04]   | `.Xyy`                | `Xyy`           | chromaticity projection       |
|  [05]   | `.Lab`                | `Lab`           | perceptual comparison         |
|  [06]   | `.Lchab`              | `Lchab`         | chroma-reduction gamut map    |
|  [07]   | `.Oklab`              | `Oklab`         | perceptual workflow           |
|  [08]   | `.Oklch`              | `Oklch`         | Oklch chroma reduction        |
|  [09]   | `.Ictcp`              | `Ictcp`         | HDR perceptual workflow       |
|  [10]   | `.Jzczhz`             | `Jzczhz`        | `DeltaE.Z`                    |
|  [11]   | `.Cam02`              | `Cam02`         | `DeltaE.Cam02`                |
|  [12]   | `.Cam16`              | `Cam16`         | `DeltaE.Cam16`                |
|  [13]   | `.Lms`                | `Lms`           | chromatic adaptation          |
|  [14]   | `.Hsluv`              | `Hsluv`         | perceptual hue uniformity     |
|  [15]   | `.Hpluv`              | `Hpluv`         | —                             |
|  [16]   | `.Hct`                | `Hct`           | dynamic colour                |
|  [17]   | `.Wxy`                | `Wxy`           | spectral-locus projection     |
|  [18]   | `.RelativeLuminance`  | `double`        | WCAG contrast                 |
|  [19]   | `.Chromaticity`       | `Chromaticity`  | white-point comparison        |
|  [20]   | `.DominantWavelength` | `double`        | spectral-locus tagging        |
|  [21]   | `.ExcitationPurity`   | `double`        | spectral-locus tagging        |
|  [22]   | `.Temperature`        | `Temperature`   | blackbody CCT readout         |
|  [23]   | `.IsInRgbGamut`       | `bool`          | gamut-check gate              |
|  [24]   | `.IsInPointerGamut`   | `bool`          | real-surface gamut check      |
|  [25]   | `.IsInMacAdamLimits`  | `bool`          | spectral limits check         |
|  [26]   | `.IsImaginary`        | `bool`          | outside spectral locus        |
|  [27]   | `.Hex`                | `string`        | —                             |
|  [28]   | `.Alpha`              | `Alpha`         | —                             |
|  [29]   | `.Configuration`      | `Configuration` | config round-trip             |

---

## [07]-[DELTA_E]

`DeltaE` rows select the metric passed to `unicolour.Difference(reference, DeltaE.X)`. `Ciede2000` governs industry and #BSDF_MODEL appearance comparison. `Itp` is `DeltaEItp × 720` on ICtCp. `Z` operates on JzCzhz. `Ok` is Euclidean on Oklab. `Cam16` carries the `× d^` scale.

| [INDEX] | [MEMBER]            | [SIGNATURE]                | [USED_BY]                |
| :-----: | :------------------ | :------------------------- | :----------------------- |
|  [01]   | `Cie76`             | `DeltaE.Cie76`             | baseline perceptual diff |
|  [02]   | `Cie94`             | `DeltaE.Cie94`             | graphic-arts weighting   |
|  [03]   | `Cie94Textiles`     | `DeltaE.Cie94Textiles`     | —                        |
|  [04]   | `Ciede2000`         | `DeltaE.Ciede2000`         | appearance comparison    |
|  [05]   | `CmcAcceptability`  | `DeltaE.CmcAcceptability`  | —                        |
|  [06]   | `CmcPerceptibility` | `DeltaE.CmcPerceptibility` | —                        |
|  [07]   | `Itp`               | `DeltaE.Itp`               | HDR perceptual diff      |
|  [08]   | `Z`                 | `DeltaE.Z`                 | JzCzhz perceptual diff   |
|  [09]   | `Hyab`              | `DeltaE.Hyab`              | —                        |
|  [10]   | `Ok`                | `DeltaE.Ok`                | perceptual mixing        |
|  [11]   | `Cam02`             | `DeltaE.Cam02`             | appearance model         |
|  [12]   | `Cam16`             | `DeltaE.Cam16`             | appearance model         |

Entry point: `double Unicolour.Difference(Unicolour reference, DeltaE deltaE)` and `double Unicolour.Contrast(Unicolour other)` (WCAG).

---

## [08]-[CONFIGURATION]

`Configuration` carries RGB, XYZ, YBR, CAM, dynamic-range, and ICC working-space policy. Its optional constructor slots are `Configuration(RgbConfiguration? rgbConfig = null, XyzConfiguration? xyzConfig = null, YbrConfiguration? ybrConfig = null, CamConfiguration? camConfig = null, DynamicRange? dynamicRange = null, IccConfiguration? iccConfig = null)`; a custom working space overrides only selected axes and inherits `Default` elsewhere. `Configuration.Default` binds its RGB and XYZ axes to sRGB and D65. `Configuration.Ybr` selects YCbCr or YPbPr matrix policy. `Configuration.Cam` carries CAM02 and CAM16 viewing conditions.

| [INDEX] | [SURFACE]                                                          | [TYPE_FAMILY]      | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------- | :----------------- | :--------------------------------------- |
|  [01]   | `Configuration.Default`                                            | static preset      | baseline working-space policy            |
|  [02]   | `Configuration..ctor(rgb?, xyz?, ybr?, cam?, dynamicRange?, icc?)` | configuration ctor | custom working space (per-slot override) |
|  [03]   | `Configuration.Rgb`                                                | property           | RGB working space                        |
|  [04]   | `Configuration.Xyz`                                                | property           | XYZ working space                        |
|  [05]   | `Configuration.Ybr`                                                | property           | YBR matrix policy                        |
|  [06]   | `Configuration.Cam`                                                | property           | CAM viewing-condition policy             |
|  [07]   | `Configuration.DynamicRange`                                       | property           | luminance-range policy                   |
|  [08]   | `Configuration.Icc`                                                | property           | ICC working-profile policy               |

---

## [09]-[RGB_CONFIGURATION]

The `RgbConfiguration` registry binds each static display, scene, log, or broadcast preset under exact member capitalization. `ProPhoto` names the ROMM RGB preset. The custom constructor binds RGB chromaticities, a white point, transfer delegates, and a name.

| [INDEX] | [SURFACE]       | [CAPABILITY]                |
| :-----: | :-------------- | :-------------------------- |
|  [01]   | `StandardRgb`   | COLOR_SPACE_AXIS sRGB       |
|  [02]   | `DisplayP3`     | COLOR_SPACE_AXIS Display P3 |
|  [03]   | `Rec2020`       | COLOR_SPACE_AXIS Rec.2020   |
|  [04]   | `Rec2100Pq`     | #PHOTOMETRIC PQ transfer    |
|  [05]   | `Rec2100Hlg`    | #PHOTOMETRIC HLG transfer   |
|  [06]   | `A98`           | Adobe RGB                   |
|  [07]   | `ProPhoto`      | ROMM RGB                    |
|  [08]   | `Aces20651`     | ACES 2065-1 scene-linear    |
|  [09]   | `Acescg`        | ACEScg scene-linear AP1     |
|  [10]   | `Acescct`       | ACEScct log                 |
|  [11]   | `Acescc`        | ACEScc log                  |
|  [12]   | `Rec709`        | Rec.709 RGB                 |
|  [13]   | `Rec601Line625` | Rec.601 625-line broadcast  |
|  [14]   | `Rec601Line525` | Rec.601 525-line broadcast  |
|  [15]   | `XvYcc`         | xvYCC broadcast             |
|  [16]   | `Pal`           | PAL broadcast               |
|  [17]   | `PalM`          | PAL-M broadcast             |
|  [18]   | `Pal625`        | PAL 625-line broadcast      |
|  [19]   | `Pal525`        | PAL 525-line broadcast      |
|  [20]   | `Ntsc`          | NTSC broadcast              |
|  [21]   | `NtscSmpteC`    | SMPTE-C NTSC broadcast      |
|  [22]   | `Ntsc525`       | NTSC 525-line broadcast     |
|  [23]   | `Secam`         | SECAM broadcast             |
|  [24]   | `Secam625`      | SECAM 625-line broadcast    |

[CUSTOM_RGB]:

- Surface: `RgbConfiguration..ctor`
- Call shape: custom primaries
- Capability: custom RGB working space

---

## [10]-[XYZ_CONFIGURATION]

`XyzConfiguration.D65` and `XyzConfiguration.D50` use the degree-2 observer and Bradford adaptation for the standard working whites.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]         | [CAPABILITY]              |
| :-----: | :---------------------------- | :------------------- | :------------------------ |
|  [01]   | `XyzConfiguration.D65`        | static preset        | default white             |
|  [02]   | `XyzConfiguration.D50`        | static preset        | ICC profile white         |
|  [03]   | `XyzConfiguration..ctor`      | custom configuration | #PHOTOMETRIC custom white |
|  [04]   | `XyzConfiguration.WhitePoint` | property             | white-point policy        |
|  [05]   | `XyzConfiguration.Observer`   | property             | CIE observer policy       |

---

## [11]-[DYNAMIC_RANGE]

`DynamicRange` presets carry white, maximum, and minimum luminance values; the custom constructor also accepts HLG white-level and name values.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]  | [CAPABILITY]      |
| :-----: | :---------------------------- | :------------ | :---------------- |
|  [01]   | `DynamicRange.Standard`       | static preset | SDR range         |
|  [02]   | `DynamicRange.High`           | static preset | HDR default range |
|  [03]   | `DynamicRange..ctor`          | range ctor    | custom HDR range  |
|  [04]   | `DynamicRange.WhiteLuminance` | property      | reference white   |
|  [05]   | `DynamicRange.MaxLuminance`   | property      | maximum luminance |

`Rec2100Pq` and `Rec2100Hlg` use the `FromLinear`/`ToLinear` delegates that call into internal `Pq` and `Hlg` OETF/EOTF kernels, scaled by `DynamicRange.WhiteLuminance / 203` at construction. The tone-curve math is inside the library; the consumer sets `DynamicRange` in `Configuration` only.

---

## [12]-[SPECTRAL_SPD]

`Spd` extends `SpectralCoefficients`; power distributions convert to XYZ through `new Unicolour(config, Spd)`. Valid intervals are 0, 1, or 5 nm, and `Spd.D65` spans 300-830 nm at 1 nm.

| [INDEX] | [SURFACE]     | [CALL_SHAPE]     | [CAPABILITY]                       |
| :-----: | :------------ | :--------------- | :--------------------------------- |
|  [01]   | `Spd..ctor`   | coefficient ctor | #PHOTOMETRIC custom illuminant SPD |
|  [02]   | `Spd.D65`     | static SPD       | D65 reference illuminant           |
|  [03]   | `Spd.IsValid` | property         | interval validation                |

[SPD_CONSTRUCTION_PATH]: `new Unicolour(Configuration, Spd)` projects through the internal SPD-to-XYZ path. The reflectance to XYZ path uses `Pigment[]`/`KubelkaMunk` with `Spd` illuminant, not the `Spd` ctor directly.

---

## [13]-[GAMUT_MAP]

`GamutMap` enum rows select the public RGB-gamut mapping strategy.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :--------------- | :----------------------- |
|  [01]   | `GamutMap.RgbClipping`          | channel clipping | fast clamp               |
|  [02]   | `GamutMap.OklchChromaReduction` | chroma reduction | perceptual quality       |
|  [03]   | `GamutMap.WxyPurityReduction`   | purity reduction | spectral gamut reduction |

`Unicolour` operations compose interpolation, compositing, simulation, and gamut mapping.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE]      | [CAPABILITY]              |
| :-----: | :--------------------------------------------------------------------------- | :---------------- | :------------------------ |
|  [01]   | `Unicolour.Mix`                                                              | interpolation     | single colour output      |
|  [02]   | `Unicolour.Palette`                                                          | interpolation set | generated colour sequence |
|  [03]   | `Unicolour.Blend(Unicolour backdrop, BlendMode blendMode)`                   | compositing       | blend-mode compositing    |
|  [04]   | `Unicolour.Simulate(Cvd cvd, double severity = 1.0)`                         | simulation        | CVD simulation            |
|  [05]   | `Unicolour.MapToRgbGamut(GamutMap gamutMap = GamutMap.OklchChromaReduction)` | gamut map         | maps to RGB gamut         |
|  [06]   | `Unicolour.MapToPointerGamut()`                                              | gamut map         | maps to Pointer gamut     |
|  [07]   | `Unicolour.MapToMacAdamLimits()`                                             | gamut map         | maps to MacAdam limits    |

---

## [14]-[CHROMATICITY_WHITE_POINT]

`Chromaticity` is the `record(double X, double Y)` value and exposes `.U`, `.V`, `.Uv`, and `.Xy` projections. White-point construction composes an `Illuminant` with an explicit `Observer`; `XyzConfiguration.D65` and `XyzConfiguration.D50` bind `Observer.Degree2`.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]       | [CAPABILITY]           |
| :-----: | :-------------------------- | :----------------- | :--------------------- |
|  [01]   | `Chromaticity`              | chromaticity value | chromaticity geometry  |
|  [02]   | `Chromaticity.FromUv(u, v)` | static conversion  | UCS conversion         |
|  [03]   | `WhitePoint`                | white-point record | illuminant white point |
|  [04]   | `Illuminant.D65`            | static illuminant  | standard white point   |
|  [05]   | `Illuminant.D50`            | static illuminant  | standard white point   |
|  [06]   | `Observer.Degree2`          | static observer    | CIE observer selection |
|  [07]   | `Observer.Degree10`         | static observer    | CIE observer selection |
