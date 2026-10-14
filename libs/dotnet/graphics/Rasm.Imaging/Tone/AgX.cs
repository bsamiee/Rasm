using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using TinyEXR;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ToneMapping(
    Formation Selected, Option<AgXLook> Look, FilmicView Filmic, FilmState Film, CurveApplication Application,
    SigmoidState Sigmoid, PiecewiseState Piecewise, LogarithmicState Logarithmic, PhotoreceptorState Photoreceptor,
    Gated<Exposure> ReinhardWhite, HableState Hable)
    : IStateRecord<ToneMapping, ToneMappingParameter, InvalidToneValue>, IPixelStage<ToneMapping> {
    public static ToneMapping Default { get; } = new(
        Formation.AgXView, None, FilmicView.MediumContrast, FilmPreset.Default.State, CurveApplication.Default,
        SigmoidState.Default, PiecewiseState.Default, LogarithmicState.Default, PhotoreceptorState.Default,
        new(Enabled: false, Exposure.Neutral), HableState.Default);

    public static Option<PixelPass> Pass(ToneMapping state, PassContext context) => Some(state.Selected.Pass(state, context));
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToneMappingParameter : IStateParameter<ToneMapping> {
    private static readonly Lens<ToneMapping, FilmState> Film =
        Lens<ToneMapping, FilmState>.New(static state => state.Film, static film => state => state with { Film = film });
    private static readonly Lens<ToneMapping, CurveApplication> Application =
        Lens<ToneMapping, CurveApplication>.New(static state => state.Application, static application => state => state with { Application = application });
    private static readonly Lens<ToneMapping, SigmoidState> Sigmoid =
        Lens<ToneMapping, SigmoidState>.New(static state => state.Sigmoid, static sigmoid => state => state with { Sigmoid = sigmoid });
    private static readonly Lens<ToneMapping, PiecewiseState> Piecewise =
        Lens<ToneMapping, PiecewiseState>.New(static state => state.Piecewise, static piecewise => state => state with { Piecewise = piecewise });
    private static readonly Lens<ToneMapping, LogarithmicState> Logarithmic =
        Lens<ToneMapping, LogarithmicState>.New(static state => state.Logarithmic, static logarithmic => state => state with { Logarithmic = logarithmic });
    private static readonly Lens<ToneMapping, PhotoreceptorState> Photoreceptor =
        Lens<ToneMapping, PhotoreceptorState>.New(static state => state.Photoreceptor, static photoreceptor => state => state with { Photoreceptor = photoreceptor });
    private static readonly Lens<ToneMapping, HableState> Hable =
        Lens<ToneMapping, HableState>.New(static state => state.Hable, static hable => state => state with { Hable = hable });

    private static readonly Seq<Formation> Applied = Seq(Tone.Formation.Sigmoid, Tone.Formation.HablePiecewise, Tone.Formation.Logarithmic);
    private static readonly Seq<Formation> OnFilm = Seq(Tone.Formation.Film);
    private static readonly Seq<Formation> OnSigmoid = Seq(Tone.Formation.Sigmoid);
    private static readonly Seq<Formation> OnPiecewise = Seq(Tone.Formation.HablePiecewise);
    private static readonly Seq<Formation> OnLogarithmic = Seq(Tone.Formation.Logarithmic);
    private static readonly Seq<Formation> OnPhotoreceptor = Seq(Tone.Formation.Photoreceptor);
    private static readonly Seq<Formation> OnHable = Seq(Tone.Formation.Hable);

    private static readonly Presentation<float> Share = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<float> Inset = new() { Soft = (0f, 0.5f) };
    private static readonly Presentation<float> Turn = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };
    private static readonly Presentation<float> FilmStops = Exposure.Presentation with { Form = NumberForm.Field };
    private static readonly Presentation<float> FilmShare = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };
    private static readonly Presentation<float> FilmScale = new() { Form = NumberForm.Field };
    private static readonly Presentation<float> FilmTurn = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };
    private static readonly Presentation<float> FilmGains = new() { Form = NumberForm.Field };
    private static readonly Presentation<float> FilmFraction = new() { Form = NumberForm.Field };
    private static readonly Presentation<float> FilmDensity = new() { Form = NumberForm.Field };

    public static readonly ToneMappingParameter Formation = new("formation", new StateParameter<ToneMapping>.Choice<Formation, InvalidToneValue>(
        Lens<ToneMapping, Formation>.New(static state => state.Selected, static selected => state => state with { Selected = selected })), toSeq(Tone.Formation.Items));
    public static readonly ToneMappingParameter Look = new("look", new StateParameter<ToneMapping>.OptionalChoice<AgXLook, InvalidToneValue>(
        Lens<ToneMapping, Option<AgXLook>>.New(static state => state.Look, static look => state => state with { Look = look })),
        Seq(Tone.Formation.AgXView, Tone.Formation.AgXFalseColor));
    public static readonly ToneMappingParameter FilmicView = new("filmic-view", new StateParameter<ToneMapping>.Choice<FilmicView, InvalidToneValue>(
        Lens<ToneMapping, FilmicView>.New(static state => state.Filmic, static filmic => state => state with { Filmic = filmic })),
        Seq(Tone.Formation.Filmic));
    public static readonly ToneMappingParameter FilmPreExposure = new("film-pre-exposure", StateParameter.Bounded<ToneMapping, Exposure, float, InvalidToneValue>(lens(Film, Lens<FilmState, Exposure>.New(static film => film.PreExposure, static value => film => film with { PreExposure = value })), FilmStops), OnFilm);
    public static readonly ToneMappingParameter FilmPreFilterRed = new("film-pre-filter-red", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Film, Lens<FilmState, Factor>.New(static film => film.PreFilterRed, static value => film => film with { PreFilterRed = value })), FilmShare), OnFilm);
    public static readonly ToneMappingParameter FilmPreFilterGreen = new("film-pre-filter-green", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Film, Lens<FilmState, Factor>.New(static film => film.PreFilterGreen, static value => film => film with { PreFilterGreen = value })), FilmShare), OnFilm);
    public static readonly ToneMappingParameter FilmPreFilterBlue = new("film-pre-filter-blue", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Film, Lens<FilmState, Factor>.New(static film => film.PreFilterBlue, static value => film => film with { PreFilterBlue = value })), FilmShare), OnFilm);
    public static readonly ToneMappingParameter FilmRedScale = new("film-red-scale", StateParameter.Bounded<ToneMapping, PrimaryScale, float, InvalidToneValue>(lens(Film, Lens<FilmState, PrimaryScale>.New(static film => film.RedScale, static value => film => film with { RedScale = value })), FilmScale), OnFilm);
    public static readonly ToneMappingParameter FilmGreenScale = new("film-green-scale", StateParameter.Bounded<ToneMapping, PrimaryScale, float, InvalidToneValue>(lens(Film, Lens<FilmState, PrimaryScale>.New(static film => film.GreenScale, static value => film => film with { GreenScale = value })), FilmScale), OnFilm);
    public static readonly ToneMappingParameter FilmBlueScale = new("film-blue-scale", StateParameter.Bounded<ToneMapping, PrimaryScale, float, InvalidToneValue>(lens(Film, Lens<FilmState, PrimaryScale>.New(static film => film.BlueScale, static value => film => film with { BlueScale = value })), FilmScale), OnFilm);
    public static readonly ToneMappingParameter FilmRedRotation = new("film-red-rotation", StateParameter.Bounded<ToneMapping, HueRotation, float, InvalidToneValue>(lens(Film, Lens<FilmState, HueRotation>.New(static film => film.RedRotation, static value => film => film with { RedRotation = value })), FilmTurn), OnFilm);
    public static readonly ToneMappingParameter FilmGreenRotation = new("film-green-rotation", StateParameter.Bounded<ToneMapping, HueRotation, float, InvalidToneValue>(lens(Film, Lens<FilmState, HueRotation>.New(static film => film.GreenRotation, static value => film => film with { GreenRotation = value })), FilmTurn), OnFilm);
    public static readonly ToneMappingParameter FilmBlueRotation = new("film-blue-rotation", StateParameter.Bounded<ToneMapping, HueRotation, float, InvalidToneValue>(lens(Film, Lens<FilmState, HueRotation>.New(static film => film.BlueRotation, static value => film => film with { BlueRotation = value })), FilmTurn), OnFilm);
    public static readonly ToneMappingParameter FilmRedMultiplier = new("film-red-multiplier", StateParameter.Bounded<ToneMapping, FilmGain, float, InvalidToneValue>(lens(Film, Lens<FilmState, FilmGain>.New(static film => film.RedMultiplier, static value => film => film with { RedMultiplier = value })), FilmGains), OnFilm);
    public static readonly ToneMappingParameter FilmGreenMultiplier = new("film-green-multiplier", StateParameter.Bounded<ToneMapping, FilmGain, float, InvalidToneValue>(lens(Film, Lens<FilmState, FilmGain>.New(static film => film.GreenMultiplier, static value => film => film with { GreenMultiplier = value })), FilmGains), OnFilm);
    public static readonly ToneMappingParameter FilmBlueMultiplier = new("film-blue-multiplier", StateParameter.Bounded<ToneMapping, FilmGain, float, InvalidToneValue>(lens(Film, Lens<FilmState, FilmGain>.New(static film => film.BlueMultiplier, static value => film => film with { BlueMultiplier = value })), FilmGains), OnFilm);
    public static readonly ToneMappingParameter FilmSigmoidMinimum = new("film-sigmoid-minimum", StateParameter.Bounded<ToneMapping, Exposure, float, InvalidToneValue>(lens(Film, Lens<FilmState, Exposure>.New(static film => film.SigmoidMinimum, static value => film => film with { SigmoidMinimum = value })), FilmStops), OnFilm);
    public static readonly ToneMappingParameter FilmSigmoidSpan = new("film-sigmoid-span", StateParameter.Bounded<ToneMapping, FilmSpan, float, InvalidToneValue>(lens(Film, Lens<FilmState, FilmSpan>.New(static film => film.SigmoidSpan, static value => film => film with { SigmoidSpan = value })), new() { Form = NumberForm.Field }), OnFilm);
    public static readonly ToneMappingParameter FilmToeX = new("film-toe-x", StateParameter.Bounded<ToneMapping, CurveFraction, float, InvalidToneValue>(lens(Film, Lens<FilmState, CurveFraction>.New(static film => film.ToeX, static value => film => film with { ToeX = value })), FilmFraction), OnFilm);
    public static readonly ToneMappingParameter FilmToeY = new("film-toe-y", StateParameter.Bounded<ToneMapping, CurveFraction, float, InvalidToneValue>(lens(Film, Lens<FilmState, CurveFraction>.New(static film => film.ToeY, static value => film => film with { ToeY = value })), FilmFraction), OnFilm);
    public static readonly ToneMappingParameter FilmShoulderReachX = new("film-shoulder-reach-x", StateParameter.Bounded<ToneMapping, CurveFraction, float, InvalidToneValue>(lens(Film, Lens<FilmState, CurveFraction>.New(static film => film.ShoulderReachX, static value => film => film with { ShoulderReachX = value })), FilmFraction), OnFilm);
    public static readonly ToneMappingParameter FilmShoulderReachY = new("film-shoulder-reach-y", StateParameter.Bounded<ToneMapping, CurveFraction, float, InvalidToneValue>(lens(Film, Lens<FilmState, CurveFraction>.New(static film => film.ShoulderReachY, static value => film => film with { ShoulderReachY = value })), FilmFraction), OnFilm);
    public static readonly ToneMappingParameter FilmNegativeExposure = new("film-negative-exposure", StateParameter.Bounded<ToneMapping, Exposure, float, InvalidToneValue>(lens(Film, Lens<FilmState, Exposure>.New(static film => film.NegativeExposure, static value => film => film with { NegativeExposure = value })), FilmStops), OnFilm);
    public static readonly ToneMappingParameter FilmNegativeDensity = new("film-negative-density", StateParameter.Bounded<ToneMapping, Density, float, InvalidToneValue>(lens(Film, Lens<FilmState, Density>.New(static film => film.NegativeDensity, static value => film => film with { NegativeDensity = value })), FilmDensity), OnFilm);
    public static readonly ToneMappingParameter FilmBacklightRed = new("film-backlight-red", StateParameter.Bounded<ToneMapping, FilmGain, float, InvalidToneValue>(lens(Film, Lens<FilmState, FilmGain>.New(static film => film.BacklightRed, static value => film => film with { BacklightRed = value })), FilmGains), OnFilm);
    public static readonly ToneMappingParameter FilmBacklightGreen = new("film-backlight-green", StateParameter.Bounded<ToneMapping, FilmGain, float, InvalidToneValue>(lens(Film, Lens<FilmState, FilmGain>.New(static film => film.BacklightGreen, static value => film => film with { BacklightGreen = value })), FilmGains), OnFilm);
    public static readonly ToneMappingParameter FilmBacklightBlue = new("film-backlight-blue", StateParameter.Bounded<ToneMapping, FilmGain, float, InvalidToneValue>(lens(Film, Lens<FilmState, FilmGain>.New(static film => film.BacklightBlue, static value => film => film with { BacklightBlue = value })), FilmGains), OnFilm);
    public static readonly ToneMappingParameter FilmPrintExposure = new("film-print-exposure", StateParameter.Bounded<ToneMapping, Exposure, float, InvalidToneValue>(lens(Film, Lens<FilmState, Exposure>.New(static film => film.PrintExposure, static value => film => film with { PrintExposure = value })), FilmStops), OnFilm);
    public static readonly ToneMappingParameter FilmPrintDensity = new("film-print-density", StateParameter.Bounded<ToneMapping, Density, float, InvalidToneValue>(lens(Film, Lens<FilmState, Density>.New(static film => film.PrintDensity, static value => film => film with { PrintDensity = value })), FilmDensity), OnFilm);
    public static readonly ToneMappingParameter FilmBlackPoint = new("film-black-point", StateParameter.OptionalBounded<ToneMapping, BlackOffset, float, InvalidToneValue>(lens(Film, Lens<FilmState, Gated<BlackOffset>>.New(static film => film.BlackPoint, static value => film => film with { BlackPoint = value })), new() { Form = NumberForm.Field }), OnFilm);
    public static readonly ToneMappingParameter FilmPostFilterRed = new("film-post-filter-red", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Film, Lens<FilmState, Factor>.New(static film => film.PostFilterRed, static value => film => film with { PostFilterRed = value })), FilmShare), OnFilm);
    public static readonly ToneMappingParameter FilmPostFilterGreen = new("film-post-filter-green", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Film, Lens<FilmState, Factor>.New(static film => film.PostFilterGreen, static value => film => film with { PostFilterGreen = value })), FilmShare), OnFilm);
    public static readonly ToneMappingParameter FilmPostFilterBlue = new("film-post-filter-blue", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Film, Lens<FilmState, Factor>.New(static film => film.PostFilterBlue, static value => film => film with { PostFilterBlue = value })), FilmShare), OnFilm);
    public static readonly ToneMappingParameter FilmMidtoneSaturation = new("film-midtone-saturation", StateParameter.Bounded<ToneMapping, MidtoneSaturation, float, InvalidToneValue>(lens(Film, Lens<FilmState, MidtoneSaturation>.New(static film => film.MidtoneSaturation, static value => film => film with { MidtoneSaturation = value })), new() { Form = NumberForm.Field }), OnFilm);
    public static readonly ToneMappingParameter PreserveHue = new("preserve-hue", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, Factor>.New(static application => application.PreserveHue, static value => application => application with { PreserveHue = value })), Share), Applied);
    public static readonly ToneMappingParameter Purity = new("purity", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, Factor>.New(static application => application.Purity, static value => application => application with { Purity = value })), Share), Applied);
    public static readonly ToneMappingParameter BasePrimaries = new("base-primaries", new StateParameter<ToneMapping>.OptionalChoice<Gamut, InvalidColor>(
        lens(Application, Lens<CurveApplication, Option<Gamut>>.New(static application => application.BasePrimaries, static value => application => application with { BasePrimaries = value }))), Applied);
    public static readonly ToneMappingParameter RedInset = new("red-inset", StateParameter.Bounded<ToneMapping, PrimaryInset, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, PrimaryInset>.New(static application => application.RedInset, static value => application => application with { RedInset = value })), Inset), Applied);
    public static readonly ToneMappingParameter GreenInset = new("green-inset", StateParameter.Bounded<ToneMapping, PrimaryInset, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, PrimaryInset>.New(static application => application.GreenInset, static value => application => application with { GreenInset = value })), Inset), Applied);
    public static readonly ToneMappingParameter BlueInset = new("blue-inset", StateParameter.Bounded<ToneMapping, PrimaryInset, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, PrimaryInset>.New(static application => application.BlueInset, static value => application => application with { BlueInset = value })), Inset), Applied);
    public static readonly ToneMappingParameter RedRotation = new("red-rotation", StateParameter.Bounded<ToneMapping, PrimaryRotation, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, PrimaryRotation>.New(static application => application.RedRotation, static value => application => application with { RedRotation = value })), Turn), Applied);
    public static readonly ToneMappingParameter GreenRotation = new("green-rotation", StateParameter.Bounded<ToneMapping, PrimaryRotation, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, PrimaryRotation>.New(static application => application.GreenRotation, static value => application => application with { GreenRotation = value })), Turn), Applied);
    public static readonly ToneMappingParameter BlueRotation = new("blue-rotation", StateParameter.Bounded<ToneMapping, PrimaryRotation, float, InvalidToneValue>(lens(Application, Lens<CurveApplication, PrimaryRotation>.New(static application => application.BlueRotation, static value => application => application with { BlueRotation = value })), Turn), Applied);
    public static readonly ToneMappingParameter SigmoidContrast = new("sigmoid-contrast", StateParameter.Bounded<ToneMapping, MiddleGreyContrast, float, InvalidToneValue>(lens(Sigmoid, Lens<SigmoidState, MiddleGreyContrast>.New(static sigmoid => sigmoid.Contrast, static value => sigmoid => sigmoid with { Contrast = value })), new() { Soft = (0.7f, 3f) }), OnSigmoid);
    public static readonly ToneMappingParameter SigmoidSkew = new("sigmoid-skew", StateParameter.Bounded<ToneMapping, ContrastSkewness, float, InvalidToneValue>(lens(Sigmoid, Lens<SigmoidState, ContrastSkewness>.New(static sigmoid => sigmoid.Skew, static value => sigmoid => sigmoid with { Skew = value })), new()), OnSigmoid);
    public static readonly ToneMappingParameter SigmoidBlack = new("sigmoid-black", StateParameter.Bounded<ToneMapping, TargetBlack, float, InvalidToneValue>(lens(Sigmoid, Lens<SigmoidState, TargetBlack>.New(static sigmoid => sigmoid.Black, static value => sigmoid => sigmoid with { Black = value })),
    new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction), Soft = (0f, 0.01f) }), OnSigmoid);
    public static readonly ToneMappingParameter PiecewiseToeStrength = new("piecewise-toe-strength", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Piecewise, Lens<PiecewiseState, Factor>.New(static piecewise => piecewise.ToeStrength, static value => piecewise => piecewise with { ToeStrength = value })), Share), OnPiecewise);
    public static readonly ToneMappingParameter PiecewiseToeLength = new("piecewise-toe-length", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Piecewise, Lens<PiecewiseState, Factor>.New(static piecewise => piecewise.ToeLength, static value => piecewise => piecewise with { ToeLength = value })), Share), OnPiecewise);
    public static readonly ToneMappingParameter PiecewiseShoulderAngle = new("piecewise-shoulder-angle", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Piecewise, Lens<PiecewiseState, Factor>.New(static piecewise => piecewise.ShoulderAngle, static value => piecewise => piecewise with { ShoulderAngle = value })), Share), OnPiecewise);
    public static readonly ToneMappingParameter PiecewiseGamma = new("piecewise-gamma", StateParameter.Bounded<ToneMapping, CurveGamma, float, InvalidToneValue>(lens(Piecewise, Lens<PiecewiseState, CurveGamma>.New(static piecewise => piecewise.Gamma, static value => piecewise => piecewise with { Gamma = value })), new()), OnPiecewise);
    public static readonly ToneMappingParameter PiecewiseShoulderStrength = new("piecewise-shoulder-strength", StateParameter.Bounded<ToneMapping, ShoulderStrength, float, InvalidToneValue>(lens(Piecewise, Lens<PiecewiseState, ShoulderStrength>.New(static piecewise => piecewise.ShoulderStrength, static value => piecewise => piecewise with { ShoulderStrength = value })), new()), OnPiecewise);
    public static readonly ToneMappingParameter PiecewiseShoulderLength = new("piecewise-shoulder-length", StateParameter.Bounded<ToneMapping, ShoulderLength, float, InvalidToneValue>(lens(Piecewise, Lens<PiecewiseState, ShoulderLength>.New(static piecewise => piecewise.ShoulderLength, static value => piecewise => piecewise with { ShoulderLength = value })), new()), OnPiecewise);
    public static readonly ToneMappingParameter LogarithmicBias = new("logarithmic-bias", StateParameter.Bounded<ToneMapping, DragoBias, float, InvalidToneValue>(lens(Logarithmic, Lens<LogarithmicState, DragoBias>.New(static logarithmic => logarithmic.Bias, static value => logarithmic => logarithmic with { Bias = value })), new() { Soft = (0.7f, 0.9f) }), OnLogarithmic);
    public static readonly ToneMappingParameter LogarithmicWhite = new("logarithmic-white", StateParameter.OptionalBounded<ToneMapping, Exposure, float, InvalidToneValue>(lens(Logarithmic, Lens<LogarithmicState, Gated<Exposure>>.New(static logarithmic => logarithmic.White, static value => logarithmic => logarithmic with { White = value })), Exposure.Presentation), OnLogarithmic);
    public static readonly ToneMappingParameter PhotoreceptorIntensity = new("photoreceptor-intensity", StateParameter.Bounded<ToneMapping, ReceptorIntensity, float, InvalidToneValue>(lens(Photoreceptor, Lens<PhotoreceptorState, ReceptorIntensity>.New(static photoreceptor => photoreceptor.Intensity, static value => photoreceptor => photoreceptor with { Intensity = value })), new() { Soft = (-8f, 8f) }), OnPhotoreceptor);
    public static readonly ToneMappingParameter PhotoreceptorContrast = new("photoreceptor-contrast", StateParameter.OptionalBounded<ToneMapping, ReceptorContrast, float, InvalidToneValue>(lens(Photoreceptor, Lens<PhotoreceptorState, Gated<ReceptorContrast>>.New(static photoreceptor => photoreceptor.Contrast, static value => photoreceptor => photoreceptor with { Contrast = value })), new() { Soft = (ReceptorContrast.Flattest, 1f) }), OnPhotoreceptor);
    public static readonly ToneMappingParameter PhotoreceptorLightAdaptation = new("photoreceptor-light-adaptation", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Photoreceptor, Lens<PhotoreceptorState, Factor>.New(static photoreceptor => photoreceptor.LightAdaptation, static value => photoreceptor => photoreceptor with { LightAdaptation = value })), Share), OnPhotoreceptor);
    public static readonly ToneMappingParameter PhotoreceptorChromaticAdaptation = new("photoreceptor-chromatic-adaptation", StateParameter.Bounded<ToneMapping, Factor, float, InvalidToneValue>(lens(Photoreceptor, Lens<PhotoreceptorState, Factor>.New(static photoreceptor => photoreceptor.ChromaticAdaptation, static value => photoreceptor => photoreceptor with { ChromaticAdaptation = value })), Share), OnPhotoreceptor);
    public static readonly ToneMappingParameter ReinhardWhite = new("reinhard-white", StateParameter.OptionalBounded<ToneMapping, Exposure, float, InvalidToneValue>(Lens<ToneMapping, Gated<Exposure>>.New(static state => state.ReinhardWhite, static white => state => state with { ReinhardWhite = white }), Exposure.Presentation),
        Seq(Tone.Formation.ReinhardExtended));
    public static readonly ToneMappingParameter HableA = new("hable-a", StateParameter.Bounded<ToneMapping, ShoulderCoefficient, float, InvalidToneValue>(lens(Hable, Lens<HableState, ShoulderCoefficient>.New(static hable => hable.A, static value => hable => hable with { A = value })), new()), OnHable);
    public static readonly ToneMappingParameter HableB = new("hable-b", StateParameter.Bounded<ToneMapping, LinearCoefficient, float, InvalidToneValue>(lens(Hable, Lens<HableState, LinearCoefficient>.New(static hable => hable.B, static value => hable => hable with { B = value })), new()), OnHable);
    public static readonly ToneMappingParameter HableC = new("hable-c", StateParameter.Bounded<ToneMapping, LinearAngle, float, InvalidToneValue>(lens(Hable, Lens<HableState, LinearAngle>.New(static hable => hable.C, static value => hable => hable with { C = value })), new()), OnHable);
    public static readonly ToneMappingParameter HableD = new("hable-d", StateParameter.Bounded<ToneMapping, ToeCoefficient, float, InvalidToneValue>(lens(Hable, Lens<HableState, ToeCoefficient>.New(static hable => hable.D, static value => hable => hable with { D = value })), new()), OnHable);
    public static readonly ToneMappingParameter HableE = new("hable-e", StateParameter.Bounded<ToneMapping, ToeNumerator, float, InvalidToneValue>(lens(Hable, Lens<HableState, ToeNumerator>.New(static hable => hable.E, static value => hable => hable with { E = value })), new()), OnHable);
    public static readonly ToneMappingParameter HableF = new("hable-f", StateParameter.Bounded<ToneMapping, ToeDenominator, float, InvalidToneValue>(lens(Hable, Lens<HableState, ToeDenominator>.New(static hable => hable.F, static value => hable => hable with { F = value })), new()), OnHable);
    public static readonly ToneMappingParameter HableWhite = new("hable-white", StateParameter.OptionalBounded<ToneMapping, Exposure, float, InvalidToneValue>(lens(Hable, Lens<HableState, Gated<Exposure>>.New(static hable => hable.White, static value => hable => hable with { White = value })), Exposure.Presentation), OnHable);

    public StateParameter<ToneMapping> Kind { get; }
    public Seq<Formation> Formations { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Formation {
    public static readonly Formation AgXView = new("agx", AgX.View);
    public static readonly Formation AgXFalseColor = new("agx-false-color", AgX.FalseColor);
    public static readonly Formation Aces = new("aces", AcesTransform.Formed);
    public static readonly Formation Filmic = new("filmic", FilmicTransform.Formed);
    public static readonly Formation Film = new("film", FilmTransform.Formed);
    public static readonly Formation Tony = new("tony", TonyTransform.Formed);
    public static readonly Formation Sigmoid = new("sigmoid", ToneCurves.Sigmoid);
    public static readonly Formation HablePiecewise = new("hable-piecewise", ToneCurves.Piecewise);
    public static readonly Formation Logarithmic = new("logarithmic", ToneCurves.Logarithmic);
    public static readonly Formation Photoreceptor = new("photoreceptor", ToneCurves.Photoreceptor);
    public static readonly Formation Reinhard = new("reinhard", ToneCurves.Reinhard);
    public static readonly Formation ReinhardExtended = new("reinhard-extended", ToneCurves.ReinhardExtended);
    public static readonly Formation Hable = new("hable", ToneCurves.Hable);
    public static readonly Formation PbrNeutral = new("pbr-neutral", ToneCurves.PbrNeutral);

    [UseDelegateFromConstructor]
    public partial PixelPass Pass(ToneMapping state, PassContext context);

    internal static PixelPass.Color Formed(Seq<Action<Span<Vector4>>> steps, ColorEncoding signal, Display display) {
        Action<Span<Vector4>>[] chain = [.. steps, .. signal.To(display.Encoding)];
        return new(row => {
            foreach (Action<Span<Vector4>> step in chain)
                step(row);
        });
    }
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Display {
    public static readonly Display Srgb = new("srgb", new ColorEncoding(Gamut.StandardRgb, TransferCurve.Srgb, Nits.ReferenceWhite), Nits.ReferenceWhite);
    public static readonly Display DisplayP3 = new("display-p3", new ColorEncoding(Gamut.DisplayP3, TransferCurve.Srgb, Nits.ReferenceWhite), Nits.ReferenceWhite);
    public static readonly Display Rec1886 = new("rec1886", new ColorEncoding(Gamut.StandardRgb, GammaExponent.Rec1886, Nits.ReferenceWhite), Nits.ReferenceWhite);
    public static readonly Display Rec2020 = new("rec2020", new ColorEncoding(Gamut.Rec2020, GammaExponent.Rec1886, Nits.ReferenceWhite), Nits.ReferenceWhite);
    public static readonly Display Rec2100PqHdr = new("rec2100-pq-hdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Pq, Nits.FileWhite), Nits.HlgPeak);
    public static readonly Display Rec2100PqSdr = new("rec2100-pq-sdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Pq, Nits.FileWhite), Nits.ReferenceWhite);
    public static readonly Display Rec2100HlgHdr = new("rec2100-hlg-hdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Hlg, Nits.FileWhite), Nits.HlgPeak);
    public static readonly Display Rec2100HlgSdr = new("rec2100-hlg-sdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Hlg, Nits.FileWhite), Nits.ReferenceWhite);

    public ColorEncoding Encoding { get; }
    public Nits Peak { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AgXLook {
    public static readonly AgXLook Punchy = new("punchy", (GradingStyle.Log.Neutral with { Shadows = new ZoneSpan(new Vector4(0.2f, 0.2f, 0.2f, 0.35f), 0.4f, 0.3f) })
        .Kernel(GradingStyle.Log).ToSeq()
        .Add(new LutTable.Cdl(CdlStyle.FwdNoClamp, Vector3.One, Vector3.Zero, new Vector3(1.0912f), 1f).Apply));
    public static readonly AgXLook Greyscale = new("greyscale", Seq(new LutTable.Antilog(LogSpace.AgXLog.Allocation).Apply, AgX.Grey.Apply, new LutTable.Log(LogSpace.AgXLog.Allocation).Apply));
    public static readonly AgXLook VeryHighContrast = new("very-high-contrast", Contrasted(1.57f, 0.9f));
    public static readonly AgXLook HighContrast = new("high-contrast", Contrasted(1.4f, 0.95f));
    public static readonly AgXLook MediumHighContrast = new("medium-high-contrast", Contrasted(1.2f, 1f));
    public static readonly AgXLook BaseContrast = new("base-contrast", Contrasted(1f, 1f));
    public static readonly AgXLook MediumLowContrast = new("medium-low-contrast", Contrasted(0.9f, 1.05f));
    public static readonly AgXLook LowContrast = new("low-contrast", Contrasted(0.8f, 1.1f));
    public static readonly AgXLook VeryLowContrast = new("very-low-contrast", Contrasted(0.7f, 1.15f));

    public Seq<Action<Span<Vector4>>> Steps { get; }

    private static Seq<Action<Span<Vector4>>> Contrasted(float contrast, float saturation) =>
        Seq(new LutTable.Cdl(CdlStyle.FwdNoClamp, new Vector3(contrast), new Vector3(0.4f * (1f - contrast)), Vector3.One, saturation).Apply);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AgX {
    public static Lut1D FalseColorBands => EmbeddedTables.Curve("AgX_False_Color.spi1d");

    internal static LutTable Grey { get; } = (X: 0.2589235355689848d, Y: 0.6104985346066525d, Z: 0.13057792982436284d) switch {
        var weights => new LutTable.Affine(Matrix<double>.Build.Dense(3, 3, (_, column) => column switch { 0 => weights.X, 1 => weights.Y, _ => weights.Z }), (0d, 0d, 0d)),
    };

    private static readonly (string Cube, ColorEncoding Signal) BaseRec1886 = ("AgX_Base_sRGB.cube", new(Gamut.StandardRgb, GammaExponent.Rec1886, Nits.ReferenceWhite));
    private static readonly (string Cube, ColorEncoding Signal) BaseDisplayP3 = ("AgX_Base_P3.cube", new(Gamut.DisplayP3, GammaExponent.Rec1886, Nits.ReferenceWhite));
    private static readonly (string Cube, ColorEncoding Signal) BaseRec2020 = ("AgX_Base_Rec2020.cube", new(Gamut.Rec2020, GammaExponent.Rec1886, Nits.ReferenceWhite));
    private static readonly (string Cube, ColorEncoding Signal) Rec2100Hlg = ("AgX_Rec2100-HLG_p3_lim.cube", new(Gamut.Rec2020, TransferCurve.Hlg, Nits.ReferenceWhite));

    public static Lut3D Cube(Display display) => EmbeddedTables.Lattice(Output(display).Cube);

    internal static PixelPass View(ToneMapping state, PassContext context) =>
        Formation.Formed(Opened(state.Look, context.Working).Add(new LutTable.Cube(Cube(context.Display), LutInterpolation.Tetrahedral).Apply), Output(context.Display).Signal, context.Display);

    internal static PixelPass FalseColor(ToneMapping state, PassContext context) =>
        Formation.Formed(ToBandPosition(state.Look, context.Working).Add(new LutTable.ChannelCurve(FalseColorBands).Apply), BaseRec1886.Signal, context.Display);

    internal static Seq<Action<Span<Vector4>>> ToBandPosition(Option<AgXLook> look, Gamut working) =>
        Opened(look, working)
            + Seq(
                new LutTable.Cube(Cube(Display.Rec2020), LutInterpolation.Tetrahedral).Apply,
                BaseRec2020.Signal.Decode,
                Grey.Apply,
                new LutTable.Exponent(ExponentStyle.BasicRev, new Vector3(2.5f), Vector3.Zero).Apply);

    private static Seq<Action<Span<Vector4>>> Opened(Option<AgXLook> look, Gamut working) =>
        LutTables.Between(working, LogSpace.AgXLog.Gamut).Map<Action<Span<Vector4>>>(static step => step.Apply)
        + look.Match(
            Some: static chosen => LogSpace.AgXLog.Into.Map<Action<Span<Vector4>>>(static step => step.Apply) + chosen.Steps + LogSpace.AgXLog.Back.Map<Action<Span<Vector4>>>(static step => step.Apply),
            None: static () => Seq<Action<Span<Vector4>>>())
        + Seq(new LutTable.Log(LogSpace.AgXLog.Allocation).Apply);

    private static (string Cube, ColorEncoding Signal) Output(Display display) =>
        display.Map(
            srgb: BaseRec1886, displayP3: BaseDisplayP3, rec1886: BaseRec1886, rec2020: BaseRec2020,
            rec2100PqHdr: Rec2100Hlg, rec2100PqSdr: BaseDisplayP3, rec2100HlgHdr: Rec2100Hlg, rec2100HlgSdr: BaseDisplayP3);
}
