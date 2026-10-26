using System.Numerics;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.UI;
using TinyEXR;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class AgXLook {
    public static readonly AgXLook Punchy = new("punchy", static _ => Chained([
        .. (GradingStyle.Log.Neutral with { Shadows = new GradingRgbmsw(0.2f, 0.2f, 0.2f, 0.35f, 0.4f, 0.3f) }).Graded(GradingStyle.Log).ToSeq(),
        new LutTable.Cdl(CdlStyle.FwdNoClamp, Vector3.One, Vector3.Zero, new Vector3(1.0912f), 1f).Apply]));
    public static readonly AgXLook Greyscale = new("greyscale", static space => Chained([new LutTable.Antilog(space.Allocation).Apply, AgX.Grey.Apply, new LutTable.Log(space.Allocation).Apply]));
    public static readonly AgXLook VeryHighContrast = new("very-high-contrast", Contrasted(1.57f, 0.9f));
    public static readonly AgXLook HighContrast = new("high-contrast", Contrasted(1.4f, 0.95f));
    public static readonly AgXLook MediumHighContrast = new("medium-high-contrast", Contrasted(1.2f, 1f));
    public static readonly AgXLook BaseContrast = new("base-contrast", Contrasted(1f, 1f));
    public static readonly AgXLook MediumLowContrast = new("medium-low-contrast", Contrasted(0.9f, 1.05f));
    public static readonly AgXLook LowContrast = new("low-contrast", Contrasted(0.8f, 1.1f));
    public static readonly AgXLook VeryLowContrast = new("very-low-contrast", Contrasted(0.7f, 1.15f));

    [UseDelegateFromConstructor]
    public partial Action<Span<Vector4>> Graded(LogSpace space);

    private static Func<LogSpace, Action<Span<Vector4>>> Contrasted(float contrast, float saturation) =>
        _ => new LutTable.Cdl(CdlStyle.FwdNoClamp, new Vector3(contrast), new Vector3(0.4f * (1f - contrast)), Vector3.One, saturation).Apply;

    private static Action<Span<Vector4>> Chained(Seq<Action<Span<Vector4>>> steps) => (Action<Span<Vector4>>)Delegate.Combine([.. steps])!;
}

[SmartEnum<string>]
public sealed partial class FilmicLook {
    public static readonly FilmicLook VeryHighContrast = new("very-high-contrast", Some("filmic_to_1.20_1-00.spi1d"));
    public static readonly FilmicLook HighContrast = new("high-contrast", Some("filmic_to_0.99_1-0075.spi1d"));
    public static readonly FilmicLook MediumHighContrast = new("medium-high-contrast", Some("filmic_to_0-85_1-011.spi1d"));
    public static readonly FilmicLook MediumContrast = new("medium-contrast", Some("filmic_to_0-70_1-03.spi1d"));
    public static readonly FilmicLook MediumLowContrast = new("medium-low-contrast", Some("filmic_to_0-60_1-04.spi1d"));
    public static readonly FilmicLook LowContrast = new("low-contrast", Some("filmic_to_0-48_1-09.spi1d"));
    public static readonly FilmicLook VeryLowContrast = new("very-low-contrast", Some("filmic_to_0-35_1-30.spi1d"));
    public static readonly FilmicLook Log = new("log", None);

    private readonly Option<string> resource;

    public Option<Lut1D> Curve => resource.Map(FormationTables.Curve);
}

[SmartEnum]
public sealed partial class AgXBase {
    public static readonly AgXBase Rec1886 = new("AgX_Base_sRGB.cube", new ColorEncoding(Gamut.StandardRgb, Transfer.Gamma.Rec1886));
    public static readonly AgXBase DisplayP3 = new("AgX_Base_P3.cube", new ColorEncoding(Gamut.DisplayP3, Transfer.Gamma.Rec1886));
    public static readonly AgXBase Rec2020 = new("AgX_Base_Rec2020.cube", new ColorEncoding(Gamut.Rec2020, Transfer.Gamma.Rec1886));
    public static readonly AgXBase Rec2100Hlg = new("AgX_Rec2100-HLG_p3_lim.cube", new ColorEncoding(Gamut.Rec2020, Transfer.Hlg.Standard));

    private readonly string resource;

    public ColorEncoding Signal { get; }

    public LutTable Table => new LutTable.Cube(FormationTables.Lattice(resource), LutInterpolation.Tetrahedral);

