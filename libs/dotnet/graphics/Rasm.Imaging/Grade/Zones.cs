using System.Numerics;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct ZoneSpan(Vector4 Values, float Start, float Width) {
    public static ZoneSpan Placed(float start, float width) => new(Vector4.One, start, width);

    public Func<Vector3, Vector3> PerChannel(Func<float, Func<float, float>> curve) =>
        (curve(Values.X), curve(Values.Y), curve(Values.Z), curve(Values.W)) switch {
            var (red, green, blue, master) => rgb => new Vector3(master(red(rgb.X)), master(green(rgb.Y)), master(blue(rgb.Z))),
        };
}

internal sealed record GradingTone(ZoneSpan Blacks, ZoneSpan Shadows, ZoneSpan Midtones, ZoneSpan Highlights, ZoneSpan Whites, float SContrast) {
    private static readonly Func<float, float> Identity = static value => value;

    public Option<Action<Span<Vector4>>> Kernel(GradingStyle style) =>
        toSeq([Blacks, Shadows, Midtones, Highlights, Whites]).ForAll(static zone => zone.Values == Vector4.One) && SContrast is 1f
            ? None
            : Composed(style);

    private Action<Span<Vector4>> Composed(GradingStyle style) {
        Func<float, float> Shadow(float value) => Bent(value, slope => Spline.Bend(Shadows.Start - Shadows.Width, Shadows.Start, slope, 1f));
        Func<float, float> Highlight(float value) => Bent(2f - value, slope => Spline.Bend(Highlights.Start, Highlights.Start + Highlights.Width, 1f, slope));
        (Func<float, float> lift, Func<float, float> sink, Func<float, float> contrast) = (Highlight(Highlights.Values.W), Shadow(Shadows.Values.W), Contrasted(style, SContrast));
        Seq<Func<Vector3, Vector3>> zones = [
            Midtones.PerChannel(value => Middle(style, value, Midtones.Start, Midtones.Width)),
            Highlights.PerChannel(Highlight),
            Whites.PerChannel(value => White(lift(Whites.Start), lift(Whites.Start + Whites.Width), value)),
            Shadows.PerChannel(Shadow),
            Blacks.PerChannel(value => Black(sink(Blacks.Start - Blacks.Width), sink(Blacks.Start), value)),
            rgb => new Vector3(contrast(rgb.X), contrast(rgb.Y), contrast(rgb.Z)),
        ];
        return row => {
            foreach (ref Vector4 pixel in row) {
                Vector3 rgb = new(style.Encode(pixel.X), style.Encode(pixel.Y), style.Encode(pixel.Z));
                foreach (Func<Vector3, Vector3> zone in zones.AsSpan())
                    rgb = zone(rgb);
                pixel = new Vector4(Vector3.Min(new Vector3(style.Decode(rgb.X), style.Decode(rgb.Y), style.Decode(rgb.Z)), new Vector3((float)Half.MaxValue)), pixel.W);
            }
        };
    }

    private static Func<float, float> Bent(float value, Func<float, Spline> spline) =>
        value switch {
            1f => Identity,
            < 1f => spline(value).Forward,
            _ => spline(2f - value).Inverse,
        };

    private static Func<float, float> Middle(GradingStyle style, float value, float start, float width) =>
        value is 1f
            ? Identity
            : float.Min(width, (style.Top - style.Bottom) * 0.95f) switch {
                var span => float.Clamp(start, style.Bottom + (span * 0.51f), style.Top - (span * 0.51f)) switch {
                    var center => Balanced(style, center, span, (value - 1f) * 0.9f).Forward,
                },
            };

