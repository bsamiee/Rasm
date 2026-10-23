using System.Globalization;
using System.Numerics;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using UnitsNet;
using UnitsNet.Units;
using Wacton.Unicolour;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Origin", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct WheelHue : IPresented<WheelHue, float> {
    public static WheelHue MinValue { get; } = new(0f);
    public static WheelHue MaxValue { get; } = new(float.BitDecrement(float.Tau));
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct WheelLuma : IPresented<WheelLuma, float> {
    public static WheelLuma MinValue { get; } = new(-3f);
    public static WheelLuma MaxValue { get; } = new(3f);
    public static Presentation<float> Presentation { get; } = new() { Origin = (float)Neutral };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public readonly record struct WheelOffset(WheelHue Hue, AxisFraction Strength, WheelLuma Luma) : IStateRecord<WheelOffset, WheelOffsetParameter> {
    public static WheelOffset Default { get; } = new(WheelHue.Origin, AxisFraction.MinValue, WheelLuma.Neutral);

    public static Lens<WheelOffset, WheelHue> HueOf { get; } = Lens<WheelOffset, WheelHue>.New(static offset => offset.Hue, static hue => offset => offset with { Hue = hue });
    public static Lens<WheelOffset, AxisFraction> StrengthOf { get; } = Lens<WheelOffset, AxisFraction>.New(static offset => offset.Strength, static strength => offset => offset with { Strength = strength });
    public static Lens<WheelOffset, WheelLuma> LumaOf { get; } = Lens<WheelOffset, WheelLuma>.New(static offset => offset.Luma, static luma => offset => offset with { Luma = luma });

    public static Vector3 Tint(Gamut gamut, Vector2 chroma) =>
        new Unicolour(Configuration.Default, ColourSpace.Oklab, 1d, chroma.X, chroma.Y).ConvertToConfiguration(gamut.Configuration).RgbLinear switch {
            var rgb => new Vector3((float)rgb.R, (float)rgb.G, (float)rgb.B),
        };

    public Vector3 Multiplier(Gamut gamut) => Tint(gamut, 0.5f * Strength * new Vector2(float.Cos(Hue), float.Sin(Hue))) * float.Exp2(Luma);

    public static Fin<WheelOffset> FromMultiplier(Vector3 multiplier, Gamut gamut, WheelHue hue) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, multiplier.X, multiplier.Y, multiplier.Z).ConvertToConfiguration(Configuration.Default).Oklab switch {
            { L: <= 0d } => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"wheel multiplier {multiplier} must have positive lightness")),
            var lab => new Vector2((float)(lab.A / lab.L), (float)(lab.B / lab.L)) switch {
                var chroma => (chroma.Length() < 1e-6f ? hue : WheelHue.Validated((float.Atan2(chroma.Y, chroma.X) + float.Tau) % float.Tau),
                               AxisFraction.Validated(2f * chroma.Length()),
                               WheelLuma.Validated(3f * float.Log2((float)lab.L)))
                    .Apply(static (h, s, l) => new WheelOffset(h, s, l)).As(),
            },
        };

    public static (Vector3 Low, Vector3 High) ChannelBounds(Gamut gamut) =>
        toSeq(Range(0, 360))
            .Map(static step => float.DegreesToRadians(step))
            .Map(angle => Tint(gamut, 0.5f * new Vector2(float.Cos(angle), float.Sin(angle))))
            .Fold((Low: new Vector3(float.MaxValue), High: new Vector3(float.MinValue)), static (bounds, tint) => (Vector3.Min(bounds.Low, tint), Vector3.Max(bounds.High, tint)));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WheelOffsetParameter : IStateParameter<WheelOffset> {
    public static readonly WheelOffsetParameter Hue = new("hue", StateParameter.Bounded(WheelOffset.HueOf),
        new("Hue", "Tint angle counterclockwise from right"));
    public static readonly WheelOffsetParameter Strength = new("strength", StateParameter.Bounded(WheelOffset.StrengthOf),
        new("Strength", "Tint chroma"));
    public static readonly WheelOffsetParameter Luma = new("luma", StateParameter.Bounded(WheelOffset.LumaOf),
        new("Luma", "Gain in stops"));

    public StateParameter<WheelOffset> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum]
public sealed partial class ToneWheel {
    public static readonly ToneWheel Global = new(Lens<WheelsState, WheelOffset>.New(static state => state.Global, static offset => state => state with { Global = offset }));
    public static readonly ToneWheel Shadows = new(Lens<WheelsState, WheelOffset>.New(static state => state.Shadows, static offset => state => state with { Shadows = offset }));
    public static readonly ToneWheel Midtones = new(Lens<WheelsState, WheelOffset>.New(static state => state.Midtones, static offset => state => state with { Midtones = offset }));
    public static readonly ToneWheel Highlights = new(Lens<WheelsState, WheelOffset>.New(static state => state.Highlights, static offset => state => state with { Highlights = offset }));

