using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using TinyEXR;
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

public readonly record struct WheelOffset(WheelHue Hue, WheelStrength Strength, WheelLuma Luma)
    : IStateRecord<WheelOffset, WheelOffsetParameter, InvalidGrade>, IPixelStage<WheelOffset> {
    public static WheelOffset Default { get; } = new(WheelHue.Origin, WheelStrength.Neutral, WheelLuma.Neutral);

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

    public static Option<PixelPass> Pass(WheelOffset state, PassContext context) =>
        state == Default
            ? None
            : state.Multiplier(context.Working) switch {
                var gain => Some<PixelPass>(new PixelPass.Color(new LutTable.Affine(new ColorMatrix3x3(gain.X, 0f, 0f, 0f, gain.Y, 0f, 0f, 0f, gain.Z), Vector3.Zero))),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WheelOffsetParameter : IStateParameter<WheelOffset> {
    public static readonly WheelOffsetParameter Hue = new("hue", new StateParameter<WheelOffset>.Bounded<WheelHue, float, InvalidGrade>(
        Lens<WheelOffset, WheelHue>.New(static offset => offset.Hue, static hue => offset => offset with { Hue = hue }),
        new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) }));
    public static readonly WheelOffsetParameter Strength = new("strength", new StateParameter<WheelOffset>.Bounded<WheelStrength, float, InvalidGrade>(
        Lens<WheelOffset, WheelStrength>.New(static offset => offset.Strength, static strength => offset => offset with { Strength = strength }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly WheelOffsetParameter Luma = new("luma", new StateParameter<WheelOffset>.Bounded<WheelLuma, float, InvalidGrade>(
        Lens<WheelOffset, WheelLuma>.New(static offset => offset.Luma, static luma => offset => offset with { Luma = luma }),
        new() { Origin = (float)WheelLuma.Neutral }));

    public StateParameter<WheelOffset> Kind { get; }
}
