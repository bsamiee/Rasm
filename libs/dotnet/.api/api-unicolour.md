# [RASM_API_UNICOLOUR]

`Wacton.Unicolour` owns an immutable colour value that admits any `ColourSpace` and lazily projects every other, with `Configuration` binding working-space policy across its slots. `DeltaE`, `BlendMode`, `Cvd`, and `GamutMap` extend the value into perceptual difference, compositing, vision-deficiency simulation, and gamut bounding; `Spd` and `Pigment` intake resolves measured spectral power and Kubelka-Munk reflectance to XYZ.

## [01]-[PUBLIC_TYPES]

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

`RgbConfiguration` and the custom `Configuration` slots each carry a constructor taking `Chromaticity` primaries, a `WhitePoint`, `FromLinear`/`ToLinear` transfer delegates, and a name, admitting an unlisted working space. Every preset PUBLISHES that geometry back — `ChromaticityR`/`ChromaticityG`/`ChromaticityB`/`WhitePoint` and the `Func<double, DynamicRange, double>` `FromLinear`/`ToLinear` pair are public, so a consumer reads a space's primaries and transfer off the row instead of transcribing a coordinate table; the derived `RgbToXyzMatrix` stays `internal`, reached through `Xyz`/`ConvertToConfiguration`.

`Configuration(RgbConfiguration?, XyzConfiguration?, YbrConfiguration?, CamConfiguration?, DynamicRange?, IccConfiguration?)` seats `camConfig` as the fourth named slot defaulting to `CamConfiguration.StandardRgb`, so every configuration carries a viewing condition implicitly. `CamConfiguration(WhitePoint whitePoint, double adaptingLuminance, double backgroundLuminance, Surround surround, string name = "(unnamed)")` publishes `WhitePoint`/`AdaptingLuminance`/`BackgroundLuminance`/`Surround`/`Name` back; `CamConfiguration.LuxToLuminance(double)` is `internal` (body `lux / Math.PI / 5.0`), so a consumer authors the illuminance conversion itself.

[`RgbConfiguration`]: `StandardRgb` `DisplayP3` `Rec2020` `Rec2100Pq` `Rec2100Hlg` `A98` `ProPhoto` `Aces20651` `Acescg` `Acescct` `Acescc` `Rec601Line625` `Rec601Line525` `Rec709` `XvYcc` `Pal` `PalM` `Pal625` `Pal525` `Ntsc` `NtscSmpteC` `Ntsc525` `Secam` `Secam625`
[`XyzConfiguration`]: `D65` `D50`
[`YbrConfiguration`]: `Rec601` `Rec709` `Rec2020` `Jpeg`
[`CamConfiguration`]: `StandardRgb` `Hct`
[`Surround`]: `Dark` `Dim` `Average`
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
|  [04]   | `RgbLinear`            | record          | `.R`/`.G`/`.B` scene-linear light           |
|  [05]   | `Rgb255`               | record          | 8-bit `.R`/`.G`/`.B`/`.Clipped`/`.Hex`      |
|  [06]   | `Alpha`                | record          | `.A`/`.A255`/`.Hex`/`.Clipped`              |
|  [07]   | `Icc.Channels`         | record          | ICC device channels `(params double[])`     |
|  [08]   | `Icc.Profile`          | class           | parsed ICC profile with `.Header`/`.Tags`   |
|  [09]   | `Icc.Header`           | record          | profile header fields and `.Intent`         |
|  [10]   | `Icc.Tags`             | class           | profile tag table over `Icc.Tag`            |
|  [11]   | `Icc.Intent`           | enum            | rendering intent                            |

[`Icc.Intent`]: `Unspecified` `Perceptual` `RelativeColorimetric` `Saturation` `AbsoluteColorimetric`

## [02]-[ENTRYPOINTS]

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
|  [12]   | `.Alpha -> Alpha`                 | property | coverage as constructed or mixed             |
|  [13]   | `.Description -> string`          | property | space-joined plain-language colour name      |

- `.Description` reads off `Hsl` and answers a phrase, never a catalogue name — a swatch label a person reads, not an identity a lookup keys on; `.Alpha` is the one coverage read, and `Mix`/`Palette` land their interpolated coverage there rather than on a second channel.

