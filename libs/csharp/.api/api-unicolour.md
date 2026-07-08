# [RASM_API_UNICOLOUR]

`Wacton.Unicolour` (MIT, pure-managed `netstandard2.0`, zero dependencies,
ALC-safe) supplies a 40-space colour model via the immutable `Unicolour` type, with
construction from any `ColourSpace` discriminant, lazy-evaluated conversion accessors
for every space, perceptual delta-E metrics (`DeltaE`), gamut mapping, hue-aware
mixing/palette generation (`HueSpan`), spectral SPD and blackbody/CCT (`Locus`)
intake, and fully configurable working spaces through `Configuration` and its
`RgbConfiguration`/`XyzConfiguration`/`YbrConfiguration`/`CamConfiguration`/`DynamicRange`/`IccConfiguration`
slots. The companion `Wacton.Unicolour.Datasets` (`netstandard2.0`) adds named
colour / ColorChecker reference sets and perceptual colourmaps over the same model.

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

| [INDEX] | [MEMBER] | [USED_BY] |
|:-----: |:---------- |:---------------------------- |
| [01] | `Rgb` | COLOR_SPACE_AXIS construction |
| [02] | `Rgb255` | COLOR_SPACE_AXIS |
| [03] | `RgbLinear` | #BSDF_MODEL linear light |
| [04] | `Hsb` |  |
| [05] | `Hsl` |  |
| [06] | `Hwb` |  |
| [07] | `Hsi` |  |
| [08] | `Hsluv` | perceptual hue-saturation |
| [09] | `Hpluv` |  |
| [10] | `Okhsv` |  |
| [11] | `Okhsl` |  |
| [12] | `Okhwb` |  |

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — XYZ, Lab, and appearance model spaces
- rail: colour

| [INDEX] | [MEMBER] | [USED_BY] |
|:-----: |:-------- |:--------------------------- |
| [01] | `Xyz` | #PHOTOMETRIC tristimulus |
| [02] | `Xyy` | chromaticity construction |
| [03] | `Wxy` | dominant wavelength |
| [04] | `Lab` | delta-E76/94/2000, gamut map |
| [05] | `Lchab` | chroma-reduction gamut map |
| [06] | `Luv` |  |
| [07] | `Lchuv` |  |
| [08] | `Lms` | chromatic adaptation |
| [09] | `Ipt` |  |
| [10] | `Ictcp` | DeltaE.Itp (HDR perceptual) |
| [11] | `Jzazbz` |  |
| [12] | `Jzczhz` | DeltaE.Z |
| [13] | `Cam02` | DeltaE.Cam02 |
| [14] | `Cam16` | DeltaE.Cam16 |
| [15] | `Hct` |  |
| [16] | `Munsell` |  |

[PUBLIC_TYPE_SCOPE]: ColourSpace enum — Oklab, Okl-derived, and video spaces
- rail: colour

| [INDEX] | [MEMBER] | [USED_BY] |
|:-----: |:------- |:----------------------------- |
| [01] | `Oklab` | DeltaE.Ok, gamut mixing |
| [02] | `Oklch` | OklchChromaReduction gamut map |
| [03] | `Oklrab` |  |
| [04] | `Oklrch` |  |
| [05] | `Xyb` |  |
| [06] | `Ypbpr` |  |
| [07] | `Ycbcr` |  |
| [08] | `Ycgco` |  |
| [09] | `Yuv` |  |
| [10] | `Yiq` |  |
| [11] | `Ydbdr` |  |
| [12] | `Tsl` |  |

[PUBLIC_TYPE_SCOPE]: Unicolour — construction overloads
- rail: colour

