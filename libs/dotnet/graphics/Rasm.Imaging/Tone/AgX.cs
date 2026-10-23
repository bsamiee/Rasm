using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
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
    : IStateRecord<ToneMapping, ToneMappingParameter>, IPixelStage<ToneMapping> {
    public static ToneMapping Default { get; } = new(
        Formation.AgXView, None, FilmicView.MediumContrast, FilmPreset.Default.State, CurveApplication.Default,
        SigmoidState.Default, PiecewiseState.Default, LogarithmicState.Default, PhotoreceptorState.Default,
        new(Enabled: false, Exposure.Neutral), HableState.Default);

    public static Option<PixelPass> Pass(ToneMapping state, PassContext context) => Some(state.Selected.Pass(state, context));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToneMappingParameter : IStateParameter<ToneMapping> {
    private static readonly Section<FilmState> Film = new(Lens<ToneMapping, FilmState>.New(static state => state.Film, static film => state => state with { Film = film }), Seq(Tone.Formation.Film));
    private static readonly Section<CurveApplication> Application = new(Lens<ToneMapping, CurveApplication>.New(static state => state.Application, static application => state => state with { Application = application }), Seq(Tone.Formation.Sigmoid, Tone.Formation.HablePiecewise, Tone.Formation.Logarithmic));
    private static readonly Section<SigmoidState> Sigmoid = new(Lens<ToneMapping, SigmoidState>.New(static state => state.Sigmoid, static sigmoid => state => state with { Sigmoid = sigmoid }), Seq(Tone.Formation.Sigmoid));
    private static readonly Section<PiecewiseState> Piecewise = new(Lens<ToneMapping, PiecewiseState>.New(static state => state.Piecewise, static piecewise => state => state with { Piecewise = piecewise }), Seq(Tone.Formation.HablePiecewise));
    private static readonly Section<LogarithmicState> Logarithmic = new(Lens<ToneMapping, LogarithmicState>.New(static state => state.Logarithmic, static logarithmic => state => state with { Logarithmic = logarithmic }), Seq(Tone.Formation.Logarithmic));
    private static readonly Section<PhotoreceptorState> Photoreceptor = new(Lens<ToneMapping, PhotoreceptorState>.New(static state => state.Photoreceptor, static photoreceptor => state => state with { Photoreceptor = photoreceptor }), Seq(Tone.Formation.Photoreceptor));
    private static readonly Section<HableState> Hable = new(Lens<ToneMapping, HableState>.New(static state => state.Hable, static hable => state => state with { Hable = hable }), Seq(Tone.Formation.Hable));

    private static readonly Presentation<float> Inset = new() { Soft = (0f, 0.5f) };
    private static readonly Presentation<float> Turn = new() { Unit = Quantity.GetUnitInfo(AngleUnit.Radian) };
    private static readonly Presentation<float> Field = new() { Form = NumberForm.Field };
    private static readonly Presentation<float> FilmStops = Exposure.Presentation with { Form = NumberForm.Field };
    private static readonly Presentation<float> FilmShare = AxisFraction.Presentation with { Form = NumberForm.Field };
    private static readonly Presentation<float> FilmTurn = Turn with { Form = NumberForm.Field };

    public static readonly ToneMappingParameter Formation = new("formation", new StateParameter<ToneMapping>.Choice<Formation>(Lens<ToneMapping, Formation>.New(static state => state.Selected, static selected => state => state with { Selected = selected })), toSeq(Tone.Formation.Items),
        new("View transform", "Scene light to display code formation"));
    public static readonly ToneMappingParameter Look = new("look", new StateParameter<ToneMapping>.OptionalChoice<AgXLook>(Lens<ToneMapping, Option<AgXLook>>.New(static state => state.Look, static look => state => state with { Look = look }), "None"), Seq(Tone.Formation.AgXView, Tone.Formation.AgXFalseColor),
        new("Look", "Look in AgX Log before AgX and AgX false color views"));
    public static readonly ToneMappingParameter FilmicView = new("filmic-view", new StateParameter<ToneMapping>.Choice<FilmicView>(Lens<ToneMapping, FilmicView>.New(static state => state.Filmic, static filmic => state => state with { Filmic = filmic })), Seq(Tone.Formation.Filmic),
        new("Filmic contrast", "Contrast look, Log shows base log encoding"));
    public static readonly ToneMappingParameter FilmPreExposure = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, Exposure>.New(static film => film.PreExposure, static value => film => film with { PreExposure = value })), FilmStops),
        new("Pre-exposure", "Stops before film stock"));
    public static readonly ToneMappingParameter FilmPreFilterRed = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, AxisFraction>.New(static film => film.PreFilterRed, static value => film => film with { PreFilterRed = value })), FilmShare),
        new("Pre-filter red", "Red transmittance of lens filter"));
    public static readonly ToneMappingParameter FilmPreFilterGreen = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, AxisFraction>.New(static film => film.PreFilterGreen, static value => film => film with { PreFilterGreen = value })), FilmShare),
        new("Pre-filter green", "Green transmittance of lens filter"));
    public static readonly ToneMappingParameter FilmPreFilterBlue = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, AxisFraction>.New(static film => film.PreFilterBlue, static value => film => film with { PreFilterBlue = value })), FilmShare),
        new("Pre-filter blue", "Blue transmittance of lens filter"));
    public static readonly ToneMappingParameter FilmRedScale = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PrimaryScale>.New(static film => film.RedScale, static value => film => film with { RedScale = value })), Field),
        new("Red scale", "Red primary gamut extension"));
    public static readonly ToneMappingParameter FilmGreenScale = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PrimaryScale>.New(static film => film.GreenScale, static value => film => film with { GreenScale = value })), Field),
        new("Green scale", "Green primary gamut extension"));
    public static readonly ToneMappingParameter FilmBlueScale = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PrimaryScale>.New(static film => film.BlueScale, static value => film => film with { BlueScale = value })), Field),
        new("Blue scale", "Blue primary gamut extension"));
    public static readonly ToneMappingParameter FilmRedRotation = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, HueRotation>.New(static film => film.RedRotation, static value => film => film with { RedRotation = value })), FilmTurn),
        new("Red rotation", "Red primary hue turn"));
    public static readonly ToneMappingParameter FilmGreenRotation = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, HueRotation>.New(static film => film.GreenRotation, static value => film => film with { GreenRotation = value })), FilmTurn),
        new("Green rotation", "Green primary hue turn"));
    public static readonly ToneMappingParameter FilmBlueRotation = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, HueRotation>.New(static film => film.BlueRotation, static value => film => film with { BlueRotation = value })), FilmTurn),
        new("Blue rotation", "Blue primary hue turn"));
    public static readonly ToneMappingParameter FilmRedMultiplier = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PositiveMagnitude>.New(static film => film.RedMultiplier, static value => film => film with { RedMultiplier = value })), Field),
        new("Red multiplier", "Red primary gain"));
    public static readonly ToneMappingParameter FilmGreenMultiplier = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PositiveMagnitude>.New(static film => film.GreenMultiplier, static value => film => film with { GreenMultiplier = value })), Field),
        new("Green multiplier", "Green primary gain"));
    public static readonly ToneMappingParameter FilmBlueMultiplier = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PositiveMagnitude>.New(static film => film.BlueMultiplier, static value => film => film with { BlueMultiplier = value })), Field),
        new("Blue multiplier", "Blue primary gain"));
    public static readonly ToneMappingParameter FilmSigmoidMinimum = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, Exposure>.New(static film => film.SigmoidMinimum, static value => film => film with { SigmoidMinimum = value })), FilmStops),
        new("Sigmoid minimum", "Lowest stop dye response reads"));
    public static readonly ToneMappingParameter FilmSigmoidSpan = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, FilmSpan>.New(static film => film.SigmoidSpan, static value => film => film with { SigmoidSpan = value })), Field),
        new("Sigmoid span", "Stops dye response reads above minimum"));
    public static readonly ToneMappingParameter FilmToeX = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, CurveFraction>.New(static film => film.ToeX, static value => film => film with { ToeX = value })), Field),
        new("Toe X", "Toe input"));
    public static readonly ToneMappingParameter FilmToeY = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, CurveFraction>.New(static film => film.ToeY, static value => film => film with { ToeY = value })), Field),
        new("Toe Y", "Toe output"));
    public static readonly ToneMappingParameter FilmShoulderReachX = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, CurveFraction>.New(static film => film.ShoulderReachX, static value => film => film with { ShoulderReachX = value })), Field),
        new("Shoulder reach X", "Share of input above toe where shoulder starts"));
    public static readonly ToneMappingParameter FilmShoulderReachY = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, CurveFraction>.New(static film => film.ShoulderReachY, static value => film => film with { ShoulderReachY = value })), Field),
        new("Shoulder reach Y", "Share of output above toe where shoulder starts"));
    public static readonly ToneMappingParameter FilmNegativeExposure = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, Exposure>.New(static film => film.NegativeExposure, static value => film => film with { NegativeExposure = value })), FilmStops),
        new("Negative exposure", "Light on negative in stops"));
    public static readonly ToneMappingParameter FilmNegativeDensity = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, Density>.New(static film => film.NegativeDensity, static value => film => film with { NegativeDensity = value })), Field),
        new("Negative density", "Negative maximum dye density"));
    public static readonly ToneMappingParameter FilmBacklightRed = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PositiveMagnitude>.New(static film => film.BacklightRed, static value => film => film with { BacklightRed = value })), Field),
        new("Backlight red", "Red of light printing negative"));
    public static readonly ToneMappingParameter FilmBacklightGreen = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PositiveMagnitude>.New(static film => film.BacklightGreen, static value => film => film with { BacklightGreen = value })), Field),
        new("Backlight green", "Green of light printing negative"));
    public static readonly ToneMappingParameter FilmBacklightBlue = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, PositiveMagnitude>.New(static film => film.BacklightBlue, static value => film => film with { BacklightBlue = value })), Field),
        new("Backlight blue", "Blue of light printing negative"));
    public static readonly ToneMappingParameter FilmPrintExposure = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, Exposure>.New(static film => film.PrintExposure, static value => film => film with { PrintExposure = value })), FilmStops),
        new("Print exposure", "Backlight on print in stops"));
    public static readonly ToneMappingParameter FilmPrintDensity = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, Density>.New(static film => film.PrintDensity, static value => film => film with { PrintDensity = value })), Field),
        new("Print density", "Print maximum dye density"));
    public static readonly ToneMappingParameter FilmBlackPoint = Film.Row(static section => StateParameter.OptionalBounded(lens(section, Lens<FilmState, Gated<BlackOffset>>.New(static film => film.BlackPoint, static value => film => film with { BlackPoint = value })), Field),
        new("Black point", "Film black in per mille, stock black when off"));
    public static readonly ToneMappingParameter FilmPostFilterRed = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, AxisFraction>.New(static film => film.PostFilterRed, static value => film => film with { PostFilterRed = value })), FilmShare),
        new("Post-filter red", "Red transmittance of print filter"));
    public static readonly ToneMappingParameter FilmPostFilterGreen = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, AxisFraction>.New(static film => film.PostFilterGreen, static value => film => film with { PostFilterGreen = value })), FilmShare),
        new("Post-filter green", "Green transmittance of print filter"));
    public static readonly ToneMappingParameter FilmPostFilterBlue = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, AxisFraction>.New(static film => film.PostFilterBlue, static value => film => film with { PostFilterBlue = value })), FilmShare),
        new("Post-filter blue", "Blue transmittance of print filter"));
    public static readonly ToneMappingParameter FilmMidtoneSaturation = Film.Row(static section => StateParameter.Bounded(lens(section, Lens<FilmState, Magnitude>.New(static film => film.MidtoneSaturation, static value => film => film with { MidtoneSaturation = value })), Field),
        new("Midtone saturation", "Saturation factor at middle gray"));
    public static readonly ToneMappingParameter PreserveHue = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, AxisFraction>.New(static application => application.PreserveHue, static value => application => application with { PreserveHue = value }))),
        new("Preserve hue", "Share of pixel hue kept through curve"));
    public static readonly ToneMappingParameter Purity = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, AxisFraction>.New(static application => application.Purity, static value => application => application with { Purity = value }))),
        new("Purity", "Share of primary inset restored after curve"));
    public static readonly ToneMappingParameter BasePrimaries = Application.Row(static section => new StateParameter<ToneMapping>.OptionalChoice<Gamut>(lens(section, Lens<CurveApplication, Option<Gamut>>.New(static application => application.BasePrimaries, static value => application => application with { BasePrimaries = value })), "Working"),
        new("Base primaries", "Gamut of rendering primaries"));
    public static readonly ToneMappingParameter RedInset = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, PrimaryInset>.New(static application => application.RedInset, static value => application => application with { RedInset = value })), Inset),
        new("Red inset", "Red rendering primary pull to white"));
    public static readonly ToneMappingParameter GreenInset = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, PrimaryInset>.New(static application => application.GreenInset, static value => application => application with { GreenInset = value })), Inset),
        new("Green inset", "Green rendering primary pull to white"));
    public static readonly ToneMappingParameter BlueInset = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, PrimaryInset>.New(static application => application.BlueInset, static value => application => application with { BlueInset = value })), Inset),
        new("Blue inset", "Blue rendering primary pull to white"));
    public static readonly ToneMappingParameter RedRotation = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, PrimaryRotation>.New(static application => application.RedRotation, static value => application => application with { RedRotation = value })), Turn),
        new("Red rotation", "Red rendering primary turn about white"));
    public static readonly ToneMappingParameter GreenRotation = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, PrimaryRotation>.New(static application => application.GreenRotation, static value => application => application with { GreenRotation = value })), Turn),
        new("Green rotation", "Green rendering primary turn about white"));
    public static readonly ToneMappingParameter BlueRotation = Application.Row(static section => StateParameter.Bounded(lens(section, Lens<CurveApplication, PrimaryRotation>.New(static application => application.BlueRotation, static value => application => application with { BlueRotation = value })), Turn),
        new("Blue rotation", "Blue rendering primary turn about white"));
    public static readonly ToneMappingParameter SigmoidContrast = Sigmoid.Row(static section => StateParameter.Bounded(lens(section, Lens<SigmoidState, CurveGamma>.New(static sigmoid => sigmoid.Contrast, static value => sigmoid => sigmoid with { Contrast = value })), new Presentation<float> { Soft = (0.7f, 3f) }),
        new("Contrast", "Middle gray slope over stops"));
    public static readonly ToneMappingParameter SigmoidSkew = Sigmoid.Row(static section => StateParameter.Bounded(lens(section, Lens<SigmoidState, SignedFraction>.New(static sigmoid => sigmoid.Skew, static value => sigmoid => sigmoid with { Skew = value }))),
        new("Skew", "Contrast shift to shadows or highlights"));
    public static readonly ToneMappingParameter SigmoidBlack = Sigmoid.Row(static section => StateParameter.Bounded(lens(section, Lens<SigmoidState, TargetBlack>.New(static sigmoid => sigmoid.Black, static value => sigmoid => sigmoid with { Black = value })), AxisFraction.Presentation with { Soft = (0f, 0.01f) }),
        new("Target black", "Display black as share of white"));
    public static readonly ToneMappingParameter PiecewiseToeStrength = Piecewise.Row(static section => StateParameter.Bounded(lens(section, Lens<PiecewiseState, AxisFraction>.New(static piecewise => piecewise.ToeStrength, static value => piecewise => piecewise with { ToeStrength = value }))),
        new("Toe strength", "Toe depth below straight section"));
    public static readonly ToneMappingParameter PiecewiseToeLength = Piecewise.Row(static section => StateParameter.Bounded(lens(section, Lens<PiecewiseState, AxisFraction>.New(static piecewise => piecewise.ToeLength, static value => piecewise => piecewise with { ToeLength = value }))),
        new("Toe length", "Toe span along input"));
    public static readonly ToneMappingParameter PiecewiseShoulderAngle = Piecewise.Row(static section => StateParameter.Bounded(lens(section, Lens<PiecewiseState, AxisFraction>.New(static piecewise => piecewise.ShoulderAngle, static value => piecewise => piecewise with { ShoulderAngle = value }))),
        new("Shoulder angle", "Shoulder roll past white"));
    public static readonly ToneMappingParameter PiecewiseGamma = Piecewise.Row(static section => StateParameter.Bounded(lens(section, Lens<PiecewiseState, CurveGamma>.New(static piecewise => piecewise.Gamma, static value => piecewise => piecewise with { Gamma = value })), new Presentation<float>()),
        new("Gamma", "Straight section power"));
    public static readonly ToneMappingParameter PiecewiseShoulderStrength = Piecewise.Row(static section => StateParameter.Bounded(lens(section, Lens<PiecewiseState, ShoulderStrength>.New(static piecewise => piecewise.ShoulderStrength, static value => piecewise => piecewise with { ShoulderStrength = value })), new Presentation<float>()),
        new("Shoulder strength", "Highlight stops in shoulder past straight section"));
    public static readonly ToneMappingParameter PiecewiseShoulderLength = Piecewise.Row(static section => StateParameter.Bounded(lens(section, Lens<PiecewiseState, ShoulderLength>.New(static piecewise => piecewise.ShoulderLength, static value => piecewise => piecewise with { ShoulderLength = value })), new Presentation<float>()),
        new("Shoulder length", "Shoulder span along output"));
    public static readonly ToneMappingParameter LogarithmicBias = Logarithmic.Row(static section => StateParameter.Bounded(lens(section, Lens<LogarithmicState, DragoBias>.New(static logarithmic => logarithmic.Bias, static value => logarithmic => logarithmic with { Bias = value })), new Presentation<float> { Soft = (0.7f, 0.9f) }),
        new("Bias", "Drago bias, lower values lift shadows"));
    public static readonly ToneMappingParameter LogarithmicWhite = Logarithmic.Row(static section => StateParameter.OptionalBounded(lens(section, Lens<LogarithmicState, Gated<Exposure>>.New(static logarithmic => logarithmic.White, static value => logarithmic => logarithmic with { White = value }))),
        new("White point", "Scene value at display white, frame white when off"));
    public static readonly ToneMappingParameter PhotoreceptorIntensity = Photoreceptor.Row(static section => StateParameter.Bounded(lens(section, Lens<PhotoreceptorState, ReceptorIntensity>.New(static photoreceptor => photoreceptor.Intensity, static value => photoreceptor => photoreceptor with { Intensity = value })), new Presentation<float> { Soft = (-8f, 8f) }),
        new("Intensity", "Response brightness"));
    public static readonly ToneMappingParameter PhotoreceptorContrast = Photoreceptor.Row(static section => StateParameter.OptionalBounded(lens(section, Lens<PhotoreceptorState, Gated<PositiveMagnitude>>.New(static photoreceptor => photoreceptor.Contrast, static value => photoreceptor => photoreceptor with { Contrast = value })), new Presentation<float> { Soft = (PhotoreceptorState.FlattestContrast, 1f) }),
        new("Contrast", "Response slope, read from frame when off"));
    public static readonly ToneMappingParameter PhotoreceptorLightAdaptation = Photoreceptor.Row(static section => StateParameter.Bounded(lens(section, Lens<PhotoreceptorState, AxisFraction>.New(static photoreceptor => photoreceptor.LightAdaptation, static value => photoreceptor => photoreceptor with { LightAdaptation = value }))),
        new("Adaptation", "Pixel level share in adaptation level"));
    public static readonly ToneMappingParameter PhotoreceptorChromaticAdaptation = Photoreceptor.Row(static section => StateParameter.Bounded(lens(section, Lens<PhotoreceptorState, AxisFraction>.New(static photoreceptor => photoreceptor.ChromaticAdaptation, static value => photoreceptor => photoreceptor with { ChromaticAdaptation = value }))),
        new("Color correction", "Channel level share in adaptation level"));
    public static readonly ToneMappingParameter ReinhardWhite = new("reinhard-white", StateParameter.OptionalBounded(Lens<ToneMapping, Gated<Exposure>>.New(static state => state.ReinhardWhite, static white => state => state with { ReinhardWhite = white })), Seq(Tone.Formation.ReinhardExtended),
        new("White point", "Scene value at display white, frame white when off"));
    public static readonly ToneMappingParameter HableA = Hable.Row(static section => StateParameter.Bounded(lens(section, Lens<HableState, ShoulderCoefficient>.New(static hable => hable.A, static value => hable => hable with { A = value })), new Presentation<float>()),
        new("Shoulder strength", "Hable's A"));
    public static readonly ToneMappingParameter HableB = Hable.Row(static section => StateParameter.Bounded(lens(section, Lens<HableState, LinearCoefficient>.New(static hable => hable.B, static value => hable => hable with { B = value })), new Presentation<float>()),
        new("Linear strength", "Hable's B"));
    public static readonly ToneMappingParameter HableC = Hable.Row(static section => StateParameter.Bounded(lens(section, Lens<HableState, LinearAngle>.New(static hable => hable.C, static value => hable => hable with { C = value })), new Presentation<float>()),
        new("Linear angle", "Hable's C"));
    public static readonly ToneMappingParameter HableD = Hable.Row(static section => StateParameter.Bounded(lens(section, Lens<HableState, ToeCoefficient>.New(static hable => hable.D, static value => hable => hable with { D = value })), new Presentation<float>()),
        new("Toe strength", "Hable's D"));
    public static readonly ToneMappingParameter HableE = Hable.Row(static section => StateParameter.Bounded(lens(section, Lens<HableState, ToeNumerator>.New(static hable => hable.E, static value => hable => hable with { E = value })), new Presentation<float>()),
        new("Toe numerator", "Hable's E"));
    public static readonly ToneMappingParameter HableF = Hable.Row(static section => StateParameter.Bounded(lens(section, Lens<HableState, ToeDenominator>.New(static hable => hable.F, static value => hable => hable with { F = value })), new Presentation<float>()),
        new("Toe denominator", "Hable's F"));
    public static readonly ToneMappingParameter HableWhite = Hable.Row(static section => StateParameter.OptionalBounded(lens(section, Lens<HableState, Gated<HableWhite>>.New(static hable => hable.White, static value => hable => hable with { White = value }))),
        new("Linear white point", "Scene value normalized to white, frame white when off"));

    public StateParameter<ToneMapping> Kind { get; }
    public Seq<Formation> Formations { get; }
    public ParameterText Text { get; }

    private sealed record Section<TSection>(Lens<ToneMapping, TSection> Lens, Seq<Formation> Formations) {
        public ToneMappingParameter Row(Func<Lens<ToneMapping, TSection>, StateParameter<ToneMapping>> kind, ParameterText text, [CallerMemberName] string member = "") =>
            new(JsonNamingPolicy.KebabCaseLower.ConvertName(member), kind(Lens), Formations, text);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Formation : ICaptioned {
    public static readonly Formation AgXView = new("agx", "AgX", AgX.View);
    public static readonly Formation AgXFalseColor = new("agx-false-color", "AgX false color", AgX.FalseColor);
    public static readonly Formation Aces = new("aces", "ACES 2.0", AcesTransform.Formed);
    public static readonly Formation Filmic = new("filmic", "Filmic", FilmicTransform.Formed);
    public static readonly Formation Film = new("film", "Film", FilmTransform.Formed);
    public static readonly Formation Tony = new("tony", "Tony McMapface", TonyTransform.Formed);
    public static readonly Formation Sigmoid = new("sigmoid", "Sigmoid", ToneCurves.Sigmoid);
    public static readonly Formation HablePiecewise = new("hable-piecewise", "Hable piecewise", ToneCurves.Piecewise);
    public static readonly Formation Logarithmic = new("logarithmic", "Logarithmic", ToneCurves.Logarithmic);
    public static readonly Formation Photoreceptor = new("photoreceptor", "R/D photoreceptor", ToneCurves.Photoreceptor);
    public static readonly Formation Reinhard = new("reinhard", "Reinhard", ToneCurves.Reinhard);
    public static readonly Formation ReinhardExtended = new("reinhard-extended", "Reinhard extended", ToneCurves.ReinhardExtended);
    public static readonly Formation Hable = new("hable", "Hable", ToneCurves.Hable);
    public static readonly Formation PbrNeutral = new("pbr-neutral", "Khronos PBR Neutral", ToneCurves.PbrNeutral);

    public string Caption { get; }

    [UseDelegateFromConstructor]
    public partial PixelPass Pass(ToneMapping state, PassContext context);

    internal static PixelPass.Color Formed(PassContext context, Gamut scene, Seq<Action<Span<Vector4>>> steps, ColorEncoding signal) {
        Action<Span<Vector4>>[] chain = [.. LutTables.Between(context.Working, scene).Map<Action<Span<Vector4>>>(static step => step.Apply), .. steps, .. signal.To(context.Display.Encoding)];
        return new(row => {
            foreach (Action<Span<Vector4>> step in chain)
                step(row);
        });
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Display : ICaptioned {
    public static readonly Display Srgb = new("srgb", new ColorEncoding(Gamut.StandardRgb, TransferCurve.Srgb, Nits.ReferenceWhite), Nits.ReferenceWhite, "sRGB");
    public static readonly Display DisplayP3 = new("display-p3", new ColorEncoding(Gamut.DisplayP3, TransferCurve.Srgb, Nits.ReferenceWhite), Nits.ReferenceWhite, "Display P3");
    public static readonly Display Rec1886 = new("rec1886", new ColorEncoding(Gamut.StandardRgb, Transfer.Gamma.Rec1886, Nits.ReferenceWhite), Nits.ReferenceWhite, "Rec.1886");
    public static readonly Display Rec2020 = new("rec2020", new ColorEncoding(Gamut.Rec2020, Transfer.Gamma.Rec1886, Nits.ReferenceWhite), Nits.ReferenceWhite, "Rec.2020");
    public static readonly Display Rec2100PqHdr = new("rec2100-pq-hdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Pq, Nits.FileWhite), Nits.HlgPeak, "Rec.2100-PQ, HDR 1000 nits");
    public static readonly Display Rec2100PqSdr = new("rec2100-pq-sdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Pq, Nits.FileWhite), Nits.ReferenceWhite, "Rec.2100-PQ, SDR");
    public static readonly Display Rec2100HlgHdr = new("rec2100-hlg-hdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Hlg, Nits.FileWhite), Nits.HlgPeak, "Rec.2100-HLG, HDR 1000 nits");
    public static readonly Display Rec2100HlgSdr = new("rec2100-hlg-sdr", new ColorEncoding(Gamut.Rec2020, TransferCurve.Hlg, Nits.FileWhite), Nits.ReferenceWhite, "Rec.2100-HLG, SDR");

    public ColorEncoding Encoding { get; }
    public Nits Peak { get; }
    public string Caption { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AgXLook : ICaptioned {
    public static readonly AgXLook Punchy = new("punchy", (GradingStyle.Log.Neutral with { Shadows = new ZoneSpan(new Vector4(0.2f, 0.2f, 0.2f, 0.35f), 0.4f, 0.3f) })
        .Kernel(GradingStyle.Log).ToSeq()
        .Add(new LutTable.Cdl(CdlStyle.FwdNoClamp, Vector3.One, Vector3.Zero, new Vector3(1.0912f), 1f).Apply), "Punchy");
    public static readonly AgXLook Greyscale = new("greyscale", Seq(new LutTable.Antilog(LogSpace.AgXLog.Allocation).Apply, AgX.Grey.Apply, new LutTable.Log(LogSpace.AgXLog.Allocation).Apply), "Greyscale");
    public static readonly AgXLook VeryHighContrast = new("very-high-contrast", Contrasted(1.57f, 0.9f), "Very high contrast");
    public static readonly AgXLook HighContrast = new("high-contrast", Contrasted(1.4f, 0.95f), "High contrast");
    public static readonly AgXLook MediumHighContrast = new("medium-high-contrast", Contrasted(1.2f, 1f), "Medium high contrast");
    public static readonly AgXLook BaseContrast = new("base-contrast", Contrasted(1f, 1f), "Base contrast");
    public static readonly AgXLook MediumLowContrast = new("medium-low-contrast", Contrasted(0.9f, 1.05f), "Medium low contrast");
    public static readonly AgXLook LowContrast = new("low-contrast", Contrasted(0.8f, 1.1f), "Low contrast");
    public static readonly AgXLook VeryLowContrast = new("very-low-contrast", Contrasted(0.7f, 1.15f), "Very low contrast");

    public Seq<Action<Span<Vector4>>> Steps { get; }
    public string Caption { get; }

    private static Seq<Action<Span<Vector4>>> Contrasted(float contrast, float saturation) =>
        Seq(new LutTable.Cdl(CdlStyle.FwdNoClamp, new Vector3(contrast), new Vector3(0.4f * (1f - contrast)), Vector3.One, saturation).Apply);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AgX {
    public static Lut1D FalseColorBands => EmbeddedTables.Curve("AgX_False_Color.spi1d");

    internal static LutTable Grey { get; } = (X: 0.2589235355689848d, Y: 0.6104985346066525d, Z: 0.13057792982436284d) switch {
        var weights => new LutTable.Affine(Matrix<double>.Build.Dense(3, 3, (_, column) => column switch { 0 => weights.X, 1 => weights.Y, _ => weights.Z })),
    };

    private static readonly (string Cube, ColorEncoding Signal) BaseRec1886 = ("AgX_Base_sRGB.cube", new(Gamut.StandardRgb, Transfer.Gamma.Rec1886, Nits.ReferenceWhite));
    private static readonly (string Cube, ColorEncoding Signal) BaseDisplayP3 = ("AgX_Base_P3.cube", new(Gamut.DisplayP3, Transfer.Gamma.Rec1886, Nits.ReferenceWhite));
    private static readonly (string Cube, ColorEncoding Signal) BaseRec2020 = ("AgX_Base_Rec2020.cube", new(Gamut.Rec2020, Transfer.Gamma.Rec1886, Nits.ReferenceWhite));
    private static readonly (string Cube, ColorEncoding Signal) Rec2100Hlg = ("AgX_Rec2100-HLG_p3_lim.cube", new(Gamut.Rec2020, TransferCurve.Hlg, Nits.ReferenceWhite));

    public static Lut3D Cube(Display display) => EmbeddedTables.Lattice(Output(display).Cube);

    internal static PixelPass View(ToneMapping state, PassContext context) =>
        Formation.Formed(context, LogSpace.AgXLog.Gamut, Opened(state.Look).Add(new LutTable.Cube(Cube(context.Display), LutInterpolation.Tetrahedral).Apply), Output(context.Display).Signal);

    internal static PixelPass FalseColor(ToneMapping state, PassContext context) =>
        Formation.Formed(context, LogSpace.AgXLog.Gamut, ToBandPosition(state.Look).Add(new LutTable.ChannelCurve(FalseColorBands).Apply), BaseRec1886.Signal);

    internal static Seq<Action<Span<Vector4>>> ToBandPosition(Option<AgXLook> look) =>
        Opened(look)
            + Seq(
                new LutTable.Cube(Cube(Display.Rec2020), LutInterpolation.Tetrahedral).Apply,
                BaseRec2020.Signal.Decode,
                Grey.Apply,
                new LutTable.Exponent(ExponentStyle.BasicRev, new Vector3(2.5f), Vector3.Zero).Apply);

    private static Seq<Action<Span<Vector4>>> Opened(Option<AgXLook> look) =>
        look.Match(
            Some: static chosen => LogSpace.AgXLog.Into.Map<Action<Span<Vector4>>>(static step => step.Apply) + chosen.Steps + LogSpace.AgXLog.Back.Map<Action<Span<Vector4>>>(static step => step.Apply),
            None: Seq<Action<Span<Vector4>>>())
        + Seq(new LutTable.Log(LogSpace.AgXLog.Allocation).Apply);

    private static (string Cube, ColorEncoding Signal) Output(Display display) =>
        display.Map(
            srgb: BaseRec1886, displayP3: BaseDisplayP3, rec1886: BaseRec1886, rec2020: BaseRec2020,
            rec2100PqHdr: Rec2100Hlg, rec2100PqSdr: BaseDisplayP3, rec2100HlgHdr: Rec2100Hlg, rec2100HlgSdr: BaseDisplayP3);
}
