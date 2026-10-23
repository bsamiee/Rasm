using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IToneCurve {
    public float Map(float value);
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TargetBlack : IMinMaxValue<TargetBlack> {
    public static TargetBlack MinValue { get; } = new(0f);
    public static TargetBlack MaxValue { get; } = new(0.15f);
    public static TargetBlack Default { get; } = new(0.000152f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShoulderStrength : IMinMaxValue<ShoulderStrength> {
    public static ShoulderStrength MinValue { get; } = new(0f);
    public static ShoulderStrength MaxValue { get; } = new(16f);
    public static ShoulderStrength Default { get; } = new(2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShoulderLength : IMinMaxValue<ShoulderLength> {
    public static ShoulderLength MinValue { get; } = new(1e-5f);
    public static ShoulderLength MaxValue { get; } = new(1f);
    public static ShoulderLength Default { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CurveGamma : IMinMaxValue<CurveGamma> {
    public static CurveGamma MinValue { get; } = new(0.1f);
    public static CurveGamma MaxValue { get; } = new(10f);
    public static CurveGamma Linear { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DragoBias : IMinMaxValue<DragoBias> {
    public static DragoBias MinValue { get; } = new(0.5f);
    public static DragoBias MaxValue { get; } = new(1f);
    public static DragoBias Default { get; } = new(0.85f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ReceptorIntensity : IMinMaxValue<ReceptorIntensity> {
    public static ReceptorIntensity MinValue { get; } = new(-float.Log(float.MaxValue));
    public static ReceptorIntensity MaxValue { get; } = new(float.Log(float.MaxValue));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShoulderCoefficient : IMinMaxValue<ShoulderCoefficient> {
    public static ShoulderCoefficient MinValue { get; } = new(0.001f);
    public static ShoulderCoefficient MaxValue { get; } = new(0.5f);
    public static ShoulderCoefficient Published { get; } = new(0.15f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LinearCoefficient : IMinMaxValue<LinearCoefficient> {
    public static LinearCoefficient MinValue { get; } = new(0f);
    public static LinearCoefficient MaxValue { get; } = new(0.6f);
    public static LinearCoefficient Published { get; } = new(0.5f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LinearAngle : IMinMaxValue<LinearAngle> {
    public static LinearAngle MinValue { get; } = new(0.1f);
    public static LinearAngle MaxValue { get; } = new(0.3f);
    public static LinearAngle Published { get; } = new(0.1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ToeCoefficient : IMinMaxValue<ToeCoefficient> {
    public static ToeCoefficient MinValue { get; } = new(0.001f);
    public static ToeCoefficient MaxValue { get; } = new(0.9f);
    public static ToeCoefficient Published { get; } = new(0.2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ToeNumerator : IMinMaxValue<ToeNumerator> {
    public static ToeNumerator MinValue { get; } = new(0.0001f);
    public static ToeNumerator MaxValue { get; } = new(float.BitDecrement((float)LinearAngle.MinValue * (float)ToeDenominator.MinValue));
    public static ToeNumerator Published { get; } = new(0.02f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ToeDenominator : IMinMaxValue<ToeDenominator> {
    public static ToeDenominator MinValue { get; } = new(0.2f);
    public static ToeDenominator MaxValue { get; } = new(0.6f);
    public static ToeDenominator Published { get; } = new(0.3f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HableWhite : IPresented<HableWhite, float> {
    public static HableWhite MinValue { get; } = new(-10f);
    public static HableWhite MaxValue { get; } = new((float)Exposure.MaxValue);
    public static HableWhite Published { get; } = new(float.Log2(11.2f));
    public static Presentation<float> Presentation => Exposure.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PrimaryInset : IMinMaxValue<PrimaryInset> {
    public static PrimaryInset MinValue => Neutral;
    public static PrimaryInset MaxValue { get; } = new(0.99f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true,
    AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
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

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record CurveApplication(
    AxisFraction PreserveHue, AxisFraction Purity, Option<Gamut> BasePrimaries,
    PrimaryInset RedInset, PrimaryInset GreenInset, PrimaryInset BlueInset,
    PrimaryRotation RedRotation, PrimaryRotation GreenRotation, PrimaryRotation BlueRotation) {
    public static CurveApplication Default { get; } = new(
        AxisFraction.MaxValue, AxisFraction.MinValue, None,
        PrimaryInset.Neutral, PrimaryInset.Neutral, PrimaryInset.Neutral,
        PrimaryRotation.Neutral, PrimaryRotation.Neutral, PrimaryRotation.Neutral);
}

public sealed record SigmoidState(CurveGamma Contrast, SignedFraction Skew, TargetBlack Black) {
    public static SigmoidState Default { get; } = new(CurveGamma.Create(1.5f), SignedFraction.Neutral, TargetBlack.Default);
}

public sealed record PiecewiseState(
    AxisFraction ToeStrength, AxisFraction ToeLength, AxisFraction ShoulderAngle, CurveGamma Gamma, ShoulderStrength ShoulderStrength, ShoulderLength ShoulderLength) {
    public static PiecewiseState Default { get; } = new(AxisFraction.Half, AxisFraction.Half, AxisFraction.MaxValue, CurveGamma.Linear, ShoulderStrength.Default, ShoulderLength.Default);
}

public sealed record LogarithmicState(DragoBias Bias, Gated<Exposure> White) {
    public static LogarithmicState Default { get; } = new(DragoBias.Default, new(Enabled: false, Exposure.Neutral));
}

public sealed record PhotoreceptorState(ReceptorIntensity Intensity, Gated<PositiveMagnitude> Contrast, AxisFraction LightAdaptation, AxisFraction ChromaticAdaptation) {
    public static PositiveMagnitude FlattestContrast { get; } = PositiveMagnitude.Create(0.3f);
    public static PhotoreceptorState Default { get; } = new(ReceptorIntensity.Neutral, new(Enabled: false, FlattestContrast), AxisFraction.MinValue, AxisFraction.MinValue);
}

public sealed record HableState(
    ShoulderCoefficient A, LinearCoefficient B, LinearAngle C, ToeCoefficient D, ToeNumerator E, ToeDenominator F, Gated<HableWhite> White) {
    public static HableState Default { get; } = new(
        ShoulderCoefficient.Published, LinearCoefficient.Published, LinearAngle.Published,
        ToeCoefficient.Published, ToeNumerator.Published, ToeDenominator.Published, new(Enabled: true, HableWhite.Published));
}

public readonly record struct SigmoidCurve(float WhiteTarget, float PaperExposure, float FilmFog, float ContrastPower, float SkewPower) : IToneCurve {
    public float Map(float value) =>
        WhiteTarget * MathF.Pow(1f + (PaperExposure * MathF.Pow(FilmFog + float.MaxNumber(value, 0f), -ContrastPower)), -SkewPower);

    public static SigmoidCurve Of(SigmoidState state, Display display) {
        double gray = Exposure.MiddleGray;
        double peak = (float)display.Peak / (float)Nits.ReferenceWhite;
        double skew = Math.Pow(5d, -(float)state.Skew);
        double contrast = (float)state.Contrast * (1d - gray) / (skew * (1d - Math.Pow(gray / peak, 1d / skew)));
        double towardGray = Math.Pow(peak / gray, 1d / skew) - 1d;
        double black = Math.Pow((float)state.Black, 1d / skew);
        double lifted = Math.Pow(towardGray * black, 1d / contrast);
        double fog = gray * lifted / (Math.Pow(1d - black, 1d / contrast) - lifted);
        return new((float)peak, (float)(Math.Pow(fog + gray, contrast) * towardGray), (float)fog, (float)contrast, (float)skew);
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
        double white = toeX + (1d - toeY) + double.Exp2(strength) - 1d;
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
        foreach (ref Vector4 pixel in row) {
            double average = double.Max(((double)pixel.X + pixel.Y + pixel.Z) / 3d, 0d);
            double least = float.Min(pixel.X, float.Min(pixel.Y, pixel.Z));
            double amount = least < 0d ? -least / (average - least) : 0d;
            pixel = new Vector4((float)double.Lerp(pixel.X, average, amount), (float)double.Lerp(pixel.Y, average, amount), (float)double.Lerp(pixel.Z, average, amount), pixel.W);
        }
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

    public static Vector3 PreserveHue(Vector3 input, Vector3 mapped, AxisFraction preserve) {
        (int min, int mid, int max) = input.X >= input.Y
            ? input.Y > input.Z ? (2, 1, 0) : input.Z > input.X ? (1, 0, 2) : input.Z > input.Y ? (1, 2, 0) : (2, 1, 0)
            : input.X >= input.Z ? (2, 0, 1) : input.Z > input.Y ? (0, 1, 2) : (0, 2, 1);
        float hue = preserve;
        float chroma = input[max] - input[min];
        float midscale = chroma is not 0f ? (input[mid] - input[min]) / chroma : 0f;
        float naive = ((1f - hue) * mapped[mid]) + (hue * (mapped[min] + ((mapped[max] - mapped[min]) * midscale)));
        float blend = input[min] + input[mid] is not 0f ? 2f * input[min] / (input[min] + input[mid]) : 0f;
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
        Whitened(context, state.Logarithmic.White.Active, white => Applied(state, context, LogarithmicCurve.Of(state.Logarithmic, context.Display, white)));

    internal static PixelPass Photoreceptor(ToneMapping state, PassContext context) =>
        Measured(context, (frame, samples) => Curved(context, context.Working, Seq(Adapted(state.Photoreceptor, context.Working.Luminance, frame, samples.LogStatistics))));

    internal static PixelPass Reinhard(ToneMapping _, PassContext context) =>
        ToneMapped(context, ToneMapOperator.Reinhard, new ToneMapParameters());

    internal static PixelPass ReinhardExtended(ToneMapping state, PassContext context) =>
        Whitened(context, state.ReinhardWhite.Active, white => ToneMapped(context, ToneMapOperator.ReinhardExtended, new ToneMapParameters(whitePoint: white)));

    internal static PixelPass Hable(ToneMapping state, PassContext context) =>
        Whitened(context, state.Hable.White.Active, white =>
            ToneMapped(context, ToneMapOperator.Hable, new ToneMapParameters(a: state.Hable.A, b: state.Hable.B, c: state.Hable.C, d: state.Hable.D, e: state.Hable.E, f: state.Hable.F, w: white)));

    internal static PixelPass PbrNeutral(ToneMapping _, PassContext context) =>
        Curved(context, Gamut.StandardRgb, Seq(Neutral));

    private static PixelPass.Color Applied<TCurve>(ToneMapping state, PassContext context, TCurve curve) where TCurve : struct, IToneCurve {
        (CurveApplication application, Gamut basis) = (state.Application, state.Application.BasePrimaries.IfNone(context.Working));
        return Curved(context, basis, Seq(
            new LutTable.Affine(Gamut.Between(Adjusted(basis, application, AxisFraction.MaxValue), basis.Configuration)).Apply,
            row => {
                foreach (ref Vector4 pixel in row)
                    pixel = new Vector4(PreserveHue(pixel.AsVector3(), new Vector3(curve.Map(pixel.X), curve.Map(pixel.Y), curve.Map(pixel.Z)), application.PreserveHue), pixel.W);
            },
            new LutTable.Affine(Gamut.Between(basis.Configuration, Adjusted(basis, application, application.Purity))).Apply));
    }

    private static PixelPass.Color Curved(PassContext context, Gamut basis, Seq<Action<Span<Vector4>>> curve) =>
        Formation.Formed(context, basis, Desaturated.Cons(curve), new ColorEncoding(basis, TransferCurve.Linear, Nits.ReferenceWhite));

    private static PixelPass Whitened<TWhite>(PassContext context, Option<TWhite> white, Func<float, PixelPass> formed) where TWhite : IPresented<TWhite, float> =>
        white.Match(
            Some: manual => formed(float.Exp2(manual.ToValue())),
            None: () => Measured(context, (_, samples) =>
                ValidationResults.Validated<TWhite, float>((float)double.Log2(samples.FrameWhite)).MapFail(error => Error.New(string.Create(CultureInfo.InvariantCulture, $"frame white {samples.FrameWhite} lies outside white point range"), error)).Map(measured => formed(float.Exp2(measured.ToValue())))));

    private static PixelPass.Frame Measured(PassContext context, Func<PixelFrame, LuminanceSamples, Fin<PixelPass>> formed) =>
        new((frame, progress) =>
            LuminanceSamples.Gather(frame, context.Working.Luminance)
                .Bind(samples => formed(frame, samples))
                .Bind(pass => pass.Run(frame, progress)));

    private static PixelPass.Color ToneMapped(PassContext context, ToneMapOperator operation, ToneMapParameters parameters) =>
        Curved(context, context.Working, Seq<Action<Span<Vector4>>>(row => {
            Span<float> components = MemoryMarshal.Cast<Vector4, float>(row);
            ImageProcessing.ToneMap(components, components, 4, operation, parameters);
        }));

    private static Action<Span<Vector4>> Adapted(PhotoreceptorState state, (double X, double Y, double Z) weights, PixelFrame frame, (double Mean, double Maximum, double Minimum) logs) {
        (double red, double green, double blue, long count) = (0d, 0d, 0d, 0L);
        foreach (Vector4 pixel in frame.View.Span) {
            Vector4 sample = pixel;
            Desaturated(new Span<Vector4>(ref sample));
            (red, green, blue, count) = pixel.W > 0f && double.IsFinite(LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, weights))
                ? (red + sample.X, green + sample.Y, blue + sample.Z, count + 1L) : (red, green, blue, count);
        }
        (double r, double g, double b) = (red / count, green / count, blue / count);
        double mean = LuminanceSamples.Luminance(r, g, b, weights);
        double intensity = double.Exp(-(float)state.Intensity);
        double contrast = state.Contrast.Active.Match(
            Some: static manual => (double)(float)manual,
            None: () => logs.Maximum == logs.Minimum ? 1d : PhotoreceptorState.FlattestContrast + ((1d - PhotoreceptorState.FlattestContrast) * double.Pow((logs.Maximum - logs.Mean) / (logs.Maximum - logs.Minimum), 1.4d)));
        (double light, double chromatic) = ((float)state.LightAdaptation, (float)state.ChromaticAdaptation);
        (double globalR, double globalG, double globalB) = (double.Lerp(mean, r, chromatic), double.Lerp(mean, g, chromatic), double.Lerp(mean, b, chromatic));
        return row => {
            foreach (ref Vector4 pixel in row) {
                double luminance = LuminanceSamples.Luminance(pixel.X, pixel.Y, pixel.Z, weights);
                pixel = new Vector4(Mapped(pixel.X, globalR, luminance), Mapped(pixel.Y, globalG, luminance), Mapped(pixel.Z, globalB, luminance), pixel.W);
            }
        };

        float Mapped(double channel, double average, double luminance) =>
            (channel + double.Pow(intensity * double.Lerp(average, double.Lerp(luminance, channel, chromatic), light), contrast)) switch {
                0d => 0f,
                var total => (float)(channel / total),
            };
    }

    private static Configuration Adjusted(Gamut basis, CurveApplication application, AxisFraction share) =>
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
