# [RASM_API_UNICOLOUR]

`Wacton.Unicolour` owns an immutable colour value that admits any `ColourSpace` and lazily projects every other, with `Configuration` binding working-space policy across its slots. `DeltaE`, `BlendMode`, `Cvd`, and `GamutMap` extend the value into perceptual difference, compositing, vision-deficiency simulation, and gamut bounding; `Spd` and `Pigment` intake resolves measured spectral power and Kubelka-Munk reflectance to XYZ.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wacton.Unicolour`
- package: `Wacton.Unicolour` (MIT)
- assembly: `Wacton.Unicolour`
- namespace: `Wacton.Unicolour`, `Wacton.Unicolour.Icc`
- asset: `netstandard2.0` pure-managed, zero-dependency, ALC-safe
- rail: colour

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the colour value and its operation vocabularies

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------ | :------------ | :--------------------------------------- |
|  [01]   | `Unicolour`   | class         | immutable lazy colour value              |
|  [02]   | `ColourSpace` | enum          | construction and conversion discriminant |
|  [03]   | `DeltaE`      | enum          | perceptual-difference metric selector    |
|  [04]   | `BlendMode`   | enum          | W3C compositing selector                 |
|  [05]   | `Cvd`         | enum          | colour-vision-deficiency selector        |
|  [06]   | `GamutMap`    | enum          | RGB gamut-mapping strategy               |
|  [07]   | `HueSpan`     | enum          | mix and palette hue-traversal axis       |
|  [08]   | `Locus`       | enum          | CCT-construction radiator selector       |

`ColourSpace` doubles as the `Unicolour` accessor roster: reading `colour.Oklab` projects the `Oklab` case. Display primaries (`DisplayP3`, `Rec2020`, the ACES presets) are `RgbConfiguration` statics, not `ColourSpace` cases; the broadcast luma encoders (`Ycbcr`, `Yuv`, `Yiq`) are `ColourSpace` cases whose matrix is `YbrConfiguration`-governed.

[`ColourSpace`]: `Rgb` `Rgb255` `RgbLinear` `Hsb` `Hsl` `Hwb` `Hsi` `Xyz` `Xyy` `Wxy` `Lab` `Lchab` `Luv` `Lchuv` `Hsluv` `Hpluv` `Ypbpr` `Ycbcr` `Ycgco` `Yuv` `Yiq` `Ydbdr` `Tsl` `Xyb` `Lms` `Ipt` `Ictcp` `Jzazbz` `Jzczhz` `Oklab` `Oklch` `Okhsv` `Okhsl` `Okhwb` `Oklrab` `Oklrch` `Cam02` `Cam16` `Hct` `Munsell`
[`DeltaE`]: `Cie76` `Cie94` `Cie94Textiles` `Ciede2000` `CmcAcceptability` `CmcPerceptibility` `Itp` `Z` `Hyab` `Ok` `Cam02` `Cam16`
[`BlendMode`]: `Normal` `Multiply` `Screen` `Overlay` `Darken` `Lighten` `ColourDodge` `ColourBurn` `HardLight` `SoftLight` `Difference` `Exclusion` `Hue` `Saturation` `Colour` `Luminosity`
[`Cvd`]: `Protanopia` `Protanomaly` `Deuteranopia` `Deuteranomaly` `Tritanopia` `Tritanomaly` `BlueConeMonochromacy` `Achromatopsia`
[`GamutMap`]: `RgbClipping` `OklchChromaReduction` `WxyPurityReduction`
[`HueSpan`]: `Shorter` `Longer` `Increasing` `Decreasing`
[`Locus`]: `Blackbody` `Daylight`

[PUBLIC_TYPE_SCOPE]: working-space configuration

`Configuration.Default` binds sRGB, D65, Rec.601, sRGB CAM, HDR range, and no ICC profile; a custom `Configuration` overrides only selected slots and inherits `Default` elsewhere.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :----------------- | :------------ | :--------------------------------------------- |
|  [01]   | `Configuration`    | class         | groups the six working-space slots; `.Default` |
|  [02]   | `RgbConfiguration` | class         | RGB primaries, white point, transfer delegates |
|  [03]   | `XyzConfiguration` | class         | working white, observer, chromatic adaptation  |
|  [04]   | `YbrConfiguration` | class         | luma-chroma matrix and quantization range      |
|  [05]   | `CamConfiguration` | class         | CAM02/16 viewing conditions                    |
|  [06]   | `DynamicRange`     | class         | SDR/HDR luminance span and HLG white level     |
|  [07]   | `IccConfiguration` | class         | ICC profile plus rendering intent; `.None`     |

`RgbConfiguration` and the custom `Configuration` slots each carry a constructor taking `Chromaticity` primaries, a `WhitePoint`, `FromLinear`/`ToLinear` transfer delegates, and a name, admitting an unlisted working space.

[`RgbConfiguration`]: `StandardRgb` `DisplayP3` `Rec2020` `Rec2100Pq` `Rec2100Hlg` `A98` `ProPhoto` `Aces20651` `Acescg` `Acescct` `Acescc` `Rec601Line625` `Rec601Line525` `Rec709` `XvYcc` `Pal` `PalM` `Pal625` `Pal525` `Ntsc` `NtscSmpteC` `Ntsc525` `Secam` `Secam625`
[`XyzConfiguration`]: `D65` `D50`
[`YbrConfiguration`]: `Rec601` `Rec709` `Rec2020` `Jpeg`
[`CamConfiguration`]: `StandardRgb` `Hct`
[`DynamicRange`]: `Standard` `High`

[PUBLIC_TYPE_SCOPE]: spectral and geometric construction inputs

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------- | :------------ | :------------------------------------------------ |
|  [01]   | `Spd`          | class         | spectral power distribution at 0, 1, or 5 nm      |
|  [02]   | `Pigment`      | class         | single- or two-constant Kubelka-Munk reflectance  |
|  [03]   | `Chromaticity` | record        | `(X, Y)` with `.U`/`.V`/`.Uv`/`.Xy` and `.FromUv` |
|  [04]   | `WhitePoint`   | record        | `(X, Y, Z)` with `.Chromaticity`/`.Triplet`       |
|  [05]   | `Illuminant`   | class         | standard SPDs and `.GetWhitePoint(Observer)`      |
|  [06]   | `Observer`     | class         | CIE colour-matching functions                     |
|  [07]   | `Temperature`  | record        | `(Cct, Duv)` with `.IsValid`/`.IsHighAccuracy`    |

`Spd` publishes only `Spd.D65`; the other reference SPDs ride `Illuminant`, and `Spd.IsValid` gates the interval before construction resolves to XYZ.

[`Illuminant`]: `A` `C` `D50` `D55` `D65` `D75` `E` `F2` `F7` `F11`
[`Observer`]: `Degree2` `Degree10`

[PUBLIC_TYPE_SCOPE]: representation records and ICC types

Every `ColourSpace` accessor returns a representation record exposing named channels with `.Triplet` (`ColourTriplet`), `.Tuple`, `.ToArray()`, and `Deconstruct`; `Cam02`/`Cam16` add `.Model` and `.Ucs` from the static `Cam` owner.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                |
| :-----: | :--------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `ColourRepresentation` | abstract record | base of every space projection              |
|  [02]   | `ColourTriplet`        | record          | `(First, Second, Third, HueIndex?)` carrier |
|  [03]   | `Rgb`                  | record          | `.R`/`.G`/`.B`/`.Clipped`/`.Byte255`        |
|  [04]   | `Rgb255`               | record          | 8-bit `.R`/`.G`/`.B`/`.Clipped`/`.Hex`      |
|  [05]   | `Alpha`                | record          | `.A`/`.A255`/`.Hex`/`.Clipped`              |
|  [06]   | `Icc.Channels`         | record          | ICC device channels `(params double[])`     |
|  [07]   | `Icc.Profile`          | class           | parsed ICC profile with `.Header`/`.Tags`   |
|  [08]   | `Icc.Header`           | record          | profile header fields and `.Intent`         |
|  [09]   | `Icc.Tags`             | class           | profile tag table over `Icc.Tag`            |
|  [10]   | `Icc.Intent`           | enum            | rendering intent                            |

[`Icc.Intent`]: `Unspecified` `Perceptual` `RelativeColorimetric` `Saturation` `AbsoluteColorimetric`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction

Every construction route carries a `Configuration`-first overload selecting the working space; alpha defaults to `1.0` where the overload exposes it.

| [INDEX] | [SURFACE]                                          | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `Unicolour(ColourSpace, double, double, double)`   | ctor    | channel triple in a space          |
|  [02]   | `Unicolour(ColourSpace, (double, double, double))` | ctor    | tuple construction, alpha variant  |
|  [03]   | `Unicolour(ColourSpace, double)`                   | ctor    | single-channel grey                |
|  [04]   | `Unicolour(string)`                                | ctor    | hex intake                         |
|  [05]   | `Unicolour(Chromaticity, double)`                  | ctor    | white point from chromaticity      |
|  [06]   | `Unicolour(double, Locus, double)`                 | ctor    | blackbody or daylight CCT          |
|  [07]   | `Unicolour(Temperature, double)`                   | ctor    | CCT plus Duv temperature           |
|  [08]   | `Unicolour(Spd)`                                   | ctor    | spectral power distribution to XYZ |
|  [09]   | `Unicolour(Pigment[], double[])`                   | ctor    | Kubelka-Munk reflectance mix       |
|  [10]   | `Unicolour(Icc.Channels, double)`                  | ctor    | ICC device-channel intake          |

[ENTRYPOINT_SCOPE]: colour operations

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Difference(Unicolour, DeltaE) -> double`                | instance | perceptual distance under a metric      |
|  [02]   | `Contrast(Unicolour) -> double`                          | instance | WCAG contrast ratio                     |
|  [03]   | `Mix(Unicolour, ColourSpace, double, HueSpan, bool)`     | instance | one hue-aware interpolated colour       |
|  [04]   | `Palette(Unicolour, ColourSpace, int, HueSpan, bool)`    | instance | hue-aware colour sequence               |
|  [05]   | `Blend(Unicolour, BlendMode) -> Unicolour`               | instance | W3C backdrop compositing                |
|  [06]   | `Simulate(Cvd, double) -> Unicolour`                     | instance | colour-vision-deficiency simulation     |
|  [07]   | `MapToRgbGamut(GamutMap) -> Unicolour`                   | instance | map into the RGB gamut                  |
|  [08]   | `MapToPointerGamut() -> Unicolour`                       | instance | map into the Pointer real-surface gamut |
|  [09]   | `MapToMacAdamLimits() -> Unicolour`                      | instance | map into the MacAdam optimal limits     |
|  [10]   | `ConvertToConfiguration(Configuration) -> Unicolour`     | instance | rebase onto another working space       |
|  [11]   | `GetRepresentation(ColourSpace) -> ColourRepresentation` | instance | project to a runtime-selected space     |