| [INDEX] | [SIGNATURE] | [USED_BY] |
|:-----: |:-------------------------------------------------------------------------------------------- |:------------------------------------------------ |
| [01] | `new Unicolour(ColourSpace, double, double, double, double alpha =)` | COLOR_SPACE_AXIS |
| [02] | `new Unicolour(ColourSpace, (double first, double second, double third), double alpha =)` |  |
| [03] | `new Unicolour(ColourSpace, (double first, double second, double third, double alpha))` |  |
| [04] | `new Unicolour(Configuration, ColourSpace, double, double, double, double alpha =)` | COLOR_SPACE_AXIS with Display P3 / Rec2020 config |
| [05] | `new Unicolour(string hex)` |  |
| [06] | `new Unicolour(Configuration, string hex)` |  |
| [07] | `new Unicolour(ColourSpace, double grey, double alpha =)` |  |
| [08] | `new Unicolour(Chromaticity, double luminance =)` | #PHOTOMETRIC white-point construction |
| [09] | `new Unicolour(Configuration, Chromaticity, double luminance =)` |  |
| [10] | `new Unicolour(double cct, Locus locus = Locus.Blackbody, double luminance =)` | #PHOTOMETRIC blackbody |
| [11] | `new Unicolour(Spd)` | #PHOTOMETRIC spectral power distribution → XYZ |
| [12] | `new Unicolour(Configuration, Spd)` |  |
| [13] | `new Unicolour(Pigment[], double[])` | Kubelka-Munk pigment mixing |

[PUBLIC_TYPE_SCOPE]: Unicolour — conversion accessors (lazy-evaluated)
- rail: colour

| [INDEX] | [MEMBER] | [TYPE] | [USED_BY] |
|:-----: |:------------------- |:---------- |:------------------------------------- |
| [01] | `.Rgb` | `Rgb` | sRGB byte output |
| [02] | `.RgbLinear` | `RgbLinear` | #BSDF_MODEL linear-light pipeline |
| [03] | `.Xyz` | `Xyz` | #PHOTOMETRIC tristimulus |
| [04] | `.Xyy` | `Xyy` | chromaticity, dominant wavelength |
| [05] | `.Lab` | `Lab` | delta-E, perceptual sort |
| [06] | `.Lchab` | `Lchab` | chroma-reduction gamut mapping |
| [07] | `.Oklab` | `Oklab` | DeltaE.Ok, perceptual interpolation |
| [08] | `.Oklch` | `Oklch` | OklchChromaReduction gamut mapping |
| [09] | `.Ictcp` | `Ictcp` | DeltaE.Itp (HDR), Rec.2100 |
| [10] | `.Jzczhz` | `Jzczhz` | DeltaE.Z |
| [11] | `.Cam02` | `Cam02` | DeltaE.Cam02 |
| [12] | `.Cam16` | `Cam16` | DeltaE.Cam16 |
| [13] | `.Lms` | `Lms` | chromatic adaptation |
| [14] | `GetRepresentation(ColourSpace)` | value object | returns the representation for a selected colour space |
| [15] | `.Hsluv` | `Hsluv` | perceptual hue-uniform |
| [16] | `.Hct` | `Hct` | Material the caller / dynamic color |
| [17] | `.Wxy` | `Wxy` | dominant wavelength, excitation purity |
| [18] | `.RelativeLuminance` | `double` | WCAG contrast |
| [19] | `.IsInRgbGamut` | `bool` | gamut-check gate |

[PUBLIC_TYPE_SCOPE]: Unicolour — scalar and metadata accessors
- rail: colour

| [INDEX] | [MEMBER] | [TYPE] | [USED_BY] |
|:-----: |:-------------------- |:-------------- |:----------------------- |
| [01] | `.Chromaticity` | `Chromaticity` | white-point comparison |
| [02] | `.DominantWavelength` | `double` | spectral locus tagging |
| [03] | `.ExcitationPurity` | `double` | spectral locus tagging |
| [04] | `.Temperature` | `Temperature` | blackbody CCT readout |
| [05] | `.IsInPointerGamut` | `bool` | real-surface gamut check |
| [06] | `.IsInMacAdamLimits` | `bool` | spectral limits check |
| [07] | `.IsImaginary` | `bool` | outside spectral locus |
| [08] | `.Hex` | `string` |  |
| [09] | `.Alpha` | `Alpha` |  |
| [10] | `.Configuration` | `Configuration` | config round-trip |

