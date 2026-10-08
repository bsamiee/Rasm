using System.Numerics;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using TinyEXR;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct Exposure : IMinMaxValue<Exposure> {
    public static Exposure MinValue { get; } = new(-32f);
    public static Exposure MaxValue { get; } = new(32f);
    public static Presentation<Exposure, float> Presentation { get; } = new() { Soft = (-10f, 10f), Origin = 0f, Step = 0.01f, Decimals = 3 };
    public static float MiddleGrey => 0.18f;

    public float Scale => float.Exp2(_value);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

public sealed record ExposureState(Exposure Exposure, Metering Metering)
    : IStateRecord<ExposureState, ExposureParameter, InvalidToneValue>, IPixelStage<ExposureState> {
    public static ExposureState Default { get; } = new(Exposure.Neutral, Metering.Default);

    public static Option<PixelPass> Pass(ExposureState state, PassContext context) =>
        state.Exposure == Exposure.Neutral
            ? None
            : state.Exposure.Scale switch {
                var gain => Some<PixelPass>(new PixelPass.Color(new LutTable.Affine(new ColorMatrix3x3(gain, 0f, 0f, 0f, gain, 0f, 0f, 0f, gain), Vector3.Zero))),
            };

    public Fin<ExposureState> Metered(PixelFrame frame, Gamut working) =>
        Metering.Metered(frame, working.Luminance).Bind(stops =>
            Exposure.Validate(stops, provider: null, out Exposure metered) is { } error ? error : (Fin<ExposureState>)(this with { Exposure = metered }));
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExposureParameter : IStateParameter<ExposureState> {
    private static readonly Lens<ExposureState, Metering> Metering =
        Lens<ExposureState, Metering>.New(static state => state.Metering, static metering => state => state with { Metering = metering });
    private static readonly Presentation<Percentile, float> Trim = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<CenterWeight, float> Weight = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly (StateParameter<ExposureState> X, StateParameter<ExposureState> Y) Start = FramePosition.Kinds(
        lens(Metering, Lens<Metering, FramePosition>.New(static metering => metering.Start, static start => metering => metering with { Start = start })));
    private static readonly (StateParameter<ExposureState> X, StateParameter<ExposureState> Y) End = FramePosition.Kinds(
        lens(Metering, Lens<Metering, FramePosition>.New(static metering => metering.End, static end => metering => metering with { End = end })));

    public static readonly ExposureParameter Exposure = new("exposure", new StateParameter<ExposureState>.Bounded<Exposure, float, InvalidToneValue>(
        Lens<ExposureState, Exposure>.New(static state => state.Exposure, static exposure => state => state with { Exposure = exposure }), Tone.Exposure.Presentation));
    public static readonly ExposureParameter MeteringLow = new("metering-low", new StateParameter<ExposureState>.Bounded<Percentile, float, InvalidToneValue>(
        lens(Metering, Lens<Metering, Percentile>.New(static metering => metering.Low, static low => metering => metering with { Low = low })), Trim));
    public static readonly ExposureParameter MeteringHigh = new("metering-high", new StateParameter<ExposureState>.Bounded<Percentile, float, InvalidToneValue>(
        lens(Metering, Lens<Metering, Percentile>.New(static metering => metering.High, static high => metering => metering with { High = high })), Trim));
    public static readonly ExposureParameter MeteringCenter = new("metering-center", new StateParameter<ExposureState>.Bounded<CenterWeight, float, InvalidToneValue>(
        lens(Metering, Lens<Metering, CenterWeight>.New(static metering => metering.Center, static center => metering => metering with { Center = center })), Weight));
    public static readonly ExposureParameter MeteringStartX = new("metering-start-x", Start.X);
    public static readonly ExposureParameter MeteringStartY = new("metering-start-y", Start.Y);
    public static readonly ExposureParameter MeteringEndX = new("metering-end-x", End.X);
    public static readonly ExposureParameter MeteringEndY = new("metering-end-y", End.Y);

    public StateParameter<ExposureState> Kind { get; }
}