    private static Spline Balanced(GradingStyle style, float center, float span, float lean) =>
        (style.Bottom, center - (span * 0.5f), center - (span * 0.25f), center + (span * 0.25f), center + (span * 0.5f), style.Top, 1f + (lean * 0.4f), 1f + lean, 1f - lean, 1f - (lean * 0.4f)) switch {
            var (x0, x1, x2, x3, x4, x5, m1, m2, m3, m4) => (center <= (x5 + x0) * 0.5f
                ? (m1, (((x1 - x0) * (m1 - 1f) * 0.5f) + ((x2 - x1) * (m1 - 1f + ((m2 - m1) * 0.5f))) + ((center - x2) * (m2 - 1f) * 0.5f)
                        - (0.5f * (x5 - x4)) + ((x4 - x3) * ((0.5f * m3) - 1f)) + ((x3 - center) * (m3 - 1f) * 0.5f)) / (-0.5f * (x5 - x3)))
                : ((((x5 - x4) * (m4 - 1f) * 0.5f) + ((x4 - x3) * (m4 - 1f + ((m3 - m4) * 0.5f))) + ((x3 - center) * (m3 - 1f) * 0.5f)
                        - (0.5f * (x1 - x0)) + ((x2 - x1) * ((0.5f * m2) - 1f)) + ((center - x2) * (m2 - 1f) * 0.5f)) / (-0.5f * (x2 - x0)), m4)) switch {
                            var (low, high) => Spline.Of(new(x0, x0, 1f), [(x1, low), (x2, m2), (x3, m3), (x4, high), (x5, 1f)]),
                        },
        };

    private static Func<float, float> White(float x0, float x1, float value) =>
        value switch {
            1f => Identity,
            < 1f => Spline.Of(new(x0, x0, 1f), [(x1, value)]).Forward,
            _ => (2f - value, (3f - value) * 0.5f) switch {
                var (slope, gain) => (Spline.Of(new(x0, x0, 1f), [(x1, slope)]), Tail(x0, x1, slope, gain)) switch {
                    var (spline, (a, b, c)) => t => t >= x1 ? (((a * t) + b) * t) + c : spline.Gained(t, x0, gain),
                },
            },
        };

    private static (float A, float B, float C) Tail(float x0, float x1, float slope, float gain) =>
        (x0 + ((x1 - x0) * 0.99f)) switch {
            var xd => (0.5f * ((1f / slope) - (1f / (1f + ((xd - x0) * (slope - 1f) / (x1 - x0))))) / (x1 - xd)) switch {
                var a => ((1f / slope) - (2f * a * x1)) switch { var b => (a, b, ((x1 - x0) / gain) + x0 - (b * x1) - (a * x1 * x1)) },
            },
        };

    private static Func<float, float> Black(float x0, float x1, float value) =>
        value switch {
            1f => Identity,
            > 1f => Spline.Of(new(x0, x1 - ((3f - value) * 0.5f * (x1 - x0)), 2f - value), [(x1, 1f)]).Forward,
            _ => ((1f + value) * 0.5f) switch {
                var gain => Spline.Of(new(x0, x1 - (gain * (x1 - x0)), value), [(x1, 1f)]) switch {
                    var spline => t => spline.Gained(t, x1, gain),
                },
            },
        };

    private static Func<float, float> Contrasted(GradingStyle style, float contrast) =>
        contrast is 1f
            ? Identity
            : (contrast > 1f ? 1f / (1.8125f - (0.8125f * contrast)) : 0.28125f + (0.71875f * contrast)) switch {
                var slope => (Lower(style, slope), Upper(style, slope)) switch {
                    var (low, high) => Spline.Of(new(low.X1, low.Y2 - ((low.Slope + slope) * (low.X2 - low.X1) * 0.5f), low.Slope), [(low.X2, slope), (high.X1, slope), (high.X2, high.Slope)]).Forward,
                },
            };

    private static (float X1, float X2, float Slope) Upper(GradingStyle style, float slope) =>
        ((style.ContrastTop - style.Pivot) * 0.25f) switch {
            var reach => (style.Pivot + reach, style.Pivot + (reach / slope)) switch {
                var (y0, x0) => ((2f * (style.ContrastTop - y0 - (style.ContrastTop / slope) + (slope * x0)) / (slope - (1f / slope))) - x0, (style.ContrastTop - x0) * 0.3f) switch {
                    var (x2, least) when x2 - x0 >= least => (x0, x2, 1f / slope),
                    var (_, least) => (x0, x0 + least, (style.ContrastTop - y0 + (slope * x0) - ((x0 + (least * 0.5f)) * slope)) / (style.ContrastTop - x0 - (least * 0.5f))),
                },
            },
        };

