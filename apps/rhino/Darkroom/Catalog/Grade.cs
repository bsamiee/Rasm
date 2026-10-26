using System.Drawing;
using System.Numerics;
using Darkroom.Chain;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.UI;
using Rasm.UI.Styling;
using TinyEXR;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class Primaries : ICaptioned {
    public static readonly Primaries StandardRgb = new("standard-rgb", Gamut.StandardRgb);
    public static readonly Primaries DisplayP3 = new("display-p3", Gamut.DisplayP3);
    public static readonly Primaries Rec2020 = new("rec2020", Gamut.Rec2020);
    public static readonly Primaries Acescg = new("acescg", Gamut.Acescg);
    public static readonly Primaries Aces20651 = new("aces20651", Gamut.Aces20651);
    public static readonly Primaries ProPhoto = new("pro-photo", Gamut.ProPhoto);
    public static readonly Primaries EGamut = new("e-gamut", Gamut.EGamut);
    public static readonly Primaries ArriWideGamut3 = new("arri-wide-gamut-3", Gamut.ArriWideGamut3);
    public static readonly Primaries ArriWideGamut4 = new("arri-wide-gamut-4", Gamut.ArriWideGamut4);
    public static readonly Primaries SGamut3 = new("s-gamut3", Gamut.SGamut3);
    public static readonly Primaries SGamut3Cine = new("s-gamut3-cine", Gamut.SGamut3Cine);
    public static readonly Primaries VeniceSGamut3 = new("venice-s-gamut3", Gamut.VeniceSGamut3);
    public static readonly Primaries VeniceSGamut3Cine = new("venice-s-gamut3-cine", Gamut.VeniceSGamut3Cine);
    public static readonly Primaries CinemaGamut = new("cinema-gamut", Gamut.CinemaGamut);
    public static readonly Primaries RedWideGamutRgb = new("red-wide-gamut-rgb", Gamut.RedWideGamutRgb);
    public static readonly Primaries VGamut = new("v-gamut", Gamut.VGamut);
    public static readonly Primaries DGamut = new("d-gamut", Gamut.DGamut);
    public static readonly Primaries ProtuneNative = new("protune-native", Gamut.ProtuneNative);
    public static readonly Primaries BlackmagicWideGamut = new("blackmagic-wide-gamut", Gamut.BlackmagicWideGamut);
    public static readonly Primaries DaVinciWideGamut = new("davinci-wide-gamut", Gamut.DaVinciWideGamut);
    public static readonly Primaries AppleWideGamut = new("apple-wide-gamut", Gamut.AppleWideGamut);

    public Gamut Gamut { get; }

    public string Caption => Gamut.Rgb.Name;
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ContrastAmount : IPresented<ContrastAmount, float> {
    public static ContrastAmount MinValue { get; } = new(0.01f);
    public static ContrastAmount MaxValue { get; } = new(2f);
    public static ContrastAmount Neutral { get; } = new(1f);
    public static Presentation<float> Presentation { get; } = new() { Origin = 1f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "MiddleGray", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ContrastPivot : IPresented<ContrastPivot, float> {
    public static ContrastPivot MinValue { get; } = new((float)CurveAxis.Stops.Span.Low);
    public static ContrastPivot MaxValue { get; } = new((float)CurveAxis.Stops.Span.High);
    public static Presentation<float> Presentation { get; } = new() { Origin = 0f };

    public float Linear => Exposure.MiddleGray * float.Exp2(_value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Kelvin : IPresented<Kelvin, double> {
    public static Kelvin MinValue { get; } = new(800d);
    public static Kelvin MaxValue { get; } = new(100_000d);
    public static Kelvin Daylight { get; } = new(new Unicolour(Configuration.Default, Configuration.Default.Xyz.WhitePoint.Chromaticity).Temperature.Cct);
    public static Presentation<double> Presentation { get; } = Presentations.Kelvin with { Soft = (2000d, 11_000d), Step = 1d, Decimals = 0 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Locus", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Tint : IPresented<Tint, double> {
    public static Tint MinValue { get; } = new(-0.05d);
    public static Tint MaxValue { get; } = new(0.05d);
    public static Tint Daylight { get; } = new(new Unicolour(Configuration.Default, Configuration.Default.Xyz.WhitePoint.Chromaticity).Temperature.Duv);
    public static Presentation<double> Presentation { get; } = new() { Origin = 0d, Decimals = 4 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Origin", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct WheelHue : IPresented<WheelHue, float> {
    public static WheelHue MinValue => Origin;
    public static WheelHue MaxValue { get; } = new(float.BitDecrement(float.Tau));
    public static Presentation<float> Presentation => Presentations.Angle;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct WheelLuma : IPresented<WheelLuma, float> {
    public static WheelLuma MinValue { get; } = new(-3f);
    public static WheelLuma MaxValue { get; } = new(3f);
    public static Presentation<float> Presentation { get; } = new() { Origin = 0f };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ZoneAdjustment : IPresented<ZoneAdjustment, float> {
    public static ZoneAdjustment MinValue { get; } = new(0.1f);
    public static ZoneAdjustment MaxValue { get; } = new(1.9f);
    public static Presentation<float> Presentation { get; } = new() { Origin = 1f, Step = 0.01f, Decimals = 3 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PivotAdjustment : IPresented<PivotAdjustment, float> {
    public static PivotAdjustment MinValue { get; } = new(0.2f);
    public static PivotAdjustment MaxValue { get; } = new(1.8f);
    public static Presentation<float> Presentation => ZoneAdjustment.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ZoneWidth : IPresented<ZoneWidth, float> {
    public static ZoneWidth MinValue { get; } = new(0.01f);
    public static ZoneWidth MaxValue { get; } = new((float)Stops.MaxValue - (float)Stops.MinValue);
    public static Presentation<float> Presentation { get; } = GradingStyle.Linear.Neutral switch {
        var neutral => new() { Soft = ((float)MinValue, neutral.Highlights.Start + neutral.Highlights.Width - (neutral.Shadows.Start - neutral.Shadows.Width)), Step = 0.1f, Decimals = 2 },
    };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ZoneContrast : IPresented<ZoneContrast, float> {
    public static ZoneContrast MinValue { get; } = new(0.01f);
    public static ZoneContrast MaxValue { get; } = new(1.99f);
    public static ZoneContrast Neutral { get; } = new(1f);
    public static Presentation<float> Presentation => ZoneAdjustment.Presentation;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CompressionThreshold : IPresented<CompressionThreshold, float> {
    public static CompressionThreshold MinValue { get; } = new(0f);
    public static CompressionThreshold MaxValue { get; } = new(float.BitDecrement(1f));
    public static Presentation<float> Presentation { get; } = new() { Step = 0.001f, Decimals = 3 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CompressionLimit : IPresented<CompressionLimit, float> {
    public static CompressionLimit MinValue { get; } = new(float.BitIncrement(1f));
    public static CompressionLimit MaxValue { get; } = new((float)Half.MaxValue);
    public static Presentation<float> Presentation { get; } = new() { Soft = (1f, 2f), Step = 0.001f, Decimals = 3 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CompressionPower : IPresented<CompressionPower, float> {
    public static CompressionPower MinValue { get; } = new(1f);
    public static CompressionPower MaxValue { get; } = new((float)Half.MaxValue);
    public static Presentation<float> Presentation { get; } = new() { Soft = (1f, 2f) };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record ColorDecisionList(Channels<Magnitude> Slope, Channels<SignedMagnitude> Offset, Channels<Factor> Power, Magnitude Saturation)
    : IStateRecord<ColorDecisionList, ColorDecisionListParameter>, IPixelStage<ColorDecisionList> {
    public static ColorDecisionList Default { get; } = new(new(Magnitude.Unit), new(SignedMagnitude.Zero), new(Factor.Unit), Magnitude.Unit);

    public LutTable.Cdl Correction =>
        new(CdlStyle.FwdNoClamp, new(Slope.Red, Slope.Green, Slope.Blue), new(Offset.Red, Offset.Green, Offset.Blue), new(Power.Red, Power.Green, Power.Blue), Saturation);

    public static Fin<ColorDecisionList> Of(LutTable.Cdl correction) =>
        (Channels<Magnitude>.Validated(correction.Slope, Magnitude.Validated), Channels<SignedMagnitude>.Validated(correction.Offset, SignedMagnitude.Validated),
         Channels<Factor>.Validated(correction.Power, Factor.Validated), Magnitude.Validated(correction.Saturation))
            .Apply(static (slope, offset, power, saturation) => new ColorDecisionList(slope, offset, power, saturation))
            .As();

    public static Option<PixelPass> Pass(ColorDecisionList state, PassContext context) =>
        state == Default ? None
            : Some<PixelPass>(new PixelPass.Color(LutTables.Bracketed(context.Working, FormationTables.AgXLog.Gamut, FormationTables.AgXLog.Into, state.Correction, FormationTables.AgXLog.Back)));
}

[SmartEnum<string>]
public sealed partial class ColorDecisionListParameter : IStateParameter<ColorDecisionList> {
    private static readonly Lens<ColorDecisionList, Channels<Magnitude>> Slopes = Lens<ColorDecisionList, Channels<Magnitude>>.New(static state => state.Slope, static slope => state => state with { Slope = slope });
    private static readonly Lens<ColorDecisionList, Channels<SignedMagnitude>> Offsets = Lens<ColorDecisionList, Channels<SignedMagnitude>>.New(static state => state.Offset, static offset => state => state with { Offset = offset });
    private static readonly Lens<ColorDecisionList, Channels<Factor>> Powers = Lens<ColorDecisionList, Channels<Factor>>.New(static state => state.Power, static power => state => state with { Power = power });
    private static readonly Presentation<float> Gain = new() { Soft = (0f, 2f), Origin = 1f };
    private static readonly Presentation<float> Shift = SignedMagnitude.Presentation with { Soft = (-1f, 1f) };
    private static readonly Presentation<float> Exponent = new() { Soft = (0.1f, 2f), Origin = 1f };

    public static readonly ColorDecisionListParameter SlopeRed = new("slope-red", StateParameter.Bounded(lens(Slopes, Channels<Magnitude>.RedOf), Gain), new("Red slope", "Red multiplier before offset"));
    public static readonly ColorDecisionListParameter SlopeGreen = new("slope-green", StateParameter.Bounded(lens(Slopes, Channels<Magnitude>.GreenOf), Gain), new("Green slope", "Green multiplier before offset"));
    public static readonly ColorDecisionListParameter SlopeBlue = new("slope-blue", StateParameter.Bounded(lens(Slopes, Channels<Magnitude>.BlueOf), Gain), new("Blue slope", "Blue multiplier before offset"));
    public static readonly ColorDecisionListParameter OffsetRed = new("offset-red", StateParameter.Bounded(lens(Offsets, Channels<SignedMagnitude>.RedOf), Shift), new("Red offset", "Value added to red after slope"));
    public static readonly ColorDecisionListParameter OffsetGreen = new("offset-green", StateParameter.Bounded(lens(Offsets, Channels<SignedMagnitude>.GreenOf), Shift), new("Green offset", "Value added to green after slope"));
    public static readonly ColorDecisionListParameter OffsetBlue = new("offset-blue", StateParameter.Bounded(lens(Offsets, Channels<SignedMagnitude>.BlueOf), Shift), new("Blue offset", "Value added to blue after slope"));
    public static readonly ColorDecisionListParameter PowerRed = new("power-red", StateParameter.Bounded(lens(Powers, Channels<Factor>.RedOf), Exponent), new("Red power", "Exponent on red after offset"));
    public static readonly ColorDecisionListParameter PowerGreen = new("power-green", StateParameter.Bounded(lens(Powers, Channels<Factor>.GreenOf), Exponent), new("Green power", "Exponent on green after offset"));
    public static readonly ColorDecisionListParameter PowerBlue = new("power-blue", StateParameter.Bounded(lens(Powers, Channels<Factor>.BlueOf), Exponent), new("Blue power", "Exponent on blue after offset"));
    public static readonly ColorDecisionListParameter Saturation = new("saturation", StateParameter.Bounded(Lens<ColorDecisionList, Magnitude>.New(static state => state.Saturation, static saturation => state => state with { Saturation = saturation }), Gain),
        new("Saturation", "Distance from Rec.709 luma after power"));

    public StateParameter<ColorDecisionList> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ContrastGrade(ContrastAmount Amount, ContrastPivot Pivot, SignedFraction Vibrance, Magnitude Saturation) : IStateRecord<ContrastGrade, ContrastGradeParameter>, IPixelStage<ContrastGrade> {
    public static ContrastGrade Default { get; } = new(ContrastAmount.Neutral, ContrastPivot.MiddleGray, SignedFraction.Neutral, Magnitude.Unit);

    public static ColorRamp Chroma { get; } = new ColorRamp.Smooth([
        new(Presets.Shown(new Vector4(new Vector3(Exposure.MiddleGray), 1f)), Fraction.MinValue), new(Presets.Shown(new Vector4(1f, 0f, 0f, 1f)), Fraction.MaxValue)]) { Space = CssSpace.SrgbLinear };

    public static Option<PixelPass> Pass(ContrastGrade state, PassContext context) =>
        state with { Pivot = Default.Pivot } == Default ? None
            : Some<PixelPass>(new PixelPass.Color(row => Primary.Contrast(row, state.Amount, state.Pivot.Linear, state.Vibrance, state.Saturation, context.Working.Luminance)));
}

[SmartEnum<string>]
public sealed partial class ContrastGradeParameter : IStateParameter<ContrastGrade> {
    public static readonly ContrastGradeParameter Amount = new("contrast", StateParameter.Bounded(Lens<ContrastGrade, ContrastAmount>.New(static grade => grade.Amount, static amount => grade => grade with { Amount = amount })),
        new("Contrast", "Exponent about pivot gray, image unchanged at one"));
    public static readonly ContrastGradeParameter Pivot = new("pivot", StateParameter.Bounded(Lens<ContrastGrade, ContrastPivot>.New(static grade => grade.Pivot, static pivot => grade => grade with { Pivot = pivot })),
        new("Pivot", "Stops from middle gray"));
    public static readonly ContrastGradeParameter Vibrance = new("vibrance", StateParameter.Bounded(Lens<ContrastGrade, SignedFraction>.New(static grade => grade.Vibrance, static vibrance => grade => grade with { Vibrance = vibrance })),
        new("Vibrance", "Saturation gain falling as saturation rises"));
    public static readonly ContrastGradeParameter Saturation = new("saturation", StateParameter.Bounded(Lens<ContrastGrade, Magnitude>.New(static grade => grade.Saturation, static saturation => grade => grade with { Saturation = saturation }),
        Magnitude.Presentation with { Soft = (0f, 2f), Origin = 1f }),
        new("Saturation", "Distance from working luminance, gray at zero"));

    public StateParameter<ContrastGrade> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Levels(SignedMagnitude Black, Stops White) : IStateRecord<Levels, LevelsParameter>, IPixelStage<Levels> {
    public static Levels Default { get; } = new(SignedMagnitude.Zero, Stops.Zero);

    public static Fin<Levels> Auto(PixelFrame frame, Gamut working) =>
        LuminanceSamples.Gather(frame, working.Luminance).Bind(static samples =>
            (SignedMagnitude.Validated((float)samples.Minimum), Stops.Validated((float)double.Log2(samples.FrameWhite - samples.Minimum)))
                .Apply(static (black, white) => new Levels(black, white))
                .As());

    public static Option<PixelPass> Pass(Levels state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(Primary.Levels(state.Black, state.White)));
}

[SmartEnum<string>]
public sealed partial class LevelsParameter : IStateParameter<Levels> {
    public static readonly LevelsParameter Black = new("black-level", StateParameter.Bounded(Lens<Levels, SignedMagnitude>.New(static levels => levels.Black, static black => levels => levels with { Black = black }),
        SignedMagnitude.Presentation with { Soft = (-0.1f, 0.1f), Decimals = 4 }),
        new("Black", "Scene value mapped to zero, lower values pass unclamped"));
    public static readonly LevelsParameter White = new("white-level", StateParameter.Bounded(Lens<Levels, Stops>.New(static levels => levels.White, static white => levels => levels with { White = white })),
        new("White", "Span above black mapped to one, in stops"));

    public StateParameter<Levels> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ChannelMixer(Channels<Channels<SignedMagnitude>> Weights) : IStateRecord<ChannelMixer, ChannelMixerParameter>, IPixelStage<ChannelMixer> {
    public static ChannelMixer Default { get; } = new(new Channels<Channels<SignedMagnitude>>(
        new(SignedMagnitude.Unit, SignedMagnitude.Zero, SignedMagnitude.Zero),
        new(SignedMagnitude.Zero, SignedMagnitude.Unit, SignedMagnitude.Zero),
        new(SignedMagnitude.Zero, SignedMagnitude.Zero, SignedMagnitude.Unit)));

    public LutTable.Affine Table =>
        new(Matrix<double>.Build.DenseOfRowArrays(Seq(Weights.Red, Weights.Green, Weights.Blue).Map(static row => new double[] { row.Red, row.Green, row.Blue })));

    public static Option<PixelPass> Pass(ChannelMixer state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(state.Table));
}

[SmartEnum<string>]
public sealed partial class ChannelMixerParameter : IStateParameter<ChannelMixer> {
    private static readonly Lens<ChannelMixer, Channels<Channels<SignedMagnitude>>> Weights =
        Lens<ChannelMixer, Channels<Channels<SignedMagnitude>>>.New(static mixer => mixer.Weights, static weights => mixer => mixer with { Weights = weights });

    public static readonly ChannelMixerParameter RedFromRed = Weight("red-from-red", Channels<Channels<SignedMagnitude>>.RedOf, Channels<SignedMagnitude>.RedOf,
        new("Red from red", "Weight of input red in output red"));
    public static readonly ChannelMixerParameter RedFromGreen = Weight("red-from-green", Channels<Channels<SignedMagnitude>>.RedOf, Channels<SignedMagnitude>.GreenOf,
        new("Red from green", "Weight of input green in output red"));
    public static readonly ChannelMixerParameter RedFromBlue = Weight("red-from-blue", Channels<Channels<SignedMagnitude>>.RedOf, Channels<SignedMagnitude>.BlueOf,
        new("Red from blue", "Weight of input blue in output red"));
    public static readonly ChannelMixerParameter GreenFromRed = Weight("green-from-red", Channels<Channels<SignedMagnitude>>.GreenOf, Channels<SignedMagnitude>.RedOf,
        new("Green from red", "Weight of input red in output green"));
    public static readonly ChannelMixerParameter GreenFromGreen = Weight("green-from-green", Channels<Channels<SignedMagnitude>>.GreenOf, Channels<SignedMagnitude>.GreenOf,
        new("Green from green", "Weight of input green in output green"));
    public static readonly ChannelMixerParameter GreenFromBlue = Weight("green-from-blue", Channels<Channels<SignedMagnitude>>.GreenOf, Channels<SignedMagnitude>.BlueOf,
        new("Green from blue", "Weight of input blue in output green"));
    public static readonly ChannelMixerParameter BlueFromRed = Weight("blue-from-red", Channels<Channels<SignedMagnitude>>.BlueOf, Channels<SignedMagnitude>.RedOf,
        new("Blue from red", "Weight of input red in output blue"));
    public static readonly ChannelMixerParameter BlueFromGreen = Weight("blue-from-green", Channels<Channels<SignedMagnitude>>.BlueOf, Channels<SignedMagnitude>.GreenOf,
        new("Blue from green", "Weight of input green in output blue"));
    public static readonly ChannelMixerParameter BlueFromBlue = Weight("blue-from-blue", Channels<Channels<SignedMagnitude>>.BlueOf, Channels<SignedMagnitude>.BlueOf,
        new("Blue from blue", "Weight of input blue in output blue"));

    public StateParameter<ChannelMixer> Kind { get; }
    public ParameterText Text { get; }

    private static ChannelMixerParameter Weight(
        string key, Lens<Channels<Channels<SignedMagnitude>>, Channels<SignedMagnitude>> output, Lens<Channels<SignedMagnitude>, SignedMagnitude> input, ParameterText text) =>
        lens(Weights, output, input) switch {
            var weight => new(key, StateParameter.Bounded(weight, SignedMagnitude.Presentation with { Soft = (-2f, 2f), Origin = (float)weight.Get(ChannelMixer.Default) }), text),
        };
}

public sealed record WhiteBalance(Kelvin Cct, Tint Duv) : IStateRecord<WhiteBalance, WhiteBalanceParameter>, IPixelStage<WhiteBalance> {
    public static WhiteBalance Default { get; } = new(Kelvin.Daylight, Tint.Daylight);

    public static Seq<(string Name, EffectLook<WhiteBalance> Look)> Looks { get; } =
        (Seq(Illuminant.A, Illuminant.C, Illuminant.D50, Illuminant.D55, Illuminant.D65, Illuminant.D75, Illuminant.E, Illuminant.F2, Illuminant.F7, Illuminant.F11)
            .Map(static illuminant => (illuminant.Name, White: new Unicolour(Configuration.Default, illuminant.GetWhitePoint(Observer.Degree2).Chromaticity).Temperature))
            .Map(static lit => (lit.Name, lit.White.Cct, lit.White.Duv))
        + toSeq(StandardIlluminant.Items).Map(static illuminant => (Name: $"Illuminant {illuminant.Key}", illuminant.Temperature.Cct, illuminant.Temperature.Duv)))
            .Traverse(static lit => Of(lit.Cct, lit.Duv).Map(white => (lit.Name, (EffectLook<WhiteBalance>)new EffectLook<WhiteBalance>.Whole(white))))
            .As()
            .ThrowIfFail();

    public static Fin<WhiteBalance> Of(double cct, double duv) => (Kelvin.Validated(cct), Tint.Validated(duv)).Apply(static (kelvin, tint) => new WhiteBalance(kelvin, tint)).As();

    public static Fin<WhiteBalance> Picked(PixelFrame frame, Option<Rectangle> region, Gamut working) =>
        Primary.PickedWhite(frame, region, working).Bind(static white => Of(white.Cct, white.Duv));

    public static Option<PixelPass> Pass(WhiteBalance state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(Primary.WhiteBalance(context.Working, new(state.Cct, state.Duv))));
}

[SmartEnum<string>]
public sealed partial class WhiteBalanceParameter : IStateParameter<WhiteBalance> {
    public static readonly WhiteBalanceParameter Cct = new("temperature", StateParameter.Bounded(Lens<WhiteBalance, Kelvin>.New(static state => state.Cct, static cct => state => state with { Cct = cct })),
        new("Temperature", "Kelvin of light rendered neutral"));
    public static readonly WhiteBalanceParameter Duv = new("tint", StateParameter.Bounded(Lens<WhiteBalance, Tint>.New(static state => state.Duv, static duv => state => state with { Duv = duv })),
        new("Tint", "Neutral light offset from blackbody locus, positive toward green"));

    public StateParameter<WhiteBalance> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ToneWheel(WheelHue Hue, Fraction Strength, WheelLuma Luma) : IStateRecord<ToneWheel, ToneWheelParameter> {
    public static ToneWheel Default { get; } = new(WheelHue.Origin, Fraction.MinValue, WheelLuma.Neutral);
}

[SmartEnum<string>]
public sealed partial class ToneWheelParameter : IStateParameter<ToneWheel> {
    public static readonly ToneWheelParameter Hue = new("hue", StateParameter.Bounded(Lens<ToneWheel, WheelHue>.New(static wheel => wheel.Hue, static hue => wheel => wheel with { Hue = hue })),
        new("Hue", "Tint angle counterclockwise from right"));
    public static readonly ToneWheelParameter Strength = new("strength", StateParameter.Bounded(Lens<ToneWheel, Fraction>.New(static wheel => wheel.Strength, static strength => wheel => wheel with { Strength = strength })),
        new("Strength", "Tint chroma"));
    public static readonly ToneWheelParameter Luma = new("luma", StateParameter.Bounded(Lens<ToneWheel, WheelLuma>.New(static wheel => wheel.Luma, static luma => wheel => wheel with { Luma = luma })),
        new("Luma", "Gain in stops"));

    public StateParameter<ToneWheel> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ColorWheels(ToneWheel Global, ToneWheel Shadows, ToneWheel Midtones, ToneWheel Highlights, PositiveFraction ShadowsEnd, Fraction HighlightsStart, PositiveFraction HighlightsWidth)
    : IStateRecord<ColorWheels, ColorWheelsParameter>, IPixelStage<ColorWheels> {
    public static ColorWheels Default { get; } = new(
        ToneWheel.Default, ToneWheel.Default, ToneWheel.Default, ToneWheel.Default, PositiveFraction.Create(0.333f), Fraction.Create(0.55f), PositiveFraction.Create(0.45f));

    public static Option<PixelPass> Pass(ColorWheels state, PassContext context) =>
        Seq(state.Global, state.Shadows, state.Midtones, state.Highlights).ForAll(static wheel => wheel == ToneWheel.Default) ? None
            : Some<PixelPass>(new PixelPass.Color(WheelOffset.Graded(
                context.Working, Offset(state.Global), Offset(state.Shadows), Offset(state.Midtones), Offset(state.Highlights), state.ShadowsEnd, state.HighlightsStart, state.HighlightsWidth)));

    private static WheelOffset Offset(ToneWheel wheel) => new(wheel.Hue, wheel.Strength, wheel.Luma);
}

[SmartEnum<string>]
public sealed partial class ColorWheelsParameter : IStateParameter<ColorWheels> {
    public static readonly ColorWheelsParameter Global = new("global", new StateParameter<ColorWheels>.Record<ToneWheel>(Lens<ColorWheels, ToneWheel>.New(static wheels => wheels.Global, static wheel => wheels => wheels with { Global = wheel })),
        new("Global", "Tint over every tone"));
    public static readonly ColorWheelsParameter Shadows = new("shadows", new StateParameter<ColorWheels>.Record<ToneWheel>(Lens<ColorWheels, ToneWheel>.New(static wheels => wheels.Shadows, static wheel => wheels => wheels with { Shadows = wheel })),
        new("Shadows", "Tint over dark tones"));
    public static readonly ColorWheelsParameter Midtones = new("midtones", new StateParameter<ColorWheels>.Record<ToneWheel>(Lens<ColorWheels, ToneWheel>.New(static wheels => wheels.Midtones, static wheel => wheels => wheels with { Midtones = wheel })),
        new("Midtones", "Tint over middle tones"));
    public static readonly ColorWheelsParameter Highlights = new("highlights", new StateParameter<ColorWheels>.Record<ToneWheel>(Lens<ColorWheels, ToneWheel>.New(static wheels => wheels.Highlights, static wheel => wheels => wheels with { Highlights = wheel })),
        new("Highlights", "Tint over bright tones"));
    public static readonly ColorWheelsParameter ShadowsEnd = new("shadows-end", StateParameter.Bounded(Lens<ColorWheels, PositiveFraction>.New(static wheels => wheels.ShadowsEnd, static end => wheels => wheels with { ShadowsEnd = end })),
        new("Shadows end", "Working luminance ending shadows weight"));
    public static readonly ColorWheelsParameter HighlightsStart = new("highlights-start", StateParameter.Bounded(Lens<ColorWheels, Fraction>.New(static wheels => wheels.HighlightsStart, static start => wheels => wheels with { HighlightsStart = start })),
        new("Highlights start", "Working luminance starting highlights weight"));
    public static readonly ColorWheelsParameter HighlightsWidth = new("highlights-width", StateParameter.Bounded(Lens<ColorWheels, PositiveFraction>.New(static wheels => wheels.HighlightsWidth, static width => wheels => wheels with { HighlightsWidth = width })),
        new("Highlights width", "Luminance span over which highlights weight reaches full"));

    public StateParameter<ColorWheels> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record QualifierBand(Fraction Center, Fraction Width, Fraction Softness) {
    public static QualifierBand Open { get; } = new(Fraction.Create(0.5f), Fraction.MaxValue, Fraction.MinValue);

    public static Lens<QualifierBand, Fraction> CenterOf { get; } = Lens<QualifierBand, Fraction>.New(static band => band.Center, static center => band => band with { Center = center });
    public static Lens<QualifierBand, Fraction> WidthOf { get; } = Lens<QualifierBand, Fraction>.New(static band => band.Width, static width => band => band with { Width = width });
    public static Lens<QualifierBand, Fraction> SoftnessOf { get; } = Lens<QualifierBand, Fraction>.New(static band => band.Softness, static softness => band => band with { Softness = softness });
}

public sealed record Qualifier(QualifierBand Hue, QualifierBand Saturation, QualifierBand Luma) : IStateRecord<Qualifier, QualifierParameter> {
    public static Qualifier Default { get; } = new(QualifierBand.Open, QualifierBand.Open, QualifierBand.Open);

    public HueRange Range => new(Band(Hue), Band(Saturation), Band(Luma));

    private static RangeBand Band(QualifierBand band) => new(band.Center, band.Width, band.Softness);
}

[SmartEnum<string>]
public sealed partial class QualifierParameter : IStateParameter<Qualifier> {
    private static readonly Lens<Qualifier, QualifierBand> Hues = Lens<Qualifier, QualifierBand>.New(static range => range.Hue, static band => range => range with { Hue = band });
    private static readonly Lens<Qualifier, QualifierBand> Saturations = Lens<Qualifier, QualifierBand>.New(static range => range.Saturation, static band => range => range with { Saturation = band });
    private static readonly Lens<Qualifier, QualifierBand> Lumas = Lens<Qualifier, QualifierBand>.New(static range => range.Luma, static band => range => range with { Luma = band });

    public static readonly QualifierParameter HueCenter = new("hue-center", StateParameter.Bounded(lens(Hues, QualifierBand.CenterOf), Presentations.Turn), new("Hue center", "Middle of selected hue band"));
    public static readonly QualifierParameter HueWidth = new("hue-width", StateParameter.Bounded(lens(Hues, QualifierBand.WidthOf), Presentations.Turn), new("Hue width", "Full-strength hue span"));
    public static readonly QualifierParameter HueSoftness = new("hue-softness", StateParameter.Bounded(lens(Hues, QualifierBand.SoftnessOf), Presentations.Turn), new("Hue softness", "Falloff past each hue band edge"));
    public static readonly QualifierParameter SaturationCenter = new("saturation-center", StateParameter.Bounded(lens(Saturations, QualifierBand.CenterOf)), new("Saturation center", "Middle of selected saturation band"));
    public static readonly QualifierParameter SaturationWidth = new("saturation-width", StateParameter.Bounded(lens(Saturations, QualifierBand.WidthOf)), new("Saturation width", "Full-strength saturation span"));
    public static readonly QualifierParameter SaturationSoftness = new("saturation-softness", StateParameter.Bounded(lens(Saturations, QualifierBand.SoftnessOf)), new("Saturation softness", "Falloff past each saturation band edge"));
    public static readonly QualifierParameter LumaCenter = new("luma-center", StateParameter.Bounded(lens(Lumas, QualifierBand.CenterOf)), new("Luma center", "Middle of selected luma band in stop axis share"));
    public static readonly QualifierParameter LumaWidth = new("luma-width", StateParameter.Bounded(lens(Lumas, QualifierBand.WidthOf)), new("Luma width", "Full-strength luma span"));
    public static readonly QualifierParameter LumaSoftness = new("luma-softness", StateParameter.Bounded(lens(Lumas, QualifierBand.SoftnessOf)), new("Luma softness", "Falloff past each luma band edge"));

    public StateParameter<Qualifier> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record SelectiveColor(Qualifier Range, SignedAngle Shift, Magnitude Saturation, Stops Exposure) : IStateRecord<SelectiveColor, SelectiveColorParameter>, IPixelStage<SelectiveColor> {
    public static SelectiveColor Default { get; } = new(Qualifier.Default, SignedAngle.Neutral, Magnitude.Unit, Stops.Zero);

    public static ColorRamp Shifts { get; } = Presets.Sampled(
        CssSpace.Oklab, static at => new Vector4(Hsy.Saturated(Hsy.From(Vector4.UnitX).Hue + float.Lerp(SignedAngle.MinValue, SignedAngle.MaxValue, at)), 1f),
        Fraction.MinValue, Fraction.Create(1f / 6f), Fraction.Create(2f / 6f), Fraction.Create(3f / 6f), Fraction.Create(4f / 6f), Fraction.Create(5f / 6f), Fraction.MaxValue);

    public static Option<PixelPass> Pass(SelectiveColor state, PassContext context) =>
        (state.Shift, state.Saturation, state.Exposure) == (SignedAngle.Neutral, Magnitude.Unit, Stops.Zero) ? None
            : Some<PixelPass>(new PixelPass.Color(row => state.Range.Range.Graded(row, state.Shift, state.Saturation, state.Exposure)));
}

[SmartEnum<string>]
public sealed partial class SelectiveColorParameter : IStateParameter<SelectiveColor> {
    public static readonly SelectiveColorParameter Range = new("range", new StateParameter<SelectiveColor>.Record<Qualifier>(Lens<SelectiveColor, Qualifier>.New(static state => state.Range, static range => state => state with { Range = range })),
        new("Range", "Band of input graded"));
    public static readonly SelectiveColorParameter HueShift = new("hue-shift", StateParameter.Bounded(Lens<SelectiveColor, SignedAngle>.New(static state => state.Shift, static shift => state => state with { Shift = shift }), Presentations.Angle),
        new("Hue shift", "Hue turn on qualified pixels"));
    public static readonly SelectiveColorParameter Saturation = new("saturation", StateParameter.Bounded(Lens<SelectiveColor, Magnitude>.New(static state => state.Saturation, static saturation => state => state with { Saturation = saturation }),
        Magnitude.Presentation with { Soft = (0f, 2f), Origin = 1f }),
        new("Saturation", "Saturation gain on qualified pixels"));
    public static readonly SelectiveColorParameter Exposure = new("exposure", StateParameter.Bounded(Lens<SelectiveColor, Stops>.New(static state => state.Exposure, static exposure => state => state with { Exposure = exposure })),
        new("Exposure", "Stops added to qualified pixels"));

    public StateParameter<SelectiveColor> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record HueIsolation(Qualifier Range) : IStateRecord<HueIsolation, HueIsolationParameter>, IPixelStage<HueIsolation> {
    public static HueIsolation Default { get; } = new(Qualifier.Default);

    public static Option<PixelPass> Pass(HueIsolation state, PassContext context) =>
        state == Default ? None : Some<PixelPass>(new PixelPass.Color(row => state.Range.Range.Isolated(row, context.Working.Luminance)));
}

[SmartEnum<string>]
public sealed partial class HueIsolationParameter : IStateParameter<HueIsolation> {
    public static readonly HueIsolationParameter Range = new("range", new StateParameter<HueIsolation>.Record<Qualifier>(Lens<HueIsolation, Qualifier>.New(static state => state.Range, static range => state => state with { Range = range })),
        new("Range", "Band of input kept in color"));

    public StateParameter<HueIsolation> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record Zone<TAdjustment>(TAdjustment Master, TAdjustment Red, TAdjustment Green, TAdjustment Blue, Stops Start, ZoneWidth Width)
    where TAdjustment : struct, IPresented<TAdjustment, float> {
    public static Lens<Zone<TAdjustment>, TAdjustment> MasterOf { get; } = Lens<Zone<TAdjustment>, TAdjustment>.New(static zone => zone.Master, static value => zone => zone with { Master = value });
    public static Lens<Zone<TAdjustment>, TAdjustment> RedOf { get; } = Lens<Zone<TAdjustment>, TAdjustment>.New(static zone => zone.Red, static value => zone => zone with { Red = value });
    public static Lens<Zone<TAdjustment>, TAdjustment> GreenOf { get; } = Lens<Zone<TAdjustment>, TAdjustment>.New(static zone => zone.Green, static value => zone => zone with { Green = value });
    public static Lens<Zone<TAdjustment>, TAdjustment> BlueOf { get; } = Lens<Zone<TAdjustment>, TAdjustment>.New(static zone => zone.Blue, static value => zone => zone with { Blue = value });
    public static Lens<Zone<TAdjustment>, Stops> StartOf { get; } = Lens<Zone<TAdjustment>, Stops>.New(static zone => zone.Start, static start => zone => zone with { Start = start });
    public static Lens<Zone<TAdjustment>, ZoneWidth> WidthOf { get; } = Lens<Zone<TAdjustment>, ZoneWidth>.New(static zone => zone.Width, static width => zone => zone with { Width = width });

    public GradingRgbmsw Rgbmsw => new(Red.ToValue(), Green.ToValue(), Blue.ToValue(), Master.ToValue(), Start, Width);

    public static Fin<Zone<TAdjustment>> Of(GradingRgbmsw zone) =>
        (ValidationResults.Validated<TAdjustment, float>(zone.Master), ValidationResults.Validated<TAdjustment, float>(zone.Red), ValidationResults.Validated<TAdjustment, float>(zone.Green),
         ValidationResults.Validated<TAdjustment, float>(zone.Blue), Stops.Validated(zone.Start), ZoneWidth.Validated(zone.Width))
            .Apply(static (master, red, green, blue, start, width) => new Zone<TAdjustment>(master, red, green, blue, start, width))
            .As();
}

public sealed record ToneZones(
    Zone<ZoneAdjustment> Blacks, Zone<PivotAdjustment> Shadows, Zone<ZoneAdjustment> Midtones, Zone<PivotAdjustment> Highlights, Zone<ZoneAdjustment> Whites, ZoneContrast SContrast)
    : IStateRecord<ToneZones, ToneZonesParameter>, IPixelStage<ToneZones> {
    public static ToneZones Default { get; } = GradingStyle.Linear.Neutral switch {
        var neutral => (Zone<ZoneAdjustment>.Of(neutral.Blacks), Zone<PivotAdjustment>.Of(neutral.Shadows), Zone<ZoneAdjustment>.Of(neutral.Midtones),
                Zone<PivotAdjustment>.Of(neutral.Highlights), Zone<ZoneAdjustment>.Of(neutral.Whites))
            .Apply(static (blacks, shadows, midtones, highlights, whites) => new ToneZones(blacks, shadows, midtones, highlights, whites, ZoneContrast.Neutral))
            .As()
            .ThrowIfFail(),
    };

    public static Option<PixelPass> Pass(ToneZones state, PassContext context) =>
        new GradingTone(state.Blacks.Rgbmsw, state.Shadows.Rgbmsw, state.Midtones.Rgbmsw, state.Highlights.Rgbmsw, state.Whites.Rgbmsw, state.SContrast)
            .Graded(GradingStyle.Linear)
            .Map(static graded => (PixelPass)new PixelPass.Color(graded));
}

[SmartEnum<string>]
public sealed partial class ToneZonesParameter : IStateParameter<ToneZones> {
    private static readonly Lens<ToneZones, Zone<ZoneAdjustment>> Blacks = Lens<ToneZones, Zone<ZoneAdjustment>>.New(static zones => zones.Blacks, static zone => zones => zones with { Blacks = zone });
    private static readonly Lens<ToneZones, Zone<PivotAdjustment>> Shadows = Lens<ToneZones, Zone<PivotAdjustment>>.New(static zones => zones.Shadows, static zone => zones => zones with { Shadows = zone });
    private static readonly Lens<ToneZones, Zone<ZoneAdjustment>> Midtones = Lens<ToneZones, Zone<ZoneAdjustment>>.New(static zones => zones.Midtones, static zone => zones => zones with { Midtones = zone });
    private static readonly Lens<ToneZones, Zone<PivotAdjustment>> Highlights = Lens<ToneZones, Zone<PivotAdjustment>>.New(static zones => zones.Highlights, static zone => zones => zones with { Highlights = zone });
    private static readonly Lens<ToneZones, Zone<ZoneAdjustment>> Whites = Lens<ToneZones, Zone<ZoneAdjustment>>.New(static zones => zones.Whites, static zone => zones => zones with { Whites = zone });

    public static readonly ToneZonesParameter BlacksMaster = new("blacks-master", StateParameter.Bounded(lens(Blacks, Zone<ZoneAdjustment>.MasterOf)), new("Blacks", "Adjustment in every channel, unchanged at one"));
    public static readonly ToneZonesParameter BlacksRed = new("blacks-red", StateParameter.Bounded(lens(Blacks, Zone<ZoneAdjustment>.RedOf)), new("Blacks red", "Adjustment in red"));
    public static readonly ToneZonesParameter BlacksGreen = new("blacks-green", StateParameter.Bounded(lens(Blacks, Zone<ZoneAdjustment>.GreenOf)), new("Blacks green", "Adjustment in green"));
    public static readonly ToneZonesParameter BlacksBlue = new("blacks-blue", StateParameter.Bounded(lens(Blacks, Zone<ZoneAdjustment>.BlueOf)), new("Blacks blue", "Adjustment in blue"));
    public static readonly ToneZonesParameter BlacksStart = new("blacks-start", StateParameter.Bounded(lens(Blacks, Zone<ZoneAdjustment>.StartOf)), new("Blacks start", "Blacks top in stops from middle gray"));
    public static readonly ToneZonesParameter BlacksWidth = new("blacks-width", StateParameter.Bounded(lens(Blacks, Zone<ZoneAdjustment>.WidthOf)), new("Blacks width", "Span in stops"));
    public static readonly ToneZonesParameter ShadowsMaster = new("shadows-master", StateParameter.Bounded(lens(Shadows, Zone<PivotAdjustment>.MasterOf)), new("Shadows", "Adjustment in every channel, unchanged at one"));
    public static readonly ToneZonesParameter ShadowsRed = new("shadows-red", StateParameter.Bounded(lens(Shadows, Zone<PivotAdjustment>.RedOf)), new("Shadows red", "Adjustment in red"));
    public static readonly ToneZonesParameter ShadowsGreen = new("shadows-green", StateParameter.Bounded(lens(Shadows, Zone<PivotAdjustment>.GreenOf)), new("Shadows green", "Adjustment in green"));
    public static readonly ToneZonesParameter ShadowsBlue = new("shadows-blue", StateParameter.Bounded(lens(Shadows, Zone<PivotAdjustment>.BlueOf)), new("Shadows blue", "Adjustment in blue"));
    public static readonly ToneZonesParameter ShadowsStart = new("shadows-start", StateParameter.Bounded(lens(Shadows, Zone<PivotAdjustment>.StartOf)), new("Shadows start", "Shadows top in stops from middle gray"));
    public static readonly ToneZonesParameter ShadowsWidth = new("shadows-width", StateParameter.Bounded(lens(Shadows, Zone<PivotAdjustment>.WidthOf)), new("Shadows width", "Span in stops"));
    public static readonly ToneZonesParameter MidtonesMaster = new("midtones-master", StateParameter.Bounded(lens(Midtones, Zone<ZoneAdjustment>.MasterOf)), new("Midtones", "Adjustment in every channel, unchanged at one"));
    public static readonly ToneZonesParameter MidtonesRed = new("midtones-red", StateParameter.Bounded(lens(Midtones, Zone<ZoneAdjustment>.RedOf)), new("Midtones red", "Adjustment in red"));
    public static readonly ToneZonesParameter MidtonesGreen = new("midtones-green", StateParameter.Bounded(lens(Midtones, Zone<ZoneAdjustment>.GreenOf)), new("Midtones green", "Adjustment in green"));
    public static readonly ToneZonesParameter MidtonesBlue = new("midtones-blue", StateParameter.Bounded(lens(Midtones, Zone<ZoneAdjustment>.BlueOf)), new("Midtones blue", "Adjustment in blue"));
    public static readonly ToneZonesParameter MidtonesStart = new("midtones-start", StateParameter.Bounded(lens(Midtones, Zone<ZoneAdjustment>.StartOf)), new("Midtones center", "Midtones center in stops from middle gray"));
    public static readonly ToneZonesParameter MidtonesWidth = new("midtones-width", StateParameter.Bounded(lens(Midtones, Zone<ZoneAdjustment>.WidthOf)), new("Midtones width", "Span in stops"));
    public static readonly ToneZonesParameter HighlightsMaster = new("highlights-master", StateParameter.Bounded(lens(Highlights, Zone<PivotAdjustment>.MasterOf)), new("Highlights", "Adjustment in every channel, unchanged at one"));
    public static readonly ToneZonesParameter HighlightsRed = new("highlights-red", StateParameter.Bounded(lens(Highlights, Zone<PivotAdjustment>.RedOf)), new("Highlights red", "Adjustment in red"));
    public static readonly ToneZonesParameter HighlightsGreen = new("highlights-green", StateParameter.Bounded(lens(Highlights, Zone<PivotAdjustment>.GreenOf)), new("Highlights green", "Adjustment in green"));
    public static readonly ToneZonesParameter HighlightsBlue = new("highlights-blue", StateParameter.Bounded(lens(Highlights, Zone<PivotAdjustment>.BlueOf)), new("Highlights blue", "Adjustment in blue"));
    public static readonly ToneZonesParameter HighlightsStart = new("highlights-start", StateParameter.Bounded(lens(Highlights, Zone<PivotAdjustment>.StartOf)), new("Highlights start", "Highlights bottom in stops from middle gray"));
    public static readonly ToneZonesParameter HighlightsWidth = new("highlights-width", StateParameter.Bounded(lens(Highlights, Zone<PivotAdjustment>.WidthOf)), new("Highlights width", "Span in stops"));
    public static readonly ToneZonesParameter WhitesMaster = new("whites-master", StateParameter.Bounded(lens(Whites, Zone<ZoneAdjustment>.MasterOf)), new("Whites", "Adjustment in every channel, unchanged at one"));
    public static readonly ToneZonesParameter WhitesRed = new("whites-red", StateParameter.Bounded(lens(Whites, Zone<ZoneAdjustment>.RedOf)), new("Whites red", "Adjustment in red"));
    public static readonly ToneZonesParameter WhitesGreen = new("whites-green", StateParameter.Bounded(lens(Whites, Zone<ZoneAdjustment>.GreenOf)), new("Whites green", "Adjustment in green"));
    public static readonly ToneZonesParameter WhitesBlue = new("whites-blue", StateParameter.Bounded(lens(Whites, Zone<ZoneAdjustment>.BlueOf)), new("Whites blue", "Adjustment in blue"));
    public static readonly ToneZonesParameter WhitesStart = new("whites-start", StateParameter.Bounded(lens(Whites, Zone<ZoneAdjustment>.StartOf)), new("Whites start", "Whites bottom in stops from middle gray"));
    public static readonly ToneZonesParameter WhitesWidth = new("whites-width", StateParameter.Bounded(lens(Whites, Zone<ZoneAdjustment>.WidthOf)), new("Whites width", "Span in stops"));
    public static readonly ToneZonesParameter SContrast = new("s-contrast", StateParameter.Bounded(Lens<ToneZones, ZoneContrast>.New(static zones => zones.SContrast, static contrast => zones => zones with { SContrast = contrast })),
        new("Contrast", "S-curve slope through tones, image unchanged at one"));

    public StateParameter<ToneZones> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record PointCurveGrade(PointCurves Curves, Fraction PreserveHue) : IStateRecord<PointCurveGrade, PointCurveGradeParameter>, IPixelStage<PointCurveGrade> {
    public static PointCurveGrade Default { get; } = new(PointCurves.Identity, Fraction.MinValue);

    public static Option<PixelPass> Pass(PointCurveGrade state, PassContext context) =>
        state.Curves == PointCurves.Identity ? None : Some<PixelPass>(new PixelPass.Color(state.Curves.Graded(state.PreserveHue)));
}

[SmartEnum<string>]
public sealed partial class PointCurveGradeParameter : IStateParameter<PointCurveGrade> {
    public static readonly PointCurveGradeParameter Red = new("red", new StateParameter<PointCurveGrade>.Keyed<PointCurve, string>(
        Lens<PointCurveGrade, PointCurve>.New(static grade => grade.Curves.Red, static curve => grade => grade with { Curves = grade.Curves with { Red = curve } })),
        new("Red", "Red curve over stops about middle gray"));
    public static readonly PointCurveGradeParameter Green = new("green", new StateParameter<PointCurveGrade>.Keyed<PointCurve, string>(
        Lens<PointCurveGrade, PointCurve>.New(static grade => grade.Curves.Green, static curve => grade => grade with { Curves = grade.Curves with { Green = curve } })),
        new("Green", "Green curve over stops about middle gray"));
    public static readonly PointCurveGradeParameter Blue = new("blue", new StateParameter<PointCurveGrade>.Keyed<PointCurve, string>(
        Lens<PointCurveGrade, PointCurve>.New(static grade => grade.Curves.Blue, static curve => grade => grade with { Curves = grade.Curves with { Blue = curve } })),
        new("Blue", "Blue curve over stops about middle gray"));
    public static readonly PointCurveGradeParameter Master = new("master", new StateParameter<PointCurveGrade>.Keyed<PointCurve, string>(
        Lens<PointCurveGrade, PointCurve>.New(static grade => grade.Curves.Master, static curve => grade => grade with { Curves = grade.Curves with { Master = curve } })),
        new("Master", "Curve every channel takes after its own"));
    public static readonly PointCurveGradeParameter PreserveHue = new("preserve-hue", StateParameter.Bounded(Lens<PointCurveGrade, Fraction>.New(static grade => grade.PreserveHue, static preserve => grade => grade with { PreserveHue = preserve })),
        new("Preserve hue", "Share of pixel hue kept through master curve"));

    public StateParameter<PointCurveGrade> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record HueCurveGrade(HueCurves Curves) : IStateRecord<HueCurveGrade, HueCurveGradeParameter>, IPixelStage<HueCurveGrade> {
    public static HueCurveGrade Default { get; } = new(HueCurves.Identity);

    public static Option<PixelPass> Pass(HueCurveGrade state, PassContext context) =>
        state.Curves == HueCurves.Identity ? None : Some<PixelPass>(new PixelPass.Color(state.Curves.Graded()));
}

[SmartEnum<string>]
public sealed partial class HueCurveGradeParameter : IStateParameter<HueCurveGrade> {
    public static readonly HueCurveGradeParameter HueHue = new("hue-hue", new StateParameter<HueCurveGrade>.Keyed<HueCurve<HueHueKind>, string>(
        Lens<HueCurveGrade, HueCurve<HueHueKind>>.New(static grade => grade.Curves.HueHue, static curve => grade => new(grade.Curves with { HueHue = curve }))),
        new("Hue vs hue", "Hue each input hue maps to"));
    public static readonly HueCurveGradeParameter HueSat = new("hue-sat", new StateParameter<HueCurveGrade>.Keyed<HueCurve<HueSatKind>, string>(
        Lens<HueCurveGrade, HueCurve<HueSatKind>>.New(static grade => grade.Curves.HueSat, static curve => grade => new(grade.Curves with { HueSat = curve }))),
        new("Hue vs saturation", "Saturation gain by input hue"));
    public static readonly HueCurveGradeParameter HueLum = new("hue-lum", new StateParameter<HueCurveGrade>.Keyed<HueCurve<HueLumKind>, string>(
        Lens<HueCurveGrade, HueCurve<HueLumKind>>.New(static grade => grade.Curves.HueLum, static curve => grade => new(grade.Curves with { HueLum = curve }))),
        new("Hue vs luma", "Luma gain by input hue"));
    public static readonly HueCurveGradeParameter LumSat = new("lum-sat", new StateParameter<HueCurveGrade>.Keyed<HueCurve<LumSatKind>, string>(
        Lens<HueCurveGrade, HueCurve<LumSatKind>>.New(static grade => grade.Curves.LumSat, static curve => grade => new(grade.Curves with { LumSat = curve }))),
        new("Luma vs saturation", "Saturation gain by luma in stops"));
    public static readonly HueCurveGradeParameter SatSat = new("sat-sat", new StateParameter<HueCurveGrade>.Keyed<HueCurve<SatSatKind>, string>(
        Lens<HueCurveGrade, HueCurve<SatSatKind>>.New(static grade => grade.Curves.SatSat, static curve => grade => new(grade.Curves with { SatSat = curve }))),
        new("Saturation vs saturation", "Saturation each input saturation maps to"));
    public static readonly HueCurveGradeParameter LumLum = new("lum-lum", new StateParameter<HueCurveGrade>.Keyed<HueCurve<LumLumKind>, string>(
        Lens<HueCurveGrade, HueCurve<LumLumKind>>.New(static grade => grade.Curves.LumLum, static curve => grade => new(grade.Curves with { LumLum = curve }))),
        new("Luma vs luma", "Luma in stops each input luma maps to"));
    public static readonly HueCurveGradeParameter SatLum = new("sat-lum", new StateParameter<HueCurveGrade>.Keyed<HueCurve<SatLumKind>, string>(
        Lens<HueCurveGrade, HueCurve<SatLumKind>>.New(static grade => grade.Curves.SatLum, static curve => grade => new(grade.Curves with { SatLum = curve }))),
        new("Saturation vs luma", "Luma gain by saturation"));

    public StateParameter<HueCurveGrade> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record GradientMapping(ColorRamp Ramp) : IStateRecord<GradientMapping, GradientMappingParameter>, IPixelStage<GradientMapping> {
    public static GradientMapping Default { get; } = new(Presets.Grayscale);

    public static Option<PixelPass> Pass(GradientMapping state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            RampLight.Of(state.Ramp, context.Working).Bind(light => new PixelPass.Color(row => GradientMap.Mapped(row, light.Lights, context.Working.Luminance)).Run(frame, progress))));
}

[SmartEnum<string>]
public sealed partial class GradientMappingParameter : IStateParameter<GradientMapping> {
    public static readonly GradientMappingParameter Ramp = new("ramp", new StateParameter<GradientMapping>.Gradient(Lens<GradientMapping, ColorRamp>.New(static map => map.Ramp, static ramp => map => map with { Ramp = ramp })),
        new("Gradient", "Colors mapped over working luminance"));

    public StateParameter<GradientMapping> Kind { get; }
    public ParameterText Text { get; }
}

public sealed record ReferenceGamutCompression(Option<Primaries> Target, Channels<CompressionThreshold> Threshold, Channels<CompressionLimit> Limit, CompressionPower Power)
    : IStateRecord<ReferenceGamutCompression, ReferenceGamutCompressionParameter>, IPixelStage<ReferenceGamutCompression> {
    public static ReferenceGamutCompression Default { get; } = Of(GamutCompression.Reference).ThrowIfFail();

    public static Fin<ReferenceGamutCompression> Of(GamutCompression compression) =>
        (Channels<CompressionThreshold>.Validated(compression.Threshold, CompressionThreshold.Validated), Channels<CompressionLimit>.Validated(compression.Limit, CompressionLimit.Validated),
         CompressionPower.Validated(compression.Power))
            .Apply(static (threshold, limit, power) => new ReferenceGamutCompression(None, threshold, limit, power))
            .As();

    public static Option<PixelPass> Pass(ReferenceGamutCompression state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            ValidationResults.ToFin(GamutCompression.Validate(
                    new Vector3(state.Threshold.Red, state.Threshold.Green, state.Threshold.Blue), new Vector3(state.Limit.Red, state.Limit.Green, state.Limit.Blue), state.Power, out GamutCompression compression),
                    compression)
                .Bind(admitted => new PixelPass.Color(admitted.Within(context.Working, state.Target.Map(static target => target.Gamut).IfNone(context.Working))).Run(frame, progress))));
}

[SmartEnum<string>]
public sealed partial class ReferenceGamutCompressionParameter : IStateParameter<ReferenceGamutCompression> {
    private static readonly Lens<ReferenceGamutCompression, Channels<CompressionThreshold>> Thresholds =
        Lens<ReferenceGamutCompression, Channels<CompressionThreshold>>.New(static state => state.Threshold, static threshold => state => state with { Threshold = threshold });
    private static readonly Lens<ReferenceGamutCompression, Channels<CompressionLimit>> Limits =
        Lens<ReferenceGamutCompression, Channels<CompressionLimit>>.New(static state => state.Limit, static limit => state => state with { Limit = limit });

    public static readonly ReferenceGamutCompressionParameter Target = new("target", new StateParameter<ReferenceGamutCompression>.OptionalChoice<Primaries>(
        Lens<ReferenceGamutCompression, Option<Primaries>>.New(static state => state.Target, static target => state => state with { Target = target }), "Working"),
        new("Gamut", "Primaries the compression measures distances in"));
    public static readonly ReferenceGamutCompressionParameter CyanThreshold = new("cyan-threshold", StateParameter.Bounded(lens(Thresholds, Channels<CompressionThreshold>.RedOf)),
        new("Cyan threshold", "Red distance from achromatic axis where compression begins"));
    public static readonly ReferenceGamutCompressionParameter MagentaThreshold = new("magenta-threshold", StateParameter.Bounded(lens(Thresholds, Channels<CompressionThreshold>.GreenOf)),
        new("Magenta threshold", "Green distance from achromatic axis where compression begins"));
    public static readonly ReferenceGamutCompressionParameter YellowThreshold = new("yellow-threshold", StateParameter.Bounded(lens(Thresholds, Channels<CompressionThreshold>.BlueOf)),
        new("Yellow threshold", "Blue distance from achromatic axis where compression begins"));
    public static readonly ReferenceGamutCompressionParameter CyanLimit = new("cyan-limit", StateParameter.Bounded(lens(Limits, Channels<CompressionLimit>.RedOf)),
        new("Cyan limit", "Red distance compressed onto the gamut boundary"));
    public static readonly ReferenceGamutCompressionParameter MagentaLimit = new("magenta-limit", StateParameter.Bounded(lens(Limits, Channels<CompressionLimit>.GreenOf)),
        new("Magenta limit", "Green distance compressed onto the gamut boundary"));
    public static readonly ReferenceGamutCompressionParameter YellowLimit = new("yellow-limit", StateParameter.Bounded(lens(Limits, Channels<CompressionLimit>.BlueOf)),
        new("Yellow limit", "Blue distance compressed onto the gamut boundary"));
    public static readonly ReferenceGamutCompressionParameter Power = new("power", StateParameter.Bounded(Lens<ReferenceGamutCompression, CompressionPower>.New(static state => state.Power, static power => state => state with { Power = power })),
        new("Power", "Compression curve exponent"));

    public StateParameter<ReferenceGamutCompression> Kind { get; }
    public ParameterText Text { get; }
}
