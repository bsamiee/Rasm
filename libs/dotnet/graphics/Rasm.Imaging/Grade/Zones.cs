using System.Numerics;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GradingRgbmsw(float Red, float Green, float Blue, float Master, float Start, float Width) {
    internal static GradingRgbmsw Placed(float start, float width) => new(1f, 1f, 1f, 1f, start, width);
}

public sealed record GradingTone(GradingRgbmsw Blacks, GradingRgbmsw Shadows, GradingRgbmsw Midtones, GradingRgbmsw Highlights, GradingRgbmsw Whites, float SContrast) {
    public Option<Action<Span<Vector4>>> Graded(GradingStyle style) {
        Curve Shadow(float value) => Bent(value, slope => Spline.Bend(Shadows.Start - Shadows.Width, Shadows.Start, slope, 1f));
        Curve Highlight(float value) => Bent(2f - value, slope => Spline.Bend(Highlights.Start, Highlights.Start + Highlights.Width, 1f, slope));
        Zone Each(GradingRgbmsw zone, Func<float, Curve> curve) => new(curve(zone.Red), curve(zone.Green), curve(zone.Blue), curve(zone.Master));
        (Curve lift, Curve sink, Curve contrast) = (Highlight(Highlights.Master), Shadow(Shadows.Master), Contrasted(style, SContrast));
        Zone[] zones = [.. toSeq([
            Each(Midtones, value => Middle(style, value, Midtones.Start, Midtones.Width)),
            Each(Highlights, Highlight),
            Each(Whites, value => White(lift.At(Whites.Start), lift.At(Whites.Start + Whites.Width), value)),
            Each(Shadows, Shadow),
            Each(Blacks, value => Black(sink.At(Blacks.Start - Blacks.Width), sink.At(Blacks.Start), value)),
            new Zone(contrast, contrast, contrast, Curve.Kept)]).Filter(static zone => !zone.Kept)];
        return zones.Length is 0 ? None : Some(Zoned);

        void Zoned(Span<Vector4> row) {
            foreach (ref Vector4 pixel in row) {
                Vector3 rgb = new(style.Encode(pixel.X), style.Encode(pixel.Y), style.Encode(pixel.Z));
                foreach (Zone zone in zones)
                    rgb = new Vector3(zone.Master.At(zone.Red.At(rgb.X)), zone.Master.At(zone.Green.At(rgb.Y)), zone.Master.At(zone.Blue.At(rgb.Z)));
                pixel = new Vector4(Vector3.Min(new Vector3(style.Decode(rgb.X), style.Decode(rgb.Y), style.Decode(rgb.Z)), new Vector3((float)Half.MaxValue)), pixel.W);
            }
        }
    }

    private static Curve Bent(float value, Func<float, Spline> spline) =>
        value switch { 1f => Curve.Kept, < 1f => new Curve.Forward(spline(value)), _ => new Curve.Inverse(spline(2f - value)) };

    private static Curve Middle(GradingStyle style, float value, float start, float width) =>
        value is 1f
            ? Curve.Kept
            : float.Min(width, (style.Top - style.Bottom) * 0.95f) switch {
                var span => new Curve.Forward(Balanced(style, float.Clamp(start, style.Bottom + (span * 0.51f), style.Top - (span * 0.51f)), span, (value - 1f) * 0.9f)),
            };

    private static Spline Balanced(GradingStyle style, float center, float span, float lean) {
        (float x0, float x1, float x2, float x3, float x4, float x5) = (style.Bottom, center - (span * 0.5f), center - (span * 0.25f), center + (span * 0.25f), center + (span * 0.5f), style.Top);
        (float m1, float m2, float m3, float m4) = (1f + (lean * 0.4f), 1f + lean, 1f - lean, 1f - (lean * 0.4f));
        (float low, float high) = center <= (x5 + x0) * 0.5f
            ? (m1, (((x1 - x0) * (m1 - 1f) * 0.5f) + ((x2 - x1) * (m1 - 1f + ((m2 - m1) * 0.5f))) + ((center - x2) * (m2 - 1f) * 0.5f)
                - (0.5f * (x5 - x4)) + ((x4 - x3) * ((0.5f * m3) - 1f)) + ((x3 - center) * (m3 - 1f) * 0.5f)) / (-0.5f * (x5 - x3)))
            : ((((x5 - x4) * (m4 - 1f) * 0.5f) + ((x4 - x3) * (m4 - 1f + ((m3 - m4) * 0.5f))) + ((x3 - center) * (m3 - 1f) * 0.5f)
                - (0.5f * (x1 - x0)) + ((x2 - x1) * ((0.5f * m2) - 1f)) + ((center - x2) * (m2 - 1f) * 0.5f)) / (-0.5f * (x2 - x0)), m4);
        return Spline.Of((x0, x0, 1f), [(x1, low), (x2, m2), (x3, m3), (x4, high), (x5, 1f)]);
    }

