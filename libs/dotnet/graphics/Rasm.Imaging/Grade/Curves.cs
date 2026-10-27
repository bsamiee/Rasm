using System.Numerics;
using System.Runtime.Intrinsics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Tone;
using SuperLinq;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class CurveAxis {
    public static readonly CurveAxis Stops = new(span: (-7d, 7d), periodic: false, unit: 1d);
    public static readonly CurveAxis Hue = new(span: (0d, double.Tau), periodic: true, unit: double.Tau);
    public static readonly CurveAxis Saturation = new(span: (0d, Hsy.SaturationPeak), periodic: false, unit: 1d);
    public static readonly CurveAxis Gain = new(span: (0d, 2d), periodic: false, unit: 1d);

    private const double LinearBreak = 0.0041318374739483946;
    private const double StopBreak = -5.5;
    private const double Shift = -0.000157849851665374;
    private const double Slope = 363.034608563;
    private const double Floor = -7d;

    public (double Low, double High) Span { get; }
    public bool Periodic { get; }
    public double Unit { get; }

    public (double Low, double High) Extent =>
        Span switch {
            var (low, high) when Periodic => ((2d * low) - high, (2d * high) - low),
            var span => span,
        };

    public bool Contains(double coordinate) => coordinate >= Span.Low && (Periodic ? coordinate < Span.High : coordinate <= Span.High);

    public static double ToStops(double linear) =>
        linear < LinearBreak ? (linear * Slope) + Floor : double.Log2((linear + Shift) / (Exposure.MiddleGray + Shift));

    public static double FromStops(double stops) =>
        stops < StopBreak ? (stops - Floor) / Slope : (double.Exp2(stops) * (Exposure.MiddleGray + Shift)) - Shift;
}

[SmartEnum]
public sealed partial class InputAxis {
    public static readonly InputAxis Stops = new(CurveAxis.Stops, static hsy => CurveAxis.ToStops(hsy.Luma));
    public static readonly InputAxis Hue = new(CurveAxis.Hue, static hsy => hsy.Hue);
    public static readonly InputAxis Saturation = new(CurveAxis.Saturation, static hsy => hsy.Saturation);

    public CurveAxis Axis { get; }

    [UseDelegateFromConstructor]
    public partial double Read(Hsy hsy);
}

public sealed record CurvePoint(double X, double Y);

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class SplineShape {
    public static readonly SplineShape Open = new(static (points, input, output) => new CurveFit.Open(Interpolated(points), points[0], points[^1], input, output));
    public static readonly SplineShape Wrapped = new(static (points, input, output) => new CurveFit.Wrapped(Interpolated(Periodic(points, turning: false)), input, output));
    public static readonly SplineShape Turning = new(static (points, input, output) => new CurveFit.Turning(Interpolated(Periodic(points, turning: true)), input, output));

    internal CurveFit Fit(Seq<CurvePoint> points, double input, double output) =>
        Shaped(points.Map(point => new CurvePoint(point.X / input, point.Y / output)).Strict(), input, output);

    [UseDelegateFromConstructor]
    private partial CurveFit Shaped(Seq<CurvePoint> points, double input, double output);

    private static Seq<CurvePoint> Periodic(Seq<CurvePoint> points, bool turning) =>
        Spaced(points.Map(static point => point.X)).Zip(turning ? Spaced(points.Map(static point => point.Y)) : points.Map(static point => point.Y), static (x, y) => new CurvePoint(x, y)) switch {
            var spaced => [Shifted(spaced[^1], -1d, turning), .. spaced, Shifted(spaced[0], 1d, turning)],
        };

    private static Seq<double> Spaced(Seq<double> values) {
        double gap = (values[^1] - values[0]) * 2e-3;
        return values.Tail.Scan(values[0], (previous, value) => double.Max(value, previous + gap));
    }

    private static CurvePoint Shifted(CurvePoint point, double turn, bool turning) => new(point.X + turn, turning ? point.Y + turn : point.Y);

    private static IInterpolation Interpolated(Seq<CurvePoint> points) =>
        (points.Map(static point => point.X).ToArray(), points.Map(static point => point.Y).ToArray()) switch {
            var (xs, ys) when xs.Length > 2 => CubicSpline.InterpolatePchipSorted(xs, ys),
            var (xs, ys) => LinearSpline.InterpolateSorted(xs, ys),
        };
}

internal abstract class CurveFit(double input, double output) {
    internal double At(double coordinate) => output * Unit(coordinate / input);

    private protected abstract double Unit(double coordinate);

    internal sealed class Open(IInterpolation spline, CurvePoint first, CurvePoint last, double input, double output) : CurveFit(input, output) {
        private readonly (double Entry, double Exit) slopes = (spline.Differentiate(first.X), spline.Differentiate(last.X));
        private protected override double Unit(double coordinate) => coordinate < first.X ? first.Y + (slopes.Entry * (coordinate - first.X)) : coordinate > last.X ? last.Y + (slopes.Exit * (coordinate - last.X)) : spline.Interpolate(coordinate);
    }

