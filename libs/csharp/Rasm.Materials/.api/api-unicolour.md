# [RASM_MATERIALS_API_UNICOLOUR]

`Wacton.Unicolour` supplies a single immutable `Unicolour` colour value with lazy conversion across 40 colour spaces, 12 perceptual difference metrics, 3 gamut-mapping strategies, 15 Porter-Duff/separable blend modes, even interpolation, spectral and Kubelka-Munk pigment construction, 8-mode colour-vision-deficiency simulation, ICC-profile channel transforms, and configurable working spaces. It is the host-neutral scene-linear/spectral colour owner the Materials `Appearance/*` engine (`surface#SPECTRAL_UPSAMPLE`, `finish#FINISH`, `photometric#PHOTOMETRIC`, `graph#MATERIAL_LIBRARY`, `acquisition#ACQUISITION`) composes for every colour transform — never a hand-rolled colour-space, delta-E, or gamut kernel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wacton.Unicolour`
- package: `Wacton.Unicolour`
- version: `7.0.0`
- license: MIT (`github.com/waacton/Unicolour`)
- assembly: `Wacton.Unicolour`
- namespace: `Wacton.Unicolour`, `Wacton.Unicolour.Icc`
- asset: runtime library (`netstandard2.0` single-TFM — the consumer `net10.0` binds the netstandard2.0 asset; pure-managed, ZERO transitive dependencies, so it is ALC-safe and loads into the in-Rhino default `AssemblyLoadContext` without a native-asset or transitive-version firebreak)
- rail: colour

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: colour value and configuration
- rail: colour

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]     | [CAPABILITY]                                       |
| :-----: | :----------------- | :----------------- | :------------------------------------------------- |
|  [01]   | `Unicolour`        | colour value       | lazy conversion, difference, mixing, gamut mapping |
|  [02]   | `Configuration`    | working-space root | groups RGB, XYZ, YBR, CAM, dynamic range, ICC      |
|  [03]   | `RgbConfiguration` | RGB working space  | primaries plus transfer functions                  |
|  [04]   | `XyzConfiguration` | XYZ working space  | illuminant, observer, chromatic adaptation         |
|  [05]   | `YbrConfiguration` | luma working space | Rec601 and Rec709 YCbCr policy                     |
|  [06]   | `CamConfiguration` | appearance space   | CAM02 and CAM16 viewing conditions                 |
|  [07]   | `DynamicRange`     | tone range         | SDR and HDR luminance levels                       |
|  [08]   | `IccConfiguration` | ICC profile policy | profile-backed channel transform                   |
|  [09]   | `Alpha`            | alpha value        | `record(double A)` with clip and hex projection    |
|  [10]   | `Temperature`      | correlated temp    | `record(double Cct, double Duv)` with validity     |
|  [11]   | `Chromaticity`     | chromaticity value | `record(double X, double Y)` with uv projection    |
|  [12]   | `WhitePoint`       | white point        | XYZ record with `.Chromaticity` projection         |

[PUBLIC_TYPE_SCOPE]: colour-space and metric discriminants
- rail: colour

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------ | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `ColourSpace` | space enum    | 40-member construction + conversion target (full member list in [02b])       |
|  [02]   | `DeltaE`      | metric enum   | 12-member perceptual difference selector                                     |
|  [03]   | `GamutMap`    | gamut enum    | `RgbClipping`, `OklchChromaReduction`, `WxyPurityReduction`                   |
|  [04]   | `HueSpan`     | hue enum      | `Shorter`, `Longer`, `Increasing`, `Decreasing`                              |
|  [05]   | `BlendMode`   | blend enum    | 16-member separable/non-separable blend selector for `Blend` (full in [05b]) |
|  [06]   | `Cvd`         | deficiency    | 8-member CVD selector (`Protan/Deuter/Tritan` -opia/-omaly, `BlueConeMonochromacy`, `Achromatopsia`) |
|  [07]   | `Locus`       | locus enum    | `Blackbody` (Planckian), `Daylight` (CIE D-series) temperature locus          |
|  [08]   | `Illuminant`  | illuminant    | 10 statics `A`/`C`/`D50`/`D55`/`D65`/`D75`/`E`/`F2`/`F7`/`F11`               |
|  [09]   | `Observer`    | observer      | `Degree2` (CIE 1931), `Degree10` (CIE 1964)                                  |

[PUBLIC_TYPE_SCOPE]: `ColourSpace` full 40-member roster ([02b])
- rail: colour

The complete construction/conversion target set; each member is also a lazy property on `Unicolour` (see [03] accessors). The Materials engine consumes `RgbLinear` (scene-linear pipeline source), `Rgb`/`Rgb255` (display), `Xyz`/`Xyy`/`Wxy` (tristimulus + dominant-λ readout), `Lab`/`Oklab`/`Oklch` (delta-E + gamut mapping), and `Acescg`-configured working spaces; the hue-cylinder, broadcast-luma, and notation families are admitted but not currently consumed.

| [INDEX] | [SPACE_FAMILY]      | [MEMBERS]                                                                 | [CAPABILITY]                                                            |
| :-----: | :------------------ | :----------------------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | display RGB         | `Rgb`, `Rgb255`, `RgbLinear`                                              | normalized sRGB, 0-255 input remapped on construction, scene-linear    |
|  [02]   | hue cylinder (sRGB) | `Hsb`, `Hsl`, `Hwb`, `Hsi`                                                | hue-saturation cylinders over the RGB working space                    |
|  [03]   | tristimulus         | `Xyz`, `Xyy`, `Wxy`                                                       | CIE XYZ, xyY chromaticity+luminance, dominant-λ/excitation-purity      |
|  [04]   | CIE perceptual      | `Lab`, `Lchab`, `Luv`, `Lchuv`, `Hsluv`, `Hpluv`                         | CIELAB/CIELUV uniform + cylindrical chroma-reduction spaces            |
|  [05]   | broadcast luma      | `Ypbpr`, `Ycbcr`, `Ycgco`, `Yuv`, `Yiq`, `Ydbdr`, `Tsl`                  | luma+chroma encoders (Rec601/Rec709 YCbCr policy via `YbrConfiguration`) |
|  [06]   | modern perceptual   | `Xyb`, `Lms`, `Ipt`, `Ictcp`, `Jzazbz`, `Jzczhz`                         | XYB, cone-response LMS, IPT, ICtCp (`DeltaE.Itp`), JzCzhz (`DeltaE.Z`)  |
|  [07]   | Oklab family        | `Oklab`, `Oklch`, `Okhsv`, `Okhsl`, `Okhwb`, `Oklrab`, `Oklrch`          | Oklab (`DeltaE.Ok`), cylindrical `Oklch` gamut-mapping + lr-toned variants |
|  [08]   | appearance/notation | `Cam02`, `Cam16`, `Hct`, `Munsell`                                       | CIECAM02 (`DeltaE.Cam02`), CAM16 (`DeltaE.Cam16`), Material HCT, Munsell |

[PUBLIC_TYPE_SCOPE]: `DeltaE` difference metrics
- rail: colour

| [INDEX] | [SYMBOL]            | [METRIC_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------ | :-------------- | :------------------------------- |
|  [01]   | `Cie76`             | CIELAB          | Euclidean CIELAB difference      |
|  [02]   | `Cie94`             | CIELAB          | graphic-arts weighted difference |
|  [03]   | `Cie94Textiles`     | CIELAB          | textile-weighted difference      |
|  [04]   | `Ciede2000`         | CIELAB          | CIEDE2000 appearance difference  |
|  [05]   | `CmcAcceptability`  | CMC             | CMC acceptability difference     |
|  [06]   | `CmcPerceptibility` | CMC             | CMC perceptibility difference    |
|  [07]   | `Itp`               | ICtCp           | HDR ICtCp difference             |
|  [08]   | `Z`                 | JzCzhz          | HDR JzCzhz difference            |
|  [09]   | `Hyab`              | hybrid          | HyAB hybrid distance             |
|  [10]   | `Ok`                | Oklab           | Euclidean Oklab difference       |
|  [11]   | `Cam02`             | appearance      | CIECAM02 colour-difference       |
|  [12]   | `Cam16`             | appearance      | CAM16-UCS colour-difference      |

[PUBLIC_TYPE_SCOPE]: `BlendMode` separable/non-separable members ([05b])
- rail: colour

The W3C compositing-and-blending modes `Blend(backdrop, BlendMode)` dispatches on; consumed for layered-coat/decal compositing where a coat tints a substrate by a named blend rather than a linear lerp.

| [INDEX] | [SYMBOL]                                                          | [BLEND_FAMILY] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `Normal`, `Multiply`, `Screen`, `Overlay`                       | separable      | source-over, darken/lighten products, overlay |
|  [02]   | `Darken`, `Lighten`, `ColourDodge`, `ColourBurn`                | separable      | min/max and dodge/burn channel blends         |
|  [03]   | `HardLight`, `SoftLight`, `Difference`, `Exclusion`             | separable      | contrast and difference channel blends        |
|  [04]   | `Hue`, `Saturation`, `Colour`, `Luminosity`                     | non-separable  | HSL-component-preserving blends               |

[PUBLIC_TYPE_SCOPE]: spectral and pigment construction inputs
- rail: colour

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE] | [CAPABILITY]                                |
| :-----: | :--------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Spd`                  | power dist     | spectral power distribution into XYZ        |
|  [02]   | `SpectralCoefficients` | spectral base  | base wavelength coefficient carrier         |
|  [03]   | `Pigment`              | reflectance    | Kubelka-Munk single/two-constant pigment    |
|  [04]   | `Icc.Channels`         | ICC channels   | `record(params double[] Values)` device set |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Unicolour` construction
- rail: colour

| [INDEX] | [SURFACE]                                                                         | [CALL_SHAPE]         | [CAPABILITY]                                                                                 |
| :-----: | :-------------------------------------------------------------------------------- | :------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `new Unicolour(ColourSpace, double, double, double, double alpha)`                | space ctor           | three components plus alpha                                                                  |
|  [02]   | `new Unicolour(ColourSpace, (double, double, double), double alpha)`              | space ctor           | tuple components plus alpha                                                                  |
|  [03]   | `new Unicolour(ColourSpace, (double, double, double, double))`                    | space ctor           | tuple components with alpha                                                                  |
|  [04]   | `new Unicolour(ColourSpace, double grey, double alpha)`                           | grey ctor            | single grey component                                                                        |
|  [05]   | `new Unicolour(string hex)`                                                       | hex ctor             | hex string parse                                                                             |
|  [06]   | `new Unicolour(string hex, double alphaOverride)`                                 | hex ctor             | hex parse with alpha override                                                                |
|  [07]   | `new Unicolour(Chromaticity, double luminance)`                                   | chromatic ctor       | chromaticity plus luminance                                                                  |
|  [08]   | `new Unicolour(double cct, Locus, double luminance)`                              | temperature ctor     | correlated temperature on a locus                                                            |
|  [09]   | `new Unicolour(Temperature, double luminance)`                                    | temperature ctor     | `Temperature` record plus luminance                                                          |
|  [10]   | `new Unicolour(Spd)`                                                              | spectral ctor        | spectral power distribution                                                                  |
|  [11]   | `new Unicolour(Pigment[], double[])`                                              | pigment ctor         | Kubelka-Munk weighted pigment mix                                                            |
|  [12]   | `new Unicolour(Icc.Channels, double alpha)`                                       | ICC ctor             | device channels through ICC profile                                                          |
|  [13]   | `new Unicolour(Configuration, ColourSpace, double, double, double, double alpha)` | config ctor          | construction with explicit working space                                                     |
|  [14]   | `new Unicolour(Configuration, Spd)`                                               | config-spectral ctor | spectral construction with explicit working space (internal `SpdToXyzTuple` → `Xyz.FromSpd`) |

[ENTRYPOINT_SCOPE]: conversion and inspection accessors
- rail: colour

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]       | [CAPABILITY]                                                                                                                                                                      |
| :-----: | :--------------------------------------------- | :----------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Rgb` .. `Munsell`                             | space property     | lazy conversion to any `ColourSpace` member, each returning its representation object (`Rgb`/`RgbLinear`/`Xyz`/...)                                                               |
|  [02]   | `<representation>.R` / `.G` / `.B`, `.Triplet` | component property | each colour-space representation exposes its scalar channels and a `ColourTriplet Triplet` (`.First`/`.Second`/`.Third`); `RgbLinear` is the scene-linear pipeline channel source |
|  [03]   | `Hex`                                          | text property      | clipped sRGB byte hex string                                                                                                                                                      |
|  [04]   | `Alpha`                                        | alpha property     | `Alpha` value carrier (`.A`, `.Clipped`, `.A255`, `.Hex`)                                                                                                                        |
|  [05]   | `Chromaticity`                                 | chromatic property | xy chromaticity of the colour                                                                                                                                                     |
|  [06]   | `Temperature`                                  | temp property      | correlated colour temperature readout (`Temperature.FromChromaticity`)                                                                                                           |
|  [07]   | `DominantWavelength`                           | spectral property  | dominant wavelength from `Wxy.W`                                                                                                                                                  |
|  [08]   | `ExcitationPurity`                             | spectral property  | excitation purity from `Wxy.X`                                                                                                                                                    |
|  [09]   | `RelativeLuminance`                            | luminance property | `Xyz.Y` relative luminance                                                                                                                                                        |
|  [10]   | `IsInRgbGamut`                                 | gamut check        | RGB gamut membership                                                                                                                                                              |
|  [11]   | `IsInPointerGamut`                             | gamut check        | Pointer real-surface gamut membership                                                                                                                                             |
|  [12]   | `IsInMacAdamLimits`                            | gamut check        | MacAdam optimal-colour-limit membership                                                                                                                                          |
|  [13]   | `IsImaginary`                                  | gamut check        | outside the spectral locus (`Configuration.Xyz.SpectralBoundary`)                                                                                                               |
|  [14]   | `Description`                                  | text property      | human colour-name description                                                                                                                                                    |
|  [15]   | `Configuration`                                | config property    | working space of the constructed colour                                                                                                                                           |

