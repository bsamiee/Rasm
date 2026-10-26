using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filters.Detail;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Optics;
using Rasm.Imaging.Filters.Stylize;
using Rasm.Imaging.Filters.Warp;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("7465f313-020e-49ce-acd3-58ade34c750a")]
[CustomPostEffect(PostEffectType.Early, "Guided Denoise", executeWhileRenderingOption: PostEffectExecuteWhileRenderingOptions.UseExecutionControl)]
public sealed class GuidedDenoise : JobEffect<GuidedDenoise, DenoiseState, DenoiseParameter, Denoiser>, IEffectDefinition<DenoiseState, DenoiseParameter> {
    public static EffectRow<DenoiseState, DenoiseParameter> Row { get; } = new() { Amount = None, Replaces = BuiltinEffect.Denoise };
}

[Guid("5bfb53d0-6c96-4fc2-bf47-fd7662e4182b")]
[CustomPostEffect(PostEffectType.Early, "Starfield")]
public sealed class NightSky : StageEffect<NightSky, Starfield, StarfieldParameter>;

[Guid("a65107ea-d4f1-4b61-ac09-b74d67ba8935")]
[CustomPostEffect(PostEffectType.Early, "Atmosphere")]
public sealed class Atmosphere : StageEffect<Atmosphere, Fog, FogParameter>, IEffectDefinition<Fog, FogParameter> {
    public static EffectRow<Fog, FogParameter> Row { get; } = new() { Looks = [("Haze", Fog.Haze)] };
}

[Guid("77743df8-86ad-48bc-b60a-a08a5dc51818")]
[CustomPostEffect(PostEffectType.Early, "Noise Texture")]
public sealed class NoiseLayer : StageEffect<NoiseLayer, NoiseTexture, NoiseTextureParameter>, IEffectDefinition<NoiseTexture, NoiseTextureParameter> {
    public static EffectRow<NoiseTexture, NoiseTextureParameter> Row { get; } = new() {
        Looks = [
            ("Turbulent Noise", NoiseTexture.TurbulentNoise), ("Block Noise", NoiseTexture.BlockNoise), ("Diamonds", NoiseTexture.Diamonds),
            ("Voronoi Noise", NoiseTexture.VoronoiNoise), ("Sparkle", NoiseTexture.Sparkle), ("Voronoi Cells", NoiseTexture.VoronoiCells),
            ("Waves", NoiseTexture.Waves), ("Light Leak", NoiseTexture.LightLeak), ("Flow", NoiseTexture.Flow),
            ("Vortex", NoiseTexture.Vortex), ("Smoke", NoiseTexture.Smoke), ("Mist", NoiseTexture.Mist),
        ],
    };
}

[Guid("36ac0f92-c557-4d37-a728-cb2249510efe")]
[CustomPostEffect(PostEffectType.Early, "Fire")]
public sealed class Flames : StageEffect<Flames, NoiseTexture, NoiseTextureParameter>, IEffectDefinition<NoiseTexture, NoiseTextureParameter> {
    public static EffectRow<NoiseTexture, NoiseTextureParameter> Row { get; } = new() { Default = NoiseTexture.Fire };
}

[Guid("31555b4a-ff97-4387-946f-e7e9b0a131e8")]
[CustomPostEffect(PostEffectType.Early, "Particles")]
public sealed class Particles : StageEffect<Particles, ParticleField, ParticleFieldParameter>, IEffectDefinition<ParticleField, ParticleFieldParameter> {
    public static EffectRow<ParticleField, ParticleFieldParameter> Row { get; } = new() {
        Looks = [
            ("Rain", ParticleField.Rain), ("Snow", ParticleField.Snow), ("Sparks", ParticleField.Sparks),
            ("Cell Rain", ParticleField.CellRain), ("Starfield", ParticleField.Starfield),
        ],
    };
}

[Guid("60ad6377-8592-43dd-acd2-cc3ef03f7b5f")]
[CustomPostEffect(PostEffectType.Early, "Lightning")]
public sealed class LightningBolt : StageEffect<LightningBolt, Lightning, LightningParameter>;

