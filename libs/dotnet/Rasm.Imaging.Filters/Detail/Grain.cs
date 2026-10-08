using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Detail;

// --- [MODELS] --------------------------------------------------------------------------

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidDetail>]
public readonly partial struct GrainSize : IMinMaxValue<GrainSize> {
    public static GrainSize Standard { get; } = new(1f / 1080f);
    public static GrainSize MinValue { get; } = new(0.1f * Standard._value);
    public static GrainSize MaxValue { get; } = new(4f * Standard._value);

    public float Pixels(PixelExtent extent) => _value * int.Min(extent.Width, extent.Height);

    static partial void ValidateFactoryArguments(ref InvalidDetail? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidDetail();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidDetail>]
public readonly partial struct ToneEdge : IMinMaxValue<ToneEdge> {
    public static ToneEdge MinValue { get; } = new(0f);
    public static ToneEdge MaxValue { get; } = new(HistogramAxis.LowerBound(HistogramAxis.Bins - 1));
    public static ToneEdge ShadowEdge { get; } = new(0.09f);
    public static ToneEdge MidtoneSpan { get; } = new(0.5f - ShadowEdge._value);
    public static ToneEdge HighlightSpan { get; } = new(1f - (ShadowEdge._value + MidtoneSpan._value));

    static partial void ValidateFactoryArguments(ref InvalidDetail? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidDetail();
}


public sealed record FilmGrain(
    AxisFraction Intensity, GrainSize Size, AxisFraction Chroma, Seed Seed, Timing Timing, Hold Hold,
    Mix Shadows, Mix Midtones, Mix Highlights, ToneEdge ShadowEdge, ToneEdge MidtoneSpan, ToneEdge HighlightSpan, Option<ImageFile> Scan, ShortSideExtent ScanSpan)
    : IStateRecord<FilmGrain, FilmGrainParameter, InvalidDetail>, IPixelStage<FilmGrain> {
    public static FilmGrain Default { get; } = new(
        AxisFraction.MinValue, GrainSize.Standard, AxisFraction.MinValue, Seed.MinValue, Timing.Standard, Hold.MinValue,
        Mix.Full, Mix.Full, Mix.Full, ToneEdge.ShadowEdge, ToneEdge.MidtoneSpan, ToneEdge.HighlightSpan, None, ShortSideExtent.Create(ReferenceFrame.GreaterSide));

    public static Option<PixelPass> Pass(FilmGrain state, PassContext context) =>
        state.Intensity == AxisFraction.MinValue || (state.Shadows == Mix.MinValue && state.Midtones == Mix.MinValue && state.Highlights == Mix.MinValue)
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => {
                uint field = CoordinateHash.Field(NoiseStream.FilmGrain, state.Seed, state.Hold.Period(state.Timing.At(context)));
                (float shared, float own, float strength) = (MathF.Sqrt(1f - state.Chroma), MathF.Sqrt(state.Chroma), state.Intensity);
                (float lower, float toShadows, float toHighlights) = (state.ShadowEdge + (float)state.MidtoneSpan, 1f / state.ShadowEdge, 1f / state.HighlightSpan);
                using Mat grain = new();
                _ = state.Scan.Match(scan => {
                    using Mat source = scan.Frame.Header();
                    double scale = state.ScanSpan.Pixels(context.Extent) / source.Cols;
                    CvInvoke.Resize(source, grain, System.Drawing.Size.Ceiling(source.Size * (float)scale), 0d, 0d, scale < 1d ? Inter.Area : Inter.Linear);
                    foreach (ref Vector4 pixel in grain.GetSpan<Vector4>()) pixel.W = Vector3.Dot(pixel.AsVector3(), Vector3.One) / 3f;
                    Span2D<float> channels = grain.GetSpan<float>().AsSpan2D(grain.Rows * grain.Cols, grain.NumberOfChannels);
                    for (int channel = 0; channel < channels.Width; channel++) {
                        double[] ranks = channels.GetColumn(channel).ToArray().Select(static value => (double)value).Ranks();
                        ranks.Select(rank => (float)Normal.InvCDF(0d, 1d, (rank - 0.5d) / ranks.Length)).ToArray().AsSpan().CopyTo(channels.GetColumn(channel));
                    }
                }, () => {
                    (int height, int width, float sigma) = (frame.Size.Height, frame.Size.Width, state.Size.Pixels(context.Extent));
                    using Mat header = frame.Header();
                    grain.Create(height, width, header.Depth, header.NumberOfChannels);
                    _ = Parallel.For(0, height, y => {
                        Span<Vector4> row = grain.GetSpan<Vector4>().Slice(y * width, width);
                        for (int x = 0; x < row.Length; x++) row[x] = CoordinateHash.Normals(frame.Origin.X + x, frame.Line(y), field);
                    });
                    using Mat taps = CvInvoke.GetGaussianKernel((2 * (int)float.Ceiling(3f * sigma)) + 1, sigma, DepthType.Cv32F);
                    using Mat squares = new();
                    using Mat variance = new(height, width, header.Depth, header.NumberOfChannels);
                    variance.GetSpan<Vector4>().Fill(Vector4.One);
                    CvInvoke.Multiply(taps, taps, squares);
                    Point anchor = new(-1, -1);
                    CvInvoke.SepFilter2D(grain, grain, DepthType.Cv32F, taps, taps, anchor, 0d, BorderType.Constant);
                    CvInvoke.SepFilter2D(variance, variance, DepthType.Cv32F, squares, squares, anchor, 0d, BorderType.Constant);
                    CvInvoke.Sqrt(variance, variance);
                    CvInvoke.Divide(grain, variance, grain);
                });
                Vector4 offset = state.Scan.Map(_ => NoiseFunctions.White(Vector4.Zero, field)).IfNone(Vector4.Zero);
                return new PixelPass.Pointwise((row, column, line) => {
                    using SpanOwner<Vector4> factors = SpanOwner<Vector4>.Allocate(row.Length);
                    using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
                    ReadOnlySpan2D<Vector4> samples = grain.GetSpan<Vector4>().AsSpan2D(grain.Rows, grain.Cols);
                    int y = state.Scan.IsSome ? (line + (int)(offset.Y * grain.Rows)) % grain.Rows : frame.Line(line);
                    for (int x = 0; x < row.Length; x++) {
                        Vector4 sample = samples[y, (column + x + (int)(offset.X * grain.Cols)) % grain.Cols];
                        Vector3 factor = Vector3.Exp((strength * ((own * sample.AsVector3()) + new Vector3(shared * sample.W))) - new Vector3(strength * strength / 2f));
                        float luma = context.Exposure.Scale * Vector3.Dot(context.Working.Luminance, row[x].AsVector3());
                        float dark = 1f - Easing.SmoothStep(Easing.Saturate(luma * toShadows));
                        float bright = Easing.SmoothStep(Easing.Saturate((luma - lower) * toHighlights));
                        weights.Span[x] = (state.Shadows * dark) + (state.Midtones * (1f - dark - bright)) + (state.Highlights * bright);
                        factors.Span[x] = new Vector4(factor, row[x].W);
                    }
                    BlendingMode.Multiply.Mixed(row, factors.Span, weights.Span);
                    factors.Span.CopyTo(row);
                }).Run(frame, progress);
            }));
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmGrainParameter : IStateParameter<FilmGrain> {
    private static readonly (StateParameter<FilmGrain> Clock, StateParameter<FilmGrain> Pace) Time = Timing.Kinds(
        Lens<FilmGrain, Timing>.New(static grain => grain.Timing, static timing => grain => grain with { Timing = timing }));
    public static readonly FilmGrainParameter Intensity = new("intensity", new StateParameter<FilmGrain>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<FilmGrain, AxisFraction>.New(static grain => grain.Intensity, static intensity => grain => grain with { Intensity = intensity }), new()));
    public static readonly FilmGrainParameter Size = new("size", new StateParameter<FilmGrain>.Bounded<GrainSize, float, InvalidDetail>(
        Lens<FilmGrain, GrainSize>.New(static grain => grain.Size, static size => grain => grain with { Size = size }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log }));
    public static readonly FilmGrainParameter Chroma = new("chroma", new StateParameter<FilmGrain>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<FilmGrain, AxisFraction>.New(static grain => grain.Chroma, static chroma => grain => grain with { Chroma = chroma }), new()));
    public static readonly FilmGrainParameter Seed = new("seed", new StateParameter<FilmGrain>.Bounded<Seed, int, InvalidGenerator>(
        Lens<FilmGrain, Seed>.New(static grain => grain.Seed, static seed => grain => grain with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmGrainParameter Clock = new("clock", Time.Clock);
    public static readonly FilmGrainParameter Pace = new("pace", Time.Pace);
    public static readonly FilmGrainParameter Hold = new("hold", new StateParameter<FilmGrain>.Bounded<Hold, float, InvalidGenerator>(
        Lens<FilmGrain, Hold>.New(static grain => grain.Hold, static hold => grain => grain with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly FilmGrainParameter Shadows = new("shadows", new StateParameter<FilmGrain>.Bounded<Mix, float, InvalidGrade>(
        Lens<FilmGrain, Mix>.New(static grain => grain.Shadows, static weight => grain => grain with { Shadows = weight }), new()));
    public static readonly FilmGrainParameter Midtones = new("midtones", new StateParameter<FilmGrain>.Bounded<Mix, float, InvalidGrade>(
        Lens<FilmGrain, Mix>.New(static grain => grain.Midtones, static weight => grain => grain with { Midtones = weight }), new()));
    public static readonly FilmGrainParameter Highlights = new("highlights", new StateParameter<FilmGrain>.Bounded<Mix, float, InvalidGrade>(
        Lens<FilmGrain, Mix>.New(static grain => grain.Highlights, static weight => grain => grain with { Highlights = weight }), new()));
    public static readonly FilmGrainParameter ShadowEdge = new("shadow-edge", new StateParameter<FilmGrain>.Bounded<ToneEdge, float, InvalidDetail>(
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.ShadowEdge, static edge => grain => grain with { ShadowEdge = edge }), new()));
    public static readonly FilmGrainParameter MidtoneSpan = new("midtone-span", new StateParameter<FilmGrain>.Bounded<ToneEdge, float, InvalidDetail>(
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.MidtoneSpan, static edge => grain => grain with { MidtoneSpan = edge }), new()));
    public static readonly FilmGrainParameter HighlightSpan = new("highlight-span", new StateParameter<FilmGrain>.Bounded<ToneEdge, float, InvalidDetail>(
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.HighlightSpan, static edge => grain => grain with { HighlightSpan = edge }), new()));

    public static readonly FilmGrainParameter Scan = new("scan", new StateParameter<FilmGrain>.Loaded<ImageFile, ImagePath, string, InvalidPixelValue>(
        Lens<FilmGrain, Option<ImageFile>>.New(static grain => grain.Scan, static scan => grain => grain with { Scan = scan }), ImageFile.Load, static scan => scan.Path));
    public static readonly FilmGrainParameter ScanSpan = new("scan-span", new StateParameter<FilmGrain>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<FilmGrain, ShortSideExtent>.New(static grain => grain.ScanSpan, static span => grain => grain with { ScanSpan = span }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log }));

    public StateParameter<FilmGrain> Kind { get; }
}

public sealed record SensorNoise(AxisFraction Shot, AxisFraction Read, AxisFraction Chroma, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<SensorNoise, SensorNoiseParameter, InvalidDetail>, IPixelStage<SensorNoise> {
    public static SensorNoise Default { get; } = new(AxisFraction.Create(0.1875f * 0.25f * MathF.Sqrt(Exposure.MiddleGrey)), AxisFraction.MinValue, AxisFraction.MinValue, Seed.MinValue, Timing.Standard, Hold.MinValue);

    public static Option<PixelPass> Pass(SensorNoise state, PassContext context) =>
        state.Shot == AxisFraction.MinValue && state.Read == AxisFraction.MinValue
            ? None
            : (CoordinateHash.Field(NoiseStream.SensorNoise, state.Seed, state.Hold.Period(state.Timing.At(context))), MathF.Sqrt(1f - state.Chroma), MathF.Sqrt(state.Chroma), state.Shot * (float)state.Shot / Exposure.MiddleGrey, state.Read * (float)state.Read) switch {
                var (field, shared, own, shot, read) => Some<PixelPass>(new PixelPass.Pointwise((row, column, line) => {
                    for (int x = 0; x < row.Length; x++) {
                        (Vector4 z, Vector3 color) = (CoordinateHash.Normals(column + x, line, field), row[x].AsVector3());
                        float luma = MathF.Sqrt((shot * float.Max(context.Exposure.Scale * Vector3.Dot(context.Working.Luminance, color), 0f)) + read);
                        Vector3 channels = Vector3.SquareRoot((shot * Vector3.Max(context.Exposure.Scale * color, Vector3.Zero)) + new Vector3(read));
                        row[x] = new Vector4(color + ((new Vector3(shared * luma * z.W) + (own * channels * z.AsVector3())) / context.Exposure.Scale), row[x].W);
                    }
                })),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensorNoiseParameter : IStateParameter<SensorNoise> {
    private static readonly (StateParameter<SensorNoise> Clock, StateParameter<SensorNoise> Pace) Time = Timing.Kinds(
        Lens<SensorNoise, Timing>.New(static noise => noise.Timing, static timing => noise => noise with { Timing = timing }));
    public static readonly SensorNoiseParameter Shot = new("shot", new StateParameter<SensorNoise>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Shot, static shot => noise => noise with { Shot = shot }), new()));
    public static readonly SensorNoiseParameter Read = new("read", new StateParameter<SensorNoise>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Read, static read => noise => noise with { Read = read }), new()));
    public static readonly SensorNoiseParameter Chroma = new("chroma", new StateParameter<SensorNoise>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Chroma, static chroma => noise => noise with { Chroma = chroma }), new()));
    public static readonly SensorNoiseParameter Seed = new("seed", new StateParameter<SensorNoise>.Bounded<Seed, int, InvalidGenerator>(
        Lens<SensorNoise, Seed>.New(static noise => noise.Seed, static seed => noise => noise with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly SensorNoiseParameter Clock = new("clock", Time.Clock);
    public static readonly SensorNoiseParameter Pace = new("pace", Time.Pace);
    public static readonly SensorNoiseParameter Hold = new("hold", new StateParameter<SensorNoise>.Bounded<Hold, float, InvalidGenerator>(
        Lens<SensorNoise, Hold>.New(static noise => noise.Hold, static hold => noise => noise with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<SensorNoise> Kind { get; }
}
