using System.Numerics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Hsy(double Hue, double Saturation, double Luma) {
    private const double Knee = 0.15d;
    private const double LowGain = 5d;
    private const double Scale = 1.4d;
    private const double MinLuma = 0.001d;
    private const double MaxLuma = 0.01d;

    public static (double X, double Y, double Z) LumaWeights { get; } = Gamut.StandardRgb.Luminance;

    public static double SaturationPeak =>
        LumaWeights switch {
            var weights => Scale * (1d + ((weights.X + weights.Y + weights.Z) / double.Min(double.Min(weights.X, weights.Y), weights.Z))) / 3d,
        };

    public static Hsy From(Vector4 color) {
        (double red, double green, double blue) = (color.X, color.Y, color.Z);
        double luma = Luminance(red, green, blue);
        double distance = double.Abs(red - luma) + double.Abs(green - luma) + double.Abs(blue - luma);
        double max = double.Max(double.Max(red, green), blue);
        double min = double.Min(double.Min(red, green), blue);
        double sextant = min == max ? 0d
            : red == max ? 1d + ((green - blue) / (max - min))
            : green == max ? 3d + ((blue - red) / (max - min))
            : 5d + ((red - green) / (max - min));
        double turn = sextant / 6d;
        return new((turn - double.Floor(turn)) * double.Tau, distance * SaturationGain(luma), luma);
    }

    public static Vector3 Saturated(double hue) =>
        HueRgb(hue) switch {
            var (red, green, blue) => new((float)red, (float)green, (float)blue),
        };

    public Vector4 ToRgb(float alpha) {
        (double red, double green, double blue) = HueRgb(Hue);
        double luma = Luminance(red, green, blue);
        (red, green, blue) = (red - luma, green - luma, blue - luma);
        double gain = Saturation / SaturationGain(Luma) / (double.Abs(red) + double.Abs(green) + double.Abs(blue));
        return new((float)(Luma + (gain * red)), (float)(Luma + (gain * green)), (float)(Luma + (gain * blue)), alpha);
    }

    private static double Luminance(double red, double green, double blue) =>
        LuminanceSamples.Luminance(red, green, blue, LumaWeights) / (LumaWeights.X + LumaWeights.Y + LumaWeights.Z);

    private static double SaturationGain(double luma) =>
        luma switch {
            <= MinLuma => Scale * LowGain,
            >= MaxLuma => Scale / (Knee + (3d * luma)),
            _ => ((luma - MinLuma) / (MaxLuma - MinLuma)) switch {
                var blend => Scale * (((1d - blend) * LowGain) + (blend / (Knee + (3d * luma)))),
            },
        };

    private static (double Red, double Green, double Blue) HueRgb(double hue) =>
        ((hue % double.Tau / double.Tau) - (1d / 6d)) switch {
            var turn => ((turn - double.Floor(turn)) * 6d) switch {
                < 1d and var sextant => (1d, sextant, 0d),
                < 2d and var sextant => (2d - sextant, 1d, 0d),
                < 3d and var sextant => (0d, 1d, sextant - 2d),
                < 4d and var sextant => (0d, 4d - sextant, 1d),
                < 5d and var sextant => (sextant - 4d, 0d, 1d),
                var sextant => (1d, 0d, 6d - sextant),
            },
        };
}

public sealed record RangeBand(AxisFraction Center, AxisFraction Width, AxisFraction Softness) {
    public float Membership(float distance) =>
        (distance - (Width / 2f)) switch {
            <= 0f => 1f,
            var outside when outside >= Softness => 0f,
            var outside => (outside / Softness) switch { var t => 1f - (t * t * (3f - (2f * t))) },
        };
}

public sealed record HueRange(RangeBand Hue, RangeBand Saturation, RangeBand Luma) : IStateRecord<HueRange, HueRangeParameter> {
    public static HueRange Default { get; } = new RangeBand(AxisFraction.Half, AxisFraction.MaxValue, AxisFraction.MinValue) switch { var open => new HueRange(open, open, open) };

    public float Membership(Hsy hsy) =>
        RangeAxis.Hue.Membership(Hue, hsy) * RangeAxis.Saturation.Membership(Saturation, hsy) * RangeAxis.Luma.Membership(Luma, hsy);
}

[SmartEnum]
public sealed partial class RangeAxis {
    private static readonly Lens<RangeBand, AxisFraction> Centers =
        Lens<RangeBand, AxisFraction>.New(static band => band.Center, static center => band => band with { Center = center });
    private static readonly Lens<RangeBand, AxisFraction> Widths =
        Lens<RangeBand, AxisFraction>.New(static band => band.Width, static width => band => band with { Width = width });
    private static readonly Lens<RangeBand, AxisFraction> Softnesses =
        Lens<RangeBand, AxisFraction>.New(static band => band.Softness, static softness => band => band with { Softness = softness });

