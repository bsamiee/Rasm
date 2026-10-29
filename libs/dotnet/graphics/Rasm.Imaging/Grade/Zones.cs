using System.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Color;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public abstract partial class GradingStyle {
    public static readonly GradingStyle Log = new Encoded("log", 4f, new GradingTone(new(0.4f, 0.4f), new(0.5f, 0f), new(0.4f, 0.6f), new(0.3f, 1f), new(0.4f, 0.5f)));
    public static readonly GradingStyle Linear = new SceneLinear();
    public static readonly GradingStyle Video = new Encoded("video", 1.25f, new GradingTone(new(0.4f, 0.4f), new(0.6f, 0f), new(0.4f, 0.7f), new(0.2f, 1f), new(0.5f, 0.5f)));

    private static readonly Vector3 Luma = Gamut.StandardRgb.Luminance;

    internal float Top { get; }
    internal float ContrastTop { get; }
    internal float Bottom { get; }
    internal float Pivot { get; }
    internal float Low { get; }
    internal float High { get; }
    internal GradingTone Tone { get; }

    internal virtual Vector3 Encode(Vector3 linear) => linear;

    internal virtual Vector3 Decode(Vector3 encoded) => encoded;

    public Vector3 ToHsy(Vector3 rgb) {
        (float luma, float high, float low) = (Vector3.Dot(rgb, Luma), float.Max(rgb.X, float.Max(rgb.Y, rgb.Z)), float.Min(rgb.X, float.Min(rgb.Y, rgb.Z)));
        float hue = high == low ? 0f
            : (rgb.X == high ? 1f + ((rgb.Y - rgb.Z) / (high - low)) : rgb.Y == high ? 3f + ((rgb.Z - rgb.X) / (high - low)) : 5f + ((rgb.X - rgb.Y) / (high - low))) / 6f;
        return new(hue, Saturation(Vector3.Sum(rgb), luma, Vector3.Sum(Vector3.Abs(rgb - new Vector3(luma)))), luma);
    }

    public Vector3 FromHsy(Vector3 hsy) {
        float turn = hsy.X - (1f / 6f) + (hsy.Z < 0f ? 0.5f : 0f);
        float sextant = (turn - float.Floor(turn)) * 6f;
        Vector3 hexcone = Vector3.Clamp(new(float.Abs(sextant - 3f) - 1f, 2f - float.Abs(sextant - 2f), 2f - float.Abs(sextant - 4f)), Vector3.Zero, Vector3.One);
        Vector3 chroma = (hexcone * (hsy.Z / Vector3.Dot(hexcone, Luma))) - new Vector3(hsy.Z);
        return new Vector3(hsy.Z) + (Gain(hsy.Y, Vector3.Sum(chroma) + (3f * hsy.Z), hsy.Z, Vector3.Sum(Vector3.Abs(chroma))) * chroma);
    }

    private protected abstract float Saturation(float sum, float luma, float distance);

    private protected abstract float Gain(float saturation, float sum, float luma, float distance);

    private sealed class Encoded(string key, float scale, GradingTone tone) : GradingStyle(key, 1f, 1f, 0f, 0.4f, 0f, 1f, tone) {
        private protected override float Saturation(float sum, float luma, float distance) => distance * scale;

        private protected override float Gain(float saturation, float sum, float luma, float distance) => saturation / float.Max(distance * scale, 1e-10f);
    }

    private sealed class SceneLinear() : GradingStyle("linear", 7.5f, 6.5f, -5.5f, 0f, -7f, 7f, new GradingTone(new(0f, 4f), new(2f, -7f), new(0f, 8f), new(-2f, 9f), new(0f, 8f))) {
        private const float Shift = -0.000157849851665374f;
        private const float Break = 0.0041318374739483946f;
        private const float Knee = 0.15f;
        private const float LowGain = 5f;
        private const float Scale = 1.4f;
        private const float MaxLuma = 0.01f;

        private static readonly LogCurve LinLog = LogCurve.Of(2f, Vector3.One, new(-float.Log2(Metering.MiddleGray + Shift)), Vector3.One, new(Shift), new(Break));

        internal override Vector3 Encode(Vector3 linear) => LinLog.Forward(linear);

        internal override Vector3 Decode(Vector3 encoded) => LinLog.Inverse(encoded);

        private protected override float Saturation(float sum, float luma, float distance) =>
            Scale * float.Lerp(distance * LowGain, distance / float.Max((0.07f * distance) + 1e-6f, Knee + sum), Blend(luma));

        private protected override float Gain(float saturation, float sum, float luma, float distance) =>
            (saturation / Scale, Blend(luma), Knee + (3f * luma), sum - (3f * luma)) switch {
                (var scaled, 1f, var knee, var excess) => float.Min(scaled * knee / float.Max(distance - (scaled * excess), 1e-6f), 50f),
                (var scaled, 0f, _, _) => scaled / float.Max(distance * LowGain, 1e-10f),
                var (scaled, alpha, knee, excess) => (distance * LowGain * (1f - alpha) * excess, (distance * LowGain * (1f - alpha) * knee) + (distance * alpha) - (scaled * excess), -scaled * knee) switch {
                    var (a, b, c) => float.Sqrt((b * b) - (4f * a * c)) switch {
                        var root => (2f * c / (-root - b)) switch { >= 0f and var gain => gain, _ => 2f * c / (root - b) },
                    },
                },
            };

        private static float Blend(float luma) => float.Clamp((luma - (0.1f * MaxLuma)) / (0.9f * MaxLuma), 0f, 1f);
    }
}

