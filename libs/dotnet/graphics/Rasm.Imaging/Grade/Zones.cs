using System.Numerics;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GradingRgbmsw(float Red, float Green, float Blue, float Master, float Start, float Width) {
    internal static GradingRgbmsw Placed(float start, float width) => new(1f, 1f, 1f, 1f, start, width);

    internal bool Neutral => Red is 1f && Green is 1f && Blue is 1f && Master is 1f;

    internal Func<Vector3, Vector3> PerChannel(Func<float, Func<float, float>> curve) =>
        (curve(Red), curve(Green), curve(Blue), curve(Master)) switch {
            var (red, green, blue, master) => rgb => new Vector3(master(red(rgb.X)), master(green(rgb.Y)), master(blue(rgb.Z))),
        };
}

public sealed record GradingTone(GradingRgbmsw Blacks, GradingRgbmsw Shadows, GradingRgbmsw Midtones, GradingRgbmsw Highlights, GradingRgbmsw Whites, float SContrast) {
    private static readonly Func<float, float> Identity = static value => value;

    public Option<Action<Span<Vector4>>> Graded(GradingStyle style) =>
        toSeq([Blacks, Shadows, Midtones, Highlights, Whites]).ForAll(static zone => zone.Neutral) && SContrast is 1f
            ? None
            : Composed(style);

    private Action<Span<Vector4>> Composed(GradingStyle style) {
        Func<float, float> Shadow(float value) => Bent(value, slope => Spline.Bend(Shadows.Start - Shadows.Width, Shadows.Start, slope, 1f));
        Func<float, float> Highlight(float value) => Bent(2f - value, slope => Spline.Bend(Highlights.Start, Highlights.Start + Highlights.Width, 1f, slope));
        (Func<float, float> lift, Func<float, float> sink, Func<float, float> contrast) = (Highlight(Highlights.Master), Shadow(Shadows.Master), Contrasted(style, SContrast));
        Seq<Func<Vector3, Vector3>> zones = [
            Midtones.PerChannel(value => Middle(style, value, Midtones.Start, Midtones.Width)),
            Highlights.PerChannel(Highlight),
            Whites.PerChannel(value => White(lift(Whites.Start), lift(Whites.Start + Whites.Width), value)),
            Shadows.PerChannel(Shadow),
            Blacks.PerChannel(value => Black(sink(Blacks.Start - Blacks.Width), sink(Blacks.Start), value)),
            rgb => new Vector3(contrast(rgb.X), contrast(rgb.Y), contrast(rgb.Z)),
        ];
        return row => {
            foreach (ref Vector4 pixel in row) {
                Vector3 rgb = new(style.Encode(pixel.X), style.Encode(pixel.Y), style.Encode(pixel.Z));
                foreach (Func<Vector3, Vector3> zone in zones.AsSpan())
                    rgb = zone(rgb);
                pixel = new Vector4(Vector3.Min(new Vector3(style.Decode(rgb.X), style.Decode(rgb.Y), style.Decode(rgb.Z)), new Vector3((float)Half.MaxValue)), pixel.W);
            }
        };
    }

    private static Func<float, float> Bent(float value, Func<float, Spline> spline) =>
        value switch {
            1f => Identity,
            < 1f => spline(value).Forward,
            _ => spline(2f - value).Inverse,
        };

    private static Func<float, float> Middle(GradingStyle style, float value, float start, float width) =>
        value is 1f
            ? Identity
            : float.Min(width, (style.Top - style.Bottom) * 0.95f) switch {
                var span => float.Clamp(start, style.Bottom + (span * 0.51f), style.Top - (span * 0.51f)) switch {
                    var center => Balanced(style, center, span, (value - 1f) * 0.9f).Forward,
                },
            };