[PUBLIC_TYPE_SCOPE]: DeltaE enum — all 12 members
- rail: colour

Called via `unicolour.Difference(reference, DeltaE.X)`.

| [INDEX] | [MEMBER] | [USED_BY] |
|:-----: |:------------------ |:-------------------------------- |
| [01] | `Cie76` | baseline perceptual diff |
| [02] | `Cie94` | graphic-arts weighting |
| [03] | `Cie94Textiles` |  |
| [04] | `Ciede2000` | industry-standard appearance diff |
| [05] | `CmcAcceptability` |  |
| [06] | `CmcPerceptibility` |  |
| [07] | `Itp` | HDR perceptual diff |
| [08] | `Z` | HDR perceptual diff (Safdar 2021) |
| [09] | `Hyab` |  |
| [10] | `Ok` | perceptual mixing |
| [11] | `Cam02` | appearance model |
| [12] | `Cam16` | appearance model |

[PUBLIC_TYPE_SCOPE]: Configuration, RgbConfiguration, XyzConfiguration, DynamicRange
- rail: colour

`Configuration` carries RGB, XYZ, YBR, CAM, dynamic-range, and ICC working-space policy. RGB custom construction uses chromaticity primaries, a white point, transfer delegates, and a name; dynamic-range custom construction carries white, maximum, minimum, HLG white-level, and name values.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------- |:----------------- |:------------------- |
| [01] | `Configuration.Default` | static preset | baseline sRGB/D65 |
| [02] | `Configuration..ctor` | configuration ctor | custom working space |

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------- |:--------------- |:------------------------ |
| [01] | `RgbConfiguration.StandardRgb` | static preset | sRGB |
| [02] | `RgbConfiguration.DisplayP3` | static preset | COLOR_SPACE_AXIS display |
| [03] | `RgbConfiguration.Rec2020` | static preset | COLOR_SPACE_AXIS wide RGB |
| [04] | `RgbConfiguration.Rec2100Pq` | static preset | #PHOTOMETRIC PQ transfer |
| [05] | `RgbConfiguration.A98` | static preset | Adobe RGB |
| [06] | `RgbConfiguration.ProPhoto` | static preset | ProPhoto / ROMM RGB |
| [07] | `RgbConfiguration.Aces20651` | static preset | ACES 2065-1 |
| [08] | `RgbConfiguration.Acescg` | static preset | ACEScg scene-linear AP1 |
| [09] | `RgbConfiguration..ctor` | custom primaries | RGB working space |

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------- |:----------- |:---------------- |
| [01] | `XyzConfiguration.D65` | D65 preset | default white |
| [02] | `XyzConfiguration.D50` | D50 preset | ICC profile white |

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------- |:----------- |:-------------- |
| [01] | `DynamicRange.Standard` | SDR preset | standard range |
| [02] | `DynamicRange.High` | HDR preset | HDR default |
| [03] | `DynamicRange..ctor` | range ctor | custom HDR span |

[PUBLIC_TYPE_SCOPE]: Spd, GamutMap, Chromaticity, WhitePoint
- rail: colour

`Spd` construction accepts measured coefficients at 0, 1, or 5 nm intervals. `Chromaticity` is the `record(double X, double Y)` value with `.U`, `.V`, `.Uv`, and `.Xy` projections.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------- |:------------------- |:----------------------- |
| [01] | `Spd..ctor` | coefficient ctor | #PHOTOMETRIC custom SPD |
| [02] | `Spd.D65` | static SPD reference | D65 reference illuminant |

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------ |:---------------- |:------------------------ |
| [01] | `GamutMap.RgbClipping` | channel clipping | fast gamut clamp |
| [02] | `GamutMap.OklchChromaReduction` | chroma reduction | perceptual gamut mapping |
| [03] | `GamutMap.WxyPurityReduction` | purity reduction | spectral gamut mapping |
| [04] | `Unicolour.Mix` | interpolation | single mixed colour |
| [05] | `Unicolour.Palette` | interpolation set | generated colour sequence |