    private static Curve White(float x0, float x1, float value) =>
        value switch { 1f => Curve.Kept, < 1f => new Curve.Forward(Spline.Of((x0, x0, 1f), [(x1, value)])), _ => Tail(Spline.Of((x0, x0, 1f), [(x1, 2f - value)]), x0, x1, 2f - value) };

    private static Curve.Tailed Tail(Spline spline, float x0, float x1, float slope) {
        (float xd, float gain) = (x0 + ((x1 - x0) * 0.99f), (1f + slope) * 0.5f);
        float a = 0.5f * ((1f / slope) - (1f / (1f + ((xd - x0) * (slope - 1f) / (x1 - x0))))) / (x1 - xd);
        float b = (1f / slope) - (2f * a * x1);
        return new(new Curve.Gained(spline, x0, gain), x1, a, b, ((x1 - x0) / gain) + x0 - (b * x1) - (a * x1 * x1));
    }

    private static Curve Black(float x0, float x1, float value) =>
        value switch {
            1f => Curve.Kept,
            > 1f => new Curve.Forward(Spline.Of((x0, x1 - ((3f - value) * 0.5f * (x1 - x0)), 2f - value), [(x1, 1f)])),
            _ => ((1f + value) * 0.5f) switch { var gain => new Curve.Gained(Spline.Of((x0, x1 - (gain * (x1 - x0)), value), [(x1, 1f)]), x1, gain) },
        };

    private static Curve Contrasted(GradingStyle style, float contrast) =>
        contrast is 1f
            ? Curve.Kept
            : (contrast > 1f ? 1f / (1.8125f - (0.8125f * contrast)) : 0.28125f + (0.71875f * contrast)) switch {
                var slope => (Lower(style, slope), Upper(style, slope)) switch {
                    var (low, high) => new Curve.Forward(Spline.Of((low.X1, low.Y2 - ((low.Slope + slope) * (low.X2 - low.X1) * 0.5f), low.Slope), [(low.X2, slope), (high.X1, slope), (high.X2, high.Slope)])),
                },
            };

    private static (float X1, float X2, float Slope) Upper(GradingStyle style, float slope) =>
        ((style.ContrastTop - style.Pivot) * 0.25f) switch {
            var reach => (style.Pivot + reach, style.Pivot + (reach / slope)) switch {
                var (y0, x0) => ((2f * (style.ContrastTop - y0 - (style.ContrastTop / slope) + (slope * x0)) / (slope - (1f / slope))) - x0, (style.ContrastTop - x0) * 0.3f) switch {
                    var (x2, least) when x2 - x0 >= least => (x0, x2, 1f / slope),
                    var (_, least) => (x0, x0 + least, (style.ContrastTop - y0 + (slope * x0) - ((x0 + (least * 0.5f)) * slope)) / (style.ContrastTop - x0 - (least * 0.5f))),
                },
            },
        };

    private static (float X1, float X2, float Slope, float Y2) Lower(GradingStyle style, float slope) =>
        ((style.Pivot - style.Bottom) * 0.25f) switch {
            var reach => (style.Pivot - reach, style.Pivot - (reach / slope)) switch {
                var (y3, x3) => ((2f * (y3 - style.Bottom - (slope * x3) + (style.Bottom / slope)) / ((1f / slope) - slope)) - x3, (x3 - style.Bottom) * 0.3f) switch {
                    var (x1, least) when x3 - x1 >= least => (x1, x3, 1f / slope, y3),
                    var (_, least) => (x3 - least, x3, (y3 - style.Bottom - (slope * x3) + ((x3 - (least * 0.5f)) * slope)) / (x3 - (least * 0.5f) - style.Bottom), y3),
                },
            },
        };