[ENTRYPOINT_SCOPE]: difference, mixing, gamut, and simulation operations
- rail: colour

| [INDEX] | [SURFACE]                                                                                   | [CALL_SHAPE]  | [CAPABILITY]                                                              |
| :-----: | :------------------------------------------------------------------------------------------ | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Difference(Unicolour reference, DeltaE deltaE)`                                            | metric call   | perceptual difference scalar; dispatches on the `DeltaE` member          |
|  [02]   | `Contrast(Unicolour other)`                                                                 | metric call   | WCAG contrast ratio                                                       |
|  [03]   | `Mix(Unicolour other, ColourSpace, double amount = 0.5, HueSpan = Shorter, bool premultiplyAlpha = true)` | mix call | interpolate in any colour space; returns a new `Unicolour`               |
|  [04]   | `Palette(Unicolour other, ColourSpace, int count, HueSpan = Shorter, bool premultiplyAlpha = true)` | palette call | even `IEnumerable<Unicolour>` interpolation sequence                      |
|  [05]   | `Blend(Unicolour backdrop, BlendMode blendMode)`                                           | blend call    | W3C separable/non-separable compositing blend; returns a new `Unicolour` |
|  [06]   | `MapToRgbGamut(GamutMap gamutMap = GamutMap.OklchChromaReduction)`                          | gamut call    | map colour into the RGB gamut (default is `OklchChromaReduction`)         |
|  [07]   | `MapToPointerGamut()`                                                                       | gamut call    | map colour into the Pointer real-surface gamut                           |
|  [08]   | `MapToMacAdamLimits()`                                                                      | gamut call    | map colour into the MacAdam optimal-colour limits                        |
|  [09]   | `Simulate(Cvd cvd, double severity = 1.0)`                                                  | simulation    | colour-vision-deficiency simulation; returns a new `Unicolour`           |
|  [10]   | `ConvertToConfiguration(Configuration config)`                                             | rebase call   | re-evaluates the colour under a new working space; returns a new `Unicolour` |
|  [11]   | `Description`                                                                               | text property | human colour-name description (`ColourDescription` over `Hsl`)           |
|  [12]   | `ToString()`                                                                                | text call     | source-space description string                                          |
|  [13]   | `Equals(Unicolour?)` / `IEquatable<Unicolour>`                                              | equality call | colour value equality                                                    |

[ENTRYPOINT_SCOPE]: configuration and value-type construction
- rail: colour

| [INDEX] | [SURFACE]                                                                                                                         | [CALL_SHAPE]      | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `Configuration.Default`                                                                                                           | static field      | sRGB, D65, Rec601, standard CAM baseline               |
|  [02]   | `new Configuration(RgbConfiguration?, XyzConfiguration?, YbrConfiguration?, CamConfiguration?, DynamicRange?, IccConfiguration?)` | ctor              | custom working space                                   |
|  [03]   | `RgbConfiguration.StandardRgb`, `DisplayP3`, `Rec2020`, `Rec2100Pq`, `Rec2100Hlg`, `A98`, `ProPhoto`, `Aces20651`, `Acescg`, `Acescct`, `Acescc`, `Rec709`, `XvYcc`, `Pal*`, `Ntsc*`, `Secam*` | static field | 24 named RGB working-space presets (display, ACES, broadcast); `Acescg` is the scene-linear pipeline space |
|  [04]   | `XyzConfiguration.D65` / `XyzConfiguration.D50`                                                                                   | static field      | named XYZ working-space presets                        |
|  [05]   | `new XyzConfiguration(Illuminant, Observer, string)` / `(WhitePoint, string)` / `(Illuminant, Observer, ChromaticAdaptation, string)` / `(WhitePoint, Observer, ChromaticAdaptation, string)` | ctor | 4 ctors: custom illuminant+observer, explicit white point, or pinned chromatic-adaptation transform |
|  [06]   | `DynamicRange.Standard` / `DynamicRange.High`                                                                                     | static field      | SDR and HDR luminance ranges                           |
|  [07]   | `new DynamicRange(double whiteLuminance, double maxLuminance, double minLuminance, double hlgWhiteLevel, string)`                 | ctor              | custom tone range                                      |
|  [08]   | `new Spd(int start, int interval, params double[])`                                                                               | ctor              | spectral coefficients, interval 0, 1, or 5             |
|  [09]   | `Spd.D65`                                                                                                                         | static property   | D65 reference illuminant SPD                           |
|  [9a]   | `Spd.IsValid`                                                                                                                     | instance property | `bool` — interval ∈ {0, 1, 5}; gates `ToSpd` admission |
|  [10]   | `new Pigment(int start, int interval, double[] r, string)`                                                                        | ctor              | single-constant Kubelka-Munk pigment                   |
|  [11]   | `new Pigment(int start, int interval, double[] k, double[] s, double? k1, double? k2, string)`                                    | ctor              | two-constant Kubelka-Munk pigment                      |
|  [12]   | `Chromaticity.FromUv(double u, double v)`                                                                                         | static factory    | chromaticity from CIE 1976 uv                          |

## [04]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `Wacton.Unicolour`; one immutable `Unicolour` value owns all conversions, differences, blends, and gamut maps — every operation returns a NEW value, the type carries no mutable state.
- conversion: every `ColourSpace` member is a lazy property on `Unicolour`, computed once and cached behind a `Get(ref field, evaluator)` memo; reading `colour.Oklab` materializes only the Oklab path.
- construction discriminant: `ColourSpace` selects the input space; `Configuration` selects the working space; the parallel construction inputs (`Spd`, `Pigment[]`, `Chromaticity`, `Temperature`, `Icc.Channels`, `cct`+`Locus`, `hex`) each have a config-explicit overload.
- difference: `Difference(reference, DeltaE)` dispatches on the `DeltaE` member; `Contrast` is WCAG-specific; `Blend(backdrop, BlendMode)` dispatches on the W3C blend member.
- gamut: `MapToRgbGamut(GamutMap)`, `MapToPointerGamut()`, and `MapToMacAdamLimits()` return new mapped `Unicolour` values (`MapToRgbGamut` defaults to `GamutMap.OklchChromaReduction`); the `IsInRgbGamut`/`IsInPointerGamut`/`IsInMacAdamLimits`/`IsImaginary` predicates gate BEFORE mapping. There is no `MapToSpectral`/`MapToPointer` member ON Unicolour — `graph#MATERIAL_LIBRARY` exposes `MapToSpectral`/`MapToPointer` as `MaterialLibrary` static projection wrappers (`MapToSpectral(Unicolour) => …MapToMacAdamLimits()`, `MapToPointer(Unicolour) => …MapToPointerGamut()`) that sit beside `SpectralAdmit`/`PointerAdmit`; a design page naming those wrappers composes the owner surface, while a direct call to THIS package uses `MapToMacAdamLimits`/`MapToPointerGamut`.
- working space: `Configuration` groups `RgbConfiguration`, `XyzConfiguration`, `YbrConfiguration`, `CamConfiguration`, `DynamicRange`, and `IccConfiguration`; `ConvertToConfiguration(config)` rebases an existing colour onto a new working space.
- HDR: `RgbConfiguration.Rec2100Pq` and `Rec2100Hlg` carry PQ and HLG transfer functions scaled by `DynamicRange.WhiteLuminance`.
- spectral: `Spd` accepts 0, 1, or 5 nm intervals (`Spd.IsValid` gates the interval); `new Unicolour(Configuration, Spd)` resolves SPD→XYZ through internal `SpdToXyzTuple` → `Xyz.FromSpd(spd, Observer, WhitePoint)`; `Pigment[]` plus weights drives Kubelka-Munk reflectance mixing.
- display primaries: `DisplayP3`, `Rec2020`, `A98`, `ProPhoto`, and the ACES presets are `RgbConfiguration` statics, not `ColourSpace` members; the YCbCr/YUV/YIQ broadcast-luma encoders ARE `ColourSpace` members governed by `YbrConfiguration` (Rec601/Rec709 policy).