[PUBLIC_TYPE_SCOPE]: blend, simulation, and ICC carriers
- rail: colour

| [INDEX] | [SYMBOL] | [CAPABILITY] |
|:-----: |:------------------------ |:----------------------------------------------- |
| [01] | `BlendMode` | blend-mode enum consumed by `Unicolour.Blend` |
| [02] | `Cvd` | colour-vision-deficiency enum consumed by `Simulate` |
| [03] | `Icc` | ICC carrier with `Channels`, `Profile`, `Header`, `Tags` |
| [04] | `IccConfiguration` | ICC-backed configuration constructors |
| [05] | `YbrConfiguration.Jpeg` / `Rec601` / `Rec709` / `Rec2020` | YBR configuration statics |
| [06] | `CamConfiguration.StandardRgb` / `Hct` | appearance-model configuration statics |
| [07] | `Configuration` | `ConvertToConfiguration(Configuration)` target |

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:-------------------------- |:----------------- |:---------------------------------- |
| [01] | `Chromaticity` | chromaticity value | white point and wavelength geometry |
| [02] | `Chromaticity.FromUv(u, v)` | static conversion | UCS conversion |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Unicolour — primary operations
- rail: colour

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [RAIL] |
|:-----: |:---------------------------------------------------- |:--------------------------------------- |:-------------------------------- |
| [01] | `new Unicolour(ColourSpace, double, double, double)` | ctor | colour construction |
| [02] | `new Unicolour(Configuration, ColourSpace,...)` | ctor | custom working-space construction |
| [03] | `new Unicolour(string hex)` | ctor | hex intake |
| [04] | `new Unicolour(Spd)` | ctor | spectral intake |
| [05] | `new Unicolour(Pigment[], double[])` | ctor | Kubelka-Munk mix |
| [06] | `double Difference(Unicolour reference, DeltaE deltaE)` | instance call → `double` | perceptual delta |
| [07] | `double Contrast(Unicolour other)` | instance call → `double` | WCAG contrast |
| [08] | `Unicolour Mix(Unicolour other, ColourSpace, double amount =, HueSpan = Shorter, bool premultiplyAlpha = true)` | instance call → `Unicolour` | hue-aware interpolation |
| [09] | `IEnumerable<Unicolour> Palette(Unicolour other, ColourSpace, int count, HueSpan = Shorter, bool premultiplyAlpha = true)` | instance call → `IEnumerable<Unicolour>` | hue-aware palette generation |

`HueSpan` enum (mix/palette hue-traversal axis): `Shorter`, `Longer`, `Increasing`, `Decreasing`. `Locus` enum (CCT-construction radiator): `Blackbody`, `Daylight`.

## [04]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `Wacton.Unicolour`
- primary type: `Unicolour` — immutable, lazy-evaluated per colour-space accessor
- construction discriminant: `ColourSpace` enum (40 members)
- working-space policy: `Configuration` (holds `Rgb`, `Xyz`, `Ybr`, `Cam`, `DynamicRange`, `Icc` slots)
- display primaries (DisplayP3, Rec2020, AdobeRGB) are `RgbConfiguration` statics, not `ColourSpace` cases

[INTEGRATION]:
- AppUi boundary: `Unicolour` is the canonical colour value; map to/from Avalonia `Color`/`HsvColor`
 (`api-avalonia-color.md`) only at the view edge — read `.Rgb` (sRGB bytes) outbound and construct
 `new Unicolour(ColourSpace.Rgb255, r, g, b)` inbound, keeping all perceptual maths in `Unicolour`.
