using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class DenoisePasses {
    public static readonly DenoisePasses Beauty = new("none", []);
    public static readonly DenoisePasses Albedo = new("albedo", [GuideChannel.Albedo]);
    public static readonly DenoisePasses AlbedoNormal = new("albedo-normal", [GuideChannel.Albedo, GuideChannel.Normal]);

    public Seq<GuideChannel> Channels { get; }
}

[SmartEnum<string>]
public sealed partial class GuidePrefilter {
    public static readonly GuidePrefilter Off = new("off", static (_, guides, _, _) => IO.pure(guides with { Clean = true }));
    public static readonly GuidePrefilter Fast = new("fast", static (_, guides, _, _) => IO.pure(guides));
    public static readonly GuidePrefilter Accurate = new("accurate", static (denoiser, guides, quality, progress) => denoiser.Prefiltered(guides, quality, progress));

    [UseDelegateFromConstructor]
    public partial IO<DenoiseGuides> Cleaned(Denoiser denoiser, DenoiseGuides guides, DenoiseQuality quality, Option<IProgress<int>> progress);
}

public sealed record Denoise(DenoisePasses Guides, GuidePrefilter Prefilter, DenoiseQuality Quality) : IPixelStage<Denoise> {
    public static Denoise Default { get; } = new(DenoisePasses.AlbedoNormal, GuidePrefilter.Accurate, DenoiseQuality.High);

    public static Seq<GuideChannel> Channels(Denoise state) => state.Guides.Channels;

    public static Fin<Option<PixelPass>> Pass(Denoise state, PassContext context) =>
        context.Rendering ? None : Some<PixelPass>(new PixelPass.Frame((frame, progress) => context.Derivations.Held(unit, static _ => Denoiser.Open(), denoiser => (
            from guides in state.Guided(context).Traverse(guides => state.Prefilter.Cleaned(denoiser, guides, state.Quality, progress)).As()
            from denoised in denoiser.Run(frame, guides, state.Quality, progress)
            select Written(denoised, frame)).RunSafe())));

    private Option<DenoiseGuides> Guided(PassContext context) =>
        Used(context, GuideChannel.Albedo).Map(albedo => new DenoiseGuides(albedo, Used(context, GuideChannel.Normal), Clean: false));

    private Option<PixelFrame> Used(PassContext context, GuideChannel channel) =>
        context.Guides.Find(channel).Filter(_ => Guides.Channels.Exists(held => held == channel));

    private static Unit Written(PixelFrame denoised, PixelFrame frame) {
        denoised.View.Span.CopyTo(frame.View.Span);
        return unit;
    }
}

public sealed record Sharpen(float Sharpness) : IPixelStage<Sharpen> {
    public static Sharpen Default { get; } = new(0f);

    public static Fin<Option<PixelPass>> Pass(Sharpen state, PassContext context) =>
        state.Sharpness is 0f ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => LocalContrast.Sharpened(frame, state.Sharpness, context.Exposure, context.Working, progress)));
}

public sealed record Clarity(float Highlights, float Shadows, float Detail) : IPixelStage<Clarity> {
    public static Clarity Default { get; } = new(1f, 1f, 1f);

    public static Fin<Option<PixelPass>> Pass(Clarity state, PassContext context) =>
        state == Default ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => LocalContrast.Graded(
                frame, state.Highlights, state.Shadows, state.Detail, 64d / ReferenceFrame.Height * context.Extent.ShortSide,
                0.5d * ReferenceFrame.GreaterSide / 6d * context.Extent.ShortSide, context.Working, progress)));
}

public sealed record FilmGrain(
    float Intensity, ShortSideLength Size, float Chroma, Seed Seed, Timing Timing, Hold Hold, float Shadows, float Midtones, float Highlights,
    float ShadowEdge, float MidtoneSpan, float HighlightSpan, Option<string> Scan, ShortSideLength ScanSpan) : IPixelStage<FilmGrain> {
    public static FilmGrain Default { get; } = new(
        0f, ShortSideLength.Create(1f / ReferenceFrame.Height), 0f, Seed.MinValue, Timing.Default, Hold.MinValue, 1f, 1f, 1f, 0.09f, 0.5f - 0.09f, 1f - 0.5f, None,
        ShortSideLength.Create(ReferenceFrame.GreaterSide));

    public static Fin<Option<PixelPass>> Pass(FilmGrain state, PassContext context) =>
        state.Intensity is 0f || state is { Shadows: 0f, Midtones: 0f, Highlights: 0f } ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from draw in state.Draw(context, state.Seed.Field(0u, state.Hold.Period(state.Timing.At(context))))
                from applied in GrainNoise.Multiplied(frame, draw.Draw, draw.Sigma, state.Intensity, state.Chroma,
                    GrainNoise.Bands((state.Shadows, state.Midtones, state.Highlights), state.ShadowEdge, state.MidtoneSpan, state.HighlightSpan, (float)context.Exposure),
                    context.Working, progress)
                select applied));

    private Fin<(Action<Span<Vector4>, int, int> Draw, double Sigma)> Draw(PassContext context, uint field) =>
        Scan.Match(
            Some: scan => context.Derivations.Derived(scan, static path => ImageFile.Read(path).RunSafe())
                .Bind(read => context.Derivations.Derived((Scan: read.Frame, Span: ScanSpan, context.Extent), static key => GrainNoise.NormalScores(key.Scan, key.Span.Pixels(key.Extent))))
                .Map(tile => (GrainNoise.Tiled(tile, field), 0d)),
            None: () => (GrainNoise.White(field), Size.Pixels(context.Extent)));
}

public sealed record SensorNoise(float Shot, float Read, float Chroma, Seed Seed, Timing Timing, Hold Hold) : IPixelStage<SensorNoise> {
    public static SensorNoise Default { get; } = new(0.1875f * 0.25f * MathF.Sqrt(Metering.MiddleGray), 0f, 0f, Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Fin<Option<PixelPass>> Pass(SensorNoise state, PassContext context) =>
        state is { Shot: 0f, Read: 0f } ? None
            : Some<PixelPass>(GrainNoise.Photon(state.Shot, state.Read, state.Chroma, context.Exposure, context.Working,
                state.Seed.Field(1u, state.Hold.Period(state.Timing.At(context)))));
}
