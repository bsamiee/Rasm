using System.Numerics;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct Contrast : IMinMaxValue<Contrast> {
    public static Contrast MinValue { get; } = new(0.01f);
    public static Contrast MaxValue { get; } = new(2f);
    public static Contrast Neutral { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "MiddleGrey", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct ContrastPivot : IMinMaxValue<ContrastPivot> {
    public static ContrastPivot MinValue { get; } = new((float)CurveAxis.Stops.Span.Low);
    public static ContrastPivot MaxValue { get; } = new((float)CurveAxis.Stops.Span.High);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct Vibrance : IMinMaxValue<Vibrance> {
    public static Vibrance MinValue { get; } = new(-1f);
    public static Vibrance MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidGrade>]
public readonly partial struct Saturation : IMinMaxValue<Saturation> {
    public static Saturation MinValue { get; } = new(0f);
    public static Saturation MaxValue { get; } = new(float.MaxValue);
    public static Saturation Neutral { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidGrade? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidGrade();
}

public sealed record ContrastGrade(Contrast Contrast, ContrastPivot Pivot, Vibrance Vibrance, Saturation Saturation)
    : IStateRecord<ContrastGrade, ContrastGradeParameter, InvalidGrade>, IPixelStage<ContrastGrade> {
    public static ContrastGrade Default { get; } = new(Contrast.Neutral, ContrastPivot.MiddleGrey, Vibrance.Neutral, Saturation.Neutral);

    public static Option<PixelPass> Pass(ContrastGrade state, PassContext context) =>
        state with { Pivot = Default.Pivot } == Default
            ? None
            : ((float)state.Contrast, (float)state.Vibrance, (float)state.Saturation, new Vector3(Exposure.MiddleGrey * float.Exp2(state.Pivot)), context.Working.Luminance) switch {
                var (contrast, vibrance, saturation, pivot, luma) => Some<PixelPass>(new PixelPass.Color(row => {
                    foreach (ref Vector4 pixel in row) {
                        Vector3 k = Vector3.CopySign(pivot * Vector3.Exp(contrast * Vector3.Log(Vector3.Abs(pixel.AsVector3()) / pivot)), pixel.AsVector3());
                        float high = float.Max(k.X, float.Max(k.Y, k.Z));
                        float s = high > 0f ? 1f - (float.Max(float.Min(k.X, float.Min(k.Y, k.Z)), 0f) / high) : 1f;
                        float gain = saturation * (1f + (vibrance * (vibrance > 0f ? (1f - s) * (1f - s) : s * s / 2f)));
                        pixel = new Vector4(Vector3.Lerp(new Vector3(Vector3.Dot(k, luma)), k, gain), pixel.W);
                    }
                })),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContrastGradeParameter : IStateParameter<ContrastGrade> {
    internal static Presentation<Saturation, float> Chroma { get; } = new() {
        Soft = (0f, 2f),
        Origin = (float)Grade.Saturation.Neutral,
        Stops = Ramp.Create(
            Seq(RampStop.Create(RampPosition.MinValue, new Vector4(new Vector3(Exposure.MiddleGrey), 1f)), RampStop.Create(RampPosition.MaxValue, new Vector4(1f, 0f, 0f, 1f))),
            RampInterpolation.Linear),
    };

    public static readonly ContrastGradeParameter Contrast = new(
        "contrast",
        new StateParameter<ContrastGrade>.Bounded<Contrast, float, InvalidGrade>(
            Lens<ContrastGrade, Contrast>.New(static grade => grade.Contrast, static contrast => grade => grade with { Contrast = contrast }),
            new() { Origin = (float)Grade.Contrast.Neutral }));
    public static readonly ContrastGradeParameter Pivot = new(
        "pivot",
        new StateParameter<ContrastGrade>.Bounded<ContrastPivot, float, InvalidGrade>(
            Lens<ContrastGrade, ContrastPivot>.New(static grade => grade.Pivot, static pivot => grade => grade with { Pivot = pivot }),
            new() { Origin = (float)ContrastPivot.MiddleGrey }));
    public static readonly ContrastGradeParameter Vibrance = new(
        "vibrance",
        new StateParameter<ContrastGrade>.Bounded<Vibrance, float, InvalidGrade>(
            Lens<ContrastGrade, Vibrance>.New(static grade => grade.Vibrance, static vibrance => grade => grade with { Vibrance = vibrance }),
            new() { Origin = (float)Grade.Vibrance.Neutral }));
    public static readonly ContrastGradeParameter Saturation = new(
        "saturation",
        new StateParameter<ContrastGrade>.Bounded<Saturation, float, InvalidGrade>(
            Lens<ContrastGrade, Saturation>.New(static grade => grade.Saturation, static saturation => grade => grade with { Saturation = saturation }),
            Chroma));

    public StateParameter<ContrastGrade> Kind { get; }
}