[Guid("ff29c5af-33af-4817-90a1-910fdbb21909")]
[CustomPostEffect(PostEffectType.Early, "Light Rays")]
public sealed class SunBeams : StageEffect<SunBeams, LightRays, LightRaysParameter>;

[Guid("05985670-b6cb-40da-9e8e-0f6ac2d30a70")]
[CustomPostEffect(PostEffectType.Early, "Lens Flare")]
public sealed class SourceFlare : StageEffect<SourceFlare, LensFlare, LensFlareParameter>, IEffectDefinition<LensFlare, LensFlareParameter> {
    public static EffectRow<LensFlare, LensFlareParameter> Row { get; } = new() {
        Looks = [("Nacho", LensFlare.NachoLens), ("Ring", LensFlare.RingLens), ("Hex", LensFlare.HexLens), ("Round", LensFlare.RoundLens)],
    };
}

[Guid("3213d575-58cc-4a78-b13a-d130c437f053")]
[CustomPostEffect(PostEffectType.Early, "Exposure")]
public sealed class SceneExposure : StageEffect<SceneExposure, ExposureState, ExposureParameter>;

[Guid("c32e1536-d129-432d-b5ae-6da715024b02")]
[CustomPostEffect(PostEffectType.Early, "White Balance")]
public sealed class WhiteBalanceCorrection : StageEffect<WhiteBalanceCorrection, WhiteBalanceState, WhiteBalanceParameter>, IEffectDefinition<WhiteBalanceState, WhiteBalanceParameter> {
    public static EffectRow<WhiteBalanceState, WhiteBalanceParameter> Row { get; } = new() {
        Looks = toSeq(StandardIlluminant.Items).Map<(string Name, EffectLook<WhiteBalanceState, WhiteBalanceParameter> Look)>(static illuminant =>
            ($"Illuminant {illuminant.Key}", new WhiteBalanceState(illuminant.Temperature, illuminant.Tint))),
    };
}

[Guid("7400690e-f17b-4539-a88a-38b5bfa154fd")]
[CustomPostEffect(PostEffectType.Early, "Lens Blur")]
public sealed class LensBlur : StageEffect<LensBlur, Defocus, DefocusParameter>;

[Guid("944522ca-dde2-4e3e-81b0-82565728951b")]
[CustomPostEffect(PostEffectType.Early, "Lens Distortion")]
public sealed class RadialDistortion : StageEffect<RadialDistortion, LensDistortion, LensDistortionParameter>;

[Guid("106bf35b-4759-4e08-a0a3-38f02c7be061")]
[CustomPostEffect(PostEffectType.Early, "Axial Fringing")]
public sealed class AxialFringing : StageEffect<AxialFringing, AxialAberration, AxialAberrationParameter>;

[Guid("b8bf3b1d-0d30-4019-b5d5-417dba7936db")]
[CustomPostEffect(PostEffectType.Early, "Lens Bloom")]
public sealed class LensBloom : StageEffect<LensBloom, Bloom, BloomParameter>;

[Guid("134e641a-47aa-4f42-9084-8040f3568e94")]
[CustomPostEffect(PostEffectType.Early, "Veiling Flare")]
public sealed class VeilingFlare : StageEffect<VeilingFlare, VeilingGlare, VeilingGlareParameter>;

[Guid("166dcfdb-9388-4cde-ac2b-2c5fe76e4604")]
[CustomPostEffect(PostEffectType.Early, "Aperture Diffraction")]
public sealed class ApertureDiffraction : StageEffect<ApertureDiffraction, StarGlare, StarGlareParameter>;

[Guid("d3df8c60-5cc3-4c70-bbf7-2996bd2d7fd0")]
[CustomPostEffect(PostEffectType.Early, "Lens Reflections")]
public sealed class LensReflections : StageEffect<LensReflections, Ghosts, GhostsParameter>;

[Guid("f4afa240-2e97-469d-8f23-ce02aab2f499")]
[CustomPostEffect(PostEffectType.Early, "Natural Vignetting")]
public sealed class NaturalVignetting : StageEffect<NaturalVignetting, Vignette, VignetteParameter>;