    internal sealed class Wrapped(IInterpolation spline, double input, double output) : CurveFit(input, output) { private protected override double Unit(double coordinate) => spline.Interpolate(coordinate - double.Floor(coordinate)); }
    internal sealed class Turning(IInterpolation spline, double input, double output) : CurveFit(input, output) { private protected override double Unit(double coordinate) => double.Floor(coordinate) switch { var turn => turn + spline.Interpolate(coordinate - turn) }; }
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class PointCurve {
    public static PointCurve Identity { get; } = new([new CurvePoint(CurveAxis.Stops.Span.Low, CurveAxis.Stops.Span.Low), new CurvePoint(0d, 0d), new CurvePoint(CurveAxis.Stops.Span.High, CurveAxis.Stops.Span.High)]);

    public Seq<CurvePoint> Points { get; }

    [IgnoreMember]
    internal CurveFit Fitted => field ??= SplineShape.Open.Fit(Points, CurveAxis.Stops.Unit, CurveAxis.Stops.Unit);

    public Func<double, double> Fit() => Fitted.At;

    public static Fin<PointCurve> Validated(Seq<CurvePoint> points) =>
        ImagingFault.Unless(points.Count >= 2
            && points.ForAll(static point => CurveAxis.Stops.Contains(point.X) && CurveAxis.Stops.Contains(point.Y))
            && points.Window(2).All(static pair => pair[0].X < pair[1].X && pair[0].Y <= pair[1].Y), ImagingReason.Degenerate, points).Map(_ => new PointCurve(points));
}

public sealed record PointCurves(PointCurve Red, PointCurve Green, PointCurve Blue, PointCurve Master) {
    public static PointCurves Identity { get; } = new(PointCurve.Identity, PointCurve.Identity, PointCurve.Identity, PointCurve.Identity);

    public (double Red, double Green, double Blue, double Master) Inputs(Vector4 pixel, Vector256<double> luminance) =>
        (CurveAxis.ToStops(pixel.X), CurveAxis.ToStops(pixel.Y), CurveAxis.ToStops(pixel.Z), CurveAxis.ToStops(Vector256.Dot(luminance,
            Vector256.Create(CurveAxis.FromStops(Mapped(Red.Fitted, pixel.X)), CurveAxis.FromStops(Mapped(Green.Fitted, pixel.Y)), CurveAxis.FromStops(Mapped(Blue.Fitted, pixel.Z)), 0d))));

    public Action<Span<Vector4>> Graded(float preserveHue) {
        (CurveFit red, CurveFit green, CurveFit blue, CurveFit master) = (Red.Fitted, Green.Fitted, Blue.Fitted, Master.Fitted);
        return row => {
            foreach (ref Vector4 pixel in row) {
                (double r, double g, double b) = (Mapped(red, pixel.X), Mapped(green, pixel.Y), Mapped(blue, pixel.Z));
                pixel = new Vector4(ToneCurves.PreserveHue(
                    new Vector3((float)CurveAxis.FromStops(r), (float)CurveAxis.FromStops(g), (float)CurveAxis.FromStops(b)),
                    new Vector3((float)CurveAxis.FromStops(master.At(r)), (float)CurveAxis.FromStops(master.At(g)), (float)CurveAxis.FromStops(master.At(b))),
                    preserveHue), pixel.W);
            }
        };
    }

    private static double Mapped(CurveFit channel, float value) => channel.At(CurveAxis.ToStops(value));
}

[ComplexValueObject(SkipFactoryMethods = true, SkipToString = true)]
public sealed partial class HueCurve {
    [MemberEqualityComparer<ComparerAccessors.Default<HueCurveKind>, HueCurveKind>]
    public HueCurveKind Kind { get; }
    public Seq<CurvePoint> Points { get; }

    [IgnoreMember]
    internal CurveFit Fitted => field ??= Kind.Shape.Fit(Points, Kind.Input.Axis.Unit, Kind.Output.Unit);

    public static HueCurve Neutral(HueCurveKind kind) => new(kind, kind.Neutral);

    public Func<double, double> Fit() => Fitted.At;