    private static Spline Balanced(GradingStyle style, float center, float span, float lean) =>
        (style.Bottom, center - (span * 0.5f), center - (span * 0.25f), center + (span * 0.25f), center + (span * 0.5f), style.Top, 1f + (lean * 0.4f), 1f + lean, 1f - lean, 1f - (lean * 0.4f)) switch {
            var (x0, x1, x2, x3, x4, x5, m1, m2, m3, m4) => (center <= (x5 + x0) * 0.5f
                ? (m1, (((x1 - x0) * (m1 - 1f) * 0.5f) + ((x2 - x1) * (m1 - 1f + ((m2 - m1) * 0.5f))) + ((center - x2) * (m2 - 1f) * 0.5f)
                        - (0.5f * (x5 - x4)) + ((x4 - x3) * ((0.5f * m3) - 1f)) + ((x3 - center) * (m3 - 1f) * 0.5f)) / (-0.5f * (x5 - x3)))
                : ((((x5 - x4) * (m4 - 1f) * 0.5f) + ((x4 - x3) * (m4 - 1f + ((m3 - m4) * 0.5f))) + ((x3 - center) * (m3 - 1f) * 0.5f)
                        - (0.5f * (x1 - x0)) + ((x2 - x1) * ((0.5f * m2) - 1f)) + ((center - x2) * (m2 - 1f) * 0.5f)) / (-0.5f * (x2 - x0)), m4)) switch {
                            var (low, high) => Spline.Of((x0, x0, 1f), [(x1, low), (x2, m2), (x3, m3), (x4, high), (x5, 1f)]),
                        },
        };

    private static Func<float, float> White(float x0, float x1, float value) =>
        (Spline.Of((x0, x0, 1f), [(x1, float.Min(value, 2f - value))]), Tail(x0, x1, 2f - value)) switch {
            _ when value is 1f => Identity,
            var (spline, _) when value < 1f => spline.Forward,
            var (spline, (a, b, c, gain)) => t => t >= x1 ? (((a * t) + b) * t) + c : spline.Gained(t, x0, gain),
        };

    private static (float A, float B, float C, float Gain) Tail(float x0, float x1, float slope) =>
        (x0 + ((x1 - x0) * 0.99f), (1f + slope) * 0.5f) switch {
            var (xd, gain) => (0.5f * ((1f / slope) - (1f / (1f + ((xd - x0) * (slope - 1f) / (x1 - x0))))) / (x1 - xd)) switch {
                var a => ((1f / slope) - (2f * a * x1)) switch { var b => (a, b, ((x1 - x0) / gain) + x0 - (b * x1) - (a * x1 * x1), gain) },
            },
        };

    private static Func<float, float> Black(float x0, float x1, float value) =>
        value switch {
            1f => Identity,
            > 1f => Spline.Of((x0, x1 - ((3f - value) * 0.5f * (x1 - x0)), 2f - value), [(x1, 1f)]).Forward,
            _ => ((1f + value) * 0.5f) switch {
                var gain => Spline.Of((x0, x1 - (gain * (x1 - x0)), value), [(x1, 1f)]) switch {
                    var spline => t => spline.Gained(t, x1, gain),
                },
            },
        };

    private static Func<float, float> Contrasted(GradingStyle style, float contrast) =>
        contrast is 1f
            ? Identity
            : (contrast > 1f ? 1f / (1.8125f - (0.8125f * contrast)) : 0.28125f + (0.71875f * contrast)) switch {
                var slope => (Lower(style, slope), Upper(style, slope)) switch {
                    var (low, high) => Spline.Of((low.X1, low.Y2 - ((low.Slope + slope) * (low.X2 - low.X1) * 0.5f), low.Slope), [(low.X2, slope), (high.X1, slope), (high.X2, high.Slope)]).Forward,
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
}

[SmartEnum]
public sealed partial class GradingStyle {
    public static readonly GradingStyle Log = new(
        1f, 1f, 0f, 0.4f,
        new GradingTone(GradingRgbmsw.Placed(0.4f, 0.4f), GradingRgbmsw.Placed(0.5f, 0.5f), GradingRgbmsw.Placed(0.4f, 0.6f), GradingRgbmsw.Placed(0.3f, 0.7f), GradingRgbmsw.Placed(0.4f, 0.5f), 1f),
        static value => value,
        static value => value);
    public static readonly GradingStyle Linear = new(
        7.5f, 6.5f, -5.5f, 0f,
        new GradingTone(GradingRgbmsw.Placed(0f, 4f), GradingRgbmsw.Placed(2f, 9f), GradingRgbmsw.Placed(0f, 8f), GradingRgbmsw.Placed(-2f, 11f), GradingRgbmsw.Placed(0f, 8f), 1f),
        static value => (float)CurveAxis.ToStops(value),
        static stops => (float)CurveAxis.FromStops(stops));

    internal float Top { get; }
    internal float ContrastTop { get; }
    internal float Bottom { get; }
    internal float Pivot { get; }
    public GradingTone Neutral { get; }

    [UseDelegateFromConstructor]
    internal partial float Encode(float value);

    [UseDelegateFromConstructor]
    internal partial float Decode(float value);
}