[Guid("6a47cbbf-f6a2-4b53-9b6d-6ccd6c5ede48")]
[CustomPostEffect(PostEffectType.Early, "Halation")]
public sealed class FilmHalation : StageEffect<FilmHalation, Halation, HalationParameter>;

[Guid("f7d32275-fcbc-4c8a-ad36-8826d09678a5")]
[CustomPostEffect(PostEffectType.Early, "Reprojection")]
public sealed class ViewReprojection : StageEffect<ViewReprojection, Panorama, PanoramaParameter>;

[Guid("30219496-aff4-482e-91cc-8059f0a1532e")]
[CustomPostEffect(PostEffectType.Early, "Twirl")]
public sealed class Swirl : StageEffect<Swirl, Twirl, TwirlParameter>;

[Guid("44c0cd28-8f52-44c7-b508-946cf6cd29cc")]
[CustomPostEffect(PostEffectType.Early, "Spherize")]
public sealed class Spherize : StageEffect<Spherize, Bulge, BulgeParameter>;

[Guid("651cc9ac-db9d-48b7-935b-142916a94f16")]
[CustomPostEffect(PostEffectType.Early, "Wave")]
public sealed class WaveWarp : StageEffect<WaveWarp, Wave, WaveParameter>;

[Guid("3017b4d2-68e3-46f7-984e-daafecefe685")]
[CustomPostEffect(PostEffectType.Early, "Turbulent Displace")]
public sealed class TurbulentDisplace : StageEffect<TurbulentDisplace, Turbulence, TurbulenceParameter>;

[Guid("3833180c-a491-44a2-961d-a875bc57874b")]
[CustomPostEffect(PostEffectType.Early, "Displace")]
public sealed class Displace : StageEffect<Displace, MapDisplacement, MapDisplacementParameter>;

[Guid("52c0caf8-262f-49d1-90d0-0bac92cedbc8")]
[CustomPostEffect(PostEffectType.Early, "Glass")]
public sealed class Glass : StageEffect<Glass, Refraction, RefractionParameter>;

[Guid("d21a93e6-91f0-497e-b9f9-21f030037564")]
[CustomPostEffect(PostEffectType.Early, "Kaleidoscope")]
public sealed class Kaleida : StageEffect<Kaleida, Kaleidoscope, KaleidoscopeParameter>;

[Guid("1f0dfd6b-db2d-41e6-8b94-0bc199d18cab")]
[CustomPostEffect(PostEffectType.Early, "Polar Coordinates")]
public sealed class PolarWarp : StageEffect<PolarWarp, PolarCoordinates, PolarCoordinatesParameter>;

[Guid("2840e7bd-5137-4e50-99c2-82efcea276b7")]
[CustomPostEffect(PostEffectType.Early, "Transform")]
public sealed class ImageTransform : StageEffect<ImageTransform, AffineTransform, AffineTransformParameter>;

[Guid("106fb69d-480f-430a-95f0-cc9ee4bea176")]
[CustomPostEffect(PostEffectType.Early, "Corner Pin")]
public sealed class CornerPin2D : StageEffect<CornerPin2D, CornerPin, CornerPinParameter>;

[Guid("4c2a5565-2369-462c-91cf-2b52ce565040")]
[CustomPostEffect(PostEffectType.Early, "Camera Shake")]
public sealed class CameraShake : StageEffect<CameraShake, ShakeWarp, ShakeWarpParameter>;

[Guid("570b0a76-a94d-4749-b0a7-b9ec2e1fc36f")]
[CustomPostEffect(PostEffectType.Early, "Channel Mixer")]
public sealed class ChannelMixer : StageEffect<ChannelMixer, MixerState, MixerParameter>;

[Guid("556b0475-308d-4a3d-b366-f541d6666a00")]
[CustomPostEffect(PostEffectType.Early, "Color Wheels")]
public sealed class ColorWheels : StageEffect<ColorWheels, WheelsState, WheelsParameter>;

