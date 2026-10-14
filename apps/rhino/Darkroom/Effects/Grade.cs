using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class LookEffect<TEffect> : StageEffect<TEffect, LutLook, LutLookParameter, InvalidColor>, IEffectDefinition<LutLook, LutLookParameter>
    where TEffect : LookEffect<TEffect> {
    static EffectRow<LutLook, LutLookParameter> IEffectDefinition<LutLook, LutLookParameter>.Row { get; } = new(static parameter =>
        parameter.Map(
            lut: ParameterText.Of("LUT", "Table file the look applies"),
            space: ParameterText.Variant<LutSpace>("Space", "Color space the table expects", ToneText.LutSpaceText)));
}

// --- [PRIMARIES] -----------------------------------------------------------------------
[Guid("c32e1536-d129-432d-b5ae-6da715024b02")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class WhiteBalanceCorrection : StageEffect<WhiteBalanceCorrection, WhiteBalanceState, WhiteBalanceParameter, InvalidGrade>, IEffectDefinition<WhiteBalanceState, WhiteBalanceParameter> {
    public const string Caption = "White balance";

    public static EffectRow<WhiteBalanceState, WhiteBalanceParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            temperature: ParameterText.Of("Temperature", "Kelvin of the light the image renders neutral"),
            tint: ParameterText.Of("Tint", "Offset of that white off the blackbody locus, positive toward green"))) {
        Looks = toSeq(StandardIlluminant.Items).Map<(string Name, EffectLook<WhiteBalanceState, WhiteBalanceParameter> Look)>(static illuminant =>
            (illuminant.Map(
                a: "Illuminant A", b: "Illuminant B", c: "Illuminant C", d50: "Illuminant D50", d55: "Illuminant D55", d65: "Illuminant D65", d75: "Illuminant D75",
                d93: "Illuminant D93", e: "Illuminant E", f1: "Illuminant F1", f2: "Illuminant F2", f3: "Illuminant F3", f4: "Illuminant F4", f5: "Illuminant F5",
                f6: "Illuminant F6", f7: "Illuminant F7", f8: "Illuminant F8", f9: "Illuminant F9", f10: "Illuminant F10", f11: "Illuminant F11", f12: "Illuminant F12",
                ledB1: "Illuminant LED-B1", ledB2: "Illuminant LED-B2", ledB3: "Illuminant LED-B3", ledB4: "Illuminant LED-B4", ledB5: "Illuminant LED-B5",
                ledBh1: "Illuminant LED-BH1", ledRgb1: "Illuminant LED-RGB1", ledV1: "Illuminant LED-V1", ledV2: "Illuminant LED-V2"),
             new EffectLook<WhiteBalanceState, WhiteBalanceParameter>.Whole(new WhiteBalanceState(illuminant.Temperature, illuminant.Tint)))),
    };
}

[Guid("570b0a76-a94d-4749-b0a7-b9ec2e1fc36f")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ChannelMixer : StageEffect<ChannelMixer, MixerState, MixerParameter, InvalidGrade>, IEffectDefinition<MixerState, MixerParameter> {
    public const string Caption = "Channel mixer";

    public static EffectRow<MixerState, MixerParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            redFromRed: ParameterText.Of("Red from red", "Weight of input red in output red"),
            redFromGreen: ParameterText.Of("Red from green", "Weight of input green in output red"),
            redFromBlue: ParameterText.Of("Red from blue", "Weight of input blue in output red"),
            greenFromRed: ParameterText.Of("Green from red", "Weight of input red in output green"),
            greenFromGreen: ParameterText.Of("Green from green", "Weight of input green in output green"),
            greenFromBlue: ParameterText.Of("Green from blue", "Weight of input blue in output green"),
            blueFromRed: ParameterText.Of("Blue from red", "Weight of input red in output blue"),
            blueFromGreen: ParameterText.Of("Blue from green", "Weight of input green in output blue"),
            blueFromBlue: ParameterText.Of("Blue from blue", "Weight of input blue in output blue")));
}