    public static readonly RangeAxis Hue = new(
        InputAxis.Hue, Lens<HueRange, RangeBand>.New(static range => range.Hue, static band => range => range with { Hue = band }),
        AxisFraction.Presentation with { Unit = Quantity.GetUnitInfo(AngleUnit.Revolution) },
        static (hsy, value) => hsy with { Hue = value });
    public static readonly RangeAxis Saturation = new(
        InputAxis.Saturation, Lens<HueRange, RangeBand>.New(static range => range.Saturation, static band => range => range with { Saturation = band }),
        AxisFraction.Presentation,
        static (hsy, value) => hsy with { Saturation = value });
    public static readonly RangeAxis Luma = new(
        InputAxis.Stops, Lens<HueRange, RangeBand>.New(static range => range.Luma, static band => range => range with { Luma = band }),
        AxisFraction.Presentation,
        static (hsy, value) => hsy with { Luma = CurveAxis.FromStops(value) });

    public InputAxis Input { get; }
    public Lens<HueRange, RangeBand> Band { get; }
    public Presentation<float> Presentation { get; }

    public (Lens<TRecord, AxisFraction> Center, Lens<TRecord, AxisFraction> Width, Lens<TRecord, AxisFraction> Softness) Members<TRecord>(Lens<TRecord, HueRange> range) =>
        lens(range, Band) switch {
            var band => (lens(band, Centers), lens(band, Widths), lens(band, Softnesses)),
        };

    public (StateParameter<HueRange> Center, StateParameter<HueRange> Width, StateParameter<HueRange> Softness) Kinds =>
        (Members(Lens.identity<HueRange>()), fun((Lens<HueRange, AxisFraction> member) => StateParameter.Bounded(member, Presentation))) switch {
            var ((center, width, softness), kind) => (kind(center), kind(width), kind(softness)),
        };

    [UseDelegateFromConstructor]
    private partial Hsy Write(Hsy hsy, double value);

    public float Coordinate(Hsy hsy) =>
        Input.Axis.Span switch {
            var (low, high) => ((Input.Read(hsy) - low) / (high - low)) switch { var u => (float)(Input.Axis.Periodic ? u - double.Floor(u) : double.Clamp(u, 0d, 1d)) },
        };

    public float Distance(float coordinate, float center) =>
        (coordinate - center) switch { var d => Input.Axis.Periodic ? float.Abs(d + 0.5f - float.Floor(d + 0.5f) - 0.5f) : float.Abs(d) };

    public Hsy Replaced(Hsy hsy, float coordinate) =>
        Input.Axis.Span switch { var (low, high) => Write(hsy, low + (coordinate * (high - low))) };