[Guid("8e3a41fa-43cb-4d27-a488-922a226a0e8a")]
[CustomPostEffect(PostEffectType.Early, "ASC CDL")]
public sealed class ColorDecisionList : StageEffect<ColorDecisionList, CdlState, CdlParameter>;

[Guid("c75e00ae-77a6-4a7f-9556-a2aa0d59a883")]
[CustomPostEffect(PostEffectType.Early, "Contrast and Saturation")]
public sealed class ContrastSaturation : StageEffect<ContrastSaturation, ContrastGrade, ContrastGradeParameter>;

[Guid("485d8eb9-7e34-4c31-99c7-8c4aee2714dd")]
[CustomPostEffect(PostEffectType.Early, "Levels")]
public sealed class SceneLevels : StageEffect<SceneLevels, Levels, LevelsParameter>;

[Guid("b10c1e89-0353-49ca-9e6e-5b9dc0c0c82c")]
[CustomPostEffect(PostEffectType.Early, "RGB Curves")]
public sealed class RgbCurves : StageEffect<RgbCurves, PointCurves, PointCurveParameter>;

[Guid("c619d89b-47ee-4f89-bf6d-9a43711af308")]
[CustomPostEffect(PostEffectType.Early, "Hue Curves")]
public sealed class HueCorrection : StageEffect<HueCorrection, HueCurves, HueCurveKind>;

[Guid("e330c3e8-bff0-452a-87d7-fb23a5ec7fbe")]
[CustomPostEffect(PostEffectType.Early, "Selective Color")]
public sealed class ColorQualifier : StageEffect<ColorQualifier, SelectiveColor, SelectiveColorParameter>;

[Guid("7d1e5a92-c64b-4f38-9e07-b3a28c51d6f4")]
[CustomPostEffect(PostEffectType.Early, "Color Isolation")]
public sealed class ColorIsolation : StageEffect<ColorIsolation, HueIsolation, HueIsolationParameter>;

[Guid("b278e80b-5018-4dbc-9910-e477e852ff02")]
[CustomPostEffect(PostEffectType.Early, "Gradient Map")]
public sealed class GradientMapping : StageEffect<GradientMapping, GradientMap, GradientMapParameter>;

[Guid("2f6b8d41-93c7-4e0a-b5d2-7a1c64e9f308")]
[CustomPostEffect(PostEffectType.Early, "Gamut Compression")]
public sealed class ReferenceGamutCompression : StageEffect<ReferenceGamutCompression, GamutCompression, GamutCompressionParameter>;

[Guid("91b7f223-a7ab-4cec-aa60-99755bdd23d6")]
[CustomPostEffect(PostEffectType.Early, "Scene LUT")]
public sealed class SceneLook : StageEffect<SceneLook, LutLook, LutLookParameter>;

[Guid("ce14bc1b-2e14-41c0-91d5-54e4886e3d1d")]
[CustomPostEffect(PostEffectType.Early, "Sharpen")]
public sealed class AdaptiveSharpening : StageEffect<AdaptiveSharpening, Sharpen, SharpenParameter>;

[Guid("b03f5ab9-d399-4e1e-95ac-ece13e441ced")]
[CustomPostEffect(PostEffectType.Early, "Film Grain")]
public sealed class EmulsionGrain : StageEffect<EmulsionGrain, FilmGrain, FilmGrainParameter>;

[Guid("a9cab48c-9612-4712-85f5-eafd71be51b6")]
[CustomPostEffect(PostEffectType.Early, "Sensor Noise")]
public sealed class ImageNoise : StageEffect<ImageNoise, SensorNoise, SensorNoiseParameter>;

[Guid("49dd6485-0396-4b83-8744-516aecbffe53")]
[CustomPostEffect(PostEffectType.Early, "Local Exposure")]
public sealed class LocalExposure : StageEffect<LocalExposure, LocalContrast, LocalContrastParameter>;

