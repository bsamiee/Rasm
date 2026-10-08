using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Grade.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct CurveCoordinate : IMinMaxValue<CurveCoordinate> {
    public static CurveCoordinate MinValue { get; } = new(CurveAxis.Items.Min(static axis => axis.Extent.Low));
    public static CurveCoordinate MaxValue { get; } = new(CurveAxis.Items.Max(static axis => axis.Extent.High));

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[SmartEnum]
public sealed partial class CurveAxis {
    public static readonly CurveAxis Stops = new(span: (-7d, 7d), periodic: false, unit: 1d, origin: 0d, measure: None);
    public static readonly CurveAxis Hue = new(span: (0d, double.Tau), periodic: true, unit: double.Tau, origin: None, measure: Quantity.GetUnitInfo(AngleUnit.Radian));
    public static readonly CurveAxis Saturation = new(span: (0d, 1.4d * (1d + Gamut.StandardRgb.Luminance.Y)), periodic: false, unit: 1d, origin: None, measure: None);
    public static readonly CurveAxis Gain = new(span: (0d, 2d), periodic: false, unit: 1d, origin: 1d, measure: None);

    private const double LinearBreak = 0.0041318374739483946;
    private const double StopBreak = -5.5;
    private const double Shift = -0.000157849851665374;
    private const double Slope = 363.034608563;
    private const double Floor = -7d;

    public (double Low, double High) Span { get; }
    public bool Periodic { get; }
    public double Unit { get; }
    public Option<double> Origin { get; }
    public Option<UnitInfo> Measure { get; }

    public (double Low, double High) Extent =>
        Span switch {
            var (low, high) when Periodic => ((2d * low) - high, (2d * high) - low),
            var span => span,
        };

    public Presentation<CurveCoordinate, double> Presentation => new() { Unit = Measure, Soft = Span, Origin = Origin };

    public bool Contains(double coordinate) => coordinate >= Span.Low && (Periodic ? coordinate < Span.High : coordinate <= Span.High);

    public static double ToStops(double linear) =>
        linear < LinearBreak ? (linear * Slope) + Floor : double.Log2((linear + Shift) / (Exposure.MiddleGrey + Shift));

    public static double FromStops(double stops) =>
        stops < StopBreak ? (stops - Floor) / Slope : (double.Exp2(stops) * (Exposure.MiddleGrey + Shift)) - Shift;
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

public readonly record struct CurvePoint(double X, double Y) {
    public static Option<Seq<CurvePoint>> Parse(string text) =>
        toSeq(text.Split(';')).Traverse(static pair =>
            pair.Split(',') is [var x, var y]
                ? from px in Invariant.Number<double>(x) from py in Invariant.Number<double>(y) select new CurvePoint(px, py)
                : None).As();

    public static string Format(Seq<CurvePoint> points) =>
        string.Join(';', points.Map(static point => string.Create(CultureInfo.InvariantCulture, $"{point.X},{point.Y}")));
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class SplineShape {
    public static readonly SplineShape BSpline = new(Rgb);
    public static readonly SplineShape HueHue = new(static domain => Hue(domain, wraps: true, monotone: true, harmonic: true));
    public static readonly SplineShape Periodic = new(static domain => Hue(domain, wraps: true, monotone: false, harmonic: true));
    public static readonly SplineShape Horizontal = new(static domain => Hue(domain, wraps: false, monotone: false, harmonic: true));
    public static readonly SplineShape Diagonal = new(static domain => Hue(domain, wraps: false, monotone: true, harmonic: false));

    public Func<double, double> Fit(Seq<CurvePoint> points, double input, double output) =>
        Prepare(points.Map(point => new CurvePoint(point.X / input, point.Y / output)).Strict()).Spline() switch {
            var spline => coordinate => output * spline.Interpolate(coordinate / input),
        };

    [UseDelegateFromConstructor]
    private partial FitInput Prepare(Seq<CurvePoint> domain);

    private static FitInput Rgb(Seq<CurvePoint> domain) {
        return Slopes(domain, harmonic: false) switch {
            var estimated => new FitInput(
                domain,
                Rescaled(estimated, Interval.Of(domain, estimated, static (span, entry, exit) => (Span: span, Knot: Split(span, entry, exit).IfNone(span.End.X))), Adjusted),
                Split),
        };

        static Option<double> Split(Interval span, double entry, double exit) =>
            Math.Abs(entry + exit - (2d * span.Secant)) < 1e-6 ? None
            : (entry - span.Secant, exit - span.Secant) switch {
                var (early, late) when early * late >= 0d => Some((span.Start.X + span.End.X) * 0.5),
                var (early, late) when Math.Abs(early) > Math.Abs(late) => Some(span.End.X + (early * span.Width / (exit - entry))),
                var (_, late) => Some(span.Start.X + (late * span.Width / (exit - entry))),
            };

        static Option<double> Adjusted((Interval Span, double Knot) step, double entry, double exit) =>
            ((((step.Knot - step.Span.Start.X) * entry) + ((step.Span.End.X - step.Knot) * exit)) / step.Span.Width) switch {
                var blend when blend > 2d * step.Span.Secant =>
                    Some(((2d * step.Span.Secant) - double.Min(0.01 * 0.5 * (entry + exit), step.Span.Secant)) / blend),
                _ => None,
            };
    }

    private static FitInput Hue(Seq<CurvePoint> domain, bool wraps, bool monotone, bool harmonic) {
        return Prepared(domain, wraps, monotone) switch {
            var points => new FitInput(points, (points, Slopes(points, harmonic)) switch {
                ([_, _], var estimated) => estimated,
                (_, var estimated) => Wrapped(Rescaled(estimated, Interval.Between(points), Preserved), wraps),
            }, Split),
        };

        static CurvePoint Shifted(CurvePoint point, double turn, bool monotone) => new(point.X + turn, monotone ? point.Y + turn : point.Y);

        static Seq<double> Spaced(Seq<double> values) =>
            values switch {
                [var first, .., var last] => values.Tail.Scan(first, (previous, value) => double.Max(value, previous + ((last - first) * 2e-3))),
                _ => values,
            };

        static Seq<CurvePoint> Prepared(Seq<CurvePoint> domain, bool wraps, bool monotone) =>
            toSeq(domain.Map(point => wraps && (point.X < 0d || point.X >= 1d) ? Shifted(point, point.X < 0d ? 1d : -1d, monotone) : point).OrderBy(static point => point.X)) switch {
                var sorted => (wraps, Spaced(sorted.Map(static point => point.X)).Zip(monotone ? Spaced(sorted.Map(static point => point.Y)) : sorted.Map(static point => point.Y), static (x, y) => new CurvePoint(x, y))),
            } switch {
                (true, [var first, .., var last] and var spaced) => Shifted(last, -1d, monotone).Cons(spaced).Add(Shifted(first, 1d, monotone)),
                (_, var spaced) => spaced,
            };

        static Seq<double> Wrapped(Seq<double> slopes, bool wraps) =>
            (wraps, slopes) switch {
                (true, [_, var second, .., var penultimate, _]) => penultimate.Cons(slopes.Tail.Init).Add(second),
                _ => slopes,
            };

        static Option<double> Preserved(Interval span, double entry, double exit) =>
            (entry + ((Math.Abs(entry) > Math.Abs(exit) ? 1d - 0.2 : 0.2) * (exit - entry))) switch {
                var near when near == 0d => None,
                var near => (0.75 * 2d * span.Secant / near) switch {
                    var scale when scale < 1d => Some(scale),
                    _ => None,
                },
            };

        static Option<double> Split(Interval span, double entry, double exit) =>
            Math.Abs(entry + exit - (2d * span.Secant)) <= 1e-5 ? None : Some(Knot(span, entry, exit));

        static double Knot(Interval span, double entry, double exit) {
            const double Bound = 0.2;
            return (span.Secant < 0d ? (-entry, -exit, -span.Secant) : (entry, exit, span.Secant)) switch {
                var (early, late, secant) => (double.Min(early, late), double.Max(early, late)) switch {
                    var (low, high) => span.Start.X + (0.5 * span.Width) + ((early > late ? 1d : -1d) * (0.5 - Bound) * span.Width
                        * double.Clamp((((high - low) / double.Max(0.01, high)) - 0.05) / (0.75 - 0.05), 0d, 1d)
                        * (secant >= high * 4d ? 0d
                            : secant > high * 1.1 ? 1d - ((secant - (high * 1.1)) / ((high * 4d) - (high * 1.1)))
                            : secant >= low + ((1d - (0.5 * Bound)) * (high - low)) ? 1d
                            : secant > low + (0.5 * Bound * (high - low)) && high != low ? (2d * (secant - (low + (0.5 * Bound * (high - low)))) / ((1d - Bound) * (high - low))) - 1d
                            : -1d)),
                },
            };
        }
    }

    private static Seq<double> Slopes(Seq<CurvePoint> points, bool harmonic) {
        return Interval.Between(points) switch {
            [var only] => Seq(only.Secant, only.Secant),
            var spans => Ended(spans, harmonic ? Harmonic(spans) : Weighted(spans), harmonic ? double.NegativeInfinity : 0.01),
        };

        static Seq<double> Ended(Seq<Interval> spans, Seq<double> interior, double floor) =>
            fun((Interval span, double inner) => double.Max(floor, 0.5 * ((3d * span.Secant) - inner))) switch {
                var end => (spans.Head, interior.Head).Apply(end).As().ToSeq().Concat(interior).Concat((spans.Last, interior.Last).Apply(end).As().ToSeq()),
            };

        static Seq<double> Harmonic(Seq<Interval> spans) =>
            spans.Zip(spans.Tail, static (left, right) => left.Secant * right.Secant <= 0d ? 0d
                : 2d * right.Secant * left.Secant / ((right.Secant + left.Secant) switch {
                    var sum when Math.Abs(sum) < 1e-3 => sum < 0d ? -1e-3 : 1e-3,
                    var sum => sum,
                }));

        static Seq<double> Weighted(Seq<Interval> spans) =>
            spans.Zip(spans.Tail).Scan(0, static (run, pair) => Math.Abs(pair.Second.Secant - pair.First.Secant) < 1e-6 ? run : run + 1).Strict() switch {
                var runs => runs.Map(run => runs.Zip(spans).Filter(member => member.First == run).Fold(0d, static (sum, member) => sum + member.Second.Length)).Strict() switch {
                    var lengths => spans.Zip(lengths).Zip(spans.Tail.Zip(lengths.Tail), static (left, right) =>
                        ((right.Second * right.First.Secant) + (left.Second * left.First.Secant)) / (right.Second + left.Second)),
                },
            };
    }

    private static Seq<double> Rescaled<T>(Seq<double> slopes, Seq<T> steps, Func<T, double, double, Option<double>> scale) =>
        steps.Map(static (step, index) => (Step: step, Index: index)).Fold(slopes, (current, item) =>
            scale(item.Step, current[item.Index], current[item.Index + 1]).Match(
                Some: factor => current.Map((slope, index) => index == item.Index || index == item.Index + 1 ? slope * factor : slope),
                None: () => current));

    private sealed record Interval(CurvePoint Start, CurvePoint End) {
        public double Width => End.X - Start.X;
        public double Rise => End.Y - Start.Y;
        public double Secant => Rise / Width;
        public double Length => double.Hypot(Width, Rise);

        public static Seq<Interval> Between(Seq<CurvePoint> points) => points.Zip(points.Tail, static (start, end) => new Interval(start, end));

        public static Seq<T> Of<T>(Seq<CurvePoint> points, Seq<double> slopes, Func<Interval, double, double, T> step) =>
            Between(points).Zip(slopes.Zip(slopes.Tail), (span, pair) => step(span, pair.First, pair.Second));

        public Seq<(double Knot, double C, double B, double A)> Segments(double entry, double exit, Option<double> split) =>
            split.Match(
                Some: knot => (knot - Start.X) switch {
                    var run => ((2d * Secant) - exit + ((exit - entry) * run / Width)) switch {
                        var bar => Seq(
                            (Start.X, Start.Y, entry, 0.5 * (bar - entry) / run),
                            (knot, Start.Y + (entry * run) + (0.5 * (bar - entry) * run), bar, 0.5 * (exit - bar) / (End.X - knot))),
                    },
                },
                None: () => Seq((Start.X, Start.Y, entry, 0.5 * (exit - entry) / Width)));
    }

    private sealed record FitInput(Seq<CurvePoint> Points, Seq<double> Slopes, Func<Interval, double, double, Option<double>> Split) {
        public QuadraticSpline Spline() =>
            Interval.Of(Points, Slopes, (span, entry, exit) => span.Segments(entry, exit, Split(span, entry, exit))).Flatten().Strict() switch {
                var inner => (Points, Slopes, inner.Rev()) switch {
                    ([var first, .., var last], [var slope, ..], [var end, ..]) => (last.X - end.Knot) switch {
                        var t => new QuadraticSpline(
                            [first.X, .. inner.Map(static segment => segment.Knot), last.X, last.X],
                            [first.Y, .. inner.Map(static segment => segment.C), (((end.A * t) + end.B) * t) + end.C],
                            [slope, .. inner.Map(static segment => segment.B), (2d * end.A * t) + end.B],
                            [0d, .. inner.Map(static segment => segment.A), 0d]),
                    },
                    _ => throw new UnreachableException(),
                },
            };
    }
}

[ComplexValueObject]
[ValidationError<InvalidGrade>]
[ObjectFactory<string>]
public sealed partial class PointCurve : IConvertible<string> {
    private static readonly Func<PointCurve, Func<double, double>> Fits = memo(static (PointCurve curve) => SplineShape.BSpline.Fit(curve.Points, CurveAxis.Stops.Unit, CurveAxis.Stops.Unit));

    public Seq<CurvePoint> Points { get; }

    public static PointCurve Identity { get; } = new(Seq(new CurvePoint(CurveAxis.Stops.Span.Low, CurveAxis.Stops.Span.Low), new CurvePoint(0d, 0d), new CurvePoint(CurveAxis.Stops.Span.High, CurveAxis.Stops.Span.High)));

    public Func<double, double> Fit() => Fits(this);

    public string ToValue() => CurvePoint.Format(Points);

    public static InvalidGrade? Validate(string? value, IFormatProvider? provider, out PointCurve? item) {
        item = null;
        return value is null ? null : CurvePoint.Parse(value).Case is Seq<CurvePoint> points ? Validate(points, out item) : new InvalidGrade();
    }

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref Seq<CurvePoint> points) =>
        validationError = points.Count >= 2
            && points.ForAll(static point => CurveAxis.Stops.Contains(point.X) && CurveAxis.Stops.Contains(point.Y))
            && points.Zip(points.Tail).ForAll(static pair => pair.First.X < pair.Second.X && pair.First.Y <= pair.Second.Y)
                ? null
                : new InvalidGrade();
}

[SmartEnum]
public sealed partial class CurveChannel {
    public static readonly CurveChannel Red = new(Lens<PointCurves, PointCurve>.New(static curves => curves.Red, static curve => curves => curves with { Red = curve }));
    public static readonly CurveChannel Green = new(Lens<PointCurves, PointCurve>.New(static curves => curves.Green, static curve => curves => curves with { Green = curve }));
    public static readonly CurveChannel Blue = new(Lens<PointCurves, PointCurve>.New(static curves => curves.Blue, static curve => curves => curves with { Blue = curve }));
    public static readonly CurveChannel Master = new(Lens<PointCurves, PointCurve>.New(static curves => curves.Master, static curve => curves => curves with { Master = curve }));

    public Lens<PointCurves, PointCurve> Curve { get; }

    public PointCurveParameter Parameter =>
        Map(red: PointCurveParameter.Red, green: PointCurveParameter.Green, blue: PointCurveParameter.Blue, master: PointCurveParameter.Master);
}

public sealed record PointCurves(PointCurve Red, PointCurve Green, PointCurve Blue, PointCurve Master, Factor PreserveHue)
    : IStateRecord<PointCurves, PointCurveParameter, InvalidGrade>, IPixelStage<PointCurves> {
    public static PointCurves Default { get; } = new(PointCurve.Identity, PointCurve.Identity, PointCurve.Identity, PointCurve.Identity, Factor.MinValue);

    public double Read(CurveChannel channel, Vector4 pixel, Gamut gamut) =>
        channel.Switch(
            (Curves: this, Pixel: pixel, Gamut: gamut),
            red: static state => CurveAxis.ToStops(state.Pixel.X),
            green: static state => CurveAxis.ToStops(state.Pixel.Y),
            blue: static state => CurveAxis.ToStops(state.Pixel.Z),
            master: static state => CurveAxis.ToStops(Vector3.Dot(state.Gamut.Luminance, new Vector3(
                (float)CurveAxis.FromStops(Mapped(state.Curves.Red.Fit(), state.Pixel.X)),
                (float)CurveAxis.FromStops(Mapped(state.Curves.Green.Fit(), state.Pixel.Y)),
                (float)CurveAxis.FromStops(Mapped(state.Curves.Blue.Fit(), state.Pixel.Z))))));

    public static Option<PixelPass> Pass(PointCurves state, PassContext context) =>
        state with { PreserveHue = Default.PreserveHue } == Default
            ? None
            : (state.Red.Fit(), state.Green.Fit(), state.Blue.Fit(), state.Master.Fit(), state.PreserveHue) switch {
                var (red, green, blue, master, preserve) => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row) {
                        (double r, double g, double b) = (Mapped(red, pixel.X), Mapped(green, pixel.Y), Mapped(blue, pixel.Z));
                        pixel = new Vector4(ToneCurves.PreserveHue(
                            new Vector3((float)CurveAxis.FromStops(r), (float)CurveAxis.FromStops(g), (float)CurveAxis.FromStops(b)),
                            new Vector3((float)CurveAxis.FromStops(master(r)), (float)CurveAxis.FromStops(master(g)), (float)CurveAxis.FromStops(master(b))),
                            preserve), pixel.W);
                    }
                })),
            };

    private static double Mapped(Func<double, double> channel, float value) => channel(CurveAxis.ToStops(value));
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointCurveParameter : IStateParameter<PointCurves> {
    public static readonly PointCurveParameter Red = new("red", new StateParameter<PointCurves>.Keyed<PointCurve, string, InvalidGrade>(CurveChannel.Red.Curve));
    public static readonly PointCurveParameter Green = new("green", new StateParameter<PointCurves>.Keyed<PointCurve, string, InvalidGrade>(CurveChannel.Green.Curve));
    public static readonly PointCurveParameter Blue = new("blue", new StateParameter<PointCurves>.Keyed<PointCurve, string, InvalidGrade>(CurveChannel.Blue.Curve));
    public static readonly PointCurveParameter Master = new("master", new StateParameter<PointCurves>.Keyed<PointCurve, string, InvalidGrade>(CurveChannel.Master.Curve));
    public static readonly PointCurveParameter PreserveHue = new(
        "preserve-hue",
        new StateParameter<PointCurves>.Bounded<Factor, float, InvalidToneValue>(
            Lens<PointCurves, Factor>.New(static curves => curves.PreserveHue, static preserve => curves => curves with { PreserveHue = preserve }),
            new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));

    public StateParameter<PointCurves> Kind { get; }
}
