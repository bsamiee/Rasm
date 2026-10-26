using System.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.Tone;

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
    public partial double Read((double Hue, double Saturation, double Luma) hsy);
}

public sealed record CurvePoint(double X, double Y);

public sealed class SequenceEquality<T> : IEqualityComparerAccessor<ImmutableArray<T>> {
    public static IEqualityComparer<ImmutableArray<T>> EqualityComparer { get; } = EqualityComparer<ImmutableArray<T>>.Create(static (left, right) => left.SequenceEqual(right), static items => toSeq(items).GetHashCode());
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class SplineShape {
    public static readonly SplineShape Open = new(Extended);
    public static readonly SplineShape Wrapped = new(static points => Interpolated(Periodic(points, turning: false)) switch {
        var spline => coordinate => spline.Interpolate(coordinate - double.Floor(coordinate)),
    });
    public static readonly SplineShape Turning = new(static points => Interpolated(Periodic(points, turning: true)) switch {
        var spline => coordinate => double.Floor(coordinate) switch { var turn => turn + spline.Interpolate(coordinate - turn) },
    });

    public Func<double, double> Fit(ImmutableArray<CurvePoint> points, double input, double output) =>
        Shaped(toSeq(points).Map(point => new CurvePoint(point.X / input, point.Y / output)).Strict()) switch {
            var curve => coordinate => output * curve(coordinate / input),
        };

    [UseDelegateFromConstructor]
    private partial Func<double, double> Shaped(Seq<CurvePoint> points);

    private static Func<double, double> Extended(Seq<CurvePoint> points) =>
        (Interpolated(points), points[0], points[^1]) switch {
            var (spline, first, last) => (spline.Differentiate(first.X), spline.Differentiate(last.X)) switch {
                var (entry, exit) => coordinate =>
                    coordinate < first.X ? first.Y + (entry * (coordinate - first.X))
                    : coordinate > last.X ? last.Y + (exit * (coordinate - last.X))
                    : spline.Interpolate(coordinate),
            },
        };

    private static Seq<CurvePoint> Periodic(Seq<CurvePoint> points, bool turning) =>
        Spaced(points.Map(static point => point.X)).Zip(turning ? Spaced(points.Map(static point => point.Y)) : points.Map(static point => point.Y), static (x, y) => new CurvePoint(x, y)) switch {
            var spaced => [Shifted(spaced[^1], -1d, turning), .. spaced, Shifted(spaced[0], 1d, turning)],
        };

    private static Seq<double> Spaced(Seq<double> values) =>
        ((values[^1] - values[0]) * 2e-3) switch {
            var gap => values.Tail.Scan(values[0], (previous, value) => double.Max(value, previous + gap)),
        };

    private static CurvePoint Shifted(CurvePoint point, double turn, bool turning) => new(point.X + turn, turning ? point.Y + turn : point.Y);

    private static IInterpolation Interpolated(Seq<CurvePoint> points) =>
        (points.Map(static point => point.X).ToArray(), points.Map(static point => point.Y).ToArray()) switch {
            var (xs, ys) when xs.Length > 2 => CubicSpline.InterpolatePchipSorted(xs, ys),
            var (xs, ys) => LinearSpline.InterpolateSorted(xs, ys),
        };
}

[ComplexValueObject(SkipToString = true)]
[ValidationError<ImagingFault>]
public sealed partial class PointCurve {
    private static readonly Func<PointCurve, Func<double, double>> Fits = memo(static (PointCurve curve) => SplineShape.Open.Fit(curve.Points, CurveAxis.Stops.Unit, CurveAxis.Stops.Unit));

    public static PointCurve Identity { get; } = new([new CurvePoint(CurveAxis.Stops.Span.Low, CurveAxis.Stops.Span.Low), new CurvePoint(0d, 0d), new CurvePoint(CurveAxis.Stops.Span.High, CurveAxis.Stops.Span.High)]);

    [MemberEqualityComparer<SequenceEquality<CurvePoint>, ImmutableArray<CurvePoint>>]
    public ImmutableArray<CurvePoint> Points { get; }

    public Func<double, double> Fit() => Fits(this);

    public static Fin<PointCurve> Validated(ImmutableArray<CurvePoint> points) =>
        TryCreate(points, out PointCurve? curve, out ImagingFault? fault) ? curve : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref ImmutableArray<CurvePoint> points) =>
        validationError = points.Length >= 2
            && points.All(static point => CurveAxis.Stops.Contains(point.X) && CurveAxis.Stops.Contains(point.Y))
            && points.Zip(points.Skip(1)).All(static pair => pair.First.X < pair.Second.X && pair.First.Y <= pair.Second.Y)
                ? null
                : new ImagingFault("point curve of 2 or more ascending stop axis points with non-descending outputs", ImagingReason.Degenerate);
}

public sealed record PointCurves(PointCurve Red, PointCurve Green, PointCurve Blue, PointCurve Master) {
    public static PointCurves Identity { get; } = new(PointCurve.Identity, PointCurve.Identity, PointCurve.Identity, PointCurve.Identity);