- Accessibility: `Contrast(other)` is the WCAG ratio and `RelativeLuminance` the WCAG luminance;
 drive theme contrast gates off these rather than re-deriving luminance from Avalonia brushes.
- Palette pipelines: `Mix`/`Palette` over `ColourSpace.Oklab`/`Oklch` with `HueSpan` produce
 perceptually-even theme ramps; feed the `IEnumerable<Unicolour>` straight into a swatch
 `ItemsSource` after a single `.Rgb`-to-`Color` projection.
- Materials/BSDF: the Materials consumer composes the spectral/appearance surface
 (`Spd`, `Pigment[]`/`KubelkaMunk`, `DeltaE.Ciede2000`, `Configuration` working spaces) for
 #PHOTOMETRIC and #BSDF_MODEL pages; AppUi reuses the same model for UI colour without a
 second colour library.

[GAMUT_LAW]:
- public gamut mapping is `Unicolour.MapToRgbGamut(GamutMap gamutMap = GamutMap.OklchChromaReduction)`, `Unicolour.MapToPointerGamut()`, and `Unicolour.MapToMacAdamLimits()`
- no public `MapToGamut` instance method exists; `GamutMap` selects the RGB-gamut mapping strategy only

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

All `Unicolour` construction routes are on `Wacton.Unicolour.Unicolour`. Alpha defaults to where the overload exposes it, and configuration variants select the working RGB/XYZ/dynamic-range policy before conversion.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------ |:--------------------------- |:------------------------------------ |
| [01] | `Unicolour(ColourSpace, …)` | space triple ctor | COLOR_SPACE_AXIS construction |
| [02] | `Unicolour(ColourSpace, …)` | space tuple ctor | tuple-based colour construction |
| [03] | `Unicolour(Configuration, ColourSpace, …)` | configured space ctor | custom working-space construction |
| [04] | `ConvertToConfiguration(Configuration config)` | instance conversion | remaps a colour to another configuration |
| [05] | `Unicolour(string hex)` | hex ctor | hex intake |
| [06] | `Unicolour(Configuration, string hex)` | configured hex ctor | hex intake in a custom working space |
| [07] | `Unicolour(ColourSpace, double grey, …)` | grey ctor | single-channel construction |
| [08] | `Unicolour(Chromaticity, …)` | chromaticity ctor | #PHOTOMETRIC white-point construction |
| [09] | `Unicolour(Configuration, Chromaticity, …)` | configured chromaticity ctor | custom white-point construction |
| [10] | `Unicolour(double cct, Locus, …)` | CCT ctor | #PHOTOMETRIC blackbody |
| [11] | `Unicolour(Spd)` | SPD ctor | spectral power distribution to XYZ |
| [12] | `Unicolour(Configuration, Spd)` | configured SPD ctor | custom spectral intake |
| [13] | `Unicolour(Pigment[], double[])` | pigment mix ctor | Kubelka-Munk pigment mixing |

---

## [06]-[CONVERSION_ACCESSORS]

Lazy-evaluated properties on `Unicolour`; all spelled exactly as below.

