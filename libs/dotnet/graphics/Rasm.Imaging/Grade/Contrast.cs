using System.Numerics;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;

namespace Rasm.Imaging.Grade;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Contrast : IMinMaxValue<Contrast> {
    public static Contrast MinValue { get; } = new(0.01f);
    public static Contrast MaxValue { get; } = new(2f);
    public static Contrast Neutral { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "MiddleGray", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ContrastPivot : IMinMaxValue<ContrastPivot> {
    public static ContrastPivot MinValue { get; } = new((float)CurveAxis.Stops.Span.Low);
    public static ContrastPivot MaxValue { get; } = new((float)CurveAxis.Stops.Span.High);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record ContrastGrade(Contrast Contrast, ContrastPivot Pivot, SignedFraction Vibrance, Magnitude Saturation)
    : IStateRecord<ContrastGrade, ContrastGradeParameter>, IPixelStage<ContrastGrade> {
    public static ContrastGrade Default { get; } = new(Contrast.Neutral, ContrastPivot.MiddleGray, SignedFraction.Neutral, Magnitude.Unit);

    public static Option<PixelPass> Pass(ContrastGrade state, PassContext context) =>
        state with { Pivot = Default.Pivot } == Default
            ? None
            : ((float)state.Contrast, (float)state.Vibrance, (float)state.Saturation, Exposure.MiddleGray * double.Exp2(state.Pivot), context.Working.Luminance) switch {
                var (contrast, vibrance, saturation, pivot, luma) => Some<PixelPass>(new PixelPass.Color(row => {
                    double Curve(float value) => double.CopySign(pivot * double.Pow(double.Abs(value) / pivot, contrast), value);
                    foreach (ref Vector4 pixel in row) {
                        (double red, double green, double blue) = (Curve(pixel.X), Curve(pixel.Y), Curve(pixel.Z));
                        double high = double.Max(red, double.Max(green, blue));
                        double s = high > 0d ? 1d - (double.Max(double.Min(red, double.Min(green, blue)), 0d) / high) : 1d;
                        double gain = saturation * (1d + (vibrance * (vibrance > 0d ? (1d - s) * (1d - s) : s * s / 2d)));
                        double gray = LuminanceSamples.Luminance(red, green, blue, luma);
                        pixel = new Vector4((float)double.Lerp(gray, red, gain), (float)double.Lerp(gray, green, gain), (float)double.Lerp(gray, blue, gain), pixel.W);
                    }
                })),
            };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContrastGradeParameter : IStateParameter<ContrastGrade> {
    internal static Presentation<float> Chroma { get; } = new() {
        Soft = (0f, 2f),
        Origin = (float)Magnitude.Unit,
        Stops = Ramp.Create(
            Seq(RampStop.Create(RampPosition.MinValue, new Vector4(new Vector3(Exposure.MiddleGray), 1f)), RampStop.Create(RampPosition.MaxValue, new Vector4(1f, 0f, 0f, 1f))),
            RampInterpolation.Linear),
    };

    public static readonly ContrastGradeParameter Contrast = new("contrast", StateParameter.Bounded(Lens<ContrastGrade, Contrast>.New(static grade => grade.Contrast, static contrast => grade => grade with { Contrast = contrast }), new Presentation<float> { Origin = (float)Grade.Contrast.Neutral }),
        new("Contrast", "Exponent about the pivot gray, one leaving the image unchanged"));
    public static readonly ContrastGradeParameter Pivot = new("pivot", StateParameter.Bounded(Lens<ContrastGrade, ContrastPivot>.New(static grade => grade.Pivot, static pivot => grade => grade with { Pivot = pivot }), new Presentation<float> { Origin = (float)ContrastPivot.MiddleGray }),
        new("Pivot", "Gray the contrast turns about, in stops from middle gray"));
    public static readonly ContrastGradeParameter Vibrance = new("vibrance", StateParameter.Bounded(Lens<ContrastGrade, SignedFraction>.New(static grade => grade.Vibrance, static vibrance => grade => grade with { Vibrance = vibrance })),
        new("Vibrance", "Saturation gain that falls as a pixel's saturation rises"));
    public static readonly ContrastGradeParameter Saturation = new("saturation", StateParameter.Bounded(Lens<ContrastGrade, Magnitude>.New(static grade => grade.Saturation, static saturation => grade => grade with { Saturation = saturation }), Chroma),
        new("Saturation", "Distance from working luminance, zero rendering gray"));

    public StateParameter<ContrastGrade> Kind { get; }
    public ParameterText Text { get; }
}
