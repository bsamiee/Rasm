using Rasm.Imaging.Filters.Generators;
using Rasm.Rhino.Render.Effects;

using System.Runtime.InteropServices;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToneText {
    public static string FormationText(Formation formation) =>
        formation.Map(
            agXView: "AgX", agXFalseColor: "AgX False Color", aces: "ACES 2.0", filmic: "Filmic", film: "Film", tony: "Tony McMapface",
            sigmoid: "Sigmoid", hablePiecewise: "Hable Piecewise", logarithmic: "Logarithmic", photoreceptor: "R/D Photoreceptor",
            reinhard: "Reinhard", reinhardExtended: "Reinhard Extended", hable: "Hable", pbrNeutral: "Khronos PBR Neutral");

    public static string LookText(AgXLook look) =>
        look.Map(
            punchy: "Punchy", greyscale: "Greyscale", veryHighContrast: "Very High Contrast", highContrast: "High Contrast",
            mediumHighContrast: "Medium High Contrast", baseContrast: "Base Contrast", mediumLowContrast: "Medium Low Contrast",
            lowContrast: "Low Contrast", veryLowContrast: "Very Low Contrast");

    public static string FilmicViewText(FilmicView view) =>
        view.Map(
            veryHighContrast: "Very High Contrast", highContrast: "High Contrast", mediumHighContrast: "Medium High Contrast",
            mediumContrast: "Medium Contrast", mediumLowContrast: "Medium Low Contrast", lowContrast: "Low Contrast",
            veryLowContrast: "Very Low Contrast", log: "Log");

    public static string GamutText(Gamut gamut) =>
        gamut.Map(
            standardRgb: "Rec.709", displayP3: "Display P3", rec2020: "Rec.2020", acescg: "ACEScg", aces20651: "ACES2065-1",
            proPhoto: "ProPhoto RGB", eGamut: "E-Gamut", arriWideGamut3: "ARRI Wide Gamut 3", arriWideGamut4: "ARRI Wide Gamut 4",
            sGamut3: "S-Gamut3", sGamut3Cine: "S-Gamut3.Cine", veniceSGamut3: "Venice S-Gamut3", veniceSGamut3Cine: "Venice S-Gamut3.Cine",
            cinemaGamut: "Cinema Gamut", redWideGamutRgb: "REDWideGamutRGB", vGamut: "V-Gamut", dGamut: "D-Gamut",
            protuneNative: "Protune Native", blackmagicWideGamut: "Blackmagic Wide Gamut", daVinciWideGamut: "DaVinci Wide Gamut",
            appleWideGamut: "Apple Wide Gamut");

    public static string TransferCurveText(TransferCurve curve) =>
        curve.Map(linear: "Linear", srgb: "sRGB", pq: "PQ", hlg: "HLG", acesCct: "ACEScct", acesCc: "ACEScc");

    public static string CameraLogText(CameraLog log) =>
        log.Map(
            arriLogC3: "ARRI LogC3", arriLogC4: "ARRI LogC4", sonySLog2: "Sony S-Log2", sonySLog3: "Sony S-Log3",
            canonLog2: "Canon Log 2", canonLog3: "Canon Log 3", redLog3G10: "RED Log3G10", panasonicVLog: "Panasonic V-Log",
            fujifilmFLog: "Fujifilm F-Log", fujifilmFLog2: "Fujifilm F-Log2", djiDLog: "DJI D-Log", goProProtune: "GoPro Protune",
            nikonNLog: "Nikon N-Log", appleLog: "Apple Log", blackmagicFilmGen5: "Blackmagic Film Gen 5",
            daVinciIntermediate: "DaVinci Intermediate", filmLightTLog: "FilmLight T-Log");

    public static string LogSpaceText(LogSpace space) => space.Map(agXLog: "AgX Log", filmicLog: "Filmic Log");

    public static (string Caption, HashMap<string, ParameterText> Parameters) TransferText(Transfer transfer) =>
        transfer.Map(
            standard: ("Standard", ParameterText.Fields<Transfer.Standard, StandardTransferParameter, InvalidColor>(static item => item.Map(
                curve: ParameterText.Choice<TransferCurve, InvalidColor>("Curve", "Transfer function of the table's input and output", TransferCurveText)))),
            gamma: ("Gamma", ParameterText.Fields<Transfer.Gamma, GammaTransferParameter, InvalidColor>(static item => item.Map(
                exponent: ParameterText.Of("Exponent", "Power decoding the table's signal to linear light")))),
            camera: ("Camera log", ParameterText.Fields<Transfer.Camera, CameraTransferParameter, InvalidColor>(static item => item.Map(
                log: ParameterText.Choice<CameraLog, InvalidColor>("Camera log", "Camera encoding of the table's input and output", CameraLogText)))));

    public static (string Caption, HashMap<string, ParameterText> Parameters) LutSpaceText(LutSpace space) =>
        space.Map(
            encoded: ("RGB encoding", ParameterText.Fields<LutSpace.Encoded, EncodedLutParameter, InvalidColor>(static field => field.Map(
                gamut: ParameterText.Choice<Gamut, InvalidColor>("Gamut", "Primaries of the table's input and output", GamutText),
                transfer: ParameterText.Variant<Transfer>("Transfer", "Encoding of the table's input and output", TransferText)))),
            log: ("Grading log", ParameterText.Fields<LutSpace.Log, LogLutParameter, InvalidColor>(static field => field.Map(
                space: ParameterText.Choice<LogSpace, InvalidColor>("Grading log", "Log space and its gamut bracket applied around the table", LogSpaceText)))));

    public static string InterpolationText(RampInterpolation interpolation) =>
        interpolation.Map(linear: "Linear", ease: "Ease", constant: "Constant", linearRgb: "Linear RGB", easeRgb: "Ease RGB", smootherRgb: "Smoother RGB");

    public static string BlendText(BlendingMode mode) =>
        mode.Map(
            mix: "Mix", darken: "Darken", multiply: "Multiply", burn: "Color Burn", lighten: "Lighten", screen: "Screen", dodge: "Color Dodge",
            add: "Add", overlay: "Overlay", softLight: "Soft Light", linearLight: "Linear Light", difference: "Difference", exclusion: "Exclusion",
            subtract: "Subtract", divide: "Divide", hue: "Hue", saturation: "Saturation", color: "Color", value: "Value");

    public static string WrapText(WrapMode wrap) =>
        wrap.Map(black: "Clip", clamp: "Extend", periodic: "Repeat", mirror: "Mirror");

    public static string MetricText(CellMetric metric) =>
        metric.Map(euclidean: "Euclidean", manhattan: "Manhattan", chebyshev: "Chebychev", minkowski: "Minkowski");
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("3213d575-58cc-4a78-b13a-d130c437f053")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class SceneExposure : StageEffect<SceneExposure, ExposureState, ExposureParameter, InvalidToneValue>, IEffectDefinition<ExposureState, ExposureParameter> {
    public const string Caption = "Exposure";

    public static EffectRow<ExposureState, ExposureParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            exposure: ParameterText.Of("Exposure", "Scene gain in stops"),
            meteringLow: ParameterText.Of("Meter Low", "Lowest luminance rank the meter averages"),
            meteringHigh: ParameterText.Of("Meter High", "Highest luminance rank the meter averages"),
            meteringCenter: ParameterText.Of("Center Weight", "Share by which samples outside the central half count less"),
            meteringStartX: ParameterText.Of("Region Left", "Left edge of the metered region as a share of the frame"),
            meteringStartY: ParameterText.Of("Region Top", "Top edge of the metered region as a share of the frame"),
            meteringEndX: ParameterText.Of("Region Right", "Right edge of the metered region as a share of the frame"),
            meteringEndY: ParameterText.Of("Region Bottom", "Bottom edge of the metered region as a share of the frame")));
}

