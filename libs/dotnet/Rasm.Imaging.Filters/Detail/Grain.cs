using System.Drawing;
using System.Numerics;
using CommunityToolkit.HighPerformance;
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
    public static GrainSize Standard { get; } = new(1f / ReferenceFrame.Height);
    public static GrainSize MinValue { get; } = new(0.1f * Standard._value);
    public static GrainSize MaxValue { get; } = new(4f * Standard._value);

    public float Pixels(PixelExtent extent) => _value * extent.ShortSide;

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
        AxisFraction.MinValue, GrainSize.Standard, AxisFraction.MinValue, Seed.MinValue, Timing.Default, Hold.MinValue, Mix.Full, Mix.Full, Mix.Full, ToneEdge.ShadowEdge, ToneEdge.MidtoneSpan,
        ToneEdge.HighlightSpan, None, Valid.Value(ShortSideExtent.Validate(ReferenceFrame.GreaterSide, provider: null, out ShortSideExtent span), span));

    public static Option<PixelPass> Pass(FilmGrain state, PassContext context) =>
        state.Intensity == AxisFraction.MinValue || (state.Shadows == Mix.MinValue && state.Midtones == Mix.MinValue && state.Highlights == Mix.MinValue)
            ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)));

    private static Fin<Unit> Kernel(FilmGrain state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        uint field = CoordinateHash.Field(NoiseStream.FilmGrain, state.Seed, state.Hold.Period(state.Timing.At(context)));
        (float sigma, float bias, Vector4 mix) = (state.Size.Pixels(context.Extent), state.Intensity * state.Intensity / 2f, state.Intensity * Draws.Mix(state.Chroma));
        (int taps, Vector3 luminance, float rise) = (PixelSampling.GaussianTaps(sigma), context.Exposure.Scale * context.Working.Luminance, state.ShadowEdge + (float)state.MidtoneSpan);
        return state.Scan.Match(
                Some: scan => Plane(0, Tiled(context.Derivations.Derived((scan, state.ScanSpan, context.Extent), Scores), field)),
                None: () => Plane(taps / 2, (row, column, line) => { for (int x = 0; x < row.Length; x++) row[x] = CoordinateHash.Normals(column + x, line, field); }).Map(Blurred))
            .Bind(plane => new PixelPass.Pointwise((row, column, line) => {
                ReadOnlySpan<Vector4> draws = plane.Row(line)[(column - plane.Origin.X)..];
                for (int x = 0; x < row.Length; x++) {
                    (float luma, Vector4 log) = (Vector3.Dot(luminance, row[x].AsVector3()), mix * draws[x]);
                    (float dark, float bright) = (1f - (float)RampInterpolation.Ease.Weight(Easing.Saturate(luma / state.ShadowEdge)), (float)RampInterpolation.Ease.Weight(Easing.Saturate((luma - rise) / state.HighlightSpan)));
                    float weight = (state.Shadows * dark) + (state.Midtones * (1f - dark - bright)) + (state.Highlights * bright);
                    row[x] *= new Vector4(Vector3.Lerp(Vector3.One, Vector3.Exp(log.AsVector3() + new Vector3(log.W - bias)), weight), 1f);
                }
            }).Run(frame, progress));

        Fin<PixelFrame> Plane(int reach, Action<Span<Vector4>, int, int> draw) =>
            PixelExtent.Validate(frame.Size.Width + (2 * reach), frame.Size.Height + (2 * reach), out PixelExtent size) is { } error
                ? error
                : new PixelFrame(frame.Origin - new Size(reach, reach), size, frame.Extent, static _ => { }) switch {
                    var plane => new PixelPass.Pointwise(draw).Run(plane, new Progress<int>()).Map(_ => plane),
                };

        PixelFrame Blurred(PixelFrame plane) {
            using Mat header = plane.Header();
            using Mat kernel = CvInvoke.GetGaussianKernel(taps, sigma, DepthType.Cv32F);
            CvInvoke.Normalize(kernel, kernel);
            CvInvoke.SepFilter2D(header, header, DepthType.Cv32F, kernel, kernel, new Point(-1, -1), borderType: BorderType.Constant);
            return plane;
        }

        static Action<Span<Vector4>, int, int> Tiled(Memory2D<Vector4> tile, uint field) {
            Vector2 shift = Vector2.Truncate(NoiseFunctions.White(Vector4.Zero, field).AsVector2() * new Vector2(tile.Width, -tile.Height));
            return (row, column, line) => {
                ReadOnlySpan2D<Vector4> texels = tile.Span;
                for (int x = 0; x < row.Length; x++) row[x] = texels.Sample(new Vector2(column + x + 0.5f, -line - 0.5f) + shift, (WrapMode.Periodic, WrapMode.Periodic));
            };
        }

        static Memory2D<Vector4> Scores((ImageFile Scan, ShortSideExtent Span, PixelExtent Extent) key) {
            using Mat source = key.Scan.Frame.Header();
            using Mat resized = new();
            using Mat lanes = new();
            using Mat mean = new(4, 4, DepthType.Cv32F, 1);
            float scale = key.Span.Pixels(key.Extent) / source.Cols;
            CvInvoke.Resize(source, resized, System.Drawing.Size.Ceiling(source.Size * scale), 0d, 0d, scale < 1f ? Inter.Area : Inter.Linear);
            mean.SetTo([1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 1f / 3f, 1f / 3f, 1f / 3f, 0f]);
            CvInvoke.Transform(resized, lanes, mean);
            float[] scores = lanes.GetSpan<float>().ToArray();
            Span2D<float> planes = scores.AsSpan().AsSpan2D(scores.Length / 4, 4);
            for (int lane = 0; lane < 4; lane++) {
                double[] ranks = planes.GetColumn(lane).ToArray().Select(static value => (double)value).Ranks();
                for (int texel = 0; texel < ranks.Length; texel++) planes[texel, lane] = (float)Normal.InvCDF(0d, 1d, (ranks[texel] - 0.5d) / ranks.Length);
            }
            return scores.AsMemory().Cast<float, Vector4>().AsMemory2D(lanes.Rows, lanes.Cols);
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmGrainParameter : IStateParameter<FilmGrain> {
    public static readonly FilmGrainParameter Intensity = new("intensity", new StateParameter<FilmGrain>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<FilmGrain, AxisFraction>.New(static grain => grain.Intensity, static intensity => grain => grain with { Intensity = intensity }), new()));
    public static readonly FilmGrainParameter Size = new("size", new StateParameter<FilmGrain>.Bounded<GrainSize, float, InvalidDetail>(
        Lens<FilmGrain, GrainSize>.New(static grain => grain.Size, static size => grain => grain with { Size = size }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log }));
    public static readonly FilmGrainParameter Chroma = new("chroma", new StateParameter<FilmGrain>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<FilmGrain, AxisFraction>.New(static grain => grain.Chroma, static chroma => grain => grain with { Chroma = chroma }), new()));
    public static readonly FilmGrainParameter Seed = new("seed", new StateParameter<FilmGrain>.Bounded<Seed, int, InvalidGenerator>(
        Lens<FilmGrain, Seed>.New(static grain => grain.Seed, static seed => grain => grain with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmGrainParameter Timing = new("timing", new StateParameter<FilmGrain>.Record<Timing>(
        Lens<FilmGrain, Timing>.New(static grain => grain.Timing, static timing => grain => grain with { Timing = timing })));
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
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.MidtoneSpan, static span => grain => grain with { MidtoneSpan = span }), new()));
    public static readonly FilmGrainParameter HighlightSpan = new("highlight-span", new StateParameter<FilmGrain>.Bounded<ToneEdge, float, InvalidDetail>(
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.HighlightSpan, static span => grain => grain with { HighlightSpan = span }), new()));
    public static readonly FilmGrainParameter Scan = new("scan", new StateParameter<FilmGrain>.Loaded<ImageFile, ImagePath, string, InvalidPixelValue>(
        Lens<FilmGrain, Option<ImageFile>>.New(static grain => grain.Scan, static scan => grain => grain with { Scan = scan }), ImageFile.Load, static scan => scan.Path));
    public static readonly FilmGrainParameter ScanSpan = new("scan-span", new StateParameter<FilmGrain>.Bounded<ShortSideExtent, float, InvalidPixelValue>(
        Lens<FilmGrain, ShortSideExtent>.New(static grain => grain.ScanSpan, static span => grain => grain with { ScanSpan = span }), ShortSideExtent.Presentation));

    public StateParameter<FilmGrain> Kind { get; }
}