| [INDEX] | [MEMBER] | [SIGNATURE] | [USED_BY] | [SOURCE] |
| :-----: | :-------------------- | :-------------------------------------------- | :------------------------------------- | :---------------- |
| [01] | `.Rgb` | `Rgb` | sRGB byte output | property |
| [02] | `.RgbLinear` | `RgbLinear` | #BSDF_MODEL linear-light pipeline | member surface |
| [03] | `.Xyz` | `Xyz` | #PHOTOMETRIC tristimulus | member surface |
| [04] | `.Xyy` | `Xyy` | chromaticity, dominant wavelength | member surface |
| [05] | `.Lab` | `Lab` | delta-E, perceptual sort | member surface |
| [06] | `.Lchab` | `Lchab` | chroma-reduction gamut mapping | member surface |
| [07] | `.Oklab` | `Oklab` | DeltaE.Ok, perceptual interpolation | member surface |
| [08] | `.Oklch` | `Oklch` | OklchChromaReduction gamut mapping | member surface |
| [09] | `.Ictcp` | `Ictcp` | DeltaE.Itp (HDR), Rec.2100 | member surface |
| [10] | `.Jzczhz` | `Jzczhz` | DeltaE.Z | member surface |
| [11] | `.Cam02` | `Cam02` | DeltaE.Cam02 | member surface |
| [12] | `.Cam16` | `Cam16` | DeltaE.Cam16 | member surface |
| [13] | `.Lms` | `Lms` | chromatic adaptation | member surface |
| [14] | `.Hsluv` | `Hsluv` | perceptual hue-uniform | member surface |
| [15] | `.Hpluv` | `Hpluv` |  | member surface |
| [16] | `.Hct` | `Hct` | Material the caller / dynamic color | member surface |
| [17] | `.Wxy` | `Wxy` | dominant wavelength, excitation purity | member surface |
| [18] | `.RelativeLuminance` | `double` | WCAG contrast | member surface |
| [19] | `.Chromaticity` | `Chromaticity` — `record(double X, double Y)` | white-point comparison | member surface |
| [20] | `.DominantWavelength` | `double` (from `.Wxy.W`) | spectral locus tagging | member surface |
| [21] | `.ExcitationPurity` | `double` (from `.Wxy.X`) | spectral locus tagging | member surface |
| [22] | `.Temperature` | `Temperature` | blackbody CCT readout | member surface |
| [23] | `.IsInRgbGamut` | `bool` | gamut-check gate | member surface |
| [24] | `.IsInPointerGamut` | `bool` | real-surface gamut check | member surface |
| [25] | `.IsInMacAdamLimits` | `bool` | spectral limits check | member surface |
| [26] | `.IsImaginary` | `bool` | outside spectral locus | member surface |
| [27] | `.Hex` | `string` |  | member surface |
| [28] | `.Alpha` | `Alpha` |  | member surface |
| [29] | `.Configuration` | `Configuration` | config round-trip | member surface |

---

## [07]-[DELTA_E]

`DeltaE` enum — all 12 members verified. Called via `unicolour.Difference(reference, DeltaE.X)`.

| [INDEX] | [MEMBER] | [SIGNATURE] | [USED_BY] | [SOURCE] |
| :-----: | :------------------ | :-------------------------------------- | :-------------------------------- | :--------------------------- |
| [01] | `Cie76` | `DeltaE.Cie76` | baseline perceptual diff | member surface |
| [02] | `Cie94` | `DeltaE.Cie94` | graphic-arts weighting | member surface |
| [03] | `Cie94Textiles` | `DeltaE.Cie94Textiles` |  | member surface |
| [04] | `Ciede2000` | `DeltaE.Ciede2000` | industry-standard appearance diff | #BSDF_MODEL appearance match |
| [05] | `CmcAcceptability` | `DeltaE.CmcAcceptability` |  | member surface |
| [06] | `CmcPerceptibility` | `DeltaE.CmcPerceptibility` |  | member surface |
| [07] | `Itp` | `DeltaE.Itp` — DeltaEItp × 720 on ICtCp | HDR perceptual diff | member surface |
| [08] | `Z` | `DeltaE.Z` — on JzCzhz | HDR perceptual diff (Safdar 2021) | member surface |
| [09] | `Hyab` | `DeltaE.Hyab` |  | member surface |
| [10] | `Ok` | `DeltaE.Ok` — Euclidean on Oklab | perceptual mixing | member surface |
| [11] | `Cam02` | `DeltaE.Cam02` | appearance model | member surface |
| [12] | `Cam16` | `DeltaE.Cam16` — × d^ | appearance model | member surface |

Entry point: `double Unicolour.Difference(Unicolour reference, DeltaE deltaE)` and `double Unicolour.Contrast(Unicolour other)` (WCAG).