[STACK]:
- Datasets seam: `finish#FINISH` mixes `Wacton.Unicolour.Datasets.ArtistPaint` `Pigment[]` reflectance values through `new Unicolour(pigments, weights)` under the `ArtistPaint.Configuration` (sRGB/D50) working space, then reads `.RgbLinear` to the scene-linear pipeline — the Datasets package supplies the measured pigment table, THIS owner runs the Kubelka-Munk mix; never a hand-rolled K/S lerp. `graph#MATERIAL_LIBRARY` `NearestChecker` measures a candidate against `Macbeth.All` patches through `Difference(patch, DeltaE.Ciede2000)`.
- MathNet seam: `acquisition#ACQUISITION` grounds a measured spectral reflectance through `new Unicolour(Spd)` → `.Xyz` → scene-linear `Acescg`, gates the fit on `IsInRgbGamut`, and pairs the colour with the `MathNet.Numerics` thin-QR GGX/Smith fit residual on one `Provenance` receipt — the colour rail and the numeric rail meet at the row, not in a fused kernel.
- UnitsNet seam: `photometric#PHOTOMETRIC` resolves a blackbody/daylight CCT through `new Unicolour(cct, Locus, luminance)` → `.RgbLinear` for scene-linear emission colour, projects `DominantWavelength` (`Wxy.W`) and `ExcitationPurity` (`Wxy.X`) onto the `EmissionInput` receipt, and admits the luminous/radiometric magnitude through the `UnitsNet` `MaterialUnits` boundary — the CCT and the SI-base magnitude are two coercions feeding one emission triple.
- wire seam: the Materials `interchange#MATERIAL_WIRE` projection serializes `Unicolour`-derived scene-linear `BaseColor` triples through the Thinktecture-generated STJ/MessagePack codecs (a `ColourTriplet`/`RgbLinear` triple becomes one wire shape the TS/Python peers decode), not a `CultureInfo.InvariantCulture` `ToString("R")` string.