    public static Fin<HueCurve> Validated(HueCurveKind kind, Seq<CurvePoint> points) =>
        ImagingFault.Unless(points.Count >= 2
            && points.ForAll(point => kind.Input.Axis.Contains(point.X) && point.Y >= kind.Output.Extent.Low && point.Y <= kind.Output.Extent.High)
            && points.Window(2).All(static pair => pair[0].X < pair[1].X), ImagingReason.Degenerate, points).Map(_ => new HueCurve(kind, points));
}

[SmartEnum<string>]
public sealed partial class HueCurveKind {
    public static readonly HueCurveKind HueHue = new("hue-hue", InputAxis.Hue, CurveAxis.Hue, SplineShape.Turning, Sixths(static hue => hue));
    public static readonly HueCurveKind HueSat = new("hue-sat", InputAxis.Hue, CurveAxis.Gain, SplineShape.Wrapped, Sixths(static _ => 1d));
    public static readonly HueCurveKind HueLum = new("hue-lum", InputAxis.Hue, CurveAxis.Gain, SplineShape.Wrapped, Sixths(static _ => 1d));
    public static readonly HueCurveKind LumSat = new("lum-sat", InputAxis.Stops, CurveAxis.Gain, SplineShape.Open, [new CurvePoint(CurveAxis.Stops.Span.Low, 1d), new CurvePoint(0d, 1d), new CurvePoint(CurveAxis.Stops.Span.High, 1d)]);
    public static readonly HueCurveKind SatSat = new("sat-sat", InputAxis.Saturation, CurveAxis.Saturation, SplineShape.Open, [new CurvePoint(0d, 0d), new CurvePoint(0.5d, 0.5d), new CurvePoint(1d, 1d)]);
    public static readonly HueCurveKind LumLum = new("lum-lum", InputAxis.Stops, CurveAxis.Stops, SplineShape.Open, PointCurve.Identity.Points);
    public static readonly HueCurveKind SatLum = new("sat-lum", InputAxis.Saturation, CurveAxis.Gain, SplineShape.Open, [new CurvePoint(0d, 1d), new CurvePoint(0.5d, 1d), new CurvePoint(1d, 1d)]);

    public InputAxis Input { get; }
    public CurveAxis Output { get; }
    internal SplineShape Shape { get; }
    public Seq<CurvePoint> Neutral { get; }

    private static Seq<CurvePoint> Sixths(Func<double, double> output) =>
        toSeq(Range(0, 6)).Map(static k => k / 6d * CurveAxis.Hue.Unit).Map(hue => new CurvePoint(hue, output(hue))).Strict();
}

public sealed record HueCurves {
    private HueCurves(HashMap<HueCurveKind, HueCurve> curves) => Curves = curves;

    public static HueCurves Identity { get; } = Of(HueCurve.Neutral);

    public HashMap<HueCurveKind, HueCurve> Curves { get; }

    public static HueCurves Of(Func<HueCurveKind, HueCurve> curve) => new(toHashMap(HueCurveKind.Items.Select(kind => (kind, curve(kind)))));

    public HueCurves With(HueCurve curve) => new(Curves.SetItem(curve.Kind, curve));

    public double Read(HueCurveKind kind, Hsy hsy) =>
        kind == HueCurveKind.SatLum
            ? Saturation(Curves[HueCurveKind.SatSat].Fitted, Curves[HueCurveKind.HueSat].Fitted, Curves[HueCurveKind.LumSat].Fitted, hsy.Saturation, InputAxis.Hue.Read(hsy), InputAxis.Stops.Read(hsy))
            : kind.Input.Read(hsy);

    public Action<Span<Vector4>> Graded() {
        (CurveFit hueHue, CurveFit hueSat, CurveFit hueLum, CurveFit lumSat, CurveFit satSat, CurveFit lumLum, CurveFit satLum) = (
            Curves[HueCurveKind.HueHue].Fitted, Curves[HueCurveKind.HueSat].Fitted, Curves[HueCurveKind.HueLum].Fitted, Curves[HueCurveKind.LumSat].Fitted,
            Curves[HueCurveKind.SatSat].Fitted, Curves[HueCurveKind.LumLum].Fitted, Curves[HueCurveKind.SatLum].Fitted);
        return row => {
            foreach (ref Vector4 pixel in row) {
                double luma = Hsy.LumaOf(pixel);
                (double hue, double stops) = (Hsy.HueOf(pixel), CurveAxis.ToStops(luma));
                double saturation = Saturation(satSat, hueSat, lumSat, Hsy.SaturationOf(pixel, luma), hue, stops);
                double gain = (1d - ((1d - Floored(hueLum.At(hue))) * double.Min(saturation, 1d))) * Floored(satLum.At(saturation));
                pixel = Hsy.ToRgb(hueHue.At(hue), saturation, CurveAxis.FromStops(lumLum.At(stops)) * gain, pixel.W);
            }
        };
    }

    private static double Saturation(CurveFit satSat, CurveFit hueSat, CurveFit lumSat, double saturation, double hue, double stops) =>
        Floored(satSat.At(saturation)) * Floored(hueSat.At(hue)) * Floored(lumSat.At(stops));

    private static double Floored(double gain) => double.Max(gain, 0d);
}