    private static (float X1, float X2, float Slope, float Y2) Lower(GradingStyle style, float slope) =>
        ((style.Pivot - style.Bottom) * 0.25f) switch {
            var reach => (style.Pivot - reach, style.Pivot - (reach / slope)) switch {
                var (y3, x3) => ((2f * (y3 - style.Bottom - (slope * x3) + (style.Bottom / slope)) / ((1f / slope) - slope)) - x3, (x3 - style.Bottom) * 0.3f) switch {
                    var (x1, least) when x3 - x1 >= least => (x1, x3, 1f / slope, y3),
                    var (_, least) => (x3 - least, x3, (y3 - style.Bottom - (slope * x3) + ((x3 - (least * 0.5f)) * slope)) / (x3 - (least * 0.5f) - style.Bottom), y3),
                },
            },
        };

    private sealed record Spline(Seq<Knot> Knots) {
        public static Spline Of(Knot first, Seq<(float X, float M)> rest) =>
            new(rest.Scan(first, static (knot, next) => new Knot(next.X, knot.Y + ((knot.M + next.M) * (next.X - knot.X) * 0.5f), next.M)));

        public static Spline Bend(float low, float high, float lowSlope, float highSlope) =>
            Of(new(low, low, lowSlope), [(low + ((high - low) * 0.5f), 2f - ((lowSlope + highSlope) * 0.5f)), (high, highSlope)]);

        public float Forward(float t) =>
            Segment(t, static knot => knot.X) switch {
                -1 => Knots[0].Y + ((t - Knots[0].X) * Knots[0].M),
                var at when at == Knots.Count - 1 => Knots[at].Y + ((t - Knots[at].X) * Knots[at].M),
                var at => (Knots[at], Knots[at + 1], t - Knots[at].X) switch {
                    var (low, high, d) => low.Y + (d * (low.M + (0.5f * (high.M - low.M) * d / (high.X - low.X)))),
                },
            };

        public float Inverse(float t) =>
            Segment(t, static knot => knot.Y) switch {
                -1 => Knots[0].X + ((t - Knots[0].Y) / Knots[0].M),
                var at when at == Knots.Count - 1 => Knots[at].X + ((t - Knots[at].Y) / Knots[at].M),
                var at => (Knots[at], Knots[at + 1].X - Knots[at].X, Knots[at + 1].M - Knots[at].M, Knots[at].Y - t) switch {
                    var (low, h, rise, c) => low.X + (-2f * c / (float.Sqrt((low.M * h * low.M * h) - (2f * rise * h * c)) + (low.M * h)) * h),
                },
            };

        public float Gained(float t, float anchor, float gain) => ((Inverse(((t - anchor) * gain) + anchor) - anchor) / gain) + anchor;

        private int Segment(float t, Func<Knot, float> axis) =>
            Knots.AsSpan().BinarySearch(new Comparable(t, axis)) switch {
                >= 0 and var at => at,
                var after => ~after - 1,
            };
    }

    private readonly record struct Knot(float X, float Y, float M);

    private readonly record struct Comparable(float Value, Func<Knot, float> Axis) : IComparable<Knot> {
        public int CompareTo(Knot other) => Value.CompareTo(Axis(other));
    }
}

[SmartEnum]
internal sealed partial class GradingStyle {
    public static readonly GradingStyle Log = new(
        1f, 1f, 0f, 0.4f,
        new GradingTone(ZoneSpan.Placed(0.4f, 0.4f), ZoneSpan.Placed(0.5f, 0.5f), ZoneSpan.Placed(0.4f, 0.6f), ZoneSpan.Placed(0.3f, 0.7f), ZoneSpan.Placed(0.4f, 0.5f), 1f),
        static value => value,
        static value => value);
    public static readonly GradingStyle Linear = new(
        7.5f, 6.5f, -5.5f, 0f,
        new GradingTone(ZoneSpan.Placed(0f, 4f), ZoneSpan.Placed(2f, 9f), ZoneSpan.Placed(0f, 8f), ZoneSpan.Placed(-2f, 11f), ZoneSpan.Placed(0f, 8f), 1f),
        static value => (float)CurveAxis.ToStops(value),
        static stops => (float)CurveAxis.FromStops(stops));

