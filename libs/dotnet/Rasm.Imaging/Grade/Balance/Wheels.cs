using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;
using Wacton.Unicolour;

namespace Rasm.Imaging.Grade.Balance;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Origin", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct WheelHue : IMinMaxValue<WheelHue> {
    public static WheelHue MinValue { get; } = new(0f);
    public static WheelHue MaxValue { get; } = new(float.BitDecrement(float.Tau));

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct WheelStrength : IMinMaxValue<WheelStrength> {
    public static WheelStrength MinValue { get; } = new(0f);
    public static WheelStrength MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct WheelLuma : IMinMaxValue<WheelLuma> {
    public static WheelLuma MinValue { get; } = new(-3f);
    public static WheelLuma MaxValue { get; } = new(3f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

public readonly record struct WheelOffset(WheelHue Hue, WheelStrength Strength, WheelLuma Luma) {
    public static WheelOffset Neutral { get; } = new(WheelHue.Origin, WheelStrength.Neutral, WheelLuma.Neutral);

    public static Vector3 Tint(Gamut gamut, Vector2 chroma) =>
        new Unicolour(Configuration.Default, ColourSpace.Oklab, 1d, chroma.X, chroma.Y).ConvertToConfiguration(gamut.Configuration).RgbLinear switch {
            var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B),
        };

    public Vector3 Multiplier(Gamut gamut) => Tint(gamut, 0.5f * Strength * new Vector2(float.Cos(Hue), float.Sin(Hue))) * float.Exp2(Luma);

    public static Fin<WheelOffset> FromMultiplier(Vector3 multiplier, Gamut gamut, WheelHue hue) {
        static Validation<Error, T> Crossed<T>(float key) where T : IObjectFactory<T, float, InvalidGrade> =>
            T.Validate(key, provider: null, out T? item) is { } refused ? refused : item!;

        return new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, multiplier.X, multiplier.Y, multiplier.Z).ConvertToConfiguration(Configuration.Default).Oklab switch {
            { L: <= 0d } => new InvalidGrade(),
            var lab => new Vector2((float)(lab.A / lab.L), (float)(lab.B / lab.L)) switch {
                var chroma => (chroma.Length() < 1e-6f ? Success<Error, WheelHue>(hue) : Crossed<WheelHue>((float.Atan2(chroma.Y, chroma.X) + float.Tau) % float.Tau),
                               Crossed<WheelStrength>(2f * chroma.Length()),
                               Crossed<WheelLuma>(3f * float.Log2((float)lab.L)))
                    .Apply(static (h, s, l) => new WheelOffset(h, s, l)).As().ToFin(),
            },
        };
    }

    public static (Vector3 Low, Vector3 High) LaneBounds(Gamut gamut) =>
        toSeq(Range(0, 360))
            .Map(static step => float.DegreesToRadians(step))
            .Map(angle => Tint(gamut, 0.5f * new Vector2(float.Cos(angle), float.Sin(angle))))
            .Fold((Low: new Vector3(float.MaxValue), High: new Vector3(float.MinValue)), static (bounds, tint) => (Vector3.Min(bounds.Low, tint), Vector3.Max(bounds.High, tint)));
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct ToneSpan : IMinMaxValue<ToneSpan> {
    public static ToneSpan MinValue { get; } = new(float.BitIncrement(0f));
    public static ToneSpan MaxValue { get; } = new(1f);
    public static ToneSpan Shadows { get; } = new(0.333f);
    public static ToneSpan Highlights { get; } = new(0.450f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct ToneEdge : IMinMaxValue<ToneEdge> {
    public static ToneEdge MinValue { get; } = new(0f);
    public static ToneEdge MaxValue { get; } = new(1f);
    public static ToneEdge Highlights { get; } = new(0.550f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[SmartEnum]
public sealed partial class ToneWheel {
    private static readonly Lens<WheelOffset, WheelHue> Hues =
        Lens<WheelOffset, WheelHue>.New(static offset => offset.Hue, static hue => offset => offset with { Hue = hue });
    private static readonly Lens<WheelOffset, WheelStrength> Strengths =
        Lens<WheelOffset, WheelStrength>.New(static offset => offset.Strength, static strength => offset => offset with { Strength = strength });
    private static readonly Lens<WheelOffset, WheelLuma> Lumas =
        Lens<WheelOffset, WheelLuma>.New(static offset => offset.Luma, static luma => offset => offset with { Luma = luma });

    public static readonly ToneWheel Global = new(Lens<WheelsState, WheelOffset>.New(static state => state.Global, static offset => state => state with { Global = offset }));
    public static readonly ToneWheel Shadows = new(Lens<WheelsState, WheelOffset>.New(static state => state.Shadows, static offset => state => state with { Shadows = offset }));
    public static readonly ToneWheel Midtones = new(Lens<WheelsState, WheelOffset>.New(static state => state.Midtones, static offset => state => state with { Midtones = offset }));
    public static readonly ToneWheel Highlights = new(Lens<WheelsState, WheelOffset>.New(static state => state.Highlights, static offset => state => state with { Highlights = offset }));

    public Lens<WheelsState, WheelOffset> Lens { get; }

    public Lens<WheelsState, WheelHue> Hue => lens(Lens, Hues);

    public Lens<WheelsState, WheelStrength> Strength => lens(Lens, Strengths);

    public Lens<WheelsState, WheelLuma> Luma => lens(Lens, Lumas);

    public (WheelsParameter Hue, WheelsParameter Strength, WheelsParameter Luma) Parameters =>
        Map(
            @global: (WheelsParameter.GlobalHue, WheelsParameter.GlobalStrength, WheelsParameter.GlobalLuma),
            shadows: (WheelsParameter.ShadowsHue, WheelsParameter.ShadowsStrength, WheelsParameter.ShadowsLuma),
            midtones: (WheelsParameter.MidtonesHue, WheelsParameter.MidtonesStrength, WheelsParameter.MidtonesLuma),
            highlights: (WheelsParameter.HighlightsHue, WheelsParameter.HighlightsStrength, WheelsParameter.HighlightsLuma));
}

public sealed record WheelsState(
    WheelOffset Global, WheelOffset Shadows, WheelOffset Midtones, WheelOffset Highlights,
    ToneSpan ShadowsEnd, ToneEdge HighlightsStart, ToneSpan HighlightsWidth)
    : IStateRecord<WheelsState, WheelsParameter, InvalidGrade>, IPixelStage<WheelsState> {
    public static WheelsState Default { get; } = new(
        WheelOffset.Neutral, WheelOffset.Neutral, WheelOffset.Neutral, WheelOffset.Neutral,
        ToneSpan.Shadows, ToneEdge.Highlights, ToneSpan.Highlights);

    public static Option<PixelPass> Pass(WheelsState state, PassContext context) {
        static float Smooth(float low, float high, float y) =>
            float.Clamp((y - low) / (high - low), 0f, 1f) switch {
                var t => t * t * (3f - (2f * t)),
            };

        return toSeq(ToneWheel.Items).ForAll(wheel => wheel.Lens.Get(state) == WheelOffset.Neutral)
            ? None
            : (context.Working.Luminance, state.Global.Multiplier(context.Working), state.Shadows.Multiplier(context.Working), state.Midtones.Multiplier(context.Working),
               state.Highlights.Multiplier(context.Working), (float)state.ShadowsEnd, (float)state.HighlightsStart, (float)state.HighlightsStart + (float)state.HighlightsWidth) switch {
                   var (weights, global, shadows, midtones, highlights, shadowsEnd, start, end) => Some<PixelPass>(new PixelPass.Color(row => {
                       foreach (ref Vector4 pixel in row) {
                           float luma = Vector3.Dot(weights, pixel.AsVector3());
                           (float a, float b) = (Smooth(0f, shadowsEnd, luma), Smooth(start, end, luma));
                           pixel = new Vector4(pixel.AsVector3() * global * ((shadows * (1f - a)) + (midtones * (a * (1f - b))) + (highlights * (a * b))), pixel.W);
                       }
                   })),
               };
    }
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WheelsParameter : IStateParameter<WheelsState> {
    public static Presentation<WheelHue, float> Hue { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };
    public static Presentation<WheelStrength, float> Strength { get; } = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    public static Presentation<WheelLuma, float> Luma { get; } = new() { Origin = (float)WheelLuma.Neutral };

    public static readonly WheelsParameter GlobalHue = new("global-hue", new StateParameter<WheelsState>.Bounded<WheelHue, float, InvalidGrade>(ToneWheel.Global.Hue, Hue));
    public static readonly WheelsParameter GlobalStrength = new("global-strength", new StateParameter<WheelsState>.Bounded<WheelStrength, float, InvalidGrade>(ToneWheel.Global.Strength, Strength));
    public static readonly WheelsParameter GlobalLuma = new("global-luma", new StateParameter<WheelsState>.Bounded<WheelLuma, float, InvalidGrade>(ToneWheel.Global.Luma, Luma));
    public static readonly WheelsParameter ShadowsHue = new("shadows-hue", new StateParameter<WheelsState>.Bounded<WheelHue, float, InvalidGrade>(ToneWheel.Shadows.Hue, Hue));
    public static readonly WheelsParameter ShadowsStrength = new("shadows-strength", new StateParameter<WheelsState>.Bounded<WheelStrength, float, InvalidGrade>(ToneWheel.Shadows.Strength, Strength));
    public static readonly WheelsParameter ShadowsLuma = new("shadows-luma", new StateParameter<WheelsState>.Bounded<WheelLuma, float, InvalidGrade>(ToneWheel.Shadows.Luma, Luma));
    public static readonly WheelsParameter MidtonesHue = new("midtones-hue", new StateParameter<WheelsState>.Bounded<WheelHue, float, InvalidGrade>(ToneWheel.Midtones.Hue, Hue));
    public static readonly WheelsParameter MidtonesStrength = new("midtones-strength", new StateParameter<WheelsState>.Bounded<WheelStrength, float, InvalidGrade>(ToneWheel.Midtones.Strength, Strength));
    public static readonly WheelsParameter MidtonesLuma = new("midtones-luma", new StateParameter<WheelsState>.Bounded<WheelLuma, float, InvalidGrade>(ToneWheel.Midtones.Luma, Luma));
    public static readonly WheelsParameter HighlightsHue = new("highlights-hue", new StateParameter<WheelsState>.Bounded<WheelHue, float, InvalidGrade>(ToneWheel.Highlights.Hue, Hue));
    public static readonly WheelsParameter HighlightsStrength = new("highlights-strength", new StateParameter<WheelsState>.Bounded<WheelStrength, float, InvalidGrade>(ToneWheel.Highlights.Strength, Strength));
    public static readonly WheelsParameter HighlightsLuma = new("highlights-luma", new StateParameter<WheelsState>.Bounded<WheelLuma, float, InvalidGrade>(ToneWheel.Highlights.Luma, Luma));
    public static readonly WheelsParameter ShadowsEnd = new("shadows-end", new StateParameter<WheelsState>.Bounded<ToneSpan, float, InvalidGrade>(
        Lens<WheelsState, ToneSpan>.New(static state => state.ShadowsEnd, static end => state => state with { ShadowsEnd = end }), new()));
    public static readonly WheelsParameter HighlightsStart = new("highlights-start", new StateParameter<WheelsState>.Bounded<ToneEdge, float, InvalidGrade>(
        Lens<WheelsState, ToneEdge>.New(static state => state.HighlightsStart, static start => state => state with { HighlightsStart = start }), new()));
    public static readonly WheelsParameter HighlightsWidth = new("highlights-width", new StateParameter<WheelsState>.Bounded<ToneSpan, float, InvalidGrade>(
        Lens<WheelsState, ToneSpan>.New(static state => state.HighlightsWidth, static width => state => state with { HighlightsWidth = width }), new()));

    public StateParameter<WheelsState> Kind { get; }
}