    private sealed record Spline(ImmutableArray<float> X, ImmutableArray<float> Y, ImmutableArray<float> M) {
        public static Spline Of((float X, float Y, float M) first, Seq<(float X, float M)> rest) =>
            rest.Scan(first, static (knot, next) => (next.X, knot.Y + ((knot.M + next.M) * (next.X - knot.X) * 0.5f), next.M)) switch {
                var knots => new([.. knots.Map(static knot => knot.X)], [.. knots.Map(static knot => knot.Y)], [.. knots.Map(static knot => knot.M)]),
            };

        public static Spline Bend(float low, float high, float lowSlope, float highSlope) =>
            Of((low, low, lowSlope), [(low + ((high - low) * 0.5f), 2f - ((lowSlope + highSlope) * 0.5f)), (high, highSlope)]);

        public float Forward(float t) =>
            Segment(X, t) switch {
                -1 => Y[0] + ((t - X[0]) * M[0]),
                var at when at == X.Length - 1 => Y[at] + ((t - X[at]) * M[at]),
                var at => (t - X[at]) switch {
                    var d => Y[at] + (d * (M[at] + (0.5f * (M[at + 1] - M[at]) * d / (X[at + 1] - X[at])))),
                },
            };

        public float Inverse(float t) =>
            Segment(Y, t) switch {
                -1 => X[0] + ((t - Y[0]) / M[0]),
                var at when at == Y.Length - 1 => X[at] + ((t - Y[at]) / M[at]),
                var at => (X[at + 1] - X[at], M[at + 1] - M[at], Y[at] - t) switch {
                    var (h, rise, c) => X[at] + (-2f * c / (float.Sqrt((M[at] * h * M[at] * h) - (2f * rise * h * c)) + (M[at] * h)) * h),
                },
            };

        public float Gained(float t, float anchor, float gain) => ((Inverse(((t - anchor) * gain) + anchor) - anchor) / gain) + anchor;

        private static int Segment(ImmutableArray<float> axis, float t) =>
            axis.AsSpan().BinarySearch(t) switch {
                >= 0 and var at => at,
                var after => ~after - 1,
            };
    }

    private sealed record Zone(Curve Red, Curve Green, Curve Blue, Curve Master) { internal bool Kept => Red == Curve.Kept && Green == Curve.Kept && Blue == Curve.Kept && Master == Curve.Kept; }

    private abstract record Curve {
        internal static Curve Kept { get; } = new Same();

        internal abstract float At(float t);

        internal sealed record Forward(Spline Spline) : Curve { internal override float At(float t) => Spline.Forward(t); }
        internal sealed record Inverse(Spline Spline) : Curve { internal override float At(float t) => Spline.Inverse(t); }
        internal sealed record Gained(Spline Spline, float Anchor, float Gain) : Curve { internal override float At(float t) => Spline.Gained(t, Anchor, Gain); }
        internal sealed record Tailed(Gained Body, float End, float A, float B, float C) : Curve { internal override float At(float t) => t >= End ? (((A * t) + B) * t) + C : Body.At(t); }
        private sealed record Same : Curve { internal override float At(float t) => t; }
    }
}

[SmartEnum]
public sealed partial class GradingStyle {
    public static readonly GradingStyle Log = new(
        1f, 1f, 0f, 0.4f, stops: false,
        new GradingTone(GradingRgbmsw.Placed(0.4f, 0.4f), GradingRgbmsw.Placed(0.5f, 0.5f), GradingRgbmsw.Placed(0.4f, 0.6f), GradingRgbmsw.Placed(0.3f, 0.7f), GradingRgbmsw.Placed(0.4f, 0.5f), 1f));
    public static readonly GradingStyle Linear = new(
        7.5f, 6.5f, -5.5f, 0f, stops: true,
        new GradingTone(GradingRgbmsw.Placed(0f, 4f), GradingRgbmsw.Placed(2f, 9f), GradingRgbmsw.Placed(0f, 8f), GradingRgbmsw.Placed(-2f, 11f), GradingRgbmsw.Placed(0f, 8f), 1f));

    internal float Top { get; }
    internal float ContrastTop { get; }
    internal float Bottom { get; }
    internal float Pivot { get; }
    internal bool Stops { get; }
    public GradingTone Neutral { get; }

    internal float Encode(float value) => Stops ? (float)CurveAxis.ToStops(value) : value;

    internal float Decode(float value) => Stops ? (float)CurveAxis.FromStops(value) : value;
}
