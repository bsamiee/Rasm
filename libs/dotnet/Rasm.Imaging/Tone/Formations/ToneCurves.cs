using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone.Formations;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IToneCurve {
    public float Map(float value);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct MiddleGreyContrast : IMinMaxValue<MiddleGreyContrast> {
    public static MiddleGreyContrast MinValue { get; } = new(0.1f);
    public static MiddleGreyContrast MaxValue { get; } = new(10f);
    public static MiddleGreyContrast Default { get; } = new(1.5f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ContrastSkewness : IMinMaxValue<ContrastSkewness> {
    public static ContrastSkewness MinValue { get; } = new(-1f);
    public static ContrastSkewness MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct TargetBlack : IMinMaxValue<TargetBlack> {
    public static TargetBlack MinValue { get; } = new(0f);
    public static TargetBlack MaxValue { get; } = new(0.15f);
    public static TargetBlack Default { get; } = new(0.000152f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true, ConstructorAccessModifier = AccessModifier.Internal,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct Factor : IMinMaxValue<Factor> {
    public static Factor MinValue { get; } = new(0f);
    public static Factor MaxValue { get; } = new(1f);
    public static Factor Half { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ShoulderStrength : IMinMaxValue<ShoulderStrength> {
    public static ShoulderStrength MinValue { get; } = new(0f);
    public static ShoulderStrength MaxValue { get; } = new(16f);
    public static ShoulderStrength Default { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ShoulderLength : IMinMaxValue<ShoulderLength> {
    public static ShoulderLength MinValue { get; } = new(1e-5f);
    public static ShoulderLength MaxValue { get; } = new(1f);
    public static ShoulderLength Default { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct CurveGamma : IMinMaxValue<CurveGamma> {
    public static CurveGamma MinValue { get; } = new(0.1f);
    public static CurveGamma MaxValue { get; } = new(10f);
    public static CurveGamma Linear { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct DragoBias : IMinMaxValue<DragoBias> {
    public static DragoBias MinValue { get; } = new(0.5f);
    public static DragoBias MaxValue { get; } = new(1f);
    public static DragoBias Default { get; } = new(0.85f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ReceptorIntensity : IMinMaxValue<ReceptorIntensity> {
    public static ReceptorIntensity MinValue { get; } = new(-float.Log(float.MaxValue));
    public static ReceptorIntensity MaxValue { get; } = new(float.Log(float.MaxValue));

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ReceptorContrast : IMinMaxValue<ReceptorContrast> {
    public static ReceptorContrast MinValue { get; } = new(float.BitIncrement(0f));
    public static ReceptorContrast MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ShoulderCoefficient : IMinMaxValue<ShoulderCoefficient> {
    public static ShoulderCoefficient MinValue { get; } = new(0.001f);
    public static ShoulderCoefficient MaxValue { get; } = new(0.5f);
    public static ShoulderCoefficient Published { get; } = new(0.15f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct LinearCoefficient : IMinMaxValue<LinearCoefficient> {
    public static LinearCoefficient MinValue { get; } = new(0f);
    public static LinearCoefficient MaxValue { get; } = new(0.6f);
    public static LinearCoefficient Published { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct LinearAngle : IMinMaxValue<LinearAngle> {
    public static LinearAngle MinValue { get; } = new(0f);
    public static LinearAngle MaxValue { get; } = new(0.3f);
    public static LinearAngle Published { get; } = new(0.1f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ToeCoefficient : IMinMaxValue<ToeCoefficient> {
    public static ToeCoefficient MinValue { get; } = new(float.BitIncrement(0f));
    public static ToeCoefficient MaxValue { get; } = new(0.9f);
    public static ToeCoefficient Published { get; } = new(0.2f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ToeNumerator : IMinMaxValue<ToeNumerator> {
    public static ToeNumerator MinValue { get; } = new(0.0001f);
    public static ToeNumerator MaxValue { get; } = new(0.05f);
    public static ToeNumerator Published { get; } = new(0.02f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct ToeDenominator : IMinMaxValue<ToeDenominator> {
    public static ToeDenominator MinValue { get; } = new(0.2f);
    public static ToeDenominator MaxValue { get; } = new(0.6f);
    public static ToeDenominator Published { get; } = new(0.3f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct PrimaryInset : IMinMaxValue<PrimaryInset> {
    public static PrimaryInset MinValue => Neutral;
    public static PrimaryInset MaxValue { get; } = new(0.99f);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidToneValue>]
public readonly partial struct PrimaryRotation : IMinMaxValue<PrimaryRotation> {
    public static PrimaryRotation MaxValue { get; } = new(float.BitDecrement((float)((Math.PI - (
        from gamut in toSeq(Gamut.Items)
        let rgb = gamut.Configuration.Rgb
        from side in Sides(rgb)
        select double.Ieee754Remainder(Angle(rgb, side.To) - Angle(rgb, side.From) - Math.PI, Math.Tau) + Math.PI).Fold(0d, Math.Max)) / 2d)));
    public static PrimaryRotation MinValue { get; } = new(-MaxValue._value);

    internal static Seq<(Chromaticity From, Chromaticity To)> Sides(RgbConfiguration rgb) =>
        Seq((rgb.ChromaticityR, rgb.ChromaticityG), (rgb.ChromaticityG, rgb.ChromaticityB), (rgb.ChromaticityB, rgb.ChromaticityR));

    internal static double Angle(RgbConfiguration rgb, Chromaticity primary) =>
        Math.Atan2(primary.Y - rgb.WhitePoint.Chromaticity.Y, primary.X - rgb.WhitePoint.Chromaticity.X);

    static partial void ValidateFactoryArguments(ref InvalidToneValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidToneValue();
}

public sealed record CurveApplication(
    Factor PreserveHue, Factor Purity, Option<Gamut> BasePrimaries,
    PrimaryInset RedInset, PrimaryInset GreenInset, PrimaryInset BlueInset,
    PrimaryRotation RedRotation, PrimaryRotation GreenRotation, PrimaryRotation BlueRotation) {
    public static CurveApplication Default { get; } = new(
        Factor.MaxValue, Factor.MinValue, None,
        PrimaryInset.Neutral, PrimaryInset.Neutral, PrimaryInset.Neutral,
        PrimaryRotation.Neutral, PrimaryRotation.Neutral, PrimaryRotation.Neutral);
}

public sealed record SigmoidState(MiddleGreyContrast Contrast, ContrastSkewness Skew, TargetBlack Black) {
    public static SigmoidState Default { get; } = new(MiddleGreyContrast.Default, ContrastSkewness.Neutral, TargetBlack.Default);
}

public sealed record PiecewiseState(
    Factor ToeStrength, Factor ToeLength, Factor ShoulderAngle, CurveGamma Gamma, ShoulderStrength ShoulderStrength, ShoulderLength ShoulderLength) {
    public static PiecewiseState Default { get; } = new(Factor.Half, Factor.Half, Factor.MaxValue, CurveGamma.Linear, ShoulderStrength.Default, ShoulderLength.Default);
}

public sealed record LogarithmicState(DragoBias Bias, Option<Exposure> White) {
    public static LogarithmicState Default { get; } = new(DragoBias.Default, None);
}

public sealed record PhotoreceptorState(ReceptorIntensity Intensity, Option<ReceptorContrast> Contrast, Factor LightAdaptation, Factor ChromaticAdaptation) {
    public static PhotoreceptorState Default { get; } = new(ReceptorIntensity.Neutral, None, Factor.MinValue, Factor.MinValue);
}

public sealed record HableState(
    ShoulderCoefficient A, LinearCoefficient B, LinearAngle C, ToeCoefficient D, ToeNumerator E, ToeDenominator F, Option<Exposure> White) {
    public static HableState Default { get; } = new(
        ShoulderCoefficient.Published, LinearCoefficient.Published, LinearAngle.Published,
        ToeCoefficient.Published, ToeNumerator.Published, ToeDenominator.Published, Some(new Exposure(float.Log2(11.2f))));
}

public readonly record struct SigmoidCurve(float WhiteTarget, float PaperExposure, float FilmFog, float ContrastPower, float SkewPower) : IToneCurve {
    public float Map(float value) =>
        WhiteTarget * MathF.Pow(1f + (PaperExposure * MathF.Pow(FilmFog + float.MaxNumber(value, 0f), -ContrastPower)), -SkewPower);

    public static SigmoidCurve Of(SigmoidState state, Display display) {
        double grey = Exposure.MiddleGrey;
        double peak = (float)display.Peak / (float)Nits.ReferenceWhite;
        double skew = Math.Pow(5d, -(float)state.Skew);
        double contrast = (float)state.Contrast * (1d - grey) / (skew * (1d - Math.Pow(grey / peak, 1d / skew)));
        double towardGrey = Math.Pow(peak / grey, 1d / skew) - 1d;
        double black = Math.Pow((float)state.Black, 1d / skew);
        double lifted = Math.Pow(towardGrey * black, 1d / contrast);
        double fog = grey * lifted / (Math.Pow(1d - black, 1d / contrast) - lifted);
        return new((float)peak, (float)(Math.Pow(fog + grey, contrast) * towardGrey), (float)fog, (float)contrast, (float)skew);
    }
}

public readonly record struct PiecewiseCurve(
    float ToeEnd, float ToeValue, float ToePower, float Intercept, float Gamma,
    float ShoulderStart, float ShoulderValue, float ShoulderPower, float ShoulderEnd, float Ceiling, float Scale) : IToneCurve {
    public float Map(float value) =>
        Scale * (float.MaxNumber(value, 0f) switch {
            var x when x < ToeEnd => ToeValue * MathF.Pow(x / ToeEnd, ToePower),
            var x when x < ShoulderStart => MathF.Pow(x + Intercept, Gamma),
            var x when x < ShoulderEnd => Ceiling - ((Ceiling - ShoulderValue) * MathF.Pow((ShoulderEnd - x) / (ShoulderEnd - ShoulderStart), ShoulderPower)),
            _ => Ceiling,
        });

    public static PiecewiseCurve Of(PiecewiseState state) {
        double gamma = (float)state.Gamma;
        double strength = (float)state.ShoulderStrength;
        double toeX = 0.5d * Math.Pow((float)state.ToeLength, 2.2d);
        double toeY = (1d - (float)state.ToeStrength) * toeX;
        double shoulderX = toeX + ((1d - (float)state.ShoulderLength) * (1d - toeY));
        double white = toeX + (1d - toeY) + Math.Pow(2d, strength) - 1d;
        double end = white * (1d + (2d * (float)state.ShoulderAngle * strength));
        double ceiling = Math.Pow(1d + (0.5d * (float)state.ShoulderAngle * strength), gamma);
        (double toe, double shoulder) = (Math.Max(1e-5d, Math.Pow(toeY, gamma)), Math.Max(1e-5d, Math.Pow(toeY + shoulderX - toeX, gamma)));
        double shoulderPower = gamma * Math.Pow(shoulder, 1d - (1d / gamma)) * (end - shoulderX) / (ceiling - shoulder);
        return new(
            (float)toeX, (float)toe, (float)(gamma * toeX * Math.Pow(toe, -1d / gamma)), (float)(toeY - toeX), (float)gamma,
            (float)shoulderX, (float)shoulder, (float)shoulderPower, (float)end, (float)ceiling,
            (float)(1d / (ceiling - ((ceiling - shoulder) * Math.Pow((end - white) / (end - shoulderX), shoulderPower)))));
    }
}

public readonly record struct LogarithmicCurve(float WhiteTarget, float Normalizer, float InverseWhite, float Bias) : IToneCurve {
    public float Map(float value) =>
        float.MaxNumber(value, 0f) switch {
            var x => WhiteTarget * float.LogP1(x) / (Normalizer * MathF.Log(2f + (8f * MathF.Pow(x * InverseWhite, Bias)))),
        };

    public static LogarithmicCurve Of(LogarithmicState state, Display display, float white) =>
        new((float)display.Peak / (float)Nits.ReferenceWhite, float.Log10P1(white), 1f / white, (float)(Math.Log((float)state.Bias) / Math.Log(0.5d)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToneCurves {
    private static readonly Action<Span<Vector4>> Desaturated = static row => {
        foreach (ref Vector4 pixel in row)
            pixel = (float.Max((pixel.X + pixel.Y + pixel.Z) / 3f, 0f), float.Min(pixel.X, float.Min(pixel.Y, pixel.Z))) switch {
                var (average, least) when least < 0f => new Vector4(new Vector3(average) + (-average / (least - average) * (pixel.AsVector3() - new Vector3(average))), pixel.W),
                _ => pixel,
            };
    };

    private static readonly Action<Span<Vector4>> Neutral = static row => {
        const float fresnel = 0.04f;
        const float specular = 0.8f - fresnel;
        const float desaturation = 0.15f;
        foreach (ref Vector4 pixel in row) {
            float least = float.Min(pixel.X, float.Min(pixel.Y, pixel.Z));
            Vector3 shifted = pixel.AsVector3() - new Vector3(least < 2f * fresnel ? least - (least * least / (4f * fresnel)) : fresnel);
            float peak = float.Max(shifted.X, float.Max(shifted.Y, shifted.Z));
            float compressed = 1f - ((1f - specular) * (1f - specular) / (peak + (1f - specular) - specular));
            pixel = peak < specular
                ? new Vector4(shifted, pixel.W)
                : new Vector4(Vector3.Lerp(shifted * (compressed / peak), new Vector3(compressed), 1f - (1f / ((desaturation * (peak - compressed)) + 1f))), pixel.W);
        }
    };

    public static Vector3 PreserveHue(Vector3 input, Vector3 mapped, Factor preserve) {
        (int min, int mid, int max) = input.X >= input.Y
            ? input.Y > input.Z ? (2, 1, 0) : input.Z > input.X ? (1, 0, 2) : input.Z > input.Y ? (1, 2, 0) : (2, 1, 0)
            : input.X >= input.Z ? (2, 0, 1) : input.Z > input.Y ? (0, 1, 2) : (0, 2, 1);
        float hue = preserve;
        float chroma = input[max] - input[min];
        float midscale = chroma != 0f ? (input[mid] - input[min]) / chroma : 0f;
        float naive = ((1f - hue) * mapped[mid]) + (hue * (mapped[min] + ((mapped[max] - mapped[min]) * midscale)));
        float blend = input[min] + input[mid] != 0f ? 2f * input[min] / (input[min] + input[mid]) : 0f;
        float target = (blend * (mapped.X + mapped.Y + mapped.Z)) + ((1f - blend) * (mapped[min] + naive + mapped[max]));
        (float low, float middle, float high) = naive <= mapped[mid]
            ? ((((1f - hue) * mapped[mid]) + (hue * ((midscale * mapped[max]) + ((1f - midscale) * (target - mapped[max]))))) / (1f + (hue * (1f - midscale)))) switch {
                var corrected => (target - mapped[max] - corrected, corrected, mapped[max]),
            }
            : ((((1f - hue) * mapped[mid]) + (hue * ((mapped[min] * (1f - midscale)) + (midscale * (target - mapped[min]))))) / (1f + (hue * midscale))) switch {
                var corrected => (mapped[min], corrected, target - mapped[min] - corrected),
            };
        return new(Placed(0), Placed(1), Placed(2));

        float Placed(int channel) => channel == min ? low : channel == mid ? middle : high;
    }

    internal static PixelPass Sigmoid(ToneMapping state, PassContext context) =>
        Applied(state, context, SigmoidCurve.Of(state.Sigmoid, context.Display));

    internal static PixelPass Piecewise(ToneMapping state, PassContext context) =>
        Applied(state, context, PiecewiseCurve.Of(state.Piecewise));

    internal static PixelPass Logarithmic(ToneMapping state, PassContext context) =>
        Whitened(context, state.Logarithmic.White, white => Applied(state, context, LogarithmicCurve.Of(state.Logarithmic, context.Display, white)));

    internal static PixelPass Photoreceptor(ToneMapping state, PassContext context) =>
        Measured(context, (frame, samples) => Curved(context, context.Working, Seq(Adapted(state.Photoreceptor, context.Working.Luminance, frame, samples.LogStatistics))));

    internal static PixelPass Reinhard(ToneMapping _, PassContext context) =>
        ToneMapped(context, ToneMapOperator.Reinhard, new ToneMapParameters());

    internal static PixelPass ReinhardExtended(ToneMapping state, PassContext context) =>
        Whitened(context, state.ReinhardWhite, white => ToneMapped(context, ToneMapOperator.ReinhardExtended, new ToneMapParameters(whitePoint: white)));

    internal static PixelPass Hable(ToneMapping state, PassContext context) =>
        state.Hable switch {
            var hable => Whitened(context, hable.White, white => ToneMapped(context, ToneMapOperator.Hable, new ToneMapParameters(
                a: hable.A, b: hable.B, c: hable.C, d: hable.D, e: (float)hable.B != 0f ? float.Min(hable.E, hable.C * (float)hable.F) : hable.E, f: hable.F, w: white))),
        };

    internal static PixelPass PbrNeutral(ToneMapping _, PassContext context) =>
        Curved(context, Gamut.StandardRgb, Seq(Neutral));

    private static PixelPass.Color Applied<TCurve>(ToneMapping state, PassContext context, TCurve curve) where TCurve : struct, IToneCurve {
        (CurveApplication application, Gamut basis) = (state.Application, state.Application.BasePrimaries.IfNone(context.Working));
        return Curved(context, basis, Seq(
            new LutTable.Affine(Gamut.Between(Adjusted(basis, application, Factor.MaxValue), basis.Configuration), Vector3.Zero).Apply,
            row => {
                foreach (ref Vector4 pixel in row)
                    pixel = new Vector4(PreserveHue(pixel.AsVector3(), new Vector3(curve.Map(pixel.X), curve.Map(pixel.Y), curve.Map(pixel.Z)), application.PreserveHue), pixel.W);
            },
            new LutTable.Affine(Gamut.Between(basis.Configuration, Adjusted(basis, application, application.Purity)), Vector3.Zero).Apply));
    }

    private static PixelPass.Color Curved(PassContext context, Gamut basis, Seq<Action<Span<Vector4>>> curve) =>
        Formation.Formed(
            Seq(new LutTable.Affine(context.Working.MatrixTo(basis), Vector3.Zero).Apply, Desaturated) + curve,
            new ColorEncoding(basis, new Transfer.Standard(TransferCurve.Linear), Nits.ReferenceWhite), context.Display);

    private static PixelPass Whitened(PassContext context, Option<Exposure> white, Func<float, PixelPass> formed) =>
        white.Match(
            Some: manual => formed(manual.Scale),
            None: () => Measured(context, (_, samples) =>
                Exposure.Validate(float.Log2(samples.FrameWhite), provider: null, out Exposure measured) is null
                    ? formed(measured.Scale)
                    : new FrameWhiteOutOfRange(samples.FrameWhite)));

    private static PixelPass.Frame Measured(PassContext context, Func<PixelFrame, LuminanceSamples, Fin<PixelPass>> formed) =>
        new((frame, progress) =>
            LuminanceSamples.Gather(frame, context.Working.Luminance)
                .Bind(samples => formed(frame, samples))
                .Bind(pass => pass.Run(frame, progress)));

    private static PixelPass.Color ToneMapped(PassContext context, ToneMapOperator operation, ToneMapParameters parameters) =>
        Curved(context, context.Working, Seq<Action<Span<Vector4>>>(row => {
            Span<float> lanes = MemoryMarshal.Cast<Vector4, float>(row);
            ImageProcessing.ToneMap(lanes, lanes, 4, operation, parameters);
        }));

    private static Action<Span<Vector4>> Adapted(PhotoreceptorState state, Vector3 weights, PixelFrame frame, (float Mean, float Maximum, float Minimum) logs) {
        (double red, double green, double blue, long count) = (0d, 0d, 0d, 0L);
        foreach (Vector4 pixel in frame.View.Span)
            (red, green, blue, count) = pixel.W > 0f ? (red + pixel.X, green + pixel.Y, blue + pixel.Z, count + 1L) : (red, green, blue, count);
        Vector3 mean = new((float)(red / count), (float)(green / count), (float)(blue / count));
        float intensity = MathF.Exp(-(float)state.Intensity);
        float contrast = state.Contrast.Match(
            Some: static manual => (float)manual,
            None: () => logs.Maximum == logs.Minimum ? 1f : 0.3f + (0.7f * MathF.Pow((logs.Maximum - logs.Mean) / (logs.Maximum - logs.Minimum), 1.4f)));
        (float light, float chromatic) = (state.LightAdaptation, state.ChromaticAdaptation);
        Vector3 global = Vector3.Lerp(new Vector3(Vector3.Dot(weights, mean)), mean, chromatic);
        Vector3 resting = Raised(intensity * global, contrast);
        return row => {
            foreach (ref Vector4 pixel in row) {
                Vector3 color = pixel.AsVector3();
                Vector3 total = color + (light == 0f ? resting : Raised(intensity * Vector3.Lerp(global, Vector3.Lerp(new Vector3(Vector3.Dot(weights, color)), color, chromatic), light), contrast));
                pixel = new Vector4(Vector3.ConditionalSelect(Vector3.Equals(total, Vector3.Zero), Vector3.Zero, color / total), pixel.W);
            }
        };

        static Vector3 Raised(Vector3 value, float power) => new(MathF.Pow(value.X, power), MathF.Pow(value.Y, power), MathF.Pow(value.Z, power));
    }

    private static Configuration Adjusted(Gamut basis, CurveApplication application, Factor share) =>
        basis.Configuration.Rgb switch {
            var rgb => new(new RgbConfiguration(
                Moved(rgb, rgb.ChromaticityR, 1d - ((float)share * application.RedInset), application.RedRotation),
                Moved(rgb, rgb.ChromaticityG, 1d - ((float)share * application.GreenInset), application.GreenRotation),
                Moved(rgb, rgb.ChromaticityB, 1d - ((float)share * application.BlueInset), application.BlueRotation),
                rgb.WhitePoint, static linear => linear, static linear => linear)),
        };

    private static Chromaticity Moved(RgbConfiguration rgb, Chromaticity primary, double scale, double rotation) {
        Chromaticity white = rgb.WhitePoint.Chromaticity;
        (double sin, double cos) = Math.SinCos(PrimaryRotation.Angle(rgb, primary) + rotation);
        double edge = PrimaryRotation.Sides(rgb)
            .Map(side => (((white.X - side.From.X) * (side.From.Y - side.To.Y)) - ((side.From.X - side.To.X) * (white.Y - side.From.Y)))
                / ((sin * (side.From.X - side.To.X)) - (cos * (side.From.Y - side.To.Y))))
            .Filter(static reach => reach >= 0d)
            .Fold(double.PositiveInfinity, Math.Min);
        return new(white.X + (scale * edge * cos), white.Y + (scale * edge * sin));
    }
}
