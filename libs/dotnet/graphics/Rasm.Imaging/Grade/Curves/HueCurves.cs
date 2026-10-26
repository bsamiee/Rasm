using System.Numerics;

namespace Rasm.Imaging.Grade.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipToString = true)]
[ValidationError<ImagingFault>]
public sealed partial class HueCurve {
    private static readonly Func<HueCurve, Func<double, double>> Fits = memo(static (HueCurve curve) => curve.Kind.Shape.Fit(curve.Points, curve.Kind.Input.Axis.Unit, curve.Kind.Output.Unit));

    [MemberEqualityComparer<ComparerAccessors.Default<HueCurveKind>, HueCurveKind>]
    public HueCurveKind Kind { get; }
    [MemberEqualityComparer<SequenceEquality<CurvePoint>, ImmutableArray<CurvePoint>>]
    public ImmutableArray<CurvePoint> Points { get; }

    public static HueCurve Neutral(HueCurveKind kind) => new(kind, kind.Neutral);

    public Func<double, double> Fit() => Fits(this);

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref HueCurveKind kind, ref ImmutableArray<CurvePoint> points) {
        (CurveAxis input, (double Low, double High) output) = (kind.Input.Axis, kind.Output.Extent);
        validationError = points.Length >= 2
            && points.All(point => input.Contains(point.X) && point.Y >= output.Low && point.Y <= output.High)
            && points.Zip(points.Skip(1)).All(static pair => pair.First.X < pair.Second.X)
                ? null
                : new ImagingFault("Hue curve needs 2 or more points inside its kind's axes, inputs ascending");
    }
}

public sealed record HueCurves(
    HueCurve HueHue, HueCurve HueSat, HueCurve HueLum, HueCurve LumSat, HueCurve SatSat, HueCurve LumLum, HueCurve SatLum) {
    public static HueCurves Identity { get; } = new(
        HueCurve.Neutral(HueCurveKind.HueHue), HueCurve.Neutral(HueCurveKind.HueSat), HueCurve.Neutral(HueCurveKind.HueLum), HueCurve.Neutral(HueCurveKind.LumSat),
        HueCurve.Neutral(HueCurveKind.SatSat), HueCurve.Neutral(HueCurveKind.LumLum), HueCurve.Neutral(HueCurveKind.SatLum));

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HueCurveKind {
    public static readonly HueCurveKind HueHue = new("hue-hue", InputAxis.Hue, CurveAxis.Hue, SplineShape.HueHue, Sixths(static hue => hue));
    public static readonly HueCurveKind HueSat = new("hue-sat", InputAxis.Hue, CurveAxis.Gain, SplineShape.Periodic, Sixths(static _ => 1d));
    public static readonly HueCurveKind HueLum = new("hue-lum", InputAxis.Hue, CurveAxis.Gain, SplineShape.Periodic, Sixths(static _ => 1d));
    public static readonly HueCurveKind LumSat = new("lum-sat", InputAxis.Stops, CurveAxis.Gain, SplineShape.Horizontal, [new CurvePoint(CurveAxis.Stops.Span.Low, 1d), new CurvePoint(0d, 1d), new CurvePoint(CurveAxis.Stops.Span.High, 1d)]);
    public static readonly HueCurveKind SatSat = new("sat-sat", InputAxis.Saturation, CurveAxis.Saturation, SplineShape.Diagonal, [new CurvePoint(0d, 0d), new CurvePoint(0.5d, 0.5d), new CurvePoint(1d, 1d)]);
    public static readonly HueCurveKind LumLum = new("lum-lum", InputAxis.Stops, CurveAxis.Stops, SplineShape.Diagonal, PointCurve.Identity.Points);
    public static readonly HueCurveKind SatLum = new("sat-lum", InputAxis.Saturation, CurveAxis.Gain, SplineShape.Horizontal, [new CurvePoint(0d, 1d), new CurvePoint(0.5d, 1d), new CurvePoint(1d, 1d)]);

    public InputAxis Input { get; }
    public CurveAxis Output { get; }
    internal SplineShape Shape { get; }
    public ImmutableArray<CurvePoint> Neutral { get; }

    private static ImmutableArray<CurvePoint> Sixths(Func<double, double> output) =>
        [.. toSeq(Range(0, 6)).Map(static k => k / 6d * CurveAxis.Hue.Unit).Map(hue => new CurvePoint(hue, output(hue)))];
}