public sealed record GradingRgbmsw(float Red, float Green, float Blue, float Master, float Start, float Width) {
    public GradingRgbmsw(float start, float width) : this(1f, 1f, 1f, 1f, start, width) { }
}

public sealed record GradingTone(GradingRgbmsw Blacks, GradingRgbmsw Shadows, GradingRgbmsw Midtones, GradingRgbmsw Highlights, GradingRgbmsw Whites, float SContrast = 1f) {
    private static readonly Func<float, float> Kept = static t => t;

    public static GradingTone Neutral(GradingStyle style) => style.Tone;

    public Option<Action<Span<Vector4>>> Graded(GradingStyle style) {
        (float highlightStart, float shadowStart) = (float.Min(Highlights.Start, Highlights.Width - 0.01f), float.Max(Shadows.Start, Shadows.Width + 0.01f));
        Func<float, float> Highlight(float value) => Bent(2f - value, slope => Spline.Bend(highlightStart, Highlights.Width, 1f, slope));
        Func<float, float> Shadow(float value) => Bent(value, slope => Spline.Bend(Shadows.Width, shadowStart, slope, 1f));
        (Func<float, float> lift, Func<float, float> sink) = (Highlight(Highlights.Master), Shadow(Shadows.Master));
        Seq<Func<Vector3, Vector3>> zones = toSeq([
            Zone(Midtones, value => Middle(style, value, Midtones.Start, Midtones.Width)),
            Zone(Highlights, Highlight),
            Zone(Whites, value => White(lift(Whites.Start), lift(Whites.Start + Whites.Width), value)),
            Zone(Shadows, Shadow),
            Zone(Blacks, value => Black(sink(Blacks.Start - Blacks.Width), sink(Blacks.Start), value)),
            SContrast is 1f ? None : Contrasted(style, SContrast) switch { var contrast => Some<Func<Vector3, Vector3>>(rgb => new(contrast(rgb.X), contrast(rgb.Y), contrast(rgb.Z))) }]).Somes();
        return zones.Head.Map(head => zones.Tail.Fold(head, static (inner, outer) => rgb => outer(inner(rgb))))
            .Map(graded => ToneCurve.Graded(color => Vector3.Min(style.Decode(graded(style.Encode(color))), new Vector3((float)Half.MaxValue))));
    }

