using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class DenoiseGuides {
    public static readonly DenoiseGuides Beauty = new("none", []);
    public static readonly DenoiseGuides Albedo = new("albedo", [GuideChannel.Albedo]);
    public static readonly DenoiseGuides AlbedoNormal = new("albedo-normal", [GuideChannel.Albedo, GuideChannel.Normal]);

    public Seq<GuideChannel> Channels { get; }
}

public sealed record Denoise(
    [property: Display(Name = "Passes", Description = "Render passes separating surface detail from noise")] DenoiseGuides Guides,
    [property: Display(Name = "Prefilter", Description = "Cleans noise from guide passes before denoising")] GuidePrefilter Prefilter,
    [property: Display(Name = "Quality", Description = "Denoiser model, slower and sharper at higher quality")] DenoiseQuality Quality) : IEffectState<Denoise> {
    public static Denoise Default { get; } = new(DenoiseGuides.AlbedoNormal, GuidePrefilter.Accurate, DenoiseQuality.High);
}

public sealed record Sharpen(
    [property: Display(Name = "Sharpness", Description = "Gain of pixel difference from four axis neighbors, none across exposed luminance steps of 1 or more")][property: Range(0d, 10d)] float Sharpness)
    : IPixelStage<Sharpen> {
    public static Sharpen Default { get; } = new(0f);

    public static Option<PixelPass> Pass(Sharpen state, PassContext context) =>
        state.Sharpness is 0f ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Sharpening.Laplacian(frame, state.Sharpness, context.Exposure, context.Working, progress)));
}

public sealed record LocalContrast(
    [property: Display(Name = "Highlights", Description = "Base contrast above middle gray, 100% unchanged, 0% flat")][property: Range(0d, 1d)] float Highlights,
    [property: Display(Name = "Shadows", Description = "Base contrast below middle gray, 100% unchanged, 0% flat")][property: Range(0d, 1d)] float Shadows,
    [property: Display(Name = "Clarity", Description = "Detail contrast around base, 1 unchanged")][property: Range(1d, 2.2d)] float Detail) : IPixelStage<LocalContrast> {
    public static LocalContrast Default { get; } = new(1f, 1f, 1f);

    public static Option<PixelPass> Pass(LocalContrast state, PassContext context) =>
        state == Default ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => ContrastGrid.Graded(
                frame, state.Highlights, state.Shadows, state.Detail, 64d / ReferenceFrame.Height * context.Extent.ShortSide, 0.5d * ReferenceFrame.GreaterSide / 6d * context.Extent.ShortSide, context.Working, progress)));
}