[Guid("72443a3b-a5f1-4018-8368-7ad55d5f6127")]
[CustomPostEffect(PostEffectType.Early, "Film Flicker")]
public sealed class ProjectorFlicker : StageEffect<ProjectorFlicker, FilmFlicker, FilmFlickerParameter>;

[Guid("72433230-65e7-4c28-b1a9-0afa4e6f32e2")]
[CustomPostEffect(PostEffectType.ToneMapping, "Darkroom", PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn)]
public sealed class ImageFormation : StageEffect<ImageFormation, ToneMapping, ToneMappingParameter>, IEffectDefinition<ToneMapping, ToneMappingParameter> {
    public static EffectRow<ToneMapping, ToneMappingParameter> Row { get; } = new() {
        Amount = None,
        Sections = [ScopesSection.Row, LooksSection.Row],
        Looks = toSeq(ToneMappingParameter.Items).Filter(static item => item.Formations.Equals(Seq(Formation.Film))) switch {
            var keys => Seq<(string Name, FilmState Film)>(
                ("Default", FilmState.Default),
                ("Nostalgia", FilmState.Default with {
                    PreExposure = Exposure.Create(5.563035f),
                    Multiplier = FilmState.Default.Multiplier with { Red = PositiveMagnitude.Create(1.1f), Blue = PositiveMagnitude.Create(1.2f) },
                    SigmoidSpan = FilmSpan.Create(33f),
                    NegativeExposure = Exposure.Create(5.8f),
                    Backlight = new(PositiveMagnitude.Create(0.99f), PositiveMagnitude.Create(1.1f), PositiveMagnitude.Create(1.035989f)),
                    PrintDensity = Density.Create(40f),
                    BlackPoint = new(Enabled: true, BlackOffset.Create(-5f)),
                    MidtoneSaturation = Magnitude.Create(1.1f),
                }),
                ("Silver", FilmState.Default with {
                    PreExposure = Exposure.Create(3.9f),
                    PreFilter = FilmState.Default.PreFilter with { Red = AxisFraction.Create(0.95f), Green = AxisFraction.Create(0.975f) },
                    Multiplier = FilmState.Default.Multiplier with { Blue = PositiveMagnitude.Create(1.06f) },
                    NegativeExposure = Exposure.Create(4.7f),
                    NegativeDensity = Density.Create(7f),
                    Backlight = FilmState.Default.Backlight with { Red = PositiveMagnitude.Create(0.9992f), Green = PositiveMagnitude.Create(0.99f) },
                    PrintExposure = Exposure.Create(4.7f),
                    PrintDensity = Density.Create(30f),
                    BlackPoint = new(Enabled: true, BlackOffset.Create(0.5f)),
                    PostFilter = FilmState.Default.PostFilter with { Blue = AxisFraction.Create(0.96f) },
                    MidtoneSaturation = Magnitude.Unit,
                })).Map<(string, EffectLook<ToneMapping, ToneMappingParameter>)>(look => (look.Name, new EffectLook<ToneMapping, ToneMappingParameter>.Partial(ToneMapping.Default with { Film = look.Film }, keys))),
        },
    };
}

[Guid("d8948345-e067-46c3-9e56-52229c540f9c")]
[CustomPostEffect(PostEffectType.Late, "Display LUT")]
public sealed class DisplayLook : StageEffect<DisplayLook, LutLook, LutLookParameter>;

[Guid("3f5e6947-cf6e-4623-a607-5ddda6e05486")]
[CustomPostEffect(PostEffectType.Late, "Mosaic")]
public sealed class Pixelation : StageEffect<Pixelation, Mosaic, MosaicParameter>;

[Guid("6666a316-3ff8-49ce-a566-34d9531372ea")]
[CustomPostEffect(PostEffectType.Late, "Posterize")]
public sealed class Posterization : StageEffect<Posterization, Posterize, PosterizeParameter>;

[Guid("b0fd305b-80fb-4726-ab16-ca4223c7d61c")]
[CustomPostEffect(PostEffectType.Late, "Adaptive Palette")]
public sealed class AdaptivePalette : StageEffect<AdaptivePalette, PaletteReduction, PaletteReductionParameter>;