    public float Membership(RangeBand band, Hsy hsy) => band.Membership(Distance(Coordinate(hsy), band.Center));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HueRangeParameter : IStateParameter<HueRange> {
    public static readonly HueRangeParameter HueCenter = new("hue-center", RangeAxis.Hue.Kinds.Center,
        new("Center", "Middle of the band the range selects"));
    public static readonly HueRangeParameter HueWidth = new("hue-width", RangeAxis.Hue.Kinds.Width,
        new("Width", "Span fully inside the band"));
    public static readonly HueRangeParameter HueSoftness = new("hue-softness", RangeAxis.Hue.Kinds.Softness,
        new("Softness", "Falloff span outside each edge of the band"));
    public static readonly HueRangeParameter SaturationCenter = new("saturation-center", RangeAxis.Saturation.Kinds.Center,
        new("Center", "Middle of the band the range selects"));
    public static readonly HueRangeParameter SaturationWidth = new("saturation-width", RangeAxis.Saturation.Kinds.Width,
        new("Width", "Span fully inside the band"));
    public static readonly HueRangeParameter SaturationSoftness = new("saturation-softness", RangeAxis.Saturation.Kinds.Softness,
        new("Softness", "Falloff span outside each edge of the band"));
    public static readonly HueRangeParameter LumaCenter = new("luma-center", RangeAxis.Luma.Kinds.Center,
        new("Center", "Middle of the band the range selects"));
    public static readonly HueRangeParameter LumaWidth = new("luma-width", RangeAxis.Luma.Kinds.Width,
        new("Width", "Span fully inside the band"));
    public static readonly HueRangeParameter LumaSoftness = new("luma-softness", RangeAxis.Luma.Kinds.Softness,
        new("Softness", "Falloff span outside each edge of the band"));

    public StateParameter<HueRange> Kind { get; }
    public ParameterText Text { get; }

    public static (HueRangeParameter Center, HueRangeParameter Width, HueRangeParameter Softness) Band(RangeAxis axis) =>
        axis.Map(
            hue: (HueCenter, HueWidth, HueSoftness),
            saturation: (SaturationCenter, SaturationWidth, SaturationSoftness),
            luma: (LumaCenter, LumaWidth, LumaSoftness));
}

public sealed record SelectiveColor(HueRange Range, SignedAngle Shift, Magnitude Saturation, Exposure Exposure)
    : IStateRecord<SelectiveColor, SelectiveColorParameter>, IPixelStage<SelectiveColor> {
    public static SelectiveColor Default { get; } = new(HueRange.Default, SignedAngle.Neutral, Magnitude.Unit, Exposure.Neutral);

    public static Lens<SelectiveColor, HueRange> RangeOf { get; } =
        Lens<SelectiveColor, HueRange>.New(static state => state.Range, static range => state => state with { Range = range });

    public Func<Vector4, Vector4> Graded() =>
        (Range, (float)Shift, (float)Saturation, Exposure.Scale, Shift == SignedAngle.Neutral && Saturation == Magnitude.Unit) switch {
            var (range, shift, saturation, gain, kept) => color => Hsy.From(color) switch {
                var hsy => new(
                    Vector3.Lerp(
                        color.AsVector3(),
                        (kept ? color : (hsy with { Hue = hsy.Hue + shift, Saturation = hsy.Saturation * saturation }).ToRgb(color.W)).AsVector3() * gain,
                        range.Membership(hsy)),
                    color.W),
            },
        };

    public static Option<PixelPass> Pass(SelectiveColor state, PassContext context) =>
        state.Shift == SignedAngle.Neutral && state.Saturation == Magnitude.Unit && state.Exposure == Exposure.Neutral
            ? None
            : state.Graded() switch {
                var graded => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row)
                        pixel = graded(pixel);
                })),
            };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SelectiveColorParameter : IStateParameter<SelectiveColor> {
    private static readonly Ramp Shifts = Ramp.Create(
        toSeq(Enumerable.Range(0, 7)).Map(static sextant => (sextant / 6d) switch {
            var at => RampStop.Create(RampPosition.Create(at), new Vector4(Hsy.Saturated(Hsy.From(Vector4.UnitX).Hue + float.Lerp(SignedAngle.MinValue, SignedAngle.MaxValue, (float)at)), 1f)),
        }),
        RampInterpolation.Linear);

    public static readonly SelectiveColorParameter Range = new("range", new StateParameter<SelectiveColor>.Record<HueRange>(SelectiveColor.RangeOf),
        new("Range", "Band of the stage input the grade acts on"));
    public static readonly SelectiveColorParameter HueShift = new("hue-shift", StateParameter.Bounded(Lens<SelectiveColor, SignedAngle>.New(static state => state.Shift, static shift => state => state with { Shift = shift }), SignedAngle.Presentation with { Stops = Shifts }),
        new("Hue shift", "Turn added to the hue of qualified pixels"));
    public static readonly SelectiveColorParameter Saturation = new("saturation", StateParameter.Bounded(Lens<SelectiveColor, Magnitude>.New(static state => state.Saturation, static saturation => state => state with { Saturation = saturation }), ContrastGradeParameter.Chroma),
        new("Saturation", "Saturation gain on qualified pixels"));
    public static readonly SelectiveColorParameter Exposure = new("exposure", StateParameter.Bounded(Lens<SelectiveColor, Exposure>.New(static state => state.Exposure, static exposure => state => state with { Exposure = exposure })),
        new("Exposure", "Stops added to qualified pixels"));

    public StateParameter<SelectiveColor> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record HueIsolation(HueRange Range) : IStateRecord<HueIsolation, HueIsolationParameter>, IPixelStage<HueIsolation> {
    public static HueIsolation Default { get; } = new(HueRange.Default);

    public static Lens<HueIsolation, HueRange> RangeOf { get; } =
        Lens<HueIsolation, HueRange>.New(static state => state.Range, static range => state => state with { Range = range });

    public Func<Vector4, Vector4> Graded((double X, double Y, double Z) luminance) =>
        color => Range.Membership(Hsy.From(color)) switch {
            1f => color,
            var membership => ((1d - membership) * LuminanceSamples.Luminance(color.X, color.Y, color.Z, luminance)) switch {
                var gray => new((float)(gray + (membership * (double)color.X)), (float)(gray + (membership * (double)color.Y)), (float)(gray + (membership * (double)color.Z)), color.W),
            },
        };

    public static Option<PixelPass> Pass(HueIsolation state, PassContext context) =>
        state.Range == HueRange.Default
            ? None
            : state.Graded(context.Working.Luminance) switch {
                var graded => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row)
                        pixel = graded(pixel);
                })),
            };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HueIsolationParameter : IStateParameter<HueIsolation> {
    public static readonly HueIsolationParameter Range = new("range", new StateParameter<HueIsolation>.Record<HueRange>(HueIsolation.RangeOf),
        new("Range", "Band of the stage input kept in color"));

    public StateParameter<HueIsolation> Kind { get; }
    public ParameterText Text { get; }
}
