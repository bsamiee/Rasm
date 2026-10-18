using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Exposure : IPresented<Exposure, float> {
    public static Exposure MinValue { get; } = new(-32f);
    public static Exposure MaxValue { get; } = new(32f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (-10f, 10f), Origin = 0f, Step = 0.01f, Decimals = 3 };
    public static float MiddleGray => 0.18f;

    public float Scale => float.Exp2(_value);

    public static double Applied(float scale, float amount) => double.Log2(double.Lerp(1d, scale, amount));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record ExposureState(Exposure Exposure, Metering Metering)
    : IStateRecord<ExposureState, ExposureParameter>, IPixelStage<ExposureState> {
    public static ExposureState Default { get; } = new(Exposure.Neutral, Metering.Default);

    public static Option<PixelPass> Pass(ExposureState state, PassContext context) =>
        state.Exposure == Exposure.Neutral
            ? None
            : Some<PixelPass>(new PixelPass.Color(new LutTable.Affine(Matrix<double>.Build.DenseDiagonal(3, state.Exposure.Scale))));

    public Fin<ExposureState> Metered(PixelFrame frame, Gamut working) =>
        Metering.Metered(frame, working.Luminance).Bind(static stops => Exposure.Validated(stops)).Map(metered => this with { Exposure = metered });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExposureParameter : IStateParameter<ExposureState> {
    private static readonly Lens<ExposureState, Metering> Metering =
        Lens<ExposureState, Metering>.New(static state => state.Metering, static metering => state => state with { Metering = metering });

    public static readonly ExposureParameter Exposure = new("exposure", StateParameter.Bounded(Lens<ExposureState, Exposure>.New(static state => state.Exposure, static exposure => state => state with { Exposure = exposure })),
        new("Exposure", "Scene gain in stops"));
    public static readonly ExposureParameter MeteringLow = new("metering-low", StateParameter.Bounded(lens(Metering, Lens<Metering, AxisFraction>.New(static metering => metering.Low, static low => metering => metering with { Low = low }))),
        new("Meter low", "Lowest luminance rank the meter averages"));
    public static readonly ExposureParameter MeteringHigh = new("metering-high", StateParameter.Bounded(lens(Metering, Lens<Metering, AxisFraction>.New(static metering => metering.High, static high => metering => metering with { High = high }))),
        new("Meter high", "Highest luminance rank the meter averages"));
    public static readonly ExposureParameter MeteringCenter = new("metering-center", StateParameter.Bounded(lens(Metering, Lens<Metering, AxisFraction>.New(static metering => metering.Center, static center => metering => metering with { Center = center }))),
        new("Center weight", "Share by which samples outside the central half count less"));
    public static readonly ExposureParameter MeteringStart = new("metering-start", new StateParameter<ExposureState>.Record<FramePosition>(lens(Metering, Lens<Metering, FramePosition>.New(static metering => metering.Start, static start => metering => metering with { Start = start }))),
        new("Region start", "Upper left corner of the metered region"));
    public static readonly ExposureParameter MeteringEnd = new("metering-end", new StateParameter<ExposureState>.Record<FramePosition>(lens(Metering, Lens<Metering, FramePosition>.New(static metering => metering.End, static end => metering => metering with { End = end }))),
        new("Region end", "Lower right corner of the metered region"));

    public StateParameter<ExposureState> Kind { get; }
    public ParameterText Text { get; }
}
