using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [PRIMARIES] -----------------------------------------------------------------------
[Guid("c32e1536-d129-432d-b5ae-6da715024b02")]
[CustomPostEffect(PostEffectType.Early, "White Balance")]
public sealed class WhiteBalanceCorrection : StageEffect<WhiteBalanceCorrection, WhiteBalanceState, WhiteBalanceParameter>, IEffectDefinition<WhiteBalanceState, WhiteBalanceParameter> {
    public static EffectRow<WhiteBalanceState, WhiteBalanceParameter> Row { get; } = new() {
        Looks = toSeq(StandardIlluminant.Items).Map<(string Name, EffectLook<WhiteBalanceState, WhiteBalanceParameter> Look)>(static illuminant =>
            ($"Illuminant {illuminant.Key}", new WhiteBalanceState(illuminant.Temperature, illuminant.Tint))),
    };
}

[Guid("570b0a76-a94d-4749-b0a7-b9ec2e1fc36f")]
[CustomPostEffect(PostEffectType.Early, "Channel Mixer")]
public sealed class ChannelMixer : StageEffect<ChannelMixer, MixerState, MixerParameter>;

[Guid("556b0475-308d-4a3d-b366-f541d6666a00")]
[CustomPostEffect(PostEffectType.Early, "Color Wheels")]
public sealed class ColorWheels : StageEffect<ColorWheels, WheelsState, WheelsParameter>;

[Guid("8e3a41fa-43cb-4d27-a488-922a226a0e8a")]
[CustomPostEffect(PostEffectType.Early, "ASC CDL")]
public sealed class ColorDecisionList : StageEffect<ColorDecisionList, CdlState, CdlParameter>;

[Guid("c75e00ae-77a6-4a7f-9556-a2aa0d59a883")]
[CustomPostEffect(PostEffectType.Early, "Contrast and Saturation")]
public sealed class ContrastSaturation : StageEffect<ContrastSaturation, ContrastGrade, ContrastGradeParameter>;

[Guid("485d8eb9-7e34-4c31-99c7-8c4aee2714dd")]
[CustomPostEffect(PostEffectType.Early, "Levels")]
public sealed class SceneLevels : StageEffect<SceneLevels, Levels, LevelsParameter>;

[Guid("b10c1e89-0353-49ca-9e6e-5b9dc0c0c82c")]
[CustomPostEffect(PostEffectType.Early, "RGB Curves")]
public sealed class RgbCurves : StageEffect<RgbCurves, PointCurves, PointCurveParameter>;

[Guid("2f6b8d41-93c7-4e0a-b5d2-7a1c64e9f308")]
[CustomPostEffect(PostEffectType.Early, "Gamut Compression")]
public sealed class ReferenceGamutCompression : StageEffect<ReferenceGamutCompression, GamutCompression, GamutCompressionParameter>;

// --- [SECONDARIES] ---------------------------------------------------------------------
[Guid("c619d89b-47ee-4f89-bf6d-9a43711af308")]
[CustomPostEffect(PostEffectType.Early, "Hue Curves")]
public sealed class HueCorrection : StageEffect<HueCorrection, HueCurves, HueCurveKind>;

[Guid("e330c3e8-bff0-452a-87d7-fb23a5ec7fbe")]
[CustomPostEffect(PostEffectType.Early, "Selective Color")]
public sealed class ColorQualifier : StageEffect<ColorQualifier, SelectiveColor, SelectiveColorParameter>;

[Guid("7d1e5a92-c64b-4f38-9e07-b3a28c51d6f4")]
[CustomPostEffect(PostEffectType.Early, "Color Isolation")]
public sealed class ColorIsolation : StageEffect<ColorIsolation, HueIsolation, HueIsolationParameter>;

[Guid("b278e80b-5018-4dbc-9910-e477e852ff02")]
[CustomPostEffect(PostEffectType.Early, "Gradient Map")]
public sealed class GradientMapping : StageEffect<GradientMapping, GradientMap, GradientMapParameter>;

// --- [LOOKS] ---------------------------------------------------------------------------
[Guid("91b7f223-a7ab-4cec-aa60-99755bdd23d6")]
[CustomPostEffect(PostEffectType.Early, "Scene LUT")]
public sealed class SceneLook : StageEffect<SceneLook, LutLook, LutLookParameter>;

[Guid("d8948345-e067-46c3-9e56-52229c540f9c")]
[CustomPostEffect(PostEffectType.Late, "Display LUT")]
public sealed class DisplayLook : StageEffect<DisplayLook, LutLook, LutLookParameter>;
