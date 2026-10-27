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

// --- [TYPES] ---------------------------------------------------------------------------
internal interface ISampleKernel<out T> { internal double Key(Vector3 rgb); internal T Sample(double key, long x, long y); }

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
        Sorted<double, Luminous>(frame, frame.Window, new(gamut.Luminance)).Map(static keys => new LuminanceSamples(keys));

    public static Fin<LuminanceSamples> Greatest(PixelFrame frame) =>
        Sorted<double, Brightest>(frame, frame.Window, default).Map(static keys => new LuminanceSamples(keys));

    public double At(float percentile) => SortedArrayStatistics.QuantileCustom(sorted, percentile, QuantileDefinition.EmpiricalInvCDF);

    public static double Luminance(double red, double green, double blue, (double R, double G, double B) weights) => (weights.R * red) + (weights.G * green) + (weights.B * blue);

    internal static ArraySegment<T> Slice<T>(T[] samples, float low, float high) =>
        (int)double.Min(double.Floor(samples.Length * (double)float.Min(low, high)), samples.Length - 1) switch {
            var start => new ArraySegment<T>(samples)[start..int.Max((int)double.Ceiling(samples.Length * (double)float.Max(low, high)), start + 1)],
        };

    internal static Fin<T[]> Sorted<T, TKernel>(PixelFrame frame, FrameWindow window, TKernel kernel) where TKernel : struct, ISampleKernel<T> {
        Span2D<Vector4> view = frame.View.Span;
        (int width, int height, int count) = ((int)long.Max(window.Right - window.Left, 0L), (int)long.Max(window.Bottom - window.Top, 0L), 0);
        T[] samples = GC.AllocateUninitializedArray<T>(width * height);
        for (long y = window.Top; width > 0 && y < window.Bottom; y++) {
            ReadOnlySpan<Vector4> row = view.GetRowSpan((int)(y - frame.Origin.Y)).Slice((int)(window.Left - frame.Origin.X), width);
            for (int x = 0; x < width; x++) {
                double level = kernel.Key(row[x].AsVector3());
                samples[count] = kernel.Sample(level, window.Left + x, y);
                count += row[x].W > 0f && double.IsFinite(level) ? 1 : 0;
            }
        }
        samples.AsSpan(0, count).Sort();
        return count is 0 ? new ImagingFault($"region {window}", ImagingReason.Empty) : samples[..count];
    }
}

public sealed record Metering(float Low, float High, float Center, FramePosition Start, FramePosition End) {
    public static Metering Default { get; } = new(0f, 1f, 0f, FramePosition.TopLeft, FramePosition.BottomRight);

    public Fin<float> Metered(PixelFrame frame, Gamut gamut) {
        ((double X, double Y) start, (double X, double Y) end) = (Start.Point(frame.Extent), End.Point(frame.Extent));
        FrameWindow window = new(
            long.Max((long)double.Round(start.X, MidpointRounding.ToEven), frame.Window.Left),
            long.Max((long)double.Round(start.Y, MidpointRounding.ToEven), frame.Window.Top),
            long.Min((long)double.Round(end.X, MidpointRounding.ToEven), frame.Window.Right),
            long.Min((long)double.Round(end.Y, MidpointRounding.ToEven), frame.Window.Bottom));
        return LuminanceSamples.Sorted<(double Key, float Weight), Centered>(frame, window, new(gamut.Luminance, window, 1f - Center)).Bind(samples => Metered(samples, window));
    }

    internal Fin<float> Metered((double Key, float Weight)[] samples, FrameWindow window) =>
        samples.Length is 0 ? new ImagingFault($"metered region {window}", ImagingReason.Empty)
        : LuminanceSamples.Slice(samples, Low, High)
            .Aggregate((Sum: 0d, Weight: 0d), static (total, sample) =>
                (total.Sum + (sample.Weight * double.Log2(double.Max(sample.Key, LuminanceSamples.Floor))), total.Weight + sample.Weight)) switch {
                    (var sum, > 0d and var weight) => (float)(double.Log2(Exposure.MiddleGray) - (sum / weight)),
                    _ => new ImagingFault($"central half of {window} at full center weight", ImagingReason.Empty),
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

file readonly struct Luminous((double R, double G, double B) luminance) : ISampleKernel<double> { double ISampleKernel<double>.Key(Vector3 rgb) => Gamut.RelativeLuminance(rgb.X, rgb.Y, rgb.Z, luminance); double ISampleKernel<double>.Sample(double key, long x, long y) => key; }
file readonly struct Brightest : ISampleKernel<double> { double ISampleKernel<double>.Key(Vector3 rgb) => float.Max(rgb.X, float.Max(rgb.Y, rgb.Z)); double ISampleKernel<double>.Sample(double key, long x, long y) => key; }
file readonly struct Centered((double R, double G, double B) luminance, FrameWindow window, float border) : ISampleKernel<(double Key, float Weight)> {
    private readonly (double X, double Y, double Across, double Down) center = ((double)(window.Right - window.Left), (double)(window.Bottom - window.Top)) switch { var (width, height) => (window.Left + (width / 2d), window.Top + (height / 2d), width / 4d, height / 4d) };
    double ISampleKernel<(double Key, float Weight)>.Key(Vector3 rgb) => Gamut.RelativeLuminance(rgb.X, rgb.Y, rgb.Z, luminance);
    (double Key, float Weight) ISampleKernel<(double Key, float Weight)>.Sample(double key, long x, long y) =>
        (key, double.Abs(x + 0.5d - center.X) < center.Across && double.Abs(y + 0.5d - center.Y) < center.Down ? 1f : border);
}
internal readonly struct Placed((double R, double G, double B) luminance, PixelFrame frame) : ISampleKernel<(double Key, int Y, int X)> { double ISampleKernel<(double Key, int Y, int X)>.Key(Vector3 rgb) => double.Max(Gamut.RelativeLuminance(rgb.X, rgb.Y, rgb.Z, luminance), LuminanceSamples.Floor); (double Key, int Y, int X) ISampleKernel<(double Key, int Y, int X)>.Sample(double key, long x, long y) => (key, (int)(y - frame.Origin.Y), (int)(x - frame.Origin.X)); }
