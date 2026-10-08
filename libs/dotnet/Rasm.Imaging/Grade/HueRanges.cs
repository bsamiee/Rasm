using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Hsy(float Hue, float Saturation, float Luma) {
    private const float Knee = 0.15f;
    private const float LowGain = 5f;
    private const float Scale = 1.4f;

    public static Vector3 LumaWeights { get; } = Gamut.StandardRgb.Luminance;

    public static Hsy From(Vector4 color) {
        Vector3 rgb = color.AsVector3();
        float luma = Vector3.Dot(LumaWeights, rgb);
        float distance = Vector3.Dot(Vector3.Abs(rgb - new Vector3(luma)), Vector3.One);
        float low = distance * LowGain;
        float high = distance / float.Max((0.07f * distance) + 1e-6f, Knee + Vector3.Dot(rgb, Vector3.One));
        float max = float.Max(float.Max(rgb.X, rgb.Y), rgb.Z);
        float min = float.Min(float.Min(rgb.X, rgb.Y), rgb.Z);
        float sextant = min == max ? 0f
            : rgb.X == max ? 1f + ((rgb.Y - rgb.Z) / (max - min))
            : rgb.Y == max ? 3f + ((rgb.Z - rgb.X) / (max - min))
            : 5f + ((rgb.X - rgb.Y) / (max - min));
        return new(sextant * 0.16666666666666666f * float.Tau, (low + (Blend(luma) * (high - low))) * Scale, luma);
    }

    public static Vector3 Saturated(float hue) =>
        ((hue / float.Tau) - (1f / 6f)) switch {
            var turn => ((turn - float.Floor(turn)) * 6f) switch {
                var sextant => Vector3.Clamp(new Vector3(float.Abs(sextant - 3f) - 1f, 2f - float.Abs(sextant - 2f), 2f - float.Abs(sextant - 4f)), Vector3.Zero, Vector3.One),
            },
        };

    public Vector4 ToRgb(float alpha) {
        static float Root(float a, float b, float c) =>
            float.Sqrt((b * b) - (4f * a * c)) switch {
                var root => (2f * c / (-root - b)) switch { >= 0f and var gain => gain, _ => 2f * c / (root - b) },
            };
        Vector3 hue = Saturated(Luma < 0f ? Hue + float.Pi : Hue);
        Vector3 rgb = hue * (Luma / Vector3.Dot(LumaWeights, hue));
        float distance = Vector3.Dot(Vector3.Abs(rgb - new Vector3(Luma)), Vector3.One);
        float excess = Vector3.Dot(rgb, Vector3.One) - (3f * Luma);
        float saturation = Saturation / Scale;
        float knee = Knee + (3f * Luma);
        float blend = Blend(Luma);
        float gain = blend switch {
            1f => float.Min(saturation * knee / float.Max(1e-6f, distance - (saturation * excess)), 50f),
            0f => saturation / float.Max(1e-10f, distance * LowGain),
            _ => (distance * LowGain * (1f - blend)) switch {
                var low => Root(low * excess, (low * knee) + (distance * blend) - (saturation * excess), -saturation * knee),
            },
        };
        return new(new Vector3(Luma) + (gain * (rgb - new Vector3(Luma))), alpha);
    }

    private static float Blend(float luma) => float.Clamp((luma - 1e-3f) / (1e-2f - 1e-3f), 0f, 1f);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct AxisFraction : IMinMaxValue<AxisFraction> {
    public static AxisFraction MinValue { get; } = new(0f);
    public static AxisFraction MaxValue { get; } = new(1f);
    public static AxisFraction Half { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

public sealed record RangeBand(AxisFraction Center, AxisFraction Width, AxisFraction Softness) {
    public float Membership(float distance) =>
        (distance - (Width / 2f)) switch {
            <= 0f => 1f,
            var outside when outside >= Softness => 0f,
            var outside => (outside / Softness) switch { var t => 1f - (t * t * (3f - (2f * t))) },
        };
}

public sealed record HueRange(RangeBand Hue, RangeBand Saturation, RangeBand Luma) {
    public static HueRange All { get; } = new RangeBand(AxisFraction.Half, AxisFraction.MaxValue, AxisFraction.MinValue) switch { var open => new HueRange(open, open, open) };

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
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Revolution) },
        static (hsy, value) => hsy with { Hue = (float)value });
    public static readonly RangeAxis Saturation = new(
        InputAxis.Saturation, Lens<HueRange, RangeBand>.New(static range => range.Saturation, static band => range => range with { Saturation = band }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) },
        static (hsy, value) => hsy with { Saturation = (float)value });
    public static readonly RangeAxis Luma = new(
        InputAxis.Stops, Lens<HueRange, RangeBand>.New(static range => range.Luma, static band => range => range with { Luma = band }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) },
        static (hsy, value) => hsy with { Luma = (float)CurveAxis.FromStops(value) });

    public InputAxis Input { get; }
    public Lens<HueRange, RangeBand> Band { get; }
    public Presentation<AxisFraction, float> Presentation { get; }

    public (Lens<TRecord, AxisFraction> Center, Lens<TRecord, AxisFraction> Width, Lens<TRecord, AxisFraction> Softness) Members<TRecord>(Lens<TRecord, HueRange> range) =>
        lens(range, Band) switch {
            var band => (lens(band, Centers), lens(band, Widths), lens(band, Softnesses)),
        };

    public (StateParameter<TRecord> Center, StateParameter<TRecord> Width, StateParameter<TRecord> Softness) Kinds<TRecord>(Lens<TRecord, HueRange> range) =>
        (Members(range), fun((Lens<TRecord, AxisFraction> member) => (StateParameter<TRecord>)new StateParameter<TRecord>.Bounded<AxisFraction, float, InvalidGrade>(member, Presentation))) switch {
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

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct HueShift : IMinMaxValue<HueShift> {
    public static HueShift MinValue { get; } = new(-float.Pi);
    public static HueShift MaxValue { get; } = new(float.Pi);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

public sealed record SelectiveColor(HueRange Range, HueShift Shift, Saturation Saturation, Exposure Exposure)
    : IStateRecord<SelectiveColor, SelectiveColorParameter, InvalidGrade>, IPixelStage<SelectiveColor> {
    public static SelectiveColor Default { get; } = new(HueRange.All, HueShift.Neutral, Saturation.Neutral, Exposure.Neutral);

    public Func<Vector4, Vector4> Graded() =>
        (Range, (float)Shift, (float)Saturation, Exposure.Scale, Shift == HueShift.Neutral && Saturation == Saturation.Neutral) switch {
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
        state.Shift == HueShift.Neutral && state.Saturation == Saturation.Neutral && state.Exposure == Exposure.Neutral
            ? None
            : state.Graded() switch {
                var graded => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row)
                        pixel = graded(pixel);
                })),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SelectiveColorParameter : IStateParameter<SelectiveColor> {
    public static Lens<SelectiveColor, HueRange> Range { get; } =
        Lens<SelectiveColor, HueRange>.New(static state => state.Range, static range => state => state with { Range = range });

    private static readonly Ramp Shifts = Valid.Value(
        Ramp.Validate(
            toSeq(Enumerable.Range(0, 7)).Map(static sextant => (sextant / 6d) switch {
                var at => Valid.Value(
                    RampStop.Validate(
                        Valid.Value(RampPosition.Validate(at, provider: null, out RampPosition position), position),
                        new Vector4(Hsy.Saturated(Hsy.From(Vector4.UnitX).Hue + float.Lerp(Grade.HueShift.MinValue, Grade.HueShift.MaxValue, (float)at)), 1f),
                        out RampStop stop),
                    stop),
            }),
            RampInterpolation.Linear,
            out Ramp? shifts),
        shifts);

    public static readonly SelectiveColorParameter HueCenter = new("hue-center", RangeAxis.Hue.Kinds(Range).Center);
    public static readonly SelectiveColorParameter HueWidth = new("hue-width", RangeAxis.Hue.Kinds(Range).Width);
    public static readonly SelectiveColorParameter HueSoftness = new("hue-softness", RangeAxis.Hue.Kinds(Range).Softness);
    public static readonly SelectiveColorParameter SaturationCenter = new("saturation-center", RangeAxis.Saturation.Kinds(Range).Center);
    public static readonly SelectiveColorParameter SaturationWidth = new("saturation-width", RangeAxis.Saturation.Kinds(Range).Width);
    public static readonly SelectiveColorParameter SaturationSoftness = new("saturation-softness", RangeAxis.Saturation.Kinds(Range).Softness);
    public static readonly SelectiveColorParameter LumaCenter = new("luma-center", RangeAxis.Luma.Kinds(Range).Center);
    public static readonly SelectiveColorParameter LumaWidth = new("luma-width", RangeAxis.Luma.Kinds(Range).Width);
    public static readonly SelectiveColorParameter LumaSoftness = new("luma-softness", RangeAxis.Luma.Kinds(Range).Softness);
    public static readonly SelectiveColorParameter HueShift = new(
        "hue-shift",
        new StateParameter<SelectiveColor>.Bounded<HueShift, float, InvalidGrade>(
            Lens<SelectiveColor, HueShift>.New(static state => state.Shift, static shift => state => state with { Shift = shift }),
            new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian), Origin = 0f, Stops = Shifts }));
    public static readonly SelectiveColorParameter Saturation = new(
        "saturation",
        new StateParameter<SelectiveColor>.Bounded<Saturation, float, InvalidGrade>(
            Lens<SelectiveColor, Saturation>.New(static state => state.Saturation, static saturation => state => state with { Saturation = saturation }),
            ContrastGradeParameter.Chroma));
    public static readonly SelectiveColorParameter Exposure = new(
        "exposure",
        new StateParameter<SelectiveColor>.Bounded<Exposure, float, InvalidToneValue>(
            Lens<SelectiveColor, Exposure>.New(static state => state.Exposure, static exposure => state => state with { Exposure = exposure }),
            Tone.Exposure.Presentation));

    public StateParameter<SelectiveColor> Kind { get; }

    public static (SelectiveColorParameter Center, SelectiveColorParameter Width, SelectiveColorParameter Softness) Band(RangeAxis axis) =>
        axis.Map(
            hue: (HueCenter, HueWidth, HueSoftness),
            saturation: (SaturationCenter, SaturationWidth, SaturationSoftness),
            luma: (LumaCenter, LumaWidth, LumaSoftness));
}

