using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Files;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.UI;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DetailStrength : IPresented<DetailStrength, float> {
    public static DetailStrength MinValue { get; } = new(1f);
    public static DetailStrength MaxValue { get; } = new(2.2f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct NoiseDeviation : IPresented<NoiseDeviation, float> {
    public static NoiseDeviation MinValue => Off;
    public static NoiseDeviation MaxValue { get; } = new(MathF.Sqrt(2f * MathF.Log(float.MaxValue)));
    public static Presentation<float> Presentation { get; } = new() { Soft = (0.001f, 1f), Track = Some<Func<AxisRange, Axis>>(static range => new Axis.Log(range, 10f)) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenoiseGuides : ICaptioned {
    public static readonly DenoiseGuides None = new("none", []);
    public static readonly DenoiseGuides Albedo = new("albedo", [GuideChannel.Albedo]);
    public static readonly DenoiseGuides AlbedoNormal = new("albedo-normal", [GuideChannel.Albedo, GuideChannel.Normal]);

    public Seq<GuideChannel> Channels { get; }
}

public sealed record Denoise(DenoiseGuides Guides, GuidePrefilter Prefilter, DenoiseQuality Quality) : IStateRecord<Denoise, DenoiseParameter> {
    public static Denoise Default { get; } = new(DenoiseGuides.AlbedoNormal, GuidePrefilter.Accurate, DenoiseQuality.High);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DenoiseParameter : IStateParameter<Denoise> {
    public static readonly DenoiseParameter Guides = new("guides", new StateParameter<Denoise>.Choice<DenoiseGuides>(Lens<Denoise, DenoiseGuides>.New(static state => state.Guides, static guides => state => state with { Guides = guides })),
        new("Passes", "Render passes separating surface detail from noise"));
    public static readonly DenoiseParameter Prefilter = new("prefilter", new StateParameter<Denoise>.Enumerated<GuidePrefilter>(Lens<Denoise, GuidePrefilter>.New(static state => state.Prefilter, static prefilter => state => state with { Prefilter = prefilter })),
        new("Prefilter", "Cleans noise from guide passes before denoising"));
    public static readonly DenoiseParameter Quality = new("quality", new StateParameter<Denoise>.Choice<DenoiseQuality>(Lens<Denoise, DenoiseQuality>.New(static state => state.Quality, static quality => state => state with { Quality = quality })),
        new("Quality", "Denoiser model, slower and sharper at higher quality"));

    public StateParameter<Denoise> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Sharpen(Magnitude Sharpness) : IStateRecord<Sharpen, SharpenParameter>, IPixelStage<Sharpen> {
    public static Sharpen Default { get; } = new(Magnitude.Zero);

    public static Option<PixelPass> Pass(Sharpen state, PassContext context) =>
        state.Sharpness == Magnitude.Zero ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Sharpening.Laplacian(frame, state.Sharpness, context.Exposure, context.Working.Luminance, progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SharpenParameter : IStateParameter<Sharpen> {
    public static readonly SharpenParameter Sharpness = new("sharpness", StateParameter.Bounded(Lens<Sharpen, Magnitude>.New(static sharpen => sharpen.Sharpness, static sharpness => sharpen => sharpen with { Sharpness = sharpness }),
        Magnitude.Presentation with { Soft = (0f, 10f) }),
        new("Sharpness", "Gain of pixel difference from four axis neighbors, none across exposed luminance steps of 1 or more"));

    public StateParameter<Sharpen> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record LocalContrast(Fraction Highlights, Fraction Shadows, DetailStrength Detail) : IStateRecord<LocalContrast, LocalContrastParameter>, IPixelStage<LocalContrast> {
    public static LocalContrast Default { get; } = new(Fraction.MaxValue, Fraction.MaxValue, DetailStrength.MinValue);

    public static Option<PixelPass> Pass(LocalContrast state, PassContext context) =>
        state == Default ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => ContrastGrid.Graded(
                frame, state.Highlights, state.Shadows, state.Detail, 64d / ReferenceFrame.Height * context.Extent.ShortSide, 0.5d * ReferenceFrame.GreaterSide / 6d * context.Extent.ShortSide, context.Working.Luminance, progress)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LocalContrastParameter : IStateParameter<LocalContrast> {
    public static readonly LocalContrastParameter Highlights = new("highlights", StateParameter.Bounded(Lens<LocalContrast, Fraction>.New(static contrast => contrast.Highlights, static highlights => contrast => contrast with { Highlights = highlights })),
        new("Highlights", "Base contrast above middle gray, 100% unchanged, 0% flat"));
    public static readonly LocalContrastParameter Shadows = new("shadows", StateParameter.Bounded(Lens<LocalContrast, Fraction>.New(static contrast => contrast.Shadows, static shadows => contrast => contrast with { Shadows = shadows })),
        new("Shadows", "Base contrast below middle gray, 100% unchanged, 0% flat"));
    public static readonly LocalContrastParameter Detail = new("detail", StateParameter.Bounded(Lens<LocalContrast, DetailStrength>.New(static contrast => contrast.Detail, static detail => contrast => contrast with { Detail = detail })),
        new("Clarity", "Detail contrast around base, 1 unchanged"));

    public StateParameter<LocalContrast> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record FilmGrain(
    NoiseDeviation Intensity, ShortSideExtent Size, Fraction Chroma, Seed Seed, Timing Timing, Hold Hold,
    Fraction Shadows, Fraction Midtones, Fraction Highlights, Magnitude ShadowEdge, Magnitude MidtoneSpan, Magnitude HighlightSpan, Option<ImageFile> Scan, ShortSideExtent ScanSpan)
    : IStateRecord<FilmGrain, FilmGrainParameter>, IPixelStage<FilmGrain> {
    private const uint Stream = 0u;

    public static FilmGrain Default { get; } = new(
        NoiseDeviation.Off, ShortSideExtent.Create(1f / ReferenceFrame.Height), Fraction.MinValue, Seed.MinValue, Timing.Default, Hold.MinValue, Fraction.MaxValue, Fraction.MaxValue, Fraction.MaxValue,
        Magnitude.Create(0.09f), Magnitude.Create(0.5f - 0.09f), Magnitude.Create(1f - 0.5f), None, ShortSideExtent.Create(ReferenceFrame.GreaterSide));

    public static Option<PixelPass> Pass(FilmGrain state, PassContext context) =>
        state.Intensity == NoiseDeviation.Off || (state.Shadows, state.Midtones, state.Highlights) == (Fraction.MinValue, Fraction.MinValue, Fraction.MinValue) ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from draw in state.Draw(context, state.Seed.Field(Stream, state.Hold.Period(state.Timing.At(context))))
                from applied in GrainNoise.Multiplied(frame, draw.Draw, draw.Sigma, state.Intensity, state.Chroma,
                    GrainNoise.Bands((state.Shadows, state.Midtones, state.Highlights), state.ShadowEdge, state.MidtoneSpan, state.HighlightSpan, context.Exposure),
                    context.Working.Luminance, progress)
                select applied));

    private Fin<(Action<Span<Vector4>, long, long> Draw, Option<double> Sigma)> Draw(PassContext context, uint field) =>
        Scan.Match(
            Some: scan => context.Derivations.Derived((Scan: scan, Span: ScanSpan, context.Extent), static key => GrainNoise.Scores(key.Scan.Frame, key.Span.Pixels(key.Extent)))
                .Map(tile => (GrainNoise.Tiled(tile, field), Option<double>.None)),
            None: () => Fin.Succ((GrainNoise.White(field), Some(Size.Pixels(context.Extent)))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmGrainParameter : IStateParameter<FilmGrain> {
    public static readonly FilmGrainParameter Intensity = new("intensity", StateParameter.Bounded(Lens<FilmGrain, NoiseDeviation>.New(static grain => grain.Intensity, static intensity => grain => grain with { Intensity = intensity })),
        new("Intensity", "Spread of light-multiplying grain factor, 0 off"));
    public static readonly FilmGrainParameter Size = new("size", StateParameter.Bounded(Lens<FilmGrain, ShortSideExtent>.New(static grain => grain.Size, static size => grain => grain with { Size = size }),
        Presentations.Extent with { Soft = (1e-4f, 0.0125f) }),
        new("Size", "Grain size in frame short sides"));
    public static readonly FilmGrainParameter Chroma = new("chroma", StateParameter.Bounded(Lens<FilmGrain, Fraction>.New(static grain => grain.Chroma, static chroma => grain => grain with { Chroma = chroma })),
        new("Chroma", "Per-channel grain share against shared grain"));
    public static readonly FilmGrainParameter Seed = new("seed", StateParameter.Bounded(Lens<FilmGrain, Seed>.New(static grain => grain.Seed, static seed => grain => grain with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly FilmGrainParameter Timing = new("timing", new StateParameter<FilmGrain>.Record<Timing>(Lens<FilmGrain, Timing>.New(static grain => grain.Timing, static timing => grain => grain with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));
    public static readonly FilmGrainParameter Hold = new("hold", StateParameter.Bounded(Lens<FilmGrain, Hold>.New(static grain => grain.Hold, static hold => grain => grain with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));
    public static readonly FilmGrainParameter Shadows = new("shadows", StateParameter.Bounded(Lens<FilmGrain, Fraction>.New(static grain => grain.Shadows, static weight => grain => grain with { Shadows = weight })),
        new("Shadows", "Grain weight below shadow edge"));
    public static readonly FilmGrainParameter Midtones = new("midtones", StateParameter.Bounded(Lens<FilmGrain, Fraction>.New(static grain => grain.Midtones, static weight => grain => grain with { Midtones = weight })),
        new("Midtones", "Grain weight between shadow edge and highlight start"));
    public static readonly FilmGrainParameter Highlights = new("highlights", StateParameter.Bounded(Lens<FilmGrain, Fraction>.New(static grain => grain.Highlights, static weight => grain => grain with { Highlights = weight })),
        new("Highlights", "Grain weight from highlight end up"));
    public static readonly FilmGrainParameter ShadowEdge = new("shadow-edge", StateParameter.Bounded(Lens<FilmGrain, Magnitude>.New(static grain => grain.ShadowEdge, static edge => grain => grain with { ShadowEdge = edge })),
        new("Shadow edge", "Exposed luminance where shadow weight reaches 0"));
    public static readonly FilmGrainParameter MidtoneSpan = new("midtone-span", StateParameter.Bounded(Lens<FilmGrain, Magnitude>.New(static grain => grain.MidtoneSpan, static span => grain => grain with { MidtoneSpan = span })),
        new("Midtone span", "Luminance span from shadow edge to highlight band"));
    public static readonly FilmGrainParameter HighlightSpan = new("highlight-span", StateParameter.Bounded(Lens<FilmGrain, Magnitude>.New(static grain => grain.HighlightSpan, static span => grain => grain with { HighlightSpan = span })),
        new("Highlight span", "Luminance span where highlight weight ramps to full"));
    public static readonly FilmGrainParameter Scan = new("scan", new StateParameter<FilmGrain>.Loaded<ImageFile, ImagePath, string>(Lens<FilmGrain, Option<ImageFile>>.New(static grain => grain.Scan, static scan => grain => grain with { Scan = scan }), ImageFile.Load, static scan => scan.Path),
        new("Scan", "Grain image scored by channel ranks"));
    public static readonly FilmGrainParameter ScanSpan = new("scan-span", StateParameter.Bounded(Lens<FilmGrain, ShortSideExtent>.New(static grain => grain.ScanSpan, static span => grain => grain with { ScanSpan = span }), Presentations.Extent),
        new("Scan span", "Scan width in frame short sides"));

    public StateParameter<FilmGrain> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record SensorNoise(NoiseDeviation Shot, NoiseDeviation Read, Fraction Chroma, Seed Seed, Timing Timing, Hold Hold) : IStateRecord<SensorNoise, SensorNoiseParameter>, IPixelStage<SensorNoise> {
    private const uint Stream = 1u;

    public static SensorNoise Default { get; } = new(
        NoiseDeviation.Create(0.1875f * 0.25f * MathF.Sqrt(Exposure.MiddleGray)), NoiseDeviation.Off, Fraction.MinValue, Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Option<PixelPass> Pass(SensorNoise state, PassContext context) =>
        (state.Shot, state.Read) == (NoiseDeviation.Off, NoiseDeviation.Off) ? None
            : Some<PixelPass>(PhotonNoise.Pass(state.Shot, state.Read, state.Chroma, context.Exposure, context.Working.Luminance,
                state.Seed.Field(Stream, state.Hold.Period(state.Timing.At(context)))));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensorNoiseParameter : IStateParameter<SensorNoise> {
    public static readonly SensorNoiseParameter Shot = new("shot", StateParameter.Bounded(Lens<SensorNoise, NoiseDeviation>.New(static noise => noise.Shot, static shot => noise => noise with { Shot = shot })),
        new("Shot", "Noise deviation at middle gray, rising with square root of light"));
    public static readonly SensorNoiseParameter Read = new("read", StateParameter.Bounded(Lens<SensorNoise, NoiseDeviation>.New(static noise => noise.Read, static read => noise => noise with { Read = read })),
        new("Read", "Noise deviation independent of signal"));
    public static readonly SensorNoiseParameter Chroma = new("chroma", StateParameter.Bounded(Lens<SensorNoise, Fraction>.New(static noise => noise.Chroma, static chroma => noise => noise with { Chroma = chroma })),
        new("Chroma", "Per-channel noise share against shared noise"));
    public static readonly SensorNoiseParameter Seed = new("seed", StateParameter.Bounded(Lens<SensorNoise, Seed>.New(static noise => noise.Seed, static seed => noise => noise with { Seed = seed })),
        new("Seed", "Number selecting pattern"));
    public static readonly SensorNoiseParameter Timing = new("timing", new StateParameter<SensorNoise>.Record<Timing>(Lens<SensorNoise, Timing>.New(static noise => noise.Timing, static timing => noise => noise with { Timing = timing })),
        new("Timing", "Start time and pace over recorded frames"));
    public static readonly SensorNoiseParameter Hold = new("hold", StateParameter.Bounded(Lens<SensorNoise, Hold>.New(static noise => noise.Hold, static hold => noise => noise with { Hold = hold })),
        new("Hold", "Seconds between pattern redraws"));

    public StateParameter<SensorNoise> Kind { get; }
    public ParameterText Text { get; }
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