public sealed record FilmGrain(
    [property: Display(Name = "Intensity", Description = "Spread of light-multiplying grain factor, 0 off")][property: Range(0d, 1d)] float Intensity,
    [property: Display(Name = "Size", Description = "Grain size in frame short sides")][property: Range(1e-4d, 0.0125d)] ShortSideExtent Size,
    [property: Display(Name = "Chroma", Description = "Per-channel grain share against shared grain")][property: Range(0d, 1d)] float Chroma,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold,
    [property: Display(Name = "Shadows", Description = "Grain weight below shadow edge")][property: Range(0d, 1d)] float Shadows,
    [property: Display(Name = "Midtones", Description = "Grain weight between shadow edge and highlight start")][property: Range(0d, 1d)] float Midtones,
    [property: Display(Name = "Highlights", Description = "Grain weight from highlight end up")][property: Range(0d, 1d)] float Highlights,
    [property: Display(Name = "Shadow edge", Description = "Exposed luminance where shadow weight reaches 0")][property: Range(0d, 1d)] float ShadowEdge,
    [property: Display(Name = "Midtone span", Description = "Luminance span from shadow edge to highlight band")][property: Range(0d, 1d)] float MidtoneSpan,
    [property: Display(Name = "Highlight span", Description = "Luminance span where highlight weight ramps to full")][property: Range(0d, 1d)] float HighlightSpan,
    [property: Display(Name = "Scan", Description = "Grain image scored by channel ranks")] string? Scan,
    [property: Display(Name = "Scan span", Description = "Scan width in frame short sides")][property: Range(0.001d, 8d)] ShortSideExtent ScanSpan) : IPixelStage<FilmGrain> {
    public static FilmGrain Default { get; } = new(
        0f, ShortSideExtent.Create(1f / ReferenceFrame.Height), 0f, Seed.MinValue, Timing.Default, Hold.MinValue, 1f, 1f, 1f, 0.09f, 0.5f - 0.09f, 1f - 0.5f, Scan: null,
        ShortSideExtent.Create(ReferenceFrame.GreaterSide));

    public static Option<PixelPass> Pass(FilmGrain state, PassContext context) =>
        state.Intensity is 0f || state is { Shadows: 0f, Midtones: 0f, Highlights: 0f } ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from draw in state.Draw(context, state.Seed.Field(0u, state.Hold.Period(state.Timing.At(context))))
                from applied in GrainNoise.Multiplied(frame, draw.Draw, draw.Sigma, state.Intensity, state.Chroma,
                    GrainNoise.Bands((state.Shadows, state.Midtones, state.Highlights), state.ShadowEdge, state.MidtoneSpan, state.HighlightSpan, context.Exposure),
                    context.Working, progress)
                select applied));

    private Fin<(Action<Span<Vector4>, long, long> Draw, Option<double> Sigma)> Draw(PassContext context, uint field) =>
        Optional(Scan).Match(
            Some: scan => context.Derivations.Derived(scan, static path => ImageFile.Load(path).RunSafe())
                .Bind(file => context.Derivations.Derived((Scan: file, Span: ScanSpan, context.Extent), static key => GrainNoise.Scores(key.Scan.Frame, key.Span.Pixels(key.Extent))))
                .Map(tile => (GrainNoise.Tiled(tile, field), Option<double>.None)),
            None: () => Fin.Succ((GrainNoise.White(field), Some(Size.Pixels(context.Extent)))));
}

public sealed record SensorNoise(
    [property: Display(Name = "Shot", Description = "Noise deviation at middle gray, rising with square root of light")][property: Range(0d, 1d)] float Shot,
    [property: Display(Name = "Read", Description = "Noise deviation independent of signal")][property: Range(0d, 1d)] float Read,
    [property: Display(Name = "Chroma", Description = "Per-channel noise share against shared noise")][property: Range(0d, 1d)] float Chroma,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold) : IPixelStage<SensorNoise> {
    public static SensorNoise Default { get; } = new(0.1875f * 0.25f * MathF.Sqrt(Exposure.MiddleGray), 0f, 0f, Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Option<PixelPass> Pass(SensorNoise state, PassContext context) =>
        state is { Shot: 0f, Read: 0f } ? None
            : Some<PixelPass>(PhotonNoise.Pass(state.Shot, state.Read, state.Chroma, context.Exposure, context.Working,
                state.Seed.Field(1u, state.Hold.Period(state.Timing.At(context)))));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DenoiseJob : IFrameJob<DenoiseJob, Denoise> {
    private readonly Denoiser denoiser;

    private DenoiseJob(Denoiser denoiser) => this.denoiser = denoiser;

    public static Fin<DenoiseJob> Open() => Denoiser.Open().Map(static denoiser => new DenoiseJob(denoiser));

    public static Seq<GuideChannel> Channels(Denoise state) => state.Guides.Channels;

    public Fin<PixelFrame> Run(Denoise state, PixelFrame color, HashMap<GuideChannel, PixelFrame> guides, IProgress<int> rows) =>
        denoiser.Run(color,
            guides.Find(GuideChannel.Albedo).Filter(_ => state.Guides.Channels.Exists(static channel => channel == GuideChannel.Albedo))
                .Map(albedo => (albedo, guides.Find(GuideChannel.Normal).Filter(_ => state.Guides.Channels.Exists(static channel => channel == GuideChannel.Normal)))),
            state.Prefilter, state.Quality, rows).RunSafe();

    public void Dispose() => denoiser.Dispose();
}
