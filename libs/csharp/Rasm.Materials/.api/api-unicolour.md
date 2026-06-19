# [RASM_MATERIALS_API_UNICOLOUR]

`Wacton.Unicolour` supplies a single `Unicolour` colour value with lazy conversion across 40 colour spaces, perceptual difference metrics, gamut mapping, interpolation, spectral and pigment construction, colour-vision-deficiency simulation, and configurable working spaces for material colour execution.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wacton.Unicolour`
- package: `Wacton.Unicolour`
- assembly: `Wacton.Unicolour`
- namespace: `Wacton.Unicolour`
- asset: runtime library
- rail: colour

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: colour value and configuration
- rail: colour

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]     | [CAPABILITY]                                       |
| :-----: | :----------------- | :----------------- | :------------------------------------------------- |
|   [1]   | `Unicolour`        | colour value       | lazy conversion, difference, mixing, gamut mapping |
|   [2]   | `Configuration`    | working-space root | groups RGB, XYZ, YBR, CAM, dynamic range, ICC      |
|   [3]   | `RgbConfiguration` | RGB working space  | primaries plus transfer functions                  |
|   [4]   | `XyzConfiguration` | XYZ working space  | illuminant, observer, chromatic adaptation         |
|   [5]   | `YbrConfiguration` | luma working space | Rec601 and Rec709 YCbCr policy                     |
|   [6]   | `CamConfiguration` | appearance space   | CAM02 and CAM16 viewing conditions                 |
|   [7]   | `DynamicRange`     | tone range         | SDR and HDR luminance levels                       |
|   [8]   | `IccConfiguration` | ICC profile policy | profile-backed channel transform                   |
|   [9]   | `Alpha`            | alpha value        | `record(double A)` with clip and hex projection    |
|  [10]   | `Temperature`      | correlated temp    | `record(double Cct, double Duv)` with validity     |
|  [11]   | `Chromaticity`     | chromaticity value | `record(double X, double Y)` with uv projection    |
|  [12]   | `WhitePoint`       | white point        | XYZ record with `.Chromaticity` projection         |

[PUBLIC_TYPE_SCOPE]: colour-space and metric discriminants
- rail: colour

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :------------ | :------------ | :---------------------------------------------------------- |
|   [1]   | `ColourSpace` | space enum    | 40-member construction and conversion target                |
|   [2]   | `DeltaE`      | metric enum   | 12-member perceptual difference selector                    |
|   [3]   | `GamutMap`    | gamut enum    | `RgbClipping`, `OklchChromaReduction`, `WxyPurityReduction` |
|   [4]   | `HueSpan`     | hue enum      | `Shorter`, `Longer`, `Increasing`, `Decreasing`             |
|   [5]   | `Cvd`         | deficiency    | 8-member colour-vision-deficiency selector                  |
|   [6]   | `Locus`       | locus enum    | `Blackbody`, `Daylight` temperature locus                   |
|   [7]   | `Illuminant`  | illuminant    | 10 statics `A`/`C`/`D50`-`D75`/`E`/`F2`/`F7`/`F11`          |
|   [8]   | `Observer`    | observer      | `Degree2` (CIE 1931), `Degree10` (CIE 1964)                 |

[PUBLIC_TYPE_SCOPE]: `ColourSpace` perceptual and appearance members
- rail: colour

| [INDEX] | [SYMBOL]    | [SPACE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------- | :------------- | :-------------------------------------------- |
|   [1]   | `Rgb`       | display RGB    | normalized sRGB working space                 |
|   [2]   | `Rgb255`    | display RGB    | 0-255 input remapped to `Rgb` on construction |
|   [3]   | `RgbLinear` | linear RGB     | linear-light working space                    |
|   [4]   | `Xyz`       | tristimulus    | CIE XYZ tristimulus                           |
|   [5]   | `Xyy`       | chromaticity   | CIE xyY chromaticity plus luminance           |
|   [6]   | `Wxy`       | dominant λ     | dominant wavelength and excitation purity     |
|   [7]   | `Lab`       | perceptual     | CIELAB for delta-E and gamut mapping          |
|   [8]   | `Lchab`     | perceptual     | cylindrical CIELAB chroma reduction           |
|   [9]   | `Luv`       | perceptual     | CIELUV uniform space                          |
|  [10]   | `Oklab`     | perceptual     | Oklab for `DeltaE.Ok` and interpolation       |
|  [11]   | `Oklch`     | perceptual     | cylindrical Oklab gamut-mapping space         |
|  [12]   | `Ictcp`     | HDR perceptual | ICtCp for `DeltaE.Itp`                        |
|  [13]   | `Jzczhz`    | HDR perceptual | JzCzhz for `DeltaE.Z`                         |
|  [14]   | `Lms`       | cone response  | chromatic adaptation space                    |
|  [15]   | `Cam02`     | appearance     | CIECAM02 for `DeltaE.Cam02`                   |
|  [16]   | `Cam16`     | appearance     | CAM16 for `DeltaE.Cam16`                      |
|  [17]   | `Hct`       | appearance     | Material hue-chroma-tone space                |
|  [18]   | `Munsell`   | notation       | Munsell colour notation space                 |

[PUBLIC_TYPE_SCOPE]: `DeltaE` difference metrics
- rail: colour

| [INDEX] | [SYMBOL]            | [METRIC_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------ | :-------------- | :------------------------------- |
|   [1]   | `Cie76`             | CIELAB          | Euclidean CIELAB difference      |
|   [2]   | `Cie94`             | CIELAB          | graphic-arts weighted difference |
|   [3]   | `Cie94Textiles`     | CIELAB          | textile-weighted difference      |
|   [4]   | `Ciede2000`         | CIELAB          | CIEDE2000 appearance difference  |
|   [5]   | `CmcAcceptability`  | CMC             | CMC acceptability difference     |
|   [6]   | `CmcPerceptibility` | CMC             | CMC perceptibility difference    |
|   [7]   | `Itp`               | ICtCp           | HDR ICtCp difference             |
|   [8]   | `Z`                 | JzCzhz          | HDR JzCzhz difference            |
|   [9]   | `Hyab`              | hybrid          | HyAB hybrid distance             |
|  [10]   | `Ok`                | Oklab           | Euclidean Oklab difference       |
|  [11]   | `Cam02`             | appearance      | CIECAM02 colour-difference       |
|  [12]   | `Cam16`             | appearance      | CAM16-UCS colour-difference      |

[PUBLIC_TYPE_SCOPE]: spectral and pigment construction inputs
- rail: colour

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE] | [CAPABILITY]                                |
| :-----: | :--------------------- | :------------- | :------------------------------------------ |
|   [1]   | `Spd`                  | power dist     | spectral power distribution into XYZ        |
|   [2]   | `SpectralCoefficients` | spectral base  | base wavelength coefficient carrier         |
|   [3]   | `Pigment`              | reflectance    | Kubelka-Munk single/two-constant pigment    |
|   [4]   | `Icc.Channels`         | ICC channels   | `record(params double[] Values)` device set |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Unicolour` construction
- rail: colour