- `Mix` returns one `Unicolour`; `Palette` returns `IEnumerable<Unicolour>`; both default `amount`/`hueSpan`/`premultiplyAlpha`.

[ENTRYPOINT_SCOPE]: projection accessors

Beyond the `ColourSpace` roster above, scalar and metadata accessors project derived facts.

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `.Rgb -> Rgb`                     | property | `.Byte255`/`.Clipped`/`.Hex` sRGB projection |
|  [02]   | `.RgbLinear -> RgbLinear`         | property | scene-linear light                           |
|  [03]   | `.Xyz -> Xyz`                     | property | tristimulus                                  |
|  [04]   | `.Icc -> Icc.Channels`            | property | ICC device channels via `Configuration.Icc`  |
|  [05]   | `.RelativeLuminance -> double`    | property | WCAG luminance from `Xyz.Y`                  |
|  [06]   | `.Chromaticity -> Chromaticity`   | property | white-point geometry                         |
|  [07]   | `.Temperature -> Temperature`     | property | CCT and Duv readout                          |
|  [08]   | `.DominantWavelength -> double`   | property | `Wxy.W`                                      |
|  [09]   | `.ExcitationPurity -> double`     | property | `Wxy.X`                                      |
|  [10]   | `.Hex -> string`                  | property | clipped 8-bit hex, `-` outside RGB gamut     |
|  [11]   | `.Configuration -> Configuration` | property | bound working-space policy                   |

