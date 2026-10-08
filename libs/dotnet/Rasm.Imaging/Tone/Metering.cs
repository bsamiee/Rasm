using System.Drawing;
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

    private readonly float[] sorted;

    internal LuminanceSamples(float[] sorted) => this.sorted = sorted;

    public float FrameWhite => At(Percentile.FrameWhite);
    public float Minimum => SortedArrayStatistics.Minimum(sorted);
    public (float Mean, float Maximum, float Minimum) LogStatistics {
        get {
            using SpanOwner<float> owner = SpanOwner<float>.Allocate(sorted.Length);
            Span<float> logs = owner.Span;
            TensorPrimitives.Max(sorted, Floor, logs);
            TensorPrimitives.Log2(logs, logs);
            return (TensorPrimitives.Sum<float>(logs) / logs.Length, logs[^1], logs[0]);
        }
    }

    public static Fin<LuminanceSamples> Gather(PixelFrame frame, Vector3 luminance) =>
        Sorted(frame, frame.Window, rgb => Vector3.Dot(luminance, rgb), static (key, _) => key).Map(static keys => new LuminanceSamples(keys));

    public static Fin<LuminanceSamples> Greatest(PixelFrame frame) =>
        Sorted(frame, frame.Window, static rgb => float.Max(rgb.X, float.Max(rgb.Y, rgb.Z)), static (key, _) => key).Map(static keys => new LuminanceSamples(keys));

    public float At(Percentile percentile) => SortedArrayStatistics.QuantileCustom(sorted, (float)percentile, QuantileDefinition.EmpiricalInvCDF);

    internal static System.Range Slice(int count, Percentile low, Percentile high) =>
        (int)double.Min(double.Floor(count * (double)float.Min(low, high)), count - 1) switch {
            var start => start..int.Max((int)double.Ceiling(count * (double)float.Max(low, high)), start + 1),
        };

    internal static Fin<T[]> Sorted<T>(PixelFrame frame, Rectangle window, Func<Vector3, float> key, Func<float, Point, T> sample) {
        Span2D<Vector4> view = frame.View.Span;
        T[] samples = GC.AllocateUninitializedArray<T>(window.Width * window.Height);
        int count = 0;
        for (int y = window.Top; y < window.Bottom; y++) {
            for (int x = window.Left; x < window.Right; x++) {
                Vector4 pixel = view[y - frame.Origin.Y, x - frame.Origin.X];
                float level = key(pixel.AsVector3());
                samples[count] = sample(level, new Point(x, y));
                count += pixel.W > 0f && float.IsFinite(level) ? 1 : 0;
            }
        }
        System.Array.Resize(ref samples, count);
        samples.AsSpan().Sort();
        return count == 0 ? new EmptyRegion(window) : samples;
    }
}

public sealed record Metering(Percentile Low, Percentile High, CenterWeight Center, FramePosition Start, FramePosition End) {
    public static Metering Default { get; } = new(Percentile.MinValue, Percentile.MaxValue, CenterWeight.MinValue, FramePosition.TopLeft, FramePosition.BottomRight);

    public Fin<float> Metered(PixelFrame frame, Vector3 luminance) {
        (Point start, Point end) = (Point.Round(new PointF(Start.Point(frame.Extent))), Point.Round(new PointF(End.Point(frame.Extent))));
        Rectangle window = Rectangle.Intersect(Rectangle.FromLTRB(start.X, start.Y, end.X, end.Y), frame.Window);
        (Vector2 size, float border) = (new Vector2(window.Width, window.Height), 1f - Center);
        Vector2 middle = new Vector2(window.X, window.Y) + (size / 2f);
        return LuminanceSamples.Sorted(frame, window, rgb => Vector3.Dot(luminance, rgb),
                (key, pixel) => (Key: key, Weight: Vector2.LessThanAll(Vector2.Abs(new Vector2(pixel.X + 0.5f, pixel.Y + 0.5f) - middle), size / 4f) ? 1f : border))
            .Bind(samples => new ArraySegment<(float Key, float Weight)>(samples)[LuminanceSamples.Slice(samples.Length, Low, High)]
                .Aggregate((Sum: 0d, Weight: 0d), static (total, sample) =>
                    (total.Sum + (sample.Weight * double.Log2(float.Max(sample.Key, LuminanceSamples.Floor))), total.Weight + sample.Weight)) switch {
                        (var sum, > 0d and var weight) => (Fin<float>)(float)(double.Log2(Exposure.MiddleGrey) - (sum / weight)),
                        _ => new EmptyCenter(window),
                    });
    }
}
