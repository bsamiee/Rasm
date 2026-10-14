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
            : ((float)state.Contrast, (float)state.Vibrance, (float)state.Saturation, Exposure.MiddleGrey * double.Exp2(state.Pivot), context.Working.Luminance) switch {
                var (contrast, vibrance, saturation, pivot, luma) => Some<PixelPass>(new PixelPass.Color(row => {
                    double Curve(float value) => double.CopySign(pivot * double.Pow(double.Abs(value) / pivot, contrast), value);
                    foreach (ref Vector4 pixel in row) {
                        (double red, double green, double blue) = (Curve(pixel.X), Curve(pixel.Y), Curve(pixel.Z));
                        double high = double.Max(red, double.Max(green, blue));
                        double s = high > 0d ? 1d - (double.Max(double.Min(red, double.Min(green, blue)), 0d) / high) : 1d;
                        double gain = saturation * (1d + (vibrance * (vibrance > 0d ? (1d - s) * (1d - s) : s * s / 2d)));
                        double grey = (red * luma.X) + (green * luma.Y) + (blue * luma.Z);
                        pixel = new Vector4((float)double.Lerp(grey, red, gain), (float)double.Lerp(grey, green, gain), (float)double.Lerp(grey, blue, gain), pixel.W);
                    }
                })),
            };
}

[SmartEnum<string>]
[ValidationError<InvalidGrade>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContrastGradeParameter : IStateParameter<ContrastGrade> {
    internal static Presentation<float> Chroma { get; } = new() {
        Soft = (0f, 2f),
        Origin = (float)Grade.Saturation.Neutral,
        Stops = Ramp.Create(
            Seq(RampStop.Create(RampPosition.MinValue, new Vector4(new Vector3(Exposure.MiddleGrey), 1f)), RampStop.Create(RampPosition.MaxValue, new Vector4(1f, 0f, 0f, 1f))),
            RampInterpolation.Linear),
    };

    public static readonly ContrastGradeParameter Contrast = new(
        "contrast",
        StateParameter.Bounded<ContrastGrade, Contrast, float, InvalidGrade>(Lens<ContrastGrade, Contrast>.New(static grade => grade.Contrast, static contrast => grade => grade with { Contrast = contrast }),
        new() { Origin = (float)Grade.Contrast.Neutral }));
    public static readonly ContrastGradeParameter Pivot = new(
        "pivot",
        StateParameter.Bounded<ContrastGrade, ContrastPivot, float, InvalidGrade>(Lens<ContrastGrade, ContrastPivot>.New(static grade => grade.Pivot, static pivot => grade => grade with { Pivot = pivot }),
        new() { Origin = (float)ContrastPivot.MiddleGrey }));
    public static readonly ContrastGradeParameter Vibrance = new(
        "vibrance",
        StateParameter.Bounded<ContrastGrade, Vibrance, float, InvalidGrade>(Lens<ContrastGrade, Vibrance>.New(static grade => grade.Vibrance, static vibrance => grade => grade with { Vibrance = vibrance }),
        new() { Origin = (float)Grade.Vibrance.Neutral }));
    public static readonly ContrastGradeParameter Saturation = new(
        "saturation",
        StateParameter.Bounded<ContrastGrade, Saturation, float, InvalidGrade>(Lens<ContrastGrade, Saturation>.New(static grade => grade.Saturation, static saturation => grade => grade with { Saturation = saturation }),
        Chroma));

    public StateParameter<ContrastGrade> Kind { get; }
}