---

## [08]-[CONFIGURATION]

`Configuration` carries RGB, XYZ, YBR, CAM, dynamic-range, and ICC policy for the
working space. The ctor takes every slot as an optional argument:
`Configuration(RgbConfiguration? = null, XyzConfiguration? = null, YbrConfiguration? = null, CamConfiguration? = null, DynamicRange? = null, IccConfiguration? = null)`,
so a custom working space overrides only the axes it cares about and inherits
`Default` for the rest.

| [INDEX] | [SURFACE] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:--------------------------- |:----------------- |:------------------------- |
| [01] | `Configuration.Default` | static preset | baseline sRGB/D65 policy |
| [02] | `Configuration..ctor(rgb?, xyz?, ybr?, cam?, dynamicRange?, icc?)` | configuration ctor | custom working space (per-slot override) |
| [03] | `Configuration.Rgb` | property | RGB working space |
| [04] | `Configuration.Xyz` | property | XYZ working space |
| [05] | `Configuration.Ybr` | property | YCbCr/YPbPr matrix policy |
| [06] | `Configuration.Cam` | property | CAM02/16 viewing-condition policy |
| [07] | `Configuration.DynamicRange` | property | HDR/SDR tone policy |
| [08] | `Configuration.Icc` | property | ICC working-profile policy |

---

## [09]-[RGB_CONFIGURATION]

Static presets on `RgbConfiguration` keep exact capitalization. The rows below are the
display/scene/log presets AppUi composes against; the full static set also includes
broadcast primaries (`Rec601Line625`, `Rec601Line525`, `XvYcc`, `Pal`, `PalM`,
`Pal625`, `Pal525`, `Ntsc`) selected the same way. The custom constructor carries red,
green, and blue chromaticities, a white point, transfer delegates, and a name.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------- |:--------------- |:-------------------------- |
| [01] | `StandardRgb` | static preset | sRGB / COLOR_SPACE_AXIS |
| [02] | `DisplayP3` | static preset | COLOR_SPACE_AXIS display-p3 |
| [03] | `Rec2020` | static preset | COLOR_SPACE_AXIS rec2020 |
| [04] | `Rec2100Pq` | static preset | #PHOTOMETRIC PQ transfer |
| [05] | `Rec2100Hlg` | static preset | #PHOTOMETRIC HLG transfer |
| [06] | `A98` | static preset | Adobe RGB |
| [07] | `ProPhoto` | static preset | ProPhoto / ROMM RGB |
| [08] | `Aces20651` | static preset | ACES 2065-1 scene-linear |
| [09] | `Acescg` | static preset | ACEScg scene-linear AP1 |
| [10] | `Acescct` | static preset | ACEScct log |
| [11] | `Acescc` | static preset | ACEScc log |
| [12] | `Rec709` | static preset | Rec.709 RGB |
| [13] | `RgbConfiguration..ctor` | custom primaries | custom RGB working space |

---

## [10]-[XYZ_CONFIGURATION]

`XyzConfiguration.D65` and `XyzConfiguration.D50` use the degree-2 observer and Bradford adaptation for the standard working whites.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------- |:------------------- |:------------------------ |
| [01] | `XyzConfiguration.D65` | static preset | default white |
| [02] | `XyzConfiguration.D50` | static preset | ICC profile white |
| [03] | `XyzConfiguration..ctor` | custom configuration | #PHOTOMETRIC custom white |
| [04] | `XyzConfiguration.WhitePoint` | property | white-point policy |
| [05] | `XyzConfiguration.Observer` | property | CIE observer policy |

---

## [11]-[DYNAMIC_RANGE]

