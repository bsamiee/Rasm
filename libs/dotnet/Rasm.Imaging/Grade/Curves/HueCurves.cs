using System.Numerics;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Grade.Curves;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IHueCurveKind {
    public static abstract HueCurveKind Kind { get; }
}

public interface IHueCurveVisitor<out TResult> {
    public TResult Curve<TKind>(Lens<HueCurves, HueCurve<TKind>> lens) where TKind : IHueCurveKind;
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
[ValidationError<InvalidGrade>]
[ObjectFactory<string>]
public sealed partial class HueCurve<TKind> : IConvertible<string> where TKind : IHueCurveKind {
    private static readonly Func<HueCurve<TKind>, Func<double, double>> Fits =
        memo(static (HueCurve<TKind> curve) => TKind.Kind.Shape.Fit(curve.Points, TKind.Kind.Input.Axis.Unit, TKind.Kind.Output.Unit));

    public Seq<CurvePoint> Points { get; }

    public static HueCurve<TKind> Identity { get; } = new(TKind.Kind.Neutral);

    public Func<double, double> Fit() => Fits(this);

    public string ToValue() => CurvePoint.Format(Points);

    static InvalidGrade? IObjectFactory<HueCurve<TKind>, string, InvalidGrade>.Validate(string? value, IFormatProvider? provider, out HueCurve<TKind>? item) {
        item = null;
        return value is null ? null : CurvePoint.Parse(value).Case is Seq<CurvePoint> points ? Validate(points, out item) : new InvalidGrade();
    }

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref Seq<CurvePoint> points) =>
        validationError = points.Count >= 2
            && points.ForAll(static point => TKind.Kind.Input.Axis.Contains(point.X) && TKind.Kind.Output.Extent switch {
                var (low, high) => point.Y >= low && point.Y <= high,
            })
            && points.Zip(points.Tail).ForAll(static pair => pair.First.X < pair.Second.X)
                ? null
                : new InvalidGrade();
}

public sealed record HueCurves(
    HueCurve<HueHueKind> HueHue, HueCurve<HueSatKind> HueSat, HueCurve<HueLumKind> HueLum, HueCurve<LumSatKind> LumSat,
    HueCurve<SatSatKind> SatSat, HueCurve<LumLumKind> LumLum, HueCurve<SatLumKind> SatLum)
    : IStateRecord<HueCurves, HueCurveKind, InvalidGrade>, IPixelStage<HueCurves> {
    public static HueCurves Default { get; } = new(
        HueCurve<HueHueKind>.Identity, HueCurve<HueSatKind>.Identity, HueCurve<HueLumKind>.Identity, HueCurve<LumSatKind>.Identity,
        HueCurve<SatSatKind>.Identity, HueCurve<LumLumKind>.Identity, HueCurve<SatLumKind>.Identity);

    public double Read(HueCurveKind kind, Hsy hsy) =>
        kind == HueCurveKind.SatLum
            ? Saturation(SatSat.Fit(), HueSat.Fit(), LumSat.Fit(), hsy.Saturation, InputAxis.Hue.Read(hsy), InputAxis.Stops.Read(hsy))
            : kind.Input.Read(hsy);

    public static Option<PixelPass> Pass(HueCurves state, PassContext context) =>
        state == Default
            ? None
            : (state.HueHue.Fit(), state.HueSat.Fit(), state.HueLum.Fit(), state.LumSat.Fit(), state.SatSat.Fit(), state.LumLum.Fit(), state.SatLum.Fit()) switch {
                var (hueHue, hueSat, hueLum, lumSat, satSat, lumLum, satLum) => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row) {
                        Hsy hsy = Hsy.From(pixel);
                        (double hue, double luma) = (InputAxis.Hue.Read(hsy), InputAxis.Stops.Read(hsy));
                        double saturation = Saturation(satSat, hueSat, lumSat, hsy.Saturation, hue, luma);
                        double turns = hueHue(hue) / CurveAxis.Hue.Unit;
                        double gain = (1d - ((1d - Floored(hueLum(hue))) * double.Min(saturation, 1d))) * Floored(satLum(saturation));
                        pixel = new Hsy((float)(CurveAxis.Hue.Unit * (turns - double.Floor(turns))), (float)saturation, (float)(CurveAxis.FromStops(lumLum(luma)) * gain)).ToRgb(pixel.W);
                    }
                })),
            };

    private static double Saturation(Func<double, double> satSat, Func<double, double> hueSat, Func<double, double> lumSat, double saturation, double hue, double luma) =>
        Floored(satSat(saturation)) * Floored(hueSat(hue)) * Floored(lumSat(luma));

    private static double Floored(double gain) => double.Max(gain, 0d);
}