    public static AgXBase For(DisplayDevice device) =>
        device.Map(
            srgb: Rec1886, displayP3: DisplayP3, rec1886: Rec1886, rec2020: Rec2020,
            rec2100PqHdr: Rec2100Hlg, rec2100PqSdr: DisplayP3, rec2100HlgHdr: Rec2100Hlg, rec2100HlgSdr: DisplayP3);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DisplayPeak : IPresented<DisplayPeak, float> {
    public static DisplayPeak MinValue { get; } = new((float)DisplayDevice.Srgb.Peak.Nits.ToDouble());
    public static DisplayPeak MaxValue { get; } = new(10_000f);
    public static DisplayPeak High { get; } = new((float)DisplayDevice.Rec2100PqHdr.Peak.Nits.ToDouble());
    public static Presentation<float> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(LuminanceUnit.Nit), Step = 1f, Decimals = 0 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct KneeOffset : IPresented<KneeOffset, float> {
    public static KneeOffset MinValue { get; } = new(Peak.Bt2390);
    public static KneeOffset MaxValue { get; } = new(2f);
    public static Presentation<float> Presentation { get; } = new() { Step = 0.01f, Decimals = 2 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct FilmSpan : IPresented<FilmSpan, float> {
    public static FilmSpan MinValue { get; } = new(float.BitIncrement(0f));
    public static FilmSpan MaxValue { get; } = new(64f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CurveFraction : IPresented<CurveFraction, float> {
    public static CurveFraction MinValue { get; } = new(float.BitIncrement(0f));
    public static CurveFraction MaxValue { get; } = new(float.BitDecrement(1f));
    public static Presentation<float> Presentation => Fraction.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DyeDensity : IPresented<DyeDensity, float> {
    public static DyeDensity MinValue { get; } = new(float.BitIncrement(0f));
    public static DyeDensity MaxValue { get; } = new(149f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HueRotation : IPresented<HueRotation, float> {
    public static HueRotation MinValue { get; } = new(float.BitIncrement(-float.Pi / 6f));
    public static HueRotation MaxValue { get; } = new(float.BitDecrement(float.Pi / 6f));
    public static Presentation<float> Presentation => Presentations.Angle;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct BlackOffset : IPresented<BlackOffset, float> {
    public static BlackOffset MinValue { get; } = new(-float.MaxValue);
    public static BlackOffset MaxValue { get; } = new(999f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (-10f, 10f), Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TargetBlack : IPresented<TargetBlack, float> {
    public static TargetBlack MinValue { get; } = new(0f);
    public static TargetBlack MaxValue { get; } = new(0.15f);
    public static Presentation<float> Presentation { get; } = Fraction.Presentation with { Soft = (0f, 0.01f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShoulderStrength : IPresented<ShoulderStrength, float> {
    public static ShoulderStrength MinValue { get; } = new(0f);
    public static ShoulderStrength MaxValue { get; } = new(16f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShoulderLength : IPresented<ShoulderLength, float> {
    public static ShoulderLength MinValue { get; } = new(1e-5f);
    public static ShoulderLength MaxValue { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CurveGamma : IPresented<CurveGamma, float> {
    public static CurveGamma MinValue { get; } = new(0.1f);
    public static CurveGamma MaxValue { get; } = new(10f);
    public static CurveGamma Linear { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (0.7f, 3f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DragoBias : IPresented<DragoBias, float> {
    public static DragoBias MinValue { get; } = new(0.5f);
    public static DragoBias MaxValue { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Soft = (0.7f, 0.9f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ReceptorIntensity : IPresented<ReceptorIntensity, float> {
    public static ReceptorIntensity MinValue { get; } = new(-float.Log(float.MaxValue));
    public static ReceptorIntensity MaxValue { get; } = new(float.Log(float.MaxValue));
    public static Presentation<float> Presentation { get; } = new() { Soft = (-8f, 8f), Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ShoulderCoefficient : IPresented<ShoulderCoefficient, float> {
    public static ShoulderCoefficient MinValue { get; } = new(0.001f);
    public static ShoulderCoefficient MaxValue { get; } = new(0.5f);
    public static ShoulderCoefficient Published { get; } = new(0.15f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LinearCoefficient : IPresented<LinearCoefficient, float> {
    public static LinearCoefficient MinValue { get; } = new(0f);
    public static LinearCoefficient MaxValue { get; } = new(0.6f);
    public static LinearCoefficient Published { get; } = new(0.5f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct LinearAngle : IPresented<LinearAngle, float> {
    public static LinearAngle MinValue { get; } = new(0.1f);
    public static LinearAngle MaxValue { get; } = new(0.3f);
    public static LinearAngle Published => MinValue;
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ToeCoefficient : IPresented<ToeCoefficient, float> {
    public static ToeCoefficient MinValue { get; } = new(0.001f);
    public static ToeCoefficient MaxValue { get; } = new(0.9f);
    public static ToeCoefficient Published { get; } = new(0.2f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ToeDenominator : IPresented<ToeDenominator, float> {
    public static ToeDenominator MinValue { get; } = new(0.2f);
    public static ToeDenominator MaxValue { get; } = new(0.6f);
    public static ToeDenominator Published { get; } = new(0.3f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ToeNumerator : IPresented<ToeNumerator, float> {
    public static ToeNumerator MinValue { get; } = new(0.0001f);
    public static ToeNumerator MaxValue { get; } = new(float.BitDecrement((float)LinearAngle.MinValue * (float)ToeDenominator.MinValue));
    public static ToeNumerator Published { get; } = new(0.02f);
    public static Presentation<float> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HableWhite : IPresented<HableWhite, float> {
    public static HableWhite MinValue { get; } = new(-10f);
    public static HableWhite MaxValue { get; } = new((float)Stops.MaxValue);
    public static HableWhite Published { get; } = new(float.Log2(11.2f));
    public static Presentation<float> Presentation => Stops.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PrimaryInset : IPresented<PrimaryInset, float> {
    public static PrimaryInset MinValue => Neutral;
    public static PrimaryInset MaxValue { get; } = new(0.99f);
    public static Presentation<float> Presentation { get; } = Fraction.Presentation with { Soft = (0f, 0.5f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PrimaryRotation : IPresented<PrimaryRotation, float> {
    public static PrimaryRotation MaxValue { get; } = new(float.BitDecrement((float)((double.Pi - (
        from rgb in toSeq(Primaries.Items).Map(static primaries => primaries.Gamut.Rgb)
        let white = rgb.WhitePoint.Chromaticity
        from side in Seq((From: rgb.ChromaticityR, To: rgb.ChromaticityG), (From: rgb.ChromaticityG, To: rgb.ChromaticityB), (From: rgb.ChromaticityB, To: rgb.ChromaticityR))
        select double.Ieee754Remainder(double.Atan2(side.To.Y - white.Y, side.To.X - white.X) - double.Atan2(side.From.Y - white.Y, side.From.X - white.X) - double.Pi, double.Tau) + double.Pi)
        .Fold(0d, double.Max)) / 2d)));
    public static PrimaryRotation MinValue { get; } = new(-MaxValue._value);
    public static Presentation<float> Presentation => Presentations.Angle;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record Meter(Fraction Low, Fraction High, Fraction Center, Position Start, Position End) : IStateRecord<Meter, MeterParameter> {
    public static Meter Default { get; } = new(Fraction.MinValue, Fraction.MaxValue, Fraction.MinValue, new(FrameAxis.Start, FrameAxis.Start), new(FrameAxis.End, FrameAxis.End));

    public Metering Reading => new(Low, High, Center, new FramePosition(Start.X, Start.Y), new FramePosition(End.X, End.Y));
}

[SmartEnum<string>]
public sealed partial class MeterParameter : IStateParameter<Meter> {
    public static readonly MeterParameter Low = new("low", StateParameter.Bounded(Lens<Meter, Fraction>.New(static meter => meter.Low, static low => meter => meter with { Low = low })),
        new("Meter low", "Lowest luminance rank the meter averages"));
    public static readonly MeterParameter High = new("high", StateParameter.Bounded(Lens<Meter, Fraction>.New(static meter => meter.High, static high => meter => meter with { High = high })),
        new("Meter high", "Highest luminance rank the meter averages"));
    public static readonly MeterParameter Center = new("center", StateParameter.Bounded(Lens<Meter, Fraction>.New(static meter => meter.Center, static center => meter => meter with { Center = center })),
        new("Center weight", "Weight cut outside central half"));
    public static readonly MeterParameter Start = new("start", new StateParameter<Meter>.Record<Position>(Lens<Meter, Position>.New(static meter => meter.Start, static start => meter => meter with { Start = start })),
        new("Region start", "Metered region upper left"));
    public static readonly MeterParameter End = new("end", new StateParameter<Meter>.Record<Position>(Lens<Meter, Position>.New(static meter => meter.End, static end => meter => meter with { End = end })),
        new("Region end", "Metered region lower right"));

    public StateParameter<Meter> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record SceneExposure(Stops Stops, Meter Meter) : IStateRecord<SceneExposure, SceneExposureParameter>, IPixelStage<SceneExposure> {
    public static SceneExposure Default { get; } = new(Stops.Zero, Meter.Default);

    public Fin<SceneExposure> Metered(PixelFrame frame, Gamut working) =>
        Meter.Reading.Metered(frame, working.Luminance).Bind(Stops.Validated).Map(metered => this with { Stops = metered });

    public static Option<PixelPass> Pass(SceneExposure state, PassContext context) =>
        state.Stops == Stops.Zero ? None : Some<PixelPass>(new PixelPass.Color(Exposure.Shifted(state.Stops)));

    public static PassContext Continued(SceneExposure state, Fraction mix, PassContext context) =>
        context with { Exposure = context.Exposure + double.Log2(double.Lerp(1d, state.Stops.Scale, mix)) };
}

[SmartEnum<string>]
public sealed partial class SceneExposureParameter : IStateParameter<SceneExposure> {
    public static readonly SceneExposureParameter Stops = new("exposure", StateParameter.Bounded(Lens<SceneExposure, Stops>.New(static state => state.Stops, static stops => state => state with { Stops = stops })),
        new("Exposure", "Scene gain in stops"));
    public static readonly SceneExposureParameter Meter = new("metering", new StateParameter<SceneExposure>.Record<Meter>(Lens<SceneExposure, Meter>.New(static state => state.Meter, static meter => state => state with { Meter = meter })),
        new("Metering", "Luminance ranks and region the meter reads"));

    public StateParameter<SceneExposure> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PeakMapping(DisplayPeak Target, Fraction Black, KneeOffset Knee) : IStateRecord<PeakMapping, PeakMappingParameter> {
    public static PeakMapping Default { get; } = new(DisplayPeak.High, Fraction.MinValue, KneeOffset.MinValue);

    public Option<Action<Span<Vector4>>> Rolled(Transfer.Pq signal, Luminance source) => Peak.Rolled(signal, source, Luminance.FromNits((double)Target), Black, Knee);
}

[SmartEnum<string>]
public sealed partial class PeakMappingParameter : IStateParameter<PeakMapping> {
    public static readonly PeakMappingParameter Target = new("target-peak", StateParameter.Bounded(Lens<PeakMapping, DisplayPeak>.New(static mapping => mapping.Target, static peak => mapping => mapping with { Target = peak })),
        new("Target peak", "Brightest display luminance highlights roll into"));
    public static readonly PeakMappingParameter Black = new("display-black", StateParameter.Bounded(Lens<PeakMapping, Fraction>.New(static mapping => mapping.Black, static black => mapping => mapping with { Black = black }),
        Fraction.Presentation with { Soft = (0f, 1e-3f), Decimals = 4 }),
        new("Display black", "Display black as share of peak"));
    public static readonly PeakMappingParameter Knee = new("knee-offset", StateParameter.Bounded(Lens<PeakMapping, KneeOffset>.New(static mapping => mapping.Knee, static knee => mapping => mapping with { Knee = knee })),
        new("Knee offset", "Distance below target peak where rolloff begins, BT.2390 knee at minimum"));

    public StateParameter<PeakMapping> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record CurveControls(Fraction PreserveHue, Fraction Purity, Option<Primaries> Basis, Channels<PrimaryInset> Inset, Channels<PrimaryRotation> Rotation)
    : IStateRecord<CurveControls, CurveControlsParameter> {
    public static CurveControls Default { get; } = new(Fraction.MaxValue, Fraction.MinValue, None, new(PrimaryInset.Neutral), new(PrimaryRotation.Neutral));

    public CurveApplication Application => new(PreserveHue, Purity, new Vector3(Inset.Red, Inset.Green, Inset.Blue), new Vector3(Rotation.Red, Rotation.Green, Rotation.Blue));

    public Action<Span<Vector4>> Applied<TCurve>(PassContext context, DisplayDevice device, TCurve curve) where TCurve : struct, IToneCurve =>
        ToneCurves.Applied(context.Working, device, Basis.Map(static basis => basis.Gamut).IfNone(context.Working), Application, curve);
}

[SmartEnum<string>]
public sealed partial class CurveControlsParameter : IStateParameter<CurveControls> {
    private static readonly Lens<CurveControls, Channels<PrimaryInset>> Insets = Lens<CurveControls, Channels<PrimaryInset>>.New(static controls => controls.Inset, static inset => controls => controls with { Inset = inset });
    private static readonly Lens<CurveControls, Channels<PrimaryRotation>> Rotations = Lens<CurveControls, Channels<PrimaryRotation>>.New(static controls => controls.Rotation, static rotation => controls => controls with { Rotation = rotation });

    public static readonly CurveControlsParameter PreserveHue = new("preserve-hue", StateParameter.Bounded(Lens<CurveControls, Fraction>.New(static controls => controls.PreserveHue, static preserve => controls => controls with { PreserveHue = preserve })),
        new("Preserve hue", "Share of pixel hue kept through curve"));
    public static readonly CurveControlsParameter Purity = new("purity", StateParameter.Bounded(Lens<CurveControls, Fraction>.New(static controls => controls.Purity, static purity => controls => controls with { Purity = purity })),
        new("Purity", "Share of primary inset restored after curve"));
    public static readonly CurveControlsParameter Basis = new("base-primaries", new StateParameter<CurveControls>.OptionalChoice<Primaries>(
        Lens<CurveControls, Option<Primaries>>.New(static controls => controls.Basis, static basis => controls => controls with { Basis = basis }), "Working"),
        new("Base primaries", "Gamut of rendering primaries"));
    public static readonly CurveControlsParameter RedInset = new("red-inset", StateParameter.Bounded(lens(Insets, Channels<PrimaryInset>.RedOf)), new("Red inset", "Red rendering primary pull to white"));
    public static readonly CurveControlsParameter GreenInset = new("green-inset", StateParameter.Bounded(lens(Insets, Channels<PrimaryInset>.GreenOf)), new("Green inset", "Green rendering primary pull to white"));
    public static readonly CurveControlsParameter BlueInset = new("blue-inset", StateParameter.Bounded(lens(Insets, Channels<PrimaryInset>.BlueOf)), new("Blue inset", "Blue rendering primary pull to white"));
    public static readonly CurveControlsParameter RedRotation = new("red-rotation", StateParameter.Bounded(lens(Rotations, Channels<PrimaryRotation>.RedOf)), new("Red rotation", "Red rendering primary turn about white"));
    public static readonly CurveControlsParameter GreenRotation = new("green-rotation", StateParameter.Bounded(lens(Rotations, Channels<PrimaryRotation>.GreenOf)), new("Green rotation", "Green rendering primary turn about white"));
    public static readonly CurveControlsParameter BlueRotation = new("blue-rotation", StateParameter.Bounded(lens(Rotations, Channels<PrimaryRotation>.BlueOf)), new("Blue rotation", "Blue rendering primary turn about white"));

    public StateParameter<CurveControls> Kind { get; }
    public ParameterText Text { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewTransform {
    public abstract PixelPass Formed(PassContext context, DisplayDevice device);

    private static PixelPass Whitened(PassContext context, Option<float> stops, Func<float, Action<Span<Vector4>>> formed) =>
        stops.Match(
            Some: chosen => (PixelPass)new PixelPass.Color(formed(float.Exp2(chosen))),
            None: () => new PixelPass.Frame((frame, progress) =>
                LuminanceSamples.Gather(frame, context.Working.Luminance).Bind(samples => new PixelPass.Color(formed((float)samples.FrameWhite)).Run(frame, progress))));

    public sealed record AgXView(Option<AgXLook> Look) : ViewTransform, IStateRecord<AgXView, AgXViewParameter> {
        public static AgXView Default { get; } = new(Option<AgXLook>.None);
        public static string Caption => "AgX";

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            AgXBase.For(device) switch {
                var basis => new PixelPass.Color(AgX.Formed(
                    context.Working, device, FormationTables.AgXLog, Look.Map(static look => look.Graded(FormationTables.AgXLog)), basis.Table, basis.Signal)),
            };
    }

    public sealed record AgXFalseColor(Option<AgXLook> Look) : ViewTransform, IStateRecord<AgXFalseColor, AgXFalseColorParameter> {
        public static AgXFalseColor Default { get; } = new(Option<AgXLook>.None);
        public static string Caption => "AgX false color";

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(FalseColor.Formed(
                context.Working, device, FormationTables.AgXLog, Look.Map(static look => look.Graded(FormationTables.AgXLog)), AgXBase.Rec2020.Table, FormationTables.FalseColorKey));
    }

    public sealed record AcesOutput : ViewTransform, IStateRecord<AcesOutput> {
        public static AcesOutput Default { get; } = new();
        public static string Caption => "ACES 2.0";

        public override PixelPass Formed(PassContext context, DisplayDevice device) => new PixelPass.Color(Aces.Formed(context.Working, device));
    }

    public sealed record FilmicView(FilmicLook Look) : ViewTransform, IStateRecord<FilmicView, FilmicViewParameter> {
        public static FilmicView Default { get; } = new(FilmicLook.MediumContrast);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(Filmic.Formed(context.Working, device, FormationTables.FilmicDesaturation, Look.Curve));
    }

    public sealed record FilmPrint(
        Stops PreExposure, Channels<Fraction> PreFilter, Channels<PrimaryScale> Scale, Channels<HueRotation> Rotation, Channels<Factor> Multiplier,
        Stops SigmoidMinimum, FilmSpan SigmoidSpan, CurveFraction ToeX, CurveFraction ToeY, CurveFraction ShoulderReachX, CurveFraction ShoulderReachY,
        Stops NegativeExposure, DyeDensity NegativeDensity, Channels<Factor> Backlight,
        Stops PrintExposure, DyeDensity PrintDensity, Gated<BlackOffset> BlackPoint, Channels<Fraction> PostFilter, Magnitude MidtoneSaturation)
        : ViewTransform, IStateRecord<FilmPrint, FilmPrintParameter> {
        public static FilmPrint Default { get; } = Of(FilmStock.Default).ThrowIfFail();

        public static FilmPrint Nostalgia { get; } = Default with {
            PreExposure = Stops.Create(5.563035f),
            Multiplier = Default.Multiplier with { Red = Factor.Create(1.1f), Blue = Factor.Create(1.2f) },
            SigmoidSpan = FilmSpan.Create(33f),
            NegativeExposure = Stops.Create(5.8f),
            Backlight = new(Factor.Create(0.99f), Factor.Create(1.1f), Factor.Create(1.035989f)),
            PrintDensity = DyeDensity.Create(40f),
            BlackPoint = new(Enabled: true, BlackOffset.Create(-5f)),
            MidtoneSaturation = Magnitude.Create(1.1f),
        };

        public static FilmPrint Silver { get; } = Default with {
            PreExposure = Stops.Create(3.9f),
            PreFilter = Default.PreFilter with { Red = Fraction.Create(0.95f), Green = Fraction.Create(0.975f) },
            Multiplier = Default.Multiplier with { Blue = Factor.Create(1.06f) },
            NegativeExposure = Stops.Create(4.7f),
            NegativeDensity = DyeDensity.Create(7f),
            Backlight = Default.Backlight with { Red = Factor.Create(0.9992f), Green = Factor.Create(0.99f) },
            PrintExposure = Stops.Create(4.7f),
            PrintDensity = DyeDensity.Create(30f),
            BlackPoint = new(Enabled: true, BlackOffset.Create(0.5f)),
            PostFilter = Default.PostFilter with { Blue = Fraction.Create(0.96f) },
            MidtoneSaturation = Magnitude.Unit,
        };

        public FilmStock Stock => new(
            PreExposure, Lanes(PreFilter), Lanes(Scale), Lanes(Rotation), Lanes(Multiplier), SigmoidMinimum, SigmoidSpan, ToeX, ToeY, ShoulderReachX, ShoulderReachY,
            NegativeExposure, NegativeDensity, Lanes(Backlight), PrintExposure, PrintDensity, BlackPoint.Active.Map(static offset => (float)offset), Lanes(PostFilter), MidtoneSaturation);

        public static Fin<FilmPrint> Of(FilmStock stock) =>
            ((Stops.Validated(stock.PreExposure), Channels<Fraction>.Validated(stock.PreFilter, Fraction.Validated), Channels<PrimaryScale>.Validated(stock.Scale, PrimaryScale.Validated),
                    Channels<HueRotation>.Validated(stock.Rotation, HueRotation.Validated), Channels<Factor>.Validated(stock.Multiplier, Factor.Validated))
                .Apply(static (preExposure, preFilter, scale, rotation, multiplier) => (preExposure, preFilter, scale, rotation, multiplier)).As(),
             (Stops.Validated(stock.SigmoidMinimum), FilmSpan.Validated(stock.SigmoidSpan), CurveFraction.Validated(stock.ToeX), CurveFraction.Validated(stock.ToeY),
                    CurveFraction.Validated(stock.ShoulderReachX), CurveFraction.Validated(stock.ShoulderReachY))
                .Apply(static (minimum, span, toeX, toeY, reachX, reachY) => (minimum, span, toeX, toeY, reachX, reachY)).As(),
             (Stops.Validated(stock.NegativeExposure), DyeDensity.Validated(stock.NegativeDensity), Channels<Factor>.Validated(stock.Backlight, Factor.Validated),
                    Stops.Validated(stock.PrintExposure), DyeDensity.Validated(stock.PrintDensity))
                .Apply(static (negativeExposure, negativeDensity, backlight, printExposure, printDensity) => (negativeExposure, negativeDensity, backlight, printExposure, printDensity)).As(),
             (stock.BlackPoint.Traverse(BlackOffset.Validated).As(), Channels<Fraction>.Validated(stock.PostFilter, Fraction.Validated), Magnitude.Validated(stock.MidtoneSaturation))
                .Apply(static (black, postFilter, saturation) => (black, postFilter, saturation)).As())
            .Apply(static (lens, curve, print, finish) => new FilmPrint(
                lens.preExposure, lens.preFilter, lens.scale, lens.rotation, lens.multiplier, curve.minimum, curve.span, curve.toeX, curve.toeY, curve.reachX, curve.reachY,
                print.negativeExposure, print.negativeDensity, print.backlight, print.printExposure, print.printDensity,
                finish.black.Match(Some: static offset => new Gated<BlackOffset>(Enabled: true, offset), None: static () => new Gated<BlackOffset>(Enabled: false, BlackOffset.Create(0.483f))),
                finish.postFilter, finish.saturation))
            .As();

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Frame((frame, progress) => context.Derivations.Derived(Stock, static stock => stock.Developed())
                .Bind(developed => new PixelPass.Color(FilmStock.Formed(context.Working, device, developed)).Run(frame, progress)));

        private static Vector3 Lanes<TValue>(Channels<TValue> channels) where TValue : notnull, IConvertible<float> =>
            new(channels.Red.ToValue(), channels.Green.ToValue(), channels.Blue.ToValue());
    }

    public sealed record TonyMcMapface : ViewTransform, IStateRecord<TonyMcMapface> {
        public static TonyMcMapface Default { get; } = new();
        public static string Caption => "Tony McMapface";

        public override PixelPass Formed(PassContext context, DisplayDevice device) => new PixelPass.Color(Tony.Formed(context.Working, device, FormationTables.TonyMcMapface));
    }

    public sealed record Sigmoid(CurveGamma Contrast, SignedFraction Skew, TargetBlack Black, CurveControls Controls) : ViewTransform, IStateRecord<Sigmoid, SigmoidParameter> {
        public static Sigmoid Default { get; } = new(CurveGamma.Create(1.5f), SignedFraction.Neutral, TargetBlack.Create(0.000152f), CurveControls.Default);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(Controls.Applied(context, device, SigmoidCurve.Of(Contrast, Skew, Black, device.Peak)));
    }

    public sealed record HablePiecewise(
        Fraction ToeStrength, Fraction ToeLength, Fraction ShoulderAngle, CurveGamma Gamma, ShoulderStrength ShoulderStrength, ShoulderLength ShoulderLength, CurveControls Controls)
        : ViewTransform, IStateRecord<HablePiecewise, HablePiecewiseParameter> {
        public static HablePiecewise Default { get; } = new(
            Fraction.Create(0.5f), Fraction.Create(0.5f), Fraction.MaxValue, CurveGamma.Linear, ShoulderStrength.Create(2f), ShoulderLength.Create(0.5f), CurveControls.Default);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(Controls.Applied(context, device, PiecewiseCurve.Of(ToeStrength, ToeLength, ShoulderAngle, Gamma, ShoulderStrength, ShoulderLength)));
    }

    public sealed record Logarithmic(DragoBias Bias, Gated<Stops> White, CurveControls Controls) : ViewTransform, IStateRecord<Logarithmic, LogarithmicParameter> {
        public static Logarithmic Default { get; } = new(DragoBias.Create(0.85f), new(Enabled: false, Stops.Zero), CurveControls.Default);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            Whitened(context, White.Active.Map(static white => (float)white), white => Controls.Applied(context, device, LogarithmicCurve.Of(Bias, white, device.Peak)));
    }

    public sealed record Photoreceptor(ReceptorIntensity Intensity, Gated<Factor> Contrast, Fraction LightAdaptation, Fraction ChromaticAdaptation)
        : ViewTransform, IStateRecord<Photoreceptor, PhotoreceptorParameter> {
        public static Photoreceptor Default { get; } = new(ReceptorIntensity.Neutral, new(Enabled: false, Factor.Create(0.3f)), Fraction.MinValue, Fraction.MinValue);

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Frame((frame, progress) =>
                ToneCurves.Photoreceptor(context.Working, device, frame, Intensity, Contrast.Active.Map(static contrast => (float)contrast), LightAdaptation, ChromaticAdaptation)
                    .Bind(adapted => new PixelPass.Color(adapted).Run(frame, progress)));
    }

    public sealed record Reinhard : ViewTransform, IStateRecord<Reinhard> {
        public static Reinhard Default { get; } = new();

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            new PixelPass.Color(ToneCurves.Mapped(context.Working, device, ToneMapOperator.Reinhard, new ToneMapParameters()));
    }

    public sealed record ReinhardExtended(Gated<Stops> White) : ViewTransform, IStateRecord<ReinhardExtended, ReinhardExtendedParameter> {
        public static ReinhardExtended Default { get; } = new(new Gated<Stops>(Enabled: false, Stops.Zero));

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            Whitened(context, White.Active.Map(static white => (float)white),
                white => ToneCurves.Mapped(context.Working, device, ToneMapOperator.ReinhardExtended, new ToneMapParameters(whitePoint: white)));
    }

    public sealed record Hable(
        ShoulderCoefficient A, LinearCoefficient B, LinearAngle C, ToeCoefficient D, ToeNumerator E, ToeDenominator F, Gated<HableWhite> White)
        : ViewTransform, IStateRecord<Hable, HableParameter> {
        public static Hable Default { get; } = new(
            ShoulderCoefficient.Published, LinearCoefficient.Published, LinearAngle.Published, ToeCoefficient.Published, ToeNumerator.Published, ToeDenominator.Published,
            new(Enabled: true, HableWhite.Published));

        public override PixelPass Formed(PassContext context, DisplayDevice device) =>
            Whitened(context, White.Active.Map(static white => (float)white),
                white => ToneCurves.Mapped(context.Working, device, ToneMapOperator.Hable, new ToneMapParameters(a: A, b: B, c: C, d: D, e: E, f: F, w: white)));
    }

    public sealed record PbrNeutral : ViewTransform, IStateRecord<PbrNeutral> {
        public static PbrNeutral Default { get; } = new();
        public static string Caption => "Khronos PBR Neutral";

        public override PixelPass Formed(PassContext context, DisplayDevice device) => new PixelPass.Color(ToneCurves.PbrNeutral(context.Working, device));
    }
}

[SmartEnum<string>]
public sealed partial class AgXViewParameter : IStateParameter<ViewTransform.AgXView> {
    public static readonly AgXViewParameter Look = new("look", new StateParameter<ViewTransform.AgXView>.OptionalChoice<AgXLook>(
        Lens<ViewTransform.AgXView, Option<AgXLook>>.New(static view => view.Look, static look => _ => new(look)), "None"),
        new("Look", "Grade in AgX Log before the AgX base"));

    public StateParameter<ViewTransform.AgXView> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class AgXFalseColorParameter : IStateParameter<ViewTransform.AgXFalseColor> {
    public static readonly AgXFalseColorParameter Look = new("look", new StateParameter<ViewTransform.AgXFalseColor>.OptionalChoice<AgXLook>(
        Lens<ViewTransform.AgXFalseColor, Option<AgXLook>>.New(static view => view.Look, static look => _ => new(look)), "None"),
        new("Look", "Grade in AgX Log before the false color bands"));

    public StateParameter<ViewTransform.AgXFalseColor> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class FilmicViewParameter : IStateParameter<ViewTransform.FilmicView> {
    public static readonly FilmicViewParameter Look = new("look", new StateParameter<ViewTransform.FilmicView>.Choice<FilmicLook>(
        Lens<ViewTransform.FilmicView, FilmicLook>.New(static view => view.Look, static look => _ => new(look))),
        new("Contrast", "Contrast look, log shows base log encoding"));

    public StateParameter<ViewTransform.FilmicView> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class FilmPrintParameter : IStateParameter<ViewTransform.FilmPrint> {
    private static readonly Lens<ViewTransform.FilmPrint, Channels<Fraction>> PreFilters = Lens<ViewTransform.FilmPrint, Channels<Fraction>>.New(static film => film.PreFilter, static value => film => film with { PreFilter = value });
    private static readonly Lens<ViewTransform.FilmPrint, Channels<PrimaryScale>> Scales = Lens<ViewTransform.FilmPrint, Channels<PrimaryScale>>.New(static film => film.Scale, static value => film => film with { Scale = value });
    private static readonly Lens<ViewTransform.FilmPrint, Channels<HueRotation>> Rotations = Lens<ViewTransform.FilmPrint, Channels<HueRotation>>.New(static film => film.Rotation, static value => film => film with { Rotation = value });
    private static readonly Lens<ViewTransform.FilmPrint, Channels<Factor>> Multipliers = Lens<ViewTransform.FilmPrint, Channels<Factor>>.New(static film => film.Multiplier, static value => film => film with { Multiplier = value });
    private static readonly Lens<ViewTransform.FilmPrint, Channels<Factor>> Backlights = Lens<ViewTransform.FilmPrint, Channels<Factor>>.New(static film => film.Backlight, static value => film => film with { Backlight = value });
    private static readonly Lens<ViewTransform.FilmPrint, Channels<Fraction>> PostFilters = Lens<ViewTransform.FilmPrint, Channels<Fraction>>.New(static film => film.PostFilter, static value => film => film with { PostFilter = value });
    private static readonly Presentation<float> Scaled = new() { Soft = (1f, 2f) };

    public static readonly FilmPrintParameter PreExposure = new("pre-exposure", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, Stops>.New(static film => film.PreExposure, static value => film => film with { PreExposure = value })),
        new("Pre-exposure", "Stops before film stock"));
    public static readonly FilmPrintParameter PreFilterRed = new("pre-filter-red", StateParameter.Bounded(lens(PreFilters, Channels<Fraction>.RedOf)), new("Pre-filter red", "Red transmittance of lens filter"));
    public static readonly FilmPrintParameter PreFilterGreen = new("pre-filter-green", StateParameter.Bounded(lens(PreFilters, Channels<Fraction>.GreenOf)), new("Pre-filter green", "Green transmittance of lens filter"));
    public static readonly FilmPrintParameter PreFilterBlue = new("pre-filter-blue", StateParameter.Bounded(lens(PreFilters, Channels<Fraction>.BlueOf)), new("Pre-filter blue", "Blue transmittance of lens filter"));
    public static readonly FilmPrintParameter RedScale = new("red-scale", StateParameter.Bounded(lens(Scales, Channels<PrimaryScale>.RedOf), Scaled), new("Red scale", "Red primary gamut extension"));
    public static readonly FilmPrintParameter GreenScale = new("green-scale", StateParameter.Bounded(lens(Scales, Channels<PrimaryScale>.GreenOf), Scaled), new("Green scale", "Green primary gamut extension"));
    public static readonly FilmPrintParameter BlueScale = new("blue-scale", StateParameter.Bounded(lens(Scales, Channels<PrimaryScale>.BlueOf), Scaled), new("Blue scale", "Blue primary gamut extension"));
    public static readonly FilmPrintParameter RedRotation = new("red-rotation", StateParameter.Bounded(lens(Rotations, Channels<HueRotation>.RedOf)), new("Red rotation", "Red primary hue turn"));
    public static readonly FilmPrintParameter GreenRotation = new("green-rotation", StateParameter.Bounded(lens(Rotations, Channels<HueRotation>.GreenOf)), new("Green rotation", "Green primary hue turn"));
    public static readonly FilmPrintParameter BlueRotation = new("blue-rotation", StateParameter.Bounded(lens(Rotations, Channels<HueRotation>.BlueOf)), new("Blue rotation", "Blue primary hue turn"));
    public static readonly FilmPrintParameter RedMultiplier = new("red-multiplier", StateParameter.Bounded(lens(Multipliers, Channels<Factor>.RedOf)), new("Red multiplier", "Red primary gain"));
    public static readonly FilmPrintParameter GreenMultiplier = new("green-multiplier", StateParameter.Bounded(lens(Multipliers, Channels<Factor>.GreenOf)), new("Green multiplier", "Green primary gain"));
    public static readonly FilmPrintParameter BlueMultiplier = new("blue-multiplier", StateParameter.Bounded(lens(Multipliers, Channels<Factor>.BlueOf)), new("Blue multiplier", "Blue primary gain"));
    public static readonly FilmPrintParameter SigmoidMinimum = new("sigmoid-minimum", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, Stops>.New(static film => film.SigmoidMinimum, static value => film => film with { SigmoidMinimum = value })),
        new("Sigmoid minimum", "Lowest stop dye response reads"));
    public static readonly FilmPrintParameter SigmoidSpan = new("sigmoid-span", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, FilmSpan>.New(static film => film.SigmoidSpan, static value => film => film with { SigmoidSpan = value })),
        new("Sigmoid span", "Stops dye response reads above minimum"));
    public static readonly FilmPrintParameter ToeX = new("toe-x", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, CurveFraction>.New(static film => film.ToeX, static value => film => film with { ToeX = value })),
        new("Toe X", "Toe input"));
    public static readonly FilmPrintParameter ToeY = new("toe-y", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, CurveFraction>.New(static film => film.ToeY, static value => film => film with { ToeY = value })),
        new("Toe Y", "Toe output"));
    public static readonly FilmPrintParameter ShoulderReachX = new("shoulder-reach-x", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, CurveFraction>.New(static film => film.ShoulderReachX, static value => film => film with { ShoulderReachX = value })),
        new("Shoulder reach X", "Share of input above toe where shoulder starts"));
    public static readonly FilmPrintParameter ShoulderReachY = new("shoulder-reach-y", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, CurveFraction>.New(static film => film.ShoulderReachY, static value => film => film with { ShoulderReachY = value })),
        new("Shoulder reach Y", "Share of output above toe where shoulder starts"));
    public static readonly FilmPrintParameter NegativeExposure = new("negative-exposure", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, Stops>.New(static film => film.NegativeExposure, static value => film => film with { NegativeExposure = value })),
        new("Negative exposure", "Light on negative in stops"));
    public static readonly FilmPrintParameter NegativeDensity = new("negative-density", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, DyeDensity>.New(static film => film.NegativeDensity, static value => film => film with { NegativeDensity = value })),
        new("Negative density", "Negative maximum dye density"));
    public static readonly FilmPrintParameter BacklightRed = new("backlight-red", StateParameter.Bounded(lens(Backlights, Channels<Factor>.RedOf)), new("Backlight red", "Red of light printing negative"));
    public static readonly FilmPrintParameter BacklightGreen = new("backlight-green", StateParameter.Bounded(lens(Backlights, Channels<Factor>.GreenOf)), new("Backlight green", "Green of light printing negative"));
    public static readonly FilmPrintParameter BacklightBlue = new("backlight-blue", StateParameter.Bounded(lens(Backlights, Channels<Factor>.BlueOf)), new("Backlight blue", "Blue of light printing negative"));
    public static readonly FilmPrintParameter PrintExposure = new("print-exposure", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, Stops>.New(static film => film.PrintExposure, static value => film => film with { PrintExposure = value })),
        new("Print exposure", "Backlight on print in stops"));
    public static readonly FilmPrintParameter PrintDensity = new("print-density", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, DyeDensity>.New(static film => film.PrintDensity, static value => film => film with { PrintDensity = value })),
        new("Print density", "Print maximum dye density"));
    public static readonly FilmPrintParameter BlackPoint = new("black-point", StateParameter.OptionalBounded(Lens<ViewTransform.FilmPrint, Gated<BlackOffset>>.New(static film => film.BlackPoint, static value => film => film with { BlackPoint = value })),
        new("Black point", "Film black in per mille, stock black when off"));
    public static readonly FilmPrintParameter PostFilterRed = new("post-filter-red", StateParameter.Bounded(lens(PostFilters, Channels<Fraction>.RedOf)), new("Post-filter red", "Red transmittance of print filter"));
    public static readonly FilmPrintParameter PostFilterGreen = new("post-filter-green", StateParameter.Bounded(lens(PostFilters, Channels<Fraction>.GreenOf)), new("Post-filter green", "Green transmittance of print filter"));
    public static readonly FilmPrintParameter PostFilterBlue = new("post-filter-blue", StateParameter.Bounded(lens(PostFilters, Channels<Fraction>.BlueOf)), new("Post-filter blue", "Blue transmittance of print filter"));
    public static readonly FilmPrintParameter MidtoneSaturation = new("midtone-saturation", StateParameter.Bounded(Lens<ViewTransform.FilmPrint, Magnitude>.New(static film => film.MidtoneSaturation, static value => film => film with { MidtoneSaturation = value }),
        Magnitude.Presentation with { Soft = (0f, 2f), Origin = 1f }),
        new("Midtone saturation", "Saturation factor at middle gray"));

    public StateParameter<ViewTransform.FilmPrint> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class SigmoidParameter : IStateParameter<ViewTransform.Sigmoid> {
    public static readonly SigmoidParameter Contrast = new("contrast", StateParameter.Bounded(Lens<ViewTransform.Sigmoid, CurveGamma>.New(static sigmoid => sigmoid.Contrast, static value => sigmoid => sigmoid with { Contrast = value })),
        new("Contrast", "Middle gray slope over stops"));
    public static readonly SigmoidParameter Skew = new("skew", StateParameter.Bounded(Lens<ViewTransform.Sigmoid, SignedFraction>.New(static sigmoid => sigmoid.Skew, static value => sigmoid => sigmoid with { Skew = value })),
        new("Skew", "Contrast shift to shadows or highlights"));
    public static readonly SigmoidParameter Black = new("target-black", StateParameter.Bounded(Lens<ViewTransform.Sigmoid, TargetBlack>.New(static sigmoid => sigmoid.Black, static value => sigmoid => sigmoid with { Black = value })),
        new("Target black", "Display black as share of white"));
    public static readonly SigmoidParameter Controls = new("controls", new StateParameter<ViewTransform.Sigmoid>.Record<CurveControls>(
        Lens<ViewTransform.Sigmoid, CurveControls>.New(static sigmoid => sigmoid.Controls, static value => sigmoid => sigmoid with { Controls = value })),
        new("Curve controls", "Hue, purity and rendering primaries around the curve"));

    public StateParameter<ViewTransform.Sigmoid> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class HablePiecewiseParameter : IStateParameter<ViewTransform.HablePiecewise> {
    public static readonly HablePiecewiseParameter ToeStrength = new("toe-strength", StateParameter.Bounded(Lens<ViewTransform.HablePiecewise, Fraction>.New(static curve => curve.ToeStrength, static value => curve => curve with { ToeStrength = value })),
        new("Toe strength", "Toe depth below straight section"));
    public static readonly HablePiecewiseParameter ToeLength = new("toe-length", StateParameter.Bounded(Lens<ViewTransform.HablePiecewise, Fraction>.New(static curve => curve.ToeLength, static value => curve => curve with { ToeLength = value })),
        new("Toe length", "Toe span along input"));
    public static readonly HablePiecewiseParameter ShoulderAngle = new("shoulder-angle", StateParameter.Bounded(Lens<ViewTransform.HablePiecewise, Fraction>.New(static curve => curve.ShoulderAngle, static value => curve => curve with { ShoulderAngle = value })),
        new("Shoulder angle", "Shoulder roll past white"));
    public static readonly HablePiecewiseParameter Gamma = new("gamma", StateParameter.Bounded(Lens<ViewTransform.HablePiecewise, CurveGamma>.New(static curve => curve.Gamma, static value => curve => curve with { Gamma = value })),
        new("Gamma", "Straight section power"));
    public static readonly HablePiecewiseParameter ShoulderStrength = new("shoulder-strength", StateParameter.Bounded(Lens<ViewTransform.HablePiecewise, ShoulderStrength>.New(static curve => curve.ShoulderStrength, static value => curve => curve with { ShoulderStrength = value })),
        new("Shoulder strength", "Highlight stops in shoulder past straight section"));
    public static readonly HablePiecewiseParameter ShoulderLength = new("shoulder-length", StateParameter.Bounded(Lens<ViewTransform.HablePiecewise, ShoulderLength>.New(static curve => curve.ShoulderLength, static value => curve => curve with { ShoulderLength = value })),
        new("Shoulder length", "Shoulder span along output"));
    public static readonly HablePiecewiseParameter Controls = new("controls", new StateParameter<ViewTransform.HablePiecewise>.Record<CurveControls>(
        Lens<ViewTransform.HablePiecewise, CurveControls>.New(static curve => curve.Controls, static value => curve => curve with { Controls = value })),
        new("Curve controls", "Hue, purity and rendering primaries around the curve"));

    public StateParameter<ViewTransform.HablePiecewise> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class LogarithmicParameter : IStateParameter<ViewTransform.Logarithmic> {
    public static readonly LogarithmicParameter Bias = new("bias", StateParameter.Bounded(Lens<ViewTransform.Logarithmic, DragoBias>.New(static curve => curve.Bias, static value => curve => curve with { Bias = value })),
        new("Bias", "Drago bias, lower values lift shadows"));
    public static readonly LogarithmicParameter White = new("white", StateParameter.OptionalBounded(Lens<ViewTransform.Logarithmic, Gated<Stops>>.New(static curve => curve.White, static value => curve => curve with { White = value })),
        new("White point", "Scene stops at display white, frame white when off"));
    public static readonly LogarithmicParameter Controls = new("controls", new StateParameter<ViewTransform.Logarithmic>.Record<CurveControls>(
        Lens<ViewTransform.Logarithmic, CurveControls>.New(static curve => curve.Controls, static value => curve => curve with { Controls = value })),
        new("Curve controls", "Hue, purity and rendering primaries around the curve"));

    public StateParameter<ViewTransform.Logarithmic> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class PhotoreceptorParameter : IStateParameter<ViewTransform.Photoreceptor> {
    public static readonly PhotoreceptorParameter Intensity = new("intensity", StateParameter.Bounded(Lens<ViewTransform.Photoreceptor, ReceptorIntensity>.New(static receptor => receptor.Intensity, static value => receptor => receptor with { Intensity = value })),
        new("Intensity", "Response brightness"));
    public static readonly PhotoreceptorParameter Contrast = new("contrast", StateParameter.OptionalBounded(Lens<ViewTransform.Photoreceptor, Gated<Factor>>.New(static receptor => receptor.Contrast, static value => receptor => receptor with { Contrast = value }),
        Factor.Presentation with { Soft = (0.3f, 1f) }),
        new("Contrast", "Response slope, read from frame when off"));
    public static readonly PhotoreceptorParameter LightAdaptation = new("light-adaptation", StateParameter.Bounded(Lens<ViewTransform.Photoreceptor, Fraction>.New(static receptor => receptor.LightAdaptation, static value => receptor => receptor with { LightAdaptation = value })),
        new("Adaptation", "Pixel level share in adaptation level"));
    public static readonly PhotoreceptorParameter ChromaticAdaptation = new("chromatic-adaptation", StateParameter.Bounded(Lens<ViewTransform.Photoreceptor, Fraction>.New(static receptor => receptor.ChromaticAdaptation, static value => receptor => receptor with { ChromaticAdaptation = value })),
        new("Color correction", "Channel level share in adaptation level"));

    public StateParameter<ViewTransform.Photoreceptor> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class ReinhardExtendedParameter : IStateParameter<ViewTransform.ReinhardExtended> {
    public static readonly ReinhardExtendedParameter White = new("white", StateParameter.OptionalBounded(Lens<ViewTransform.ReinhardExtended, Gated<Stops>>.New(static curve => curve.White, static value => _ => new(value))),
        new("White point", "Scene stops at display white, frame white when off"));

    public StateParameter<ViewTransform.ReinhardExtended> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class HableParameter : IStateParameter<ViewTransform.Hable> {
    public static readonly HableParameter A = new("a", StateParameter.Bounded(Lens<ViewTransform.Hable, ShoulderCoefficient>.New(static hable => hable.A, static value => hable => hable with { A = value })),
        new("Shoulder strength", "Hable's A"));
    public static readonly HableParameter B = new("b", StateParameter.Bounded(Lens<ViewTransform.Hable, LinearCoefficient>.New(static hable => hable.B, static value => hable => hable with { B = value })),
        new("Linear strength", "Hable's B"));
    public static readonly HableParameter C = new("c", StateParameter.Bounded(Lens<ViewTransform.Hable, LinearAngle>.New(static hable => hable.C, static value => hable => hable with { C = value })),
        new("Linear angle", "Hable's C"));
    public static readonly HableParameter D = new("d", StateParameter.Bounded(Lens<ViewTransform.Hable, ToeCoefficient>.New(static hable => hable.D, static value => hable => hable with { D = value })),
        new("Toe strength", "Hable's D"));
    public static readonly HableParameter E = new("e", StateParameter.Bounded(Lens<ViewTransform.Hable, ToeNumerator>.New(static hable => hable.E, static value => hable => hable with { E = value })),
        new("Toe numerator", "Hable's E"));
    public static readonly HableParameter F = new("f", StateParameter.Bounded(Lens<ViewTransform.Hable, ToeDenominator>.New(static hable => hable.F, static value => hable => hable with { F = value })),
        new("Toe denominator", "Hable's F"));
    public static readonly HableParameter White = new("white", StateParameter.OptionalBounded(Lens<ViewTransform.Hable, Gated<HableWhite>>.New(static hable => hable.White, static value => hable => hable with { White = value })),
        new("Linear white point", "Scene stops normalized to white, frame white when off"));

    public StateParameter<ViewTransform.Hable> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ImageFormation(ViewTransform View, DisplayDevice Device) : IStateRecord<ImageFormation, ImageFormationParameter>, IPixelStage<ImageFormation> {
    public static ImageFormation Default { get; } = new(ViewTransform.AgXView.Default, DisplayDevice.Srgb);

    public static Seq<(string Name, EffectLook<ImageFormation> Look)> Looks { get; } =
        toSeq(AgXLook.Items).Map(static look => Viewed($"{ViewTransform.AgXView.Caption} {Captions.Of(look)}", new ViewTransform.AgXView(Some(look))))
        + toSeq(FilmicLook.Items).Map(static look => Viewed($"{Captions.Of(typeof(ViewTransform.FilmicView))} {Captions.Of(look)}", new ViewTransform.FilmicView(look)))
        + Seq(Viewed(Captions.Of(nameof(ViewTransform.FilmPrint.Nostalgia)), ViewTransform.FilmPrint.Nostalgia), Viewed(Captions.Of(nameof(ViewTransform.FilmPrint.Silver)), ViewTransform.FilmPrint.Silver));

    public static Option<PixelPass> Pass(ImageFormation state, PassContext context) => Some(state.View.Formed(context, state.Device));

    public static PassContext Continued(ImageFormation state, Fraction mix, PassContext context) =>
        context with { Display = state.Device, Working = state.Device.Encoding.Gamut, Signal = state.Device.Encoding.Transfer };

    private static (string Name, EffectLook<ImageFormation> Look) Viewed(string name, ViewTransform view) =>
        (name, new EffectLook<ImageFormation>.Partial(Default with { View = view }, [ImageFormationParameter.View]));
}

[SmartEnum<string>]
public sealed partial class ImageFormationParameter : IStateParameter<ImageFormation> {
    public static readonly ImageFormationParameter View = new("view", new StateParameter<ImageFormation>.Variant<ViewTransform>(
        Lens<ImageFormation, ViewTransform>.New(static formation => formation.View, static view => formation => formation with { View = view })),
        new("View transform", "Scene light to display code formation"));
    public static readonly ImageFormationParameter Device = new("device", new StateParameter<ImageFormation>.Choice<DisplayDevice>(
        Lens<ImageFormation, DisplayDevice>.New(static formation => formation.Device, static device => formation => formation with { Device = device })),
        new("Display", "Display encoding and peak the view transform forms for"));

    public StateParameter<ImageFormation> Kind { get; }
    public ParameterText Text { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FormationTables {
    public static Func<string, Lut3D> Lattice { get; } = memo(static (string resource) =>
        Parsed(resource, LutFormat.Cube).Bind(Fin<Lut3D> (table) => table is LutTable.Cube { Lattice: var lattice } ? lattice : Error.New($"embedded table {resource} holds no 3D lattice")).ThrowIfFail());

    public static Func<string, Lut1D> Curve { get; } = memo(static (string resource) =>
        Parsed(resource, LutFormat.Spi1d).Bind(Fin<Lut1D> (table) => table is LutTable.ChannelCurve { Table: var curve } ? curve : Error.New($"embedded table {resource} holds no channel curve")).ThrowIfFail());

    private static readonly Lazy<LogSpace> AgXSpace = new(static () => LogSpace.AgX(Lattice("luminance_compensation_bt2020.cube")));
    private static readonly Lazy<Lut3D> TonyTable = new(static () => {
        using Stream stream = Opened("tony_mc_mapface.dds");
        using MemoryStream texture = new();
        stream.CopyTo(texture);
        return Tony.Table(texture.ToArray()).ThrowIfFail();
    });

    public static LogSpace AgXLog => AgXSpace.Value;
    public static Lut3D FilmicDesaturation => Lattice("filmic_desat_33.cube");
    public static Lut1D FalseColorKey => Curve("AgX_False_Color.spi1d");
    public static Lut3D TonyMcMapface => TonyTable.Value;

    private static Stream Opened(string resource) => typeof(FormationTables).Assembly.GetManifestResourceStream(resource)!;

    private static Fin<LutTable> Parsed(string resource, LutFormat format) {
        using StreamReader reader = new(Opened(resource));
        return LutFiles.Parse(reader.ReadToEnd(), format);
    }
}