public sealed record SensorNoise(AxisFraction Shot, AxisFraction Read, AxisFraction Chroma, Seed Seed, Timing Timing, Hold Hold)
    : IStateRecord<SensorNoise, SensorNoiseParameter, InvalidDetail>, IPixelStage<SensorNoise> {
    public static SensorNoise Default { get; } = new(
        Valid.Value(AxisFraction.Validate(0.1875f * 0.25f * MathF.Sqrt(Exposure.MiddleGrey), provider: null, out AxisFraction shot), shot),
        AxisFraction.MinValue, AxisFraction.MinValue, Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Option<PixelPass> Pass(SensorNoise state, PassContext context) {
        uint field = CoordinateHash.Field(NoiseStream.SensorNoise, state.Seed, state.Hold.Period(state.Timing.At(context)));
        (float shot, float read, Vector4 mix) = (state.Shot * (float)state.Shot * context.Exposure.Scale / Exposure.MiddleGrey, state.Read * (float)state.Read, Draws.Mix(state.Chroma) / context.Exposure.Scale);
        return state.Shot == AxisFraction.MinValue && state.Read == AxisFraction.MinValue ? None : Some<PixelPass>(new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                Vector4 deviation = mix * CoordinateHash.Normals(column + x, line, field)
                    * Vector4.SquareRoot((shot * Vector4.Max(row[x] with { W = Vector3.Dot(context.Working.Luminance, row[x].AsVector3()) }, Vector4.Zero)) + new Vector4(read));
                row[x] += new Vector4(deviation.AsVector3() + new Vector3(deviation.W), 0f);
            }
        }));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensorNoiseParameter : IStateParameter<SensorNoise> {
    public static readonly SensorNoiseParameter Shot = new("shot", new StateParameter<SensorNoise>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Shot, static shot => noise => noise with { Shot = shot }), new()));
    public static readonly SensorNoiseParameter Read = new("read", new StateParameter<SensorNoise>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Read, static read => noise => noise with { Read = read }), new()));
    public static readonly SensorNoiseParameter Chroma = new("chroma", new StateParameter<SensorNoise>.Bounded<AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Chroma, static chroma => noise => noise with { Chroma = chroma }), new()));
    public static readonly SensorNoiseParameter Seed = new("seed", new StateParameter<SensorNoise>.Bounded<Seed, int, InvalidGenerator>(
        Lens<SensorNoise, Seed>.New(static noise => noise.Seed, static seed => noise => noise with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly SensorNoiseParameter Timing = new("timing", new StateParameter<SensorNoise>.Record<Timing>(
        Lens<SensorNoise, Timing>.New(static noise => noise.Timing, static timing => noise => noise with { Timing = timing })));
    public static readonly SensorNoiseParameter Hold = new("hold", new StateParameter<SensorNoise>.Bounded<Hold, float, InvalidGenerator>(
        Lens<SensorNoise, Hold>.New(static noise => noise.Hold, static hold => noise => noise with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<SensorNoise> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Draws {
    public static Vector4 Mix(AxisFraction chroma) => new(new Vector3(MathF.Sqrt(chroma)), MathF.Sqrt(1f - chroma));
}