    private static Option<Func<Vector3, Vector3>> Zone(GradingRgbmsw zone, Func<float, Func<float, float>> curve) =>
        zone is { Red: 1f, Green: 1f, Blue: 1f, Master: 1f }
            ? None
            : (curve(zone.Red), curve(zone.Green), curve(zone.Blue), curve(zone.Master)) switch {
                var (red, green, blue, master) => Some<Func<Vector3, Vector3>>(rgb => new(master(red(rgb.X)), master(green(rgb.Y)), master(blue(rgb.Z)))),
            };

    private static Func<float, float> Bent(float value, Func<float, Spline> spline) =>
        value switch { 1f => Kept, < 1f => spline(float.Max(value, 0.01f)).Forward, _ => spline(float.Max(2f - value, 0.01f)).Inverse };

    private static Func<float, float> Middle(GradingStyle style, float value, float start, float width) {
        (float lean, float span) = ((float.Clamp(value, 0.01f, 1.99f) - 1f) * 0.9f, float.Clamp(width, 0.01f, (style.Top - style.Bottom) * 0.95f));
        float center = float.Clamp(start, style.Bottom + (span * 0.51f), style.Top - (span * 0.51f));
        (float x0, float x1, float x2, float x3, float x4, float x5) = (style.Bottom, center - (span * 0.5f), center - (span * 0.25f), center + (span * 0.25f), center + (span * 0.5f), style.Top);
        (float m1, float m2, float m3, float m4) = (1f + (lean * 0.4f), 1f + lean, 1f - lean, 1f - (lean * 0.4f));
        (float low, float high) = center <= (x5 + x0) * 0.5f
            ? (m1, (((x1 - x0) * (m1 - 1f) * 0.5f) + ((x2 - x1) * (m1 - 1f + ((m2 - m1) * 0.5f))) + ((center - x2) * (m2 - 1f) * 0.5f)
                - (0.5f * (x5 - x4)) + ((x4 - x3) * ((0.5f * m3) - 1f)) + ((x3 - center) * (m3 - 1f) * 0.5f)) / (-0.5f * (x5 - x3)))
            : ((((x5 - x4) * (m4 - 1f) * 0.5f) + ((x4 - x3) * (m4 - 1f + ((m3 - m4) * 0.5f))) + ((x3 - center) * (m3 - 1f) * 0.5f)
                - (0.5f * (x1 - x0)) + ((x2 - x1) * ((0.5f * m2) - 1f)) + ((center - x2) * (m2 - 1f) * 0.5f)) / (-0.5f * (x2 - x0)), m4);
        return lean is 0f ? Kept : Spline.Of((x0, x0, 1f), [(x1, low), (x2, m2), (x3, m3), (x4, high), (x5, 1f)]).Forward;
    }

    private static Func<float, float> White(float x0, float x1, float value) =>
        value switch {
            1f => Kept,
            < 1f => Spline.Of((x0, x0, 1f), [(x1, float.Max(value, 0.01f))]).Forward,
            _ => float.Max(2f - value, 0.01f) switch { var slope => Tail(Spline.Of((x0, x0, 1f), [(x1, slope)]), x0, x1, slope) },
        };

    private static Func<float, float> Tail(Spline spline, float x0, float x1, float slope) {
        (float xd, float gain) = (x0 + ((x1 - x0) * 0.99f), (1f + slope) * 0.5f);
        float a = 0.5f * ((1f / slope) - (1f / (1f + ((xd - x0) * (slope - 1f) / (x1 - x0))))) / (x1 - xd);
        float b = (1f / slope) - (2f * a * x1);
        (Func<float, float> body, float c) = (Gained(spline, x0, gain), ((x1 - x0) / gain) + x0 - (b * x1) - (a * x1 * x1));
        return t => t >= x1 ? (((a * t) + b) * t) + c : body(t);
    }

    private static Func<float, float> Black(float x0, float x1, float value) =>
        value switch {
            1f => Kept,
            > 1f => float.Max(2f - value, 0.01f) switch { var slope => Spline.Of((x0, x1 - ((1f + slope) * 0.5f * (x1 - x0)), slope), [(x1, 1f)]).Forward },
            _ => float.Max(value, 0.01f) switch { var slope => ((1f + slope) * 0.5f) switch { var gain => Gained(Spline.Of((x0, x1 - (gain * (x1 - x0)), slope), [(x1, 1f)]), x1, gain) } },
        };

