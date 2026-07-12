# [RASM_MATERIALS_API_UNICOLOUR]

Full surface and stacking: `libs/csharp/.api/api-unicolour.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[COLOUR_TOPOLOGY]:
- namespace: `Wacton.Unicolour`; one immutable `Unicolour` value owns all conversions, differences, blends, and gamut maps — every operation returns a NEW value, the type carries no mutable state.
- conversion: every `ColourSpace` member is a lazy property on `Unicolour`, computed once and cached behind a `Get(ref field, evaluator)` memo; reading `colour.Oklab` materializes only the Oklab path.
- construction discriminant: `ColourSpace` selects the input space; `Configuration` selects the working space; the parallel construction inputs (`Spd`, `Pigment[]`, `Chromaticity`, `Temperature`, `Icc.Channels`, `cct`+`Locus`, `hex`) each have a config-explicit overload.
- difference: `Difference(reference, DeltaE)` dispatches on the `DeltaE` member; `Contrast` is WCAG-specific; `Blend(backdrop, BlendMode)` dispatches on the W3C blend member.
- gamut: `MapToRgbGamut(GamutMap)`, `MapToPointerGamut()`, and `MapToMacAdamLimits()` return new mapped `Unicolour` values; `MapToRgbGamut` defaults to `GamutMap.OklchChromaReduction`. The `IsInRgbGamut`/`IsInPointerGamut`/`IsInMacAdamLimits`/`IsImaginary` predicates gate mapping.
- projection boundary: `Unicolour` has no `MapToSpectral` or `MapToPointer` member. `graph#MATERIAL_LIBRARY` exposes those names as `MaterialLibrary` static projection wrappers (`MapToSpectral(Unicolour) => …MapToMacAdamLimits()`, `MapToPointer(Unicolour) => …MapToPointerGamut()`) beside `SpectralAdmit`/`PointerAdmit`; a design page naming the wrappers composes the owner surface, while a direct package call uses `MapToMacAdamLimits`/`MapToPointerGamut`.
- working space: `Configuration` groups `RgbConfiguration`, `XyzConfiguration`, `YbrConfiguration`, `CamConfiguration`, `DynamicRange`, and `IccConfiguration`; `ConvertToConfiguration(config)` rebases an existing colour onto a new working space.
- HDR: `RgbConfiguration.Rec2100Pq` and `Rec2100Hlg` carry PQ and HLG transfer functions scaled by `DynamicRange.WhiteLuminance`.
- spectral: `Spd` accepts 0, 1, or 5 nm intervals (`Spd.IsValid` gates the interval); `new Unicolour(Configuration, Spd)` resolves SPD to XYZ through the internal SPD-to-XYZ path; `Pigment[]` plus weights drives Kubelka-Munk reflectance mixing.
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
- Package: `Wacton.Unicolour` (MIT, `netstandard2.0`, zero-dependency, ALC-safe)
- Owns: the immutable colour value, lazy conversion across all 40 `ColourSpace` members, the 12-member `DeltaE` difference, WCAG contrast, the 16-member `BlendMode` blend, interpolation/palette, the 3 gamut maps, 8-mode CVD simulation, ICC channel transform, and spectral/Kubelka-Munk pigment construction
- Accept: an explicit `ColourSpace` and `Configuration` for construction, a `DeltaE` member for difference, a `BlendMode` for compositing, and a `GamutMap` for RGB gamut mapping; gamut checks gate before mapping
- Reject: hand-rolled colour-space conversion, delta-E, blend, or gamut math; display primaries treated as `ColourSpace` members; `MapToSpectral`/`MapToPointer` AS Unicolour members (they are `graph#MATERIAL_LIBRARY` `MaterialLibrary` projection wrappers over this package's `MapToMacAdamLimits`/`MapToPointerGamut`)