    public (double Red, double Green, double Blue, double Master) Inputs(Vector4 pixel, (double R, double G, double B) luminance) =>
        (Red.Fit(), Green.Fit(), Blue.Fit()) switch {
            var (red, green, blue) => (CurveAxis.ToStops(pixel.X), CurveAxis.ToStops(pixel.Y), CurveAxis.ToStops(pixel.Z), CurveAxis.ToStops(LuminanceSamples.Luminance(
                CurveAxis.FromStops(Mapped(red, pixel.X)), CurveAxis.FromStops(Mapped(green, pixel.Y)), CurveAxis.FromStops(Mapped(blue, pixel.Z)), luminance))),
        };

    public Action<Span<Vector4>> Graded(float preserveHue) {
        (Func<double, double> red, Func<double, double> green, Func<double, double> blue, Func<double, double> master) = (Red.Fit(), Green.Fit(), Blue.Fit(), Master.Fit());
        return row => {
            foreach (ref Vector4 pixel in row) {
                (double r, double g, double b) = (Mapped(red, pixel.X), Mapped(green, pixel.Y), Mapped(blue, pixel.Z));
                pixel = new Vector4(ToneCurves.PreserveHue(
                    new Vector3((float)CurveAxis.FromStops(r), (float)CurveAxis.FromStops(g), (float)CurveAxis.FromStops(b)),
                    new Vector3((float)CurveAxis.FromStops(master(r)), (float)CurveAxis.FromStops(master(g)), (float)CurveAxis.FromStops(master(b))),
                    preserveHue), pixel.W);
            }
        };
    }

    private static double Mapped(Func<double, double> channel, float value) => channel(CurveAxis.ToStops(value));
}

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

    public static Fin<HueCurve> Validated(HueCurveKind kind, ImmutableArray<CurvePoint> points) =>
        TryCreate(kind, points, out HueCurve? curve, out ImagingFault? fault) ? curve : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref HueCurveKind kind, ref ImmutableArray<CurvePoint> points) {
        (CurveAxis input, (double Low, double High) output) = (kind.Input.Axis, kind.Output.Extent);
        validationError = points.Length >= 2
            && points.All(point => input.Contains(point.X) && point.Y >= output.Low && point.Y <= output.High)
            && points.Zip(points.Skip(1)).All(static pair => pair.First.X < pair.Second.X)
                ? null
                : new ImagingFault("hue curve of 2 or more ascending points inside its kind's axes", ImagingReason.Degenerate);
    }
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
    public ImmutableArray<CurvePoint> Neutral { get; }

    private static ImmutableArray<CurvePoint> Sixths(Func<double, double> output) =>
        [.. toSeq(Range(0, 6)).Map(static k => k / 6d * CurveAxis.Hue.Unit).Map(hue => new CurvePoint(hue, output(hue)))];
}

public sealed record HueCurves(
    HueCurve HueHue, HueCurve HueSat, HueCurve HueLum, HueCurve LumSat, HueCurve SatSat, HueCurve LumLum, HueCurve SatLum) {
    public static HueCurves Identity { get; } = new(
        HueCurve.Neutral(HueCurveKind.HueHue), HueCurve.Neutral(HueCurveKind.HueSat), HueCurve.Neutral(HueCurveKind.HueLum), HueCurve.Neutral(HueCurveKind.LumSat),
        HueCurve.Neutral(HueCurveKind.SatSat), HueCurve.Neutral(HueCurveKind.LumLum), HueCurve.Neutral(HueCurveKind.SatLum));

    public double Read(HueCurveKind kind, (double Hue, double Saturation, double Luma) hsy) =>
        kind == HueCurveKind.SatLum
            ? Saturation(SatSat.Fit(), HueSat.Fit(), LumSat.Fit(), hsy.Saturation, InputAxis.Hue.Read(hsy), InputAxis.Stops.Read(hsy))
            : kind.Input.Read(hsy);

    public Action<Span<Vector4>> Graded() {
        (Func<double, double> hueHue, Func<double, double> hueSat, Func<double, double> hueLum, Func<double, double> lumSat, Func<double, double> satSat, Func<double, double> lumLum, Func<double, double> satLum) =
            (HueHue.Fit(), HueSat.Fit(), HueLum.Fit(), LumSat.Fit(), SatSat.Fit(), LumLum.Fit(), SatLum.Fit());
        return row => {
            foreach (ref Vector4 pixel in row) {
                (double Hue, double Saturation, double Luma) hsy = Hsy.From(pixel);
                (double hue, double luma) = (InputAxis.Hue.Read(hsy), InputAxis.Stops.Read(hsy));
                double saturation = Saturation(satSat, hueSat, lumSat, hsy.Saturation, hue, luma);
                double gain = (1d - ((1d - Floored(hueLum(hue))) * double.Min(saturation, 1d))) * Floored(satLum(saturation));
                pixel = Hsy.ToRgb((hueHue(hue), saturation, CurveAxis.FromStops(lumLum(luma)) * gain), pixel.W);
            }
        };
    }

    private static double Saturation(Func<double, double> satSat, Func<double, double> hueSat, Func<double, double> lumSat, double saturation, double hue, double luma) =>
        Floored(satSat(saturation)) * Floored(hueSat(hue)) * Floored(lumSat(luma));

    private static double Floored(double gain) => double.Max(gain, 0d);
}