    public Lens<WheelsState, WheelOffset> Lens { get; }

    public Lens<WheelsState, WheelHue> Hue => lens(Lens, WheelOffset.HueOf);

    public Lens<WheelsState, AxisFraction> Strength => lens(Lens, WheelOffset.StrengthOf);

    public Lens<WheelsState, WheelLuma> Luma => lens(Lens, WheelOffset.LumaOf);

    public WheelsParameter Parameter =>
        Map(@global: WheelsParameter.Global, shadows: WheelsParameter.Shadows, midtones: WheelsParameter.Midtones, highlights: WheelsParameter.Highlights);
}

public sealed record WheelsState(
    WheelOffset Global, WheelOffset Shadows, WheelOffset Midtones, WheelOffset Highlights,
    PositiveFraction ShadowsEnd, AxisFraction HighlightsStart, PositiveFraction HighlightsWidth)
    : IStateRecord<WheelsState, WheelsParameter>, IPixelStage<WheelsState> {
    public static WheelsState Default { get; } = new(
        WheelOffset.Default, WheelOffset.Default, WheelOffset.Default, WheelOffset.Default,
        PositiveFraction.Create(0.333f), AxisFraction.Create(0.550f), PositiveFraction.Create(0.450f));

    public static Option<PixelPass> Pass(WheelsState state, PassContext context) {
        static double Smooth(double low, double high, double y) =>
            double.Clamp((y - low) / (high - low), 0d, 1d) switch {
                var t => t * t * (3d - (2d * t)),
            };

        return from changed in Some(state)
               where toSeq(ToneWheel.Items).Exists(wheel => wheel.Lens.Get(changed) != WheelOffset.Default)
               let wheels = (Weights: context.Working.Luminance, Global: changed.Global.Multiplier(context.Working), Shadows: changed.Shadows.Multiplier(context.Working),
                   Midtones: changed.Midtones.Multiplier(context.Working), Highlights: changed.Highlights.Multiplier(context.Working))
               select (PixelPass)new PixelPass.Color(row => {
                   foreach (ref Vector4 pixel in row) {
                       double luma = LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, wheels.Weights);
                       (double a, double b) = (Smooth(0d, (float)changed.ShadowsEnd, luma), Smooth((float)changed.HighlightsStart, (float)changed.HighlightsStart + (float)changed.HighlightsWidth, luma));
                       (double low, double middle, double high) = (1d - a, a * (1d - b), a * b);
                       pixel = new Vector4(
                           (float)(pixel.X * (double)wheels.Global.X * ((wheels.Shadows.X * low) + (wheels.Midtones.X * middle) + (wheels.Highlights.X * high))),
                           (float)(pixel.Y * (double)wheels.Global.Y * ((wheels.Shadows.Y * low) + (wheels.Midtones.Y * middle) + (wheels.Highlights.Y * high))),
                           (float)(pixel.Z * (double)wheels.Global.Z * ((wheels.Shadows.Z * low) + (wheels.Midtones.Z * middle) + (wheels.Highlights.Z * high))), pixel.W);
                   }
               });
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WheelsParameter : IStateParameter<WheelsState> {
    public static readonly WheelsParameter Global = new("global", new StateParameter<WheelsState>.Record<WheelOffset>(ToneWheel.Global.Lens),
        new("Global", "Tint over every tone"));
    public static readonly WheelsParameter Shadows = new("shadows", new StateParameter<WheelsState>.Record<WheelOffset>(ToneWheel.Shadows.Lens),
        new("Shadows", "Tint over dark tones"));
    public static readonly WheelsParameter Midtones = new("midtones", new StateParameter<WheelsState>.Record<WheelOffset>(ToneWheel.Midtones.Lens),
        new("Midtones", "Tint over middle tones"));
    public static readonly WheelsParameter Highlights = new("highlights", new StateParameter<WheelsState>.Record<WheelOffset>(ToneWheel.Highlights.Lens),
        new("Highlights", "Tint over bright tones"));
    public static readonly WheelsParameter ShadowsEnd = new("shadows-end", StateParameter.Bounded(Lens<WheelsState, PositiveFraction>.New(static state => state.ShadowsEnd, static end => state => state with { ShadowsEnd = end })),
        new("Shadows end", "Working luminance ending shadows weight"));
    public static readonly WheelsParameter HighlightsStart = new("highlights-start", StateParameter.Bounded(Lens<WheelsState, AxisFraction>.New(static state => state.HighlightsStart, static start => state => state with { HighlightsStart = start })),
        new("Highlights start", "Working luminance starting highlights weight"));
    public static readonly WheelsParameter HighlightsWidth = new("highlights-width", StateParameter.Bounded(Lens<WheelsState, PositiveFraction>.New(static state => state.HighlightsWidth, static width => state => state with { HighlightsWidth = width })),
        new("Highlights width", "Luminance span over which highlights weight reaches full"));

    public StateParameter<WheelsState> Kind { get; }
    public ParameterText Text { get; }
}
