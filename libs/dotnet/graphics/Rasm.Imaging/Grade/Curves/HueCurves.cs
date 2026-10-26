using System.Numerics;

namespace Rasm.Imaging.Grade.Curves;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IHueCurveKind {
    public static abstract HueCurveKind Kind { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct HueHueKind : IHueCurveKind {
    static HueCurveKind IHueCurveKind.Kind => HueCurveKind.HueHue;
}

public readonly record struct HueSatKind : IHueCurveKind {
    static HueCurveKind IHueCurveKind.Kind => HueCurveKind.HueSat;
}

public readonly record struct HueLumKind : IHueCurveKind {
    static HueCurveKind IHueCurveKind.Kind => HueCurveKind.HueLum;
}

public readonly record struct LumSatKind : IHueCurveKind {
    static HueCurveKind IHueCurveKind.Kind => HueCurveKind.LumSat;
}

public readonly record struct SatSatKind : IHueCurveKind {
    static HueCurveKind IHueCurveKind.Kind => HueCurveKind.SatSat;
}

public readonly record struct LumLumKind : IHueCurveKind {
    static HueCurveKind IHueCurveKind.Kind => HueCurveKind.LumLum;
}

public readonly record struct SatLumKind : IHueCurveKind {
    static HueCurveKind IHueCurveKind.Kind => HueCurveKind.SatLum;
}

[ComplexValueObject]
[ObjectFactory<string>]
public sealed partial class HueCurve<TKind> : IConvertible<string> where TKind : IHueCurveKind {
    private static readonly Func<HueCurve<TKind>, Func<double, double>> Fits =
        memo(static (HueCurve<TKind> curve) => TKind.Kind.Shape.Fit(curve.Points, TKind.Kind.Input.Axis.Unit, TKind.Kind.Output.Unit));

    public Seq<CurvePoint> Points { get; }

    public Func<double, double> Fit() => Fits(this);

    public string ToValue() => CurvePoint.Format(Points);

    static ValidationError? IObjectFactory<HueCurve<TKind>, string, ValidationError>.Validate(string? value, IFormatProvider? provider, out HueCurve<TKind>? item) {
        item = null;
        return value is null ? null
            : CurvePoint.Parse(value).Case is Seq<CurvePoint> points ? Validate(points, out item)
            : new ValidationError($"{typeof(TKind).Name} curve '{value}' did not parse as curve points");
    }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<CurvePoint> points) =>
        validationError = points.Count >= 2
            && points.ForAll(static point => TKind.Kind.Input.Axis.Contains(point.X) && TKind.Kind.Output.Extent switch {
                var (low, high) => point.Y >= low && point.Y <= high,
            })
            && points.Zip(points.Tail).ForAll(static pair => pair.First.X < pair.Second.X)
                ? null
                : new ValidationError($"{typeof(TKind).Name} curve needs 2 or more points inside axes, inputs ascending");
}

public sealed record HueCurves(
    HueCurve<HueHueKind> HueHue, HueCurve<HueSatKind> HueSat, HueCurve<HueLumKind> HueLum, HueCurve<LumSatKind> LumSat,
    HueCurve<SatSatKind> SatSat, HueCurve<LumLumKind> LumLum, HueCurve<SatLumKind> SatLum) {
    public static HueCurves Identity { get; } = new(
        Neutral<HueHueKind>(), Neutral<HueSatKind>(), Neutral<HueLumKind>(), Neutral<LumSatKind>(),
        Neutral<SatSatKind>(), Neutral<LumLumKind>(), Neutral<SatLumKind>());

    public static HueCurve<TKind> Neutral<TKind>() where TKind : IHueCurveKind => HueCurve<TKind>.Create(TKind.Kind.Neutral);

    public double Read(HueCurveKind kind, Hsy hsy) =>
        kind == HueCurveKind.SatLum
            ? Saturation(SatSat.Fit(), HueSat.Fit(), LumSat.Fit(), hsy.Saturation, InputAxis.Hue.Read(hsy), InputAxis.Stops.Read(hsy))
            : kind.Input.Read(hsy);

    public Action<Span<Vector4>> Graded() {
        (Func<double, double> hueHue, Func<double, double> hueSat, Func<double, double> hueLum, Func<double, double> lumSat, Func<double, double> satSat, Func<double, double> lumLum, Func<double, double> satLum) =
            (HueHue.Fit(), HueSat.Fit(), HueLum.Fit(), LumSat.Fit(), SatSat.Fit(), LumLum.Fit(), SatLum.Fit());
        return row => {
            foreach (ref Vector4 pixel in row) {
                Hsy hsy = Hsy.From(pixel);
                (double hue, double luma) = (InputAxis.Hue.Read(hsy), InputAxis.Stops.Read(hsy));
                double saturation = Saturation(satSat, hueSat, lumSat, hsy.Saturation, hue, luma);
                double gain = (1d - ((1d - Floored(hueLum(hue))) * double.Min(saturation, 1d))) * Floored(satLum(saturation));
                pixel = new Hsy(hueHue(hue), saturation, CurveAxis.FromStops(lumLum(luma)) * gain).ToRgb(pixel.W);
            }
        };
    }

    private static double Saturation(Func<double, double> satSat, Func<double, double> hueSat, Func<double, double> lumSat, double saturation, double hue, double luma) =>
        Floored(satSat(saturation)) * Floored(hueSat(hue)) * Floored(lumSat(luma));

    private static double Floored(double gain) => double.Max(gain, 0d);
}

[SmartEnum]
public sealed partial class HueCurveKind {
    public static readonly HueCurveKind HueHue = new(InputAxis.Hue, CurveAxis.Hue, SplineShape.HueHue, Sixths(static hue => hue));
    public static readonly HueCurveKind HueSat = new(InputAxis.Hue, CurveAxis.Gain, SplineShape.Periodic, Sixths(static _ => 1d));
    public static readonly HueCurveKind HueLum = new(InputAxis.Hue, CurveAxis.Gain, SplineShape.Periodic, Sixths(static _ => 1d));
    public static readonly HueCurveKind LumSat = new(
        InputAxis.Stops, CurveAxis.Gain, SplineShape.Horizontal,
        Seq(new CurvePoint(CurveAxis.Stops.Span.Low, 1d), new CurvePoint(0d, 1d), new CurvePoint(CurveAxis.Stops.Span.High, 1d)));
    public static readonly HueCurveKind SatSat = new(
        InputAxis.Saturation, CurveAxis.Saturation, SplineShape.Diagonal, Seq(new CurvePoint(0d, 0d), new CurvePoint(0.5d, 0.5d), new CurvePoint(1d, 1d)));
    public static readonly HueCurveKind LumLum = new(InputAxis.Stops, CurveAxis.Stops, SplineShape.Diagonal, PointCurve.Identity.Points);
    public static readonly HueCurveKind SatLum = new(
        InputAxis.Saturation, CurveAxis.Gain, SplineShape.Horizontal, Seq(new CurvePoint(0d, 1d), new CurvePoint(0.5d, 1d), new CurvePoint(1d, 1d)));

    public InputAxis Input { get; }
    public CurveAxis Output { get; }
    internal SplineShape Shape { get; }
    public Seq<CurvePoint> Neutral { get; }

    private static Seq<CurvePoint> Sixths(Func<double, double> output) =>
        toSeq(Range(0, 6)).Map(static k => k / 6d * CurveAxis.Hue.Unit).Map(hue => new CurvePoint(hue, output(hue))).Strict();
}