public sealed record HueIsolation(HueRange Range) : IStateRecord<HueIsolation, HueIsolationParameter, InvalidGrade>, IPixelStage<HueIsolation> {
    public static HueIsolation Default { get; } = new(HueRange.All);

    public Func<Vector4, Vector4> Graded(Vector3 luminance) =>
        color => new(Vector3.Lerp(new Vector3(Vector3.Dot(luminance, color.AsVector3())), color.AsVector3(), Range.Membership(Hsy.From(color))), color.W);

    public static Option<PixelPass> Pass(HueIsolation state, PassContext context) =>
        state.Range == HueRange.All
            ? None
            : state.Graded(context.Working.Luminance) switch {
                var graded => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row)
                        pixel = graded(pixel);
                })),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HueIsolationParameter : IStateParameter<HueIsolation> {
    public static Lens<HueIsolation, HueRange> Range { get; } =
        Lens<HueIsolation, HueRange>.New(static state => state.Range, static range => state => state with { Range = range });

    public static readonly HueIsolationParameter HueCenter = new("hue-center", RangeAxis.Hue.Kinds(Range).Center);
    public static readonly HueIsolationParameter HueWidth = new("hue-width", RangeAxis.Hue.Kinds(Range).Width);
    public static readonly HueIsolationParameter HueSoftness = new("hue-softness", RangeAxis.Hue.Kinds(Range).Softness);
    public static readonly HueIsolationParameter SaturationCenter = new("saturation-center", RangeAxis.Saturation.Kinds(Range).Center);
    public static readonly HueIsolationParameter SaturationWidth = new("saturation-width", RangeAxis.Saturation.Kinds(Range).Width);
    public static readonly HueIsolationParameter SaturationSoftness = new("saturation-softness", RangeAxis.Saturation.Kinds(Range).Softness);
    public static readonly HueIsolationParameter LumaCenter = new("luma-center", RangeAxis.Luma.Kinds(Range).Center);
    public static readonly HueIsolationParameter LumaWidth = new("luma-width", RangeAxis.Luma.Kinds(Range).Width);
    public static readonly HueIsolationParameter LumaSoftness = new("luma-softness", RangeAxis.Luma.Kinds(Range).Softness);

    public StateParameter<HueIsolation> Kind { get; }

    public static (HueIsolationParameter Center, HueIsolationParameter Width, HueIsolationParameter Softness) Band(RangeAxis axis) =>
        axis.Map(
            hue: (HueCenter, HueWidth, HueSoftness),
            saturation: (SaturationCenter, SaturationWidth, SaturationSoftness),
            luma: (LumaCenter, LumaWidth, LumaSoftness));
}