[Guid("fc33be5b-eb04-4bb7-b890-89e5778451c9")]
[CustomPostEffect(PostEffectType.Late, "Custom Palette")]
public sealed class CustomPalette : StageEffect<CustomPalette, PaletteMapping, PaletteMappingParameter>;

[Guid("d0635c79-6508-4cc8-85fc-4fbc0af55405")]
[CustomPostEffect(PostEffectType.Late, "Halftone")]
public sealed class HalftoneScreen : StageEffect<HalftoneScreen, Halftone, HalftoneParameter>;

[Guid("1aa387a7-1493-4d9f-aeb2-0c9a1b5773b8")]
[CustomPostEffect(PostEffectType.Late, "Glyphs")]
public sealed class GlyphGrid : StageEffect<GlyphGrid, GlyphCells, GlyphCellsParameter>;

[Guid("d9419256-acc5-4728-b874-d9b529d5cab4")]
[CustomPostEffect(PostEffectType.Late, "Pixel Sort")]
public sealed class PixelSorting : StageEffect<PixelSorting, PixelSort, PixelSortParameter>;

[Guid("dbf0553d-7010-4f70-b0c8-1df82c2b722b")]
[CustomPostEffect(PostEffectType.Late, "Kuwahara")]
public sealed class PaintFilter : StageEffect<PaintFilter, Painterly, PainterlyParameter>;

[Guid("965adbbd-2586-4504-8fc7-efff82b7983c")]
[CustomPostEffect(PostEffectType.Late, "Edge Detect")]
public sealed class EdgeFilter : StageEffect<EdgeFilter, EdgeDetect, EdgeDetectParameter>;

[Guid("8a1307d8-a9b1-4d95-b44c-64e8ab72b3d4")]
[CustomPostEffect(PostEffectType.Late, "Outline")]
public sealed class SceneOutline : StageEffect<SceneOutline, GeometryOutline, GeometryOutlineParameter>;

[Guid("2c3dfe8b-89b8-42d4-9bab-7c1f920b02e5")]
[CustomPostEffect(PostEffectType.Late, "Film Damage")]
public sealed class FilmWear : StageEffect<FilmWear, FilmDamage, FilmDamageParameter>;

[Guid("de4f9efc-1a2c-4668-814f-0f280ded7917")]
[CustomPostEffect(PostEffectType.Late, "JPEG Damage")]
public sealed class JpegArtifacts : StageEffect<JpegArtifacts, CodecDamage, CodecDamageParameter>, IEffectDefinition<CodecDamage, CodecDamageParameter> {
    public static EffectRow<CodecDamage, CodecDamageParameter> Row { get; } = new() {
        Looks = [("JPEG", CodecDamage.Default with { Quality = JpegQuality.Create(60), Coded = new(Enabled: true, LineCount.Create(360)) })],
    };
}

[Guid("50657d74-77ee-4dc0-99c9-01b94b4ff0d0")]
[CustomPostEffect(PostEffectType.Late, "Glitch")]
public sealed class DigitalGlitch : StageEffect<DigitalGlitch, Glitch, GlitchParameter>, IEffectDefinition<Glitch, GlitchParameter> {
    public static EffectRow<Glitch, GlitchParameter> Row { get; } = new() {
        Looks = [
            ("Corruption", Glitch.Default with {
                BlockWidth = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide), BlockHeight = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide), BlockDensity = AxisFraction.Create(0.2f),
                BlockShift = ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide), ChromaCorruption = AxisFraction.Create(0.6f), Hold = Hold.Create(12f / 24f),
            }),
            ("Glitch", Glitch.Default with {
                BlockDensity = AxisFraction.Create(0.2f), BlockShift = ShortSideLength.Create(0.05f * ReferenceFrame.GreaterSide), Split = ShortSideOffset.Create(0.03f * ReferenceFrame.GreaterSide), Hold = Hold.Create(2f / 24f),
            }),
        ],
    };
}

