using System.Numerics;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct Percentile : IMinMaxValue<Percentile> {
    public static Percentile MinValue { get; } = new(0f);
    public static Percentile MaxValue { get; } = new(1f);
    public static Percentile FrameWhite { get; } = new(0.99995f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct CenterWeight : IMinMaxValue<CenterWeight> {
    public static CenterWeight MinValue { get; } = new(0f);
    public static CenterWeight MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

public sealed class LuminanceSamples {
    public const float Floor = 1e-5f;

    private readonly double[] sorted;

    internal LuminanceSamples(double[] sorted) => this.sorted = sorted;

    public double FrameWhite => At(Percentile.FrameWhite);
    public double Minimum => SortedArrayStatistics.Minimum(sorted);
    public (double Mean, double Maximum, double Minimum) LogStatistics {
        get {
            using SpanOwner<double> owner = SpanOwner<double>.Allocate(sorted.Length);
            Span<double> logs = owner.Span;
            TensorPrimitives.Max(sorted, Floor, logs);
            TensorPrimitives.Log2(logs, logs);
            return (TensorPrimitives.Sum(logs) / logs.Length, logs[^1], logs[0]);
        }
    }

    public static Fin<LuminanceSamples> Gather(PixelFrame frame, (double X, double Y, double Z) luminance) =>
        Sorted(frame, frame.Window, rgb => Luminance(rgb, luminance), static (key, _) => key).Map(static keys => new LuminanceSamples(keys));

    public static Fin<LuminanceSamples> Greatest(PixelFrame frame) =>
        Sorted(frame, frame.Window, static rgb => float.Max(rgb.X, float.Max(rgb.Y, rgb.Z)), static (key, _) => key).Map(static keys => new LuminanceSamples(keys));

    public double At(Percentile percentile) => SortedArrayStatistics.QuantileCustom(sorted, (float)percentile, QuantileDefinition.EmpiricalInvCDF);

    internal static double Luminance(Vector3 color, (double X, double Y, double Z) weights) =>
        (weights.X * color.X) + (weights.Y * color.Y) + (weights.Z * color.Z);

    internal static System.Range Slice(int count, Percentile low, Percentile high) =>
        (int)double.Min(double.Floor(count * (double)float.Min(low, high)), count - 1) switch {
            var start => start..int.Max((int)double.Ceiling(count * (double)float.Max(low, high)), start + 1),
        };

    internal static Fin<T[]> Sorted<T>(PixelFrame frame, (long Left, long Top, long Right, long Bottom) window, Func<Vector3, double> key, Func<double, (long X, long Y), T> sample) {
        Span2D<Vector4> view = frame.View.Span;
        int width = (int)long.Max(window.Right - window.Left, 0L);
        int height = (int)long.Max(window.Bottom - window.Top, 0L);
        T[] samples = GC.AllocateUninitializedArray<T>(width * height);
        int count = 0;
        for (long y = window.Top; width > 0 && y < window.Bottom; y++) {
            for (long x = window.Left; x < window.Right; x++) {
                Vector4 pixel = view[(int)(y - frame.Origin.Y), (int)(x - frame.Origin.X)];
                double level = key(pixel.AsVector3());
                samples[count] = sample(level, (x, y));
                count += pixel.W > 0f && double.IsFinite(level) ? 1 : 0;
            }
        }
        System.Array.Resize(ref samples, count);
        samples.AsSpan().Sort();
        return count is 0 ? new EmptyRegion(window) : samples;
    }
}

public sealed record Metering(Percentile Low, Percentile High, CenterWeight Center, FramePosition Start, FramePosition End) {
    public static Metering Default { get; } = new(Percentile.MinValue, Percentile.MaxValue, CenterWeight.MinValue, FramePosition.TopLeft, FramePosition.BottomRight);

    public Fin<float> Metered(PixelFrame frame, (double X, double Y, double Z) luminance) {
        ((double X, double Y) start, (double X, double Y) end) = (Start.Point(frame.Extent), End.Point(frame.Extent));
        (long Left, long Top, long Right, long Bottom) window = (
            long.Max((long)double.Round(start.X, MidpointRounding.ToEven), frame.Window.Left),
            long.Max((long)double.Round(start.Y, MidpointRounding.ToEven), frame.Window.Top),
            long.Min((long)double.Round(end.X, MidpointRounding.ToEven), frame.Window.Right),
            long.Min((long)double.Round(end.Y, MidpointRounding.ToEven), frame.Window.Bottom));
        (double width, double height, float border) = (window.Right - window.Left, window.Bottom - window.Top, 1f - Center);
        (double middleX, double middleY) = (window.Left + (width / 2d), window.Top + (height / 2d));
        return LuminanceSamples.Sorted(frame, window, rgb => LuminanceSamples.Luminance(rgb, luminance),
                (key, pixel) => (Key: key, Weight: double.Abs(pixel.X + 0.5d - middleX) < width / 4d && double.Abs(pixel.Y + 0.5d - middleY) < height / 4d ? 1f : border))
            .Bind(samples => Metered(samples, window));
    }

    internal Fin<float> Metered((double Key, float Weight)[] samples, (long Left, long Top, long Right, long Bottom) window) =>
        samples.Length is 0 ? new EmptyRegion(window)
        : new ArraySegment<(double Key, float Weight)>(samples)[LuminanceSamples.Slice(samples.Length, Low, High)]
            .Aggregate((Sum: 0d, Weight: 0d), static (total, sample) =>
                (total.Sum + (sample.Weight * double.Log2(double.Max(sample.Key, LuminanceSamples.Floor))), total.Weight + sample.Weight)) switch {
                    (var sum, > 0d and var weight) => (Fin<float>)(float)(double.Log2(Exposure.MiddleGrey) - (sum / weight)),
                    _ => new EmptyCenter(window),
                };
}