[Guid("556b0475-308d-4a3d-b366-f541d6666a00")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ColorWheels : StageEffect<ColorWheels, WheelsState, WheelsParameter, InvalidGrade>, IEffectDefinition<WheelsState, WheelsParameter> {
    public const string Caption = "Color wheels";

    public static EffectRow<WheelsState, WheelsParameter> Row { get; } = new(static parameter =>
        (ParameterText.Of("Hue", "Angle of the tint, zero on the positive horizontal axis growing counterclockwise"),
         ParameterText.Of("Strength", "Chroma of the tint"),
         ParameterText.Of("Luma", "Gain in stops")) switch {
             var (hue, strength, luma) => parameter.Map(
                 globalHue: hue, globalStrength: strength, globalLuma: luma,
                 shadowsHue: hue, shadowsStrength: strength, shadowsLuma: luma,
                 midtonesHue: hue, midtonesStrength: strength, midtonesLuma: luma,
                 highlightsHue: hue, highlightsStrength: strength, highlightsLuma: luma,
                 shadowsEnd: ParameterText.Of("Shadows end", "Working luminance where the shadows weight reaches zero"),
                 highlightsStart: ParameterText.Of("Highlights start", "Working luminance where the highlights weight begins"),
                 highlightsWidth: ParameterText.Of("Highlights width", "Luminance span over which the highlights weight reaches full")),
         });
}

[Guid("8e3a41fa-43cb-4d27-a488-922a226a0e8a")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ColorDecisionList : StageEffect<ColorDecisionList, CdlState, CdlParameter, InvalidGrade>, IEffectDefinition<CdlState, CdlParameter> {
    public const string Caption = "ASC CDL";

    public static EffectRow<CdlState, CdlParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            slopeRed: ParameterText.Of("Red slope", "Multiplier on red ahead of offset and power"),
            slopeGreen: ParameterText.Of("Green slope", "Multiplier on green ahead of offset and power"),
            slopeBlue: ParameterText.Of("Blue slope", "Multiplier on blue ahead of offset and power"),
            offsetRed: ParameterText.Of("Red offset", "Value added to red after slope"),
            offsetGreen: ParameterText.Of("Green offset", "Value added to green after slope"),
            offsetBlue: ParameterText.Of("Blue offset", "Value added to blue after slope"),
            powerRed: ParameterText.Of("Red power", "Exponent on red after offset"),
            powerGreen: ParameterText.Of("Green power", "Exponent on green after offset"),
            powerBlue: ParameterText.Of("Blue power", "Exponent on blue after offset"),
            saturation: ParameterText.Of("Saturation", "Distance from Rec.709 luma after power")));
}

[Guid("c75e00ae-77a6-4a7f-9556-a2aa0d59a883")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ContrastSaturation : StageEffect<ContrastSaturation, ContrastGrade, ContrastGradeParameter, InvalidGrade>, IEffectDefinition<ContrastGrade, ContrastGradeParameter> {
    public const string Caption = "Contrast and saturation";

    public static EffectRow<ContrastGrade, ContrastGradeParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            contrast: ParameterText.Of("Contrast", "Exponent about the pivot grey, one leaving the image unchanged"),
            pivot: ParameterText.Of("Pivot", "Grey the contrast turns about, in stops from middle grey"),
            vibrance: ParameterText.Of("Vibrance", "Saturation gain that falls as a pixel's saturation rises"),
            saturation: ParameterText.Of("Saturation", "Distance from working luminance, zero rendering grey")));
}

[Guid("485d8eb9-7e34-4c31-99c7-8c4aee2714dd")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class SceneLevels : StageEffect<SceneLevels, Levels, LevelsParameter, InvalidGrade>, IEffectDefinition<Levels, LevelsParameter> {
    public const string Caption = "Levels";

    public static EffectRow<Levels, LevelsParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            black: ParameterText.Of("Black", "Scene value mapped to zero, values below it passing unclamped"),
            white: ParameterText.Of("White", "Span above black mapped to one, in stops")));
}

[Guid("b10c1e89-0353-49ca-9e6e-5b9dc0c0c82c")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class RgbCurves : StageEffect<RgbCurves, PointCurves, PointCurveParameter, InvalidGrade>, IEffectDefinition<PointCurves, PointCurveParameter> {
    public const string Caption = "Curves";

    public static EffectRow<PointCurves, PointCurveParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            red: ParameterText.Of("Red", "Red channel curve over stops about middle grey"),
            green: ParameterText.Of("Green", "Green channel curve over stops about middle grey"),
            blue: ParameterText.Of("Blue", "Blue channel curve over stops about middle grey"),
            master: ParameterText.Of("Master", "Curve every channel takes after its own"),
            preserveHue: ParameterText.Of("Preserve hue", "Share of each pixel's hue the master curve keeps")));
}

[Guid("2f6b8d41-93c7-4e0a-b5d2-7a1c64e9f308")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ReferenceGamutCompression : StageEffect<ReferenceGamutCompression, GamutCompression, GamutCompressionParameter, InvalidColor>, IEffectDefinition<GamutCompression, GamutCompressionParameter> {
    public const string Caption = "Gamut compression";

    public static EffectRow<GamutCompression, GamutCompressionParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            target: ParameterText.Choice<Gamut, InvalidColor>("Gamut", "Primaries the compression measures distances in", "Working", ToneText.GamutText),
            cyanThreshold: ParameterText.Of("Cyan threshold", "Red distance from the achromatic axis where compression begins"),
            magentaThreshold: ParameterText.Of("Magenta threshold", "Green distance from the achromatic axis where compression begins"),
            yellowThreshold: ParameterText.Of("Yellow threshold", "Blue distance from the achromatic axis where compression begins"),
            cyanLimit: ParameterText.Of("Cyan limit", "Red distance compressed onto the gamut boundary"),
            magentaLimit: ParameterText.Of("Magenta limit", "Green distance compressed onto the gamut boundary"),
            yellowLimit: ParameterText.Of("Yellow limit", "Blue distance compressed onto the gamut boundary"),
            power: ParameterText.Of("Power", "Exponent of the compression curve")));
}