[`gamut predicates`]: `IsInRgbGamut` `IsInPointerGamut` `IsInMacAdamLimits` `IsImaginary`

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Unicolour` is immutable; every accessor lazily memoizes on first read, and every operation returns a fresh value carrying no shared state.
- `ColourSpace` selects the input space and `Configuration` the working space; the slots resolve independently, so a colour rebases through `ConvertToConfiguration` without re-authoring channels.
- `.Rgb` is the COMPANDED display-referred projection — its evaluator applies the working `RgbConfiguration` transfer over `.RgbLinear` at the bound `DynamicRange` — and `.RgbLinear` the scene-linear one; only `Rgb` carries `.Clipped`/`.Byte255`/`.Hex`, so an 8-bit read is display-referred by construction and a linear-light read names `RgbLinear`.
- `Difference` and `Contrast` re-project a mismatched operand onto the reference's `Configuration` before measuring, so a cross-configuration comparison is well-defined without a manual rebase.
- A `Configuration` INSTANCE is the working-space identity: `ConvertToConfiguration` short-circuits on `config == Configuration` and the class overrides no value equality, so two instances of one space adapt through XYZ on every crossing and hold separate lazy-conversion caches — one instance per space, held by one owner, is the composing law.
- `Configuration`'s `dynamicRange` argument defaults to `DynamicRange.High` (203-nit white, 1000-nit max), so an SDR working space states `DynamicRange.Standard` explicitly or its `Rec2100Pq`/`Rec2100Hlg` transfers encode at the HDR white level.
- `GamutMap` is a strategy over the RGB volume alone; the Pointer real-surface and MacAdam optimal-limit volumes take no argument, so the three domains pair with `IsInRgbGamut`, `IsInPointerGamut`, and `IsInMacAdamLimits`/`IsImaginary` respectively — one predicate and one projection per domain.

[STACKING]:
- `Avalonia.Controls.ColorPicker`(`Rasm.AppUi/.api/api-avalonia-color.md`): `ConvertToConfiguration(Configuration.Default).Rgb.Byte255` crosses outbound to an Avalonia `Color`, and `new Unicolour(ColourSpace.Rgb255, r, g, b)` reads inbound, keeping every perceptual transform on this value.
- `Rasm` kernel: `Numerics/atoms#SCALAR_FLOOR` `RgbProfile` is the corpus' ONE `Configuration` mint — a row per working space carrying its explicit `DynamicRange` and publishing the preset's chromaticity geometry — and `GamutPolicy` pairs each domain's predicate with its projection, so `PerceptualColor` and every consuming package name a row rather than calling `MapToRgbGamut`, `IsInPointerGamut`, or `MapToMacAdamLimits` directly. The cam-bearing mint publishes — `RgbProfile.Viewed(CamConfiguration)` returns the memoized cam-bearing `Configuration` and `DeltaMetric.Measure(Unicolour, Unicolour)` the condition-correct distance — so a direct `Unicolour` composer states a viewing condition through the kernel's one mint instead of constructing a peer `Configuration`; `DeltaMetric.Appearance.Working` stays `internal` as that mint's own memo. `RgbTransfer` rows pair with `GamutPolicy` on egress, `Encoded` naming the `.Rgb` companded read and `Linear` the `.RgbLinear` scene-linear one, so the profile leg reads one representation per row after the shared domain bound.
- `Wacton.Unicolour.Datasets`(`Rasm.Materials/.api/api-unicolour-datasets.md`): supplies the `Pigment[]` reflectance tables the `Unicolour(Pigment[], double[])` ctor mixes and the reference `Unicolour` sets the `Difference(patch, DeltaE.Ciede2000)` metric measures against — the `Rasm.Materials` `finish#FINISH` mix runs under the `ArtistPaint.Configuration` (sRGB/D50) working space, never a hand-rolled K/S lerp, and `graph#MATERIAL_LIBRARY` `NearestChecker` ranks candidates against `Macbeth.All`.
- `Rasm.Materials`: `acquisition#ACQUISITION` grounds a measured spectral reflectance through `new Unicolour(Spd)` -> `.Xyz` -> scene-linear `Acescg`, gates the fit on `IsInRgbGamut`, and pairs the colour with the MathNet thin-QR fit residual on the acquisition result; `photometric#PHOTOMETRIC` resolves a blackbody/daylight CCT through `new Unicolour(cct, Locus, luminance)` -> `.RgbLinear`, projecting `DominantWavelength` (`Wxy.W`) and `ExcitationPurity` (`Wxy.X`) onto `EmissionInput`; `interchange#MATERIAL_WIRE` serializes `Unicolour`-derived scene-linear `BaseColor` triples through the Thinktecture-generated STJ/MessagePack codecs, never a `ToString("R")` string; and `graph#MATERIAL_LIBRARY` `PointerAdmit`/`SpectralAdmit` rail the kernel `GamutPolicy.Pointer`/`MacAdam` containment onto `MaterialFault.Gamut` with `IsImaginary` the spectral pre-test, the recovery being the same row's projection.
- within-lib: `Mix`/`Palette` over `Oklab`/`Oklch` under a `HueSpan` compose into `MapToRgbGamut(GamutMap.OklchChromaReduction)` for perceptually-even in-gamut ramps; `Spd` and `Pigment[]` reflectance intake feeds `DeltaE.Ciede2000` comparison; `IccConfiguration` profile slots drive `.Icc` device-channel projection.

[LOCAL_ADMISSION]:
- `GamutMap` is accepted only by `MapToRgbGamut`; `MapToPointerGamut` and `MapToMacAdamLimits` take no argument, and the four gamut predicates gate mapping — the kernel `GamutPolicy` row is the estate's admitted spelling for the pair.
- `Mix` and `Palette` premultiply alpha by default and carry the interpolated coverage on the result's `Alpha.A`, so alpha never interpolates in a second pass.
- Reflectance mixing enters through `Unicolour(Configuration, Pigment[], double[])` — NO illuminant slot exists; the working space is the `Configuration`'s own `XyzConfiguration` — distinct from the raw `Unicolour(Spd)` spectral ctor.
- `XyzConfiguration` ships four public ctors; `(Illuminant, Observer, string name = "(unnamed)")` derives the white point FROM the observer, so an observer-axis mint never reuses a sibling observer's white point.
- `Configuration.DynamicRange`/`.Rgb`/`.Ybr`/`.Cam`/`.Icc` are public reads — an observer-axis `Configuration` mint derives every non-observer slot off its source row rather than re-spelling literals.
- Colour values carry an explicit `Configuration` wherever the working space affects meaning.