[LOCAL_ADMISSION]:
- Material colour values carry a `Unicolour` with an explicit `Configuration` when the working space affects meaning.
- Conversion is a lazy accessor; consumers read the target-space property rather than re-deriving colour transforms.
- Perceptual difference and contrast are metric calls; the `DeltaE` member is part of the difference contract, not a hidden default.
- Gamut mapping returns a new value; gamut checks (`IsInRgbGamut`, `IsInPointerGamut`) gate output before mapping.
- Spectral and pigment construction stay at the colour boundary; downstream code consumes the resulting `Unicolour`.

[RAIL_LAW]:
- Package: `Wacton.Unicolour` 7.0.0 (MIT, `netstandard2.0`, zero-dependency, ALC-safe)
- Owns: the immutable colour value, lazy conversion across all 40 `ColourSpace` members, the 12-member `DeltaE` difference, WCAG contrast, the 16-member `BlendMode` blend, interpolation/palette, the 3 gamut maps, 8-mode CVD simulation, ICC channel transform, and spectral/Kubelka-Munk pigment construction
- Accept: an explicit `ColourSpace` and `Configuration` for construction, a `DeltaE` member for difference, a `BlendMode` for compositing, and a `GamutMap` for RGB gamut mapping; gamut checks gate before mapping
- Reject: hand-rolled colour-space conversion, delta-E, blend, or gamut math; display primaries treated as `ColourSpace` members; `MapToSpectral`/`MapToPointer` AS Unicolour members (they are `graph#MATERIAL_LIBRARY` `MaterialLibrary` projection wrappers over this package's `MapToMacAdamLimits`/`MapToPointerGamut`)
