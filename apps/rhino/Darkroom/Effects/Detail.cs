using Rasm.Imaging.Filters.Detail;
using Rasm.Rhino.Render.Effects;

using System.Runtime.InteropServices;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("ce14bc1b-2e14-41c0-91d5-54e4886e3d1d")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class AdaptiveSharpening : StageEffect<AdaptiveSharpening, Sharpen, SharpenParameter, InvalidDetail>, IEffectDefinition<Sharpen, SharpenParameter> {
    public const string Caption = "Sharpen";

    public static EffectRow<Sharpen, SharpenParameter> Row { get; } = new(static parameter => parameter.Map(
        sharpness: ParameterText.Of("Sharpness", "Gain of each pixel's difference from its four axis neighbors, none across an edge whose exposed luminance step reaches 1")));
}

[Guid("b03f5ab9-d399-4e1e-95ac-ece13e441ced")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class EmulsionGrain : StageEffect<EmulsionGrain, FilmGrain, FilmGrainParameter, InvalidDetail>, IEffectDefinition<FilmGrain, FilmGrainParameter> {
    public const string Caption = "Film Grain";

    public static EffectRow<FilmGrain, FilmGrainParameter> Row { get; } = new(static parameter => parameter.Map(
        intensity: ParameterText.Of("Intensity", "Spread of the grain factor multiplied into scene light, 0 adding no grain"),
        size: ParameterText.Of("Size", "Grain size as a share of the frame's short side"),
        chroma: ParameterText.Of("Chroma", "Share of each channel's own grain against grain every channel shares"),
        seed: NoiseText.Seed, timing: NoiseText.Timing, hold: NoiseText.Hold,
        shadows: ParameterText.Of("Shadows", "Grain weight below the shadow edge"),
        midtones: ParameterText.Of("Midtones", "Grain weight between the shadow edge and the highlight start"),
        highlights: ParameterText.Of("Highlights", "Grain weight from the highlight end up"),
        shadowEdge: ParameterText.Of("Shadow Edge", "Exposed luminance where the shadow weight reaches 0"),
        midtoneSpan: ParameterText.Of("Midtone Span", "Luminance span from the shadow edge to the highlight band"),
        highlightSpan: ParameterText.Of("Highlight Span", "Luminance span over which the highlight weight reaches full"),
        scan: ParameterText.Of("Scan", "Grain image scored by channel ranks"),
        scanSpan: ParameterText.Of("Scan Span", "Scan width as a share of the frame's short side")));
}

[Guid("49dd6485-0396-4b83-8744-516aecbffe53")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class LocalExposure : StageEffect<LocalExposure, LocalContrast, LocalContrastParameter, InvalidDetail>, IEffectDefinition<LocalContrast, LocalContrastParameter> {
    public const string Caption = "Local Exposure";

    public static EffectRow<LocalContrast, LocalContrastParameter> Row { get; } = new(static parameter => parameter.Map(
        highlights: ParameterText.Of("Highlights", "Base contrast above middle gray, 1 unchanged and 0 flat"),
        shadows: ParameterText.Of("Shadows", "Base contrast below middle gray, 1 unchanged and 0 flat"),
        detail: ParameterText.Of("Clarity", "Detail contrast about the base, 1 unchanged")));
}

[Guid("a9cab48c-9612-4712-85f5-eafd71be51b6")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ImageNoise : StageEffect<ImageNoise, SensorNoise, SensorNoiseParameter, InvalidDetail>, IEffectDefinition<SensorNoise, SensorNoiseParameter> {
    public const string Caption = "Sensor Noise";

    public static EffectRow<SensorNoise, SensorNoiseParameter> Row { get; } = new(static parameter => parameter.Map(
        shot: ParameterText.Of("Shot", "Noise deviation at middle gray, rising with the square root of exposed light"),
        read: ParameterText.Of("Read", "Noise deviation independent of the signal"),
        chroma: ParameterText.Of("Chroma", "Share of each channel's own noise against noise every channel shares"),
        seed: NoiseText.Seed, timing: NoiseText.Timing, hold: NoiseText.Hold));
}
