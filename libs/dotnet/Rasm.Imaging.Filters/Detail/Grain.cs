using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using TorchSharp;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Detail;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidDetail>]
public readonly partial struct GrainSize : IMinMaxValue<GrainSize> {
    public static GrainSize Standard { get; } = new(1f / ReferenceFrame.Height);
    public static GrainSize MinValue { get; } = new(float.Epsilon);
    public static GrainSize MaxValue { get; } = new(float.MaxValue);

    public double Pixels(PixelExtent extent) => (double)_value * extent.ShortSide;

    static partial void ValidateFactoryArguments(ref InvalidDetail? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidDetail();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidDetail>]
public readonly partial struct ToneEdge : IMinMaxValue<ToneEdge> {
    public static ToneEdge MinValue { get; } = new(0f);
    public static ToneEdge MaxValue { get; } = new(float.MaxValue);
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

    private static unsafe Fin<Unit> Kernel(FilmGrain state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        uint field = CoordinateHash.Field(NoiseStream.FilmGrain, state.Seed, state.Hold.Period(state.Timing.At(context)));
        (double sigma, float bias, Vector4 mix) = (state.Size.Pixels(context.Extent), state.Intensity * state.Intensity / 2f, state.Intensity * Draws.Mix(state.Chroma));
        (double start, double span, double width) = (state.ShadowEdge, state.MidtoneSpan, state.HighlightSpan);
        double highlightScale = width > 0d ? width : double.Max(start, span);
        (double X, double Y, double Z) = context.Working.Luminance;
        return Try.lift(() => {
            Fin<(Action<Span<Vector4>, long, long> Draw, Option<long> Taps)> draws = state.Scan.Match(
                Some: scan => context.Derivations.Derived((scan, state.ScanSpan, context.Extent), Scores).Map(tile => (Tiled(tile, field), Option<long>.None)),
                None: () => PixelSampling.GaussianTaps<long>(sigma).Map(taps => ((Action<Span<Vector4>, long, long>)((row, column, line) => { for (int x = 0; x < row.Length; x++) row[x] = CoordinateHash.Normals(column + x, line, field); }), Some(taps))));
            return from noise in draws
                   let plane = new PixelFrame(frame.Origin, frame.Size, frame.Extent, static _ => { })
                   from drawn in new PixelPass.Pointwise(noise.Draw).Run(plane, new Progress<int>())
                   from normalized in plane.Filter(header => noise.Taps.Match(Some: taps => Normalized(header, taps, sigma), None: () => Fin.Succ(drawn)))
                   from applied in new PixelPass.Pointwise((row, _, line) => {
                       ReadOnlySpan<Vector4> draws = plane.Row(line);
                       for (int x = 0; x < row.Length; x++) {
                           double luma = (X * row[x].X) + (Y * row[x].Y) + (Z * row[x].Z);
                           double logLuma = double.Log2(double.Max(luma, 0d)) + context.Exposure;
                           double dark = start == 0d ? luma > 0d ? 0d : 1d : 1d - RampInterpolation.Ease.Weight(double.Clamp(double.Exp2(logLuma - double.Log2(start)), 0d, 1d));
                           double distance = highlightScale == 0d ? luma : double.Exp2(logLuma - double.Log2(highlightScale)) - (start / highlightScale) - (span / highlightScale);
                           double bright = width == 0d ? distance > 0d ? 1d : 0d : RampInterpolation.Ease.Weight(double.Clamp(distance, 0d, 1d));
                           double weight = ((double)state.Shadows * dark) + ((double)state.Midtones * (1d - dark - bright)) + ((double)state.Highlights * bright);
                           Vector4 log = mix * draws[x];
                           row[x] *= new Vector4(Vector3.Lerp(Vector3.One, Vector3.Exp(log.AsVector3() + new Vector3(log.W - bias)), (float)weight), 1f);
                       }
                   }).Run(frame, progress)
                   select applied;
        }).Run().MapFail(error => error.Exception.Match(
            Some: cause => cause is OverflowException or OutOfMemoryException ? new GrainFrameRefused(frame.Size, cause) : error,
            None: () => error));

        static Fin<Unit> Normalized(Mat header, long taps, double sigma) =>
            FrameHeaders.GaussianKernel(taps, sigma).Bind(kernel => {
                const double unitVariance = 1d;
                torch.Tensor squared = kernel.square();
                using Mat filtered = new();
                using Mat deviation = new(header.Rows, header.Cols, DepthType.Cv64F, header.NumberOfChannels);
                deviation.SetTo(new MCvScalar(unitVariance, unitVariance, unitVariance, unitVariance));
                return header.Separable(filtered, kernel, kernel, DepthType.Cv64F, PaddingModes.Constant)
                    .Bind(_ => deviation.Separable(deviation, squared, squared, DepthType.Cv64F, PaddingModes.Constant))
                    .Map(_ => {
                        CvInvoke.Sqrt(deviation, deviation);
                        CvInvoke.Divide(filtered, deviation, filtered);
                        filtered.ConvertTo(header, DepthType.Cv32F);
                        return unit;
                    });
            });

        static Action<Span<Vector4>, long, long> Tiled(Memory2D<Vector4> tile, uint field) {
            Vector2 random = NoiseFunctions.White(Vector4.Zero, field).AsVector2();
            (double X, double Y) shift = ((double)random.X * tile.Width, -(double)random.Y * tile.Height);
            return (row, column, line) => {
                ReadOnlySpan2D<Vector4> texels = tile.Span;
                for (int x = 0; x < row.Length; x++) row[x] = texels.Sample((column + x + 0.5d + shift.X, -line - 0.5d + shift.Y), (WrapMode.Periodic, WrapMode.Periodic), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight));
            };
        }

        static Fin<Memory2D<Vector4>> Scores((ImageFile Scan, ShortSideExtent Span, PixelExtent Extent) key) {
            double footprint = key.Span.Pixels(key.Extent);
            (double Width, double Height) size = (double.Ceiling(footprint), double.Ceiling(key.Scan.Frame.Size.Height * footprint / key.Scan.Frame.Size.Width));
            return Try.lift(() => {
                System.Drawing.Size dimensions = new(int.CreateChecked(size.Width), int.CreateChecked(size.Height));
                int pixels = int.CreateChecked((long)dimensions.Width * dimensions.Height);
                Vector4[] scores = new Vector4[pixels];
                using Mat header = key.Scan.Frame.Header();
                using Mat source = new();
                using Mat resized = new();
                using Mat lanes = new();
                using Mat mean = new(4, 4, DepthType.Cv64F, 1);
                Inter interpolation = footprint < header.Cols ? Inter.Area : Inter.Linear;
                header.ConvertTo(source, DepthType.Cv64F);
                CvInvoke.Resize(source, resized, dimensions, 0d, 0d, interpolation);
                const double third = 1d / 3d;
                mean.SetTo([1d, 0d, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 0d, 1d, 0d, third, third, third, 0d]);
                CvInvoke.Transform(resized, lanes, mean);
                ReadOnlySpan<Vector256<double>> texels = new(lanes.DataPointer.ToPointer(), pixels);
                for (int lane = 0; lane < 4; lane++) {
                    double[] values = new double[pixels];
                    for (int texel = 0; texel < pixels; texel++) values[texel] = texels[texel][lane];
                    double[] lower = values.Ranks(RankDefinition.Min);
                    double[] upper = ArrayStatistics.RanksInplace(values, RankDefinition.Max);
                    for (int texel = 0; texel < pixels; texel++) scores[texel][lane] = (float)Normal.InvCDF(0d, 1d, (lower[texel] + upper[texel] - 1d) / (2d * pixels));
                }
                return scores.AsMemory().AsMemory2D(dimensions.Height, dimensions.Width);
            }).Run().MapFail(error => error.Exception.Match(
                Some: cause => cause is CvException or OverflowException or OutOfMemoryException ? new GrainScanRefused(size, cause) : error,
                None: () => error));
        }
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilmGrainParameter : IStateParameter<FilmGrain> {
    public static readonly FilmGrainParameter Intensity = new("intensity", StateParameter.Bounded<FilmGrain, AxisFraction, float, InvalidGrade>(
        Lens<FilmGrain, AxisFraction>.New(static grain => grain.Intensity, static intensity => grain => grain with { Intensity = intensity }), new()));
    public static readonly FilmGrainParameter Size = new("size", StateParameter.Bounded<FilmGrain, GrainSize, float, InvalidDetail>(
        Lens<FilmGrain, GrainSize>.New(static grain => grain.Size, static size => grain => grain with { Size = size }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Scale = TrackScale.Log }));
    public static readonly FilmGrainParameter Chroma = new("chroma", StateParameter.Bounded<FilmGrain, AxisFraction, float, InvalidGrade>(
        Lens<FilmGrain, AxisFraction>.New(static grain => grain.Chroma, static chroma => grain => grain with { Chroma = chroma }), new()));
    public static readonly FilmGrainParameter Seed = new("seed", StateParameter.Bounded<FilmGrain, Seed, int, InvalidGenerator>(
        Lens<FilmGrain, Seed>.New(static grain => grain.Seed, static seed => grain => grain with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly FilmGrainParameter Timing = new("timing", new StateParameter<FilmGrain>.Record<Timing>(
        Lens<FilmGrain, Timing>.New(static grain => grain.Timing, static timing => grain => grain with { Timing = timing })));
    public static readonly FilmGrainParameter Hold = new("hold", StateParameter.Bounded<FilmGrain, Hold, float, InvalidGenerator>(
        Lens<FilmGrain, Hold>.New(static grain => grain.Hold, static hold => grain => grain with { Hold = hold }), Generators.Hold.Presentation));
    public static readonly FilmGrainParameter Shadows = new("shadows", StateParameter.Bounded<FilmGrain, Mix, float, InvalidGrade>(
        Lens<FilmGrain, Mix>.New(static grain => grain.Shadows, static weight => grain => grain with { Shadows = weight }), new()));
    public static readonly FilmGrainParameter Midtones = new("midtones", StateParameter.Bounded<FilmGrain, Mix, float, InvalidGrade>(
        Lens<FilmGrain, Mix>.New(static grain => grain.Midtones, static weight => grain => grain with { Midtones = weight }), new()));
    public static readonly FilmGrainParameter Highlights = new("highlights", StateParameter.Bounded<FilmGrain, Mix, float, InvalidGrade>(
        Lens<FilmGrain, Mix>.New(static grain => grain.Highlights, static weight => grain => grain with { Highlights = weight }), new()));
    public static readonly FilmGrainParameter ShadowEdge = new("shadow-edge", StateParameter.Bounded<FilmGrain, ToneEdge, float, InvalidDetail>(
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.ShadowEdge, static edge => grain => grain with { ShadowEdge = edge }), new()));
    public static readonly FilmGrainParameter MidtoneSpan = new("midtone-span", StateParameter.Bounded<FilmGrain, ToneEdge, float, InvalidDetail>(
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.MidtoneSpan, static span => grain => grain with { MidtoneSpan = span }), new()));
    public static readonly FilmGrainParameter HighlightSpan = new("highlight-span", StateParameter.Bounded<FilmGrain, ToneEdge, float, InvalidDetail>(
        Lens<FilmGrain, ToneEdge>.New(static grain => grain.HighlightSpan, static span => grain => grain with { HighlightSpan = span }), new()));
    public static readonly FilmGrainParameter Scan = new("scan", new StateParameter<FilmGrain>.Loaded<ImageFile, ImagePath, string, InvalidPixelValue>(
        Lens<FilmGrain, Option<ImageFile>>.New(static grain => grain.Scan, static scan => grain => grain with { Scan = scan }), ImageFile.Load, static scan => scan.Path));
    public static readonly FilmGrainParameter ScanSpan = new("scan-span", StateParameter.Bounded<FilmGrain, ShortSideExtent, float, InvalidPixelValue>(
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
        double shot = double.Log2(state.Shot) - (0.5d * (context.Exposure + double.Log2(Exposure.MiddleGrey)));
        double read = double.Log2(state.Read) - context.Exposure;
        Vector4 mix = Draws.Mix(state.Chroma);
        (double X, double Y, double Z) = context.Working.Luminance;
        int luminanceLane = Vector256<double>.Count - 1;
        return state.Shot == AxisFraction.MinValue && state.Read == AxisFraction.MinValue ? None : Some<PixelPass>(new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                double luma = (X * row[x].X) + (Y * row[x].Y) + (Z * row[x].Z);
                Vector4 draws = mix * CoordinateHash.Normals(column + x, line, field);
                Vector256<double> deviations = Vector256<double>.Zero;
                for (int lane = 0; lane < Vector256<double>.Count; lane++) {
                    double signal = lane == luminanceLane ? luma : row[x][lane];
                    double amplitude = double.Log2(double.Abs(draws[lane]));
                    deviations = deviations.WithElement(lane, double.CopySign(double.Hypot(double.Exp2(amplitude + shot + (0.5d * double.Log2(double.Max(signal, 0d)))), double.Exp2(amplitude + read)), draws[lane]));
                }
                for (int lane = 0; lane < luminanceLane; lane++) row[x][lane] = (float)(row[x][lane] + deviations[lane] + deviations[luminanceLane]);
            }
        }));
    }
}

[SmartEnum<string>]
[ValidationError<InvalidDetail>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SensorNoiseParameter : IStateParameter<SensorNoise> {
    public static readonly SensorNoiseParameter Shot = new("shot", StateParameter.Bounded<SensorNoise, AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Shot, static shot => noise => noise with { Shot = shot }), new()));
    public static readonly SensorNoiseParameter Read = new("read", StateParameter.Bounded<SensorNoise, AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Read, static read => noise => noise with { Read = read }), new()));
    public static readonly SensorNoiseParameter Chroma = new("chroma", StateParameter.Bounded<SensorNoise, AxisFraction, float, InvalidGrade>(
        Lens<SensorNoise, AxisFraction>.New(static noise => noise.Chroma, static chroma => noise => noise with { Chroma = chroma }), new()));
    public static readonly SensorNoiseParameter Seed = new("seed", StateParameter.Bounded<SensorNoise, Seed, int, InvalidGenerator>(
        Lens<SensorNoise, Seed>.New(static noise => noise.Seed, static seed => noise => noise with { Seed = seed }), Generators.Seed.Presentation));
    public static readonly SensorNoiseParameter Timing = new("timing", new StateParameter<SensorNoise>.Record<Timing>(
        Lens<SensorNoise, Timing>.New(static noise => noise.Timing, static timing => noise => noise with { Timing = timing })));
    public static readonly SensorNoiseParameter Hold = new("hold", StateParameter.Bounded<SensorNoise, Hold, float, InvalidGenerator>(
        Lens<SensorNoise, Hold>.New(static noise => noise.Hold, static hold => noise => noise with { Hold = hold }), Generators.Hold.Presentation));

    public StateParameter<SensorNoise> Kind { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class Draws {
    public static Vector4 Mix(AxisFraction chroma) => new(new Vector3(MathF.Sqrt(chroma)), MathF.Sqrt(1f - chroma));
}