[Guid("72433230-65e7-4c28-b1a9-0afa4e6f32e2")]
[CustomPostEffect(PostEffectType.ToneMapping, Caption, EffectStyles.Listed)]
public sealed class ImageFormation : StageEffect<ImageFormation, ToneMapping, ToneMappingParameter, InvalidToneValue>, IEffectDefinition<ToneMapping, ToneMappingParameter> {
    public const string Caption = "Darkroom";

    public static EffectRow<ToneMapping, ToneMappingParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            formation: ParameterText.Choice<Formation, InvalidToneValue>("View Transform", "Formation turning scene light into display codes", ToneText.FormationText),
            look: ParameterText.Choice<AgXLook, InvalidToneValue>("Look", "AgX look applied in AgX Log ahead of the AgX and AgX False Color views", "None", ToneText.LookText),
            filmicView: ParameterText.Choice<FilmicView, InvalidToneValue>("Filmic Contrast", "Filmic contrast look, Log showing the base log encoding", ToneText.FilmicViewText),
            filmPreExposure: ParameterText.Of("Pre-exposure", "Stops added ahead of the film stock"),
            filmPreFilterRed: ParameterText.Of("Pre-filter Red", "Red transmittance of the filter over the lens"),
            filmPreFilterGreen: ParameterText.Of("Pre-filter Green", "Green transmittance of the filter over the lens"),
            filmPreFilterBlue: ParameterText.Of("Pre-filter Blue", "Blue transmittance of the filter over the lens"),
            filmRedScale: ParameterText.Of("Red Scale", "Gamut extension of the red primary"),
            filmGreenScale: ParameterText.Of("Green Scale", "Gamut extension of the green primary"),
            filmBlueScale: ParameterText.Of("Blue Scale", "Gamut extension of the blue primary"),
            filmRedRotation: ParameterText.Of("Red Rotation", "Hue turn of the red primary"),
            filmGreenRotation: ParameterText.Of("Green Rotation", "Hue turn of the green primary"),
            filmBlueRotation: ParameterText.Of("Blue Rotation", "Hue turn of the blue primary"),
            filmRedMultiplier: ParameterText.Of("Red Multiplier", "Gain of the red primary"),
            filmGreenMultiplier: ParameterText.Of("Green Multiplier", "Gain of the green primary"),
            filmBlueMultiplier: ParameterText.Of("Blue Multiplier", "Gain of the blue primary"),
            filmSigmoidMinimum: ParameterText.Of("Sigmoid Minimum", "Lowest stop the dye response reads"),
            filmSigmoidSpan: ParameterText.Of("Sigmoid Span", "Stops the dye response reads above its minimum"),
            filmToeX: ParameterText.Of("Toe X", "Toe point on the response input"),
            filmToeY: ParameterText.Of("Toe Y", "Toe point on the response output"),
            filmShoulderReachX: ParameterText.Of("Shoulder Reach X", "Share of the input above the toe where the shoulder starts"),
            filmShoulderReachY: ParameterText.Of("Shoulder Reach Y", "Share of the output above the toe where the shoulder starts"),
            filmNegativeExposure: ParameterText.Of("Negative Exposure", "Stops of light reaching the negative"),
            filmNegativeDensity: ParameterText.Of("Negative Density", "Maximum dye density of the negative"),
            filmBacklightRed: ParameterText.Of("Backlight Red", "Red of the light printing the negative"),
            filmBacklightGreen: ParameterText.Of("Backlight Green", "Green of the light printing the negative"),
            filmBacklightBlue: ParameterText.Of("Backlight Blue", "Blue of the light printing the negative"),
            filmPrintExposure: ParameterText.Of("Print Exposure", "Stops of backlight reaching the print"),
            filmPrintDensity: ParameterText.Of("Print Density", "Maximum dye density of the print"),
            filmBlackPoint: ParameterText.Of("Black Point", "Film black in per mille, off reading the stock's own black"),
            filmPostFilterRed: ParameterText.Of("Post-filter Red", "Red transmittance of the filter over the print"),
            filmPostFilterGreen: ParameterText.Of("Post-filter Green", "Green transmittance of the filter over the print"),
            filmPostFilterBlue: ParameterText.Of("Post-filter Blue", "Blue transmittance of the filter over the print"),
            filmMidtoneSaturation: ParameterText.Of("Midtone Saturation", "Saturation factor at mid grey"),
            preserveHue: ParameterText.Of("Preserve Hue", "Share of each pixel's hue kept through the curve"),
            purity: ParameterText.Of("Purity", "Share of the primaries' inset restored after the curve"),
            basePrimaries: ParameterText.Choice<Gamut, InvalidColor>("Base Primaries", "Gamut the rendering primaries are set in", "Working", ToneText.GamutText),
            redInset: ParameterText.Of("Red Inset", "Pull of the red rendering primary toward white"),
            greenInset: ParameterText.Of("Green Inset", "Pull of the green rendering primary toward white"),
            blueInset: ParameterText.Of("Blue Inset", "Pull of the blue rendering primary toward white"),
            redRotation: ParameterText.Of("Red Rotation", "Turn of the red rendering primary about white"),
            greenRotation: ParameterText.Of("Green Rotation", "Turn of the green rendering primary about white"),
            blueRotation: ParameterText.Of("Blue Rotation", "Turn of the blue rendering primary about white"),
            sigmoidContrast: ParameterText.Of("Contrast", "Slope at middle grey on the stops axis"),
            sigmoidSkew: ParameterText.Of("Skew", "Moves contrast toward the shadows or the highlights"),
            sigmoidBlack: ParameterText.Of("Target Black", "Display black as a fraction of display white"),
            piecewiseToeStrength: ParameterText.Of("Toe Strength", "Depth of the toe below the straight section"),
            piecewiseToeLength: ParameterText.Of("Toe Length", "Span of the toe along the input"),
            piecewiseShoulderAngle: ParameterText.Of("Shoulder Angle", "Roll of the shoulder past white"),
            piecewiseGamma: ParameterText.Of("Gamma", "Power of the straight section"),
            piecewiseShoulderStrength: ParameterText.Of("Shoulder Strength", "Stops of highlight the shoulder holds past the straight section"),
            piecewiseShoulderLength: ParameterText.Of("Shoulder Length", "Span of the shoulder along the output"),
            logarithmicBias: ParameterText.Of("Bias", "Drago's bias, lower values lifting the shadows"),
            logarithmicWhite: ParameterText.Of("White Point", "Scene value mapped to display white, off reading the frame's white"),
            photoreceptorIntensity: ParameterText.Of("Intensity", "Overall brightness of the response"),
            photoreceptorContrast: ParameterText.Of("Contrast", "Slope of the response, off reading it from the frame"),
            photoreceptorLightAdaptation: ParameterText.Of("Adaptation", "Share of each pixel's own level in the adaptation level"),
            photoreceptorChromaticAdaptation: ParameterText.Of("Color Correction", "Share of each channel's own level in the adaptation level"),
            reinhardWhite: ParameterText.Of("White Point", "Scene value mapped to display white, off reading the frame's white"),
            hableA: ParameterText.Of("Shoulder Strength", "Hable's A"),
            hableB: ParameterText.Of("Linear Strength", "Hable's B"),
            hableC: ParameterText.Of("Linear Angle", "Hable's C"),
            hableD: ParameterText.Of("Toe Strength", "Hable's D"),
            hableE: ParameterText.Of("Toe Numerator", "Hable's E"),
            hableF: ParameterText.Of("Toe Denominator", "Hable's F"),
            hableWhite: ParameterText.Of("Linear White Point", "Scene value the curve normalizes to white, off reading the frame's white"))) {
        Amount = None,
        Looks = toSeq(ToneMappingParameter.Items).Filter(static item => item.Formations.Equals(Seq(Formation.Film))) switch {
            var keys => toSeq(FilmPreset.Items).Map<(string, EffectLook<ToneMapping, ToneMappingParameter>)>(preset => (
                preset.Map(@default: "Default", nostalgia: "Nostalgia", silver: "Silver"),
                new EffectLook<ToneMapping, ToneMappingParameter>.Partial(ToneMapping.Default with { Film = preset.State }, keys))),
        },
    };
}