[Guid("c33c6983-8c08-4ace-b768-dd7988d96500")]
[CustomPostEffect(PostEffectType.Late, "Analog Video")]
public sealed class VideoSignal : StageEffect<VideoSignal, AnalogVideo, AnalogVideoParameter>, IEffectDefinition<AnalogVideo, AnalogVideoParameter> {
    public static EffectRow<AnalogVideo, AnalogVideoParameter> Row { get; } = new() {
        Looks = [("Chroma Blur", AnalogVideo.Default with { ChromaBandwidth = ShortSideLength.Create(128f / ReferenceFrame.Height / 3f), ChromaVertical = ShortSideLength.Create(12.8f / ReferenceFrame.Height / 3f) })],
    };
}

[Guid("0bfccb12-f88d-4843-8836-e91acfd439a2")]
[CustomPostEffect(PostEffectType.Late, "CRT Display")]
public sealed class TubeDisplay : StageEffect<TubeDisplay, CrtDisplay, CrtDisplayParameter>, IEffectDefinition<CrtDisplay, CrtDisplayParameter> {
    internal static CrtDisplay Scanlines { get; } = CrtDisplay.Default with { MaskStrength = AxisFraction.MinValue, ScanlineDepth = AxisFraction.Create(0.4f) };

    public static EffectRow<CrtDisplay, CrtDisplayParameter> Row { get; } = new() {
        Looks = [
            ("CRT", CrtDisplay.Default with {
                Mask = PhosphorMask.ApertureGrille, Pitch = ShortSideExtent.Create(ReferenceFrame.GreaterSide / 640f), MaskStrength = AxisFraction.Create(0.705394f), Lines = LineCount.Create(240),
                ScanlineDepth = AxisFraction.Create(0.5f), Glow = ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide / 3f), Curvature = Distortion.Create(0.0497677f),
            }),
            ("Scanlines", Scanlines),
        ],
    };
}

[Guid("99007c9c-22fe-4569-a242-ae2392a3a08d")]
[CustomPostEffect(PostEffectType.Late, "Gradient")]
public sealed class GradientLayer : StageEffect<GradientLayer, Gradient, GradientParameter>;

[Guid("cd4ad9d9-44cd-49d1-ad8f-a332aadd6d8d")]
[CustomPostEffect(PostEffectType.Late, "Four Corner Gradient")]
public sealed class FourCornerGradient : StageEffect<FourCornerGradient, CornerGradient, CornerGradientParameter>;

[Guid("45ae93a8-0400-486a-af24-b70e9f05598d")]
[CustomPostEffect(PostEffectType.Late, "Stripes")]
public sealed class Bands : StageEffect<Bands, Stripes, StripesParameter>;

[Guid("fc4fd80c-73d9-4184-bad1-a759b4182f60")]
[CustomPostEffect(PostEffectType.Late, "Grid")]
public sealed class GridLines : StageEffect<GridLines, Grid, GridParameter>;

[Guid("e7c2ca0d-f654-4980-b540-2314d6c58de2")]
[CustomPostEffect(PostEffectType.Late, "Shape")]
public sealed class ShapeLayer : StageEffect<ShapeLayer, Shapes, ShapesParameter>;

[Guid("1f4355d5-07b5-417c-8ff7-4373e3efc668")]
[CustomPostEffect(PostEffectType.Late, "Letterbox")]
public sealed class AspectBars : StageEffect<AspectBars, Letterbox, LetterboxParameter>, IEffectDefinition<Letterbox, LetterboxParameter> {
    public static EffectRow<Letterbox, LetterboxParameter> Row { get; } = new() { Looks = [("Film Gate", Letterbox.FilmGate)] };
}

[Guid("f2d92349-c684-4276-86dd-587c71634aff")]
[CustomPostEffect(PostEffectType.Late, "Output Dither", PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn)]
public sealed class OutputDither : StageEffect<OutputDither, Dither, DitherParameter>, IEffectDefinition<Dither, DitherParameter> {
    public static EffectRow<Dither, DitherParameter> Row { get; } = new() { Replaces = BuiltinEffect.Dithering };
}