    private static Func<float, float> Gained(Spline spline, float anchor, float gain) => t => ((spline.Inverse(((t - anchor) * gain) + anchor) - anchor) / gain) + anchor;

    private static Func<float, float> Contrasted(GradingStyle style, float contrast) {
        float slope = contrast > 1f ? 1f / (1.8125f - (0.8125f * float.Min(contrast, 1.99f))) : 0.28125f + (0.71875f * float.Max(contrast, 0.01f));
        ((float X1, float X2, float Slope) low, (float X1, float X2, float Slope) high) = (Upper(style.Pivot, (2f * style.Pivot) - style.Bottom, slope), Upper(style.Pivot, style.ContrastTop, slope));
        (float x1, float x2) = ((2f * style.Pivot) - low.X2, (2f * style.Pivot) - low.X1);
        return Spline.Of((x1, style.Pivot - ((style.Pivot - style.Bottom) * 0.25f) - ((low.Slope + slope) * (x2 - x1) * 0.5f), low.Slope), [(x2, slope), (high.X1, slope), (high.X2, high.Slope)]).Forward;
    }

    private static (float X1, float X2, float Slope) Upper(float pivot, float top, float slope) =>
        (pivot + ((top - pivot) * 0.25f), pivot + ((top - pivot) * 0.25f / slope)) switch {
            var (y0, x0) => ((2f * (top - y0 - (top / slope) + (slope * x0)) / (slope - (1f / slope))) - x0, (top - x0) * 0.3f) switch {
                var (x2, _) when x2 > top => (x2 + x0 - top, top, 1f / slope),
                var (x2, least) when x2 - x0 >= least => (x0, x2, 1f / slope),
                var (_, least) => (x0, x0 + least, (top - y0 + (slope * x0) - ((x0 + (least * 0.5f)) * slope)) / (top - x0 - (least * 0.5f))),
            },
        };

    private sealed class Spline(float[] x, float[] y, float[] m) {
        private readonly QuadraticSpline forward = new(
            [x[0] - 1d, .. x.Select(static knot => (double)knot), x[^1] + 1d],
            [y[0] - m[0], .. y.Select(static knot => (double)knot)],
            [m[0], .. m.Select(static knot => (double)knot)],
            [0d, .. Enumerable.Range(0, x.Length - 1).Select(at => 0.5d * (m[at + 1] - m[at]) / (x[at + 1] - x[at])), 0d]);

        public static Spline Of((float X, float Y, float M) first, Seq<(float X, float M)> rest) =>
            rest.Scan(first, static (knot, next) => (next.X, knot.Y + ((knot.M + next.M) * (next.X - knot.X) * 0.5f), next.M)) switch {
                var knots => new([.. knots.Map(static knot => knot.X)], [.. knots.Map(static knot => knot.Y)], [.. knots.Map(static knot => knot.M)]),
            };

        public static Spline Bend(float low, float high, float lowSlope, float highSlope) =>
            Of((low, low, lowSlope), [(low + ((high - low) * 0.5f), 2f - ((lowSlope + highSlope) * 0.5f)), (high, highSlope)]);

        public float Forward(float t) => (float)forward.Interpolate(t);

        public float Inverse(float t) =>
            y.AsSpan().BinarySearch(t) switch { >= 0 and var at => at, var after => ~after - 1 } switch {
                -1 => x[0] + ((t - y[0]) / m[0]),
                var at when at == y.Length - 1 => x[at] + ((t - y[at]) / m[at]),
                var at => (x[at + 1] - x[at], m[at + 1] - m[at], y[at] - t) switch {
                    var (h, rise, c) => x[at] + (-2f * c / (float.Sqrt((m[at] * h * m[at] * h) - (2f * rise * h * c)) + (m[at] * h)) * h),
                },
            };
    }
}