    public float Top { get; }
    public float ContrastTop { get; }
    public float Bottom { get; }
    public float Pivot { get; }
    public GradingTone Neutral { get; }

    [UseDelegateFromConstructor]
    public partial float Encode(float value);

    [UseDelegateFromConstructor]
    public partial float Decode(float value);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ZoneAdjustment : IPresented<ZoneAdjustment, float> {
    public static ZoneAdjustment MinValue { get; } = new(0.1f);
    public static ZoneAdjustment MaxValue { get; } = new(1.9f);
    public static ZoneAdjustment Neutral { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral, Step = 0.01f, Decimals = 3 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PivotAdjustment : IPresented<PivotAdjustment, float> {
    public static PivotAdjustment MinValue { get; } = new(0.2f);
    public static PivotAdjustment MaxValue { get; } = new(1.8f);
    public static PivotAdjustment Neutral { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral, Step = 0.01f, Decimals = 3 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ZoneWidth : IPresented<ZoneWidth, float> {
    public static ZoneWidth MinValue { get; } = new(0.01f);
    public static ZoneWidth MaxValue { get; } = new((float)Exposure.MaxValue - (float)Exposure.MinValue);
    public static Presentation<float> Presentation { get; } = GradingStyle.Linear.Neutral switch {
        var neutral => new() {
            Soft = ((float)MinValue, neutral.Highlights.Start + neutral.Highlights.Width - (neutral.Shadows.Start - neutral.Shadows.Width)),
            Step = 0.1f,
            Decimals = 2,
        },
    };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ZoneContrast : IPresented<ZoneContrast, float> {
    public static ZoneContrast MinValue { get; } = new(0.01f);
    public static ZoneContrast MaxValue { get; } = new(1.99f);
    public static ZoneContrast Neutral { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral, Step = 0.01f, Decimals = 3 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record Zone<TAdjustment>(TAdjustment Master, TAdjustment Red, TAdjustment Green, TAdjustment Blue, Exposure Start, ZoneWidth Width)
    where TAdjustment : struct, IObjectFactory<TAdjustment, float, ValidationError>, IConvertible<float>, IMinMaxValue<TAdjustment> {
    internal ZoneSpan Span => new(new Vector4(Red.ToValue(), Green.ToValue(), Blue.ToValue(), Master.ToValue()), Start, Width);

    internal static Zone<TAdjustment> Of(ZoneSpan span) =>
        new(Adjusted(span.Values.W), Adjusted(span.Values.X), Adjusted(span.Values.Y), Adjusted(span.Values.Z),
            Exposure.Create(span.Start),
            ZoneWidth.Create(span.Width));

    private static TAdjustment Adjusted(float value) => ValidationResults.Validated<TAdjustment, float>(value).ThrowIfFail();
}

public sealed record ToneZones(
    Zone<ZoneAdjustment> Blacks, Zone<PivotAdjustment> Shadows, Zone<ZoneAdjustment> Midtones, Zone<PivotAdjustment> Highlights, Zone<ZoneAdjustment> Whites, ZoneContrast SContrast)
    : IStateRecord<ToneZones, ToneZonesParameter>, IPixelStage<ToneZones> {
    public static ToneZones Default { get; } = GradingStyle.Linear.Neutral switch {
        var neutral => new(
            Zone<ZoneAdjustment>.Of(neutral.Blacks), Zone<PivotAdjustment>.Of(neutral.Shadows), Zone<ZoneAdjustment>.Of(neutral.Midtones),
            Zone<PivotAdjustment>.Of(neutral.Highlights), Zone<ZoneAdjustment>.Of(neutral.Whites), ZoneContrast.Neutral),
    };

    public static Option<PixelPass> Pass(ToneZones state, PassContext context) =>
        new GradingTone(state.Blacks.Span, state.Shadows.Span, state.Midtones.Span, state.Highlights.Span, state.Whites.Span, state.SContrast)
            .Kernel(GradingStyle.Linear)
            .Map(static kernel => (PixelPass)new PixelPass.Color(kernel));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToneZonesParameter : IStateParameter<ToneZones> {
    private static readonly ZoneKinds Blacks = Kinds(
        Lens<ToneZones, Zone<ZoneAdjustment>>.New(static zones => zones.Blacks, static zone => zones => zones with { Blacks = zone }));
    private static readonly ZoneKinds Shadows = Kinds(
        Lens<ToneZones, Zone<PivotAdjustment>>.New(static zones => zones.Shadows, static zone => zones => zones with { Shadows = zone }));
    private static readonly ZoneKinds Midtones = Kinds(
        Lens<ToneZones, Zone<ZoneAdjustment>>.New(static zones => zones.Midtones, static zone => zones => zones with { Midtones = zone }));
    private static readonly ZoneKinds Highlights = Kinds(
        Lens<ToneZones, Zone<PivotAdjustment>>.New(static zones => zones.Highlights, static zone => zones => zones with { Highlights = zone }));
    private static readonly ZoneKinds Whites = Kinds(
        Lens<ToneZones, Zone<ZoneAdjustment>>.New(static zones => zones.Whites, static zone => zones => zones with { Whites = zone }));

    public static readonly ToneZonesParameter BlacksMaster = new("blacks-master", Blacks.Master,
        new("Blacks", "Adjustment of the blacks in every channel, one leaving them unchanged"));
    public static readonly ToneZonesParameter BlacksRed = new("blacks-red", Blacks.Red,
        new("Blacks red", "Adjustment of the blacks in red"));
    public static readonly ToneZonesParameter BlacksGreen = new("blacks-green", Blacks.Green,
        new("Blacks green", "Adjustment of the blacks in green"));
    public static readonly ToneZonesParameter BlacksBlue = new("blacks-blue", Blacks.Blue,
        new("Blacks blue", "Adjustment of the blacks in blue"));
    public static readonly ToneZonesParameter BlacksStart = new("blacks-start", Blacks.Start,
        new("Blacks start", "Stops from middle gray where the blacks begin, reaching downward"));
    public static readonly ToneZonesParameter BlacksWidth = new("blacks-width", Blacks.Width,
        new("Blacks width", "Stops the blacks span"));
    public static readonly ToneZonesParameter ShadowsMaster = new("shadows-master", Shadows.Master,
        new("Shadows", "Adjustment of the shadows in every channel, one leaving them unchanged"));
    public static readonly ToneZonesParameter ShadowsRed = new("shadows-red", Shadows.Red,
        new("Shadows red", "Adjustment of the shadows in red"));
    public static readonly ToneZonesParameter ShadowsGreen = new("shadows-green", Shadows.Green,
        new("Shadows green", "Adjustment of the shadows in green"));
    public static readonly ToneZonesParameter ShadowsBlue = new("shadows-blue", Shadows.Blue,
        new("Shadows blue", "Adjustment of the shadows in blue"));
    public static readonly ToneZonesParameter ShadowsStart = new("shadows-start", Shadows.Start,
        new("Shadows start", "Stops from middle gray where the shadows begin, reaching downward"));
    public static readonly ToneZonesParameter ShadowsWidth = new("shadows-width", Shadows.Width,
        new("Shadows width", "Stops the shadows span"));
    public static readonly ToneZonesParameter MidtonesMaster = new("midtones-master", Midtones.Master,
        new("Midtones", "Adjustment of the midtones in every channel, one leaving them unchanged"));
    public static readonly ToneZonesParameter MidtonesRed = new("midtones-red", Midtones.Red,
        new("Midtones red", "Adjustment of the midtones in red"));
    public static readonly ToneZonesParameter MidtonesGreen = new("midtones-green", Midtones.Green,
        new("Midtones green", "Adjustment of the midtones in green"));
    public static readonly ToneZonesParameter MidtonesBlue = new("midtones-blue", Midtones.Blue,
        new("Midtones blue", "Adjustment of the midtones in blue"));
    public static readonly ToneZonesParameter MidtonesStart = new("midtones-start", Midtones.Start,
        new("Midtones center", "Stops from middle gray the midtones center on"));
    public static readonly ToneZonesParameter MidtonesWidth = new("midtones-width", Midtones.Width,
        new("Midtones width", "Stops the midtones span"));
    public static readonly ToneZonesParameter HighlightsMaster = new("highlights-master", Highlights.Master,
        new("Highlights", "Adjustment of the highlights in every channel, one leaving them unchanged"));
    public static readonly ToneZonesParameter HighlightsRed = new("highlights-red", Highlights.Red,
        new("Highlights red", "Adjustment of the highlights in red"));
    public static readonly ToneZonesParameter HighlightsGreen = new("highlights-green", Highlights.Green,
        new("Highlights green", "Adjustment of the highlights in green"));
    public static readonly ToneZonesParameter HighlightsBlue = new("highlights-blue", Highlights.Blue,
        new("Highlights blue", "Adjustment of the highlights in blue"));
    public static readonly ToneZonesParameter HighlightsStart = new("highlights-start", Highlights.Start,
        new("Highlights start", "Stops from middle gray where the highlights begin, reaching upward"));
    public static readonly ToneZonesParameter HighlightsWidth = new("highlights-width", Highlights.Width,
        new("Highlights width", "Stops the highlights span"));
    public static readonly ToneZonesParameter WhitesMaster = new("whites-master", Whites.Master,
        new("Whites", "Adjustment of the whites in every channel, one leaving them unchanged"));
    public static readonly ToneZonesParameter WhitesRed = new("whites-red", Whites.Red,
        new("Whites red", "Adjustment of the whites in red"));
    public static readonly ToneZonesParameter WhitesGreen = new("whites-green", Whites.Green,
        new("Whites green", "Adjustment of the whites in green"));
    public static readonly ToneZonesParameter WhitesBlue = new("whites-blue", Whites.Blue,
        new("Whites blue", "Adjustment of the whites in blue"));
    public static readonly ToneZonesParameter WhitesStart = new("whites-start", Whites.Start,
        new("Whites start", "Stops from middle gray where the whites begin, reaching upward"));
    public static readonly ToneZonesParameter WhitesWidth = new("whites-width", Whites.Width,
        new("Whites width", "Stops the whites span"));
    public static readonly ToneZonesParameter SContrast = new("s-contrast", StateParameter.Bounded(Lens<ToneZones, ZoneContrast>.New(static zones => zones.SContrast, static contrast => zones => zones with { SContrast = contrast })),
        new("Contrast", "Slope of the S-curve through the tones, one leaving the image unchanged"));

    public StateParameter<ToneZones> Kind { get; }
    public ParameterText Text { get; }

    private static ZoneKinds Kinds<TAdjustment>(Lens<ToneZones, Zone<TAdjustment>> zone)
        where TAdjustment : struct, IPresented<TAdjustment, float> =>
        fun((Lens<Zone<TAdjustment>, TAdjustment> channel) =>
            StateParameter.Bounded(lens(zone, channel))) switch {
                var adjusted => new(
                    adjusted(Lens<Zone<TAdjustment>, TAdjustment>.New(static at => at.Master, static value => at => at with { Master = value })),
                    adjusted(Lens<Zone<TAdjustment>, TAdjustment>.New(static at => at.Red, static value => at => at with { Red = value })),
                    adjusted(Lens<Zone<TAdjustment>, TAdjustment>.New(static at => at.Green, static value => at => at with { Green = value })),
                    adjusted(Lens<Zone<TAdjustment>, TAdjustment>.New(static at => at.Blue, static value => at => at with { Blue = value })),
                    StateParameter.Bounded(lens(zone, Lens<Zone<TAdjustment>, Exposure>.New(static at => at.Start, static start => at => at with { Start = start }))),
                    StateParameter.Bounded(lens(zone, Lens<Zone<TAdjustment>, ZoneWidth>.New(static at => at.Width, static width => at => at with { Width = width })))),
            };

    private readonly record struct ZoneKinds(
        StateParameter<ToneZones> Master, StateParameter<ToneZones> Red, StateParameter<ToneZones> Green, StateParameter<ToneZones> Blue,
        StateParameter<ToneZones> Start, StateParameter<ToneZones> Width);
}
