using System.Numerics;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using UnitsNet;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class LuminanceSamples {
    public const float Floor = 1e-5f;

    private const float WhitePercentile = 0.99995f;

    private readonly double[] sorted;

    internal LuminanceSamples(double[] sorted) => this.sorted = sorted;

    public double FrameWhite => At(WhitePercentile);
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

    public static Fin<LuminanceSamples> Gather(PixelFrame frame, Gamut gamut) =>
        gamut.Luminance switch {
            var luminance => Sorted(frame, frame.Window, rgb => Luminance(rgb.X, rgb.Y, rgb.Z, luminance), static (key, _) => key).Map(static keys => new LuminanceSamples(keys)),
        };

    public static Fin<LuminanceSamples> Greatest(PixelFrame frame) =>
        Sorted(frame, frame.Window, static rgb => float.Max(rgb.X, float.Max(rgb.Y, rgb.Z)), static (key, _) => key).Map(static keys => new LuminanceSamples(keys));

    public double At(float percentile) => SortedArrayStatistics.QuantileCustom(sorted, percentile, QuantileDefinition.EmpiricalInvCDF);

    public static double Luminance(double red, double green, double blue, (double R, double G, double B) weights) =>
        (weights.R * red) + (weights.G * green) + (weights.B * blue);

    internal static ArraySegment<T> Slice<T>(T[] samples, float low, float high) =>
        (int)double.Min(double.Floor(samples.Length * (double)float.Min(low, high)), samples.Length - 1) switch {
            var start => new ArraySegment<T>(samples)[start..int.Max((int)double.Ceiling(samples.Length * (double)float.Max(low, high)), start + 1)],
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
        return count is 0 ? new ImagingFault($"region {window} holds no covered finite pixel") : samples;
    }
}

public sealed record Metering(float Low, float High, float Center, FramePosition Start, FramePosition End) {
    public static Metering Default { get; } = new(0f, 1f, 0f, FramePosition.TopLeft, FramePosition.BottomRight);

    public Fin<float> Metered(PixelFrame frame, Gamut gamut) {
        ((double X, double Y) start, (double X, double Y) end) = (Start.Point(frame.Extent), End.Point(frame.Extent));
        (long Left, long Top, long Right, long Bottom) window = (
            long.Max((long)double.Round(start.X, MidpointRounding.ToEven), frame.Window.Left),
            long.Max((long)double.Round(start.Y, MidpointRounding.ToEven), frame.Window.Top),
            long.Min((long)double.Round(end.X, MidpointRounding.ToEven), frame.Window.Right),
            long.Min((long)double.Round(end.Y, MidpointRounding.ToEven), frame.Window.Bottom));
        (double width, double height, float border, (double R, double G, double B) luminance) = (window.Right - window.Left, window.Bottom - window.Top, 1f - Center, gamut.Luminance);
        (double middleX, double middleY) = (window.Left + (width / 2d), window.Top + (height / 2d));
        return LuminanceSamples.Sorted(frame, window, rgb => LuminanceSamples.Luminance(rgb.X, rgb.Y, rgb.Z, luminance),
                (key, pixel) => (Key: key, Weight: double.Abs(pixel.X + 0.5d - middleX) < width / 4d && double.Abs(pixel.Y + 0.5d - middleY) < height / 4d ? 1f : border))
            .Bind(samples => Metered(samples, window));
    }

    internal Fin<float> Metered((double Key, float Weight)[] samples, (long Left, long Top, long Right, long Bottom) window) =>
        samples.Length is 0 ? new ImagingFault($"metered region {window} holds no covered finite pixel")
        : LuminanceSamples.Slice(samples, Low, High)
            .Aggregate((Sum: 0d, Weight: 0d), static (total, sample) =>
                (total.Sum + (sample.Weight * double.Log2(double.Max(sample.Key, LuminanceSamples.Floor))), total.Weight + sample.Weight)) switch {
                    (var sum, > 0d and var weight) => (float)(double.Log2(Exposure.MiddleGray) - (sum / weight)),
                    _ => new ImagingFault($"central half of {window} holds no sample in meter range at full center weight"),
                };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Exposure {
    public const float MiddleGray = 0.18f;

    public static LutTable.Affine Shifted(float stops) => new(Matrix<double>.Build.DenseDiagonal(3, double.Exp2(stops)));
}

public static class Peak {
    public const float Bt2390 = 0.5f;

    public static Fin<Luminance> Measured(PixelFrame frame, Luminance white) =>
        LuminanceSamples.Greatest(frame).Map(samples => white * samples.FrameWhite);

    public static Option<Action<Span<Vector4>>> Rolled(Transfer.Pq signal, Luminance source, Luminance target, float black, float knee) {
        Luminance fitted = target < source ? target : source;
        Vector4 levels = new(new Vector3((float)(source / signal.White).ToDouble(), (float)(fitted / signal.White).ToDouble(), black * (float)(fitted / signal.White).ToDouble()), 1f);
        signal.Encode(new Span<Vector4>(ref levels));
        (float ceiling, float peak, float floor) = (levels.X, levels.Y / levels.X, levels.Z / levels.X);
        (float start, float power) = (((1f + knee) * peak) - knee, float.Min(1f / floor, 4f));
        float gain = 1f / (1f + (floor / peak * float.Pow(1f - peak, power)));
        CubicSpline rolloff = CubicSpline.InterpolateHermiteSorted([0d, 1d], [start, peak], [1d - start, 0d]);
        return source > target || black is not 0f
            ? Some<Action<Span<Vector4>>>(row => {
                signal.Encode(row);
                foreach (ref Vector4 pixel in row)
                    pixel = new Vector4(Mapped(pixel.X), Mapped(pixel.Y), Mapped(pixel.Z), pixel.W);
                signal.Decode(row);
            })
            : None;

        float Mapped(float code) =>
            float.Clamp(code / ceiling, 0f, 1f) switch {
                var x when x > start => (float)rolloff.Interpolate((x - start) / (1d - start)),
                var x => x,
            } switch {
                var x => float.Clamp((gain * (x + (floor * float.Pow(1f - x, power)) - floor)) + floor, floor, peak) * ceiling,
            };
    }
}
