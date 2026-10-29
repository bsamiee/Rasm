using System.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Tone;
using SuperLinq;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class PointCurve {
    public Seq<Vector2> Points { get; }

    public Func<float, float> Fit() {
        (IInterpolation spline, Vector2 first, Vector2 last) = (Interpolated(Points), Points[0], Points[^1]);
        (float entry, float exit) = ((float)spline.Differentiate(first.X), (float)spline.Differentiate(last.X));
        return t => t < first.X ? first.Y + (entry * (t - first.X)) : t > last.X ? last.Y + (exit * (t - last.X)) : (float)spline.Interpolate(t);
    }

    internal Func<float, float> Wrapped(float turn) {
        Seq<Vector2> turned = toSeq(Points.Map(point => point - (float.Floor(point.X) * new Vector2(1f, turn))).DistinctBy(static point => point.X).OrderBy(static point => point.X));
        IInterpolation spline = Interpolated([turned[^1] - new Vector2(1f, turn), .. turned, turned[0] + new Vector2(1f, turn)]);
        return t => float.Floor(t) switch { var whole => (whole * turn) + (float)spline.Interpolate(t - whole) };
    }

    public static Fin<PointCurve> Validated(Seq<Vector2> points) =>
        ImagingFault.Unless(points.Count >= 2 && points.AsIterable().Window(2, static pair => pair[0].X < pair[1].X).All(static rising => rising), ImagingReason.Degenerate, points).Map(_ => new PointCurve(points));

    private static IInterpolation Interpolated(Seq<Vector2> points) =>
        (points.Map(static point => (double)point.X).ToArray(), points.Map(static point => (double)point.Y).ToArray()) switch {
            var (xs, ys) when xs.Length > 2 => CubicSpline.InterpolatePchipSorted(xs, ys),
            var (xs, ys) => LinearSpline.InterpolateSorted(xs, ys),
        };
}

public sealed record GradingRgbCurve(PointCurve Red, PointCurve Green, PointCurve Blue, PointCurve Master) {
    public static GradingRgbCurve Neutral(GradingStyle style) => HueCurveKind.LumLum.Neutral(style) switch { var diagonal => new(diagonal, diagonal, diagonal, diagonal) };

    public Option<Action<Span<Vector4>>> Graded(GradingStyle style, float preserveHue) {
        (Func<float, float> red, Func<float, float> green, Func<float, float> blue, Func<float, float> master) = (Red.Fit(), Green.Fit(), Blue.Fit(), Master.Fit());
        return this == Neutral(style) ? None : Some(ToneCurve.Graded(Curved));

        Vector3 Curved(Vector3 color) {
            Vector3 curved = style.Encode(color) switch { var encoded => new Vector3(red(encoded.X), green(encoded.Y), blue(encoded.Z)) };
            return ToneCurve.PreserveHue(style.Decode(curved), style.Decode(new Vector3(master(curved.X), master(curved.Y), master(curved.Z))), preserveHue);
        }
    }
}

[SmartEnum<string>]
public sealed partial class HueCurveKind {
    public static readonly HueCurveKind HueHue = new("hue-hue", 0, 1f, 0f);
    public static readonly HueCurveKind HueSat = new("hue-sat", 0, 0f, 1f);
    public static readonly HueCurveKind HueLum = new("hue-lum", 0, 0f, 1f);
    public static readonly HueCurveKind LumSat = new("lum-sat", 2, 0f, 1f);
    public static readonly HueCurveKind SatSat = new("sat-sat", 1, 1f, 0f);
    public static readonly HueCurveKind LumLum = new("lum-lum", 2, 1f, 0f);
    public static readonly HueCurveKind SatLum = new("sat-lum", 1, 0f, 1f);
    public static readonly HueCurveKind HueFx = new("hue-fx", 0, 0f, 0f);

    private readonly int input;
    private readonly float slope;
    private readonly float level;

    public PointCurve Neutral(GradingStyle style) =>
        PointCurve.Validated((input switch { 0 => toSeq(Range(0, 6)).Map(static sixth => sixth / 6f), 1 => Seq(0f, 0.5f, 1f), _ => Seq(style.Low, (style.Low + style.High) * 0.5f, style.High) })
            .Map(x => new Vector2(x, (slope * x) + level))).ThrowIfFail();

    public Func<float, float> Fit(PointCurve curve) => input is 0 ? curve.Wrapped(slope) : curve.Fit();
}

public sealed record GradingHueCurve {
    private GradingHueCurve(HashMap<HueCurveKind, PointCurve> curves) => Curves = curves;

    public HashMap<HueCurveKind, PointCurve> Curves { get; }

    public static GradingHueCurve Neutral(GradingStyle style) => new(toHashMap(HueCurveKind.Items.Select(kind => (kind, kind.Neutral(style)))));

    public GradingHueCurve With(HueCurveKind kind, PointCurve curve) => new(Curves.SetItem(kind, curve));

    public Option<Action<Span<Vector4>>> Graded(GradingStyle style) {
        Func<float, float> Fit(HueCurveKind kind) => kind.Fit(Curves[kind]);
        (Func<float, float> hueHue, Func<float, float> hueSat, Func<float, float> hueLum, Func<float, float> lumSat) = (Fit(HueCurveKind.HueHue), Fit(HueCurveKind.HueSat), Fit(HueCurveKind.HueLum), Fit(HueCurveKind.LumSat));
        (Func<float, float> satSat, Func<float, float> lumLum, Func<float, float> satLum, Func<float, float> hueFx) = (Fit(HueCurveKind.SatSat), Fit(HueCurveKind.LumLum), Fit(HueCurveKind.SatLum), Fit(HueCurveKind.HueFx));
        bool additive = style == GradingStyle.Log;
        return this == Neutral(style) ? None : Some(ToneCurve.Graded(color => style.FromHsy(Curved(style.ToHsy(color)))));

        Vector3 Curved(Vector3 hsy) {
            float level = style.Encode(new Vector3(hsy.Z)).X;
            float saturation = float.Max(satSat(hsy.Y), 0f) * float.Max(lumSat(level), 0f) * float.Max(hueSat(hsy.X), 0f);
            (float lumaGain, float saturationGain) = (1f - ((1f - float.Max(hueLum(hsy.X), 0f)) * float.Min(saturation, 1f)), float.Max(satLum(saturation), 0f));
            float luma = style.Decode(new Vector3(lumLum(level))).X;
            float hue = hueHue(hsy.X) switch { var turned => turned - float.Floor(turned) };
            return new(hue + hueFx(hue), saturation, additive ? luma + ((lumaGain + saturationGain - 2f) * 0.1f) : luma * lumaGain * saturationGain);
        }
    }
}