[`gamut predicates`]: `IsInRgbGamut` `IsInPointerGamut` `IsInMacAdamLimits` `IsImaginary`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Unicolour` is immutable; every accessor lazily memoizes on first read, and every operation returns a fresh value carrying no shared state.
- `ColourSpace` selects the input space and `Configuration` the working space; the slots resolve independently, so a colour rebases through `ConvertToConfiguration` without re-authoring channels.
- `Difference` and `Contrast` re-project a mismatched operand onto the reference's `Configuration` before measuring, so a cross-configuration comparison is well-defined without a manual rebase.

[STACKING]:
- `Avalonia.Controls.ColorPicker`(`Rasm.AppUi/.api/api-avalonia-color.md`): `ConvertToConfiguration(Configuration.Default).Rgb.Byte255` crosses outbound to an Avalonia `Color`, and `new Unicolour(ColourSpace.Rgb255, r, g, b)` reads inbound, keeping every perceptual transform on this value.
- `Wacton.Unicolour.Datasets`(`Rasm.Materials/.api/api-unicolour-datasets.md`): supplies the `Pigment[]` reflectance tables the `Unicolour(Pigment[], double[])` ctor mixes and the reference `Unicolour` sets the `Difference(patch, DeltaE.Ciede2000)` metric measures against — the `Rasm.Materials` `finish#FINISH` mix runs under the `ArtistPaint.Configuration` (sRGB/D50) working space, never a hand-rolled K/S lerp, and `graph#MATERIAL_LIBRARY` `NearestChecker` ranks candidates against `Macbeth.All`.
- `Rasm.Materials`: `acquisition#ACQUISITION` grounds a measured spectral reflectance through `new Unicolour(Spd)` -> `.Xyz` -> scene-linear `Acescg`, gates the fit on `IsInRgbGamut`, and pairs the colour with the MathNet thin-QR fit residual on one `Provenance` receipt; `photometric#PHOTOMETRIC` resolves a blackbody/daylight CCT through `new Unicolour(cct, Locus, luminance)` -> `.RgbLinear`, projecting `DominantWavelength` (`Wxy.W`) and `ExcitationPurity` (`Wxy.X`) onto the `EmissionInput` receipt; `interchange#MATERIAL_WIRE` serializes `Unicolour`-derived scene-linear `BaseColor` triples through the Thinktecture-generated STJ/MessagePack codecs, never a `ToString("R")` string; and `graph#MATERIAL_LIBRARY` exposes `MapToSpectral`/`MapToPointer` as `MaterialLibrary` static projection wrappers over `MapToMacAdamLimits`/`MapToPointerGamut` — neither is a `Unicolour` member.
- within-lib: `Mix`/`Palette` over `Oklab`/`Oklch` under a `HueSpan` compose into `MapToRgbGamut(GamutMap.OklchChromaReduction)` for perceptually-even in-gamut ramps; `Spd` and `Pigment[]` reflectance intake feeds `DeltaE.Ciede2000` comparison; `IccConfiguration` profile slots drive `.Icc` device-channel projection.

[LOCAL_ADMISSION]:
- `GamutMap` is accepted only by `MapToRgbGamut`; `MapToPointerGamut` and `MapToMacAdamLimits` take no argument, and the four gamut predicates gate mapping.
- Reflectance mixing enters through `Unicolour(Pigment[], double[])` under a `Spd` illuminant, distinct from the raw `Unicolour(Spd)` spectral ctor.
- A colour value carries an explicit `Configuration` wherever the working space affects meaning.

[RAIL_LAW]:
- Package: `Wacton.Unicolour`
- Owns: the immutable colour value and every lazy conversion, difference, contrast, compositing, gamut map, `Cvd` simulation, ICC transform, and spectral or Kubelka-Munk construction over it
- Accept: an explicit `ColourSpace` and `Configuration`, a `DeltaE`, `BlendMode`, or `GamutMap` selector, gated by the gamut predicates
- Reject: hand-rolled colour-space conversion, delta-E, blend, or gamut math; display primaries treated as `ColourSpace` cases; per-consumer luminance or contrast re-derivation from framework colour types; `MapToSpectral`/`MapToPointer` treated as `Unicolour` members — they are `MaterialLibrary` projection wrappers