// --- [SECONDARIES] ---------------------------------------------------------------------
[Guid("c619d89b-47ee-4f89-bf6d-9a43711af308")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class HueCorrection : StageEffect<HueCorrection, HueCurves, HueCurveKind, InvalidGrade>, IEffectDefinition<HueCurves, HueCurveKind> {
    public const string Caption = "Hue curves";

    public static EffectRow<HueCurves, HueCurveKind> Row { get; } = new(static parameter =>
        parameter.Map(
            hueHue: ParameterText.Of("Hue vs hue", "Hue each input hue maps to"),
            hueSat: ParameterText.Of("Hue vs saturation", "Saturation gain by input hue"),
            hueLum: ParameterText.Of("Hue vs luma", "Luma gain by input hue"),
            lumSat: ParameterText.Of("Luma vs saturation", "Saturation gain by luma in stops"),
            satSat: ParameterText.Of("Saturation vs saturation", "Saturation each input saturation maps to"),
            lumLum: ParameterText.Of("Luma vs luma", "Luma in stops each input luma maps to"),
            satLum: ParameterText.Of("Saturation vs luma", "Luma gain by saturation")));
}

[Guid("e330c3e8-bff0-452a-87d7-fb23a5ec7fbe")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ColorQualifier : StageEffect<ColorQualifier, SelectiveColor, SelectiveColorParameter, InvalidGrade>, IEffectDefinition<SelectiveColor, SelectiveColorParameter> {
    public const string Caption = "Selective color";

    public static EffectRow<SelectiveColor, SelectiveColorParameter> Row { get; } = new(static parameter =>
        (ParameterText.Of("Center", "Middle of the qualified band"),
         ParameterText.Of("Width", "Span fully inside the band"),
         ParameterText.Of("Softness", "Falloff span outside each edge")) switch {
             var (center, width, softness) => parameter.Map(
                 hueCenter: center, hueWidth: width, hueSoftness: softness,
                 saturationCenter: center, saturationWidth: width, saturationSoftness: softness,
                 lumaCenter: center, lumaWidth: width, lumaSoftness: softness,
                 hueShift: ParameterText.Of("Hue shift", "Turn added to the hue of qualified pixels"),
                 saturation: ParameterText.Of("Saturation", "Saturation gain on qualified pixels"),
                 exposure: ParameterText.Of("Exposure", "Stops added to qualified pixels")),
         });
}

[Guid("7d1e5a92-c64b-4f38-9e07-b3a28c51d6f4")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ColorIsolation : StageEffect<ColorIsolation, HueIsolation, HueIsolationParameter, InvalidGrade>, IEffectDefinition<HueIsolation, HueIsolationParameter> {
    public const string Caption = "Color isolation";

    public static EffectRow<HueIsolation, HueIsolationParameter> Row { get; } = new(static parameter =>
        (ParameterText.Of("Center", "Middle of the band kept in color"),
         ParameterText.Of("Width", "Span fully inside the band"),
         ParameterText.Of("Softness", "Falloff span outside each edge")) switch {
             var (center, width, softness) => parameter.Map(
                 hueCenter: center, hueWidth: width, hueSoftness: softness,
                 saturationCenter: center, saturationWidth: width, saturationSoftness: softness,
                 lumaCenter: center, lumaWidth: width, lumaSoftness: softness),
         });
}

[Guid("b278e80b-5018-4dbc-9910-e477e852ff02")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class GradientMapping : StageEffect<GradientMapping, GradientMap, GradientMapParameter, InvalidGrade>, IEffectDefinition<GradientMap, GradientMapParameter> {
    public const string Caption = "Gradient map";

    public static EffectRow<GradientMap, GradientMapParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            ramp: ParameterText.Choice<RampInterpolation, InvalidPixelValue>("Gradient", "Colors mapped over working luminance", ToneText.InterpolationText)));
}

// --- [LOOKS] ---------------------------------------------------------------------------
[Guid("91b7f223-a7ab-4cec-aa60-99755bdd23d6")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class SceneLook : LookEffect<SceneLook> {
    public const string Caption = "Scene LUT";
}

[Guid("d8948345-e067-46c3-9e56-52229c540f9c")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class DisplayLook : LookEffect<DisplayLook> {
    public const string Caption = "Display LUT";
}