| [INDEX] | [SURFACE]                                                                         | [CALL_SHAPE]         | [CAPABILITY]                                                                                 |
| :-----: | :-------------------------------------------------------------------------------- | :------------------- | :------------------------------------------------------------------------------------------- |
|   [1]   | `new Unicolour(ColourSpace, double, double, double, double alpha)`                | space ctor           | three components plus alpha                                                                  |
|   [2]   | `new Unicolour(ColourSpace, (double, double, double), double alpha)`              | space ctor           | tuple components plus alpha                                                                  |
|   [3]   | `new Unicolour(ColourSpace, (double, double, double, double))`                    | space ctor           | tuple components with alpha                                                                  |
|   [4]   | `new Unicolour(ColourSpace, double grey, double alpha)`                           | grey ctor            | single grey component                                                                        |
|   [5]   | `new Unicolour(string hex)`                                                       | hex ctor             | hex string parse                                                                             |
|   [6]   | `new Unicolour(string hex, double alphaOverride)`                                 | hex ctor             | hex parse with alpha override                                                                |
|   [7]   | `new Unicolour(Chromaticity, double luminance)`                                   | chromatic ctor       | chromaticity plus luminance                                                                  |
|   [8]   | `new Unicolour(double cct, Locus, double luminance)`                              | temperature ctor     | correlated temperature on a locus                                                            |
|   [9]   | `new Unicolour(Temperature, double luminance)`                                    | temperature ctor     | `Temperature` record plus luminance                                                          |
|  [10]   | `new Unicolour(Spd)`                                                              | spectral ctor        | spectral power distribution                                                                  |
|  [11]   | `new Unicolour(Pigment[], double[])`                                              | pigment ctor         | Kubelka-Munk weighted pigment mix                                                            |
|  [12]   | `new Unicolour(Icc.Channels, double alpha)`                                       | ICC ctor             | device channels through ICC profile                                                          |
|  [13]   | `new Unicolour(Configuration, ColourSpace, double, double, double, double alpha)` | config ctor          | construction with explicit working space                                                     |
|  [14]   | `new Unicolour(Configuration, Spd)`                                               | config-spectral ctor | spectral construction with explicit working space (internal `SpdToXyzTuple` → `Xyz.FromSpd`) |