public abstract class HueCurveLens {
    private HueCurveLens() { }

    public abstract StateParameter<HueCurves> Kind { get; }

    public abstract TResult Accept<TResult>(IHueCurveVisitor<TResult> visitor);

    public sealed class Of<TKind>(Lens<HueCurves, HueCurve<TKind>> lens) : HueCurveLens where TKind : IHueCurveKind {
        private readonly Lens<HueCurves, HueCurve<TKind>> curve = lens;

        public override StateParameter<HueCurves> Kind { get; } = new StateParameter<HueCurves>.Keyed<HueCurve<TKind>, string, InvalidGrade>(lens);

        public override TResult Accept<TResult>(IHueCurveVisitor<TResult> visitor) => visitor.Curve(curve);
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HueCurveKind : IStateParameter<HueCurves> {
    public static readonly HueCurveKind HueHue = new(
        "hue-hue", InputAxis.Hue, CurveAxis.Hue, SplineShape.HueHue, Sixths(static hue => hue),
        new HueCurveLens.Of<HueHueKind>(
            Lens<HueCurves, HueCurve<HueHueKind>>.New(static curves => curves.HueHue, static curve => curves => curves with { HueHue = curve })));
    public static readonly HueCurveKind HueSat = new(
        "hue-sat", InputAxis.Hue, CurveAxis.Gain, SplineShape.Periodic, Sixths(static _ => 1d),
        new HueCurveLens.Of<HueSatKind>(
            Lens<HueCurves, HueCurve<HueSatKind>>.New(static curves => curves.HueSat, static curve => curves => curves with { HueSat = curve })));
    public static readonly HueCurveKind HueLum = new(
        "hue-lum", InputAxis.Hue, CurveAxis.Gain, SplineShape.Periodic, Sixths(static _ => 1d),
        new HueCurveLens.Of<HueLumKind>(
            Lens<HueCurves, HueCurve<HueLumKind>>.New(static curves => curves.HueLum, static curve => curves => curves with { HueLum = curve })));
    public static readonly HueCurveKind LumSat = new(
        "lum-sat", InputAxis.Stops, CurveAxis.Gain, SplineShape.Horizontal,
        Seq(new CurvePoint(CurveAxis.Stops.Span.Low, 1d), new CurvePoint(0d, 1d), new CurvePoint(CurveAxis.Stops.Span.High, 1d)),
        new HueCurveLens.Of<LumSatKind>(
            Lens<HueCurves, HueCurve<LumSatKind>>.New(static curves => curves.LumSat, static curve => curves => curves with { LumSat = curve })));
    public static readonly HueCurveKind SatSat = new(
        "sat-sat", InputAxis.Saturation, CurveAxis.Saturation, SplineShape.Diagonal, Seq(new CurvePoint(0d, 0d), new CurvePoint(0.5d, 0.5d), new CurvePoint(1d, 1d)),
        new HueCurveLens.Of<SatSatKind>(
            Lens<HueCurves, HueCurve<SatSatKind>>.New(static curves => curves.SatSat, static curve => curves => curves with { SatSat = curve })));
    public static readonly HueCurveKind LumLum = new(
        "lum-lum", InputAxis.Stops, CurveAxis.Stops, SplineShape.Diagonal, PointCurve.Identity.Points,
        new HueCurveLens.Of<LumLumKind>(
            Lens<HueCurves, HueCurve<LumLumKind>>.New(static curves => curves.LumLum, static curve => curves => curves with { LumLum = curve })));
    public static readonly HueCurveKind SatLum = new(
        "sat-lum", InputAxis.Saturation, CurveAxis.Gain, SplineShape.Horizontal, Seq(new CurvePoint(0d, 1d), new CurvePoint(0.5d, 1d), new CurvePoint(1d, 1d)),
        new HueCurveLens.Of<SatLumKind>(
            Lens<HueCurves, HueCurve<SatLumKind>>.New(static curves => curves.SatLum, static curve => curves => curves with { SatLum = curve })));

    public InputAxis Input { get; }
    public CurveAxis Output { get; }
    internal SplineShape Shape { get; }
    internal Seq<CurvePoint> Neutral { get; }
    public HueCurveLens Curve { get; }

    public StateParameter<HueCurves> Kind => Curve.Kind;

    private static Seq<CurvePoint> Sixths(Func<double, double> output) =>
        toSeq(Range(0, 6)).Map(static k => k / 6d * CurveAxis.Hue.Unit).Map(hue => new CurvePoint(hue, output(hue))).Strict();
}