`DynamicRange` presets carry white, maximum, and minimum luminance values; the custom constructor also accepts HLG white-level and name values.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------- |:------------ |:---------------- |
| [01] | `DynamicRange.Standard` | static preset | SDR range |
| [02] | `DynamicRange.High` | static preset | HDR default range |
| [03] | `DynamicRange..ctor` | range ctor | custom HDR range |
| [04] | `DynamicRange.WhiteLuminance` | property | reference white |
| [05] | `DynamicRange.MaxLuminance` | property | maximum luminance |

`Rec2100Pq` and `Rec2100Hlg` use the `FromLinear`/`ToLinear` delegates that call into internal `Pq` and `Hlg` OETF/EOTF kernels, scaled by `DynamicRange.WhiteLuminance / 203` at construction. The tone-curve math is inside the library; the consumer sets `DynamicRange` in `Configuration` only.

---

## [12]-[SPECTRAL_SPD]

`Spd` extends `SpectralCoefficients`; power distributions convert to XYZ through `new Unicolour(config, Spd)`. Valid intervals are 0, 1, or 5 nm, and `Spd.D65` spans 300-830 nm at 1 nm.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------ |:------------------ |:--------------------------------- |
| [01] | `Spd..ctor` | coefficient ctor | #PHOTOMETRIC custom illuminant SPD |
| [02] | `Spd.D65` | static SPD | D65 reference illuminant |
| [03] | `Spd.IsValid` | property | interval validation |

[SPD_CONSTRUCTION_PATH]: `new Unicolour(Configuration, Spd)` projects through the internal SPD-to-XYZ path. The reflectance to XYZ path uses `Pigment[]`/`KubelkaMunk` with `Spd` illuminant, not the `Spd` ctor directly.

---

## [13]-[GAMUT_MAP]

`GamutMap` enum rows select the public RGB-gamut mapping strategy. Public interpolation uses `Unicolour.Mix` for one colour and `Unicolour.Palette` for a generated sequence.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------ |:--------------- |:----------------------- |
| [01] | `GamutMap.RgbClipping` | channel clipping | fast clamp |
| [02] | `GamutMap.OklchChromaReduction` | chroma reduction | perceptual quality |
| [03] | `GamutMap.WxyPurityReduction` | purity reduction | spectral gamut reduction |

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------ |:---------------- |:------------------------ |
| [01] | `Unicolour.Mix` | interpolation | single colour output |
| [02] | `Unicolour.Palette` | interpolation set | generated colour sequence |
| [03] | `Unicolour.Blend(Unicolour backdrop, BlendMode blendMode)` | compositing | blend-mode compositing |
| [04] | `Unicolour.Simulate(Cvd cvd, double severity =)` | simulation | colour-vision-deficiency simulation |
| [05] | `Unicolour.MapToRgbGamut(GamutMap gamutMap = GamutMap.OklchChromaReduction)` | gamut map | maps to RGB gamut |
| [06] | `Unicolour.MapToPointerGamut()` | gamut map | maps to Pointer gamut |
| [07] | `Unicolour.MapToMacAdamLimits()` | gamut map | maps to MacAdam limits |

Gamut mapping is exposed through the public `Unicolour.MapToRgbGamut`, `MapToPointerGamut`, and `MapToMacAdamLimits` instance methods. No public `MapToGamut` instance method exists.

---

## [14]-[CHROMATICITY_WHITE_POINT]

`Chromaticity` is the `record(double X, double Y)` value and exposes `.U`, `.V`, `.Uv`, and `.Xy` projections. Standard white-point construction uses D65/D50 illuminants and degree-2/degree-10 observers.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------------- |:----------------- |:---------------------------------- |
| [01] | `Chromaticity` | chromaticity value | white-point and wavelength geometry |
| [02] | `Chromaticity.FromUv(u, v)` | static conversion | UCS conversion |
| [03] | `WhitePoint` | wrapper class | illuminant white point |
| [04] | `Illuminant.D65` / `Illuminant.D50` | static illuminants | standard white points |
| [05] | `Observer.Degree2` / `Observer.Degree10` | static observers | CIE observer selection |