[ENTRYPOINT_SCOPE]: conversion and inspection accessors
- rail: colour

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]       | [CAPABILITY]                                                                                                                                                                      |
| :-----: | :--------------------------------------------- | :----------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Rgb` .. `Munsell`                             | space property     | lazy conversion to any `ColourSpace` member, each returning its representation object (`Rgb`/`RgbLinear`/`Xyz`/...)                                                               |
|   [2]   | `<representation>.R` / `.G` / `.B`, `.Triplet` | component property | each colour-space representation exposes its scalar channels and a `ColourTriplet Triplet` (`.First`/`.Second`/`.Third`); `RgbLinear` is the scene-linear pipeline channel source |
|   [3]   | `Hex`                                          | text property      | clipped sRGB byte hex string                                                                                                                                                      |
|   [3]   | `Alpha`                                        | alpha property     | alpha value carrier                                                                                                                                                               |
|   [4]   | `Chromaticity`                                 | chromatic property | xy chromaticity of the colour                                                                                                                                                     |
|   [5]   | `Temperature`                                  | temp property      | correlated colour temperature readout                                                                                                                                             |
|   [6]   | `DominantWavelength`                           | spectral property  | dominant wavelength from `Wxy.W`                                                                                                                                                  |
|   [7]   | `ExcitationPurity`                             | spectral property  | excitation purity from `Wxy.X`                                                                                                                                                    |
|   [8]   | `RelativeLuminance`                            | luminance property | `Xyz.Y` relative luminance                                                                                                                                                        |
|   [9]   | `IsInRgbGamut`                                 | gamut check        | RGB gamut membership                                                                                                                                                              |
|  [10]   | `IsInPointerGamut`                             | gamut check        | Pointer real-surface gamut membership                                                                                                                                             |
|  [11]   | `IsInMacAdamLimits`                            | gamut check        | MacAdam spectral-limit membership                                                                                                                                                 |
|  [12]   | `IsImaginary`                                  | gamut check        | outside the spectral locus                                                                                                                                                        |
|  [13]   | `Configuration`                                | config property    | working space of the constructed colour                                                                                                                                           |

[ENTRYPOINT_SCOPE]: difference, mixing, gamut, and simulation operations
- rail: colour

| [INDEX] | [SURFACE]                                                                          | [CALL_SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------------- | :------------ | :---------------------------------- |
|   [1]   | `Difference(Unicolour reference, DeltaE)`                                          | metric call   | perceptual difference scalar        |
|   [2]   | `Contrast(Unicolour other)`                                                        | metric call   | WCAG contrast ratio                 |
|   [3]   | `Mix(Unicolour other, ColourSpace, double amount, HueSpan, bool premultiplyAlpha)` | mix call      | interpolate in any colour space     |
|   [4]   | `Palette(Unicolour other, ColourSpace, int count, HueSpan, bool premultiplyAlpha)` | palette call  | even interpolation sequence         |
|   [5]   | `MapToRgbGamut(GamutMap)`                                                          | gamut call    | map colour into the RGB gamut       |
|   [6]   | `MapToPointerGamut()`                                                              | gamut call    | map colour into the Pointer gamut   |
|   [7]   | `MapToMacAdamLimits()`                                                             | gamut call    | map colour into MacAdam limits      |
|   [8]   | `Simulate(Cvd, double severity)`                                                   | simulation    | colour-vision-deficiency simulation |
|   [9]   | `ToString`                                                                         | text call     | source-space description string     |
|  [10]   | `Equals` / `IEquatable<Unicolour>`                                                 | equality call | colour value equality               |

[ENTRYPOINT_SCOPE]: configuration and value-type construction
- rail: colour

| [INDEX] | [SURFACE]                                                                                                                         | [CALL_SHAPE]      | [CAPABILITY]                                           |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------- | :---------------- | :----------------------------------------------------- |
|   [1]   | `Configuration.Default`                                                                                                           | static field      | sRGB, D65, Rec601, standard CAM baseline               |
|   [2]   | `new Configuration(RgbConfiguration?, XyzConfiguration?, YbrConfiguration?, CamConfiguration?, DynamicRange?, IccConfiguration?)` | ctor              | custom working space                                   |
|   [3]   | `RgbConfiguration.StandardRgb` .. `Acescg`                                                                                        | static field      | named RGB working-space presets                        |
|   [4]   | `XyzConfiguration.D65` / `XyzConfiguration.D50`                                                                                   | static field      | named XYZ working-space presets                        |
|   [5]   | `new XyzConfiguration(Illuminant, Observer, string)`                                                                              | ctor              | custom illuminant and observer                         |
|   [6]   | `DynamicRange.Standard` / `DynamicRange.High`                                                                                     | static field      | SDR and HDR luminance ranges                           |
|   [7]   | `new DynamicRange(double whiteLuminance, double maxLuminance, double minLuminance, double hlgWhiteLevel, string)`                 | ctor              | custom tone range                                      |
|   [8]   | `new Spd(int start, int interval, params double[])`                                                                               | ctor              | spectral coefficients, interval 0, 1, or 5             |
|   [9]   | `Spd.D65`                                                                                                                         | static property   | D65 reference illuminant SPD                           |
|  [9a]   | `Spd.IsValid`                                                                                                                     | instance property | `bool` — interval ∈ {0, 1, 5}; gates `ToSpd` admission |
|  [10]   | `new Pigment(int start, int interval, double[] r, string)`                                                                        | ctor              | single-constant Kubelka-Munk pigment                   |
|  [11]   | `new Pigment(int start, int interval, double[] k, double[] s, double? k1, double? k2, string)`                                    | ctor              | two-constant Kubelka-Munk pigment                      |
|  [12]   | `Chromaticity.FromUv(double u, double v)`                                                                                         | static factory    | chromaticity from CIE 1976 uv                          |

## [4]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `Wacton.Unicolour`; one `Unicolour` value owns all conversions
- conversion: every `ColourSpace` member is a lazy property on `Unicolour`, computed once and cached
- construction discriminant: `ColourSpace` selects the input space; `Configuration` selects the working space
- difference: `Difference(reference, DeltaE)` dispatches on the `DeltaE` member; `Contrast` is WCAG-specific
- gamut: `MapToRgbGamut`, `MapToPointerGamut`, and `MapToMacAdamLimits` return new mapped `Unicolour` values; `GamutMap` selects the RGB mapping strategy
- working space: `Configuration` groups `RgbConfiguration`, `XyzConfiguration`, `YbrConfiguration`, `CamConfiguration`, `DynamicRange`, and `IccConfiguration`
- HDR: `RgbConfiguration.Rec2100Pq` and `Rec2100Hlg` carry PQ and HLG transfer functions scaled by `DynamicRange.WhiteLuminance`
- spectral: `Spd` accepts 0, 1, or 5 nm intervals (`Spd.IsValid` gates the interval); `new Unicolour(Configuration, Spd)` resolves SPD→XYZ through internal `SpdToXyzTuple` → `Xyz.FromSpd(spd, Observer, WhitePoint)`; `Pigment[]` plus weights drives Kubelka-Munk reflectance mixing
- display primaries: `DisplayP3`, `Rec2020`, `A98`, `ProPhoto`, and the ACES presets are `RgbConfiguration` statics, not `ColourSpace` members

[LOCAL_ADMISSION]:
- Material colour values carry a `Unicolour` with an explicit `Configuration` when the working space affects meaning.
- Conversion is a lazy accessor; consumers read the target-space property rather than re-deriving colour transforms.
- Perceptual difference and contrast are metric calls; the `DeltaE` member is part of the difference contract, not a hidden default.
- Gamut mapping returns a new value; gamut checks (`IsInRgbGamut`, `IsInPointerGamut`) gate output before mapping.
- Spectral and pigment construction stay at the colour boundary; downstream code consumes the resulting `Unicolour`.

[RAIL_LAW]:
- Package: `Wacton.Unicolour`
- Owns: colour value, conversion, difference, gamut mapping, interpolation, and spectral construction
- Accept: an explicit `ColourSpace` and `Configuration` for construction and a `DeltaE` member for difference
- Reject: hand-rolled colour-space conversion, delta-E, or gamut math; display primaries treated as `ColourSpace` members
