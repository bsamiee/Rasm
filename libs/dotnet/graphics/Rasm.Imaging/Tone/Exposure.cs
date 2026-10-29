using System.Drawing;
using System.Numerics;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
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

    private LuminanceSamples(double[] sorted) => this.sorted = sorted;

    public double FrameWhite => At(WhitePercentile);
    public (double Mean, double Maximum, double Minimum) LogStatistics {
        get {
            using SpanOwner<double> owner = SpanOwner<double>.Allocate(sorted.Length);
            Span<double> logs = owner.Span;
            TensorPrimitives.Max(sorted, Floor, logs);
            TensorPrimitives.Log2(logs, logs);
            return (TensorPrimitives.Sum(logs) / logs.Length, logs[^1], logs[0]);
        }
    }

    public static Fin<LuminanceSamples> Gather(PixelFrame frame, Gamut gamut) => Of(Plane(frame, frame.Window, gamut.Luminance), frame.Window);

    public static Fin<LuminanceSamples> Greatest(PixelFrame frame) => Of(Plane(frame, frame.Window, None), frame.Window);

    public double At(float percentile) => SortedArrayStatistics.QuantileCustom(sorted, percentile, QuantileDefinition.EmpiricalInvCDF);

    public Luminance White(Luminance reference) => reference * FrameWhite;

    internal static Fin<LuminanceSamples> Of(double[] plane, Rectangle window) =>
        plane.Where(double.IsFinite).Order().ToArray() switch {
            [] => ImagingFault.Of(ImagingReason.Empty, window),
            var kept => new LuminanceSamples(kept),
        };

    internal static double[] Plane(PixelFrame frame, Rectangle region, Option<Vector3> weights) {
        (Vector3 luminance, bool greatest) = weights.Match(Some: static stated => (stated, false), None: static () => (Vector3.Zero, true));
        Rectangle window = Rectangle.Intersect(region, frame.Window);
        double[] plane = GC.AllocateUninitializedArray<double>(window.Width * window.Height);
        Span2D<double> levels = new(plane, window.Height, window.Width);
        for (int y = 0; y < window.Height; y++) {
            ReadOnlySpan<Vector4> row = frame.View.Span.GetRowSpan(window.Top + y - frame.Origin.Y).Slice(window.Left - frame.Origin.X, window.Width);
            Span<double> line = levels.GetRowSpan(y);
            for (int x = 0; x < row.Length; x++) {
                float level = greatest ? float.Max(row[x].X, float.Max(row[x].Y, row[x].Z)) : Vector3.Dot(row[x].AsVector3(), luminance);
                line[x] = row[x].W > 0f && float.IsFinite(level) ? level : double.NaN;
            }
        }
        return plane;
    }
}

public sealed record Metering(float Low, float High, float Center, Vector2 Start, Vector2 End) {
    public const float MiddleGray = 0.18f;

    public static Metering Default { get; } = new(0f, 1f, 0f, Vector2.Zero, Vector2.One);

    public Rectangle Window(PixelFrame frame) =>
        (Vector2.Round(frame.Extent.Point(Start)), Vector2.Round(frame.Extent.Point(End))) switch {
            var (start, end) => Rectangle.Intersect(Rectangle.FromLTRB((int)start.X, (int)start.Y, (int)end.X, (int)end.Y), frame.Window),
        };

    public Fin<float> Metered(PixelFrame frame, Gamut gamut) {
        Rectangle window = Window(frame);
        double[] plane = LuminanceSamples.Plane(frame, window, gamut.Luminance);
        Rectangle center = Rectangle.Inflate(new Rectangle(0, 0, window.Width, window.Height), -window.Width / 4, -window.Height / 4);
        return LuminanceSamples.Of(plane, window).Bind(samples => {
            ReadOnlySpan2D<double> levels = new(plane, window.Height, window.Width);
            (double low, double high, double border) = (samples.At(Low), samples.At(High), 1d - Center);
            ((double Sum, double Count) all, (double Sum, double Count) inner) = (Logged(levels, low, high), Logged(levels.Slice(center.Top, center.Left, center.Height, center.Width), low, high));
            double weight = inner.Count + (border * (all.Count - inner.Count));
            return weight > 0d
                ? Fin.Succ((float)(double.Log2(MiddleGray) - ((inner.Sum + (border * (all.Sum - inner.Sum))) / weight)))
                : ImagingFault.Of(ImagingReason.Empty, (window, Center));
        });
    }

    private static (double Sum, double Count) Logged(ReadOnlySpan2D<double> levels, double low, double high) {
        (double sum, double count) = (0d, 0d);
        foreach (double level in levels)
            (sum, count) = level >= low && level <= high ? (sum + double.Log2(double.Max(level, LuminanceSamples.Floor)), count + 1d) : (sum, count);
        return (sum, count);
    }
}
